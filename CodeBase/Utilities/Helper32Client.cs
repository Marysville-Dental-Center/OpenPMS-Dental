// The program's side of the 32-bit helper program (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, sections 3 and 4): starts
// OpenPmsDentalHelper32.exe from the program's folder on first use, one helper per program process, and calls its modules over a named pipe
// that only the current user may open. The protocol is OpenPmsDentalHelper32\Helper32Protocol.cs, linked into this assembly.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using OpenPmsDentalHelper32;

namespace CodeBase {
	///<summary>Why a call to the 32-bit helper failed.</summary>
	public enum Helper32Failure {
		///<summary>OpenPmsDentalHelper32.exe is not in the program's folder.</summary>
		Missing,
		///<summary>The helper did not start, did not connect or did not say hello in time.</summary>
		StartFailed,
		///<summary>The helper did not answer the call in time; it was ended, and the next call starts a new one.</summary>
		TimedOut,
		///<summary>The helper ended during the call; the next call starts a new one.</summary>
		Crashed,
		///<summary>The module threw: the message is the module's exception's message, RemoteTypeName its type.</summary>
		Remote,
		///<summary>The helper's messages did not follow the protocol (another version, another process at the pipe's end).</summary>
		Protocol,
	}

	///<summary>A failed call to the 32-bit helper. For Remote, Message is the message of the exception the vendor code threw in the helper, as the
	///program showed it when the vendor code ran in its own process.</summary>
	public class Helper32Exception:Exception {
		public Helper32Failure Failure { get; }
		///<summary>Remote only: the full name of the exception's type in the helper (System.Runtime.InteropServices.COMException, ...).</summary>
		public string RemoteTypeName { get; }

		public Helper32Exception(Helper32Failure failure,string message,string remoteTypeName=null,Exception innerException=null)
			: base(message,innerException)
		{
			Failure=failure;
			RemoteTypeName=remoteTypeName;
		}
	}

	///<summary>What a module returned.</summary>
	public sealed class Helper32Result {
		public string[] Results;
		public byte[] Data;
	}

	///<summary>One helper process and its pipe. Default is the program's; tests make their own from a copy of the helper. Calls are made one at a
	///time (a second caller waits for the first). Every call has a timeout and then fails with Helper32Failure.TimedOut; a helper that ended
	///during a call fails that call once (Crashed), and the next call starts a new helper; a missing helper gives Missing. The helper ends when
	///this client is disposed, when the program's process ends (Default is disposed at its exit, and the helper is in a job object that ends
	///it when the program's process is gone, however it ended) and when it loses its pipe.</summary>
	public sealed class Helper32Client:IDisposable {
		public const string HELPER_FILE_NAME="OpenPmsDentalHelper32.exe";
		///<summary>How long a start may take: the process, its connection and its hello.</summary>
		public static readonly TimeSpan TimeSpanStart=TimeSpan.FromSeconds(30);
		private static readonly object _lockDefault=new object();
		private static Helper32Client _clientDefault;
		private readonly object _lock=new object();
		private readonly string _pathHelper;
		private readonly string _argumentsExtra;
		private Process _process;
		private NamedPipeServerStream _pipe;
		private SafeJobHandle _job;
		private volatile int _processId;
		private volatile string _pipeName;
		private bool _isDisposed;

		///<summary>The helper of the program's process, from the program's folder (OpenPmsDentalHelper32.exe next to the program; S3b-4 moves it
		///to the sub folder Helper32\ with its own runtime). Started on its first call.</summary>
		public static Helper32Client Default {
			get {
				lock(_lockDefault) {
					if(_clientDefault==null) {
						_clientDefault=new Helper32Client(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,HELPER_FILE_NAME));
						AppDomain.CurrentDomain.ProcessExit+=(sender,e) => _clientDefault.Dispose();
					}
					return _clientDefault;
				}
			}
		}

		///<summary>A client of the helper at pathHelper (a copy of it, for tests).</summary>
		public Helper32Client(string pathHelper) : this(pathHelper,null) {
		}

		///<summary>Tests: argumentsExtra are added to the helper's command line (--test-module, Helper32Protocol.ARG_TEST_MODULE).</summary>
		public Helper32Client(string pathHelper,string argumentsExtra) {
			_pathHelper=Path.GetFullPath(pathHelper);
			_argumentsExtra=argumentsExtra;
		}

		public string PathHelper {
			get {
				return _pathHelper;
			}
		}

		///<summary>The process id of the running helper; 0 before the first call and after it ended.</summary>
		public int ProcessId {
			get {
				return _processId;
			}
		}

