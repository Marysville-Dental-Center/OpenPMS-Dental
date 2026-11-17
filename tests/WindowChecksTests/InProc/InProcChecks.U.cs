// Group U's in-process scripts (package UI-0c of docs/specs/ui-refresh-plan.md; docs/port-notes/UI-0c.md): the common windows of
// UiGallery\windows.tsv, the main window at fixed sizes with each module, with and without a patient, and the patient dashboard with a widget.
// Each window is opened the way a person's click opens it (the main menu item's PerformClick, the module's own handler, a module button's
// PerformClick, a double-click on a module grid's row), measured by the walker after its Load (UiWalker: UI-0b's shared measuring files),
// printed under the row's picture name, and closed as Cancel does; nothing is saved, and the tables the row names keep their row counts. A
// message box instead of the window fails the check with its text. No input is sent and no UAC prompt is answered.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using UiRefreshTests.TextFit;

namespace WindowChecksTests.InProc {
	public static partial class InProcChecks {
		///<summary>Windows a script opened on the way to its window (a list before its edit window), closed after the window, last first.</summary>
		private static readonly List<IntPtr> _listUParents=new List<IntPtr>();
		///<summary>The day the Appointments module is paged to (B1's day, with the synthetic appointments), so its pictures do not move with the date.
		///</summary>
		private static readonly DateTime _dateApptsU=new DateTime(2026,11,9);

		///<summary>Runs a group U check (U<n>, UM<n>, UD<n>) from its row of windows.tsv. False for another id.</summary>
		public static bool TryRunGroupU(string id,Ui ui,CheckResult checkResult) {
			if(!Regex.IsMatch(id,@"^U(M|D)?\d+$")) {
				return false;
			}
			UiWindowRow row=UiWindows.Get(id);
			if(row==null) {
				throw new Exception("No row "+id+" in "+UiWindows.PathTsv);
			}
			switch(row.Opened) {
				case UiWindowRow.OPENED_NONE:
					checkResult.StatusForced=CheckStatus.Skipped;
					checkResult.Reason="Not filled by the synthetic dump: "+row.Target+".";
					return true;
				case UiWindowRow.OPENED_MAIN:
					CheckUMain(ui,checkResult,row);
					return true;
				case UiWindowRow.OPENED_DASHBOARD:
					CheckUDashboard(ui,checkResult,row);
					return true;
				default:
					CheckUWindow(ui,checkResult,row);
					return true;
			}
		}

		#region Windows
		private static void CheckUWindow(Ui ui,CheckResult checkResult,UiWindowRow row) {
			if(row.HasPatient) {
				SelectPatientU(ui,checkResult,true);
			}
			if(row.Module!="") {
				GoToModule(ui,(EnumModuleType)Enum.Parse(typeof(EnumModuleType),row.Module));
			}
			Dictionary<string,int> dictCountsBefore=GuardCounts(row);
			_listUParents.Clear();
			Ui.Opened opened=OpenU(ui,checkResult,row);
			try {
				if(opened.IsOpen) {
					string typeName=ui.GetWindowTypeName(opened.Hwnd);
					checkResult.Expect(typeName==row.Window || typeName.EndsWith("/"+row.Window),"The window "+row.Window+" opens ("+ui.DescribeWindow(opened.Hwnd)
						+")",typeName);
					//Let it finish its Load and the work it posts there before it is measured and printed.
					Thread.Sleep(800);
					WalkAndShoot(ui,checkResult,row,opened.Hwnd,isClosedAfter:true);
					checkResult.Expect(ui.Close(opened.Hwnd),row.Window+" closes (Cancel)");
				}
			}
			finally {
				for(int i=_listUParents.Count-1;i>=0;i--) {
					if(Native.IsWindow(_listUParents[i])) {
						checkResult.Expect(ui.Close(_listUParents[i]),"The window it was opened from closes (Cancel)");
					}
				}
				_listUParents.Clear();
			}
			if(opened.Posted!=null && !opened.Posted.Wait(30000)) {
				checkResult.Fail("The handler that opened "+row.Window+" did not return within 30 s of its window closing");
			}
			ui.CloseStrayWindows(checkResult);
			CompareGuardCounts(checkResult,row,dictCountsBefore);
		}

