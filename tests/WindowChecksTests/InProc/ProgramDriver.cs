// The in-process half of the harness: the program's own assemblies, in the program's own process started from OpenPmsDental\bin\Debug through
// its apphost with TestProbeHost's startup hook (so the folder's runtime, OpenPmsDental.runtimeconfig.json, .deps.json and .dll.config are the
// real ones and OpenPmsDental.dll is the entry assembly; InProcHost, WINCHK-5; a copy of TestProbeHost with OpenPmsDental.exe.config on .NET
// Framework, an AppDomain of the test host before S3a-8b of the .NET 10 move), run the main window on an STA thread as
// ProgramEntry.Main does, and the checks drive it from a second thread of that process: they post what a click would run (the
// real menu item's PerformClick, the toolbar's and module bar's handlers) to the window's thread, wait for the windows it opens, read and
// print them, and close them. Every unhandled exception (Application.ThreadException, AppDomain.UnhandledException, ODThread's, WPF's
// Dispatcher) is collected and fails the check that was running.
// The main window is never closed through the program's own closing (its FormClosed ends the process with Environment.Exit): Stop removes
// those handlers and ends the window's thread, and the process is ended. They are removed before the harness closes any window left open
// (Stop, a timed-out or stopped check), and the log-on window is never cancelled (Ui.Close logs on as Admin instead), because a cancelled log-on
// window reaches the program's exit too.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using TestProbeHost;

namespace WindowChecksTests.InProc {
	///<summary>The session process's handler (TestProbeHost): one ProgramDriver per process, and one command per call of the test's InProcHost.
	///Only text crosses: lists as ProbeText lists, a check's result as CheckResult.ToText.</summary>
	public static class ProgramDriverHost {
		public const string COMMAND_PATCH="Patch";
		public const string COMMAND_START="Start";
		public const string COMMAND_SET_KEYS="SetKeys";
		public const string COMMAND_RUN_CHECK="RunCheck";
		public const string COMMAND_GET_STUCK_STATE="GetStuckState";
		public const string COMMAND_STOP="Stop";
		///<summary>The location of this process's Assembly.GetEntryAssembly() ("" for none), read after Patch (S3a-FU2).</summary>
		public const string COMMAND_GET_ENTRY_ASSEMBLY="GetEntryAssembly";
		///<summary>What .NET read for this process (WINCHK-5): a list of name=value lines (ProgramDriver.GetRuntimeLines); the arguments are the
		///names of the runtimeconfig properties to read back.</summary>
		public const string COMMAND_GET_RUNTIME="GetRuntime";
		private static readonly ProgramDriver _programDriver=new ProgramDriver();

		public static string Handle(string command,string[] arrayArgs) {
			switch(command) {
				case COMMAND_PATCH:
					return _programDriver.PatchProcessIdentity(arrayArgs[0]);
				case COMMAND_GET_ENTRY_ASSEMBLY:
					return Assembly.GetEntryAssembly()?.Location??"";
				case COMMAND_GET_RUNTIME:
					return ProbeText.JoinList(ProgramDriver.GetRuntimeLines(ProbeText.SplitList(arrayArgs[0])));
				case COMMAND_START:
					string startError=_programDriver.Start(ProbeText.SplitList(arrayArgs[0]),int.Parse(arrayArgs[1],CultureInfo.InvariantCulture),
						ProbeText.SplitList(arrayArgs[2]));
					return ProbeText.JoinList(new[] { startError }.Concat(_programDriver.ListStartPrompts));
				case COMMAND_SET_KEYS:
					string[] arrayKeys=ProbeText.SplitList(arrayArgs[0]);
					Dictionary<string,long> dictKeys=new Dictionary<string,long>();
					for(int i=0;i<arrayKeys.Length;i+=2) {
						dictKeys[arrayKeys[i]]=long.Parse(arrayKeys[i+1],CultureInfo.InvariantCulture);
					}
					_programDriver.SetKeys(dictKeys);
					return "";
				case COMMAND_RUN_CHECK:
					return _programDriver.RunCheck(arrayArgs[0],arrayArgs[1],int.Parse(arrayArgs[2],CultureInfo.InvariantCulture)).ToText();
				case COMMAND_GET_STUCK_STATE:
					return _programDriver.GetStuckState();
				case COMMAND_STOP:
					return _programDriver.Stop(int.Parse(arrayArgs[0],CultureInfo.InvariantCulture));
				default:
					throw new ArgumentException("Unknown command "+command);
			}
		}
	}

