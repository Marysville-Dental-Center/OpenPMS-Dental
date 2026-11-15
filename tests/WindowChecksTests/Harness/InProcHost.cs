// The test's side of an in-process session: a new AppDomain based on the program's output folder (ApplicationBase OpenPmsDental\bin\Debug,
// ConfigurationFile OpenPmsDental.exe.config), the driver created in it from this assembly, the program started there with its command line,
// and the AppDomain unloaded at the end. One session is one start of the program; "exit and start" in a check is a new session.
// The row numbers the checks made (CheckResult.DictKeys) are kept here and handed to every session, so a check's undo in a later session
// deletes only the rows its check added. No session starts and no check runs once the run's time budget is spent (WinChkFixture.Budget).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WindowChecksTests.InProc;

namespace WindowChecksTests {
	public class InProcHost:IDisposable {
		private AppDomain _appDomain;
		public ProgramDriver Driver;
		public string StartError="";
		public string Patched="";
		public double SecondsToStart;
		private static int _countSessions;
		private static readonly Dictionary<string,long> _dictKeys=new Dictionary<string,long>();
		///<summary>Set when a check did not end and the session is left stuck (a window that does not close, or a thread that does not answer): the
		///session's later checks are not run in it.</summary>
		public string StuckReason="";

		///<summary>Prompts the start-up showed and the harness cancelled.</summary>
		public string StartPrompts="";

		public static InProcHost Start(string module,int timeoutSeconds=180) {
			return Start(module,WinChkEnv.DebugFolder,new string[0],timeoutSeconds);
		}

		///<summary>Starts the program of the folder (Debug or Release output) in process; start-up message boxes containing one of cancelTexts are
		///answered with Cancel.</summary>
		public static InProcHost Start(string module,string folder,string[] cancelTexts,int timeoutSeconds=180) {
			InProcHost inProcHost=new InProcHost();
			if(WinChkFixture.IsBudgetSpent) {
				inProcHost.StartError="not started: "+WinChkFixture.BudgetSpentText;
				Results.Log("In-process session not started: "+WinChkFixture.BudgetSpentText);
				return inProcHost;
			}
			timeoutSeconds=WinChkFixture.CapTimeout(timeoutSeconds);
			string exePath=Path.Combine(folder,"OpenPmsDental.exe");
			AppDomainSetup appDomainSetup=new AppDomainSetup {
				ApplicationBase=folder,
				ConfigurationFile=exePath+".config",
				ApplicationName="OpenPmsDental",
			};
			_countSessions++;
			Stopwatch stopwatch=Stopwatch.StartNew();
			inProcHost._appDomain=AppDomain.CreateDomain("winchk-"+_countSessions,null,appDomainSetup);
			inProcHost.Driver=(ProgramDriver)inProcHost._appDomain.CreateInstanceFromAndUnwrap(typeof(ProgramDriver).Assembly.Location,
				typeof(ProgramDriver).FullName);
			inProcHost.Patched=inProcHost.Driver.PatchProcessIdentity(exePath);
			string[] arrayArgs=ProgramProcess.ProgramArgs(module).Split(' ');
			Results.Log("In-process session "+_countSessions+" starting ("+inProcHost.Patched+")");
			inProcHost.StartError=inProcHost.Driver.Start(arrayArgs,timeoutSeconds,cancelTexts);
			inProcHost.StartPrompts=string.Join(" || ",inProcHost.Driver.ListStartPrompts);
			inProcHost.SecondsToStart=stopwatch.Elapsed.TotalSeconds;
			Results.Log("In-process session "+_countSessions+" start: "+(inProcHost.StartError=="" ? "ready" : inProcHost.StartError)+" after "
				+inProcHost.SecondsToStart.ToString("0.0")+" s");
			return inProcHost;
		}

		public CheckResult Run(string id,string title,int timeoutSeconds=240) {
			if(StartError!="") {
				CheckResult checkResult=new CheckResult(id,title);
				checkResult.Fail(WinChkFixture.IsBudgetSpent && StartError.StartsWith("not started: ") ? "Not run: "+WinChkFixture.BudgetSpentText
					: "The program did not start in process: "+StartError);
				return checkResult;
			}
			if(StuckReason!="") {
				CheckResult checkResult=new CheckResult(id,title);
				checkResult.Fail("Not run: the session is stuck after an earlier check ("+StuckReason+"); the caller starts a new session for the next one");
				return checkResult;
			}
			if(WinChkFixture.IsBudgetSpent) {
				CheckResult checkResult=new CheckResult(id,title);
				checkResult.Fail("Not run: "+WinChkFixture.BudgetSpentText);
				Results.Log("In-process check "+id+" not run: "+WinChkFixture.BudgetSpentText);
				return checkResult;
			}
			timeoutSeconds=WinChkFixture.CapTimeout(timeoutSeconds);
			Results.Log("In-process check "+id+" starts");
			lock(_dictKeys) {
				Driver.SetKeys(_dictKeys);
			}
			CheckResult checkResultRun=Driver.RunCheck(id,title,timeoutSeconds);
			lock(_dictKeys) {
				foreach(KeyValuePair<string,long> pair in checkResultRun.DictKeys) {
					_dictKeys[pair.Key]=pair.Value;
				}
			}
			Results.Log("In-process check "+id+": "+checkResultRun.Status);
			if(checkResultRun.ListFailures.Exists(x => x.Contains("did not end within") || x.Contains("did not answer"))) {
				try {
					StuckReason=Driver.GetStuckState();
				}
				catch(Exception ex) {
					StuckReason="the driver does not answer ("+ex.Message+")";
				}
				Results.Log("After "+id+" the session is "+(StuckReason=="" ? "not stuck" : "stuck: "+StuckReason));
			}
			return checkResultRun;
		}

		///<summary>The row number (or flag) a check recorded under the name, or 0.</summary>
		public static long GetKey(string name) {
			lock(_dictKeys) {
				return _dictKeys.TryGetValue(name,out long value) ? value : 0;
			}
		}

		///<summary>True when the caller should start a new session before its next check: a check left this one stuck.</summary>
		public bool NeedsNewSession {
			get {
				return StuckReason!="";
			}
		}

		public void Dispose() {
			if(Driver!=null) {
				try {
					string stop=Driver.Stop(60);
					Results.Log("In-process session stop: "+(stop==""? "ended" : stop));
				}
				catch(Exception ex) {
					Results.Log("In-process session stop failed: "+ex.Message);
				}
			}
			if(_appDomain!=null) {
				try {
					AppDomain.Unload(_appDomain);
					Results.Log("AppDomain unloaded");
				}
				catch(Exception ex) {
					Results.Log("AppDomain not unloaded: "+ex.GetType().Name+": "+ex.Message);
				}
				_appDomain=null;
			}
		}
	}
}
