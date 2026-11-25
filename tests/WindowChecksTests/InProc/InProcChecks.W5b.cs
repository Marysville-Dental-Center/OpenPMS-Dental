// The stage 3b checks WINCHK-5's fix round added from section 6 of docs/specs/net10-plan.md ("Stage 3b (net10)"), each as a picture and
// without input: FormSnap (C21), MonthCalendarOD and DateTimePicker with its drop-down (C22, UI-CAL), a TreeView with check boxes (C23),
// RichTextBox text (C24), the capture notice and status windows of ImagingDeviceManager (C25), the EOB viewer with its hosted WPF strips
// (C26), the WPF Frm pop-ups (D6: the combo box picker, the month calendar, the tool tip), an RDL report with VB <Code> functions (G4) and one whose <Code> does not compile, with the RDL
// viewer's messages dialog (G5). Nothing is pressed that prints, saves, exports or sends; every row a check makes (C23's auto notes, C26's EOB)
// is made by the business layer and deleted again, and checked gone.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental;
using OpenPmsDentalBusiness;

namespace WindowChecksTests.InProc {
	public static partial class InProcChecks {
		public static void CheckC21(Ui ui,CheckResult checkResult) {
			//FormSnap (WpfControlsOD/Frame/FormSnap.cs) is the Windows 11 snap-layout window FormODBase and FormFrame would show when the pointer rests
			//on the maximize button. Both timerHoverSnap_Tick return before showing it ("incomplete, so this feature will be hidden for now"), as in
			//Open Dental, so the program never shows it. It is shown here as that code would show it (anchored below the main window's top right
			//corner) and printed: a plain form on the default font.
			checkResult.Note("The program does not show FormSnap: FormODBase.timerHoverSnap_Tick and FormFrame.timerHoverSnap_Tick return before it "
				+"(hidden, as in Open Dental); shown here as their code would show it");
			Ui.Opened opened=ui.Open("FormSnap, as timerHoverSnap_Tick would show it",() => {
				FormSnap formSnap=new FormSnap();
				formSnap.PointAnchor=new Point(ui.FormMain.Left+ui.FormMain.Width-70,ui.FormMain.Top+34);
				formSnap.Show();
			});
			if(!InspectWindow(ui,checkResult,opened,"FormSnap",null,close:false)) {
				return;
			}
			Form form=ui.GetForm(opened.Hwnd);
			ExpectDefaultFont(checkResult,form,"FormSnap");
			string size=ui.Invoke(() => form.Width+" x "+form.Height+" (ScaleMy "+((FormSnap)form).ScaleMy+")");
			//At ScaleMy 1: two screens of 120 x 80 with margins of 16 (FormSnap_Load).
			checkResult.Expect(ui.Invoke(() => form.Width==120*2+16*3 && form.Height==80+16*2),"FormSnap lays itself out at 288 x 112 ("+size+")",size);
			checkResult.AddShot(ui.Shot(opened.Hwnd,"C21.png"),"FormSnap (the four quarter and two half layouts)");
			checkResult.Expect(ui.Close(opened.Hwnd),"FormSnap closes");
		}

		///<summary>Opens a report of Reports > Standard (FormReportsMore) by its internal name, as a click on its row does (OpenReportLocalHelper),
		///checks it holds visible controls of the type, prints it, runs actionOpen on the open window (its handle; may be null) and closes it
		///(Cancel).</summary>
		private static void ShootStandardReport(Ui ui,CheckResult checkResult,IntPtr hwndReports,string internalName,string typeName,Type typeControl,
			string shot,Action<IntPtr> actionOpen=null)
		{
			Ui.Opened opened=ui.Open("Reports > Standard > "+internalName,() => {
				Form formReportsMore=(Form)Control.FromHandle(hwndReports);
				object displayReport=null;
				foreach(FieldInfo fieldInfo in formReportsMore.GetType().GetFields(BindingFlags.Instance|BindingFlags.NonPublic)
					.Where(x => x.Name.StartsWith("_listDisplayReports")))
				{
					displayReport=((IList)fieldInfo.GetValue(formReportsMore)).Cast<object>()
						.FirstOrDefault(x => (string)x.GetType().GetField("InternalName").GetValue(x)==internalName);
					if(displayReport!=null) {
						break;
					}
				}
				if(displayReport==null) {
					throw new Exception("No standard report "+internalName);
				}
				Ui.Call(formReportsMore,"OpenReportLocalHelper",displayReport,Ui.GetField(formReportsMore,"_listGroupPermissions"),true);
			});
			if(!InspectWindow(ui,checkResult,opened,typeName,null,close:false)) {
				return;
			}
			string controls=ui.Invoke(() => string.Join(", ",Ui.AllControls(Control.FromHandle(opened.Hwnd)).Where(x => typeControl.IsInstanceOfType(x) && x.Visible)
				.Select(x => x.Name+(x is DateTimePicker dateTimePicker ? " ("+dateTimePicker.Format+", "+dateTimePicker.Value.ToShortDateString()+")" : ""))));
			checkResult.Expect(controls!="",typeName+" shows its "+typeControl.Name+" controls: "+controls,controls);
			checkResult.AddShot(ui.Shot(opened.Hwnd,shot),typeName+" with its "+typeControl.Name+" controls");
			actionOpen?.Invoke(opened.Hwnd);
			checkResult.Expect(ui.Close(opened.Hwnd),typeName+" closes (Cancel; the report is not run)");
			opened.Posted?.Wait();
		}

