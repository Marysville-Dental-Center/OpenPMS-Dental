// The helper's QuickBooks Desktop module against a fake of QBFC's session interfaces (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md,
// section 6): for every command, success and failure, the module makes the calls today's code made (QuickBooksToday, the code of
// WpfControlsOD/Bridges/QuickBooks.cs before S3a-13), with the same arguments in the same order, and gives the same results and messages; the
// main scenarios are also pinned call by call. Since S3a-14n every scenario also runs on the program's in-process path
// (WpfControlsOD/Bridges/QuickBooksInProcess.cs, linked, through NativeFirstChooser.QuickBooksDesktop as a 32-bit program chooses), which must
// make the same calls as today's code too. Since N64-B6 the scenarios that connect also run on a 64-bit program's late-bound path
// (WpfControlsOD/Bridges/QuickBooksLateBound.cs, linked, through NativeFirstChooser.QuickBooksDesktopNative64 as a 64-bit program chooses),
// whose dynamic calls reach the same fakes by name (LateBoundQbfc.cs) and must be today's calls too; its own cases (the choice, a failed
// connection and the helper) are in QuickBooksLateBoundTests. No QuickBooks session is opened and no real QBFC object is created.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Interop.QBFC10;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.Bridges;
using OpenPmsDentalHelper32;
using OpenPmsDentalHelper32.QuickBooks;

namespace Helper32Tests.QuickBooks {
	[TestClass]
	public class QuickBooksModuleTests {
		internal const string COMPANY_FILE=@"C:\QuickBooks\Synthetic Company.qbw";
		internal static readonly DateTime _dateDeposit=new DateTime(2026,11,14);

		///<summary>What one run did: the calls on the fakes, and the result or the exception's message.</summary>
		internal sealed class Run {
			public List<string> ListCalls;
			public string Result;
			public string ErrorMessage;
			public int CountSessions;
			///<summary>The late-bound path's argument types (N64-B6); null on the other paths.</summary>
			public LateBoundCallLog CallLog;
		}

		///<summary>A scenario's script: what the fake QBFC objects return. Every new session manager starts from it.</summary>
		internal delegate void Script(QbfcRecorder recorder);

		///<summary>Runs action on today's code, on the module and on the program's in-process path (S3a-14n), and with funcLateBound on a 64-bit
		///program's late-bound path (N64-B6), each with a fresh recorder that the same script drives, and checks that all of them made the same
		///calls and gave the same result, the late-bound path with the automation type of each QBFC 10 parameter. Returns the module's run.
		///</summary>
		private static Run RunBoth(Script script,Func<string> funcToday,Func<QuickBooksModule,string> funcModule,Func<string> funcInProcess,
			Func<string> funcLateBound=null)
		{
			Run runToday=RunToday(script,funcToday);
			Run runModule=RunModule(script,funcModule);
			Run runInProcess=RunInProcess(script,funcInProcess);
			List<Tuple<string,Run>> listRuns=new List<Tuple<string,Run>> { Tuple.Create("module",runModule),Tuple.Create("in-process path",runInProcess) };
			if(funcLateBound!=null) {
				Run runLateBound=RunLateBound(script,funcLateBound);
				CollectionAssert.AreEqual(new string[0],runLateBound.CallLog.ListMismatches,"Late-bound arguments of another type than QBFC 10's.");
				listRuns.Add(Tuple.Create("late-bound path",runLateBound));
			}
			foreach(Tuple<string,Run> tuple in listRuns) {
				CollectionAssert.AreEqual(runToday.ListCalls,tuple.Item2.ListCalls,"The calls differ from today's.\ntoday:\n  "
					+string.Join("\n  ",runToday.ListCalls)+"\n"+tuple.Item1+":\n  "+string.Join("\n  ",tuple.Item2.ListCalls));
				Assert.AreEqual(runToday.Result,tuple.Item2.Result,tuple.Item1);
				Assert.AreEqual(runToday.ErrorMessage,tuple.Item2.ErrorMessage,tuple.Item1);
				Assert.AreEqual(runToday.CountSessions,tuple.Item2.CountSessions,tuple.Item1);
			}
			return runModule;
		}

