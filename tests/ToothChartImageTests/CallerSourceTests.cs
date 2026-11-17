// Source tests of the tooth chart's callers (package TC-2, docs/specs/wpf-tooth-chart-plan.md, sections 4 and 7): the callers hold no chart
// code of their own any more. Their painting is ToothChartPainter's, in the order PainterCallers (the call-log drivers) follows; their chart
// settings are the relay's helpers; no program file outside the relay chooses an engine by its DrawingMode value or names the old engines'
// libraries and types, except the licence texts for good (FormGraphics too until TC-8 rebuilt it). Since TC-8 nothing selects DirectX or
// OpenGL, Setup > Graphics names no engine and the WPF developer switch of TC-2 is gone. Since TC-9a the old engines are gone: no start-up DirectX
// search, no graphics device exception, and ApplyComputerSettings takes the wrapper only. Since TC-8b File > Graphics has Open Dental's layout and
// the perio gate refuses on a computer that draws Simple 2D.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;

namespace ToothChartImageTests {
	[TestClass]
	public class CallerSourceTests {
		private const string CONTROL_CHART=@"OpenPmsDental\Main Modules\ControlChart.cs";
		private const string CONTROL_TREAT=@"OpenPmsDental\Main Modules\ControlTreat.cs";
		private const string SHEET_PRINTING=@"OpenPmsDental\SheetFramework\SheetPrinting.cs";
		private const string FORM_BIG=@"OpenPmsDental\Forms\FormToothChartBig.cs";
		private const string FORM_SHEET_DEF_EDIT=@"OpenPmsDental\Forms\FormSheetDefEdit.cs";
		private const string FORM_PERIO=@"OpenPmsDental\Forms\FormPerio.cs";
		private const string FORM_PERIO_GRAPHICAL=@"OpenPmsDental\Forms\FormPerioGraphical.cs";
		private const string FORM_GRAPHICS=@"OpenPmsDental\Forms\FormGraphics.cs";
		private const string FORM_OPEN_DENTAL=@"OpenPmsDental\Main Modules\FormOpenDental.cs";
		private const string RELAY=@"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs";
		///<summary>Files allowed to name SharpDX or Tao: none since TC-9b, which removed the licence window's SharpDX and Tao entries and resources
		///(FormLicense.cs and Resources.Designer.cs were the two until then).</summary>
		private static readonly string[] _arrayLicenceFiles=new string[0];

		#region The painting code is the painter's
		[TestMethod]
		public void ChartModule_PaintsThroughThePainter_InTheDriversOrder() {
			string code=SourceText.ReadCode(CONTROL_CHART);
			string fill=SourceText.GetMethod(code,"private void FillToothChart(bool retainSelection,DateTime dateLimit) {");
			SourceText.AssertInOrder(fill,"ControlChart.FillToothChart",
				"_toothChartRelay.ResetTeeth();",
				"ToothChartPainter.PaintPrimaryTeeth(_toothChartRelay,Pd.ListToothInitials);",
				"_toothChartRelay.SetSelected(listTeethSelected[i],true);",
				"DataTable table=Pd.TableProgNotes.Copy();",
				"if(checkTreatPlans.Checked) {",
				"listProcNumsTreatPlans=gridTreatPlans.SelectedIndices.SelectMany(x => _listTreatPlans[x].ListProcTPs).Select(x => x.ProcNumOrig).ToList();",
				"_listDataRowsProcsForGraphical=ToothChartPainter.GetChartModuleRows(table,listProcNumsTreatPlans,_listDataRowsProcsOrig,dateLimit,",
				"out _listDataRowsProcsSkipped);",
				"_listToothInitialsCopy=ToothChartPainter.GetChartModuleToothInitials(Pd.ListToothInitials,_listDataRowsProcsSkipped,_listDataRowsProcsForGraphical,",
				"ToothChartPainter.PaintToothInitials(_toothChartRelay,_listToothInitialsCopy);",
				"DrawProcGraphics();",
				"DrawOrthoHardware();",
				"_toothChartRelay.EndUpdate();");
			SourceText.AssertInOrder(SourceText.GetMethod(code,"private void DrawProcGraphics(bool isPrinting=false) {"),"ControlChart.DrawProcGraphics",
				"ToothChartPainter.PaintProcedures(_toothChartRelay,ToothChartPaintProc.FromDataRows(_listDataRowsProcsForGraphical),_listToothInitialsCopy,",
				"ToothChartPaintOptions.ForChartModule(Pd.Patient,Pd.ListAppointments,isPrinting));");
			SourceText.AssertInOrder(SourceText.GetMethod(code,"private void DrawOrthoHardware(){"),"ControlChart.DrawOrthoHardware",
				"if(!checkOrthoGraphics.Checked){",
				"return;",
				"FillOrthoDateIfNeeded();",
				"ToothChartPainter.GetChartModuleOrthoHardwares(Pd.ListOrthoHardwares,comboOrthoDate.GetSelected<DateTime>(),",
				"checkShowHidden.Checked);",
				"ToothChartPainter.PaintOrthoHardware(_toothChartRelay,listOrthoHardwares);");
			//The print recolour of Save to Images: printing colours, the picture, then the screen's colours again.
			SourceText.AssertInOrder(SourceText.GetMethod(code,"private void menuItemChartSave_Click(object sender,EventArgs e) {"),"menuItemChartSave_Click",
				"if(_toothChartRelay.HasToothColorUserPref()) {","DrawProcGraphics(true);","bitmapChart=_toothChartRelay.GetBitmap();","DrawProcGraphics(false);");
			Assert.IsFalse(code.Contains("bool ShouldRowShowGraphical("),"The slider's row rule moved to the painter.");
		}

