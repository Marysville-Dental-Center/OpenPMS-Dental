// The Measure Calc window of package EHR-4a (docs/specs/ehr-rebuild-plan.md, sections 7, 9.1 and 9.2): the engine's per-measure method gives
// what SelectAll, SelectAllMu2 and SelectAllMu2Mod give; a provider with no ehrprovkey row gives no SQL error and a denominator of 0; the
// window's rules for each stage, measure and year (the 2015-2017 rule for Modified Stage 2, the portal rows, the green rows); Cancel between two
// measures leaves no work table; the work-table helper drops only old work tables of its own database, also through the Middle Tier mock; the
// patient picked in the detail goes up to the EHR window as PatientSelect; and source checks (no WinForms menu, no Process.Start, no
// ProgressWin, no Join). On the ehr4a_ scratch database of Support/MeasureCalcFixture.cs with synthetic rows; no window is created or shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;

namespace EhrTests {
	[TestClass]
	public class MeasureCalcTests {
		private static readonly EhrMeasureStage[] _arrayStages=new[] { EhrMeasureStage.Stage1,EhrMeasureStage.Stage2,EhrMeasureStage.ModifiedStage2 };
		private CultureInfo _cultureBefore;

		[TestInitialize]
		public void Initialize() {
			//The rule tests format numbers; the database tests set en-US themselves (MeasureCalcFixture.Begin).
			_cultureBefore=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
		}

		[TestCleanup]
		public void Cleanup() {
			MeasureCalcFixture.End();
			Thread.CurrentThread.CurrentCulture=_cultureBefore;
		}

		[ClassCleanup]
		public static void ClassCleanup() {
			MeasureCalcFixture.DropAll();
		}

		private static string ReadSource(string repoPath) {
			return File.ReadAllText(TestPaths.Combine(repoPath));
		}

		#region The window's rules (section 9.2), no database
		[TestMethod]
		public void DefaultStage_FollowsTheEhrWindow_ProviderStageFirst_ThenTheGlobalSetting() {
			for(int pref=0;pref<=3;pref++) {
				Assert.AreEqual(EhrMeasureStage.Stage1,EhrMeasureCalcRules.GetDefaultStage(1,pref));
				Assert.AreEqual(EhrMeasureStage.Stage2,EhrMeasureCalcRules.GetDefaultStage(2,pref));
				Assert.AreEqual(EhrMeasureStage.ModifiedStage2,EhrMeasureCalcRules.GetDefaultStage(3,pref));
			}
			Assert.AreEqual(EhrMeasureStage.Stage1,EhrMeasureCalcRules.GetDefaultStage(0,0));
			Assert.AreEqual(EhrMeasureStage.Stage2,EhrMeasureCalcRules.GetDefaultStage(0,1));
			Assert.AreEqual(EhrMeasureStage.ModifiedStage2,EhrMeasureCalcRules.GetDefaultStage(0,2));
			Assert.AreEqual(EhrMeasureStage.Stage1,EhrMeasureCalcRules.GetDefaultStage(0,7),"An unknown setting is Stage 1, as in the EHR window.");
		}

		[TestMethod]
		public void Requirement_ForEachStageMeasureAndYear_FollowsThe2015To2017Rule() {
			//Modified Stage 2 (2015-2017 rule): patient electronic access measure 2 (ElectronicCopy) at least 1 patient in 2015 and 2016; secure
			//messaging a yes/no attestation in 2015 and at least 1 patient in 2016; the engine's "more than" threshold otherwise. The years come from
			//the public final rule "Medicare and Medicaid Programs; Electronic Health Record Incentive Program-Stage 3 and Modifications to Meaningful
			//Use in 2015 Through 2017" (80 FR 62762, 16 October 2015), the eligible professional objectives of 42 CFR 495.22(e) (patient electronic
			//access, secure messaging), and the CMS EP specification sheets for Modified Stage 2 of 2015-2017 that restate them.
			int countChecked=0;
			foreach(EhrMeasureStage stage in _arrayStages) {
				foreach(EhrMeasureType type in EhrMeasures.GetStageMeasureTypes(stage)) {
					for(int year=2013;year<=2019;year++) {
						EhrMeasureRequirement requirement=EhrMeasureCalcRules.GetRequirement(stage,type,37,year);
						EhrMeasureRequirementKind kindExpected=EhrMeasureRequirementKind.MoreThanPercent;
						if(stage==EhrMeasureStage.ModifiedStage2 && type==EhrMeasureType.ElectronicCopy && (year==2015 || year==2016)) {
							kindExpected=EhrMeasureRequirementKind.AtLeastOnePatient;
						}
						if(stage==EhrMeasureStage.ModifiedStage2 && type==EhrMeasureType.SecureMessaging && year==2015) {
							kindExpected=EhrMeasureRequirementKind.Attestation;
						}
						if(stage==EhrMeasureStage.ModifiedStage2 && type==EhrMeasureType.SecureMessaging && year==2016) {
							kindExpected=EhrMeasureRequirementKind.AtLeastOnePatient;
						}
						Assert.AreEqual(kindExpected,requirement.Kind,stage+" "+type+" "+year);
						Assert.AreEqual(37,requirement.PercentThreshold,"The engine's threshold is kept.");
						countChecked++;
					}
				}
			}
			Assert.AreEqual((20+22+14)*7,countChecked);
			Assert.AreEqual("More than 5%",EhrMeasureCalcRules.GetRequirement(EhrMeasureStage.ModifiedStage2,EhrMeasureType.ElectronicCopy,5,2017).GetText());
			Assert.AreEqual("At least 1 patient",
				EhrMeasureCalcRules.GetRequirement(EhrMeasureStage.ModifiedStage2,EhrMeasureType.ElectronicCopy,5,2016).GetText());
			Assert.AreEqual("At least 1 patient",
				EhrMeasureCalcRules.GetRequirement(EhrMeasureStage.ModifiedStage2,EhrMeasureType.SecureMessaging,5,2016).GetText());
			StringAssert.StartsWith(EhrMeasureCalcRules.GetRequirement(EhrMeasureStage.ModifiedStage2,EhrMeasureType.SecureMessaging,5,2015).GetText(),
				"Yes/no");
			//Stage 2 keeps the engine's thresholds in every year: the rule is Modified Stage 2's.
			Assert.AreEqual(EhrMeasureRequirementKind.MoreThanPercent,
				EhrMeasureCalcRules.GetRequirement(EhrMeasureStage.Stage2,EhrMeasureType.SecureMessaging,5,2015).Kind);
			Assert.AreEqual(2016,EhrMeasureCalcRules.GetReportingYear(new DateTime(2016,7,1)),"The year of the start date.");
		}

