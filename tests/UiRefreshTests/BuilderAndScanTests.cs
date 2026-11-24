// The tier 1 builder and scan (package UI-0b, docs/specs/ui-refresh-plan.md section 5.4, docs/port-notes/UI-0b.md): a FormODBase window gets
// InitializeLayoutManager only when its constructor calls it; the no-database guard refuses before the network; an AutoSize label that grows
// into its neighbour is reported and one that grows into free space is not; and the scan on today's font reports nothing new.
// The full scan with the font options (Segoe UI 9 and 8.25), every scale and the text-scale run is opt-in (OPMS_UI_SCAN=1, files in
// OPMS_UI_SCAN_OUT): it writes the counts of docs/port-notes/UI-0b.md. From UI-2a-1 on it puts the options through the token override
// UiTheme.SetFontForTest (the product's own font path, section 5.4; docs/port-notes/UI-2a.md has its counts); OPMS_UI_SCAN_SWAP=1 runs UI-0b's
// test-side font swap instead, for comparing. From UI-2b on Segoe UI 9 is the program's own font, so the scan runs it
// with no override at all (ScanOptions.IsProgramFont); OPMS_UI_SCAN_OVERRIDE=1 puts it through the override as before. No database, no window shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
		public const string VARIABLE_SCAN_OVERRIDE="OPMS_UI_SCAN_OVERRIDE";
		///<summary>The five FormODBase windows whose constructor never called InitializeLayoutManager (section 5.1); since UI-12F (the grid's final
		///consistency pass) they call it, so they scale at 150% like every FormODBase window.</summary>
		private static readonly string[] _arrayAllocatorWindows=new string[] { "FormProgressViewer","FormInstallAllocator_Provider",
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
				//UI-12F: the five allocator windows call InitializeLayoutManager now, and no FormODBase window is without it.
				foreach(string name in _arrayAllocatorWindows) {
					ScanType scanTypeAllocator=listScanTypes.Single(x => x.Type.Name==name);
					Assert.AreEqual(BuildKind.LayoutManagerWindow,scanTypeAllocator.Kind,name);
				}
				List<string> listWithout=listScanTypes.Where(x => x.Kind==BuildKind.WindowWithoutLayoutManager).Select(x => x.Type.Name).ToList();
				Assert.AreEqual(0,listWithout.Count,"FormODBase windows without InitializeLayoutManager: "+string.Join(", ",listWithout));
				ScanType scanType=listScanTypes.Single(x => x.Type.Name=="FormInstallAllocator_Provider");
				using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
					Assert.IsNotNull(builtWindow.LayoutManager,"An allocator window is built with its LayoutManager.");
					Assert.IsNotNull(((FormODBase)builtWindow.Form).PanelClient);
					Assert.IsFalse(builtWindow.Form.IsHandleCreated);
				}
				//The builder's path for a FormODBase window whose constructor does not call it stays: built like a plain form, no LayoutManager.
				ScanType scanTypeWithout=new ScanType { Type=scanType.Type,Assembly=scanType.Assembly,Kind=BuildKind.WindowWithoutLayoutManager };
				using(BuiltWindow builtWindow=WindowBuilder.Build(scanTypeWithout)) {
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

		///<summary>UI-12F fix round: a LayoutManager window's designer fonts are drawn as written (the font scan exempts LayoutManager windows), so
		///the allocator windows, which call InitializeLayoutManager since UI-12F, name no family but Segoe UI (FormWarnToCloseComputers drew its
		///Arial Black and Arial once it had its LayoutManager).</summary>
		[TestMethod]
		public void AllocatorWindows_DesignerFontsAreSegoeUi() {
			string folder=Path.Combine(RepoPaths.Root(),"OpenPmsDental","ReportingForRdl","Allocators");
			List<string> listFiles=Directory.GetFiles(folder,"*.Designer.cs",SearchOption.AllDirectories).ToList();
			Assert.IsTrue(listFiles.Count>=5,"The allocator windows' designer files: "+listFiles.Count);
			List<string> listWrong=new List<string>();
			int countFonts=0;
			foreach(string file in listFiles) {
				foreach(System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(file),
					"new System\\.Drawing\\.Font\\(\"([^\"]*)\""))
				{
					countFonts++;
					if(match.Groups[1].Value!="Segoe UI") {
						listWrong.Add(Path.GetFileName(file)+": "+match.Groups[1].Value);
					}
				}
			}
			Assert.IsTrue(countFonts>0,"The allocator designers set fonts.");
			Assert.AreEqual(0,listWrong.Count,"Allocator designer fonts that are not Segoe UI:"+Environment.NewLine+string.Join(Environment.NewLine,listWrong));
		}

		///<summary>UI-12F fix round: FormProgressViewer's size limits are set in LayoutManager's scale once FormODBase.OnLoad has applied the
		///screen's dpi (in the constructor the scale is still 96 dpi's), and they always contain the window LayoutManager sized: at 175% the
		///scaled window (596 px client at 96 dpi, 1043 px) is wider than the 1000 px limit at 96 dpi.</summary>
		[TestMethod]
		public void FormProgressViewer_SizeLimitsFollowTheScreensDpi() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				ScanType scanType=WindowBuilder.GetScanTypes().Single(x => x.Type.Name=="FormProgressViewer");
				using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
					OpenPmsDental.Reporting.Allocators.MyAllocator1.FormProgressViewer formProgressViewer=(OpenPmsDental.Reporting.Allocators.MyAllocator1.FormProgressViewer)builtWindow.Form;
					LayoutManagerForms layoutManager=builtWindow.LayoutManager;
					Assert.AreEqual(Size.Empty,formProgressViewer.MinimumSize,"No limits in the constructor (LayoutManager's scale is 96 dpi's there).");
					Assert.AreEqual(Size.Empty,formProgressViewer.MaximumSize);
					foreach(float scale in new float[] { 1f,1.5f,1.75f,2.5f }) {
						layoutManager.SetScaleMS(scale);
						//As FormODBase.OnLoad sizes the window: the scaled client size plus its borders.
						int widthBorder=(int)Math.Round(8+(5*(scale-1)),MidpointRounding.AwayFromZero);
						Size sizeWindow=new Size(layoutManager.Scale(layoutManager.SizeClientOriginal.Width)+widthBorder*2,
							layoutManager.Scale(layoutManager.SizeClientOriginal.Height)+widthBorder+layoutManager.GetHeightTitleBar());
						formProgressViewer.MinimumSize=Size.Empty;
						formProgressViewer.MaximumSize=Size.Empty;
						formProgressViewer.Size=sizeWindow;
						formProgressViewer.SetSizeLimitsScaled();
						Assert.AreEqual(sizeWindow,formProgressViewer.Size,"The limits never resize the window at "+scale);
						Assert.IsTrue(formProgressViewer.MinimumSize.Width<=sizeWindow.Width && formProgressViewer.MinimumSize.Height<=sizeWindow.Height,
							"The minimum contains the window at "+scale);
						Assert.IsTrue(formProgressViewer.MaximumSize.Width>=sizeWindow.Width && formProgressViewer.MaximumSize.Height>=sizeWindow.Height,
							"The maximum contains the window at "+scale);
						Assert.AreEqual(Math.Max(layoutManager.Scale(1000),sizeWindow.Width),formProgressViewer.MaximumSize.Width,"Scaled at "+scale);
						Assert.AreEqual(Math.Max(layoutManager.Scale(800),sizeWindow.Height),formProgressViewer.MaximumSize.Height,"Scaled at "+scale);
						Assert.AreEqual(Math.Min(layoutManager.Scale(525),sizeWindow.Width),formProgressViewer.MinimumSize.Width,"Scaled at "+scale);
						Assert.AreEqual(Math.Min(layoutManager.Scale(166),sizeWindow.Height),formProgressViewer.MinimumSize.Height,"Scaled at "+scale);
						if(scale>=1.75f) {
							Assert.IsTrue(sizeWindow.Width>1000,"The case the limits at 96 dpi broke: the window wider than 1000 at "+scale);
						}
					}
				}
			});
		}

		///<summary>UI-12 slice 2 continuation: a base class's designer runs only when its constructor calls it, as in the program. SetupWizControl
		///has a designer (a title and a description label) but no constructor, so the setup wizard's pages never have those labels; FormODBase
		///windows' own designers still run.</summary>
		[TestMethod]
		public void CreateBare_RunsABaseDesignerOnlyWhenItsConstructorDoes() {
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				DefaultFontOverride.Set(TextFitFontOption.Today,1f);
				Type typeBase=typeof(OpenPmsDental.User_Controls.SetupWizard.SetupWizControl);
				MethodInfo methodInfo=typeBase.GetMethod("InitializeComponent",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.DeclaredOnly);
				Assert.IsNotNull(methodInfo,"SetupWizControl has a designer.");
				Assert.IsFalse(WindowBuilder.ConstructorCalls(typeBase,methodInfo),"SetupWizControl has no constructor that calls it.");
				Type typePage=typeof(OpenPmsDental.User_Controls.SetupWizard.UserControlSetupWizSchedule);
				Assert.IsTrue(WindowBuilder.ConstructorCalls(typePage,typePage.GetMethod("InitializeComponent",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.DeclaredOnly)));
				using(UserControl userControl=(UserControl)WindowBuilder.CreateBare(typePage,typeof(UserControl))) {
					Assert.IsTrue(userControl.Controls.Find("checkMonday",false).Length==1,"The page's own designer ran.");
					Assert.AreEqual(0,userControl.Controls.Find("labelTitle",false).Length,"SetupWizControl's labels are never made.");
					Assert.AreEqual(0,userControl.Controls.Find("labelDesc",false).Length);
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
			//UI-2b: Segoe UI 9 is the program's font, so it runs without the override (the product as it ships), unless asked.
			scanOptions.IsProgramFont=!scanOptions.IsFontSwap && !BuildOutput.IsRequiredValue(Environment.GetEnvironmentVariable(VARIABLE_SCAN_OVERRIDE));
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
