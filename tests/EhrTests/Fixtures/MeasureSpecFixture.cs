// The scratch database and the synthetic practice of the measure fixtures (package EHR-4b of docs/specs/ehr-rebuild-plan.md, section 7 and
// section 9.3), on the local MariaDB (127.0.0.1:3306, root, blank password). Built as Support/MeasureCalcFixture.cs builds the EHR-4a one, from
// the committed db/init/new-database.sql and the fork migrations, under its own names: every name starts with ehr4b_ plus this run's id, so these
// tests never touch another scratch database. Built by the first test that needs it, dropped by the class's cleanup; leftovers of earlier runs
// older than 12 hours are dropped first, and only names starting with ehr4b_. EHR_TEST_DB=skip skips (inconclusive), EHR_KEEP_DB=1 keeps it.
//
// The data rule of the plan's section 6: made-up names; made-up codes that start with TEST; CDCREC codes (a US government code system) for race
// and ethnicity; and the codes the engine's own source compares with (a smoking status, a positive/negative lab result), read from
// EhrMeasures.cs at run time (MeasureSpecCodes), never written here. No code of any licensed code system is in this file.
//
// One provider reports (an ehrprovkey row for its names, and a made-up all-digit NPI); a second provider has no row. Each case of
// MeasureSpecTests has its own patients, registered under a key such as "Vs.Child2", with only the rows its measures read, so a case counts its
// patients' rows of the engine's audit table whatever the other cases add. The reporting period is the calendar year 2014, and every patient is
// born before 14 November 2013: the engine takes the smoking (13 or older) and the first Stage 1 vital signs (2 or older) ages on today's date,
// so on any day from 14 November 2026 on every patient is old enough for both, and the golden numbers do not drift. Days of the week matter to
// the business-day rules: 3 March 2014 is a Monday.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DataConnectionBase;
using EhrLaboratories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;

namespace EhrTests {
	///<summary>The ehr4b_ scratch database and the synthetic practice the measure fixture tests calculate on.</summary>
	public static class MeasureSpecFixture {
		public const string PREFIX="ehr4b_";
		private static readonly TimeSpan _timeSpanStale=TimeSpan.FromHours(12);
		private static readonly string _runId=DateTime.UtcNow.ToString("yyMMddHHmmss",CultureInfo.InvariantCulture)+"_"
			+System.Diagnostics.Process.GetCurrentProcess().Id+"_"+new Random().Next(1000,10000);
		public static readonly string DbName=Name("main");
		private static readonly object _lockBuild=new object();
		private static bool _isBuildTried;
		private static bool _isBuilt;
		private static string _skipReason;
		private static string _buildError;
		private static CultureInfo _cultureBefore;
		///<summary>The reporting period: the calendar year 2014.</summary>
		public static readonly DateTime DateStart=new DateTime(2014,1,1);
		public static readonly DateTime DateEnd=new DateTime(2014,12,31);
		///<summary>The day of most visits: Monday 3 March 2014.</summary>
		public static readonly DateTime DateMonday=new DateTime(2014,3,3);
		///<summary>Every patient is born before this day (see the header).</summary>
		public static readonly DateTime DateBornBefore=new DateTime(2013,11,14);
		///<summary>The hour of the day of every event: the program writes an event with the time it happened, not at midnight.</summary>
		public const int EVENT_HOUR=11;
		///<summary>A made-up NPI: digits only, because the engine puts the NPIs into an IN() list without quotes.</summary>
		public const string NPI="1000000004";
		///<summary>The provider with an ehrprovkey row: the EP of every case.</summary>
		public static Provider ProviderEp;
		///<summary>A provider with no ehrprovkey row.</summary>
		public static Provider ProviderOther;
		///<summary>The patients of the cases, by key.</summary>
		private static readonly Dictionary<string,Patient> _dictPatients=new Dictionary<string,Patient>();
		///<summary>The engine's audit table of each stage and measure on the fixture, fetched once per run.</summary>
		private static readonly Dictionary<string,DataTable> _dictAudit=new Dictionary<string,DataTable>();
		///<summary>The three SelectAll results on the fixture, calculated once per run.</summary>
		private static readonly Dictionary<EhrMeasureStage,List<EhrMeasure>> _dictSelectAll=new Dictionary<EhrMeasureStage,List<EhrMeasure>>();
		private static long _codeNumVisit;
		private static long _referralNum;

		public static string Name(string purpose) {
			string name=PREFIX+_runId+"_"+purpose;
			if(name.Length>64 || !Regex.IsMatch(name,"^[a-z0-9_]+$")) {
				throw new ArgumentException("Not a usable scratch database name: "+name);
			}
			return name;
		}

		///<summary>Call first in each database test: builds the database and the fixture on the first call, skips when there is no server,
		///otherwise connects to it directly (no Middle Tier) as user 1 in the en-US culture.</summary>
		public static void Begin() {
			lock(_lockBuild) {
				if(!_isBuildTried) {
					_isBuildTried=true;
					Build();
				}
			}
			if(_skipReason!=null) {
				Assert.Inconclusive(_skipReason);
			}
			Assert.IsNull(_buildError,_buildError);
			if(_cultureBefore==null) {
				_cultureBefore=Thread.CurrentThread.CurrentCulture;
			}
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
			UseDirectConnection();
			RefreshCaches();
		}

		///<summary>Call from [TestCleanup]: the culture back, and the tests of the other classes back on the EHR-1 database when it exists.</summary>
		public static void End() {
			if(_cultureBefore!=null) {
				Thread.CurrentThread.CurrentCulture=_cultureBefore;
				_cultureBefore=null;
			}
			ScratchDb.UseDirectConnection();
			if(_isBuilt && ScratchDb.SkipReason==null && Exists(ScratchDb.DbName)) {
				ScratchDb.Connect(ScratchDb.DbName);
				RefreshCaches();
			}
		}