		[TestMethod]
		public void IsMet_MoreThanTheThreshold_AtLeastOnePatient_NeverAnAttestation() {
			EhrMeasureRequirement moreThan50=new EhrMeasureRequirement(EhrMeasureRequirementKind.MoreThanPercent,50);
			Assert.IsTrue(EhrMeasureCalcRules.IsMet(moreThan50,3,4),"75% is more than 50%.");
			Assert.IsFalse(EhrMeasureCalcRules.IsMet(moreThan50,1,2),"50% is not more than 50%.");
			Assert.IsFalse(EhrMeasureCalcRules.IsMet(moreThan50,0,0),"A denominator of 0 is not met.");
			Assert.IsFalse(EhrMeasureCalcRules.IsMet(moreThan50,-1,-1),"Not calculated is not met.");
			Assert.IsTrue(EhrMeasureCalcRules.IsMet(moreThan50,50001,100000),"Whole numbers, no rounding.");
			Assert.IsFalse(EhrMeasureCalcRules.IsMet(moreThan50,50000,100000));
			EhrMeasureRequirement moreThan5=new EhrMeasureRequirement(EhrMeasureRequirementKind.MoreThanPercent,5);
			Assert.IsFalse(EhrMeasureCalcRules.IsMet(moreThan5,1,20),"5% is not more than 5%.");
			Assert.IsTrue(EhrMeasureCalcRules.IsMet(moreThan5,2,20));
			EhrMeasureRequirement atLeastOne=new EhrMeasureRequirement(EhrMeasureRequirementKind.AtLeastOnePatient,5);
			Assert.IsTrue(EhrMeasureCalcRules.IsMet(atLeastOne,1,1000),"0.1% meets at least 1 patient.");
			Assert.IsFalse(EhrMeasureCalcRules.IsMet(atLeastOne,0,1000));
			Assert.IsFalse(EhrMeasureCalcRules.IsMet(atLeastOne,-1,-1));
			EhrMeasureRequirement attestation=new EhrMeasureRequirement(EhrMeasureRequirementKind.Attestation,5);
			Assert.IsFalse(EhrMeasureCalcRules.IsMet(attestation,10,10),"An attestation is never shown as met.");
			Assert.AreEqual(Color(178,255,178),EhrMeasureCalcRules.ColorMet,"The colour of the EHR window's grid.");
		}

		private static System.Drawing.Color Color(int r,int g,int b) {
			return System.Drawing.Color.FromArgb(r,g,b);
		}

		[TestMethod]
		public void ResultText_NotCalculated_Attestation_Portal_NoPatients_Percent() {
			EhrMeasureRequirement moreThan50=new EhrMeasureRequirement(EhrMeasureRequirementKind.MoreThanPercent,50);
			EhrMeasureRequirement moreThan5=new EhrMeasureRequirement(EhrMeasureRequirementKind.MoreThanPercent,5);
			EhrMeasureRequirement attestation=new EhrMeasureRequirement(EhrMeasureRequirementKind.Attestation,5);
			Assert.AreEqual("not calculated",EhrMeasureCalcRules.GetResultText(EhrMeasureStage.Stage1,EhrMeasureType.ProblemList,moreThan50,-1,-1));
			Assert.AreEqual("attestation, not calculated",
				EhrMeasureCalcRules.GetResultText(EhrMeasureStage.ModifiedStage2,EhrMeasureType.SecureMessaging,attestation,0,12));
			Assert.AreEqual("no patients in the denominator",
				EhrMeasureCalcRules.GetResultText(EhrMeasureStage.Stage1,EhrMeasureType.ProblemList,moreThan50,0,0));
			Assert.AreEqual("75%",EhrMeasureCalcRules.GetResultText(EhrMeasureStage.Stage2,EhrMeasureType.Rx,moreThan50,3,4),
				"Stage 2 e-prescribing: 4 prescriptions, 3 electronic.");
			Assert.AreEqual("33.3%",EhrMeasureCalcRules.GetPercentText(1,3));
			Assert.AreEqual("66.6%",EhrMeasureCalcRules.GetPercentText(2,3),"Rounded down, never shown above what it is.");
			Assert.AreEqual("5%",EhrMeasureCalcRules.GetPercentText(1,20));
			Assert.AreEqual("100%",EhrMeasureCalcRules.GetPercentText(7,7));
			//The two portal measures: marked instead of a 0%, in Stage 2 and Modified Stage 2; a met one shows its percent.
			foreach(EhrMeasureStage stage in new[] { EhrMeasureStage.Stage2,EhrMeasureStage.ModifiedStage2 }) {
				foreach(EhrMeasureType type in new[] { EhrMeasureType.ElectronicCopy,EhrMeasureType.SecureMessaging }) {
					Assert.AreEqual("depends on the removed patient portal",EhrMeasureCalcRules.GetResultText(stage,type,moreThan5,0,40),stage+" "+type);
					Assert.AreEqual("depends on the removed patient portal",EhrMeasureCalcRules.GetResultText(stage,type,moreThan5,0,0));
					Assert.AreEqual("10%",EhrMeasureCalcRules.GetResultText(stage,type,moreThan5,4,40),"Met: the percent.");
					Assert.IsTrue(EhrMeasureCalcRules.IsPortalMeasure(stage,type));
				}
			}
			int countPortal=0;
			foreach(EhrMeasureStage stage in _arrayStages) {
				countPortal+=EhrMeasures.GetStageMeasureTypes(stage).Count(x => EhrMeasureCalcRules.IsPortalMeasure(stage,x));
			}
			Assert.AreEqual(4,countPortal,"Two measures in Stage 2 and two in Modified Stage 2; none in Stage 1.");
			Assert.AreEqual("not calculated",EhrMeasureCalcRules.GetCountText(-1));
			Assert.AreEqual("12",EhrMeasureCalcRules.GetCountText(12));
			Assert.AreEqual("",EhrMeasureCalcRules.GetExclusionCountText(-1),"-1: no count applies to the measure.");
			Assert.AreEqual("0",EhrMeasureCalcRules.GetExclusionCountText(0));
		}

		[TestMethod]
		public void PeriodNotes_TwoCalendarYears_AlternateExclusions_Portal() {
			List<string> listOneYear=EhrMeasureCalcRules.GetPeriodNotes(EhrMeasureStage.ModifiedStage2,new DateTime(2016,1,1),new DateTime(2016,12,31));
			Assert.IsFalse(listOneYear.Any(x => x.StartsWith("Warning")),"One calendar year: no warning.");
			Assert.IsTrue(listOneYear.Any(x => x.Contains("alternate exclusions") && x.Contains("not modelled")),"E-16.");
			Assert.IsTrue(listOneYear.Any(x => x.Contains("patient portal")));
			List<string> listTwoYears=EhrMeasureCalcRules.GetPeriodNotes(EhrMeasureStage.ModifiedStage2,new DateTime(2015,10,1),new DateTime(2016,3,31));
			string warning=listTwoYears.Single(x => x.StartsWith("Warning"));
			StringAssert.Contains(warning,"two calendar years");
			StringAssert.Contains(warning,"2015");
			Assert.IsTrue(EhrMeasureCalcRules.GetPeriodNotes(EhrMeasureStage.ModifiedStage2,new DateTime(2014,1,1),new DateTime(2014,12,31))
				.Any(x => x.Contains("began in 2015")));
			Assert.AreEqual(0,EhrMeasureCalcRules.GetPeriodNotes(EhrMeasureStage.Stage1,new DateTime(2015,10,1),new DateTime(2016,3,31)).Count,
				"Stage 1 has no portal measure and no year rule.");
			List<string> listStage2=EhrMeasureCalcRules.GetPeriodNotes(EhrMeasureStage.Stage2,new DateTime(2015,10,1),new DateTime(2016,3,31));
			Assert.AreEqual(1,listStage2.Count,"Stage 2: only the portal note.");
			Assert.AreEqual("",EhrMeasureCalcRules.GetDateError(new DateTime(2016,1,1),new DateTime(2016,1,1)));
			Assert.AreNotEqual("",EhrMeasureCalcRules.GetDateError(new DateTime(2016,2,1),new DateTime(2016,1,1)));
			Assert.AreNotEqual("",EhrMeasureCalcRules.GetDateError(DateTime.MinValue,new DateTime(2016,1,1)));
		}

