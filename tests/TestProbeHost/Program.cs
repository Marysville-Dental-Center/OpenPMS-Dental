// The child process of the tests that need a fresh process (S3a-8 of the .NET 10 move): it loads the test's assembly, then answers one line per
// request with the result of the handler's Handle method, until its standard input ends. Or, started with RUN_ONCE first (S3a-8b), it runs
// one entry method of the test's assembly as its Main and ends with its exit code, in place of a console program compiled at run time.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TestProbeHost {
	///<summary>Internal, so the test assemblies that reference the host see no second type named Program.</summary>
	internal static class Program {
		///<summary>The first argument of a run-once start (ProbeProcess.RunOnce).</summary>
		public const string RUN_ONCE="--run-once";
		///<summary>The exit code when the host itself cannot start the work: wrong arguments.</summary>
		public const int EXIT_USAGE=90;
		///<summary>The exit code when the handler or entry method cannot be loaded.</summary>
		public const int EXIT_LOAD_FAILED=91;
		///<summary>The exit code when the entry method threw (its exception is on standard error).</summary>
		public const int EXIT_RUN_THREW=92;

		///<summary>args[0] is the assembly file of the handler, args[1] the handler's type, which declares public static string Handle(string
		///command,string[] args). The first line on standard output is READY, or an error reply when the handler cannot be loaded. Then each
		///line of standard input (ProbeText.Join: the command and its arguments) gets one reply line (ProbeText.FormatReply). The calls run on
		///this MTA thread, as a test method's code runs on the test runner's; work that needs STA makes its own thread.
		///With RUN_ONCE first: RUN_ONCE, the assembly file, the type, then the entry method's arguments (see RunOnce).</summary>
		[MTAThread]
		public static int Main(string[] args) {
			//The code pages, as the program registers them first (ODInitialize, S3b-2): .NET has only the Unicode encodings, ASCII and Latin-1
			//built in, and the probes run the libraries' code (S3b-5).
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			if(args.Length>0 && args[0]==RUN_ONCE) {
				return RunOnce(args.Skip(1).ToArray());
			}
			//Standard output carries the replies only. What the work writes to the console goes to standard error, which the test keeps for its
			//messages.
			StreamWriter streamWriterReplies=new StreamWriter(Console.OpenStandardOutput(),new UTF8Encoding(false));
			streamWriterReplies.AutoFlush=true;
			Console.SetOut(Console.Error);
			if(args.Length!=2) {
				streamWriterReplies.WriteLine(ProbeText.FormatReply(false,"Usage: TestProbeHost <assembly file> <type name>"));
				return 2;
			}
			MethodInfo methodInfoHandle;
			try {
				Type type=LoadAssembly(args[0]).GetType(args[1],true);
				methodInfoHandle=type.GetMethod("Handle",BindingFlags.Public|BindingFlags.Static,null,new[] { typeof(string),typeof(string[]) },null);
				if(methodInfoHandle==null || methodInfoHandle.ReturnType!=typeof(string)) {
					throw new MissingMethodException(type.FullName+" declares no public static string Handle(string,string[]).");
				}
			}
			catch(Exception ex) {
				streamWriterReplies.WriteLine(ProbeText.FormatReply(false,ex.ToString()));
				return 3;
			}
			streamWriterReplies.WriteLine(ProbeText.READY);
			StreamReader streamReaderRequests=new StreamReader(Console.OpenStandardInput(),new UTF8Encoding(false));
			while(true) {
				string line=streamReaderRequests.ReadLine();
				if(line==null) {
					return 0;//the test closed standard input: done
				}
				string reply;
				try {
					string[] arrayFields=ProbeText.Split(line);
					string result=(string)methodInfoHandle.Invoke(null,new object[] { arrayFields[0],arrayFields.Skip(1).ToArray() });
					reply=ProbeText.FormatReply(true,result);
				}
				catch(TargetInvocationException ex) {
					reply=ProbeText.FormatReply(false,(ex.InnerException??ex).ToString());
				}
				catch(Exception ex) {
					reply=ProbeText.FormatReply(false,ex.ToString());
				}
				streamWriterReplies.WriteLine(reply);
			}
		}

		///<summary>args[0] is the assembly file, args[1] the type, which declares public static int Run(string[] args); the rest are Run's arguments.
		///Run is this process's Main: it runs on this MTA thread, its console output is this process's, and its result is the exit code. The
		///process then ends as a console program's does: once Main has returned and only background threads are left. EXIT_USAGE or
		///EXIT_LOAD_FAILED (with the reason on standard error) when the work cannot start, EXIT_RUN_THREW when Run threw.</summary>
		private static int RunOnce(string[] args) {
			if(args.Length<2) {
				Console.Error.WriteLine("Usage: TestProbeHost "+RUN_ONCE+" <assembly file> <type name> [arguments...]");
				return EXIT_USAGE;
			}
			MethodInfo methodInfoRun;
			try {
				Type type=LoadAssembly(args[0]).GetType(args[1],true);
				methodInfoRun=type.GetMethod("Run",BindingFlags.Public|BindingFlags.Static,null,new[] { typeof(string[]) },null);
				if(methodInfoRun==null || methodInfoRun.ReturnType!=typeof(int)) {
					throw new MissingMethodException(type.FullName+" declares no public static int Run(string[]).");
				}
			}
			catch(Exception ex) {
				Console.Error.WriteLine("TestProbeHost could not load the entry method: "+ex);
				return EXIT_LOAD_FAILED;
			}
			try {
				return (int)methodInfoRun.Invoke(null,new object[] { args.Skip(2).ToArray() });
			}
			catch(TargetInvocationException ex) {
				//Written and ended with an exit code of its own, not left unhandled: an unhandled exception would also start Windows Error
				//Reporting for the process.
				Console.Error.WriteLine("Run threw: "+(ex.InnerException??ex));
				return EXIT_RUN_THREW;
			}
		}

		///<summary>An assembly in this process's own folder is loaded by its name, so the name a config or a type string gives it later finds the
		///same copy (the test's binding redirects apply to it and to everything it loads); one elsewhere is loaded from its file.</summary>
		private static Assembly LoadAssembly(string path) {
			string pathFull=Path.GetFullPath(path);
			string folderBase=Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\');
			if(string.Equals(Path.GetDirectoryName(pathFull).TrimEnd('\\'),folderBase,StringComparison.OrdinalIgnoreCase)) {
				try {
					return Assembly.Load(AssemblyName.GetAssemblyName(pathFull));
				}
				catch(FileNotFoundException) {
					//.NET (S3b-6): the default load context knows only the files of TestProbeHost.deps.json by name. LoadFrom loads the file into it
					//and finds its references in the same folder; a later load by the same name gets that copy.
				}
			}
			return Assembly.LoadFrom(pathFull);
		}
	}
}
