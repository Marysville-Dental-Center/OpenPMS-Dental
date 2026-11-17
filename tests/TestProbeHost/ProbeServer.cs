// The child's side of the line protocol (ProbeText): load the handler's assembly, say READY, then answer one line per request with the result of
// the handler's Handle method until standard input ends. TestProbeHost's own Main serves this way (S3a-8), and so does the startup hook that
// serves inside a real program's process (StartupHook, WINCHK-5): the same protocol, so ProbeProcess talks to both alike.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TestProbeHost {
	internal static class ProbeServer {
		///<summary>Loads typeName from the assembly file and serves its public static string Handle(string command,string[] args). The first line on
		///standard output is READY, or an error reply when the handler cannot be loaded (exit code 3). Then each line of standard input
		///(ProbeText.Join: the command and its arguments) gets one reply line (ProbeText.FormatReply). Returns 0 when the test closes standard input.
		///What the work writes to the console goes to standard error, which the test keeps for its messages. The calls run on the calling thread.
		///</summary>
		public static int Serve(string pathAssembly,string typeName) {
			StreamWriter streamWriterReplies=new StreamWriter(Console.OpenStandardOutput(),new UTF8Encoding(false));
			streamWriterReplies.AutoFlush=true;
			Console.SetOut(Console.Error);
			MethodInfo methodInfoHandle;
			try {
				Type type=LoadAssembly(pathAssembly).GetType(typeName,true);
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
		public static Assembly LoadAssembly(string path) {
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
