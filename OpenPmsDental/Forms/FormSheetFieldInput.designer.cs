namespace OpenPmsDental{
	partial class FormSheetFieldInput {
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
			this.textUiLabelMobile = new System.Windows.Forms.TextBox();
			this.labelUiLabelMobile = new System.Windows.Forms.Label();
			this.textReportableName = new System.Windows.Forms.TextBox();
			this.labelReportableName = new System.Windows.Forms.Label();
			this.textTabOrder = new OpenPmsDental.ValidNum();
			this.labelTabOrder = new System.Windows.Forms.Label();
			this.checkRequired = new OpenPmsDental.UI.CheckBox();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.comboGrowthBehavior = new OpenPmsDental.UI.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.textHeight = new OpenPmsDental.ValidNum();
			this.label8 = new System.Windows.Forms.Label();
			this.textWidth = new OpenPmsDental.ValidNum();
			this.label7 = new System.Windows.Forms.Label();
			this.textYPos = new OpenPmsDental.ValidNum();
			this.label6 = new System.Windows.Forms.Label();
			this.textXPos = new OpenPmsDental.ValidNum();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.checkFontIsBold = new OpenPmsDental.UI.CheckBox();
			this.textFontSize = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.comboFontName = new OpenPmsDental.UI.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.listFields = new OpenPmsDental.UI.ListBox();
			this.butSave = new OpenPmsDental.UI.Button();
			this.butAutoNote = new OpenPmsDental.UI.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.butClearAutoNote = new OpenPmsDental.UI.Button();
			this.textAutoNote = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textUiLabelMobile
			// 
			this.textUiLabelMobile.Location = new System.Drawing.Point(269, 35);
			this.textUiLabelMobile.MaxLength = 0;
			this.textUiLabelMobile.Name = "textUiLabelMobile";
			this.textUiLabelMobile.Size = new System.Drawing.Size(196, 23);
			this.textUiLabelMobile.TabIndex = 106;
			// 
			// labelUiLabelMobile
			// 
			this.labelUiLabelMobile.Location = new System.Drawing.Point(167, 35);
			this.labelUiLabelMobile.Name = "labelUiLabelMobile";
			this.labelUiLabelMobile.Size = new System.Drawing.Size(96, 23);
			this.labelUiLabelMobile.TabIndex = 107;
			this.labelUiLabelMobile.Text = "Mobile Caption";
			this.labelUiLabelMobile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textReportableName
			// 
			this.textReportableName.Location = new System.Drawing.Point(269, 8);
			this.textReportableName.Name = "textReportableName";
			this.textReportableName.Size = new System.Drawing.Size(196, 23);
			this.textReportableName.TabIndex = 104;
			// 
			// labelReportableName
			// 
			this.labelReportableName.Location = new System.Drawing.Point(159, 8);
			this.labelReportableName.Name = "labelReportableName";
			this.labelReportableName.Size = new System.Drawing.Size(104, 23);
			this.labelReportableName.TabIndex = 105;
			this.labelReportableName.Text = "Reportable Name";
			this.labelReportableName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textTabOrder
			// 
			this.textTabOrder.Location = new System.Drawing.Point(269, 338);
			this.textTabOrder.MaxVal = 2000;
			this.textTabOrder.MinVal = -100;
			this.textTabOrder.Name = "textTabOrder";
			this.textTabOrder.Size = new System.Drawing.Size(68, 23);
			this.textTabOrder.TabIndex = 103;
			// 
			// labelTabOrder
			// 
			this.labelTabOrder.Location = new System.Drawing.Point(199, 338);
			this.labelTabOrder.Name = "labelTabOrder";
			this.labelTabOrder.Size = new System.Drawing.Size(64, 23);
			this.labelTabOrder.TabIndex = 102;
			this.labelTabOrder.Text = "Tab Order";
			this.labelTabOrder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkRequired
			// 
			this.checkRequired.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRequired.Location = new System.Drawing.Point(269, 314);
			this.checkRequired.Name = "checkRequired";
			this.checkRequired.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkRequired.Size = new System.Drawing.Size(72, 20);
			this.checkRequired.TabIndex = 101;
			this.checkRequired.Text = "Required";
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 405);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(77, 24);
			this.butDelete.TabIndex = 100;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// comboGrowthBehavior
			// 
			this.comboGrowthBehavior.Location = new System.Drawing.Point(269, 179);
			this.comboGrowthBehavior.Name = "comboGrowthBehavior";
			this.comboGrowthBehavior.Size = new System.Drawing.Size(196, 23);
			this.comboGrowthBehavior.TabIndex = 99;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(163, 179);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(100, 23);
			this.label9.TabIndex = 98;
			this.label9.Text = "Growth Behavior";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textHeight
			// 
			this.textHeight.Location = new System.Drawing.Point(269, 287);
			this.textHeight.MaxVal = 2000;
			this.textHeight.MinVal = 1;
			this.textHeight.Name = "textHeight";
			this.textHeight.Size = new System.Drawing.Size(68, 23);
			this.textHeight.TabIndex = 97;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(215, 287);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(48, 23);
			this.label8.TabIndex = 96;
			this.label8.Text = "Height";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textWidth
			// 
			this.textWidth.Location = new System.Drawing.Point(269, 260);
			this.textWidth.MaxVal = 2000;
			this.textWidth.MinVal = 1;
			this.textWidth.Name = "textWidth";
			this.textWidth.Size = new System.Drawing.Size(68, 23);
			this.textWidth.TabIndex = 95;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(219, 260);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(44, 23);
			this.label7.TabIndex = 94;
			this.label7.Text = "Width";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textYPos
			// 
			this.textYPos.Location = new System.Drawing.Point(269, 233);
			this.textYPos.MaxVal = 2000;
			this.textYPos.MinVal = -100;
			this.textYPos.Name = "textYPos";
			this.textYPos.Size = new System.Drawing.Size(68, 23);
			this.textYPos.TabIndex = 93;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(223, 233);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 23);
			this.label6.TabIndex = 92;
			this.label6.Text = "Y Pos";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textXPos
			// 
			this.textXPos.Location = new System.Drawing.Point(269, 206);
			this.textXPos.MaxVal = 2000;
			this.textXPos.MinVal = -100;
			this.textXPos.Name = "textXPos";
			this.textXPos.Size = new System.Drawing.Size(68, 23);
			this.textXPos.TabIndex = 91;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(223, 206);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(40, 23);
			this.label5.TabIndex = 90;
			this.label5.Text = "X Pos";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.checkFontIsBold);
			this.groupBox1.Controls.Add(this.textFontSize);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.comboFontName);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Location = new System.Drawing.Point(182, 66);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(283, 105);
			this.groupBox1.TabIndex = 88;
			this.groupBox1.Text = "Font";
			// 
			// checkFontIsBold
			// 
			this.checkFontIsBold.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkFontIsBold.Location = new System.Drawing.Point(87, 77);
			this.checkFontIsBold.Name = "checkFontIsBold";
			this.checkFontIsBold.Size = new System.Drawing.Size(47, 20);
			this.checkFontIsBold.TabIndex = 90;
			this.checkFontIsBold.Text = "Bold";
			// 
			// textFontSize
			// 
			this.textFontSize.Location = new System.Drawing.Point(87, 50);
			this.textFontSize.Name = "textFontSize";
			this.textFontSize.Size = new System.Drawing.Size(44, 20);
			this.textFontSize.TabIndex = 89;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(49, 50);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(32, 23);
			this.label4.TabIndex = 89;
			this.label4.Text = "Size";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboFontName
			// 
			this.comboFontName.Location = new System.Drawing.Point(87, 23);
			this.comboFontName.Name = "comboFontName";
			this.comboFontName.Size = new System.Drawing.Size(188, 23);
			this.comboFontName.TabIndex = 88;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(37, 23);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(44, 23);
			this.label3.TabIndex = 87;
			this.label3.Text = "Name";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(13, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(108, 16);
			this.label2.TabIndex = 86;
			this.label2.Text = "Field Name";
			this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// listFields
			// 
			this.listFields.Location = new System.Drawing.Point(13, 26);
			this.listFields.Name = "listFields";
			this.listFields.Size = new System.Drawing.Size(142, 352);
			this.listFields.TabIndex = 85;
			this.listFields.SelectionChangeCommitted += new System.EventHandler(this.listFields_SelectionChangeCommitted);
			this.listFields.DoubleClick += new System.EventHandler(this.listFields_DoubleClick);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(488, 405);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// butAutoNote
			// 
			this.butAutoNote.Location = new System.Drawing.Point(470, 340);
			this.butAutoNote.Name = "butAutoNote";
			this.butAutoNote.Size = new System.Drawing.Size(25, 24);
			this.butAutoNote.TabIndex = 108;
			this.butAutoNote.Text = "...";
			this.butAutoNote.Click += new System.EventHandler(this.butAutoNote_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(195, 365);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(68, 23);
			this.label1.TabIndex = 110;
			this.label1.Text = "Auto Note";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butClearAutoNote
			// 
			this.butClearAutoNote.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butClearAutoNote.Location = new System.Drawing.Point(500, 340);
			this.butClearAutoNote.Name = "butClearAutoNote";
			this.butClearAutoNote.Size = new System.Drawing.Size(24, 24);
			this.butClearAutoNote.TabIndex = 111;
			this.butClearAutoNote.Click += new System.EventHandler(this.butClearAutoNote_Click);
			// 
			// textAutoNote
			// 
			this.textAutoNote.Location = new System.Drawing.Point(269, 365);
			this.textAutoNote.MaxLength = 0;
			this.textAutoNote.Name = "textAutoNote";
			this.textAutoNote.ReadOnly = true;
			this.textAutoNote.Size = new System.Drawing.Size(196, 23);
			this.textAutoNote.TabIndex = 112;
			// 
			// FormSheetFieldInput
			// 
			this.ClientSize = new System.Drawing.Size(571, 437);
			this.Controls.Add(this.textAutoNote);
			this.Controls.Add(this.butClearAutoNote);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butAutoNote);
			this.Controls.Add(this.textUiLabelMobile);
			this.Controls.Add(this.labelUiLabelMobile);
			this.Controls.Add(this.textReportableName);
			this.Controls.Add(this.labelReportableName);
			this.Controls.Add(this.textTabOrder);
			this.Controls.Add(this.labelTabOrder);
			this.Controls.Add(this.checkRequired);
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
			this.Controls.Add(this.label2);
			this.Controls.Add(this.listFields);
			this.Controls.Add(this.butSave);
			this.Name = "FormSheetFieldInput";
			this.Text = "Edit Input Field";
			this.Load += new System.EventHandler(this.FormSheetFieldInput_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label2;
		private OpenPmsDental.UI.ListBox listFields;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private System.Windows.Forms.Label label3;
		private OpenPmsDental.UI.ComboBox comboFontName;
		private OpenPmsDental.UI.CheckBox checkFontIsBold;
		private System.Windows.Forms.TextBox textFontSize;
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
		private OpenPmsDental.UI.CheckBox checkRequired;
		private System.Windows.Forms.Label labelTabOrder;
		private ValidNum textTabOrder;
		private System.Windows.Forms.TextBox textReportableName;
		private System.Windows.Forms.Label labelReportableName;
		private System.Windows.Forms.TextBox textUiLabelMobile;
		private System.Windows.Forms.Label labelUiLabelMobile;
		private UI.Button butAutoNote;
		private System.Windows.Forms.Label label1;
		private UI.Button butClearAutoNote;
		private System.Windows.Forms.TextBox textAutoNote;
	}
}