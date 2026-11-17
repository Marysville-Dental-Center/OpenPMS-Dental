// The colour tokens of the UI refresh (docs/specs/ui-refresh-plan.md, section 3.2; package UI-1, docs/port-notes/UI-1.md), light only, and their
// high-contrast partners. Windows Forms and System.Drawing only, C# 7.3: RdlDesign can compile this file in as a linked file (section 3.6).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;

namespace CodeBase {
	///<summary>The colour tokens of section 3.2 of the UI refresh plan, in the order the design sheet shows them. Never stored in the database. The
	///last five are role names for colours section 3.2 gives by use (white text on the accent, text on a selection, the title bar), so that each
	///has the right high-contrast partner.</summary>
	public enum EnumUiColor {
		///<summary>#FAFAFA. Window and panel background (replaces ColorOD.Background).</summary>
		Surface,
		///<summary>#FFFFFF. Inputs, grids, lists, menus, buttons, tooltips.</summary>
		SurfaceRaised,
		///<summary>#F3F3F3. Toolbars, module bar, grid header, tab strip.</summary>
		SurfaceSubtle,
		///<summary>#EBEBEB. Toolbar, module bar and menu items on hover.</summary>
		SubtleHover,
		///<summary>#E0E0E0. Toolbar, module bar and menu items pressed.</summary>
		SubtlePressed,
		///<summary>#F5F5F5. White buttons on hover; also the fill of a disabled button (section 3.2: no extra token).</summary>
		ButtonHover,
		///<summary>#EBEBEB. White buttons pressed.</summary>
		ButtonPressed,
		///<summary>#D1D1D1. Button border, container borders, group boxes, dividers (decorative).</summary>
		Stroke,
		///<summary>#8A8A8A. Outline of inputs, combo boxes, list boxes, check boxes; the selection bar without focus.</summary>
		StrokeStrong,
		///<summary>#616161. The same outlines on hover.</summary>
		StrokeStrongHover,
		///<summary>#E0E0E0. Grid lines, menu separators (decorative).</summary>
		Gridline,
		///<summary>#1F1F1F. All text.</summary>
		Text,
		///<summary>#5C5C5C. Hints, unselected tabs, captions.</summary>
		TextSecondary,
		///<summary>#A0A0A0. Disabled text and icons (exempt from the contrast rules).</summary>
		TextDisabled,
		///<summary>#3C5A96, the program's brand blue. Title bar, grid title, default button, focus ring, check box fill, selected tab line, the
		///selection bar with focus.</summary>
		Accent,
		///<summary>#324C80. Default button and accent states on hover.</summary>
		AccentHover,
		///<summary>#2A406C. Default button and accent states pressed; the title bar outline.</summary>
		AccentPressed,
		///<summary>#DCE6F5. Selected grid row, selected list item, toggled toolbar button.</summary>
		AccentSubtle,
		///<summary>#F0F4FA. Grid and list hover.</summary>
		RowHover,
		///<summary>#5370A6. Title bar minimise and maximise hover.</summary>
		CaptionHover,
		///<summary>#C42B1C. Close button hover, error text and the error toolbar state.</summary>
		Danger,
		///<summary>#FDE7E9. Error toolbar state background.</summary>
		DangerSubtle,
		///<summary>#9D5D00. Notify toolbar state text.</summary>
		Warning,
		///<summary>#FFF4CE. Notify toolbar state background.</summary>
		WarningSubtle,
		///<summary>#0F7B0F. The "complete" icon tint.</summary>
		Success,
		///<summary>#FFFFFF. Text and glyphs on Accent, AccentHover, AccentPressed, CaptionHover and Danger fills (white on the accent, section 3.2).</summary>
		TextOnAccent,
		///<summary>#1F1F1F (Text). Text on a selection (AccentSubtle): the same as Text in the light look, HighlightText in high contrast.</summary>
		TextSelected,
		///<summary>#3C5A96 (Accent). The title bar (section 3.2; Misc Colors 11 overrides it per item from UI-3 on).</summary>
		TitleBar,
		///<summary>#FFFFFF. The title bar's text and caption glyphs (Misc Colors 13 overrides it per item from UI-3 on).</summary>
		TitleBarText,
		///<summary>#2A406C (AccentPressed). The window's 1 px outline (Misc Colors 12 overrides it per item from UI-3 on).</summary>
		TitleBarOutline,
	}

