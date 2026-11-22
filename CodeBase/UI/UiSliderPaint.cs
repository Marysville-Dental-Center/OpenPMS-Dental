// The tokens and glyphs of the imaging and appointment chrome (docs/specs/ui-refresh-plan.md, sections 3.2, 3.3 and 3.6; package UI-4c-1,
// docs/port-notes/UI-4c.md): the handles of the windowing slider (flat, as a button: SurfaceRaised, ButtonHover, ButtonPressed, with a StrokeStrong
// outline that darkens under the mouse and turns Accent while dragged), the zoom slider's cells (transparent, SubtleHover, SubtlePressed, as toolbar
// buttons), its value box (an input: SurfaceRaised with a StrokeStrong, StrokeStrongHover or Accent outline) and its minus and plus glyphs (Fluent
// subtract, and add from EnumIcons.Add's manifest row, so no icon is added), and the snap areas of the floating image windows' menu. The WinForms
// controls (OpenPmsDental/UI/Appt, OpenPmsDental/UI/ImagesModule) and the WPF ones (WpfControlsOD/UI/Controls/ImagingModule) take the same tokens
// from here. The shapes are UiControlPaint's (UI-4a). Windows Forms and System.Drawing only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;

namespace CodeBase {
	///<summary>The tokens and glyphs of the imaging and appointment chrome (section 3.6), shared by the WinForms and the WPF controls.</summary>
	public static class UiSliderPaint {
		///<summary>The EnumIcons member whose Fluent icon is add (UI-7b): the zoom slider's plus. Taken from there so no icon is added.</summary>
		public const string ICON_ADD="Add";
		///<summary>The size of the zoom slider's minus and plus glyphs and of the caption glyphs of the image dock's header at 96 DPI.</summary>
		public const int SIZE_GLYPH=12;
		///<summary>The designs of ICON_ADD, found once.</summary>
		private static volatile IconDesign[] _arrayDesignsAdd;

		#region Tokens
		///<summary>The fill of a windowing slider handle (a small button): SurfaceRaised at rest, ButtonHover on hover and when disabled, ButtonPressed
		///while dragged.</summary>
		public static EnumUiColor GetHandleFill(EnumUiButtonState buttonState) {
			return UiControlPaint.GetButtonFill(buttonState,isPrimary:false);
		}

		///<summary>The outline of a windowing slider handle and of the range between the handles (an input's outline): StrokeStrong at rest,
		///StrokeStrongHover on hover, Accent while dragged, Stroke when disabled.</summary>
		public static EnumUiColor GetHandleOutline(EnumUiButtonState buttonState) {
			switch(buttonState) {
				case EnumUiButtonState.Hover:
					return EnumUiColor.StrokeStrongHover;
				case EnumUiButtonState.Pressed:
					return EnumUiColor.Accent;
				case EnumUiButtonState.Disabled:
					return EnumUiColor.Stroke;
				default:
					return EnumUiColor.StrokeStrong;
			}
		}

		///<summary>The fill of a zoom slider cell (minus, plus, Fit, 100), as a toolbar button: none at rest (null), SubtleHover on hover, SubtlePressed
		///pressed.</summary>
		public static EnumUiColor? GetCellFill(EnumUiButtonState buttonState) {
			switch(buttonState) {
				case EnumUiButtonState.Hover:
					return EnumUiColor.SubtleHover;
				case EnumUiButtonState.Pressed:
					return EnumUiColor.SubtlePressed;
				default:
					return null;
			}
		}

		///<summary>The token of the glyphs and texts on the chrome: Text, TextDisabled when disabled.</summary>
		public static EnumUiColor GetText(bool isEnabled) {
			return isEnabled ? EnumUiColor.Text : EnumUiColor.TextDisabled;
		}
		#endregion Tokens

		#region Track bar (UI-13)
		//UI-13 (docs/port-notes/UI-13.md, the owner's G-UI3-ui12p row "Modernize horizontal sliding bar"): the stock track bar (ControlChart's
		//procedure date slider, any other) in the Windows 11 Fluent slider look: a thin rounded track in Stroke, the part from the minimum to the
		//thumb in Accent, a round thumb (an Accent dot inside a white ring with a 1 px Stroke edge; the dot grows under the mouse and shrinks while
		//dragged, as Windows 11's does) and quiet tick marks in StrokeStrong. Disabled: the track Stroke, the fill and the dot TextDisabled.
		///<summary>The track's thickness at 96 DPI.</summary>
		public const int THICKNESS_SLIDER_TRACK=4;
		///<summary>The thumb's diameter at 96 DPI (the white ring included).</summary>
		public const int SIZE_SLIDER_THUMB=16;
		///<summary>The white ring's width at rest at 96 DPI; 3 under the mouse (a larger dot), 5 while dragged (a smaller dot).</summary>
		public const int WIDTH_SLIDER_RING=4;
		///<summary>A tick mark's length at 96 DPI.</summary>
		public const int LENGTH_SLIDER_TICK=4;

