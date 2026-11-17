// The design sheet's drawing kit (package UI-1, docs/specs/ui-refresh-plan.md sections 3.3 and 3.6, docs/port-notes/UI-1.md): a page that grows as
// it is drawn and is cropped when saved, and a painter that draws in device pixels at a scale with the tokens of one look (the light look, or a
// Windows contrast theme through the high-contrast mapping), snapping lines and corners as section 3.3 says. Off screen only: GDI+ into bitmaps.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using CodeBase;

namespace UiRefreshTests {
	///<summary>Draws with the tokens of one look at one scale. Coordinates are device pixels; S() turns a 96 DPI length into them.</summary>
	public class SheetPainter : IDisposable {
		public Graphics G;
		public float Scale;
		///<summary>The token's colour in this look.</summary>
		public Func<EnumUiColor,Color> FuncColor;
		///<summary>Body text (Segoe UI 9 pt at the scale), its bold, the self-drawn title (Segoe UI Semibold 10), the module caption (8), and the
		///sheet's own headings and notes.</summary>
		public Font FontBody;
		public Font FontBold;
		public Font FontTitle;
		public Font FontCaption;
		public Font FontHeading;
		public Font FontNote;

		public SheetPainter(Graphics g,float scale,Func<EnumUiColor,Color> funcColor) {
			G=g;
			//Pixel centres at .5, so fills on whole pixels and 1 px lines on .5 are sharp (UiTheme.GetStrokeRectangle).
			G.PixelOffsetMode=PixelOffsetMode.Half;
			Scale=scale;
			FuncColor=funcColor;
			FontBody=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY*scale);
			FontBold=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY*scale,FontStyle.Bold);
			FontTitle=new Font(UiTheme.FONT_FAMILY_SEMIBOLD,UiTheme.FONT_SIZE_TITLE*scale);
			FontCaption=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_MODULE_CAPTION*scale);
			FontHeading=new Font(UiTheme.FONT_FAMILY_SEMIBOLD,13f*scale);
			FontNote=new Font(UiTheme.FONT_FAMILY,8f*scale);
		}

		public static Color Light(EnumUiColor uiColor) {
			return UiTheme.GetColorLight(uiColor);
		}

		public int S(int value96) {
			return UiTheme.ScaleI(value96,Scale);
		}

		public Color C(EnumUiColor uiColor) {
			return FuncColor(uiColor);
		}

		public int LineWidth {
			get { return UiTheme.GetLineWidth(Scale); }
		}

		public int FocusWidth {
			get { return UiTheme.GetFocusWidth(Scale); }
		}

		#region Shapes
		///<summary>Fills the rectangle, with corners of radius96 at 96 DPI (snapped).</summary>
		public void Fill(Color color,Rectangle rectangle,int radius96) {
			int radius=UiTheme.GetCornerRadius(radius96,Scale);
			using(SolidBrush brush=new SolidBrush(color)) {
				if(radius==0) {
					G.SmoothingMode=SmoothingMode.None;
					G.FillRectangle(brush,rectangle);
					return;
				}
				G.SmoothingMode=SmoothingMode.AntiAlias;
				using(GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(rectangle,radius)) {
					G.FillPath(brush,graphicsPath);
				}
			}
		}

		public void Fill(EnumUiColor uiColor,Rectangle rectangle,int radius96) {
			Fill(C(uiColor),rectangle,radius96);
		}

		///<summary>An outline width device pixels wide, inside the rectangle, on whole pixels (section 3.3).</summary>
		public void Outline(Color color,Rectangle rectangle,int width,int radius96) {
			int radius=UiTheme.GetCornerRadius(radius96,Scale);
			using(Pen pen=new Pen(color,width)) {
				RectangleF rectangleStroke=UiTheme.GetStrokeRectangle(rectangle,width);
				if(radius==0) {
					G.SmoothingMode=SmoothingMode.None;
					//Without antialiasing GDI+ draws a 1 px pen at the top-left of the pixel: fill the edges instead, exactly inside.
					using(SolidBrush brush=new SolidBrush(color)) {
						G.FillRectangle(brush,rectangle.X,rectangle.Y,rectangle.Width,width);
						G.FillRectangle(brush,rectangle.X,rectangle.Bottom-width,rectangle.Width,width);
						G.FillRectangle(brush,rectangle.X,rectangle.Y,width,rectangle.Height);
						G.FillRectangle(brush,rectangle.Right-width,rectangle.Y,width,rectangle.Height);
					}
					return;
				}
				G.SmoothingMode=SmoothingMode.AntiAlias;
				using(GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(rectangleStroke,Math.Max(0,radius-width/2f))) {
					G.DrawPath(pen,graphicsPath);
				}
			}
		}

		public void Outline(EnumUiColor uiColor,Rectangle rectangle,int width,int radius96) {
			Outline(C(uiColor),rectangle,width,radius96);
		}

		///<summary>The 2 px Accent focus ring inside the bounds, following the corners (section 3.6).</summary>
		public void FocusRing(Rectangle rectangle,int radius96) {
			Outline(EnumUiColor.Accent,rectangle,FocusWidth,radius96);
		}

		///<summary>The focus of an accent-filled control: the Accent ring outside a 1 px white inner ring (section 3.6).</summary>
		public void FocusRingOnAccent(Rectangle rectangle,int radius96) {
			Outline(EnumUiColor.Accent,rectangle,FocusWidth,radius96);
			Rectangle rectangleInner=Rectangle.Inflate(rectangle,-FocusWidth,-FocusWidth);
			Outline(EnumUiColor.TextOnAccent,rectangleInner,LineWidth,Math.Max(0,radius96-2));
		}

		public void Line(EnumUiColor uiColor,int x1,int y1,int x2,int y2,int width) {
			using(SolidBrush brush=new SolidBrush(C(uiColor))) {
				G.SmoothingMode=SmoothingMode.None;
				if(y1==y2) {
					G.FillRectangle(brush,Math.Min(x1,x2),y1,Math.Abs(x2-x1),width);
				}
				else {
					G.FillRectangle(brush,x1,Math.Min(y1,y2),width,Math.Abs(y2-y1));
				}
			}
		}
		#endregion Shapes

		#region Text
		///<summary>Text with GDI+ and ClearType, whatever the session's own text smoothing is (a disconnected session draws Windows' own GDI text
		///without smoothing, which is not how it looks on a desktop). TextFormatFlags give the alignment, wrapping and ellipsis.</summary>
		public void Text(string text,Font font,Color color,Rectangle rectangle,TextFormatFlags textFormatFlags) {
			using(StringFormat stringFormat=GetStringFormat(textFormatFlags))
			using(SolidBrush brush=new SolidBrush(color)) {
				G.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
				G.DrawString(text,font,brush,rectangle,stringFormat);
			}
		}

		private static StringFormat GetStringFormat(TextFormatFlags textFormatFlags) {
			StringFormat stringFormat=new StringFormat();
			stringFormat.HotkeyPrefix=System.Drawing.Text.HotkeyPrefix.None;
			stringFormat.Alignment=(textFormatFlags & TextFormatFlags.HorizontalCenter)!=0 ? StringAlignment.Center
				: ((textFormatFlags & TextFormatFlags.Right)!=0 ? StringAlignment.Far : StringAlignment.Near);
			stringFormat.LineAlignment=(textFormatFlags & TextFormatFlags.VerticalCenter)!=0 ? StringAlignment.Center
				: ((textFormatFlags & TextFormatFlags.Bottom)!=0 ? StringAlignment.Far : StringAlignment.Near);
			if((textFormatFlags & TextFormatFlags.SingleLine)!=0) {
				stringFormat.FormatFlags|=StringFormatFlags.NoWrap;
			}
			stringFormat.Trimming=(textFormatFlags & TextFormatFlags.EndEllipsis)!=0 ? StringTrimming.EllipsisCharacter : StringTrimming.None;
			return stringFormat;
		}

		public void Text(string text,Font font,EnumUiColor uiColor,Rectangle rectangle,TextFormatFlags textFormatFlags) {
			Text(text,font,C(uiColor),rectangle,textFormatFlags);
		}

		///<summary>Text in one line, vertically centred, left-aligned.</summary>
		public void TextLeft(string text,Font font,EnumUiColor uiColor,Rectangle rectangle) {
			Text(text,font,uiColor,rectangle,TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis);
		}

		public void TextCenter(string text,Font font,EnumUiColor uiColor,Rectangle rectangle) {
			Text(text,font,uiColor,rectangle,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine);
		}

		///<summary>The width of one line of text as Text draws it, in device pixels.</summary>
		public int MeasureWidth(string text,Font font) {
			using(StringFormat stringFormat=GetStringFormat(TextFormatFlags.SingleLine)) {
				G.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
				return (int)Math.Ceiling(G.MeasureString(text,font,int.MaxValue,stringFormat).Width);
			}
		}

		///<summary>Wrapped text from the top of the rectangle; returns the height it took.</summary>
		public int TextWrapped(string text,Font font,Color color,Rectangle rectangle) {
			using(StringFormat stringFormat=GetStringFormat(TextFormatFlags.Left|TextFormatFlags.Top|TextFormatFlags.WordBreak)) {
				G.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
				SizeF sizeF=G.MeasureString(text,font,rectangle.Width,stringFormat);
				int height=(int)Math.Ceiling(sizeF.Height);
				Text(text,font,color,new Rectangle(rectangle.X,rectangle.Y,rectangle.Width,height+2),TextFormatFlags.Left|TextFormatFlags.Top|TextFormatFlags.WordBreak);
				return height;
			}
		}
		#endregion Text

		#region Glyphs (stand-ins until the Fluent icons of UI-7a)
		///<summary>A down chevron in a square of size96 (section 3.6's drop-down arrow, 12 px).</summary>
		public void Chevron(Color color,Rectangle rectangle) {
			int size=Math.Min(rectangle.Width,rectangle.Height);
			float x=rectangle.X+(rectangle.Width-size)/2f;
			float y=rectangle.Y+(rectangle.Height-size)/2f;
			using(Pen pen=new Pen(color,Math.Max(1f,1.25f*Scale))) {
				G.SmoothingMode=SmoothingMode.AntiAlias;
				G.DrawLines(pen,new PointF[] { new PointF(x+size*0.2f,y+size*0.38f),new PointF(x+size*0.5f,y+size*0.66f),new PointF(x+size*0.8f,y+size*0.38f) });
			}
		}

		public void CheckMark(Color color,Rectangle rectangle) {
			using(Pen pen=new Pen(color,Math.Max(1.2f,1.5f*Scale))) {
				pen.StartCap=LineCap.Round;
				pen.EndCap=LineCap.Round;
				pen.LineJoin=LineJoin.Round;
				G.SmoothingMode=SmoothingMode.AntiAlias;
				float x=rectangle.X;
				float y=rectangle.Y;
				float w=rectangle.Width;
				float h=rectangle.Height;
				G.DrawLines(pen,new PointF[] { new PointF(x+w*0.2f,y+h*0.52f),new PointF(x+w*0.42f,y+h*0.74f),new PointF(x+w*0.8f,y+h*0.28f) });
			}
		}

		public void Dash(Color color,Rectangle rectangle) {
			int height=Math.Max(1,S(2));
			using(SolidBrush brush=new SolidBrush(color)) {
				G.SmoothingMode=SmoothingMode.None;
				G.FillRectangle(brush,rectangle.X+rectangle.Width/4,rectangle.Y+(rectangle.Height-height)/2,rectangle.Width-rectangle.Width/2,height);
			}
		}

		///<summary>Caption glyphs: 0 minimise, 1 maximise, 2 close, in a 10 px square at 96 DPI centred in the rectangle.</summary>
		public void CaptionGlyph(int kind,Color color,Rectangle rectangle) {
			int size=S(10);
			int x=rectangle.X+(rectangle.Width-size)/2;
			int y=rectangle.Y+(rectangle.Height-size)/2;
			int width=LineWidth;
			using(Pen pen=new Pen(color,width)) {
				if(kind==0) {
					G.SmoothingMode=SmoothingMode.None;
					using(SolidBrush brush=new SolidBrush(color)) {
						G.FillRectangle(brush,x,y+size/2,size,width);
					}
				}
				else if(kind==1) {
					Outline(color,new Rectangle(x,y,size,size),width,0);
				}
				else {
					G.SmoothingMode=SmoothingMode.AntiAlias;
					G.DrawLine(pen,x+0.5f,y+0.5f,x+size-0.5f,y+size-0.5f);
					G.DrawLine(pen,x+size-0.5f,y+0.5f,x+0.5f,y+size-0.5f);
				}
			}
		}

		///<summary>A stand-in icon shape for the role colours of UQ-4: 0 delete (X), 1 complete (check), 2 warning (triangle), 3 main action (plus),
		///4 a plain one (a page).</summary>
		public void RoleIcon(int kind,Color color,Rectangle rectangle) {
			float x=rectangle.X;
			float y=rectangle.Y;
			float w=rectangle.Width;
			using(Pen pen=new Pen(color,Math.Max(1.2f,1.4f*Scale))) {
				pen.StartCap=LineCap.Round;
				pen.EndCap=LineCap.Round;
				pen.LineJoin=LineJoin.Round;
				G.SmoothingMode=SmoothingMode.AntiAlias;
				switch(kind) {
					case 0:
						G.DrawLine(pen,x+w*0.25f,y+w*0.25f,x+w*0.75f,y+w*0.75f);
						G.DrawLine(pen,x+w*0.75f,y+w*0.25f,x+w*0.25f,y+w*0.75f);
						break;
					case 1:
						G.DrawEllipse(pen,x+w*0.1f,y+w*0.1f,w*0.8f,w*0.8f);
						G.DrawLines(pen,new PointF[] { new PointF(x+w*0.3f,y+w*0.52f),new PointF(x+w*0.45f,y+w*0.66f),new PointF(x+w*0.72f,y+w*0.36f) });
						break;
					case 2:
						G.DrawPolygon(pen,new PointF[] { new PointF(x+w*0.5f,y+w*0.12f),new PointF(x+w*0.9f,y+w*0.85f),new PointF(x+w*0.1f,y+w*0.85f) });
						G.DrawLine(pen,x+w*0.5f,y+w*0.4f,x+w*0.5f,y+w*0.6f);
						G.DrawLine(pen,x+w*0.5f,y+w*0.72f,x+w*0.5f,y+w*0.73f);
						break;
					case 3:
						G.DrawLine(pen,x+w*0.5f,y+w*0.2f,x+w*0.5f,y+w*0.8f);
						G.DrawLine(pen,x+w*0.2f,y+w*0.5f,x+w*0.8f,y+w*0.5f);
						break;
					default:
						G.DrawLines(pen,new PointF[] { new PointF(x+w*0.25f,y+w*0.12f),new PointF(x+w*0.6f,y+w*0.12f),new PointF(x+w*0.78f,y+w*0.3f),
							new PointF(x+w*0.78f,y+w*0.88f),new PointF(x+w*0.25f,y+w*0.88f),new PointF(x+w*0.25f,y+w*0.12f) });
						break;
				}
			}
		}
		#endregion Glyphs (stand-ins until the Fluent icons of UI-7a)

		public void Dispose() {
			FontBody.Dispose();
			FontBold.Dispose();
			FontTitle.Dispose();
			FontCaption.Dispose();
			FontHeading.Dispose();
			FontNote.Dispose();
		}
	}

	///<summary>A page of the sheet: a tall bitmap filled with the window surface, drawn top to bottom, cropped to what was drawn when saved.</summary>
	public class SheetPage : IDisposable {
		public Bitmap Bitmap;
		public Graphics G;
		public SheetPainter Painter;
		///<summary>The next free line, in device pixels.</summary>
		public int Y;
		public int Width;
		public int Margin;

		public SheetPage(int width96,int heightMax96,float scale,Func<EnumUiColor,Color> funcColor) {
			Width=UiTheme.ScaleI(width96,scale);
			Bitmap=new Bitmap(Width,UiTheme.ScaleI(heightMax96,scale),PixelFormat.Format24bppRgb);
			G=Graphics.FromImage(Bitmap);
			G.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			Painter=new SheetPainter(G,scale,funcColor);
			Margin=Painter.S(16);
			Y=Margin;
			using(SolidBrush brush=new SolidBrush(SheetPainter.Light(EnumUiColor.Surface))) {
				G.FillRectangle(brush,0,0,Bitmap.Width,Bitmap.Height);
			}
		}

		public int S(int value96) {
			return Painter.S(value96);
		}

		///<summary>A heading in the sheet's own style, then a line of space.</summary>
		public void Heading(string text) {
			int height=S(26);
			Painter.Text(text,Painter.FontHeading,SheetPainter.Light(EnumUiColor.Text),new Rectangle(Margin,Y,Width-2*Margin,height),
				TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine);
			Y+=height+S(4);
		}

		///<summary>A paragraph in TextSecondary (the sheet's notes).</summary>
		public void Note(string text) {
			Y+=Painter.TextWrapped(text,Painter.FontBody,SheetPainter.Light(EnumUiColor.TextSecondary),new Rectangle(Margin,Y,Width-2*Margin,0))+S(8);
		}

		public void Space(int value96) {
			Y+=S(value96);
		}

		///<summary>Saves the page cropped to what was drawn, as PNG.</summary>
		public string Save(string path) {
			int height=Math.Min(Bitmap.Height,Y+Margin);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			using(Bitmap bitmapCrop=Bitmap.Clone(new Rectangle(0,0,Bitmap.Width,height),PixelFormat.Format24bppRgb)) {
				bitmapCrop.Save(path,ImageFormat.Png);
			}
			return path;
		}

		public void Dispose() {
			Painter.Dispose();
			G.Dispose();
			Bitmap.Dispose();
		}
	}
}
