// UI-CAL (docs/port-notes/UI-CAL.md): the behaviour the windows had from Windows' calendars, now that MonthCalendarOD replaces them and is the
// stock date pickers' drop-down (UiDateDropLook): the selection and its range, MinDate, the events (DateChanged on each change by the user,
// DateSelected on the mouse button's release, none for the program's changes), the keyboard, and the picker's drop-down (its date, its check
// box, DropDown, ValueChanged and CloseUp in Windows' order). Off screen; nothing is shown (the drop-down is made but not shown,
// UiDateDropLook.IsShowSkippedForTest); no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;

namespace UiRefreshTests {
	///<summary>MonthCalendarOD with its mouse and keyboard handlers open to the tests, and the cell of a day.</summary>
	public class BehaviourCalendar : MonthCalendarOD {
		public List<string> ListEvents=new List<string>();

		public BehaviourCalendar() {
			DateChanged+=(sender,e) => ListEvents.Add("DateChanged "+SelectionStart.ToString("yyyy-MM-dd")+".."+SelectionEnd.ToString("yyyy-MM-dd"));
			DateSelected+=(sender,e) => ListEvents.Add("DateSelected "+e.Start.ToString("yyyy-MM-dd")+".."+e.End.ToString("yyyy-MM-dd"));
		}

		///<summary>Paints the calendar into a bitmap, which fills its cells (the days of the month showing).</summary>
		public void PaintOnce() {
			using(Bitmap bitmap=new Bitmap(Width,Height))
			using(Graphics g=Graphics.FromImage(bitmap)) {
				OnPaint(new PaintEventArgs(g,ClientRectangle));
			}
		}

		///<summary>The centre of the day's cell, painting first; the day must be showing.</summary>
		public Point PointOf(DateTime date) {
			PaintOnce();
			for(int c=0;c<7;c++) {
				for(int r=0;r<6;r++) {
					if(GetDateInCell(c,r)==date.Date) {
						Rectangle rectangle=GetRectangleDay(c,r+1);
						return new Point(rectangle.X+rectangle.Width/2,rectangle.Y+rectangle.Height/2);
					}
				}
			}
			throw new Exception("The day is not showing: "+date.ToShortDateString());
		}

		public void Press(DateTime date) {
			Point point=PointOf(date);
			OnMouseMove(new MouseEventArgs(MouseButtons.None,0,point.X,point.Y,0));
			OnMouseDown(new MouseEventArgs(MouseButtons.Left,1,point.X,point.Y,0));
		}

		public void DragTo(DateTime date) {
			Point point=PointOf(date);
			OnMouseMove(new MouseEventArgs(MouseButtons.Left,0,point.X,point.Y,0));
		}

		public void Release() {
			OnMouseUp(new MouseEventArgs(MouseButtons.Left,1,0,0,0));
		}

		public void Click(DateTime date) {
			Press(date);
			Release();
		}

		public void Key(Keys keyData) {
			if(!IsInputKey(keyData)) {
				return;//the window takes the key (a dialog key), as for Windows' calendar without the keyboard
			}
			OnKeyDown(new KeyEventArgs(keyData));
		}

		public bool IsInput(Keys keyData) {
			return IsInputKey(keyData);
		}
	}

	[TestClass]
	public class CalendarBehaviourTests {
		private static readonly DateTime _date0310=new DateTime(2026,3,10);

		private static BehaviourCalendar MakeCalendar(int maxSelectionCount,bool allowKeyboard) {
			BehaviourCalendar behaviourCalendar=new BehaviourCalendar();
			behaviourCalendar.Size=new Size(227,162);
			behaviourCalendar.MaxSelectionCount=maxSelectionCount;
			behaviourCalendar.AllowKeyboard=allowKeyboard;
			behaviourCalendar.SetDate(_date0310);
			return behaviourCalendar;
		}

