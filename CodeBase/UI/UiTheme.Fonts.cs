// The font role tokens of the UI refresh (docs/specs/ui-refresh-plan.md, sections 3.4 and 5.1; package UI-2a-1, docs/port-notes/UI-2a.md).
// Every screen font the program sets goes through the token of its role: FontBody, FontGrid (and its bold and underline styles),
// FontButtonPanel, FontTitle, FontModuleCaption, and FontFamilyName for the window titles, whose size the frame computes. Until the flip
// (UI-2b) each token held exactly today's font (UiFontOption.Today), so routing a site through its token changed nothing on screen; since the
// flip (UI-2b, docs/port-notes/UI-2b.md) they hold the values of UiFontOption.UI2b, the program's fonts. MapFont is the rule of 3.4 that
// LayoutManagerForms applies to the designer fonts it records: the identity while FontBody is Microsoft Sans Serif (only with the test
// override set to Today since the flip).
// The test-only override SetFontForTest sets every token to an option's values (section 5.4: the scan, the galleries, the print guard and the
// bounds compare run the product's own font path in a font option, Today among them for the before set). It refuses outside a test process.
// System.Drawing only, C# 7.3: RdlDesign can compile this file in as a linked file (section 3.6); a NoHq test checks it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;

namespace CodeBase {
	///<summary>A set of values for the font role tokens (section 3.4): Today (what the program showed until UI-2b; the before set of the scan,
	///the galleries and the print guard), UI2b (the program's fonts since the flip, the owner's UQ-1 and UQ-2 answers of 2026-11-17), or Compact
	///(UQ-1's other option, kept for the galleries). Immutable.</summary>
	public sealed class UiFontOption {
		///<summary>Short name for files and reports: today, segoe9, segoe825.</summary>
		public readonly string Name;
		///<summary>Body: labels, inputs, buttons, menus; the window's own font (FormODBase, FormFrame, LayoutManagerForms.FontInitial).</summary>
		public readonly string FamilyBody;
		public readonly float SizeBody;
		///<summary>Grid cells and headers (GridOD), in Regular, Bold, Underline and Underline Bold.</summary>
		public readonly string FamilyGrid;
		public readonly float SizeGrid;
		///<summary>Button-panel items (ODButtonPanel).</summary>
		public readonly string FamilyButtonPanel;
		public readonly float SizeButtonPanel;
		///<summary>Self-drawn titles: the grid title and group titles drawn in code.</summary>
		public readonly string FamilyTitle;
		public readonly float SizeTitle;
		public readonly FontStyle StyleTitle;
		///<summary>The module bar's captions.</summary>
		public readonly string FamilyModuleCaption;
		public readonly float SizeModuleCaption;
		///<summary>The window titles' family (FormODBase, FormFrame); the frame computes their size. Also the family of a code font site whose
		///size fits no role (section 5.1).</summary>
		public readonly string FamilyName;

		public UiFontOption(string name,string familyBody,float sizeBody,string familyGrid,float sizeGrid,string familyButtonPanel,float sizeButtonPanel,
			string familyTitle,float sizeTitle,FontStyle styleTitle,string familyModuleCaption,float sizeModuleCaption,string familyName)
		{
			Name=name;
			FamilyBody=familyBody;
			SizeBody=sizeBody;
			FamilyGrid=familyGrid;
			SizeGrid=sizeGrid;
			FamilyButtonPanel=familyButtonPanel;
			SizeButtonPanel=sizeButtonPanel;
			FamilyTitle=familyTitle;
			SizeTitle=sizeTitle;
			StyleTitle=styleTitle;
			FamilyModuleCaption=familyModuleCaption;
			SizeModuleCaption=sizeModuleCaption;
			FamilyName=familyName;
		}

