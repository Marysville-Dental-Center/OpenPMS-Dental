// The shapes the self-drawn WinForms controls paint with (docs/specs/ui-refresh-plan.md, sections 3.3 and 3.6; package UI-4a,
// docs/port-notes/UI-4a.md): fills, outlines, the focus ring and the focus ring on an accent fill, the button's chrome and the check box's box,
// drawn from the UiTheme tokens with the snapping rule of section 3.3 (whole device pixels, 1 px lines on the pixel centres with
// PixelOffsetMode.Half, corner radii in whole pixels). These are the same shapes the design sheet draws (tests/UiRefreshTests/DesignSheetPainter.cs),
// so a control and the sheet give the same pixels; the control state tests compare them. Each method restores the Graphics' settings, so a
// control's own text drawing after it is as before. Windows Forms and System.Drawing only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CodeBase {
	///<summary>The states a self-drawn button shows (section 3.6). Disabled wins over the others.</summary>
	public enum EnumUiButtonState {
		Normal,
		Hover,
		Pressed,
		Disabled,
	}

	///<summary>The token shapes of the self-drawn WinForms controls (section 3.6), in device pixels at a scale.</summary>
	public static class UiControlPaint {
		#region Shapes
		///<summary>Fills the rectangle with corners of radius96 at 96 DPI (snapped to whole pixels): antialiased when rounded, plain when square.</summary>
		public static void Fill(Graphics g,Color color,Rectangle rectangle,int radius96,float scale) {
			if(rectangle.Width<=0 || rectangle.Height<=0) {
				return;
			}
			int radius=UiTheme.GetCornerRadius(radius96,scale);
			GraphicsState graphicsState=g.Save();
			try {
				g.PixelOffsetMode=PixelOffsetMode.Half;
				using(SolidBrush solidBrush=new SolidBrush(color)) {
					if(radius==0) {
						g.SmoothingMode=SmoothingMode.None;
						g.FillRectangle(solidBrush,rectangle);
						return;
					}
					g.SmoothingMode=SmoothingMode.AntiAlias;
					using(GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(rectangle,radius)) {
						g.FillPath(solidBrush,graphicsPath);
					}
				}
			}
			finally {
				g.Restore(graphicsState);
			}
		}

		public static void Fill(Graphics g,EnumUiColor uiColor,Rectangle rectangle,int radius96,float scale) {
			Fill(g,UiTheme.GetColor(uiColor),rectangle,radius96,scale);
		}

		///<summary>An outline width device pixels wide inside the rectangle, on whole pixels (section 3.3): a square one as four filled edges, a
		///rounded one as an antialiased path on the pixel centres.</summary>
		public static void Outline(Graphics g,Color color,Rectangle rectangle,int width,int radius96,float scale) {
			if(rectangle.Width<=0 || rectangle.Height<=0 || width<=0) {
				return;
			}
			int radius=UiTheme.GetCornerRadius(radius96,scale);
			GraphicsState graphicsState=g.Save();
			try {
				g.PixelOffsetMode=PixelOffsetMode.Half;
				if(radius==0) {
					g.SmoothingMode=SmoothingMode.None;
					using(SolidBrush solidBrush=new SolidBrush(color)) {
						g.FillRectangle(solidBrush,rectangle.X,rectangle.Y,rectangle.Width,width);
						g.FillRectangle(solidBrush,rectangle.X,rectangle.Bottom-width,rectangle.Width,width);
						g.FillRectangle(solidBrush,rectangle.X,rectangle.Y,width,rectangle.Height);
						g.FillRectangle(solidBrush,rectangle.Right-width,rectangle.Y,width,rectangle.Height);
					}
					return;
				}
				g.SmoothingMode=SmoothingMode.AntiAlias;
				RectangleF rectangleStroke=UiTheme.GetStrokeRectangle(rectangle,width);
				using(Pen pen=new Pen(color,width))
				using(GraphicsPath graphicsPath=UiTheme.GetRoundedRectanglePath(rectangleStroke,Math.Max(0,radius-width/2f))) {
					g.DrawPath(pen,graphicsPath);
				}
			}
			finally {
				g.Restore(graphicsState);
			}
		}

		public static void Outline(Graphics g,EnumUiColor uiColor,Rectangle rectangle,int width,int radius96,float scale) {
			Outline(g,UiTheme.GetColor(uiColor),rectangle,width,radius96,scale);
		}

		///<summary>The focus ring (section 3.6): 2 px (scaled by GetFocusWidth) in Accent, inside the rectangle, following its corners.</summary>
		public static void FocusRing(Graphics g,Rectangle rectangle,int radius96,float scale) {
			Outline(g,EnumUiColor.Accent,rectangle,UiTheme.GetFocusWidth(scale),radius96,scale);
		}

		///<summary>The focus of an accent-filled control (section 3.6): the Accent ring outside a 1 px white inner ring, so the ring shows against the
		///fill.</summary>
		public static void FocusRingOnAccent(Graphics g,Rectangle rectangle,int radius96,float scale) {
			int widthFocus=UiTheme.GetFocusWidth(scale);
			Outline(g,EnumUiColor.Accent,rectangle,widthFocus,radius96,scale);
			Rectangle rectangleInner=Rectangle.Inflate(rectangle,-widthFocus,-widthFocus);
			Outline(g,EnumUiColor.TextOnAccent,rectangleInner,UiTheme.GetLineWidth(scale),Math.Max(0,radius96-2),scale);
		}
		#endregion Shapes

		#region Controls
		///<summary>A button's chrome (section 3.6) in its bounds: flat SurfaceRaised with a 1 px Stroke border and 4 px corners, ButtonHover on hover,
		///ButtonPressed pressed; the default (primary) button filled Accent, AccentHover, AccentPressed without a border; a disabled one, the default
		///one too, #F5F5F5 (ButtonHover) with a Stroke border. The focus ring when isFocused (the ring on the accent for the default button). Returns
		///the token of the button's text and icon.</summary>
		public static EnumUiColor DrawButton(Graphics g,Rectangle rectangle,EnumUiButtonState buttonState,bool isPrimary,bool isFocused,float scale) {
			int widthLine=UiTheme.GetLineWidth(scale);
			if(buttonState==EnumUiButtonState.Disabled) {
				Fill(g,EnumUiColor.ButtonHover,rectangle,UiTheme.CORNER_CONTROL,scale);
				Outline(g,EnumUiColor.Stroke,rectangle,widthLine,UiTheme.CORNER_CONTROL,scale);
				return EnumUiColor.TextDisabled;
			}
			if(isPrimary) {
				Fill(g,GetButtonFill(buttonState,isPrimary:true),rectangle,UiTheme.CORNER_CONTROL,scale);
				if(isFocused) {
					FocusRingOnAccent(g,rectangle,UiTheme.CORNER_CONTROL,scale);
				}
				return EnumUiColor.TextOnAccent;
			}
			Fill(g,GetButtonFill(buttonState,isPrimary:false),rectangle,UiTheme.CORNER_CONTROL,scale);
			Outline(g,EnumUiColor.Stroke,rectangle,widthLine,UiTheme.CORNER_CONTROL,scale);
			if(isFocused) {
				FocusRing(g,rectangle,UiTheme.CORNER_CONTROL,scale);
			}
			return EnumUiColor.Text;
		}

		///<summary>The fill of a button in a state (section 3.6): SurfaceRaised, ButtonHover, ButtonPressed; Accent, AccentHover, AccentPressed for the
		///default button; ButtonHover (#F5F5F5) when disabled.</summary>
		public static EnumUiColor GetButtonFill(EnumUiButtonState buttonState,bool isPrimary) {
			switch(buttonState) {
				case EnumUiButtonState.Hover:
					return isPrimary ? EnumUiColor.AccentHover : EnumUiColor.ButtonHover;
				case EnumUiButtonState.Pressed:
					return isPrimary ? EnumUiColor.AccentPressed : EnumUiColor.ButtonPressed;
				case EnumUiButtonState.Disabled:
					return EnumUiColor.ButtonHover;
				default:
					return isPrimary ? EnumUiColor.Accent : EnumUiColor.SurfaceRaised;
			}
		}

		///<summary>A check box's box (section 3.6) in rectangleBox: empty, SurfaceRaised with a 1 px StrokeStrong border and 2 px corners
		///(StrokeStrongHover on hover, TextDisabled when disabled); checked, Accent with the white Fluent check mark; indeterminate, Accent with
		///the white Fluent dash. A disabled checked box is filled TextDisabled. colorBack replaces SurfaceRaised for an empty box when not empty
		///(the control's own box colour).</summary>
		public static void DrawCheckBoxBox(Graphics g,Rectangle rectangleBox,CheckState checkState,bool isHover,bool isEnabled,float scale,Color colorBack) {
			if(checkState==CheckState.Unchecked) {
				Fill(g,colorBack.IsEmpty ? UiTheme.GetColor(EnumUiColor.SurfaceRaised) : colorBack,rectangleBox,UiTheme.CORNER_CHECKBOX,scale);
				EnumUiColor uiColorBorder=!isEnabled ? EnumUiColor.TextDisabled : (isHover ? EnumUiColor.StrokeStrongHover : EnumUiColor.StrokeStrong);
				Outline(g,uiColorBorder,rectangleBox,UiTheme.GetLineWidth(scale),UiTheme.CORNER_CHECKBOX,scale);
				return;
			}
			Fill(g,isEnabled ? EnumUiColor.Accent : EnumUiColor.TextDisabled,rectangleBox,UiTheme.CORNER_CHECKBOX,scale);
			UiGlyph uiGlyph=checkState==CheckState.Indeterminate ? UiGlyph.Dash : UiGlyph.Checkmark;
			UiIcons.Draw(g,uiGlyph,rectangleBox,EnumUiColor.TextOnAccent);
		}

		///<summary>The ring of a focused check box or radio button (section 3.6): around the box and its text, 4 px outside them at 96 DPI, kept
		///inside the control's bounds.</summary>
		public static Rectangle GetCheckFocusRectangle(Rectangle rectangleBox,Rectangle rectangleTextUsed,Rectangle rectangleBounds,float scale) {
			int inset=UiTheme.ScaleI(UiTheme.SPACE_4,scale);
			Rectangle rectangleUnion=Rectangle.Union(rectangleBox,rectangleTextUsed);
			Rectangle rectangleRing=Rectangle.FromLTRB(rectangleUnion.Left-inset,rectangleBox.Top-inset,rectangleUnion.Right+inset,rectangleBox.Bottom+inset);
			rectangleRing.Intersect(rectangleBounds);
			return rectangleRing;
		}
		#endregion Controls
	}
}
