// The new look's pieces for the WPF imaging controls (docs/specs/ui-refresh-plan.md, sections 3.2 and 3.6; package UI-4c-1,
// docs/port-notes/UI-4c.md): the token brushes their XAML takes ({x:Static local:UiLookImaging.BrushSurface}), the Fluent minus and plus of the zoom
// slider, and the snap areas of the floating image windows' menu. The tokens of the handles, cells and value box are UiSliderPaint's (CodeBase), shared
// with the WinForms controls; UiLook (UI-4a) and UiLookLists (UI-4b-1) have the brushes of the core controls and the lists.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Windows.Media;
using System.Windows.Shapes;
using CodeBase;

namespace WpfControls.UI {
	///<summary>The token brushes, glyphs and shapes of the new look for the WPF imaging controls.</summary>
	public static class UiLookImaging {
		public static SolidColorBrush BrushGridline => UiTheme.GetBrushWpf(EnumUiColor.Gridline);
		public static SolidColorBrush BrushStroke => UiTheme.GetBrushWpf(EnumUiColor.Stroke);
		public static SolidColorBrush BrushStrokeStrong => UiTheme.GetBrushWpf(EnumUiColor.StrokeStrong);
		public static SolidColorBrush BrushSurface => UiTheme.GetBrushWpf(EnumUiColor.Surface);
		public static SolidColorBrush BrushSurfaceRaised => UiTheme.GetBrushWpf(EnumUiColor.SurfaceRaised);
		public static SolidColorBrush BrushSurfaceSubtle => UiTheme.GetBrushWpf(EnumUiColor.SurfaceSubtle);
		public static SolidColorBrush BrushText => UiTheme.GetBrushWpf(EnumUiColor.Text);
		///<summary>The Fluent subtract glyph for a SIZE_GLYPH slot: the zoom slider's minus.</summary>
		public static Geometry GeometryMinus => UiIcons.GetGeometryWpf(UiGlyph.Subtract,UiSliderPaint.SIZE_GLYPH);

		///<summary>The Fluent add glyph (EnumIcons.Add's design) for a SIZE_GLYPH slot: the zoom slider's plus.</summary>
		public static Geometry GeometryPlus {
			get {
				IconDesign iconDesign=UiSliderPaint.GetAddDesign(UiSliderPaint.SIZE_GLYPH);
				if(iconDesign==null) {
					return Geometry.Empty;
				}
				return UiIcons.GetGeometryWpf(iconDesign,UiSliderPaint.SIZE_GLYPH);
			}
		}

		///<summary>A snap area of the floating image windows' menu (section 3.6, as a toggle): SurfaceSubtle with a 1 px StrokeStrong outline at rest,
		///AccentSubtle with an Accent outline under the mouse.</summary>
		public static void SetSnapArea(Path path,bool isHover) {
			path.Fill=UiTheme.GetBrushWpf(isHover ? EnumUiColor.AccentSubtle : EnumUiColor.SurfaceSubtle);
			path.Stroke=UiTheme.GetBrushWpf(isHover ? EnumUiColor.Accent : EnumUiColor.StrokeStrong);
		}
	}
}