		#region MonthCalendarOD: the program's changes
		[TestMethod]
		public void Calendar_SelectionStartAndEnd_AsWindowsCalendar() {
			IconRendererTests.RunSta(() => {
				using BehaviourCalendar behaviourCalendar=MakeCalendar(7,false);
				Assert.AreEqual(1,new MonthCalendarOD().MaxSelectionCount,"one day by default (the existing calendars)");
				Assert.AreEqual(_date0310,behaviourCalendar.SelectionStart);
				Assert.AreEqual(_date0310,behaviourCalendar.SelectionEnd);
				behaviourCalendar.SelectionEnd=new DateTime(2026,3,14);
				Assert.AreEqual(new SelectionRange(_date0310,new DateTime(2026,3,14)).ToString(),behaviourCalendar.SelectionRange.ToString());
				//A start after the end moves the end to it; an end too far from the start moves the start up.
				behaviourCalendar.SelectionStart=new DateTime(2026,3,20);
				Assert.AreEqual(new DateTime(2026,3,20),behaviourCalendar.SelectionEnd);
				behaviourCalendar.SelectionEnd=new DateTime(2026,3,30);
				Assert.AreEqual(new DateTime(2026,3,24),behaviourCalendar.SelectionStart,"7 days at most");
				behaviourCalendar.SetSelectionRange(new DateTime(2026,4,20),new DateTime(2026,4,1));
				Assert.AreEqual(new DateTime(2026,4,1),behaviourCalendar.SelectionStart);
				Assert.AreEqual(new DateTime(2026,4,7),behaviourCalendar.SelectionEnd,"cut to MaxSelectionCount from the earlier day");
				//The time of day is dropped, as Windows' calendar keeps days.
				behaviourCalendar.SelectionStart=new DateTime(2026,5,2,15,30,0);
				Assert.AreEqual(new DateTime(2026,5,2),behaviourCalendar.SelectionStart);
				Assert.AreEqual(0,behaviourCalendar.ListEvents.Count,"no event for the program's changes: "+string.Join("; ",behaviourCalendar.ListEvents));
			});
		}

		[TestMethod]
		public void Calendar_MinDateAndMaxDate_MoveTheSelection_AndRefuseTheDaysOutside() {
			IconRendererTests.RunSta(() => {
				using BehaviourCalendar behaviourCalendar=MakeCalendar(1,false);
				behaviourCalendar.MinDate=new DateTime(2026,3,12);
				Assert.AreEqual(new DateTime(2026,3,12),behaviourCalendar.SelectionStart,"a selection before MinDate moves to it");
				behaviourCalendar.Click(new DateTime(2026,3,11));
				Assert.AreEqual(new DateTime(2026,3,12),behaviourCalendar.SelectionStart,"a day before MinDate is not selectable");
				Assert.AreEqual(0,behaviourCalendar.ListEvents.Count,string.Join("; ",behaviourCalendar.ListEvents));
				behaviourCalendar.MaxDate=new DateTime(2026,3,20);
				behaviourCalendar.SetDate(new DateTime(2026,3,25));
				Assert.AreEqual(new DateTime(2026,3,20),behaviourCalendar.SelectionStart,"SetDate after MaxDate takes MaxDate");
			});
		}
		#endregion MonthCalendarOD: the program's changes

		#region MonthCalendarOD: the mouse
		[TestMethod]
		public void Calendar_Click_RaisesDateChangedOnTheDown_AndDateSelectedOnTheRelease() {
			IconRendererTests.RunSta(() => {
				using BehaviourCalendar behaviourCalendar=MakeCalendar(1,true);
				behaviourCalendar.Press(new DateTime(2026,3,18));
				CollectionAssert.AreEqual(new[] { "DateChanged 2026-03-18..2026-03-18" },behaviourCalendar.ListEvents);
				behaviourCalendar.Release();
				CollectionAssert.AreEqual(new[] { "DateChanged 2026-03-18..2026-03-18","DateSelected 2026-03-18..2026-03-18" },behaviourCalendar.ListEvents);
				Assert.AreEqual(new DateTime(2026,3,18),behaviourCalendar.GetDateSelected());
				//A day of the next month showing in the last row selects it and shows its month.
				behaviourCalendar.ListEvents.Clear();
				behaviourCalendar.Click(new DateTime(2026,4,2));
				Assert.AreEqual(new DateTime(2026,4,2),behaviourCalendar.SelectionStart);
				behaviourCalendar.PointOf(new DateTime(2026,4,30));//April shows
			});
		}

