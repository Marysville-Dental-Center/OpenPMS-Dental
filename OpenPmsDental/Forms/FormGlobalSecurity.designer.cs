namespace OpenPmsDental{
	partial class FormGlobalSecurity {
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
			this.butSave = new OpenPmsDental.UI.Button();
			this.labelDomainPath = new System.Windows.Forms.Label();
			this.textDomainLoginPath = new System.Windows.Forms.TextBox();
			this.checkDomainLoginEnabled = new OpenPmsDental.UI.CheckBox();
			this.checkDomainLoginForSigBoxes = new OpenPmsDental.UI.CheckBox();
			this.checkPasswordForceWeakToStrong = new OpenPmsDental.UI.CheckBox();
			this.checkPasswordsStrongIncludeSpecial = new OpenPmsDental.UI.CheckBox();
			this.checkDisableBackupReminder = new OpenPmsDental.UI.CheckBox();
			this.labelGlobalDateLockDisabled = new System.Windows.Forms.Label();
			this.checkUserNameManualEntry = new OpenPmsDental.UI.CheckBox();
			this.textLogOffAfterMinutes = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.checkLogOffWindows = new OpenPmsDental.UI.CheckBox();
			this.textDaysLock = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.checkPasswordsMustBeStrong = new OpenPmsDental.UI.CheckBox();
			this.butChange = new OpenPmsDental.UI.Button();
			this.textDateLock = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.checkCannotEditOwn = new OpenPmsDental.UI.CheckBox();
			this.checkTimecardSecurityEnabled = new OpenPmsDental.UI.CheckBox();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.comboGroups = new OpenPmsDental.UI.ComboBox();
			this.labelPermission = new System.Windows.Forms.Label();
			this.checkAllowLogoffOverride = new OpenPmsDental.UI.CheckBox();
			this.checkCannotEditPastPayPeriods = new OpenPmsDental.UI.CheckBox();
			this.checkMaintainPatient = new OpenPmsDental.UI.CheckBox();
			this.checkBadgeLogIn = new OpenPmsDental.UI.CheckBox();
			this.checkAuditPatientSelected = new OpenPmsDental.UI.CheckBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(287, 647);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// labelDomainPath
			// 
			this.labelDomainPath.Location = new System.Drawing.Point(23, 47);
			this.labelDomainPath.Name = "labelDomainPath";
			this.labelDomainPath.Size = new System.Drawing.Size(80, 23);
			this.labelDomainPath.TabIndex = 272;
			this.labelDomainPath.Text = "Domain Path";
			this.labelDomainPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDomainLoginPath
			// 
			this.textDomainLoginPath.Location = new System.Drawing.Point(109, 47);
			this.textDomainLoginPath.Name = "textDomainLoginPath";
			this.textDomainLoginPath.Size = new System.Drawing.Size(147, 23);
			this.textDomainLoginPath.TabIndex = 271;
			this.textDomainLoginPath.Leave += new System.EventHandler(this.textDomainLoginPath_Leave);
			// 
			// checkDomainLoginEnabled
			// 
			this.checkDomainLoginEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkDomainLoginEnabled.Location = new System.Drawing.Point(8, 23);
			this.checkDomainLoginEnabled.Name = "checkDomainLoginEnabled";
			this.checkDomainLoginEnabled.Size = new System.Drawing.Size(146, 20);
			this.checkDomainLoginEnabled.TabIndex = 270;
			this.checkDomainLoginEnabled.Text = "Domain Login Enabled";
			this.checkDomainLoginEnabled.MouseUp += new System.Windows.Forms.MouseEventHandler(this.checkDomainLoginEnabled_MouseUp);
			// 
			// checkDomainLoginForSigBoxes
			// 
			this.checkDomainLoginForSigBoxes.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkDomainLoginForSigBoxes.Location = new System.Drawing.Point(8, 74);
			this.checkDomainLoginForSigBoxes.Name = "checkDomainLoginForSigBoxes";
			this.checkDomainLoginForSigBoxes.Size = new System.Drawing.Size(248, 40);
			this.checkDomainLoginForSigBoxes.TabIndex = 273;
			this.checkDomainLoginForSigBoxes.Text = "Offer domain login in signature boxes' Change User";
			// 
			// checkPasswordForceWeakToStrong
			// 
			this.checkPasswordForceWeakToStrong.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPasswordForceWeakToStrong.Location = new System.Drawing.Point(8, 196);
			this.checkPasswordForceWeakToStrong.Name = "checkPasswordForceWeakToStrong";
			this.checkPasswordForceWeakToStrong.Size = new System.Drawing.Size(221, 20);
			this.checkPasswordForceWeakToStrong.TabIndex = 269;
			this.checkPasswordForceWeakToStrong.Text = "Force password change if not strong";
			// 
			// checkPasswordsStrongIncludeSpecial
			// 
			this.checkPasswordsStrongIncludeSpecial.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPasswordsStrongIncludeSpecial.Location = new System.Drawing.Point(8, 172);
			this.checkPasswordsStrongIncludeSpecial.Name = "checkPasswordsStrongIncludeSpecial";
			this.checkPasswordsStrongIncludeSpecial.Size = new System.Drawing.Size(256, 20);
			this.checkPasswordsStrongIncludeSpecial.TabIndex = 268;
			this.checkPasswordsStrongIncludeSpecial.Text = "Strong passwords require special character";
			// 
			// checkDisableBackupReminder
			// 
			this.checkDisableBackupReminder.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkDisableBackupReminder.Location = new System.Drawing.Point(8, 100);
			this.checkDisableBackupReminder.Name = "checkDisableBackupReminder";
			this.checkDisableBackupReminder.Size = new System.Drawing.Size(207, 20);
			this.checkDisableBackupReminder.TabIndex = 267;
			this.checkDisableBackupReminder.Text = "Disable monthly backup reminder";
			this.checkDisableBackupReminder.Click += new System.EventHandler(this.checkDisableBackupReminder_Click);
			// 
			// labelGlobalDateLockDisabled
			// 
			this.labelGlobalDateLockDisabled.Location = new System.Drawing.Point(38, 77);
			this.labelGlobalDateLockDisabled.Name = "labelGlobalDateLockDisabled";
			this.labelGlobalDateLockDisabled.Size = new System.Drawing.Size(218, 18);
			this.labelGlobalDateLockDisabled.TabIndex = 266;
			this.labelGlobalDateLockDisabled.Text = "(Disabled from Central Management Tool)";
			this.labelGlobalDateLockDisabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelGlobalDateLockDisabled.Visible = false;
			// 
			// checkUserNameManualEntry
			// 
			this.checkUserNameManualEntry.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkUserNameManualEntry.Location = new System.Drawing.Point(8, 220);
			this.checkUserNameManualEntry.Name = "checkUserNameManualEntry";
			this.checkUserNameManualEntry.Size = new System.Drawing.Size(205, 20);
			this.checkUserNameManualEntry.TabIndex = 265;
			this.checkUserNameManualEntry.Text = "Manually enter log on credentials";
			// 
			// textLogOffAfterMinutes
			// 
			this.textLogOffAfterMinutes.Location = new System.Drawing.Point(320, 319);
			this.textLogOffAfterMinutes.Name = "textLogOffAfterMinutes";
			this.textLogOffAfterMinutes.Size = new System.Drawing.Size(29, 20);
			this.textLogOffAfterMinutes.TabIndex = 253;
			this.textLogOffAfterMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(76, 319);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(260, 18);
			this.label3.TabIndex = 263;
			this.label3.Text = "Automatic logoff time in minutes (0 to disable):";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkLogOffWindows
			// 
			this.checkLogOffWindows.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkLogOffWindows.Location = new System.Drawing.Point(8, 124);
			this.checkLogOffWindows.Name = "checkLogOffWindows";
			this.checkLogOffWindows.Size = new System.Drawing.Size(184, 20);
			this.checkLogOffWindows.TabIndex = 255;
			this.checkLogOffWindows.Text = "Log off user on Windows lock";
			// 
			// textDaysLock
			// 
			this.textDaysLock.AutoSize = false;
			this.textDaysLock.Location = new System.Drawing.Point(109, 50);
			this.textDaysLock.Name = "textDaysLock";
			this.textDaysLock.ReadOnly = true;
			this.textDaysLock.Size = new System.Drawing.Size(80, 23);
			this.textDaysLock.TabIndex = 262;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(39, 50);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(64, 23);
			this.label2.TabIndex = 261;
			this.label2.Text = "Lock Days";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkPasswordsMustBeStrong
			// 
			this.checkPasswordsMustBeStrong.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPasswordsMustBeStrong.Location = new System.Drawing.Point(8, 148);
			this.checkPasswordsMustBeStrong.Name = "checkPasswordsMustBeStrong";
			this.checkPasswordsMustBeStrong.Size = new System.Drawing.Size(165, 20);
			this.checkPasswordsMustBeStrong.TabIndex = 257;
			this.checkPasswordsMustBeStrong.Text = "Passwords must be strong";
			this.checkPasswordsMustBeStrong.Click += new System.EventHandler(this.checkPasswordsMustBeStrong_Click);
			// 
			// butChange
			// 
			this.butChange.Location = new System.Drawing.Point(186, 23);
			this.butChange.Name = "butChange";
			this.butChange.Size = new System.Drawing.Size(70, 23);
			this.butChange.TabIndex = 258;
			this.butChange.Text = "Change";
			this.butChange.Click += new System.EventHandler(this.butChange_Click);
			// 
			// textDateLock
			// 
			this.textDateLock.AutoSize = false;
			this.textDateLock.Location = new System.Drawing.Point(109, 23);
			this.textDateLock.Name = "textDateLock";
			this.textDateLock.ReadOnly = true;
			this.textDateLock.Size = new System.Drawing.Size(71, 23);
			this.textDateLock.TabIndex = 260;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(39, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 23);
			this.label1.TabIndex = 259;
			this.label1.Text = "Lock Date";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkCannotEditOwn
			// 
			this.checkCannotEditOwn.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkCannotEditOwn.Location = new System.Drawing.Point(8, 32);
			this.checkCannotEditOwn.Name = "checkCannotEditOwn";
			this.checkCannotEditOwn.Size = new System.Drawing.Size(227, 20);
			this.checkCannotEditOwn.TabIndex = 256;
			this.checkCannotEditOwn.Text = "Users cannot edit their own time card";
			this.checkCannotEditOwn.Click += new System.EventHandler(this.checkCannotEditOwn_Click);
			// 
			// checkTimecardSecurityEnabled
			// 
			this.checkTimecardSecurityEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkTimecardSecurityEnabled.Location = new System.Drawing.Point(8, 8);
			this.checkTimecardSecurityEnabled.Name = "checkTimecardSecurityEnabled";
			this.checkTimecardSecurityEnabled.Size = new System.Drawing.Size(168, 20);
			this.checkTimecardSecurityEnabled.TabIndex = 254;
			this.checkTimecardSecurityEnabled.Text = "Time card security enabled";
			this.checkTimecardSecurityEnabled.Click += new System.EventHandler(this.checkTimecardSecurityEnabled_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.checkDomainLoginEnabled);
			this.groupBox1.Controls.Add(this.textDomainLoginPath);
			this.groupBox1.Controls.Add(this.labelDomainPath);
			this.groupBox1.Controls.Add(this.checkDomainLoginForSigBoxes);
			this.groupBox1.Location = new System.Drawing.Point(90, 374);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(264, 122);
			this.groupBox1.TabIndex = 273;
			this.groupBox1.Text = "Domain Login";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.textDateLock);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.butChange);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.textDaysLock);
			this.groupBox2.Controls.Add(this.labelGlobalDateLockDisabled);
			this.groupBox2.Location = new System.Drawing.Point(90, 504);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(264, 103);
			this.groupBox2.TabIndex = 274;
			this.groupBox2.Text = "Global Lock";
			// 
			// comboGroups
			// 
			this.comboGroups.Location = new System.Drawing.Point(199, 292);
			this.comboGroups.Name = "comboGroups";
			this.comboGroups.Size = new System.Drawing.Size(152, 23);
			this.comboGroups.TabIndex = 275;
			// 
			// labelPermission
			// 
			this.labelPermission.Location = new System.Drawing.Point(81, 292);
			this.labelPermission.Name = "labelPermission";
			this.labelPermission.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.labelPermission.Size = new System.Drawing.Size(112, 23);
			this.labelPermission.TabIndex = 276;
			this.labelPermission.Text = "Default User Group";
			this.labelPermission.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkAllowLogoffOverride
			// 
			this.checkAllowLogoffOverride.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAllowLogoffOverride.Location = new System.Drawing.Point(8, 346);
			this.checkAllowLogoffOverride.Name = "checkAllowLogoffOverride";
			this.checkAllowLogoffOverride.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkAllowLogoffOverride.Size = new System.Drawing.Size(240, 20);
			this.checkAllowLogoffOverride.TabIndex = 277;
			this.checkAllowLogoffOverride.Text = "Allow user override for automatic logoff";
			// 
			// checkCannotEditPastPayPeriods
			// 
			this.checkCannotEditPastPayPeriods.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkCannotEditPastPayPeriods.Location = new System.Drawing.Point(8, 56);
			this.checkCannotEditPastPayPeriods.Name = "checkCannotEditPastPayPeriods";
			this.checkCannotEditPastPayPeriods.Size = new System.Drawing.Size(354, 40);
			this.checkCannotEditPastPayPeriods.TabIndex = 278;
			this.checkCannotEditPastPayPeriods.Text = "Users cannot edit their own time card except current pay period\r\n";
			this.checkCannotEditPastPayPeriods.Click += new System.EventHandler(this.checkCanEditOwnCur_Click);
			// 
			// checkMaintainPatient
			// 
			this.checkMaintainPatient.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkMaintainPatient.Location = new System.Drawing.Point(8, 268);
			this.checkMaintainPatient.Name = "checkMaintainPatient";
			this.checkMaintainPatient.Size = new System.Drawing.Size(279, 20);
			this.checkMaintainPatient.TabIndex = 279;
			this.checkMaintainPatient.Text = "Maintain selected patient when changing users";
			// 
			// checkBadgeLogIn
			// 
			this.checkBadgeLogIn.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkBadgeLogIn.Location = new System.Drawing.Point(8, 244);
			this.checkBadgeLogIn.Name = "checkBadgeLogIn";
			this.checkBadgeLogIn.Size = new System.Drawing.Size(297, 20);
			this.checkBadgeLogIn.TabIndex = 280;
			this.checkBadgeLogIn.Text = "When logging in with a badge, require a password";
			//
			// checkAuditPatientSelected
			//
			this.checkAuditPatientSelected.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAuditPatientSelected.Location = new System.Drawing.Point(8, 615);
			this.checkAuditPatientSelected.Name = "checkAuditPatientSelected";
			this.checkAuditPatientSelected.Size = new System.Drawing.Size(259, 20);
			this.checkAuditPatientSelected.TabIndex = 281;
			this.checkAuditPatientSelected.Text = "Log each patient selection in the Audit Trail";
			//
			// FormGlobalSecurity
			//
			this.ClientSize = new System.Drawing.Size(370, 679);
			this.Controls.Add(this.checkAuditPatientSelected);
			this.Controls.Add(this.checkBadgeLogIn);
			this.Controls.Add(this.checkMaintainPatient);
			this.Controls.Add(this.checkCannotEditPastPayPeriods);
			this.Controls.Add(this.checkAllowLogoffOverride);
			this.Controls.Add(this.labelPermission);
			this.Controls.Add(this.comboGroups);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.checkPasswordForceWeakToStrong);
			this.Controls.Add(this.checkPasswordsStrongIncludeSpecial);
			this.Controls.Add(this.checkDisableBackupReminder);
			this.Controls.Add(this.checkUserNameManualEntry);
			this.Controls.Add(this.textLogOffAfterMinutes);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.checkLogOffWindows);
			this.Controls.Add(this.checkPasswordsMustBeStrong);
			this.Controls.Add(this.checkCannotEditOwn);
			this.Controls.Add(this.checkTimecardSecurityEnabled);
			this.Controls.Add(this.butSave);
			this.Name = "FormGlobalSecurity";
			this.Text = "Global Security Settings";
			this.Load += new System.EventHandler(this.FormGlobalSecurity_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label labelDomainPath;
		private System.Windows.Forms.TextBox textDomainLoginPath;
		private OpenPmsDental.UI.CheckBox checkDomainLoginEnabled;
		private OpenPmsDental.UI.CheckBox checkDomainLoginForSigBoxes;
		private OpenPmsDental.UI.CheckBox checkPasswordForceWeakToStrong;
		private OpenPmsDental.UI.CheckBox checkPasswordsStrongIncludeSpecial;
		private OpenPmsDental.UI.CheckBox checkDisableBackupReminder;
		private System.Windows.Forms.Label labelGlobalDateLockDisabled;
		private OpenPmsDental.UI.CheckBox checkUserNameManualEntry;
		private System.Windows.Forms.TextBox textLogOffAfterMinutes;
		private System.Windows.Forms.Label label3;
		private OpenPmsDental.UI.CheckBox checkLogOffWindows;
		private System.Windows.Forms.TextBox textDaysLock;
		private System.Windows.Forms.Label label2;
		private OpenPmsDental.UI.CheckBox checkPasswordsMustBeStrong;
		private UI.Button butChange;
		private System.Windows.Forms.TextBox textDateLock;
		private System.Windows.Forms.Label label1;
		private OpenPmsDental.UI.CheckBox checkCannotEditOwn;
		private OpenPmsDental.UI.CheckBox checkTimecardSecurityEnabled;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private UI.ComboBox comboGroups;
		private System.Windows.Forms.Label labelPermission;
		private OpenPmsDental.UI.CheckBox checkAllowLogoffOverride;
		private OpenPmsDental.UI.CheckBox checkCannotEditPastPayPeriods;
		private OpenPmsDental.UI.CheckBox checkMaintainPatient;
		private UI.CheckBox checkBadgeLogIn;
		private UI.CheckBox checkAuditPatientSelected;
	}
}