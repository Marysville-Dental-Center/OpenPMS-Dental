// The Middle Tier host round trips (package S3b-6 of the .NET 10 move, docs/port-notes/S3b-6.md, the S3b-6 row of docs/specs/net10-plan.md):
// the program's client (RemotingClient, Meth, OpenDentalServerHttp) against the server's endpoint (OpenPmsDentalServer), every DTO type that
// carries data with XML's special characters, the login, the availability check, the version check, the body limit, the connection-lost
// retry, a 50 MB payload and two users on two databases at the same time. Synthetic values only; host names are *.example.test.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.WebServices;

namespace MiddleTierHostTests {
	[TestClass]
	public class HostRoundTripTests {
		///<summary>XML's special characters, line ends, a tab, accented letters, CJK and a character outside the BMP.</summary>
		private const string TEXT_SPECIAL="<a href=\"x\">&amp; ' \" & < > ]]> \r\n\ttab \u00e9\u00fc\u00df \u4e2d\u6587 \U0001F600</a>";
		///<summary>TEXT_SPECIAL without the character outside the BMP, for text that goes into the database: the databases are utf8mb3
		///(db/README.md), which stores the BMP only.</summary>
		private const string TEXT_SPECIAL_BMP="<a href=\"x\">&amp; ' \" & < > ]]> \r\n\ttab \u00e9\u00fc\u00df \u4e2d\u6587</a>";

		[TestInitialize]
		public void SetupTest() {
			HostTestRun.Begin();
			HostTestRun.UseTarget(HostTestRun.TargetA,HostTestRun.UserA);
		}

		[TestCleanup]
		public void TearDownTest() {
			RemotingClient.HasAutomaticConnectionLostRetry=false;
			RemotingClient.HasMiddleTierConnectionFailed=false;
			RemotingClient.HasLoginFailed=false;
		}

		#region Login
		[TestMethod]
		public void Login_RightPassword_GivesTheUser() {
			Userod userod=Userods.CheckUserAndPassword(HostTestRun.UserA.UserName,HostTestRun.UserA.Password,false);
			Assert.IsNotNull(userod);
			Assert.AreEqual(HostTestRun.UserA.UserName,userod.UserName);
			Assert.IsTrue(userod.UserNum>0);
		}

		[TestMethod]
		public void Login_WrongPassword_IsRefused() {
			HostTestRun.UseTarget(HostTestRun.TargetA,new HostUser(HostTestRun.UserBad.UserName,"Bad-pass-wrong"));
			ODException odException=Assert.ThrowsExactly<ODException>(() =>
				Userods.CheckUserAndPassword(HostTestRun.UserBad.UserName,"Bad-pass-wrong",false));
			Assert.AreEqual((int)ODException.ErrorCodes.CheckUserAndPasswordFailed,odException.ErrorCode);
			StringAssert.Contains(odException.Message,"Invalid username or password");
		}
		#endregion Login

		#region The DTO types
		[TestMethod]
		public void DtoGetTable_SpecialCharactersInCellsAndColumnNames_ComeBack() {
			DataTable table=WebServiceTests.GetTableSpecialChars();
			char[] arrayChars={ '|','<','>','&','\'','"','\\','/' };
			Assert.AreEqual(4,table.Rows.Count);
			for(int i=0;i<table.Rows.Count;i++) {
				for(int j=0;j<table.Columns.Count-1;j++) {//the last column is DirtyString
					Assert.AreEqual("cell"+i+j+arrayChars[i*2+j],table.Rows[i][j].ToString());
				}
			}
			Assert.AreEqual("|<>",table.Columns[0].ColumnName);
			Assert.AreEqual(WebServiceTests.DirtyString,table.Rows[0]["DirtyString"].ToString(),"Every UTF-16 code unit, controls and lone surrogates included.");
		}