	///<summary>Lives in the session's process; the test's InProcHost reaches it through ProgramDriverHost. Only strings and results as text cross.
	///</summary>
	public class ProgramDriver {
		private Thread _threadUi;
		private Form _formMain;
		private int _threadIdUi;
		private readonly ManualResetEvent _manualResetEventShown=new ManualResetEvent(false);
		private readonly ManualResetEvent _manualResetEventUiEnded=new ManualResetEvent(false);
		private string _startError="";
		private readonly List<string> _listExceptions=new List<string>();
		private readonly object _lock=new object();
		///<summary>Set on the window's thread once the main window's FormClosing and FormClosed handlers are removed (DetachMainClosing).</summary>
		private volatile bool _isMainClosingDetached;
		public Ui UiCur;

		///<summary>Makes sure this process looks like the program's own where .NET reads it from the process: Application.StartupPath and
		///ExecutablePath (the folder and the exe given) and Assembly.GetEntryAssembly (OpenPmsDental.dll of that folder). In the session's process
		///(the program's own, InProcHost.StartSessionProcess, WINCHK-5) all three are the program's from the start and nothing is patched; a value
		///that differs (a host that is not the program, as TestProbeHost's copy was on .NET Framework) is set. Returns each item with "(own)" or
		///"(patched)", the file OpenPmsDental loaded from and the config file System.Configuration reads.</summary>
		public string PatchProcessIdentity(string exePath) {
			List<string> listItems=new List<string>();
			//The fields' names on .NET Framework (startupPath, executablePath) and on .NET (s_startupPath, s_executablePath).
			FieldInfo fieldStartupPath=typeof(Application).GetField("startupPath",BindingFlags.NonPublic|BindingFlags.Static)
				??typeof(Application).GetField("s_startupPath",BindingFlags.NonPublic|BindingFlags.Static);
			FieldInfo fieldExecutablePath=typeof(Application).GetField("executablePath",BindingFlags.NonPublic|BindingFlags.Static)
				??typeof(Application).GetField("s_executablePath",BindingFlags.NonPublic|BindingFlags.Static);
			string folder=Path.GetDirectoryName(exePath);
			//The public properties fill the fields on first use, from the process's own exe.
			if(string.Equals(Application.StartupPath.TrimEnd('\\'),folder.TrimEnd('\\'),StringComparison.OrdinalIgnoreCase)) {
				listItems.Add("Application.StartupPath (own)");
			}
			else if(fieldStartupPath!=null) {
				fieldStartupPath.SetValue(null,folder);
				listItems.Add("Application.StartupPath (patched)");
			}
			if(string.Equals(Application.ExecutablePath,exePath,StringComparison.OrdinalIgnoreCase)) {
				listItems.Add("Application.ExecutablePath (own)");
			}
			else if(fieldExecutablePath!=null) {
				fieldExecutablePath.SetValue(null,exePath);
				listItems.Add("Application.ExecutablePath (patched)");
			}
			Assembly assemblyExe=Assembly.Load("OpenPmsDental");
			//.NET has no AppDomainManager to patch (the .NET Framework way, before S3b-5); Assembly.SetEntryAssembly (.NET 9 and later) sets what
			//Assembly.GetEntryAssembly returns. In the program's own process it is the program's already.
			if(Assembly.GetEntryAssembly()==assemblyExe) {
				listItems.Add("Assembly.GetEntryAssembly (own)");
			}
			else {
				Assembly.SetEntryAssembly(assemblyExe);
				listItems.Add("Assembly.GetEntryAssembly (patched)");
			}
			//The application config: on .NET, System.Configuration reads APP_CONFIG_FILE when the host set it, else the entry assembly's .dll.config.
			string config=AppContext.GetData("APP_CONFIG_FILE") as string;
			return string.Join(", ",listItems)+"; OpenPmsDental loaded from "+assemblyExe.Location+"; config "
				+(config??(assemblyExe.Location+".config"));
		}

