// UI-FOCUS (docs/port-notes/UI-FOCUS.md, the owner's "There's shaking when a text box is in focus"): the inputs with UiInputFrame's frame,
// in a FormODBase window shown on screen at 100% and 150% (LayoutManager's zoom), with Windows' visual styles (VisualStylesScope). A probe
// subclasses each input under the frame (it is attached first, so the frame's WndProc calls it): at every WM_NCPAINT and WM_PAINT it reads the
// window's pixels before and after Windows' own painting, before the frame paints. The frame's area (and the frame scroll bars UiScrollLook
// paints) must be the same after Windows' painting as before: Windows never puts its border on screen under the frame. Before UI-FOCUS a text
// box changed there 4 or 5 times on each focus change (Windows 11's animated focus line), the flash the owner saw. Then, after the focus
// animation: the focused picture differs from the unfocused one only in the focus line's rows (no text pixel moves; a date picker's value,
// which Windows highlights when focused, is left out), the focused one shows the Accent line, and the unfocused frame on screen is the frame
// WM_PRINT draws (the pictures the galleries took, G-UI3-ui13 included). Inconclusive when the session draws nothing (a disconnected desktop).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;

namespace UiRefreshTests {
	[TestClass]
	public class InputFocusTests {
		private const int WM_PRINT=0x0317;
		private const int PRF_NONCLIENT=0x0002;
		private const int PRF_CLIENT=0x0004;
		private const int PRF_ERASEBKGND=0x0008;
		private const int SRCCOPY=0x00CC0020;
		///<summary>Long enough for Windows 11's focus animation (about 200 ms) to end.</summary>
		private const int MS_ANIMATION=700;

		[TestMethod]
		public void InputFrame_FocusChange_NoWindowsBorderAndNoMove_100() {
			RunFocusCheck(zoomTest:0);
		}

		[TestMethod]
		public void InputFrame_FocusChange_NoWindowsBorderAndNoMove_150() {
			RunFocusCheck(zoomTest:50);
		}

		#region Helpers
		private static void RunFocusCheck(int zoomTest) {
			List<string> listFailures=new List<string>();
			string inconclusive=null;
			StaRunnerHelpers.RunSta(() => {
				using VisualStylesScope visualStylesScope=new VisualStylesScope();
				Assert.IsTrue(visualStylesScope.IsActive,"Windows' visual styles could not be activated for the inputs.");
				using TestFocusWindow window=new TestFocusWindow(zoomTest);
				window.Show();
				try {
					Pump(400);
					window.buttonFocus.Focus();
					Pump(MS_ANIMATION);
					int[] pixelsText=CaptureWindow(window.textBox.Handle,window.textBox.Width,window.textBox.Height);
					if(IsUniform(pixelsText)) {
						inconclusive="The session draws nothing (a disconnected desktop?): the inputs on screen cannot be read.";
						return;
					}
					if(!window.buttonFocus.Focused) {
						inconclusive="The test window could not take the focus in this session.";
						return;
					}
					if(zoomTest!=0 && window.textBox.Height<window.HeightTextBoxDesign*1.3) {
						listFailures.Add("The 150% layout was not applied: the text box is "+window.textBox.Height+" px tall (designed "
							+window.HeightTextBoxDesign+").");
					}
					int countPaints=0;
					foreach(Control control in window.ListInputs) {
						countPaints+=CheckOne(window,control,listFailures);
					}
					if(countPaints==0) {
						listFailures.Add("The probes saw no paint: they are not in the inputs' message chain, so nothing was checked.");
					}
				}
				finally {
					window.Close();
				}
			});
			if(inconclusive!=null) {
				Assert.Inconclusive(inconclusive);
			}
			Assert.AreEqual(0,listFailures.Count,"Scale "+(100+zoomTest)+"%:\r\n"+string.Join("\r\n",listFailures));
		}

