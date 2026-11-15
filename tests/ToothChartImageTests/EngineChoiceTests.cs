// Tests of package TC-8 (docs/specs/wpf-tooth-chart-plan.md, the TC-8 row; docs/port-notes/TC-8.md): the engine choice moves to the WPF chart.
// The mapping (a stored 0 and 2 start the WPF chart, 1 stays the 2D chart, O-30), Setup > Graphics' save rule, the "Draw in software" switch set
// before the engine, Wpf3D's lack of a stored form, the wrapper's DrawMode request, the dashboard's MTA path, and the live view's fallback when
// WPF's render thread fails on the UI thread. No database: the settings are passed as ComputerPref objects (ToothChartRelay.ApplyComputerPref);
// the host tests (tests/ToothChartHostTests) check the same through the program on a scratch database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace ToothChartImageTests {
	[TestClass]
	public class EngineChoiceTests {
		private static ComputerPref NewComputerPref(DrawingMode drawingMode,string directXFormat) {
			return new ComputerPref { ComputerName="SYNTHETIC-PC",GraphicsSimple=drawingMode,DirectXFormat=directXFormat,GraphicsUseHardware=true,
				PreferredPixelFormatNum=7,GraphicsDoubleBuffering=true,GraphicsUseDirectX11=YN.Unknown,ComputerOS=PlatformOD.Win32NT };
		}

		#region The mapping
		[TestMethod]
		public void StoredModes_ZeroAndTwoGiveWpf3D_OneGivesSimple2D() {
			Assert.AreEqual(ToothChartEngine.Wpf3D,ToothChartRelay.GetEngineForStoredMode(DrawingMode.DirectX),"0 (DirectX) starts the WPF chart.");
			Assert.AreEqual(ToothChartEngine.Simple2D,ToothChartRelay.GetEngineForStoredMode(DrawingMode.Simple2D),"1 stays the 2D chart (O-30).");
			Assert.AreEqual(ToothChartEngine.Wpf3D,ToothChartRelay.GetEngineForStoredMode(DrawingMode.OpenGL),"2 (OpenGL) starts the WPF chart.");
			Assert.AreEqual(ToothChartEngine.Wpf3D,ToothChartRelay.GetEngineForStoredMode((DrawingMode)5),"Any other stored number, as the wrapper's DrawMode.");
			Assert.IsTrue(ToothChartRelay.Is3DChart(NewComputerPref(DrawingMode.DirectX,"")));
			Assert.IsTrue(ToothChartRelay.Is3DChart(NewComputerPref(DrawingMode.OpenGL,"")));
			Assert.IsFalse(ToothChartRelay.Is3DChart(NewComputerPref(DrawingMode.Simple2D,"")));
			//The stored values stay 0, 1 and 2: DrawingMode gets no new value, and Wpf3D is the in-memory engine only.
			CollectionAssert.AreEqual(new[] { 0,1,2 },Enum.GetValues(typeof(DrawingMode)).Cast<int>().ToArray());
			Assert.IsFalse(Enum.GetNames(typeof(DrawingMode)).Contains("Wpf3D"),"Wpf3D has no stored form.");
		}

		[TestMethod]
		public void SoftwareSwitch_ReadsTheStoredValue() {
			Assert.IsFalse(ToothChartRelay.IsDrawnInSoftware(null));
			Assert.IsFalse(ToothChartRelay.IsDrawnInSoftware(NewComputerPref(DrawingMode.DirectX,"")),"Empty is the live view.");
			Assert.IsFalse(ToothChartRelay.IsDrawnInSoftware(NewComputerPref(DrawingMode.DirectX,"0;Hardware;64;D16;R5G6B5;FourSamples")),
				"A DirectX device format an older build saved is the live view.");
			Assert.IsTrue(ToothChartRelay.IsDrawnInSoftware(NewComputerPref(DrawingMode.DirectX,ToothChartWrapper.DirectXFormatWpfSoftware)));
			Assert.IsTrue(ToothChartRelay.IsDrawnInSoftware(NewComputerPref(DrawingMode.OpenGL," WpfSoftware ")));
			Assert.IsFalse(ToothChartRelay.IsDrawnInSoftware(NewComputerPref(DrawingMode.DirectX,"wpfsoftware")),"The stored value is exact.");
		}

		[TestMethod]
		public void ApplyComputerPref_StartsTheEngineOfTheStoredMode_WithTheSwitchFirst() {
			StaRunner.Run(() => {
				foreach(DrawingMode drawingMode in new[] { DrawingMode.DirectX,DrawingMode.Simple2D,DrawingMode.OpenGL }) {
					foreach(string directXFormat in new[] { "","0;Hardware;64;D16;R5G6B5;FourSamples",ToothChartWrapper.DirectXFormatWpfSoftware }) {
						using(ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size)) {
							wrapper.Engine=ToothChartEngine.Simple2D;
							int countBefore=ToothChartWrapper.GetCountFallbackMessages();
							ComputerPref computerPref=NewComputerPref(drawingMode,directXFormat);
							ComputerPref computerPrefBefore=computerPref.Copy();
							ToothChartRelay.ApplyComputerPref(wrapper,computerPref);
							string what=drawingMode+" with \""+directXFormat+"\"";
							Assert.AreEqual(drawingMode==DrawingMode.Simple2D ? ToothChartEngine.Simple2D : ToothChartEngine.Wpf3D,wrapper.Engine,what);
							Assert.AreEqual(directXFormat==ToothChartWrapper.DirectXFormatWpfSoftware,wrapper.WpfDrawInSoftware,what);
							Assert.AreEqual(drawingMode==DrawingMode.Simple2D ? DrawingMode.Simple2D : DrawingMode.DirectX,wrapper.DrawMode,
								what+": Wpf3D reads as DirectX, so a caller that saves DrawMode keeps the 3D chart.");
							Assert.AreEqual(0,ToothChartWrapper.GetFallbackMessagesSince(countBefore).Count,what+": nothing fell back.");
							AssertFieldsEqual(computerPrefBefore,computerPref,what+": the settings are read, never changed.");
						}
					}
				}
			});
			//The order in the source: the switch before the engine (TC-3's fix round 3), so a computer drawn in software never makes a live view first.
			string method=SourceText.GetMethod(SourceText.ReadCode(@"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs"),
				"public static void ApplyComputerPref(ToothChartWrapper toothChartWrapper,ComputerPref computerPref) {");
			SourceText.AssertInOrder(method,"ApplyComputerPref","toothChartWrapper.WpfDrawInSoftware=IsDrawnInSoftware(computerPref);",
				"toothChartWrapper.Engine=GetEngineForStoredMode(computerPref.GraphicsSimple);");
			StringAssert.DoesNotMatch(method,new System.Text.RegularExpressions.Regex(@"ComputerPrefs\.Update|DeviceFormat|UseHardware|PreferredPixelFormat"));
			string settings=SourceText.GetMethod(SourceText.ReadCode(@"WpfControlsOD\UI\ToothChart\ToothChartRelay.cs"),
				"public static void ApplyComputerSettings(ToothChartWrapper toothChartWrapper,ToothChartSettingsUse toothChartSettingsUse) {");
			StringAssert.Contains(settings,"ApplyComputerPref(toothChartWrapper,ComputerPrefs.LocalComputer);");
			StringAssert.DoesNotMatch(settings,new System.Text.RegularExpressions.Regex(@"ComputerPrefs\.Update"),"No ComputerPrefs.Update when an image is made.");
		}

		[TestMethod]
		public void ShownWrapper_OfAComputerDrawnInSoftware_NeverMakesALiveView() {
			WpfLiveViewTests.RequireDesktop();
			StaRunner.Run(() => {
				int countViewsMade=0;
				ToothChartWpfView.ActionStepForTests=(toothChartWpfView,step) => {
					if(step==ToothChartWpfView.StepCreate) {
						countViewsMade++;
					}
				};
				try {
					using Form form=WpfCharts.CreateOffscreenForm(ChartScenes.SizeModule.Size);
					ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
					form.Controls.Add(wrapper);
					form.Show();
					Application.DoEvents();
					//A shown wrapper that already runs the WPF chart's live view, as the Chart module's does, then gets a computer drawn in software.
					ToothChartRelay.ApplyComputerPref(wrapper,NewComputerPref(DrawingMode.DirectX,""));
					WpfLiveViewTests.WaitForLiveView(wrapper);
					int countViewsBefore=countViewsMade;
					ToothChartRelay.ApplyComputerPref(wrapper,NewComputerPref(DrawingMode.OpenGL,ToothChartWrapper.DirectXFormatWpfSoftware));
					ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
					ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
					ToothChartImageHost toothChartImageHost=wrapper.ImageHostWpf;
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartImageHost.CountImagesReceived>=1 && !toothChartImageHost.IsRenderInFlight,20000),
						"The image host shows the scene.");
					Assert.AreEqual(countViewsBefore,countViewsMade,"The engine restarted drawn in software: no live view was made first.");
					Assert.IsNull(wrapper.LiveViewWpf);
					Assert.IsTrue(toothChartImageHost.Visible);
					wrapper.Dispose();
					form.Close();
				}
				finally {
					ToothChartWpfView.ActionStepForTests=null;
				}
			});
		}

		[TestMethod]
		public void DrawModeRequests_OfDirectXAndOpenGL_StartWpf3D_AndSimple2DStays() {
			StaRunner.Run(() => {
				foreach(DrawingMode drawingMode in new[] { DrawingMode.DirectX,DrawingMode.OpenGL,DrawingMode.Simple2D }) {
					using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
						wrapper.Size=ChartScenes.SizeModule.Size;
						int countBefore=ToothChartWrapper.GetCountFallbackMessages();
						wrapper.DrawMode=drawingMode;
						Assert.AreEqual(drawingMode==DrawingMode.Simple2D ? ToothChartEngine.Simple2D : ToothChartEngine.Wpf3D,wrapper.Engine,drawingMode.ToString());
						Assert.AreEqual(0,ToothChartWrapper.GetFallbackMessagesSince(countBefore).Count,drawingMode+": nothing fell back, and no DirectX was tried.");
						using(Bitmap bitmap=wrapper.GetBitmap()) {
							Assert.AreEqual(ChartScenes.SizeModule.Size,bitmap.Size);
						}
					}
				}
			});
		}
		[TestMethod]
		public void StoredChoice_WhoseWpfStartFails_FallsBackToSimple2D_WithOneLogLine() {
			//In a fresh process, because a start failure and a logged line are kept for the whole process (SecondProcess, "stored-start-fails").
			SecondProcessResult result=SecondProcess.Run(120000,"stored-start-fails");
			Console.WriteLine(result.Output);
			Assert.AreEqual(0,result.ExitCode,result.Output);
			StringAssert.Contains(result.Output,"engine stored 0: Simple2D");
			StringAssert.Contains(result.Output,"engine requested 2: Simple2D");
			StringAssert.Contains(result.Output,"bitmap: 410x307, blank False");
			StringAssert.Contains(result.Output,"fallback messages: 2","Each wrapper fell back once; no DirectX or OpenGL engine was tried.");
			StringAssert.Contains(result.Output,"log lines: 1","One line was written.");
			StringAssert.Contains(result.Output,"message: The Wpf3D tooth chart could not start (InvalidOperationException: ");
			StringAssert.Contains(result.Output,"). Using Simple2D.");
			Assert.IsFalse(result.Output.Contains("DirectX tooth chart"),"No DirectX fallback line.");
			Assert.IsFalse(result.Output.Contains("OpenGL tooth chart"),"No OpenGL fallback line.");
		}
		#endregion The mapping

		#region The save rule
		[TestMethod]
		public void SaveRule_3DChartWritesZeroOnlyOverOne_AndKeepsAStoredZeroOrTwo() {
			//(stored, "3D chart") -> stored after the save
			object[][] arrayCases=new object[][] {
				new object[] { DrawingMode.Simple2D,true,DrawingMode.DirectX },
				new object[] { DrawingMode.DirectX,true,DrawingMode.DirectX },
				new object[] { DrawingMode.OpenGL,true,DrawingMode.OpenGL },
				new object[] { DrawingMode.DirectX,false,DrawingMode.Simple2D },
				new object[] { DrawingMode.OpenGL,false,DrawingMode.Simple2D },
				new object[] { DrawingMode.Simple2D,false,DrawingMode.Simple2D },
			};
			foreach(object[] arrayCase in arrayCases) {
				ComputerPref computerPref=NewComputerPref((DrawingMode)arrayCase[0],"");
				ComputerPref computerPrefBefore=computerPref.Copy();
				ToothChartRelay.SetGraphicsChoice(computerPref,(bool)arrayCase[1],false);
				string what="stored "+arrayCase[0]+", 3D chart "+arrayCase[1];
				Assert.AreEqual((DrawingMode)arrayCase[2],computerPref.GraphicsSimple,what);
				computerPrefBefore.GraphicsSimple=computerPref.GraphicsSimple;
				AssertFieldsEqual(computerPrefBefore,computerPref,what+": no other column is written.");
			}
		}

		[TestMethod]
		public void SaveRule_DrawInSoftware_WritesOrClearsOnlyItsValue() {
			string formatDirectX="0;Hardware;64;D16;R5G6B5;FourSamples";
			//(stored DirectXFormat, "Draw in software") -> DirectXFormat after the save
			string[][] arrayCases=new string[][] {
				new[] { "","on",ToothChartWrapper.DirectXFormatWpfSoftware },
				new[] { formatDirectX,"on",ToothChartWrapper.DirectXFormatWpfSoftware },
				new[] { ToothChartWrapper.DirectXFormatWpfSoftware,"on",ToothChartWrapper.DirectXFormatWpfSoftware },
				new[] { ToothChartWrapper.DirectXFormatWpfSoftware,"off","" },
				new[] { " WpfSoftware ","off","" },
				new[] { formatDirectX,"off",formatDirectX },
				new[] { "","off","" },
			};
			foreach(bool is3DChart in new[] { true,false }) {
				foreach(string[] arrayCase in arrayCases) {
					ComputerPref computerPref=NewComputerPref(DrawingMode.DirectX,arrayCase[0]);
					ToothChartRelay.SetGraphicsChoice(computerPref,is3DChart,arrayCase[1]=="on");
					Assert.AreEqual(arrayCase[2],computerPref.DirectXFormat,"\""+arrayCase[0]+"\", "+arrayCase[1]+", 3D chart "+is3DChart);
					//What was saved reads back as the window's choice.
					Assert.AreEqual(arrayCase[1]=="on",ToothChartRelay.IsDrawnInSoftware(computerPref));
					Assert.AreEqual(is3DChart,ToothChartRelay.Is3DChart(computerPref));
				}
			}
		}

		[TestMethod]
		public void SaveRule_SavedThenApplied_GivesTheWindowsChoice() {
			StaRunner.Run(() => {
				foreach(DrawingMode drawingModeStored in new[] { DrawingMode.DirectX,DrawingMode.Simple2D,DrawingMode.OpenGL }) {
					foreach(bool is3DChart in new[] { true,false }) {
						foreach(bool isDrawnInSoftware in new[] { true,false }) {
							ComputerPref computerPref=NewComputerPref(drawingModeStored,"");
							ToothChartRelay.SetGraphicsChoice(computerPref,is3DChart,isDrawnInSoftware);
							using(ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size)) {
								ToothChartRelay.ApplyComputerPref(wrapper,computerPref);
								string what="stored "+drawingModeStored+", 3D "+is3DChart+", software "+isDrawnInSoftware;
								Assert.AreEqual(is3DChart ? ToothChartEngine.Wpf3D : ToothChartEngine.Simple2D,wrapper.Engine,what);
								Assert.AreEqual(isDrawnInSoftware,wrapper.WpfDrawInSoftware,what);
							}
						}
					}
				}
			});
		}

		///<summary>Every public field of the two ComputerPref objects is equal.</summary>
		private static void AssertFieldsEqual(ComputerPref computerPrefExpected,ComputerPref computerPrefActual,string what) {
			foreach(FieldInfo fieldInfo in typeof(ComputerPref).GetFields(BindingFlags.Public|BindingFlags.Instance)) {
				Assert.AreEqual(fieldInfo.GetValue(computerPrefExpected),fieldInfo.GetValue(computerPrefActual),what+" ("+fieldInfo.Name+")");
			}
		}
		#endregion The save rule

		#region The dashboard's MTA worker
		[TestMethod]
		public void MtaWorker_GetsTheWpfChartFromTheStoredMode_AndNothingIsLogged() {
			//The Patient Dashboard makes its wrapper on an MTA ODThread worker (UserControlDashboard, DashToothChart, SheetPrinting.GetToothChartHelper).
			//Since TC-1 the wrapper makes no WPF object there; since TC-8 the stored 0 or 2 asks for the WPF chart there too.
			foreach(DrawingMode drawingMode in new[] { DrawingMode.DirectX,DrawingMode.OpenGL }) {
				Exception exception=null;
				string result="";
				Thread thread=new Thread(() => {
					try {
						int countBefore=ToothChartWrapper.GetCountFallbackMessages();
						using(Form form=new Form())
						using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
							wrapper.Size=new Size(410,307);
							ToothChartRelay.ApplyComputerPref(wrapper,NewComputerPref(drawingMode,""));
							form.Controls.Add(wrapper);
							ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
							ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
							using(Bitmap bitmap=relay.GetBitmap()) {
								result=wrapper.Engine+" "+bitmap.Width+"x"+bitmap.Height+" blank "+ChartImageCompare.DescribeBitmap(bitmap).IsBlank
									+" logged "+ToothChartWrapper.GetFallbackMessagesSince(countBefore).Count;
							}
						}
					}
					catch(Exception ex) {
						exception=ex;
					}
				});
				thread.SetApartmentState(ApartmentState.MTA);
				thread.Start();
				Assert.IsTrue(thread.Join(120000),"The MTA worker finished.");
				Assert.IsNull(exception,exception?.ToString());
				Assert.AreEqual("Wpf3D 410x307 blank False logged 0",result,drawingMode.ToString());
			}
		}
		#endregion The dashboard's MTA worker

		#region The live view's render-thread failure on the UI thread
		[TestMethod]
		public void RenderThreadFailure_IsRecognisedByItsHResultOrItsThrower() {
			Assert.AreEqual(unchecked((int)0x88980406),ToothChartWrapper.HRESULT_UCEERR_RENDERTHREADFAILURE);
			Assert.IsTrue(ToothChartWrapper.IsWpfRenderThreadFailure(new COMException("UCEERR_RENDERTHREADFAILURE",ToothChartWrapper.HRESULT_UCEERR_RENDERTHREADFAILURE)));
			Assert.IsTrue(ToothChartWrapper.IsWpfRenderThreadFailure(new InvalidOperationException("Wrapped.",
				new COMException("UCEERR_RENDERTHREADFAILURE",ToothChartWrapper.HRESULT_UCEERR_RENDERTHREADFAILURE))),"Inside another exception.");
			Assert.IsFalse(ToothChartWrapper.IsWpfRenderThreadFailure(null));
			Assert.IsFalse(ToothChartWrapper.IsWpfRenderThreadFailure(new InvalidOperationException("Something else.")));
			Assert.IsFalse(ToothChartWrapper.IsWpfRenderThreadFailure(new COMException("Another HRESULT.",unchecked((int)0x88980403))));
			//The exception MediaContext.NotifyPartitionIsZombie throws (InvalidOperationException, "An unspecified error occurred on the render thread")
			//carries no HRESULT of its own; its thrower names it. WPF's own method is internal, so it is called by reflection with the failure code WPF
			//passes it, which throws without touching a render thread.
			MethodInfo methodInfoZombie=typeof(System.Windows.Media.Visual).Assembly.GetType("System.Windows.Media.MediaContext")
				?.GetMethod("NotifyPartitionIsZombie",BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static);
			if(methodInfoZombie==null) {
				Assert.Inconclusive("MediaContext.NotifyPartitionIsZombie is not in this WPF; the HRESULT checks above ran.");
			}
			Exception exceptionZombie=null;
			string whyNot="";
			StaRunner.Run(() => {
				object mediaContext=null;
				try {
					if(!methodInfoZombie.IsStatic) {
						mediaContext=typeof(System.Windows.Media.Visual).Assembly.GetType("System.Windows.Media.MediaContext")
							.GetMethod("From",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[] { System.Windows.Threading.Dispatcher.CurrentDispatcher });
					}
				}
				catch(Exception ex) {
					whyNot="MediaContext.From failed: "+ex.GetBaseException().Message;
					return;
				}
				try {
					methodInfoZombie.Invoke(mediaContext,new object[] { unchecked((int)0x88980406) });
				}
				catch(TargetInvocationException ex) {
					exceptionZombie=ex.InnerException;
				}
			});
			if(whyNot!="") {
				Assert.Inconclusive(whyNot+"; the HRESULT checks above ran.");
			}
			Assert.IsNotNull(exceptionZombie,"NotifyPartitionIsZombie threw.");
			Assert.IsTrue(ToothChartWrapper.IsWpfRenderThreadFailure(exceptionZombie),exceptionZombie.GetType().Name+": "+exceptionZombie.Message);
		}

		[TestMethod]
		public void LiveView_WhoseRenderThreadFailsOnTheUiThread_FallsBackToTheImageHost() {
			WpfLiveViewTests.RequireDesktop();
			StaRunner.Run(() => {
				using Form form=WpfCharts.CreateOffscreenForm(ChartScenes.SizeModule.Size);
				ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
				form.Controls.Add(wrapper);
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
				form.Show();
				ToothChartWpfView toothChartWpfView=WpfLiveViewTests.WaitForLiveView(wrapper);
				ToothChartImageHost toothChartImageHost=wrapper.ImageHostWpf;
				int countImages=toothChartImageHost.CountImagesReceived;
				int countBefore=ToothChartWrapper.GetCountFallbackMessages();
				//Another exception on the same Dispatcher is not the chart's: it is left unhandled for the program's handlers (here the test's, added after
				//the wrapper's, which sees it unhandled and handles it so the test goes on).
				List<string> listSeenByLaterHandler=new List<string>();
				System.Windows.Threading.DispatcherUnhandledExceptionEventHandler handlerLater=(sender,e) => {
					listSeenByLaterHandler.Add(e.Exception.Message+" handled before: "+e.Handled);
					e.Handled=true;
				};
				System.Windows.Threading.Dispatcher dispatcher=System.Windows.Threading.Dispatcher.CurrentDispatcher;
				dispatcher.UnhandledException+=handlerLater;
				try {
					dispatcher.BeginInvoke(new Action(() => throw new InvalidOperationException("Not a render failure.")));
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => listSeenByLaterHandler.Count==1,20000));
					Assert.AreEqual("Not a render failure. handled before: False",listSeenByLaterHandler[0]);
					Assert.AreSame(toothChartWpfView,wrapper.LiveViewWpf,"The live view stays.");
					//The failure WPF throws on the UI thread when its render thread fails.
					dispatcher.BeginInvoke(new Action(() => throw new COMException("UCEERR_RENDERTHREADFAILURE",ToothChartWrapper.HRESULT_UCEERR_RENDERTHREADFAILURE)));
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => wrapper.LiveViewWpf==null,20000),"The live view is released.");
					Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartImageHost.CountImagesReceived>countImages && !toothChartImageHost.IsRenderInFlight,20000),
						"The image host shows the current data.");
					Assert.AreEqual(2,listSeenByLaterHandler.Count);
					StringAssert.EndsWith(listSeenByLaterHandler[1],"handled before: True","The chart handled the render failure.");
				}
				finally {
					dispatcher.UnhandledException-=handlerLater;
				}
				Assert.IsTrue(toothChartWpfView.IsDisposed,"The failed view is released.");
				Assert.IsTrue(toothChartImageHost.Visible);
				Assert.AreSame(toothChartImageHost,wrapper.Controls.Cast<Control>().Single());
				List<string> listMessages=ToothChartWrapper.GetFallbackMessagesSince(countBefore);
				Assert.AreEqual(1,listMessages.Count,string.Join("; ",listMessages));
				StringAssert.Contains(listMessages[0],"live view could not draw on screen (COMException: UCEERR_RENDERTHREADFAILURE). Showing its software render on screen.");
				using(Bitmap bitmap=relay.GetBitmap())
				using(Bitmap bitmapShown=toothChartImageHost.GetBitmapShown()) {
					Assert.IsTrue(ChartImageCompare.GetPixelsArgb(bitmap).SequenceEqual(ChartImageCompare.GetPixelsArgb(bitmapShown)),"The image host shows the scene.");
				}
				//The wrapper stays on the image host: no live view is made again until the engine starts again.
				int countImagesAfter=toothChartImageHost.CountImagesReceived;
				relay.SetSelected("8",true);
				Assert.IsTrue(WpfRendererTests.PumpUntil(() => toothChartImageHost.CountImagesReceived>countImagesAfter && !toothChartImageHost.IsRenderInFlight,20000));
				Assert.IsNull(wrapper.LiveViewWpf,"No new live view.");
				wrapper.Dispose();
				form.Close();
			});
		}
		#endregion The live view's render-thread failure on the UI thread
	}
}
