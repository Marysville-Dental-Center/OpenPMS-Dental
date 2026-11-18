// Package TC-5's parity round (docs/specs/wpf-tooth-chart-plan.md, "Open Dental parity (2026-11-14)"; O-21 as revised): paint type TextBelow (18) is
// shown as Open Dental 26.2 shows it, in a one-line read-only box under the Chart module's date slider, not in the tooth picture. The strip
// (OpenPmsDental.ToothChartTextBelowStrip) on an off-screen form, filled from the Chart module's own procedure filter (GetChartModuleRows) on
// synthetic patients in the scratch database of the call-log tests (ChartTestDb), and its picture for gallery G2.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;

namespace ToothChartImageTests {
	[TestClass]
	public class TextBelowStripTests {
		///<summary>The Chart module's rows for the chart (ControlChart.FillToothChart: GetChartModuleRows of the progress notes at the slider's date).</summary>
		private static List<DataRow> GetChartModuleRows(long patNum,DateTime dateLimit) {
			DataTable tableProgNotes=CallLogFixtures.GetProgNotes(patNum);
			List<DataRow> listDataRowsProcsOrig=CallLogFixtures.GetProcRowsOrig(tableProgNotes);
			return ToothChartPainter.GetChartModuleRows(tableProgNotes.Copy(),null,listDataRowsProcsOrig,dateLimit,out List<DataRow> listDataRowsSkipped);
		}

		private static long InsertCode(string procCode,TreatmentArea treatmentArea,string paintText) {
			ProcedureCode procedureCode=ProcedureCodes.GetProcCode(procCode);
			if(procedureCode!=null && procedureCode.CodeNum!=0 && procedureCode.ProcCode==procCode) {
				return procedureCode.CodeNum;
			}
			long codeNum=OpenPmsDentalBusiness.Crud.ProcedureCodeCrud.Insert(new ProcedureCode { ProcCode=procCode,Descript="Synthetic TextBelow "+procCode,
				AbbrDesc="Syn"+procCode.Substring(1),ProcTime="/X/",PaintType=ToothPaintingType.TextBelow,TreatArea=treatmentArea,PaintText=paintText });
			Cache.Refresh(InvalidType.ProcCodes);
			return codeNum;
		}

		private static void InsertProc(long patNum,long codeNum,string toothNum,string surf,ProcStat procStat,DateTime dateProc,DateTime dateTP,
			bool isHidden=false)
		{
			long procNum=OpenPmsDentalBusiness.Crud.ProcedureCrud.Insert(new Procedure { PatNum=patNum,CodeNum=codeNum,ToothNum=toothNum,Surf=surf,
				ToothRange="",ProcStatus=procStat,ProvNum=CallLogFixtures.ProvNum1,ProcDate=dateProc,DateTP=dateTP,HideGraphics=isHidden });
			DateTime dateEntryC=(procStat==ProcStat.C) ? dateProc : DateTime.MinValue;
			DataCore.NonQ("UPDATE procedurelog SET DateEntryC="+POut.Date(dateEntryC)+" WHERE ProcNum="+POut.Long(procNum));
		}

