/* ====================================================================
    Copyright (C) 2004-2006  fyiReporting Software, LLC

    This file is part of the fyiReporting RDL project.
	
    The RDL project is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

    For additional information, email info@fyireporting.com or visit
    the website www.fyiReporting.com.
*/
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;


namespace fyiReporting.RdlDesign
{
	internal class SimpleToggle : CheckBox
	{
		private Color _UpColor;
		private Color _DownColor;
		private Color _Transparency;
		private bool bIn=false;

		public SimpleToggle() 
		{	
			this.Appearance = Appearance.Button; 

			this.UpColor = Color.LightGray;
			this.DownColor = Color.Azure;
			this.TranparencyColor = Color.White;

			this.BackColor = this.Checked? this.DownColor: this.UpColor;
			this.ForeColor = this.Enabled? Color.Black: Color.Gray;
			this.Paint += new PaintEventHandler(this.DrawPanelPaint);
			this.MouseEnter +=new EventHandler(SimpleToggle_MouseEnter);
			this.MouseLeave +=new EventHandler(SimpleToggle_MouseLeave);
		}

		private void DrawPanelPaint(object sender, System.Windows.Forms.PaintEventArgs e)
		{

			Graphics g = e.Graphics;
			Brush b = null;
			Pen p = null;

			try			// never want to die in here
			{

				// UI-PLAIN-2 fix round: the program's tool button (UiToolStripRenderer's): the toolbar's colour (UpColor) at rest and when disabled
				// (was LightGray), checked AccentSubtle with a 1 px Accent border (was Azure with a blue rectangle), SubtleHover under the mouse,
				// 4 px corners, a disabled picture or text faded. BackColor keeps its values (Up, Down, LightGray) as before.
				b = new SolidBrush(this.UpColor);
				g.FillRectangle(b, e.ClipRectangle);
				float scale = this.DeviceDpi / 96f;
				int inset = CodeBase.UiTheme.ScaleI(1, scale);
				Rectangle rectShape = new Rectangle(inset, inset, this.Width - 2 * inset, this.Height - 2 * inset);
				if (this.Enabled && this.Checked)
				{
					CodeBase.UiControlPaint.Fill(g, CodeBase.EnumUiColor.AccentSubtle, rectShape, CodeBase.UiTheme.CORNER_CONTROL, scale);
					CodeBase.UiControlPaint.Outline(g, CodeBase.EnumUiColor.Accent, rectShape, CodeBase.UiTheme.GetLineWidth(scale), CodeBase.UiTheme.CORNER_CONTROL, scale);
				}
				else if (this.Enabled && bIn)
				{
					CodeBase.UiControlPaint.Fill(g, CodeBase.EnumUiColor.SubtleHover, rectShape, CodeBase.UiTheme.CORNER_CONTROL, scale);
				}
				if (this.Image != null)
				{
					int x = (this.Width - this.Image.Width) / 2;
					int y = (this.Height - this.Image.Height) / 2;

					// Draw Image using the transparency color
					ImageAttributes imageAttr = new ImageAttributes();
					imageAttr.SetColorKey(_Transparency, _Transparency,
						ColorAdjustType.Default);
					if (!this.Enabled)
					{
						ColorMatrix colorMatrix = new ColorMatrix();
						colorMatrix.Matrix33 = 0.35f;
						imageAttr.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
					}

					g.DrawImage(this.Image,         // Image
						new Rectangle(x, y, this.Image.Width, this.Image.Height),    // Dest. rect.
						0,							// srcX
						0,							// srcY
						this.Image.Width,           // srcWidth
						this.Image.Height,          // srcHeight
						GraphicsUnit.Pixel,			// srcUnit
						imageAttr);					// ImageAttributes
				}
				else
				{
					StringFormat format = new StringFormat(StringFormatFlags.NoWrap);
					g.DrawString(this.Text, this.Font, this.Enabled ? Brushes.Black : Brushes.Gray, new Rectangle(2, 2, this.Width, this.Height), format);
				}
			}
			catch {}	// todo draw the error message
			finally
			{
				if (b != null)
					b.Dispose();
				if (p != null)
					p.Dispose();
			}
		}

		public Color TranparencyColor
		{
			get { return this._Transparency;	}
			set { this._Transparency = value; }
		}

		public Color UpColor
		{
			get { return this._UpColor;	}
			set { this._UpColor = value; }
		}

		public Color DownColor
		{
			get { return this._DownColor;	}
			set { this._DownColor = value; }
		}

		protected override void OnCheckedChanged(EventArgs e)
		{
			base.OnCheckedChanged(e);		// CheckBox OnCheckedChanged method	 
			this.BackColor = this.Checked? this.DownColor: this.UpColor;
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);		// CheckBox OnClick method
			this.BackColor = this.Checked? this.DownColor: this.UpColor;
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);		// CheckBox OnEnabled method
			if (this.Enabled)
			{
				this.ForeColor = Color.Black;
				this.BackColor = this.Checked? this.DownColor: this.UpColor;
			}
			else
			{
				this.ForeColor = Color.LightGray;
				this.BackColor = Color.LightGray;
			}
		}

		private void SimpleToggle_MouseEnter(object sender, EventArgs e)
		{
			bIn = true;
		}

		private void SimpleToggle_MouseLeave(object sender, EventArgs e)
		{
			bIn = false;
		}
	}
}
