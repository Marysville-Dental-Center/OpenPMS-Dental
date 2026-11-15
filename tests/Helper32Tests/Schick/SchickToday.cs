// The Schick CDR code of WpfControlsOD/Bridges/Schick.cs before S3a-14 of the .NET 10 move (ShowExam, LoadExamWindow, CreateExam, OnExamClosed),
// kept here to compare the helper's Schick module with (SchickModuleTests). Two lines are replaced, each marked: the creation of CDR's
// application object and SetForegroundWindow, so a test's fakes stand in for them; the patient comes as its two names (pat.LName, pat.FName),
// and the long comment on Quit is shortened. The calls are the old text's.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace Helper32Tests.Schick {
	internal static class SchickToday {
		///<summary>Replaces "new CDRDicom.Application()".</summary>
		public static Func<CDRDicom.Application> FuncCreateApplication;
		///<summary>Replaces the DllImport of User32.dll's SetForegroundWindow.</summary>
		public static Func<IntPtr,bool> FuncSetForegroundWindow;

		private static CDRDicom.Application _cdrApp=null;
		private static CDRDicom.ExamDocument _exam=null;

		///<summary>Forgets the state of an earlier test, as a new program process would.</summary>
		public static void Reset() {
			_cdrApp=null;
			_exam=null;
		}

		///<summary>SendData called ShowExam(pat,patID); the patient's LName and FName are passed here.</summary>
		public static void ShowExam(string lName,string fName,string patID) {
			//If the CDR application is not running yet, then getting a new instance will start the CDR application.
			//If the CDR application is already running, then getting a new instance will simply reference the existing CDR application.
			//Thus there can never be more than one CDR application running.
			_cdrApp=FuncCreateApplication();//replaced: new CDRDicom.Application()
			LoadExamWindow();
			if(!_exam.LoadPatient(lName,fName,patID)) {//Attempt to locate an existing patient in the Schick database.
				//Quit() causes OnExamClosed() to fire, which sets cdrApp to null so it will be reinitialized in LoadCdrApp().  Var cdrApp is still valid.
				_cdrApp.Quit();
				LoadExamWindow();
				CreateExam(lName,fName,patID);
			}
		}

		///<summary>Initializes Schick and loads an empty exam.</summary>
		private static void LoadExamWindow() {
			if(_exam==null) {
				_exam=(CDRDicom.ExamDocument)_cdrApp.CreateExamDocument();
				_exam.OnClose+=new CDRDicom.IExamDocumentEvents_OnCloseEventHandler(OnExamClosed);
			}
			_exam.Visible=true;
			FuncSetForegroundWindow(new IntPtr(_exam.hWnd));//replaced: SetForegroundWindow(new IntPtr(_exam.hWnd))
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
	}
}