		[TestMethod]
		public void TextBelowStrip_HiddenWhenNone_ShownForOne_JoinedInProcDateOrder_FollowsTheSlider() {
			WpfLiveViewTests.RequireDesktop();
			CallLogFixtures.EnsureBuilt();
			//Open Dental's own examples are a mouth and an arch procedure ("Occlusal Guard", clear aligners): the strip takes TextBelow of any
			//treatment area, past the painter's Mouth skip.
			long codeNumMouth=InsertCode("Z9201",TreatmentArea.Mouth,"Occlusal Guard");
			long codeNumArch=InsertCode("Z9202",TreatmentArea.Arch,"Aligners");
			long codeNumTooth=InsertCode("Z9203",TreatmentArea.Tooth,"Implant planned");
			long codeNumHidden=InsertCode("Z9204",TreatmentArea.Tooth,"Hidden one");
			long patNumNone=OpenPmsDentalBusiness.Crud.PatientCrud.Insert(new Patient { LName="Synthetic",FName="Strip none",PriProv=CallLogFixtures.ProvNum1,
				PatStatus=PatientStatus.Patient,Birthdate=new DateTime(1980,1,1) });
			long patNumOne=OpenPmsDentalBusiness.Crud.PatientCrud.Insert(new Patient { LName="Synthetic",FName="Strip one",PriProv=CallLogFixtures.ProvNum1,
				PatStatus=PatientStatus.Patient,Birthdate=new DateTime(1980,1,1) });
			long patNumMany=OpenPmsDentalBusiness.Crud.PatientCrud.Insert(new Patient { LName="Synthetic",FName="Strip many",PriProv=CallLogFixtures.ProvNum1,
				PatStatus=PatientStatus.Patient,Birthdate=new DateTime(1980,1,1) });
			try {
				//A patient with other procedures only: a filling and a crown.
				InsertProc(patNumNone,ProcedureCodes.GetProcCode("Z2007").CodeNum,"3","",ProcStat.C,new DateTime(2022,1,10),new DateTime(2022,1,10));
				InsertProc(patNumOne,codeNumMouth,"","",ProcStat.TP,new DateTime(2022,1,10),new DateTime(2022,1,10));
				//Entered out of date order: the strip sorts by ProcDate. The hidden one (Hide Graphics) is left out, as the painter leaves it out.
				InsertProc(patNumMany,codeNumMouth,"","",ProcStat.C,new DateTime(2022,3,1),new DateTime(2022,3,1));
				InsertProc(patNumMany,codeNumTooth,"8","",ProcStat.TP,new DateTime(2022,1,10),new DateTime(2022,1,10));
				InsertProc(patNumMany,codeNumArch,"","U",ProcStat.C,new DateTime(2022,2,1),new DateTime(2022,2,1));
				InsertProc(patNumMany,codeNumHidden,"9","",ProcStat.TP,new DateTime(2022,1,5),new DateTime(2022,1,5),isHidden:true);
				StaRunner.Run(() => {
					using Form form=WpfCharts.CreateOffscreenForm(new Size(410,60));
					ToothChartTextBelowStrip strip=new ToothChartTextBelowStrip();
					strip.Width=410;
					form.Controls.Add(strip);
					form.Show();
					Application.DoEvents();
					Assert.IsFalse(strip.Visible,"Hidden at first.");
					Assert.IsFalse(strip.SetProcedures(GetChartModuleRows(patNumNone,DateTime.Today)),"No TextBelow procedure: no change.");
					Assert.IsFalse(strip.Visible,"Hidden when the patient has no TextBelow procedure.");
					Assert.AreEqual("",strip.Text);
					Assert.IsTrue(strip.SetProcedures(GetChartModuleRows(patNumOne,DateTime.Today)),"Shown: the caller lays out again.");
					Assert.IsTrue(strip.Visible,"Shown for one, a Mouth procedure.");
					Assert.AreEqual("Occlusal Guard",strip.Text);
					Assert.IsTrue(strip.ReadOnly && !strip.Multiline && !strip.TabStop);
					//UQ-6 (UI-4c-2): the strip is restyled, its text the Text token on SurfaceRaised, the same on every engine.
					Assert.AreEqual(CodeBase.UiTheme.Text.ToArgb(),strip.ForeColor.ToArgb(),"Text-token text, the same on every engine.");
					Assert.AreEqual(CodeBase.UiTheme.SurfaceRaised.ToArgb(),strip.BackColor.ToArgb(),"On SurfaceRaised.");
					Assert.AreEqual(form.Font,strip.Font,"The standard text-box font, the form's.");
					Assert.IsFalse(strip.SetProcedures(GetChartModuleRows(patNumMany,DateTime.Today)),"Still shown: no layout change.");
					Assert.AreEqual("Implant planned; Aligners; Occlusal Guard",strip.Text,"Tooth, arch and mouth, in ProcDate order, joined by \"; \".");
					//The slider before the guard's dates drops it, as the chart drops its graphic.
					strip.SetProcedures(GetChartModuleRows(patNumMany,new DateTime(2022,2,15)));
					Assert.AreEqual("Implant planned; Aligners",strip.Text);
					//Before every date: nothing, so the strip hides.
					Assert.IsTrue(strip.SetProcedures(GetChartModuleRows(patNumMany,new DateTime(2021,12,31))));
					Assert.IsFalse(strip.Visible);
					Assert.IsTrue(strip.SetProcedures(GetChartModuleRows(patNumMany,DateTime.Today)));
					Assert.IsTrue(strip.Visible);
					Assert.IsTrue(strip.SetProcedures(null),"No patient: hidden.");
					Assert.IsFalse(strip.Visible);
					form.Close();
				});
			}
			finally {
				foreach(long patNum in new[] { patNumNone,patNumOne,patNumMany }) {
					DataCore.NonQ("DELETE FROM procedurelog WHERE PatNum="+POut.Long(patNum));
					DataCore.NonQ("DELETE FROM patient WHERE PatNum="+POut.Long(patNum));
				}
			}
		}

