// The tier 1 builder and scan (package UI-0b, docs/specs/ui-refresh-plan.md section 5.4, docs/port-notes/UI-0b.md): a FormODBase window gets
// InitializeLayoutManager only when its constructor calls it; the no-database guard refuses before the network; an AutoSize label that grows
// into its neighbour is reported and one that grows into free space is not; and the scan on today's font reports nothing new.
// The full scan with the font options (Segoe UI 9 and 8.25), every scale and the text-scale run is opt-in (OPMS_UI_SCAN=1, files in
// OPMS_UI_SCAN_OUT): it writes the counts of docs/port-notes/UI-0b.md. From UI-2a-1 on it puts the options through the token override
// UiTheme.SetFontForTest (the product's own font path, section 5.4; docs/port-notes/UI-2a.md has its counts); OPMS_UI_SCAN_SWAP=1 runs UI-0b's
// test-side font swap instead, for comparing. No database, no window shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using ProjectFileTests;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	[TestClass]
	public class BuilderAndScanTests {
		public const string VARIABLE_SCAN="OPMS_UI_SCAN";
		public const string VARIABLE_SCAN_OUT="OPMS_UI_SCAN_OUT";
		public const string VARIABLE_SCAN_SWAP="OPMS_UI_SCAN_SWAP";
		///<summary>The five FormODBase windows whose constructor never calls InitializeLayoutManager on main (section 5.1).</summary>
		private static readonly string[] _arrayWindowsWithoutLayoutManager=new string[] { "FormProgressViewer","FormInstallAllocator_Provider",
			"FormReport_GuarantorAllocationCheck","FormReport_ProviderIncomeReport","FormWarnToCloseComputers" };

		[TestMethod]
		public void NoDatabaseGuard_RefusesEveryConnectionBeforeTheNetwork() {
			Assert.IsTrue(DbConnectionSecurity.GetMissing(NoDatabaseGuard.CONNECTION_STRING).Count>0,"The connection rule refuses the guard's string.");
			Assert.IsFalse(DbConnectionSecurity.IsLoopbackConnectorServer("no-database.example.test"));
			NoDatabaseGuard.Install();
			Assert.ThrowsExactly<DbConnectionRefusedException>(() => new DataConnection());
			Assert.IsTrue(NoDatabaseGuard.IsDatabaseException(new TypeInitializationException("X",new DbConnectionRefusedException("s",new List<string> { "a" }))));
		}

		[TestMethod]
		public void FormInstallAllocator_Provider_IsBuiltWithoutLayoutManager() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(string name in _arrayWindowsWithoutLayoutManager) {
					ScanType scanTypeWithout=listScanTypes.Single(x => x.Type.Name==name);
					Assert.AreEqual(BuildKind.WindowWithoutLayoutManager,scanTypeWithout.Kind,name);
				}
				List<string> listOthers=listScanTypes.Where(x => x.Kind==BuildKind.WindowWithoutLayoutManager).Select(x => x.Type.Name)
					.Except(_arrayWindowsWithoutLayoutManager).ToList();
				Assert.AreEqual(0,listOthers.Count,"Other FormODBase windows without InitializeLayoutManager: "+string.Join(", ",listOthers));
				ScanType scanType=listScanTypes.Single(x => x.Type.Name=="FormInstallAllocator_Provider");
				using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
					Assert.IsNull(builtWindow.LayoutManager);
					Assert.IsNull(TextFitFontSwap.GetLayoutManager(builtWindow.Form),"No LayoutManagerForms after the build.");
					Assert.IsTrue(builtWindow.Form.Controls.Count>0,"InitializeComponent ran.");
					Assert.IsFalse(builtWindow.Form.IsHandleCreated);
				}
				//A window whose constructor calls it gets it, and a PanelClient as in OnLoad.
				ScanType scanTypePreferences=listScanTypes.Single(x => x.Type==typeof(FormPreferences));
				Assert.AreEqual(BuildKind.LayoutManagerWindow,scanTypePreferences.Kind);
				using(BuiltWindow builtWindow=WindowBuilder.Build(scanTypePreferences)) {
					Assert.IsNotNull(builtWindow.LayoutManager);
					Assert.IsNotNull(((FormODBase)builtWindow.Form).PanelClient);
					Assert.IsFalse(builtWindow.Form.IsHandleCreated);
				}
			});
		}

		[TestMethod]
		public void GetScanTypes_HostsUserControlsTheWayTheProgramAddsThem() {
			StaRunner.Run(() => {
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				Assert.AreEqual(BuildKind.UserControlAddUnscaled,listScanTypes.Single(x => x.Type==typeof(ControlImages)).Kind);
				Assert.AreEqual(BuildKind.UserControlAddUnscaled,listScanTypes.Single(x => x.Type.Name=="UserControlAccountRecAndRepCharges").Kind);
				Assert.AreEqual(BuildKind.UserControlAdd,listScanTypes.Single(x => x.Type.Name=="ControlAccount").Kind);
				Assert.AreEqual(BuildKind.UserControlPlain,listScanTypes.Single(x => x.Type.Name=="DashboardTabCtrl").Kind,"The graph's UserControls.");
				Assert.IsTrue(listScanTypes.Any(x => x.Assembly=="CentralManager" && x.Kind==BuildKind.PlainWindow),"The plain exes' forms.");
				Assert.IsTrue(listScanTypes.Count(x => typeof(Form).IsAssignableFrom(x.Type))>800,"About 892 forms: "+listScanTypes.Count(x => typeof(Form).IsAssignableFrom(x.Type)));
				Assert.IsTrue(listScanTypes.Count(x => typeof(UserControl).IsAssignableFrom(x.Type))>100,"About 153 UserControls.");
			});
		}

		[TestMethod]
		public void AutoSizeLabel_GrowingIntoItsNeighbour_IsReported_GrowingIntoFreeSpace_IsNot() {
			StaRunner.Run(() => {
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				using(TestWindowAutoSize window=new TestWindowAutoSize()) {
					using(BuiltWindow builtWindow=WindowBuilder.LoadWithoutShow(window,window.GetLayoutManager())) {
						Assert.IsTrue(window.labelNear.AutoSize,"LayoutManager leaves a label's AutoSize on.");
						List<TextFitBoundsRow> listBefore=TextFitBoundsRecord.Collect("TestWindowAutoSize",window);
						Assert.AreEqual(0,TextFitBoundsRecord.FindOverlaps(listBefore).Count,"No overlap today.");
						//The label next to the text box grows (its text, as a longer font would): it now reaches into the text box.
						window.labelNear.Text="Name of the patient";
						List<TextFitBoundsRow> listAfter=TextFitBoundsRecord.Collect("TestWindowAutoSize",window);
						List<TextFitBoundsDifference> listDifferences=TextFitBoundsRecord.Compare(listBefore,listAfter,new HashSet<string>());
						Assert.AreEqual(1,listDifferences.Count,string.Join("; ",listDifferences));
						Assert.AreEqual("overlap",listDifferences[0].Kind);
						Assert.AreEqual("PanelClient/labelNear",listDifferences[0].Row.Path);
						Assert.AreEqual("PanelClient/textNear",listDifferences[0].Other);
						//Listed in fit-fixes.tsv: allowed.
						Assert.AreEqual(0,TextFitBoundsRecord.Compare(listBefore,listAfter,
							new HashSet<string> { TextFitBoundsRecord.GetFitFixKey("TestWindowAutoSize","labelNear") }).Count);
						//The label with free space to its right grows: no difference.
						window.labelNear.Text="Name";
						listBefore=TextFitBoundsRecord.Collect("TestWindowAutoSize",window);
						window.labelFree.Text="Free space to the right of this label";
						listAfter=TextFitBoundsRecord.Collect("TestWindowAutoSize",window);
						Assert.AreNotEqual(listBefore.Single(x => x.Name=="labelFree").Bounds.Width,listAfter.Single(x => x.Name=="labelFree").Bounds.Width);
						Assert.AreEqual(0,TextFitBoundsRecord.Compare(listBefore,listAfter,new HashSet<string>()).Count);
					}
				}
			});
		}

		[TestMethod]
		public void Scan_TodaysFont_ReportsNothingNew() {
			ScanOptions scanOptions=new ScanOptions();
			scanOptions.ListFontOptions.Add(TextFitFontOption.Today);
			scanOptions.ArrayScales=new float[] { 1f,1.5f };
			scanOptions.FolderOut=GetFolderOut("today");
			ScanReport scanReport=TierOneScan.Run(scanOptions);
			List<ScanTypeResult> listBuilt=scanReport.ListTypeResults.Where(x => x.Status=="built").ToList();
			Assert.IsTrue(listBuilt.Count>500,"Built: "+listBuilt.Count+" of "+scanReport.ListTypeResults.Count);
			Assert.IsTrue(listBuilt.Sum(x => x.CountMeasured)>5000,"Controls with measured text: "+listBuilt.Sum(x => x.CountMeasured));
			Assert.AreEqual(0,scanReport.ListOverflows.Count,"New overflows: "+string.Join("; ",scanReport.ListOverflows.Take(10).Select(x => x.Window+" "+x.Path)));
			Assert.AreEqual(0,scanReport.ListSelfSizing.Count,"New self-sizing overlaps: "+string.Join("; ",scanReport.ListSelfSizing.Take(10).Select(x => x.Window+" "+x.Path)));
			Assert.AreEqual(0,scanReport.ListBoundsDifferences.Count,"Bounds differences: "+string.Join("; ",scanReport.ListBoundsDifferences.Take(10).Select(x => x.Item2)));
			Assert.AreEqual(0,scanReport.ListTypeResults.Count(x => x.Reason.Contains(" throws: ")),"A type that built once must build again.");
		}

		[TestMethod]
		public void Scan_FontOptions_WritesTheCounts() {
			if(!BuildOutput.IsRequiredValue(Environment.GetEnvironmentVariable(VARIABLE_SCAN))) {
				Assert.Inconclusive(VARIABLE_SCAN+" is not 1: the full scan (Segoe UI 9 and 8.25, every scale, the text-scale run) is opt-in; it writes its files to "
					+VARIABLE_SCAN_OUT+".");
			}
			ScanOptions scanOptions=new ScanOptions();
			scanOptions.ListFontOptions.Add(TextFitFontOption.Segoe9);
			scanOptions.ListFontOptions.Add(TextFitFontOption.Segoe825);
			scanOptions.ArrayScales=new float[] { 1f,1.2f,1.25f,1.5f,1.75f,2f };
			scanOptions.IsTextScaleRun=true;
			scanOptions.IsFontSwap=BuildOutput.IsRequiredValue(Environment.GetEnvironmentVariable(VARIABLE_SCAN_SWAP));
			scanOptions.FolderOut=GetFolderOut(scanOptions.IsFontSwap ? "options-swap" : "options");
			ScanReport scanReport=TierOneScan.Run(scanOptions);
			Assert.IsTrue(File.Exists(Path.Combine(scanOptions.FolderOut,"summary.txt")));
			Assert.IsTrue(scanReport.ListOverflows.Count>0,"Segoe UI 9 must overflow somewhere: the scan measured nothing?");
		}

		///<summary>OPMS_UI_SCAN_OUT\name, else the test's output folder's ui0b-scan\name.</summary>
		private static string GetFolderOut(string name) {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_SCAN_OUT);
			if(string.IsNullOrWhiteSpace(folder)) {
				folder=Path.Combine(AppContext.BaseDirectory,"ui0b-scan");
			}
			return Path.Combine(folder.Trim(),name);
		}
	}
}
