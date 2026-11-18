// The shapes of the self-drawn WinForms combo boxes and lists (docs/specs/ui-refresh-plan.md, sections 3.2, 3.3 and 3.6; package UI-4b-1,
// docs/port-notes/UI-4b.md): the combo box's box (SurfaceRaised, a 1 px StrokeStrong border with 4 px corners, StrokeStrongHover on hover, the
// Fluent chevron, the 2 px Accent focus line inside the bottom edge), a list's border (square, StrokeStrong), and a list item (RowHover on hover,
// AccentSubtle when selected with the 2 px selection bar in the item's first pixels: Accent while the list has focus, SelectionBarNoFocus without;
// the focused row's 1 px Accent outline). Drawn with UiControlPaint's shapes (UI-4a), so the snapping rule of section 3.3 holds, and the same shapes
// as the design sheet's drawing of the families (tests/UiRefreshTests/DesignSheetControls.cs). Windows Forms and System.Drawing only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;

namespace CodeBase {
	///<summary>The token shapes of the self-drawn WinForms combo boxes and lists (section 3.6), in device pixels at a scale.</summary>
	public static class UiListPaint {
		///<summary>The chevron's inset from the combo box's right edge at 96 DPI, as the design sheet draws it.</summary>
		public const int INSET_CHEVRON=6;

		#region Combo box
		///<summary>A combo box's box in its rectangle (section 3.6): colorBack (empty: SurfaceRaised) with 4 px corners, a 1 px border in StrokeStrong,
		///StrokeStrongHover on hover, Stroke when disabled; the Fluent chevron in Text (TextDisabled when disabled) at the right; the 2 px Accent focus
		///line inside the bottom edge when isFocused. Returns the token of the text.</summary>
		public static EnumUiColor DrawComboBox(Graphics g,Rectangle rectangle,bool isHover,bool isEnabled,bool isFocused,float scale,Color colorBack) {
			EnumUiColor uiColorText=DrawComboBoxBox(g,rectangle,isHover,isEnabled,scale,colorBack);
			DrawComboBoxFront(g,rectangle,isEnabled,isFocused,scale);
			return uiColorText;
		}

		///<summary>The combo box's fill and border (DrawComboBox without the chevron and the focus line), for a control that draws its text between
		///the two. Returns the token of the text.</summary>
		public static EnumUiColor DrawComboBoxBox(Graphics g,Rectangle rectangle,bool isHover,bool isEnabled,float scale,Color colorBack) {
			Color colorFill=colorBack.IsEmpty ? UiTheme.GetColor(EnumUiColor.SurfaceRaised) : colorBack;
			UiControlPaint.Fill(g,colorFill,rectangle,UiTheme.CORNER_CONTROL,scale);
			EnumUiColor uiColorBorder=!isEnabled ? EnumUiColor.Stroke : (isHover ? EnumUiColor.StrokeStrongHover : EnumUiColor.StrokeStrong);
			UiControlPaint.Outline(g,uiColorBorder,rectangle,UiTheme.GetLineWidth(scale),UiTheme.CORNER_CONTROL,scale);
			return isEnabled ? EnumUiColor.Text : EnumUiColor.TextDisabled;
		}

		///<summary>The combo box's chevron and, when isFocused, its focus line (the rest of DrawComboBox).</summary>
		public static void DrawComboBoxFront(Graphics g,Rectangle rectangle,bool isEnabled,bool isFocused,float scale) {
			DrawChevron(g,rectangle,isEnabled ? EnumUiColor.Text : EnumUiColor.TextDisabled,scale);
			if(isFocused && isEnabled) {
				DrawComboFocusLine(g,rectangle,scale);
			}
		}

		///<summary>The left edge of the chevron's area at the right of a combo box (today's text mask, 15 px at 96 DPI from the right edge, less the
		///old 1 px outline): the text is clipped there, so it is cut where it was cut before.</summary>
		public static int GetComboTextRight(Rectangle rectangle,float scale) {
			return rectangle.Right-1-UiTheme.ScaleI(15,scale);
		}

		///<summary>The chevron's square: SIZE_CHEVRON at the scale, INSET_CHEVRON from the right edge, centred top to bottom, on whole pixels.</summary>
		public static Rectangle GetChevronRectangle(Rectangle rectangle,float scale) {
			int size=UiTheme.ScaleI(UiTheme.SIZE_CHEVRON,scale);
			return new Rectangle(rectangle.Right-UiTheme.ScaleI(INSET_CHEVRON,scale)-size,rectangle.Y+(rectangle.Height-size)/2,size,size);
		}

