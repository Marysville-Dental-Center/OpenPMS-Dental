// Tests of the dashboard's picture made in the background (package TC-3b, docs/specs/wpf-tooth-chart-plan.md, section 2's TC-3b notes):
// ToothChartWrapper.RequestBitmapInBackground returns without waiting for the render, its picture is GetBitmap's picture of the same data pixel for
// pixel, a burst makes one picture and only the newest request is handed on, the other engines and wrappers that cannot take a picture back give no
// request, a disposed wrapper or one leaving the engine drops its request, a lost handle asks again, a failed render gives the 2D picture, the caller's
// thread gets no WPF object, and on a shown chart the screen's update comes first.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	[TestClass]
	public class WpfBackgroundPictureTests {
		///<summary>A WPF wrapper that is hidden but has a handle, as the Chart module's chart has one: a request can post to it, and no rebuild on screen
		///(and so no live view) is made.</summary>
		private static ToothChartWrapper CreateHiddenWrapperWithHandle(Size size) {
			ToothChartWrapper wrapper=WpfCharts.CreateWrapper(size);
			wrapper.Visible=false;
			IntPtr intPtrHandle=wrapper.Handle;
			Assert.IsTrue(wrapper.IsHandleCreated);
			return wrapper;
		}

		///<summary>Asks for the picture and collects what reaches its action.</summary>
		private static ToothChartBitmapRequest Request(ToothChartRelay relay,List<Bitmap> listBitmaps) {
			ToothChartBitmapRequest toothChartBitmapRequest=relay.RequestBitmapInBackground();
			Assert.IsNotNull(toothChartBitmapRequest,"The WPF chart makes the picture in the background.");
			toothChartBitmapRequest.SetActionDone(bitmap => listBitmaps.Add(bitmap));
			return toothChartBitmapRequest;
		}

		private static void AssertSamePixels(Bitmap bitmapExpected,Bitmap bitmapActual,string message) {
			Assert.AreEqual(bitmapExpected.Size,bitmapActual.Size,message+": size");
			Assert.AreEqual(PixelFormat.Format24bppRgb,bitmapActual.PixelFormat,message+": GetBitmap's pixel format");
			Assert.IsTrue(ChartImageCompare.GetPixelsArgb(bitmapExpected).SequenceEqual(ChartImageCompare.GetPixelsArgb(bitmapActual)),message);
		}

		[TestMethod]
		public void Request_ReturnsWithoutWaitingForTheRender_AndItsPictureIsGetBitmaps() {
			int countMessages=ToothChartWrapper.GetCountFallbackMessages();
			int countUnhandled=ToothChartRenderer.CountUnhandledExceptions;
			StaRunner.Run(() => {
				foreach(Size size in new[] { ChartScenes.SizeModule.Size,WpfTimings.Size150 }) {
					using ToothChartWrapper wrapper=CreateHiddenWrapperWithHandle(size);
					ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
					ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
					using Bitmap bitmapExpected=relay.GetBitmap();
					List<Bitmap> listBitmaps=new List<Bitmap>();
					ToothChartBitmapRequest toothChartBitmapRequest;
					//The render thread is held in a job of its own: a request that waited for its picture would not return until the hold ends.
					using(RenderJobHold renderJobHold=new RenderJobHold(ToothChartRenderer.JobRender)) {
						ToothChartRenderer.Instance.RenderAsync(WpfCharts.BuildScene(wrapper),null,result => { });
						Assert.IsTrue(renderJobHold.WaitEntered(20000),"The render thread is held.");
						Stopwatch stopwatch=Stopwatch.StartNew();
						toothChartBitmapRequest=Request(relay,listBitmaps);
						long millisecondsRequest=stopwatch.ElapsedMilliseconds;
						Assert.IsTrue(millisecondsRequest<2000,"The request returned while the render thread was held, in "+millisecondsRequest+" ms.");
						Assert.IsFalse(wrapper.IsWpfSceneDirty,"The request built the scene of the data, as GetBitmap does.");
						Assert.AreEqual(size,toothChartBitmapRequest.Size);
						WpfRendererTests.PumpUntil(() => false,300);
						Assert.IsFalse(toothChartBitmapRequest.IsDone,"No picture while the render thread is held.");
						Assert.AreEqual(0,listBitmaps.Count);
						renderJobHold.Release();
					}
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartBitmapRequest.IsDone,20000),"The picture arrived.");
					Assert.AreEqual(1,listBitmaps.Count,"Handed on once.");
					using Bitmap bitmap=listBitmaps[0];
					AssertSamePixels(bitmapExpected,bitmap,size+": the picture made in the background is GetBitmap's, pixel for pixel");
					Assert.IsFalse(toothChartBitmapRequest.IsDropped);
				}
			});
			Assert.AreEqual(0,ToothChartWrapper.GetFallbackMessagesSince(countMessages).Count,"Nothing was logged: "
				+string.Join("; ",ToothChartWrapper.GetFallbackMessagesSince(countMessages)));
			Assert.AreEqual(countUnhandled,ToothChartRenderer.CountUnhandledExceptions);
		}

		[TestMethod]
		public void Picture_IsTheSceneOfItsRequest_EvenWhenTheDataChangesBeforeItIsMade() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=CreateHiddenWrapperWithHandle(ChartScenes.SizeModule.Size);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("treatment-plan"),ChartColorSet.Module);
				using Bitmap bitmapExpected=relay.GetBitmap();
				List<Bitmap> listBitmaps=new List<Bitmap>();
				ToothChartBitmapRequest toothChartBitmapRequest=Request(relay,listBitmaps);
				//A selection after the request, before the picture is made (the job starts only after this message): not in the picture.
				relay.SetSelected("8",true);
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartBitmapRequest.IsDone,20000));
				using Bitmap bitmap=listBitmaps.Single();
				AssertSamePixels(bitmapExpected,bitmap,"The picture of the data at the request, as GetBitmap at that moment");
			});
		}

		[TestMethod]
		public void Burst_MakesOnePicture_AndOnlyTheNewestRequestIsHandedOn() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=CreateHiddenWrapperWithHandle(ChartScenes.SizeModule.Size);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				string[] arraySceneNames={ "restored-adult","treatment-plan","missing-many","primary-dentition","mixed-dentition" };
				//A burst in one UI message (refills one after another): one job, which takes the newest scene.
				int countMade=wrapper.CountBitmapsMadeInBackground;
				List<List<Bitmap>> listListBitmaps=new List<List<Bitmap>>();
				List<ToothChartBitmapRequest> listRequests=new List<ToothChartBitmapRequest>();
				for(int i=0;i<arraySceneNames.Length;i++) {
					ChartScenes.Prepare(relay,ChartScenes.Get(arraySceneNames[i]),ChartColorSet.Module);
					listListBitmaps.Add(new List<Bitmap>());
					listRequests.Add(Request(relay,listListBitmaps[i]));
				}
				using Bitmap bitmapExpected=relay.GetBitmap();
				ToothChartBitmapRequest toothChartBitmapRequestLast=listRequests[listRequests.Count-1];
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartBitmapRequestLast.IsDone,20000));
				WpfRendererTests.PumpUntil(() => false,300);
				Assert.AreEqual(countMade+1,wrapper.CountBitmapsMadeInBackground,"One picture for the burst.");
				for(int i=0;i<listRequests.Count-1;i++) {
					Assert.IsTrue(listRequests[i].IsDropped && !listRequests[i].IsDone,"Request "+i+" was dropped by a newer one.");
					Assert.AreEqual(0,listListBitmaps[i].Count,"A dropped request's action is never called.");
				}
				using(Bitmap bitmap=listListBitmaps[listRequests.Count-1].Single()) {
					AssertSamePixels(bitmapExpected,bitmap,"The newest request's picture");
				}
				//A burst while a picture is being made: that picture is dropped and the newest is made after it (two renders, one picture).
				countMade=wrapper.CountBitmapsMadeInBackground;
				List<Bitmap> listBitmapsFirst=new List<Bitmap>();
				List<Bitmap> listBitmapsLast=new List<Bitmap>();
				ToothChartBitmapRequest toothChartBitmapRequestFirst;
				ToothChartBitmapRequest toothChartBitmapRequestNewest=null;
				using(RenderJobHold renderJobHold=new RenderJobHold(ToothChartRenderer.JobRender)) {
					ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
					toothChartBitmapRequestFirst=Request(relay,listBitmapsFirst);
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => renderJobHold.WaitEntered(0),20000),"The first picture's render started.");
					for(int i=0;i<3;i++) {
						ChartScenes.Prepare(relay,ChartScenes.Get(arraySceneNames[i+1]),ChartColorSet.Module);
						toothChartBitmapRequestNewest=Request(relay,i==2?listBitmapsLast:new List<Bitmap>());
						WpfRendererTests.PumpUntil(() => false,50);//each its own UI message
					}
					renderJobHold.Release();
				}
				using Bitmap bitmapNewest=relay.GetBitmap();
				ToothChartBitmapRequest toothChartBitmapRequestWaited=toothChartBitmapRequestNewest;
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartBitmapRequestWaited.IsDone,20000));
				WpfRendererTests.PumpUntil(() => false,300);
				Assert.AreEqual(countMade+2,wrapper.CountBitmapsMadeInBackground,"The running render and one for the newest request.");
				Assert.IsTrue(toothChartBitmapRequestFirst.IsDropped,"The picture that was being made was dropped.");
				Assert.AreEqual(0,listBitmapsFirst.Count);
				using(Bitmap bitmap=listBitmapsLast.Single()) {
					AssertSamePixels(bitmapNewest,bitmap,"The newest request's picture, made after the running one");
				}
			});
		}

		[TestMethod]
		public void OtherEnginesAndWrappersThatCannotTakeThePictureBack_GiveNoRequest() {
			StaRunner.Run(() => {
				//Simple 2D keeps GetBitmap (as DirectX and OpenGL do, below when they start here).
				using(ToothChartWrapper wrapper2D=new ToothChartWrapper()) {
					wrapper2D.Size=ChartScenes.SizeModule.Size;
					wrapper2D.Visible=false;
					IntPtr intPtrHandle=wrapper2D.Handle;
					Assert.AreEqual(ToothChartEngine.Simple2D,wrapper2D.Engine);
					Assert.IsNull(wrapper2D.RequestBitmapInBackground(),"Simple 2D gives no request.");
				}
				//A WPF wrapper with no handle (the hidden forms of prints, sheets and the dashboard's own chart): nothing could hand the picture back.
				using(ToothChartWrapper wrapperNoHandle=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size)) {
					Assert.IsFalse(wrapperNoHandle.IsHandleCreated);
					Assert.IsNull(wrapperNoHandle.RequestBitmapInBackground());
				}
				//A call off the wrapper's thread (the dashboard's MTA worker, for instance).
				using(ToothChartWrapper wrapper=CreateHiddenWrapperWithHandle(ChartScenes.SizeModule.Size)) {
					ToothChartBitmapRequest toothChartBitmapRequestMta=StaRunner.RunMta(() => wrapper.RequestBitmapInBackground());
					Assert.IsNull(toothChartBitmapRequestMta,"Off the wrapper's thread.");
					Assert.IsNotNull(wrapper.RequestBitmapInBackground(),"On its thread.");
				}
				//An empty chart.
				using(ToothChartWrapper wrapperEmpty=CreateHiddenWrapperWithHandle(new Size(0,0))) {
					Assert.IsNull(wrapperEmpty.RequestBitmapInBackground());
				}
			});
			if(!Environment.UserInteractive) {
				return;//DirectX needs a shown window
			}
			StaRunner.Run(() => {
				using Form form=WpfCharts.CreateOffscreenForm(ChartScenes.SizeModule.Size);
				form.Show();
				Application.DoEvents();
				string directXFormat=DirectXCharts.FindFormat(form);
				if(directXFormat==null) {
					Console.WriteLine("DirectX does not start here; Simple 2D was checked.");
					return;
				}
				ToothChartWrapper wrapperDirectX=DirectXCharts.CreateWrapper(form,ChartScenes.SizeModule.Size,directXFormat);
				try {
					if(wrapperDirectX.Engine==ToothChartEngine.DirectX) {
						Assert.IsNull(wrapperDirectX.RequestBitmapInBackground(),"DirectX keeps its GetBitmap until TC-8.");
					}
				}
				finally {
					form.Controls.Remove(wrapperDirectX);
					wrapperDirectX.Dispose();
					form.Close();
				}
			});
		}

		[TestMethod]
		public void DisposedWrapperOrAnotherEngine_DropsTheRequest_AndCallsNothing() {
			int countMessages=ToothChartWrapper.GetCountFallbackMessages();
			int countUnhandled=ToothChartRenderer.CountUnhandledExceptions;
			StaRunner.Run(() => {
				//Disposed before the job started.
				ToothChartWrapper wrapper=CreateHiddenWrapperWithHandle(ChartScenes.SizeModule.Size);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
				List<Bitmap> listBitmaps=new List<Bitmap>();
				ToothChartBitmapRequest toothChartBitmapRequest=Request(relay,listBitmaps);
				wrapper.Dispose();
				Assert.IsTrue(toothChartBitmapRequest.IsDropped);
				WpfRendererTests.PumpUntil(() => false,300);
				ToothChartRenderer.Instance.WaitIdle();
				WpfRendererTests.PumpUntil(() => false,300);
				Assert.AreEqual(0,listBitmaps.Count,"No call after the wrapper went.");
				Assert.IsFalse(toothChartBitmapRequest.IsDone);
				//Disposed while its render runs.
				wrapper=CreateHiddenWrapperWithHandle(ChartScenes.SizeModule.Size);
				relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("treatment-plan"),ChartColorSet.Module);
				using(RenderJobHold renderJobHold=new RenderJobHold(ToothChartRenderer.JobRender)) {
					toothChartBitmapRequest=Request(relay,listBitmaps);
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => renderJobHold.WaitEntered(0),20000));
					wrapper.Dispose();
					renderJobHold.Release();
				}
				ToothChartRenderer.Instance.WaitIdle();
				WpfRendererTests.PumpUntil(() => false,300);
				Assert.IsTrue(toothChartBitmapRequest.IsDropped);
				Assert.AreEqual(0,listBitmaps.Count,"No call for a render that finished after the wrapper went.");
				//The wrapper leaves the WPF engine.
				using(ToothChartWrapper wrapperEngine=CreateHiddenWrapperWithHandle(ChartScenes.SizeModule.Size)) {
					relay=WpfCharts.CreateRelay(wrapperEngine);
					ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
					toothChartBitmapRequest=Request(relay,listBitmaps);
					wrapperEngine.Engine=ToothChartEngine.Simple2D;
					Assert.IsTrue(toothChartBitmapRequest.IsDropped);
					WpfRendererTests.PumpUntil(() => false,300);
					ToothChartRenderer.Instance.WaitIdle();
					WpfRendererTests.PumpUntil(() => false,300);
					Assert.AreEqual(0,listBitmaps.Count);
				}
			});
			Assert.AreEqual(0,ToothChartWrapper.GetFallbackMessagesSince(countMessages).Count,"Nothing was logged: "
				+string.Join("; ",ToothChartWrapper.GetFallbackMessagesSince(countMessages)));
			Assert.AreEqual(countUnhandled,ToothChartRenderer.CountUnhandledExceptions,"Nothing was thrown on the render thread.");
		}

		[TestMethod]
		public void HandleThatGoes_GivesTheJobUp_AndTheNewHandleAsksAgain() {
			StaRunner.Run(() => {
				TestWrapper wrapper=new TestWrapper();
				try {
					wrapper.Size=ChartScenes.SizeModule.Size;
					wrapper.Engine=ToothChartEngine.Wpf3D;
					wrapper.Visible=false;
					IntPtr intPtrHandle=wrapper.Handle;
					ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
					//The handle goes before the posted start runs (DestroyHandle drops it).
					ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
					using(Bitmap bitmapExpected=relay.GetBitmap()) {
						List<Bitmap> listBitmaps=new List<Bitmap>();
						ToothChartBitmapRequest toothChartBitmapRequest=Request(relay,listBitmaps);
						wrapper.DestroyHandleForTests();
						Assert.IsFalse(wrapper.IsHandleCreated);
						wrapper.CreateHandleForTests();
						Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartBitmapRequest.IsDone,20000),"The new handle asked again.");
						using Bitmap bitmap=listBitmaps.Single();
						AssertSamePixels(bitmapExpected,bitmap,"After the handle came back");
					}
					//The handle goes while the render runs: its late result is left alone and the newest request is made again.
					ChartScenes.Prepare(relay,ChartScenes.Get("treatment-plan"),ChartColorSet.Module);
					using(Bitmap bitmapExpected=relay.GetBitmap()) {
						List<Bitmap> listBitmaps=new List<Bitmap>();
						ToothChartBitmapRequest toothChartBitmapRequest;
						int countMade=wrapper.CountBitmapsMadeInBackground;
						using(RenderJobHold renderJobHold=new RenderJobHold(ToothChartRenderer.JobRender)) {
							toothChartBitmapRequest=Request(relay,listBitmaps);
							Assert.IsTrue(WpfRendererTests.PumpUntil(() => renderJobHold.WaitEntered(0),20000));
							wrapper.DestroyHandleForTests();
							wrapper.CreateHandleForTests();
							renderJobHold.Release();
						}
						Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartBitmapRequest.IsDone,20000),"Made again after the handle came back.");
						WpfRendererTests.PumpUntil(() => false,300);
						using Bitmap bitmap=listBitmaps.Single();
						AssertSamePixels(bitmapExpected,bitmap,"The picture made again");
						Assert.AreEqual(countMade+1,wrapper.CountBitmapsMadeInBackground,"The given-up job's result was left alone.");
					}
				}
				finally {
					wrapper.Dispose();
				}
			});
		}

		[TestMethod]
		public void FailedRender_GivesThe2DPicture_AsGetBitmapDoes_AndLogs() {
			int countUnhandled=ToothChartRenderer.CountUnhandledExceptions;
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=CreateHiddenWrapperWithHandle(ChartScenes.SizeModule.Size);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
				List<Bitmap> listBitmaps=new List<Bitmap>();
				Bitmap bitmap2D;
				int countMessages;
				ToothChartRenderer.ActionJobStartForTests=kind => {
					if(kind==ToothChartRenderer.JobRender) {
						throw new InvalidOperationException("A test's failed render.");
					}
				};
				try {
					bitmap2D=relay.GetBitmap();//GetBitmap's fallback: the 2D chart's picture of the data
					countMessages=ToothChartWrapper.GetCountFallbackMessages();
					ToothChartBitmapRequest toothChartBitmapRequest=Request(relay,listBitmaps);
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartBitmapRequest.IsDone,20000));
				}
				finally {
					ToothChartRenderer.ActionJobStartForTests=null;
				}
				List<string> listMessages=ToothChartWrapper.GetFallbackMessagesSince(countMessages);
				Assert.AreEqual(1,listMessages.Count,string.Join("; ",listMessages));
				StringAssert.Contains(listMessages[0],"in the background");
				StringAssert.Contains(listMessages[0],"A test's failed render.");
				using(bitmap2D)
				using(Bitmap bitmap=listBitmaps.Single()) {
					AssertSamePixels(bitmap2D,bitmap,"The 2D chart's picture, as GetBitmap gives after a failed render");
				}
				using(Bitmap bitmapAfter=relay.GetBitmap()) {//a render that works again, so the render thread's run of failures ends here
					Assert.AreEqual(ChartScenes.SizeModule.Size,bitmapAfter.Size);
				}
			});
			Assert.AreEqual(countUnhandled,ToothChartRenderer.CountUnhandledExceptions);
		}

		[TestMethod]
		public void CallersThread_GetsNoWpfObject() {
			StaRunner.Run(() => {
				using ToothChartWrapper wrapper=CreateHiddenWrapperWithHandle(ChartScenes.SizeModule.Size);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
				List<Bitmap> listBitmaps=new List<Bitmap>();
				ToothChartBitmapRequest toothChartBitmapRequest=Request(relay,listBitmaps);
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartBitmapRequest.IsDone,20000));
				listBitmaps.Single().Dispose();
				Assert.IsNull(System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread),"No WPF object was made on the caller's thread.");
				Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId,ToothChartRenderer.Instance.ThreadId,"The render thread made it.");
			});
		}

		[TestMethod]
		public void ShownChartDrawnInSoftware_ScreenFirst_ThenTheSamePicture() {
			WpfLiveViewTests.RequireDesktop();
			StaRunner.Run(() => {
				using Form form=WpfCharts.CreateOffscreenForm(ChartScenes.SizeModule.Size);
				ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
				wrapper.WpfDrawInSoftware=true;
				form.Controls.Add(wrapper);
				form.Show();
				ToothChartImageHost toothChartImageHost=wrapper.ImageHostWpf;
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartImageHost.CountImagesReceived>=1 && !toothChartImageHost.IsRenderInFlight,20000));
				int countImages=toothChartImageHost.CountImagesReceived;
				//The refill, as ControlChart.FillToothChart makes it with the dashboard visible.
				ChartScenes.Prepare(relay,ChartScenes.Get("treatment-plan"),ChartColorSet.Module);
				ToothChartBitmapRequest toothChartBitmapRequest=relay.RequestBitmapInBackground();
				Assert.IsNotNull(toothChartBitmapRequest);
				bool isScreenFirst=false;
				List<Bitmap> listBitmaps=new List<Bitmap>();
				toothChartBitmapRequest.SetActionDone(bitmap => {
					isScreenFirst=toothChartImageHost.CountImagesReceived>countImages;
					listBitmaps.Add(bitmap);
				});
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartBitmapRequest.IsDone && toothChartImageHost.CountImagesReceived>countImages
					&& !toothChartImageHost.IsRenderInFlight,20000));
				Assert.IsTrue(isScreenFirst,"The screen's picture arrived before the dashboard's: its render was queued first.");
				using(Bitmap bitmap=listBitmaps.Single())
				using(Bitmap bitmapShown=toothChartImageHost.GetBitmapShown())
				using(Bitmap bitmapGet=relay.GetBitmap()) {
					AssertSamePixels(bitmapShown,bitmap,"The dashboard's picture is the screen's");
					AssertSamePixels(bitmapGet,bitmap,"and GetBitmap's");
				}
				wrapper.Dispose();
				form.Close();
			});
		}

		[TestMethod]
		public void ShownLiveView_RefillReturnsAtOnce_TheViewShowsIt_ThenThePictureArrives() {
			WpfLiveViewTests.RequireDesktop();
			StaRunner.Run(() => {
				using Form form=WpfCharts.CreateOffscreenForm(ChartScenes.SizeModule.Size);
				ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
				form.Controls.Add(wrapper);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
				form.Show();
				ToothChartWpfView toothChartWpfView=WpfCharts.WaitForLiveView(wrapper);
				Assert.IsNotNull(toothChartWpfView,"The live view started.");
				int countShown=toothChartWpfView.CountScenesShown;
				ChartScenes.Prepare(relay,ChartScenes.Get("treatment-plan"),ChartColorSet.Module);
				ToothChartBitmapRequest toothChartBitmapRequest=relay.RequestBitmapInBackground();
				Assert.IsNotNull(toothChartBitmapRequest);
				bool isViewFirst=false;
				List<Bitmap> listBitmaps=new List<Bitmap>();
				toothChartBitmapRequest.SetActionDone(bitmap => {
					isViewFirst=toothChartWpfView.CountScenesShown>countShown;
					listBitmaps.Add(bitmap);
				});
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartBitmapRequest.IsDone,20000));
				Assert.IsTrue(isViewFirst,"The live view showed the refill before the dashboard's picture arrived.");
				Assert.AreEqual(countShown+1,toothChartWpfView.CountScenesShown,"One rebuild for the refill.");
				using(Bitmap bitmap=listBitmaps.Single())
				using(Bitmap bitmapScene=ToothChartRenderer.Instance.Render(toothChartWpfView.SceneShown,null).ToBitmap())
				using(Bitmap bitmapGet=relay.GetBitmap()) {
					AssertSamePixels(bitmapScene,bitmap,"The dashboard's picture is of the scene the view shows");
					AssertSamePixels(bitmapGet,bitmap,"and GetBitmap's");
				}
				wrapper.Dispose();
				form.Close();
			});
		}
	}
}
