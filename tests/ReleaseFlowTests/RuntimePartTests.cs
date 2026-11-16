// The runtime part of a release (S3b-7 of the .NET 10 move, decision 20; docs/port-notes/S3b-7.md): the runtime files of a self-contained
// build are what its .deps.json files name from the runtime packs, one version per folder, the program's architecture first; the program part
// leaves them out; the runtime part's own manifest is strict; a download folder with the runtime part is checked with it; the copier runs from
// a temp folder on the runtime files its .deps.json names and refuses another version; UpdateFileCopier checks the installation folder's runtime
// files before it writes the release manifest. Also the release's layout on the real Release build (section 7 of docs/specs/net10-plan.md:
// "Config file name", "Package assets for a runtime identifier", "Native files", "Self-contained runtime part") and Build-Release.ps1's
// release-flow refusals. Synthetic files in scratch folders under the temp folder; no database (RuntimePartDbTests has the publish and pull).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using ProjectFileTests;

namespace ReleaseFlowTests {
	///<summary>A synthetic self-contained build in a folder, as the SDK writes one: the programs' apphosts, assemblies and .deps.json files at the
	///top, the helper in Helper32\, and the runtime files their .deps.json files name from the runtime packs, each with text that names its runtime
	///identifier and version (so another version gives other bytes and another key).</summary>
	internal static class SyntheticBuild {
		public static readonly string[] ArrayRuntimeX64=new[] { "coreclr.dll","hostfxr.dll","hostpolicy.dll","System.Private.CoreLib.dll","System.Windows.Forms.dll" };
		public static readonly string[] ArrayRuntimeX86=new[] { "coreclr.dll","hostfxr.dll","hostpolicy.dll","System.Private.CoreLib.dll","System.Windows.Forms.dll" };

		///<summary>Writes the build into the folder (created when missing). versionCopier and versionHelper default to version; programSalt changes
		///the program's own files (a rebuild of the program on the same runtime).</summary>
		public static void Write(string folder,string version,string versionHelper=null,string versionCopier=null,string programSalt="") {
			Directory.CreateDirectory(folder);
			Directory.CreateDirectory(Path.Combine(folder,"Helper32"));
			File.WriteAllText(Path.Combine(folder,"OpenPmsDental.exe"),"synthetic apphost "+programSalt);
			File.WriteAllText(Path.Combine(folder,"OpenPmsDental.dll"),"synthetic program "+programSalt);
			File.WriteAllText(Path.Combine(folder,"OpenPmsDentalBusiness.dll"),"synthetic business layer "+programSalt);
			File.WriteAllText(Path.Combine(folder,"OpenPmsDental.deps.json"),Deps("OpenPmsDental","win-x64",version,ArrayRuntimeX64));
			File.WriteAllText(Path.Combine(folder,"UpdateFileCopier.exe"),"synthetic copier apphost "+programSalt);
			File.WriteAllText(Path.Combine(folder,"UpdateFileCopier.dll"),"synthetic copier "+programSalt);
			File.WriteAllText(Path.Combine(folder,"UpdateFileCopier.runtimeconfig.json"),"{ \"runtimeOptions\": { \"includedFrameworks\": [ { \"name\": "
				+"\"Microsoft.NETCore.App\", \"version\": \""+(versionCopier??version)+"\" } ] } }");
			File.WriteAllText(Path.Combine(folder,"UpdateFileCopier.deps.json"),Deps("UpdateFileCopier","win-x64",versionCopier??version,
				ArrayRuntimeX64.Take(4).ToArray()));
			File.WriteAllText(Path.Combine(folder,"Helper32","OpenPmsDentalHelper32.exe"),"synthetic helper apphost "+programSalt);
			File.WriteAllText(Path.Combine(folder,"Helper32","OpenPmsDentalHelper32.dll"),"synthetic helper "+programSalt);
			File.WriteAllText(Path.Combine(folder,"Helper32","OpenPmsDentalHelper32.deps.json"),Deps("OpenPmsDentalHelper32","win-x86",versionHelper??version,
				ArrayRuntimeX86));
			foreach(string fileName in ArrayRuntimeX64) {
				File.WriteAllText(Path.Combine(folder,fileName),"synthetic runtime "+fileName+" win-x64 "+version);
			}
			foreach(string fileName in ArrayRuntimeX86) {
				File.WriteAllText(Path.Combine(folder,"Helper32",fileName),"synthetic runtime "+fileName+" win-x86 "+(versionHelper??version));
			}
		}