		///<summary>The name of the running helper's pipe (Helper32Protocol.PIPE_NAME_PREFIX, the program's process id, 32 random hex digits).
		///</summary>
		public string PipeName {
			get {
				return _pipeName;
			}
		}

		///<summary>Calls command of module in the helper with args and data, and returns its results. Starts the helper first when none runs.
		///Throws Helper32Exception: Missing, StartFailed, TimedOut (after timeSpanTimeout), Crashed, Remote (the module threw) or Protocol.</summary>
		public Helper32Result Call(string module,string command,string[] args,byte[] data,TimeSpan timeSpanTimeout) {
			lock(_lock) {
				if(_isDisposed) {
					throw new ObjectDisposedException(nameof(Helper32Client));
				}
				EnsureStarted();
				Helper32Request request=new Helper32Request();
				request.Module=module;
				request.Command=command;
				request.Args=args??new string[0];
				request.Data=data;
				NamedPipeServerStream pipe=_pipe;
				try {
					Helper32Protocol.WriteRequest(pipe,request);
				}
				catch(IOException exception) {
					throw Ended(module,command,exception);
				}
				catch(InvalidDataException exception) {
					throw new Helper32Exception(Helper32Failure.Protocol,exception.Message,null,exception);//too long: nothing was sent
				}
				Task<Helper32Reply> taskRead=Task.Run(() => Helper32Protocol.ReadReply(pipe));
				bool isDone;
				try {
					isDone=taskRead.Wait(timeSpanTimeout);
				}
				catch(AggregateException) {
					isDone=true;
				}
				if(!isDone) {
					taskRead.ContinueWith(x => x.Exception,TaskContinuationOptions.OnlyOnFaulted);//observed: the pipe is closed below
					Stop(isKill:true);
					throw new Helper32Exception(Helper32Failure.TimedOut,"The 32-bit helper program ("+HELPER_FILE_NAME+") did not answer "+module+" "
						+command+" within "+timeSpanTimeout.TotalSeconds.ToString("0",CultureInfo.InvariantCulture)+" seconds. It was ended and is "
						+"started again on the next call.");
				}
				if(taskRead.IsFaulted) {
					Exception exception=taskRead.Exception.GetBaseException();
					if(exception is IOException) {//EndOfStreamException included: the helper ended inside its reply
						throw Ended(module,command,exception);
					}
					Stop(isKill:true);
					throw new Helper32Exception(Helper32Failure.Protocol,"The 32-bit helper program's reply to "+module+" "+command+" was not "
						+"readable: "+exception.Message,null,exception);
				}
				Helper32Reply reply=taskRead.Result;
				if(reply==null) {
					throw Ended(module,command,null);
				}
				if(!reply.IsOk) {
					throw new Helper32Exception(Helper32Failure.Remote,reply.ErrorMessage,reply.ErrorType);
				}
				Helper32Result result=new Helper32Result();
				result.Results=reply.Results??new string[0];
				result.Data=reply.Data;
				return result;
			}
		}

		///<summary>Call with string arguments and results only.</summary>
		public string[] Call(string module,string command,TimeSpan timeSpanTimeout,params string[] args) {
			return Call(module,command,args,null,timeSpanTimeout).Results;
		}

		///<summary>Ends the helper: closes its pipe, which ends it, and ends it after 2 seconds if it has not. A later Call throws.</summary>
		public void Dispose() {
			lock(_lock) {
				if(_isDisposed) {
					return;
				}
				_isDisposed=true;
				Stop(isKill:false);
			}
		}

		///<summary>The helper ended during a call: reported once, by this exception, and the next call starts a new one.</summary>
		private Helper32Exception Ended(string module,string command,Exception exception) {
			string exitCode="unknown";
			try {
				if(_process.WaitForExit(5000)) {
					exitCode=_process.ExitCode.ToString(CultureInfo.InvariantCulture);
				}
			}
			catch(Exception) {
				//no exit code
			}
			Stop(isKill:true);
			return new Helper32Exception(Helper32Failure.Crashed,"The 32-bit helper program ("+HELPER_FILE_NAME+") ended during "+module+" "+command
				+" (exit code "+exitCode+"). It is started again on the next call.",null,exception);
		}

