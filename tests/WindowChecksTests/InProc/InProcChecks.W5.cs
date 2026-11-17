// The stage 3b (.NET 10) checks WINCHK-5 added from section 6 of docs/specs/net10-plan.md ("Stage 3b (net10)"): the plain forms that use the
// default font (the .NET Framework's Microsoft Sans Serif 8.25, which ProgramEntry.Main sets on .NET 10, decision 9), each shown as the program
// shows it and printed: MsgBoxCopyPaste (C16), the progress windows of CodeBase (C17), the combo box drop-down that the clinic picker and every
// UI.ComboBox open (C18), ToolTipOD (C19); the dashboard's graphs drawn on their own (F5, OpenPmsDentalGraph on the Windows Forms charting
// package of .NET 10); and a classic print preview (G3, FormPrintPreview of the time cards). Nothing is pressed that prints, saves or sends;
// G3's employee and pay period are made by the business layer and deleted again.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental;
using OpenPmsDentalBusiness;

namespace WindowChecksTests.InProc {
	public static partial class InProcChecks {
		///<summary>The form's font, on its own thread. A form that sets no font of its own (ShouldSerializeFont false: it takes the ambient
		///Control.DefaultFont) must have the program's default font, Microsoft Sans Serif at 8.25 pt times the Windows text scale factor (decision
		///9; Segoe UI 9 would mean the default font was not set before the first window); a form with its own font is noted.</summary>
		private static void ExpectDefaultFont(CheckResult checkResult,Form form,string what) {
			string[] arrayFont=(string[])form.Invoke((Func<string[]>)(() => new[] { form.Font.Name,form.Font.SizeInPoints.ToString("0.##"),
				Control.DefaultFont.Name,Control.DefaultFont.SizeInPoints.ToString("0.##"),((bool)Ui.Call(form,"ShouldSerializeFont")).ToString() }));
			string font=arrayFont[0]+" "+arrayFont[1]+" pt";
			if(arrayFont[4]=="True") {
				checkResult.Note(what+" sets its own font: "+font);
				return;
			}
			checkResult.Expect(arrayFont[0]=="Microsoft Sans Serif" && arrayFont[0]==arrayFont[2] && arrayFont[1]==arrayFont[3],
				what+" uses the program's default font, Microsoft Sans Serif ("+font+"; the default font is "+arrayFont[2]+" "+arrayFont[3]+" pt)",font);
		}

		///<summary>AddShot for a picture of a line of text on one background (a tooltip, a menu bar): such a picture has few colours (text drawn
		///without smoothing), so not AddShot's "more than 5 colours" of a window; it is drawn when it has two colours or more and part of it is not
		///background.</summary>
		private static void AddTextShot(CheckResult checkResult,ShotInfo shotInfo,string whatItShows) {
			if(!checkResult.Expect(shotInfo.Error=="",shotInfo.FileName+" is taken",shotInfo.Error)) {
				return;
			}
			checkResult.ListShots.Add(shotInfo.FileName);
			checkResult.Expect(shotInfo.Width>0 && shotInfo.Height>0 && shotInfo.CountColors>=2 && shotInfo.ShareNonBackground>0.05,
				whatItShows+" is drawn: its text on its background ("+shotInfo+")",shotInfo.ToString());
		}

		public static void CheckC16(Ui ui,CheckResult checkResult) {
			string text="Window checks (WINCHK-5): a text in MsgBoxCopyPaste, as the program shows a long message or an error's details."+Environment.NewLine
				+Environment.NewLine+"Line 3: 0123456789 ABCDEFGHIJKLMNOPQRSTUVWXYZ abcdefghijklmnopqrstuvwxyz";
			Ui.Opened opened=ui.Open("MsgBoxCopyPaste with a synthetic text",() => {
				OpenPmsDental.MsgBoxCopyPaste msgBoxCopyPaste=new OpenPmsDental.MsgBoxCopyPaste(text);
				msgBoxCopyPaste.Show();
			});
			if(!InspectWindow(ui,checkResult,opened,"MsgBoxCopyPaste",null,close:false)) {
				return;
			}
			Form form=ui.GetForm(opened.Hwnd);
			ExpectDefaultFont(checkResult,form,"MsgBoxCopyPaste");
			string textShown=ui.Invoke(() => ((CodeBase.MsgBoxCopyPaste)form).textMain.Text);
			checkResult.Expect(textShown==text,"The text shows in its box");
			checkResult.AddShot(ui.Shot(opened.Hwnd,"C16.png"),"MsgBoxCopyPaste");
			checkResult.Expect(ui.Close(opened.Hwnd),"MsgBoxCopyPaste closes");
		}

