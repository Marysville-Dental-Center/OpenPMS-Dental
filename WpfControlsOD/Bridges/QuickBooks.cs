using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeBase;
using OpenPmsDentalBusiness;
using OpenPmsDentalHelper32.QuickBooks;

namespace OpenPmsDental.Bridges {
	///<summary>Contains all logic for QuickBook connections and requests to the QB company file. Since S3a-13 of the .NET 10 move
	///(docs/port-notes/S3a-13.md) the QBFC session and its requests run in the 32-bit helper program (OpenPmsDentalHelper32, its QuickBooks
	///module), because QBFC 10 is 32-bit only and the program is 64-bit after stage 3b; the public methods, their arguments, results and
	///messages are those of before. An error of the helper itself (missing, not answering, ended) comes as its own message.
	///Since S3a-14n (native first, net10-plan decision 23) the helper is the fallback: the QBFC session runs in the program's own process
	///(QuickBooksInProcess, the code of before S3a-13) whenever QBFC's session manager activates here (NativeFirstChooser.QuickBooksDesktop),
	///which for QBFC 10, a 32-bit in-process COM server, is the case in a 32-bit program; a 64-bit program uses the helper. The choice is made
	///once per program process and logged. (QBFC 15 and later, Intuit's 64-bit SDK for QuickBooks Desktop 2022 and later, is a later package.)
	///</summary>
	public class QuickBooks {
		///<summary>How long one QuickBooks call in the helper may take. QuickBooks asks the user to allow the program's access on its first connection to a company
		///file and the call waits for that answer, as it did in the program's own process, so the wait is long.</summary>
		private static readonly TimeSpan _timeSpanCall=TimeSpan.FromMinutes(30);

		///<summary></summary>
		public QuickBooks() {

		}

		///<summary>Simplest connection test to QB.  Users have to connect to their QB company file with OD and QB running at the same time the first time they connect.  This is just a simple tool to let them get this connection out of the way.  QB will prompt the user to set permissions / access rights for OD and then from there on QB does not need to be open in the background.</summary>
		public static string TestConnection(string companyPath) {
			if(QuickBooksInProcess.IsChosen()) {
				return QuickBooksInProcess.TestConnection(companyPath);
			}
			try {
				return CallHelper(QuickBooksCommands.TEST_CONNECTION,companyPath)[0];
			}
			catch(Exception e) {
				return "Error: "+e.Message;
			}
		}

		///<summary>Returns list of all active accounts.</summary>
		public static List<string> GetListOfAccounts() {
			if(QuickBooksInProcess.IsChosen()) {
				return QuickBooksInProcess.GetListOfAccounts(PrefC.GetString(PrefName.QuickBooksCompanyFile));
			}
			return CallHelper(QuickBooksCommands.GET_LIST_OF_ACCOUNTS,PrefC.GetString(PrefName.QuickBooksCompanyFile)).ToList();
		}

		///<summary>Returns list of all QB "class" names.</summary>
		public static List<string> GetListOfClasses() {
			if(QuickBooksInProcess.IsChosen()) {
				return QuickBooksInProcess.GetListOfClasses(PrefC.GetString(PrefName.QuickBooksCompanyFile));
			}
			return CallHelper(QuickBooksCommands.GET_LIST_OF_CLASSES,PrefC.GetString(PrefName.QuickBooksCompanyFile)).ToList();
		}

		///<summary>Creates a deposit within QuickBooks.</summary>
		public static void CreateDeposit(DateTime txnDate,string depositAcct,string incomeAcct,double amount,string memo,string classRef) {
			if(QuickBooksInProcess.IsChosen()) {
				QuickBooksInProcess.CreateDeposit(PrefC.GetString(PrefName.QuickBooksCompanyFile),txnDate,depositAcct,incomeAcct,amount,memo,classRef,
					PrefC.GetBool(PrefName.QuickBooksClassRefsEnabled));
				return;
			}
			CallHelper(QuickBooksCommands.CREATE_DEPOSIT,GetCreateDepositArgs(PrefC.GetString(PrefName.QuickBooksCompanyFile),txnDate,depositAcct,
				incomeAcct,amount,memo,classRef));
		}

		///<summary>The helper's arguments of CreateDeposit (QuickBooksCommands.CREATE_DEPOSIT). The amount is formatted here, in the program's
		///culture, as BuildDepositAddRq did ("F"); ClassRef is only used if clinics is turned on, as before: a class was given and
		///PrefName.QuickBooksClassRefsEnabled is on.</summary>
		private static string[] GetCreateDepositArgs(string companyPath,DateTime txnDate,string depositAcct,string incomeAcct,double amount,string memo,
			string classRef)
		{
			bool isClassRefUsed=!string.IsNullOrEmpty(classRef) && PrefC.GetBool(PrefName.QuickBooksClassRefsEnabled);
			return new[] { companyPath,QuickBooksCommands.DateToText(txnDate),depositAcct,incomeAcct,amount.ToString("F"),memo,classRef,
				QuickBooksCommands.BoolToText(isClassRefUsed) };
		}

		///<summary>Runs one command of the helper's QuickBooks module and returns its results. The module's own errors (QBFC's and QuickBooks'
		///messages) come with their messages, as when QBFC ran in the program.</summary>
		private static string[] CallHelper(string command,params string[] args) {
			return Helper32Client.Default.Call(QuickBooksCommands.MODULE,command,_timeSpanCall,args);
		}
	}
}