		///<summary>UI-CAL: opens the drop-down of the window's first date picker with a calendar (not the up-down style) as its drop button opens
		///it (UiDateDropLook.OpenForTest), checks the drop-down is the program's MonthCalendarOD, prints it and closes it (as Esc closes it; the
		///picker's date is not changed).</summary>
		private static void ShootDateDropDown(Ui ui,CheckResult checkResult,IntPtr hwnd,string shot) {
			ToolStripDropDown toolStripDropDown=ui.Invoke(() => {
				DateTimePicker dateTimePicker=Ui.AllControls(Control.FromHandle(hwnd)).OfType<DateTimePicker>().FirstOrDefault(x => x.Visible && !x.ShowUpDown);
				if(dateTimePicker==null || !UiDateDropLook.OpenForTest(dateTimePicker)) {
					return null;
				}
				return UiDateDropLook.GetDropDownForTest(dateTimePicker);
			});
			checkResult.Expect(toolStripDropDown!=null,"A date picker's drop-down opens as the program's calendar (UiDateDropLook)");
			if(toolStripDropDown==null) {
				return;
			}
			string description=ui.Invoke(() => string.Join(", ",toolStripDropDown.Items.OfType<ToolStripControlHost>().Select(x => x.Control.GetType().Name))
				+(toolStripDropDown.Visible ? "" : " (not shown)"));
			checkResult.Expect(description=="MonthCalendarOD","The drop-down holds the program's MonthCalendarOD, shown: "+description,description);
			IntPtr hwndDropDown=ui.Invoke(() => toolStripDropDown.Visible ? toolStripDropDown.Handle : IntPtr.Zero);
			if(hwndDropDown!=IntPtr.Zero) {
				//Paint it before the shot: a shot taken right after it opened was blank once on a connected desktop (2026-11-24); the rerun was whole.
				ui.Invoke(() => toolStripDropDown.Refresh());
				checkResult.AddShot(ui.Shot(hwndDropDown,shot),"The date picker's drop-down (MonthCalendarOD)");
			}
			ui.Invoke(() => {
				if(!toolStripDropDown.IsDisposed) {
					toolStripDropDown.Close(ToolStripDropDownCloseReason.Keyboard);
				}
			});
		}

		public static void CheckC22(Ui ui,CheckResult checkResult) {
			//The calendars of two standard report windows (UI-CAL): the program's MonthCalendarOD where Windows' MonthCalendar was, and the Windows
			//Forms DateTimePicker (comctl32's box, drawn by Windows on both runtimes) whose drop-down is the program's MonthCalendarOD
			//(UiDateDropLook). Nothing is run.
			Ui.Opened openedReports=OpenMenu(ui,"Reports","Standard");
			if(!InspectWindow(ui,checkResult,openedReports,"FormReportsMore",null,close:false)) {
				return;
			}
			IntPtr hwndReports=openedReports.Hwnd;
			//Public Health > Raw Population Data (C6 opens it too): two MonthCalendarODs.
			ShootStandardReport(ui,checkResult,hwndReports,"ODRawPopulationData","FormRpPHRawPop",typeof(OpenPmsDental.UI.MonthCalendarOD),"C22-monthcalendarod.png");
			//Lists > Payment Plans: two DateTimePickers, the first one's drop-down opened as its drop button opens it (UiDateDropLook.OpenForTest).
			ShootStandardReport(ui,checkResult,hwndReports,"ODPaymentPlans","FormRpPayPlans",typeof(DateTimePicker),"C22-datetimepicker.png",
				hwnd => ShootDateDropDown(ui,checkResult,hwnd,"C22-datetimepicker-dropdown.png"));
			checkResult.Expect(ui.Close(hwndReports),"Standard reports closes");
			openedReports.Posted?.Wait();
		}

