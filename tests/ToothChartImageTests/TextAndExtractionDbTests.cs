// Package TC-5 on the scratch database of the call-log tests (ChartTestDb, synthetic patients only): the extraction graphic's preference row
// (O-20; as tests/P262MiscTests/RecallSwitchDbTests.cs checks the fork's other rows), the painter with the setting on and off against the Chart
// module's date slider, and the free-text add, move and save round trip of the Chart module (ControlChart.butAddText_Click and
// toothChartRelay_TextMoved) through the WPF chart's image host, under en-US and de-DE. The Chart preferences page is checked in its source.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	[TestClass]
	public class TextAndExtractionDbTests {
		private static string CountRows() {
			return DataCore.GetScalar("SELECT COUNT(*) FROM preference WHERE PrefName='"+POut.String(ToothChartExtractionGraphic.PREF_NAME)+"'");
		}

		private static void DeleteRow() {
			DataCore.NonQ("DELETE FROM preference WHERE PrefName='"+POut.String(ToothChartExtractionGraphic.PREF_NAME)+"'");
			Prefs.RefreshCache();
		}

		#region The setting
		[TestMethod]
		public void NewDatabase_HasNoSettingRow_SoTheGraphicIsOff() {
			CallLogFixtures.EnsureBuilt();
			string text=System.IO.File.ReadAllText(System.IO.Path.Combine(TestPaths.RepoRoot,"db","init","new-database.sql"));
			Assert.IsFalse(text.Contains(ToothChartExtractionGraphic.PREF_NAME),"No migration step or init row: the office turns the setting on.");
			Assert.IsFalse(ToothChartExtractionGraphic.IsOn());
		}

		[TestMethod]
		public void Setting_MissingRowReadsOff_FirstSaveInsertsTheRow_ThenOneRowIsUpdated() {
			CallLogFixtures.EnsureBuilt();
			try {
				DeleteRow();
				Assert.IsFalse(ToothChartExtractionGraphic.IsOn(),"No row: off.");
				Assert.IsFalse(ToothChartExtractionGraphic.Save(false),"Saving off when off changes nothing.");
				Assert.AreEqual("0",CountRows(),"Nothing is written while the setting stays off.");
				Assert.IsTrue(ToothChartExtractionGraphic.Save(true));
				Assert.IsTrue(ToothChartExtractionGraphic.IsOn());
				Assert.AreEqual("1",DataCore.GetScalar("SELECT ValueString FROM preference WHERE PrefName='"+ToothChartExtractionGraphic.PREF_NAME+"'"));
				Prefs.RefreshCache();
				Assert.IsTrue(ToothChartExtractionGraphic.IsOn(),"Read back from the database.");
				Assert.IsTrue(ToothChartExtractionGraphic.Save(false));
				Prefs.RefreshCache();
				Assert.IsFalse(ToothChartExtractionGraphic.IsOn());
				Assert.AreEqual("1",CountRows(),"The row is updated, never doubled.");
				Assert.AreEqual("0",DataCore.GetScalar("SELECT ValueString FROM preference WHERE PrefName='"+ToothChartExtractionGraphic.PREF_NAME+"'"));
				Assert.IsFalse(Enum.GetNames(typeof(PrefName)).Contains(ToothChartExtractionGraphic.PREF_NAME),"Open Dental's name is not public: not in PrefName.");
				StringAssert.StartsWith(ToothChartExtractionGraphic.PREF_NAME,"Fork","The fork's own row, like the other fork-owned preference rows.");
				Assert.AreEqual("",ToothChartExtractionGraphic.GetChangeLogLine(true,true));
				Assert.AreEqual("Show completed extractions with an X on the tooth chart changed from 'False' to 'True'",
					ToothChartExtractionGraphic.GetChangeLogLine(false,true));
			}
			finally {
				DeleteRow();
			}
		}

		[TestMethod]
		public void SettingsSnapshot_SeesTheSetting_SoTheChartModuleRedraws() {
			CallLogFixtures.EnsureBuilt();
			try {
				DeleteRow();
				ToothChartSettingsSnapshot snapshot=new ToothChartSettingsSnapshot();
				Assert.IsTrue(snapshot.HasChanged(),"Nothing taken yet.");
				snapshot.Take();
				Assert.IsFalse(snapshot.HasChanged());
				ToothChartExtractionGraphic.Save(true);
				Assert.IsTrue(snapshot.HasChanged(),"Turning the graphic on redraws the Chart module.");
				snapshot.Take();
				Assert.IsFalse(snapshot.HasChanged());
			}
			finally {
				DeleteRow();
			}
		}

		[TestMethod]
		public void PreferencesPage_ShowsAndSavesTheSetting_WithAnAuditLineAndTheSignal() {
			string code=SourceText.ReadCode("OpenPmsDental/User Controls/Preferences/UserControlChartGeneral.cs");
			StringAssert.Contains(code,"checkToothChartShowCompletedExtractions.Checked=ToothChartExtractionGraphic.IsOn();");
			StringAssert.Contains(code,"if(!ToothChartExtractionGraphic.Save(isOnNew)) {");
			StringAssert.Contains(code,"Changed=true;");
			StringAssert.Contains(code,"SecurityLogs.MakeLogEntry(EnumPermType.Setup,0,ToothChartExtractionGraphic.GetChangeLogLine(isOnOld,isOnNew));");
			StringAssert.Contains(code,"SaveToothChartExtractionGraphic();\n\t\t\treturn true;");
			string designer=SourceText.Read("OpenPmsDental/User Controls/Preferences/UserControlChartGeneral.Designer.cs");
			StringAssert.Contains(designer,"this.checkToothChartShowCompletedExtractions.Text = \"Show completed extractions with an X on the tooth chart\";");
			StringAssert.Contains(designer,"this.Controls.Add(this.checkToothChartShowCompletedExtractions);");
			//The page's Changed flag is what makes the Preferences window send the preference signal when it closes.
			string codePreferences=SourceText.ReadCode("OpenPmsDental/Forms/FormPreferences.cs");
			StringAssert.Contains(codePreferences,"|| userControlChartGeneral.Changed");
			StringAssert.Contains(codePreferences,"DataValid.SetInvalid(InvalidType.Prefs);");
		}
		#endregion The setting

		#region The painter and the date slider
		///<summary>The SetBigX lines of the Chart module's paint of the slider patient at the date, with the setting as given.</summary>
		private static List<string> GetChartModuleXs(DateTime dateLimit,bool isOn) {
			ChartModuleScene scene=new ChartModuleScene { Name="x",PatNum=CallLogFixtures.PatNumSlider,DateLimit=dateLimit,DateOrthoExam=DateTime.Today };
			CallLogFixtures.CultureScope cultureScope=CallLogFixtures.SetCulture();
			try {
				CallLogFixtures.ApplyUser(new CallLogUser { IsExtractionGraphicOn=isOn });
				return PainterCallers.RunChartModule(scene).ListLines.Where(x => x.StartsWith("SetBigX ")).OrderBy(x => x,StringComparer.Ordinal).ToList();
			}
			finally {
				CallLogFixtures.ApplyUser(CallLogUser.Plain);
				cultureScope.Dispose();
			}
		}

		private static string X(string toothNum,ProcStat procStat) {
			ToothChartPainter.GetToothColors(ProcedureCodes.GetProcCode("Z2001"),procStat,false,out Color colorDark,out Color colorLight);
			return "SetBigX \""+toothNum+"\" "+CallLog.Format(colorDark);
		}

		[TestMethod]
		public void ExtractionGraphic_OnAndOff_AgainstTheDateSlider() {
			CallLogFixtures.EnsureBuilt();
			//The slider patient: extractions of 19 (C on 2022-03-15, planned 2022-01-10), 31 (C on 2022-02-01, planned 2022-01-10), 2 (EO on
			//2022-01-05) and 30 (TP, 2022-02-01); 19, 31 and 2 have their missing initials.
			try {
				//Before every date: nothing, either way.
				CollectionAssert.AreEqual(new string[0],GetChartModuleXs(new DateTime(2021,12,31),false));
				CollectionAssert.AreEqual(new string[0],GetChartModuleXs(new DateTime(2021,12,31),true),"The slider drops extractions it has not reached.");
				//2022-02-10: 31 and 2 are done, 19 is planned (completed after the slider date, so drawn as planned), 30 is planned.
				List<string> listOff=GetChartModuleXs(new DateTime(2022,2,10),false);
				CollectionAssert.AreEquivalent(new[] { X("19",ProcStat.TP),X("30",ProcStat.TP) },listOff,string.Join(", ",listOff));
				List<string> listOn=GetChartModuleXs(new DateTime(2022,2,10),true);
				CollectionAssert.AreEquivalent(new[] { X("19",ProcStat.TP),X("30",ProcStat.TP),X("31",ProcStat.C),X("2",ProcStat.EO) },listOn,
					string.Join(", ",listOn));
				//Today: all three done.
				CollectionAssert.AreEquivalent(new[] { X("30",ProcStat.TP) },GetChartModuleXs(DateTime.Today,false));
				CollectionAssert.AreEquivalent(new[] { X("19",ProcStat.C),X("30",ProcStat.TP),X("31",ProcStat.C),X("2",ProcStat.EO) },
					GetChartModuleXs(DateTime.Today,true));
			}
			finally {
				DeleteRow();
			}
		}

		[TestMethod]
		public void ExtractionGraphic_EveryCallerDrawsIt_TheMissingTeethStay() {
			CallLogFixtures.EnsureBuilt();
			CallLogFixtures.CultureScope cultureScope=CallLogFixtures.SetCulture();
			try {
				CallLogFixtures.ApplyUser(new CallLogUser { IsExtractionGraphicOn=true });
				List<string> listTreat=PainterCallers.RunTreatPlan(new TreatPlanScene { Name="x",PatNum=CallLogFixtures.PatNumSlider,
					FuncProceduresFiltered=x => CallLogFixtures.GetFilteredProcedures(x,true) }).ListLines;
				List<string> listSheet=PainterCallers.RunSheet(new SheetScene { Name="x",PatNum=CallLogFixtures.PatNumSlider,ShowCompleted=true,
					FuncTreatPlan=CallLogFixtures.GetTreatPlanAllTP }).ListLines;
				List<string> listBig=PainterCallers.RunBigChart(new BigChartScene { Name="x",PatNum=CallLogFixtures.PatNumSlider }).ListLines;
				foreach(List<string> listLines in new[] { listTreat,listSheet,listBig }) {
					Assert.IsTrue(listLines.Any(x => x.StartsWith("SetBigX \"31\" ")),string.Join("\n",listLines));
					Assert.IsTrue(listLines.Any(x => x.StartsWith("SetBigX \"2\" ")));
					Assert.IsTrue(listLines.Contains("SetMissing \"31\""),"The tooth stays missing.");
					Assert.IsTrue(listLines.IndexOf("SetMissing \"31\"")<listLines.FindIndex(x => x.StartsWith("SetBigX \"31\" ")),"Missing first, then its X.");
				}
			}
			finally {
				CallLogFixtures.ApplyUser(CallLogUser.Plain);
				cultureScope.Dispose();
				DeleteRow();
			}
		}
		[TestMethod]
		public void ExtractionGraphic_NoXUnderACompletedImplantOrPontic_APlannedOneKeepsIt() {
			//Review of TC-5, item 3: the X of a completed extraction is not drawn where a completed implant or a completed bridge or denture's pontic
			//stands in the tooth's place now. A planned implant or bridge, a hidden one, and a planned extraction keep the X. The painter feeds every
			//engine (WPF, 2D, DirectX and OpenGL), so this call log is what each of them is told.
			CallLogFixtures.EnsureBuilt();
			ProcedureCode codeExtraction=new ProcedureCode { ProcCode="Z9101",PaintType=ToothPaintingType.Extraction,TreatArea=TreatmentArea.Tooth };
			ProcedureCode codeImplant=new ProcedureCode { ProcCode="Z9102",PaintType=ToothPaintingType.Implant,TreatArea=TreatmentArea.Tooth };
			ProcedureCode codeBridgeDark=new ProcedureCode { ProcCode="Z9103",PaintType=ToothPaintingType.BridgeDark,TreatArea=TreatmentArea.Tooth };
			ProcedureCode codeBridgeLight=new ProcedureCode { ProcCode="Z9104",PaintType=ToothPaintingType.BridgeLight,TreatArea=TreatmentArea.Tooth };
			ProcedureCode codeDenture=new ProcedureCode { ProcCode="Z9105",PaintType=ToothPaintingType.DentureDark,TreatArea=TreatmentArea.Arch };
			Func<ProcedureCode,string,ProcStat,ToothChartPaintProc> funcProc=(procedureCode,toothNum,procStat) => new ToothChartPaintProc {
				ProcedureCode=procedureCode,ToothNum=toothNum,ProcStatus=procStat,Surf="",ToothRange="" };
			List<ToothChartPaintProc> listProcs=new List<ToothChartPaintProc>();
			//19 C and 14 EO extracted, with a completed (C) and an existing (EC) implant: no X.
			listProcs.Add(funcProc(codeExtraction,"19",ProcStat.C));
			listProcs.Add(funcProc(codeImplant,"19",ProcStat.C));
			listProcs.Add(funcProc(codeImplant,"14",ProcStat.EC));
			listProcs.Add(funcProc(codeExtraction,"14",ProcStat.EO));
			//30 extracted with a planned implant (as the slider shows one completed after its date): the X stays.
			listProcs.Add(funcProc(codeExtraction,"30",ProcStat.C));
			listProcs.Add(funcProc(codeImplant,"30",ProcStat.TP));
			//4 extracted under the pontic of a completed dark bridge 3-4-5, 12 under an existing light bridge's: no X.
			foreach(string toothNum in new[] { "3","4","5" }) {
				listProcs.Add(funcProc(codeBridgeDark,toothNum,ProcStat.C));
			}
			listProcs.Add(funcProc(codeExtraction,"4",ProcStat.C));
			listProcs.Add(funcProc(codeExtraction,"12",ProcStat.C));
			listProcs.Add(funcProc(codeBridgeLight,"12",ProcStat.EC));
			//20 extracted under the pontic of a planned bridge 19-20-21: the X stays.
			foreach(string toothNum in new[] { "21","20" }) {
				listProcs.Add(funcProc(codeBridgeDark,toothNum,ProcStat.TP));
			}
			listProcs.Add(funcProc(codeExtraction,"20",ProcStat.C));
			//7 extracted under a completed upper denture (its pontic): no X.
			ToothChartPaintProc procDenture=funcProc(codeDenture,"",ProcStat.C);
			procDenture.Surf="U";
			listProcs.Add(procDenture);
			listProcs.Add(funcProc(codeExtraction,"7",ProcStat.C));
			//2 extracted with a hidden completed implant: the X stays (a hidden procedure draws nothing).
			listProcs.Add(funcProc(codeExtraction,"2",ProcStat.C));
			ToothChartPaintProc procImplantHidden=funcProc(codeImplant,"2",ProcStat.C);
			procImplantHidden.IsHidden=true;
			listProcs.Add(procImplantHidden);
			//31 extracted with nothing over it, and a planned extraction of 25, which has a completed implant: both keep the X.
			listProcs.Add(funcProc(codeExtraction,"31",ProcStat.C));
			listProcs.Add(funcProc(codeImplant,"25",ProcStat.C));
			listProcs.Add(funcProc(codeExtraction,"25",ProcStat.TP));
			List<ToothInitial> listToothInitials=new[] { "2","4","7","12","14","19","20","30","31" }
				.Select(x => new ToothInitial { ToothNum=x,InitialType=ToothInitialType.Missing }).ToList();
			Func<bool,List<string>> funcPaint=isOn => {
				CallLogFixtures.ApplyUser(new CallLogUser { IsExtractionGraphicOn=isOn });
				CallLog callLog=new CallLog();
				ToothChartPainter.PaintProcedures(new RecordingPaintTarget(callLog),listProcs,listToothInitials,new ToothChartPaintOptions());
				return callLog.ListLines;
			};
			CallLogFixtures.CultureScope cultureScope=CallLogFixtures.SetCulture();
			try {
				List<string> listOn=funcPaint(true);
				List<string> listXsOn=listOn.Where(x => x.StartsWith("SetBigX ")).Select(x => x.Split(' ')[1].Trim('"')).OrderBy(x => x,StringComparer.Ordinal)
					.ToList();
				CollectionAssert.AreEqual(new[] { "2","20","25","30","31" },listXsOn,string.Join("\n",listOn));
				Assert.IsTrue(listOn.Any(x => x.StartsWith("SetImplant \"19\" ")),"The implant is drawn.");
				Assert.IsTrue(listOn.Any(x => x.StartsWith("SetPontic \"4\" ")),"The pontic is drawn.");
				Assert.IsTrue(listOn.Any(x => x.StartsWith("SetPontic \"7\" ")),"The denture's pontic is drawn.");
				ToothChartPainter.GetToothColors(codeExtraction,ProcStat.C,false,out Color colorDarkC,out Color colorLightC);
				Assert.IsTrue(listOn.Contains("SetBigX \"30\" "+CallLog.Format(colorDarkC)),"The X in its status's colour.");
				//With the setting off: only the planned extraction's X, as always.
				List<string> listOff=funcPaint(false);
				CollectionAssert.AreEqual(new[] { "SetBigX \"25\"" },listOff.Where(x => x.StartsWith("SetBigX ")).Select(x => x.Substring(0,x.LastIndexOf(' ')))
					.ToList(),string.Join("\n",listOff));
			}
			finally {
				CallLogFixtures.ApplyUser(CallLogUser.Plain);
				cultureScope.Dispose();
				DeleteRow();
			}
		}
		#endregion The painter and the date slider

		#region Free text round trip
		[TestMethod]
		public void ChartModuleTextMovedHandler_IsTheOneThisRoundTripCopies() {
			//The round trip below gives the relay's TextMoved a copy of the Chart module's handler; this pins the handler and its subscription in
			//ControlChart's source, so a change there fails here instead of passing the round trip unseen (review of TC-5, item 5).
			string code=SourceText.ReadCode("OpenPmsDental/Main Modules/ControlChart.cs");
			StringAssert.Contains(code,"_toothChartRelay.TextMoved+=toothChartRelay_TextMoved;");
			int indexHandler=code.IndexOf("private void toothChartRelay_TextMoved(object sender,ToothChartTextMovedEventArgs e) {");
			Assert.IsTrue(indexHandler>=0,"The handler.");
			int indexEnd=code.IndexOf("\n\t\t}\n",indexHandler);
			Assert.IsTrue(indexEnd>indexHandler);
			string handler=code.Substring(indexHandler,indexEnd-indexHandler);
			StringAssert.Contains(handler,
				"ToothInitial toothInitial=Pd.ListToothInitials.Find(x => x.ToothInitialNum==e.ToothInitialNum && x.InitialType==ToothInitialType.Text);");
			StringAssert.Contains(handler,"ToothChartRelay.MoveTextLabel(toothInitial,e.Location);");
			StringAssert.Contains(handler,"ToothInitials.Update(toothInitial);");
			StringAssert.Contains(handler,"FillToothChart(retainSelection:true);");
			Assert.IsTrue(handler.IndexOf("ToothChartRelay.MoveTextLabel(")<handler.IndexOf("ToothInitials.Update(")
				&& handler.IndexOf("ToothInitials.Update(")<handler.IndexOf("FillToothChart("),"Move, save, then refill, as the copy does.");
		}

		[TestMethod]
		public void FreeText_AddMoveSave_RoundTrip_EnUs() {
			RoundTrip("en-US");
		}

		[TestMethod]
		public void FreeText_AddMoveSave_RoundTrip_DeDe() {
			RoundTrip("de-DE");
		}

		///<summary>The Chart module's steps with a real WPF chart: Add Text inserts the label at 0,0 (butAddText_Click), the refill paints the
		///patient's tooth initials (ToothChartPainter.PaintToothInitials), a drag with the text mover on the image host raises TextMoved, the handler
		///gives the label its new point (ToothChartRelay.MoveTextLabel) and saves it (toothChartRelay_TextMoved), and the next refill draws it there.
		///The culture is the workstation's for every step, as in the program.</summary>
		private static void RoundTrip(string cultureName) {
			CallLogFixtures.EnsureBuilt();
			StaRunner.Run(() => {
				CultureInfo cultureBefore=Thread.CurrentThread.CurrentCulture;
				Thread.CurrentThread.CurrentCulture=new CultureInfo(cultureName);
				long patNum=0;
				try {
					patNum=OpenPmsDentalBusiness.Crud.PatientCrud.Insert(new Patient { LName="Synthetic",FName="FreeText "+cultureName,PriProv=CallLogFixtures.ProvNum1,
						PatStatus=PatientStatus.Patient,Birthdate=new DateTime(1980,1,1) });
					ToothInitial toothInitialNew=new ToothInitial();
					toothInitialNew.SetText(new PointF(),"Synthetic note");
					toothInitialNew.InitialType=ToothInitialType.Text;
					toothInitialNew.PatNum=patNum;
					toothInitialNew.ColorDraw=Color.DarkBlue;
					ToothInitials.Insert(toothInitialNew);
					using ToothChartWrapper wrapper=WpfCharts.CreateWrapper(new Size(615,461));
					ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
					List<ToothInitial> listToothInitials=null;
					Action actionRefill=() => {
						listToothInitials=ToothInitials.GetPatientData(patNum);
						relay.BeginUpdate();
						relay.ResetTeeth();
						ToothChartPainter.PaintPrimaryTeeth(relay,listToothInitials);
						ToothChartPainter.PaintToothInitials(relay,listToothInitials);
						relay.EndUpdate();
					};
					//A copy of ControlChart.toothChartRelay_TextMoved (Find, MoveTextLabel, ToothInitials.Update, then the refill), which
					//ChartModuleTextMovedHandler_IsTheOneThisRoundTripCopies pins in ControlChart's source.
					relay.TextMoved+=(sender,e) => {
						ToothInitial toothInitial=listToothInitials.Find(x => x.ToothInitialNum==e.ToothInitialNum && x.InitialType==ToothInitialType.Text);
						Assert.IsNotNull(toothInitial);
						ToothChartRelay.MoveTextLabel(toothInitial,e.Location);
						ToothInitials.Update(toothInitial);
						actionRefill();
					};
					actionRefill();
					relay.CursorTool=CursorTool.MoveText;
					Assert.AreEqual(1,wrapper.TcData.ListFreeTexts.Count);
					Assert.AreEqual(new PointF(0,0),wrapper.TcData.ListFreeTexts[0].Location,"Add Text puts the label at 0,0.");
					Assert.AreEqual(toothInitialNew.ToothInitialNum,wrapper.TcData.ListFreeTexts[0].ToothInitialNum);
					FreeTextBox box=ToothSceneBuilder.GetFreeTextBoxes(wrapper.TcData)[0];
					ToothChartImageHost toothChartImageHost=wrapper.ImageHostWpf;
					toothChartImageHost.Size=wrapper.Size;
					Point pointDown=new Point(box.Bounds.X+3,box.Bounds.Y+3);
					//615 pixels wide: the drawing space's 1.5 times; 151 and 92 pixels are 100.67 and 61.33, saved as 101 and 61.
					Point pointUp=new Point(pointDown.X+151,pointDown.Y+92);
					ToothChartMouseTests.RaiseMouse(toothChartImageHost,"OnMouseDown",pointDown);
					ToothChartMouseTests.RaiseMouse(toothChartImageHost,"OnMouseMove",new Point(pointDown.X+60,pointDown.Y+30));
					ToothChartMouseTests.RaiseMouse(toothChartImageHost,"OnMouseUp",pointUp);
					ToothInitial toothInitialSaved=ToothInitials.GetPatientData(patNum).Single(x => x.InitialType==ToothInitialType.Text);
					Assert.AreEqual(new PointF(101,61),toothInitialSaved.GetTextPoint(),cultureName+": "+toothInitialSaved.DrawText);
					Assert.AreEqual("Synthetic note",toothInitialSaved.GetTextString());
					Assert.AreEqual(1,wrapper.TcData.ListFreeTexts.Count,"The refill drew the label once.");
					Assert.AreEqual(new PointF(101,61),wrapper.TcData.ListFreeTexts[0].Location);
					Rectangle boundsAfter=ToothSceneBuilder.GetFreeTextBoxes(wrapper.TcData)[0].Bounds;
					Assert.IsTrue(Math.Abs(boundsAfter.X-(box.Bounds.X+151))<=1 && Math.Abs(boundsAfter.Y-(box.Bounds.Y+92))<=1,
						"Drawn where it was dropped: "+box.Bounds+" to "+boundsAfter);
					using(Bitmap bitmap=relay.GetBitmap()) {
						Assert.AreEqual(ToothChartEngine.Wpf3D,wrapper.Engine);
					}
					//Then a delete (butDeleteText_Click): the next refill draws no label.
					ToothInitials.Delete(toothInitialSaved);
					actionRefill();
					Assert.AreEqual(0,wrapper.TcData.ListFreeTexts.Count);
				}
				finally {
					Thread.CurrentThread.CurrentCulture=cultureBefore;
					if(patNum!=0) {
						DataCore.NonQ("DELETE FROM toothinitial WHERE PatNum="+POut.Long(patNum));
					}
				}
			});
		}
		#endregion Free text round trip
	}
}
