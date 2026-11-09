// The Patient Dashboard's wait for the Chart module's picture made in the background (package TC-3b, docs/specs/wpf-tooth-chart-plan.md, section 2's
// TC-3b notes), with real pictures of a hidden WPF chart: the dashboard is handed the newest request's picture for the current patient, GetBitmap's
// picture of that refill; a patient change, a refresh, a newer picture or a closed dashboard while the picture is being made drops it, so the dashboard
// never shows another patient's chart; an event with neither picture nor request leaves the wait. UserControlDashboard's wiring and the tooth chart
// field are tested in BSHELLTests (DashboardToothChartImageTests), which references the program.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	[TestClass]
	public class ToothChartDashboardWaitTests {
		#region Helpers
		///<summary>Each synthetic patient's chart: patient 1, 2 and 3 have different scenes.</summary>
		private static string GetSceneName(long patNum) {
			return patNum==1?"restored-adult":(patNum==2?"treatment-plan":"missing-many");
		}

		///<summary>The Chart module's chart as the WPF chart draws it, hidden but with a window handle, as the request needs.</summary>
		private sealed class Chart:IDisposable {
			public readonly ToothChartWrapper Wrapper;
			public readonly ToothChartRelay Relay;

			public Chart() {
				Wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
				Assert.AreEqual(ToothChartEngine.Wpf3D,Wrapper.Engine,string.Join("; ",ToothChartWrapper.GetFallbackMessages()));
				Wrapper.Visible=false;
				IntPtr intPtrHandle=Wrapper.Handle;
				Relay=WpfCharts.CreateRelay(Wrapper);
			}

			///<summary>A refill of the patient's chart and the request ControlChart.FillToothChart makes with the dashboard visible.</summary>
			public ToothChartBitmapRequest FillAndRequest(long patNum) {
				ChartScenes.Prepare(Relay,ChartScenes.Get(GetSceneName(patNum)),ChartColorSet.Module);
				ToothChartBitmapRequest toothChartBitmapRequest=Relay.RequestBitmapInBackground();
				Assert.IsNotNull(toothChartBitmapRequest,"The WPF chart makes the dashboard's picture in the background.");
				return toothChartBitmapRequest;
			}

			public void Dispose() {
				Wrapper.Dispose();
			}
		}

		///<summary>What the dashboard was handed (the wait's actionShow): the patient and the picture.</summary>
		private sealed class ShownPictures {
			public readonly List<long> ListPatNums=new List<long>();
			public readonly List<Bitmap> ListBitmaps=new List<Bitmap>();

			public void Show(long patNum,Bitmap bitmap) {
				ListPatNums.Add(patNum);
				ListBitmaps.Add(bitmap);
			}

			public void Clear() {
				ListBitmaps.ForEach(x => x.Dispose());
				ListBitmaps.Clear();
				ListPatNums.Clear();
			}
		}

		private static bool Pump(Func<bool> funcCondition) {
			return WpfRendererTests.PumpUntil(funcCondition,20000);
		}

		///<summary>Lets anything still posted run.</summary>
		private static void PumpRest() {
			WpfRendererTests.PumpUntil(() => false,300);
		}
		#endregion Helpers

		[TestMethod]
		public void Dashboard_IsHandedTheNewestRefillsPicture_GetBitmapsPicture() {
			StaRunner.Run(() => {
				using Chart chart=new Chart();
				long patNumCur=1;
				ShownPictures shownPictures=new ShownPictures();
				ToothChartDashboardWait toothChartDashboardWait=new ToothChartDashboardWait(() => true,() => patNumCur,shownPictures.Show);
				//Two refills of patient 1's chart, one after the other: only the newest request's picture is shown.
				ToothChartBitmapRequest toothChartBitmapRequestFirst=chart.FillAndRequest(1);
				toothChartDashboardWait.Wait(1,toothChartBitmapRequestFirst,false);
				ChartScenes.Prepare(chart.Relay,ChartScenes.Get(GetSceneName(1)),ChartColorSet.Module);
				chart.Relay.SetSelected("8",true);//the newest refill differs from the first
				using Bitmap bitmapExpected=chart.Relay.GetBitmap();
				ToothChartBitmapRequest toothChartBitmapRequestNewest=chart.Relay.RequestBitmapInBackground();
				toothChartDashboardWait.Wait(1,toothChartBitmapRequestNewest,false);
				Assert.AreSame(toothChartBitmapRequestNewest,toothChartDashboardWait.RequestWaitedFor);
				Assert.IsTrue(Pump(() => toothChartBitmapRequestNewest.IsDone),"The picture arrived.");
				PumpRest();
				Assert.IsTrue(toothChartBitmapRequestFirst.IsDropped,"The older request was dropped by the newer one.");
				CollectionAssert.AreEqual(new long[] { 1 },shownPictures.ListPatNums,"One picture shown, patient 1's.");
				Assert.IsNull(toothChartDashboardWait.RequestWaitedFor,"The wait has ended.");
				Assert.IsTrue(ChartImageCompare.GetPixelsArgb(bitmapExpected).SequenceEqual(ChartImageCompare.GetPixelsArgb(shownPictures.ListBitmaps[0])),
					"The newest refill's picture, GetBitmap's.");
				shownPictures.Clear();
			});
		}

		[TestMethod]
		public void PatientChange_WhileThePictureIsBeingMade_NeverShowsTheOtherPatientsChart() {
			StaRunner.Run(() => {
				using Chart chart=new Chart();
				long patNumCur=1;
				ShownPictures shownPictures=new ShownPictures();
				ToothChartDashboardWait toothChartDashboardWait=new ToothChartDashboardWait(() => true,() => patNumCur,shownPictures.Show);
				//1. In the Chart module: patient 1's picture is being made when the user selects patient 2. The Patient event refreshes the dashboard (the
				//wait ends), then the Chart module refills for patient 2 and asks again.
				ToothChartBitmapRequest toothChartBitmapRequest1;
				ToothChartBitmapRequest toothChartBitmapRequest2;
				using(RenderJobHold renderJobHold=new RenderJobHold(ToothChartRenderer.JobRender)) {
					toothChartBitmapRequest1=chart.FillAndRequest(1);
					toothChartDashboardWait.Wait(1,toothChartBitmapRequest1,false);
					Assert.IsTrue(Pump(() => renderJobHold.WaitEntered(0)),"Patient 1's picture is being made.");
					patNumCur=2;
					toothChartDashboardWait.Clear();
					toothChartBitmapRequest2=chart.FillAndRequest(2);
					toothChartDashboardWait.Wait(2,toothChartBitmapRequest2,false);
					renderJobHold.Release();
				}
				using(Bitmap bitmapPatient2=chart.Relay.GetBitmap()) {
					Assert.IsTrue(Pump(() => toothChartBitmapRequest2.IsDone));
					PumpRest();
					Assert.IsTrue(toothChartBitmapRequest1.IsDropped,"Patient 1's picture was dropped.");
					CollectionAssert.AreEqual(new long[] { 2 },shownPictures.ListPatNums,"Only patient 2's picture was shown.");
					Assert.IsTrue(ChartImageCompare.GetPixelsArgb(bitmapPatient2).SequenceEqual(ChartImageCompare.GetPixelsArgb(shownPictures.ListBitmaps[0])),
						"Patient 2's chart.");
				}
				shownPictures.Clear();
				//2. Patient 2's picture is being made when the user selects patient 3 in another module: no new request, only the dashboard's refresh.
				ToothChartBitmapRequest toothChartBitmapRequest3;
				using(RenderJobHold renderJobHold=new RenderJobHold(ToothChartRenderer.JobRender)) {
					toothChartBitmapRequest3=chart.FillAndRequest(2);
					toothChartDashboardWait.Wait(2,toothChartBitmapRequest3,false);
					Assert.IsTrue(Pump(() => renderJobHold.WaitEntered(0)));
					patNumCur=3;
					toothChartDashboardWait.Clear();
					renderJobHold.Release();
				}
				Assert.IsTrue(Pump(() => toothChartBitmapRequest3.IsDone),"The chart handed its picture on.");
				PumpRest();
				Assert.AreEqual(0,shownPictures.ListPatNums.Count,"The dashboard did not show it: patient 3 is current.");
				//3. The same without the refresh (a Patient event the dashboard missed): the patient check alone drops it.
				patNumCur=2;
				ToothChartBitmapRequest toothChartBitmapRequest4;
				using(RenderJobHold renderJobHold=new RenderJobHold(ToothChartRenderer.JobRender)) {
					toothChartBitmapRequest4=chart.FillAndRequest(2);
					toothChartDashboardWait.Wait(2,toothChartBitmapRequest4,false);
					Assert.IsTrue(Pump(() => renderJobHold.WaitEntered(0)));
					patNumCur=3;
					renderJobHold.Release();
				}
				Assert.IsTrue(Pump(() => toothChartBitmapRequest4.IsDone));
				PumpRest();
				Assert.AreEqual(0,shownPictures.ListPatNums.Count,"Not shown while another patient is current.");
				//4. Back on patient 2 before the picture arrives (patient 2, 3, 2 quickly, each with its refresh and refill): only the last refill's
				//picture is shown, for patient 2.
				ToothChartBitmapRequest toothChartBitmapRequestLast;
				using(RenderJobHold renderJobHold=new RenderJobHold(ToothChartRenderer.JobRender)) {
					patNumCur=2;
					toothChartDashboardWait.Clear();
					ToothChartBitmapRequest toothChartBitmapRequestA=chart.FillAndRequest(2);
					toothChartDashboardWait.Wait(2,toothChartBitmapRequestA,false);
					Assert.IsTrue(Pump(() => renderJobHold.WaitEntered(0)));
					patNumCur=3;
					toothChartDashboardWait.Clear();
					toothChartDashboardWait.Wait(3,chart.FillAndRequest(3),false);
					WpfRendererTests.PumpUntil(() => false,50);
					patNumCur=2;
					toothChartDashboardWait.Clear();
					toothChartBitmapRequestLast=chart.FillAndRequest(2);
					toothChartDashboardWait.Wait(2,toothChartBitmapRequestLast,false);
					renderJobHold.Release();
				}
				using(Bitmap bitmapLast=chart.Relay.GetBitmap()) {
					Assert.IsTrue(Pump(() => toothChartBitmapRequestLast.IsDone));
					PumpRest();
					CollectionAssert.AreEqual(new long[] { 2 },shownPictures.ListPatNums,"Patient 2's last refill only.");
					Assert.IsTrue(ChartImageCompare.GetPixelsArgb(bitmapLast).SequenceEqual(ChartImageCompare.GetPixelsArgb(shownPictures.ListBitmaps[0])));
				}
				shownPictures.Clear();
			});
		}

		[TestMethod]
		public void NewerChartOrClosedDashboard_EndsTheWait_AndAnEventWithNeitherLeavesIt() {
			StaRunner.Run(() => {
				using Chart chart=new Chart();
				long patNumCur=1;
				bool isOpen=true;
				ShownPictures shownPictures=new ShownPictures();
				ToothChartDashboardWait toothChartDashboardWait=new ToothChartDashboardWait(() => isOpen,() => patNumCur,shownPictures.Show);
				//An event that brings a picture (the other engines) is newer: the picture still being made is not shown.
				ToothChartBitmapRequest toothChartBitmapRequest=chart.FillAndRequest(1);
				toothChartDashboardWait.Wait(1,toothChartBitmapRequest,false);
				toothChartDashboardWait.Wait(1,null,true);
				Assert.IsNull(toothChartDashboardWait.RequestWaitedFor);
				Assert.IsTrue(Pump(() => toothChartBitmapRequest.IsDone));
				PumpRest();
				Assert.AreEqual(0,shownPictures.ListPatNums.Count,"A newer picture came with an event.");
				//An event with neither (another module, or a refill dated before today) leaves the wait, as it leaves the dashboard's chart.
				toothChartBitmapRequest=chart.FillAndRequest(1);
				toothChartDashboardWait.Wait(1,toothChartBitmapRequest,false);
				toothChartDashboardWait.Wait(1,null,false);
				Assert.AreSame(toothChartBitmapRequest,toothChartDashboardWait.RequestWaitedFor);
				Assert.IsTrue(Pump(() => toothChartBitmapRequest.IsDone));
				PumpRest();
				CollectionAssert.AreEqual(new long[] { 1 },shownPictures.ListPatNums,"Shown after an event with neither.");
				//A refresh (the dashboard makes its own chart) ends the wait.
				toothChartBitmapRequest=chart.FillAndRequest(1);
				toothChartDashboardWait.Wait(1,toothChartBitmapRequest,false);
				toothChartDashboardWait.Clear();
				Assert.IsTrue(Pump(() => toothChartBitmapRequest.IsDone));
				PumpRest();
				Assert.AreEqual(1,shownPictures.ListPatNums.Count,"Not shown after a refresh.");
				//A dashboard closed while the picture is being made shows nothing.
				toothChartBitmapRequest=chart.FillAndRequest(1);
				toothChartDashboardWait.Wait(1,toothChartBitmapRequest,false);
				isOpen=false;
				Assert.IsTrue(Pump(() => toothChartBitmapRequest.IsDone));
				PumpRest();
				Assert.AreEqual(1,shownPictures.ListPatNums.Count,"Not shown on a closed dashboard.");
				shownPictures.Clear();
			});
		}
	}
}