		[TestMethod]
		public void TextBelowStripText_JoinsByProcDate_SkipsHiddenAndOtherPaintTypes_NoDatabaseRowsNeeded() {
			CallLogFixtures.EnsureBuilt();
			InsertCode("Z9201",TreatmentArea.Mouth,"Occlusal Guard");
			InsertCode("Z9202",TreatmentArea.Arch,"Aligners");
			DataTable table=new DataTable();
			table.Columns.Add("ProcCode");
			table.Columns.Add("HideGraphics");
			table.Columns.Add("ProcDate",typeof(DateTime));
			table.Rows.Add("Z9201","0",new DateTime(2022,3,1));
			table.Rows.Add("Z2007","0",new DateTime(2022,1,1));//a crown: no strip text
			table.Rows.Add("Z9202","1",new DateTime(2022,1,2));//hidden
			table.Rows.Add("Z9202","0",new DateTime(2022,3,1));//same date as the guard: the list's order
			table.Rows.Add("Z2022","0",new DateTime(2021,12,1));//the fixtures' tooth code, paint text "Below"
			Assert.AreEqual("Below; Occlusal Guard; Aligners",ToothChartPainter.GetTextBelowStripText(table.Select().ToList()));
			Assert.AreEqual("",ToothChartPainter.GetTextBelowStripText(new List<DataRow>()));
			Assert.AreEqual("",ToothChartPainter.GetTextBelowStripText(null));
			Assert.AreEqual("; ",ToothChartPainter.TextBelowStripSeparator);
		}

		[TestMethod]
		public void TextBelowStrip_IsPlacedUnderTheDateSlider_InTheChartModule() {
			//ControlChart is not built by this test project (it never references the program), so its source is pinned: the strip in the designer,
			//filled in FillToothChart from the rows it paints, laid out under the slider after the sheet layout.
			string designer=SourceText.Read("OpenPmsDental/Main Modules/ControlChart.designer.cs");
			StringAssert.Contains(designer,"this.textTextBelow = new OpenPmsDental.ToothChartTextBelowStrip();");
			StringAssert.Contains(designer,"this.Controls.Add(this.textTextBelow);");
			string code=SourceText.ReadCode("OpenPmsDental/Main Modules/ControlChart.cs");
			StringAssert.Contains(code,"FillTextBelow(_listDataRowsProcsForGraphical);");
			StringAssert.Contains(code,"FillTextBelow(null);");
			Assert.IsTrue(code.IndexOf("FillTextBelow(_listDataRowsProcsForGraphical);")<code.IndexOf("DrawProcGraphics();\n\t\t\tDrawOrthoHardware();"),
				"Filled from the rows before the painter, which skips Mouth procedures.");
			StringAssert.Contains(code,"LayoutTextBelowAfterSheet(sheetFieldLayoutMode,dictionaryStringControl);");
			StringAssert.Contains(code,"LayoutManager.MoveLocation(textTextBelow,new Point(panelToothTrackBar.Left,panelToothTrackBar.Bottom+LayoutManager.Scale(1)));");
			//The sheet layout hides every control that is not a sheet field (SheetLayoutController.InitLayoutForSheetDef), the strip too, on each
			//LayoutControls(), and ModuleSelected lays out after it fills the chart: LayoutTextBelow shows the strip again after the move, only while
			//the layout places the slider (fix round 5; BSHELLTests.ChartTextBelowLayoutTests runs a real ControlChart through the hide pass).
			string layoutAfterSheet=SourceText.GetMethod(code,"private void LayoutTextBelowAfterSheet(");
			StringAssert.Contains(layoutAfterSheet,"dictionaryStringControl.TryGetValue(\"TrackToothProcDates\",out Control controlSlider)");
			StringAssert.Contains(layoutAfterSheet,"_sheetLayoutController.GetPertinentSheetFieldDefs(sheetFieldLayoutMode).Any(x => x.FieldName==\"TrackToothProcDates\")");
			string layoutTextBelow=SourceText.GetMethod(code,"private void LayoutTextBelow() {");
			SourceText.AssertInOrder(layoutTextBelow,"LayoutTextBelow","LayoutManager.MoveLocation(textTextBelow,","SetTextBelowVisible();");
			StringAssert.Contains(SourceText.GetMethod(code,"private void SetTextBelowVisible() {"),
				"textTextBelow.Visible=_isTextBelowSliderPlaced && textTextBelow.HasText;");
			StringAssert.Contains(SourceText.GetMethod(code,"private void FillTextBelow(List<DataRow> listDataRows) {"),"SetTextBelowVisible();");
			string layoutControls=SourceText.GetMethod(code,"public void LayoutControls() {");
			SourceText.AssertInOrder(layoutControls,"LayoutControls","_sheetLayoutController.ReloadSheetLayout(","LayoutTextBelowAfterSheet(");
		}