		[TestMethod]
		public void ProviderChoices_OneEntryPerRowName_TheUsersProviderFirst() {
			List<Provider> listProviders=new List<Provider>() {
				new Provider() { ProvNum=1,LName="Alast",FName="Afirst",Abbr="AA" },
				new Provider() { ProvNum=2,LName="Blast",FName="Bfirst",Abbr="BB",IsHidden=true },
				new Provider() { ProvNum=3,LName="BLAST",FName="Bfirst",Abbr="BB2" },
				new Provider() { ProvNum=4,LName="Clast",FName="Cfirst",Abbr="CC" },
			};
			//GetReportingProviders' shape: ProvNum and KeyGroup (the smallest row the provider's names match), ordered by ProvNum. 2 and 3 share a
			//group (the database's collation ignores case); 9 is not in the provider list.
			DataTable table=new DataTable();
			table.Columns.Add("ProvNum");
			table.Columns.Add("KeyGroup");
			table.Rows.Add("1","10");
			table.Rows.Add("2","11");
			table.Rows.Add("3","11");
			table.Rows.Add("9","12");
			List<EhrMeasureCalcProvider> listChoices=EhrMeasureCalcRules.GetProviderChoices(table,listProviders,0);
			Assert.AreEqual(2,listChoices.Count);
			Assert.AreEqual("Alast, Afirst (AA)",listChoices[0].Text);
			Assert.AreEqual(1,listChoices[0].ProvNum);
			Assert.AreEqual("Blast, Bfirst (BB, BB2)",listChoices[1].Text);
			CollectionAssert.AreEqual(new List<long> { 2,3 },listChoices[1].ListProvNums);
			Assert.AreEqual(3,listChoices[1].ProvNum,"The first provider that is not hidden.");
			Assert.AreEqual(0,EhrMeasureCalcRules.GetDefaultProviderIndex(listChoices,0));
			Assert.AreEqual(0,EhrMeasureCalcRules.GetDefaultProviderIndex(listChoices,4),"A user whose provider has no row: the first entry.");
			List<EhrMeasureCalcProvider> listForUser2=EhrMeasureCalcRules.GetProviderChoices(table,listProviders,2);
			Assert.AreEqual(2,listForUser2[1].ProvNum,"The logged-on user's own provider, hidden or not.");
			Assert.AreEqual(1,EhrMeasureCalcRules.GetDefaultProviderIndex(listForUser2,2));
			Assert.AreEqual(-1,EhrMeasureCalcRules.GetDefaultProviderIndex(new List<EhrMeasureCalcProvider>(),2));
		}

		[TestMethod]
		public void PatientPick_OkWithAPatientOnly() {
			Assert.IsTrue(EhrMeasureCalcRules.IsPatientPicked(DialogResult.OK,42));
			Assert.IsFalse(EhrMeasureCalcRules.IsPatientPicked(DialogResult.OK,0));
			Assert.IsFalse(EhrMeasureCalcRules.IsPatientPicked(DialogResult.Cancel,42));
			Assert.IsFalse(EhrMeasureCalcRules.IsPatientPicked(DialogResult.None,42));
			DataTable table=new DataTable("audit");
			table.Columns.Add("PatNum");
			table.Columns.Add("met");
			table.Rows.Add("57","X");
			table.Rows.Add("","");
			Assert.AreEqual(57,EhrMeasureCalcRules.GetAuditPatNum(table.Rows[0]));
			Assert.AreEqual(0,EhrMeasureCalcRules.GetAuditPatNum(table.Rows[1]));
			Assert.AreEqual(0,EhrMeasureCalcRules.GetAuditPatNum(null));
		}

		[TestMethod]
		public void IsOldWorkTable_OnlyTheEnginesTables_OnlyWhenOldEnough() {
			int day=EhrMeasures.WORK_TABLE_MAX_AGE_SECONDS;
			Assert.AreEqual(86400,day,"One day.");
			Assert.IsTrue(EhrMeasures.IsOldWorkTable("tempehrmeasure123456",day,day));
			Assert.IsTrue(EhrMeasures.IsOldWorkTable("tempehrquality7",day+1,day));
			Assert.IsTrue(EhrMeasures.IsOldWorkTable("TempEhrMeasure7",day+1,day),"The server's LIKE ignores case, and so does the rule.");
			Assert.IsFalse(EhrMeasures.IsOldWorkTable("tempehrmeasure123456",day-1,day),"Another workstation may still be using it.");
			Assert.IsFalse(EhrMeasures.IsOldWorkTable("tempehrmeasure123456",-1,day),"No creation time known.");
			Assert.IsFalse(EhrMeasures.IsOldWorkTable("tempehrmeasure1`; DROP TABLE patient; --",day*10,day),"Not a safe name.");
			Assert.IsFalse(EhrMeasures.IsOldWorkTable("tempehr",day*10,day));
			Assert.IsFalse(EhrMeasures.IsOldWorkTable("patient",day*10,day));
			Assert.IsFalse(EhrMeasures.IsOldWorkTable("xtempehrmeasure1",day*10,day));
			Assert.IsFalse(EhrMeasures.IsOldWorkTable(null,day*10,day));
			Assert.IsTrue(EhrMeasures.IsOldWorkTable("tempehrmeasure1",0,0));
		}
		#endregion

		#region The engine (database)
		[TestMethod]
		public void StageLists_AreTheRowsEachSelectAllCalculates() {
			MeasureCalcFixture.Begin();
			foreach(EhrMeasureStage stage in _arrayStages) {
				List<EhrMeasure> listRows=EhrMeasures.GetMeasuresToCalc(stage);
				CollectionAssert.AreEqual(EhrMeasures.GetStageMeasureTypes(stage),listRows.Select(x => x.MeasureType).ToList(),stage.ToString());
				Assert.IsTrue(listRows.All(x => x.Numerator==-1 && x.Denominator==-1 && x.Objective==null),"Not calculated yet.");
			}
			Assert.AreNotSame(EhrMeasures.GetStageMeasureTypes(EhrMeasureStage.Stage1),EhrMeasures.GetStageMeasureTypes(EhrMeasureStage.Stage1),
				"A new list on each call.");
		}

		[TestMethod]
		public void PerMeasureMethod_GivesWhatEachSelectAllGives() {
			MeasureCalcFixture.Begin();
			DateTime dateStart=MeasureCalcFixture.DateStart;
			DateTime dateEnd=MeasureCalcFixture.DateEnd;
			long provNum=MeasureCalcFixture.ProviderKeyed.ProvNum;
			Dictionary<EhrMeasureStage,List<EhrMeasure>> dictSelectAll=new Dictionary<EhrMeasureStage,List<EhrMeasure>>() {
				{ EhrMeasureStage.Stage1,EhrMeasures.SelectAll(dateStart,dateEnd,provNum) },
				{ EhrMeasureStage.Stage2,EhrMeasures.SelectAllMu2(dateStart,dateEnd,provNum) },
				{ EhrMeasureStage.ModifiedStage2,EhrMeasures.SelectAllMu2Mod(dateStart,dateEnd,provNum) },
			};
			foreach(EhrMeasureStage stage in _arrayStages) {
				List<EhrMeasure> listWindow=FormEhrMeasureCalc.RunCalculation(stage,dateStart,dateEnd,provNum,null,null,null);
				AssertSameMeasures(dictSelectAll[stage],listWindow,stage.ToString());
				//The detail's audit table is the one the numbers came from.
				foreach(EhrMeasure ehrMeasure in listWindow) {
					DataTable table=EhrMeasures.GetAuditTable(stage,ehrMeasure.MeasureType,dateStart,dateEnd,provNum);
					Assert.IsNotNull(table,stage+" "+ehrMeasure.MeasureType);
					Assert.AreEqual(ehrMeasure.Denominator,table.Rows.Count,stage+" "+ehrMeasure.MeasureType);
					Assert.AreEqual(ehrMeasure.Numerator,EhrMeasures.CalcNumerator(table),stage+" "+ehrMeasure.MeasureType);
				}
			}
			//The fixture is not empty: the four patients seen by the two provider records with the row's names, two with a problem.
			EhrMeasure problemList=dictSelectAll[EhrMeasureStage.Stage1].Single(x => x.MeasureType==EhrMeasureType.ProblemList);
			Assert.AreEqual(4,problemList.Denominator);
			Assert.AreEqual(2,problemList.Numerator);
			Assert.AreEqual(80,problemList.PercentThreshold);
			//The engine counts both records with the names together, whichever one is passed.
			AssertSameMeasures(dictSelectAll[EhrMeasureStage.Stage1],
				EhrMeasures.SelectAll(dateStart,dateEnd,MeasureCalcFixture.ProviderKeyedSameNames.ProvNum),"the other record",isTimeIgnored:true);
			//The engine's own Modified Stage 2 thresholds of the two measures the window's year rule changes.
			List<EhrMeasure> listMod=dictSelectAll[EhrMeasureStage.ModifiedStage2];
			Assert.AreEqual(5,listMod.Single(x => x.MeasureType==EhrMeasureType.ElectronicCopy).PercentThreshold);
			Assert.AreEqual(5,listMod.Single(x => x.MeasureType==EhrMeasureType.SecureMessaging).PercentThreshold);
			//E-19: the ehrmeasure table's Numerator and Denominator stay unused; no calculation writes them.
			Assert.IsTrue(MeasureCalcFixture.GetStoredNumbers().All(x => x=="-1/-1"),string.Join(" ",MeasureCalcFixture.GetStoredNumbers()));
			Assert.IsNotNull(typeof(EhrMeasures).GetMethod("Update"),"EhrMeasures.Update stays (E-19).");
		}