		[TestMethod]
		public void Calendar_Drag_SelectsTheRange_UpToMaxSelectionCount() {
			IconRendererTests.RunSta(() => {
				using BehaviourCalendar behaviourCalendar=MakeCalendar(7,true);
				behaviourCalendar.Press(new DateTime(2026,3,10));
				behaviourCalendar.DragTo(new DateTime(2026,3,13));
				Assert.AreEqual(new DateTime(2026,3,10),behaviourCalendar.SelectionStart);
				Assert.AreEqual(new DateTime(2026,3,13),behaviourCalendar.SelectionEnd);
				behaviourCalendar.DragTo(new DateTime(2026,3,25));
				Assert.AreEqual(new DateTime(2026,3,16),behaviourCalendar.SelectionEnd,"7 days at most");
				behaviourCalendar.DragTo(new DateTime(2026,3,8));
				Assert.AreEqual(new DateTime(2026,3,8),behaviourCalendar.SelectionStart,"a drag back from the anchor");
				Assert.AreEqual(new DateTime(2026,3,10),behaviourCalendar.SelectionEnd);
				behaviourCalendar.Release();
				Assert.AreEqual("DateSelected 2026-03-08..2026-03-10",behaviourCalendar.ListEvents.Last());
				//One day at most: a drag moves nothing.
				using BehaviourCalendar behaviourCalendarOne=MakeCalendar(1,true);
				behaviourCalendarOne.Press(new DateTime(2026,3,10));
				behaviourCalendarOne.DragTo(new DateTime(2026,3,13));
				Assert.AreEqual(new DateTime(2026,3,10),behaviourCalendarOne.SelectionEnd);
			});
		}
		#endregion MonthCalendarOD: the mouse

		#region MonthCalendarOD: the keyboard
		[TestMethod]
		public void Calendar_Keyboard_Off_LeavesTheKeysToTheWindow() {
			IconRendererTests.RunSta(() => {
				using BehaviourCalendar behaviourCalendar=MakeCalendar(1,false);
				Assert.IsFalse(behaviourCalendar.IsInput(Keys.Right),"the appointment module's and ODDatePicker's calendars keep their behaviour");
				behaviourCalendar.Key(Keys.Right);
				Assert.AreEqual(_date0310,behaviourCalendar.SelectionStart);
			});
		}