		///<summary>Shows a progress window through ODProgress (on its own thread, as the program shows it), prints it, checks its font and closes it
		///with the close action the program calls when its work is done.</summary>
		private static void ShowProgressAndShoot(CheckResult checkResult,Func<Action> funcShow,string typeName,string shot) {
			Action actionClose=funcShow();
			try {
				FormProgressBase formProgressBase=ODProgress.FormProgressActive;
				if(!checkResult.Expect(formProgressBase!=null && formProgressBase.GetType().Name==typeName,"The progress window "+typeName+" shows",
					formProgressBase?.GetType().Name??"none"))
				{
					return;
				}
				IntPtr hwnd=(IntPtr)formProgressBase.Invoke((Func<IntPtr>)(() => formProgressBase.Handle));
				System.Threading.Thread.Sleep(1500);//its first status and the bar's first steps
				ExpectDefaultFont(checkResult,formProgressBase,typeName);
				checkResult.AddShot(WindowChecksTests.Shot.Window(hwnd,shot),typeName);
			}
			finally {
				actionClose();
			}
			checkResult.Expect(ODProgress.FormProgressActive==null,typeName+" closes when the work is done (its close action)");
		}

		public static void CheckC17(Ui ui,CheckResult checkResult) {
			//The CodeBase progress windows (plain forms on the default font); ProgressWin, the WPF one, is not a default-font window.
#pragma warning disable CS0618 //ODProgress.Show is deprecated in favour of ProgressWin, but the program still shows FormProgressStatus through it.
			ShowProgressAndShoot(checkResult,() => ODProgress.Show("Window checks (WINCHK-5): a progress window that finishes",hasHistory:false,
				hasMinimize:true,progStyle:ProgressBarStyle.Marquee),"FormProgressStatus","C17-status.png");
#pragma warning restore CS0618
			ShowProgressAndShoot(checkResult,() => ODProgress.ShowExtended(null,tag:"Window checks (WINCHK-5): an extended progress window"),
				"FormProgressExtended","C17-extended.png");
		}

		public static void CheckC18(Ui ui,CheckResult checkResult) {
			//The drop-down of OpenPmsDental.UI.ComboBox (FormComboPicker, a plain form), which the clinic picker (ComboBoxClinicPicker) and every
			//UI.ComboBox open on a mouse-down; opened by the combo box's own OnMouseDown on the Edit Patient Information window.
			GoToModule(ui,EnumModuleType.Family);
			Control controlFamily=ModuleControl("controlFamily");
			Ui.Opened opened=ui.Open("Family > double-click the patient information",() => {
				Control gridPat=(Control)Ui.GetField(controlFamily,"gridPat");
				DoubleClickRowOnThread(gridPat,0);
			});
			if(!InspectWindow(ui,checkResult,opened,"FormPatientEdit",null,close:false)) {
				return;
			}
			IntPtr hwndPatient=opened.Hwnd;
			string comboName=ui.Invoke(() => Ui.AllControls(Control.FromHandle(hwndPatient)).OfType<OpenPmsDental.UI.ComboBox>()
				.FirstOrDefault(x => x.Visible && x.Enabled && x.Items.Count>1)?.Name??"");
			if(checkResult.Expect(comboName!="","Edit Patient Information has a UI.ComboBox with items")) {
				Ui.Opened openedPicker=ui.Open("Mouse-down on the combo box "+comboName,() => {
					Control combo=Ui.AllControls(Control.FromHandle(hwndPatient)).First(x => x.Name==comboName);
					Ui.Call(combo,"OnMouseDown",new MouseEventArgs(MouseButtons.Left,1,combo.Width-8,combo.Height/2,0));
				});
				if(InspectWindow(ui,checkResult,openedPicker,"FormComboPicker",null,close:false)) {
					//Control.FromHandle on the window's thread (ui.GetForm would invoke onto the thread it is already on).
					checkResult.Note("The drop-down of "+comboName+": "+string.Join(", ",ui.Invoke(() => ((List<string>)Ui.GetField(Control.FromHandle(openedPicker.Hwnd),
						"ListStrings")).Take(8))));
					checkResult.AddShot(ui.Shot(openedPicker.Hwnd,"C18-picker.png"),"The combo box drop-down (FormComboPicker)");
					checkResult.AddShot(ui.Shot(hwndPatient,"C18.png"),"Edit Patient Information with the drop-down open");
					ui.Invoke(() => ((Form)Control.FromHandle(openedPicker.Hwnd)).Close());
					checkResult.Expect(ui.WaitGone(openedPicker.Hwnd),"The drop-down closes");
				}
			}
			checkResult.Expect(ui.Close(hwndPatient),"Edit Patient Information closes (Cancel)");
		}

