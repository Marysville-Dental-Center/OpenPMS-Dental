// CodeBase.PortableExecutableFile (N64-A): the PE header, CLI flags, metadata version, TargetFrameworkAttribute and core library of a file,
// read without loading it. Checked on the synthetic plug-ins and against Mono.Cecil on every assembly of this test's output folder; native
// files are read as native. Runs in the x86 test run and in the lane's x64 test build.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;

namespace N64ATests {
	[TestClass]
	public class PortableExecutableFileTests {
		private static string _folder;

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			_folder=TestSupport.CreateTempFolder("n64a_pe");
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			TestSupport.DeleteFolder(_folder);
		}

		[TestMethod]
		public void Synthetic_AnyCpu_IsIlOnlyPe32X86WithoutFlags() {
			string path=TestSupport.WritePlugin(_folder,"N64aPeAnyCpu",SyntheticPlatform.AnyCpu,".NETFramework,Version=v4.8");
			PortableExecutableFile portableExecutableFile=PortableExecutableFile.Read(path);
			Assert.IsTrue(portableExecutableFile.IsPe);
			Assert.IsTrue(portableExecutableFile.IsManaged);
			Assert.AreEqual(PortableExecutableFile.IMAGE_FILE_MACHINE_I386,portableExecutableFile.Machine);
			Assert.IsFalse(portableExecutableFile.IsPe32Plus);
			Assert.IsTrue(portableExecutableFile.IsIlOnly);
			Assert.IsFalse(portableExecutableFile.Is32BitRequired);
			Assert.IsTrue(portableExecutableFile.IsAnyCpu);
			Assert.AreEqual("v4.0.30319",portableExecutableFile.MetadataVersion);
			Assert.AreEqual(".NETFramework,Version=v4.8",portableExecutableFile.TargetFramework);
			Assert.AreEqual("mscorlib",portableExecutableFile.CoreLibraryName);
			Assert.AreEqual(new Version(4,0,0,0),portableExecutableFile.CoreLibraryVersion);
			Assert.AreEqual("",portableExecutableFile.MetadataError);
			Assert.IsTrue(portableExecutableFile.CanLoadInProcess(PortableExecutableFile.IMAGE_FILE_MACHINE_I386));
			Assert.IsTrue(portableExecutableFile.CanLoadInProcess(PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64));
			Assert.AreEqual("a .NET assembly for any CPU",portableExecutableFile.DescribeBitness());
		}

		[TestMethod]
		public void Synthetic_X86_Is32BitRequired_LoadsOnlyInX86() {
			string path=TestSupport.WritePlugin(_folder,"N64aPeX86",SyntheticPlatform.X86,".NETFramework,Version=v4.8");
			PortableExecutableFile portableExecutableFile=PortableExecutableFile.Read(path);
			Assert.IsTrue(portableExecutableFile.IsManaged);
			Assert.AreEqual(PortableExecutableFile.IMAGE_FILE_MACHINE_I386,portableExecutableFile.Machine);
			Assert.IsTrue(portableExecutableFile.Is32BitRequired);
			Assert.IsFalse(portableExecutableFile.Is32BitPreferred);
			Assert.IsFalse(portableExecutableFile.IsAnyCpu);
			Assert.IsTrue(portableExecutableFile.CanLoadInProcess(PortableExecutableFile.IMAGE_FILE_MACHINE_I386));
			Assert.IsFalse(portableExecutableFile.CanLoadInProcess(PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64));
			Assert.AreEqual("a 32-bit only (x86) .NET assembly",portableExecutableFile.DescribeBitness());
		}

		///<summary>32BITREQUIRED with 32BITPREFERRED is "any CPU, 32-bit preferred": a 64-bit .NET Framework process loads such a DLL (the x64 run of
		///PluginLoadTests.EachBitness_RefusedExactlyWhenThisProcessCannotLoadIt), so the reader calls it any CPU.</summary>
		[TestMethod]
		public void Synthetic_X86Preferred_IsAnyCpu32BitPreferred() {
			string path=TestSupport.WritePlugin(_folder,"N64aPeX86Preferred",SyntheticPlatform.X86Preferred,".NETFramework,Version=v4.8");
			PortableExecutableFile portableExecutableFile=PortableExecutableFile.Read(path);
			Assert.IsTrue(portableExecutableFile.Is32BitRequired);
			Assert.IsTrue(portableExecutableFile.Is32BitPreferred);
			Assert.IsTrue(portableExecutableFile.IsAnyCpu);
			Assert.IsTrue(portableExecutableFile.CanLoadInProcess(PortableExecutableFile.IMAGE_FILE_MACHINE_I386));
			Assert.IsTrue(portableExecutableFile.CanLoadInProcess(PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64));
			Assert.AreEqual("a .NET assembly for any CPU, 32-bit preferred",portableExecutableFile.DescribeBitness());
		}

