using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormRpLaserLabels {
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

		private void InitializeComponent() {
			this.tabCustomLabels = new OpenPmsDental.UI.TabPage();
			this.butCusRemove = new OpenPmsDental.UI.Button();
			this.butCusAdd = new OpenPmsDental.UI.Button();
			this.listCusNames = new OpenPmsDental.UI.ListBox();
			this.numericCusCount = new System.Windows.Forms.NumericUpDown();
			this.textCusZip = new System.Windows.Forms.TextBox();
			this.textCusState = new System.Windows.Forms.TextBox();
			this.textCusCity = new System.Windows.Forms.TextBox();
			this.textCusAddr2 = new System.Windows.Forms.TextBox();
			this.textCusAddr1 = new System.Windows.Forms.TextBox();
			this.textCusName = new System.Windows.Forms.TextBox();
			this.labLabelCount = new System.Windows.Forms.Label();
			this.labZip = new System.Windows.Forms.Label();
			this.labState = new System.Windows.Forms.Label();
			this.labCity = new System.Windows.Forms.Label();
			this.labCustAddr2 = new System.Windows.Forms.Label();
			this.labCustAddr1 = new System.Windows.Forms.Label();
			this.labCustName = new System.Windows.Forms.Label();
			this.tabInsCo = new OpenPmsDental.UI.TabPage();
			this.butInsCoEnd = new OpenPmsDental.UI.Button();
			this.butInsCoStart = new OpenPmsDental.UI.Button();
			this.labInsCoEndAddr = new System.Windows.Forms.Label();
			this.labInsCoStartAddr = new System.Windows.Forms.Label();
			this.labInsCoSingle = new System.Windows.Forms.Label();
			this.numericInsCoSingle = new System.Windows.Forms.NumericUpDown();
			this.labInsCoEnd = new System.Windows.Forms.Label();
			this.textInsCoEnd = new System.Windows.Forms.TextBox();
			this.labInsCoStart = new System.Windows.Forms.Label();
			this.textInsCoStart = new System.Windows.Forms.TextBox();
			this.radioButRange = new System.Windows.Forms.RadioButton();
			this.radioButSingle = new System.Windows.Forms.RadioButton();
			this.tabPage1 = new OpenPmsDental.UI.TabPage();
			this.labEndingName = new System.Windows.Forms.Label();
			this.listProviders = new OpenPmsDental.UI.ListBox();
			this.checkAllProviders = new OpenPmsDental.UI.CheckBox();
			this.checkActiveOnly = new OpenPmsDental.UI.CheckBox();
			this.checkGroupByFamily = new OpenPmsDental.UI.CheckBox();
			this.butEndName = new OpenPmsDental.UI.Button();
			this.textEndName = new System.Windows.Forms.TextBox();
			this.textStartName = new System.Windows.Forms.TextBox();
			this.butStartName = new OpenPmsDental.UI.Button();
			this.labAllProviders = new System.Windows.Forms.Label();
			this.labOnlyActive = new System.Windows.Forms.Label();
			this.labGroupByFamily = new System.Windows.Forms.Label();
			this.labEndName = new System.Windows.Forms.Label();
			this.labStartName = new System.Windows.Forms.Label();
			this.listStatus = new OpenPmsDental.UI.ListBox();
			this.tabLabelSetup = new OpenPmsDental.UI.TabControl();
			this.tabBirthday = new OpenPmsDental.UI.TabPage();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.butBirthdayMonth = new OpenPmsDental.UI.Button();
			this.butBirthdayRight = new OpenPmsDental.UI.Button();
			this.butBirthdayLeft = new OpenPmsDental.UI.Button();
			this.textBirthdayTo = new System.Windows.Forms.TextBox();
			this.labBirthdayFrom = new System.Windows.Forms.Label();
			this.textBirthdayFrom = new System.Windows.Forms.TextBox();
			this.labBirthdayTo = new System.Windows.Forms.Label();
			this.checkBirthdayActive = new OpenPmsDental.UI.CheckBox();
			this.labLabelStart = new System.Windows.Forms.Label();
			this.panLabels = new System.Windows.Forms.Panel();
			this.butOK = new OpenPmsDental.UI.Button();
			this.tabCustomLabels.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericCusCount)).BeginInit();
			this.tabInsCo.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericInsCoSingle)).BeginInit();
			this.tabPage1.SuspendLayout();
			this.tabLabelSetup.SuspendLayout();
			this.tabBirthday.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabCustomLabels
			// 
			this.tabCustomLabels.Controls.Add(this.butCusRemove);
			this.tabCustomLabels.Controls.Add(this.butCusAdd);
			this.tabCustomLabels.Controls.Add(this.listCusNames);
			this.tabCustomLabels.Controls.Add(this.numericCusCount);
			this.tabCustomLabels.Controls.Add(this.textCusZip);
			this.tabCustomLabels.Controls.Add(this.textCusState);
			this.tabCustomLabels.Controls.Add(this.textCusCity);
			this.tabCustomLabels.Controls.Add(this.textCusAddr2);
			this.tabCustomLabels.Controls.Add(this.textCusAddr1);
			this.tabCustomLabels.Controls.Add(this.textCusName);
			this.tabCustomLabels.Controls.Add(this.labLabelCount);
			this.tabCustomLabels.Controls.Add(this.labZip);
			this.tabCustomLabels.Controls.Add(this.labState);
			this.tabCustomLabels.Controls.Add(this.labCity);
			this.tabCustomLabels.Controls.Add(this.labCustAddr2);
			this.tabCustomLabels.Controls.Add(this.labCustAddr1);
			this.tabCustomLabels.Controls.Add(this.labCustName);
			this.tabCustomLabels.Location = new System.Drawing.Point(2, 21);
			this.tabCustomLabels.Name = "tabCustomLabels";
			this.tabCustomLabels.Padding = new System.Windows.Forms.Padding(3);
			this.tabCustomLabels.Size = new System.Drawing.Size(419, 326);
			this.tabCustomLabels.TabIndex = 2;
			this.tabCustomLabels.Text = "Custom Labels";
			// 
			// butCusRemove
			// 
			this.butCusRemove.Location = new System.Drawing.Point(330, 170);
			this.butCusRemove.Name = "butCusRemove";
			this.butCusRemove.Size = new System.Drawing.Size(75, 23);
			this.butCusRemove.TabIndex = 15;
			this.butCusRemove.Text = "Remove";
			this.butCusRemove.UseVisualStyleBackColor = true;
			this.butCusRemove.Visible = false;
			this.butCusRemove.Click += new System.EventHandler(this.butCusRemove_Click);
			// 
			// butCusAdd
			// 
			this.butCusAdd.Location = new System.Drawing.Point(249, 170);
			this.butCusAdd.Name = "butCusAdd";
			this.butCusAdd.Size = new System.Drawing.Size(75, 23);
			this.butCusAdd.TabIndex = 14;
			this.butCusAdd.Text = "&Add";
			this.butCusAdd.UseVisualStyleBackColor = true;
			this.butCusAdd.Click += new System.EventHandler(this.butCusAdd_Click);
			// 
			// listCusNames
			// 
			this.listCusNames.Location = new System.Drawing.Point(8, 197);
			this.listCusNames.Name = "listCusNames";
			this.listCusNames.Size = new System.Drawing.Size(403, 121);
			this.listCusNames.TabIndex = 16;
			this.listCusNames.SelectedIndexChanged += new System.EventHandler(this.listCusNames_SelectedIndexChanged);
			// 
			// numericCusCount
			// 
			this.numericCusCount.Location = new System.Drawing.Point(159, 143);
			this.numericCusCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericCusCount.Name = "numericCusCount";
			this.numericCusCount.Size = new System.Drawing.Size(120, 20);
			this.numericCusCount.TabIndex = 13;
			this.numericCusCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// textCusZip
			// 
			this.textCusZip.Location = new System.Drawing.Point(249, 116);
			this.textCusZip.MaxLength = 10;
			this.textCusZip.Name = "textCusZip";
			this.textCusZip.Size = new System.Drawing.Size(100, 20);
			this.textCusZip.TabIndex = 12;
			this.textCusZip.WordWrap = false;
			// 
			// textCusState
			// 
			this.textCusState.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.textCusState.Location = new System.Drawing.Point(159, 116);
			this.textCusState.MaxLength = 2;
			this.textCusState.Name = "textCusState";
			this.textCusState.Size = new System.Drawing.Size(36, 23);
			this.textCusState.TabIndex = 11;
			this.textCusState.WordWrap = false;
			// 
			// textCusCity
			// 
			this.textCusCity.Location = new System.Drawing.Point(159, 89);
			this.textCusCity.MaxLength = 40;
			this.textCusCity.Name = "textCusCity";
			this.textCusCity.Size = new System.Drawing.Size(252, 23);
			this.textCusCity.TabIndex = 10;
			this.textCusCity.WordWrap = false;
			// 
			// textCusAddr2
			// 
			this.textCusAddr2.Location = new System.Drawing.Point(159, 62);
			this.textCusAddr2.MaxLength = 40;
			this.textCusAddr2.Name = "textCusAddr2";
			this.textCusAddr2.Size = new System.Drawing.Size(252, 23);
			this.textCusAddr2.TabIndex = 9;
			this.textCusAddr2.WordWrap = false;
			// 
			// textCusAddr1
			// 
			this.textCusAddr1.Location = new System.Drawing.Point(159, 35);
			this.textCusAddr1.MaxLength = 40;
			this.textCusAddr1.Name = "textCusAddr1";
			this.textCusAddr1.Size = new System.Drawing.Size(252, 23);
			this.textCusAddr1.TabIndex = 8;
			this.textCusAddr1.WordWrap = false;
			// 
			// textCusName
			// 
			this.textCusName.Location = new System.Drawing.Point(159, 8);
			this.textCusName.MaxLength = 40;
			this.textCusName.Name = "textCusName";
			this.textCusName.Size = new System.Drawing.Size(252, 23);
			this.textCusName.TabIndex = 7;
			this.textCusName.WordWrap = false;
			// 
			// labLabelCount
			// 
			this.labLabelCount.AutoSize = false;
			this.labLabelCount.Location = new System.Drawing.Point(77, 143);
			this.labLabelCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labLabelCount.Name = "labLabelCount";
			this.labLabelCount.Size = new System.Drawing.Size(76, 23);
			this.labLabelCount.TabIndex = 6;
			this.labLabelCount.Text = "Label Count";
			// 
			// labZip
			// 
			this.labZip.AutoSize = false;
			this.labZip.Location = new System.Drawing.Point(211, 116);
			this.labZip.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labZip.Name = "labZip";
			this.labZip.Size = new System.Drawing.Size(32, 23);
			this.labZip.TabIndex = 5;
			this.labZip.Text = "Zip:";
			// 
			// labState
			// 
			this.labState.AutoSize = false;
			this.labState.Location = new System.Drawing.Point(113, 116);
			this.labState.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labState.Name = "labState";
			this.labState.Size = new System.Drawing.Size(40, 23);
			this.labState.TabIndex = 4;
			this.labState.Text = "State:";
			// 
			// labCity
			// 
			this.labCity.AutoSize = false;
			this.labCity.Location = new System.Drawing.Point(117, 89);
			this.labCity.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labCity.Name = "labCity";
			this.labCity.Size = new System.Drawing.Size(36, 23);
			this.labCity.TabIndex = 3;
			this.labCity.Text = "City:";
			// 
			// labCustAddr2
			// 
			this.labCustAddr2.AutoSize = false;
			this.labCustAddr2.Location = new System.Drawing.Point(89, 62);
			this.labCustAddr2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labCustAddr2.Name = "labCustAddr2";
			this.labCustAddr2.Size = new System.Drawing.Size(64, 23);
			this.labCustAddr2.TabIndex = 2;
			this.labCustAddr2.Text = "Address2:";
			// 
			// labCustAddr1
			// 
			this.labCustAddr1.AutoSize = false;
			this.labCustAddr1.Location = new System.Drawing.Point(89, 35);
			this.labCustAddr1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labCustAddr1.Name = "labCustAddr1";
			this.labCustAddr1.Size = new System.Drawing.Size(64, 23);
			this.labCustAddr1.TabIndex = 1;
			this.labCustAddr1.Text = "Address1:";
			// 
			// labCustName
			// 
			this.labCustName.AutoSize = false;
			this.labCustName.Location = new System.Drawing.Point(105, 8);
			this.labCustName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labCustName.Name = "labCustName";
			this.labCustName.Size = new System.Drawing.Size(48, 23);
			this.labCustName.TabIndex = 0;
			this.labCustName.Text = "Name:";
			// 
			// tabInsCo
			// 
			this.tabInsCo.Controls.Add(this.butInsCoEnd);
			this.tabInsCo.Controls.Add(this.butInsCoStart);
			this.tabInsCo.Controls.Add(this.labInsCoEndAddr);
			this.tabInsCo.Controls.Add(this.labInsCoStartAddr);
			this.tabInsCo.Controls.Add(this.labInsCoSingle);
			this.tabInsCo.Controls.Add(this.numericInsCoSingle);
			this.tabInsCo.Controls.Add(this.labInsCoEnd);
			this.tabInsCo.Controls.Add(this.textInsCoEnd);
			this.tabInsCo.Controls.Add(this.labInsCoStart);
			this.tabInsCo.Controls.Add(this.textInsCoStart);
			this.tabInsCo.Controls.Add(this.radioButRange);
			this.tabInsCo.Controls.Add(this.radioButSingle);
			this.tabInsCo.Location = new System.Drawing.Point(2, 21);
			this.tabInsCo.Name = "tabInsCo";
			this.tabInsCo.Padding = new System.Windows.Forms.Padding(3);
			this.tabInsCo.Size = new System.Drawing.Size(419, 322);
			this.tabInsCo.TabIndex = 1;
			this.tabInsCo.Text = "Insurance Company Labels";
			// 
			// butInsCoEnd
			// 
			this.butInsCoEnd.Location = new System.Drawing.Point(303, 75);
			this.butInsCoEnd.Name = "butInsCoEnd";
			this.butInsCoEnd.Size = new System.Drawing.Size(21, 20);
			this.butInsCoEnd.TabIndex = 12;
			this.butInsCoEnd.Text = "...";
			this.butInsCoEnd.UseVisualStyleBackColor = true;
			this.butInsCoEnd.Visible = false;
			this.butInsCoEnd.Click += new System.EventHandler(this.butInsCo_Click);
			// 
			// butInsCoStart
			// 
			this.butInsCoStart.Location = new System.Drawing.Point(345, 32);
			this.butInsCoStart.Name = "butInsCoStart";
			this.butInsCoStart.Size = new System.Drawing.Size(21, 23);
			this.butInsCoStart.TabIndex = 11;
			this.butInsCoStart.Text = "...";
			this.butInsCoStart.UseVisualStyleBackColor = true;
			this.butInsCoStart.Click += new System.EventHandler(this.butInsCo_Click);
			// 
			// labInsCoEndAddr
			// 
			this.labInsCoEndAddr.Location = new System.Drawing.Point(8, 101);
			this.labInsCoEndAddr.Name = "labInsCoEndAddr";
			this.labInsCoEndAddr.Size = new System.Drawing.Size(333, 13);
			this.labInsCoEndAddr.TabIndex = 10;
			this.labInsCoEndAddr.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// labInsCoStartAddr
			// 
			this.labInsCoStartAddr.Enabled = false;
			this.labInsCoStartAddr.Location = new System.Drawing.Point(8, 59);
			this.labInsCoStartAddr.Name = "labInsCoStartAddr";
			this.labInsCoStartAddr.Size = new System.Drawing.Size(331, 13);
			this.labInsCoStartAddr.TabIndex = 9;
			this.labInsCoStartAddr.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// labInsCoSingle
			// 
			this.labInsCoSingle.AutoSize = false;
			this.labInsCoSingle.Location = new System.Drawing.Point(36, 75);
			this.labInsCoSingle.Name = "labInsCoSingle";
			this.labInsCoSingle.Size = new System.Drawing.Size(74, 17);
			this.labInsCoSingle.TabIndex = 7;
			this.labInsCoSingle.Text = "Label Count:";
			// 
			// numericInsCoSingle
			// 
			this.numericInsCoSingle.Location = new System.Drawing.Point(116, 74);
			this.numericInsCoSingle.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
			this.numericInsCoSingle.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericInsCoSingle.Name = "numericInsCoSingle";
			this.numericInsCoSingle.Size = new System.Drawing.Size(120, 20);
			this.numericInsCoSingle.TabIndex = 6;
			this.numericInsCoSingle.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// labInsCoEnd
			// 
			this.labInsCoEnd.AutoSize = true;
			this.labInsCoEnd.Location = new System.Drawing.Point(43, 75);
			this.labInsCoEnd.Name = "labInsCoEnd";
			this.labInsCoEnd.Size = new System.Drawing.Size(60, 13);
			this.labInsCoEnd.TabIndex = 5;
			this.labInsCoEnd.Text = "End Name:";
			this.labInsCoEnd.Visible = false;
			// 
			// textInsCoEnd
			// 
			this.textInsCoEnd.Location = new System.Drawing.Point(116, 74);
			this.textInsCoEnd.Name = "textInsCoEnd";
			this.textInsCoEnd.Size = new System.Drawing.Size(181, 20);
			this.textInsCoEnd.TabIndex = 4;
			this.textInsCoEnd.Visible = false;
			this.textInsCoEnd.WordWrap = false;
			this.textInsCoEnd.Click += new System.EventHandler(this.textInsCoEnd_Click);
			// 
			// labInsCoStart
			// 
			this.labInsCoStart.AutoSize = false;
			this.labInsCoStart.Location = new System.Drawing.Point(77, 32);
			this.labInsCoStart.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labInsCoStart.Name = "labInsCoStart";
			this.labInsCoStart.Size = new System.Drawing.Size(76, 23);
			this.labInsCoStart.TabIndex = 3;
			this.labInsCoStart.Text = "Start Name:";
			// 
			// textInsCoStart
			// 
			this.textInsCoStart.Location = new System.Drawing.Point(159, 32);
			this.textInsCoStart.Name = "textInsCoStart";
			this.textInsCoStart.Size = new System.Drawing.Size(180, 23);
			this.textInsCoStart.TabIndex = 2;
			this.textInsCoStart.WordWrap = false;
			this.textInsCoStart.Click += new System.EventHandler(this.textInsCoStart_Click);
			// 
			// radioButRange
			// 
			this.radioButRange.Location = new System.Drawing.Point(277, 8);
			this.radioButRange.Name = "radioButRange";
			this.radioButRange.Size = new System.Drawing.Size(134, 20);
			this.radioButRange.TabIndex = 1;
			this.radioButRange.Text = "Range of Companies";
			this.radioButRange.UseVisualStyleBackColor = true;
			this.radioButRange.CheckedChanged += new System.EventHandler(this.radioButRange_CheckedChanged);
			// 
			// radioButSingle
			// 
			this.radioButSingle.AutoSize = true;
			this.radioButSingle.Checked = true;
			this.radioButSingle.Location = new System.Drawing.Point(159, 8);
			this.radioButSingle.Name = "radioButSingle";
			this.radioButSingle.Size = new System.Drawing.Size(112, 20);
			this.radioButSingle.TabIndex = 0;
			this.radioButSingle.TabStop = true;
			this.radioButSingle.Text = "Single Company";
			this.radioButSingle.UseVisualStyleBackColor = true;
			this.radioButSingle.CheckedChanged += new System.EventHandler(this.radioButSingle_CheckedChanged);
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.labEndingName);
			this.tabPage1.Controls.Add(this.listProviders);
			this.tabPage1.Controls.Add(this.checkAllProviders);
			this.tabPage1.Controls.Add(this.checkActiveOnly);
			this.tabPage1.Controls.Add(this.checkGroupByFamily);
			this.tabPage1.Controls.Add(this.butEndName);
			this.tabPage1.Controls.Add(this.textEndName);
			this.tabPage1.Controls.Add(this.textStartName);
			this.tabPage1.Controls.Add(this.butStartName);
			this.tabPage1.Controls.Add(this.labAllProviders);
			this.tabPage1.Controls.Add(this.labOnlyActive);
			this.tabPage1.Controls.Add(this.labGroupByFamily);
			this.tabPage1.Controls.Add(this.labEndName);
			this.tabPage1.Controls.Add(this.labStartName);
			this.tabPage1.Location = new System.Drawing.Point(2, 21);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(419, 322);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Patient Labels";
			// 
			// labEndingName
			// 
			this.labEndingName.AutoSize = false;
			this.labEndingName.Location = new System.Drawing.Point(96, 35);
			this.labEndingName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labEndingName.Name = "labEndingName";
			this.labEndingName.Size = new System.Drawing.Size(100, 23);
			this.labEndingName.TabIndex = 15;
			this.labEndingName.Text = "End Name (L, F):";
			// 
			// listProviders
			// 
			this.listProviders.Location = new System.Drawing.Point(202, 143);
			this.listProviders.Name = "listProviders";
			this.listProviders.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listProviders.Size = new System.Drawing.Size(172, 160);
			this.listProviders.TabIndex = 14;
			this.listProviders.Visible = false;
			// 
			// checkAllProviders
			// 
			this.checkAllProviders.Checked = true;
			this.checkAllProviders.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAllProviders.Location = new System.Drawing.Point(202, 120);
			this.checkAllProviders.Name = "checkAllProviders";
			this.checkAllProviders.Size = new System.Drawing.Size(15, 14);
			this.checkAllProviders.TabIndex = 12;
			this.checkAllProviders.CheckedChanged += new System.EventHandler(this.checkAllProviders_CheckedChanged);
			// 
			// checkActiveOnly
			// 
			this.checkActiveOnly.Checked = true;
			this.checkActiveOnly.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkActiveOnly.Location = new System.Drawing.Point(202, 93);
			this.checkActiveOnly.Name = "checkActiveOnly";
			this.checkActiveOnly.Size = new System.Drawing.Size(15, 14);
			this.checkActiveOnly.TabIndex = 11;
			this.checkActiveOnly.CheckedChanged += new System.EventHandler(this.checkActiveOnly_CheckedChanged);
			// 
			// checkGroupByFamily
			// 
			this.checkGroupByFamily.Checked = true;
			this.checkGroupByFamily.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkGroupByFamily.Location = new System.Drawing.Point(202, 66);
			this.checkGroupByFamily.Name = "checkGroupByFamily";
			this.checkGroupByFamily.Size = new System.Drawing.Size(15, 14);
			this.checkGroupByFamily.TabIndex = 10;
			// 
			// butEndName
			// 
			this.butEndName.Location = new System.Drawing.Point(380, 35);
			this.butEndName.Name = "butEndName";
			this.butEndName.Size = new System.Drawing.Size(19, 23);
			this.butEndName.TabIndex = 9;
			this.butEndName.Text = "...";
			this.butEndName.UseVisualStyleBackColor = true;
			this.butEndName.Click += new System.EventHandler(this.butEndName_Click);
			// 
			// textEndName
			// 
			this.textEndName.Location = new System.Drawing.Point(202, 35);
			this.textEndName.Name = "textEndName";
			this.textEndName.Size = new System.Drawing.Size(172, 20);
			this.textEndName.TabIndex = 8;
			this.textEndName.WordWrap = false;
			// 
			// textStartName
			// 
			this.textStartName.Location = new System.Drawing.Point(159, 8);
			this.textStartName.Name = "textStartName";
			this.textStartName.Size = new System.Drawing.Size(172, 20);
			this.textStartName.TabIndex = 7;
			this.textStartName.WordWrap = false;
			// 
			// butStartName
			// 
			this.butStartName.Location = new System.Drawing.Point(337, 8);
			this.butStartName.Name = "butStartName";
			this.butStartName.Size = new System.Drawing.Size(19, 23);
			this.butStartName.TabIndex = 6;
			this.butStartName.Text = "...";
			this.butStartName.UseVisualStyleBackColor = true;
			this.butStartName.Click += new System.EventHandler(this.butStartName_Click);
			// 
			// labAllProviders
			// 
			this.labAllProviders.AutoSize = false;
			this.labAllProviders.Location = new System.Drawing.Point(116, 116);
			this.labAllProviders.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labAllProviders.Name = "labAllProviders";
			this.labAllProviders.Size = new System.Drawing.Size(80, 23);
			this.labAllProviders.TabIndex = 4;
			this.labAllProviders.Text = "All Providers:";
			// 
			// labOnlyActive
			// 
			this.labOnlyActive.AutoSize = false;
			this.labOnlyActive.Location = new System.Drawing.Point(76, 89);
			this.labOnlyActive.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labOnlyActive.Name = "labOnlyActive";
			this.labOnlyActive.Size = new System.Drawing.Size(120, 23);
			this.labOnlyActive.TabIndex = 3;
			this.labOnlyActive.Text = "Only Active Patients:";
			// 
			// labGroupByFamily
			// 
			this.labGroupByFamily.AutoSize = false;
			this.labGroupByFamily.Location = new System.Drawing.Point(92, 62);
			this.labGroupByFamily.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labGroupByFamily.Name = "labGroupByFamily";
			this.labGroupByFamily.Size = new System.Drawing.Size(104, 23);
			this.labGroupByFamily.TabIndex = 2;
			this.labGroupByFamily.Text = "Group by Family:";
			// 
			// labEndName
			// 
			this.labEndName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.labEndName.AutoSize = false;
			this.labEndName.Location = new System.Drawing.Point(-95, 53);
			this.labEndName.Name = "labEndName";
			this.labEndName.Size = new System.Drawing.Size(61, 20);
			this.labEndName.TabIndex = 1;
			this.labEndName.Text = "End Name:";
			// 
			// labStartName
			// 
			this.labStartName.AutoSize = false;
			this.labStartName.Location = new System.Drawing.Point(49, 8);
			this.labStartName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labStartName.Name = "labStartName";
			this.labStartName.Size = new System.Drawing.Size(104, 23);
			this.labStartName.TabIndex = 0;
			this.labStartName.Text = "Start Name (L, F):";
			// 
			// listStatus
			// 
			this.listStatus.Location = new System.Drawing.Point(0, 0);
			this.listStatus.Name = "listStatus";
			this.listStatus.Size = new System.Drawing.Size(120, 95);
			this.listStatus.TabIndex = 0;
			// 
			// tabLabelSetup
			// 
			this.tabLabelSetup.Controls.Add(this.tabPage1);
			this.tabLabelSetup.Controls.Add(this.tabInsCo);
			this.tabLabelSetup.Controls.Add(this.tabCustomLabels);
			this.tabLabelSetup.Controls.Add(this.tabBirthday);
			this.tabLabelSetup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.tabLabelSetup.Location = new System.Drawing.Point(13, 8);
			this.tabLabelSetup.Name = "tabLabelSetup";
			this.tabLabelSetup.Size = new System.Drawing.Size(423, 349);
			this.tabLabelSetup.TabIndex = 3;
			// 
			// tabBirthday
			// 
			this.tabBirthday.Controls.Add(this.groupBox1);
			this.tabBirthday.Location = new System.Drawing.Point(2, 21);
			this.tabBirthday.Name = "tabBirthday";
			this.tabBirthday.Padding = new System.Windows.Forms.Padding(3);
			this.tabBirthday.Size = new System.Drawing.Size(419, 322);
			this.tabBirthday.TabIndex = 3;
			this.tabBirthday.Text = "Birthday Labels";
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.butBirthdayMonth);
			this.groupBox1.Controls.Add(this.butBirthdayRight);
			this.groupBox1.Controls.Add(this.butBirthdayLeft);
			this.groupBox1.Controls.Add(this.textBirthdayTo);
			this.groupBox1.Controls.Add(this.labBirthdayFrom);
			this.groupBox1.Controls.Add(this.textBirthdayFrom);
			this.groupBox1.Controls.Add(this.labBirthdayTo);
			this.groupBox1.Controls.Add(this.checkBirthdayActive);
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(330, 132);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.Text = "Date Range (without the year)";
			// 
			// butBirthdayMonth
			// 
			this.butBirthdayMonth.Location = new System.Drawing.Point(120, 23);
			this.butBirthdayMonth.Name = "butBirthdayMonth";
			this.butBirthdayMonth.Size = new System.Drawing.Size(75, 23);
			this.butBirthdayMonth.TabIndex = 9;
			this.butBirthdayMonth.Text = "Next Month";
			this.butBirthdayMonth.UseVisualStyleBackColor = true;
			this.butBirthdayMonth.Click += new System.EventHandler(this.butBirthdayMonth_Click);
			// 
			// butBirthdayRight
			// 
			this.butBirthdayRight.Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons.ArrowRight,22,22);
			this.butBirthdayRight.Location = new System.Drawing.Point(201, 23);
			this.butBirthdayRight.Name = "butBirthdayRight";
			this.butBirthdayRight.Size = new System.Drawing.Size(37, 23);
			this.butBirthdayRight.TabIndex = 8;
			this.butBirthdayRight.UseVisualStyleBackColor = true;
			this.butBirthdayRight.Click += new System.EventHandler(this.butBirthdayRight_Click);
			// 
			// butBirthdayLeft
			// 
			this.butBirthdayLeft.Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons.ArrowLeft,22,22);
			this.butBirthdayLeft.Location = new System.Drawing.Point(77, 23);
			this.butBirthdayLeft.Name = "butBirthdayLeft";
			this.butBirthdayLeft.Size = new System.Drawing.Size(37, 23);
			this.butBirthdayLeft.TabIndex = 7;
			this.butBirthdayLeft.Text = "button1";
			this.butBirthdayLeft.UseVisualStyleBackColor = true;
			this.butBirthdayLeft.Click += new System.EventHandler(this.butBirthdayLeft_Click);
			// 
			// textBirthdayTo
			// 
			this.textBirthdayTo.Location = new System.Drawing.Point(151, 77);
			this.textBirthdayTo.Name = "textBirthdayTo";
			this.textBirthdayTo.Size = new System.Drawing.Size(72, 23);
			this.textBirthdayTo.TabIndex = 6;
			this.textBirthdayTo.WordWrap = false;
			// 
			// labBirthdayFrom
			// 
			this.labBirthdayFrom.AutoSize = false;
			this.labBirthdayFrom.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
			this.labBirthdayFrom.Location = new System.Drawing.Point(101, 50);
			this.labBirthdayFrom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labBirthdayFrom.Name = "labBirthdayFrom";
			this.labBirthdayFrom.Size = new System.Drawing.Size(44, 23);
			this.labBirthdayFrom.TabIndex = 1;
			this.labBirthdayFrom.Text = "From:";
			// 
			// textBirthdayFrom
			// 
			this.textBirthdayFrom.Location = new System.Drawing.Point(151, 50);
			this.textBirthdayFrom.Name = "textBirthdayFrom";
			this.textBirthdayFrom.Size = new System.Drawing.Size(72, 23);
			this.textBirthdayFrom.TabIndex = 5;
			this.textBirthdayFrom.WordWrap = false;
			// 
			// labBirthdayTo
			// 
			this.labBirthdayTo.AutoSize = false;
			this.labBirthdayTo.Location = new System.Drawing.Point(117, 77);
			this.labBirthdayTo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labBirthdayTo.Name = "labBirthdayTo";
			this.labBirthdayTo.Size = new System.Drawing.Size(28, 23);
			this.labBirthdayTo.TabIndex = 2;
			this.labBirthdayTo.Text = "To:";
			// 
			// checkBirthdayActive
			// 
			this.checkBirthdayActive.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkBirthdayActive.Checked = true;
			this.checkBirthdayActive.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBirthdayActive.Location = new System.Drawing.Point(151, 104);
			this.checkBirthdayActive.Name = "checkBirthdayActive";
			this.checkBirthdayActive.Size = new System.Drawing.Size(134, 20);
			this.checkBirthdayActive.TabIndex = 4;
			this.checkBirthdayActive.Text = "Active Patients Only:";
			this.checkBirthdayActive.CheckedChanged += new System.EventHandler(this.checkBirthdayActive_CheckedChanged);
			// 
			// labLabelStart
			// 
			this.labLabelStart.AutoSize = false;
			this.labLabelStart.Location = new System.Drawing.Point(458, 48);
			this.labLabelStart.Name = "labLabelStart";
			this.labLabelStart.Size = new System.Drawing.Size(156, 19);
			this.labLabelStart.TabIndex = 5;
			this.labLabelStart.Text = "Choose Label Start position";
			// 
			// panLabels
			// 
			this.panLabels.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.panLabels.Location = new System.Drawing.Point(458, 69);
			this.panLabels.Name = "panLabels";
			this.panLabels.Size = new System.Drawing.Size(170, 230);
			this.panLabels.TabIndex = 8;
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(566, 386);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 23);
			this.butOK.TabIndex = 0;
			this.butOK.Text = "OK";
			this.butOK.UseVisualStyleBackColor = true;
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// FormRpLaserLabels
			// 
			this.ClientSize = new System.Drawing.Size(649, 417);
			this.Controls.Add(this.panLabels);
			this.Controls.Add(this.labLabelStart);
			this.Controls.Add(this.tabLabelSetup);
			this.Controls.Add(this.butOK);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormRpLaserLabels";
			this.ShowInTaskbar = false;
			this.Text = "Laser Labels";
			this.Load += new System.EventHandler(this.FormLaserLabels_Load);
			this.tabCustomLabels.ResumeLayout(false);
			this.tabCustomLabels.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericCusCount)).EndInit();
			this.tabInsCo.ResumeLayout(false);
			this.tabInsCo.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericInsCoSingle)).EndInit();
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabLabelSetup.ResumeLayout(false);
			this.tabBirthday.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private OpenPmsDental.UI.Button butOK;
		private OpenPmsDental.UI.TabPage tabCustomLabels;
		private OpenPmsDental.UI.TabPage tabInsCo;
		private OpenPmsDental.UI.TabPage tabPage1;
		private OpenPmsDental.UI.ListBox listProviders;
		private OpenPmsDental.UI.CheckBox checkAllProviders;
		private OpenPmsDental.UI.CheckBox checkActiveOnly;
		private OpenPmsDental.UI.CheckBox checkGroupByFamily;
		private TextBox textEndName;
		private TextBox textStartName;
		private Label labAllProviders;
		private Label labOnlyActive;
		private Label labGroupByFamily;
		private Label labEndName;
		private Label labStartName;
		private OpenPmsDental.UI.TabControl tabLabelSetup;
		private Label labLabelStart;
		private Label labEndingName;
		private RadioButton radioButRange;
		private RadioButton radioButSingle;
		private TextBox textInsCoStart;
		private Label labInsCoEnd;
		private TextBox textInsCoEnd;
		private Label labInsCoStart;
		private Label labInsCoSingle;
		private NumericUpDown numericInsCoSingle;
		private OpenPmsDental.UI.Button butEndName;
		private OpenPmsDental.UI.Button butStartName;
		private Label labZip;
		private Label labState;
		private Label labCity;
		private Label labCustAddr2;
		private Label labCustAddr1;
		private Label labCustName;
		private Label labLabelCount;
		private TextBox textCusZip;
		private TextBox textCusState;
		private TextBox textCusCity;
		private TextBox textCusAddr2;
		private TextBox textCusAddr1;
		private TextBox textCusName;
		private NumericUpDown numericCusCount;
		private OpenPmsDental.UI.ListBox listCusNames;
		private OpenPmsDental.UI.Button butCusAdd;
		private OpenPmsDental.UI.ListBox listStatus;
		private Panel panLabels;
		private OpenPmsDental.UI.Button butCusRemove;
		private Label labInsCoStartAddr;
		private Label labInsCoEndAddr;
		private OpenPmsDental.UI.Button butInsCoEnd;
		private OpenPmsDental.UI.TabPage tabBirthday;
		private Label labBirthdayTo;
		private Label labBirthdayFrom;
		private OpenPmsDental.UI.CheckBox checkBirthdayActive;
		private TextBox textBirthdayFrom;
		private TextBox textBirthdayTo;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private OpenPmsDental.UI.Button butBirthdayLeft;
		private OpenPmsDental.UI.Button butBirthdayRight;
		private OpenPmsDental.UI.Button butBirthdayMonth;
		private OpenPmsDental.UI.Button butInsCoStart;
	}
}
