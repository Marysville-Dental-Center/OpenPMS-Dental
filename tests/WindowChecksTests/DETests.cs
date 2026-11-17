// Groups D and E of the checklist: the WPF windows of WpfControlsOD and the capture windows of ImagingCapture, in process.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowChecksTests {
	[TestClass]
	public class DETests {
		[TestMethod]
		public void D1toD5_WpfWindows() {
			WinChkFixture.Begin("D");
			CTests.RunInProcess(new[] {
				new[] { "D1","Patient select (FrmPatientSelect)" },
				new[] { "D2","Account edit (FrmAccountEdit)" },
				new[] { "D3","Spell check in TextRich (FrmCommItem)" },
				new[] { "D4","eForm editor (FrmEFormDefEdit)" },
				new[] { "D5","eForm fill and print preview (FrmEFormFillEdit, FrmPrintPreview)" },
			},"chart");
		}

		///<summary>Stage 3b (WINCHK-5's fix round): the WPF pop-ups of a Frm hosted in FormFrame.</summary>
		[TestMethod]
		public void D6_WpfPopups() {
			WinChkFixture.Begin("D");
			CTests.RunInProcess(new[] { new[] { "D6","WPF pop-ups of a Frm: combo box picker, month calendar, tool tip" } },"chart");
		}

		///<summary>Stage 3b's launch check of the WebView2 windows (LAUNCH-3b, docs/launch-checks-3b.md): the e-mail preview and the program's WPF
		///WebView2 control (Microsoft.Web.WebView2.Wpf), each with its page loaded and pictured. Needs the desktop (the browser renders only in a
		///connected session): the lead runs it at a connected session.</summary>
		[TestMethod]
		public void D7_WebView2Windows() {
			WinChkFixture.Begin("D");
			CTests.RunInProcess(new[] { new[] { "D7","WebView2 windows: the e-mail preview and the WPF WebView2 control (Microsoft.Web.WebView2.Wpf)" } },"chart");
		}

		[TestMethod]
		public void E1toE4_CaptureWindows() {
			WinChkFixture.Begin("E");
			CTests.RunInProcess(new[] {
				new[] { "E1","Unassigned captures (FrmCaptureQuarantine)" },
				new[] { "E2","Standard templates (FrmStandardTemplates)" },
				new[] { "E3","Capture settings (FrmCaptureSettings)" },
				new[] { "E4","Test capture (FrmTestCapture)" },
			},"chart");
		}
	}
}