		///<summary>A .deps.json as the SDK writes it for a self-contained program: its project library and the two runtime packs (NETCore with all
		///but the last file, WindowsDesktop with the last), each a library of type runtimepack whose runtime and native assets are the files.
		///</summary>
		public static string Deps(string name,string rid,string version,string[] arrayFiles) {
			string target=".NETCoreApp,Version=v10.0/"+rid;
			string packNetCore="runtimepack.Microsoft.NETCore.App.Runtime."+rid+"/"+version;
			string packDesktop="runtimepack.Microsoft.WindowsDesktop.App.Runtime."+rid+"/"+version;
			Func<IEnumerable<string>,string> funcAssets=listFiles => string.Join(",",listFiles.Select(x => "\"runtimes/"+rid+"/lib/net10.0/"+x+"\": {}"));
			List<string> listNetCore=arrayFiles.Take(Math.Max(1,arrayFiles.Length-1)).ToList();
			List<string> listDesktop=arrayFiles.Skip(listNetCore.Count).ToList();
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append("{ \"runtimeTarget\": { \"name\": \""+target+"\", \"signature\": \"\" }, \"targets\": { \".NETCoreApp,Version=v10.0\": {}, \""
				+target+"\": { \""+name+"/1.0.0\": { \"runtime\": { \""+name+".dll\": {} } }, ");
			stringBuilder.Append("\""+packNetCore+"\": { \"runtime\": { "+funcAssets(listNetCore.Where(x => !x.StartsWith("host") && x!="coreclr.dll"))
				+" }, \"native\": { "+string.Join(",",listNetCore.Where(x => x.StartsWith("host") || x=="coreclr.dll").Select(x => "\""+x+"\": {}"))+" } }");
			if(listDesktop.Count>0) {
				stringBuilder.Append(", \""+packDesktop+"\": { \"runtime\": { "+funcAssets(listDesktop)+" } }");
			}
			stringBuilder.Append(" } }, \"libraries\": { \""+name+"/1.0.0\": { \"type\": \"project\", \"serviceable\": false, \"sha512\": \"\" }, \""+packNetCore
				+"\": { \"type\": \"runtimepack\", \"serviceable\": false, \"sha512\": \"\" }");
			if(listDesktop.Count>0) {
				stringBuilder.Append(", \""+packDesktop+"\": { \"type\": \"runtimepack\", \"serviceable\": false, \"sha512\": \"\" }");
			}
			stringBuilder.Append(" } }");
			return stringBuilder.ToString();
		}