	public static partial class UiTheme {
		#region Colour values
		///<summary>The light value of each token, indexed by EnumUiColor (section 3.2).</summary>
		private static readonly Color[] _arrayColorsLight=new Color[] {
			FromHex(0xFAFAFA),//Surface
			FromHex(0xFFFFFF),//SurfaceRaised
			FromHex(0xF3F3F3),//SurfaceSubtle
			FromHex(0xEBEBEB),//SubtleHover
			FromHex(0xE0E0E0),//SubtlePressed
			FromHex(0xF5F5F5),//ButtonHover
			FromHex(0xEBEBEB),//ButtonPressed
			FromHex(0xD1D1D1),//Stroke
			FromHex(0x8A8A8A),//StrokeStrong
			FromHex(0x616161),//StrokeStrongHover
			FromHex(0xE0E0E0),//Gridline
			FromHex(0x1F1F1F),//Text
			FromHex(0x5C5C5C),//TextSecondary
			FromHex(0xA0A0A0),//TextDisabled
			FromHex(0x3C5A96),//Accent
			FromHex(0x324C80),//AccentHover
			FromHex(0x2A406C),//AccentPressed
			FromHex(0xDCE6F5),//AccentSubtle
			FromHex(0xF0F4FA),//RowHover
			FromHex(0x5370A6),//CaptionHover
			FromHex(0xC42B1C),//Danger
			FromHex(0xFDE7E9),//DangerSubtle
			FromHex(0x9D5D00),//Warning
			FromHex(0xFFF4CE),//WarningSubtle
			FromHex(0x0F7B0F),//Success
			FromHex(0xFFFFFF),//TextOnAccent
			FromHex(0x1F1F1F),//TextSelected
			FromHex(0x3C5A96),//TitleBar
			FromHex(0xFFFFFF),//TitleBarText
			FromHex(0x2A406C),//TitleBarOutline
		};

		///<summary>The high-contrast partner of each token, indexed by EnumUiColor (section 3.2): surfaces and hover fills the window colour, text the
		///window text, borders the window frame, the accent, the selection and the role colours the highlight with the highlight text on them, the
		///hover outline the hot-track colour, disabled text the grey text, the title bar the active caption.</summary>
		private static readonly KnownColor[] _arrayKnownColorsHighContrast=new KnownColor[] {
			KnownColor.Window,//Surface
			KnownColor.Window,//SurfaceRaised
			KnownColor.Window,//SurfaceSubtle
			KnownColor.Window,//SubtleHover
			KnownColor.Window,//SubtlePressed
			KnownColor.Window,//ButtonHover
			KnownColor.Window,//ButtonPressed
			KnownColor.WindowFrame,//Stroke
			KnownColor.WindowFrame,//StrokeStrong
			KnownColor.HotTrack,//StrokeStrongHover
			KnownColor.GrayText,//Gridline
			KnownColor.WindowText,//Text
			KnownColor.WindowText,//TextSecondary
			KnownColor.GrayText,//TextDisabled
			KnownColor.Highlight,//Accent
			KnownColor.Highlight,//AccentHover
			KnownColor.Highlight,//AccentPressed
			KnownColor.Highlight,//AccentSubtle
			KnownColor.Window,//RowHover
			KnownColor.Highlight,//CaptionHover
			KnownColor.Highlight,//Danger
			KnownColor.Window,//DangerSubtle
			KnownColor.Highlight,//Warning
			KnownColor.Window,//WarningSubtle
			KnownColor.Highlight,//Success
			KnownColor.HighlightText,//TextOnAccent
			KnownColor.HighlightText,//TextSelected
			KnownColor.ActiveCaption,//TitleBar
			KnownColor.ActiveCaptionText,//TitleBarText
			KnownColor.WindowFrame,//TitleBarOutline
		};

		///<summary>The number of colour tokens.</summary>
		public static int CountColors {
			get { return _arrayColorsLight.Length; }
		}

		private static Color FromHex(int rgb) {
			return Color.FromArgb(255,(rgb>>16)&0xFF,(rgb>>8)&0xFF,rgb&0xFF);
		}
		#endregion Colour values