		[TestMethod]
		public void Calendar_Keyboard_MovesTheDay_AsWindowsCalendar() {
			IconRendererTests.RunSta(() => {
				using BehaviourCalendar behaviourCalendar=MakeCalendar(1,true);
				Assert.IsFalse(behaviourCalendar.IsInput(Keys.Enter),"Enter stays the window's (its OK button)");
				Assert.IsFalse(behaviourCalendar.IsInput(Keys.Tab));
				behaviourCalendar.Key(Keys.Right);
				Assert.AreEqual(new DateTime(2026,3,11),behaviourCalendar.SelectionStart);
				behaviourCalendar.Key(Keys.Down);
				Assert.AreEqual(new DateTime(2026,3,18),behaviourCalendar.SelectionStart);
				behaviourCalendar.Key(Keys.Up);
				behaviourCalendar.Key(Keys.Left);
				Assert.AreEqual(_date0310,behaviourCalendar.SelectionStart);
				behaviourCalendar.Key(Keys.PageDown);
				Assert.AreEqual(new DateTime(2026,4,10),behaviourCalendar.SelectionStart);
				behaviourCalendar.PointOf(new DateTime(2026,4,30));//the month follows
				behaviourCalendar.Key(Keys.End);
				Assert.AreEqual(new DateTime(2026,4,30),behaviourCalendar.SelectionStart);
				behaviourCalendar.Key(Keys.Home);
				Assert.AreEqual(new DateTime(2026,4,1),behaviourCalendar.SelectionStart);
				behaviourCalendar.Key(Keys.PageUp);
				Assert.AreEqual(new DateTime(2026,3,1),behaviourCalendar.SelectionStart);
				Assert.AreEqual(8,behaviourCalendar.ListEvents.Count,string.Join("; ",behaviourCalendar.ListEvents));
				Assert.IsTrue(behaviourCalendar.ListEvents.All(x => x.StartsWith("DateChanged ")),"the keyboard raises DateChanged, not DateSelected");
				//MinDate holds the keyboard.
				behaviourCalendar.MinDate=new DateTime(2026,3,1);
				behaviourCalendar.Key(Keys.Left);
				Assert.AreEqual(new DateTime(2026,3,1),behaviourCalendar.SelectionStart);
			});
		}

		[TestMethod]
		public void Calendar_KeyboardWithShift_ExtendsTheRange() {
			IconRendererTests.RunSta(() => {
				using BehaviourCalendar behaviourCalendar=MakeCalendar(7,true);
				behaviourCalendar.Key(Keys.Right|Keys.Shift);
				behaviourCalendar.Key(Keys.Right|Keys.Shift);
				Assert.AreEqual(_date0310,behaviourCalendar.SelectionStart);
				Assert.AreEqual(new DateTime(2026,3,12),behaviourCalendar.SelectionEnd);
				behaviourCalendar.Key(Keys.Down|Keys.Shift);
				Assert.AreEqual(new DateTime(2026,3,16),behaviourCalendar.SelectionEnd,"7 days at most");
				behaviourCalendar.Key(Keys.Right);
				Assert.AreEqual(behaviourCalendar.SelectionStart,behaviourCalendar.SelectionEnd,"without Shift one day again");
			});
		}
		#endregion MonthCalendarOD: the keyboard

		#region The date picker's drop-down
		///<summary>A stock picker with the shared looks (as a window attaches them) and its events recorded.</summary>
		private static DateTimePicker MakePicker(List<string> listEvents) {
			LayoutManagerForms.SetLooksScale();//the program's calendar for the drop-down (FuncCreateCalendar)
			DateTimePicker dateTimePicker=new DateTimePicker();
			dateTimePicker.Format=DateTimePickerFormat.Short;
			dateTimePicker.Value=new DateTime(2026,3,10,14,30,0);
			UiLooks.AttachOne(dateTimePicker);
			dateTimePicker.DropDown+=(sender,e) => listEvents.Add("DropDown");
			dateTimePicker.ValueChanged+=(sender,e) => listEvents.Add("ValueChanged "+dateTimePicker.Value.ToString("yyyy-MM-dd HH:mm"));
			dateTimePicker.CloseUp+=(sender,e) => listEvents.Add("CloseUp");
			return dateTimePicker;
		}