		///<summary>Opens the row's window the way the program does for a person's click.</summary>
		private static Ui.Opened OpenU(Ui ui,CheckResult checkResult,UiWindowRow row) {
			switch(row.Opened) {
				case UiWindowRow.OPENED_MENU:
					return OpenMenu(ui,row.MenuPath);
				case UiWindowRow.OPENED_HANDLER:
					return ui.Open("Handler "+row.Target,() => {
						Control control=MainField<Control>(row.ModuleField);
						if(control==null) {
							throw new Exception("The main window has no "+row.ModuleField);
						}
						MethodInfo methodInfo=FindMethod(control.GetType(),row.Member);
						object[] arrayArgs=methodInfo.GetParameters().Length==0 ? new object[0] : new object[] { control,EventArgs.Empty };
						InvokeOnThread(control,methodInfo,arrayArgs);
					});
				case UiWindowRow.OPENED_BUTTON:
					return ui.Open("Button "+row.Target,() => {
						Control control=MainField<Control>(row.ModuleField);
						Control button=control==null ? null : Ui.GetField(control,row.Member) as Control;
						if(button==null) {
							throw new Exception("No button "+row.Target);
						}
						if(!button.Visible || !button.Enabled) {
							throw new Exception("The button "+row.Target+" is not available (visible "+button.Visible+", enabled "+button.Enabled+")");
						}
						if(button is IButtonControl buttonControl) {
							buttonControl.PerformClick();
						}
						else {
							Ui.Call(button,"OnClick",EventArgs.Empty);
						}
					});
				case UiWindowRow.OPENED_GRID:
					return ui.Open("Double-click "+row.Target,() => {
						Control control=MainField<Control>(row.ModuleField);
						Control grid=control==null ? null : Ui.GetField(control,row.Member) as Control;
						if(grid==null) {
							throw new Exception("No grid "+row.Target);
						}
						int index=FindGridRowU(grid,row.GridMatch);
						if(index<0) {
							throw new Exception("No row of "+row.Member+" matches '"+row.GridMatch+"' (rows: "+string.Join("; ",GridRows(grid).Take(12))+")");
						}
						DoubleClickRowOnThread(grid,index);
					});
				case UiWindowRow.OPENED_ACCOUNTROW:
					return ui.Open("Account grid row with "+row.Target,() => {
						Control controlAccount=MainField<Control>("controlAccount");
						DataSet dataSet=(DataSet)Ui.GetField(controlAccount,"_dataSetMain");
						DataTable table=dataSet?.Tables["account"];
						int index=-1;
						for(int i=0;table!=null && i<table.Rows.Count;i++) {
							if(table.Columns.Contains(row.Target) && table.Rows[i][row.Target].ToString()!="0") {
								index=i;
								break;
							}
						}
						if(index<0) {
							throw new Exception("No row of the Account grid has a "+row.Target);
						}
						DoubleClickRowOnThread((Control)Ui.GetField(controlAccount,"gridAccount"),index);
					});
				case UiWindowRow.OPENED_SCRIPT:
					MethodInfo methodScript=typeof(InProcChecks).GetMethod("ScriptU"+row.Target,BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public);
					if(methodScript==null) {
						throw new MissingMethodException("No script ScriptU"+row.Target);
					}
					try {
						return (Ui.Opened)methodScript.Invoke(null,new object[] { ui,checkResult });
					}
					catch(TargetInvocationException ex) {
						System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
						throw;
					}
				default:
					throw new ArgumentException("No way to open '"+row.Opened+"'");
			}
		}

		///<summary>The instance method of the type (any visibility, up the class chain) by name; the module handlers take no arguments or (sender, e).
		///</summary>
		public static MethodInfo FindMethod(Type type,string name) {
			for(Type t=type;t!=null;t=t.BaseType) {
				MethodInfo methodInfo=t.GetMethods(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly)
					.FirstOrDefault(x => x.Name==name && (x.GetParameters().Length==0 || x.GetParameters().Length==2));
				if(methodInfo!=null) {
					return methodInfo;
				}
			}
			throw new MissingMethodException(type.Name,name);
		}

		private static void InvokeOnThread(object obj,MethodInfo methodInfo,object[] arrayArgs) {
			try {
				methodInfo.Invoke(obj,arrayArgs);
			}
			catch(TargetInvocationException ex) {
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}
		}

