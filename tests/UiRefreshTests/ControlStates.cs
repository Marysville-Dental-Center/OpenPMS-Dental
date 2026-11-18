// The real controls of package UI-4a put in each state off screen (StateControls), and the design sheet's drawing of each state at a control's
// own bounds (StateDrawings: SheetPainter's shapes with the light tokens, as DesignSheetControls draws the families, without the text). Used by
// ControlStateTests and by the G-UI1 part 2 state strips (ControlStateGallery). docs/port-notes/UI-4a.md.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;

namespace UiRefreshTests {
	///<summary>The WinForms button with its states reachable from a test: the focus ring forced (a hidden window cannot take focus), hover and
	///press through the button's own mouse handlers.</summary>
	public class StateButton : OpenPmsDental.UI.Button {
		public bool IsFocusForced;

		protected override bool IsFocusRingShown {
			get { return IsFocusForced; }
		}

		public void Hover() {
			OnMouseEnter(EventArgs.Empty);
		}

		public void Press() {
			OnMouseDown(new MouseEventArgs(MouseButtons.Left,1,5,5,0));
		}
	}

	///<summary>The WinForms check box with its focus ring forced and hover through its own mouse handler.</summary>
	public class StateCheckBox : OpenPmsDental.UI.CheckBox {
		public bool IsFocusForced;

		protected override bool IsFocusRingShown {
			get { return IsFocusForced; }
		}

		public void Hover() {
			OnMouseMove(new MouseEventArgs(MouseButtons.None,0,5,5,0));
		}
	}

	///<summary>The WinForms tooltip window, painted into a bitmap by its own OnPaint (it is never shown). ScaleForced, when above 0, stands in
	///for its monitor's scale (the gallery's 125-200% strips).</summary>
	public class StateToolTip : OpenPmsDental.UI.ToolTipOD {
		public float ScaleForced;

		protected override float GetScale() {
			if(ScaleForced>0) {
				return ScaleForced;
			}
			return base.GetScale();
		}

		public void PaintTo(Graphics g) {
			using(PaintEventArgs paintEventArgs=new PaintEventArgs(g,ClientRectangle)) {
				OnPaint(paintEventArgs);
			}
		}
	}

	public static class StateControls {
		///<summary>The states of a check box (section 3.6): rest, hover, focus, disabled, checked; indeterminate is checked or disabled with the dash.</summary>
		public static readonly SheetState[] ArrayCheckStates=new SheetState[] { SheetState.Normal,SheetState.Hover,SheetState.Focused,SheetState.Disabled,SheetState.Checked };

		#region WinForms
		///<summary>A hidden form (never shown) with a panel in the Surface colour, the parent of the control under test.</summary>
		private static Form MakeHost(out System.Windows.Forms.Panel panel) {
			Form form=new Form();
			form.ShowInTaskbar=false;
			form.Size=new Size(600,400);//room for the group box at 200%
			panel=new System.Windows.Forms.Panel();
			panel.BackColor=SheetPainter.Light(EnumUiColor.Surface);
			panel.Bounds=new Rectangle(0,0,580,360);
			form.Controls.Add(panel);
			return form;
		}

		private static Bitmap Draw(Control control) {
			Bitmap bitmap=new Bitmap(control.Width,control.Height,PixelFormat.Format32bppArgb);
			control.DrawToBitmap(bitmap,new Rectangle(0,0,control.Width,control.Height));
			return bitmap;
		}

		public static Bitmap RenderWinFormsButton(SheetState sheetState,bool isPrimary,string text,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateButton button=new StateButton();
				button.Name="butSave";
				button.LayoutManager.SetScaleMS(scale);
				button.Font=new Font(button.Font.FontFamily,button.Font.Size*scale,button.Font.Style);
				button.Bounds=new Rectangle(10,10,UiTheme.ScaleI(75,scale),UiTheme.ScaleI(24,scale));
				button.Text=text;
				panel.Controls.Add(button);
				if(isPrimary) {
					form.AcceptButton=button;
				}
				IntPtr handle=button.Handle;
				ApplyWinFormsButtonState(button,sheetState);
				return Draw(button);
			}
		}

		public static void ApplyWinFormsButtonState(StateButton button,SheetState sheetState) {
			switch(sheetState) {
				case SheetState.Hover:
					button.Hover();
					break;
				case SheetState.Pressed:
					button.Hover();
					button.Press();
					break;
				case SheetState.Focused:
					button.IsFocusForced=true;
					break;
				case SheetState.Disabled:
					button.Enabled=false;
					break;
			}
		}

