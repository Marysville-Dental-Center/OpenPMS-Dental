// Package ADOPT-LABELS: real version labels (DataBaseVersion 25.3.47.0, ProgramVersion 25.3.54.0, the 25.3.48 schema), as
// pure decisions over the version preferences and a schema comparison. The database tests are in AdoptionDbTests.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;

namespace ReleaseFlowTests {
	[TestClass]
	public class AdoptionVersionLabelsTests {
		private const string PATIENT_REFERENCE="CREATE TABLE `patient` (\n  `PatNum` bigint(20) NOT NULL,\n  `Email` varchar(100) NOT NULL DEFAULT '',\n"
			+"  `PatStatus` tinyint(4) NOT NULL DEFAULT 0,\n  PRIMARY KEY (`PatNum`)\n) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3;\n";
		private const string PATIENT_OFFICE="CREATE TABLE `patient` (\n  `PatNum` bigint(20) NOT NULL,\n  `Email` varchar(100) NOT NULL DEFAULT '',\n"
			+"  `PatStatus` tinyint(4) NOT NULL DEFAULT 0,\n"
			+"  `_portal_active_email` varchar(100) GENERATED ALWAYS AS (case when `PatStatus` = 0 and `Email` <> '' then `Email` else NULL end) VIRTUAL,\n"
			+"  PRIMARY KEY (`PatNum`),\n  UNIQUE KEY `portal_active_email_uniq` (`_portal_active_email`)\n) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3;\n";
		private const string TABLE_DEFINITION="CREATE TABLE `definition` (\n  `DefNum` bigint(20) NOT NULL,\n  `ItemName` varchar(255) NOT NULL DEFAULT '',\n"
			+"  PRIMARY KEY (`DefNum`)\n) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3;\n";

		private static ForkDbState Unadopted(string dataBaseVersion,string programVersion) {
			return new ForkDbState { DataBaseVersion=dataBaseVersion,ProgramVersion=programVersion };
		}

		private static SchemaVerifyResult Compare(string ddlActual) {
			return SchemaVerifier.Compare(SchemaSnapshot.Parse(PATIENT_REFERENCE+TABLE_DEFINITION),SchemaSnapshot.Parse(ddlActual),
				SchemaVerifier.DocumentedExceptions25_3_48);
		}

		[TestMethod]
		public void Labels_TheOfficePairOnly() {
			Assert.AreEqual(1,ForkVersions.ListAdoptableLabels.Count);
			AdoptableVersionLabels labels=ForkVersions.FindAdoptableLabels(new Version(25,3,47,0),new Version(25,3,54,0));
			Assert.IsNotNull(labels);
			Assert.AreEqual(new Version(25,3,48,0),labels.SchemaVersion);
			Assert.IsTrue(ForkVersions.IsAdoptable(labels.SchemaVersion),"The pair's schema version is on the allowlist.");
			Assert.IsNull(ForkVersions.FindAdoptableLabels(new Version(25,3,54,0),new Version(25,3,47,0)),"ProgramVersion on the wrong side.");
			Assert.IsNull(ForkVersions.FindAdoptableLabels(new Version(25,3,47,0),new Version(25,3,53,0)));
			Assert.IsNull(ForkVersions.FindAdoptableLabels(new Version(25,3,46,0),new Version(25,3,54,0)));
			Assert.IsNull(ForkVersions.FindAdoptableLabels(new Version(25,3,47,0),null));
			Assert.IsFalse(ForkVersions.IsAdoptable(new Version(25,3,47,0)),"25.3.47 alone is not on the allowlist.");
			Assert.IsFalse(ForkVersions.IsAdoptable(new Version(25,3,54,0)),"25.3.54 alone is not on the allowlist.");
			//The allowlist itself is unchanged.
			CollectionAssert.AreEquivalent(new[] { new Version(24,3,63,0),new Version(25,3,48,0) },ForkVersions.ListAdoptableVersions.ToList());
			Assert.IsTrue(ForkVersions.HasAdoptableVersions(Unadopted("25.3.47.0","25.3.54.0")));
			Assert.IsTrue(ForkVersions.HasAdoptableVersions(Unadopted("25.3.48","25.3.48.0")));
			Assert.IsFalse(ForkVersions.HasAdoptableVersions(Unadopted("25.3.54.0","25.3.47.0")));
		}