		///<summary>The first row of the grid whose Tag (as text) is one of "tag:a;b;c", or whose cells (joined with |) hold "text:...", or -1.</summary>
		public static int FindGridRowU(Control grid,string match) {
			if(match.StartsWith("tag:")) {
				List<string> listTags=match.Substring(4).Split(';').Select(x => x.Trim()).Where(x => x!="").ToList();
				System.Collections.IList listRows=(System.Collections.IList)Member(grid,"ListGridRows");
				foreach(string tag in listTags) {
					for(int i=0;listRows!=null && i<listRows.Count;i++) {
						if((Member(listRows[i],"Tag")?.ToString()??"")==tag) {
							return i;
						}
					}
				}
				return -1;
			}
			if(match.StartsWith("text:")) {
				string text=match.Substring(5).Trim();
				return FindRow(grid,x => x.IndexOf(text,StringComparison.OrdinalIgnoreCase)>=0);
			}
			throw new ArgumentException("A grid row is matched by tag: or text:, not '"+match+"'");
		}

		///<summary>Lists > Procedure Codes, then a double-click on D0120's row (its code column): the procedure code's edit window. The list is closed
		///after it.</summary>
		public static Ui.Opened ScriptUProcCodeEdit(Ui ui,CheckResult checkResult) {
			Ui.Opened openedList=OpenMenu(ui,"Lists","Procedure Codes");
			if(!openedList.IsOpen) {
				return openedList;
			}
			_listUParents.Add(openedList.Hwnd);
			checkResult.Expect(ui.GetWindowTypeName(openedList.Hwnd)=="FormProcCodes","Lists > Procedure Codes opens FormProcCodes");
			Thread.Sleep(800);
			return ui.Open("Procedure Codes > double-click D0120",() => {
				Control grid=FindControl(ui,openedList.Hwnd,"gridMain",isOnThread:true);
				System.Collections.IList listRows=(System.Collections.IList)Member(grid,"ListGridRows");
				int index=-1;
				for(int i=0;i<listRows.Count;i++) {
					if(Member(listRows[i],"Tag") is ProcedureCode procedureCode && procedureCode.ProcCode=="D0120") {
						index=i;
						break;
					}
				}
				if(index<0) {
					throw new Exception("D0120 is not listed in Procedure Codes");
				}
				DoubleClickRowOnThread(grid,index,0);
			});
		}
		#endregion Windows

		#region Walker and picture
		///<summary>The walker after Load (today's font), the picture, then (for a window the check closes, with OPMS_WINDOW_CHECKS_UI_FONT) the font
		///option measured after the swap; the walker's files are written and its summary noted.</summary>
		private static void WalkAndShoot(Ui ui,CheckResult checkResult,UiWindowRow row,IntPtr hwnd,bool isClosedAfter) {
			Rectangle rectangleWindow=Native.GetRect(hwnd);
			DateTime dateTimeNow=DateTime.Now;
			UiWalkResult uiWalkResult=null;
			try {
				uiWalkResult=ui.Invoke(() => {
					Control root=Control.FromHandle(hwnd);
					return root==null ? null : UiWalker.Walk(root,row.WindowKey,row.Picture,rectangleWindow,dateTimeNow);
				});
			}
			catch(Exception ex) {
				checkResult.Fail("The walker failed on "+row.Window+": "+ex.GetBaseException().GetType().Name+": "+ex.GetBaseException().Message);
			}
			checkResult.AddShot(ui.Shot(hwnd,row.Picture),row.Title);
			if(uiWalkResult==null) {
				return;
			}
			TextFitFontOption fontOption=UiWalker.GetFontOption();
			if(fontOption!=null && isClosedAfter) {
				try {
					ui.Invoke(() => UiWalker.WalkOption(Control.FromHandle(hwnd),uiWalkResult,fontOption,
						UiWalker.ReadFitFixKeys(row.WindowKey,uiWalkResult.WindowType)));
				}
				catch(Exception ex) {
					uiWalkResult.FontOptionSkipped="the swap failed: "+ex.GetBaseException().GetType().Name+": "+ex.GetBaseException().Message;
				}
			}
			else if(fontOption!=null) {
				uiWalkResult.FontOptionSkipped="the main window stays open for the next checks, so its fonts are not swapped";
			}
			UiWalker.Write(uiWalkResult,UiWalker.FolderOut);
			checkResult.Verified("The walker measured "+row.Window+" after its Load and recorded its bounds (ui-walker\\bounds-record.tsv)");
			checkResult.Note(uiWalkResult.Summary);
		}
		#endregion Walker and picture

		#region Guard counts
		private static Dictionary<string,int> GuardCounts(UiWindowRow row) {
			Dictionary<string,int> dictCounts=new Dictionary<string,int>();
			foreach(string table in row.ListGuardTables) {
				dictCounts[table]=Count("SELECT COUNT(*) FROM "+table);
			}
			return dictCounts;
		}

