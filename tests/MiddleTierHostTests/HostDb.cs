// The scratch databases of the Middle Tier host tests (package S3b-6 of the .NET 10 move, docs/port-notes/S3b-6.md) on the local MariaDB
// (127.0.0.1:3306, root, blank password): the committed db/init/new-database.sql with every fork migration of this build run on it, the
// ProgramVersion of this build and the tests' users. In-process names start with s3b6_ht_ plus this run's id, so a run never drops anything
// else and runs from other worktrees never collide; the database of a run against a server URL is named by MIDDLETIER_TEST_DB and must start
// with s3b6_. Synthetic rows only. Skipped (inconclusive) when the server does not answer, or when S3B6_TEST_DB=skip.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DataConnectionBase;
using MySqlConnector;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations;

namespace MiddleTierHostTests {
	///<summary>A user the tests log in as: the name and the password typed.</summary>
	public class HostUser {
		public readonly string UserName;
		public readonly string Password;

		public HostUser(string userName,string password) {
			UserName=userName;
			Password=password;
		}
	}

	public static class HostDb {
		public const string Server="127.0.0.1";
		public const string User="root";
		public const string Password="";
		///<summary>Every database these tests create or drop starts with this.</summary>
		public const string PREFIX_PACKAGE="s3b6_";
		///<summary>The in-process run's databases.</summary>
		public const string PREFIX="s3b6_ht_";
		private static readonly TimeSpan _timeSpanStale=TimeSpan.FromHours(12);
		///<summary>Start time (UTC, yyMMddHHmmss), process id and a random number.</summary>
		private static readonly string _runId=DateTime.UtcNow.ToString("yyMMddHHmmss",CultureInfo.InvariantCulture)+"_"
			+System.Diagnostics.Process.GetCurrentProcess().Id+"_"+new Random().Next(1000,10000);

		///<summary>This run's database name for the given purpose, like s3b6_ht_261116101500_1234_5678_a.</summary>
		public static string Name(string purpose) {
			string name=PREFIX+_runId+"_"+purpose;
			CheckName(name);
			return name;
		}

		///<summary>Null when the local server answers; otherwise why the tests are skipped.</summary>
		public static string GetSkipReason() {
			if(Environment.GetEnvironmentVariable("S3B6_TEST_DB")=="skip") {
				return "S3B6_TEST_DB=skip";
			}
			try {
				using(MySqlConnection connection=OpenServer()) {
				}
				return null;
			}
			catch(Exception ex) {
				return "No local MariaDB at "+Server+":3306 as "+User+" with a blank password: "+ex.GetBaseException().Message;
			}
		}

		public static MySqlConnection OpenServer(string database="") {
			MySqlConnection connection=new MySqlConnection(DevDbTls.Apply("Server="+Server+";Port=3306;User ID="+User+";Password="+Password
				+(database=="" ? "" : ";Database="+database)+";Connection Timeout=15;Allow User Variables=true;Default Command Timeout=600"));
			connection.Open();
			return connection;
		}

