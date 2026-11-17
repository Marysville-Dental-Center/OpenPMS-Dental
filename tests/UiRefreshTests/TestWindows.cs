// Synthetic test windows for the UI-0b tests (docs/port-notes/UI-0b.md): written like designer code (InitializeComponent, then
// InitializeLayoutManager in the constructor, as every program window does), never shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Drawing;
using System.Windows.Forms;
using OpenPmsDental;

namespace UiRefreshTests {
	///<summary>A FormODBase window with the designer fonts of section 3.4's rule: Microsoft Sans Serif at 8, 8.25, 8.5 and 10, with Bold, Italic
	///and Underline, another family, a container whose child inherits, a stock check box and a text box.</summary>
	public class TestWindowFonts : FormODBase {
		public System.Windows.Forms.Label labelUnderline;
		public System.Windows.Forms.Label labelBold10;
		public System.Windows.Forms.Label labelArial;
		public System.Windows.Forms.Label labelItalic85;
		public System.Windows.Forms.RadioButton radioButton;
		public System.Windows.Forms.CheckBox checkBox8;
		public System.Windows.Forms.TextBox textBox;
		public System.Windows.Forms.Button buttonBold;
		public System.Windows.Forms.Panel panel;
		public System.Windows.Forms.Label labelInPanel;

		public TestWindowFonts() {
			InitializeComponent();
			InitializeLayoutManager();
		}

		private void InitializeComponent() {
			labelUnderline=new System.Windows.Forms.Label();
			labelBold10=new System.Windows.Forms.Label();
			labelArial=new System.Windows.Forms.Label();
			labelItalic85=new System.Windows.Forms.Label();
			radioButton=new System.Windows.Forms.RadioButton();
			checkBox8=new System.Windows.Forms.CheckBox();
			textBox=new System.Windows.Forms.TextBox();
			buttonBold=new System.Windows.Forms.Button();
			panel=new System.Windows.Forms.Panel();
			labelInPanel=new System.Windows.Forms.Label();
			panel.SuspendLayout();
			SuspendLayout();
			labelUnderline.Font=new Font("Microsoft Sans Serif",8.25F,FontStyle.Underline,GraphicsUnit.Point,0);
			labelUnderline.Location=new Point(10,10);
			labelUnderline.Name="labelUnderline";
			labelUnderline.Size=new Size(150,18);
			labelUnderline.Text="Underlined heading";
			labelBold10.Font=new Font("Microsoft Sans Serif",10F,FontStyle.Bold,GraphicsUnit.Point,0);
			labelBold10.Location=new Point(10,32);
			labelBold10.Name="labelBold10";
			labelBold10.Size=new Size(150,20);
			labelBold10.Text="Bold ten";
			labelArial.Font=new Font("Arial",8.25F,FontStyle.Bold|FontStyle.Italic,GraphicsUnit.Point,0);
			labelArial.Location=new Point(10,56);
			labelArial.Name="labelArial";
			labelArial.Size=new Size(150,18);
			labelArial.Text="Arial stays";
			labelItalic85.Font=new Font("Microsoft Sans Serif",8.5F,FontStyle.Italic,GraphicsUnit.Point,0);
			labelItalic85.Location=new Point(10,78);
			labelItalic85.Name="labelItalic85";
			labelItalic85.Size=new Size(150,18);
			labelItalic85.Text="Italic eight and a half";
			radioButton.Location=new Point(10,100);
			radioButton.Name="radioButton";
			radioButton.Size=new Size(150,18);
			radioButton.Text="Radio";
			checkBox8.Font=new Font("Microsoft Sans Serif",8F,FontStyle.Regular,GraphicsUnit.Point,0);
			checkBox8.Location=new Point(10,122);
			checkBox8.Name="checkBox8";
			checkBox8.Size=new Size(150,18);
			checkBox8.Text="Check eight";
			textBox.Location=new Point(10,144);
			textBox.Name="textBox";
			textBox.Size=new Size(150,20);
			buttonBold.Font=new Font("Microsoft Sans Serif",8.25F,FontStyle.Bold,GraphicsUnit.Point,0);
			buttonBold.Location=new Point(10,170);
			buttonBold.Name="buttonBold";
			buttonBold.Size=new Size(75,24);
			buttonBold.Text="Bold";
			panel.Controls.Add(labelInPanel);
			panel.Location=new Point(200,10);
			panel.Name="panel";
			panel.Size=new Size(150,60);
			labelInPanel.Location=new Point(5,5);
			labelInPanel.Name="labelInPanel";
			labelInPanel.Size=new Size(120,18);
			labelInPanel.Text="Inherits";
			ClientSize=new Size(400,220);
			Controls.Add(labelUnderline);
			Controls.Add(labelBold10);
			Controls.Add(labelArial);
			Controls.Add(labelItalic85);
			Controls.Add(radioButton);
			Controls.Add(checkBox8);
			Controls.Add(textBox);
			Controls.Add(buttonBold);
			Controls.Add(panel);
			Name="TestWindowFonts";
			Text="Test window fonts";
			panel.ResumeLayout(false);
			ResumeLayout(false);
		}