		///<summary>The Fluent down chevron (UiGlyph.ChevronDown) in its square at the right of the rectangle.</summary>
		public static void DrawChevron(Graphics g,Rectangle rectangle,EnumUiColor uiColor,float scale) {
			UiIcons.Draw(g,UiGlyph.ChevronDown,GetChevronRectangle(rectangle,scale),uiColor);
		}

		///<summary>The combo box's focus (section 3.6): a 2 px Accent line (GetFocusWidth) inside the bottom edge, inset by half the corner radius at
		///each end so it stays inside the rounded corners.</summary>
		public static void DrawComboFocusLine(Graphics g,Rectangle rectangle,float scale) {
			int width=UiTheme.GetFocusWidth(scale);
			int inset=UiTheme.GetCornerRadius(UiTheme.CORNER_CONTROL,scale)/2;
			Rectangle rectangleLine=new Rectangle(rectangle.X+inset,rectangle.Bottom-width,rectangle.Width-2*inset,width);
			UiControlPaint.Fill(g,EnumUiColor.Accent,rectangleLine,UiTheme.CORNER_SQUARE,scale);
		}
		#endregion Combo box

		#region List
		///<summary>A list's border (section 3.6): square, 1 px (GetLineWidth) StrokeStrong inside the rectangle, Stroke when disabled.</summary>
		public static void DrawListBorder(Graphics g,Rectangle rectangle,bool isEnabled,float scale) {
			UiControlPaint.Outline(g,isEnabled ? EnumUiColor.StrokeStrong : EnumUiColor.Stroke,rectangle,UiTheme.GetLineWidth(scale),UiTheme.CORNER_SQUARE,scale);
		}

		///<summary>The fill of a list item (section 3.6): colorSelectedBack (empty: AccentSubtle) when selected, RowHover on hover, colorBack (empty:
		///SurfaceRaised) otherwise.</summary>
		public static Color GetItemFill(bool isSelected,bool isHover,Color colorBack,Color colorSelectedBack) {
			if(isSelected) {
				return colorSelectedBack.IsEmpty ? UiTheme.GetColor(EnumUiColor.AccentSubtle) : colorSelectedBack;
			}
			if(isHover) {
				return UiTheme.GetColor(EnumUiColor.RowHover);
			}
			return colorBack.IsEmpty ? UiTheme.GetColor(EnumUiColor.SurfaceRaised) : colorBack;
		}

		///<summary>A list item's background in its rectangle (GetItemFill), and the selection bar when selected (DrawSelectionBar). Returns the token of
		///the item's text: TextDisabled when the list is disabled, TextSelected when selected, Text otherwise.</summary>
		public static EnumUiColor DrawListItem(Graphics g,Rectangle rectangleItem,bool isSelected,bool isHover,bool hasFocus,bool isEnabled,float scale,
			Color colorBack,Color colorSelectedBack)
		{
			UiControlPaint.Fill(g,GetItemFill(isSelected,isHover && isEnabled,colorBack,colorSelectedBack),rectangleItem,UiTheme.CORNER_SQUARE,scale);
			if(isSelected) {
				DrawSelectionBar(g,rectangleItem,hasFocus && isEnabled,scale);
			}
			if(!isEnabled) {
				return EnumUiColor.TextDisabled;
			}
			return isSelected ? EnumUiColor.TextSelected : EnumUiColor.Text;
		}

		///<summary>The selection bar's rectangle (section 3.2): the item's first GetFocusWidth pixels, its full height.</summary>
		public static Rectangle GetSelectionBarRectangle(Rectangle rectangleItem,float scale) {
			return new Rectangle(rectangleItem.X,rectangleItem.Y,UiTheme.GetFocusWidth(scale),rectangleItem.Height);
		}

		///<summary>The selection bar (section 3.2): Accent while the list has focus, SelectionBarNoFocus without, in the item's first 2 px (scaled).</summary>
		public static void DrawSelectionBar(Graphics g,Rectangle rectangleItem,bool hasFocus,float scale) {
			UiControlPaint.Fill(g,hasFocus ? EnumUiColor.Accent : EnumUiColor.SelectionBarNoFocus,GetSelectionBarRectangle(rectangleItem,scale),UiTheme.CORNER_SQUARE,scale);
		}

		///<summary>The focused row (section 3.6, the one the arrow keys move): a 1 px (GetLineWidth) Accent outline inside the item.</summary>
		public static void DrawFocusedItem(Graphics g,Rectangle rectangleItem,float scale) {
			UiControlPaint.Outline(g,EnumUiColor.Accent,rectangleItem,UiTheme.GetLineWidth(scale),UiTheme.CORNER_SQUARE,scale);
		}
		#endregion List
	}
}
