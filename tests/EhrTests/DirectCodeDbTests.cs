// The import of the release's three files on a database (docs/specs/ehr-2027-measures-plan.md, EHR-6a, sections 7.1, 7.3 and 7.4): the pick
// writes the value-set codes and the direct reference codes in one swap and records the three files, the code-system versions and the roles;
// a second pick replaces both kinds together and the roles resolve again; an import that fails part way, or a pick without all three files,
// leaves every old row of both kinds, and so does a pick MakePick did not build; a failure while the release is recorded says the codes were
// replaced; the record keeps both files' measure checks; the code-system status counts the direct reference codes apart from the value-set
// codes and flags a loaded table older than the release; and a Middle Tier client's list
// has the direct reference codes after InvalidType.EhrCodes. On this run's ehr6a_ scratch database (Support/ReleaseDb.cs); made-up TEST codes.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace EhrTests {
	[TestClass]
	public class DirectCodeDbTests {
		private const string PASSWORD="TestPassword1!";

		[TestInitialize]
		public void Begin() {
			ReleaseDb.Begin();
			foreach(string table in new[] { "ehrcode","snomed","loinc" }) {
				DataCore.NonQ("DELETE FROM "+table);
			}
			DataCore.NonQ("DELETE FROM preference WHERE PrefName='"+EhrValueSetImport.PREF_RELEASE+"'");
			DataCore.NonQ("UPDATE codesystem SET VersionCur='' WHERE CodeSystemName IN ('SNOMEDCT','LOINC')");
			ValueSetDb.RefreshCaches();
			EhrCodes.ClearList();
		}

		[TestCleanup]
		public void End() {
			ReleaseDb.End();
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			ReleaseDb.Cleanup();
		}

		private static EhrValueSetPick PickOrFail(List<KeyValuePair<string,byte[]>> listFiles) {
			EhrValueSetPick pick=ValueSetFixtures.ParsePick(listFiles);
			Assert.IsFalse(pick.IsRefused,EhrValueSetImport.GetReport(pick));
			return pick;
		}

		private static List<EhrCode> ReadRows() {
			return EhrCodes.GetAllFromDb();
		}

		private static byte[] MakeValueSets(params TestValueSetRow[] arrayRows) {
			ValueSetFixtures.Sheet sheet=new ValueSetFixtures.Sheet();
			sheet.Name="CMS138v15";
			sheet.ListRows=arrayRows.ToList();
			return ValueSetFixtures.MakeWorkbook(sheet);
		}

		[TestMethod]
		public void Import_ThePickOfTheThreeFiles_WritesBothKindsInOneSwap_AndRecordsTheFilesVersionsAndRoles() {
			long securityLogNum=ScratchDb.GetLastSecurityLogNum();
			List<KeyValuePair<string,byte[]>> listFiles=ValueSetFixtures.MakeRelease();
			EhrValueSetPick pick=PickOrFail(listFiles);
			Assert.AreEqual("",EhrValueSetImport.Import(pick));
			List<EhrCode> listRows=ReadRows();
			Assert.AreEqual(pick.ListEhrCodesAll.Count,listRows.Count);
			Assert.AreEqual(EhrValueSetImport.GetManifest().Count,listRows.Count(x => !CqmCatalog.IsDirectCodeKey(x.ValueSetOID)),"One code in each value set.");
			Assert.AreEqual(ValueSetFixtures.MakeDirectCodeRows().Count,listRows.Count(x => CqmCatalog.IsDirectCodeKey(x.ValueSetOID)));
			//Each role's rows are found by its key, as the engine will find them (GetForValueSetOIDs without IsInDb, section 5.2).
			foreach(CqmDirectCodeRole role in CqmCatalog.GetDirectCodeRoles()) {
				List<EhrCode> listRole=EhrCodes.GetForValueSetOIDs(new List<string> { CqmCatalog.GetDirectCodeKey(role.CmsId,role.Role) },false);
				Assert.AreEqual(role.CountExpected,listRole.Count,role.Role);
				Assert.IsTrue(listRole.All(x => x.QDMCategory==CqmCatalog.DIRECT_CODE_CATEGORY && x.CodeSystem==role.CodeSystem),role.Role);
			}
			Assert.AreEqual("Systolic blood pressure",EhrCodes.GetForValueSetOIDs(new List<string> { "drc:CMS165:Cms165Systolic" },false).Single().Description);
			//The release: the three files, the code-system versions and the roles.
			EhrValueSetRelease release=EhrValueSetImport.GetRelease();
			Assert.AreEqual(CqmCatalog.RELEASE_LABEL,release.Label);
			Assert.AreEqual("7 of 7 measures at the 2027 versions",release.MeasureCheck);
			Assert.AreEqual("7 of 7 measures at the 2027 versions",release.MeasureCheckDirectCodes,"The Direct Reference Codes file's own check.");
			CollectionAssert.AreEqual(new[] { EhrValueSetFileKind.ValueSets,EhrValueSetFileKind.DirectReferenceCodes,EhrValueSetFileKind.CodeSystemVersions },
				release.ListFiles.Select(x => x.Kind).ToList());
			CollectionAssert.AreEqual(listFiles.Select(x => x.Key).ToList(),release.ListFiles.Select(x => x.FileName).ToList());
			CollectionAssert.AreEqual(pick.ListFiles.Select(x => x.Sha256).ToList(),release.ListFiles.Select(x => x.Sha256).ToList());
			Assert.IsTrue(release.ListFiles.All(x => x.Sha256.Length==64 && x.CountBytes>0));
			Assert.AreEqual(listFiles[0].Key,release.FileName);
			Assert.AreEqual(ValueSetFixtures.MakeCodeSystemVersionRows().Count,release.ListCodeSystemVersions.Count);
			Assert.AreEqual("2.80",release.ListCodeSystemVersions.Single(x => x.CodeSystem=="LOINC").Version);
			Assert.AreEqual(8,release.ListRoles.Count(x => x.IsResolved));
			Assert.AreEqual(pick.ListEhrCodesDirect.Count,release.CountDirectCodes);
			Assert.AreEqual(listRows.Count,release.CountCodes);
			//One Setup entry naming the three files and their SHA-256; no code.
			SecurityLog securityLog=ScratchDb.GetSecurityLogsAfter(securityLogNum).Single(x => x.LogText.StartsWith("EHR value sets imported"));
			Assert.AreEqual(EnumPermType.Setup,securityLog.PermType);
			foreach(EhrValueSetReleaseFile releaseFile in release.ListFiles) {
				StringAssert.Contains(securityLog.LogText,releaseFile.Kind+" '"+releaseFile.FileName+"' SHA-256 "+releaseFile.Sha256);
			}
			StringAssert.Contains(securityLog.LogText,"8 of 8 direct-reference-code roles resolved");
			StringAssert.Contains(securityLog.LogText,"value sets the measures use; 7 of 7 measures at the 2027 versions; direct reference codes: 7 of 7 measures "
				+"at the 2027 versions;");
			Assert.IsFalse(securityLog.LogText.Contains("TEST"+"D0"),"No code is logged.");
			Assert.IsFalse(securityLog.LogText.Contains("TESTM"),"No code is logged.");
		}

		[TestMethod]
		public void Import_ASecondPick_ReplacesBothKindsTogether_AndTheRolesStillResolve() {
			Assert.AreEqual("",EhrValueSetImport.Import(PickOrFail(ValueSetFixtures.MakeRelease())));
			List<KeyValuePair<string,byte[]>> listSecond=ValueSetFixtures.MakeRelease(
				arrayValueSets:MakeValueSets(TestValueSetRow.Make("2.16.840.1.113883.3.526.3.1278","SNOMEDCT","TESTN0001","CMS138v15")),
				listDirectCodes:ValueSetFixtures.MakeDirectCodeRows("TESTE"));
			Assert.AreEqual("",EhrValueSetImport.Import(PickOrFail(listSecond)));
			List<EhrCode> listRows=ReadRows();
			CollectionAssert.AreEqual(new[] { "TESTN0001" },listRows.Where(x => !CqmCatalog.IsDirectCodeKey(x.ValueSetOID)).Select(x => x.CodeValue).ToList(),
				"Every earlier value-set code replaced.");
			Assert.IsTrue(listRows.Where(x => CqmCatalog.IsDirectCodeKey(x.ValueSetOID)).All(x => x.CodeValue.StartsWith("TESTE")),
				"Every earlier direct reference code replaced: the two kinds never come from different releases.");
			Assert.AreEqual(1,EhrCodes.GetForValueSetOIDs(new List<string> { "drc:CMS69:Cms69Bmi" },false).Count);
			Assert.AreEqual(4,EhrCodes.GetForValueSetOIDs(new List<string> { "drc:CMS138:Cms138QualifyingVisitCpt" },false).Count);
			Assert.AreEqual(8,EhrValueSetImport.GetRelease().ListRoles.Count(x => x.IsResolved));
		}

		[TestMethod]
		public void Import_AFailurePartWay_OrAPickWithoutAllThree_LeavesEveryOldRowOfBothKinds() {
			Assert.AreEqual("",EhrValueSetImport.Import(PickOrFail(ValueSetFixtures.MakeRelease())));
			List<string> listBefore=ReadRows().Select(x => x.ValueSetOID+" "+x.CodeValue).OrderBy(x => x,StringComparer.Ordinal).ToList();
			EhrValueSetRelease releaseBefore=EhrValueSetImport.GetRelease();
			//The rows are written, then the server fails.
			EhrValueSetPick pickSecond=PickOrFail(ValueSetFixtures.MakeRelease(listDirectCodes:ValueSetFixtures.MakeDirectCodeRows("TESTE")));
			EhrValueSetImport.ActionAfterInsertBatch=countWritten => throw new ApplicationException("TEST failure after "+countWritten+" rows were written");
			string error;
			try {
				error=EhrValueSetImport.Import(pickSecond);
			}
			finally {
				EhrValueSetImport.ActionAfterInsertBatch=null;
			}
			StringAssert.Contains(error,"TEST failure");
			StringAssert.Contains(error,"unchanged");
			CollectionAssert.AreEqual(listBefore,ReadRows().Select(x => x.ValueSetOID+" "+x.CodeValue).OrderBy(x => x,StringComparer.Ordinal).ToList());
			Assert.AreEqual(releaseBefore.DateImportedUtc+releaseBefore.Sha256,EhrValueSetImport.GetRelease().DateImportedUtc+EhrValueSetImport.GetRelease().Sha256);
			Assert.AreEqual(0,PIn.Long(DataCore.GetScalar(@"SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME LIKE 'ehrcode\_%'")),
				"No scratch table is left.");
			//A pick of two files: refused before anything changes.
			List<KeyValuePair<string,byte[]>> listTwo=ValueSetFixtures.MakeRelease().Take(2).ToList();
			EhrValueSetPick pickTwo=ValueSetFixtures.ParsePick(listTwo);
			Assert.IsTrue(pickTwo.IsRefused);
			StringAssert.Contains(EhrValueSetImport.Import(pickTwo),"Pick the three files of the release together");
			CollectionAssert.AreEqual(listBefore,ReadRows().Select(x => x.ValueSetOID+" "+x.CodeValue).OrderBy(x => x,StringComparer.Ordinal).ToList());
		}

		///<summary>The text of the ForkEhrValueSetRelease row in the table, or "" when there is none.</summary>
		private static string ReadReleaseJson() {
			return PIn.String(DataCore.GetScalar("SELECT ValueString FROM preference WHERE PrefName='"+EhrValueSetImport.PREF_RELEASE+"'")??"");
		}

		private static List<string> ReadRowKeys() {
			return ReadRows().Select(x => x.ValueSetOID+" "+x.CodeValue).OrderBy(x => x,StringComparer.Ordinal).ToList();
		}

		private static long CountScratchTables() {
			return PIn.Long(DataCore.GetScalar(@"SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME LIKE 'ehrcode\_%'"));
		}

		[TestMethod]
		public void Import_APickMakePickDidNotBuild_IsRefusedBeforeAnyTableWork_TheRowsAndTheReleaseUnchanged() {
			Assert.AreEqual("",EhrValueSetImport.Import(PickOrFail(ValueSetFixtures.MakeRelease())));
			List<string> listBefore=ReadRowKeys();
			string jsonBefore=ReadReleaseJson();
			Assert.AreNotEqual("",jsonBefore);
			//Hand-built picks: none refused, but without the value-set file (the swap would replace every value-set code with none), or empty.
			EhrValueSetPick pickGood=PickOrFail(ValueSetFixtures.MakeRelease(listDirectCodes:ValueSetFixtures.MakeDirectCodeRows("TESTE")));
			EhrValueSetPick pickNoValueSets=new EhrValueSetPick();
			pickNoValueSets.ListFiles=pickGood.ListFiles.ToList();
			pickNoValueSets.FileDirectCodes=pickGood.FileDirectCodes;
			pickNoValueSets.FileCodeSystemVersions=pickGood.FileCodeSystemVersions;
			pickNoValueSets.ListEhrCodesDirect=pickGood.ListEhrCodesDirect.ToList();
			pickNoValueSets.ListRoles=pickGood.ListRoles.ToList();
			EhrValueSetPick pickTwoFiles=new EhrValueSetPick();
			pickTwoFiles.ListFiles=pickGood.ListFiles.Take(2).ToList();
			pickTwoFiles.FileValueSets=pickGood.FileValueSets;
			pickTwoFiles.FileDirectCodes=pickGood.FileDirectCodes;
			pickTwoFiles.FileCodeSystemVersions=pickGood.FileCodeSystemVersions;
			foreach(EhrValueSetPick pick in new[] { pickNoValueSets,new EhrValueSetPick(),pickTwoFiles }) {
				Assert.IsFalse(pick.IsRefused);
				string error=EhrValueSetImport.Import(pick,out bool areRowsReplaced);
				StringAssert.Contains(error,"The pick is not the release's three files as MakePick reads them. Nothing was imported");
				Assert.IsFalse(areRowsReplaced);
				CollectionAssert.AreEqual(listBefore,ReadRowKeys(),"Every old row of both kinds.");
				Assert.AreEqual(jsonBefore,ReadReleaseJson(),"The release record is unchanged.");
				Assert.AreEqual(0,CountScratchTables());
			}
		}

		[TestMethod]
		public void Import_AFailureWhileTheReleaseIsRecorded_SaysTheCodesWereReplaced_NotThatTheImportFailed() {
			Assert.AreEqual("",EhrValueSetImport.Import(PickOrFail(ValueSetFixtures.MakeRelease())));
			string jsonBefore=ReadReleaseJson();
			EhrValueSetPick pickSecond=PickOrFail(ValueSetFixtures.MakeRelease(listDirectCodes:ValueSetFixtures.MakeDirectCodeRows("TESTE")));
			//The swap is done, then the preference row or the log entry cannot be written.
			EhrValueSetImport.ActionBeforeRecord=() => throw new ApplicationException("TEST record failure");
			string error;
			bool areRowsReplaced;
			try {
				error=EhrValueSetImport.Import(pickSecond,out areRowsReplaced);
			}
			finally {
				EhrValueSetImport.ActionBeforeRecord=null;
			}
			Assert.IsTrue(areRowsReplaced);
			Assert.AreEqual("The codes of the three files replaced the old ones, but the release record was not saved: TEST record failure. "
				+"Import the three files again.",error);
			Assert.IsFalse(error.Contains("unchanged"),error);
			//The rows are the new ones, of both kinds; the record is the old one; no scratch table is left.
			List<EhrCode> listRows=ReadRows();
			Assert.AreEqual(pickSecond.ListEhrCodesAll.Count,listRows.Count);
			Assert.IsTrue(listRows.Where(x => CqmCatalog.IsDirectCodeKey(x.ValueSetOID)).All(x => x.CodeValue.StartsWith("TESTE")));
			Assert.AreEqual(jsonBefore,ReadReleaseJson());
			Assert.AreEqual(0,CountScratchTables());
			//Importing the three files again records the release.
			Assert.AreEqual("",EhrValueSetImport.Import(pickSecond,out areRowsReplaced));
			Assert.IsTrue(areRowsReplaced);
			Assert.AreNotEqual(jsonBefore,ReadReleaseJson());
		}

		[TestMethod]
		public void Import_RecordsTheDirectReferenceCodesFilesOwnMeasureCheck_ApartFromTheValueSetFiles() {
			//Direct reference codes of two measures only: their file's check differs from the value-set file's 7 of 7.
			List<TestValueSetRow> listDirectCodes=new List<TestValueSetRow> {
				TestValueSetRow.MakeDirectCode(CqmCatalog.GetMeasure("CMS68").MeasureId,"SNOMEDCT","TESTD0001"),
				TestValueSetRow.MakeDirectCode(CqmCatalog.GetMeasure("CMS74").MeasureId,"SNOMEDCT","TESTD0002"),
			};
			long securityLogNum=ScratchDb.GetLastSecurityLogNum();
			Assert.AreEqual("",EhrValueSetImport.Import(PickOrFail(ValueSetFixtures.MakeRelease(listDirectCodes:listDirectCodes))));
			EhrValueSetRelease release=EhrValueSetImport.GetRelease();
			Assert.AreEqual("7 of 7 measures at the 2027 versions",release.MeasureCheck);
			Assert.AreEqual("2 of 7 measures at the 2027 versions",release.MeasureCheckDirectCodes);
			StringAssert.Contains(ReadReleaseJson(),"\"MeasureCheckDirectCodes\":\"2 of 7 measures at the 2027 versions\"");
			string logText=ScratchDb.GetSecurityLogsAfter(securityLogNum).Single(x => x.LogText.StartsWith("EHR value sets imported")).LogText;
			StringAssert.Contains(logText,"; 7 of 7 measures at the 2027 versions; direct reference codes: 2 of 7 measures at the 2027 versions;");
			//A record written before EHR-6a's fix round 4 has no such check: the log text says so.
			release.MeasureCheckDirectCodes="";
			StringAssert.Contains(EhrValueSetImport.GetLogText(release),"direct reference codes: not recorded;");
		}

		[TestMethod]
		public void Status_TheValueSetCodes_LeaveOutTheDirectReferenceCodes_CountedApart() {
			Assert.AreEqual("",EhrValueSetImport.Import(PickOrFail(ValueSetFixtures.MakeRelease())));
			List<EhrCode> listRows=ReadRows();
			List<EhrCodeSystemStatus> listStatuses=EhrCodes.GetCodeSystemStatuses();
			foreach(string codeSystem in new[] { "LOINC","SNOMEDCT","CPT" }) {
				EhrCodeSystemStatus status=listStatuses.Single(x => x.CodeSystem==codeSystem);
				int countDirect=listRows.Count(x => x.CodeSystem==codeSystem && CqmCatalog.IsDirectCodeKey(x.ValueSetOID));
				Assert.IsTrue(countDirect>0,codeSystem+": the synthetic release has direct reference codes of this system.");
				Assert.AreEqual(listRows.Count(x => x.CodeSystem==codeSystem && !CqmCatalog.IsDirectCodeKey(x.ValueSetOID)),status.CountValueSetCodes,codeSystem);
				Assert.AreEqual(countDirect,status.CountDirectCodes,codeSystem);
			}
			Assert.AreEqual(listRows.Count,listStatuses.Sum(x => x.CountValueSetCodes+x.CountDirectCodes),"Every row counted once.");
		}

		[TestMethod]
		public void Status_ALoadedTableOlderThanTheRelease_IsFlagged_WhereTheMappingExists() {
			Assert.AreEqual("",EhrValueSetImport.Import(PickOrFail(ValueSetFixtures.MakeRelease())));
			ValueSetDb.FillCodeTable("snomed","SnomedCode",701,"TESTS");
			ValueSetDb.FillCodeTable("loinc","LoincCode",21,"TESTL");
			DataCore.NonQ("UPDATE codesystem SET VersionCur='20250301' WHERE CodeSystemName='SNOMEDCT'");
			DataCore.NonQ("UPDATE codesystem SET VersionCur='2.80' WHERE CodeSystemName='LOINC'");
			DataCore.NonQ("UPDATE codesystem SET VersionCur='TEST' WHERE CodeSystemName='CDCREC'");
			EhrCodes.UpdateList();
			List<EhrCodeSystemStatus> listStatuses=EhrCodes.GetCodeSystemStatuses();
			EhrCodeSystemStatus statusSnomed=listStatuses.Single(x => x.CodeSystem=="SNOMEDCT");
			Assert.AreEqual("20250301",statusSnomed.VersionLoaded);
			Assert.AreEqual("2025-09",statusSnomed.VersionInRelease);
			Assert.IsTrue(statusSnomed.IsVersionCompared);
			Assert.IsTrue(statusSnomed.IsOlderThanRelease,"SNOMED CT of March 2025 is older than the release's September 2025.");
			StringAssert.Contains(OpenPmsDental.FormCodeSystemsImport.GetStatusText(statusSnomed),"older than the release's version");
			EhrCodeSystemStatus statusLoinc=listStatuses.Single(x => x.CodeSystem=="LOINC");
			Assert.IsTrue(statusLoinc.IsVersionCompared);
			Assert.IsFalse(statusLoinc.IsOlderThanRelease,"The same release of LOINC.");
			Assert.AreEqual("Loaded",OpenPmsDental.FormCodeSystemsImport.GetStatusText(statusLoinc));
			EhrCodeSystemStatus statusCdcrec=listStatuses.Single(x => x.CodeSystem=="CDCREC");
			Assert.AreEqual("TEST",statusCdcrec.VersionLoaded);
			Assert.AreEqual("1.2",statusCdcrec.VersionInRelease,"Both strings shown.");
			Assert.IsFalse(statusCdcrec.IsVersionCompared,"No mapping for CDCREC: nothing flagged.");
			Assert.IsFalse(statusCdcrec.IsOlderThanRelease);
			//The direct reference codes' ActCode is a code system without a table.
			Assert.IsFalse(listStatuses.Single(x => x.CodeSystem=="ActCode").HasTable);
		}

		[TestMethod]
		public void MiddleTier_AClientsList_HasTheDirectCodes_AfterInvalidTypeEhrCodes() {
			Userod userod=ScratchDb.MakeUser(PASSWORD,EnumPermType.Setup);
			ScratchDb.LogOn(userod,PASSWORD);
			ReleaseDb.UseMockMiddleTier();
			try {
				EhrCodes.Listt=new List<EhrCode>();//The list a client read before the import.
				Assert.AreEqual(0,EhrCodes.GetForValueSetOIDs(new List<string> { "drc:CMS69:Cms69Bmi" },false).Count);
				StringAssert.Contains(EhrValueSetImport.Import(PickOrFail(ValueSetFixtures.MakeRelease())),"middle tier","A client cannot import.");
				ScratchDb.UseDirectConnection();
				Assert.AreEqual("",EhrValueSetImport.Import(PickOrFail(ValueSetFixtures.MakeRelease())));
				ReleaseDb.UseMockMiddleTier();
				//The signal on a client: GetCacheDs on the server, then the client's FillCache drops its list, so the next read fetches the new rows.
				Cache.Refresh(InvalidType.EhrCodes);
				List<EhrCode> listRole=EhrCodes.GetForValueSetOIDs(new List<string> { "drc:CMS69:Cms69Bmi" },false);
				Assert.AreEqual(1,listRole.Count);
				Assert.AreEqual("Body mass index (BMI) [Ratio]",listRole[0].Description);
				Assert.AreEqual(ValueSetFixtures.MakeDirectCodeRows().Count,EhrCodes.Listt.Count(x => CqmCatalog.IsDirectCodeKey(x.ValueSetOID)));
				//The release record reads through the client's preferences as well.
				Assert.AreEqual(3,EhrValueSetImport.GetRelease().ListFiles.Count);
			}
			finally {
				ScratchDb.UseDirectConnection();
			}
		}
	}
}
