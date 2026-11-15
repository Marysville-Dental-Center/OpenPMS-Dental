// The test's side of a child process (S3a-8 of the .NET 10 move, docs/port-notes/S3a-8.md): starts TestProbeHost from a folder and with a config
// of the test's choosing, sends it requests and reads its replies, and ends it and deletes its files afterwards. In place of a new AppDomain,
// which .NET cannot create: a fresh process starts with empty static state, its own config (binding redirects, trace sources) and its own
// application base, as the AppDomains of these tests did.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestProbeHost {
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
		///so its config is its alone) and configText as its config (null for none). It loads the assembly of typeHandler and calls typeHandler's
		///public static string Handle(string command,string[] args) for each Call. Each wait for the child, its start included, ends after
		///timeSpanTimeout, and the child is then ended.</summary>
		public static ProbeProcess Start(string folderBase,string configText,Type typeHandler,TimeSpan timeSpanTimeout) {
			string pathHost=typeof(ProbeProcess).Assembly.Location;//TestProbeHost.exe, next to the test assembly that references it
			string pathExe=Path.Combine(folderBase,"TestProbeHost-"+Guid.NewGuid().ToString("N").Substring(0,12)+".exe");
			string pathConfig=pathExe+".config";
			File.Copy(pathHost,pathExe);
			if(configText!=null) {
				File.WriteAllText(pathConfig,configText,new UTF8Encoding(false));
			}
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
			if(_isDisposed) {
				throw new ObjectDisposedException(nameof(ProbeProcess));
			}
			_process.StandardInput.WriteLine(ProbeText.Join(new[] { command }.Concat(arrayArgs)));
			_process.StandardInput.Flush();
			string line=ReadLine();
			if(line!=null && line.StartsWith(ProbeText.REPLY_OK+" ")) {
				return ProbeText.Split(line.Substring(ProbeText.REPLY_OK.Length+1))[0];
			}
			throw new InvalidOperationException(command+" failed in the child process: "+DescribeReply(line));
		}

		///<summary>The next line of the child's standard output, or null when it ended. Ends the child and throws when it does not answer in time.
		///</summary>
		private string ReadLine() {
			Task<string> taskLine=_process.StandardOutput.ReadLineAsync();
			if(!taskLine.Wait(_timeSpanTimeout)) {
				Kill();
				throw new TimeoutException("The child process did not answer in "+_timeSpanTimeout+". Its error output:\r\n"+ErrorOutput);
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
