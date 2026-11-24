// QuickBooks Desktop's QBFC 10 session in the program's own process, the native-first path (S3a-14n of the .NET 10 move,
// docs/port-notes/S3a-14.md, section "S3a-14n: native first"; docs/port-notes/S3a-13.md): QuickBooks.cs's code of before S3a-13 (main before
// the S3a-13 merge), moved here line for line with three changes, each marked S3a-14n: the session manager comes through
// NativeFirstChooser.QuickBooksDesktop instead of new QBSessionManager(), and the two preferences the code read (PrefName.QuickBooksCompanyFile,
// PrefName.QuickBooksClassRefsEnabled) come from QuickBooks.cs as arguments (the commented-out entities region is left out; the helper's module
// keeps it). QBFC 10 is an in-process COM server that is 32-bit only, so the
// chooser picks this process in a 32-bit program and the 32-bit helper's QuickBooks module (QuickBooks.cs's other path) in a 64-bit one.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeBase;
using Interop.QBFC10;

namespace OpenPmsDental.Bridges {
	///<summary>Contains all logic for QuickBook connections and requests to the QB company file, in the program's process (QuickBooks.cs's path
	///before S3a-13).</summary>
	internal static class QuickBooksInProcess {
		///<summary>Creates QBFC's session manager, through NativeFirstChooser.QuickBooksDesktop. A test sets a fake; nothing else changes it.</summary>
		internal static Func<QBSessionManager> FuncCreateSessionManager=() => new QBSessionManager();
		///<summary>The session manager IsChosen activated to make the choice, for the first OpenConnection, so it is created once as before.</summary>
		private static QBSessionManager _sessionManagerChosen;

		private static QBSessionManager SessionManager;
		private static IMsgSetRequest RequestMsgSet;
		private static IMsgSetResponse ResponseMsgSet;
		private static bool SessionBegun;
		private static bool ConnectionOpen;

		///<summary>Forgets the state, as a new program process has it. For tests.</summary>
		internal static void ResetForTests() {
			_sessionManagerChosen=null;
			SessionManager=null;
			RequestMsgSet=null;
			ResponseMsgSet=null;
			SessionBegun=false;
			ConnectionOpen=false;
		}

		///<summary>True when QuickBooks.cs runs the call here, in the program's process (NativeFirstChooser.QuickBooksDesktop). The first call
		///activates the session manager once to choose and keeps it for its OpenConnection: activated, this process; not registered for this
		///bitness, the helper in a 64-bit program (false). Any failure of that first activation leaves the call here, where OpenConnection
		///activates again inside the method's own try and the failure shows as before (TestConnection's "Error: ...", the others' exception).
		///</summary>
		public static bool IsChosen() {
			NativeFirstChooser nativeFirstChooser=NativeFirstChooser.QuickBooksDesktop;
			VendorPath? vendorPath=nativeFirstChooser.Choice;
			if(vendorPath!=null) {
				return vendorPath==VendorPath.InProcess;
			}
			try {
				_sessionManagerChosen=nativeFirstChooser.ActivateComObject(FuncCreateSessionManager);
			}
			catch(Exception ex) {
				ex.DoNothing();//A 32-bit program (the helper could not do better) or another failure: the method below reports it as before.
				return true;
			}
			return _sessionManagerChosen!=null;
		}

		///<summary>S3a-14n: what "new QBSessionManager()" was. The session manager IsChosen activated, else a new one through the chooser.</summary>
		private static QBSessionManager CreateSessionManager() {
			QBSessionManager sessionManager=_sessionManagerChosen;
			_sessionManagerChosen=null;
			if(sessionManager==null) {
				sessionManager=NativeFirstChooser.QuickBooksDesktop.ActivateComObject(FuncCreateSessionManager);
			}
			if(sessionManager==null) {//The chooser picked the helper during this call (not seen: IsChosen chose first).
				throw new InvalidOperationException("QuickBooks Desktop's QBFC cannot be activated in this process; the next call uses the 32-bit helper program.");
			}
			return sessionManager;
		}

