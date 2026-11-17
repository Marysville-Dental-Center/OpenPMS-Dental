// The [AssemblyInitialize] that registers the code pages (TestCodePages.cs) in a test project that has no [AssemblyInitialize] of its own
// (MSTest allows one per assembly). tests\Directory.Build.props links it unless the project sets ODOwnAssemblyInitialize.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenPmsDentalTests {
	[TestClass]
	public class TestCodePagesAssemblySetup {
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext testContext) {
			TestCodePages.Register();
		}
	}
}