		public static void CheckC23(Ui ui,CheckResult checkResult) {
			//Setup > Auto Notes (FormAutoNotes) > Export (FormAutoNoteExport): the auto notes in a TreeView with CheckBoxes. The seed has none: a
			//category (an AutoNoteCats definition) and two auto notes are made by the business layer and deleted again at the end. Select All checks
			//every box (nothing is exported: Export Selected is never pressed).
			int countDefsBefore=Count("SELECT COUNT(*) FROM definition WHERE Category="+POut.Int((int)DefCat.AutoNoteCats));
			int countNotesBefore=Count("SELECT COUNT(*) FROM autonote");
			Def def=new Def { Category=DefCat.AutoNoteCats,ItemName="Window checks (synthetic)",ItemOrder=countDefsBefore,ItemValue="",
				ItemColor=Color.Empty };
			List<AutoNote> listAutoNotes=new List<AutoNote> {
				new AutoNote { AutoNoteName="Window checks note 1",MainText="Synthetic auto note 1.",Category=0 },
				new AutoNote { AutoNoteName="Window checks note 2",MainText="Synthetic auto note 2.",Category=0 },
			};
			ui.Invoke(() => {
				Defs.Insert(def);
				for(int i=0;i<listAutoNotes.Count;i++) {
					listAutoNotes[i].Category=def.DefNum;
					AutoNotes.Insert(listAutoNotes[i]);
				}
				DataValid.SetInvalid(InvalidType.Defs,InvalidType.AutoNotes);
				Cache.Refresh(InvalidType.Defs,InvalidType.AutoNotes);
			});
			checkResult.Note("Auto note category "+def.DefNum+" and auto notes "+string.Join(", ",listAutoNotes.Select(x => x.AutoNoteNum))+" made by the business layer");
			try {
				Ui.Opened openedNotes=ui.Open("Setup > Auto Notes",() => Ui.Call(ui.FormMain,"menuItemAutoNotes_Click",ui.FormMain,EventArgs.Empty));
				if(InspectWindow(ui,checkResult,openedNotes,"FormAutoNotes",null,close:false)) {
					IntPtr hwndNotes=openedNotes.Hwnd;
					Ui.Opened openedExport=ui.Open("Auto Notes > Export",() => Ui.Call(Control.FromHandle(hwndNotes),"butExport_Click",Control.FromHandle(hwndNotes),EventArgs.Empty));
					if(InspectWindow(ui,checkResult,openedExport,"FormAutoNoteExport",null,close:false)) {
						IntPtr hwndExport=openedExport.Hwnd;
						ui.Invoke(() => {
							Form formExport=(Form)Control.FromHandle(hwndExport);
							Ui.Call(formExport,"butSelectAll_Click",formExport,EventArgs.Empty);
							TreeView treeView=(TreeView)Ui.GetField(formExport,"treeNotes");
							treeView.ExpandAll();
						});
						string[] arrayTree=ui.Invoke(() => {
							TreeView treeView=(TreeView)Ui.GetField(Control.FromHandle(hwndExport),"treeNotes");
							List<TreeNode> listNodes=treeView.Nodes.Cast<TreeNode>().SelectMany(x => new[] { x }.Concat(x.Nodes.Cast<TreeNode>())).ToList();
							return new[] { treeView.CheckBoxes.ToString(),listNodes.Count.ToString(),listNodes.Count(x => x.Checked).ToString(),
								string.Join(", ",listNodes.Select(x => x.Text+(x.Checked ? " [x]" : " [ ]"))),
								treeView.ImageList==null ? "none" : treeView.ImageList.ColorDepth+", "+treeView.ImageList.Images.Count+" images" };
						});
						checkResult.Expect(arrayTree[0]=="True","The auto notes' TreeView has CheckBoxes");
						checkResult.Expect(arrayTree[1]=="3" && arrayTree[2]=="3","Select All checks the category and its two notes: "+arrayTree[3],arrayTree[1]+" nodes, "
							+arrayTree[2]+" checked");
						checkResult.Note("The tree's ImageList (from the designer's ImageStream): "+arrayTree[4]);
						checkResult.AddShot(ui.Shot(hwndExport,"C23.png"),"Auto Note Export: the TreeView with its check boxes checked");
						checkResult.Expect(ui.Close(hwndExport),"Auto Note Export closes (nothing exported)");
					}
					openedExport.Posted?.Wait();
					checkResult.Expect(ui.Close(hwndNotes),"Auto Notes closes");
				}
				openedNotes.Posted?.Wait();
			}
			finally {
				ui.Invoke(() => {
					for(int i=0;i<listAutoNotes.Count;i++) {
						if(listAutoNotes[i].AutoNoteNum>0) {
							AutoNotes.Delete(listAutoNotes[i].AutoNoteNum);
						}
					}
					if(def.DefNum>0) {
						DataCore.NonQ("DELETE FROM definition WHERE DefNum="+POut.Long(def.DefNum));
					}
					DataValid.SetInvalid(InvalidType.Defs,InvalidType.AutoNotes);
					Cache.Refresh(InvalidType.Defs,InvalidType.AutoNotes);
				});
			}
			checkResult.Expect(Count("SELECT COUNT(*) FROM definition WHERE Category="+POut.Int((int)DefCat.AutoNoteCats))==countDefsBefore
				&& Count("SELECT COUNT(*) FROM autonote")==countNotesBefore,"Undo: the category and the two auto notes are gone");
		}