		///<summary>Call from [ClassCleanup].</summary>
		public static void DropAll() {
			if(!_isBuilt || Environment.GetEnvironmentVariable("EHR_KEEP_DB")=="1") {
				return;
			}
			try {
				Drop(DbName);
			}
			catch(Exception) {
				//The stale clean-up of a later run drops it.
			}
		}

		public static void UseDirectConnection() {
			ScratchDb.UseDirectConnection();
			ScratchDb.Connect(DbName);
		}

		///<summary>The patient of a case by its key.</summary>
		public static Patient GetPatient(string key) {
			if(!_dictPatients.TryGetValue(key,out Patient patient)) {
				throw new ArgumentException("No fixture patient "+key);
			}
			return patient;
		}

		///<summary>Every fixture patient.</summary>
		public static List<Patient> GetPatients() {
			return _dictPatients.Values.ToList();
		}

		///<summary>The engine's audit table of the measure (GetAuditTable: GetTable, GetTableMu2 or GetTableMu2Mod) for the EP and the period, one
		///row per unit of the denominator and "X" in met for the numerator: the table SelectAll, SelectAllMu2 and SelectAllMu2Mod count.</summary>
		public static DataTable GetAudit(EhrMeasureStage stage,EhrMeasureType measureType) {
			string key=stage+"."+measureType;
			if(!_dictAudit.TryGetValue(key,out DataTable table)) {
				table=EhrMeasures.GetAuditTable(stage,measureType,DateStart,DateEnd,ProviderEp.ProvNum);
				Assert.IsNotNull(table,key+" has an audit table.");
				_dictAudit[key]=table;
			}
			return table;
		}

		///<summary>The engine's numerator and denominator of the measure for the patients under the keys only: the audit table's rows of those
		///patients, and the met ones among them.</summary>
		public static void Count(EhrMeasureStage stage,EhrMeasureType measureType,IEnumerable<string> listKeys,out int numerator,out int denominator) {
			HashSet<string> setPatNums=new HashSet<string>(listKeys.Select(x => GetPatient(x).PatNum.ToString(CultureInfo.InvariantCulture)));
			List<DataRow> listRows=GetAudit(stage,measureType).Rows.Cast<DataRow>().Where(x => setPatNums.Contains(x["PatNum"].ToString())).ToList();
			denominator=listRows.Count;
			numerator=listRows.Count(x => x["met"].ToString()=="X");
		}

		///<summary>The result of SelectAll (Stage 1), SelectAllMu2 (Stage 2) or SelectAllMu2Mod (Modified Stage 2) for the EP on the fixture, once
		///per run.</summary>
		public static List<EhrMeasure> GetSelectAll(EhrMeasureStage stage) {
			if(!_dictSelectAll.TryGetValue(stage,out List<EhrMeasure> listEhrMeasures)) {
				long provNum=ProviderEp.ProvNum;
				switch(stage) {
					case EhrMeasureStage.Stage1:
						listEhrMeasures=EhrMeasures.SelectAll(DateStart,DateEnd,provNum);
						break;
					case EhrMeasureStage.Stage2:
						listEhrMeasures=EhrMeasures.SelectAllMu2(DateStart,DateEnd,provNum);
						break;
					default:
						listEhrMeasures=EhrMeasures.SelectAllMu2Mod(DateStart,DateEnd,provNum);
						break;
				}
				_dictSelectAll[stage]=listEhrMeasures;
			}
			return _dictSelectAll[stage];
		}

		private static void RefreshCaches() {
			if(!Prefs.DictIsNull()) {
				Prefs.ClearCache();
			}
			Cache.Refresh(InvalidType.Prefs,InvalidType.Providers,InvalidType.Security,InvalidType.ProcCodes);
			Userods.RefreshCache();
			Security.CurUser=Userods.GetUser(1);
			Security.PasswordTyped="";
		}

