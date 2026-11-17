// Package S3a-1b of the .NET 10 plan (docs/specs/net10-plan.md, the S3a-1 row and its risk item; docs/port-notes/S3a-1.md section 8): the
// cancellation that a progress window's action carries on its thread (CodeBase ODCancellation) without a window. The Middle Tier client sends no
// request from a cancelled thread; ODThread.RunParallel shares the token with its pool threads, starts no further action after a cancel and then
// throws on the caller, in all three of its paths, unless the caller runs a unit that a cancel must not split (RunUncancellable); an exception that
// an ODThread throws after a timed-out Join is logged instead of dropped; GetTimeElapsed of a thread still running after a timed-out Join counts
// to now. No program window is shown; the Middle Tier test uses the mock server of ScratchDb.cs.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace MiddleTierMockTests {
	///<summary>The Middle Tier client on a cancelled thread.</summary>
	[TestClass]
	public class CancellationMiddleTierTests {
		[TestInitialize]
		public void Initialize() {
			ScratchDb.BeginTest();
			ScratchDb.UseMockMiddleTier(ScratchDb.NewMockServer());
		}

		[TestCleanup]
		public void Cleanup() {
			ScratchDb.EndTest();
		}

		///<summary>A Middle Tier client sends nothing from the thread of a cancelled progress action (so the server writes nothing for it either):
		///the call throws OperationCanceledException before the request. The same call works on the thread before the cancel and on every other
		///thread after it, and inside a unit that a cancel must not split.</summary>
		[TestMethod]
		public void MiddleTierClient_CancelledThread_SendsNoRequest() {
			using(CancellationTokenSource cancellationTokenSource=new CancellationTokenSource()) {
				using(ODCancellation.Use(cancellationTokenSource.Token)) {
					Assert.AreEqual("Processed: before",WebServiceTests.GetString("before"),"Before the cancel the call goes through.");
					cancellationTokenSource.Cancel();
					Assert.ThrowsExactly<OperationCanceledException>(() => WebServiceTests.GetString("after"));
					string resultInUnit=null;
					ODCancellation.RunUncancellable(() => resultInUnit=WebServiceTests.GetString("unit"));
					Assert.AreEqual("Processed: unit",resultInUnit,"A unit under way finishes.");
				}
				Assert.AreEqual("Processed: other",WebServiceTests.GetString("other"),"Outside the cancelled action nothing changes.");
			}
		}
	}

	///<summary>RunParallel, ODThread logging and timing with the cancellation of the calling thread.</summary>
	[TestClass]
	public class CancellationODThreadTests {
		///<summary>RunParallel on a cancelled caller: the pool threads have the caller's token (their database writes stop as the caller's do), start
		///no further action after the cancel, and RunParallel then throws OperationCanceledException on the caller. Improved, legacy and
		///one-thread-on-the-caller paths.</summary>
		[TestMethod]
		public void RunParallel_CancelledCaller_PoolSharesTheToken_StartsNoFurtherAction_Throws() {
			RunParallelCancelled(isLegacy:false,numThreads:2,doRunOnCurrentThread:false);
			RunParallelCancelled(isLegacy:true,numThreads:2,doRunOnCurrentThread:false);
			RunParallelCancelled(isLegacy:false,numThreads:1,doRunOnCurrentThread:true);
		}

		private void RunParallelCancelled(bool isLegacy,int numThreads,bool doRunOnCurrentThread) {
			string label="isLegacy="+isLegacy+", numThreads="+numThreads+", onCurrentThread="+doRunOnCurrentThread+": ";
			using(CancellationTokenSource cancellationTokenSource=new CancellationTokenSource())
			using(ODCancellation.Use(cancellationTokenSource.Token)) {
				int countStarted=0;
				int countWithToken=0;
				int countStartedAtCancel=-1;
				List<Action> listActions=Enumerable.Range(0,200).Select(x => new Action(() => {
					int started=Interlocked.Increment(ref countStarted);
					if(ODCancellation.Current==cancellationTokenSource.Token) {
						Interlocked.Increment(ref countWithToken);
					}
					if(started==4) {
						cancellationTokenSource.Cancel();//as the user's Cancel would, from another thread
						Volatile.Write(ref countStartedAtCancel,Volatile.Read(ref countStarted));
					}
					Thread.Sleep(10);
				})).ToList();
				Assert.ThrowsExactly<OperationCanceledException>(() => ODThread.RunParallel(listActions,30000,numThreads,
					doRunOnCurrentThreadIf1Processor:doRunOnCurrentThread,isLegacy:isLegacy),label+"RunParallel throws on the caller after a cancel.");
				Thread.Sleep(200);
				Assert.IsTrue(countStartedAtCancel>=4,label+"started at the cancel: "+countStartedAtCancel);
				Assert.IsTrue(countStarted<=countStartedAtCancel+numThreads,label+"no further action after the cancel: started "+countStarted
					+", at the cancel "+countStartedAtCancel);
				Assert.AreEqual(countStarted,countWithToken,label+"every action ran with the caller's token.");
			}
		}

		///<summary>Inside a unit that a cancel must not split, RunParallel's pool gets no token: a cancel during it lets every action run, and nothing
		///is thrown. Without any token (every caller outside a progress window) RunParallel is unchanged.</summary>
		[TestMethod]
		public void RunParallel_InsideAUnit_OrWithoutAToken_RunsEveryAction() {
			using(CancellationTokenSource cancellationTokenSource=new CancellationTokenSource())
			using(ODCancellation.Use(cancellationTokenSource.Token)) {
				int countRun=0;
				List<Action> listActions=Enumerable.Range(0,40).Select(x => new Action(() => {
					if(Interlocked.Increment(ref countRun)==2) {
						cancellationTokenSource.Cancel();
					}
					Assert.IsFalse(ODCancellation.Current.CanBeCanceled,"The unit's pool has no token.");
				})).ToList();
				ODCancellation.RunUncancellable(() => ODThread.RunParallel(listActions,30000,numThreads:3));
				Assert.AreEqual(40,countRun);
				Assert.ThrowsExactly<OperationCanceledException>(() => ODCancellation.ThrowIfCancellationRequested(),"After the unit the check stops the action.");
			}
			int countRunNoToken=0;
			ODThread.RunParallel(Enumerable.Range(0,40).Select(x => new Action(() => Interlocked.Increment(ref countRunNoToken))).ToList(),30000,numThreads:3);
			Assert.AreEqual(40,countRunNoToken);
			Assert.IsFalse(ODCancellation.Current.CanBeCanceled,"Use puts back the thread's earlier token (none).");
		}

		///<summary>An exception that an ODThread throws after its Join timed out reaches no handler (nobody waits for the thread any more), as before,
		///but it is logged now instead of dropped silently. Meanwhile GetTimeElapsed of the thread, still running though asked to quit, counts to
		///now (it used to subtract the start from DateTime.MinValue).</summary>
		[TestMethod]
		public void ODThread_ExceptionAfterTimedOutJoin_IsLogged_GetTimeElapsedCountsToNow() {
			ConcurrentQueue<string> queueLines=new ConcurrentQueue<string>();
			Action<string> actionLogged=line => queueLines.Enqueue(line);
			ODCancellation.LineLogged+=actionLogged;
			try {
				ManualResetEventSlim manualResetEventStarted=new ManualResetEventSlim(false);
				ManualResetEventSlim manualResetEventGo=new ManualResetEventSlim(false);
				ManualResetEventSlim manualResetEventLogged=new ManualResetEventSlim(false);
				bool hasHandlerRun=false;
				ODThread thread=new ODThread(o => {
					manualResetEventStarted.Set();
					manualResetEventGo.Wait(TimeSpan.FromSeconds(30));
					throw new InvalidOperationException("Synthetic failure after the timed-out Join.");
				});
				thread.AddExceptionHandler(ex => hasHandlerRun=true);
				string threadName=MethodBase.GetCurrentMethod().Name;
				thread.Name=threadName;
				thread.Start();
				Assert.IsTrue(manualResetEventStarted.Wait(TimeSpan.FromSeconds(30)));
				Assert.IsFalse(thread.Join(10));
				TimeSpan timeSpanElapsed1=thread.GetTimeElapsed();
				Thread.Sleep(50);
				TimeSpan timeSpanElapsed2=thread.GetTimeElapsed();
				Assert.IsTrue(timeSpanElapsed1>=TimeSpan.Zero && timeSpanElapsed1<TimeSpan.FromMinutes(5),"elapsed: "+timeSpanElapsed1);
				Assert.IsTrue(timeSpanElapsed2>timeSpanElapsed1,"A thread still running counts to now: "+timeSpanElapsed1+", "+timeSpanElapsed2);
				manualResetEventGo.Set();
				Assert.IsTrue(thread.Join(30000),"The thread quits after its exception.");
				Assert.IsFalse(hasHandlerRun,"The handler is not called after the Join timed out (as before).");
				Assert.IsTrue(queueLines.Any(x => x.Contains(threadName) && x.Contains("Synthetic failure after the timed-out Join.")),
					"The exception was logged: "+string.Join(" | ",queueLines));
				TimeSpan timeSpanFinished=thread.GetTimeElapsed();
				Thread.Sleep(50);
				Assert.AreEqual(timeSpanFinished,thread.GetTimeElapsed(),"A finished thread's time stays.");
			}
			finally {
				ODCancellation.LineLogged-=actionLogged;
			}
		}
	}
}
