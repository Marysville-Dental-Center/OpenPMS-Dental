// The 32-bit helper's Schick CDR module (S3a-14 of the .NET 10 move, docs/port-notes/S3a-14.md, section 3): CDR DICOM's automation objects
// (CDRDicom, CDRDATALib), created and called here instead of in the program's process, with the calls WpfControlsOD\Bridges\Schick.cs made
// before S3a-14, line for line. The algorithm was inspired by the Schick example project named CSAutoApp_2008.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace OpenPmsDentalHelper32.Schick {
	///<summary>Runs ShowExam for the program. The CDR objects are created on the first ShowExam (never in the constructor, so a workstation
	///without CDR loses nothing else of the helper) and kept for the helper's lifetime, which is the program's: the application and the exam
	///window are reused for the next patient, as the static fields of Schick.cs were. CDR's OnClose event reaches this thread between two
	///requests because the helper pumps its messages there (MessagePumpingStream).</summary>
	internal sealed class SchickModule:IHelper32Module {
		private readonly Func<CDRDicom.Application> _funcCreateApplication;
		private readonly Func<IntPtr,bool> _funcSetForegroundWindow;
		private CDRDicom.Application _cdrApp=null;
		private CDRDicom.ExamDocument _exam=null;

		///<summary>The module the helper runs: CDR's own automation objects and the Windows SetForegroundWindow.</summary>
		public SchickModule() : this(() => new CDRDicom.Application(),hWnd => NativeMethods.SetForegroundWindow(hWnd)) {
		}

		///<summary>Tests: a fake of CDR's application and of SetForegroundWindow.</summary>
		internal SchickModule(Func<CDRDicom.Application> funcCreateApplication,Func<IntPtr,bool> funcSetForegroundWindow) {
			_funcCreateApplication=funcCreateApplication;
			_funcSetForegroundWindow=funcSetForegroundWindow;
		}

		public string Name {
			get {
				return SchickCommands.MODULE;
			}
		}

		public Helper32Reply Handle(Helper32Request request) {
			switch(request.Command) {
				case SchickCommands.SHOW_EXAM:
					if(request.Args.Length!=3) {
						throw new ArgumentException(SchickCommands.SHOW_EXAM+" takes the last name, the first name and the patient id.");
					}
					ShowExam(request.Args[0],request.Args[1],request.Args[2]);
					return Helper32Reply.Ok(null,null);
				default:
					throw new ArgumentException("The Schick module has no command "+request.Command+".");
			}
		}

		private void ShowExam(string lName,string fName,string patID) {
			//If the CDR application is not running yet, then getting a new instance will start the CDR application.
			//If the CDR application is already running, then getting a new instance will simply reference the existing CDR application.
			//Thus there can never be more than one CDR application running.
			_cdrApp=_funcCreateApplication();
			LoadExamWindow();
			if(!_exam.LoadPatient(lName,fName,patID)) {//Attempt to locate an existing patient in the Schick database.
				//If the user clicks accross with a new patient when an existing patient is already loaded into Schick, then there are only two options:
				//1) Reuse the same CDRDicom.Application() object and pass "true" into exam.NewExam() below.  The problem with this option is that
				//		the user must set the create patient checkbox, otherwise the exam is created for the previous patient, which is terrifying.
				//2) Quit the application, then create a new exam with the patient create checkbox hidden.  In this specific scenario, Schick will create
				//	the patient even though the create patient checkbox is neither showing nor allowed, because there is no previous patient currently loaded.
				//We chose option #2, because it more closely follows our typical bridge patterns.
				//Quit() causes OnExamClosed() to fire, which sets cdrApp to null so it will be reinitialized in LoadCdrApp().  Var cdrApp is still valid.
				_cdrApp.Quit();
				LoadExamWindow();
				CreateExam(lName,fName,patID);
			}
		}

		///<summary>Initializes Schick and loads an empty exam.</summary>
		private void LoadExamWindow() {
			if(_exam==null) {
				_exam=(CDRDicom.ExamDocument)(object)_cdrApp.CreateExamDocument();//(object): the embedded interop types return dynamic
				_exam.OnClose+=new CDRDicom.IExamDocumentEvents_OnCloseEventHandler(OnExamClosed);
			}
			_exam.Visible=true;
			//The exam's hWnd is a 32-bit int; a window handle is that value sign-extended in a 64-bit process. The program allowed this helper to set
			//the foreground window just before the call (Helper32Client.AllowSetForeground), as the program could itself.
			_funcSetForegroundWindow(new IntPtr(_exam.hWnd));
		}

		///<summary>When the user clicks OK from the exam window, the patient will be created.  The user can also cancel.</summary>
		private void CreateExam(string lName,string fName,string patID) {
			CDRDATALib.ICDRPatient patient=(CDRDATALib.ICDRPatient)(object)_exam.Patient;
			if(patient!=null) {
				//Copy patient info to the new exam
				patient.LastName=lName;
				patient.FirstName=fName;
				patient.IDNumber=patID;
				//Then display the new exam window.
				_exam.NewExam(false);
			}
		}

		private void OnExamClosed() {
			_exam=null;
		}
	}
}
