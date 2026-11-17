// Group C of the checklist: forms whose initializer order changed, each opened through the handler its click runs, in one in-process session
// (a new session after a check that hung).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowChecksTests {
	[TestClass]
	public class CTests {
		public static readonly string[][] ArrayChecks=new[] {
			new[] { "C1","Edit Patient Information (FormPatientEdit)" },
			new[] { "C2","Edit Procedure with signature box (FormProcEdit)" },
			new[] { "C3","Payment (FormPayment)" },
			new[] { "C4","Sheet fill (FormSheetFillEdit)" },
			new[] { "C5","Insurance benefits (FormInsBenefits)" },
			new[] { "C6","Query result window (FormQuery)" },
			new[] { "C7","Edit Deposit (FormDepositEdit)" },
			new[] { "C8","Time card (FormTimeCard)" },
			new[] { "C9","Edit Appointment (FormApptEdit)" },
			new[] { "C10","Database Maintenance list" },
			new[] { "C11","Claim forms" },
			new[] { "C12","Internal sheets and eForms" },
			new[] { "C13","CC Recurring Charges" },
			new[] { "C14","Billing options (statements)" },
			new[] { "C15","WebView2 window (Wiki)" },
		};

		///<summary>The stage 3b (.NET 10) default-font windows of section 6 of docs/specs/net10-plan.md (WINCHK-5): plain forms on the default font
		///the program sets (Microsoft Sans Serif 8.25, decision 9), each shown as the program shows it and printed. The splash is A1's.</summary>
		public static readonly string[][] ArrayChecksDefaultFont=new[] {
			new[] { "C16","Default font: MsgBoxCopyPaste" },
			new[] { "C17","Default font: progress windows (FormProgressStatus, FormProgressExtended)" },
			new[] { "C18","Default font: the combo box drop-down of the clinic picker (FormComboPicker)" },
			new[] { "C19","Default font: ToolTipOD" },
			new[] { "C20","Default font: the main menu (MenuOD) in its window's font" },
		};

		///<summary>Runs the in-process checks in one session, starting a new session after a check that did not end; records every result and
		///fails the test when one failed.</summary>
		public static void RunInProcess(string[][] arrayChecks,string module="",Action<CheckResult> actionAfterEach=null) {
			List<string> listFailed=new List<string>();
			InProcHost inProcHost=InProcHost.Start(module);
			try {
				foreach(string[] arrayCheck in arrayChecks) {
					CheckResult checkResult=inProcHost.Run(arrayCheck[0],arrayCheck[1]);
					actionAfterEach?.Invoke(checkResult);
					Results.Write(checkResult);
					Console.WriteLine(checkResult.Id+" "+checkResult.Status+": "+checkResult.Summary);
					if(checkResult.Status==CheckStatus.Failed) {
						listFailed.Add(checkResult.Id+": "+checkResult.Summary);
					}
					if(checkResult.ListFailures.Exists(x => x.Contains("did not end within") || x.Contains("did not answer"))) {
						inProcHost.Dispose();
						inProcHost=InProcHost.Start(module);
					}
				}
			}
			finally {
				inProcHost.Dispose();
			}
			Assert.AreEqual(0,listFailed.Count,string.Join(" || ",listFailed));
		}

		[TestMethod]
		public void C1toC15_FormsWhoseInitializerOrderChanged() {
			WinChkFixture.Begin("C");
			RunInProcess(ArrayChecks,"chart",checkResult => {
				if(checkResult.Id=="C15") {
					checkResult.Note("Exit after the Wiki: the in-process session does not exit the program; A4 times every out-of-process exit");
					checkResult.ForPerson("the exit time after a session that opened the Wiki (stage 0 finding 2) needs File > Exit in a real run");
				}
			});
		}

		[TestMethod]
		public void C16toC20_DefaultFontWindows() {
			WinChkFixture.Begin("C");
			RunInProcess(ArrayChecksDefaultFont,"chart");
		}
	}
}
