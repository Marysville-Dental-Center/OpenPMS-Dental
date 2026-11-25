// Each host of the tooth chart checked end to end through the program's own code (package TC-8, the TC-8 row's "Each host checked end to end";
// plan section 7, "Host tests"): the picture is made by the WPF chart (no fallback is logged, and it matches the baseline the WPF chart made), has
// the expected size, is opaque, has the pixel format TC-0 recorded for DirectX and is not blank. The local computer is the scratch database's row
// for this machine, set to a new computer's settings (GraphicsSimple 0, "Use the global setting", the default on-screen picture) under the global
// "Use if available" unless a test says otherwise, and put back after each test (TC-8b: Open Dental's 3D settings choose the chart). The hosts that only a shown
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
		private YN _chart3DThisComputerBefore;
		private int _countFallbacksBefore;

		[TestInitialize]
		public void TestInitialize() {
			HostDb.EnsureOrSkip();
			HostDb.SetCulture();
			_drawingModeBefore=ComputerPrefs.LocalComputer.GraphicsSimple;
			_directXFormatBefore=ComputerPrefs.LocalComputer.DirectXFormat;
			_chart3DThisComputerBefore=ComputerPrefs.LocalComputer.GraphicsUseDirectX11;
			SetLocalComputer(DrawingMode.DirectX,"");
			ToothChartRelay.SetChart3DOnAllComputers(true);
			_countFallbacksBefore=ToothChartWrapper.GetCountFallbackMessages();
		}

		[TestCleanup]
		public void TestCleanup() {
			if(ComputerPrefs.IsLocalComputerNull()) {
				return;
			}
			SetLocalComputer(_drawingModeBefore,_directXFormatBefore,_chart3DThisComputerBefore);
			ToothChartRelay.SetChart3DOnAllComputers(true);//the new database's value
		}

		///<summary>The local computer's stored chart settings, in memory, as the program reads them (ComputerPrefs.LocalComputer): GraphicsSimple, the
		///"Draw in software" value and, since TC-8b, its 3D tooth chart setting (Unknown: "Use the global setting").</summary>
		public static void SetLocalComputer(DrawingMode drawingMode,string directXFormat,YN chart3DThisComputer=YN.Unknown) {
			ComputerPrefs.LocalComputer.GraphicsSimple=drawingMode;
			ComputerPrefs.LocalComputer.DirectXFormat=directXFormat;
			ComputerPrefs.LocalComputer.GraphicsUseDirectX11=chart3DThisComputer;
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
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			//A sheet's toothChart field (and the toothChartImg parameter of Treatment Plan sheets): 500x370 in the printout colours 14 and 15.
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false)) {
				HostPictures.Check("sheet",image,new Size(500,370));
			}
			AssertNothingFellBack();
		}

		[TestMethod]
		public void TreatPlanSheet_ToothChartImg_IsTheWpfChartsPicture() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			//ControlTreat's sheet printout: GetToothChartHelper with the plan and "Show Completed" (ControlTreat.cs, toothChartImg).
			TreatPlan treatPlan=HostFixtures.GetTreatPlanOfTPs(HostFixtures.PatNumWork);
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,true,treatPlan)) {
				HostPictures.Check("treatplan-sheet",image,new Size(500,370));
			}
			AssertNothingFellBack();
		}

		[TestMethod]
		public void ReferralLetterAndAutomationSheet_ToothChartImg_AreTheWpfChartsPicture() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			//The referral letter (FormOpenDental: the day's completed procedures) and an automation's sheet (AutomationL: the procedures just completed)
			//both pass their procedures as the override list.
			List<Procedure> listProcedures=Procedures.GetCompletedForDateRange(HostFixtures.DateWork,HostFixtures.DateWork,
				listPatNums:new List<long>() { HostFixtures.PatNumWork },includeNote:true,includeGroupNote:true);
			Assert.IsTrue(listProcedures.Count>0,"The patient has completed work that day.");
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false,listProceduresFilteredOverride:listProcedures)) {
				HostPictures.Check("referral-letter",image,new Size(500,370));
			}
			AssertNothingFellBack();
		}

		[TestMethod]
		public void Sheet_OfAComputerStoredAsOpenGL_IsTheWpfChartsPicture() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			//A stored OpenGL choice (2) starts the WPF chart too: the same picture as the stored DirectX one.
			SetLocalComputer(DrawingMode.OpenGL,"");
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false)) {
				HostPictures.Check("sheet",image,new Size(500,370));
			}
			AssertNothingFellBack();
		}

		[TestMethod]
		public void Sheet_OfAComputerStoredAsSimple2D_IsTheWpfChartsPicture_UnlessItsSettingIsDoNotUse_ENG2() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			//ENG-2 (revises O-30): a computer stored as Simple 2D (1) gets the 3D chart unless its setting is "Do not use", with no data change.
			SetLocalComputer(DrawingMode.Simple2D,"");
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false)) {
				HostPictures.Check("sheet",image,new Size(500,370));
			}
			Assert.AreEqual(DrawingMode.Simple2D,ComputerPrefs.LocalComputer.GraphicsSimple,"Nothing changed the stored choice.");
			Assert.AreEqual(YN.Unknown,ComputerPrefs.LocalComputer.GraphicsUseDirectX11,"Nothing changed the stored choice.");
			AssertNothingFellBack();
		}

		[TestMethod]
		public void Sheet_OfAComputerThatDoesNotUseThe3DChart_IsThe2DChartsPicture() {
			//"Do not use" on this computer, or "Use the global setting" with the global "Do not use": Open Dental's older technology, of which Simple 2D is
			//left, whatever GraphicsSimple holds. The 2D picture is the one TC-8 recorded for a computer stored as Simple 2D (the same chart and data).
			foreach(DrawingMode drawingMode in new[] { DrawingMode.DirectX,DrawingMode.Simple2D,DrawingMode.OpenGL }) {
				foreach(bool isGlobalDoNotUse in new[] { false,true }) {
					ToothChartRelay.SetChart3DOnAllComputers(!isGlobalDoNotUse);
					SetLocalComputer(drawingMode,"",isGlobalDoNotUse ? YN.Unknown : YN.No);
					using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false))
					using(Image imageWpf=GetSheetPictureOfThe3DChart()) {
						HostPictures.Check("sheet-simple2d",image,new Size(500,370),System.Drawing.Imaging.PixelFormat.Format32bppArgb);
						Assert.IsFalse(ChartImageCompare.Compare((Bitmap)imageWpf,(Bitmap)image).IsPass,"The 2D chart's picture, not the WPF chart's.");
					}
					Assert.AreEqual(drawingMode,ComputerPrefs.LocalComputer.GraphicsSimple,"Nothing changed the stored choice.");
				}
			}
			//REL-0.1-FIX: the 2D pictures above are checked in any session; the WPF chart's picture below needs one where WPF draws.
			WpfSession.RequireRendering("The 2D pictures were checked.");
			//"Use if available" on this computer wins over the global "Do not use".
			ToothChartRelay.SetChart3DOnAllComputers(false);
			SetLocalComputer(DrawingMode.Simple2D,"",YN.Yes);
			using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false)) {
				HostPictures.Check("sheet",image,new Size(500,370));
			}
			AssertNothingFellBack();
		}

		///<summary>The sheet picture with this computer set to "Use if available", the WPF chart's, then the settings as they were.</summary>
		private static Image GetSheetPictureOfThe3DChart() {
			YN chart3DThisComputerBefore=ComputerPrefs.LocalComputer.GraphicsUseDirectX11;
			ComputerPrefs.LocalComputer.GraphicsUseDirectX11=YN.Yes;
			try {
				return SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false);
			}
			finally {
				ComputerPrefs.LocalComputer.GraphicsUseDirectX11=chart3DThisComputerBefore;
			}
		}

		[TestMethod]
		public void Sheet_OfAComputerWithTheQuickLiveView_IsTheSamePicture() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			//The "Draw in software" switch changes only the screen (ENG-3: ticked, the smoothed picture, is the default; unticked the quick live view):
			//every picture is the renderer's either way.
			foreach(string directXFormat in new[] { ToothChartWrapper.DirectXFormatWpfLiveView,ToothChartWrapper.DirectXFormatWpfSoftware }) {
				SetLocalComputer(DrawingMode.DirectX,directXFormat);
				using(Image image=SheetPrinting.GetToothChartHelper(HostFixtures.PatNumWork,false)) {
					HostPictures.Check("sheet",image,new Size(500,370));
				}
			}
			AssertNothingFellBack();
		}
		#endregion Sheets: SheetPrinting.GetToothChartHelper

		#region The Patient Dashboard
		[TestMethod]
		public void Dashboard_OnAnMtaWorker_IsTheWpfChartsPicture_AndNothingIsLogged() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
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
					ToothChartRelay.ApplyComputerSettings(wrapper);
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
		}
		#endregion The Patient Dashboard

		#region The sheet editor's preview
		[TestMethod]
		public void SheetEditorPreview_IsTheWpfChartsPicture() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
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
		}
		#endregion The sheet editor's preview

		#region The big chart window
		[TestMethod]
		public void BigChart_IsTheWpfChartsPicture() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			//FormToothChartingBig (Show Big in the Chart module): its load applies the computer's settings and fills the chart from the lists the Chart
			//module hands it. The window is made but not shown; its chart's picture is the wrapper's GetBitmap, 926x858.
			Patient patient=HostFixtures.GetPatientWork();
			DataTable tableProgNotes=ChartModules.GetProgNotes(patient.PatNum,false,new ChartModuleFilters());
			List<DataRow> listDataRowsProcs=tableProgNotes.Copy().Select().Where(x => x["ProcNum"].ToString()!="0" && x["ProcNum"].ToString()!="").ToList();
			Bitmap bitmap=null;
			string engine="";
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
		}
		#endregion The big chart window

		#region The graphical perio chart
		[TestMethod]
		public void GraphicalPerio_DisplayPrintAndSave_AreTheWpfChartsPictures() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			CheckGraphicalPerio("perio");
		}

		[TestMethod]
		public void GraphicalPerio_OnA2DComputer_IsRefused_WithAMessageNamingFileGraphics_ENG5() {
			//ENG-5 (revises O-31): on a computer that draws Simple 2D ("Do not use" here, or the global "Do not use") the gate refuses, as Open Dental
			//opens its graphical perio chart only on its 3D charts.
			foreach(bool isGlobalDoNotUse in new[] { false,true }) {
				ToothChartRelay.SetChart3DOnAllComputers(!isGlobalDoNotUse);
				SetLocalComputer(DrawingMode.Simple2D,"",isGlobalDoNotUse ? YN.Unknown : YN.No);
				Assert.IsFalse(ToothChartRelay.CanOpenGraphicalPerio(out string message));
				Assert.AreEqual("In the Graphics setup window (File > Graphics), you must first select the 3D tooth chart.",message);
			}
		}

		[TestMethod]
		public void GraphicalPerio_OnAComputerStoredAsSimple2D_ThatUsesThe3DChart_Opens_ENG2() {
			WpfSession.RequireRendering();//REL-0.1-FIX: inconclusive where WPF draws nothing (a disconnected session)
			//ENG-2: a computer stored as Simple 2D with "Use the global setting" uses the 3D chart, so its graphical perio chart opens, with the same
			//pictures.
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
			StaRunner.Run(() => {
				using(FormPerioGraphical formPerioGraphical=new FormPerioGraphical(perioExam,patient)) {
					typeof(FormPerioGraphical).GetMethod("LayoutTeeth",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(formPerioGraphical,null);
					ToothChartWrapper wrapper=(ToothChartWrapper)typeof(FormPerioGraphical).GetField("toothChartWrapper",BindingFlags.NonPublic|BindingFlags.Instance)
						.GetValue(formPerioGraphical);
					engine=wrapper.Engine.ToString();
					isPerioMode=wrapper.PerioMode;
					bitmapChart=wrapper.GetBitmap();
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
		}
		#endregion The graphical perio chart

		#region The settings snapshot
		[TestMethod]
		public void SettingsSnapshot_SeesAnEngineOrSoftwareChange_NotAChangeThatKeepsTheChart() {
			//FormOpenDental redraws the Chart module after a refresh only when the snapshot changed (NeedsRedraw("ChartModule")). It holds the engine the
			//computer draws with (TC-8b: the global 3D setting, this computer's and GraphicsSimple) and the "Draw in software" switch in place of the
			//DirectX and OpenGL fields.
			ToothChartSettingsSnapshot snapshot=new ToothChartSettingsSnapshot();
			Assert.IsTrue(snapshot.HasChanged(),"Nothing taken yet.");
			snapshot.Take();
			Assert.IsFalse(snapshot.HasChanged());
			SetLocalComputer(DrawingMode.OpenGL,"");
			Assert.IsFalse(snapshot.HasChanged(),"0 and 2 start the same chart.");
			SetLocalComputer(DrawingMode.Simple2D,"");
			Assert.IsFalse(snapshot.HasChanged(),"A stored 1 under \"Use the global setting\" is the 3D chart too (ENG-2).");
			ComputerPrefs.LocalComputer.PreferredPixelFormatNum=ComputerPrefs.LocalComputer.PreferredPixelFormatNum+1;
			ComputerPrefs.LocalComputer.GraphicsUseHardware=!ComputerPrefs.LocalComputer.GraphicsUseHardware;
			Assert.IsFalse(snapshot.HasChanged(),"The OpenGL fields are not read any more.");
			SetLocalComputer(DrawingMode.Simple2D,"",YN.No);
			Assert.IsTrue(snapshot.HasChanged(),"\"Do not use\": the 2D chart.");
			snapshot.Take();
			SetLocalComputer(DrawingMode.DirectX,"",YN.No);
			Assert.IsFalse(snapshot.HasChanged(),"Still the 2D chart.");
			SetLocalComputer(DrawingMode.DirectX,"",YN.Unknown);
			Assert.IsTrue(snapshot.HasChanged(),"The global setting, \"Use if available\": the 3D chart.");
			snapshot.Take();
			ToothChartRelay.SetChart3DOnAllComputers(false);
			Assert.IsTrue(snapshot.HasChanged(),"The global setting \"Do not use\": the 2D chart.");
			snapshot.Take();
			SetLocalComputer(DrawingMode.DirectX,"",YN.No);
			Assert.IsFalse(snapshot.HasChanged(),"\"Do not use\" under the global \"Do not use\": the same chart.");
			ToothChartRelay.SetChart3DOnAllComputers(true);
			snapshot.Take();
			SetLocalComputer(DrawingMode.DirectX,ToothChartWrapper.DirectXFormatWpfLiveView);
			Assert.IsTrue(snapshot.HasChanged(),"The 3D chart again, with the quick live view.");
			snapshot.Take();
			SetLocalComputer(DrawingMode.DirectX,"0;Hardware;64;D16;R5G6B5;FourSamples");
			Assert.IsTrue(snapshot.HasChanged(),"The default again (a DirectX format an older build saved is the default, the smoothed picture).");
			snapshot.Take();
			SetLocalComputer(DrawingMode.DirectX,ToothChartWrapper.DirectXFormatWpfSoftware);
			Assert.IsFalse(snapshot.HasChanged(),"TC-8's value is the default too.");
		}
		#endregion The settings snapshot
	}
}
