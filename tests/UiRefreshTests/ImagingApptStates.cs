// The real imaging and appointment chrome of package UI-4c-1 (docs/port-notes/UI-4c.md) put in each state off screen (ImagingApptControls), and the
// token drawing of each state at the controls' own geometry (ImagingApptDrawings: SheetPainter's shapes with the light tokens, as the design sheet
// draws the toolbar button, the list row and the floating windows, without the text). Used by ImagingApptStateTests and by the G-UI1 part 2e state
// strips (ImagingApptGallery). No window is shown; the WPF window of the floating windows' menu is built and never shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;

namespace UiRefreshTests {
	///<summary>The states the chrome is drawn in. Each control uses the ones that apply to it (ImagingApptControls' arrays).</summary>
	public enum ChromeState {
		Rest,
		Hover,
		Pressed,
		Disabled,
		Selected,
		Cursor,
		HoverSecond,
		Empty,
		Unselected,
		UnselectedHover,
		WindowsOpen,
		HoverClose,
	}

	///<summary>A picture of a control with the rectangles it was drawn with, in device pixels from the control's top left corner, and the regions the
	///comparison leaves out (child controls with their own look).</summary>
	public class ChromePicture : IDisposable {
		public Bitmap Bitmap;
		public Dictionary<string,Rectangle> DictRectangles=new Dictionary<string,Rectangle>();
		public List<Rectangle> ListExcluded=new List<Rectangle>();
		public Dictionary<string,object> DictValues=new Dictionary<string,object>();

		public Rectangle R(string name) {
			return DictRectangles[name];
		}

		public T V<T>(string name) {
			return (T)DictValues[name];
		}

		public void Dispose() {
			Bitmap?.Dispose();
		}
	}

	#region WinForms subclasses
	public class StateToggleDayWeek : OpenPmsDental.UI.ToggleDayWeek {
		public bool IsTextHidden;
		protected override bool IsTextShown => !IsTextHidden;
		public Rectangle Item(int index) => GetRectangleItem(index);
		public Rectangle Glyph(int index) => GetRectangleGlyph(index);
		public void Hover(int index) {
			OnMouseMove(new MouseEventArgs(MouseButtons.None,0,Width/2,Height/2*index+Height/4,0));
		}
	}

	public class StatePinBoard : OpenPmsDental.UI.PinBoard {
		public bool IsTextHidden;
		protected override bool IsTextShown => !IsTextHidden;
	}

	public class StateProvSlider : OpenPmsDental.UI.ControlApptProvSlider {
		public bool IsTextHidden;
		public bool IsCursorForced;
		protected override bool IsTextShown => !IsTextHidden;
		protected override bool IsCursorShown => IsCursorForced;
		public Rectangle Slider() => GetRectangleSlider();
		public void Move(Point point) {
			OnMouseMove(new MouseEventArgs(MouseButtons.None,0,point.X,point.Y,0));
		}
	}

	public class StateUnmountedBar : OpenPmsDental.UI.UnmountedBar {
		public bool IsTextHidden;
		protected override bool IsTextShown => !IsTextHidden;
		public Rectangle Header() => GetRectangleHeader();
	}

	public class StateWindowingSlider : OpenPmsDental.UI.WindowingSlider {
		public void Move(Point point) {
			OnMouseMove(new MouseEventArgs(MouseButtons.None,0,point.X,point.Y,0));
		}

		public void Press(Point point) {
			OnMouseDown(new MouseEventArgs(MouseButtons.Left,1,point.X,point.Y,0));
		}
	}

	public class StateZoomSlider : OpenPmsDental.UI.ZoomSlider {
		public bool IsTextHidden;
		protected override bool IsTextShown => !IsTextHidden;
		public int[] Dividers() => GetXDividers();
		public Rectangle[] Ticks(int xFitPixels,int yTickLine,int widthLine) => GetRectanglesTickLine(xFitPixels,yTickLine,widthLine);
		public void Move(Point point) {
			OnMouseMove(new MouseEventArgs(MouseButtons.None,0,point.X,point.Y,0));
		}

		public void Press(Point point) {
			OnMouseDown(new MouseEventArgs(MouseButtons.Left,1,point.X,point.Y,0));
		}
	}
	#endregion WinForms subclasses

	public static class ImagingApptControls {
		public static readonly ChromeState[] ArrayToggleStates=new ChromeState[] { ChromeState.Rest,ChromeState.Hover,ChromeState.Selected };
		public static readonly ChromeState[] ArrayPinBoardStates=new ChromeState[] { ChromeState.Empty,ChromeState.Rest };
		public static readonly ChromeState[] ArrayProvSliderStates=new ChromeState[] { ChromeState.Rest,ChromeState.Hover,ChromeState.Pressed,ChromeState.Cursor };
		public static readonly ChromeState[] ArrayWindowingStates=new ChromeState[] { ChromeState.Rest,ChromeState.Hover,ChromeState.HoverSecond,
			ChromeState.Pressed,ChromeState.Disabled };
		public static readonly ChromeState[] ArrayZoomStates=new ChromeState[] { ChromeState.Rest,ChromeState.Hover,ChromeState.Pressed,ChromeState.HoverSecond,
			ChromeState.Selected,ChromeState.Disabled };
		public static readonly ChromeState[] ArrayDockHeaderStates=new ChromeState[] { ChromeState.Empty,ChromeState.Rest,ChromeState.Hover,ChromeState.HoverClose,
			ChromeState.Unselected,ChromeState.UnselectedHover,ChromeState.WindowsOpen };
		///<summary>The windowing slider's width in the state tests: 263 at 96 DPI, so one of its 255 steps is one pixel and the handles sit on whole
		///pixels (the real one is 154 wide; the gallery shows that).</summary>
		public const int WIDTH_WINDOWING_TEST=263;
		public const int MIN_WINDOWING=40;
		public const int MAX_WINDOWING=200;

		#region Reflection
		public static object Invoke(object target,string method,params object[] arrayArgs) {
			Type type=target.GetType();
			while(type!=null) {
				MethodInfo methodInfo=type.GetMethods(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly)
					.FirstOrDefault(x => x.Name==method && x.GetParameters().Length==arrayArgs.Length);
				if(methodInfo!=null) {
					return methodInfo.Invoke(target,arrayArgs);
				}
				type=type.BaseType;
			}
			throw new MissingMethodException(target.GetType().Name,method);
		}

		///<summary>Sets a private enum field (of a private enum type) from its member's name.</summary>
		public static void SetPrivateEnum(object target,string field,string memberName) {
			Type type=target.GetType();
			while(type!=null) {
				FieldInfo fieldInfo=type.GetField(field,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.DeclaredOnly);
				if(fieldInfo!=null) {
					fieldInfo.SetValue(target,Enum.Parse(fieldInfo.FieldType,memberName));
					return;
				}
				type=type.BaseType;
			}
			throw new MissingFieldException(field);
		}

		public static T GetPrivate<T>(object target,string field) {
			return ListStateControls.GetPrivate<T>(target,field);
		}
		#endregion Reflection

		#region WinForms
		private static Form MakeHost(out System.Windows.Forms.Panel panel) {
			Form form=new Form();
			form.ShowInTaskbar=false;
			form.Size=new Size(1500,900);
			panel=new System.Windows.Forms.Panel();
			panel.BackColor=SheetPainter.Light(EnumUiColor.Surface);
			panel.Bounds=new Rectangle(0,0,1480,860);
			form.Controls.Add(panel);
			return form;
		}

