// Package S3a-1b of the .NET 10 plan (docs/specs/net10-plan.md, the S3a-1 row and its risk item; docs/port-notes/S3a-1.md section 8): with
// Thread.Abort gone, no database write starts for a progress window's action after the user cancelled it. One test per pattern of the audited
// ProgressWin callers: an action that writes without checking anything (the database layer refuses its next command), a write and its log entry
// run as one unit (ODCancellation.RunUncancellable), a cleanup write in a catch after the cancel (a unit too), an action that spreads its writes
// over ODThread.RunParallel, a loop that writes files and checks before each, an action that does not stop within the window's wait (logged, and
// still no write after the cancel), and a second ShowDialog while the first action still runs (each run keeps its own token). The rows are
// synthetic, in a scratch database of this class whose name starts with s3a1b_ (dropped afterwards) on the local MariaDB only; the windows are
// the real ones, closed by their X from a timer.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace M2MISCTests {
	[TestClass]
	public class ProgressWinCancelWriteTests {
		private const string PREFIX="s3a1b_";
		///<summary>Unique to this run (several worktrees share the one local server): start time (UTC), process id and a random number.</summary>
		private static readonly string _dbName=PREFIX+DateTime.UtcNow.ToString("yyMMddHHmmss")+"_"+System.Diagnostics.Process.GetCurrentProcess().Id
			+"_"+new Random().Next(1000,10000);
		///<summary>Bound for every wait, so that a fault fails the test instead of hanging the run.</summary>
		private static readonly TimeSpan _timeSpanWait=TimeSpan.FromSeconds(30);
		private static bool _isCreated;
		private static string _skipReason;

		#region Scratch database
		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			if(TestDb.SkipReason!=null) {
				_skipReason=TestDb.SkipReason;
				return;
			}
			DropStale();
			using(MySqlConnection connection=OpenServer(null)) {
				Exec(connection,"CREATE DATABASE `"+_dbName+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
			}
			_isCreated=true;
			using(MySqlConnection connection=OpenServer(_dbName)) {
				//Synthetic probe rows: Kind is "row" for a data write, "log" for the log entry that belongs to it, Step numbers them.
				Exec(connection,"CREATE TABLE cancelprobe (ProbeNum bigint NOT NULL AUTO_INCREMENT,Kind varchar(10) NOT NULL,Step int NOT NULL,"
					+"PRIMARY KEY (ProbeNum)) ENGINE=MyISAM");
			}
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			if(!_isCreated) {
				return;
			}
			ConnectProgram(TestDb.DbName);
			try {
				Drop(_dbName);
			}
			catch(Exception) {
				//Left behind, it is dropped by a later run once it is stale.
			}
		}

		///<summary>The program's connection (the static one, which the progress window's worker thread uses) goes to this class's database for each
		///test and back to M2MISCTests' own afterwards.</summary>
		[TestInitialize]
		public void TestInitialize() {
			if(_skipReason!=null) {
				Assert.Inconclusive(_skipReason);
			}
			ConnectProgram(_dbName);
			DataCore.NonQ("DELETE FROM cancelprobe");
		}

		[TestCleanup]
		public void TestCleanup() {
			if(_skipReason==null) {
				ConnectProgram(TestDb.DbName);
			}
		}

		private static void ConnectProgram(string dbName) {
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientDirect;
			new DataConnection().SetDb(TestDb.Server,dbName,TestDb.User,TestDb.Password,"","",DatabaseType.MySql,true,DevDbTls.GetSslCa(TestDb.Server));
		}

		private static void DropStale() {
			Regex regex=new Regex("^"+PREFIX+@"(\d{12})_\d+_\d+$");
			List<string> listNames=new List<string>();
			using(MySqlConnection connection=OpenServer(null))
			using(MySqlCommand command=new MySqlCommand("SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME LIKE 's3a1b\\_%'",connection))
			using(MySqlDataReader reader=command.ExecuteReader()) {
				while(reader.Read()) {
					listNames.Add(reader.GetString(0));
				}
			}
			foreach(string name in listNames) {
				Match match=regex.Match(name);
				if(!match.Success || !DateTime.TryParseExact(match.Groups[1].Value,"yyMMddHHmmss",CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeUniversal|DateTimeStyles.AdjustToUniversal,out DateTime dateStarted))
				{
					continue;
				}
				if(DateTime.UtcNow-dateStarted<TimeSpan.FromHours(12)) {
					continue;//may belong to a run that is still going
				}
				try {
					Drop(name);
				}
				catch(Exception) {
					//Another run may be dropping it at the same time.
				}
			}
		}

		private static void Drop(string name) {
			if(!name.StartsWith(PREFIX)) {
				throw new ArgumentException("These tests only drop databases whose name starts with "+PREFIX);
			}
			using(MySqlConnection connection=OpenServer(null)) {
				Exec(connection,"DROP DATABASE IF EXISTS `"+name+"`");
			}
		}

		private static MySqlConnection OpenServer(string database) {
			string connectionString="Server="+TestDb.Server+";Port=3306;User ID="+TestDb.User+";Password="+TestDb.Password+";Connection Timeout=15";
			if(database!=null) {
				connectionString+=";Database="+database;
			}
			MySqlConnection connection=new MySqlConnection(DevDbTls.Apply(connectionString));
			connection.Open();
			return connection;
		}

		private static void Exec(MySqlConnection connection,string command) {
			using(MySqlCommand mySqlCommand=new MySqlCommand(command,connection)) {
				mySqlCommand.ExecuteNonQuery();
			}
		}
		#endregion Scratch database

		#region Helpers
		///<summary>A write of the program: one probe row through the database layer (DataCore, DataConnection).</summary>
		private static void InsertProbe(string kind,int step) {
			DataCore.NonQ("INSERT INTO cancelprobe (Kind,Step) VALUES('"+POut.String(kind)+"',"+step+")");
		}

		///<summary>Counted on the calling thread (the test's or the window's main thread, which has no cancellation token).</summary>
		private static int CountProbes(string kind) {
			return PIn.Int(DataCore.GetScalar("SELECT COUNT(*) FROM cancelprobe WHERE Kind='"+POut.String(kind)+"'"));
		}

		///<summary>Pumps the STA thread (as the program's main thread does after the window closed) until isDone, or the bound.</summary>
		private static void PumpUntil(Func<bool> isDone) {
			DateTime dateTimeEnd=DateTime.Now+_timeSpanWait;
			while(!isDone() && DateTime.Now<dateTimeEnd) {
				FrmODBase.DoEvents();
				System.Windows.Forms.Application.DoEvents();
				Thread.Sleep(10);
			}
		}
		#endregion Helpers

		///<summary>An action that writes row after row and neither checks the token nor reports progress (most audited callers: one business call
		///that writes many rows): after the cancel the database layer refuses its next command, so the action stops within the window's wait and at
		///most the write under way at the cancel finishes.</summary>
		[TestMethod]
		public void Cancel_ActionThatChecksNothing_NoDatabaseWriteAfterTheCancel() {
			ProgressWinTests.RunSta(() => {
				int countWrites=0;
				int countAtCancel=-1;
				Exception exceptionInAction=null;
				ManualResetEventSlim manualResetEventEnded=new ManualResetEventSlim(false);
				ProgressWin progressWin=new ProgressWin();
				progressWin.StartingMessage="S3a-1b rows";
				progressWin.ActionCancel=() => countAtCancel=CountProbes("row");//the main thread: no token, so it can still read
				progressWin.ActionMain=() => {
					try {
						DateTime dateTimeEnd=DateTime.Now+_timeSpanWait;
						for(int i=0;DateTime.Now<dateTimeEnd;i++) {
							InsertProbe("row",i);
							Interlocked.Increment(ref countWrites);
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
				Exception exception=ProgressWinTests.ShowAndCloseWhen(progressWin,x => Volatile.Read(ref countWrites)>=3);
				Assert.IsNull(exception,"Nothing is thrown to the caller: "+exception);
				Assert.IsTrue(progressWin.IsCancelled);
				Assert.IsFalse(progressWin.IsWorkerStillRunning,"The database layer stopped the action within the window's wait.");
				Assert.IsTrue(manualResetEventEnded.IsSet);
				Assert.IsInstanceOfType(exceptionInAction,typeof(OperationCanceledException),"The next write after the cancel threw: "+exceptionInAction);
				Thread.Sleep(200);
				int countRows=CountProbes("row");
				Assert.IsTrue(countAtCancel>=3,"rows at the cancel: "+countAtCancel);
				Assert.IsTrue(countRows<=countAtCancel+1,"At most the write under way at the cancel: rows "+countRows+", at the cancel "+countAtCancel);
				//The main thread of the program writes as before: the check only stops the cancelled action's thread.
				InsertProbe("main",0);
				Assert.AreEqual(1,CountProbes("main"));
			});
		}

		///<summary>A write and its log entry as one unit (FormBackup's archive deletes, FormClinics' and FormProviderSetup's moves, the claim
		///attachment sends): a cancel during a unit lets it finish with its log entry, even through its progress event, and no unit starts after
		///it. Every row has its log entry.</summary>
		[TestMethod]
		public void Cancel_WriteAndItsLogEntryAsOneUnit_NeverSplit_NoUnitAfterTheCancel() {
			ProgressWinTests.RunSta(() => {
				int countUnitsStarted=0;
				int countUnitsAtCancel=-1;
				int isInUnit=0;
				ManualResetEventSlim manualResetEventEnded=new ManualResetEventSlim(false);
				ManualResetEventSlim manualResetEventInUnit=new ManualResetEventSlim(false);
				ProgressWin progressWin=new ProgressWin();
				progressWin.StartingMessage="S3a-1b units";
				progressWin.ActionCancel=() => countUnitsAtCancel=Volatile.Read(ref countUnitsStarted);
				progressWin.ActionMain=() => {
					try {
						for(int i=0;i<1000;i++) {
							progressWin.CancellationToken.ThrowIfCancellationRequested();//before each unit
							int step=i;
							ODCancellation.RunUncancellable(() => {
								Interlocked.Increment(ref countUnitsStarted);
								InsertProbe("row",step);
								Volatile.Write(ref isInUnit,1);
								manualResetEventInUnit.Set();
								Thread.Sleep(60);//the cancel comes in here
								ODEvent.Fire(ODEventType.ProgressBar,"Unit "+step);//no cancellation point inside a unit
								InsertProbe("log",step);
								Volatile.Write(ref isInUnit,0);
							});
						}
					}
					finally {
						manualResetEventEnded.Set();
					}
				};
				Exception exception=ProgressWinTests.ShowAndCloseWhen(progressWin,x => Volatile.Read(ref countUnitsStarted)>=3 && Volatile.Read(ref isInUnit)==1);
				Assert.IsNull(exception,"Nothing is thrown to the caller: "+exception);
				Assert.IsTrue(progressWin.IsCancelled);
				Assert.IsFalse(progressWin.IsWorkerStillRunning,"The unit under way took less than the window's wait.");
				Assert.IsTrue(manualResetEventEnded.IsSet);
				int countRows=CountProbes("row");
				Assert.AreEqual(countRows,CountProbes("log"),"Every row has its log entry: the unit under way at the cancel finished.");
				Assert.IsTrue(countRows<=countUnitsAtCancel+1,"No unit started after the cancel: rows "+countRows+", units at the cancel "
					+countUnitsAtCancel);
			});
		}

		///<summary>A cleanup write in a catch after the cancel (ImageStore.Import and its siblings: the document row first, then the file and its
		///security log entry, and the catch deletes the row if that fails): the log entry after the cancel is refused, and so would a plain cleanup
		///be (unlike under Thread.Abort, whose catch bodies still wrote); the cleanup as a unit (ODCancellation.RunUncancellable) runs, so no
		///orphan row is left.</summary>
		[TestMethod]
		public void Cancel_CleanupWriteInCatchAfterTheCancel_AsUnit_LeavesNoOrphanRow() {
			ProgressWinTests.RunSta(() => {
				ManualResetEventSlim manualResetEventInserted=new ManualResetEventSlim(false);
				ManualResetEventSlim manualResetEventEnded=new ManualResetEventSlim(false);
				Exception exceptionInStep=null;
				bool isPlainCleanupRefused=false;
				bool isCleanupRun=false;
				ProgressWin progressWin=new ProgressWin();
				progressWin.StartingMessage="S3a-1b cleanup in catch";
				progressWin.ActionMain=() => {
					try {
						InsertProbe("row",1);//the document row (Documents.Insert)
						try {
							manualResetEventInserted.Set();
							progressWin.CancellationToken.WaitHandle.WaitOne(_timeSpanWait);//the file copy or upload under way at the cancel
							InsertProbe("log",1);//the file's security log entry: refused after the cancel
						}
						catch(Exception ex) {
							exceptionInStep=ex;
							try {
								DataCore.NonQ("DELETE FROM cancelprobe WHERE Kind='none'");//a cleanup that is not a unit
							}
							catch(OperationCanceledException) {
								isPlainCleanupRefused=true;
							}
							ODCancellation.RunUncancellable(() => DataCore.NonQ("DELETE FROM cancelprobe WHERE Kind='row' AND Step=1"));//as ImageStore.Import
							isCleanupRun=true;
							throw;
						}
					}
					finally {
						manualResetEventEnded.Set();
					}
				};
				Exception exception=ProgressWinTests.ShowAndCloseWhen(progressWin,x => manualResetEventInserted.IsSet);
				Assert.IsNull(exception,"Nothing is thrown to the caller: "+exception);
				Assert.IsTrue(progressWin.IsCancelled);
				Assert.IsFalse(progressWin.IsWorkerStillRunning,"The action stopped within the window's wait.");
				Assert.IsTrue(manualResetEventEnded.IsSet);
				Assert.IsInstanceOfType(exceptionInStep,typeof(OperationCanceledException),"The log entry after the cancel was refused: "+exceptionInStep);
				Assert.IsTrue(isPlainCleanupRefused,"A cleanup write that is not a unit is refused after the cancel.");
				Assert.IsTrue(isCleanupRun);
				Assert.AreEqual(0,CountProbes("row"),"No orphan row: the cleanup ran as a unit.");
				Assert.AreEqual(0,CountProbes("log"),"No log entry after the cancel.");
			});
		}

		///<summary>An action that spreads its writes over ODThread.RunParallel (FormClinics, FormProviderSetup, FormFinanceCharges; FeeScheds,
		///Procedures and LargeTableHelper inside the business layer): the pool threads share the action's token, so after the cancel they start no
		///further action and their database writes stop as the action's do; RunParallel then throws on the action's thread, which stops there.</summary>
		[TestMethod]
		public void Cancel_RunParallelPool_StartsNoFurtherAction_NoWriteAfterTheCancel_ParentStops() {
			ProgressWinTests.RunSta(() => {
				int countWrites=0;
				int countAtCancel=-1;
				bool isAfterRunParallelReached=false;
				Exception exceptionInAction=null;
				ProgressWin progressWin=new ProgressWin();
				progressWin.StartingMessage="S3a-1b pool";
				progressWin.ActionCancel=() => countAtCancel=CountProbes("row");
				progressWin.ActionMain=() => {
					try {
						List<Action> listActions=Enumerable.Range(0,400).Select(x => new Action(() => {
							InsertProbe("row",x);
							Interlocked.Increment(ref countWrites);
							ODEvent.Fire(ODEventType.ProgressBar,"Row "+x);//a pool thread's progress event does not throw
							Thread.Sleep(20);
						})).ToList();
						ODThread.RunParallel(listActions,_timeSpanWait,numThreads:2,onException:ex => ex.DoNothing());
						isAfterRunParallelReached=true;
						InsertProbe("after",0);
					}
					catch(Exception ex) {
						exceptionInAction=ex;
						throw;
					}
				};
				Exception exception=ProgressWinTests.ShowAndCloseWhen(progressWin,x => Volatile.Read(ref countWrites)>=4);
				Assert.IsNull(exception,"Nothing is thrown to the caller: "+exception);
				Assert.IsTrue(progressWin.IsCancelled);
				Assert.IsFalse(progressWin.IsWorkerStillRunning,"The pool's actions under way ended within the window's wait.");
				Assert.IsInstanceOfType(exceptionInAction,typeof(OperationCanceledException),"RunParallel threw on the action's thread: "+exceptionInAction);
				Assert.IsFalse(isAfterRunParallelReached,"The action did not go on after RunParallel.");
				Thread.Sleep(300);
				int countRows=CountProbes("row");
				Assert.IsTrue(countRows<=countAtCancel+2,"At most one write under way per pool thread: rows "+countRows+", at the cancel "+countAtCancel);
				Assert.IsTrue(countRows<400,"The pool started no further action: rows "+countRows);
				Assert.AreEqual(0,CountProbes("after"));
			});
		}

		///<summary>A loop that writes files (FormBackup's copies of the database and the A to Z folder, DemandForce's extract): it checks before each
		///file, so no file is written after the cancel beyond the one under way.</summary>
		[TestMethod]
		public void Cancel_FileLoopChecksBeforeEachFile_NoFileAfterTheCancel() {
			string folder=Path.Combine(Path.GetTempPath(),"s3a1b_files_"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(folder);
			try {
				ProgressWinTests.RunSta(() => {
					int countFiles=0;
					int countAtCancel=-1;
					ProgressWin progressWin=new ProgressWin();
					progressWin.StartingMessage="S3a-1b files";
					progressWin.ActionCancel=() => countAtCancel=Directory.GetFiles(folder).Length;
					progressWin.ActionMain=() => {
						for(int i=0;i<5000;i++) {
							ODCancellation.ThrowIfCancellationRequested();//as in FormBackup's copy loops
							File.WriteAllText(Path.Combine(folder,"synthetic"+i+".txt"),"synthetic "+i);
							Interlocked.Increment(ref countFiles);
							Thread.Sleep(5);
						}
					};
					Exception exception=ProgressWinTests.ShowAndCloseWhen(progressWin,x => Volatile.Read(ref countFiles)>=3);
					Assert.IsNull(exception);
					Assert.IsTrue(progressWin.IsCancelled);
					Assert.IsFalse(progressWin.IsWorkerStillRunning);
					Thread.Sleep(200);
					int countFilesAfter=Directory.GetFiles(folder).Length;
					Assert.IsTrue(countAtCancel>=3);
					Assert.IsTrue(countFilesAfter<=countAtCancel+1,"files "+countFilesAfter+", at the cancel "+countAtCancel);
				});
			}
			finally {
				ODException.SwallowAnyException(() => Directory.Delete(folder,true));
			}
		}

		///<summary>An action that does not stop within the window's wait (a long vendor call, here a wait that ignores the token): the window
		///closes as cancelled and logs that the action still runs; when the action then tries to write, the database layer refuses, the action ends
		///and its end is logged. An exception that an action throws after the cancel is logged, not dropped. Nothing is shown and nothing is
		///thrown to the caller.</summary>
		[TestMethod]
		public void Cancel_WorkerStillRunning_IsLogged_ThenWritesNothing_AndAnExceptionAfterTheCancelIsLogged() {
			ConcurrentQueue<string> queueLines=new ConcurrentQueue<string>();
			Action<string> actionLogged=line => queueLines.Enqueue(line);
			ODCancellation.LineLogged+=actionLogged;
			try {
				ProgressWinTests.RunSta(() => {
					ManualResetEventSlim manualResetEventStarted=new ManualResetEventSlim(false);
					ManualResetEventSlim manualResetEventRelease=new ManualResetEventSlim(false);
					ManualResetEventSlim manualResetEventEnded=new ManualResetEventSlim(false);
					Exception exceptionInAction=null;
					ProgressWin progressWin=new ProgressWin();
					progressWin.StartingMessage="S3a-1b slow vendor call";
					progressWin.ActionMain=() => {
						try {
							manualResetEventStarted.Set();
							manualResetEventRelease.Wait(_timeSpanWait);//ignores the token
							InsertProbe("row",1);//refused: the user cancelled
						}
						catch(Exception ex) {
							exceptionInAction=ex;
							throw;
						}
						finally {
							manualResetEventEnded.Set();
						}
					};
					Exception exception=ProgressWinTests.ShowAndCloseWhen(progressWin,x => manualResetEventStarted.IsSet);
					Assert.IsNull(exception);
					Assert.IsTrue(progressWin.IsCancelled);
					Assert.IsTrue(progressWin.IsWorkerStillRunning);
					Assert.IsTrue(queueLines.Any(x => x.Contains("'S3a-1b slow vendor call'") && x.Contains("still running")),
						"The still-running worker was logged: "+string.Join(" | ",queueLines));
					manualResetEventRelease.Set();
					PumpUntil(() => manualResetEventEnded.IsSet && queueLines.Any(x => x.Contains("'S3a-1b slow vendor call'") && x.Contains("after the window closed")));
					Assert.IsTrue(manualResetEventEnded.IsSet);
					Assert.IsInstanceOfType(exceptionInAction,typeof(OperationCanceledException),"The write after the cancel was refused: "+exceptionInAction);
					Assert.AreEqual(0,CountProbes("row"),"No row was written after the cancel.");
					Assert.IsTrue(queueLines.Any(x => x.Contains("'S3a-1b slow vendor call'") && x.Contains("stopped at a cancellation point")
						&& x.Contains("after the window closed")),"The worker's end was logged: "+string.Join(" | ",queueLines));
					//An action that fails after the cancel (an exception of its own, not the cancellation): logged with its text.
					ManualResetEventSlim manualResetEventStarted2=new ManualResetEventSlim(false);
					ManualResetEventSlim manualResetEventRelease2=new ManualResetEventSlim(false);
					ManualResetEventSlim manualResetEventEnded2=new ManualResetEventSlim(false);
					ProgressWin progressWin2=new ProgressWin();
					progressWin2.StartingMessage="S3a-1b failing call";
					progressWin2.ActionMain=() => {
						try {
							manualResetEventStarted2.Set();
							manualResetEventRelease2.Wait(_timeSpanWait);
							throw new InvalidOperationException("Synthetic failure after the cancel.");
						}
						finally {
							manualResetEventEnded2.Set();
						}
					};
					Exception exception2=ProgressWinTests.ShowAndCloseWhen(progressWin2,x => manualResetEventStarted2.IsSet);
					Assert.IsNull(exception2,"Not thrown to the caller: "+exception2);
					Assert.IsTrue(progressWin2.IsCancelled);
					manualResetEventRelease2.Set();
					PumpUntil(() => queueLines.Any(x => x.Contains("'S3a-1b failing call'") && x.Contains("Synthetic failure after the cancel.")));
					Assert.IsTrue(queueLines.Any(x => x.Contains("'S3a-1b failing call'") && x.Contains("threw after the user cancelled")
						&& x.Contains("Synthetic failure after the cancel.")),"The exception was logged: "+string.Join(" | ",queueLines));
					Assert.IsFalse(progressWin2.IsSuccess);
				});
			}
			finally {
				ODCancellation.LineLogged-=actionLogged;
			}
		}

		///<summary>A second ShowDialog of the same ProgressWin while the first, cancelled action still runs: each run keeps its own token, so the
		///first action still sees its cancel (and writes nothing more) and the second one runs with a new token to success.</summary>
		[TestMethod]
		public void Cancel_SecondShowDialog_EarlierActionKeepsItsCancelledToken() {
			ProgressWinTests.RunSta(() => {
				ManualResetEventSlim manualResetEventStarted=new ManualResetEventSlim(false);
				ManualResetEventSlim manualResetEventRelease=new ManualResetEventSlim(false);
				ManualResetEventSlim manualResetEventEnded=new ManualResetEventSlim(false);
				bool isFirstTokenCancelled=false;
				bool isSecondTokenCancelled=true;
				ProgressWin progressWin=new ProgressWin();
				progressWin.StartingMessage="S3a-1b first run";
				progressWin.ActionMain=() => {
					try {
						manualResetEventStarted.Set();
						manualResetEventRelease.Wait(_timeSpanWait);//ignores the token until released
						isFirstTokenCancelled=progressWin.CancellationToken.IsCancellationRequested;//read after the second ShowDialog
						if(!progressWin.CancellationToken.IsCancellationRequested) {
							InsertProbe("row",1);//would be a write after the cancel
						}
					}
					finally {
						manualResetEventEnded.Set();
					}
				};
				Exception exception=ProgressWinTests.ShowAndCloseWhen(progressWin,x => manualResetEventStarted.IsSet);
				Assert.IsNull(exception);
				Assert.IsTrue(progressWin.IsCancelled);
				Assert.IsTrue(progressWin.IsWorkerStillRunning);
				//The second run, while the first action still waits.
				progressWin.IsCancelled=false;
				progressWin.StartingMessage="S3a-1b second run";
				progressWin.ActionMain=() => {
					isSecondTokenCancelled=progressWin.CancellationToken.IsCancellationRequested;
					manualResetEventRelease.Set();//the first action goes on now, during the second run
					if(!manualResetEventEnded.Wait(_timeSpanWait)) {
						throw new TimeoutException("The first action did not end.");
					}
					InsertProbe("second",1);
				};
				Exception exception2=ProgressWinTests.ShowAndCloseWhen(progressWin,null);
				Assert.IsNull(exception2,"The second run succeeded: "+exception2);
				Assert.IsTrue(progressWin.IsSuccess);
				Assert.IsFalse(isSecondTokenCancelled,"The second run has a new token.");
				Assert.IsTrue(isFirstTokenCancelled,"The first action still read its own, cancelled token.");
				Assert.AreEqual(0,CountProbes("row"),"The first action wrote nothing after its cancel.");
				Assert.AreEqual(1,CountProbes("second"));
			});
		}
	}
}
