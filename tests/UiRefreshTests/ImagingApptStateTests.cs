// The control state pictures of package UI-4c-1 (docs/specs/ui-refresh-plan.md, section 7's UI-4c-1 line and section 3.6; docs/port-notes/UI-4c.md):
// the WinForms ToggleDayWeek, PinBoard, ControlApptProvSlider, UnmountedBar, WindowingSlider and ZoomSlider and the WPF ImageDockHeader, UnmountedBar,
// WindowingSlider and ZoomSlider, each drawn off screen in every state and compared with the token drawing of the state at the control's own geometry
// (ImagingApptDrawings) by ChartImageCompare's tolerance, as UI-4a to UI-4b-3 do: WinForms every pixel, WPF the solid pixels. Then with text: the
// text and glyph pixels reach their token. The WPF ImageSelector and the floating windows' menu are checked at their rows and snap areas, pixel by
// pixel and through their brushes. What the plan keeps (the appointments on the pinboard, the providers' colours, the windowing ramp, the mount's
// colour) is checked as kept. No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToothChartImageTests;

namespace UiRefreshTests {
	[TestClass]
	public class ImagingApptStateTests {
		#region Tokens
		[TestMethod]
		public void SliderPaint_TokensAndGlyphs_AsChosen() {
			Assert.AreEqual(EnumUiColor.SurfaceRaised,UiSliderPaint.GetHandleFill(EnumUiButtonState.Normal));
			Assert.AreEqual(EnumUiColor.ButtonHover,UiSliderPaint.GetHandleFill(EnumUiButtonState.Hover));
			Assert.AreEqual(EnumUiColor.ButtonPressed,UiSliderPaint.GetHandleFill(EnumUiButtonState.Pressed));
			Assert.AreEqual(EnumUiColor.ButtonHover,UiSliderPaint.GetHandleFill(EnumUiButtonState.Disabled));
			Assert.AreEqual(EnumUiColor.StrokeStrong,UiSliderPaint.GetHandleOutline(EnumUiButtonState.Normal));
			Assert.AreEqual(EnumUiColor.StrokeStrongHover,UiSliderPaint.GetHandleOutline(EnumUiButtonState.Hover));
			Assert.AreEqual(EnumUiColor.Accent,UiSliderPaint.GetHandleOutline(EnumUiButtonState.Pressed));
			Assert.AreEqual(EnumUiColor.Stroke,UiSliderPaint.GetHandleOutline(EnumUiButtonState.Disabled));
			Assert.IsNull(UiSliderPaint.GetCellFill(EnumUiButtonState.Normal));
			Assert.AreEqual(EnumUiColor.SubtleHover,UiSliderPaint.GetCellFill(EnumUiButtonState.Hover));
			Assert.AreEqual(EnumUiColor.SubtlePressed,UiSliderPaint.GetCellFill(EnumUiButtonState.Pressed));
			Assert.IsNull(UiSliderPaint.GetCellFill(EnumUiButtonState.Disabled));
			//The plus is EnumIcons.Add's Fluent icon (add), found through its manifest row, so no icon is added for it.
			IconDesign iconDesign=UiSliderPaint.GetAddDesign(12);
			Assert.IsNotNull(iconDesign,"EnumIcons.Add has no design.");
			StringAssert.StartsWith(iconDesign.FileName,"add_","The zoom slider's plus is not Fluent add: "+iconDesign.FileName);
		}
		#endregion Tokens