		[TestMethod]
		public void DtoGetObject_ObjectsWithXmlSpecialText_ComeBack() {
			Patient patient=WebServiceTests.GetObjectPat();
			Assert.AreEqual("Smith",patient.LName);
			Assert.IsNull(patient.FName);
			Assert.AreEqual(WebServiceTests.DirtyString,patient.AddrNote);
			//A row written and read through the server: the text goes into the scratch database and back.
			Patient patientNew=new Patient { LName="O'Brien <&> \"S3b6\"",FName=TEXT_SPECIAL_BMP,Address="1 Main St & <Suite> 2",PatStatus=PatientStatus.Patient,
				Birthdate=new DateTime(1990,1,2) };
			long patNum=Patients.Insert(patientNew,false);
			Assert.IsTrue(patNum>0);
			Patient patientDb=Patients.GetPat(patNum);
			Assert.AreEqual(patientNew.LName,patientDb.LName);
			Assert.AreEqual(TEXT_SPECIAL_BMP,patientDb.FName);
			Assert.AreEqual(patientNew.Address,patientDb.Address);
			Assert.AreEqual(new DateTime(1990,1,2),patientDb.Birthdate);
		}

		[TestMethod]
		public void DtoGetVoid_RunsAndCarriesTheServersException() {
			WebServiceTests.GetVoid();
			ODException odException=Assert.ThrowsExactly<ODException>(() => WebServiceTests.GetODException());
			Assert.AreEqual(36,odException.ErrorCode);
			Assert.AreEqual("New password cannot be the same as the current password.",odException.Message);
		}

		[TestMethod]
		public void DtoGetString_XmlSpecialText_ComesBack() {
			Assert.AreEqual("Processed: "+TEXT_SPECIAL,WebServiceTests.GetString(TEXT_SPECIAL));
			Assert.AreEqual("Processed: "+WebServiceTests.DirtyString,WebServiceTests.GetString(WebServiceTests.DirtyString));
			Assert.AreEqual(HostTestRun.TargetA.Database,MiscData.GetCurrentDatabase(),"The server reads the database its config file names.");
		}

		[TestMethod]
		public void DtoGetBoolLongInt_ComeBack() {
			Assert.IsTrue(WebServiceTests.GetBool());
			Assert.AreEqual(2L,WebServiceTests.GetLong(1));
			Assert.AreEqual(2,WebServiceTests.GetInt(1));
		}

		[TestMethod]
		public void IsMiddleTierAvailable_IsTrue() {
			Assert.IsTrue(RemotingClient.IsMiddleTierAvailable());
		}
		#endregion The DTO types

		#region The server's checks
		///<summary>Server V's database has the new-database file's ProgramVersion, not this build's: every request is refused with the server's
		///message, as a Release build refuses it (DtoProcessor.DoCheckVersionInDebug makes the Debug build check too).</summary>
		[TestMethod]
		public void VersionMismatch_IsRefusedWithBothVersions() {
			HostTestRun.RequireInProcess("server V's database has another ProgramVersion");
			HostTestRun.UseTarget(HostTestRun.TargetVersion,HostTestRun.UserA);
			string versionDb=HostDb.GetPref(HostTestRun.TargetVersion.Database,"ProgramVersion");
			Assert.AreNotEqual(HostTestRun.VersionBuild,versionDb);
			ODException odException=Assert.ThrowsExactly<ODException>(() => WebServiceTests.GetString("x"));
			Assert.AreEqual("Version mismatch.  Middle Tier Server: '"+HostTestRun.VersionBuild+"'  Database: '"+versionDb+"'",odException.Message);
		}

