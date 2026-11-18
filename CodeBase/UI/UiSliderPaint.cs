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
