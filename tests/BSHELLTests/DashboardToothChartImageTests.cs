// The Patient Dashboard's side of the Chart module's picture made in the background (package TC-3b of the WPF tooth chart track,
// docs/specs/wpf-tooth-chart-plan.md, section 2's TC-3b notes): the dashboard's tooth chart field draws the picture that arrives later exactly as it
// drew the picture the event brought, the refresh thread's step sets it only for the current patient, and UserControlDashboard is wired to the wait
// (ToothChartDashboardWait, whose decisions ToothChartImageTests tests with real pictures). No window is shown and no database is used.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;

namespace BSHELLTests {
	[TestClass]
	public class DashboardToothChartImageTests {
		#region Helpers
		///<summary>Runs the action on a new STA thread, as WinForms controls need, and throws what it threw on the test thread.</summary>
		private static void RunSta(Action action) {
			ExceptionDispatchInfo exceptionDispatchInfo=null;
			Thread thread=new Thread(() => {
				try {
					action();
				}
				catch(Exception ex) {
					exceptionDispatchInfo=ExceptionDispatchInfo.Capture(ex);
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			thread.Join();
			exceptionDispatchInfo?.Throw();
		}

		///<summary>A synthetic chart picture of the size the Chart module's chart has at 100% (410x307), in the format of the WPF chart's pictures.</summary>
		private static Bitmap CreatePicture(Color colorTeeth) {
			Bitmap bitmap=new Bitmap(410,307,PixelFormat.Format24bppRgb);
			using(Graphics g=Graphics.FromImage(bitmap))
			using(LinearGradientBrush brush=new LinearGradientBrush(new Rectangle(0,0,410,307),Color.FromArgb(59,90,134),Color.Black,30f))
			using(SolidBrush brushTeeth=new SolidBrush(colorTeeth)) {
				g.FillRectangle(brush,0,0,410,307);
				for(int i=0;i<16;i++) {
					g.FillEllipse(brushTeeth,10+i*24,60,20,40);
					g.FillEllipse(brushTeeth,10+i*24,200,20,40);
				}
			}
			return bitmap;
		}

		///<summary>A widget's controls as UserControlDashboardWidget holds them: one tooth chart field of a dashboard sheet's size.</summary>
		private static DashToothChart AddField(Control controlWidget) {
			DashToothChart dashToothChart=new DashToothChart();
			dashToothChart.Size=new Size(205,154);
			controlWidget.Controls.Add(dashToothChart);
			return dashToothChart;
		}

		private static byte[] GetPixels(Bitmap bitmap) {
			using Bitmap bitmap32=new Bitmap(bitmap.Width,bitmap.Height,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmap32)) {
				g.DrawImageUnscaled(bitmap,0,0);
			}
			BitmapData bitmapData=bitmap32.LockBits(new Rectangle(0,0,bitmap32.Width,bitmap32.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
			try {
				byte[] arrayBytes=new byte[bitmapData.Stride*bitmap32.Height];
				Marshal.Copy(bitmapData.Scan0,arrayBytes,0,arrayBytes.Length);
				return arrayBytes;
			}
			finally {
				bitmap32.UnlockBits(bitmapData);
			}
		}
		#endregion Helpers

		[TestMethod]
		public void PictureFromTheBackground_IsDrawnAsTheEventsPictureWas() {
			RunSta(() => {
				using Bitmap bitmapPicture=CreatePicture(Color.White);
				//The other engines: the picture comes with the ModuleSelected event (SetData).
				using Panel panelEvent=new Panel();
				DashToothChart dashToothChartEvent=AddField(panelEvent);
				dashToothChartEvent.SetData(new PatientDashboardDataEventArgs { Pat=new Patient { PatNum=1 },ImageToothChart=bitmapPicture },null);
				dashToothChartEvent.RefreshView();
				//The WPF chart: the picture arrives later and the dashboard's refresh thread sets it (SetImageInWidget).
				using Panel panelBackground=new Panel();
				DashToothChart dashToothChartBackground=AddField(panelBackground);
				DashToothChart dashToothChartSecond=AddField(panelBackground);//a sheet may hold two tooth chart fields: both get it
				Assert.IsTrue(DashToothChart.SetImageInWidget(panelBackground,1,1,bitmapPicture),"The fields took the picture.");
				dashToothChartBackground.RefreshView();
				dashToothChartSecond.RefreshView();
				Assert.IsNotNull(dashToothChartEvent.Image);
				byte[] arrayPixelsEvent=GetPixels((Bitmap)dashToothChartEvent.Image);
				Assert.IsTrue(arrayPixelsEvent.SequenceEqual(GetPixels((Bitmap)dashToothChartBackground.Image)),"The dashboard looks the same.");
				Assert.IsTrue(arrayPixelsEvent.SequenceEqual(GetPixels((Bitmap)dashToothChartSecond.Image)));
				//A widget without a tooth chart field takes nothing (the refresh thread still redraws it, as after every set).
				using Panel panelNoField=new Panel();
				panelNoField.Controls.Add(new Label());
				Assert.IsFalse(DashToothChart.SetImageInWidget(panelNoField,1,1,bitmapPicture));
			});
		}

		[TestMethod]
		public void RefreshThreadsStep_SetsThePictureOnlyForTheCurrentPatient() {
			RunSta(() => {
				using Panel panelWidget=new Panel();
				DashToothChart dashToothChart=AddField(panelWidget);
				using Bitmap bitmapPatient3=CreatePicture(Color.Gray);
				using Bitmap bitmapPatient2=CreatePicture(Color.Red);
				//The field shows patient 3's chart (the refresh after the patient change made it).
				Assert.IsTrue(DashToothChart.SetImageInWidget(panelWidget,3,3,bitmapPatient3));
				dashToothChart.RefreshView();
				byte[] arrayPixelsPatient3=GetPixels((Bitmap)dashToothChart.Image);
				//Patient 2's picture arrived for patient 2, but patient 3 became current before the refresh thread got to it: the field is left alone.
				Assert.IsFalse(DashToothChart.SetImageInWidget(panelWidget,2,3,bitmapPatient2),"Not set while another patient is current.");
				dashToothChart.RefreshView();
				Assert.IsTrue(arrayPixelsPatient3.SequenceEqual(GetPixels((Bitmap)dashToothChart.Image)),"The field still shows patient 3's chart.");
				Assert.IsFalse(DashToothChart.SetImageInWidget(panelWidget,3,3,null),"No picture, nothing set.");
			});
		}

		[TestMethod]
		public void SetImageToothChart_IgnoresNull_AndKeepsItsOwnCopy() {
			RunSta(() => {
				using DashToothChart dashToothChart=new DashToothChart();
				dashToothChart.Size=new Size(41,31);
				dashToothChart.SetImageToothChart(null);
				dashToothChart.RefreshView();
				Assert.IsNull(dashToothChart.Image,"No picture yet: nothing drawn.");
				Bitmap bitmap=new Bitmap(410,307);
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.Red);
				}
				dashToothChart.SetImageToothChart(bitmap);
				bitmap.Dispose();//the dashboard disposes its bitmap when its refresh thread ends; the field keeps its own copy
				dashToothChart.RefreshView();
				Assert.IsNotNull(dashToothChart.Image);
				Assert.AreEqual(Color.Red.ToArgb(),((Bitmap)dashToothChart.Image).GetPixel(20,15).ToArgb());
			});
		}

		[TestMethod]
		public void UserControlDashboard_WaitsForThePicture_AndEndsTheWaitOnEveryRefreshAndClose() {
			string source=TestPaths.ReadRepoFile("OpenPmsDental","User Controls","Dashboard","UserControlDashboard.cs");
			StringAssert.Contains(source,
				"_toothChartDashboardWait=new ToothChartDashboardWait(() => IsInitialized && !IsDisposed,() => FormOpenDental.PatNumCur,ShowToothChartImage);",
				"Open while initialized, and the main window's current patient.");
			//Every ModuleSelected event with a patient goes to the wait first, before the copy that leaves the request out.
			string setData=TestPaths.GetMethodBody(source,"private void SetData(ODEventArgs e)");
			int indexWait=setData.IndexOf("_toothChartDashboardWait.Wait(dataEvent.Pat.PatNum,dataEvent.ImageToothChartRequest,dataEvent.ImageToothChart!=null);",
				StringComparison.Ordinal);
			int indexCopy=setData.IndexOf("new PatientDashboardDataEventArgs(e.Tag);",StringComparison.Ordinal);
			Assert.IsTrue(indexWait>0 && indexWait<indexCopy,"SetData waits before it copies the event.");
			StringAssert.Contains(setData,"if(dataEvent!=null && dataEvent.Pat!=null) {","An event without a patient is ignored, as DashToothChart.SetData ignores it.");
			//Every refresh (Initialize, RefreshDashboard: the Patient events, the menus, the widget's refresh) ends the wait before its thread starts.
			string refresh=TestPaths.GetMethodBody(source,"private void StartRefreshThread() {");
			int indexClear=refresh.IndexOf("_toothChartDashboardWait.Clear();",StringComparison.Ordinal);
			Assert.IsTrue(indexClear>0 && indexClear<refresh.IndexOf("REFRESH_THREAD_NAME",StringComparison.Ordinal));
			StringAssert.Contains(TestPaths.GetMethodBody(source,"public void RefreshDashboard()"),"StartRefreshThread();");
			StringAssert.Contains(TestPaths.GetMethodBody(source,"private void PatientChangedEvent_Fired(ODEventArgs e)"),"RefreshDashboard();");
			Assert.AreEqual(1,TestPaths.Count(source,"StartRefreshThread((widget) => widget.TryRefreshData(),REFRESH_THREAD_NAME);"),"One refresh path.");
			//Closing ends it.
			StringAssert.Contains(TestPaths.GetMethodBody(source,"private void CloseWidget(UserControlDashboardWidget widget,EventArgs e)"),
				"_toothChartDashboardWait.Clear();");
			StringAssert.Contains(TestPaths.GetMethodBody(source,"public void CloseDashboard(bool isLoggingOff)"),"_toothChartDashboardWait.Clear();");
			//The picture is set on the dashboard's own serial refresh thread, the patient checked again there, the widget redrawn as after every set, and
			//the bitmap disposed when that thread ends.
			string show=TestPaths.GetMethodBody(source,"private void ShowToothChartImage(long patNum,Bitmap bitmap)");
			StringAssert.Contains(show,"StartRefreshThread((widget) => {");
			StringAssert.Contains(show,"DashToothChart.SetImageInWidget(widget,patNum,FormOpenDental.PatNumCur,bitmap);");
			StringAssert.Contains(show,"return widget.SheetDefWidget!=null;");
			StringAssert.Contains(show,"bitmap.Dispose()");
			//The refresh threads run one after another: each waits for the one before it.
			StringAssert.Contains(TestPaths.GetMethodBody(source,"private void StartRefreshThread(Func<UserControlDashboardWidget,bool> funcData,string name,"),
				"previousThread.QuitSync(Timeout.Infinite);");
			//The dashboard's own picture (DashToothChart.RefreshData on its MTA worker) still comes from the sheet helper, which calls GetBitmap.
			string dash=TestPaths.ReadRepoFile("OpenPmsDental","User Controls","Dashboard","DashToothChart.cs");
			StringAssert.Contains(TestPaths.GetMethodBody(dash,"public void RefreshData(Patient pat,SheetField sheetField)"),
				"_imgToothChart=GetToothChart(pat,treatPlan,listAppts:listAppts);");
			string sheetPrinting=TestPaths.ReadRepoFile("OpenPmsDental","SheetFramework","SheetPrinting.cs");
			string helper=TestPaths.GetMethodBody(sheetPrinting,"public static Image GetToothChartHelper(");
			StringAssert.Contains(helper,"Image retVal=toothChartRelay.GetBitmap();");
			Assert.IsFalse(helper.Contains("RequestBitmapInBackground"),"The sheet helper's hidden chart has no handle and waits for its picture.");
		}
	}
}
