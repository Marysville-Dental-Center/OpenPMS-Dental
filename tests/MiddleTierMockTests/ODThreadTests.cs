// The ODThread tests of the legacy UnitTests project (UnitTests/UnitTests/ODThreadTests.cs: the 7 active tests; the two commented-out timing
// tests are not ported), moved here by package S0-T of the .NET 10 plan. Each test keeps its name and its arrange, act and assert; only the
// harness changed: the scratch database of ScratchDb.cs instead of the legacy unittest database and TestBase, and the two SetDb tests connect
// their thread to a second scratch database whose name starts with s0t_ (the legacy test named it after the test method) on 127.0.0.1.
// Package S3a-1 (no more Thread.Abort) rewrote the two tests that relied on Join aborting a thread, ODThread_RunParallel_IsSynchronous and
// ODThread_CatchThreadAbortException, for cooperative cancellation (they keep their names), and added the ODThread_JoinTimeout_ and
// ODThread_RunParallel_Timeout tests: a Join that times out returns false, asks the thread to quit and lets it finish on its own.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace MiddleTierMockTests {
	[TestClass]
	public class ODThreadTests {

		///<summary>This method will execute only once, just before any tests in this class run.</summary>
		[ClassInitialize]
		public static void SetupClass(TestContext testContext) {
		}

		///<summary>This method will execute just before each test in this class.</summary>
		[TestInitialize]
		public void SetupTest() {
			ScratchDb.BeginTest();
			ScratchDb.UseDirectConnection();//Run against direct connection as these tests have many queries ran and are not relevant to middle tier.
		}

		///<summary>This method will execute after each test in this class.</summary>
		[TestCleanup]
		public void TearDownTest() {
			//Some weird database connection stuff happens in this class so set the main db connection back to the scratch database.
			ScratchDb.EndTest();
		}

		///<summary>The legacy TestBase.CreateDatabaseIfNeeded: creates a database with a preference table so that DataConnection can pass its test
		///query. Only for this run's s0t_ databases.</summary>
		private static void CreateDatabaseIfNeeded(string databaseName) {
			if(!databaseName.StartsWith(ScratchDb.PREFIX)) {
				throw new ArgumentException("Tests only create databases whose name starts with "+ScratchDb.PREFIX+".");
			}
			string command=$"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{POut.String(databaseName)}'";
			DataTable table=DataCore.GetTable(command);
			//Create a custom database if needed.
			if(table.Rows.Count==0) {
				DataCore.NonQ($"CREATE DATABASE `{POut.String(databaseName)}` CHARACTER SET utf8");
				DataCore.NonQ($@"CREATE TABLE `{POut.String(databaseName)}`.`preference` (
						`PrefName` varchar(255) NOT NULL DEFAULT '',
						`ValueString` text NOT NULL,
						`PrefNum` bigint(20) NOT NULL AUTO_INCREMENT,
						`Comments` text,
						PRIMARY KEY (`PrefNum`)
					) ENGINE=MyISAM AUTO_INCREMENT=1 DEFAULT CHARSET=utf8");
			}
		}

		///<summary>The legacy TestBase.DropDatabase, for this run's s0t_ databases only.</summary>
		private static void DropDatabase(string databaseName) {
			ODException.SwallowAnyException(() => ScratchDb.Drop(databaseName));
		}

		///<summary>This method will execute only once, just after all tests in this class have run.
		///However, this method is not guaranteed to execute before starting another TestMethod from another TestClass.</summary>
		[ClassCleanup]
		public static void TearDownClass() {
		}

		[TestMethod]
		///<summary>The ThreadStatic database context should be inherited by any child thread that is spawned.</summary>
		public void ODThread_SetDbT_ParentChildDatabaseContext() {
			Exception ex=null;
			string methodName=MethodBase.GetCurrentMethod().Name;
			string databaseName=ScratchDb.Name("odthread_setdbt");//The legacy test used POut.String(methodName).ToLower().
			string odThreadGroupName=methodName+"_GroupName";
			//The main thread needs to connect to a unique database that the child thread should NOT end up getting connected to.
			//Use a query to get our current database context because unit tests utilizes a connection string and we only care about DataConnection.Database
			string databaseMain=LargeTableHelper.GetCurrentDatabase();//should be pointing to the s0t_ scratch database
			Assert.IsFalse(string.IsNullOrEmpty(databaseMain));
			try {
				ODThread odThreadParent=new ODThread(workerParent => {
					//The parent thread needs to connect to a database that the main thread is not connected to.
					CreateDatabaseIfNeeded(databaseName);
					new DataConnection().SetDbT(ScratchDb.Server,databaseName,"root","","","",DatabaseType.MySql,DevDbTls.GetSslCa(ScratchDb.Server));
					VerifyCurrentDatabaseName(databaseName);
					//Now the parent should spawn a child thread which should default to the thread specific database connection from the parent.
					ODThread odThreadChild=new ODThread(workerChild => {
						//The child thread should default to the database context of its parent and NOT the database context from the main thread.
						VerifyCurrentDatabaseName(databaseName);
					});
					odThreadChild.AddExceptionHandler(e => ex=e);
					odThreadChild.Name=MethodBase.GetCurrentMethod().Name+"_Child";
					odThreadChild.GroupName=odThreadGroupName;
					odThreadChild.Start();
					odThreadChild.Join(Timeout.Infinite);
					VerifyCurrentDatabaseName(databaseName);
				});
				odThreadParent.AddExceptionHandler(e => ex=e);
				odThreadParent.Name=MethodBase.GetCurrentMethod().Name+"_Parent";
				odThreadParent.GroupName=odThreadGroupName;
				odThreadParent.Start();
				odThreadParent.Join(Timeout.Infinite);//Manually join the parent to the main thread so that we know for certain that our child thread exists.
				VerifyCurrentDatabaseName(databaseMain);
			}
			catch(Exception e) {
				ex=e;
			}
			finally {
				DropDatabase(databaseName);
			}
			Assert.IsNull(ex,ex?.Message);//Asserts do not bubble up as expected to the main thread so do a manual assert at the end of the day.
		}

		[TestMethod]
		///<summary>The following test calls setDb within a child thread. This will change the static database connection for all threads. The 
		///parent thread, or any other thread already alive, should not have their context changed.</summary>
		public void ODThread_SetDb_ChildContext() {
			Exception ex=null;
			string methodName=MethodBase.GetCurrentMethod().Name;
			string databaseName=ScratchDb.Name("odthread_setdb");//The legacy test used POut.String(methodName).ToLower().
			string odThreadGroupName=methodName+"_GroupName";
			string databaseMain=LargeTableHelper.GetCurrentDatabase();//should be pointing to the s0t_ scratch database
			Assert.IsFalse(string.IsNullOrEmpty(databaseMain));
			try {
				ODThread odThreadAffect=new ODThread((o) => {
					string databaseBefore=LargeTableHelper.GetCurrentDatabase();//should be pointing to the s0t_ scratch database
					Assert.AreEqual(databaseMain,databaseBefore);
					ODThread odThreadChangeDb=new ODThread(workerChild => {
						//The parent thread needs to call SetDb on a different database than the main thread is connected to.
						CreateDatabaseIfNeeded(databaseName);
						new DataConnection().SetDb(ScratchDb.Server,databaseName,"root","","","",DatabaseType.MySql,DevDbTls.GetSslCa(ScratchDb.Server));
						VerifyCurrentDatabaseName(databaseName);
					});
					odThreadChangeDb.AddExceptionHandler(e => ex=e);
					odThreadChangeDb.Name=MethodBase.GetCurrentMethod().Name+"_Child";
					odThreadChangeDb.GroupName=odThreadGroupName;
					//This thread needs to only start the above thread and then make sure it is still connected to the s0t_ scratch database when complete.
					odThreadChangeDb.Start();
					odThreadChangeDb.Join(Timeout.Infinite);//Manually join the parent to the main thread so that we know for certain that our child thread exists.
					VerifyCurrentDatabaseName(databaseMain);
				});
				odThreadAffect.AddExceptionHandler(e => ex=e);
				odThreadAffect.Name=MethodBase.GetCurrentMethod().Name+"_ThreadAffected";
				odThreadAffect.Start();
				odThreadAffect.Join(Timeout.Infinite);
				//The main thread should still be on the original database after this.
				VerifyCurrentDatabaseName(databaseMain);
				//Verify that new thread is not affected by the call to SetDb() from the child thread above.
				//You should only SetDb() once per application instance. 
				//After that, it does nothing to any thread (existing or subsequent) but the thread that calls it.
				ODThread odThreadChangeDb1= new ODThread(workerChild => {
					VerifyCurrentDatabaseName(databaseMain);
				});
				odThreadChangeDb1.AddExceptionHandler(e => ex=e);
				odThreadChangeDb1.Name=MethodBase.GetCurrentMethod().Name+"_Child";
				odThreadChangeDb1.GroupName=odThreadGroupName;
				odThreadChangeDb1.Start();
				odThreadChangeDb1.Join(Timeout.Infinite);//Manually join the parent to the main thread so that we know for certain that our child thread exists.				
			}
			catch(Exception e) {
				ex=e;
			}
			finally {
				DropDatabase(databaseName);
			}
			Assert.IsNull(ex,ex?.Message);//Asserts do not bubble up as expected to the main thread so do a manual assert at the end of the day.
		}

		private void VerifyCurrentDatabaseName(string expected) {
			Assert.AreEqual(expected,LargeTableHelper.GetCurrentDatabase());
		}

		[TestMethod]
		///<summary>Tests that RunParallel itself is a synchronous method call, meaning that any subsequent code will not execute until either the timeout
		///is reached or all actions are completed. S3a-1: the timeout no longer aborts the action's thread; RunParallel still returns at the timeout,
		///before the action finished, and the action then runs to its end on its own. Both the legacy and the improved path (the legacy test ran the
		///improved path twice).</summary>
		public void ODThread_RunParallel_IsSynchronous() {
			ODThread_RunParallel_IsSynchronous(isLegacy:true);
			ODThread_RunParallel_IsSynchronous(isLegacy:false);
		}

		private void ODThread_RunParallel_IsSynchronous(bool isLegacy) {
			//Long enough that the pool thread surely reaches the action before the timeout: a Join that times out first sets HasQuit, and then no
			//action starts at all (on a loaded machine starting the thread can take longer than a timer tick).
			int timeoutMS=1000;
			object testObject=null;
			ManualResetEventSlim manualResetEventActionStarted=new ManualResetEventSlim(false);
			ManualResetEventSlim manualResetEventRunParallelFinished=new ManualResetEventSlim(false);
			ManualResetEventSlim manualResetEventActionEnded=new ManualResetEventSlim(false);
			List<Action> listActions=new List<Action>();
			listActions.Add(() => {
				manualResetEventActionStarted.Set();
				try {
					//Wait here until ODThread.RunParallel finishes (bounded, so that a failing test cannot leave the thread behind for long).
					manualResetEventRunParallelFinished.Wait(TimeSpan.FromSeconds(30));
					testObject=new object();//Runs only after RunParallel returned.
				}
				finally {
					manualResetEventActionEnded.Set();
				}
			});
			int numThreads=4;//Mimic what is used in AccountModules.GetAll(...).
			ODThread.RunParallel(listActions,timeoutMS,numThreads,isLegacy:isLegacy);
			Assert.IsTrue(manualResetEventActionStarted.IsSet,"isLegacy="+isLegacy+": the action had not started when the "+timeoutMS+" ms timeout was "
				+"reached (the machine was too slow to start the thread); the test cannot check anything further.");
			Assert.IsNull(testObject,"isLegacy="+isLegacy+": RunParallel returned at the timeout, before the action finished.");
			manualResetEventRunParallelFinished.Set();
			//The thread was not aborted: the action finishes on its own.
			Assert.IsTrue(manualResetEventActionEnded.Wait(TimeSpan.FromSeconds(30)),"isLegacy="+isLegacy+": the action ended on its own.");
			Assert.IsNotNull(testObject,"isLegacy="+isLegacy+": the action ran to its end.");
		}

		[TestMethod]
		///<summary>ODThread should be able to catch all exceptions after a Join timed out. If this test fails, an exception is being thrown and
		///not being caught from within ODThread. S3a-1: Join no longer aborts the thread; a Join that times out wakes the thread and asks it to quit
		///(HasQuit), the worker stops cooperatively, and the exception it throws then is still caught quietly within ODThread, as the exception
		///after the old abort was.</summary>
		public void ODThread_CatchThreadAbortException() {
			bool result=true;
			ManualResetEventSlim manualResetEventStarted=new ManualResetEventSlim(false);
			ODThread thread=new ODThread(o => {
				manualResetEventStarted.Set();
				try {
					o.Wait((int)TimeSpan.FromMinutes(10).TotalMilliseconds);//The Join that times out wakes this wait.
					if(o.HasQuit) {
						throw new OperationCanceledException("The thread was asked to quit.");
					}
				}
				catch(Exception ex) {
					//The cancellation comes in here. Because of some code in this exception, a different type of exception is thrown.
					ex.DoNothing();
					throw new Exception("Exception that is not a ThreadAbortException");
				}
			});
			thread.AddExceptionHandler(ex => {
				//If the exception handler is every reached, the exception after the timed-out Join was not caught correctly.
				result=false;
			});
			thread.Start();
			Assert.IsTrue(manualResetEventStarted.Wait(TimeSpan.FromSeconds(30)),"Give the thread a chance to start.");
			Assert.IsFalse(thread.Join(0),"Join times out at once and returns false; it does not throw.");
			Assert.IsTrue(thread.HasQuit,"The timed-out Join asked the thread to quit.");
			Assert.IsTrue(thread.Join(30000),"Now actually join and wait for the thread to finish: it stopped on its own.");
			Assert.IsTrue(result);
		}

		///<summary>S3a-1: a Join that times out returns false and does not stop the thread: the thread runs on, sees HasQuit, becomes a background
		///thread (so it cannot keep the program from exiting) and finishes on its own. Its exit handler does not fire, as after the old abort.</summary>
		[TestMethod]
		public void ODThread_JoinTimeout_ReturnsFalse_ThreadFinishesOnItsOwn() {
			ManualResetEventSlim manualResetEventStarted=new ManualResetEventSlim(false);
			ManualResetEventSlim manualResetEventGo=new ManualResetEventSlim(false);
			ManualResetEventSlim manualResetEventEnded=new ManualResetEventSlim(false);
			bool isBackgroundBefore=true;
			bool isBackgroundAfter=false;
			bool hasQuitSeen=false;
			bool hasExitHandlerRun=false;
			ODThread thread=new ODThread(o => {
				isBackgroundBefore=Thread.CurrentThread.IsBackground;
				manualResetEventStarted.Set();
				manualResetEventGo.Wait(TimeSpan.FromSeconds(30));
				isBackgroundAfter=Thread.CurrentThread.IsBackground;
				hasQuitSeen=o.HasQuit;
				manualResetEventEnded.Set();
			});
			thread.AddExitHandler(o => hasExitHandlerRun=true);
			thread.Name=MethodBase.GetCurrentMethod().Name;
			thread.Start();
			Assert.IsTrue(manualResetEventStarted.Wait(TimeSpan.FromSeconds(30)));
			Assert.IsFalse(thread.Join(10),"The timeout is reached while the worker runs.");
			Assert.IsFalse(manualResetEventEnded.IsSet,"The worker was not stopped by the Join.");
			manualResetEventGo.Set();
			Assert.IsTrue(manualResetEventEnded.Wait(TimeSpan.FromSeconds(30)),"The worker finished on its own.");
			Assert.IsTrue(thread.Join(30000));
			Assert.IsFalse(isBackgroundBefore,"An ODThread is a foreground thread until a Join times out.");
			Assert.IsTrue(isBackgroundAfter,"After the timed-out Join the thread is a background thread.");
			Assert.IsTrue(hasQuitSeen,"The timed-out Join asked the thread to quit.");
			Assert.IsFalse(hasExitHandlerRun,"The exit handler does not fire after a Join timed out.");
		}

		///<summary>S3a-1: a thread that runs on an interval and is waiting for its next run when a Join times out is woken up and quits, in place of
		///the old abort; it does not run its worker again.</summary>
		[TestMethod]
		public void ODThread_JoinTimeout_IntervalThreadQuits() {
			int countRuns=0;
			ManualResetEventSlim manualResetEventFirstRun=new ManualResetEventSlim(false);
			ODThread thread=new ODThread((int)TimeSpan.FromMinutes(10).TotalMilliseconds,o => {
				Interlocked.Increment(ref countRuns);
				manualResetEventFirstRun.Set();
			});
			thread.Name=MethodBase.GetCurrentMethod().Name;
			thread.Start();
			Assert.IsTrue(manualResetEventFirstRun.Wait(TimeSpan.FromSeconds(30)));
			Assert.IsFalse(thread.Join(50),"The thread waits ten minutes for its next run.");
			Assert.IsTrue(thread.Join(30000),"Woken up and asked to quit, the thread ends without waiting out its interval.");
			Assert.AreEqual(1,countRuns);
		}

		///<summary>S3a-1: when RunParallel's timeout is reached its threads are told to quit: the action each thread is in runs to its end, and no thread
		///starts another action (they used to be aborted, which also stopped the rest). One thread, two actions; the second never runs.</summary>
		[TestMethod]
		public void ODThread_RunParallel_TimeoutStartsNoFurtherAction() {
			ODThread_RunParallel_TimeoutStartsNoFurtherAction(isLegacy:true);
			ODThread_RunParallel_TimeoutStartsNoFurtherAction(isLegacy:false);
		}

		private void ODThread_RunParallel_TimeoutStartsNoFurtherAction(bool isLegacy) {
			//Long enough that the pool thread surely reaches the first action before the timeout (see ODThread_RunParallel_IsSynchronous).
			int timeoutMS=1000;
			ManualResetEventSlim manualResetEventFirstStarted=new ManualResetEventSlim(false);
			ManualResetEventSlim manualResetEventGo=new ManualResetEventSlim(false);
			ManualResetEventSlim manualResetEventFirstEnded=new ManualResetEventSlim(false);
			bool hasFirstRun=false;
			bool hasSecondRun=false;
			List<Action> listActions=new List<Action>();
			listActions.Add(() => {
				manualResetEventFirstStarted.Set();
				try {
					manualResetEventGo.Wait(TimeSpan.FromSeconds(30));
					hasFirstRun=true;
				}
				finally {
					manualResetEventFirstEnded.Set();
				}
			});
			listActions.Add(() => hasSecondRun=true);
			ODThread.RunParallel(listActions,timeoutMS,numThreads:1,isLegacy:isLegacy);
			Assert.IsTrue(manualResetEventFirstStarted.IsSet,"isLegacy="+isLegacy+": the first action had not started when the "+timeoutMS+" ms timeout "
				+"was reached (the machine was too slow to start the thread); the test cannot check anything further.");
			Assert.IsFalse(hasFirstRun,"isLegacy="+isLegacy+": RunParallel returned at the timeout.");
			manualResetEventGo.Set();
			Assert.IsTrue(manualResetEventFirstEnded.Wait(TimeSpan.FromSeconds(30)));
			Thread.Sleep(500);//Time in which a thread that went on would have started the second action.
			Assert.IsTrue(hasFirstRun,"isLegacy="+isLegacy+": the running action finished on its own.");
			Assert.IsFalse(hasSecondRun,"isLegacy="+isLegacy+": no action starts after the timeout.");
		}

		[TestMethod]
		///<summary>An exception should not cause an ODThread set to autocleanup to fail to clean itself up from the list of running ODThreads.</summary>
		public void ODThread_AutoCleanup_OnThreadAbortException() {
			//Arrange
			string method=MethodBase.GetCurrentMethod().Name;
			ODThread.RegisterForUnhandledExceptions(null,(ex,str) => { });//Stops exception from crashing the test.
			ODThread threadAutoCleanup=new ODThread((o) => {
				throw new Exception();
			});
			threadAutoCleanup.GroupName=method;
			threadAutoCleanup.Name="AutoCleanup";
			ODThread threadDirty=new ODThread((o) => {
				throw new Exception();
			});
			threadDirty.GroupName=method;
			threadDirty.Name="Dirty";
			//Act
			threadAutoCleanup.Start(isAutoCleanup:true);
			threadDirty.Start(isAutoCleanup:false);
			threadAutoCleanup.Join(5000);
			threadDirty.Join(5000);
			List<ODThread> listThreads=ODThread.GetThreadsByGroupName(method);
			//Assert
			Assert.AreEqual(1,listThreads.Count);
			Assert.AreEqual("Dirty",listThreads.First().Name);
			//Cleanup
			ODThread.RegisterForUnhandledExceptions(null,null);//Stops exception from crashing the test.
		}

		///<summary>Shows that all Actions passed to ODThread.RunParallel complete.</summary>
		[TestMethod]
		public void ODThread_RunParallel_AllActionsComplete() {
			ODThread_RunParallel_AllActionsComplete(isLegacy:true);			
			ODThread_RunParallel_AllActionsComplete(isLegacy:false);
		}

		///<summary>Shows that all Actions passed to ODThread.RunParallel complete.</summary>
		private void ODThread_RunParallel_AllActionsComplete(bool isLegacy) {
			//Arrange
			int numThreads=8;
			int countActions=1024;
			List<ParallelAction> listActions=new List<ParallelAction>();
			listActions=Enumerable.Range(0,countActions).Select(x => new ParallelAction(0)).ToList();
			//Act
			ODThread.RunParallel(listActions.Select(x => x.Action).ToList(),Timeout.InfiniteTimeSpan,numThreads,isLegacy:isLegacy);
			//Assert
			Assert.IsTrue(listActions.All(x => x.DidRun));
		}

		///<summary>Shows that all Actions passed to ODThread.RunParallel complete.</summary>
		[TestMethod]
		public void ODThread_RunParallel_IsParallel() {
			ODThread_RunParallel_IsParallel(isLegacy:true);			
			ODThread_RunParallel_IsParallel(isLegacy:false);
		}

		///<summary>Shows that all Actions passed to ODThread.RunParallel complete.</summary>
		private void ODThread_RunParallel_IsParallel(bool isLegacy) {
			//Arrange
			int numThreads=8;
			int countActions=8;
			List<ParallelAction> listActions=new List<ParallelAction>();
			object _lock=new object();
			bool areAllRunning=false;
			listActions=Enumerable.Range(0,countActions).Select(x => new ParallelAction(1
				,sleepWhile:() => {
					lock(_lock) {
						//Keep each action running until all actions have been started.  Then, all actions will release, allowing the threads to complete.
						areAllRunning=areAllRunning ? true : listActions.All(y => y.IsRunning);
						return !areAllRunning;
					}
				})
			).ToList();
			//Act
			ODThread.RunParallel(listActions.Select(x => x.Action).ToList(),50,numThreads,isLegacy:isLegacy);
			//Assert
			Assert.IsTrue(listActions.All(x => x.DidRun));//Will fail if timeout.
		}

		private class ParallelAction {
			public bool IsRunning;
			public bool DidRun;
			private int _sleepMs;
			private Func<bool> _sleepWhile = () => false;
			private object _lock=new object();

			public ParallelAction(int sleepMs,Func<bool> sleepWhile=null) {
				_sleepMs=sleepMs;
				_sleepWhile=sleepWhile??_sleepWhile;
			}

			public Action Action => () => {
				lock(_lock) {
					IsRunning=true;
					do {
						if(DidRun) {
							throw new Exception("Action was already run.");
						}
						Thread.Sleep(_sleepMs);
					}while(_sleepWhile());
					DidRun=true;
					IsRunning=false;
				}
			};
		}

	}
}
