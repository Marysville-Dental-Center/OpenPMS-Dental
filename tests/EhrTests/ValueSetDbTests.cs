// The EHR-2 tests that need a database (docs/specs/ehr-rebuild-plan.md, EHR-2): the step 2026111401 and the ehrcode declaration on scratch
// databases, the import (rows replaced, the release recorded, Setup and the Middle Tier refused, a refused file leaving the old rows), the
// list read from the table and read again, IsInDb against synthetic code-table rows above and below the thresholds, the code-system status,
// the tobacco pick lists and the pregnancy lookup filled from imported value sets, the Middle Tier round trip (InvalidType.EhrCodes on a
// client), and the time a full synthetic release takes to load. They run on this run's ehr2_ scratch database (Support/ValueSetDb.cs);
// every row is synthetic (made-up TEST codes). Each test starts from empty value-set and code-system tables. Since EHR-6a an import takes the
// release's three files together (MakePick: the value-set file with a one-code Direct Reference Codes file and a Code System Versions file),
// and the tests count the value-set rows without the direct reference code; DirectCodeDbTests tests the pick itself.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;
using OpenPmsDentalBusiness.ForkMigrations.FeatureSteps;

namespace EhrTests {
	[TestClass]
	public class ValueSetDbTests {
		private const string TABLE="ehrcode";
		private const string OID_TOBACCO_SCREENING="2.16.840.1.113883.3.526.3.1278";
		private const string OID_TOBACCO_USER="2.16.840.1.113883.3.526.3.1170";
		private const string OID_TOBACCO_NON_USER="2.16.840.1.113883.3.526.3.1189";
		private const string OID_TOBACCO_COUNSELING="2.16.840.1.113883.3.526.3.509";
		private const string OID_TOBACCO_PHARMACOTHERAPY="2.16.840.1.113883.3.526.3.1190";
		private const string OID_PREGNANCY_DX="2.16.840.1.113883.3.600.1.1623";
		private const string OID_PREGNANCY="2.16.840.1.113883.3.526.3.378";
		private const string PASSWORD="TestPassword1!";

		[TestInitialize]
		public void Begin() {
			ValueSetDb.Begin();
			ResetTables();
		}

		[TestCleanup]
		public void End() {
			ValueSetDb.End();
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			ValueSetDb.Cleanup();
		}

		///<summary>Empty value-set and code-system tables and no recorded import, so each test starts the same.</summary>
		private static void ResetTables() {
			foreach(string table in new[] { "ehrcode","snomed","rxnorm","icd9","icd10","loinc","cpt","hcpcs","cvx","cdcrec","sop" }) {
				DataCore.NonQ("DELETE FROM "+table);
			}
			DataCore.NonQ("DELETE FROM preference WHERE PrefName='"+EhrValueSetImport.PREF_RELEASE+"'");
			ValueSetDb.RefreshCaches();
			EhrCodes.ClearList();
		}

		private static EhrValueSetFile ParseWorkbook(params TestValueSetRow[] arrayRows) {
			ValueSetFixtures.Sheet sheet=new ValueSetFixtures.Sheet();
			sheet.Name="CMS138v2";
			sheet.ListRows=arrayRows.ToList();
			EhrValueSetFile file=EhrValueSetImport.Parse("ep_value_sets_TEST.xlsx",ValueSetFixtures.MakeWorkbook(sheet));
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			return file;
		}

		///<summary>The value-set file picked with the release's other two files (EHR-6a: an import takes the three together): a Direct Reference
		///Codes file of one made-up HCPCS code under CMS68v16 (so the code systems these tests count are not touched) and a Code System Versions
		///file.</summary>
		private static EhrValueSetPick MakePick(EhrValueSetFile file) {
			EhrValueSetFile fileDirect=EhrValueSetImport.Parse("ec_direct_reference_codes_TEST.xlsx",ValueSetFixtures.MakeDirectCodesWorkbook(
				new List<TestValueSetRow> { TestValueSetRow.MakeDirectCode("CMS68v16","HCPCS","TESTH0001") }));
			EhrValueSetFile fileVersions=EhrValueSetImport.Parse("ec_code_system_versions_TEST.xlsx",
				ValueSetFixtures.MakeCodeSystemVersionsWorkbook(ValueSetFixtures.MakeCodeSystemVersionRows()));
			return EhrValueSetImport.MakePick(new List<EhrValueSetFile> { file,fileDirect,fileVersions });
		}

		private static void ImportOrFail(EhrValueSetFile file) {
			EhrValueSetPick pick=MakePick(file);
			Assert.IsFalse(pick.IsRefused,EhrValueSetImport.GetReport(pick));
			Assert.AreEqual("",EhrValueSetImport.Import(pick));
		}

		///<summary>The value-set rows of the list, without the direct reference codes the pick adds (drc: keys).</summary>
		private static List<EhrCode> ValueSetRows(List<EhrCode> listEhrCodes) {
			return listEhrCodes.Where(x => !CqmCatalog.IsDirectCodeKey(x.ValueSetOID)).ToList();
		}

		///<summary>The value-set rows of ehrcode (the direct reference codes left out).</summary>
		private static long CountRows() {
			return PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM ehrcode WHERE ValueSetOID NOT LIKE 'drc:%'"));
		}