		[TestMethod]
		public void TreatPlan_PaintsThroughThePainter_InTheDriversOrder() {
			string code=SourceText.ReadCode(CONTROL_TREAT);
			SourceText.AssertInOrder(SourceText.GetMethod(code,"private void PrepImageForPrinting(){"),"ControlTreat.PrepImageForPrinting",
				"ToothChartRelay.ApplyComputerSettings(_toothChartWrapper);",
				"_toothChartRelay.ResetTeeth();",
				"_listToothInitials=ToothInitials.GetPatientData(PatientCur.PatNum);",
				"ToothChartPainter.PaintPrimaryTeeth(_toothChartRelay,_listToothInitials);",
				"ToothChartPainter.PaintToothInitials(_toothChartRelay,_listToothInitials);",
				"ComputeProcListFiltered();",
				"ToothChartPainter.PaintProcedures(_toothChartRelay,ToothChartPaintProc.FromProcedures(_listProceduresFiltered),_listToothInitials,",
				"ToothChartPaintOptions.ForTreatPlanPrintout());",
				"ToothChartPainter.PaintOrthoHardware(_toothChartRelay,OrthoHardwares.GetPatientData(PatientCur.PatNum));",
				"_toothChartRelay.EndUpdate();");
		}

		[TestMethod]
		public void Sheet_PaintsThroughThePainter_InTheDriversOrder() {
			string code=SourceText.ReadCode(SHEET_PRINTING);
			SourceText.AssertInOrder(SourceText.GetMethod(code,"public static Image GetToothChartHelper("),"SheetPrinting.GetToothChartHelper",
				"ToothChartRelay.ApplyComputerSettings(toothChartWrapper);",
				"toothChartRelay.ResetTeeth();",
				"List<ToothInitial> toothInitialList=patNum==0?new List<ToothInitial>():ToothInitials.GetPatientData(patNum);",
				"ToothChartPainter.PaintPrimaryTeeth(toothChartRelay,toothInitialList);",
				"ToothChartPainter.PaintToothInitials(toothChartRelay,toothInitialList);",
				"if(listProceduresFilteredOverride!=null) {",
				"listProceduresFiltered=listProceduresFilteredOverride;",
				"else if(patNum>0) {",
				"List<Procedure> listProceduresAll=Procedures.Refresh(patNum);",
				"listProceduresFiltered=OpenPmsDentalBusiness.SheetPrinting.FilterProceduresForToothChart(listProceduresAll,treatPlan,showCompleted);",
				"listProceduresFiltered.Sort(OpenPmsDentalBusiness.SheetPrinting.CompareProcListFiltered);",
				"ToothChartPainter.PaintProcedures(toothChartRelay,ToothChartPaintProc.FromProcedures(listProceduresFiltered),toothInitialList,",
				"ToothChartPaintOptions.ForSheet(isInPatientDashboard,patCur,listAppts));",
				"List<OrthoHardware> listOrthoHardwares=OrthoHardwares.GetPatientData(patNum);",
				"ToothChartPainter.PaintOrthoHardware(toothChartRelay,listOrthoHardwares);",
				"toothChartRelay.EndUpdate();");
		}

		[TestMethod]
		public void BigChart_PaintsThroughThePainter_InTheDriversOrder() {
			string code=SourceText.ReadCode(FORM_BIG);
			StringAssert.Contains(SourceText.GetMethod(code,"private void FormToothChartingBig_Load(object sender,EventArgs e) {"),
				"ToothChartRelay.ApplyComputerSettings(toothChartWrapper);");
			SourceText.AssertInOrder(SourceText.GetMethod(code,"private void FillToothChart(){"),"FormToothChartingBig.FillToothChart",
				"_toothChartRelay.ResetTeeth();",
				"_toothChartRelay.SetSelected(listSelectedTeeth[i],true);",
				"ToothChartPainter.PaintPrimaryTeeth(_toothChartRelay,ListToothInitials);",
				"ToothChartPainter.PaintToothInitials(_toothChartRelay,ListToothInitials);",
				"ToothChartPainter.PaintProcedures(_toothChartRelay,ToothChartPaintProc.FromDataRows(ListDataRowsProcs),ListToothInitials,",
				"ToothChartPaintOptions.ForBigChart(PatientCur,ListAppointments));",
				"ToothChartPainter.PaintOrthoHardware(_toothChartRelay,ListOrthoHardwares);",
				"_toothChartRelay.EndUpdate();");
		}