		private static void AssertSameMeasures(List<EhrMeasure> listExpected,List<EhrMeasure> listActual,string message,bool isTimeIgnored=true) {
			Assert.AreEqual(listExpected.Count,listActual.Count,message);
			for(int i=0;i<listExpected.Count;i++) {
				EhrMeasure expected=listExpected[i];
				EhrMeasure actual=listActual[i];
				string where=message+" "+expected.MeasureType;
				Assert.AreEqual(expected.EhrMeasureNum,actual.EhrMeasureNum,where);
				Assert.AreEqual(expected.MeasureType,actual.MeasureType,where);
				Assert.AreEqual(expected.Numerator,actual.Numerator,where);
				Assert.AreEqual(expected.Denominator,actual.Denominator,where);
				Assert.AreEqual(expected.Objective,actual.Objective,where);
				Assert.AreEqual(expected.Measure,actual.Measure,where);
				Assert.AreEqual(expected.PercentThreshold,actual.PercentThreshold,where);
				Assert.AreEqual(expected.NumeratorExplain,actual.NumeratorExplain,where);
				Assert.AreEqual(expected.DenominatorExplain,actual.DenominatorExplain,where);
				Assert.AreEqual(expected.ExclusionExplain,actual.ExclusionExplain,where);
				Assert.AreEqual(expected.ExclusionCount,actual.ExclusionCount,where);
				Assert.AreEqual(expected.ExclusionCountDescript,actual.ExclusionCountDescript,where);
				Assert.IsTrue(isTimeIgnored || expected.ElapsedTime==actual.ElapsedTime);
			}
		}

