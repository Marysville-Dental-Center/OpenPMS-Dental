namespace OpenPmsDental{
	partial class FormTimeCardRuleEdit {
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
			this.label5 = new System.Windows.Forms.Label();
			this.checkIsOvertimeExempt = new OpenPmsDental.UI.CheckBox();
			this.textClockInMin = new System.Windows.Forms.TextBox();
			this.textOverHoursPerDay = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.but6am = new OpenPmsDental.UI.Button();
			this.textAfterTimeOfDay = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.but5pm = new OpenPmsDental.UI.Button();
			this.textBeforeTimeOfDay = new System.Windows.Forms.TextBox();
			this.listEmp = new OpenPmsDental.UI.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.butSave = new OpenPmsDental.UI.Button();
			this.checkUseRate3 = new OpenPmsDental.UI.CheckBox();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(60, 176);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(124, 23);
			this.label5.TabIndex = 0;
			this.label5.Text = "Earliest Clock in Time";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkIsOvertimeExempt
			// 
			this.checkIsOvertimeExempt.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIsOvertimeExempt.Location = new System.Drawing.Point(190, 152);
			this.checkIsOvertimeExempt.Name = "checkIsOvertimeExempt";
			this.checkIsOvertimeExempt.Size = new System.Drawing.Size(129, 20);
			this.checkIsOvertimeExempt.TabIndex = 4;
			this.checkIsOvertimeExempt.Text = "Is Overtime Exempt";
			// 
			// textClockInMin
			// 
			this.textClockInMin.Location = new System.Drawing.Point(190, 176);
			this.textClockInMin.Name = "textClockInMin";
			this.textClockInMin.Size = new System.Drawing.Size(64, 23);
			this.textClockInMin.TabIndex = 5;
			this.textClockInMin.Text = "6:00";
			// 
			// textOverHoursPerDay
			// 
			this.textOverHoursPerDay.Location = new System.Drawing.Point(190, 8);
			this.textOverHoursPerDay.Name = "textOverHoursPerDay";
			this.textOverHoursPerDay.Size = new System.Drawing.Size(64, 23);
			this.textOverHoursPerDay.TabIndex = 1;
			this.textOverHoursPerDay.Text = "8:00";
			this.textOverHoursPerDay.TextChanged += new System.EventHandler(this.textOverHoursPerDay_TextChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(176, 23);
			this.label2.TabIndex = 0;
			this.label2.Text = "Overtime if over Hours Per Day";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.but6am);
			this.groupBox2.Controls.Add(this.textAfterTimeOfDay);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.but5pm);
			this.groupBox2.Controls.Add(this.textBeforeTimeOfDay);
			this.groupBox2.Location = new System.Drawing.Point(31, 39);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(282, 81);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.Text = "Rate2 Hours";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(41, 23);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(112, 23);
			this.label4.TabIndex = 0;
			this.label4.Text = "Before Time of Day";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// but6am
			// 
			this.but6am.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.but6am.Location = new System.Drawing.Point(229, 23);
			this.but6am.Name = "but6am";
			this.but6am.Size = new System.Drawing.Size(35, 23);
			this.but6am.TabIndex = 0;
			this.but6am.TabStop = false;
			this.but6am.Text = "6 AM";
			this.but6am.Click += new System.EventHandler(this.but6am_Click);
			// 
			// textAfterTimeOfDay
			// 
			this.textAfterTimeOfDay.Location = new System.Drawing.Point(159, 50);
			this.textAfterTimeOfDay.Name = "textAfterTimeOfDay";
			this.textAfterTimeOfDay.Size = new System.Drawing.Size(64, 23);
			this.textAfterTimeOfDay.TabIndex = 2;
			this.textAfterTimeOfDay.Text = "17:00";
			this.textAfterTimeOfDay.TextChanged += new System.EventHandler(this.textAfterTimeOfDay_TextChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(49, 50);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(104, 23);
			this.label3.TabIndex = 0;
			this.label3.Text = "After Time of Day";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// but5pm
			// 
			this.but5pm.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.but5pm.Location = new System.Drawing.Point(229, 50);
			this.but5pm.Name = "but5pm";
			this.but5pm.Size = new System.Drawing.Size(35, 23);
			this.but5pm.TabIndex = 0;
			this.but5pm.TabStop = false;
			this.but5pm.Text = "5 PM";
			this.but5pm.Click += new System.EventHandler(this.but5pm_Click);
			// 
			// textBeforeTimeOfDay
			// 
			this.textBeforeTimeOfDay.Location = new System.Drawing.Point(159, 23);
			this.textBeforeTimeOfDay.Name = "textBeforeTimeOfDay";
			this.textBeforeTimeOfDay.Size = new System.Drawing.Size(64, 23);
			this.textBeforeTimeOfDay.TabIndex = 1;
			this.textBeforeTimeOfDay.Text = "6:00";
			this.textBeforeTimeOfDay.TextChanged += new System.EventHandler(this.textBeforeTimeOfDay_TextChanged);
			// 
			// listEmp
			// 
			this.listEmp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.listEmp.Location = new System.Drawing.Point(31, 222);
			this.listEmp.Name = "listEmp";
			this.listEmp.Size = new System.Drawing.Size(282, 212);
			this.listEmp.TabIndex = 6;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(31, 203);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(175, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "Employee (can select multiple)";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 446);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 24);
			this.butDelete.TabIndex = 9;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(276, 446);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 7;
			this.butSave.Text = "&Save";
			this.butSave.UseVisualStyleBackColor = false;
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// checkUseRate3
			// 
			this.checkUseRate3.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkUseRate3.Location = new System.Drawing.Point(8, 128);
			this.checkUseRate3.Name = "checkUseRate3";
			this.checkUseRate3.Size = new System.Drawing.Size(187, 20);
			this.checkUseRate3.TabIndex = 3;
			this.checkUseRate3.Text = "Use Rate3 for Weekend Hours";
			// 
			// FormTimeCardRuleEdit
			// 
			this.ClientSize = new System.Drawing.Size(359, 478);
			this.Controls.Add(this.checkUseRate3);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.checkIsOvertimeExempt);
			this.Controls.Add(this.textClockInMin);
			this.Controls.Add(this.textOverHoursPerDay);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.listEmp);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butSave);
			this.Name = "FormTimeCardRuleEdit";
			this.Text = "Time Card Rule Edit";
			this.Load += new System.EventHandler(this.FormTimeCardRuleEdit_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textOverHoursPerDay;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textAfterTimeOfDay;
		private OpenPmsDental.UI.Button butDelete;
		private OpenPmsDental.UI.ListBox listEmp;
		private OpenPmsDental.UI.Button but5pm;
		private UI.Button but6am;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textBeforeTimeOfDay;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private OpenPmsDental.UI.CheckBox checkIsOvertimeExempt;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textClockInMin;
		private OpenPmsDental.UI.CheckBox checkUseRate3;
	}
}