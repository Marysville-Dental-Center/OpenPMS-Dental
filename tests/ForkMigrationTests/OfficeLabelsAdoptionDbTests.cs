// Package ADOPT-LABELS: adoption of a database with the office's real version labels (DataBaseVersion 25.3.47.0, ProgramVersion 25.3.54.0) on the
// 25.3.48 reference schema plus the office's patient._portal_active_email: accepted on an exact match, refused on a drifted schema or other labels.
// No office data is used. Skipped (inconclusive) when the local MariaDB does not answer.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;

namespace ForkMigrationTests {
	[TestClass]
	public class OfficeLabelsAdoptionDbTests {
		private static readonly string DB_NAME=TestDb.ScratchName("odlabels");
		private static readonly DateTime _dateTimeUtc=new DateTime(2026,11,25,8,0,0,DateTimeKind.Utc);
		private static string _skipReason;

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			_skipReason=TestDb.GetSkipReason();
			if(_skipReason==null) {
				TestDb.DropStaleScratchDatabases();
			}
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			if(_skipReason==null) {
				TestDb.Drop(DB_NAME);
			}
		}

		///<summary>The 25.3.48 reference schema, the office column and key, the version preferences, and a patient.</summary>
		private static void CreateOfficeCopy(string dataBaseVersion,string programVersion,string sqlDrift=null) {
			if(_skipReason!=null) {
				Assert.Inconclusive(_skipReason);
			}
			TestDb.Recreate(DB_NAME);
			using(MySqlConnection connection=TestDb.OpenServer()) {
				TestDb.Exec(connection,"USE `"+DB_NAME+"`");
				foreach(SchemaTable table in SchemaVerifier.GetReference25_3_48().Tables) {
					TestDb.Exec(connection,table.ToDdl());
				}
				TestDb.Exec(connection,"ALTER TABLE patient ADD COLUMN `_portal_active_email` varchar(100) GENERATED ALWAYS AS (case when `PatStatus` = 0 "
					+"and `Email` <> '' then `Email` else NULL end) VIRTUAL, ADD UNIQUE KEY `portal_active_email_uniq` (`_portal_active_email`)");
				TestDb.Exec(connection,"INSERT INTO preference (PrefName,ValueString,Comments) VALUES ('DataBaseVersion','"+dataBaseVersion+"',''),"
					+"('ProgramVersion','"+programVersion+"',''),('CorruptedDatabase','0',''),('UpdateInProgressOnComputerName','','')");
				TestDb.Exec(connection,"INSERT INTO patient (LName,FName,PatStatus,Email) VALUES ('Synthetic','Parent',0,'family@example.test')");
				if(sqlDrift!=null) {
					TestDb.Exec(connection,sqlDrift);
				}
			}
			TestDb.Connect(DB_NAME);
		}

		private static string GetPref(string prefName) {
			return ForkVersions.GetMarker(prefName);
		}