		///<summary>Starts the helper unless one runs and is connected. A helper that ended between two calls is replaced without a message.</summary>
		private void EnsureStarted() {
			if(_process!=null) {
				bool isRunning=false;
				try {
					isRunning=!_process.HasExited && _pipe.IsConnected;
				}
				catch(Exception) {
					//treated as ended
				}
				if(isRunning) {
					return;
				}
				Stop(isKill:true);
			}
			if(!File.Exists(_pathHelper)) {
				throw new Helper32Exception(Helper32Failure.Missing,"The 32-bit helper program "+HELPER_FILE_NAME+" was not found at "+_pathHelper
					+". Reinstall Open PMS - Dental or update it again to restore it.");
			}
			string pipeName=Helper32Protocol.NewPipeName(Process.GetCurrentProcess().Id);
			NamedPipeServerStream pipe=Helper32Pipe.Create(pipeName);
			Process process=null;
			SafeJobHandle job=null;
			try {
				IAsyncResult asyncResultConnect=pipe.BeginWaitForConnection(null,null);
				string arguments=Helper32Protocol.ARG_PIPE+" "+pipeName+" "+Helper32Protocol.ARG_PARENT+" "
					+Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
				if(!string.IsNullOrEmpty(_argumentsExtra)) {
					arguments+=" "+_argumentsExtra;
				}
				ProcessStartInfo processStartInfo=new ProcessStartInfo(_pathHelper,arguments);
				processStartInfo.UseShellExecute=false;//the helper itself, never a file association
				processStartInfo.CreateNoWindow=true;
				processStartInfo.ErrorDialog=false;
				processStartInfo.WorkingDirectory=Path.GetDirectoryName(_pathHelper);
				try {
					process=Process.Start(processStartInfo);
				}
				catch(Exception exception) {
					throw new Helper32Exception(Helper32Failure.StartFailed,"The 32-bit helper program "+_pathHelper+" could not be started: "
						+exception.Message,null,exception);
				}
				job=SafeJobHandle.CreateFor(process);
				DateTime dateTimeDeadline=DateTime.UtcNow+TimeSpanStart;
				while(!asyncResultConnect.AsyncWaitHandle.WaitOne(100)) {
					if(process.HasExited) {
						throw new Helper32Exception(Helper32Failure.StartFailed,"The 32-bit helper program "+HELPER_FILE_NAME+" ended before it "
							+"connected (exit code "+process.ExitCode.ToString(CultureInfo.InvariantCulture)+").");
					}
					if(DateTime.UtcNow>dateTimeDeadline) {
						throw new Helper32Exception(Helper32Failure.StartFailed,"The 32-bit helper program "+HELPER_FILE_NAME+" did not connect "
							+"within "+TimeSpanStart.TotalSeconds.ToString("0",CultureInfo.InvariantCulture)+" seconds.");
					}
				}
				pipe.EndWaitForConnection(asyncResultConnect);
				//Only the helper this client started may be at the pipe's other end.
				if(!NativeMethods.GetNamedPipeClientProcessId(pipe.SafePipeHandle,out uint processIdClient) || processIdClient!=(uint)process.Id) {
					throw new Helper32Exception(Helper32Failure.Protocol,"Another process than the 32-bit helper program connected to its pipe.");
				}
				Task<Helper32Hello> taskHello=Task.Run(() => Helper32Protocol.ReadHello(pipe));
				bool isDone;
				try {
					isDone=taskHello.Wait(TimeSpanStart);
				}
				catch(AggregateException) {
					isDone=true;
				}
				if(!isDone) {
					taskHello.ContinueWith(x => x.Exception,TaskContinuationOptions.OnlyOnFaulted);
					throw new Helper32Exception(Helper32Failure.StartFailed,"The 32-bit helper program "+HELPER_FILE_NAME+" did not say hello.");
				}
				if(taskHello.IsFaulted || taskHello.Result==null) {
					throw new Helper32Exception(Helper32Failure.StartFailed,"The 32-bit helper program "+HELPER_FILE_NAME+" ended before its hello"
						+(process.WaitForExit(5000) ? " (exit code "+process.ExitCode.ToString(CultureInfo.InvariantCulture)+")." : "."),null,
						taskHello.Exception?.GetBaseException());
				}
				Helper32Hello hello=taskHello.Result;
				if(hello.Version!=Helper32Protocol.VERSION || hello.ProcessId!=process.Id) {
					throw new Helper32Exception(Helper32Failure.Protocol,"The 32-bit helper program "+_pathHelper+" speaks protocol version "
						+hello.Version.ToString(CultureInfo.InvariantCulture)+", not "+Helper32Protocol.VERSION.ToString(CultureInfo.InvariantCulture)
						+", or is not the process that was started. Reinstall Open PMS - Dental.");
				}
			}
			catch(Exception exception) {
				KillQuietly(process);
				pipe.Dispose();
				job?.Dispose();
				process?.Dispose();
				if(exception is Helper32Exception) {
					throw;
				}
				throw new Helper32Exception(Helper32Failure.StartFailed,"The 32-bit helper program "+_pathHelper+" could not be started: "
					+exception.Message,null,exception);
			}
			_process=process;
			_pipe=pipe;
			_job=job;
			_processId=process.Id;
			_pipeName=pipeName;
		}

