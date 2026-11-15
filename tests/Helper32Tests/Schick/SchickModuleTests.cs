// The helper's Schick CDR module against fakes of CDR DICOM's automation interfaces (S3a-14 of the .NET 10 move, docs/port-notes/S3a-14.md,
// section 3): for each scenario the module makes the calls today's code made (SchickToday, the code of WpfControlsOD/Bridges/Schick.cs before
// S3a-14), in the same order with the same arguments, and the main scenarios are pinned call by call. No CDR object is created and CDR is never
// started: the fakes are made at run time from the interop's interfaces (QbfcFake).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Helper32Tests.QuickBooks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalHelper32;
using OpenPmsDentalHelper32.Schick;

namespace Helper32Tests.Schick {
	[TestClass]
	public class SchickModuleTests {
		private const int HWND_EXAM=0x00012345;

		///<summary>A fake CDR: the application, its exam documents (exam1, exam2, ... in the order they are created), the patient of each exam, and
		///what LoadPatient answers. Quit fires the open exam's OnClose, as CDR does ("Quit() causes OnExamClosed() to fire").</summary>
		private sealed class FakeCdr {
			public readonly QbfcRecorder Recorder=new QbfcRecorder();
			public readonly Queue<bool> QueueLoadPatient=new Queue<bool>();
			public bool IsPatientNull;
			public int CountApplications;
			private int _countExams;
			private CDRDicom.IExamDocumentEvents_OnCloseEventHandler _handlerClose;

			public FakeCdr() {
				Recorder.On("app.CreateExamDocument",x => NewExam());
				Recorder.On("app.Quit",x => {
					CDRDicom.IExamDocumentEvents_OnCloseEventHandler handler=_handlerClose;
					_handlerClose=null;
					handler?.Invoke();
					return null;
				});
			}

			private object NewExam() {
				_countExams++;
				string name="exam"+_countExams.ToString(CultureInfo.InvariantCulture);
				Recorder.On(name+".add_OnClose",x => { _handlerClose=(CDRDicom.IExamDocumentEvents_OnCloseEventHandler)x[0]; return null; });
				Recorder.On(name+".hWnd",x => HWND_EXAM);
				Recorder.On(name+".LoadPatient",x => QueueLoadPatient.Count>0 ? QueueLoadPatient.Dequeue() : false);
				Recorder.On(name+".NewExam",x => true);
				Recorder.On(name+".Patient",x => IsPatientNull ? null : Recorder.Create<CDRDATALib.ICDRPatient>(name+".Patient"));
				return Recorder.Create<CDRDicom.ExamDocument>(name);
			}

			public CDRDicom.Application CreateApplication() {
				CountApplications++;
				Recorder.ListCalls.Add("new CDRDicom.Application()");
				return Recorder.Create<CDRDicom.Application>("app");
			}

			public bool SetForegroundWindow(IntPtr hWnd) {
				Recorder.ListCalls.Add("SetForegroundWindow(0x"+hWnd.ToInt64().ToString("x",CultureInfo.InvariantCulture)+")");
				return true;
			}
		}

		private delegate void Script(FakeCdr fakeCdr);

		///<summary>Runs the patients through today's code and through the module (one module for all, as one helper serves a program process), each
		///with a fake CDR the same script drives, and checks that both made the same calls. Returns the module's calls.</summary>
		private static List<string> RunBoth(Script script,params string[][] arrayPatients) {
			FakeCdr fakeToday=new FakeCdr();
			script(fakeToday);
			SchickToday.Reset();
			SchickToday.FuncCreateApplication=fakeToday.CreateApplication;
			SchickToday.FuncSetForegroundWindow=fakeToday.SetForegroundWindow;
			foreach(string[] arrayPatient in arrayPatients) {
				SchickToday.ShowExam(arrayPatient[0],arrayPatient[1],arrayPatient[2]);
			}
			FakeCdr fakeModule=new FakeCdr();
			script(fakeModule);
			SchickModule module=new SchickModule(fakeModule.CreateApplication,fakeModule.SetForegroundWindow);
			foreach(string[] arrayPatient in arrayPatients) {
				Helper32Reply reply=module.Handle(Request(SchickCommands.SHOW_EXAM,arrayPatient));
				Assert.IsTrue(reply.IsOk);
				Assert.AreEqual(0,reply.Results.Length);
			}
			CollectionAssert.AreEqual(fakeToday.Recorder.ListCalls,fakeModule.Recorder.ListCalls,"The calls differ from today's.\ntoday:\n  "
				+string.Join("\n  ",fakeToday.Recorder.ListCalls)+"\nmodule:\n  "+string.Join("\n  ",fakeModule.Recorder.ListCalls));
			return fakeModule.Recorder.ListCalls;
		}

		private static Helper32Request Request(string command,params string[] args) {
			return new Helper32Request { Module=SchickCommands.MODULE,Command=command,Args=args };
		}

		private static readonly string[] _patientOne=new[] { "Synthetic","Patient One","1001" };
		private static readonly string[] _patientTwo=new[] { "Test\u00e9","Zweite","CH-2002" };

		[TestMethod]
		public void ShowExam_PatientFound_LoadsTheExamWindow_AsToday() {
			List<string> listCalls=RunBoth(x => x.QueueLoadPatient.Enqueue(true),_patientOne);
			CollectionAssert.AreEqual(new[] {
				"new CDRDicom.Application()",
				"app.CreateExamDocument()",
				"exam1.add_OnClose(CDRDicom.IExamDocumentEvents_OnCloseEventHandler)",
				"exam1.Visible=true",
				"exam1.hWnd",
				"SetForegroundWindow(0x12345)",
				"exam1.LoadPatient(\"Synthetic\",\"Patient One\",\"1001\")",
			},listCalls);
		}