		public static void CheckC24(Ui ui,CheckResult checkResult) {
			//RichTextBox text: the program's ODtextBox (a RichTextBox with the program's spell check) on Edit Patient Information, with a synthetic
			//text of three lines and a misspelt word. The window is closed with Cancel: nothing is saved.
			string addrNoteBefore=ui.Invoke(() => DataCore.GetScalar("SELECT AddrNote FROM patient WHERE PatNum=1"));
			GoToModule(ui,EnumModuleType.Family);
			Control controlFamily=ModuleControl("controlFamily");
			Ui.Opened opened=ui.Open("Family > double-click the patient information",() => {
				Control gridPat=(Control)Ui.GetField(controlFamily,"gridPat");
				DoubleClickRowOnThread(gridPat,0);
			});
			if(!InspectWindow(ui,checkResult,opened,"FormPatientEdit",null,close:false)) {
				return;
			}
			string text="Window checks (WINCHK-5): synthetic text in a RichTextBox."+Environment.NewLine+"Line 2: 0123456789 ABCDEFGHIJKLMNOPQRSTUVWXYZ"
				+Environment.NewLine+"Line 3 with a mispelled word.";
			string[] arrayBox=ui.Invoke(() => {
				List<RichTextBox> listBoxes=Ui.AllControls(Control.FromHandle(opened.Hwnd)).OfType<RichTextBox>().Where(x => x.Visible && x.Enabled && !x.ReadOnly).ToList();
				RichTextBox richTextBox=listBoxes.FirstOrDefault(x => x.Name=="textAddrNotes")??listBoxes.FirstOrDefault();
				if(richTextBox==null) {
					return new[] { "","","" };
				}
				richTextBox.Text=text;
				return new[] { richTextBox.Name,richTextBox.GetType().FullName,richTextBox.Font.Name+" "+richTextBox.Font.SizeInPoints.ToString("0.##")+" pt" };
			});
			if(checkResult.Expect(arrayBox[0]!="","Edit Patient Information has an editable RichTextBox ("+arrayBox[0]+", "+arrayBox[1]+", "+arrayBox[2]+")")) {
				System.Threading.Thread.Sleep(1500);//the spell check marks the misspelt word after a pause
				string textShown=ui.Invoke(() => Ui.AllControls(Control.FromHandle(opened.Hwnd)).First(x => x.Name==arrayBox[0]).Text);
				checkResult.Expect(textShown.Replace("\r\n","\n").Replace("\r","\n")==text.Replace("\r\n","\n"),"The RichTextBox shows the three lines");
				checkResult.AddShot(ui.Shot(opened.Hwnd,"C24.png"),"Edit Patient Information with the text in "+arrayBox[0]);
				checkResult.ForPerson("the text in C24.png ("+arrayBox[0]+"): its font, line breaks and the spell check's red underline under 'mispelled'");
			}
			checkResult.Expect(ui.Close(opened.Hwnd),"Edit Patient Information closes (Cancel)");
			opened.Posted?.Wait();
			string addrNoteAfter=ui.Invoke(() => DataCore.GetScalar("SELECT AddrNote FROM patient WHERE PatNum=1"));
			checkResult.Expect(addrNoteAfter==addrNoteBefore,"Undo: nothing saved (the patient's AddrNote is unchanged)");
		}

		public static void CheckC25(Ui ui,CheckResult checkResult) {
			//The capture notice and the acquisition status window of ImagingDeviceManager (design 7.1), shown by its production ICaptureUi
			//(WinFormsCaptureUi, internal: reached by reflection) as an acquisition shows them. They set SystemFonts.MessageBoxFont themselves (they
			//are not default-font windows in this fork), so the check is that they have it.
			Assembly assembly=Assembly.Load("ImagingDeviceManager");
			Type typeCaptureUi=assembly.GetType("ImagingDeviceManager.UI.WinFormsCaptureUi",true);
			Type typeController=assembly.GetType("ImagingDeviceManager.UI.AcquireStatusController",true);
			Type typeView=assembly.GetType("ImagingDeviceManager.UI.IAcquireStatusView",true);
			object captureUi=ui.Invoke(() => Activator.CreateInstance(typeCaptureUi,nonPublic:true));
			string fontExpected=ui.Invoke(() => SystemFonts.MessageBoxFont.Name+" "+SystemFonts.MessageBoxFont.SizeInPoints.ToString("0.##")+" pt");
			string textNotice="Window checks (WINCHK-5): a capture notice, as an acquisition shows one (synthetic text)."+Environment.NewLine
				+"Second line: 0123456789 ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			Ui.Opened openedNotice=ui.Open("The capture notice (WinFormsCaptureUi.ShowNotice)",() => typeCaptureUi.GetMethod("ShowNotice").Invoke(captureUi,new object[] { textNotice }));
			if(InspectWindow(ui,checkResult,openedNotice,"CaptureNoticeWindow",null,close:false)) {
				string[] arrayNotice=ui.Invoke(() => {
					Form form=(Form)Control.FromHandle(openedNotice.Hwnd);
					return new[] { form.Font.Name+" "+form.Font.SizeInPoints.ToString("0.##")+" pt",form.TopMost.ToString(),
						string.Join(" | ",Ui.AllControls(form).Select(x => x.Text.Replace("\r\n"," / "))) };
				});
				checkResult.Expect(arrayNotice[0]==fontExpected,"The notice has the message box font ("+arrayNotice[0]+")",arrayNotice[0]);
				checkResult.Expect(arrayNotice[1]=="True","The notice is topmost");
				checkResult.Expect(arrayNotice[2].Contains("a capture notice") && arrayNotice[2].Contains("OK"),"The notice shows its text and OK: "+arrayNotice[2]);
				checkResult.AddShot(ui.Shot(openedNotice.Hwnd,"C25-notice.png"),"The capture notice");
				checkResult.Expect(ui.Close(openedNotice.Hwnd),"The notice closes");
			}
			object controller=Activator.CreateInstance(typeController,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,null,
				new object[] { (Func<bool>)(() => false),false,(Action)(() => { }),"Window checks: 3 unassigned captures on this computer (synthetic)." },null);
			object view=null;
			Ui.Opened openedStatus=ui.Open("The acquisition status window (WinFormsCaptureUi.ShowStatus)",
				() => view=typeCaptureUi.GetMethod("ShowStatus").Invoke(captureUi,new object[] { controller }));
			if(InspectWindow(ui,checkResult,openedStatus,"AcquireStatusWindow",null,close:false)) {
				string[] arrayStatus=ui.Invoke(() => {
					Form form=(Form)Control.FromHandle(openedStatus.Hwnd);
					return new[] { form.Font.Name+" "+form.Font.SizeInPoints.ToString("0.##")+" pt",
						string.Join(" | ",Ui.AllControls(form).Where(x => x.Visible).Select(x => x.Text.Replace("\r\n"," / "))) };
				});
				checkResult.Expect(arrayStatus[0]==fontExpected,"The status window has the message box font ("+arrayStatus[0]+")",arrayStatus[0]);
				checkResult.Note("The status window shows: "+arrayStatus[1]);
				checkResult.AddShot(ui.Shot(openedStatus.Hwnd,"C25-status.png"),"The acquisition status and Cancel window");
				//Closed as the library closes it when the acquisition ends (a person's close is a cancel request that keeps it).
				ui.Invoke(() => typeView.GetMethod("Close").Invoke(view,null));
				checkResult.Expect(ui.WaitGone(openedStatus.Hwnd),"The status window closes when the acquisition ends (IAcquireStatusView.Close)");
			}
		}

