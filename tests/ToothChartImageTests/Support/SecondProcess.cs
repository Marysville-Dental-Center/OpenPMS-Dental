// A second process for the WPF chart's tests (package TC-1): the determinism test's second render, the process-exit test, the cold render times,
// and the start and logging tests that need a fresh process.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CodeBase;
using OpenPmsDental;
using SparksToothChart;
using SparksToothChart.Wpf3D;
using TestProbeHost;

namespace ToothChartImageTests {
	///<summary>What a second process did.</summary>
	public class SecondProcessResult {
		public int ExitCode;
		public bool IsTimedOut;
		public string Output;
		public double MillisecondsElapsed;
	}

	///<summary>Runs SecondProcessEntry.Run in a new 32-bit process. The test assembly is a library, so TestProbeHost runs it as its Main
	///(ProbeProcess.RunOnce): a copy of the host in the test assembly's folder (so it finds the same assemblies), with the test assembly's binding
	///redirects as its config, started with no window, and deleted afterwards. Its Main is [MTAThread]: the WPF chart needs no STA caller. Before
	///S3a-8b of the .NET 10 move a tiny console program was compiled for each run with the .NET Framework's own C# compiler, which .NET does not
	///have; the process is the same: the program's Main calls Run and returns its exit code, and the process ends once only background threads
	///are left.</summary>
	public static class SecondProcess {
		public static SecondProcessResult Run(int millisecondsTimeout,params string[] arrayArgs) {
			string folder=Path.GetDirectoryName(typeof(SecondProcessEntry).Assembly.Location);
			string pathConfigTests=typeof(SecondProcessEntry).Assembly.Location+".config";
			string configText=File.Exists(pathConfigTests) ? File.ReadAllText(pathConfigTests) : null;
			ProbeRunResult probeRunResult=ProbeProcess.RunOnce(folder,configText,typeof(SecondProcessEntry),arrayArgs,
				TimeSpan.FromMilliseconds(millisecondsTimeout));
			SecondProcessResult result=new SecondProcessResult();
			result.ExitCode=probeRunResult.ExitCode;
			result.IsTimedOut=probeRunResult.IsTimedOut;
			result.Output=probeRunResult.Output;
			result.MillisecondsElapsed=probeRunResult.Elapsed.TotalMilliseconds;
			return result;
		}
	}

	///<summary>The second process's work. No MSTest type is touched here, so the program needs only the chart's assemblies.</summary>
	public static class SecondProcessEntry {
		///<summary>args[0] is the mode:
		///"hash &lt;scene&gt;": renders the scene at 410x307 with the module colours and prints its pixel SHA-256.
		///"exit": renders one image and returns at once, leaving the render thread alive; the process must still end.
		///"times &lt;out file&gt;": measures the render times for O-19 and G1 (WpfTimings) in a process that draws WPF in software, as the program
		///does (O-34), and writes the report.
		///"onscreen-times &lt;out file&gt;": the on-screen times alone (DirectX, the live view and its frames, the image host), the same way.
		///"start-fails", "start-interrupted" and "log-kinds": a start failure, an interrupted cold start and the logged-once lines, each in a fresh
		///process because a start failure and a logged line are kept for the whole process (see their methods).
		///"abort-race" and "shutdown-throws": an aborted job's late Aborted handler and an error in the render Dispatcher's own shutdown, each in a
		///fresh process because, unguarded, they end the process (see their methods).</summary>
		public static int Run(string[] args) {
			try {
				string mode=args.Length>0?args[0]:"";
				switch(mode) {
					case "hash":
						Console.WriteLine("SHA256 "+RenderHash(args[1]));
						return 0;
					case "exit":
						using(Bitmap bitmap=RenderOne()) {
							Console.WriteLine("Rendered "+bitmap.Width+"x"+bitmap.Height+"; render thread alive: "
								+SparksToothChart.Wpf3D.ToothChartRenderer.Instance.IsThreadAlive);
						}
						return 0;//no shutdown: the background render thread must not keep the process alive
					case "times":
						//As OpenPmsDental.exe does first in Main (ProgramEntry, O-34): every WPF window of the process, the live view included, is drawn
						//in software. It must be set before WPF makes its first window.
						System.Windows.Media.RenderOptions.ProcessRenderMode=System.Windows.Interop.RenderMode.SoftwareOnly;
						string report=WpfTimings.Measure(true).GetReport();
						File.WriteAllText(args[1],report,new UTF8Encoding(false));
						Console.WriteLine(report);
						return 0;
					case "onscreen-times":
						//TC-3's on-screen times alone (WpfOnScreenTimings), drawn in software as the program draws (O-34), set before WPF's first window.
						System.Windows.Media.RenderOptions.ProcessRenderMode=System.Windows.Interop.RenderMode.SoftwareOnly;
						string reportOnScreen=WpfTimings.MeasureOnScreen().GetReport();
						File.WriteAllText(args[1],reportOnScreen,new UTF8Encoding(false));
						Console.WriteLine(reportOnScreen);
						return 0;
					case "start-fails":
						RunStartFails();
						return 0;
					case "stored-start-fails":
						RunStoredStartFails();
						return 0;
					case "start-interrupted":
						RunStartInterrupted();
						return 0;
					case "log-kinds":
						RunLogKinds();
						return 0;
					case "abort-race":
						RunAbortRace();
						return 0;
					case "shutdown-throws":
						RunShutdownThrows();
						return 0;
					default:
						Console.WriteLine("Unknown mode "+mode);
						return 2;
				}
			}
			catch(Exception ex) {
				Console.WriteLine("Failed: "+ex);
				return 1;
			}
		}

