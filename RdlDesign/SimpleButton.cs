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
	internal class SimpleButton : Button
	{
		private Color _Transparency;
		private bool bDown=false;
		private bool bIn=false;

		public SimpleButton(Control parent) 
		{	
			this.Parent = parent;
			this.TranparencyColor = Color.White;

			this.BackColor = parent.BackColor;
			this.ForeColor = this.Enabled? Color.Black: Color.Gray;
			this.MouseDown +=new MouseEventHandler(SimpleButton_MouseDown);
			this.MouseUp +=new MouseEventHandler(SimpleButton_MouseUp);
			this.MouseEnter +=new EventHandler(SimpleButton_MouseEnter);
			this.MouseLeave +=new EventHandler(SimpleButton_MouseLeave);
			this.Paint += new PaintEventHandler(this.DrawPanelPaint);
		}

		private void DrawPanelPaint(object sender, System.Windows.Forms.PaintEventArgs e)
		{

			Graphics g = e.Graphics;
			Brush b = null;
			Pen p = null;

			try			// never want to die in here
			{
				// UI-PLAIN-2 fix round: the program's tool button (UiToolStripRenderer's): the toolbar's colour at rest and when disabled (was
				// LightGray), SubtleHover under the mouse and SubtlePressed while pressed with 4 px corners (was a blue rectangle), a disabled
				// picture faded. The gallery's before pictures (CodeBase.UiLooks.IsOffForGallery) draw as before.
				bool isOldLook = CodeBase.UiLooks.IsOffForGallery;
				b = new SolidBrush(this.Enabled || !isOldLook ? this.BackColor: Color.LightGray);
				g.FillRectangle(b, e.ClipRectangle);
				if (bIn && this.Enabled && isOldLook)
					g.DrawRectangle(Pens.Blue, 0, 0, this.Width-1, this.Height-1);
				else if (bIn && this.Enabled)
				{
					float scale = this.DeviceDpi / 96f;
					int inset = CodeBase.UiTheme.ScaleI(1, scale);
					Rectangle rectShape = new Rectangle(inset, inset, this.Width - 2 * inset, this.Height - 2 * inset);
					CodeBase.UiControlPaint.Fill(g, bDown ? CodeBase.EnumUiColor.SubtlePressed : CodeBase.EnumUiColor.SubtleHover, rectShape,
						CodeBase.UiTheme.CORNER_CONTROL, scale);
				}

				if (this.Image != null)
				{
					int x = (this.Width - this.Image.Width) / 2;
					int y = (this.Height - this.Image.Height) / 2;
					if (bDown && bIn)
					{
						x += 1;
						y += 1;
					}

					// Draw Image using the transparency color
					ImageAttributes imageAttr = new ImageAttributes();
					imageAttr.SetColorKey(_Transparency, _Transparency,
						ColorAdjustType.Default);
					if (!this.Enabled && !isOldLook)
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
					g.DrawString(this.Text, this.Font, this.Enabled || isOldLook ? Brushes.Black : Brushes.Gray, new Rectangle(2, 2, this.Width, this.Height), format);
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

		private void SimpleButton_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				bDown = true;
		}

		private void SimpleButton_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
				bDown = false;
		}

		private void SimpleButton_MouseEnter(object sender, EventArgs e)
		{
			bIn = true;
		}

		private void SimpleButton_MouseLeave(object sender, EventArgs e)
		{
			bIn = false;
		}
	}
}
