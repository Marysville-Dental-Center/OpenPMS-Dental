// Source tests of the tooth chart's callers (package TC-2, docs/specs/wpf-tooth-chart-plan.md, sections 4 and 7): the callers hold no chart
// code of their own any more. Their painting is ToothChartPainter's, in the order PainterCallers (the call-log drivers) follows; their chart
// settings are the relay's helpers; no program file outside the relay chooses an engine by its DrawingMode value or names the old engines'
// libraries and types, except the licence texts for good (FormGraphics too until TC-8 rebuilt it). Since TC-8 nothing selects DirectX or
// OpenGL, Setup > Graphics names no engine and the WPF developer switch of TC-2 is gone.
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
		///<summary>The licence window and its resources name SharpDX and Tao for good (their notices), until TC-9b removes those entries.</summary>
		private static readonly string[] _arrayLicenceFiles=new[] { @"OpenPmsDental\Forms\FormLicense.cs",@"OpenPmsDental\Properties\Resources.Designer.cs" };

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
				"ToothChartRelay.ApplyComputerSettings(_toothChartWrapper,ToothChartSettingsUse.StoredMode);",
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
				"ToothChartRelay.ApplyComputerSettings(toothChartWrapper,ToothChartSettingsUse.StoredMode);",
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
				"ToothChartRelay.ApplyComputerSettings(toothChartWrapper,ToothChartSettingsUse.ModeByName);");
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
			Dictionary<string,string[]> dictSites=new Dictionary<string,string[]> {
				{ CONTROL_CHART,new[] { "ToothChartRelay.ApplyComputerSettings(toothChartWrapper,ToothChartSettingsUse.StoredMode);",
					"ToothChartRelay.ApplyComputerSettings(toothChartWrapper,ToothChartSettingsUse.StartupMode);" } },
				{ CONTROL_TREAT,new[] { "ToothChartRelay.ApplyComputerSettings(_toothChartWrapper,ToothChartSettingsUse.StoredMode);" } },
				{ SHEET_PRINTING,new[] { "ToothChartRelay.ApplyComputerSettings(toothChartWrapper,ToothChartSettingsUse.StoredMode);" } },
				{ FORM_SHEET_DEF_EDIT,new[] { "ToothChartRelay.ApplyComputerSettings(toothChartWrapper,ToothChartSettingsUse.ModeByName);" } },
				{ FORM_BIG,new[] { "ToothChartRelay.ApplyComputerSettings(toothChartWrapper,ToothChartSettingsUse.ModeByName);" } },
				{ FORM_PERIO_GRAPHICAL,new[] { "ToothChartRelay.ApplyGraphicalPerioMode(toothChartWrapper);" } },
				{ FORM_GRAPHICS,new[] { "ToothChartRelay.ApplyGraphicsPreview(toothChartWrapper,radio3DChart.Checked,checkDrawInSoftware.Checked);" } },
			};
			int countSites=0;
			foreach(KeyValuePair<string,string[]> pair in dictSites) {
				string code=SourceText.ReadCode(pair.Key);
				foreach(string call in pair.Value) {
					Assert.AreEqual(1,Regex.Matches(code,Regex.Escape(call)).Count,pair.Key+": "+call);
					countSites++;
				}
			}
			Assert.AreEqual(8,countSites);
			int countCallsInProgram=SourceText.GetCsFiles("OpenPmsDental").Sum(x => Regex.Matches(SourceText.ReadCode(x),
				@"ToothChartRelay\.ApplyComputerSettings\(|ToothChartRelay\.ApplyGraphicalPerioMode\(|ToothChartRelay\.ApplyGraphicsPreview\(").Count);
			Assert.AreEqual(8,countCallsInProgram,"No other program file applies chart settings.");
			//Setup > Graphics saves through the relay's save rule, and nothing in the program tests or searches a DirectX format any more.
			Assert.AreEqual(1,Regex.Matches(SourceText.ReadCode(FORM_GRAPHICS),
				Regex.Escape("ToothChartRelay.SetGraphicsChoice(ComputerPrefCur,radio3DChart.Checked,checkDrawInSoftware.Checked);")).Count);
			Assert.AreEqual(0,SourceText.GetCsFiles("OpenPmsDental").Sum(x => Regex.Matches(SourceText.ReadCode(x),
				@"\.TestDirectXFormat\(|\.GetPreferredDirectXFormat\(").Count),"No program file tests or searches a DirectX format.");
			//Nothing else in the program sets a chart's engine or its settings: only the wrapper's designer defaults remain.
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
			//The checks are the relay's helpers, which TC-8 changed: the stored 1 is the 2D chart, everything else the WPF chart; the snapshot compares
			//the engine and the software switch; the perio gate and the start-up search check nothing.
			string relay=SourceText.ReadCode(RELAY);
			SourceText.AssertInOrder(SourceText.GetMethod(relay,"public static ToothChartEngine GetEngineForStoredMode(DrawingMode drawingMode) {"),
				"GetEngineForStoredMode","if(drawingMode==DrawingMode.Simple2D) {","return ToothChartEngine.Simple2D;","return ToothChartEngine.Wpf3D;");
			StringAssert.Contains(relay,
				"|| ToothChartRelay.GetEngineForStoredMode(ComputerPrefs.LocalComputer.GraphicsSimple)!=(ToothChartEngine)_dictionaryChartPrefsCache[\"ToothChartEngine\"]");
			StringAssert.Contains(relay,"|| ToothChartRelay.IsDrawnInSoftware(ComputerPrefs.LocalComputer)!=(bool)_dictionaryChartPrefsCache[\"WpfDrawInSoftware\"]");
			StringAssert.DoesNotMatch(relay,new Regex(@"_dictionaryChartPrefsCache\.Add\(""(GraphicsUseHardware|PreferredPixelFormatNum|DirectXFormat|GraphicsSimple)"""),
				"The snapshot drops the DirectX and OpenGL fields.");
			SourceText.AssertInOrder(SourceText.GetMethod(relay,"public static bool ChooseStartupDirectXFormat(Control controlHost) {"),
				"ChooseStartupDirectXFormat","return false;");
		}

		[TestMethod]
		public void NothingSelectsDirectXOrOpenGL() {
			//TC-8: the program and the relay start only the WPF chart and the 2D chart. The stored DirectX and OpenGL choices reach the wrapper as the
			//engine of the stored mode (GetEngineForStoredMode), never as DrawMode or as the old engines; only the legacy recorder and the tests set
			//ToothChartEngine.DirectX or OpenGL, until TC-9a.
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
			//The wrapper: a stored DirectX or OpenGL request (DrawMode) starts Wpf3D, and only Engine reaches the old engines.
			string wrapper=SourceText.ReadCode(@"SparksToothChart\ToothChartWrapper.cs");
			SourceText.AssertInOrder(wrapper,"ToothChartWrapper.DrawMode's setter","public DrawingMode DrawMode{","if(value!=DrawingMode.Simple2D) {",
				"Engine=ToothChartEngine.Wpf3D;","return;","StartDrawingModeEngine(value);");
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
			StringAssert.Contains(chart,"catch(Exception ex) when(ToothChartWrapper.IsGraphicsDeviceException(ex)) {");
			StringAssert.DoesNotMatch(chart,new Regex(@"#\s*if\s+EHRTEST|using\s+EHR\s*;"),"The dead EHRTEST block is gone.");
		}

		[TestMethod]
		public void FormGraphics_NamesNoEngineType_AndHasTwoChoicesAndTheSoftwareSwitch() {
			//TC-8: Setup > Graphics names no engine: no engine type, no DrawingMode value, no DirectX format or OpenGL pixel format. Its choices are
			//the relay's (Is3DChart, IsDrawnInSoftware, ApplyGraphicsPreview, SetGraphicsChoice).
			string code=SourceText.ReadCode(FORM_GRAPHICS);
			string designer=SourceText.ReadCode(@"OpenPmsDental\Forms\FormGraphics.designer.cs");
			StringAssert.DoesNotMatch(code+designer,new Regex(@"\bToothChartEngine\b|\bDrawingMode\b|DirectX|OpenGL|\bSharpDX\b|\bTao\b|PixelFormat|gridFormats"),
				"FormGraphics names an engine.");
			StringAssert.Contains(code,"radio3DChart.Checked=ToothChartRelay.Is3DChart(ComputerPrefCur);");
			StringAssert.Contains(code,"checkDrawInSoftware.Checked=ToothChartRelay.IsDrawnInSoftware(ComputerPrefCur);");
			//A medical practice or clinic keeps Open Dental's plain "Graphics" wording (main's FormGraphics_Load did the same with the old radio buttons).
			SourceText.AssertInOrder(SourceText.GetMethod(code,"private void FormGraphics_Load(object sender,EventArgs e) {"),"FormGraphics.FormGraphics_Load",
				"radio3DChart.Checked=ToothChartRelay.Is3DChart(ComputerPrefCur);",
				"if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {",
				"groupToothChart.Text=\"Graphics\";",
				"radio3DChart.Text=\"Use 3D Graphics (recommended)\";",
				"radioSimpleChart.Text=\"Use Simple Graphics\";",
				"RefreshPreview();");
			SourceText.AssertInOrder(SourceText.GetMethod(code,"private void butSave_Click(object sender,System.EventArgs e) {"),"FormGraphics.butSave_Click",
				"ComputerPref computerPrefOld=ComputerPrefCur.Copy();",
				"ToothChartRelay.SetGraphicsChoice(ComputerPrefCur,radio3DChart.Checked,checkDrawInSoftware.Checked);",
				"ComputerPrefs.Update(ComputerPrefCur,computerPrefOld);","DialogResult=DialogResult.OK;");
			//Two choices and the check box, by the names the window checks (WindowChecksTests) look for.
			Assert.AreEqual(2,Regex.Matches(designer,@"=\s*new System\.Windows\.Forms\.RadioButton\(\);").Count);
			StringAssert.Contains(designer,"this.radio3DChart.Text = \"3D chart\";");
			StringAssert.Contains(designer,"this.radioSimpleChart.Text = \"Simple 2D chart\";");
			StringAssert.Contains(designer,"this.checkDrawInSoftware.Text = \"Draw in software\";");
			StringAssert.Contains(designer,"this.toothChartWrapper = new SparksToothChart.ToothChartWrapper();");
		}

		[TestMethod]
		public void PerioGate_HasNoDirectXCheck_O31() {
			string gate=SourceText.GetMethod(SourceText.ReadCode(RELAY),"public static bool CanOpenGraphicalPerio(out string message) {");
			StringAssert.DoesNotMatch(gate,new Regex(@"d3dx9|IsDirectXRuntimeAvailable|DrawingMode|GraphicsSimple"),"The perio gate checks no DirectX.");
			SourceText.AssertInOrder(gate,"CanOpenGraphicalPerio","message=\"\";","return true;");
			StringAssert.Contains(SourceText.ReadCode(FORM_PERIO),"if(!ToothChartRelay.CanOpenGraphicalPerio(out string message)) {");
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
		public void MainWindow_UsesTheRelaysStartupSearchAndSnapshot() {
			string code=SourceText.ReadCode(FORM_OPEN_DENTAL);
			SourceText.AssertInOrder(code,"FormOpenDental start-up","if(ToothChartRelay.ChooseStartupDirectXFormat(this)) {","try {",
				"controlChart.InitializeOnStartup();","catch(Exception) {");
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
