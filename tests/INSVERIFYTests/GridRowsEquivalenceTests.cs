// The Insurance Verification List's grid rows in linear time (INSV4, docs/port-notes/INSVERIFY.md): InsVerifies.TableToListInsVerifyGridObjects
// (a private method) looks up the grid rows it has built by plan and by patient plan in dictionaries instead of with List.Find. These tests run
// it and the method as it was before (GridRowsBeforeInsv4, a copy kept in this project) on the same tables and require the same grid rows, in
// the same order, with every field of both halves the same: on tables built by hand (two family members on one plan, a plan with both an
// eligibility and a benefits row, Medicaid rows, rows whose plan or patient plan number is 0, duplicate keys, a plan number equal to another
// row's patient plan number), on random tables, and on the rows the list's query returns from the scratch database. Each with both year checks
// off, both on, and each on alone, as the method combines a patient plan's two rows only when both are off or both on. A third part of the class
// VerifyGridKeyTests: the same scratch database and helpers. Synthetic rows only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using UnitTestsCore;

namespace INSVERIFYTests {
	public partial class VerifyGridKeyTests {
		///<summary>The four settings of the two year checks (benefits, eligibility).</summary>
		private static readonly bool[][] _arrayYearChecks={
			new[] { false,false },new[] { true,true },new[] { true,false },new[] { false,true } };

		#region Grid rows in linear time
		[TestMethod]
		public void GridRows_HandBuiltTables_SameAsBefore() {
			List<KeyValuePair<string,DataTable>> listCases=GetHandBuiltCases();
			ForEachYearCheck(label => {
				foreach(KeyValuePair<string,DataTable> pair in listCases) {
					foreach(bool isForMedicaid in new[] { false,true }) {
						AssertSameGridRows(pair.Value,isForMedicaid,label+", "+pair.Key+(isForMedicaid ? ", Medicaid" : ""));
					}
				}
			});
		}

		[TestMethod]
		public void GridRows_RandomTables_SameAsBefore() {
			//Few plans and patient plans, so keys repeat often, in every combination of kind, note and status; plan and patient plan numbers from 0,
			//so the same number is now a plan, now a patient plan. Fixed seeds, so a failure repeats.
			ForEachYearCheck(label => {
				for(int seed=1;seed<=400;seed++) {
					Random random=new Random(seed);
					DataTable table=NewGridTable();
					int countRows=random.Next(0,45);
					for(int i=0;i<countRows;i++) {
						int roll=random.Next(20);
						VerifyTypes verifyType=(roll==0 ? VerifyTypes.None : (roll<10 ? VerifyTypes.InsuranceBenefit : VerifyTypes.PatientEnrollment));
						long patPlanNum=random.Next(0,7);
						long planNum=(random.Next(3)==0 ? random.Next(0,5) : patPlanNum%5);
						string note=(random.Next(4)==0 ? "Synthetic note" : "");
						long defNum=random.Next(3)==0 ? 1 : 0;
						AddGridRow(table,i+1,verifyType,patPlanNum,planNum,note,defNum);
					}
					AssertSameGridRows(table,isForMedicaid:seed%2==0,label+", seed "+seed);
				}
			});
		}

