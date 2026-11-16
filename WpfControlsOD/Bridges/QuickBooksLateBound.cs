// QuickBooks Desktop's 64-bit QBFC session in a 64-bit program's own process, late-bound (N64-B6 of the native 64-bit integration survey,
// docs/port-notes/N64-B6.md; net10-plan decision 23). Intuit's 64-bit QBFC (15 and later, for QuickBooks Desktop 2022 and later) is an
// in-process COM server that a 64-bit process loads; this file calls it through IDispatch (dynamic), with no interop and no SDK file in the
// repository, so the build PC needs no QuickBooks SDK. The calls are QuickBooksInProcess.cs's (QBFC 10, the 32-bit program's path) one for one,
// with the same request versions (1.0, 8.0 and 9.0, which every QBFC accepts), and the enum values read by name from the activated QBFC's own
// type library (ComTypeLibEnums), since they differ between QBFC versions. The session manager is the newest of ArrayProgIds that
// NativeFirstChooser.QuickBooksDesktopNative64 finds registered for 64-bit and activates (both registry views, then the activation HRESULT rule
// of N64-A3); none: the 32-bit helper's QBFC 10 module (QuickBooks.cs's other path). A failed connection (creating the session manager,
// CreateMsgSetRequest, OpenConnection, BeginSession) moves the program to the helper too, and the call is made there once more.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CodeBase;

namespace OpenPmsDental.Bridges {
	///<summary>QuickBooks Desktop's QBFC session in a 64-bit program's process, late-bound (QuickBooks.cs's 64-bit path, N64-B6). Every Try
	///method returns false, having done nothing that reached QuickBooks, when the call goes to the 32-bit helper instead; true with the result
	///otherwise.</summary>
	internal static class QuickBooksLateBound {
		///<summary>The 64-bit QBFC session managers tried, the newest first, each by the exact ProgID a public page gives (N64-B6.md, section 2,
		///with each source). A version whose ProgID no public page gives is left out; adding it is one line here, after the launch check's registry
		///query (section 8) shows it. A test sets its own list; nothing else changes it.</summary>
		internal static string[] ArrayProgIds={
			"QBFC16.QBSessionManager.1",//QBFC 16 (Desktop SDK 16.0, 64-bit)
		};
		///<summary>The enum constants the calls use, read by name from the activated QBFC's type library.</summary>
		internal static readonly string[] ArrayEnumNames={ "roeContinue","omDontCare","asActiveOnly","rtAccountQueryRs","rtClassQueryRs" };
		///<summary>Creates the session manager of a ProgID (Type.GetTypeFromProgID, Activator.CreateInstance). A test sets a fake; nothing else
		///changes it.</summary>
		internal static Func<string,object> FuncCreateComObject=progId => CreateComObject(progId);
		///<summary>Reads a ProgID's registration in both registry views, read only. A test sets a fake; nothing else changes it.</summary>
		internal static Func<string,ComServerRegistration> FuncReadRegistration=progId => ComServerRegistration.Read(progId);
		///<summary>Reads ArrayEnumNames' values from the session manager's type library. A test sets a fake; nothing else changes it.</summary>
		internal static Func<object,IList<string>,Dictionary<string,int>> FuncReadEnumValues=(comObject,listNames) => ComTypeLibEnums.ReadFromObject(comObject,listNames);

		///<summary>The ProgID that activated in this process, "" before; with the enum values of its type library.</summary>
		private static string _progIdChosen="";
		private static Dictionary<string,int> _dictEnumValues;

		private static dynamic SessionManager;
		private static dynamic RequestMsgSet;
		private static dynamic ResponseMsgSet;
		private static bool SessionBegun;
		private static bool ConnectionOpen;

		///<summary>Forgets the state, as a new program process has it. For tests.</summary>
		internal static void ResetForTests() {
			_progIdChosen="";
			_dictEnumValues=null;
			SessionManager=null;
			RequestMsgSet=null;
			ResponseMsgSet=null;
			SessionBegun=false;
			ConnectionOpen=false;
		}