		///<summary>Today's fonts (section 3.4, "Today"): Microsoft Sans Serif 8.25 (body); sans serif 8.5 (grid, button panel); sans serif 10 Bold
		///(titles); Arial 8 (module captions); Microsoft Sans Serif (window titles). "sans serif" is FontFamily.GenericSansSerif, as the sites used
		///it. The program held these until UI-2b; since the flip only a test sets them (the before set).</summary>
		public static readonly UiFontOption Today=new UiFontOption("today",UiTheme.FONT_FAMILY_TODAY,8.25f,
			FontFamily.GenericSansSerif.Name,8.5f,FontFamily.GenericSansSerif.Name,8.5f,
			FontFamily.GenericSansSerif.Name,10f,FontStyle.Bold,"Arial",8f,UiTheme.FONT_FAMILY_TODAY);
		///<summary>The program's fonts since the flip (UI-2b, section 3.4's table): Segoe UI 9 (body, UQ-1; grid and button panel at the body
		///size, UQ-2); Segoe UI Semibold 10 (titles); Segoe UI 8 (module captions); Segoe UI (window titles).</summary>
		public static readonly UiFontOption UI2b=new UiFontOption("segoe9",UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY,
			UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY,UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY,
			UiTheme.FONT_FAMILY_SEMIBOLD,UiTheme.FONT_SIZE_TITLE,FontStyle.Regular,UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_MODULE_CAPTION,UiTheme.FONT_FAMILY);
		///<summary>UQ-1's other option, kept for the galleries and the scan: as UI2b with the body, grid and button panel at 8.25 pt.</summary>
		public static readonly UiFontOption Compact=new UiFontOption("segoe825",UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY_COMPACT,
			UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY_COMPACT,UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY_COMPACT,
			UiTheme.FONT_FAMILY_SEMIBOLD,UiTheme.FONT_SIZE_TITLE,FontStyle.Regular,UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_MODULE_CAPTION,UiTheme.FONT_FAMILY);

		public override string ToString() {
			return Name+" ("+FamilyBody+" "+SizeBody.ToString(System.Globalization.CultureInfo.InvariantCulture)+")";
		}
	}

	public static partial class UiTheme {
		///<summary>The token fonts of one option, made together and swapped as one reference, so a reader on another thread never sees a mix.</summary>
		private sealed class FontSet {
			public readonly UiFontOption Option;
			public readonly Font Body;
			public readonly Font Grid;
			public readonly Font GridBold;
			public readonly Font GridUnderline;
			public readonly Font GridUnderlineBold;
			public readonly Font ButtonPanel;
			public readonly Font Title;
			public readonly Font ModuleCaption;

			public FontSet(UiFontOption option) {
				Option=option;
				Body=new Font(option.FamilyBody,option.SizeBody);
				Grid=new Font(option.FamilyGrid,option.SizeGrid);
				GridBold=new Font(option.FamilyGrid,option.SizeGrid,FontStyle.Bold);
				GridUnderline=new Font(option.FamilyGrid,option.SizeGrid,FontStyle.Underline);
				GridUnderlineBold=new Font(option.FamilyGrid,option.SizeGrid,FontStyle.Underline|FontStyle.Bold);
				ButtonPanel=new Font(option.FamilyButtonPanel,option.SizeButtonPanel);
				Title=new Font(option.FamilyTitle,option.SizeTitle,option.StyleTitle);
				ModuleCaption=new Font(option.FamilyModuleCaption,option.SizeModuleCaption);
			}
		}

		///<summary>The program's font option: UI2b since the flip (UI-2b). SetFontForTest(null) gives it back.</summary>
		public static UiFontOption FontOptionProgram {
			get { return UiFontOption.UI2b; }
		}

		///<summary>The current token fonts: the program's (FontOptionProgram), unless a test set an option. Never disposed: windows and controls
		///hold them (a replaced set is left to the garbage collector).</summary>
		private static volatile FontSet _fontSet=new FontSet(FontOptionProgram);

		#region Font tokens (section 3.4)
		///<summary>The option the tokens hold: FontOptionProgram (UiFontOption.UI2b since the flip) in the program, unless a test set another.</summary>
		public static UiFontOption FontOption {
			get { return _fontSet.Option; }
		}

		///<summary>Body text: labels, inputs, buttons, menus, and the window's own font. Today Microsoft Sans Serif 8.25; UI-2b: Segoe UI 9.
		///A shared instance: never dispose it (a control that needs its own copy clones it).</summary>
		public static Font FontBody {
			get { return _fontSet.Body; }
		}

