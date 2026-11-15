// S3a-1c of the .NET 10 plan (docs/port-notes/S3a-1.md, section 9; the S3a-1b review leftovers): after the user cancelled a progress window,
// nothing shows an error that only the cancel caused, and a worker that outlives its window is always logged with its end. The real window,
// closed by its X from a timer, for the worker that ends right after the window's wait (decided under a lock with the worker's end, through a
// test hook between the wait and the decision) and for an action whose catch would show an error from the worker (it rethrows the cancel, as
// DemandForce, PaySimpleAchStatusSync and QuickBooksOnline.RunForWindow do). The callers that showed a message from the worker's result after
// a cancel are checked in their source. No database: a command refused after the cancel is refused before any connection is opened.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace M2MISCTests {
	[TestClass]
	public class ProgressWinCancelMessageTests {
		///<summary>Bound for every wait, so that a fault fails the test instead of hanging the run.</summary>
		private static readonly TimeSpan _timeSpanWait=TimeSpan.FromSeconds(30);

		#region Helpers
		///<summary>FrmProgressAuto's hook between the cancel's timed-out wait and the decision whether the worker still runs (tests only).</summary>
		private static FieldInfo GetHookField() {
			FieldInfo fieldInfo=typeof(WpfControls.UI.FrmProgressAuto).GetField("_actionAfterJoinTimeoutForTests",BindingFlags.NonPublic|BindingFlags.Static);
			Assert.IsNotNull(fieldInfo,"FrmProgressAuto has its test hook.");
			return fieldInfo;
		}

		///<summary>Pumps the STA thread (as the program's main thread does after the window closed) until isDone, or the bound.</summary>
		private static void PumpUntil(Func<bool> isDone) {
			DateTime dateTimeEnd=DateTime.Now+_timeSpanWait;
			while(!isDone() && DateTime.Now<dateTimeEnd) {
				System.Windows.Forms.Application.DoEvents();
				Thread.Sleep(10);
			}
		}

		///<summary>Index of value in text after the index start; fails when it is not there.</summary>
		private static int IndexOf(string text,string value,int start,string what) {
			int index=text.IndexOf(value,start,StringComparison.Ordinal);
			Assert.IsTrue(index>=0,what+": '"+value+"' not found.");
			return index;
		}

		///<summary>In the method that starts with methodStart: first comes before second.</summary>
		private static void AssertBefore(string code,string methodStart,string first,string second,string what) {
			int indexMethod=IndexOf(code,methodStart,0,what);
			int indexFirst=IndexOf(code,first,indexMethod,what);
			int indexSecond=IndexOf(code,second,indexMethod,what);
			Assert.IsTrue(indexFirst<indexSecond,what+": '"+first+"' comes before '"+second+"'.");
		}
		#endregion Helpers

		///<summary>The worker ends right after the window's wait has timed out, before the window decides whether it still runs (the hook lets it end
		///there and waits for its thread). The decision sees the end: the worker is not still running, and nothing is logged. Before S3a-1c the window
		///marked it as still running from the wait alone, logged that, and the worker, already past its check, never logged its end.</summary>
		[TestMethod]
		public void Cancel_WorkerEndsRightAfterTheWait_NotStillRunning_NothingLogged() {
			FieldInfo fieldInfo=GetHookField();
			ConcurrentQueue<string> queueLines=new ConcurrentQueue<string>();
			Action<string> actionLogged=line => queueLines.Enqueue(line);
			ODCancellation.LineLogged+=actionLogged;
			try {
				ProgressWinTests.RunSta(() => {
					ManualResetEventSlim manualResetEventStarted=new ManualResetEventSlim(false);
					ManualResetEventSlim manualResetEventRelease=new ManualResetEventSlim(false);
					bool isHookCalled=false;
					bool isWorkerJoined=false;
					fieldInfo.SetValue(null,new Action<Thread>(threadWorker => {
						isHookCalled=true;
						manualResetEventRelease.Set();
						isWorkerJoined=threadWorker.Join(_timeSpanWait);
					}));
					ProgressWin progressWin=new ProgressWin();
					progressWin.StartingMessage="S3a-1c ends right after the wait";
					progressWin.ActionMain=() => {
						manualResetEventStarted.Set();
						manualResetEventRelease.Wait(_timeSpanWait);//ignores the token
					};
					Exception exception=ProgressWinTests.ShowAndCloseWhen(progressWin,x => manualResetEventStarted.IsSet);
					Assert.IsNull(exception,"Nothing is thrown to the caller: "+exception);
					Assert.IsTrue(progressWin.IsCancelled);
					Assert.IsTrue(isHookCalled,"The window's wait timed out.");
					Assert.IsTrue(isWorkerJoined,"The worker ended in the hook.");
					Assert.IsFalse(progressWin.IsWorkerStillRunning,"The worker had ended before the decision.");
					Thread.Sleep(200);
					Assert.IsFalse(queueLines.Any(x => x.Contains("'S3a-1c ends right after the wait'")),
						"Neither 'still running' nor an end is logged: "+string.Join(" | ",queueLines));
				});
			}
			finally {
				fieldInfo.SetValue(null,null);
				ODCancellation.LineLogged-=actionLogged;
			}
		}

		///<summary>The worker is let go at the moment of the decision and races it. Whichever wins, "still running" is logged if and only if the
		///worker's end is logged too, and the end always after it.</summary>
		[TestMethod]
		public void Cancel_WorkerEndsDuringTheDecision_StillRunningLoggedIfAndOnlyIfItsEndIs() {
			FieldInfo fieldInfo=GetHookField();
			ConcurrentQueue<string> queueLines=new ConcurrentQueue<string>();
			Action<string> actionLogged=line => queueLines.Enqueue(line);
			ODCancellation.LineLogged+=actionLogged;
			try {
				ProgressWinTests.RunSta(() => {
					for(int i=0;i<5;i++) {
						string startingMessage="S3a-1c race "+i;
						ManualResetEventSlim manualResetEventStarted=new ManualResetEventSlim(false);
						ManualResetEventSlim manualResetEventRelease=new ManualResetEventSlim(false);
						ManualResetEventSlim manualResetEventEnded=new ManualResetEventSlim(false);
						fieldInfo.SetValue(null,new Action<Thread>(threadWorker => manualResetEventRelease.Set()));
						ProgressWin progressWin=new ProgressWin();
						progressWin.StartingMessage=startingMessage;
						progressWin.ActionMain=() => {
							try {
								manualResetEventStarted.Set();
								manualResetEventRelease.Wait(_timeSpanWait);
							}
							finally {
								manualResetEventEnded.Set();
							}
						};
						Exception exception=ProgressWinTests.ShowAndCloseWhen(progressWin,x => manualResetEventStarted.IsSet);
						Assert.IsNull(exception);
						Assert.IsTrue(progressWin.IsCancelled);
						Func<string,bool> funcIsStillRunningLine=x => x.Contains("'"+startingMessage+"'") && x.Contains("still running");
						Func<string,bool> funcIsEndLine=x => x.Contains("'"+startingMessage+"'") && x.Contains("after the window closed");
						if(progressWin.IsWorkerStillRunning) {
							PumpUntil(() => queueLines.Any(funcIsEndLine));
							string[] arrayLines=queueLines.ToArray();
							int indexStillRunning=Array.FindIndex(arrayLines,x => funcIsStillRunningLine(x));
							int indexEnd=Array.FindIndex(arrayLines,x => funcIsEndLine(x));
							Assert.IsTrue(indexStillRunning>=0,"Still running is logged: "+string.Join(" | ",arrayLines));
							Assert.IsTrue(indexEnd>indexStillRunning,"Its end is logged, after it: "+string.Join(" | ",arrayLines));
						}
						else {
							Assert.IsTrue(manualResetEventEnded.Wait(_timeSpanWait));
							Thread.Sleep(100);
							Assert.IsFalse(queueLines.Any(x => x.Contains("'"+startingMessage+"'")),"Not still running: nothing logged.");
						}
					}
				});
			}
			finally {
				fieldInfo.SetValue(null,null);
				ODCancellation.LineLogged-=actionLogged;
			}
		}

		///<summary>An action whose catch shows an error from the worker (DemandForce's "Export file creation failed", a vendor's "... failed"):
		///after the cancel the database layer refuses its next command, also a read. With the cancel rethrown before the general catch, nothing is
		///shown; without it, the catch would show an error that only the cancel caused.</summary>
		[TestMethod]
		public void Cancel_ActionWhoseCatchShowsAnError_RethrowsTheCancel_NoErrorShown() {
			ProgressWinTests.RunSta(() => {
				for(int i=0;i<2;i++) {
					bool isRethrowing=(i==0);
					int countErrorsShown=0;
					bool isRefused=false;
					ManualResetEventSlim manualResetEventStarted=new ManualResetEventSlim(false);
					ManualResetEventSlim manualResetEventEnded=new ManualResetEventSlim(false);
					ProgressWin progressWin=new ProgressWin();
					progressWin.StartingMessage="S3a-1c error after the cancel "+i;
					progressWin.ActionMain=() => {
						try {
							manualResetEventStarted.Set();
							progressWin.CancellationToken.WaitHandle.WaitOne(_timeSpanWait);//the call under way at the cancel
							try {
								DataCore.GetScalar("SELECT 1");//refused after the cancel, before a connection is opened
							}
							catch(OperationCanceledException) when(isRethrowing && ODCancellation.IsCancellationRequested) {
								isRefused=true;
								throw;
							}
							catch(Exception ex) {
								ex.DoNothing();
								Interlocked.Increment(ref countErrorsShown);//where the action showed its error
							}
						}
						finally {
							manualResetEventEnded.Set();
						}
					};
					Exception exception=ProgressWinTests.ShowAndCloseWhen(progressWin,x => manualResetEventStarted.IsSet);
					Assert.IsNull(exception,"Nothing is thrown to the caller: "+exception);
					Assert.IsTrue(progressWin.IsCancelled);
					Assert.IsTrue(manualResetEventEnded.Wait(_timeSpanWait));
					if(isRethrowing) {
						Assert.IsTrue(isRefused,"The read after the cancel was refused.");
						Assert.AreEqual(0,countErrorsShown,"No error is shown after the cancel.");
					}
					else {
						Assert.AreEqual(1,Volatile.Read(ref countErrorsShown),"Without the rethrow the catch takes the cancel for an error.");
					}
				}
			});
		}

		///<summary>EDS's Re-validate worker (FormEdsValidation) after the cancel: ValidateClaimNum, which turns every other failure into a failed
		///call, throws the database layer's refusal of the claim's read on, so the loop stops there instead of giving each claim not yet
		///revalidated an "EDS could not be reached" result. No database: the read is refused before any connection is opened.</summary>
		[TestMethod]
		public void EdsValidateClaimNum_AfterTheCancel_ThrowsTheCancel_NoFailedCall() {
			using(CancellationTokenSource cancellationTokenSource=new CancellationTokenSource()) {
				cancellationTokenSource.Cancel();
				using(ODCancellation.Use(cancellationTokenSource.Token)) {
					Assert.ThrowsExactly<OperationCanceledException>(
						() => OpenPmsDentalBusiness.VendorDirect.Eds.EdsClaimValidation.ValidateClaimNum(1,0,TimeSpan.FromSeconds(1)),
						"The cancel is thrown on, not returned as a failed call.");
				}
			}
		}

		///<summary>The audited callers (S3a-1.md, section 9.4) that showed an error or a result from the worker after the window closed as cancelled
		///now check IsCancelled first, and the worker-side ones rethrow the cancel or check before their question.</summary>
		[TestMethod]
		public void Source_CallersCheckTheCancelBeforeTheirMessage() {
			string formBilling=TestDb.ReadRepoFile("OpenPmsDental","Forms","FormBilling.cs");
			AssertBefore(formBilling,"progressOD.StartingMessage=\"Deleting Statements...\";","if(progressOD.IsCancelled){","Unsent statements deleted: ",
				"FormBilling");
			string formPaySimpleSetup=TestDb.ReadRepoFile("OpenPmsDental","Forms","FormPaySimpleSetup.cs");
			AssertBefore(formPaySimpleSetup,"PaySimpleAchStatusSync.Run(isManual:true);","if(progressWin.IsCancelled || paySimpleAchSyncResult==null) {",
				"paySimpleAchSyncResult.GetSummary()","FormPaySimpleSetup");
			string formProviderSetup=TestDb.ReadRepoFile("OpenPmsDental","Forms","FormProviderSetup.cs");
			AssertBefore(formProviderSetup,"\"Gathering patient and provider details\"","if(progressOD.IsCancelled) {","\"No patients to reassign.\"",
				"FormProviderSetup, gathering");
			AssertBefore(formProviderSetup,"\"Reassigning patients\"","if(progressOD.IsCancelled) {","MsgBox.Show(this,\"Done\");",
				"FormProviderSetup, reassigning");
			string formQuickBooksOnlineSetup=TestDb.ReadRepoFile("OpenPmsDental","Forms","FormQuickBooksOnlineSetup.cs");
			AssertBefore(formQuickBooksOnlineSetup,"private void RunDisconnect() {","if(progressWin.IsCancelled) {","ShowError(exDisconnect,",
				"FormQuickBooksOnlineSetup, disconnect");
			AssertBefore(formQuickBooksOnlineSetup,"private void RunDisconnect() {","if(progressWin.IsCancelled) {","\"QuickBooks Online disconnected\"",
				"FormQuickBooksOnlineSetup, the disconnect's log entry");
			string formSheetFieldImage=TestDb.ReadRepoFile("OpenPmsDental","Forms","FormSheetFieldImage.cs");
			Assert.IsTrue(Regex.IsMatch(formSheetFieldImage,@"progressWin\.ShowDialog\(\);\s*if\(progressWin\.IsCancelled\) \{\s*return;[^\n]*\n\s*\}\s*"
				+@"if\(byteArray==null \|\| byteArray\.Length==0\)\{\s*MsgBox\.Show\(this,""Select a valid image first\.""\);"),"FormSheetFieldImage");
			string claimL=TestDb.ReadRepoFile("OpenPmsDental","Logic","ClaimL.cs");
			AssertBefore(claimL,"public static void ValidateXConnectAfterSave(","if(progressWin.IsCancelled || result==null) {","if(result.IsCallFailed) {",
				"ClaimL, validation after save");
			string formEdsValidation=TestDb.ReadRepoFile("OpenPmsDental","Forms","FormEdsValidation.cs");
			AssertBefore(formEdsValidation,"private void Revalidate(","if(progressWin.IsCancelled) {","dictResultsLatest.TryGetValue(",
				"FormEdsValidation, Re-validate");
			string formXConnectValidation=TestDb.ReadRepoFile("OpenPmsDental","Forms","FormXConnectValidation.cs");
			AssertBefore(formXConnectValidation,"private void Revalidate(","if(progressWin.IsCancelled) {","dictResultsLatest.TryGetValue(",
				"FormXConnectValidation, Re-validate");
			string reportComplex=TestDb.ReadRepoFile("OpenPmsDental","ReportingComplex","ReportComplex.cs");
			AssertBefore(reportComplex,"bool wasSubmitted=false;","if(!query.IsQueryStringNullOrEmpty && progressOD.IsCancelled){",
				"\"There was an error generating this report.\"","ReportComplex");
			string formBackup=TestDb.ReadRepoFile("OpenPmsDental","Forms","FormBackup.cs");
			Assert.IsTrue(Regex.IsMatch(formBackup,@"ODCancellation\.ThrowIfCancellationRequested\(\);\s*if\(!MsgBox\.Show\(MsgBoxButtons\.YesNo, ""Your AtoZ images folder"),
				"FormBackup: no question from the worker after a cancel.");
			string demandForce=TestDb.ReadRepoFile("WpfControlsOD","Bridges","DemandForce.cs");
			Assert.IsTrue(Regex.IsMatch(demandForce,@"catch\(OperationCanceledException\) \{\s*throw;[^\n]*\n\s*\}\s*catch \{\s*MessageBox\.Show\(Lang\.g\(""DemandForce"",""Export file creation failed"""),
				"DemandForce (S3a-1b): the cancel is rethrown before the catch that shows the error.");
			string quickBooksOnline=TestDb.ReadRepoFile("OpenPmsDentalBusiness","Bridges","QuickBooksOnline","QuickBooksOnline.cs");
			Assert.IsTrue(Regex.IsMatch(quickBooksOnline,@"catch\(OperationCanceledException\) when\(ODCancellation\.IsCancellationRequested\) \{\s*throw;\s*\}\s*catch\(Exception ex\) \{\s*return ex;"),
				"QuickBooksOnline.RunForWindow throws the cancel.");
		}
	}
}