		[TestMethod]
		public void Synthetic_X64_IsPe32PlusAmd64_LoadsOnlyInX64() {
			string path=TestSupport.WritePlugin(_folder,"N64aPeX64",SyntheticPlatform.X64,".NETCoreApp,Version=v10.0");
			PortableExecutableFile portableExecutableFile=PortableExecutableFile.Read(path);
			Assert.IsTrue(portableExecutableFile.IsManaged);
			Assert.AreEqual(PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64,portableExecutableFile.Machine);
			Assert.IsTrue(portableExecutableFile.IsPe32Plus);
			Assert.IsTrue(portableExecutableFile.IsIlOnly);
			Assert.IsFalse(portableExecutableFile.IsAnyCpu);
			Assert.IsFalse(portableExecutableFile.CanLoadInProcess(PortableExecutableFile.IMAGE_FILE_MACHINE_I386));
			Assert.IsTrue(portableExecutableFile.CanLoadInProcess(PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64));
			Assert.AreEqual(".NETCoreApp,Version=v10.0",portableExecutableFile.TargetFramework);
			Assert.AreEqual("a 64-bit (x64) .NET assembly",portableExecutableFile.DescribeBitness());
		}

		[TestMethod]
		public void Synthetic_NoTargetFramework_CoreLibraryFromTheReferences() {
			string path=TestSupport.WritePlugin(_folder,"N64aPeNoTfm",SyntheticPlatform.AnyCpu,null);
			PortableExecutableFile portableExecutableFile=PortableExecutableFile.Read(path);
			Assert.AreEqual("",portableExecutableFile.TargetFramework);
			Assert.AreEqual("mscorlib",portableExecutableFile.CoreLibraryName);
			path=TestSupport.WritePlugin(_folder,"N64aPeRuntime10",SyntheticPlatform.AnyCpu,null,new Version(10,0,0,0));
			portableExecutableFile=PortableExecutableFile.Read(path);
			Assert.AreEqual("",portableExecutableFile.TargetFramework);
			Assert.AreEqual("System.Runtime",portableExecutableFile.CoreLibraryName,"System.Runtime is named before mscorlib.");
			Assert.AreEqual(new Version(10,0,0,0),portableExecutableFile.CoreLibraryVersion);
		}

		[TestMethod]
		public void NotPe_AndTruncated_AreReadWithoutThrowing() {
			Assert.IsFalse(PortableExecutableFile.Read(new byte[0]).IsPe);
			Assert.IsFalse(PortableExecutableFile.Read(Encoding.ASCII.GetBytes("N64A not an assembly, long enough to have a DOS header field at 0x3C"
				+" and more text after it.")).IsPe);
			byte[] bytesFull=File.ReadAllBytes(TestSupport.WritePlugin(_folder,"N64aPeTruncated",SyntheticPlatform.AnyCpu,".NETFramework,Version=v4.8"));
			foreach(int length in new[] { 0x40,0x80,0x100,0x180,0x200,0x280,0x300,bytesFull.Length/2,bytesFull.Length-1 }) {
				byte[] bytes=new byte[Math.Min(length,bytesFull.Length)];
				Array.Copy(bytesFull,bytes,bytes.Length);
				PortableExecutableFile.Read(bytes);//never throws
			}
		}

		[TestMethod]
		public void SystemKernel32_IsNativeOfThisProcess() {
			//A 32-bit process reads System32 through WOW64's redirection, so it reads SysWOW64's x86 file: the file of its own bitness either way.
			string path=Path.Combine(Environment.SystemDirectory,"kernel32.dll");
			PortableExecutableFile portableExecutableFile=PortableExecutableFile.Read(path);
			Assert.IsTrue(portableExecutableFile.IsPe);
			Assert.IsFalse(portableExecutableFile.IsManaged);
			Assert.AreEqual(PortableExecutableFile.GetProcessMachine(),portableExecutableFile.Machine);
			Assert.AreEqual(Environment.Is64BitProcess,portableExecutableFile.IsPe32Plus);
			Assert.IsTrue(portableExecutableFile.CanLoadInProcess(PortableExecutableFile.GetProcessMachine()));
			StringAssert.StartsWith(portableExecutableFile.DescribeBitness(),"a native "+(Environment.Is64BitProcess ? "64-bit (x64) PE32+" : "32-bit (x86) PE32"));
		}

		[TestMethod]
		public void ProcessMachine_IsThisRunsBitness() {
			Assert.AreEqual(Environment.Is64BitProcess ? PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64 : PortableExecutableFile.IMAGE_FILE_MACHINE_I386,
				PortableExecutableFile.GetProcessMachine());
		}

