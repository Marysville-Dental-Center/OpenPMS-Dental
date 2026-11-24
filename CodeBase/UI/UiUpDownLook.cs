// UI-12 polish round (docs/port-notes/UI-12.md, round 6; the owner's addition of 2026-11-21, "drop down, radio, text also"): the arrow buttons
// of a stock up-down box (NumericUpDown, DomainUpDown) drew as dark blocks with white arrows beside the combo box's light chevron. This paints
// the up-down box's button window in the theme: the input's own fill (SurfaceRaised, as the combo box's box), SubtleHover under the pointer and
// SubtlePressed while a button is held, with the Fluent up and down chevrons in the token the combo box's chevron uses (UiListPaint.DrawChevron:
// Text, TextDisabled when disabled), one in each half. The buttons keep their Windows behaviour (clicks, repeat, keys); only their picture
// changes. UiInputFrame.AttachTree attaches it with the input frames.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace CodeBase {
	///<summary>Paints the arrow buttons of a stock up-down box like the combo box's drop area: a light fill and the Fluent chevrons.</summary>
	public class UiUpDownLook : NativeWindow {
		#region Fields
		private const int WM_PAINT=0x000F;
		private const int WM_PRINT=0x0317;
		private const int WM_PRINTCLIENT=0x0318;
		///<summary>The button windows that have the look: a control is attached once.</summary>
		private static readonly ConditionalWeakTable<Control,UiUpDownLook> _table=new ConditionalWeakTable<Control,UiUpDownLook>();
		private readonly Control _controlButtons;
		#endregion Fields

		private UiUpDownLook(Control controlButtons) {
			_controlButtons=controlButtons;
			if(controlButtons.IsHandleCreated) {
				AssignHandle(controlButtons.Handle);
			}
			controlButtons.HandleCreated+=(sender,e) => AssignHandle(_controlButtons.Handle);
			controlButtons.HandleDestroyed+=(sender,e) => ReleaseHandle();
			controlButtons.MouseMove+=(sender,e) => _controlButtons.Invalidate();
			controlButtons.MouseLeave+=(sender,e) => _controlButtons.Invalidate();
			controlButtons.MouseDown+=(sender,e) => _controlButtons.Invalidate();
			controlButtons.MouseUp+=(sender,e) => _controlButtons.Invalidate();
		}

		#region Methods - Public
		///<summary>True for the button window of a stock up-down box: the child of an UpDownBase that is not its text box.</summary>
		public static bool IsButtons(Control control) {
			return control!=null && control.Parent is UpDownBase && !(control is TextBoxBase);
		}

		public static void Attach(Control control) {
			if(!IsButtons(control) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiUpDownLook(control));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>Paints the two buttons over the whole client area (rectangle in g's coordinates): the up button in the upper half, the down
		///button in the lower half. pointMouse is the pointer in the rectangle's coordinates (null: not over the buttons); isPressed: the left
		///button is held.</summary>
		public static void PaintButtons(Graphics g,Rectangle rectangle,Control controlButtons,Point? pointMouse,bool isPressed) {
			if(rectangle.Width<=0 || rectangle.Height<=0) {
				return;
			}
			//The dpi, or the program's zoom read from the box's height (23 px at 100%), whichever is larger: the chevrons grow with the box as the
			//combo box's do.
			float scale=Math.Max(1f,controlButtons.DeviceDpi/96f);
			if(controlButtons.Parent!=null) {
				scale=Math.Max(scale,controlButtons.Parent.Height/23f);
			}
			bool isEnabled=controlButtons.Enabled && (controlButtons.Parent==null || controlButtons.Parent.Enabled);
			Color colorBack=controlButtons.Parent?.BackColor ?? SystemColors.Window;
			if(!isEnabled) {
				colorBack=SystemColors.Control;
			}
			if(colorBack.A<255) {
				colorBack=UiTheme.GetColor(EnumUiColor.SurfaceRaised);
			}
			int heightUp=rectangle.Height/2;
			Rectangle rectangleUp=new Rectangle(rectangle.X,rectangle.Y,rectangle.Width,heightUp);
			Rectangle rectangleDown=new Rectangle(rectangle.X,rectangle.Y+heightUp,rectangle.Width,rectangle.Height-heightUp);
			EnumUiColor uiColorGlyph=isEnabled ? EnumUiColor.Text : EnumUiColor.TextDisabled;
			DrawButton(g,rectangleUp,UiGlyph.ChevronUp,GetFill(rectangleUp,pointMouse,isPressed,isEnabled,colorBack),uiColorGlyph,scale);
			DrawButton(g,rectangleDown,UiGlyph.ChevronDown,GetFill(rectangleDown,pointMouse,isPressed,isEnabled,colorBack),uiColorGlyph,scale);
		}
		#endregion Methods - Public

		#region Methods - Private
		///<summary>The fill of one button: SubtlePressed while held under the pointer, SubtleHover under the pointer, the box's fill otherwise.</summary>
		private static Color GetFill(Rectangle rectangleButton,Point? pointMouse,bool isPressed,bool isEnabled,Color colorBack) {
			if(!isEnabled || pointMouse==null || !rectangleButton.Contains(pointMouse.Value)) {
				return colorBack;
			}
			return UiTheme.GetColor(isPressed ? EnumUiColor.SubtlePressed : EnumUiColor.SubtleHover);
		}

		///<summary>One button: its fill and its chevron, centred in it, at most the combo box's chevron size (SIZE_CHEVRON at the scale).</summary>
		private static void DrawButton(Graphics g,Rectangle rectangleButton,UiGlyph uiGlyph,Color colorFill,EnumUiColor uiColorGlyph,float scale) {
			using(SolidBrush solidBrush=new SolidBrush(colorFill)) {
				g.FillRectangle(solidBrush,rectangleButton);
			}
			int size=Math.Min(UiTheme.ScaleI(UiTheme.SIZE_CHEVRON,scale),rectangleButton.Width-2);
			if(size<=0) {
				return;
			}
			//The chevron's square centred on the button; its glyph is drawn in the middle of the square, so a square taller than the button
			//still draws the chevron inside it. Clipped to the button so the two chevrons never meet.
			Rectangle rectangleSlot=new Rectangle(rectangleButton.X+(rectangleButton.Width-size)/2,rectangleButton.Y+(rectangleButton.Height-size)/2,size,size);
			System.Drawing.Drawing2D.GraphicsState graphicsState=g.Save();
			try {
				g.IntersectClip(rectangleButton);
				UiIcons.Draw(g,uiGlyph,rectangleSlot,uiColorGlyph);
			}
			finally {
				g.Restore(graphicsState);
			}
		}

		///<summary>The pointer over the buttons in client coordinates, or null when it is elsewhere.</summary>
		private Point? GetMouse() {
			Point point=_controlButtons.PointToClient(Control.MousePosition);
			if(!_controlButtons.ClientRectangle.Contains(point)) {
				return null;
			}
			return point;
		}

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
			try {
				switch(m.Msg) {
					case WM_PAINT:
						using(Graphics g=_controlButtons.CreateGraphics()) {
							PaintButtons(g,_controlButtons.ClientRectangle,_controlButtons,GetMouse(),Control.MouseButtons==MouseButtons.Left);
						}
						break;
					case WM_PRINT:
					case WM_PRINTCLIENT:
						using(Graphics g=Graphics.FromHdc(m.WParam)) {
							PaintButtons(g,_controlButtons.ClientRectangle,_controlButtons,null,false);
						}
						break;
				}
			}
			catch(Exception) {
				//Buttons that cannot be painted keep the Windows picture; nothing else depends on it.
			}
		}
		#endregion Methods - Private
	}
}
