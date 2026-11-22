// UI-13 (docs/port-notes/UI-13.md; the owner, 2026-11-22: "Overall make sure you modernize all element types"): the shared looks of the stock
// Windows Forms element types that UI-4 to UI-12 left in their Windows-classic picture, attached to every control of a window by
// UiInputFrame.AttachOne (FormODBase and LayoutManagerForms call it), never per window. Each keeps the control's behaviour, values, events and
// texts; only its picture changes, in the UiTheme tokens:
// - every scroll bar (UiScrollLook) and the track bar (UiTrackBarLook), in their own files;
// - the stock button (System.Windows.Forms.Button, not the program's UI.Button): the program's button look (UiControlPaint.DrawButton: flat
//   SurfaceRaised, 1 px Stroke, 4 px corners, ButtonHover and ButtonPressed), or, when it carries its own colour (a colour swatch, the time
//   pattern's slider handle), that colour as a rounded swatch with a 1 px StrokeStrong edge (UiButtonLook);
// - the stock check box: the program's check box (UiCheckLook, as UiRadioLook draws the stock radio button);
// - a label used as a grey block (BackColor LightGray, ControlAccount's "Ins Rem" and "DPlan Rem"): the program's button look (UiBlockLook);
// - the stock progress bar: a rounded Stroke track with the Accent fill (UiProgressLook);
// - a 3-D or black single-line border of a panel, picture box or user control: one flat 1 px StrokeStrong line (UiBorderLook);
// - the tree view and list view: Windows 11's Explorer theme (SetWindowTheme), the hover and selection Windows 11's own file lists use;
// - the stock data grid view: the grid's tokens (header SurfaceSubtle, lines Gridline, selection AccentSubtle) where it has Windows' defaults;
// - the link label: Accent links where it has Windows' default blue.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI {
	///<summary>Attaches UI-13's shared looks of the stock element types to one control.</summary>
	public static class UiLooks {
		private static readonly LayoutManagerForms _layoutManager=new LayoutManagerForms();
		///<summary>True leaves the stock types as Windows draws them (no UI-13 look is attached): only the gallery harness sets it, for the before
		///pictures of windows outside the UI-12 pilot (G-UI3-ui13). The program never sets it.</summary>
		public static bool IsOffForGallery;

		///<summary>Every UI-13 look that applies to the one control (not its children).</summary>
		public static void AttachOne(Control control) {
			if(control==null || IsOffForGallery) {
				return;
			}
			if(UiScrollLook.IsScrolled(control)) {
				UiScrollLook.Attach(control);
			}
			if(control is TrackBar) {
				UiTrackBarLook.Attach(control);
			}
			if(UiButtonLook.IsDrawn(control)) {
				UiButtonLook.Attach(control);
			}
			if(UiCheckLook.IsDrawn(control)) {
				UiCheckLook.Attach(control);
			}
			if(UiBlockLook.IsDrawn(control)) {
				UiBlockLook.Attach(control);
			}
			if(control is ProgressBar) {
				UiProgressLook.Attach(control);
			}
			if(UiBorderLook.IsDrawn(control)) {
				UiBorderLook.Attach(control);
			}
			if(control is TreeView || control is ListView) {
				AttachExplorerTheme(control);
			}
			if(control is DataGridView dataGridView) {
				SetDataGridViewLook(dataGridView);
			}
			if(control is LinkLabel linkLabel) {
				SetLinkLabelLook(linkLabel);
			}
			if(control is MonthCalendar monthCalendar) {
				SetMonthCalendarLook(monthCalendar);
			}
			if(control is DateTimePicker dateTimePicker) {
				SetDateTimePickerLook(dateTimePicker);
			}
		}

		///<summary>A stock month calendar's colours in the tokens where it has Windows' defaults: the title Accent with TextOnAccent, the days Text
		///on SurfaceRaised, the other months' days TextDisabled. Windows uses them where it draws the calendar without a theme; with Windows 11's
		///theme it draws its own themed calendar.</summary>
		public static void SetMonthCalendarLook(MonthCalendar monthCalendar) {
			if(monthCalendar.TitleBackColor==SystemColors.ActiveCaption) {
				monthCalendar.TitleBackColor=UiTheme.GetColor(EnumUiColor.Accent);
			}
			if(monthCalendar.TitleForeColor==SystemColors.ActiveCaptionText) {
				monthCalendar.TitleForeColor=UiTheme.GetColor(EnumUiColor.TextOnAccent);
			}
			if(monthCalendar.TrailingForeColor==SystemColors.GrayText) {
				monthCalendar.TrailingForeColor=UiTheme.GetColor(EnumUiColor.TextDisabled);
			}
			if(monthCalendar.BackColor==SystemColors.Window) {
				monthCalendar.BackColor=UiTheme.GetColor(EnumUiColor.SurfaceRaised);
			}
			if(monthCalendar.ForeColor==SystemColors.WindowText) {
				monthCalendar.ForeColor=UiTheme.GetColor(EnumUiColor.Text);
			}
		}

		///<summary>A stock date picker's drop-down calendar in the same token colours as SetMonthCalendarLook.</summary>
		public static void SetDateTimePickerLook(DateTimePicker dateTimePicker) {
			if(dateTimePicker.CalendarTitleBackColor==SystemColors.ActiveCaption) {
				dateTimePicker.CalendarTitleBackColor=UiTheme.GetColor(EnumUiColor.Accent);
			}
			if(dateTimePicker.CalendarTitleForeColor==SystemColors.ActiveCaptionText) {
				dateTimePicker.CalendarTitleForeColor=UiTheme.GetColor(EnumUiColor.TextOnAccent);
			}
			if(dateTimePicker.CalendarTrailingForeColor==SystemColors.GrayText) {
				dateTimePicker.CalendarTrailingForeColor=UiTheme.GetColor(EnumUiColor.TextDisabled);
			}
			if(dateTimePicker.CalendarMonthBackground==SystemColors.Window) {
				dateTimePicker.CalendarMonthBackground=UiTheme.GetColor(EnumUiColor.SurfaceRaised);
			}
			if(dateTimePicker.CalendarForeColor==SystemColors.WindowText) {
				dateTimePicker.CalendarForeColor=UiTheme.GetColor(EnumUiColor.Text);
			}
		}

		///<summary>The scale of the looks: the program's dpi and zoom.</summary>
		public static float GetScale() {
			return Math.Max(1f,_layoutManager.ScaleMy());
		}

		///<summary>The colour behind a control: its own when opaque, else its parent's, else the window colour.</summary>
		public static Color GetBackColor(Control control) {
			Color color=control.BackColor;
			if(color.A<255) {
				color=control.Parent?.BackColor ?? SystemColors.Control;
			}
			if(color.A<255) {
				color=SystemColors.Control;
			}
			return color;
		}

		#region Tree view, list view, data grid view, link label
		///<summary>Windows 11's Explorer theme on a tree view or list view (its hover and selection, its chevrons), now and on every new handle.
		///</summary>
		public static void AttachExplorerTheme(Control control) {
			if(control.IsHandleCreated) {
				SetExplorerTheme(control);
			}
			control.HandleCreated+=(sender,e) => SetExplorerTheme((Control)sender);
		}

		private static void SetExplorerTheme(Control control) {
			try {
				SetWindowTheme(control.Handle,"Explorer",null);
			}
			catch(Exception) {
				//Without the theme the control keeps Windows' default picture.
			}
		}

		///<summary>A stock data grid view in the grid's tokens where it has Windows' defaults: its lines Gridline, the header SurfaceSubtle with Text,
		///the selection AccentSubtle with TextSelected, the background Surface. A colour the window sets itself stays.</summary>
		public static void SetDataGridViewLook(DataGridView dataGridView) {
			if(dataGridView.GridColor==SystemColors.ControlDark) {
				dataGridView.GridColor=UiTheme.GetColor(EnumUiColor.Gridline);
			}
			if(dataGridView.BackgroundColor==SystemColors.AppWorkspace) {
				dataGridView.BackgroundColor=UiTheme.GetColor(EnumUiColor.Surface);
			}
			dataGridView.EnableHeadersVisualStyles=false;
			foreach(DataGridViewCellStyle dataGridViewCellStyle in new DataGridViewCellStyle[] {
				dataGridView.ColumnHeadersDefaultCellStyle,dataGridView.RowHeadersDefaultCellStyle })
			{
				if(dataGridViewCellStyle.BackColor==SystemColors.Control) {
					dataGridViewCellStyle.BackColor=UiTheme.GetColor(EnumUiColor.SurfaceSubtle);
				}
				if(dataGridViewCellStyle.SelectionBackColor==SystemColors.Highlight) {
					dataGridViewCellStyle.SelectionBackColor=UiTheme.GetColor(EnumUiColor.SurfaceSubtle);
					dataGridViewCellStyle.SelectionForeColor=UiTheme.GetColor(EnumUiColor.Text);
				}
			}
			if(dataGridView.DefaultCellStyle.SelectionBackColor==SystemColors.Highlight) {
				dataGridView.DefaultCellStyle.SelectionBackColor=UiTheme.GetColor(EnumUiColor.AccentSubtle);
				dataGridView.DefaultCellStyle.SelectionForeColor=UiTheme.GetColor(EnumUiColor.TextSelected);
			}
		}

		///<summary>A link label's links in Accent (AccentPressed while clicked) where it has Windows' default blue and red.</summary>
		public static void SetLinkLabelLook(LinkLabel linkLabel) {
			if(linkLabel.LinkColor.ToArgb()==Color.Blue.ToArgb()) {
				linkLabel.LinkColor=UiTheme.GetColor(EnumUiColor.Accent);
			}
			if(linkLabel.ActiveLinkColor.ToArgb()==Color.Red.ToArgb()) {
				linkLabel.ActiveLinkColor=UiTheme.GetColor(EnumUiColor.AccentPressed);
			}
		}
		#endregion Tree view, list view, data grid view, link label

		[DllImport("uxtheme.dll",CharSet=CharSet.Unicode)]
		private static extern int SetWindowTheme(IntPtr hWnd,string pszSubAppName,string pszSubIdList);
	}

	///<summary>The stock button (System.Windows.Forms.Button, not UI.Button) in the program's button look, or as a rounded colour swatch when it
	///carries its own colour. Painted in the button's Paint event, after Windows Forms has drawn it.</summary>
	public class UiButtonLook {
		private static readonly ConditionalWeakTable<Control,UiButtonLook> _table=new ConditionalWeakTable<Control,UiButtonLook>();
		private static readonly PropertyInfo _propertyInfoShowFocusCues=typeof(Control).GetProperty("ShowFocusCues",BindingFlags.Instance|BindingFlags.NonPublic);
		private readonly System.Windows.Forms.Button _button;
		private bool _isHover;
		private bool _isPressed;

		private UiButtonLook(System.Windows.Forms.Button button) {
			_button=button;
			button.Paint+=(sender,e) => {
				try {
					PaintButton(e.Graphics,_button,GetState(),IsFocusShown(_button));
				}
				catch(Exception) {
					//The look is only a picture: Windows Forms' own drawing stays.
				}
			};
			button.MouseEnter+=(sender,e) => { _isHover=true; _button.Invalidate(); };
			button.MouseLeave+=(sender,e) => { _isHover=false; _isPressed=false; _button.Invalidate(); };
			button.MouseDown+=(sender,e) => { if(e.Button==MouseButtons.Left) { _isPressed=true; _button.Invalidate(); } };
			button.MouseUp+=(sender,e) => { _isPressed=false; _button.Invalidate(); };
		}

		///<summary>True for a stock button drawn by Windows Forms (not FlatStyle.System, which Windows draws) and not the program's own UI.Button.
		///</summary>
		public static bool IsDrawn(Control control) {
			return control!=null && control.GetType()==typeof(System.Windows.Forms.Button)
				&& ((System.Windows.Forms.Button)control).FlatStyle!=FlatStyle.System;
		}

		public static void Attach(Control control) {
			if(!IsDrawn(control) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiButtonLook((System.Windows.Forms.Button)control));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>True when the button shows its own colour (a colour swatch, a handle): not the visual-style back colour and an opaque colour other
		///than the control colours.</summary>
		public static bool IsSwatch(System.Windows.Forms.Button button) {
			if(button.UseVisualStyleBackColor) {
				return false;
			}
			Color color=button.BackColor;
			if(color.A<255) {
				return false;
			}
			int argb=color.ToArgb();
			return argb!=SystemColors.Control.ToArgb() && argb!=SystemColors.ButtonFace.ToArgb();
		}

		private EnumUiButtonState GetState() {
			if(!_button.Enabled) {
				return EnumUiButtonState.Disabled;
			}
			if(_isPressed) {
				return EnumUiButtonState.Pressed;
			}
			return _isHover ? EnumUiButtonState.Hover : EnumUiButtonState.Normal;
		}

		private static bool IsFocusShown(Control control) {
			if(!control.Focused) {
				return false;
			}
			try {
				return (bool)_propertyInfoShowFocusCues.GetValue(control);
			}
			catch(Exception) {
				return true;
			}
		}

		///<summary>The whole button in g (origin at its top left): the parent's colour behind the rounded corners, the chrome (the button look or
		///the swatch), its image and its text.</summary>
		public static void PaintButton(Graphics g,System.Windows.Forms.Button button,EnumUiButtonState buttonState,bool isFocused) {
			float scale=UiLooks.GetScale();
			Rectangle rectangle=new Rectangle(0,0,button.Width,button.Height);
			Color colorParent=button.Parent==null ? SystemColors.Control : UiLooks.GetBackColor(button.Parent);
			using(SolidBrush brush=new SolidBrush(colorParent)) {
				g.FillRectangle(brush,rectangle);
			}
			Color colorText;
			if(IsSwatch(button)) {
				UiControlPaint.Fill(g,button.BackColor,rectangle,UiTheme.CORNER_CONTROL,scale);
				EnumUiColor uiColorEdge=buttonState==EnumUiButtonState.Disabled ? EnumUiColor.Stroke
					: (buttonState==EnumUiButtonState.Normal ? EnumUiColor.StrokeStrong : EnumUiColor.StrokeStrongHover);
				UiControlPaint.Outline(g,uiColorEdge,rectangle,UiTheme.GetLineWidth(scale),UiTheme.CORNER_CONTROL,scale);
				if(isFocused) {
					UiControlPaint.FocusRing(g,rectangle,UiTheme.CORNER_CONTROL,scale);
				}
				colorText=button.ForeColor;
			}
			else {
				EnumUiColor uiColorText=UiControlPaint.DrawButton(g,rectangle,buttonState,isPrimary:false,isFocused,scale);
				int argbFore=button.ForeColor.ToArgb();
				bool isDefaultFore=argbFore==SystemColors.ControlText.ToArgb() || argbFore==Color.Black.ToArgb();
				colorText=(isDefaultFore || buttonState==EnumUiButtonState.Disabled) ? UiTheme.GetColor(uiColorText) : button.ForeColor;
			}
			Rectangle rectangleContent=Rectangle.Inflate(rectangle,-UiTheme.ScaleI(UiTheme.SPACE_4,scale),-UiTheme.ScaleI(UiTheme.SPACE_2,scale));
			if(button.Image!=null) {
				Rectangle rectangleImage=GetAligned(rectangleContent,button.Image.Size,button.ImageAlign);
				if(button.Enabled) {
					g.DrawImage(button.Image,rectangleImage);
				}
				else {
					ControlPaint.DrawImageDisabled(g,button.Image,rectangleImage.X,rectangleImage.Y,colorParent);
				}
				if(!string.IsNullOrEmpty(button.Text) && button.ImageAlign==ContentAlignment.MiddleLeft) {
					int widthImage=rectangleImage.Width+UiTheme.ScaleI(UiTheme.GAP_ICON_TEXT,scale);
					rectangleContent=new Rectangle(rectangleContent.X+widthImage,rectangleContent.Y,Math.Max(0,rectangleContent.Width-widthImage),rectangleContent.Height);
				}
			}
			if(string.IsNullOrEmpty(button.Text)) {
				return;
			}
			TextFormatFlags textFormatFlags=GetTextFlags(button.TextAlign)|TextFormatFlags.WordBreak;
			if(!button.UseMnemonic) {
				textFormatFlags|=TextFormatFlags.NoPrefix;
			}
			TextRenderer.DrawText(g,button.Text,button.Font,rectangleContent,colorText,textFormatFlags);
		}

		private static Rectangle GetAligned(Rectangle rectangle,Size size,ContentAlignment contentAlignment) {
			int x=rectangle.X+(rectangle.Width-size.Width)/2;
			int y=rectangle.Y+(rectangle.Height-size.Height)/2;
			switch(contentAlignment) {
				case ContentAlignment.TopLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.BottomLeft:
					x=rectangle.X;
					break;
				case ContentAlignment.TopRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.BottomRight:
					x=rectangle.Right-size.Width;
					break;
			}
			switch(contentAlignment) {
				case ContentAlignment.TopLeft:
				case ContentAlignment.TopCenter:
				case ContentAlignment.TopRight:
					y=rectangle.Y;
					break;
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight:
					y=rectangle.Bottom-size.Height;
					break;
			}
			return new Rectangle(x,y,size.Width,size.Height);
		}

		///<summary>The text flags of a content alignment.</summary>
		public static TextFormatFlags GetTextFlags(ContentAlignment contentAlignment) {
			TextFormatFlags textFormatFlags=TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter;
			switch(contentAlignment) {
				case ContentAlignment.TopLeft:
					return TextFormatFlags.Left|TextFormatFlags.Top;
				case ContentAlignment.TopCenter:
					return TextFormatFlags.HorizontalCenter|TextFormatFlags.Top;
				case ContentAlignment.TopRight:
					return TextFormatFlags.Right|TextFormatFlags.Top;
				case ContentAlignment.MiddleLeft:
					return TextFormatFlags.Left|TextFormatFlags.VerticalCenter;
				case ContentAlignment.MiddleRight:
					return TextFormatFlags.Right|TextFormatFlags.VerticalCenter;
				case ContentAlignment.BottomLeft:
					return TextFormatFlags.Left|TextFormatFlags.Bottom;
				case ContentAlignment.BottomCenter:
					return TextFormatFlags.HorizontalCenter|TextFormatFlags.Bottom;
				case ContentAlignment.BottomRight:
					return TextFormatFlags.Right|TextFormatFlags.Bottom;
			}
			return textFormatFlags;
		}
	}

	///<summary>A label used as a grey block (BackColor LightGray: ControlAccount's "Ins Rem" and "DPlan Rem", which open and show things like a
	///button) drawn in the program's button look, its text where the label puts it. Painted in the label's Paint event.</summary>
	public class UiBlockLook {
		private static readonly ConditionalWeakTable<Control,UiBlockLook> _table=new ConditionalWeakTable<Control,UiBlockLook>();
		private readonly Label _label;
		private bool _isHover;

		private UiBlockLook(Label label) {
			_label=label;
			label.Paint+=(sender,e) => {
				try {
					PaintBlock(e.Graphics,_label,_label.Enabled ? (_isHover ? EnumUiButtonState.Hover : EnumUiButtonState.Normal) : EnumUiButtonState.Disabled);
				}
				catch(Exception) {
					//The look is only a picture.
				}
			};
			label.MouseEnter+=(sender,e) => { _isHover=true; _label.Invalidate(); };
			label.MouseLeave+=(sender,e) => { _isHover=false; _label.Invalidate(); };
		}

		///<summary>True for a label (not a link label) whose colour is LightGray, the classic grey block.</summary>
		public static bool IsDrawn(Control control) {
			return control!=null && control.GetType()==typeof(Label) && control.BackColor.ToArgb()==Color.LightGray.ToArgb();
		}

		public static void Attach(Control control) {
			if(!IsDrawn(control) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiBlockLook((Label)control));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		public static void PaintBlock(Graphics g,Label label,EnumUiButtonState buttonState) {
			float scale=UiLooks.GetScale();
			Rectangle rectangle=new Rectangle(0,0,label.Width,label.Height);
			using(SolidBrush brush=new SolidBrush(label.Parent==null ? SystemColors.Control : UiLooks.GetBackColor(label.Parent))) {
				g.FillRectangle(brush,rectangle);
			}
			EnumUiColor uiColorText=UiControlPaint.DrawButton(g,rectangle,buttonState,isPrimary:false,isFocused:false,scale);
			TextFormatFlags textFormatFlags=UiButtonLook.GetTextFlags(label.TextAlign)|TextFormatFlags.WordBreak;
			if(!label.UseMnemonic) {
				textFormatFlags|=TextFormatFlags.NoPrefix;
			}
			TextRenderer.DrawText(g,label.Text,label.Font,rectangle,UiTheme.GetColor(uiColorText),textFormatFlags);
		}
	}

	///<summary>The stock check box (System.Windows.Forms.CheckBox, not UI.CheckBox) drawn like the program's check box: the box on the left in
	///UiControlPaint.DrawCheckBoxBox, the text 3 px after it (as UiRadioLook draws the stock radio button). A check box drawn as a button, right
	///to left or with its box elsewhere than on the left keeps the Windows picture.</summary>
	public class UiCheckLook : NativeWindow {
		private const int WM_PAINT=0x000F;
		private const int WM_PRINT=0x0317;
		private const int WM_PRINTCLIENT=0x0318;
		private const int SIZE_BOX_96=12;
		private static readonly ConditionalWeakTable<Control,UiCheckLook> _table=new ConditionalWeakTable<Control,UiCheckLook>();
		private readonly System.Windows.Forms.CheckBox _checkBox;
		private bool _isHovering;

		private UiCheckLook(System.Windows.Forms.CheckBox checkBox) {
			_checkBox=checkBox;
			if(checkBox.IsHandleCreated) {
				AssignHandle(checkBox.Handle);
			}
			checkBox.HandleCreated+=(sender,e) => AssignHandle(_checkBox.Handle);
			checkBox.HandleDestroyed+=(sender,e) => ReleaseHandle();
			checkBox.MouseEnter+=(sender,e) => { _isHovering=true; _checkBox.Invalidate(); };
			checkBox.MouseLeave+=(sender,e) => { _isHovering=false; _checkBox.Invalidate(); };
		}

		public static bool IsDrawn(Control control) {
			if(control==null || control.GetType()!=typeof(System.Windows.Forms.CheckBox)) {
				return false;
			}
			System.Windows.Forms.CheckBox checkBox=(System.Windows.Forms.CheckBox)control;
			return checkBox.Appearance==Appearance.Normal
				&& (checkBox.CheckAlign==ContentAlignment.MiddleLeft || checkBox.CheckAlign==ContentAlignment.TopLeft)
				&& checkBox.RightToLeft!=RightToLeft.Yes;
		}

		public static void Attach(Control control) {
			if(!IsDrawn(control) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiCheckLook((System.Windows.Forms.CheckBox)control));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		public static void PaintCheck(Graphics g,Rectangle rectangle,System.Windows.Forms.CheckBox checkBox,float scale,bool isHovering) {
			using(SolidBrush brushBack=new SolidBrush(UiLooks.GetBackColor(checkBox))) {
				g.FillRectangle(brushBack,rectangle);
			}
			int sizeBox=(int)Math.Round(SIZE_BOX_96*scale);
			int gap=(int)Math.Round(3*scale);
			int top=rectangle.Y;
			TextFormatFlags textFormatFlags=TextFormatFlags.Left|TextFormatFlags.WordBreak;
			if(checkBox.CheckAlign==ContentAlignment.TopLeft) {
				textFormatFlags|=TextFormatFlags.Top;
			}
			else {
				top=rectangle.Y+(int)Math.Round(rectangle.Height/2f-sizeBox/2f,0);
				textFormatFlags|=TextFormatFlags.VerticalCenter;
			}
			if(!checkBox.UseMnemonic) {
				textFormatFlags|=TextFormatFlags.NoPrefix;
			}
			int sizeDrawn=sizeBox+UiTheme.GetLineWidth(scale);
			Rectangle rectangleBox=new Rectangle(rectangle.X,top,sizeDrawn,sizeDrawn);
			UiControlPaint.DrawCheckBoxBox(g,rectangleBox,checkBox.CheckState,isHovering,checkBox.Enabled,scale,Color.Empty);
			Rectangle rectangleText=new Rectangle(rectangle.X+sizeBox+gap,rectangle.Y,Math.Max(0,rectangle.Width-sizeBox-gap),rectangle.Height);
			TextRenderer.DrawText(g,checkBox.Text,checkBox.Font,rectangleText,UiTheme.GetColor(checkBox.Enabled ? EnumUiColor.Text : EnumUiColor.TextDisabled),
				textFormatFlags);
			if(checkBox.Focused) {
				Size sizeText=TextRenderer.MeasureText(g,checkBox.Text??"",checkBox.Font,rectangleText.Size,textFormatFlags);
				Rectangle rectangleTextUsed=new Rectangle(rectangleText.X,rectangleBox.Y,Math.Min(rectangleText.Width,sizeText.Width),rectangleBox.Height);
				UiControlPaint.FocusRing(g,UiControlPaint.GetCheckFocusRectangle(rectangleBox,rectangleTextUsed,rectangle,scale),UiTheme.CORNER_CONTROL,scale);
			}
		}

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
			try {
				if(!IsDrawn(_checkBox)) {
					return;
				}
				switch(m.Msg) {
					case WM_PAINT:
						using(Graphics g=_checkBox.CreateGraphics()) {
							PaintCheck(g,_checkBox.ClientRectangle,_checkBox,UiLooks.GetScale(),_isHovering);
						}
						break;
					case WM_PRINT:
					case WM_PRINTCLIENT:
						using(Graphics g=Graphics.FromHdc(m.WParam)) {
							PaintCheck(g,_checkBox.ClientRectangle,_checkBox,UiLooks.GetScale(),false);
						}
						break;
				}
			}
			catch(Exception) {
				//The look is only a picture.
			}
		}
	}

	///<summary>The stock progress bar drawn as a rounded Stroke track with its value in Accent (the program's OdProgressBar's look), over
	///Windows' picture after each paint and each change of value.</summary>
	public class UiProgressLook : NativeWindow {
		private const int WM_PAINT=0x000F;
		private const int WM_TIMER=0x0113;
		private const int WM_PRINT=0x0317;
		private const int WM_PRINTCLIENT=0x0318;
		private const int WM_NCDESTROY=0x0082;
		private static readonly ConditionalWeakTable<Control,UiProgressLook> _table=new ConditionalWeakTable<Control,UiProgressLook>();
		private readonly ProgressBar _progressBar;
		///<summary>True while painting: the messages the paint sends itself (the value's PBM_GETPOS) pass straight to Windows.</summary>
		private bool _isBusy;

		private UiProgressLook(ProgressBar progressBar) {
			_progressBar=progressBar;
			if(progressBar.IsHandleCreated) {
				AssignHandle(progressBar.Handle);
			}
			progressBar.HandleCreated+=(sender,e) => AssignHandle(_progressBar.Handle);
			progressBar.HandleDestroyed+=(sender,e) => ReleaseHandle();
		}

		public static void Attach(Control control) {
			if(!(control is ProgressBar progressBar) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiProgressLook(progressBar));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>The bar in g (origin at its top left): the parent's colour, a rounded Stroke track the bar's height, the value's part in Accent
		///(TextDisabled when disabled); a marquee bar shows a third of the track filled from the left.</summary>
		public static void PaintProgress(Graphics g,ProgressBar progressBar) {
			float scale=UiLooks.GetScale();
			Rectangle rectangle=new Rectangle(0,0,progressBar.Width,progressBar.Height);
			using(SolidBrush brush=new SolidBrush(progressBar.Parent==null ? SystemColors.Control : UiLooks.GetBackColor(progressBar.Parent))) {
				g.FillRectangle(brush,rectangle);
			}
			int radius=Math.Min(UiTheme.GetCornerRadius(UiTheme.CORNER_CONTROL,scale),rectangle.Height/2);
			UiControlPaint.Fill(g,EnumUiColor.Stroke,rectangle,radius,1f);
			double fraction=0;
			if(progressBar.Style==ProgressBarStyle.Marquee) {
				fraction=1/3d;
			}
			else if(progressBar.Maximum>progressBar.Minimum) {
				fraction=(progressBar.Value-progressBar.Minimum)/(double)(progressBar.Maximum-progressBar.Minimum);
			}
			int width=(int)Math.Round(rectangle.Width*Math.Max(0,Math.Min(1,fraction)));
			if(width>0) {
				UiControlPaint.Fill(g,progressBar.Enabled ? EnumUiColor.Accent : EnumUiColor.TextDisabled,new Rectangle(0,0,width,rectangle.Height),radius,1f);
			}
		}

		protected override void WndProc(ref Message m) {
			if(_isBusy) {
				base.WndProc(ref m);
				return;
			}
			int msg=m.Msg;
			base.WndProc(ref m);
			_isBusy=true;
			try {
				switch(msg) {
					case WM_NCDESTROY:
						return;
					case WM_PRINT:
					case WM_PRINTCLIENT:
						using(Graphics g=Graphics.FromHdc(m.WParam)) {
							PaintProgress(g,_progressBar);
						}
						return;
					case WM_PAINT:
					case WM_TIMER:
					case 0x0402://PBM_SETPOS
					case 0x0403://PBM_DELTAPOS
					case 0x0405://PBM_STEPIT
					case 0x0406://PBM_SETRANGE32
						using(Graphics g=_progressBar.CreateGraphics()) {
							PaintProgress(g,_progressBar);
						}
						return;
				}
			}
			catch(Exception) {
				//The look is only a picture.
			}
			finally {
				_isBusy=false;
			}
		}
	}

	///<summary>The border Windows draws around a panel, picture box or user control (BorderStyle Fixed3D, a sunken 3-D edge, or FixedSingle, a
	///black line) painted over as one flat 1 px StrokeStrong line with the control's colour inside it.</summary>
	public class UiBorderLook : NativeWindow {
		private const int WM_NCPAINT=0x0085;
		private const int WM_PRINT=0x0317;
		private const int WM_NCDESTROY=0x0082;
		private const int PRF_NONCLIENT=0x0002;
		private static readonly ConditionalWeakTable<Control,UiBorderLook> _table=new ConditionalWeakTable<Control,UiBorderLook>();
		private readonly Control _control;

		private UiBorderLook(Control control) {
			_control=control;
			if(control.IsHandleCreated) {
				AssignHandle(control.Handle);
			}
			control.HandleCreated+=(sender,e) => AssignHandle(_control.Handle);
			control.HandleDestroyed+=(sender,e) => ReleaseHandle();
		}

		///<summary>The border style of a control that has one (a stock Panel, PictureBox or UserControl), None otherwise.</summary>
		public static BorderStyle GetBorderStyle(Control control) {
			if(control is Panel panel && control.GetType()==typeof(Panel)) {
				return panel.BorderStyle;
			}
			if(control is PictureBox pictureBox && control.GetType()==typeof(PictureBox)) {
				return pictureBox.BorderStyle;
			}
			if(control is UserControl userControl) {
				return userControl.BorderStyle;
			}
			return BorderStyle.None;
		}

		public static bool IsDrawn(Control control) {
			return control!=null && GetBorderStyle(control)!=BorderStyle.None;
		}

		public static void Attach(Control control) {
			if(!IsDrawn(control) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiBorderLook(control));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>The flat border in g over the window rectangle (origin at the window's top left): the edge Windows uses (2 px for 3-D, 1 for a
		///line) in the control's colour, its outer pixel StrokeStrong.</summary>
		public static void PaintBorder(Graphics g,Control control,Rectangle rectangleWindow) {
			BorderStyle borderStyle=GetBorderStyle(control);
			if(borderStyle==BorderStyle.None) {
				return;
			}
			int edge=borderStyle==BorderStyle.Fixed3D ? 2 : 1;
			using(Region region=new Region(rectangleWindow)) {
				region.Exclude(Rectangle.Inflate(rectangleWindow,-edge,-edge));
				System.Drawing.Drawing2D.GraphicsState graphicsState=g.Save();
				try {
					g.Clip=region;
					using(SolidBrush brush=new SolidBrush(UiLooks.GetBackColor(control))) {
						g.FillRectangle(brush,rectangleWindow);
					}
					UiControlPaint.Outline(g,EnumUiColor.StrokeStrong,rectangleWindow,1,UiTheme.CORNER_SQUARE,1f);
				}
				finally {
					g.Restore(graphicsState);
				}
			}
		}

		protected override void WndProc(ref Message m) {
			int msg=m.Msg;
			base.WndProc(ref m);
			try {
				if(msg==WM_NCDESTROY) {
					return;
				}
				if(msg==WM_NCPAINT) {
					IntPtr hdc=GetWindowDC(Handle);
					if(hdc==IntPtr.Zero) {
						return;
					}
					try {
						using(Graphics g=Graphics.FromHdc(hdc)) {
							PaintBorder(g,_control,new Rectangle(0,0,_control.Width,_control.Height));
						}
					}
					finally {
						ReleaseDC(Handle,hdc);
					}
				}
				else if(msg==WM_PRINT && ((int)m.LParam & PRF_NONCLIENT)!=0) {
					using(Graphics g=Graphics.FromHdc(m.WParam)) {
						PaintBorder(g,_control,new Rectangle(0,0,_control.Width,_control.Height));
					}
				}
			}
			catch(Exception) {
				//The look is only a picture.
			}
		}

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindowDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern int ReleaseDC(IntPtr hWnd,IntPtr hDC);
	}
}
