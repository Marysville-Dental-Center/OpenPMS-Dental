// The helper's QuickBooks Desktop module against a fake of QBFC's session interfaces (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md,
// section 6): for every command, success and failure, the module makes the calls today's code made (QuickBooksToday, the code of
// WpfControlsOD/Bridges/QuickBooks.cs before S3a-13), with the same arguments in the same order, and gives the same results and messages; the
// main scenarios are also pinned call by call. No QuickBooks session is opened and no real QBFC object is created.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Interop.QBFC10;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalHelper32;
using OpenPmsDentalHelper32.QuickBooks;

namespace Helper32Tests.QuickBooks {
	[TestClass]
	public class QuickBooksModuleTests {
		private const string COMPANY_FILE=@"C:\QuickBooks\Synthetic Company.qbw";
		private static readonly DateTime _dateDeposit=new DateTime(2026,11,14);

		///<summary>What one run did: the calls on the fakes, and the result or the exception's message.</summary>
		private sealed class Run {
			public List<string> ListCalls;
			public string Result;
			public string ErrorMessage;
			public int CountSessions;
		}

		///<summary>A scenario's script: what the fake QBFC objects return. Every new session manager starts from it.</summary>
		private delegate void Script(QbfcRecorder recorder);

		///<summary>Runs action on today's code and on the module, each with a fresh recorder that the same script drives, and checks that both made
		///the same calls and gave the same result. Returns the module's run.</summary>
		private static Run RunBoth(Script script,Func<string> funcToday,Func<QuickBooksModule,string> funcModule) {
			Run runToday=RunToday(script,funcToday);
			Run runModule=RunModule(script,funcModule);
			CollectionAssert.AreEqual(runToday.ListCalls,runModule.ListCalls,"The calls differ from today's.\ntoday:\n  "
				+string.Join("\n  ",runToday.ListCalls)+"\nmodule:\n  "+string.Join("\n  ",runModule.ListCalls));
			Assert.AreEqual(runToday.Result,runModule.Result);
			Assert.AreEqual(runToday.ErrorMessage,runModule.ErrorMessage);
			Assert.AreEqual(runToday.CountSessions,runModule.CountSessions);
			return runModule;
		}

		private static Run RunToday(Script script,Func<string> func) {
			QbfcRecorder recorder=new QbfcRecorder();
			script(recorder);
			Run run=new Run();
			QuickBooksToday.Reset();
			QuickBooksToday.CompanyFile=COMPANY_FILE;
			QuickBooksToday.FuncCreateSessionManager=() => { run.CountSessions++; return recorder.Create<QBSessionManager>("session"); };
			Execute(run,func);
			run.ListCalls=recorder.ListCalls;
			return run;
		}

		private static Run RunModule(Script script,Func<QuickBooksModule,string> func) {
			QbfcRecorder recorder=new QbfcRecorder();
			script(recorder);
			Run run=new Run();
			QuickBooksModule module=new QuickBooksModule(() => { run.CountSessions++; return recorder.Create<QBSessionManager>("session"); });
			Execute(run,() => func(module));
			run.ListCalls=recorder.ListCalls;
			return run;
		}

		private static void Execute(Run run,Func<string> func) {
			try {
				run.Result=func();
			}
			catch(Exception exception) {
				run.ErrorMessage=exception.GetType().FullName+": "+exception.Message;
			}
		}

		///<summary>The request and response objects of a session, by name: "request" and "response" with its list "responses".</summary>
		private static void ScriptSession(QbfcRecorder recorder) {
			recorder.On("session.CreateMsgSetRequest",x => recorder.Create<IMsgSetRequest>("request"));
			recorder.On("session.DoRequests",x => recorder.Create<IMsgSetResponse>("response"));
			recorder.On("response.ResponseList",x => recorder.Create<IResponseList>("responses"));
		}

		///<summary>Responses: one per entry, each its status code, its type and its detail (or none).</summary>
		private static void ScriptResponses(QbfcRecorder recorder,params Tuple<int,ENResponseType,Func<object>>[] arrayResponses) {
			ScriptSession(recorder);
			recorder.On("responses.Count",x => arrayResponses.Length);
			recorder.On("responses.GetAt",x => recorder.Create<IResponse>("responses["+x[0]+"]"));
			for(int i=0;i<arrayResponses.Length;i++) {
				Tuple<int,ENResponseType,Func<object>> tuple=arrayResponses[i];
				string name="responses["+i+"]";
				recorder.On(name+".StatusCode",x => tuple.Item1);
				recorder.On(name+".StatusMessage",x => "Synthetic status message "+tuple.Item1);
				recorder.On(name+".Type",x => recorder.Create<IResponseType>(name+".Type"));
				recorder.On(name+".Type.GetValue",x => (short)tuple.Item2);
				recorder.On(name+".Detail",x => tuple.Item3==null ? null : tuple.Item3());
			}
		}

