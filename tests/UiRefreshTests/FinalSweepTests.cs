// UI-11-final (docs/port-notes/UI-11-final.md, section 1): the final sweep of the UI refresh over every window the harness builds
// (WindowBuilder.GetScanTypes: the program's windows and user controls, the plain exes' windows, the graph's, the report designer's and ODR's).
// Each is built off screen as the gallery builds it and its controls are walked for what is left of the classic look: the classic grey or the
// old window background on a control that shows the window's background, a stock button, check box, radio button, group box, tab control, grey
// block, caption, separator or 3-D border without its look, an input without the input frame, scroll bars in Windows' look, a plain window
// without the program's frame and background, a calendar in Windows' look (UI-CAL), and (UI-LOAD) a picture box showing an animated picture, a busy GIF where the ring belongs.
// Every scanned window is also on the layout grid's list (grid-windows, GridRulesTests). A finding
// that stays is listed with its reason in Data\sweep-known.tsv (window, rule, key, why); a new one fails, and so does a row the sweep no longer
// finds. OPMS_UI_SWEEP_OUT names a file for every finding of the run (the sweep's record). Nothing is shown; no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	///<summary>The sweep's rules and lists, shared with the gallery (FitGallery's "@all" and "@densest").</summary>
	public static class FinalSweep {
		public const string RULE_CLASSIC_GREY="classic-grey";
		public const string RULE_INPUT_FRAME="input-frame";
		public const string RULE_SCROLL="scroll";
		public const string RULE_PLAIN_WINDOW="plain-window";
		public const string RULE_TITLE_BAR="title-bar";
		public const string RULE_GRID="grid";
		///<summary>UI-CAL: a calendar in Windows' look (a stock MonthCalendar, or a stock date picker's drop-down).</summary>
		public const string RULE_STOCK_CALENDAR="stock-calendar";
		///<summary>UI-LOAD: a picture box showing an animated picture (an animated GIF as a busy indicator; the ring, CodeBase.UiProgressRing, is the
		///busy indicator now).</summary>
		public const string RULE_BUSY_GIF="busy-gif";
		public const string HEADER_KNOWN="window\trule\tkey\twhy";
		///<summary>The shared looks that draw a stock type (CodeBase's class with IsDrawn and IsAttached) and the rule each missing one is.</summary>
		public static readonly string[][] ArrayLooks=new string[][] {
			new[] { "UiButtonLook","stock-button" },new[] { "UiCheckLook","stock-check-box" },new[] { "UiRadioLook","stock-radio-button" },
			new[] { "UiGroupBoxLook","stock-group-box" },new[] { "UiTabLook","stock-tab-control" },new[] { "UiBlockLook","stock-grey-block" },
			new[] { "UiCaptionLook","stock-caption" },new[] { "UiSeparatorLook","stock-separator" },new[] { "UiBorderLook","stock-3d-border" },
		};

		///<summary>The types the sweep builds: every scanned type but the plain forms that are no window of their own (the progress windows' base)
		///and the report designer's MDI child (only ever inside Windows' MDI frame).</summary>
		public static List<ScanType> GetSweepTypes(List<ScanType> listScanTypes) {
			return listScanTypes.Where(x => !PlainLookTests.ArrayNotWindows.Contains(x.Type.FullName) && x.Type.FullName!="fyiReporting.RdlDesign.MDIChild")
				.OrderBy(x => x.WindowKey,StringComparer.Ordinal).ToList();
		}

		///<summary>The window keys of the share (0.1: the tenth, rounded up) of the sweep's types with the most controls, as built; ties by key.</summary>
		public static List<string> GetDensest(List<ScanType> listScanTypes,double share) {
			List<KeyValuePair<string,int>> listCounts=new List<KeyValuePair<string,int>>();
			List<ScanType> listSweepTypes=GetSweepTypes(listScanTypes);
			foreach(ScanType scanType in listSweepTypes) {
				try {
					using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
						listCounts.Add(new KeyValuePair<string,int>(scanType.WindowKey,Walk(builtWindow.Root).Count()));
					}
				}
				catch(Exception) {
					//A type the builder cannot build is in the sweep's failures, not in the densest list.
				}
			}
			int count=(int)Math.Ceiling(listSweepTypes.Count*share);
			return listCounts.OrderByDescending(x => x.Value).ThenBy(x => x.Key,StringComparer.Ordinal).Take(count).Select(x => x.Key).ToList();
		}

		public static IEnumerable<Control> Walk(Control root) {
			yield return root;
			foreach(Control control in root.Controls) {
				foreach(Control descendant in Walk(control)) {
					yield return descendant;
				}
			}
		}

		///<summary>The control's key in the findings: its path of names from the root (a control without a name by its type and index).</summary>
		public static string GetKey(Control control,Control root) {
			List<string> listNames=new List<string>();
			while(control!=null && control!=root) {
				string name=control.Name;
				if(string.IsNullOrEmpty(name)) {
					name=control.GetType().Name+"["+(control.Parent==null ? 0 : control.Parent.Controls.IndexOf(control))+"]";
				}
				listNames.Insert(0,name);
				control=control.Parent;
			}
			return listNames.Count==0 ? "(window)" : string.Join("/",listNames);
		}

		///<summary>The classic look left in the built window: "rule\tkey" per finding. The looks are those of the window's assembly's own copy of
		///CodeBase's UI files when it has one (the report designer), else CodeBase's.</summary>
		public static List<string> Check(BuiltWindow builtWindow) {
			List<string> listFindings=new List<string>();
			System.Reflection.Assembly assemblyLooks=builtWindow.ScanType.Type.Assembly;
			if(assemblyLooks.GetType("CodeBase.UiLooks")==null) {
				assemblyLooks=null;
			}
			Control root=builtWindow.Root;
			bool isPlain=builtWindow.ScanType.Kind==BuildKind.PlainWindow;
			if(isPlain) {
				if(!PlainLookCopy.IsTrue(assemblyLooks,"UiPlainWindow","IsAttached",builtWindow.Form)) {
					listFindings.Add(RULE_PLAIN_WINDOW+"\t(window)");
				}
				else if(!builtWindow.Form.IsMdiChild && !PlainLookCopy.IsTrue(assemblyLooks,"UiPlainFrame","IsAttached",builtWindow.Form)) {
					listFindings.Add(RULE_TITLE_BAR+"\t(window)");
				}
			}
			foreach(Control control in Walk(root)) {
				string key=GetKey(control,root);
				string nameIsOld=control is Form ? "IsOldWindowBackground" : "IsOldBackground";
				if(PlainLookCopy.IsTrue(assemblyLooks,"UiPlainWindow","IsBackground",control)
					&& PlainLookCopy.IsTrue(assemblyLooks,"UiPlainWindow",nameIsOld,control.BackColor))
				{
					listFindings.Add(RULE_CLASSIC_GREY+"\t"+key);
				}
				if(control!=root && PlainLookCopy.IsTrue(assemblyLooks,"UiInputFrame","IsFramed",control)
					&& !PlainLookCopy.IsTrue(assemblyLooks,"UiInputFrame","IsAttached",control))
				{
					listFindings.Add(RULE_INPUT_FRAME+"\t"+key);
				}
				foreach(string[] arrayLook in ArrayLooks) {
					if(PlainLookCopy.IsTrue(assemblyLooks,arrayLook[0],"IsDrawn",control) && !PlainLookCopy.IsTrue(assemblyLooks,arrayLook[0],"IsAttached",control)) {
						listFindings.Add(arrayLook[1]+"\t"+key);
					}
				}
				//UI-CAL: no calendar in Windows' look: a stock MonthCalendar (MonthCalendarOD replaced them), or a stock date picker whose drop-down
				//is Windows' calendar (UiDateDropLook gives it the program's; the up-down style has none).
				if(control is MonthCalendar || (control is DateTimePicker dateTimePicker && !dateTimePicker.ShowUpDown
					&& !PlainLookCopy.IsTrue(assemblyLooks,"UiDateDropLook","IsProgramCalendar",control)))
				{
					listFindings.Add(RULE_STOCK_CALENDAR+"\t"+key);
				}
				if(control is PictureBox pictureBox && pictureBox.Image!=null && System.Drawing.ImageAnimator.CanAnimate(pictureBox.Image)) {
					listFindings.Add(RULE_BUSY_GIF+"\t"+key);
				}
				if(control!=root && PlainLookCopy.IsTrue(assemblyLooks,"UiScrollLook","IsScrolled",control)
					&& !PlainLookCopy.IsTrue(assemblyLooks,"UiScrollLook","IsAttached",control))
				{
					listFindings.Add(RULE_SCROLL+"\t"+key);
				}
			}
			return listFindings.Distinct().ToList();
		}

		///<summary>Data\sweep-known.tsv: "window\trule\tkey" of each finding that stays, with its reason.</summary>
		public static HashSet<string> ReadKnown() {
			HashSet<string> hashSetKnown=new HashSet<string>(StringComparer.Ordinal);
			string path=RepoPaths.DataFile("sweep-known.tsv");
			string[] arrayLines=File.ReadAllLines(path);
			Assert.AreEqual(HEADER_KNOWN,arrayLines[0],"sweep-known.tsv's header");
			foreach(string line in arrayLines.Skip(1)) {
				if(line=="" || line.StartsWith("#")) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				Assert.AreEqual(4,arrayFields.Length,"sweep-known.tsv: window, rule, key, why: "+line);
				Assert.IsFalse(string.IsNullOrWhiteSpace(arrayFields[3]),"sweep-known.tsv: every row has its reason: "+line);
				Assert.IsTrue(hashSetKnown.Add(arrayFields[0]+"\t"+arrayFields[1]+"\t"+arrayFields[2]),"sweep-known.tsv lists a row twice: "+line);
			}
			return hashSetKnown;
		}
	}

	[TestClass]
	public class FinalSweepTests {
		///<summary>Every window the harness builds is checked by the layout grid's rules (grid-windows), or listed with its reason.</summary>
		[TestMethod]
		public void Sweep_EveryWindow_IsOnTheGridList() {
			HashSet<string> hashSetKnown=FinalSweep.ReadKnown();
			HashSet<string> hashSetGrid=new HashSet<string>(GridRulesTests.ReadGridWindows().Select(x => x.Window));
			List<string> listMissing=new List<string>();
			List<string> listKeys=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				listKeys=FinalSweep.GetSweepTypes(WindowBuilder.GetScanTypes()).Select(x => x.WindowKey).ToList();
			});
			Assert.IsTrue(listKeys.Count>=1000,"The sweep's windows: "+listKeys.Count);
			foreach(string key in listKeys) {
				if(!hashSetGrid.Contains(key) && !hashSetKnown.Contains(key+"\t"+FinalSweep.RULE_GRID+"\t(window)")) {
					listMissing.Add(key);
				}
			}
			Assert.AreEqual(0,listMissing.Count,"Not on the grid's list (grid-windows) and not in sweep-known.tsv:"+Environment.NewLine+string.Join(Environment.NewLine,listMissing));
		}

		///<summary>The report viewer library's two dialogs (its messages, the data source pass phrase) take the look from their host: the program
		///sets RdlViewer.ActionAttachLook with the looks' scale (LayoutManagerForms.SetLooksScale), and each dialog's constructor calls it. The
		///sweep and the gallery build them by their constructor (WindowBuilder's list), so the sweep checks them too.</summary>
		[TestMethod]
		public void ReportViewerDialogs_TakeTheLookFromTheHost() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				LayoutManagerForms.SetLooksScale();
				//The report viewer library by reflection: this project references the program, not the library itself.
				Type typeViewer=Type.GetType("fyiReporting.RdlViewer.RdlViewer, RdlViewer",throwOnError:true);
				Assert.IsNotNull(typeViewer.GetField("ActionAttachLook").GetValue(null),"the program sets the hook");
				using(Form formMessages=(Form)Activator.CreateInstance(typeViewer.Assembly.GetType("fyiReporting.RdlViewer.DialogMessages",true),
					new System.Collections.ArrayList { "A message" }))
				{
					Assert.IsTrue(UiPlainWindow.IsAttached(formMessages),"DialogMessages");
					Assert.AreEqual(UiTheme.Surface.ToArgb(),formMessages.BackColor.ToArgb());
				}
				using(Form formPassword=(Form)Activator.CreateInstance(typeViewer.Assembly.GetType("fyiReporting.RdlViewer.DataSourcePassword",true))) {
					Assert.IsTrue(UiPlainWindow.IsAttached(formPassword),"DataSourcePassword");
				}
			});
		}

		///<summary>No window the harness builds has the classic look left, but the findings sweep-known.tsv lists with their reasons; every row of
		///it is still found.</summary>
		[TestMethod]
		public void Sweep_EveryWindow_NoClassicLookLeft() {
			HashSet<string> hashSetKnown=FinalSweep.ReadKnown();
			List<string> listFindings=new List<string>();
			List<string> listFailed=new List<string>();
			int countWindows=0;
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				bool isBuildingBefore=WindowBuilder.IsBuildingByConstructor;
				WindowBuilder.IsBuildingByConstructor=true;//as the gallery builds them: the windows made in their constructor with their controls
				try {
					foreach(ScanType scanType in FinalSweep.GetSweepTypes(WindowBuilder.GetScanTypes())) {
						FitGallery.SetGalleryFont("program");
						try {
							using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
								listFindings.AddRange(FinalSweep.Check(builtWindow).Select(x => scanType.WindowKey+"\t"+x));
								countWindows++;//only once its check ran: a window whose check throws is a failure, not a swept window
							}
						}
						catch(Exception exception) {
							listFailed.Add(scanType.WindowKey+": "+TierOneScan.Describe(exception));
						}
						finally {
							UiTheme.SetFontForTest(null);
							DefaultFontOverride.Set(TextFitFontOption.Today,1f);
						}
					}
				}
				finally {
					WindowBuilder.IsBuildingByConstructor=isBuildingBefore;
				}
			});
			string pathOut=Environment.GetEnvironmentVariable("OPMS_UI_SWEEP_OUT");
			if(!string.IsNullOrEmpty(pathOut)) {
				File.WriteAllLines(pathOut,new[] { "window\trule\tkey" }.Concat(listFindings).Concat(listFailed.Select(x => "# failed: "+x)));
			}
			Assert.IsTrue(countWindows>=1000,"The sweep built "+countWindows+" windows. Failed: "+string.Join("; ",listFailed.Take(20)));
			//Only the types the harness cannot build may fail (UI-11-final.md; the lead after the review): any other failure is a lapse in coverage.
			string[] arrayUnbuildable=new[] { "ODDatePicker","ToothChartWrapper","UserControlSecurityUserGroup","DataSetRowsCtl" };
			List<string> listFailedOther=listFailed.Where(x => !arrayUnbuildable.Contains(x.Split(':')[0].Split('.').Last().Split('@')[0])).ToList();
			Assert.AreEqual(0,listFailedOther.Count,"Windows the sweep could not check: "+string.Join("; ",listFailedOther.Take(20)));
			List<string> listNew=listFindings.Where(x => !hashSetKnown.Contains(x)).ToList();
			HashSet<string> hashSetFound=new HashSet<string>(listFindings,StringComparer.Ordinal);
			List<string> listGone=hashSetKnown.Where(x => x.Split('\t')[1]!=FinalSweep.RULE_GRID && !hashSetFound.Contains(x)).ToList();
			Assert.AreEqual(0,listNew.Count+listGone.Count,"Classic look left (not in sweep-known.tsv):"+Environment.NewLine+string.Join(Environment.NewLine,listNew)
				+Environment.NewLine+"Rows of sweep-known.tsv no longer found:"+Environment.NewLine+string.Join(Environment.NewLine,listGone));
		}
	}
}