		[TestMethod]
		public void DatePicker_DropDown_IsTheProgramsCalendar_OnThePickersDate() {
			IconRendererTests.RunSta(() => {
				bool isSkippedBefore=UiDateDropLook.IsShowSkippedForTest;
				UiDateDropLook.IsShowSkippedForTest=true;
				try {
					List<string> listEvents=new List<string>();
					using DateTimePicker dateTimePicker=MakePicker(listEvents);
					dateTimePicker.MinDate=new DateTime(2026,3,5);
					Assert.IsTrue(UiDateDropLook.IsAttached(dateTimePicker),"UiLooks attaches the drop-down to every stock date picker");
					Assert.IsTrue(UiDateDropLook.OpenForTest(dateTimePicker));
					ToolStripDropDown toolStripDropDown=UiDateDropLook.GetDropDownForTest(dateTimePicker);
					Assert.IsNotNull(toolStripDropDown);
					MonthCalendarOD monthCalendarOD=(MonthCalendarOD)((ToolStripControlHost)toolStripDropDown.Items[0]).Control;
					Assert.AreEqual(new DateTime(2026,3,10),monthCalendarOD.SelectionStart);
					Assert.AreEqual(new DateTime(2026,3,5),monthCalendarOD.MinDate);
					Assert.IsTrue(monthCalendarOD.AllowKeyboard);
					Assert.IsFalse(monthCalendarOD.AllowClickingTopText,"the picker is the box to type in");
					Assert.AreEqual(new Size(227,162),monthCalendarOD.Size);
					CollectionAssert.AreEqual(new[] { "DropDown" },listEvents);
					//A day chosen: the picker's date with its time of day, then CloseUp.
					UiDateDropLook.ChooseForTest(dateTimePicker,new DateTime(2026,3,12));
					Assert.AreEqual(new DateTime(2026,3,12,14,30,0),dateTimePicker.Value);
					CollectionAssert.AreEqual(new[] { "DropDown","ValueChanged 2026-03-12 14:30","CloseUp" },listEvents);
					Assert.IsNull(UiDateDropLook.GetDropDownForTest(dateTimePicker),"closed");
					//The same day again: no ValueChanged, as Windows' picker.
					listEvents.Clear();
					UiDateDropLook.OpenForTest(dateTimePicker);
					UiDateDropLook.ChooseForTest(dateTimePicker,new DateTime(2026,3,12));
					CollectionAssert.AreEqual(new[] { "DropDown","CloseUp" },listEvents);
				}
				finally {
					UiDateDropLook.IsShowSkippedForTest=isSkippedBefore;
				}
			});
		}

		[TestMethod]
		public void DatePicker_DropDown_ClickAndKeyboard_ChooseTheDay() {
			IconRendererTests.RunSta(() => {
				bool isSkippedBefore=UiDateDropLook.IsShowSkippedForTest;
				UiDateDropLook.IsShowSkippedForTest=true;
				try {
					List<string> listEvents=new List<string>();
					using DateTimePicker dateTimePicker=MakePicker(listEvents);
					//A click on a day of the calendar (its release) chooses it.
					UiDateDropLook.OpenForTest(dateTimePicker);
					BehaviourCalendarHost(dateTimePicker,out MonthCalendarOD monthCalendarOD);
					ClickDay(monthCalendarOD,new DateTime(2026,3,20));
					Assert.AreEqual(new DateTime(2026,3,20,14,30,0),dateTimePicker.Value);
					Assert.IsNull(UiDateDropLook.GetDropDownForTest(dateTimePicker));
					//The arrows move the day (the picker keeps its date), Enter chooses it.
					UiDateDropLook.OpenForTest(dateTimePicker);
					BehaviourCalendarHost(dateTimePicker,out monthCalendarOD);
					SendKey(monthCalendarOD,Keys.Right);
					SendKey(monthCalendarOD,Keys.Down);
					Assert.AreEqual(new DateTime(2026,3,20,14,30,0),dateTimePicker.Value,"the arrows alone change nothing");
					SendKey(monthCalendarOD,Keys.Enter);
					Assert.AreEqual(new DateTime(2026,3,28,14,30,0),dateTimePicker.Value);
					//Escape closes without a change.
					listEvents.Clear();
					UiDateDropLook.OpenForTest(dateTimePicker);
					BehaviourCalendarHost(dateTimePicker,out monthCalendarOD);
					SendKey(monthCalendarOD,Keys.Left);
					SendKey(monthCalendarOD,Keys.Escape);
					Assert.AreEqual(new DateTime(2026,3,28,14,30,0),dateTimePicker.Value);
					CollectionAssert.AreEqual(new[] { "DropDown","CloseUp" },listEvents);
				}
				finally {
					UiDateDropLook.IsShowSkippedForTest=isSkippedBefore;
				}
			});
		}

