// The screen look of the WPF grids, the program's Grid and the DataGrid style of Themes/DataGridDict.xaml (UI refresh, package UI-5;
// docs/specs/ui-refresh-plan.md sections 3.2, 3.3 and 3.6, docs/port-notes/UI-5.md): the token brushes for their XAML
// ({x:Static local:GridLook.BrushHeader}), the overlay colours of hover and selection, and the title and header heights, which follow the
// font. The same look as GridOD's (OpenPmsDentalBusiness/UI/GridScreenLook.cs). Row colours set by the program or the office still win.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Windows;
using System.Windows.Media;
using CodeBase;

namespace WpfControls.UI {
	///<summary>The screen look of the WPF grids (UI-5).</summary>
	public static class GridLook {
		///<summary>Today's default selection colour of the WPF Grid (its ColorSelectedRow default).</summary>
		public static readonly Color ColorSelectedRowStock=Color.FromRgb(205,220,235);

		#region Brushes for XAML
		///<summary>The grid title: Accent.</summary>
		public static SolidColorBrush BrushTitle => UiTheme.GetBrushWpf(EnumUiColor.Accent);
		///<summary>The grid title's text: white on the accent.</summary>
		public static SolidColorBrush BrushTitleText => UiTheme.GetBrushWpf(EnumUiColor.TextOnAccent);
		///<summary>The header row: SurfaceSubtle.</summary>
		public static SolidColorBrush BrushHeader => UiTheme.GetBrushWpf(EnumUiColor.SurfaceSubtle);
		///<summary>The header's separators and bottom line, the outer border and the line beside the scroll bars: Stroke.</summary>
		public static SolidColorBrush BrushStroke => UiTheme.GetBrushWpf(EnumUiColor.Stroke);
		///<summary>The cells: SurfaceRaised.</summary>
		public static SolidColorBrush BrushCell => UiTheme.GetBrushWpf(EnumUiColor.SurfaceRaised);
		///<summary>The lines between rows and cells: Gridline.</summary>
		public static SolidColorBrush BrushGridline => UiTheme.GetBrushWpf(EnumUiColor.Gridline);
		///<summary>Text in the cells and headers: Text.</summary>
		public static SolidColorBrush BrushText => UiTheme.GetBrushWpf(EnumUiColor.Text);
		///<summary>A hovered row: RowHover.</summary>
		public static SolidColorBrush BrushRowHover => UiTheme.GetBrushWpf(EnumUiColor.RowHover);
		///<summary>A selected row: AccentSubtle.</summary>
		public static SolidColorBrush BrushSelection => UiTheme.GetBrushWpf(EnumUiColor.AccentSubtle);
		///<summary>The selection bar while the grid has focus, and the focused row's outline: Accent.</summary>
		public static SolidColorBrush BrushAccent => UiTheme.GetBrushWpf(EnumUiColor.Accent);
		///<summary>The selection bar while the grid does not have focus: SelectionBarNoFocus.</summary>
		public static SolidColorBrush BrushSelectionBarNoFocus => UiTheme.GetBrushWpf(EnumUiColor.SelectionBarNoFocus);
		///<summary>A see-through layer over a cell that shows RowHover over white (the DataGrid style's hover, which lies over the cell's content).</summary>
		public static SolidColorBrush BrushRowHoverOverlay => GetFrozenBrush(GetOverlayOnWhite(UiTheme.GetColorWpf(EnumUiColor.RowHover)));
		///<summary>A see-through layer over a cell that shows AccentSubtle over white (the DataGrid style's selection).</summary>
		public static SolidColorBrush BrushSelectionOverlay => GetFrozenBrush(GetOverlayOnWhite(UiTheme.GetColorWpf(EnumUiColor.AccentSubtle)));

		private static SolidColorBrush GetFrozenBrush(Color color) {
			SolidColorBrush solidColorBrush=new SolidColorBrush(color);
			solidColorBrush.Freeze();
			return solidColorBrush;
		}
		#endregion Brushes for XAML

		#region Colours
		///<summary>The selection's colour: AccentSubtle for a grid that keeps the stock ColorSelectedRow, the window's own colour otherwise.</summary>
		public static Color GetSelectionColor(Color colorSelectedRow) {
			if(colorSelectedRow.R==ColorSelectedRowStock.R && colorSelectedRow.G==ColorSelectedRowStock.G && colorSelectedRow.B==ColorSelectedRowStock.B
				&& colorSelectedRow.A==255)
			{
				return UiTheme.GetColorWpf(EnumUiColor.AccentSubtle);
			}
			return colorSelectedRow;
		}

		///<summary>A text colour: Text for the default black (the GridRow default), the program's or office's colour otherwise.</summary>
		public static Color GetTextColor(Color colorText) {
			if(colorText.A==255 && colorText.R==0 && colorText.G==0 && colorText.B==0) {
				return UiTheme.GetColorWpf(EnumUiColor.Text);
			}
			return colorText;
		}

		///<summary>The least opaque colour that gives color over white: the hover overlay, so a white row shows RowHover exactly and a coloured row a
		///shade darker. Opaque white gives transparent.</summary>
		public static Color GetOverlayOnWhite(Color color) {
			int min=Math.Min(color.R,Math.Min(color.G,color.B));
			double alpha=1-min/255.0;
			if(alpha<=0) {
				return Color.FromArgb(0,255,255,255);
			}
			byte r=(byte)Math.Max(0,Math.Min(255,Math.Round((color.R-min)/alpha)));
			byte g=(byte)Math.Max(0,Math.Min(255,Math.Round((color.G-min)/alpha)));
			byte b=(byte)Math.Max(0,Math.Min(255,Math.Round((color.B-min)/alpha)));
			return Color.FromArgb((byte)Math.Round(alpha*255),r,g,b);
		}
		#endregion Colours

		#region Sizes
		///<summary>The height in DIP of a line of Segoe UI at the size, rounded up: 16 at 11.5 (the grid's body), 19 at 14 (the title).</summary>
		public static int GetLineHeight(double fontSize) {
			return (int)Math.Ceiling(new FontFamily(UiTheme.FONT_FAMILY).LineSpacing*fontSize-0.001);
		}

		///<summary>The title's or the header's height in DIP from its font size (UI-5: they follow the font, inside the control): the line height less
		///1, so 18 for the 14 DIP title and 15 for the 11.5 DIP header, as before.</summary>
		public static int GetHeightBar(double fontSize) {
			return GetLineHeight(fontSize)-1;
		}

		///<summary>The selection bar's width in DIP at the DPI scale: round(2 x scale) device pixels (UiTheme.GetFocusWidth), so it lands on whole pixels.</summary>
		public static double GetSelectionBarWidth(double scale) {
			if(scale<=0) {
				scale=1;
			}
			return UiTheme.GetFocusWidth((float)scale)/scale;
		}

		///<summary>A 1 px line in DIP at the DPI scale: max(1, floor(scale)) device pixels (UiTheme.GetLineWidth).</summary>
		public static double GetLineWidth(double scale) {
			if(scale<=0) {
				scale=1;
			}
			return UiTheme.GetLineWidth((float)scale)/scale;
		}
		#endregion Sizes
	}
}
