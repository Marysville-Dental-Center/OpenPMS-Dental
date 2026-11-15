// The scratch databases of the EHR-2 tests (docs/specs/ehr-rebuild-plan.md, EHR-2), on the local MariaDB (127.0.0.1:3306, root, blank
// password): every name starts with ehr2_ plus this run's id, so a test can never drop anything else and runs from other worktrees never
// collide. The main one is built as the EHR-1 helper builds its own (the committed db/init/new-database.sql loaded statement by statement, then
// every fork migration of this build, which creates the ehrcode table), by the first test that needs it, and dropped after the last test
// (ValueSetDbTests' ClassCleanup); a test may make an extra empty one and drops it itself. Leftovers older than 12 hours are dropped first.
// Synthetic rows only. Without the server the tests are inconclusive (EHR_TEST_DB=skip skips them; EHR_KEEP_DB=1 keeps the database).
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
	public static class ValueSetDb {
		public const string PREFIX="ehr2_";
		private static readonly TimeSpan _timeSpanStale=TimeSpan.FromHours(12);
		private static readonly string _runId=DateTime.UtcNow.ToString("yyMMddHHmmss",CultureInfo.InvariantCulture)+"_"
			+System.Diagnostics.Process.GetCurrentProcess().Id+"_"+new Random().Next(1000,10000);
		///<summary>This run's main database.</summary>
		public static readonly string DbName=Name("main");
		private static readonly object _lockBuild=new object();
		private static bool _isBuildTried;
		private static bool _isBuilt;
		///<summary>Why the tests are inconclusive: no server, or EHR_TEST_DB=skip.</summary>
		private static string _skipReason;
		///<summary>Why the database could not be built: the tests fail with it.</summary>
		private static string _buildError;
		private static CultureInfo _cultureBefore;
		///<summary>The changes the fork migrations made while the database was built (the step's CREATE TABLE among them).</summary>
		public static List<string> ListChangesBuild=new List<string>();

		///<summary>This run's database name for the given purpose, like ehr2_261114101500_1234_5678_main. At most 64 characters.</summary>
		public static string Name(string purpose) {
			string name=PREFIX+_runId+"_"+purpose;
			if(name.Length>64 || !Regex.IsMatch(name,"^[a-z0-9_]+$")) {
				throw new ArgumentException("Not a usable scratch database name: "+name);
			}
			return name;
		}

		///<summary>Call from [TestInitialize]: builds the database on the first call, skips the test when there is none, otherwise connects to it
		///directly (no Middle Tier) as user 1 (Admin) in the en-US culture, with an empty value-set list.</summary>
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
			ScratchDb.UseDirectConnection();
			ScratchDb.Connect(DbName);
			RefreshCaches();
			EhrCodes.ClearList();
		}

		///<summary>Call from [TestCleanup]: back to a direct connection, the value-set list dropped (so no other test sees these rows), and back to the
		///EHR-1 scratch database with its caches when it exists, as the EHR-3 tests do.</summary>
		public static void End() {
			if(_cultureBefore!=null) {
				Thread.CurrentThread.CurrentCulture=_cultureBefore;
				_cultureBefore=null;
			}
			ScratchDb.UseDirectConnection();
			EhrCodes.ClearList();
			if(ScratchDb.SkipReason==null && Exists(ScratchDb.DbName)) {
				ScratchDb.Connect(ScratchDb.DbName);
				RefreshCaches();
			}
		}

		public static void RefreshCaches() {
			if(!Prefs.DictIsNull()) {
				Prefs.ClearCache();
			}
			Cache.Refresh(InvalidType.Prefs,InvalidType.Providers,InvalidType.Security,InvalidType.Diseases);
			Userods.RefreshCache();
			Security.CurUser=Userods.GetUser(1);
			Security.PasswordTyped="";
		}

		///<summary>Drops this run's main database (ValueSetDbTests' ClassCleanup).</summary>
		public static void Cleanup() {
			ScratchDb.UseDirectConnection();
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

		private static void Build() {
			_skipReason=ScratchDb.GetSkipReason();
			if(_skipReason!=null) {
				return;
			}
			CultureInfo cultureBefore=Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
			try {
				DropStale();
				_isBuilt=true;//Set before the database exists, so Cleanup drops a half-built one too (Drop uses IF EXISTS).
				CreateFromInitFile(DbName);
				ScratchDb.UseDirectConnection();
				ScratchDb.Connect(DbName);
				RefreshCaches();
				ListChangesBuild=ForkMigrationRunner.Run().ListChanges;
				RefreshCaches();
			}
			catch(Exception ex) {
				_buildError="Could not build the ehr2_ scratch database: "+ex.GetBaseException().Message;
			}
			finally {
				Thread.CurrentThread.CurrentCulture=cultureBefore;
			}
		}

		///<summary>An empty utf8mb3 database with the committed new-database file loaded into it, statement by statement.</summary>
		private static void CreateFromInitFile(string name) {
			Drop(name);
			using(MySqlConnection connection=ScratchDb.OpenServer()) {
				ScratchDb.Exec(connection,"CREATE DATABASE `"+name+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
			}
			string text=File.ReadAllText(TestPaths.Combine(ScratchDb.RepoPathInitSql),new UTF8Encoding(false)).Replace("\r\n","\n");
			using(MySqlConnection connection=ScratchDb.OpenServer(name)) {
				//Every statement of the generated file ends with a semicolon at the end of a line, and no value holds a raw line break.
				foreach(string statement in text.Split(new[] { ";\n" },StringSplitOptions.None)) {
					if(statement.Trim()=="") {
						continue;
					}
					ScratchDb.Exec(connection,statement);
				}
			}
		}

		///<summary>An extra empty database of this run (dropped by the test that made it, with Drop).</summary>
		public static string CreateEmpty(string purpose) {
			string name=Name(purpose);
			Drop(name);
			using(MySqlConnection connection=ScratchDb.OpenServer()) {
				ScratchDb.Exec(connection,"CREATE DATABASE `"+name+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
			}
			return name;
		}

		public static bool Exists(string name) {
			using(MySqlConnection connection=ScratchDb.OpenServer())
			using(MySqlCommand command=new MySqlCommand("SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME=@name",connection)) {
				command.Parameters.AddWithValue("@name",name);
				return Convert.ToInt64(command.ExecuteScalar(),CultureInfo.InvariantCulture)>0;
			}
		}

		///<summary>Drops this fixture's leftovers of earlier runs once they are older than 12 hours. Only names that start with ehr2_.</summary>
		private static void DropStale() {
			Regex regex=new Regex("^"+PREFIX+@"(\d{12})_\d+_\d+_[a-z0-9_]+$");
			List<string> listNames=new List<string>();
			using(MySqlConnection connection=ScratchDb.OpenServer())
			using(MySqlCommand command=new MySqlCommand("SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME LIKE 'ehr2\\_%'",connection))
			using(MySqlDataReader reader=command.ExecuteReader())
			{
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

		///<summary>Every Middle Tier call of this thread goes through the mock server, connected to this run's database (as ScratchDb does for its
		///own).</summary>
		public static void UseMockMiddleTier() {
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientMT;
			OpenDentalServerProxy.MockOpenDentalServerCur=new OpenDentalServerMockIIS(server:ScratchDb.Server,db:DbName,user:ScratchDb.User,
				password:ScratchDb.Password,userLow:ScratchDb.User,passLow:ScratchDb.Password,dbType:DatabaseType.MySql,sslCa:DevDbTls.GetSslCa(ScratchDb.Server));
		}

		///<summary>Rows of a code-system table (synthetic, made-up TEST codes), so the code list counts its codes as in the database.</summary>
		public static void FillCodeTable(string table,string columnCode,int count,string prefix) {
			StringBuilder stringBuilder=new StringBuilder();
			for(int i=0;i<count;i++) {
				if(stringBuilder.Length==0) {
					stringBuilder.Append("INSERT INTO "+table+" ("+columnCode+") VALUES ");
				}
				else {
					stringBuilder.Append(",");
				}
				stringBuilder.Append("('"+prefix+i.ToString("D6",CultureInfo.InvariantCulture)+"')");
				if(stringBuilder.Length>200000) {
					DataCore.NonQ(stringBuilder.ToString());
					stringBuilder.Clear();
				}
			}
			if(stringBuilder.Length>0) {
				DataCore.NonQ(stringBuilder.ToString());
			}
		}
	}
}