		public static void CheckC26(Ui ui,CheckResult checkResult) {
			//The EOB viewer (FormImages, rebuilt by S0-6 on the Imaging module's parts: its WPF ToolBar and ZoomSlider hosted in ElementHosts), as
			//Insurance Payment's View EOB opens it (FormClaimPayBatch: ClaimPaymentNum). The seed has no EOB: a synthetic page is imported for a claim
			//payment key no payment has (ImageStore.ImportEobAttach, into the scratch copy's A to Z folder) and deleted again (DeleteEobAttach).
			long claimPaymentNum=MaxKey("claimpayment","ClaimPaymentNum")+900000;
			int countBefore=Count("SELECT COUNT(*) FROM eobattach");
			EobAttach eobAttach=null;
			try {
				eobAttach=ui.Invoke(() => {
					using Bitmap bitmap=new Bitmap(850,1100);
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(Color.White);
						using Font fontTitle=new Font("Arial",28,FontStyle.Bold);
						using Font font=new Font("Arial",16);
						g.DrawString("EOB (synthetic, window checks)",fontTitle,Brushes.Black,60,60);
						for(int i=0;i<12;i++) {
							g.DrawString("Line "+(i+1)+": D0120   periodic evaluation   $0.00",font,Brushes.Black,60,160+i*50);
						}
						g.DrawRectangle(Pens.Black,40,40,770,1020);
					}
					return ImageStore.ImportEobAttach(bitmap,claimPaymentNum);
				});
				checkResult.Note("EOB "+eobAttach.EobAttachNum+" ("+eobAttach.FileName+") made for claim payment key "+claimPaymentNum+" by ImageStore.ImportEobAttach");
				Ui.Opened opened=ui.Open("View EOB (FormImages with ClaimPaymentNum)",() => {
					using FormImages formImages=new FormImages();
					formImages.ClaimPaymentNum=claimPaymentNum;
					formImages.ShowDialog();
				});
				if(InspectWindow(ui,checkResult,opened,"FormImages",null,close:false)) {
					System.Threading.Thread.Sleep(2000);//the list fills and the first EOB shows once the window is visible
					string[] arrayWindow=ui.Invoke(() => {
						Form form=(Form)Control.FromHandle(opened.Hwnd);
						List<System.Windows.Forms.Integration.ElementHost> listHosts=Ui.AllControls(form).OfType<System.Windows.Forms.Integration.ElementHost>()
							.Where(x => x.Visible).ToList();
						IList listEntries=(IList)Ui.GetField(form,"_listFileEntries");
						Bitmap bitmapShowing=(Bitmap)Ui.GetField(form,"_bitmapShowing");
						return new[] { string.Join(", ",listHosts.Select(x => x.Name+": "+(x.Child==null ? "empty" : x.Child.GetType().Name))),listEntries.Count.ToString(),
							bitmapShowing==null ? "" : bitmapShowing.Width+" x "+bitmapShowing.Height };
					});
					checkResult.Expect(arrayWindow[0].Contains("ToolBar") && arrayWindow[0].Contains("ZoomSlider"),"The hosted WPF strips show: "+arrayWindow[0],arrayWindow[0]);
					checkResult.Expect(arrayWindow[1]=="1","The list holds the one EOB ("+arrayWindow[1]+")");
					checkResult.Expect(arrayWindow[2]=="850 x 1100","The EOB's image shows ("+arrayWindow[2]+")",arrayWindow[2]);
					checkResult.AddShot(ui.Shot(opened.Hwnd,"C26.png"),"The EOB viewer with the synthetic EOB");
					checkResult.ForPerson("the EOB viewer in C26.png: the toolbar's and the zoom slider's look (WPF hosted in Windows Forms), the page fitted");
					checkResult.Expect(ui.Close(opened.Hwnd),"The EOB viewer closes");
				}
				opened.Posted?.Wait();
			}
			finally {
				if(eobAttach!=null) {
					ui.Invoke(() => ImageStore.DeleteEobAttach(eobAttach));
				}
			}
			checkResult.Expect(Count("SELECT COUNT(*) FROM eobattach")==countBefore,"Undo: the EOB row is gone");
			if(eobAttach!=null) {
				string path=ui.Invoke(() => ODFileUtils.CombinePaths(ImageStore.GetEobFolder(),eobAttach.FileName));
				checkResult.Expect(!System.IO.File.Exists(path),"Undo: the EOB's file is gone ("+path+")");
			}
		}

