// The vital signs' age rule and BMI range of E-35 (docs/specs/ehr-2027-measures-plan.md, section 8, package EHR-7), with no database: the age on
// the day taken (CQL's AgeInYearsAt) on the day before and the day of the 3rd, 17th, 18th and 65th birthdays, the BMI percentile at 3 to 17 (a
// percentile at 17 years and 11 months, none at 18), the over- or underweight class from 18, the child counseling group under 18, CMS69v15's
// normal range for every adult (65 and over too), GetBMIPercentile giving -1 outside the LMS table, the same rule through the eForm import
// (EFormVitalSigns.CreateVitalsign), and source checks that the vital sign window takes the age from the one business-layer helper. Synthetic
// patients only; no code is written here (the eForm's codes are checked by being set or empty). A patient without a birth date is an adult
// in the vital sign window (EHR-7-R).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace EhrTests {
	[TestClass]
	public class VitalSignAgeTests {
		private static readonly DateTime _dateBirth=new DateTime(2010,6,15);

		[TestMethod]
		public void Age_OnTheDayTaken_TheDayBeforeAndTheDayOfEachBirthday() {
			foreach(int years in new[] { 3,17,18,65 }) {
				DateTime dateBirthday=_dateBirth.AddYears(years);
				Assert.AreEqual(years-1,Vitalsigns.GetAgeOnDateTaken(_dateBirth,dateBirthday.AddDays(-1)),"The day before birthday "+years);
				Assert.AreEqual(years,Vitalsigns.GetAgeOnDateTaken(_dateBirth,dateBirthday),"Birthday "+years);
			}
			//The day of the visit, not January 1 of its year: on December 31 a patient born in June has had the year's birthday.
			Assert.AreEqual(17,Vitalsigns.GetAgeOnDateTaken(_dateBirth,new DateTime(2027,12,31)));
			Assert.AreEqual(16,Vitalsigns.GetAgeOnDateTaken(_dateBirth,new DateTime(2027,6,14)));
			Assert.AreEqual(0,Vitalsigns.GetAgeOnDateTaken(_dateBirth,_dateBirth));
			//A time of day changes nothing.
			Assert.AreEqual(17,Vitalsigns.GetAgeOnDateTaken(_dateBirth.AddHours(23),_dateBirth.AddYears(17)));
			//Unknown: no birth date, or a day before the birth.
			Assert.AreEqual(-1,Vitalsigns.GetAgeOnDateTaken(DateTime.MinValue,new DateTime(2027,3,1)));
			Assert.AreEqual(-1,Vitalsigns.GetAgeOnDateTaken(_dateBirth,_dateBirth.AddDays(-1)));
			//February 29: the birthday counts from March 1 in a common year, as AgeInYearsAt does.
			DateTime dateLeap=new DateTime(2012,2,29);
			Assert.AreEqual(14,Vitalsigns.GetAgeOnDateTaken(dateLeap,new DateTime(2027,2,28)));
			Assert.AreEqual(15,Vitalsigns.GetAgeOnDateTaken(dateLeap,new DateTime(2027,3,1)));
		}

		[TestMethod]
		public void AgeGroups_PercentileAt3To17_ClassFrom18_ChildGroupUnder18() {
			Assert.IsFalse(Vitalsigns.IsBMIPercentileAge(2));
			Assert.IsTrue(Vitalsigns.IsBMIPercentileAge(3));
			Assert.IsTrue(Vitalsigns.IsBMIPercentileAge(17),"17 has a percentile (the 2014 rule stopped at 16).");
			Assert.IsFalse(Vitalsigns.IsBMIPercentileAge(18));
			Assert.IsFalse(Vitalsigns.IsBMIPercentileAge(-1));
			Assert.IsTrue(Vitalsigns.IsChildCounselingAge(0));
			Assert.IsTrue(Vitalsigns.IsChildCounselingAge(17),"17 is in the child group (once in neither group).");
			Assert.IsFalse(Vitalsigns.IsChildCounselingAge(18));
			Assert.IsFalse(Vitalsigns.IsChildCounselingAge(-1));
			Assert.IsFalse(Vitalsigns.IsWeightClassAge(17));
			Assert.IsTrue(Vitalsigns.IsWeightClassAge(18));
			Assert.IsTrue(Vitalsigns.IsWeightClassAge(65));
			Assert.IsFalse(Vitalsigns.IsWeightClassAge(-1));
			//Through the age on the day: the day before the 18th birthday is the child group with a percentile, the birthday the class.
			int ageBefore18=Vitalsigns.GetAgeOnDateTaken(_dateBirth,_dateBirth.AddYears(18).AddDays(-1));
			Assert.IsTrue(Vitalsigns.IsBMIPercentileAge(ageBefore18) && Vitalsigns.IsChildCounselingAge(ageBefore18) && !Vitalsigns.IsWeightClassAge(ageBefore18));
			int age18=Vitalsigns.GetAgeOnDateTaken(_dateBirth,_dateBirth.AddYears(18));
			Assert.IsTrue(!Vitalsigns.IsBMIPercentileAge(age18) && !Vitalsigns.IsChildCounselingAge(age18) && Vitalsigns.IsWeightClassAge(age18));
			int ageBefore3=Vitalsigns.GetAgeOnDateTaken(_dateBirth,_dateBirth.AddYears(3).AddDays(-1));
			Assert.IsFalse(Vitalsigns.IsBMIPercentileAge(ageBefore3));
			Assert.IsTrue(Vitalsigns.IsBMIPercentileAge(Vitalsigns.GetAgeOnDateTaken(_dateBirth,_dateBirth.AddYears(3))));
		}

		[TestMethod]
		public void BmiClass_The2027RangeForEveryAdult_At65AndOverToo() {
			foreach(int age in new[] { 18,40,64,65,80 }) {
				Assert.AreEqual(InterventionCodeSet.BelowNormalWeight,Vitalsigns.GetBMIInterventionCode(18.4f,age),"Age "+age);
				Assert.AreEqual(InterventionCodeSet.None,Vitalsigns.GetBMIInterventionCode(18.5f,age),"Age "+age);
				Assert.AreEqual(InterventionCodeSet.None,Vitalsigns.GetBMIInterventionCode(22f,age),"Age "+age+" (the 2014 rule called 22 underweight from 65)");
				Assert.AreEqual(InterventionCodeSet.None,Vitalsigns.GetBMIInterventionCode(24.9f,age),"Age "+age);
				Assert.AreEqual(InterventionCodeSet.AboveNormalWeight,Vitalsigns.GetBMIInterventionCode(25f,age),"Age "+age);
				Assert.AreEqual(InterventionCodeSet.AboveNormalWeight,Vitalsigns.GetBMIInterventionCode(27f,age),"Age "+age+" (the 2014 rule called 27 normal from 65)");
			}
			//The day before and the day of the 65th birthday: the same range.
			int age64=Vitalsigns.GetAgeOnDateTaken(_dateBirth,_dateBirth.AddYears(65).AddDays(-1));
			int age65=Vitalsigns.GetAgeOnDateTaken(_dateBirth,_dateBirth.AddYears(65));
			Assert.AreEqual(Vitalsigns.GetBMIInterventionCode(27f,age64),Vitalsigns.GetBMIInterventionCode(27f,age65));
			Assert.AreEqual(Vitalsigns.GetBMIInterventionCode(22f,age64),Vitalsigns.GetBMIInterventionCode(22f,age65));
			//A child is not classed; an unknown age is neither.
			Assert.AreEqual(InterventionCodeSet.Nutrition,Vitalsigns.GetBMIInterventionCode(30f,17));
			Assert.AreEqual(InterventionCodeSet.Nutrition,Vitalsigns.GetBMIInterventionCode(30f,0));
			Assert.AreEqual(InterventionCodeSet.None,Vitalsigns.GetBMIInterventionCode(30f,-1));
		}

		[TestMethod]
		public void Percentile_At17YearsAnd11Months_NoneAt18_MinusOneOutsideTheTable() {
			List<GenderAge_LMS> listLms=Vitalsigns.GetListLMS();
			foreach(PatientGender gender in new[] { PatientGender.Male,PatientGender.Female }) {
				Patient patient=new Patient { Birthdate=_dateBirth,Gender=gender };
				//17 years and 11 months (215 months), the table's last month.
				DateTime dateBefore18=_dateBirth.AddYears(18).AddDays(-1);
				int percentile=Vitalsigns.GetBMIPercentile(21f,patient,dateBefore18,listLms);
				Assert.IsTrue(percentile>=0 && percentile<=100,gender+": "+percentile);
				//The day of the 3rd birthday (36 months), the table's first month.
				percentile=Vitalsigns.GetBMIPercentile(16f,patient,_dateBirth.AddYears(3),listLms);
				Assert.IsTrue(percentile>=0 && percentile<=100,gender+" at 3: "+percentile);
				//Outside the table: -1, not an exception (it threw before EHR-7).
				Assert.AreEqual(-1,Vitalsigns.GetBMIPercentile(21f,patient,_dateBirth.AddYears(18),listLms),gender+" at 18 (216 months)");
				Assert.AreEqual(-1,Vitalsigns.GetBMIPercentile(16f,patient,_dateBirth.AddYears(3).AddDays(-1),listLms),gender+" at 35 months");
				Assert.AreEqual(-1,Vitalsigns.GetBMIPercentile(16f,patient,_dateBirth.AddYears(40),listLms),gender+" at 40");
				Assert.AreEqual(-1,Vitalsigns.GetBMIPercentile(16f,patient,_dateBirth.AddYears(10),new List<GenderAge_LMS>()),"An empty table");
				Assert.AreEqual(-1,Vitalsigns.GetBMIPercentile(0f,patient,_dateBirth.AddYears(10),listLms),"No BMI");
			}
			Assert.AreEqual(-1,Vitalsigns.GetBMIPercentile(16f,new Patient { Birthdate=_dateBirth,Gender=PatientGender.Unknown },_dateBirth.AddYears(10),listLms));
		}

		///<summary>The eForm import takes the same age (EHR-7, E-35): a percentile at 17 and none at 18, the class at 18 and none at 17.</summary>
		[TestMethod]
		public void EFormImport_TakesTheAgeOnTheDayTaken() {
			Patient patient=new Patient { PatNum=7,Birthdate=_dateBirth,Gender=PatientGender.Male };
			Vitalsign vitalsign17=CreateVitalsign(patient,_dateBirth.AddYears(18).AddDays(-1));
			Assert.IsTrue(vitalsign17.BMIPercentile>=0,"17 years and 11 months: a percentile (the 2014 rule had none at 16 on January 1).");
			Assert.AreNotEqual("",vitalsign17.BMIExamCode);
			Assert.AreEqual("",vitalsign17.WeightCode,"Not classed at 17.");
			Vitalsign vitalsign18=CreateVitalsign(patient,_dateBirth.AddYears(18));
			Assert.AreEqual(-1,vitalsign18.BMIPercentile,"None at 18.");
			Assert.AreEqual("",vitalsign18.BMIExamCode);
			Assert.AreNotEqual("",vitalsign18.WeightCode,"Classed overweight on the 18th birthday (BMI 34).");
			Vitalsign vitalsign3=CreateVitalsign(patient,_dateBirth.AddYears(3),height:38,weight:35);
			Assert.IsTrue(vitalsign3.BMIPercentile>=0,"The 3rd birthday: a percentile.");
			Vitalsign vitalsignBefore3=CreateVitalsign(patient,_dateBirth.AddYears(3).AddDays(-1),height:38,weight:35);
			Assert.AreEqual(-1,vitalsignBefore3.BMIPercentile,"The day before: none.");
			//An adult of 70 with a BMI of 27: overweight by the 2027 range (normal by the 2014 rule from 65).
			Vitalsign vitalsign70=CreateVitalsign(patient,_dateBirth.AddYears(70),height:70,weight:188);
			Assert.AreNotEqual("",vitalsign70.WeightCode);
			Assert.AreEqual(vitalsign18.WeightCode,vitalsign70.WeightCode,"The overweight code.");
			string source=TestPaths.ReadSource(@"OpenPmsDentalBusiness\EForms\EFormVitalSigns.cs");
			StringAssert.Contains(source,"Vitalsigns.GetAgeOnDateTaken(patient.Birthdate,dateTaken)");
			StringAssert.Contains(source,"Vitalsigns.IsBMIPercentileAge(ageOnDateTaken)");
			Assert.IsFalse(source.Contains("ageBeforeJanFirst"));
		}

		///<summary>EHR-7-R (the lead's decision of 2026-11-15): in the vital sign window a patient without a birth date is an adult, as before
		///EHR-7 (the age "before January 1" of a missing birth date was about 2,000 years): no percentile, not the child group, the over- or
		///underweight class of the 2027 range. A known birth date gives the age on the day taken; a day before it stays unclassed. The eForm import
		///keeps its own rule: no percentile and no weight code without a birth date, as before EHR-7.</summary>
		[TestMethod]
		public void VitalSignWindow_APatientWithoutABirthDate_IsAnAdult() {
			DateTime dateTaken=new DateTime(2027,3,1);
			foreach(DateTime dateNoBirth in new[] { DateTime.MinValue,new DateTime(1800,1,1),new DateTime(1879,12,31) }) {
				int age=Vitalsigns.GetAgeForVitalSignWindow(dateNoBirth,dateTaken);
				Assert.AreEqual(Vitalsigns.AGE_ADULT,age,dateNoBirth.ToString("yyyy-MM-dd"));
				Assert.IsFalse(Vitalsigns.IsBMIPercentileAge(age),"No percentile.");
				Assert.IsFalse(Vitalsigns.IsChildCounselingAge(age),"The adult interventions, not the child group.");
				Assert.IsTrue(Vitalsigns.IsWeightClassAge(age),"Classed over- or underweight.");
				Assert.AreEqual(InterventionCodeSet.AboveNormalWeight,Vitalsigns.GetBMIInterventionCode(27f,age));
				Assert.AreEqual(InterventionCodeSet.BelowNormalWeight,Vitalsigns.GetBMIInterventionCode(18f,age));
				Assert.AreEqual(InterventionCodeSet.None,Vitalsigns.GetBMIInterventionCode(22f,age));
			}
			//The business rule itself still calls the age unknown; only the window classes such a patient as an adult.
			Assert.AreEqual(-1,Vitalsigns.GetAgeOnDateTaken(DateTime.MinValue,dateTaken));
			//A known birth date: the age on the day taken, unchanged (E-35), and a day before the birth unclassed.
			foreach(DateTime date in new[] { _dateBirth,_dateBirth.AddYears(3).AddDays(-1),_dateBirth.AddYears(17),_dateBirth.AddYears(18).AddDays(-1),
				_dateBirth.AddYears(18),_dateBirth.AddYears(65) })
			{
				Assert.AreEqual(Vitalsigns.GetAgeOnDateTaken(_dateBirth,date),Vitalsigns.GetAgeForVitalSignWindow(_dateBirth,date),date.ToString("yyyy-MM-dd"));
			}
			Assert.AreEqual(17,Vitalsigns.GetAgeForVitalSignWindow(_dateBirth,_dateBirth.AddYears(18).AddDays(-1)));
			Assert.AreEqual(-1,Vitalsigns.GetAgeForVitalSignWindow(_dateBirth,_dateBirth.AddDays(-1)));
			//The eForm import without a birth date: no percentile and no weight code (BMI about 34), as before EHR-7.
			Vitalsign vitalsign=CreateVitalsign(new Patient { PatNum=7,Birthdate=DateTime.MinValue,Gender=PatientGender.Male },dateTaken);
			Assert.AreEqual(-1,vitalsign.BMIPercentile);
			Assert.AreEqual("",vitalsign.WeightCode);
		}

		///<summary>A vital sign of height and weight (BMI about 34 by default) through the eForm import, every code table present.</summary>
		private static Vitalsign CreateVitalsign(Patient patient,DateTime dateTaken,double height=64,double weight=200) {
			List<EFormVitalImport> listImports=new List<EFormVitalImport> {
				new EFormVitalImport { DbLink=EFormVitalSigns.DBLINK_HEIGHT,FieldDisplay=EFormVitalSigns.GetFieldDisplay(EFormVitalSigns.DBLINK_HEIGHT),Value=height },
				new EFormVitalImport { DbLink=EFormVitalSigns.DBLINK_WEIGHT,FieldDisplay=EFormVitalSigns.GetFieldDisplay(EFormVitalSigns.DBLINK_WEIGHT),Value=weight },
			};
			return EFormVitalSigns.CreateVitalsign(patient,dateTaken,listImports,x => true,x => true);
		}

		///<summary>The vital sign window takes the age from the one helper, and its three groups from the helper's rules: no "before January 1".</summary>
		[TestMethod]
		public void VitalSignWindow_TakesTheAgeFromTheBusinessLayersHelper() {
			string source=TestPaths.ReadSource(@"OpenPmsDental\Forms\FormVitalsignEdit2014.cs");
			Assert.IsFalse(source.Contains("AgeBeforeJanFirst"),"The age before January 1 is gone.");
			Assert.AreEqual(2,source.Split(new[] { "AgeOnDateTaken=Vitalsigns.GetAgeForVitalSignWindow(_patient.Birthdate," },StringSplitOptions.None).Length-1,
				"On load and when the date changes, with the window's rule for a missing birth date (EHR-7-R).");
			Assert.IsFalse(source.Contains("Vitalsigns.GetAgeOnDateTaken("),"The window takes no age that leaves a patient without a birth date unclassed.");
			StringAssert.Contains(source,"if(Vitalsigns.IsBMIPercentileAge(AgeOnDateTaken)) {");
			StringAssert.Contains(source,"if(Vitalsigns.IsChildCounselingAge(AgeOnDateTaken)) {");
			StringAssert.Contains(source,"else if(Vitalsigns.IsWeightClassAge(AgeOnDateTaken) && labelWeightCode.Text!=\"\") {");
			StringAssert.Contains(source,"Vitalsigns.GetBMIInterventionCode(bmi,AgeOnDateTaken)");
			StringAssert.Contains(source,"FillGridInterventions(Vitalsigns.IsChildCounselingAge(AgeOnDateTaken));");
			StringAssert.Contains(source,"bool isChild=Vitalsigns.IsChildCounselingAge(AgeOnDateTaken);");
			Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(source,@"\.Year-_patient\.Birthdate\.Year"),"No age from the years alone.");
		}
	}
}