		[TestMethod]
		public void Preflight_OfficePair_WithoutSchemaResult_WaitsForTheCheck() {
			AdoptionPreflight adoptionPreflight=Adoption.Preflight(Unadopted("25.3.47.0","25.3.54.0"));
			Assert.IsFalse(adoptionPreflight.CanAdopt,"Never adoptable before the schema is checked.");
			Assert.IsTrue(adoptionPreflight.IsSchemaCheckPending);
			Assert.IsNotNull(adoptionPreflight.Labels);
			Assert.AreEqual(new Version(25,3,48,0),adoptionPreflight.VersionSource);
		}

		[TestMethod]
		public void Preflight_OfficePair_MatchingSchema_IsAccepted() {
			//The office's own column and key are the one allowed difference.
			SchemaVerifyResult result=Compare(PATIENT_OFFICE+TABLE_DEFINITION);
			Assert.IsTrue(result.IsMatch,result.GetReport());
			Assert.AreEqual(0,Adoption.GetLabelsSchemaProblems(result).Count,result.GetReport());
			AdoptionPreflight adoptionPreflight=Adoption.Preflight(Unadopted("25.3.47.0","25.3.54.0"),result);
			Assert.IsTrue(adoptionPreflight.CanAdopt,adoptionPreflight.Message);
			Assert.IsFalse(adoptionPreflight.IsSchemaCheckPending);
			Assert.IsFalse(adoptionPreflight.IsResume);
			Assert.AreEqual(new Version(25,3,48,0),adoptionPreflight.VersionSource);
			Assert.AreEqual(new Version(25,3,47,0),adoptionPreflight.Labels.DataBaseVersion);
			Assert.AreEqual(new Version(25,3,54,0),adoptionPreflight.Labels.ProgramVersion);
			StringAssert.Contains(adoptionPreflight.Message,"25.3.47.0");
			StringAssert.Contains(adoptionPreflight.Message,"25.3.54.0");
			//Without the office column too.
			Assert.IsTrue(Adoption.Preflight(Unadopted("25.3.47.0","25.3.54.0"),Compare(PATIENT_REFERENCE+TABLE_DEFINITION)).CanAdopt);
		}

		[TestMethod]
		public void Preflight_OfficePair_DriftedSchema_IsRefusedNamingTheDifferences() {
			//A changed column, a missing table and an extra column, each refuses on its own.
			string patientDrifted=PATIENT_OFFICE.Replace("`Email` varchar(100)","`Email` varchar(255)");
			string definitionExtra=TABLE_DEFINITION.Replace("  PRIMARY KEY","  `ItemColor` int(11) NOT NULL DEFAULT 0,\n  PRIMARY KEY");
			foreach((string ddl,string named) in new[] { (patientDrifted+TABLE_DEFINITION,"patient.Email"),(PATIENT_OFFICE,"definition"),
				(PATIENT_OFFICE+definitionExtra,"definition.ItemColor") })
			{
				SchemaVerifyResult result=Compare(ddl);
				AdoptionPreflight adoptionPreflight=Adoption.Preflight(Unadopted("25.3.47.0","25.3.54.0"),result);
				Assert.IsFalse(adoptionPreflight.CanAdopt,named);
				Assert.IsFalse(adoptionPreflight.IsSchemaCheckPending);
				Assert.AreEqual(1,adoptionPreflight.ListSchemaProblems.Count,string.Join("; ",adoptionPreflight.ListSchemaProblems));
				StringAssert.Contains(adoptionPreflight.Message,named);
				StringAssert.Contains(adoptionPreflight.Message,"not adopted");
			}
		}

		[TestMethod]
		public void Preflight_OfficePair_DifferencesSchemaVerifierAllows_StillRefuse() {
			//An HQ-only table and a legacy table pass SchemaVerifier, but an office database with these labels has neither: not an exact match.
			string ddl=PATIENT_OFFICE+TABLE_DEFINITION+"CREATE TABLE `job` (\n  `JobNum` bigint(20) NOT NULL\n) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3;\n"
				+"CREATE TABLE `files` (\n  `Num` bigint(20) NOT NULL\n) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3;\n";
			SchemaVerifyResult result=Compare(ddl);
			Assert.IsTrue(result.IsMatch,"SchemaVerifier allows them: "+result.GetReport());
			AdoptionPreflight adoptionPreflight=Adoption.Preflight(Unadopted("25.3.47.0","25.3.54.0"),result);
			Assert.IsFalse(adoptionPreflight.CanAdopt);
			Assert.AreEqual(2,adoptionPreflight.ListSchemaProblems.Count,string.Join("; ",adoptionPreflight.ListSchemaProblems));
			StringAssert.Contains(adoptionPreflight.Message,"job");
			StringAssert.Contains(adoptionPreflight.Message,"files");
		}

