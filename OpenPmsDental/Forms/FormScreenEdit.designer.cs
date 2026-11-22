using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormScreenEdit {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.comboGradeLevel = new OpenPmsDental.UI.ComboBox();
			this.label35 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.textComments = new System.Windows.Forms.TextBox();
			this.listUrgency = new OpenPmsDental.UI.ListBox();
			this.checkHasCaries = new OpenPmsDental.UI.CheckBox();
			this.checkNeedsSealants = new OpenPmsDental.UI.CheckBox();
			this.updownAgeArrows = new System.Windows.Forms.DomainUpDown();
			this.radioM = new System.Windows.Forms.RadioButton();
			this.radioF = new System.Windows.Forms.RadioButton();
			this.radioUnknown = new System.Windows.Forms.RadioButton();
			this.checkExistingSealants = new OpenPmsDental.UI.CheckBox();
			this.checkCariesExperience = new OpenPmsDental.UI.CheckBox();
			this.checkEarlyChildCaries = new OpenPmsDental.UI.CheckBox();
			this.checkMissingAllTeeth = new OpenPmsDental.UI.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textBirthdate = new System.Windows.Forms.TextBox();
			this.textAge = new System.Windows.Forms.TextBox();
			this.listRace = new OpenPmsDental.UI.ListBox();
			this.textScreenGroupOrder = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.butSave = new OpenPmsDental.UI.Button();
			this.textName = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// comboGradeLevel
			// 
			this.comboGradeLevel.BackColor = System.Drawing.SystemColors.Window;
			this.comboGradeLevel.Location = new System.Drawing.Point(111, 66);
			this.comboGradeLevel.Name = "comboGradeLevel";
			this.comboGradeLevel.Size = new System.Drawing.Size(148, 23);
			this.comboGradeLevel.TabIndex = 6;
			// 
			// label35
			// 
			this.label35.Location = new System.Drawing.Point(49, 452);
			this.label35.Name = "label35";
			this.label35.Size = new System.Drawing.Size(56, 23);
			this.label35.TabIndex = 16;
			this.label35.Text = "Urgency";
			this.label35.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(33, 66);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(72, 23);
			this.label15.TabIndex = 13;
			this.label15.Text = "Grade Level";
			this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(8, 299);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(113, 17);
			this.label10.TabIndex = 10;
			this.label10.Text = "Race/Ethnicity";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(73, 95);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(32, 23);
			this.label4.TabIndex = 108;
			this.label4.Text = "Age";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(33, 514);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(72, 23);
			this.label8.TabIndex = 115;
			this.label8.Text = "Comments";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textComments
			// 
			this.textComments.Location = new System.Drawing.Point(111, 514);
			this.textComments.MaxLength = 255;
			this.textComments.Name = "textComments";
			this.textComments.Size = new System.Drawing.Size(172, 23);
			this.textComments.TabIndex = 20;
			// 
			// listUrgency
			// 
			this.listUrgency.Location = new System.Drawing.Point(111, 452);
			this.listUrgency.Name = "listUrgency";
			this.listUrgency.Size = new System.Drawing.Size(97, 56);
			this.listUrgency.TabIndex = 9;
			// 
			// checkHasCaries
			// 
			this.checkHasCaries.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkHasCaries.Checked = true;
			this.checkHasCaries.CheckState = System.Windows.Forms.CheckState.Indeterminate;
			this.checkHasCaries.Location = new System.Drawing.Point(111, 153);
			this.checkHasCaries.Name = "checkHasCaries";
			this.checkHasCaries.Size = new System.Drawing.Size(79, 20);
			this.checkHasCaries.TabIndex = 14;
			this.checkHasCaries.Text = "Has Caries";
			this.checkHasCaries.ThreeState = true;
			this.checkHasCaries.Click += new System.EventHandler(this.checkBox_Click);
			// 
			// checkNeedsSealants
			// 
			this.checkNeedsSealants.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkNeedsSealants.Location = new System.Drawing.Point(111, 249);
			this.checkNeedsSealants.Name = "checkNeedsSealants";
			this.checkNeedsSealants.Size = new System.Drawing.Size(106, 20);
			this.checkNeedsSealants.TabIndex = 18;
			this.checkNeedsSealants.Text = "Needs Sealants";
			this.checkNeedsSealants.ThreeState = true;
			this.checkNeedsSealants.Click += new System.EventHandler(this.checkBox_Click);
			// 
			// updownAgeArrows
			// 
			this.updownAgeArrows.InterceptArrowKeys = false;
			this.updownAgeArrows.Location = new System.Drawing.Point(153, 95);
			this.updownAgeArrows.Name = "updownAgeArrows";
			this.updownAgeArrows.Size = new System.Drawing.Size(20, 20);
			this.updownAgeArrows.TabIndex = 7;
			this.updownAgeArrows.MouseDown += new System.Windows.Forms.MouseEventHandler(this.updownAgeArrows_MouseDown);
			// 
			// radioM
			// 
			this.radioM.Location = new System.Drawing.Point(127, 426);
			this.radioM.Name = "radioM";
			this.radioM.Size = new System.Drawing.Size(38, 20);
			this.radioM.TabIndex = 11;
			this.radioM.Text = "M";
			// 
			// radioF
			// 
			this.radioF.Location = new System.Drawing.Point(171, 426);
			this.radioF.Name = "radioF";
			this.radioF.Size = new System.Drawing.Size(33, 20);
			this.radioF.TabIndex = 12;
			this.radioF.Text = "F";
			// 
			// radioUnknown
			// 
			this.radioUnknown.Checked = true;
			this.radioUnknown.Location = new System.Drawing.Point(208, 426);
			this.radioUnknown.Name = "radioUnknown";
			this.radioUnknown.Size = new System.Drawing.Size(32, 20);
			this.radioUnknown.TabIndex = 13;
			this.radioUnknown.TabStop = true;
			this.radioUnknown.Text = "?";
			// 
			// checkExistingSealants
			// 
			this.checkExistingSealants.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkExistingSealants.Location = new System.Drawing.Point(111, 225);
			this.checkExistingSealants.Name = "checkExistingSealants";
			this.checkExistingSealants.Size = new System.Drawing.Size(113, 20);
			this.checkExistingSealants.TabIndex = 17;
			this.checkExistingSealants.Text = "Existing Sealants";
			this.checkExistingSealants.ThreeState = true;
			this.checkExistingSealants.Click += new System.EventHandler(this.checkBox_Click);
			// 
			// checkCariesExperience
			// 
			this.checkCariesExperience.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkCariesExperience.Location = new System.Drawing.Point(111, 201);
			this.checkCariesExperience.Name = "checkCariesExperience";
			this.checkCariesExperience.Size = new System.Drawing.Size(117, 20);
			this.checkCariesExperience.TabIndex = 16;
			this.checkCariesExperience.Text = "Caries Experience";
			this.checkCariesExperience.ThreeState = true;
			this.checkCariesExperience.Click += new System.EventHandler(this.checkBox_Click);
			// 
			// checkEarlyChildCaries
			// 
			this.checkEarlyChildCaries.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEarlyChildCaries.Location = new System.Drawing.Point(111, 177);
			this.checkEarlyChildCaries.Name = "checkEarlyChildCaries";
			this.checkEarlyChildCaries.Size = new System.Drawing.Size(119, 20);
			this.checkEarlyChildCaries.TabIndex = 15;
			this.checkEarlyChildCaries.Text = "Early Child. Caries";
			this.checkEarlyChildCaries.ThreeState = true;
			this.checkEarlyChildCaries.Click += new System.EventHandler(this.checkBox_Click);
			// 
			// checkMissingAllTeeth
			// 
			this.checkMissingAllTeeth.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkMissingAllTeeth.Location = new System.Drawing.Point(111, 273);
			this.checkMissingAllTeeth.Name = "checkMissingAllTeeth";
			this.checkMissingAllTeeth.Size = new System.Drawing.Size(116, 20);
			this.checkMissingAllTeeth.TabIndex = 19;
			this.checkMissingAllTeeth.Text = "Missing All Teeth";
			this.checkMissingAllTeeth.ThreeState = true;
			this.checkMissingAllTeeth.Click += new System.EventHandler(this.checkBox_Click);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(45, 124);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(60, 23);
			this.label5.TabIndex = 139;
			this.label5.Text = "Birthdate";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBirthdate
			// 
			this.textBirthdate.AcceptsReturn = true;
			this.textBirthdate.Location = new System.Drawing.Point(111, 125);
			this.textBirthdate.Multiline = true;
			this.textBirthdate.Name = "textBirthdate";
			this.textBirthdate.Size = new System.Drawing.Size(81, 20);
			this.textBirthdate.TabIndex = 8;
			this.textBirthdate.Validating += new System.ComponentModel.CancelEventHandler(this.textBirthdate_Validating);
			// 
			// textAge
			// 
			this.textAge.AutoSize = false;
			this.textAge.Location = new System.Drawing.Point(111, 95);
			this.textAge.Name = "textAge";
			this.textAge.Size = new System.Drawing.Size(36, 23);
			this.textAge.TabIndex = 141;
			this.textAge.Validating += new System.ComponentModel.CancelEventHandler(this.textAge_Validating);
			// 
			// listRace
			// 
			this.listRace.Location = new System.Drawing.Point(127, 299);
			this.listRace.Name = "listRace";
			this.listRace.Size = new System.Drawing.Size(113, 121);
			this.listRace.TabIndex = 142;
			// 
			// textScreenGroupOrder
			// 
			this.textScreenGroupOrder.Location = new System.Drawing.Point(111, 8);
			this.textScreenGroupOrder.Name = "textScreenGroupOrder";
			this.textScreenGroupOrder.Size = new System.Drawing.Size(36, 23);
			this.textScreenGroupOrder.TabIndex = 144;
			this.textScreenGroupOrder.Validating += new System.ComponentModel.CancelEventHandler(this.textScreenGroupOrder_Validating);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(69, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(36, 23);
			this.label1.TabIndex = 143;
			this.label1.Text = "Row";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 427);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(113, 17);
			this.label2.TabIndex = 10;
			this.label2.Text = "Gender";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 543);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 24);
			this.butDelete.TabIndex = 24;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(228, 579);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 24;
			this.butSave.Text = "Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// textName
			// 
			this.textName.Location = new System.Drawing.Point(111, 37);
			this.textName.MaxLength = 255;
			this.textName.Name = "textName";
			this.textName.ReadOnly = true;
			this.textName.Size = new System.Drawing.Size(172, 23);
			this.textName.TabIndex = 145;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(61, 37);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(44, 23);
			this.label3.TabIndex = 146;
			this.label3.Text = "Name";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FormScreenEdit
			// 
			this.ClientSize = new System.Drawing.Size(311, 611);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textName);
			this.Controls.Add(this.textComments);
			this.Controls.Add(this.listUrgency);
			this.Controls.Add(this.textScreenGroupOrder);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.listRace);
			this.Controls.Add(this.textAge);
			this.Controls.Add(this.checkNeedsSealants);
			this.Controls.Add(this.comboGradeLevel);
			this.Controls.Add(this.updownAgeArrows);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textBirthdate);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.checkMissingAllTeeth);
			this.Controls.Add(this.checkEarlyChildCaries);
			this.Controls.Add(this.checkCariesExperience);
			this.Controls.Add(this.checkExistingSealants);
			this.Controls.Add(this.radioUnknown);
			this.Controls.Add(this.radioF);
			this.Controls.Add(this.radioM);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.checkHasCaries);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label35);
			this.Controls.Add(this.label15);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label10);
			this.Name = "FormScreenEdit";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Screening";
			this.Load += new System.EventHandler(this.FormScreenEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private OpenPmsDental.UI.ComboBox comboGradeLevel;
		private System.Windows.Forms.Label label35;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox textComments;
		private OpenPmsDental.UI.ListBox listUrgency;
		private OpenPmsDental.UI.CheckBox checkNeedsSealants;
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.RadioButton radioM;
		private System.Windows.Forms.RadioButton radioF;
		private System.Windows.Forms.RadioButton radioUnknown;
		private System.Windows.Forms.Label label5;
		private OpenPmsDental.UI.CheckBox checkHasCaries;
		private OpenPmsDental.UI.CheckBox checkExistingSealants;
		private OpenPmsDental.UI.CheckBox checkCariesExperience;
		private OpenPmsDental.UI.CheckBox checkEarlyChildCaries;
		private OpenPmsDental.UI.CheckBox checkMissingAllTeeth;
		private System.Windows.Forms.TextBox textBirthdate;
		private System.Windows.Forms.TextBox textAge;
		private System.Windows.Forms.DomainUpDown updownAgeArrows;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textScreenGroupOrder;
		private OpenPmsDental.UI.ListBox listRace;
		private UI.Button butDelete;
		private Label label2;
		private TextBox textName;
		private Label label3;
	}
}