		///<summary>Grid cells and headers. Today sans serif 8.5; UI-2b: Segoe UI at the body size (UQ-2). Shared: never dispose it.</summary>
		public static Font FontGrid {
			get { return _fontSet.Grid; }
		}

		///<summary>FontGrid in Bold (bold cells, headers). Shared: never dispose it.</summary>
		public static Font FontGridBold {
			get { return _fontSet.GridBold; }
		}

		///<summary>FontGrid underlined (link cells). Shared: never dispose it.</summary>
		public static Font FontGridUnderline {
			get { return _fontSet.GridUnderline; }
		}

		///<summary>FontGrid underlined and Bold. Shared: never dispose it.</summary>
		public static Font FontGridUnderlineBold {
			get { return _fontSet.GridUnderlineBold; }
		}

		///<summary>Button-panel items. Today sans serif 8.5; UI-2b: Segoe UI at the body size. Shared: never dispose it.</summary>
		public static Font FontButtonPanel {
			get { return _fontSet.ButtonPanel; }
		}

		///<summary>Self-drawn titles (grid title, group titles drawn in code). Today sans serif 10 Bold; UI-2b: Segoe UI Semibold 10. Shared: never
		///dispose it.</summary>
		public static Font FontTitle {
			get { return _fontSet.Title; }
		}

		///<summary>The module bar's captions. Today Arial 8; UI-2b: Segoe UI 8. Shared: never dispose it.</summary>
		public static Font FontModuleCaption {
			get { return _fontSet.ModuleCaption; }
		}

		///<summary>The window titles' family, at the size the frame computes; also the family of a code font site whose size fits no role. Today
		///Microsoft Sans Serif; UI-2b: Segoe UI.</summary>
		public static string FontFamilyName {
			get { return _fontSet.Option.FamilyName; }
		}
		#endregion Font tokens (section 3.4)

		#region The mapping (sections 3.4 and 5.1)
		///<summary>True while the mapping changes nothing: FontBody's family is today's (Microsoft Sans Serif): in the program until
		///UI-2b, since then only with the test override set to UiFontOption.Today.</summary>
		public static bool IsFontMappingIdentity {
			get { return string.Equals(_fontSet.Option.FamilyBody,FONT_FAMILY_TODAY,StringComparison.OrdinalIgnoreCase); }
		}

		///<summary>True for a font the mapping changes: Microsoft Sans Serif while FontBody is in another family. Other families (Courier New,
		///Wingdings, Calibri, Arial) stay (3.4).</summary>
		public static bool IsFontMapped(Font font) {
			if(font==null || IsFontMappingIdentity) {
				return false;
			}
			return string.Equals(font.Name,FONT_FAMILY_TODAY,StringComparison.OrdinalIgnoreCase);
		}

		///<summary>The rule of 3.4 for a size of a Microsoft Sans Serif font: 8, 8.25 and 8.5 pt (today's body sizes) become FontBody's size; any
		///other size keeps its number (10 stays 10).</summary>
		public static float MapFontSize(float sizeInPoints) {
			if(IsBodySize(sizeInPoints)) {
				return _fontSet.Option.SizeBody;
			}
			return sizeInPoints;
		}

		///<summary>8, 8.25 and 8.5 pt, today's body sizes (3.4). A size that went through a float scale and back may differ in the last bits, so
		///the compare allows 0.01 pt.</summary>
		public static bool IsBodySize(float sizeInPoints) {
			return Math.Abs(sizeInPoints-8f)<0.01f || Math.Abs(sizeInPoints-8.25f)<0.01f || Math.Abs(sizeInPoints-8.5f)<0.01f;
		}

