// The x64 build of Direct2dWrapper (S3a-16 of the .NET 10 move, docs/port-notes/S3a-16.md): its x64 configurations build into
// Direct2dWrapper\bin\x64\<Configuration>\ and copy nothing, so the program's folder keeps the x86 DLL of the Win32 build; the x64 DLL exports the
// same functions as the Win32 one, every entry point the program imports among them; the x64 build treats the pointer-truncation warnings C4311
// and C4312 as errors; and the solution maps Any CPU to Win32, as x86. The x64 DLL is built by the stage gates (port note, section 4), not by
// the build lane's slnf build: a test that reads it is inconclusive while it is not built, also when OPMS_REQUIRE_BUILD_OUTPUT=1; the gate
// enforces the x64 build through its two lane build commands, each of which must exit 0. S3b-4 copies the x64 DLL into the program's output
// and removes the Win32 configurations.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectFileTests;

namespace ReleaseFlowTests {
	[TestClass]
	public class Direct2dWrapperX64Tests {
		private const string DLL="Direct2dWrapper.dll";
		private const string PROJECT_GUID="{50C36CEE-5A29-4836-8853-6BE157B3DCF3}";
		private const int MACHINE_X86=0x14C;
		private const int MACHINE_X64=0x8664;
		private static readonly string[] _arrayConfigurations=new[] { "Debug","Release" };

