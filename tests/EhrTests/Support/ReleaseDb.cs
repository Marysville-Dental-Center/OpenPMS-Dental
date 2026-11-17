// The scratch database of the EHR-6a tests (docs/specs/ehr-2027-measures-plan.md, EHR-6a), on the local MariaDB (127.0.0.1:3306, root, blank
// password): every name starts with ehr6a_ plus this run's id, so a test can never drop anything else and runs from other worktrees never
// collide. Built as the EHR-2 helper builds its own (Support/ValueSetDb.cs: the committed db/init/new-database.sql loaded statement by
// statement, then every fork migration of this build), by the first test that needs it, and dropped after the last test (DirectCodeDbTests'
// ClassCleanup). Leftovers older than 12 hours are dropped first. Synthetic rows only. Without the server the tests are inconclusive
// (EHR_TEST_DB=skip skips them; EHR_KEEP_DB=1 keeps the database).
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
	public static class ReleaseDb {
		public const string PREFIX="ehr6a_";
		private static readonly TimeSpan _timeSpanStale=TimeSpan.FromHours(12);
		private static readonly string _runId=DateTime.UtcNow.ToString("yyMMddHHmmss",CultureInfo.InvariantCulture)+"_"
			+System.Diagnostics.Process.GetCurrentProcess().Id+"_"+new Random().Next(1000,10000);
		///<summary>This run's database, like ehr6a_261114101500_1234_5678_main.</summary>
		public static readonly string DbName=PREFIX+_runId+"_main";
		private static readonly object _lockBuild=new object();
		private static bool _isBuildTried;
		private static bool _isBuilt;
		private static string _skipReason;
		private static string _buildError;
		private static CultureInfo _cultureBefore;

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
			ValueSetDb.RefreshCaches();
			EhrCodes.ClearList();
		}

		///<summary>Call from [TestCleanup]: back to a direct connection, the value-set list dropped, and back to the EHR-1 scratch database with
		///its caches when it exists, as the EHR-2 tests do.</summary>
		public static void End() {
			if(_cultureBefore!=null) {
				Thread.CurrentThread.CurrentCulture=_cultureBefore;
				_cultureBefore=null;
			}
			ScratchDb.UseDirectConnection();
			EhrCodes.ClearList();
			//A user a test logged on here is no user of another scratch database: the next fixture that refreshes its caches would refuse it.
			Security.CurUser=null;
			Security.PasswordTyped="";
			if(ScratchDb.SkipReason==null && ValueSetDb.Exists(ScratchDb.DbName)) {
				ScratchDb.Connect(ScratchDb.DbName);
				ValueSetDb.RefreshCaches();
			}
		}

		///<summary>Drops this run's database (DirectCodeDbTests' ClassCleanup).</summary>
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
				Drop(DbName);
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
				ScratchDb.UseDirectConnection();
				ScratchDb.Connect(DbName);
				ValueSetDb.RefreshCaches();
				ForkMigrationRunner.Run();
				ValueSetDb.RefreshCaches();
			}
			catch(Exception ex) {
				_buildError="Could not build the ehr6a_ scratch database: "+ex.GetBaseException().Message;
			}
			finally {
				Thread.CurrentThread.CurrentCulture=cultureBefore;
			}
		}

		///<summary>Drops this fixture's leftovers of earlier runs once they are older than 12 hours. Only names that start with ehr6a_.</summary>
		private static void DropStale() {
			Regex regex=new Regex("^"+PREFIX+@"(\d{12})_\d+_\d+_[a-z0-9_]+$");
			List<string> listNames=new List<string>();
			using(MySqlConnection connection=ScratchDb.OpenServer())
			using(MySqlCommand command=new MySqlCommand("SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME LIKE 'ehr6a\\_%'",connection))
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

		///<summary>Every Middle Tier call of this thread goes through the mock server, connected to this run's database.</summary>
		public static void UseMockMiddleTier() {
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientMT;
			OpenDentalServerProxy.MockOpenDentalServerCur=new OpenDentalServerMockIIS(server:ScratchDb.Server,db:DbName,user:ScratchDb.User,
				password:ScratchDb.Password,userLow:ScratchDb.User,passLow:ScratchDb.Password,dbType:DatabaseType.MySql,sslCa:DevDbTls.GetSslCa(ScratchDb.Server));
		}
	}
}