		[TestMethod]
		public void TextBelowStrip_GalleryPicture_ChartSliderAndStrip() {
			WpfLiveViewTests.RequireDesktop();
			string path=Path.Combine(Path.GetTempPath(),"ToothChartImageTests","tc5-text-below-strip.png");
			Size size=TextBelowStripPicture.Write(path);
			Assert.AreEqual(410,size.Width);
			Assert.IsTrue(File.Exists(path));
			using Bitmap bitmap=WpfImageTests.LoadBitmap(path);
			//The strip's text is black on white, below the chart and the slider.
			int countDark=0;
			for(int y=TextBelowStripPicture.TopStrip;y<bitmap.Height;y++) {
				for(int x=0;x<bitmap.Width;x++) {
					Color color=bitmap.GetPixel(x,y);
					if(color.R<80 && color.G<80 && color.B<80) {
						countDark++;
					}
				}
			}
			Assert.IsTrue(countDark>30,"The strip's text is drawn: "+countDark+" dark pixels.");
		}
	}

	///<summary>Gallery G2's picture of the TextBelow strip (TC-5's parity round): the WPF chart of scene text-all-kinds at the module size, a date
	///slider as the Chart module's (panelToothTrackBar: the date and the track bar), and the strip under it with that scene's TextBelow texts
	///("IMP" on 12, planned, and on 21, referred), drawn by an off-screen form. Needs an interactive desktop session.</summary>
	public static class TextBelowStripPicture {
		public const int HeightChart=307;
		public const int HeightSlider=27;
		///<summary>The strip's top in the picture: under the chart and the slider, one pixel down as ControlChart.LayoutTextBelow puts it.</summary>
		public const int TopStrip=HeightChart+HeightSlider+1;

		///<summary>Writes the picture to path (PNG) and returns its size.</summary>
		public static Size Write(string path) {
			Size sizePicture=Size.Empty;
			StaRunner.Run(() => {
				using Bitmap bitmapChart=GetChartPicture();
				using Form form=WpfCharts.CreateOffscreenForm(new Size(410,TopStrip+24));
				form.BackColor=Color.White;
				PictureBox pictureBox=new PictureBox { Location=new Point(0,0),Size=new Size(410,HeightChart),Image=bitmapChart };
				Panel panelSlider=new Panel { Location=new Point(0,HeightChart),Size=new Size(410,HeightSlider) };
				Label labelDate=new Label { Location=new Point(0,0),Size=new Size(76,27),Text=new DateTime(2026,11,14).ToShortDateString(),
					TextAlign=ContentAlignment.MiddleRight };
				TrackBar trackBar=new TrackBar { Location=new Point(76,0),Size=new Size(329,27),Minimum=0,Maximum=4,Value=4 };
				panelSlider.Controls.Add(labelDate);
				panelSlider.Controls.Add(trackBar);
				ToothChartTextBelowStrip strip=new ToothChartTextBelowStrip { Location=new Point(0,TopStrip),Width=410 };
				form.Controls.Add(pictureBox);
				form.Controls.Add(panelSlider);
				form.Controls.Add(strip);
				strip.Text="IMP; IMP";
				strip.Visible=true;
				form.Show();
				Application.DoEvents();
				form.ClientSize=new Size(410,strip.Bottom+1);
				Application.DoEvents();
				using Bitmap bitmap=new Bitmap(form.ClientSize.Width,form.ClientSize.Height);
				form.DrawToBitmap(bitmap,new Rectangle(Point.Empty,form.ClientSize));
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				PngWriter.Save(bitmap,path);
				sizePicture=bitmap.Size;
				pictureBox.Image=null;
				form.Close();
			});
			return sizePicture;
		}

		private static Bitmap GetChartPicture() {
			using SparksToothChart.ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
			ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
			return WpfCharts.Draw(relay,ChartScenes.Get("text-all-kinds"),ChartColorSet.Module);
		}
	}
}
