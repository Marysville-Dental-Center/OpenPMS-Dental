// The new look's pieces for the WPF combo boxes, pickers and lists (docs/specs/ui-refresh-plan.md, sections 3.2 and 3.6; package UI-4b-1,
// docs/port-notes/UI-4b.md): the key of the combo box's focus visual style in Themes/Generic.xaml (the 2 px Accent line inside the bottom edge),
// the token brushes the list styles take ({x:Static local:UiLookLists.BrushRowHover}), the Fluent chevrons of the tree's expander, and the
// selection colours the lists share (AccentSubtle with the 2 px selection bar: Accent while the list has keyboard focus, SelectionBarNoFocus
// without). UiLook (UI-4a) has the brushes and focus styles of the core controls.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Windows;
using System.Windows.Media;
using CodeBase;

namespace WpfControls.UI {
	///<summary>The token brushes, chevrons and focus style of the new look for the WPF combo boxes and lists.</summary>
	public static class UiLookLists {
		///<summary>The width of the selection bar at 96 DPI (section 3.2): the item's first 2 px.</summary>
		public const double WIDTH_SELECTION_BAR=2;
		///<summary>The key of the combo box's focus visual style (Themes/Generic.xaml): a 2 px Accent line inside the bottom edge, inset 2 px at each
		///end so it stays inside the 4 px corners.</summary>
		public static readonly ComponentResourceKey KeyFocusVisualStyleCombo=new ComponentResourceKey(typeof(UiLookLists),"FocusVisualStyleCombo");

		public static SolidColorBrush BrushAccentSubtle => UiTheme.GetBrushWpf(EnumUiColor.AccentSubtle);
		public static SolidColorBrush BrushRowHover => UiTheme.GetBrushWpf(EnumUiColor.RowHover);
		public static SolidColorBrush BrushSelectionBarNoFocus => UiTheme.GetBrushWpf(EnumUiColor.SelectionBarNoFocus);
		public static SolidColorBrush BrushStrokeStrong => UiTheme.GetBrushWpf(EnumUiColor.StrokeStrong);
		public static SolidColorBrush BrushSurfaceRaised => UiTheme.GetBrushWpf(EnumUiColor.SurfaceRaised);
		public static SolidColorBrush BrushText => UiTheme.GetBrushWpf(EnumUiColor.Text);
		public static SolidColorBrush BrushTextDisabled => UiTheme.GetBrushWpf(EnumUiColor.TextDisabled);
		public static SolidColorBrush BrushTextSecondary => UiTheme.GetBrushWpf(EnumUiColor.TextSecondary);
		public static SolidColorBrush BrushTextSelected => UiTheme.GetBrushWpf(EnumUiColor.TextSelected);
		///<summary>The Fluent right chevron for a 12 DIP slot (the tree's collapsed expander).</summary>
		public static Geometry GeometryChevronRight => UiIcons.GetGeometryWpf(UiGlyph.ChevronRight,12);
		///<summary>The Fluent down chevron for a 12 DIP slot (the combo box's drop-down arrow, the tree's expanded expander).</summary>
		public static Geometry GeometryChevronDown => UiIcons.GetGeometryWpf(UiGlyph.ChevronDown,12);

		///<summary>The fill of a list item (section 3.6): colorSelectedBack (null: AccentSubtle) when selected, RowHover on hover, colorBack (white:
		///SurfaceRaised) otherwise.</summary>
		public static SolidColorBrush GetItemFill(bool isSelected,bool isHover,Color colorBack,Color? colorSelectedBack) {
			if(isSelected) {
				if(colorSelectedBack==null) {
					return BrushAccentSubtle;
				}
				return new SolidColorBrush(colorSelectedBack.Value);
			}
			if(isHover) {
				return BrushRowHover;
			}
			return GetBack(colorBack);
		}

		///<summary>A control's background: SurfaceRaised while it is today's default white, the window's colour otherwise.</summary>
		public static SolidColorBrush GetBack(Color colorBack) {
			if(colorBack==Colors.White) {
				return UiTheme.GetBrushWpf(EnumUiColor.SurfaceRaised);
			}
			return new SolidColorBrush(colorBack);
		}

		///<summary>The selection bar (section 3.2): Accent while the list has keyboard focus, SelectionBarNoFocus without.</summary>
		public static SolidColorBrush GetSelectionBar(bool hasFocus) {
			return hasFocus ? UiTheme.GetBrushWpf(EnumUiColor.Accent) : BrushSelectionBarNoFocus;
		}

		///<summary>The text of a list item: TextDisabled when the list is disabled, TextSelected when selected, Text otherwise.</summary>
		public static SolidColorBrush GetItemText(bool isSelected,bool isEnabled) {
			if(!isEnabled) {
				return BrushTextDisabled;
			}
			return isSelected ? BrushTextSelected : BrushText;
		}
	}
}
