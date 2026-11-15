// Tests of NuGetFiles (S2-4 of the .NET 10 move, docs/specs/net10-plan.md): the files a project's restore copies from its packages, each of the
// package at the version the project's lock file names, found in a package folder; a restore that disagrees with the lock file, and two packages
// that bring a file of one name, throw instead of being read as fewer files. Synthetic files in a scratch folder under the temp folder.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProjectFileTests {
	[TestClass]
	public class NuGetFilesTests {
		private const string LOCK_FILE="{\r\n  \"version\": 2,\r\n  \"dependencies\": {\r\n    \".NETFramework,Version=v4.8\": {\r\n"
			+"      \"Fixture.Package\": { \"type\": \"Direct\", \"requested\": \"[1.2.3, )\", \"resolved\": \"1.2.3\", \"contentHash\": \"aGFzaA==\" },\r\n"
			+"      \"System.ValueTuple\": { \"type\": \"Transitive\", \"resolved\": \"4.6.2\", \"contentHash\": \"dnQ=\" },\r\n"
			+"      \"CodeBase\": { \"type\": \"Project\" } },\r\n"
			+"    \".NETFramework,Version=v4.8/win-x86\": { \"Fixture.Package\": { \"type\": \"Direct\", \"requested\": \"[1.2.3, )\", \"resolved\": \"1.2.3\", \"contentHash\": \"aGFzaA==\" } } } }";

		///<summary>project.assets.json in the form the restore writes it, with the package folder given (JSON-escaped).</summary>
		private static string AssetsFile(string folderPackages,string versionFixture,string nativeName) {
			return "{\r\n  \"version\": 3,\r\n  \"targets\": {\r\n"
				+"    \".NETFramework,Version=v4.8\": {\r\n"
				+"      \"Fixture.Package/"+versionFixture+"\": { \"type\": \"package\", \"compile\": { \"ref/net462/Fixture.Package.dll\": {} },\r\n"
				+"        \"runtime\": { \"lib/net462/Fixture.Package.dll\": {} } },\r\n"
				+"      \"System.ValueTuple/4.6.2\": { \"type\": \"package\", \"compile\": { \"lib/net471/_._\": {} }, \"runtime\": { \"lib/net471/_._\": {} } },\r\n"
				+"      \"CodeBase/1.0.0\": { \"type\": \"project\", \"runtime\": { \"bin/placeholder/CodeBase.dll\": {} } } },\r\n"
				+"    \".NETFramework,Version=v4.8/win-x86\": {\r\n"
				+"      \"Fixture.Package/"+versionFixture+"\": { \"type\": \"package\", \"runtime\": { \"lib/net462/Fixture.Package.dll\": {} },\r\n"
				+"        \"native\": { \"runtimes/win-x86/native/"+nativeName+"\": {} } } } },\r\n"
				+"  \"libraries\": { },\r\n"
				+"  \"packageFolders\": { \""+folderPackages.Replace("\\","\\\\")+"\\\\\": {} } }";
		}

		[TestMethod]
		public void ReadCopiedFiles_TheLockedVersionsAssets_InThePackageFolder() {
			string folder=Path.Combine(Path.GetTempPath(),"ProjectFileTests_NuGet_"+Guid.NewGuid().ToString("N"));
			string folderProject=Path.Combine(folder,"Project");
			string folderPackages=Path.Combine(folder,"packages");
			Directory.CreateDirectory(Path.Combine(folderProject,"obj"));
			try {
				//No lock file: no package; a lock file without a restore: not known yet.
				Assert.AreEqual(0,NuGetFiles.ReadCopiedFiles(folder,folderProject).Count);
				File.WriteAllText(Path.Combine(folderProject,"packages.lock.json"),LOCK_FILE);
				Assert.IsNull(NuGetFiles.ReadCopiedFiles(folder,folderProject));
				CollectionAssert.AreEqual(new[] { "Fixture.Package/1.2.3/aGFzaA==","System.ValueTuple/4.6.2/dnQ=" },
					NuGetFiles.ReadLockFile(Path.Combine(folderProject,"packages.lock.json")).Select(x => x.Id+"/"+x.Version+"/"+x.ContentHash).ToArray(),
					"Each package once, with its content hash; a project is not a package.");
				string pathAssets=Path.Combine(folderProject,"obj","project.assets.json");
				File.WriteAllText(pathAssets,AssetsFile(folderPackages,"1.2.3","Fixture.Native.dll"));
				CollectionAssert.AreEqual(new[] { folderPackages+"\\" },NuGetFiles.ReadPackageFolders(pathAssets).ToArray());
				//The package is in no package folder yet: its files are known, their path is not.
				Dictionary<string,PackageFile> dictFiles=NuGetFiles.ReadCopiedFiles(folder,folderProject);
				CollectionAssert.AreEquivalent(new[] { "Fixture.Package.dll","Fixture.Native.dll" },dictFiles.Keys.ToArray(),"Runtime and native assets; no _._.");
				Assert.IsNull(dictFiles["fixture.package.dll"].Path,"File names ignore case; no folder holds the package.");
				//In the restore's package folder (the fixture id is in no offline folder).
				string folderFixture=Path.Combine(folderPackages,"fixture.package","1.2.3");
				Directory.CreateDirectory(folderFixture);
				dictFiles=NuGetFiles.ReadCopiedFiles(folder,folderProject);
				PackageFile packageFile=dictFiles["Fixture.Package.dll"];
				Assert.AreEqual("Fixture.Package",packageFile.Id);
				Assert.AreEqual("1.2.3",packageFile.Version);
				Assert.AreEqual("lib/net462/Fixture.Package.dll",packageFile.Asset);
				Assert.AreEqual(Path.Combine(folderFixture,"lib","net462","Fixture.Package.dll"),packageFile.Path);
				Assert.AreEqual(Path.Combine(folderFixture,"runtimes","win-x86","native","Fixture.Native.dll"),dictFiles["Fixture.Native.dll"].Path);
				//A restore of another version than the lock file's is not read as the locked package.
				File.WriteAllText(pathAssets,AssetsFile(folderPackages,"1.2.4","Fixture.Native.dll"));
				AssertThrows(() => NuGetFiles.ReadCopiedFiles(folder,folderProject),"has the package Fixture.Package/1.2.4");
				//Two files of one name from different assets: which one a folder gets is not decided by a test.
				File.WriteAllText(pathAssets,AssetsFile(folderPackages,"1.2.3","Fixture.Package.dll"));
				AssertThrows(() => NuGetFiles.ReadCopiedFiles(folder,folderProject),"two package files are named Fixture.Package.dll");
			}
			finally {
				Directory.Delete(folder,true);
			}
		}

		///<summary>An exe's restore has a win-x86 target, and the build copies from it alone: a package whose win-x86 file differs from its net48 one
		///(System.Net.Http 4.3.4: lib\net46 and runtimes\win\lib\net46, TC-4) is read as the win-x86 file, not as two files of one name. A managed
		///file of a name the .NET Framework 4.8 has at a higher assembly version is not copied (the conflict resolution keeps the framework's);
		///one at a higher version than the framework's is. The package files are synthetic assemblies written here.</summary>
		[TestMethod]
		public void ReadCopiedFiles_TheRuntimeTarget_WithoutWhatTheFrameworkReplaces() {
			if(NuGetFiles.GetFrameworkReferenceFolder()==null || NuGetFiles.GetFrameworkAssemblyVersion("System.Net.Http.dll")==null) {
				Assert.Inconclusive("The .NET Framework 4.8 reference assemblies are not installed; the build needs them (Visual Studio Build Tools).");
			}
			Version versionFramework=NuGetFiles.GetFrameworkAssemblyVersion("System.Net.Http.dll");
			string folder=Path.Combine(Path.GetTempPath(),"ProjectFileTests_NuGet_"+Guid.NewGuid().ToString("N"));
			string folderProject=Path.Combine(folder,"Project");
			string folderPackages=Path.Combine(folder,"packages");
			Directory.CreateDirectory(Path.Combine(folderProject,"obj"));
			try {
				File.WriteAllText(Path.Combine(folderProject,"packages.lock.json"),"{ \"version\": 2, \"dependencies\": { \".NETFramework,Version=v4.8\": {\r\n"
					+"  \"Fixture.Http\": { \"type\": \"Transitive\", \"resolved\": \"4.3.4\", \"contentHash\": \"aA==\" } },\r\n"
					+"  \".NETFramework,Version=v4.8/win-x86\": { \"Fixture.Http\": { \"type\": \"Transitive\", \"resolved\": \"4.3.4\", \"contentHash\": \"aA==\" } } } }");
				string pathAssets=Path.Combine(folderProject,"obj","project.assets.json");
				File.WriteAllText(pathAssets,"{\r\n  \"version\": 3,\r\n  \"targets\": {\r\n"
					+"    \".NETFramework,Version=v4.8\": { \"Fixture.Http/4.3.4\": { \"type\": \"package\", \"runtime\": { \"lib/net46/System.Net.Http.dll\": {} } } },\r\n"
					+"    \".NETFramework,Version=v4.8/win-x86\": { \"Fixture.Http/4.3.4\": { \"type\": \"package\",\r\n"
					+"      \"runtime\": { \"runtimes/win/lib/net46/System.Net.Http.dll\": {} } } } },\r\n"
					+"  \"libraries\": { },\r\n"
					+"  \"packageFolders\": { \""+folderPackages.Replace("\\","\\\\")+"\\\\\": {} } }");
				CollectionAssert.AreEqual(new[] { "runtimes/win/lib/net46/System.Net.Http.dll" },NuGetFiles.ReadCopiedAssets(pathAssets)["Fixture.Http/4.3.4"].ToArray(),
					"The win-x86 target's file only.");
				//Not in a package folder: its version is unknown, so it is listed (and the checks that compare it fail on its missing file).
				Assert.IsTrue(NuGetFiles.ReadCopiedFiles(folder,folderProject).ContainsKey("System.Net.Http.dll"));
				string folderAsset=Path.Combine(folderPackages,"fixture.http","4.3.4","runtimes","win","lib","net46");
				//Below the framework's version (System.Net.Http 4.3.4's own file is 4.1.1.3, the framework's 4.2.0.0): the build keeps the framework's.
				WriteAssembly(folderAsset,"System.Net.Http",new Version(versionFramework.Major-1,0,0,0));
				Dictionary<string,PackageFile> dictFiles=NuGetFiles.ReadCopiedFiles(folder,folderProject);
				Assert.AreEqual(0,dictFiles.Count,string.Join(", ",dictFiles.Values));
				//Above it: the package's file is copied.
				WriteAssembly(folderAsset,"System.Net.Http",new Version(versionFramework.Major+5,0,0,0));
				dictFiles=NuGetFiles.ReadCopiedFiles(folder,folderProject);
				Assert.AreEqual("runtimes/win/lib/net46/System.Net.Http.dll",dictFiles["System.Net.Http.dll"].Asset);
				//The same version: which one wins is not modelled.
				WriteAssembly(folderAsset,"System.Net.Http",versionFramework);
				AssertThrows(() => NuGetFiles.ReadCopiedFiles(folder,folderProject),"is not modelled by NuGetFiles");
			}
			finally {
				Directory.Delete(folder,true);
			}
		}

		///<summary>Writes a synthetic assembly of the name and version, with no types, as folder\name.dll.</summary>
		private static void WriteAssembly(string folder,string name,Version version) {
			Directory.CreateDirectory(folder);
			string folderTemp=Path.Combine(folder,"build-"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(folderTemp);
			System.Reflection.AssemblyName assemblyName=new System.Reflection.AssemblyName(name);
			assemblyName.Version=version;
			System.Reflection.Emit.AssemblyBuilder assemblyBuilder=AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName,
				System.Reflection.Emit.AssemblyBuilderAccess.Save,folderTemp);
			assemblyBuilder.DefineDynamicModule(name,name+".dll");
			assemblyBuilder.Save(name+".dll");
			File.Copy(Path.Combine(folderTemp,name+".dll"),Path.Combine(folder,name+".dll"),true);
			Directory.Delete(folderTemp,true);
		}

		///<summary>The lock files the Helix tests read (TC-4) are those of the solution filter's projects: never one of another worktree under
		///.claude\worktrees (which, cleaned, has no analyzer record and no build output), nor one in build output or a package folder, nor one of a
		///project the filter does not name.</summary>
		[TestMethod]
		public void ListSolutionLockFiles_TheFiltersProjectsOnly_NoOtherWorktree() {
			string folder=Path.Combine(Path.GetTempPath(),"ProjectFileTests_Slnf_"+Guid.NewGuid().ToString("N"));
			string lockMvvm=LOCK_FILE.Replace("Fixture.Package","CommunityToolkit.Mvvm");
			try {
				Directory.CreateDirectory(folder);
				AssertThrows(() => NuGetFiles.ListSolutionLockFiles(folder),"OpenDental.Local.slnf is not there");
				File.WriteAllText(Path.Combine(folder,"OpenDental.Local.slnf"),"{\r\n  \"solution\": {\r\n    \"path\": \"OpenDental.sln\",\r\n    \"projects\": [\r\n"
					+"      \"P\\\\xP.csproj\",\r\n      \"NoLock\\\\NoLock.csproj\",\r\n      \"tests\\\\T\\\\T.csproj\",\r\n      \"Direct2dWrapper\\\\Direct2dWrapper.vcxproj\"\r\n"
					+"    ]\r\n  }\r\n}");
				foreach(string relative in new[] { "P",@"tests\T",@".claude\worktrees\x\P",@"P\bin\Debug",@"P\obj",@"packages\p\1.0.0",@"Unlisted" }) {
					Directory.CreateDirectory(Path.Combine(folder,relative));
					File.WriteAllText(Path.Combine(folder,relative,"packages.lock.json"),lockMvvm);
				}
				Directory.CreateDirectory(Path.Combine(folder,"NoLock"));
				Directory.CreateDirectory(Path.Combine(folder,"Direct2dWrapper"));
				File.WriteAllText(Path.Combine(folder,"Direct2dWrapper","packages.lock.json"),lockMvvm);
				List<string> listPaths=NuGetFiles.ListSolutionLockFiles(folder);
				CollectionAssert.AreEqual(new[] { Path.Combine(folder,"P","packages.lock.json"),Path.Combine(folder,"tests","T","packages.lock.json") },listPaths.ToArray(),
					"The C# projects of the filter that have a lock file, in its order.");
				Assert.IsFalse(listPaths.Any(x => x.IndexOf(@"\.claude\",StringComparison.OrdinalIgnoreCase)>=0),"No other worktree's lock file.");
				Assert.IsTrue(NuGetFiles.ReadLockFile(Path.Combine(folder,@".claude\worktrees\x\P","packages.lock.json")).Any(x => x.Id=="CommunityToolkit.Mvvm"),
					"The worktree's lock file names the package, and has no obj records: listed, it would be read as not built.");
			}
			finally {
				Directory.Delete(folder,true);
			}
		}

		[TestMethod]
		public void GetPackageFolder_IsTheLowerCaseIdAndVersion_AsNuGetLaysItOut() {
			Assert.AreEqual(@"C:\packages\microsoft.web.webview2\1.0.2420.47",NuGetFiles.GetPackageFolder(@"C:\packages","Microsoft.Web.WebView2","1.0.2420.47"));
		}

		private static void AssertThrows(Action action,string expectedText) {
			try {
				action();
			}
			catch(InvalidOperationException ex) {
				StringAssert.Contains(ex.Message,expectedText);
				return;
			}
			Assert.Fail("No InvalidOperationException for "+expectedText);
		}
	}
}