		[TestMethod]
		public void OfficeLabels_MatchingSchema_AreAdoptedAs25_3_48_AndRecorded() {
			CreateOfficeCopy("25.3.47.0","25.3.54.0");
			Assert.AreEqual(ForkGuardAction.NeedsAdoption,ForkVersionGuard.Evaluate(ForkVersions.ReadState(),new BaselineForkMigrationRunner()).Action);
			AdoptionPreflight adoptionPreflight=Adoption.Preflight();
			Assert.IsTrue(adoptionPreflight.CanAdopt,adoptionPreflight.Message);
			Assert.IsFalse(adoptionPreflight.IsSchemaCheckPending);
			Assert.AreEqual(new Version(25,3,48,0),adoptionPreflight.VersionSource);
			Assert.AreEqual(0,adoptionPreflight.ListSchemaProblems.Count);
			AdoptionResult adoptionResult=Adoption.Run(null,new BaselineForkMigrationRunner(),_dateTimeUtc);
			Assert.AreEqual(new Version(25,3,48,0),adoptionResult.VersionSource);
			Assert.IsNotNull(adoptionResult.Labels);
			Assert.IsTrue(adoptionResult.WasPortalEmailColumnDropped);
			Assert.AreEqual("source=25.3.48.0;odDataBaseVersion=25.3.47.0;odProgramVersion=25.3.54.0;started=2026-11-25T08:00:00Z;adopted=2026-11-25T08:00:00Z",
				GetPref(ForkVersions.PREF_FORK_ADOPTED_FROM));
			Assert.IsTrue(adoptionResult.ListLog.Any(x => x.Contains("DataBaseVersion 25.3.47.0") && x.Contains("ProgramVersion 25.3.54.0")),
				string.Join("\n",adoptionResult.ListLog));
			Assert.AreEqual("25.3.48",GetPref(ForkVersions.PREF_OD_SCHEMA_COMPAT));
			Assert.AreEqual("99.0.0.0",GetPref("DataBaseVersion"));
			Assert.AreEqual("99.0.0.0",GetPref("ProgramVersion"));
			Assert.AreEqual(ForkGuardAction.ProceedFork,ForkVersionGuard.Evaluate(ForkVersions.ReadState(),new BaselineForkMigrationRunner()).Action);
			Assert.IsTrue(Adoption.Preflight().IsAlreadyAdopted);
		}

		[TestMethod]
		public void OfficeLabels_DriftedSchema_AreRefusedAndNothingChanges() {
			foreach((string sqlDrift,string named) in new[] {
				("ALTER TABLE definition ADD COLUMN `ExtraDrift` int(11) NOT NULL DEFAULT 0","definition.ExtraDrift"),
				("ALTER TABLE patient MODIFY `Email` varchar(255) NOT NULL DEFAULT ''","patient.Email"),
				("CREATE TABLE `job` (`JobNum` bigint(20) NOT NULL) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3","job"),
				("DROP TABLE apikey","apikey") })
			{
				CreateOfficeCopy("25.3.47.0","25.3.54.0",sqlDrift);
				AdoptionPreflight adoptionPreflight=Adoption.Preflight();
				Assert.IsFalse(adoptionPreflight.CanAdopt,named+": "+adoptionPreflight.Message);
				StringAssert.Contains(adoptionPreflight.Message,named);
				Assert.IsTrue(adoptionPreflight.ListSchemaProblems.Count>=1);
				ApplicationException ex=Assert.ThrowsExactly<ApplicationException>(() => Adoption.Run(null,new BaselineForkMigrationRunner(),_dateTimeUtc));
				StringAssert.Contains(ex.Message,named);
				Assert.IsNull(GetPref(ForkVersions.PREF_FORK_ADOPTED_FROM),named);
				Assert.AreEqual("25.3.47.0",GetPref("DataBaseVersion"),named);
				Assert.AreEqual("25.3.54.0",GetPref("ProgramVersion"),named);
			}
		}

		[TestMethod]
		public void OtherLabels_OnTheSameSchema_AreRefused() {
			foreach((string db,string program) in new[] { ("25.3.54.0","25.3.47.0"),("25.3.47.0","25.3.47.0"),("25.3.54.0","25.3.54.0"),("25.3.48.0","25.3.54.0") }) {
				CreateOfficeCopy(db,program);
				AdoptionPreflight adoptionPreflight=Adoption.Preflight();
				Assert.IsFalse(adoptionPreflight.CanAdopt,db+" with "+program+": "+adoptionPreflight.Message);
				Assert.IsNull(adoptionPreflight.Labels);
				Assert.ThrowsExactly<ApplicationException>(() => Adoption.Run(null,new BaselineForkMigrationRunner(),_dateTimeUtc));
				Assert.IsNull(GetPref(ForkVersions.PREF_FORK_ADOPTED_FROM));
			}
		}
	}
}