		///<summary>The control drawn off screen by itself (DrawToBitmap), as a 32-bit picture.</summary>
		private static Bitmap Draw(Control control) {
			Bitmap bitmap=new Bitmap(control.Width,control.Height,PixelFormat.Format32bppArgb);
			control.DrawToBitmap(bitmap,new Rectangle(0,0,control.Width,control.Height));
			return bitmap;
		}

		///<summary>The control and its children drawn off screen (DesignSheetWindows.Capture), as a 32-bit picture.</summary>
		private static Bitmap CaptureArgb(Control control) {
			using(Bitmap bitmap=DesignSheetWindows.Capture(control,Rectangle.Empty)) {
				return bitmap.Clone(new Rectangle(Point.Empty,bitmap.Size),PixelFormat.Format32bppArgb);
			}
		}

		private static Font ScaleFont(Font font,float scale) {
			return new Font(font.FontFamily,font.Size*scale,font.Style);
		}

		///<summary>ToggleDayWeek (80 x 35 at 96 DPI): Rest (Day selected), Hover (the mouse on Week), Selected (Week selected).</summary>
		public static ChromePicture RenderToggle(ChromeState state,bool withText,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateToggleDayWeek toggle=new StateToggleDayWeek();
				toggle.LayoutManager.SetScaleMS(scale);
				toggle.Font=ScaleFont(toggle.Font,scale);
				toggle.Bounds=new Rectangle(10,10,UiTheme.ScaleI(80,scale),UiTheme.ScaleI(35,scale));
				toggle.IsTextHidden=!withText;
				panel.Controls.Add(toggle);
				IntPtr handle=toggle.Handle;
				toggle.SetDay();
				if(state==ChromeState.Selected) {
					toggle.SetWeek();
				}
				if(state==ChromeState.Hover) {
					toggle.Hover(1);
				}
				ChromePicture chromePicture=new ChromePicture();
				chromePicture.Bitmap=Draw(toggle);
				for(int i=0;i<2;i++) {
					chromePicture.DictRectangles["item"+i]=toggle.Item(i);
					chromePicture.DictRectangles["glyph"+i]=toggle.Glyph(i);
					chromePicture.DictRectangles["row"+i]=new Rectangle(0,i*(toggle.Height/2),toggle.Width,toggle.Height/2);
				}
				chromePicture.DictValues["selected"]=state==ChromeState.Selected ? 1 : 0;
				chromePicture.DictValues["hover"]=state==ChromeState.Hover ? 1 : -1;
				return chromePicture;
			}
		}

