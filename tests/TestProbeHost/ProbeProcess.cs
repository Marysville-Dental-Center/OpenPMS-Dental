// The test's side of a child process (S3a-8 of the .NET 10 move, docs/port-notes/S3a-8.md): starts TestProbeHost from a folder and with a config
// of the test's choosing, sends it requests and reads its replies, and ends it and deletes its files afterwards. In place of a new AppDomain,
// which .NET cannot create: a fresh process starts with empty static state, its own config (binding redirects, trace sources) and its own
// application base, as the AppDomains of these tests did.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TestProbeHost {
	///<summary>What a process of ProbeProcess.RunOnce did.</summary>
	public class ProbeRunResult {
		///<summary>The process's exit code; -1 when it was ended after the timeout.</summary>
		public int ExitCode;
		public bool IsTimedOut;
		///<summary>Standard output and standard error as they came, each line of standard error after "stderr: ".</summary>
		public string Output;
		///<summary>From the process's start to its end (or to the timeout).</summary>
		public TimeSpan Elapsed;
	}

	///<summary>A child process that runs a test's handler. Start it in a using block; every Call is answered or throws.</summary>
	public sealed class ProbeProcess:IDisposable {
		private readonly Process _process;
		private readonly string _pathExe;
		private readonly string _pathConfig;
		private readonly TimeSpan _timeSpanTimeout;
		private readonly StringBuilder _stringBuilderError=new StringBuilder();
		private readonly object _lockError=new object();
		private bool _isDisposed;

		private ProbeProcess(Process process,string pathExe,string pathConfig,TimeSpan timeSpanTimeout) {
			_process=process;
			_pathExe=pathExe;
			_pathConfig=pathConfig;
			_timeSpanTimeout=timeSpanTimeout;
		}

		///<summary>What the child wrote to standard error (its console output included), for messages.</summary>
		public string ErrorOutput {
			get {
				lock(_lockError) {
					return _stringBuilderError.ToString();
				}
			}
		}

		///<summary>Starts the child with folderBase as its application base (a copy of TestProbeHost.exe is written there under a name of its own,
		///so its config is its alone; the copies a test process that died left there are deleted first) and configText as its config (null for
		///none). It loads the assembly of typeHandler and calls typeHandler's
		///public static string Handle(string command,string[] args) for each Call. Each wait for the child, its start included, ends after
		///timeSpanTimeout, and the child is then ended.</summary>
		public static ProbeProcess Start(string folderBase,string configText,Type typeHandler,TimeSpan timeSpanTimeout) {
			string pathExe=CopyHost(folderBase,configText);
			string pathConfig=pathExe+".config";
			ProcessStartInfo processStartInfo=new ProcessStartInfo(pathExe,Quote(typeHandler.Assembly.Location)+" "+Quote(typeHandler.FullName));
			processStartInfo.UseShellExecute=false;//redirected standard streams
			processStartInfo.CreateNoWindow=true;
			processStartInfo.RedirectStandardInput=true;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.StandardOutputEncoding=Encoding.ASCII;//ProbeText lines are ASCII
			processStartInfo.WorkingDirectory=folderBase;
			Process process;
			try {
				process=Process.Start(processStartInfo);
			}
			catch(Exception) {
				DeleteQuietly(pathExe);
				DeleteQuietly(pathConfig);
				throw;
			}
			ProbeProcess probeProcess=new ProbeProcess(process,pathExe,pathConfig,timeSpanTimeout);
			process.ErrorDataReceived+=(sender,e) => {
				if(e.Data!=null) {
					lock(probeProcess._lockError) {
						probeProcess._stringBuilderError.AppendLine(e.Data);
					}
				}
			};
			process.BeginErrorReadLine();
			try {
				string line=probeProcess.ReadLine();
				if(line!=ProbeText.READY) {
					throw new InvalidOperationException("The child process did not start its handler: "+probeProcess.DescribeReply(line));
				}
			}
			catch(Exception) {
				probeProcess.Dispose();
				throw;
			}
			return probeProcess;
		}

		///<summary>Sends the command and its arguments, and returns the handler's result. Throws when the handler threw (with its exception's text),
		///when the child ended, or when it did not answer in time.</summary>
		public string Call(string command,params string[] arrayArgs) {
			return CallWithTimeout(_timeSpanTimeout,command,arrayArgs);
		}

		///<summary>Call, with its own wait for the reply in place of the one Start was given (a call that runs longer than the others).</summary>
		public string CallWithTimeout(TimeSpan timeSpanTimeout,string command,params string[] arrayArgs) {
			if(_isDisposed) {
				throw new ObjectDisposedException(nameof(ProbeProcess));
			}
			_process.StandardInput.WriteLine(ProbeText.Join(new[] { command }.Concat(arrayArgs)));
			_process.StandardInput.Flush();
			string line=ReadLine(timeSpanTimeout);
			if(line!=null && line.StartsWith(ProbeText.REPLY_OK+" ")) {
				return ProbeText.Split(line.Substring(ProbeText.REPLY_OK.Length+1))[0];
			}
			throw new InvalidOperationException(command+" failed in the child process: "+DescribeReply(line));
		}

		///<summary>The next line of the child's standard output, or null when it ended. Ends the child and throws when it does not answer in time.
		///</summary>
		private string ReadLine() {
			return ReadLine(_timeSpanTimeout);
		}

		private string ReadLine(TimeSpan timeSpanTimeout) {
			Task<string> taskLine=_process.StandardOutput.ReadLineAsync();
			if(!taskLine.Wait(timeSpanTimeout)) {
				Kill();
				throw new TimeoutException("The child process did not answer in "+timeSpanTimeout+". Its error output:\r\n"+ErrorOutput);
			}
			return taskLine.Result;
		}

		private string DescribeReply(string line) {
			if(line==null) {
				_process.WaitForExit(5000);
				return "it ended"+(_process.HasExited ? " with exit code "+_process.ExitCode : "")+". Its error output:\r\n"+ErrorOutput;
			}
			if(line.StartsWith(ProbeText.REPLY_ERROR+" ")) {
				return ProbeText.Split(line.Substring(ProbeText.REPLY_ERROR.Length+1))[0];
			}
			return "an unexpected line: "+line;
		}

		///<summary>Closes the child's standard input, which ends it, waits for it (ends it after 10 seconds), and deletes its copy of the host and
		///its config.</summary>
		public void Dispose() {
			if(_isDisposed) {
				return;
			}
			_isDisposed=true;
			try {
				_process.StandardInput.Close();
			}
			catch(Exception) {
				//It ended already.
			}
			if(!_process.WaitForExit(10000)) {
				Kill();
			}
			if(_process.HasExited) {
				_process.WaitForExit();//its standard error is read to its end
			}
			_process.Dispose();
			DeleteQuietly(_pathExe);
			DeleteQuietly(_pathConfig);
		}

		private void Kill() {
			try {
				if(!_process.HasExited) {
					_process.Kill();//the child this test started, and nothing else
					_process.WaitForExit(10000);
				}
			}
			catch(Exception) {
				//It ended meanwhile.
			}
		}

		///<summary>Runs typeEntry's public static int Run(string[] args) as the Main of a new process (TestProbeHost with RUN_ONCE) whose application
		///base is folderBase and whose config is configText (null for none), and waits for the process to end, as a console program compiled for
		///the test would run. The process ends once Run has returned and only background threads are left, or is ended after timeSpanTimeout.
		///The copy of the host and its config are deleted afterwards.</summary>
		public static ProbeRunResult RunOnce(string folderBase,string configText,Type typeEntry,string[] arrayArgs,TimeSpan timeSpanTimeout) {
			string pathExe=CopyHost(folderBase,configText);
			string pathConfig=pathExe+".config";
			try {
				string arguments=string.Join(" ",new[] { Program.RUN_ONCE,typeEntry.Assembly.Location,typeEntry.FullName }.Concat(arrayArgs).Select(x => Quote(x)));
				ProcessStartInfo processStartInfo=new ProcessStartInfo(pathExe,arguments);
				processStartInfo.UseShellExecute=false;//redirected standard streams
				processStartInfo.CreateNoWindow=true;
				processStartInfo.RedirectStandardOutput=true;
				processStartInfo.RedirectStandardError=true;
				processStartInfo.WorkingDirectory=folderBase;
				ProbeRunResult probeRunResult=new ProbeRunResult();
				Stopwatch stopwatch=Stopwatch.StartNew();
				using(Process process=Process.Start(processStartInfo)) {
					StringBuilder stringBuilder=new StringBuilder();
					object lockOutput=new object();
					process.OutputDataReceived+=(sender,e) => {
						if(e.Data!=null) {
							lock(lockOutput) {
								stringBuilder.AppendLine(e.Data);
							}
						}
					};
					process.ErrorDataReceived+=(sender,e) => {
						if(e.Data!=null) {
							lock(lockOutput) {
								stringBuilder.AppendLine("stderr: "+e.Data);
							}
						}
					};
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();
					if(process.WaitForExit((int)Math.Min(int.MaxValue,timeSpanTimeout.TotalMilliseconds))) {
						process.WaitForExit();//the output is read to its end
						probeRunResult.ExitCode=process.ExitCode;
					}
					else {
						probeRunResult.IsTimedOut=true;
						probeRunResult.ExitCode=-1;
						try {
							process.Kill();//the process this test started, and nothing else
						}
						catch(Exception) {
							//It ended meanwhile.
						}
						process.WaitForExit(10000);
					}
					probeRunResult.Elapsed=stopwatch.Elapsed;
					lock(lockOutput) {
						probeRunResult.Output=stringBuilder.ToString();
					}
				}
				return probeRunResult;
			}
			finally {
				DeleteQuietly(pathExe);
				DeleteQuietly(pathConfig);
			}
		}

		///<summary>A copy of TestProbeHost.exe in folderBase under a name of its own, with configText as its config (none for null), so the folder is
		///the child's application base and the config its alone. The copies a test process left behind when it died before Dispose are deleted
		///first (DeleteStaleCopies).</summary>
		private static string CopyHost(string folderBase,string configText) {
			DeleteStaleCopies(folderBase);
			string pathHost=typeof(ProbeProcess).Assembly.Location;//TestProbeHost.exe, next to the test assembly that references it
			string pathExe=Path.Combine(folderBase,COPY_PREFIX+Guid.NewGuid().ToString("N").Substring(0,COPY_HEX_LENGTH)+".exe");
			File.Copy(pathHost,pathExe);
			//File.Copy keeps the source's last write time; the creation time says when this copy was made, which DeleteStaleCopies reads.
			File.SetCreationTimeUtc(pathExe,DateTime.UtcNow);
			if(configText!=null) {
				File.WriteAllText(pathExe+".config",configText,new UTF8Encoding(false));
			}
			return pathExe;
		}

		private const string COPY_PREFIX="TestProbeHost-";
		private const int COPY_HEX_LENGTH=12;

		///<summary>True for the name of a copy CopyHost makes (TestProbeHost-, 12 lower-case hex digits, .exe) or of its config (the same, .exe.config).
		///</summary>
		public static bool IsCopyName(string fileName) {
			return Regex.IsMatch(fileName,"^"+Regex.Escape(COPY_PREFIX)+"[0-9a-f]{"+COPY_HEX_LENGTH+@"}\.exe(\.config)?$");
		}

		///<summary>Deletes the copies of the host, and their configs, that a test process left in the folder when it died before Dispose (killed by
		///the test runner's time limit, for one): only files named as CopyHost names them, and only those made before this process started, so a
		///copy that a child of this process runs from is never touched. A copy whose child still runs (a process started earlier that has not ended)
		///cannot be deleted and is left, with its config. Returns the files deleted.</summary>
		public static List<string> DeleteStaleCopies(string folder) {
			List<string> listDeleted=new List<string>();
			if(!Directory.Exists(folder)) {
				return listDeleted;
			}
			DateTime dateStartUtc;
			using(Process processThis=Process.GetCurrentProcess()) {
				dateStartUtc=processThis.StartTime.ToUniversalTime();
			}
			foreach(string pathExe in Directory.GetFiles(folder,COPY_PREFIX+"*.exe").Where(x => IsCopyName(Path.GetFileName(x)))) {
				if(File.GetCreationTimeUtc(pathExe)>=dateStartUtc) {
					continue;
				}
				try {
					File.Delete(pathExe);
					listDeleted.Add(pathExe);
				}
				catch(IOException) {
					continue;//its child still runs; its config stays with it
				}
				catch(UnauthorizedAccessException) {
					continue;
				}
			}
			//Configs without their copy: deleted above, or a copy already gone.
			foreach(string pathConfig in Directory.GetFiles(folder,COPY_PREFIX+"*.exe.config").Where(x => IsCopyName(Path.GetFileName(x)))) {
				string pathExe=pathConfig.Substring(0,pathConfig.Length-".config".Length);
				if(File.Exists(pathExe) || File.GetCreationTimeUtc(pathConfig)>=dateStartUtc) {
					continue;
				}
				try {
					File.Delete(pathConfig);
					listDeleted.Add(pathConfig);
				}
				catch(IOException) {
				}
				catch(UnauthorizedAccessException) {
				}
			}
			return listDeleted;
		}

		private static string Quote(string arg) {
			return "\""+arg+"\"";
		}

		private static void DeleteQuietly(string path) {
			for(int i=0;i<50;i++) {
				try {
					if(File.Exists(path)) {
						File.Delete(path);
					}
					return;
				}
				catch(IOException) {
					Thread.Sleep(100);//the ended process may hold its file for a moment
				}
				catch(UnauthorizedAccessException) {
					Thread.Sleep(100);
				}
			}
		}
	}
}
