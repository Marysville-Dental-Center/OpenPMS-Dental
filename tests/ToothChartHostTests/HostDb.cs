// The scratch database of the chart's host tests (package TC-8): a new database loaded from db/init/new-database.sql, as an office gets it, on the
// local MariaDB only (127.0.0.1:3306, root, blank password), then filled with synthetic patients by HostFixtures. Nothing here reads or copies an
// office database. The database is made the first time a test asks for it and dropped when the test run ends. Its name starts with "tc8_" (or the
// prefix OPMS_CHART_HOST_TEST_DB_PREFIX names, such as "tc8b_"), and a
// run drops only names of that pattern that are more than 12 hours old (left by a crash, or kept with OPMS_CHART_HOST_TEST_KEEP_DB=1).
// OPMS_CHART_HOST_TEST_DB=skip skips the tests (inconclusive), as does a server that does not answer.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDentalBusiness;

namespace ToothChartHostTests {
	[TestClass]
	public class HostDb {
		public const string Server="127.0.0.1";
		public const string User="root";
		public const string Password="";
		///<summary>"tc8_", or the prefix a run names in OPMS_CHART_HOST_TEST_DB_PREFIX ("tc8", lowercase letters or digits, then "_"; for example
		///"tc8b_"), so a package's runs make, and drop when stale, only scratch databases of their own name.</summary>
		private static readonly string PREFIX=GetPrefix();
		private static readonly TimeSpan _timeSpanStale=TimeSpan.FromHours(12);
		///<summary>Unique to this run: the start time (UTC), the process id and a random number, because several worktrees share the one server.</summary>
		public static readonly string DbName=PREFIX+DateTime.UtcNow.ToString("yyMMddHHmmss")+"_"+System.Diagnostics.Process.GetCurrentProcess().Id
			+"_"+new Random().Next(1000,10000);
		private static bool _isTried;
		private static bool _isCreated;
		private static string _skipReason;
		private static readonly object _lock=new object();

		///<summary>As OpenPmsDental.exe does first in Main (ProgramEntry, owner question O-34): every WPF window of the process, the live view included,
		///is drawn in software. CodeBase's logger is told it runs in a unit test, so the chart's fallback lines write no log file.</summary>
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext testContext) {
			OpenPmsDentalTests.TestCodePages.Register();//the code pages first, as the program registers them (tests\Shared\TestCodePages.cs)
			//The program's default font before any window exists, as every exe's Main sets it (S3b-4): the Simple 2D chart and the program's forms
			//draw with the control's Font, which is .NET's Segoe UI 9 otherwise (S3b-5W, docs/port-notes/S3b-5W.md).
			System.Windows.Forms.Application.SetDefaultFont(new System.Drawing.Font("Microsoft Sans Serif",8.25f));
			System.Windows.Media.RenderOptions.ProcessRenderMode=System.Windows.Interop.RenderMode.SoftwareOnly;
			ODBuild.IsUnitTest=true;
		}

		///<summary>Creates the database and the fixtures the first time and connects the business layer to it. Marks the calling test inconclusive
		///when there is no server (or OPMS_CHART_HOST_TEST_DB=skip); a server that answers but fails later fails the test.</summary>
		public static void EnsureOrSkip() {
			lock(_lock) {
				if(!_isTried) {
					_isTried=true;
					_skipReason=Create();
					if(_skipReason==null) {
						CultureInfo cultureInfo=Thread.CurrentThread.CurrentCulture;
						CultureInfo cultureInfoUI=Thread.CurrentThread.CurrentUICulture;
						try {
							SetCulture();
							HostFixtures.Build();
						}
						finally {
							Thread.CurrentThread.CurrentCulture=cultureInfo;
							Thread.CurrentThread.CurrentUICulture=cultureInfoUI;
						}
					}
				}
			}
			if(_skipReason!=null) {
				Assert.Inconclusive(_skipReason);
			}
		}

		///<summary>The pictures are compared in en-US: the perio printout writes the exam date with the workstation's short date format.</summary>
		public static void SetCulture() {
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
			Thread.CurrentThread.CurrentUICulture=new CultureInfo("en-US");
		}