		///<summary>True in a 64-bit program, which takes this path (else the helper), never QBFC 10 in its own process; false in a 32-bit
		///program, which keeps QuickBooksInProcess (QBFC 10) exactly as before.</summary>
		public static bool Is64BitProcess {
			get {
				return NativeFirstChooser.QuickBooksDesktopNative64.Is64BitProcess;
			}
		}

		///<summary>Type.GetTypeFromProgID and Activator.CreateInstance. A ProgID this process's registry view does not know throws
		///REGDB_E_CLASSNOTREG (the next older ProgID is tried).</summary>
		internal static object CreateComObject(string progId) {
			Type type=Type.GetTypeFromProgID(progId);
			if(type==null) {
				throw new COMException(progId+" is not registered in this process's registry view.",NativeFirstChooser.REGDB_E_CLASSNOTREG);
			}
			return Activator.CreateInstance(type);
		}

		///<summary>The session manager: the first time, the newest of ArrayProgIds that activates here (the chooser, which logs its choice) with
		///its type library's enum values; later, a new one of the chosen ProgID, as new QBSessionManager() made one per connection. Throws when
		///none activated (the chooser picked the helper) or the enum values cannot be read, which QuickBooks.cs takes as a failed connection.</summary>
		private static dynamic CreateSessionManager() {
			if(_progIdChosen!="") {
				return FuncCreateComObject(_progIdChosen);
			}
			object sessionManager=NativeFirstChooser.QuickBooksDesktopNative64.ActivateNewestComServer(ArrayProgIds,FuncReadRegistration,FuncCreateComObject,
				out string progIdActivated);
			if(sessionManager==null) {
				throw new InvalidOperationException("No 64-bit QuickBooks Desktop QBFC session manager activates in this process; the 32-bit helper program is used.");
			}
			_dictEnumValues=FuncReadEnumValues(sessionManager,ArrayEnumNames);
			_progIdChosen=progIdActivated;
			return sessionManager;
		}

		///<summary>An enum constant of the chosen QBFC's type library, as an int (its automation type, VT_I4).</summary>
		private static int EnumValue(string name) {
			return _dictEnumValues[name];
		}

		///<summary>True when this call goes to the helper: nothing is chosen yet or this process was, and connecting failed here (any failure up to
		///and including BeginSession), so NativeFirstChooser.QuickBooksDesktopNative64 moves the program to the helper (logged), or the helper was
		///chosen already. A failure after the connection (DoRequests, the response) is QuickBooks' answer and shows as before.</summary>
		private static bool IsRetriedInHelper(Exception ex,bool isConnected) {
			if(isConnected) {
				return false;
			}
			return NativeFirstChooser.QuickBooksDesktopNative64.ChooseHelperAfterConnectionFailure(ex);
		}

		///<summary>False when the helper is chosen already; nothing is called then.</summary>
		private static bool IsChosen() {
			return NativeFirstChooser.QuickBooksDesktopNative64.Choice!=VendorPath.Helper32;
		}

		///<summary>QuickBooksInProcess.OpenConnection, late-bound: the same calls, the enum values from the chosen QBFC.</summary>
		private static void OpenConnection(short majorVer,short minorVer,string companyPath) {
			SessionManager=CreateSessionManager();
			//Create the message set request object to hold our request.
			RequestMsgSet=SessionManager.CreateMsgSetRequest("US",majorVer,minorVer);
			RequestMsgSet.Attributes.OnError=EnumValue("roeContinue");
			//Connect to QuickBooks and begin a session
			SessionManager.OpenConnection("","Open Dental");
			ConnectionOpen=true;
			SessionManager.BeginSession(companyPath,EnumValue("omDontCare"));
			SessionBegun=true;
		}

		///<summary>Runs the request that has been built.  QB connection must be open before calling this method.</summary>
		private static void DoRequests() {
			if(!ConnectionOpen) {
				return;
			}
			ResponseMsgSet=SessionManager.DoRequests(RequestMsgSet);
		}

		///<summary>Ends the session and then closes the connection.</summary>
		private static void CloseConnection() {
			if(SessionBegun) {
				SessionManager.EndSession();
				SessionBegun=false;
			}
			if(ConnectionOpen) {
				SessionManager.CloseConnection();
				ConnectionOpen=false;
			}
		}

