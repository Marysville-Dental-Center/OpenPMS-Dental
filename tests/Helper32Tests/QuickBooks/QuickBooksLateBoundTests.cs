// A 64-bit program's late-bound QBFC path (N64-B6, docs/port-notes/N64-B6.md; WpfControlsOD/Bridges/QuickBooksLateBound.cs, linked): the
// choice of the newest registered QBFC session manager that activates (NativeFirstChooser.QuickBooksDesktopNative64, against a fake registry
// and fake activations), each activation HRESULT moving to the next older one, none moving the program to the helper, a failed connection
// moving it to the helper once (nothing reached QuickBooks, so the call is made there again), a failure after the connection staying here as
// before, the enum values taken from the activated QBFC's type library, and the argument types the dynamic calls pass. The scenarios that
// connect are compared call for call with today's code and the QBFC 10 path in QuickBooksModuleTests (RunBoth). No QBFC object is created,
// no QuickBooks session is opened and no helper is started: the session managers are the recording fakes reached by name (LateBoundQbfc).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using CodeBase;
using Interop.QBFC10;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.Bridges;

namespace Helper32Tests.QuickBooks {
	///<summary>QuickBooksLateBound's seams for one test, as a new program process of that bitness has them: a new chooser, the ProgIDs (default:
	///the program's list), a registry (default: every ProgID a 64-bit InprocServer32), the activation and QBFC 10's enum values as the type
	///library's. Records the registry reads, the activations and the enum reads; puts everything back at the end.</summary>
	internal sealed class LateBoundScope:IDisposable {
		private readonly string[] _arrayProgIdsOld=QuickBooksLateBound.ArrayProgIds;
		private readonly Func<string,object> _funcCreateOld=QuickBooksLateBound.FuncCreateComObject;
		private readonly Func<string,ComServerRegistration> _funcReadRegistrationOld=QuickBooksLateBound.FuncReadRegistration;
		private readonly Func<object,IList<string>,Dictionary<string,int>> _funcReadEnumValuesOld=QuickBooksLateBound.FuncReadEnumValues;
		public readonly List<string> ListRegistrationReads=new List<string>();
		public readonly List<string> ListActivations=new List<string>();
		public readonly List<string> ListEnumReads=new List<string>();

		public LateBoundScope(Func<string,object> funcCreate,bool is64BitProcess=true,string[] arrayProgIds=null,
			Func<string,ComServerRegistration> funcReadRegistration=null,Func<IList<string>,Dictionary<string,int>> funcReadEnumValues=null)
		{
			NativeFirstChooser.QuickBooksDesktopNative64.ResetForTests(is64BitProcess);
			QuickBooksLateBound.ResetForTests();
			if(arrayProgIds!=null) {
				QuickBooksLateBound.ArrayProgIds=arrayProgIds;
			}
			funcReadRegistration=funcReadRegistration??(progId => RegisteredInproc(progId,true));
			funcReadEnumValues=funcReadEnumValues??Qbfc10EnumValues;
			QuickBooksLateBound.FuncReadRegistration=progId => {
				ListRegistrationReads.Add(progId);
				return funcReadRegistration(progId);
			};
			QuickBooksLateBound.FuncCreateComObject=progId => {
				ListActivations.Add(progId);
				return funcCreate(progId);
			};
			QuickBooksLateBound.FuncReadEnumValues=(comObject,listNames) => {
				ListEnumReads.Add(comObject+": "+string.Join(",",listNames));
				return funcReadEnumValues(listNames);
			};
		}

		public void Dispose() {
			QuickBooksLateBound.ArrayProgIds=_arrayProgIdsOld;
			QuickBooksLateBound.FuncCreateComObject=_funcCreateOld;
			QuickBooksLateBound.FuncReadRegistration=_funcReadRegistrationOld;
			QuickBooksLateBound.FuncReadEnumValues=_funcReadEnumValuesOld;
			QuickBooksLateBound.ResetForTests();
			NativeFirstChooser.QuickBooksDesktopNative64.ResetForTests();
		}

		///<summary>progId's class as an InprocServer32 of one view (a synthetic class id).</summary>
		public static ComServerRegistration RegisteredInproc(string progId,bool isView64) {
			return new ComServerRegistration { ProgId=progId,Clsid="{3B0E7C1A-5D2F-4E8B-9C61-0A4F2D7E8B15}",HasInprocServer32View64=isView64,
				HasInprocServer32View32=!isView64 };
		}

