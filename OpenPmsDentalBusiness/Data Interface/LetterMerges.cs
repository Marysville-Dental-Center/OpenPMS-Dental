using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace OpenPmsDentalBusiness{
	///<summary></summary>
	public class LetterMerges {
		#region CachePattern

		private class LetterMergeCache : CacheListAbs<LetterMerge> {
			protected override List<LetterMerge> GetCacheFromDb() {
				string command="SELECT * FROM lettermerge ORDER BY Description";
				List<LetterMerge> listLetterMerges=Crud.LetterMergeCrud.SelectMany(command);
				return listLetterMerges;
			}
			protected override List<LetterMerge> TableToList(DataTable table) {
				return Crud.LetterMergeCrud.TableToList(table);
			}
			protected override LetterMerge Copy(LetterMerge letterMerge) {
				return letterMerge.Copy();
			}
			protected override DataTable ListToTable(List<LetterMerge> listLetterMerges) {
				return Crud.LetterMergeCrud.ListToTable(listLetterMerges,"LetterMerge");
			}
			protected override void FillCacheIfNeeded() {
				LetterMerges.GetTableFromCache(false);
			}
		}
		
		///<summary>The object that accesses the cache in a thread-safe manner.</summary>
		private static LetterMergeCache _letterMergeCache=new LetterMergeCache();

		public static List<LetterMerge> GetWhere(Predicate<LetterMerge> match,bool isShort=false) {
			return _letterMergeCache.GetWhere(match,isShort);
		}

		///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
		public static DataTable RefreshCache() {
			return GetTableFromCache(true);
		}

		///<summary>Fills the local cache with the passed in DataTable.</summary>
		public static void FillCacheFromTable(DataTable table) {
			_letterMergeCache.FillCacheFromTable(table);
		}

		///<summary>Always refreshes the ClientWeb's cache.</summary>
		public static DataTable GetTableFromCache(bool doRefreshCache) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				DataTable table=Meth.GetTable(MethodBase.GetCurrentMethod(),doRefreshCache);
				_letterMergeCache.FillCacheFromTable(table);
				return table;
			}
			return _letterMergeCache.GetTableFromCache(doRefreshCache);
		}

		public static void ClearCache() {
			_letterMergeCache.ClearCache();
		}
		#endregion

		#region Word automation
		//Word's automation interface is late bound (callers use dynamic), so no Office interop assembly (Interop.Word, Interop.Microsoft.Office.Core,
		//Interop.VBIDE, stdole) is referenced or shipped. These are the values of Word's own enumerations that the letter merge windows use.
		///<summary>WdMailMergeDestination.wdSendToNewDocument.</summary>
		public const int WORD_MAIL_MERGE_TO_NEW_DOCUMENT=0;
		///<summary>WdMailMergeDestination.wdSendToPrinter.</summary>
		public const int WORD_MAIL_MERGE_TO_PRINTER=1;
		///<summary>WdWindowState.wdWindowStateMaximize.</summary>
		public const int WORD_WINDOW_STATE_MAXIMIZE=1;
		///<summary>WdWindowState.wdWindowStateMinimize.</summary>
		public const int WORD_WINDOW_STATE_MINIMIZE=2;

