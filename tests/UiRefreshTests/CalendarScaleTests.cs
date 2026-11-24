// UI-CAL-2 (docs/port-notes/UI-CAL.md, "UI-CAL-2"): the calendar at 125%, 150% and 175% is the 100% calendar scaled, its size, its day cells,
// its header, the weekdays, the dates and the Today line by the one factor (MonthCalendarOD.GetFontDraw: the text in pixels at the calendar's
// scale, not in points at the graphics' dpi), inline (a calendar a window lays out) and as a date picker's drop-down in a program window
// (FormODBase) and in a plain window (the graphs' controls), and the drop-down's place on the screen (UiDateDropLook.GetDropBounds). Off screen;
// nothing is shown; no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;

namespace UiRefreshTests {
	///<summary>A program window (FormODBase) with one stock date picker in the window's font, as a designer file makes it.</summary>
	public class TestWindowPicker : FormODBase {
		public DateTimePicker DateTimePickerOne;

		public TestWindowPicker() {
			DateTimePickerOne=new DateTimePicker();
			DateTimePickerOne.Format=DateTimePickerFormat.Short;
			DateTimePickerOne.Location=new Point(10,10);
			DateTimePickerOne.Name="dateTimePicker";
			DateTimePickerOne.Size=new Size(100,20);
			Name="TestWindowPicker";
			ClientSize=new Size(300,240);
			Controls.Add(DateTimePickerOne);
			InitializeLayoutManager();
		}
	}

	[TestClass]
	public class CalendarScaleTests {
		private static readonly float[] _arrayScales=new float[] { 1f,1.25f,1.5f,1.75f };
		///<summary>A month far from today, so neither today's ring nor the selected day's fill is in the cells measured.</summary>
		private static readonly DateTime _dateMonth=new DateTime(2020,5,1);

		#region The calendar's measures
		///<summary>Where the text of a calendar is, as drawn: the ink (pixels darker than the background: the text, also the other months' days in
		///TextDisabled, not the hover fill, the grid line or the outline) of each weekday cell and date cell, of the month's name and of the Today
		///line, each as its bounds from where the calendar draws it: a cell's from the cell's left and the top its text is drawn from (the cell's
		///top as drawn, a whole pixel), the month's name from the calendar's top left, the Today line from the footer's top left. So the 100%
		///calendar's whole-pixel rounding of the cells' tops is not scaled into the expected place.</summary>
		private class CalendarInk {
			public Size Size;
			public float SizeFontPixels;
			public RectangleF[] ArrayCells=new RectangleF[49];
			///<summary>The number of characters of each cell's text (the weekday's short name, the day's digits).</summary>
			public int[] ArrayGlyphs=new int[49];
			public RectangleF RectangleMonth;
			public RectangleF RectangleToday;
		}