		public static void CheckD6(Ui ui,CheckResult checkResult) {
			//The WPF pop-ups of a Frm hosted in FormFrame (Select Patient, FrmPatientSelect): the combo box's picker (WindowComboPicker) as its
			//mouse-up opens it, the month calendar (WindowMonthCalendar) as DatePicker opens it below a control, and WpfControls.UI.ToolTip with no
			//delay. Each is a top-level WPF window of its own; each is printed and closed. No production Frm has a DatePicker (only the developer
			//test Frms FrmTestAllControls and FrmTestCombos), so the calendar is shown below the combo box.
			Ui.Opened opened=OpenToolbar(ui,"Patient");
			if(!InspectWindow(ui,checkResult,opened,"FrmPatientSelect",null,close:false)) {
				return;
			}
			System.Windows.FrameworkElement frm=ui.GetFrm(opened.Hwnd);
			string focus=ui.Invoke(() => {
				System.Windows.IInputElement inputElement=System.Windows.Input.Keyboard.FocusedElement;
				return inputElement==null ? "none" : inputElement.GetType().Name+(inputElement is System.Windows.FrameworkElement frameworkElement && frameworkElement.Name!=""
					? " "+frameworkElement.Name : "");
			});
			checkResult.Note("WPF keyboard focus after the window opened (no input sent): "+focus);
			System.Windows.FrameworkElement combo=ui.Invoke(() => new[] { "comboBillingType","comboSite" }.Select(x => Ui.GetField(frm,x) as System.Windows.FrameworkElement)
				.FirstOrDefault(x => x!=null && x.IsVisible && x.IsEnabled && (int)Member(Member(x,"Items"),"Count")>0));
			if(checkResult.Expect(combo!=null,"Select Patient has a WPF combo box with items")) {
				string comboName=ui.Invoke(() => combo.Name);
				Ui.Opened openedPicker=ui.Open("The combo box "+comboName+": its mouse-up opens the picker",() => {
					Ui.SetField(combo,"_isMouseDown",true);
					Ui.Call(combo,"comboBox_MouseLeftButtonUp",combo,null);
				});
				if(openedPicker.IsOpen) {
					checkResult.Expect(ui.DescribeWindow(openedPicker.Hwnd).Contains("WindowComboPicker"),"The picker opens ("+ui.DescribeWindow(openedPicker.Hwnd)+")");
					checkResult.AddShot(ui.Shot(openedPicker.Hwnd,"D6-combo.png"),"The WPF combo box's picker (WindowComboPicker) of "+comboName);
					checkResult.Expect(ui.Close(openedPicker.Hwnd),"The picker closes");
				}
				Ui.Opened openedCalendar=ui.Open("The month calendar pop-up, as DatePicker opens it",() => {
					WpfControls.UI.WindowMonthCalendar windowMonthCalendar=new WpfControls.UI.WindowMonthCalendar();
					System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(windowMonthCalendar);
					windowMonthCalendar.SetDateSelected(new DateTime(2026,11,16));
					windowMonthCalendar.PointInitial=combo.PointToScreen(new System.Windows.Point(0,combo.ActualHeight));
					windowMonthCalendar.Show();
				});
				if(openedCalendar.IsOpen) {
					checkResult.Expect(ui.DescribeWindow(openedCalendar.Hwnd).Contains("WindowMonthCalendar"),"The calendar opens ("+ui.DescribeWindow(openedCalendar.Hwnd)+")");
					checkResult.AddShot(ui.Shot(openedCalendar.Hwnd,"D6-calendar.png"),"The WPF month calendar pop-up (WindowMonthCalendar), 16 November 2026 selected");
					checkResult.Expect(ui.Close(openedCalendar.Hwnd),"The calendar closes");
				}
			}
			System.Windows.FrameworkElement elementTip=ui.Invoke(() => new[] { "butSearch","gridMain" }.Select(x => Ui.GetField(frm,x) as System.Windows.FrameworkElement)
				.FirstOrDefault(x => x!=null && x.IsVisible && !x.IsKeyboardFocusWithin));
			if(checkResult.Expect(elementTip!=null,"Select Patient has an element for the tool tip")) {
				Ui.Opened openedTip=ui.Open("WpfControls.UI.ToolTip on "+ui.Invoke(() => elementTip.Name),() => {
					WpfControls.UI.ToolTip toolTip=new WpfControls.UI.ToolTip();
					toolTip.SetControlAndAction(elementTip,(frameworkElement,point) => { });
					toolTip.TimeSpanDelay=TimeSpan.Zero;
					toolTip.SetString(elementTip,"Window checks (WINCHK-5): a WPF tool tip");
				});
				if(openedTip.IsOpen) {
					checkResult.Expect(ui.DescribeWindow(openedTip.Hwnd).Contains("ToolTip"),"The tool tip shows ("+ui.DescribeWindow(openedTip.Hwnd)+")");
					AddTextShot(checkResult,ui.Shot(openedTip.Hwnd,"D6-tooltip.png"),"The WPF tool tip");
					checkResult.Expect(ui.Close(openedTip.Hwnd),"The tool tip closes");
				}
			}
			checkResult.ForPerson("the keyboard focus in a Frm (Tab order, typing into its fields, Alt shortcuts): keyboard input, which the harness does not send");
			checkResult.Expect(ui.Close(opened.Hwnd),"Select Patient closes (Cancel)");
			opened.Posted?.Wait();
		}

