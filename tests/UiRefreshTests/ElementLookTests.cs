// UI-13 (docs/port-notes/UI-13.md, step 3): the unit tests of the shared looks every element type now draws with: the scroll bar
// (UiScrollPaint, UiScrollLook, WPF's UiLookScroll), the slider (UiSliderPaint's track bar part, UiTrackBarLook), the stock button, check box,
// grey block, progress bar, border, calendar, data grid view and link label looks (UiLooks) and the time pattern (TableTimeBar): the colours
// come from the tokens, the sizes scale with the bar or the scale, and the drawing puts the tokens where the shape is. No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;

namespace UiRefreshTests {
	[TestClass]
	public class ElementLookTests {
		#region Scroll bar
		[TestMethod]
		public void ScrollPaint_Tokens_AsChosen() {
			Assert.AreEqual(EnumUiColor.Surface,UiScrollPaint.GetTrackColor());
			Assert.AreEqual(EnumUiColor.StrokeStrong,UiScrollPaint.GetThumbColor(EnumUiButtonState.Normal));
			Assert.AreEqual(EnumUiColor.TextSecondary,UiScrollPaint.GetThumbColor(EnumUiButtonState.Hover));
			Assert.AreEqual(EnumUiColor.Text,UiScrollPaint.GetThumbColor(EnumUiButtonState.Pressed));
			Assert.AreEqual(EnumUiColor.StrokeStrong,UiScrollPaint.GetArrowColor(EnumUiButtonState.Normal));
			Assert.AreEqual(EnumUiColor.TextSecondary,UiScrollPaint.GetArrowColor(EnumUiButtonState.Hover));
			Assert.AreEqual(EnumUiColor.Text,UiScrollPaint.GetArrowColor(EnumUiButtonState.Pressed));
			Assert.AreEqual(EnumUiColor.TextDisabled,UiScrollPaint.GetArrowColor(EnumUiButtonState.Disabled));
			Assert.IsNull(UiScrollPaint.GetArrowFill(EnumUiButtonState.Normal));
			Assert.AreEqual(EnumUiColor.SubtleHover,UiScrollPaint.GetArrowFill(EnumUiButtonState.Hover));
			Assert.AreEqual(EnumUiColor.SubtlePressed,UiScrollPaint.GetArrowFill(EnumUiButtonState.Pressed));
		}

		[TestMethod]
		public void ScrollPaint_Sizes_ScaleWithTheBar() {
			Assert.AreEqual(1f,UiScrollPaint.GetScale(17));
			Assert.AreEqual(25/17f,UiScrollPaint.GetScale(25),0.0001f);
			Assert.AreEqual(1f,UiScrollPaint.GetScale(10),"Never under 1.");
			//A 17 px bar (100%): the thumb 7 px thick (5 px each side), 2 px from each end of its hit rectangle.
			Rectangle rectangleThumb=UiScrollPaint.GetThumbShape(new Rectangle(0,40,17,60),isVertical:true,1f);
			Assert.AreEqual(new Rectangle(5,42,7,56),rectangleThumb);
			//A 26 px bar (150%): the insets scale (8 and 3 px), the thumb 10 px thick.
			float scale=UiScrollPaint.GetScale(26);
			rectangleThumb=UiScrollPaint.GetThumbShape(new Rectangle(0,60,26,90),isVertical:true,scale);
			Assert.AreEqual(10,rectangleThumb.Width);
			Assert.AreEqual(63,rectangleThumb.Y);
			//Horizontal: the same across the height.
			rectangleThumb=UiScrollPaint.GetThumbShape(new Rectangle(40,0,60,17),isVertical:false,1f);
			Assert.AreEqual(new Rectangle(42,5,56,7),rectangleThumb);
			//The chevron's square scales and stays centred.
			Assert.AreEqual(new Rectangle(4,4,8,8),UiScrollPaint.GetArrowGlyphRectangle(new Rectangle(0,0,17,17),1f));
			Assert.AreEqual(12,UiScrollPaint.GetArrowGlyphRectangle(new Rectangle(0,0,26,26),scale).Width);
		}

		[TestMethod]
		public void ScrollPaint_GetPart_FollowsWindowsGeometry() {
			Rectangle rectangleBar=new Rectangle(0,0,17,200);
			Assert.AreEqual(EnumUiScrollPart.ArrowStart,UiScrollPaint.GetPart(new Point(8,5),rectangleBar,true,17,40,80));
			Assert.AreEqual(EnumUiScrollPart.TrackStart,UiScrollPaint.GetPart(new Point(8,30),rectangleBar,true,17,40,80));
			Assert.AreEqual(EnumUiScrollPart.Thumb,UiScrollPaint.GetPart(new Point(8,60),rectangleBar,true,17,40,80));
			Assert.AreEqual(EnumUiScrollPart.TrackEnd,UiScrollPaint.GetPart(new Point(8,120),rectangleBar,true,17,40,80));
			Assert.AreEqual(EnumUiScrollPart.ArrowEnd,UiScrollPaint.GetPart(new Point(8,195),rectangleBar,true,17,40,80));
			Assert.AreEqual(EnumUiScrollPart.None,UiScrollPaint.GetPart(new Point(30,60),rectangleBar,true,17,40,80));
			Assert.AreEqual(EnumUiButtonState.Pressed,UiScrollPaint.GetPartState(EnumUiScrollPart.Thumb,true,EnumUiScrollPart.ArrowEnd,EnumUiScrollPart.Thumb));
			Assert.AreEqual(EnumUiButtonState.Normal,UiScrollPaint.GetPartState(EnumUiScrollPart.ArrowEnd,true,EnumUiScrollPart.ArrowEnd,EnumUiScrollPart.Thumb));
			Assert.AreEqual(EnumUiButtonState.Hover,UiScrollPaint.GetPartState(EnumUiScrollPart.ArrowEnd,true,EnumUiScrollPart.ArrowEnd,EnumUiScrollPart.None));
			Assert.AreEqual(EnumUiButtonState.Disabled,UiScrollPaint.GetPartState(EnumUiScrollPart.Thumb,false,EnumUiScrollPart.Thumb,EnumUiScrollPart.None));
		}