		///<summary>A list of names (accounts or classes) as a fake ret list: null for an entry without FullName.</summary>
		private static object ScriptNames<TList,TRet>(QbfcRecorder recorder,string listName,params string[] arrayNames) where TList:class where TRet:class {
			recorder.On(listName+".Count",x => arrayNames.Length);
			recorder.On(listName+".GetAt",x => recorder.Create<TRet>(listName+"["+x[0]+"]"));
			for(int i=0;i<arrayNames.Length;i++) {
				string nameItem=listName+"["+i+"]";
				string value=arrayNames[i];
				recorder.On(nameItem+".FullName",x => value==null ? null : recorder.Create<IQBStringType>(nameItem+".FullName"));
				recorder.On(nameItem+".FullName.GetValue",x => value);
			}
			return recorder.Create<TList>(listName);
		}

		private static Helper32Request Request(string command,params string[] args) {
			return new Helper32Request { Module=QuickBooksCommands.MODULE,Command=command,Args=args };
		}

		private static string Join(string[] arrayValues) {
			return string.Join("|",arrayValues);
		}

		#region TestConnection
		[TestMethod]
		public void TestConnection_Success_MakesTodaysCalls() {
			Run run=RunBoth(ScriptSession,() => QuickBooksToday.TestConnection(COMPANY_FILE),
				module => Join(module.Handle(Request(QuickBooksCommands.TEST_CONNECTION,COMPANY_FILE)).Results));
			Assert.AreEqual("Connection to QuickBooks was successful.",run.Result);
			CollectionAssert.AreEqual(new[] {
				"session.CreateMsgSetRequest(\"US\",1,0)",
				"request.Attributes.OnError=roeContinue",
				"session.OpenConnection(\"\",\"Open Dental\")",
				"session.BeginSession(\""+COMPANY_FILE+"\",omDontCare)",
				"session.DoRequests(request)",
				"session.EndSession()",
				"session.CloseConnection()",
			},run.ListCalls);
		}

		[TestMethod]
		public void TestConnection_BeginSessionFails_ReturnsTodaysMessage() {
			Script script=recorder => {
				ScriptSession(recorder);
				recorder.On("session.BeginSession",x => throw new COMException("Synthetic: the company file could not be opened.",unchecked((int)0x80040408)));
			};
			Run run=RunBoth(script,() => QuickBooksToday.TestConnection(COMPANY_FILE),
				module => Join(module.Handle(Request(QuickBooksCommands.TEST_CONNECTION,COMPANY_FILE)).Results));
			Assert.AreEqual("Error: Synthetic: the company file could not be opened.",run.Result);
			Assert.AreEqual("session.CloseConnection()",run.ListCalls.Last(),"The connection was open, the session not.");
			Assert.IsFalse(run.ListCalls.Contains("session.EndSession()"));
		}

		[TestMethod]
		public void TestConnection_OpenConnectionFails_ClosesNothing() {
			Script script=recorder => {
				ScriptSession(recorder);
				recorder.On("session.OpenConnection",x => throw new COMException("Synthetic: QuickBooks is not installed."));
			};
			Run run=RunBoth(script,() => QuickBooksToday.TestConnection(COMPANY_FILE),
				module => Join(module.Handle(Request(QuickBooksCommands.TEST_CONNECTION,COMPANY_FILE)).Results));
			Assert.AreEqual("Error: Synthetic: QuickBooks is not installed.",run.Result);
			Assert.AreEqual("session.OpenConnection(\"\",\"Open Dental\")",run.ListCalls.Last());
		}
		#endregion TestConnection

