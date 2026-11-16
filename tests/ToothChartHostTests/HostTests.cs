// Each host of the tooth chart checked end to end through the program's own code (package TC-8, the TC-8 row's "Each host checked end to end";
// plan section 7, "Host tests"): the picture is made by the WPF chart (no fallback is logged, and it matches the baseline the WPF chart made), has
// the expected size, is opaque, has the pixel format TC-0 recorded for DirectX and is not blank. The local computer is the scratch database's row
// for this machine, set to a stored DirectX choice (0) unless a test says otherwise, and put back after each test. The hosts that only a shown
// window of the program can check (the Chart module's refill, selection, Draw tab and Save to Images, and the classic Treatment Plan document,
// whose module needs the main window) are launch check L-7 (docs/port-notes/TC-8.md).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;
using ToothChartImageTests;
using SheetPrinting=OpenPmsDental.SheetPrinting;

namespace ToothChartHostTests {
	[TestClass]
	public class HostTests {
		private DrawingMode _drawingModeBefore;
		private string _directXFormatBefore;
		private int _countFallbacksBefore;

		[TestInitialize]
		public void TestInitialize() {
			HostDb.EnsureOrSkip();
			HostDb.SetCulture();
			_drawingModeBefore=ComputerPrefs.LocalComputer.GraphicsSimple;
			_directXFormatBefore=ComputerPrefs.LocalComputer.DirectXFormat;
			SetLocalComputer(DrawingMode.DirectX,"");
			_countFallbacksBefore=ToothChartWrapper.GetCountFallbackMessages();
		}

		[TestCleanup]
		public void TestCleanup() {
			if(ComputerPrefs.IsLocalComputerNull()) {
				return;
			}
			SetLocalComputer(_drawingModeBefore,_directXFormatBefore);
		}

		///<summary>The local computer's stored chart choice, in memory, as the program reads it (ComputerPrefs.LocalComputer).</summary>
		public static void SetLocalComputer(DrawingMode drawingMode,string directXFormat) {
			ComputerPrefs.LocalComputer.GraphicsSimple=drawingMode;
			ComputerPrefs.LocalComputer.DirectXFormat=directXFormat;
		}

		///<summary>No fallback since the test began: the WPF chart started and drew every picture (a failed WPF render gives the 2D picture and a
		///message).</summary>
		private void AssertNothingFellBack() {
			List<string> listMessages=ToothChartWrapper.GetFallbackMessagesSince(_countFallbacksBefore);
			Assert.AreEqual(0,listMessages.Count,"Fallbacks: "+string.Join("; ",listMessages));
		}