		///<summary>Focuses the input and back, checking every paint on the way and the steady pictures. Returns the paints the probe saw.</summary>
		private static int CheckOne(TestFocusWindow window,Control control,List<string> listFailures) {
			string name=control.Name;
			Assert.IsTrue(UiInputFrame.IsAttached(control),name+" has no input frame.");
			PaintProbe paintProbe=window.DictProbes[control];
			int width=control.Width;
			int height=control.Height;
			int[] pixelsUnfocused=CaptureWindow(control.Handle,width,height);
			paintProbe.Start();
			control.Focus();
			ClearSelection(control);
			Pump(MS_ANIMATION);
			HideCaret(IntPtr.Zero);
			Pump(50);
			bool isFocused=control.ContainsFocus;
			int[] pixelsFocused=CaptureWindow(control.Handle,width,height);
			window.buttonFocus.Focus();
			Pump(MS_ANIMATION);
			paintProbe.Stop();
			if(!isFocused) {
				listFailures.Add(name+": the focus did not move to it.");
				return paintProbe.CountPaints;
			}
			listFailures.AddRange(paintProbe.ListFlashes);
			bool[] maskFrame=GetFrameMask(control,width,height);
			//Focused and unfocused: the same pixels but in the focus line's rows (a date picker's value: Windows highlights its field when focused).
			int rowFocusLine=height-2;
			int countMoved=0;
			string firstMoved="";
			for(int y=0;y<rowFocusLine;y++) {
				for(int x=0;x<width;x++) {
					int i=y*width+x;
					if(control is DateTimePicker && !maskFrame[i]) {
						continue;
					}
					if(pixelsUnfocused[i]!=pixelsFocused[i]) {
						if(countMoved==0) {
							firstMoved=" first at ("+x+","+y+") "+pixelsUnfocused[i].ToString("X6")+" to "+pixelsFocused[i].ToString("X6");
						}
						countMoved++;
					}
				}
			}
			if(countMoved>0) {
				listFailures.Add(name+": "+countMoved+" pixels above the focus line differ between unfocused and focused;"+firstMoved);
			}
			//The focus line: Accent in the bottom row half way across when focused, not when unfocused.
			Color colorAccent=UiTheme.GetColor(EnumUiColor.Accent);
			int iLine=(height-1)*width+width/3;
			if(!IsNear(pixelsFocused[iLine],colorAccent)) {
				listFailures.Add(name+": no Accent focus line when focused ("+pixelsFocused[iLine].ToString("X6")+" at the bottom).");
			}
			if(IsNear(pixelsUnfocused[iLine],colorAccent)) {
				listFailures.Add(name+": the Accent focus line shows when unfocused.");
			}
			//The unfocused frame on screen is the frame WM_PRINT draws (the galleries' pictures), but where a child window stands over it.
			int[] pixelsPrinted=PrintWindowPixels(control,width,height);
			bool[] maskChildren=GetChildMask(control,width,height);
			int countLook=0;
			string firstLook="";
			for(int i=0;i<pixelsPrinted.Length;i++) {
				if(!maskFrame[i] || maskChildren[i] || pixelsPrinted[i]==pixelsUnfocused[i]) {
					continue;
				}
				if(countLook==0) {
					firstLook=" first at ("+(i%width)+","+(i/width)+") screen "+pixelsUnfocused[i].ToString("X6")+", printed "+pixelsPrinted[i].ToString("X6");
				}
				countLook++;
			}
			if(countLook>0) {
				listFailures.Add(name+": "+countLook+" frame pixels on screen differ from the printed frame;"+firstLook);
			}
			return paintProbe.CountPaints;
		}

		private static void ClearSelection(Control control) {
			if(control is TextBoxBase textBoxBase) {
				textBoxBase.Select(0,0);
			}
			else if(control is UpDownBase upDownBase) {
				upDownBase.Select(0,0);
			}
			else if(control is System.Windows.Forms.ComboBox comboBox) {
				comboBox.SelectionStart=0;
				comboBox.SelectionLength=0;
			}
		}

