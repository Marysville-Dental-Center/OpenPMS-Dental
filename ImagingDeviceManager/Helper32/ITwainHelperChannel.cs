// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace ImagingDeviceManager.Helper32 {
	///<summary>Why a call to the 32-bit helper's TWAIN module failed (the helper client's failures, as this library sees them).</summary>
	public enum TwainHelperFailure {
		///<summary>OpenPmsDentalHelper32.exe is not in the program folder's Helper32\.</summary>
		Missing,
		///<summary>The helper did not start, connect or say hello in time.</summary>
		StartFailed,
		///<summary>The helper did not answer the call in time; it was ended.</summary>
		TimedOut,
		///<summary>The helper ended during the call.</summary>
		Crashed,
		///<summary>The TWAIN module threw: the message is its exception's message.</summary>
		Remote,
		///<summary>The messages did not follow the protocol (another helper, a message too long).</summary>
		Protocol,
		///<summary>The helper that answered is not the one the session was opened in (it ended between two calls and was replaced): the session's
		///source and its images in that helper are gone.</summary>
		Replaced,
	}

	///<summary>A failed call to the helper's TWAIN module. Message is a sentence for the capture log and the window.</summary>
	public sealed class TwainHelperException:Exception {
		public TwainHelperFailure Failure { get; }

		public TwainHelperException(TwainHelperFailure failure,string message,Exception innerException=null) : base(message,innerException) {
			Failure=failure;
		}
	}

	///<summary>The program's way to the TWAIN module of the 32-bit helper (S3b-8): one call at a time, each with its own timeout, as the helper
	///client makes them (CodeBase's Helper32Client; ImagingCapture's TwainHelperChannel wraps it, since this library does not reference CodeBase).
	///Tests give an in-memory channel that calls TwainHelperServer directly, and one that fails as the helper can.</summary>
	public interface ITwainHelperChannel {
		///<summary>Calls command of the TWAIN module with args and data. Throws TwainHelperException for every failure.</summary>
		string[] Call(string command,string[] args,byte[] data,out byte[] dataReply,TimeSpan timeout);
		///<summary>The process id of the helper that answers now (0 when none runs): a session checks after each call that it is still the helper
		///it was opened in.</summary>
		int ProcessId { get; }
		///<summary>Lets the helper bring a window to the foreground on its next call (the source's own window at an enable), as the program could
		///when the source ran in its process. Never throws.</summary>
		void AllowSetForeground();
	}
}