		#region The step and the declaration
		[TestMethod]
		public void Step_TheInitFileHasTheTableAsDeclared_TheStepFindsIt_AndTheSchemaStillMatchesOpenDental() {
			//db/init/new-database.sql was regenerated at fork level 2026111401 (329793122): it creates the table as the step declares it, so the
			//scratch database built from it has nothing left for the step to do. The step's own CREATE is the test on an empty database below.
			string init=File.ReadAllText(TestPaths.Combine(ScratchDb.RepoPathInitSql)).Replace("\r\n","\n");
			StringAssert.Contains(init,EhrTables.CREATE_EHR_CODE.Replace("\r\n","\n")+";\n");
			StringAssert.Contains(init,"('ForkSchemaLevel','2026111401',");
			Assert.AreEqual(0,ValueSetDb.ListChangesBuild.Count(x => x.Contains("ehrcode")),string.Join("\r\n",ValueSetDb.ListChangesBuild));
			Assert.IsFalse(ForkMigrationRunner.GetPendingSteps().Any(x => x.Level==2026111401));
			SchemaVerifyResult resultFork=ForkTables.VerifyDatabase(ValueSetDb.DbName);
			Assert.AreEqual(0,resultFork.ListDifferences.Count(x => x.Table==TABLE),resultFork.GetReport());
			SchemaVerifyResult result=SchemaVerifier.VerifyDatabase(ValueSetDb.DbName);
			Assert.IsTrue(result.IsMatch,result.GetReport());
			List<SchemaDifference> listAllowed=result.ListAllowed.Where(x => x.Table==TABLE).ToList();
			Assert.AreEqual(1,listAllowed.Count,"Excepted once.");
			Assert.AreEqual(SchemaDifferenceKind.ExtraTable,listAllowed[0].Kind);
			StringAssert.Contains(listAllowed[0].ExceptionReason,"Open PMS - Dental table (EHR value sets, fork migration 2026111401)");
			Assert.AreEqual(0,CountRows(),"The step writes no row.");
			//The table's DDL is exactly what SHOW CREATE TABLE prints on this server.
			DataTable table=DataCore.GetTable("SHOW CREATE TABLE ehrcode");
			Assert.AreEqual(EhrTables.CREATE_EHR_CODE.Replace("\r\n","\n"),System.Text.RegularExpressions.Regex.Replace(table.Rows[0][1].ToString(),
				@" AUTO_INCREMENT=\d+",""));
		}

		[TestMethod]
		public void Step_OnAnAlreadyMigratedDatabase_ChangesNothing_AndKeepsTheRows() {
			ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001")));
			ForkMigrationContext context=new ForkMigrationContext(ValueSetDb.DbName);
			new EhrCodeTableStep().Apply(context);
			Assert.AreEqual(0,context.ListChanges.Count,string.Join("\r\n",context.ListChanges));
			Assert.AreEqual(1,CountRows());
			//The runner sees nothing pending: the step is recorded as applied.
			Assert.IsFalse(ForkMigrationRunner.GetPendingSteps().Any(x => x.Level==2026111401));
		}

		[TestMethod]
		public void Step_OnAnEmptyDatabase_CreatesTheEmptyTable_ThenChangesNothing_AndATableOfAnotherDefinitionStopsIt() {
			string dbName=ValueSetDb.CreateEmpty("empty");
			try {
				new DataConnection().SetDb(ScratchDb.Server,dbName,ScratchDb.User,ScratchDb.Password,"","",DatabaseType.MySql,true,DevDbTls.GetSslCa(ScratchDb.Server));
				ForkMigrationContext context=new ForkMigrationContext(dbName);
				new EhrCodeTableStep().Apply(context);
				Assert.AreEqual(1,context.ListChanges.Count,string.Join("\r\n",context.ListChanges));
				Assert.AreEqual(1,SchemaSnapshot.FromDatabase(dbName).Tables.Count);
				SchemaVerifyResult result=ForkTables.VerifyDatabase(dbName);
				Assert.AreEqual(0,result.ListDifferences.Count(x => x.Table==TABLE),result.GetReport());
				ForkMigrationContext contextSecond=new ForkMigrationContext(dbName);
				new EhrCodeTableStep().Apply(contextSecond);
				Assert.AreEqual(0,contextSecond.ListChanges.Count,string.Join("\r\n",contextSecond.ListChanges));
				//A table of that name with another definition (as an HQ database's or another program's could be) stops the step, unchanged.
				DataCore.NonQ("DROP TABLE ehrcode");
				DataCore.NonQ("CREATE TABLE ehrcode (EhrCodeNum bigint(20) NOT NULL AUTO_INCREMENT, CodeValue varchar(20) NOT NULL, IsInDb tinyint(4) NOT NULL, "
					+"PRIMARY KEY (EhrCodeNum)) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci");
				DataCore.NonQ("INSERT INTO ehrcode (CodeValue,IsInDb) VALUES ('TEST0001',1)");
				ForkMigrationContext contextThird=new ForkMigrationContext(dbName);
				ApplicationException exception=Expect.Throws<ApplicationException>(() => new EhrCodeTableStep().Apply(contextThird));
				StringAssert.Contains(exception.Message,"already has a table named ehrcode");
				StringAssert.Contains(exception.Message,"IsInDb");
				StringAssert.Contains(exception.Message,"MeasureIds");
				Assert.AreEqual(0,contextThird.ListChanges.Count);
				Assert.AreEqual(1L,PIn.Long(DataCore.GetScalar("SELECT COUNT(*) FROM ehrcode")),"The other table is left alone.");
				Assert.IsNotNull(SchemaSnapshot.FromDatabase(dbName).GetTable(TABLE).GetColumn("IsInDb"));
			}
			finally {
				ValueSetDb.Drop(dbName);
				ScratchDb.Connect(ValueSetDb.DbName);
			}
		}
		#endregion The step and the declaration

