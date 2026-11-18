using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI {
	///<summary>This panel differs from the stock MS Panel in two ways: It lets user set a border color and it's also double buffered.  This does support custom drawing, but it can't focus, so it won't accept keys.  For that, use ControlDoubleBuffered.  BorderStyle.None is default, so change that in addition to setting color.</summary>
	public partial class PanelOD:Panel {
		private Color _borderColor=Color.Silver;

		public PanelOD() {
			InitializeComponent();
			DoubleBuffered=true;
		}

		///<summary>The color of the border.</summary>
		[Category("Appearance"), Description("The color of the border.")]
		[DefaultValue(typeof(Color), "Silver")]
		public Color BorderColor { 
			get=>_borderColor; 
			set{
				_borderColor=value;
				Invalidate();
			}
		} 

		protected override void OnPaint(PaintEventArgs e) {
			using(SolidBrush solidBrushBack=new SolidBrush(BackColor)) {
				e.Graphics.FillRectangle(solidBrushBack,0,0,Width,Height);
			}
			if(BorderStyle==BorderStyle.FixedSingle){
				//Section 3.6: the default border (Silver) is the Stroke token; a colour a window sets stays. The panel stays square: rounding it would
				//mean dropping the Windows border, which changes the client area and so where the children sit (decision 1).
				Color colorBorder=BorderColor;
				if(colorBorder==Color.Silver){
					colorBorder=UiTheme.GetColor(EnumUiColor.Stroke);
				}
				using(Pen pen=new Pen(colorBorder)) {
					e.Graphics.DrawRectangle(pen,0,0,ClientSize.Width-1,ClientSize.Height-1);
				}
			}
			base.OnPaint(e);
		}

		protected override void OnResize(EventArgs eventargs) {
			base.OnResize(eventargs);
			this.Invalidate();
		}

		protected override void OnScroll(ScrollEventArgs se) {
			base.OnScroll(se);
			this.Invalidate();
		}

	}
}
