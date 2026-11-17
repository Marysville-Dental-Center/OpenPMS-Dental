// The runtime part on the release channel (S3b-7 of the .NET 10 move, decision 20; docs/port-notes/S3b-7.md, section 7 of
// docs/specs/net10-plan.md "Self-contained runtime part"): a publish writes the runtime part (documentmisc types 102 and 103) only when its key
// differs from the published one, then the program part, its manifest last; a pull downloads the runtime part only when the published key
// differs from the installation's, and checks both parts in full before anything is copied; a runtime part that is not the one the release
// names, or whose archive does not match, is refused; a workstation never ends with runtime files of two versions. Synthetic self-contained
// builds (SyntheticBuild) in scratch folders, and scratch databases named s3b7_ on the local MariaDB that the class drops.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ReleaseFlowTests {
	[TestClass]
	public class RuntimePartDbTests {
		///<summary>This run's scratch database: s3b7_, the start time, the process id and a random number, so two runs never share one.</summary>
		private static readonly string DB_NAME="s3b7_"+DateTime.UtcNow.ToString("yyMMddHHmmss")+"_"+System.Diagnostics.Process.GetCurrentProcess().Id+"_"
			+new Random().Next(1000,10000)+"_runtime";
		private static readonly List<string> _listFolders=new List<string>();

		[ClassCleanup]
		public static void ClassCleanup() {
			try {
				ScratchDb.Exec(null,"DROP DATABASE IF EXISTS `"+DB_NAME+"`");
			}
			catch(Exception) {
				//No server, or it went away: nothing was left behind that a later run would mind.
			}
			foreach(string folder in _listFolders) {
				try {
					Directory.Delete(folder,true);
				}
				catch(Exception) {
					//Left in the temp folder.
				}
			}
		}

		///<summary>Drops and creates this run's database with the tables publishing and pulling use, and points the business layer at it.</summary>
		private static void CreateDb() {
			ScratchDb.RequireServer();
			Assert.IsTrue(DB_NAME.StartsWith("s3b7_") && DB_NAME.Length<=64,DB_NAME);
			ScratchDb.Exec(null,"DROP DATABASE IF EXISTS `"+DB_NAME+"`");
			ScratchDb.Exec(null,"CREATE DATABASE `"+DB_NAME+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
			ScratchDb.Exec(DB_NAME,"CREATE TABLE preference (PrefName varchar(255) NOT NULL DEFAULT '',ValueString text NOT NULL,PrefNum bigint(20) NOT NULL "
				+"AUTO_INCREMENT,Comments text DEFAULT NULL,PRIMARY KEY (PrefNum),KEY PrefName (PrefName)) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3");
			ScratchDb.Exec(DB_NAME,"CREATE TABLE documentmisc (DocMiscNum bigint(20) NOT NULL AUTO_INCREMENT,DateCreated date NOT NULL DEFAULT '0001-01-01',"
				+"FileName varchar(255) NOT NULL,DocMiscType tinyint(4) NOT NULL,RawBase64 longtext NOT NULL,PRIMARY KEY (DocMiscNum)) "
				+"ENGINE=MyISAM DEFAULT CHARSET=utf8mb3");
			new DataConnection().SetDb("127.0.0.1",DB_NAME,"root","","root","",DatabaseType.MySql,true,DevDbTls.GetSslCa("127.0.0.1"));
		}

		private static string NewFolder(string name) {
			string folder=TestPaths.NewTempFolder("s3b7_"+name);
			_listFolders.Add(folder);
			return folder;
		}

		///<summary>A synthetic self-contained build of the runtime version, published; programSalt makes another build of the program.</summary>
		private static ReleaseManifest PublishBuild(string folder,string version,string programSalt,DateTime dateTimeBuildUtc) {
			SyntheticBuild.Write(folder,version,programSalt:programSalt);
			ReleaseIdentity releaseIdentity=new ReleaseIdentity { ReleaseVersion="1.0",BuildId=ForkReleases.ComputeBuildId(folder),BuildUtc=dateTimeBuildUtc,
				Runtime=ReleaseRuntime.GetRunning() };
			return ForkReleases.Publish(folder,releaseIdentity,"SERVER1",dateTimeBuildUtc.AddMinutes(30));
		}

		///<summary>The rows of the type: DocMiscNum and RawBase64, in DocMiscNum order.</summary>
		private static List<string> GetRows(DocumentMiscType documentMiscType) {
			return DocumentMiscs.GetByType(documentMiscType,canIncludeRawBase64:true).OrderBy(x => x.DocMiscNum)
				.Select(x => x.DocMiscNum+"|"+x.RawBase64).ToList();
		}

		private static ReleaseIdentity RunningNet10(string buildId) {
			return new ReleaseIdentity { ReleaseVersion="1.0",BuildId=buildId,BuildUtc=new DateTime(2027,1,1,12,0,0,DateTimeKind.Utc),
				Runtime=ReleaseRuntime.GetRunning() };
		}

		///<summary>A release with a new runtime: the publish writes the runtime part (its manifest row of type 102 and its segments of type 103) and
		///the program part, which names it; a pull from an installation of another runtime downloads both parts, checks them and holds the release's
		///runtime files; a pull with no installation folder known downloads both too.</summary>
		[TestMethod]
		public void Publish_NewRuntime_WritesBothParts_AndThePullDownloadsBoth() {
			CreateDb();
			string folderServer=NewFolder("server");
			ReleaseManifest releaseManifest=PublishBuild(folderServer,"10.0.15","a",new DateTime(2027,1,10,9,0,0,DateTimeKind.Utc));
			Assert.IsNotNull(releaseManifest.RuntimePart);
			Assert.AreEqual(1,GetRows(DocumentMiscType.ForkRuntimeManifest).Count);
			Assert.IsTrue(GetRows(DocumentMiscType.ForkRuntimeSegment).Count>=1);
			Assert.AreEqual(1,GetRows(DocumentMiscType.ForkReleaseManifest).Count);
			ReleaseRuntimePart releaseRuntimePartPublished=ForkReleases.GetPublishedRuntimePart(out string errorRuntime);
			Assert.AreEqual("",errorRuntime);
			Assert.IsTrue(releaseRuntimePartPublished.IsSameAs(releaseManifest.RuntimePart));
			Assert.AreEqual(releaseManifest.RuntimePart.ArchiveSha256,releaseRuntimePartPublished.ArchiveSha256);
			ReleaseManifest releaseManifestPublished=ForkReleases.GetPublishedManifest(out string error);
			Assert.AreEqual("",error);
			Assert.AreEqual(releaseManifest.RuntimePart.Key,releaseManifestPublished.RuntimePart.Key,"The program part names the runtime part it needs.");
			Assert.IsFalse(releaseManifestPublished.ListFiles.Any(x => x.RelativePath.EndsWith("coreclr.dll",StringComparison.OrdinalIgnoreCase)),
				"No runtime file in the program part.");
			//A workstation on the old runtime pulls both parts.
			string folderInstall=NewFolder("install_old");
			SyntheticBuild.Write(folderInstall,"10.0.14",programSalt:"old");
			string folderPull=Path.Combine(NewFolder("pull_new"),"ForkRelease");
			ReleaseDownloadResult releaseDownloadResult=ForkReleases.DownloadAndVerify(folderPull,RunningNet10("old"),folderInstall);
			Assert.IsTrue(releaseDownloadResult.IsOk,string.Join("\r\n",releaseDownloadResult.ListProblems));
			Assert.IsTrue(releaseDownloadResult.IsRuntimeIncluded);
			Assert.AreEqual("synthetic runtime coreclr.dll win-x86 10.0.15",File.ReadAllText(Path.Combine(folderPull,"Helper32","coreclr.dll")));
			Assert.IsTrue(File.Exists(Path.Combine(folderPull,ReleaseRuntimePart.MANIFEST_FILE_NAME)));
			Assert.AreEqual(0,releaseDownloadResult.Manifest.VerifyFolder(folderPull,includeRuntimePart:true).Count);
			//The copy into the installation folder: afterwards every runtime file is of the release's one runtime version.
			SyntheticBuild.CopyLikeTheCopier(folderPull,folderInstall);
			Assert.AreEqual(0,releaseDownloadResult.Manifest.RuntimePart.VerifyFiles(folderInstall).Count);
			Assert.AreEqual(releaseManifest.RuntimePart.Key,ForkReleases.GetRuntimePart(folderInstall).Key);
			//No installation folder known (an older caller): both parts.
			string folderPullNoInstall=Path.Combine(NewFolder("pull_noinstall"),"ForkRelease");
			releaseDownloadResult=ForkReleases.DownloadAndVerify(folderPullNoInstall,RunningNet10("old"));
			Assert.IsTrue(releaseDownloadResult.IsOk && releaseDownloadResult.IsRuntimeIncluded,string.Join("\r\n",releaseDownloadResult.ListProblems));
		}

		///<summary>A release on the same runtime: the publish writes no runtime row (the runtime part's rows are the same rows as before) and replaces
		///the program part; a pull from an installation on that runtime downloads no runtime segment: it still succeeds with the runtime part's
		///segments made unreadable, holds no runtime file, and the installation's runtime key comes from its release manifest or, without one,
		///from its runtime files.</summary>
		[TestMethod]
		public void Publish_UnchangedRuntime_WritesNoRuntimeRows_AndThePullDownloadsNoRuntimeSegment() {
			CreateDb();
			string folderServer=NewFolder("server_same");
			ReleaseManifest releaseManifestFirst=PublishBuild(folderServer,"10.0.15","first",new DateTime(2027,1,10,9,0,0,DateTimeKind.Utc));
			List<string> listRuntimeManifestRows=GetRows(DocumentMiscType.ForkRuntimeManifest);
			List<string> listRuntimeSegmentRows=GetRows(DocumentMiscType.ForkRuntimeSegment);
			List<string> listProgramRows=GetRows(DocumentMiscType.ForkReleaseManifest);
			ReleaseManifest releaseManifestSecond=PublishBuild(folderServer,"10.0.15","second",new DateTime(2027,1,20,9,0,0,DateTimeKind.Utc));
			Assert.AreNotEqual(releaseManifestFirst.BuildId,releaseManifestSecond.BuildId,"Another build of the program.");
			CollectionAssert.AreEqual(listRuntimeManifestRows,GetRows(DocumentMiscType.ForkRuntimeManifest),"No runtime manifest row written.");
			CollectionAssert.AreEqual(listRuntimeSegmentRows,GetRows(DocumentMiscType.ForkRuntimeSegment),"No runtime segment written.");
			CollectionAssert.AreNotEqual(listProgramRows,GetRows(DocumentMiscType.ForkReleaseManifest),"The program part is replaced.");
			Assert.AreEqual(releaseManifestFirst.RuntimePart.Key,releaseManifestSecond.RuntimePart.Key);
			Assert.AreEqual(releaseManifestFirst.RuntimePart.ArchiveSha256,releaseManifestSecond.RuntimePart.ArchiveSha256,
				"The program part names the runtime part that is published.");
			//The pull never reads a runtime segment: make them unreadable.
			ScratchDb.Exec(DB_NAME,"UPDATE documentmisc SET RawBase64='*** not base64 ***' WHERE DocMiscType="+(int)DocumentMiscType.ForkRuntimeSegment);
			string folderInstall=NewFolder("install_same");
			SyntheticBuild.Write(folderInstall,"10.0.15",programSalt:"first");
			string folderPull=Path.Combine(NewFolder("pull_same"),"ForkRelease");
			ReleaseDownloadResult releaseDownloadResult=ForkReleases.DownloadAndVerify(folderPull,RunningNet10(releaseManifestFirst.BuildId),folderInstall);
			Assert.IsTrue(releaseDownloadResult.IsOk,string.Join("\r\n",releaseDownloadResult.ListProblems));
			Assert.IsFalse(releaseDownloadResult.IsRuntimeIncluded);
			Assert.IsFalse(File.Exists(Path.Combine(folderPull,"coreclr.dll")),"No runtime file downloaded.");
			Assert.IsFalse(File.Exists(Path.Combine(folderPull,"Helper32","coreclr.dll")));
			Assert.IsFalse(File.Exists(Path.Combine(folderPull,ReleaseRuntimePart.MANIFEST_FILE_NAME)));
			Assert.AreEqual("synthetic program second",File.ReadAllText(Path.Combine(folderPull,"OpenPmsDental.dll")));
			//The installation's key from its release manifest, which UpdateFileCopier wrote after it checked every runtime file.
			File.WriteAllText(Path.Combine(folderInstall,ReleaseManifest.FILE_NAME),releaseManifestFirst.Serialize());
			Assert.AreEqual(releaseManifestFirst.RuntimePart.Key,ForkReleases.GetInstalledRuntimeKey(folderInstall));
			string folderPull2=Path.Combine(NewFolder("pull_same2"),"ForkRelease");
			releaseDownloadResult=ForkReleases.DownloadAndVerify(folderPull2,RunningNet10(releaseManifestFirst.BuildId),folderInstall);
			Assert.IsTrue(releaseDownloadResult.IsOk && !releaseDownloadResult.IsRuntimeIncluded,string.Join("\r\n",releaseDownloadResult.ListProblems));
			//A workstation on another runtime needs the segments, which are unreadable now: refused, nothing to copy.
			string folderInstallOld=NewFolder("install_other");
			SyntheticBuild.Write(folderInstallOld,"10.0.14");
			releaseDownloadResult=ForkReleases.DownloadAndVerify(Path.Combine(NewFolder("pull_other"),"ForkRelease"),RunningNet10("old"),folderInstallOld);
			Assert.IsFalse(releaseDownloadResult.IsOk);
			StringAssert.StartsWith(releaseDownloadResult.ListProblems.Single(),"The runtime part's archive is not readable:");
		}

		///<summary>A release with a new runtime replaces the runtime part (the old rows are gone, the new key is published), and the program part
		///names it; an installation on the new runtime pulls the program part only, one on the old runtime both parts.</summary>
		[TestMethod]
		public void Publish_NewRuntime_ReplacesTheRuntimePart() {
			CreateDb();
			string folderServer=NewFolder("server_bump");
			ReleaseManifest releaseManifestOld=PublishBuild(folderServer,"10.0.14","old",new DateTime(2027,1,10,9,0,0,DateTimeKind.Utc));
			List<string> listOldRows=GetRows(DocumentMiscType.ForkRuntimeManifest).Concat(GetRows(DocumentMiscType.ForkRuntimeSegment)).ToList();
			ReleaseManifest releaseManifestNew=PublishBuild(folderServer,"10.0.15","new",new DateTime(2027,2,10,9,0,0,DateTimeKind.Utc));
			Assert.AreNotEqual(releaseManifestOld.RuntimePart.Key,releaseManifestNew.RuntimePart.Key);
			List<string> listNewRows=GetRows(DocumentMiscType.ForkRuntimeManifest).Concat(GetRows(DocumentMiscType.ForkRuntimeSegment)).ToList();
			Assert.AreEqual(0,listNewRows.Intersect(listOldRows).Count(),"Every runtime row was replaced.");
			Assert.AreEqual(1,GetRows(DocumentMiscType.ForkRuntimeManifest).Count);
			Assert.AreEqual("10.0.15",ForkReleases.GetPublishedRuntimePart(out string _).Version);
			Assert.AreEqual("Microsoft.NETCore.App 10.0.15 win-x64",ForkReleases.GetPublishedManifest(out string _).Runtime.ToString());
			string folderInstallNew=NewFolder("install_bumped");
			SyntheticBuild.Write(folderInstallNew,"10.0.15",programSalt:"other");
			ReleaseDownloadResult releaseDownloadResult=ForkReleases.DownloadAndVerify(Path.Combine(NewFolder("pull_bumped"),"ForkRelease"),RunningNet10("x"),
				folderInstallNew);
			Assert.IsTrue(releaseDownloadResult.IsOk && !releaseDownloadResult.IsRuntimeIncluded,string.Join("\r\n",releaseDownloadResult.ListProblems));
			string folderInstallOld=NewFolder("install_notbumped");
			SyntheticBuild.Write(folderInstallOld,"10.0.14",programSalt:"old");
			releaseDownloadResult=ForkReleases.DownloadAndVerify(Path.Combine(NewFolder("pull_notbumped"),"ForkRelease"),RunningNet10(releaseManifestOld.BuildId),
				folderInstallOld);
			Assert.IsTrue(releaseDownloadResult.IsOk && releaseDownloadResult.IsRuntimeIncluded,string.Join("\r\n",releaseDownloadResult.ListProblems));
		}

		///<summary>A pull whose runtime part is not the one the release names (another key published after it, a missing runtime part, an archive
		///that does not match its SHA-256) is refused, so nothing is copied; the problem says to publish again.</summary>
		[TestMethod]
		public void Pull_RuntimePartThatIsNotTheReleases_IsRefused() {
			CreateDb();
			string folderServer=NewFolder("server_mismatch");
			PublishBuild(folderServer,"10.0.15","a",new DateTime(2027,1,10,9,0,0,DateTimeKind.Utc));
			string folderInstall=NewFolder("install_mismatch");
			SyntheticBuild.Write(folderInstall,"10.0.14");
			//Another runtime part published without its program part (a publish that stopped between the two parts).
			string folderOther=NewFolder("other_runtime");
			SyntheticBuild.Write(folderOther,"10.0.16");
			ReleaseRuntimePart releaseRuntimePartOther=ForkReleases.GetRuntimePart(folderOther);
			byte[] arrayOther=ForkReleases.BuildRuntimeArchive(folderOther,releaseRuntimePartOther);
			List<string> listSavedManifest=GetRows(DocumentMiscType.ForkRuntimeManifest);
			string rawManifestSaved=DocumentMiscs.GetForkRuntimeManifest().RawBase64;
			DocumentMiscs.ReplaceForkRuntime(Convert.ToBase64String(Encoding.UTF8.GetBytes(releaseRuntimePartOther.SerializeManifest())),
				new List<string> { Convert.ToBase64String(arrayOther) });
			ReleaseDownloadResult releaseDownloadResult=ForkReleases.DownloadAndVerify(Path.Combine(NewFolder("pull_mismatch"),"ForkRelease"),RunningNet10("old"),
				folderInstall);
			Assert.IsFalse(releaseDownloadResult.IsOk);
			StringAssert.Contains(releaseDownloadResult.ListProblems.Single(),"is not the one this release needs");
			StringAssert.Contains(releaseDownloadResult.ListProblems.Single(),"Publish the release again from the update server.");
			//No runtime part at all.
			DocumentMiscs.DeleteAllForType(DocumentMiscType.ForkRuntimeManifest);
			releaseDownloadResult=ForkReleases.DownloadAndVerify(Path.Combine(NewFolder("pull_none"),"ForkRelease"),RunningNet10("old"),folderInstall);
			Assert.IsFalse(releaseDownloadResult.IsOk);
			StringAssert.StartsWith(releaseDownloadResult.ListProblems.Single(),"The runtime part this release needs is not published.");
			//The right manifest row, but segments of other bytes: the archive does not match its SHA-256.
			DocumentMiscs.ReplaceForkRuntime(rawManifestSaved,new List<string> { Convert.ToBase64String(arrayOther) });
			releaseDownloadResult=ForkReleases.DownloadAndVerify(Path.Combine(NewFolder("pull_badsegment"),"ForkRelease"),RunningNet10("old"),folderInstall);
			Assert.IsFalse(releaseDownloadResult.IsOk);
			StringAssert.StartsWith(releaseDownloadResult.ListProblems.Single(),"The runtime part's archive does not match its SHA-256.");
			Assert.AreEqual(1,listSavedManifest.Count);
		}

		///<summary>The forward publish of S3a-10 on the .NET 10 build (its port note asks S3b-7 to repeat it): the named update server, now on .NET 10,
		///publishes its release over a .NET Framework release, with the runtime part; a .NET Framework workstation then refuses it ("run the new
		///setup") and downloads nothing; a .NET 10 workstation that is not the update server never pulls the .NET Framework release back.</summary>
		[TestMethod]
		public void Net10UpdateServer_PublishesOverTheNetFrameworkRelease_WithTheRuntimePart() {
			CreateDb();
			string folderBridge=NewFolder("bridge");
			File.WriteAllText(Path.Combine(folderBridge,"OpenPmsDental.exe"),"synthetic bridge");
			File.WriteAllText(Path.Combine(folderBridge,"UpdateFileCopier.exe"),"synthetic bridge copier");
			ReleaseIdentity releaseIdentityBridge=new ReleaseIdentity { ReleaseVersion="0.0",BuildId=ForkReleases.ComputeBuildId(folderBridge),
				BuildUtc=new DateTime(2026,12,1,9,0,0,DateTimeKind.Utc),Runtime=ReleaseRuntime.NetFramework48X86 };
			ForkReleases.Publish(folderBridge,releaseIdentityBridge,"SERVER1",new DateTime(2026,12,1,9,30,0,DateTimeKind.Utc));
			Assert.AreEqual(0,GetRows(DocumentMiscType.ForkRuntimeManifest).Count,"A .NET Framework build has no runtime part.");
			ReleaseManifest releaseManifestBridge=ForkReleases.GetPublishedManifest(out string error);
			Assert.IsNull(releaseManifestBridge.Runtime);
			string folderServer=NewFolder("net10_server");
			SyntheticBuild.Write(folderServer,"10.0.15");
			ReleaseIdentity releaseIdentityNet10=new ReleaseIdentity { ReleaseVersion="1.0",BuildId=ForkReleases.ComputeBuildId(folderServer),
				BuildUtc=new DateTime(2027,1,10,9,0,0,DateTimeKind.Utc),Runtime=ReleaseRuntime.GetRunning() };
			ReleaseDecision releaseDecision=ForkReleases.Decide(releaseManifestBridge,error,releaseIdentityNet10,UpdateServerRole.ThisComputer,false,"SERVER1");
			Assert.AreEqual(ReleaseAction.Publish,releaseDecision.Action,releaseDecision.Message);
			Assert.AreEqual("",ForkReleases.GetRefusal(releaseManifestBridge,error,releaseIdentityNet10,UpdateServerRole.ThisComputer),"Recopy publishes it too.");
			ForkReleases.Publish(folderServer,releaseIdentityNet10,"SERVER1",new DateTime(2027,1,10,9,30,0,DateTimeKind.Utc));
			ReleaseManifest releaseManifestNet10=ForkReleases.GetPublishedManifest(out error);
			Assert.AreEqual("",error);
			Assert.AreEqual("Microsoft.NETCore.App 10.0.15 win-x64",releaseManifestNet10.Runtime.ToString());
			Assert.IsNotNull(releaseManifestNet10.RuntimePart);
			Assert.AreEqual(1,GetRows(DocumentMiscType.ForkRuntimeManifest).Count);
			//The .NET Framework workstation: refused, nothing downloaded.
			Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,ForkReleases.Decide(releaseManifestNet10,error,releaseIdentityBridge,UpdateServerRole.OtherComputer,false).Action);
			string folderPull=Path.Combine(NewFolder("pull_bridge"),"ForkRelease");
			ReleaseDownloadResult releaseDownloadResult=ForkReleases.DownloadAndVerify(folderPull,releaseIdentityBridge,folderBridge);
			Assert.IsFalse(releaseDownloadResult.IsOk);
			StringAssert.Contains(releaseDownloadResult.ListProblems.Single(),"run the new setup");
			Assert.IsFalse(Directory.Exists(folderPull));
			//A .NET 10 workstation of the same architecture pulls it normally.
			string folderInstall=NewFolder("net10_ws");
			SyntheticBuild.Write(folderInstall,"10.0.15",programSalt:"ws");
			releaseDownloadResult=ForkReleases.DownloadAndVerify(Path.Combine(NewFolder("pull_ws"),"ForkRelease"),RunningNet10("ws"),folderInstall);
			Assert.IsTrue(releaseDownloadResult.IsOk && !releaseDownloadResult.IsRuntimeIncluded,string.Join("\r\n",releaseDownloadResult.ListProblems));
			//An x86 .NET workstation (another architecture) is refused before anything is downloaded.
			ReleaseIdentity releaseIdentityX86=new ReleaseIdentity { ReleaseVersion="1.0",BuildId="x86",
				Runtime=new ReleaseRuntime(ReleaseRuntime.FRAMEWORK_NET_CORE_APP,"10.0.15",ReleaseRuntime.ARCHITECTURE_X86) };
			string folderPullX86=Path.Combine(NewFolder("pull_x86"),"ForkRelease");
			releaseDownloadResult=ForkReleases.DownloadAndVerify(folderPullX86,releaseIdentityX86,folderInstall);
			Assert.IsFalse(releaseDownloadResult.IsOk);
			StringAssert.Contains(releaseDownloadResult.ListProblems.Single(),"win-x64");
			Assert.IsFalse(Directory.Exists(folderPullX86));
		}
	}
}
