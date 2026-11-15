// The interface of a module of the 32-bit helper program (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 5).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.

namespace OpenPmsDentalHelper32 {
	///<summary>One kind of 32-bit vendor code that the helper runs for the program: QuickBooks Desktop (S3a-13); Schick CDR, Topaz and Guru
	///(S3a-14). A module is added to Helper32Modules.CreateModules and needs no change of the protocol: its name is the request's Module, its
	///commands and their arguments are its own (strings and one byte field each way).
	///
	///The helper calls Handle on its one main thread, a single-threaded apartment that the helper keeps for the whole process (COM objects such
	///as QBFC's session manager and Schick's CDR objects are created and called there, and a window such as Topaz's signing window can be shown
	///there modally, which runs a message loop), one request at a time, in the order the program sends them. A module keeps its state (an open
	///session, a loaded library) in its own fields for the helper's lifetime, which is that of one program process. Whatever Handle throws goes
	///back to the program as an error reply with the exception's type name and message; the helper keeps running.</summary>
	internal interface IHelper32Module {
		///<summary>The module's name in requests, such as "QuickBooks". Unique in the helper.</summary>
		string Name { get; }

		///<summary>Runs one command and returns its reply (Helper32Reply.Ok). Throws for a failed command or an unknown one.</summary>
		Helper32Reply Handle(Helper32Request request);
	}
}
