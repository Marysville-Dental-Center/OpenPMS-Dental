// The GDI+ part of the icon renderer (docs/specs/ui-refresh-plan.md, section 4.2, items 3, 5 and 6; package UI-7a): a design as a GraphicsPath,
// filled with antialiasing, pixel centres at +0.5 as in the SVG (PixelOffsetMode.Half), fill rule nonzero (FillMode.Winding); GetBitmap for the
// places that need a bitmap. This is also the OnlyGDI path of the program (NoD2D.txt, IconLibrary.OnlyGDI): OpenPmsDental.UI.Direct2d.DrawGlyph
// falls back to it. System and System.Drawing only, C# 7.3: RdlDesign can compile this file in as a linked file (UI-9a-7).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CodeBase {
	public static partial class UiIcons {
		///<summary>This thread's paths, by design and drawn size, at (0,0). GDI+ objects are not thread-safe, so each thread has its own (as
		///UiTheme's pens). Filled on first use; never handed out.</summary>
		[ThreadStatic]
		private static Dictionary<PathKey,GraphicsPath> _dictPaths;
		///<summary>A thread's cache is emptied when it holds more paths than this (every glyph at every size is far fewer).</summary>
		private const int PATHS_CACHED_MAX=1024;

		///<summary>Fills the glyph's design for the slot (GetDesign, GetDrawRectangle) in the colour. The slot is in the Graphics' units, normally
		///device pixels. Nothing for None or an empty slot.</summary>
		public static void Draw(Graphics g,UiGlyph uiGlyph,Rectangle rectangleSlot,Color color) {
			Draw(g,GetDesign(uiGlyph,Math.Min(rectangleSlot.Width,rectangleSlot.Height)),rectangleSlot,color);
		}

		///<summary>Fills the glyph in the colour of a token (UiTheme.GetBrush: the token's current colour, the high-contrast partner included).</summary>
		public static void Draw(Graphics g,UiGlyph uiGlyph,Rectangle rectangleSlot,EnumUiColor uiColor) {
			Draw(g,uiGlyph,rectangleSlot,UiTheme.GetColor(uiColor));
		}

		///<summary>Fills the glyph as disabled: in TextDisabled, not greyed with ControlPaint (section 4.2, item 6).</summary>
		public static void DrawDisabled(Graphics g,UiGlyph uiGlyph,Rectangle rectangleSlot) {
			Draw(g,uiGlyph,rectangleSlot,EnumUiColor.TextDisabled);
		}

		///<summary>Fills a design, placed in the slot by GetDrawRectangle, in the colour. The Graphics' own settings are restored afterwards.</summary>
		public static void Draw(Graphics g,IconDesign iconDesign,Rectangle rectangleSlot,Color color) {
			if(iconDesign==null || rectangleSlot.Width<=0 || rectangleSlot.Height<=0) {
				return;
			}
			Rectangle rectangleDraw=GetDrawRectangle(iconDesign,rectangleSlot);
			GraphicsPath graphicsPath=GetPathCached(iconDesign,rectangleDraw.Width);
			GraphicsState graphicsState=g.Save();
			try {
				g.SmoothingMode=SmoothingMode.AntiAlias;
				g.PixelOffsetMode=PixelOffsetMode.Half;
				g.TranslateTransform(rectangleDraw.X,rectangleDraw.Y);
				using(SolidBrush solidBrush=new SolidBrush(color)) {
					g.FillPath(solidBrush,graphicsPath);
				}
			}
			finally {
				g.Restore(graphicsState);
			}
		}

		///<summary>A new GraphicsPath of the glyph's design for the slot, in the slot's coordinates, fill mode winding. The caller disposes it (for a
		///clip or a hit test). An empty path for None.</summary>
		public static GraphicsPath GetPath(UiGlyph uiGlyph,Rectangle rectangleSlot) {
			GraphicsPath graphicsPath=new GraphicsPath(FillMode.Winding);
			WriteFigures(new GdiPathSink(graphicsPath),uiGlyph,rectangleSlot);
			return graphicsPath;
		}

		///<summary>A new 32-bit ARGB bitmap of sizePixels square (96 DPI in its header, so it draws 1:1) with the glyph drawn in the colour on a
		///transparent background (section 4.2, item 5: ImageLists, TreeViews, ListViews and designer Image properties). The caller owns it. Pass
		///the slot's real pixel size (after DPI scaling), or use the overload that scales.</summary>
		public static Bitmap GetBitmap(UiGlyph uiGlyph,int sizePixels,Color color) {
			return GetBitmap(GetDesign(uiGlyph,sizePixels),sizePixels,color);
		}

		///<summary>GetBitmap for a slot of size96 at 96 DPI, scaled (UiTheme.ScaleI: rounded as LayoutManager rounds).</summary>
		public static Bitmap GetBitmap(UiGlyph uiGlyph,int size96,float scale,Color color) {
			return GetBitmap(uiGlyph,UiTheme.ScaleI(size96,scale),color);
		}

		///<summary>GetBitmap in the colour of a token.</summary>
		public static Bitmap GetBitmap(UiGlyph uiGlyph,int sizePixels,EnumUiColor uiColor) {
			return GetBitmap(uiGlyph,sizePixels,UiTheme.GetColor(uiColor));
		}

		///<summary>A new bitmap of a design (GetBitmap). A transparent bitmap for null. sizePixels below 1 is taken as 1.</summary>
		public static Bitmap GetBitmap(IconDesign iconDesign,int sizePixels,Color color) {
			sizePixels=Math.Max(sizePixels,1);
			Bitmap bitmap=new Bitmap(sizePixels,sizePixels,PixelFormat.Format32bppArgb);
			bitmap.SetResolution(96,96);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.Transparent);
				Draw(g,iconDesign,new Rectangle(0,0,sizePixels,sizePixels),color);
			}
			return bitmap;
		}

		///<summary>A new 32-bit ARGB bitmap of width x height at 96 DPI with the icon of a manifest row (enum and member name, as
		///GetDesigns(string,string)) in its role colour, in the largest square that fits and centred (section 3.5: the largest design that fits,
		///never stretched up), on a transparent background. For the projects that cannot reach OpenPmsDental.UI.IconLibrary (OpenPmsDentalGraph,
		///OpenPmsDentalBusiness): a picture of today's size is replaced and nothing moves (UI-9a, docs/port-notes/UI-9a.md). A transparent bitmap
		///when the row has no design. The caller owns it.</summary>
		public static Bitmap GetBitmap(string enumName,string name,int width,int height) {
			width=Math.Max(width,1);
			height=Math.Max(height,1);
			Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb);
			bitmap.SetResolution(96,96);
			int sizeSlot=Math.Min(width,height);
			IconDesign iconDesign=GetDesign(GetDesigns(enumName,name),sizeSlot);
			if(iconDesign==null) {
				return bitmap;
			}
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.Transparent);
				Draw(g,iconDesign,new Rectangle((width-sizeSlot)/2,(height-sizeSlot)/2,sizeSlot,sizeSlot),GetRoleColor(iconDesign));
			}
			return bitmap;
		}

		///<summary>This thread's path of the design in a box of the drawn size at (0,0), placed by GetPlacement.</summary>
		private static GraphicsPath GetPathCached(IconDesign iconDesign,int sizeDraw) {
			if(_dictPaths==null) {
				_dictPaths=new Dictionary<PathKey,GraphicsPath>();
			}
			PathKey pathKey=new PathKey(iconDesign,sizeDraw);
			GraphicsPath graphicsPath;
			if(_dictPaths.TryGetValue(pathKey,out graphicsPath)) {
				return graphicsPath;
			}
			if(_dictPaths.Count>=PATHS_CACHED_MAX) {
				foreach(GraphicsPath graphicsPathOld in _dictPaths.Values) {
					graphicsPathOld.Dispose();
				}
				_dictPaths.Clear();
			}
			graphicsPath=new GraphicsPath(FillMode.Winding);
			GetPlacement(iconDesign,sizeDraw,out float offset,out float scale);
			iconDesign.GetFigures().WriteTo(new GdiPathSink(graphicsPath),offset,offset,scale);
			_dictPaths.Add(pathKey,graphicsPath);
			return graphicsPath;
		}

		///<summary>The key of a cached path: the design (by reference: the table's designs live as long as the program) and the drawn size.</summary>
		private struct PathKey:IEquatable<PathKey> {
			private readonly IconDesign _iconDesign;
			private readonly int _sizeDraw;

			public PathKey(IconDesign iconDesign,int sizeDraw) {
				_iconDesign=iconDesign;
				_sizeDraw=sizeDraw;
			}

			public bool Equals(PathKey other) {
				return ReferenceEquals(_iconDesign,other._iconDesign) && _sizeDraw==other._sizeDraw;
			}

			public override bool Equals(object obj) {
				return obj is PathKey && Equals((PathKey)obj);
			}

			public override int GetHashCode() {
				return _iconDesign.GetHashCode()*397^_sizeDraw;
			}
		}

		///<summary>Writes a design's figures into a GraphicsPath.</summary>
		private sealed class GdiPathSink:IIconPathSink {
			private readonly GraphicsPath _graphicsPath;
			private PointF _pointCurrent;

			public GdiPathSink(GraphicsPath graphicsPath) {
				_graphicsPath=graphicsPath;
			}

			public void BeginFigure(float x,float y) {
				_graphicsPath.StartFigure();
				_pointCurrent=new PointF(x,y);
			}

			public void AddLine(float x,float y) {
				PointF point=new PointF(x,y);
				_graphicsPath.AddLine(_pointCurrent,point);
				_pointCurrent=point;
			}

			public void AddBezier(float x1,float y1,float x2,float y2,float x,float y) {
				PointF point=new PointF(x,y);
				_graphicsPath.AddBezier(_pointCurrent,new PointF(x1,y1),new PointF(x2,y2),point);
				_pointCurrent=point;
			}

			public void EndFigure() {
				_graphicsPath.CloseFigure();
			}
		}
	}
}