		///<summary>Closes the pipe (the helper then ends by itself), ends the helper after 2 seconds (the time a process exit gives its handlers)
		///or at once (isKill), and forgets it.</summary>
		private void Stop(bool isKill) {
			Process process=_process;
			_process=null;
			_processId=0;
			_pipeName=null;
			try {
				_pipe?.Dispose();
			}
			catch(Exception) {
				//already broken
			}
			_pipe=null;
			if(process!=null) {
				try {
					if(isKill || !process.WaitForExit(2000)) {
						KillQuietly(process);
					}
				}
				catch(Exception) {
					//ended meanwhile
				}
				process.Dispose();
			}
			_job?.Dispose();//ends the helper if it still runs
			_job=null;
		}

		private static void KillQuietly(Process process) {
			if(process==null) {
				return;
			}
			try {
				if(!process.HasExited) {
					process.Kill();//the helper this client started, and nothing else
					process.WaitForExit(10000);
				}
			}
			catch(Exception) {
				//ended meanwhile
			}
		}

		///<summary>The helper's pipe: created by the program before the helper starts, in the local machine's namespace (\\.\pipe\), with a random
		///name; one instance only (FILE_FLAG_FIRST_PIPE_INSTANCE: the create fails if any process made a pipe of that name first); remote
		///clients rejected (PIPE_REJECT_REMOTE_CLIENTS: no connection over the network, whoever asks); a protected DACL that allows read and
		///write to the current user's SID and to no one else (no Everyone, no other user, no inherited entries), with the current user as owner;
		///not inherited by any process the program starts.</summary>
		internal static class Helper32Pipe {
			public static NamedPipeServerStream Create(string pipeName) {
				string sid=WindowsIdentity.GetCurrent().User.Value;
				string sddl=GetSddl(sid);
				if(!NativeMethods.ConvertStringSecurityDescriptorToSecurityDescriptor(sddl,1,out IntPtr securityDescriptor,out uint _)) {
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
				try {
					NativeMethods.SECURITY_ATTRIBUTES securityAttributes=new NativeMethods.SECURITY_ATTRIBUTES();
					securityAttributes.nLength=Marshal.SizeOf(typeof(NativeMethods.SECURITY_ATTRIBUTES));
					securityAttributes.lpSecurityDescriptor=securityDescriptor;
					securityAttributes.bInheritHandle=0;
					SafePipeHandle safePipeHandle=NativeMethods.CreateNamedPipe(@"\\.\pipe\"+pipeName,
						NativeMethods.PIPE_ACCESS_DUPLEX|NativeMethods.FILE_FLAG_FIRST_PIPE_INSTANCE|NativeMethods.FILE_FLAG_OVERLAPPED,
						NativeMethods.PIPE_TYPE_BYTE|NativeMethods.PIPE_READMODE_BYTE|NativeMethods.PIPE_WAIT|NativeMethods.PIPE_REJECT_REMOTE_CLIENTS,
						1,65536,65536,0,ref securityAttributes);
					if(safePipeHandle.IsInvalid) {
						int error=Marshal.GetLastWin32Error();
						safePipeHandle.Dispose();
						throw new Helper32Exception(Helper32Failure.StartFailed,"The 32-bit helper program's pipe could not be created: "
							+new Win32Exception(error).Message);
					}
					return new NamedPipeServerStream(PipeDirection.InOut,true,false,safePipeHandle);
				}
				finally {
					NativeMethods.LocalFree(securityDescriptor);
				}
			}

			///<summary>The pipe's security descriptor: owner the user, a protected DACL with one entry, read and write for the user.</summary>
			public static string GetSddl(string sid) {
				return "O:"+sid+"D:P(A;;GRGW;;;"+sid+")";
			}
		}

		///<summary>A job object that ends the helper when the program's process is gone, however it ended (KILL_ON_JOB_CLOSE: the program's
		///handle is the only one, and Windows closes it with the process), and keeps a crash of the helper quiet (DIE_ON_UNHANDLED_EXCEPTION: no
		///Windows error dialog). Processes the helper or its vendor code start are not in it (SILENT_BREAKAWAY_OK): QuickBooks or a vendor's
		///program that the vendor code starts is never ended with the program. Null when Windows refused (the helper then ends by its own watch
		///on the program's process).</summary>
		private sealed class SafeJobHandle:SafeHandleZeroOrMinusOneIsInvalid {
			private SafeJobHandle() : base(true) {
			}