		private static void CompareGuardCounts(CheckResult checkResult,UiWindowRow row,Dictionary<string,int> dictCountsBefore) {
			if(dictCountsBefore.Count==0) {
				return;
			}
			Dictionary<string,int> dictCountsAfter=GuardCounts(row);
			List<string> listChanged=dictCountsBefore.Where(x => dictCountsAfter[x.Key]!=x.Value).Select(x => x.Key+" "+x.Value+" -> "+dictCountsAfter[x.Key]).ToList();
			checkResult.Expect(listChanged.Count==0,"Nothing saved: the row counts of "+string.Join(", ",dictCountsBefore.Select(x => x.Key+" ("+x.Value+")"))
				+" are unchanged",string.Join(", ",listChanged));
		}
		#endregion Guard counts

		#region The main window
		///<summary>Patient #1 selected (as the patient selection does), or no patient (as the program's start without one).</summary>
		private static void SelectPatientU(Ui ui,CheckResult checkResult,bool hasPatient) {
			ui.Invoke(() => {
				if(hasPatient) {
					if(FormOpenDental.PatNumCur!=1) {
						GlobalFormOpenDental.PatientSelected(Patients.GetPat(1),true);
					}
					return;
				}
				if(FormOpenDental.PatNumCur!=0) {
					FormOpenDental.PatNumCur=0;
					Ui.Call(FormMain,"RefreshCurrentModule",false,true,true,0L);
					Ui.Call(FormMain,"FillPatientButton",(object)null);
				}
			});
			Thread.Sleep(500);
			long patNum=ui.Invoke(() => FormOpenDental.PatNumCur);
			checkResult.Expect(patNum==(hasPatient ? 1 : 0),hasPatient ? "Patient #1 is selected" : "No patient is selected","PatNumCur "+patNum);
		}

		///<summary>The main window at the row's size (restored from maximized), in the module, with or without patient #1; the Appointments module paged
		///to B1's day. Measured by the walker and printed; the window's size, state and patient are put back afterwards.</summary>
		private static void CheckUMain(Ui ui,CheckResult checkResult,UiWindowRow row) {
			IntPtr hwndMain=ui.Invoke(() => FormMain.Handle);
			FormWindowState formWindowStateOld=ui.Invoke(() => FormMain.WindowState);
			Rectangle rectangleOld=ui.Invoke(() => FormMain.Bounds);
			try {
				SelectPatientU(ui,checkResult,row.HasPatient);
				GoToModule(ui,(EnumModuleType)Enum.Parse(typeof(EnumModuleType),row.Module));
				checkResult.Expect(GetModule(ui).ToString()==row.Module,"The module button opens "+row.Module);
				SetMainSize(ui,checkResult,row.MainSize);
				if(row.Module=="Appointments") {
					PageApptsTo(ui,checkResult,_dateApptsU);
				}
				Thread.Sleep(1000);
				WalkAndShoot(ui,checkResult,row,hwndMain,isClosedAfter:false);
			}
			finally {
				ui.Invoke(() => {
					FormMain.Bounds=rectangleOld;
					FormMain.WindowState=formWindowStateOld;
				});
				if(!row.HasPatient) {
					SelectPatientU(ui,checkResult,true);
				}
			}
		}

		private static void SetMainSize(Ui ui,CheckResult checkResult,Size size) {
			Size sizeNow=ui.Invoke(() => {
				FormMain.WindowState=FormWindowState.Normal;
				FormMain.Bounds=new Rectangle(0,0,size.Width,size.Height);
				return FormMain.Size;
			});
			Thread.Sleep(1000);
			Rectangle rectangle=Native.GetRect(ui.Invoke(() => FormMain.Handle));
			Rectangle rectangleScreen=ui.Invoke(() => System.Windows.Forms.Screen.FromHandle(FormMain.Handle).Bounds);
			if(rectangle.Size==size) {
				checkResult.Verified("The main window is "+size.Width+"x"+size.Height+" (restored, not maximized; the screen is "+rectangleScreen.Width+"x"
					+rectangleScreen.Height+")");
			}
			else {
				checkResult.ForPerson("the main window should be "+size.Width+"x"+size.Height+" but is "+rectangle.Width+"x"+rectangle.Height
					+" (WinForms said "+sizeNow.Width+"x"+sizeNow.Height+"; the screen is "+rectangleScreen.Width+"x"+rectangleScreen.Height
					+", and Windows keeps a window within what the screen allows)");
			}
		}