		[TestMethod]
		public void GridRows_TheFirstRowOfAKeyIsKept() {
			//List.Find returned the first grid row of a key; the dictionaries must keep that one, not a later one. With both year checks off.
			Dictionary<PrefName,string> dictPrefsBefore=GetPrefValues(PrefName.InsVerifyFutureDateBenefitYear,PrefName.InsVerifyFutureDatePatEnrollmentYear);
			try {
				SetYearChecks(false,false);
				//Two family members on plan 5: the plan's benefits row goes with the first (patient plan 12, combined with its eligibility row); the
				//second's benefits row is not listed again, its eligibility row stands alone.
				DataTable table=NewGridTable();
				AddGridRow(table,1,VerifyTypes.PatientEnrollment,12,5);
				AddGridRow(table,2,VerifyTypes.InsuranceBenefit,12,5);
				AddGridRow(table,3,VerifyTypes.PatientEnrollment,13,5);
				AddGridRow(table,4,VerifyTypes.InsuranceBenefit,13,5);
				//Two eligibility rows of patient plan 20 (a key the choice of the earliest appointment would not leave twice, here by hand): the first.
				AddGridRow(table,5,VerifyTypes.PatientEnrollment,20,21,"first");
				AddGridRow(table,6,VerifyTypes.PatientEnrollment,20,21,"second");
				//A benefits row of plan 21 with the second row's note: it does not join the first eligibility row (another note), and stays alone.
				AddGridRow(table,7,VerifyTypes.InsuranceBenefit,20,21,"second");
				List<InsVerifyGridObject> listInsVerifyGridObjects=AssertSameGridRows(table,isForMedicaid:false,"first row of a key");
				Assert.AreEqual(4,listInsVerifyGridObjects.Count);
				Assert.AreEqual(1,listInsVerifyGridObjects[0].InsVerifyPat.InsVerifyNum);
				Assert.AreEqual(2,listInsVerifyGridObjects[0].InsVerifyPlan.InsVerifyNum,"plan 5's benefits with the first family member");
				Assert.AreEqual(3,listInsVerifyGridObjects[1].InsVerifyPat.InsVerifyNum);
				Assert.IsNull(listInsVerifyGridObjects[1].InsVerifyPlan,"plan 5's benefits are not listed a second time");
				Assert.AreEqual(5,listInsVerifyGridObjects[2].InsVerifyPat.InsVerifyNum,"the first eligibility row of patient plan 20");
				Assert.IsNull(listInsVerifyGridObjects[2].InsVerifyPlan);
				Assert.IsNull(listInsVerifyGridObjects[3].InsVerifyPat);
				Assert.AreEqual(7,listInsVerifyGridObjects[3].InsVerifyPlan.InsVerifyNum);
				//An eligibility row joins the first benefits-only row of its plan, whichever patient plan that came with (Open Dental's predicate looks at
				//the plan, its note and status only): patient plan 31's eligibility joins the benefits row of plan 6 that came with patient plan 30. The
				//benefits row of plan 6 that patient plan 31 brings later is not listed again.
				table=NewGridTable();
				AddGridRow(table,11,VerifyTypes.InsuranceBenefit,30,6);
				AddGridRow(table,12,VerifyTypes.PatientEnrollment,31,6);
				AddGridRow(table,13,VerifyTypes.InsuranceBenefit,31,6);
				AddGridRow(table,14,VerifyTypes.PatientEnrollment,30,6);
				listInsVerifyGridObjects=AssertSameGridRows(table,isForMedicaid:false,"a plan's first benefits row");
				Assert.AreEqual(2,listInsVerifyGridObjects.Count);
				Assert.AreEqual(11,listInsVerifyGridObjects[0].InsVerifyPlan.InsVerifyNum);
				Assert.AreEqual(12,listInsVerifyGridObjects[0].InsVerifyPat.InsVerifyNum,"patient plan 31's eligibility on the first benefits row of plan 6");
				Assert.AreEqual(14,listInsVerifyGridObjects[1].InsVerifyPat.InsVerifyNum,"patient plan 30's eligibility alone: its plan's row is taken");
				Assert.IsNull(listInsVerifyGridObjects[1].InsVerifyPlan);
			}
			finally {
				RestorePrefValues(dictPrefsBefore);
			}
			AssertPrefValuesRestored(dictPrefsBefore);
		}