			public static SafeJobHandle CreateFor(Process process) {
				SafeJobHandle job=NativeMethods.CreateJobObject(IntPtr.Zero,null);
				if(job.IsInvalid) {
					job.Dispose();
					return null;
				}
				NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION info=new NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
				info.BasicLimitInformation.LimitFlags=NativeMethods.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
					|NativeMethods.JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION|NativeMethods.JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK;
				int size=Marshal.SizeOf(typeof(NativeMethods.JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
				if(!NativeMethods.SetInformationJobObject(job,NativeMethods.JobObjectExtendedLimitInformation,ref info,(uint)size)
					|| !NativeMethods.AssignProcessToJobObject(job,process.Handle))
				{
					job.Dispose();
					return null;
				}
				return job;
			}

			protected override bool ReleaseHandle() {
				return NativeMethods.CloseHandle(handle);
			}

			private static class NativeMethods {
				public const int JobObjectExtendedLimitInformation=9;
				public const uint JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION=0x00000400;
				public const uint JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK=0x00001000;
				public const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE=0x00002000;

				[StructLayout(LayoutKind.Sequential)]
				public struct JOBOBJECT_BASIC_LIMIT_INFORMATION {
					public long PerProcessUserTimeLimit;
					public long PerJobUserTimeLimit;
					public uint LimitFlags;
					public UIntPtr MinimumWorkingSetSize;
					public UIntPtr MaximumWorkingSetSize;
					public uint ActiveProcessLimit;
					public UIntPtr Affinity;
					public uint PriorityClass;
					public uint SchedulingClass;
				}

				[StructLayout(LayoutKind.Sequential)]
				public struct IO_COUNTERS {
					public ulong ReadOperationCount;
					public ulong WriteOperationCount;
					public ulong OtherOperationCount;
					public ulong ReadTransferCount;
					public ulong WriteTransferCount;
					public ulong OtherTransferCount;
				}

				[StructLayout(LayoutKind.Sequential)]
				public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION {
					public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
					public IO_COUNTERS IoInfo;
					public UIntPtr ProcessMemoryLimit;
					public UIntPtr JobMemoryLimit;
					public UIntPtr PeakProcessMemoryUsed;
					public UIntPtr PeakJobMemoryUsed;
				}

				[DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
				public static extern SafeJobHandle CreateJobObject(IntPtr jobAttributes,string name);

				[DllImport("kernel32.dll",SetLastError=true)]
				[return: MarshalAs(UnmanagedType.Bool)]
				public static extern bool SetInformationJobObject(SafeJobHandle job,int infoClass,ref JOBOBJECT_EXTENDED_LIMIT_INFORMATION info,
					uint length);

				[DllImport("kernel32.dll",SetLastError=true)]
				[return: MarshalAs(UnmanagedType.Bool)]
				public static extern bool AssignProcessToJobObject(SafeJobHandle job,IntPtr process);

				[DllImport("kernel32.dll",SetLastError=true)]
				[return: MarshalAs(UnmanagedType.Bool)]
				public static extern bool CloseHandle(IntPtr handle);
			}
		}

		private static class NativeMethods {
			public const uint PIPE_ACCESS_DUPLEX=0x00000003;
			public const uint FILE_FLAG_FIRST_PIPE_INSTANCE=0x00080000;
			public const uint FILE_FLAG_OVERLAPPED=0x40000000;
			public const uint PIPE_TYPE_BYTE=0x00000000;
			public const uint PIPE_READMODE_BYTE=0x00000000;
			public const uint PIPE_WAIT=0x00000000;
			public const uint PIPE_REJECT_REMOTE_CLIENTS=0x00000008;

			[StructLayout(LayoutKind.Sequential)]
			public struct SECURITY_ATTRIBUTES {
				public int nLength;
				public IntPtr lpSecurityDescriptor;
				public int bInheritHandle;
			}

			///<summary>https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-createnamedpipew</summary>
			[DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
			public static extern SafePipeHandle CreateNamedPipe(string name,uint openMode,uint pipeMode,uint maxInstances,uint outBufferSize,
				uint inBufferSize,uint defaultTimeOut,ref SECURITY_ATTRIBUTES securityAttributes);

			///<summary>https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getnamedpipeclientprocessid</summary>
			[DllImport("kernel32.dll",SetLastError=true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool GetNamedPipeClientProcessId(SafePipeHandle pipe,out uint clientProcessId);

			[DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string stringSecurityDescriptor,uint revision,
				out IntPtr securityDescriptor,out uint size);

			[DllImport("kernel32.dll")]
			public static extern IntPtr LocalFree(IntPtr memory);
		}
	}
}