		public static void CheckC19(Ui ui,CheckResult checkResult) {
			//ToolTipOD (a plain form) shows a text below the mouse pointer for the control it serves; shown here for the main window with the
			//main window's font, as a grid's or the module bar's hover shows it.
			IntPtr hwnd=ui.Invoke(() => {
				OpenPmsDental.UI.ToolTipOD toolTipOD=new OpenPmsDental.UI.ToolTipOD();
				toolTipOD.SetString(ui.FormMain,"Window checks (WINCHK-5): ToolTipOD",ui.FormMain.Font);
				toolTipOD.Refresh();
				return toolTipOD.Handle;
			});
			System.Threading.Thread.Sleep(500);
			bool isVisible=ui.Invoke(() => Control.FromHandle(hwnd).Visible);
			checkResult.Expect(isVisible,"ToolTipOD shows the text");
			string font=ui.Invoke(() => Control.FromHandle(hwnd).Font.Name+" "+Control.FromHandle(hwnd).Font.SizeInPoints.ToString("0.##")+" pt");
			checkResult.Note("ToolTipOD's font (the main window's): "+font);
			AddTextShot(checkResult,ui.Shot(hwnd,"C19.png"),"ToolTipOD");
			ui.Invoke(() => ((Form)Control.FromHandle(hwnd)).Close());
			checkResult.Expect(ui.WaitGone(hwnd),"ToolTipOD closes");
		}

		public static void CheckC20(Ui ui,CheckResult checkResult) {
			//The main menu (MenuOD and the MenuStripOD inside it) draws in its form's font, Microsoft Sans Serif 8.25, as on .NET Framework. The
			//pairs of WINCHK-5's first run showed every main menu in Segoe UI 9 (ToolStrip's own default) on .NET 10: MenuOD.OnParentChanged.
			string[] arrayFonts=ui.Invoke(() => {
				OpenPmsDental.UI.MenuOD menuOD=(OpenPmsDental.UI.MenuOD)Ui.GetField(ui.FormMain,"menuMain");
				ToolStripItem toolStripItem=menuOD.GetMenuItems().Cast<ToolStripItem>().First(x => x.Available);
				return new[] { ui.FormMain.Font.Name+" "+ui.FormMain.Font.SizeInPoints.ToString("0.##"),menuOD.Font.Name+" "+menuOD.Font.SizeInPoints.ToString("0.##"),
					toolStripItem.Owner.Font.Name+" "+toolStripItem.Owner.Font.SizeInPoints.ToString("0.##"),
					toolStripItem.Font.Name+" "+toolStripItem.Font.SizeInPoints.ToString("0.##"),toolStripItem.Text };
			});
			checkResult.Note("Fonts: the main window "+arrayFonts[0]+", MenuOD "+arrayFonts[1]+", its strip "+arrayFonts[2]+", the item '"+arrayFonts[4]+"' "
				+arrayFonts[3]);
			checkResult.Expect(arrayFonts[1].StartsWith("Microsoft Sans Serif "),"MenuOD has the window's font, Microsoft Sans Serif ("+arrayFonts[1]+")",arrayFonts[1]);
			checkResult.Expect(arrayFonts[2]==arrayFonts[1] && arrayFonts[3]==arrayFonts[1],"The menu strip and its items draw in MenuOD's font ("+arrayFonts[2]+", "
				+arrayFonts[3]+"), not ToolStrip's Segoe UI 9",arrayFonts[2]+" / "+arrayFonts[3]);
			Control controlMenu=ui.Invoke(() => (Control)Ui.GetField(ui.FormMain,"menuMain"));
			AddTextShot(checkResult,ui.ShotDraw(controlMenu,"C20-menu.png"),"The main menu bar");
		}

