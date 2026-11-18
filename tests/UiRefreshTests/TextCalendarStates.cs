// The real controls of package UI-4b-3 (calendars, date pickers and text boxes; docs/port-notes/UI-4b.md) put in each state off screen
// (TextCalendarControls), and the design sheet's drawing of each state at a control's own geometry (TextCalendarDrawings: SheetPainter's shapes with
// the light tokens, as DesignSheetControls draws the calendar day, the month and the combo box, without the text, with the Fluent glyphs of UI-7a).
// Used by TextCalendarStateTests and by the G-UI1 part 2d state strips (TextCalendarGallery). No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using CodeBase;

namespace UiRefreshTests {
	///<summary>The states of the pictures: rest, hover, keyboard focus, disabled, read only (TextBox, TextRich), a validation error (the validated boxes
	///and the WPF date picker), and today as the selected day (the calendars).</summary>
	public enum TextPictureState {
		Rest,
		Hover,
		Focus,
		Disabled,
		ReadOnly,
		Error,
		TodaySelected,
	}

	///<summary>The WinForms calendar with its focus ring, its texts and the mouse position under the test's control, and its geometry open to it.</summary>
	public class StateMonthCalendar : OpenPmsDental.UI.MonthCalendarOD {
		public bool IsFocusForced;
		public bool IsTextHidden;
		public Point PointMouse=new Point(-1000,-1000);

		protected override bool IsFocusRingShown {
			get { return IsFocusForced; }
		}

		protected override bool IsTextShown {
			get { return !IsTextHidden; }
		}

		protected override Point GetPointMouse() {
			return PointMouse;
		}

		public Rectangle Day(int column,int rowIndex) {
			return GetRectangleDay(column,rowIndex);
		}

		public Rectangle LineUnderDays() {
			return GetRectangleLineUnderDays();
		}

		///<summary>0 year back, 1 month back, 2 month forward, 3 year forward.</summary>
		public Rectangle Arrow(int index) {
			return GetRectangleArrow((EnumCalendarArrow)index);
		}
	}

	///<summary>A calendar picture with the rectangles it was drawn with, in device pixels from the control's top left corner.</summary>
	public class CalendarPicture : IDisposable {
		public Bitmap Bitmap;
		public Rectangle RectangleSelected;
		public Rectangle RectangleToday;
		public Rectangle RectangleHover;
		public Rectangle RectangleHoverButton;
		public Rectangle RectangleLine;
		public Rectangle RectangleFooterMark;
		public Rectangle[] ArrayArrows=new Rectangle[0];
		public bool IsEnabled=true;

		public void Dispose() {
			Bitmap?.Dispose();
		}
	}

	public static class TextCalendarControls {
		public static readonly TextPictureState[] ArrayCalendarStatesWinForms=new TextPictureState[] { TextPictureState.Rest,TextPictureState.Hover,
			TextPictureState.Focus,TextPictureState.Disabled,TextPictureState.TodaySelected };
		public static readonly TextPictureState[] ArrayCalendarStatesWpf=new TextPictureState[] { TextPictureState.Rest,TextPictureState.Hover,
			TextPictureState.Disabled,TextPictureState.TodaySelected };
		public static readonly TextPictureState[] ArrayPickerStatesWinForms=new TextPictureState[] { TextPictureState.Rest,TextPictureState.Hover,
			TextPictureState.Disabled };
		public static readonly TextPictureState[] ArrayPickerStatesWpf=new TextPictureState[] { TextPictureState.Rest,TextPictureState.Hover,
			TextPictureState.Focus,TextPictureState.Disabled,TextPictureState.Error };
		///<summary>The WPF text boxes, by class name.</summary>
		public static readonly string[] ArrayTextKinds=new string[] { "TextBox","TextRich","TextPassword","TextVDate","TextVDouble","TextVInt","TextVTime" };

		///<summary>The selected day of the calendar pictures: two days from today, in today's month, so today's ring shows beside it.</summary>
		public static DateTime GetDateSelected() {
			DateTime dateToday=DateTime.Today;
			return dateToday.Day<=14 ? dateToday.AddDays(2) : dateToday.AddDays(-2);
		}

