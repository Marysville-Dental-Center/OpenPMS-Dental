// The 32-bit helper program's entry point (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 3).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;

namespace OpenPmsDentalHelper32 {
	internal static class Program {
		///<summary>How long the helper waits for the program's pipe after its start.</summary>
		private const int CONNECT_TIMEOUT_MS=30000;

		///<summary>Started by the program (CodeBase.Helper32Client) with --pipe &lt;name&gt; --parent &lt;the program's process id&gt;. Shows no window,
		///whatever happens: started any other way it ends with EXIT_USAGE at once. Connects to the program's pipe, checks that the program is at
		///its other end, sends its hello and answers the program's requests on this thread, a single-threaded apartment, until the program closes
		///the pipe or ends. Between two requests the thread dispatches its window messages (MessagePumpingStream).</summary>
		[STAThread]
		private static int Main(string[] args) {
			//Before anything creates a window (S3b-4 of docs/specs/net10-plan.md): the .NET Framework's default font, which the forms were laid out
			//with (.NET's is Segoe UI 9; SetDefaultFont throws once a window exists), then per-monitor DPI awareness, the program's mode (the
			//.NET Framework build of the program set it in its app.config, which .NET does not read).
			System.Windows.Forms.Application.SetDefaultFont(new System.Drawing.Font("Microsoft Sans Serif",8.25f));
			System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.PerMonitorV2);
			//No Windows error dialog for this process (a missing drive, a crash): the program reports a helper that failed. This sets the mode of
			//this process only (and of the processes it starts), never a system setting.
			NativeMethods.SetErrorMode(NativeMethods.SEM_FAILCRITICALERRORS|NativeMethods.SEM_NOGPFAULTERRORBOX|NativeMethods.SEM_NOOPENFILEERRORBOX);
			HelperArguments helperArguments=HelperArguments.Parse(args);
			if(helperArguments==null) {
				return Helper32Protocol.EXIT_USAGE;
			}
			Process processParent;
			try {
				processParent=Process.GetProcessById(helperArguments.ParentProcessId);
				//A process id can be reused: the program started before its helper.
				if(processParent.StartTime>Process.GetCurrentProcess().StartTime) {
					return Helper32Protocol.EXIT_PARENT;
				}
			}
			catch(Exception) {
				return Helper32Protocol.EXIT_PARENT;
			}
			StartParentWatch(processParent);
			using(NamedPipeClientStream pipe=new NamedPipeClientStream(".",helperArguments.PipeName,PipeDirection.InOut,PipeOptions.Asynchronous,
				TokenImpersonationLevel.Identification))
			{
				try {
					pipe.Connect(CONNECT_TIMEOUT_MS);
				}
				catch(Exception) {
					return Helper32Protocol.EXIT_CONNECT;
				}
				//The pipe's other end is the program that started this helper, and no other process.
				if(!NativeMethods.GetNamedPipeServerProcessId(pipe.SafePipeHandle,out uint processIdServer)
					|| processIdServer!=(uint)helperArguments.ParentProcessId)
				{
					return Helper32Protocol.EXIT_CONNECT;
				}
				List<IHelper32Module> listModules;
				try {
					listModules=Helper32Modules.CreateModules(pipe,helperArguments);
				}
				catch(Exception) {
					return Helper32Protocol.EXIT_USAGE;
				}
				Helper32Hello hello=new Helper32Hello();
				hello.Version=Helper32Protocol.VERSION;
				hello.ProcessId=Process.GetCurrentProcess().Id;
				try {
					Helper32Protocol.WriteHello(pipe,hello);
				}
				catch(Exception) {
					return Helper32Protocol.EXIT_CONNECT;
				}
				//Overlapped reads with the thread's messages pumped between two requests (S3a-14): the modules' windows and COM objects live on
				//this thread.
				return new Helper32Host(listModules).Run(new MessagePumpingStream(pipe));
			}
		}

		///<summary>Ends this process when the program's process ends, even while a module's call still runs (a vendor call that hangs): the
		///program's end also closes the pipe, which ends the request loop, but only between two calls. The program's job object ends the helper
		///too (Helper32Client); this covers a program that could not put it into one.</summary>
		private static void StartParentWatch(Process processParent) {
			Thread thread=new Thread(() => {
				try {
					processParent.WaitForExit();
				}
				catch(Exception) {
					//No handle on the parent: it ended already.
				}
				Thread threadKill=new Thread(() => {
					Thread.Sleep(10000);
					Process.GetCurrentProcess().Kill();//Environment.Exit did not finish
				});
				threadKill.IsBackground=true;
				threadKill.Start();
				Environment.Exit(Helper32Protocol.EXIT_OK);
			});
			thread.IsBackground=true;
			thread.Name="Helper32ParentWatch";
			thread.Start();
		}
	}

	///<summary>The helper's command line.</summary>
	internal sealed class HelperArguments {
		public string PipeName;
		public int ParentProcessId;
		///<summary>Tests only (Helper32Protocol.ARG_TEST_MODULE): the assembly name, the type and the module name of a test module; null without.
		///</summary>
		public string TestModuleAssembly;
		public string TestModuleType;
		public string TestModuleName;
		///<summary>Tests only (Helper32Protocol.ARG_TEST_TWAIN_DSM): the assembly name, the type and the scenario of a fake TWAIN DSM; null without.
		///</summary>
		public string TestTwainDsmAssembly;
		public string TestTwainDsmType;
		public string TestTwainDsmScenario;

		///<summary>The arguments, or null when they are not the helper's: --pipe and --parent once each, with a pipe name of Helper32Protocol's
		///form and a process id; optionally --test-module and --test-twain-dsm.</summary>
		public static HelperArguments Parse(string[] args) {
			HelperArguments helperArguments=new HelperArguments();
			for(int i=0;i<args.Length;i++) {
				if(args[i]==Helper32Protocol.ARG_PIPE && i+1<args.Length && helperArguments.PipeName==null) {
					helperArguments.PipeName=args[++i];
				}
				else if(args[i]==Helper32Protocol.ARG_PARENT && i+1<args.Length && helperArguments.ParentProcessId==0) {
					if(!int.TryParse(args[++i],NumberStyles.None,CultureInfo.InvariantCulture,out helperArguments.ParentProcessId)) {
						return null;
					}
				}
				else if(args[i]==Helper32Protocol.ARG_TEST_MODULE && i+2<args.Length && helperArguments.TestModuleAssembly==null) {
					helperArguments.TestModuleAssembly=args[++i];
					helperArguments.TestModuleType=args[++i];
					helperArguments.TestModuleName=TestModule.NAME_DEFAULT;
					if(i+1<args.Length && !args[i+1].StartsWith("--")) {
						helperArguments.TestModuleName=args[++i];
					}
				}
				else if(args[i]==Helper32Protocol.ARG_TEST_TWAIN_DSM && i+3<args.Length && helperArguments.TestTwainDsmAssembly==null) {
					helperArguments.TestTwainDsmAssembly=args[++i];
					helperArguments.TestTwainDsmType=args[++i];
					helperArguments.TestTwainDsmScenario=args[++i];
				}
				else {
					return null;
				}
			}
			if(!Helper32Protocol.IsPipeName(helperArguments.PipeName) || helperArguments.ParentProcessId<=0) {
				return null;
			}
			//The program names its pipes after its own process id.
			if(!helperArguments.PipeName.StartsWith(Helper32Protocol.PIPE_NAME_PREFIX
				+helperArguments.ParentProcessId.ToString(CultureInfo.InvariantCulture)+"-",StringComparison.Ordinal))
			{
				return null;
			}
			return helperArguments;
		}
	}
}
