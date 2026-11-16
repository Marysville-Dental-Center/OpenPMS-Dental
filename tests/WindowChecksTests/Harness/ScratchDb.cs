// The scratch database of a run: the dump of the synthetic test database (OPMS_WINDOW_CHECKS_DUMP, made once from it) restored with the local
// MariaDB's own mysql.exe into a new database winchk_<time>, on 127.0.0.1:3306 only. Every check runs against this copy, and the copy is
// dropped at the end; a copy an earlier run left (the runner killed it before its clean-up) is dropped at the start of the next run with the
// same results folder (DropLeftovers). Only names starting with winchk_ are ever created or dropped. The copy's A to Z folder is moved to a scratch folder in
// the temp folder, so no check writes into the test database's own images folder. Right after the restore the copy is brought to this build's fork
// schema level (CopyMigration, WINCHK-4), so the program never asks to update it; when that cannot be done, P0 fails and no check runs.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataConnectionBase;
using MySqlConnector;

namespace WindowChecksTests {
	public static class ScratchDb {
		public const string PREFIX="winchk_";
		public const string Server="127.0.0.1";
		public const int Port=3306;
		private static string _name;
		private static string _atoZFolder;
		private static string _skipReason;
		///<summary>P0's line on the fork schema level of the copy (CopyMigrationResult.Text): migrated, already there, or why not.</summary>
		private static string _migrationText;
		///<summary>True when the copy could not be brought to this build's fork schema level: P0 fails and no check runs.</summary>
		private static bool _isMigrationFailed;
		private static bool _hasTried;
		private static readonly object _lock=new object();

		///<summary>This run's database, made on first use.</summary>
		public static string Name {
			get {
				Ensure();
				return _name;
			}
		}

		///<summary>The copy's A to Z folder (DocPath), a scratch folder in the temp folder with the letter folders.</summary>
		public static string AtoZFolder {
			get {
				Ensure();
				return _atoZFolder;
			}
		}

		///<summary>Null when the copy is ready; otherwise why it could not be made.</summary>
		public static string GetSkipReason() {
			Ensure();
			return _skipReason;
		}

		public static bool IsMade {
			get {
				return _name!=null && _skipReason==null;
			}
		}

		///<summary>P0's line on the copy's fork schema level ("migrated the copy from A to B: steps ...", "already at A", or why it could not be done);
		///null before the restore.</summary>
		public static string MigrationText {
			get {
				Ensure();
				return _migrationText;
			}
		}

		///<summary>True when the copy was restored but could not be brought to this build's fork schema level (the copy is dropped at once).</summary>
		public static bool IsMigrationFailed {
			get {
				Ensure();
				return _isMigrationFailed;
			}
		}

		private static void Ensure() {
			lock(_lock) {
				if(_hasTried) {
					return;
				}
				_hasTried=true;
				try {
					Make();
				}
				catch(Exception ex) {
					_skipReason="The scratch copy of the test database could not be made: "+ex.GetBaseException().Message;
					Results.Log(_skipReason);
					//AssemblyCleanup drops only a copy that was made: a database created for a restore that failed is dropped here.
					try {
						Drop();
					}
					catch(Exception exDrop) {
						Results.Log("Drop failed: "+exDrop.Message);
					}
				}
			}
		}

