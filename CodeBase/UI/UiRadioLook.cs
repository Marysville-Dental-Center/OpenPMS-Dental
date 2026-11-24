// UI-12 pilot, round 3 (docs/port-notes/UI-12.md; the owner's addition of 2026-11-21: "radio buttons are laid out exactly like checkboxes"): the
// stock radio button (System.Windows.Forms.RadioButton) draws its Windows circle and puts its text 3 px further from it than the program's
// check box (UI.CheckBox: a 12 px box, its 1 px line and 3 px before the text). This paints a radio button with its circle on the left the
// way UI.CheckBox paints its box: the same size, place and tokens (UiControlPaint.DrawRadioCircle) and the text 16 px from the left at 96 DPI,
// so a radio button and a check box in one column start their text at one x. The radio button keeps its Windows behaviour (keys, groups,
// events); only its picture changes. A radio button drawn as a button (Appearance.Button), right to left or with its circle elsewhere than
// on the left keeps the Windows picture. UiInputFrame.AttachTree attaches it with the input frames.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace CodeBase {
	///<summary>Paints a stock radio button like the program's check box: the circle where UI.CheckBox puts its box, the text after it.</summary>
	public class UiRadioLook : NativeWindow {
		#region Fields
		private const int WM_PAINT=0x000F;
		private const int WM_PRINT=0x0317;
		private const int WM_PRINTCLIENT=0x0318;
		///<summary>The radio buttons that have the look: a control is attached once.</summary>
		private static readonly ConditionalWeakTable<Control,UiRadioLook> _table=new ConditionalWeakTable<Control,UiRadioLook>();
		private readonly RadioButton _radioButton;
		private bool _isHovering;
		///<summary>The circle's size at 96 DPI, before its 1 px line (UI.CheckBox's _sqrSize96).</summary>
		private const int SIZE_CIRCLE_96=12;
		#endregion Fields

		private UiRadioLook(RadioButton radioButton) {
			_radioButton=radioButton;
			if(radioButton.IsHandleCreated) {
				AssignHandle(radioButton.Handle);
			}
			radioButton.HandleCreated+=(sender,e) => AssignHandle(_radioButton.Handle);
			radioButton.HandleDestroyed+=(sender,e) => ReleaseHandle();
			radioButton.MouseEnter+=(sender,e) => { _isHovering=true; _radioButton.Invalidate(); };
			radioButton.MouseLeave+=(sender,e) => { _isHovering=false; _radioButton.Invalidate(); };
		}

		#region Methods - Public
		///<summary>True for a radio button the look paints: drawn as a radio button (not a button), its circle on the left, left to right.</summary>
		public static bool IsDrawn(Control control) {
			if(!(control is RadioButton radioButton)) {
				return false;
			}
			return radioButton.Appearance==Appearance.Normal
				&& (radioButton.CheckAlign==ContentAlignment.MiddleLeft || radioButton.CheckAlign==ContentAlignment.TopLeft
					//UI-13: a circle the window puts on the right is drawn there in the same look (FormPerio's sequence choices).
					|| radioButton.CheckAlign==ContentAlignment.MiddleRight || radioButton.CheckAlign==ContentAlignment.TopRight)
				&& radioButton.RightToLeft!=RightToLeft.Yes;
		}

		public static void Attach(Control control) {
			if(!(control is RadioButton radioButton) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiRadioLook(radioButton));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>Paints the radio button over its whole client area (rectangle in g's coordinates): its background, the circle and the text.</summary>
		public static void PaintRadio(Graphics g,Rectangle rectangle,RadioButton radioButton,float scale,bool isHovering) {
			Color colorBack=radioButton.BackColor;
			if(colorBack.A<255) {
				colorBack=radioButton.Parent?.BackColor ?? SystemColors.Control;
				if(colorBack.A<255) {
					colorBack=SystemColors.Control;
				}
			}
			using(SolidBrush brushBack=new SolidBrush(colorBack)) {
				g.FillRectangle(brushBack,rectangle);
			}
			int sizeCircle=(int)Math.Round(SIZE_CIRCLE_96*scale);
			int gap=(int)Math.Round(3*scale);
			int top=rectangle.Y;
			StringFormat stringFormat=new StringFormat();
			stringFormat.Alignment=StringAlignment.Near;
			bool isCircleRight=radioButton.CheckAlign==ContentAlignment.MiddleRight || radioButton.CheckAlign==ContentAlignment.TopRight;
			if(isCircleRight) {
				stringFormat.Alignment=StringAlignment.Far;
			}
			if(radioButton.CheckAlign==ContentAlignment.TopLeft || radioButton.CheckAlign==ContentAlignment.TopRight) {
				stringFormat.LineAlignment=StringAlignment.Near;
			}
			else {
				top=rectangle.Y+(int)Math.Round(rectangle.Height/2f-sizeCircle/2f,0);
				stringFormat.LineAlignment=StringAlignment.Center;
			}
			int sizeDrawn=sizeCircle+UiTheme.GetLineWidth(scale);
			Rectangle rectangleCircle=new Rectangle(rectangle.X,top,sizeDrawn,sizeDrawn);
			Rectangle rectangleText=new Rectangle(rectangle.X+sizeCircle+gap,rectangle.Y,Math.Max(0,rectangle.Width-sizeCircle-gap),rectangle.Height);
			if(isCircleRight) {
				rectangleCircle=new Rectangle(rectangle.Right-sizeDrawn,top,sizeDrawn,sizeDrawn);
				rectangleText=new Rectangle(rectangle.X,rectangle.Y,Math.Max(0,rectangle.Width-sizeCircle-gap),rectangle.Height);
			}
			UiControlPaint.DrawRadioCircle(g,rectangleCircle,radioButton.Checked,isHovering,radioButton.Enabled,scale,Color.Empty);
			//A token brush: cached per thread by UiTheme, never disposed here.
			SolidBrush solidBrushText=UiTheme.GetBrush(radioButton.Enabled ? EnumUiColor.Text : EnumUiColor.TextDisabled);
			g.DrawString(radioButton.Text,radioButton.Font,solidBrushText,rectangleText,stringFormat);
			if(radioButton.Focused) {
				Rectangle rectangleTextUsed=rectangleCircle;
				if(!string.IsNullOrEmpty(radioButton.Text)) {
					SizeF sizeFText=g.MeasureString(radioButton.Text,radioButton.Font,rectangleText.Width,stringFormat);
					int widthText=Math.Min(rectangleText.Width,(int)Math.Ceiling(sizeFText.Width));
					rectangleTextUsed=new Rectangle(rectangleText.X,rectangleCircle.Y,widthText,rectangleCircle.Height);
				}
				Rectangle rectangleRing=UiControlPaint.GetCheckFocusRectangle(rectangleCircle,rectangleTextUsed,rectangle,scale);
				UiControlPaint.FocusRing(g,rectangleRing,UiTheme.CORNER_CONTROL,scale);
			}
			stringFormat.Dispose();
		}
		#endregion Methods - Public

		#region Methods - Private
		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
			try {
				if(!IsDrawn(_radioButton)) {
					return;
				}
				switch(m.Msg) {
					case WM_PAINT:
						using(Graphics g=_radioButton.CreateGraphics()) {
							PaintRadio(g,_radioButton.ClientRectangle,_radioButton,UiLooks.GetScale(_radioButton),_isHovering);
						}
						break;
					case WM_PRINT:
					case WM_PRINTCLIENT:
						using(Graphics g=Graphics.FromHdc(m.WParam)) {
							PaintRadio(g,_radioButton.ClientRectangle,_radioButton,UiLooks.GetScale(_radioButton),false);
						}
						break;
				}
			}
			catch(Exception) {
				//A radio button that cannot be painted keeps the Windows picture; nothing else depends on it.
			}
		}
		#endregion Methods - Private
	}
}