		///<summary>The Appointments module's day arrows clicked until the day shown is the date (as B1 pages there).</summary>
		private static void PageApptsTo(Ui ui,CheckResult checkResult,DateTime dateWanted) {
			Control controlAppt=ModuleControl("controlAppt");
			ControlApptPanel controlApptPanel=(ControlApptPanel)Ui.GetField(controlAppt,"contrApptPanel");
			if(ui.Invoke(() => controlApptPanel.IsWeeklyView)) {
				ui.Invoke(() => Ui.Call(controlAppt,"toggleDayWeek_DayClick",controlAppt,EventArgs.Empty));
			}
			DateTime dateSelected=ui.Invoke(() => controlApptPanel.DateSelected);
			int countClicks=0;
			while(dateSelected.Date!=dateWanted && countClicks<400) {
				string handler=dateSelected.Date>dateWanted ? "butBack_Click" : "butFwd_Click";
				ui.Invoke(() => Ui.Call(controlAppt,handler,controlAppt,EventArgs.Empty));
				dateSelected=ui.Invoke(() => controlApptPanel.DateSelected);
				countClicks++;
			}
			checkResult.Expect(dateSelected.Date==dateWanted,"The day view shows "+dateWanted.ToString("yyyy-MM-dd")+" ("+countClicks+" click(s) of the day arrows)",
				dateSelected.ToString("yyyy-MM-dd"));
		}
		#endregion The main window

