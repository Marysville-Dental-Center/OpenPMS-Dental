// The shapes of the self-drawn WinForms dialog parts of package UI-4c-2 (docs/specs/ui-refresh-plan.md, sections 3.2, 3.3 and 3.6;
// docs/port-notes/UI-4c.md): the progress bar (a SurfaceRaised track with 4 px corners, the Accent fill, a 1 px Stroke outline, as the design sheet's
// progress bar, DesignSheetControls.DrawCalendarAndProgress), the colour picker's swatch (the colour in a box with the input outline, StrokeStrong,
// StrokeStrongHover under the mouse, Stroke when disabled, the focus ring with keyboard cues; "no colour" a white box with a diagonal line) and the
// colours of the database integrity warning triangle (Warning, or Danger for the red one, with the white mark). Drawn with UiControlPaint's shapes
// (UI-4a), so the snapping rule of section 3.3 holds and the controls give the design sheet's pixels. Windows Forms and System.Drawing only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CodeBase {
	///<summary>The token shapes of the progress bar, the colour swatch and the integrity warning (section 3.6), in device pixels at a scale.</summary>
	public static class UiDialogPaint {
		#region Progress
		///<summary>The part of the track the fill covers: fraction (0 to 1, clamped) of its width from the left, whole pixels; empty for 0.</summary>
		public static Rectangle GetProgressFillRectangle(Rectangle rectangleTrack,double fraction) {
			if(double.IsNaN(fraction) || fraction<=0) {
				return new Rectangle(rectangleTrack.X,rectangleTrack.Y,0,rectangleTrack.Height);
			}
			fraction=Math.Min(1,fraction);
			int width=(int)Math.Round(rectangleTrack.Width*fraction,MidpointRounding.AwayFromZero);
			return new Rectangle(rectangleTrack.X,rectangleTrack.Y,width,rectangleTrack.Height);
		}

		///<summary>A progress bar (sections 3.6, "Dialogs", and the design sheet): the SurfaceRaised track with 4 px corners, the fill over fraction of
		///it in Accent (TextDisabled when disabled) with 4 px corners, and the 1 px Stroke outline over both. Returns the fill's rectangle.</summary>
		public static Rectangle DrawProgress(Graphics g,Rectangle rectangleTrack,double fraction,bool isEnabled,float scale) {
			UiControlPaint.Fill(g,EnumUiColor.SurfaceRaised,rectangleTrack,UiTheme.CORNER_CONTROL,scale);
			Rectangle rectangleFill=GetProgressFillRectangle(rectangleTrack,fraction);
			UiControlPaint.Fill(g,isEnabled ? EnumUiColor.Accent : EnumUiColor.TextDisabled,rectangleFill,UiTheme.CORNER_CONTROL,scale);
			UiControlPaint.Outline(g,EnumUiColor.Stroke,rectangleTrack,UiTheme.GetLineWidth(scale),UiTheme.CORNER_CONTROL,scale);
			return rectangleFill;
		}
		#endregion Progress

		#region Swatch
		///<summary>The token of a swatch's outline: StrokeStrong (an input's outline), StrokeStrongHover under the mouse, Stroke when disabled.</summary>
		public static EnumUiColor GetSwatchBorder(bool isHover,bool isEnabled) {
			if(!isEnabled) {
				return EnumUiColor.Stroke;
			}
			return isHover ? EnumUiColor.StrokeStrongHover : EnumUiColor.StrokeStrong;
		}

		///<summary>The colour picker's swatch in its bounds: colorBack behind the corners, the colour (the office's data, drawn as it is) inside 4 px
		///corners, the outline of GetSwatchBorder, and the focus ring when isFocused. A transparent colour ("None") is a SurfaceRaised box with a
		///diagonal StrokeStrong line from the bottom left to the top right.</summary>
		public static void DrawSwatch(Graphics g,Rectangle rectangle,Color color,bool isHover,bool isEnabled,bool isFocused,float scale,Color colorBack) {
			UiControlPaint.Fill(g,colorBack,rectangle,0,scale);
			bool isNone=color.A==0;
			UiControlPaint.Fill(g,isNone ? UiTheme.GetColor(EnumUiColor.SurfaceRaised) : Color.FromArgb(255,color),rectangle,UiTheme.CORNER_CONTROL,scale);
			if(isNone) {
				DrawNoneLine(g,rectangle,scale);
			}
			UiControlPaint.Outline(g,GetSwatchBorder(isHover,isEnabled),rectangle,UiTheme.GetLineWidth(scale),UiTheme.CORNER_CONTROL,scale);
			if(isFocused) {
				UiControlPaint.FocusRing(g,rectangle,UiTheme.CORNER_CONTROL,scale);
			}
		}

		///<summary>The "no colour" mark: a 1 px (GetLineWidth) StrokeStrong line from the bottom left to the top right, inset by the corner radius.</summary>
		private static void DrawNoneLine(Graphics g,Rectangle rectangle,float scale) {
			int inset=UiTheme.GetCornerRadius(UiTheme.CORNER_CONTROL,scale);
			GraphicsState graphicsState=g.Save();
			try {
				g.SmoothingMode=SmoothingMode.AntiAlias;
				g.PixelOffsetMode=PixelOffsetMode.Half;
				using(Pen pen=new Pen(UiTheme.GetColor(EnumUiColor.StrokeStrong),UiTheme.GetLineWidth(scale))) {
					g.DrawLine(pen,rectangle.Left+inset,rectangle.Bottom-inset,rectangle.Right-inset,rectangle.Top+inset);
				}
			}
			finally {
				g.Restore(graphicsState);
			}
		}
		#endregion Swatch

		#region Warning
		///<summary>The fill and outline token of the database integrity warning triangle: Warning (the notify colour of section 3.2), or Danger for the
		///red triangle (EnumIntegrityBehavior.TriangleRed). The mark inside is TextOnAccent (white: 5.3:1 on Warning, 5.7:1 on Danger).</summary>
		public static EnumUiColor GetWarningFill(bool isRed) {
			return isRed ? EnumUiColor.Danger : EnumUiColor.Warning;
		}
		#endregion Warning
	}
}
