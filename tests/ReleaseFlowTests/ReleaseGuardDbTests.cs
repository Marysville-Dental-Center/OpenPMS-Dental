// The release-channel guards of the bridge release on a scratch database (S3a-10 of docs/specs/net10-plan.md): a published manifest that is not
// readable, one for another product and the .NET 10 release are read from the documentmisc rows, refused, and never downloaded. Synthetic files
// and rows only, in scratch databases named s3a10_ that the tests drop.
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
	public class ReleaseGuardDbTests {
		///<summary>This run's scratch database: s3a10_, the start time, the process id and a random number, so two runs never share one.</summary>
		private static readonly string DB_NAME="s3a10_"+DateTime.UtcNow.ToString("yyMMddHHmmss")+"_"+System.Diagnostics.Process.GetCurrentProcess().Id+"_"
			+new Random().Next(1000,10000)+"_guard";

		[ClassCleanup]
		public static void ClassCleanup() {
			try {
				ScratchDb.Exec(null,"DROP DATABASE IF EXISTS `"+DB_NAME+"`");
			}
			catch(Exception) {
				//No server, or it went away: nothing was left behind that a later run would mind.
			}
		}

		///<summary>Drops and creates this run's database with the tables publishing and pulling use, and points the business layer at it.</summary>
		private static void CreateDb() {
			ScratchDb.RequireServer();
			Assert.IsTrue(DB_NAME.StartsWith("s3a10_") && DB_NAME.Length<=64,DB_NAME);
			ScratchDb.Exec(null,"DROP DATABASE IF EXISTS `"+DB_NAME+"`");
			ScratchDb.Exec(null,"CREATE DATABASE `"+DB_NAME+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
			ScratchDb.Exec(DB_NAME,"CREATE TABLE preference (PrefName varchar(255) NOT NULL DEFAULT '',ValueString text NOT NULL,PrefNum bigint(20) NOT NULL "
				+"AUTO_INCREMENT,Comments text DEFAULT NULL,PRIMARY KEY (PrefNum),KEY PrefName (PrefName)) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3");
			ScratchDb.Exec(DB_NAME,"CREATE TABLE documentmisc (DocMiscNum bigint(20) NOT NULL AUTO_INCREMENT,DateCreated date NOT NULL DEFAULT '0001-01-01',"
				+"FileName varchar(255) NOT NULL,DocMiscType tinyint(4) NOT NULL,RawBase64 longtext NOT NULL,PRIMARY KEY (DocMiscNum)) "
				+"ENGINE=MyISAM DEFAULT CHARSET=utf8mb3");
			new DataConnection().SetDb("127.0.0.1",DB_NAME,"root","","root","",DatabaseType.MySql,true,DevDbTls.GetSslCa("127.0.0.1"));
		}

		private static string MakeInstallFolder() {
			string folder=TestPaths.NewTempFolder("s3a10_install");
			File.WriteAllText(Path.Combine(folder,"OpenPmsDental.exe"),"synthetic program "+Guid.NewGuid());
			File.WriteAllText(Path.Combine(folder,"OpenPmsDentalBusiness.dll"),"synthetic business layer");
			File.WriteAllText(Path.Combine(folder,"UpdateFileCopier.exe"),"synthetic copier");
			return folder;
		}

		private static ReleaseIdentity RunningBridge(string buildId="bridge") {
			return new ReleaseIdentity { ReleaseVersion="0.0",BuildId=buildId,BuildUtc=new DateTime(2026,11,1,12,0,0,DateTimeKind.Utc),
				Runtime=ReleaseRuntime.NetFramework48X86 };
		}

		///<summary>Publishes a synthetic release so the segment rows exist, and returns its manifest text as published.</summary>
		private static ReleaseManifest PublishSynthetic() {
			string folderInstall=MakeInstallFolder();
			ReleaseIdentity releaseIdentity=new ReleaseIdentity { ReleaseVersion="0.0",BuildId=ForkReleases.ComputeBuildId(folderInstall),
				BuildUtc=new DateTime(2026,11,15,9,0,0,DateTimeKind.Utc) };
			return ForkReleases.Publish(folderInstall,releaseIdentity,"SERVER1",new DateTime(2026,11,15,9,30,0,DateTimeKind.Utc));
		}

		private static void SetManifestRow(string text) {
			ScratchDb.Exec(DB_NAME,"UPDATE documentmisc SET RawBase64='"+Convert.ToBase64String(Encoding.UTF8.GetBytes(text))+"' WHERE DocMiscType=100");
		}

		private static void SetManifestRowRaw(string rawBase64) {
			ScratchDb.Exec(DB_NAME,"UPDATE documentmisc SET RawBase64='"+MySqlConnector.MySqlHelper.EscapeString(rawBase64)+"' WHERE DocMiscType=100");
		}

		///<summary>Every way the published manifest row can be unreadable gives an error, never "nothing published", and every role refuses it.
		///</summary>
		[TestMethod]
		public void UnreadableOrForeignManifestRow_IsRefused_NeverNothingPublished() {
			CreateDb();
			ReleaseManifest releaseManifest=PublishSynthetic();
			string text=releaseManifest.Serialize();
			Dictionary<string,Action> dictCases=new Dictionary<string,Action>() {
				{ "not base64",() => SetManifestRowRaw("*** not base64 ***") },
				{ "empty",() => SetManifestRowRaw("") },
				{ "a newer format",() => SetManifestRow(text.Replace("Format=1","Format=2")) },
				{ "an unknown key",() => SetManifestRow(text.Replace("PublishedBy=","SomethingNew=1\nPublishedBy=")) },
				{ "a truncated manifest",() => SetManifestRow(text.Substring(0,text.IndexOf("BuildId="))) },
				{ "another product",() => SetManifestRow(text.Replace("Product="+ReleaseManifest.PRODUCT_NAME,"Product=Some Other Program")) },
			};
			foreach(KeyValuePair<string,Action> pair in dictCases) {
				pair.Value();
				ReleaseManifest releaseManifestPublished=ForkReleases.GetPublishedManifest(out string error);
				Assert.IsNull(releaseManifestPublished,pair.Key);
				Assert.AreNotEqual("",error,pair.Key+": an unreadable manifest is not \"nothing published\".");
				foreach(UpdateServerRole updateServerRole in new[] { UpdateServerRole.ThisComputer,UpdateServerRole.AnyWorkstation,UpdateServerRole.OtherComputer }) {
					ReleaseDecision releaseDecision=ForkReleases.Decide(releaseManifestPublished,error,RunningBridge(),updateServerRole,false,"SERVER1");
					Assert.AreEqual(ReleaseAction.RefuseUnreadable,releaseDecision.Action,pair.Key+", "+updateServerRole);
					StringAssert.Contains(releaseDecision.Message,error);
				}
				Assert.AreNotEqual("",ForkReleases.GetRefusal(releaseManifestPublished,error,RunningBridge()),pair.Key+": Recopy and the guard's pull refuse it.");
				Assert.IsNull(ForkReleases.GetPublishedManifestOrMarker(RunningBridge("marker")),pair.Key+": the restart check lets the workstation run on.");
				Assert.IsFalse(ForkReleases.DownloadAndVerify(Path.Combine(TestPaths.NewTempFolder("s3a10_unreadable"),"ForkRelease"),RunningBridge()).IsOk,pair.Key);
			}
			Assert.AreEqual("1",ScratchDb.Scalar(DB_NAME,"SELECT COUNT(*) FROM documentmisc WHERE DocMiscType=100"),"Nothing replaced the published row.");
		}

		///<summary>A bridge workstation reads the .NET 10 release's manifest (its Runtime and runtime part lines), refuses it with "run the new setup"
		///and downloads nothing of it.</summary>
		[TestMethod]
		public void Net10ReleaseRow_IsReadable_RefusedAndNotDownloaded() {
			CreateDb();
			ReleaseManifest releaseManifestBridge=PublishSynthetic();
			//The .NET 10 release as 3b would publish it: the same rows, with a manifest that names its runtime and runtime part.
			ReleaseManifest releaseManifestNet10=ReleaseManifest.Parse(releaseManifestBridge.Serialize());
			releaseManifestNet10.ReleaseVersion="1.0";
			releaseManifestNet10.BuildId=new string('4',64);
			releaseManifestNet10.Runtime=ReleaseRuntime.Parse("Microsoft.NETCore.App 10.0.14 win-x64");
			ReleaseRuntimePart releaseRuntimePart=new ReleaseRuntimePart();
			releaseRuntimePart.Version="10.0.14";
			releaseRuntimePart.ListArchitectures=new List<string> { "win-x64","win-x86" };
			releaseRuntimePart.ArchiveSha256=new string('6',64);
			releaseRuntimePart.ListFiles.Add(new ReleaseManifestFile { RelativePath="coreclr.dll",Size=7340032,Sha256=new string('1',64) });
			releaseRuntimePart.Key=releaseRuntimePart.ComputeKey();
			releaseManifestNet10.RuntimePart=releaseRuntimePart;
			SetManifestRow(releaseManifestNet10.Serialize());
			ReleaseManifest releaseManifestPublished=ForkReleases.GetPublishedManifest(out string error);
			Assert.AreEqual("",error,"The bridge reads a .NET 10 manifest.");
			Assert.AreEqual("Microsoft.NETCore.App 10.0.14 win-x64",releaseManifestPublished.Runtime.ToString());
			ReleaseDecision releaseDecision=ForkReleases.Decide(releaseManifestPublished,error,RunningBridge(),UpdateServerRole.ThisComputer,false);
			Assert.AreEqual(ReleaseAction.RefuseOtherRuntime,releaseDecision.Action,"The update server never publishes its old build over it.");
			StringAssert.Contains(releaseDecision.Message,"run the new setup");
			string folderPull=Path.Combine(TestPaths.NewTempFolder("s3a10_net10"),"ForkRelease");
			ReleaseDownloadResult releaseDownloadResult=ForkReleases.DownloadAndVerify(folderPull,RunningBridge());
			Assert.IsFalse(releaseDownloadResult.IsOk);
			StringAssert.Contains(releaseDownloadResult.ListProblems.Single(),"run the new setup");
			Assert.IsFalse(Directory.Exists(folderPull),"Nothing is downloaded for another runtime.");
			//The restart check keeps a bridge workstation running on it, until its setup runs.
			ReleaseIdentity releaseIdentityMarker=new ReleaseIdentity { ReleaseVersion="1.0",BuildId=releaseManifestNet10.BuildId };
			ReleaseManifest releaseManifestOrMarker=ForkReleases.GetPublishedManifestOrMarker(releaseIdentityMarker);
			Assert.IsNotNull(releaseManifestOrMarker.Runtime,"The marker's build is the published one, so its manifest (with the runtime) is used.");
			Assert.IsTrue(ForkReleases.MayRunUnpublishedBuild(releaseManifestOrMarker,RunningBridge(),UpdateServerRole.OtherComputer));
		}

		///<summary>A release for this runtime still downloads with the runtime check on (the program passes its own identity).</summary>
		[TestMethod]
		public void BridgeRelease_DownloadsWithTheRuntimeCheck() {
			CreateDb();
			PublishSynthetic();
			string folderPull=Path.Combine(TestPaths.NewTempFolder("s3a10_bridge"),"ForkRelease");
			ReleaseDownloadResult releaseDownloadResult=ForkReleases.DownloadAndVerify(folderPull,RunningBridge());
			Assert.IsTrue(releaseDownloadResult.IsOk,string.Join("; ",releaseDownloadResult.ListProblems));
			Assert.AreEqual("synthetic copier",File.ReadAllText(Path.Combine(folderPull,"UpdateFileCopier.exe")));
		}
	}
}