		///<summary>Synthetic appointment pictures for the pinboard (an appointment book's drawing: content, kept as it is).</summary>
		public static Bitmap MakeApptBitmap(int width,int height,Color color) {
			Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(color);
				g.FillRectangle(Brushes.DarkSlateBlue,0,0,4,height);
				g.FillRectangle(Brushes.Black,0,height-1,width,1);
			}
			return bitmap;
		}

		///<summary>PinBoard (120 x 160 at 96 DPI): Empty (the hint), Rest (two synthetic appointments on it, none selected).</summary>
		public static ChromePicture RenderPinBoard(ChromeState state,bool withText,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StatePinBoard pinBoard=new StatePinBoard();
				pinBoard.LayoutManager.SetScaleMS(scale);
				pinBoard.Bounds=new Rectangle(10,10,UiTheme.ScaleI(120,scale),UiTheme.ScaleI(160,scale));
				pinBoard.IsTextHidden=!withText;
				panel.Controls.Add(pinBoard);
				IntPtr handle=pinBoard.Handle;
				ChromePicture chromePicture=new ChromePicture();
				List<Bitmap> listBitmaps=new List<Bitmap>();
				if(state==ChromeState.Rest) {
					DataTable table=new DataTable();
					table.Columns.Add("AptStatus");
					table.Columns.Add("AptDateTime");
					Color[] arrayColors=new Color[] { Color.LightSkyBlue,Color.PaleGreen };
					for(int i=0;i<2;i++) {
						DataRow dataRow=table.NewRow();
						dataRow["AptStatus"]="1";//scheduled: Dispose leaves it alone
						dataRow["AptDateTime"]="2026-11-18 09:00:00";
						table.Rows.Add(dataRow);
						Bitmap bitmap=MakeApptBitmap(pinBoard.Width-2,UiTheme.ScaleI(40,scale),arrayColors[i]);
						listBitmaps.Add(bitmap);
						pinBoard.ListPinBoardItems.Add(new OpenPmsDental.UI.PinBoardItem { BitmapAppt=bitmap,AptNum=i+1,DataRowAppt=dataRow });
					}
					pinBoard.SelectedIndex=-1;
				}
				chromePicture.Bitmap=Draw(pinBoard);
				chromePicture.DictValues["bitmaps"]=listBitmaps.Select(x => new Bitmap(x)).ToList();
				return chromePicture;
			}
		}

		///<summary>ControlApptProvSlider (45 x 200 at 96 DPI, a 1 h 20 min pattern with a second provider): Rest, Hover (the mouse on the slider),
		///Pressed (dragging the slider), Cursor (the typing cursor in the third cell).</summary>
		public static ChromePicture RenderProvSlider(ChromeState state,bool withText,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateProvSlider provSlider=new StateProvSlider();
				provSlider.LayoutManager.SetScaleMS(scale);
				provSlider.Bounds=new Rectangle(10,10,UiTheme.ScaleI(45,scale),UiTheme.ScaleI(200,scale));
				provSlider.IsTextHidden=!withText;
				panel.Controls.Add(provSlider);
				IntPtr handle=provSlider.Handle;
				provSlider.Pattern="XXXX////XXXXXX//";
				provSlider.PatternSecondary="//XXXX//////////";
				provSlider.ProvBarText="AB C";
				Draw(provSlider).Dispose();//the first paint places the slider
				Rectangle rectangleSlider=provSlider.Slider();
				switch(state) {
					case ChromeState.Hover:
						provSlider.Move(new Point(rectangleSlider.X+rectangleSlider.Width/2,rectangleSlider.Y+rectangleSlider.Height/2));
						break;
					case ChromeState.Pressed:
						ListStateControls.SetPrivate(provSlider,"_isDraggingSlider",true);
						break;
					case ChromeState.Cursor:
						ListStateControls.SetPrivate(provSlider,"_selectedIndex",2);
						provSlider.IsCursorForced=true;
						break;
				}
				ChromePicture chromePicture=new ChromePicture();
				chromePicture.Bitmap=Draw(provSlider);
				chromePicture.DictRectangles["slider"]=provSlider.Slider();
				chromePicture.DictValues["heightRow"]=GetPrivate<float>(provSlider,"_heightRow");
				chromePicture.DictValues["marginLR"]=GetPrivate<float>(provSlider,"_marginLR");
				chromePicture.DictValues["widthLeft"]=GetPrivate<float>(provSlider,"_widthLeft");
				chromePicture.DictValues["pattern"]=provSlider.Pattern;
				chromePicture.DictValues["patternSecondary"]=provSlider.PatternSecondary;
				chromePicture.DictValues["colorBack"]=provSlider.BackColor;
				chromePicture.DictValues["colorProv"]=provSlider.ColorProv;
				chromePicture.DictValues["colorProv2"]=provSlider.ColorProv2;
				chromePicture.DictValues["scale"]=scale;
				return chromePicture;
			}
		}

		///<summary>The WinForms UnmountedBar (600 x 60 at 96 DPI, no images): its header and outline; its four buttons are UI-4a's and left out.</summary>
		public static ChromePicture RenderUnmountedBar(bool withText,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateUnmountedBar unmountedBar=new StateUnmountedBar();
				unmountedBar.LayoutManager.SetScaleMS(scale);
				unmountedBar.Bounds=new Rectangle(10,10,UiTheme.ScaleI(600,scale),UiTheme.ScaleI(60,scale));
				unmountedBar.IsTextHidden=!withText;
				panel.Controls.Add(unmountedBar);
				unmountedBar.SetObjects(new List<OpenPmsDental.UI.UnmountedObject>());
				ChromePicture chromePicture=new ChromePicture();
				chromePicture.Bitmap=CaptureArgb(unmountedBar);
				chromePicture.DictRectangles["header"]=unmountedBar.Header();
				foreach(Control control in unmountedBar.Controls) {
					chromePicture.ListExcluded.Add(control.Bounds);
				}
				return chromePicture;
			}
		}

		///<summary>The WinForms WindowingSlider (WIDTH_WINDOWING_TEST x 20 at 96 DPI, or 154 x 20 for the gallery, 40 to 200): Rest, Hover (the left handle),
		///HoverSecond (the range between), Pressed (dragging the right handle), Disabled.</summary>
		public static ChromePicture RenderWindowingSlider(ChromeState state,float scale,int width96=WIDTH_WINDOWING_TEST) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateWindowingSlider windowingSlider=new StateWindowingSlider();
				windowingSlider.LayoutManager.SetScaleMS(scale);
				windowingSlider.Bounds=new Rectangle(10,10,UiTheme.ScaleI(width96,scale),UiTheme.ScaleI(20,scale));
				panel.Controls.Add(windowingSlider);
				IntPtr handle=windowingSlider.Handle;
				windowingSlider.MinVal=MIN_WINDOWING;
				windowingSlider.MaxVal=MAX_WINDOWING;
				Draw(windowingSlider).Dispose();//sets the step
				float tick=GetPrivate<float>(windowingSlider,"_tick");
				float widthBut=GetPrivate<float>(windowingSlider,"_widthBut");
				int yMiddle=windowingSlider.Height/2;
				switch(state) {
					case ChromeState.Hover:
						windowingSlider.Move(new Point((int)(MIN_WINDOWING*tick+widthBut/2),yMiddle));
						break;
					case ChromeState.HoverSecond:
						windowingSlider.Move(new Point((int)((MIN_WINDOWING+MAX_WINDOWING)/2*tick),yMiddle));
						break;
					case ChromeState.Pressed:
						windowingSlider.Press(new Point((int)(MAX_WINDOWING*tick+widthBut/2),yMiddle));
						break;
					case ChromeState.Disabled:
						windowingSlider.Enabled=false;
						break;
				}
				ChromePicture chromePicture=new ChromePicture();
				chromePicture.Bitmap=Draw(windowingSlider);
				chromePicture.DictValues["tick"]=GetPrivate<float>(windowingSlider,"_tick");
				chromePicture.DictValues["widthBut"]=widthBut;
				chromePicture.DictValues["min"]=windowingSlider.MinVal;
				chromePicture.DictValues["max"]=windowingSlider.MaxVal;
				chromePicture.DictValues["enabled"]=windowingSlider.Enabled;
				chromePicture.DictValues["scale"]=scale;
				return chromePicture;
			}
		}

		///<summary>The WinForms ZoomSlider (231 x 25 at 96 DPI, fit 50%): Rest, Hover (the minus), Pressed (the Fit cell), HoverSecond (the value box),
		///Selected (dragging the value box), Disabled.</summary>
		public static ChromePicture RenderZoomSlider(ChromeState state,bool withText,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateZoomSlider zoomSlider=new StateZoomSlider();
				zoomSlider.LayoutManager.SetScaleMS(scale);
				zoomSlider.Bounds=new Rectangle(10,10,UiTheme.ScaleI(231,scale),UiTheme.ScaleI(25,scale));
				zoomSlider.IsTextHidden=!withText;
				panel.Controls.Add(zoomSlider);
				IntPtr handle=zoomSlider.Handle;
				zoomSlider.SetValueInitialFit(new Size(400,300),new Size(800,600),0);
				Draw(zoomSlider).Dispose();//lays the tick line and the value box out
				Rectangle rectangleMinus=GetPrivate<Rectangle>(zoomSlider,"_rectangleMinus");
				Rectangle rectangleFit=GetPrivate<Rectangle>(zoomSlider,"_rectangleFit");
				Rectangle rectangleSlider=GetPrivate<Rectangle>(zoomSlider,"_rectangleSlider");
				switch(state) {
					case ChromeState.Hover:
						zoomSlider.Move(Center(rectangleMinus));
						break;
					case ChromeState.Pressed:
						zoomSlider.Press(Center(rectangleFit));
						break;
					case ChromeState.HoverSecond:
						zoomSlider.Move(Center(rectangleSlider));
						break;
					case ChromeState.Selected:
						zoomSlider.Press(Center(rectangleSlider));
						break;
					case ChromeState.Disabled:
						zoomSlider.Enabled=false;
						break;
				}
				ChromePicture chromePicture=new ChromePicture();
				chromePicture.Bitmap=Draw(zoomSlider);
				foreach(string name in new[] { "_rectangleMinus","_rectanglePlus","_rectangleFit","_rectangle100","_rectangleSlider","_rectangleTickLine" }) {
					chromePicture.DictRectangles[name]=GetPrivate<Rectangle>(zoomSlider,name);
				}
				int widthLine=UiTheme.GetLineWidth(scale);
				int xFitPixels=(int)(((float)chromePicture.R("_rectangleTickLine").Right-chromePicture.R("_rectangleTickLine").Left)
					/(GetPrivate<int>(zoomSlider,"_maximum")-1)*GetPrivate<int>(zoomSlider,"_fit"));
				int yTickLine=zoomSlider.LayoutManager.Scale(4);
				chromePicture.DictValues["dividers"]=zoomSlider.Dividers();
				chromePicture.DictValues["ticks"]=zoomSlider.Ticks(xFitPixels,yTickLine,widthLine);
				Rectangle rectangleTickLine=chromePicture.R("_rectangleTickLine");
				float value=GetPrivate<float>(zoomSlider,"_value");
				int xValuePixels=rectangleTickLine.Left+(int)(((float)rectangleTickLine.Right-rectangleTickLine.Left)/(GetPrivate<int>(zoomSlider,"_maximum")-1)*value);
				chromePicture.DictValues["triangle"]=new PointF[] { new PointF(xValuePixels,yTickLine),
					new PointF(xValuePixels-zoomSlider.LayoutManager.ScaleF(2.5f),yTickLine+zoomSlider.LayoutManager.ScaleF(5f)),
					new PointF(xValuePixels+zoomSlider.LayoutManager.ScaleF(2.5f),yTickLine+zoomSlider.LayoutManager.ScaleF(5f)) };
				chromePicture.DictValues["enabled"]=zoomSlider.Enabled;
				chromePicture.DictValues["scale"]=scale;
				return chromePicture;
			}
		}

		public static Point Center(Rectangle rectangle) {
			return new Point(rectangle.X+rectangle.Width/2,rectangle.Y+rectangle.Height/2);
		}
		#endregion WinForms

		#region WPF
		private static Rectangle Rect(System.Windows.FrameworkElement frameworkElement,System.Windows.Media.Visual visualAncestor,float scale) {
			return TabSplitControls.GetRectangle(frameworkElement,visualAncestor,scale);
		}

		private static T Field<T>(object target,string name) {
			return GetPrivate<T>(target,name);
		}

		///<summary>The WPF ImageDockHeader (625 x 23): Empty, Rest (selected), Hover (selected, the mouse on maximise), HoverClose (selected, on close),
		///Unselected, UnselectedHover (on minimise), WindowsOpen (not selected, the Windows menu open). withText false hides the texts and the caption
		///glyphs, so the shapes alone can be compared.</summary>
		public static ChromePicture RenderWpfDockHeader(ChromeState state,bool withText,float scale) {
			WpfControls.UI.ImageDockHeader imageDockHeader=new WpfControls.UI.ImageDockHeader();
			imageDockHeader.Text="11/18/2026 Patient Photo";
			imageDockHeader.IsEmpty=state==ChromeState.Empty;
			imageDockHeader.IsSelected=state==ChromeState.Rest || state==ChromeState.Hover || state==ChromeState.HoverClose;
			switch(state) {
				case ChromeState.Hover:
					ListStateControls.SetPrivate(imageDockHeader,"_isHoverMax",true);
					break;
				case ChromeState.HoverClose:
					ListStateControls.SetPrivate(imageDockHeader,"_isHoverX",true);
					break;
				case ChromeState.UnselectedHover:
					ListStateControls.SetPrivate(imageDockHeader,"_isHoverMin",true);
					break;
				case ChromeState.WindowsOpen:
					imageDockHeader.IsButWinPressed=true;
					break;
			}
			Invoke(imageDockHeader,"SetColors");
			System.Windows.Visibility visibility=withText ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
			foreach(string name in new[] { "labelTitle","labelWin","pathMin","pathMax","pathClose" }) {
				Field<System.Windows.UIElement>(imageDockHeader,name).Visibility=visibility;
			}
			imageDockHeader.Measure(new System.Windows.Size(625,23));
			imageDockHeader.Arrange(new System.Windows.Rect(0,0,625,23));
			imageDockHeader.UpdateLayout();
			ChromePicture chromePicture=new ChromePicture();
			foreach(string name in new[] { "rectangleWindows","rectangleMin","rectangleMax","rectangleClose","pathMin","pathMax","pathClose","labelTitle","labelWin" }) {
				chromePicture.DictRectangles[name]=Rect(Field<System.Windows.FrameworkElement>(imageDockHeader,name),imageDockHeader,scale);
			}
			foreach(string name in new[] { "pathMin","pathMax","pathClose" }) {
				chromePicture.DictValues[name+"Fill"]=Field<System.Windows.Shapes.Path>(imageDockHeader,name).Fill;
			}
			chromePicture.Bitmap=ControlPictures.RenderWpf(imageDockHeader,625,23,scale);
			return chromePicture;
		}

		///<summary>The WPF UnmountedBar (600 x 60, no images): its background and header; its four buttons are UI-4a's and left out.</summary>
		public static ChromePicture RenderWpfUnmountedBar(bool withText,float scale) {
			WpfControls.UI.UnmountedBar unmountedBar=new WpfControls.UI.UnmountedBar();
			unmountedBar.Width=600;
			unmountedBar.Height=60;
			Field<System.Windows.UIElement>(unmountedBar,"textBlockTitle").Visibility=withText ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
			unmountedBar.Measure(new System.Windows.Size(600,60));
			unmountedBar.Arrange(new System.Windows.Rect(0,0,600,60));
			unmountedBar.UpdateLayout();
			ChromePicture chromePicture=new ChromePicture();
			System.Windows.Controls.Grid grid=Field<System.Windows.Controls.Grid>(unmountedBar,"grid");
			foreach(WpfControls.UI.Button button in grid.Children.OfType<WpfControls.UI.Button>()) {
				chromePicture.ListExcluded.Add(Rectangle.Inflate(Rect(button,unmountedBar,scale),1,1));
			}
			chromePicture.DictRectangles["header"]=Rect(Field<System.Windows.FrameworkElement>(unmountedBar,"rectangleHeader"),unmountedBar,scale);
			chromePicture.DictRectangles["title"]=Rect(Field<System.Windows.FrameworkElement>(unmountedBar,"textBlockTitle"),unmountedBar,scale);
			chromePicture.Bitmap=ControlPictures.RenderWpf(unmountedBar,600,60,scale);
			return chromePicture;
		}

		///<summary>The WPF WindowingSlider (WIDTH_WINDOWING_TEST x 20, or 154 x 20 for the gallery, 40 to 200): Rest, Hover (the left handle),
		///HoverSecond (the range), Pressed (the right handle), Disabled, set as its mouse handlers set them.</summary>
		public static ChromePicture RenderWpfWindowingSlider(ChromeState state,float scale,int width96=WIDTH_WINDOWING_TEST) {
			WpfControls.UI.WindowingSlider windowingSlider=new WpfControls.UI.WindowingSlider();
			windowingSlider.Width=width96;
			windowingSlider.Height=20;
			windowingSlider.Measure(new System.Windows.Size(width96,20));
			windowingSlider.Arrange(new System.Windows.Rect(0,0,width96,20));
			switch(state) {
				case ChromeState.Hover:
					SetPrivateEnum(windowingSlider,"_enumSliderButStateL","Hover");
					break;
				case ChromeState.HoverSecond:
					SetPrivateEnum(windowingSlider,"_enumSliderButStateM","Hover");
					break;
				case ChromeState.Pressed:
					SetPrivateEnum(windowingSlider,"_enumSliderButStateR","Pressed");
					break;
				case ChromeState.Disabled:
					windowingSlider.IsEnabled=false;
					break;
			}
			windowingSlider.SetBounds(MIN_WINDOWING,MAX_WINDOWING);
			windowingSlider.UpdateLayout();
			ChromePicture chromePicture=new ChromePicture();
			chromePicture.DictValues["tick"]=(float)Field<double>(windowingSlider,"_tick");
			chromePicture.DictValues["widthBut"]=(float)Field<double>(windowingSlider,"_widthBut");
			chromePicture.DictValues["min"]=windowingSlider.MinVal;
			chromePicture.DictValues["max"]=windowingSlider.MaxVal;
			chromePicture.DictValues["enabled"]=windowingSlider.IsEnabled;
			chromePicture.DictValues["width"]=(float)width96;
			chromePicture.Bitmap=ControlPictures.RenderWpf(windowingSlider,width96,20,scale);
			return chromePicture;
		}

		///<summary>The WPF ZoomSlider (231 x 25, fit 50%): its Loaded step run by hand (it builds the cells), then Rest, Hover (the minus), Pressed (the
		///Fit cell), HoverSecond (the value box), Selected (dragging the value box), Disabled, set as its mouse handlers set them. withText false hides
		///the texts and the minus and plus.</summary>
		public static ChromePicture RenderWpfZoomSlider(ChromeState state,bool withText,float scale) {
			WpfControls.UI.ZoomSlider zoomSlider=new WpfControls.UI.ZoomSlider();
			zoomSlider.Width=231;
			zoomSlider.Height=25;
			zoomSlider.Measure(new System.Windows.Size(231,25));
			zoomSlider.Arrange(new System.Windows.Rect(0,0,231,25));
			zoomSlider.UpdateLayout();
			Invoke(zoomSlider,"ZoomSlider_Loaded",zoomSlider,new System.Windows.RoutedEventArgs());
			zoomSlider.SetValueInitialFit(new System.Windows.Size(400,300),new System.Windows.Size(800,600),0);
			Dictionary<string,System.Windows.Shapes.Rectangle> dictCells=new Dictionary<string,System.Windows.Shapes.Rectangle>();
			foreach(string name in new[] { "rectangleMinus","rectanglePlus","rectangleFit","rectangle100" }) {
				dictCells[name]=Field<System.Windows.Shapes.Rectangle>(zoomSlider,name);
			}
			System.Windows.Shapes.Rectangle rectangleSlider=Field<System.Windows.Shapes.Rectangle>(zoomSlider,"rectangleSlider");
			System.Windows.Point pointInSlider=new System.Windows.Point(System.Windows.Controls.Canvas.GetLeft(rectangleSlider)+rectangleSlider.Width/2,15);
			switch(state) {
				case ChromeState.Hover:
					ListStateControls.SetPrivate(zoomSlider,"_pointMouse",new System.Windows.Point(8,12));
					break;
				case ChromeState.Pressed:
					ListStateControls.SetPrivate(zoomSlider,"_isMouseDown",true);
					ListStateControls.SetPrivate(zoomSlider,"_pointMouseDown",new System.Windows.Point(188,12));
					break;
				case ChromeState.HoverSecond:
					ListStateControls.SetPrivate(zoomSlider,"_pointMouse",pointInSlider);
					break;
				case ChromeState.Selected:
					ListStateControls.SetPrivate(zoomSlider,"_isMouseDown",true);
					ListStateControls.SetPrivate(zoomSlider,"_isMouseDownInRectangleSlider",true);
					ListStateControls.SetPrivate(zoomSlider,"_pointMouseDown",pointInSlider);
					break;
				case ChromeState.Disabled:
					zoomSlider.IsEnabled=false;
					break;
			}
			Invoke(zoomSlider,"Draw");
			System.Windows.Visibility visibility=withText ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
			foreach(string name in new[] { "textBlock","textFit","text100","pathMinus","pathPlus" }) {
				Field<System.Windows.UIElement>(zoomSlider,name).Visibility=visibility;
			}
			zoomSlider.UpdateLayout();
			ChromePicture chromePicture=new ChromePicture();
			System.Windows.Controls.Canvas canvas=Field<System.Windows.Controls.Canvas>(zoomSlider,"canvas");
			foreach(KeyValuePair<string,System.Windows.Shapes.Rectangle> pair in dictCells) {
				chromePicture.DictRectangles[pair.Key]=Rect(pair.Value,zoomSlider,scale);
			}
			chromePicture.DictRectangles["rectangleSlider"]=Rect(rectangleSlider,zoomSlider,scale);
			chromePicture.DictRectangles["pathMinus"]=Rect(Field<System.Windows.FrameworkElement>(zoomSlider,"pathMinus"),zoomSlider,scale);
			chromePicture.DictRectangles["pathPlus"]=Rect(Field<System.Windows.FrameworkElement>(zoomSlider,"pathPlus"),zoomSlider,scale);
			chromePicture.DictValues["pathMinusFill"]=Field<System.Windows.Shapes.Path>(zoomSlider,"pathMinus").Fill;
			chromePicture.DictValues["pathPlusFill"]=Field<System.Windows.Shapes.Path>(zoomSlider,"pathPlus").Fill;
			foreach(string name in new[] { "textBlock","textFit","text100" }) {
				chromePicture.DictRectangles[name]=Rect(Field<System.Windows.FrameworkElement>(zoomSlider,name),zoomSlider,scale);
			}
			chromePicture.DictValues["sliderLeft"]=System.Windows.Controls.Canvas.GetLeft(rectangleSlider);
			chromePicture.DictValues["sliderWidth"]=rectangleSlider.Width;
			chromePicture.DictValues["lines"]=canvas.Children.OfType<System.Windows.Shapes.Line>().Select(x => new double[] { x.X1,x.Y1,x.X2,x.Y2 }).ToList();
			chromePicture.DictValues["lineStrokes"]=canvas.Children.OfType<System.Windows.Shapes.Line>().Select(x => ((System.Windows.Media.SolidColorBrush)x.Stroke).Color).ToList();
			System.Windows.Shapes.Polygon polygonTriangle=Field<System.Windows.Shapes.Polygon>(zoomSlider,"polygonTriangle");
			chromePicture.DictValues["triangleLeft"]=System.Windows.Controls.Canvas.GetLeft(polygonTriangle);
			chromePicture.DictValues["enabled"]=zoomSlider.IsEnabled;
			chromePicture.DictValues["cellFills"]=dictCells.ToDictionary(x => x.Key,x => x.Value.Fill);
			chromePicture.Bitmap=ControlPictures.RenderWpf(zoomSlider,231,25,scale);
			return chromePicture;
		}

		///<summary>The WPF ImageSelector (227 x 200) with synthetic categories and items: "Radiographs" (expanded, two items, the second selected),
		///"Photos" (collapsed, one item) and "Empty" (no items). hover is the index in its list of the row under the mouse, or -1.</summary>
		public static WpfControls.UI.ImageSelector MakeImageSelector(int idxHover) {
			WpfControls.UI.ImageSelector imageSelector=new WpfControls.UI.ImageSelector();
			imageSelector.Width=227;
			imageSelector.Height=200;
			List<OpenPmsDentalBusiness.Def> listDefs=new List<OpenPmsDentalBusiness.Def> {
				new OpenPmsDentalBusiness.Def { DefNum=1,ItemName="Radiographs",ItemValue="" },
				new OpenPmsDentalBusiness.Def { DefNum=2,ItemName="Photos",ItemValue="" },
				new OpenPmsDentalBusiness.Def { DefNum=3,ItemName="Empty",ItemValue="" },
			};
			imageSelector.SetCategories(listDefs);
			DataTable table=new DataTable();
			foreach(string column in new[] { "DocNum","MountNum","DocCategory","DateCreated","Description","ImgType" }) {
				table.Columns.Add(column);
			}
			table.Rows.Add("11","0","1","2026-11-02","Bitewing left","2");
			table.Rows.Add("12","0","1","2026-11-02","Bitewing right","2");
			table.Rows.Add("13","0","2","2026-11-03","Smile","3");
			imageSelector.SetData(null,table,false,"");
			imageSelector.SetExpandedCategories(new List<long> { 1 });
			imageSelector.SetSelected(OpenPmsDental.UI.EnumImageNodeType.Document,12);
			if(idxHover>=0) {
				ListStateControls.SetPrivate(imageSelector,"_hoverIndex",idxHover);
				Invoke(imageSelector,"SetColors");
			}
			imageSelector.Measure(new System.Windows.Size(227,200));
			imageSelector.Arrange(new System.Windows.Rect(0,0,227,200));
			imageSelector.UpdateLayout();
			return imageSelector;
		}

		///<summary>The WPF floating windows' menu, never shown: its grid (taken out of the window) with two snap areas made by its own CreatePath, the
		///second under the mouse, laid out at 345 x 195.</summary>
		public static System.Windows.Controls.Grid MakeFloatWindowsGrid(out System.Windows.Shapes.Path pathRest,out System.Windows.Shapes.Path pathHover) {
			WpfControls.UI.WindowImageFloatWindows window=new WpfControls.UI.WindowImageFloatWindows();
			pathRest=(System.Windows.Shapes.Path)Invoke(window,"CreatePath",10.5,30.5,35.0,48.0,false,false,false,false);
			pathHover=(System.Windows.Shapes.Path)Invoke(window,"CreatePath",50.5,30.5,35.0,48.0,false,false,false,false);
			WpfControls.UI.UiLookImaging.SetSnapArea(pathHover,isHover:true);
			System.Windows.Controls.Grid grid=Field<System.Windows.Controls.Grid>(window,"grid");
			window.Content=null;
			grid.Width=345;
			grid.Height=195;
			grid.Measure(new System.Windows.Size(345,195));
			grid.Arrange(new System.Windows.Rect(0,0,345,195));
			grid.UpdateLayout();
			return grid;
		}
		#endregion WPF
	}

	///<summary>The token drawing of each state at the controls' geometry: section 3.6's shapes (the design sheet's toolbar button, list row and
	///floating windows where it defines them, the tokens of docs/port-notes/UI-4c.md section 3 where it does not), without the text.</summary>
	public static class ImagingApptDrawings {
		private static Bitmap Start(Size size,Color colorBack,float scale,out Graphics g,out SheetPainter p) {
			Bitmap bitmap=new Bitmap(size.Width,size.Height,PixelFormat.Format32bppArgb);
			g=Graphics.FromImage(bitmap);
			g.Clear(colorBack);
			p=new SheetPainter(g,scale,SheetPainter.Light);
			return bitmap;
		}

		private static Color L(EnumUiColor uiColor) {
			return SheetPainter.Light(uiColor);
		}

		///<summary>ToggleDayWeek: SurfaceRaised, the selected row AccentSubtle with a 1 px Accent outline (4 px corners, as a toggled toolbar button), the
		///row under the mouse SubtleHover, the day and week boxes 1 px in Text, the 1 px Stroke border.</summary>
		public static Bitmap Toggle(ChromePicture chromePicture,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(chromePicture.Bitmap.Size,L(EnumUiColor.SurfaceRaised),scale,out g,out p);
			using(g)
			using(p) {
				int idxSelected=chromePicture.V<int>("selected");
				int idxHover=chromePicture.V<int>("hover");
				for(int i=0;i<2;i++) {
					Rectangle rectangleItem=chromePicture.R("item"+i);
					if(i==idxSelected) {
						p.Fill(EnumUiColor.AccentSubtle,rectangleItem,UiTheme.CORNER_CONTROL);
						p.Outline(EnumUiColor.Accent,rectangleItem,p.LineWidth,UiTheme.CORNER_CONTROL);
					}
					else if(i==idxHover) {
						p.Fill(EnumUiColor.SubtleHover,rectangleItem,UiTheme.CORNER_CONTROL);
					}
					p.Outline(i==idxSelected ? EnumUiColor.TextSelected : EnumUiColor.Text,chromePicture.R("glyph"+i),p.LineWidth,0);
				}
				p.Outline(EnumUiColor.Stroke,new Rectangle(Point.Empty,bitmap.Size),p.LineWidth,0);
			}
			return bitmap;
		}

		///<summary>PinBoard: SurfaceRaised with a 1 px Stroke border; the appointments drawn over it where they were (content).</summary>
		public static Bitmap PinBoard(ChromePicture chromePicture,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(chromePicture.Bitmap.Size,L(EnumUiColor.SurfaceRaised),scale,out g,out p);
			using(g)
			using(p) {
				p.Outline(EnumUiColor.Stroke,new Rectangle(Point.Empty,bitmap.Size),p.LineWidth,0);
				List<Bitmap> listBitmaps=chromePicture.V<List<Bitmap>>("bitmaps");
				g.PixelOffsetMode=PixelOffsetMode.Default;
				g.SmoothingMode=SmoothingMode.AntiAlias;
				for(int i=0;i<listBitmaps.Count;i++) {
					g.DrawImage(listBitmaps[i],1,i*13,listBitmaps[i].Width,listBitmaps[i].Height);
				}
			}
			return bitmap;
		}

		///<summary>ControlApptProvSlider: the control's background, the cells SurfaceRaised with the providers' colours (data), the lines above the cells
		///(each hour StrokeStrong, the others Gridline), the two dividers Gridline, the outline StrokeStrong, the slider a button (SurfaceRaised,
		///ButtonHover, ButtonPressed, 1 px Stroke, 4 px corners), the cursor a 1 px Accent line.</summary>
		public static Bitmap ProvSlider(ChromePicture chromePicture,ChromeState state,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(chromePicture.Bitmap.Size,chromePicture.V<Color>("colorBack"),scale,out g,out p);
			using(g)
			using(p) {
				int h=(int)chromePicture.V<float>("heightRow");
				int m=(int)chromePicture.V<float>("marginLR");
				int w=(int)chromePicture.V<float>("widthLeft");
				int width=bitmap.Width;
				string pattern=chromePicture.V<string>("pattern");
				string patternSecondary=chromePicture.V<string>("patternSecondary");
				int lengthSelected=pattern.Length/2;
				p.Fill(EnumUiColor.SurfaceRaised,new Rectangle(m,0,width-m*2-1,lengthSelected*h),0);
				for(int i=0;i<lengthSelected;i++) {
					if(pattern.Substring(i*2,1)=="X") {
						p.Fill(chromePicture.V<Color>("colorProv"),new Rectangle(m,i*h,w,h),0);
					}
					if(patternSecondary.Substring(i*2,1)=="X") {
						p.Fill(chromePicture.V<Color>("colorProv2"),new Rectangle(m+w,i*h,w,h),0);
					}
					p.Fill(i%6==0 ? EnumUiColor.StrokeStrong : EnumUiColor.Gridline,new Rectangle(m,i*h,width-m*2,1),0);
				}
				p.Fill(EnumUiColor.Gridline,new Rectangle(m+w,0,1,lengthSelected*h+1),0);
				p.Fill(EnumUiColor.Gridline,new Rectangle(m+w*2,0,1,lengthSelected*h+1),0);
				p.Fill(EnumUiColor.StrokeStrong,new Rectangle(m,0,width-m*2,1),0);
				p.Fill(EnumUiColor.StrokeStrong,new Rectangle(m,0,1,lengthSelected*h+1),0);
				p.Fill(EnumUiColor.StrokeStrong,new Rectangle(width-m-1,0,1,lengthSelected*h+1),0);
				Rectangle rectangleSlider=chromePicture.R("slider");
				EnumUiColor uiColorFill=state==ChromeState.Pressed ? EnumUiColor.ButtonPressed : (state==ChromeState.Hover ? EnumUiColor.ButtonHover : EnumUiColor.SurfaceRaised);
				p.Fill(uiColorFill,rectangleSlider,UiTheme.CORNER_CONTROL);
				p.Outline(EnumUiColor.Stroke,rectangleSlider,p.LineWidth,UiTheme.CORNER_CONTROL);
				if(state==ChromeState.Cursor) {
					int x=m+w*2+UiTheme.ScaleI(3,scale);
					p.Fill(EnumUiColor.Accent,new Rectangle(x,2*h+2,1,h-3),0);
				}
			}
			return bitmap;
		}

		///<summary>UnmountedBar (WinForms and WPF): the background (white, the mount's colour, in WinForms; Surface in WPF), the header a flat Accent bar,
		///the WinForms one with a 1 px Stroke outline.</summary>
		public static Bitmap UnmountedBar(ChromePicture chromePicture,bool isWpf,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(chromePicture.Bitmap.Size,isWpf ? L(EnumUiColor.Surface) : Color.White,scale,out g,out p);
			using(g)
			using(p) {
				p.Fill(EnumUiColor.Accent,chromePicture.R("header"),0);
				if(!isWpf) {
					p.Outline(EnumUiColor.Stroke,new Rectangle(Point.Empty,bitmap.Size),p.LineWidth,0);
				}
			}
			return bitmap;
		}

		///<summary>The handle's points: today's pentagon at the value (the pointy part at its centre).</summary>
		public static PointF[] GetHandlePoints(float x,float widthBut,float height) {
			return new PointF[] { new PointF(x,height-4),new PointF(x,3),new PointF(x+widthBut/2f,0),new PointF(x+widthBut,3),new PointF(x+widthBut,height-4),
				new PointF(x+widthBut/2f,height-1),new PointF(x,height-4) };
		}

		///<summary>The token states of the windowing slider's parts in a picture state: the left handle, the range, the right handle.</summary>
		public static EnumUiButtonState[] GetWindowingStates(ChromeState state,bool isEnabled) {
			if(!isEnabled) {
				return new EnumUiButtonState[] { EnumUiButtonState.Disabled,EnumUiButtonState.Disabled,EnumUiButtonState.Disabled };
			}
			EnumUiButtonState[] arrayStates=new EnumUiButtonState[] { EnumUiButtonState.Normal,EnumUiButtonState.Normal,EnumUiButtonState.Normal };
			if(state==ChromeState.Hover) {
				arrayStates[0]=EnumUiButtonState.Hover;
			}
			if(state==ChromeState.HoverSecond) {
				arrayStates[1]=EnumUiButtonState.Hover;
			}
			if(state==ChromeState.Pressed) {
				arrayStates[2]=EnumUiButtonState.Pressed;
			}
			return arrayStates;
		}

		///<summary>The WinForms WindowingSlider: Surface, the range outlined (StrokeStrong; StrokeStrongHover, Accent, Stroke by state), the black, the grey
		///ramp and the white as today (content), the handles flat (UiSliderPaint's fill and outline) on today's path.</summary>
		public static Bitmap WindowingSlider(ChromePicture chromePicture,ChromeState state,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(chromePicture.Bitmap.Size,L(EnumUiColor.Surface),scale,out g,out p);
			using(g)
			using(p) {
				float tick=chromePicture.V<float>("tick");
				float widthBut=chromePicture.V<float>("widthBut");
				int min=chromePicture.V<int>("min");
				int max=chromePicture.V<int>("max");
				bool isEnabled=chromePicture.V<bool>("enabled");
				int height=bitmap.Height;
				int width=bitmap.Width;
				EnumUiButtonState[] arrayStates=GetWindowingStates(state,isEnabled);
				Rectangle rectangleMiddle=new Rectangle((int)(widthBut/2f+min*tick),0,(int)((max-min)*tick),height-1);
				p.Outline(UiSliderPaint.GetHandleOutline(arrayStates[1]),new Rectangle(rectangleMiddle.X,rectangleMiddle.Y,rectangleMiddle.Width+1,rectangleMiddle.Height+1),p.LineWidth,0);
				g.PixelOffsetMode=PixelOffsetMode.Default;
				g.SmoothingMode=SmoothingMode.HighQuality;
				g.FillRectangle(Brushes.Black,new Rectangle((int)(widthBut/2),2,rectangleMiddle.Left-(int)(widthBut/2),height-5));
				if(isEnabled && rectangleMiddle.Width>widthBut) {
					using(LinearGradientBrush linearGradientBrush=new LinearGradientBrush(new Point(rectangleMiddle.X+(int)(widthBut/2),0),
						new Point(rectangleMiddle.Right-(int)(widthBut/2)+1,0),Color.Black,Color.White))
					{
						g.FillRectangle(linearGradientBrush,new Rectangle(rectangleMiddle.Left,2,rectangleMiddle.Width,height-5));
					}
				}
				g.FillRectangle(Brushes.White,new Rectangle(rectangleMiddle.Right-(int)(widthBut/2)+1,2,2,height-5));
				g.FillRectangle(Brushes.White,new Rectangle(rectangleMiddle.Right,2,width-(int)(widthBut/2)-rectangleMiddle.Right,height-5));
				int[] arrayValues=new int[] { min,max };
				for(int i=0;i<2;i++) {
					EnumUiButtonState buttonState=arrayStates[i==0 ? 0 : 2];
					using(GraphicsPath graphicsPath=new GraphicsPath())
					using(SolidBrush brush=new SolidBrush(L(UiSliderPaint.GetHandleFill(buttonState))))
					using(Pen pen=new Pen(L(UiSliderPaint.GetHandleOutline(buttonState)))) {
						graphicsPath.AddLines(GetHandlePoints(arrayValues[i]*tick,widthBut,height));
						g.FillPath(brush,graphicsPath);
						g.DrawPath(pen,graphicsPath);
					}
				}
			}
			return bitmap;
		}

		///<summary>The WinForms ZoomSlider: SurfaceSubtle, the cell's SubtleHover or SubtlePressed (4 px corners), the 1 px Stroke dividers, the Fluent minus
		///and plus, the StrokeStrong tick line, the value box SurfaceRaised with its outline, the pointer, the 1 px Gridline at the bottom.</summary>
		public static Bitmap ZoomSlider(ChromePicture chromePicture,ChromeState state,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(chromePicture.Bitmap.Size,L(EnumUiColor.SurfaceSubtle),scale,out g,out p);
			using(g)
			using(p) {
				bool isEnabled=chromePicture.V<bool>("enabled");
				EnumUiColor uiColorText=isEnabled ? EnumUiColor.Text : EnumUiColor.TextDisabled;
				if(state==ChromeState.Hover) {
					p.Fill(EnumUiColor.SubtleHover,chromePicture.R("_rectangleMinus"),UiTheme.CORNER_CONTROL);
				}
				if(state==ChromeState.Pressed) {
					p.Fill(EnumUiColor.SubtlePressed,chromePicture.R("_rectangleFit"),UiTheme.CORNER_CONTROL);
				}
				foreach(int x in chromePicture.V<int[]>("dividers")) {
					p.Fill(EnumUiColor.Stroke,new Rectangle(x,0,p.LineWidth,bitmap.Height),0);
				}
				UiIcons.Draw(g,UiGlyph.Subtract,UiSliderPaint.GetGlyphRectangle(chromePicture.R("_rectangleMinus"),scale),uiColorText);
				UiIcons.Draw(g,UiSliderPaint.GetAddDesign(UiTheme.ScaleI(UiSliderPaint.SIZE_GLYPH,scale)),UiSliderPaint.GetGlyphRectangle(chromePicture.R("_rectanglePlus"),scale),
					L(uiColorText));
				foreach(Rectangle rectangleTick in chromePicture.V<Rectangle[]>("ticks")) {
					p.Fill(EnumUiColor.StrokeStrong,rectangleTick,0);
				}
				Rectangle rectangleSlider=chromePicture.R("_rectangleSlider");
				p.Fill(EnumUiColor.SurfaceRaised,rectangleSlider,0);
				EnumUiColor uiColorBox=!isEnabled ? EnumUiColor.Stroke : (state==ChromeState.Selected ? EnumUiColor.Accent
					: (state==ChromeState.HoverSecond ? EnumUiColor.StrokeStrongHover : EnumUiColor.StrokeStrong));
				p.Outline(uiColorBox,new Rectangle(rectangleSlider.X,rectangleSlider.Y,rectangleSlider.Width+1,rectangleSlider.Height+1),p.LineWidth,0);
				g.PixelOffsetMode=PixelOffsetMode.Default;
				g.SmoothingMode=SmoothingMode.HighQuality;
				using(SolidBrush brush=new SolidBrush(L(uiColorText))) {
					g.FillPolygon(brush,chromePicture.V<PointF[]>("triangle"));
				}
				p.Fill(EnumUiColor.Gridline,new Rectangle(0,bitmap.Height-p.LineWidth,bitmap.Width,p.LineWidth),0);
			}
			return bitmap;
		}

		///<summary>The image dock's header as the design sheet's floating windows: selected Accent (CaptionHover under the mouse), not selected
		///SurfaceSubtle (SubtleHover), close Danger under the mouse, the Windows button SurfaceRaised while its menu is open, empty SurfaceRaised.</summary>
		public static Bitmap DockHeader(ChromePicture chromePicture,ChromeState state,float scale) {
			bool isEmpty=state==ChromeState.Empty;
			bool isSelected=state==ChromeState.Rest || state==ChromeState.Hover || state==ChromeState.HoverClose;
			EnumUiColor uiColorBack=isEmpty ? EnumUiColor.SurfaceRaised : (isSelected ? EnumUiColor.Accent : EnumUiColor.SurfaceSubtle);
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(chromePicture.Bitmap.Size,L(uiColorBack),scale,out g,out p);
			using(g)
			using(p) {
				switch(state) {
					case ChromeState.Hover:
						p.Fill(EnumUiColor.CaptionHover,chromePicture.R("rectangleMax"),0);
						break;
					case ChromeState.HoverClose:
						p.Fill(EnumUiColor.Danger,chromePicture.R("rectangleClose"),0);
						break;
					case ChromeState.UnselectedHover:
						p.Fill(EnumUiColor.SubtleHover,chromePicture.R("rectangleMin"),0);
						break;
					case ChromeState.WindowsOpen:
						p.Fill(EnumUiColor.SurfaceRaised,chromePicture.R("rectangleWindows"),0);
						break;
				}
			}
			return bitmap;
		}

		///<summary>The token of the dock header's glyphs and title in a state, and of the close glyph.</summary>
		public static EnumUiColor DockHeaderText(ChromeState state,bool isClose) {
			if(state==ChromeState.Empty) {
				return EnumUiColor.SurfaceRaised;
			}
			if(isClose && state==ChromeState.HoverClose) {
				return EnumUiColor.TextOnAccent;
			}
			return state==ChromeState.Rest || state==ChromeState.Hover || state==ChromeState.HoverClose ? EnumUiColor.TextOnAccent : EnumUiColor.Text;
		}

		///<summary>The WPF WindowingSlider at its geometry (DIP, rendered at the scale): Surface, the black, the white (the ramp between is content and
		///not a token colour, so the comparison leaves it out), the handles, then the range's top and bottom lines over them.</summary>
		public static Bitmap WpfWindowingSlider(ChromePicture chromePicture,ChromeState state,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(chromePicture.Bitmap.Size,L(EnumUiColor.Surface),scale,out g,out p);
			using(g)
			using(p) {
				g.ScaleTransform(scale,scale);
				g.SmoothingMode=SmoothingMode.AntiAlias;
				float tick=chromePicture.V<float>("tick");
				float widthBut=chromePicture.V<float>("widthBut");
				int min=chromePicture.V<int>("min");
				int max=chromePicture.V<int>("max");
				float width=chromePicture.V<float>("width");
				bool isEnabled=chromePicture.V<bool>("enabled");
				EnumUiButtonState[] arrayStates=GetWindowingStates(state,isEnabled);
				float xLeft=widthBut/2+min*tick;
				float xRight=widthBut/2+max*tick;
				g.FillRectangle(Brushes.Black,4,2,min*tick,16);
				if(isEnabled) {
					using(LinearGradientBrush linearGradientBrush=new LinearGradientBrush(new PointF(xLeft-0.01f,0),new PointF(xRight+0.01f,0),Color.Black,Color.White)) {
						g.FillRectangle(linearGradientBrush,xLeft,2,(max-min)*tick,16);
					}
				}
				g.FillRectangle(Brushes.White,xRight,2,width-widthBut-max*tick,16);
				float[] arrayX=new float[] { xLeft,xRight };
				for(int i=0;i<2;i++) {
					EnumUiButtonState buttonState=arrayStates[i==0 ? 0 : 2];
					PointF[] arrayPoints=new PointF[] { new PointF(-3.5f,16),new PointF(-3.5f,3),new PointF(0,0),new PointF(3.5f,3),new PointF(3.5f,16),new PointF(0,19) }
						.Select(x => new PointF(x.X+arrayX[i],x.Y+0.5f)).ToArray();
					using(SolidBrush brush=new SolidBrush(L(UiSliderPaint.GetHandleFill(buttonState))))
					using(Pen pen=new Pen(L(UiSliderPaint.GetHandleOutline(buttonState)),1f)) {
						g.FillPolygon(brush,arrayPoints);
						g.DrawPolygon(pen,arrayPoints);
					}
				}
				using(Pen pen=new Pen(L(UiSliderPaint.GetHandleOutline(arrayStates[1])),1f)) {
					g.DrawLine(pen,xLeft,0.5f,xRight,0.5f);
					g.DrawLine(pen,xLeft,19.5f,xRight,19.5f);
				}
			}
			return bitmap;
		}

		///<summary>The WPF ZoomSlider at its geometry (DIP, rendered at the scale): SurfaceSubtle, the cell fill (4 px corners), the value box, then the
		///lines over them (the dividers Stroke, the tick line StrokeStrong, the bottom Gridline) as its canvas draws them.</summary>
		public static Bitmap WpfZoomSlider(ChromePicture chromePicture,ChromeState state,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(chromePicture.Bitmap.Size,L(EnumUiColor.SurfaceSubtle),scale,out g,out p);
			using(g)
			using(p) {
				bool isEnabled=chromePicture.V<bool>("enabled");
				if(state==ChromeState.Hover) {
					p.Fill(EnumUiColor.SubtleHover,chromePicture.R("rectangleMinus"),UiTheme.CORNER_CONTROL);
				}
				if(state==ChromeState.Pressed) {
					p.Fill(EnumUiColor.SubtlePressed,chromePicture.R("rectangleFit"),UiTheme.CORNER_CONTROL);
				}
				g.ScaleTransform(scale,scale);
				g.SmoothingMode=SmoothingMode.AntiAlias;
				List<double[]> listLines=chromePicture.V<List<double[]>>("lines");
				List<System.Windows.Media.Color> listStrokes=chromePicture.V<List<System.Windows.Media.Color>>("lineStrokes");
				for(int i=0;i<listLines.Count;i++) {
					double[] arrayLine=listLines[i];
					using(Pen pen=new Pen(Color.FromArgb(listStrokes[i].R,listStrokes[i].G,listStrokes[i].B),1f)) {
						g.DrawLine(pen,(float)arrayLine[0],(float)arrayLine[1],(float)arrayLine[2],(float)arrayLine[3]);
					}
				}
				double sliderLeft=chromePicture.V<double>("sliderLeft");
				double sliderWidth=chromePicture.V<double>("sliderWidth");
				EnumUiColor uiColorBox=!isEnabled ? EnumUiColor.Stroke : (state==ChromeState.Selected ? EnumUiColor.Accent
					: (state==ChromeState.HoverSecond ? EnumUiColor.StrokeStrongHover : EnumUiColor.StrokeStrong));
				using(SolidBrush brush=new SolidBrush(L(EnumUiColor.SurfaceRaised)))
				using(Pen pen=new Pen(L(uiColorBox),1f)) {
					g.FillRectangle(brush,(float)sliderLeft+0.5f,9.5f,(float)sliderWidth-1,13.5f);
					g.DrawRectangle(pen,(float)sliderLeft+0.5f,9.5f,(float)sliderWidth-1,13.5f);
				}
				double triangleLeft=chromePicture.V<double>("triangleLeft");
				using(SolidBrush brush=new SolidBrush(L(isEnabled ? EnumUiColor.Text : EnumUiColor.TextDisabled))) {
					g.FillPolygon(brush,new PointF[] { new PointF((float)triangleLeft-2.5f,9),new PointF((float)triangleLeft,4),new PointF((float)triangleLeft+2.5f,9) });
				}
			}
			return bitmap;
		}
	}
}
