// The font role tokens and the mapping (package UI-2a-1, docs/specs/ui-refresh-plan.md sections 3.4, 5.1, 5.2, 5.4 and 9, docs/port-notes/UI-2a.md):
// every token holds today's font with its UI-2b value written down beside it; the test-only override UiTheme.SetFontForTest sets them all; the
// mapping returns every font unchanged with today's tokens (the designer fonts at 8 and 8.5 pt included) and, with the override set to Segoe UI,
// follows the rule of 3.4 in all three places LayoutManagerForms records a control (a window's designer controls, an Add'ed control, a
// preference page and ControlImages through AddUnscaled); the underlined labels of FormInsBenefits still draw without underline; content keeps
// its fonts (a marked panel, a sheet field of FormSheetFillEdit and a composite one's children, the perio chart's picture, the patient dashboard at 100% and one zoom step,
// the tooth chart's pinned font); a late MarkContent throws. No database (the no-database guard refuses every connection), no window shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	[TestClass]
	public class FontTokenTests {
		[TestCleanup]
		public void TestCleanup() {
			UiTheme.SetFontForTest(null);
		}

		#region Tokens
		[TestMethod]
		public void Tokens_HoldTodaysFonts() {
			Assert.AreSame(UiFontOption.Today,UiTheme.FontOption);
			Assert.IsTrue(UiTheme.IsFontMappingIdentity);
			AssertFont(UiTheme.FontBody,"Microsoft Sans Serif",8.25f,FontStyle.Regular);
			string sansSerif=FontFamily.GenericSansSerif.Name;
			AssertFont(UiTheme.FontGrid,sansSerif,8.5f,FontStyle.Regular);
			AssertFont(UiTheme.FontGridBold,sansSerif,8.5f,FontStyle.Bold);
			AssertFont(UiTheme.FontGridUnderline,sansSerif,8.5f,FontStyle.Underline);
			AssertFont(UiTheme.FontGridUnderlineBold,sansSerif,8.5f,FontStyle.Underline|FontStyle.Bold);
			AssertFont(UiTheme.FontButtonPanel,sansSerif,8.5f,FontStyle.Regular);
			AssertFont(UiTheme.FontTitle,sansSerif,10f,FontStyle.Bold);
			AssertFont(UiTheme.FontModuleCaption,"Arial",8f,FontStyle.Regular);
			Assert.AreEqual("Microsoft Sans Serif",UiTheme.FontFamilyName);
			//The sites that go through the body token: LayoutManagerForms.FontInitial is the token itself, as one shared instance like before.
			Assert.AreSame(UiTheme.FontBody,LayoutManagerForms.FontInitial);
			using(Font fontOld=new Font("Microsoft Sans Serif",8.25f)) {
				Assert.AreEqual(fontOld,UiTheme.FontBody,"The body token equals the literal FormODBase, FormFrame and FontInitial had.");
			}
		}

		[TestMethod]
		public void Tokens_TheirUI2bValues_AreWrittenDown() {
			UiFontOption ui2b=UiFontOption.UI2b;
			Assert.AreEqual("Segoe UI",ui2b.FamilyBody);
			Assert.AreEqual(9f,ui2b.SizeBody);
			Assert.AreEqual("Segoe UI",ui2b.FamilyGrid);
			Assert.AreEqual(9f,ui2b.SizeGrid,"UQ-2: grid cells at the body size.");
			Assert.AreEqual(9f,ui2b.SizeButtonPanel);
			Assert.AreEqual("Segoe UI Semibold",ui2b.FamilyTitle);
			Assert.AreEqual(10f,ui2b.SizeTitle);
			Assert.AreEqual(FontStyle.Regular,ui2b.StyleTitle);
			Assert.AreEqual("Segoe UI",ui2b.FamilyModuleCaption);
			Assert.AreEqual(8f,ui2b.SizeModuleCaption);
			Assert.AreEqual("Segoe UI",ui2b.FamilyName);
			Assert.AreEqual(8.25f,UiFontOption.Compact.SizeBody,"UQ-1's other option.");
			Assert.AreEqual(UiTheme.FONT_FAMILY,ui2b.FamilyBody,"The design's values (UiTheme.Metrics) and the UI-2b values are one set.");
		}

		[TestMethod]
		public void SetFontForTest_SetsEveryToken_AndNullGivesTodaysBack() {
			UiTheme.SetFontForTest(UiFontOption.UI2b);
			Assert.AreSame(UiFontOption.UI2b,UiTheme.FontOption);
			Assert.IsFalse(UiTheme.IsFontMappingIdentity);
			AssertFont(UiTheme.FontBody,"Segoe UI",9f,FontStyle.Regular);
			AssertFont(UiTheme.FontGrid,"Segoe UI",9f,FontStyle.Regular);
			AssertFont(UiTheme.FontGridBold,"Segoe UI",9f,FontStyle.Bold);
			AssertFont(UiTheme.FontGridUnderline,"Segoe UI",9f,FontStyle.Underline);
			AssertFont(UiTheme.FontGridUnderlineBold,"Segoe UI",9f,FontStyle.Underline|FontStyle.Bold);
			AssertFont(UiTheme.FontButtonPanel,"Segoe UI",9f,FontStyle.Regular);
			AssertFont(UiTheme.FontTitle,"Segoe UI Semibold",10f,FontStyle.Regular);
			AssertFont(UiTheme.FontModuleCaption,"Segoe UI",8f,FontStyle.Regular);
			Assert.AreEqual("Segoe UI",UiTheme.FontFamilyName);
			Assert.AreSame(UiTheme.FontBody,LayoutManagerForms.FontInitial);
			UiTheme.SetFontForTest(UiFontOption.Compact);
			AssertFont(UiTheme.FontBody,"Segoe UI",8.25f,FontStyle.Regular);
			UiTheme.SetFontForTest(null);
			Assert.AreSame(UiFontOption.Today,UiTheme.FontOption);
			AssertFont(UiTheme.FontBody,"Microsoft Sans Serif",8.25f,FontStyle.Regular);
			Assert.IsTrue(UiTheme.IsTestProcess(),"This process is a test process.");
		}

		[TestMethod]
		public void SetFontForTest_RefusesOutsideATestProcess() {
			//The refusal is the check in SetFontForTest; the program's processes load no test framework and are not test hosts.
			string source=File.ReadAllText(Path.Combine(RepoPaths.Root(),"CodeBase","UI","UiTheme.Fonts.cs"));
			Match match=Regex.Match(source,@"public static void SetFontForTest\(UiFontOption fontOption\) \{\s*if\(!IsTestProcess\(\)\) \{\s*throw new InvalidOperationException");
			Assert.IsTrue(match.Success,"SetFontForTest throws first unless IsTestProcess.");
		}

		[TestMethod]
		public void FormODBase_TakesTheBodyToken() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				using(TestWindowFonts window=new TestWindowFonts()) {
					Assert.AreEqual(UiTheme.FontBody,window.Font);
				}
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					using(TestWindowFonts window=new TestWindowFonts()) {
						AssertFont(window.Font,"Segoe UI",9f,FontStyle.Regular);
						Assert.AreNotSame(UiTheme.FontBody,window.Font,"Each window has its own copy, as before.");
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
			});
		}
		#endregion Tokens

		#region The rule
		[TestMethod]
		public void MapFont_TodaysTokens_ReturnsEveryDesignerFontUnchanged() {
			List<Font> listFonts=ReadDesignerFontsOfTheProgram();
			Assert.IsTrue(listFonts.Count>20,"Distinct designer fonts: "+listFonts.Count);
			Assert.IsTrue(listFonts.Any(x => x.Name=="Microsoft Sans Serif" && x.SizeInPoints==8f),"The designer fonts at 8 pt are among them.");
			Assert.IsTrue(listFonts.Any(x => x.Name=="Microsoft Sans Serif" && x.SizeInPoints==8.5f),"The designer fonts at 8.5 pt are among them.");
			foreach(Font font in listFonts) {
				Assert.AreSame(font,UiTheme.MapFont(font),"Today's tokens map "+font+" to itself.");
			}
			foreach(Font font in listFonts) {
				font.Dispose();
			}
		}

		[TestMethod]
		public void MapFont_TheOverrideSegoeUI_FollowsTheRuleOfSection34() {
			UiTheme.SetFontForTest(UiFontOption.UI2b);
			using(Font font825=new Font("Microsoft Sans Serif",8.25f,FontStyle.Bold|FontStyle.Underline))
			using(Font font8=new Font("Microsoft Sans Serif",8f))
			using(Font font85=new Font("Microsoft Sans Serif",8.5f,FontStyle.Italic))
			using(Font font10=new Font("Microsoft Sans Serif",10f,FontStyle.Bold))
			using(Font fontArial=new Font("Arial",8.25f,FontStyle.Bold|FontStyle.Italic))
			using(Font fontCourier=new Font("Courier New",8.25f)) {
				AssertFont(UiTheme.MapFont(font825),"Segoe UI",9f,FontStyle.Bold|FontStyle.Underline);
				AssertFont(UiTheme.MapFont(font8),"Segoe UI",9f,FontStyle.Regular);
				AssertFont(UiTheme.MapFont(font85),"Segoe UI",9f,FontStyle.Italic);
				AssertFont(UiTheme.MapFont(font10),"Segoe UI",10f,FontStyle.Bold);
				Assert.AreSame(fontArial,UiTheme.MapFont(fontArial));
				Assert.AreSame(fontCourier,UiTheme.MapFont(fontCourier));
				//The test-side swap of UI-0b and the product's mapping are one rule.
				foreach(Font font in new[] { font825,font8,font85,font10,fontArial,fontCourier }) {
					Font fontSwap=TextFitFontSwap.MapFont(font,"Segoe UI",9f);
					Font fontMapped=UiTheme.MapFont(font);
					Assert.AreEqual(fontSwap.Name+fontSwap.SizeInPoints+fontSwap.Style,fontMapped.Name+fontMapped.SizeInPoints+fontMapped.Style,font.ToString());
				}
			}
		}

		///<summary>Every distinct Microsoft Sans Serif font the designer files of the program's folders set (size and style).</summary>
		private static List<Font> ReadDesignerFontsOfTheProgram() {
			Regex regex=new Regex(@"new System\.Drawing\.Font\(""Microsoft Sans Serif"", ([\d.]+)F(?:, (System\.Drawing\.FontStyle\.\w+(?: \| System\.Drawing\.FontStyle\.\w+)*|\(\(System\.Drawing\.FontStyle\)\(\([^)]*\)\)\)))?");
			HashSet<string> setKeys=new HashSet<string>();
			List<Font> listFonts=new List<Font>();
			foreach(string folder in new[] { "OpenPmsDental","OpenPmsDentalBusiness","CodeBase","WpfControlsOD" }) {
				foreach(string path in Directory.EnumerateFiles(Path.Combine(RepoPaths.Root(),folder),"*.Designer.cs",SearchOption.AllDirectories)) {
					foreach(Match match in regex.Matches(File.ReadAllText(path))) {
						float size=float.Parse(match.Groups[1].Value,CultureInfo.InvariantCulture);
						FontStyle fontStyle=FontStyle.Regular;
						foreach(string name in new[] { "Bold","Italic","Underline","Strikeout" }) {
							if(match.Groups[2].Value.Contains(name)) {
								fontStyle|=(FontStyle)Enum.Parse(typeof(FontStyle),name);
							}
						}
						if(setKeys.Add(size.ToString(CultureInfo.InvariantCulture)+"/"+fontStyle)) {
							listFonts.Add(new Font("Microsoft Sans Serif",size,fontStyle));
						}
					}
				}
			}
			return listFonts;
		}
		#endregion The rule

		#region The three recording places
		[TestMethod]
		public void Mapping_TodaysTokens_AWindowsDesignerFonts_AreUnchanged() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				using(TestWindowContent window=new TestWindowContent()) {
					AssertFont(window.labelBold.Font,"Microsoft Sans Serif",8.25f,FontStyle.Bold);
					AssertFont(window.label8.Font,"Microsoft Sans Serif",8f,FontStyle.Regular);
					AssertFont(window.groupBox.Font,"Microsoft Sans Serif",8.5f,FontStyle.Bold);
					Assert.AreSame(window.groupBox.Font,window.labelInGroup.Font,"A child that inherits still inherits.");
					Assert.AreEqual(8f,GetFontSize96(window,window.label8),"Recorded at its own size: the 8 pt designer fonts keep their size until the flip.");
				}
			});
		}

		[TestMethod]
		public void Mapping_TheOverrideSegoeUI_AWindowsDesignerFonts_FollowTheToken() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					using(TestWindowContent window=new TestWindowContent()) {
						AssertFont(window.labelBold.Font,"Segoe UI",9f,FontStyle.Bold);
						AssertFont(window.label8.Font,"Segoe UI",9f,FontStyle.Regular);
						AssertFont(window.label10.Font,"Segoe UI",10f,FontStyle.Italic);
						AssertFont(window.groupBox.Font,"Segoe UI",9f,FontStyle.Bold);
						Assert.AreSame(window.groupBox.Font,window.labelInGroup.Font,"A child that inherits follows its parent's mapped font.");
						AssertFont(window.labelArial.Font,"Arial",8.25f,FontStyle.Bold);
						Assert.AreEqual(9f,GetFontSize96(window,window.labelBold),"Mapped before FontSize96 is read.");
						Assert.AreEqual(9f,GetFontSize96(window,window.labelInGroup),"The inheriting child records its parent's mapped size.");
						Assert.AreEqual(10f,GetFontSize96(window,window.label10));
						//The marked panel is content: it and everything inside keep their fonts.
						AssertFont(window.labelInContent.Font,"Microsoft Sans Serif",8.25f,FontStyle.Regular);
						Assert.AreEqual(8.25f,GetFontSize96(window,window.labelInContent));
						//The rebuild is not changed: after the layout (as FormODBase.OnLoad does it) a label has the 0.96 factor and only Bold; the family
						//stays the mapped one.
						using(WindowBuilder.LoadWithoutShow(window,window.GetLayoutManager())) {
							AssertFont(window.labelBold.Font,"Segoe UI",9f*0.96f,FontStyle.Bold);
							AssertFont(window.label10.Font,"Segoe UI",10f*0.96f,FontStyle.Regular);
							AssertFont(window.groupBox.Font,"Segoe UI",9f,FontStyle.Bold);
							AssertFont(window.labelInContent.Font,"Microsoft Sans Serif",8.25f*0.96f,FontStyle.Regular);
						}
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
			});
		}

		[TestMethod]
		public void Mapping_TheOverrideSegoeUI_AnAddedControl_FollowsTheToken_AtItsZoomedSize() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					using(TestWindowContent window=new TestWindowContent())
					using(WindowBuilder.LoadWithoutShow(window,window.GetLayoutManager())) {
						LayoutManagerForms layoutManager=window.GetLayoutManager();
						//Add expects a font already scaled by the zoom: at zoom 120% Microsoft Sans Serif 8.25 Bold is 9.9 pt.
						layoutManager.ZoomTest=120;
						try {
							System.Windows.Forms.Label label=new System.Windows.Forms.Label { Name="labelAdded",Size=new Size(100,18),
								Font=new Font("Microsoft Sans Serif",layoutManager.ScaleFontODZoom(8.25f),FontStyle.Bold) };
							layoutManager.Add(label,window.PanelClient);
							AssertFont(label.Font,"Segoe UI",9f*1.2f,FontStyle.Bold);
							Assert.AreEqual(9f,GetFontSize96(window,label),0.01f,"Recorded at 96 dpi as the mapped size.");
							System.Windows.Forms.Label labelInContent=new System.Windows.Forms.Label { Name="labelAddedToContent",Size=new Size(100,18),
								Font=new Font("Microsoft Sans Serif",layoutManager.ScaleFontODZoom(8.25f)) };
							layoutManager.Add(labelInContent,window.panelContent);
							AssertFont(labelInContent.Font,"Microsoft Sans Serif",8.25f*1.2f,FontStyle.Regular);
						}
						finally {
							layoutManager.ZoomTest=0;
						}
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
			});
		}

		[TestMethod]
		public void Mapping_TheOverrideSegoeUI_APreferencePageThroughAddUnscaled_FollowsTheToken() {
			AssertHostedExplicitFontsFollowTheToken("OpenPmsDental.UserControlAccountRecAndRepCharges",
				@"OpenPmsDental\User Controls\Preferences\UserControlAccountRecAndRepCharges.Designer.cs");
		}

		[TestMethod]
		public void Mapping_TheOverrideSegoeUI_ControlImagesThroughAddUnscaled_FollowsTheToken() {
			Dictionary<string,float> dictDesignerFonts=ReadDesignerFonts(Path.Combine(RepoPaths.Root(),"OpenPmsDental","Main Modules","ControlImages.Designer.cs"));
			Assert.AreEqual(4,dictDesignerFonts.Count,"ControlImages has four explicit Microsoft Sans Serif designer fonts (section 5.1).");
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					//ControlImages throws when built without its constructor (UI-0b), so it is made with it, as FormOpenDental does, and added the way the
					//program adds it. Its constructor reads the program links (an empty list here: no database).
					Programs.FillCacheFromTable(OpenPmsDentalBusiness.Crud.ProgramCrud.ListToTable(new List<OpenPmsDentalBusiness.Program>()));
					using(HostFormOD hostFormOD=new HostFormOD(new Size(900,700)))
					using(WindowBuilder.LoadWithoutShow(hostFormOD,hostFormOD.GetLayoutManager())) {
						ControlImages controlImages=new ControlImages();
						controlImages.Size=new Size(900,700);
						controlImages.Dock=DockStyle.Fill;//as FormOpenDental adds it
						hostFormOD.GetLayoutManager().AddUnscaled(controlImages,hostFormOD.PanelClient);
						AssertExplicitFontsFollow(controlImages,dictDesignerFonts,isLaidOut:false);
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
			});
		}

		///<summary>Hosts the UserControl as the scan does (WindowBuilder: AddUnscaled in a test FormODBase), with the override set to Segoe UI 9
		///before the build, and checks every control with an explicit Microsoft Sans Serif designer font: Segoe UI at the rule's size, with the
		///0.96 factor for labels and radio buttons after the layout.</summary>
		private static void AssertHostedExplicitFontsFollowTheToken(string typeName,string designerFile) {
			Dictionary<string,float> dictDesignerFonts=ReadDesignerFonts(Path.Combine(RepoPaths.Root(),designerFile));
			Assert.IsTrue(dictDesignerFonts.Count>=5,"Explicit Microsoft Sans Serif fonts in "+designerFile+": "+dictDesignerFonts.Count);
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					ScanType scanType=WindowBuilder.GetScanTypes().Single(x => x.Type.FullName==typeName);
					Assert.AreEqual(BuildKind.UserControlAddUnscaled,scanType.Kind);
					using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
						AssertExplicitFontsFollow(builtWindow.Root,dictDesignerFonts,isLaidOut:true);
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
			});
		}

		private static void AssertExplicitFontsFollow(Control root,Dictionary<string,float> dictDesignerFonts,bool isLaidOut) {
			List<string> listProblems=new List<string>();
			foreach(KeyValuePair<string,float> pair in dictDesignerFonts) {
				Control control=root.Controls.Find(pair.Key,true).FirstOrDefault();
				if(control==null) {
					listProblems.Add(pair.Key+": not found");
					continue;
				}
				float sizeExpected=UiTheme.IsBodySize(pair.Value) ? 9f : pair.Value;
				if(isLaidOut) {
					sizeExpected=TextFitMeasurer.FontSizeUnderLayoutManager(control,sizeExpected);
				}
				if(control.Font.Name!="Segoe UI" || Math.Abs(control.Font.SizeInPoints-sizeExpected)>0.01f) {
					listProblems.Add(pair.Key+": "+control.Font.Name+" "+control.Font.SizeInPoints+", expected Segoe UI "+sizeExpected);
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems));
		}

		[TestMethod]
		public void FormInsBenefits_UnderlinedLabels_DrawWithoutUnderline_AsToday() {
			Dictionary<string,FontStyle> dictUnderlined=ReadDesignerFontStyles(Path.Combine(RepoPaths.Root(),"OpenPmsDental","Forms","FormInsBenefits.Designer.cs"))
				.Where(x => x.Value.HasFlag(FontStyle.Underline)).ToDictionary(x => x.Key,x => x.Value);
			Assert.IsTrue(dictUnderlined.Count>=5,"Underlined designer labels in FormInsBenefits: "+dictUnderlined.Count);
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				ScanType scanType=WindowBuilder.GetScanTypes().Single(x => x.Type.FullName=="OpenPmsDental.FormInsBenefits");
				foreach(UiFontOption uiFontOption in new[] { UiFontOption.Today,UiFontOption.UI2b }) {
					UiTheme.SetFontForTest(uiFontOption);
					try {
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							builtWindow.LayOutAt(1f);
							foreach(string name in dictUnderlined.Keys) {
								Control control=builtWindow.Root.Controls.Find(name,true).Single();
								Assert.IsInstanceOfType(control,typeof(System.Windows.Forms.Label),name);
								Assert.AreEqual(uiFontOption.FamilyBody,control.Font.Name,name+" ("+uiFontOption+")");
								Assert.IsFalse(control.Font.Underline,name+" draws without underline, as today and in Open Dental ("+uiFontOption+").");
								Assert.AreEqual(uiFontOption.SizeBody*0.96f,control.Font.SizeInPoints,0.01f,name+" ("+uiFontOption+")");
							}
						}
					}
					finally {
						UiTheme.SetFontForTest(null);
					}
				}
			});
		}

		///<summary>The controls of a designer file whose Font is set to Microsoft Sans Serif, with the size.</summary>
		private static Dictionary<string,float> ReadDesignerFonts(string path) {
			Dictionary<string,float> dictFonts=new Dictionary<string,float>();
			foreach(Match match in Regex.Matches(File.ReadAllText(path),@"this\.(\w+)\.Font = new System\.Drawing\.Font\(""Microsoft Sans Serif"", ([\d.]+)F")) {
				dictFonts[match.Groups[1].Value]=float.Parse(match.Groups[2].Value,CultureInfo.InvariantCulture);
			}
			return dictFonts;
		}

		private static Dictionary<string,FontStyle> ReadDesignerFontStyles(string path) {
			Dictionary<string,FontStyle> dictStyles=new Dictionary<string,FontStyle>();
			foreach(Match match in Regex.Matches(File.ReadAllText(path),@"this\.(\w+)\.Font = new System\.Drawing\.Font\(""Microsoft Sans Serif"", [\d.]+F, ([^,]+),")) {
				FontStyle fontStyle=FontStyle.Regular;
				foreach(string name in new[] { "Bold","Italic","Underline" }) {
					if(match.Groups[2].Value.Contains(name)) {
						fontStyle|=(FontStyle)Enum.Parse(typeof(FontStyle),name);
					}
				}
				dictStyles[match.Groups[1].Value]=fontStyle;
			}
			return dictStyles;
		}
		#endregion The three recording places

		#region Content
		[TestMethod]
		public void MarkContent_AfterInitializeLayoutManager_Throws() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				InvalidOperationException exception=Assert.ThrowsExactly<InvalidOperationException>(() => new TestWindowLateMark());
				StringAssert.Contains(exception.Message,"before InitializeLayoutManager");
				//The check walks up from the panel to its window, so a nested panel is refused too.
				using(TestWindowContent window=new TestWindowContent()) {
					Assert.ThrowsExactly<InvalidOperationException>(() => LayoutManagerForms.MarkContent(window.labelInGroup));
				}
				//A control that is not in a built window yet can be marked.
				using(Panel panel=new Panel()) {
					LayoutManagerForms.MarkContent(panel);
					Assert.IsTrue(LayoutManagerForms.IsContent(panel,null));
				}
			});
		}

		[TestMethod]
		public void MarkContent_TheThreeDocumentWindows_MarkBeforeInitializeLayoutManager() {
			foreach(string[] arrayWindow in TextFitFontSwap.ArrayContentPanels) {
				string text=File.ReadAllText(Path.Combine(RepoPaths.Root(),"OpenPmsDental","Forms",arrayWindow[0]+".cs")).Replace("\r\n","\n");
				Match match=Regex.Match(text,"public "+arrayWindow[0]+@"\([^)]*\)\s*\{\s*(//[^\n]*\n\s*)*InitializeComponent\(\);\s*(//[^\n]*\n\s*)*LayoutManagerForms\.MarkContent\("
					+arrayWindow[1]+@"\);\s*InitializeLayoutManager\(\);");
				Assert.IsTrue(match.Success,arrayWindow[0]+" marks "+arrayWindow[1]+" between InitializeComponent and InitializeLayoutManager.");
			}
		}

		[TestMethod]
		public void ContentTypes_AreSkipped_AsTheScanSkipsThem() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				using(Panel panel=new Panel())
				using(Control toothChartWrapper=NewToothChartControl("ToothChartWrapper"))
				using(OpenPmsDental.UI.ODPrintPreviewControl oDPrintPreviewControl=new OpenPmsDental.UI.ODPrintPreviewControl()) {
					Assert.IsFalse(LayoutManagerForms.IsContent(panel,null));
					Assert.IsTrue(LayoutManagerForms.IsContent(toothChartWrapper,null));
					Assert.IsTrue(LayoutManagerForms.IsContent(oDPrintPreviewControl,null));
					Assert.IsTrue(LayoutManagerForms.IsContent(panel,toothChartWrapper),"Inside a content control: the walk goes on from the parent passed.");
				}
				//The scan's list of content types (TextFitFontSwap) is the product's.
				string source=File.ReadAllText(Path.Combine(RepoPaths.Root(),"OpenPmsDental","UI","BordersDpi","LayoutManagerForms.cs"));
				foreach(string typeName in TextFitFontSwap.ArrayContentTypeNames) {
					Assert.IsTrue(Regex.IsMatch(source,@"control is (\w+\.)*"+typeName+@"\b"),typeName+" is a content type of LayoutManagerForms.IsContentItself.");
				}
			});
		}

		[TestMethod]
		public void FormSheetFillEdit_AMicrosoftSansSerifSheetField_KeepsItsFont_WithTheOverride() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture=new CultureInfo("en-US");
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					using(FormSheetFillEdit formSheetFillEdit=new FormSheetFillEdit()) {
						Assert.AreEqual("Segoe UI",formSheetFillEdit.Font.Name,"The window itself takes the token.");
						Control panelMain=(Control)GetField(formSheetFillEdit,"panelMain");
						Assert.IsTrue(LayoutManagerForms.IsContent(panelMain,panelMain.Parent),"panelMain is marked in the constructor.");
						//A field text box as FormSheetFillEdit makes one (CreateFloatingTextBox, line 1414): the sheet field's stored font, added to panelMain
						//through LayoutManager.Add.
						SheetField sheetField=new SheetField { FieldType=SheetFieldType.InputField,FieldName="misc",FieldValue="Synthetic text",
							FontName="Microsoft Sans Serif",FontSize=8.25f,XPos=20,YPos=20,Width=200,Height=40,TextAlign=HorizontalAlignment.Left };
						ODtextBox textBox=new ODtextBox { Name="textSynthetic",Size=new Size(200,40),
							Font=new Font(sheetField.FontName,sheetField.FontSize,FontStyle.Regular) };
						LayoutManagerForms layoutManager=(LayoutManagerForms)GetField(formSheetFillEdit,"LayoutManager");
						layoutManager.Add(textBox,panelMain);
						AssertFont(textBox.Font,"Microsoft Sans Serif",8.25f,FontStyle.Regular);
						//The same field outside the marked panel would follow the token: the mark is what keeps it.
						ODtextBox textBoxOutside=new ODtextBox { Name="textOutside",Size=new Size(200,40),
							Font=new Font(sheetField.FontName,sheetField.FontSize,FontStyle.Regular) };
						layoutManager.Add(textBoxOutside,formSheetFillEdit);
						AssertFont(textBoxOutside.Font,"Segoe UI",9f,FontStyle.Regular);
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
			});
		}

		[TestMethod]
		public void FormSheetFillEdit_TheChildrenOfACompositeSheetField_KeepTheirFonts_WithTheOverride() {
			//Add records a control before it is parented, so the content decision is made once at the top control and passed down to its
			//children: a walk from a child would stop at the unparented top control and never reach the marked panelMain.
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture=new CultureInfo("en-US");
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					using(FormSheetFillEdit formSheetFillEdit=new FormSheetFillEdit()) {
						Control panelMain=(Control)GetField(formSheetFillEdit,"panelMain");
						LayoutManagerForms layoutManager=(LayoutManagerForms)GetField(formSheetFillEdit,"LayoutManager");
						//A screen tooth chart as FormSheetFillEdit adds one (the ScreenChart fields): its tooth labels are Microsoft Sans Serif 8.25 Bold in its designer.
						ScreenToothChart screenToothChart=new ScreenToothChart("0;;;;;;;;;;;;;;;;",false) { Size=new Size(500,200) };
						List<Label> listLabelsChart=GetLabels(screenToothChart);
						Assert.IsTrue(listLabelsChart.Count>=10,"The screen tooth chart has its tooth labels.");
						layoutManager.Add(screenToothChart,panelMain);
						foreach(Label label in listLabelsChart) {
							AssertFont(label.Font,"Microsoft Sans Serif",8.25f,FontStyle.Bold);
						}
						//A panel holding a label of its own font, as a signature box holds labelInvalidSig.
						Panel panel=new Panel { Name="panelSynthetic",Size=new Size(200,80) };
						Label labelInside=new Label { Name="labelSynthetic",Text="Synthetic",Size=new Size(150,20),
							Font=new Font("Microsoft Sans Serif",8.25f,FontStyle.Regular) };
						panel.Controls.Add(labelInside);
						layoutManager.Add(panel,panelMain);
						AssertFont(labelInside.Font,"Microsoft Sans Serif",8.25f,FontStyle.Regular);
						//The same composite outside the marked panel follows the token, child included: the mark is what keeps it.
						Panel panelOutside=new Panel { Name="panelOutside",Size=new Size(200,80) };
						Label labelOutside=new Label { Name="labelOutside",Text="Synthetic",Size=new Size(150,20),
							Font=new Font("Microsoft Sans Serif",8.25f,FontStyle.Regular) };
						panelOutside.Controls.Add(labelOutside);
						layoutManager.Add(panelOutside,formSheetFillEdit);
						AssertFont(labelOutside.Font,"Segoe UI",9f,FontStyle.Regular);
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
			});
		}

		private static List<Label> GetLabels(Control control) {
			List<Label> listLabels=new List<Label>();
			foreach(Control controlChild in control.Controls) {
				if(controlChild is Label label) {
					listLabels.Add(label);
				}
				listLabels.AddRange(GetLabels(controlChild));
			}
			return listLabels;
		}

		[TestMethod]
		public void ContrPerio_APerioChartPicture_IsUnchanged_WithTheOverride() {
			byte[] arrayToday=null;
			byte[] arrayOverride=null;
			byte[] arrayHosted=null;
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture=new CultureInfo("en-US");
				FillPerioPrefs();
				arrayToday=DrawPerioChart(null);
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				DefaultFontOverride.Set(new Font("Segoe UI",9f),1f);
				try {
					arrayOverride=DrawPerioChart(null);
					//In a window with the token font, added the way a window adds a control: the pin holds what the window would give it.
					using(HostFormOD hostFormOD=new HostFormOD(new Size(700,760)))
					using(WindowBuilder.LoadWithoutShow(hostFormOD,hostFormOD.GetLayoutManager())) {
						arrayHosted=DrawPerioChart(hostFormOD);
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
					DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				}
			});
			Assert.IsTrue(arrayToday.Length>0);
			CollectionAssert.AreEqual(arrayToday,arrayOverride,"The perio chart's picture on its own (the claim attachment's copy) is the same with Segoe UI.");
			CollectionAssert.AreEqual(arrayToday,arrayHosted,"The perio chart's picture in a Segoe UI window is the same.");
		}

		///<summary>A synthetic exam (no database): two teeth with probing, bleeding and a recession value, drawn as FormClaimEdit draws its copy.
		///Returns the picture's pixels.</summary>
		private static byte[] DrawPerioChart(HostFormOD hostFormOD) {
			ContrPerio contrPerio=new ContrPerio();
			try {
				contrPerio.Size=new Size(602,665+3*13);
				if(hostFormOD!=null) {
					hostFormOD.GetLayoutManager().Add(contrPerio,hostFormOD.PanelClient);
					Assert.AreEqual("Microsoft Sans Serif",contrPerio.Font.Name,"ContrPerio pins its font.");
				}
				//No procedures: IsImplant reads this list (cleared by the constructor) instead of the database.
				typeof(ContrPerio).GetField("_listProcedures",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,new List<Procedure>());
				PerioExam perioExam=new PerioExam { PerioExamNum=1,PatNum=1,ExamDate=new DateTime(2026,1,5),DateTMeasureEdit=new DateTime(2026,1,5,8,30,0) };
				contrPerio.ListPerioExams=new List<PerioExam> { perioExam };
				contrPerio.ListPerioMeasures=new List<PerioMeasure> {
					new PerioMeasure { PerioMeasureNum=1,PerioExamNum=1,SequenceType=PerioSequenceType.Probing,IntTooth=3,ToothValue=-1,MBvalue=3,Bvalue=5,DBvalue=4,MLvalue=2,Lvalue=3,DLvalue=6 },
					new PerioMeasure { PerioMeasureNum=2,PerioExamNum=1,SequenceType=PerioSequenceType.Probing,IntTooth=19,ToothValue=-1,MBvalue=2,Bvalue=2,DBvalue=3,MLvalue=4,Lvalue=5,DLvalue=3 },
					new PerioMeasure { PerioMeasureNum=3,PerioExamNum=1,SequenceType=PerioSequenceType.GingMargin,IntTooth=3,ToothValue=-1,MBvalue=1,Bvalue=2,DBvalue=1,MLvalue=-1,Lvalue=-1,DLvalue=-1 },
					new PerioMeasure { PerioMeasureNum=4,PerioExamNum=1,SequenceType=PerioSequenceType.Mobility,IntTooth=19,ToothValue=2,MBvalue=-1,Bvalue=-1,DBvalue=-1,MLvalue=-1,Lvalue=-1,DLvalue=-1 },
				};
				contrPerio.IdxExamSelected=0;
				contrPerio.LoadData();
				using(Bitmap bitmap=new Bitmap(602,665+3*13,PixelFormat.Format32bppArgb)) {
					bitmap.SetResolution(96,96);
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(Color.White);
						contrPerio.DrawChart(g);
					}
					return GetPixels(bitmap);
				}
			}
			finally {
				contrPerio.Dispose();
			}
		}

		///<summary>The preferences the perio chart reads, at Open Dental's defaults, in the cache only (no database).</summary>
		private static void FillPerioPrefs() {
			Dictionary<string,string> dictPrefs=new Dictionary<string,string> {
				{ "RandomPrimaryKeys","0" },{ "DxIcdVersion","9" },{ "UseInternationalToothNumbers","0" },{ "PerioRedProb","4" },{ "PerioRedMGJ","0" },
				{ "PerioRedGing","2" },{ "PerioRedCAL","6" },{ "PerioRedFurc","2" },{ "PerioRedMob","2" },{ "PerioSkipMissingTeeth","1" },
				{ "PerioTreatImplantsAsNotMissing","1" },{ "PerioAutoCalculateRecession","0" },
			};
			long prefNum=1;
			List<Pref> listPrefs=dictPrefs.Select(x => new Pref { PrefNum=prefNum++,PrefName=x.Key,ValueString=x.Value }).ToList();
			Prefs.FillCacheFromTable(OpenPmsDentalBusiness.Crud.PrefCrud.ListToTable(listPrefs));
		}

		[TestMethod]
		public void PatientDashboard_WidgetFields_KeepTheirFonts_WithTheOverride_At100AndOneZoomStep() {
			Dictionary<string,string> dictToday=null;
			Dictionary<string,string> dictOverride=null;
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture=new CultureInfo("en-US");
				dictToday=GetDashboardFonts();
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					dictOverride=GetDashboardFonts();
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
			});
			Assert.AreEqual(string.Join("; ",dictToday.Select(x => x.Key+"="+x.Value)),string.Join("; ",dictOverride.Select(x => x.Key+"="+x.Value)),
				"Every widget field keeps its font with Segoe UI, at 100% and at zoom 120%.");
			Assert.AreEqual("Microsoft Sans Serif 8.25 Regular",dictToday["100%/labelNoFontName"],"A label field without a font name: DashLabel's fallback family, at the sheet field's size.");
			Assert.AreEqual("Arial 10 Bold",dictToday["100%/labelArial"]);
			Assert.IsTrue(dictToday["120%/discount.label11"].StartsWith("Microsoft Sans Serif "),"DashIndividualDiscount's labels inherit the dashboard's pinned font.");
		}

		///<summary>The patient dashboard as FormOpenDental adds it (LayoutManager.Add in the window's constructor), with one widget holding two label
		///fields (one with a font name, one without) and a DashIndividualDiscount field, as UserControlDashboardWidget adds them (Controls.Add).
		///Returns every field's font at 100% and at zoom 120%.</summary>
		private static Dictionary<string,string> GetDashboardFonts() {
			Dictionary<string,string> dictFonts=new Dictionary<string,string>();
			using(HostFormOD hostFormOD=new HostFormOD(new Size(500,600)))
			using(WindowBuilder.LoadWithoutShow(hostFormOD,hostFormOD.GetLayoutManager())) {
				LayoutManagerForms layoutManager=hostFormOD.GetLayoutManager();
				UserControlDashboard userControlDashboard=new UserControlDashboard();
				userControlDashboard.Size=new Size(400,500);
				layoutManager.Add(userControlDashboard,hostFormOD.PanelClient);
				UserControlDashboardWidget userControlDashboardWidget=new UserControlDashboardWidget(new SheetDef { SheetDefNum=1,Width=300,Height=400 },layoutManager);
				userControlDashboard.Controls.Add(userControlDashboardWidget);
				DashLabel dashLabelNamed=new DashLabel { Name="labelArial" };
				DashLabel dashLabelUnnamed=new DashLabel { Name="labelNoFontName" };
				DashIndividualDiscount dashIndividualDiscount=new DashIndividualDiscount { Name="discount" };
				userControlDashboardWidget.Controls.Add(dashLabelNamed);
				userControlDashboardWidget.Controls.Add(dashLabelUnnamed);
				userControlDashboardWidget.Controls.Add(dashIndividualDiscount);
				SheetField sheetFieldNamed=new SheetField { FieldType=SheetFieldType.StaticText,FieldValue="Named",FontName="Arial",FontSize=10f,FontIsBold=true };
				SheetField sheetFieldUnnamed=new SheetField { FieldType=SheetFieldType.StaticText,FieldValue="Unnamed",FontName="",FontSize=8.25f };
				foreach(int zoom in new[] { 100,120 }) {
					layoutManager.ZoomTest=zoom==100 ? 0 : zoom;
					layoutManager.LayoutControlBoundsAndFonts(hostFormOD.PanelClient);
					foreach(Tuple<DashLabel,SheetField> tuple in new[] { Tuple.Create(dashLabelNamed,sheetFieldNamed),Tuple.Create(dashLabelUnnamed,sheetFieldUnnamed) }) {
						tuple.Item1.PassLayoutManager(layoutManager);
						tuple.Item1.RefreshData(null,tuple.Item2);
						tuple.Item1.RefreshView();
					}
					string prefix=zoom+"%/";
					dictFonts[prefix+"dashboard"]=Describe(userControlDashboard.Font);
					dictFonts[prefix+"widget"]=Describe(userControlDashboardWidget.Font);
					dictFonts[prefix+"labelArial"]=Describe(dashLabelNamed.Font);
					dictFonts[prefix+"labelNoFontName"]=Describe(dashLabelUnnamed.Font);
					foreach(Control control in UIHelper.GetAllControls(dashIndividualDiscount)) {
						dictFonts[prefix+"discount."+control.Name]=Describe(control.Font);
					}
				}
				layoutManager.ZoomTest=0;
			}
			return dictFonts;
		}

		[TestMethod]
		public void ToothChart_PinsItsFont_WithTheDefaultFontSegoeUI_AndTheOverride() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DefaultFontOverride.Set(new Font("Segoe UI",9f),1f);
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					//On its own (the 2D chart's fallback picture, ToothChartWrapper.GetSimple2DBitmap): today's font, not the default.
					using(Control toothChart2D=NewToothChartControl("ToothChart2D")) {
						AssertFont(toothChart2D.Font,"Microsoft Sans Serif",8.25f,FontStyle.Regular);
					}
					//An image wrapper made in code (SheetPrinting, ControlTreat, FormSheetDefEdit), and one in a Segoe UI window through LayoutManager, at
					//zoom 120%: the 2D chart follows the wrapper's zoomed size and keeps the family.
					using(Control toothChartWrapper=NewToothChartControl("ToothChartWrapper")) {
						AssertFont(toothChartWrapper.Font,"Microsoft Sans Serif",8.25f,FontStyle.Regular);
						Control control2D=toothChartWrapper.Controls.Cast<Control>().Single(x => x.GetType().Name=="ToothChart2D");
						AssertFont(control2D.Font,"Microsoft Sans Serif",8.25f,FontStyle.Regular);
						using(HostFormOD hostFormOD=new HostFormOD(new Size(500,400)))
						using(WindowBuilder.LoadWithoutShow(hostFormOD,hostFormOD.GetLayoutManager())) {
							LayoutManagerForms layoutManager=hostFormOD.GetLayoutManager();
							toothChartWrapper.Size=new Size(400,300);
							layoutManager.Add(toothChartWrapper,hostFormOD.PanelClient);
							layoutManager.ZoomTest=120;
							layoutManager.LayoutControlBoundsAndFonts(hostFormOD.PanelClient);
							AssertFont(toothChartWrapper.Font,"Microsoft Sans Serif",8.25f*1.2f,FontStyle.Regular);
							AssertFont(control2D.Font,"Microsoft Sans Serif",8.25f*1.2f,FontStyle.Regular);
							layoutManager.ZoomTest=0;
							hostFormOD.PanelClient.Controls.Remove(toothChartWrapper);
						}
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
					DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				}
			});
		}
		#endregion Content

		#region Helpers
		///<summary>A control of SparksToothChart, which this project does not reference (the program does, so the assembly is in the output folder).
		///</summary>
		private static Control NewToothChartControl(string typeName) {
			Assembly assembly=Assembly.Load("SparksToothChart");
			return (Control)Activator.CreateInstance(assembly.GetType("SparksToothChart."+typeName,throwOnError:true));
		}

		private static float GetFontSize96(FormODBase window,Control control) {
			object layoutManager=TextFitFontSwap.GetLayoutManager(window);
			object info=TextFitFontSwap.GetControl96Infos(layoutManager)[control];
			return (float)info.GetType().GetField("FontSize96").GetValue(info);
		}

		private static object GetField(object obj,string name) {
			for(Type type=obj.GetType();type!=null;type=type.BaseType) {
				FieldInfo fieldInfo=type.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly);
				if(fieldInfo!=null) {
					return fieldInfo.GetValue(obj);
				}
			}
			throw new MissingFieldException(obj.GetType().Name,name);
		}

		private static string Describe(Font font) {
			return font.Name+" "+Math.Round(font.SizeInPoints,2).ToString(CultureInfo.InvariantCulture)+" "+font.Style;
		}

		private static byte[] GetPixels(Bitmap bitmap) {
			BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
			try {
				byte[] arrayBytes=new byte[bitmapData.Stride*bitmap.Height];
				Marshal.Copy(bitmapData.Scan0,arrayBytes,0,arrayBytes.Length);
				return arrayBytes;
			}
			finally {
				bitmap.UnlockBits(bitmapData);
			}
		}

		private static void AssertFont(Font font,string family,float size,FontStyle fontStyle) {
			Assert.AreEqual(family,font.Name,"Family of "+font);
			Assert.AreEqual(size,font.SizeInPoints,0.01f,"Size of "+font);
			Assert.AreEqual(fontStyle,font.Style,"Style of "+font);
		}
		#endregion Helpers
	}

	///<summary>A FormODBase window for the mapping (UI-2a-1), written like designer code: Microsoft Sans Serif at 8.25 Bold, 8 and 10 Italic, an
	///Arial label, a group box with its own 8.5 Bold font and a child that inherits it, and a panel marked as content (MarkContent before
	///InitializeLayoutManager, as FormSheetFillEdit marks panelMain) with a label of its own font inside. Never shown.</summary>
	public class TestWindowContent : FormODBase {
		public System.Windows.Forms.Label labelBold;
		public System.Windows.Forms.Label label8;
		public System.Windows.Forms.Label label10;
		public System.Windows.Forms.Label labelArial;
		public System.Windows.Forms.GroupBox groupBox;
		public System.Windows.Forms.Label labelInGroup;
		public System.Windows.Forms.Panel panelContent;
		public System.Windows.Forms.Label labelInContent;

		public TestWindowContent() {
			InitializeComponent();
			LayoutManagerForms.MarkContent(panelContent);
			InitializeLayoutManager();
		}

		private void InitializeComponent() {
			labelBold=new System.Windows.Forms.Label();
			label8=new System.Windows.Forms.Label();
			label10=new System.Windows.Forms.Label();
			labelArial=new System.Windows.Forms.Label();
			groupBox=new System.Windows.Forms.GroupBox();
			labelInGroup=new System.Windows.Forms.Label();
			panelContent=new System.Windows.Forms.Panel();
			labelInContent=new System.Windows.Forms.Label();
			groupBox.SuspendLayout();
			panelContent.SuspendLayout();
			SuspendLayout();
			labelBold.Font=new Font("Microsoft Sans Serif",8.25F,FontStyle.Bold,GraphicsUnit.Point,0);
			labelBold.Location=new Point(10,10);
			labelBold.Name="labelBold";
			labelBold.Size=new Size(150,18);
			labelBold.Text="Bold";
			label8.Font=new Font("Microsoft Sans Serif",8F,FontStyle.Regular,GraphicsUnit.Point,0);
			label8.Location=new Point(10,30);
			label8.Name="label8";
			label8.Size=new Size(150,18);
			label8.Text="Eight";
			label10.Font=new Font("Microsoft Sans Serif",10F,FontStyle.Italic,GraphicsUnit.Point,0);
			label10.Location=new Point(10,50);
			label10.Name="label10";
			label10.Size=new Size(150,20);
			label10.Text="Ten italic";
			labelArial.Font=new Font("Arial",8.25F,FontStyle.Bold,GraphicsUnit.Point,0);
			labelArial.Location=new Point(10,72);
			labelArial.Name="labelArial";
			labelArial.Size=new Size(150,18);
			labelArial.Text="Arial";
			groupBox.Controls.Add(labelInGroup);
			groupBox.Font=new Font("Microsoft Sans Serif",8.5F,FontStyle.Bold,GraphicsUnit.Point,0);
			groupBox.Location=new Point(180,10);
			groupBox.Name="groupBox";
			groupBox.Size=new Size(180,60);
			groupBox.Text="Group";
			labelInGroup.Location=new Point(6,20);
			labelInGroup.Name="labelInGroup";
			labelInGroup.Size=new Size(150,18);
			labelInGroup.Text="Inherits";
			panelContent.Controls.Add(labelInContent);
			panelContent.Location=new Point(180,80);
			panelContent.Name="panelContent";
			panelContent.Size=new Size(180,60);
			labelInContent.Font=new Font("Microsoft Sans Serif",8.25F,FontStyle.Regular,GraphicsUnit.Point,0);
			labelInContent.Location=new Point(6,6);
			labelInContent.Name="labelInContent";
			labelInContent.Size=new Size(150,18);
			labelInContent.Text="A document's text";
			ClientSize=new Size(400,200);
			Controls.Add(labelBold);
			Controls.Add(label8);
			Controls.Add(label10);
			Controls.Add(labelArial);
			Controls.Add(groupBox);
			Controls.Add(panelContent);
			Name="TestWindowContent";
			groupBox.ResumeLayout(false);
			panelContent.ResumeLayout(false);
			ResumeLayout(false);
		}

		public LayoutManagerForms GetLayoutManager() {
			return LayoutManager;
		}
	}

	///<summary>A window that marks its panel after InitializeLayoutManager: the mistake MarkContent refuses (UI-2a-1).</summary>
	public class TestWindowLateMark : FormODBase {
		private System.Windows.Forms.Panel panel;

		public TestWindowLateMark() {
			panel=new System.Windows.Forms.Panel { Name="panel",Size=new Size(100,100) };
			Controls.Add(panel);
			Name="TestWindowLateMark";
			InitializeLayoutManager();
			LayoutManagerForms.MarkContent(panel);
		}
	}
}
