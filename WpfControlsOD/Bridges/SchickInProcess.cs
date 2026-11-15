#if !DISABLE_WINDOWS_BRIDGES
// Schick CDR DICOM in the program's own process, the native-first path (S3a-14n of the .NET 10 move, docs/port-notes/S3a-14.md, section
// "S3a-14n: native first"): Schick.cs's ShowExam, LoadExamWindow, CreateExam and OnExamClosed of before S3a-14, moved here line for line. CDR's
// application object is CDRDicom.Application, an automation server whose client is its own program (CDRDicom.exe), so a program of either
// bitness can call it once CDR registered it for that bitness; NativeFirstChooser.SchickCdr finds out by activating it (the same
// "new CDRDicom.Application()" as before). The fallback, the 32-bit helper's Schick module, is Schick.cs's other path.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.InteropServices;
using CodeBase;

namespace OpenPmsDental.Bridges {
	///<summary>Schick CDR's automation objects in the program's process (Schick.cs's path before S3a-14). The algorithm provided below was
	///inspired by the Schick example project named CSAutoApp_2008.</summary>
	internal static class SchickInProcess {
		///<summary>Creates CDR's application object, through NativeFirstChooser.SchickCdr. A test sets a fake; nothing else changes it.</summary>
		internal static Func<CDRDicom.Application> FuncCreateApplication=() => new CDRDicom.Application();
		///<summary>User32's SetForegroundWindow. A test sets a fake; nothing else changes it.</summary>
		internal static Func<IntPtr,bool> FuncSetForegroundWindow=SetForegroundWindow;

		private static CDRDicom.Application _cdrApp=null;
		private static CDRDicom.ExamDocument _exam=null;

		///<summary>Forgets the state, as a new program process has it. For tests.</summary>
		internal static void ResetForTests() {
			_cdrApp=null;
			_exam=null;
		}

		///<summary>Loads the patient in CDR's exam window, or creates the exam, as before S3a-14 (Schick.cs called it with pat.LName and pat.FName).
		///False, with nothing done, when NativeFirstChooser.SchickCdr chose the 32-bit helper: CDR's class cannot be activated from this process
		///(not registered for this bitness, or its server could not start), and Schick.cs sends the patient to the helper instead.</summary>
		public static bool ShowExam(string lName,string fName,string patID) {
			//If the CDR application is not running yet, then getting a new instance will start the CDR application.
			//If the CDR application is already running, then getting a new instance will simply reference the existing CDR application.
			//Thus there can never be more than one CDR application running.
			CDRDicom.Application cdrApp=NativeFirstChooser.SchickCdr.ActivateComObject(FuncCreateApplication);//S3a-14n: new CDRDicom.Application(), once chosen
			if(cdrApp==null) {
				return false;
			}
			_cdrApp=cdrApp;
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
			return true;
		}

		///<summary>Initializes Schick and loads an empty exam.</summary>
		private static void LoadExamWindow() {
			if(_exam==null) {
				_exam=(CDRDicom.ExamDocument)_cdrApp.CreateExamDocument();
				_exam.OnClose+=new CDRDicom.IExamDocumentEvents_OnCloseEventHandler(OnExamClosed);
			}
			_exam.Visible=true;
			FuncSetForegroundWindow(new IntPtr(_exam.hWnd));//The exam's hWnd is a 32-bit int; a window handle is that value sign-extended in a 64-bit process.
		}

		///<summary>When the user clicks OK from the exam window, the patient will be created.  The user can also cancel.</summary>
		private static void CreateExam(string lName,string fName,string patID) {
			CDRDATALib.ICDRPatient patient=(CDRDATALib.ICDRPatient)_exam.Patient;
			if(patient!=null) {
				//Copy patient info to the new exam
				patient.LastName=lName;
				patient.FirstName=fName;
				patient.IDNumber=patID;
				//Then display the new exam window.
				_exam.NewExam(false);
			}
		}

		private static void OnExamClosed() {
			_exam=null;
		}

		[DllImport("User32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
	}
}
#endif