		[TestMethod]
		public void ChartModule_MakesTheDashboardImage_OnlyWhenADashboardCanShowIt() {
			string fill=SourceText.GetMethod(SourceText.ReadCode(CONTROL_CHART),"private void FillToothChart(bool retainSelection,DateTime dateLimit) {");
			//TC-3b: the WPF chart's request first, made in the background; GetBitmap only when the chart gives none (DirectX, OpenGL, Simple 2D),
			//all inside the dashboard's guard, so a closed dashboard asks for nothing; then the event, at once.
			SourceText.AssertInOrder(fill,"ControlChart.FillToothChart's dashboard image",
				"PatientDashboardDataEventArgs patientDashboardDataEventArgs=new PatientDashboardDataEventArgs();",
				"if(dateLimit==DateTime.Today && FormOpenDental.IsDashboardVisible) {",
				"patientDashboardDataEventArgs.ImageToothChartRequest=_toothChartRelay.RequestBitmapInBackground();",
				"if(patientDashboardDataEventArgs.ImageToothChartRequest==null) {",
				"patientDashboardDataEventArgs.ImageToothChart=_toothChartRelay.GetBitmap();",
				"ODEvent.Fire(ODEventType.ModuleSelected,patientDashboardDataEventArgs);");
			Assert.AreEqual(1,Regex.Matches(fill,@"GetBitmap\(\)").Count,"The guarded image is the refill's only picture.");
			Assert.AreEqual(1,Regex.Matches(fill,@"RequestBitmapInBackground\(\)").Count,"One request per refill.");
			//The image's only reader is the dashboard's tooth chart widget, which makes its own image when it opens later; the dashboard reads it only
			//to end the wait for a picture still being made (TC-3b).
			CollectionAssert.AreEquivalent(new[] { CONTROL_CHART,@"OpenPmsDental\User Controls\Dashboard\DashToothChart.cs",
				@"OpenPmsDental\User Controls\Dashboard\PatientDashboardDataEventArgs.cs",@"OpenPmsDental\User Controls\Dashboard\UserControlDashboard.cs" },
				GetProgramFilesMatching(@"\.ImageToothChart\b"));
			//The request: made only by the Chart module's refill, read only by the dashboard, which hands it to its wait (ToothChartDashboardWait).
			CollectionAssert.AreEquivalent(new[] { CONTROL_CHART,@"OpenPmsDental\User Controls\Dashboard\UserControlDashboard.cs" },
				GetProgramFilesMatching(@"\.ImageToothChartRequest\b"));
			CollectionAssert.AreEquivalent(new[] { CONTROL_CHART },GetProgramFilesMatching(@"\.RequestBitmapInBackground\("));
			string dash=SourceText.ReadCode(@"OpenPmsDental\User Controls\Dashboard\DashToothChart.cs");
			SourceText.AssertInOrder(SourceText.GetMethod(dash,"public void RefreshData(Patient pat,SheetField sheetField) {"),"DashToothChart.RefreshData",
				"_imgToothChart=GetToothChart(pat,treatPlan,listAppts:listAppts);");
			StringAssert.Contains(SourceText.ReadCode(FORM_OPEN_DENTAL),
				"return (!_formOpenDentalSingleton.splitContainer.Panel2Collapsed && _formOpenDentalSingleton.userControlDashboard.IsInitialized);");
		}

		///<summary>The program's .cs files whose code (comments left out) matches the pattern.</summary>
		private static string[] GetProgramFilesMatching(string pattern) {
			List<string> listFiles=new List<string>();
			foreach(string relative in SourceText.GetCsFiles("OpenPmsDental")) {
				if(Regex.IsMatch(SourceText.ReadCode(relative),pattern)) {
					listFiles.Add(relative);
				}
			}
			return listFiles.ToArray();
		}