		public LayoutManagerForms GetLayoutManager() {
			return LayoutManager;
		}
	}

	///<summary>A plain Form (no LayoutManager) with an underlined Microsoft Sans Serif label and a label that inherits the window's font.</summary>
	public class TestPlainWindowFonts : Form {
		public System.Windows.Forms.Label labelUnderline;
		public System.Windows.Forms.Label labelInherits;

		public TestPlainWindowFonts() {
			labelUnderline=new System.Windows.Forms.Label();
			labelInherits=new System.Windows.Forms.Label();
			SuspendLayout();
			AutoScaleMode=AutoScaleMode.None;
			labelUnderline.Font=new Font("Microsoft Sans Serif",8.25F,FontStyle.Underline,GraphicsUnit.Point,0);
			labelUnderline.Location=new Point(10,10);
			labelUnderline.Name="labelUnderline";
			labelUnderline.Size=new Size(150,18);
			labelUnderline.Text="Underlined";
			labelInherits.Location=new Point(10,40);
			labelInherits.Name="labelInherits";
			labelInherits.Size=new Size(150,18);
			labelInherits.Text="Inherits";
			ClientSize=new Size(200,80);
			Controls.Add(labelUnderline);
			Controls.Add(labelInherits);
			Font=new Font("Microsoft Sans Serif",8.25F);
			ResumeLayout(false);
		}
	}

	///<summary>A FormODBase window with an AutoSize label next to a text box (labelNear, 4 px to the left of textNear) and one with free space
	///to its right (labelFree).</summary>
	public class TestWindowAutoSize : FormODBase {
		public System.Windows.Forms.Label labelNear;
		public System.Windows.Forms.TextBox textNear;
		public System.Windows.Forms.Label labelFree;

		public TestWindowAutoSize() {
			InitializeComponent();
			InitializeLayoutManager();
		}

		private void InitializeComponent() {
			labelNear=new System.Windows.Forms.Label();
			textNear=new System.Windows.Forms.TextBox();
			labelFree=new System.Windows.Forms.Label();
			SuspendLayout();
			labelNear.AutoSize=true;
			labelNear.Location=new Point(10,13);
			labelNear.Name="labelNear";
			labelNear.Text="Name";
			textNear.Location=new Point(60,10);
			textNear.Name="textNear";
			textNear.Size=new Size(120,20);
			labelFree.AutoSize=true;
			labelFree.Location=new Point(10,50);
			labelFree.Name="labelFree";
			labelFree.Text="Free";
			ClientSize=new Size(400,100);
			Controls.Add(labelNear);
			Controls.Add(textNear);
			Controls.Add(labelFree);
			Name="TestWindowAutoSize";
			Text="Test window AutoSize";
			ResumeLayout(false);
			PerformLayout();
		}

		public LayoutManagerForms GetLayoutManager() {
			return LayoutManager;
		}
	}
}
