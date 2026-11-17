// The text-fit measurer of the UI refresh (package UI-0b, docs/specs/ui-refresh-plan.md sections 5.4 and 7, docs/port-notes/UI-0b.md): does a
// control's text fit the room the control gives it, measured the way the control draws it. TextRenderer (GDI) for the stock Label (and
// LinkLabel), RadioButton, CheckBox, Button and GroupBox; Graphics.MeasureString (GDI+) for the program's UI.Button, UI.CheckBox and
// UI.GroupBox, whose OnPaint draws with DrawString (Button.cs DrawTextAndImage, CheckBox.cs OnPaint, GroupBox.cs OnPaint). A control is measured
// with the font it has: under LayoutManager that is the font LayoutManagerForms gave it, so a Label or RadioButton already carries the 0.96
// factor (LayoutManagerForms.LayoutChildren); FontSizeUnderLayoutManager gives the factor for a font that has not been through it. For the
// 1-2 px height cases the text is drawn the control's way into a bitmap, unclipped, and ink past the control's text box (its last row or
// column and beyond) means the text is visibly cut (the ink check).
// Shared: tests\WindowChecksTests compiles this file in as a linked file (UI-0c), so it uses only Windows Forms, System.Drawing and reflection,
// and finds the program's controls by type name.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UiRefreshTests.TextFit {
	///<summary>How a control's text is measured.</summary>
	public enum TextFitMethod {
		///<summary>Not measured: no text, or a control type the scan does not measure.</summary>
		None,
		///<summary>TextRenderer (GDI), as the stock controls draw.</summary>
		TextRenderer,
		///<summary>Graphics.MeasureString (GDI+), as the program's self-drawn controls draw.</summary>
		MeasureString,
	}

	///<summary>The result of one measurement, in the words of section 5.3 of the plan.</summary>
	public enum TextFitKind {
		///<summary>The text fits (on one line, or wrapped inside the control).</summary>
		Fits,
		///<summary>The text fits the width on one line, but the line is taller than the room (most by 1-2 px).</summary>
		Height,
		///<summary>The text needs more width: one line is all the control has room for, or it cannot wrap.</summary>
		Width,
		///<summary>The text wraps, and the wrapped lines need more height than the control has (another line).</summary>
		Line,
	}

	///<summary>One control's measurement.</summary>
	public class TextFitResult {
		public TextFitMethod Method;
		public TextFitKind Kind;
		///<summary>Pixels of width missing (0 when the width fits).</summary>
		public int NeedWidth;
		///<summary>Pixels of height missing (0 when the height fits).</summary>
		public int NeedHeight;
		///<summary>The room for the text, in pixels.</summary>
		public Size SizeAvailable;
		///<summary>The text on one line, in pixels.</summary>
		public Size SizeLine;
		///<summary>True when the ink check ran (a Height result of 1-2 px).</summary>
		public bool IsInkChecked;
		///<summary>True when the ink check found ink past the text box: the cut shows. Meaningful only when IsInkChecked.</summary>
		public bool IsInkClipped;

		///<summary>True for any result but Fits.</summary>
		public bool IsOverflow {
			get { return Kind!=TextFitKind.Fits; }
		}

		///<summary>Fits, or a 1-2 px height case whose ink stays inside: nothing shows cut.</summary>
		public bool IsVisiblyCut {
			get {
				if(Kind==TextFitKind.Fits) {
					return false;
				}
				if(IsInkChecked) {
					return IsInkClipped;
				}
				return true;
			}
		}
	}

	public static class TextFitMeasurer {
		///<summary>LayoutManagerForms multiplies the font of a Label or RadioButton by this (LayoutManagerForms.LayoutChildren).</summary>
		public const float LABEL_FACTOR=0.96f;
		///<summary>The largest height shortfall the ink check looks at (section 5.4: the 1-2 px height cases).</summary>
		public const int INK_CHECK_MAX_HEIGHT=2;
		///<summary>UI.CheckBox's box at 96 dpi (CheckBox._sqrSize96).</summary>
		public const int UI_CHECKBOX_BOX_96=12;
		///<summary>The type names of the program's self-drawn controls that draw their text with DrawString.</summary>
		public const string TYPE_UI_BUTTON="OpenPmsDental.UI.Button";
		public const string TYPE_UI_CHECKBOX="OpenPmsDental.UI.CheckBox";
		public const string TYPE_UI_GROUPBOX="OpenPmsDental.UI.GroupBox";
		[ThreadStatic]
		private static Bitmap _bitmapMeasure;
		[ThreadStatic]
		private static Graphics _graphicsMeasure;

		///<summary>A 96 dpi Graphics of this thread for measuring (GDI+ objects are not shared between threads).</summary>
		public static Graphics GraphicsMeasure {
			get {
				if(_graphicsMeasure==null) {
					_bitmapMeasure=new Bitmap(1,1,PixelFormat.Format32bppArgb);
					_bitmapMeasure.SetResolution(96,96);
					_graphicsMeasure=Graphics.FromImage(_bitmapMeasure);
				}
				return _graphicsMeasure;
			}
		}

		///<summary>True when control is of the type, or derives from it (by full name, so the program need not be referenced).</summary>
		public static bool IsOfType(Control control,string typeFullName) {
			for(Type type=control.GetType();type!=null;type=type.BaseType) {
				if(type.FullName==typeFullName) {
					return true;
				}
			}
			return false;
		}

		///<summary>How the control's text is measured, by its type (section 5.4). The program's controls first: UI.Button is a Button.</summary>
		public static TextFitMethod GetMethod(Control control) {
			if(IsOfType(control,TYPE_UI_BUTTON) || IsOfType(control,TYPE_UI_CHECKBOX) || IsOfType(control,TYPE_UI_GROUPBOX)) {
				return TextFitMethod.MeasureString;
			}
			if(control is Label || control is RadioButton || control is System.Windows.Forms.CheckBox || control is System.Windows.Forms.Button
				|| control is System.Windows.Forms.GroupBox)
			{
				return TextFitMethod.TextRenderer;
			}
			return TextFitMethod.None;
		}

		///<summary>The controls whose font LayoutManagerForms multiplies by LABEL_FACTOR: a Label (LinkLabel included) or a RadioButton.</summary>
		public static bool IsLabelFactorType(Control control) {
			return control is Label || control is RadioButton;
		}

		///<summary>The font size LayoutManagerForms gives the control at 100% for a recorded size: × 0.96 for a Label or RadioButton.</summary>
		public static float FontSizeUnderLayoutManager(Control control,float fontSize96) {
			if(IsLabelFactorType(control)) {
				return fontSize96*LABEL_FACTOR;
			}
			return fontSize96;
		}

		///<summary>The controls whose font LayoutManagerForms scales by the whole scale (Windows' and the zoom) instead of the zoom only
		///(LayoutManagerForms.LayoutChildren and AddToList96).</summary>
		public static bool IsFullyScaledFontType(Control control) {
			return control is System.Windows.Forms.TextBox || control is System.Windows.Forms.ComboBox || control is RichTextBox
				|| control is TreeView || control is ListView || control is System.Windows.Forms.ListBox;
		}

		///<summary>The font to measure with at a simulated Windows scale (1.5 = 150%). In a 96 dpi process LayoutManagerForms leaves most fonts at
		///their zoom size and Windows would draw them scaleMS times larger; the fully scaled types already have the whole scale. At 1 the
		///control's own font is returned (not a copy); otherwise a new font the caller disposes.</summary>
		public static Font CreateMeasuringFont(Control control,float scaleMS) {
			if(scaleMS==1f || IsFullyScaledFontType(control)) {
				return control.Font;
			}
			return new Font(control.Font.FontFamily,control.Font.Size*scaleMS,control.Font.Style,GraphicsUnit.Point);
		}

		///<summary>Measures the control's text at the simulated Windows scale (1 at 100%). Returns a result with Method None for a control that is
		///not measured. The control's size is its size now (built and laid out by its window at that scale).</summary>
		public static TextFitResult Measure(Control control,float scaleMS) {
			TextFitResult result=new TextFitResult();
			result.Method=GetMethod(control);
			if(result.Method==TextFitMethod.None || string.IsNullOrWhiteSpace(control.Text)) {
				result.Method=TextFitMethod.None;
				return result;
			}
			Font font=CreateMeasuringFont(control,scaleMS);
			try {
				if(result.Method==TextFitMethod.MeasureString) {
					MeasureProgramControl(control,font,scaleMS,result);
				}
				else {
					MeasureStockControl(control,font,scaleMS,result);
				}
			}
			finally {
				if(!ReferenceEquals(font,control.Font)) {
					font.Dispose();
				}
			}
			return result;
		}

		#region Stock controls (TextRenderer)
		///<summary>The flags a stock control draws its text with: its alignment, word wrap (labels and buttons wrap), and the mnemonic rule.</summary>
		public static TextFormatFlags GetStockFlags(Control control) {
			ContentAlignment contentAlignment=ContentAlignment.TopLeft;
			bool useMnemonic=true;
			if(control is Label label) {
				contentAlignment=label.TextAlign;
				useMnemonic=label.UseMnemonic;
			}
			else if(control is ButtonBase buttonBase) {
				contentAlignment=buttonBase.TextAlign;
				useMnemonic=buttonBase.UseMnemonic;
			}
			TextFormatFlags flags=TextFormatFlags.WordBreak|TextFormatFlags.TextBoxControl;
			if(control is System.Windows.Forms.GroupBox) {
				flags=TextFormatFlags.SingleLine;
			}
			flags|=GetAlignmentFlags(contentAlignment);
			if(!useMnemonic) {
				flags|=TextFormatFlags.NoPrefix;
			}
			return flags;
		}

		///<summary>TextFormatFlags for a ContentAlignment.</summary>
		public static TextFormatFlags GetAlignmentFlags(ContentAlignment contentAlignment) {
			TextFormatFlags flags=TextFormatFlags.Default;
			switch(contentAlignment) {
				case ContentAlignment.TopCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.BottomCenter:
					flags|=TextFormatFlags.HorizontalCenter;
					break;
				case ContentAlignment.TopRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.BottomRight:
					flags|=TextFormatFlags.Right;
					break;
			}
			switch(contentAlignment) {
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleRight:
					flags|=TextFormatFlags.VerticalCenter;
					break;
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight:
					flags|=TextFormatFlags.Bottom;
					break;
			}
			return flags;
		}

		///<summary>The text on one line, measured with TextRenderer at 96 dpi.</summary>
		public static Size MeasureLineGdi(string text,Font font,TextFormatFlags flags) {
			TextFormatFlags flagsLine=(flags&~(TextFormatFlags.WordBreak|TextFormatFlags.TextBoxControl))|TextFormatFlags.SingleLine;
			return TextRenderer.MeasureText(GraphicsMeasure,text,font,new Size(int.MaxValue,int.MaxValue),flagsLine);
		}

		///<summary>The text wrapped at width, measured with TextRenderer at 96 dpi.</summary>
		public static Size MeasureWrappedGdi(string text,Font font,int width,TextFormatFlags flags) {
			return TextRenderer.MeasureText(GraphicsMeasure,text,font,new Size(Math.Max(1,width),int.MaxValue),flags|TextFormatFlags.WordBreak);
		}

		private static void MeasureStockControl(Control control,Font font,float scaleMS,TextFitResult result) {
			TextFormatFlags flags=GetStockFlags(control);
			string text=control.Text;
			Size sizeLine=MeasureLineGdi(text,font,flags);
			Rectangle rectangleText;
			bool canWrap=true;
			if(control is Label) {
				rectangleText=control.ClientRectangle;
				rectangleText=ShrinkBy(rectangleText,control.Padding);
			}
			else if(control is System.Windows.Forms.GroupBox) {
				//GroupBox draws its caption on one line from 8 px in, and keeps the same room on the right.
				int inset=(int)Math.Round(8*scaleMS);
				rectangleText=new Rectangle(inset,0,control.Width-inset*2,sizeLine.Height);
				canWrap=false;
			}
			else {
				//ButtonBase (Button, CheckBox, RadioButton): what the control adds around its text (the glyph, the padding) comes from its own
				//preferred size at its font, and grows with the scale like the rest of the control.
				Size sizePreferred=control.GetPreferredSize(Size.Empty);
				Size sizeLineOwn=MeasureLineGdi(text,control.Font,flags);
				int extraWidth=(int)Math.Round(Math.Max(0,sizePreferred.Width-sizeLineOwn.Width)*scaleMS);
				int extraHeight=(int)Math.Round(Math.Max(0,sizePreferred.Height-sizeLineOwn.Height)*scaleMS);
				rectangleText=new Rectangle(0,0,control.Width-extraWidth,control.Height-extraHeight);
				if(control is ButtonBase buttonBase && buttonBase.AutoEllipsis) {
					canWrap=false;
				}
			}
			result.SizeAvailable=rectangleText.Size;
			result.SizeLine=sizeLine;
			Size sizeWrapped=Size.Empty;
			if(canWrap && sizeLine.Width>rectangleText.Width) {
				sizeWrapped=MeasureWrappedGdi(text,font,rectangleText.Width,flags);
			}
			Classify(result,sizeLine,sizeWrapped,canWrap);
			if(result.Kind==TextFitKind.Height && result.NeedHeight<=INK_CHECK_MAX_HEIGHT) {
				result.IsInkChecked=true;
				result.IsInkClipped=IsInkPastBoxGdi(text,font,rectangleText.Size,flags);
			}
		}

		private static Rectangle ShrinkBy(Rectangle rectangle,Padding padding) {
			return new Rectangle(rectangle.X+padding.Left,rectangle.Y+padding.Top,rectangle.Width-padding.Horizontal,rectangle.Height-padding.Vertical);
		}
		#endregion Stock controls (TextRenderer)

		#region The program's controls (MeasureString)
		///<summary>A StringFormat as UI.Button's GetStringFormat makes it for an alignment, with the mnemonic hidden (no keyboard cues).</summary>
		public static StringFormat CreateStringFormat(ContentAlignment contentAlignment) {
			StringFormat stringFormat=new StringFormat();
			switch(contentAlignment) {
				case ContentAlignment.TopCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.BottomCenter:
					stringFormat.Alignment=StringAlignment.Center;
					break;
				case ContentAlignment.TopRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.BottomRight:
					stringFormat.Alignment=StringAlignment.Far;
					break;
				default:
					stringFormat.Alignment=StringAlignment.Near;
					break;
			}
			switch(contentAlignment) {
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleRight:
					stringFormat.LineAlignment=StringAlignment.Center;
					break;
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight:
					stringFormat.LineAlignment=StringAlignment.Far;
					break;
				default:
					stringFormat.LineAlignment=StringAlignment.Near;
					break;
			}
			stringFormat.HotkeyPrefix=System.Drawing.Text.HotkeyPrefix.Hide;
			return stringFormat;
		}

		///<summary>The text on one line, measured with Graphics.MeasureString at 96 dpi, rounded up to whole pixels.</summary>
		public static Size MeasureLineGdiPlus(string text,Font font,StringFormat stringFormat) {
			using(StringFormat stringFormatLine=new StringFormat(stringFormat)) {
				stringFormatLine.FormatFlags|=StringFormatFlags.NoWrap;
				SizeF sizeF=GraphicsMeasure.MeasureString(text,font,new PointF(0,0),stringFormatLine);
				return new Size((int)Math.Ceiling(sizeF.Width),(int)Math.Ceiling(sizeF.Height));
			}
		}

		///<summary>The text wrapped at width, measured with Graphics.MeasureString at 96 dpi, rounded up to whole pixels.</summary>
		public static Size MeasureWrappedGdiPlus(string text,Font font,int width,StringFormat stringFormat) {
			SizeF sizeF=GraphicsMeasure.MeasureString(text,font,new SizeF(Math.Max(1,width),100000f),stringFormat);
			return new Size((int)Math.Ceiling(sizeF.Width),(int)Math.Ceiling(sizeF.Height));
		}

		private static void MeasureProgramControl(Control control,Font font,float scaleMS,TextFitResult result) {
			string text=control.Text;
			ContentAlignment contentAlignment=ContentAlignment.MiddleLeft;
			Rectangle rectangleText=control.ClientRectangle;
			bool canWrap=true;
			if(IsOfType(control,TYPE_UI_BUTTON)) {
				ButtonBase buttonBase=(ButtonBase)control;
				contentAlignment=buttonBase.TextAlign;
				rectangleText=GetUiButtonTextRectangle(buttonBase,scaleMS);
			}
			else if(IsOfType(control,TYPE_UI_CHECKBOX)) {
				//CheckBox.OnPaint: the box is LayoutManager.Scale(12); the text starts 3 px after it (TopLeft, MiddleLeft), or ends 4 px before it
				//(MiddleRight); near alignment, centred vertically except for TopLeft.
				int sqrSize=(int)Math.Round(UI_CHECKBOX_BOX_96*scaleMS);
				ContentAlignment checkAlign=GetProperty(control,"CheckAlign",ContentAlignment.MiddleLeft);
				if(checkAlign==ContentAlignment.TopLeft) {
					rectangleText=new Rectangle(sqrSize+3,0,control.Width-sqrSize-3,control.Height);
					contentAlignment=ContentAlignment.TopLeft;
				}
				else if(checkAlign==ContentAlignment.MiddleRight) {
					rectangleText=new Rectangle(0,0,control.Width-sqrSize-4,control.Height);
					contentAlignment=ContentAlignment.MiddleRight;
				}
				else {
					rectangleText=new Rectangle(sqrSize+3,0,control.Width-sqrSize-3,control.Height);
					contentAlignment=ContentAlignment.MiddleLeft;
				}
			}
			else if(IsOfType(control,TYPE_UI_GROUPBOX)) {
				//GroupBox.OnPaint: DrawString at (LayoutManager.Scale(5),1), one line, cut by the control's bounds.
				int x=(int)Math.Round(5*scaleMS);
				rectangleText=new Rectangle(x,1,control.Width-x,control.Height-1);
				contentAlignment=ContentAlignment.TopLeft;
				canWrap=false;
			}
			result.SizeAvailable=rectangleText.Size;
			if(rectangleText.Width<=0 || rectangleText.Height<=0) {
				//No room for text at all (a button whose picture takes the middle): not measured.
				result.Method=TextFitMethod.None;
				return;
			}
			using(StringFormat stringFormat=CreateStringFormat(contentAlignment)) {
				Size sizeLine=MeasureLineGdiPlus(text,font,stringFormat);
				result.SizeLine=sizeLine;
				Size sizeWrapped=Size.Empty;
				if(canWrap && sizeLine.Width>rectangleText.Width) {
					sizeWrapped=MeasureWrappedGdiPlus(text,font,rectangleText.Width,stringFormat);
				}
				Classify(result,sizeLine,sizeWrapped,canWrap);
				if(result.Kind==TextFitKind.Height && result.NeedHeight<=INK_CHECK_MAX_HEIGHT) {
					result.IsInkChecked=true;
					result.IsInkClipped=IsInkPastBoxGdiPlus(text,font,rectangleText.Size,stringFormat);
				}
			}
		}

		///<summary>UI.Button's text rectangle (Button.DrawTextAndImage): the client area without a picture; beside the picture (22 px for an
		///icon, the image's size otherwise, scaled) for MiddleLeft and MiddleRight; none for MiddleCenter. Other picture alignments draw the text
		///in an empty rectangle, which DrawString does not bound; they are returned as the client area.</summary>
		public static Rectangle GetUiButtonTextRectangle(ButtonBase button,float scaleMS) {
			Rectangle rectangleClient=button.ClientRectangle;
			Image image=button.Image;
			object icon=GetProperty<object>(button,"Icon",null);
			bool hasIcon=icon!=null && icon.ToString()!="None";
			if(image==null && !hasIcon) {
				return rectangleClient;
			}
			Size sizeImage;
			if(hasIcon) {
				sizeImage=new Size((int)Math.Round(22*scaleMS),(int)Math.Round(22*scaleMS));
			}
			else {
				sizeImage=new Size((int)Math.Round(image.Width*scaleMS),(int)Math.Round(image.Height*scaleMS));
			}
			Point pointAdjust=GetProperty(button,"AdjustImageLocation",new Point(0,0));
			switch(button.ImageAlign) {
				case ContentAlignment.MiddleLeft:
					return new Rectangle(sizeImage.Width,0,rectangleClient.Width-sizeImage.Width,rectangleClient.Height);
				case ContentAlignment.MiddleRight:
					return new Rectangle(0,0,rectangleClient.Width-sizeImage.Width-8+pointAdjust.X,rectangleClient.Height);
				case ContentAlignment.MiddleCenter:
					return Rectangle.Empty;
			}
			return rectangleClient;
		}
		#endregion The program's controls (MeasureString)

		#region Classification
		///<summary>Sorts a measurement into Fits, Height, Width or Line (section 5.3) and the pixels missing.</summary>
		public static void Classify(TextFitResult result,Size sizeLine,Size sizeWrapped,bool canWrap) {
			Size sizeAvailable=result.SizeAvailable;
			if(sizeLine.Width<=sizeAvailable.Width) {
				if(sizeLine.Height<=sizeAvailable.Height) {
					result.Kind=TextFitKind.Fits;
					return;
				}
				result.Kind=TextFitKind.Height;
				result.NeedHeight=sizeLine.Height-sizeAvailable.Height;
				return;
			}
			//Too wide for one line.
			if(canWrap && sizeWrapped.Width<=sizeAvailable.Width && sizeWrapped.Height<=sizeAvailable.Height) {
				result.Kind=TextFitKind.Fits;
				return;
			}
			if(!canWrap || sizeAvailable.Height<sizeLine.Height*2) {
				//Room for one line only: the line needs more width.
				result.Kind=TextFitKind.Width;
				result.NeedWidth=sizeLine.Width-sizeAvailable.Width;
				result.NeedHeight=Math.Max(0,sizeLine.Height-sizeAvailable.Height);
				return;
			}
			result.Kind=TextFitKind.Line;
			result.NeedHeight=Math.Max(0,sizeWrapped.Height-sizeAvailable.Height);
			result.NeedWidth=Math.Max(0,sizeWrapped.Width-sizeAvailable.Width);
		}
		#endregion Classification

		#region The ink check
		///<summary>A margin around the text box in the ink check's bitmap, wide enough for any overhang.</summary>
		private const int INK_MARGIN=24;

		///<summary>Draws the text with TextRenderer into a box of sizeBox, unclipped, and returns true when any ink falls outside the box.</summary>
		public static bool IsInkPastBoxGdi(string text,Font font,Size sizeBox,TextFormatFlags flags) {
			if(sizeBox.Width<=0 || sizeBox.Height<=0) {
				return true;
			}
			using(Bitmap bitmap=NewInkBitmap(sizeBox)) {
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.White);
					Rectangle rectangle=new Rectangle(INK_MARGIN,INK_MARGIN,sizeBox.Width,sizeBox.Height);
					TextRenderer.DrawText(g,text,font,rectangle,Color.Black,Color.White,flags|TextFormatFlags.NoClipping);
				}
				return HasInkOutside(bitmap,sizeBox);
			}
		}

		///<summary>Draws the text with Graphics.DrawString into a box of sizeBox, unclipped, and returns true when any ink falls outside the box.</summary>
		public static bool IsInkPastBoxGdiPlus(string text,Font font,Size sizeBox,StringFormat stringFormat) {
			if(sizeBox.Width<=0 || sizeBox.Height<=0) {
				return true;
			}
			using(Bitmap bitmap=NewInkBitmap(sizeBox))
			using(StringFormat stringFormatNoClip=new StringFormat(stringFormat)) {
				stringFormatNoClip.FormatFlags|=StringFormatFlags.NoClip;
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.White);
					RectangleF rectangleF=new RectangleF(INK_MARGIN,INK_MARGIN,sizeBox.Width,sizeBox.Height);
					g.DrawString(text,font,Brushes.Black,rectangleF,stringFormatNoClip);
				}
				return HasInkOutside(bitmap,sizeBox);
			}
		}

		private static Bitmap NewInkBitmap(Size sizeBox) {
			Bitmap bitmap=new Bitmap(sizeBox.Width+INK_MARGIN*2,sizeBox.Height+INK_MARGIN*2,PixelFormat.Format32bppArgb);
			bitmap.SetResolution(96,96);
			return bitmap;
		}

		///<summary>True when a pixel outside the box (at INK_MARGIN, INK_MARGIN) is not white.</summary>
		private static bool HasInkOutside(Bitmap bitmap,Size sizeBox) {
			Rectangle rectangleBox=new Rectangle(INK_MARGIN,INK_MARGIN,sizeBox.Width,sizeBox.Height);
			BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
			try {
				int[] arrayRow=new int[bitmap.Width];
				for(int y=0;y<bitmap.Height;y++) {
					Marshal.Copy(IntPtr.Add(bitmapData.Scan0,y*bitmapData.Stride),arrayRow,0,bitmap.Width);
					bool isRowInBox=y>=rectangleBox.Top && y<rectangleBox.Bottom;
					for(int x=0;x<bitmap.Width;x++) {
						if(isRowInBox && x>=rectangleBox.Left && x<rectangleBox.Right) {
							continue;
						}
						if((arrayRow[x]&0xFFFFFF)!=0xFFFFFF) {
							return true;
						}
					}
				}
				return false;
			}
			finally {
				bitmap.UnlockBits(bitmapData);
			}
		}
		#endregion The ink check

		///<summary>A public or non-public instance property of the control by name, or the default when it has none of that type.</summary>
		public static T GetProperty<T>(Control control,string name,T valueDefault) {
			object value;
			try {
				PropertyInfo propertyInfo=control.GetType().GetProperty(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
				if(propertyInfo==null) {
					return valueDefault;
				}
				value=propertyInfo.GetValue(control);
			}
			catch(AmbiguousMatchException) {
				return valueDefault;
			}
			if(value is T valueT) {
				return valueT;
			}
			return valueDefault;
		}
	}
}