		private static void Make() {
			string name=PREFIX+DateTime.Now.ToString("MMddHHmmss");
			using(MySqlConnection connection=Open("")) {
				DropLeftovers(connection);
				Exec(connection,"CREATE DATABASE `"+name+"` CHARACTER SET utf8mb3");
			}
			_name=name;
			Results.Log("Created "+name);
			Stopwatch stopwatch=Stopwatch.StartNew();
			List<string> listArgs=new List<string> { "-h",Server,"-P",Port.ToString(),"-u","root","--default-character-set=utf8mb4" };
			listArgs.AddRange(DevDbTls.GetClientArguments(Server,Port));
			listArgs.Add(name);
			ProcessStartInfo processStartInfo=new ProcessStartInfo(Path.Combine(WinChkEnv.MySqlBin,"mysql.exe"),
				string.Join(" ",listArgs.Select(x => x.Contains(" ") ? "\""+x+"\"" : x)))
			{
				UseShellExecute=false,
				RedirectStandardInput=true,
				RedirectStandardError=true,
				RedirectStandardOutput=true,
				CreateNoWindow=true,
			};
			using(Process process=Process.Start(processStartInfo)) {
				System.Threading.Tasks.Task<string> taskError=process.StandardError.ReadToEndAsync();
				System.Threading.Tasks.Task<string> taskOut=process.StandardOutput.ReadToEndAsync();
				using(FileStream fileStream=File.OpenRead(WinChkEnv.DumpPath)) {
					fileStream.CopyTo(process.StandardInput.BaseStream);
				}
				process.StandardInput.Close();
				if(!process.WaitForExit(600000)) {
					process.Kill();
					throw new Exception("mysql.exe did not restore the dump within 10 minutes.");
				}
				if(process.ExitCode!=0) {
					throw new Exception("mysql.exe exit "+process.ExitCode+": "+taskError.Result);
				}
			}
			Results.Log("Restored the dump into "+name+" in "+stopwatch.Elapsed.TotalSeconds.ToString("0.0")+" s");
			//Never let the program contact a time server: a ForkTimeSyncMode other than 0 stops the run.
			string timeSync=Scalar("SELECT ValueString FROM preference WHERE PrefName='ForkTimeSyncMode'");
			if(timeSync!="" && timeSync!="0") {
				throw new Exception("ForkTimeSyncMode is "+timeSync+" in the copy: the checks would contact a time server. Stopped.");
			}
			string patient=Scalar("SELECT CONCAT(LName,', ',FName) FROM patient WHERE PatNum=1");
			if(patient!="Gatetest, Stage") {
				throw new Exception("Patient #1 is '"+patient+"', not the synthetic 'Gatetest, Stage'.");
			}
			//This build's fork schema level, with the program's own migration code, before any check starts the program (start-up would ask).
			stopwatch.Restart();
			CopyMigrationResult copyMigrationResult;
			try {
				copyMigrationResult=CopyMigration.Run(name);
			}
			catch(Exception ex) {
				copyMigrationResult=new CopyMigrationResult { Text="the copy could not be brought to this build's fork schema level: "+ex.GetBaseException().Message };
			}
			foreach(string line in copyMigrationResult.ListLog) {
				Results.Log("  "+line);
			}
			_migrationText=copyMigrationResult.Text;
			Results.Log("Fork schema level of "+name+": "+copyMigrationResult.Text+" ("+stopwatch.Elapsed.TotalSeconds.ToString("0.0")+" s)");
			if(!copyMigrationResult.IsDone) {
				_isMigrationFailed=true;
				throw new Exception(copyMigrationResult.Text);
			}
			_atoZFolder=Path.Combine(Path.GetTempPath(),"winchk",name,"AtoZ");
			Directory.CreateDirectory(_atoZFolder);
			for(char letter='A';letter<='Z';letter++) {
				Directory.CreateDirectory(Path.Combine(_atoZFolder,letter.ToString()));
			}
			using(MySqlConnection connection=Open(name)) {
				MySqlCommand command=new MySqlCommand("UPDATE preference SET ValueString=@path WHERE PrefName='DocPath'",connection);
				command.Parameters.AddWithValue("@path",_atoZFolder+"\\");
				command.ExecuteNonQuery();
				//No computer of the copy overrides the A to Z folder.
				Exec(connection,"UPDATE computerpref SET AtoZpath=''");
			}
		}