		///<summary>The values QBFC 10's interop gives these enum constants.</summary>
		public static Dictionary<string,int> Qbfc10EnumValues(IList<string> listNames) {
			Dictionary<string,int> dictValues=new Dictionary<string,int>();
			foreach(Type typeEnum in typeof(QBSessionManager).Assembly.GetTypes().Where(x => x.IsEnum)) {
				foreach(string name in listNames) {
					FieldInfo fieldInfo=typeEnum.GetField(name,BindingFlags.Public|BindingFlags.Static);
					if(fieldInfo!=null) {
						dictValues[name]=Convert.ToInt32(fieldInfo.GetRawConstantValue());
					}
				}
			}
			return dictValues;
		}
	}

	[TestClass]
	public class QuickBooksLateBoundTests {
		private const string COMPANY_FILE=QuickBooksModuleTests.COMPANY_FILE;
		private const string PROG_ID_18="Synthetic.QBFC18.SessionManager";
		private const string PROG_ID_17="Synthetic.QBFC17.SessionManager";
		private const string PROG_ID_16="Synthetic.QBFC16.SessionManager";
		private static readonly string[] _arrayProgIdsSynthetic=new[] { PROG_ID_18,PROG_ID_17,PROG_ID_16 };
		private const int E_FAIL=unchecked((int)0x80004005);

		///<summary>A recorder with a working session (QuickBooksModuleTests.ScriptSession) and extra script.</summary>
		private static QbfcRecorder NewRecorder(Action<QbfcRecorder> actionScript=null) {
			QbfcRecorder recorder=new QbfcRecorder();
			QuickBooksModuleTests.ScriptSession(recorder);
			actionScript?.Invoke(recorder);
			return recorder;
		}

		private static Func<string,object> Sessions(QbfcRecorder recorder,LateBoundCallLog callLog=null) {
			callLog=callLog??new LateBoundCallLog();
			return progId => new LateBoundQbfc(recorder.Create<QBSessionManager>("session"),callLog);
		}

		private static string TestConnection(out bool isHere) {
			isHere=QuickBooksLateBound.TryTestConnection(COMPANY_FILE,out string result);
			return result;
		}

		private static string GetRoot() {
			DirectoryInfo directoryInfo=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while(directoryInfo!=null && !File.Exists(Path.Combine(directoryInfo.FullName,"OpenDental.sln"))) {
				directoryInfo=directoryInfo.Parent;
			}
			Assert.IsNotNull(directoryInfo,"The repository root (OpenDental.sln) above the test folder.");
			return directoryInfo.FullName;
		}

		[TestMethod]
		public void ProgIds_AreTheSourcedOnes_NewestFirst_EachInThePortNote() {
			CollectionAssert.AreEqual(new[] { "QBFC16.QBSessionManager.1" },QuickBooksLateBound.ArrayProgIds,
				"Only ProgIDs a public page gives (N64-B6.md, section 2); QBFC 15, 17 and 18 have none.");
			string textNote=File.ReadAllText(Path.Combine(GetRoot(),@"docs\port-notes\N64-B6.md"));
			foreach(string progId in QuickBooksLateBound.ArrayProgIds) {
				StringAssert.Contains(textNote,"`"+progId+"`");
			}
			CollectionAssert.AreEqual(new[] { "roeContinue","omDontCare","asActiveOnly","rtAccountQueryRs","rtClassQueryRs" },QuickBooksLateBound.ArrayEnumNames);
		}

		[TestMethod]
		public void ThirtyTwoBitProgram_DoesNotTakeThePath_AndAFailureMovesNothing() {
			using(new LateBoundScope(progId => throw new InvalidOperationException("not activated in a 32-bit program"),is64BitProcess:false)) {
				Assert.IsFalse(QuickBooksLateBound.Is64BitProcess,"QuickBooks.cs keeps QBFC 10 in process (QuickBooksInProcess) in a 32-bit program.");
				Assert.IsFalse(NativeFirstChooser.QuickBooksDesktopNative64.ChooseHelperAfterConnectionFailure(new COMException("Synthetic",E_FAIL)));
				Assert.IsNull(NativeFirstChooser.QuickBooksDesktopNative64.Choice);
			}
			using(new LateBoundScope(progId => throw new InvalidOperationException("not activated"),is64BitProcess:true)) {
				Assert.IsTrue(QuickBooksLateBound.Is64BitProcess);
			}
		}

