// Package S3a-2 of the .NET 10 plan (docs/specs/net10-plan.md, the S3a-2 row; docs/port-notes/S3a-2.md): every start of CodeBase ODFileUtils sets
// UseShellExecute explicitly. On .NET Framework it is true by default and on .NET false, and only the shell opens documents and URLs and asks
// for consent to a program that needs elevation. The tests replace ODFileUtils' starters and look at the ProcessStartInfo that would be started:
// no program is started, no file or URL is opened and no window is shown. Files are written only in a scratch folder in the temp folder.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeBase;
using CodeBase.Controls;
using CodeBase.VendorDirect;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MiddleTierMockTests {
	[TestClass]
	public class ODFileUtilsStartTests {
		private List<ProcessStartInfo> _listStartInfos;
		private List<Process> _listProcesses;
		private string _pathFolder;

		[TestInitialize]
		public void Initialize() {
			_listStartInfos=new List<ProcessStartInfo>();
			_listProcesses=new List<Process>();
			_pathFolder=Path.Combine(Path.GetTempPath(),"S3a2Start_"+Guid.NewGuid().ToString("N").Substring(0,12));
			Directory.CreateDirectory(_pathFolder);
		}

		[TestCleanup]
		public void Cleanup() {
			try {
				Directory.Delete(_pathFolder,true);
			}
			catch(IOException) {
			}
			catch(UnauthorizedAccessException) {
			}
		}

		///<summary>Runs action with both starters of ODFileUtils replaced by recorders that start nothing, and puts the real ones back. The
		///replacement is ODFileUtils' own scoped one (S3a-2f): one at a time in the whole test process, not only in this class.</summary>
		private void RunRecorded(Action action) {
			Func<ProcessStartInfo,Process> funcRecordInfo=x => { _listStartInfos.Add(x); return null; };
			Func<Process,bool> funcRecordProcess=x => { _listProcesses.Add(x); return false; };
			using(ODFileUtils.ReplaceStartersForTests(funcRecordInfo,funcRecordProcess)) {
				//Nothing may be started: make sure the recorders are in place before the call.
				Assert.AreSame(funcRecordInfo,ODFileUtils.FuncStartProcessInfo);
				Assert.AreSame(funcRecordProcess,ODFileUtils.FuncStartProcess);
				action();
			}
		}

		private ProcessStartInfo GetSingleShellStart(string fileNameExpected,string argumentsExpected) {
			Assert.AreEqual(1,_listStartInfos.Count,"Exactly one start.");
			Assert.AreEqual(0,_listProcesses.Count);
			ProcessStartInfo processStartInfo=_listStartInfos[0];
			Assert.IsTrue(processStartInfo.UseShellExecute,"Every start of a program, document or URL goes through the shell.");
			Assert.AreEqual(fileNameExpected,processStartInfo.FileName);
			Assert.AreEqual(argumentsExpected,processStartInfo.Arguments);
			Assert.IsFalse(processStartInfo.RedirectStandardInput);
			Assert.IsFalse(processStartInfo.RedirectStandardOutput);
			Assert.IsFalse(processStartInfo.RedirectStandardError);
			Assert.IsFalse(processStartInfo.CreateNoWindow,"As Process.Start(string,string) on .NET Framework.");
			Assert.AreEqual(ProcessWindowStyle.Normal,processStartInfo.WindowStyle);
			Assert.AreEqual("",processStartInfo.WorkingDirectory);
			Assert.AreEqual("",processStartInfo.Verb);
			return processStartInfo;
		}

		[TestMethod]
		public void ProcessStart_PathOnly_UsesTheShell() {
			RunRecorded(() => Assert.IsNull(ODFileUtils.ProcessStart(@"C:\Program Files\Example Vendor\Example.exe")));
			GetSingleShellStart(@"C:\Program Files\Example Vendor\Example.exe","");
		}

		[TestMethod]
		public void ProcessStart_PathAndArguments_UsesTheShell() {
			RunRecorded(() => ODFileUtils.ProcessStart("Example.exe","/patient \"Doe, Jane\" -id 42"));
			GetSingleShellStart("Example.exe","/patient \"Doe, Jane\" -id 42");
		}

		[TestMethod]
		public void ProcessStart_CreateDirIfNeeded_CreatesItThenUsesTheShell() {
			string pathDir=Path.Combine(_pathFolder,"Export");
			RunRecorded(() => ODFileUtils.ProcessStart("Example.exe","-x",pathDir));
			Assert.IsTrue(Directory.Exists(pathDir));
			GetSingleShellStart("Example.exe","-x");
		}

		[TestMethod]
		public void ProcessStart_NullArguments_AsProcessStartOfTwoStrings() {
			RunRecorded(() => ODFileUtils.ProcessStart("Example.exe",null));
			GetSingleShellStart("Example.exe","");
		}

		[TestMethod]
		public void ShellOpen_Url_UsesTheShell() {
			RunRecorded(() => Assert.IsNull(ODFileUtils.ShellOpen("https://www.example.test/help?page=1&x=2")));
			GetSingleShellStart("https://www.example.test/help?page=1&x=2","");
		}

		[TestMethod]
		public void ShellOpen_Document_UsesTheShell() {
			string pathFile=Path.Combine(_pathFolder,"letter.pdf");
			RunRecorded(() => ODFileUtils.ShellOpen(pathFile));
			GetSingleShellStart(pathFile,"");
		}

		[TestMethod]
		public void ShellOpen_Folder_UsesTheShell() {
			RunRecorded(() => ODFileUtils.ShellOpen(_pathFolder));
			GetSingleShellStart(_pathFolder,"");
		}

		[TestMethod]
		public void GetShellStartInfo_IsTheShellStart() {
			ProcessStartInfo processStartInfo=ODFileUtils.GetShellStartInfo("Example.exe","-a");
			_listStartInfos.Add(processStartInfo);
			GetSingleShellStart("Example.exe","-a");
		}

		[TestMethod]
		public void WriteAllTextThenStart_ThreeOverloads_WriteThenUseTheShell() {
			string pathFile=Path.Combine(_pathFolder,"bridge.txt");
			RunRecorded(() => ODFileUtils.WriteAllTextThenStart(pathFile,"one","Example.exe"));
			Assert.AreEqual("one",File.ReadAllText(pathFile));
			GetSingleShellStart("Example.exe","");
			_listStartInfos.Clear();
			RunRecorded(() => ODFileUtils.WriteAllTextThenStart(pathFile,"two","Example.exe","-f \""+pathFile+"\""));
			Assert.AreEqual("two",File.ReadAllText(pathFile));
			GetSingleShellStart("Example.exe","-f \""+pathFile+"\"");
			_listStartInfos.Clear();
			RunRecorded(() => ODFileUtils.WriteAllTextThenStart(pathFile,"thr\u00e9e",Encoding.Unicode,"Example.exe","-u"));
			Assert.AreEqual("thr\u00e9e",File.ReadAllText(pathFile,Encoding.Unicode));
			GetSingleShellStart("Example.exe","-u");
		}

		[TestMethod]
		public void WriteAllBytesThenStart_WithProgram_StartsTheProgramThroughTheShell() {
			string pathFile=Path.Combine(_pathFolder,"bridge.bin");
			RunRecorded(() => ODFileUtils.WriteAllBytesThenStart(pathFile,new byte[] { 1,2,3 },"Example.exe"));
			CollectionAssert.AreEqual(new byte[] { 1,2,3 },File.ReadAllBytes(pathFile));
			GetSingleShellStart("Example.exe","");
			_listStartInfos.Clear();
			RunRecorded(() => ODFileUtils.WriteAllBytesThenStart(pathFile,new byte[] { 4 },"Example.exe","-b",1));
			GetSingleShellStart("Example.exe","-b");
		}

		[TestMethod]
		public void WriteAllBytesThenStart_WithoutProgram_OpensTheFileThroughTheShell() {
			string pathFile=Path.Combine(_pathFolder,"bridge.xml");
			RunRecorded(() => ODFileUtils.WriteAllBytesThenStart(pathFile,new byte[] { 60,97,47,62 },"","-o"));
			GetSingleShellStart(pathFile,"-o");
		}

		[TestMethod]
		public void ProcessStart_StartInfo_IsStartedAsTheCallerSetIt() {
			ProcessStartInfo processStartInfo=new ProcessStartInfo("Example.exe","-r");
			processStartInfo.UseShellExecute=false;
			processStartInfo.RedirectStandardOutput=true;
			RunRecorded(() => Assert.IsNull(ODFileUtils.ProcessStart(processStartInfo)));
			Assert.AreEqual(1,_listStartInfos.Count);
			Assert.AreSame(processStartInfo,_listStartInfos[0]);
			Assert.IsFalse(_listStartInfos[0].UseShellExecute,"The caller's explicit choice stays.");
			Assert.IsTrue(_listStartInfos[0].RedirectStandardOutput);
		}

		[TestMethod]
		public void ProcessStart_Process_IsStartedAsTheCallerSetIt() {
			using(Process process=new Process()) {
				process.StartInfo.FileName="Example.exe";
				process.StartInfo.UseShellExecute=true;
				RunRecorded(() => ODFileUtils.ProcessStart(process));
				Assert.AreEqual(0,_listStartInfos.Count);
				Assert.AreEqual(1,_listProcesses.Count);
				Assert.AreSame(process,_listProcesses[0]);
				Assert.IsTrue(process.StartInfo.UseShellExecute);
				Assert.AreEqual("Example.exe",process.StartInfo.FileName);
			}
		}

		[TestMethod]
		public void Starters_DefaultToTheRealStart() {
			//Not called (that would start a program): the defaults are the private methods that call Process.Start. Read as the starters a
			//replacement replaced, so no other test can have its recorders in place meanwhile.
			using(ODFileUtils.StartersReplacement startersReplacement=ODFileUtils.ReplaceStartersForTests(x => null,x => false)) {
				Assert.AreEqual("StartProcessInfoDefault",startersReplacement.FuncStartProcessInfoReplaced.Method.Name);
				Assert.AreEqual(typeof(ODFileUtils),startersReplacement.FuncStartProcessInfoReplaced.Method.DeclaringType);
				Assert.AreEqual("StartProcessDefault",startersReplacement.FuncStartProcessReplaced.Method.Name);
				Assert.AreEqual(typeof(ODFileUtils),startersReplacement.FuncStartProcessReplaced.Method.DeclaringType);
			}
		}

		[TestMethod]
		public void Starters_AreNotPublic_AndSetOnlyByTheScopedReplacement() {
			//S3a-2f: no code outside CodeBase and this assembly sees the starters, and nothing assigns them except ReplaceStartersForTests.
			foreach(string name in new[] { "FuncStartProcessInfo","FuncStartProcess" }) {
				Assert.IsNull(typeof(ODFileUtils).GetField(name,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static),name+" is no field.");
				Assert.IsNull(typeof(ODFileUtils).GetProperty(name,BindingFlags.Public|BindingFlags.Static),name+" is not public.");
				PropertyInfo propertyInfo=typeof(ODFileUtils).GetProperty(name,BindingFlags.NonPublic|BindingFlags.Static);
				Assert.IsNotNull(propertyInfo,name);
				Assert.IsTrue(propertyInfo.GetMethod.IsAssembly,name+" is internal.");
				Assert.IsTrue(propertyInfo.SetMethod.IsPrivate,name+" is set only inside ODFileUtils.");
			}
			MethodInfo methodInfo=typeof(ODFileUtils).GetMethod("ReplaceStartersForTests",BindingFlags.NonPublic|BindingFlags.Static);
			Assert.IsNotNull(methodInfo);
			Assert.IsTrue(methodInfo.IsAssembly);
		}

		[TestMethod]
		public void ReplaceStarters_Dispose_PutsTheReplacedStartersBack() {
			Func<ProcessStartInfo,Process> funcRecordInfo=x => null;
			Func<Process,bool> funcRecordProcess=x => false;
			ODFileUtils.StartersReplacement startersReplacementFirst=ODFileUtils.ReplaceStartersForTests(funcRecordInfo,funcRecordProcess);
			try {
				Assert.AreSame(funcRecordInfo,ODFileUtils.FuncStartProcessInfo);
				Assert.AreSame(funcRecordProcess,ODFileUtils.FuncStartProcess);
			}
			finally {
				startersReplacementFirst.Dispose();
			}
			startersReplacementFirst.Dispose();//A second Dispose neither puts anything back nor lets a second replacement in.
			//The next replacement replaces what the first put back: the real starters, whoever replaced them in between (each puts them back).
			using(ODFileUtils.StartersReplacement startersReplacementSecond=ODFileUtils.ReplaceStartersForTests(x => null,x => false)) {
				Assert.AreSame(startersReplacementFirst.FuncStartProcessInfoReplaced,startersReplacementSecond.FuncStartProcessInfoReplaced);
				Assert.AreSame(startersReplacementFirst.FuncStartProcessReplaced,startersReplacementSecond.FuncStartProcessReplaced);
				Assert.AreEqual("StartProcessInfoDefault",startersReplacementSecond.FuncStartProcessInfoReplaced.Method.Name);
				//The doubled Dispose did not release twice: a replacement on another thread still waits for this one.
				Task<ODFileUtils.StartersReplacement> taskThird=Task.Run(() => ODFileUtils.ReplaceStartersForTests(x => null,x => false));
				bool isThirdTaken=false;
				try {
					bool isThirdWaiting=!taskThird.Wait(300);
					startersReplacementSecond.Dispose();
					Assert.IsTrue(taskThird.Wait(TimeSpan.FromSeconds(30)),"It goes in once the first is disposed.");
					ODFileUtils.StartersReplacement startersReplacementThird=taskThird.Result;
					isThirdTaken=true;
					using(startersReplacementThird) {
						Assert.AreEqual("StartProcessInfoDefault",startersReplacementThird.FuncStartProcessInfoReplaced.Method.Name,
							"It replaced the real starters, which the first put back before letting it in.");
					}
					Assert.IsTrue(isThirdWaiting,"A second replacement waits while one is in place.");
				}
				finally {
					if(!isThirdTaken) {
						//Its wait timed out (or failed before the using): dispose its replacement once it is in, so that every later replacement in
						//this test process does not wait two minutes for it and throw (S3a-FU2).
						startersReplacementSecond.Dispose();
						taskThird.ContinueWith(x => {
							if(x.Status==TaskStatus.RanToCompletion) {
								x.Result.Dispose();
							}
						},TaskScheduler.Default);
					}
				}
			}
		}

		[TestMethod]
		public void ReplaceStarters_NullStarter_ThrowsAndHoldsNothing() {
			Assert.ThrowsExactly<ArgumentNullException>(() => ODFileUtils.ReplaceStartersForTests(null,x => false));
			Assert.ThrowsExactly<ArgumentNullException>(() => ODFileUtils.ReplaceStartersForTests(x => null,null));
			using(ODFileUtils.StartersReplacement startersReplacement=ODFileUtils.ReplaceStartersForTests(x => null,x => false)) {
				Assert.AreEqual("StartProcessDefault",startersReplacement.FuncStartProcessReplaced.Method.Name);
			}
		}

		[TestMethod]
		public void SystemBrowser_Open_GoesThroughTheRecordedShellStart() {
			//S3a-2f item 4: the vendor sign-in page opens through ODFileUtils.ProcessStart(ProcessStartInfo), so it can be recorded.
			RunRecorded(() => SystemBrowser.Open(" https://vendor.example.test/sign-in?state=1 "));
			GetSingleShellStart("https://vendor.example.test/sign-in?state=1","");
		}

		[TestMethod]
		public void WebView2Policy_OpenInSystemBrowser_GoesThroughTheRecordedShellStart() {
			RunRecorded(() => Assert.IsTrue(WebView2Policy.OpenInSystemBrowser("https://www.example.test/page")));
			GetSingleShellStart("https://www.example.test/page","");
			_listStartInfos.Clear();
			RunRecorded(() => Assert.IsTrue(WebView2Policy.OpenInSystemBrowser("mailto:someone@example.test")));
			GetSingleShellStart("mailto:someone@example.test","");
		}
	}
}
