// The managed assembly of a built program (S3a-8 of the .NET 10 move, docs/port-notes/S3a-8.md): on .NET Framework the program's .exe is its
// assembly; on .NET the .exe is the native apphost and the code is in the .dll beside it (OpenPmsDental.exe starts OpenPmsDental.dll). The
// tests that load or read a program's code take the .dll when it is there and the .exe otherwise. Linked into every test project with such a
// test, as BuildOutput.cs is.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectFileTests {
	internal static class ProgramAssembly {
		///<summary>The file of the program's managed assembly in the folder: programName.dll when it is there, else programName.exe when that is
		///there, else null (not built). programName has no extension, for example "OpenPmsDental".</summary>
		public static string Find(string folder,string programName) {
			string pathDll=Path.Combine(folder,programName+".dll");
			if(File.Exists(pathDll)) {
				return pathDll;
			}
			string pathExe=Path.Combine(folder,programName+".exe");
			if(File.Exists(pathExe)) {
				return pathExe;
			}
			return null;
		}

		///<summary>Find for a folder relative to root, for the tests that read a built program's references (S3a-8b): the path of the program's
		///managed assembly relative to root, or folderRelative\programName.exe when neither file is built, so the caller's own "not built" check and
		///message stay as they were. For example FindRelative(root,@"OpenPmsDental\bin\Debug","OpenPmsDental").</summary>
		public static string FindRelative(string root,string folderRelative,string programName) {
			string path=Find(Path.Combine(root,folderRelative),programName);
			return Path.Combine(folderRelative,path==null ? programName+".exe" : Path.GetFileName(path));
		}

		///<summary>Find, for a test: the file, or the test ends through BuildOutput.NotBuilt when neither file is built, and fails when the file
		///found holds no managed code (an apphost without its .dll).</summary>
		public static string GetPath(string folder,string programName) {
			string path=Find(folder,programName);
			if(path==null) {
				BuildOutput.NotBuilt(programName+" is not built in "+folder+". Build OpenDental.Local.slnf (Debug|x86) first.");
			}
			try {
				AssemblyName.GetAssemblyName(path);
			}
			catch(BadImageFormatException) {
				Assert.Fail(path+" holds no managed code (a native apphost?), and "+programName+".dll is not beside it.");
			}
			return path;
		}

		///<summary>GetPath, loaded into this process to be looked at by reflection (nothing in it runs).</summary>
		public static Assembly Load(string folder,string programName) {
			return Assembly.LoadFrom(GetPath(folder,programName));
		}
	}
}