		[TestMethod]
		public void Preflight_OtherPairs_AreRefused_WhateverTheSchema() {
			SchemaVerifyResult resultMatch=Compare(PATIENT_OFFICE+TABLE_DEFINITION);
			foreach((string db,string program) in new[] { ("25.3.54.0","25.3.47.0"),("25.3.47.0","25.3.47.0"),("25.3.54.0","25.3.54.0"),
				("25.3.47.0","25.3.55.0"),("25.3.47.0",null),("25.3.48.0","25.3.54.0"),("25.3.47.0","25.3.48.0") })
			{
				AdoptionPreflight adoptionPreflight=Adoption.Preflight(Unadopted(db,program),resultMatch);
				Assert.IsFalse(adoptionPreflight.CanAdopt,db+" with "+program+": "+adoptionPreflight.Message);
				Assert.IsNull(adoptionPreflight.Labels,db+" with "+program);
			}
		}

		[TestMethod]
		public void Preflight_AllowlistVersions_Unchanged() {
			foreach(string version in new[] { "24.3.63.0","25.3.48.0" }) {
				AdoptionPreflight adoptionPreflight=Adoption.Preflight(Unadopted(version,version));
				Assert.IsTrue(adoptionPreflight.CanAdopt,adoptionPreflight.Message);
				Assert.IsNull(adoptionPreflight.Labels);
				Assert.IsFalse(adoptionPreflight.IsSchemaCheckPending,"The allowlist versions need no schema check, as before.");
				Assert.IsFalse(Adoption.Preflight(Unadopted(version,"25.3.54.0")).CanAdopt,"ProgramVersion must still equal DataBaseVersion.");
			}
		}

		[TestMethod]
		public void Guard_OfficePair_NeedsAdoption_WrongSideRefused() {
			ForkGuardResult forkGuardResult=ForkVersionGuard.Evaluate(Unadopted("25.3.47.0","25.3.54.0"),new BaselineForkMigrationRunner());
			Assert.AreEqual(ForkGuardAction.NeedsAdoption,forkGuardResult.Action,forkGuardResult.Message);
			Assert.AreEqual(ForkVersionGuard.EXIT_CODE_NEEDS_ADOPTION,forkGuardResult.ExitCode);
			StringAssert.Contains(forkGuardResult.Message,"25.3.54");
			forkGuardResult=ForkVersionGuard.Evaluate(Unadopted("25.3.54.0","25.3.47.0"),new BaselineForkMigrationRunner());
			Assert.AreEqual(ForkGuardAction.Refuse,forkGuardResult.Action,forkGuardResult.Message);
		}

		[TestMethod]
		public void AdoptedFrom_RecordsTheRealPair() {
			AdoptableVersionLabels labels=ForkVersions.ListAdoptableLabels[0];
			string adoptedFrom=Adoption.FormatAdoptedFromStart(new Version(25,3,48,0),labels,"2026-11-25T00:00:00Z")+";adopted=2026-11-25T00:10:00Z";
			Assert.AreEqual("source=25.3.48.0;odDataBaseVersion=25.3.47.0;odProgramVersion=25.3.54.0;started=2026-11-25T00:00:00Z;adopted=2026-11-25T00:10:00Z",
				adoptedFrom);
			//source= stays the schema version, which the fork migration steps read.
			Assert.AreEqual(new Version(25,3,48,0),Adoption.ParseAdoptedFromSource(adoptedFrom));
			Assert.AreSame(labels,Adoption.ParseAdoptedFromLabels(adoptedFrom));
			Assert.AreEqual("2026-11-25T00:00:00Z",Adoption.ParseAdoptedFromField(adoptedFrom,"started"));
			//The allowlist versions keep the record they had.
			Assert.AreEqual("source=25.3.48.0;started=x",Adoption.FormatAdoptedFromStart(new Version(25,3,48,0),null,"x"));
			Assert.IsNull(Adoption.ParseAdoptedFromLabels("source=25.3.48.0;started=x"));
			//A resume of an unfinished labels adoption is still a resume.
			ForkDbState forkDbState=Unadopted("99.0.0.0","99.0.0.0");
			forkDbState.ForkAdoptedFrom=Adoption.FormatAdoptedFromStart(new Version(25,3,48,0),labels,"2026-11-25T00:00:00Z");
			AdoptionPreflight adoptionPreflight=Adoption.Preflight(forkDbState);
			Assert.IsTrue(adoptionPreflight.CanAdopt && adoptionPreflight.IsResume,adoptionPreflight.Message);
			Assert.AreEqual(new Version(25,3,48,0),adoptionPreflight.VersionSource);
		}
	}
}
