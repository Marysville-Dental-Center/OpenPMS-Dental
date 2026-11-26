// UI-12 pilot (docs/port-notes/UI-12.md, the owner's G-UI3 row "drop down sytle inconsistent" on FormPayment): every single-line input draws
// the approved combo box's frame (section 3.6, UI-4b-1): the fill, a 1 px StrokeStrong border with 4 px corners, and with focus a 2 px Accent
// line inside the bottom edge. The program's own ComboBox paints itself; the stock single-line text box (and the program's Valid* boxes),
// the up-down boxes and the date picker keep their Windows behaviour and get the frame painted over their Windows border, so the thick
// black or 3-D edges and the date picker's drop button look like the combo box's. FormODBase attaches it to its controls when it makes its
// LayoutManager (AttachTree). Round 2: the stock (editable) ComboBox too, its Windows drop button covered by the program's chevron. Polish
// round: every multi-line text box and rich text box with a border too (one framed look for every text box), and the up-down boxes' arrow
// buttons drawn in the theme (UiUpDownLook). UI-13 fix round: the themed date picker's whole drop button is covered (its rectangle from
// DTM_GETDATETIMEPICKERINFO; Windows 11's calendar button is wider than the scroll bar width the frame covered), and its value is recoloured
// from Windows' black to the Text token. UI-FOCUS (docs/port-notes/UI-FOCUS.md, the owner's "shaking when a text box is in focus"): Windows
// never paints its own border under the frame on screen (it flashed for a frame at each of the 4 or 5 steps of Windows 11's focus animation),
// and a focus change paints only the frame again.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CodeBase {
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
		private const int RGN_AND=1;
		private const int RGN_DIFF=4;
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
			control.GotFocus+=(sender,e) => RepaintFocus();
			control.LostFocus+=(sender,e) => RepaintFocus();
			control.EnabledChanged+=(sender,e) => Redraw();
			if(control is UpDownBase) {
				//The up-down box's text is a child edit: its focus is the box's focus.
				foreach(Control child in control.Controls) {
					child.GotFocus+=(sender,e) => RepaintFocus();
					child.LostFocus+=(sender,e) => RepaintFocus();
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
			if(IsFaceDrawn(control)) {
				PaintComboFace(g,GetInnerRectangle(rectangle,control),(System.Windows.Forms.ComboBox)control,colorBack);
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
		///<summary>UI-PLAIN fix round: true for an enabled stock DropDownList combo box that Windows draws. Windows draws its face as a read-only
		///button (its theme's grey, (240,240,240), light blue under the mouse, the text in the highlight with focus), a grey box with the frame's
		///white chevron strip beside the white text boxes; the program's ComboBox is white with the text in the Text token. So the frame paints that
		///face too (PaintComboFace). A disabled one keeps Windows' grey (the disabled inputs' grey); an owner-drawn one its owner's drawing; high
		///contrast and right to left Windows' drawing.</summary>
		public static bool IsFaceDrawn(Control control) {
			if(control==null || control.GetType()!=typeof(System.Windows.Forms.ComboBox)) {
				return false;
			}
			System.Windows.Forms.ComboBox comboBox=(System.Windows.Forms.ComboBox)control;
			return comboBox.DropDownStyle==ComboBoxStyle.DropDownList && comboBox.DrawMode==DrawMode.Normal && comboBox.Enabled
				&& comboBox.RightToLeft!=RightToLeft.Yes && !SystemInformation.HighContrast;
		}

		///<summary>UI-PLAIN fix round: a DropDownList combo box's face, as the program's ComboBox draws it: the input fill (colorBack, the frame's
		///fill and the text boxes' white) over the part the frame leaves, and the selected item's text in the Text token, left and centred top to
		///bottom where Windows puts it, cut with an ellipsis in front of the chevron. The focus shows as the frame's Accent line, not Windows'
		///highlight.</summary>
		public static void PaintComboFace(Graphics g,Rectangle rectangleInner,System.Windows.Forms.ComboBox comboBox,Color colorBack) {
			if(rectangleInner.Width<=0 || rectangleInner.Height<=0) {
				return;
			}
			using(SolidBrush brush=new SolidBrush(colorBack)) {
				g.FillRectangle(brush,rectangleInner);
			}
			TextRenderer.DrawText(g,comboBox.Text,comboBox.Font,rectangleInner,UiTheme.GetColor(EnumUiColor.Text),colorBack,
				TextFormatFlags.Left|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
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
		///token, so antialiased and ClearType pixels keep their shape. A disabled picker: Windows fills it nearly white, so its box (the lightest
		///value of each channel) is moved to the disabled inputs' grey (SystemColors.Control, the frame's fill) with its grey text kept, the
		///check box included. Not a focused picker (it shows its selected field in Windows' highlight) and not in high contrast (its own
		///colours). The caller passes pixels Windows has just drawn, so none is recoloured twice.</summary>
		public static void RecolorDateText(IntPtr hdc,Control control,Rectangle rectangleUpdate) {
			if(!(control is DateTimePicker dateTimePicker) || control.ContainsFocus || SystemInformation.HighContrast) {
				return;
			}
			bool isEnabled=control.Enabled;
			Rectangle rectangleText=GetInnerRectangle(new Rectangle(0,0,control.Width,control.Height),control);
			if(isEnabled && dateTimePicker.ShowCheckBox && control.IsHandleCreated) {
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
			//A 32 bpp top-down DIB in a memory DC (GDI only: the hdc can be a GDI+ bitmap's, as when the harness prints).
			int width=rectangleText.Width;
			int height=rectangleText.Height;
			IntPtr hdcMemory=CreateCompatibleDC(hdc);
			if(hdcMemory==IntPtr.Zero) {
				return;
			}
			IntPtr hBitmap=IntPtr.Zero;
			IntPtr hBitmapOld=IntPtr.Zero;
			try {
				BITMAPINFOHEADER bitmapInfoHeader=new BITMAPINFOHEADER();
				bitmapInfoHeader.biSize=Marshal.SizeOf(typeof(BITMAPINFOHEADER));
				bitmapInfoHeader.biWidth=width;
				bitmapInfoHeader.biHeight=-height;//top-down
				bitmapInfoHeader.biPlanes=1;
				bitmapInfoHeader.biBitCount=32;
				hBitmap=CreateDIBSection(hdc,ref bitmapInfoHeader,0,out IntPtr pBits,IntPtr.Zero,0);
				if(hBitmap==IntPtr.Zero || pBits==IntPtr.Zero) {
					return;
				}
				hBitmapOld=SelectObject(hdcMemory,hBitmap);
				if(!BitBlt(hdcMemory,0,0,width,height,hdc,rectangleText.X,rectangleText.Y,SRCCOPY)) {
					return;
				}
				GdiFlush();
				int count=width*height*4;
				byte[] arrayBytes=new byte[count];
				Marshal.Copy(pBits,arrayBytes,0,count);
				//Blue, green, red and an unused byte in each pixel.
				if(isEnabled) {
					for(int i=0;i<count;i+=4) {
						arrayBytes[i]=RecolorChannel(arrayBytes[i],colorBack.B,colorText.B);
						arrayBytes[i+1]=RecolorChannel(arrayBytes[i+1],colorBack.G,colorText.G);
						arrayBytes[i+2]=RecolorChannel(arrayBytes[i+2],colorBack.R,colorText.R);
					}
				}
				else {
					byte[] arrayLightest=new byte[3];
					for(int i=0;i<count;i+=4) {
						for(int c=0;c<3;c++) {
							arrayLightest[c]=Math.Max(arrayLightest[c],arrayBytes[i+c]);
						}
					}
					Color colorInk=SystemColors.GrayText;
					Color colorDisabled=SystemColors.Control;
					byte[] arrayInk=new byte[] { colorInk.B,colorInk.G,colorInk.R };
					byte[] arrayDisabled=new byte[] { colorDisabled.B,colorDisabled.G,colorDisabled.R };
					for(int i=0;i<count;i+=4) {
						for(int c=0;c<3;c++) {
							arrayBytes[i+c]=RecolorDisabledChannel(arrayBytes[i+c],arrayInk[c],arrayLightest[c],arrayDisabled[c]);
						}
					}
				}
				Marshal.Copy(arrayBytes,0,pBits,count);
				BitBlt(hdc,rectangleText.X,rectangleText.Y,width,height,hdcMemory,0,0,SRCCOPY);
				GdiFlush();
			}
			finally {
				if(hBitmapOld!=IntPtr.Zero) {
					SelectObject(hdcMemory,hBitmapOld);
				}
				if(hBitmap!=IntPtr.Zero) {
					DeleteObject(hBitmap);
				}
				DeleteDC(hdcMemory);
			}
		}

		///<summary>One channel of a disabled picker's pixel: the ink stays, the box's colour (valueBack) becomes valueTarget, the blends in
		///between in proportion. A channel at or below the ink is left.</summary>
		public static byte RecolorDisabledChannel(byte value,byte valueInk,byte valueBack,byte valueTarget) {
			if(value<=valueInk || valueBack<=valueInk) {
				return value;
			}
			return (byte)Math.Round(valueInk+(value-valueInk)*(double)(valueTarget-valueInk)/(valueBack-valueInk));
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

		///<summary>UI-FOCUS: true when Windows is kept from painting under the frame (WndProc): always, except in high contrast, where Windows
		///paints its own border first and the frame goes over it, as before.</summary>
		private static bool IsWindowsBorderSuppressed() {
			return !UiTheme.IsHighContrast;
		}

		///<summary>UI-FOCUS: a focus change changes only the frame's focus line, so only the frame is painted again, at once, on the window (the
		///whole frame, which covers the old line). It used to redraw the whole window (RedrawWindow with RDW_UPDATENOW): Windows painted its
		///border and the text again and the frame went over them after, one more flash. What Windows changes itself on a focus change (the
		///themed edit's and combo box's focus animation, a date picker's selected field) it paints in its own WM_NCPAINT and WM_PAINT, which
		///leave the frame's area alone (WndProc). High contrast: the whole window is redrawn, as before.</summary>
		private void RepaintFocus() {
			if(!_control.IsHandleCreated) {
				return;
			}
			if(!IsWindowsBorderSuppressed()) {
				Redraw();
				return;
			}
			try {
				PaintOnWindow();
			}
			catch(Exception) {
				//As in WndProc: a frame that cannot be painted leaves what is on the window.
			}
		}

		///<summary>UI-FOCUS: the frame's area (the rectangle the frame paints, less GetInnerRectangle, which it leaves as Windows draws it) as a
		///new GDI region, with the window's top left corner at (x,y). The caller deletes it.</summary>
		private IntPtr CreateFrameRegion(int x,int y) {
			Rectangle rectangle=new Rectangle(0,0,_control.Width,_control.Height);
			Rectangle rectangleInner=GetInnerRectangle(rectangle,_control);
			IntPtr hRgn=CreateRectRgn(x,y,x+rectangle.Width,y+rectangle.Height);
			if(rectangleInner.Width>0 && rectangleInner.Height>0) {
				IntPtr hRgnInner=CreateRectRgn(x+rectangleInner.Left,y+rectangleInner.Top,x+rectangleInner.Right,y+rectangleInner.Bottom);
				CombineRgn(hRgn,hRgn,hRgnInner,RGN_DIFF);
				DeleteObject(hRgnInner);
			}
			return hRgn;
		}

		///<summary>UI-FOCUS: the region (screen coordinates) WM_NCPAINT's default painting gets instead of hRgnUpdate (1: the whole window): the
		///update region less the frame's area and less the frame's scroll bars UiScrollLook paints (it paints them after every WM_NCPAINT). The
		///themed edit paints its border, and Windows the scroll bars, only inside the region passed, so the stock edge and bars never reach the
		///screen under the frame and the look; the scroll bars' corner and the rest outside the frame are painted by Windows as before. Zero when
		///the window has no rectangle. The caller deletes it.</summary>
		private IntPtr CreateNcPaintRegion(IntPtr hRgnUpdate) {
			if(!GetWindowRect(Handle,out RECT rectWindow)) {
				return IntPtr.Zero;
			}
			IntPtr hRgn=CreateRectRgn(rectWindow.Left,rectWindow.Top,rectWindow.Right,rectWindow.Bottom);
			if(hRgn==IntPtr.Zero) {
				return IntPtr.Zero;
			}
			try {
				if(hRgnUpdate!=IntPtr.Zero && hRgnUpdate!=(IntPtr)1) {
					CombineRgn(hRgn,hRgn,hRgnUpdate,RGN_AND);
				}
				IntPtr hRgnFrame=CreateFrameRegion(rectWindow.Left,rectWindow.Top);
				CombineRgn(hRgn,hRgn,hRgnFrame,RGN_DIFF);
				DeleteObject(hRgnFrame);
				List<Rectangle> listRectanglesOut=new List<Rectangle>(UiScrollLook.GetFrameBarRectanglesScreen(_control));
				//Both bars: the square between them too (UiScrollLook fills it), so Windows never paints it first (review of UI-FOCUS, point 3).
				Rectangle rectangleVertical=listRectanglesOut.Find(x => x.Height>x.Width);
				Rectangle rectangleHorizontal=listRectanglesOut.Find(x => x.Width>x.Height);
				if(!rectangleVertical.IsEmpty && !rectangleHorizontal.IsEmpty) {
					listRectanglesOut.Add(Rectangle.FromLTRB(rectangleVertical.Left,rectangleHorizontal.Top,rectangleVertical.Right,rectangleHorizontal.Bottom));
				}
				foreach(Rectangle rectangleOut in listRectanglesOut) {
					IntPtr hRgnOut=CreateRectRgn(rectangleOut.Left,rectangleOut.Top,rectangleOut.Right,rectangleOut.Bottom);
					CombineRgn(hRgn,hRgn,hRgnOut,RGN_DIFF);
					DeleteObject(hRgnOut);
				}
			}
			catch {
				DeleteObject(hRgn);//Not leaked when building the region fails (review of UI-FOCUS, point 2); the caller paints as Windows does.
				return IntPtr.Zero;
			}
			return hRgn;
		}

		///<summary>UI-FOCUS: before Windows' WM_PAINT, the frame's area inside the client area is taken out of the update region, so the control's
		///own painting (clipped by BeginPaint to the update region) cannot draw there: the up-down box, the date picker and the combo box draw
		///their Windows border (the combo box its animated focus line) in the client area. A text box's frame is all in its window frame: nothing
		///changes for it.</summary>
		private void ValidateFrameInClient() {
			Point pointClient=GetClientOffset(Handle);
			IntPtr hRgn;
			if(IsFaceDrawn(_control)) {
				//FB-1: the frame paints a face-drawn combo box's whole face too (PaintComboFace), so Windows' face (its light blue hover fill,
				//animated in and out) is kept off the screen as well; it used to show for a moment under the mouse before the face went over it.
				hRgn=CreateRectRgn(-pointClient.X,-pointClient.Y,-pointClient.X+_control.Width,-pointClient.Y+_control.Height);
			}
			else {
				hRgn=CreateFrameRegion(-pointClient.X,-pointClient.Y);
			}
			if(hRgn==IntPtr.Zero) {
				return;//ValidateRgn with no region would validate the whole client area (review of UI-FOCUS, point 1).
			}
			try {
				ValidateRgn(Handle,hRgn);
			}
			finally {
				DeleteObject(hRgn);
			}
		}

		protected override void WndProc(ref Message m) {
			//UI-FOCUS: Windows' default painting is kept out of the frame's area, so its border is never on screen under the frame between its
			//paint and the frame's (the frame used to be painted over it after each message, so it flashed: 4 or 5 times on each focus change with
			//Windows 11's animated focus line).
			IntPtr wParamNcPaint=m.WParam;
			IntPtr hRgnNcPaint=IntPtr.Zero;
			if((m.Msg==WM_NCPAINT || (m.Msg==WM_PAINT && m.WParam==IntPtr.Zero)) && IsWindowsBorderSuppressed()) {
				try {
					if(m.Msg==WM_NCPAINT) {
						hRgnNcPaint=CreateNcPaintRegion(m.WParam);
						if(hRgnNcPaint!=IntPtr.Zero) {
							m.WParam=hRgnNcPaint;
						}
					}
					else {
						ValidateFrameInClient();
					}
				}
				catch(Exception) {
					//Windows paints as without the suppression; the frame still goes over it.
				}
			}
			//UI-13 fix round: the part of a date picker Windows is about to paint on screen (its update region, in window coordinates), so only the
			//pixels it paints now get the Text token's colour (RecolorDateText).
			Rectangle[] arrayRectanglesUpdate=null;
			if(m.Msg==WM_PAINT && m.WParam==IntPtr.Zero && _control is DateTimePicker) {
				arrayRectanglesUpdate=GetUpdateRectangles();
			}
			try {
				base.WndProc(ref m);
			}
			finally {
				if(hRgnNcPaint!=IntPtr.Zero) {
					m.WParam=wParamNcPaint;
					DeleteObject(hRgnNcPaint);
				}
			}
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

		///<summary>FB-1 (text boxes flickered on hover): the frame is drawn into an off-screen picture first and put on the
		///window in one copy, clipped to what it paints (the frame's area, and the face of a face-drawn combo box). It used to be drawn straight on
		///the window DC, the parent's colour over the whole edge first and the box after, so every repaint showed the bare fill for a moment.
		///Windows 11 repaints a text box's and combo box's border 4 or 5 times when the mouse comes in and again when it goes out (the hover
		///animation), and each of those repaints flashed. With one copy the screen goes straight from the old frame to the new one.</summary>
		private void PaintOnWindow() {
			if(Handle==IntPtr.Zero) {
				//GetWindowDC(0) is the whole screen's.
				return;
			}
			int width=_control.Width;
			int height=_control.Height;
			if(width<=0 || height<=0) {
				return;
			}
			IntPtr hdc=GetWindowDC(Handle);
			if(hdc==IntPtr.Zero) {
				return;
			}
			try {
				Rectangle rectangle=new Rectangle(0,0,width,height);
				//No alpha channel: GDI text (the combo box face's) on a 32bppArgb picture would come out transparent. Every pixel copied is painted.
				using(Bitmap bitmap=new Bitmap(width,height,System.Drawing.Imaging.PixelFormat.Format32bppRgb))
				using(Region regionCopy=new Region(rectangle)) {
					using(Graphics gBitmap=Graphics.FromImage(bitmap)) {
						PaintFrame(gBitmap,rectangle,_control,IsFocusShown());
					}
					if(!IsFaceDrawn(_control)) {
						regionCopy.Exclude(GetInnerRectangle(rectangle,_control));//the text and the content stay as Windows drew them
					}
					using(Graphics g=Graphics.FromHdc(hdc)) {
						g.Clip=regionCopy;
						g.DrawImageUnscaled(bitmap,0,0);
					}
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
		private static extern int CombineRgn(IntPtr hRgnDest,IntPtr hRgnSource1,IntPtr hRgnSource2,int mode);

		[DllImport("user32.dll")]
		private static extern bool ValidateRgn(IntPtr hWnd,IntPtr hRgn);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);

		[StructLayout(LayoutKind.Sequential)]
		private struct BITMAPINFOHEADER {
			public int biSize;
			public int biWidth;
			public int biHeight;
			public short biPlanes;
			public short biBitCount;
			public int biCompression;
			public int biSizeImage;
			public int biXPelsPerMeter;
			public int biYPelsPerMeter;
			public int biClrUsed;
			public int biClrImportant;
		}

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateDIBSection(IntPtr hdc,ref BITMAPINFOHEADER bitmapInfoHeader,int usage,out IntPtr pBits,IntPtr hSection,int offset);

		[DllImport("gdi32.dll")]
		private static extern IntPtr SelectObject(IntPtr hdc,IntPtr hObject);

		[DllImport("gdi32.dll")]
		private static extern bool GdiFlush();

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
