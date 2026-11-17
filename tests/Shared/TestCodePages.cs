// The code pages for the tests (S3b-5 of the .NET 10 move, docs/specs/net10-plan.md, the S3b-5 row; S3b-0 item 6). .NET has only the Unicode
// encodings, ASCII and Latin-1 built in; the program registers the others (1252 for ANSI text, 850 for Canada, and the rest) first in
// ODInitialize.Initialize() and in every Main that does not call it (S3b-2). A test calls the libraries without either, so every test assembly
// registers them once before its first test, as the program does at its start: tests\Directory.Build.props links this file into every test
// project, with TestCodePagesAssemblySetup.cs where the project has no [AssemblyInitialize] of its own; a project with its own calls
// TestCodePages.Register() first in it. Registering twice is harmless. Whether ODInitialize itself registers them, in a fresh process before
// any other code ran, is checked by NoHqTests (EncodingsAfterInitializeTests).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Text;

namespace OpenPmsDentalTests {
	///<summary>Registers the code pages of the .NET Framework (CodePagesEncodingProvider) for a test assembly.</summary>
	public static class TestCodePages {
		public static void Register() {
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}
	}
}
