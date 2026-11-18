using System;
using System.Drawing;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI {
	public partial class ODColorPicker:UserControl {
		private Color _backgroundColor;
		///<summary>The mouse is over the colour button (its swatch's outline darkens).</summary>
		private bool _isHoverColor;
		///<summary>Set by the window's LayoutManager: the scale of the swatch's corners and lines (section 3.3 of the UI refresh plan).</summary>
		public LayoutManagerForms LayoutManager=new LayoutManagerForms();

		public Color BackgroundColor {
			get {
				return _backgroundColor;
			}
			set {
				_backgroundColor=value;
				butColor.BackColor=value;
				butColor.Invalidate();
			}
		}

		public bool AllowTransparentColor {
			set {
				butNone.Visible=value;
			}
		}

		public ODColorPicker() {
			InitializeComponent();
			Font=LayoutManagerForms.FontInitial;
			//UI-4c-2: the colour button draws the swatch in the new look over the Windows button (UiDialogPaint.DrawSwatch); its bounds, its click and
			//its colour dialog are as before.
			butColor.Paint+=butColor_Paint;
			butColor.MouseEnter+=(sender,e) => SetHoverColor(true);
			butColor.MouseLeave+=(sender,e) => SetHoverColor(false);
			butColor.GotFocus+=(sender,e) => butColor.Invalidate();
			butColor.LostFocus+=(sender,e) => butColor.Invalidate();
			butColor.EnabledChanged+=(sender,e) => butColor.Invalidate();
		}

		///<summary>True when the colour button shows the focus ring: it has focus and Windows asks for keyboard cues (section 3.6), so a click draws no
		///ring. The cues are the window's, so the picker's own ShowFocusCues is the button's.</summary>
		protected virtual bool IsFocusRingShown {
			get { return butColor.Focused && ShowFocusCues; }
		}

		///<summary>True while the mouse is over the colour button.</summary>
		protected virtual bool IsHoverColor {
			get { return _isHoverColor; }
		}

		private void SetHoverColor(bool isHover) {
			_isHoverColor=isHover;
			butColor.Invalidate();
		}

		///<summary>The colour button's swatch: the colour inside 4 px corners with the input outline (StrokeStrong, StrokeStrongHover under the mouse,
		///Stroke when disabled), the focus ring, and "None" as a white box with a diagonal line. Painted over the Windows button's own paint.</summary>
		private void butColor_Paint(object sender,PaintEventArgs e) {
			UiDialogPaint.DrawSwatch(e.Graphics,butColor.ClientRectangle,_backgroundColor,IsHoverColor,butColor.Enabled,IsFocusRingShown,
				LayoutManager.ScaleMy(),BackColor);
		}

		private void butNone_Click(object sender,EventArgs e) {
			BackgroundColor=Color.Transparent;
		}

		private void butColor_Click(object sender,EventArgs e) {
			colorDialog1.Color=BackgroundColor;
			colorDialog1.ShowDialog();
			BackgroundColor=colorDialog1.Color;
		}
	}
}