		///<summary>Builds the database: the new-database file, the fork migrations, ProgramVersion (null keeps the file's own value) and the users
		///(each in the Admin group, with its password hashed as the program hashes it). Leaves the business layer connected to it directly.
		///</summary>
		public static void Create(string name,string programVersion,IEnumerable<HostUser> listHostUsers) {
			CheckName(name);
			Drop(name);
			using(MySqlConnection connection=OpenServer()) {
				Exec(connection,"CREATE DATABASE `"+name+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
			}
			string text=File.ReadAllText(FindRepoFile(Path.Combine("db","init","new-database.sql")),new UTF8Encoding(false)).Replace("\r\n","\n");
			using(MySqlConnection connection=OpenServer(name)) {
				//Every statement of the generated file ends with a semicolon at the end of a line, and no value holds a raw line break.
				foreach(string statement in text.Split(new[] { ";\n" },StringSplitOptions.None)) {
					if(statement.Trim()=="") {
						continue;
					}
					Exec(connection,statement);
				}
				if(programVersion!=null) {
					using(MySqlCommand command=new MySqlCommand("UPDATE preference SET ValueString=@Version WHERE PrefName='ProgramVersion'",connection)) {
						command.Parameters.AddWithValue("@Version",programVersion);
						command.ExecuteNonQuery();
					}
				}
			}
			Connect(name);
			if(!Prefs.DictIsNull()) {
				Prefs.ClearCache();
			}
			Cache.Refresh(InvalidType.Prefs);
			ForkMigrationRunner.Run();
			Cache.Refresh(InvalidType.AllLocal);
			Security.CurUser=Userods.GetUser(1)??new Userod { UserNum=1,UserName="Admin" };
			foreach(HostUser hostUser in listHostUsers) {
				Userod userod=new Userod();
				userod.UserName=hostUser.UserName;
				userod.SetPassword(Authentication.GenerateLoginDetails(hostUser.Password,HashTypes.SHA3_512));
				Userods.Insert(userod,new List<long> { 1 });
			}
			Userods.RefreshCache();
			UserGroupAttaches.RefreshCache();
		}

		///<summary>The value of a preference, read straight from the database.</summary>
		public static string GetPref(string name,string prefName) {
			CheckName(name);
			using(MySqlConnection connection=OpenServer(name))
			using(MySqlCommand command=new MySqlCommand("SELECT ValueString FROM preference WHERE PrefName=@PrefName",connection))
			{
				command.Parameters.AddWithValue("@PrefName",prefName);
				object value=command.ExecuteScalar();
				return value==null || value==DBNull.Value ? null : value.ToString();
			}
		}

		public static void Connect(string name) {
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientDirect;
			new DataConnection().SetDb(Server,name,User,Password,"","",DatabaseType.MySql,true,DevDbTls.GetSslCa(Server));
		}

		public static void Drop(string name) {
			CheckName(name);
			using(MySqlConnection connection=OpenServer()) {
				Exec(connection,"DROP DATABASE IF EXISTS `"+name+"`");
			}
		}

		///<summary>Drops the in-process runs' leftovers (a crash, or S3B6_KEEP_DB=1) once they are older than 12 hours.</summary>
		public static void DropStale() {
			Regex regex=new Regex("^"+PREFIX+@"(\d{12})_\d+_\d+_[a-z0-9_]+$");
			List<string> listNames=new List<string>();
			using(MySqlConnection connection=OpenServer())
			using(MySqlCommand command=new MySqlCommand("SELECT SCHEMA_NAME FROM information_schema.SCHEMATA WHERE SCHEMA_NAME LIKE 's3b6\\_ht\\_%'",
				connection))
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

		///<summary>Throws unless the name is one these tests may create and drop: s3b6_, then lower-case letters, digits and underscores, at most 64
		///characters.</summary>
		public static void CheckName(string name) {
			if(name==null || !name.StartsWith(PREFIX_PACKAGE) || name.Length>64 || !Regex.IsMatch(name,"^[a-z0-9_]+$")) {
				throw new ArgumentException("These tests only create and drop databases whose name starts with "+PREFIX_PACKAGE
					+" (lower-case letters, digits and underscores, at most 64 characters): "+name);
			}
		}

		public static void Exec(MySqlConnection connection,string command) {
			using(MySqlCommand mySqlCommand=new MySqlCommand(command,connection)) {
				mySqlCommand.CommandTimeout=600;
				mySqlCommand.ExecuteNonQuery();
			}
		}

		///<summary>Walks up from the test assembly's folder to the repository root and returns the path of a file in it.</summary>
		public static string FindRepoFile(string relativePath) {
			DirectoryInfo dir=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while(dir!=null) {
				string candidate=Path.Combine(dir.FullName,relativePath);
				if(File.Exists(candidate)) {
					return candidate;
				}
				dir=dir.Parent;
			}
			throw new FileNotFoundException("Not found above the test folder: "+relativePath);
		}
	}
}