		///<summary>What .NET read for this process, as name=value lines (WINCHK-5): ProcessPath, BaseDirectory, CoreLib and WindowsForms (the
		///files of the runtime it runs on: a self-contained program's are in its folder), Framework, Is64BitProcess, DepsFiles
		///(APP_CONTEXT_DEPS_FILES, the .deps.json the host read), Program (where OpenPmsDental loads from), StartupHooksVariable (empty: the hook
		///removed it), ConfigFile (the file System.Configuration reads for the exe). Then, for each name, "Property.name=" the runtimeconfig
		///property's value as the process sees it ("(none)" when absent), or for "Setting:name" "Setting.name=" the program's Properties.Settings
		///value of that name, read through System.Configuration.</summary>
		public static List<string> GetRuntimeLines(string[] arrayNames) {
			List<string> listLines=new List<string>();
			listLines.Add("ProcessPath="+Environment.ProcessPath);
			listLines.Add("BaseDirectory="+AppContext.BaseDirectory);
			listLines.Add("CoreLib="+typeof(object).Assembly.Location);
			listLines.Add("WindowsForms="+typeof(Form).Assembly.Location);
			listLines.Add("Framework="+System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
			listLines.Add("Is64BitProcess="+Environment.Is64BitProcess);
			listLines.Add("DepsFiles="+(AppContext.GetData("APP_CONTEXT_DEPS_FILES") as string??""));
			Assembly assemblyProgram=Assembly.Load("OpenPmsDental");
			listLines.Add("Program="+assemblyProgram.Location);
			listLines.Add("StartupHooksVariable="+(Environment.GetEnvironmentVariable(ProbeProcess.VARIABLE_STARTUP_HOOKS)??""));
			//System.Configuration.ConfigurationManager (a package on .NET, in the program's .deps.json), by reflection: this assembly needs no
			//reference to it.
			Type typeManager=Type.GetType("System.Configuration.ConfigurationManager, System.Configuration.ConfigurationManager",true);
			Type typeLevel=Type.GetType("System.Configuration.ConfigurationUserLevel, System.Configuration.ConfigurationManager",true);
			object configuration=typeManager.GetMethod("OpenExeConfiguration",new[] { typeLevel }).Invoke(null,new[] { Enum.ToObject(typeLevel,0) });
			listLines.Add("ConfigFile="+configuration.GetType().GetProperty("FilePath").GetValue(configuration));
			foreach(string name in arrayNames) {
				if(name.StartsWith("Setting:")) {
					string nameSetting=name.Substring("Setting:".Length);
					Type typeSettings=assemblyProgram.GetType("OpenPmsDental.Properties.Settings",true);
					object settings=typeSettings.GetProperty("Default",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static).GetValue(null);
					//A setting the config file lists but the Settings class does not declare is not readable (SettingsPropertyNotFoundException).
					System.Collections.IEnumerable enumerableProperties=(System.Collections.IEnumerable)typeSettings.GetProperty("Properties").GetValue(settings);
					if(!enumerableProperties.Cast<object>().Any(x => (string)x.GetType().GetProperty("Name").GetValue(x)==nameSetting)) {
						listLines.Add("Setting."+nameSetting+"=(not declared)");
						continue;
					}
					object value=typeSettings.GetProperty("Item",new[] { typeof(string) }).GetValue(settings,new object[] { nameSetting });
					listLines.Add("Setting."+nameSetting+"="+(value?.ToString()??"(none)"));
					continue;
				}
				listLines.Add("Property."+name+"="+(AppContext.GetData(name)?.ToString()??"(none)"));
			}
			return listLines;
		}

		///<summary>Starts the main window with the command line, as ProgramEntry.Main does, and waits until it is shown and the user is logged on.
		///Returns "" or the problem.</summary>
		///<summary>Message boxes the start-up showed and the harness answered with Cancel (cancelTexts), as "text".</summary>
		public List<string> ListStartPrompts=new List<string>();

		public string Start(string[] args,int timeoutSeconds) {
			return Start(args,timeoutSeconds,new string[0]);
		}

		///<summary>As Start; a start-up message box that contains one of cancelTexts is answered with Cancel (BM_CLICK) and recorded in
		///ListStartPrompts. Any other window the start-up opens is a problem.</summary>
		public string Start(string[] args,int timeoutSeconds,string[] cancelTexts) {
			AppDomain.CurrentDomain.UnhandledException+=(sender,e) => AddException("AppDomain.UnhandledException",e.ExceptionObject as Exception);
			_threadUi=new Thread(() => RunUi(args)) { Name="winchk UI",IsBackground=true };
			_threadUi.SetApartmentState(ApartmentState.STA);
			_threadUi.Start();
			Stopwatch stopwatchShown=Stopwatch.StartNew();
			//A prompt can come before the window shows (the release check runs in Shown, before the log-on).
			while(!_manualResetEventShown.WaitOne(200)) {
				if(stopwatchShown.Elapsed.TotalSeconds>timeoutSeconds) {
					return "The main window was not shown within "+timeoutSeconds+" s."+(_startError=="" ? "" : " "+_startError);
				}
				AnswerStartPrompts(cancelTexts);
			}
			if(_startError!="") {
				return _startError;
			}
			UiCur=new Ui(this,_formMain,_threadIdUi);
			//The log-on runs in Shown, after the window shows: wait for the user and patient #1.
			Stopwatch stopwatch=Stopwatch.StartNew();
			while(stopwatch.Elapsed.TotalSeconds<timeoutSeconds) {
				bool isReady=UiCur.Invoke(() => InProcChecks.IsLoggedOnWithPatient());
				if(isReady) {
					return "";
				}
				AnswerStartPrompts(cancelTexts);
				//A window the start-up opened (a message, Choose Database) is a problem: name it.
				IntPtr hwndMain=UiCur.Invoke(() => _formMain.Handle);
				foreach(IntPtr hwnd in Native.GetThreadWindows(_threadIdUi).Where(x => x!=hwndMain)) {
					string text=UiCur.DescribeWindow(hwnd);
					if(text!="") {
						return "The start-up opened a window: "+text;
					}
				}
				Thread.Sleep(300);
			}
			return "Not logged on with patient #1 within "+timeoutSeconds+" s.";
		}

		private void AnswerStartPrompts(string[] cancelTexts) {
			if(_threadIdUi==0) {
				return;
			}
			foreach(IntPtr hwnd in Native.GetThreadWindows(_threadIdUi).Where(x => Native.IsDialogClass(x))) {
				string text=Native.GetDialogMessage(hwnd);
				if(cancelTexts.Any(x => text.Contains(x))) {
					ListStartPrompts.Add(text);
					Native.ClickDialogButton(hwnd,"Cancel");
					Thread.Sleep(500);
				}
			}
		}

		private void RunUi(string[] args) {
			try {
				_threadIdUi=Native.GetCurrentThreadId();
				InProcChecks.InitializeAsProgramEntry();
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
				Application.ThreadException+=(sender,e) => AddException("Application.ThreadException",e.Exception);
				_formMain=InProcChecks.CreateMainWindow(args,(ex,threadName) => AddException("ODThread "+threadName,ex));
				_formMain.Shown+=(sender,e) => _manualResetEventShown.Set();
				InProcChecks.HookDispatcher((ex) => AddException("Dispatcher.UnhandledException",ex));
				Application.Run(_formMain);
			}
			catch(Exception ex) {
				_startError="The main window failed: "+ex;
				_manualResetEventShown.Set();
			}
			finally {
				_manualResetEventUiEnded.Set();
			}
		}

		///<summary>The program's own aborts of its worker threads (ODThread.QuitSync aborts a thread that does not end in time): noted, not failed; a
		///person sees nothing of them. Only an abort that ODThread reports (source "ODThread ..."), or one of an ODThread's own thread (its stack runs
		///in ODThread.Run) that reaches AppDomain.UnhandledException, is one of these; any other abort (Application.ThreadException, the Dispatcher,
		///another thread) is an unhandled exception like any other.</summary>
		private readonly List<string> _listAborts=new List<string>();

		public void AddException(string source,Exception ex) {
			//An ODThread's worker aborted by ODThread.QuitSync can also reach AppDomain.UnhandledException; its stack shows it is ODThread's own thread.
			bool isODThreadAbort=ex is ThreadAbortException && (source.StartsWith("ODThread ")
				|| (source=="AppDomain.UnhandledException" && (ex.StackTrace??"").Contains("CodeBase.ODThread.Run(")));
			if(isODThreadAbort) {
				lock(_lock) {
					_listAborts.Add(source+": "+ex.GetType().FullName+" "+FirstFrames(ex));
				}
				return;
			}
			lock(_lock) {
				_listExceptions.Add(source+": "+(ex==null ? "(no exception object)" : ex.GetType().FullName+": "+ex.Message+" "+FirstFrames(ex)));
			}
		}

		private static string FirstFrames(Exception ex) {
			string stack=(ex.GetBaseException().StackTrace??"").Replace("\r","");
			return "["+string.Join(" / ",stack.Split('\n').Select(x => x.Trim()).Where(x => x!="").Take(6))+"]";
		}

		public int CountExceptions {
			get {
				lock(_lock) {
					return _listExceptions.Count;
				}
			}
		}

		public List<string> GetExceptionsFrom(int index) {
			lock(_lock) {
				return _listExceptions.Skip(index).ToList();
			}
		}

		///<summary>The row numbers earlier checks made (CheckResult.DictKeys), from the test's side, before each check: this session's copy of them.
		///</summary>
		public void SetKeys(Dictionary<string,long> dictKeys) {
			InProcChecks.SetKeys(dictKeys);
		}

		///<summary>After a check that did not end: "" when the window's thread still answers and no window but the main window is open; otherwise
		///what is stuck. The rest of the session's checks would hang on it, so the caller starts a new session.</summary>
		public string GetStuckState() {
			if(UiCur==null) {
				return "no session";
			}
			try {
				UiCur.Invoke(() => true,15000);
				List<IntPtr> listWindows=UiCur.GetWindows();
				if(listWindows.Count>0) {
					return "windows still open: "+string.Join("; ",listWindows.Select(x => UiCur.DescribeWindow(x)));
				}
				return "";
			}
			catch(Exception ex) {
				return "the window's thread does not answer ("+ex.Message+")";
			}
		}

		///<summary>Runs one check's in-process script on a thread of this process and returns its result. A script that does not end in time is
		///reported as failed (its windows are closed by Stop).</summary>
		public CheckResult RunCheck(string id,string title,int timeoutSeconds) {
			CheckResult checkResult=new CheckResult(id,title);
			int countExceptionsBefore=CountExceptions;
			int countAbortsBefore;
			lock(_lock) {
				countAbortsBefore=_listAborts.Count;
			}
			Exception exceptionScript=null;
			Thread thread=new Thread(() => {
				try {
					UiCur.CheckResultCur=checkResult;
					InProcChecks.Run(id,UiCur,checkResult);
				}
				catch(ThreadAbortException) {
				}
				catch(Exception ex) {
					exceptionScript=ex;
				}
			}) { Name="winchk check "+id,IsBackground=true };
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			if(!thread.Join(TimeSpan.FromSeconds(timeoutSeconds))) {
				checkResult.Fail("The check did not end within "+timeoutSeconds+" s (a window hung or an expected window never came)");
				//.NET cannot abort a thread (Thread.Abort throws PlatformNotSupportedException, SYSLIB0006; the .NET Framework build aborted it here):
				//the script's background thread is left waiting, its windows are closed below, and InProcHost asks whether the session is stuck and
				//starts a new one for the next check when it is (WINCHK-5).
				DetachMainClosing(checkResult);
				UiCur.CloseStrayWindows(checkResult);
			}
			if(exceptionScript!=null) {
				checkResult.Fail("The check's script stopped: "+exceptionScript.GetType().Name+": "+exceptionScript.Message+" "+FirstFrames(exceptionScript));
				DetachMainClosing(checkResult);
				UiCur.CloseStrayWindows(checkResult);
			}
			foreach(string exception in GetExceptionsFrom(countExceptionsBefore)) {
				checkResult.Fail("Unhandled exception: "+exception);
			}
			lock(_lock) {
				foreach(string abort in _listAborts.Skip(countAbortsBefore)) {
					checkResult.Note("A worker thread of the program was aborted (not shown to a person; ODThread aborts a thread that does not quit in time): "+abort);
				}
			}
			return checkResult;
		}

		///<summary>Ends the window's thread without the program's own closing (no Environment.Exit), and waits for it.</summary>
		public string Stop(int timeoutSeconds) {
			if(_formMain==null || _threadUi==null) {
				return "not started";
			}
			try {
				//First, so that no window closed below can reach the program's own exit (Environment.Exit would end the session's process).
				DetachMainClosing(null);
				UiCur?.CloseStrayWindows(null);
				_formMain.BeginInvoke((Action)(() => {
					RemoveEventHandlers(_formMain,"EVENT_FORMCLOSING");
					RemoveEventHandlers(_formMain,"EVENT_FORMCLOSED");
					Application.ExitThread();
				}));
			}
			catch(Exception ex) {
				return "Stop failed: "+ex.Message;
			}
			if(!_manualResetEventUiEnded.WaitOne(TimeSpan.FromSeconds(timeoutSeconds))) {
				return "The window's thread did not end within "+timeoutSeconds+" s.";
			}
			return "";
		}

		///<summary>Removes the main window's FormClosing and FormClosed handlers on the window's thread and waits for it (up to 30 s; a hung thread
		///is noted). Done before the harness closes windows left open (a timed-out or stopped check, Stop): a log-on window closed without OK makes
		///FormOpenDental.ShowLogOn call Application.Exit, and FormOpenDental_FormClosed ends with Environment.Exit, which ends the session's process
		///(the test host itself before S3a-8b). The rest of the session's checks never close the main window, so they do not need them.</summary>
		private void DetachMainClosing(CheckResult checkResult) {
			if(_isMainClosingDetached || _formMain==null) {
				return;
			}
			try {
				IAsyncResult asyncResult=_formMain.BeginInvoke((Action)(() => {
					RemoveEventHandlers(_formMain,"EVENT_FORMCLOSING");
					RemoveEventHandlers(_formMain,"EVENT_FORMCLOSED");
					_isMainClosingDetached=true;
				}));
				if(!asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(30))) {
					checkResult?.Note("The main window's closing could not be detached within 30 s (its thread did not run the call)");
				}
			}
			catch(Exception ex) {
				checkResult?.Note("The main window's closing was not detached: "+ex.GetType().Name+": "+ex.Message);
			}
		}