		[TestMethod]
		public void Choice_TheNewestRegisteredForThisBitness_Activates_OnceLogged_ThenItsProgIdDirectly() {
			QbfcRecorder recorder=NewRecorder();
			Func<string,ComServerRegistration> funcReadRegistration=progId => {
				switch(progId) {
					case PROG_ID_18:
						return new ComServerRegistration { ProgId=progId };//not installed
					case PROG_ID_17:
						return LateBoundScope.RegisteredInproc(progId,false);//a 32-bit DLL only: skipped, not activated
					default:
						return LateBoundScope.RegisteredInproc(progId,true);
				}
			};
			using(LateBoundScope scope=new LateBoundScope(Sessions(recorder),arrayProgIds:_arrayProgIdsSynthetic,funcReadRegistration:funcReadRegistration)) {
				Assert.AreEqual("Connection to QuickBooks was successful.",TestConnection(out bool isHere));
				Assert.IsTrue(isHere);
				CollectionAssert.AreEqual(new[] { PROG_ID_18,PROG_ID_17,PROG_ID_16 },scope.ListRegistrationReads);
				CollectionAssert.AreEqual(new[] { PROG_ID_16 },scope.ListActivations);
				Assert.AreEqual(1,scope.ListEnumReads.Count);
				StringAssert.StartsWith(scope.ListEnumReads[0],"session: roeContinue,omDontCare,asActiveOnly,rtAccountQueryRs,rtClassQueryRs");
				Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.QuickBooksDesktopNative64.Choice);
				string logLine=NativeFirstChooser.QuickBooksDesktopNative64.LogLine;
				StringAssert.StartsWith(logLine,"QuickBooks Desktop 64-bit QBFC (QBSessionManager, late-bound): in the program's process (64-bit process); "
					+PROG_ID_16+" activated in this process ("+PROG_ID_16+" ({");
				StringAssert.Contains(logLine,"before it: "+PROG_ID_18+" has no class id in either registry view; "+PROG_ID_17+" ({");
				StringAssert.Contains(logLine,"InprocServer32 in the 32-bit registry view only");
				//The next call: a new session manager of the chosen ProgID, as QBFC 10's path makes one per connection; no registry read, no new choice.
				Assert.AreEqual("Connection to QuickBooks was successful.",TestConnection(out isHere));
				CollectionAssert.AreEqual(new[] { PROG_ID_16,PROG_ID_16 },scope.ListActivations);
				Assert.AreEqual(3,scope.ListRegistrationReads.Count);
				Assert.AreEqual(1,scope.ListEnumReads.Count);
				Assert.AreEqual(logLine,NativeFirstChooser.QuickBooksDesktopNative64.LogLine);
			}
		}

