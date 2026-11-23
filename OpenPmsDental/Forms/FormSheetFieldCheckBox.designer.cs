namespace OpenPmsDental{
	partial class FormSheetFieldCheckBox {
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
			this.label2 = new System.Windows.Forms.Label();
			this.listBoxFields = new OpenPmsDental.UI.ListBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.checkRequired = new OpenPmsDental.UI.CheckBox();
			this.groupRadioMisc = new OpenPmsDental.UI.GroupBox();
			this.comboRadioGroupNameMisc = new System.Windows.Forms.ComboBox();
			this.textUiLabelMobileMisc = new System.Windows.Forms.TextBox();
			this.labelUiLabelMobileMisc = new System.Windows.Forms.Label();
			this.textUiLabelMobileRadioButtonMisc = new System.Windows.Forms.TextBox();
			this.labelUiLabelMobileRadioButtonMisc = new System.Windows.Forms.Label();
			this.listRadio = new OpenPmsDental.UI.ListBox();
			this.groupRadio = new OpenPmsDental.UI.GroupBox();
			this.labelItemOverride = new System.Windows.Forms.Label();
			this.textItemOverride = new System.Windows.Forms.TextBox();
			this.labelAlsoActs = new System.Windows.Forms.Label();
			this.textUiLabelMobile = new System.Windows.Forms.TextBox();
			this.labelUiLabelMobile = new System.Windows.Forms.Label();
			this.labelTabOrder = new System.Windows.Forms.Label();
			this.listMedical = new OpenPmsDental.UI.ListBox();
			this.labelMedical = new System.Windows.Forms.Label();
			this.radioYes = new System.Windows.Forms.RadioButton();
			this.radioNo = new System.Windows.Forms.RadioButton();
			this.labelRequired = new System.Windows.Forms.Label();
			this.textReportableName = new System.Windows.Forms.TextBox();
			this.labelReportableName = new System.Windows.Forms.Label();
			this.labelMiscInstructions = new System.Windows.Forms.Label();
			this.labelYesNo = new System.Windows.Forms.Label();
			this.textTabOrder = new OpenPmsDental.ValidNum();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.textHeight = new OpenPmsDental.ValidNum();
			this.textWidth = new OpenPmsDental.ValidNum();
			this.textYPos = new OpenPmsDental.ValidNum();
			this.textXPos = new OpenPmsDental.ValidNum();
			this.butSave = new OpenPmsDental.UI.Button();
			this.textUiLabelMobileCheckBoxNonMisc = new System.Windows.Forms.TextBox();
			this.labelUiLabelMobileCheckBoxNonMisc = new System.Windows.Forms.Label();
			this.butAddAllergy = new OpenPmsDental.UI.Button();
			this.butAddProblem = new OpenPmsDental.UI.Button();
			this.butAddProc = new OpenPmsDental.UI.Button();
			this.textMobileMedicalNameOverride = new System.Windows.Forms.TextBox();
			this.labelMobileMedicalNameOverride = new System.Windows.Forms.Label();
			this.textMobileCheckOverride = new System.Windows.Forms.TextBox();
			this.labelMobileCheckOverride = new System.Windows.Forms.Label();
			this.groupRadioMisc.SuspendLayout();
			this.groupRadio.SuspendLayout();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(13, 20);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(108, 16);
			this.label2.TabIndex = 86;
			this.label2.Text = "Field Name";
			this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// listBoxFields
			// 
			this.listBoxFields.Location = new System.Drawing.Point(13, 38);
			this.listBoxFields.Name = "listBoxFields";
			this.listBoxFields.Size = new System.Drawing.Size(142, 511);
			this.listBoxFields.TabIndex = 11;
			this.listBoxFields.SelectedIndexChanged += new System.EventHandler(this.listFields_SelectedIndexChanged);
			this.listBoxFields.DoubleClick += new System.EventHandler(this.listFields_DoubleClick);
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label5.Location = new System.Drawing.Point(437, 8);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(40, 23);
			this.label5.TabIndex = 90;
			this.label5.Text = "X Pos";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label6.Location = new System.Drawing.Point(567, 8);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 23);
			this.label6.TabIndex = 92;
			this.label6.Text = "Y Pos";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label7.Location = new System.Drawing.Point(433, 35);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(44, 23);
			this.label7.TabIndex = 94;
			this.label7.Text = "Width";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label8.Location = new System.Drawing.Point(567, 35);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(48, 23);
			this.label8.TabIndex = 96;
			this.label8.Text = "Height";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(63, 23);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 23);
			this.label3.TabIndex = 103;
			this.label3.Text = "Group Name";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkRequired
			// 
			this.checkRequired.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRequired.Location = new System.Drawing.Point(483, 72);
			this.checkRequired.Name = "checkRequired";
			this.checkRequired.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkRequired.Size = new System.Drawing.Size(72, 20);
			this.checkRequired.TabIndex = 5;
			this.checkRequired.Text = "Required";
			// 
			// groupRadioMisc
			// 
			this.groupRadioMisc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupRadioMisc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(253)))), ((int)(((byte)(254)))));
			this.groupRadioMisc.Controls.Add(this.comboRadioGroupNameMisc);
			this.groupRadioMisc.Controls.Add(this.textUiLabelMobileMisc);
			this.groupRadioMisc.Controls.Add(this.labelUiLabelMobileMisc);
			this.groupRadioMisc.Controls.Add(this.textUiLabelMobileRadioButtonMisc);
			this.groupRadioMisc.Controls.Add(this.labelUiLabelMobileRadioButtonMisc);
			this.groupRadioMisc.Controls.Add(this.label3);
			this.groupRadioMisc.Location = new System.Drawing.Point(334, 290);
			this.groupRadioMisc.Name = "groupRadioMisc";
			this.groupRadioMisc.Size = new System.Drawing.Size(322, 108);
			this.groupRadioMisc.TabIndex = 4;
			this.groupRadioMisc.Text = "Radio Button";
			this.groupRadioMisc.Visible = false;
			// 
			// comboRadioGroupNameMisc
			// 
			this.comboRadioGroupNameMisc.Location = new System.Drawing.Point(149, 23);
			this.comboRadioGroupNameMisc.Name = "comboRadioGroupNameMisc";
			this.comboRadioGroupNameMisc.Size = new System.Drawing.Size(165, 23);
			this.comboRadioGroupNameMisc.TabIndex = 0;
			this.comboRadioGroupNameMisc.SelectedIndexChanged += new System.EventHandler(this.comboRadioGroupNameMisc_SelectedIndexChanged);
			// 
			// textUiLabelMobileMisc
			// 
			this.textUiLabelMobileMisc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textUiLabelMobileMisc.Location = new System.Drawing.Point(149, 50);
			this.textUiLabelMobileMisc.Name = "textUiLabelMobileMisc";
			this.textUiLabelMobileMisc.Size = new System.Drawing.Size(165, 23);
			this.textUiLabelMobileMisc.TabIndex = 1;
			// 
			// labelUiLabelMobileMisc
			// 
			this.labelUiLabelMobileMisc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelUiLabelMobileMisc.Location = new System.Drawing.Point(11, 50);
			this.labelUiLabelMobileMisc.Name = "labelUiLabelMobileMisc";
			this.labelUiLabelMobileMisc.Size = new System.Drawing.Size(132, 23);
			this.labelUiLabelMobileMisc.TabIndex = 120;
			this.labelUiLabelMobileMisc.Text = "Mobile Group Caption";
			this.labelUiLabelMobileMisc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textUiLabelMobileRadioButtonMisc
			// 
			this.textUiLabelMobileRadioButtonMisc.Location = new System.Drawing.Point(149, 77);
			this.textUiLabelMobileRadioButtonMisc.Name = "textUiLabelMobileRadioButtonMisc";
			this.textUiLabelMobileRadioButtonMisc.Size = new System.Drawing.Size(165, 23);
			this.textUiLabelMobileRadioButtonMisc.TabIndex = 2;
			// 
			// labelUiLabelMobileRadioButtonMisc
			// 
			this.labelUiLabelMobileRadioButtonMisc.Location = new System.Drawing.Point(23, 77);
			this.labelUiLabelMobileRadioButtonMisc.Name = "labelUiLabelMobileRadioButtonMisc";
			this.labelUiLabelMobileRadioButtonMisc.Size = new System.Drawing.Size(120, 23);
			this.labelUiLabelMobileRadioButtonMisc.TabIndex = 108;
			this.labelUiLabelMobileRadioButtonMisc.Text = "Mobile Item Caption";
			this.labelUiLabelMobileRadioButtonMisc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// listRadio
			// 
			this.listRadio.Location = new System.Drawing.Point(149, 50);
			this.listRadio.Name = "listRadio";
			this.listRadio.Size = new System.Drawing.Size(197, 56);
			this.listRadio.TabIndex = 0;
			this.listRadio.Click += new System.EventHandler(this.listRadio_Click);
			// 
			// groupRadio
			// 
			this.groupRadio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupRadio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(253)))), ((int)(((byte)(254)))));
			this.groupRadio.Controls.Add(this.labelItemOverride);
			this.groupRadio.Controls.Add(this.textItemOverride);
			this.groupRadio.Controls.Add(this.labelAlsoActs);
			this.groupRadio.Controls.Add(this.textUiLabelMobile);
			this.groupRadio.Controls.Add(this.labelUiLabelMobile);
			this.groupRadio.Controls.Add(this.listRadio);
			this.groupRadio.Location = new System.Drawing.Point(334, 437);
			this.groupRadio.Name = "groupRadio";
			this.groupRadio.Size = new System.Drawing.Size(354, 141);
			this.groupRadio.TabIndex = 8;
			this.groupRadio.Text = "Radio Button Value";
			this.groupRadio.Visible = false;
			// 
			// labelItemOverride
			// 
			this.labelItemOverride.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelItemOverride.Location = new System.Drawing.Point(15, 110);
			this.labelItemOverride.Name = "labelItemOverride";
			this.labelItemOverride.Size = new System.Drawing.Size(128, 23);
			this.labelItemOverride.TabIndex = 125;
			this.labelItemOverride.Text = "Item Caption Override";
			this.labelItemOverride.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textItemOverride
			// 
			this.textItemOverride.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textItemOverride.Location = new System.Drawing.Point(149, 110);
			this.textItemOverride.Name = "textItemOverride";
			this.textItemOverride.Size = new System.Drawing.Size(197, 20);
			this.textItemOverride.TabIndex = 124;
			// 
			// labelAlsoActs
			// 
			this.labelAlsoActs.Location = new System.Drawing.Point(35, 50);
			this.labelAlsoActs.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelAlsoActs.Name = "labelAlsoActs";
			this.labelAlsoActs.Size = new System.Drawing.Size(108, 41);
			this.labelAlsoActs.TabIndex = 123;
			this.labelAlsoActs.Text = "Also acts as the\r\nMobile Item Caption";
			// 
			// textUiLabelMobile
			// 
			this.textUiLabelMobile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textUiLabelMobile.Location = new System.Drawing.Point(149, 23);
			this.textUiLabelMobile.Name = "textUiLabelMobile";
			this.textUiLabelMobile.Size = new System.Drawing.Size(197, 20);
			this.textUiLabelMobile.TabIndex = 121;
			// 
			// labelUiLabelMobile
			// 
			this.labelUiLabelMobile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelUiLabelMobile.Location = new System.Drawing.Point(11, 23);
			this.labelUiLabelMobile.Name = "labelUiLabelMobile";
			this.labelUiLabelMobile.Size = new System.Drawing.Size(132, 23);
			this.labelUiLabelMobile.TabIndex = 122;
			this.labelUiLabelMobile.Text = "Mobile Group Caption";
			this.labelUiLabelMobile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelTabOrder
			// 
			this.labelTabOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelTabOrder.Location = new System.Drawing.Point(413, 259);
			this.labelTabOrder.Name = "labelTabOrder";
			this.labelTabOrder.Size = new System.Drawing.Size(64, 23);
			this.labelTabOrder.TabIndex = 108;
			this.labelTabOrder.Text = "Tab Order";
			this.labelTabOrder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// listMedical
			// 
			this.listMedical.Location = new System.Drawing.Point(183, 38);
			this.listMedical.Name = "listMedical";
			this.listMedical.Size = new System.Drawing.Size(142, 511);
			this.listMedical.TabIndex = 12;
			this.listMedical.Visible = false;
			this.listMedical.DoubleClick += new System.EventHandler(this.listMedical_DoubleClick);
			// 
			// labelMedical
			// 
			this.labelMedical.Location = new System.Drawing.Point(183, 20);
			this.labelMedical.Name = "labelMedical";
			this.labelMedical.Size = new System.Drawing.Size(108, 16);
			this.labelMedical.TabIndex = 111;
			this.labelMedical.Text = "labelMedical";
			this.labelMedical.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.labelMedical.Visible = false;
			// 
			// radioYes
			// 
			this.radioYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioYes.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.radioYes.Location = new System.Drawing.Point(395, 122);
			this.radioYes.Name = "radioYes";
			this.radioYes.Size = new System.Drawing.Size(42, 20);
			this.radioYes.TabIndex = 6;
			this.radioYes.TabStop = true;
			this.radioYes.Text = "Yes";
			this.radioYes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.radioYes.UseVisualStyleBackColor = true;
			this.radioYes.Visible = false;
			this.radioYes.Click += new System.EventHandler(this.radioYes_Click);
			// 
			// radioNo
			// 
			this.radioNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioNo.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.radioNo.Location = new System.Drawing.Point(395, 146);
			this.radioNo.Name = "radioNo";
			this.radioNo.Size = new System.Drawing.Size(41, 20);
			this.radioNo.TabIndex = 7;
			this.radioNo.TabStop = true;
			this.radioNo.Text = "No";
			this.radioNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.radioNo.UseVisualStyleBackColor = true;
			this.radioNo.Visible = false;
			this.radioNo.Click += new System.EventHandler(this.radioNo_Click);
			// 
			// labelRequired
			// 
			this.labelRequired.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelRequired.Location = new System.Drawing.Point(561, 62);
			this.labelRequired.Name = "labelRequired";
			this.labelRequired.Size = new System.Drawing.Size(188, 40);
			this.labelRequired.TabIndex = 116;
			this.labelRequired.Text = "Same for the whole group";
			// 
			// textReportableName
			// 
			this.textReportableName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textReportableName.Location = new System.Drawing.Point(483, 232);
			this.textReportableName.Name = "textReportableName";
			this.textReportableName.Size = new System.Drawing.Size(196, 23);
			this.textReportableName.TabIndex = 9;
			// 
			// labelReportableName
			// 
			this.labelReportableName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelReportableName.Location = new System.Drawing.Point(373, 232);
			this.labelReportableName.Name = "labelReportableName";
			this.labelReportableName.Size = new System.Drawing.Size(104, 23);
			this.labelReportableName.TabIndex = 108;
			this.labelReportableName.Text = "Reportable Name";
			this.labelReportableName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelMiscInstructions
			// 
			this.labelMiscInstructions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelMiscInstructions.Location = new System.Drawing.Point(370, 586);
			this.labelMiscInstructions.Name = "labelMiscInstructions";
			this.labelMiscInstructions.Size = new System.Drawing.Size(289, 32);
			this.labelMiscInstructions.TabIndex = 117;
			this.labelMiscInstructions.Text = "One Reportable Name per button";
			this.labelMiscInstructions.Visible = false;
			// 
			// labelYesNo
			// 
			this.labelYesNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelYesNo.Location = new System.Drawing.Point(462, 106);
			this.labelYesNo.Name = "labelYesNo";
			this.labelYesNo.Size = new System.Drawing.Size(197, 68);
			this.labelYesNo.TabIndex = 118;
			this.labelYesNo.Text = "Pair with a \"No\" box";
			this.labelYesNo.Visible = false;
			// 
			// textTabOrder
			// 
			this.textTabOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textTabOrder.Location = new System.Drawing.Point(483, 259);
			this.textTabOrder.MaxVal = 2000;
			this.textTabOrder.MinVal = -100;
			this.textTabOrder.Name = "textTabOrder";
			this.textTabOrder.Size = new System.Drawing.Size(68, 23);
			this.textTabOrder.TabIndex = 10;
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 664);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(77, 24);
			this.butDelete.TabIndex = 13;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// textHeight
			// 
			this.textHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textHeight.Location = new System.Drawing.Point(621, 35);
			this.textHeight.MaxVal = 2000;
			this.textHeight.MinVal = 1;
			this.textHeight.Name = "textHeight";
			this.textHeight.Size = new System.Drawing.Size(58, 23);
			this.textHeight.TabIndex = 3;
			// 
			// textWidth
			// 
			this.textWidth.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textWidth.Location = new System.Drawing.Point(483, 35);
			this.textWidth.MaxVal = 2000;
			this.textWidth.MinVal = 1;
			this.textWidth.Name = "textWidth";
			this.textWidth.Size = new System.Drawing.Size(68, 23);
			this.textWidth.TabIndex = 2;
			// 
			// textYPos
			// 
			this.textYPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textYPos.Location = new System.Drawing.Point(613, 8);
			this.textYPos.MaxVal = 2000;
			this.textYPos.MinVal = -100;
			this.textYPos.Name = "textYPos";
			this.textYPos.Size = new System.Drawing.Size(66, 23);
			this.textYPos.TabIndex = 1;
			// 
			// textXPos
			// 
			this.textXPos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textXPos.Location = new System.Drawing.Point(483, 8);
			this.textXPos.MaxVal = 2000;
			this.textXPos.MinVal = -100;
			this.textXPos.Name = "textXPos";
			this.textXPos.Size = new System.Drawing.Size(68, 23);
			this.textXPos.TabIndex = 0;
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(674, 664);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 14;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// textUiLabelMobileCheckBoxNonMisc
			// 
			this.textUiLabelMobileCheckBoxNonMisc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textUiLabelMobileCheckBoxNonMisc.Location = new System.Drawing.Point(483, 205);
			this.textUiLabelMobileCheckBoxNonMisc.Name = "textUiLabelMobileCheckBoxNonMisc";
			this.textUiLabelMobileCheckBoxNonMisc.Size = new System.Drawing.Size(196, 23);
			this.textUiLabelMobileCheckBoxNonMisc.TabIndex = 119;
			// 
			// labelUiLabelMobileCheckBoxNonMisc
			// 
			this.labelUiLabelMobileCheckBoxNonMisc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelUiLabelMobileCheckBoxNonMisc.Location = new System.Drawing.Point(357, 205);
			this.labelUiLabelMobileCheckBoxNonMisc.Name = "labelUiLabelMobileCheckBoxNonMisc";
			this.labelUiLabelMobileCheckBoxNonMisc.Size = new System.Drawing.Size(120, 23);
			this.labelUiLabelMobileCheckBoxNonMisc.TabIndex = 120;
			this.labelUiLabelMobileCheckBoxNonMisc.Text = "Mobile Item Caption";
			this.labelUiLabelMobileCheckBoxNonMisc.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butAddAllergy
			// 
			this.butAddAllergy.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAddAllergy.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAddAllergy.Location = new System.Drawing.Point(183, 553);
			this.butAddAllergy.Name = "butAddAllergy";
			this.butAddAllergy.Size = new System.Drawing.Size(105, 24);
			this.butAddAllergy.TabIndex = 121;
			this.butAddAllergy.Text = "Add Allergy";
			this.butAddAllergy.Visible = false;
			this.butAddAllergy.Click += new System.EventHandler(this.butAddAllergy_Click);
			// 
			// butAddProblem
			// 
			this.butAddProblem.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAddProblem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAddProblem.Location = new System.Drawing.Point(186, 577);
			this.butAddProblem.Name = "butAddProblem";
			this.butAddProblem.Size = new System.Drawing.Size(105, 24);
			this.butAddProblem.TabIndex = 122;
			this.butAddProblem.Text = "Add Problem";
			this.butAddProblem.Visible = false;
			this.butAddProblem.Click += new System.EventHandler(this.butAddProblem_Click);
			// 
			// butAddProc
			// 
			this.butAddProc.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAddProc.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAddProc.Location = new System.Drawing.Point(13, 553);
			this.butAddProc.Name = "butAddProc";
			this.butAddProc.Size = new System.Drawing.Size(124, 24);
			this.butAddProc.TabIndex = 123;
			this.butAddProc.Text = "Add Procedure";
			this.butAddProc.Visible = false;
			this.butAddProc.Click += new System.EventHandler(this.butAddProc_Click);
			// 
			// textMobileMedicalNameOverride
			// 
			this.textMobileMedicalNameOverride.Location = new System.Drawing.Point(483, 178);
			this.textMobileMedicalNameOverride.Name = "textMobileMedicalNameOverride";
			this.textMobileMedicalNameOverride.Size = new System.Drawing.Size(196, 23);
			this.textMobileMedicalNameOverride.TabIndex = 131;
			// 
			// labelMobileMedicalNameOverride
			// 
			this.labelMobileMedicalNameOverride.Location = new System.Drawing.Point(345, 178);
			this.labelMobileMedicalNameOverride.Name = "labelMobileMedicalNameOverride";
			this.labelMobileMedicalNameOverride.Size = new System.Drawing.Size(132, 23);
			this.labelMobileMedicalNameOverride.TabIndex = 130;
			this.labelMobileMedicalNameOverride.Text = "Mobile Group Caption";
			this.labelMobileMedicalNameOverride.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textMobileCheckOverride
			// 
			this.textMobileCheckOverride.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textMobileCheckOverride.Location = new System.Drawing.Point(483, 406);
			this.textMobileCheckOverride.MaxLength = 65535;
			this.textMobileCheckOverride.Name = "textMobileCheckOverride";
			this.textMobileCheckOverride.Size = new System.Drawing.Size(192, 23);
			this.textMobileCheckOverride.TabIndex = 128;
			this.textMobileCheckOverride.Visible = false;
			// 
			// labelMobileCheckOverride
			// 
			this.labelMobileCheckOverride.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelMobileCheckOverride.Location = new System.Drawing.Point(357, 406);
			this.labelMobileCheckOverride.Name = "labelMobileCheckOverride";
			this.labelMobileCheckOverride.Size = new System.Drawing.Size(120, 23);
			this.labelMobileCheckOverride.TabIndex = 129;
			this.labelMobileCheckOverride.Text = "Mobile Y/N Override";
			this.labelMobileCheckOverride.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelMobileCheckOverride.Visible = false;
			// 
			// FormSheetFieldCheckBox
			// 
			this.ClientSize = new System.Drawing.Size(757, 696);
			this.Controls.Add(this.textMobileMedicalNameOverride);
			this.Controls.Add(this.labelMobileMedicalNameOverride);
			this.Controls.Add(this.textMobileCheckOverride);
			this.Controls.Add(this.labelMobileCheckOverride);
			this.Controls.Add(this.butAddProc);
			this.Controls.Add(this.butAddProblem);
			this.Controls.Add(this.butAddAllergy);
			this.Controls.Add(this.textUiLabelMobileCheckBoxNonMisc);
			this.Controls.Add(this.labelUiLabelMobileCheckBoxNonMisc);
			this.Controls.Add(this.labelYesNo);
			this.Controls.Add(this.listMedical);
			this.Controls.Add(this.labelMiscInstructions);
			this.Controls.Add(this.textReportableName);
			this.Controls.Add(this.labelReportableName);
			this.Controls.Add(this.labelRequired);
			this.Controls.Add(this.radioNo);
			this.Controls.Add(this.radioYes);
			this.Controls.Add(this.labelMedical);
			this.Controls.Add(this.textTabOrder);
			this.Controls.Add(this.labelTabOrder);
			this.Controls.Add(this.checkRequired);
			this.Controls.Add(this.groupRadioMisc);
			this.Controls.Add(this.groupRadio);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.textHeight);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textWidth);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.textYPos);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.textXPos);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.listBoxFields);
			this.Controls.Add(this.butSave);
			this.Name = "FormSheetFieldCheckBox";
			this.Text = "Edit CheckBox";
			this.Load += new System.EventHandler(this.FormSheetFieldCheckBox_Load);
			this.Shown += new System.EventHandler(this.FormSheetFieldCheckBox_Shown);
			this.groupRadioMisc.ResumeLayout(false);
			this.groupRadioMisc.PerformLayout();
			this.groupRadio.ResumeLayout(false);
			this.groupRadio.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label2;
		private OpenPmsDental.UI.ListBox listBoxFields;
		private System.Windows.Forms.Label label5;
		private ValidNum textXPos;
		private ValidNum textYPos;
		private System.Windows.Forms.Label label6;
		private ValidNum textWidth;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private ValidNum textHeight;
		private UI.Button butDelete;
		private System.Windows.Forms.Label label3;
		private OpenPmsDental.UI.CheckBox checkRequired;
		private OpenPmsDental.UI.GroupBox groupRadioMisc;
		private OpenPmsDental.UI.ListBox listRadio;
		private OpenPmsDental.UI.GroupBox groupRadio;
		private ValidNum textTabOrder;
		private System.Windows.Forms.Label labelTabOrder;
		private OpenPmsDental.UI.ListBox listMedical;
		private System.Windows.Forms.Label labelMedical;
		private System.Windows.Forms.RadioButton radioYes;
		private System.Windows.Forms.RadioButton radioNo;
		private System.Windows.Forms.Label labelRequired;
		private System.Windows.Forms.TextBox textReportableName;
		private System.Windows.Forms.Label labelReportableName;
		private System.Windows.Forms.Label labelMiscInstructions;
		private System.Windows.Forms.Label labelYesNo;
		private System.Windows.Forms.TextBox textUiLabelMobileMisc;
		private System.Windows.Forms.Label labelUiLabelMobileMisc;
		private System.Windows.Forms.TextBox textUiLabelMobileRadioButtonMisc;
		private System.Windows.Forms.Label labelUiLabelMobileRadioButtonMisc;
		private System.Windows.Forms.TextBox textUiLabelMobile;
		private System.Windows.Forms.Label labelUiLabelMobile;
		private System.Windows.Forms.Label labelAlsoActs;
		private System.Windows.Forms.ComboBox comboRadioGroupNameMisc;
		private System.Windows.Forms.TextBox textUiLabelMobileCheckBoxNonMisc;
		private System.Windows.Forms.Label labelUiLabelMobileCheckBoxNonMisc;
		private UI.Button butAddAllergy;
		private UI.Button butAddProblem;
		private UI.Button butAddProc;
		private System.Windows.Forms.TextBox textMobileMedicalNameOverride;
		private System.Windows.Forms.Label labelMobileMedicalNameOverride;
		private System.Windows.Forms.TextBox textMobileCheckOverride;
		private System.Windows.Forms.Label labelMobileCheckOverride;
		private System.Windows.Forms.TextBox textItemOverride;
		private System.Windows.Forms.Label labelItemOverride;
	}
}