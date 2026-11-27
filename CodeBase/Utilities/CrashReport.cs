// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CodeBase {
	///<summary>FB-1: one text file per unhandled exception, with what is needed to debug it later: the exception chain (type, message,
	///HResult, data, stack of every inner exception), the program's version and build, Windows and .NET, the process's memory, handle, GDI and
	///USER object counts, the thread, the program's state (module, windows open; given by FuncAppState) and the last things the user did
	///(Note). Before, a crash that ended the process (a WPF exception on the UI thread, for example the tooltip crash of 2026-11-25) left only
	///Windows' own event log entry, and the handled paths wrote one line of exception text.
	///
	///Files go to %LOCALAPPDATA%\OpenPMS-Dental\CrashReports (always writable, unlike the program folder), named crash-yyyyMMdd-HHmmss-fff-pid.txt;
	///the newest ReportsKept are kept. A line naming the file also goes to the Logger's "Crash" folder. Nothing is sent anywhere. No patient
	///name, no password and no command-line value is written: the state lines carry numbers and type names only.
	///Never throws: a report that cannot be written is skipped.</summary>
	public static class CrashReport {
		#region Fields
		///<summary>The last things done, oldest first.</summary>
		public const int BreadcrumbCount=60;
		///<summary>Reports kept in the folder; older ones are deleted when a new one is written.</summary>
		public const int ReportsKept=50;
		private static readonly object _lock=new object();
		private static readonly Queue<string> _queueBreadcrumbs=new Queue<string>();
		///<summary>The last exception reported, so the same exception reaching two handlers (the WPF dispatcher, then the AppDomain) gives one
		///report.</summary>
		private static Exception _exceptionLast;
		[ThreadStatic]
		private static bool _isWriting;
		#endregion Fields

		#region Properties
		///<summary>The program's state for the report: lines such as "Module: Imaging", "PatNum: 42", "Windows open: FormOpenDental, FormVideo".
		///Set by the program at start. Called on the thread that crashed; it must not show anything, and what it throws is caught.</summary>
		public static Func<List<string>> FuncAppState { get; set; }

		///<summary>For tests: the folder to write to instead of %LOCALAPPDATA%\OpenPMS-Dental\CrashReports. Null for the default.</summary>
		public static string FolderOverride { get; set; }
		#endregion Properties

		#region Public methods
		///<summary>The folder reports go to.</summary>
		public static string GetFolder() {
			if(!string.IsNullOrEmpty(FolderOverride)) {
				return FolderOverride;
			}
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),ProgramNames.ProgramDataFolderName,"CrashReports");
		}

		///<summary>Remembers one thing the user or the program did ("Module: Imaging", "Window opened: FormVideo"), for the next report. Cheap and
		///thread-safe; keeps the last BreadcrumbCount.</summary>
		public static void Note(string text) {
			string line=DateTime.Now.ToString("HH:mm:ss.fff",CultureInfo.InvariantCulture)+" ["+Thread.CurrentThread.ManagedThreadId+"] "+(text??"");
			lock(_lock) {
				_queueBreadcrumbs.Enqueue(line);
				while(_queueBreadcrumbs.Count>BreadcrumbCount) {
					_queueBreadcrumbs.Dequeue();
				}
			}
		}

		///<summary>The remembered notes, oldest first.</summary>
		public static List<string> GetBreadcrumbs() {
			lock(_lock) {
				return _queueBreadcrumbs.ToList();
			}
		}

		///<summary>Writes a report for ex and returns its path, or "" when nothing was written (no exception, the same exception again, a report
		///already being written on this thread, or the file could not be written). source names the handler ("AppDomain.UnhandledException");
		///isTerminating says whether the process ends now.</summary>
		public static string Write(Exception ex,string source,bool isTerminating) {
			if(ex==null || _isWriting) {
				return "";
			}
			lock(_lock) {
				if(ReferenceEquals(ex,_exceptionLast)) {
					return "";
				}
				_exceptionLast=ex;
			}
			_isWriting=true;
			try {
				DateTime dateTimeNow=DateTime.Now;
				string text=GetText(ex,source,isTerminating,dateTimeNow);
				string folder=GetFolder();
				Directory.CreateDirectory(folder);
				string path=Path.Combine(folder,"crash-"+dateTimeNow.ToString("yyyyMMdd-HHmmss-fff",CultureInfo.InvariantCulture)
					+"-"+Environment.ProcessId.ToString(CultureInfo.InvariantCulture)+".txt");
				File.WriteAllText(path,text,Encoding.UTF8);
				PruneOldReports(folder);
				ODException.SwallowAnyException(() => Logger.WriteLine(source+": "+ex.GetType().FullName+": "+ex.Message+" Crash report: "+path,"Crash"));
				return path;
			}
			catch(Exception exWrite) {
				exWrite.DoNothing();
				return "";
			}
			finally {
				_isWriting=false;
			}
		}

		///<summary>The report's text. Each section is written on its own, so one that fails still leaves the others.</summary>
		public static string GetText(Exception ex,string source,bool isTerminating,DateTime dateTimeNow) {
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.AppendLine("===== Crash report =====");
			stringBuilder.AppendLine("Time: "+dateTimeNow.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture)
				+" local, "+dateTimeNow.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture)+" UTC");
			stringBuilder.AppendLine("Caught by: "+(source??"")+(isTerminating ? " (the process ends)" : ""));
			stringBuilder.AppendLine("Exception: "+(ex==null ? "(none)" : ex.GetType().FullName+": "+ex.Message));
			AppendSection(stringBuilder,"Thread",AppendThread);
			AppendSection(stringBuilder,"Program",AppendProgram);
			AppendSection(stringBuilder,"Computer",AppendComputer);
			AppendSection(stringBuilder,"Process",AppendProcess);
			AppendSection(stringBuilder,"Program state",AppendAppState);
			AppendSection(stringBuilder,"Last actions (oldest first)",x => {
				foreach(string line in GetBreadcrumbs()) {
					x.AppendLine(line);
				}
			});
			AppendSection(stringBuilder,"Exception detail",x => AppendException(x,ex,0,""));
			AppendSection(stringBuilder,"Exception.ToString()",x => x.AppendLine(ex?.ToString()??""));
			return stringBuilder.ToString();
		}
		#endregion Public methods

		#region Sections
		private static void AppendSection(StringBuilder stringBuilder,string title,Action<StringBuilder> actionAppend) {
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("--- "+title+" ---");
			try {
				actionAppend(stringBuilder);
			}
			catch(Exception ex) {
				stringBuilder.AppendLine("(not available: "+ex.GetType().Name+": "+ex.Message+")");
			}
		}

		private static void AppendThread(StringBuilder stringBuilder) {
			Thread thread=Thread.CurrentThread;
			stringBuilder.AppendLine("Managed id: "+thread.ManagedThreadId+", name: "+(thread.Name??"(none)")+", background: "+thread.IsBackground
				+", apartment: "+thread.GetApartmentState()+", thread pool: "+thread.IsThreadPoolThread);
			stringBuilder.AppendLine("Culture: "+thread.CurrentCulture.Name+", UI culture: "+thread.CurrentUICulture.Name);
		}

		private static void AppendProgram(StringBuilder stringBuilder) {
			string pathExe=Environment.ProcessPath??"";
			stringBuilder.AppendLine("Executable: "+pathExe);
			if(pathExe!="" && File.Exists(pathExe)) {
				FileVersionInfo fileVersionInfo=FileVersionInfo.GetVersionInfo(pathExe);
				stringBuilder.AppendLine("File version: "+fileVersionInfo.FileVersion+", product version: "+fileVersionInfo.ProductVersion);
				stringBuilder.AppendLine("Executable written: "+File.GetLastWriteTime(pathExe).ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture));
			}
			stringBuilder.AppendLine("Build: "+(ODBuild.IsDebug() ? "Debug" : "Release")+", "+(Environment.Is64BitProcess ? "64-bit" : "32-bit")+" process");
			//Only the names of the arguments: a value can be a user name or a password.
			string[] arrayArgs=Environment.GetCommandLineArgs().Skip(1).ToArray();
			stringBuilder.AppendLine("Arguments: "+(arrayArgs.Length==0 ? "(none)"
				: string.Join(" ",arrayArgs.Select(x => x.Contains("=") ? x.Substring(0,x.IndexOf('='))+"=..." : "(value)"))));
		}

		private static void AppendComputer(StringBuilder stringBuilder) {
			stringBuilder.AppendLine("Windows: "+RuntimeInformation.OSDescription+" ("+Environment.OSVersion.Version+"), "+RuntimeInformation.OSArchitecture);
			stringBuilder.AppendLine(".NET: "+RuntimeInformation.FrameworkDescription);
			stringBuilder.AppendLine("Computer: "+Environment.MachineName+", user: "+Environment.UserName+", processors: "+Environment.ProcessorCount
				+", remote session: "+System.Windows.Forms.SystemInformation.TerminalServerSession);
			stringBuilder.AppendLine("Screens: "+string.Join("; ",System.Windows.Forms.Screen.AllScreens.Select(x => x.Bounds.Width+"x"+x.Bounds.Height
				+(x.Primary ? " primary" : ""))));
		}

		private static void AppendProcess(StringBuilder stringBuilder) {
			using Process process=Process.GetCurrentProcess();
			stringBuilder.AppendLine("Pid: "+process.Id+", started: "+process.StartTime.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture)
				+", running for: "+(DateTime.Now-process.StartTime).ToString(@"d\.hh\:mm\:ss",CultureInfo.InvariantCulture));
			stringBuilder.AppendLine("Memory: working set "+ToMB(process.WorkingSet64)+", private "+ToMB(process.PrivateMemorySize64)
				+", managed heap "+ToMB(GC.GetTotalMemory(false))+", GC counts "+GC.CollectionCount(0)+"/"+GC.CollectionCount(1)+"/"+GC.CollectionCount(2));
			stringBuilder.AppendLine("Handles: "+process.HandleCount+", threads: "+process.Threads.Count
				+", GDI objects: "+GetGuiResources(process.Handle,0)+", USER objects: "+GetGuiResources(process.Handle,1)
				+" (Windows ends a process at 10000 of either)");
		}

		private static void AppendAppState(StringBuilder stringBuilder) {
			Func<List<string>> funcAppState=FuncAppState;
			if(funcAppState==null) {
				stringBuilder.AppendLine("(not given)");
				return;
			}
			foreach(string line in funcAppState()??new List<string>()) {
				stringBuilder.AppendLine(line);
			}
		}

		///<summary>Every exception of the chain: type, message, HResult, source, method, data and stack; the inner exceptions after it, each
		///AggregateException's all of them, at most 10 deep.</summary>
		private static void AppendException(StringBuilder stringBuilder,Exception ex,int depth,string label) {
			if(ex==null) {
				return;
			}
			if(depth>=10) {
				stringBuilder.AppendLine("(more inner exceptions not shown)");
				return;
			}
			if(depth>0) {
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("Inner exception "+label+":");
			}
			stringBuilder.AppendLine("Type: "+ex.GetType().FullName);
			stringBuilder.AppendLine("Message: "+ex.Message);
			stringBuilder.AppendLine("HResult: 0x"+ex.HResult.ToString("X8",CultureInfo.InvariantCulture));
			stringBuilder.AppendLine("Source: "+(ex.Source??"")+", method: "+(SafeTargetSite(ex)??""));
			if(ex.Data!=null && ex.Data.Count>0) {
				foreach(DictionaryEntry dictionaryEntry in ex.Data) {
					string value=dictionaryEntry.Value?.ToString()??"";
					stringBuilder.AppendLine("Data: "+dictionaryEntry.Key+" = "+(value.Length>500 ? value.Substring(0,500)+"..." : value));
				}
			}
			stringBuilder.AppendLine("Stack:");
			stringBuilder.AppendLine(ex.StackTrace??"(none)");
			if(ex is AggregateException aggregateException) {
				for(int i=0;i<aggregateException.InnerExceptions.Count;i++) {
					AppendException(stringBuilder,aggregateException.InnerExceptions[i],depth+1,(label=="" ? "" : label+".")+(i+1));
				}
				return;
			}
			AppendException(stringBuilder,ex.InnerException,depth+1,(label=="" ? "" : label+".")+"1");
		}
		#endregion Sections

		#region Helpers
		private static string SafeTargetSite(Exception ex) {
			try {
				return ex.TargetSite==null ? null : ex.TargetSite.DeclaringType?.FullName+"."+ex.TargetSite.Name;
			}
			catch {
				return null;
			}
		}

		private static string ToMB(long bytes) {
			return (bytes/(1024.0*1024.0)).ToStringNetFx("0.0",CultureInfo.InvariantCulture)+" MB";
		}

		private static void PruneOldReports(string folder) {
			List<FileInfo> listFiles=new DirectoryInfo(folder).GetFiles("crash-*.txt").OrderByDescending(x => x.Name).ToList();
			foreach(FileInfo fileInfo in listFiles.Skip(ReportsKept)) {
				ODException.SwallowAnyException(() => fileInfo.Delete());
			}
		}

		///<summary>uiFlags 0: GDI objects, 1: USER objects.</summary>
		[DllImport("user32.dll")]
		private static extern int GetGuiResources(IntPtr hProcess,int uiFlags);
		#endregion Helpers
	}
}