		///<summary>True for the frame's area: the window less UiInputFrame.GetInnerRectangle, and the frame scroll bars UiScrollLook paints.</summary>
		internal static bool[] GetFrameMask(Control control,int width,int height) {
			bool[] mask=new bool[width*height];
			Rectangle rectangleInner=UiInputFrame.GetInnerRectangle(new Rectangle(0,0,width,height),control);
			List<Rectangle> listRectanglesBar=new List<Rectangle>();
			Point pointWindow=GetWindowOrigin(control.Handle);
			foreach(Rectangle rectangleBar in UiScrollLook.GetFrameBarRectanglesScreen(control)) {
				Rectangle rectangle=rectangleBar;
				rectangle.Offset(-pointWindow.X,-pointWindow.Y);
				listRectanglesBar.Add(rectangle);
			}
			for(int y=0;y<height;y++) {
				for(int x=0;x<width;x++) {
					bool isFrame=!rectangleInner.Contains(x,y);
					foreach(Rectangle rectangle in listRectanglesBar) {
						isFrame|=rectangle.Contains(x,y);
					}
					mask[y*width+x]=isFrame;
				}
			}
			return mask;
		}

		///<summary>True where a child window of the control stands (an up-down box's edit and arrow buttons), in window coordinates.</summary>
		private static bool[] GetChildMask(Control control,int width,int height) {
			bool[] mask=new bool[width*height];
			Point pointClient=control.PointToScreen(Point.Empty);
			Point pointWindow=GetWindowOrigin(control.Handle);
			foreach(Control child in control.Controls) {
				if(!child.Visible) {
					continue;
				}
				Rectangle rectangle=child.Bounds;
				rectangle.Offset(pointClient.X-pointWindow.X,pointClient.Y-pointWindow.Y);
				for(int y=Math.Max(0,rectangle.Top);y<Math.Min(height,rectangle.Bottom);y++) {
					for(int x=Math.Max(0,rectangle.Left);x<Math.Min(width,rectangle.Right);x++) {
						mask[y*width+x]=true;
					}
				}
			}
			return mask;
		}

		///<summary>The window's pixels as they are on screen (its DWM surface, read through its window DC), 0xRRGGBB.</summary>
		internal static int[] CaptureWindow(IntPtr hWnd,int width,int height) {
			using Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppRgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				IntPtr hdcBitmap=g.GetHdc();
				IntPtr hdcWindow=GetWindowDC(hWnd);
				try {
					BitBlt(hdcBitmap,0,0,width,height,hdcWindow,0,0,SRCCOPY);
				}
				finally {
					ReleaseDC(hWnd,hdcWindow);
					g.ReleaseHdc(hdcBitmap);
				}
			}
			return GetPixels(bitmap);
		}