		///<summary>The RDL viewer of a FormReportCustom once its pages are built (or 30 s): the texts of every page's text items, the viewer's own
		///messages (_errorMsgs) and whether its warning button shows.</summary>
		private static void ReadRdlViewer(Ui ui,IntPtr hwnd,out Control viewer,out List<string> listTexts,out List<string> listMessages,out bool isWarningVisible,
			out int countPages)
		{
			Control viewerFound=ui.Invoke(() => (Control)Ui.GetField(Control.FromHandle(hwnd),"viewer"));
			viewer=viewerFound;
			System.Diagnostics.Stopwatch stopwatch=System.Diagnostics.Stopwatch.StartNew();
			while(stopwatch.Elapsed.TotalSeconds<30 && ui.Invoke(() => Ui.GetField(viewerFound,"_pgs"))==null) {
				System.Threading.Thread.Sleep(300);
			}
			System.Threading.Thread.Sleep(1000);
			List<string> listTextsFound=new List<string>();
			List<string> listMessagesFound=new List<string>();
			bool isVisible=false;
			int count=0;
			ui.Invoke(() => {
				if(Ui.GetField(viewerFound,"_pgs") is IEnumerable pages) {
					foreach(object page in pages) {
						count++;
						foreach(object pageItem in (IEnumerable)page) {
							if(pageItem.GetType().GetProperty("Text")?.GetValue(pageItem) is string text) {
								listTextsFound.Add(text);
							}
						}
					}
				}
				if(Ui.GetField(viewerFound,"_errorMsgs") is IList listErrors) {
					listMessagesFound.AddRange(listErrors.Cast<object>().Select(x => x?.ToString()??""));
				}
				isVisible=((Control)Ui.GetField(viewerFound,"_WarningButton")).Visible;
			});
			listTexts=listTextsFound;
			listMessages=listMessagesFound;
			isWarningVisible=isVisible;
			countPages=count;
		}

		///<summary>The text's words joined by single spaces.</summary>
		private static string NormalizeSpaceInProc(string text) {
			return string.Join(" ",(text??"").Split(new[] { ' ','\r','\n','\t' },StringSplitOptions.RemoveEmptyEntries));
		}

		///<summary>Opens a report of Reports > Custom Reports (the .rdl files of the A to Z folder's report folder, listed at start-up) and returns
		///the FormReportCustom (no parameter window: the G4 and G5 reports have no parameters).</summary>
		private static Ui.Opened OpenCustomReport(Ui ui,string name) {
			ToolStripMenuItem toolStripMenuItem=ui.Invoke(() => GetMenuItem("Custom Reports",name));
			return ui.Open("Reports > Custom Reports > "+name,() => toolStripMenuItem.PerformClick());
		}

		public static void CheckG4(Ui ui,CheckResult checkResult) {
			//A report whose expressions call three VB functions of its <Code> block (compiled by Roslyn on .NET 10, S3b-2 part B), on the scratch copy's
			//patients through the program's report connection. The page's texts are read from the viewer's pages and compared with the functions'
			//results computed here from the same rows.
			DataTable table=ui.Invoke(() => DataCore.GetTable(RdlCodeReports.QUERY));
			List<string> listExpected=new List<string> { RdlCodeReports.TITLE };
			foreach(DataRow dataRow in table.Rows) {
				listExpected.Add(dataRow["LName"].ToString().ToUpperInvariant()+", "+dataRow["FName"]);
				listExpected.Add((PIn.Long(dataRow["PatNum"].ToString())*3).ToString());
			}
			Ui.Opened opened=OpenCustomReport(ui,RdlCodeReports.NAME_CODE);
			if(!InspectWindow(ui,checkResult,opened,"FormReportCustom",null,close:false)) {
				return;
			}
			ReadRdlViewer(ui,opened.Hwnd,out Control viewer,out List<string> listTexts,out List<string> listMessages,out bool isWarningVisible,out int countPages);
			checkResult.Note("The page's texts: "+string.Join(" | ",listTexts));
			checkResult.Expect(countPages==1,"The report has one page ("+countPages+")");
			checkResult.Expect(listMessages.Count==0 && !isWarningVisible,"No message from the engine (the Code block compiled)",string.Join(" || ",listMessages));
			List<string> listMissing=listExpected.Where(x => !listTexts.Contains(x)).ToList();
			checkResult.Expect(table.Rows.Count>0 && listMissing.Count==0,"The page shows what Code.Title(), Code.PatientName() and Code.Triple() give for the "
				+table.Rows.Count+" patients: "+string.Join(", ",listExpected),"missing: "+string.Join(", ",listMissing));
			checkResult.AddShot(ui.Shot(opened.Hwnd,"G4.png"),"The report with VB Code functions in the RDL viewer");
			checkResult.ForPerson("the page in G4.png (the title from Code.Title(), the table's rows)");
			checkResult.Expect(ui.Close(opened.Hwnd),"The report closes");
			opened.Posted?.Wait();
		}

