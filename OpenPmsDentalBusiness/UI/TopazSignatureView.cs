// The program's side of one Topaz signature control (S3a-14 of the .NET 10 move, docs/port-notes/S3a-14.md, section 5): the control itself lives
// in the 32-bit helper program (OpenPmsDentalHelper32, its Topaz module); this control stands where Topaz's control stood in the program's
// windows and shows the helper's picture of it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalHelper32.Topaz;

namespace OpenPmsDental.UI {
	///<summary>What TopazWrapper.GetTopaz returns since S3a-14: a control of the program's window (location, size, visibility, focus, events and
	///DrawToBitmap as before) whose Topaz.SigPlusNET control is in the 32-bit helper. TopazWrapper sends that control the calls it made on it
	///before, one by one (Call); this control then shows the helper's picture of it, asked for when it paints (or is drawn into a bitmap) after a
	///call changed it. The helper's control is created on the first call (no helper is started for a box that is never used) and disposed of
	///with this control. A helper that ended since (a crash in Topaz's code) has lost the control: the next call creates it again, with a fresh
	///state, as each of TopazWrapper's sequences starts from a cleared pad.</summary>
	public class TopazSignatureView:Control {
		///<summary>How long one call may take. A call of the control returns at once; the wait covers a busy helper.</summary>
		private static readonly TimeSpan _timeSpanCall=TimeSpan.FromSeconds(60);
		///<summary>How long the signing window may stay open.</summary>
		private static readonly TimeSpan _timeSpanSign=TimeSpan.FromMinutes(30);
		private readonly Helper32Client _helper32Client;
		private readonly string _pathAssembly;
		private string _id;
		private Bitmap _bitmapPicture;
		private Size _sizePicture;
		private bool _isPictureStale=true;

		///<summary>pathAssembly is the SigPlusNET.dll the program found (TopazWrapper.FindAssemblyPath), which the helper loads.</summary>
		public TopazSignatureView(Helper32Client helper32Client,string pathAssembly) {
			_helper32Client=helper32Client;
			_pathAssembly=pathAssembly;
			SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);
			Size=new Size(362,79);
		}

		///<summary>The helper's id of the control; null before the first call (tests).</summary>
		public string HelperControlId {
			get {
				return _id;
			}
		}

		#region Calls
		private void EnsureCreated() {
			if(_id!=null) {
				return;
			}
			_id=_helper32Client.Call(TopazCommands.MODULE,TopazCommands.CREATE,_timeSpanCall,_pathAssembly)[0];
		}

		///<summary>Calls command for the helper's control. A control the helper does not know (its helper ended) is created again and the call is
		///made once more.</summary>
		private Helper32Result CallHelper(string command,string[] arrayArgsAfterId,byte[] data,TimeSpan timeSpan) {
			EnsureCreated();
			try {
				return _helper32Client.Call(TopazCommands.MODULE,command,new[] { _id }.Concat(arrayArgsAfterId).ToArray(),data,timeSpan);
			}
			catch(Helper32Exception ex) when(IsControlGone(ex)) {
				_id=null;
				EnsureCreated();
				return _helper32Client.Call(TopazCommands.MODULE,command,new[] { _id }.Concat(arrayArgsAfterId).ToArray(),data,timeSpan);
			}
			finally {
				MarkPictureStale();
			}
		}

		private static bool IsControlGone(Helper32Exception ex) {
			return ex.Failure==Helper32Failure.Remote && ex.RemoteTypeName==typeof(TopazControlGoneException).FullName;
		}

		///<summary>The control's public instance method of that name, with these arguments (ints and strings), and what it returned (null for
		///void). What the method threw comes as a Helper32Exception (Remote) with its message and type name.</summary>
		public object Call(string methodName,params object[] arrayArgs) {
			string[] arrayArgsAfterId=new[] { methodName }.Concat(arrayArgs.SelectMany(x => TopazCommands.EncodeValue(x))).ToArray();
			string[] arrayResults=CallHelper(TopazCommands.CALL,arrayArgsAfterId,null,_timeSpanCall).Results;
			return TopazCommands.DecodeValue(arrayResults[0],arrayResults[1]);
		}