		///<summary>A body above the server's limit gets 413 before anything reads it, and the client says so; one just below it is processed. Server
		///V's limit is 1 MiB (the default, 1 GiB, is checked in ServerSettingsTests), and its version check comes after the limit.</summary>
		[TestMethod]
		public void RequestAboveTheLimit_Gets413() {
			HostTestRun.RequireInProcess("server V's limit is 1 MiB");
			OpenDentalServerHttp openDentalServerHttp=new OpenDentalServerHttp(HostTestRun.TargetVersion.Url);
			HttpRequestException httpRequestException=Assert.ThrowsExactly<HttpRequestException>(() =>
				openDentalServerHttp.ProcessRequest(new string('x',(int)HostTestRun.SMALL_LIMIT+1)));
			Assert.AreEqual(HttpStatusCode.RequestEntityTooLarge,httpRequestException.StatusCode);
			StringAssert.Contains(httpRequestException.Message,"larger than the Middle Tier server accepts ("+HostTestRun.SMALL_LIMIT+" bytes");
			Assert.IsFalse(OpenDentalServerHttp.IsConnectionFailure(httpRequestException),"An error answer is not a lost connection.");
			//Exactly the limit is read and processed (here: refused by the version check, which comes after it).
			HostTestRun.UseTarget(HostTestRun.TargetVersion,HostTestRun.UserA);
			string answer=openDentalServerHttp.ProcessRequest(new string('x',(int)HostTestRun.SMALL_LIMIT));
			StringAssert.Contains(answer,"DtoException");
		}

		///<summary>The server answers HTTPS only, at {PathBase}/ServiceMain.asmx and {PathBase}/, POST only.</summary>
		[TestMethod]
		public void Endpoint_HttpsOnly_OnePath_PostOnly() {
			HostTestRun.RequireInProcess("the test server's handler");
			using(HttpClient httpClient=new HttpClient(HostTestRun.ServerA.Handler,false)) {
				Assert.AreEqual(HttpStatusCode.Forbidden,Send(httpClient,HttpMethod.Post,"http://s3b6-a.example.test/S3b6A/ServiceMain.asmx"));
				Assert.AreEqual(HttpStatusCode.MethodNotAllowed,Send(httpClient,HttpMethod.Get,"https://s3b6-a.example.test/S3b6A/ServiceMain.asmx"));
				Assert.AreEqual(HttpStatusCode.NotFound,Send(httpClient,HttpMethod.Post,"https://s3b6-a.example.test/S3b6A/Other.asmx"));
				Assert.AreEqual(HttpStatusCode.NotFound,Send(httpClient,HttpMethod.Post,"https://s3b6-a.example.test/ServiceMain.asmx"));
				Assert.AreEqual(HttpStatusCode.NotFound,Send(httpClient,HttpMethod.Post,"https://s3b6-a.example.test/S3b6Other/ServiceMain.asmx"));
			}
			//The page name in any case, and the path base alone, reach the endpoint.
			foreach(string url in new[] { "https://s3b6-a.example.test/S3b6A/servicemain.ASMX","https://s3b6-a.example.test/S3b6A/",
				"https://s3b6-a.example.test/s3b6a" })
			{
				RemotingClient.ServerURI=url;
				Assert.AreEqual("Processed: "+url,WebServiceTests.GetString(url),url);
			}
		}
		#endregion The server's checks