		#region GetListOfAccounts and GetListOfClasses
		[TestMethod]
		public void GetListOfAccounts_ReturnsTheAccountsOfTheAccountResponses() {
			Script script=recorder => ScriptResponses(recorder,
				Tuple.Create(0,ENResponseType.rtAccountQueryRs,(Func<object>)(() => ScriptNames<IAccountRetList,IAccountRet>(recorder,"accounts","Checking",
					null,"Income:Patient Payments"))),
				Tuple.Create(1,ENResponseType.rtAccountQueryRs,(Func<object>)(() => ScriptNames<IAccountRetList,IAccountRet>(recorder,"accountsWarning",
					"Savings"))),
				Tuple.Create(-1,ENResponseType.rtAccountQueryRs,(Func<object>)(() => ScriptNames<IAccountRetList,IAccountRet>(recorder,"accountsError",
					"Not this one"))),
				Tuple.Create(0,ENResponseType.rtClassQueryRs,(Func<object>)(() => ScriptNames<IClassRetList,IClassRet>(recorder,"classes","Not a class"))),
				Tuple.Create(0,ENResponseType.rtAccountQueryRs,(Func<object>)null));
			Run run=RunBoth(script,() => Join(QuickBooksToday.GetListOfAccounts().ToArray()),
				module => Join(module.Handle(Request(QuickBooksCommands.GET_LIST_OF_ACCOUNTS,COMPANY_FILE)).Results));
			Assert.AreEqual("Checking|Income:Patient Payments|Savings",run.Result,"Status 0 and warnings (>0), account responses only.");
			CollectionAssert.AreEqual(new[] {
				"session.CreateMsgSetRequest(\"US\",8,0)",
				"request.Attributes.OnError=roeContinue",
				"session.OpenConnection(\"\",\"Open Dental\")",
				"session.BeginSession(\""+COMPANY_FILE+"\",omDontCare)",
				"request.AppendAccountQueryRq()",
				"request.AppendAccountQueryRq().ORAccountListQuery.AccountListFilter.ActiveStatus.SetValue(asActiveOnly)",
				"session.DoRequests(request)",
				"session.EndSession()",
				"session.CloseConnection()",
			},run.ListCalls.Take(9).ToArray());
		}

		[TestMethod]
		public void GetListOfAccounts_NoResponseList_IsEmpty() {
			Script script=recorder => {
				ScriptSession(recorder);
				recorder.On("response.ResponseList",x => null);
			};
			Run run=RunBoth(script,() => Join(QuickBooksToday.GetListOfAccounts().ToArray()),
				module => Join(module.Handle(Request(QuickBooksCommands.GET_LIST_OF_ACCOUNTS,COMPANY_FILE)).Results));
			Assert.AreEqual("",run.Result);
		}

		[TestMethod]
		public void GetListOfAccounts_DoRequestsFails_ThrowsTodaysException_AfterEndingTheSession() {
			Script script=recorder => {
				ScriptSession(recorder);
				recorder.On("session.DoRequests",x => throw new COMException("Synthetic: QuickBooks closed the connection."));
			};
			Run run=RunBoth(script,() => Join(QuickBooksToday.GetListOfAccounts().ToArray()),
				module => Join(module.Handle(Request(QuickBooksCommands.GET_LIST_OF_ACCOUNTS,COMPANY_FILE)).Results));
			Assert.AreEqual(typeof(COMException).FullName+": Synthetic: QuickBooks closed the connection.",run.ErrorMessage);
			CollectionAssert.AreEqual(new[] { "session.DoRequests(request)","session.EndSession()","session.CloseConnection()" },
				run.ListCalls.Skip(run.ListCalls.Count-3).ToArray());
		}

		[TestMethod]
		public void GetListOfClasses_ReturnsTheClassesOfTheClassResponses() {
			Script script=recorder => ScriptResponses(recorder,
				Tuple.Create(0,ENResponseType.rtClassQueryRs,(Func<object>)(() => ScriptNames<IClassRetList,IClassRet>(recorder,"classes","Clinic East",
					"Clinic West",null))),
				Tuple.Create(0,ENResponseType.rtAccountQueryRs,(Func<object>)(() => ScriptNames<IAccountRetList,IAccountRet>(recorder,"accounts","Checking"))));
			Run run=RunBoth(script,() => Join(QuickBooksToday.GetListOfClasses().ToArray()),
				module => Join(module.Handle(Request(QuickBooksCommands.GET_LIST_OF_CLASSES,COMPANY_FILE)).Results));
			Assert.AreEqual("Clinic East|Clinic West",run.Result);
			CollectionAssert.AreEqual(new[] {
				"request.AppendClassQueryRq()",
				"request.AppendClassQueryRq().ORListQuery.ListFilter.ActiveStatus.SetValue(asActiveOnly)",
			},run.ListCalls.Skip(4).Take(2).ToArray());
			StringAssert.StartsWith(run.ListCalls[0],"session.CreateMsgSetRequest(\"US\",8,0)");
		}
		#endregion GetListOfAccounts and GetListOfClasses