		[TestMethod]
		public void GridRows_ScratchDatabase_SameAsBefore() {
			//The rows of the list's query (after the choice of the earliest appointment), as GetVerifyGridList turns them into grid rows: every row of
			//the scratch database in the range (the other tests' patients too) and these: a patient whose plan and patient plan have the same key, with
			//a family member on the plan (appointments at the same time and later); a patient with different keys, a second (redundant) benefits row
			//and a note on the eligibility row only; a Medicaid plan with a family member; a plan that renews next month, for the year checks.
			Dictionary<PrefName,string> dictPrefsBefore=GetPrefValues(PrefName.InsVerifyFutureDateBenefitYear,PrefName.InsVerifyFutureDatePatEnrollmentYear,
				PrefName.InsVerifyMedicaidFilingCodes);
			try {
				Coverage coverageEqual=InsertCoveredPatient(KeysEqual());
				InsertFamilyMember(coverageEqual,DateTime.Today.AddDays(2).AddHours(9));
				AppointmentT.CreateAppointment(coverageEqual.Patient.PatNum,DateTime.Today.AddDays(4).AddHours(10),coverageEqual.OperatoryNum,
					coverageEqual.ProvNum);
				long key=GetKeyAboveBoth();
				Coverage coverageApart=InsertCoveredPatient(new long[] { key,key+50 });
				InsVerifies.InsertForPlanNum(coverageApart.InsPlan.PlanNum);
				DataCore.NonQ("UPDATE insverify SET Note='Synthetic note' WHERE VerifyType="+POut.Int((int)VerifyTypes.PatientEnrollment)
					+" AND FKey="+POut.Long(coverageApart.PatPlan.PatPlanNum));
				Coverage coverageMedicaid=InsertCoveredPatient(KeysEqual());
				InsertFamilyMember(coverageMedicaid,DateTime.Today.AddDays(3).AddHours(8));
				InsFilingCode insFilingCode=new InsFilingCode();
				insFilingCode.Descript="Synthetic Medicaid "+NextNum();
				insFilingCode.EclaimCode="MC";
				InsFilingCodes.Insert(insFilingCode);
				DataCore.NonQ("UPDATE insplan SET FilingCode="+POut.Long(insFilingCode.InsFilingCodeNum)+" WHERE PlanNum="
					+POut.Long(coverageMedicaid.InsPlan.PlanNum));
				Prefs.UpdateString(PrefName.InsVerifyMedicaidFilingCodes,POut.Long(insFilingCode.InsFilingCodeNum));
				Coverage coverageRenew=InsertCoveredPatient(KeysEqual(),monthRenew:(byte)DateTime.Today.AddMonths(1).Month);
				AppointmentT.CreateAppointment(coverageRenew.Patient.PatNum,new DateTime(DateTime.Today.AddMonths(1).Year,DateTime.Today.AddMonths(1).Month,2)
					.AddHours(11),coverageRenew.OperatoryNum,coverageRenew.ProvNum);
				Cache.Refresh(InvalidType.Prefs,InvalidType.InsFilingCodes);
				List<long> listInsFilingCodeNumsMedicaid=new List<long> { insFilingCode.InsFilingCodeNum };
				List<long> listInsFilingCodeNumsStandard=InsFilingCodes.GetAll().Select(x => x.InsFilingCodeNum)
					.Where(x => !listInsFilingCodeNumsMedicaid.Contains(x)).ToList();
				listInsFilingCodeNumsStandard.Add(0);
				int countRowsStandard=0;
				int countRowsMedicaid=0;
				ForEachYearCheck(label => {
					foreach(DateTime[] arrayDates in new[] {
						new[] { DateTime.Today,DateTime.Today.AddDays(7) },
						new[] { DateTime.Today.AddDays(-30),DateTime.Today.AddDays(45) } })
					{
						string labelRange=label+", "+arrayDates[0].ToShortDateString()+" through "+arrayDates[1].ToShortDateString();
						DataTable tableStandard=RunGridQuery(arrayDates[0],arrayDates[1],listInsFilingCodeNumsStandard);
						DataTable tableMedicaid=RunGridQuery(arrayDates[0],arrayDates[1],listInsFilingCodeNumsMedicaid);
						List<InsVerifyGridObject> listInsVerifyGridObjectsBefore=GridRowsBeforeInsv4.TableToListInsVerifyGridObjects(tableStandard,false);
						listInsVerifyGridObjectsBefore.AddRange(GridRowsBeforeInsv4.TableToListInsVerifyGridObjects(tableMedicaid,true));
						AssertSameGridRows(tableStandard,isForMedicaid:false,labelRange+", standard");
						AssertSameGridRows(tableMedicaid,isForMedicaid:true,labelRange+", Medicaid");
						//And the whole list as the window asks for it, through GetVerifyGridList.
						DateTime dateLastVerified=DateTime.Today.AddDays(-30);
						List<InsVerifyGridObject> listInsVerifyGridObjectsList=InsVerifies.GetVerifyGridList(arrayDates[0],arrayDates[1],dateLastVerified,
							dateLastVerified,new List<long> { -1 },new List<long> { 0 },0,-1,new List<long>(),false,false,
							arrayDates[0],arrayDates[1],dateLastVerified,dateLastVerified,InsVerifyListType.Both);
						AssertSameGridRows(listInsVerifyGridObjectsBefore,listInsVerifyGridObjectsList,labelRange+", GetVerifyGridList");
						countRowsStandard=Math.Max(countRowsStandard,tableStandard.Rows.Count);
						countRowsMedicaid=Math.Max(countRowsMedicaid,tableMedicaid.Rows.Count);
					}
				});
				Assert.IsTrue(countRowsStandard>=6,"the standard list has rows: "+countRowsStandard);
				Assert.IsTrue(countRowsMedicaid>=4,"the Medicaid list has the plan's rows: "+countRowsMedicaid);
			}
			finally {
				RestorePrefValues(dictPrefsBefore);
			}
			AssertPrefValuesRestored(dictPrefsBefore);
		}
		#endregion Grid rows in linear time