		[TestMethod]
		public void ScrollPaint_Draw_TrackThumbAndChevronsInTheTokens() {
			foreach(int thickness in new int[] { 17,26 }) {
				using(Bitmap bitmap=new Bitmap(thickness,200,PixelFormat.Format24bppRgb))
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.Magenta);
					UiScrollPaint.Draw(g,new Rectangle(0,0,thickness,200),true,true,thickness,60,120,EnumUiScrollPart.None,EnumUiScrollPart.None,Color.Empty);
					AssertColour(UiTheme.GetColor(EnumUiColor.Surface),bitmap.GetPixel(1,thickness+10),"the track at "+thickness);
					AssertColour(UiTheme.GetColor(EnumUiColor.StrokeStrong),bitmap.GetPixel(thickness/2,90),"the thumb at "+thickness);
					AssertColour(UiTheme.GetColor(EnumUiColor.Surface),bitmap.GetPixel(1,90),"the track beside the thumb at "+thickness);
					Assert.IsTrue(HasDarker(bitmap,new Rectangle(0,0,thickness,thickness),200),"No chevron in the start arrow at "+thickness);
					Assert.IsFalse(HasColour(bitmap,new Rectangle(0,0,thickness,200),Color.Magenta),"Not every pixel painted at "+thickness);
				}
			}
			//Disabled: no thumb.
			using(Bitmap bitmap=new Bitmap(17,200,PixelFormat.Format24bppRgb))
			using(Graphics g=Graphics.FromImage(bitmap)) {
				UiScrollPaint.Draw(g,new Rectangle(0,0,17,200),true,false,17,60,120,EnumUiScrollPart.None,EnumUiScrollPart.None,Color.Empty);
				AssertColour(UiTheme.GetColor(EnumUiColor.Surface),bitmap.GetPixel(8,90),"a disabled bar's track where the thumb was");
			}
		}

		[TestMethod]
		public void ScrollLook_IsScrolled_TheScrollingTypes() {
			Assert.IsTrue(UiScrollLook.IsScrolled(new VScrollBar()));
			Assert.IsTrue(UiScrollLook.IsScrolled(new HScrollBar()));
			Assert.IsTrue(UiScrollLook.IsScrolled(new TextBox { Multiline=true }));
			Assert.IsFalse(UiScrollLook.IsScrolled(new TextBox()));
			Assert.IsTrue(UiScrollLook.IsScrolled(new RichTextBox()));
			Assert.IsTrue(UiScrollLook.IsScrolled(new System.Windows.Forms.ListBox()));
			Assert.IsTrue(UiScrollLook.IsScrolled(new TreeView()));
			Assert.IsTrue(UiScrollLook.IsScrolled(new ListView()));
			Assert.IsTrue(UiScrollLook.IsScrolled(new Panel { AutoScroll=true }));
			Assert.IsFalse(UiScrollLook.IsScrolled(new Panel()));
			Assert.IsFalse(UiScrollLook.IsScrolled(new Form { AutoScroll=true }),"A window draws its own frame.");
		}

		[TestMethod]
		public void ScrollLook_PaintsAStockScrollBarInTheTokens() {
			StaRunnerHelpers.RunSta(() => {
				using(Form form=new Form())
				using(VScrollBar vScrollBar=new VScrollBar { Bounds=new Rectangle(0,0,17,200),Minimum=0,Maximum=100,LargeChange=20,Value=40 }) {
					form.Controls.Add(vScrollBar);
					UiScrollLook.Attach(vScrollBar);
					IntPtr handle=vScrollBar.Handle;
					Assert.IsTrue(UiScrollLook.IsAttached(vScrollBar));
					using(Bitmap bitmap=new Bitmap(17,200,PixelFormat.Format24bppRgb)) {
						vScrollBar.DrawToBitmap(bitmap,new Rectangle(0,0,17,200));
						//No hatching: every track pixel is Surface, and the thumb has StrokeStrong.
						Assert.IsTrue(HasColour(bitmap,new Rectangle(0,17,17,166),UiTheme.GetColor(EnumUiColor.StrokeStrong)),"No thumb in the token.");
						AssertColour(UiTheme.GetColor(EnumUiColor.Surface),bitmap.GetPixel(1,30),"the track");
						AssertColour(UiTheme.GetColor(EnumUiColor.Surface),bitmap.GetPixel(1,170),"the track");
					}
				}
			});
		}

		[TestMethod]
		public void LookScroll_Wpf_DictionaryHasTheImplicitScrollBarStyle() {
			StaRunnerHelpers.RunSta(() => {
				System.Windows.ResourceDictionary resourceDictionary=WpfControls.UI.UiLookScroll.GetDictionary();
				Assert.IsNotNull(resourceDictionary,"The dictionary's XAML did not parse.");
				object style=resourceDictionary[typeof(System.Windows.Controls.Primitives.ScrollBar)];
				Assert.IsInstanceOfType(style,typeof(System.Windows.Style));
				StringAssert.Contains(WpfControls.UI.UiLookScroll.GetXaml(),UiTheme.GetHex(EnumUiColor.StrokeStrong));
				StringAssert.Contains(WpfControls.UI.UiLookScroll.GetXaml(),UiTheme.GetHex(EnumUiColor.Surface));
				//A ScrollBar under an element with the dictionary takes the template.
				System.Windows.Controls.Grid grid=new System.Windows.Controls.Grid();
				WpfControls.UI.UiLookScroll.AddTo(grid);
				WpfControls.UI.UiLookScroll.AddTo(grid);
				Assert.AreEqual(1,grid.Resources.MergedDictionaries.Count,"Merged once.");
				System.Windows.Controls.Primitives.ScrollBar scrollBar=new System.Windows.Controls.Primitives.ScrollBar();
				grid.Children.Add(scrollBar);
				grid.Measure(new System.Windows.Size(17,200));
				grid.Arrange(new System.Windows.Rect(0,0,17,200));
				Assert.AreSame(style,scrollBar.Style);
			});
		}
		#endregion Scroll bar

		#region Slider
		[TestMethod]
		public void SliderPaint_TrackBarTokens_AsChosen() {
			Assert.AreEqual(EnumUiColor.Stroke,UiSliderPaint.GetSliderTrackColor());
			Assert.AreEqual(EnumUiColor.Accent,UiSliderPaint.GetSliderFillColor(true));
			Assert.AreEqual(EnumUiColor.TextDisabled,UiSliderPaint.GetSliderFillColor(false));
			Assert.AreEqual(EnumUiColor.Accent,UiSliderPaint.GetSliderDotColor(EnumUiButtonState.Normal));
			Assert.AreEqual(EnumUiColor.AccentHover,UiSliderPaint.GetSliderDotColor(EnumUiButtonState.Hover));
			Assert.AreEqual(EnumUiColor.AccentPressed,UiSliderPaint.GetSliderDotColor(EnumUiButtonState.Pressed));
			Assert.AreEqual(EnumUiColor.TextDisabled,UiSliderPaint.GetSliderDotColor(EnumUiButtonState.Disabled));
			Assert.AreEqual(EnumUiColor.StrokeStrong,UiSliderPaint.GetSliderTickColor(true));
		}

		[TestMethod]
		public void SliderPaint_TrackBarSizes_Scale() {
			Assert.AreEqual(4,UiSliderPaint.GetSliderRingWidth(EnumUiButtonState.Normal,1f));
			Assert.AreEqual(3,UiSliderPaint.GetSliderRingWidth(EnumUiButtonState.Hover,1f));
			Assert.AreEqual(5,UiSliderPaint.GetSliderRingWidth(EnumUiButtonState.Pressed,1f));
			Assert.AreEqual(6,UiSliderPaint.GetSliderRingWidth(EnumUiButtonState.Normal,1.5f));
			Assert.AreEqual(new Rectangle(42,2,16,16),UiSliderPaint.GetSliderThumbRectangle(new Point(50,10),1f));
			Assert.AreEqual(24,UiSliderPaint.GetSliderThumbRectangle(new Point(50,10),1.5f).Width);
			Assert.AreEqual(new Rectangle(8,8,104,4),UiSliderPaint.GetSliderTrackRectangle(new Point(10,10),new Point(110,10),false,1f));
			Assert.AreEqual(6,UiSliderPaint.GetSliderTrackRectangle(new Point(10,10),new Point(110,10),false,1.5f).Height);
		}

		[TestMethod]
		public void SliderPaint_Draw_TrackFillAndThumbInTheTokens() {
			foreach(float scale in new float[] { 1f,1.5f }) {
				using(Bitmap bitmap=new Bitmap(200,40,PixelFormat.Format24bppRgb))
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.White);
					UiSliderPaint.DrawSliderTrack(g,new Point(10,20),new Point(190,20),new Point(100,20),false,true,scale);
					UiSliderPaint.DrawSliderThumb(g,new Point(100,20),EnumUiButtonState.Normal,false,scale);
					AssertColour(UiTheme.GetColor(EnumUiColor.Accent),bitmap.GetPixel(50,20),"the filled track at "+scale);
					AssertColour(UiTheme.GetColor(EnumUiColor.Stroke),bitmap.GetPixel(150,20),"the track at "+scale);
					AssertColour(UiTheme.GetColor(EnumUiColor.Accent),bitmap.GetPixel(100,20),"the thumb's dot at "+scale);
					AssertColour(UiTheme.GetColor(EnumUiColor.SurfaceRaised),bitmap.GetPixel(100,20-UiTheme.ScaleI(6,scale)),"the thumb's ring at "+scale);
				}
			}
		}

		[TestMethod]
		public void TrackBarLook_PaintsTheSliderInTheTokens() {
			StaRunnerHelpers.RunSta(() => {
				using(Form form=new Form())
				using(TrackBar trackBar=new TrackBar { AutoSize=false,Bounds=new Rectangle(0,0,300,27),Minimum=0,Maximum=10,Value=5 }) {
					form.Controls.Add(trackBar);
					UiTrackBarLook.Attach(trackBar);
					IntPtr handle=trackBar.Handle;
					using(Bitmap bitmap=new Bitmap(300,27,PixelFormat.Format24bppRgb)) {
						trackBar.DrawToBitmap(bitmap,new Rectangle(0,0,300,27));
						Assert.IsTrue(HasColour(bitmap,new Rectangle(0,0,140,27),UiTheme.GetColor(EnumUiColor.Accent)),"No Accent fill left of the thumb.");
						Assert.IsTrue(HasColour(bitmap,new Rectangle(170,0,130,27),UiTheme.GetColor(EnumUiColor.Stroke)),"No Stroke track right of the thumb.");
						Assert.IsFalse(HasColour(bitmap,new Rectangle(0,0,300,27),Color.Black),"Windows' black ticks or channel remain.");
					}
				}
			});
		}
		#endregion Slider

		#region Other stock types
		[TestMethod]
		public void ButtonLook_SwatchAndDrawnTypes() {
			Assert.IsTrue(UiButtonLook.IsDrawn(new System.Windows.Forms.Button()));
			Assert.IsFalse(UiButtonLook.IsDrawn(new OpenPmsDental.UI.Button()),"The program's own button draws itself.");
			Assert.IsFalse(UiButtonLook.IsDrawn(new System.Windows.Forms.Button { FlatStyle=FlatStyle.System }),"Windows draws a System button.");
			Assert.IsFalse(UiButtonLook.IsSwatch(new System.Windows.Forms.Button { UseVisualStyleBackColor=true }));
			Assert.IsTrue(UiButtonLook.IsSwatch(new System.Windows.Forms.Button { BackColor=Color.RoyalBlue,UseVisualStyleBackColor=false }));
			Assert.IsFalse(UiButtonLook.IsSwatch(new System.Windows.Forms.Button { BackColor=Color.Silver,UseVisualStyleBackColor=false,Text="Edit" }),
				"A button with a text on a colour is a button.");
			using(System.Windows.Forms.Button button=new System.Windows.Forms.Button()) {
				Assert.IsTrue(UiButtonLook.IsDrawn(button));
				button.Paint+=(sender,e) => { };
				Assert.IsTrue(UiButtonLook.IsPaintedByOwner(button));
				Assert.IsFalse(UiButtonLook.IsDrawn(button),"A button its owner paints (the colour picker's swatch) keeps its owner's picture.");
			}
			Assert.IsTrue(UiButtonLook.IsSwatch(new System.Windows.Forms.Button { BackColor=SystemColors.ControlDark,UseVisualStyleBackColor=false }),"The time pattern's handle.");
			Assert.IsFalse(UiButtonLook.IsSwatch(new System.Windows.Forms.Button { BackColor=SystemColors.Control,UseVisualStyleBackColor=false }));
			Assert.IsTrue(UiBlockLook.IsDrawn(new Label { BackColor=Color.LightGray }));
			Assert.IsFalse(UiBlockLook.IsDrawn(new Label()));
			Assert.IsTrue(UiCheckLook.IsDrawn(new System.Windows.Forms.CheckBox()));
			Assert.IsFalse(UiCheckLook.IsDrawn(new OpenPmsDental.UI.CheckBox()));
			Assert.IsTrue(UiBorderLook.IsDrawn(new Panel { BorderStyle=BorderStyle.Fixed3D }));
			Assert.IsFalse(UiBorderLook.IsDrawn(new Panel()));
		}

		[TestMethod]
		public void ButtonLook_PaintsTheButtonLookAndTheSwatch() {
			{
				using(System.Windows.Forms.Button button=new System.Windows.Forms.Button { Bounds=new Rectangle(0,0,80,24),Text="OK" })
				using(Bitmap bitmap=new Bitmap(80,24,PixelFormat.Format24bppRgb))
				using(Graphics g=Graphics.FromImage(bitmap)) {
					UiButtonLook.PaintButton(g,button,EnumUiButtonState.Normal,false);
					AssertColour(UiTheme.GetColor(EnumUiColor.SurfaceRaised),bitmap.GetPixel(6,4),"the button's fill");
					AssertColour(UiTheme.GetColor(EnumUiColor.Stroke),bitmap.GetPixel(40,0),"the button's edge");
				}
				using(System.Windows.Forms.Button button=new System.Windows.Forms.Button { Bounds=new Rectangle(0,0,24,24),BackColor=Color.RoyalBlue,UseVisualStyleBackColor=false })
				using(Bitmap bitmap=new Bitmap(24,24,PixelFormat.Format24bppRgb))
				using(Graphics g=Graphics.FromImage(bitmap)) {
					UiButtonLook.PaintButton(g,button,EnumUiButtonState.Normal,false);
					AssertColour(Color.RoyalBlue,bitmap.GetPixel(12,12),"the swatch's colour");
					AssertColour(UiTheme.GetColor(EnumUiColor.StrokeStrong),bitmap.GetPixel(12,0),"the swatch's edge");
				}
			}
		}

		[TestMethod]
		public void Looks_CalendarGridAndLinkColours_FromTheTokens() {
			using(MonthCalendar monthCalendar=new MonthCalendar()) {
				UiLooks.SetMonthCalendarLook(monthCalendar);
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Accent),monthCalendar.TitleBackColor);
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.TextOnAccent),monthCalendar.TitleForeColor);
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.TextDisabled),monthCalendar.TrailingForeColor);
			}
			using(MonthCalendar monthCalendar=new MonthCalendar { TitleBackColor=Color.Green }) {
				UiLooks.SetMonthCalendarLook(monthCalendar);
				Assert.AreEqual(Color.Green,monthCalendar.TitleBackColor,"A colour the window sets stays.");
			}
			using(DateTimePicker dateTimePicker=new DateTimePicker()) {
				UiLooks.SetDateTimePickerLook(dateTimePicker);
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Accent),dateTimePicker.CalendarTitleBackColor);
			}
			using(DataGridView dataGridView=new DataGridView()) {
				UiLooks.SetDataGridViewLook(dataGridView);
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Gridline),dataGridView.GridColor);
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.SurfaceSubtle),dataGridView.ColumnHeadersDefaultCellStyle.BackColor);
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.AccentSubtle),dataGridView.DefaultCellStyle.SelectionBackColor);
				Assert.IsFalse(dataGridView.EnableHeadersVisualStyles);
			}
			using(LinkLabel linkLabel=new LinkLabel()) {
				UiLooks.SetLinkLabelLook(linkLabel);
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Accent),linkLabel.LinkColor);
			}
		}

		[TestMethod]
		public void ProgressAndBorderLooks_DrawInTheTokens() {
			using(ProgressBar progressBar=new ProgressBar { Bounds=new Rectangle(0,0,100,12),Value=50 })
			using(Bitmap bitmap=new Bitmap(100,12,PixelFormat.Format24bppRgb))
			using(Graphics g=Graphics.FromImage(bitmap)) {
				UiProgressLook.PaintProgress(g,progressBar);
				AssertColour(UiTheme.GetColor(EnumUiColor.Accent),bitmap.GetPixel(25,6),"the value's part");
				AssertColour(UiTheme.GetColor(EnumUiColor.Stroke),bitmap.GetPixel(75,6),"the track");
			}
			using(Panel panel=new Panel { Bounds=new Rectangle(0,0,40,40),BorderStyle=BorderStyle.Fixed3D,BackColor=Color.White })
			using(Bitmap bitmap=new Bitmap(40,40,PixelFormat.Format24bppRgb))
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.Magenta);
				UiBorderLook.PaintBorder(g,panel,new Rectangle(0,0,40,40));
				AssertColour(UiTheme.GetColor(EnumUiColor.StrokeStrong),bitmap.GetPixel(20,0),"the flat edge");
				AssertColour(Color.White,bitmap.GetPixel(20,1),"the second pixel of the 3-D edge in the panel's colour");
			}
		}

		[TestMethod]
		public void TimeBar_ModernLook_FilledCellsInAccent() {
			StaRunnerHelpers.RunSta(() => {
				using(TimeBarProbe timeBarProbe=new TimeBarProbe()) {
					timeBarProbe.Cell[0,0]="X";
					timeBarProbe.Cell[0,1]="X";
					timeBarProbe.Cell[0,2]="/";
					using(Bitmap bitmap=new Bitmap(timeBarProbe.Width,timeBarProbe.Height,PixelFormat.Format24bppRgb))
					using(Graphics g=Graphics.FromImage(bitmap)) {
						timeBarProbe.PaintForTest(g);
						Rectangle rectangleCell1=timeBarProbe.GetCellForTest(1);
						Rectangle rectangleCell2=timeBarProbe.GetCellForTest(2);
						Rectangle rectangleCell5=timeBarProbe.GetCellForTest(5);
						AssertColour(UiTheme.GetColor(EnumUiColor.Accent),bitmap.GetPixel(2,rectangleCell1.Y+3),"a provider cell");
						AssertColour(UiTheme.GetColor(EnumUiColor.AccentSubtle),bitmap.GetPixel(2,rectangleCell2.Y+3),"an assistant cell");
						AssertColour(UiTheme.GetColor(EnumUiColor.SurfaceRaised),bitmap.GetPixel(2,rectangleCell5.Y+3),"an empty cell");
						AssertColour(UiTheme.GetColor(EnumUiColor.StrokeStrong),bitmap.GetPixel(0,timeBarProbe.Height/2),"the frame");
					}
				}
			});
		}

		[TestMethod]
		public void TimeBar_CellsAndSlider_ScaleWithTheWindow() {
			//UI-13 fix round: the time pattern's cells, column and slider steps follow the window's LayoutManager (dpi and zoom), not 14 px always.
			StaRunnerHelpers.RunSta(() => {
				//In a plain window, where LayoutManager's moves of the bar's inner panels set their bounds.
				using(Form form=new Form())
				using(TimeBarProbe timeBarProbe=new TimeBarProbe()) {
					form.Controls.Add(timeBarProbe);
					Assert.AreEqual(14,timeBarProbe.GetCellHeight(),"96 dpi.");
					LayoutManagerForms layoutManagerForms=new LayoutManagerForms();
					layoutManagerForms.SetScaleMS(1.5f);
					timeBarProbe.LayoutManager=layoutManagerForms;
					timeBarProbe.LayoutTables();
					Assert.AreEqual(21,timeBarProbe.GetCellHeight(),"14 px at 150%.");
					Assert.AreEqual(21,timeBarProbe.GetCellForTest(1).Height);
					Assert.AreEqual(layoutManagerForms.Scale(13)+2,timeBarProbe.Width,"The column 13 px at 96 dpi, scaled, in its frame.");
					Assert.AreEqual(timeBarProbe.MaxRows*21+1,timeBarProbe.Height);
					for(int count=1;count<timeBarProbe.MaxRows;count++) {
						Point pointSlider=timeBarProbe.GetSliderLocation(count);
						Assert.AreEqual(count,timeBarProbe.GetSliderStep(pointSlider.Y),"The slider's step at "+count+" cells.");
						Assert.AreEqual(timeBarProbe.Location.Y+count*21+layoutManagerForms.Scale(1),pointSlider.Y);
					}
					timeBarProbe.Cell[0,0]="X";
					timeBarProbe.Cell[0,1]="/";
					using(Bitmap bitmap=new Bitmap(timeBarProbe.Width,timeBarProbe.Height,PixelFormat.Format24bppRgb))
					using(Graphics g=Graphics.FromImage(bitmap)) {
						timeBarProbe.PaintForTest(g);
						Rectangle rectangleCell1=timeBarProbe.GetCellForTest(1);
						AssertColour(UiTheme.GetColor(EnumUiColor.AccentSubtle),bitmap.GetPixel(3,rectangleCell1.Bottom-3),"the scaled assistant cell's bottom");
						AssertColour(UiTheme.GetColor(EnumUiColor.SurfaceRaised),bitmap.GetPixel(3,timeBarProbe.GetCellForTest(2).Y+10),"the next cell");
					}
				}
			});
		}

		[TestMethod]
		public void Looks_Scale_FollowsTheWindowDpi() {
			//UI-13 fix round: the looks take their scale from the painted control's window (FormODBase: its monitor's dpi times the zoom) or its
			//own dpi, not from a LayoutManager without a dpi (which gave 1 on a 150% monitor).
			Assert.AreEqual(1.5f,UiLooks.GetScale(0,144,1f),0.0001f,"A control on a 144 dpi monitor outside a FormODBase.");
			Assert.AreEqual(1.8f,UiLooks.GetScale(0,144,1.2f),0.0001f,"Its dpi times the zoom.");
			Assert.AreEqual(1.25f,UiLooks.GetScale(0,96,1.25f),0.0001f,"The zoom alone at 96 dpi.");
			Assert.AreEqual(1.5f,UiLooks.GetScale(1.5f,96,1f),0.0001f,"The window's scale when it has one.");
			Assert.AreEqual(1f,UiLooks.GetScale(0,72,1f),0.0001f,"Never under 1.");
			StaRunnerHelpers.RunSta(() => {
				using(UiRefreshTests.Scan.HostFormOD hostFormOD=new UiRefreshTests.Scan.HostFormOD(new Size(200,100))) {
					System.Windows.Forms.Button button=new System.Windows.Forms.Button { Bounds=new Rectangle(0,0,80,26),Text="OK" };
					hostFormOD.Controls.Add(button);
					Assert.AreEqual(1f,UiLooks.GetScale(button),0.0001f,"The window at 96 dpi.");
					hostFormOD.GetLayoutManager().SetScaleMS(1.5f);
					Assert.AreEqual(1.5f,UiLooks.GetScale(button),0.0001f,"The window on a 150% monitor (FormODBase's SetScaleMS).");
				}
				using(Form form=new Form())
				using(System.Windows.Forms.CheckBox checkBox=new System.Windows.Forms.CheckBox()) {
					form.Controls.Add(checkBox);
					Assert.AreEqual(Math.Max(1f,checkBox.DeviceDpi/96f),UiLooks.GetScale(checkBox),0.0001f,"A plain window: the control's own dpi.");
				}
			});
		}

		[TestMethod]
		public void ButtonLook_OneLineButton_DoesNotWrapItsCaption() {
			//UI-13 fix round: a caption wider than a one-line button (FormEHR's "Clinical Summary") stays on one line, as Windows drew it, instead of
			//wrapping onto a second line the button's bottom edge cuts.
			using(System.Windows.Forms.Button button=new System.Windows.Forms.Button { Bounds=new Rectangle(0,0,60,26),Text="Clinical Summary" }) {
				TextFormatFlags textFormatFlags=UiButtonLook.GetButtonTextFlags(button,Rectangle.Inflate(button.ClientRectangle,-4,-2));
				Assert.AreNotEqual(TextFormatFlags.WordBreak,textFormatFlags & TextFormatFlags.WordBreak,"No wrap in a 26 px button.");
				Assert.AreEqual(TextFormatFlags.SingleLine,textFormatFlags & TextFormatFlags.SingleLine);
				using(Bitmap bitmap=new Bitmap(60,26,PixelFormat.Format24bppRgb))
				using(Graphics g=Graphics.FromImage(bitmap)) {
					UiButtonLook.PaintButton(g,button,EnumUiButtonState.Normal,false);
					int heightFont=button.Font.Height;
					int yTopLine=(26-heightFont)/2-1;
					int yBottomLine=yTopLine+heightFont+2;
					for(int y=0;y<26;y++) {
						if(y>=yTopLine && y<=yBottomLine) {
							continue;
						}
						Assert.IsFalse(HasDarker(bitmap,new Rectangle(3,y,54,1),110),"Text ink outside the one line, at y "+y+".");
					}
				}
			}
			using(System.Windows.Forms.Button button=new System.Windows.Forms.Button { Bounds=new Rectangle(0,0,60,48),Text="Clinical Summary" }) {
				TextFormatFlags textFormatFlags=UiButtonLook.GetButtonTextFlags(button,Rectangle.Inflate(button.ClientRectangle,-4,-2));
				Assert.AreEqual(TextFormatFlags.WordBreak,textFormatFlags & TextFormatFlags.WordBreak,"A button two lines tall wraps.");
			}
		}

		[TestMethod]
		public void DataGridView_FlatHeaderAndFrame() {
			//UI-13 fix round: without the visual styles a Raised header draws the classic bevel and the frame is black; Single headers (lines in
			//GridColor) and a StrokeStrong frame.
			using(DataGridView dataGridView=new DataGridView { Bounds=new Rectangle(0,0,120,60) }) {
				UiLooks.SetDataGridViewLook(dataGridView);
				Assert.AreEqual(DataGridViewHeaderBorderStyle.Single,dataGridView.ColumnHeadersBorderStyle);
				Assert.AreEqual(DataGridViewHeaderBorderStyle.Single,dataGridView.RowHeadersBorderStyle);
				using(Bitmap bitmap=new Bitmap(120,60,PixelFormat.Format24bppRgb))
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.Black);
					UiLooks.PaintDataGridViewBorder(g,dataGridView);
					AssertColour(UiTheme.GetColor(EnumUiColor.StrokeStrong),bitmap.GetPixel(60,0),"the frame's top");
					AssertColour(UiTheme.GetColor(EnumUiColor.StrokeStrong),bitmap.GetPixel(0,30),"the frame's left");
				}
			}
			using(DataGridView dataGridView=new DataGridView { ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.None }) {
				UiLooks.SetDataGridViewLook(dataGridView);
				Assert.AreEqual(DataGridViewHeaderBorderStyle.None,dataGridView.ColumnHeadersBorderStyle,"A style the window sets stays.");
			}
		}

		[TestMethod]
		public void SeparatorAndCaptionLooks_InTheTokens() {
			//UI-13 fix round: FormInsPlan's 2 px ControlText divider and FormQuery's ActiveCaption grid title.
			using(Panel panelParent=new Panel { BackColor=Color.White })
			using(Panel panel=new Panel { Bounds=new Rectangle(0,0,200,2),BackColor=SystemColors.ControlText }) {
				panelParent.Controls.Add(panel);
				Assert.IsTrue(UiSeparatorLook.IsDrawn(panel));
				Assert.IsFalse(UiSeparatorLook.IsDrawn(new Panel { Bounds=new Rectangle(0,0,200,40),BackColor=SystemColors.ControlText }),"A block, not a line.");
				Assert.IsFalse(UiSeparatorLook.IsDrawn(new Panel { Bounds=new Rectangle(0,0,200,2),BackColor=Color.Red }),"A colour bar.");
				using(Bitmap bitmap=new Bitmap(200,2,PixelFormat.Format24bppRgb))
				using(Graphics g=Graphics.FromImage(bitmap)) {
					UiSeparatorLook.PaintSeparator(g,panel);
					AssertColour(UiTheme.GetColor(EnumUiColor.Stroke),bitmap.GetPixel(100,0),"the line");
					AssertColour(Color.White,bitmap.GetPixel(100,1),"the parent's colour beside it");
				}
			}
			using(Label label=new Label { Bounds=new Rectangle(0,0,200,19),BackColor=SystemColors.ActiveCaption,Text="Query results" }) {
				Assert.IsTrue(UiCaptionLook.IsDrawn(label));
				Assert.IsFalse(UiCaptionLook.IsDrawn(new Label()));
				using(Bitmap bitmap=new Bitmap(200,19,PixelFormat.Format24bppRgb))
				using(Graphics g=Graphics.FromImage(bitmap)) {
					UiCaptionLook.PaintCaption(g,label);
					AssertColour(UiTheme.GetColor(EnumUiColor.Accent),bitmap.GetPixel(190,2),"the title's colour");
				}
			}
		}

		///<summary>TableTimeBar with its look's drawing and cells reachable from the test.</summary>
		private class TimeBarProbe : TableTimeBar {
			public void PaintForTest(Graphics g) {
				PaintModern(g);
			}

			public Rectangle GetCellForTest(int row) {
				return GetCellRectangleLaidOut(0,row);
			}
		}
		#endregion Other stock types

		[TestMethod]
		public void InputFrame_PrintedCorners_InTheParentColour() {
			//UI-13: an input printed through WM_PRINT (an up-down box, a date picker, a rich text box) showed Windows' dark corner pixel under the
			//frame's rounded corner (UI-12.md section 5). The printed corner is the parent's colour.
			StaRunnerHelpers.RunSta(() => {
				using(Form form=new Form { BackColor=Color.White })
				using(NumericUpDown numericUpDown=new NumericUpDown { Bounds=new Rectangle(10,10,90,23) }) {
					form.Controls.Add(numericUpDown);
					UiInputFrame.Attach(numericUpDown);
					IntPtr handle=numericUpDown.Handle;
					using(Bitmap bitmap=new Bitmap(numericUpDown.Width,numericUpDown.Height,PixelFormat.Format24bppRgb)) {
						using(Graphics g=Graphics.FromImage(bitmap)) {
							g.Clear(Color.Magenta);
							IntPtr hdc=g.GetHdc();
							try {
								SendMessage(handle,0x0317,hdc,(IntPtr)(0x2|0x4|0x8));
							}
							finally {
								g.ReleaseHdc(hdc);
							}
						}
						string pixels="";
						for(int y=0;y<3;y++) {
							for(int x=0;x<4;x++) {
								Color color=bitmap.GetPixel(x,y);
								pixels+=" ("+x+","+y+")="+color.R+","+color.G+","+color.B;
							}
						}
						AssertColour(Color.White,bitmap.GetPixel(0,0),"the printed top left corner:"+pixels);
						AssertColour(Color.White,bitmap.GetPixel(bitmap.Width-1,bitmap.Height-1),"the printed bottom right corner");
					}
				}
			});
		}

		[TestMethod]
		public void InputFrame_ThemedDatePicker_WholeButtonCoveredAndTextInTheToken() {
			//UI-13 fix round 2 (review of G-UI3-ui13, FormFamilyBalancer): with Windows' visual styles the date picker's calendar button is wider
			//than the scroll bar width the frame covered, so a clipped Windows glyph (a brown outline, a blue gradient) showed beside the chevron,
			//and the value was pure black. Printed through the frame, the button's area holds only the frame's neutral greys (the fill, the
			//border, the chevron in Text and their blends), and the value's darkest channel is the Text token.
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Text).R,UiInputFrame.RecolorChannel(0,255,UiTheme.GetColor(EnumUiColor.Text).R));
			Assert.AreEqual(255,UiInputFrame.RecolorChannel(255,255,31));
			Assert.AreEqual(143,UiInputFrame.RecolorChannel(128,255,31));
			StaRunnerHelpers.RunSta(() => {
				using VisualStylesScope visualStylesScope=new VisualStylesScope();
				Assert.IsTrue(visualStylesScope.IsActive,"Windows' visual styles could not be activated for the themed date picker.");
				using(Form form=new Form { BackColor=SystemColors.Control })
				using(DateTimePicker dateTimePicker=new DateTimePicker { Bounds=new Rectangle(10,10,132,23),Format=DateTimePickerFormat.Short,
					Value=new DateTime(2026,11,22) })
				{
					form.Controls.Add(dateTimePicker);
					UiInputFrame.Attach(dateTimePicker);
					IntPtr handle=dateTimePicker.Handle;
					Rectangle rectangleButton=UiInputFrame.GetDropButtonRectangle(dateTimePicker);
					Assert.IsFalse(rectangleButton.IsEmpty,"The themed date picker reports no drop button.");
					Assert.IsTrue(rectangleButton.Right<=dateTimePicker.Width && rectangleButton.Left>dateTimePicker.Width/2,
						"The drop button is not at the right: "+rectangleButton);
					Rectangle rectangleInner=UiInputFrame.GetInnerRectangle(new Rectangle(0,0,dateTimePicker.Width,dateTimePicker.Height),dateTimePicker);
					Assert.AreEqual(rectangleButton.Left-1,rectangleInner.Right,"The frame leaves Windows' drawing up to 1 px left of the button.");
					using(Bitmap bitmap=new Bitmap(dateTimePicker.Width,dateTimePicker.Height,PixelFormat.Format24bppRgb)) {
						using(Graphics g=Graphics.FromImage(bitmap)) {
							g.Clear(SystemColors.Control);
							IntPtr hdc=g.GetHdc();
							try {
								SendMessage(handle,0x0317,hdc,(IntPtr)(0x2|0x4|0x8));
							}
							finally {
								g.ReleaseHdc(hdc);
							}
						}
						string offending="";
						int countOffending=0;
						Rectangle rectangleArea=Rectangle.FromLTRB(rectangleButton.Left-1,0,bitmap.Width,bitmap.Height);
						for(int y=rectangleArea.Top;y<rectangleArea.Bottom;y++) {
							for(int x=rectangleArea.Left;x<rectangleArea.Right;x++) {
								Color color=bitmap.GetPixel(x,y);
								int spread=Math.Max(color.R,Math.Max(color.G,color.B))-Math.Min(color.R,Math.Min(color.G,color.B));
								if(spread>4) {
									countOffending++;
									if(countOffending<=8) {
										offending+=" ("+x+","+y+")="+color.R+","+color.G+","+color.B;
									}
								}
							}
						}
						Assert.AreEqual(0,countOffending,"Pixels outside the frame's tokens in the drop button "+rectangleArea+":"+offending);
						Assert.IsTrue(HasDarker(bitmap,rectangleArea,128),"No chevron in the drop button.");
						//The value: drawn (dark pixels), and none darker than the Text token.
						Rectangle rectangleText=Rectangle.Inflate(rectangleInner,0,-1);
						int darkest=255;
						for(int y=rectangleText.Top;y<rectangleText.Bottom;y++) {
							for(int x=rectangleText.Left;x<rectangleText.Right;x++) {
								Color color=bitmap.GetPixel(x,y);
								darkest=Math.Min(darkest,Math.Min(color.R,Math.Min(color.G,color.B)));
							}
						}
						Assert.IsTrue(darkest<120,"No value drawn in the date picker (darkest channel "+darkest+").");
						Assert.IsTrue(darkest>=UiTheme.GetColor(EnumUiColor.Text).R-1,"The value is darker than the Text token: darkest channel "+darkest+".");
					}
				}
			});
		}

		[TestMethod]
		public void ScrollLook_Sizes_FromTheBarRectangle() {
			//UI-13 fix round 2: UiScrollLook draws from the bar Windows reports (GetScrollBarInfo), so a wider bar (25 px, as a 150% monitor's
			//metric gives) gets the thumb and chevrons of its scale, without the 96 dpi metric of the test process.
			int[] arrayThickness=new int[] { 17,25 };
			int[] arrayThumbWidths=new int[2];
			int[] arrayChevronWidths=new int[2];
			StaRunnerHelpers.RunSta(() => {
				for(int t=0;t<arrayThickness.Length;t++) {
					int thickness=arrayThickness[t];
					using(Form form=new Form())
					using(VScrollBar vScrollBar=new VScrollBar { Bounds=new Rectangle(0,0,thickness,200),Minimum=0,Maximum=100,LargeChange=20,Value=40 }) {
						form.Controls.Add(vScrollBar);
						UiScrollLook.Attach(vScrollBar);
						IntPtr handle=vScrollBar.Handle;
						using(Bitmap bitmap=new Bitmap(thickness,200,PixelFormat.Format24bppRgb)) {
							vScrollBar.DrawToBitmap(bitmap,new Rectangle(0,0,thickness,200));
							Color colorThumb=UiTheme.GetColor(EnumUiColor.StrokeStrong);
							Color colorTrack=UiTheme.GetColor(EnumUiColor.Surface);
							int level=(colorThumb.R+colorTrack.R)/2;
							//The thumb's middle row: the middle of the run of thumb pixels down the bar's centre line.
							int yFirst=-1;
							int yLast=-1;
							for(int y=thickness;y<200-thickness;y++) {
								if(bitmap.GetPixel(thickness/2,y).R<level) {
									if(yFirst<0) {
										yFirst=y;
									}
									yLast=y;
								}
							}
							Assert.IsTrue(yFirst>=0,"No thumb in the "+thickness+" px bar.");
							int yMiddle=(yFirst+yLast)/2;
							int width=0;
							for(int x=0;x<thickness;x++) {
								if(bitmap.GetPixel(x,yMiddle).R<level) {
									width++;
								}
							}
							arrayThumbWidths[t]=width;
							//The start arrow's chevron: the width of its dark pixels in the bar's first square.
							int xMin=thickness;
							int xMax=-1;
							for(int y=0;y<Math.Min(thickness,17);y++) {
								for(int x=0;x<thickness;x++) {
									if(bitmap.GetPixel(x,y).R<200) {
										xMin=Math.Min(xMin,x);
										xMax=Math.Max(xMax,x);
									}
								}
							}
							Assert.IsTrue(xMax>=0,"No chevron in the "+thickness+" px bar.");
							arrayChevronWidths[t]=xMax-xMin+1;
						}
					}
				}
			});
			for(int t=0;t<arrayThickness.Length;t++) {
				int thickness=arrayThickness[t];
				int expected=UiScrollPaint.GetThumbShape(new Rectangle(0,0,thickness,200),isVertical:true,UiScrollPaint.GetScale(thickness)).Width;
				Assert.IsTrue(Math.Abs(arrayThumbWidths[t]-expected)<=1,"The "+thickness+" px bar's thumb is "+arrayThumbWidths[t]+" px wide, expected "+expected+".");
			}
			Assert.IsTrue(arrayThumbWidths[1]>arrayThumbWidths[0],"The 25 px bar's thumb is not wider than the 17 px bar's.");
			Assert.IsTrue(arrayChevronWidths[1]>arrayChevronWidths[0],
				"The 25 px bar's chevron ("+arrayChevronWidths[1]+" px) is not wider than the 17 px bar's ("+arrayChevronWidths[0]+" px).");
		}

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd,int msg,IntPtr wParam,IntPtr lParam);

		#region Helpers
		private static void AssertColour(Color colorExpected,Color colorActual,string what) {
			Assert.AreEqual(colorExpected.ToArgb() & 0xFFFFFF,colorActual.ToArgb() & 0xFFFFFF,
				what+": expected "+colorExpected.Name+", found "+colorActual.Name);
		}

		///<summary>True when a pixel in the rectangle is darker than the level (an antialiased glyph's pixels reach their token only in part).</summary>
		private static bool HasDarker(Bitmap bitmap,Rectangle rectangle,int level) {
			rectangle.Intersect(new Rectangle(0,0,bitmap.Width,bitmap.Height));
			for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
				for(int x=rectangle.Left;x<rectangle.Right;x++) {
					if(bitmap.GetPixel(x,y).R<level) {
						return true;
					}
				}
			}
			return false;
		}

		private static bool HasColour(Bitmap bitmap,Rectangle rectangle,Color color) {
			rectangle.Intersect(new Rectangle(0,0,bitmap.Width,bitmap.Height));
			for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
				for(int x=rectangle.Left;x<rectangle.Right;x++) {
					if((bitmap.GetPixel(x,y).ToArgb() & 0xFFFFFF)==(color.ToArgb() & 0xFFFFFF)) {
						return true;
					}
				}
			}
			return false;
		}
		#endregion Helpers
	}
}