		///<summary>The control printed through WM_PRINT (frame, client, background), as the galleries' harness prints it.</summary>
		private static int[] PrintWindowPixels(Control control,int width,int height) {
			using Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppRgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.Magenta);
				IntPtr hdc=g.GetHdc();
				try {
					SendMessage(control.Handle,WM_PRINT,hdc,(IntPtr)(PRF_NONCLIENT|PRF_CLIENT|PRF_ERASEBKGND));
				}
				finally {
					g.ReleaseHdc(hdc);
				}
			}
			return GetPixels(bitmap);
		}

		private static int[] GetPixels(Bitmap bitmap) {
			BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppRgb);
			int[] pixels=new int[bitmap.Width*bitmap.Height];
			try {
				for(int y=0;y<bitmap.Height;y++) {
					Marshal.Copy(bitmapData.Scan0+y*bitmapData.Stride,pixels,y*bitmap.Width,bitmap.Width);
				}
			}
			finally {
				bitmap.UnlockBits(bitmapData);
			}
			for(int i=0;i<pixels.Length;i++) {
				pixels[i]&=0xFFFFFF;
			}
			return pixels;
		}

		///<summary>The window's top left corner on screen.</summary>
		private static Point GetWindowOrigin(IntPtr hWnd) {
			GetWindowRect(hWnd,out RECT rect);
			return new Point(rect.Left,rect.Top);
		}

		private static bool IsUniform(int[] pixels) {
			for(int i=1;i<pixels.Length;i++) {
				if(pixels[i]!=pixels[0]) {
					return false;
				}
			}
			return true;
		}

		private static bool IsNear(int pixel,Color color) {
			int r=(pixel>>16)&0xFF;
			int g=(pixel>>8)&0xFF;
			int b=pixel&0xFF;
			return Math.Abs(r-color.R)<=24 && Math.Abs(g-color.G)<=24 && Math.Abs(b-color.B)<=24;
		}

		private static void Pump(int milliseconds) {
			DateTime dateTimeEnd=DateTime.Now.AddMilliseconds(milliseconds);
			while(DateTime.Now<dateTimeEnd) {
				Application.DoEvents();
				System.Threading.Thread.Sleep(5);
			}
		}

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindowDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern int ReleaseDC(IntPtr hWnd,IntPtr hDC);

		[DllImport("gdi32.dll")]
		private static extern bool BitBlt(IntPtr hdcDest,int xDest,int yDest,int width,int height,IntPtr hdcSource,int xSource,int ySource,int rop);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd,int msg,IntPtr wParam,IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool HideCaret(IntPtr hWnd);

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT {
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd,out RECT rect);
		#endregion Helpers
	}

	///<summary>Subclasses an input under its frame (attached before UiInputFrame: the frame's WndProc calls it) and, while started, reads the
	///frame's area before and after Windows' own WM_NCPAINT and WM_PAINT, before the frame paints: a change is Windows' border on screen.</summary>
	public class PaintProbe : NativeWindow {
		private const int WM_PAINT=0x000F;
		private const int WM_NCPAINT=0x0085;
		private readonly Control _control;
		private bool _isStarted;
		public int CountPaints;
		public List<string> ListFlashes=new List<string>();

		public PaintProbe(Control control) {
			_control=control;
			if(control.IsHandleCreated) {
				AssignHandle(control.Handle);
			}
			control.HandleCreated+=(sender,e) => AssignHandle(_control.Handle);
			control.HandleDestroyed+=(sender,e) => ReleaseHandle();
		}

		public void Start() {
			CountPaints=0;
			ListFlashes.Clear();
			_isStarted=true;
		}

		public void Stop() {
			_isStarted=false;
		}

		protected override void WndProc(ref Message m) {
			bool isPaint=_isStarted && Handle!=IntPtr.Zero && (m.Msg==WM_NCPAINT || (m.Msg==WM_PAINT && m.WParam==IntPtr.Zero));
			int width=_control.Width;
			int height=_control.Height;
			int[] pixelsBefore=null;
			bool[] mask=null;
			if(isPaint) {
				mask=InputFocusTests.GetFrameMask(_control,width,height);
				pixelsBefore=InputFocusTests.CaptureWindow(Handle,width,height);
			}
			base.WndProc(ref m);
			if(!isPaint) {
				return;
			}
			CountPaints++;
			int[] pixelsAfter=InputFocusTests.CaptureWindow(Handle,width,height);
			int count=0;
			string first="";
			for(int i=0;i<pixelsAfter.Length;i++) {
				if(!mask[i] || pixelsBefore[i]==pixelsAfter[i]) {
					continue;
				}
				if(count==0) {
					first=" first at ("+(i%width)+","+(i/width)+") "+pixelsBefore[i].ToString("X6")+" to "+pixelsAfter[i].ToString("X6");
				}
				count++;
			}
			if(count>0 && ListFlashes.Count<4) {
				ListFlashes.Add(_control.Name+": Windows' "+(m.Msg==WM_NCPAINT ? "WM_NCPAINT" : "WM_PAINT")+" changed "+count
					+" pixels of the frame's area before the frame was painted (a flash on screen);"+first);
			}
		}
	}

	///<summary>A FormODBase window with one of each framed input and a button that holds the focus between them, written like designer code; a
	///probe is attached to each input before InitializeLayoutManager attaches the frames.</summary>
	public class TestFocusWindow : FormODBase {
		public System.Windows.Forms.Button buttonFocus;
		public System.Windows.Forms.TextBox textBox;
		public System.Windows.Forms.TextBox textBoxMulti;
		public ValidDouble validDouble;
		public System.Windows.Forms.RichTextBox richTextBox;
		public System.Windows.Forms.ComboBox comboBox;
		public System.Windows.Forms.NumericUpDown numericUpDown;
		public System.Windows.Forms.DateTimePicker dateTimePicker;
		public List<Control> ListInputs;
		public Dictionary<Control,PaintProbe> DictProbes=new Dictionary<Control,PaintProbe>();
		public int HeightTextBoxDesign;

		public TestFocusWindow(int zoomTest) {
			InitializeComponent();
			ListInputs=new List<Control> { textBox,textBoxMulti,validDouble,richTextBox,comboBox,numericUpDown,dateTimePicker };
			foreach(Control control in ListInputs) {
				DictProbes[control]=new PaintProbe(control);
			}
			HeightTextBoxDesign=textBox.Height;
			InitializeLayoutManager();
			if(zoomTest!=0) {
				LayoutManager.ZoomTest=zoomTest;
			}
		}

		private void InitializeComponent() {
			buttonFocus=new System.Windows.Forms.Button();
			textBox=new System.Windows.Forms.TextBox();
			textBoxMulti=new System.Windows.Forms.TextBox();
			validDouble=new ValidDouble();
			richTextBox=new System.Windows.Forms.RichTextBox();
			comboBox=new System.Windows.Forms.ComboBox();
			numericUpDown=new System.Windows.Forms.NumericUpDown();
			dateTimePicker=new System.Windows.Forms.DateTimePicker();
			((System.ComponentModel.ISupportInitialize)numericUpDown).BeginInit();
			SuspendLayout();
			buttonFocus.Location=new Point(320,10);
			buttonFocus.Name="buttonFocus";
			buttonFocus.Size=new Size(75,24);
			buttonFocus.Text="OK";
			textBox.Location=new Point(10,10);
			textBox.Name="textBox";
			textBox.Size=new Size(150,20);
			textBox.Text="Smith, Jane";
			textBoxMulti.Location=new Point(10,40);
			textBoxMulti.Multiline=true;
			textBoxMulti.Name="textBoxMulti";
			textBoxMulti.ScrollBars=ScrollBars.Vertical;
			textBoxMulti.Size=new Size(150,60);
			textBoxMulti.Text="First line\r\nSecond line\r\nThird line\r\nFourth line\r\nFifth line";
			validDouble.Location=new Point(10,110);
			validDouble.Name="validDouble";
			validDouble.Size=new Size(80,20);
			validDouble.Text="12.50";
			richTextBox.Location=new Point(10,140);
			richTextBox.Name="richTextBox";
			richTextBox.Size=new Size(150,46);
			richTextBox.Text="Rich text";
			comboBox.Location=new Point(180,40);
			comboBox.Name="comboBox";
			comboBox.Size=new Size(120,21);
			comboBox.Items.AddRange(new object[] { "Primary","Secondary" });
			comboBox.Text="Primary";
			numericUpDown.Location=new Point(180,70);
			numericUpDown.Name="numericUpDown";
			numericUpDown.Size=new Size(80,20);
			numericUpDown.Value=3;
			dateTimePicker.Format=DateTimePickerFormat.Short;
			dateTimePicker.Location=new Point(180,100);
			dateTimePicker.Name="dateTimePicker";
			dateTimePicker.Size=new Size(120,20);
			dateTimePicker.Value=new DateTime(2026,11,23);
			ClientSize=new Size(420,220);
			Controls.Add(buttonFocus);
			Controls.Add(textBox);
			Controls.Add(textBoxMulti);
			Controls.Add(validDouble);
			Controls.Add(richTextBox);
			Controls.Add(comboBox);
			Controls.Add(numericUpDown);
			Controls.Add(dateTimePicker);
			Name="TestFocusWindow";
			ShowInTaskbar=false;
			StartPosition=FormStartPosition.Manual;
			Location=new Point(120,120);
			Text="Focus";
			((System.ComponentModel.ISupportInitialize)numericUpDown).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
