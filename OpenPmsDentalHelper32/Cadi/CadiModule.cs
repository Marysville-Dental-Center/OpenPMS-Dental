// The 32-bit helper's Mediadent / CADI module (N64-A3 of the native 64-bit integration survey, docs/port-notes/N64-A3.md): CADI's automation
// object (ProgID Mediadent.Application, late-bound through IDispatch) created and called here, the fallback when the program's process cannot
// reach it (CADI.cs, CADIInProcess.cs, NativeFirstChooser.Cadi). The calls, their order and their values, the window CADI posts its exit notice
// to (WM_CLOSE), and the object's lifetime are those of WpfControlsOD\Bridges\CADI.cs before N64-A3 (CADINativeWindow), line for line.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenPmsDentalHelper32.Cadi {
	///<summary>Runs SendData for the program. The window and the CADI object are made on the first SendData (never in the constructor, so a
	///workstation without CADI loses nothing else of the helper) and kept between calls, as CADI.cs's static CADIWindow kept them: until CADI
	///posts its exit notice (WM_CLOSE) to the window, which reaches this thread between two requests because the helper pumps its messages
	///(MessagePumpingStream), or a call fails; the next SendData then makes both again.</summary>
	internal sealed class CadiModule:IHelper32Module {
		private readonly Func<object> _funcCreateComObject;
		private readonly Action<object> _actionReleaseComObject;
		///<summary>CADI.cs's CADIWindow: only released once the window has closed.</summary>
		private CadiWindow _cadiWindow;

		///<summary>The module the helper runs: CADI's own automation object (Type.GetTypeFromProgID and Activator.CreateInstance, as CADI.cs).
		///</summary>
		public CadiModule() : this(CadiWindow.CreateComObject,x => Marshal.ReleaseComObject(x)) {
		}

		///<summary>Tests: a fake of CADI's automation object (and of its release).</summary>
		internal CadiModule(Func<object> funcCreateComObject,Action<object> actionReleaseComObject) {
			_funcCreateComObject=funcCreateComObject;
			_actionReleaseComObject=actionReleaseComObject;
		}

		public string Name {
			get {
				return CadiCommands.MODULE;
			}
		}

		///<summary>For tests: the window CADI was given, or null when none is open.</summary>
		internal IntPtr WindowHandle {
			get {
				return _cadiWindow==null ? IntPtr.Zero : _cadiWindow.Handle;
			}
		}

		public Helper32Reply Handle(Helper32Request request) {
			switch(request.Command) {
				case CadiCommands.SEND_DATA:
					CadiPatient cadiPatient=CadiCommands.DecodePatient(request.Args);
					if(_cadiWindow==null) { //Only release this reference once the CADIWindow instance has closed.
						_cadiWindow=new CadiWindow(new Action(() => _cadiWindow=null ),_funcCreateComObject,_actionReleaseComObject);
					}
					_cadiWindow.SendData(cadiPatient);
					return Helper32Reply.Ok(null,null);
				default:
					throw new ArgumentException("The Cadi module has no command "+request.Command+".");
			}
		}

		///<summary>Contains the logic for opening the CADI application, sending OLE commands to it, and listening to messages (CADI.cs's
		///CADINativeWindow).</summary>
		private sealed class CadiWindow:NativeWindow {
			///<summary>Per bridging documents, this is the programID used to identify CADI</summary>
			private const string PROGRAM_ID="Mediadent.Application";
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
			private readonly Func<object> _funcCreateComObject;
			private readonly Action<object> _actionReleaseComObject;

			public CadiWindow(Action onClose,Func<object> funcCreateComObject,Action<object> actionReleaseComObject) {
				_onClose=onClose;
				_funcCreateComObject=funcCreateComObject;
				_actionReleaseComObject=actionReleaseComObject;
				//Set up handle to get back closing events from CADI window
				CreateParams cp=new CreateParams();
				this.CreateHandle(cp);
			}

			///<summary>CADI.cs's two lines: the type of the ProgID, and its instance (which starts CADI).</summary>
			public static object CreateComObject() {
				Type progType=Type.GetTypeFromProgID(PROGRAM_ID);
				return Activator.CreateInstance(progType);
			}

			///<summary>Launches the program as a comObject, and passes CADI OLE commands to the program (CADI.cs's SendData, with the patient's values
			///worked out by the program; null: no patient).</summary>
			public void SendData(CadiPatient cadiPatient) {
				try {
					if(_comObject==null) {
						_comObject=_funcCreateComObject();
						//register the handle of this window with CADI to listen for application closing events
						int result=_comObject.OleClientWindowHandle(_windowHandle,OLECLIENTNOTIFY_APPLICATIONEXIT,WM_CLOSE,0,0);
						if(result!=0) { //0=success; 14=Invalid argument; -1=Unspecified error.
							throw new Exception("Unable to communicate with CADI. Verify it is installed and try again.");
						}
					}
					if(cadiPatient==null) {
						return;
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
				SwallowAnyException(() => { this.ReleaseHandle(); });
				_onClose();
				if(_comObject==null) {
					return;
				}
				object comObject=_comObject;
				SwallowAnyException(() => { _actionReleaseComObject(comObject); });
				_comObject=null;
			}

			///<summary>CodeBase's ODException.SwallowAnyException, which the helper does not reference.</summary>
			private static void SwallowAnyException(Action action) {
				try {
					action();
				}
				catch(Exception) {
				}
			}

			///<summary>Listen to when the handle changes to keep the variable in sync
			///See https://msdn.microsoft.com/en-us/library/system.windows.forms.nativewindow.handle(v=vs.110).aspx </summary>
			protected override void OnHandleChange() {
				_windowHandle = (int)this.Handle;
			}

			///<summary>Listen for messages being sent back to this handle by CADI. We need to include this to allow the CADI window to close properly.
			///See https://msdn.microsoft.com/en-us/library/system.windows.forms.nativewindow.handle(v=vs.110).aspx </summary>
			protected override void WndProc(ref Message m) {
				switch ((uint)m.Msg) {
					case WM_CLOSE: //Note, if in demo mode and user clicks 'Bye', this event won't be raised. This will fix itself if they open the bridge again.
						CloseWindow();
						break;
				}
				base.WndProc(ref m);
			}
		}
	}
}
