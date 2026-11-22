// UI-12 pilot (docs/port-notes/UI-12.md, the owner's G-UI3 row "drop down sytle inconsistent" on FormPayment): every single-line input draws
// the approved combo box's frame (section 3.6, UI-4b-1): the fill, a 1 px StrokeStrong border with 4 px corners, and with focus a 2 px Accent
// line inside the bottom edge. The program's own ComboBox paints itself; the stock single-line text box (and the program's Valid* boxes),
// the up-down boxes and the date picker keep their Windows behaviour and get the frame painted over their Windows border, so the thick
// black or 3-D edges and the date picker's drop button look like the combo box's. FormODBase attaches it to its controls when it makes its
// LayoutManager (AttachTree). Round 2: the stock (editable) ComboBox too, its Windows drop button covered by the program's chevron. Polish
// round: every multi-line text box and rich text box with a border too (one framed look for every text box), and the up-down boxes' arrow
// buttons drawn in the theme (UiUpDownLook). UI-13 fix round: the themed date picker's whole drop button is covered (its rectangle from
// DTM_GETDATETIMEPICKERINFO; Windows 11's calendar button is wider than the scroll bar width the frame covered), and its value is recoloured
// from Windows' black to the Text token.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI {
	///<summary>Paints the combo box's frame over a stock single-line input (TextBox, NumericUpDown, DateTimePicker, ComboBox).</summary>
	public class UiInputFrame : NativeWindow {
		#region Fields
		private const int WM_PAINT=0x000F;
		private const int WM_NCPAINT=0x0085;
		private const int WM_PRINT=0x0317;
		private const int WM_SETFOCUS=0x0007;
		private const int WM_KILLFOCUS=0x0008;
		private const int RDW_INVALIDATE=0x0001;
		private const int RDW_FRAME=0x0400;
		private const int RDW_UPDATENOW=0x0100;
		///<summary>The inputs that have a frame: a control is attached once.</summary>
		private static readonly ConditionalWeakTable<Control,UiInputFrame> _table=new ConditionalWeakTable<Control,UiInputFrame>();
		private readonly Control _control;
		#endregion Fields

		private UiInputFrame(Control control) {
			_control=control;
			if(control.IsHandleCreated) {
				AssignHandle(control.Handle);
			}
			control.HandleCreated+=(sender,e) => AssignHandle(_control.Handle);
			control.HandleDestroyed+=(sender,e) => ReleaseHandle();
			control.GotFocus+=(sender,e) => Redraw();
			control.LostFocus+=(sender,e) => Redraw();
			control.EnabledChanged+=(sender,e) => Redraw();
			if(control is UpDownBase) {
				//The up-down box's text is a child edit: its focus is the box's focus.
				foreach(Control child in control.Controls) {
					child.GotFocus+=(sender,e) => Redraw();
					child.LostFocus+=(sender,e) => Redraw();
				}
			}
		}

		#region Methods - Public
		///<summary>True for the inputs that get the frame: a single-line text box with a 3-D border (the program's Valid* boxes are text boxes),
		///a multi-line text box with a border (polish round), a rich text box (ODtextBox) with a border, single-line or multi-line (fix round), an
		///up-down box, a date picker. Not the text inside an up-down box (its box has the frame), not a borderless box.</summary>
		public static bool IsFramed(Control control) {
			if(control is UpDownBase upDownBase) {
				//Not a borderless up-down box: FormScreenEdit's age arrows stand inside the age box's frame (UI-12 fix round).
				return upDownBase.BorderStyle!=BorderStyle.None;
			}
			if(control is DateTimePicker) {
				return true;
			}
			if(control.GetType()==typeof(System.Windows.Forms.ComboBox)) {
				//The stock combo box (the program's own ComboBox paints itself): not the list-only Simple style.
				return ((System.Windows.Forms.ComboBox)control).DropDownStyle!=ComboBoxStyle.Simple;
			}
			if(control is TextBoxBase textBoxBase) {
				if(textBoxBase.Parent is UpDownBase || textBoxBase.BorderStyle==BorderStyle.None) {
					return false;
				}
				if(textBoxBase is TextBox textBox && (!textBox.Multiline || IsOneLineTall(textBox))) {
					return textBox.BorderStyle==BorderStyle.Fixed3D;
				}
				//UI-12 polish round: every multi-line box with a border (the stock TextBox's square grey line, the RichTextBox's and ODtextBox's 3-D
				//edge) gets the one framed look of the single-line inputs. Fix round: a RichTextBox (ODtextBox) with a border is framed one-line or
				//multi-line (FormEmailInbox's address and subject boxes, ControlChart's search box are single-line ODtextBoxes).
				return true;
			}
			return false;
		}

		///<summary>UI-12 fix round: a multi-line text box drawn one line tall (FormPatientEdit's Referred From, FormScreenEdit's Birthdate) is a
		///single-line field to the eye, so it gets the single-line look: shorter than two lines of its font with the border.</summary>
		public static bool IsOneLineTall(TextBox textBox) {
			return textBox.Height<2*textBox.Font.Height+4;
		}

		///<summary>Attaches the frame to every framed input under root.</summary>
		public static void AttachTree(Control root) {
			if(root==null) {
				return;
			}
			foreach(Control control in root.Controls) {
				AttachOne(control);
				AttachTree(control);
			}
		}

		///<summary>Attaches every shared look that applies to the one control (not its children): the frame, the radio button's and the up-down
		///box's looks (UI-12), and UI-13's looks (UiLooks.AttachOne: scroll bars, slider, stock buttons, grey label blocks and the rest).</summary>
		public static void AttachOne(Control control) {
			if(control==null) {
				return;
			}
			if(IsFramed(control)) {
				Attach(control);
			}
			else if(control is RadioButton) {
				//Round 3: the stock radio button drawn like the program's check box (UiRadioLook).
				UiRadioLook.Attach(control);
			}
			else if(UiUpDownLook.IsButtons(control)) {
				//Polish round: the up-down box's arrow buttons drawn like the combo box's drop area (UiUpDownLook).
				UiUpDownLook.Attach(control);
			}
			UiLooks.AttachOne(control);
		}

		public static void Attach(Control control) {
			if(control==null || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiInputFrame(control));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>The frame over the control's whole bounds (rectangle in g's coordinates): the corners in the parent's colour, the combo box's
		///fill only along the 2 px edge the Windows border used (the text inside is left as Windows drew it), its border, and the focus line. A
		///date picker's drop button is covered by the combo box's chevron.</summary>
		public static void PaintFrame(Graphics g,Rectangle rectangle,Control control,bool isFocused) {
			float scale=GetScale(control);
			int edge=GetEdgeWidth(control);
			Color colorBack=control.Enabled ? control.BackColor : SystemColors.Control;
			if(control is DateTimePicker || control is System.Windows.Forms.ComboBox) {
				colorBack=control.Enabled ? SystemColors.Window : SystemColors.Control;
			}
			Color colorParent=control.Parent?.BackColor ?? SystemColors.Control;
			if(colorParent.A<255) {
				colorParent=SystemColors.Control;
			}
			GraphicsState graphicsState=g.Save();
			try {
				g.SmoothingMode=SmoothingMode.AntiAlias;
				Region region=new Region(rectangle);
				Rectangle rectangleInner=GetInnerRectangle(rectangle,control);
				region.Exclude(rectangleInner);
				g.Clip=region;
				//UI-13: the parent's colour on whole pixels. With antialiasing and the default pixel offset the fill covered the edge pixels by half
				//and the corner pixel by a quarter, so Windows' dark border showed through under each rounded corner (UI-12.md section 5).
				g.SmoothingMode=SmoothingMode.None;
				using(SolidBrush brushParent=new SolidBrush(colorParent)) {
					g.FillRectangle(brushParent,rectangle);
				}
				g.SmoothingMode=SmoothingMode.AntiAlias;
				Rectangle rectangleBox=new Rectangle(rectangle.X,rectangle.Y,rectangle.Width,rectangle.Height);
				UiListPaint.DrawComboBoxBox(g,rectangleBox,false,control.Enabled,scale,colorBack);
				EnumUiColor uiColorChevron=control.Enabled ? EnumUiColor.Text : EnumUiColor.TextDisabled;
				Rectangle rectangleButton=GetDropButtonRectangle(control);
				if(control is DateTimePicker && !rectangleButton.IsEmpty) {
					//UI-13 fix round: the themed date picker's whole drop button is covered (rectangleInner stops 1 px left of it). The chevron is at the
					//combo box's place, so a date picker over a combo box keeps one chevron column; centred in the covered area when the button is too
					//narrow for that place.
					int left=rectangle.X+rectangleButton.Left-1;
					Rectangle rectangleChevron=UiListPaint.GetChevronRectangle(rectangleBox,scale);
					if(rectangleChevron.Left<left) {
						rectangleChevron.X=left+(rectangle.Right-1-left-rectangleChevron.Width)/2;
					}
					UiIcons.Draw(g,UiGlyph.ChevronDown,rectangleChevron,uiColorChevron);
				}
				else if(control is DateTimePicker || control is System.Windows.Forms.ComboBox) {
					UiListPaint.DrawChevron(g,rectangleBox,uiColorChevron,scale);
				}
				if(isFocused && control.Enabled) {
					UiListPaint.DrawComboFocusLine(g,rectangleBox,scale);
				}
				region.Dispose();
			}
			finally {
				g.Restore(graphicsState);
			}
		}
		///<summary>The part of the control the frame leaves as Windows drew it: inside the edge, less a date picker's or combo box's drop button,
		///which the frame covers with the chevron. A date picker's button is the one Windows reports (DTM_GETDATETIMEPICKERINFO: with Windows'
		///visual styles it is wider than a scroll bar), covered from 1 px left of it; otherwise (a combo box, or a picker that does not answer the
		///message, as without visual styles) the width of a scroll bar inside the edge, at the right.</summary>
		public static Rectangle GetInnerRectangle(Rectangle rectangle,Control control) {
			Rectangle rectangleInner=Rectangle.Inflate(rectangle,-GetEdgeWidth(control),-GetEdgeWidth(control));
			Rectangle rectangleButton=GetDropButtonRectangle(control);
			if(control is DateTimePicker && !rectangleButton.IsEmpty) {
				int left=rectangle.X+rectangleButton.Left-1;
				rectangleInner.Width=Math.Max(0,Math.Min(rectangleInner.Width,left-rectangleInner.X));
			}
			else if(control is DateTimePicker || control is System.Windows.Forms.ComboBox) {
				rectangleInner.Width-=SystemInformation.VerticalScrollBarWidth+2;
			}
			return rectangleInner;
		}

		///<summary>UI-13 fix round: a date picker's drop button in the coordinates of its window (its top left corner, as the frame paints), from
		///DTM_GETDATETIMEPICKERINFO (rcButton is in client coordinates). Empty when the control is not a date picker with a handle, when Windows
		///does not answer the message (comctl32 before version 6) or the picker has no drop button (ShowUpDown).</summary>
		public static Rectangle GetDropButtonRectangle(Control control) {
			if(!(control is DateTimePicker) || !control.IsHandleCreated) {
				return Rectangle.Empty;
			}
			DATETIMEPICKERINFO dateTimePickerInfo=new DATETIMEPICKERINFO();
			dateTimePickerInfo.cbSize=Marshal.SizeOf(typeof(DATETIMEPICKERINFO));
			SendMessage(control.Handle,DTM_GETDATETIMEPICKERINFO,IntPtr.Zero,ref dateTimePickerInfo);
			RECT rect=dateTimePickerInfo.rcButton;
			if(rect.Right<=rect.Left || rect.Bottom<=rect.Top) {
				return Rectangle.Empty;
			}
			Point pointClient=GetClientOffset(control.Handle);
			return Rectangle.FromLTRB(rect.Left+pointClient.X,rect.Top+pointClient.Y,rect.Right+pointClient.X,rect.Bottom+pointClient.Y);
		}

		///<summary>UI-13 fix round: the date picker's value as Windows drew it (pure black, its theme's text colour, which no property of the stock
		///picker changes) recoloured to the Text token, in the part of the window the frame leaves (left of the drop button, right of the check
		///box), inside rectangleUpdate (window coordinates). Every channel darker than the box's colour is moved linearly from black to the
		///token, so antialiased and ClearType pixels keep their shape. Only an enabled, unfocused picker in the light look: a focused picker shows
		///its selected field in Windows' highlight, a disabled one its grey, and high contrast its own colours. The caller passes pixels Windows
		///has just drawn, so none is recoloured twice.</summary>
		public static void RecolorDateText(IntPtr hdc,Control control,Rectangle rectangleUpdate) {
			if(!(control is DateTimePicker dateTimePicker) || !control.Enabled || control.ContainsFocus || SystemInformation.HighContrast) {
				return;
			}
			Rectangle rectangleText=GetInnerRectangle(new Rectangle(0,0,control.Width,control.Height),control);
			if(dateTimePicker.ShowCheckBox && control.IsHandleCreated) {
				DATETIMEPICKERINFO dateTimePickerInfo=new DATETIMEPICKERINFO();
				dateTimePickerInfo.cbSize=Marshal.SizeOf(typeof(DATETIMEPICKERINFO));
				SendMessage(control.Handle,DTM_GETDATETIMEPICKERINFO,IntPtr.Zero,ref dateTimePickerInfo);
				int rightCheck=dateTimePickerInfo.rcCheck.Right+GetClientOffset(control.Handle).X;
				if(rightCheck>rectangleText.Left) {
					rectangleText=Rectangle.FromLTRB(rightCheck,rectangleText.Top,rectangleText.Right,rectangleText.Bottom);
				}
			}
			rectangleText.Intersect(rectangleUpdate);
			if(rectangleText.Width<=0 || rectangleText.Height<=0) {
				return;
			}
			Color colorBack=SystemColors.Window;
			Color colorText=UiTheme.GetColor(EnumUiColor.Text);
			using(Bitmap bitmap=new Bitmap(rectangleText.Width,rectangleText.Height,System.Drawing.Imaging.PixelFormat.Format32bppRgb)) {
				using(Graphics g=Graphics.FromImage(bitmap)) {
					IntPtr hdcBitmap=g.GetHdc();
					try {
						BitBlt(hdcBitmap,0,0,rectangleText.Width,rectangleText.Height,hdc,rectangleText.X,rectangleText.Y,SRCCOPY);
					}
					finally {
						g.ReleaseHdc(hdcBitmap);
					}
				}
				System.Drawing.Imaging.BitmapData bitmapData=bitmap.LockBits(new Rectangle(0,0,bitmap.Width,bitmap.Height),
					System.Drawing.Imaging.ImageLockMode.ReadWrite,System.Drawing.Imaging.PixelFormat.Format32bppRgb);
				try {
					int count=bitmapData.Stride*bitmap.Height;
					byte[] arrayBytes=new byte[count];
					Marshal.Copy(bitmapData.Scan0,arrayBytes,0,count);
					//Blue, green, red in each 4 byte pixel.
					for(int y=0;y<bitmap.Height;y++) {
						for(int x=0;x<bitmap.Width;x++) {
							int i=y*bitmapData.Stride+x*4;
							arrayBytes[i]=RecolorChannel(arrayBytes[i],colorBack.B,colorText.B);
							arrayBytes[i+1]=RecolorChannel(arrayBytes[i+1],colorBack.G,colorText.G);
							arrayBytes[i+2]=RecolorChannel(arrayBytes[i+2],colorBack.R,colorText.R);
						}
					}
					Marshal.Copy(arrayBytes,0,bitmapData.Scan0,count);
				}
				finally {
					bitmap.UnlockBits(bitmapData);
				}
				using(Graphics g=Graphics.FromImage(bitmap)) {
					IntPtr hdcBitmap=g.GetHdc();
					try {
						BitBlt(hdc,rectangleText.X,rectangleText.Y,rectangleText.Width,rectangleText.Height,hdcBitmap,0,0,SRCCOPY);
					}
					finally {
						g.ReleaseHdc(hdcBitmap);
					}
				}
			}
		}

		///<summary>One channel of a pixel of black ink on the box's colour, moved to the token's ink: black becomes the token, the box's colour
		///stays, the blends in between in proportion. A channel at or above the box's colour is left.</summary>
		public static byte RecolorChannel(byte value,byte valueBack,byte valueText) {
			if(value>=valueBack || valueBack==0) {
				return value;
			}
			return (byte)Math.Round(valueBack-(valueBack-value)*(double)(valueBack-valueText)/valueBack);
		}

		#endregion Methods - Public

		#region Methods - Private
		///<summary>The width of the Windows border the frame covers: 2 px for a 3-D edge, 1 px for a single line, at least the frame's line.</summary>
		private static int GetEdgeWidth(Control control) {
			if(control is TextBoxBase textBoxBase && textBoxBase.BorderStyle==BorderStyle.FixedSingle) {
				return 1;
			}
			return 2;
		}

		///<summary>The scale of the frame's line and corners: the control's dpi (LayoutManager sizes the control itself).</summary>
		private static float GetScale(Control control) {
			return Math.Max(1f,control.DeviceDpi/96f);
		}

		private bool IsFocusShown() {
			return _control.ContainsFocus;
		}

		private void Redraw() {
			if(!_control.IsHandleCreated) {
				return;
			}
			RedrawWindow(_control.Handle,IntPtr.Zero,IntPtr.Zero,RDW_INVALIDATE|RDW_FRAME|RDW_UPDATENOW);
		}

		protected override void WndProc(ref Message m) {
			//UI-13 fix round: the part of a date picker Windows is about to paint on screen (its update region, in window coordinates), so only the
			//pixels it paints now get the Text token's colour (RecolorDateText).
			Rectangle[] arrayRectanglesUpdate=null;
			if(m.Msg==WM_PAINT && m.WParam==IntPtr.Zero && _control is DateTimePicker) {
				arrayRectanglesUpdate=GetUpdateRectangles();
			}
			base.WndProc(ref m);
			try {
				switch(m.Msg) {
					case WM_NCPAINT:
					case WM_PAINT:
						if(arrayRectanglesUpdate!=null) {
							RecolorOnWindow(arrayRectanglesUpdate);
						}
						PaintOnWindow();
						break;
					case WM_PRINT:
						if(((int)m.LParam & PRF_CLIENT)!=0) {
							RecolorDateText(m.WParam,_control,new Rectangle(0,0,_control.Width,_control.Height));
						}
						using(Graphics g=Graphics.FromHdc(m.WParam)) {
							PaintFrame(g,new Rectangle(0,0,_control.Width,_control.Height),_control,false);
						}
						break;
				}
			}
			catch(Exception) {
				//A frame that cannot be painted leaves the Windows border; nothing else depends on it.
			}
		}

		private void PaintOnWindow() {
			IntPtr hdc=GetWindowDC(Handle);
			if(hdc==IntPtr.Zero) {
				return;
			}
			try {
				using(Graphics g=Graphics.FromHdc(hdc)) {
					PaintFrame(g,new Rectangle(0,0,_control.Width,_control.Height),_control,IsFocusShown());
				}
			}
			finally {
				ReleaseDC(Handle,hdc);
			}
		}

		///<summary>The window's update region as rectangles in window coordinates (before WM_PAINT's BeginPaint validates it); empty when there
		///is none.</summary>
		private Rectangle[] GetUpdateRectangles() {
			IntPtr hRgn=CreateRectRgn(0,0,0,0);
			try {
				if(GetUpdateRgn(Handle,hRgn,false)<=NULLREGION) {
					return new Rectangle[0];
				}
				Point pointClient=GetClientOffset(Handle);
				using(Region region=Region.FromHrgn(hRgn))
				using(Matrix matrix=new Matrix()) {
					RectangleF[] arrayRectangleFs=region.GetRegionScans(matrix);
					Rectangle[] arrayRectangles=new Rectangle[arrayRectangleFs.Length];
					for(int i=0;i<arrayRectangleFs.Length;i++) {
						Rectangle rectangle=Rectangle.Round(arrayRectangleFs[i]);
						rectangle.Offset(pointClient);
						arrayRectangles[i]=rectangle;
					}
					return arrayRectangles;
				}
			}
			finally {
				DeleteObject(hRgn);
			}
		}

		private void RecolorOnWindow(Rectangle[] arrayRectanglesUpdate) {
			if(arrayRectanglesUpdate.Length==0) {
				return;
			}
			IntPtr hdc=GetWindowDC(Handle);
			if(hdc==IntPtr.Zero) {
				return;
			}
			try {
				foreach(Rectangle rectangle in arrayRectanglesUpdate) {
					RecolorDateText(hdc,_control,rectangle);
				}
			}
			finally {
				ReleaseDC(Handle,hdc);
			}
		}

		///<summary>Where the window's client area starts, from its top left corner (the width of its border).</summary>
		private static Point GetClientOffset(IntPtr hWnd) {
			if(!GetWindowRect(hWnd,out RECT rectWindow)) {
				return Point.Empty;
			}
			POINT point=new POINT();
			if(!ClientToScreen(hWnd,ref point)) {
				return Point.Empty;
			}
			return new Point(point.X-rectWindow.Left,point.Y-rectWindow.Top);
		}

		private const int DTM_GETDATETIMEPICKERINFO=0x1000+14;
		private const int PRF_CLIENT=0x0004;
		private const int NULLREGION=1;
		private const int SRCCOPY=0x00CC0020;

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT {
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT {
			public int X;
			public int Y;
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

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd,out RECT rect);

		[DllImport("user32.dll")]
		private static extern bool ClientToScreen(IntPtr hWnd,ref POINT point);

		[DllImport("user32.dll")]
		private static extern int GetUpdateRgn(IntPtr hWnd,IntPtr hRgn,bool erase);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateRectRgn(int left,int top,int right,int bottom);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);

		[DllImport("gdi32.dll")]
		private static extern bool BitBlt(IntPtr hdcDest,int xDest,int yDest,int width,int height,IntPtr hdcSource,int xSource,int ySource,int rop);

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindowDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern int ReleaseDC(IntPtr hWnd,IntPtr hDC);

		[DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr hWnd,IntPtr lprcUpdate,IntPtr hrgnUpdate,int flags);
		#endregion Methods - Private
	}
}
