// A .NET startup hook (https://learn.microsoft.com/en-us/dotnet/core/runtime-config/debugging-profiling#startup-hooks) that serves a test's
// handler inside a real program's own process (WINCHK-5): ProbeProcess.StartInProgram starts the program's apphost from its output folder with
// DOTNET_STARTUP_HOOKS naming this assembly and VARIABLE_HANDLER naming the handler. The runtime calls Initialize before the program's Main, in
// the program's process as the program starts it: its folder, its self-contained runtime, its .runtimeconfig.json and .deps.json, its
// .dll.config, and its assembly as the entry assembly. TestProbeHost cannot run from a self-contained program's folder (its apphost takes the
// folder's hostfxr.dll, which looks for the shared frameworks in that folder and finds none: "You must install or update .NET"), and the
// program cannot start from a test's output folder (S3b-0 item 7), so this is how a test gets a process of the program's own. Initialize serves (ProbeServer.Serve) until the test closes standard input and
// then ends the process: the program's Main never runs. Without VARIABLE_HANDLER it returns at once and the program starts as usual. Both
// variables are removed first, so no process the program starts inherits the hook. Only the tests use it; no shipped file references it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Runtime.Loader;
using System.Threading;
using TestProbeHost;

///<summary>The startup hook: the runtime looks for a type of this name in no namespace, with a static Initialize method. Internal, so the test
///assemblies that reference the host see no global type of this name.</summary>
internal class StartupHook {
	public static void Initialize() {
		string handler=Environment.GetEnvironmentVariable(ProbeProcess.VARIABLE_HANDLER);
		Environment.SetEnvironmentVariable(ProbeProcess.VARIABLE_STARTUP_HOOKS,null);
		Environment.SetEnvironmentVariable(ProbeProcess.VARIABLE_HANDLER,null);
		if(string.IsNullOrEmpty(handler)) {
			return;
		}
		//"<assembly file>|<type name>"; a file name holds no '|'.
		int index=handler.IndexOf('|');
		string pathAssembly=handler.Substring(0,index);
		string typeName=handler.Substring(index+1);
		//The program's own files come first: the default context binds every file of the program's .deps.json from the program's folder, and this
		//handler is asked only for a name it does not know (the test's own assemblies and their test-only references, in the test's folder).
		string folderHandler=Path.GetDirectoryName(Path.GetFullPath(pathAssembly));
		AssemblyLoadContext.Default.Resolving+=(assemblyLoadContext,assemblyName) => {
			string path=Path.Combine(folderHandler,assemblyName.Name+".dll");
			return File.Exists(path) ? assemblyLoadContext.LoadFromAssemblyPath(path) : null;
		};
		int exitCode=0;
		//The calls run on an MTA thread, as in TestProbeHost's own Main; the program's main thread only waits.
		Thread thread=new Thread(() => exitCode=ProbeServer.Serve(pathAssembly,typeName)) { Name="TestProbeHost startup hook" };
		thread.SetApartmentState(ApartmentState.MTA);
		thread.Start();
		thread.Join();
		Environment.Exit(exitCode);
	}
}
