// WMI from the program folder on .NET 10 (the plan's section 7, "Package assets for a runtime identifier"; S3b-5): System.Management is a NuGet
// package on .NET, whose lib\net10.0 file is a reference build that throws PlatformNotSupportedException for every call; only its
// runtimes\win\lib\net10.0 file works. The program (CodeBase and the business library) calls WMI, so the program folder must hold the runtimes
// file. A fresh process (TestProbeHost run once, from a scratch folder) binds System.Management.dll from the program folder by name, as the
// program's .deps.json binds it, runs SELECT Version FROM Win32_OperatingSystem and returns the version and the assembly's location: a version
// comes back, from the program folder's file, which is byte for byte the package's runtimes\win\lib\net10.0 asset. No database, no window.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectFileTests;
using TestProbeHost;

namespace ReleaseFlowTests {
	[TestClass]
	public class WmiProgramFolderTests {
		[TestMethod]
		public void Wmi_RunsFromTheProgramFolder_WithThePackagesWindowsFile() {
			List<string> listFolders=new[] { "Debug","Release" }.Select(x => Path.Combine(TestPaths.RepoRoot,"OpenPmsDental","bin",x))
				.Where(x => File.Exists(Path.Combine(x,"OpenPmsDental.exe"))).ToList();
			if(listFolders.Count==0) {
				BuildOutput.NotBuilt(@"OpenPmsDental\bin\Debug is not built. Build OpenPmsDental (Debug|x64) first.");
				return;
			}
			Dictionary<string,PackageFile> dictPackageFiles=NuGetFiles.ReadCopiedFiles(TestPaths.RepoRoot,Path.Combine(TestPaths.RepoRoot,"OpenPmsDental"));
			Assert.IsNotNull(dictPackageFiles,"OpenPmsDental is built, so it is restored.");
			Assert.IsTrue(dictPackageFiles.TryGetValue("System.Management.dll",out PackageFile packageFile),"The program's restore copies System.Management.dll.");
			Assert.AreEqual("System.Management",packageFile.Id);
			Assert.AreEqual("runtimes/win/lib/net10.0/System.Management.dll",packageFile.Asset,"The Windows file of the package, not its lib/ reference build.");
			Assert.IsNotNull(packageFile.Path,packageFile+" is in no package folder.");
			string sha256Windows=NuGetFiles.Sha256(packageFile.Path);
			//The lib\ file is the one that throws: another file, so the byte comparison below tells the two apart.
			string pathLib=Path.Combine(packageFile.Path.Substring(0,packageFile.Path.Length-packageFile.Asset.Length),"lib","net10.0","System.Management.dll");
			if(File.Exists(pathLib)) {
				Assert.AreNotEqual(sha256Windows,NuGetFiles.Sha256(pathLib),"The package's lib and runtimes files differ.");
			}
			foreach(string folderProgram in listFolders) {
				string pathProgramFile=Path.Combine(folderProgram,"System.Management.dll");
				Assert.IsTrue(File.Exists(pathProgramFile),pathProgramFile);
				Assert.AreEqual(sha256Windows,NuGetFiles.Sha256(pathProgramFile),pathProgramFile+" is "+packageFile+".");
				string folderProbe=TestPaths.NewTempFolder("wmi");
				try {
					ProbeRunResult probeRunResult=ProbeProcess.RunOnce(folderProbe,null,typeof(WmiProbe),new[] { folderProgram },TimeSpan.FromMinutes(2));
					string output=probeRunResult.Output;
					Assert.IsFalse(probeRunResult.IsTimedOut,output);
					Assert.AreEqual(0,probeRunResult.ExitCode,output);
					string[] arrayLines=output.Split('\n').Select(x => x.Trim()).ToArray();
					string lineVersion=arrayLines.SingleOrDefault(x => x.StartsWith("VERSION|"));
					string lineLocation=arrayLines.SingleOrDefault(x => x.StartsWith("LOCATION|"));
					Assert.IsNotNull(lineVersion,output);
					Assert.IsNotNull(lineLocation,output);
					Assert.IsFalse(string.IsNullOrWhiteSpace(lineVersion.Substring("VERSION|".Length)),"Win32_OperatingSystem has a version: "+output);
					Assert.AreEqual(pathProgramFile,lineLocation.Substring("LOCATION|".Length),true,"System.Management from the program folder: "+output);
				}
				finally {
					try {
						Directory.Delete(folderProbe,true);
					}
					catch(Exception) {
						//Left in the temp folder.
					}
				}
			}
		}
	}

	///<summary>The child process of the WMI test (TestProbeHost run once). args[0] is the program folder. System.Management.dll is not on the
	///host's list (TestProbeHost.deps.json), so the default load context asks Resolving for it, which loads the program folder's file before
	///the method that uses WMI is compiled.</summary>
	public static class WmiProbe {
		public static int Run(string[] args) {
			string folderProgram=args[0];
			AssemblyLoadContext.Default.Resolving+=(assemblyLoadContext,assemblyName) => {
				if(!string.Equals(assemblyName.Name,"System.Management",StringComparison.OrdinalIgnoreCase)) {
					return null;
				}
				return assemblyLoadContext.LoadFromAssemblyPath(Path.Combine(folderProgram,"System.Management.dll"));
			};
			Console.WriteLine("VERSION|"+QueryVersion());
			Assembly assemblyManagement=AppDomain.CurrentDomain.GetAssemblies().Single(x => x.GetName().Name=="System.Management");
			Console.WriteLine("LOCATION|"+assemblyManagement.Location);
			return 0;
		}

		///<summary>Separate and not inlined, so System.Management is bound when this is compiled, after Run has registered the resolver.</summary>
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string QueryVersion() {
			using(ManagementObjectSearcher managementObjectSearcher=new ManagementObjectSearcher("SELECT Version FROM Win32_OperatingSystem"))
			using(ManagementObjectCollection managementObjectCollection=managementObjectSearcher.Get()) {
				foreach(ManagementBaseObject managementBaseObject in managementObjectCollection) {
					return Convert.ToString(managementBaseObject["Version"]);
				}
			}
			return "";
		}
	}
}
