// UI-CAL (docs/port-notes/UI-CAL.md): the stock date picker's drop-down calendar in the program's calendar (MonthCalendarOD, section 3.6 of
// docs/specs/ui-refresh-plan.md) instead of Windows' one. The picker keeps everything else of Windows (typing in the box, the check box, the
// formats, MinDate and MaxDate, the up-down style, which has no calendar): only the opening of its drop-down is taken. A click on its drop button
// (DTM_GETDATETIMEPICKERINFO's rcButton), F4 and Alt+Down open the program's calendar below the picker (aligned as DropDownAlign says, above it
// when the screen has no room below), in a drop-down that closes on a click elsewhere and on Escape. A day chosen with the mouse, or Enter on the
// day the keyboard moved to, becomes the picker's date (its time of day kept) and checks its check box; the picker raises DropDown when the
// calendar opens, ValueChanged when the date changed (or the check box was checked) and CloseUp when it closes, as with Windows' drop-down. The
// calendar is the program's (OpenPmsDental.UI.MonthCalendarOD), which CodeBase cannot reference: the program gives FuncCreateCalendar
// (LayoutManagerForms.SetLooksScale); without it (the plain exes, which have no date pickers) Windows' drop-down stays.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CodeBase {
	///<summary>UI-CAL: what the date picker's drop-down needs of the program's calendar (MonthCalendarOD has these members).</summary>
	public interface IUiDropCalendar {
		DateTime MaxDate { get; set; }
		DateTime MinDate { get; set; }
		DateTime SelectionStart { get; set; }
		///<summary>The user chose a day with the mouse (or Today).</summary>
		event DateRangeEventHandler DateSelected;
	}

	///<summary>UI-CAL: the stock date picker's drop-down in the program's calendar (see the file's header).</summary>
	public class UiDateDropLook : NativeWindow {
		#region Fields
		private const int WM_LBUTTONDOWN=0x0201;
		private const int WM_LBUTTONDBLCLK=0x0203;
		private const int WM_KEYDOWN=0x0100;
		private const int WM_SYSKEYDOWN=0x0104;
		private const int VK_F4=0x73;
		private const int VK_UP=0x26;
		private const int VK_DOWN=0x28;
		private const int DTM_GETDATETIMEPICKERINFO=0x1000+14;
		///<summary>Makes the program's calendar for a picker's drop-down: a Control that is an IUiDropCalendar, sized and scaled for the picker's
		///window, with its keyboard on. Set by the program (LayoutManagerForms.SetLooksScale); null leaves Windows' drop-down.</summary>
		public static Func<DateTimePicker,Control> FuncCreateCalendar;
		private static readonly ConditionalWeakTable<Control,UiDateDropLook> _table=new ConditionalWeakTable<Control,UiDateDropLook>();
		private readonly DateTimePicker _dateTimePicker;
		///<summary>The open drop-down, null when closed.</summary>
		private ToolStripDropDown _toolStripDropDown;
		///<summary>For the tests: the drop-down is made and its events raised, but never shown on screen (the test host shows no window).</summary>
		public static bool IsShowSkippedForTest;
		///<summary>The drop-down was just closed by the click on the drop button that is now arriving (Environment.TickCount of the close, 0 when
		///none): that click closes it and does not open it again, as Windows' drop button toggles its calendar.</summary>
		private int _tickClosedByButton;
		#endregion Fields

		private UiDateDropLook(DateTimePicker dateTimePicker) {
			_dateTimePicker=dateTimePicker;
			if(dateTimePicker.IsHandleCreated) {
				AssignHandle(dateTimePicker.Handle);
			}
			dateTimePicker.HandleCreated+=(sender,e) => AssignHandle(_dateTimePicker.Handle);
			dateTimePicker.HandleDestroyed+=(sender,e) => {
				CloseDropDown(ToolStripDropDownCloseReason.CloseCalled);
				ReleaseHandle();
			};
		}

		#region Methods - Public
		///<summary>Every stock date picker: one in the up-down style has no calendar and never opens one (the picker is checked at each click, as
		///ShowUpDown can change).</summary>
		public static bool IsDrawn(Control control) {
			return control is DateTimePicker;
		}

		public static void Attach(Control control) {
			if(!(control is DateTimePicker dateTimePicker) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiDateDropLook(dateTimePicker));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>Whether the picker's drop-down is the program's calendar now: attached, a calendar to show (FuncCreateCalendar), not the up-down
		///style.</summary>
		public static bool IsProgramCalendar(Control control) {
			return IsAttached(control) && FuncCreateCalendar!=null && control is DateTimePicker dateTimePicker && !dateTimePicker.ShowUpDown;
		}

		///<summary>For the tests and the gallery: opens the picker's drop-down as its drop button does. False when the picker's drop-down is
		///Windows' (IsProgramCalendar).</summary>
		public static bool OpenForTest(DateTimePicker dateTimePicker) {
			if(!IsProgramCalendar(dateTimePicker) || !_table.TryGetValue(dateTimePicker,out UiDateDropLook uiDateDropLook)) {
				return false;
			}
			return uiDateDropLook.OpenDropDown();
		}

		///<summary>For the tests and the gallery: the open drop-down of the picker, else null.</summary>
		public static ToolStripDropDown GetDropDownForTest(DateTimePicker dateTimePicker) {
			if(!_table.TryGetValue(dateTimePicker,out UiDateDropLook uiDateDropLook)) {
				return null;
			}
			return uiDateDropLook._toolStripDropDown;
		}

		///<summary>For the tests: the day chosen in the drop-down, as a click or Enter chooses it (the picker's date takes it and the drop-down
		///closes).</summary>
		public static void ChooseForTest(DateTimePicker dateTimePicker,DateTime date) {
			if(_table.TryGetValue(dateTimePicker,out UiDateDropLook uiDateDropLook) && uiDateDropLook._toolStripDropDown!=null) {
				uiDateDropLook.Choose(date);
			}
		}

		///<summary>Where the drop-down opens, in the picker's client coordinates: below it, at its left edge, or at its right edge when DropDownAlign
		///is Right; above it when its screen's working area has no room below and has room above, as Windows' drop-down opens (ToolStripDropDown
		///keeps it on the screen sideways).</summary>
		public static Point GetDropLocation(DateTimePicker dateTimePicker,Size sizeCalendar) {
			int x=0;
			if(dateTimePicker.DropDownAlign==LeftRightAlignment.Right) {
				x=dateTimePicker.Width-sizeCalendar.Width;
			}
			int y=dateTimePicker.Height;
			if(dateTimePicker.IsHandleCreated) {
				Rectangle rectangleScreen=dateTimePicker.RectangleToScreen(dateTimePicker.ClientRectangle);
				Rectangle rectangleWorkingArea=Screen.FromControl(dateTimePicker).WorkingArea;
				if(rectangleScreen.Bottom+sizeCalendar.Height>rectangleWorkingArea.Bottom && rectangleScreen.Top-sizeCalendar.Height>=rectangleWorkingArea.Top) {
					y=-sizeCalendar.Height;
				}
			}
			return new Point(x,y);
		}

		///<summary>The date the picker takes for a day chosen in its drop-down: the day with the picker's time of day, within its MinDate and
		///MaxDate.</summary>
		public static DateTime GetValueForDay(DateTimePicker dateTimePicker,DateTime day) {
			DateTime value=day.Date+dateTimePicker.Value.TimeOfDay;
			if(value<dateTimePicker.MinDate) {
				value=dateTimePicker.MinDate;
			}
			if(value>dateTimePicker.MaxDate) {
				value=dateTimePicker.MaxDate;
			}
			return value;
		}
		#endregion Methods - Public

		#region Methods - Window
		protected override void WndProc(ref Message m) {
			switch(m.Msg) {
				case WM_LBUTTONDOWN:
				case WM_LBUTTONDBLCLK:
					if(IsProgramCalendar(_dateTimePicker) && _dateTimePicker.Enabled && IsOnDropButton(GetPoint(m.LParam))) {
						if(!_dateTimePicker.Focused) {
							_dateTimePicker.Focus();
						}
						bool isClosedByThisClick=_tickClosedByButton!=0 && unchecked(Environment.TickCount-_tickClosedByButton)<500;
						_tickClosedByButton=0;
						if(!isClosedByThisClick && _toolStripDropDown==null) {
							OpenDropDown();
						}
						return;//never to Windows: it would open its own calendar
					}
					break;
				case WM_KEYDOWN:
				case WM_SYSKEYDOWN:
					int key=m.WParam.ToInt32();
					bool isAlt=m.Msg==WM_SYSKEYDOWN;
					if(IsProgramCalendar(_dateTimePicker) && ((key==VK_F4 && !isAlt) || (isAlt && (key==VK_DOWN || key==VK_UP)))) {
						if(_toolStripDropDown==null) {
							OpenDropDown();
						}
						return;
					}
					break;
			}
			base.WndProc(ref m);
		}

		private static Point GetPoint(IntPtr lParam) {
			int value=unchecked((int)lParam.ToInt64());
			return new Point((short)(value & 0xFFFF),(short)((value>>16) & 0xFFFF));
		}

		///<summary>Whether the point (client coordinates) is on the picker's drop button (DTM_GETDATETIMEPICKERINFO's rcButton, client
		///coordinates). Without the answer (comctl32 before version 6): the right part a scroll bar wide, as Windows draws the button.</summary>
		private bool IsOnDropButton(Point point) {
			DATETIMEPICKERINFO dateTimePickerInfo=new DATETIMEPICKERINFO();
			dateTimePickerInfo.cbSize=Marshal.SizeOf(typeof(DATETIMEPICKERINFO));
			SendMessage(_dateTimePicker.Handle,DTM_GETDATETIMEPICKERINFO,IntPtr.Zero,ref dateTimePickerInfo);
			RECT rect=dateTimePickerInfo.rcButton;
			Rectangle rectangleButton;
			if(rect.Right>rect.Left && rect.Bottom>rect.Top) {
				rectangleButton=Rectangle.FromLTRB(rect.Left,rect.Top,rect.Right,rect.Bottom);
			}
			else {
				Rectangle rectangleClient=_dateTimePicker.ClientRectangle;
				int width=SystemInformation.VerticalScrollBarWidth;
				rectangleButton=new Rectangle(rectangleClient.Right-width,0,width,rectangleClient.Height);
			}
			return rectangleButton.Contains(point);
		}
		#endregion Methods - Window

		#region Methods - Drop-down
		///<summary>Opens the program's calendar below the picker on its date, its days limited to the picker's MinDate and MaxDate, and raises the
		///picker's DropDown. False when the program gives no calendar.</summary>
		private bool OpenDropDown() {
			Control control=FuncCreateCalendar?.Invoke(_dateTimePicker);
			if(!(control is IUiDropCalendar uiDropCalendar)) {
				control?.Dispose();
				return false;
			}
			uiDropCalendar.MinDate=_dateTimePicker.MinDate.Date;
			uiDropCalendar.MaxDate=_dateTimePicker.MaxDate.Date;
			uiDropCalendar.SelectionStart=_dateTimePicker.Value.Date;
			ToolStripControlHost toolStripControlHost=new ToolStripControlHost(control);
			toolStripControlHost.AutoSize=false;
			toolStripControlHost.Margin=Padding.Empty;
			toolStripControlHost.Padding=Padding.Empty;
			toolStripControlHost.Size=control.Size;
			ToolStripDropDown toolStripDropDown=new ToolStripDropDown();
			toolStripDropDown.Renderer=new RendererNone();//the calendar draws its own background and outline
			toolStripDropDown.Padding=Padding.Empty;
			toolStripDropDown.AutoSize=true;
			toolStripDropDown.DropShadowEnabled=true;
			toolStripDropDown.Items.Add(toolStripControlHost);
			uiDropCalendar.DateSelected+=(sender,e) => Choose(e.Start);
			control.PreviewKeyDown+=(sender,e) => {
				if(e.KeyCode==Keys.Enter || e.KeyCode==Keys.Escape) {
					e.IsInputKey=true;
				}
			};
			control.KeyDown+=(sender,e) => {
				if(e.KeyCode==Keys.Enter) {
					e.Handled=true;
					Choose(uiDropCalendar.SelectionStart);
				}
				else if(e.KeyCode==Keys.Escape) {
					e.Handled=true;
					CloseDropDown(ToolStripDropDownCloseReason.Keyboard);
				}
			};
			toolStripDropDown.Closing+=(sender,e) => {
				//A click on the drop button closes the drop-down; the click then arrives at the picker and must not open it again.
				if(e.CloseReason==ToolStripDropDownCloseReason.AppClicked && _dateTimePicker.IsHandleCreated
					&& IsOnDropButton(_dateTimePicker.PointToClient(Control.MousePosition)))
				{
					_tickClosedByButton=Environment.TickCount;
					if(_tickClosedByButton==0) {
						_tickClosedByButton=1;
					}
				}
			};
			toolStripDropDown.Closed+=(sender,e) => {
				if(_toolStripDropDown==toolStripDropDown) {
					_toolStripDropDown=null;
				}
				Raise(_dateTimePicker,"OnCloseUp");
				if(e.CloseReason!=ToolStripDropDownCloseReason.AppClicked && e.CloseReason!=ToolStripDropDownCloseReason.AppFocusChange
					&& _dateTimePicker.CanFocus)
				{
					_dateTimePicker.Focus();
				}
				//Disposed after the close has finished (the drop-down is still raising its events).
				if(_dateTimePicker.IsHandleCreated) {
					_dateTimePicker.BeginInvoke((Action)(() => toolStripDropDown.Dispose()));
				}
				else {
					toolStripDropDown.Dispose();
				}
			};
			_toolStripDropDown=toolStripDropDown;
			_tickClosedByButton=0;
			Raise(_dateTimePicker,"OnDropDown");
			if(IsShowSkippedForTest) {
				return true;
			}
			toolStripDropDown.Show(_dateTimePicker,GetDropLocation(_dateTimePicker,control.Size),ToolStripDropDownDirection.BelowRight);
			control.Focus();
			return true;
		}

		///<summary>The day chosen in the drop-down: the picker takes it (GetValueForDay) and checks its check box, raising ValueChanged when its
		///date changed or the box was unchecked (Windows' picker raises it for either), then the drop-down closes.</summary>
		private void Choose(DateTime day) {
			bool wasUnchecked=_dateTimePicker.ShowCheckBox && !_dateTimePicker.Checked;
			DateTime value=GetValueForDay(_dateTimePicker,day);
			if(wasUnchecked) {
				_dateTimePicker.Checked=true;
			}
			bool isChanged=_dateTimePicker.Value!=value;
			_dateTimePicker.Value=value;
			if(wasUnchecked && !isChanged) {
				Raise(_dateTimePicker,"OnValueChanged");
			}
			CloseDropDown(ToolStripDropDownCloseReason.ItemClicked);
		}

		private void CloseDropDown(ToolStripDropDownCloseReason toolStripDropDownCloseReason) {
			if(_toolStripDropDown==null) {
				return;
			}
			ToolStripDropDown toolStripDropDown=_toolStripDropDown;
			if(toolStripDropDown.Visible) {
				toolStripDropDown.Close(toolStripDropDownCloseReason);
				return;
			}
			//Never shown (off screen): Close raises nothing, so close it as Closed would.
			_toolStripDropDown=null;
			Raise(_dateTimePicker,"OnCloseUp");
			toolStripDropDown.Dispose();
		}

		///<summary>Raises the picker's event through its protected On method (OnDropDown, OnCloseUp, OnValueChanged), as Windows' drop-down makes the
		///picker raise it.</summary>
		private static void Raise(DateTimePicker dateTimePicker,string nameMethod) {
			MethodInfo methodInfo=typeof(DateTimePicker).GetMethod(nameMethod,BindingFlags.Instance|BindingFlags.NonPublic,null,new[] { typeof(EventArgs) },null);
			methodInfo?.Invoke(dateTimePicker,new object[] { EventArgs.Empty });
		}
		#endregion Methods - Drop-down

		#region Interop
		///<summary>Draws nothing: the drop-down is only the calendar, which draws its own background and outline.</summary>
		private class RendererNone : ToolStripRenderer {
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT {
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DATETIMEPICKERINFO {
			public int cbSize;
			public RECT rcCheck;
			public int stateCheck;
			public RECT rcButton;
			public int stateButton;
			public IntPtr hwndEdit;
			public IntPtr hwndUD;
			public IntPtr hwndDropDown;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd,int msg,IntPtr wParam,ref DATETIMEPICKERINFO lParam);
		#endregion Interop
	}
}
