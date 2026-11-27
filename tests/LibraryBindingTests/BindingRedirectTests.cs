// How the programs and tests bind the library files on .NET 10 (S3b-5 of the .NET 10 move): every reference between the library files (those of
// "Required dlls" and the NuGet packages' own files at the lock files' versions) is satisfied by the version there, the built programs and tests
// hold those very files, and each one's .deps.json names them at the lock files' versions, which is what the runtime binds by.
// Until S3b-5 these were the .NET Framework's checks: binding redirects in every app.config, Web.config and generated .dll.config, the shared
// test config tests\BindingRedirects.config, and redirect targets in the global assembly cache. Binding redirects mean nothing on .NET (the
// runtime takes the file its .deps.json names for any lower version) and .NET has no global assembly cache, so those checks retired with the
// .NET Framework build; docs/port-notes/S3b-5.md names each one and the check that replaced it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectFileTests;

namespace LibraryBindingTests {
	[TestClass]
	public class BindingRedirectTests {
		///<summary>The .NET 10 rule in place of the redirects (Libraries_EveryReferenceBetweenThemResolves_InEveryConfigThatUsesThem until S3b-5):
		///in the closure of every program and test that uses the library files (its own, those of the projects it references and the files of every
		///package of its lock file: SolutionProjects.LibrariesInClosure), each reference between them asks for the version there or a lower one.
		///</summary>
		[TestMethod]
		public void Libraries_EveryReferenceBetweenThem_IsSatisfiedByTheVersionThere_InEveryProgramAndTest() {
			List<ProjectInfo> listProjects=SolutionProjects.All.Where(x => (x.IsExe || x.IsTest) && SolutionProjects.LoadsLibraryAssemblies(x)).ToList();
			foreach(string program in new[] { "OpenPmsDental.csproj","ServiceManager.csproj","OpenPmsDentalHL7.csproj","xCentralManager.csproj" }) {
				Assert.IsTrue(listProjects.Any(x => Path.GetFileName(x.Path).Equals(program,StringComparison.OrdinalIgnoreCase)),program+" is not checked.");
			}
			List<string> listProblems=new List<string>();
			foreach(ProjectInfo projectInfo in listProjects) {
				HashSet<string> hashSetLibraries=SolutionProjects.LibrariesInClosure(projectInfo);
				Dictionary<string,AssemblyFacts> dictAssemblies=Libraries.Assemblies.Where(x => hashSetLibraries.Contains(x.Value.FileName))
					.ToDictionary(x => x.Key,x => x.Value,StringComparer.OrdinalIgnoreCase);
				listProblems.AddRange(BindingRules.FindReferencesAboveTheVersionThere(dictAssemblies,RepoPaths.Relative(projectInfo.Path)));
			}
			Assert.AreEqual(0,listProblems.Count,"References that do not load:\r\n"+string.Join("\r\n",listProblems.Distinct()));
		}

