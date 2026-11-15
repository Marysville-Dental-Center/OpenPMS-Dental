// The synthetic stand-in for the Topaz signature pad's control that BINBTests' TopazWrapperTests load at run time (never Topaz's file): the
// same type name and the public methods TopazWrapper calls. Every call is recorded in the static Calls list. SetSigString("valid") gives 250
// points; "needs-bytes" gives points only after the private Sig.SetAutoKeyData(byte[]) was called; "throw" throws as Topaz's control does for a
// corrupt signature. The same code TopazWrapperTests compiled at run time before S3a-8b of the .NET 10 move. Since S3a-14 the control runs in
// the 32-bit helper program, so the tests read the calls through the control's own methods (GetRecordedCalls, ClearRecordedCalls,
// GetAssemblyLocation), and the control paints a signature (a black diagonal and a frame) when it has points, for the picture the helper draws.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Topaz {
	public class Signature {
		public bool IsKeyBytesSet;

		private void SetAutoKeyData(byte[] arrayBytes) {
			SigPlusNET.Calls.Add("Sig.SetAutoKeyData");
			IsKeyBytesSet=true;
		}
	}

	public class SigPlusNET:Control {
		public static List<string> Calls=new List<string>();
		private Signature Sig=new Signature();
		private int _points;
		private int _state;
		private string _sig="";

		public void ClearTablet() {
			Calls.Add("ClearTablet");
			_points=0;
		}

		public void SetTabletLogicalXSize(int x) {
			Calls.Add("SetTabletLogicalXSize:"+x);
		}

		public void SetTabletLogicalYSize(int y) {
			Calls.Add("SetTabletLogicalYSize:"+y);
		}

		public void SetSigCompressionMode(int mode) {
			Calls.Add("SetSigCompressionMode:"+mode);
		}

		public void SetEncryptionMode(int mode) {
			Calls.Add("SetEncryptionMode:"+mode);
		}

		public void SetKeyString(string key) {
			Calls.Add("SetKeyString:"+key);
		}

		public void AutoKeyStart() {
			Calls.Add("AutoKeyStart");
		}

		public void AutoKeyFinish() {
			Calls.Add("AutoKeyFinish");
		}

		public void SetAutoKeyData(string data) {
			Calls.Add("SetAutoKeyData:"+data);
		}

		public void SetAutoKeyANSIData(string data) {
			Calls.Add("SetAutoKeyANSIData:"+data);
		}

		public void SetSigString(string sig) {
			Calls.Add("SetSigString:"+sig);
			if(sig=="throw") {
				throw new IndexOutOfRangeException("corrupt signature");
			}
			_sig=sig;
			_points=(sig=="valid" || (sig=="needs-bytes" && Sig.IsKeyBytesSet)) ? 250 : 0;
		}

		public string GetSigString() {
			Calls.Add("GetSigString");
			return _sig;
		}

		public int NumberOfTabletPoints() {
			return _points;
		}

		public void SetTabletState(int state) {
			Calls.Add("SetTabletState:"+state);
			_state=state;
		}

		public int GetTabletState() {
			return _state;
		}

		///<summary>The calls recorded so far, one per line (not recorded itself).</summary>
		public string GetRecordedCalls() {
			return string.Join("\n",Calls);
		}

		public void ClearRecordedCalls() {
			Calls.Clear();
		}

		///<summary>Where this assembly was loaded from.</summary>
		public string GetAssemblyLocation() {
			return typeof(SigPlusNET).Assembly.Location;
		}

		///<summary>A signature with points: a frame and a diagonal in black on white. Without points: white only.</summary>
		protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.Clear(Color.White);
			if(_points>0) {
				e.Graphics.DrawRectangle(Pens.Black,0,0,Width-1,Height-1);
				e.Graphics.DrawLine(Pens.Black,0,0,Width-1,Height-1);
			}
		}
	}
}