		[TestMethod]
		public void Callers_HaveNoPaintSwitchesOrToothInitialLoopsOrHardwareLoops() {
			foreach(string relative in new[] { CONTROL_CHART,CONTROL_TREAT,SHEET_PRINTING,FORM_BIG }) {
				string code=SourceText.ReadCode(relative);
				StringAssert.DoesNotMatch(code,new Regex(@"case\s+ToothPaintingType\."),relative+" paints procedures itself.");
				StringAssert.DoesNotMatch(code,new Regex(@"case\s+ToothInitialType\.(Missing|Rotate|Drawing|Text)\s*:"),relative+" draws tooth initials itself.");
				StringAssert.DoesNotMatch(code,new Regex(@"OrthoHardwares\.Get(Wires|Elastics)\("),relative+" draws ortho hardware itself.");
				StringAssert.DoesNotMatch(code,new Regex(@"\.GetToothColors\("),relative+" picks procedure colours itself.");
			}
		}

		[TestMethod]
		public void Relay_IsThePaintersTarget() {
			Assert.IsTrue(typeof(IToothChartPaintTarget).IsAssignableFrom(typeof(ToothChartRelay)));
		}

		[TestMethod]
		public void PaintOptions_KeepEachCallersDifferences_O25() {
			//Only the Chart module skips Mouth procedures, and it turns the per-provider colours off when it prints.
			ToothChartPaintOptions options=ToothChartPaintOptions.ForChartModule(null,null,false);
			Assert.IsTrue(options.DoSkipMouth && options.DoLookUpProviderColors && options.IsProviderColorAllowed && !options.DoUseTreatPlanColors);
			Assert.IsFalse(ToothChartPaintOptions.ForChartModule(null,null,true).IsProviderColorAllowed);
			//The big chart always has the per-provider colours.
			options=ToothChartPaintOptions.ForBigChart(null,null);
			Assert.IsTrue(!options.DoSkipMouth && options.DoLookUpProviderColors && options.IsProviderColorAllowed && !options.DoUseTreatPlanColors);
			//Sheets have them only in the Patient Dashboard, but look them up either way, as before.
			options=ToothChartPaintOptions.ForSheet(false,null,null);
			Assert.IsTrue(!options.DoSkipMouth && options.DoLookUpProviderColors && !options.IsProviderColorAllowed && !options.DoUseTreatPlanColors);
			Assert.IsTrue(ToothChartPaintOptions.ForSheet(true,null,null).IsProviderColorAllowed);
			//The Treatment Plan printout has its own colours and never had the per-provider ones.
			options=ToothChartPaintOptions.ForTreatPlanPrintout();
			Assert.IsTrue(!options.DoSkipMouth && !options.DoLookUpProviderColors && !options.IsProviderColorAllowed && options.DoUseTreatPlanColors);
			//One painter: the relay has one procedure painter, one tooth-initial painter and one hardware painter.
			string relay=SourceText.ReadCode(RELAY);
			Assert.AreEqual(1,Regex.Matches(relay,@"switch\(procedureCode\.PaintType\)").Count,"One paint switch.");
			Assert.AreEqual(1,Regex.Matches(relay,@"case ToothInitialType\.Rotate:").Count,"One tooth-initial loop.");
			Assert.AreEqual(1,Regex.Matches(relay,@"OrthoHardwares\.GetWires\(").Count,"One hardware loop.");
		}
		#endregion The painting code is the painter's