		///<summary>Whether the control's private Sig.SetAutoKeyData(byte[]) can be reached (TopazWrapper.FillSignatureEncodings).</summary>
		public bool CanSetAutoKeyBytes() {
			return CallHelper(TopazCommands.CAN_SET_AUTO_KEY_BYTES,new string[0],null,_timeSpanCall).Results[0]=="1";
		}

		///<summary>Sig.SetAutoKeyData(byte[]) with these bytes.</summary>
		public void SetAutoKeyBytes(byte[] arrayBytes) {
			CallHelper(TopazCommands.SET_AUTO_KEY_BYTES,new string[0],arrayBytes,_timeSpanCall);
		}
		#endregion Calls

		#region Picture
		private void MarkPictureStale() {
			_isPictureStale=true;
			if(IsHandleCreated) {
				Invalidate();
			}
		}

		///<summary>The helper's picture of the control at this control's size, asked for once after each change. Null when there is none (a
		///control never called, a helper that failed: nothing is drawn, as the empty Topaz control drew nothing).</summary>
		public Bitmap GetPicture() {
			if(_id==null || Width<=0 || Height<=0) {
				return null;
			}
			if(!_isPictureStale && _sizePicture==Size) {
				return _bitmapPicture;
			}
			if(_helper32Client.IsCallPumping) {
				return _bitmapPicture;//the helper shows the signing window (its picture comes with its reply) or Schick CDR's exam window
			}
			_isPictureStale=false;
			_sizePicture=Size;
			try {
				byte[] arrayPng=_helper32Client.Call(TopazCommands.MODULE,TopazCommands.PICTURE,
					new[] { _id,TopazCommands.IntToText(Width),TopazCommands.IntToText(Height) },null,_timeSpanCall).Data;
				SetPicture(arrayPng);
			}
			catch(Helper32Exception ex) {
				ex.DoNothing();//A missing picture is not a reason to stop the window; the next call asks again.
				SetPicture(null);
			}
			return _bitmapPicture;
		}

		private void SetPicture(byte[] arrayPng) {
			_bitmapPicture?.Dispose();
			_bitmapPicture=null;
			if(arrayPng==null || arrayPng.Length==0) {
				return;
			}
			using(MemoryStream memoryStream=new MemoryStream(arrayPng))
			using(Image image=Image.FromStream(memoryStream)) {
				_bitmapPicture=new Bitmap(image);//not tied to the stream
			}
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			Bitmap bitmap=GetPicture();
			if(bitmap!=null) {
				e.Graphics.DrawImage(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));
			}
		}
		#endregion Picture

		#region Signing
		///<summary>Shows the helper's signing window over the program's window (handleOwner) until the patient presses OK or Cancel, and takes
		///the picture it sends back. Blocks this thread's windows and keeps them painting meanwhile (Helper32Client.CallPumping), then activates
		///the window that was active. True for OK.</summary>
		internal bool Sign(IntPtr handleOwner,string title,string textOk,string textClear,string textCancel) {
			EnsureCreated();
			//The call runs on another thread and touches nothing of this control: the helper's control was just created or used.
			string[] arrayArgs=new[] { _id,TopazCommands.HandleToText(handleOwner),TopazCommands.IntToText(Width),TopazCommands.IntToText(Height),
				title,textOk,textClear,textCancel };
			Helper32Result helper32Result=_helper32Client.CallPumping(TopazCommands.MODULE,TopazCommands.SIGN,arrayArgs,null,_timeSpanSign,
				isReactivating:true);
			SetPicture(helper32Result.Data);
			_sizePicture=Size;
			_isPictureStale=false;
			Invalidate();
			return helper32Result.Results[0]=="1";
		}
		#endregion Signing

		protected override void Dispose(bool disposing) {
			if(disposing) {
				string id=_id;
				_id=null;
				if(id!=null && !_helper32Client.IsCallPumping && _helper32Client.ProcessId!=0) {
					try {
						_helper32Client.Call(TopazCommands.MODULE,TopazCommands.RELEASE,_timeSpanCall,id);
					}
					catch(Exception ex) {
						ex.DoNothing();//The helper ended: the control went with it.
					}
				}
				_bitmapPicture?.Dispose();
				_bitmapPicture=null;
			}
			base.Dispose(disposing);
		}
	}
}
