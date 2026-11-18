// The test assembly's start (package TC-3, docs/specs/wpf-tooth-chart-plan.md section 7): the live view's tests draw in software, as the program does.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToothChartImageTests {
	[TestClass]
	public class TestAssemblySetup {
		///<summary>Section 7: the tests that host the live view (O-19 (b)) set RenderOptions.ProcessRenderMode to SoftwareOnly, so the window WPF draws on
		///screen does not depend on the graphics card. It is also what OpenPmsDental.exe does first in Main (ProgramEntry.cs, a line from upstream;
		///owner question O-34), so the live view in these tests is drawn as the program draws it: in software at 1x. It changes nothing else:
		///RenderTargetBitmap renders in software anyway, and no test waits for a frame on screen.</summary>
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext testContext) {
			OpenPmsDentalTests.TestCodePages.Register();//the code pages first, as the program registers them (tests\Shared\TestCodePages.cs)
			//A default font before any window exists, as every exe's Main sets one (S3b-4). UI-2a-1 (docs/specs/ui-refresh-plan.md, section 9): Segoe UI
			//9, the default font of UI-2b, already now, so the Simple 2D baselines prove that the chart's pinned font (ToothChart2D and ToothChartWrapper:
			//Microsoft Sans Serif 8.25, which the chart draws its tooth numbers and texts with) holds before the flip (S3b-5W, docs/port-notes/S3b-5W.md).
			System.Windows.Forms.Application.SetDefaultFont(new System.Drawing.Font(CodeBase.UiTheme.FONT_FAMILY,CodeBase.UiTheme.FONT_SIZE_BODY));
			RenderOptions.ProcessRenderMode=RenderMode.SoftwareOnly;
		}
	}
}
