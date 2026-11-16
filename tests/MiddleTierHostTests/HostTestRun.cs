// The run of the Middle Tier host tests (package S3b-6 of the .NET 10 move, docs/port-notes/S3b-6.md): the scratch databases, the servers
// and the client's set-up, once for the assembly. In process (the default): three servers on the test server, each in its own load context
// with its own scratch database (A and B for the round trips and the two users on two databases; V with an old ProgramVersion and a 1 MiB
// body limit, for the version mismatch and 413). Against a running server (MIDDLETIER_TEST_URL, the 3b gate's check of the Windows service):
// that server alone, on the scratch database MIDDLETIER_TEST_DB that this run builds before the first request, and the tests that need the
// in-process servers are inconclusive.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.WebServices;

namespace MiddleTierHostTests {
	///<summary>A server the client sends to: its address, the database behind it and its name for messages.</summary>
	public class HostTarget {
		public readonly string Name;
		public readonly string Url;
		public readonly string Database;

		public HostTarget(string name,string url,string database) {
			Name=name;
			Url=url;
			Database=database;
		}
	}

	[TestClass]
	public class HostTestRun {
		///<summary>The address of a running Middle Tier server to test instead of the in-process ones, like
		///https://127.0.0.1:7443/OpenDentalServer/ServiceMain.asmx.</summary>
		public const string VARIABLE_URL="MIDDLETIER_TEST_URL";
		///<summary>The scratch database the running server's config file names (s3b6_ first; s3b6_gate when not set). This run builds it.</summary>
		public const string VARIABLE_DB="MIDDLETIER_TEST_DB";
		///<summary>The SHA-1 thumbprint of the running server's test certificate, when Windows does not trust it.</summary>
		public const string VARIABLE_THUMBPRINT="MIDDLETIER_TEST_CERT_THUMBPRINT";
		///<summary>1 keeps the scratch databases for a look afterwards.</summary>
		public const string VARIABLE_KEEP_DB="S3B6_KEEP_DB";
		public const string DB_GATE_DEFAULT="s3b6_gate";
		///<summary>The users, in every database. Their passwords hold XML's special characters, which the credentials carry in every request.
		///</summary>
		public static readonly HostUser UserA=new HostUser("s3b6usera","A-pass <&> \"'é");
		public static readonly HostUser UserB=new HostUser("s3b6userb","B-pass &amp; <b>ü");
		///<summary>The user of the wrong-password test only, so its failed attempts lock no other user out.</summary>
		public static readonly HostUser UserBad=new HostUser("s3b6userbad","Bad-pass-right");
		///<summary>The in-process body limit of server V: 1 MiB.</summary>
		public const long SMALL_LIMIT=1048576;
		public static HostTarget TargetA;
		public static HostTarget TargetB;
		///<summary>Null against a running server.</summary>
		public static HostTarget TargetVersion;
		public static RoutingHandler RoutingHandlerCur;
		public static IsolatedServer ServerA;
		public static IsolatedServer ServerB;
		public static IsolatedServer ServerVersion;
		///<summary>This build's version, which the databases A and B have as ProgramVersion.</summary>
		public static string VersionBuild;
		private static string _skipReason;
		private static string _setupError;
		private static readonly List<string> _listDatabases=new List<string>();