		///<summary>Drops the scratch copies earlier runs with this results folder made and never dropped (a run the test runner killed skips
		///AssemblyCleanup): only names run.log says this harness created, that are still on the server and were made more than 25 minutes ago (the
		///runner kills a run at 20), so a run going on beside this one is never touched. Each one is logged as dropped.</summary>
		private static void DropLeftovers(MySqlConnection connection) {
			string pathLog=Path.Combine(WinChkEnv.OutFolder,"run.log");
			if(!File.Exists(pathLog)) {
				return;
			}
			string log=File.ReadAllText(pathLog);
			List<string> listCreated=Regex.Matches(log,"(?:Created|Restored the dump into) ("+PREFIX+"\\d{10})\\b").Cast<Match>().Select(x => x.Groups[1].Value)
				.Distinct().ToList();
			HashSet<string> hashSetDropped=new HashSet<string>(Regex.Matches(log,"Dropped ("+PREFIX+"\\d{10})\\b").Cast<Match>().Select(x => x.Groups[1].Value));
			List<string> listOnServer=new List<string>();
			using(MySqlCommand command=new MySqlCommand("SHOW DATABASES",connection))
			using(MySqlDataReader reader=command.ExecuteReader()) {
				while(reader.Read()) {
					listOnServer.Add(reader.GetString(0));
				}
			}
			foreach(string name in listCreated.Where(x => !hashSetDropped.Contains(x) && listOnServer.Contains(x))) {
				if(!DateTime.TryParseExact(DateTime.Now.Year+name.Substring(PREFIX.Length),"yyyyMMddHHmmss",CultureInfo.InvariantCulture,DateTimeStyles.None,
					out DateTime dateMade))
				{
					continue;
				}
				if(dateMade>DateTime.Now) {
					dateMade=dateMade.AddYears(-1);
				}
				if((DateTime.Now-dateMade).TotalMinutes<25) {
					Results.Log("Leftover "+name+" kept: made "+(DateTime.Now-dateMade).TotalMinutes.ToString("0")+" minutes ago (a run may still use it)");
					continue;
				}
				Exec(connection,"DROP DATABASE IF EXISTS `"+name+"`");
				Results.Log("Dropped "+name+" (a leftover of an earlier run that was stopped before its clean-up)");
				try {
					Directory.Delete(Path.Combine(Path.GetTempPath(),"winchk",name),true);
				}
				catch(Exception) {
					//No scratch folder left, or not removable now: only the temp folder.
				}
			}
		}

		public static void Drop() {
			lock(_lock) {
				if(_name==null || !_name.StartsWith(PREFIX)) {
					return;
				}
				using(MySqlConnection connection=Open("")) {
					Exec(connection,"DROP DATABASE IF EXISTS `"+_name+"`");
				}
				Results.Log("Dropped "+_name);
				try {
					Directory.Delete(Path.Combine(Path.GetTempPath(),"winchk",_name),true);
				}
				catch(Exception ex) {
					Results.Log("Scratch A to Z folder not deleted: "+ex.Message);
				}
			}
		}

		///<summary>A connection to the local test server; database "" for none. Only winchk_ databases are ever opened.</summary>
		public static MySqlConnection Open(string database) {
			if(database!="" && !database.StartsWith(PREFIX)) {
				throw new ArgumentException("The window checks only open databases whose name starts with "+PREFIX);
			}
			MySqlConnection connection=new MySqlConnection(DevDbTls.Apply("Server="+Server+";Port="+Port+";User ID=root;Password=;Connection Timeout=15;"
				+"Allow User Variables=true"+(database=="" ? "" : ";Database="+database)));
			connection.Open();
			return connection;
		}

		public static void Exec(MySqlConnection connection,string command) {
			using MySqlCommand mySqlCommand=new MySqlCommand(command,connection);
			mySqlCommand.ExecuteNonQuery();
		}

		///<summary>A command against this run's copy.</summary>
		public static void Exec(string command) {
			using MySqlConnection connection=Open(Name);
			Exec(connection,command);
		}

		///<summary>The first value of the first row of a query on this run's copy, or "" when there is none.</summary>
		public static string Scalar(string command) {
			using MySqlConnection connection=Open(_name);
			using MySqlCommand mySqlCommand=new MySqlCommand(command,connection);
			object value=mySqlCommand.ExecuteScalar();
			return value==null || value==DBNull.Value ? "" : value.ToString();
		}

		public static DataTable Table(string command) {
			using MySqlConnection connection=Open(_name);
			using MySqlDataAdapter adapter=new MySqlDataAdapter(command,connection);
			DataTable table=new DataTable();
			adapter.Fill(table);
			return table;
		}

		///<summary>A query's rows as "a|b|c" lines, for notes.</summary>
		public static string Rows(string command) {
			DataTable table=Table(command);
			return string.Join("; ",table.Rows.Cast<DataRow>().Select(x => string.Join("|",x.ItemArray.Select(y => y==DBNull.Value ? "NULL" : y.ToString()))));
		}
	}
}
