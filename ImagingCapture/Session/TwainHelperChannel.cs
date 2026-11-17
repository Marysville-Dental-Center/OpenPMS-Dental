// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.InteropServices;
using CodeBase;
using ImagingDeviceManager.Capture;
using ImagingDeviceManager.Helper32;

namespace ImagingCapture.Session {
	///<summary>The 32-bit TWAIN path's way to the helper (S3b-8; docs/port-notes/S3b-8.md): ImagingDeviceManager's ITwainHelperChannel on
	///CodeBase's Helper32Client, the client of OpenPmsDentalHelper32 (S3a-13), calling its TWAIN module. Default is a helper process of its own,
	///not Helper32Client.Default: a TWAIN session polls its helper several times a second for as long as it is open, and a call of another
	///module that waits for the user (Topaz's signing window, QuickBooks' access question, up to 30 minutes) would hold that client and stall the
	///capture; a TWAIN driver that crashes ends only this helper, not the state of the other modules. Both helpers end with the program.
	///</summary>
	public sealed class TwainHelperChannel:ITwainHelperChannel {
		private static readonly object _lockDefault=new object();
		private static TwainHelperChannel _channelDefault;
		private readonly Helper32Client _client;

		public TwainHelperChannel(Helper32Client client) {
			_client=client;
		}

		///<summary>The program's: a client of Helper32\OpenPmsDentalHelper32.exe of the program's folder, started on the first call, disposed at
		///the program's exit (and its helper ends with the program's process however it ends, Helper32Client's job object).</summary>
		public static TwainHelperChannel Default {
			get {
				lock(_lockDefault) {
					if(_channelDefault==null) {
						Helper32Client client=new Helper32Client(Helper32Client.GetDefaultPath());
						AppDomain.CurrentDomain.ProcessExit+=(sender,e) => client.Dispose();
						_channelDefault=new TwainHelperChannel(client);
					}
					return _channelDefault;
				}
			}
		}

		public int ProcessId => _client.ProcessId;

		public string[] Call(string command,string[] args,byte[] data,out byte[] dataReply,TimeSpan timeout) {
			try {
				Helper32Result result=_client.Call(TwainHelperCommands.MODULE,command,args,data,timeout);
				dataReply=result.Data;
				return result.Results;
			}
			catch(Helper32Exception ex) {
				throw new TwainHelperException(ToFailure(ex.Failure),ex.Message,ex);
			}
			catch(ObjectDisposedException ex) {
				throw new TwainHelperException(TwainHelperFailure.StartFailed,"The 32-bit helper is closed: the program is ending.",ex);
			}
		}

		public static TwainHelperFailure ToFailure(Helper32Failure failure) {
			switch(failure) {
				case Helper32Failure.Missing: return TwainHelperFailure.Missing;
				case Helper32Failure.StartFailed: return TwainHelperFailure.StartFailed;
				case Helper32Failure.TimedOut: return TwainHelperFailure.TimedOut;
				case Helper32Failure.Crashed: return TwainHelperFailure.Crashed;
				case Helper32Failure.Remote: return TwainHelperFailure.Remote;
				default: return TwainHelperFailure.Protocol;
			}
		}

		///<summary>AllowSetForegroundWindow for the running helper's process, without the client's lock (a Poll may hold it), so the caller's
		///thread never waits here. Nothing when no helper runs.</summary>
		public void AllowSetForeground() {
			int processId=_client.ProcessId;
			if(processId<=0) {
				return;
			}
			try {
				AllowSetForegroundWindow((uint)processId);
			}
			catch(Exception) {
				//A refused grant: the source's window may open behind the program's, or flash on the task bar.
			}
		}

		[DllImport("user32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AllowSetForegroundWindow(uint processId);
	}

	///<summary>The capture sessions the program creates (S3b-8): TwainCaptureSessionRouter on the native-first choice (TwainSourcePath) with the
	///helper channel, and the device list of every TWAIN source with its stored text.</summary>
	public static class TwainCaptureSessions {
		///<summary>The program's choice of the DSM, with TwainHelperChannel.Default for the 32-bit path.</summary>
		public static TwainSourcePath SourcePathDefault {
			get {
				return new TwainSourcePath(() => TwainHelperChannel.Default);
			}
		}

		///<summary>A session for a stored choice (ImagingDevice.TwainName): native first, the 32-bit helper for a source only the 32-bit DSM lists.
		///</summary>
		public static ITwainCaptureSession Create() {
			return new TwainCaptureSessionRouter(SourcePathDefault,() => TwainHelperChannel.Default);
		}

		///<summary>Every TWAIN source of this computer for a device, native first (TwainSourcePath.ListAll): the program's own DSM's, then those
		///only the 32-bit DSM lists, which carry the 32-bit marker in their StoredText.</summary>
		public static System.Collections.Generic.List<TwainSourceEntry> ListSources(out string errorText) {
			return SourcePathDefault.ListAll(out errorText);
		}
	}
}
