// Groups F, G and H of the checklist: the dashboard and its graph cells' menus (in process), the reports (in process; G2's RDL file is copied into the scratch copy's A to Z
// folder before the program starts, as the Custom Reports menu is built at start-up, and deleted afterwards), and RdlDesigner (out of process,
// its main menu, a MenuStrip since S3a-M3a, chosen through UI Automation by the items' texts).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowChecksTests {
	[TestClass]
	public class FGHTests {
		///<summary>RdlDesigner's designerstate.xml as H1 found it (null: there was none, or H did not run): Z2 checks it is again exactly that.</summary>
		public static byte[] DesignerStateBefore;
		public static bool HasRunH;

		[TestMethod]
		public void F1toF4_Dashboard() {
			WinChkFixture.Begin("F");
			//F2 and F4 click the graph cell's context menu items by their PerformClick (the right-click menu itself is not shown); F3 runs Setup through
			//its menu item and the tab name's double-click through the tab control's handler.
			List<CheckResult> listCheckResults=new List<CheckResult>();
			using(InProcHost inProcHost=InProcHost.Start("chart")) {
				foreach(string[] arrayCheck in new[] { new[] { "F1","Dashboard: open" },new[] { "F2","Dashboard: edit a cell" },new[] { "F3","Dashboard: edit a tab" },
					new[] { "F4","Dashboard: print preview" } })
				{
					CheckResult checkResult=inProcHost.Run(arrayCheck[0],arrayCheck[1]);
					Results.Write(checkResult);
					listCheckResults.Add(checkResult);
				}
			}
			Assert.AreEqual("",string.Join(" || ",listCheckResults.Where(x => x.Status==CheckStatus.Failed).Select(x => x.Id+": "+x.Summary)));
		}

		///<summary>Stage 3b (WINCHK-5): the dashboard's graphs drawn on their own (OpenPmsDentalGraph on .NET 10's Windows Forms charting).</summary>
		[TestMethod]
		public void F5_DashboardGraphs() {
			WinChkFixture.Begin("F");
			using InProcHost inProcHost=InProcHost.Start("chart");
			WinChkFixture.Finish(inProcHost.Run("F5","Dashboard: the graphs drawn on their own"));
		}

		///<summary>Stage 3b (WINCHK-5): a classic print preview (FormPrintPreview) of the time cards, which only previews.</summary>
		[TestMethod]
		public void G3_PrintPreview() {
			WinChkFixture.Begin("G");
			using InProcHost inProcHost=InProcHost.Start("chart");
			WinChkFixture.Finish(inProcHost.Run("G3","Print preview: the time cards (FormPrintPreview)"));
		}

		[TestMethod]
		public void G1_FormReportComplex() {
			WinChkFixture.Begin("G");
			using InProcHost inProcHost=InProcHost.Start("chart");
			WinChkFixture.Finish(inProcHost.Run("G1","FormReportComplex (Appointments report)"));
		}

		[TestMethod]
		public void G2_RdlReport() {
			WinChkFixture.Begin("G");
			string reportFolderName=ScratchDb.Scalar("SELECT ValueString FROM preference WHERE PrefName='ReportFolderName'");
			string folder=Path.Combine(ScratchDb.AtoZFolder,reportFolderName);
			bool isFolderMade=!Directory.Exists(folder);
			Directory.CreateDirectory(folder);
			string path=Path.Combine(folder,"Unsched Treat Plans.rdl");
			File.Copy(Path.Combine(WinChkEnv.RepoRoot,"OpenPmsDental","Resources","Unsched Treat Plans.rdl"),path);
			CheckResult checkResult;
			try {
				using InProcHost inProcHost=InProcHost.Start("chart");
				checkResult=inProcHost.Run("G2","RDL report");
			}
			finally {
				File.Delete(path);
				if(isFolderMade) {
					Directory.Delete(folder);
				}
			}
			checkResult.Note("The .rdl was copied into the scratch copy's A to Z folder's "+reportFolderName+" folder before the start and deleted afterwards"
				+(isFolderMade ? " (with the folder this check made)" : ""));
			checkResult.Expect(!File.Exists(path),"Undo: the copied file is deleted");
			WinChkFixture.Finish(checkResult);
		}

		///<summary>Stage 3b (WINCHK-5's fix round): a report with VB Code functions (G4) and one whose Code does not compile, with the RDL viewer's
		///dialogs (G5), from Reports > Custom Reports. As G2: the two .rdl files are written into the scratch copy's A to Z report folder before the
		///program starts (the menu is built at start-up) and deleted afterwards.</summary>
		[TestMethod]
		public void G4G5_RdlCodeReports() {
			WinChkFixture.Begin("G");
			string reportFolderName=ScratchDb.Scalar("SELECT ValueString FROM preference WHERE PrefName='ReportFolderName'");
			string folder=Path.Combine(ScratchDb.AtoZFolder,reportFolderName);
			bool isFolderMade=!Directory.Exists(folder);
			Directory.CreateDirectory(folder);
			string pathCode=Path.Combine(folder,RdlCodeReports.NAME_CODE+".rdl");
			string pathCodeError=Path.Combine(folder,RdlCodeReports.NAME_CODE_ERROR+".rdl");
			File.WriteAllText(pathCode,RdlCodeReports.ReportCode());
			File.WriteAllText(pathCodeError,RdlCodeReports.ReportCodeError());
			List<CheckResult> listCheckResults=new List<CheckResult>();
			try {
				using InProcHost inProcHost=InProcHost.Start("chart");
				listCheckResults.Add(inProcHost.Run("G4","RDL report with VB Code functions"));
				listCheckResults.Add(inProcHost.Run("G5","RDL report whose Code does not compile; the RDL viewer's dialogs"));
			}
			finally {
				File.Delete(pathCode);
				File.Delete(pathCodeError);
				if(isFolderMade) {
					Directory.Delete(folder);
				}
			}
			foreach(CheckResult checkResult in listCheckResults) {
				checkResult.Note("The .rdl files were written into the scratch copy's A to Z folder's "+reportFolderName+" folder before the start and deleted "
					+"afterwards"+(isFolderMade ? " (with the folder this test made)" : ""));
				checkResult.Expect(!File.Exists(pathCode) && !File.Exists(pathCodeError),"Undo: the written .rdl files are deleted");
				Results.Write(checkResult);
			}
			Assert.AreEqual("",string.Join(" || ",listCheckResults.Where(x => x.Status==CheckStatus.Failed).Select(x => x.Id+": "+x.Summary)));
		}

		[TestMethod]
		public void H1H2_RdlDesigner() {
			WinChkFixture.Begin("H");
			CheckResult checkResultH1=new CheckResult("H1","RdlDesigner: open and edit");
			CheckResult checkResultH2=new CheckResult("H2","RdlDesigner: preview");
			string folderTemp=Path.Combine(Path.GetTempPath(),"stage1-rdl");
			Directory.CreateDirectory(folderTemp);
			string pathCopy=Path.Combine(folderTemp,"Unsched Treat Plans.rdl");
			File.Copy(Path.Combine(WinChkEnv.RepoRoot,"OpenPmsDental","Resources","Unsched Treat Plans.rdl"),pathCopy,true);
			string folderDesigner=Path.Combine(WinChkEnv.RepoRoot,"RdlDesign","bin","Debug");
			string pathState=Path.Combine(folderDesigner,"designerstate.xml");
			bool hadState=File.Exists(pathState);
			//A designerstate.xml that was there is copied aside and copied back afterwards, byte for byte (its encoding and BOM unchanged); the backup is
			//in the results folder, outside the worktree, and deleted once the restore is checked.
			string pathStateBackup=Path.Combine(WinChkEnv.OutFolder,"H-designerstate-before.xml");
			if(hadState) {
				File.Copy(pathState,pathStateBackup,true);
				DesignerStateBefore=File.ReadAllBytes(pathState);
			}
			HasRunH=true;
			try {
				//RdlDesigner opens the files named on its command line only while it restores its designerstate.xml (RdlDesigner.GetStartupState), so an
				//empty state is written beside it first (deleted again below; Z2 checks it is gone). It starts from its own folder.
				if(!hadState) {
					File.WriteAllText(pathState,"<?xml version='1.0' encoding='UTF-8'?><designerstate><CurrentFiles /><RecentFilesMax>5</RecentFilesMax>"
						+"<RecentFiles /></designerstate>");
				}
				using ProgramProcess programProcess=ProgramProcess.Start(Path.Combine(folderDesigner,"RdlDesigner.exe"),"\""+pathCopy+"\"");
				IntPtr hwnd=programProcess.WaitForWindow((h,className,title) => className!="#32770" && title!="",TimeSpan.FromSeconds(60));
				if(checkResultH1.Expect(hwnd!=IntPtr.Zero,"RdlDesigner opens ("+Native.GetText(hwnd)+")")) {
					Thread.Sleep(4000);
					checkResultH1.Expect(programProcess.ListDialogsSeen.Count==0,"No message box at the start",string.Join(" || ",programProcess.ListDialogsSeen));
					//The copy's MDI child (its title names the file) with the layout.
					List<string> listChildTexts=Native.GetChildren(hwnd).Select(x => Native.GetText(x)).Where(x => x!="").ToList();
					checkResultH1.Expect(listChildTexts.Any(x => x.Contains("Unsched Treat Plans.rdl")) || Native.GetText(hwnd).Contains("Unsched Treat Plans.rdl"),
						"The copy is open in the designer (its window names Unsched Treat Plans.rdl)",Native.GetText(hwnd)+"; "+string.Join(", ",listChildTexts.Take(12)));
					checkResultH1.AddShot(Shot.Window(hwnd,"H1-designer.png"),"RdlDesigner with the copy open (Designer view)");
					//View > RDL Text, through the MenuStrip by UI Automation (Uia.ChooseMenuStripItem); the XML shows in the editor.
					if(ChooseMenu(programProcess,hwnd,new[] { "View","RDL Text" },checkResultH1)) {
						Thread.Sleep(2500);
						string textXml=Native.GetChildren(hwnd).Select(x => Native.GetText(x)).FirstOrDefault(x => x.Contains("<Report"))??"";
						checkResultH1.Expect(textXml!="","View > RDL Text shows the XML ("+textXml.Length+" characters, '<Report' found in the editor)");
						checkResultH1.AddShot(Shot.Window(hwnd,"H1-rdltext.png"),"RdlDesigner, View > RDL Text");
						ChooseMenu(programProcess,hwnd,new[] { "View","Designer" },checkResultH1);
						Thread.Sleep(1500);
					}
					checkResultH1.ForPerson("adding a Text box to the page header (the toolbar's Text, then a click on the design surface) and File > Save are mouse work "
						+"on RdlDesigner's design surface: an on-screen pass");
					//H2: View > Preview (F5), through the MenuStrip by UI Automation.
					if(ChooseMenu(programProcess,hwnd,new[] { "View","Preview" },checkResultH2)) {
						Thread.Sleep(6000);
						checkResultH2.Expect(!programProcess.HasExited,"RdlDesigner is still running after View > Preview (no crash)");
						List<IntPtr> listDialogs=programProcess.GetWindows().Where(x => x!=hwnd).ToList();
						List<string> listShown=new List<string>();
						foreach(IntPtr hwndDialog in listDialogs) {
							string text=Native.IsDialogClass(hwndDialog) ? Native.GetDialogMessage(hwndDialog)
								: string.Join(" ",Native.GetChildren(hwndDialog).Select(x => Native.GetText(x)).Where(x => x!=""));
							listShown.Add("\""+Native.GetText(hwndDialog)+"\" ("+Native.GetClass(hwndDialog)+"): "+text.Replace("\r"," ").Replace("\n"," "));
						}
						string textPreview=string.Join(" ",Native.GetChildren(hwnd).Select(x => Native.GetText(x)).Where(x => x!="" && !x.Contains("<Report")));
						checkResultH2.Note("Windows besides the designer after Preview: "+(listShown.Count==0 ? "none" : string.Join(" || ",listShown)));
						checkResultH2.Note("Texts in the designer's window after Preview: "+(textPreview.Length>600 ? textPreview.Substring(0,600)+"..." : textPreview));
						string all=string.Join(" ",listShown)+" "+textPreview;
						checkResultH2.Expect(!all.Contains("Unhandled exception") && !listDialogs.Any(x => Native.GetText(x).Contains("Unhandled")),
							"No unhandled exception dialog",all);
						checkResultH2.AddShot(Shot.Window(hwnd,"H2-preview.png"),"RdlDesigner, View > Preview");
						if(listDialogs.Count>0) {
							checkResultH2.AddShot(Shot.Window(listDialogs[0],"H2-message.png"),"What the preview showed");
						}
						bool isExpectedError=all.Contains("MySQL") || all.Contains("ODR") || all.Contains("data provider") || all.Contains("DataProvider");
						if(isExpectedError) {
							checkResultH2.Verified("The preview reports the expected missing data provider (MySQL.NET or the ODR connection): its folder has no "
								+"RdlEngineConfig.xml, ODR.dll or MySqlConnector, which the checklist expects and does not count as a failure");
						}
						else {
							checkResultH2.ForPerson("read the preview's messages in H2-preview.png: RdlViewer draws them itself, so no window text holds them (the "
								+"checklist expects an error naming the data provider MySQL.NET or the ODR connection, not a failure)");
						}
						foreach(IntPtr hwndDialog in listDialogs.Where(x => Native.IsDialogClass(x))) {
							if(!Native.ClickDialogButton(hwndDialog,"OK")) {
								Native.CancelDialog(hwndDialog);
							}
						}
						Thread.Sleep(1000);
					}
					double seconds=programProcess.CloseAndTimeExit(hwnd,TimeSpan.FromSeconds(30));
					if(seconds<0) {
						//It may ask to save: answer No, never Save.
						IntPtr hwndAsk=programProcess.GetWindows().FirstOrDefault(x => Native.IsDialogClass(x));
						if(hwndAsk!=IntPtr.Zero) {
							checkResultH1.Note("On close: "+Native.GetDialogMessage(hwndAsk));
							Native.ClickDialogButton(hwndAsk,"No");
						}
						if(!programProcess.Process.WaitForExit(15000)) {
							programProcess.Kill();
							checkResultH1.Fail("RdlDesigner did not exit after WM_CLOSE");
						}
					}
					checkResultH1.Note("Closed in "+seconds.ToString("0.0")+" s; message boxes seen: "+(programProcess.ListDialogsSeen.Count==0 ? "none"
						: string.Join(" || ",programProcess.ListDialogsSeen)));
				}
			}
			finally {
				if(hadState) {
					File.Copy(pathStateBackup,pathState,true);
					//Against the bytes read before the start, not the backup the file was just copied from (that comparison could not fail).
					bool isSame=File.ReadAllBytes(pathState).SequenceEqual(DesignerStateBefore);
					checkResultH1.Expect(isSame,"designerstate.xml is restored byte for byte (copied aside before the start and back after)");
					if(isSame) {
						File.Delete(pathStateBackup);
					}
					else {
						checkResultH1.Note("The copy taken before the start is kept as "+Path.GetFileName(pathStateBackup)+" in the results folder");
					}
				}
				else if(File.Exists(pathState)) {
					File.Delete(pathState);
				}
				checkResultH1.Note("designerstate.xml: "+(hadState ? "it was there: restored from its copy" : "written for the start, deleted again"));
				Directory.Delete(folderTemp,true);
			}
			checkResultH2.Expect(!Directory.Exists(folderTemp),"The temp folder stage1-rdl is deleted");
			Results.Write(checkResultH1);
			Results.Write(checkResultH2);
			Assert.AreEqual("",string.Join(" || ",new[] { checkResultH1,checkResultH2 }.Where(x => x.Status==CheckStatus.Failed).Select(x => x.Id+": "+x.Summary)));
		}

		///<summary>Stage 3b (WINCHK-5's fix round): RdlDesigner opens the report with VB Code functions (G4's), its Report Properties' Code tab
		///shows the block, and Check Syntax compiles it (CodeCtl.CheckCode: RdlEngine's VbCodeCompiler, Roslyn on .NET 10) and says "No errors".
		///Out of process: the menu, the tab and the buttons through UI Automation patterns, the message box by window messages; no input is sent.
		///Report Properties is closed with Cancel and RdlDesigner without saving. designerstate.xml is handled as H1 handles it.</summary>
		[TestMethod]
		public void H3_RdlDesignerCode() {
			WinChkFixture.Begin("H");
			CheckResult checkResult=new CheckResult("H3","RdlDesigner: the Code tab and Check Syntax");
			string folderTemp=Path.Combine(Path.GetTempPath(),"stage1-rdl-code");
			Directory.CreateDirectory(folderTemp);
			string pathCopy=Path.Combine(folderTemp,RdlCodeReports.NAME_CODE+".rdl");
			File.WriteAllText(pathCopy,RdlCodeReports.ReportCode());
			string folderDesigner=Path.Combine(WinChkEnv.RepoRoot,"RdlDesign","bin","Debug");
			string pathState=Path.Combine(folderDesigner,"designerstate.xml");
			byte[] bytesStateBefore=File.Exists(pathState) ? File.ReadAllBytes(pathState) : null;
			if(bytesStateBefore!=null && DesignerStateBefore==null) {
				DesignerStateBefore=bytesStateBefore;//Z2 checks it is there again, byte for byte
			}
			HasRunH=true;
			try {
				if(bytesStateBefore==null) {
					File.WriteAllText(pathState,"<?xml version='1.0' encoding='UTF-8'?><designerstate><CurrentFiles /><RecentFilesMax>5</RecentFilesMax>"
						+"<RecentFiles /></designerstate>");
				}
				using ProgramProcess programProcess=ProgramProcess.Start(Path.Combine(folderDesigner,"RdlDesigner.exe"),"\""+pathCopy+"\"");
				IntPtr hwnd=programProcess.WaitForWindow((h,className,title) => className!="#32770" && title!="",TimeSpan.FromSeconds(60));
				if(checkResult.Expect(hwnd!=IntPtr.Zero,"RdlDesigner opens ("+Native.GetText(hwnd)+")")) {
					Thread.Sleep(4000);
					checkResult.Expect(programProcess.ListDialogsSeen.Count==0,"No message box at the start",string.Join(" || ",programProcess.ListDialogsSeen));
					List<string> listChildTexts=Native.GetChildren(hwnd).Select(x => Native.GetText(x)).Where(x => x!="").ToList();
					checkResult.Expect(listChildTexts.Any(x => x.Contains(RdlCodeReports.NAME_CODE)) || Native.GetText(hwnd).Contains(RdlCodeReports.NAME_CODE),
						"The report is open in the designer (its window names "+RdlCodeReports.NAME_CODE+".rdl)",Native.GetText(hwnd)+"; "+string.Join(", ",listChildTexts.Take(12)));
					checkResult.AddShot(Shot.Window(hwnd,"H3-designer.png"),"RdlDesigner with the report with VB Code functions");
					//View > Properties... with nothing selected opens the Report Properties sheets (DesignCtl.menuProperties_Click); its handler shows them
					//modally, so the menu's Invoke returns once they close and ChooseMenu does not wait for it.
					if(ChooseMenu(programProcess,hwnd,new[] { "View","Properties..." },checkResult)) {
						IntPtr hwndProperties=programProcess.WaitForWindow((h,className,title) => title=="Report Properties",TimeSpan.FromSeconds(20));
						if(checkResult.Expect(hwndProperties!=IntPtr.Zero,"Report Properties opens")) {
							Thread.Sleep(1000);
							checkResult.AddShot(Shot.Window(hwndProperties,"H3-properties.png"),"Report Properties (its Report tab)");
							CheckCodeTab(programProcess,hwndProperties,checkResult);
							//Cancel: nothing is applied.
							Uia.Try(() => Uia.Invoke(System.Windows.Automation.AutomationElement.FromHandle(hwndProperties),System.Windows.Automation.ControlType.Button,
								"Cancel"),10000,out string errorCancel);
							Stopwatch stopwatch=Stopwatch.StartNew();
							while(Native.IsWindow(hwndProperties) && Native.IsWindowVisible(hwndProperties) && stopwatch.ElapsedMilliseconds<10000) {
								Thread.Sleep(200);
							}
							if(Native.IsWindow(hwndProperties) && Native.IsWindowVisible(hwndProperties)) {
								Native.PostMessage(hwndProperties,Native.WM_CLOSE,IntPtr.Zero,IntPtr.Zero);
								Thread.Sleep(1500);
							}
							checkResult.Expect(!Native.IsWindow(hwndProperties) || !Native.IsWindowVisible(hwndProperties),"Report Properties closes (Cancel)",errorCancel);
						}
					}
					double seconds=programProcess.CloseAndTimeExit(hwnd,TimeSpan.FromSeconds(30));
					if(seconds<0) {
						//It may ask to save: answer No, never Save.
						IntPtr hwndAsk=programProcess.GetWindows().FirstOrDefault(x => Native.IsDialogClass(x));
						if(hwndAsk!=IntPtr.Zero) {
							checkResult.Note("On close: "+Native.GetDialogMessage(hwndAsk));
							Native.ClickDialogButton(hwndAsk,"No");
						}
						if(!programProcess.Process.WaitForExit(15000)) {
							programProcess.Kill();
							checkResult.Fail("RdlDesigner did not exit after WM_CLOSE");
						}
					}
					checkResult.Note("Closed in "+seconds.ToString("0.0")+" s; message boxes seen: "+(programProcess.ListDialogsSeen.Count==0 ? "none"
						: string.Join(" || ",programProcess.ListDialogsSeen)));
				}
				checkResult.Expect(File.ReadAllText(pathCopy)==RdlCodeReports.ReportCode(),"The report file is unchanged (nothing saved)");
			}
			finally {
				if(bytesStateBefore!=null) {
					File.WriteAllBytes(pathState,bytesStateBefore);
					checkResult.Expect(File.ReadAllBytes(pathState).SequenceEqual(bytesStateBefore),"designerstate.xml is restored byte for byte");
				}
				else if(File.Exists(pathState)) {
					File.Delete(pathState);
				}
				checkResult.Note("designerstate.xml: "+(bytesStateBefore!=null ? "it was there: written back" : "written for the start, deleted again"));
				Directory.Delete(folderTemp,true);
			}
			checkResult.Expect(!Directory.Exists(folderTemp),"The temp folder stage1-rdl-code is deleted");
			WinChkFixture.Finish(checkResult);
		}

		///<summary>H3: selects Report Properties' Code tab (SelectionItemPattern), checks the block shows, prints it, presses Check Syntax
		///(InvokePattern; its "No errors" message box is modal, so the call is not waited for) and reads and closes the message.</summary>
		private static void CheckCodeTab(ProgramProcess programProcess,IntPtr hwndProperties,CheckResult checkResult) {
			bool isSelected=Uia.Try(() => {
				System.Windows.Automation.AutomationElement automationElementTab=Uia.Find(System.Windows.Automation.AutomationElement.FromHandle(hwndProperties),
					System.Windows.Automation.ControlType.TabItem,"Code");
				if(automationElementTab==null) {
					return false;
				}
				((System.Windows.Automation.SelectionItemPattern)automationElementTab.GetCurrentPattern(System.Windows.Automation.SelectionItemPattern.Pattern)).Select();
				return true;
			},15000,out string errorTab);
			if(!checkResult.Expect(isSelected,"The Code tab is selected (UI Automation SelectionItemPattern)",errorTab)) {
				return;
			}
			Thread.Sleep(1500);
			string codeShown=Native.GetChildren(hwndProperties).Select(x => Native.GetText(x)).FirstOrDefault(x => x.Contains("Public Function"))??"";
			checkResult.Expect(NormalizeSpace(codeShown)==NormalizeSpace(RdlCodeReports.CODE_BLOCK),"The Code tab shows the report's Code block ("+codeShown.Length
				+" characters)",codeShown);
			checkResult.AddShot(Shot.Window(hwndProperties,"H3-code.png"),"Report Properties, the Code tab");
			List<IntPtr> listBefore=programProcess.GetWindows();
			Uia.Try(() => Uia.Invoke(System.Windows.Automation.AutomationElement.FromHandle(hwndProperties),System.Windows.Automation.ControlType.Button,"Check Syntax"),
				3000,out string errorInvoke);
			checkResult.Note("Check Syntax invoked"+(errorInvoke=="" ? "" : " ("+errorInvoke+": the message box it shows is modal)"));
			IntPtr hwndMessage=programProcess.WaitForWindow((h,className,title) => className=="#32770" && !listBefore.Contains(h),TimeSpan.FromSeconds(30));
			if(checkResult.Expect(hwndMessage!=IntPtr.Zero,"Check Syntax answers with a message box")) {
				string title=Native.GetText(hwndMessage);
				string message=Native.GetDialogMessage(hwndMessage);
				checkResult.Expect(title=="Code Verification" && message=="No errors","Check Syntax compiles the block: '"+title+"': "+message,title+": "+message);
				checkResult.AddShot(Shot.Window(hwndMessage,"H3-noerrors.png"),"Check Syntax's answer");
				Native.ClickDialogButton(hwndMessage,"OK");
				Stopwatch stopwatch=Stopwatch.StartNew();
				while(Native.IsWindow(hwndMessage) && stopwatch.ElapsedMilliseconds<10000) {
					Thread.Sleep(200);
				}
				checkResult.Expect(!Native.IsWindow(hwndMessage),"The message closes (OK)");
			}
		}

		///<summary>The text's words joined by single spaces (line ends and indents as an edit box shows them do not count).</summary>
		private static string NormalizeSpace(string text) {
			return string.Join(" ",(text??"").Split(new[] { ' ','\r','\n','\t' },StringSplitOptions.RemoveEmptyEntries));
		}

		///<summary>Chooses the menu item at the path of the window through its MenuStrip by UI Automation (RdlDesigner's main menu is a MenuStrip since
		///S3a-M3a: it has no native HMENU, so GetMenu and WM_COMMAND, which this check used with the MainMenu, find nothing): the menu is opened
		///(its DropDownOpening sets the items' Enabled states) and the item invoked; false (and a failure) when the item is absent, disabled or not
		///chosen.</summary>
		private static bool ChooseMenu(ProgramProcess programProcess,IntPtr hwnd,string[] arrayPath,CheckResult checkResult) {
			Uia.MenuChoice menuChoice=Uia.ChooseMenuStripItem(hwnd,programProcess.Process.Id,arrayPath);
			string path=string.Join(" > ",arrayPath);
			string evidence=menuChoice.Found+(menuChoice.Error=="" ? "" : "; UI Automation: "+menuChoice.Error);
			if(!checkResult.Expect(menuChoice.IsFound,path+" is in the menu bar (the MenuStrip's items: "+menuChoice.Found+")",evidence)) {
				return false;
			}
			if(!checkResult.Expect(!menuChoice.IsDisabled,path+" is enabled (after its menu opened)",evidence)) {
				return false;
			}
			return checkResult.Expect(menuChoice.IsInvoked,path+" chosen ("+menuChoice.Note+")",evidence);
		}
	}
}
