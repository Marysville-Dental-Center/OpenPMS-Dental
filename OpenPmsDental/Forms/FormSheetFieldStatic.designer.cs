namespace OpenPmsDental{
	partial class FormSheetFieldStatic {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.checkPmtOpt = new OpenPmsDental.UI.CheckBox();
			this.labelTextW = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.listFields = new OpenPmsDental.UI.ListBox();
			this.textFieldValue = new System.Windows.Forms.TextBox();
			this.comboGrowthBehavior = new OpenPmsDental.UI.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.textFontSize = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.butColor = new System.Windows.Forms.Button();
			this.comboTextAlign = new OpenPmsDental.UI.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.checkFontIsBold = new OpenPmsDental.UI.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.comboFontName = new OpenPmsDental.UI.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.checkIsLocked = new OpenPmsDental.UI.CheckBox();
			this.butExamSheet = new OpenPmsDental.UI.Button();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.textHeight = new OpenPmsDental.ValidNum();
			this.textWidth = new OpenPmsDental.ValidNum();
			this.textYPos = new OpenPmsDental.ValidNum();
			this.textXPos = new OpenPmsDental.ValidNum();
			this.butSave = new OpenPmsDental.UI.Button();
			this.labelTextH = new System.Windows.Forms.Label();
			this.checkIncludeInMobile = new OpenPmsDental.UI.CheckBox();
			this.butCalcWidth = new OpenPmsDental.UI.Button();
			this.butCalcHeight = new OpenPmsDental.UI.Button();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkPmtOpt
			// 
			this.checkPmtOpt.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPmtOpt.Location = new System.Drawing.Point(126, 617);
			this.checkPmtOpt.Name = "checkPmtOpt";
			this.checkPmtOpt.Size = new System.Drawing.Size(123, 20);
			this.checkPmtOpt.TabIndex = 236;
			this.checkPmtOpt.Text = "Is Payment Option";
			// 
			// labelTextW
			// 
			this.labelTextW.Location = new System.Drawing.Point(231, 563);
			this.labelTextW.Name = "labelTextW";
			this.labelTextW.Size = new System.Drawing.Size(52, 23);
			this.labelTextW.TabIndex = 104;
			this.labelTextW.Text = "TextW: ";
			this.labelTextW.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(518, 319);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(131, 62);
			this.label1.TabIndex = 103;
			this.label1.Text = "Click on the list to the right to insert a Field into the main text above";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// listFields
			// 
			this.listFields.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.listFields.IntegralHeight = false;
			this.listFields.Location = new System.Drawing.Point(657, 8);
			this.listFields.Name = "listFields";
			this.listFields.Size = new System.Drawing.Size(201, 653);
			this.listFields.TabIndex = 0;
			this.listFields.TabStop = false;
			this.listFields.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ListFields_MouseDown);
			// 
			// textFieldValue
			// 
			this.textFieldValue.AcceptsReturn = true;
			this.textFieldValue.AcceptsTab = true;
			this.textFieldValue.Location = new System.Drawing.Point(8, 8);
			this.textFieldValue.Multiline = true;
			this.textFieldValue.Name = "textFieldValue";
			this.textFieldValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textFieldValue.Size = new System.Drawing.Size(641, 307);
			this.textFieldValue.TabIndex = 102;
			this.textFieldValue.TextChanged += new System.EventHandler(this.UpdateTextSizeLabels);
			this.textFieldValue.Leave += new System.EventHandler(this.textFieldValue_Leave);
			// 
			// comboGrowthBehavior
			// 
			this.comboGrowthBehavior.Location = new System.Drawing.Point(126, 482);
			this.comboGrowthBehavior.Name = "comboGrowthBehavior";
			this.comboGrowthBehavior.Size = new System.Drawing.Size(196, 23);
			this.comboGrowthBehavior.TabIndex = 87;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(20, 482);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(100, 23);
			this.label9.TabIndex = 98;
			this.label9.Text = "Growth Behavior";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(72, 590);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(48, 23);
			this.label8.TabIndex = 96;
			this.label8.Text = "Height";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(76, 563);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(44, 23);
			this.label7.TabIndex = 94;
			this.label7.Text = "Width";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(80, 536);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 23);
			this.label6.TabIndex = 92;
			this.label6.Text = "Y Pos";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(80, 509);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(40, 23);
			this.label5.TabIndex = 90;
			this.label5.Text = "X Pos";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.textFontSize);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.butColor);
			this.groupBox1.Controls.Add(this.comboTextAlign);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.checkFontIsBold);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.comboFontName);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Location = new System.Drawing.Point(43, 319);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(287, 159);
			this.groupBox1.TabIndex = 85;
			this.groupBox1.Text = "Font";
			// 
			// textFontSize
			// 
			this.textFontSize.Location = new System.Drawing.Point(83, 50);
			this.textFontSize.Name = "textFontSize";
			this.textFontSize.Size = new System.Drawing.Size(44, 20);
			this.textFontSize.TabIndex = 241;
			this.textFontSize.TextChanged += new System.EventHandler(this.UpdateTextSizeLabels);
			// 
			// label10
			// 
			this.label10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.label10.Location = new System.Drawing.Point(37, 101);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(40, 23);
			this.label10.TabIndex = 240;
			this.label10.Text = "Color";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butColor
			// 
			this.butColor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.butColor.Location = new System.Drawing.Point(83, 101);
			this.butColor.Name = "butColor";
			this.butColor.Size = new System.Drawing.Size(30, 23);
			this.butColor.TabIndex = 103;
			this.butColor.Click += new System.EventHandler(this.butColor_Click);
			// 
			// comboTextAlign
			// 
			this.comboTextAlign.Location = new System.Drawing.Point(83, 128);
			this.comboTextAlign.Name = "comboTextAlign";
			this.comboTextAlign.Size = new System.Drawing.Size(196, 23);
			this.comboTextAlign.TabIndex = 107;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(37, 128);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 23);
			this.label2.TabIndex = 106;
			this.label2.Text = "Align";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkFontIsBold
			// 
			this.checkFontIsBold.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkFontIsBold.Location = new System.Drawing.Point(83, 77);
			this.checkFontIsBold.Name = "checkFontIsBold";
			this.checkFontIsBold.Size = new System.Drawing.Size(47, 20);
			this.checkFontIsBold.TabIndex = 90;
			this.checkFontIsBold.Text = "Bold";
			this.checkFontIsBold.CheckedChanged += new System.EventHandler(this.UpdateTextSizeLabels);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(45, 50);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(32, 23);
			this.label4.TabIndex = 89;
			this.label4.Text = "Size";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboFontName
			// 
			this.comboFontName.Location = new System.Drawing.Point(83, 23);
			this.comboFontName.Name = "comboFontName";
			this.comboFontName.Size = new System.Drawing.Size(196, 23);
			this.comboFontName.TabIndex = 88;
			this.comboFontName.SelectedIndexChanged += new System.EventHandler(this.UpdateTextSizeLabels);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(33, 23);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(44, 23);
			this.label3.TabIndex = 87;
			this.label3.Text = "Name";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkIsLocked
			// 
			this.checkIsLocked.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIsLocked.Location = new System.Drawing.Point(249, 641);
			this.checkIsLocked.Name = "checkIsLocked";
			this.checkIsLocked.Size = new System.Drawing.Size(115, 20);
			this.checkIsLocked.TabIndex = 237;
			this.checkIsLocked.Text = "Lock Text Editing";
			// 
			// butExamSheet
			// 
			this.butExamSheet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.butExamSheet.Location = new System.Drawing.Point(336, 319);
			this.butExamSheet.Name = "butExamSheet";
			this.butExamSheet.Size = new System.Drawing.Size(98, 24);
			this.butExamSheet.TabIndex = 105;
			this.butExamSheet.Text = "Exam Sheet Field";
			this.butExamSheet.Click += new System.EventHandler(this.butExamSheet_Click);
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 673);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(77, 24);
			this.butDelete.TabIndex = 100;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// textHeight
			// 
			this.textHeight.Location = new System.Drawing.Point(126, 590);
			this.textHeight.MaxVal = 2000;
			this.textHeight.MinVal = 1;
			this.textHeight.Name = "textHeight";
			this.textHeight.Size = new System.Drawing.Size(68, 23);
			this.textHeight.TabIndex = 97;
			// 
			// textWidth
			// 
			this.textWidth.Location = new System.Drawing.Point(126, 563);
			this.textWidth.MaxVal = 2000;
			this.textWidth.MinVal = 1;
			this.textWidth.Name = "textWidth";
			this.textWidth.Size = new System.Drawing.Size(68, 23);
			this.textWidth.TabIndex = 95;
			// 
			// textYPos
			// 
			this.textYPos.Location = new System.Drawing.Point(126, 536);
			this.textYPos.MaxVal = 2000;
			this.textYPos.MinVal = -100;
			this.textYPos.Name = "textYPos";
			this.textYPos.Size = new System.Drawing.Size(68, 23);
			this.textYPos.TabIndex = 93;
			// 
			// textXPos
			// 
			this.textXPos.Location = new System.Drawing.Point(126, 509);
			this.textXPos.MaxVal = 2000;
			this.textXPos.MinVal = -100;
			this.textXPos.Name = "textXPos";
			this.textXPos.Size = new System.Drawing.Size(68, 23);
			this.textXPos.TabIndex = 91;
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(783, 673);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// labelTextH
			// 
			this.labelTextH.Location = new System.Drawing.Point(235, 590);
			this.labelTextH.Name = "labelTextH";
			this.labelTextH.Size = new System.Drawing.Size(48, 23);
			this.labelTextH.TabIndex = 238;
			this.labelTextH.Text = "TextH: ";
			this.labelTextH.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkIncludeInMobile
			// 
			this.checkIncludeInMobile.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIncludeInMobile.Location = new System.Drawing.Point(126, 641);
			this.checkIncludeInMobile.Name = "checkIncludeInMobile";
			this.checkIncludeInMobile.Size = new System.Drawing.Size(117, 20);
			this.checkIncludeInMobile.TabIndex = 239;
			this.checkIncludeInMobile.Text = "Include In Mobile";
			// 
			// butCalcWidth
			// 
			this.butCalcWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butCalcWidth.Location = new System.Drawing.Point(289, 563);
			this.butCalcWidth.Name = "butCalcWidth";
			this.butCalcWidth.Size = new System.Drawing.Size(98, 23);
			this.butCalcWidth.TabIndex = 240;
			this.butCalcWidth.Text = "Calculate Width";
			this.butCalcWidth.Click += new System.EventHandler(this.butCalcWidth_Click);
			// 
			// butCalcHeight
			// 
			this.butCalcHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butCalcHeight.Location = new System.Drawing.Point(289, 590);
			this.butCalcHeight.Name = "butCalcHeight";
			this.butCalcHeight.Size = new System.Drawing.Size(98, 23);
			this.butCalcHeight.TabIndex = 241;
			this.butCalcHeight.Text = "Calculate Height";
			this.butCalcHeight.Click += new System.EventHandler(this.butCalcHeight_Click);
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(393, 565);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(86, 18);
			this.label11.TabIndex = 242;
			this.label11.Text = "from Height";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(393, 593);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(86, 16);
			this.label12.TabIndex = 243;
			this.label12.Text = "from Width";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// FormSheetFieldStatic
			// 
			this.ClientSize = new System.Drawing.Size(866, 705);
			this.Controls.Add(this.label12);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.butCalcHeight);
			this.Controls.Add(this.butCalcWidth);
			this.Controls.Add(this.checkIncludeInMobile);
			this.Controls.Add(this.labelTextH);
			this.Controls.Add(this.checkIsLocked);
			this.Controls.Add(this.checkPmtOpt);
			this.Controls.Add(this.butExamSheet);
			this.Controls.Add(this.labelTextW);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.listFields);
			this.Controls.Add(this.textFieldValue);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.comboGrowthBehavior);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.textHeight);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textWidth);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.textYPos);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.textXPos);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.butSave);
			this.Name = "FormSheetFieldStatic";
			this.Text = "Edit Static Text Field";
			this.Load += new System.EventHandler(this.FormSheetFieldStatic_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private System.Windows.Forms.Label label3;
		private OpenPmsDental.UI.ComboBox comboFontName;
		private OpenPmsDental.UI.CheckBox checkFontIsBold;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private ValidNum textXPos;
		private ValidNum textYPos;
		private System.Windows.Forms.Label label6;
		private ValidNum textWidth;
		private System.Windows.Forms.Label label7;
		private ValidNum textHeight;
		private System.Windows.Forms.Label label8;
		private UI.ComboBox comboGrowthBehavior;
		private System.Windows.Forms.Label label9;
		private OpenPmsDental.UI.Button butDelete;
		private System.Windows.Forms.TextBox textFieldValue;
		private OpenPmsDental.UI.ListBox listFields;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelTextW;
		private UI.Button butExamSheet;
		private OpenPmsDental.UI.ComboBox comboTextAlign;
		private System.Windows.Forms.Label label2;
		private OpenPmsDental.UI.CheckBox checkPmtOpt;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Button butColor;
		private OpenPmsDental.UI.CheckBox checkIsLocked;
		private System.Windows.Forms.Label labelTextH;
		private OpenPmsDental.UI.CheckBox checkIncludeInMobile;
		private System.Windows.Forms.TextBox textFontSize;
		private UI.Button butCalcWidth;
		private UI.Button butCalcHeight;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
	}
}