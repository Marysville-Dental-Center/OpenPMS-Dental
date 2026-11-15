// The EHR window's per-patient summary against Measure Calc (package EHR-4d, the follow-up of EHR-4c; docs/port-notes/EHR-4.md, section
// "EHR-4d"). EhrMeasures.GetMu, GetMu2 and GetMu2Mod fill the grid of FormEHR for one patient; since EHR-4d they take a period (the window's is
// the year before today, to today) and the rules EHR-4c gave Measure Calc: the ages at the last visit (smoking, vital signs) or at the period's
// start (Stage 1 reminders), the business-day deadlines, the transitions, lab results, messages and education as GetTable, GetTableMu2 and
// GetTableMu2Mod count them. Each case says what the window gives and checks that Measure Calc says the same of the patient: N/A when the
// measure's audit table for the EP and the period has no row of the patient, met when every row of the patient is met, not met otherwise. The
// comments name the cases where the window's old rules gave something else. The fixture is MeasureSpecFixture's (this file is a part of
// MeasureSpecTests, so the scratch database is built once and dropped once). Synthetic data only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace EhrTests {
	public partial class MeasureSpecTests {
		///<summary>The window's rows of each stage, patient and period, fetched once per run.</summary>
		private static readonly Dictionary<string,List<EhrMu>> _dictWindow=new Dictionary<string,List<EhrMu>>();

		///<summary>Compares the window with Measure Calc, case by case, and with the value the case expects; Finish fails on any difference.</summary>
		private sealed class WindowCheck {
			private readonly List<string> _listFailures=new List<string>();

			///<summary>The window and Measure Calc both give the value for each patient under the keys, over the fixture's 2014 period.</summary>
			public void Agree(EhrMeasureStage stage,EhrMeasureType measureType,MuMet muMet,params string[] arrayKeys) {
				AgreeInPeriod(stage,measureType,MeasureSpecFixture.DateStart,MeasureSpecFixture.DateEnd,muMet,arrayKeys);
			}

			public void AgreeInPeriod(EhrMeasureStage stage,EhrMeasureType measureType,DateTime dateStart,DateTime dateEnd,MuMet muMet,
				params string[] arrayKeys)
			{
				foreach(string key in arrayKeys) {
					Patient patient=MeasureSpecFixture.GetPatient(key);
					MuMet muMetWindow=GetWindowMet(stage,measureType,patient,dateStart,dateEnd);
					MuMet muMetCalc=GetCalcMet(stage,measureType,patient,dateStart,dateEnd);
					if(muMetWindow!=muMet || muMetCalc!=muMet) {
						_listFailures.Add(stage+" "+measureType+" "+key+" ("+dateStart.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture)+" to "
							+dateEnd.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture)+"): expected "+muMet+", the window gives "+muMetWindow
							+", Measure Calc "+muMetCalc+".");
					}
				}
			}

			public void Finish() {
				if(_listFailures.Count>0) {
					Assert.Fail(string.Join("\r\n",_listFailures));
				}
			}
		}

		///<summary>The window's Met of the measure for the patient over the period: GetMu (Stage 1), GetMu2 (Stage 2) or GetMu2Mod.</summary>
		private static MuMet GetWindowMet(EhrMeasureStage stage,EhrMeasureType measureType,Patient patient,DateTime dateStart,DateTime dateEnd) {
			string key=stage+"."+patient.PatNum+"."+dateStart.ToString("yyyyMMdd",CultureInfo.InvariantCulture)+"."
				+dateEnd.ToString("yyyyMMdd",CultureInfo.InvariantCulture);
			if(!_dictWindow.TryGetValue(key,out List<EhrMu> listEhrMus)) {
				patient=Patients.GetPat(patient.PatNum);//As FormEHR reads it: the fixture's object has null where the database has "".
				switch(stage) {
					case EhrMeasureStage.Stage1:
						listEhrMus=EhrMeasures.GetMu(patient,dateStart,dateEnd);
						break;
					case EhrMeasureStage.Stage2:
						listEhrMus=EhrMeasures.GetMu2(patient,dateStart,dateEnd);
						break;
					default:
						listEhrMus=EhrMeasures.GetMu2Mod(patient,dateStart,dateEnd);
						break;
				}
				_dictWindow[key]=listEhrMus;
			}
			EhrMu ehrMu=listEhrMus.FirstOrDefault(x => x.MeasureType==measureType);
			Assert.IsNotNull(ehrMu,stage+": the window has a row for "+measureType+".");
			return ehrMu.Met;
		}

		///<summary>What Measure Calc says of the patient: N/A when the audit table of the measure for the EP and the period has no row of the patient,
		///True when every row of the patient is met, False otherwise.</summary>
		private static MuMet GetCalcMet(EhrMeasureStage stage,EhrMeasureType measureType,Patient patient,DateTime dateStart,DateTime dateEnd) {
			string patNum=patient.PatNum.ToString(CultureInfo.InvariantCulture);
			List<DataRow> listRows=MeasureSpecFixture.GetAudit(stage,measureType,dateStart,dateEnd).Rows.Cast<DataRow>()
				.Where(x => x["PatNum"].ToString()==patNum).ToList();
			if(listRows.Count==0) {
				return MuMet.NA;
			}
			if(listRows.All(x => x["met"].ToString()=="X")) {
				return MuMet.True;
			}
			return MuMet.False;
		}

		#region The EHR window (EHR-4d)
		[TestMethod]
		public void EhrWindow_AgesOnTheLastVisit_AsMeasureCalc() {
			MeasureSpecFixture.Begin();
			WindowCheck check=new WindowCheck();
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				//Smoking (EHR4B-D03): 13 or older at the last visit. Smoke.Age12 is 12 all 2014; the old window took today's age (in no case under 13
				//now) and gave not met.
				check.Agree(stage,EhrMeasureType.Smoking,MuMet.True,"Smoke.Recorded");
				check.Agree(stage,EhrMeasureType.Smoking,MuMet.False,"Smoke.NotRecorded");
				check.Agree(stage,EhrMeasureType.Smoking,MuMet.NA,"Smoke.Age12");
			}
			//Stage 1 VitalSigns (the 2013 edition, EHR4B-D04, D05): age 2 or older at the last visit, height, weight and blood pressure. Vs.Infant (1)
			//is in no denominator: the old window gave not met (no blood pressure, today's age over 3).
			check.Agree(S1,EhrMeasureType.VitalSigns,MuMet.True,"Vs.Full");
			check.Agree(S1,EhrMeasureType.VitalSigns,MuMet.False,"Vs.HeightWeight","Vs.Child2","Vs.Bp","Vs.None");
			check.Agree(S1,EhrMeasureType.VitalSigns,MuMet.NA,"Vs.Infant");
			//VitalSigns2014, and Stage 2's and Modified Stage 2's VitalSigns: blood pressure from age 3 at the last visit. Vs.Child2 (2) and Vs.Infant
			//(1) are met with height and weight; the old window asked their blood pressure (today's age) and gave not met.
			foreach(Tuple<EhrMeasureStage,EhrMeasureType> tuple in new[] {
				Tuple.Create(S1,EhrMeasureType.VitalSigns2014),Tuple.Create(S2,EhrMeasureType.VitalSigns),Tuple.Create(MS2,EhrMeasureType.VitalSigns) })
			{
				check.Agree(tuple.Item1,tuple.Item2,MuMet.True,"Vs.Full","Vs.Child2","Vs.Infant");
				check.Agree(tuple.Item1,tuple.Item2,MuMet.False,"Vs.HeightWeight","Vs.Bp","Vs.None");
			}
			//The blood pressure measures: 3 or older at the last visit; under 3 is N/A (the old window: not met).
			foreach(EhrMeasureStage stage in new[] { S1,S2 }) {
				check.Agree(stage,EhrMeasureType.VitalSignsBPOnly,MuMet.True,"Vs.Full","Vs.Bp");
				check.Agree(stage,EhrMeasureType.VitalSignsBPOnly,MuMet.False,"Vs.HeightWeight","Vs.None");
				check.Agree(stage,EhrMeasureType.VitalSignsBPOnly,MuMet.NA,"Vs.Child2","Vs.Infant");
			}
			check.Finish();
		}

		[TestMethod]
		public void EhrWindow_BusinessDayDeadlines_AsMeasureCalc() {
			MeasureSpecFixture.Begin();
			WindowCheck check=new WindowCheck();
			//Stage 1 clinical summaries: within 3 business days (EHR4B-D16). ClinSum.SaturdayWednesday: the old window's deadline after a Saturday
			//was the Tuesday (not met).
			check.Agree(S1,EhrMeasureType.ClinicalSummaries,MuMet.True,"ClinSum.SameDay","ClinSum.NextDay","ClinSum.Thursday","ClinSum.FridayMonday",
				"ClinSum.SaturdayWednesday");
			check.Agree(S1,EhrMeasureType.ClinicalSummaries,MuMet.False,"ClinSum.Friday","ClinSum.None","ClinSum.SaturdayThursday","ClinSum.TwoVisits");
			//Stage 2: within 1 business day (EHR4B-D02, D15). Win.SaturdayMonday (2015): the old window's deadline after a Saturday was the Sunday.
			check.Agree(S2,EhrMeasureType.ClinicalSummaries,MuMet.True,"ClinSum.SameDay","ClinSum.NextDay","ClinSum.FridayMonday");
			check.Agree(S2,EhrMeasureType.ClinicalSummaries,MuMet.False,"ClinSum.Thursday","ClinSum.Friday","ClinSum.None","ClinSum.SaturdayWednesday",
				"ClinSum.SaturdayThursday","ClinSum.TwoVisits");
			foreach(EhrMeasureStage stage in new[] { S1,S2 }) {
				check.AgreeInPeriod(stage,EhrMeasureType.ClinicalSummaries,MeasureSpecFixture.DateStart2015,MeasureSpecFixture.DateEnd2015,MuMet.True,
					"Win.SaturdayMonday");
			}
			//Online access, the three stages: the first access no later than the fourth business day after the first visit (EHR4B-D01, D15). The
			//old window met any access, whenever given: Access.NextTuesday (the sixth business day) is not met.
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				check.Agree(stage,EhrMeasureType.ElectronicCopyAccess,MuMet.True,"Access.SameDay","Access.Thursday","Access.Friday","Access.TuesdayMonday");
				check.Agree(stage,EhrMeasureType.ElectronicCopyAccess,MuMet.False,"Access.NextTuesday","Access.None");
			}
			check.Finish();
		}

		[TestMethod]
		public void EhrWindow_Reminders_AsMeasureCalc() {
			MeasureSpecFixture.Begin();
			WindowCheck check=new WindowCheck();
			//Stage 1: 65 or older or 5 or younger at the start of the period (EHR4B-D06). The old window took today's age: Rem1.Child3 and Rem1.Age5
			//were N/A, and Rem1.Turns65 (64 at the start of 2015) was not met.
			check.Agree(S1,EhrMeasureType.Reminders,MuMet.True,"Rem1.Senior","Rem1.Child3","Rem1.Age5");
			check.Agree(S1,EhrMeasureType.Reminders,MuMet.False,"Rem1.SeniorNone");
			check.Agree(S1,EhrMeasureType.Reminders,MuMet.NA,"Rem1.Adult");
			check.AgreeInPeriod(S1,EhrMeasureType.Reminders,MeasureSpecFixture.DateStart2015,MeasureSpecFixture.DateEnd2015,MuMet.NA,"Rem1.Turns65");
			//Stage 2: two or more visit days in the 24 months before the period (the old window: the two years before today, the period included).
			//The window counts visits with any provider (it has no EP), so Rem2.VisitsWithOther is not compared.
			check.Agree(S2,EhrMeasureType.Reminders,MuMet.True,"Rem2.TwoVisits","Rem2.OtherPrimary");
			check.Agree(S2,EhrMeasureType.Reminders,MuMet.False,"Rem2.TwoVisitsNone");
			check.Agree(S2,EhrMeasureType.Reminders,MuMet.NA,"Rem2.OneVisit");
			check.Finish();
		}

		[TestMethod]
		public void EhrWindow_TransitionsLabsMessagesEducation_AsMeasureCalc() {
			MeasureSpecFixture.Begin();
			WindowCheck check=new WindowCheck();
			//Medication reconciliation, the three stages (EHR4B-D08): each reconciliation counts for one transition dated on or before it.
			//Win.ReconcileBefore (2015) was reconciled before the transition: the old window compared the counts and gave met.
			foreach(EhrMeasureStage stage in new[] { S1,S2,MS2 }) {
				check.Agree(stage,EhrMeasureType.MedReconcile,MuMet.True,"MedRec.Reconciled","MedRec.NoVisit");
				check.Agree(stage,EhrMeasureType.MedReconcile,MuMet.False,"MedRec.NotReconciled","MedRec.TwoOne");
				check.AgreeInPeriod(stage,EhrMeasureType.MedReconcile,MeasureSpecFixture.DateStart2015,MeasureSpecFixture.DateEnd2015,MuMet.False,
					"Win.ReconcileBefore");
			}
			//Lab results, Stage 1 and Stage 2 (EHR4B-D11): an order with a free-text result only and a radiology order are in no denominator. The
			//old window met Lab.Text (a result is entered) and missed the radiology orders (no result). The window does not list the older
			//medical-order labs, so the Lab.Old cases are not compared.
			foreach(EhrMeasureStage stage in new[] { S1,S2 }) {
				check.Agree(stage,EhrMeasureType.Lab,MuMet.True,"Lab.Numeric","Lab.Coded");
				check.Agree(stage,EhrMeasureType.Lab,MuMet.False,"Lab.NoResult","CpoeLab.Cpoe","CpoeLab.NotCpoe");
				check.Agree(stage,EhrMeasureType.Lab,MuMet.NA,"Lab.Text","Rad.Cpoe","Rad.NotCpoe");
			}
			//Stage 2 secure messaging (EHR4B-D09): a message from the patient in the period. The old window met Msg.Before (a message of 2013).
			check.Agree(S2,EhrMeasureType.SecureMessaging,MuMet.True,"Msg.FromPatient");
			check.Agree(S2,EhrMeasureType.SecureMessaging,MuMet.False,"Msg.None","Msg.Before");
			//Modified Stage 2 secure messaging: web mail sent to the patient in the period's calendar year.
			check.Agree(MS2,EhrMeasureType.SecureMessaging,MuMet.True,"Web.Sent");
			check.Agree(MS2,EhrMeasureType.SecureMessaging,MuMet.False,"Web.None");
			//Education: any date for Stage 1 and Stage 2; Modified Stage 2 the calendar year of a visit in the period (EHR4B-D10). The old window met
			//Edu.Before (resources of 2013) in Modified Stage 2 too, and Edu.CrossBefore in the period across a year boundary.
			foreach(EhrMeasureStage stage in new[] { S1,S2 }) {
				check.Agree(stage,EhrMeasureType.Education,MuMet.True,"Edu.Given","Edu.Before");
				check.Agree(stage,EhrMeasureType.Education,MuMet.False,"Edu.None");
			}
			check.Agree(MS2,EhrMeasureType.Education,MuMet.True,"Edu.Given");
			check.Agree(MS2,EhrMeasureType.Education,MuMet.False,"Edu.None","Edu.Before");
			check.AgreeInPeriod(MS2,EhrMeasureType.Education,MeasureSpecFixture.DateStartCross,MeasureSpecFixture.DateEndCross,MuMet.True,"Edu.CrossAfter");
			check.AgreeInPeriod(MS2,EhrMeasureType.Education,MeasureSpecFixture.DateStartCross,MeasureSpecFixture.DateEndCross,MuMet.False,"Edu.CrossBefore");
			check.Finish();
		}

		[TestMethod]
		public void EhrWindow_WithoutAPeriod_TakesTheYearBeforeToday() {
			MeasureSpecFixture.Begin();
			//FormEHR calls the overloads without a period: the year before today, to today, as the window always took. No fixture row is dated in it,
			//so the visit-based rows are N/A.
			Patient patient=Patients.GetPat(MeasureSpecFixture.GetPatient("ClinSum.SameDay").PatNum);
			List<EhrMu> listEhrMus=EhrMeasures.GetMu(patient);
			List<EhrMu> listEhrMusPeriod=EhrMeasures.GetMu(patient,DateTime.Today.AddYears(-1),DateTime.Today);
			CollectionAssert.AreEqual(listEhrMusPeriod.Select(x => x.MeasureType+" "+x.Met+" "+x.Details).ToList(),
				listEhrMus.Select(x => x.MeasureType+" "+x.Met+" "+x.Details).ToList());
			Assert.AreEqual(MuMet.NA,listEhrMus.First(x => x.MeasureType==EhrMeasureType.ClinicalSummaries).Met);
			Assert.AreEqual(EhrMeasures.GetMu2(patient,DateTime.Today.AddYears(-1),DateTime.Today).Count,EhrMeasures.GetMu2(patient).Count);
			Assert.AreEqual(EhrMeasures.GetMu2Mod(patient,DateTime.Today.AddYears(-1),DateTime.Today).Count,EhrMeasures.GetMu2Mod(patient).Count);
		}
		#endregion The EHR window (EHR-4d)
	}
}