		[TestMethod]
		public void DatePicker_DropDown_ChecksTheCheckBox_AndKeepsMinDate() {
			IconRendererTests.RunSta(() => {
				bool isSkippedBefore=UiDateDropLook.IsShowSkippedForTest;
				UiDateDropLook.IsShowSkippedForTest=true;
				try {
					List<string> listEvents=new List<string>();
					using DateTimePicker dateTimePicker=MakePicker(listEvents);
					dateTimePicker.ShowCheckBox=true;
					dateTimePicker.Checked=false;
					listEvents.Clear();
					UiDateDropLook.OpenForTest(dateTimePicker);
					UiDateDropLook.ChooseForTest(dateTimePicker,new DateTime(2026,3,10));
					Assert.IsTrue(dateTimePicker.Checked,"a day chosen checks the box, as Windows' drop-down");
					CollectionAssert.AreEqual(new[] { "DropDown","ValueChanged 2026-03-10 14:30","CloseUp" },listEvents,"the box checked raises ValueChanged");
					Assert.AreEqual(new DateTime(2026,3,5,0,0,0),UiDateDropLook.GetValueForDay(new DateTimePicker { MinDate=new DateTime(2026,3,5,0,0,0),Value=new DateTime(2026,3,6,23,0,0) },new DateTime(2026,3,4)),
						"never before MinDate");
				}
				finally {
					UiDateDropLook.IsShowSkippedForTest=isSkippedBefore;
				}
			});
		}

		[TestMethod]
		public void DatePicker_UpDownStyle_HasNoCalendar() {
			IconRendererTests.RunSta(() => {
				List<string> listEvents=new List<string>();
				using DateTimePicker dateTimePicker=MakePicker(listEvents);
				dateTimePicker.ShowUpDown=true;
				Assert.IsFalse(UiDateDropLook.IsProgramCalendar(dateTimePicker));
				Assert.IsFalse(UiDateDropLook.OpenForTest(dateTimePicker));
				Assert.AreEqual(0,listEvents.Count);
			});
		}

		[TestMethod]
		public void DatePicker_F4AndAltDown_OpenTheProgramsCalendar_NotWindows() {
			IconRendererTests.RunSta(() => {
				bool isSkippedBefore=UiDateDropLook.IsShowSkippedForTest;
				UiDateDropLook.IsShowSkippedForTest=true;
				try {
					//With Windows' visual styles, as the program runs: the picker's window class is registered from comctl32 version 6, as the
					//themed picker tests (ElementLookTests) need it.
					using VisualStylesScope visualStylesScope=new VisualStylesScope();
					List<string> listEvents=new List<string>();
					using Form form=new Form();//never shown: its handle and the picker's exist, hidden
					DateTimePicker dateTimePicker=MakePicker(listEvents);
					form.Controls.Add(dateTimePicker);
					IntPtr handleForm=form.Handle;
					IntPtr handle=dateTimePicker.Handle;
					SendMessage(handle,WM_KEYDOWN,(IntPtr)0x73,IntPtr.Zero);//F4
					CollectionAssert.AreEqual(new[] { "DropDown" },listEvents);
					Assert.IsNotNull(UiDateDropLook.GetDropDownForTest(dateTimePicker),"the program's calendar");
					Assert.AreEqual(IntPtr.Zero,SendMessage(handle,DTM_GETMONTHCAL,IntPtr.Zero,IntPtr.Zero),"Windows' calendar is not open");
					UiDateDropLook.ChooseForTest(dateTimePicker,new DateTime(2026,3,11));
					listEvents.Clear();
					SendMessage(handle,WM_SYSKEYDOWN,(IntPtr)0x28,(IntPtr)(1<<29));//Alt+Down
					CollectionAssert.AreEqual(new[] { "DropDown" },listEvents);
					Assert.AreEqual(IntPtr.Zero,SendMessage(handle,DTM_GETMONTHCAL,IntPtr.Zero,IntPtr.Zero),"Windows' calendar is not open");
					UiDateDropLook.ChooseForTest(dateTimePicker,new DateTime(2026,3,11));
				}
				finally {
					UiDateDropLook.IsShowSkippedForTest=isSkippedBefore;
				}
			});
		}