		public static bool IsExternal {
			get {
				return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(VARIABLE_URL));
			}
		}

		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext testContext) {
			Thread.CurrentThread.CurrentCulture=new CultureInfo("en-US");
			ODBuild.IsUnitTest=true;
			ODInitialize.Initialize();
			_skipReason=HostDb.GetSkipReason();
			if(_skipReason!=null) {
				return;
			}
			try {
				VersionBuild=typeof(Db).Assembly.GetName().Version.ToString();
				List<HostUser> listHostUsers=new List<HostUser> { UserA,UserB,UserBad };
				if(IsExternal) {
					string url=Environment.GetEnvironmentVariable(VARIABLE_URL).Trim();
					string database=Environment.GetEnvironmentVariable(VARIABLE_DB);
					database=string.IsNullOrWhiteSpace(database) ? DB_GATE_DEFAULT : database.Trim();
					_listDatabases.Add(database);
					HostDb.Create(database,VersionBuild,listHostUsers);
					TargetA=new HostTarget("the server at "+url,url,database);
					TargetB=TargetA;
					RoutingHandlerCur=new RoutingHandler(Environment.GetEnvironmentVariable(VARIABLE_THUMBPRINT));
				}
				else {
					HostDb.DropStale();
					string databaseA=HostDb.Name("a");
					string databaseB=HostDb.Name("b");
					string databaseVersion=HostDb.Name("v");
					_listDatabases.AddRange(new[] { databaseA,databaseB,databaseVersion });
					HostDb.Create(databaseA,VersionBuild,listHostUsers);
					HostDb.Create(databaseB,VersionBuild,listHostUsers);
					//V keeps the new-database file's ProgramVersion (99.0.0.0), never this build's.
					HostDb.Create(databaseVersion,null,listHostUsers);
					RoutingHandlerCur=new RoutingHandler(null);
					ServerA=IsolatedServer.Start("a","/S3b6A",databaseA,null);
					ServerB=IsolatedServer.Start("b","/S3b6B",databaseB,null);
					ServerVersion=IsolatedServer.Start("v","/S3b6V",databaseVersion,SMALL_LIMIT);
					RoutingHandlerCur.AddHost("s3b6-a.example.test",ServerA.Handler);
					RoutingHandlerCur.AddHost("s3b6-b.example.test",ServerB.Handler);
					RoutingHandlerCur.AddHost("s3b6-v.example.test",ServerVersion.Handler);
					TargetA=new HostTarget("server A",UrlOf("s3b6-a.example.test","/S3b6A"),databaseA);
					TargetB=new HostTarget("server B",UrlOf("s3b6-b.example.test","/S3b6B"),databaseB);
					TargetVersion=new HostTarget("server V",UrlOf("s3b6-v.example.test","/S3b6V"),databaseVersion);
				}
				OpenDentalServerHttp.HttpMessageHandlerOverride=RoutingHandlerCur;
				RemotingClient.MiddleTierRole=MiddleTierRole.ClientMT;
			}
			catch(Exception ex) {
				_setupError="The host tests could not set up: "+ex;
			}
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup() {
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientDirect;
			RemotingClient.HasAutomaticConnectionLostRetry=false;
			OpenDentalServerHttp.HttpMessageHandlerOverride=null;
			foreach(IsolatedServer isolatedServer in new[] { ServerA,ServerB,ServerVersion }) {
				isolatedServer?.Dispose();
			}
			if(Environment.GetEnvironmentVariable(VARIABLE_KEEP_DB)=="1") {
				return;
			}
			//The servers' pooled connections (MySqlConnector is shared by every load context) are closed first.
			MySqlConnection.ClearAllPools();
			foreach(string database in _listDatabases) {
				try {
					HostDb.Drop(database);
				}
				catch(Exception) {
					//A later run drops an in-process database once it is stale.
				}
			}
		}

		///<summary>The address of an in-process server, as a workstation would save it.</summary>
		public static string UrlOf(string host,string pathBase) {
			return "https://"+host+pathBase+"/ServiceMain.asmx";
		}

		///<summary>Call first in every test: inconclusive without the local MariaDB, failed when the set-up failed.</summary>
		public static void Begin() {
			if(_skipReason!=null) {
				Assert.Inconclusive(_skipReason);
			}
			if(_setupError!=null) {
				Assert.Fail(_setupError);
			}
			RemotingClient.HasAutomaticConnectionLostRetry=false;
			RemotingClient.HasLoginFailed=false;
			RemotingClient.HasMiddleTierConnectionFailed=false;
		}

		///<summary>For a test that needs the in-process servers: inconclusive against a running server.</summary>
		public static void RequireInProcess(string reason) {
			if(IsExternal) {
				Assert.Inconclusive("Needs the in-process servers ("+reason+"); "+VARIABLE_URL+" names a running server.");
			}
		}

		///<summary>This thread is a Middle Tier client of target, logged in as hostUser: the role, the address and the credentials every request
		///carries. The setters set the process's values too, which threads the business layer starts read.</summary>
		public static void UseTarget(HostTarget hostTarget,HostUser hostUser) {
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientMT;
			RemotingClient.ServerURI=hostTarget.Url;
			Security.CurUser=new Userod { UserName=hostUser.UserName };
			Security.PasswordTyped=hostUser.Password;
		}

		///<summary>Only this thread is a Middle Tier client of target as hostUser, for the threads of the concurrency test.</summary>
		public static void UseTargetOnThisThread(HostTarget hostTarget,HostUser hostUser) {
			RemotingClient.SetRemotingRoleT(MiddleTierRole.ClientMT);
			RemotingClient.SetServerURIT(hostTarget.Url);
			Security.CurUser=new Userod { UserName=hostUser.UserName };
			Security.PasswordTyped=hostUser.Password;
		}
	}
}
