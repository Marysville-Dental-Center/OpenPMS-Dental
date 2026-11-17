// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NativeFirstTests {
	[TestClass]
	public static class AssemblySetup {
		///<summary>Unit-test mode, so the choices' log lines (NativeFirstChooser.LogLine, which the tests read) are not written to a Logger folder
		///beside the test assembly.</summary>
		[AssemblyInitialize]
		public static void Initialize(TestContext testContext) {
			OpenPmsDentalTests.TestCodePages.Register();//the code pages first, as the program registers them (tests\Shared\TestCodePages.cs)
			ODBuild.IsUnitTest=true;
		}
	}
}
