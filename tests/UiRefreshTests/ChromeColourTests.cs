// UI-11 (docs/port-notes/UI-11.md): the remaining background colours in their tokens. ColorOD, the old shared palette, now returns the tokens by
// role (and so their high-contrast partners); a borderless caption box (a read-only text box without a border in an old background, used as a
// label) takes its parent's colour; and a chrome control whose designer line went takes its parent's token. The source scan that keeps new
// chrome literals out is NoHqTests' ChromeColourScanTests. Nothing is shown; no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Drawing;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;

namespace UiRefreshTests {
	[TestClass]
	public class ChromeColourTests {
		[TestMethod]
		public void ColorOD_ReturnsTheTokens() {
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Surface),ColorOD.Background,"Background: the window background token");
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.SurfaceSubtle),ColorOD.Control,"Control: the controls' grey (SystemColors.Control's place)");
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Gridline),ColorOD.Gridline,"Gridline");
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.RowHover),ColorOD.Hover,"Hover: the hover fill");
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.StrokeStrong),ColorOD.Outline,"Outline: the controls' outline");
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.TextDisabled),ColorOD.DisabledText,"DisabledText");
			Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.Surface),ColorOD.GetColorBackground(),"GetColorBackground: the WPF window background");
		}

		[TestMethod]
		public void CaptionBox_TakesItsParentsColour() {
			using(Panel panel=new Panel()) {
				panel.BackColor=UiTheme.GetColor(EnumUiColor.Surface);
				TextBox textBoxCaption=new TextBox { BorderStyle=BorderStyle.None,ReadOnly=true,Multiline=true,BackColor=Color.FromArgb(252,253,254) };
				TextBox textBoxSurface=new TextBox { BorderStyle=BorderStyle.None,Multiline=true,BackColor=UiTheme.GetColorLight(EnumUiColor.Surface) };
				TextBox textBoxGrey=new TextBox { BorderStyle=BorderStyle.None,ReadOnly=true,BackColor=SystemColors.Control };
				TextBox textBoxFramed=new TextBox { ReadOnly=true,BackColor=Color.FromArgb(252,253,254) };
				TextBox textBoxWhite=new TextBox { BorderStyle=BorderStyle.None,BackColor=SystemColors.Window };
				TextBox textBoxNote=new TextBox { BorderStyle=BorderStyle.None,BackColor=Color.LightYellow };
				panel.Controls.AddRange(new Control[] { textBoxCaption,textBoxSurface,textBoxGrey,textBoxFramed,textBoxWhite,textBoxNote });
				foreach(Control control in panel.Controls) {
					UiLooks.AttachOne(control);
				}
				Assert.AreEqual(panel.BackColor,textBoxCaption.BackColor,"the old background 252,253,254");
				Assert.AreEqual(panel.BackColor,textBoxSurface.BackColor,"the Surface value a hand row wrote");
				Assert.AreEqual(panel.BackColor,textBoxGrey.BackColor,"the classic grey");
				Assert.AreEqual(Color.FromArgb(252,253,254).ToArgb(),textBoxFramed.BackColor.ToArgb(),"a framed box is an input: its colour stays");
				Assert.AreEqual(SystemColors.Window,textBoxWhite.BackColor,"a white borderless box stays white");
				Assert.AreEqual(Color.LightYellow,textBoxNote.BackColor,"a coloured box stays");
			}
		}

		[TestMethod]
		public void CaptionBox_OnATransparentParent_TakesTheSurface() {
			using(Panel panel=new Panel()) {
				panel.BackColor=Color.Transparent;
				TextBox textBox=new TextBox { BorderStyle=BorderStyle.None,ReadOnly=true,BackColor=Color.FromArgb(252,253,254) };
				panel.Controls.Add(textBox);
				UiLooks.AttachOne(textBox);
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Surface),textBox.BackColor);
			}
		}
	}
}
