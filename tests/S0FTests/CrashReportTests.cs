// FB-1: crash reports (CodeBase.CrashReport). An unhandled exception writes one text file with the exception chain, the program, the
// computer, the process's memory and handle counts, the program's state and the last actions, so a crash can be debugged later. The tooltip
// crash of 2026-11-25 left only Windows' event log entry, because a WPF exception on the UI thread had no handler. Tested on a temporary
// folder; the wiring of ProgramEntry by its source. No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace S0FTests {
	[TestClass]
	public class CrashReportTests {
		private string _folder;

		[TestInitialize]
		public void Init() {
			_folder=Path.Combine(Path.GetTempPath(),"CrashReportTests-"+Guid.NewGuid().ToString("N"));
			CrashReport.FolderOverride=_folder;
			CrashReport.FuncAppState=null;
		}

		[TestCleanup]
		public void Cleanup() {
			CrashReport.FolderOverride=null;
			CrashReport.FuncAppState=null;
			try {
				Directory.Delete(_folder,true);
			}
			catch {
			}
		}

		///<summary>An exception with a stack, an inner exception and a data entry, as a real one has.</summary>
		private static Exception MakeException() {
			try {
				try {
					throw new InvalidOperationException("This Visual is not connected to a PresentationSource.");
				}
				catch(Exception exInner) {
					ApplicationException exception=new ApplicationException("Outer failure",exInner);
					exception.Data["SlotIndex"]=3;
					throw exception;
				}
			}
			catch(Exception ex) {
				return ex;
			}
		}

		[TestMethod]
		public void Write_HasTheChain_TheProgram_TheState_AndTheLastActions() {
			CrashReport.Note("Module: Imaging (clicked)");
			CrashReport.Note("Window opened: FormVideo");
			CrashReport.FuncAppState=() => new List<string> { "Module: Imaging","PatNum: 42" };
			Exception exception=MakeException();
			string path=CrashReport.Write(exception,"WPF dispatcher",isTerminating:false);
			Assert.AreNotEqual("",path,"A report is written.");
			Assert.IsTrue(File.Exists(path));
			StringAssert.StartsWith(Path.GetFileName(path),"crash-");
			string text=File.ReadAllText(path);
			StringAssert.Contains(text,"Caught by: WPF dispatcher");
			StringAssert.Contains(text,"Exception: System.ApplicationException: Outer failure");
			StringAssert.Contains(text,"Inner exception 1:");
			StringAssert.Contains(text,"Type: System.InvalidOperationException");
			StringAssert.Contains(text,"This Visual is not connected to a PresentationSource.");
			StringAssert.Contains(text,"HResult: 0x");
			StringAssert.Contains(text,"Data: SlotIndex = 3");
			StringAssert.Contains(text,"MakeException","The stack is written.");
			StringAssert.Contains(text,"--- Program ---");
			StringAssert.Contains(text,"64-bit process");
			StringAssert.Contains(text,"--- Computer ---");
			StringAssert.Contains(text,".NET: ");
			StringAssert.Contains(text,"GDI objects: ");
			StringAssert.Contains(text,"USER objects: ");
			StringAssert.Contains(text,"PatNum: 42");
			StringAssert.Contains(text,"Window opened: FormVideo");
			Assert.IsTrue(text.IndexOf("Module: Imaging (clicked)")<text.IndexOf("Window opened: FormVideo"),"The last actions, oldest first.");
		}

		[TestMethod]
		public void Write_SameExceptionTwice_OneReport() {
			Exception exception=MakeException();
			Assert.AreNotEqual("",CrashReport.Write(exception,"WPF dispatcher",isTerminating:false));
			Assert.AreEqual("",CrashReport.Write(exception,"AppDomain.UnhandledException",isTerminating:true),"The same exception from a second handler.");
			Assert.AreEqual(1,Directory.GetFiles(_folder,"crash-*.txt").Length);
		}

		[TestMethod]
		public void Write_AggregateException_ListsEveryInner() {
			AggregateException aggregateException=new AggregateException(new TimeoutException("first"),new IOException("second"));
			string text=File.ReadAllText(CrashReport.Write(aggregateException,"TaskScheduler.UnobservedTaskException",isTerminating:false));
			StringAssert.Contains(text,"Inner exception 1:");
			StringAssert.Contains(text,"Inner exception 2:");
			StringAssert.Contains(text,"System.TimeoutException");
			StringAssert.Contains(text,"System.IO.IOException");
		}

		[TestMethod]
		public void Write_StateThatThrows_StillWritesTheRest() {
			CrashReport.FuncAppState=() => throw new NullReferenceException();
			string text=File.ReadAllText(CrashReport.Write(MakeException(),"Unhandled exception",isTerminating:false));
			StringAssert.Contains(text,"--- Program state ---\r\n(not available: NullReferenceException");
			StringAssert.Contains(text,"--- Exception detail ---");
		}

		[TestMethod]
		public void Write_KeepsTheNewestReports() {
			Directory.CreateDirectory(_folder);
			for(int i=0;i<CrashReport.ReportsKept+5;i++) {
				File.WriteAllText(Path.Combine(_folder,"crash-20200101-000000-"+i.ToString("000")+"-1.txt"),"old");
			}
			string path=CrashReport.Write(MakeException(),"Unhandled exception",isTerminating:false);
			string[] arrayFiles=Directory.GetFiles(_folder,"crash-*.txt");
			Assert.AreEqual(CrashReport.ReportsKept,arrayFiles.Length);
			Assert.IsTrue(arrayFiles.Contains(path),"The new report is kept.");
		}

		[TestMethod]
		public void Note_KeepsTheLastOnes() {
			for(int i=0;i<CrashReport.BreadcrumbCount+10;i++) {
				CrashReport.Note("step "+i);
			}
			List<string> listNotes=CrashReport.GetBreadcrumbs();
			Assert.AreEqual(CrashReport.BreadcrumbCount,listNotes.Count);
			StringAssert.EndsWith(listNotes.Last(),"step "+(CrashReport.BreadcrumbCount+9));
		}

		[TestMethod]
		public void ProgramEntry_ReportsEveryUnhandledPath() {
			string entry=File.ReadAllText(S0FDb.FindRepoFile(Path.Combine("OpenPmsDental","Main Modules","ProgramEntry.cs")));
			StringAssert.Contains(entry,"AppDomain.CurrentDomain.UnhandledException+=","Exceptions that end the process.");
			StringAssert.Contains(entry,"TaskScheduler.UnobservedTaskException+=");
			StringAssert.Contains(entry,"Dispatcher.CurrentDispatcher.UnhandledException+=","WPF exceptions on the UI thread (the tooltip crash).");
			StringAssert.Contains(entry,"CrashReport.FuncAppState=");
			StringAssert.Contains(entry,"string pathCrashReport=CrashReport.Write(","WinForms and thread exceptions.");
		}
	}
}
