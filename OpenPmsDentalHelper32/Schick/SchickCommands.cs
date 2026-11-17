// The Schick CDR module's commands and their arguments (S3a-14 of the .NET 10 move, docs/port-notes/S3a-14.md, section 3). Compiled into the
// helper and linked into WpfControlsOD (Bridges\Schick.cs), so the program and the helper name them alike.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.

namespace OpenPmsDentalHelper32.Schick {
	internal static class SchickCommands {
		public const string MODULE="Schick";
		///<summary>Args: the patient's last name, first name and the patient id (PatNum or ChartNumber, as the program property says), exactly as
		///Schick.ShowExam took them before S3a-14. Results: none. Loads the patient in CDR DICOM's exam window, or creates the exam.</summary>
		public const string SHOW_EXAM="ShowExam";
	}
}