		///<summary>Creates a new QB connection and begins the session.  Session will be left open until CloseConnection is called.  Major and minor version refer to the implementation version of the paticular QB request you are trying to run.  The connection will fail if the version you pass in does not support the type of request you are trying to run.</summary>
		private static void OpenConnection(short majorVer,short minorVer,string companyPath) {
			SessionManager=CreateSessionManager();//S3a-14n
			//Create the message set request object to hold our request.
			RequestMsgSet=SessionManager.CreateMsgSetRequest("US",majorVer,minorVer);
			RequestMsgSet.Attributes.OnError=ENRqOnError.roeContinue;
			//Connect to QuickBooks and begin a session
			SessionManager.OpenConnection("","Open PMS - Dental");
			ConnectionOpen=true;
			SessionManager.BeginSession(companyPath,ENOpenMode.omDontCare);
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

		///<summary>Simplest connection test to QB.  Users have to connect to their QB company file with OD and QB running at the same time the first time they connect.  This is just a simple tool to let them get this connection out of the way.  QB will prompt the user to set permissions / access rights for OD and then from there on QB does not need to be open in the background.</summary>
		public static string TestConnection(string companyPath) {
			try {
				OpenConnection(1,0,companyPath);
				//Send the empty request and get the response from QuickBooks.
				ResponseMsgSet=SessionManager.DoRequests(RequestMsgSet);
				CloseConnection();
				return "Connection to QuickBooks was successful.";
			}
			catch(Exception e) {
				if(SessionBegun) {
					SessionManager.EndSession();
				}
				if(ConnectionOpen) {
					SessionManager.CloseConnection();
				}
				return "Error: "+e.Message;
			}
		}

		///<summary>Returns list of all active accounts. S3a-14n: companyPath is PrefName.QuickBooksCompanyFile.</summary>
		public static List<string> GetListOfAccounts(string companyPath) {
			List<string> accountList=new List<string>();
			try {
				OpenConnection(8,0,companyPath);
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
				throw e;
			}
			if(ResponseMsgSet==null) {
				return accountList;
			}
			IResponseList responseList=ResponseMsgSet.ResponseList;
			if(responseList==null) {
				return accountList;
			}
			//Loop through the list to pick out the AccountQueryRs section.
			for(int i=0;i<responseList.Count;i++) {
				IResponse response = responseList.GetAt(i);
				//Check the status code of the response, 0=ok, >0 is warning.
				if(response.StatusCode >= 0) {
					//The request-specific response is in the details, make sure we have some.
					if(response.Detail!=null) {
						//Make sure the response is the type we're expecting.
						ENResponseType responseType=(ENResponseType)response.Type.GetValue();
						if(responseType==ENResponseType.rtAccountQueryRs) {
							//Upcast to more specific type here, this is safe because we checked with response.Type check above.
							IAccountRetList AccountRetList=(IAccountRetList)response.Detail;
							for(int j=0;j<AccountRetList.Count;j++) {
								IAccountRet AccountRet=AccountRetList.GetAt(j);
								if(AccountRet.FullName!=null) {
									accountList.Add(AccountRet.FullName.GetValue());
								}
							}
						}
					}
				}
			}
			return accountList;
		}

		///<summary>Adds an account query to the request message.  A QB connection must be open before calling this method. Requires connection with version 8.0</summary>
		private static void QueryListOfAccounts() {
			if(!ConnectionOpen) {
				return;
			}
			//Build the account query add append it to the request message.
			IAccountQuery AccountQueryRq=RequestMsgSet.AppendAccountQueryRq();
			//Filters
			AccountQueryRq.ORAccountListQuery.AccountListFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly);
		}