#if !DISABLE_MICROSOFT_OFFICE
		private static object _word_Application;
		///<summary>Tests only (CLEAN-1): when set, CreateWordApp returns what this returns instead of starting Word.</summary>
		internal static Func<object> FuncCreateWordAppForTests;
		///<summary>Tests only (CLEAN-1): when set, ReleaseWordApp passes the object to this instead of releasing it.</summary>
		internal static Action<object> ActionReleaseWordAppForTests;

		///<summary>This is a static reference to a word application. That way, we can reuse it instead of having to reopen Word each time. Surround with try/catch. Will throw exception if Word is not installed.
		///The object is Word's "Word.Application" automation object, late bound: callers use it as dynamic.</summary>
		public static object GetWordApp() {
			Meth.NoCheckMiddleTierRole();
			bool isWordNew=false;
			if(_word_Application==null) {
				_word_Application=CreateWordApp();
				isWordNew=true;
			}
			try {
				_word_Application.GetType().InvokeMember("Activate",BindingFlags.InvokeMethod,null,_word_Application,null);
			}
			catch(Exception ex) when(isWordNew && GetWordRegistrationErrorHResult(ex)!=0) {
				//The Word just started fails its first call because its registration is broken. Another start would meet the same registration
				//and leave a second Word running before the message, so release this one and give the message now (CLEAN-1).
				ReleaseWordApp();
				throw CreateWordRegistrationException(ex);
			}
			catch {
				//The object of an earlier call no longer works (the user closed Word, for example): start Word again.
				_word_Application=CreateWordApp();
				try {
					_word_Application.GetType().InvokeMember("Activate",BindingFlags.InvokeMethod,null,_word_Application,null);
				}
				catch(Exception ex) when(GetWordRegistrationErrorHResult(ex)!=0) {
					ReleaseWordApp();
					throw CreateWordRegistrationException(ex);
				}
			}
			return _word_Application;
		}

		///<summary>Releases the kept Word object (it failed with a registration error) and keeps none, so the next call starts Word again. Never
		///throws.</summary>
		private static void ReleaseWordApp() {
			object application=_word_Application;
			_word_Application=null;
			ReleaseComObjectQuietly(application);
		}

		///<summary>Releases a Word object that failed with a registration error, so its Word process ends now rather than when the garbage
		///collector finalizes it. Never throws.</summary>
		private static void ReleaseComObjectQuietly(object application) {
			if(ActionReleaseWordAppForTests!=null) {
				ActionReleaseWordAppForTests(application);
				return;
			}
			try {
				if(application!=null && System.Runtime.InteropServices.Marshal.IsComObject(application)) {
					System.Runtime.InteropServices.Marshal.ReleaseComObject(application);
				}
			}
			catch(Exception exRelease) {
				CodeBase.ODExceptionExtensions.DoNothing(exRelease);
			}
		}

		///<summary>Starts the word processor registered for "Word.Application" and makes it visible. Throws if none is installed, and a
		///WordRegistrationException with a clear message when Word's registration in Windows is broken (N64-A).</summary>
		private static object CreateWordApp() {
			if(FuncCreateWordAppForTests!=null) {
				return FuncCreateWordAppForTests();
			}
			Type type=Type.GetTypeFromProgID("Word.Application");
			if(type==null) {
				throw new ApplicationException("Microsoft Word is not installed.");
			}
			object application;
			try {
				application=Activator.CreateInstance(type);
			}
			catch(Exception ex) when(GetWordRegistrationErrorHResult(ex)!=0) {
				throw CreateWordRegistrationException(ex);
			}
			return InitWordApp(application);
		}

		///<summary>Makes the Word object just started visible and returns it. Its first late-bound call is the one that meets a broken type
		///library or interface registration: then the object is released, so no hidden Word process stays running (it could get in the way of
		///the Office repair the message asks for), and the WordRegistrationException is thrown (CLEAN-1).</summary>
		internal static object InitWordApp(object application) {
			try {
				application.GetType().InvokeMember("Visible",BindingFlags.SetProperty,null,application,new object[] { true });
				return application;
			}
			catch(Exception ex) when(GetWordRegistrationErrorHResult(ex)!=0) {
				ReleaseComObjectQuietly(application);
				throw CreateWordRegistrationException(ex);
			}
		}

		///<summary>The WordRegistrationException for ex, written to the log (Logger, sub folder Word) with the HRESULT and ex's text.</summary>
		private static WordRegistrationException CreateWordRegistrationException(Exception ex) {
			WordRegistrationException wordRegistrationException=new WordRegistrationException(GetWordRegistrationErrorHResult(ex),ex);
			try {
				CodeBase.Logger.WriteLine(wordRegistrationException.Message+" "+ex.GetType().Name+": "+ex.Message,"Word");
			}
			catch(Exception exLog) {
				CodeBase.ODExceptionExtensions.DoNothing(exLog);
			}
			return wordRegistrationException;
		}
