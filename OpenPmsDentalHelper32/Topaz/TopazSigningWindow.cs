// The helper's own small window in which a patient signs on a Topaz pad (S3a-14 of the .NET 10 move, docs/port-notes/S3a-14.md, section 5).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenPmsDentalHelper32.Topaz {
	///<summary>Holds the program's Topaz control while the patient signs: the control on top, the OK, Clear and Cancel buttons (the program's
	///translated texts) below. The program has already cleared the pad and turned capture on (TopazWrapper, as before S3a-14), so the control
	///shows the signature as the pen draws it. Clear clears the pad as TopazWrapper.ClearTopaz did; Cancel clears it and closes; OK closes and
	///keeps the signature. The window looks different from the box inside the program's window that the patient watched before (release notes);
	///the signature data does not change. Shown over the program's window (ShowOver), never by the tests, which press its buttons through
	///PressOk, PressClear and PressCancel.</summary>
	internal sealed class TopazSigningWindow:Form {
		private const int MARGIN=8;
		private readonly Control _topaz;
		private readonly Button _butOk;
		private readonly Button _butClear;
		private readonly Button _butCancel;
		private bool _isControlReleased;

		public TopazSigningWindow(Control topaz,string title,string textOk,string textClear,string textCancel) {
			_topaz=topaz;
			Text=title;
			FormBorderStyle=FormBorderStyle.FixedDialog;
			MinimizeBox=false;
			MaximizeBox=false;
			ShowInTaskbar=false;
			ShowIcon=false;
			StartPosition=FormStartPosition.Manual;
			AutoScaleMode=AutoScaleMode.None;//the control has the program's pixel size already
			_butOk=NewButton(textOk,"butOk");
			_butClear=NewButton(textClear,"butClear");
			_butCancel=NewButton(textCancel,"butCancel");
			_butOk.Click+=(sender,e) => PressOk();
			_butClear.Click+=(sender,e) => PressClear();
			_butCancel.Click+=(sender,e) => PressCancel();
			int widthButtons=_butOk.Width+_butClear.Width+_butCancel.Width+2*MARGIN;
			int width=Math.Max(topaz.Width,widthButtons);
			topaz.Location=new Point(MARGIN+(width-topaz.Width)/2,MARGIN);
			topaz.Visible=true;
			int yButtons=topaz.Bottom+MARGIN;
			_butCancel.Location=new Point(MARGIN+width-_butCancel.Width,yButtons);
			_butClear.Location=new Point(_butCancel.Left-MARGIN-_butClear.Width,yButtons);
			_butOk.Location=new Point(_butClear.Left-MARGIN-_butOk.Width,yButtons);
			ClientSize=new Size(width+2*MARGIN,yButtons+_butOk.Height+MARGIN);
			Controls.Add(topaz);
			Controls.Add(_butOk);
			Controls.Add(_butClear);
			Controls.Add(_butCancel);
			AcceptButton=_butOk;
			CancelButton=_butCancel;
		}

		private static Button NewButton(string text,string name) {
			Button button=new Button();
			button.Name=name;
			button.Text=text;
			button.Size=new Size(75,24);
			button.UseVisualStyleBackColor=true;
			return button;
		}

		///<summary>The control, its buttons and their places (tests read them without showing the window).</summary>
		internal Control ControlTopaz {
			get {
				return _topaz;
			}
		}

		internal Button ButtonOk {
			get {
				return _butOk;
			}
		}

		internal Button ButtonClear {
			get {
				return _butClear;
			}
		}

		internal Button ButtonCancel {
			get {
				return _butCancel;
			}
		}

		internal void PressOk() {
			DialogResult=DialogResult.OK;
		}

		internal void PressClear() {
			ClearPad(_topaz);
		}

		internal void PressCancel() {
			ClearPad(_topaz);
			DialogResult=DialogResult.Cancel;
		}

		///<summary>The calls of TopazWrapper.ClearTopaz (without its plug-in hook, which is the program's): ClearTablet and the logical size
		///2000 by 600.</summary>
		internal static void ClearPad(Control topaz) {
			TopazModule.Call(topaz,"ClearTablet");
			TopazModule.Call(topaz,"SetTabletLogicalXSize",2000);
			TopazModule.Call(topaz,"SetTabletLogicalYSize",600);
		}

		///<summary>Takes the control out of the window, so disposing of the window does not dispose of it.</summary>
		internal void ReleaseControl() {
			if(_isControlReleased) {
				return;
			}
			_isControlReleased=true;
			Controls.Remove(_topaz);
		}

		protected override void Dispose(bool disposing) {
			if(disposing) {
				ReleaseControl();
			}
			base.Dispose(disposing);
		}

		///<summary>Shows window modally, centered over the program's window (handleOwner, a window of the program's process), and returns how it
		///closed. WinForms disables only this thread's windows for a modal window, so the program disables its own while it waits
		///(TopazWrapper.ShowSigningWindow).</summary>
		internal static DialogResult ShowOver(TopazSigningWindow window,IntPtr handleOwner) {
			IWin32Window owner=null;
			if(handleOwner!=IntPtr.Zero && NativeMethods.IsWindow(handleOwner)) {
				owner=new OwnerWindow(handleOwner);
				if(NativeMethods.GetWindowRect(handleOwner,out NativeMethods.RECT rect)) {
					window.Location=new Point(rect.Left+(rect.Right-rect.Left-window.Width)/2,rect.Top+(rect.Bottom-rect.Top-window.Height)/2);
				}
			}
			else {
				window.StartPosition=FormStartPosition.CenterScreen;
			}
			window.TopMost=owner==null;//without an owner it would open behind the program
			return owner==null ? window.ShowDialog() : window.ShowDialog(owner);
		}

		///<summary>The program's window as the signing window's owner.</summary>
		private sealed class OwnerWindow:IWin32Window {
			public OwnerWindow(IntPtr handle) {
				Handle=handle;
			}

			public IntPtr Handle { get; }
		}
	}
}