		public static void CheckF5(Ui ui,CheckResult checkResult) {
			IntPtr hwnd=OpenDashboard(ui,checkResult);
			if(hwnd==IntPtr.Zero) {
				return;
			}
			//Each graph cell's docked graph (GraphIncome, GraphQuantityOverTime and its filter: System.Windows.Forms.DataVisualization's Chart),
			//drawn by its own painting at its size.
			List<Control> listGraphs=ui.Invoke(() => Ui.AllControls(Control.FromHandle(hwnd)).Where(x => x.GetType().Name=="DashboardCellCtrl" && x.Visible
				&& (bool)Member(x,"HasDockedControl")).Select(x => (Control)Member(x,"DockedControl")??x.Controls.Cast<Control>().FirstOrDefault())
				.Where(x => x!=null).ToList());
			checkResult.Expect(listGraphs.Count>0,"The dashboard has graph cells ("+listGraphs.Count+")");
			//The graphs fill on a thread of their own and show a 'Loading' picture until then (GraphQuantityOverTime.IsLoading).
			System.Diagnostics.Stopwatch stopwatch=System.Diagnostics.Stopwatch.StartNew();
			int countLoading=int.MaxValue;
			while(stopwatch.Elapsed.TotalSeconds<60) {
				countLoading=ui.Invoke(() => listGraphs.SelectMany(x => new[] { x }.Concat(Ui.AllControls(x)))
					.Count(x => x.GetType().GetProperty("IsLoading")?.GetValue(x) is bool isLoading && isLoading));
				if(countLoading==0) {
					break;
				}
				System.Threading.Thread.Sleep(500);
			}
			checkResult.Expect(countLoading==0,"Every graph has loaded its data ("+stopwatch.Elapsed.TotalSeconds.ToString("0.0")+" s)",countLoading+" still loading");
			for(int i=0;i<listGraphs.Count && i<6;i++) {
				Control control=listGraphs[i];
				string description=ui.Invoke(() => {
					List<string> listCharts=Ui.AllControls(control).Where(x => x.GetType().FullName=="System.Windows.Forms.DataVisualization.Charting.Chart")
						.Select(x => x.Name+" ("+x.Width+"x"+x.Height+")").ToList();
					return control.GetType().Name+": charts "+(listCharts.Count==0 ? "none" : string.Join(", ",listCharts));
				});
				checkResult.Note("Graph "+(i+1)+": "+description);
				//Printed (PrintWindow), not DrawToBitmap: DrawToBitmap draws the children in the wrong order, so the 'Loading' picture behind the
				//chart came out on top of it.
				IntPtr hwndGraph=ui.Invoke(() => control.Handle);
				checkResult.AddShot(ui.Shot(hwndGraph,"F5-"+(i+1)+".png"),"Graph "+(i+1)+" ("+description+")");
			}
			checkResult.ForPerson("the graphs' axes, legends and fonts in F5-*.png (the Windows Forms charting package of .NET 10 draws them; a check new "
				+"in WINCHK-5, so the .NET Framework pictures to compare with are the same graphs in F1.png)");
			CloseDashboard(ui,checkResult,hwnd);
		}

