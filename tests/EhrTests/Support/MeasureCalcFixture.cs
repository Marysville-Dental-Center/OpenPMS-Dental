// The scratch database and the fixture builder of the Measure Calc tests (package EHR-4a of docs/specs/ehr-rebuild-plan.md, section 7), on the
// local MariaDB (127.0.0.1:3306, root, blank password). Built as Support/ScratchDb.cs builds the EHR-1 one, from the committed
// db/init/new-database.sql and the fork migrations, but under its own names: every name starts with ehr4a_ plus this run's id, so these tests
// never touch another scratch database, and runs from other worktrees never collide. Built by the first Measure Calc test, dropped by the
// class's cleanup; leftovers of earlier runs older than 12 hours are dropped first, and only names starting with ehr4a_. Synthetic rows only:
// made-up names, and one made-up procedure code (TESTEX1). No code of any licensed system is written here; EHR_TEST_DB=skip skips (inconclusive),
// EHR_KEEP_DB=1 keeps the database for a look afterwards.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;
using OpenPmsDentalBusiness.WebServices;

namespace EhrTests {
	///<summary>The ehr4a_ scratch database and the synthetic practice the Measure Calc tests calculate on.</summary>
	public static class MeasureCalcFixture {
		public const string PREFIX="ehr4a_";
		public const string PASSWORD="ehr4a-test-password";
		private static readonly TimeSpan _timeSpanStale=TimeSpan.FromHours(12);
		private static readonly string _runId=DateTime.UtcNow.ToString("yyMMddHHmmss",CultureInfo.InvariantCulture)+"_"
			+System.Diagnostics.Process.GetCurrentProcess().Id+"_"+new Random().Next(1000,10000);
		public static readonly string DbName=Name("main");
		///<summary>A second database, for the test that the work-table helper never drops another database's tables.</summary>
		public static readonly string DbNameOther=Name("other");
		private static readonly object _lockBuild=new object();
		private static bool _isBuildTried;
		private static bool _isBuilt;
		private static string _skipReason;
		private static string _buildError;
		private static CultureInfo _cultureBefore;
		///<summary>The reporting period of the fixture: one calendar year.</summary>
		public static readonly DateTime DateStart=new DateTime(2016,1,1);
		public static readonly DateTime DateEnd=new DateTime(2016,12,31);
		///<summary>The fixture's rows (MakeRows).</summary>
		public static Provider ProviderKeyed;
		///<summary>A second provider record with the same last and first name as ProviderKeyed: the engine counts the two together.</summary>
		public static Provider ProviderKeyedSameNames;
		///<summary>A provider with no ehrprovkey row.</summary>
		public static Provider ProviderNoRow;
		///<summary>The patients seen by ProviderKeyed (the first three) and ProviderKeyedSameNames (the fourth) in the period.</summary>
		public static List<Patient> ListPatientsKeyed=new List<Patient>();
		///<summary>Seen by ProviderNoRow in the period.</summary>
		public static Patient PatientNoRow;
		///<summary>A user with a known password, for the Middle Tier mock (it checks the credentials of every call).</summary>
		public static Userod UserodMiddleTier;

		public static string Name(string purpose) {
			string name=PREFIX+_runId+"_"+purpose;
			if(name.Length>64 || !Regex.IsMatch(name,"^[a-z0-9_]+$")) {
				throw new ArgumentException("Not a usable scratch database name: "+name);
			}
			return name;
		}

		///<summary>Call first in each test: builds the database and the fixture on the first call, skips when there is no server, otherwise connects
		///to it directly (no Middle Tier) as user 1 in the en-US culture.</summary>
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