		///<summary>Every DLL and EXE of this test's output folder (the program's libraries, the packages', the test framework's) read by the reader
		///and by Mono.Cecil: the same machine, PE32+, CLI flags, metadata version, TargetFrameworkAttribute and core library. A file Cecil cannot read
		///as an assembly must be a native (or non-PE) file to the reader.</summary>
		[TestMethod]
		public void OutputFolder_ReaderAgreesWithCecil() {
			string folder=AppDomain.CurrentDomain.BaseDirectory;
			List<string> listPaths=Directory.GetFiles(folder,"*.dll").Concat(Directory.GetFiles(folder,"*.exe")).ToList();
			int countManaged=0;
			int countWithFramework=0;
			List<string> listProblems=new List<string>();
			foreach(string path in listPaths) {
				PortableExecutableFile portableExecutableFile=PortableExecutableFile.Read(path);
				string name=Path.GetFileName(path);
				ModuleDefinition moduleDefinition;
				try {
					moduleDefinition=ModuleDefinition.ReadModule(path);
				}
				catch(BadImageFormatException) {
					if(portableExecutableFile.IsManaged) {
						listProblems.Add(name+": Cecil reads no assembly, the reader a managed one");
					}
					continue;
				}
				using(moduleDefinition) {
					countManaged++;
					if(!portableExecutableFile.IsManaged) {
						listProblems.Add(name+": not managed to the reader");
						continue;
					}
					ushort machineCecil=moduleDefinition.Architecture==TargetArchitecture.AMD64 ? PortableExecutableFile.IMAGE_FILE_MACHINE_AMD64
						: (moduleDefinition.Architecture==TargetArchitecture.ARM64 ? PortableExecutableFile.IMAGE_FILE_MACHINE_ARM64
						: PortableExecutableFile.IMAGE_FILE_MACHINE_I386);
					if(machineCecil!=portableExecutableFile.Machine) {
						listProblems.Add(name+": machine "+portableExecutableFile.Machine.ToString("X4")+", Cecil "+moduleDefinition.Architecture);
					}
					uint flagsCecil=(uint)moduleDefinition.Attributes & (PortableExecutableFile.COMIMAGE_FLAGS_ILONLY|PortableExecutableFile.COMIMAGE_FLAGS_32BITREQUIRED
						|PortableExecutableFile.COMIMAGE_FLAGS_32BITPREFERRED);
					uint flagsReader=portableExecutableFile.CorFlags & (PortableExecutableFile.COMIMAGE_FLAGS_ILONLY|PortableExecutableFile.COMIMAGE_FLAGS_32BITREQUIRED
						|PortableExecutableFile.COMIMAGE_FLAGS_32BITPREFERRED);
					if(flagsCecil!=flagsReader) {
						listProblems.Add(name+": CLI flags "+flagsReader.ToString("X")+", Cecil "+flagsCecil.ToString("X"));
					}
					if(moduleDefinition.RuntimeVersion!=portableExecutableFile.MetadataVersion) {
						listProblems.Add(name+": metadata version '"+portableExecutableFile.MetadataVersion+"', Cecil '"+moduleDefinition.RuntimeVersion+"'");
					}
					string targetFrameworkCecil="";
					if(moduleDefinition.Assembly!=null) {
						CustomAttribute customAttribute=moduleDefinition.Assembly.CustomAttributes
							.FirstOrDefault(x => x.AttributeType.FullName=="System.Runtime.Versioning.TargetFrameworkAttribute");
						if(customAttribute!=null && customAttribute.ConstructorArguments.Count>0) {
							targetFrameworkCecil=(customAttribute.ConstructorArguments[0].Value as string)??"";
						}
					}
					if(targetFrameworkCecil!=portableExecutableFile.TargetFramework) {
						listProblems.Add(name+": target framework '"+portableExecutableFile.TargetFramework+"', Cecil '"+targetFrameworkCecil+"'");
					}
					if(targetFrameworkCecil!="") {
						countWithFramework++;
					}
					string[] arrayCoreNames=new[] { "System.Private.CoreLib","System.Runtime","netstandard","mscorlib" };
					AssemblyNameReference assemblyNameReferenceCore=moduleDefinition.AssemblyReferences
						.Where(x => arrayCoreNames.Contains(x.Name)).OrderBy(x => Array.IndexOf(arrayCoreNames,x.Name)).FirstOrDefault();
					string coreCecil=assemblyNameReferenceCore?.Name??"";
					if(coreCecil!=portableExecutableFile.CoreLibraryName
						|| (assemblyNameReferenceCore!=null && assemblyNameReferenceCore.Version!=portableExecutableFile.CoreLibraryVersion))
					{
						listProblems.Add(name+": core library '"+portableExecutableFile.CoreLibraryName+" "+portableExecutableFile.CoreLibraryVersion+"', Cecil '"
							+coreCecil+" "+assemblyNameReferenceCore?.Version+"'");
					}
				}
			}
			Assert.IsTrue(countManaged>=20,"Expected the assemblies of the output folder, found "+countManaged+" of "+listPaths.Count+" files.");
			Assert.IsTrue(countWithFramework>=10,"Expected assemblies with a TargetFrameworkAttribute, found "+countWithFramework+".");
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
		}
	}
}