		#region WinForms
		[TestMethod]
		public void WinForms_ToggleDayWeek_EveryState_AsTheTokenDrawing() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(ChromeState state in ImagingApptControls.ArrayToggleStates) {
					string name="WinForms ToggleDayWeek "+state;
					using(ChromePicture chromePicture=ImagingApptControls.RenderToggle(state,false,1f))
					using(ChromePicture chromePictureText=ImagingApptControls.RenderToggle(state,true,1f))
					using(Bitmap bitmapExpected=ImagingApptDrawings.Toggle(chromePicture,1f)) {
						ControlStateTests.CompareAll(listFailures,name,bitmapExpected,chromePicture.Bitmap);
						for(int i=0;i<2;i++) {
							CheckTextIn(listFailures,name+" row "+i,chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R("row"+i),SheetPainter.Light(EnumUiColor.Text));
						}
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_PinBoard_EmptyAndWithAppointments_AsTheTokenDrawing() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(ChromeState state in ImagingApptControls.ArrayPinBoardStates) {
					string name="WinForms PinBoard "+state;
					using(ChromePicture chromePicture=ImagingApptControls.RenderPinBoard(state,false,1f))
					using(ChromePicture chromePictureText=ImagingApptControls.RenderPinBoard(state,true,1f))
					using(Bitmap bitmapExpected=ImagingApptDrawings.PinBoard(chromePicture,1f)) {
						ControlStateTests.CompareAll(listFailures,name,bitmapExpected,chromePicture.Bitmap);
						if(state==ChromeState.Empty) {
							CheckTextIn(listFailures,name+" hint",chromePicture.Bitmap,chromePictureText.Bitmap,new Rectangle(Point.Empty,chromePicture.Bitmap.Size),
								SheetPainter.Light(EnumUiColor.TextSecondary));
						}
						foreach(Bitmap bitmap in chromePicture.V<List<Bitmap>>("bitmaps").Concat(chromePictureText.V<List<Bitmap>>("bitmaps"))) {
							bitmap.Dispose();
						}
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_ProvSlider_EveryState_AsTheTokenDrawing() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(ChromeState state in ImagingApptControls.ArrayProvSliderStates) {
					string name="WinForms ControlApptProvSlider "+state;
					using(ChromePicture chromePicture=ImagingApptControls.RenderProvSlider(state,false,1f))
					using(ChromePicture chromePictureText=ImagingApptControls.RenderProvSlider(state,true,1f))
					using(Bitmap bitmapExpected=ImagingApptDrawings.ProvSlider(chromePicture,state,1f)) {
						ControlStateTests.CompareAll(listFailures,name,bitmapExpected,chromePicture.Bitmap);
						CheckTextIn(listFailures,name+" length",chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R("slider"),SheetPainter.Light(EnumUiColor.Text));
						int h=(int)chromePicture.V<float>("heightRow");
						CheckTextIn(listFailures,name+" provider bar text",chromePicture.Bitmap,chromePictureText.Bitmap,new Rectangle(0,0,chromePicture.Bitmap.Width,h*4),
							SheetPainter.Light(EnumUiColor.Text));
					}
				}
				//The providers' colours are the office's (data colours): the cells keep them.
				using(ChromePicture chromePicture=ImagingApptControls.RenderProvSlider(ChromeState.Rest,false,1f)) {
					int h=(int)chromePicture.V<float>("heightRow");
					int m=(int)chromePicture.V<float>("marginLR");
					Assert.AreEqual(chromePicture.V<Color>("colorProv").ToArgb(),chromePicture.Bitmap.GetPixel(m+3,h/2).ToArgb(),"The provider's colour changed.");
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_UnmountedBar_HeaderAndOutline_AsTheTokenDrawing() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				using(ChromePicture chromePicture=ImagingApptControls.RenderUnmountedBar(false,1f))
				using(ChromePicture chromePictureText=ImagingApptControls.RenderUnmountedBar(true,1f))
				using(Bitmap bitmapExpected=ImagingApptDrawings.UnmountedBar(chromePicture,false,1f)) {
					LeaveOut(bitmapExpected,chromePicture.Bitmap,chromePicture.ListExcluded);
					ControlStateTests.CompareAll(listFailures,"WinForms UnmountedBar",bitmapExpected,chromePicture.Bitmap);
					CheckTextIn(listFailures,"WinForms UnmountedBar title",chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R("header"),
						SheetPainter.Light(EnumUiColor.TextOnAccent));
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_WindowingSlider_EveryState_AsTheTokenDrawing() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(ChromeState state in ImagingApptControls.ArrayWindowingStates) {
					string name="WinForms WindowingSlider "+state;
					using(ChromePicture chromePicture=ImagingApptControls.RenderWindowingSlider(state,1f))
					using(Bitmap bitmapExpected=ImagingApptDrawings.WindowingSlider(chromePicture,state,1f)) {
						ControlStateTests.CompareAll(listFailures,name,bitmapExpected,chromePicture.Bitmap);
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_ZoomSlider_EveryState_AsTheTokenDrawing() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(ChromeState state in ImagingApptControls.ArrayZoomStates) {
					string name="WinForms ZoomSlider "+state;
					using(ChromePicture chromePicture=ImagingApptControls.RenderZoomSlider(state,false,1f))
					using(ChromePicture chromePictureText=ImagingApptControls.RenderZoomSlider(state,true,1f))
					using(Bitmap bitmapExpected=ImagingApptDrawings.ZoomSlider(chromePicture,state,1f)) {
						ControlStateTests.CompareAll(listFailures,name,bitmapExpected,chromePicture.Bitmap);
						Color colorText=SheetPainter.Light(state==ChromeState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text);
						CheckTextIn(listFailures,name+" Fit",chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R("_rectangleFit"),colorText);
						CheckTextIn(listFailures,name+" 100",chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R("_rectangle100"),colorText);
						CheckTextIn(listFailures,name+" value",chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R("_rectangleSlider"),colorText);
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}
		#endregion WinForms

		#region WPF
		[TestMethod]
		public void Wpf_ImageDockHeader_EveryState_AsTheDesignSheetsFloatingWindows() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(ChromeState state in ImagingApptControls.ArrayDockHeaderStates) {
					string name="WPF ImageDockHeader "+state;
					using(ChromePicture chromePicture=ImagingApptControls.RenderWpfDockHeader(state,false,1f))
					using(ChromePicture chromePictureText=ImagingApptControls.RenderWpfDockHeader(state,true,1f))
					using(Bitmap bitmapExpected=ImagingApptDrawings.DockHeader(chromePicture,state,1f)) {
						ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,chromePicture.Bitmap);
						if(state==ChromeState.Empty) {
							ControlStateTests.CompareSolid(listFailures,name+" (nothing shows)",bitmapExpected,chromePictureText.Bitmap);
							continue;
						}
						foreach(string glyph in new[] { "pathMin","pathMax","pathClose" }) {
							CheckGlyph(listFailures,name+" "+glyph,chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R(glyph),
								chromePictureText.V<System.Windows.Media.Brush>(glyph+"Fill"),SheetPainter.Light(ImagingApptDrawings.DockHeaderText(state,isClose:glyph=="pathClose")));
						}
						CheckTextIn(listFailures,name+" title",chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R("labelTitle"),
							SheetPainter.Light(ImagingApptDrawings.DockHeaderText(state,isClose:false)));
						CheckTextIn(listFailures,name+" Windows",chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R("labelWin"),
							SheetPainter.Light(state==ChromeState.WindowsOpen ? EnumUiColor.Text : ImagingApptDrawings.DockHeaderText(state,isClose:false)));
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_UnmountedBar_HeaderAndBackground_AsTheTokenDrawing() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				using(ChromePicture chromePicture=ImagingApptControls.RenderWpfUnmountedBar(false,1f))
				using(ChromePicture chromePictureText=ImagingApptControls.RenderWpfUnmountedBar(true,1f))
				using(Bitmap bitmapExpected=ImagingApptDrawings.UnmountedBar(chromePicture,true,1f)) {
					LeaveOut(bitmapExpected,chromePicture.Bitmap,chromePicture.ListExcluded);
					ControlStateTests.CompareSolid(listFailures,"WPF UnmountedBar",bitmapExpected,chromePicture.Bitmap);
					CheckTextIn(listFailures,"WPF UnmountedBar title",chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R("title"),
						SheetPainter.Light(EnumUiColor.TextOnAccent));
				}
				//The images sit on the mount's own colour (the office's): their outlines stay gray 100 and the selected one orange.
				WpfControls.UI.UnmountedBar unmountedBar=new WpfControls.UI.UnmountedBar();
				unmountedBar.SetObjects(new List<WpfControls.UI.UnmountedObj> { new WpfControls.UI.UnmountedObj(),new WpfControls.UI.UnmountedObj() });
				ImagingApptControls.Invoke(unmountedBar,"SetSelectedIndex",1);
				System.Windows.Controls.StackPanel stackPanel=ListStateControls.GetPrivate<System.Windows.Controls.StackPanel>(unmountedBar,"stackPanel");
				List<System.Windows.Media.Color> listColors=stackPanel.Children.OfType<System.Windows.Controls.Border>()
					.Select(x => ((System.Windows.Media.SolidColorBrush)x.BorderBrush).Color).ToList();
				CollectionAssert.AreEqual(new[] { System.Windows.Media.Color.FromRgb(100,100,100),System.Windows.Media.Colors.Orange },listColors);
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_WindowingSlider_EveryState_AsTheTokenDrawing() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(ChromeState state in ImagingApptControls.ArrayWindowingStates) {
					string name="WPF WindowingSlider "+state;
					using(ChromePicture chromePicture=ImagingApptControls.RenderWpfWindowingSlider(state,1f))
					using(Bitmap bitmapExpected=ImagingApptDrawings.WpfWindowingSlider(chromePicture,state,1f)) {
						ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,chromePicture.Bitmap);
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_ZoomSlider_EveryState_AsTheTokenDrawing() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(ChromeState state in ImagingApptControls.ArrayZoomStates) {
					string name="WPF ZoomSlider "+state;
					using(ChromePicture chromePicture=ImagingApptControls.RenderWpfZoomSlider(state,false,1f))
					using(ChromePicture chromePictureText=ImagingApptControls.RenderWpfZoomSlider(state,true,1f))
					using(Bitmap bitmapExpected=ImagingApptDrawings.WpfZoomSlider(chromePicture,state,1f)) {
						ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,chromePicture.Bitmap);
						Color colorText=SheetPainter.Light(state==ChromeState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Text);
						foreach(string glyph in new[] { "pathMinus","pathPlus" }) {
							CheckGlyph(listFailures,name+" "+glyph,chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R(glyph),
								chromePictureText.V<System.Windows.Media.Brush>(glyph+"Fill"),colorText);
						}
						foreach(string part in new[] { "textBlock","textFit","text100" }) {
							CheckTextIn(listFailures,name+" "+part,chromePicture.Bitmap,chromePictureText.Bitmap,chromePicture.R(part),colorText);
						}
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_ImageSelector_RowsSelectionBarAndChevrons() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(int idxHover in new[] { -1,1 }) {
					WpfControls.UI.ImageSelector imageSelector=ImagingApptControls.MakeImageSelector(idxHover);
					System.Windows.Controls.StackPanel stackPanel=ListStateControls.GetPrivate<System.Windows.Controls.StackPanel>(imageSelector,"stackPanel");
					List<System.Windows.Controls.Border> listRows=stackPanel.Children.OfType<System.Windows.Controls.Border>().ToList();
					//Radiographs, its two items, Photos (collapsed), Empty
					Assert.AreEqual(5,listRows.Count,"The synthetic tree has 5 rows.");
					EnumUiColor[] arrayFills=new EnumUiColor[] { EnumUiColor.SurfaceSubtle,idxHover==1 ? EnumUiColor.RowHover : EnumUiColor.SurfaceRaised,EnumUiColor.AccentSubtle,
						EnumUiColor.SurfaceSubtle,EnumUiColor.SurfaceSubtle };
					using(Bitmap bitmap=ControlPictures.RenderWpf(imageSelector,227,200,1f)) {
						for(int i=0;i<listRows.Count;i++) {
							string name="WPF ImageSelector row "+i+(idxHover>=0 ? " (hover on row 1)" : "");
							Color colorFill=SheetPainter.Light(arrayFills[i]);
							AssertBrush(listFailures,name+" fill",colorFill,listRows[i].Background);
							Rectangle rectangleRow=TabSplitControls.GetRectangle(listRows[i],imageSelector,1f);
							//A point of the row's own background: between the icon column and the text, away from the top line.
							Point pointBack=new Point(rectangleRow.X+(i==0 || i>=3 ? 36 : 18),rectangleRow.Y+rectangleRow.Height-3);
							AssertPixel(listFailures,name+" background",colorFill,bitmap.GetPixel(pointBack.X,pointBack.Y));
							//The selection bar in the row's first 2 px inside the selector's 1 px StrokeStrong outline: SelectionBarNoFocus (the selector has
							//no keyboard focus here).
							Color colorBar=i==2 ? SheetPainter.Light(EnumUiColor.SelectionBarNoFocus) : colorFill;
							int yMiddle=rectangleRow.Y+2;//above the row's icon, below its top line
							AssertPixel(listFailures,name+" outline",SheetPainter.Light(EnumUiColor.StrokeStrong),bitmap.GetPixel(rectangleRow.X,yMiddle));
							AssertPixel(listFailures,name+" first pixel",colorBar,bitmap.GetPixel(rectangleRow.X+1,yMiddle));
							AssertPixel(listFailures,name+" second pixel",colorBar,bitmap.GetPixel(rectangleRow.X+2,yMiddle));
							if(i==2) {
								AssertPixel(listFailures,name+" after the bar",colorFill,bitmap.GetPixel(rectangleRow.X+3,rectangleRow.Y+2));
							}
							//The owner's answers of 2026-11-18: a category's top line is StrokeStrong, and an item's icon starts clear of the 2 px bar, so the
							//bar's columns hold only the bar (or the fill) over the whole row.
							if(i==0 || i==3) {
								AssertPixel(listFailures,name+" category line",SheetPainter.Light(EnumUiColor.StrokeStrong),bitmap.GetPixel(rectangleRow.X+36,rectangleRow.Y));
							}
							else {
								for(int y=rectangleRow.Y+1;y<rectangleRow.Y+rectangleRow.Height;y++) {
									AssertPixel(listFailures,name+" bar column 1 at y "+y,colorBar,bitmap.GetPixel(rectangleRow.X+1,y));
									AssertPixel(listFailures,name+" bar column 2 at y "+y,colorBar,bitmap.GetPixel(rectangleRow.X+2,y));
								}
							}
						}
					}
					//The expanders: Fluent chevrons in TextSecondary, down while expanded, right while collapsed; none without items.
					string[] arrayExpected=new string[] { "down",null,null,"right",null };
					for(int i=0;i<listRows.Count;i++) {
						System.Windows.Shapes.Path pathChevron=FindChevron(listRows[i]);
						if(arrayExpected[i]==null) {
							if(pathChevron!=null) {
								listFailures.Add("WPF ImageSelector row "+i+": a chevron where none belongs.");
							}
							continue;
						}
						if(pathChevron==null) {
							listFailures.Add("WPF ImageSelector row "+i+": no chevron.");
							continue;
						}
						System.Windows.Media.Geometry geometryExpected=arrayExpected[i]=="down" ? WpfControls.UI.UiLookLists.GeometryChevronDown : WpfControls.UI.UiLookLists.GeometryChevronRight;
						if(pathChevron.Data.ToString()!=geometryExpected.ToString()) {
							listFailures.Add("WPF ImageSelector row "+i+": the chevron is not "+arrayExpected[i]+".");
						}
						AssertBrush(listFailures,"WPF ImageSelector row "+i+" chevron",SheetPainter.Light(EnumUiColor.TextSecondary),pathChevron.Fill);
					}
					System.Windows.Controls.Border borderOutline=ListStateControls.GetPrivate<System.Windows.Controls.Border>(imageSelector,"borderOutline");
					AssertBrush(listFailures,"WPF ImageSelector outline",SheetPainter.Light(EnumUiColor.StrokeStrong),borderOutline.BorderBrush);
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_FloatWindows_MenuAndSnapAreas() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				System.Windows.Shapes.Path pathRest;
				System.Windows.Shapes.Path pathHover;
				System.Windows.Controls.Grid grid=ImagingApptControls.MakeFloatWindowsGrid(out pathRest,out pathHover);
				using(Bitmap bitmap=ControlPictures.RenderWpf(grid,345,195,1f)) {
					AssertPixel(listFailures,"menu border",SheetPainter.Light(EnumUiColor.Stroke),bitmap.GetPixel(0,60));
					AssertPixel(listFailures,"menu border right",SheetPainter.Light(EnumUiColor.Stroke),bitmap.GetPixel(344,60));
					AssertPixel(listFailures,"menu background",SheetPainter.Light(EnumUiColor.SurfaceRaised),bitmap.GetPixel(300,20));
					AssertPixel(listFailures,"snap area",SheetPainter.Light(EnumUiColor.SurfaceSubtle),bitmap.GetPixel(28,55));
					AssertPixel(listFailures,"snap area outline",SheetPainter.Light(EnumUiColor.StrokeStrong),bitmap.GetPixel(10,55));
					AssertPixel(listFailures,"snap area under the mouse",SheetPainter.Light(EnumUiColor.AccentSubtle),bitmap.GetPixel(68,55));
					AssertPixel(listFailures,"snap area under the mouse, outline",SheetPainter.Light(EnumUiColor.Accent),bitmap.GetPixel(50,55));
				}
				AssertBrush(listFailures,"rest fill",SheetPainter.Light(EnumUiColor.SurfaceSubtle),pathRest.Fill);
				AssertBrush(listFailures,"hover fill",SheetPainter.Light(EnumUiColor.AccentSubtle),pathHover.Fill);
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}
		#endregion WPF

		#region Helpers
		///<summary>The text's pixels inside the rectangle (where the picture with text differs from the one without) include pixels of the colour.</summary>
		internal static void CheckTextIn(List<string> listFailures,string name,Bitmap bitmapChrome,Bitmap bitmapText,Rectangle rectangle,Color colorText) {
			rectangle.Intersect(new Rectangle(Point.Empty,bitmapChrome.Size));
			if(rectangle.Width<=0 || rectangle.Height<=0) {
				listFailures.Add(name+": the rectangle is outside the picture.");
				return;
			}
			using(Bitmap bitmapChromePart=bitmapChrome.Clone(rectangle,PixelFormat.Format32bppArgb))
			using(Bitmap bitmapTextPart=bitmapText.Clone(rectangle,PixelFormat.Format32bppArgb)) {
				ControlStateTests.CheckText(listFailures,name,bitmapChromePart,bitmapTextPart,colorText);
			}
		}

		///<summary>A glyph: it draws (its pixels differ from the picture without it) in a brush of the token. Its strokes are 1 px or thinner, so
		///antialiasing leaves few pixels of the full colour; the brush says which colour WPF blends.</summary>
		internal static void CheckGlyph(List<string> listFailures,string name,Bitmap bitmapChrome,Bitmap bitmapText,Rectangle rectangle,System.Windows.Media.Brush brush,
			Color colorToken)
		{
			rectangle.Intersect(new Rectangle(Point.Empty,bitmapChrome.Size));
			int countInk=0;
			for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
				for(int x=rectangle.Left;x<rectangle.Right;x++) {
					if(bitmapChrome.GetPixel(x,y).ToArgb()!=bitmapText.GetPixel(x,y).ToArgb()) {
						countInk++;
					}
				}
			}
			if(countInk==0) {
				listFailures.Add(name+": the glyph draws nothing.");
			}
			AssertBrush(listFailures,name+" brush",colorToken,brush);
		}

		///<summary>Copies the actual picture's pixels of the regions into the expected one: child controls with a look of their own are left out.</summary>
		internal static void LeaveOut(Bitmap bitmapExpected,Bitmap bitmapActual,List<Rectangle> listRectangles) {
			foreach(Rectangle rectangle in listRectangles) {
				Rectangle rectangleIn=Rectangle.Intersect(rectangle,new Rectangle(Point.Empty,bitmapExpected.Size));
				for(int y=rectangleIn.Top;y<rectangleIn.Bottom;y++) {
					for(int x=rectangleIn.Left;x<rectangleIn.Right;x++) {
						bitmapExpected.SetPixel(x,y,bitmapActual.GetPixel(x,y));
					}
				}
			}
		}

		private static void AssertPixel(List<string> listFailures,string name,Color colorExpected,Color colorActual) {
			if(!ControlStateTests.IsNear(colorExpected,colorActual,ChartImageCompare.ChannelTolerance)) {
				listFailures.Add(name+": expected "+ColorTranslator.ToHtml(colorExpected)+", got "+ColorTranslator.ToHtml(Color.FromArgb(colorActual.R,colorActual.G,colorActual.B))+".");
			}
		}

		private static void AssertBrush(List<string> listFailures,string name,Color colorExpected,System.Windows.Media.Brush brush) {
			if(!(brush is System.Windows.Media.SolidColorBrush solidColorBrush)) {
				listFailures.Add(name+": not a solid brush.");
				return;
			}
			System.Windows.Media.Color color=solidColorBrush.Color;
			AssertPixel(listFailures,name,colorExpected,Color.FromArgb(color.A,color.R,color.G,color.B));
		}

		private static System.Windows.Shapes.Path FindChevron(System.Windows.DependencyObject dependencyObject) {
			for(int i=0;i<System.Windows.Media.VisualTreeHelper.GetChildrenCount(dependencyObject);i++) {
				System.Windows.DependencyObject child=System.Windows.Media.VisualTreeHelper.GetChild(dependencyObject,i);
				if(child is System.Windows.Shapes.Path path && path.Name=="pathChevron") {
					return path;
				}
				System.Windows.Shapes.Path pathFound=FindChevron(child);
				if(pathFound!=null) {
					return pathFound;
				}
			}
			return null;
		}
		#endregion Helpers
	}
}
