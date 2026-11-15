// The helper's Schick CDR module against fakes of CDR DICOM's automation interfaces (S3a-14 of the .NET 10 move, docs/port-notes/S3a-14.md,
// section 3): for each scenario the module makes the calls today's code made (SchickToday, the code of WpfControlsOD/Bridges/Schick.cs before
// S3a-14), in the same order with the same arguments, and the main scenarios are pinned call by call. Since S3a-14n every scenario also runs on
// the program's in-process path (WpfControlsOD/Bridges/SchickInProcess.cs, linked, through NativeFirstChooser.SchickCdr), which must make the
// same calls too. No CDR object is created and CDR is never started: the fakes are made at run time from the interop's interfaces (QbfcFake).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CodeBase;
using Helper32Tests.QuickBooks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.Bridges;
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
			FakeCdr fakeInProcess=new FakeCdr();
			script(fakeInProcess);
			RunInProcess(fakeInProcess,null,() => {
				foreach(string[] arrayPatient in arrayPatients) {
					Assert.IsTrue(SchickInProcess.ShowExam(arrayPatient[0],arrayPatient[1],arrayPatient[2]),"CDR activated in this process.");
				}
			});
			CollectionAssert.AreEqual(fakeToday.Recorder.ListCalls,fakeInProcess.Recorder.ListCalls,"The calls differ from today's.\ntoday:\n  "
				+string.Join("\n  ",fakeToday.Recorder.ListCalls)+"\nin-process path:\n  "+string.Join("\n  ",fakeInProcess.Recorder.ListCalls));
			return fakeModule.Recorder.ListCalls;
		}

		///<summary>Runs action on the in-process path with fakeCdr's application and SetForegroundWindow, from a new process's state and chooser
		///(deciding for is64BitProcess; null: this process's bitness), and puts both back afterwards.</summary>
		private static void RunInProcess(FakeCdr fakeCdr,bool? is64BitProcess,Action action) {
			Func<CDRDicom.Application> funcCreateApplicationOld=SchickInProcess.FuncCreateApplication;
			Func<IntPtr,bool> funcSetForegroundWindowOld=SchickInProcess.FuncSetForegroundWindow;
			NativeFirstChooser.SchickCdr.ResetForTests(is64BitProcess);
			SchickInProcess.ResetForTests();
			SchickInProcess.FuncCreateApplication=fakeCdr.CreateApplication;
			SchickInProcess.FuncSetForegroundWindow=fakeCdr.SetForegroundWindow;
			try {
				action();
			}
			finally {
				SchickInProcess.FuncCreateApplication=funcCreateApplicationOld;
				SchickInProcess.FuncSetForegroundWindow=funcSetForegroundWindowOld;
				SchickInProcess.ResetForTests();
				NativeFirstChooser.SchickCdr.ResetForTests();
			}
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
		public void InProcess_CdrThrows_TheExceptionGoesToSendData_AsToday() {
			//Schick.cs's SendData shows "Error launching Schick CDR Dicom." for it, as before S3a-14.
			FakeCdr fakeCdr=new FakeCdr();
			fakeCdr.Recorder.On("app.CreateExamDocument",x => throw new System.Runtime.InteropServices.COMException("Synthetic CDR failure"));
			RunInProcess(fakeCdr,null,() => {
				System.Runtime.InteropServices.COMException exception=Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(
					() => SchickInProcess.ShowExam(_patientOne[0],_patientOne[1],_patientOne[2]));
				Assert.AreEqual("Synthetic CDR failure",exception.Message);
				Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.SchickCdr.Choice,"The class activated here: this process is chosen.");
			});
		}

		[TestMethod]
		public void InProcess_CdrNotActivatableHere_TheHelperIn64Bit_TheOldFailureIn32Bit() {
			foreach(int hResult in new[] { NativeFirstChooser.REGDB_E_CLASSNOTREG,NativeFirstChooser.CO_E_SERVER_EXEC_FAILURE }) {
				//64-bit: CDR's class is not registered for a 64-bit caller (or its server did not start): ShowExam returns false with no CDR call, and
				//Schick.cs sends the patient to the helper; no later call activates again.
				FakeCdr fakeCdr64=new FakeCdr();
				Func<CDRDicom.Application> funcThrow=() => {
					fakeCdr64.CountApplications++;
					throw new System.Runtime.InteropServices.COMException("Synthetic: not activatable",hResult);
				};
				RunInProcess(fakeCdr64,true,() => {
					SchickInProcess.FuncCreateApplication=funcThrow;
					Assert.IsFalse(SchickInProcess.ShowExam(_patientOne[0],_patientOne[1],_patientOne[2]));
					Assert.IsFalse(SchickInProcess.ShowExam(_patientTwo[0],_patientTwo[1],_patientTwo[2]));
					Assert.AreEqual(1,fakeCdr64.CountApplications,"Activated once.");
					Assert.AreEqual(0,fakeCdr64.Recorder.ListCalls.Count,"No CDR call in this process.");
					Assert.AreEqual(VendorPath.Helper32,NativeFirstChooser.SchickCdr.Choice);
				});
				//32-bit: the failure is CDR's, thrown to SendData as before ("Error launching Schick CDR Dicom."), and this process stays chosen.
				FakeCdr fakeCdr32=new FakeCdr();
				RunInProcess(fakeCdr32,false,() => {
					SchickInProcess.FuncCreateApplication=() => throw new System.Runtime.InteropServices.COMException("Synthetic: not activatable",hResult);
					Assert.ThrowsExactly<System.Runtime.InteropServices.COMException>(() => SchickInProcess.ShowExam(_patientOne[0],_patientOne[1],_patientOne[2]));
					Assert.AreEqual(VendorPath.InProcess,NativeFirstChooser.SchickCdr.Choice);
				});
			}
		}

		[TestMethod]
		public void SchickCs_CallsTheInProcessPathFirst_AndTheHelperOnlyWhenItWasNotChosen() {
			string text=Helper32Tests.Support.TestPaths.ReadSource("WpfControlsOD","Bridges","Schick.cs");
			int indexInProcess=text.IndexOf("if(SchickInProcess.ShowExam(pat.LName,pat.FName,patID)) {",StringComparison.Ordinal);
			int indexHelper=text.IndexOf("Helper32Client.Default.CallPumping(SchickCommands.MODULE,SchickCommands.SHOW_EXAM,",StringComparison.Ordinal);
			Assert.IsTrue(indexInProcess>0 && indexHelper>indexInProcess,"The in-process path first, the helper after it.");
			StringAssert.Contains(text.Substring(indexInProcess,indexHelper-indexInProcess).Replace("\r\n","\n"),"\n\t\t\t\treturn;\n\t\t\t}\n");
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