		///<summary>Null when the database is ready; otherwise why the tests are skipped.</summary>
		private static string Create() {
			if(Environment.GetEnvironmentVariable("OPMS_CHART_HOST_TEST_DB")=="skip") {
				return "OPMS_CHART_HOST_TEST_DB=skip";
			}
			try {
				using(MySqlConnection connection=OpenServer()) {
					string sqlMode=Scalar(connection,"SELECT @@GLOBAL.sql_mode");
					if(sqlMode!="" && sqlMode.ToUpperInvariant()!="NO_AUTO_CREATE_USER") {
						return "Global sql_mode is '"+sqlMode+"'; the program's queries need a blank sql_mode, as OpenPmsDental.exe sets at startup.";
					}
					Exec(connection,"CREATE DATABASE `"+DbName+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
				}
			}
			catch(Exception ex) {
				return "No local MariaDB at "+Server+":3306 as "+User+" with a blank password: "+ex.GetBaseException().Message;
			}
			_isCreated=true;
			//The server answers, so from here on a failure is a real failure, not a reason to skip.
			DropStaleScratchDatabases();
			LoadNewDatabase();
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientDirect;
			new DataConnection().SetDb(Server,DbName,User,Password,"","",DatabaseType.MySql,true,DevDbTls.GetSslCa(Server));
			Cache.Refresh(InvalidType.AllLocal);
			return null;
		}

		///<summary>Runs db/init/new-database.sql statement by statement (every statement ends with a semicolon at the end of a line), as
		///tests/ToothChartImageTests does.</summary>
		private static void LoadNewDatabase() {
			string text=File.ReadAllText(Path.Combine(HostPaths.RepoRoot,"db","init","new-database.sql"),new UTF8Encoding(false)).Replace("\r\n","\n");
			using(MySqlConnection connection=OpenServer(DbName)) {
				foreach(string statement in text.Split(new[] { ";\n" },StringSplitOptions.None)) {
					if(statement.Trim()=="") {
						continue;
					}
					Exec(connection,statement);
				}
			}
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup() {
			if(!_isCreated || Environment.GetEnvironmentVariable("OPMS_CHART_HOST_TEST_KEEP_DB")=="1") {
				return;
			}
			try {
				Drop(DbName);
			}
			catch(Exception) {
				//Leaving the scratch database behind is harmless; a later run drops it once it is stale.
			}
		}

		///<summary>Drops scratch databases that earlier runs left behind, once they are older than _timeSpanStale. Never drops the database of a run
		///that may still be going, and never a name without this naming scheme.</summary>
		private static void DropStaleScratchDatabases() {
			Regex regex=new Regex("^"+PREFIX+@"(\d{12})_\d+_\d+$");
			List<string> listNames=new List<string>();
			using(MySqlConnection connection=OpenServer())
			using(MySqlCommand command=new MySqlCommand("SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME LIKE '"
				+PREFIX.Replace("_","\\_")+"%'",connection))
			using(MySqlDataReader reader=command.ExecuteReader()) {
				while(reader.Read()) {
					listNames.Add(reader.GetString(0));
				}
			}
			foreach(string name in listNames.Where(x => x!=DbName)) {
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

		private static string GetPrefix() {
			string prefix=Environment.GetEnvironmentVariable("OPMS_CHART_HOST_TEST_DB_PREFIX");
			if(!string.IsNullOrEmpty(prefix) && Regex.IsMatch(prefix,"^tc8[a-z0-9]*_$")) {
				return prefix;
			}
			return "tc8_";
		}

		private static void Drop(string name) {
			if(!name.StartsWith(PREFIX)) {
				throw new ArgumentException("Tests only drop databases whose name starts with "+PREFIX);
			}
			using(MySqlConnection connection=OpenServer()) {
				Exec(connection,"DROP DATABASE IF EXISTS `"+name+"`");
			}
		}

		private static MySqlConnection OpenServer(string database=null) {
			string connectionString="Server="+Server+";Port=3306;User ID="+User+";Password="+Password+";Connection Timeout=15";
			if(database!=null) {
				connectionString+=";Database="+database;
			}
			MySqlConnection connection=new MySqlConnection(DevDbTls.Apply(connectionString));
			connection.Open();
			return connection;
		}

		private static void Exec(MySqlConnection connection,string command) {
			using(MySqlCommand mySqlCommand=new MySqlCommand(command,connection)) {
				mySqlCommand.CommandTimeout=600;
				mySqlCommand.ExecuteNonQuery();
			}
		}

		private static string Scalar(MySqlConnection connection,string command) {
			using(MySqlCommand mySqlCommand=new MySqlCommand(command,connection)) {
				return mySqlCommand.ExecuteScalar()?.ToString()??"";
			}
		}
	}

	///<summary>Paths of the repository and of this project.</summary>
	public static class HostPaths {
		///<summary>The repository root: the first folder above the test assembly that holds OpenDental.sln.</summary>
		public static string RepoRoot {
			get {
				DirectoryInfo directoryInfo=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
				while(directoryInfo!=null) {
					if(File.Exists(Path.Combine(directoryInfo.FullName,"OpenDental.sln"))) {
						return directoryInfo.FullName;
					}
					directoryInfo=directoryInfo.Parent;
				}
				throw new DirectoryNotFoundException("No OpenDental.sln above "+AppDomain.CurrentDomain.BaseDirectory);
			}
		}

		///<summary>tests\ToothChartHostTests\Baselines: the host pictures TC-8 recorded on .NET Framework 4.8 x86 (they stay there).</summary>
		public static string BaselinesNet48 {
			get {
				return Path.Combine(RepoRoot,"tests","ToothChartHostTests","Baselines");
			}
		}

		///<summary>The host pictures of this process's runtime and bitness: on .NET x64, tests\ToothChartHostTests\Baselines\net10-x64 (S3b-5W,
		///docs/port-notes/S3b-5W.md: every host picture, recorded on .NET 10 x64, because .NET draws WPF with another render DLL); otherwise the
		///.NET Framework 4.8 x86 pictures of TC-8 (BaselinesNet48). A recording writes to the same folder.</summary>
		public static string Baselines {
			get {
				bool isNetFramework=System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework",StringComparison.Ordinal);
				if(!isNetFramework && Environment.Is64BitProcess) {
					return Path.Combine(BaselinesNet48,"net10-x64");
				}
				return BaselinesNet48;
			}
		}
	}
}