		[TestMethod]
		public void ShowExam_PatientNotFound_QuitsAndCreatesTheExam_AsToday() {
			List<string> listCalls=RunBoth(x => x.QueueLoadPatient.Enqueue(false),_patientTwo);
			CollectionAssert.AreEqual(new[] {
				"new CDRDicom.Application()",
				"app.CreateExamDocument()",
				"exam1.add_OnClose(CDRDicom.IExamDocumentEvents_OnCloseEventHandler)",
				"exam1.Visible=true",
				"exam1.hWnd",
				"SetForegroundWindow(0x12345)",
				"exam1.LoadPatient(\"Test\u00e9\",\"Zweite\",\"CH-2002\")",
				"app.Quit()",
				//Quit fired OnClose, so the exam window is made again.
				"app.CreateExamDocument()",
				"exam2.add_OnClose(CDRDicom.IExamDocumentEvents_OnCloseEventHandler)",
				"exam2.Visible=true",
				"exam2.hWnd",
				"SetForegroundWindow(0x12345)",
				"exam2.Patient",
				"exam2.Patient.LastName=\"Test\u00e9\"",
				"exam2.Patient.FirstName=\"Zweite\"",
				"exam2.Patient.IDNumber=\"CH-2002\"",
				"exam2.NewExam(false)",
			},listCalls);
		}

		[TestMethod]
		public void ShowExam_SecondPatient_ReusesTheOpenExamWindow_AsToday() {
			List<string> listCalls=RunBoth(x => { x.QueueLoadPatient.Enqueue(true); x.QueueLoadPatient.Enqueue(true); },_patientOne,_patientTwo);
			Assert.AreEqual(1,listCalls.Count(x => x=="app.CreateExamDocument()"),"The exam window stays open between two patients.");
			Assert.AreEqual(2,listCalls.Count(x => x=="new CDRDicom.Application()"),"Each ShowExam gets the application, as today.");
			Assert.AreEqual("exam1.LoadPatient(\"Test\u00e9\",\"Zweite\",\"CH-2002\")",listCalls.Last());
		}

		[TestMethod]
		public void ShowExam_NotFoundThenFound_AsToday() {
			RunBoth(x => { x.QueueLoadPatient.Enqueue(false); x.QueueLoadPatient.Enqueue(true); },_patientOne,_patientTwo);
		}

		[TestMethod]
		public void ShowExam_ExamClosedByTheUserBetweenTwoPatients_OpensANewOne_AsToday() {
			//The user closes CDR's exam window: CDR fires OnClose, which reaches the helper between two requests (its message pump).
			FakeCdr fakeCdr=new FakeCdr();
			fakeCdr.QueueLoadPatient.Enqueue(true);
			fakeCdr.QueueLoadPatient.Enqueue(true);
			SchickModule module=new SchickModule(fakeCdr.CreateApplication,fakeCdr.SetForegroundWindow);
			module.Handle(Request(SchickCommands.SHOW_EXAM,_patientOne));
			((CDRDicom.ICDRApplication)fakeCdr.Recorder.Create<CDRDicom.Application>("app")).Quit();//fires exam1's OnClose
			module.Handle(Request(SchickCommands.SHOW_EXAM,_patientTwo));
			Assert.AreEqual(2,fakeCdr.Recorder.ListCalls.Count(x => x=="app.CreateExamDocument()"));
			Assert.AreEqual("exam2.LoadPatient(\"Test\u00e9\",\"Zweite\",\"CH-2002\")",fakeCdr.Recorder.ListCalls.Last());
		}

		[TestMethod]
		public void ShowExam_NoPatientObject_CreatesNoExam_AsToday() {
			List<string> listCalls=RunBoth(x => { x.QueueLoadPatient.Enqueue(false); x.IsPatientNull=true; },_patientOne);
			Assert.AreEqual("exam2.Patient",listCalls.Last());
			Assert.IsFalse(listCalls.Any(x => x.Contains("NewExam")));
		}

		[TestMethod]
		public void ShowExam_CdrThrows_TheExceptionGoesToTheProgram() {
			FakeCdr fakeCdr=new FakeCdr();
			fakeCdr.Recorder.On("app.CreateExamDocument",x => throw new System.Runtime.InteropServices.COMException("Synthetic CDR failure"));
			SchickModule module=new SchickModule(fakeCdr.CreateApplication,fakeCdr.SetForegroundWindow);
			System.Runtime.InteropServices.COMException exception=Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
				() => module.Handle(Request(SchickCommands.SHOW_EXAM,_patientOne)));
			Assert.AreEqual("Synthetic CDR failure",exception.Message);
		}

		[TestMethod]
		public void Arguments_WrongCountOrCommand_ReachNoCdrCall() {
			FakeCdr fakeCdr=new FakeCdr();
			SchickModule module=new SchickModule(fakeCdr.CreateApplication,fakeCdr.SetForegroundWindow);
			Assert.ThrowsExactly<ArgumentException>(() => module.Handle(Request(SchickCommands.SHOW_EXAM,"Synthetic","Patient")));
			Assert.ThrowsExactly<ArgumentException>(() => module.Handle(Request("NoSuchCommand")));
			Assert.AreEqual(0,fakeCdr.Recorder.ListCalls.Count);
			Assert.AreEqual(0,fakeCdr.CountApplications);
		}

		[TestMethod]
		public void Constructor_LoadsNoVendorCode() {
			//The helper builds every module at its start (Helper32Modules.CreateModules): the real module's constructor creates no CDR object.
			SchickModule module=new SchickModule();
			Assert.AreEqual(SchickCommands.MODULE,module.Name);
			Assert.IsFalse(Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(x => x.ModuleName.StartsWith("CDR",StringComparison.OrdinalIgnoreCase)));
		}
	}
}
