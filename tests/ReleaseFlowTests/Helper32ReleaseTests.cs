// The 32-bit helper program ships, and it is x86 (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 7): Build-Release.ps1 stages
// it as the program's own files from the helper project's Release output, and refuses a helper that is not x86 (PE machine 0x14C and, for its
// managed assembly, the CLR's 32BITREQUIRED flag); the QBFC interop is never in the program's folder. Since stage 3b (S3b-4, S3b-7) the helper is
// self-contained for win-x86 in the sub folder Helper32\ of the program folder: its apphost (OpenPmsDentalHelper32.exe, native x86), its
// assembly (.dll), its .runtimeconfig.json and .deps.json, beside its own x86 runtime, which the release's runtime part carries.
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
		private const string HELPER_DLL="OpenPmsDentalHelper32.dll";
		private const string HELPER32=@"Helper32\";

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
			foreach(string ending in new[] { ".exe",".dll",".runtimeconfig.json",".deps.json" }) {
				CollectionAssert.Contains(listLines,"own|"+HELPER32+"OpenPmsDentalHelper32"+ending);
			}
			Assert.IsFalse(listLines.Any(x => x.IndexOf("OpenPmsDentalHelper32.exe.config",StringComparison.OrdinalIgnoreCase)>=0),"No .exe.config on .NET.");
			Assert.IsFalse(listLines.Any(x => x=="own|"+HELPER),"The helper is not at the top of the program folder any more.");
			Assert.IsTrue(listLines.Any(x => x=="runtime|"+HELPER32+"coreclr.dll"),"Its x86 runtime is on the runtime list.");
			Assert.IsFalse(listLines.Any(x => x.IndexOf("Interop.QBFC",StringComparison.OrdinalIgnoreCase)>=0),"The QBFC interop is never staged.");
			string text=TestPaths.ReadSource("tools","Build-Release.ps1");
			StringAssert.Contains(text,@"'Helper32\OpenPmsDentalHelper32.exe'='OpenPmsDentalHelper32\bin\Release'","The staged helper must be its project's output.");
			StringAssert.Contains(text,@"'Helper32\OpenPmsDentalHelper32.dll'='OpenPmsDentalHelper32\bin\Release'");
			StringAssert.Contains(text,@"$X86OnlyFiles=@('Helper32\OpenPmsDentalHelper32.exe','Helper32\OpenPmsDentalHelper32.dll')");
			//The Win32 resources of every staged exe are recorded (ApphostResourceTests); the helper's line is there too.
			StringAssert.Contains(TestPaths.ReadSource("tests","ReleaseFlowTests","Fixtures","ApphostResources.net48.txt"),
				HELPER+"|asInvoker|false|1.0.0.0|0.1.1|");
		}

		///<summary>The build puts the helper's whole output (the apphost, its assembly, its two host files and its x86 runtime) into Helper32\ of the
		///program's output, byte for byte, and nothing else there (CopyHelper32 deletes a stale file first, S3b-7); the apphost is a native x86 file
		///and the assembly x86 with 32BITREQUIRED; the program's folder has no QBFC interop (the helper embeds its types).</summary>
		[TestMethod]
		public void Build_PutsTheX86HelperNextToTheProgram() {
			string folderProgram=Path.Combine(TestPaths.RepoRoot,"OpenPmsDental","bin",Configuration);
			string folderHelper=Path.Combine(folderProgram,"Helper32");
			string pathHelper=Path.Combine(folderHelper,HELPER);
			ProjectFileTests.BuildOutput.RequireFile(pathHelper,pathHelper+" is not built. Build OpenPmsDental ("+Configuration+"|x64) first.");
			string folderProject=Path.Combine(TestPaths.RepoRoot,"OpenPmsDentalHelper32","bin",Configuration);
			List<string> listProject=Directory.GetFiles(folderProject).Select(x => Path.GetFileName(x)).OrderBy(x => x,StringComparer.OrdinalIgnoreCase).ToList();
			List<string> listCopied=Directory.GetFiles(folderHelper).Select(x => Path.GetFileName(x)).OrderBy(x => x,StringComparer.OrdinalIgnoreCase).ToList();
			CollectionAssert.AreEqual(listProject,listCopied,"Helper32\\ holds exactly the helper's output.");
			Assert.AreEqual(0,Directory.GetDirectories(folderHelper).Length,"Helper32\\ is flat.");
			foreach(string fileName in new[] { HELPER,HELPER_DLL,"OpenPmsDentalHelper32.runtimeconfig.json","OpenPmsDentalHelper32.deps.json","coreclr.dll" }) {
				CollectionAssert.AreEqual(File.ReadAllBytes(Path.Combine(folderProject,fileName)),File.ReadAllBytes(Path.Combine(folderHelper,fileName)),fileName);
			}
			Assert.AreEqual(0x14C,PeFile.GetMachine(pathHelper),"The x86 apphost.");
			Assert.AreEqual(0x14C,PeFile.GetMachine(Path.Combine(folderHelper,HELPER_DLL)));
			Assert.AreEqual(2u,PeFile.GetClrFlags(Path.Combine(folderHelper,HELPER_DLL))&2u,"32BITREQUIRED: a 32-bit process on 64-bit Windows too.");
			Assert.IsFalse(File.Exists(Path.Combine(folderProgram,HELPER)),"Not at the top of the program folder.");
			Assert.IsFalse(File.Exists(Path.Combine(folderProgram,"Interop.QBFC10.dll")));
			Assert.IsFalse(File.Exists(Path.Combine(folderHelper,"Interop.QBFC10.dll")));
			//Its resources are those of the record (the Release build's are compared by ApphostResourceTests).
			string line=ExeResourceReader.Read(pathHelper).ToLine();
			StringAssert.Contains(TestPaths.ReadSource("tests","ReleaseFlowTests","Fixtures","ApphostResources.net48.txt"),line);
		}

		///<summary>CopyHelper32 (OpenPmsDental.csproj) deletes a file of Helper32\ that the helper's output does not have before it copies (the S3b-4
		///handoff to S3b-7), and both copies of the program's build list what they wrote for Clean.</summary>
		[TestMethod]
		public void CopyHelper32_DeletesStaleFilesFirst_AndListsItsWrites() {
			string project=TestPaths.ReadSource("OpenPmsDental","OpenPmsDental.csproj").Replace("\r\n","\n");
			int indexTarget=project.IndexOf("<Target Name=\"CopyHelper32\"",StringComparison.Ordinal);
			Assert.IsTrue(indexTarget>0);
			string target=project.Substring(indexTarget,project.IndexOf("</Target>",indexTarget,StringComparison.Ordinal)-indexTarget);
			StringAssert.Contains(target,"<_ODHelper32Stale Include=\"$(OutDir)Helper32\\*.*\" Exclude=\"@(_ODHelper32File->'$(OutDir)Helper32\\%(Filename)%(Extension)')\" />");
			Assert.IsTrue(target.IndexOf("<Delete Files=\"@(_ODHelper32Stale)\" />",StringComparison.Ordinal)<target.IndexOf("<Copy ",StringComparison.Ordinal),
				"The stale files go before the copy.");
			StringAssert.Contains(target,"<Output TaskParameter=\"CopiedFiles\" ItemName=\"FileWrites\" />");
			int indexCopier=project.IndexOf("<Target Name=\"CopyUpdateFileCopier\"",StringComparison.Ordinal);
			string targetCopier=project.Substring(indexCopier,project.IndexOf("</Target>",indexCopier,StringComparison.Ordinal)-indexCopier);
			StringAssert.Contains(targetCopier,"<Output TaskParameter=\"CopiedFiles\" ItemName=\"FileWrites\" />");
		}

		///<summary>Build-Release's check of a staged folder (-CheckFolder -Full) refuses a helper assembly that is AnyCPU and a helper apphost that is not
		///x86, and takes the built ones. The folder holds only the helper's two files in Helper32\, so the check reports other files missing too;
		///only the helper's lines are read here.</summary>
		[TestMethod]
		public void CheckFolder_RefusesAHelperThatIsNotX86() {
			string folderBuilt=Path.Combine(TestPaths.RepoRoot,"OpenPmsDentalHelper32","bin",Configuration);
			ProjectFileTests.BuildOutput.RequireFile(Path.Combine(folderBuilt,HELPER_DLL),Path.Combine(folderBuilt,HELPER_DLL)+" is not built. Build OpenPmsDental ("
				+Configuration+"|x64) first.");
			byte[] arrayExe=File.ReadAllBytes(Path.Combine(folderBuilt,HELPER));
			byte[] arrayDll=File.ReadAllBytes(Path.Combine(folderBuilt,HELPER_DLL));
			byte[] arrayDllAnyCpu=(byte[])arrayDll.Clone();
			int offsetFlags=PeFile.GetClrFlagsOffset(arrayDllAnyCpu);
			BitConverter.GetBytes(BitConverter.ToUInt32(arrayDllAnyCpu,offsetFlags)&~0x2u).CopyTo(arrayDllAnyCpu,offsetFlags);
			byte[] arrayExeX64=(byte[])arrayExe.Clone();
			BitConverter.GetBytes((ushort)0x8664).CopyTo(arrayExeX64,BitConverter.ToInt32(arrayExeX64,0x3C)+4);
			foreach(Tuple<byte[],byte[],string> tuple in new[] { Tuple.Create(arrayExe,arrayDll,(string)null),
				Tuple.Create(arrayExe,arrayDllAnyCpu,"Not an x86 file: "+HELPER32+HELPER_DLL+" (an AnyCPU assembly"),
				Tuple.Create(arrayExeX64,arrayDll,"Not an x86 file: "+HELPER32+HELPER+" (PE machine 0x8664)") })
			{
				string folder=TestPaths.NewTempFolder("Helper32Check");
				try {
					Directory.CreateDirectory(Path.Combine(folder,"Helper32"));
					File.WriteAllBytes(Path.Combine(folder,HELPER32+HELPER),tuple.Item1);
					File.WriteAllBytes(Path.Combine(folder,HELPER32+HELPER_DLL),tuple.Item2);
					string output=RunBuildRelease(out int exitCode,"-CheckFolder","\""+folder+"\"","-Full");
					List<string> listHelperLines=output.Split('\n').Select(x => x.Trim())
						.Where(x => x.StartsWith("Not an x86 file: "+HELPER32+"OpenPmsDentalHelper32.")).ToList();
					if(tuple.Item3==null) {
						Assert.AreEqual(0,listHelperLines.Count,output);
					}
					else {
						Assert.AreEqual(1,listHelperLines.Count,output);
						StringAssert.StartsWith(listHelperLines[0],tuple.Item3);
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