		#region Colour lookups
		///<summary>The token's colour now: its light value, or its SystemColors partner while Windows runs a high-contrast theme.</summary>
		public static Color GetColor(EnumUiColor uiColor) {
			return GetColor(uiColor,IsHighContrast);
		}

		///<summary>The token's colour in the light look or in the high-contrast mapping (with this computer's SystemColors), whatever Windows runs now.
		///For the design sheet and the tests.</summary>
		public static Color GetColor(EnumUiColor uiColor,bool isHighContrast) {
			if(isHighContrast) {
				return Color.FromKnownColor(GetKnownColorHighContrast(uiColor));
			}
			return GetColorLight(uiColor);
		}

		///<summary>The token's light value (section 3.2), whatever Windows runs.</summary>
		public static Color GetColorLight(EnumUiColor uiColor) {
			return _arrayColorsLight[(int)uiColor];
		}

		///<summary>The SystemColors partner of the token in a high-contrast theme (section 3.2).</summary>
		public static KnownColor GetKnownColorHighContrast(EnumUiColor uiColor) {
			return _arrayKnownColorsHighContrast[(int)uiColor];
		}

		///<summary>The token's light value as #RRGGBB.</summary>
		public static string GetHex(EnumUiColor uiColor) {
			Color color=GetColorLight(uiColor);
			return "#"+color.R.ToString("X2")+color.G.ToString("X2")+color.B.ToString("X2");
		}
		#endregion Colour lookups

		#region Colour tokens
		public static Color Surface { get { return GetColor(EnumUiColor.Surface); } }
		public static Color SurfaceRaised { get { return GetColor(EnumUiColor.SurfaceRaised); } }
		public static Color SurfaceSubtle { get { return GetColor(EnumUiColor.SurfaceSubtle); } }
		public static Color SubtleHover { get { return GetColor(EnumUiColor.SubtleHover); } }
		public static Color SubtlePressed { get { return GetColor(EnumUiColor.SubtlePressed); } }
		public static Color ButtonHover { get { return GetColor(EnumUiColor.ButtonHover); } }
		public static Color ButtonPressed { get { return GetColor(EnumUiColor.ButtonPressed); } }
		public static Color Stroke { get { return GetColor(EnumUiColor.Stroke); } }
		public static Color StrokeStrong { get { return GetColor(EnumUiColor.StrokeStrong); } }
		public static Color StrokeStrongHover { get { return GetColor(EnumUiColor.StrokeStrongHover); } }
		public static Color Gridline { get { return GetColor(EnumUiColor.Gridline); } }
		public static Color Text { get { return GetColor(EnumUiColor.Text); } }
		public static Color TextSecondary { get { return GetColor(EnumUiColor.TextSecondary); } }
		public static Color TextDisabled { get { return GetColor(EnumUiColor.TextDisabled); } }
		public static Color Accent { get { return GetColor(EnumUiColor.Accent); } }
		public static Color AccentHover { get { return GetColor(EnumUiColor.AccentHover); } }
		public static Color AccentPressed { get { return GetColor(EnumUiColor.AccentPressed); } }
		public static Color AccentSubtle { get { return GetColor(EnumUiColor.AccentSubtle); } }
		public static Color RowHover { get { return GetColor(EnumUiColor.RowHover); } }
		public static Color CaptionHover { get { return GetColor(EnumUiColor.CaptionHover); } }
		public static Color Danger { get { return GetColor(EnumUiColor.Danger); } }
		public static Color DangerSubtle { get { return GetColor(EnumUiColor.DangerSubtle); } }
		public static Color Warning { get { return GetColor(EnumUiColor.Warning); } }
		public static Color WarningSubtle { get { return GetColor(EnumUiColor.WarningSubtle); } }
		public static Color Success { get { return GetColor(EnumUiColor.Success); } }
		public static Color TextOnAccent { get { return GetColor(EnumUiColor.TextOnAccent); } }
		public static Color TextSelected { get { return GetColor(EnumUiColor.TextSelected); } }
		public static Color TitleBar { get { return GetColor(EnumUiColor.TitleBar); } }
		public static Color TitleBarText { get { return GetColor(EnumUiColor.TitleBarText); } }
		public static Color TitleBarOutline { get { return GetColor(EnumUiColor.TitleBarOutline); } }
		#endregion Colour tokens
	}
}