		#region CreateDeposit
		private static string Deposit(QuickBooksModule module,string classRef,bool isClassRefUsed,double amount=1234.5) {
			module.Handle(Request(QuickBooksCommands.CREATE_DEPOSIT,COMPANY_FILE,QuickBooksCommands.DateToText(_dateDeposit),"Checking","Income:Deposits",
				amount.ToString("F"),"Deposit 2026-11-14 (synthetic)",classRef,QuickBooksCommands.BoolToText(isClassRefUsed)));
			return "";
		}

		private static string DepositToday(string classRef,bool isClassRefsEnabled,double amount=1234.5) {
			QuickBooksToday.IsClassRefsEnabled=isClassRefsEnabled;
			QuickBooksToday.CreateDeposit(_dateDeposit,"Checking","Income:Deposits",amount,"Deposit 2026-11-14 (synthetic)",classRef);
			return "";
		}

		[TestMethod]
		public void CreateDeposit_WithAClass_MakesTodaysCalls() {
			Script script=recorder => ScriptResponses(recorder,Tuple.Create(0,ENResponseType.rtDepositAddRs,(Func<object>)null));
			Run run=RunBoth(script,() => DepositToday("Clinic East",true),module => Deposit(module,"Clinic East",true));
			Assert.IsNull(run.ErrorMessage);
			const string line="request.AppendDepositAddRq().DepositLineAddList.Append().ORDepositLineAdd.DepositInfo.";
			CollectionAssert.AreEqual(new[] {
				"session.CreateMsgSetRequest(\"US\",9,0)",
				"request.Attributes.OnError=roeContinue",
				"session.OpenConnection(\"\",\"Open Dental\")",
				"session.BeginSession(\""+COMPANY_FILE+"\",omDontCare)",
				"request.AppendDepositAddRq()",
				"request.AppendDepositAddRq().TxnDate.SetValue("+_dateDeposit.ToString("o",CultureInfo.InvariantCulture)+")",
				"request.AppendDepositAddRq().DepositToAccountRef.FullName.SetValue(\"Checking\")",
				"request.AppendDepositAddRq().Memo.SetValue(\"Deposit 2026-11-14 (synthetic)\")",
				"request.AppendDepositAddRq().DepositLineAddList.Append()",
				line+"AccountRef.FullName.SetValue(\"Income:Deposits\")",
				line+"Amount.SetAsString(\""+1234.5.ToString("F")+"\")",
				line+"ClassRef.FullName.SetValue(\"Clinic East\")",
				"session.DoRequests(request)",
				"session.EndSession()",
				"session.CloseConnection()",
				"responses.Count",
				"responses.GetAt(0)",
				"responses[0].StatusCode",
				"responses.Count",
			},run.ListCalls);
		}

		[TestMethod]
		public void CreateDeposit_WithoutAClass_OrClassesOff_SetsNoClass() {
			Script script=recorder => ScriptResponses(recorder,Tuple.Create(0,ENResponseType.rtDepositAddRs,(Func<object>)null));
			foreach(Tuple<string,bool> tuple in new[] { Tuple.Create("",true),Tuple.Create((string)null,true),Tuple.Create("Clinic East",false) }) {
				Run run=RunBoth(script,() => DepositToday(tuple.Item1,tuple.Item2),
					module => Deposit(module,tuple.Item1,!string.IsNullOrEmpty(tuple.Item1) && tuple.Item2));
				Assert.IsFalse(run.ListCalls.Any(x => x.Contains("ClassRef")),string.Join("\n",run.ListCalls));
			}
		}

		[TestMethod]
		public void CreateDeposit_AmountIsTheProgramsText() {
			Script script=recorder => ScriptResponses(recorder,Tuple.Create(0,ENResponseType.rtDepositAddRs,(Func<object>)null));
			foreach(double amount in new[] { 0.01,10,99999.995,1234567.891 }) {
				Run run=RunBoth(script,() => DepositToday("",false,amount),module => Deposit(module,"",false,amount));
				CollectionAssert.Contains(run.ListCalls,"request.AppendDepositAddRq().DepositLineAddList.Append().ORDepositLineAdd.DepositInfo.Amount.SetAsString(\""
					+amount.ToString("F")+"\")");
			}
		}

