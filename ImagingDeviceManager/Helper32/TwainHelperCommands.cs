// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.

namespace ImagingDeviceManager.Helper32 {
	///<summary>The TWAIN module of the 32-bit helper (S3b-8; docs/port-notes/S3b-8.md, section 3): its name and commands, which this library
	///(the program's side, TwainHelperCaptureSession and TwainSourcePath) and the helper (OpenPmsDentalHelper32\Twain\TwainModule.cs, which runs
	///TwainHelperServer) both compile from this file. Every command's string arguments start with the session id, except ListSources and Hello.
	///Structures cross in the request's and the reply's byte field, written by TwainHelperWire.</summary>
	public static class TwainHelperCommands {
		public const string MODULE="Twain";
		///<summary>The version of these commands and TwainHelperWire's layout. Hello returns it; the program refuses another.</summary>
		public const int VERSION=1;

		///<summary>No arguments. Results: VERSION, the helper's process bitness ("32"), the DSM library the helper would load ("" when none), and
		///the sizes of TW_USERINTERFACE and TW_IMAGEMEMXFER in its process (8 and 38 in a 32-bit one, S3a-12's x86 column), for the tests.</summary>
		public const string Hello="Hello";
		///<summary>No arguments. Results: the DSM's library name (TWAINDSM.dll or twain_32.dll; "" when none loads), the error text ("" when none).
		///Data: the sources (TwainHelperWire.WriteSources).</summary>
		public const string ListSources="ListSources";
		///<summary>Args: session id, ProductName, SessionGuid ("D"), "1" to read the capture metadata keys (CaptureMetadataParser) or "0", the
		///capture folder for tests ("" = the default; honoured only by a helper started with the test DSM), the QuarantineContext's PatNum and
		///patient name. Data: TwainCaptureOptions. Blocks
		///until the source is open or failed (TwainCaptureSession.Open). Data back: TwainCaptureOpenResult.</summary>
		public const string Open="Open";
		///<summary>Args: session id, the program's enable id, "1" for a redelivery enable or "0". Data: TwainEnableRequest (without Tag).
		///Results: the helper session's enable id ("0": nothing was enabled, as Enable returns 0).</summary>
		public const string Enable="Enable";
		///<summary>Args: session id, TwainCaptureStopReason.</summary>
		public const string Disable="Disable";
		///<summary>Args: session id.</summary>
		public const string BeginClose="BeginClose";
		///<summary>Args: session id, PatNum, patient name.</summary>
		public const string SetQuarantineContext="SetQuarantineContext";
		///<summary>Args: session id, how long to wait for the first event in milliseconds (at most POLL_WAIT_MAX_MS). Results: "1" when the
		///session's worker has ended and every event was taken, else "0". Data: the events (TwainHelperWire.WriteEvents); an image's pixels are
		///read with ReadData.</summary>
		public const string Poll="Poll";
		///<summary>Args: session id, payload id, offset, count (at most DATA_CHUNK_MAX). Data back: those bytes. The payload is released when its
		///last byte was read.</summary>
		public const string ReadData="ReadData";
		///<summary>Args: session id. Closes the session if it still runs (BeginClose, a bounded wait) and forgets it.</summary>
		public const string Release="Release";

		///<summary>The longest wait of one Poll in the helper: short, so that a command the program sends meanwhile waits little.</summary>
		public const int POLL_WAIT_MAX_MS=200;
		///<summary>The largest piece of an image's pixels one ReadData carries, well below the protocol's 64 MB message limit.</summary>
		public const int DATA_CHUNK_MAX=8*1024*1024;
	}
}