		#region Import
		[TestMethod]
		public void Import_ReplacesEveryRow_RecordsTheRelease_AndLogsSetup() {
			long securityLogNum=ScratchDb.GetLastSecurityLogNum();
			ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"),
				TestValueSetRow.Make(OID_TOBACCO_USER,"SNOMEDCT","TEST0002")));
			Assert.AreEqual(2,CountRows());
			CollectionAssert.AreEqual(new string[0],ListScratchTables(),"The scratch table became ehrcode, the old table was dropped.");
			EhrValueSetFile fileSecond=ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_NON_USER,"SNOMEDCT","TEST0003"),
				TestValueSetRow.Make(OID_TOBACCO_COUNSELING,"SNOMEDCT","TEST0004"),TestValueSetRow.Make(OID_TOBACCO_PHARMACOTHERAPY,"RXNORM","TEST0005"));
			ImportOrFail(fileSecond);
			List<EhrCode> listRows=ValueSetRows(EhrCodes.GetAllFromDb());
			CollectionAssert.AreEquivalent(new[] { "TEST0003","TEST0004","TEST0005" },listRows.Select(x => x.CodeValue).ToList(),"Every earlier row replaced.");
			EhrCode ehrCode=listRows.Single(x => x.CodeValue=="TEST0005");
			Assert.AreEqual(OID_TOBACCO_PHARMACOTHERAPY,ehrCode.ValueSetOID);
			Assert.AreEqual("Tobacco Use Cessation Pharmacotherapy",ehrCode.ValueSetName);
			Assert.AreEqual("RXNORM",ehrCode.CodeSystem);
			Assert.AreEqual("2.16.840.1.113883.6.88",ehrCode.CodeSystemOID);
			Assert.AreEqual("138v15",ehrCode.MeasureIds);
			Assert.AreEqual("Medication",ehrCode.QDMCategory);
			Assert.AreEqual("TEST description of TEST0005",ehrCode.Description);
			//The release.
			EhrValueSetRelease release=EhrValueSetImport.GetRelease();
			Assert.IsNotNull(release);
			Assert.AreEqual(EhrValueSetImport.RELEASE_LABEL,release.Label);
			Assert.AreEqual("ep_value_sets_TEST.xlsx",release.FileName);
			Assert.AreEqual(fileSecond.Sha256,release.Sha256);
			Assert.AreEqual(4,release.CountCodes,"Three value-set codes and the one direct reference code.");
			Assert.AreEqual(1,release.CountDirectCodes);
			Assert.AreEqual(3,release.CountValueSets);
			Assert.AreEqual(3,release.CountManifestFound);
			Assert.AreEqual(EhrValueSetImport.GetManifest().Count,release.CountManifest);
			Assert.AreEqual("1 of 7 measures at the 2027 versions",release.MeasureCheck);
			Assert.IsTrue(DateTime.TryParse(release.DateImportedUtc,System.Globalization.CultureInfo.InvariantCulture,
				System.Globalization.DateTimeStyles.AdjustToUniversal,out DateTime dateImported));
			Assert.IsTrue(Math.Abs((DateTime.UtcNow-dateImported).TotalMinutes)<10,release.DateImportedUtc);
			string json=PIn.String(DataCore.GetScalar("SELECT ValueString FROM preference WHERE PrefName='"+EhrValueSetImport.PREF_RELEASE+"'"));
			StringAssert.Contains(json,fileSecond.Sha256,"The row in the table, not only the cache.");
			//One Setup entry per import, with the file and the hash, and no code.
			List<SecurityLog> listLogs=ScratchDb.GetSecurityLogsAfter(securityLogNum).Where(x => x.LogText.StartsWith("EHR value sets imported")).ToList();
			Assert.AreEqual(2,listLogs.Count);
			Assert.IsTrue(listLogs.All(x => x.PermType==EnumPermType.Setup));
			StringAssert.Contains(listLogs[1].LogText,"ValueSets 'ep_value_sets_TEST.xlsx' SHA-256 "+fileSecond.Sha256);
			StringAssert.Contains(listLogs[1].LogText,"4 codes (1 direct reference codes) in 3 value sets, 3 of the "
				+EhrValueSetImport.GetManifest().Count+" value sets the measures use; 1 of 7 measures at the 2027 versions;");
			Assert.IsFalse(listLogs.Any(x => x.LogText.Contains("TEST000")),"No code is logged.");
		}

		[TestMethod]
		public void Import_ARefusedFile_LeavesTheOldRowsAndTheRelease() {
			ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001")));
			EhrValueSetRelease releaseBefore=EhrValueSetImport.GetRelease();
			EhrValueSetFile fileRefused=EhrValueSetImport.Parse("other.xml",System.Text.Encoding.UTF8.GetBytes("<notes>TEST</notes>"));
			Assert.IsTrue(fileRefused.IsRefused);
			string error=EhrValueSetImport.Import(MakePick(fileRefused));
			StringAssert.Contains(error,"not an SVS file");
			Assert.AreEqual(1,CountRows());
			Assert.AreEqual(releaseBefore.Sha256,EhrValueSetImport.GetRelease().Sha256);
			Assert.AreNotEqual("",EhrValueSetImport.Import((EhrValueSetPick)null));
			Assert.AreEqual(1,CountRows());
		}

		///<summary>The tables of this database whose names start with ehrcode_ (the import's scratch tables), sorted.</summary>
		private static List<string> ListScratchTables() {
			DataTable table=DataCore.GetTable(@"SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME LIKE 'ehrcode\_%'");
			return table.Rows.Cast<DataRow>().Select(x => x[0].ToString()).OrderBy(x => x,StringComparer.Ordinal).ToList();
		}

		[TestMethod]
		public void Import_AFailureWhileWritingTheRows_LeavesTheOldRows_AndNoScratchTable() {
			ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001")));
			EhrValueSetRelease releaseBefore=EhrValueSetImport.GetRelease();
			EhrValueSetFile fileSecond=ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_USER,"SNOMEDCT","TEST0002"),
				TestValueSetRow.Make(OID_TOBACCO_NON_USER,"SNOMEDCT","TEST0003"));
			//The rows are written, then the server fails (as a full disk or a lost connection would).
			EhrValueSetImport.ActionAfterInsertBatch=countWritten => throw new ApplicationException("TEST failure after "+countWritten+" rows were written");
			string error;
			try {
				error=EhrValueSetImport.Import(MakePick(fileSecond));
			}
			finally {
				EhrValueSetImport.ActionAfterInsertBatch=null;
			}
			StringAssert.Contains(error,"TEST failure after 3 rows were written","The two value-set codes and the direct reference code.");
			StringAssert.Contains(error,"unchanged");
			CollectionAssert.AreEqual(new[] { "TEST0001" },ValueSetRows(EhrCodes.GetAllFromDb()).Select(x => x.CodeValue).ToList(),"The old rows, all of them.");
			Assert.AreEqual(releaseBefore.Sha256,EhrValueSetImport.GetRelease().Sha256,"The release is not recorded.");
			CollectionAssert.AreEqual(new string[0],ListScratchTables(),"No scratch table is left.");
			//The next import works.
			ImportOrFail(fileSecond);
			CollectionAssert.AreEquivalent(new[] { "TEST0002","TEST0003" },ValueSetRows(EhrCodes.GetAllFromDb()).Select(x => x.CodeValue).ToList());
			CollectionAssert.AreEqual(new string[0],ListScratchTables());
		}

		[TestMethod]
		public void Import_DropsTheScratchTablesAnImportLeftADayAgo_AndKeepsTheOthers() {
			string recent="ehrcode_import_"+DateTime.UtcNow.ToString("yyyyMMddHHmmss",System.Globalization.CultureInfo.InvariantCulture)+"_0a1b2c";
			string[] arrayTables=new[] { "ehrcode_import_20200101000000_0a1b2c","ehrcode_old_20200101000000_0a1b2c",recent,"ehrcode_import_test" };
			try {
				foreach(string table in arrayTables) {
					DataCore.NonQ("CREATE TABLE "+table+" LIKE ehrcode");
				}
				ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001")));
				CollectionAssert.AreEqual(new[] { recent,"ehrcode_import_test" }.OrderBy(x => x,StringComparer.Ordinal).ToList(),ListScratchTables(),
					"Another import's table of today, and a table of a name this program does not write, are left alone.");
				Assert.AreEqual(1,CountRows());
			}
			finally {
				foreach(string table in arrayTables) {
					DataCore.NonQ("DROP TABLE IF EXISTS "+table);
				}
			}
		}

		[TestMethod]
		public void Import_ADescriptionAsLongAsTheTextColumn_IsStoredWhole() {
			TestValueSetRow row=TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001");
			row.Description=new string('€',21845);//65,535 bytes in UTF-8.
			ImportOrFail(ParseWorkbook(row));
			Assert.AreEqual(row.Description,ValueSetRows(EhrCodes.GetAllFromDb()).Single().Description);
		}

		[TestMethod]
		public void Import_WithoutSetup_IsRefused_NothingWrittenOrLogged() {
			ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001")));
			Userod userod=ScratchDb.MakeUser(PASSWORD,EnumPermType.ProblemDefEdit,EnumPermType.EhrKeyAdd,EnumPermType.EhrMeasureEventEdit);
			long securityLogNum=ScratchDb.GetLastSecurityLogNum();
			ScratchDb.LogOn(userod,PASSWORD);
			string error=EhrValueSetImport.Import(MakePick(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_USER,"SNOMEDCT","TEST0002"))));
			StringAssert.Contains(error,"Not authorized");
			Assert.AreEqual("TEST0001",ValueSetRows(EhrCodes.GetAllFromDb()).Single().CodeValue);
			Assert.AreEqual(0,ScratchDb.GetSecurityLogsAfter(securityLogNum).Count(x => x.LogText.StartsWith("EHR value sets imported")));
			Security.CurUser=null;
			StringAssert.Contains(EhrValueSetImport.Import(MakePick(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_USER,"SNOMEDCT","TEST0002")))),"Not authorized");
			Assert.AreEqual(1,CountRows());
		}

		[TestMethod]
		public void Import_ThroughTheMiddleTier_IsRefused_AsTheCptImportIs() {
			Userod userod=ScratchDb.MakeUser(PASSWORD,EnumPermType.Setup);
			ScratchDb.LogOn(userod,PASSWORD);
			EhrValueSetPick pick=MakePick(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001")));
			ValueSetDb.UseMockMiddleTier();
			try {
				StringAssert.Contains(EhrValueSetImport.Import(pick),"middle tier");
			}
			finally {
				ScratchDb.UseDirectConnection();
			}
			Assert.AreEqual(0,CountRows());
			Assert.IsNull(EhrValueSetImport.GetRelease());
			//The window refuses too, before it asks for a file.
			string window=TestPaths.ReadSource(@"OpenPmsDental\Forms\FormCodeSystemsImport.cs");
			string click=window.Substring(window.IndexOf("private void butImportValueSets_Click",StringComparison.Ordinal));
			Assert.IsTrue(click.IndexOf("MiddleTierRole.ClientMT",StringComparison.Ordinal)<click.IndexOf("Security.IsAuthorized(EnumPermType.Setup)",StringComparison.Ordinal));
			Assert.IsTrue(click.IndexOf("Security.IsAuthorized(EnumPermType.Setup)",StringComparison.Ordinal)<click.IndexOf("GetValueSetLicensePrompt()",StringComparison.Ordinal));
			Assert.IsTrue(click.IndexOf("GetValueSetLicensePrompt()",StringComparison.Ordinal)<click.IndexOf("new OpenFileDialog()",StringComparison.Ordinal));
			Assert.IsTrue(click.IndexOf("new OpenFileDialog()",StringComparison.Ordinal)<click.IndexOf("EhrValueSetImport.Import(",StringComparison.Ordinal));
			StringAssert.Contains(click,"DataValid.SetInvalid(InvalidType.EhrCodes,InvalidType.Prefs)");
			string cpt=window.Substring(window.IndexOf("private void butImport_Click",StringComparison.Ordinal));
			Assert.IsTrue(cpt.IndexOf("Security.IsAuthorized(EnumPermType.Setup)",StringComparison.Ordinal)<cpt.IndexOf("new OpenFileDialog()",StringComparison.Ordinal),
				"The CPT import needs Setup too (section 5.1).");
			StringAssert.Contains(FormCodeSystemsImportText.LicensePrompt,"UMLS license");
			StringAssert.Contains(FormCodeSystemsImportText.LicensePrompt,"CPT");
			StringAssert.Contains(FormCodeSystemsImportText.LicensePrompt,"CDT");
			StringAssert.Contains(FormCodeSystemsImportText.LicensePrompt,"SNOMED CT");
		}
		#endregion Import

		#region The list
		[TestMethod]
		public void Listt_ReadsTheTable_AndUpdateListReadsItAgain() {
			Assert.AreEqual(0,EhrCodes.Listt.Count,"Empty until the office imports.");
			ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001")));
			Assert.AreEqual(0,EhrCodes.Listt.Count,"The list in memory until it is read again.");
			EhrCodes.UpdateList();
			Assert.AreEqual("TEST0001",ValueSetRows(EhrCodes.Listt).Single().CodeValue);
			ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_USER,"SNOMEDCT","TEST0002"),TestValueSetRow.Make(OID_TOBACCO_USER,"SNOMEDCT","TEST0003")));
			//The signal the window sends: on a direct connection Cache.GetCacheDs reads the table again.
			Cache.Refresh(InvalidType.EhrCodes);
			CollectionAssert.AreEquivalent(new[] { "TEST0002","TEST0003" },ValueSetRows(EhrCodes.Listt).Select(x => x.CodeValue).ToList());
			Assert.AreEqual("138v15",EhrCodes.GetMeasureIdsForCode("TEST0002","SNOMEDCT"));
			CollectionAssert.AreEqual(new[] { OID_TOBACCO_USER },EhrCodes.GetValueSetOIDsForCode("TEST0003","SNOMEDCT"));
			EhrCodes.ClearList();
			Assert.AreEqual(2,ValueSetRows(EhrCodes.Listt).Count,"Read again after ClearList.");
		}

		[TestMethod]
		public void IsInDb_FollowsTheCodeTablesThresholds_AndTheStatusShowsThem() {
			TestValueSetRow rowSex=TestValueSetRow.Make("2.16.840.1.113762.1.4.1021.121","AdministrativeSex","TEST-F");//Federal Administrative Sex.
			TestValueSetRow rowPcs=TestValueSetRow.Make(OID_TOBACCO_USER,"SNOMEDCT","TEST0004");
			rowPcs.CodeSystem="ICD10PCS";
			rowPcs.CodeSystemOID="2.16.840.1.113883.6.4";
			ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"),
				TestValueSetRow.Make(OID_TOBACCO_PHARMACOTHERAPY,"RXNORM","TEST0002"),TestValueSetRow.Make(OID_PREGNANCY,"ICD9CM","TEST0003"),rowSex,rowPcs));
			//At the threshold: not in the database (the table must have more rows than the code list's minimum).
			ValueSetDb.FillCodeTable("snomed","SnomedCode",700,"TESTS");
			ValueSetDb.FillCodeTable("rxnorm","RxCui",41,"TESTR");
			EhrCodes.UpdateList();
			Dictionary<string,bool> dictIsInDb=EhrCodes.Listt.ToDictionary(x => x.CodeValue,x => x.IsInDb);
			Assert.IsFalse(dictIsInDb["TEST0001"],"700 SNOMED CT rows: not more than 700.");
			Assert.IsTrue(dictIsInDb["TEST0002"],"41 RxNorm rows: more than 40.");
			Assert.IsFalse(dictIsInDb["TEST0003"],"No ICD-9-CM rows.");
			Assert.IsTrue(dictIsInDb["TEST-F"],"AdministrativeSex is always in the database.");
			Assert.IsFalse(dictIsInDb["TEST0004"],"A code system without a table never is.");
			ValueSetDb.FillCodeTable("snomed","SnomedCode",1,"TESTT");
			EhrCodes.UpdateList();
			Assert.IsTrue(EhrCodes.Listt.Single(x => x.CodeValue=="TEST0001").IsInDb,"701 SNOMED CT rows.");
			Assert.AreEqual(1,EhrCodes.GetForValueSetOIDs(new List<string> { OID_TOBACCO_SCREENING },true).Count);
			Assert.AreEqual(0,EhrCodes.GetForValueSetOIDs(new List<string> { OID_PREGNANCY },true).Count);
			Assert.AreEqual(1,EhrCodes.GetForValueSetOIDs(new List<string> { OID_PREGNANCY },false).Count);
			//The status the Code System Importer shows.
			List<EhrCodeSystemStatus> listStatuses=EhrCodes.GetCodeSystemStatuses();
			CollectionAssert.AreEqual(EhrCodes.ArrayCodeSystems.Concat(new[] { "ICD10PCS" }).ToList(),listStatuses.Select(x => x.CodeSystem).ToList(),
				"The pick's one direct reference code is HCPCS, a code system of the list.");
			EhrCodeSystemStatus statusSnomed=listStatuses.Single(x => x.CodeSystem=="SNOMEDCT");
			Assert.AreEqual(701,statusSnomed.CountInDb);
			Assert.AreEqual(700,statusSnomed.CountMinimum);
			Assert.IsTrue(statusSnomed.IsInDb);
			Assert.AreEqual(1,statusSnomed.CountValueSetCodes);
			EhrCodeSystemStatus statusIcd9=listStatuses.Single(x => x.CodeSystem=="ICD9CM");
			Assert.AreEqual(0,statusIcd9.CountInDb);
			Assert.AreEqual(1500,statusIcd9.CountMinimum);
			Assert.IsFalse(statusIcd9.IsInDb);
			Assert.IsFalse(listStatuses.Single(x => x.CodeSystem=="ICD10PCS").HasTable);
			Assert.IsTrue(listStatuses.Single(x => x.CodeSystem=="AdministrativeSex").IsInDb);
			Assert.AreEqual("Load this code system's table directly",OpenPmsDental.FormCodeSystemsImport.GetStatusText(statusIcd9));
			Assert.AreEqual("Loaded",OpenPmsDental.FormCodeSystemsImport.GetStatusText(statusSnomed));
			StringAssert.Contains(OpenPmsDental.FormCodeSystemsImport.GetStatusText(listStatuses.Single(x => x.CodeSystem=="ICD10PCS")),"No table");
			Assert.AreEqual("not imported",OpenPmsDental.FormCodeSystemsImport.GetValueSetVersionText(null));
			//The pick's one direct reference code (HCPCS) is counted apart from the value-set codes.
			EhrCodeSystemStatus statusHcpcs=listStatuses.Single(x => x.CodeSystem=="HCPCS");
			Assert.AreEqual(0,statusHcpcs.CountValueSetCodes,"No value-set code is HCPCS: the direct reference code is not one.");
			Assert.AreEqual(1,statusHcpcs.CountDirectCodes);
			Assert.AreEqual(0,statusSnomed.CountDirectCodes);
			Assert.AreEqual("imported 2026-11-14: vs.xlsx",OpenPmsDental.FormCodeSystemsImport.GetValueSetVersionText(
				new EhrValueSetRelease { Label=EhrValueSetImport.RELEASE_LABEL,DateImportedUtc="2026-11-14T10:00:00Z",FileName="vs.xlsx" }));
			//An office still on EHR-2's import of the June 2013 release (the same preference row) is told that the release it has is the old one.
			string textOld=OpenPmsDental.FormCodeSystemsImport.GetValueSetVersionText(new EhrValueSetRelease {
				Label="VSAC June 2013 EP Only Release eCQM Value Sets (MU2 EP Update 2013-06-14)",DateImportedUtc="2026-10-20T10:00:00Z",FileName="ep_TEST.zip" });
			StringAssert.StartsWith(textOld,"old release loaded (VSAC June 2013 EP Only Release");
			StringAssert.Contains(textOld,"imported 2026-10-20: ep_TEST.zip");
			StringAssert.EndsWith(textOld,"Import the release this program needs: "+EhrValueSetImport.RELEASE_LABEL+".");
			StringAssert.Contains(OpenPmsDental.FormCodeSystemsImport.GetValueSetVersionText(new EhrValueSetRelease { FileName="vs.xlsx" }),
				"old release loaded (an earlier release");
		}

		[TestMethod]
		public void TobaccoLists_FillFromTheImportedValueSets_WhenTheCodeTablesAreLoaded() {
			ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0201"),TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0202"),
				TestValueSetRow.Make(OID_TOBACCO_USER,"SNOMEDCT","TEST0211"),TestValueSetRow.Make(OID_TOBACCO_NON_USER,"SNOMEDCT","TEST0221"),
				TestValueSetRow.Make(OID_TOBACCO_COUNSELING,"SNOMEDCT","TEST0231"),TestValueSetRow.Make(OID_TOBACCO_PHARMACOTHERAPY,"RXNORM","TEST0241")));
			EhrCodes.UpdateList();
			Assert.AreEqual(0,EhrCodes.GetForValueSetOIDs(new List<string> { OID_TOBACCO_SCREENING },true).Count,"No SNOMED CT table yet: the tab stays off.");
			ValueSetDb.FillCodeTable("snomed","SnomedCode",701,"TESTS");
			ValueSetDb.FillCodeTable("rxnorm","RxCui",41,"TESTR");
			Cache.Refresh(InvalidType.EhrCodes);
			Dictionary<string,string[]> dictExpected=new Dictionary<string,string[]> {
				{ OID_TOBACCO_SCREENING,new[] { "TEST0201","TEST0202" } },
				{ OID_TOBACCO_USER,new[] { "TEST0211" } },
				{ OID_TOBACCO_NON_USER,new[] { "TEST0221" } },
				{ OID_TOBACCO_COUNSELING,new[] { "TEST0231" } },
				{ OID_TOBACCO_PHARMACOTHERAPY,new[] { "TEST0241" } },
			};
			foreach(KeyValuePair<string,string[]> pair in dictExpected) {
				CollectionAssert.AreEquivalent(pair.Value,EhrCodes.GetForValueSetOIDs(new List<string> { pair.Key },true).Select(x => x.CodeValue).ToList(),pair.Key);
			}
			//The windows read exactly these value sets, with usingIsInDb (the tobacco tab of FormMedical, FormEhrPatientSmoking).
			foreach(string path in new[] { @"OpenPmsDental\Forms\FormMedical.cs",@"OpenPmsDental\Forms\FormEhrPatientSmoking.cs" }) {
				string source=TestPaths.ReadSource(path);
				foreach(string oid in dictExpected.Keys) {
					StringAssert.Contains(source,"EhrCodes.GetForValueSetOIDs(new List<string> { \""+oid+"\" },true)",path);
				}
			}
		}

		[TestMethod]
		public void Pregnancy_DiseaseDefsLooksInBothPregnancyValueSets_AndTheDiagnosisSetOfThe2014Measures_IsNotKept() {
			//DiseaseDefs still names the 2014 diagnosis value set (OID_PREGNANCY_DX, on the allowlist of ValueSetSourceTests until EHR-7 moves the
			//lookup to the catalog's Pregnancy). It is no value set of the catalog, so the import does not keep its rows (E-30), and E-20's record
			//finds it nowhere in the 2026-05-14 release either: the lookup finds no problem until EHR-7.
			ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_PREGNANCY_DX,"ICD9CM","TEST0101"),TestValueSetRow.Make(OID_PREGNANCY,"ICD9CM","TEST0101"),
				TestValueSetRow.Make(OID_PREGNANCY,"ICD9CM","TEST0102"),TestValueSetRow.Make(OID_PREGNANCY_DX,"SNOMEDCT","TEST0103"),
				TestValueSetRow.Make(OID_PREGNANCY,"SNOMEDCT","TEST0103")));
			ValueSetDb.FillCodeTable("icd9","ICD9Code",1501,"TESTI");
			EhrCodes.UpdateList();
			DiseaseDef diseaseDefBoth=new DiseaseDef { DiseaseName="TEST pregnancy in both sets",ICD9Code="TEST0101" };
			DiseaseDef diseaseDefOne=new DiseaseDef { DiseaseName="TEST pregnancy in one set",ICD9Code="TEST0102" };
			DiseaseDef diseaseDefSnomed=new DiseaseDef { DiseaseName="TEST pregnancy SNOMED CT, table not loaded",SnomedCode="TEST0103" };
			DiseaseDefs.Insert(diseaseDefBoth);
			DiseaseDefs.Insert(diseaseDefOne);
			DiseaseDefs.Insert(diseaseDefSnomed);
			DiseaseDefs.RefreshCache();
			try {
				Assert.AreEqual(0,EhrCodes.GetForValueSetOIDs(new List<string> { OID_PREGNANCY_DX },false).Count,"Not a value set of the catalog: not kept.");
				Assert.AreEqual(2,EhrCodes.GetForValueSetOIDs(new List<string> { OID_PREGNANCY },true).Count,"The catalog's Pregnancy: kept.");
				List<DiseaseDef> listFound=DiseaseDefs.GetAllPregDiseaseDefs();
				Assert.AreEqual(0,listFound.Count,"No code is in both sets: the 2014 one is not imported.");
				//Its call names the two value sets (DiseaseDefs.cs:404).
				StringAssert.Contains(TestPaths.ReadSource(@"OpenPmsDentalBusiness\Data Interface\DiseaseDefs.cs"),
					"EhrCodes.GetCodesExistingInAllSets(new List<string> { \""+OID_PREGNANCY_DX+"\",\""+OID_PREGNANCY+"\" })");
			}
			finally {
				DataCore.NonQ("DELETE FROM diseasedef WHERE DiseaseName LIKE 'TEST pregnancy%'");
				DiseaseDefs.RefreshCache();
			}
		}
		#endregion The list

		#region Middle Tier
		[TestMethod]
		public void MiddleTier_AClientReadsTheListThroughTheServer_AndInvalidTypeEhrCodesBringsTheNewRows() {
			Userod userod=ScratchDb.MakeUser(PASSWORD,EnumPermType.Setup);
			ValueSetDb.FillCodeTable("snomed","SnomedCode",701,"TESTS");
			ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_SCREENING,"SNOMEDCT","TEST0001"),TestValueSetRow.Make(OID_TOBACCO_USER,"SNOMEDCT","TEST0002")));
			ScratchDb.LogOn(userod,PASSWORD);
			ValueSetDb.UseMockMiddleTier();
			try {
				//GetAllFromDb runs on the server (the client may not send SQL).
				CollectionAssert.AreEquivalent(new[] { "TEST0001","TEST0002" },ValueSetRows(EhrCodes.GetAllFromDb()).Select(x => x.CodeValue).ToList());
				EhrCodes.ClearList();
				List<EhrCode> listClient=ValueSetRows(EhrCodes.Listt);
				Assert.AreEqual(2,listClient.Count);
				Assert.IsTrue(listClient.All(x => x.IsInDb),"IsInDb worked out on the client from the server's counts.");
				//The client keeps its list until it is told: a stale list stands for the list a client read before the import.
				EhrCodes.Listt=new List<EhrCode> { new EhrCode { CodeValue="TEST-STALE",CodeSystem="SNOMEDCT",ValueSetOID=OID_TOBACCO_SCREENING } };
				ScratchDb.UseDirectConnection();
				ImportOrFail(ParseWorkbook(TestValueSetRow.Make(OID_TOBACCO_NON_USER,"SNOMEDCT","TEST0003"),TestValueSetRow.Make(OID_TOBACCO_NON_USER,"SNOMEDCT","TEST0004"),
					TestValueSetRow.Make(OID_TOBACCO_COUNSELING,"SNOMEDCT","TEST0005")));
				ValueSetDb.UseMockMiddleTier();
				Assert.AreEqual("TEST-STALE",EhrCodes.Listt.Single().CodeValue);
				//The client's own FillCache drops the list (the server sends no table for EhrCodes), so the next read fetches the new rows.
				Cache.FillCache(new DataSet(),InvalidType.EhrCodes);
				CollectionAssert.AreEquivalent(new[] { "TEST0003","TEST0004","TEST0005" },ValueSetRows(EhrCodes.Listt).Select(x => x.CodeValue).ToList());
				//The whole signal on a client: GetCacheDs on the server (which reads the table again there), then FillCache here.
				EhrCodes.Listt=new List<EhrCode>();
				Cache.Refresh(InvalidType.EhrCodes);
				Assert.AreEqual(3,ValueSetRows(EhrCodes.Listt).Count);
				Assert.AreEqual(2,EhrCodes.GetForValueSetOIDs(new List<string> { OID_TOBACCO_NON_USER },true).Count);
			}
			finally {
				ScratchDb.UseDirectConnection();
			}
			//The cache code: the client branch is in FillCache, the server's in GetCacheDs.
			string cache=TestPaths.ReadSource(@"OpenPmsDentalBusiness\Cache\Cache.cs");
			string fillCache=cache.Substring(cache.IndexOf("public static void FillCache(DataSet ds",StringComparison.Ordinal));
			fillCache=fillCache.Substring(0,fillCache.IndexOf("\r\n\t\t}\r\n",StringComparison.Ordinal));
			StringAssert.Contains(fillCache,"EhrCodes.ClearList();");
		}
		#endregion Middle Tier

		#region Load time
		///<summary>The full load, timed on the seed database with a full synthetic release (section 7, EHR-2 step 2): a background load comes back
		///only if it takes over 1 second. 60,000 rows is above what the June 2013 EP release holds for all its measures (docs/port-notes/EHR-2.md).
		///The times go to the file EHR2_TIMING_FILE names, when set.</summary>
		[TestMethod]
		public void LoadTime_AFullSyntheticRelease_LoadsInUnderASecond() {
			const int countRowsRead=66000;
			ValueSetDb.FillCodeTable("snomed","SnomedCode",701,"TESTS");
			ValueSetFixtures.Sheet sheet=new ValueSetFixtures.Sheet();
			sheet.Name="All";
			//The columns the importer reads, so 66,000 rows stay one part within the part limit.
			sheet.ArrayHeaders=new[] { "CMS ID","Value Set Name","Value Set OID","QDM Category","Expansion Version","Code","Description","Code System",
				"Code System Version","Code System OID" };
			List<TestValueSetRow> listRows=ValueSetFixtures.MakeLargeRelease(countRowsRead);
			sheet.ListRows=listRows;
			int countRows=listRows.Count(x => EhrValueSetImport.GetManifestItem(x.ValueSetOID)!=null);//60,000: only these are kept (E-30).
			byte[] arrayWorkbook=ValueSetFixtures.MakeWorkbook(sheet);
			Stopwatch stopwatchParse=Stopwatch.StartNew();
			EhrValueSetFile file=EhrValueSetImport.Parse("large_TEST.xlsx",arrayWorkbook);
			stopwatchParse.Stop();
			Assert.IsFalse(file.IsRefused,EhrValueSetImport.GetReport(file));
			Assert.AreEqual(60000,countRows);
			Assert.AreEqual(countRows,file.ListEhrCodes.Count);
			Assert.AreEqual(countRowsRead-countRows,file.CountRowsOtherValueSets);
			Stopwatch stopwatchImport=Stopwatch.StartNew();
			ImportOrFail(file);
			stopwatchImport.Stop();
			Assert.AreEqual(countRows,CountRows());
			//The first load after start-up (the table read, then IsInDb), as Listt does it: three times, the best of the three against the bound, so
			//a run beside other tests (parallel load on the same machine) does not fail it for a slow moment.
			List<long> listLoadMs=new List<long>();
			for(int i=0;i<3;i++) {
				EhrCodes.ClearList();
				Stopwatch stopwatchLoad=Stopwatch.StartNew();
				int count=EhrCodes.Listt.Count;
				stopwatchLoad.Stop();
				Assert.AreEqual(countRows+1,count,"With the pick's one direct reference code.");
				listLoadMs.Add(stopwatchLoad.ElapsedMilliseconds);
			}
			long loadMsBest=listLoadMs.Min();
			Stopwatch stopwatchReload=Stopwatch.StartNew();
			EhrCodes.UpdateList();
			stopwatchReload.Stop();
			Stopwatch stopwatchLookup=Stopwatch.StartNew();
			//Without IsInDb: with the 68 value sets of the 2027 catalog the synthetic rows of this value set fall on code systems whose tables are
			//not loaded here (MakeLargeRelease cycles eight code systems), so IsInDb would find none (EHR-6a).
			int countFound=EhrCodes.GetForValueSetOIDs(new List<string> { OID_TOBACCO_SCREENING },false).Count;
			stopwatchLookup.Stop();
			Assert.IsTrue(countFound>0);
			string timing="EHR-2 load time, "+countRows+" rows, workbook "+arrayWorkbook.Length+" bytes: parse "+stopwatchParse.ElapsedMilliseconds+" ms, import "
				+stopwatchImport.ElapsedMilliseconds+" ms, first load (Listt) "+string.Join(" / ",listLoadMs)+" ms (best "+loadMsBest+" ms), UpdateList "
				+stopwatchReload.ElapsedMilliseconds
				+" ms, one value-set lookup "+stopwatchLookup.ElapsedMilliseconds+" ms.";
			Console.WriteLine(timing);
			string pathTiming=Environment.GetEnvironmentVariable("EHR2_TIMING_FILE");
			if(!string.IsNullOrEmpty(pathTiming)) {
				File.AppendAllText(pathTiming,DateTime.Now.ToString("s")+" "+timing+"\r\n");
			}
			Assert.IsTrue(loadMsBest<1000,"Over 1 second, the best of three: a background load comes back (section 7, EHR-2 step 2). "+timing);
		}
		#endregion Load time
	}

	///<summary>The window's texts that the tests read without showing the window.</summary>
	internal static class FormCodeSystemsImportText {
		public static string LicensePrompt {
			get {
				return OpenPmsDental.FormCodeSystemsImport.GetValueSetLicensePrompt();
			}
		}
	}
}
