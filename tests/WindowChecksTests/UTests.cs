// Group U of the window checks (package UI-0c of docs/specs/ui-refresh-plan.md; docs/port-notes/UI-0c.md): the common windows of
// UiGallery\windows.tsv, the main window at fixed sizes with each module, and the patient dashboard, each opened in an in-process session by the
// program's own handlers on the scratch copy's synthetic patient, measured by the walker after its Load and printed, and closed with Cancel. A
// window the synthetic dump cannot fill is recorded SKIPPED with the reason its row gives. The check U0 runs at the end of the run
// (WinChkFixture.AssemblyCleanup), over the pictures of every group the run took. Opt-in like every group: OPMS_WINDOW_CHECKS=1 and the dump;
// OPMS_WINDOW_CHECKS_GROUPS=U for this group alone (or its parts UW, UM, UD). It needs the owner's connected, unlocked session: WPF draws
// nothing in a locked one.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowChecksTests {
	[TestClass]
	public class UTests {
		///<summary>Group U's parts, each also a group name of OPMS_WINDOW_CHECKS_GROUPS, so a run that would not fit the runner's 20 minutes can be
		///split: UW the common windows, UM the main window and its modules, UD the dashboard. U runs all three.</summary>
		public static readonly string[] ArrayParts=new[] { "UW","UM","UD" };

		///<summary>True when the run includes group U or one of its parts (U0 then runs at the end of the run).</summary>
		public static bool IsInRun {
			get {
				return WinChkEnv.GetSkipReason("U")==null || ArrayParts.Any(x => WinChkEnv.GetSkipReason(x)==null);
			}
		}

		///<summary>WinChkFixture.Begin for group U, or for the part alone when the run names it and not U.</summary>
		private static void BeginPart(string part) {
			if(WinChkEnv.GetSkipReason("U")!=null && WinChkEnv.GetSkipReason(part)==null) {
				WinChkFixture.Begin(part);
				return;
			}
			WinChkFixture.Begin("U");
		}

		///<summary>The rows of windows.tsv whose id has the prefix (U, UM or UD).</summary>
		private static List<UiWindowRow> GetRows(string prefix) {
			return UiWindows.Read().Where(x => System.Text.RegularExpressions.Regex.IsMatch(x.Id,"^"+prefix+@"\d+$")).ToList();
		}

		///<summary>Records the rows the dump cannot fill (SKIPPED with the reason), runs the others in process, and fails for a failed check.</summary>
		private static void RunRows(List<UiWindowRow> listRows,string module) {
			List<string> listFailed=new List<string>();
			foreach(UiWindowRow row in listRows.Where(x => x.Opened==UiWindowRow.OPENED_NONE)) {
				CheckResult checkResult=new CheckResult(row.Id,row.Title) { StatusForced=CheckStatus.Skipped,Reason="Not filled by the synthetic dump: "+row.Target+"." };
				Results.Write(checkResult);
				Console.WriteLine(checkResult.Id+" "+checkResult.Status+": "+checkResult.Summary);
			}
			List<UiWindowRow> listRun=listRows.Where(x => x.Opened!=UiWindowRow.OPENED_NONE).ToList();
			if(listRun.Count==0) {
				return;
			}
			try {
				CTests.RunInProcess(listRun.Select(x => new[] { x.Id,x.Title }).ToArray(),module);
			}
			catch(AssertFailedException ex) {
				listFailed.Add(ex.Message);
			}
			Assert.AreEqual(0,listFailed.Count,string.Join(" || ",listFailed));
		}

		[TestMethod]
		public void U1toU68_CommonWindows() {
			BeginPart("UW");
			RunRows(GetRows("U"),"");
		}

		[TestMethod]
		public void UM1toUM21_MainWindowAndModules() {
			BeginPart("UM");
			RunRows(GetRows("UM"),"");
		}

		[TestMethod]
		public void UD1_PatientDashboard() {
			BeginPart("UD");
			RunRows(GetRows("UD"),"");
		}
	}
}