		///<summary>The in-process path as QuickBooks.cs takes it in a 32-bit program: a new process's chooser (ResetForTests(false)), IsChosen
		///before each call (InProcess, the call below), and the session managers from the recorder.</summary>
		private static Run RunInProcess(Script script,Func<string> func) {
			QbfcRecorder recorder=new QbfcRecorder();
			script(recorder);
			Run run=new Run();
			NativeFirstChooser.QuickBooksDesktop.ResetForTests(false);
			QuickBooksInProcess.ResetForTests();
			Func<QBSessionManager> funcCreateSessionManagerOld=QuickBooksInProcess.FuncCreateSessionManager;
			QuickBooksInProcess.FuncCreateSessionManager=() => { run.CountSessions++; return recorder.Create<QBSessionManager>("session"); };
			try {
				Execute(run,func);
			}
			finally {
				QuickBooksInProcess.FuncCreateSessionManager=funcCreateSessionManagerOld;
				QuickBooksInProcess.ResetForTests();
				NativeFirstChooser.QuickBooksDesktop.ResetForTests();
			}
			run.ListCalls=recorder.ListCalls;
			return run;
		}

		///<summary>The late-bound path as QuickBooks.cs takes it in a 64-bit program (N64-B6): a new process's chooser
		///(ResetForTests(true)), the QBFC 16 ProgID registered as a 64-bit InprocServer32 and activating, the session managers from the recorder
		///reached by name (LateBoundQbfc), and QBFC 10's enum values as the type library's.</summary>
		internal static Run RunLateBound(Script script,Func<string> func) {
			QbfcRecorder recorder=new QbfcRecorder();
			script(recorder);
			Run run=new Run { CallLog=new LateBoundCallLog() };
			using(new LateBoundScope(progId => { run.CountSessions++; return new LateBoundQbfc(recorder.Create<QBSessionManager>("session"),run.CallLog); })) {
				Execute(run,func);
			}
			run.ListCalls=recorder.ListCalls;
			return run;
		}

		///<summary>func on the late-bound path, whose Try method must not send the call to the helper.</summary>
		internal static string LateBound(Func<Tuple<bool,string>> func) {
			Assert.IsTrue(QuickBooksLateBound.Is64BitProcess,"A 64-bit program takes the late-bound path.");
			Tuple<bool,string> tuple=func();
			Assert.IsTrue(tuple.Item1,"The call stays in this process.");
			return tuple.Item2;
		}

		internal static string LateBoundTestConnection() {
			return LateBound(() => Tuple.Create(QuickBooksLateBound.TryTestConnection(COMPANY_FILE,out string result),result));
		}

		internal static string LateBoundAccounts() {
			return LateBound(() => Tuple.Create(QuickBooksLateBound.TryGetListOfAccounts(COMPANY_FILE,out List<string> listAccounts),Join(listAccounts.ToArray())));
		}

		internal static string LateBoundClasses() {
			return LateBound(() => Tuple.Create(QuickBooksLateBound.TryGetListOfClasses(COMPANY_FILE,out List<string> listClasses),Join(listClasses.ToArray())));
		}

		internal static string LateBoundDeposit(string classRef,bool isClassRefsEnabled,double amount=1234.5) {
			return LateBound(() => Tuple.Create(QuickBooksLateBound.TryCreateDeposit(COMPANY_FILE,_dateDeposit,"Checking","Income:Deposits",amount,
				"Deposit 2026-11-14 (synthetic)",classRef,isClassRefsEnabled),""));
		}

		///<summary>func on the in-process path, after QuickBooks.cs's IsChosen, which a 32-bit program answers with this process.</summary>
		private static T InProcess<T>(Func<T> func) {
			Assert.IsTrue(QuickBooksInProcess.IsChosen(),"A 32-bit program runs QBFC 10 in its own process.");
			return func();
		}

