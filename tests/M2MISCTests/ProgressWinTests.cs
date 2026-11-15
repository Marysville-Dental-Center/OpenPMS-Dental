// Package S3a-1 of the .NET 10 plan (docs/specs/net10-plan.md; no more Thread.Abort): ProgressWin and its window FrmProgressAuto shown on an STA
// thread of the test, as the program shows them, for success, an exception and a cancel. Cancel no longer aborts the worker thread (Thread.Abort
// throws PlatformNotSupportedException on .NET 5 and later): the window closes as cancelled, the action is asked to stop (ProgressWin's token,
// ActionCancel, and a progress event after the cancel), and an action that does not stop runs on in the background with its result dropped.
// The cancel is the X of the window (FormFrame.Close from a timer on the window's thread), the same path as the Cancel button without a
// MessageCancel. No database is used; the small windows open and close by themselves.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;

namespace M2MISCTests {
	[TestClass]
	public class ProgressWinTests {
		///<summary>Bound for every wait in these tests, so that a fault fails the test instead of hanging the run.</summary>
		private static readonly TimeSpan _timeSpanWait=TimeSpan.FromSeconds(30);

		#region Helpers
		///<summary>Runs the action on a new STA thread (WPF and WinForms windows need one) and waits for it. What the action throws is thrown again here.
		///</summary>
		private static void RunSta(Action action) {
			Exception exception=null;
			Thread thread=new Thread(() => {
				try {
					action();
				}
				catch(Exception ex) {
					exception=ex;
				}
				finally {
					System.Windows.Threading.Dispatcher dispatcher=System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread);
					if(dispatcher!=null && !dispatcher.HasShutdownStarted) {
						ODException.SwallowAnyException(() => dispatcher.InvokeShutdown());
					}
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			Assert.IsTrue(thread.Join(TimeSpan.FromMinutes(2)),"The STA thread of the test ended.");
			if(exception is UnitTestAssertException) {
				throw exception;
			}
			if(exception!=null) {
				throw new AssertFailedException("Failed on the STA thread: "+exception,exception);
			}
		}

		///<summary>Shows progressWin on this (STA) thread. Once funcIsTimeToClose returns true, the progress window is closed the way the user's X
		///closes it, from a timer on this thread (so inside the window's modal loop). Returns the exception ShowDialog threw, or null.</summary>
		private static Exception ShowAndCloseWhen(ProgressWin progressWin,Func<FrmProgressAutoView,bool> funcIsTimeToClose) {
			System.Windows.Forms.Timer timer=null;
			if(funcIsTimeToClose!=null) {
				timer=new System.Windows.Forms.Timer();
				timer.Interval=20;
				timer.Tick+=(sender,e) => {
					FormFrame formFrame=System.Windows.Forms.Application.OpenForms.OfType<FormFrame>().FirstOrDefault(x => !x.InvokeRequired);//this thread's
					if(formFrame==null) {
						return;
					}
					FrmProgressAutoView frmProgressAutoView=new FrmProgressAutoView(formFrame);
					if(!funcIsTimeToClose(frmProgressAutoView)) {
						return;
					}
					timer.Stop();
					formFrame.Close();
				};
				timer.Start();
			}
			try {
				progressWin.ShowDialog();
				return null;
			}
			catch(Exception ex) {
				return ex;
			}
			finally {
				timer?.Stop();
				timer?.Dispose();
			}
		}

		///<summary>What the tests read of the open progress window.</summary>
		private class FrmProgressAutoView {
			private readonly WpfControls.UI.FrmProgressAuto _frmProgressAuto;

			public FrmProgressAutoView(FormFrame formFrame) {
				//FormFrame.ElementHostUI.Child, read by reflection so that this project needs no WindowsFormsIntegration reference.
				object elementHost=typeof(FormFrame).GetField("ElementHostUI").GetValue(formFrame);
				_frmProgressAuto=elementHost?.GetType().GetProperty("Child").GetValue(elementHost) as WpfControls.UI.FrmProgressAuto;
			}

			///<summary>True once the window shows "Done" in history mode: the progress bar is hidden.</summary>
			public bool IsDoneShown {
				get {
					System.Windows.UIElement uIElement=_frmProgressAuto?.FindName("progressBar") as System.Windows.UIElement;
					return uIElement!=null && uIElement.Visibility==System.Windows.Visibility.Collapsed;
				}
			}
		}
		#endregion Helpers

		[TestMethod]
		public void ProgressWin_Success_ClosesByItself_IsSuccess() {
			RunSta(() => {
				int result=0;
				ProgressWin progressWin=new ProgressWin();
				progressWin.ActionMain=() => {
					Thread.Sleep(50);
					result=42;
				};
				Exception exception=ShowAndCloseWhen(progressWin,null);
				Assert.IsNull(exception);
				Assert.AreEqual(42,result,"The action ran on the worker thread.");
				Assert.IsTrue(progressWin.IsSuccess);
				Assert.IsFalse(progressWin.IsCancelled);
				Assert.IsFalse(progressWin.IsWorkerStillRunning);
				Assert.IsFalse(progressWin.CancellationToken.IsCancellationRequested);
			});
		}

		[TestMethod]
		public void ProgressWin_Exception_ClosesAndRethrows() {
			RunSta(() => {
				ProgressWin progressWin=new ProgressWin();
				progressWin.ActionMain=() => throw new InvalidOperationException("The action failed.");
				Exception exception=ShowAndCloseWhen(progressWin,null);
				Assert.IsInstanceOfType(exception,typeof(InvalidOperationException),"ShowDialog throws the action's exception: "+exception);
				Assert.AreEqual("The action failed.",exception.Message);
				Assert.IsFalse(progressWin.IsSuccess);
				Assert.IsFalse(progressWin.IsCancelled);
				Assert.IsFalse(progressWin.IsWorkerStillRunning);
				//An exception with an inner exception: the inner one is thrown (methods invoked by reflection), as before.
				ProgressWin progressWinInner=new ProgressWin();
				progressWinInner.ActionMain=() => throw new ApplicationException("outer",new FormatException("inner"));
				Exception exceptionInner=ShowAndCloseWhen(progressWinInner,null);
				Assert.IsInstanceOfType(exceptionInner,typeof(FormatException));
				Assert.AreEqual("inner",exceptionInner.Message);
			});
		}

		///<summary>The action checks ProgressWin's token: after the cancel it stops and writes nothing more. ActionCancel runs once.</summary>
		[TestMethod]
		public void ProgressWin_Cancel_TheTokenStopsTheAction_NoWriteAfterTheCancel() {
			RunSta(() => {
				ManualResetEventSlim manualResetEventStarted=new ManualResetEventSlim(false);
				ManualResetEventSlim manualResetEventEnded=new ManualResetEventSlim(false);
				int countWrites=0;
				int countWritesAtCancel=-1;
				int countActionCancel=0;
				ProgressWin progressWin=new ProgressWin();
				progressWin.ActionCancel=() => {
					countActionCancel++;
					countWritesAtCancel=Volatile.Read(ref countWrites);
				};
				progressWin.ActionMain=() => {
					try {
						manualResetEventStarted.Set();
						DateTime dateTimeEnd=DateTime.Now+_timeSpanWait;
						while(DateTime.Now<dateTimeEnd) {
							progressWin.CancellationToken.ThrowIfCancellationRequested();
							Interlocked.Increment(ref countWrites);//stands for one write of a long action
							Thread.Sleep(5);
						}
					}
					finally {
						manualResetEventEnded.Set();
					}
				};
				Exception exception=ShowAndCloseWhen(progressWin,x => manualResetEventStarted.IsSet && Volatile.Read(ref countWrites)>3);
				Assert.IsNull(exception,"A cancelled action's OperationCanceledException is not thrown to the caller: "+exception);
				Assert.IsTrue(progressWin.IsCancelled);
				Assert.IsFalse(progressWin.IsSuccess);
				Assert.IsFalse(progressWin.IsWorkerStillRunning,"The action stopped within the window's wait.");
				Assert.IsTrue(manualResetEventEnded.IsSet,"The action had ended when ShowDialog returned.");
				Assert.IsTrue(progressWin.CancellationToken.IsCancellationRequested);
				Assert.AreEqual(1,countActionCancel,"ActionCancel ran once.");
				//At most the write that was under way when the user cancelled; none after.
				Assert.IsTrue(countWrites<=countWritesAtCancel+1,"writes: "+countWrites+", at the cancel: "+countWritesAtCancel);
			});
		}

		///<summary>The action does not check the token but reports progress: its next progress event after the cancel ends it (in place of the old
		///abort), and the window shows no more progress.</summary>
		[TestMethod]
		public void ProgressWin_Cancel_TheNextProgressEventStopsTheAction() {
			RunSta(() => {
				ManualResetEventSlim manualResetEventEnded=new ManualResetEventSlim(false);
				int countSteps=0;
				int countStepsAtCancel=-1;
				Exception exceptionInAction=null;
				ProgressWin progressWin=new ProgressWin();
				progressWin.ActionCancel=() => countStepsAtCancel=Volatile.Read(ref countSteps);
				progressWin.ActionMain=() => {
					try {
						DateTime dateTimeEnd=DateTime.Now+_timeSpanWait;
						while(DateTime.Now<dateTimeEnd) {
							ODEvent.Fire(ODEventType.ProgressBar,"Step "+countSteps);
							Interlocked.Increment(ref countSteps);//stands for the step's write
							Thread.Sleep(5);
						}
					}
					catch(Exception ex) {
						exceptionInAction=ex;
						throw;
					}
					finally {
						manualResetEventEnded.Set();
					}
				};
				Exception exception=ShowAndCloseWhen(progressWin,x => Volatile.Read(ref countSteps)>3);
				Assert.IsNull(exception,"Nothing is thrown to the caller: "+exception);
				Assert.IsTrue(progressWin.IsCancelled);
				Assert.IsFalse(progressWin.IsWorkerStillRunning);
				Assert.IsTrue(manualResetEventEnded.IsSet);
				Assert.IsInstanceOfType(exceptionInAction,typeof(OperationCanceledException),"The progress event after the cancel threw.");
				Assert.IsTrue(countSteps<=countStepsAtCancel+1,"steps: "+countSteps+", at the cancel: "+countStepsAtCancel);
				//A progress event from another thread after the cancel is ignored, not thrown.
				ODEvent.Fire(ODEventType.ProgressBar,"From another thread");
			});
		}

		///<summary>An action that neither checks the token nor reports progress: the window still closes as cancelled after its bounded wait,
		///IsWorkerStillRunning says so, and what the action does and throws afterwards is dropped (no exception anywhere, the outcome stays
		///cancelled).</summary>
		[TestMethod]
		public void ProgressWin_Cancel_AnActionThatDoesNotStop_ClosesAnyway_ItsOutcomeIsDropped() {
			ManualResetEventSlim manualResetEventStarted=new ManualResetEventSlim(false);
			ManualResetEventSlim manualResetEventRelease=new ManualResetEventSlim(false);
			ManualResetEventSlim manualResetEventEnded=new ManualResetEventSlim(false);
			ProgressWin progressWin=new ProgressWin();
			RunSta(() => {
				progressWin.ActionMain=() => {
					try {
						manualResetEventStarted.Set();
						manualResetEventRelease.Wait(_timeSpanWait);//does not look at the token
						throw new InvalidOperationException("Thrown after the cancel.");
					}
					finally {
						manualResetEventEnded.Set();
					}
				};
				DateTime dateTimeStart=DateTime.Now;
				Exception exception=ShowAndCloseWhen(progressWin,x => manualResetEventStarted.IsSet);
				TimeSpan timeSpanShown=DateTime.Now-dateTimeStart;
				Assert.IsNull(exception);
				Assert.IsTrue(progressWin.IsCancelled);
				Assert.IsFalse(progressWin.IsSuccess);
				Assert.IsTrue(progressWin.IsWorkerStillRunning,"The action had not stopped within the window's wait.");
				Assert.IsFalse(manualResetEventEnded.IsSet,"The action was not stopped by force.");
				Assert.IsTrue(timeSpanShown<TimeSpan.FromSeconds(15),"The window closed after its bounded wait: "+timeSpanShown);
				//The action goes on and fails after the window closed. The window's thread keeps pumping meanwhile, as the program's main thread does.
				manualResetEventRelease.Set();
				DateTime dateTimeEnd=DateTime.Now+_timeSpanWait;
				while(!manualResetEventEnded.IsSet && DateTime.Now<dateTimeEnd) {
					FrmODBase.DoEvents();
					System.Windows.Forms.Application.DoEvents();
					Thread.Sleep(10);
				}
				Assert.IsTrue(manualResetEventEnded.IsSet,"The action ended on its own.");
				for(int i=0;i<20;i++) {//time for a late report that the window must not take (it would have to come through this thread)
					FrmODBase.DoEvents();
					System.Windows.Forms.Application.DoEvents();
					Thread.Sleep(10);
				}
			});
			Assert.IsTrue(progressWin.IsCancelled,"The outcome stays cancelled.");
			Assert.IsFalse(progressWin.IsSuccess);
		}

		///<summary>History mode: the window stays open with Done after the action, and closing it then is success, not a cancel (the window no
		///longer tells the two apart by the worker thread's IsAlive).</summary>
		[TestMethod]
		public void ProgressWin_History_StaysOpenUntilClosed_IsSuccess() {
			RunSta(() => {
				ProgressWin progressWin=new ProgressWin();
				progressWin.HasHistory=true;
				progressWin.ActionMain=() => {
					ODEvent.Fire(ODEventType.ProgressBar,"First step");
					ODEvent.Fire(ODEventType.ProgressBar,"Second step");
				};
				Exception exception=ShowAndCloseWhen(progressWin,x => x.IsDoneShown);
				Assert.IsNull(exception);
				Assert.IsTrue(progressWin.IsSuccess);
				Assert.IsFalse(progressWin.IsCancelled);
				Assert.IsFalse(progressWin.CancellationToken.IsCancellationRequested,"Closing after Done is not a cancel.");
				StringAssert.Contains(progressWin.HistoryMsg,"First step");
				StringAssert.Contains(progressWin.HistoryMsg,"Second step");
			});
		}

		///<summary>No Thread.Abort is left in the progress window, ProgressWin or ODThread (it throws PlatformNotSupportedException on .NET 5 and
		///later); the window still refuses to close while the action runs without a Cancel button (ImagingTests' Window_WritesCannotBeCancelledPartWay
		///relies on that).</summary>
		[TestMethod]
		public void Source_NoThreadAbort_InTheProgressWindowOrODThread() {
			string frmProgressAuto=TestDb.ReadRepoFile("WpfControlsOD","UI","Progress","FrmProgressAuto.xaml.cs");
			string progressWin=TestDb.ReadRepoFile("WpfControlsOD","UI","Progress","ProgressWin.cs");
			string odThread=TestDb.ReadRepoFile("CodeBase","Utilities","ODThread.cs");
			foreach(string code in new[] { frmProgressAuto,progressWin,odThread }) {
				Assert.IsFalse(Regex.IsMatch(code,@"\.Abort\(\)"));
				Assert.IsFalse(code.Contains("ResetAbort"));
			}
			Assert.IsTrue(Regex.IsMatch(frmProgressAuto,@"if\(!ShowCancelButton\) \{//tried to close with X[^\n]*\n\s*MessageBox\.Show\(StopNotAllowedMessage\);[^\n]*\n\s*e\.Cancel=true;"));
		}
	}
}