		#region Grid rows helpers
		///<summary>The tables built by hand, by name. Each row: InsVerifyNum, verify type, patient plan, plan, note, status (FKey follows from the type).
		///</summary>
		private static List<KeyValuePair<string,DataTable>> GetHandBuiltCases() {
			List<KeyValuePair<string,DataTable>> listCases=new List<KeyValuePair<string,DataTable>>();
			DataTable table=NewGridTable();
			listCases.Add(new KeyValuePair<string,DataTable>("no rows",table));
			table=NewGridTable();//Two family members on one plan, each with both rows, in the query's order.
			AddGridRow(table,1,VerifyTypes.InsuranceBenefit,12,5);
			AddGridRow(table,2,VerifyTypes.PatientEnrollment,12,5);
			AddGridRow(table,3,VerifyTypes.InsuranceBenefit,13,5);
			AddGridRow(table,4,VerifyTypes.PatientEnrollment,13,5);
			listCases.Add(new KeyValuePair<string,DataTable>("two family members on one plan",table));
			table=NewGridTable();//The same, the second member's benefits row first.
			AddGridRow(table,1,VerifyTypes.InsuranceBenefit,13,5);
			AddGridRow(table,2,VerifyTypes.PatientEnrollment,12,5);
			AddGridRow(table,3,VerifyTypes.InsuranceBenefit,12,5);
			AddGridRow(table,4,VerifyTypes.PatientEnrollment,13,5);
			listCases.Add(new KeyValuePair<string,DataTable>("two family members on one plan, the other first",table));
			table=NewGridTable();//A plan whose number is its patient plan's, both rows: benefits first, then eligibility first, note or status apart.
			AddGridRow(table,1,VerifyTypes.InsuranceBenefit,10,10);
			AddGridRow(table,2,VerifyTypes.PatientEnrollment,10,10);
			AddGridRow(table,3,VerifyTypes.PatientEnrollment,11,14);
			AddGridRow(table,4,VerifyTypes.InsuranceBenefit,11,14);
			AddGridRow(table,5,VerifyTypes.PatientEnrollment,15,16,"Synthetic note");
			AddGridRow(table,6,VerifyTypes.InsuranceBenefit,15,16);
			AddGridRow(table,7,VerifyTypes.InsuranceBenefit,17,18,"",1);
			AddGridRow(table,8,VerifyTypes.PatientEnrollment,17,18);
			AddGridRow(table,9,VerifyTypes.InsuranceBenefit,19,20,"Synthetic note",1);
			AddGridRow(table,10,VerifyTypes.PatientEnrollment,19,20,"Synthetic note",1);
			listCases.Add(new KeyValuePair<string,DataTable>("a plan with an eligibility and a benefits row",table));
			table=NewGridTable();//Plan and patient plan numbers 0, and a row of no verify type.
			AddGridRow(table,1,VerifyTypes.PatientEnrollment,0,0);
			AddGridRow(table,2,VerifyTypes.InsuranceBenefit,0,0);
			AddGridRow(table,3,VerifyTypes.InsuranceBenefit,7,0);
			AddGridRow(table,4,VerifyTypes.PatientEnrollment,7,0);
			AddGridRow(table,5,VerifyTypes.PatientEnrollment,0,8);
			AddGridRow(table,6,VerifyTypes.InsuranceBenefit,0,8);
			AddGridRow(table,7,VerifyTypes.None,9,9);
			AddGridRow(table,8,VerifyTypes.InsuranceBenefit,9,9);
			listCases.Add(new KeyValuePair<string,DataTable>("zero keys",table));
			table=NewGridTable();//The same key more than once: two eligibility rows of a patient plan, two benefits rows of a plan, three patient plans
			//on one plan, and a plan number that is another row's patient plan number (a row added under one key must not be found by the other).
			AddGridRow(table,1,VerifyTypes.PatientEnrollment,20,21,"first");
			AddGridRow(table,2,VerifyTypes.PatientEnrollment,20,21,"second");
			AddGridRow(table,3,VerifyTypes.InsuranceBenefit,20,21,"second");
			AddGridRow(table,4,VerifyTypes.InsuranceBenefit,20,21,"first");
			AddGridRow(table,5,VerifyTypes.InsuranceBenefit,22,23);
			AddGridRow(table,6,VerifyTypes.InsuranceBenefit,24,23);
			AddGridRow(table,7,VerifyTypes.PatientEnrollment,25,23);
			AddGridRow(table,8,VerifyTypes.PatientEnrollment,24,23);
			AddGridRow(table,9,VerifyTypes.PatientEnrollment,22,23);
			AddGridRow(table,10,VerifyTypes.InsuranceBenefit,26,30);
			AddGridRow(table,11,VerifyTypes.PatientEnrollment,30,27);
			AddGridRow(table,12,VerifyTypes.InsuranceBenefit,30,27);
			AddGridRow(table,13,VerifyTypes.PatientEnrollment,26,30);
			listCases.Add(new KeyValuePair<string,DataTable>("duplicate keys",table));
			table=NewGridTable();//An eligibility row that joins another patient plan's benefits row of its plan, then a later benefits row of that plan.
			AddGridRow(table,1,VerifyTypes.PatientEnrollment,40,41);
			AddGridRow(table,2,VerifyTypes.InsuranceBenefit,42,41);
			AddGridRow(table,3,VerifyTypes.PatientEnrollment,43,41);
			AddGridRow(table,4,VerifyTypes.InsuranceBenefit,40,41);
			AddGridRow(table,5,VerifyTypes.PatientEnrollment,42,41);
			AddGridRow(table,6,VerifyTypes.PatientEnrollment,43,41);
			listCases.Add(new KeyValuePair<string,DataTable>("joined across patient plans",table));
			return listCases;
		}

