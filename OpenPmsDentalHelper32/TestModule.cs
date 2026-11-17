// A module of a test assembly (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 5): the protocol tests' fake module.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Reflection;

namespace OpenPmsDentalHelper32 {
	///<summary>Only with --test-module, which the program never passes: the helper loads an assembly of its own folder by name, whose name ends
	///with "Tests" (a test assembly next to the copy of the helper that the test started), and calls its type's
	///public static string[] Handle(string command,string[] args,byte[] data,out byte[] dataReply) for each request to the module. The
	///program's folder holds no test assembly, and whoever can write there can replace the program itself, so this loads nothing the program
	///would not.</summary>
	internal sealed class TestModule:IHelper32Module {
		public const string NAME_DEFAULT="Test";
		private readonly string _name;
		private readonly MethodInfo _methodHandle;

		private TestModule(string name,MethodInfo methodHandle) {
			_name=name;
			_methodHandle=methodHandle;
		}

		public string Name {
			get {
				return _name;
			}
		}

		public static TestModule Load(string assemblyName,string typeName,string moduleName) {
			if(!assemblyName.EndsWith("Tests",StringComparison.Ordinal) || assemblyName.IndexOfAny(new[] { '\\','/',':',',' })>=0) {
				throw new ArgumentException("A test module comes from a test assembly of the helper's folder, by name: "+assemblyName);
			}
			if(moduleName==Helper32Protocol.MODULE_HOST) {
				throw new ArgumentException("A test module cannot take the place of the helper's own module.");
			}
			string folder=AppDomain.CurrentDomain.BaseDirectory;
			string path=Path.Combine(folder,assemblyName+".dll");
			if(!File.Exists(path)) {
				throw new FileNotFoundException("No test assembly "+assemblyName+".dll in the helper's folder.");
			}
			//By its path in the helper's folder: on .NET the default load context knows only the assemblies of the helper's .deps.json, so a load
			//by name finds no other file of the folder, as the .NET Framework's probing of the application base did (S3b-5).
			Assembly assembly=Assembly.LoadFrom(path);
			Type type=assembly.GetType(typeName,true);
			MethodInfo methodHandle=type.GetMethod("Handle",BindingFlags.Public|BindingFlags.Static,null,
				new[] { typeof(string),typeof(string[]),typeof(byte[]),typeof(byte[]).MakeByRefType() },null);
			if(methodHandle==null || methodHandle.ReturnType!=typeof(string[])) {
				throw new MissingMethodException(typeName,"Handle");
			}
			return new TestModule(moduleName,methodHandle);
		}

		public Helper32Reply Handle(Helper32Request request) {
			object[] arrayParameters=new object[] { request.Command,request.Args,request.Data,null };
			string[] arrayResults;
			try {
				arrayResults=(string[])_methodHandle.Invoke(null,arrayParameters);
			}
			catch(TargetInvocationException exception) when(exception.InnerException!=null) {
				throw Rethrow(exception.InnerException);
			}
			return Helper32Reply.Ok(arrayResults,(byte[])arrayParameters[3]);
		}

		private static Exception Rethrow(Exception exception) {
			System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
			return exception;//not reached
		}
	}
}
