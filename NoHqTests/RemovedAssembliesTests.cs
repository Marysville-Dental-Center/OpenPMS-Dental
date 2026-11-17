using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using ProjectFileTests;

namespace NoHqTests {
	///<summary>Checks that the proprietary Open Dental assemblies that were removed (the local REST API and the help system) stay out of the build.
	///Reads the metadata of the built assemblies, so it needs the solution to be built first, but no database or network connection.</summary>
	[TestClass]
	public class RemovedAssembliesTests {
		///<summary>ODApi.dll was Open Dental's local REST API. OpenDentalHelp.dll was the online manual, help browser and support-plan key check.
		///The WMPLib interop assemblies only played voicemail in Open Dental HQ's phone tools.</summary>
		private static readonly string[] _arrayRemovedAssemblies=new string[] { "ODApi","OpenDentalHelp","AxInterop.WMPLib","Interop.WMPLib" };

		[TestMethod]
		public void RemovedAssemblies_AreNotInRequiredDlls() {
			string pathRoot=GetRepositoryRoot();
			for(int i=0;i<_arrayRemovedAssemblies.Length;i++) {
				string path=Path.Combine(pathRoot,"Required dlls",_arrayRemovedAssemblies[i]+".dll");
				Assert.IsFalse(File.Exists(path),path+" should have been deleted.");
			}
		}

		[TestMethod]
		public void RemovedAssemblies_AreNotReferenced() {
			string pathRoot=GetRepositoryRoot();
			List<string> listPaths=new List<string> {
				//The program's managed assembly (ProgramAssembly: OpenPmsDental.dll on .NET, where the .exe is the apphost).
				Path.Combine(pathRoot,ProgramAssembly.FindRelative(pathRoot,Path.Combine("OpenPmsDental","bin","Debug"),"OpenPmsDental")),
				Path.Combine(pathRoot,"WpfControlsOD","bin","Debug","WpfControlsOD.dll"),
			};
			for(int i=0;i<listPaths.Count;i++) {
				if(!File.Exists(listPaths[i])) {
					BuildOutput.NotBuilt(listPaths[i]+" is not built. Build OpenDental.Local.slnf (Debug|x86) first.");
				}
				List<string> listReferences=GetReferencedAssemblyNames(listPaths[i]);
				for(int j=0;j<_arrayRemovedAssemblies.Length;j++) {
					Assert.IsFalse(listReferences.Contains(_arrayRemovedAssemblies[j],StringComparer.OrdinalIgnoreCase),
						Path.GetFileName(listPaths[i])+" still references "+_arrayRemovedAssemblies[j]+".");
				}
			}
		}

		///<summary>Reads the assembly references of the file's metadata with Mono.Cecil, from a copy of its bytes, so the file is not locked and
		///nothing is loaded (S3a-8: in place of a reflection-only load in a separate app domain, neither of which .NET has).</summary>
		internal static List<string> GetReferencedAssemblyNames(string path) {
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(path)))
			using(ModuleDefinition moduleDefinition=ModuleDefinition.ReadModule(memoryStream)) {
				return moduleDefinition.AssemblyReferences.Select(x => x.Name).ToList();
			}
		}

		///<summary>The repository root: the first folder above the test assembly (MSTest may run it from a deployment folder below the repository)
		///that holds LICENSE and NOTICE. Found by those files rather than by the solution's name, so it survives a rename. The one locator of this
		///project.</summary>
		internal static string GetRepositoryRoot() {
			List<string> listStarts=new List<string> { Path.GetDirectoryName(typeof(RemovedAssembliesTests).Assembly.Location),Environment.CurrentDirectory };
			for(int i=0;i<listStarts.Count;i++) {
				DirectoryInfo directoryInfo=new DirectoryInfo(listStarts[i]);
				while(directoryInfo!=null) {
					if(File.Exists(Path.Combine(directoryInfo.FullName,"LICENSE")) && File.Exists(Path.Combine(directoryInfo.FullName,"NOTICE"))) {
						return directoryInfo.FullName;
					}
					directoryInfo=directoryInfo.Parent;
				}
			}
			Assert.Inconclusive("Could not find the repository root (the folder with LICENSE and NOTICE).");
			return null;
		}
	}
}
