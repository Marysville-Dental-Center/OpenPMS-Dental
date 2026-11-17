// Scratch database for the translation tests of package S3a-M1, on the local MariaDB only (127.0.0.1:3306, root, blank password). It holds the
// language, languageforeign and preference tables of the Open Dental 26.1.70 schema (the reference embedded in OpenPmsDentalBusiness,
// SchemaVerifier.GetReference) and synthetic rows only. Every name starts with s3am1_. The database tests are skipped (inconclusive) when no
// server answers, or when S3AM1_TEST_DB=skip.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using CodeBase;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;

namespace S3aM1Tests {
	///<summary>Creates, connects to and drops this run's scratch database. Every name starts with "s3am1_" so the tests can never drop anything
	///else.</summary>
	[TestClass]
	public class S3aM1Db {
		public const string Server="127.0.0.1";
		public const string User="root";
		public const string Password="";
		public const string PREFIX="s3am1_";
		///<summary>Scratch databases left behind by a run that started this long ago are dropped by later runs. No run takes nearly this long.</summary>
		private static readonly TimeSpan _timeSpanStale=TimeSpan.FromHours(12);
		///<summary>Unique to this test run: the start time (UTC, yyMMddHHmmss), the process id and a random number, because several worktrees and
		///test runners share the one local server.</summary>
		public static readonly string DbName=PREFIX+DateTime.UtcNow.ToString("yyMMddHHmmss")+"_"+System.Diagnostics.Process.GetCurrentProcess().Id
			+"_"+new Random().Next(1000,10000);
		///<summary>Null when the scratch database is ready; otherwise why database tests are skipped.</summary>
		public static string SkipReason { get; private set; }
		private static bool _isCreated;

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext testContext) {
			SetCultureUs();
			if(Environment.GetEnvironmentVariable("S3AM1_TEST_DB")=="skip") {
				SkipReason="S3AM1_TEST_DB=skip";
				return;
			}
			try {
				using(MySqlConnection connection=OpenServer()) {
					Exec(connection,"CREATE DATABASE `"+DbName+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
				}
			}
			catch(Exception ex) {
				SkipReason="No local MariaDB at "+Server+":3306 as "+User+" with a blank password: "+ex.GetBaseException().Message;
				return;
			}
			_isCreated=true;
			//The server answers, so from here on a failure is a real failure, not a reason to skip.
			DropStaleScratchDatabases();
			using(MySqlConnection connection=OpenServer(DbName)) {
				foreach(SchemaTable table in SchemaVerifier.GetReference().Tables.Where(x => x.Name.In("language","languageforeign","preference"))) {
					Exec(connection,table.ToDdl());
				}
			}
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientDirect;
			Connect();
			//Every preference exists (RandomPrimaryKeys off for the inserts of Lans.ConvertString, WikiDetectLinks off for PopupHelper).
			List<string> listPrefRows=Enum.GetNames(typeof(PrefName)).Select(x => "('"+POut.String(x)+"','0')").ToList();
			DataCore.NonQ("INSERT INTO preference (PrefName,ValueString) VALUES "+string.Join(",",listPrefRows));
			Prefs.RefreshCache();
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup() {
			if(!_isCreated || Environment.GetEnvironmentVariable("S3AM1_KEEP_DB")=="1") {
				return;
			}
			try {
				Drop(DbName);
			}
			catch(Exception) {
				//Leaving the scratch database behind is harmless; a later run drops it once it is stale.
			}
		}

		public static void Connect() {
			new DataConnection().SetDb(Server,DbName,User,Password,"","",DatabaseType.MySql,true,DevDbTls.GetSslCa(Server));
		}

		///<summary>Call from a database test: marks the test inconclusive when there is no scratch database, otherwise empties the language
		///tables and the two language caches.</summary>
		public static void ResetOrSkip() {
			if(SkipReason!=null) {
				Assert.Inconclusive(SkipReason);
			}
			Connect();
			DataCore.NonQ("DELETE FROM language");
			DataCore.NonQ("DELETE FROM languageforeign");
			Lans.ClearCache();
		}

		public static void SetCultureUs() {
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
			Thread.CurrentThread.CurrentUICulture=new CultureInfo("en-US");
		}

		///<summary>Runs action with this thread's culture set to cultureName, as on a workstation of that culture, then puts it back.</summary>
		public static void RunInCulture(string cultureName,Action action) {
			CultureInfo cultureOld=Thread.CurrentThread.CurrentCulture;
			CultureInfo cultureUiOld=Thread.CurrentThread.CurrentUICulture;
			try {
				Thread.CurrentThread.CurrentCulture=new CultureInfo(cultureName);
				Thread.CurrentThread.CurrentUICulture=new CultureInfo(cultureName);
				action();
			}
			finally {
				Thread.CurrentThread.CurrentCulture=cultureOld;
				Thread.CurrentThread.CurrentUICulture=cultureUiOld;
			}
		}

		///<summary>Adds a translation for the current culture, with its English row in the language table, as the translation window saves it.</summary>
		public static void AddTranslation(string classType,string english,string translation) {
			DataCore.NonQ("INSERT INTO language (EnglishComments,ClassType,English,IsObsolete) VALUES ('','"+POut.String(classType)+"','"
				+POut.String(english)+"',0)");
			DataCore.NonQ("INSERT INTO languageforeign (ClassType,English,Culture,Translation,Comments) VALUES ('"+POut.String(classType)+"','"
				+POut.String(english)+"','"+POut.String(CultureInfo.CurrentCulture.Name)+"','"+POut.String(translation)+"','')");
		}

		///<summary>Fills both language caches from the database, after the rows of a test are in. Run in the test's culture.</summary>
		public static void RefreshLanguageCaches() {
			Lans.RefreshCache();
			LanguageForeigns.RefreshCache();
		}

		///<summary>The English texts the language table holds for classType: the translation keys that Lans.ConvertString inserted or that the
		///test added, sorted.</summary>
		public static List<string> GetKeys(string classType) {
			return DataCore.GetTable("SELECT English FROM language WHERE ClassType='"+POut.String(classType)+"'").Rows.OfType<System.Data.DataRow>()
				.Select(x => x["English"].ToString()).OrderBy(x => x,StringComparer.Ordinal).ToList();
		}

		private static void DropStaleScratchDatabases() {
			Regex regex=new Regex("^"+PREFIX+@"(\d{12})_\d+_\d+$");
			List<string> listNames=new List<string>();
			using(MySqlConnection connection=OpenServer())
			using(MySqlCommand command=new MySqlCommand("SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME LIKE 's3am1\\_%'",connection))
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
				if(DateTime.UtcNow-dateStarted>_timeSpanStale) {
					Drop(name);
				}
			}
		}

		public static void Drop(string name) {
			if(!name.StartsWith(PREFIX)) {
				throw new ArgumentException("Tests only drop databases whose name starts with "+PREFIX);
			}
			using(MySqlConnection connection=OpenServer()) {
				Exec(connection,"DROP DATABASE IF EXISTS `"+name+"`");
			}
		}

		public static MySqlConnection OpenServer(string database=null) {
			string connectionString="Server="+Server+";Port=3306;User ID="+User+";Password="+Password+";Connection Timeout=15";
			if(database!=null) {
				connectionString+=";Database="+database;
			}
			MySqlConnection connection=new MySqlConnection(DevDbTls.Apply(connectionString));
			connection.Open();
			return connection;
		}

		public static void Exec(MySqlConnection connection,string command) {
			using(MySqlCommand mySqlCommand=new MySqlCommand(command,connection)) {
				mySqlCommand.CommandTimeout=600;
				mySqlCommand.ExecuteNonQuery();
			}
		}
	}

