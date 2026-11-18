// The control state pictures of package UI-4b-3 (docs/specs/ui-refresh-plan.md, section 7's UI-4b-3 line and section 3.6; docs/port-notes/UI-4b.md):
// the WinForms MonthCalendarOD (rest, hover, focus, disabled, today selected) and ODDatePicker's drop-down button (rest, hover, disabled), the WPF
// text boxes (TextBox, TextRich, TextPassword, TextVDate, TextVDouble, TextVInt, TextVTime: rest, hover, focus, disabled, read only, a validation
// error), the WPF DatePicker (rest, hover, focus, disabled, error) and MonthCalendar (rest, hover, disabled, today selected), each drawn off screen and
// compared with the design sheet's drawing of the state at the control's own geometry (TextCalendarDrawings) by ChartImageCompare's tolerance, as
// UI-4a, UI-4b-1 and UI-4b-2 do: WinForms every pixel, WPF the solid pixels. Then with text: the text's pixels reach the state's token. The text of
// every WPF text box stays where the Windows template put it; the templates' triggers give the states the tests set by hand; office content keeps
// the Windows template. No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UiRefreshTests {
	[TestClass]
	public class TextCalendarStateTests {
		#region WinForms
		[TestMethod]
		public void WinForms_MonthCalendar_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(TextPictureState state in TextCalendarControls.ArrayCalendarStatesWinForms) {
					string name="WinForms MonthCalendarOD "+state;
					using(CalendarPicture calendarPictureChrome=TextCalendarControls.RenderWinFormsCalendar(state,withText:false,1f))
					using(CalendarPicture calendarPictureText=TextCalendarControls.RenderWinFormsCalendar(state,withText:true,1f))
					using(Bitmap bitmapExpected=TextCalendarDrawings.Calendar(calendarPictureChrome,state,1f)) {
						if(calendarPictureChrome.RectangleSelected.IsEmpty || calendarPictureChrome.RectangleToday.IsEmpty) {
							listFailures.Add(name+": the selected day or today is not shown.");
							continue;
						}
						ControlStateTests.CompareAll(listFailures,name,bitmapExpected,calendarPictureChrome.Bitmap);
						bool isEnabled=state!=TextPictureState.Disabled;
						//The selected day's number in white on the accent; the header's month in Text.
						CheckTextIn(listFailures,name+" selected day",calendarPictureChrome,calendarPictureText,calendarPictureChrome.RectangleSelected,
							SheetPainter.Light(EnumUiColor.TextOnAccent));
						Rectangle rectangleHeader=new Rectangle(calendarPictureChrome.ArrayArrows[1].Right,0,
							calendarPictureChrome.ArrayArrows[2].Left-calendarPictureChrome.ArrayArrows[1].Right,calendarPictureChrome.RectangleLine.Top/2);
						CheckTextIn(listFailures,name+" month",calendarPictureChrome,calendarPictureText,rectangleHeader,
							SheetPainter.Light(isEnabled ? EnumUiColor.Text : EnumUiColor.TextDisabled));
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_DatePicker_TheButtonDrawsTheCalendarGlyph_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(TextPictureState state in TextCalendarControls.ArrayPickerStatesWinForms) {
					string name="WinForms ODDatePicker "+state;
					Rectangle rectangleButton;
					using(Bitmap bitmap=TextCalendarControls.RenderWinFormsDatePicker(state,1f,out rectangleButton))
					using(Bitmap bitmapButton=bitmap.Clone(rectangleButton,PixelFormat.Format32bppArgb))
					using(Bitmap bitmapExpected=TextCalendarDrawings.PickerButton(rectangleButton.Size,state,1f)) {
						ControlStateTests.CompareAll(listFailures,name,bitmapExpected,bitmapButton);
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}
		#endregion WinForms

		#region WPF
		[TestMethod]
		public void Wpf_TextBoxes_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(string kind in TextCalendarControls.ArrayTextKinds) {
					foreach(TextPictureState state in TextCalendarControls.GetTextStates(kind)) {
						string name="WPF "+kind+" "+state;
						using(Bitmap bitmapChrome=TextCalendarControls.RenderWpfText(kind,state,withText:false,1f))
						using(Bitmap bitmapText=TextCalendarControls.RenderWpfText(kind,state,withText:true,1f))
						using(Bitmap bitmapExpected=TextCalendarDrawings.TextBox(bitmapChrome.Size,state,1f)) {
							ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,bitmapChrome);
							ControlStateTests.CheckText(listFailures,name,bitmapChrome,bitmapText,
								SheetPainter.Light(state==TextPictureState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text));
						}
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_TextBoxes_TheTextStaysWhereTheWindowsTemplatePutIt() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(string kind in TextCalendarControls.ArrayTextKinds) {
					Rectangle rectangleNew=GetTextInk(kind,isPinned:false);
					Rectangle rectanglePinned=GetTextInk(kind,isPinned:true);
					if(rectanglePinned.IsEmpty) {
						listFailures.Add("WPF "+kind+": the text draws nothing.");
						continue;
					}
					if(rectangleNew!=rectanglePinned) {
						listFailures.Add("WPF "+kind+": the text moved from "+rectanglePinned+" (the Windows template) to "+rectangleNew+".");
					}
				}
				//The multi-line text box, and the date picker's text box (its own template without a border colour against the Windows one).
				Rectangle rectangleMultiNew=GetMultilineInk(isPinned:false);
				Rectangle rectangleMultiPinned=GetMultilineInk(isPinned:true);
				if(rectangleMultiNew!=rectangleMultiPinned || rectangleMultiPinned.IsEmpty) {
					listFailures.Add("WPF TextBox multi-line: the text moved from "+rectangleMultiPinned+" to "+rectangleMultiNew+".");
				}
				Rectangle rectanglePickerNew=GetDatePickerInk(isWindowsTemplate:false);
				Rectangle rectanglePickerWindows=GetDatePickerInk(isWindowsTemplate:true);
				if(rectanglePickerNew!=rectanglePickerWindows || rectanglePickerWindows.IsEmpty) {
					listFailures.Add("WPF DatePicker: the text moved from "+rectanglePickerWindows+" to "+rectanglePickerNew+".");
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_TextTemplates_TriggersGiveTheStates_AndOfficeContentKeepsTheWindowsTemplate() {
			ControlStateTests.RunWpf(() => {
				System.Windows.Controls.TextBox textBoxProbe=new System.Windows.Controls.TextBox();
				System.Windows.Controls.ControlTemplate controlTemplate=(System.Windows.Controls.ControlTemplate)textBoxProbe.FindResource(WpfControls.UI.UiLookText.KeyTemplateTextBox);
				AssertTrigger(controlTemplate,System.Windows.UIElement.IsMouseOverProperty,true,"border",System.Windows.Controls.Border.BorderBrushProperty,EnumUiColor.StrokeStrongHover);
				AssertTrigger(controlTemplate,System.Windows.Controls.Primitives.TextBoxBase.IsReadOnlyProperty,true,"border",System.Windows.Controls.Border.BorderBrushProperty,EnumUiColor.Stroke);
				AssertTrigger(controlTemplate,WpfControls.UI.UiLookText.IsErrorProperty,true,"border",System.Windows.Controls.Border.BorderBrushProperty,EnumUiColor.Danger);
				AssertTrigger(controlTemplate,System.Windows.UIElement.IsEnabledProperty,false,"border",System.Windows.Controls.Border.BorderBrushProperty,EnumUiColor.Stroke);
				AssertTrigger(controlTemplate,System.Windows.UIElement.IsEnabledProperty,false,null,System.Windows.Controls.Control.ForegroundProperty,EnumUiColor.TextDisabled);
				System.Windows.Setter setterFocus=GetSetter(controlTemplate,System.Windows.UIElement.IsKeyboardFocusedProperty,true,"focusLine",System.Windows.UIElement.VisibilityProperty);
				Assert.AreEqual(System.Windows.Visibility.Visible,setterFocus.Value,"The focus line shows with keyboard focus.");
				System.Windows.Controls.ControlTemplate controlTemplatePassword=(System.Windows.Controls.ControlTemplate)textBoxProbe.FindResource(WpfControls.UI.UiLookText.KeyTemplatePasswordBox);
				AssertTrigger(controlTemplatePassword,System.Windows.UIElement.IsMouseOverProperty,true,"border",System.Windows.Controls.Border.BorderBrushProperty,EnumUiColor.StrokeStrongHover);
				//The new look on a text box of a window; the Windows template inside office content (the eForm fill surface), and back.
				foreach(string kind in TextCalendarControls.ArrayTextKinds) {
					System.Windows.Controls.Control controlInner;
					TextCalendarControls.MakeWpfText(kind,TextPictureState.Rest,withText:true,isPinned:false,out controlInner);
					object resourceKey=controlInner is System.Windows.Controls.PasswordBox ? WpfControls.UI.UiLookText.KeyTemplatePasswordBox : WpfControls.UI.UiLookText.KeyTemplateTextBox;
					Assert.AreSame(controlInner.FindResource(resourceKey),controlInner.Template,kind+" takes the new template.");
					Assert.AreEqual(SheetPainter.Light(EnumUiColor.StrokeStrong).ToArgb(),ToDrawing(((System.Windows.Media.SolidColorBrush)controlInner.BorderBrush).Color).ToArgb(),kind+" border");
					TextCalendarControls.MakeWpfText(kind,TextPictureState.Rest,withText:true,isPinned:true,out controlInner);
					Assert.AreNotSame(controlInner.FindResource(resourceKey),controlInner.Template,kind+" in office content keeps the Windows template.");
					Assert.AreEqual(System.Windows.DependencyProperty.UnsetValue,controlInner.ReadLocalValue(System.Windows.Controls.Control.BorderBrushProperty),kind+" in office content keeps the Windows border.");
				}
			});
		}

		[TestMethod]
		public void Wpf_DatePicker_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(TextPictureState state in TextCalendarControls.ArrayPickerStatesWpf) {
					string name="WPF DatePicker "+state;
					using(Bitmap bitmapChrome=TextCalendarControls.RenderWpfDatePicker(state,withText:false,1f))
					using(Bitmap bitmapText=TextCalendarControls.RenderWpfDatePicker(state,withText:true,1f))
					using(Bitmap bitmapExpected=TextCalendarDrawings.DatePicker(bitmapChrome.Size,state,1f)) {
						ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,bitmapChrome);
						ControlStateTests.CheckText(listFailures,name,bitmapChrome,bitmapText,
							SheetPainter.Light(state==TextPictureState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text));
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_MonthCalendar_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(TextPictureState state in TextCalendarControls.ArrayCalendarStatesWpf) {
					string name="WPF MonthCalendar "+state;
					using(CalendarPicture calendarPicture=TextCalendarControls.RenderWpfCalendar(state,withText:false,1f))
					using(Bitmap bitmapExpected=TextCalendarDrawings.Calendar(calendarPicture,state,1f)) {
						if(calendarPicture.RectangleSelected.IsEmpty || calendarPicture.RectangleToday.IsEmpty) {
							listFailures.Add(name+": the selected day or today is not shown.");
							continue;
						}
						ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,calendarPicture.Bitmap);
					}
					//The texts' and chevrons' tokens (the picture above has them hidden).
					WpfControls.UI.MonthCalendar monthCalendar=TextCalendarControls.MakeWpfCalendar(state,withText:true);
					bool isEnabled=state!=TextPictureState.Disabled;
					List<System.Windows.Controls.TextBlock> listTextBlocks=ListStateControls.GetPrivate<List<System.Windows.Controls.TextBlock>>(monthCalendar,"_listTextBlocks");
					DateTime[,] arrayDates=ListStateControls.GetPrivate<DateTime[,]>(monthCalendar,"_dateTimeArray");
					AssertBrush(listFailures,name+" weekday",listTextBlocks[0].Foreground,isEnabled ? EnumUiColor.TextSecondary : EnumUiColor.TextDisabled);
					for(int c=0;c<7;c++) {
						for(int r=0;r<6;r++) {
							EnumUiColor uiColorDay=UiCalendarPaint.GetDayText(arrayDates[c,r]==monthCalendar.GetDateSelected(),arrayDates[c,r].Month==monthCalendar.GetDateSelected().Month,isEnabled);
							AssertBrush(listFailures,name+" day "+arrayDates[c,r].ToShortDateString(),listTextBlocks[7+c*6+r].Foreground,uiColorDay);
						}
					}
					foreach(System.Windows.Shapes.Path path in TextCalendarControls.FindAll(monthCalendar).OfType<System.Windows.Shapes.Path>()) {
						AssertBrush(listFailures,name+" chevron",path.Fill,isEnabled ? EnumUiColor.Text : EnumUiColor.TextDisabled);
					}
					AssertBrush(listFailures,name+" header text",monthCalendar.Foreground,isEnabled ? EnumUiColor.Text : EnumUiColor.TextDisabled);
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void CalendarGlyph_IsTheFluentCalendar() {
			IconDesign iconDesign=UiCalendarPaint.GetCalendarDesign(12);
			Assert.IsNotNull(iconDesign,"EnumIcons."+UiCalendarPaint.ICON_CALENDAR+" has no design.");
			StringAssert.StartsWith(iconDesign.FileName,"calendar_ltr_12");
			Assert.IsTrue(UiCalendarPaint.GetCalendarDesign(24).Size>=16,"A larger slot (the 200% picker) takes a larger design.");
		}
		#endregion WPF

		#region Helpers
		private static void CheckTextIn(List<string> listFailures,string name,CalendarPicture calendarPictureChrome,CalendarPicture calendarPictureText,
			Rectangle rectangle,Color colorText)
		{
			rectangle.Intersect(new Rectangle(Point.Empty,calendarPictureChrome.Bitmap.Size));
			using(Bitmap bitmapChrome=calendarPictureChrome.Bitmap.Clone(rectangle,PixelFormat.Format32bppArgb))
			using(Bitmap bitmapText=calendarPictureText.Bitmap.Clone(rectangle,PixelFormat.Format32bppArgb)) {
				ControlStateTests.CheckText(listFailures,name,bitmapChrome,bitmapText,colorText);
			}
		}

		///<summary>The rectangle of the text's ink (pixels well away from white) inside the box less 3 px at each side (its borders and focus line), with
		///the text in black in both looks so only its place counts.</summary>
		private static Rectangle GetTextInk(string kind,bool isPinned) {
			System.Windows.Controls.Control controlInner;
			System.Windows.FrameworkElement frameworkElement=TextCalendarControls.MakeWpfText(kind,TextPictureState.Rest,withText:true,isPinned,out controlInner);
			controlInner.Foreground=System.Windows.Media.Brushes.Black;
			frameworkElement.UpdateLayout();
			using(Bitmap bitmap=ControlPictures.RenderWpf(frameworkElement,frameworkElement.Width,frameworkElement.Height,1f)) {
				return GetInk(bitmap,Rectangle.Inflate(new Rectangle(Point.Empty,bitmap.Size),-3,-3));
			}
		}

		private static Rectangle GetMultilineInk(bool isPinned) {
			WpfControls.UI.TextBox textBox=new WpfControls.UI.TextBox { Width=120,Height=36,IsMultiline=true,Text="Line one\r\nLine two" };
			if(isPinned) {
				WpfControls.UI.ContentLook.SetIsPinned(textBox,true);
			}
			TextCalendarControls.GetInner(textBox).Foreground=System.Windows.Media.Brushes.Black;
			using(Bitmap bitmap=ControlPictures.RenderWpf(textBox,120,36,1f)) {
				return GetInk(bitmap,Rectangle.Inflate(new Rectangle(Point.Empty,bitmap.Size),-3,-3));
			}
		}

		private static Rectangle GetDatePickerInk(bool isWindowsTemplate) {
			WpfControls.UI.DatePicker datePicker=new WpfControls.UI.DatePicker { Width=100,Height=20,Text="11/18/2026" };
			System.Windows.Controls.TextBox textBox=ListStateControls.GetPrivate<System.Windows.Controls.TextBox>(datePicker,"textBox");
			if(isWindowsTemplate) {
				textBox.ClearValue(System.Windows.Controls.Control.TemplateProperty);
			}
			textBox.Foreground=System.Windows.Media.Brushes.Black;
			using(Bitmap bitmap=ControlPictures.RenderWpf(datePicker,100,20,1f)) {
				//Left of the text box's own right border and the glyph's column.
				return GetInk(bitmap,Rectangle.FromLTRB(3,3,bitmap.Width-14-3,bitmap.Height-3));
			}
		}

		private static Rectangle GetInk(Bitmap bitmap,Rectangle rectangleArea) {
			int left=int.MaxValue;
			int top=int.MaxValue;
			int right=int.MinValue;
			int bottom=int.MinValue;
			for(int y=rectangleArea.Top;y<rectangleArea.Bottom;y++) {
				for(int x=rectangleArea.Left;x<rectangleArea.Right;x++) {
					Color color=bitmap.GetPixel(x,y);
					if(color.R+color.G+color.B>3*200) {
						continue;
					}
					left=Math.Min(left,x);
					top=Math.Min(top,y);
					right=Math.Max(right,x);
					bottom=Math.Max(bottom,y);
				}
			}
			if(left==int.MaxValue) {
				return Rectangle.Empty;
			}
			return Rectangle.FromLTRB(left,top,right+1,bottom+1);
		}

		private static System.Windows.Setter GetSetter(System.Windows.Controls.ControlTemplate controlTemplate,System.Windows.DependencyProperty dependencyPropertyTrigger,
			object value,string targetName,System.Windows.DependencyProperty dependencyPropertySet)
		{
			System.Windows.Setter setter=controlTemplate.Triggers.OfType<System.Windows.Trigger>()
				.Where(x => x.Property==dependencyPropertyTrigger && Equals(x.Value,value))
				.SelectMany(x => x.Setters.OfType<System.Windows.Setter>())
				.LastOrDefault(x => x.TargetName==targetName && x.Property==dependencyPropertySet);
			Assert.IsNotNull(setter,"The template has no "+dependencyPropertyTrigger.Name+"="+value+" trigger setting "+dependencyPropertySet.Name+" on "+(targetName??"the box")+".");
			return setter;
		}

		private static void AssertTrigger(System.Windows.Controls.ControlTemplate controlTemplate,System.Windows.DependencyProperty dependencyPropertyTrigger,object value,
			string targetName,System.Windows.DependencyProperty dependencyPropertySet,EnumUiColor uiColor)
		{
			System.Windows.Setter setter=GetSetter(controlTemplate,dependencyPropertyTrigger,value,targetName,dependencyPropertySet);
			Assert.AreEqual(SheetPainter.Light(uiColor).ToArgb(),ToDrawing(((System.Windows.Media.SolidColorBrush)setter.Value).Color).ToArgb(),
				dependencyPropertyTrigger.Name+"="+value+" gives "+uiColor+".");
		}

		private static void AssertBrush(List<string> listFailures,string name,System.Windows.Media.Brush brush,EnumUiColor uiColor) {
			System.Windows.Media.SolidColorBrush solidColorBrush=brush as System.Windows.Media.SolidColorBrush;
			if(solidColorBrush==null || ToDrawing(solidColorBrush.Color).ToArgb()!=SheetPainter.Light(uiColor).ToArgb()) {
				listFailures.Add(name+": expected "+uiColor+", got "+(solidColorBrush==null ? "no solid brush" : solidColorBrush.Color.ToString())+".");
			}
		}

		private static Color ToDrawing(System.Windows.Media.Color color) {
			return Color.FromArgb(color.A,color.R,color.G,color.B);
		}
		#endregion Helpers
	}
}
