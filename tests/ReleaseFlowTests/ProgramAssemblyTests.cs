// The helper that finds a built program's managed assembly (tests\ProjectFileTests\ProgramAssembly.cs, S3a-8 of the .NET 10 move): the .dll when
// it is there (on .NET the .exe is the native apphost), else the .exe, and a clear failure for an apphost without its .dll. In scratch folders
// under the temp folder; nothing of the build output is touched.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectFileTests;

namespace ReleaseFlowTests {
	[TestClass]
	public class ProgramAssemblyTests {
		///<summary>A managed assembly to copy: CodeBase.dll, beside this test assembly.</summary>
		private static string ManagedFile {
			get { return typeof(CodeBase.ODBuild).Assembly.Location; }
		}

		///<summary>A native program to copy, as an apphost is one: cmd.exe of Windows.</summary>
		private static string NativeFile {
			get { return Path.Combine(Environment.SystemDirectory,"cmd.exe"); }
		}

		private static void DeleteQuietly(string folder) {
			try {
				Directory.Delete(folder,true);
			}
			catch(Exception) {
				//Left in the temp folder.
			}
		}

		[TestMethod]
		public void Find_TakesTheDll_WhenThereIsOne() {
			string folder=TestPaths.NewTempFolder("programassembly");
			try {
				//.NET: the native apphost and the managed .dll.
				File.Copy(NativeFile,Path.Combine(folder,"Prog.exe"));
				File.Copy(ManagedFile,Path.Combine(folder,"Prog.dll"));
				Assert.AreEqual(Path.Combine(folder,"Prog.dll"),ProgramAssembly.Find(folder,"Prog"));
				Assert.AreEqual(Path.Combine(folder,"Prog.dll"),ProgramAssembly.GetPath(folder,"Prog"));
				//.NET Framework: the managed .exe alone.
				File.Delete(Path.Combine(folder,"Prog.dll"));
				File.Delete(Path.Combine(folder,"Prog.exe"));
				File.Copy(ManagedFile,Path.Combine(folder,"Prog.exe"));
				Assert.AreEqual(Path.Combine(folder,"Prog.exe"),ProgramAssembly.Find(folder,"Prog"));
				Assert.AreEqual(Path.Combine(folder,"Prog.exe"),ProgramAssembly.GetPath(folder,"Prog"));
				//Neither: not built.
				Assert.IsNull(ProgramAssembly.Find(folder,"Other"));
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		[TestMethod]
		public void GetPath_FailsForAnApphostWithoutItsDll() {
			string folder=TestPaths.NewTempFolder("programassembly");
			try {
				File.Copy(NativeFile,Path.Combine(folder,"Prog.exe"));
				Assert.AreEqual(Path.Combine(folder,"Prog.exe"),ProgramAssembly.Find(folder,"Prog"));
				AssertFailedException assertFailedException=Assert.ThrowsExactly<AssertFailedException>(() => ProgramAssembly.GetPath(folder,"Prog"));
				StringAssert.Contains(assertFailedException.Message,"holds no managed code");
				StringAssert.Contains(assertFailedException.Message,"Prog.dll is not beside it");
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		[TestMethod]
		public void GetPath_NotBuilt_FollowsTheBuildOutputRule() {
			string folder=TestPaths.NewTempFolder("programassembly");
			try {
				//Inconclusive on a developer's run, a failure with OPMS_REQUIRE_BUILD_OUTPUT=1 (BuildOutput.NotBuilt); either way it names the program.
				UnitTestAssertException unitTestAssertException=null;
				try {
					ProgramAssembly.GetPath(folder,"Prog");
				}
				catch(UnitTestAssertException ex) {
					unitTestAssertException=ex;
				}
				Assert.IsNotNull(unitTestAssertException,"A program that is not built ends the test.");
				Assert.AreEqual(BuildOutput.IsRequired ? typeof(AssertFailedException) : typeof(AssertInconclusiveException),unitTestAssertException.GetType());
				StringAssert.Contains(unitTestAssertException.Message,"Prog is not built in "+folder);
			}
			finally {
				DeleteQuietly(folder);
			}
		}
	}
}