		public static void CheckG5(Ui ui,CheckResult checkResult) {
			//A report whose <Code> block does not compile: the viewer reports the compile error (BC30451, the name it does not declare) either as its
			//warning or on its error page ("Errors processing report"; the expression that calls the missing function is a severity 8 error, so the
			//report does not load). The viewer's messages dialog (DialogMessages, a plain form on the default font) is opened as its warning button
			//opens it (WarningClick), with the messages the viewer holds then; without the button, with the compile error.
			Ui.Opened opened=OpenCustomReport(ui,RdlCodeReports.NAME_CODE_ERROR);
			if(!InspectWindow(ui,checkResult,opened,"FormReportCustom",null,close:false)) {
				return;
			}
			ReadRdlViewer(ui,opened.Hwnd,out Control viewer,out List<string> listTexts,out List<string> listMessages,out bool isWarningVisible,out int countPages);
			checkResult.Note("Pages "+countPages+"; texts: "+string.Join(" | ",listTexts)+"; the viewer's messages: "+string.Join(" || ",listMessages)
				+"; warning button "+(isWarningVisible ? "shown" : "not shown"));
			List<string> listAll=listTexts.Concat(listMessages).ToList();
			List<string> listError=listAll.Where(x => x.Contains("BC30451") || x.Contains(RdlCodeReports.NAME_UNDECLARED)).Distinct().ToList();
			checkResult.Expect(listError.Count>0,"The compile error is reported: "+string.Join(" || ",listError),string.Join(" || ",listAll));
			checkResult.AddShot(ui.Shot(opened.Hwnd,"G5.png"),"The report whose Code does not compile, in the RDL viewer");
			Type typeDialog=viewer.GetType().Assembly.GetType("fyiReporting.RdlViewer.DialogMessages",true);
			Ui.Opened openedMessages;
			List<string> listForDialog;
			if(isWarningVisible) {
				//The messages WarningClick hands DialogMessages, read just before the click. With the error page they are the warnings of the error page's
				//own run: RdlViewer.GetReportErrorMsg gives that page a data source with the provider "xxx", which no connection opens ("Unable to
				//connect to datasource 'DS1'"), on any runtime; its rows are inline, so the page still shows the messages.
				listForDialog=ui.Invoke(() => ((IList)Ui.GetField(viewer,"_errorMsgs"))?.Cast<object>().Select(x => x?.ToString()??"").ToList()??new List<string>());
				checkResult.Note("The warning button's messages: "+string.Join(" || ",listForDialog));
				openedMessages=ui.Open("The viewer's warning button (WarningClick)",() => Ui.Call(viewer,"WarningClick",viewer,EventArgs.Empty));
			}
			else {
				checkResult.Note("No warning button: DialogMessages is opened with the compile error");
				listForDialog=listError.Count>0 ? listError : listAll;
				IList listDialog=new ArrayList(listForDialog);
				openedMessages=ui.Open("DialogMessages with the viewer's messages",() => {
					using Form form=(Form)Activator.CreateInstance(typeDialog,new object[] { listDialog });
					form.ShowDialog();
				});
			}
			if(InspectWindow(ui,checkResult,openedMessages,"DialogMessages",null,close:false)) {
				Form formMessages=ui.GetForm(openedMessages.Hwnd);
				ExpectDefaultFont(checkResult,formMessages,"DialogMessages");
				string textShown=ui.Invoke(() => string.Join(" ",Ui.AllControls(formMessages).Select(x => x.Text)));
				List<string> listNotShown=listForDialog.Where(x => !NormalizeSpaceInProc(textShown).Contains(NormalizeSpaceInProc(x))).ToList();
				checkResult.Expect(listForDialog.Count>0 && listNotShown.Count==0,"DialogMessages shows the "+listForDialog.Count+" message(s) it was given",
					"not shown: "+string.Join(" || ",listNotShown));
				checkResult.AddShot(ui.Shot(openedMessages.Hwnd,"G5-messages.png"),"The RDL viewer's messages dialog (DialogMessages)");
				checkResult.Expect(ui.Close(openedMessages.Hwnd),"DialogMessages closes");
			}
			openedMessages.Posted?.Wait();
			//DataSourcePassword, RdlViewer's other dialog (RdlDesigner asks with it for a shared data source's pass phrase): shown and printed.
			Type typePassword=viewer.GetType().Assembly.GetType("fyiReporting.RdlViewer.DataSourcePassword",true);
			Ui.Opened openedPassword=ui.Open("DataSourcePassword",() => {
				using Form form=(Form)Activator.CreateInstance(typePassword);
				form.ShowDialog();
			});
			if(InspectWindow(ui,checkResult,openedPassword,"DataSourcePassword",null,close:false)) {
				ExpectDefaultFont(checkResult,ui.GetForm(openedPassword.Hwnd),"DataSourcePassword");
				checkResult.AddShot(ui.Shot(openedPassword.Hwnd,"G5-password.png"),"RdlViewer's DataSourcePassword dialog (nothing typed)");
				checkResult.Expect(ui.Close(openedPassword.Hwnd),"DataSourcePassword closes (Cancel)");
			}
			openedPassword.Posted?.Wait();
			checkResult.Expect(ui.Close(opened.Hwnd),"The report closes");
			opened.Posted?.Wait();
		}
	}
}
