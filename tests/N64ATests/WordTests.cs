// Microsoft Word (N64-A, item A4 of docs/specs/native64-survey.md, row 9): Word is reached out of process through late-bound COM, so the
// 64-bit program uses it natively. The x64 test build compiles the #if !DISABLE_MICROSOFT_OFFICE code (no project file defines the symbol, in
// any configuration or platform), and a broken Word.Application registration (REGDB_E_CLASSNOTREG, CO_E_SERVER_EXEC_FAILURE, TYPE_E_* and
// similar) gives a clear message instead of the raw exception. No test starts Word.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace N64ATests {
	[TestClass]
	public class WordTests {
		///<summary>GetWordApp exists only when DISABLE_MICROSOFT_OFFICE is not defined: the build this test runs from (x86, or the lane's x64 test
		///build) compiled the Word code paths of the business library.</summary>
		[TestMethod]
		public void ThisBuild_CompiledTheWordCode() {
			MethodInfo methodInfo=typeof(LetterMerges).GetMethod("GetWordApp",BindingFlags.Public|BindingFlags.Static);
			Assert.IsNotNull(methodInfo,"LetterMerges.GetWordApp is compiled in this "+(Environment.Is64BitProcess ? "64" : "32")+"-bit test build.");
			Assert.AreEqual(typeof(object),methodInfo.ReturnType,"Late bound: callers use it as dynamic.");
			Assert.IsNotNull(typeof(LetterMerges).GetMethod("CreateWordApp",BindingFlags.NonPublic|BindingFlags.Static));
			Console.WriteLine("Word code compiled; process "+(Environment.Is64BitProcess ? "x64" : "x86")+", "+typeof(LetterMerges).Assembly.Location);
		}

		///<summary>No project, props or targets file of the repository defines DISABLE_MICROSOFT_OFFICE, for any configuration or platform, so the
		///x64 program of stage 3b keeps Word (survey row 9: it "must not be defined in the x64 build").</summary>
		[TestMethod]
		public void NoProjectFile_DefinesDisableMicrosoftOffice() {
			string root=TestSupport.Root();
			List<string> listHits=new List<string>();
			int countFiles=0;
			foreach(string path in TestSupport.EnumerateRepoFiles(root,"*.csproj","*.props","*.targets","*.vcxproj")) {
				countFiles++;
				if(File.ReadAllText(path).IndexOf("DISABLE_MICROSOFT_OFFICE",StringComparison.OrdinalIgnoreCase)>=0) {
					listHits.Add(path.Substring(root.Length+1));
				}
			}
			Assert.IsTrue(countFiles>50,"Expected the project files, found "+countFiles+".");
			Assert.AreEqual(0,listHits.Count,"DISABLE_MICROSOFT_OFFICE is defined in: "+string.Join(", ",listHits));
		}

		[TestMethod]
		public void RegistrationErrors_AreRecognized_AlsoWrapped() {
			foreach(int hResult in new[] { unchecked((int)0x80040154),unchecked((int)0x80080005),unchecked((int)0x8002801D),unchecked((int)0x80029C4A),
				unchecked((int)0x800401F3),unchecked((int)0x80004002) })
			{
				Assert.AreEqual(hResult,LetterMerges.GetWordRegistrationErrorHResult(new COMException("n64a",hResult)),hResult.ToString("X8"));
				Assert.AreEqual(hResult,LetterMerges.GetWordRegistrationErrorHResult(new TargetInvocationException(new COMException("n64a",hResult))));
			}
			//A failed QueryInterface on a COM object is an InvalidCastException (E_NOINTERFACE).
			Assert.AreEqual(unchecked((int)0x80004002),LetterMerges.GetWordRegistrationErrorHResult(new InvalidCastException("n64a")));
			Assert.AreEqual(0,LetterMerges.GetWordRegistrationErrorHResult(new COMException("n64a",unchecked((int)0x80004005))),"E_FAIL is Word's own error.");
			Assert.AreEqual(0,LetterMerges.GetWordRegistrationErrorHResult(new ApplicationException("Microsoft Word is not installed.")));
			Assert.AreEqual(0,LetterMerges.GetWordRegistrationErrorHResult(null));
		}

		[TestMethod]
		public void RegistrationException_HasTheClearMessage_TheCallersShowIt() {
			COMException comException=new COMException("Class not registered",unchecked((int)0x80040154));
			WordRegistrationException wordRegistrationException=new WordRegistrationException(unchecked((int)0x80040154),comException);
			Assert.AreEqual("Microsoft Word is installed, but its registration in Windows is broken, so this program cannot start it (0x80040154, "
				+"REGDB_E_CLASSNOTREG, class not registered). This often follows a change of Office between 32-bit and 64-bit or an incomplete removal of "
				+"another Office version. Repair Office (Windows Settings, Apps, Microsoft Office, Modify, Quick Repair, then Online Repair if needed), then "
				+"try again.",wordRegistrationException.Message);
			Assert.AreSame(comException,wordRegistrationException.InnerException);
			Assert.AreEqual(unchecked((int)0x80040154),wordRegistrationException.HResultRegistration);
			Assert.AreEqual(wordRegistrationException.Message,LetterMerges.GetWordStartFailureText(wordRegistrationException,"Error. Is MS Word installed?"));
			Assert.AreEqual("Error. Is MS Word installed?",LetterMerges.GetWordStartFailureText(new ApplicationException("Microsoft Word is not installed."),
				"Error. Is MS Word installed?"));
			StringAssert.Contains(LetterMerges.GetWordRegistrationErrorText(unchecked((int)0x8002801D)),"(0x8002801D, TYPE_E_LIBNOTREGISTERED, library not registered)");
		}

		///<summary>CreateWordApp and GetWordApp turn a registration error into the WordRegistrationException (logged), and every caller of
		///GetWordApp shows its message.</summary>
		[TestMethod]
		public void Source_CreateWordApp_AndTheCallers() {
			string letterMerges=TestSupport.ReadRepoFile(@"OpenPmsDentalBusiness\Data Interface\LetterMerges.cs");
			Assert.AreEqual(2,CountOf(letterMerges,"catch(Exception ex) when(GetWordRegistrationErrorHResult(ex)!=0) {"));
			Assert.AreEqual(2,CountOf(letterMerges,"throw CreateWordRegistrationException(ex);"));
			StringAssert.Contains(letterMerges,"CodeBase.Logger.WriteLine(wordRegistrationException.Message+\" \"+ex.GetType().Name+\": \"+ex.Message,\"Word\");");
			Assert.AreEqual(3,CountOf(TestSupport.ReadRepoFile(@"OpenPmsDental\Forms\FormLetterMerges.cs"),
				"FriendlyException.Show(LetterMerges.GetWordStartFailureText(ex,Lan.g(this,\"Error. Is MS Word installed?\")),ex);"));
			StringAssert.Contains(TestSupport.ReadRepoFile(@"OpenPmsDental\Logic\ChartLetterL.cs"),
				"FriendlyException.Show(LetterMerges.GetWordStartFailureText(ex,textDefault),ex);");
			StringAssert.Contains(TestSupport.ReadRepoFile(@"OpenPmsDental\Forms\FormLetterMergeEdit.cs"),"if(ex is WordRegistrationException) {");
			int countCallers=0;
			foreach(string relative in new[] { @"OpenPmsDental\Forms\FormLetterMerges.cs",@"OpenPmsDental\Logic\ChartLetterL.cs",
				@"OpenPmsDental\Forms\FormLetterMergeEdit.cs" })
			{
				countCallers+=CountOf(TestSupport.ReadRepoFile(relative),"LetterMerges.GetWordApp()");
			}
			Assert.AreEqual(5,countCallers,"The five callers of GetWordApp (if one is added, it shows the clear message too).");
		}

		private static int CountOf(string text,string value) {
			int count=0;
			for(int index=text.IndexOf(value);index>=0;index=text.IndexOf(value,index+value.Length)) {
				count++;
			}
			return count;
		}
	}
}