		///<summary>Call from [TestCleanup]: the culture back, no Middle Tier, and the tests of the other classes back on the EHR-1 database when it
		///exists.</summary>
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
				Drop(DbNameOther);
			}
			catch(Exception) {
				//The stale clean-up of a later run drops them.
			}
		}

		public static void UseDirectConnection() {
			ScratchDb.UseDirectConnection();
			ScratchDb.Connect(DbName);
		}

		///<summary>Every Middle Tier call of this thread goes through the mock server, connected to this fixture's database, as the user with the
		///known password.</summary>
		public static void UseMockMiddleTier() {
			ScratchDb.LogOn(UserodMiddleTier,PASSWORD);
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientMT;
			OpenDentalServerProxy.MockOpenDentalServerCur=new OpenDentalServerMockIIS(server:ScratchDb.Server,db:DbName,user:ScratchDb.User,
				password:ScratchDb.Password,userLow:ScratchDb.User,passLow:ScratchDb.Password,dbType:DatabaseType.MySql,
				sslCa:DevDbTls.GetSslCa(ScratchDb.Server));
		}

		private static void RefreshCaches() {
			if(!Prefs.DictIsNull()) {
				Prefs.ClearCache();
			}
			Cache.Refresh(InvalidType.Prefs,InvalidType.Providers,InvalidType.Security);
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
					ScratchDb.Exec(connection,"CREATE DATABASE `"+DbNameOther+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
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
				_buildError="Could not build the ehr4a_ scratch database: "+ex.GetBaseException().Message;
			}
			finally {
				Thread.CurrentThread.CurrentCulture=cultureBefore;
			}
		}

		#region Synthetic rows
		///<summary>The fixture: a provider with an ehrprovkey row and a second provider record with the same names, a provider with no row, a
		///made-up procedure code, four patients seen by the first two in the period and one by the third, problems for two of them, and medication
		///orders: one by the keyed provider, and one with no provider (ProvNum 0, as the medical window enters them) for the third provider's
		///patient.</summary>
		private static void MakeRows() {
			string lName=ScratchDb.UniqueName("Ehrmeasurelast");
			string fName=ScratchDb.UniqueName("Ehrmeasurefirst");
			ProviderKeyed=ScratchDb.MakeProvider(lName,fName);
			ProviderKeyedSameNames=ScratchDb.MakeProvider(lName,fName);
			ProviderNoRow=ScratchDb.MakeProvider(ScratchDb.UniqueName("Ehrnorowlast"),ScratchDb.UniqueName("Ehrnorowfirst"));
			EhrProvKey ehrProvKey=new EhrProvKey();
			ehrProvKey.LName=lName;
			ehrProvKey.FName=fName;
			ehrProvKey.YearValue=16;
			ehrProvKey.ProvKey="";
			EhrProvKeys.Insert(ehrProvKey);
			UserodMiddleTier=ScratchDb.MakeUser(PASSWORD);
			ProcedureCode procedureCode=new ProcedureCode();
			procedureCode.ProcCode="TESTEX1";
			procedureCode.Descript="Synthetic visit";
			procedureCode.AbbrDesc="Visit";
			long codeNum=OpenPmsDentalBusiness.Crud.ProcedureCodeCrud.Insert(procedureCode);
			DiseaseDef diseaseDef=new DiseaseDef();
			diseaseDef.DiseaseName="Synthetic problem";
			diseaseDef.SnomedCode="TEST0001";//A made-up code: the engine counts a problem that has a code.
			long diseaseDefNum=OpenPmsDentalBusiness.Crud.DiseaseDefCrud.Insert(diseaseDef);
			ListPatientsKeyed.Clear();
			for(int i=0;i<4;i++) {
				Patient patient=MakePatient(ProviderKeyed.ProvNum);
				ListPatientsKeyed.Add(patient);
				long provNum=i<3 ? ProviderKeyed.ProvNum : ProviderKeyedSameNames.ProvNum;
				MakeCompletedProc(patient.PatNum,provNum,codeNum,new DateTime(2016,3,10+i));
			}
			MakeDisease(ListPatientsKeyed[0].PatNum,diseaseDefNum);
			MakeDisease(ListPatientsKeyed[1].PatNum,diseaseDefNum);
			MakeMedicationOrder(ListPatientsKeyed[0].PatNum,ProviderKeyed.ProvNum,new DateTime(2016,3,10));
			PatientNoRow=MakePatient(ProviderNoRow.ProvNum);
			MakeCompletedProc(PatientNoRow.PatNum,ProviderNoRow.ProvNum,codeNum,new DateTime(2016,4,1));
			MakeMedicationOrder(PatientNoRow.PatNum,0,new DateTime(2016,4,1));
		}

		private static Patient MakePatient(long provNum) {
			Patient patient=new Patient();
			patient.LName=ScratchDb.UniqueName("Ehrpatlast");
			patient.FName=ScratchDb.UniqueName("Ehrpatfirst");
			patient.Birthdate=new DateTime(1980,6,15);
			patient.Gender=PatientGender.Female;
			patient.PriProv=provNum;
			patient.PatStatus=PatientStatus.Patient;
			patient.Position=PatientPosition.Single;
			patient.State="ZZ";
			patient.Zip="00000";
			Patients.Insert(patient,false);
			return patient;
		}

		private static void MakeCompletedProc(long patNum,long provNum,long codeNum,DateTime dateProc) {
			Procedure procedure=new Procedure();
			procedure.PatNum=patNum;
			procedure.ProvNum=provNum;
			procedure.CodeNum=codeNum;
			procedure.ProcDate=dateProc;
			procedure.DateEntryC=dateProc;
			procedure.ProcStatus=ProcStat.C;
			OpenPmsDentalBusiness.Crud.ProcedureCrud.Insert(procedure);
		}

		private static void MakeDisease(long patNum,long diseaseDefNum) {
			Disease disease=new Disease();
			disease.PatNum=patNum;
			disease.DiseaseDefNum=diseaseDefNum;
			disease.DateStart=new DateTime(2016,1,5);
			OpenPmsDentalBusiness.Crud.DiseaseCrud.Insert(disease);
		}

		///<summary>A medication order as an e-prescription leaves it: MedicationNum 0, the provider, the start date.</summary>
		private static void MakeMedicationOrder(long patNum,long provNum,DateTime dateStart) {
			MedicationPat medicationPat=new MedicationPat();
			medicationPat.PatNum=patNum;
			medicationPat.ProvNum=provNum;
			medicationPat.MedicationNum=0;
			medicationPat.MedDescript="Synthetic medication";
			medicationPat.DateStart=dateStart;
			OpenPmsDentalBusiness.Crud.MedicationPatCrud.Insert(medicationPat);
		}
		#endregion Synthetic rows

		#region Server
		///<summary>The names of the engines' work tables in the database (tempehrmeasure..., tempehrquality...).</summary>
		public static List<string> GetWorkTables(string database) {
			List<string> listNames=new List<string>();
			using(MySqlConnection connection=ScratchDb.OpenServer())
			using(MySqlCommand command=new MySqlCommand("SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA=@db "
				+"AND (TABLE_NAME LIKE 'tempehrmeasure%' OR TABLE_NAME LIKE 'tempehrquality%') ORDER BY TABLE_NAME",connection))
			{
				command.Parameters.AddWithValue("@db",database);
				using(MySqlDataReader reader=command.ExecuteReader()) {
					while(reader.Read()) {
						listNames.Add(reader.GetString(0));
					}
				}
			}
			return listNames;
		}

		///<summary>Creates a table in one of this fixture's databases (the names are this test's own).</summary>
		public static void CreateTable(string database,string table) {
			if(!database.StartsWith(PREFIX) || !Regex.IsMatch(table,"^[a-z0-9_]+$")) {
				throw new ArgumentException("Only simple tables in this fixture's databases.");
			}
			using(MySqlConnection connection=ScratchDb.OpenServer(database)) {
				ScratchDb.Exec(connection,"CREATE TABLE `"+table+"` (PatNum bigint NOT NULL PRIMARY KEY) DEFAULT CHARSET=utf8");
			}
		}

		public static void DropTable(string database,string table) {
			if(!database.StartsWith(PREFIX) || !Regex.IsMatch(table,"^[a-z0-9_]+$")) {
				throw new ArgumentException("Only simple tables in this fixture's databases.");
			}
			using(MySqlConnection connection=ScratchDb.OpenServer(database)) {
				ScratchDb.Exec(connection,"DROP TABLE IF EXISTS `"+table+"`");
			}
		}

		///<summary>The Numerator and Denominator columns of every ehrmeasure row, "num/den" (E-19: nothing writes them).</summary>
		public static List<string> GetStoredNumbers() {
			List<string> listNumbers=new List<string>();
			using(MySqlConnection connection=ScratchDb.OpenServer(DbName))
			using(MySqlCommand command=new MySqlCommand("SELECT Numerator,Denominator FROM ehrmeasure ORDER BY EhrMeasureNum",connection))
			using(MySqlDataReader reader=command.ExecuteReader()) {
				while(reader.Read()) {
					listNumbers.Add(Convert.ToString(reader.GetValue(0),CultureInfo.InvariantCulture)+"/"
						+Convert.ToString(reader.GetValue(1),CultureInfo.InvariantCulture));
				}
			}
			return listNumbers;
		}

		private static void DropStale() {
			Regex regex=new Regex("^"+PREFIX+@"(\d{12})_\d+_\d+_[a-z0-9_]+$");
			List<string> listNames=new List<string>();
			using(MySqlConnection connection=ScratchDb.OpenServer())
			using(MySqlCommand command=new MySqlCommand("SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME LIKE 'ehr4a\\_%'",connection))
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
}