		public static Bitmap RenderWinFormsCheckBox(SheetState sheetState,bool isIndeterminate,string text) {
			return RenderWinFormsCheckBox(sheetState,isIndeterminate,text,1f);
		}

		public static Bitmap RenderWinFormsCheckBox(SheetState sheetState,bool isIndeterminate,string text,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateCheckBox checkBox=new StateCheckBox();
				checkBox.LayoutManager.SetScaleMS(scale);
				checkBox.Font=new Font(checkBox.Font.FontFamily,checkBox.Font.Size*scale,checkBox.Font.Style);
				checkBox.Bounds=new Rectangle(10,10,UiTheme.ScaleI(120,scale),UiTheme.ScaleI(18,scale));
				checkBox.Text=text;
				panel.Controls.Add(checkBox);
				IntPtr handle=checkBox.Handle;
				if(isIndeterminate) {
					checkBox.ThreeState=true;
					checkBox.CheckState=CheckState.Indeterminate;
				}
				switch(sheetState) {
					case SheetState.Hover:
						checkBox.Hover();
						break;
					case SheetState.Focused:
						checkBox.IsFocusForced=true;
						break;
					case SheetState.Disabled:
						checkBox.Enabled=false;
						break;
					case SheetState.Checked:
						if(!isIndeterminate) {
							checkBox.Checked=true;
						}
						break;
				}
				return Draw(checkBox);
			}
		}

		public static Bitmap RenderWinFormsGroupBox(string text,bool enabled) {
			return RenderWinFormsGroupBox(text,enabled,1f);
		}

		///<summary>The group box at a scale through its LayoutManager, as a window at that scale sizes it.</summary>
		public static Bitmap RenderWinFormsGroupBox(string text,bool enabled,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				OpenPmsDental.UI.GroupBox groupBox=new OpenPmsDental.UI.GroupBox();
				groupBox.LayoutManager.SetScaleMS(scale);
				groupBox.Font=new Font(groupBox.Font.FontFamily,groupBox.Font.Size*scale,groupBox.Font.Style);
				groupBox.Bounds=new Rectangle(10,10,UiTheme.ScaleI(200,scale),UiTheme.ScaleI(100,scale));
				groupBox.Text=text;
				groupBox.Enabled=enabled;
				panel.Controls.Add(groupBox);
				groupBox.BackColor=SheetPainter.Light(EnumUiColor.SurfaceRaised);
				return Draw(groupBox);
			}
		}

		public static Bitmap RenderWinFormsToolTip(string text,out float scale) {
			return RenderWinFormsToolTip(text,0,out scale);
		}

		///<summary>The tooltip painted at scaleForced (0: its monitor's scale, returned in scale), its size and font scaled with it.</summary>
		public static Bitmap RenderWinFormsToolTip(string text,float scaleForced,out float scale) {
			using(StateToolTip toolTipOD=new StateToolTip()) {
				toolTipOD.ScaleForced=scaleForced;
				float scaleSize=scaleForced>0 ? scaleForced : 1f;
				toolTipOD.StringToShow=text;
				toolTipOD.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f*scaleSize);
				toolTipOD.Size=new Size(UiTheme.ScaleI(120,scaleSize),UiTheme.ScaleI(18,scaleSize));
				scale=scaleForced>0 ? scaleForced : toolTipOD.DeviceDpi/96f;
				//A hidden window prints white through DrawToBitmap: its own paint is called into the bitmap instead.
				Bitmap bitmap=new Bitmap(toolTipOD.Width,toolTipOD.Height,PixelFormat.Format32bppArgb);
				using(Graphics g=Graphics.FromImage(bitmap)) {
					toolTipOD.PaintTo(g);
				}
				return bitmap;
			}
		}

		public static Bitmap RenderWinFormsPanelOD(out Rectangle rectangleClient) {
			return RenderWinFormsPanelOD(true,1f,out rectangleClient);
		}

		///<summary>PanelOD with the default border at the bounds a window at the scale gives it (the panel has no LayoutManager of its own: its
		///border stays 1 px, as in a scaled window).</summary>
		public static Bitmap RenderWinFormsPanelOD(bool enabled,float scale,out Rectangle rectangleClient) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				OpenPmsDental.UI.PanelOD panelOD=new OpenPmsDental.UI.PanelOD();
				panelOD.Bounds=new Rectangle(10,10,UiTheme.ScaleI(100,scale),UiTheme.ScaleI(60,scale));
				panelOD.BorderStyle=BorderStyle.FixedSingle;
				panelOD.Enabled=enabled;
				panelOD.BackColor=SheetPainter.Light(EnumUiColor.SurfaceRaised);
				panel.Controls.Add(panelOD);
				IntPtr handle=panelOD.Handle;
				int border=(panelOD.Width-panelOD.ClientSize.Width)/2;
				rectangleClient=new Rectangle(border,border,panelOD.ClientSize.Width,panelOD.ClientSize.Height);
				return Draw(panelOD);
			}
		}
		#endregion WinForms

		#region WPF
		private static void SetPrivate(object target,string field,object value) {
			FieldInfo fieldInfo=target.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic);
			fieldInfo.SetValue(target,value);
		}

		private static void CallPrivate(object target,string method) {
			target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic,null,Type.EmptyTypes,null).Invoke(target,null);
		}

		private static T GetInternal<T>(object target,string field) {
			return (T)target.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).GetValue(target);
		}

		///<summary>The control with the shared focus visual style laid over it at its bounds, as WPF's adorner draws it while the control has keyboard
		///focus (a control off screen cannot take keyboard focus).</summary>
		private static System.Windows.FrameworkElement WithFocusVisual(System.Windows.FrameworkElement frameworkElement,double width,double height,bool onAccent) {
			System.Windows.Controls.Grid grid=new System.Windows.Controls.Grid();
			grid.Width=width;
			grid.Height=height;
			grid.Children.Add(frameworkElement);
			System.Windows.Controls.Control controlFocus=new System.Windows.Controls.Control();
			controlFocus.Style=(System.Windows.Style)frameworkElement.FindResource(onAccent ? WpfControls.UI.UiLook.KeyFocusVisualStyleOnAccent : WpfControls.UI.UiLook.KeyFocusVisualStyle);
			controlFocus.Width=width;
			controlFocus.Height=height;
			grid.Children.Add(controlFocus);
			return grid;
		}

		public static Bitmap RenderWpfButton(SheetState sheetState,bool isPrimary,string text,bool isPinned,float scale) {
			WpfControls.UI.Button button=new WpfControls.UI.Button();
			button.Width=75;
			button.Height=24;
			button.Text=text;
			if(isPinned) {
				WpfControls.UI.ContentLook.SetIsPinned(button,true);
			}
			button.IsPrimary=isPrimary;
			if(sheetState==SheetState.Disabled) {
				button.IsEnabled=false;
			}
			if(sheetState==SheetState.Hover || sheetState==SheetState.Pressed) {
				SetPrivate(button,"_isHover",true);
				SetPrivate(button,"_isPressed",sheetState==SheetState.Pressed);
				CallPrivate(button,"SetColors");
			}
			System.Windows.FrameworkElement frameworkElement=button;
			if(sheetState==SheetState.Focused) {
				frameworkElement=WithFocusVisual(button,75,24,onAccent:isPrimary);
			}
			return ControlPictures.RenderWpf(frameworkElement,75,24,scale);
		}

		public static Bitmap RenderWpfCheckBox(SheetState sheetState,bool isIndeterminate,string text,bool isPinned,float scale,out Rectangle rectangleBox) {
			WpfControls.UI.CheckBox checkBox=new WpfControls.UI.CheckBox();
			checkBox.Width=100;
			checkBox.Height=19;
			checkBox.Text=text;
			if(isPinned) {
				WpfControls.UI.ContentLook.SetIsPinned(checkBox,true);
			}
			if(isIndeterminate) {
				checkBox.IsThreeState=true;
				checkBox.Checked=null;
			}
			else if(sheetState==SheetState.Checked) {
				checkBox.Checked=true;
			}
			if(sheetState==SheetState.Disabled) {
				checkBox.IsEnabled=false;
			}
			if(sheetState==SheetState.Hover) {
				SetPrivate(checkBox,"_isHover",true);
				CallPrivate(checkBox,"SetColors");
			}
			System.Windows.FrameworkElement frameworkElement=checkBox;
			if(sheetState==SheetState.Focused) {
				frameworkElement=WithFocusVisual(checkBox,100,19,onAccent:false);
			}
			Bitmap bitmap=ControlPictures.RenderWpf(frameworkElement,100,19,scale);
			System.Windows.FrameworkElement frameworkElementBox=GetInternal<System.Windows.FrameworkElement>(checkBox,isPinned ? "border" : "gridBoxNew");
			System.Windows.Point point=frameworkElementBox.TranslatePoint(new System.Windows.Point(0,0),checkBox);
			rectangleBox=UiTheme.SnapRectangle(new RectangleF((float)point.X*scale,(float)point.Y*scale,13*scale,13*scale));
			return bitmap;
		}

		public static Bitmap RenderWpfRadioButton(SheetState sheetState,string text,bool isPinned,out Rectangle rectangleCircle) {
			return RenderWpfRadioButton(sheetState,text,isPinned,1f,out rectangleCircle);
		}

		public static Bitmap RenderWpfRadioButton(SheetState sheetState,string text,bool isPinned,float scale,out Rectangle rectangleCircle) {
			WpfControls.UI.RadioButton radioButton=new WpfControls.UI.RadioButton();
			radioButton.Width=100;
			radioButton.Height=19;
			radioButton.Text=text;
			if(isPinned) {
				WpfControls.UI.ContentLook.SetIsPinned(radioButton,true);
			}
			if(sheetState==SheetState.Checked) {
				radioButton.Checked=true;
			}
			if(sheetState==SheetState.Disabled) {
				radioButton.IsEnabled=false;
			}
			if(sheetState==SheetState.Hover) {
				SetPrivate(radioButton,"_isHover",true);
				CallPrivate(radioButton,"SetColors");
			}
			Bitmap bitmap=ControlPictures.RenderWpf(radioButton,100,19,scale);
			System.Windows.FrameworkElement frameworkElementCircle=GetInternal<System.Windows.FrameworkElement>(radioButton,isPinned ? "ellipseOutline" : "ellipseNew");
			System.Windows.Point point=frameworkElementCircle.TranslatePoint(new System.Windows.Point(0,0),radioButton);
			rectangleCircle=UiTheme.SnapRectangle(new RectangleF((float)point.X*scale,(float)point.Y*scale,12*scale,12*scale));
			return bitmap;
		}

		public static Bitmap RenderWpfGroupBox(string text) {
			return RenderWpfGroupBox(text,true,1f);
		}

		public static Bitmap RenderWpfGroupBox(string text,bool enabled,float scale) {
			WpfControls.UI.GroupBox groupBox=new WpfControls.UI.GroupBox();
			groupBox.Width=200;
			groupBox.Height=100;
			groupBox.Text=text;
			groupBox.IsEnabled=enabled;
			//An element made in code fetches its theme style (Themes/Generic.xaml) when it is initialized, which happens when it gets a parent, as
			//in a Frm.
			System.Windows.Controls.Grid grid=new System.Windows.Controls.Grid();
			grid.Width=200;
			grid.Height=100;
			grid.Children.Add(groupBox);
			return ControlPictures.RenderWpf(grid,200,100,scale);
		}

		public static Bitmap RenderWpfToolTip(string text) {
			return RenderWpfToolTip(text,1f);
		}

		public static Bitmap RenderWpfToolTip(string text,float scale) {
			WpfControls.UI.ToolTip toolTip=new WpfControls.UI.ToolTip();
			GetInternal<System.Windows.Controls.TextBlock>(toolTip,"textBlock").Text=text;
			System.Windows.FrameworkElement frameworkElementContent=(System.Windows.FrameworkElement)toolTip.Content;
			frameworkElementContent.Width=120;
			frameworkElementContent.Height=22;
			frameworkElementContent.Measure(new System.Windows.Size(120,22));
			frameworkElementContent.Arrange(new System.Windows.Rect(0,0,120,22));
			frameworkElementContent.UpdateLayout();
			Bitmap bitmap=ControlPictures.RenderWpf(frameworkElementContent,120,22,scale);
			toolTip.Close();
			return bitmap;
		}

		public static Bitmap RenderWpfLabel(string text,bool isPinned) {
			return RenderWpfLabel(text,isPinned,true,1f);
		}

		public static Bitmap RenderWpfLabel(string text,bool isPinned,bool enabled,float scale) {
			WpfControls.UI.Label label=new WpfControls.UI.Label();
			label.Width=75;
			label.Height=18;
			label.Text=text;
			if(isPinned) {
				WpfControls.UI.ContentLook.SetIsPinned(label,true);
			}
			label.IsEnabled=enabled;
			return ControlPictures.RenderWpf(label,75,18,scale);
		}

		///<summary>The synthetic eForm with the pin taken off its fill control: the controls inside draw the new look.</summary>
		public static Bitmap RenderEFormUnpinned() {
			return EFormLookTests.RenderFill(enabled:true,isPinned:false);
		}
		#endregion WPF
	}

	///<summary>The design sheet's drawing of a state at a control's own bounds: the shapes and tokens of DesignSheetControls' families, without
	///the text, with the Fluent check mark and dash of UI-7a in place of the sheet's stand-in glyphs.</summary>
	public static class StateDrawings {
		private static Bitmap Start(Size size,Color colorBack,float scale,out Graphics g,out SheetPainter p) {
			Bitmap bitmap=new Bitmap(size.Width,size.Height,PixelFormat.Format32bppArgb);
			g=Graphics.FromImage(bitmap);
			g.Clear(colorBack);
			p=new SheetPainter(g,scale,SheetPainter.Light);
			return bitmap;
		}

		public static Color ButtonTextColor(SheetState sheetState,bool isPrimary) {
			if(sheetState==SheetState.Disabled) {
				return SheetPainter.Light(EnumUiColor.TextDisabled);
			}
			return SheetPainter.Light(isPrimary ? EnumUiColor.TextOnAccent : EnumUiColor.Text);
		}

		///<summary>DesignSheetControls.DrawButton at the button's bounds.</summary>
		public static Bitmap Button(Size size,SheetState sheetState,bool isPrimary,float scale,Color colorBack=default(Color)) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,colorBack.IsEmpty ? SheetPainter.Light(EnumUiColor.Surface) : colorBack,scale,out g,out p);
			using(g)
			using(p) {
				Rectangle rectangle=new Rectangle(Point.Empty,size);
				if(sheetState==SheetState.Disabled) {
					p.Fill(EnumUiColor.ButtonHover,rectangle,UiTheme.CORNER_CONTROL);
					p.Outline(EnumUiColor.Stroke,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
				}
				else if(isPrimary) {
					p.Fill(sheetState==SheetState.Hover ? EnumUiColor.AccentHover : (sheetState==SheetState.Pressed ? EnumUiColor.AccentPressed : EnumUiColor.Accent),rectangle,UiTheme.CORNER_CONTROL);
					if(sheetState==SheetState.Focused) {
						p.FocusRingOnAccent(rectangle,UiTheme.CORNER_CONTROL);
					}
				}
				else {
					p.Fill(sheetState==SheetState.Hover ? EnumUiColor.ButtonHover : (sheetState==SheetState.Pressed ? EnumUiColor.ButtonPressed : EnumUiColor.SurfaceRaised),rectangle,UiTheme.CORNER_CONTROL);
					p.Outline(EnumUiColor.Stroke,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
					if(sheetState==SheetState.Focused) {
						p.FocusRing(rectangle,UiTheme.CORNER_CONTROL);
					}
				}
			}
			return bitmap;
		}

		///<summary>DesignSheetControls.DrawCheckBox's box at the control's box, and the focus ring: around the box and its text inside the bounds for
		///WinForms (rectangleRing null), the control's bounds for WPF (the shared focus visual style).</summary>
		public static Bitmap CheckBox(Size size,Rectangle rectangleBox,SheetState sheetState,bool isIndeterminate,float scale,Color colorBack=default(Color),bool isRingOnBounds=false) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,colorBack.IsEmpty ? SheetPainter.Light(EnumUiColor.Surface) : colorBack,scale,out g,out p);
			using(g)
			using(p) {
				bool isFilled=sheetState==SheetState.Checked || isIndeterminate;
				if(isFilled) {
					p.Fill(sheetState==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.Accent,rectangleBox,UiTheme.CORNER_CHECKBOX);
					UiIcons.Draw(g,isIndeterminate ? UiGlyph.Dash : UiGlyph.Checkmark,rectangleBox,p.C(EnumUiColor.TextOnAccent));
				}
				else {
					p.Fill(EnumUiColor.SurfaceRaised,rectangleBox,UiTheme.CORNER_CHECKBOX);
					EnumUiColor uiColorBorder=sheetState==SheetState.Hover ? EnumUiColor.StrokeStrongHover
						: (sheetState==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.StrokeStrong);
					p.Outline(uiColorBorder,rectangleBox,p.LineWidth,UiTheme.CORNER_CHECKBOX);
				}
				if(sheetState==SheetState.Focused) {
					Rectangle rectangleRing=new Rectangle(Point.Empty,size);
					if(!isRingOnBounds) {
						int inset=p.S(4);
						rectangleRing=Rectangle.FromLTRB(Math.Max(0,rectangleBox.Left-inset),Math.Max(0,rectangleBox.Top-inset),
							Math.Min(size.Width,rectangleBox.Right+inset),Math.Min(size.Height,rectangleBox.Bottom+inset));
					}
					p.FocusRing(rectangleRing,UiTheme.CORNER_CONTROL);
				}
			}
			return bitmap;
		}

		///<summary>DesignSheetControls.DrawRadioButton's circle at the control's circle, on white.</summary>
		public static Bitmap RadioButton(Size size,Rectangle rectangleCircle,SheetState sheetState) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,Color.White,1f,out g,out p);
			using(g)
			using(p) {
				g.SmoothingMode=SmoothingMode.AntiAlias;
				if(sheetState==SheetState.Checked) {
					using(SolidBrush brush=new SolidBrush(p.C(EnumUiColor.Accent))) {
						g.FillEllipse(brush,rectangleCircle);
					}
					float dot=rectangleCircle.Width*0.4f;
					using(SolidBrush brush=new SolidBrush(p.C(EnumUiColor.TextOnAccent))) {
						g.FillEllipse(brush,rectangleCircle.X+(rectangleCircle.Width-dot)/2f,rectangleCircle.Y+(rectangleCircle.Height-dot)/2f,dot,dot);
					}
				}
				else {
					EnumUiColor uiColorBorder=sheetState==SheetState.Hover ? EnumUiColor.StrokeStrongHover
						: (sheetState==SheetState.Disabled ? EnumUiColor.TextDisabled : EnumUiColor.StrokeStrong);
					using(SolidBrush brush=new SolidBrush(p.C(EnumUiColor.SurfaceRaised))) {
						g.FillEllipse(brush,rectangleCircle);
					}
					using(Pen pen=new Pen(p.C(uiColorBorder),p.LineWidth)) {
						g.DrawEllipse(pen,UiTheme.GetStrokeRectangle(rectangleCircle,p.LineWidth));
					}
				}
			}
			return bitmap;
		}

		///<summary>The group box (section 3.6): the box's background with 6 px corners on the parent's colour, a 1 px Stroke outline.</summary>
		public static Bitmap GroupBox(Size size,Color colorBack=default(Color),Color colorParent=default(Color)) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,colorParent.IsEmpty ? SheetPainter.Light(EnumUiColor.Surface) : colorParent,1f,out g,out p);
			using(g)
			using(p) {
				Rectangle rectangle=new Rectangle(Point.Empty,size);
				p.Fill(colorBack.IsEmpty ? SheetPainter.Light(EnumUiColor.SurfaceRaised) : colorBack,rectangle,UiTheme.CORNER_CONTAINER);
				p.Outline(EnumUiColor.Stroke,rectangle,p.LineWidth,UiTheme.CORNER_CONTAINER);
			}
			return bitmap;
		}

		///<summary>The tooltip (section 3.6, as the sheet's window composite draws it): SurfaceRaised, 1 px Stroke, 4 px corners.</summary>
		public static Bitmap ToolTip(Size size,float scale,Color colorCorner=default(Color)) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,colorCorner.IsEmpty ? SheetPainter.Light(EnumUiColor.SurfaceRaised) : colorCorner,scale,out g,out p);
			using(g)
			using(p) {
				Rectangle rectangle=new Rectangle(Point.Empty,size);
				p.Fill(EnumUiColor.SurfaceRaised,rectangle,UiTheme.CORNER_CONTROL);
				p.Outline(EnumUiColor.Stroke,rectangle,p.LineWidth,UiTheme.CORNER_CONTROL);
			}
			return bitmap;
		}

		///<summary>PanelOD's client area: its background and the default border in Stroke, square.</summary>
		public static Bitmap PanelOD(Size size) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(size,SheetPainter.Light(EnumUiColor.SurfaceRaised),1f,out g,out p);
			using(g)
			using(p) {
				p.Outline(EnumUiColor.Stroke,new Rectangle(Point.Empty,size),1,0);
			}
			return bitmap;
		}
	}
}