		///<summary>The columns of the list's query that the grid rows read (insverify.* and the joined ones), typed as MySqlConnector fills them.
		///</summary>
		private static DataTable NewGridTable() {
			DataTable table=new DataTable();
			table.Columns.Add("InsVerifyNum",typeof(long));
			table.Columns.Add("DateLastVerified",typeof(DateTime));
			table.Columns.Add("UserNum",typeof(long));
			table.Columns.Add("VerifyType",typeof(sbyte));
			table.Columns.Add("FKey",typeof(long));
			table.Columns.Add("DefNum",typeof(long));
			table.Columns.Add("DateLastAssigned",typeof(DateTime));
			table.Columns.Add("Note",typeof(string));
			table.Columns.Add("DateTimeEntry",typeof(DateTime));
			table.Columns.Add("HoursAvailableForVerification",typeof(double));
			table.Columns.Add("SecDateTEdit",typeof(DateTime));
			table.Columns.Add("SecurityHash",typeof(string));
			table.Columns.Add("LName",typeof(string));
			table.Columns.Add("FName",typeof(string));
			table.Columns.Add("Preferred",typeof(string));
			table.Columns.Add("PatNum",typeof(long));
			table.Columns.Add("AptNum",typeof(long));
			table.Columns.Add("AptDateTime",typeof(DateTime));
			table.Columns.Add("PatPlanNum",typeof(long));
			table.Columns.Add("PlanNum",typeof(long));
			table.Columns.Add("CarrierName",typeof(string));
			table.Columns.Add("ClinicName",typeof(string));
			table.Columns.Add("ClinicNum",typeof(long));
			table.Columns.Add("InsSubNum",typeof(long));
			table.Columns.Add("CarrierNum",typeof(long));
			return table;
		}