		#region Settings and engines
		[TestMethod]
		public void EightSettingsSites_CallTheRelay() {
			//Six copies of the computer's chart settings, the graphical perio chart's engine and, since TC-8, Setup > Graphics' preview (before TC-8 its
			//DirectX format test, ToothChartWrapper.TestDirectXFormat).
			//Since TC-9a every call is the same (the ToothChartSettingsUse argument is gone), so ControlChart's two sites (InitializeLocalData and
			//InitializeOnStartup) are one call twice.
			Dictionary<string,string[]> dictSites=new Dictionary<string,string[]> {
				{ CONTROL_CHART,new[] { "ToothChartRelay.ApplyComputerSettings(toothChartWrapper);",
					"ToothChartRelay.ApplyComputerSettings(toothChartWrapper);" } },
				{ CONTROL_TREAT,new[] { "ToothChartRelay.ApplyComputerSettings(_toothChartWrapper);" } },
				{ SHEET_PRINTING,new[] { "ToothChartRelay.ApplyComputerSettings(toothChartWrapper);" } },
				{ FORM_SHEET_DEF_EDIT,new[] { "ToothChartRelay.ApplyComputerSettings(toothChartWrapper);" } },
				{ FORM_BIG,new[] { "ToothChartRelay.ApplyComputerSettings(toothChartWrapper);" } },
				{ FORM_PERIO_GRAPHICAL,new[] { "ToothChartRelay.ApplyGraphicalPerioMode(toothChartWrapper);" } },
				{ FORM_GRAPHICS,new[] { "ToothChartRelay.ApplyGraphicsPreview(toothChartWrapper,is3DChart,checkDrawInSoftware.Checked);" } },
			};
			int countSites=0;
			foreach(KeyValuePair<string,string[]> pair in dictSites) {
				string code=SourceText.ReadCode(pair.Key);
				foreach(string call in pair.Value.Distinct()) {
					int countListed=pair.Value.Count(x => x==call);
					Assert.AreEqual(countListed,Regex.Matches(code,Regex.Escape(call)).Count,pair.Key+": "+call);
					countSites+=countListed;
				}
			}
			Assert.AreEqual(8,countSites);
			int countCallsInProgram=SourceText.GetCsFiles("OpenPmsDental").Sum(x => Regex.Matches(SourceText.ReadCode(x),
				@"ToothChartRelay\.ApplyComputerSettings\(|ToothChartRelay\.ApplyGraphicalPerioMode\(|ToothChartRelay\.ApplyGraphicsPreview\(").Count);
			Assert.AreEqual(8,countCallsInProgram,"No other program file applies chart settings.");
			//Setup > Graphics saves through the relay's save rule, and nothing in the program tests or searches a DirectX format any more.
			Assert.AreEqual(1,Regex.Matches(SourceText.ReadCode(FORM_GRAPHICS),
				Regex.Escape("ToothChartRelay.SetGraphicsChoice(ComputerPrefCur,_isChart3DOnAllComputersLoaded,isChart3DOnAllComputers,GetChart3DThisComputerChosen(),")).Count);
			Assert.AreEqual(0,SourceText.GetCsFiles("OpenPmsDental").Sum(x => Regex.Matches(SourceText.ReadCode(x),
				@"\.TestDirectXFormat\(|\.GetPreferredDirectXFormat\(").Count),"No program file tests or searches a DirectX format.");
			//Nothing else in the program sets a chart's engine or its settings: only the wrapper's designer defaults remain (since TC-9a DrawMode's; the
			//old engines' DeviceFormat, UseHardware and PreferredPixelFormatNumber are gone, and the regex still catches one that comes back).
			Regex regexSet=new Regex(@"toothChartWrapper\w*\.(DrawMode|DeviceFormat|UseHardware|PreferredPixelFormatNumber|Engine|WpfDrawInSoftware)\s*=(?!=)\s*(?<value>[^;]*);",
				RegexOptions.IgnoreCase);
			string[] arrayDesignerDefaults=new[] { "null","OpenPmsDentalBusiness.DrawingMode.Simple2D","0","false" };
			List<string> listProblems=new List<string>();
			foreach(string relative in SourceText.GetCsFiles("OpenPmsDental")) {
				if(relative.EndsWith(".designer.cs",StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				foreach(Match match in regexSet.Matches(SourceText.ReadCode(relative))) {
					if(!arrayDesignerDefaults.Contains(match.Groups["value"].Value.Trim())) {
						listProblems.Add(relative+": "+match.Value);
					}
				}
			}
			Assert.AreEqual(0,listProblems.Count,"Chart settings set outside the relay: "+string.Join("; ",listProblems));
		}

		[TestMethod]
		public void Program_ChoosesNoEngineByItsDrawingModeValue() {
			//Since TC-8 there is no exception: Setup > Graphics reads and saves the stored mode through the relay (Is3DChart, SetGraphicsChoice).
			Regex regex=new Regex(@"[!=]=\s*\(?\s*(OpenPmsDentalBusiness\.)?DrawingMode\b|\bDrawingMode\.\w+\s*[!=]=|\bcase\s+(OpenPmsDentalBusiness\.)?DrawingMode\.|switch\s*\([^)]*GraphicsSimple");
			List<string> listProblems=new List<string>();
			foreach(string relative in SourceText.GetCsFiles("OpenPmsDental")) {
				foreach(Match match in regex.Matches(SourceText.ReadCode(relative))) {
					listProblems.Add(relative+": "+match.Value);
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems));
			//The checks are the relay's helpers, which TC-8b changed: Open Dental's 3D settings choose the WPF chart, and a computer that does not use it
			//draws Simple 2D, the one older technology left; the snapshot compares the engine and the software switch; the start-up search is gone.
			string relay=SourceText.ReadCode(RELAY);
			Assert.IsFalse(relay.Contains("GetEngineForStoredMode"),"TC-8b: the engine is the computer's (GetEngineForComputer), not the stored mode's.");
			SourceText.AssertInOrder(SourceText.GetMethod(relay,"public static ToothChartEngine GetEngineForComputer(bool isChart3DOnAllComputers,ComputerPref computerPref) {"),
				"GetEngineForComputer","if(IsChart3DUsed(isChart3DOnAllComputers,GetChart3DThisComputer(computerPref))) {","return ToothChartEngine.Wpf3D;",
				"return ToothChartEngine.Simple2D;");
			StringAssert.Contains(relay,
				"|| ToothChartRelay.GetEngineForComputer(ComputerPrefs.LocalComputer)!=(ToothChartEngine)_dictionaryChartPrefsCache[\"ToothChartEngine\"]");
			StringAssert.Contains(relay,"|| ToothChartRelay.IsDrawnInSoftware(ComputerPrefs.LocalComputer)!=(bool)_dictionaryChartPrefsCache[\"WpfDrawInSoftware\"]");
			StringAssert.DoesNotMatch(relay,new Regex(@"_dictionaryChartPrefsCache\.Add\(""(GraphicsUseHardware|PreferredPixelFormatNum|DirectXFormat|GraphicsSimple)"""),
				"The snapshot drops the DirectX and OpenGL fields.");
			//The start-up DirectX search, a no-op since TC-8, went with the DirectX chart in TC-9a.
			Assert.IsFalse(relay.Contains("ChooseStartupDirectXFormat"),"The relay has no start-up DirectX search.");
		}

		[TestMethod]
		public void NothingSelectsDirectXOrOpenGL() {
			//TC-8: the program and the relay start only the WPF chart and the 2D chart. A computer's settings reach the wrapper as the engine it draws
			//with (GetEngineForComputer, TC-8b), never as DrawMode or as the old engines; since TC-9a ToothChartEngine has no DirectX or OpenGL at all.
			Regex regexSelect=new Regex(@"\bToothChartEngine\.(DirectX|OpenGL)\b|\bDrawingMode\.OpenGL\b|(\bDrawMode|\.DeviceFormat|\.UseHardware|\.PreferredPixelFormatNumber)\s*=(?!=)\s*(?<value>[^;]*);");
			//The wrappers' designer defaults (in the designer files, and in ControlChart.cs, whose designer code is in the file itself).
			string[] arrayDesignerDefaults=new[] { "null","OpenPmsDentalBusiness.DrawingMode.Simple2D","0","false" };
			List<string> listProblems=new List<string>();
			foreach(string relative in SourceText.GetCsFiles("OpenPmsDental").Concat(new[] { RELAY })) {
				foreach(Match match in regexSelect.Matches(SourceText.ReadCode(relative))) {
					if(match.Groups["value"].Success && arrayDesignerDefaults.Contains(match.Groups["value"].Value.Trim())) {
						continue;
					}
					listProblems.Add(relative+": "+match.Value);
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems));
			//The wrapper: a stored DirectX or OpenGL request (DrawMode) starts Wpf3D, and a stored Simple 2D the 2D chart.
			string wrapper=SourceText.ReadCode(@"SparksToothChart\ToothChartWrapper.cs");
			SourceText.AssertInOrder(wrapper,"ToothChartWrapper.DrawMode's setter","public DrawingMode DrawMode{","if(value!=DrawingMode.Simple2D) {",
				"Engine=ToothChartEngine.Wpf3D;","return;","StartSimple2D();");
			CollectionAssert.AreEqual(new[] { "Simple2D","Wpf3D" },Enum.GetNames(typeof(SparksToothChart.ToothChartEngine)),"The engines left after TC-9a.");
		}

		[TestMethod]
		public void Program_NamesNoOldEngineLibraryOrType_ExceptTheLicenceTexts() {
			//Since TC-8 FormGraphics is no exception: it was rebuilt without the DirectX format grid and the OpenGL pixel grid.
			Regex regex=new Regex(@"\bSharpDX\b|\bTao\b|\bToothChartDirectX\b|\bToothChartOpenGL\b|\bOpenGLWinFormsControl\b");
			List<string> listAllowed=new List<string>(_arrayLicenceFiles);
			List<string> listProblems=new List<string>();
			foreach(string relative in SourceText.GetCsFiles("OpenPmsDental")) {
				if(listAllowed.Contains(relative,StringComparer.OrdinalIgnoreCase)) {
					continue;
				}
				foreach(Match match in regex.Matches(SourceText.ReadCode(relative))) {
					listProblems.Add(relative+": "+match.Value);
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("; ",listProblems));
			string chart=SourceText.Read(CONTROL_CHART);
			//TC-9a: the DirectX chart's device exception (IsGraphicsDeviceException, SharpDX's) went with it, and Save to Images keeps its one catch.
			Assert.IsFalse(chart.Contains("IsGraphicsDeviceException"),"Save to Images catches no graphics device exception any more.");
			SourceText.AssertInOrder(SourceText.GetMethod(SourceText.ReadCode(CONTROL_CHART),"private void menuItemChartSave_Click(object sender,EventArgs e) {"),
				"menuItemChartSave_Click","ImageStore.Import(bitmapChart,defNum,ImageType.Photo,Pd.Patient,doPrintHeading:true);","catch(Exception ex) {",
				"MessageBox.Show(Lan.g(this,\"Unable to save file: \")+ex.Message);");
			StringAssert.DoesNotMatch(chart,new Regex(@"#\s*if\s+EHRTEST|using\s+EHR\s*;"),"The dead EHRTEST block is gone.");
		}

		[TestMethod]
		public void FormGraphics_HasOpenDentalsLayout_AndNamesNoEngineType() {
			//TC-8b (ENG-1): File > Graphics on Open Dental's layout (https://www.opendental.com/manual/graphics.html) with the 3D tooth chart in DirectX
			//11's place. It names no engine: no engine type, no DrawingMode value, no DirectX format or OpenGL pixel format; its settings are the
			//relay's (IsChart3DOnAllComputers, GetChart3DThisComputer, IsDrawnInSoftware, GetChart3DStatus, ApplyGraphicsPreview, SetGraphicsChoice,
			//SetChart3DOnAllComputers).
			string code=SourceText.ReadCode(FORM_GRAPHICS);
			string designer=SourceText.ReadCode(@"OpenPmsDental\Forms\FormGraphics.designer.cs");
			StringAssert.DoesNotMatch(code+designer,new Regex(@"\bToothChartEngine\b|\bDrawingMode\b|DirectX|OpenGL|\bSharpDX\b|\bTao\b|PixelFormat|gridFormats"),
				"FormGraphics names an engine.");
			string load=SourceText.GetMethod(code,"private void FormGraphics_Load(object sender,EventArgs e) {");
			//The choices as stored, then the status, then Open Dental's medical wording (its own Load words its older choices and options so), then
			//the preview.
			SourceText.AssertInOrder(load,"FormGraphics.FormGraphics_Load",
				"_isChart3DOnAllComputersLoaded=ToothChartRelay.IsChart3DOnAllComputers();",
				"radioChart3DAllUse.Checked=_isChart3DOnAllComputersLoaded;",
				"YN chart3DThisComputer=ToothChartRelay.GetChart3DThisComputer(ComputerPrefCur);",
				"radioSimpleChart.Checked=true;",
				"checkDrawInSoftware.Checked=ToothChartRelay.IsDrawnInSoftware(ComputerPrefCur);",
				"FillStatus();",
				"if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {",
				"radioSimpleChart.Text=\"Use Simple Graphics\";",
				"groupOptions.Text=\"Options For 3D Graphics\";",
				"RefreshPreview();");
			StringAssert.Contains(SourceText.GetMethod(code,"private void FillStatus() {"),"string status=ToothChartRelay.GetChart3DStatus();");
			SourceText.AssertInOrder(SourceText.GetMethod(code,"private void butSave_Click(object sender,System.EventArgs e) {"),"FormGraphics.butSave_Click",
				"ComputerPref computerPrefOld=ComputerPrefCur.Copy();",
				"ToothChartRelay.SetGraphicsChoice(ComputerPrefCur,_isChart3DOnAllComputersLoaded,isChart3DOnAllComputers,GetChart3DThisComputerChosen(),",
				"ComputerPrefs.Update(ComputerPrefCur,computerPrefOld);",
				"if(ToothChartRelay.SetChart3DOnAllComputers(isChart3DOnAllComputers)) {",
				"SecurityLogs.MakeLogEntry(EnumPermType.GraphicsEdit,0,",
				"DataValid.SetInvalid(InvalidType.Prefs);",
				"DialogResult=DialogResult.OK;");
			//Open Dental's groups and words, in its order: the global setting, this computer's, the status, the older technology, the options, then
			//the fork's preview. The status cannot be changed by hand.
			string[] arrayGroups=new[] { "this.groupChart3DAll.Text = \"3D Tooth Chart on all computers\";",
				"this.groupChart3DThis.Text = \"3D Tooth Chart\";",
				"this.groupStatus.Text = \"3D Tooth Chart status on this computer\";",
				"this.groupOlder.Text = \"Older Tooth Chart Technology\";",
				"this.groupOptions.Text = \"Options For The 3D Tooth Chart\";",
				"this.groupPreview.Text = \"Preview\";" };
			SourceText.AssertInOrder(designer,"FormGraphics.designer",arrayGroups);
			foreach(string text in new[] {
				"this.labelChart3DAll.Text = \"This option affects all computers in the office.  All other options are specific to this computer.\";",
				"this.radioChart3DAllUse.Text = \"Use if available.",
				"this.radioChart3DAllDontUse.Text = \"Do not use.  Advantage is that older or underpowered hardware might work better.  This is an easy first step for troubleshooting certain graphics problems.\";",
				"this.radioChart3DThisGlobal.Text = \"Use the global setting\";",
				"this.radioChart3DThisUse.Text = \"Use if available\";",
				"this.radioChart3DThisDontUse.Text = \"Do not use\";",
				"this.radioStatusAvailable.Text = \"Available\";",
				"this.radioStatusFellBack.Text = \"Fell back: ",
				"this.radioSimpleChart.Text = \"Simple 2D (pie shapes)\";",
				"this.labelOptions.Text = \"Most users will never need to change any of these options.  These are only used when the 3D tooth chart is not working properly.\";",
				"this.checkDrawInSoftware.Text = \"Draw in software\";",
				"this.radioStatusAvailable.AutoCheck = false;",
				"this.radioStatusFellBack.AutoCheck = false;",
				"this.toothChartWrapper = new SparksToothChart.ToothChartWrapper();" })
			{
				StringAssert.Contains(designer,text);
			}
			//Eight radio buttons: two global, three for this computer, two for the status, one older technology (Simple 2D, the one left after TC-9a).
			Assert.AreEqual(8,Regex.Matches(designer,@"=\s*new System\.Windows\.Forms\.RadioButton\(\);").Count);
			Assert.IsFalse(designer.Contains("radio3DChart"),"TC-8's two-choice radio is gone.");
		}

		[TestMethod]
		public void PerioGate_RefusesOnSimple2D_WithAMessageNamingFileGraphics_ENG5() {
			//TC-8b (ENG-5, revises O-31): the gate opens only on a computer that uses the 3D tooth chart, and checks no DirectX runtime.
			string relay=SourceText.ReadCode(RELAY);
			string gate=SourceText.GetMethod(relay,"public static bool CanOpenGraphicalPerio(bool isChart3DOnAllComputers,ComputerPref computerPref,out string message) {");
			StringAssert.DoesNotMatch(gate,new Regex(@"d3dx9|IsDirectXRuntimeAvailable|DrawingMode|GraphicsSimple"),"The perio gate checks no DirectX.");
			SourceText.AssertInOrder(gate,"CanOpenGraphicalPerio","if(GetEngineForComputer(isChart3DOnAllComputers,computerPref)!=ToothChartEngine.Wpf3D) {",
				"message=PerioNeeds3DChartMessage;","return false;","message=\"\";","return true;");
			StringAssert.Contains(ToothChartRelay.PerioNeeds3DChartMessage,"File > Graphics");
			SourceText.AssertInOrder(SourceText.GetMethod(SourceText.ReadCode(FORM_PERIO),"private void butGraphical_Click(object sender,EventArgs e) {"),
				"FormPerio.butGraphical_Click","if(!ToothChartRelay.CanOpenGraphicalPerio(out string message)) {","MsgBox.Show(this,message);","return;");
		}

		[TestMethod]
		public void DeveloperSwitch_IsGone() {
			//TC-2's DEBUG switch (OPMS_TOOTHCHART_WPF=1) chose the WPF chart before TC-8 made it every computer's chart. TC-8 removed it.
			foreach(string folder in new[] { "OpenPmsDental","WpfControlsOD","SparksToothChart","OpenPmsDentalBusiness" }) {
				foreach(string relative in SourceText.GetCsFiles(folder)) {
					string text=SourceText.Read(relative);
					Assert.IsFalse(text.Contains("OPMS_TOOTHCHART_WPF"),relative+" still names the developer switch.");
					Assert.IsFalse(text.Contains("IsWpfDeveloperSwitchOn"),relative+" still names the developer switch.");
				}
			}
		}

		[TestMethod]
		public void MainWindow_UsesTheRelaysSnapshot_AndHasNoStartupSearch() {
			string code=SourceText.ReadCode(FORM_OPEN_DENTAL);
			//TC-9a: the start-up DirectX search (a no-op since TC-8) and its call are gone, with the chart restart that followed it.
			Assert.IsFalse(code.Contains("ChooseStartupDirectXFormat"),"FormOpenDental has no start-up DirectX search.");
			Assert.IsFalse(code.Contains("Logger.LogToPath(\"DirectXFormat\""),"No start-up phase of the DirectX search is logged.");
			StringAssert.Contains(code,"private ToothChartSettingsSnapshot _toothChartSettingsSnapshot=new ToothChartSettingsSnapshot();");
			SourceText.AssertInOrder(code,"FormOpenDental refresh","_dictionaryTaskListPrefsCache.Clear();","_toothChartSettingsSnapshot.Take();",
				"_dictionaryTaskListPrefsCache.Add(\"TaskDock\"");
			SourceText.AssertInOrder(SourceText.GetMethod(code,"private bool NeedsRedraw(string section) {"),"NeedsRedraw",
				"case \"ChartModule\":","if(_toothChartSettingsSnapshot.HasChanged()) {","return true;","case \"TaskLists\":","catch {","return true;");
			Assert.IsFalse(code.Contains("_dictionaryChartPrefsCache"));
			StringAssert.Contains(SourceText.ReadCode(FORM_PERIO),"if(!ToothChartRelay.CanOpenGraphicalPerio(out string message)) {");
		}
		#endregion Settings and engines
	}
}