		///<summary>Direct2dWrapper\bin\x64\configuration\Direct2dWrapper.dll, or the test ends inconclusive. Not BuildOutput.RequireFile: no slnf
		///build and no lane step builds the x64 DLL, so OPMS_REQUIRE_BUILD_OUTPUT=1 (set by every lane test run) must not fail the test; the gate
		///enforces the x64 build through its two lane build commands (port note, section 4), each of which must exit 0.</summary>
		private static string GetX64Dll(string configuration) {
			string path=Path.Combine(TestPaths.RepoRoot,"Direct2dWrapper","bin","x64",configuration,DLL);
			if(!File.Exists(path)) {
				Assert.Inconclusive(@"Direct2dWrapper\bin\x64\"+configuration+@"\"+DLL+" is not built: build Direct2dWrapper x64 (the gate does).");
			}
			return path;
		}

		///<summary>The Win32 DLL of the configuration (Direct2dWrapper\bin\configuration\), or null when it is not built.</summary>
		private static string FindWin32Dll(string configuration) {
			string path=Path.Combine(TestPaths.RepoRoot,"Direct2dWrapper","bin",configuration,DLL);
			return File.Exists(path) ? path : null;
		}

		///<summary>The configuration's property groups and item definition groups of the project, by "Configuration|Platform".</summary>
		private static Dictionary<string,List<XElement>> ReadConfigurationGroups() {
			XDocument xDocument=XDocument.Parse(TestPaths.ReadSource("Direct2dWrapper","Direct2dWrapper.vcxproj"));
			Dictionary<string,List<XElement>> dictGroups=new Dictionary<string,List<XElement>>();
			foreach(XElement xElementGroup in xDocument.Root.Elements()) {
				Match match=Regex.Match((string)xElementGroup.Attribute("Condition")??"",@"^\s*'\$\(Configuration\)\|\$\(Platform\)'\s*==\s*'(\w+\|\w+)'\s*$");
				if(!match.Success) {
					continue;
				}
				if(!dictGroups.ContainsKey(match.Groups[1].Value)) {
					dictGroups.Add(match.Groups[1].Value,new List<XElement>());
				}
				dictGroups[match.Groups[1].Value].Add(xElementGroup);
			}
			return dictGroups;
		}

		private static List<XElement> GetDescendants(List<XElement> listGroups,string localName) {
			return listGroups.SelectMany(x => x.Descendants()).Where(x => x.Name.LocalName==localName).ToList();
		}

		///<summary>The x64 configurations copy nothing (no post-build step), the Win32 ones still copy their DLL into the program's folder of the
		///same configuration; the x64 compile treats C4311 and C4312 as errors; every configuration keeps the v143 toolset.</summary>
		[TestMethod]
		public void Project_X64CopiesNothing_AndTreatsPointerTruncationAsErrors() {
			Dictionary<string,List<XElement>> dictGroups=ReadConfigurationGroups();
			foreach(string configuration in _arrayConfigurations) {
				List<XElement> listX64=dictGroups[configuration+"|x64"];
				Assert.AreEqual(0,GetDescendants(listX64,"PostBuildEvent").Count,configuration+"|x64 copies nothing.");
				Assert.AreEqual(0,GetDescendants(listX64,"Command").Count,configuration+"|x64 runs no command.");
				List<XElement> listErrors=GetDescendants(listX64,"TreatSpecificWarningsAsErrors");
				Assert.AreEqual(1,listErrors.Count,configuration+"|x64");
				Assert.IsTrue(listErrors[0].Parent.Name.LocalName=="ClCompile",configuration+"|x64: a compiler setting.");
				CollectionAssert.IsSubsetOf(new[] { "4311","4312" },listErrors[0].Value.Split(';').Select(x => x.Trim()).ToArray(),configuration+"|x64");
				Assert.AreEqual(@"$(ProjectDir)bin\$(Platform)\$(Configuration)\",GetDescendants(listX64,"OutDir").Single().Value.Trim(),configuration+"|x64");
				List<XElement> listWin32=dictGroups[configuration+"|Win32"];
				List<XElement> listCommands=GetDescendants(listWin32,"Command");
				Assert.AreEqual(1,listCommands.Count,configuration+"|Win32");
				Assert.AreEqual("xcopy \"$(TargetDir)Direct2dWrapper.dll\" \"..\\OpenPmsDental\\bin\\"+configuration+"\\\" /Y",listCommands[0].Value.Trim(),
					configuration+"|Win32 still copies its DLL into the program's folder.");
				foreach(string platform in new[] { "Win32","x64" }) {
					Assert.AreEqual("v143",GetDescendants(dictGroups[configuration+"|"+platform],"PlatformToolset").Single().Value.Trim(),configuration+"|"+platform);
				}
			}
		}

		///<summary>In OpenDental.sln the project's Any CPU and x86 solution platforms both map to Win32, so no solution build puts the 64-bit DLL
		///beside the 32-bit program, and no solution platform builds the x64 configurations (the gate builds them from the project).</summary>
		[TestMethod]
		public void Solution_MapsEveryPlatformToWin32() {
			string text=TestPaths.ReadSource("OpenDental.sln");
			List<string> listLines=text.Split('\n').Select(x => x.Trim()).Where(x => x.StartsWith(PROJECT_GUID+".",StringComparison.OrdinalIgnoreCase)).ToList();
			List<string> listExpected=new List<string>();
			foreach(string configuration in _arrayConfigurations) {
				foreach(string platform in new[] { "Any CPU","x86" }) {
					listExpected.Add(PROJECT_GUID+"."+configuration+"|"+platform+".ActiveCfg = "+configuration+"|Win32");
					listExpected.Add(PROJECT_GUID+"."+configuration+"|"+platform+".Build.0 = "+configuration+"|Win32");
				}
			}
			CollectionAssert.AreEquivalent(listExpected,listLines);
		}

		///<summary>The x64 build's DLL of each configuration is a 64-bit file in Direct2dWrapper\bin\x64\configuration\.</summary>
		[TestMethod]
		public void X64Dll_IsAnX64File() {
			foreach(string configuration in _arrayConfigurations) {
				Assert.AreEqual(MACHINE_X64,PeExports.GetMachine(File.ReadAllBytes(GetX64Dll(configuration))),configuration);
			}
		}

		///<summary>The program's folder of the test's own configuration (bin\Debug from the build lane) has the x86 DLL of the Win32 build, byte for
		///byte, not the x64 one.</summary>
		[TestMethod]
		public void ProgramFolder_HasTheX86Dll() {
			string configuration=Path.GetFileName(Path.GetDirectoryName(typeof(Direct2dWrapperX64Tests).Assembly.Location));
			AssertProgramFolderCopy(configuration,isRequired:true);
		}

		///<summary>The other configuration's program folder (bin\Release, which Build-Release.ps1 stages from) has the x86 DLL too. Inconclusive
		///while that configuration's DLL is not built there: the build lane makes the Debug build, the Release dry run of the gate the Release one.</summary>
		[TestMethod]
		public void ProgramFolder_OtherConfiguration_HasTheX86Dll() {
			string configurationTests=Path.GetFileName(Path.GetDirectoryName(typeof(Direct2dWrapperX64Tests).Assembly.Location));
			string configuration=configurationTests.Equals("Release",StringComparison.OrdinalIgnoreCase) ? "Debug" : "Release";
			AssertProgramFolderCopy(configuration,isRequired:false);
		}

		private static void AssertProgramFolderCopy(string configuration,bool isRequired) {
			string path=Path.Combine(TestPaths.RepoRoot,"OpenPmsDental","bin",configuration,DLL);
			string message=@"OpenPmsDental\bin\"+configuration+@"\"+DLL+" is not built. Build OpenDental.Local.slnf ("+configuration+"|x86) first.";
			if(!File.Exists(path)) {
				if(isRequired) {
					BuildOutput.NotBuilt(message);
				}
				Assert.Inconclusive(message);
			}
			byte[] arrayCopy=File.ReadAllBytes(path);
			Assert.AreEqual(MACHINE_X86,PeExports.GetMachine(arrayCopy),path);
			string pathWin32=FindWin32Dll(configuration);
			Assert.IsNotNull(pathWin32,@"The program's folder has a "+DLL+" but Direct2dWrapper\\bin\\"+configuration+" has none.");
			CollectionAssert.AreEqual(File.ReadAllBytes(pathWin32),arrayCopy,"The program's folder has the Win32 build's DLL of "+configuration+".");
		}

		///<summary>The x64 DLL of each configuration exports exactly the functions of the Win32 DLL (of the same configuration when it is built,
		///otherwise of the other one: the exports do not depend on it), and every entry point the program imports from Direct2dWrapper.dll is among
		///them. The imports are read from the built program with Mono.Cecil and cross-checked with the DllImports of Direct2d.cs, its comments
		///removed (one block of commented-out imports is there).</summary>
		[TestMethod]
		public void X64Exports_EqualTheWin32Exports_AndHoldEveryImport() {
			List<string> listImports=ReadImportsFromProgram();
			CollectionAssert.AreEquivalent(ReadImportsFromSource(),listImports,"The program's imports from "+DLL+" are those of Direct2d.cs.");
			Assert.IsTrue(listImports.Count>=60,"Direct2d.cs imports "+listImports.Count+" functions.");
			foreach(string configuration in _arrayConfigurations) {
				List<string> listX64=PeExports.ReadExportNames(File.ReadAllBytes(GetX64Dll(configuration)));
				string pathWin32=FindWin32Dll(configuration)??FindWin32Dll(_arrayConfigurations.First(x => x!=configuration));
				if(pathWin32==null) {
					BuildOutput.NotBuilt(@"Direct2dWrapper\bin\Debug\"+DLL+" is not built. Build OpenDental.Local.slnf (Debug|x86) first.");
				}
				List<string> listWin32=PeExports.ReadExportNames(File.ReadAllBytes(pathWin32));
				CollectionAssert.AreEqual(listWin32,listX64,configuration+": the x64 exports equal "+pathWin32+"'s.");
				List<string> listMissing=listImports.Except(listX64,StringComparer.Ordinal).ToList();
				Assert.AreEqual(0,listMissing.Count,configuration+": imported, not exported by the x64 DLL: "+string.Join(", ",listMissing));
			}
		}

		///<summary>The entry points of every P/Invoke method of the built program whose module is Direct2dWrapper.dll, sorted, distinct.</summary>
		private static List<string> ReadImportsFromProgram() {
			string configuration=Path.GetFileName(Path.GetDirectoryName(typeof(Direct2dWrapperX64Tests).Assembly.Location));
			string path=ProgramAssembly.GetPath(Path.Combine(TestPaths.RepoRoot,"OpenPmsDental","bin",configuration),"OpenPmsDental");
			List<string> listEntryPoints=new List<string>();
			//Read from a copy of the bytes, so the program's file is neither loaded nor locked.
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
			using(Mono.Cecil.ModuleDefinition moduleDefinition=Mono.Cecil.ModuleDefinition.ReadModule(memoryStream)) {
				foreach(Mono.Cecil.TypeDefinition typeDefinition in moduleDefinition.GetTypes()) {
					foreach(Mono.Cecil.MethodDefinition methodDefinition in typeDefinition.Methods.Where(x => x.IsPInvokeImpl && x.PInvokeInfo!=null)) {
						if(!methodDefinition.PInvokeInfo.Module.Name.Equals(DLL,StringComparison.OrdinalIgnoreCase)) {
							continue;
						}
						Assert.AreEqual("OpenPmsDental.UI",typeDefinition.Namespace,methodDefinition.FullName+": only Direct2d.cs imports from "+DLL+".");
						listEntryPoints.Add(methodDefinition.PInvokeInfo.EntryPoint??methodDefinition.Name);
					}
				}
			}
			return listEntryPoints.Distinct(StringComparer.Ordinal).OrderBy(x => x,StringComparer.Ordinal).ToList();
		}

		///<summary>The functions Direct2d.cs imports from Direct2dWrapper.dll, from its source without comments: the EntryPoint of each DllImport,
		///or the name of the extern method that follows it. Sorted, distinct.</summary>
		private static List<string> ReadImportsFromSource() {
			string source=TestPaths.ReadSource("OpenPmsDental","UI","Direct2d","Direct2d.cs");
			//No string in the file holds "//" or "/*", so removing the comments with two patterns is safe here.
			source=Regex.Replace(source,@"/\*.*?\*/","",RegexOptions.Singleline);
			source=Regex.Replace(source,@"//[^\n]*","");
			List<string> listEntryPoints=new List<string>();
			MatchCollection matchCollection=Regex.Matches(source,
				@"\[DllImport\(\s*""Direct2dWrapper\.dll""(?<args>[^\]]*)\)\]\s*(\[return:[^\]]*\]\s*)?public\s+static\s+extern\s+[\w\.\[\]]+\s+(?<name>\w+)\s*\(");
			foreach(Match match in matchCollection) {
				Match matchEntryPoint=Regex.Match(match.Groups["args"].Value,@"EntryPoint\s*=\s*""(?<entry>[^""]+)""");
				listEntryPoints.Add(matchEntryPoint.Success ? matchEntryPoint.Groups["entry"].Value : match.Groups["name"].Value);
			}
			Assert.AreEqual(Regex.Matches(source,@"\[DllImport\(\s*""Direct2dWrapper\.dll""").Count,matchCollection.Count,
				"Every DllImport of Direct2d.cs is read.");
			return listEntryPoints.Distinct(StringComparer.Ordinal).OrderBy(x => x,StringComparer.Ordinal).ToList();
		}

		///<summary>The reader itself, on the test host's own files: a managed DLL of this test exports nothing, kernel32.dll of the process's
		///bitness exports its well-known functions, and its machine is the process's.</summary>
		[TestMethod]
		public void PeExports_ReadsMachineAndNames() {
			Assert.AreEqual(0,PeExports.ReadExportNames(File.ReadAllBytes(typeof(Direct2dWrapperX64Tests).Assembly.Location)).Count);
			string pathKernel32=Path.Combine(Environment.SystemDirectory,"kernel32.dll");
			byte[] arrayKernel32=File.ReadAllBytes(pathKernel32);
			Assert.AreEqual(Environment.Is64BitProcess ? MACHINE_X64 : MACHINE_X86,PeExports.GetMachine(arrayKernel32));
			List<string> listNames=PeExports.ReadExportNames(arrayKernel32);
			CollectionAssert.Contains(listNames,"CreateFileW");
			CollectionAssert.Contains(listNames,"GetProcAddress");
			Assert.IsTrue(listNames.Count>1000,"kernel32.dll exports "+listNames.Count+" names.");
		}
	}

	///<summary>A reader of a PE file's machine and export names (the export directory, data directory 0 of the optional header), for 32-bit (PE32)
	///and 64-bit (PE32+) files.</summary>
	internal static class PeExports {
		public static int GetMachine(byte[] arrayBytes) {
			return BitConverter.ToUInt16(arrayBytes,GetPeOffset(arrayBytes)+4);
		}

		///<summary>The names in the export table, sorted by ordinal comparison of their ASCII text. Empty when the file has no export table.</summary>
		public static List<string> ReadExportNames(byte[] arrayBytes) {
			int offsetPe=GetPeOffset(arrayBytes);
			int offsetOptional=offsetPe+24;
			int magic=BitConverter.ToUInt16(arrayBytes,offsetOptional);
			Assert.IsTrue(magic==0x10B || magic==0x20B,"Not a PE32 or PE32+ optional header: 0x"+magic.ToString("X"));
			int offsetDirectories=offsetOptional+(magic==0x20B ? 112 : 96);
			int countDirectories=(int)BitConverter.ToUInt32(arrayBytes,offsetOptional+(magic==0x20B ? 108 : 92));
			List<string> listNames=new List<string>();
			if(countDirectories<1) {
				return listNames;
			}
			uint rvaExports=BitConverter.ToUInt32(arrayBytes,offsetDirectories);
			if(rvaExports==0) {
				return listNames;
			}
			int offsetExports=MapRva(arrayBytes,rvaExports);
			int countNames=(int)BitConverter.ToUInt32(arrayBytes,offsetExports+24);
			int offsetNames=MapRva(arrayBytes,BitConverter.ToUInt32(arrayBytes,offsetExports+32));
			for(int i=0;i<countNames;i++) {
				int offsetName=MapRva(arrayBytes,BitConverter.ToUInt32(arrayBytes,offsetNames+i*4));
				int end=Array.IndexOf(arrayBytes,(byte)0,offsetName);
				listNames.Add(Encoding.ASCII.GetString(arrayBytes,offsetName,end-offsetName));
			}
			return listNames.OrderBy(x => x,StringComparer.Ordinal).ToList();
		}

		private static int GetPeOffset(byte[] arrayBytes) {
			Assert.AreEqual((ushort)0x5A4D,BitConverter.ToUInt16(arrayBytes,0),"No MZ header.");
			int offsetPe=BitConverter.ToInt32(arrayBytes,0x3C);
			Assert.AreEqual(0x00004550u,BitConverter.ToUInt32(arrayBytes,offsetPe),"No PE signature.");
			return offsetPe;
		}

		///<summary>The file offset of a relative virtual address, through the section table.</summary>
		private static int MapRva(byte[] arrayBytes,uint rva) {
			int offsetPe=GetPeOffset(arrayBytes);
			int countSections=BitConverter.ToUInt16(arrayBytes,offsetPe+6);
			int sizeOptional=BitConverter.ToUInt16(arrayBytes,offsetPe+20);
			int offsetSections=offsetPe+24+sizeOptional;
			for(int i=0;i<countSections;i++) {
				int offsetSection=offsetSections+i*40;
				uint sizeVirtual=BitConverter.ToUInt32(arrayBytes,offsetSection+8);
				uint addressVirtual=BitConverter.ToUInt32(arrayBytes,offsetSection+12);
				uint sizeRaw=BitConverter.ToUInt32(arrayBytes,offsetSection+16);
				uint pointerRaw=BitConverter.ToUInt32(arrayBytes,offsetSection+20);
				if(rva>=addressVirtual && rva<addressVirtual+Math.Max(sizeVirtual,sizeRaw)) {
					return (int)(pointerRaw+(rva-addressVirtual));
				}
			}
			throw new InvalidDataException("RVA 0x"+rva.ToString("X")+" is in no section.");
		}
	}
}
