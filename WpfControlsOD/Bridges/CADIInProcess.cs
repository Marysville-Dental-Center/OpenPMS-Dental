// Mediadent / CADI in the program's own process, the native-first path (N64-A3 of the native 64-bit integration survey,
// docs/port-notes/N64-A3.md; net10-plan decision 23): CADI.cs's CADINativeWindow of before N64-A3, moved here with the same calls in the same
// order, the same hidden window for CADI's exit notice (WM_CLOSE) and the same object lifetime. CADI's automation object (ProgID
// Mediadent.Application) is called late-bound through IDispatch; it is very probably a server program of its own (LocalServer32), which a
// program of either bitness reaches. NativeFirstChooser.Cadi reads its registration in both registry views first, and moves to the 32-bit
// helper's Cadi module (the fallback, CADI.cs's other path) when the activation or a call fails in a way that says this process cannot reach
// the server.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalHelper32.Cadi;

namespace OpenPmsDental.Bridges {
	///<summary>CADI's path in the program's process and its choice (NativeFirstChooser.Cadi).</summary>
	internal static class CADIInProcess {
		///<summary>Per bridging documents, this is the programID used to identify CADI</summary>
		internal const string PROGRAM_ID="Mediadent.Application";
		///<summary>Creates CADI's automation object (which starts CADI), CADI.cs's two lines of before N64-A3. A test sets a fake; nothing else
		///changes it.</summary>
		internal static Func<object> FuncCreateComObject=() => {
			Type progType=Type.GetTypeFromProgID(PROGRAM_ID);
			return Activator.CreateInstance(progType);
		};
		///<summary>Releases CADI's automation object (Marshal.ReleaseComObject). A test sets a fake; nothing else changes it.</summary>
		internal static Action<object> ActionReleaseComObject=x => Marshal.ReleaseComObject(x);
		///<summary>Reads the ProgID's registration in both registry views, read only. A test sets a fake; nothing else changes it.</summary>
		internal static Func<ComServerRegistration> FuncReadRegistration=() => ComServerRegistration.Read(PROGRAM_ID);

		///<summary>True when CADI is called in this process (NativeFirstChooser.Cadi, from the ProgID's registration on the first call; a class
		///registered nowhere stays here, where creating it says CADI is not installed, as before); false when the 32-bit helper was chosen.
		///</summary>
		public static bool IsChosen() {
			return NativeFirstChooser.Cadi.ChooseForComServer(FuncReadRegistration)!=VendorPath.Helper32;
		}

		///<summary>After CADI's object failed in this process: true when the call is retried in the helper now, which is when the failure says this
		///process cannot reach CADI's server (NativeFirstChooser.Cadi.ChooseHelperAfterFailure; a 64-bit process only) and no call reached CADI
		///yet in this window's life (the activation failed, or the first call, OleClientWindowHandle, did). When a call had reached CADI the
		///helper is chosen for the next click on, and this one shows CADI.cs's message as before.</summary>
		public static bool IsRetriedInHelper(Exception ex,bool isCadiReached) {
			return NativeFirstChooser.Cadi.ChooseHelperAfterFailure(ex) && !isCadiReached;
		}
	}

	///<summary>Contains the logic for opening the CADI application, sending OLE commands to it, and listening to messages</summary>
	internal class CADINativeWindow:NativeWindow {
		///<summary>Per bridging documents, this is the value of the sysuint we expect back when the CADI window is closed by the user</summary>
		private const int OLECLIENTNOTIFY_APPLICATIONEXIT=1;
		//Define the windows close message to send back to this window from the CADI application when it closes
		//See https://msdn.microsoft.com/en-us/library/windows/desktop/ms632617(v=vs.85).aspx
		private const uint WM_CLOSE=0x0010;
		///<summary>The comObject used to communicate to CADI</summary>
		private dynamic _comObject;
		///<summary>A handle to this "window" object. Register this handle with CADI to receive window messages (e.g. Window closing etc.);</summary>
		private int _windowHandle;
		///<summary>Event will be raised anytime this instance closes. Alerts owner to release reference to this instance.</summary>
		Action _onClose=null;

		public CADINativeWindow(Action onClose) {
			_onClose=onClose;
			//Set up handle to get back closing events from CADI window
			CreateParams cp=new CreateParams();
			this.CreateHandle(cp);
		}

		///<summary>Launches the program as a comObject, and passes CADI OLE commands to the program.
		///See https://stackoverflow.com/questions/29787825/how-to-interact-with-another-programs-with-ole-automation-in-c
		///The patient's values are worked out by CADI.cs before (N64-A3; null: no patient). Returns false, with this window closed, when the call
		///goes to the 32-bit helper instead (CADIInProcess.IsRetriedInHelper); true otherwise. Throws the messages of before N64-A3.</summary>
		public bool SendData(CadiPatient cadiPatient) {
			bool isCadiReached=_comObject!=null;//N64-A3: an object from an earlier call has had calls
			try {
				if(_comObject==null) {
					_comObject=CADIInProcess.FuncCreateComObject();//Type.GetTypeFromProgID(PROGRAM_ID), Activator.CreateInstance(progType)
					//register the handle of this window with CADI to listen for application closing events
					int result=_comObject.OleClientWindowHandle(_windowHandle,OLECLIENTNOTIFY_APPLICATIONEXIT,WM_CLOSE,0,0);
					isCadiReached=true;//N64-A3
					if(result!=0) { //0=success; 14=Invalid argument; -1=Unspecified error.
						throw new Exception("Unable to communicate with CADI. Verify it is installed and try again.");
					}
				}
				if(cadiPatient==null) {
					return true;
				}
				//Set patient info
				_comObject.OleBringToFrontApp();
				_comObject.OleBeginTransaction();
				_comObject.OleSetPatientName(cadiPatient.Name);
				_comObject.OleSetPatientDateOfBirth(cadiPatient.DateOfBirth);
				_comObject.OleSetPatientBiologicalAge(cadiPatient.Age);
				_comObject.OleSetPatientSex(cadiPatient.Sex);
				//Send patient images file path
				_comObject.OleLoadPicLib(cadiPatient.PicLibPath);
				_comObject.OleEndTransaction();
				return true;
			}
			catch(Exception ex) when(CADIInProcess.IsRetriedInHelper(ex,isCadiReached)) {
				//N64-A3: this process cannot reach CADI's server and nothing reached CADI yet: the helper repeats the call. A 32-bit process never
				//gets here (the helper is 32-bit too), so it shows the messages below as before.
				CloseWindow();
				return false;
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
			ODException.SwallowAnyException(() => { CADIInProcess.ActionReleaseComObject((object)_comObject); });//Marshal.ReleaseComObject(_comObject)
			_comObject=null;
		}

		///<summary>Listen to when the handle changes to keep the variable in sync
		///See https://msdn.microsoft.com/en-us/library/system.windows.forms.nativewindow.handle(v=vs.110).aspx </summary>
    protected override void OnHandleChange()
    {
        _windowHandle = (int)this.Handle;
    }

		///<summary>Listen for messages being sent back to this handle by CADI. We need to include this to allow the CADI window to close properly.
		///See https://msdn.microsoft.com/en-us/library/system.windows.forms.nativewindow.handle(v=vs.110).aspx </summary>
    protected override void WndProc(ref Message m)
    {
			switch ((uint)m.Msg)
			{
        case WM_CLOSE: //Note, if in demo mode and user clicks 'Bye', this event won't be raised. This will fix itself if they open the bridge again.
					CloseWindow();
				break;
			}
			base.WndProc(ref m);
    }
	}
}