		///<summary>Copies every file of folderFrom (the top and Helper32\) into folderTo, as UpdateFileCopier copies a download into the
		///installation folder, but the two manifests; listSkip names relative paths left out (a copy that did not finish).</summary>
		public static void CopyLikeTheCopier(string folderFrom,string folderTo,params string[] arraySkip) {
			foreach(string path in Directory.GetFiles(folderFrom,"*",SearchOption.AllDirectories)) {
				string relative=path.Substring(folderFrom.TrimEnd('\\').Length+1);
				if(relative.Equals(ReleaseManifest.FILE_NAME,StringComparison.OrdinalIgnoreCase)
					|| relative.Equals(ReleaseRuntimePart.MANIFEST_FILE_NAME,StringComparison.OrdinalIgnoreCase)
					|| arraySkip.Contains(relative,StringComparer.OrdinalIgnoreCase))
				{
					continue;
				}
				Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(folderTo,relative)));
				File.Copy(path,Path.Combine(folderTo,relative),true);
			}
		}
	}

	[TestClass]
	public class RuntimePartTests {
		private static void DeleteQuietly(string folder) {
			try {
				Directory.Delete(folder,true);
			}
			catch(Exception) {
				//Left in the temp folder.
			}
		}

		[TestMethod]
		public void GetRuntimePart_ReadsTheRuntimeFilesFromTheDepsFiles_TheProgramsArchitectureFirst() {
			string folder=TestPaths.NewTempFolder("s3b7_runtime");
			try {
				SyntheticBuild.Write(folder,"10.0.15");
				ReleaseRuntimePart releaseRuntimePart=ForkReleases.GetRuntimePart(folder);
				Assert.AreEqual("10.0.15",releaseRuntimePart.Version);
				CollectionAssert.AreEqual(new[] { "win-x64","win-x86" },releaseRuntimePart.ListArchitectures);
				List<string> listExpected=SyntheticBuild.ArrayRuntimeX64.Concat(SyntheticBuild.ArrayRuntimeX86.Select(x => @"Helper32\"+x)).ToList();
				CollectionAssert.AreEquivalent(listExpected,releaseRuntimePart.ListFiles.Select(x => x.RelativePath).ToList());
				Assert.AreEqual(releaseRuntimePart.ComputeKey(),releaseRuntimePart.Key);
				ReleaseManifestFile releaseManifestFile=releaseRuntimePart.ListFiles.Single(x => x.RelativePath==@"Helper32\coreclr.dll");
				Assert.AreEqual(ReleaseManifest.ComputeSha256File(Path.Combine(folder,"Helper32","coreclr.dll")),releaseManifestFile.Sha256);
				//The program part leaves the runtime files out, and keeps the helper's own files and every other file.
				List<string> listRelease=ForkReleases.GetReleaseFiles(folder);
				Assert.IsFalse(listRelease.Intersect(listExpected,StringComparer.OrdinalIgnoreCase).Any(),string.Join(", ",listRelease));
				foreach(string path in new[] { "OpenPmsDental.exe","OpenPmsDental.dll","OpenPmsDental.deps.json","UpdateFileCopier.exe",@"Helper32\OpenPmsDentalHelper32.exe",
					@"Helper32\OpenPmsDentalHelper32.dll",@"Helper32\OpenPmsDentalHelper32.deps.json" })
				{
					CollectionAssert.Contains(listRelease,path);
				}
				//Another runtime version gives another key; a rebuild of the program on the same runtime keeps it.
				string folderOther=TestPaths.NewTempFolder("s3b7_runtime_other");
				SyntheticBuild.Write(folderOther,"10.0.15",programSalt:"rebuilt");
				Assert.AreEqual(releaseRuntimePart.Key,ForkReleases.GetRuntimePart(folderOther).Key);
				SyntheticBuild.Write(folderOther,"10.0.16");
				Assert.AreNotEqual(releaseRuntimePart.Key,ForkReleases.GetRuntimePart(folderOther).Key);
				DeleteQuietly(folderOther);
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		///<summary>A .NET Framework build (no .deps.json) has no runtime part, and its release lists every file as before.</summary>
		[TestMethod]
		public void NetFrameworkFolder_HasNoRuntimePart() {
			string folder=TestPaths.NewTempFolder("s3b7_net48");
			try {
				File.WriteAllText(Path.Combine(folder,"OpenPmsDental.exe"),"synthetic");
				File.WriteAllText(Path.Combine(folder,"System.Memory.dll"),"synthetic package file");
				Assert.IsNull(ForkReleases.GetRuntimePart(folder));
				CollectionAssert.AreEqual(new[] { "OpenPmsDental.exe","System.Memory.dll" },ForkReleases.GetReleaseFiles(folder));
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		///<summary>A folder never mixes runtime versions (decision 20): two programs that name their shared runtime files at two versions, and a named
		///runtime file that is missing, are refused, each with the setup as the way out.</summary>
		[TestMethod]
		public void GetRuntimePart_RefusesMixedVersions_AndAMissingFile() {
			string folder=TestPaths.NewTempFolder("s3b7_mixed");
			try {
				SyntheticBuild.Write(folder,"10.0.15",versionCopier:"10.0.14");
				InvalidDataException ex=Assert.ThrowsExactly<InvalidDataException>(() => ForkReleases.GetRuntimePart(folder));
				StringAssert.Contains(ex.Message,"is named at");
				SyntheticBuild.Write(folder,"10.0.15",versionHelper:"10.0.14");
				ex=Assert.ThrowsExactly<InvalidDataException>(() => ForkReleases.GetRuntimePart(folder));
				StringAssert.Contains(ex.Message,"more than one version (10.0.14, 10.0.15)");
				StringAssert.Contains(ex.Message,"Run the setup");
				SyntheticBuild.Write(folder,"10.0.15");
				File.Delete(Path.Combine(folder,"Helper32","coreclr.dll"));
				ex=Assert.ThrowsExactly<InvalidDataException>(() => ForkReleases.GetRuntimePart(folder));
				StringAssert.Contains(ex.Message,@"Helper32\coreclr.dll is missing");
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		///<summary>An installation with no release manifest (a fresh setup, or one whose copier found a runtime mismatch and removed it) and a
		///runtime file missing or of two versions has no runtime key: GetInstalledRuntimeKey returns "" instead of throwing, so the next pull
		///downloads the runtime part, which repairs the folder (lead fix at the S3b-7 merge).</summary>
		[TestMethod]
		public void GetInstalledRuntimeKey_BrokenRuntimeWithoutManifest_IsEmpty_SoThePullBringsTheRuntime() {
			string folder=TestPaths.NewTempFolder("s3b7_repair");
			try {
				SyntheticBuild.Write(folder,"10.0.15");
				Assert.AreEqual(ForkReleases.GetRuntimePart(folder).Key,ForkReleases.GetInstalledRuntimeKey(folder),"A whole runtime keys on its files.");
				File.Delete(Path.Combine(folder,"coreclr.dll"));
				Assert.AreEqual("",ForkReleases.GetInstalledRuntimeKey(folder),"A runtime file missing (antivirus quarantine, say).");
				SyntheticBuild.Write(folder,"10.0.15",versionHelper:"10.0.14");
				Assert.AreEqual("",ForkReleases.GetInstalledRuntimeKey(folder),"Two runtime versions.");
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		///<summary>The runtime part's own manifest (the row of type ForkRuntimeManifest and the copy in its archive) round-trips and is strict.</summary>
		[TestMethod]
		public void RuntimePartManifest_RoundTrips_AndIsStrict() {
			string folder=TestPaths.NewTempFolder("s3b7_manifest");
			try {
				SyntheticBuild.Write(folder,"10.0.15");
				ReleaseRuntimePart releaseRuntimePart=ForkReleases.GetRuntimePart(folder);
				byte[] arrayArchive=ForkReleases.BuildRuntimeArchive(folder,releaseRuntimePart);
				Assert.AreEqual(ReleaseManifest.ComputeSha256(arrayArchive),releaseRuntimePart.ArchiveSha256);
				string text=releaseRuntimePart.SerializeManifest();
				StringAssert.Contains(text,"Runtime=Microsoft.NETCore.App 10.0.15 win-x64\n");
				StringAssert.Contains(text,"RuntimeArchitectures=win-x64 win-x86\n");
				ReleaseRuntimePart releaseRuntimePartRead=ReleaseRuntimePart.ParseManifest(text);
				Assert.IsTrue(releaseRuntimePartRead.IsSameAs(releaseRuntimePart));
				Assert.AreEqual(releaseRuntimePart.ArchiveSha256,releaseRuntimePartRead.ArchiveSha256);
				Assert.AreEqual(text,releaseRuntimePartRead.SerializeManifest());
				//The archive's copy has no archive hash; only that copy may lack it.
				string textInside=releaseRuntimePart.SerializeManifest(includeArchiveHash:false);
				Assert.IsFalse(textInside.Contains("RuntimeArchiveSha256="));
				ReleaseRuntimePart.ParseManifest(textInside,requireArchiveHash:false);
				Assert.ThrowsExactly<FormatException>(() => ReleaseRuntimePart.ParseManifest(textInside));
				string lineFile=text.Split('\n').First(x => x.StartsWith("RuntimeFile="));
				Dictionary<string,string> dictBad=new Dictionary<string,string>() {
					{ "a file that does not fit the key",text.Replace(lineFile,lineFile.Replace("|"+releaseRuntimePart.ListFiles[0].Size+"|","|1|")) },
					{ "another product",text.Replace("Product="+ReleaseManifest.PRODUCT_NAME,"Product=Other") },
					{ "another format",text.Replace("Format=1","Format=2") },
					{ "an unknown key",text.Replace("RuntimeKey=","Extra=1\nRuntimeKey=") },
					{ "the .NET Framework",text.Replace("Runtime=Microsoft.NETCore.App 10.0.15 win-x64","Runtime=.NETFramework 4.8 win-x86") },
					{ "another version on the Runtime line",text.Replace("Runtime=Microsoft.NETCore.App 10.0.15","Runtime=Microsoft.NETCore.App 10.0.16") },
					{ "a key twice",text.Replace("RuntimeKey=","RuntimeKey="+releaseRuntimePart.Key+"\nRuntimeKey=") },
					{ "no file",string.Join("\n",text.Split('\n').Where(x => !x.StartsWith("RuntimeFile="))) },
					{ "an unsafe path",text.Replace("|coreclr.dll\n",@"|..\coreclr.dll"+"\n") },
				};
				foreach(KeyValuePair<string,string> pair in dictBad) {
					Assert.ThrowsExactly<FormatException>(() => ReleaseRuntimePart.ParseManifest(pair.Value),pair.Key);
				}
			}
			finally {
				DeleteQuietly(folder);
			}
		}

		///<summary>A download folder with both parts is checked with the runtime part: every runtime file counts as listed, its manifest is allowed,
		///and a runtime file of another version is a problem; a program part download that holds a runtime file is refused.</summary>
		[TestMethod]
		public void VerifyFolder_WithTheRuntimePart_RefusesARuntimeFileOfAnotherVersion() {
			string folderBuild=TestPaths.NewTempFolder("s3b7_verify_build");
			string folderDownload=TestPaths.NewTempFolder("s3b7_verify_download");
			try {
				SyntheticBuild.Write(folderBuild,"10.0.15");
				ReleaseRuntimePart releaseRuntimePart=ForkReleases.GetRuntimePart(folderBuild);
				ForkReleases.BuildRuntimeArchive(folderBuild,releaseRuntimePart);
				ReleaseIdentity releaseIdentity=new ReleaseIdentity { ReleaseVersion="1.0",BuildId=ForkReleases.ComputeBuildId(folderBuild),
					Runtime=ReleaseRuntime.GetRunning() };
				ReleaseManifest releaseManifest=ForkReleases.BuildRelease(folderBuild,releaseIdentity,"SERVER1",DateTime.UtcNow,out byte[] _,releaseRuntimePart);
				Assert.AreEqual("Microsoft.NETCore.App 10.0.15 win-x64",releaseManifest.Runtime.ToString(),"The runtime its files carry.");
				//The manifest is Format 1 with the keys S3a-10 defined, so every reader of the release channel parses it.
				ReleaseManifest releaseManifestRead=ReleaseManifest.Parse(releaseManifest.Serialize());
				Assert.AreEqual(releaseRuntimePart.Key,releaseManifestRead.RuntimePart.Key);
				SyntheticBuild.CopyLikeTheCopier(folderBuild,folderDownload);
				File.WriteAllText(Path.Combine(folderDownload,ReleaseManifest.FILE_NAME),releaseManifest.Serialize());
				File.WriteAllText(Path.Combine(folderDownload,ReleaseRuntimePart.MANIFEST_FILE_NAME),releaseRuntimePart.SerializeManifest());
				Assert.AreEqual(0,releaseManifest.VerifyFolder(folderDownload,includeRuntimePart:true).Count,
					string.Join("\r\n",releaseManifest.VerifyFolder(folderDownload,true)));
				//Without the runtime part, its files are files the manifest does not list.
				List<string> listProblems=releaseManifest.VerifyFolder(folderDownload);
				Assert.IsTrue(listProblems.Contains("Not in the manifest: coreclr.dll"),string.Join("\r\n",listProblems));
				Assert.IsTrue(listProblems.Contains("Not in the manifest: "+ReleaseRuntimePart.MANIFEST_FILE_NAME),string.Join("\r\n",listProblems));
				//A runtime file of another version is refused.
				File.WriteAllText(Path.Combine(folderDownload,"Helper32","hostfxr.dll"),"synthetic runtime hostfxr.dll win-x86 10.0.14");
				listProblems=releaseManifest.VerifyFolder(folderDownload,includeRuntimePart:true);
				CollectionAssert.AreEqual(new[] { @"Wrong SHA-256: Helper32\hostfxr.dll" },listProblems,"The same size, other bytes.");
			}
			finally {
				DeleteQuietly(folderBuild);
				DeleteQuietly(folderDownload);
			}
		}

		///<summary>After a copy, UpdateFileCopier checks every runtime file of the installation folder against the release's runtime part before it
		///writes the release manifest (VerifyFiles): a folder that kept one file of the old runtime has problems, so no manifest is written and the
		///workstation never claims runtime files of two versions. The copier's own code: the check comes before the manifest, a mismatch writes none
		///and says to run the setup; it copies Helper32\, skips the runtime part's manifest, and stops every process of the folder first.</summary>
		[TestMethod]
		public void InstallFolder_WithAnOldRuntimeFile_GetsNoReleaseManifest() {
			string folderOld=TestPaths.NewTempFolder("s3b7_install");
			string folderNew=TestPaths.NewTempFolder("s3b7_new");
			try {
				SyntheticBuild.Write(folderOld,"10.0.14");
				SyntheticBuild.Write(folderNew,"10.0.15",programSalt:"new");
				ReleaseRuntimePart releaseRuntimePartNew=ForkReleases.GetRuntimePart(folderNew);
				SyntheticBuild.CopyLikeTheCopier(folderNew,folderOld,@"Helper32\coreclr.dll");
				CollectionAssert.AreEqual(new[] { @"Wrong SHA-256: runtime file Helper32\coreclr.dll" },releaseRuntimePartNew.VerifyFiles(folderOld));
				SyntheticBuild.CopyLikeTheCopier(folderNew,folderOld);
				Assert.AreEqual(0,releaseRuntimePartNew.VerifyFiles(folderOld).Count,"Every runtime file of the new version.");
				string copier=FileCopierStartTests.StripComments(TestPaths.ReadSource("UpdateFileCopier","FormMain.cs"));
				string move=FileCopierStartTests.GetMethodBody(copier,"private bool MoveFilesToDestDir(");
				int indexCheck=move.IndexOf("_releaseManifest.RuntimePart.VerifyFiles(_destDirectory)",StringComparison.Ordinal);
				int indexManifest=move.IndexOf("File.Copy(sourceManifest,",StringComparison.Ordinal);
				Assert.IsTrue(indexCheck>0 && indexCheck<indexManifest,"The runtime check comes before the release manifest is written.");
				string between=move.Substring(indexCheck,indexManifest-indexCheck);
				StringAssert.Contains(between,"_hasRuntimeMismatch=true;");
				StringAssert.Contains(between,"return true;");
				StringAssert.Contains(move,"sourceFile.FileInfoCur.Name==ReleaseRuntimePart.MANIFEST_FILE_NAME");
				StringAssert.Contains(FileCopierStartTests.GetMethodBody(copier,"private void LaunchOpenDental("),"Run the setup of this release on this workstation");
				StringAssert.Contains(copier,"\"\\\\Helper32\",");
				string verify=FileCopierStartTests.GetMethodBody(copier,"private bool VerifyRelease(");
				StringAssert.Contains(verify,"ReleaseRuntimePart.ParseManifest(");
				StringAssert.Contains(verify,"releaseManifest.VerifyFolder(folder,hasRuntimePart)");
				StringAssert.Contains(FileCopierStartTests.GetMethodBody(copier,"private void StartFileCopierThread("),"KillProcessesInFolder(_destDirectory);");
				CollectionAssert.Contains(ForkReleases.ListSpecialDirs,"Helper32");
			}
			finally {
				DeleteQuietly(folderOld);
				DeleteQuietly(folderNew);
			}
		}

		///<summary>CopyFileCopier with a runtime folder (OpenFileCopier's temp folder "with one x64 runtime"): the copier's files and every runtime
		///file its .deps.json names, from the runtime folder; a runtime folder whose program names another runtime version is refused before
		///anything is copied; a missing runtime file is refused.</summary>
		[TestMethod]
		public void CopyFileCopier_WithItsRuntime_RefusesAnotherVersion() {
			string folderBuild=TestPaths.NewTempFolder("s3b7_copier_build");
			string folderRuntimeOld=TestPaths.NewTempFolder("s3b7_copier_old");
			string folderDest=Path.Combine(TestPaths.NewTempFolder("s3b7_copier_temp"),ForkReleases.FILE_COPIER_FOLDER_NAME);
			try {
				SyntheticBuild.Write(folderBuild,"10.0.15");
				string pathExe=ForkReleases.CopyFileCopier(folderBuild,folderDest,folderBuild);
				Assert.AreEqual(Path.Combine(folderDest,"UpdateFileCopier.exe"),pathExe);
				CollectionAssert.AreEquivalent(new[] { "UpdateFileCopier.exe","UpdateFileCopier.dll","UpdateFileCopier.runtimeconfig.json","UpdateFileCopier.deps.json",
					"coreclr.dll","hostfxr.dll","hostpolicy.dll","System.Private.CoreLib.dll" },Directory.GetFiles(folderDest).Select(x => Path.GetFileName(x)).ToArray());
				Assert.AreEqual("synthetic runtime coreclr.dll win-x64 10.0.15",File.ReadAllText(Path.Combine(folderDest,"coreclr.dll")));
				//The release's copier (10.0.15) with the program folder's runtime of another version: never.
				SyntheticBuild.Write(folderRuntimeOld,"10.0.14");
				File.WriteAllText(Path.Combine(folderDest,"marker.txt"),"before");
				InvalidDataException ex=Assert.ThrowsExactly<InvalidDataException>(() => ForkReleases.CopyFileCopier(folderBuild,folderDest,folderRuntimeOld));
				StringAssert.Contains(ex.Message,"runs on .NET 10.0.15, but the runtime files in");
				Assert.IsTrue(File.Exists(Path.Combine(folderDest,"marker.txt")),"Nothing was touched.");
				File.Delete(Path.Combine(folderBuild,"hostfxr.dll"));
				Assert.ThrowsExactly<FileNotFoundException>(() => ForkReleases.CopyFileCopier(folderBuild,folderDest,folderBuild));
			}
			finally {
				DeleteQuietly(folderBuild);
				DeleteQuietly(folderRuntimeOld);
				DeleteQuietly(Path.GetDirectoryName(folderDest));
			}
		}

		[TestMethod]
		public void ForkEnums_DocumentMiscTypes_OfTheRuntimePart() {
			Assert.AreEqual(102,(int)DocumentMiscType.ForkRuntimeManifest);
			Assert.AreEqual(103,(int)DocumentMiscType.ForkRuntimeSegment);
			Assert.AreEqual(ForkEnums.FEATURE_RELEASE_CHANNEL,ForkEnums.GetFeature(DocumentMiscType.ForkRuntimeManifest));
			Assert.AreEqual(ForkEnums.FEATURE_RELEASE_CHANNEL,ForkEnums.GetFeature(DocumentMiscType.ForkRuntimeSegment));
			string doc=TestPaths.ReadSource("docs","port-notes","FORK-ENUMS.md");
			StringAssert.Contains(doc,"| DocumentMiscType | 102   | ForkRuntimeManifest   | Release channel     |");
			StringAssert.Contains(doc,"| DocumentMiscType | 103   | ForkRuntimeSegment    | Release channel     |");
			//The release manifest's file and the runtime part's are never published as program files.
			Assert.IsFalse(ForkReleases.ShouldPublishFile(ReleaseRuntimePart.MANIFEST_FILE_NAME));
			Assert.IsFalse(ForkReleases.ShouldPublishFile(ReleaseManifest.FILE_NAME));
			CollectionAssert.Contains(ForkReleases.ListBuildIdFiles,"OpenPmsDental.dll");
		}

		#region The Release build (Build-Release.ps1 stages from OpenPmsDental\bin\Release)
		///<summary>The Release output, or inconclusive when it is not there: the build lane builds Debug, the Release dry run of the gate builds
		///Release (OPMS_RELEASE_STAGING names a staging folder instead).</summary>
		private static string GetReleaseFolder() {
			string folder=Environment.GetEnvironmentVariable("OPMS_RELEASE_STAGING");
			if(string.IsNullOrEmpty(folder)) {
				folder=Path.Combine(TestPaths.RepoRoot,"OpenPmsDental","bin","Release");
			}
			if(!File.Exists(Path.Combine(folder,"OpenPmsDental.deps.json"))) {
				Assert.Inconclusive("No Release build at "+folder+": run the Release dry run first (tools\\Build-Release.ps1, or the lane with -Configuration "
					+"Release -Projects OpenPmsDental,xDirect2dWrapper -ScopedRestore -Platform x64).");
			}
			return folder;
		}

		private static List<string> RunListFiles() {
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe");
			processStartInfo.Arguments="-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \""+Path.Combine(TestPaths.RepoRoot,"tools","Build-Release.ps1")+"\" -ListFiles";
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.CreateNoWindow=true;
			using(Process process=Process.Start(processStartInfo)) {
				Task<string> taskOut=process.StandardOutput.ReadToEndAsync();
				Task<string> taskErr=process.StandardError.ReadToEndAsync();
				Assert.IsTrue(process.WaitForExit(120000),"Build-Release.ps1 -ListFiles did not finish in 2 minutes.");
				process.WaitForExit();
				Assert.AreEqual(0,process.ExitCode,taskOut.Result+taskErr.Result);
				return taskOut.Result.Split(new[] { "\r\n","\n" },StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
			}
		}

		///<summary>Section 7, "Config file name" and "Package assets for a runtime identifier": next to each exe are its .dll, .runtimeconfig.json and
		///.deps.json (and .dll.config where it has an app.config); no *.exe.config; no folder but Helper32\, which is flat.</summary>
		[TestMethod]
		public void ReleaseBuild_Layout_ApphostsWithTheirFiles_NoExeConfig_OnlyHelper32() {
			string folder=GetReleaseFolder();
			foreach(string name in new[] { "OpenPmsDental","ServiceManager","OpenPmsDentalDbSettings","UpdateFileCopier" }) {
				foreach(string ending in new[] { ".exe",".dll",".runtimeconfig.json",".deps.json" }) {
					Assert.IsTrue(File.Exists(Path.Combine(folder,name+ending)),name+ending);
				}
			}
			foreach(string name in new[] { "OpenPmsDental","ServiceManager","OpenPmsDentalDbSettings" }) {
				Assert.IsTrue(File.Exists(Path.Combine(folder,name+".dll.config")),name+".dll.config");
			}
			Assert.AreEqual(0,Directory.GetFiles(folder,"*.exe.config",SearchOption.AllDirectories).Length,"No *.exe.config on .NET.");
			CollectionAssert.AreEqual(new[] { "Helper32" },Directory.GetDirectories(folder).Select(x => Path.GetFileName(x)).ToArray(),"Helper32\\ is the one sub folder.");
			Assert.AreEqual(0,Directory.GetDirectories(Path.Combine(folder,"Helper32")).Length,"Helper32\\ is flat.");
			foreach(string ending in new[] { ".exe",".dll",".runtimeconfig.json",".deps.json" }) {
				Assert.IsTrue(File.Exists(Path.Combine(folder,"Helper32","OpenPmsDentalHelper32"+ending)),"Helper32\\OpenPmsDentalHelper32"+ending);
			}
			//Both XmlSerializers files and the Roslyn DLLs the report engine loads.
			foreach(string fileName in new[] { "OpenPmsDental.XmlSerializers.dll","OpenPmsDentalBusiness.XmlSerializers.dll","Microsoft.CodeAnalysis.dll",
				"Microsoft.CodeAnalysis.VisualBasic.dll" })
			{
				Assert.IsTrue(File.Exists(Path.Combine(folder,fileName)),fileName);
			}
		}

		///<summary>Section 7, "Self-contained runtime part": every .runtimeconfig.json of the Release build, the helper's too, includes frameworks of
		///one runtime version, and the runtime part of the folder (GetRuntimePart) is exactly Build-Release's runtime list (read from the runtime
		///packs' data/RuntimeList.xml), x64 at the top and x86 in Helper32\, of that version.</summary>
		[TestMethod]
		public void ReleaseBuild_OneRuntimeVersion_TheRuntimePartIsBuildReleasesRuntimeList() {
			string folder=GetReleaseFolder();
			List<string> listVersions=new List<string>();
			foreach(string path in Directory.GetFiles(folder,"*.runtimeconfig.json",SearchOption.AllDirectories)) {
				string text=File.ReadAllText(path);
				StringAssert.Contains(text,"\"includedFrameworks\"",path+" is self-contained.");
				listVersions.AddRange(Regex.Matches(text,"\"version\":\\s*\"([^\"]+)\"").Cast<Match>().Select(x => x.Groups[1].Value));
			}
			Assert.IsTrue(listVersions.Count>=10,"Five programs, two or three frameworks each.");
			Assert.AreEqual(1,listVersions.Distinct().Count(),"One runtime version: "+string.Join(", ",listVersions.Distinct()));
			ReleaseRuntimePart releaseRuntimePart=ForkReleases.GetRuntimePart(folder);
			Assert.AreEqual(listVersions[0],releaseRuntimePart.Version);
			CollectionAssert.AreEqual(new[] { "win-x64","win-x86" },releaseRuntimePart.ListArchitectures);
			List<string> listRuntime=RunListFiles().Where(x => x.StartsWith("runtime|")).Select(x => x.Substring("runtime|".Length)).ToList();
			CollectionAssert.AreEquivalent(listRuntime,releaseRuntimePart.ListFiles.Select(x => x.RelativePath).ToList(),
				"The runtime files the .deps.json files name are the runtime packs' lists (if this fails after a monthly bump, the Release build is older than the newest runtime pack).");
			Assert.IsTrue(releaseRuntimePart.ListFiles.Count>400,"Both runtimes: "+releaseRuntimePart.ListFiles.Count);
			//The program part leaves them out, and holds the helper's own files.
			List<string> listProgram=ForkReleases.GetReleaseFiles(folder);
			Assert.IsFalse(listProgram.Intersect(listRuntime,StringComparer.OrdinalIgnoreCase).Any());
			CollectionAssert.Contains(listProgram,@"Helper32\OpenPmsDentalHelper32.exe");
		}

		///<summary>Section 7, "Native files" (decision 19): PE machine 0x8664 for every apphost and native DLL of the program folder and for the x64
		///runtime (a managed file is x64 or IL only), 0x14C for everything in Helper32\ (the helper and its x86 runtime).</summary>
		[TestMethod]
		public void ReleaseBuild_PeMachines_X64AtTheTop_X86InHelper32() {
			string folder=GetReleaseFolder();
			List<string> listProblems=new List<string>();
			int countPe=0;
			foreach(string path in Directory.GetFiles(folder,"*",SearchOption.AllDirectories)) {
				byte[] arrayBytes=File.ReadAllBytes(path);
				if(arrayBytes.Length<64 || arrayBytes[0]!=0x4D || arrayBytes[1]!=0x5A) {
					continue;
				}
				countPe++;
				int machine=PeExports.GetMachine(arrayBytes);
				string relative=path.Substring(folder.TrimEnd('\\').Length+1);
				bool isHelper=relative.StartsWith(@"Helper32\",StringComparison.OrdinalIgnoreCase);
				if(isHelper) {
					if(machine!=0x14C) {
						listProblems.Add(relative+": 0x"+machine.ToString("X"));
					}
					continue;
				}
				if(machine==0x8664) {
					continue;
				}
				//An IL-only file (machine 0x14C, ILONLY, not 32BITREQUIRED) runs as x64.
				uint flags;
				try {
					flags=PeFile.GetClrFlags(path);
				}
				catch(AssertFailedException) {
					listProblems.Add(relative+": native 0x"+machine.ToString("X"));
					continue;
				}
				catch(InvalidDataException) {
					listProblems.Add(relative+": native 0x"+machine.ToString("X"));
					continue;
				}
				if(machine!=0x14C || (flags & 0x1)==0 || (flags & 0x2)!=0) {
					listProblems.Add(relative+": 0x"+machine.ToString("X")+", CLR flags 0x"+flags.ToString("X"));
				}
			}
			Assert.IsTrue(countPe>500,"PE files: "+countPe);
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
			foreach(string fileName in new[] { "OpenPmsDental.exe","ServiceManager.exe","OpenPmsDentalDbSettings.exe","UpdateFileCopier.exe","Direct2dWrapper.dll",
				"WebView2Loader.dll","coreclr.dll" })
			{
				Assert.AreEqual(0x8664,PeFile.GetMachine(Path.Combine(folder,fileName)),fileName);
			}
			Assert.AreEqual(0x14C,PeFile.GetMachine(Path.Combine(folder,"Helper32","OpenPmsDentalHelper32.exe")));
			Assert.AreEqual(0x14C,PeFile.GetMachine(Path.Combine(folder,"Helper32","coreclr.dll")));
		}
		#endregion

		#region Build-Release.ps1's release-flow refusals
		private static List<string> CheckFolderFull(string folder,out int exitCode) {
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe");
			processStartInfo.Arguments="-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \""+Path.Combine(TestPaths.RepoRoot,"tools","Build-Release.ps1")
				+"\" -CheckFolder \""+folder+"\" -Full";
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			processStartInfo.CreateNoWindow=true;
			using(Process process=Process.Start(processStartInfo)) {
				Task<string> taskOut=process.StandardOutput.ReadToEndAsync();
				Task<string> taskErr=process.StandardError.ReadToEndAsync();
				Assert.IsTrue(process.WaitForExit(300000),"Build-Release.ps1 -CheckFolder did not finish in 5 minutes.");
				process.WaitForExit();
				exitCode=process.ExitCode;
				return (taskOut.Result+taskErr.Result).Split(new[] { "\r\n","\n" },StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
			}
		}

		///<summary>The newest 10.0.x Microsoft.NETCore.App.Runtime.win-x64 in the offline folder, or inconclusive without one.</summary>
		private static string GetNewestRuntimePack() {
			string folderOffline=NuGetFiles.GetOfflineFolder(TestPaths.RepoRoot);
			string folderPack=folderOffline==null ? null : Path.Combine(folderOffline,"microsoft.netcore.app.runtime.win-x64");
			if(folderPack==null || !Directory.Exists(folderPack)) {
				Assert.Inconclusive("No offline folder with the runtime packs.");
			}
			return Directory.GetDirectories(folderPack).Select(x => Path.GetFileName(x)).Where(x => Regex.IsMatch(x,@"^10\.0\.\d+$"))
				.OrderBy(x => new Version(x)).Last();
		}

		///<summary>Build-Release refuses to stage (section 7, "Self-contained runtime part"; section 2.4): .deps.json files of more than one runtime
		///version, a helper on another version than the program, a .runtimeconfig.json of another version, and a runtime older than the newest 10.0.x
		///runtime pack in the offline folder. A folder of the newest version and one helper of it passes these checks. Only these lines are read.
		///</summary>
		[TestMethod]
		public void BuildRelease_RefusesMixedHelperAndOlderRuntimes() {
			string newest=GetNewestRuntimePack();
			string older="10.0.1";
			Func<List<string>,List<string>> funcRuntimeLines=listLines => listLines.Where(x => x.StartsWith("Mixed runtime versions") || x.StartsWith("A different helper runtime")
				|| x.StartsWith("An older runtime") || x.StartsWith("No runtime") || x.StartsWith("No helper runtime")).ToList();
			string folder=TestPaths.NewTempFolder("s3b7_refusals");
			try {
				SyntheticBuild.Write(folder,newest);
				List<string> listLines=CheckFolderFull(folder,out int exitCode);
				Assert.AreEqual(0,funcRuntimeLines(listLines).Count,string.Join("\r\n",listLines));
				SyntheticBuild.Write(folder,newest,versionCopier:older);
				listLines=funcRuntimeLines(CheckFolderFull(folder,out exitCode));
				Assert.AreEqual(1,exitCode);
				Assert.IsTrue(listLines.Any(x => x.StartsWith("Mixed runtime versions: the programs' .deps.json files name")),string.Join("\r\n",listLines));
				Assert.IsTrue(listLines.Any(x => x.StartsWith("Mixed runtime versions: UpdateFileCopier.runtimeconfig.json includes Microsoft.NETCore.App "+older)),
					string.Join("\r\n",listLines));
				SyntheticBuild.Write(folder,newest,versionHelper:older);
				listLines=funcRuntimeLines(CheckFolderFull(folder,out exitCode));
				Assert.AreEqual(1,exitCode);
				Assert.IsTrue(listLines.Any(x => x.StartsWith("A different helper runtime: the helper in Helper32\\ runs on .NET "+older)),string.Join("\r\n",listLines));
				string folderOld=TestPaths.NewTempFolder("s3b7_refusals_old");
				SyntheticBuild.Write(folderOld,older);
				listLines=funcRuntimeLines(CheckFolderFull(folderOld,out exitCode));
				Assert.AreEqual(1,exitCode);
				Assert.IsTrue(listLines.Any(x => x.StartsWith("An older runtime: the release runs on .NET "+older+", but the offline folder has "
					+"Microsoft.NETCore.App.Runtime.win-x64 "+newest)),string.Join("\r\n",listLines));
				DeleteQuietly(folderOld);
			}
			finally {
				DeleteQuietly(folder);
			}
			string script=TestPaths.ReadSource("tools","Build-Release.ps1");
			//The staging step runs the checks on the Release output before it copies anything.
			Assert.IsTrue(script.IndexOf("$runtimeCheck=Test-RuntimeVersions $releaseOutput",StringComparison.Ordinal)
				<script.IndexOf("#region Stage",script.IndexOf("#region Release output",StringComparison.Ordinal),StringComparison.Ordinal));
			StringAssert.Contains(script,"data\\RuntimeList.xml");
		}
		#endregion
	}
}