		///<summary>No config of the repository holds binding redirects any more, and nothing generates them (Configs_RedirectOnlyToVersionsThatAreThere,
		///Programs_ThatUseTheLibraries_HaveAConfig and TestProjects_GenerateTheirRedirects_FromTheSharedTestConfig until S3b-5): they mean nothing on
		///.NET, and a config that still had them would only look as if it bound something. The shared test config and its rule are gone, no project
		///turns AutoGenerateBindingRedirects on, and the app.config files keep only what .NET reads (application settings and the like).</summary>
		[TestMethod]
		public void Configs_HoldNoBindingRedirects_AndNothingGeneratesThem() {
			Assert.IsFalse(File.Exists(RepoPaths.Combine("tests","BindingRedirects.config")),"tests\\BindingRedirects.config went with S3b-5.");
			StringAssert.DoesNotMatch(RepoPaths.ReadText("Directory.Build.targets"),new System.Text.RegularExpressions.Regex("<AppConfig>"));
			List<string> listProblems=new List<string>();
			foreach(ProjectInfo projectInfo in SolutionProjects.All) {
				if(projectInfo.AutoGeneratesRedirects) {
					listProblems.Add(RepoPaths.Relative(projectInfo.Path)+" generates binding redirects.");
				}
				if(projectInfo.ConfigPath!=null && ConfigFiles.ReadRedirects(projectInfo.ConfigPath).Count>0) {
					listProblems.Add(RepoPaths.Relative(projectInfo.ConfigPath)+" holds binding redirects.");
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
			//The checks above read real configs: the program's is among them, and UpdateFileCopier uses only the framework.
			Assert.IsNotNull(SolutionProjects.ByName("OpenPmsDental.csproj").ConfigPath,"OpenPmsDental\\app.config (its application settings).");
			Assert.AreEqual(0,SolutionProjects.LibrariesInClosure(SolutionProjects.ByName("UpdateFileCopier.csproj")).Count);
		}

		[TestMethod]
		public void BuiltProgramsAndTests_HoldTheLibraryFiles_AndTheirDepsJsonNamesThem() {
			List<ProjectInfo> listProjects=SolutionProjects.All.Where(x => (x.IsExe || x.IsTest) && SolutionProjects.LoadsLibraryAssemblies(x)).ToList();
			Assert.IsTrue(listProjects.Any(x => Path.GetFileName(x.Path).Equals("OpenPmsDental.csproj",StringComparison.OrdinalIgnoreCase)),"The program is checked.");
			CheckBuiltProgramsAndTests(listProjects,BuildOutput.IsRequired);
		}

		[TestMethod]
		public void BuiltProgramsAndTests_AProjectThatIsNotBuilt_IsNotSkipped() {
			//A project with no output folder: inconclusive on a developer's run, a failure when the build output is required, never a pass.
			ProjectInfo projectInfo=new ProjectInfo();
			projectInfo.Path=Path.Combine(Path.GetTempPath(),"LibraryBindingTests_"+Guid.NewGuid().ToString("N"),"NotBuiltFixture.csproj");
			projectInfo.Folder=Path.GetDirectoryName(projectInfo.Path);
			projectInfo.AssemblyName="NotBuiltFixture";
			projectInfo.IsExe=true;
			foreach(bool isRequired in new[] { true,false }) {
				//The check's own exception is caught; the verdict on it comes after the try, so the test fails on a pass by itself, whatever the
				//message text says.
				UnitTestAssertException exceptionCaught=null;
				try {
					CheckBuiltProgramsAndTests(new List<ProjectInfo> { projectInfo },isRequired);
				}
				catch(UnitTestAssertException ex) {
					exceptionCaught=ex;
				}
				Assert.IsNotNull(exceptionCaught,"A project that is not built passed (required: "+isRequired+").");
				Assert.AreEqual(isRequired ? typeof(AssertFailedException) : typeof(AssertInconclusiveException),exceptionCaught.GetType(),"Required: "+isRequired);
				StringAssert.Contains(exceptionCaught.Message,"NotBuiltFixture.dll");
			}
		}

		///<summary>For each project: the library files in its output folder are the library files themselves (the file of "Required dlls", or the
		///package's own file at the lock files' version); its .deps.json, by which the runtime binds them, names each package file there under its
		///package at the lock files' version; and every reference between the assemblies of the folder is satisfied by the version there (the .NET
		///rule). Until S3b-5 the folder's configs had to bind them by redirects instead. A project with no output is not skipped: once the built
		///ones are checked, it ends the check through BuildOutput.NotBuilt (inconclusive on a developer's run, a failure under
		///OPMS_REQUIRE_BUILD_OUTPUT=1), so a gate never passes it on fewer folders.</summary>
		private static void CheckBuiltProgramsAndTests(List<ProjectInfo> listProjects,bool isRequired) {
			List<string> listProblems=new List<string>();
			List<string> listNotBuilt=new List<string>();
			int countFolders=0;
			foreach(ProjectInfo projectInfo in listProjects) {
				string folder=projectInfo.OutputFolder;
				if(folder==null) {
					listNotBuilt.Add(RepoPaths.Relative(projectInfo.Path)+" ("+projectInfo.PrimaryOutputName+")");
					continue;
				}
				countFolders++;
				string labelFolder=RepoPaths.Relative(folder);
				//The very library files: a same-named file from elsewhere (a stale build, another version of the package) is not what the release ships.
				//A self-contained exe's folder also holds its runtime (decision 20): where the runtime has a file of a library's name (System.CodeDom,
				//System.Diagnostics.EventLog and the like, part of the Windows Desktop runtime), the build keeps the runtime's, and its .deps.json names
				//it under the runtime pack; those are the runtime's files, not the library's (S3b-5).
				bool isSelfContained=projectInfo.IsExe && !projectInfo.IsTest && File.Exists(Path.Combine(folder,"hostfxr.dll"));
				foreach(string path in Directory.GetFiles(folder)) {
					if(isSelfContained && SharedFramework.Has(Path.GetFileNameWithoutExtension(path))) {
						continue;
					}
					if(Libraries.Files.TryGetValue(Path.GetFileName(path),out string pathLibrary)
						&& MetadataReader.Sha256(path)!=MetadataReader.Sha256(pathLibrary))
					{
						string packageId=Libraries.PackageOf(Path.GetFileName(path));
						listProblems.Add(labelFolder+"\\"+Path.GetFileName(path)+" is not "+(packageId==null ? "the file in Required dlls"
							: "the file of the package "+packageId+" "+Libraries.LockedVersions[packageId])+".");
					}
				}
				string pathDeps=Path.Combine(folder,projectInfo.AssemblyName+".deps.json");
				if(!File.Exists(pathDeps)) {
					listProblems.Add(RepoPaths.Relative(pathDeps)+" is missing; the runtime binds the library files by it.");
				}
				else {
					Dictionary<string,string> dictFilePackages=ReadDepsJsonRuntimeFiles(pathDeps);
					foreach(string path in Directory.GetFiles(folder,"*.dll")) {
						string fileName=Path.GetFileName(path);
						string packageId=Libraries.PackageOf(fileName);
						if(packageId==null || (isSelfContained && SharedFramework.Has(Path.GetFileNameWithoutExtension(path)))) {
							continue;
						}
						string expected=packageId+"/"+Libraries.LockedVersions[packageId];
						if(!dictFilePackages.TryGetValue(fileName,out string named) || !named.Equals(expected,StringComparison.OrdinalIgnoreCase)) {
							listProblems.Add(RepoPaths.Relative(pathDeps)+" names "+fileName+" under "+(named??"nothing")+", not "+expected+".");
						}
					}
				}
				listProblems.AddRange(BindingRules.FindReferencesAboveTheVersionThere(MetadataReader.ReadFolder(folder),labelFolder+" (built)"));
			}
			Assert.AreEqual(0,listProblems.Count,listProblems.Count+" problem(s); a rebuild of the project may be needed (delete a wrong library file first: MSBuild does not copy "
				+"over a file of the same size and time, such as another target framework's build of the same package version):\r\n"
				+string.Join("\r\n",listProblems.Distinct())
				+(listNotBuilt.Count==0 ? "" : "\r\nNot built, so not checked: "+string.Join(", ",listNotBuilt)));
			if(listNotBuilt.Count>0) {
				BuildOutput.NotBuilt((countFolders==0 ? "Nothing is built" : listNotBuilt.Count+" of "+(countFolders+listNotBuilt.Count)+" programs and tests "
					+"that use the libraries are not built")+": "+string.Join(", ",listNotBuilt.Take(20))+(listNotBuilt.Count>20 ? ", ..." : "")
					+". Build OpenDental.Local.slnf (Debug|x64) first.",isRequired);
			}
		}

		///<summary>File name of each runtime asset of a package that the .deps.json's target names, to "id/version" of its package.</summary>
		private static Dictionary<string,string> ReadDepsJsonRuntimeFiles(string pathDeps) {
			Dictionary<string,string> dictFiles=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
			using(JsonDocument jsonDocument=JsonDocument.Parse(File.ReadAllText(pathDeps))) {
				JsonElement jsonElementRoot=jsonDocument.RootElement;
				JsonElement jsonElementLibraries=jsonElementRoot.GetProperty("libraries");
				foreach(JsonProperty jsonPropertyTarget in jsonElementRoot.GetProperty("targets").EnumerateObject()) {
					foreach(JsonProperty jsonPropertyLibrary in jsonPropertyTarget.Value.EnumerateObject()) {
						if(!jsonElementLibraries.TryGetProperty(jsonPropertyLibrary.Name,out JsonElement jsonElementLibrary)
							|| jsonElementLibrary.GetProperty("type").GetString()!="package")
						{
							continue;
						}
						foreach(string section in new[] { "runtime","native" }) {
							if(!jsonPropertyLibrary.Value.TryGetProperty(section,out JsonElement jsonElementSection)) {
								continue;
							}
							foreach(JsonProperty jsonPropertyAsset in jsonElementSection.EnumerateObject()) {
								dictFiles[Path.GetFileName(jsonPropertyAsset.Name)]=jsonPropertyLibrary.Name;
							}
						}
					}
				}
			}
			return dictFiles;
		}
	}
}
