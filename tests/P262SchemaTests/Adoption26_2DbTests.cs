// Package ADOPT-262: adoption of an Open Dental 26.2 database (any 26.2.x point release) whose schema equals the 26.2.0 reference, the owner's
// decision of 2026-11-26 that Open Dental's published 26.2 database documents are the schema. On scratch databases built from that reference
// with synthetic rows:
// - a database labelled DataBaseVersion 26.2.5.0, ProgramVersion 26.2.9.0 is adopted as 26.2.0 with the real runner (every fork step), the real
//   labels are recorded, its schema still equals the reference (fork tables aside), and no row of an Open Dental table changes except the
//   version preferences and Open Dental's update file rows;
// - a drifted schema or labels off the line are refused, and nothing changes.
// Skipped (inconclusive) when the local MariaDB does not answer. No office data is used.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;

namespace P262SchemaTests {
	[TestClass]
	public class Adoption26_2DbTests {
		private static readonly string DB_OD262=ScratchDb.Name("adopt262");
		private static readonly string DB_REFUSED=ScratchDb.Name("refuse262");
		private static readonly DateTime _dateTimeUtc=new DateTime(2026,11,26,8,0,0,DateTimeKind.Utc);
		private static string _skipReason;
		private static AdoptionPreflight _preflight;
		private static AdoptionResult _adoptionResult;
		private static ForkDbState _forkDbStateAdopted;
		private static Dictionary<string,string> _dictChecksumsBefore;
		private static Dictionary<string,string> _dictChecksumsAfter;
		private static SchemaVerifyResult _resultSchema;

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			_skipReason=ScratchDb.GetSkipReason();
			if(_skipReason!=null) {
				return;
			}
			ScratchDb.DropStale();
			Security.CurUser=Security.CurUser??new Userod { UserNum=1,UserName="Synthetic" };
			CreateOd262(DB_OD262,"26.2.5.0","26.2.9.0");
			_dictChecksumsBefore=ScratchDb.ChecksumAllTables(DB_OD262);
			ScratchDb.Connect(DB_OD262);
			_preflight=Adoption.Preflight();
			_adoptionResult=Adoption.Run(null,null,_dateTimeUtc);
			_forkDbStateAdopted=ForkVersions.ReadState();
			_dictChecksumsAfter=ScratchDb.ChecksumAllTables(DB_OD262);
			_resultSchema=SchemaVerifier.VerifyDatabase(DB_OD262);
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			if(_skipReason!=null) {
				return;
			}
			foreach(string name in new[] { DB_OD262,DB_REFUSED }) {
				ScratchDb.DropQuietly(name);
			}
		}

		[TestInitialize]
		public void TestInitialize() {
			if(_skipReason!=null) {
				Assert.Inconclusive(_skipReason);
			}
		}

		///<summary>The 26.2.0 reference schema, the version preferences, and synthetic rows with values in the columns and tables 26.2 added.</summary>
		private static void CreateOd262(string dbName,string dataBaseVersion,string programVersion,string sqlDrift=null) {
			ScratchDb.CreateFromSnapshot(dbName,SchemaVerifier.GetReference26_2_0());
			ScratchDb.Exec(dbName,
				"INSERT INTO preference (PrefName,ValueString) VALUES ('DataBaseVersion','"+dataBaseVersion+"'),('ProgramVersion','"+programVersion+"'),"
					+"('CorruptedDatabase','0'),('UpdateInProgressOnComputerName',''),('RandomPrimaryKeys','0'),('DxIcdVersion','10')",
				"INSERT INTO patient (LName,FName,Email,PatStatus) VALUES ('Example','Ava','ava@example.invalid',0)",
				"INSERT INTO provider (Abbr,FeeSched,Specialty,SchoolClassNum,AnesthProvType,TaxonomyCodeOverride,EcwID,StateRxID,StateWhereLicensed,"
					+"EmailAddressNum,ProvNumBillingOverride,CustomID,SchedNote,WebSchedDescript,WebSchedImageLocation,PreferredName,ClaimFormNum,"
					+"GlidewellAcctNum,DateTStamp) VALUES ('SYN1',1,0,0,0,'','','','',0,0,'','','','','',12,1,'2026-03-04 05:06:07')",
				"INSERT INTO apisubscription (EndPointUrl,Workstation,CustomerKey,WatchTable,PollingSeconds,UiEventType,Note,DateTimeLastFailure,"
					+"FailureReason,SubsequentFailures,DateTimeNextRetry) VALUES ('https://example.invalid/hook','','SYNKEY','Patient',60,'','',"
					+"'2026-03-01 10:00:00','Synthetic timeout',3,'2026-03-01 10:30:00')",
				"INSERT INTO glidewellacct (AccountNumber,ApiKey,DateTimeExpires) VALUES ('00-0000001','synthetic-encrypted-key','2026-04-01 00:00:00')",
				"INSERT INTO glidewellmsg (LabCaseNum,OrderStatus,VendorRxId,PatNum,GlidewellAcctNum,RawJsonResponse,RawJsonRequest,SentITero,SentMedit) "
					+"VALUES (1,4,'SYN-RX-1',1,1,'{\"status\":\"Shipped\"}','{\"OrderId\":1}',1,0)",
				"INSERT INTO labcaseproduct (LabCaseNum,PatNum,GlidewellProduct,Material,Shade,ToothRange,Notes) VALUES (1,1,3,'Zirconia','A1','8-9','Synthetic')"
			);
			if(sqlDrift!=null) {
				ScratchDb.Exec(dbName,sqlDrift);
			}
		}

