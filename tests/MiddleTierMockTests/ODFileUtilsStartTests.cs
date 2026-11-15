// Package S3a-2 of the .NET 10 plan (docs/specs/net10-plan.md, the S3a-2 row; docs/port-notes/S3a-2.md): every start of CodeBase ODFileUtils sets
// UseShellExecute explicitly. On .NET Framework it is true by default and on .NET false, and only the shell opens documents and URLs and asks
// for consent to a program that needs elevation. The tests replace ODFileUtils' starters and look at the ProcessStartInfo that would be started:
// no program is started, no file or URL is opened and no window is shown. Files are written only in a scratch folder in the temp folder.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MiddleTierMockTests {
	[TestClass]
	public class ODFileUtilsStartTests {
		///<summary>The starters are static: one test at a time replaces them.</summary>
		private static readonly object _lockStarters=new object();
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

		///<summary>Runs action with both starters of ODFileUtils replaced by recorders that start nothing, and puts the real ones back.</summary>
		private void RunRecorded(Action action) {
			lock(_lockStarters) {
				Func<ProcessStartInfo,Process> funcStartProcessInfoOld=ODFileUtils.FuncStartProcessInfo;
				Func<Process,bool> funcStartProcessOld=ODFileUtils.FuncStartProcess;
				Func<ProcessStartInfo,Process> funcRecordInfo=x => { _listStartInfos.Add(x); return null; };
				Func<Process,bool> funcRecordProcess=x => { _listProcesses.Add(x); return false; };
				ODFileUtils.FuncStartProcessInfo=funcRecordInfo;
				ODFileUtils.FuncStartProcess=funcRecordProcess;
				try {
					//Nothing may be started: make sure the recorders are in place before the call.
					Assert.AreSame(funcRecordInfo,ODFileUtils.FuncStartProcessInfo);
					Assert.AreSame(funcRecordProcess,ODFileUtils.FuncStartProcess);
					action();
				}
				finally {
					ODFileUtils.FuncStartProcessInfo=funcStartProcessInfoOld;
					ODFileUtils.FuncStartProcess=funcStartProcessOld;
				}
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
			//Not called (that would start a program): the defaults are the private methods that call Process.Start.
			lock(_lockStarters) {
				Assert.AreEqual("StartProcessInfoDefault",ODFileUtils.FuncStartProcessInfo.Method.Name);
				Assert.AreEqual(typeof(ODFileUtils),ODFileUtils.FuncStartProcessInfo.Method.DeclaringType);
				Assert.AreEqual("StartProcessDefault",ODFileUtils.FuncStartProcess.Method.Name);
				Assert.AreEqual(typeof(ODFileUtils),ODFileUtils.FuncStartProcess.Method.DeclaringType);
			}
		}
	}
}