		///<summary>Returns list of all QB "class" names. S3a-14n: companyPath is PrefName.QuickBooksCompanyFile.</summary>
		public static List<string> GetListOfClasses(string companyPath) {
			List<string> listClasses=new List<string>();
			try {
				OpenConnection(8,0,companyPath);
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
				throw e;
			}
			if(ResponseMsgSet==null) {
				return listClasses;
			}
			IResponseList responseList=ResponseMsgSet.ResponseList;
			if(responseList==null) {
				return listClasses;
			}
			//Loop through the list to pick out the ClassQueryRs section.
			for(int i = 0;i<responseList.Count;i++) {
				IResponse response = responseList.GetAt(i);
				//Check the status code of the response, 0=ok, >0 is warning.
				if(response.StatusCode >= 0) {
					//The request-specific response is in the details, make sure we have some.
					if(response.Detail!=null) {
						//Make sure the response is the type we're expecting.
						ENResponseType responseType=(ENResponseType)response.Type.GetValue();
						if(responseType==ENResponseType.rtClassQueryRs) {
							//Upcast to more specific type here, this is safe because we checked with response.Type check above.
							IClassRetList ClassRetList = (IClassRetList)response.Detail;
							for(int j = 0;j<ClassRetList.Count;j++) {
								IClassRet ClassRet=ClassRetList.GetAt(j);
								if(ClassRet.FullName!=null) {
									listClasses.Add(ClassRet.FullName.GetValue());
								}
							}
						}
					}
				}
			}
			return listClasses;
		}

		///<summary>Adds a class query to the request message.  A QB connection must be open before calling this method. Requires connection with version 8.0</summary>
		private static void QueryListOfClasses() {
			if(!ConnectionOpen) {
				return;
			}
			//Build the class query add append it to the request message.
			IClassQuery ClassQueryRq=RequestMsgSet.AppendClassQueryRq();
			//Filters
			ClassQueryRq.ORListQuery.ListFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly);
		}

		///<summary>Creates a deposit within QuickBooks. S3a-14n: companyPath is PrefName.QuickBooksCompanyFile and isClassRefsEnabled
		///PrefName.QuickBooksClassRefsEnabled.</summary>
		public static void CreateDeposit(string companyPath,DateTime txnDate,string depositAcct,string incomeAcct,double amount,string memo,string classRef,
			bool isClassRefsEnabled)
		{
			try {
				OpenConnection(9,0,companyPath);
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
				throw e;
			}
		}

		///<summary>Creates a deposit within QuickBooks.  A QB connection must be open before calling this method. Requires connection with version 9.0. ClassRef is only used if clinics is turned on.</summary>
		private static void BuildDepositAddRq(DateTime txnDate,string depositAcct,string incomeAcct,double amount,string memo,string classRef,
			bool isClassRefsEnabled)
		{
			if(!ConnectionOpen) {
				return;
			}
			//Build the deposit add request and append it to the request message.
			IDepositAdd DepositAddRq=RequestMsgSet.AppendDepositAddRq();
			//Set field value for TxnDate
			DepositAddRq.TxnDate.SetValue(txnDate);
			//Set field value for FullName.
			DepositAddRq.DepositToAccountRef.FullName.SetValue(depositAcct);
			//Set field value for Memo
			DepositAddRq.Memo.SetValue(memo);
			//Set deposit info attributes.
			IDepositLineAdd DepositLineAdd1=DepositAddRq.DepositLineAddList.Append();
			//Set field value for income account
			DepositLineAdd1.ORDepositLineAdd.DepositInfo.AccountRef.FullName.SetValue(incomeAcct);
			//Set field value for Amount
			DepositLineAdd1.ORDepositLineAdd.DepositInfo.Amount.SetAsString(amount.ToStringNetFx("F"));
			//Set field value for ClassRef which is a way they can group deposits together
			if(!string.IsNullOrEmpty(classRef) && isClassRefsEnabled) {//S3a-14n: PrefC.GetBool(PrefName.QuickBooksClassRefsEnabled)
				DepositLineAdd1.ORDepositLineAdd.DepositInfo.ClassRef.FullName.SetValue(classRef);
			}
		}

		///<summary>Checks if the status code for the deposit is "ok".</summary>
		private static void ValidateDepositAddRs() {
			if(ResponseMsgSet == null) {
				return;
			}
			IResponseList responseList = ResponseMsgSet.ResponseList;
			if(responseList == null) {
				return;
			}
			//if we sent only one request, there is only one response, we'll walk the list for this sample
			for(int i=0;i<responseList.Count;i++) {
				IResponse response = responseList.GetAt(i);
				//check the status code of the response, 0=ok, >0 is warning
				if(response.StatusCode > 0) {
					throw new Exception(response.StatusMessage);
				}
			}
		}
	}
}