		private static void Build() {
			_skipReason=ScratchDb.GetSkipReason();
			if(_skipReason!=null) {
				return;
			}
			CultureInfo cultureBefore=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
			try {
				DropStale();
				_isBuilt=true;//Set before the database exists, so DropAll drops a half-built one too (Drop uses IF EXISTS).
				using(MySqlConnection connection=ScratchDb.OpenServer()) {
					ScratchDb.Exec(connection,"CREATE DATABASE `"+DbName+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
				}
				string text=File.ReadAllText(TestPaths.Combine(ScratchDb.RepoPathInitSql),new UTF8Encoding(false)).Replace("\r\n","\n");
				using(MySqlConnection connection=ScratchDb.OpenServer(DbName)) {
					//Every statement of the generated file ends with a semicolon at the end of a line, and no value holds a raw line break.
					foreach(string statement in text.Split(new[] { ";\n" },StringSplitOptions.None)) {
						if(statement.Trim()=="") {
							continue;
						}
						ScratchDb.Exec(connection,statement);
					}
				}
				UseDirectConnection();
				RefreshCaches();
				ForkMigrationRunner.Run();
				RefreshCaches();
				MakeRows();
				RefreshCaches();
			}
			catch(Exception ex) {
				_buildError="Could not build the ehr4b_ scratch database: "+ex.GetBaseException().Message;
			}
			finally {
				Thread.CurrentThread.CurrentCulture=cultureBefore;
			}
		}

		#region Synthetic rows
		///<summary>A day of 2014.</summary>
		private static DateTime D(int month,int day) {
			return new DateTime(2014,month,day);
		}

		///<summary>Every case of MeasureSpecTests. The comments name the measures that read each group.</summary>
		private static void MakeRows() {
			_dictPatients.Clear();
			_dictAudit.Clear();
			_dictSelectAll.Clear();
			string lName=ScratchDb.UniqueName("Ehrspeclast");
			string fName=ScratchDb.UniqueName("Ehrspecfirst");
			ProviderEp=new Provider();
			ProviderEp.Abbr=ScratchDb.UniqueName("Ehrspec");
			ProviderEp.LName=lName;
			ProviderEp.FName=fName;
			ProviderEp.NationalProvID=NPI;
			Providers.Insert(ProviderEp);
			ProviderOther=ScratchDb.MakeProvider(ScratchDb.UniqueName("Ehrotherlast"),ScratchDb.UniqueName("Ehrotherfirst"));
			Providers.RefreshCache();
			EhrProvKey ehrProvKey=new EhrProvKey();
			ehrProvKey.LName=lName;
			ehrProvKey.FName=fName;
			ehrProvKey.YearValue=14;
			ehrProvKey.ProvKey="";
			EhrProvKeys.Insert(ehrProvKey);
			ProcedureCode procedureCode=new ProcedureCode();
			procedureCode.ProcCode="TESTV01";
			procedureCode.Descript="Synthetic office visit";
			procedureCode.AbbrDesc="Visit";
			_codeNumVisit=OpenPmsDentalBusiness.Crud.ProcedureCodeCrud.Insert(procedureCode);
			Referral referral=new Referral();
			referral.LName=ScratchDb.UniqueName("Ehrreflast");
			referral.FName=ScratchDb.UniqueName("Ehrreffirst");
			referral.IsDoctor=true;
			_referralNum=OpenPmsDentalBusiness.Crud.ReferralCrud.Insert(referral);
			MakeListRows();
			MakeDemographicsRows();
			MakeOrderRows();
			MakeVitalSignRows();
			MakeLabRows();
			MakeVisitRows();
			MakeReminderRows();
			MakeTransitionRows();
			MakePortalRows();
			foreach(Patient patient in _dictPatients.Values) {
				if(patient.Birthdate>=DateBornBefore) {
					throw new ApplicationException("Every fixture patient is born before "+DateBornBefore.ToShortDateString()+" (see the header).");
				}
			}
		}

		///<summary>Problem list, medication list and medication allergy list (Stage 1), with the three "none known" entries the preferences name;
		///CPOE by patient (Stage 1); family history (Stage 2).</summary>
		private static void MakeListRows() {
			DiseaseDef diseaseDefCoded=new DiseaseDef();
			diseaseDefCoded.DiseaseName="Synthetic problem";
			diseaseDefCoded.SnomedCode="TEST0001";//Made up: the engine counts a problem whose definition has a code.
			long diseaseDefNumCoded=OpenPmsDentalBusiness.Crud.DiseaseDefCrud.Insert(diseaseDefCoded);
			DiseaseDef diseaseDefNone=new DiseaseDef();
			diseaseDefNone.DiseaseName="Synthetic no known problems";
			long diseaseDefNumNone=OpenPmsDentalBusiness.Crud.DiseaseDefCrud.Insert(diseaseDefNone);
			Prefs.UpdateLong(PrefName.ProblemsIndicateNone,diseaseDefNumNone);
			Medication medication=new Medication();
			medication.MedName="Synthetic medication";
			long medicationNum=OpenPmsDentalBusiness.Crud.MedicationCrud.Insert(medication);
			Medication medicationNone=new Medication();
			medicationNone.MedName="Synthetic no medications";
			long medicationNumNone=OpenPmsDentalBusiness.Crud.MedicationCrud.Insert(medicationNone);
			Prefs.UpdateLong(PrefName.MedicationsIndicateNone,medicationNumNone);
			AllergyDef allergyDef=new AllergyDef();
			allergyDef.Description="Synthetic allergy";
			long allergyDefNum=OpenPmsDentalBusiness.Crud.AllergyDefCrud.Insert(allergyDef);
			AllergyDef allergyDefNone=new AllergyDef();
			allergyDefNone.Description="Synthetic no known allergies";
			long allergyDefNumNone=OpenPmsDentalBusiness.Crud.AllergyDefCrud.Insert(allergyDefNone);
			Prefs.UpdateLong(PrefName.AllergiesIndicateNone,allergyDefNumNone);
			RefreshPrefs();
			//ProblemList
			MakeDisease(SeenPatient("Problem.Coded"),diseaseDefNumCoded);
			MakeDisease(SeenPatient("Problem.NoneKnown"),diseaseDefNumNone);
			SeenPatient("Problem.Empty");
			//MedicationList
			MakeMedicationPat(SeenPatient("MedList.Entry"),medicationNum,0,D(1,10),"",false);
			MakeMedicationPat(SeenPatient("MedList.NoneKnown"),medicationNumNone,0,D(1,10),"",false);
			SeenPatient("MedList.Empty");
			//ProvOrderEntry (Stage 1 CPOE by patient): a medication in the list, ordered through CPOE or entered in the medical window.
			MakeMedicationPat(SeenPatient("Cpoe.ListCpoe"),medicationNum,ProviderEp.ProvNum,D(3,3),"Synthetic sig",true);
			MakeMedicationPat(SeenPatient("Cpoe.ListNotCpoe"),medicationNum,0,D(1,10),"",false);
			SeenPatient("Cpoe.NoMeds");
			//AllergyList
			MakeAllergy(SeenPatient("Allergy.Entry"),allergyDefNum);
			MakeAllergy(SeenPatient("Allergy.NoneKnown"),allergyDefNumNone);
			SeenPatient("Allergy.Empty");
			//FamilyHistory (Stage 2): a first-degree relative.
			FamilyHealth familyHealth=new FamilyHealth();
			familyHealth.PatNum=SeenPatient("Fam.Parent").PatNum;
			familyHealth.Relationship=FamilyRelationship.Parent;
			familyHealth.DiseaseDefNum=diseaseDefNumCoded;
			familyHealth.PersonName="Synthetic parent";
			OpenPmsDentalBusiness.Crud.FamilyHealthCrud.Insert(familyHealth);
			SeenPatient("Fam.None");
		}

		///<summary>Demographics: CDCREC race and ethnicity codes (a US government code system the data rule allows), and the "declined" codes the
		///program itself defines (PatientRace's constants).</summary>
		private static void MakeDemographicsRows() {
			MakeCdcrec("2106-3","R5","White");
			MakeCdcrec("2186-5","E2","Not Hispanic or Latino");
			Patient patientAll=SeenPatient("Demo.All");
			MakePatientRace(patientAll,"2106-3",false);
			MakePatientRace(patientAll,"2186-5",true);
			Patient patientDeclined=SeenPatient("Demo.Declined");
			MakePatientRace(patientDeclined,PatientRace.DECLINE_SPECIFY_RACE_CODE,false);
			MakePatientRace(patientDeclined,PatientRace.DECLINE_SPECIFY_ETHNICITY_CODE,true);
			MakePatientRace(SeenPatient("Demo.NoEthnicity"),"2106-3",false);
		}

		///<summary>Medication orders (CPOE), prescriptions (e-prescribing), patient education.</summary>
		private static void MakeOrderRows() {
			//CPOE_MedOrdersOnly: orders as an e-prescription leaves them (MedicationNum 0, the provider, a sig, the start date).
			MakeMedicationPat(SeenPatient("Order.Cpoe"),0,ProviderEp.ProvNum,D(3,3),"Synthetic sig",true);
			MakeMedicationPat(SeenPatient("Order.NotCpoe"),0,ProviderEp.ProvNum,D(3,3),"Synthetic sig",false);
			//Rx: three sent electronically and one printed (the plan's example: 4 prescriptions, 3 electronic, 75%), and a controlled substance.
			Patient patientRx=SeenPatient("Rx.Three");
			MakeRx(patientRx,D(3,3),RxSendStatus.SentElect,false);
			MakeRx(patientRx,D(4,7),RxSendStatus.SentElect,false);
			MakeRx(patientRx,D(5,5),RxSendStatus.SentElect,false);
			MakeRx(SeenPatient("Rx.Printed"),D(3,3),RxSendStatus.Printed,false);
			MakeRx(SeenPatient("Rx.Controlled"),D(3,3),RxSendStatus.SentElect,true);
			//Education: in the period, none, and only the year before.
			MakeEvent(SeenPatient("Edu.Given"),EhrMeasureEventType.EducationProvided,D(3,3),0);
			SeenPatient("Edu.None");
			MakeEvent(SeenPatient("Edu.Before"),EhrMeasureEventType.EducationProvided,new DateTime(2013,6,3),0);
		}

		///<summary>Vital signs and smoking status. Ages at the visit of 3 March 2014: Vs.Child2 is 2 and Smoke.Age12 12 (both all year),
		///Vs.Infant 1; the others are adults.</summary>
		private static void MakeVitalSignRows() {
			MakeVitalsign(SeenPatient("Vs.Full"),64,150,120,80);
			MakeVitalsign(SeenPatient("Vs.HeightWeight"),64,150,0,0);
			//Born 1 January 2012: 2 years old on every day of 2014.
			MakeVitalsign(SeenPatient("Vs.Child2",new DateTime(2012,1,1)),36,30,0,0);
			MakeVitalsign(SeenPatient("Vs.Bp"),0,0,120,80);
			SeenPatient("Vs.None");
			MakeVitalsign(SeenPatient("Vs.Infant",new DateTime(2013,1,15)),30,22,0,0);
			Patient patientSmoker=SeenPatient("Smoke.Recorded");
			patientSmoker.SmokingSnoMed=MeasureSpecCodes.GetSmokingStatusCode();
			OpenPmsDentalBusiness.Crud.PatientCrud.Update(patientSmoker);
			SeenPatient("Smoke.NotRecorded");
			//12 on every day of the 2014 period (13 on 15 January 2015), so the D03 case holds whichever day of the period the sheet's age is
			//taken on.
			SeenPatient("Smoke.Age12",new DateTime(2002,1,15));
		}

		///<summary>Lab orders and results (ehrlab, by the provider's NPI; and the older medicalorder rows), laboratory and radiology orders, imaging
		///results.</summary>
		private static void MakeLabRows() {
			//A made-up LOINC-format radiology code: the engine tells radiology from other lab orders by the loinc table's class.
			Loinc loinc=new Loinc();
			loinc.LoincCode="TESTRAD1";
			loinc.ClassType="RAD";
			loinc.NameShort="Synthetic imaging study";
			OpenPmsDentalBusiness.Crud.LoincCrud.Insert(loinc);
			//Lab: a numeric result, a coded positive/negative result (the engine's own code), no result incorporated, a free-text result.
			MakeLabResult(MakeEhrLab(SeenPatient("Lab.Numeric"),false,"",""),HL70125.NM,"");
			MakeLabResult(MakeEhrLab(SeenPatient("Lab.Coded"),false,"",""),HL70125.CWE,MeasureSpecCodes.GetLabResultCode());
			MakeEhrLab(SeenPatient("Lab.NoResult"),false,"","");
			MakeLabResult(MakeEhrLab(SeenPatient("Lab.Text"),false,"",""),HL70125.ST,"");
			//The older lab orders (medical order and lab panel).
			Patient patientPanel=SeenPatient("Lab.OldPanel");
			long medicalOrderNum=MakeLabOrder(patientPanel);
			LabPanel labPanel=new LabPanel();
			labPanel.PatNum=patientPanel.PatNum;
			labPanel.MedicalOrderNum=medicalOrderNum;
			labPanel.ServiceName="Synthetic panel";
			OpenPmsDentalBusiness.Crud.LabPanelCrud.Insert(labPanel);
			MakeLabOrder(SeenPatient("Lab.OldNoPanel"));
			//CPOE of laboratory and radiology orders.
			MakeEhrLab(SeenPatient("CpoeLab.Cpoe"),true,"","");
			MakeEhrLab(SeenPatient("CpoeLab.NotCpoe"),false,"","");
			MakeEhrLab(SeenPatient("Rad.Cpoe"),true,"TESTRAD1","LN");
			MakeEhrLab(SeenPatient("Rad.NotCpoe"),false,"TESTRAD1","LN");
			//LabImages: an image attached, and a test still expecting its image (DocNum -1, EhrLabImage's own convention).
			Patient patientImage=SeenPatient("Img.Attached");
			Document document=new Document();
			document.PatNum=patientImage.PatNum;
			document.Description="Synthetic image";
			document.DateCreated=D(3,3);
			long docNum=OpenPmsDentalBusiness.Crud.DocumentCrud.Insert(document);
			MakeLabImage(MakeEhrLab(patientImage,true,"TESTRAD1","LN"),docNum);
			MakeLabImage(MakeEhrLab(SeenPatient("Img.Expected"),true,"TESTRAD1","LN"),-1);
		}

		///<summary>Clinical summaries (by visit), electronic notes.</summary>
		private static void MakeVisitRows() {
			//Seen on Monday 3 March: a summary the same day, the next day, on the Thursday (the third business day), on the Friday (the fourth),
			//and none.
			MakeEvent(SeenPatient("ClinSum.SameDay"),EhrMeasureEventType.ClinicalSummaryProvidedToPt,D(3,3),0);
			MakeEvent(SeenPatient("ClinSum.NextDay"),EhrMeasureEventType.ClinicalSummaryProvidedToPt,D(3,4),0);
			MakeEvent(SeenPatient("ClinSum.Thursday"),EhrMeasureEventType.ClinicalSummaryProvidedToPt,D(3,6),0);
			MakeEvent(SeenPatient("ClinSum.Friday"),EhrMeasureEventType.ClinicalSummaryProvidedToPt,D(3,7),0);
			SeenPatient("ClinSum.None");
			//Seen on Friday 7 March, the summary on Monday 10 March: one business day.
			Patient patient=MakePatient("ClinSum.FridayMonday",new DateTime(1980,6,15),ProviderEp.ProvNum);
			MakeVisit(patient,D(3,7),ProviderEp.ProvNum);
			MakeEvent(patient,EhrMeasureEventType.ClinicalSummaryProvidedToPt,D(3,10),0);
			//Two visits (Monday 3 March and Monday 2 June), one summary, on the day of the first.
			patient=SeenPatient("ClinSum.TwoVisits");
			MakeVisit(patient,D(6,2),ProviderEp.ProvNum);
			MakeEvent(patient,EhrMeasureEventType.ClinicalSummaryProvidedToPt,D(3,3),0);
			//ElectronicNote: a signed note on the visit, an unsigned one, and none.
			patient=MakePatient("Note.Signed",new DateTime(1980,6,15),ProviderEp.ProvNum);
			MakeProcNote(patient,MakeVisit(patient,D(3,3),ProviderEp.ProvNum),"TESTSIGNATURE");
			patient=MakePatient("Note.Unsigned",new DateTime(1980,6,15),ProviderEp.ProvNum);
			MakeProcNote(patient,MakeVisit(patient,D(3,3),ProviderEp.ProvNum),"");
			SeenPatient("Note.None");
		}

		///<summary>Reminders: Stage 1 (65 or older, 5 or younger, the provider's patients) and Stage 2 (two or more office visits with the EP in the
		///24 months before the period).</summary>
		private static void MakeReminderRows() {
			MakeEvent(SeenPatient("Rem1.Senior",new DateTime(1945,2,1)),EhrMeasureEventType.ReminderSent,D(3,3),0);
			SeenPatient("Rem1.SeniorNone",new DateTime(1945,2,1));
			MakeEvent(SeenPatient("Rem1.Child3",new DateTime(2010,6,1)),EhrMeasureEventType.ReminderSent,D(3,3),0);
			MakeEvent(SeenPatient("Rem1.Adult"),EhrMeasureEventType.ReminderSent,D(3,3),0);
			//Born 1 January 2009: 5 years old on every day of 2014.
			MakeEvent(SeenPatient("Rem1.Age5",new DateTime(2009,1,1)),EhrMeasureEventType.ReminderSent,D(3,3),0);
			DateTime dateVisit1=new DateTime(2012,5,7);
			DateTime dateVisit2=new DateTime(2013,5,6);
			Patient patient=MakePatient("Rem2.TwoVisits",new DateTime(1980,6,15),ProviderEp.ProvNum);
			MakeVisit(patient,dateVisit1,ProviderEp.ProvNum);
			MakeVisit(patient,dateVisit2,ProviderEp.ProvNum);
			MakeEvent(patient,EhrMeasureEventType.ReminderSent,D(3,3),0);
			patient=MakePatient("Rem2.TwoVisitsNone",new DateTime(1980,6,15),ProviderEp.ProvNum);
			MakeVisit(patient,dateVisit1,ProviderEp.ProvNum);
			MakeVisit(patient,dateVisit2,ProviderEp.ProvNum);
			patient=MakePatient("Rem2.OneVisit",new DateTime(1980,6,15),ProviderEp.ProvNum);
			MakeVisit(patient,dateVisit2,ProviderEp.ProvNum);
			MakeEvent(patient,EhrMeasureEventType.ReminderSent,D(3,3),0);
			//Both visits with the EP, but the other provider is the primary provider.
			patient=MakePatient("Rem2.OtherPrimary",new DateTime(1980,6,15),ProviderOther.ProvNum);
			MakeVisit(patient,dateVisit1,ProviderEp.ProvNum);
			MakeVisit(patient,dateVisit2,ProviderEp.ProvNum);
			MakeEvent(patient,EhrMeasureEventType.ReminderSent,D(3,3),0);
			//The EP is the primary provider, but both visits were with the other provider.
			patient=MakePatient("Rem2.VisitsWithOther",new DateTime(1980,6,15),ProviderEp.ProvNum);
			MakeVisit(patient,dateVisit1,ProviderOther.ProvNum);
			MakeVisit(patient,dateVisit2,ProviderOther.ProvNum);
			MakeEvent(patient,EhrMeasureEventType.ReminderSent,D(3,3),0);
		}

		///<summary>Transitions of care: received (medication reconciliation) and sent (summary of care).</summary>
		private static void MakeTransitionRows() {
			Patient patient=SeenPatient("MedRec.Reconciled");
			MakeRefAttach(patient,ReferralType.RefFrom,D(3,3));
			MakeEvent(patient,EhrMeasureEventType.MedicationReconcile,D(3,3),0);
			MakeRefAttach(SeenPatient("MedRec.NotReconciled"),ReferralType.RefFrom,D(3,3));
			//Two transitions received (3 March, 2 June), one reconciliation.
			patient=SeenPatient("MedRec.TwoOne");
			MakeRefAttach(patient,ReferralType.RefFrom,D(3,3));
			MakeRefAttach(patient,ReferralType.RefFrom,D(6,2));
			MakeEvent(patient,EhrMeasureEventType.MedicationReconcile,D(3,3),0);
			//A transition received by the EP and reconciled, for a patient with no office visit in the period.
			patient=MakePatient("MedRec.NoVisit",new DateTime(1980,6,15),ProviderEp.ProvNum);
			MakeRefAttach(patient,ReferralType.RefFrom,D(3,3));
			MakeEvent(patient,EhrMeasureEventType.MedicationReconcile,D(3,3),0);
			//Referrals by the EP: a summary of care provided, none, and one sent electronically (the program writes both events then).
			patient=SeenPatient("Soc.Sent");
			MakeEvent(patient,EhrMeasureEventType.SummaryOfCareProvidedToDr,D(3,3),MakeRefAttach(patient,ReferralType.RefTo,D(3,3)));
			MakeRefAttach(SeenPatient("Soc.NotSent"),ReferralType.RefTo,D(3,3));
			patient=SeenPatient("Soc.Electronic");
			long refAttachNum=MakeRefAttach(patient,ReferralType.RefTo,D(3,3));
			MakeEvent(patient,EhrMeasureEventType.SummaryOfCareProvidedToDr,D(3,3),refAttachNum);
			MakeEvent(patient,EhrMeasureEventType.SummaryOfCareProvidedToDrElectronic,D(3,3),refAttachNum);
		}

		///<summary>Patient electronic access (timely online access; view, download or transmit) and secure messaging.</summary>
		private static void MakePortalRows() {
			//Seen on Monday 3 March: access the same day, on the Thursday (the third business day), on the Friday (the fourth), on the Tuesday of
			//the next week (the sixth), and none.
			MakeEvent(SeenPatient("Access.SameDay"),EhrMeasureEventType.OnlineAccessProvided,D(3,3),0);
			MakeEvent(SeenPatient("Access.Thursday"),EhrMeasureEventType.OnlineAccessProvided,D(3,6),0);
			MakeEvent(SeenPatient("Access.Friday"),EhrMeasureEventType.OnlineAccessProvided,D(3,7),0);
			MakeEvent(SeenPatient("Access.NextTuesday"),EhrMeasureEventType.OnlineAccessProvided,D(3,11),0);
			SeenPatient("Access.None");
			//Seen on Tuesday 4 March; access on Monday 10 March, the fourth business day after it.
			Patient patient=MakePatient("Access.TuesdayMonday",new DateTime(1980,6,15),ProviderEp.ProvNum);
			MakeVisit(patient,D(3,4),ProviderEp.ProvNum);
			MakeEvent(patient,EhrMeasureEventType.OnlineAccessProvided,D(3,10),0);
			//View, download or transmit: in the period, none, and only the year before.
			MakeEvent(SeenPatient("Vdt.Viewed"),EhrMeasureEventType.ElectronicCopyRequested,D(4,7),0);
			SeenPatient("Vdt.None");
			MakeEvent(SeenPatient("Vdt.Before"),EhrMeasureEventType.ElectronicCopyRequested,new DateTime(2013,6,3),0);
			//Secure messages from the patient: in the period, none, and only the year before.
			MakeEvent(SeenPatient("Msg.FromPatient"),EhrMeasureEventType.SecureMessageFromPat,D(4,7),0);
			SeenPatient("Msg.None");
			MakeEvent(SeenPatient("Msg.Before"),EhrMeasureEventType.SecureMessageFromPat,new DateTime(2013,6,3),0);
			//A secure (web mail) message sent by the EP to the patient in the period, and none.
			Patient patientWebMail=SeenPatient("Web.Sent");
			EmailMessage emailMessage=new EmailMessage();
			emailMessage.PatNum=patientWebMail.PatNum;
			emailMessage.PatNumSubj=patientWebMail.PatNum;
			emailMessage.ProvNumWebMail=ProviderEp.ProvNum;
			emailMessage.SentOrReceived=EmailMessages.GetSentTypes(EmailPlatform.WebMail).First();
			emailMessage.MsgDateTime=D(4,7).AddHours(EVENT_HOUR);
			emailMessage.Subject="Synthetic message";
			emailMessage.BodyText="Synthetic text.";
			OpenPmsDentalBusiness.Crud.EmailMessageCrud.Insert(emailMessage);
			SeenPatient("Web.None");
		}

		///<summary>An adult patient with one completed office visit with the EP on Monday 3 March 2014.</summary>
		private static Patient SeenPatient(string key) {
			return SeenPatient(key,new DateTime(1980,6,15));
		}

		private static Patient SeenPatient(string key,DateTime birthdate) {
			Patient patient=MakePatient(key,birthdate,ProviderEp.ProvNum);
			MakeVisit(patient,DateMonday,ProviderEp.ProvNum);
			return patient;
		}

		///<summary>A patient with every demographic element but race and ethnicity (patientrace rows), under the key.</summary>
		private static Patient MakePatient(string key,DateTime birthdate,long priProv) {
			if(_dictPatients.ContainsKey(key)) {
				throw new ArgumentException("Two fixture patients with the key "+key);
			}
			Patient patient=new Patient();
			patient.LName=ScratchDb.UniqueName("Ehrspecpat");
			patient.FName=Regex.Replace(key,"[^A-Za-z0-9]","");
			patient.Birthdate=birthdate;
			patient.Gender=PatientGender.Female;
			patient.Language="eng";
			patient.PriProv=priProv;
			patient.PatStatus=PatientStatus.Patient;
			patient.Position=PatientPosition.Single;
			patient.State="ZZ";
			patient.Zip="00000";
			Patients.Insert(patient,false);
			_dictPatients[key]=patient;
			return patient;
		}

		///<summary>A completed office visit (the made-up code TESTV01); returns its ProcNum.</summary>
		private static long MakeVisit(Patient patient,DateTime dateProc,long provNum) {
			Procedure procedure=new Procedure();
			procedure.PatNum=patient.PatNum;
			procedure.ProvNum=provNum;
			procedure.CodeNum=_codeNumVisit;
			procedure.ProcDate=dateProc;
			procedure.DateEntryC=dateProc;
			procedure.ProcStatus=ProcStat.C;
			return OpenPmsDentalBusiness.Crud.ProcedureCrud.Insert(procedure);
		}

		private static void MakeProcNote(Patient patient,long procNum,string signature) {
			ProcNote procNote=new ProcNote();
			procNote.PatNum=patient.PatNum;
			procNote.ProcNum=procNum;
			procNote.EntryDateTime=DateMonday.AddHours(EVENT_HOUR);
			procNote.UserNum=1;
			procNote.Note="Synthetic progress note.";
			procNote.Signature=signature;
			OpenPmsDentalBusiness.Crud.ProcNoteCrud.Insert(procNote);
		}

		private static void MakeDisease(Patient patient,long diseaseDefNum) {
			Disease disease=new Disease();
			disease.PatNum=patient.PatNum;
			disease.DiseaseDefNum=diseaseDefNum;
			disease.DateStart=new DateTime(2013,1,7);
			disease.ProbStatus=ProblemStatus.Active;
			OpenPmsDentalBusiness.Crud.DiseaseCrud.Insert(disease);
		}

		private static void MakeMedicationPat(Patient patient,long medicationNum,long provNum,DateTime dateStart,string patNote,bool isCpoe) {
			MedicationPat medicationPat=new MedicationPat();
			medicationPat.PatNum=patient.PatNum;
			medicationPat.MedicationNum=medicationNum;
			medicationPat.ProvNum=provNum;
			medicationPat.DateStart=dateStart;
			medicationPat.PatNote=patNote;
			medicationPat.MedDescript="Synthetic medication";
			medicationPat.IsCpoe=isCpoe;
			OpenPmsDentalBusiness.Crud.MedicationPatCrud.Insert(medicationPat);
		}

		private static void MakeAllergy(Patient patient,long allergyDefNum) {
			Allergy allergy=new Allergy();
			allergy.PatNum=patient.PatNum;
			allergy.AllergyDefNum=allergyDefNum;
			allergy.StatusIsActive=true;
			allergy.Reaction="Synthetic reaction";
			OpenPmsDentalBusiness.Crud.AllergyCrud.Insert(allergy);
		}

		private static void MakeCdcrec(string code,string heirarchicalCode,string description) {
			Cdcrec cdcrec=new Cdcrec();
			cdcrec.CdcrecCode=code;
			cdcrec.HeirarchicalCode=heirarchicalCode;
			cdcrec.Description=description;
			OpenPmsDentalBusiness.Crud.CdcrecCrud.Insert(cdcrec);
		}

		private static void MakePatientRace(Patient patient,string cdcrecCode,bool isEthnicity) {
			PatientRace patientRace=new PatientRace();
			patientRace.PatNum=patient.PatNum;
			patientRace.CdcrecCode=cdcrecCode;
			patientRace.Description="Synthetic";
			patientRace.IsEthnicity=isEthnicity;
			OpenPmsDentalBusiness.Crud.PatientRaceCrud.Insert(patientRace);
		}

		private static void MakeRx(Patient patient,DateTime rxDate,RxSendStatus rxSendStatus,bool isControlled) {
			RxPat rxPat=new RxPat();
			rxPat.PatNum=patient.PatNum;
			rxPat.ProvNum=ProviderEp.ProvNum;
			rxPat.RxDate=rxDate;
			rxPat.Drug="Synthetic drug";
			rxPat.Sig="Synthetic sig";
			rxPat.SendStatus=rxSendStatus;
			rxPat.IsControlled=isControlled;
			OpenPmsDentalBusiness.Crud.RxPatCrud.Insert(rxPat);
		}

		///<summary>An event at EVENT_HOUR on the day.</summary>
		private static void MakeEvent(Patient patient,EhrMeasureEventType eventType,DateTime dateEvent,long fKey) {
			EhrMeasureEvent ehrMeasureEvent=new EhrMeasureEvent();
			ehrMeasureEvent.PatNum=patient.PatNum;
			ehrMeasureEvent.EventType=eventType;
			ehrMeasureEvent.DateTEvent=dateEvent.Date.AddHours(EVENT_HOUR);
			ehrMeasureEvent.FKey=fKey;
			OpenPmsDentalBusiness.Crud.EhrMeasureEventCrud.Insert(ehrMeasureEvent);
		}

		private static void MakeVitalsign(Patient patient,float height,float weight,int bpSystolic,int bpDiastolic) {
			Vitalsign vitalsign=new Vitalsign();
			vitalsign.PatNum=patient.PatNum;
			vitalsign.Height=height;
			vitalsign.Weight=weight;
			vitalsign.BpSystolic=bpSystolic;
			vitalsign.BpDiastolic=bpDiastolic;
			vitalsign.DateTaken=DateMonday;
			OpenPmsDentalBusiness.Crud.VitalsignCrud.Insert(vitalsign);
		}

		///<summary>A lab order by the EP, who is named by the NPI, observed on 3 March 2014; returns its EhrLabNum.</summary>
		private static long MakeEhrLab(Patient patient,bool isCpoe,string usiId,string usiCodeSystemName) {
			EhrLab ehrLab=new EhrLab();
			ehrLab.PatNum=patient.PatNum;
			ehrLab.OrderingProviderIdentifierTypeCode=HL70203.NPI;
			ehrLab.OrderingProviderID=NPI;
			ehrLab.OrderingProviderLName=ProviderEp.LName;
			ehrLab.OrderingProviderFName=ProviderEp.FName;
			ehrLab.ObservationDateTimeStart=DateMonday.ToString("yyyyMMdd",CultureInfo.InvariantCulture);
			ehrLab.UsiID=usiId;
			ehrLab.UsiCodeSystemName=usiCodeSystemName;
			ehrLab.UsiText="Synthetic test";
			ehrLab.IsCpoe=isCpoe;
			return OpenPmsDentalBusiness.Crud.EhrLabCrud.Insert(ehrLab);
		}

		private static void MakeLabResult(long ehrLabNum,HL70125 valueType,string codedElementId) {
			EhrLabResult ehrLabResult=new EhrLabResult();
			ehrLabResult.EhrLabNum=ehrLabNum;
			ehrLabResult.ValueType=valueType;
			if(valueType==HL70125.NM) {
				ehrLabResult.ObservationValueNumber1=5.5;
			}
			ehrLabResult.ObservationValueCodedElementID=codedElementId;
			if(valueType==HL70125.ST) {
				ehrLabResult.ObservationValueText="Synthetic free-text result";
			}
			OpenPmsDentalBusiness.Crud.EhrLabResultCrud.Insert(ehrLabResult);
		}

		private static void MakeLabImage(long ehrLabNum,long docNum) {
			EhrLabImage ehrLabImage=new EhrLabImage();
			ehrLabImage.EhrLabNum=ehrLabNum;
			ehrLabImage.DocNum=docNum;
			OpenPmsDentalBusiness.Crud.EhrLabImageCrud.Insert(ehrLabImage);
		}

		private static long MakeLabOrder(Patient patient) {
			MedicalOrder medicalOrder=new MedicalOrder();
			medicalOrder.PatNum=patient.PatNum;
			medicalOrder.MedOrderType=MedicalOrderType.Laboratory;
			medicalOrder.ProvNum=ProviderEp.ProvNum;
			medicalOrder.DateTimeOrder=DateMonday.AddHours(9);
			medicalOrder.Description="Synthetic lab order";
			return OpenPmsDentalBusiness.Crud.MedicalOrderCrud.Insert(medicalOrder);
		}

		///<summary>A transition of care with the made-up referral, the EP as the provider; returns its RefAttachNum.</summary>
		private static long MakeRefAttach(Patient patient,ReferralType referralType,DateTime refDate) {
			RefAttach refAttach=new RefAttach();
			refAttach.ReferralNum=_referralNum;
			refAttach.PatNum=patient.PatNum;
			refAttach.RefDate=refDate;
			refAttach.RefType=referralType;
			refAttach.IsTransitionOfCare=true;
			refAttach.ProvNum=ProviderEp.ProvNum;
			return OpenPmsDentalBusiness.Crud.RefAttachCrud.Insert(refAttach);
		}

		private static void RefreshPrefs() {
			if(!Prefs.DictIsNull()) {
				Prefs.ClearCache();
			}
			Cache.Refresh(InvalidType.Prefs);
		}
		#endregion Synthetic rows

		#region Server
		private static void DropStale() {
			Regex regex=new Regex("^"+PREFIX+@"(\d{12})_\d+_\d+_[a-z0-9_]+$");
			List<string> listNames=new List<string>();
			using(MySqlConnection connection=ScratchDb.OpenServer())
			using(MySqlCommand command=new MySqlCommand("SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME LIKE 'ehr4b\\_%'",connection))
			using(MySqlDataReader reader=command.ExecuteReader()) {
				while(reader.Read()) {
					listNames.Add(reader.GetString(0));
				}
			}
			foreach(string name in listNames) {
				Match match=regex.Match(name);
				if(!match.Success || !DateTime.TryParseExact(match.Groups[1].Value,"yyMMddHHmmss",CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeUniversal|DateTimeStyles.AdjustToUniversal,out DateTime dateStarted))
				{
					continue;
				}
				if(DateTime.UtcNow-dateStarted<_timeSpanStale) {
					continue;
				}
				try {
					Drop(name);
				}
				catch(Exception) {
					//Another run may be dropping it at the same time.
				}
			}
		}

		public static void Drop(string name) {
			if(!name.StartsWith(PREFIX)) {
				throw new ArgumentException("These tests only drop databases whose name starts with "+PREFIX+".");
			}
			using(MySqlConnection connection=ScratchDb.OpenServer()) {
				ScratchDb.Exec(connection,"DROP DATABASE IF EXISTS `"+name+"`");
			}
		}

		public static bool Exists(string name) {
			using(MySqlConnection connection=ScratchDb.OpenServer())
			using(MySqlCommand command=new MySqlCommand("SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME=@name",connection)) {
				command.Parameters.AddWithValue("@name",name);
				return Convert.ToInt64(command.ExecuteScalar(),CultureInfo.InvariantCulture)>0;
			}
		}
		#endregion Server
	}

	///<summary>Codes the engine's own source contains, read from EhrMeasures.cs at run time (the data rule of the plan's section 6): never written as
	///literals in the tests.</summary>
	public static class MeasureSpecCodes {
		public const string RepoPathEngine=@"OpenPmsDentalBusiness\Data Interface\EhrMeasures.cs";

		///<summary>The first smoking status code the engine's Smoking measure accepts (its smokeSnoMed=="..." comparisons).</summary>
		public static string GetSmokingStatusCode() {
			Match match=Regex.Match(TestPaths.ReadSource(RepoPathEngine),"smokeSnoMed==\"([0-9]+)\"");
			if(!match.Success) {
				throw new InvalidOperationException("No smoking status code in the engine's source.");
			}
			return match.Groups[1].Value;
		}

		///<summary>The first positive/negative lab result code of the engine's list (_snomedLabResult), which its Lab measure counts as a result.</summary>
		public static string GetLabResultCode() {
			Match match=Regex.Match(TestPaths.ReadSource(RepoPathEngine),"_snomedLabResult=@\"'([0-9]+)'");
			if(!match.Success) {
				throw new InvalidOperationException("No lab result code in the engine's source.");
			}
			return match.Groups[1].Value;
		}
	}
}