		[TestMethod]
		public void Choice_EachHResultOfTheActivation_MovesToTheNextOlder() {
			int[] arrayHResults=new[] { NativeFirstChooser.REGDB_E_CLASSNOTREG,NativeFirstChooser.CO_E_SERVER_EXEC_FAILURE,NativeFirstChooser.E_NOINTERFACE,
				NativeFirstChooser.TYPE_E_LIBNOTREGISTERED };
			foreach(int hResult in arrayHResults) {
				foreach(Func<Exception> funcException in new Func<Exception>[] {
					() => new COMException("Synthetic activation failure",hResult),
					() => new TargetInvocationException(new COMException("Synthetic activation failure",hResult)),
				}) {
					QbfcRecorder recorder=NewRecorder();
					Func<string,object> funcCreate=progId => {
						if(progId==PROG_ID_18) {
							throw funcException();
						}
						return Sessions(recorder)(progId);
					};
					using(LateBoundScope scope=new LateBoundScope(funcCreate,arrayProgIds:_arrayProgIdsSynthetic)) {
						Assert.AreEqual("Connection to QuickBooks was successful.",TestConnection(out bool isHere),"0x"+hResult.ToString("X8"));
						CollectionAssert.AreEqual(new[] { PROG_ID_18,PROG_ID_17 },scope.ListActivations);
						Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.QuickBooksDesktopNative64.Choice);
						StringAssert.Contains(NativeFirstChooser.QuickBooksDesktopNative64.LogLine,PROG_ID_17+" activated in this process");
						StringAssert.Contains(NativeFirstChooser.QuickBooksDesktopNative64.LogLine,PROG_ID_18+": activating it here failed with 0x"+hResult.ToString("X8"));
					}
				}
			}
			//E_NOINTERFACE also as the InvalidCastException of a failed QueryInterface (its HResult, COR_E_INVALIDCAST, is that value).
			Assert.AreEqual(NativeFirstChooser.E_NOINTERFACE,new InvalidCastException().HResult);
			QbfcRecorder recorderCast=NewRecorder();
			using(LateBoundScope scope=new LateBoundScope(progId => progId==PROG_ID_18 ? throw new InvalidCastException("Synthetic QueryInterface failure")
				: Sessions(recorderCast)(progId),arrayProgIds:_arrayProgIdsSynthetic))
			{
				TestConnection(out bool isHere);
				Assert.IsTrue(isHere);
				CollectionAssert.AreEqual(new[] { PROG_ID_18,PROG_ID_17 },scope.ListActivations);
			}
		}

		[TestMethod]
		public void Choice_NoneActivates_TheHelper_NothingCalledHere_Ever() {
			foreach(Func<string,ComServerRegistration> funcReadRegistration in new Func<string,ComServerRegistration>[] {
				progId => new ComServerRegistration { ProgId=progId },//none installed
				progId => LateBoundScope.RegisteredInproc(progId,false),//32-bit DLLs only
				progId => LateBoundScope.RegisteredInproc(progId,true),//registered, but each activation says CLASSNOTREG (below)
			}) {
				QbfcRecorder recorder=NewRecorder();
				using(LateBoundScope scope=new LateBoundScope(progId => throw new COMException("Synthetic: class not registered",NativeFirstChooser.REGDB_E_CLASSNOTREG),
					arrayProgIds:_arrayProgIdsSynthetic,funcReadRegistration:funcReadRegistration))
				{
					Assert.IsFalse(QuickBooksLateBound.TryTestConnection(COMPANY_FILE,out string result));
					Assert.AreEqual("",result);
					Assert.AreEqual(VendorPath.Helper32,NativeFirstChooser.QuickBooksDesktopNative64.Choice);
					StringAssert.StartsWith(NativeFirstChooser.QuickBooksDesktopNative64.LogLine,"QuickBooks Desktop 64-bit QBFC (QBSessionManager, late-bound): "
						+"in the 32-bit helper program (64-bit process); none of "+string.Join(", ",_arrayProgIdsSynthetic)+" activated in this process: ");
					int countReads=scope.ListRegistrationReads.Count;
					int countActivations=scope.ListActivations.Count;
					string logLine=NativeFirstChooser.QuickBooksDesktopNative64.LogLine;
					//Every later call goes to the helper without reading or activating anything again.
					Assert.IsFalse(QuickBooksLateBound.TryGetListOfAccounts(COMPANY_FILE,out List<string> listAccounts));
					Assert.IsFalse(QuickBooksLateBound.TryGetListOfClasses(COMPANY_FILE,out List<string> listClasses));
					Assert.IsFalse(QuickBooksLateBound.TryCreateDeposit(COMPANY_FILE,QuickBooksModuleTests._dateDeposit,"Checking","Income:Deposits",1,"Memo","",
						false));
					Assert.AreEqual(countReads,scope.ListRegistrationReads.Count);
					Assert.AreEqual(countActivations,scope.ListActivations.Count);
					Assert.AreEqual(logLine,NativeFirstChooser.QuickBooksDesktopNative64.LogLine);
					Assert.AreEqual(0,recorder.ListCalls.Count,"No QBFC call.");
					Assert.AreEqual(0,scope.ListEnumReads.Count);
				}
			}
		}

		[TestMethod]
		public void Choice_AnotherActivationFailure_IsAFailedConnection_TheHelperFromNowOn() {
			using(LateBoundScope scope=new LateBoundScope(progId => throw new COMException("Synthetic: unspecified failure",E_FAIL),
				arrayProgIds:_arrayProgIdsSynthetic))
			{
				Assert.IsFalse(QuickBooksLateBound.TryTestConnection(COMPANY_FILE,out string result));
				CollectionAssert.AreEqual(new[] { PROG_ID_18 },scope.ListActivations,"Thrown by the chooser: no older one tried.");
				Assert.AreEqual(VendorPath.Helper32,NativeFirstChooser.QuickBooksDesktopNative64.Choice);
				StringAssert.Contains(NativeFirstChooser.QuickBooksDesktopNative64.LogLine,"connecting failed in this process with 0x80004005 (COMException: "
					+"Synthetic: unspecified failure)");
				Assert.IsFalse(QuickBooksLateBound.TryTestConnection(COMPANY_FILE,out result));
				Assert.AreEqual(1,scope.ListActivations.Count);
			}
		}

		///<summary>Connecting fails at each step: CreateMsgSetRequest, OpenConnection, BeginSession (QuickBooks too old, not running, the file
		///refused...), and the enum values of the type library. Each Try method gives the call to the helper (false), after the calls today's code
		///makes up to that failure, its clean-up included; the helper is chosen and logged with the failure and the earlier choice, and every later
		///call goes to the helper without touching QBFC (once).</summary>
		[TestMethod]
		public void FailedConnection_EachStep_EachCall_TheHelperOnce() {
			List<Tuple<string,Action<QbfcRecorder>>> listSteps=new List<Tuple<string,Action<QbfcRecorder>>> {
				Tuple.Create("CreateMsgSetRequest",(Action<QbfcRecorder>)(recorder => recorder.On("session.CreateMsgSetRequest",
					x => throw new COMException("Synthetic: the qbXML version is not supported.",E_FAIL)))),
				Tuple.Create("OpenConnection",(Action<QbfcRecorder>)(recorder => recorder.On("session.OpenConnection",
					x => throw new COMException("Synthetic: QuickBooks could not be started.",E_FAIL)))),
				Tuple.Create("BeginSession",(Action<QbfcRecorder>)(recorder => recorder.On("session.BeginSession",
					x => throw new COMException("Synthetic: the company file could not be opened.",unchecked((int)0x80040408))))),
			};
			List<Tuple<string,Func<bool>,Func<string>>> listCalls=new List<Tuple<string,Func<bool>,Func<string>>> {
				Tuple.Create("TestConnection",(Func<bool>)(() => QuickBooksLateBound.TryTestConnection(COMPANY_FILE,out string result)),
					(Func<string>)(() => QuickBooksToday.TestConnection(COMPANY_FILE))),
				Tuple.Create("GetListOfAccounts",(Func<bool>)(() => QuickBooksLateBound.TryGetListOfAccounts(COMPANY_FILE,out List<string> listAccounts)),
					(Func<string>)(() => string.Join("|",QuickBooksToday.GetListOfAccounts()))),
				Tuple.Create("GetListOfClasses",(Func<bool>)(() => QuickBooksLateBound.TryGetListOfClasses(COMPANY_FILE,out List<string> listClasses)),
					(Func<string>)(() => string.Join("|",QuickBooksToday.GetListOfClasses()))),
				Tuple.Create("CreateDeposit",(Func<bool>)(() => QuickBooksLateBound.TryCreateDeposit(COMPANY_FILE,QuickBooksModuleTests._dateDeposit,"Checking",
					"Income:Deposits",1234.5,"Deposit 2026-11-14 (synthetic)","Clinic East",true)),
					(Func<string>)(() => QuickBooksModuleTests.DepositToday("Clinic East",true))),
			};
			foreach(Tuple<string,Action<QbfcRecorder>> tupleStep in listSteps) {
				foreach(Tuple<string,Func<bool>,Func<string>> tupleCall in listCalls) {
					string what=tupleCall.Item1+", "+tupleStep.Item1+" fails";
					QuickBooksModuleTests.Run runToday=QuickBooksModuleTests.RunToday(recorder => {
						QuickBooksModuleTests.ScriptSession(recorder);
						tupleStep.Item2(recorder);
					},tupleCall.Item3);
					Assert.IsNotNull(runToday.Result??runToday.ErrorMessage);
					QbfcRecorder recorderLateBound=NewRecorder(tupleStep.Item2);
					using(LateBoundScope scope=new LateBoundScope(Sessions(recorderLateBound))) {
						Assert.IsFalse(tupleCall.Item2(),what+": sent to the helper.");
						CollectionAssert.AreEqual(runToday.ListCalls,recorderLateBound.ListCalls,what+":\n"+string.Join("\n",recorderLateBound.ListCalls));
						Assert.IsFalse(recorderLateBound.ListCalls.Any(x => x.Contains("Append")),what+": nothing was asked of QuickBooks.");
						Assert.AreEqual(VendorPath.Helper32,NativeFirstChooser.QuickBooksDesktopNative64.Choice,what);
						string logLine=NativeFirstChooser.QuickBooksDesktopNative64.LogLine;
						StringAssert.Contains(logLine,"in the 32-bit helper program (64-bit process); connecting failed in this process with 0x",what);
						StringAssert.Contains(logLine,", after this process had been chosen (QBFC16.QBSessionManager.1 activated in this process",what);
						int countCalls=recorderLateBound.ListCalls.Count;
						foreach(Tuple<string,Func<bool>,Func<string>> tupleLater in listCalls) {
							Assert.IsFalse(tupleLater.Item2(),what+", then "+tupleLater.Item1);
						}
						Assert.AreEqual(countCalls,recorderLateBound.ListCalls.Count,what+": no QBFC call after the move.");
						Assert.AreEqual(1,scope.ListActivations.Count,what);
						Assert.AreEqual(logLine,NativeFirstChooser.QuickBooksDesktopNative64.LogLine,what+": logged once.");
					}
				}
			}
		}

		[TestMethod]
		public void FailedConnection_TheTypeLibraryCannotBeRead_TheHelper_NoQbfcCall() {
			QbfcRecorder recorder=NewRecorder();
			using(LateBoundScope scope=new LateBoundScope(Sessions(recorder),
				funcReadEnumValues:listNames => throw new KeyNotFoundException("The type library has no enum constant rtClassQueryRs.")))
			{
				Assert.IsFalse(QuickBooksLateBound.TryGetListOfClasses(COMPANY_FILE,out List<string> listClasses));
				Assert.AreEqual(0,recorder.ListCalls.Count);
				Assert.AreEqual(VendorPath.Helper32,NativeFirstChooser.QuickBooksDesktopNative64.Choice);
				StringAssert.Contains(NativeFirstChooser.QuickBooksDesktopNative64.LogLine,"connecting failed in this process with (KeyNotFoundException: "
					+"The type library has no enum constant rtClassQueryRs.)");
			}
		}

		///<summary>A failure after the connection is QuickBooks' answer: it shows as before (thrown, or TestConnection's "Error: ...") and the call
		///is never repeated in the helper, so a deposit QuickBooks may have taken is not sent twice; the choice stays.</summary>
		[TestMethod]
		public void FailureAfterTheConnection_ShowsAsBefore_NeverRepeatedInTheHelper() {
			QbfcRecorder recorder=NewRecorder(x => x.On("session.DoRequests",y => throw new COMException("Synthetic: QuickBooks closed the connection.")));
			using(new LateBoundScope(Sessions(recorder))) {
				COMException comException=Assert.ThrowsExactly<COMException>(() => QuickBooksLateBound.TryCreateDeposit(COMPANY_FILE,
					QuickBooksModuleTests._dateDeposit,"Checking","Income:Deposits",1234.5,"Deposit 2026-11-14 (synthetic)","",false));
				Assert.AreEqual("Synthetic: QuickBooks closed the connection.",comException.Message);
				Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.QuickBooksDesktopNative64.Choice);
				CollectionAssert.AreEqual(new[] { "session.DoRequests(request)","session.EndSession()","session.CloseConnection()" },
					recorder.ListCalls.Skip(recorder.ListCalls.Count-3).ToArray());
				Assert.IsTrue(QuickBooksLateBound.TryTestConnection(COMPANY_FILE,out string result));
				Assert.AreEqual("Error: Synthetic: QuickBooks closed the connection.",result);
				Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.QuickBooksDesktopNative64.Choice);
			}
		}

		///<summary>The enum values are the activated QBFC's (its type library), not QBFC 10's: with other numbers there, the request carries them
		///and the response types are matched with them.</summary>
		[TestMethod]
		public void EnumValues_AreTheActivatedQbfcs() {
			Func<IList<string>,Dictionary<string,int>> funcReadEnumValues=listNames => new Dictionary<string,int> {
				{ "roeContinue",(int)ENRqOnError.roeRollback },//a number QBFC 10 gives another constant, to show which one went out
				{ "omDontCare",(int)ENOpenMode.omMultiUser },
				{ "asActiveOnly",(int)ENActiveStatus.asAll },
				{ "rtAccountQueryRs",5000 },
				{ "rtClassQueryRs",5001 },
			};
			QbfcRecorder recorder=new QbfcRecorder();
			QuickBooksModuleTests.ScriptResponses(recorder,
				Tuple.Create(0,(ENResponseType)5000,(Func<object>)(() => QuickBooksModuleTests.ScriptNames<IAccountRetList,IAccountRet>(recorder,"accounts","Checking"))),
				Tuple.Create(0,ENResponseType.rtAccountQueryRs,(Func<object>)(() => QuickBooksModuleTests.ScriptNames<IAccountRetList,IAccountRet>(recorder,
					"accountsQbfc10","Not this one"))));
			using(new LateBoundScope(Sessions(recorder),funcReadEnumValues:funcReadEnumValues)) {
				Assert.IsTrue(QuickBooksLateBound.TryGetListOfAccounts(COMPANY_FILE,out List<string> listAccounts));
				CollectionAssert.AreEqual(new[] { "Checking" },listAccounts);
				CollectionAssert.Contains(recorder.ListCalls,"request.Attributes.OnError=roeRollback");
				CollectionAssert.Contains(recorder.ListCalls,"session.BeginSession(\""+COMPANY_FILE+"\",omMultiUser)");
				CollectionAssert.Contains(recorder.ListCalls,"request.AppendAccountQueryRq().ORAccountListQuery.AccountListFilter.ActiveStatus.SetValue(asAll)");
			}
		}

		///<summary>The types the dynamic calls pass: what IDispatch needs for QBFC 10's parameters (the request versions as Int16, the enums as
		///Int32, the date as DateTime, the request as the COM object), pinned for the deposit and the account list.</summary>
		[TestMethod]
		public void ArgumentTypes_AreTheAutomationTypesOfQbfcsParameters() {
			QuickBooksModuleTests.Run runDeposit=QuickBooksModuleTests.RunLateBound(
				recorder => QuickBooksModuleTests.ScriptResponses(recorder,Tuple.Create(0,ENResponseType.rtDepositAddRs,(Func<object>)null)),
				() => QuickBooksModuleTests.LateBoundDeposit("Clinic East",true));
			Assert.IsNull(runDeposit.ErrorMessage);
			const string line="request.AppendDepositAddRq().DepositLineAddList.Append().ORDepositLineAdd.DepositInfo.";
			CollectionAssert.AreEqual(new[] {
				"session.CreateMsgSetRequest(String,Int16,Int16)",
				"request.Attributes.OnError=Int32",
				"session.OpenConnection(String,String)",
				"session.BeginSession(String,Int32)",
				"request.AppendDepositAddRq()",
				"request.AppendDepositAddRq().TxnDate.SetValue(DateTime)",
				"request.AppendDepositAddRq().DepositToAccountRef.FullName.SetValue(String)",
				"request.AppendDepositAddRq().Memo.SetValue(String)",
				"request.AppendDepositAddRq().DepositLineAddList.Append()",
				line+"AccountRef.FullName.SetValue(String)",
				line+"Amount.SetAsString(String)",
				line+"ClassRef.FullName.SetValue(String)",
				"session.DoRequests(COM object)",
				"session.EndSession()",
				"session.CloseConnection()",
				"responses.GetAt(Int32)",
			},runDeposit.CallLog.ListArgTypes);
			CollectionAssert.AreEqual(new string[0],runDeposit.CallLog.ListMismatches);
			QuickBooksModuleTests.Run runAccounts=QuickBooksModuleTests.RunLateBound(QuickBooksModuleTests.ScriptSession,QuickBooksModuleTests.LateBoundAccounts);
			CollectionAssert.Contains(runAccounts.CallLog.ListArgTypes,"request.AppendAccountQueryRq().ORAccountListQuery.AccountListFilter.ActiveStatus.SetValue(Int32)");
			CollectionAssert.AreEqual(new string[0],runAccounts.CallLog.ListMismatches);
		}
	}
}