		private const int WM_KEYDOWN=0x0100;
		private const int WM_SYSKEYDOWN=0x0104;
		private const int DTM_GETMONTHCAL=0x1000+8;

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd,int msg,IntPtr wParam,IntPtr lParam);

		private static void BehaviourCalendarHost(DateTimePicker dateTimePicker,out MonthCalendarOD monthCalendarOD) {
			ToolStripDropDown toolStripDropDown=UiDateDropLook.GetDropDownForTest(dateTimePicker);
			Assert.IsNotNull(toolStripDropDown);
			monthCalendarOD=(MonthCalendarOD)((ToolStripControlHost)toolStripDropDown.Items[0]).Control;
		}

		///<summary>A click on the day of the calendar, through its protected handlers (the cell from a painted copy of the same geometry).</summary>
		private static void ClickDay(MonthCalendarOD monthCalendarOD,DateTime date) {
			Point point;
			using(BehaviourCalendar behaviourCalendar=new BehaviourCalendar()) {
				behaviourCalendar.Size=monthCalendarOD.Size;
				behaviourCalendar.Font=monthCalendarOD.Font;
				behaviourCalendar.SetDate(monthCalendarOD.SelectionStart);
				point=behaviourCalendar.PointOf(date);
			}
			using(Bitmap bitmap=new Bitmap(monthCalendarOD.Width,monthCalendarOD.Height))
			using(Graphics g=Graphics.FromImage(bitmap)) {
				Invoke(monthCalendarOD,"OnPaint",new PaintEventArgs(g,monthCalendarOD.ClientRectangle));
			}
			Invoke(monthCalendarOD,"OnMouseMove",new MouseEventArgs(MouseButtons.None,0,point.X,point.Y,0));
			Invoke(monthCalendarOD,"OnMouseDown",new MouseEventArgs(MouseButtons.Left,1,point.X,point.Y,0));
			Invoke(monthCalendarOD,"OnMouseUp",new MouseEventArgs(MouseButtons.Left,1,point.X,point.Y,0));
		}

		///<summary>A key as the drop-down's calendar gets it: PreviewKeyDown (a dialog key the drop-down claims), then KeyDown when it is an input key.</summary>
		private static void SendKey(MonthCalendarOD monthCalendarOD,Keys keyData) {
			PreviewKeyDownEventArgs previewKeyDownEventArgs=new PreviewKeyDownEventArgs(keyData);
			Invoke(monthCalendarOD,"OnPreviewKeyDown",previewKeyDownEventArgs);
			bool isInput=previewKeyDownEventArgs.IsInputKey
				|| (bool)typeof(Control).GetMethod("IsInputKey",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(monthCalendarOD,new object[] { keyData });
			Assert.IsTrue(isInput,"the calendar takes "+keyData);
			Invoke(monthCalendarOD,"OnKeyDown",new KeyEventArgs(keyData));
		}

		private static void Invoke(Control control,string nameMethod,EventArgs eventArgs) {
			MethodInfo methodInfo=control.GetType().GetMethod(nameMethod,BindingFlags.Instance|BindingFlags.NonPublic,null,new[] { eventArgs.GetType() },null);
			methodInfo.Invoke(control,new object[] { eventArgs });
		}
		#endregion The date picker's drop-down
	}
}