		///<summary>The mapping of section 5.1, by the rule of 3.4: a Microsoft Sans Serif font becomes FontBody's family, at FontBody's size for
		///8, 8.25 and 8.5 pt and at its own size otherwise, with its style kept in full (Bold, Italic, Underline, Strikeout). Returns the same
		///instance when nothing changes: always while FontBody is Microsoft Sans Serif (Today), and for every other family. The caller owns a
		///new font it gets back.</summary>
		public static Font MapFont(Font font) {
			if(!IsFontMapped(font)) {
				return font;
			}
			return new Font(_fontSet.Option.FamilyBody,MapFontSize(font.SizeInPoints),font.Style,GraphicsUnit.Point,font.GdiCharSet,font.GdiVerticalFont);
		}
		#endregion The mapping (sections 3.4 and 5.1)

		#region The pins (UI-2b)
		///<summary>The size of the body font before the flip (Microsoft Sans Serif 8.25, FONT_FAMILY_TODAY). A printout or office content that drew
		///with the window's inherited font before UI-2b keeps this font (the UI-2b pins, docs/port-notes/UI-2b.md): plan 5.2, UQ-5.</summary>
		public const float FONT_SIZE_PINNED=8.25f;

		///<summary>A new Microsoft Sans Serif 8.25 Regular: the pinned font of a printout or office content that inherited the body font before
		///UI-2b (a window's font that LayoutManager does not zoom, the exe's default font). Never a screen font. The caller owns it.</summary>
		public static Font NewFontPinned() {
			return new Font(FONT_FAMILY_TODAY,FONT_SIZE_PINNED);
		}

		///<summary>The pinned font (Microsoft Sans Serif) at the scale of an inherited font, in its style: a control's font is the body token at the
		///window's zoom (LayoutManager scales it), so the pin keeps that zoom. Its size is fontInherited's size times 8.25 over FontBody's size:
		///with today's tokens exactly fontInherited's size (8.25 at 100%, 9.9 at 120%); after the flip Segoe UI 9 at 120% (10.8) gives 9.9 again.
		///The caller owns it.</summary>
		public static Font NewFontPinned(Font fontInherited) {
			float size=fontInherited.SizeInPoints;
			float sizeBody=FontBody.SizeInPoints;
			if(sizeBody!=FONT_SIZE_PINNED) {
				size=size*FONT_SIZE_PINNED/sizeBody;
			}
			return new Font(FONT_FAMILY_TODAY,size,fontInherited.Style);
		}
		#endregion The pins (UI-2b)

		#region The test override (section 5.4)
		///<summary>Test only: sets every font token to the option's values (null: the program's, FontOptionProgram), so a test process runs the product's own font path in
		///a font option: FormODBase's and FormFrame's font, LayoutManagerForms.FontInitial, the mapping of the designer fonts and the code sites
		///that go through the tokens. Set it before any window is built; windows built before keep the fonts they took. Refuses (throws) outside
		///a test process, so the program can never change its fonts this way. The default font (Application.SetDefaultFont) is the test's own
		///business, as each exe's Main sets it.</summary>
		public static void SetFontForTest(UiFontOption fontOption) {
			if(!IsTestProcess()) {
				throw new InvalidOperationException("UiTheme.SetFontForTest is for test processes only.");
			}
			_fontSet=new FontSet(fontOption??FontOptionProgram);
		}

		///<summary>True in a test process: one that has loaded the MSTest framework, or whose entry assembly is a test host or a test project's
		///own process (TestProbeHost, a name that ends in Tests).</summary>
		public static bool IsTestProcess() {
			foreach(System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				string name=assembly.GetName().Name??"";
				if(name.StartsWith("Microsoft.VisualStudio.TestPlatform.TestFramework",StringComparison.OrdinalIgnoreCase)
					|| name.StartsWith("MSTest.TestFramework",StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			System.Reflection.Assembly assemblyEntry=System.Reflection.Assembly.GetEntryAssembly();
			string nameEntry=assemblyEntry==null ? "" : (assemblyEntry.GetName().Name??"");
			return nameEntry.StartsWith("testhost",StringComparison.OrdinalIgnoreCase)
				|| nameEntry.Equals("TestProbeHost",StringComparison.OrdinalIgnoreCase)
				|| nameEntry.EndsWith("Tests",StringComparison.OrdinalIgnoreCase);
		}
		#endregion The test override (section 5.4)
	}
}