		///<summary>The .NET Framework's names of Form's event keys and the names .NET's Windows Forms gives the same keys (WINCHK-5: on .NET 10 the
		///.NET Framework name found no field, so the closing was never detached and Stop threw on the window's thread).</summary>
		private static readonly Dictionary<string,string> _dictEventKeysNet=new Dictionary<string,string> {
			{ "EVENT_FORMCLOSING","s_formClosingEvent" },
			{ "EVENT_FORMCLOSED","s_formClosedEvent" },
		};

		///<summary>Removes every handler of the form's event (EVENT_FORMCLOSING or EVENT_FORMCLOSED, by the .NET Framework's key name) and returns
		///how many delegates were removed. Throws when this runtime's Form has no such key (HarnessTests checks it does).</summary>
		public static int RemoveEventHandlers(Component component,string keyName) {
			FieldInfo fieldKey=typeof(Form).GetField(keyName,BindingFlags.NonPublic|BindingFlags.Static)
				??typeof(Form).GetField(_dictEventKeysNet[keyName],BindingFlags.NonPublic|BindingFlags.Static);
			if(fieldKey==null) {
				throw new MissingFieldException(typeof(Form).FullName,keyName+" / "+_dictEventKeysNet[keyName]);
			}
			PropertyInfo propertyEvents=typeof(Component).GetProperty("Events",BindingFlags.NonPublic|BindingFlags.Instance);
			EventHandlerList eventHandlerList=(EventHandlerList)propertyEvents.GetValue(component);
			object key=fieldKey.GetValue(null);
			Delegate handler=eventHandlerList[key];
			if(handler==null) {
				return 0;
			}
			eventHandlerList.RemoveHandler(key,handler);
			return handler.GetInvocationList().Length;
		}
	}
}