		#region Sheets: SheetPrinting.GetToothChartHelper
		[TestMethod]
		public void Sheet_ToothChartField_IsTheWpfChartsPicture() {
			//A sheet's toothChart field (and the toothChartImg parameter of Treatment Plan sheets): 500x370 in the printout colours 14 and 15.
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false)) {
				HostPictures.Check("sheet",image,new Size(500,370));
			}
			AssertNothingFellBack();
			HostPictures.WriteDirectXPicture("sheet",new Size(500,370),wrapper => HostDirectX.PaintLikeGetToothChartHelper(wrapper,HostFixtures.PatNumWork,false,null,null,
				false,null,null));
		}

		[TestMethod]
		public void TreatPlanSheet_ToothChartImg_IsTheWpfChartsPicture() {
			//ControlTreat's sheet printout: GetToothChartHelper with the plan and "Show Completed" (ControlTreat.cs, toothChartImg).
			TreatPlan treatPlan=HostFixtures.GetTreatPlanOfTPs(HostFixtures.PatNumWork);
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,true,treatPlan)) {
				HostPictures.Check("treatplan-sheet",image,new Size(500,370));
			}
			AssertNothingFellBack();
			HostPictures.WriteDirectXPicture("treatplan-sheet",new Size(500,370),wrapper => HostDirectX.PaintLikeGetToothChartHelper(wrapper,HostFixtures.PatNumWork,true,
				treatPlan,null,false,null,null));
		}

		[TestMethod]
		public void ReferralLetterAndAutomationSheet_ToothChartImg_AreTheWpfChartsPicture() {
			//The referral letter (FormOpenDental: the day's completed procedures) and an automation's sheet (AutomationL: the procedures just completed)
			//both pass their procedures as the override list.
			List<Procedure> listProcedures=Procedures.GetCompletedForDateRange(HostFixtures.DateWork,HostFixtures.DateWork,
				listPatNums:new List<long>() { HostFixtures.PatNumWork },includeNote:true,includeGroupNote:true);
			Assert.IsTrue(listProcedures.Count>0,"The patient has completed work that day.");
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false,listProceduresFilteredOverride:listProcedures)) {
				HostPictures.Check("referral-letter",image,new Size(500,370));
			}
			AssertNothingFellBack();
			HostPictures.WriteDirectXPicture("referral-letter",new Size(500,370),wrapper => HostDirectX.PaintLikeGetToothChartHelper(wrapper,HostFixtures.PatNumWork,false,
				null,listProcedures,false,null,null));
		}

		[TestMethod]
		public void Sheet_OfAComputerStoredAsOpenGL_IsTheWpfChartsPicture() {
			//A stored OpenGL choice (2) starts the WPF chart too: the same picture as the stored DirectX one.
			SetLocalComputer(DrawingMode.OpenGL,"");
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false)) {
				HostPictures.Check("sheet",image,new Size(500,370));
			}
			AssertNothingFellBack();
		}

		[TestMethod]
		public void Sheet_OfAComputerStoredAsSimple2D_IsThe2DChartsPicture_O30() {
			//O-30: a computer stored as Simple 2D stays 2D, with no data change.
			SetLocalComputer(DrawingMode.Simple2D,"");
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false))
			using(Image imageWpf=GetSheetPictureAs(DrawingMode.DirectX)) {
				HostPictures.Check("sheet-simple2d",image,new Size(500,370),System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				Assert.IsFalse(ChartImageCompare.Compare((Bitmap)imageWpf,(Bitmap)image).IsPass,"The 2D chart's picture, not the WPF chart's.");
			}
			Assert.AreEqual(DrawingMode.Simple2D,ComputerPrefs.LocalComputer.GraphicsSimple,"Nothing changed the stored choice.");
			AssertNothingFellBack();
		}

		private static Image GetSheetPictureAs(DrawingMode drawingMode) {
			DrawingMode drawingModeBefore=ComputerPrefs.LocalComputer.GraphicsSimple;
			ComputerPrefs.LocalComputer.GraphicsSimple=drawingMode;
			try {
				return SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false);
			}
			finally {
				ComputerPrefs.LocalComputer.GraphicsSimple=drawingModeBefore;
			}
		}

		[TestMethod]
		public void Sheet_OfAComputerDrawnInSoftware_IsTheSamePicture() {
			//The "Draw in software" switch changes only the screen: every picture is the renderer's either way.
			SetLocalComputer(DrawingMode.DirectX,ToothChartWrapper.DirectXFormatWpfSoftware);
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false)) {
				HostPictures.Check("sheet",image,new Size(500,370));
			}
			AssertNothingFellBack();
		}
		#endregion Sheets: SheetPrinting.GetToothChartHelper

		#region The Patient Dashboard
		[TestMethod]
		public void Dashboard_OnAnMtaWorker_IsTheWpfChartsPicture_AndNothingIsLogged() {
			//UserControlDashboard refreshes its widgets on an MTA ODThread worker (DashToothChart.RefreshData, SheetPrinting.GetToothChartHelper with
			//isInPatientDashboard): 410x307 in the module colours 10 and 11. Today an exception there is swallowed, so the engine is checked too.
			Patient patient=HostFixtures.GetPatientWork();
			Image image=null;
			string engineOnWorker="";
			StaRunner.RunMta(() => {
				Assert.AreEqual(System.Threading.ApartmentState.MTA,System.Threading.Thread.CurrentThread.GetApartmentState());
				using(DashToothChart dashToothChart=new DashToothChart()) {
					dashToothChart.RefreshData(patient,null);
					Image imageWidget=(Image)typeof(DashToothChart).GetField("_imgToothChart",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(dashToothChart);
					image=(Image)imageWidget?.Clone();
				}
				//The relay's settings on the same kind of thread: the engine the dashboard's wrapper gets.
				using(Form form=new Form())
				using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
					wrapper.Size=new Size(410,307);
					ToothChartRelay.ApplyComputerSettings(wrapper,ToothChartSettingsUse.StoredMode);
					form.Controls.Add(wrapper);
					using(Bitmap bitmap=wrapper.GetBitmap()) {
					}
					engineOnWorker=wrapper.Engine.ToString();
				}
			});
			try {
				HostPictures.Check("dashboard",image,new Size(410,307));
			}
			finally {
				image?.Dispose();
			}
			Assert.AreEqual("Wpf3D",engineOnWorker,"The dashboard's wrapper on the MTA worker is still the WPF chart afterwards.");
			AssertNothingFellBack();
			TreatPlan treatPlan=TreatPlans.GetActiveForPat(patient.PatNum);
			HostPictures.WriteDirectXPicture("dashboard",new Size(410,307),wrapper => HostDirectX.PaintLikeGetToothChartHelper(wrapper,patient.PatNum,true,treatPlan,
				OpenPmsDentalBusiness.SheetPrinting.FilterProceduresForToothChart(null,treatPlan,true),true,patient,Appointments.GetTodaysApptsForPat(patient.PatNum)));
		}
		#endregion The Patient Dashboard

		#region The sheet editor's preview
		[TestMethod]
		public void SheetEditorPreview_IsTheWpfChartsPicture() {
			//FormSheetDefEdit.SetToothChartImage, the empty chart the sheet editor shows in a toothChart field: 500x370 in the printout colours. Called on
			//an object made without the window (its only state is the picture it keeps), so no window is made.
			FormSheetDefEdit formSheetDefEdit=(FormSheetDefEdit)FormatterServices.GetUninitializedObject(typeof(FormSheetDefEdit));
			GC.SuppressFinalize(formSheetDefEdit);//never made as a window, so it is never disposed as one
			Image image=null;
			StaRunner.Run(() => {
				typeof(FormSheetDefEdit).GetMethod("SetToothChartImage",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(formSheetDefEdit,null);
				image=(Image)typeof(FormSheetDefEdit).GetField("_imageToothChart",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(formSheetDefEdit);
			});
			try {
				HostPictures.Check("sheet-editor-preview",image,new Size(500,370));
			}
			finally {
				image?.Dispose();
			}
			AssertNothingFellBack();
			HostPictures.WriteDirectXPicture("sheet-editor-preview",new Size(500,370),wrapper => {
				ToothChartRelay toothChartRelay=new ToothChartRelay();
				toothChartRelay.SetToothChartWrapper(wrapper);
				toothChartRelay.SetToothNumberingNomenclature((ToothNumberingNomenclature)PrefC.GetInt(PrefName.UseInternationalToothNumbers));
				List<Def> listDefs=Defs.GetDefsForCategory(DefCat.ChartGraphicColors);
				toothChartRelay.ColorBackgroundMain=listDefs[14].ItemColor;
				toothChartRelay.ColorText=listDefs[15].ItemColor;
				toothChartRelay.ResetTeeth();
				toothChartRelay.EndUpdate();
				return toothChartRelay.GetBitmap();
			});
		}
		#endregion The sheet editor's preview

		#region The big chart window
		[TestMethod]
		public void BigChart_IsTheWpfChartsPicture() {
			//FormToothChartingBig (Show Big in the Chart module): its load applies the computer's settings and fills the chart from the lists the Chart
			//module hands it. The window is made but not shown; its chart's picture is the wrapper's GetBitmap, 926x858.
			Patient patient=HostFixtures.GetPatientWork();
			DataTable tableProgNotes=ChartModules.GetProgNotes(patient.PatNum,false,new ChartModuleFilters());
			List<DataRow> listDataRowsProcs=tableProgNotes.Copy().Select().Where(x => x["ProcNum"].ToString()!="0" && x["ProcNum"].ToString()!="").ToList();
			Bitmap bitmap=null;
			string engine="";
			ToothChartData toothChartData=null;
			StaRunner.Run(() => {
				using(FormToothChartingBig formToothChartingBig=new FormToothChartingBig(false)) {
					formToothChartingBig.PatientCur=patient;
					formToothChartingBig.ListToothInitials=ToothInitials.GetPatientData(patient.PatNum);
					formToothChartingBig.ListDataRowsProcs=listDataRowsProcs;
					formToothChartingBig.ListAppointments=new List<Appointment>();
					formToothChartingBig.ListOrthoHardwares=OrthoHardwares.GetPatientData(patient.PatNum);
					typeof(FormToothChartingBig).GetMethod("FormToothChartingBig_Load",BindingFlags.NonPublic|BindingFlags.Instance)
						.Invoke(formToothChartingBig,new object[] { formToothChartingBig,EventArgs.Empty });
					ToothChartWrapper wrapper=(ToothChartWrapper)typeof(FormToothChartingBig).GetField("toothChartWrapper",BindingFlags.NonPublic|BindingFlags.Instance)
						.GetValue(formToothChartingBig);
					engine=wrapper.Engine.ToString();
					bitmap=wrapper.GetBitmap();
					toothChartData=wrapper.TcData.Copy();
				}
			});
			try {
				Assert.AreEqual("Wpf3D",engine);
				HostPictures.Check("big-chart",bitmap,new Size(926,858));
			}
			finally {
				bitmap?.Dispose();
			}
			AssertNothingFellBack();
			HostPictures.WriteDirectXPicture("big-chart",new Size(926,858),wrapper => {
				wrapper.TcData=toothChartData;
				return wrapper.GetBitmap();
			});
		}
		#endregion The big chart window

		#region The graphical perio chart
		[TestMethod]
		public void GraphicalPerio_DisplayPrintAndSave_AreTheWpfChartsPictures() {
			CheckGraphicalPerio("perio");
		}

		[TestMethod]
		public void GraphicalPerio_OnA2DComputer_OpensOnTheWpfChart_O31() {
			//O-31: the graphical perio chart opens on the WPF chart on every computer; the 2D choice applies to the tooth chart only.
			SetLocalComputer(DrawingMode.Simple2D,"");
			Assert.IsTrue(ToothChartRelay.CanOpenGraphicalPerio(out string message),message);
			Assert.AreEqual("",message);
			CheckGraphicalPerio("perio");
			Assert.AreEqual(DrawingMode.Simple2D,ComputerPrefs.LocalComputer.GraphicsSimple,"Nothing changed the stored choice.");
		}

		///<summary>FormPerioGraphical, made but not shown: its LayoutTeeth (ApplyGraphicalPerioMode, then the exam) as its Load runs it; the chart's own
		///picture (649x696, what the window shows and prints); and the 750x1000 save (butSave_Click's bitmap, RenderPerioPrintout with the title, the
		///clinic or practice, the patient, the birth date, the exam date and the chart), which the printout draws the same way.</summary>
		private void CheckGraphicalPerio(string name) {
			Patient patient=HostFixtures.GetPatientWork();
			PerioExam perioExam=PerioExams.GetOnePerioExam(HostFixtures.PerioExamNum);
			Bitmap bitmapChart=null;
			Bitmap bitmapSave=null;
			string engine="";
			bool isPerioMode=false;
			ToothChartData toothChartData=null;
			StaRunner.Run(() => {
				using(FormPerioGraphical formPerioGraphical=new FormPerioGraphical(perioExam,patient)) {
					typeof(FormPerioGraphical).GetMethod("LayoutTeeth",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(formPerioGraphical,null);
					ToothChartWrapper wrapper=(ToothChartWrapper)typeof(FormPerioGraphical).GetField("toothChartWrapper",BindingFlags.NonPublic|BindingFlags.Instance)
						.GetValue(formPerioGraphical);
					engine=wrapper.Engine.ToString();
					isPerioMode=wrapper.PerioMode;
					bitmapChart=wrapper.GetBitmap();
					toothChartData=wrapper.TcData.Copy();
					Bitmap bitmap=new Bitmap(750,1000);
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(Color.White);
						g.CompositingQuality=System.Drawing.Drawing2D.CompositingQuality.HighQuality;
						g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.HighQuality;
						formPerioGraphical.RenderPerioPrintout(g,patient,new Rectangle(0,0,bitmap.Width,bitmap.Height));
					}
					bitmapSave=new Bitmap(750,1000,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
					using(Graphics g=Graphics.FromImage(bitmapSave)) {
						g.DrawImageUnscaled(bitmap,0,0);
					}
					bitmap.Dispose();
					wrapper.TcData.ListToothGraphics.Clear();//as FormPerioGraphical_FormClosing does
				}
			});
			try {
				Assert.AreEqual("Wpf3D",engine,"The graphical perio chart draws on the WPF chart.");
				Assert.IsTrue(isPerioMode);
				HostPictures.Check(name+"-chart",bitmapChart,new Size(649,696));
				HostPictures.Check(name+"-save",bitmapSave,new Size(750,1000));
			}
			finally {
				bitmapChart?.Dispose();
				bitmapSave?.Dispose();
			}
			AssertNothingFellBack();
			HostPictures.WriteDirectXPicture(name+"-chart",new Size(649,696),wrapper => {
				wrapper.TcData=toothChartData;
				return wrapper.GetBitmap();
			});
		}
		#endregion The graphical perio chart

		#region The settings snapshot
		[TestMethod]
		public void SettingsSnapshot_SeesAnEngineOrSoftwareChange_NotAChangeBetweenZeroAndTwo() {
			//FormOpenDental redraws the Chart module after a refresh only when the snapshot changed (NeedsRedraw("ChartModule")). Since TC-8 it holds the
			//engine of the stored mode and the "Draw in software" switch in place of the DirectX and OpenGL fields.
			ToothChartSettingsSnapshot snapshot=new ToothChartSettingsSnapshot();
			Assert.IsTrue(snapshot.HasChanged(),"Nothing taken yet.");
			snapshot.Take();
			Assert.IsFalse(snapshot.HasChanged());
			SetLocalComputer(DrawingMode.OpenGL,"");
			Assert.IsFalse(snapshot.HasChanged(),"0 and 2 start the same chart.");
			ComputerPrefs.LocalComputer.PreferredPixelFormatNum=ComputerPrefs.LocalComputer.PreferredPixelFormatNum+1;
			ComputerPrefs.LocalComputer.GraphicsUseHardware=!ComputerPrefs.LocalComputer.GraphicsUseHardware;
			Assert.IsFalse(snapshot.HasChanged(),"The OpenGL fields are not read any more.");
			SetLocalComputer(DrawingMode.Simple2D,"");
			Assert.IsTrue(snapshot.HasChanged(),"The 2D chart.");
			snapshot.Take();
			SetLocalComputer(DrawingMode.Simple2D,ToothChartWrapper.DirectXFormatWpfSoftware);
			Assert.IsTrue(snapshot.HasChanged(),"The software switch.");
			snapshot.Take();
			SetLocalComputer(DrawingMode.Simple2D,"0;Hardware;64;D16;R5G6B5;FourSamples");
			Assert.IsTrue(snapshot.HasChanged(),"Off again (a DirectX format an older build saved is off).");
			snapshot.Take();
			SetLocalComputer(DrawingMode.Simple2D,"");
			Assert.IsFalse(snapshot.HasChanged(),"Still off.");
		}
		#endregion The settings snapshot
	}

	///<summary>For gallery G4 only: today's DirectX picture of the same host, painted on a DirectX wrapper with the program's own painter calls of
	///SheetPrinting.GetToothChartHelper (the host's wrapper is made and dropped inside the program), so only the engine differs.</summary>
	public static class HostDirectX {
		public static Bitmap PaintLikeGetToothChartHelper(ToothChartWrapper toothChartWrapper,long patNum,bool showCompleted,TreatPlan treatPlan,
			List<Procedure> listProceduresFilteredOverride,bool isInPatientDashboard,Patient patCur,List<Appointment> listAppts)
		{
			ToothChartRelay toothChartRelay=new ToothChartRelay(false);
			toothChartRelay.SetToothChartWrapper(toothChartWrapper);
			toothChartRelay.SetToothNumberingNomenclature((ToothNumberingNomenclature)PrefC.GetInt(PrefName.UseInternationalToothNumbers));
			List<Def> listDefs=Defs.GetDefsForCategory(DefCat.ChartGraphicColors);
			toothChartRelay.ColorBackgroundMain=listDefs[isInPatientDashboard ? 10 : 14].ItemColor;
			toothChartRelay.ColorText=listDefs[isInPatientDashboard ? 11 : 15].ItemColor;
			toothChartRelay.ResetTeeth();
			toothChartRelay.SetOrthoMode(false);
			List<ToothInitial> toothInitialList=patNum==0 ? new List<ToothInitial>() : ToothInitials.GetPatientData(patNum);
			ToothChartPainter.PaintPrimaryTeeth(toothChartRelay,toothInitialList);
			ToothChartPainter.PaintToothInitials(toothChartRelay,toothInitialList);
			List<Procedure> listProceduresFiltered=new List<Procedure>();
			if(listProceduresFilteredOverride!=null) {
				listProceduresFiltered=listProceduresFilteredOverride;
			}
			else if(patNum>0) {
				listProceduresFiltered=OpenPmsDentalBusiness.SheetPrinting.FilterProceduresForToothChart(Procedures.Refresh(patNum),treatPlan,showCompleted);
			}
			listProceduresFiltered.Sort(OpenPmsDentalBusiness.SheetPrinting.CompareProcListFiltered);
			ToothChartPainter.PaintProcedures(toothChartRelay,ToothChartPaintProc.FromProcedures(listProceduresFiltered),toothInitialList,
				ToothChartPaintOptions.ForSheet(isInPatientDashboard,patCur,listAppts));
			ToothChartPainter.PaintOrthoHardware(toothChartRelay,OrthoHardwares.GetPatientData(patNum));
			toothChartRelay.EndUpdate();
			return toothChartRelay.GetBitmap();
		}
	}
}
