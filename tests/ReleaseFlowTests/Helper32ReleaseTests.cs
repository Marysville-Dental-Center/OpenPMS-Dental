// The 32-bit helper program ships, and it is x86 (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 7): Build-Release.ps1 stages
// OpenPmsDentalHelper32.exe and its .exe.config as the program's own files, from the helper project's Release output, and refuses a helper that
// is not x86 (PE machine 0x14C and, for a managed file, the CLR's 32BITREQUIRED flag), also after stage 3b; the build puts the helper next to
// the program; the QBFC interop is never in the program's folder.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ReleaseFlowTests {
	[TestClass]
	public class Helper32ReleaseTests {
		private const string HELPER="OpenPmsDentalHelper32.exe";

		private static string Configuration {
			get {
				return Path.GetFileName(Path.GetDirectoryName(typeof(Helper32ReleaseTests).Assembly.Location));
			}
		}

		private static string RunBuildRelease(out int exitCode,params string[] arrayArgs) {
			string script=Path.Combine(TestPaths.RepoRoot,"tools","Build-Release.ps1");
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe");
			processStartInfo.Arguments="-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \""+script+"\" "+string.Join(" ",arrayArgs);
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.CreateNoWindow=true;
			using(Process process=Process.Start(processStartInfo)) {
				Task<string> taskOut=process.StandardOutput.ReadToEndAsync();
				Task<string> taskErr=process.StandardError.ReadToEndAsync();
				if(!process.WaitForExit(300000)) {
					process.Kill();
					Assert.Fail("Build-Release.ps1 did not finish in 5 minutes.");
				}
				process.WaitForExit();
				exitCode=process.ExitCode;
				return taskOut.Result+taskErr.Result;
			}
		}

		[TestMethod]
		public void AllowList_ShipsTheHelperAndItsConfig_AsTheProgramsOwnFiles() {
			string output=RunBuildRelease(out int exitCode,"-ListFiles");
			Assert.AreEqual(0,exitCode,output);
			List<string> listLines=output.Split('\n').Select(x => x.Trim()).ToList();
			CollectionAssert.Contains(listLines,"own|"+HELPER);
			CollectionAssert.Contains(listLines,"own|"+HELPER+".config");
			Assert.IsFalse(listLines.Any(x => x.IndexOf("Interop.QBFC",StringComparison.OrdinalIgnoreCase)>=0),"The QBFC interop is never staged.");
			string text=TestPaths.ReadSource("tools","Build-Release.ps1");
			StringAssert.Contains(text,@"'OpenPmsDentalHelper32.exe'='OpenPmsDentalHelper32\bin\Release'","The staged helper must be its project's output.");
			StringAssert.Contains(text,@"'OpenPmsDentalHelper32.exe.config'='OpenPmsDentalHelper32\bin\Release'");
			StringAssert.Contains(text,"$X86OnlyFiles=@('OpenPmsDentalHelper32.exe')");
			//The Win32 resources of every staged exe are recorded (ApphostResourceTests); the helper's line is there too.
			StringAssert.Contains(TestPaths.ReadSource("tests","ReleaseFlowTests","Fixtures","ApphostResources.net48.txt"),
				HELPER+"|asInvoker|false|1.0.0.0|0.0|");
		}

		///<summary>The build puts the helper and its config next to the program, byte for byte the helper project's output; it is an x86 file; and
		///the program's folder has no QBFC interop (the helper embeds its types).</summary>
		[TestMethod]
		public void Build_PutsTheX86HelperNextToTheProgram() {
			string folderProgram=Path.Combine(TestPaths.RepoRoot,"OpenPmsDental","bin",Configuration);
			string pathHelper=Path.Combine(folderProgram,HELPER);
			ProjectFileTests.BuildOutput.RequireFile(pathHelper,pathHelper+" is not built. Build OpenDental.Local.slnf ("+Configuration+"|x86) first.");
			string pathProject=Path.Combine(TestPaths.RepoRoot,"OpenPmsDentalHelper32","bin",Configuration,HELPER);
			CollectionAssert.AreEqual(File.ReadAllBytes(pathProject),File.ReadAllBytes(pathHelper));
			CollectionAssert.AreEqual(File.ReadAllBytes(pathProject+".config"),File.ReadAllBytes(pathHelper+".config"));
			Assert.AreEqual(0x14C,PeFile.GetMachine(pathHelper));
			Assert.AreEqual(2u,PeFile.GetClrFlags(pathHelper)&2u,"32BITREQUIRED: a 32-bit process on 64-bit Windows too.");
			Assert.IsFalse(File.Exists(Path.Combine(folderProgram,"Interop.QBFC10.dll")));
			//Its resources are those of the record (the Release build's are compared by ApphostResourceTests).
			string line=ExeResourceReader.Read(pathHelper).ToLine();
			StringAssert.Contains(TestPaths.ReadSource("tests","ReleaseFlowTests","Fixtures","ApphostResources.net48.txt"),line);
		}

		///<summary>Build-Release's check of a staged folder (-CheckFolder -Full) refuses a helper that is AnyCPU or not x86, and takes the built one.
		///The folder holds only the helper, so the check reports other files missing too; only the helper's lines are read here.</summary>
		[TestMethod]
		public void CheckFolder_RefusesAHelperThatIsNotX86() {
			string pathBuilt=Path.Combine(TestPaths.RepoRoot,"OpenPmsDentalHelper32","bin",Configuration,HELPER);
			ProjectFileTests.BuildOutput.RequireFile(pathBuilt,pathBuilt+" is not built. Build OpenDental.Local.slnf ("+Configuration+"|x86) first.");
			byte[] arrayBuilt=File.ReadAllBytes(pathBuilt);
			byte[] arrayAnyCpu=(byte[])arrayBuilt.Clone();
			int offsetFlags=PeFile.GetClrFlagsOffset(arrayAnyCpu);
			BitConverter.GetBytes(BitConverter.ToUInt32(arrayAnyCpu,offsetFlags)&~0x2u).CopyTo(arrayAnyCpu,offsetFlags);
			byte[] arrayX64=(byte[])arrayBuilt.Clone();
			BitConverter.GetBytes((ushort)0x8664).CopyTo(arrayX64,BitConverter.ToInt32(arrayX64,0x3C)+4);
			foreach(Tuple<byte[],string> tuple in new[] { Tuple.Create(arrayBuilt,(string)null),
				Tuple.Create(arrayAnyCpu,"Not an x86 file: "+HELPER+" (an AnyCPU assembly"),
				Tuple.Create(arrayX64,"Not an x86 file: "+HELPER+" (PE machine 0x8664)") })
			{
				string folder=TestPaths.NewTempFolder("Helper32Check");
				try {
					File.WriteAllBytes(Path.Combine(folder,HELPER),tuple.Item1);
					string output=RunBuildRelease(out int exitCode,"-CheckFolder","\""+folder+"\"","-Full");
					List<string> listHelperLines=output.Split('\n').Select(x => x.Trim()).Where(x => x.StartsWith("Not an x86 file: "+HELPER)).ToList();
					if(tuple.Item2==null) {
						Assert.AreEqual(0,listHelperLines.Count,output);
					}
					else {
						Assert.AreEqual(1,listHelperLines.Count,output);
						StringAssert.StartsWith(listHelperLines[0],tuple.Item2);
						Assert.AreNotEqual(0,exitCode);
					}
				}
				finally {
					Directory.Delete(folder,true);
				}
			}
		}
	}

	///<summary>The PE header fields the release checks read.</summary>
	internal static class PeFile {
		public static int GetMachine(string path) {
			byte[] arrayBytes=File.ReadAllBytes(path);
			return BitConverter.ToUInt16(arrayBytes,BitConverter.ToInt32(arrayBytes,0x3C)+4);
		}

		public static uint GetClrFlags(string path) {
			byte[] arrayBytes=File.ReadAllBytes(path);
			return BitConverter.ToUInt32(arrayBytes,GetClrFlagsOffset(arrayBytes));
		}

		///<summary>The file offset of the CLR header's Flags (directory 14 of the optional header, mapped through the section table).</summary>
		public static int GetClrFlagsOffset(byte[] arrayBytes) {
			int offsetPe=BitConverter.ToInt32(arrayBytes,0x3C);
			int countSections=BitConverter.ToUInt16(arrayBytes,offsetPe+6);
			int sizeOptional=BitConverter.ToUInt16(arrayBytes,offsetPe+20);
			int offsetOptional=offsetPe+24;
			int magic=BitConverter.ToUInt16(arrayBytes,offsetOptional);
			int offsetDirectories=offsetOptional+(magic==0x20B ? 112 : 96);
			uint rva=BitConverter.ToUInt32(arrayBytes,offsetDirectories+14*8);
			Assert.AreNotEqual(0u,rva,"No CLR header.");
			int offsetSections=offsetOptional+sizeOptional;
			for(int i=0;i<countSections;i++) {
				int offsetSection=offsetSections+i*40;
				uint sizeVirtual=BitConverter.ToUInt32(arrayBytes,offsetSection+8);
				uint addressVirtual=BitConverter.ToUInt32(arrayBytes,offsetSection+12);
				uint pointerRaw=BitConverter.ToUInt32(arrayBytes,offsetSection+20);
				if(rva>=addressVirtual && rva<addressVirtual+sizeVirtual) {
					return (int)(pointerRaw+(rva-addressVirtual))+16;
				}
			}
			throw new InvalidDataException("The CLR header is in no section.");
		}
	}
}