		#region The connection
		///<summary>A connection that is refused is HttpRequestError.ConnectionError, which the client takes for a lost connection; with the automatic
		///retry on, the call waits, the program is told the connection is lost, and the call goes through once the server is back.</summary>
		[TestMethod]
		public void ConnectionRefused_IsALostConnection_AndTheCallGoesThroughWhenTheServerIsBack() {
			RoutingHandler routingHandler=HostTestRun.RoutingHandlerCur;
			OpenDentalServerHttp openDentalServerHttp=new OpenDentalServerHttp(new Uri(routingHandler.ClosedPortUri,"/OpenDentalServer/ServiceMain.asmx")
				.ToString());
			HttpRequestException httpRequestException=Assert.ThrowsExactly<HttpRequestException>(() => openDentalServerHttp.ProcessRequest("x"));
			Assert.AreEqual(HttpRequestError.ConnectionError,httpRequestException.HttpRequestError,httpRequestException.ToString());
			Assert.IsTrue(OpenDentalServerHttp.IsConnectionFailure(httpRequestException));
			//Without the automatic retry (or for a first log on, hasConnectionLost=false) the error goes to the caller.
			HostTestRun.UseTarget(new HostTarget("the closed port",openDentalServerHttp.Url,null),HostTestRun.UserA);
			Assert.ThrowsExactly<HttpRequestException>(() => WebServiceTests.GetString("x"));
			//The server goes down and comes back: the retry host is server A (or the running server) while up.
			HttpMessageHandler handlerUp=HostTestRun.IsExternal ? null : HostTestRun.ServerA.Handler;
			if(handlerUp==null) {
				handlerUp=new ForwardHandler(new Uri(HostTestRun.TargetA.Url),HostTestRun.RoutingHandlerCur);
			}
			routingHandler.SetRetryHost(handlerUp,true);
			string pathBase=new Uri(HostTestRun.TargetA.Url).AbsolutePath;
			HostTestRun.UseTarget(new HostTarget("the retry host","https://"+RoutingHandler.HOST_RETRY+pathBase,HostTestRun.TargetA.Database),
				HostTestRun.UserA);
			RemotingClient.HasAutomaticConnectionLostRetry=true;
			List<bool> listEvents=new List<bool>();
			ManualResetEventSlim manualResetEventLost=new ManualResetEventSlim(false);
			MiddleTierConnectionEventHandler handler=(e) => {
				lock(listEvents) {
					listEvents.Add(e.IsConnectionRestored);
				}
				if(!e.IsConnectionRestored) {
					manualResetEventLost.Set();
				}
			};
			MiddleTierConnectionEvent.Fired+=handler;
			try {
				string result=null;
				Exception exceptionCall=null;
				Thread thread=new Thread(() => {
					try {
						result=WebServiceTests.GetString("after the outage");
					}
					catch(Exception ex) {
						exceptionCall=ex;
					}
				});
				thread.Start();
				Assert.IsTrue(manualResetEventLost.Wait(TimeSpan.FromSeconds(60)),"The client did not report the lost connection.");
				Assert.IsTrue(RemotingClient.HasMiddleTierConnectionFailed);
				Assert.IsTrue(thread.IsAlive,"The call waits while the server is down.");
				routingHandler.IsRetryHostDown=false;
				Assert.IsTrue(thread.Join(TimeSpan.FromSeconds(60)),"The call did not go through after the server came back.");
				Assert.IsNull(exceptionCall,exceptionCall?.ToString());
				Assert.AreEqual("Processed: after the outage",result);
				Assert.IsFalse(RemotingClient.HasMiddleTierConnectionFailed);
				lock(listEvents) {
					CollectionAssert.AreEqual(new List<bool> { false,true },listEvents,"Lost, then restored.");
				}
			}
			finally {
				MiddleTierConnectionEvent.Fired-=handler;
				RemotingClient.HasAutomaticConnectionLostRetry=false;
				routingHandler.IsRetryHostDown=false;
			}
		}

		[TestMethod]
		public void Payload50MB_ComesBack() {
			StringBuilder stringBuilder=new StringBuilder(50*1024*1024+64);
			while(stringBuilder.Length<50*1024*1024) {
				stringBuilder.Append("S3b6 payload <&> \"'\u00e9 ");
			}
			string text=stringBuilder.ToString();
			Assert.AreEqual("Processed: "+text,WebServiceTests.GetString(text));
		}
		#endregion The connection