		[TestMethod]
		public void ProviderWithNoKeyRow_NoSqlError_DenominatorZero() {
			MeasureCalcFixture.Begin();
			long provNum=MeasureCalcFixture.ProviderNoRow.ProvNum;
			Assert.IsFalse(EhrProvKeys.HasProvHadKey(MeasureCalcFixture.ProviderNoRow.LName,MeasureCalcFixture.ProviderNoRow.FName));
			List<EhrMeasure> listAll=new List<EhrMeasure>();
			listAll.AddRange(EhrMeasures.SelectAll(MeasureCalcFixture.DateStart,MeasureCalcFixture.DateEnd,provNum));
			listAll.AddRange(EhrMeasures.SelectAllMu2(MeasureCalcFixture.DateStart,MeasureCalcFixture.DateEnd,provNum));
			listAll.AddRange(EhrMeasures.SelectAllMu2Mod(MeasureCalcFixture.DateStart,MeasureCalcFixture.DateEnd,provNum));
			Assert.AreEqual(20+22+14,listAll.Count,"Every measure of every stage was calculated, none threw.");
			foreach(EhrMeasure ehrMeasure in listAll) {
				Assert.AreEqual(0,ehrMeasure.Denominator,ehrMeasure.MeasureType+": the provider's own patient and the order with no provider are not "
					+"counted.");
				Assert.AreEqual(0,ehrMeasure.Numerator,ehrMeasure.MeasureType.ToString());
			}
			//The engine guard: the five provider lists go through it, and it gives -1, never 0 (a row with no provider holds 0).
			string source=ReadSource(@"OpenPmsDentalBusiness\Data Interface\EhrMeasures.cs");
			Assert.AreEqual(5,Regex.Matches(source,@"string provs=GetProvsOrNone\(Db\.GetScalar\(command\)\);").Count);
			Assert.AreEqual(0,Regex.Matches(source,@"string provs=Db\.GetScalar").Count);
			System.Reflection.MethodInfo methodInfo=typeof(EhrMeasures).GetMethod("GetProvsOrNone",
				System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
			Assert.AreEqual("-1",methodInfo.Invoke(null,new object[] { "" }));
			Assert.AreEqual("-1",methodInfo.Invoke(null,new object[] { null }));
			Assert.AreEqual("3,7",methodInfo.Invoke(null,new object[] { "3,7" }));
		}

		[TestMethod]
		public void Cancel_BetweenMeasures_StopsThere_AndLeavesNoWorkTable() {
			MeasureCalcFixture.Begin();
			foreach(EhrMeasureStage stage in _arrayStages) {
				int countStarted=0;
				int countDone=0;
				List<EhrMeasure> listDone=FormEhrMeasureCalc.RunCalculation(stage,MeasureCalcFixture.DateStart,MeasureCalcFixture.DateEnd,
					MeasureCalcFixture.ProviderKeyed.ProvNum,() => countDone>=2,(index,count,type) => countStarted++,(index,count,ehrMeasure) => countDone++);
				Assert.AreEqual(2,listDone.Count,stage+": cancelled after the second measure.");
				Assert.AreEqual(2,countStarted,"The third measure never started.");
				CollectionAssert.AreEqual(EhrMeasures.GetStageMeasureTypes(stage).Take(2).ToList(),listDone.Select(x => x.MeasureType).ToList());
				CollectionAssert.AreEqual(new List<string>(),MeasureCalcFixture.GetWorkTables(MeasureCalcFixture.DbName),stage+": no work table left.");
				List<EhrMeasure> listNone=FormEhrMeasureCalc.RunCalculation(stage,MeasureCalcFixture.DateStart,MeasureCalcFixture.DateEnd,
					MeasureCalcFixture.ProviderKeyed.ProvNum,() => true,null,null);
				Assert.AreEqual(0,listNone.Count,"Cancelled before the first measure.");
			}
		}

		[TestMethod]
		public void WorkTables_OnlyOldOnes_OfThisDatabase_AreDropped() {
			MeasureCalcFixture.Begin();
			string db=MeasureCalcFixture.DbName;
			string dbOther=MeasureCalcFixture.DbNameOther;
			MeasureCalcFixture.CreateTable(db,"tempehrmeasure900001");
			MeasureCalcFixture.CreateTable(db,"tempehrquality900002");
			MeasureCalcFixture.CreateTable(db,"tempother900003");
			MeasureCalcFixture.CreateTable(dbOther,"tempehrmeasure900004");
			try {
				//Created just now: a refresh (one day) drops neither, because another workstation may be calculating with it.
				FormEhrMeasureCalc.RunCalculation(EhrMeasureStage.Stage1,MeasureCalcFixture.DateStart,MeasureCalcFixture.DateEnd,
					MeasureCalcFixture.ProviderKeyed.ProvNum,() => true,null,null);
				CollectionAssert.AreEqual(new List<string> { "tempehrmeasure900001","tempehrquality900002" },MeasureCalcFixture.GetWorkTables(db));
				CollectionAssert.AreEqual(new List<string>(),EhrMeasures.DropOldWorkTables(EhrMeasures.WORK_TABLE_MAX_AGE_SECONDS));
				//Old enough (an age of 0 seconds stands in for a day): the two work tables of this database go, nothing else.
				List<string> listDropped=EhrMeasures.DropOldWorkTables(0);
				listDropped.Sort();
				CollectionAssert.AreEqual(new List<string> { "tempehrmeasure900001","tempehrquality900002" },listDropped);
				CollectionAssert.AreEqual(new List<string>(),MeasureCalcFixture.GetWorkTables(db));
				CollectionAssert.AreEqual(new List<string> { "tempehrmeasure900004" },MeasureCalcFixture.GetWorkTables(dbOther),"Another database's.");
				Assert.AreEqual("1",DataCore.GetScalar("SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() "
					+"AND TABLE_NAME='tempother900003'"),"Not a work table of the engines.");
			}
			finally {
				MeasureCalcFixture.DropTable(db,"tempehrmeasure900001");
				MeasureCalcFixture.DropTable(db,"tempehrquality900002");
				MeasureCalcFixture.DropTable(db,"tempother900003");
				MeasureCalcFixture.DropTable(dbOther,"tempehrmeasure900004");
			}
		}

		[TestMethod]
		public void MiddleTier_WorkTableHelper_AndOneMeasureAtATime_RunOnTheServer() {
			MeasureCalcFixture.Begin();
			string db=MeasureCalcFixture.DbName;
			DateTime dateStart=MeasureCalcFixture.DateStart;
			DateTime dateEnd=MeasureCalcFixture.DateEnd;
			long provNum=MeasureCalcFixture.ProviderKeyed.ProvNum;
			List<EhrMeasure> listDirect=EhrMeasures.SelectAllMu2Mod(dateStart,dateEnd,provNum);
			DataTable tableDirect=EhrMeasures.GetReportingProviders();
			MeasureCalcFixture.CreateTable(db,"tempehrmeasure900011");
			try {
				//Db refuses SQL on a Middle Tier client, so these answers came back from DtoProcessor on the mock server's thread: arguments and results
				//serialized both ways.
				MeasureCalcFixture.UseMockMiddleTier();
				Assert.AreEqual(MiddleTierRole.ClientMT,RemotingClient.MiddleTierRole);
				CollectionAssert.AreEqual(new List<string>(),EhrMeasures.DropOldWorkTables(EhrMeasures.WORK_TABLE_MAX_AGE_SECONDS),"Too new.");
				CollectionAssert.AreEqual(new List<string> { "tempehrmeasure900011" },EhrMeasures.DropOldWorkTables(0));
				List<EhrMeasure> listMiddleTier=FormEhrMeasureCalc.RunCalculation(EhrMeasureStage.ModifiedStage2,dateStart,dateEnd,provNum,null,null,null);
				AssertSameMeasures(listDirect,listMiddleTier,"through the Middle Tier");
				DataTable tableMiddleTier=EhrMeasures.GetReportingProviders();
				Assert.AreEqual(tableDirect.Rows.Count,tableMiddleTier.Rows.Count);
				for(int i=0;i<tableDirect.Rows.Count;i++) {
					Assert.AreEqual(tableDirect.Rows[i]["ProvNum"].ToString(),tableMiddleTier.Rows[i]["ProvNum"].ToString());
					Assert.AreEqual(tableDirect.Rows[i]["KeyGroup"].ToString(),tableMiddleTier.Rows[i]["KeyGroup"].ToString());
				}
				DataTable tableAudit=EhrMeasures.GetAuditTable(EhrMeasureStage.Stage1,EhrMeasureType.ProblemList,dateStart,dateEnd,provNum);
				Assert.AreEqual(4,tableAudit.Rows.Count);
				Assert.AreEqual(MiddleTierRole.ClientMT,RemotingClient.MiddleTierRole,"The mock puts the client's role back after each call.");
			}
			finally {
				MeasureCalcFixture.UseDirectConnection();
				MeasureCalcFixture.DropTable(db,"tempehrmeasure900011");
			}
			CollectionAssert.AreEqual(new List<string>(),MeasureCalcFixture.GetWorkTables(db));
		}

		[TestMethod]
		public void ReportingProviders_TheRecordsWithTheRowsNames_InOneGroup() {
			MeasureCalcFixture.Begin();
			DataTable table=EhrMeasures.GetReportingProviders();
			List<long> listProvNums=table.Rows.Cast<DataRow>().Select(x => PIn.Long(x["ProvNum"].ToString())).ToList();
			CollectionAssert.Contains(listProvNums,MeasureCalcFixture.ProviderKeyed.ProvNum);
			CollectionAssert.Contains(listProvNums,MeasureCalcFixture.ProviderKeyedSameNames.ProvNum);
			CollectionAssert.DoesNotContain(listProvNums,MeasureCalcFixture.ProviderNoRow.ProvNum);
			List<EhrMeasureCalcProvider> listChoices=EhrMeasureCalcRules.GetProviderChoices(table,Providers.GetDeepCopy(),0);
			EhrMeasureCalcProvider choice=listChoices.Single(x => x.ListProvNums.Contains(MeasureCalcFixture.ProviderKeyed.ProvNum));
			CollectionAssert.AreEqual(new List<long> { MeasureCalcFixture.ProviderKeyed.ProvNum,MeasureCalcFixture.ProviderKeyedSameNames.ProvNum },
				choice.ListProvNums);
			Assert.AreEqual(MeasureCalcFixture.ProviderKeyed.ProvNum,choice.ProvNum);
			StringAssert.StartsWith(choice.Text,MeasureCalcFixture.ProviderKeyed.LName+", "+MeasureCalcFixture.ProviderKeyed.FName+" (");
		}

		[TestMethod]
		public void PatientPick_TheAuditPatient_GoesUpToTheEhrWindowAsPatientSelect() {
			MeasureCalcFixture.Begin();
			DataTable table=EhrMeasures.GetAuditTable(EhrMeasureStage.Stage1,EhrMeasureType.ProblemList,MeasureCalcFixture.DateStart,
				MeasureCalcFixture.DateEnd,MeasureCalcFixture.ProviderKeyed.ProvNum);
			List<long> listPatNums=table.Rows.Cast<DataRow>().Select(x => EhrMeasureCalcRules.GetAuditPatNum(x)).OrderBy(x => x).ToList();
			CollectionAssert.AreEqual(MeasureCalcFixture.ListPatientsKeyed.Select(x => x.PatNum).OrderBy(x => x).ToList(),listPatNums);
			//The chain, in the source: the detail sets SelectedPatNum with OK; Measure Calc passes it up with OK; the EHR window closes with
			//PatientSelect for it; the Chart switches (ControlChart, unchanged).
			string detail=ReadSource(@"OpenPmsDental\Forms\FormEhrMeasureCalcDetail.cs");
			StringAssert.Contains(detail,"row.Tag=EhrMeasureCalcRules.GetAuditPatNum(table.Rows[i]);");
			Assert.IsTrue(Regex.IsMatch(detail,@"SelectedPatNum=patNum;\s*DialogResult=DialogResult\.OK;"),"The detail returns the patient with OK.");
			string calc=ReadSource(@"OpenPmsDental\Forms\FormEhrMeasureCalc.cs");
			StringAssert.Contains(calc,"EhrMeasureCalcRules.IsPatientPicked(formEhrMeasureCalcDetail.DialogResult,formEhrMeasureCalcDetail.SelectedPatNum)");
			StringAssert.Contains(calc,"SelectedPatNum=formEhrMeasureCalcDetail.SelectedPatNum;");
			string ehr=ReadSource(@"OpenPmsDental\Forms\FormEHR.cs");
			string handler=GetMethodBody(ehr,"private void butMeasures_Click(");
			StringAssert.Contains(handler,"new FormEhrMeasureCalc()");
			StringAssert.Contains(handler,"EhrMeasureCalcRules.IsPatientPicked(formEhrMeasureCalc.DialogResult,formEhrMeasureCalc.SelectedPatNum)");
			StringAssert.Contains(handler,"PatNum=formEhrMeasureCalc.SelectedPatNum;");
			StringAssert.Contains(handler,"EhrFormResultOnClose=EhrFormResult.PatientSelect;");
			Assert.IsFalse(handler.Contains("not available in this build"),"The old message is gone.");
			Assert.IsFalse(handler.Contains("Security.IsAuthorized"),"No new gate, as in Open Dental (E-4).");
			string chart=ReadSource(@"OpenPmsDental\Main Modules\ControlChart.cs");
			StringAssert.Contains(chart,"if(formEHR.EhrFormResultOnClose==EhrFormResult.PatientSelect) {");
		}

		private static string GetMethodBody(string source,string signature) {
			int start=source.IndexOf(signature,StringComparison.Ordinal);
			Assert.IsTrue(start>=0,signature);
			int end=source.IndexOf("\n\t\tprivate ",start+signature.Length,StringComparison.Ordinal);
			if(end<0) {
				end=source.Length;
			}
			return source.Substring(start,end-start);
		}
		#endregion

		#region Source checks
		[TestMethod]
		public void Source_TheWindows_NoMenuNoProcessNoProgressWinNoJoin_OneThreadAndAFlag() {
			string[] arrayFiles=new[] {
				@"OpenPmsDental\Forms\FormEhrMeasureCalc.cs",
				@"OpenPmsDental\Forms\FormEhrMeasureCalc.designer.cs",
				@"OpenPmsDental\Forms\FormEhrMeasureCalcDetail.cs",
				@"OpenPmsDental\Forms\FormEhrMeasureCalcDetail.designer.cs",
			};
			foreach(string file in arrayFiles) {
				string source=ReadSource(file);
				//The rules of 3a, kept in advance (section 6): no WinForms ContextMenu, MenuItem or DataGrid; no process.
				foreach(string forbidden in new[] { "ContextMenu","MenuItem","DataGrid","Process.Start","new Process(","new System.Diagnostics.Process(",
					"ProgressWin","Thread.Abort","Encoding.Default","Encoding.GetEncoding",".Join(" })
				{
					Assert.IsFalse(Regex.IsMatch(RemoveStringJoins(source),Regex.Escape(forbidden)),file+" holds "+forbidden);
				}
			}
			string calc=ReadSource(@"OpenPmsDental\Forms\FormEhrMeasureCalc.cs");
			StringAssert.Contains(calc,"new ODThread(");
			StringAssert.Contains(calc,"private volatile bool _isCancelRequested;");
			string run=GetMethodBody(calc,"public static List<EhrMeasure> RunCalculation(");
			//The flag is read before each measure, inside the loop over the measures, and each measure is one call of the engine.
			int indexLoop=run.IndexOf("for(int i=0;i<listEhrMeasuresToCalc.Count;i++) {",StringComparison.Ordinal);
			int indexFlag=run.IndexOf("funcIsCancelRequested()",StringComparison.Ordinal);
			int indexCalc=run.IndexOf("EhrMeasures.CalcMeasure(",StringComparison.Ordinal);
			Assert.IsTrue(indexLoop>=0 && indexLoop<indexFlag && indexFlag<indexCalc,"The flag is read between two measures.");
			Assert.IsTrue(run.IndexOf("EhrMeasures.DropOldWorkTables(EhrMeasures.WORK_TABLE_MAX_AGE_SECONDS);",StringComparison.Ordinal)
				<indexLoop,"Old work tables go before each refresh.");
			//The internal-review line (E-1) is on the window, the detail and the printout.
			StringAssert.Contains(EhrMeasureCalcRules.INTERNAL_REVIEW_TEXT,"internal review only");
			StringAssert.Contains(EhrMeasureCalcRules.INTERNAL_REVIEW_TEXT,"not certified");
			Assert.AreEqual(3,Regex.Matches(calc+ReadSource(@"OpenPmsDental\Forms\FormEhrMeasureCalcDetail.cs"),
				@"EhrMeasureCalcRules\.INTERNAL_REVIEW_TEXT\)").Count);
			StringAssert.Contains(calc,"gridMain.PrintPage(");
		}

		///<summary>string.Join is not a thread's Join.</summary>
		private static string RemoveStringJoins(string source) {
			return source.Replace("string.Join(","stringJoin(");
		}

		[TestMethod]
		public void Source_TheEngine_SelectAllCallsThePerMeasureMethod() {
			string source=ReadSource(@"OpenPmsDentalBusiness\Data Interface\EhrMeasures.cs");
			string[][] arrayCalls=new[] {
				new[] { "public static List<EhrMeasure> SelectAll(","EhrMeasureStage.Stage1" },
				new[] { "public static List<EhrMeasure> SelectAllMu2(","EhrMeasureStage.Stage2" },
				new[] { "public static List<EhrMeasure> SelectAllMu2Mod(","EhrMeasureStage.ModifiedStage2" },
			};
			foreach(string[] call in arrayCalls) {
				int start=source.IndexOf(call[0],StringComparison.Ordinal);
				int end=source.IndexOf("\n\t\t}",start,StringComparison.Ordinal);
				string body=source.Substring(start,end-start);
				StringAssert.Contains(body,"GetMeasuresToCalc("+call[1]+")");
				StringAssert.Contains(body,"CalcMeasure("+call[1]+",retVal[i],dateStart,dateEnd,provNum)");
				StringAssert.Contains(body,"Meth.GetObject<List<EhrMeasure>>(MethodBase.GetCurrentMethod(),dateStart,dateEnd,provNum)");
			}
			StringAssert.Contains(source,"return Meth.GetObject<EhrMeasure>(MethodBase.GetCurrentMethod(),ehrMeasureStage,ehrMeasure,dateStart,dateEnd,provNum);");
			StringAssert.Contains(source,"return Meth.GetObject<List<string>>(MethodBase.GetCurrentMethod(),maxAgeSeconds);");
			StringAssert.Contains(source,"WHERE TABLE_SCHEMA=DATABASE() ");
		}
		#endregion

		#region The lead fix round (EHR-4a, round 2)
		[TestMethod]
		public void Percent_AJustMetRow_ShowsMoreThanItsThreshold_AnUnmetRowNoMore() {
			EhrMeasureRequirement moreThan80=new EhrMeasureRequirement(EhrMeasureRequirementKind.MoreThanPercent,80);
			Assert.IsTrue(EhrMeasureCalcRules.IsMet(moreThan80,4001,5000),"80.02% is more than 80%.");
			Assert.AreEqual("80.02%",EhrMeasureCalcRules.GetResultText(EhrMeasureStage.Stage1,EhrMeasureType.ProblemList,moreThan80,4001,5000),
				"A green row beside 'More than 80%' must not show 80%.");
			Assert.AreEqual("80%",EhrMeasureCalcRules.GetResultText(EhrMeasureStage.Stage1,EhrMeasureType.ProblemList,moreThan80,4000,5000),
				"Exactly 80% is not met.");
			Assert.AreEqual("79.9%",EhrMeasureCalcRules.GetResultText(EhrMeasureStage.Stage1,EhrMeasureType.ProblemList,moreThan80,3999,5000),
				"79.98% is never rounded up to the threshold.");
			EhrMeasureRequirement moreThan50=new EhrMeasureRequirement(EhrMeasureRequirementKind.MoreThanPercent,50);
			Assert.AreEqual("50.00000005%",EhrMeasureCalcRules.GetResultText(EhrMeasureStage.Stage1,EhrMeasureType.ProblemList,moreThan50,
				1000000001,2000000000),"As many decimals as it takes, for the largest denominators too.");
			//Every numerator of every denominator up to 300, against the engine's thresholds: a met row shows more than its threshold, an unmet one
			//no more than it, and the shown value is never more than the real one.
			foreach(int threshold in new[] { 5,10,30,40,50,60,80 }) {
				EhrMeasureRequirement requirement=new EhrMeasureRequirement(EhrMeasureRequirementKind.MoreThanPercent,threshold);
				for(int denominator=1;denominator<=300;denominator++) {
					for(int numerator=0;numerator<=denominator;numerator++) {
						string text=EhrMeasureCalcRules.GetResultText(EhrMeasureStage.Stage1,EhrMeasureType.ProblemList,requirement,numerator,denominator);
						Assert.IsTrue(text.EndsWith("%"),text);
						decimal shown=decimal.Parse(text.TrimEnd('%'),CultureInfo.CurrentCulture);
						string where=numerator+" of "+denominator+" against "+threshold+": "+text;
						Assert.IsTrue(shown*denominator<=numerator*100m,"Never more than it is. "+where);
						if(EhrMeasureCalcRules.IsMet(requirement,numerator,denominator)) {
							Assert.IsTrue(shown>threshold,"Met: more than the threshold. "+where);
						}
						else {
							Assert.IsTrue(shown<=threshold,"Not met: no more than the threshold. "+where);
						}
					}
				}
			}
		}

		[TestMethod]
		public void Detail_TheAuditTable_IsTheOneAlreadyFetched_OrComesOffTheWindowsThread() {
			string detail=ReadSource(@"OpenPmsDental\Forms\FormEhrMeasureCalcDetail.cs");
			//One query, inside an ODThread's worker: never on the window's thread.
			Assert.AreEqual(1,Regex.Matches(detail,@"EhrMeasures\.GetAuditTable\(").Count);
			int indexThread=detail.IndexOf("new ODThread(",StringComparison.Ordinal);
			Assert.IsTrue(indexThread>=0,"The detail starts an ODThread.");
			string worker=detail.Substring(indexThread,detail.IndexOf("});",indexThread,StringComparison.Ordinal)-indexThread);
			StringAssert.Contains(worker,"EhrMeasures.GetAuditTable(","The query is the thread's work.");
			StringAssert.Contains(detail,"this.InvokeIfNotDisposed(","The answer comes back through InvokeIfNotDisposed.");
			StringAssert.Contains(detail,"private void butCancel_Click(");
			StringAssert.Contains(detail,"private volatile bool _isCancelRequested;");
			string designer=ReadSource(@"OpenPmsDental\Forms\FormEhrMeasureCalcDetail.designer.cs");
			StringAssert.Contains(designer,"this.butCancel.Click += new System.EventHandler(this.butCancel_Click);");
			//Meanwhile the patient grid is off; Measure Calc hands the detail a table it already has and keeps the one the detail fetched.
			StringAssert.Contains(detail,"gridAudit.Enabled=!isLoading;");
			string calc=ReadSource(@"OpenPmsDental\Forms\FormEhrMeasureCalc.cs");
			StringAssert.Contains(calc,"formEhrMeasureCalcDetail.TableAudit=");
			StringAssert.Contains(calc,"=formEhrMeasureCalcDetail.TableAudit;");
		}

		[TestMethod]
		public void Refresh_AfterAnError_TheProgressLineSaysWhereItStopped_NotDone() {
			string calc=ReadSource(@"OpenPmsDental\Forms\FormEhrMeasureCalc.cs");
			string refresh=GetMethodBody(calc,"private void butRefresh_Click(");
			StringAssert.Contains(refresh,"EhrMeasureCalcRules.GetProgressEndText(","The end of a refresh is worded by the rule, which knows of the error.");
			Assert.IsFalse(refresh.Contains("\"Done:\""),"No 'Done' regardless of an error.");
		}

		[TestMethod]
		public void PeriodNotes_ModifiedStage2_OneStatementOfTheYearShown_AndTheLabelShowsEveryNote() {
			List<EhrMeasureType> listTypes=EhrMeasures.GetStageMeasureTypes(EhrMeasureStage.ModifiedStage2);
			for(int yearStart=2013;yearStart<=2018;yearStart++) {
				foreach(DateTime dateEnd in new[] { new DateTime(yearStart,12,31),new DateTime(yearStart+1,3,31) }) {
					DateTime dateStart=new DateTime(yearStart,10,1);
					List<string> listNotes=EhrMeasureCalcRules.GetPeriodNotes(EhrMeasureStage.ModifiedStage2,dateStart,dateEnd);
					string where=dateStart.ToShortDateString()+" to "+dateEnd.ToShortDateString()+": "+string.Join(" | ",listNotes);
					List<string> listYearNotes=listNotes.Where(x => Regex.IsMatch(x,@"requirements of \d{4}")).ToList();
					bool isTwoYears=dateEnd.Year!=yearStart;
					if(!isTwoYears && yearStart>=2015) {
						Assert.AreEqual(0,listYearNotes.Count,where);
						continue;
					}
					Assert.AreEqual(1,listYearNotes.Count,"One statement of the year whose requirements are shown. "+where);
					MatchCollection matches=Regex.Matches(listYearNotes[0],@"requirements of (\d{4})");
					Assert.AreEqual(1,matches.Count,"One year named. "+where);
					int yearShown=int.Parse(matches[0].Groups[1].Value,CultureInfo.InvariantCulture);
					Assert.AreEqual(isTwoYears,listYearNotes[0].Contains("two calendar years"),"The two-year warning is in that statement. "+where);
					Assert.AreEqual(yearStart<2015,listYearNotes[0].Contains("began in 2015"),where);
					//The year named is the one whose requirements the grid shows.
					int reportingYear=EhrMeasureCalcRules.GetReportingYear(dateStart);
					foreach(EhrMeasureType type in listTypes) {
						Assert.AreEqual(EhrMeasureCalcRules.GetRequirement(EhrMeasureStage.ModifiedStage2,type,5,yearShown).GetText(),
							EhrMeasureCalcRules.GetRequirement(EhrMeasureStage.ModifiedStage2,type,5,reportingYear).GetText(),type+" "+where);
					}
				}
			}
			//The notes area shows every note of the longest combination, in the window's font, without covering the progress line or the grid.
			string designer=ReadSource(@"OpenPmsDental\Forms\FormEhrMeasureCalc.designer.cs");
			System.Drawing.Rectangle rectangleNotes=GetDesignerBounds(designer,"labelNotes");
			System.Drawing.Rectangle rectangleProgress=GetDesignerBounds(designer,"labelProgress");
			System.Drawing.Rectangle rectangleGrid=GetDesignerBounds(designer,"gridMain");
			Assert.IsTrue(rectangleNotes.Bottom<=rectangleProgress.Top,"The notes do not cover the progress line.");
			Assert.IsTrue(rectangleProgress.Bottom<=rectangleGrid.Top,"The progress line does not cover the grid.");
			int heightMax=0;
			foreach(EhrMeasureStage stage in _arrayStages) {
				for(int yearStart=2013;yearStart<=2018;yearStart++) {
					foreach(DateTime dateEnd in new[] { new DateTime(yearStart,12,31),new DateTime(yearStart+1,3,31) }) {
						string text=string.Join("\r\n",EhrMeasureCalcRules.GetPeriodNotes(stage,new DateTime(yearStart,10,1),dateEnd));
						System.Drawing.Size size=MeasureNotes(text,rectangleNotes.Width);
						heightMax=Math.Max(heightMax,size.Height);
						Assert.IsTrue(size.Height<=rectangleNotes.Height,stage+" "+yearStart+"-"+dateEnd.Year+": "+size.Height+" px of notes in "
							+rectangleNotes.Height+" px.");
					}
				}
			}
			Assert.IsTrue(heightMax>0);
		}

		///<summary>The size of word-wrapped text in the window's font (FormODBase: Microsoft Sans Serif 8.25 at 96 dpi; the layout manager scales
		///the font and the control together).</summary>
		private static System.Drawing.Size MeasureNotes(string text,int width) {
			using(System.Drawing.Font font=new System.Drawing.Font("Microsoft Sans Serif",8.25f)) {
				return TextRenderer.MeasureText(text,font,new System.Drawing.Size(width,int.MaxValue),TextFormatFlags.WordBreak|TextFormatFlags.TextBoxControl);
			}
		}

		///<summary>A control's location and size as the designer file sets them.</summary>
		private static System.Drawing.Rectangle GetDesignerBounds(string designer,string name) {
			Match matchLocation=Regex.Match(designer,@"this\."+name+@"\.Location = new System\.Drawing\.Point\((\d+), (\d+)\);");
			Match matchSize=Regex.Match(designer,@"this\."+name+@"\.Size = new System\.Drawing\.Size\((\d+), (\d+)\);");
			Assert.IsTrue(matchLocation.Success && matchSize.Success,name);
			return new System.Drawing.Rectangle(int.Parse(matchLocation.Groups[1].Value),int.Parse(matchLocation.Groups[2].Value),
				int.Parse(matchSize.Groups[1].Value),int.Parse(matchSize.Groups[2].Value));
		}

		[TestMethod]
		public void WorkTables_TheServerKeepsAFloorOnTheAge_NoClientDropsATableInUse() {
			MeasureCalcFixture.Begin();
			string db=MeasureCalcFixture.DbName;
			MeasureCalcFixture.CreateTable(db,"tempehrmeasure900021");
			try {
				CollectionAssert.AreEqual(new List<string>(),EhrMeasures.DropOldWorkTables(0),"Directly: an age of 0 is raised to the floor.");
				MeasureCalcFixture.UseMockMiddleTier();
				Assert.AreEqual(MiddleTierRole.ClientMT,RemotingClient.MiddleTierRole);
				foreach(int maxAgeSeconds in new[] { 0,-1,1,59,int.MinValue }) {
					CollectionAssert.AreEqual(new List<string>(),EhrMeasures.DropOldWorkTables(maxAgeSeconds),
						"Through the Middle Tier, "+maxAgeSeconds+" s: the server raises it to the floor.");
				}
			}
			finally {
				MeasureCalcFixture.UseDirectConnection();
			}
			try {
				CollectionAssert.AreEqual(new List<string> { "tempehrmeasure900021" },MeasureCalcFixture.GetWorkTables(db),"A table made just now stays.");
				//The floor is applied after the Middle Tier call, in the code the server runs; the window's one day is above it.
				string source=ReadSource(@"OpenPmsDentalBusiness\Data Interface\EhrMeasures.cs");
				int start=source.IndexOf("public static List<string> DropOldWorkTables(int maxAgeSeconds)",StringComparison.Ordinal);
				Assert.IsTrue(start>=0);
				string body=source.Substring(start,source.IndexOf("\n\t\t}",start,StringComparison.Ordinal)-start);
				int indexCall=body.IndexOf("return Meth.GetObject<List<string>>(MethodBase.GetCurrentMethod(),maxAgeSeconds);",StringComparison.Ordinal);
				int indexFloor=body.IndexOf("WORK_TABLE_MIN_AGE_SECONDS",StringComparison.Ordinal);
				Assert.IsTrue(indexCall>=0 && indexCall<indexFloor,"The server applies the floor.");
				System.Reflection.FieldInfo fieldInfo=typeof(EhrMeasures).GetField("WORK_TABLE_MIN_AGE_SECONDS");
				Assert.IsNotNull(fieldInfo);
				int floor=(int)fieldInfo.GetValue(null);
				Assert.IsTrue(floor>=3600,"At least an hour.");
				Assert.IsTrue(floor<=EhrMeasures.WORK_TABLE_MAX_AGE_SECONDS);
			}
			finally {
				MeasureCalcFixture.DropTable(db,"tempehrmeasure900021");
			}
		}

		[TestMethod]
		public void ElectronicCopyAccess_CountsTheRemovedPortalsOnlineAccess_MarkedInEveryStage() {
			EhrMeasureRequirement moreThan50=new EhrMeasureRequirement(EhrMeasureRequirementKind.MoreThanPercent,50);
			foreach(EhrMeasureStage stage in _arrayStages) {
				CollectionAssert.Contains(EhrMeasures.GetStageMeasureTypes(stage),EhrMeasureType.ElectronicCopyAccess,stage.ToString());
				Assert.IsTrue(EhrMeasureCalcRules.IsPortalMeasure(stage,EhrMeasureType.ElectronicCopyAccess),stage.ToString());
				Assert.AreEqual(EhrMeasureCalcRules.DEPENDS_ON_REMOVED_PORTAL,
					EhrMeasureCalcRules.GetResultText(stage,EhrMeasureType.ElectronicCopyAccess,moreThan50,0,40),stage+": not met, marked.");
				Assert.AreEqual(EhrMeasureCalcRules.DEPENDS_ON_REMOVED_PORTAL,
					EhrMeasureCalcRules.GetResultText(stage,EhrMeasureType.ElectronicCopyAccess,moreThan50,20,40),stage+": 50% is not met either.");
				Assert.AreEqual("75%",EhrMeasureCalcRules.GetResultText(stage,EhrMeasureType.ElectronicCopyAccess,moreThan50,30,40),
					stage+": met from old data, its percent.");
				//The notes name the row as the grid does.
				string title=EhrMeasureCalcRules.GetMeasureTitle(stage,EhrMeasureType.ElectronicCopyAccess);
				Assert.IsTrue(EhrMeasureCalcRules.GetPeriodNotes(stage,new DateTime(2016,1,1),new DateTime(2016,12,31))
					.Any(x => x.Contains(title) && x.Contains("patient portal")),stage+": "+title);
			}
			//Nothing in the program or the business library writes the event it counts any more; the engine only reads it.
			Regex regexWrite=new Regex(@"EventType\s*=\s*EhrMeasureEventType\.OnlineAccessProvided");
			foreach(string folder in new[] { "OpenPmsDental","OpenPmsDentalBusiness" }) {
				foreach(string file in Directory.GetFiles(TestPaths.Combine(folder),"*.cs",SearchOption.AllDirectories)) {
					Assert.IsFalse(regexWrite.IsMatch(File.ReadAllText(file)),file);
				}
			}
		}
		#endregion
	}
}
