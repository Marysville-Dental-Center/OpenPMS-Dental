// The Mediadent / CADI code of WpfControlsOD/Bridges/CADI.cs before N64-A3 of the native 64-bit integration survey (CADI.SendData and
// CADINativeWindow, main at e2fc2306f), kept here to compare the program's in-process path and the helper's Cadi module with (CadiModuleTests).
// Replaced, each marked: the creation of CADI's object and its release, so a test's fake stands in for them; the program properties and the
// patient, which come as a TodayPatient; MessageBox.Show and the security log, which become the shown message. The calls are the old text's.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;

namespace Helper32Tests.Cadi {
	///<summary>The patient's fields and the two program properties CADINativeWindow.SendData read.</summary>
	internal sealed class TodayPatient {
		public long PatNum;
		public string ChartNumber;
		public string LName;
		public string FName;
		public DateTime Birthdate;
		public int Age;
		public bool IsFemale;
		///<summary>ProgramProperties.PropertyDescs.PatOrChartNum: "0" for the PatNum.</summary>
		public string PatOrChartNum;
		///<summary>ProgramProperties.PropertyDescs.ImageFolder.</summary>
		public string ImageFolder;
	}

	internal static class CadiToday {
		///<summary>Replaces "Type progType=Type.GetTypeFromProgID(PROGRAM_ID); _comObject=Activator.CreateInstance(progType);".</summary>
		public static Func<object> FuncCreateComObject;
		///<summary>Replaces "Marshal.ReleaseComObject(_comObject)".</summary>
		public static Action<object> ActionReleaseComObject;
		///<summary>Replaces MessageBox.Show(e.Message) (and the security log entry with the same message).</summary>
		public static Action<string> ActionShowMessage;

		///<summary>Keep as a static object so it doesn't get garbage collected.</summary>
		private static CADINativeWindowToday CADIWindow;

		///<summary>Forgets the state of an earlier test, as a new program process would (the window of an earlier test is closed by it).</summary>
		public static void Reset() {
			CADIWindow=null;
		}

		///<summary>The window CADI was given, or zero.</summary>
		public static IntPtr WindowHandle {
			get {
				return CADIWindow==null ? IntPtr.Zero : CADIWindow.Handle;
			}
		}

		public static void SendData(TodayPatient pat) {
			try {
				if(CADIWindow==null) { //Only release this reference once the CADIWindow instance has closed.
					CADIWindow=new CADINativeWindowToday(new Action(() => CADIWindow=null ));
				}
				CADIWindow.SendData(pat);
			}
			catch (Exception e) {
				ActionShowMessage(e.Message);//replaced: MessageBox.Show(e.Message); SecurityLogs.MakeLogEntry(...,e.Message);
			}
		}

		///<summary>Contains the logic for opening the CADI application, sending OLE commands to it, and listening to messages</summary>
		private class CADINativeWindowToday:NativeWindow {
			///<summary>Per bridging documents, this is the value of the sysuint we expect back when the CADI window is closed by the user</summary>
			private const int OLECLIENTNOTIFY_APPLICATIONEXIT=1;
			///<summary>Per bridging documents, birthdate is specified using COM TDATE, which is calculated as the number of days that have elapsed since 12/30/1899</summary>
			private static readonly DateTime TDATE_START=new DateTime(1899,12,30);
			private const uint WM_CLOSE=0x0010;
			///<summary>The comObject used to communicate to CADI</summary>
			private dynamic _comObject;
			///<summary>A handle to this "window" object. Register this handle with CADI to receive window messages (e.g. Window closing etc.);</summary>
			private int _windowHandle;
			///<summary>Event will be raised anytime this instance closes. Alerts owner to release reference to this instance.</summary>
			Action _onClose=null;

			public CADINativeWindowToday(Action onClose) {
				_onClose=onClose;
				//Set up handle to get back closing events from CADI window
				CreateParams cp=new CreateParams();
				this.CreateHandle(cp);
			}

			public void SendData(TodayPatient pat) {
				try {
					if(_comObject==null) {
						_comObject=FuncCreateComObject();//replaced: Type.GetTypeFromProgID(PROGRAM_ID); Activator.CreateInstance(progType)
						//register the handle of this window with CADI to listen for application closing events
						int result=_comObject.OleClientWindowHandle(_windowHandle,OLECLIENTNOTIFY_APPLICATIONEXIT,WM_CLOSE,0,0);
						if(result!=0) { //0=success; 14=Invalid argument; -1=Unspecified error.
							throw new Exception("Unable to communicate with CADI. Verify it is installed and try again.");
						}
					}
					if(pat==null) {
						return;
					}
					//Set patient info
					_comObject.OleBringToFrontApp();
					_comObject.OleBeginTransaction();
					string id="";
					if(pat.PatOrChartNum=="0") {//replaced: ProgramProperties.GetPropVal(ProgramCur.ProgramNum,ProgramProperties.PropertyDescs.PatOrChartNum)
						id=pat.PatNum.ToString();
					}
					else {
						id=pat.ChartNumber;
					}
					_comObject.OleSetPatientName(pat.LName+", "+pat.FName);
					_comObject.OleSetPatientDateOfBirth(pat.Birthdate.Subtract(TDATE_START).TotalDays);
					_comObject.OleSetPatientBiologicalAge(pat.Age);
					_comObject.OleSetPatientSex(pat.IsFemale ? "F" : "M");//replaced: pat.Gender==PatientGender.Female
					//Send patient images file path
					string imagePath=pat.ImageFolder;//replaced: ProgramProperties.GetPropVal(ProgramCur.ProgramNum,ProgramProperties.PropertyDescs.ImageFolder)
					_comObject.OleLoadPicLib(CodeBase.ODFileUtils.CombinePaths(imagePath,id));
					_comObject.OleEndTransaction();
				}
				catch(COMException) { //_comObject cannot be referenced except to set to null in this context. Anything else would throw an exception.
					_comObject=null;
					CloseWindow();
					throw new Exception("Unable to access CADI. Verify it is installed and try again.");
				}
				catch (Exception e) { //Not a COMException so it is ok to reference the _comObject.
					bool throwFriendly=_comObject==null;
					CloseWindow();
					if(throwFriendly) {
						throw new Exception("Unable to open CADI. Verify it is installed and try again.");
					}
					throw e;
				}
			}

			///<summary>Release _comObject and inform the owner window that is can release it's reference to this class instance.</summary>
			private void CloseWindow() {
				ODException.SwallowAnyException(() => { this.ReleaseHandle(); });
				_onClose();
				if(_comObject==null) {
					return;
				}
				ODException.SwallowAnyException(() => { ActionReleaseComObject((object)_comObject); });//replaced: Marshal.ReleaseComObject(_comObject)
				_comObject=null;
			}

			protected override void OnHandleChange() {
				_windowHandle = (int)this.Handle;
			}

			protected override void WndProc(ref Message m) {
				switch ((uint)m.Msg) {
					case WM_CLOSE:
						CloseWindow();
						break;
				}
				base.WndProc(ref m);
			}
		}
	}
}