		internal static Run RunToday(Script script,Func<string> func) {
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

		internal static void Execute(Run run,Func<string> func) {
			try {
				run.Result=func();
			}
			catch(Exception exception) {
				run.ErrorMessage=exception.GetType().FullName+": "+exception.Message;
			}
		}

		///<summary>The request and response objects of a session, by name: "request" and "response" with its list "responses".</summary>
		internal static void ScriptSession(QbfcRecorder recorder) {
			recorder.On("session.CreateMsgSetRequest",x => recorder.Create<IMsgSetRequest>("request"));
			recorder.On("session.DoRequests",x => recorder.Create<IMsgSetResponse>("response"));
			recorder.On("response.ResponseList",x => recorder.Create<IResponseList>("responses"));
		}

		///<summary>Responses: one per entry, each its status code, its type and its detail (or none).</summary>
		internal static void ScriptResponses(QbfcRecorder recorder,params Tuple<int,ENResponseType,Func<object>>[] arrayResponses) {
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
		internal static object ScriptNames<TList,TRet>(QbfcRecorder recorder,string listName,params string[] arrayNames) where TList:class where TRet:class {
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

		internal static string Join(string[] arrayValues) {
			return string.Join("|",arrayValues);
		}

		#region TestConnection
		[TestMethod]
		public void TestConnection_Success_MakesTodaysCalls() {
			Run run=RunBoth(ScriptSession,() => QuickBooksToday.TestConnection(COMPANY_FILE),
				module => Join(module.Handle(Request(QuickBooksCommands.TEST_CONNECTION,COMPANY_FILE)).Results),
				() => InProcess(() => QuickBooksInProcess.TestConnection(COMPANY_FILE)),LateBoundTestConnection);
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
				module => Join(module.Handle(Request(QuickBooksCommands.TEST_CONNECTION,COMPANY_FILE)).Results),
				() => InProcess(() => QuickBooksInProcess.TestConnection(COMPANY_FILE)));
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
				module => Join(module.Handle(Request(QuickBooksCommands.TEST_CONNECTION,COMPANY_FILE)).Results),
				() => InProcess(() => QuickBooksInProcess.TestConnection(COMPANY_FILE)));
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
				module => Join(module.Handle(Request(QuickBooksCommands.GET_LIST_OF_ACCOUNTS,COMPANY_FILE)).Results),
				() => InProcess(() => Join(QuickBooksInProcess.GetListOfAccounts(COMPANY_FILE).ToArray())),LateBoundAccounts);
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
				module => Join(module.Handle(Request(QuickBooksCommands.GET_LIST_OF_ACCOUNTS,COMPANY_FILE)).Results),
				() => InProcess(() => Join(QuickBooksInProcess.GetListOfAccounts(COMPANY_FILE).ToArray())),LateBoundAccounts);
			Assert.AreEqual("",run.Result);
		}

		[TestMethod]
		public void GetListOfAccounts_DoRequestsFails_ThrowsTodaysException_AfterEndingTheSession() {
			Script script=recorder => {
				ScriptSession(recorder);
				recorder.On("session.DoRequests",x => throw new COMException("Synthetic: QuickBooks closed the connection."));
			};
			Run run=RunBoth(script,() => Join(QuickBooksToday.GetListOfAccounts().ToArray()),
				module => Join(module.Handle(Request(QuickBooksCommands.GET_LIST_OF_ACCOUNTS,COMPANY_FILE)).Results),
				() => InProcess(() => Join(QuickBooksInProcess.GetListOfAccounts(COMPANY_FILE).ToArray())),LateBoundAccounts);
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
				module => Join(module.Handle(Request(QuickBooksCommands.GET_LIST_OF_CLASSES,COMPANY_FILE)).Results),
				() => InProcess(() => Join(QuickBooksInProcess.GetListOfClasses(COMPANY_FILE).ToArray())),LateBoundClasses);
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

		internal static string DepositToday(string classRef,bool isClassRefsEnabled,double amount=1234.5) {
			QuickBooksToday.IsClassRefsEnabled=isClassRefsEnabled;
			QuickBooksToday.CreateDeposit(_dateDeposit,"Checking","Income:Deposits",amount,"Deposit 2026-11-14 (synthetic)",classRef);
			return "";
		}

		///<summary>CreateDeposit on the in-process path, with the two preferences as QuickBooks.cs passes them.</summary>
		private static string DepositInProcess(string classRef,bool isClassRefsEnabled,double amount=1234.5) {
			return InProcess(() => {
				QuickBooksInProcess.CreateDeposit(COMPANY_FILE,_dateDeposit,"Checking","Income:Deposits",amount,"Deposit 2026-11-14 (synthetic)",classRef,
					isClassRefsEnabled);
				return "";
			});
		}