		///<summary>The track: Stroke.</summary>
		public static EnumUiColor GetSliderTrackColor() {
			return EnumUiColor.Stroke;
		}

		///<summary>The track from the minimum to the thumb: Accent, TextDisabled when disabled.</summary>
		public static EnumUiColor GetSliderFillColor(bool isEnabled) {
			return isEnabled ? EnumUiColor.Accent : EnumUiColor.TextDisabled;
		}

		///<summary>The thumb's dot: Accent, AccentHover under the mouse, AccentPressed while dragged, TextDisabled when disabled.</summary>
		public static EnumUiColor GetSliderDotColor(EnumUiButtonState buttonState) {
			switch(buttonState) {
				case EnumUiButtonState.Hover:
					return EnumUiColor.AccentHover;
				case EnumUiButtonState.Pressed:
					return EnumUiColor.AccentPressed;
				case EnumUiButtonState.Disabled:
					return EnumUiColor.TextDisabled;
				default:
					return EnumUiColor.Accent;
			}
		}

		///<summary>The tick marks: StrokeStrong, Stroke when disabled.</summary>
		public static EnumUiColor GetSliderTickColor(bool isEnabled) {
			return isEnabled ? EnumUiColor.StrokeStrong : EnumUiColor.Stroke;
		}

		///<summary>The white ring's width in device pixels in a state: WIDTH_SLIDER_RING at rest and disabled, one less under the mouse, one more
		///while dragged, at the scale.</summary>
		public static int GetSliderRingWidth(EnumUiButtonState buttonState,float scale) {
			int width96=WIDTH_SLIDER_RING;
			if(buttonState==EnumUiButtonState.Hover) {
				width96--;
			}
			else if(buttonState==EnumUiButtonState.Pressed) {
				width96++;
			}
			return Math.Max(1,UiTheme.ScaleI(width96,scale));
		}

		///<summary>The thumb's square (SIZE_SLIDER_THUMB at the scale) centred on a point, on whole pixels.</summary>
		public static Rectangle GetSliderThumbRectangle(Point pointCentre,float scale) {
			int size=UiTheme.ScaleI(SIZE_SLIDER_THUMB,scale);
			return new Rectangle(pointCentre.X-size/2,pointCentre.Y-size/2,size,size);
		}

		///<summary>The track's rectangle along the line from pointStart to pointEnd (both on the track's centre line, start at the minimum):
		///THICKNESS_SLIDER_TRACK thick at the scale, running half a thickness past each end so the rounded ends sit on the points.</summary>
		public static Rectangle GetSliderTrackRectangle(Point pointStart,Point pointEnd,bool isVertical,float scale) {
			int thickness=Math.Max(2,UiTheme.ScaleI(THICKNESS_SLIDER_TRACK,scale));
			int half=thickness/2;
			if(isVertical) {
				int top=Math.Min(pointStart.Y,pointEnd.Y);
				int bottom=Math.Max(pointStart.Y,pointEnd.Y);
				return Rectangle.FromLTRB(pointStart.X-half,top-half,pointStart.X-half+thickness,bottom+half);
			}
			int left=Math.Min(pointStart.X,pointEnd.X);
			int right=Math.Max(pointStart.X,pointEnd.X);
			return Rectangle.FromLTRB(left-half,pointStart.Y-half,right+half,pointStart.Y-half+thickness);
		}

		///<summary>The track (pointStart at the minimum to pointEnd at the maximum) and its filled part up to pointThumb, as pills.</summary>
		public static void DrawSliderTrack(Graphics g,Point pointStart,Point pointEnd,Point pointThumb,bool isVertical,bool isEnabled,float scale) {
			Rectangle rectangleTrack=GetSliderTrackRectangle(pointStart,pointEnd,isVertical,scale);
			int thickness=isVertical ? rectangleTrack.Width : rectangleTrack.Height;
			UiControlPaint.Fill(g,GetSliderTrackColor(),rectangleTrack,thickness/2,1f);
			Rectangle rectangleFill=GetSliderTrackRectangle(pointStart,pointThumb,isVertical,scale);
			UiControlPaint.Fill(g,GetSliderFillColor(isEnabled),rectangleFill,thickness/2,1f);
		}