	///<summary>Reads and calls private and internal members of the controls under test, and raises their protected events, without showing them.</summary>
	public static class Priv {
		private const BindingFlags FLAGS=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;

		public static object Get(object obj,string name) {
			for(Type type=obj.GetType();type!=null;type=type.BaseType) {
				FieldInfo fieldInfo=type.GetField(name,FLAGS|BindingFlags.DeclaredOnly);
				if(fieldInfo!=null) {
					return fieldInfo.GetValue(obj);
				}
				PropertyInfo propertyInfo=type.GetProperty(name,FLAGS|BindingFlags.DeclaredOnly);
				if(propertyInfo!=null && propertyInfo.CanRead) {
					return propertyInfo.GetValue(obj);
				}
			}
			throw new MissingMemberException(obj.GetType().Name,name);
		}

		public static void Set(object obj,string name,object value) {
			for(Type type=obj.GetType();type!=null;type=type.BaseType) {
				FieldInfo fieldInfo=type.GetField(name,FLAGS|BindingFlags.DeclaredOnly);
				if(fieldInfo!=null) {
					fieldInfo.SetValue(obj,value);
					return;
				}
				PropertyInfo propertyInfo=type.GetProperty(name,FLAGS|BindingFlags.DeclaredOnly);
				if(propertyInfo!=null && propertyInfo.CanWrite) {
					propertyInfo.SetValue(obj,value);
					return;
				}
			}
			throw new MissingMemberException(obj.GetType().Name,name);
		}

		public static object Call(object obj,string name,params object[] arrayArgs) {
			for(Type type=obj.GetType();type!=null;type=type.BaseType) {
				MethodInfo methodInfo=type.GetMethods(FLAGS|BindingFlags.DeclaredOnly)
					.FirstOrDefault(x => x.Name==name && x.GetParameters().Length==arrayArgs.Length);
				if(methodInfo!=null) {
					try {
						return methodInfo.Invoke(obj,arrayArgs);
					}
					catch(TargetInvocationException ex) {
						throw ex.InnerException;
					}
				}
			}
			throw new MissingMethodException(obj.GetType().Name,name);
		}
	}
}