		///<summary>QuickBooksInProcess.TestConnection, late-bound. False: the call goes to the helper.</summary>
		public static bool TryTestConnection(string companyPath,out string result) {
			result="";
			if(!IsChosen()) {
				return false;
			}
			bool isConnected=false;
			try {
				OpenConnection(1,0,companyPath);
				isConnected=true;
				//Send the empty request and get the response from QuickBooks.
				ResponseMsgSet=SessionManager.DoRequests(RequestMsgSet);
				CloseConnection();
				result="Connection to QuickBooks was successful.";
				return true;
			}
			catch(Exception e) {
				if(SessionBegun) {
					SessionManager.EndSession();
				}
				if(ConnectionOpen) {
					SessionManager.CloseConnection();
				}
				if(IsRetriedInHelper(e,isConnected)) {
					return false;
				}
				result="Error: "+e.Message;
				return true;
			}
		}

		///<summary>QuickBooksInProcess.GetListOfAccounts, late-bound. False: the call goes to the helper.</summary>
		public static bool TryGetListOfAccounts(string companyPath,out List<string> accountList) {
			accountList=new List<string>();
			if(!IsChosen()) {
				return false;
			}
			bool isConnected=false;
			try {
				OpenConnection(8,0,companyPath);
				isConnected=true;
				QueryListOfAccounts();
				DoRequests();
				CloseConnection();
			}
			catch(Exception e) {
				if(SessionBegun) {
					SessionManager.EndSession();
				}
				if(ConnectionOpen) {
					SessionManager.CloseConnection();
				}
				if(IsRetriedInHelper(e,isConnected)) {
					return false;
				}
				throw;
			}
			accountList=GetFullNames(EnumValue("rtAccountQueryRs"));
			return true;
		}

		///<summary>QuickBooksInProcess.QueryListOfAccounts, late-bound.</summary>
		private static void QueryListOfAccounts() {
			if(!ConnectionOpen) {
				return;
			}
			//Build the account query add append it to the request message.
			dynamic AccountQueryRq=RequestMsgSet.AppendAccountQueryRq();
			//Filters
			AccountQueryRq.ORAccountListQuery.AccountListFilter.ActiveStatus.SetValue(EnumValue("asActiveOnly"));
		}

		///<summary>QuickBooksInProcess.GetListOfClasses, late-bound. False: the call goes to the helper.</summary>
		public static bool TryGetListOfClasses(string companyPath,out List<string> listClasses) {
			listClasses=new List<string>();
			if(!IsChosen()) {
				return false;
			}
			bool isConnected=false;
			try {
				OpenConnection(8,0,companyPath);
				isConnected=true;
				QueryListOfClasses();
				DoRequests();
				CloseConnection();
			}
			catch(Exception e) {
				if(SessionBegun) {
					SessionManager.EndSession();
				}
				if(ConnectionOpen) {
					SessionManager.CloseConnection();
				}
				if(IsRetriedInHelper(e,isConnected)) {
					return false;
				}
				throw;
			}
			listClasses=GetFullNames(EnumValue("rtClassQueryRs"));
			return true;
		}

		///<summary>QuickBooksInProcess.QueryListOfClasses, late-bound.</summary>
		private static void QueryListOfClasses() {
			if(!ConnectionOpen) {
				return;
			}
			//Build the class query add append it to the request message.
			dynamic ClassQueryRq=RequestMsgSet.AppendClassQueryRq();
			//Filters
			ClassQueryRq.ORListQuery.ListFilter.ActiveStatus.SetValue(EnumValue("asActiveOnly"));
		}

