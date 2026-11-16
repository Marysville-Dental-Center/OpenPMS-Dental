// Groups I and J of the checklist: the tooth chart and EHR-0/EHR-1. What this harness reaches runs in process (the chart's handlers, Show
// Features, the Setup Wizard, the EHR setup with the 0.4 test users and their log-offs, the print previews, the dashboard widget, the e-mail
// windows) or out of process (I8's WPF chart start, I11's start-up with no DirectX format saved, which searches and saves nothing since TC-8).
// Every check after the first start of a group runs in
// a new session where the checklist says "exit; start".
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowChecksTests {
	[TestClass]
	public class IJTests {
		[TestMethod]
		public void I1toI11_ToothChart() {
			WinChkFixture.Begin("I");
			List<CheckResult> listResults=new List<CheckResult>();
			//I1 to I7 in one session (I5's dashboard copy and I6's exam stay until I10; I6's 2D setting until I7). A check that leaves the session stuck
			//(a window that does not close) gets the rest of them a new session, so one hang costs one check, not the group; the rows a check made reach
			//the later sessions as keys (InProcHost), and the run's time budget (WinChkFixture) stops the group before the runner's 20-minute kill.
			InProcHost inProcHostI=InProcHost.Start("chart");
			try {
				CheckResult checkResultPrep=inProcHostI.Run("IPREP","I prep: D2391 on 30 O");
				foreach(string[] arrayCheck in new[] { new[] { "I1","Refill, slider, Show Big" },new[] { "I2","Save to Images" },
					new[] { "I3","Treatment Plan print preview, classic and sheet" },new[] { "I4","Referral letter" },new[] { "I5","Dashboard widget, open and closed" },
					new[] { "I6","Graphical perio, refused on the Simple 2D chart" },new[] { "I7","Graphics: the 3D Tooth Chart settings, Draw in software" } })
				{
					if(inProcHostI.NeedsNewSession) {
						inProcHostI.Dispose();
						inProcHostI=InProcHost.Start("chart");
					}
					CheckResult checkResult=inProcHostI.Run(arrayCheck[0],arrayCheck[1],timeoutSeconds:360);
					if(arrayCheck[0]=="I1") {
						ATests.Merge(checkResult,checkResultPrep);
					}
					Results.Write(checkResult);
					listResults.Add(checkResult);
				}
			}
			finally {
				inProcHostI.Dispose();
			}
			WinChkFixture.Record("I9","WPF developer switch, the other callers",CheckStatus.Skipped,"Not run: TC-8 removed the developer switch "
				+"(OPMS_TOOTHCHART_WPF); the WPF chart is every computer's 3D chart, which I1 to I8 check.");
			//I8: the WPF chart, out of process (its memory) and in process (Show Big and the resize).
			CheckResult checkResultI8=new CheckResult("I8","The WPF chart at 100%");
			if(WinChkFixture.IsBudgetSpent) {
				checkResultI8.Fail("Not run: "+WinChkFixture.BudgetSpentText);
			}
			else {
				//Since TC-8 the WPF chart is every computer's 3D chart: no developer switch (OPMS_TOOTHCHART_WPF) is needed or read.
				using(ProgramProcess programProcessWpf=ProgramProcess.Start(ATests.DebugExe,ProgramProcess.ProgramArgs("chart")))
				{
					IntPtr hwnd=programProcessWpf.WaitForWindow((h,className,title) => className!="#32770" && title.Contains("Gatetest"),
						TimeSpan.FromSeconds(WinChkFixture.CapTimeout(180)));
					if(checkResultI8.Expect(hwnd!=IntPtr.Zero,"The program starts (out of process)")) {
						System.Threading.Thread.Sleep(4000);
						checkResultI8.AddShot(Shot.Window(hwnd,"I8-wpf-chart.png"),"The Chart with the WPF chart");
						programProcessWpf.Process.Refresh();
						checkResultI8.Note("Memory with the Chart open: working set "+programProcessWpf.Process.WorkingSet64/1048576+" MB, private "
							+programProcessWpf.Process.PrivateMemorySize64/1048576+" MB");
						Exits.CloseProgram(programProcessWpf,hwnd,"I8",checkResultI8);
					}
				}
				using(InProcHost inProcHost=InProcHost.Start("chart")) {
					ATests.Merge(checkResultI8,inProcHost.Run("I8BIG","The WPF chart: Show Big and resize"));
				}
				checkResultI8.ForPerson("the memory after Show Big and after closing it (in process the memory is the test host's)");
			}
			Results.Write(checkResultI8);
			//I10: the program started again (a new session): the sheet editor's preview, undoing I5 and I6 (only the rows they made: their keys).
			using(InProcHost inProcHost=InProcHost.Start("chart")) {
				CheckResult checkResultI10=inProcHost.Run("I10","Sheet editor preview",timeoutSeconds:300);
				Results.Write(checkResultI10);
				listResults.Add(checkResultI10);
			}
			//I11: start-up with no DirectX format saved. Until TC-8 the start-up searched a DirectX format (minutes on some PCs) and saved it; since TC-8
			//a stored DirectX choice starts the WPF chart, which needs none, and TC-9a removed the search: the start saves nothing.
			CheckResult checkResultI11=new CheckResult("I11","Start-up with no DirectX format saved");
			//The program's own computer name (ODEnvironment.MachineName: the client's name in a remote session), whose computerpref row it reads;
			//escaped as in PZTests.
			string computerName=CodeBase.ODEnvironment.MachineName.Replace("'","''");
			if(WinChkFixture.IsBudgetSpent) {
				checkResultI11.Fail("Not run: "+WinChkFixture.BudgetSpentText);
			}
			else {
				string formatBefore=ScratchDb.Scalar("SELECT DirectXFormat FROM computerpref WHERE ComputerName='"+computerName+"'");
				string graphicsSimpleBefore=ScratchDb.Scalar("SELECT GraphicsSimple FROM computerpref WHERE ComputerName='"+computerName+"'");
				ScratchDb.Exec("UPDATE computerpref SET DirectXFormat='' WHERE ComputerName='"+computerName+"'");
				Stopwatch stopwatch=Stopwatch.StartNew();
				IntPtr hwndI11=ATests.StartProgram(ATests.DebugExe,"chart",checkResultI11,out ProgramProcess programProcessI11,null);
				using(programProcessI11) {
					if(hwndI11!=IntPtr.Zero) {
						System.Threading.Thread.Sleep(5000);
						checkResultI11.Note("Start to the main window with the Chart: "+stopwatch.Elapsed.TotalSeconds.ToString("0.0")+" s");
						checkResultI11.AddShot(Shot.Window(hwndI11,"I11.png"),"The Chart after the start");
						Exits.CloseProgram(programProcessI11,hwndI11,"I11",checkResultI11);
					}
				}
				string formatAfter=ScratchDb.Scalar("SELECT DirectXFormat FROM computerpref WHERE ComputerName='"+computerName+"'");
				checkResultI11.Expect(formatAfter=="","The start-up searched and saved no DirectXFormat (it is still empty: '"+formatAfter+"')",formatAfter);
				string graphicsSimple=ScratchDb.Scalar("SELECT GraphicsSimple FROM computerpref WHERE ComputerName='"+computerName+"'");
				checkResultI11.Expect(graphicsSimple==graphicsSimpleBefore,"The start-up kept the stored choice (GraphicsSimple "+graphicsSimpleBefore+" before, "
					+graphicsSimple+" after)",graphicsSimple);
				checkResultI11.Note("Before: "+formatBefore+(formatAfter==formatBefore ? " (the same)" : "; restored"));
				if(formatAfter!=formatBefore) {
					ScratchDb.Exec("UPDATE computerpref SET DirectXFormat='"+formatBefore.Replace("'","''")+"' WHERE ComputerName='"+computerName+"'");
				}
			}
			using(InProcHost inProcHost=InProcHost.Start("chart")) {
				//B5's on-screen test in this session: when the chart's own window prints drawn, the screen shows it, and so does I11.png.
				CheckResult checkResultScreen=inProcHost.Run("ISCREEN","The WPF chart printed from its window (B5's on-screen test)");
				if(checkResultScreen.DictKeys.TryGetValue("ISCREEN.IsDrawn",out long isDrawn) && isDrawn==1) {
					checkResultI11.Verified("The WPF chart's own window prints drawn in this session (B5's on-screen test, I11-screen.png): the screen is not "
						+"locked, so I11.png shows the chart as the screen does");
				}
				else {
					checkResultI11.ForPerson("that the Chart shows the 3D chart on screen (I11.png; the chart's own window printed blank in this session, "
						+"I11-screen.png, as the screen's WPF content does while the session is locked)");
				}
				ATests.Merge(checkResultI11,checkResultScreen);
				ATests.Merge(checkResultI11,inProcHost.Run("IUNDO","Undo of the prep"));
			}
			Results.Write(checkResultI11);
			listResults.Add(checkResultI8);
			listResults.Add(checkResultI11);
			Assert.AreEqual("",string.Join(" || ",listResults.Where(x => x.Status==CheckStatus.Failed).Select(x => x.Id+": "+x.Summary)));
		}

		[TestMethod]
		public void J1toJ17_Ehr() {
			WinChkFixture.Begin("J");
			List<CheckResult> listResults=new List<CheckResult>();
			CheckResult checkResultJ1;
			using(InProcHost inProcHost=InProcHost.Start("chart")) {
				checkResultJ1=inProcHost.Run("J1","Show Features: EHR on");
			}
			//Exit and start with EHR on: J1's Chart, J2's wizard, J3's SNOMED window and data row, the 0.4 users, J4 to J13 with their log-offs, J15.
			using(InProcHost inProcHost=InProcHost.Start("chart")) {
				CheckResult checkResultChart=inProcHost.Run("JCHARTJ1","Chart after the start");
				checkResultJ1.Expect(checkResultChart.ListVerified.Contains("EHR button present"),"Exit, start, log on: the Chart toolbar has EHR");
				ATests.Merge(checkResultJ1,checkResultChart);
				Results.Write(checkResultJ1);
				listResults.Add(checkResultJ1);
				foreach(string[] arrayCheck in new[] { new[] { "J2","Setup Wizard, unsaved" },new[] { "J3","SNOMED tools hidden without rows" } }) {
					CheckResult checkResult=inProcHost.Run(arrayCheck[0],arrayCheck[1]);
					Results.Write(checkResult);
					listResults.Add(checkResult);
				}
				CheckResult checkResultUsers=inProcHost.Run("JUSERS","0.4 test users");
				Results.Log("JUSERS: "+checkResultUsers.Status+": "+checkResultUsers.Summary);
				foreach(string[] arrayCheck in new[] { new[] { "J4","EHR window, no row" },new[] { "J5","EHR setup without Setup" },
					new[] { "J6","SNOMED tools hidden without Setup" },new[] { "J7","Add a provider row" },new[] { "J8","A name no provider has" },
					new[] { "J9","SNOMED tools with Setup; map refused" },new[] { "J10","EHR window with a row" },new[] { "J11","CPOE" },
					new[] { "J12","Change and delete the row; the log" },new[] { "J13","Map to ICD9 with Problem Definition Edit" } })
				{
					CheckResult checkResult;
					if(checkResultUsers.Status==CheckStatus.Failed) {
						checkResult=new CheckResult(arrayCheck[0],arrayCheck[1]);
						checkResult.Fail("The 0.4 test users were not made: "+checkResultUsers.Summary);
					}
					else {
						checkResult=inProcHost.Run(arrayCheck[0],arrayCheck[1]);
						if(arrayCheck[0]=="J4") {
							ATests.Merge(checkResult,checkResultUsers);
						}
					}
					Results.Write(checkResult);
					listResults.Add(checkResult);
				}
				CheckResult checkResultJ15=inProcHost.Run("J15","Direct messaging options");
				Results.Write(checkResultJ15);
				listResults.Add(checkResultJ15);
				//The harness's own check (not a checklist row): a failed log-on goes back to Admin instead of cancelling the log-on window.
				CheckResult checkResultLogOnFail=inProcHost.Run("JLOGONFAIL","A failed log-on goes back to Admin");
				Results.Log("JLOGONFAIL: "+checkResultLogOnFail.Status+": "+checkResultLogOnFail.Summary);
				listResults.Add(checkResultLogOnFail);
			}
			//J14: the time check is off, after the log-ons above with EHR on.
			CheckResult checkResultJ14=new CheckResult("J14","Time check off");
			string folderDocuments=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Logger","TimeSync");
			string folderDebug=Path.Combine(WinChkEnv.DebugFolder,"Logger","TimeSync");
			checkResultJ14.Expect(!Directory.Exists(folderDocuments),"No Documents\\Logger\\TimeSync after the log-ons with EHR on (Admin, "
				+InProc.InProcChecks.USER_SETUP+", "+InProc.InProcChecks.USER_NOSETUP+")");
			checkResultJ14.Expect(!Directory.Exists(folderDebug),"No <dbg>\\Logger\\TimeSync");
			string timeSync=ScratchDb.Scalar("SELECT ValueString FROM preference WHERE PrefName='ForkTimeSyncMode'");
			checkResultJ14.Expect(timeSync=="" || timeSync=="0","ForkTimeSyncMode: "+(timeSync=="" ? "no row" : timeSync));
			checkResultJ14.Note("Time Synchronization is never opened; text instead of a picture");
			Results.Write(checkResultJ14);
			listResults.Add(checkResultJ14);
			//J16: EHR off; exit, start: no EHR button.
			CheckResult checkResultJ16;
			using(InProcHost inProcHost=InProcHost.Start("chart")) {
				checkResultJ16=inProcHost.Run("J16","EHR off");
			}
			CheckResult checkResultJ17;
			using(InProcHost inProcHost=InProcHost.Start("chart")) {
				CheckResult checkResultChart=inProcHost.Run("JCHARTJ16","Chart after the start");
				checkResultJ16.Expect(checkResultChart.ListVerified.Contains("EHR button absent"),"Exit, start, log on: no EHR button in the Chart");
				ATests.Merge(checkResultJ16,checkResultChart);
				ATests.Merge(checkResultJ16,inProcHost.Run("J16SF","Show Features after the restart"));
				//J17: the Setup Wizard saves EHR on Next.
				checkResultJ17=inProcHost.Run("J17","Setup Wizard saves EHR");
			}
			Results.Write(checkResultJ16);
			listResults.Add(checkResultJ16);
			using(InProcHost inProcHost=InProcHost.Start("chart")) {
				CheckResult checkResultChart=inProcHost.Run("JCHARTJ17ON","Chart after the start");
				checkResultJ17.Expect(checkResultChart.ListVerified.Contains("EHR button present"),"Exit, start, log on: the Chart toolbar has EHR");
				ATests.Merge(checkResultJ17,checkResultChart);
				ATests.Merge(checkResultJ17,inProcHost.Run("J17OFF","Show Features: EHR off again"));
			}
			using(InProcHost inProcHost=InProcHost.Start("chart")) {
				CheckResult checkResultChart=inProcHost.Run("JCHARTJ17OFF","Chart after the start");
				checkResultJ17.Expect(checkResultChart.ListVerified.Contains("EHR button absent"),"Exit, start, log on: no EHR button");
				ATests.Merge(checkResultJ17,checkResultChart);
			}
			Results.Write(checkResultJ17);
			listResults.Add(checkResultJ17);
			//0.4's removal: the copy's test users and groups go (the program cannot delete users; in the scratch copy the rows are deleted).
			RemoveTestUsers();
			Assert.AreEqual("",string.Join(" || ",listResults.Where(x => x.Status==CheckStatus.Failed).Select(x => x.Id+": "+x.Summary)));
		}

		///<summary>0.4's removal in the scratch copy: the two test users, their group attaches, the two groups and their permissions. Logged.</summary>
		public static void RemoveTestUsers() {
			string userNums=ScratchDb.Scalar("SELECT GROUP_CONCAT(UserNum) FROM userod WHERE UserName IN ('"+InProc.InProcChecks.USER_SETUP+"','"
				+InProc.InProcChecks.USER_NOSETUP+"')");
			string groupNums=ScratchDb.Scalar("SELECT GROUP_CONCAT(UserGroupNum) FROM usergroup WHERE Description IN ('"+InProc.InProcChecks.GROUP_SETUP+"','"
				+InProc.InProcChecks.GROUP_NOSETUP+"')");
			if(userNums!="") {
				ScratchDb.Exec("DELETE FROM usergroupattach WHERE UserNum IN ("+userNums+")");
				ScratchDb.Exec("DELETE FROM userod WHERE UserNum IN ("+userNums+")");
			}
			if(groupNums!="") {
				ScratchDb.Exec("DELETE FROM grouppermission WHERE UserGroupNum IN ("+groupNums+")");
				ScratchDb.Exec("DELETE FROM usergroup WHERE UserGroupNum IN ("+groupNums+")");
			}
			Results.Log("0.4 removal: users "+(userNums=="" ? "none" : userNums)+", groups "+(groupNums=="" ? "none" : groupNums)+" deleted from the scratch copy");
		}
	}
}
