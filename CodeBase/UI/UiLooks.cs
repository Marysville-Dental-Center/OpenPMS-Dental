// UI-13 (docs/port-notes/UI-13.md; the owner, 2026-11-22: "Overall make sure you modernize all element types"): the shared looks of the stock
// Windows Forms element types that UI-4 to UI-12 left in their Windows-classic picture, attached to every control of a window by
// UiInputFrame.AttachOne (FormODBase and LayoutManagerForms call it, and UiPlainWindow for the plain windows), never per window. Each keeps the
// control's behaviour, values, events and texts; only its picture changes, in the UiTheme tokens. UI-PLAIN (docs/port-notes/UI-PLAIN.md) moved
// them from OpenPmsDental/UI to CodeBase, so the plain exes' windows get them too; the program gives them its window scale and zoom
// (FuncGetScaleWindow, FuncGetZoom, set by LayoutManagerForms):
// - every scroll bar (UiScrollLook) and the track bar (UiTrackBarLook), in their own files;
// - the stock button (System.Windows.Forms.Button, not the program's UI.Button): the program's button look (UiControlPaint.DrawButton: flat
//   SurfaceRaised, 1 px Stroke, 4 px corners, ButtonHover and ButtonPressed), or, when it carries its own colour (a colour swatch, the time
//   pattern's slider handle), that colour as a rounded swatch with a 1 px StrokeStrong edge (UiButtonLook);
// - the stock check box: the program's check box (UiCheckLook, as UiRadioLook draws the stock radio button);
// - a label used as a grey block (BackColor LightGray, ControlAccount's "Ins Rem" and "DPlan Rem"): the program's button look (UiBlockLook);
// - the stock progress bar: a rounded Stroke track with the Accent fill (UiProgressLook);
// - a 3-D or black single-line border of a panel, picture box, user control, stock list, tree or list view: one flat 1 px StrokeStrong line
//   (UiBorderLook);
// - the stock month calendar and the date picker's drop-down calendar: the token colours where Windows draws them without a theme;
// - the tree view and list view: Windows 11's Explorer theme (SetWindowTheme), the hover and selection Windows 11's own file lists use;
// - the stock data grid view: the grid's tokens (header SurfaceSubtle, lines Gridline, selection AccentSubtle) where it has Windows' defaults;
// - the link label: Accent links where it has Windows' default blue;
// - UI-PLAIN: the stock group box (the plain windows' 36): the program's group box (UI.GroupBox: 6 px corners, 1 px Stroke, the title inside
//   the box at its top left), UiGroupBoxLook; the stock tab control with its tabs on top: the program's tab control (UiTabLook, its own file).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CodeBase {
	///<summary>Attaches UI-13's shared looks of the stock element types to one control.</summary>
	public static class UiLooks {
		///<summary>The scale of the program's window (a FormODBase: its dpi times the program's zoom) that holds the control, or 0 when the control
		///is not in one. Set by the program (LayoutManagerForms); null in the plain exes, whose windows are not FormODBase.</summary>
		public static Func<Control,float> FuncGetScaleWindow;
		///<summary>The program's zoom (ComputerPrefs' Zoom/100; 1 without a database). Set by the program (LayoutManagerForms); null in the plain
		///exes, which have no zoom (1).</summary>
		public static Func<float> FuncGetZoom;
		private static readonly PropertyInfo _propertyInfoShowKeyboardCues=typeof(Control).GetProperty("ShowKeyboardCues",BindingFlags.Instance|BindingFlags.NonPublic);
		///<summary>The data grid views whose frame the look paints (its Paint handler is attached once).</summary>
		private static readonly ConditionalWeakTable<DataGridView,object> _tableDataGridViews=new ConditionalWeakTable<DataGridView,object>();
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
			if(UiCaptionLook.IsDrawn(control)) {
				UiCaptionLook.Attach(control);
			}
			if(UiSeparatorLook.IsDrawn(control)) {
				UiSeparatorLook.Attach(control);
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
			if(UiGroupBoxLook.IsDrawn(control)) {
				UiGroupBoxLook.Attach(control);
			}
			if(UiTabLook.IsDrawn(control)) {
				UiTabLook.Attach(control);
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

		///<summary>The scale of the looks for the control being painted: its window's scale when the window is a FormODBase (FormODBase sets its
		///LayoutManager from the monitor's dpi, SetScaleMS, and LayoutManager.ScaleMy multiplies the program's zoom; FuncGetScaleWindow), else the
		///control's own dpi (DeviceDpi/96) times the zoom (GetZoom). Never under 1.</summary>
		public static float GetScale(Control control) {
			float scaleWindow=0;
			int deviceDpi=96;
			if(control!=null) {
				deviceDpi=control.DeviceDpi;
				Func<Control,float> funcGetScaleWindow=FuncGetScaleWindow;
				if(funcGetScaleWindow!=null) {
					scaleWindow=funcGetScaleWindow(control);
				}
			}
			return GetScale(scaleWindow,deviceDpi,GetZoom());
		}

		///<summary>The program's zoom (FuncGetZoom: ComputerPrefs' Zoom/100, the zoom of a LayoutManager without a dpi of its own), 1 in a plain
		///exe. Never 0.</summary>
		public static float GetZoom() {
			Func<float> funcGetZoom=FuncGetZoom;
			float zoom=funcGetZoom==null ? 1f : funcGetZoom();
			if(zoom<=0) {
				return 1f;
			}
			return zoom;
		}

		///<summary>The scale from its parts: the window's scale (dpi times zoom) when there is one (above 0), else deviceDpi/96 times zoom. Never
		///under 1.</summary>
		public static float GetScale(float scaleWindow,int deviceDpi,float zoom) {
			if(scaleWindow>0) {
				return Math.Max(1f,scaleWindow);
			}
			if(zoom<=0) {
				zoom=1f;
			}
			return Math.Max(1f,Math.Max(1f,deviceDpi/96f)*zoom);
		}

		///<summary>UI-PLAIN: the mnemonic flag of a control's text as Windows Forms draws it: no prefix when the control does not use mnemonics, the
		///access key's underline hidden until Windows shows the keyboard cues (the Alt key; Control.ShowKeyboardCues), as Windows draws a stock
		///button, check box, label or group box. The looks drew the underline always (the plain windows' &amp;OK and &amp;Cancel).</summary>
		public static TextFormatFlags GetPrefixFlags(Control control,bool useMnemonic) {
			if(!useMnemonic) {
				return TextFormatFlags.NoPrefix;
			}
			try {
				if(_propertyInfoShowKeyboardCues!=null && !(bool)_propertyInfoShowKeyboardCues.GetValue(control)) {
					return TextFormatFlags.HidePrefix;
				}
			}
			catch(Exception) {
				//The underline shows, as Windows shows it when the cues are on.
			}
			return TextFormatFlags.Default;
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
			if(control is TreeView treeView && treeView.ShowLines) {
				//Windows 11's trees (Explorer's) have no dotted lines between the items: the chevrons and the indent show the levels.
				treeView.ShowLines=false;
			}
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
			if(dataGridView.GridColor==SystemColors.ControlDark || dataGridView.GridColor==SystemColors.WindowFrame) {
				dataGridView.GridColor=UiTheme.GetColor(EnumUiColor.Gridline);
			}
			int argbBackground=dataGridView.BackgroundColor.ToArgb();
			if(dataGridView.BackgroundColor==SystemColors.AppWorkspace || dataGridView.BackgroundColor==SystemColors.ControlDark
				|| argbBackground==Color.Gainsboro.ToArgb() || argbBackground==Color.DarkGray.ToArgb())
			{
				//Windows' grey and the classic greys windows chose (FormQuery's Gainsboro): the program's surface.
				dataGridView.BackgroundColor=UiTheme.GetColor(EnumUiColor.Surface);
			}
			dataGridView.EnableHeadersVisualStyles=false;
			//Without the visual styles a Raised header (the default) draws Windows' classic 3-D bevel: a single line instead, in GridColor
			//(Gridline), as the grid's header lines.
			if(dataGridView.ColumnHeadersBorderStyle==DataGridViewHeaderBorderStyle.Raised) {
				dataGridView.ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.Single;
			}
			if(dataGridView.RowHeadersBorderStyle==DataGridViewHeaderBorderStyle.Raised) {
				dataGridView.RowHeadersBorderStyle=DataGridViewHeaderBorderStyle.Single;
			}
			//The frame the grid draws itself (a black line, or a 3-D edge) painted over in StrokeStrong, as UiBorderLook draws the panels' borders.
			if(!_tableDataGridViews.TryGetValue(dataGridView,out _)) {
				_tableDataGridViews.Add(dataGridView,new object());
				dataGridView.Paint+=(sender,e) => {
					try {
						PaintDataGridViewBorder(e.Graphics,(DataGridView)sender);
					}
					catch(Exception) {
						//The look is only a picture.
					}
				};
			}
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

		///<summary>The border a data grid view draws inside its client area (BorderStyle FixedSingle: a black line; Fixed3D: a sunken 2 px edge)
		///painted over as one flat 1 px StrokeStrong line, the rest of a 3-D edge in the grid's background colour.</summary>
		public static void PaintDataGridViewBorder(Graphics g,DataGridView dataGridView) {
			if(dataGridView.BorderStyle==BorderStyle.None) {
				return;
			}
			Rectangle rectangle=new Rectangle(0,0,dataGridView.ClientSize.Width,dataGridView.ClientSize.Height);
			if(dataGridView.BorderStyle==BorderStyle.Fixed3D) {
				using(Pen pen=new Pen(dataGridView.BackgroundColor)) {
					g.DrawRectangle(pen,1,1,rectangle.Width-3,rectangle.Height-3);
				}
			}
			UiControlPaint.Outline(g,EnumUiColor.StrokeStrong,rectangle,1,UiTheme.CORNER_SQUARE,1f);
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
			if(control==null || control.GetType()!=typeof(System.Windows.Forms.Button)) {
				return false;
			}
			System.Windows.Forms.Button button=(System.Windows.Forms.Button)control;
			if(button.FlatStyle==FlatStyle.System) {
				return false;
			}
			//A flat borderless button is a part of the control that owns it (the date picker's calendar button, UI-4b): that control styles it.
			if(button.FlatStyle==FlatStyle.Flat && button.FlatAppearance.BorderSize==0) {
				return false;
			}
			//A button its owner already paints (the colour picker's swatch, UI-4c, in a Paint handler) keeps its owner's picture.
			return !IsPaintedByOwner(button);
		}

		///<summary>True when the control's Paint event has a handler before the look attaches (its owner paints it).</summary>
		public static bool IsPaintedByOwner(Control control) {
			try {
				FieldInfo fieldInfoKey=typeof(Control).GetField("s_paintEvent",BindingFlags.Static|BindingFlags.NonPublic)
					??typeof(Control).GetField("EventPaint",BindingFlags.Static|BindingFlags.NonPublic);
				PropertyInfo propertyInfoEvents=typeof(System.ComponentModel.Component).GetProperty("Events",BindingFlags.Instance|BindingFlags.NonPublic);
				if(fieldInfoKey==null || propertyInfoEvents==null) {
					return false;
				}
				System.ComponentModel.EventHandlerList eventHandlerList=(System.ComponentModel.EventHandlerList)propertyInfoEvents.GetValue(control);
				return eventHandlerList[fieldInfoKey.GetValue(null)]!=null;
			}
			catch(Exception) {
				return false;
			}
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

		///<summary>True when the button shows its own colour (a colour swatch, a handle): no icon or text, not the visual-style back colour, and an
		///opaque colour other than the control colours.</summary>
		public static bool IsSwatch(System.Windows.Forms.Button button) {
			if(button.UseVisualStyleBackColor) {
				return false;
			}
			if(button.Image!=null || button.BackgroundImage!=null || !string.IsNullOrEmpty(button.Text)) {
				//A button with an icon or a text on a colour (FormProcEdit's small edit and delete buttons) is a button, not a swatch.
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
			float scale=UiLooks.GetScale(button);
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
			if(button.BackgroundImage!=null) {
				//The background picture as Windows Forms lays it out, inside the frame.
				Rectangle rectangleBack=Rectangle.Inflate(rectangle,-UiTheme.GetLineWidth(scale),-UiTheme.GetLineWidth(scale));
				if(button.BackgroundImageLayout==ImageLayout.Stretch) {
					g.DrawImage(button.BackgroundImage,rectangleBack);
				}
				else {
					Size sizeImage=button.BackgroundImage.Size;
					if(button.BackgroundImageLayout==ImageLayout.Zoom) {
						float ratio=Math.Min(rectangleBack.Width/(float)sizeImage.Width,rectangleBack.Height/(float)sizeImage.Height);
						sizeImage=new Size((int)(sizeImage.Width*ratio),(int)(sizeImage.Height*ratio));
					}
					g.DrawImage(button.BackgroundImage,GetAligned(rectangleBack,sizeImage,ContentAlignment.MiddleCenter));
				}
			}
			if(button.Image!=null && !string.IsNullOrEmpty(button.Text) && button.TextImageRelation!=TextImageRelation.Overlay) {
				//UI-PLAIN: the image beside, above or below the text (TextImageRelation) as Windows Forms lays them out (the dashboard's Add Col
				//and Add Row: the text over its icon, the text before its icon); the look drew them where each alignment alone put them.
				PaintImageAndText(g,button,rectangleContent,colorText,colorParent,scale);
				return;
			}
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
			TextFormatFlags textFormatFlags=GetButtonTextFlags(button,rectangleContent);
			if((textFormatFlags & TextFormatFlags.SingleLine)!=0) {
				//One line, as Windows draws it: only the frame's line is kept clear at the sides, so a caption that fit before still fits.
				//A text after a left image keeps its start after the image.
				int widthLine=UiTheme.GetLineWidth(scale);
				bool isAfterImage=rectangleContent.X>rectangle.X+UiTheme.ScaleI(UiTheme.SPACE_4,scale);
				int xLeft=isAfterImage ? rectangleContent.X : rectangle.X+widthLine;
				rectangleContent=new Rectangle(xLeft,rectangleContent.Y,Math.Max(0,rectangle.Right-widthLine-xLeft),rectangleContent.Height);
			}
			TextRenderer.DrawText(g,button.Text,button.Font,rectangleContent,colorText,textFormatFlags);
		}

		///<summary>UI-PLAIN: a button's image and text with a TextImageRelation other than Overlay, as one block: the image and the text side by side
		///(ImageBeforeText, TextBeforeImage) or one over the other (ImageAboveText, TextAboveImage), the gap between them the icon-text gap, the
		///text wrapped in the content's width when they stand one over the other. The block stands where both alignments put it on an axis where
		///they agree, else centred on that axis (Windows Forms' own drawing of the dashboard's buttons: Add Row, image MiddleLeft and text
		///MiddleRight, is centred; Add Col, image TopCenter and text BottomCenter, is centred).</summary>
		public static void PaintImageAndText(Graphics g,System.Windows.Forms.Button button,Rectangle rectangleContent,Color colorText,Color colorParent,float scale) {
			Size sizeImage=button.Image.Size;
			int gap=UiTheme.ScaleI(UiTheme.GAP_ICON_TEXT,scale);
			TextImageRelation textImageRelation=button.TextImageRelation;
			bool isVertical=textImageRelation==TextImageRelation.ImageAboveText || textImageRelation==TextImageRelation.TextAboveImage;
			bool isImageFirst=textImageRelation==TextImageRelation.ImageBeforeText || textImageRelation==TextImageRelation.ImageAboveText;
			TextFormatFlags textFormatFlagsMeasure=UiLooks.GetPrefixFlags(button,button.UseMnemonic);
			Size sizeText;
			if(isVertical) {
				textFormatFlagsMeasure|=TextFormatFlags.WordBreak;
				sizeText=TextRenderer.MeasureText(g,button.Text,button.Font,new Size(Math.Max(1,rectangleContent.Width),int.MaxValue),textFormatFlagsMeasure);
				sizeText=new Size(Math.Min(sizeText.Width,rectangleContent.Width),sizeText.Height);
			}
			else {
				textFormatFlagsMeasure|=TextFormatFlags.SingleLine;
				sizeText=TextRenderer.MeasureText(g,button.Text,button.Font,new Size(int.MaxValue,int.MaxValue),textFormatFlagsMeasure);
				sizeText=new Size(Math.Min(sizeText.Width,Math.Max(0,rectangleContent.Width-sizeImage.Width-gap)),sizeText.Height);
			}
			Size sizeBlock=isVertical ? new Size(Math.Max(sizeImage.Width,sizeText.Width),sizeImage.Height+gap+sizeText.Height)
				: new Size(sizeImage.Width+gap+sizeText.Width,Math.Max(sizeImage.Height,sizeText.Height));
			Rectangle rectangleBlock=GetAligned(rectangleContent,sizeBlock,GetBlockAlign(button.ImageAlign,button.TextAlign));
			Rectangle rectangleImage;
			Rectangle rectangleText;
			if(isVertical) {
				int yImage=isImageFirst ? rectangleBlock.Y : rectangleBlock.Bottom-sizeImage.Height;
				int yText=isImageFirst ? rectangleBlock.Y+sizeImage.Height+gap : rectangleBlock.Y;
				rectangleImage=new Rectangle(rectangleBlock.X+(rectangleBlock.Width-sizeImage.Width)/2,yImage,sizeImage.Width,sizeImage.Height);
				rectangleText=new Rectangle(rectangleBlock.X,yText,rectangleBlock.Width,sizeText.Height);
			}
			else {
				int xImage=isImageFirst ? rectangleBlock.X : rectangleBlock.Right-sizeImage.Width;
				int xText=isImageFirst ? rectangleBlock.X+sizeImage.Width+gap : rectangleBlock.X;
				rectangleImage=new Rectangle(xImage,rectangleBlock.Y+(rectangleBlock.Height-sizeImage.Height)/2,sizeImage.Width,sizeImage.Height);
				rectangleText=new Rectangle(xText,rectangleBlock.Y,sizeText.Width,rectangleBlock.Height);
			}
			if(button.Enabled) {
				g.DrawImage(button.Image,rectangleImage);
			}
			else {
				ControlPaint.DrawImageDisabled(g,button.Image,rectangleImage.X,rectangleImage.Y,colorParent);
			}
			TextFormatFlags textFormatFlags=TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|UiLooks.GetPrefixFlags(button,button.UseMnemonic)
				|(isVertical ? TextFormatFlags.WordBreak : TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis);
			TextRenderer.DrawText(g,button.Text,button.Font,rectangleText,colorText,textFormatFlags);
		}

		///<summary>Where a button's image and text stand as one block: on each axis the side both alignments name, else the centre.</summary>
		public static ContentAlignment GetBlockAlign(ContentAlignment contentAlignmentImage,ContentAlignment contentAlignmentText) {
			int columnImage=GetColumn(contentAlignmentImage);
			int columnText=GetColumn(contentAlignmentText);
			int rowImage=GetRow(contentAlignmentImage);
			int rowText=GetRow(contentAlignmentText);
			int column=columnImage==columnText ? columnImage : 1;
			int row=rowImage==rowText ? rowImage : 1;
			ContentAlignment[,] arrayAlignments=new ContentAlignment[,] {
				{ ContentAlignment.TopLeft,ContentAlignment.TopCenter,ContentAlignment.TopRight },
				{ ContentAlignment.MiddleLeft,ContentAlignment.MiddleCenter,ContentAlignment.MiddleRight },
				{ ContentAlignment.BottomLeft,ContentAlignment.BottomCenter,ContentAlignment.BottomRight } };
			return arrayAlignments[row,column];
		}

		///<summary>0 left, 1 centre, 2 right.</summary>
		private static int GetColumn(ContentAlignment contentAlignment) {
			if(contentAlignment==ContentAlignment.TopLeft || contentAlignment==ContentAlignment.MiddleLeft || contentAlignment==ContentAlignment.BottomLeft) {
				return 0;
			}
			if(contentAlignment==ContentAlignment.TopRight || contentAlignment==ContentAlignment.MiddleRight || contentAlignment==ContentAlignment.BottomRight) {
				return 2;
			}
			return 1;
		}

		///<summary>0 top, 1 middle, 2 bottom.</summary>
		private static int GetRow(ContentAlignment contentAlignment) {
			if(contentAlignment==ContentAlignment.TopLeft || contentAlignment==ContentAlignment.TopCenter || contentAlignment==ContentAlignment.TopRight) {
				return 0;
			}
			if(contentAlignment==ContentAlignment.BottomLeft || contentAlignment==ContentAlignment.BottomCenter || contentAlignment==ContentAlignment.BottomRight) {
				return 2;
			}
			return 1;
		}

		///<summary>The text flags of a stock button's caption in its content rectangle: wrapped (WordBreak) only when the rectangle holds two lines
		///of its font, else one line (SingleLine), so a caption wider than a one-line button is never broken onto a line its bottom edge cuts.
		///</summary>
		public static TextFormatFlags GetButtonTextFlags(System.Windows.Forms.Button button,Rectangle rectangleContent) {
			TextFormatFlags textFormatFlags=GetTextFlags(button.TextAlign);
			if(rectangleContent.Height>=2*button.Font.Height) {
				textFormatFlags|=TextFormatFlags.WordBreak;
			}
			else {
				textFormatFlags|=TextFormatFlags.SingleLine;
			}
			textFormatFlags|=UiLooks.GetPrefixFlags(button,button.UseMnemonic);
			return textFormatFlags;
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
			float scale=UiLooks.GetScale(label);
			Rectangle rectangle=new Rectangle(0,0,label.Width,label.Height);
			using(SolidBrush brush=new SolidBrush(label.Parent==null ? SystemColors.Control : UiLooks.GetBackColor(label.Parent))) {
				g.FillRectangle(brush,rectangle);
			}
			EnumUiColor uiColorText=UiControlPaint.DrawButton(g,rectangle,buttonState,isPrimary:false,isFocused:false,scale);
			TextFormatFlags textFormatFlags=UiButtonLook.GetTextFlags(label.TextAlign)|TextFormatFlags.WordBreak;
			textFormatFlags|=UiLooks.GetPrefixFlags(label,label.UseMnemonic);
			TextRenderer.DrawText(g,label.Text,label.Font,rectangle,UiTheme.GetColor(uiColorText),textFormatFlags);
		}
	}

	///<summary>A label used as a grid title (BackColor ActiveCaption, Windows' title-bar blue: FormQuery's labelGridCaption over its result
	///grid) drawn as the program's grid title (GridOD's, GridScreenLook.DrawTitle): Accent with the text in TextOnAccent, in the title font token
	///at the program's zoom, where the label puts its text. Painted in the label's Paint event.</summary>
	public class UiCaptionLook {
		private static readonly ConditionalWeakTable<Control,UiCaptionLook> _table=new ConditionalWeakTable<Control,UiCaptionLook>();

		private UiCaptionLook(Label label) {
			label.Paint+=(sender,e) => {
				try {
					PaintCaption(e.Graphics,(Label)sender);
				}
				catch(Exception) {
					//The look is only a picture.
				}
			};
		}

		///<summary>True for a label (not a link label) whose colour is ActiveCaption and that its owner does not paint.</summary>
		public static bool IsDrawn(Control control) {
			return control!=null && control.GetType()==typeof(Label) && control.BackColor.ToArgb()==SystemColors.ActiveCaption.ToArgb()
				&& !UiButtonLook.IsPaintedByOwner(control);
		}

		public static void Attach(Control control) {
			if(!IsDrawn(control) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiCaptionLook((Label)control));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>The title font: UiTheme.FontTitle at the program's zoom, as GridOD makes its title font (the dpi is Windows').</summary>
		public static Font GetFontTitle() {
			float zoom=UiLooks.GetZoom();
			return new Font(UiTheme.FontTitle.FontFamily,UiTheme.FontTitle.SizeInPoints*zoom,UiTheme.FontTitle.Style);
		}

		public static void PaintCaption(Graphics g,Label label) {
			Rectangle rectangle=new Rectangle(0,0,label.Width,label.Height);
			g.FillRectangle(UiTheme.GetBrush(EnumUiColor.Accent),rectangle);
			if(string.IsNullOrEmpty(label.Text)) {
				return;
			}
			TextFormatFlags textFormatFlags=UiButtonLook.GetTextFlags(label.TextAlign)|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis;
			textFormatFlags|=UiLooks.GetPrefixFlags(label,label.UseMnemonic);
			using(Font font=GetFontTitle()) {
				TextRenderer.DrawText(g,label.Text,font,Rectangle.Inflate(rectangle,-UiTheme.ScaleI(UiTheme.SPACE_4,UiLooks.GetScale(label)),0),UiTheme.GetColor(EnumUiColor.TextOnAccent),
					textFormatFlags);
			}
		}
	}

	///<summary>A thin childless panel in a dark colour used as a divider (FormInsPlan's panel1: 2 px of ControlText across the window) drawn as one
	///1 px Stroke line in its middle on the parent's colour, the dividers of the new look. Its size and place stay. Painted in its Paint event.
	///</summary>
	public class UiSeparatorLook {
		private static readonly ConditionalWeakTable<Control,UiSeparatorLook> _table=new ConditionalWeakTable<Control,UiSeparatorLook>();
		///<summary>The thickest divider at 96 dpi.</summary>
		private const int THICKNESS_MAX_96=3;

		private UiSeparatorLook(Panel panel) {
			panel.Paint+=(sender,e) => {
				try {
					Panel panelSender=(Panel)sender;
					if(IsDrawn(panelSender)) {
						PaintSeparator(e.Graphics,panelSender);
					}
				}
				catch(Exception) {
					//The look is only a picture.
				}
			};
		}

		///<summary>True for a stock Panel with no children and no border, 3 px or less thick (scaled) and longer than thick, whose colour is
		///ControlText, WindowText, Black, ControlDark or ControlDarkDark.</summary>
		public static bool IsDrawn(Control control) {
			if(control==null || control.GetType()!=typeof(Panel) || control.HasChildren || ((Panel)control).BorderStyle!=BorderStyle.None) {
				return false;
			}
			int thickness=Math.Min(control.Width,control.Height);
			int length=Math.Max(control.Width,control.Height);
			if(thickness<1 || thickness>Math.Max(THICKNESS_MAX_96,(int)Math.Round(THICKNESS_MAX_96*UiLooks.GetScale(control))) || length<=thickness*2) {
				return false;
			}
			int argb=control.BackColor.ToArgb();
			return argb==SystemColors.ControlText.ToArgb() || argb==SystemColors.WindowText.ToArgb() || argb==Color.Black.ToArgb()
				|| argb==SystemColors.ControlDark.ToArgb() || argb==SystemColors.ControlDarkDark.ToArgb();
		}

		public static void Attach(Control control) {
			if(!IsDrawn(control) || UiButtonLook.IsPaintedByOwner(control) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiSeparatorLook((Panel)control));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>The parent's colour over the panel and one Stroke line (the line width of the scale) along its middle.</summary>
		public static void PaintSeparator(Graphics g,Panel panel) {
			Rectangle rectangle=new Rectangle(0,0,panel.Width,panel.Height);
			using(SolidBrush brush=new SolidBrush(panel.Parent==null ? SystemColors.Control : UiLooks.GetBackColor(panel.Parent))) {
				g.FillRectangle(brush,rectangle);
			}
			int widthLine=UiTheme.GetLineWidth(UiLooks.GetScale(panel));
			if(panel.Width>=panel.Height) {
				int heightLine=Math.Min(widthLine,panel.Height);
				g.FillRectangle(UiTheme.GetBrush(EnumUiColor.Stroke),new Rectangle(0,(panel.Height-heightLine)/2,panel.Width,heightLine));
			}
			else {
				int widthLineV=Math.Min(widthLine,panel.Width);
				g.FillRectangle(UiTheme.GetBrush(EnumUiColor.Stroke),new Rectangle((panel.Width-widthLineV)/2,0,widthLineV,panel.Height));
			}
		}
	}

	///<summary>The stock check box (System.Windows.Forms.CheckBox, not UI.CheckBox) drawn like the program's check box: the box in
	///UiControlPaint.DrawCheckBoxBox on the left with the text 3 px after it (as UiRadioLook draws the stock radio button), or, for a box the
	///window puts on the right (CheckAlign MiddleRight or TopRight), on the right with the text 3 px before it, where it was. A check box drawn as
	///a button or right to left keeps the Windows picture.</summary>
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
				&& (checkBox.CheckAlign==ContentAlignment.MiddleLeft || checkBox.CheckAlign==ContentAlignment.TopLeft
					|| checkBox.CheckAlign==ContentAlignment.MiddleRight || checkBox.CheckAlign==ContentAlignment.TopRight)
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
			bool isBoxRight=checkBox.CheckAlign==ContentAlignment.MiddleRight || checkBox.CheckAlign==ContentAlignment.TopRight;
			if(isBoxRight) {
				textFormatFlags=(textFormatFlags & ~TextFormatFlags.Left)|UiButtonLook.GetTextFlags(checkBox.TextAlign) & (TextFormatFlags.Right|TextFormatFlags.HorizontalCenter);
			}
			if(checkBox.CheckAlign==ContentAlignment.TopLeft || checkBox.CheckAlign==ContentAlignment.TopRight) {
				textFormatFlags|=TextFormatFlags.Top;
			}
			else {
				top=rectangle.Y+(int)Math.Round(rectangle.Height/2f-sizeBox/2f,0);
				textFormatFlags|=TextFormatFlags.VerticalCenter;
			}
			textFormatFlags|=UiLooks.GetPrefixFlags(checkBox,checkBox.UseMnemonic);
			int sizeDrawn=sizeBox+UiTheme.GetLineWidth(scale);
			Rectangle rectangleBox=new Rectangle(rectangle.X,top,sizeDrawn,sizeDrawn);
			Rectangle rectangleText=new Rectangle(rectangle.X+sizeBox+gap,rectangle.Y,Math.Max(0,rectangle.Width-sizeBox-gap),rectangle.Height);
			if(isBoxRight) {
				rectangleBox=new Rectangle(rectangle.Right-sizeDrawn,top,sizeDrawn,sizeDrawn);
				rectangleText=new Rectangle(rectangle.X,rectangle.Y,Math.Max(0,rectangle.Width-sizeBox-gap),rectangle.Height);
			}
			UiControlPaint.DrawCheckBoxBox(g,rectangleBox,checkBox.CheckState,isHovering,checkBox.Enabled,scale,Color.Empty);
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
							PaintCheck(g,_checkBox.ClientRectangle,_checkBox,UiLooks.GetScale(_checkBox),_isHovering);
						}
						break;
					case WM_PRINT:
					case WM_PRINTCLIENT:
						using(Graphics g=Graphics.FromHdc(m.WParam)) {
							PaintCheck(g,_checkBox.ClientRectangle,_checkBox,UiLooks.GetScale(_checkBox),false);
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
			float scale=UiLooks.GetScale(progressBar);
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

	///<summary>The border Windows draws around a panel, picture box, user control, stock list box, tree view or list view (BorderStyle Fixed3D, a
	///sunken 3-D edge, or FixedSingle, a black line) painted over as one flat 1 px StrokeStrong line with the control's colour inside it.</summary>
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
			if(control is System.Windows.Forms.ListBox listBox) {
				return listBox.BorderStyle;
			}
			if(control is TreeView treeView) {
				return treeView.BorderStyle;
			}
			if(control is ListView listView) {
				return listView.BorderStyle;
			}
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

	///<summary>UI-PLAIN: the stock group box (System.Windows.Forms.GroupBox, not the program's UI.GroupBox) drawn like the program's group box
	///(UI.GroupBox, UI-4a, section 3.6): the parent's colour behind 6 px corners, the box's colour inside, a 1 px Stroke outline over the whole
	///box, the title in Text at the top left inside the box (5 px in, 1 px down, at the look's scale). Painted in its Paint event, over what
	///Windows Forms drew; the box's children, size, place, title and behaviour stay.</summary>
	public class UiGroupBoxLook {
		private static readonly ConditionalWeakTable<Control,UiGroupBoxLook> _table=new ConditionalWeakTable<Control,UiGroupBoxLook>();

		private UiGroupBoxLook(System.Windows.Forms.GroupBox groupBox) {
			groupBox.Paint+=(sender,e) => {
				try {
					PaintGroupBox(e.Graphics,(System.Windows.Forms.GroupBox)sender);
				}
				catch(Exception) {
					//The look is only a picture: Windows Forms' own drawing stays.
				}
			};
			//Windows Forms draws a group box only where it changed; the look's outline runs along the whole box.
			groupBox.Resize+=(sender,e) => ((Control)sender).Invalidate();
		}

		///<summary>True for a stock group box drawn by Windows Forms (not FlatStyle.System, which Windows draws) that its owner does not paint.
		///</summary>
		public static bool IsDrawn(Control control) {
			if(control==null || control.GetType()!=typeof(System.Windows.Forms.GroupBox)) {
				return false;
			}
			if(((System.Windows.Forms.GroupBox)control).FlatStyle==FlatStyle.System) {
				return false;
			}
			return !UiButtonLook.IsPaintedByOwner(control);
		}

		public static void Attach(Control control) {
			if(!IsDrawn(control) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiGroupBoxLook((System.Windows.Forms.GroupBox)control));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		public static void PaintGroupBox(Graphics g,System.Windows.Forms.GroupBox groupBox) {
			float scale=UiLooks.GetScale(groupBox);
			Rectangle rectangle=new Rectangle(0,0,groupBox.Width,groupBox.Height);
			Color colorParent=groupBox.Parent==null ? SystemColors.Control : UiLooks.GetBackColor(groupBox.Parent);
			using(SolidBrush brushParent=new SolidBrush(colorParent)) {
				g.FillRectangle(brushParent,rectangle);
			}
			UiControlPaint.Fill(g,UiLooks.GetBackColor(groupBox),rectangle,UiTheme.CORNER_CONTAINER,scale);
			UiControlPaint.Outline(g,EnumUiColor.Stroke,rectangle,UiTheme.GetLineWidth(scale),UiTheme.CORNER_CONTAINER,scale);
			if(string.IsNullOrEmpty(groupBox.Text)) {
				return;
			}
			Color colorText=groupBox.Enabled ? UiTheme.GetColor(EnumUiColor.Text) : UiTheme.GetColor(EnumUiColor.TextDisabled);
			Point pointText=new Point(UiTheme.ScaleI(5,scale),1);
			TextFormatFlags textFormatFlags=TextFormatFlags.Left|TextFormatFlags.Top|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis
				|UiLooks.GetPrefixFlags(groupBox,useMnemonic:true);
			Rectangle rectangleText=new Rectangle(pointText.X,pointText.Y,Math.Max(0,groupBox.Width-pointText.X*2),groupBox.Font.Height+2);
			TextRenderer.DrawText(g,groupBox.Text,groupBox.Font,rectangleText,colorText,textFormatFlags);
		}
	}
}
