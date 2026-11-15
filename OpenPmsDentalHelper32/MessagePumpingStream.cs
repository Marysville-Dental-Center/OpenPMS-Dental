// The helper's wait for the next request, with the thread's messages pumped (S3a-14 of the .NET 10 move, docs/port-notes/S3a-14.md, section 6).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Threading;

namespace OpenPmsDentalHelper32 {
	///<summary>The program's pipe for the request loop: every read is an overlapped read, and while it waits the helper's main thread (the STA of
	///every module) dispatches its window messages (MsgWaitForMultipleObjectsEx with QS_ALLINPUT). Without it, the thread would wait between two
	///requests in a read that pumps nothing, and the windows and COM objects the modules keep on it would not answer: Topaz's signature control
	///(a WinForms control with a window) and Schick CDR's exam document, whose OnClose event is a call from CDR's process into this apartment that
	///only a dispatched message delivers. A broadcast to the thread's top-level windows (WM_SETTINGCHANGE) would hang its sender too. Writes go
	///to the pipe as they are. The pipe must be opened for asynchronous I/O (PipeOptions.Asynchronous).</summary>
	internal sealed class MessagePumpingStream:Stream {
		private readonly Stream _stream;

		public MessagePumpingStream(Stream stream) {
			_stream=stream;
		}

		public override bool CanRead {
			get {
				return _stream.CanRead;
			}
		}

		public override bool CanSeek {
			get {
				return false;
			}
		}

		public override bool CanWrite {
			get {
				return _stream.CanWrite;
			}
		}

		public override long Length {
			get {
				throw new NotSupportedException();
			}
		}

		public override long Position {
			get {
				throw new NotSupportedException();
			}
			set {
				throw new NotSupportedException();
			}
		}

		public override int Read(byte[] buffer,int offset,int count) {
			IAsyncResult asyncResult=_stream.BeginRead(buffer,offset,count,null,null);
			WaitPumping(asyncResult.AsyncWaitHandle,() => asyncResult.IsCompleted);
			return _stream.EndRead(asyncResult);
		}

		///<summary>Waits until waitHandle is set, dispatching every message of this thread meanwhile. A failed wait (it never fails for a valid
		///handle) falls back to a plain wait, so a request is never missed.</summary>
		public static void WaitPumping(WaitHandle waitHandle,Func<bool> funcIsDone) {
			IntPtr[] arrayHandles=new[] { waitHandle.SafeWaitHandle.DangerousGetHandle() };
			while(!funcIsDone()) {
				uint result=NativeMethods.MsgWaitForMultipleObjectsEx(1,arrayHandles,NativeMethods.INFINITE,NativeMethods.QS_ALLINPUT,
					NativeMethods.MWMO_INPUTAVAILABLE);
				if(result==NativeMethods.WAIT_OBJECT_0) {
					return;
				}
				if(result==NativeMethods.WAIT_OBJECT_0+1) {
					PumpMessages();
					continue;
				}
				waitHandle.WaitOne();
				return;
			}
		}

		///<summary>Dispatches the messages in this thread's queue. A WM_QUIT (posted by vendor code) is dropped: the helper ends only when the
		///program closes its pipe or ends.</summary>
		public static void PumpMessages() {
			while(NativeMethods.PeekMessage(out NativeMethods.MSG msg,IntPtr.Zero,0,0,NativeMethods.PM_REMOVE)) {
				if(msg.message==0x0012) {//WM_QUIT
					continue;
				}
				NativeMethods.TranslateMessage(ref msg);
				NativeMethods.DispatchMessage(ref msg);
			}
		}

		public override void Write(byte[] buffer,int offset,int count) {
			_stream.Write(buffer,offset,count);
		}

		public override void Flush() {
			_stream.Flush();
		}

		public override long Seek(long offset,SeekOrigin origin) {
			throw new NotSupportedException();
		}

		public override void SetLength(long value) {
			throw new NotSupportedException();
		}
	}
}