		///<summary>A row of the list's query: FKey is the plan for a benefits row, the patient plan for an eligibility row, 0 otherwise. The other values
		///follow from the numbers, so every field differs between rows.</summary>
		private static void AddGridRow(DataTable table,long insVerifyNum,VerifyTypes verifyType,long patPlanNum,long planNum,string note="",long defNum=0) {
			long fKey=(verifyType==VerifyTypes.InsuranceBenefit ? planNum : (verifyType==VerifyTypes.PatientEnrollment ? patPlanNum : 0));
			DateTime dateApt=new DateTime(2026,3,2,8,0,0).AddMinutes(10*insVerifyNum);
			table.Rows.Add(insVerifyNum,new DateTime(2025,1,1).AddDays(insVerifyNum),insVerifyNum%3,(sbyte)verifyType,fKey,defNum,
				new DateTime(2026,1,1,9,30,0).AddDays(insVerifyNum),note,new DateTime(2024,6,1,12,0,0).AddHours(insVerifyNum),0.5*insVerifyNum,
				new DateTime(2026,2,1,7,15,0).AddMinutes(insVerifyNum),"hash"+insVerifyNum,"Synthetic","Insv"+patPlanNum,(patPlanNum%2==0 ? "" : "Pref"),
				1000+patPlanNum,5000+insVerifyNum,dateApt,patPlanNum,planNum,"Syn Carrier "+planNum,(insVerifyNum%2==0 ? "None" : "C"+insVerifyNum),
				insVerifyNum%2==0 ? 0 : insVerifyNum,2000+patPlanNum,300+planNum);
		}

		///<summary>Runs the action with each of the four settings of the year checks, and puts them back as they were afterwards.</summary>
		private static void ForEachYearCheck(Action<string> action) {
			Dictionary<PrefName,string> dictPrefsBefore=GetPrefValues(PrefName.InsVerifyFutureDateBenefitYear,PrefName.InsVerifyFutureDatePatEnrollmentYear);
			try {
				foreach(bool[] arrayChecks in _arrayYearChecks) {
					SetYearChecks(arrayChecks[0],arrayChecks[1]);
					action("benefit year "+(arrayChecks[0] ? "on" : "off")+", eligibility year "+(arrayChecks[1] ? "on" : "off"));
				}
			}
			finally {
				RestorePrefValues(dictPrefsBefore);
			}
			AssertPrefValuesRestored(dictPrefsBefore);
		}

		///<summary>Runs the method as it was (GridRowsBeforeInsv4) and the business layer's (by reflection) on the table and fails unless they give the
		///same grid rows. Returns the business layer's.</summary>
		private static List<InsVerifyGridObject> AssertSameGridRows(DataTable table,bool isForMedicaid,string label) {
			List<InsVerifyGridObject> listInsVerifyGridObjectsBefore=GridRowsBeforeInsv4.TableToListInsVerifyGridObjects(table,isForMedicaid);
			List<InsVerifyGridObject> listInsVerifyGridObjectsNow=TableToListInsVerifyGridObjects(table,isForMedicaid);
			AssertSameGridRows(listInsVerifyGridObjectsBefore,listInsVerifyGridObjectsNow,label);
			return listInsVerifyGridObjectsNow;
		}

		///<summary>Fails unless both lists have the same grid rows in the same order: the Medicaid flag, which halves are there, and every field of
		///each half (the public fields of InsVerify, the database columns and the others, and IsNew and TagOD of TableBase).</summary>
		private static void AssertSameGridRows(List<InsVerifyGridObject> listExpected,List<InsVerifyGridObject> listActual,string label) {
			Assert.AreEqual(listExpected.Count,listActual.Count,label+": the number of grid rows");
			for(int i=0;i<listExpected.Count;i++) {
				string labelRow=label+", grid row "+i;
				Assert.AreEqual(listExpected[i].IsForMedicaidPlan,listActual[i].IsForMedicaidPlan,labelRow+": IsForMedicaidPlan");
				AssertSameHalf(listExpected[i].InsVerifyPat,listActual[i].InsVerifyPat,labelRow+", eligibility half");
				AssertSameHalf(listExpected[i].InsVerifyPlan,listActual[i].InsVerifyPlan,labelRow+", benefits half");
			}
		}