		#region The patient dashboard
		///<summary>The patient dashboard with a widget sheet definition: the copy's own widget when it has one, else a synthetic one added to the
		///scratch copy (the internal Patient Dashboard Default with two labels, one with a font name and one without), with the Dashboard Widget
		///permission for Admin's group when the group does not have it. Opened through Tools > Patient Dashboards (its menu item's PerformClick), the
		///main window measured and printed at the row's size, the dashboard closed again (the item's second click), and every row this check added
		///deleted, by its key.</summary>
		private static void CheckUDashboard(Ui ui,CheckResult checkResult,UiWindowRow row) {
			Dictionary<string,int> dictCountsBefore=GuardCounts(row);
			FormWindowState formWindowStateOld=ui.Invoke(() => FormMain.WindowState);
			Rectangle rectangleOld=ui.Invoke(() => FormMain.Bounds);
			long sheetDefNumAdded=0;
			GroupPermission groupPermissionAdded=null;
			try {
				SelectPatientU(ui,checkResult,true);
				GoToModule(ui,(EnumModuleType)Enum.Parse(typeof(EnumModuleType),row.Module));
				int countWidgets=Count("SELECT COUNT(*) FROM sheetdef WHERE SheetType="+(int)SheetTypeEnum.PatientDashboardWidget);
				//Always its own synthetic widget, whatever the copy holds, so the picture does not depend on run order (a widget another check made,
				//such as group I's I5 copy, is not used).
				if(countWidgets>0) {
					checkResult.Note("The copy has "+countWidgets+" widget sheet definition(s); UD1 adds and shows its own synthetic one");
				}
				SheetDef sheetDefWidget;
				{
					sheetDefWidget=SheetsInternal.GetSheetDef(SheetInternalType.PatientDashboard).Copy();
					sheetDefWidget.IsNew=true;
					sheetDefWidget.Description="UI-0c synthetic widget";
					sheetDefWidget.SheetFieldDefs.Add(SheetFieldDef.NewStaticText("Synthetic label, no font name",8.25f,"",false,10,5,200,14));
					sheetDefWidget.SheetFieldDefs.Add(SheetFieldDef.NewStaticText("Synthetic label, Microsoft Sans Serif",8.25f,"Microsoft Sans Serif",false,
						10,20,220,14));
					long sheetDefNumBefore=MaxKey("sheetdef","SheetDefNum");
					SheetDefs.InsertOrUpdate(sheetDefWidget);
					sheetDefNumAdded=AddedKey("sheetdef","SheetDefNum",sheetDefNumBefore,"SheetType="+(int)SheetTypeEnum.PatientDashboardWidget);
					checkResult.Expect(sheetDefNumAdded>0 && sheetDefNumAdded==sheetDefWidget.SheetDefNum,"A synthetic widget sheet definition is "
						+"added (SheetDefNum "+sheetDefNumAdded+", "+sheetDefWidget.SheetFieldDefs.Count+" fields; deleted at the end of this check)");
					if(sheetDefNumAdded<=0 && sheetDefWidget.SheetDefNum>sheetDefNumBefore) {
						sheetDefNumAdded=sheetDefWidget.SheetDefNum;//the row InsertOrUpdate made, so the finally deletes it
					}
					SetKey(checkResult,"UD1.SheetDefNumWidget",sheetDefNumAdded);
				}
				SheetDefs.RefreshCache();
				SheetFieldDefs.RefreshCache();
				if(!Security.IsAuthorized(EnumPermType.DashboardWidget,sheetDefWidget.SheetDefNum,true)) {
					groupPermissionAdded=new GroupPermission {
						PermType=EnumPermType.DashboardWidget,
						FKey=sheetDefWidget.SheetDefNum,
						UserGroupNum=UserGroups.GetForUser(Security.CurUser.UserNum,false).First().UserGroupNum,
						NewerDate=DateTime.MinValue,
					};
					GroupPermissions.Insert(groupPermissionAdded);
					GroupPermissions.RefreshCache();
					checkResult.Note("Admin's group gets the Dashboard Widget permission for it (GroupPermNum "+groupPermissionAdded.GroupPermNum+"; deleted at the end)");
				}
				ui.Invoke(() => Ui.Call(FormMain,"RefreshMenuDashboards"));
				long sheetDefNumWidget=sheetDefWidget.SheetDefNum;
				ToolStripMenuItem menuItem=ui.Invoke(() => GetMenuItem("Tools","Patient Dashboards").DropDownItems.OfType<ToolStripMenuItem>()
					.FirstOrDefault(x => x.Tag is SheetDef sheetDef && sheetDef.SheetDefNum==sheetDefNumWidget));
				if(!checkResult.Expect(menuItem!=null,"Tools > Patient Dashboards lists the widget")) {
					return;
				}
				SetMainSize(ui,checkResult,row.MainSize);
				Ui.Opened openedMessage=ui.Open("Patient Dashboards > "+menuItem.Text,() => menuItem.PerformClick(),allowMessage:true,timeoutMs:20000);
				if(openedMessage.IsOpen && Native.IsDialogClass(openedMessage.Hwnd)) {
					checkResult.Note("Opening the widget said: '"+ReadAndCloseMessage(ui,openedMessage)+"'");
				}
				else if(openedMessage.IsOpen) {
					checkResult.Fail("Opening the widget opened "+ui.DescribeWindow(openedMessage.Hwnd));
					ui.Close(openedMessage.Hwnd);
				}
				openedMessage.Posted.Wait();
				checkResult.ListFailures.RemoveAll(x => x.Contains("no window opened"));//the message is the program's only window; none is fine
				Thread.Sleep(2500);
				bool isOpen=ui.Invoke(() => ((System.Collections.ICollection)Member(MainField<Control>("userControlDashboard"),"ListOpenWidgets")).Count>0);
				checkResult.Expect(isOpen,"The dashboard shows the widget");
				string fields=ui.Invoke(() => string.Join(", ",Ui.AllControls(MainField<Control>("userControlDashboard")).Where(x => x.Visible)
					.Select(x => x.GetType().Name).Distinct().OrderBy(x => x)));
				checkResult.Note("The widget's controls: "+fields);
				WalkAndShoot(ui,checkResult,row,ui.Invoke(() => FormMain.Handle),isClosedAfter:false);
				ui.Post("Patient Dashboards > the checked item (close)",() => menuItem.PerformClick()).Wait();
				ui.CloseStrayWindows(checkResult);
				checkResult.Expect(!ui.Invoke(() => ((System.Collections.ICollection)Member(MainField<Control>("userControlDashboard"),"ListOpenWidgets")).Count>0),
					"Clicking the checked item closes the dashboard");
			}
			finally {
				ui.Invoke(() => {
					FormMain.Bounds=rectangleOld;
					FormMain.WindowState=formWindowStateOld;
				});
				if(groupPermissionAdded!=null) {
					GroupPermissions.Delete(groupPermissionAdded);
					GroupPermissions.RefreshCache();
				}
				if(sheetDefNumAdded>0) {
					SheetDefs.DeleteObject(sheetDefNumAdded);
					SheetDefs.RefreshCache();
					SheetFieldDefs.RefreshCache();
					SetKey(checkResult,"UD1.SheetDefNumWidget",0);
				}
				ui.Invoke(() => Ui.Call(FormMain,"RefreshMenuDashboards"));
			}
			CompareGuardCounts(checkResult,row,dictCountsBefore);
		}
		#endregion The patient dashboard
	}
}