		[TestMethod]
		public void Od262_PreflightChecksTheSchemaAndAccepts() {
			Assert.IsTrue(_preflight.CanAdopt,_preflight.Message);
			Assert.IsFalse(_preflight.IsSchemaCheckPending);
			Assert.AreEqual(0,_preflight.ListSchemaProblems.Count,string.Join("; ",_preflight.ListSchemaProblems));
			Assert.AreEqual(new Version(26,2,0,0),_preflight.VersionSource);
			Assert.AreEqual(new Version(26,2,5,0),_preflight.Labels.DataBaseVersion);
			Assert.AreEqual(new Version(26,2,9,0),_preflight.Labels.ProgramVersion);
		}

		[TestMethod]
		public void Od262_IsAdoptedAs26_2_0_WithTheRealLabelsRecorded() {
			Assert.AreEqual(new Version(26,2,0,0),_adoptionResult.VersionSource);
			Assert.AreEqual("source=26.2.0.0;odDataBaseVersion=26.2.5.0;odProgramVersion=26.2.9.0;started=2026-11-26T08:00:00Z;adopted=2026-11-26T08:00:00Z",
				_forkDbStateAdopted.ForkAdoptedFrom);
			Assert.AreEqual("26.2.0",_forkDbStateAdopted.OdSchemaCompat);
			Assert.AreEqual("99.0.0.0",_forkDbStateAdopted.DataBaseVersion);
			Assert.AreEqual("99.0.0.0",_forkDbStateAdopted.ProgramVersion);
			Assert.AreEqual(ForkMigrationRunner.GetSteps().Max(x => x.Level).ToString(),_forkDbStateAdopted.ForkSchemaLevel);
			Assert.AreEqual(ForkGuardAction.ProceedFork,ForkVersionGuard.Evaluate(_forkDbStateAdopted,ForkMigrationHandOff.Runner).Action);
		}

		[TestMethod]
		public void Od262_SchemaStillEqualsTheReference_ForkTablesAside() {
			Assert.IsTrue(_resultSchema.IsMatch,_resultSchema.GetReport());
			Assert.IsTrue(_resultSchema.ListDifferences.All(x => x.Kind==SchemaDifferenceKind.ExtraTable && ForkTables.IsForkTable(x.Table)),
				_resultSchema.GetReport());
			Assert.AreEqual(0,ForkTables.VerifyDatabase(DB_OD262).ListDifferences.Count);
		}

		[TestMethod]
		public void Od262_NoOpenDentalRowChanges_ButTheVersionPreferences() {
			//preference: DataBaseVersion and ProgramVersion get the sentinel, the fork markers and the steps' own preference rows are added.
			//securitylog and securityloghash: the adoption's entry and its hash. Every other Open Dental table, the 26.2 columns and tables included, keeps every row as it was.
			HashSet<string> setExpectedChanges=new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "preference","securitylog","securityloghash" };
			List<string> listChanged=_dictChecksumsBefore.Keys
				.Where(x => !setExpectedChanges.Contains(x) && _dictChecksumsAfter.TryGetValue(x,out string after) && after!=_dictChecksumsBefore[x])
				.OrderBy(x => x).ToList();
			Assert.AreEqual(0,listChanged.Count,"Changed: "+string.Join(", ",listChanged)+"\n"+string.Join("\n",_adoptionResult.ListLog));
			Assert.AreEqual("12",ScratchDb.Scalar(DB_OD262,"SELECT ClaimFormNum FROM provider WHERE Abbr='SYN1'"));
			Assert.AreEqual("SYN-RX-1",ScratchDb.Scalar(DB_OD262,"SELECT VendorRxId FROM glidewellmsg"));
		}

		[TestMethod]
		public void Od262_DriftedSchema_IsRefusedAndNothingChanges() {
			foreach((string sqlDrift,string named) in new[] {
				("ALTER TABLE provider ADD COLUMN `ExtraDrift` int(11) NOT NULL DEFAULT 0","provider.ExtraDrift"),
				("DROP TABLE glidewellmsg","glidewellmsg"),
				("ALTER TABLE apisubscription MODIFY `FailureReason` varchar(255) NOT NULL DEFAULT ''","apisubscription.FailureReason") })
			{
				CreateOd262(DB_REFUSED,"26.2.3.0","26.2.3.0",sqlDrift);
				ScratchDb.Connect(DB_REFUSED);
				AdoptionPreflight adoptionPreflight=Adoption.Preflight();
				Assert.IsFalse(adoptionPreflight.CanAdopt,named+": "+adoptionPreflight.Message);
				StringAssert.Contains(adoptionPreflight.Message,named);
				Assert.ThrowsExactly<ApplicationException>(() => Adoption.Run(null,new BaselineForkMigrationRunner(),_dateTimeUtc));
				Assert.IsNull(ForkVersions.GetMarker(ForkVersions.PREF_FORK_ADOPTED_FROM),named);
				Assert.AreEqual("26.2.3.0",ForkVersions.GetMarker("DataBaseVersion"),named);
			}
		}

		[TestMethod]
		public void Od262_LabelsOffTheLine_AreRefused() {
			foreach((string db,string program) in new[] { ("26.2.9.0","26.2.5.0"),("26.3.0.0","26.3.0.0"),("26.2.5.0","26.3.1.0"),("26.1.70.0","26.2.0.0") }) {
				CreateOd262(DB_REFUSED,db,program);
				ScratchDb.Connect(DB_REFUSED);
				AdoptionPreflight adoptionPreflight=Adoption.Preflight();
				Assert.IsFalse(adoptionPreflight.CanAdopt,db+" with "+program+": "+adoptionPreflight.Message);
				Assert.ThrowsExactly<ApplicationException>(() => Adoption.Run(null,new BaselineForkMigrationRunner(),_dateTimeUtc));
				Assert.IsNull(ForkVersions.GetMarker(ForkVersions.PREF_FORK_ADOPTED_FROM),db+" with "+program);
			}
		}
	}
}