		///<summary>The thumb centred on a point: the white (SurfaceRaised) disc with a 1 px Stroke edge, the dot inside the ring (GetSliderDotColor,
		///GetSliderRingWidth), and the focus ring around it when isFocused.</summary>
		public static void DrawSliderThumb(Graphics g,Point pointCentre,EnumUiButtonState buttonState,bool isFocused,float scale) {
			Rectangle rectangleThumb=GetSliderThumbRectangle(pointCentre,scale);
			System.Drawing.Drawing2D.GraphicsState graphicsState=g.Save();
			try {
				g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				g.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.Half;
				using(SolidBrush brush=new SolidBrush(UiTheme.GetColor(EnumUiColor.SurfaceRaised))) {
					g.FillEllipse(brush,rectangleThumb);
				}
				float widthLine=UiTheme.GetLineWidth(scale);
				using(Pen pen=new Pen(UiTheme.GetColor(EnumUiColor.Stroke),widthLine)) {
					g.DrawEllipse(pen,rectangleThumb.X+widthLine/2f,rectangleThumb.Y+widthLine/2f,rectangleThumb.Width-widthLine,rectangleThumb.Height-widthLine);
				}
				int ring=GetSliderRingWidth(buttonState,scale);
				Rectangle rectangleDot=Rectangle.Inflate(rectangleThumb,-ring,-ring);
				if(rectangleDot.Width>0) {
					using(SolidBrush brush=new SolidBrush(UiTheme.GetColor(GetSliderDotColor(buttonState)))) {
						g.FillEllipse(brush,rectangleDot);
					}
				}
			}
			finally {
				g.Restore(graphicsState);
			}
			if(isFocused) {
				int gap=UiTheme.ScaleI(UiTheme.SPACE_2,scale);
				UiControlPaint.FocusRing(g,Rectangle.Inflate(rectangleThumb,gap+UiTheme.GetFocusWidth(scale),gap+UiTheme.GetFocusWidth(scale)),
					SIZE_SLIDER_THUMB,scale);
			}
		}

		///<summary>A tick mark from a point on the track's side, LENGTH_SLIDER_TICK long away from the track (across it: down or right when
		///isAfter, up or left otherwise), 1 px wide at 100%.</summary>
		public static void DrawSliderTick(Graphics g,Point point,bool isVertical,bool isAfter,bool isEnabled,float scale) {
			int length=UiTheme.ScaleI(LENGTH_SLIDER_TICK,scale);
			int width=UiTheme.GetLineWidth(scale);
			Rectangle rectangleTick;
			if(isVertical) {
				rectangleTick=new Rectangle(isAfter ? point.X : point.X-length,point.Y-width/2,length,width);
			}
			else {
				rectangleTick=new Rectangle(point.X-width/2,isAfter ? point.Y : point.Y-length,width,length);
			}
			UiControlPaint.Fill(g,GetSliderTickColor(isEnabled),rectangleTick,UiTheme.CORNER_SQUARE,scale);
		}
		#endregion Track bar (UI-13)

		#region Glyphs
		///<summary>The square of a glyph of SIZE_GLYPH at the scale, centred in the rectangle on whole pixels.</summary>
		public static Rectangle GetGlyphRectangle(Rectangle rectangle,float scale) {
			int size=UiTheme.ScaleI(SIZE_GLYPH,scale);
			return new Rectangle(rectangle.X+(rectangle.Width-size)/2,rectangle.Y+(rectangle.Height-size)/2,size,size);
		}

		///<summary>The Fluent add glyph (ICON_ADD's design) for a square of sizeSlot device pixels, or null when the icon set has none.</summary>
		public static IconDesign GetAddDesign(int sizeSlot) {
			IconDesign[] arrayDesigns=_arrayDesignsAdd;
			if(arrayDesigns==null) {
				arrayDesigns=UiIcons.GetDesigns("EnumIcons",ICON_ADD);
				_arrayDesignsAdd=arrayDesigns;//read-only after this; a second thread at the same moment only finds the same designs
			}
			if(arrayDesigns.Length==0) {
				return null;
			}
			return UiIcons.GetDesign(arrayDesigns,sizeSlot);
		}

		///<summary>The zoom slider's minus (Fluent subtract) or plus (Fluent add) in its square, in the token.</summary>
		public static void DrawZoomGlyph(Graphics g,Rectangle rectangleGlyph,bool isPlus,EnumUiColor uiColor) {
			if(!isPlus) {
				UiIcons.Draw(g,UiGlyph.Subtract,rectangleGlyph,uiColor);
				return;
			}
			IconDesign iconDesign=GetAddDesign(rectangleGlyph.Width);
			if(iconDesign==null) {
				return;
			}
			UiIcons.Draw(g,iconDesign,rectangleGlyph,UiTheme.GetColor(uiColor));
		}
		#endregion Glyphs
	}
}