		public static void CheckG3(Ui ui,CheckResult checkResult) {
			//Manage > Time Cards (FormTimeCardManage) > Print All, which only previews (PrinterL.TryPreview: FormPrintPreview; printing would be its
			//Print button, never pressed). The seed has no employee and no pay period: made by the business layer, the period around today so the
			//window lists the employee, and deleted again at the end.
			Employee employee=new Employee { LName="Gatetest",FName="Preview",MiddleI="",ClockStatus="",PhoneExt=0,PayrollID="",WirelessPhone="",EmailWork="",
				EmailPersonal="" };
			PayPeriod payPeriod=new PayPeriod { DateStart=DateTime.Today.AddDays(-6),DateStop=DateTime.Today.AddDays(7),DatePaycheck=DateTime.Today.AddDays(8) };
			ui.Invoke(() => {
				Employees.Insert(employee);
				PayPeriods.Insert(payPeriod);
				DataValid.SetInvalid(InvalidType.Employees);
				Cache.Refresh(InvalidType.Employees);
			});
			checkResult.Note("Employee "+employee.EmployeeNum+" and pay period "+payPeriod.PayPeriodNum+" made by the business layer");
			try {
				GoToModule(ui,EnumModuleType.Appointments);
				GoToModule(ui,EnumModuleType.Manage);
				Control controlManage=ModuleControl("controlManage");
				Ui.Opened opened=ui.Open("Manage > Time Cards",() => Ui.Call(controlManage,"butManage_Click",controlManage,EventArgs.Empty));
				if(InspectWindow(ui,checkResult,opened,"FormTimeCardManage",null,close:false)) {
					IntPtr hwndManage=opened.Hwnd;
					checkResult.AddShot(ui.Shot(hwndManage,"G3-timecards.png"),"Time Card Manage");
					Form formManage=ui.GetForm(hwndManage);
					Ui.Opened openedPreview=ui.Open("Print All",() => Ui.Call(formManage,"butPrintAll_Click",formManage,EventArgs.Empty),
						allowMessage:true);
					if(openedPreview.IsOpen && Native.IsDialogClass(openedPreview.Hwnd)) {
						//No printer to preview for (the printout's settings need one): what the program said is for a person.
						checkResult.ForPerson("the time cards' print preview: the program said "+ui.DescribeWindow(openedPreview.Hwnd));
						ui.Close(openedPreview.Hwnd);
					}
					else if(InspectWindow(ui,checkResult,openedPreview,"FormPrintPreview",null,close:false)) {
						System.Threading.Thread.Sleep(2000);//the preview control draws its pages in the background
						string pages=ui.Invoke(() => {
							PrintPreviewControl printPreviewControl=Ui.AllControls(Control.FromHandle(openedPreview.Hwnd)).OfType<PrintPreviewControl>().FirstOrDefault();
							return printPreviewControl==null ? "no PrintPreviewControl" : "page "+(printPreviewControl.StartPage+1)+", document "
								+printPreviewControl.Document?.DocumentName;
						});
						checkResult.Note("The preview: "+pages);
						checkResult.AddShot(ui.Shot(openedPreview.Hwnd,"G3.png"),"The time cards' print preview (FormPrintPreview)");
						checkResult.ForPerson("the page in G3.png (margins, fonts, the time card's grid; a check new in WINCHK-5, with no .NET Framework picture)");
						checkResult.Expect(ui.Close(openedPreview.Hwnd),"The print preview closes (Print never pressed)");
					}
					openedPreview.Posted?.Wait();
					checkResult.Expect(ui.Close(hwndManage),"Time Card Manage closes");
				}
				opened.Posted?.Wait();
			}
			finally {
				ui.Invoke(() => {
					PayPeriods.Delete(payPeriod);
					Employees.Delete(employee.EmployeeNum);
					DataValid.SetInvalid(InvalidType.Employees);
					Cache.Refresh(InvalidType.Employees);
				});
			}
			checkResult.Expect(Count("SELECT COUNT(*) FROM employee")==0 && Count("SELECT COUNT(*) FROM payperiod")==0,"Undo: the employee and the pay period are gone");
		}
	}
}