		#region Two users on two databases
		///<summary>User A on server A and user B on server B at the same time, many requests each: every answer is for its own user (the server's
		///thread-static Security.CurUser, read on a thread the server starts) and from its own database. Against a running server both users use
		///its one database.</summary>
		[TestMethod]
		public void TwoUsersOnTwoDatabases_AtTheSameTime_EachGetsItsOwn() {
			const int COUNT=40;
			List<string> listProblems=new List<string>();
			Barrier barrier=new Barrier(2);
			Thread[] arrayThreads=new[] {
				new Thread(() => RunUser(HostTestRun.TargetA,HostTestRun.UserA,COUNT,barrier,listProblems)),
				new Thread(() => RunUser(HostTestRun.TargetB,HostTestRun.UserB,COUNT,barrier,listProblems)),
			};
			foreach(Thread thread in arrayThreads) {
				thread.Start();
			}
			foreach(Thread thread in arrayThreads) {
				Assert.IsTrue(thread.Join(TimeSpan.FromMinutes(5)),"A user's thread did not finish.");
			}
			Assert.AreEqual("",string.Join("\r\n",listProblems.Take(20)));
			if(!HostTestRun.IsExternal) {
				Assert.AreNotEqual(HostTestRun.TargetA.Database,HostTestRun.TargetB.Database);
				Assert.AreNotEqual(HostTestRun.ServerA.LoadContextName,HostTestRun.ServerB.LoadContextName);
			}
		}

		private static void RunUser(HostTarget hostTarget,HostUser hostUser,int count,Barrier barrier,List<string> listProblems) {
			try {
				HostTestRun.UseTargetOnThisThread(hostTarget,hostUser);
				barrier.SignalAndWait(TimeSpan.FromMinutes(1));
				for(int i=0;i<count;i++) {
					Userod userodServer=WebServiceTests.GetCurUserFromThread();
					if(userodServer==null || userodServer.UserName!=hostUser.UserName) {
						AddProblem(listProblems,hostUser.UserName+" #"+i+": the server's user was "+(userodServer==null ? "null" : userodServer.UserName));
					}
					string database=MiscData.GetCurrentDatabase();
					if(database!=hostTarget.Database) {
						AddProblem(listProblems,hostUser.UserName+" #"+i+": the database was "+database+", not "+hostTarget.Database);
					}
					string text=hostUser.UserName+" <"+i+"> &";
					string result=WebServiceTests.GetString(text);
					if(result!="Processed: "+text) {
						AddProblem(listProblems,hostUser.UserName+" #"+i+": GetString gave "+result);
					}
				}
			}
			catch(Exception ex) {
				AddProblem(listProblems,hostUser.UserName+": "+ex);
			}
		}

		private static void AddProblem(List<string> listProblems,string problem) {
			lock(listProblems) {
				listProblems.Add(problem);
			}
		}
		#endregion Two users on two databases

		private static HttpStatusCode Send(HttpClient httpClient,HttpMethod httpMethod,string url) {
			using(HttpRequestMessage httpRequestMessage=new HttpRequestMessage(httpMethod,url)) {
				if(httpMethod==HttpMethod.Post) {
					httpRequestMessage.Content=new StringContent("x",new UTF8Encoding(false),"text/plain");
				}
				using(HttpResponseMessage httpResponseMessage=httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult()) {
					return httpResponseMessage.StatusCode;
				}
			}
		}

		///<summary>Against a running server: the retry host's requests go to the server's own address once it is "back".</summary>
		private class ForwardHandler:HttpMessageHandler {
			private readonly Uri _uri;
			private readonly HttpMessageInvoker _httpMessageInvoker;

			public ForwardHandler(Uri uri,HttpMessageHandler handlerInner) {
				_uri=uri;
				_httpMessageInvoker=new HttpMessageInvoker(handlerInner,false);
			}

			protected override System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken) {
				HttpRequestMessage httpRequestMessage=new HttpRequestMessage(request.Method,_uri);
				httpRequestMessage.Content=request.Content;
				foreach(KeyValuePair<string,IEnumerable<string>> header in request.Headers) {
					httpRequestMessage.Headers.TryAddWithoutValidation(header.Key,header.Value);
				}
				return _httpMessageInvoker.SendAsync(httpRequestMessage,cancellationToken);
			}
		}
	}
}
