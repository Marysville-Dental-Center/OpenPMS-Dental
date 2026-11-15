// The data-entry windows' lists filled from an imported synthetic release (docs/specs/ehr-2027-measures-plan.md, section 8, package EHR-7): the
// not-performed items' codes (the two roles, tobacco screening, the BMI follow-up of E-34, the CVX table for influenza) and the item of each
// imported code, the intervention window's lists and the medication labels, the tobacco lists by the catalog's constants, the recommended
// encounter and pregnancy codes from the imported value sets and the snomed table (E-32), the pregnancy problems from "Pregnancy" alone, and the
// same lists on a Middle Tier client. On this run's ehr7_ scratch database (Support/PickListDb.cs); every code is a made-up TEST code.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace EhrTests {
	[TestClass]
	public class PickListDbTests {
		private const string PASSWORD="TestPassword1!";

		[TestInitialize]
		public void Begin() {
			PickListDb.Begin();
			foreach(string table in new[] { "ehrcode","snomed","loinc","rxnorm","hcpcs","icd10","cvx","diseasedef" }) {
				DataCore.NonQ("DELETE FROM "+table);
			}
			DataCore.NonQ("DELETE FROM preference WHERE PrefName='"+EhrValueSetImport.PREF_RELEASE+"'");
			ValueSetDb.RefreshCaches();
			EhrCodes.ClearList();
		}

		[TestCleanup]
		public void End() {
			PickListDb.End();
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			PickListDb.Cleanup();
		}

		///<summary>Imports a synthetic release: these value-set rows, one code per direct-reference-code role (ValueSetFixtures.MakeDirectCodeRows)
		///and the Code System Versions file, as the Code System Importer reads the three files.</summary>
		private static void ImportOrFail(params TestValueSetRow[] arrayRows) {
			ValueSetFixtures.Sheet sheet=new ValueSetFixtures.Sheet();
			sheet.Name="All";
			sheet.ListRows=arrayRows.ToList();
			EhrValueSetPick pick=ValueSetFixtures.ParsePick(ValueSetFixtures.MakeRelease(ValueSetFixtures.MakeWorkbook(sheet)));
			Assert.IsFalse(pick.IsRefused,EhrValueSetImport.GetReport(pick));
			Assert.AreEqual("",EhrValueSetImport.Import(pick));
			EhrCodes.UpdateList();
		}

		///<summary>Rows of a code table with these codes (the code tables of these tests hold made-up TEST codes only).</summary>
		private static void InsertCodes(string table,string columnCode,params string[] arrayCodes) {
			foreach(string code in arrayCodes) {
				DataCore.NonQ("INSERT INTO "+table+" ("+columnCode+",Description) VALUES ('"+POut.String(code)+"','TEST description of "+POut.String(code)+"')");
			}
		}

		///<summary>The rows of the release every test here imports: one or two codes in each pick list the windows read, and the codes of E-32.</summary>
		private static TestValueSetRow[] MakeRows() {
			return new TestValueSetRow[] {
				TestValueSetRow.Make(CqmCatalog.OID_TOBACCO_SCREENING,"LOINC","TEST-L01"),
				TestValueSetRow.Make(CqmCatalog.OID_TOBACCO_USER,"SNOMEDCT","TEST-S01"),
				TestValueSetRow.Make(CqmCatalog.OID_TOBACCO_NON_USER,"SNOMEDCT","TEST-S02"),
				TestValueSetRow.Make(CqmCatalog.OID_TOBACCO_COUNSELING,"SNOMEDCT","TEST-S03"),
				TestValueSetRow.Make(CqmCatalog.OID_TOBACCO_PHARMACOTHERAPY,"RXNORM","TEST-R01"),
				TestValueSetRow.Make(CqmCatalog.OID_BMI_FOLLOW_UP_ABOVE,"SNOMEDCT","TEST-S11"),
				TestValueSetRow.Make(CqmCatalog.OID_BMI_FOLLOW_UP_BELOW,"SNOMEDCT","TEST-S12"),
				TestValueSetRow.Make(CqmCatalog.OID_BMI_REFERRALS,"SNOMEDCT","TEST-S13"),
				TestValueSetRow.Make(CqmCatalog.OID_BMI_FOLLOW_UP_ABOVE,"SNOMEDCT","TEST-S13"),//One code in two sets: listed once
				TestValueSetRow.Make(CqmCatalog.OID_BMI_MEDICATIONS_ABOVE,"RXNORM","TEST-R11"),
				TestValueSetRow.Make(CqmCatalog.OID_BMI_MEDICATIONS_BELOW,"RXNORM","TEST-R12"),
				TestValueSetRow.Make(CqmCatalog.OID_COUNSELING_NUTRITION,"SNOMEDCT","TEST-S21"),
				TestValueSetRow.Make(CqmCatalog.OID_COUNSELING_PHYSICAL_ACTIVITY,"SNOMEDCT","TEST-S22"),
				TestValueSetRow.Make(CqmCatalog.OID_DIALYSIS_SERVICES,"SNOMEDCT","TEST-S31"),
				TestValueSetRow.Make(CqmCatalog.OID_REASON_MEDICAL,"SNOMEDCT","TEST-S41"),
				TestValueSetRow.Make(CqmCatalog.OID_REASON_PATIENT_DECLINED,"SNOMEDCT","TEST-S42"),
				TestValueSetRow.Make(CqmCatalog.OID_REASON_ACUTE_HEALTH_CRISIS,"SNOMEDCT","TEST-S43"),
				//E-32: the encounter codes of a qualifying and of an excluding set, and the pregnancy codes.
				TestValueSetRow.Make(CqmCatalog.GetQualifyingEncounterValueSets().First(x => x.Name=="Office Visit").Oid,"SNOMEDCT","TEST-E02"),
				TestValueSetRow.Make(CqmCatalog.GetQualifyingEncounterValueSets().First(x => x.Name=="Clinical Oral Evaluation").Oid,"SNOMEDCT","TEST-E01"),
				TestValueSetRow.Make(CqmCatalog.GetQualifyingEncounterValueSets().First(x => x.Name=="Office Visit").Oid,"CPT","TEST-E03"),
				TestValueSetRow.Make(CqmCatalog.GetQualifyingEncounterValueSets().First(x => x.Name=="Office Visit").Oid,"SNOMEDCT","TEST-E04"),
				TestValueSetRow.Make(CqmCatalog.GetValueSets().First(x => x.Name=="Hospice Encounter").Oid,"SNOMEDCT","TEST-E05"),
				TestValueSetRow.Make(CqmCatalog.OID_PREGNANCY,"SNOMEDCT","TEST-P02"),
				TestValueSetRow.Make(CqmCatalog.OID_PREGNANCY,"SNOMEDCT","TEST-P01"),
				TestValueSetRow.Make(CqmCatalog.OID_PREGNANCY,"SNOMEDCT","TEST-P03"),
				TestValueSetRow.Make(CqmCatalog.OID_PREGNANCY,"ICD10CM","TEST-P04"),
			};
		}

		///<summary>The code tables loaded, as an office loads them: the release's codes that the tests expect in the pick lists, plus synthetic rows
		///above each table's threshold (EhrCodes.GetCodeCountMinimum).</summary>
		private static void LoadCodeTables() {
			InsertCodes("snomed","SnomedCode","TEST-S01","TEST-S02","TEST-S03","TEST-S11","TEST-S12","TEST-S13","TEST-S21","TEST-S22","TEST-S31","TEST-S41",
				"TEST-S42","TEST-S43","TEST-E01","TEST-E02","TEST-E05","TEST-P01","TEST-P02");
			ValueSetDb.FillCodeTable("snomed","SnomedCode",701,"TESTS");
			ValueSetDb.FillCodeTable("loinc","LoincCode",21,"TESTL");
			ValueSetDb.FillCodeTable("rxnorm","RxCui",41,"TESTR");
			InsertCodes("cvx","CvxCode","TEST-C02","TEST-C01");
			ValueSetDb.FillCodeTable("cvx","CvxCode",5,"TESTC");
			Cache.Refresh(InvalidType.EhrCodes);
		}

		[TestMethod]
		public void NotPerformed_EachItemsCodes_FillFromTheRelease_AndEachImportedCodeHasItsItem() {
			Assert.IsFalse(CqmDataEntry.IsReleaseImported(),"No import yet: the not-performed windows say the 2027 value sets are not imported.");
			ImportOrFail(MakeRows());
			Assert.IsTrue(CqmDataEntry.IsReleaseImported(),"The import recorded the 2027 release.");
			LoadCodeTables();
			EhrCode ehrCodeBmi=CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS69_BMI);
			EhrCode ehrCodeMeds=CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED);
			Assert.IsNotNull(ehrCodeBmi,"The import resolved the role.");
			Assert.IsNotNull(ehrCodeMeds);
			Assert.AreEqual("LOINC",ehrCodeBmi.CodeSystem);
			Assert.AreEqual("SNOMEDCT",ehrCodeMeds.CodeSystem);
			StringAssert.StartsWith(ehrCodeBmi.CodeValue,"TESTD");
			CollectionAssert.AreEqual(new[] { ehrCodeBmi.CodeValue },CqmDataEntry.GetNotPerformedCodes(EhrNotPerformedItem.BMIExam).Select(x => x.CodeValue).ToList());
			CollectionAssert.AreEqual(new[] { ehrCodeMeds.CodeValue },
				CqmDataEntry.GetNotPerformedCodes(EhrNotPerformedItem.DocumentCurrentMeds).Select(x => x.CodeValue).ToList());
			CollectionAssert.AreEqual(new[] { "TEST-L01" },CqmDataEntry.GetNotPerformedCodes(EhrNotPerformedItem.TobaccoScreening).Select(x => x.CodeValue).ToList());
			CollectionAssert.AreEquivalent(new[] { "TEST-S11","TEST-S12","TEST-S13","TEST-R11","TEST-R12" },
				CqmDataEntry.GetNotPerformedCodes(EhrNotPerformedItem.BMIFollowUp).Select(x => x.CodeValue).ToList(),"E-34; TEST-S13 once.");
			List<EhrCode> listCvx=CqmDataEntry.GetNotPerformedCodes(EhrNotPerformedItem.InfluenzaVaccination);
			Assert.AreEqual(7,listCvx.Count,"E-25: the whole CVX table.");
			Assert.IsTrue(listCvx.All(x => x.CodeSystem=="CVX"));
			CollectionAssert.AreEqual(listCvx.Select(x => x.CodeValue).OrderBy(x => x,StringComparer.Ordinal).ToList(),listCvx.Select(x => x.CodeValue).ToList());
			Assert.AreEqual("TEST description of TEST-C01",listCvx.Single(x => x.CodeValue=="TEST-C01").Description);
			//The reasons of each item fill from the release.
			Dictionary<EhrNotPerformedItem,string[]> dictReasons=new Dictionary<EhrNotPerformedItem,string[]> {
				{ EhrNotPerformedItem.BMIExam,new[] { "TEST-S41","TEST-S42" } },
				{ EhrNotPerformedItem.DocumentCurrentMeds,new[] { "TEST-S43" } },
				{ EhrNotPerformedItem.BMIFollowUp,new[] { "TEST-S41" } },
				{ EhrNotPerformedItem.TobaccoScreening,new[] { "TEST-S41" } },
				{ EhrNotPerformedItem.InfluenzaVaccination,new[] { "TEST-S41" } },
			};
			foreach(KeyValuePair<EhrNotPerformedItem,string[]> pair in dictReasons) {
				CollectionAssert.AreEquivalent(pair.Value,EhrCodes.GetForValueSetOIDs(CqmDataEntry.GetNotPerformedReasonOids(pair.Key,false),true)
					.Select(x => x.CodeValue).ToList(),pair.Key.ToString());
			}
			//Each imported code labels its saved row with its item; any other is "other".
			Assert.AreEqual(EhrNotPerformedItem.BMIExam,CqmDataEntry.GetNotPerformedItem(ehrCodeBmi.CodeValue,"LOINC"));
			Assert.AreEqual(EhrNotPerformedItem.DocumentCurrentMeds,CqmDataEntry.GetNotPerformedItem(ehrCodeMeds.CodeValue,"SNOMEDCT"));
			Assert.AreEqual(EhrNotPerformedItem.TobaccoScreening,CqmDataEntry.GetNotPerformedItem("TEST-L01","LOINC"));
			Assert.AreEqual(EhrNotPerformedItem.BMIFollowUp,CqmDataEntry.GetNotPerformedItem("TEST-R12","RXNORM"));
			Assert.AreEqual(EhrNotPerformedItem.InfluenzaVaccination,CqmDataEntry.GetNotPerformedItem("TEST-C01","CVX"));
			Assert.IsNull(CqmDataEntry.GetNotPerformedItem("TEST-S01","SNOMEDCT"));
			//Before the import, no role has a code and the new BMI and current-medications rows have none to take.
			DataCore.NonQ("DELETE FROM ehrcode");
			Cache.Refresh(InvalidType.EhrCodes);
			Assert.IsNull(CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED));
			Assert.AreEqual(0,CqmDataEntry.GetNotPerformedCodes(EhrNotPerformedItem.BMIExam).Count);
			Assert.AreEqual(0,CqmDataEntry.GetNotPerformedCodes(EhrNotPerformedItem.DocumentCurrentMeds).Count);
		}

		[TestMethod]
		public void Interventions_AndTobacco_TheWindowsListsFillFromTheRelease() {
			ImportOrFail(MakeRows());
			Assert.AreEqual(0,EhrCodes.GetForValueSetOIDs(new List<string> { CqmCatalog.OID_DIALYSIS_SERVICES },true).Count,"No SNOMED CT table yet.");
			LoadCodeTables();
			Dictionary<string,string[]> dictExpected=new Dictionary<string,string[]> {
				{ "AboveNormalWeight Follow-up",new[] { "TEST-S11","TEST-S13" } },
				{ "AboveNormalWeight Referral",new[] { "TEST-S13" } },
				{ "AboveNormalWeight Medication",new[] { "TEST-R11" } },
				{ "BelowNormalWeight Follow-up",new[] { "TEST-S12" } },
				{ "BelowNormalWeight Referral",new[] { "TEST-S13" } },
				{ "BelowNormalWeight Medication",new[] { "TEST-R12" } },
				{ "Nutrition Counseling",new[] { "TEST-S21" } },
				{ "PhysicalActivity Counseling",new[] { "TEST-S22" } },
				{ "TobaccoCessation Counseling",new[] { "TEST-S03" } },
				{ "TobaccoCessation Medication",new[] { "TEST-R01" } },
				{ "Dialysis Services",new[] { "TEST-S31" } },
			};
			List<KeyValuePair<string,string>> listValueSets=CqmDataEntry.GetInterventionValueSets(InterventionCodeSet.None,true);
			CollectionAssert.AreEqual(dictExpected.Keys.ToList(),listValueSets.Select(x => x.Key).ToList());
			foreach(KeyValuePair<string,string> pair in listValueSets) {
				CollectionAssert.AreEquivalent(dictExpected[pair.Key],EhrCodes.GetForValueSetOIDs(new List<string> { pair.Value },true)
					.Select(x => x.CodeValue).ToList(),pair.Key);
			}
			//The medication rows: labelled by the value set that holds the RxCui.
			Assert.AreEqual(InterventionCodeSet.AboveNormalWeight,CqmDataEntry.GetMedicationCodeSet("TEST-R11"));
			Assert.AreEqual(InterventionCodeSet.BelowNormalWeight,CqmDataEntry.GetMedicationCodeSet("TEST-R12"));
			Assert.AreEqual(InterventionCodeSet.TobaccoCessation,CqmDataEntry.GetMedicationCodeSet("TEST-R01"));
			CollectionAssert.AreEquivalent(new[] { "TEST-R11","TEST-R12","TEST-R01" },
				EhrCodes.GetForValueSetOIDs(CqmDataEntry.GetMedicationInterventionOids(),true).Select(x => x.CodeValue).ToList());
			//The Tobacco Use tab's lists, by the catalog's constants (FormMedical, FormEhrPatientSmoking).
			Assert.AreEqual("TEST-L01",EhrCodes.GetForValueSetOIDs(new List<string> { CqmCatalog.OID_TOBACCO_SCREENING },true).Single().CodeValue);
			Assert.AreEqual("TEST-S01",EhrCodes.GetForValueSetOIDs(new List<string> { CqmCatalog.OID_TOBACCO_USER },true).Single().CodeValue);
			Assert.AreEqual("TEST-S02",EhrCodes.GetForValueSetOIDs(new List<string> { CqmCatalog.OID_TOBACCO_NON_USER },true).Single().CodeValue);
		}

		///<summary>E-32: the recommended codes are the SNOMED CT codes of the imported qualifying-encounter value sets (not the excluding ones) and of
		///"Pregnancy" that the snomed table holds, each once, in code order; none before the import or without the table.</summary>
		[TestMethod]
		public void RecommendedCodes_AreTheImportedSnomedCodesTheTableHolds() {
			Assert.AreEqual(0,CqmDataEntry.GetRecommendedEncounterCodes().Count,"Nothing imported.");
			ImportOrFail(MakeRows());
			Assert.AreEqual(0,CqmDataEntry.GetRecommendedPregnancyCodes().Count,"No snomed rows yet.");
			LoadCodeTables();
			CollectionAssert.AreEqual(new[] { "TEST-E01","TEST-E02" },CqmDataEntry.GetRecommendedEncounterCodes().Select(x => x.SnomedCode).ToList(),
				"TEST-E03 is CPT, TEST-E04 is not in the snomed table, TEST-E05 is of Hospice Encounter.");
			CollectionAssert.AreEqual(new[] { "TEST-P01","TEST-P02" },CqmDataEntry.GetRecommendedPregnancyCodes().Select(x => x.SnomedCode).ToList(),
				"TEST-P03 is not in the snomed table, TEST-P04 is ICD-10-CM.");
			Assert.AreEqual("TEST description of TEST-P01",CqmDataEntry.GetRecommendedPregnancyCodes()[0].Description);
			//More codes than one query is sent: every one is found.
			List<string> listCodes=Enumerable.Range(1,1203).Select(x => "TEST-Q"+x.ToString("D4")).ToList();
			DataCore.NonQ("DELETE FROM ehrcode");
			ImportOrFail(listCodes.Select(x => TestValueSetRow.Make(CqmCatalog.OID_PREGNANCY,"SNOMEDCT",x)).ToArray());
			InsertCodes("snomed","SnomedCode",listCodes.Where((x,i) => i%2==0).ToArray());
			Assert.AreEqual(602,CqmDataEntry.GetRecommendedPregnancyCodes().Count);
		}

		///<summary>On a Middle Tier client: the roles' codes, the recommended codes and the pregnancy problems come through the server.</summary>
		[TestMethod]
		public void MiddleTier_TheListsAndThePregnancyProblems_ThroughTheServer() {
			ImportOrFail(MakeRows());
			LoadCodeTables();
			DiseaseDef diseaseDefPreg=new DiseaseDef { DiseaseName="TEST pregnancy",SnomedCode="TEST-P01" };
			DiseaseDef diseaseDefOther=new DiseaseDef { DiseaseName="TEST not pregnancy",SnomedCode="TEST-S01" };
			DiseaseDefs.Insert(diseaseDefPreg);
			DiseaseDefs.Insert(diseaseDefOther);
			DiseaseDefs.RefreshCache();
			string codeMeds=CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED).CodeValue;
			CollectionAssert.AreEqual(new[] { diseaseDefPreg.DiseaseDefNum },DiseaseDefs.GetAllPregDiseaseDefs().Select(x => x.DiseaseDefNum).ToList(),
				"Pregnancy problems from \"Pregnancy\" alone.");
			Userod userod=ScratchDb.MakeUser(PASSWORD,EnumPermType.Setup);
			ScratchDb.LogOn(userod,PASSWORD);
			PickListDb.UseMockMiddleTier();
			try {
				EhrCodes.ClearList();
				Assert.AreEqual(codeMeds,CqmDataEntry.GetDirectCode(CqmCatalog.ROLE_CMS68_MEDS_DOCUMENTED).CodeValue);
				CollectionAssert.AreEqual(new[] { "TEST-E01","TEST-E02" },CqmDataEntry.GetRecommendedEncounterCodes().Select(x => x.SnomedCode).ToList());
				CollectionAssert.AreEqual(new[] { "TEST-P01","TEST-P02" },CqmDataEntry.GetRecommendedPregnancyCodes().Select(x => x.SnomedCode).ToList());
				Assert.AreEqual(7,CqmDataEntry.GetNotPerformedCodes(EhrNotPerformedItem.InfluenzaVaccination).Count);
				CollectionAssert.AreEqual(new[] { diseaseDefPreg.DiseaseDefNum },DiseaseDefs.GetAllPregDiseaseDefs().Select(x => x.DiseaseDefNum).ToList());
			}
			finally {
				ScratchDb.UseDirectConnection();
				DataCore.NonQ("DELETE FROM diseasedef WHERE DiseaseName LIKE 'TEST%'");
				DiseaseDefs.RefreshCache();
			}
		}
	}
}