		private static void AssertSameHalf(InsVerify insVerifyExpected,InsVerify insVerifyActual,string label) {
			if(insVerifyExpected==null || insVerifyActual==null) {
				Assert.AreEqual(insVerifyExpected==null,insVerifyActual==null,label+": there or not");
				return;
			}
			Assert.AreNotSame(insVerifyExpected,insVerifyActual,label);
			FieldInfo[] arrayFieldInfos=typeof(InsVerify).GetFields(BindingFlags.Public|BindingFlags.Instance);
			Assert.IsTrue(arrayFieldInfos.Length>=24,"the fields of InsVerify: "+arrayFieldInfos.Length);
			foreach(FieldInfo fieldInfo in arrayFieldInfos) {
				Assert.AreEqual(fieldInfo.GetValue(insVerifyExpected),fieldInfo.GetValue(insVerifyActual),label+": "+fieldInfo.Name);
			}
			Assert.AreEqual(insVerifyExpected.IsNew,insVerifyActual.IsNew,label+": IsNew");
			Assert.AreEqual(insVerifyExpected.TagOD,insVerifyActual.TagOD,label+": TagOD");
		}

		///<summary>InsVerifies.TableToListInsVerifyGridObjects, a private method.</summary>
		private static List<InsVerifyGridObject> TableToListInsVerifyGridObjects(DataTable table,bool isForMedicaid) {
			MethodInfo methodInfo=typeof(InsVerifies).GetMethod("TableToListInsVerifyGridObjects",BindingFlags.NonPublic|BindingFlags.Static);
			Assert.IsNotNull(methodInfo,"InsVerifies.TableToListInsVerifyGridObjects");
			return (List<InsVerifyGridObject>)methodInfo.Invoke(null,new object[] { table,isForMedicaid });
		}

		///<summary>The rows GetVerifyGridList turns into grid rows for one list: the list's query (every filter on "All", verifications older than 30
		///days, plans of the filing codes given) for appointments from dateStart through dateEnd, then the earliest appointment of each key.</summary>
		private static DataTable RunGridQuery(DateTime dateStart,DateTime dateEnd,List<long> listInsFilingCodeNums) {
			MethodInfo methodInfo=typeof(InsVerifies).GetMethod("GetVerifyGridListQuery",BindingFlags.NonPublic|BindingFlags.Static);
			Assert.IsNotNull(methodInfo,"InsVerifies.GetVerifyGridListQuery");
			MethodInfo methodInfoChoice=typeof(InsVerifies).GetMethod("RemoveLaterAppointmentRows",BindingFlags.NonPublic|BindingFlags.Static);
			Assert.IsNotNull(methodInfoChoice,"InsVerifies.RemoveLaterAppointmentRows");
			DateTime dateLastVerified=DateTime.Today.AddDays(-30);
			string command=(string)methodInfo.Invoke(null,new object[] { dateStart,dateEnd,dateLastVerified,dateLastVerified,
				new List<long> { -1 },new List<long> { 0 },0L,-1L,new List<long>(),false,false,listInsFilingCodeNums });
			DataTable table=DataCore.GetTable(command);
			methodInfoChoice.Invoke(null,new object[] { table });
			return table;
		}

		///<summary>A family member on the coverage's plan (a patient plan on the same subscriber row) with an appointment at the time given.</summary>
		private static PatPlan InsertFamilyMember(Coverage coverage,DateTime dateTimeApt) {
			Patient patientFamily=PatientT.CreatePatient(priProvNum:coverage.ProvNum,lName:"Synthetic",fName:"InsvFamily"+NextNum(),
				birthDate:new DateTime(1985,1,1));
			PatPlan patPlanFamily=PatPlanT.CreatePatPlan(1,patientFamily.PatNum,coverage.InsSub.InsSubNum);
			AppointmentT.CreateAppointment(patientFamily.PatNum,dateTimeApt,coverage.OperatoryNum,coverage.ProvNum);
			return patPlanFamily;
		}
		#endregion Grid rows helpers
	}
}
