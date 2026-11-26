// Package ADOPT-262: an Open Dental 26.2 database (any 26.2.x point release) is adoptable as the 26.2.0 schema when its schema equals that
// reference, the owner's decision of 2026-11-26 that Open Dental's published 26.2 database documents are the schema. Pure decisions over the
// version preferences and a schema comparison; the database tests are in P262SchemaTests.Adoption26_2DbTests.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;

namespace ReleaseFlowTests {
	[TestClass]
	public class AdoptionLine26_2Tests {
		private const string TABLE_PROVIDER="CREATE TABLE `provider` (\n  `ProvNum` bigint(20) NOT NULL,\n  `ClaimFormNum` bigint(20) NOT NULL,\n"
			+"  PRIMARY KEY (`ProvNum`)\n) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3;\n";

		private static ForkDbState Unadopted(string dataBaseVersion,string programVersion) {
			return new ForkDbState { DataBaseVersion=dataBaseVersion,ProgramVersion=programVersion };
		}

		private static SchemaVerifyResult Compare(string ddlActual) {
			return SchemaVerifier.Compare(SchemaSnapshot.Parse(TABLE_PROVIDER),SchemaSnapshot.Parse(ddlActual),SchemaVerifier.DocumentedExceptions26_2_0);
		}

		[TestMethod]
		public void Line_26_2_AnyPointRelease_ProgramNotBelowDatabase() {
			Assert.AreEqual(1,ForkVersions.ListAdoptableLines.Count);
			foreach((Version db,Version program) in new[] { (new Version(26,2,0,0),new Version(26,2,0,0)),(new Version(26,2,5,0),new Version(26,2,9,0)),
				(new Version(26,2,14,0),new Version(26,2,14,0)) })
			{
				AdoptableVersionLabels labels=ForkVersions.FindAdoptableLabels(db,program);
				Assert.IsNotNull(labels,db+" with "+program);
				Assert.AreEqual(new Version(26,2,0,0),labels.SchemaVersion);
				Assert.AreEqual(db,labels.DataBaseVersion);
				Assert.AreEqual(program,labels.ProgramVersion);
				Assert.IsTrue(ForkVersions.HasAdoptableVersions(Unadopted(db.ToString(4),program.ToString(4))));
			}
			foreach((Version db,Version program) in new[] { (new Version(26,2,9,0),new Version(26,2,5,0)),(new Version(26,3,0,0),new Version(26,3,0,0)),
				(new Version(26,2,5,0),new Version(26,3,1,0)),(new Version(26,1,70,0),new Version(26,2,0,0)),(new Version(26,2,1,0),null) })
			{
				Assert.IsNull(ForkVersions.FindAdoptableLabels(db,program),db+" with "+program);
			}
			//The fixed allowlist and the office pair are unchanged; 26.2.0 is a source of adoption only through the line, after the schema check.
			CollectionAssert.AreEquivalent(new[] { new Version(24,3,63,0),new Version(25,3,48,0) },ForkVersions.ListAdoptableVersions.ToList());
			Assert.IsFalse(ForkVersions.IsAdoptable(new Version(26,2,0,0)));
			Assert.IsTrue(ForkVersions.IsAdoptableSource(new Version(26,2,0,0)));
			Assert.IsFalse(ForkVersions.IsAdoptableSource(new Version(26,2,5,0)),"source= is always the schema version.");
		}

		[TestMethod]
		public void Preflight_26_2_WaitsForTheSchemaCheck_ThenAcceptsAnExactMatch() {
			AdoptionPreflight adoptionPreflight=Adoption.Preflight(Unadopted("26.2.5.0","26.2.9.0"));
			Assert.IsFalse(adoptionPreflight.CanAdopt,"Never adoptable before the schema is checked.");
			Assert.IsTrue(adoptionPreflight.IsSchemaCheckPending);
			adoptionPreflight=Adoption.Preflight(Unadopted("26.2.5.0","26.2.9.0"),Compare(TABLE_PROVIDER));
			Assert.IsTrue(adoptionPreflight.CanAdopt,adoptionPreflight.Message);
			Assert.AreEqual(new Version(26,2,0,0),adoptionPreflight.VersionSource);
			StringAssert.Contains(adoptionPreflight.Message,"26.2.5.0");
			StringAssert.Contains(adoptionPreflight.Message,"26.2.0");
		}

		[TestMethod]
		public void Preflight_26_2_DriftedSchema_IsRefused() {
			string ddlDrifted=TABLE_PROVIDER.Replace("  PRIMARY KEY","  `ExtraDrift` int(11) NOT NULL,\n  PRIMARY KEY");
			AdoptionPreflight adoptionPreflight=Adoption.Preflight(Unadopted("26.2.5.0","26.2.5.0"),Compare(ddlDrifted));
			Assert.IsFalse(adoptionPreflight.CanAdopt);
			StringAssert.Contains(adoptionPreflight.Message,"provider.ExtraDrift");
		}

		[TestMethod]
		public void AdoptedFrom_26_2_RecordsTheRealLabels_AndResumes() {
			AdoptableVersionLabels labels=ForkVersions.FindAdoptableLabels(new Version(26,2,5,0),new Version(26,2,9,0));
			string adoptedFrom=Adoption.FormatAdoptedFromStart(labels.SchemaVersion,labels,"2026-11-26T00:00:00Z");
			Assert.AreEqual("source=26.2.0.0;odDataBaseVersion=26.2.5.0;odProgramVersion=26.2.9.0;started=2026-11-26T00:00:00Z",adoptedFrom);
			AdoptableVersionLabels labelsParsed=Adoption.ParseAdoptedFromLabels(adoptedFrom);
			Assert.AreEqual(new Version(26,2,5,0),labelsParsed.DataBaseVersion);
			Assert.AreEqual(new Version(26,2,0,0),labelsParsed.SchemaVersion);
			ForkDbState forkDbState=Unadopted("99.0.0.0","99.0.0.0");
			forkDbState.ForkAdoptedFrom=adoptedFrom;
			AdoptionPreflight adoptionPreflight=Adoption.Preflight(forkDbState);
			Assert.IsTrue(adoptionPreflight.CanAdopt && adoptionPreflight.IsResume,adoptionPreflight.Message);
			Assert.AreEqual(new Version(26,2,0,0),adoptionPreflight.VersionSource);
		}

		[TestMethod]
		public void Guard_26_2_NeedsAdoption_OtherVersionsNameTheLine() {
			ForkGuardResult forkGuardResult=ForkVersionGuard.Evaluate(Unadopted("26.2.5.0","26.2.9.0"),new BaselineForkMigrationRunner());
			Assert.AreEqual(ForkGuardAction.NeedsAdoption,forkGuardResult.Action,forkGuardResult.Message);
			StringAssert.Contains(forkGuardResult.Message,"26.2.0");
			forkGuardResult=ForkVersionGuard.Evaluate(Unadopted("26.3.0.0","26.3.0.0"),new BaselineForkMigrationRunner());
			Assert.AreEqual(ForkGuardAction.Refuse,forkGuardResult.Action,forkGuardResult.Message);
			StringAssert.Contains(forkGuardResult.Message,"26.2.x");
		}
	}
}
