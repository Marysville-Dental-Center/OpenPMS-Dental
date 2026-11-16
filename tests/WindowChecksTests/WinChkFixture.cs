// The run's set-up and clean-up, and what every check test shares: the opt-in, the scratch copy of the database, recording each check's
// result, and failing the test for a failed check.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowChecksTests {
	[TestClass]
	public class WinChkFixture {
		private static bool _hasPrechecked;
		///<summary>The run's clock, from the start of the test assembly.</summary>
		private static readonly Stopwatch _stopwatchRun=new Stopwatch();
		///<summary>The run's time budget in minutes (OPMS_WINDOW_CHECKS_BUDGET_MINUTES; default 15). The test runner kills an assembly after 20 minutes
		///(taskkill, so AssemblyCleanup never runs: no Z, no drop, no report); once the budget is spent no session starts and no check runs (each is
		///recorded FAILED, 'not run'), and each check's timeout is cut to the budget left plus a minute, so Z, the drop and the report still fit.
		///</summary>
		public const string VARIABLE_BUDGET="OPMS_WINDOW_CHECKS_BUDGET_MINUTES";

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext testContext) {
			_stopwatchRun.Start();
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
		}

		public static int BudgetMinutes {
			get {
				return int.TryParse(Environment.GetEnvironmentVariable(VARIABLE_BUDGET),out int minutes) && minutes>0 ? minutes : 15;
			}
		}

		public static double BudgetSecondsLeft {
			get {
				return BudgetMinutes*60-_stopwatchRun.Elapsed.TotalSeconds;
			}
		}

		public static bool IsBudgetSpent {
			get {
				return BudgetSecondsLeft<=0;
			}
		}

		public static string BudgetSpentText {
			get {
				return "the run's time budget is spent ("+_stopwatchRun.Elapsed.TotalMinutes.ToString("0.0")+" of "+BudgetMinutes+" minutes; the test runner kills "
					+"the assembly at 20); run this group again on its own ("+WinChkEnv.VARIABLE_GROUPS+")";
			}
		}

		///<summary>The timeout cut to the budget left plus a minute (at least 30 s).</summary>
		public static int CapTimeout(int timeoutSeconds) {
			return (int)Math.Max(30,Math.Min(timeoutSeconds,BudgetSecondsLeft+60));
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup() {
			if(!ScratchDb.IsMade) {
				if(_hasPrechecked) {
					//P0 failed (the copy could not be brought to this build's fork schema level; the copy is already dropped): the report still shows why.
					string reportErrorP0=Report.Write();
					Results.Log(reportErrorP0=="" ? "REPORT.md written" : "REPORT.md not written: "+reportErrorP0);
				}
				return;
			}
			try {
				PZTests.RunCleanUp();
			}
			catch(Exception ex) {
				Results.Log("Z failed: "+ex);
			}
			if(Environment.GetEnvironmentVariable("OPMS_WINDOW_CHECKS_KEEP_DB")=="1") {
				Results.Log("Kept "+ScratchDb.Name+" (OPMS_WINDOW_CHECKS_KEEP_DB=1)");
			}
			else {
				try {
					ScratchDb.Drop();
				}
				catch(Exception ex) {
					Results.Log("Drop failed: "+ex.Message);
				}
			}
			//The report, from every result in results.tsv (the last of each check), so the run that ends last leaves the whole report.
			string reportError=Report.Write();
			Results.Log(reportError=="" ? "REPORT.md written" : "REPORT.md not written: "+reportError);
		}

		///<summary>Call first in a check test: skips the test (inconclusive) when the checks are off, the group is not asked for, or the copy of
		///the database cannot be made. When the copy was restored but cannot be brought to this build's fork schema level, P0 is recorded FAILED with
		///the reason, the first check test fails with it, and no check runs (the program is never started on that copy).</summary>
		public static void Begin(string group) {
			string skipReason=WinChkEnv.GetSkipReason(group);
			if(skipReason!=null) {
				Assert.Inconclusive(skipReason);
			}
			skipReason=ScratchDb.GetSkipReason();
			if(skipReason!=null && ScratchDb.IsMigrationFailed) {
				if(!_hasPrechecked) {
					_hasPrechecked=true;
					PZTests.RecordMigrationFailure();
					Assert.Fail("P0 FAILED: "+ScratchDb.MigrationText+"; no check runs");
				}
				Assert.Inconclusive("Not run: P0 failed ("+ScratchDb.MigrationText+")");
			}
			if(skipReason!=null) {
				Assert.Inconclusive(skipReason);
			}
			if(!_hasPrechecked) {
				_hasPrechecked=true;
				PZTests.RunPrechecks();
			}
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
		}

		///<summary>Records the check's result and fails the test when the check failed. NEEDS-SCREEN, SKIPPED and PARTLY pass the test: the report
		///lists them for an on-screen pass.</summary>
		public static void Finish(CheckResult checkResult) {
			Results.Write(checkResult);
			Console.WriteLine(checkResult.Id+" "+checkResult.Status+": "+checkResult.Summary);
			if(checkResult.Status==CheckStatus.Failed) {
				Assert.Fail(checkResult.Id+" FAILED: "+checkResult.Summary);
			}
		}

		///<summary>Records a check that headless cannot reach, or that the checklist skips.</summary>
		public static void Record(string id,string title,string status,string reason) {
			CheckResult checkResult=new CheckResult(id,title) { StatusForced=status,Reason=reason };
			Finish(checkResult);
		}
	}
}