#endif

		///<summary>The HRESULTs that mean Word's COM registration in Windows is broken rather than Word failing (N64-A, item A4 of
		///docs/specs/native64-survey.md, row 9): the class or its server is not registered or cannot start, or the type library or an interface
		///is not registered (orphaned TypeLib or Interface keys, for example after Office was changed from 32-bit to 64-bit or removed badly:
		///https://learn.microsoft.com/en-us/office365/troubleshoot/administration/com-net-errors-after-migration). Word is reached out of process,
		///so this program's bitness is not the cause and the 32-bit helper would meet the same registration.</summary>
		public static readonly Dictionary<int,string> DictWordRegistrationErrors=new Dictionary<int,string> {
			{ unchecked((int)0x80040154),"REGDB_E_CLASSNOTREG, class not registered" },
			{ unchecked((int)0x80040155),"REGDB_E_IIDNOTREG, interface not registered" },
			{ unchecked((int)0x800401F3),"CO_E_CLASSSTRING, invalid class string" },
			{ unchecked((int)0x80080005),"CO_E_SERVER_EXEC_FAILURE, server execution failed" },
			{ unchecked((int)0x80004002),"E_NOINTERFACE, no such interface supported" },
			{ unchecked((int)0x8002801C),"TYPE_E_REGISTRYACCESS, error accessing the registry" },
			{ unchecked((int)0x8002801D),"TYPE_E_LIBNOTREGISTERED, library not registered" },
			{ unchecked((int)0x80029C4A),"TYPE_E_CANTLOADLIBRARY, error loading type library" },
		};

		///<summary>The HRESULT of ex, or of the first exception it wraps, that is one of DictWordRegistrationErrors (a COMException, or the
		///InvalidCastException that a failed QueryInterface gives), or 0 when there is none.</summary>
		public static int GetWordRegistrationErrorHResult(Exception ex) {
			for(Exception exception=ex;exception!=null;exception=exception.InnerException) {
				if((exception is System.Runtime.InteropServices.COMException || exception is InvalidCastException)
					&& DictWordRegistrationErrors.ContainsKey(exception.HResult))
				{
					return exception.HResult;
				}
			}
			return 0;
		}

		///<summary>The message for a broken Word registration with that HRESULT, naming it and what to do.</summary>
		public static string GetWordRegistrationErrorText(int hResult) {
			DictWordRegistrationErrors.TryGetValue(hResult,out string name);
			return "Microsoft Word is installed, but its registration in Windows is broken, so this program cannot start it (0x"+hResult.ToString("X8")
				+(string.IsNullOrEmpty(name) ? "" : ", "+name)+"). This often follows a change of Office between 32-bit and 64-bit or an incomplete "
				+"removal of another Office version. Repair Office (Windows Settings, Apps, Microsoft Office, Modify, Quick Repair, then Online Repair "
				+"if needed), then try again.";
		}

		///<summary>For a caller that could not start Word: the clear message of a WordRegistrationException (its Message), otherwise
		///textDefault, the caller's own message as before.</summary>
		public static string GetWordStartFailureText(Exception ex,string textDefault) {
			if(ex is WordRegistrationException) {
				return ex.Message;
			}
			return textDefault;
		}
		#endregion Word automation

		///<summary>Inserts this lettermerge into database.</summary>
		public static long Insert(LetterMerge letterMerge) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				letterMerge.LetterMergeNum=Meth.GetLong(MethodBase.GetCurrentMethod(),letterMerge);
				return letterMerge.LetterMergeNum;
			}
			return Crud.LetterMergeCrud.Insert(letterMerge);
		}

		///<summary></summary>
		public static void Update(LetterMerge letterMerge){
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),letterMerge);
				return;
			}
			Crud.LetterMergeCrud.Update(letterMerge);
		}

		///<summary></summary>
		public static void Delete(LetterMerge letterMerge){
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),letterMerge);
				return;
			}
			string command="DELETE FROM lettermerge "
				+"WHERE LetterMergeNum = "+POut.Long(letterMerge.LetterMergeNum);
			Db.NonQ(command);
		}

		///<summary>Supply the index of the cat within Defs.Short.</summary>
		public static List<LetterMerge> GetListForCat(int catIndex){
			Meth.NoCheckMiddleTierRole();
			long defNum=Defs.GetDefsForCategory(DefCat.LetterMergeCats,true)[catIndex].DefNum;
			return GetWhere(x => x.Category==defNum);
		}
	}

	



	///<summary>Word is installed (its ProgID is registered) but cannot be started or called because its COM registration in Windows is broken
	///(LetterMerges.DictWordRegistrationErrors). Message is the clear text for the user (LetterMerges.GetWordRegistrationErrorText).</summary>
	public class WordRegistrationException:ApplicationException {
		///<summary>The HRESULT that showed the broken registration.</summary>
		public int HResultRegistration { get; }

		public WordRegistrationException(int hResult,Exception innerException)
			:base(LetterMerges.GetWordRegistrationErrorText(hResult),innerException)
		{
			HResultRegistration=hResult;
		}
	}
}