		///<summary>The response walk of QuickBooksInProcess.GetListOfAccounts and GetListOfClasses (the same code in both): the FullName of each
		///entry of the responses of type responseTypeWanted with status 0 or a warning (above 0) and a detail; an entry without FullName is
		///skipped.</summary>
		private static List<string> GetFullNames(int responseTypeWanted) {
			List<string> listNames=new List<string>();
			if(ResponseMsgSet==null) {
				return listNames;
			}
			dynamic responseList=ResponseMsgSet.ResponseList;
			if(responseList==null) {
				return listNames;
			}
			//Loop through the list to pick out the query's response section.
			for(int i=0;i<responseList.Count;i++) {
				dynamic response=responseList.GetAt(i);
				//Check the status code of the response, 0=ok, >0 is warning.
				if(response.StatusCode >= 0) {
					//The request-specific response is in the details, make sure we have some.
					if(response.Detail!=null) {
						//Make sure the response is the type we're expecting.
						int responseType=(int)response.Type.GetValue();
						if(responseType==responseTypeWanted) {
							//The ret list: safe because we checked with response.Type check above.
							dynamic RetList=response.Detail;
							for(int j=0;j<RetList.Count;j++) {
								dynamic Ret=RetList.GetAt(j);
								if(Ret.FullName!=null) {
									listNames.Add((string)Ret.FullName.GetValue());
								}
							}
						}
					}
				}
			}
			return listNames;
		}

		///<summary>QuickBooksInProcess.CreateDeposit, late-bound. False: the call goes to the helper (nothing reached QuickBooks, so no deposit
		///was made); a failure after the connection (DoRequests, the response check) is thrown as before and never sent to the helper.</summary>
		public static bool TryCreateDeposit(string companyPath,DateTime txnDate,string depositAcct,string incomeAcct,double amount,string memo,
			string classRef,bool isClassRefsEnabled)
		{
			if(!IsChosen()) {
				return false;
			}
			bool isConnected=false;
			try {
				OpenConnection(9,0,companyPath);
				isConnected=true;
				BuildDepositAddRq(txnDate,depositAcct,incomeAcct,amount,memo,classRef,isClassRefsEnabled);
				DoRequests();
				CloseConnection();
				ValidateDepositAddRs();
			}
			catch(Exception e) {
				if(SessionBegun) {
					SessionManager.EndSession();
				}
				if(ConnectionOpen) {
					SessionManager.CloseConnection();
				}
				if(IsRetriedInHelper(e,isConnected)) {
					return false;
				}
				throw;
			}
			return true;
		}

		///<summary>QuickBooksInProcess.BuildDepositAddRq, late-bound.</summary>
		private static void BuildDepositAddRq(DateTime txnDate,string depositAcct,string incomeAcct,double amount,string memo,string classRef,
			bool isClassRefsEnabled)
		{
			if(!ConnectionOpen) {
				return;
			}
			//Build the deposit add request and append it to the request message.
			dynamic DepositAddRq=RequestMsgSet.AppendDepositAddRq();
			//Set field value for TxnDate
			DepositAddRq.TxnDate.SetValue(txnDate);
			//Set field value for FullName.
			DepositAddRq.DepositToAccountRef.FullName.SetValue(depositAcct);
			//Set field value for Memo
			DepositAddRq.Memo.SetValue(memo);
			//Set deposit info attributes.
			dynamic DepositLineAdd1=DepositAddRq.DepositLineAddList.Append();
			//Set field value for income account
			DepositLineAdd1.ORDepositLineAdd.DepositInfo.AccountRef.FullName.SetValue(incomeAcct);
			//Set field value for Amount
			DepositLineAdd1.ORDepositLineAdd.DepositInfo.Amount.SetAsString(amount.ToString("F"));
			//Set field value for ClassRef which is a way they can group deposits together
			if(!string.IsNullOrEmpty(classRef) && isClassRefsEnabled) {
				DepositLineAdd1.ORDepositLineAdd.DepositInfo.ClassRef.FullName.SetValue(classRef);
			}
		}

		///<summary>QuickBooksInProcess.ValidateDepositAddRs, late-bound: a status above 0 throws its status message.</summary>
		private static void ValidateDepositAddRs() {
			if(ResponseMsgSet == null) {
				return;
			}
			dynamic responseList = ResponseMsgSet.ResponseList;
			if(responseList == null) {
				return;
			}
			//if we sent only one request, there is only one response, we'll walk the list for this sample
			for(int i=0;i<responseList.Count;i++) {
				dynamic response = responseList.GetAt(i);
				//check the status code of the response, 0=ok, >0 is warning
				if(response.StatusCode > 0) {
					throw new Exception((string)response.StatusMessage);
				}
			}
		}
	}
}