		public static string RenderHash(string sceneName) {
			ChartScene scene=ChartScenes.Get(sceneName);
			using(ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size)) {
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				using(Bitmap bitmap=WpfCharts.Draw(relay,scene,ChartColorSet.Module)) {
					if(wrapper.Engine!=ToothChartEngine.Wpf3D) {
						throw new InvalidOperationException("The chart fell back to "+wrapper.Engine+".");
					}
					return ChartImageCompare.Sha256(ChartImageCompare.GetPixelsArgb(bitmap));
				}
			}
		}

		private static Bitmap RenderOne() {
			ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size);
			ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
			return WpfCharts.Draw(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
		}

		///<summary>"start-fails": the renderer's start check fails (a forced failure through ActionJobStartForTests) in a fresh process, so the WPF
		///chart cannot start. Two wrappers ask for it; each falls back to Simple 2D. The start failure is kept, and one line is handed to the log.
		///CodeBase's logger is told it runs in a unit test, so no log file is written.</summary>
		private static void RunStartFails() {
			ODBuild.IsUnitTest=true;
			int countChecks=0;
			ToothChartRenderer.ActionJobStartForTests=kind => {
				if(kind==ToothChartRenderer.JobCheck) {
					Interlocked.Increment(ref countChecks);
					throw new InvalidOperationException("A forced start failure.");
				}
			};
			using(ToothChartWrapper wrapper1=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size))
			using(ToothChartWrapper wrapper2=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size)) {
				Console.WriteLine("engine 1: "+wrapper1.Engine);
				Console.WriteLine("engine 2: "+wrapper2.Engine);
				ChartScenes.Prepare(WpfCharts.CreateRelay(wrapper1),ChartScenes.Get("restored-adult"),ChartColorSet.Module);
				using(Bitmap bitmap=wrapper1.GetBitmap()) {
					Console.WriteLine("bitmap: "+bitmap.Width+"x"+bitmap.Height+", blank "+ChartImageCompare.DescribeBitmap(bitmap).IsBlank);
				}
			}
			Console.WriteLine("start checks run: "+countChecks);
			List<string> listMessages=ToothChartWrapper.GetFallbackMessages();
			Console.WriteLine("fallback messages: "+listMessages.Count);
			Console.WriteLine("log lines: "+ToothChartWrapper.GetCountFallbackLinesWritten());
			foreach(string message in listMessages) {
				Console.WriteLine("message: "+message);
			}
		}

		///<summary>"stored-start-fails" (TC-8): as "start-fails", but the WPF chart is asked for the way the program asks for it since TC-8: a computer
		///stored as DirectX (0) through the relay's ApplyComputerPref, and a DrawMode request of OpenGL (2). Both fall back to Simple 2D, no DirectX or
		///OpenGL engine is tried, and one line is handed to the log.</summary>
		private static void RunStoredStartFails() {
			ODBuild.IsUnitTest=true;
			ToothChartRenderer.ActionJobStartForTests=kind => {
				if(kind==ToothChartRenderer.JobCheck) {
					throw new InvalidOperationException("A forced start failure.");
				}
			};
			using(ToothChartWrapper wrapper1=new ToothChartWrapper())
			using(ToothChartWrapper wrapper2=new ToothChartWrapper()) {
				wrapper1.Size=ChartScenes.SizeModule.Size;
				wrapper2.Size=ChartScenes.SizeModule.Size;
				ToothChartRelay.ApplyComputerPref(wrapper1,new OpenPmsDentalBusiness.ComputerPref { GraphicsSimple=OpenPmsDentalBusiness.DrawingMode.DirectX,
					DirectXFormat="" });
				wrapper2.DrawMode=OpenPmsDentalBusiness.DrawingMode.OpenGL;
				Console.WriteLine("engine stored 0: "+wrapper1.Engine);
				Console.WriteLine("engine requested 2: "+wrapper2.Engine);
				ChartScenes.Prepare(WpfCharts.CreateRelay(wrapper1),ChartScenes.Get("restored-adult"),ChartColorSet.Module);
				using(Bitmap bitmap=wrapper1.GetBitmap()) {
					Console.WriteLine("bitmap: "+bitmap.Width+"x"+bitmap.Height+", blank "+ChartImageCompare.DescribeBitmap(bitmap).IsBlank);
				}
			}
			List<string> listMessages=ToothChartWrapper.GetFallbackMessages();
			Console.WriteLine("fallback messages: "+listMessages.Count);
			Console.WriteLine("log lines: "+ToothChartWrapper.GetCountFallbackLinesWritten());
			foreach(string message in listMessages) {
				Console.WriteLine("message: "+message);
			}
			//File > Graphics' status on this computer (TC-8b, NEW-1) after the real start failure: the reason, and no log line of its own.
			Console.WriteLine("status: "+ToothChartRelay.GetChart3DStatus());
			Console.WriteLine("log lines after the status: "+ToothChartWrapper.GetCountFallbackLinesWritten());
		}

		///<summary>"start-interrupted": the caller of the cold start is interrupted (Thread.Interrupt, as an ODThread's finite Join can abort it)
		///while it waits for the start check. That is the caller's matter, not a start failure: a later start works.</summary>
		private static void RunStartInterrupted() {
			ODBuild.IsUnitTest=true;
			bool isInterrupted=false;
			Exception exceptionOther=null;
			using(RenderJobHold renderJobHold=new RenderJobHold(ToothChartRenderer.JobCheck)) {
				Thread threadCaller=new Thread(() => {
					try {
						ToothChartRenderer.Instance.EnsureStarted();
					}
					catch(ThreadInterruptedException) {
						isInterrupted=true;
					}
					catch(Exception ex) {
						exceptionOther=ex;
					}
				});
				threadCaller.IsBackground=true;
				threadCaller.Start();
				if(!renderJobHold.WaitEntered(60000)) {
					throw new TimeoutException("The start check did not start.");
				}
				threadCaller.Interrupt();
				if(!threadCaller.Join(60000)) {
					throw new TimeoutException("The interrupted caller did not end.");
				}
			}
			Console.WriteLine("caller interrupted: "+isInterrupted+(exceptionOther==null?"":" (other: "+exceptionOther+")"));
			bool isStarted=false;
			try {
				ToothChartRenderer.Instance.EnsureStarted();
				isStarted=true;
			}
			catch(Exception ex) {
				Console.WriteLine("start failed: "+ex);
			}
			Console.WriteLine("started afterwards: "+isStarted);
			using(ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size)) {
				Console.WriteLine("engine: "+wrapper.Engine);
			}
		}

		///<summary>"log-kinds": a fallback line of another kind first (a cursor that could not be made; until TC-9a this was a DirectX fallback line,
		///as a computer without DirectX wrote one, a kind that went with the DirectX chart), then two WPF render failures (the render thread held past
		///a short timeout). The WPF render failure gets its own logged-once line.</summary>
		private static void RunLogKinds() {
			ODBuild.IsUnitTest=true;
			MethodInfo methodInfoLog=typeof(ToothChartWrapper).GetMethod("LogFallbackOnce",BindingFlags.NonPublic|BindingFlags.Static,null,
				new[] { typeof(ToothChartFallbackKind),typeof(string) },null);
			methodInfoLog.Invoke(null,new object[] { ToothChartFallbackKind.Cursor,"The pen cursor could not be made (a test line). Using the arrow." });
			Console.WriteLine("log lines after a line of another kind: "+ToothChartWrapper.GetCountFallbackLinesWritten());
			using(ToothChartWrapper wrapper=WpfCharts.CreateWrapper(ChartScenes.SizeModule.Size)) {
				if(wrapper.Engine!=ToothChartEngine.Wpf3D) {
					throw new InvalidOperationException("The chart fell back to "+wrapper.Engine+".");
				}
				ToothChartRelay relay=WpfCharts.CreateRelay(wrapper);
				ChartScenes.Prepare(relay,ChartScenes.Get("restored-adult"),ChartColorSet.Module);
				using(WpfCharts.HoldRenderThread(WpfCharts.BuildScene(wrapper),250)) {
					using(Bitmap bitmap=relay.GetBitmap()) {
					}
					Console.WriteLine("log lines after a WPF render failure: "+ToothChartWrapper.GetCountFallbackLinesWritten());
					using(Bitmap bitmap=relay.GetBitmap()) {
					}
					Console.WriteLine("log lines after a second WPF render failure: "+ToothChartWrapper.GetCountFallbackLinesWritten());
				}
			}
		}

		///<summary>"abort-race": the order a review of TC-1's fix round found, pinned. A caller queues a job just as the render thread shuts down.
		///The thread aborts the job; the caller adds its Aborted handler, sees the Aborted status, wakes, returns and closes its wait; only then does
		///the thread raise Aborted. Two holds pin that order. The render Dispatcher's OperationPosted hook runs on the caller after the job is
		///queued and before BeginInvoke returns, so it holds the caller before it adds its handler until the thread has aborted the job. A
		///synchronous continuation of the job's Task runs on the render thread inside the abort, after the status is Aborted and the Dispatcher's
		///lock is released and before Aborted is raised, so it holds the thread until the caller has returned. Unfixed, the thread's Aborted
		///handler set the closed event, and the ObjectDisposedException left Dispatcher.Run and ended this process.</summary>
		private static void RunAbortRace() {
			ODBuild.IsUnitTest=true;
			ToothChartRenderer.MillisecondsTimeout=30000;
			ToothChartRenderer toothChartRenderer=ToothChartRenderer.Instance;
			ToothScene toothScene=WpfCharts.BuildScene("restored-adult",ChartScenes.SizeModule);
			toothChartRenderer.Render(toothScene,null);
			Thread threadCaller=Thread.CurrentThread;
			//Not disposed: a hold that timed out may still wait on them when this method returns.
			ManualResetEvent manualResetEventHeld=new ManualResetEvent(false);
			ManualResetEvent manualResetEventRelease=new ManualResetEvent(false);
			ManualResetEvent manualResetEventAborted=new ManualResetEvent(false);
			ManualResetEvent manualResetEventCallerDone=new ManualResetEvent(false);
			Dispatcher dispatcherRender=null;
			int isTaken=0;
			ToothChartRenderer.ActionJobStartForTests=kind => {
				if(kind!=ToothChartRenderer.JobRender || Interlocked.Exchange(ref isTaken,1)==1) {
					return;
				}
				dispatcherRender=Dispatcher.CurrentDispatcher;
				manualResetEventHeld.Set();
				manualResetEventRelease.WaitOne(30000);
			};
			toothChartRenderer.RenderAsync(toothScene,null,null);//the job that holds the thread, so the caller's job stays queued
			if(!manualResetEventHeld.WaitOne(30000)) {
				throw new TimeoutException("The holding job did not start.");
			}
			ToothChartRenderer.ActionJobStartForTests=null;
			bool isArmed=false;
			bool isCallerHeld=false;
			DispatcherOperationStatus statusWhenCallerGoesOn=DispatcherOperationStatus.Pending;
			bool isHeldOnRenderThread=false;
			bool isRenderThreadHeld=false;
			dispatcherRender.Hooks.OperationPosted+=(sender,e) => {
				if(Thread.CurrentThread!=threadCaller || !isArmed) {
					return;//the holding job, and the shutdown posted below
				}
				isArmed=false;
				e.Operation.Task.ContinueWith(task => {
					isHeldOnRenderThread=Thread.CurrentThread==dispatcherRender.Thread;
					manualResetEventAborted.Set();
					isRenderThreadHeld=manualResetEventCallerDone.WaitOne(20000);
				},TaskContinuationOptions.ExecuteSynchronously);
				toothChartRenderer.Shutdown(0);//the shutdown runs before the caller's job, and aborts it
				manualResetEventRelease.Set();//the holding job ends
				isCallerHeld=manualResetEventAborted.WaitOne(20000);
				statusWhenCallerGoesOn=e.Operation.Status;
			};
			Exception exceptionCaller=null;
			isArmed=true;
			Stopwatch stopwatch=Stopwatch.StartNew();
			try {
				toothChartRenderer.GetCountMeshSets();//queues a job and waits for it
			}
			catch(Exception ex) {
				exceptionCaller=ex;
			}
			long millisecondsCaller=stopwatch.ElapsedMilliseconds;
			manualResetEventCallerDone.Set();//the render thread raises the job's Aborted now, after its caller closed the wait
			bool isEnded=dispatcherRender.Thread.Join(30000);
			Console.WriteLine("caller held until the abort: "+isCallerHeld+" (job "+statusWhenCallerGoesOn+" before the caller added its handler)");
			Console.WriteLine("render thread held inside the abort: "+isHeldOnRenderThread);
			Console.WriteLine("render thread held until the caller returned: "+isRenderThreadHeld);
			Console.WriteLine("caller: "+(exceptionCaller==null?"no exception":exceptionCaller.GetType().Name+": "+exceptionCaller.Message));
			Console.WriteLine("caller heard at once: "+(millisecondsCaller<10000)+" ("+millisecondsCaller+" ms)");
			Console.WriteLine("old render thread ended: "+isEnded);
			Console.WriteLine("unhandled exceptions: "+ToothChartRenderer.CountUnhandledExceptions);
			Console.WriteLine("log lines: "+ToothChartWrapper.GetCountFallbackLinesWritten());
			ToothChartRenderResult resultNext=toothChartRenderer.Render(toothScene,null);
			Console.WriteLine("next render: "+resultNext.Size.Width+"x"+resultNext.Size.Height+", on a new thread "
				+(resultNext.ThreadIdRender!=dispatcherRender.Thread.ManagedThreadId));
		}

		///<summary>"shutdown-throws": an error raised during the render Dispatcher's own shutdown, outside every job (here a ShutdownFinished
		///handler that throws), leaves Dispatcher.Run, where Dispatcher.UnhandledException never sees it. The render thread's last guard catches it
		///and hands one line to the log; unguarded, the unhandled exception on the render thread ended this process. The next render starts a new
		///thread.</summary>
		private static void RunShutdownThrows() {
			ODBuild.IsUnitTest=true;
			ToothChartRenderer toothChartRenderer=ToothChartRenderer.Instance;
			ToothScene toothScene=WpfCharts.BuildScene("restored-adult",ChartScenes.SizeModule);
			Dispatcher dispatcherRender=null;
			ToothChartRenderer.ActionJobStartForTests=kind => {
				if(kind!=ToothChartRenderer.JobRender || dispatcherRender!=null) {
					return;
				}
				dispatcherRender=Dispatcher.CurrentDispatcher;
				dispatcherRender.ShutdownFinished+=(sender,e) => throw new InvalidOperationException("A forced error in the Dispatcher's own shutdown.");
			};
			ToothChartRenderResult resultFirst=toothChartRenderer.Render(toothScene,null);
			ToothChartRenderer.ActionJobStartForTests=null;
			int countMessages=ToothChartWrapper.GetCountFallbackMessages();
			toothChartRenderer.Shutdown(30000);
			Console.WriteLine("old render thread ended: "+dispatcherRender.Thread.Join(30000));
			Console.WriteLine("unhandled exceptions: "+ToothChartRenderer.CountUnhandledExceptions);
			Console.WriteLine("log lines: "+ToothChartWrapper.GetCountFallbackLinesWritten());
			foreach(string message in ToothChartWrapper.GetFallbackMessagesSince(countMessages)) {
				Console.WriteLine("message: "+message);
			}
			ToothChartRenderResult resultNext=toothChartRenderer.Render(toothScene,null);
			Console.WriteLine("next render: "+resultNext.Size.Width+"x"+resultNext.Size.Height+", on a new thread "
				+(resultNext.ThreadIdRender!=resultFirst.ThreadIdRender)+", the same image "+resultNext.PixelsBgra.SequenceEqual(resultFirst.PixelsBgra));
		}
	}
}
