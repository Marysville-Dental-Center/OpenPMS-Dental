// The test's side of an in-process session: a process of its own whose application base is the program's output folder (OpenPmsDental\bin\Debug
// or the Release folder) and whose config is OpenPmsDental.exe.config (TestProbeHost, started by ProbeProcess with a copy of the host in that
// folder), the driver created in it from this assembly (ProgramDriverHost), the program started there with its command line, and the process
// ended at the end. One session is one start of the program; "exit and start" in a check is a new session. Before S3a-8b of the .NET 10 move
// the session was a new AppDomain of this process, which .NET cannot create; a process isolates the program's static state, its config and its
// application base as the AppDomain did, and the program's own exit (Environment.Exit) now ends the session's process, not the test host.
// The row numbers the checks made (CheckResult.DictKeys) are kept here and handed to every session, so a check's undo in a later session
// deletes only the rows its check added. No session starts and no check runs once the run's time budget is spent (WinChkFixture.Budget).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using TestProbeHost;
using WindowChecksTests.InProc;

namespace WindowChecksTests {
	public class InProcHost:IDisposable {
		///<summary>How much longer than the driver's own timeout the test waits for a reply before it ends the session's process: the driver's
		///clean-up after a check that did not end (closing its windows, 30 s for the main window's closing) runs within it.</summary>
		private const int SECONDS_REPLY_MARGIN=180;
		private ProbeProcess _probeProcess;
		public string StartError="";
		public string Patched="";
		public double SecondsToStart;
		private static int _countSessions;
		private static readonly Dictionary<string,long> _dictKeys=new Dictionary<string,long>();
		///<summary>Set when a check did not end and the session is left stuck (a window that does not close, or a thread that does not answer), or
		///when the session's process ended or stopped answering: the session's later checks are not run in it.</summary>
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
			_countSessions++;
			Stopwatch stopwatch=Stopwatch.StartNew();
			string[] arrayArgs=ProgramProcess.ProgramArgs(module).Split(' ');
			try {
				inProcHost._probeProcess=StartSessionProcess(folder,TimeSpan.FromSeconds(timeoutSeconds+SECONDS_REPLY_MARGIN));
				inProcHost.Patched=inProcHost._probeProcess.Call(ProgramDriverHost.COMMAND_PATCH,exePath);
				Results.Log("In-process session "+_countSessions+" starting ("+inProcHost.Patched+")");
				string[] arrayReply=ProbeText.SplitList(inProcHost._probeProcess.Call(ProgramDriverHost.COMMAND_START,ProbeText.JoinList(arrayArgs),
					timeoutSeconds.ToString(CultureInfo.InvariantCulture),ProbeText.JoinList(cancelTexts)));
				inProcHost.StartError=arrayReply[0];
				inProcHost.StartPrompts=string.Join(" || ",arrayReply.Skip(1));
			}
			catch(Exception ex) {
				inProcHost.StartError="The session's process failed: "+ex.GetType().Name+": "+ex.Message
					+(inProcHost._probeProcess==null ? "" : " "+inProcHost._probeProcess.ErrorOutput);
			}
			inProcHost.SecondsToStart=stopwatch.Elapsed.TotalSeconds;
			Results.Log("In-process session "+_countSessions+" start: "+(inProcHost.StartError=="" ? "ready" : inProcHost.StartError)+" after "
				+inProcHost.SecondsToStart.ToString("0.0")+" s");
			return inProcHost;
		}

		///<summary>The session's process, not yet driving anything: a copy of TestProbeHost in the folder (its application base) with the folder's
		///OpenPmsDental.exe.config as its config, so the program's binding redirects and probing are the real ones, and ProgramDriverHost as its
		///handler. Each reply is waited for up to timeSpanTimeout. The caller disposes it, which ends the process and deletes the copy.</summary>
		public static ProbeProcess StartSessionProcess(string folder,TimeSpan timeSpanTimeout) {
			string pathConfig=Path.Combine(folder,"OpenPmsDental.exe.config");
			string configText=File.Exists(pathConfig) ? File.ReadAllText(pathConfig) : null;
			return ProbeProcess.Start(folder,configText,typeof(ProgramDriverHost),timeSpanTimeout);
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
			CheckResult checkResultRun;
			try {
				lock(_dictKeys) {
					_probeProcess.Call(ProgramDriverHost.COMMAND_SET_KEYS,ProbeText.JoinList(_dictKeys.SelectMany(x => new[] { x.Key,
						x.Value.ToString(CultureInfo.InvariantCulture) })));
				}
				checkResultRun=CheckResult.FromText(_probeProcess.CallWithTimeout(TimeSpan.FromSeconds(timeoutSeconds+SECONDS_REPLY_MARGIN),
					ProgramDriverHost.COMMAND_RUN_CHECK,id,title,timeoutSeconds.ToString(CultureInfo.InvariantCulture)));
			}
			catch(Exception ex) {
				//The session's process ended (the program's own exit, an unhandled exception) or stopped answering, and was ended.
				checkResultRun=new CheckResult(id,title);
				checkResultRun.Fail("The in-process session's process did not answer: "+ex.GetType().Name+": "+ex.Message);
				StuckReason="the session's process did not answer ("+ex.GetType().Name+")";
				Results.Log("In-process check "+id+": "+checkResultRun.Status+"; the session's process is gone: "+ex.Message);
				return checkResultRun;
			}
			lock(_dictKeys) {
				foreach(KeyValuePair<string,long> pair in checkResultRun.DictKeys) {
					_dictKeys[pair.Key]=pair.Value;
				}
			}
			Results.Log("In-process check "+id+": "+checkResultRun.Status);
			if(checkResultRun.ListFailures.Exists(x => x.Contains("did not end within") || x.Contains("did not answer"))) {
				try {
					StuckReason=_probeProcess.Call(ProgramDriverHost.COMMAND_GET_STUCK_STATE);
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
			if(_probeProcess==null) {
				return;
			}
			if(StartError=="" || !StartError.StartsWith("The session's process failed")) {
				try {
					string stop=_probeProcess.CallWithTimeout(TimeSpan.FromSeconds(60+SECONDS_REPLY_MARGIN),ProgramDriverHost.COMMAND_STOP,"60");
					Results.Log("In-process session stop: "+(stop=="" ? "ended" : stop));
				}
				catch(Exception ex) {
					Results.Log("In-process session stop failed: "+ex.Message);
				}
			}
			try {
				//Closes the process's standard input, which ends it (ended after 10 s), and deletes the copy of the host and its config.
				_probeProcess.Dispose();
				Results.Log("Session process ended");
			}
			catch(Exception ex) {
				Results.Log("Session process not ended: "+ex.GetType().Name+": "+ex.Message);
			}
			_probeProcess=null;
		}
	}
}
