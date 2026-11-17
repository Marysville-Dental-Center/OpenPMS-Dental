// The test-side font swap (package UI-0b, docs/specs/ui-refresh-plan.md sections 3.4 and 7, docs/port-notes/UI-0b.md): it gives the fonts of
// the rule of 3.4 on a test window, with LayoutManager's 0.96 factor and its Bold-only rebuild, sets the window's and the containers' fonts, and
// reaches the explicit designer fonts of UserControls hosted the way the program adds them (ControlAccount through LayoutManager.Add, a
// preference page through AddUnscaled). No database, no window shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	[TestClass]
	public class FontSwapTests {
		[TestMethod]
		public void MapFont_FollowsTheRuleOfSection34() {
			using(Font font825=new Font("Microsoft Sans Serif",8.25f,FontStyle.Bold|FontStyle.Underline))
			using(Font font8=new Font("Microsoft Sans Serif",8f))
			using(Font font85=new Font("Microsoft Sans Serif",8.5f,FontStyle.Italic))
			using(Font font10=new Font("Microsoft Sans Serif",10f,FontStyle.Bold))
			using(Font fontArial=new Font("Arial",8.25f,FontStyle.Bold|FontStyle.Italic))
			using(Font fontCourier=new Font("Courier New",8.25f)) {
				AssertFont(TextFitFontSwap.MapFont(font825,"Segoe UI",9f),"Segoe UI",9f,FontStyle.Bold|FontStyle.Underline);
				AssertFont(TextFitFontSwap.MapFont(font8,"Segoe UI",9f),"Segoe UI",9f,FontStyle.Regular);
				AssertFont(TextFitFontSwap.MapFont(font85,"Segoe UI",9f),"Segoe UI",9f,FontStyle.Italic);
				AssertFont(TextFitFontSwap.MapFont(font10,"Segoe UI",9f),"Segoe UI",10f,FontStyle.Bold);
				AssertFont(TextFitFontSwap.MapFont(font825,"Segoe UI",8.25f),"Segoe UI",8.25f,FontStyle.Bold|FontStyle.Underline);
				Assert.AreSame(fontArial,TextFitFontSwap.MapFont(fontArial,"Segoe UI",9f),"Other families stay.");
				Assert.AreSame(fontCourier,TextFitFontSwap.MapFont(fontCourier,"Segoe UI",9f),"Other families stay.");
				Assert.AreSame(font825,TextFitFontSwap.MapFont(font825,"Microsoft Sans Serif",8.25f),"Today's option changes nothing.");
				Assert.AreSame(font8,TextFitFontSwap.MapFont(font8,"Microsoft Sans Serif",8.25f),
					"While the family is Microsoft Sans Serif the mapping is the identity: the 8 pt and 8.5 pt designer fonts keep their size (3.4).");
			}
		}

		[TestMethod]
		public void Swap_OnATestWindow_GivesTheFontsOfTheRuleWithTheLabelFactor() {
			StaRunner.Run(() => {
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				using(TestWindowFonts window=new TestWindowFonts()) {
					using(BuiltWindow builtWindow=WindowBuilder.LoadWithoutShow(window,window.GetLayoutManager())) {
						//Today: the rebuild already gave labels and radio buttons 0.96 and kept only Bold.
						AssertFont(window.labelUnderline.Font,"Microsoft Sans Serif",8.25f*0.96f,FontStyle.Regular);
						int countChanged=TextFitFontSwap.Swap(window,TextFitFontOption.Segoe9);
						Assert.IsTrue(countChanged>=9,"Fonts changed: "+countChanged);
						AssertFont(window.Font,"Segoe UI",9f,FontStyle.Regular);
						AssertFont(window.labelUnderline.Font,"Segoe UI",9f*0.96f,FontStyle.Regular);
						AssertFont(window.labelBold10.Font,"Segoe UI",10f*0.96f,FontStyle.Bold);
						AssertFont(window.labelArial.Font,"Arial",8.25f*0.96f,FontStyle.Bold);
						AssertFont(window.labelItalic85.Font,"Segoe UI",9f*0.96f,FontStyle.Regular);
						AssertFont(window.radioButton.Font,"Segoe UI",9f*0.96f,FontStyle.Regular);
						AssertFont(window.checkBox8.Font,"Segoe UI",9f,FontStyle.Regular);
						AssertFont(window.textBox.Font,"Segoe UI",9f,FontStyle.Regular);
						AssertFont(window.buttonBold.Font,"Segoe UI",9f,FontStyle.Bold);
						AssertFont(window.panel.Font,"Segoe UI",9f,FontStyle.Regular);
						AssertFont(window.labelInPanel.Font,"Segoe UI",9f*0.96f,FontStyle.Regular);
						//The records LayoutManager keeps now hold the mapped size, so a zoom or DPI change keeps the new font.
						Dictionary<Control,object> dictInfos=TextFitFontSwap.GetControl96Infos(window.GetLayoutManager());
						Assert.AreEqual(9f,(float)dictInfos[window.labelUnderline].GetType().GetField("FontSize96").GetValue(dictInfos[window.labelUnderline]));
						builtWindow.LayOutAt(1.5f);
						AssertFont(window.labelUnderline.Font,"Segoe UI",9f*0.96f,FontStyle.Regular);
						AssertFont(window.textBox.Font,"Segoe UI",9f*1.5f,FontStyle.Regular);
						//Swapping again changes nothing.
						builtWindow.LayOutAt(1f);
						Assert.AreEqual(0,TextFitFontSwap.Swap(window,TextFitFontOption.Segoe9));
					}
				}
				using(TestWindowFonts window=new TestWindowFonts()) {
					using(BuiltWindow builtWindow=WindowBuilder.LoadWithoutShow(window,window.GetLayoutManager())) {
						TextFitFontSwap.Swap(window,TextFitFontOption.Segoe825);
						AssertFont(window.labelUnderline.Font,"Segoe UI",8.25f*0.96f,FontStyle.Regular);
						AssertFont(window.checkBox8.Font,"Segoe UI",8.25f,FontStyle.Regular);
						AssertFont(window.labelBold10.Font,"Segoe UI",10f*0.96f,FontStyle.Bold);
					}
				}
			});
		}

		[TestMethod]
		public void Swap_OnAPlainWindow_KeepsTheStyleAndSetsTheWindowsFont() {
			StaRunner.Run(() => {
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				using(TestPlainWindowFonts window=new TestPlainWindowFonts()) {
					TextFitFontSwap.Swap(window,TextFitFontOption.Segoe9);
					AssertFont(window.Font,"Segoe UI",9f,FontStyle.Regular);
					//No LayoutManager: no 0.96 factor, no rebuild, the style stays in full.
					AssertFont(window.labelUnderline.Font,"Segoe UI",9f,FontStyle.Underline);
					AssertFont(window.labelInherits.Font,"Segoe UI",9f,FontStyle.Regular);
				}
			});
		}

		[TestMethod]
		public void Swap_HostedControlAccount_ExplicitDesignerFontsFollow() {
			AssertHostedExplicitFontsFollow("OpenPmsDental.ControlAccount",@"OpenPmsDental\Main Modules\ControlAccount.Designer.cs",BuildKind.UserControlAdd);
		}

		[TestMethod]
		public void Swap_HostedPreferencePage_ExplicitDesignerFontsFollow() {
			AssertHostedExplicitFontsFollow("OpenPmsDental.UserControlAccountRecAndRepCharges",
				@"OpenPmsDental\User Controls\Preferences\UserControlAccountRecAndRepCharges.Designer.cs",BuildKind.UserControlAddUnscaled);
		}

		///<summary>Hosts the UserControl as the scan does, swaps to Segoe UI 9, and checks every control with an explicit Microsoft Sans Serif
		///designer font: Segoe UI at the rule's size (0.96 for labels and radio buttons).</summary>
		private static void AssertHostedExplicitFontsFollow(string typeName,string designerFile,BuildKind kindExpected) {
			Dictionary<string,float> dictDesignerFonts=ReadDesignerFonts(Path.Combine(RepoPaths.Root(),designerFile));
			Assert.IsTrue(dictDesignerFonts.Count>=5,"Explicit Microsoft Sans Serif fonts in "+designerFile+": "+dictDesignerFonts.Count);
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				ScanType scanType=WindowBuilder.GetScanTypes().Single(x => x.Type.FullName==typeName);
				Assert.AreEqual(kindExpected,scanType.Kind);
				using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
					Assert.IsNotNull(builtWindow.LayoutManager,"Hosted in a test FormODBase.");
					Assert.IsInstanceOfType(builtWindow.Form,typeof(HostFormOD));
					TextFitFontSwap.Swap(builtWindow.Form,TextFitFontOption.Segoe9);
					List<string> listProblems=new List<string>();
					foreach(KeyValuePair<string,float> pair in dictDesignerFonts) {
						Control control=builtWindow.Root.Controls.Find(pair.Key,true).FirstOrDefault();
						if(control==null) {
							listProblems.Add(pair.Key+": not found");
							continue;
						}
						float sizeExpected=TextFitFontSwap.IsBodySize(pair.Value) ? 9f : pair.Value;
						sizeExpected=TextFitMeasurer.FontSizeUnderLayoutManager(control,sizeExpected);
						if(control.Font.Name!="Segoe UI" || Math.Abs(control.Font.SizeInPoints-sizeExpected)>0.01f) {
							listProblems.Add(pair.Key+": "+control.Font.Name+" "+control.Font.SizeInPoints+", expected Segoe UI "+sizeExpected);
						}
					}
					Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems));
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

		private static void AssertFont(Font font,string family,float size,FontStyle fontStyle) {
			Assert.AreEqual(family,font.Name,"Family of "+font);
			Assert.AreEqual(size,font.SizeInPoints,0.01f,"Size of "+font);
			Assert.AreEqual(fontStyle,font.Style,"Style of "+font);
		}
	}
}
