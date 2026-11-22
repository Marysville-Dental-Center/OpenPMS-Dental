// UI-12 pilot (docs/port-notes/UI-12.md, the owner's G-UI3 row "drop down sytle inconsistent" on FormPayment): every single-line input draws
// the approved combo box's frame (section 3.6, UI-4b-1): the fill, a 1 px StrokeStrong border with 4 px corners, and with focus a 2 px Accent
// line inside the bottom edge. The program's own ComboBox paints itself; the stock single-line text box (and the program's Valid* boxes),
// the up-down boxes and the date picker keep their Windows behaviour and get the frame painted over their Windows border, so the thick
// black or 3-D edges and the date picker's drop button look like the combo box's. FormODBase attaches it to its controls when it makes its
// LayoutManager (AttachTree). Round 2: the stock (editable) ComboBox too, its Windows drop button covered by the program's chevron.
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
		///<summary>True for the inputs that get the frame: a single-line text box with a border (the program's Valid* boxes are text boxes), an
		///up-down box, a date picker. Not the text inside an up-down box (its box has the frame), not a multi-line or borderless box.</summary>
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
			if(control is TextBox textBox) {
				return (!textBox.Multiline || IsOneLineTall(textBox)) && textBox.BorderStyle==BorderStyle.Fixed3D && !(textBox.Parent is UpDownBase);
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
				if(IsFramed(control)) {
					Attach(control);
				}
				else if(control is RadioButton) {
					//Round 3: the stock radio button drawn like the program's check box (UiRadioLook).
					UiRadioLook.Attach(control);
				}
				AttachTree(control);
			}
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
				Rectangle rectangleInner=Rectangle.Inflate(rectangle,-edge,-edge);
				Rectangle rectangleButton=Rectangle.Empty;
				if(control is DateTimePicker || control is System.Windows.Forms.ComboBox) {
					//The drop button: the width of a scroll bar inside the edge, at the right.
					int widthButton=SystemInformation.VerticalScrollBarWidth+2;
					rectangleButton=new Rectangle(rectangleInner.Right-widthButton,rectangleInner.Y,widthButton,rectangleInner.Height);
					rectangleInner.Width-=widthButton;
				}
				region.Exclude(rectangleInner);
				g.Clip=region;
				using(SolidBrush brushParent=new SolidBrush(colorParent)) {
					g.FillRectangle(brushParent,rectangle);
				}
				Rectangle rectangleBox=new Rectangle(rectangle.X,rectangle.Y,rectangle.Width,rectangle.Height);
				UiListPaint.DrawComboBoxBox(g,rectangleBox,false,control.Enabled,scale,colorBack);
				if(control is DateTimePicker || control is System.Windows.Forms.ComboBox) {
					UiListPaint.DrawChevron(g,rectangleBox,control.Enabled ? EnumUiColor.Text : EnumUiColor.TextDisabled,scale);
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
		#endregion Methods - Public

		#region Methods - Private
		///<summary>The width of the Windows border the frame covers: 2 px for a 3-D edge, 1 px for a single line, at least the frame's line.</summary>
		private static int GetEdgeWidth(Control control) {
			if(control is TextBox textBox && textBox.BorderStyle==BorderStyle.FixedSingle) {
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
			base.WndProc(ref m);
			try {
				switch(m.Msg) {
					case WM_NCPAINT:
					case WM_PAINT:
						PaintOnWindow();
						break;
					case WM_PRINT:
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

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindowDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern int ReleaseDC(IntPtr hWnd,IntPtr hDC);

		[DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr hWnd,IntPtr lprcUpdate,IntPtr hrgnUpdate,int flags);
		#endregion Methods - Private
	}
}