		[TestMethod]
		public void CreateDeposit_WithAClass_MakesTodaysCalls() {
			Script script=recorder => ScriptResponses(recorder,Tuple.Create(0,ENResponseType.rtDepositAddRs,(Func<object>)null));
			Run run=RunBoth(script,() => DepositToday("Clinic East",true),module => Deposit(module,"Clinic East",true),
				() => DepositInProcess("Clinic East",true),() => LateBoundDeposit("Clinic East",true));
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
					module => Deposit(module,tuple.Item1,!string.IsNullOrEmpty(tuple.Item1) && tuple.Item2),
					() => DepositInProcess(tuple.Item1,tuple.Item2),() => LateBoundDeposit(tuple.Item1,tuple.Item2));
				Assert.IsFalse(run.ListCalls.Any(x => x.Contains("ClassRef")),string.Join("\n",run.ListCalls));
			}
		}

		[TestMethod]
		public void CreateDeposit_AmountIsTheProgramsText() {
			Script script=recorder => ScriptResponses(recorder,Tuple.Create(0,ENResponseType.rtDepositAddRs,(Func<object>)null));
			foreach(double amount in new[] { 0.01,10,99999.995,1234567.891 }) {
				Run run=RunBoth(script,() => DepositToday("",false,amount),module => Deposit(module,"",false,amount),
					() => DepositInProcess("",false,amount),() => LateBoundDeposit("",false,amount));
				CollectionAssert.Contains(run.ListCalls,"request.AppendDepositAddRq().DepositLineAddList.Append().ORDepositLineAdd.DepositInfo.Amount.SetAsString(\""
					+amount.ToString("F")+"\")");
			}
		}

		[TestMethod]
		public void CreateDeposit_ResponseWithAStatusAboveZero_ThrowsItsMessage() {
			Script script=recorder => ScriptResponses(recorder,Tuple.Create(0,ENResponseType.rtDepositAddRs,(Func<object>)null),
				Tuple.Create(3120,ENResponseType.rtDepositAddRs,(Func<object>)null));
			Run run=RunBoth(script,() => DepositToday("",false),module => Deposit(module,"",false),() => DepositInProcess("",false),
				() => LateBoundDeposit("",false));
			Assert.AreEqual("System.Exception: Synthetic status message 3120",run.ErrorMessage);
			Assert.AreEqual("responses[1].StatusMessage",run.ListCalls.Last());
		}

		[TestMethod]
		public void CreateDeposit_BeginSessionFails_ThrowsAfterClosingTheConnection() {
			Script script=recorder => {
				ScriptSession(recorder);
				recorder.On("session.BeginSession",x => throw new COMException("Synthetic: another user has the company file open in single-user mode."));
			};
			Run run=RunBoth(script,() => DepositToday("Clinic East",true),module => Deposit(module,"Clinic East",true),
				() => DepositInProcess("Clinic East",true));
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
			},() => {
				string first=InProcess(() => QuickBooksInProcess.TestConnection(COMPANY_FILE));
				try {
					InProcess(() => QuickBooksInProcess.GetListOfClasses(COMPANY_FILE));
				}
				catch(Exception exception) {
					return first+" / "+exception.Message;
				}
				return first;
			},() => {
				string first=LateBoundTestConnection();
				try {
					LateBoundClasses();
				}
				catch(Exception exception) {
					return first+" / "+exception.Message;
				}
				return first;
			});
		}

		///<summary>S3a-14n: the in-process path in a 64-bit program, where QBFC 10 (a 32-bit in-process server) is not registered: IsChosen
		///activates once, gets REGDB_E_CLASSNOTREG, chooses the helper (false) and makes no QBFC call; later calls do not activate again. In a 32-bit
		///program the same failure keeps this process (IsChosen true), and the method reports it as before S3a-13.</summary>
		[TestMethod]
		public void InProcess_QbfcNotRegisteredForThisBitness_TheHelperIn64Bit_TheOldMessageIn32Bit() {
			int countActivations=0;
			Func<QBSessionManager> funcCreateSessionManagerOld=QuickBooksInProcess.FuncCreateSessionManager;
			QuickBooksInProcess.FuncCreateSessionManager=() => {
				countActivations++;
				throw new COMException("Synthetic: class not registered",NativeFirstChooser.REGDB_E_CLASSNOTREG);
			};
			try {
				NativeFirstChooser.QuickBooksDesktop.ResetForTests(true);
				QuickBooksInProcess.ResetForTests();
				Assert.IsFalse(QuickBooksInProcess.IsChosen());
				Assert.IsFalse(QuickBooksInProcess.IsChosen());
				Assert.AreEqual(1,countActivations,"Activated once per process.");
				Assert.AreEqual(VendorPath.Helper32,NativeFirstChooser.QuickBooksDesktop.Choice);
				NativeFirstChooser.QuickBooksDesktop.ResetForTests(false);
				QuickBooksInProcess.ResetForTests();
				Assert.IsTrue(QuickBooksInProcess.IsChosen());
				Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.QuickBooksDesktop.Choice);
				Assert.AreEqual("Error: Synthetic: class not registered",QuickBooksInProcess.TestConnection(COMPANY_FILE),
					"As before S3a-13: new QBSessionManager() failed inside TestConnection's try.");
				Assert.AreEqual(3,countActivations);
			}
			finally {
				QuickBooksInProcess.FuncCreateSessionManager=funcCreateSessionManagerOld;
				QuickBooksInProcess.ResetForTests();
				NativeFirstChooser.QuickBooksDesktop.ResetForTests();
			}
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
