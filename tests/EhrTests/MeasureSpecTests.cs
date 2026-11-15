// The measure fixtures of package EHR-4b (docs/specs/ehr-rebuild-plan.md, section 7 and section 9.3): for each stage (Stage 1: SelectAll,
// Stage 2: SelectAllMu2, Modified Stage 2: SelectAllMu2Mod), synthetic patients who meet and who miss each measure the engine calculates, with
// the numerator and denominator the public CMS specification sheet's definitions give, and a recorded run of the three SelectAll calls on the
// whole fixture as golden numbers (Fixtures/MeasureSpecGolden.txt). A case where the engine disagrees with its sheet is Inconclusive with a
// deviation ID (EHR4B-Dnn), listed in docs/port-notes/EHR-4.md (section "EHR-4b"). Package EHR-4c fixed the engine for every numeric deviation
// (D01-D04, D06-D11, D15): each of those tests now asserts the sheet's numbers and keeps its ID in its name (section "EHR-4c" of that note).
//
// The sheets (CMS EHR Incentive Program specification sheets for eligible professionals, US government works), cited in each test by file name,
// the date the sheet states and its rule; the latest edition of each stage first:
//   Stage 1: "Stage 1 (2014 Definition)", Last updated: May 2014 (22 sheets, such as 3_Maintain_Problem_ListEP.pdf); the older "Stage 1 (2013
//            Definition)" sheets (2013DefinitionEP_*.pdf, Last Updated: April 2013) only where the engine follows that edition.
//   Stage 2: Stage2_EPCore_*.pdf and Stage2_EPMenu_*.pdf, "Date issued" October 2012 to November 2014, Stage2_EPCore_15 "Last Updated: August
//            2015": the last Stage 2 edition.
//   Modified Stage 2: "Objectives and Measures for 2018", Updated: July 2018 (MedicaidEPModStage2_2018_Obj1.pdf to Obj10.pdf); the 2017 edition
//            (MedicaidEPStage2_Obj1.pdf to Obj10.pdf, Updated: November 2016) gives the same numbers for every case here.
// Only the definitions are cited and paraphrased; no sheet text is copied. The engine's numbers of a case are its audit table's rows for the
// case's patients (MeasureSpecFixture.Count), the table SelectAll counts (AuditTables_AreWhatTheSelectAllCallsCount). Synthetic data only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace EhrTests {
	[TestClass]
	public class MeasureSpecTests {
		private const string GOLDEN_PATH=@"tests\EhrTests\Fixtures\MeasureSpecGolden.txt";
		private const EhrMeasureStage S1=EhrMeasureStage.Stage1;
		private const EhrMeasureStage S2=EhrMeasureStage.Stage2;
		private const EhrMeasureStage MS2=EhrMeasureStage.ModifiedStage2;
		private CultureInfo _cultureBefore;

		[TestInitialize]
		public void Initialize() {
			_cultureBefore=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
		}

		[TestCleanup]
		public void Cleanup() {
			MeasureSpecFixture.End();
			Thread.CurrentThread.CurrentCulture=_cultureBefore;
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			MeasureSpecFixture.DropAll();
		}

		#region Helpers
		///<summary>Compares the engine with the sheets, case by case. A disagreement that is a known deviation is recorded, with the engine's own
		///numbers checked too, so a change to the engine is noticed; any other disagreement fails. Finish fails on a failure, and is Inconclusive
		///when a deviation was recorded.</summary>
		private sealed class SpecCheck {
			private readonly List<string> _listFailures=new List<string>();
			private readonly List<string> _listDeviations=new List<string>();

			///<summary>The engine must give the sheet's numerator and denominator for the patients under the keys.</summary>
			public void Sheet(EhrMeasureStage stage,EhrMeasureType measureType,int numerator,int denominator,params string[] arrayKeys) {
				MeasureSpecFixture.Count(stage,measureType,arrayKeys,out int numeratorEngine,out int denominatorEngine);
				if(numeratorEngine!=numerator || denominatorEngine!=denominator) {
					_listFailures.Add(Text(stage,measureType,arrayKeys)+": the sheet gives "+numerator+"/"+denominator+", the engine "
						+numeratorEngine+"/"+denominatorEngine+".");
				}
			}

			///<summary>A known deviation: the sheet gives one pair for the patients under the keys, the engine (as recorded in the list) another. No
			///numeric deviation is open since EHR-4c; kept for the next one found.</summary>
			public void Deviation(string deviationId,EhrMeasureStage stage,EhrMeasureType measureType,int numeratorSheet,int denominatorSheet,
				int numeratorEngine,int denominatorEngine,params string[] arrayKeys)
			{
				MeasureSpecFixture.Count(stage,measureType,arrayKeys,out int numerator,out int denominator);
				string text=Text(stage,measureType,arrayKeys);
				if(numerator==numeratorSheet && denominator==denominatorSheet) {
					_listFailures.Add(text+": the engine now gives the sheet's "+numeratorSheet+"/"+denominatorSheet+"; take "+deviationId
						+" off the list in docs/port-notes/EHR-4.md and make this a sheet case.");
					return;
				}
				if(numerator!=numeratorEngine || denominator!=denominatorEngine) {
					_listFailures.Add(text+" ("+deviationId+"): the engine gives "+numerator+"/"+denominator+", not the recorded "
						+numeratorEngine+"/"+denominatorEngine+" (the sheet: "+numeratorSheet+"/"+denominatorSheet+").");
					return;
				}
				_listDeviations.Add(deviationId+" "+text+": the sheet gives "+numeratorSheet+"/"+denominatorSheet+", the engine "+numeratorEngine+"/"
					+denominatorEngine+".");
			}

			public void Finish() {
				if(_listFailures.Count>0) {
					Assert.Fail(string.Join("\r\n",_listFailures));
				}
				if(_listDeviations.Count>0) {
					Assert.Inconclusive("Known deviations from the sheets (docs/port-notes/EHR-4.md, EHR-4b): "+string.Join(" ",_listDeviations));
				}
			}

			private static string Text(EhrMeasureStage stage,EhrMeasureType measureType,string[] arrayKeys) {
				return stage+" "+measureType+" ("+string.Join(", ",arrayKeys)+")";
			}
		}

		///<summary>One measure the engine calculates in a stage: the threshold ("more than N percent") and the sheet it takes it from, or the
		///deviation that records it has none.</summary>
		private sealed class SheetRule {
			public EhrMeasureType MeasureType;
			public int Threshold;
			public string Sheet;

			public SheetRule(EhrMeasureType measureType,int threshold,string sheet) {
				MeasureType=measureType;
				Threshold=threshold;
				Sheet=sheet;
			}
		}

		///<summary>Every measure of each stage, in the engine's order (GetStageMeasureTypes), with its sheet and the sheet's threshold.</summary>
		private static List<SheetRule> GetSheetRules(EhrMeasureStage stage) {
			const string s1="Stage 1 (2014 Definition), Last updated: May 2014: ";
			const string ms2="Modified Stage 2, Objectives and Measures for 2018, Updated: July 2018: ";
			switch(stage) {
				case EhrMeasureStage.Stage1:
					return new List<SheetRule>() {
						new SheetRule(EhrMeasureType.ProblemList,80,s1+"3_Maintain_Problem_ListEP.pdf, more than 80 percent"),
						new SheetRule(EhrMeasureType.MedicationList,80,s1+"5_Active_Medication_List.pdf, more than 80 percent"),
						new SheetRule(EhrMeasureType.AllergyList,80,s1+"6_Medication_Allergy_List.pdf, more than 80 percent"),
						new SheetRule(EhrMeasureType.Demographics,50,s1+"7_Record_Demographics.pdf, more than 50 percent"),
						new SheetRule(EhrMeasureType.Education,10,s1+"6_Patient-Specific_Education_Resources.pdf, more than 10 percent"),
						new SheetRule(EhrMeasureType.ProvOrderEntry,30,s1+"1_CPOE_for_Medication_Orders.pdf, NUMERATOR / DENOMINATOR, more than 30 percent"),
						new SheetRule(EhrMeasureType.CPOE_MedOrdersOnly,30,s1+"1_CPOE_for_Medication_Orders.pdf, OPTIONAL NUMERATOR / DENOMINATOR, more than 30 percent"),
						new SheetRule(EhrMeasureType.CPOE_PreviouslyOrdered,30,"no sheet: EHR4B-D13 (a CMS FAQ alternate of 1_CPOE_for_Medication_Orders.pdf, whose 30 percent it keeps)"),
						new SheetRule(EhrMeasureType.Rx,40,s1+"4_e-prescribing.pdf, more than 40 percent"),
						new SheetRule(EhrMeasureType.VitalSigns,50,"Stage 1 (2013 Definition), Last updated: April 2013: 2013DefinitionEP_8_Record_Vital_Signs.pdf, the first NUMERATOR / DENOMINATOR, more than 50 percent (EHR4B-D05)"),
						new SheetRule(EhrMeasureType.VitalSignsBMIOnly,50,s1+"8_Record_Vital_Signs.pdf, EXCLUSION 3 (height and weight only), more than 50 percent"),
						new SheetRule(EhrMeasureType.VitalSignsBPOnly,50,s1+"8_Record_Vital_Signs.pdf, EXCLUSION 4 (blood pressure only), more than 50 percent"),
						new SheetRule(EhrMeasureType.Smoking,50,s1+"9_Record_Smoking_Status.pdf, more than 50 percent"),
						new SheetRule(EhrMeasureType.Lab,40,s1+"2_Clinical_Lab_Test_Results.pdf, more than 40 percent"),
						new SheetRule(EhrMeasureType.ClinicalSummaries,50,s1+"13_Clinical_Summaries.pdf, more than 50 percent"),
						new SheetRule(EhrMeasureType.Reminders,20,s1+"4_Patient_Reminders.pdf, more than 20 percent"),
						new SheetRule(EhrMeasureType.MedReconcile,50,s1+"7_Medication_Reconciliation.pdf, more than 50 percent"),
						new SheetRule(EhrMeasureType.SummaryOfCare,50,s1+"8_Transition_of_Care_Summary.pdf, more than 50 percent"),
						new SheetRule(EhrMeasureType.ElectronicCopyAccess,50,s1+"11_Patient_Electronic_Access-.pdf, more than 50 percent"),
						new SheetRule(EhrMeasureType.VitalSigns2014,50,s1+"8_Record_Vital_Signs.pdf, NUMERATOR / DENOMINATOR, more than 50 percent")
					};
				case EhrMeasureStage.Stage2:
					return new List<SheetRule>() {
						new SheetRule(EhrMeasureType.CPOE_MedOrdersOnly,60,"Stage2_EPCore_1_CPOE_MedicationOrders.pdf (Date issued: October, 2012), Measure 1: Medication, more than 60 percent"),
						new SheetRule(EhrMeasureType.CPOE_LabOrdersOnly,30,"Stage2_EPCore_1_CPOE_MedicationOrders.pdf (Date issued: October, 2012), Measure 3: Laboratory, more than 30 percent"),
						new SheetRule(EhrMeasureType.CPOE_RadiologyOrdersOnly,30,"Stage2_EPCore_1_CPOE_MedicationOrders.pdf (Date issued: October, 2012), Measure 2: Radiology, more than 30 percent"),
						new SheetRule(EhrMeasureType.Rx,50,"Stage2_EPCore_2_ePrescribing.pdf (Date issued: November, 2014), more than 50 percent"),
						new SheetRule(EhrMeasureType.Demographics,80,"Stage2_EPCore_3_RecordingDemographics.pdf (Date issued: October, 2012), more than 80 percent"),
						new SheetRule(EhrMeasureType.VitalSigns,80,"Stage2_EPCore_4_RecordVitalSigns.pdf (Date issued: October, 2012), more than 80 percent"),
						new SheetRule(EhrMeasureType.VitalSignsBMIOnly,80,"Stage2_EPCore_4_RecordVitalSigns.pdf (Date issued: October, 2012), EXCLUSIONS (3), more than 80 percent"),
						new SheetRule(EhrMeasureType.VitalSignsBPOnly,80,"Stage2_EPCore_4_RecordVitalSigns.pdf (Date issued: October, 2012), EXCLUSIONS (4), more than 80 percent"),
						new SheetRule(EhrMeasureType.Smoking,80,"Stage2_EPCore_5_RecordSmokingStatus.pdf (Date issued: October, 2012), more than 80 percent"),
						new SheetRule(EhrMeasureType.ElectronicCopyAccess,50,"Stage2_EPCore_7_PatientElectronicAccess.pdf (Date issued: August, 2014), Measure 1, more than 50 percent"),
						new SheetRule(EhrMeasureType.ElectronicCopy,5,"Stage2_EPCore_7_PatientElectronicAccess.pdf (Date issued: August, 2014), Measure 2, more than 5 percent"),
						new SheetRule(EhrMeasureType.ClinicalSummaries,50,"Stage2_EPCore_8_ClinicalSummaries.pdf (Date issued: November, 2014), more than 50 percent"),
						new SheetRule(EhrMeasureType.Lab,55,"Stage2_EPCore_10_ClinicalLabTestResults.pdf (Date issued: November, 2014), more than 55 percent"),
						new SheetRule(EhrMeasureType.Reminders,10,"Stage2_EPCore_12_PreventiveCare.pdf (Date issued: October, 2012), more than 10 percent"),
						new SheetRule(EhrMeasureType.Education,10,"Stage2_EPCore_13_PatientSpecificEdRes.pdf (Date issued: October, 2012), more than 10 percent"),
						new SheetRule(EhrMeasureType.MedReconcile,50,"Stage2_EPCore_14_MedicationReconciliation.pdf (Date issued: October, 2012), more than 50 percent"),
						new SheetRule(EhrMeasureType.SummaryOfCare,50,"Stage2_EPCore_15_SummaryCare.pdf (Last Updated: August, 2015), MEASURE 1, more than 50 percent"),
						new SheetRule(EhrMeasureType.SummaryOfCareElectronic,10,"Stage2_EPCore_15_SummaryCare.pdf (Last Updated: August, 2015), MEASURE 2, more than 10 percent"),
						new SheetRule(EhrMeasureType.SecureMessaging,5,"Stage2_EPCore_17_UseSecureElectronicMessaging.pdf (Date issued: October, 2012), more than 5 percent"),
						new SheetRule(EhrMeasureType.FamilyHistory,20,"Stage2_EPMenu_4_FamilyHealthHistory.pdf (Date issued: October, 2012), more than 20 percent"),
						new SheetRule(EhrMeasureType.ElectronicNote,30,"Stage2_EPMenu_2_ElectronicNotes.pdf (Date issued: November, 2014), more than 30 percent"),
						new SheetRule(EhrMeasureType.LabImages,10,"Stage2_EPMenu_3_ImagingResults.pdf (Date issued: October, 2012), more than 10 percent")
					};
				case EhrMeasureStage.ModifiedStage2:
					return new List<SheetRule>() {
						new SheetRule(EhrMeasureType.CPOE_MedOrdersOnly,60,ms2+"MedicaidEPModStage2_2018_Obj3.pdf, Measure 1, more than 60 percent"),
						new SheetRule(EhrMeasureType.CPOE_LabOrdersOnly,30,ms2+"MedicaidEPModStage2_2018_Obj3.pdf, Measure 2, more than 30 percent"),
						new SheetRule(EhrMeasureType.CPOE_RadiologyOrdersOnly,30,ms2+"MedicaidEPModStage2_2018_Obj3.pdf, Measure 3, more than 30 percent"),
						new SheetRule(EhrMeasureType.Rx,50,ms2+"MedicaidEPModStage2_2018_Obj4.pdf, more than 50 percent"),
						new SheetRule(EhrMeasureType.SummaryOfCare,50,"no sheet: EHR4B-D12 (Stage 2's Stage2_EPCore_15_SummaryCare.pdf MEASURE 1, whose 50 percent it keeps)"),
						new SheetRule(EhrMeasureType.SummaryOfCareElectronic,10,ms2+"MedicaidEPModStage2_2018_Obj5.pdf, more than 10 percent"),
						new SheetRule(EhrMeasureType.Education,10,ms2+"MedicaidEPModStage2_2018_Obj6.pdf, more than 10 percent"),
						new SheetRule(EhrMeasureType.MedReconcile,50,ms2+"MedicaidEPModStage2_2018_Obj7.pdf, more than 50 percent"),
						new SheetRule(EhrMeasureType.ElectronicCopyAccess,50,ms2+"MedicaidEPModStage2_2018_Obj8.pdf, Measure 1, more than 50 percent"),
						new SheetRule(EhrMeasureType.ElectronicCopy,5,ms2+"MedicaidEPModStage2_2018_Obj8.pdf, Measure 2, greater than 5 percent"),
						new SheetRule(EhrMeasureType.SecureMessaging,5,ms2+"MedicaidEPModStage2_2018_Obj9.pdf, more than 5 percent"),
						new SheetRule(EhrMeasureType.VitalSigns,80,"no sheet: EHR4B-D12 (Stage 2's Stage2_EPCore_4_RecordVitalSigns.pdf, whose 80 percent it keeps)"),
						new SheetRule(EhrMeasureType.Smoking,80,"no sheet: EHR4B-D12 (Stage 2's Stage2_EPCore_5_RecordSmokingStatus.pdf, whose 80 percent it keeps)"),
						new SheetRule(EhrMeasureType.Demographics,80,"no sheet: EHR4B-D12 (Stage 2's Stage2_EPCore_3_RecordingDemographics.pdf, whose 80 percent it keeps)")
					};
			}
			throw new ApplicationException("Stage not found: "+stage);
		}
		#endregion Helpers

		#region The sheet map and the fixture's dates, no database
		[TestMethod]
		public void SheetMap_EveryMeasureOfEachStage_HasItsSheetOrIsListedWithoutOne() {
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				List<EhrMeasureType> listEngine=EhrMeasures.GetStageMeasureTypes(stage);
				List<EhrMeasureType> listMapped=GetSheetRules(stage).Select(x => x.MeasureType).ToList();
				CollectionAssert.AreEqual(listEngine,listMapped,stage+": every measure the engine calculates, in its order, is mapped.");
				foreach(SheetRule sheetRule in GetSheetRules(stage)) {
					Assert.IsTrue(sheetRule.Sheet.Contains(".pdf"),stage+" "+sheetRule.MeasureType+": the sheet is named.");
					Assert.IsTrue(!sheetRule.Sheet.StartsWith("no sheet") || sheetRule.Sheet.Contains("EHR4B-D"),
						stage+" "+sheetRule.MeasureType+": a measure with no sheet names its entry of the list.");
				}
			}
			//The measures with no sheet: Stage 1's previously-ordered CPOE alternate (D13), and four Modified Stage 2 measures that the 2015-2017
			//rule removed (D12: neither MedicaidEPModStage2_2018_Obj1.pdf to Obj10.pdf nor the 2017 edition has them).
			Assert.AreEqual(1,GetSheetRules(S1).Count(x => x.Sheet.StartsWith("no sheet")));
			Assert.AreEqual(0,GetSheetRules(S2).Count(x => x.Sheet.StartsWith("no sheet")));
			Assert.AreEqual(4,GetSheetRules(MS2).Count(x => x.Sheet.StartsWith("no sheet")));
		}

		[TestMethod]
		public void Fixture_TheWeekdaysTheBusinessDayCasesNeed_AndTheEnginesCodesReadAtRunTime() {
			Assert.AreEqual(DayOfWeek.Monday,MeasureSpecFixture.DateMonday.DayOfWeek,"3 March 2014");
			Assert.AreEqual(DayOfWeek.Thursday,MeasureSpecFixture.DateMonday.AddDays(3).DayOfWeek,"The third business day after it.");
			Assert.AreEqual(DayOfWeek.Friday,MeasureSpecFixture.DateMonday.AddDays(4).DayOfWeek,"The fourth.");
			Assert.AreEqual(DayOfWeek.Tuesday,MeasureSpecFixture.DateMonday.AddDays(8).DayOfWeek,"The sixth.");
			Assert.AreEqual(DayOfWeek.Monday,MeasureSpecFixture.DateMonday.AddDays(7).DayOfWeek,"The fourth business day after Tuesday 4 March.");
			Assert.AreEqual(DayOfWeek.Monday,new DateTime(2014,6,2).DayOfWeek,"The second visit of ClinSum.TwoVisits.");
			Assert.AreEqual(MeasureSpecFixture.DateStart.Year,MeasureSpecFixture.DateEnd.Year,"One full calendar year.");
			Assert.AreEqual(new DateTime(MeasureSpecFixture.DateStart.Year,1,1),MeasureSpecFixture.DateStart);
			Assert.AreEqual(new DateTime(MeasureSpecFixture.DateStart.Year,12,31),MeasureSpecFixture.DateEnd);
			//Before EHR-4c the engine took two ages on today's date (EHR4B-D03, D04); it takes them at the visit now. The guard stays: from 14
			//November 2026 on, a patient born before 14 November 2013 is 13 or older.
			Assert.IsTrue(DateTime.Today.AddYears(-13)>=MeasureSpecFixture.DateBornBefore.AddDays(-1),"Every patient is 13 or older today.");
			Assert.IsTrue(MeasureSpecCodes.GetSmokingStatusCode().All(char.IsDigit),"The engine's smoking status code is read from its source.");
			Assert.IsTrue(MeasureSpecCodes.GetLabResultCode().All(char.IsDigit),"The engine's lab result code is read from its source.");
		}
		#endregion The sheet map and the fixture's dates, no database

		#region The engine's numbers, thresholds and golden numbers
		[TestMethod]
		public void AuditTables_AreWhatTheSelectAllCallsCount() {
			MeasureSpecFixture.Begin();
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				foreach(EhrMeasure ehrMeasure in MeasureSpecFixture.GetSelectAll(stage)) {
					System.Data.DataTable table=MeasureSpecFixture.GetAudit(stage,ehrMeasure.MeasureType);
					Assert.AreEqual(table.Rows.Count,ehrMeasure.Denominator,stage+" "+ehrMeasure.MeasureType+": the denominator is the audit table's rows.");
					Assert.AreEqual(table.Rows.Cast<System.Data.DataRow>().Count(x => x["met"].ToString()=="X"),ehrMeasure.Numerator,
						stage+" "+ehrMeasure.MeasureType+": the numerator is its met rows.");
				}
			}
		}

		[TestMethod]
		public void Thresholds_EachStageAndMeasure_AreTheSheetsMoreThanPercent() {
			MeasureSpecFixture.Begin();
			List<string> listProblems=new List<string>();
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				Dictionary<EhrMeasureType,int> dictThresholds=MeasureSpecFixture.GetSelectAll(stage).ToDictionary(x => x.MeasureType,x => x.PercentThreshold);
				foreach(SheetRule sheetRule in GetSheetRules(stage)) {
					if(dictThresholds[sheetRule.MeasureType]!=sheetRule.Threshold) {
						listProblems.Add(stage+" "+sheetRule.MeasureType+": the engine's "+dictThresholds[sheetRule.MeasureType]+", the sheet's "
							+sheetRule.Threshold+" ("+sheetRule.Sheet+")");
					}
				}
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
		}

		///<summary>SelectAll, SelectAllMu2 and SelectAllMu2Mod for the EP on the whole fixture, one line per measure.</summary>
		private static List<string> GetGoldenLines() {
			List<string> listLines=new List<string>();
			AddGoldenLines(listLines,"SelectAll",MeasureSpecFixture.GetSelectAll(S1));
			AddGoldenLines(listLines,"SelectAllMu2",MeasureSpecFixture.GetSelectAll(S2));
			AddGoldenLines(listLines,"SelectAllMu2Mod",MeasureSpecFixture.GetSelectAll(MS2));
			return listLines;
		}

		private static void AddGoldenLines(List<string> listLines,string call,List<EhrMeasure> listEhrMeasures) {
			foreach(EhrMeasure ehrMeasure in listEhrMeasures) {
				listLines.Add(string.Format(CultureInfo.InvariantCulture,"{0} {1} {2}/{3} more than {4} exclusions {5}",call,ehrMeasure.MeasureType,
					ehrMeasure.Numerator,ehrMeasure.Denominator,ehrMeasure.PercentThreshold,ehrMeasure.ExclusionCount));
			}
		}

		[TestMethod]
		public void Golden_TheThreeSelectAllCalls_GiveTheRecordedNumbers() {
			MeasureSpecFixture.Begin();
			List<string> listActual=GetGoldenLines();
			Assert.AreEqual(20+22+14,listActual.Count,"20 Stage 1, 22 Stage 2 and 14 Modified Stage 2 measures.");
			string pathGolden=TestPaths.Combine(GOLDEN_PATH);
			List<string> listGolden=File.Exists(pathGolden)
				? File.ReadAllLines(pathGolden,Encoding.UTF8).Select(x => x.Trim()).Where(x => x!="" && !x.StartsWith("#")).ToList()
				: new List<string>();
			if(!listGolden.SequenceEqual(listActual)) {
				Assert.Fail("The numbers differ from "+GOLDEN_PATH+". This run gave:\r\n"+string.Join("\r\n",listActual));
			}
		}
		#endregion The engine's numbers, thresholds and golden numbers

		#region Lists (Stage 1)
		[TestMethod]
		public void ProblemMedicationAllergyLists_Stage1_AnEntryOrNoneKnown_Meets_NoEntryMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//3_Maintain_Problem_ListEP.pdf, Stage 1 (2014 Definition), Last updated: May 2014, Attestation Requirements. DENOMINATOR: unique patients
			//seen by the EP in the period. NUMERATOR: those with at least one entry, or an indication that no problems are known, as structured data.
			check.Sheet(S1,EhrMeasureType.ProblemList,2,3,"Problem.Coded","Problem.NoneKnown","Problem.Empty");
			//5_Active_Medication_List.pdf, May 2014, Attestation Requirements. DENOMINATOR: unique patients seen. NUMERATOR: those with a medication,
			//or an indication that the patient is not currently prescribed any, as structured data.
			check.Sheet(S1,EhrMeasureType.MedicationList,2,3,"MedList.Entry","MedList.NoneKnown","MedList.Empty");
			//6_Medication_Allergy_List.pdf, May 2014, Attestation Requirements. DENOMINATOR: unique patients seen. NUMERATOR: those with at least one
			//entry, or an indication of no known medication allergies, as structured data.
			check.Sheet(S1,EhrMeasureType.AllergyList,2,3,"Allergy.Entry","Allergy.NoneKnown","Allergy.Empty");
			check.Finish();
		}
		#endregion Lists (Stage 1)

		#region Demographics, vital signs, smoking
		[TestMethod]
		public void Demographics_AllElementsOrDeclined_Meets_AMissingElementMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//7_Record_Demographics.pdf, Stage 1 (2014 Definition), May 2014, and Stage2_EPCore_3_RecordingDemographics.pdf, Date issued: October,
			//2012, Attestation Requirements. DENOMINATOR: unique patients seen. NUMERATOR: those with all the elements, or a specific exclusion
			//(notation) where the patient declined one, as structured data. Modified Stage 2 has no sheet (EHR4B-D12; ModifiedStage2_...).
			check.Sheet(S1,EhrMeasureType.Demographics,2,3,"Demo.All","Demo.Declined","Demo.NoEthnicity");
			check.Sheet(S2,EhrMeasureType.Demographics,2,3,"Demo.All","Demo.Declined","Demo.NoEthnicity");
			check.Finish();
		}

		[TestMethod]
		public void VitalSigns_EachNumerator_HeightAndWeight_BloodPressureFrom3() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			string[] arrayAll=new[] { "Vs.Full","Vs.HeightWeight","Vs.Child2","Vs.Bp","Vs.None" };
			//An adult with all three signs, one with height and weight, a child of 2 all year with height and weight, an adult with blood pressure,
			//and one with none.
			//8_Record_Vital_Signs.pdf, Stage 1 (2014 Definition), May 2014, NUMERATOR / DENOMINATOR. DENOMINATOR: unique patients seen (age 3 or over
			//for blood pressure). NUMERATOR: those with height, weight and blood pressure (ages 3 and over): the adult with all three and the child.
			check.Sheet(S1,EhrMeasureType.VitalSigns2014,2,5,arrayAll);
			//The same sheet, EXCLUSION 3 (height and weight relevant, blood pressure not): height and weight of every patient seen.
			check.Sheet(S1,EhrMeasureType.VitalSignsBMIOnly,3,5,arrayAll);
			//EXCLUSION 4 (blood pressure relevant, height and weight not), with EXCLUSION 1's age: blood pressure of the patients 3 or older.
			check.Sheet(S1,EhrMeasureType.VitalSignsBPOnly,2,4,arrayAll);
			//2013DefinitionEP_8_Record_Vital_Signs.pdf, Stage 1 (2013 Definition), Last updated: April 2013, the first NUMERATOR / DENOMINATOR (the
			//May 2014 edition no longer has it: EHR4B-D05). DENOMINATOR: unique patients age 2 or over seen. NUMERATOR: height, weight and blood
			//pressure: the adult with all three only.
			check.Sheet(S1,EhrMeasureType.VitalSigns,1,5,arrayAll);
			//Stage2_EPCore_4_RecordVitalSigns.pdf, Date issued: October, 2012, Attestation Requirements. DENOMINATOR: unique patients seen. NUMERATOR:
			//height/length and weight (all ages) and/or blood pressure (ages 3 and over). Only the patients whose answer does not depend on the
			//sheet's "and/or" are counted here: all three, none, and the child of 2 with height and weight.
			check.Sheet(S2,EhrMeasureType.VitalSigns,2,3,"Vs.Full","Vs.None","Vs.Child2");
			//The same sheet, EXCLUSIONS (3) and (4), as in Stage 1.
			check.Sheet(S2,EhrMeasureType.VitalSignsBMIOnly,3,5,arrayAll);
			check.Sheet(S2,EhrMeasureType.VitalSignsBPOnly,2,4,arrayAll);
			check.Finish();
		}

		[TestMethod]
		public void VitalSigns_AnInfantOf1_IsInNoDenominatorOfAge2_D04() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//An infant of 1 at the visit, with height and weight. 8_Record_Vital_Signs.pdf (May 2014) and Stage2_EPCore_4_RecordVitalSigns.pdf
			//(October 2012): every patient seen is in the denominator, blood pressure only from 3, so the infant meets; and is in no denominator of
			//blood pressure.
			check.Sheet(S1,EhrMeasureType.VitalSigns2014,1,1,"Vs.Infant");
			check.Sheet(S2,EhrMeasureType.VitalSigns,1,1,"Vs.Infant");
			check.Sheet(S1,EhrMeasureType.VitalSignsBPOnly,0,0,"Vs.Infant");
			//2013DefinitionEP_8_Record_Vital_Signs.pdf (April 2013), the first DENOMINATOR: unique patients age 2 or over seen, so not the infant.
			//EHR4B-D04, fixed by EHR-4c: the engine takes the age at the last visit in the period (it took it on today's date: 0/1).
			check.Sheet(S1,EhrMeasureType.VitalSigns,0,0,"Vs.Infant");
			check.Finish();
		}

		[TestMethod]
		public void Smoking_StatusRecorded_Meets_NoneMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//9_Record_Smoking_Status.pdf, Stage 1 (2014 Definition), May 2014, and Stage2_EPCore_5_RecordSmokingStatus.pdf, Date issued: October,
			//2012, Attestation Requirements. DENOMINATOR: unique patients age 13 or older seen. NUMERATOR: those with smoking status recorded as
			//structured data. Modified Stage 2 has no sheet (EHR4B-D12).
			check.Sheet(S1,EhrMeasureType.Smoking,1,2,"Smoke.Recorded","Smoke.NotRecorded");
			check.Sheet(S2,EhrMeasureType.Smoking,1,2,"Smoke.Recorded","Smoke.NotRecorded");
			check.Finish();
		}

		[TestMethod]
		public void Smoking_APatientOf12AtTheVisit_IsNotInTheDenominator_D03() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//The same sheets: age 13 or older seen in the period, so a patient of 12 at the visit (and all year) is not counted. No sheet of the
			//batches names another day for the age. EHR4B-D03, fixed by EHR-4c: the engine takes the age at the last visit in the period (it took it
			//on today's date: 0/1). Modified Stage 2's Smoking, which has no sheet (D12), counts as Stage 2's.
			check.Sheet(S1,EhrMeasureType.Smoking,0,0,"Smoke.Age12");
			check.Sheet(S2,EhrMeasureType.Smoking,0,0,"Smoke.Age12");
			check.Sheet(MS2,EhrMeasureType.Smoking,0,0,"Smoke.Age12");
			check.Finish();
		}

		[TestMethod]
		public void ModifiedStage2_TheMeasuresWithNoSheet_CountAsStage2Does_D12() {
			MeasureSpecFixture.Begin();
			//No sheet of Modified Stage 2 (MedicaidEPModStage2_2018_Obj1.pdf to Obj10.pdf, July 2018; nor the 2017 edition) has demographics, vital
			//signs, smoking status or a summary of care provided by any means; the engine still calculates them (EHR4B-D12), with Stage 2's
			//thresholds. They count exactly as Stage 2's, whose sheets the cases above check.
			foreach(EhrMeasureType measureType in new[] { EhrMeasureType.Demographics,EhrMeasureType.VitalSigns,EhrMeasureType.Smoking,
				EhrMeasureType.SummaryOfCare })
			{
				EhrMeasure ehrMeasureStage2=MeasureSpecFixture.GetSelectAll(S2).Single(x => x.MeasureType==measureType);
				EhrMeasure ehrMeasureModified=MeasureSpecFixture.GetSelectAll(MS2).Single(x => x.MeasureType==measureType);
				Assert.IsTrue(ehrMeasureStage2.Denominator>0,measureType+": the fixture has patients for it.");
				Assert.AreEqual(ehrMeasureStage2.Numerator+"/"+ehrMeasureStage2.Denominator,ehrMeasureModified.Numerator+"/"+ehrMeasureModified.Denominator,
					measureType.ToString());
				Assert.AreEqual(ehrMeasureStage2.PercentThreshold,ehrMeasureModified.PercentThreshold,measureType.ToString());
			}
		}
		#endregion Demographics, vital signs, smoking

		#region Orders and prescriptions
		[TestMethod]
		public void Cpoe_OrdersThroughCpoe_Meet_OtherOrdersMiss() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//1_CPOE_for_Medication_Orders.pdf, Stage 1 (2014 Definition), May 2014, NUMERATOR / DENOMINATOR. DENOMINATOR: unique patients with at
			//least one medication in their medication list seen by the EP. NUMERATOR: those with at least one medication order entered using CPOE.
			//The patient with no medication is in neither.
			check.Sheet(S1,EhrMeasureType.ProvOrderEntry,1,2,"Cpoe.ListCpoe","Cpoe.ListNotCpoe","Cpoe.NoMeds");
			//The same sheet, OPTIONAL NUMERATOR / DENOMINATOR: medication orders created by the EP in the period; those entered using CPOE.
			check.Sheet(S1,EhrMeasureType.CPOE_MedOrdersOnly,1,2,"Order.Cpoe","Order.NotCpoe");
			//Stage2_EPCore_1_CPOE_MedicationOrders.pdf, Date issued: October, 2012, Measure 1: Medication, and MedicaidEPModStage2_2018_Obj3.pdf,
			//July 2018, Measure 1. DENOMINATOR: medication orders created by the EP in the period. NUMERATOR: those recorded using CPOE.
			check.Sheet(S2,EhrMeasureType.CPOE_MedOrdersOnly,1,2,"Order.Cpoe","Order.NotCpoe");
			check.Sheet(MS2,EhrMeasureType.CPOE_MedOrdersOnly,1,2,"Order.Cpoe","Order.NotCpoe");
			//The same sheets, laboratory orders (Stage 2 Measure 3, Modified Stage 2 Measure 2) and radiology orders (Stage 2 Measure 2, Modified
			//Stage 2 Measure 3): orders created by the EP in the period; those recorded using CPOE. A radiology order is not a laboratory order.
			foreach(EhrMeasureStage stage in new[] { S2,MS2 }) {
				check.Sheet(stage,EhrMeasureType.CPOE_LabOrdersOnly,1,2,"CpoeLab.Cpoe","CpoeLab.NotCpoe");
				check.Sheet(stage,EhrMeasureType.CPOE_RadiologyOrdersOnly,1,2,"Rad.Cpoe","Rad.NotCpoe");
				check.Sheet(stage,EhrMeasureType.CPOE_LabOrdersOnly,0,0,"Rad.Cpoe","Rad.NotCpoe");
				check.Sheet(stage,EhrMeasureType.CPOE_RadiologyOrdersOnly,0,0,"CpoeLab.Cpoe","CpoeLab.NotCpoe");
			}
			check.Finish();
		}

		[TestMethod]
		public void EPrescribing_FourPrescriptionsThreeElectronic_Is75Percent_ControlledNotCounted() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			string[] arrayKeys=new[] { "Rx.Three","Rx.Printed","Rx.Controlled" };
			//4_e-prescribing.pdf, Stage 1 (2014 Definition), May 2014, NUMERATOR / DENOMINATOR / EXCLUSION. DENOMINATOR: prescriptions written for
			//drugs requiring a prescription, other than controlled substances, in the period. NUMERATOR: those generated and transmitted electronically.
			check.Sheet(S1,EhrMeasureType.Rx,3,4,arrayKeys);
			//Stage2_EPCore_2_ePrescribing.pdf, Date issued: November, 2014 (its first denominator, without controlled substances), and
			//MedicaidEPModStage2_2018_Obj4.pdf, July 2018 (permissible prescriptions). NUMERATOR: generated, queried for a drug formulary and
			//transmitted electronically; the engine has no record of the formulary query and counts those transmitted (EHR4B-D14, not a number).
			check.Sheet(S2,EhrMeasureType.Rx,3,4,arrayKeys);
			check.Sheet(MS2,EhrMeasureType.Rx,3,4,arrayKeys);
			check.Finish();
			//The plan's example (section 7, EHR-4b): 4 prescriptions, 3 electronic, gives 75%, more than Stage 2's 50.
			MeasureSpecFixture.Count(S2,EhrMeasureType.Rx,arrayKeys,out int numerator,out int denominator);
			Assert.AreEqual(75,numerator*100/denominator);
		}
		#endregion Orders and prescriptions

		#region Laboratory results and images
		[TestMethod]
		public void LabResults_IncorporatedAsStructuredData_Meet_NotIncorporatedMiss() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//2_Clinical_Lab_Test_Results.pdf, Stage 1 (2014 Definition), May 2014, and Stage2_EPCore_10_ClinicalLabTestResults.pdf, Date issued:
			//November, 2014, Attestation Requirements. DENOMINATOR: lab tests ordered by the EP in the period whose results are a positive or negative
			//affirmation or a number. NUMERATOR: those results incorporated as structured data. A numeric result and a coded positive/negative one
			//incorporated, a test whose result was not incorporated, and the older lab orders with and without a panel. An imaging order is no lab
			//test here.
			string[] arrayKeys=new[] { "Lab.Numeric","Lab.Coded","Lab.NoResult","Lab.OldPanel","Lab.OldNoPanel" };
			check.Sheet(S1,EhrMeasureType.Lab,3,5,arrayKeys);
			check.Sheet(S2,EhrMeasureType.Lab,3,5,arrayKeys);
			check.Sheet(S1,EhrMeasureType.Lab,0,0,"Rad.Cpoe","Rad.NotCpoe");
			check.Sheet(S2,EhrMeasureType.Lab,0,0,"Rad.Cpoe","Rad.NotCpoe");
			check.Finish();
		}

		[TestMethod]
		public void LabResults_AFreeTextResult_IsNotInTheDenominator_D11() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//The same sheets: only tests whose results are a positive or negative affirmation or a number are in the denominator. A test with a
			//free-text result is in none. EHR4B-D11, fixed by EHR-4c: the engine leaves out an order whose results are neither (it counted every
			//order: 0/1); an order with no result yet stays in (Lab.NoResult in the case above).
			check.Sheet(S1,EhrMeasureType.Lab,0,0,"Lab.Text");
			check.Sheet(S2,EhrMeasureType.Lab,0,0,"Lab.Text");
			check.Finish();
		}

		[TestMethod]
		public void ImagingResults_Stage2_AnAttachedImage_Meets_AnExpectedOneMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//Stage2_EPMenu_3_ImagingResults.pdf, Date issued: October, 2012, Attestation Requirements. DENOMINATOR: tests whose result is one or more
			//images ordered by the EP in the period. NUMERATOR: those results accessible through CEHRT. One image attached, one still expected.
			check.Sheet(S2,EhrMeasureType.LabImages,1,2,"Img.Attached","Img.Expected");
			check.Finish();
		}
		#endregion Laboratory results and images

		#region Patient access, summaries, education, reminders
		[TestMethod]
		public void OnlineAccess_WithinFourBusinessDays_Meets_LaterOrNoneMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//11_Patient_Electronic_Access-.pdf, Stage 1 (2014 Definition), May 2014; Stage2_EPCore_7_PatientElectronicAccess.pdf, Date issued: August,
			//2014, Measure 1; MedicaidEPModStage2_2018_Obj8.pdf, July 2018, Measure 1. DENOMINATOR: unique patients seen. NUMERATOR: those with
			//timely online access (within 4 business days after the information is available to the EP). Seen on a Monday: access the same day and
			//on the Thursday meet; on the Tuesday of the next week (the sixth business day), and none, miss.
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				check.Sheet(stage,EhrMeasureType.ElectronicCopyAccess,2,4,"Access.SameDay","Access.Thursday","Access.NextTuesday","Access.None");
			}
			check.Finish();
		}

		[TestMethod]
		public void OnlineAccess_ATuesdayVisit_TheNextMondayIsTheFourthBusinessDay_D01() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//The same sheets: seen on a Tuesday, access on the next Monday (Wednesday, Thursday, Friday, Monday: the fourth business day) is timely.
			//EHR4B-D01, fixed by EHR-4c: the engine counts 4 business days, Monday to Friday (its deadline for a Tuesday visit was the Saturday: 0/1).
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				check.Sheet(stage,EhrMeasureType.ElectronicCopyAccess,1,1,"Access.TuesdayMonday");
			}
			check.Finish();
		}

		[TestMethod]
		public void OnlineAccess_OnTheFourthBusinessDay_IsTimely_D15() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//The same sheets: seen on a Monday, access on the Friday (the fourth business day, at 11:00) is timely. EHR4B-D15, fixed by EHR-4c: the
			//engine compares dates (it compared the event's date and time with midnight of the last day: 0/1).
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				check.Sheet(stage,EhrMeasureType.ElectronicCopyAccess,1,1,"Access.Friday");
			}
			check.Finish();
		}

		[TestMethod]
		public void ViewDownloadTransmit_APatientsActionInThePeriod_Meets_NoneMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//Stage2_EPCore_7_PatientElectronicAccess.pdf, Date issued: August, 2014, Measure 2, and MedicaidEPModStage2_2018_Obj8.pdf, July 2018,
			//Measure 2. DENOMINATOR: unique patients seen. NUMERATOR: those (or their authorized representatives) who view, download or transmit
			//their health information. The engine's event for it is ElectronicCopyRequested.
			check.Sheet(S2,EhrMeasureType.ElectronicCopy,1,2,"Vdt.Viewed","Vdt.None");
			check.Sheet(MS2,EhrMeasureType.ElectronicCopy,1,2,"Vdt.Viewed","Vdt.None");
			//MedicaidEPModStage2_2018_Obj8.pdf, Additional Information: the action counts only within the reporting period when it is a full
			//calendar year, so an action of the year before does not. (The Stage 2 sheet states no time for it, so Stage 2 is not compared.)
			check.Sheet(MS2,EhrMeasureType.ElectronicCopy,0,1,"Vdt.Before");
			check.Finish();
		}

		[TestMethod]
		public void ClinicalSummaries_WithinTheStagesBusinessDays_Meet_LaterOrNoneMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//13_Clinical_Summaries.pdf, Stage 1 (2014 Definition), May 2014, NUMERATOR / DENOMINATOR / EXCLUSION. DENOMINATOR: office visits by the EP
			//in the period. NUMERATOR: those for which the patient is provided a clinical summary within three business days. Visits on a Monday with
			//a summary the same day, the next day, the Thursday (all three met), the Friday (the fourth business day) and none; a visit on a Friday
			//with the summary on the Monday (met).
			check.Sheet(S1,EhrMeasureType.ClinicalSummaries,4,6,"ClinSum.SameDay","ClinSum.NextDay","ClinSum.Thursday","ClinSum.Friday",
				"ClinSum.None","ClinSum.FridayMonday");
			//Stage2_EPCore_8_ClinicalSummaries.pdf, Date issued: November, 2014, Attestation Requirements: the same, within one business day: of
			//these, only the same day.
			check.Sheet(S2,EhrMeasureType.ClinicalSummaries,1,4,"ClinSum.SameDay","ClinSum.Thursday","ClinSum.Friday","ClinSum.None");
			//One patient, two office visits, a summary on the day of the first only. 13_Clinical_Summaries.pdf (May 2014): one of the two visits.
			check.Sheet(S1,EhrMeasureType.ClinicalSummaries,1,2,"ClinSum.TwoVisits");
			check.Finish();
		}

		[TestMethod]
		public void ClinicalSummaries_Stage2_ASummaryOfAnEarlierVisit_DoesNotCountForALaterOne_D02() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//Stage2_EPCore_8_ClinicalSummaries.pdf (November 2014): one of the two visits had its summary within a business day. EHR4B-D02, fixed by
			//EHR-4c: the engine compares each visit with the first summary on or after its own day (it compared each visit with the patient's first
			//summary ever, which is before the second visit's deadline too: 2/2).
			check.Sheet(S2,EhrMeasureType.ClinicalSummaries,1,2,"ClinSum.TwoVisits");
			check.Finish();
		}

		[TestMethod]
		public void ClinicalSummaries_Stage2_ASummaryDuringTheNextBusinessDay_IsWithinOne_D15() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//Stage2_EPCore_8_ClinicalSummaries.pdf (November 2014): a summary the next business day (the Tuesday after a Monday visit, the Monday
			//after a Friday visit) is within one business day. EHR4B-D15, fixed by EHR-4c: the engine compares dates (it compared the event's date
			//and time with midnight of the last day: 0/1 each).
			check.Sheet(S2,EhrMeasureType.ClinicalSummaries,1,1,"ClinSum.NextDay");
			check.Sheet(S2,EhrMeasureType.ClinicalSummaries,1,1,"ClinSum.FridayMonday");
			check.Finish();
		}

		[TestMethod]
		public void Education_ResourcesProvided_Meet_NoneMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//6_Patient-Specific_Education_Resources.pdf, Stage 1 (2014 Definition), May 2014; Stage2_EPCore_13_PatientSpecificEdRes.pdf, Date
			//issued: October, 2012; MedicaidEPModStage2_2018_Obj6.pdf, July 2018. DENOMINATOR: unique patients seen (with office visits). NUMERATOR:
			//those provided patient-specific education resources.
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				check.Sheet(stage,EhrMeasureType.Education,1,2,"Edu.Given","Edu.None");
			}
			check.Finish();
		}

		[TestMethod]
		public void Education_ModifiedStage2_ResourcesOfTheYearBefore_DoNotCount_D10() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//MedicaidEPModStage2_2018_Obj6.pdf, July 2018, Additional Information: the action counts only within the reporting period when it is a
			//full calendar year (the 2017 edition, MedicaidEPStage2_Obj6.pdf of November 2016: no earlier than the start of the period's calendar
			//year). Resources given the year before do not count. EHR4B-D10, fixed by EHR-4c: the engine counts them within the period's calendar
			//year (it counted them at any date: 1/1). The Stage 1 and Stage 2 sheets give no time for it, so they are not compared.
			check.Sheet(MS2,EhrMeasureType.Education,0,1,"Edu.Before");
			check.Finish();
		}

		[TestMethod]
		public void Reminders_Stage1_65OrOlderOr5OrYounger_AReminderMeets() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//4_Patient_Reminders.pdf, Stage 1 (2014 Definition), May 2014, NUMERATOR / DENOMINATOR / EXCLUSION. DENOMINATOR: unique patients 65 years
			//old or older or 5 years old or younger. NUMERATOR: those sent the appropriate reminder. Two of 68 (one reminded), a child of 3
			//(reminded), and an adult of 33 (reminded; in no denominator).
			check.Sheet(S1,EhrMeasureType.Reminders,2,3,"Rem1.Senior","Rem1.SeniorNone","Rem1.Child3","Rem1.Adult");
			check.Finish();
		}

		[TestMethod]
		public void Reminders_Stage1_AChildOf5AllYear_IsInTheDenominator_D06() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//The same sheet: "5 years old or younger" includes a child who is 5 on every day of the period. EHR4B-D06, fixed by EHR-4c: the engine
			//takes 5 or younger at the start of the period (it took younger than 5: 0/0).
			check.Sheet(S1,EhrMeasureType.Reminders,1,1,"Rem1.Age5");
			check.Finish();
		}

		[TestMethod]
		public void PreventiveCare_Stage2_TwoVisitsInThe24MonthsBefore_AReminderMeets() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//Stage2_EPCore_12_PreventiveCare.pdf, Date issued: October, 2012, Attestation Requirements. DENOMINATOR: unique patients with two or more
			//office visits with the EP in the 24 months before the period. NUMERATOR: those sent a reminder in the period. Two such patients (one
			//reminded), and one with a single visit (reminded; in no denominator).
			check.Sheet(S2,EhrMeasureType.Reminders,1,2,"Rem2.TwoVisits","Rem2.TwoVisitsNone","Rem2.OneVisit");
			check.Finish();
		}

		[TestMethod]
		public void PreventiveCare_Stage2_TheVisitsAreWithTheEP_NotThePrimaryProvider_D07() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//The same sheet: the two visits are with the EP. Two visits with the EP and another primary provider: in the denominator (reminded, met).
			//Two visits with another provider and the EP as primary provider: in none. EHR4B-D07, fixed by EHR-4c: the engine counts the visits with
			//the EP, whoever the primary provider is (it took the EP's primary patients and visits with any provider: 0/0 and 1/1).
			check.Sheet(S2,EhrMeasureType.Reminders,1,1,"Rem2.OtherPrimary");
			check.Sheet(S2,EhrMeasureType.Reminders,0,0,"Rem2.VisitsWithOther");
			check.Finish();
		}
		#endregion Patient access, summaries, education, reminders

		#region Transitions of care, messages, history, notes
		[TestMethod]
		public void MedReconcile_AReconciledTransition_Meets_AnotherMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//7_Medication_Reconciliation.pdf, Stage 1 (2014 Definition), May 2014; Stage2_EPCore_14_MedicationReconciliation.pdf, Date issued:
			//October, 2012; MedicaidEPModStage2_2018_Obj7.pdf, July 2018. DENOMINATOR: transitions of care in the period for which the EP was the
			//receiving party. NUMERATOR: those where medication reconciliation was performed.
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				check.Sheet(stage,EhrMeasureType.MedReconcile,1,2,"MedRec.Reconciled","MedRec.NotReconciled");
			}
			check.Finish();
		}

		[TestMethod]
		public void MedReconcile_TheDenominatorCountsTransitions_NotPatientsSeen_D08() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//The same sheets: one patient with two transitions of care and one reconciliation is 1 of 2 transitions; the engine counts the patient
			//once, met only when every transition was reconciled. And a transition received by the EP and reconciled counts whether or not the
			//patient had an office visit in the period. EHR4B-D08, fixed by EHR-4c: the engine counts the transitions received by the EP in the
			//period, each reconciliation for one of them (it counted the patients seen by the EP: 0/1 and 0/0).
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				check.Sheet(stage,EhrMeasureType.MedReconcile,1,2,"MedRec.TwoOne");
				check.Sheet(stage,EhrMeasureType.MedReconcile,1,1,"MedRec.NoVisit");
			}
			check.Finish();
		}

		[TestMethod]
		public void SummaryOfCare_AReferralWithASummary_Meets_OneWithoutMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			string[] arrayKeys=new[] { "Soc.Sent","Soc.NotSent","Soc.Electronic" };
			//8_Transition_of_Care_Summary.pdf, Stage 1 (2014 Definition), May 2014, and Stage2_EPCore_15_SummaryCare.pdf, Last Updated: August, 2015,
			//MEASURE 1. DENOMINATOR: transitions of care and referrals in the period for which the EP was the transferring or referring provider.
			//NUMERATOR: those where a summary of care record was provided. Modified Stage 2 has no sheet for it (EHR4B-D12).
			check.Sheet(S1,EhrMeasureType.SummaryOfCare,2,3,arrayKeys);
			check.Sheet(S2,EhrMeasureType.SummaryOfCare,2,3,arrayKeys);
			//Stage2_EPCore_15_SummaryCare.pdf, MEASURE 2, and MedicaidEPModStage2_2018_Obj5.pdf, July 2018: the same denominator; those where the
			//summary of care record was transmitted (exchanged) electronically.
			check.Sheet(S2,EhrMeasureType.SummaryOfCareElectronic,1,3,arrayKeys);
			check.Sheet(MS2,EhrMeasureType.SummaryOfCareElectronic,1,3,arrayKeys);
			check.Finish();
		}

		[TestMethod]
		public void SecureMessaging_AMessageInThePeriod_Meets_NoneMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//Stage2_EPCore_17_UseSecureElectronicMessaging.pdf, Date issued: October, 2012, Attestation Requirements. DENOMINATOR: unique patients
			//seen. NUMERATOR: those who send a secure electronic message to the EP during the period.
			check.Sheet(S2,EhrMeasureType.SecureMessaging,1,2,"Msg.FromPatient","Msg.None");
			//MedicaidEPModStage2_2018_Obj9.pdf, July 2018: those for whom a secure message was sent to the patient (or in response to one of theirs)
			//in the period. A message from the patient alone, with no reply, does not count (Additional Information: a provider counts a patient's
			//message only when the provider responds).
			check.Sheet(MS2,EhrMeasureType.SecureMessaging,1,3,"Web.Sent","Web.None","Msg.FromPatient");
			check.Finish();
		}

		[TestMethod]
		public void SecureMessaging_Stage2_AMessageOfTheYearBefore_DoesNotCount_D09() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//Stage2_EPCore_17_UseSecureElectronicMessaging.pdf, October 2012: the message is sent during the reporting period. A patient seen whose
			//only message is of the year before is 0 of 1. EHR4B-D09, fixed by EHR-4c: the engine counts a message of the period (it counted one of
			//any date: 1/1).
			check.Sheet(S2,EhrMeasureType.SecureMessaging,0,1,"Msg.Before");
			check.Finish();
		}

		[TestMethod]
		public void FamilyHistoryAndElectronicNotes_Stage2_AnEntry_Meets_NoneMisses() {
			MeasureSpecFixture.Begin();
			SpecCheck check=new SpecCheck();
			//Stage2_EPMenu_4_FamilyHealthHistory.pdf, Date issued: October, 2012, Attestation Requirements. DENOMINATOR: unique patients seen.
			//NUMERATOR: those with a structured entry for one or more first-degree relatives (a parent).
			check.Sheet(S2,EhrMeasureType.FamilyHistory,1,2,"Fam.Parent","Fam.None");
			//Stage2_EPMenu_2_ElectronicNotes.pdf, Date issued: November, 2014, Attestation Requirements and Additional Information. DENOMINATOR: unique
			//patients with at least one office visit in the period. NUMERATOR: those with at least one electronic progress note from an EP, which the
			//EP signs: a signed note meets, an unsigned one and none miss.
			check.Sheet(S2,EhrMeasureType.ElectronicNote,1,3,"Note.Signed","Note.Unsigned","Note.None");
			check.Finish();
		}
		#endregion Transitions of care, messages, history, notes
	}
}