		private static CalendarInk Measure(MonthCalendarOD monthCalendarOD) {
			monthCalendarOD.SetDate(new DateTime(2020,3,10));//selected in March, May showing: no day of May filled
			typeof(MonthCalendarOD).GetField("_dateMonthShowing",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(monthCalendarOD,_dateMonth);
			CalendarInk calendarInk=new CalendarInk();
			calendarInk.Size=monthCalendarOD.Size;
			Font fontDraw=(Font)typeof(MonthCalendarOD).GetMethod("GetFontDraw",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(monthCalendarOD,null);
			Assert.AreEqual(GraphicsUnit.Pixel,fontDraw.Unit,"the text in pixels at the calendar's scale");
			calendarInk.SizeFontPixels=fontDraw.Size;
			using Bitmap bitmap=new Bitmap(monthCalendarOD.Width,monthCalendarOD.Height,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				MethodInfo methodInfo=typeof(MonthCalendarOD).GetMethod("OnPaint",BindingFlags.Instance|BindingFlags.NonPublic);
				methodInfo.Invoke(monthCalendarOD,new object[] { new PaintEventArgs(g,new Rectangle(0,0,monthCalendarOD.Width,monthCalendarOD.Height)) });
			}
			float[] arrayX=(float[])typeof(MonthCalendarOD).GetField("_xPos",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(monthCalendarOD);
			float[] arrayY=(float[])typeof(MonthCalendarOD).GetField("_yPos",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(monthCalendarOD);
			float heightCell=(float)typeof(MonthCalendarOD).GetField("_heightCell",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(monthCalendarOD);
			float widthCell=monthCalendarOD.Width/7f;
			DateTime[,] arrayDates=(DateTime[,])typeof(MonthCalendarOD).GetField("_arrayDates",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(monthCalendarOD);
			Color colorBack=UiTheme.GetColor(EnumUiColor.SurfaceRaised);
			for(int r=0;r<7;r++) {
				for(int c=0;c<7;c++) {
					//The cell's inside, a pixel in from its edges (the grid line under the weekdays is above the first row of dates).
					Rectangle rectangleCell=Rectangle.FromLTRB((int)Math.Ceiling(arrayX[c])+1,(int)Math.Ceiling(arrayY[r])+1,
						(int)(arrayX[c]+widthCell)-1,(int)(arrayY[r]+heightCell)-1);
					calendarInk.ArrayCells[r*7+c]=Offset(GetInk(bitmap,rectangleCell,colorBack),arrayX[c],(int)arrayY[r]);
					if(r==0) {
						int day=((int)DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek+c)%7;
						calendarInk.ArrayGlyphs[c]=DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName((DayOfWeek)day).Length;
					}
					else {
						calendarInk.ArrayGlyphs[r*7+c]=arrayDates[c,r-1].Day.ToString().Length;
					}
				}
			}
			int widthArrows=(int)Math.Ceiling(60*monthCalendarOD.LayoutManager.ScaleMy());//the arrows and their letters at both ends of the header
			calendarInk.RectangleMonth=Offset(GetInk(bitmap,Rectangle.FromLTRB(widthArrows,1,monthCalendarOD.Width-widthArrows,(int)arrayY[0]),colorBack),0,0);
			int heightFooter=monthCalendarOD.LayoutManager.Scale(21);
			calendarInk.RectangleToday=Offset(GetInk(bitmap,Rectangle.FromLTRB(2,monthCalendarOD.Height-heightFooter+1,monthCalendarOD.Width-2,monthCalendarOD.Height-2),colorBack),
				0,monthCalendarOD.Height-heightFooter);
			return calendarInk;
		}

		///<summary>The ink's bounds from an origin; empty stays empty.</summary>
		private static RectangleF Offset(Rectangle rectangle,float x,float y) {
			if(rectangle.IsEmpty) {
				return RectangleF.Empty;
			}
			return new RectangleF(rectangle.X-x,rectangle.Y-y,rectangle.Width,rectangle.Height);
		}

		///<summary>The bounds of the ink in the area: pixels whose brightness is well under the background's; empty when none.</summary>
		private static Rectangle GetInk(Bitmap bitmap,Rectangle rectangleArea,Color colorBack) {
			float brightnessBack=GetBrightness(colorBack);
			int left=int.MaxValue,top=int.MaxValue,right=int.MinValue,bottom=int.MinValue;
			for(int y=Math.Max(0,rectangleArea.Top);y<Math.Min(bitmap.Height,rectangleArea.Bottom);y++) {
				for(int x=Math.Max(0,rectangleArea.Left);x<Math.Min(bitmap.Width,rectangleArea.Right);x++) {
					if(brightnessBack-GetBrightness(bitmap.GetPixel(x,y))<30) {
						continue;
					}
					left=Math.Min(left,x);
					top=Math.Min(top,y);
					right=Math.Max(right,x+1);
					bottom=Math.Max(bottom,y+1);
				}
			}
			if(left==int.MaxValue) {
				return Rectangle.Empty;
			}
			return Rectangle.FromLTRB(left,top,right,bottom);
		}

		private static float GetBrightness(Color color) {
			return 0.299f*color.R+0.587f*color.G+0.114f*color.B;
		}

		///<summary>The calendar at the scale is the 100% one scaled: its size within a pixel, its text's pixel size exactly, and every cell's text,
		///the month's name and the Today line where the 100% ones are times the scale and as large, within a pixel of the 100% calendar per cell
		///(that pixel scaled, and half a pixel for the rounding at the scale; a text's centre a pixel more, both its edges rounded): the glyphs' hinting rounds each size's outlines and advances to
		///its own whole pixels, so a 100% glyph is up to a pixel wider, taller or lower than the 175% one over 1.75. A cell's width within a pixel
		///per character; the long texts within 5% of their width. Returns the differences found, empty when none.</summary>
		private static string CompareScaled(CalendarInk calendarInk100,CalendarInk calendarInk,float scale) {
			StringBuilder stringBuilder=new StringBuilder();
			Size sizeExpected=new Size((int)Math.Round(calendarInk100.Size.Width*scale),(int)Math.Round(calendarInk100.Size.Height*scale));
			if(Math.Abs(calendarInk.Size.Width-sizeExpected.Width)>1 || Math.Abs(calendarInk.Size.Height-sizeExpected.Height)>1) {
				stringBuilder.AppendLine("size "+calendarInk.Size+", expected "+sizeExpected);
			}
			if(Math.Abs(calendarInk.SizeFontPixels-calendarInk100.SizeFontPixels*scale)>0.01f) {
				stringBuilder.AppendLine("text "+calendarInk.SizeFontPixels+" px, expected "+calendarInk100.SizeFontPixels*scale);
			}
			for(int i=0;i<calendarInk.ArrayCells.Length;i++) {
				CompareRectangle(stringBuilder,"cell "+i,calendarInk100.ArrayCells[i],calendarInk.ArrayCells[i],scale,1f+scale,0.5f+scale,
					0.5f+scale*Math.Max(1,calendarInk.ArrayGlyphs[i]));
			}
			CompareRectangle(stringBuilder,"month's name",calendarInk100.RectangleMonth,calendarInk.RectangleMonth,scale,
				Math.Max(0.5f+scale,0.025f*calendarInk100.RectangleMonth.Width*scale),0.5f+scale,Math.Max(0.5f+scale,0.05f*calendarInk100.RectangleMonth.Width*scale));
			CompareRectangle(stringBuilder,"Today line",calendarInk100.RectangleToday,calendarInk.RectangleToday,scale,
				Math.Max(0.5f+scale,0.025f*calendarInk100.RectangleToday.Width*scale),0.5f+scale,Math.Max(0.5f+scale,0.05f*calendarInk100.RectangleToday.Width*scale));
			return stringBuilder.ToString();
		}

		private static void CompareRectangle(StringBuilder stringBuilder,string name,RectangleF rectangle100,RectangleF rectangle,float scale,
			float tolerancePlace,float toleranceHeight,float toleranceWidth)
		{
			if(rectangle100.IsEmpty || rectangle.IsEmpty) {
				if(rectangle100.IsEmpty!=rectangle.IsEmpty) {
					stringBuilder.AppendLine(name+": text at one scale only ("+rectangle100+", "+rectangle+")");
				}
				return;
			}
			float[] arrayExpected=new float[] { (rectangle100.Left+rectangle100.Right)/2f*scale,(rectangle100.Top+rectangle100.Bottom)/2f*scale,
				rectangle100.Width*scale,rectangle100.Height*scale };
			float[] arrayFound=new float[] { (rectangle.Left+rectangle.Right)/2f,(rectangle.Top+rectangle.Bottom)/2f,rectangle.Width,rectangle.Height };
			string[] arrayNames=new string[] { "centre x","centre y","width","height" };
			for(int i=0;i<4;i++) {
				float toleranceOne=i<2 ? tolerancePlace : (i==2 ? toleranceWidth : toleranceHeight);
				if(Math.Abs(arrayFound[i]-arrayExpected[i])>toleranceOne) {
					stringBuilder.AppendLine(name+" "+arrayNames[i]+" "+arrayFound[i]+", expected "+arrayExpected[i].ToString("0.0")+" (100%: "+rectangle100+", now "+rectangle+")");
				}
			}
		}
		#endregion The calendar's measures

		#region Calendars at each scale
		///<summary>A calendar a window lays out (its LayoutManager at the scale), in the body font as LayoutManager sets it (the zoom only).</summary>
		private static MonthCalendarOD MakeInline(float scale) {
			MonthCalendarOD monthCalendarOD=new MonthCalendarOD();
			monthCalendarOD.Font=UiTheme.FontBody;
			monthCalendarOD.LayoutManager.SetScaleMS(scale);
			monthCalendarOD.Size=monthCalendarOD.LayoutManager.ScaleSize(monthCalendarOD.GetDefaultSize());
			return monthCalendarOD;
		}

		[TestMethod]
		public void Calendar_AtEachScale_IsThe100PercentCalendarScaled() {
			IconRendererTests.RunSta(() => {
				CalendarInk calendarInk100;
				using(MonthCalendarOD monthCalendarOD=MakeInline(1f)) {
					calendarInk100=Measure(monthCalendarOD);
				}
				Assert.AreEqual(new Size(227,162),calendarInk100.Size);
				Assert.AreEqual(UiTheme.FontBody.SizeInPoints*96f/72f,calendarInk100.SizeFontPixels,0.01f,"100%: the body font's size");
				Assert.IsFalse(calendarInk100.RectangleMonth.IsEmpty || calendarInk100.RectangleToday.IsEmpty,"the month's name and the Today line drawn");
				StringBuilder stringBuilder=new StringBuilder();
				foreach(float scale in _arrayScales) {
					using MonthCalendarOD monthCalendarOD=MakeInline(scale);
					string differences=CompareScaled(calendarInk100,Measure(monthCalendarOD),scale);
					if(differences!="") {
						stringBuilder.AppendLine("At "+scale*100+"%:").Append(differences);
					}
				}
				Assert.AreEqual("",stringBuilder.ToString());
			});
		}

		[TestMethod]
		public void Calendar_TextDoesNotFollowTheGraphicsDpi_ButTheCalendarsScale() {
			IconRendererTests.RunSta(() => {
				//A window's calendar keeps the font LayoutManager recorded for it at 96 dpi, whatever its font now (on a 150% screen its font is the
				//zoom's only; the gallery harness scales it as Windows draws it).
				using MonthCalendarOD monthCalendarOD=MakeInline(1.5f);
				float size96=monthCalendarOD.GetFontSize96Drawn();
				monthCalendarOD.FontSize96=size96;
				monthCalendarOD.Font=new Font(UiTheme.FontBody.FontFamily,UiTheme.FontBody.Size*1.5f);
				CalendarInk calendarInk=Measure(monthCalendarOD);
				Assert.AreEqual(size96*96f/72f*1.5f,calendarInk.SizeFontPixels,0.01f);
			});
		}

		[TestMethod]
		public void DropDown_InAProgramWindow_IsThe100PercentCalendarScaled() {
			IconRendererTests.RunSta(() => {
				LayoutManagerForms.SetLooksScale();
				CalendarInk calendarInk100;
				using(MonthCalendarOD monthCalendarOD=MakeInline(1f)) {
					calendarInk100=Measure(monthCalendarOD);
				}
				StringBuilder stringBuilder=new StringBuilder();
				foreach(float scale in _arrayScales) {
					using TestWindowPicker testWindowPicker=new TestWindowPicker();
					LayoutManagerForms layoutManagerForms=testWindowPicker.GetLayoutManager();
					layoutManagerForms.SetScaleMS(scale);
					//As the gallery and Windows' dpi draw the picker's own text: its font grown by the scale. The calendar keeps the 96 dpi size.
					DateTimePicker dateTimePicker=testWindowPicker.DateTimePickerOne;
					dateTimePicker.Font=new Font(dateTimePicker.Font.FontFamily,dateTimePicker.Font.Size*scale);
					using MonthCalendarOD monthCalendarOD=(MonthCalendarOD)LayoutManagerForms.CreateDropCalendar(dateTimePicker);
					Assert.AreSame(layoutManagerForms,monthCalendarOD.LayoutManager,"the window's scale");
					Assert.AreEqual(UiTheme.FontBody.SizeInPoints,monthCalendarOD.GetFontSize96Drawn(),0.01f,"the picker's 96 dpi font");
					string differences=CompareScaled(calendarInk100,Measure(monthCalendarOD),scale);
					if(differences!="") {
						stringBuilder.AppendLine("At "+scale*100+"%:").Append(differences);
					}
				}
				Assert.AreEqual("",stringBuilder.ToString());
			});
		}

		[TestMethod]
		public void DropDown_InAPlainWindow_IsThe100PercentCalendarScaled() {
			IconRendererTests.RunSta(() => {
				LayoutManagerForms.SetLooksScale();
				CalendarInk calendarInk100;
				using(MonthCalendarOD monthCalendarOD=MakeInline(1f)) {
					calendarInk100=Measure(monthCalendarOD);
				}
				StringBuilder stringBuilder=new StringBuilder();
				try {
					foreach(float scale in _arrayScales) {
						LayoutManagerForms.FuncGetScalePlainForTest=control => scale;
						using Form form=new Form();
						using DateTimePicker dateTimePicker=new DateTimePicker();
						//A plain window on the screen at the scale, drawn at 96 dpi: the picker's text as large as Windows draws it there.
						dateTimePicker.Font=new Font(UiTheme.FontBody.FontFamily,UiTheme.FontBody.Size*scale);
						form.Controls.Add(dateTimePicker);
						using MonthCalendarOD monthCalendarOD=(MonthCalendarOD)LayoutManagerForms.CreateDropCalendar(dateTimePicker);
						Assert.AreEqual(scale,monthCalendarOD.LayoutManager.ScaleMy(),0.001f,"the plain window's scale, no zoom");
						Assert.AreEqual(UiTheme.FontBody.SizeInPoints,monthCalendarOD.GetFontSize96Drawn(),0.01f,"the picker's font brought back to 96 dpi");
						string differences=CompareScaled(calendarInk100,Measure(monthCalendarOD),scale);
						if(differences!="") {
							stringBuilder.AppendLine("At "+scale*100+"%:").Append(differences);
						}
					}
				}
				finally {
					LayoutManagerForms.FuncGetScalePlainForTest=null;
				}
				Assert.AreEqual("",stringBuilder.ToString());
			});
		}

		[TestMethod]
		public void DropDown_PlainWindowFont_BroughtBackTo96Dpi_OnAnyMonitor() {
			//Windows draws a font in points at its system dpi (144 on a 150% primary monitor): a 9 pt picker there is 18 px high in a 150% window.
			Assert.AreEqual(9f,LayoutManagerForms.GetFontSize96Plain(new Font("Segoe UI",9f),1.5f,144f),0.001f);
			//The window moved to a 100% monitor: Windows Forms scales the font down to 6 pt (12 px at 144) in the 100% window.
			Assert.AreEqual(9f,LayoutManagerForms.GetFontSize96Plain(new Font("Segoe UI",6f),1f,144f),0.001f);
			//All at 100%, and a font in pixels (its size is its height whatever the dpi).
			Assert.AreEqual(9f,LayoutManagerForms.GetFontSize96Plain(new Font("Segoe UI",9f),1f,96f),0.001f);
			Assert.AreEqual(9f,LayoutManagerForms.GetFontSize96Plain(new Font("Segoe UI",18f,FontStyle.Regular,GraphicsUnit.Pixel),1.5f,144f),0.001f);
		}
		#endregion Calendars at each scale

		#region The drop-down's place on the screen
		[TestMethod]
		public void DropDown_Placement_StaysWhollyOnTheScreen() {
			Rectangle rectangleWorkingArea=new Rectangle(0,0,1920,1040);
			Size sizeCalendar=new Size(341,243);//the 150% calendar
			//Room below: below the box, at its left edge (its right edge when aligned right).
			Rectangle rectanglePicker=new Rectangle(100,100,150,30);
			Assert.AreEqual(new Rectangle(100,130,341,243),UiDateDropLook.GetDropBounds(rectanglePicker,sizeCalendar,rectangleWorkingArea,false));
			Assert.AreEqual(new Rectangle(550-341,130,341,243),UiDateDropLook.GetDropBounds(new Rectangle(400,100,150,30),sizeCalendar,rectangleWorkingArea,true),
				"aligned right: the calendar's right edge at the box's");
			//Near the bottom: flipped above the box.
			rectanglePicker=new Rectangle(100,900,150,30);
			Assert.AreEqual(new Rectangle(100,900-243,341,243),UiDateDropLook.GetDropBounds(rectanglePicker,sizeCalendar,rectangleWorkingArea,false));
			//Near the right edge: moved left to end at the screen's edge.
			rectanglePicker=new Rectangle(1800,100,110,30);
			Assert.AreEqual(new Rectangle(1920-341,130,341,243),UiDateDropLook.GetDropBounds(rectanglePicker,sizeCalendar,rectangleWorkingArea,false));
			//Near the bottom right corner: above and moved left.
			rectanglePicker=new Rectangle(1800,1000,110,30);
			Assert.AreEqual(new Rectangle(1920-341,1000-243,341,243),UiDateDropLook.GetDropBounds(rectanglePicker,sizeCalendar,rectangleWorkingArea,false));
			//Aligned right near the left edge: moved right to start at the screen's edge.
			rectanglePicker=new Rectangle(10,100,150,30);
			Assert.AreEqual(new Rectangle(0,130,341,243),UiDateDropLook.GetDropBounds(rectanglePicker,sizeCalendar,rectangleWorkingArea,true));
			//A short screen with room neither below nor above: as low as it fits wholly.
			Rectangle rectangleShort=new Rectangle(0,0,800,400);
			Assert.AreEqual(new Rectangle(100,400-243,341,243),UiDateDropLook.GetDropBounds(new Rectangle(100,200,150,30),sizeCalendar,rectangleShort,false));
			//A second monitor left of the primary (negative coordinates) with a taskbar at its top: the same rules in its working area.
			Rectangle rectangleSecond=new Rectangle(-2560,40,2560,1400);
			Rectangle rectangleDrop=UiDateDropLook.GetDropBounds(new Rectangle(-200,1380,180,30),sizeCalendar,rectangleSecond,false);
			Assert.AreEqual(new Rectangle(-341,1380-243,341,243),rectangleDrop);
			Assert.IsTrue(rectangleSecond.Contains(rectangleDrop));
			rectangleDrop=UiDateDropLook.GetDropBounds(new Rectangle(-2555,45,150,30),sizeCalendar,rectangleSecond,true);
			Assert.AreEqual(new Rectangle(-2560,75,341,243),rectangleDrop);
		}

		[TestMethod]
		public void DropDown_Placement_EveryPickerOnTheScreen_KeepsTheCalendarOnIt() {
			//Every place of a picker on a 1920 x 1040 working area, a 125% calendar and a 175% one: the calendar is wholly on the screen and
			//touches the box (below or above it) whenever there is room for that.
			Rectangle rectangleWorkingArea=new Rectangle(0,0,1920,1040);
			List<string> listFailed=new List<string>();
			foreach(Size sizeCalendar in new[] { new Size(284,203),new Size(397,284) }) {
				for(int x=0;x<=1920-120;x+=60) {
					for(int y=0;y<=1040-26;y+=26) {
						foreach(bool isAlignRight in new[] { false,true }) {
							Rectangle rectanglePicker=new Rectangle(x,y,120,26);
							Rectangle rectangleDrop=UiDateDropLook.GetDropBounds(rectanglePicker,sizeCalendar,rectangleWorkingArea,isAlignRight);
							if(!rectangleWorkingArea.Contains(rectangleDrop)) {
								listFailed.Add(rectanglePicker+" "+isAlignRight+": "+rectangleDrop+" off the screen");
							}
							bool isRoom=rectanglePicker.Bottom+sizeCalendar.Height<=1040 || rectanglePicker.Top-sizeCalendar.Height>=0;
							if(isRoom && rectangleDrop.Top!=rectanglePicker.Bottom && rectangleDrop.Bottom!=rectanglePicker.Top) {
								listFailed.Add(rectanglePicker+" "+isAlignRight+": "+rectangleDrop+" away from the box");
							}
						}
					}
				}
			}
			Assert.AreEqual(0,listFailed.Count,string.Join(Environment.NewLine,listFailed.GetRange(0,Math.Min(20,listFailed.Count))));
		}
		#endregion The drop-down's place on the screen
	}
}