		///<summary>The states a WPF text box shows: read only for TextBox and TextRich, a validation error for the validated boxes.</summary>
		public static TextPictureState[] GetTextStates(string kind) {
			List<TextPictureState> listStates=new List<TextPictureState> { TextPictureState.Rest,TextPictureState.Hover,TextPictureState.Focus,TextPictureState.Disabled };
			if(kind=="TextBox" || kind=="TextRich") {
				listStates.Add(TextPictureState.ReadOnly);
			}
			if(kind.StartsWith("TextV")) {
				listStates.Add(TextPictureState.Error);
			}
			return listStates.ToArray();
		}

		#region WinForms
		private static Form MakeHost(out System.Windows.Forms.Panel panel) {
			Form form=new Form();
			form.ShowInTaskbar=false;
			form.Size=new Size(900,700);
			panel=new System.Windows.Forms.Panel();
			panel.BackColor=SheetPainter.Light(EnumUiColor.Surface);
			panel.Bounds=new Rectangle(0,0,880,660);
			form.Controls.Add(panel);
			return form;
		}

		private static Bitmap Draw(Control control) {
			Bitmap bitmap=new Bitmap(control.Width,control.Height,PixelFormat.Format32bppArgb);
			control.DrawToBitmap(bitmap,new Rectangle(0,0,control.Width,control.Height));
			return bitmap;
		}

		private static Font ScaleFont(Font font,float scale) {
			return new Font(font.FontFamily,font.Size*scale,font.Style);
		}

		///<summary>MonthCalendarOD (227 x 162 at 96 DPI) in the state; the selected day GetDateSelected (today for TodaySelected); hover on a day of the
		///month that is neither and on the month-back button; withText false draws the shapes alone.</summary>
		public static CalendarPicture RenderWinFormsCalendar(TextPictureState state,bool withText,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateMonthCalendar monthCalendar=new StateMonthCalendar();
				monthCalendar.LayoutManager.SetScaleMS(scale);
				monthCalendar.Font=ScaleFont(monthCalendar.Font,scale);
				monthCalendar.Bounds=new Rectangle(10,10,UiTheme.ScaleI(227,scale),UiTheme.ScaleI(162,scale));
				monthCalendar.SetDateSelected(state==TextPictureState.TodaySelected ? DateTime.Today : GetDateSelected());
				monthCalendar.IsTextHidden=!withText;
				panel.Controls.Add(monthCalendar);
				IntPtr handle=monthCalendar.Handle;
				Draw(monthCalendar).Dispose();//the first paint lays the days out
				DateTime[,] arrayDates=ListStateControls.GetPrivate<DateTime[,]>(monthCalendar,"_arrayDates");
				CalendarPicture calendarPicture=new CalendarPicture();
				Point pointHover=Point.Empty;
				for(int r=0;r<6;r++) {
					for(int c=0;c<7;c++) {
						DateTime date=arrayDates[c,r];
						if(date==monthCalendar.GetDateSelected()) {
							calendarPicture.RectangleSelected=monthCalendar.Day(c,r+1);
						}
						if(date==DateTime.Today) {
							calendarPicture.RectangleToday=monthCalendar.Day(c,r+1);
						}
						if(pointHover.IsEmpty && r>=1 && date.Month==DateTime.Today.Month && date!=DateTime.Today && date!=monthCalendar.GetDateSelected()) {
							pointHover=new Point(c,r+1);
						}
					}
				}
				switch(state) {
					case TextPictureState.Hover:
						ListStateControls.SetPrivate(monthCalendar,"_pointHotHover",pointHover);
						calendarPicture.RectangleHover=monthCalendar.Day(pointHover.X,pointHover.Y);
						calendarPicture.RectangleHoverButton=ListStateControls.GetPrivate<Rectangle>(monthCalendar,"_rectangleMonthLeft");
						monthCalendar.PointMouse=new Point(calendarPicture.RectangleHoverButton.X+calendarPicture.RectangleHoverButton.Width/2,
							calendarPicture.RectangleHoverButton.Y+calendarPicture.RectangleHoverButton.Height/2);
						break;
					case TextPictureState.Focus:
						monthCalendar.IsFocusForced=true;
						break;
					case TextPictureState.Disabled:
						monthCalendar.Enabled=false;
						calendarPicture.IsEnabled=false;
						break;
				}
				calendarPicture.Bitmap=Draw(monthCalendar);
				calendarPicture.RectangleLine=monthCalendar.LineUnderDays();
				calendarPicture.RectangleFooterMark=ListStateControls.GetPrivate<Rectangle>(monthCalendar,"_rectangleFooterMark");
				calendarPicture.ArrayArrows=Enumerable.Range(0,4).Select(x => monthCalendar.Arrow(x)).ToArray();
				return calendarPicture;
			}
		}

