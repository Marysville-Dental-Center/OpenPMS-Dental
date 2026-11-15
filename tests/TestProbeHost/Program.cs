// The child process of the tests that need a fresh process (S3a-8 of the .NET 10 move): it loads the test's assembly, then answers one line per
// request with the result of the handler's Handle method, until its standard input ends.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TestProbeHost {
	public static class Program {
		///<summary>args[0] is the assembly file of the handler, args[1] the handler's type, which declares public static string Handle(string
		///command,string[] args). The first line on standard output is READY, or an error reply when the handler cannot be loaded. Then each
		///line of standard input (ProbeText.Join: the command and its arguments) gets one reply line (ProbeText.FormatReply). The calls run on
		///this MTA thread, as a test method's code runs on the test runner's; work that needs STA makes its own thread.</summary>
		[MTAThread]
		public static int Main(string[] args) {
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

		///<summary>An assembly in this process's own folder is loaded by its name, so the name a config or a type string gives it later finds the
		///same copy (the test's binding redirects apply to it and to everything it loads); one elsewhere is loaded from its file.</summary>
		private static Assembly LoadAssembly(string path) {
			string pathFull=Path.GetFullPath(path);
			string folderBase=Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\');
			if(string.Equals(Path.GetDirectoryName(pathFull).TrimEnd('\\'),folderBase,StringComparison.OrdinalIgnoreCase)) {
				return Assembly.Load(AssemblyName.GetAssemblyName(pathFull));
			}
			return Assembly.LoadFrom(pathFull);
		}
	}
}
