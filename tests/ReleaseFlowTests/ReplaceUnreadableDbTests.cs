// Recopy over a published manifest that is not readable (S3a-10f, docs/port-notes/S3a-10.md section 12; the owner's decision of 2026-11-15):
// only the named update server is offered it, only after a Yes to the question, and then the documentmisc rows of types 100 and 101 hold
// this build, published as for a first release, with a security log entry. No, a silent run and every other computer change nothing. The
// question is the seam these tests answer: no window is shown. Synthetic files and rows only, in scratch databases named s3a10f_ that the
// tests drop.
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
	public class ReplaceUnreadableDbTests {
		///<summary>This run's scratch database: s3a10f_, the start time, the process id and a random number, so two runs never share one.</summary>
		private static readonly string DB_NAME="s3a10f_"+DateTime.UtcNow.ToString("yyMMddHHmmss")+"_"+System.Diagnostics.Process.GetCurrentProcess().Id+"_"
			+new Random().Next(1000,10000)+"_replace";
		private static readonly UpdateServerRole[] _arrayRolesNotNamed=new[] { UpdateServerRole.OtherComputer,UpdateServerRole.AnyWorkstation };

		[ClassCleanup]
		public static void ClassCleanup() {
			try {
				ScratchDb.Exec(null,"DROP DATABASE IF EXISTS `"+DB_NAME+"`");
			}
			catch(Exception) {
				//No server, or it went away: nothing was left behind that a later run would mind.
			}
		}

		///<summary>Drops and creates this run's database with the tables publishing and the security log use, and points the business layer at it.
		///</summary>
		private static void CreateDb() {
			ScratchDb.RequireServer();
			Assert.IsTrue(DB_NAME.StartsWith("s3a10f_") && DB_NAME.Length<=64,DB_NAME);
			ScratchDb.Exec(null,"DROP DATABASE IF EXISTS `"+DB_NAME+"`");
			ScratchDb.Exec(null,"CREATE DATABASE `"+DB_NAME+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
			ScratchDb.Exec(DB_NAME,"CREATE TABLE preference (PrefName varchar(255) NOT NULL DEFAULT '',ValueString text NOT NULL,PrefNum bigint(20) NOT NULL "
				+"AUTO_INCREMENT,Comments text DEFAULT NULL,PRIMARY KEY (PrefNum),KEY PrefName (PrefName)) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3");
			ScratchDb.Exec(DB_NAME,"CREATE TABLE documentmisc (DocMiscNum bigint(20) NOT NULL AUTO_INCREMENT,DateCreated date NOT NULL DEFAULT '0001-01-01',"
				+"FileName varchar(255) NOT NULL,DocMiscType tinyint(4) NOT NULL,RawBase64 longtext NOT NULL,PRIMARY KEY (DocMiscNum)) "
				+"ENGINE=MyISAM DEFAULT CHARSET=utf8mb3");
			//The two security log tables as db/init/new-database.sql has them.
			ScratchDb.Exec(DB_NAME,"CREATE TABLE securitylog (SecurityLogNum bigint(20) NOT NULL AUTO_INCREMENT,PermType smallint(6) NOT NULL,"
				+"UserNum bigint(20) NOT NULL,LogDateTime datetime NOT NULL DEFAULT '0000-00-00 00:00:00',LogText text DEFAULT NULL,PatNum bigint(20) NOT NULL,"
				+"CompName varchar(255) NOT NULL,FKey bigint(20) NOT NULL,LogSource tinyint(4) NOT NULL,DefNum bigint(20) NOT NULL,"
				+"DefNumError bigint(20) NOT NULL,DateTPrevious datetime NOT NULL DEFAULT '0001-01-01 00:00:00',PRIMARY KEY (SecurityLogNum)) "
				+"ENGINE=MyISAM DEFAULT CHARSET=utf8mb3");
			ScratchDb.Exec(DB_NAME,"CREATE TABLE securityloghash (SecurityLogHashNum bigint(20) NOT NULL AUTO_INCREMENT,SecurityLogNum bigint(20) NOT NULL,"
				+"LogHash varchar(255) NOT NULL,PRIMARY KEY (SecurityLogHashNum)) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3");
			new DataConnection().SetDb("127.0.0.1",DB_NAME,"root","","root","",DatabaseType.MySql,true,DevDbTls.GetSslCa("127.0.0.1"));
		}

		private static string MakeInstallFolder(string name) {
			string folder=TestPaths.NewTempFolder("s3a10f_"+name);
			File.WriteAllText(Path.Combine(folder,"OpenPmsDental.exe"),"synthetic program "+name+" "+Guid.NewGuid());
			File.WriteAllText(Path.Combine(folder,"OpenPmsDentalBusiness.dll"),"synthetic business layer "+name);
			File.WriteAllText(Path.Combine(folder,"UpdateFileCopier.exe"),"synthetic copier");
			return folder;
		}

		private static ReleaseIdentity IdentityOf(string folder,int hour) {
			return new ReleaseIdentity { ReleaseVersion="0.0",BuildId=ForkReleases.ComputeBuildId(folder),
				BuildUtc=new DateTime(2026,11,15,hour,0,0,DateTimeKind.Utc),Runtime=ReleaseRuntime.NetFramework48X86 };
		}

		///<summary>Publishes a synthetic release (the one whose manifest row the tests then damage) and returns its manifest.</summary>
		private static ReleaseManifest PublishOld() {
			string folder=MakeInstallFolder("old");
			return ForkReleases.Publish(folder,IdentityOf(folder,9),"SERVER1",new DateTime(2026,11,15,9,30,0,DateTimeKind.Utc));
		}

		private static void SetManifestRowRaw(string rawBase64) {
			ScratchDb.Exec(DB_NAME,"UPDATE documentmisc SET RawBase64='"+MySqlConnector.MySqlHelper.EscapeString(rawBase64)+"' WHERE DocMiscType=100");
		}

		private static void SetManifestRow(string text) {
			SetManifestRowRaw(Convert.ToBase64String(Encoding.UTF8.GetBytes(text)));
		}

		///<summary>The ways a manifest row can be unreadable (RefuseUnreadable), each applied to the published row.</summary>
		private static Dictionary<string,Action> GetDamages(ReleaseManifest releaseManifest) {
			string text=releaseManifest.Serialize();
			return new Dictionary<string,Action>() {
				{ "not base64",() => SetManifestRowRaw("*** not base64 ***") },
				{ "a newer format",() => SetManifestRow(text.Replace("Format=1","Format=2")) },
				{ "a truncated manifest",() => SetManifestRow(text.Substring(0,text.IndexOf("BuildId="))) },
				{ "another product",() => SetManifestRow(text.Replace("Product="+ReleaseManifest.PRODUCT_NAME,"Product=Some Other Program")) },
			};
		}

		///<summary>Everything the replacement could change: the release rows (number, type, name and a hash of the data), the preferences and the
		///security log.</summary>
		private static string Snapshot() {
			return ScratchDb.Scalar(DB_NAME,"SELECT CONCAT_WS('|',(SELECT GROUP_CONCAT(DocMiscNum,':',DocMiscType,':',FileName,':',MD5(RawBase64) ORDER BY "
				+"DocMiscNum SEPARATOR ';') FROM documentmisc),(SELECT GROUP_CONCAT(PrefName,'=',ValueString ORDER BY PrefNum SEPARATOR ';') FROM preference),"
				+"(SELECT COUNT(*) FROM securitylog),(SELECT COUNT(*) FROM securityloghash))");
		}

		private static string CountLogs() {
			return ScratchDb.Scalar(DB_NAME,"SELECT COUNT(*) FROM securitylog");
		}

		///<summary>Unreadable row, named update server, Yes: the rows hold this build, readable and downloadable, the marker names it, and one
		///security log entry says what was replaced.</summary>
		[TestMethod]
		public void UnreadableRow_NamedUpdateServer_Yes_IsReplaced_AndLogged() {
			CreateDb();
			foreach(KeyValuePair<string,Action> pair in GetDamages(PublishOld())) {
				ScratchDb.Exec(DB_NAME,"DELETE FROM securitylog");
				ScratchDb.Exec(DB_NAME,"DELETE FROM securityloghash");
				pair.Value();
				ForkReleases.GetPublishedManifest(out string errorDamaged);
				Assert.AreNotEqual("",errorDamaged,pair.Key);
				string folderNew=MakeInstallFolder("new");
				ReleaseIdentity releaseIdentityNew=IdentityOf(folderNew,10);
				List<string> listQuestions=new List<string>();
				ReleaseReplaceResult releaseReplaceResult=ForkReleases.ReplaceUnreadableRelease(folderNew,releaseIdentityNew,"SERVER1","SERVER1-COMP",
					UpdateServerRole.ThisComputer,isSilent:false,funcConfirm:x => { listQuestions.Add(x); return true; },
					dateTimeUtc:new DateTime(2026,11,15,10,30,0,DateTimeKind.Utc));
				Assert.AreEqual(ReleaseReplaceOutcome.Replaced,releaseReplaceResult.Outcome,pair.Key+": "+releaseReplaceResult.Message);
				Assert.AreEqual(1,listQuestions.Count,pair.Key+": asked once.");
				string question=listQuestions[0];
				Assert.IsTrue(question.StartsWith(errorDamaged),pair.Key+": the question starts with the reason. "+question);
				StringAssert.Contains(question,"the documentmisc rows of types 100 and 101","It says what is replaced.");
				StringAssert.Contains(question,"publishes this build, "+releaseIdentityNew+", as for a first release");
				StringAssert.Contains(question,"The other workstations then update to this build when they start.");
				StringAssert.Contains(question,"click No");
				//The rows: this build, readable, and its archive downloads and verifies.
				ReleaseManifest releaseManifestPublished=ForkReleases.GetPublishedManifest(out string error);
				Assert.AreEqual("",error,pair.Key);
				Assert.AreEqual(releaseIdentityNew.BuildId,releaseManifestPublished.BuildId,pair.Key);
				Assert.AreEqual(releaseIdentityNew.BuildId,releaseReplaceResult.Manifest.BuildId);
				Assert.AreEqual("1",ScratchDb.Scalar(DB_NAME,"SELECT COUNT(*) FROM documentmisc WHERE DocMiscType=100"),pair.Key);
				Assert.AreEqual("0",ScratchDb.Scalar(DB_NAME,"SELECT COUNT(*) FROM documentmisc WHERE DocMiscType NOT IN (100,101)"),pair.Key);
				string folderPull=Path.Combine(TestPaths.NewTempFolder("s3a10f_pull"),"ForkRelease");
				ReleaseDownloadResult releaseDownloadResult=ForkReleases.DownloadAndVerify(folderPull,IdentityOf(MakeInstallFolder("other"),8));
				Assert.IsTrue(releaseDownloadResult.IsOk,pair.Key+": "+string.Join("; ",releaseDownloadResult.ListProblems));
				Assert.AreEqual(File.ReadAllText(Path.Combine(folderNew,"OpenPmsDental.exe")),File.ReadAllText(Path.Combine(folderPull,"OpenPmsDental.exe")));
				Assert.AreEqual(ForkReleases.FormatProgramVersion(releaseIdentityNew),ScratchDb.Scalar(DB_NAME,
					"SELECT ValueString FROM preference WHERE PrefName='"+ForkVersions.PREF_FORK_PROGRAM_VERSION+"'"),pair.Key);
				//The security log entry.
				Assert.AreEqual("1",CountLogs(),pair.Key);
				Assert.AreEqual(((int)EnumPermType.UpdateInstall).ToString(),ScratchDb.Scalar(DB_NAME,"SELECT PermType FROM securitylog"));
				Assert.AreEqual("SERVER1-COMP",ScratchDb.Scalar(DB_NAME,"SELECT CompName FROM securitylog"));
				string logText=ScratchDb.Scalar(DB_NAME,"SELECT LogText FROM securitylog");
				StringAssert.Contains(logText,"replaced a published release manifest that was not readable",pair.Key);
				StringAssert.Contains(logText,errorDamaged.Replace("\r"," ").Replace("\n"," "),pair.Key);
				StringAssert.Contains(logText,"Release "+releaseIdentityNew+" published as for a first release",pair.Key);
				Assert.AreEqual("1",ScratchDb.Scalar(DB_NAME,"SELECT COUNT(*) FROM securityloghash"),pair.Key);
				//Now readable: Recopy offers nothing more, and the next damage starts from this build's rows.
				Assert.AreEqual(ReleaseReplaceOutcome.NotOffered,ForkReleases.ReplaceUnreadableRelease(folderNew,releaseIdentityNew,"SERVER1","SERVER1",
					UpdateServerRole.ThisComputer,false,x => throw new AssertFailedException("Not asked for a readable manifest.")).Outcome);
			}
		}

		///<summary>Unreadable row, named update server, No: nothing changes, nothing is logged.</summary>
		[TestMethod]
		public void UnreadableRow_NamedUpdateServer_No_ChangesNothing() {
			CreateDb();
			foreach(KeyValuePair<string,Action> pair in GetDamages(PublishOld())) {
				pair.Value();
				string snapshot=Snapshot();
				string folderNew=MakeInstallFolder("new");
				int countAsked=0;
				ReleaseReplaceResult releaseReplaceResult=ForkReleases.ReplaceUnreadableRelease(folderNew,IdentityOf(folderNew,10),"SERVER1","SERVER1",
					UpdateServerRole.ThisComputer,isSilent:false,funcConfirm:x => { countAsked++; return false; });
				Assert.AreEqual(ReleaseReplaceOutcome.Declined,releaseReplaceResult.Outcome,pair.Key);
				Assert.AreEqual(1,countAsked,pair.Key);
				Assert.IsNull(releaseReplaceResult.Manifest);
				Assert.AreEqual(snapshot,Snapshot(),pair.Key+": No changes nothing.");
				Assert.AreEqual("0",CountLogs(),pair.Key);
			}
		}

		///<summary>Another workstation (another computer named, or the blank WebServiceServerName) and a silent run on the named update server are
		///never asked and change nothing: Recopy refuses the manifest as before.</summary>
		[TestMethod]
		public void UnreadableRow_AnotherWorkstationOrSilent_NoOffer_NothingChanges() {
			CreateDb();
			foreach(KeyValuePair<string,Action> pair in GetDamages(PublishOld())) {
				pair.Value();
				string snapshot=Snapshot();
				ReleaseManifest releaseManifestPublished=ForkReleases.GetPublishedManifest(out string errorPublished);
				string folderNew=MakeInstallFolder("new");
				ReleaseIdentity releaseIdentityNew=IdentityOf(folderNew,10);
				Func<string,bool> funcNeverAsked=x => throw new AssertFailedException(pair.Key+": not offered here, yet asked: "+x);
				foreach(UpdateServerRole updateServerRole in _arrayRolesNotNamed) {
					Assert.AreEqual("",ForkReleases.GetReplaceUnreadableQuestion(releaseManifestPublished,errorPublished,releaseIdentityNew,updateServerRole,false));
					Assert.AreEqual("",ForkReleases.GetReplaceUnreadableHint(releaseManifestPublished,errorPublished,updateServerRole));
					ReleaseReplaceResult releaseReplaceResult=ForkReleases.ReplaceUnreadableRelease(folderNew,releaseIdentityNew,"WS1","WS1",updateServerRole,
						isSilent:false,funcConfirm:funcNeverAsked);
					Assert.AreEqual(ReleaseReplaceOutcome.NotOffered,releaseReplaceResult.Outcome,pair.Key+", "+updateServerRole);
					Assert.AreNotEqual("",ForkReleases.GetRefusal(releaseManifestPublished,errorPublished,releaseIdentityNew,updateServerRole),
						pair.Key+", "+updateServerRole+": Recopy still refuses it.");
				}
				Assert.AreEqual(ReleaseReplaceOutcome.NotOffered,ForkReleases.ReplaceUnreadableRelease(folderNew,releaseIdentityNew,"SERVER1","SERVER1",
					UpdateServerRole.ThisComputer,isSilent:true,funcConfirm:funcNeverAsked).Outcome,pair.Key+": never automatic.");
				Assert.AreEqual(snapshot,Snapshot(),pair.Key);
				Assert.AreEqual("0",CountLogs(),pair.Key);
				//The named update server's startup message points to Recopy; Decide still refuses for every role.
				StringAssert.Contains(ForkReleases.GetReplaceUnreadableHint(releaseManifestPublished,errorPublished,UpdateServerRole.ThisComputer),"Recopy");
				Assert.AreEqual(ReleaseAction.RefuseUnreadable,ForkReleases.Decide(releaseManifestPublished,errorPublished,releaseIdentityNew,
					UpdateServerRole.ThisComputer,false).Action);
			}
		}

		///<summary>A readable manifest, and none at all, are not offered: Recopy publishes by its usual rules.</summary>
		[TestMethod]
		public void ReadableOrNothingPublished_NoOffer() {
			CreateDb();
			string folderNew=MakeInstallFolder("new");
			ReleaseIdentity releaseIdentityNew=IdentityOf(folderNew,10);
			Func<string,bool> funcNeverAsked=x => throw new AssertFailedException("Not offered, yet asked: "+x);
			Assert.AreEqual(ReleaseReplaceOutcome.NotOffered,ForkReleases.ReplaceUnreadableRelease(folderNew,releaseIdentityNew,"SERVER1","SERVER1",
				UpdateServerRole.ThisComputer,false,funcNeverAsked).Outcome,"Nothing published.");
			Assert.AreEqual("0",ScratchDb.Scalar(DB_NAME,"SELECT COUNT(*) FROM documentmisc"));
			ReleaseManifest releaseManifestOld=PublishOld();
			string snapshot=Snapshot();
			Assert.AreEqual(ReleaseReplaceOutcome.NotOffered,ForkReleases.ReplaceUnreadableRelease(folderNew,releaseIdentityNew,"SERVER1","SERVER1",
				UpdateServerRole.ThisComputer,false,funcNeverAsked).Outcome,"A readable manifest.");
			Assert.AreEqual(snapshot,Snapshot());
			Assert.AreEqual("",ForkReleases.GetReplaceUnreadableQuestion(releaseManifestOld,"",releaseIdentityNew,UpdateServerRole.ThisComputer,false));
			Assert.AreEqual("",ForkReleases.GetReplaceUnreadableHint(releaseManifestOld,"",UpdateServerRole.ThisComputer));
			Assert.AreEqual("",ForkReleases.GetReplaceUnreadableQuestion(null,"",releaseIdentityNew,UpdateServerRole.ThisComputer,false));
		}

		///<summary>Another computer publishes while the question is shown: the answer Yes replaces nothing, because the manifest it was about is
		///gone.</summary>
		[TestMethod]
		public void PublishedWhileAsked_YesReplacesNothing() {
			CreateDb();
			ReleaseManifest releaseManifestOld=PublishOld();
			SetManifestRow(releaseManifestOld.Serialize().Replace("Format=1","Format=2"));
			string folderOther=MakeInstallFolder("other");
			string folderNew=MakeInstallFolder("new");
			ReleaseReplaceResult releaseReplaceResult=ForkReleases.ReplaceUnreadableRelease(folderNew,IdentityOf(folderNew,10),"SERVER1","SERVER1",
				UpdateServerRole.ThisComputer,false,funcConfirm:x => {
					ForkReleases.Publish(folderOther,IdentityOf(folderOther,11),"SERVER2",new DateTime(2026,11,15,11,30,0,DateTimeKind.Utc));
					return true;
				});
			Assert.AreEqual(ReleaseReplaceOutcome.Changed,releaseReplaceResult.Outcome);
			StringAssert.Contains(releaseReplaceResult.Message,"nothing was replaced");
			Assert.AreEqual(ForkReleases.ComputeBuildId(folderOther),ForkReleases.GetPublishedManifest(out string _).BuildId,"The other computer's release stays.");
			Assert.AreEqual("0",CountLogs());
		}

		///<summary>Yes, but the publish does not finish (the program's wrapper returns null after showing why): no security log entry, Failed.
		///</summary>
		[TestMethod]
		public void Yes_PublishFails_NoLogEntry() {
			CreateDb();
			ReleaseManifest releaseManifestOld=PublishOld();
			SetManifestRowRaw("*** not base64 ***");
			string snapshot=Snapshot();
			string folderNew=MakeInstallFolder("new");
			bool isRun=false;
			ReleaseReplaceResult releaseReplaceResult=ForkReleases.ReplaceUnreadableRelease(folderNew,IdentityOf(folderNew,10),"SERVER1","SERVER1",
				UpdateServerRole.ThisComputer,false,funcConfirm:x => true,funcRunPublish:funcPublish => { isRun=true; return null; });
			Assert.IsTrue(isRun,"The publish runs through the caller's wrapper.");
			Assert.AreEqual(ReleaseReplaceOutcome.Failed,releaseReplaceResult.Outcome);
			Assert.AreEqual(snapshot,Snapshot());
			Assert.AreEqual("0",CountLogs());
		}

		///<summary>Recopy (PrefL.CopyFromHereToUpdateFiles) asks through ReplaceUnreadableRelease before its refusals, with a Yes/No box whose default
		///is No, and runs the publish through PublishRelease (UpdateInProgressOnComputerName and the progress window).</summary>
		[TestMethod]
		public void Recopy_AsksYesNo_DefaultNo_BeforeItRefuses() {
			string body=FileCopierStartTests.GetMethodBody(FileCopierStartTests.StripComments(TestPaths.ReadSource("OpenPmsDental","Logic","PrefL.cs")),
				"public static bool CopyFromHereToUpdateFiles(");
			int indexReplace=body.IndexOf("ForkReleases.ReplaceUnreadableRelease(");
			Assert.IsTrue(indexReplace>=0,body);
			Assert.IsTrue(indexReplace<body.IndexOf("ForkReleases.GetRefusal("),"The offer comes before the refusal.");
			Assert.IsTrue(indexReplace>body.IndexOf("if(!IsUpdateServer())"),"Another computer named never gets that far.");
			StringAssert.Contains(body,"MessageBoxButtons.YesNo,MessageBoxIcon.Warning,MessageBoxDefaultButton.Button2)==DialogResult.Yes");
			StringAssert.Contains(body,"GetUpdateServerRole(),isSilent,");
			StringAssert.Contains(body,"PublishRelease(releaseIdentityRunning,isSilent,\"\",doAsk:false,funcPublish:");
		}
	}
}