		[TestMethod]
		public void CreateDeposit_ResponseWithAStatusAboveZero_ThrowsItsMessage() {
			Script script=recorder => ScriptResponses(recorder,Tuple.Create(0,ENResponseType.rtDepositAddRs,(Func<object>)null),
				Tuple.Create(3120,ENResponseType.rtDepositAddRs,(Func<object>)null));
			Run run=RunBoth(script,() => DepositToday("",false),module => Deposit(module,"",false));
			Assert.AreEqual("System.Exception: Synthetic status message 3120",run.ErrorMessage);
			Assert.AreEqual("responses[1].StatusMessage",run.ListCalls.Last());
		}

		[TestMethod]
		public void CreateDeposit_BeginSessionFails_ThrowsAfterClosingTheConnection() {
			Script script=recorder => {
				ScriptSession(recorder);
				recorder.On("session.BeginSession",x => throw new COMException("Synthetic: another user has the company file open in single-user mode."));
			};
			Run run=RunBoth(script,() => DepositToday("Clinic East",true),module => Deposit(module,"Clinic East",true));
			Assert.AreEqual(typeof(COMException).FullName+": Synthetic: another user has the company file open in single-user mode.",run.ErrorMessage);
			Assert.AreEqual("session.CloseConnection()",run.ListCalls.Last());
		}
		#endregion CreateDeposit

		///<summary>Today's code kept its state between calls (static fields; the module's live as long as its helper, one per program process): a
		///failed TestConnection leaves its flags set, so the next call's failure also ends a session on the new session manager. The module does
		///the same.</summary>
		[TestMethod]
		public void StateBetweenCalls_IsKeptAsToday() {
			Script script=recorder => {
				ScriptSession(recorder);
				recorder.On("session.DoRequests",x => throw new COMException("Synthetic: request failed."));
			};
			RunBoth(script,() => {
				string first=QuickBooksToday.TestConnection(COMPANY_FILE);
				try {
					QuickBooksToday.GetListOfClasses();
				}
				catch(Exception exception) {
					return first+" / "+exception.Message;
				}
				return first;
			},module => {
				string first=module.Handle(Request(QuickBooksCommands.TEST_CONNECTION,COMPANY_FILE)).Results[0];
				try {
					module.Handle(Request(QuickBooksCommands.GET_LIST_OF_CLASSES,COMPANY_FILE));
				}
				catch(Exception exception) {
					return first+" / "+exception.Message;
				}
				return first;
			});
		}

		[TestMethod]
		public void Handle_RefusesAWrongArgumentCount_AndAnUnknownCommand() {
			QbfcRecorder recorder=new QbfcRecorder();
			QuickBooksModule module=new QuickBooksModule(() => recorder.Create<QBSessionManager>("session"));
			Assert.AreEqual(QuickBooksCommands.MODULE,module.Name);
			Assert.ThrowsExactly<ArgumentException>(() => module.Handle(Request(QuickBooksCommands.TEST_CONNECTION)));
			Assert.ThrowsExactly<ArgumentException>(() => module.Handle(Request(QuickBooksCommands.CREATE_DEPOSIT,COMPANY_FILE)));
			Assert.ThrowsExactly<InvalidOperationException>(() => module.Handle(Request("DeleteCompany",COMPANY_FILE)));
			Assert.AreEqual(0,recorder.ListCalls.Count,"Nothing reached QBFC.");
		}

		[TestMethod]
		public void CommandTexts_RoundTrip() {
			foreach(DateTime dateTime in new[] { _dateDeposit,new DateTime(2026,11,14,13,5,7,DateTimeKind.Local),new DateTime(2000,1,1,0,0,0,DateTimeKind.Utc),
				DateTime.MinValue })
			{
				DateTime dateTimeRead=QuickBooksCommands.DateFromText(QuickBooksCommands.DateToText(dateTime));
				Assert.AreEqual(dateTime,dateTimeRead);
				Assert.AreEqual(dateTime.Kind,dateTimeRead.Kind);
			}
			Assert.IsTrue(QuickBooksCommands.BoolFromText(QuickBooksCommands.BoolToText(true)));
			Assert.IsFalse(QuickBooksCommands.BoolFromText(QuickBooksCommands.BoolToText(false)));
		}
	}
}