		///<summary>ODDatePicker (227 x 23 at 96 DPI, a date in its box) in the state; hover is the mouse over the drop-down button. Returns the picture and
		///the button's bounds.</summary>
		public static Bitmap RenderWinFormsDatePicker(TextPictureState state,float scale,out Rectangle rectangleButton) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				OpenPmsDental.UI.ODDatePicker datePicker=new OpenPmsDental.UI.ODDatePicker();
				datePicker.LayoutManager.SetScaleMS(scale);
				datePicker.Font=ScaleFont(datePicker.Font,scale);
				panel.Controls.Add(datePicker);
				datePicker.Bounds=new Rectangle(10,10,UiTheme.ScaleI(227,scale),UiTheme.ScaleI(23,scale));
				IntPtr handle=datePicker.Handle;
				foreach(Control control in datePicker.Controls) {
					handle=control.Handle;//a hidden window does not create its children's windows, and DrawToBitmap prints only those that have one
				}
				datePicker.SetDateTime(new DateTime(2026,11,18));
				datePicker.HideCalendar();//as its Load does
				System.Windows.Forms.Button butDrop=ListStateControls.GetPrivate<System.Windows.Forms.Button>(datePicker,"butDrop");
				switch(state) {
					case TextPictureState.Hover:
						ListStateControls.CallPrivate(butDrop,"OnMouseEnter",EventArgs.Empty);
						break;
					case TextPictureState.Disabled:
						datePicker.Enabled=false;
						break;
				}
				rectangleButton=butDrop.Bounds;
				//The tree as the design sheet's window pictures draw it: the stock date box painted as Windows paints it (an edit control of a hidden
				//window prints nothing), the button by its own paint.
				Bitmap bitmap=new Bitmap(datePicker.Width,datePicker.Height,PixelFormat.Format32bppArgb);
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(panel.BackColor);
					DesignSheetWindows.DrawTree(g,datePicker,Point.Empty);
				}
				return bitmap;
			}
		}
		#endregion WinForms

		#region WPF
		///<summary>The WPF text box of the kind (120 x 20, TextRich 120 x 36) in the state, with its text (withText) or empty; isPinned sets
		///ContentLook.IsPinned (office content). Hover and focus are set on the template's parts as its triggers do (a control off screen has no mouse
		///and no keyboard focus); read only, disabled and the error are the controls' own.</summary>
		public static System.Windows.FrameworkElement MakeWpfText(string kind,TextPictureState state,bool withText,bool isPinned,out System.Windows.Controls.Control controlInner) {
			System.Windows.FrameworkElement frameworkElement;
			string text=withText ? "Sample 123" : "";
			switch(kind) {
				case "TextBox":
					WpfControls.UI.TextBox textBox=new WpfControls.UI.TextBox { Text=text };
					textBox.ReadOnly=state==TextPictureState.ReadOnly;
					frameworkElement=textBox;
					break;
				case "TextRich":
					WpfControls.UI.TextRich textRich=new WpfControls.UI.TextRich { SpellCheckIsEnabled=false };
					OpenPmsDentalBusiness.QuickPasteCats.FillCacheFromTable(OpenPmsDentalBusiness.Crud.QuickPasteCatCrud.ListToTable(new List<OpenPmsDentalBusiness.QuickPasteCat>()));
					try {
						textRich.Text=text;
					}
					finally {
						OpenPmsDentalBusiness.QuickPasteCats.ClearCache();
					}
					textRich.ReadOnly=state==TextPictureState.ReadOnly;
					frameworkElement=textRich;
					break;
				case "TextPassword":
					WpfControls.UI.TextPassword textPassword=new WpfControls.UI.TextPassword();
					ListStateControls.GetPrivate<System.Windows.Controls.PasswordBox>(textPassword,"passwordBox").Password=withText ? "secret" : "";
					frameworkElement=textPassword;
					break;
				case "TextVDate":
					frameworkElement=new WpfControls.UI.TextVDate { Text=withText ? "11/18/2026" : "" };
					break;
				case "TextVDouble":
					frameworkElement=new WpfControls.UI.TextVDouble();
					ListStateControls.GetPrivate<System.Windows.Controls.TextBox>(frameworkElement,"textBox").Text=withText ? "12.50" : "";
					break;
				case "TextVInt":
					frameworkElement=new WpfControls.UI.TextVInt();
					ListStateControls.GetPrivate<System.Windows.Controls.TextBox>(frameworkElement,"textBox").Text=withText ? "1234" : "";
					break;
				default:
					frameworkElement=new WpfControls.UI.TextVTime();
					ListStateControls.GetPrivate<System.Windows.Controls.TextBox>(frameworkElement,"textBox").Text=withText ? "8:30 AM" : "";
					break;
			}
			controlInner=GetInner(frameworkElement);
			frameworkElement.Width=120;
			frameworkElement.Height=kind=="TextRich" ? 36 : 20;
			if(isPinned) {
				WpfControls.UI.ContentLook.SetIsPinned(frameworkElement,true);
			}
			if(state==TextPictureState.Disabled) {
				frameworkElement.IsEnabled=false;
			}
			if(state==TextPictureState.Error) {
				ListStateControls.CallPrivate(frameworkElement,"SetError","Invalid.");
			}
			frameworkElement.Measure(new System.Windows.Size(frameworkElement.Width,frameworkElement.Height));
			frameworkElement.Arrange(new System.Windows.Rect(0,0,frameworkElement.Width,frameworkElement.Height));
			frameworkElement.UpdateLayout();
			if(state==TextPictureState.Hover) {
				((System.Windows.Controls.Border)controlInner.Template.FindName("border",controlInner)).BorderBrush=WpfControls.UI.UiLookText.BrushStrokeStrongHover;
			}
			if(state==TextPictureState.Focus) {
				((System.Windows.FrameworkElement)controlInner.Template.FindName("focusLine",controlInner)).Visibility=System.Windows.Visibility.Visible;
			}
			frameworkElement.UpdateLayout();
			return frameworkElement;
		}

		///<summary>The Windows text box inside one of the WPF text controls.</summary>
		public static System.Windows.Controls.Control GetInner(System.Windows.FrameworkElement frameworkElement) {
			if(frameworkElement is WpfControls.UI.TextRich) {
				return ListStateControls.GetPrivate<System.Windows.Controls.RichTextBox>(frameworkElement,"richTextBox");
			}
			if(frameworkElement is WpfControls.UI.TextPassword) {
				return ListStateControls.GetPrivate<System.Windows.Controls.PasswordBox>(frameworkElement,"passwordBox");
			}
			return ListStateControls.GetPrivate<System.Windows.Controls.TextBox>(frameworkElement,"textBox");
		}

		public static Bitmap RenderWpfText(string kind,TextPictureState state,bool withText,float scale) {
			System.Windows.Controls.Control controlInner;
			System.Windows.FrameworkElement frameworkElement=MakeWpfText(kind,state,withText,isPinned:false,out controlInner);
			return ControlPictures.RenderWpf(frameworkElement,frameworkElement.Width,frameworkElement.Height,scale);
		}

		///<summary>The WPF DatePicker (100 x 20, a date or empty) in the state: hover and the focus line through its own fields, as the mouse and the keyboard
		///set them.</summary>
		public static Bitmap RenderWpfDatePicker(TextPictureState state,bool withText,float scale) {
			WpfControls.UI.DatePicker datePicker=new WpfControls.UI.DatePicker();
			datePicker.Width=100;
			datePicker.Height=20;
			if(withText) {
				datePicker.Text="11/18/2026";
			}
			switch(state) {
				case TextPictureState.Hover:
					ListStateControls.SetPrivate(datePicker,"_isHover",true);
					break;
				case TextPictureState.Focus:
					ListStateControls.SetPrivate(datePicker,"_isFocusForced",true);
					break;
				case TextPictureState.Disabled:
					datePicker.IsEnabled=false;
					break;
				case TextPictureState.Error:
					ListStateControls.CallPrivate(datePicker,"SetError","Invalid date.");
					break;
			}
			ListStateControls.CallPrivate(datePicker,"SetColors");
			return ControlPictures.RenderWpf(datePicker,100,20,scale);
		}

		///<summary>The WPF MonthCalendar (227 x 162) in the state, laid out; hover on a day of the month that is neither selected nor today, placed as the
		///mouse handler places it. withText false hides the texts and the chevrons (the shapes alone).</summary>
		public static WpfControls.UI.MonthCalendar MakeWpfCalendar(TextPictureState state,bool withText) {
			WpfControls.UI.MonthCalendar monthCalendar=new WpfControls.UI.MonthCalendar();
			monthCalendar.Width=227;
			monthCalendar.Height=162;
			monthCalendar.SetDateSelected(state==TextPictureState.TodaySelected ? DateTime.Today : GetDateSelected());
			if(state==TextPictureState.Disabled) {
				monthCalendar.IsEnabled=false;
			}
			monthCalendar.Measure(new System.Windows.Size(227,162));
			monthCalendar.Arrange(new System.Windows.Rect(0,0,227,162));
			monthCalendar.UpdateLayout();
			if(state==TextPictureState.Hover) {
				DateTime[,] arrayDates=ListStateControls.GetPrivate<DateTime[,]>(monthCalendar,"_dateTimeArray");
				System.Windows.Controls.Border borderHover=ListStateControls.GetPrivate<System.Windows.Controls.Border>(monthCalendar,"borderHover");
				System.Windows.Controls.Grid gridMain=ListStateControls.GetPrivate<System.Windows.Controls.Grid>(monthCalendar,"gridMain");
				for(int r=1;r<6 && borderHover.Parent==null;r++) {
					for(int c=0;c<7;c++) {
						DateTime date=arrayDates[c,r];
						if(date.Month!=DateTime.Today.Month || date==DateTime.Today || date==monthCalendar.GetDateSelected()) {
							continue;
						}
						borderHover.Visibility=System.Windows.Visibility.Visible;
						borderHover.Margin=new System.Windows.Thickness(2,0,2,0);
						gridMain.Children.Add(borderHover);
						System.Windows.Controls.Grid.SetRow(borderHover,r+1);
						System.Windows.Controls.Grid.SetColumn(borderHover,c);
						break;
					}
				}
			}
			if(!withText) {
				foreach(System.Windows.FrameworkElement frameworkElement in FindAll(monthCalendar).Where(x => x is System.Windows.Controls.TextBlock
					|| x is System.Windows.Shapes.Path))
				{
					frameworkElement.Visibility=System.Windows.Visibility.Hidden;
				}
			}
			monthCalendar.UpdateLayout();
			return monthCalendar;
		}

		public static List<System.Windows.FrameworkElement> FindAll(System.Windows.DependencyObject dependencyObject) {
			List<System.Windows.FrameworkElement> listFound=new List<System.Windows.FrameworkElement>();
			for(int i=0;i<System.Windows.Media.VisualTreeHelper.GetChildrenCount(dependencyObject);i++) {
				System.Windows.DependencyObject dependencyObjectChild=System.Windows.Media.VisualTreeHelper.GetChild(dependencyObject,i);
				if(dependencyObjectChild is System.Windows.FrameworkElement frameworkElement) {
					listFound.Add(frameworkElement);
				}
				listFound.AddRange(FindAll(dependencyObjectChild));
			}
			return listFound;
		}

		///<summary>A part's rectangle in device pixels within an ancestor, at the scale; empty when it is not in the tree.</summary>
		public static Rectangle GetRectangle(System.Windows.FrameworkElement frameworkElement,System.Windows.Media.Visual visualAncestor,float scale) {
			if(frameworkElement==null || System.Windows.Media.VisualTreeHelper.GetParent(frameworkElement)==null) {
				return Rectangle.Empty;
			}
			return TabSplitControls.GetRectangle(frameworkElement,visualAncestor,scale);
		}

		public static CalendarPicture RenderWpfCalendar(TextPictureState state,bool withText,float scale) {
			WpfControls.UI.MonthCalendar monthCalendar=MakeWpfCalendar(state,withText);
			CalendarPicture calendarPicture=new CalendarPicture();
			calendarPicture.IsEnabled=state!=TextPictureState.Disabled;
			System.Windows.Controls.Border borderSelected=ListStateControls.GetPrivate<System.Windows.Controls.Border>(monthCalendar,"borderDateSelected");
			System.Windows.Controls.Border borderToday=ListStateControls.GetPrivate<System.Windows.Controls.Border>(monthCalendar,"borderToday");
			System.Windows.Controls.Border borderHover=ListStateControls.GetPrivate<System.Windows.Controls.Border>(monthCalendar,"borderHover");
			calendarPicture.RectangleSelected=borderSelected.Visibility==System.Windows.Visibility.Visible ? GetRectangle(borderSelected,monthCalendar,scale) : Rectangle.Empty;
			calendarPicture.RectangleToday=borderToday.Visibility==System.Windows.Visibility.Visible ? GetRectangle(borderToday,monthCalendar,scale) : Rectangle.Empty;
			calendarPicture.RectangleHover=borderHover.Parent!=null ? GetRectangle(borderHover,monthCalendar,scale) : Rectangle.Empty;
			Rectangle rectangleUnderDays=GetRectangle(ListStateControls.GetPrivate<System.Windows.Controls.Border>(monthCalendar,"borderUnderDays"),monthCalendar,scale);
			calendarPicture.RectangleLine=new Rectangle(rectangleUnderDays.X,rectangleUnderDays.Bottom-UiTheme.GetLineWidth(scale),rectangleUnderDays.Width,UiTheme.GetLineWidth(scale));
			calendarPicture.RectangleFooterMark=GetRectangle(ListStateControls.GetPrivate<System.Windows.Controls.Border>(monthCalendar,"borderTodayBottom"),monthCalendar,scale);
			calendarPicture.Bitmap=ControlPictures.RenderWpf(monthCalendar,227,162,scale);
			return calendarPicture;
		}
		#endregion WPF
	}

	///<summary>The design sheet's drawing of a state at a control's own geometry: DesignSheetControls.DrawCalendarDay and DrawCalendarAndProgress's
	///shapes for the calendars, DrawComboBox's for the WPF date picker with the calendar glyph, and the input of the sheet's window drawing (SurfaceRaised,
	///a square 1 px StrokeStrong border) with the states of section 3.6 for the text boxes; without the text.</summary>
	public static class TextCalendarDrawings {
		private static Bitmap Start(Size size,Color colorBack,float scale,out Graphics g,out SheetPainter p) {
			Bitmap bitmap=new Bitmap(size.Width,size.Height,PixelFormat.Format32bppArgb);
			g=Graphics.FromImage(bitmap);
			g.Clear(colorBack);
			p=new SheetPainter(g,scale,SheetPainter.Light);
			return bitmap;
		}

		///<summary>The month: SurfaceRaised, the Gridline under the weekdays, hover SubtleHover, the selected day Accent (TextDisabled disabled), today's
		///1 px Accent ring (and the white inner ring when it is the selected day), the focus ring on the accent, the hovered button, the chevrons
		///(arrows given), the footer's today mark and the 1 px Stroke outline, in the controls' order.</summary>
		public static Bitmap Calendar(CalendarPicture calendarPicture,TextPictureState state,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(calendarPicture.Bitmap.Size,Color.White,scale,out g,out p);
			using(g)
			using(p) {
				Rectangle rectangleAll=new Rectangle(Point.Empty,calendarPicture.Bitmap.Size);
				p.Fill(EnumUiColor.SurfaceRaised,rectangleAll,0);
				p.Fill(EnumUiColor.Gridline,calendarPicture.RectangleLine,0);
				if(!calendarPicture.RectangleHover.IsEmpty) {
					p.Fill(EnumUiColor.SubtleHover,calendarPicture.RectangleHover,UiTheme.CORNER_CONTROL);
				}
				p.Fill(calendarPicture.IsEnabled ? EnumUiColor.Accent : EnumUiColor.TextDisabled,calendarPicture.RectangleSelected,UiTheme.CORNER_CONTROL);
				if(!calendarPicture.RectangleToday.IsEmpty) {
					p.Outline(EnumUiColor.Accent,calendarPicture.RectangleToday,p.LineWidth,UiTheme.CORNER_CONTROL);
					if(state==TextPictureState.TodaySelected) {
						p.Outline(EnumUiColor.TextOnAccent,Rectangle.Inflate(calendarPicture.RectangleToday,-p.LineWidth,-p.LineWidth),p.LineWidth,UiTheme.CORNER_CONTROL-1);
					}
				}
				if(state==TextPictureState.Focus) {
					p.FocusRingOnAccent(calendarPicture.RectangleSelected,UiTheme.CORNER_CONTROL);
				}
				if(!calendarPicture.RectangleHoverButton.IsEmpty) {
					p.Fill(EnumUiColor.SubtleHover,calendarPicture.RectangleHoverButton,UiTheme.CORNER_CONTROL);
				}
				Color colorGlyph=p.C(calendarPicture.IsEnabled ? EnumUiColor.Text : EnumUiColor.TextDisabled);
				for(int i=0;i<calendarPicture.ArrayArrows.Length;i++) {
					UiIcons.Draw(p.G,i<2 ? UiGlyph.ChevronLeft : UiGlyph.ChevronRight,calendarPicture.ArrayArrows[i],colorGlyph);
				}
				p.Outline(EnumUiColor.Accent,calendarPicture.RectangleFooterMark,p.LineWidth,UiTheme.CORNER_CONTROL);
				p.Outline(EnumUiColor.Stroke,rectangleAll,p.LineWidth,0);
			}
			return bitmap;
		}

		///<summary>The WinForms date picker's drop-down button: SurfaceRaised (SubtleHover under the mouse) with the calendar glyph in Text (TextDisabled
		///disabled), centred.</summary>
		public static Bitmap PickerButton(Size size,TextPictureState state,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,SheetPainter.Light(state==TextPictureState.Hover ? EnumUiColor.SubtleHover : EnumUiColor.SurfaceRaised),scale,out g,out p);
			using(g)
			using(p) {
				Rectangle rectangleGlyph=OpenPmsDental.UI.ODDatePicker.GetRectangleGlyph(new Rectangle(Point.Empty,size),scale);
				UiCalendarPaint.DrawCalendarGlyph(p.G,rectangleGlyph,state==TextPictureState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text);
			}
			return bitmap;
		}

		///<summary>The token of a text box's border in the state.</summary>
		public static EnumUiColor TextBorder(TextPictureState state) {
			switch(state) {
				case TextPictureState.Hover:
					return EnumUiColor.StrokeStrongHover;
				case TextPictureState.ReadOnly:
				case TextPictureState.Disabled:
					return EnumUiColor.Stroke;
				case TextPictureState.Error:
					return EnumUiColor.Danger;
				default:
					return EnumUiColor.StrokeStrong;
			}
		}

		///<summary>A text box: SurfaceRaised with the square 1 px border of the state, the 2 px Accent line inside the bottom edge with focus.</summary>
		public static Bitmap TextBox(Size size,TextPictureState state,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,Color.White,scale,out g,out p);
			using(g)
			using(p) {
				Rectangle rectangle=new Rectangle(Point.Empty,size);
				p.Fill(EnumUiColor.SurfaceRaised,rectangle,(int)WpfControls.UI.UiLookText.CORNER_TEXT);
				p.Outline(TextBorder(state),rectangle,p.LineWidth,(int)WpfControls.UI.UiLookText.CORNER_TEXT);
				if(state==TextPictureState.Focus) {
					int inset=p.S((int)WpfControls.UI.UiLookText.CORNER_TEXT)/2;
					p.Fill(EnumUiColor.Accent,new Rectangle(inset,size.Height-p.FocusWidth,size.Width-2*inset,p.FocusWidth),0);
				}
			}
			return bitmap;
		}

		///<summary>The WPF date picker as the sheet's combo box: SurfaceRaised with 4 px corners, the border of the state (StrokeStrongHover on hover, Stroke
		///disabled, Danger with an error), the calendar glyph in the 14 px column at the right, the focus line inside the bottom edge.</summary>
		public static Bitmap DatePicker(Size size,TextPictureState state,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,Color.White,scale,out g,out p);
			using(g)
			using(p) {
				Rectangle rectangle=new Rectangle(Point.Empty,size);
				p.Fill(EnumUiColor.SurfaceRaised,rectangle,UiTheme.CORNER_CONTROL);
				EnumUiColor uiColorBorder=state==TextPictureState.Disabled ? EnumUiColor.Stroke : TextBorder(state);
				p.Outline(uiColorBorder,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
				int sizeGlyph=p.S(UiCalendarPaint.SIZE_GLYPH);
				Rectangle rectangleGlyph=new Rectangle(size.Width-p.S(14),(size.Height-sizeGlyph)/2,sizeGlyph,sizeGlyph);
				UiCalendarPaint.DrawCalendarGlyph(p.G,rectangleGlyph,state==TextPictureState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text);
				if(state==TextPictureState.Focus) {
					int inset=p.S(UiTheme.CORNER_CONTROL)/2;
					p.Fill(EnumUiColor.Accent,new Rectangle(inset,size.Height-p.FocusWidth,size.Width-2*inset,p.FocusWidth),0);
				}
			}
			return bitmap;
		}
	}
}
