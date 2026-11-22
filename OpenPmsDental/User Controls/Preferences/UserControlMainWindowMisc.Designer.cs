
namespace OpenPmsDental {
	partial class UserControlMainWindowMisc {
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.textWebServiceServerName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.textSyncCode = new System.Windows.Forms.TextBox();
			this.textNumDecimals = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.labelTrackClinic = new System.Windows.Forms.Label();
			this.checkBackupIndexesDisabled = new OpenPmsDental.UI.CheckBox();
			this.checkModuleSwitchBlur = new OpenPmsDental.UI.CheckBox();
			this.textAuditEntries = new OpenPmsDental.ValidNum();
			this.comboTrackClinic = new OpenPmsDental.UI.ComboBox();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.textLanguageAndRegion = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.butLanguages = new OpenPmsDental.UI.Button();
			this.checkImeCompositionCompatibility = new OpenPmsDental.UI.CheckBox();
			this.label6 = new System.Windows.Forms.Label();
			this.butPickLanguageAndRegion = new OpenPmsDental.UI.Button();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.textProcessSigsIntervalInSecs = new OpenPmsDental.ValidNum();
			this.textSignalInactiveMinutes = new OpenPmsDental.ValidNum();
			this.textInactiveAlert = new OpenPmsDental.ValidNum();
			this.textAlertInterval = new OpenPmsDental.ValidNum();
			this.label3 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.checkAuditTrailUseReportingServer = new OpenPmsDental.UI.CheckBox();
			this.checkSubmitExceptions = new OpenPmsDental.UI.CheckBox();
			this.butClearCode = new OpenPmsDental.UI.Button();
			this.butDecimal = new OpenPmsDental.UI.Button();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textWebServiceServerName
			// 
			this.textWebServiceServerName.Location = new System.Drawing.Point(307, 340);
			this.textWebServiceServerName.Name = "textWebServiceServerName";
			this.textWebServiceServerName.Size = new System.Drawing.Size(164, 23);
			this.textWebServiceServerName.TabIndex = 248;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(217, 340);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(84, 23);
			this.label2.TabIndex = 249;
			this.label2.Text = "Update Server";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(172, 430);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(220, 23);
			this.label9.TabIndex = 259;
			this.label9.Text = "Number of Audit Trail entries displayed";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(223, 282);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(120, 23);
			this.label8.TabIndex = 257;
			this.label8.Text = "Sync code for CEMT";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textSyncCode
			// 
			this.textSyncCode.Location = new System.Drawing.Point(349, 282);
			this.textSyncCode.Name = "textSyncCode";
			this.textSyncCode.ReadOnly = true;
			this.textSyncCode.Size = new System.Drawing.Size(72, 23);
			this.textSyncCode.TabIndex = 256;
			// 
			// textNumDecimals
			// 
			this.textNumDecimals.Location = new System.Drawing.Point(396, 311);
			this.textNumDecimals.Name = "textNumDecimals";
			this.textNumDecimals.ReadOnly = true;
			this.textNumDecimals.Size = new System.Drawing.Size(48, 23);
			this.textNumDecimals.TabIndex = 253;
			this.textNumDecimals.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(166, 311);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(224, 23);
			this.label7.TabIndex = 254;
			this.label7.Text = "Currency number of digits after decimal";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelTrackClinic
			// 
			this.labelTrackClinic.Location = new System.Drawing.Point(189, 375);
			this.labelTrackClinic.Name = "labelTrackClinic";
			this.labelTrackClinic.Size = new System.Drawing.Size(112, 23);
			this.labelTrackClinic.TabIndex = 252;
			this.labelTrackClinic.Text = "Track Last Clinic By";
			this.labelTrackClinic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkBackupIndexesDisabled
			// 
			this.checkBackupIndexesDisabled.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkBackupIndexesDisabled.Checked = true;
			this.checkBackupIndexesDisabled.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBackupIndexesDisabled.Location = new System.Drawing.Point(8, 483);
			this.checkBackupIndexesDisabled.Name = "checkBackupIndexesDisabled";
			this.checkBackupIndexesDisabled.Size = new System.Drawing.Size(282, 20);
			this.checkBackupIndexesDisabled.TabIndex = 267;
			this.checkBackupIndexesDisabled.Text = "Disable indexes to speed up automatic backups";
			//
			// checkModuleSwitchBlur
			//
			this.checkModuleSwitchBlur.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkModuleSwitchBlur.Location = new System.Drawing.Point(8, 507);
			this.checkModuleSwitchBlur.Name = "checkModuleSwitchBlur";
			this.checkModuleSwitchBlur.Size = new System.Drawing.Size(394, 20);
			this.checkModuleSwitchBlur.TabIndex = 268;
			this.checkModuleSwitchBlur.Text = "Blur when switching between modules to prevent accidental clicking";
			// 
			// textAuditEntries
			// 
			this.textAuditEntries.Location = new System.Drawing.Point(398, 430);
			this.textAuditEntries.MaxLength = 5;
			this.textAuditEntries.MaxVal = 10000;
			this.textAuditEntries.MinVal = 1;
			this.textAuditEntries.Name = "textAuditEntries";
			this.textAuditEntries.Size = new System.Drawing.Size(72, 23);
			this.textAuditEntries.TabIndex = 260;
			this.textAuditEntries.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// comboTrackClinic
			// 
			this.comboTrackClinic.Location = new System.Drawing.Point(307, 375);
			this.comboTrackClinic.Name = "comboTrackClinic";
			this.comboTrackClinic.Size = new System.Drawing.Size(128, 23);
			this.comboTrackClinic.TabIndex = 251;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.textLanguageAndRegion);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.butLanguages);
			this.groupBox2.Controls.Add(this.checkImeCompositionCompatibility);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.butPickLanguageAndRegion);
			this.groupBox2.Location = new System.Drawing.Point(8, 161);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(478, 109);
			this.groupBox2.TabIndex = 266;
			this.groupBox2.Text = "Language";
			// 
			// textLanguageAndRegion
			// 
			this.textLanguageAndRegion.Location = new System.Drawing.Point(272, 52);
			this.textLanguageAndRegion.Name = "textLanguageAndRegion";
			this.textLanguageAndRegion.ReadOnly = true;
			this.textLanguageAndRegion.Size = new System.Drawing.Size(164, 23);
			this.textLanguageAndRegion.TabIndex = 204;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(209, 23);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(160, 23);
			this.label4.TabIndex = 64;
			this.label4.Text = "Languages used by patients";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butLanguages
			// 
			this.butLanguages.Location = new System.Drawing.Point(375, 23);
			this.butLanguages.Name = "butLanguages";
			this.butLanguages.Size = new System.Drawing.Size(88, 23);
			this.butLanguages.TabIndex = 63;
			this.butLanguages.Text = "Edit Languages";
			this.butLanguages.Click += new System.EventHandler(this.butLanguages_Click);
			// 
			// checkImeCompositionCompatibility
			// 
			this.checkImeCompositionCompatibility.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkImeCompositionCompatibility.Location = new System.Drawing.Point(8, 81);
			this.checkImeCompositionCompatibility.Name = "checkImeCompositionCompatibility";
			this.checkImeCompositionCompatibility.Size = new System.Drawing.Size(416, 20);
			this.checkImeCompositionCompatibility.TabIndex = 203;
			this.checkImeCompositionCompatibility.Text = "Text boxes use foreign language Input Method Editor (IME) composition";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(50, 52);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(216, 23);
			this.label6.TabIndex = 205;
			this.label6.Text = "Language and region used by program";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butPickLanguageAndRegion
			// 
			this.butPickLanguageAndRegion.Location = new System.Drawing.Point(443, 52);
			this.butPickLanguageAndRegion.Name = "butPickLanguageAndRegion";
			this.butPickLanguageAndRegion.Size = new System.Drawing.Size(23, 21);
			this.butPickLanguageAndRegion.TabIndex = 206;
			this.butPickLanguageAndRegion.Text = "...";
			this.butPickLanguageAndRegion.Click += new System.EventHandler(this.butPickLanguageAndRegion_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.textProcessSigsIntervalInSecs);
			this.groupBox1.Controls.Add(this.textSignalInactiveMinutes);
			this.groupBox1.Controls.Add(this.textInactiveAlert);
			this.groupBox1.Controls.Add(this.textAlertInterval);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.label11);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(478, 141);
			this.groupBox1.TabIndex = 265;
			this.groupBox1.Text = "Signal Interval";
			// 
			// textProcessSigsIntervalInSecs
			// 
			this.textProcessSigsIntervalInSecs.Location = new System.Drawing.Point(389, 23);
			this.textProcessSigsIntervalInSecs.MaxVal = 1000000;
			this.textProcessSigsIntervalInSecs.MinVal = 1;
			this.textProcessSigsIntervalInSecs.Name = "textProcessSigsIntervalInSecs";
			this.textProcessSigsIntervalInSecs.ShowZero = false;
			this.textProcessSigsIntervalInSecs.Size = new System.Drawing.Size(72, 23);
			this.textProcessSigsIntervalInSecs.TabIndex = 57;
			this.textProcessSigsIntervalInSecs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textProcessSigsIntervalInSecs.Validating += new System.ComponentModel.CancelEventHandler(this.textProcessSigsIntervalInSecs_Validating);
			// 
			// textSignalInactiveMinutes
			// 
			this.textSignalInactiveMinutes.Location = new System.Drawing.Point(389, 52);
			this.textSignalInactiveMinutes.MaxVal = 1000000;
			this.textSignalInactiveMinutes.Name = "textSignalInactiveMinutes";
			this.textSignalInactiveMinutes.ShowZero = false;
			this.textSignalInactiveMinutes.Size = new System.Drawing.Size(72, 23);
			this.textSignalInactiveMinutes.TabIndex = 201;
			this.textSignalInactiveMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textSignalInactiveMinutes.Validating += new System.ComponentModel.CancelEventHandler(this.textSignalInactiveMinutes_Validating);
			// 
			// textInactiveAlert
			// 
			this.textInactiveAlert.Location = new System.Drawing.Point(389, 110);
			this.textInactiveAlert.MaxVal = 1000000;
			this.textInactiveAlert.MinVal = 1;
			this.textInactiveAlert.Name = "textInactiveAlert";
			this.textInactiveAlert.ShowZero = false;
			this.textInactiveAlert.Size = new System.Drawing.Size(72, 23);
			this.textInactiveAlert.TabIndex = 240;
			this.textInactiveAlert.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textAlertInterval
			// 
			this.textAlertInterval.Location = new System.Drawing.Point(389, 81);
			this.textAlertInterval.MaxVal = 1000000;
			this.textAlertInterval.MinVal = 1;
			this.textAlertInterval.Name = "textAlertInterval";
			this.textAlertInterval.ShowZero = false;
			this.textAlertInterval.Size = new System.Drawing.Size(72, 23);
			this.textAlertInterval.TabIndex = 238;
			this.textAlertInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(71, 23);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(312, 23);
			this.label3.TabIndex = 56;
			this.label3.Text = "Process signal interval in seconds.  Leave blank to disable";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(39, 52);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(344, 23);
			this.label5.TabIndex = 200;
			this.label5.Text = "Disable signal interval after this many minutes of user inactivity\r\n";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(211, 81);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(172, 23);
			this.label11.TabIndex = 237;
			this.label11.Text = "Check alert interval in seconds";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(47, 110);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(336, 23);
			this.label10.TabIndex = 239;
			this.label10.Text = "Disable alert interval after this many minutes of user inactivity\r\n";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkAuditTrailUseReportingServer
			// 
			this.checkAuditTrailUseReportingServer.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAuditTrailUseReportingServer.Location = new System.Drawing.Point(8, 459);
			this.checkAuditTrailUseReportingServer.Name = "checkAuditTrailUseReportingServer";
			this.checkAuditTrailUseReportingServer.Size = new System.Drawing.Size(201, 20);
			this.checkAuditTrailUseReportingServer.TabIndex = 264;
			this.checkAuditTrailUseReportingServer.Text = "Audit Trail uses Reporting Server";
			// 
			// checkSubmitExceptions
			// 
			this.checkSubmitExceptions.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkSubmitExceptions.Location = new System.Drawing.Point(8, 404);
			this.checkSubmitExceptions.Name = "checkSubmitExceptions";
			this.checkSubmitExceptions.Size = new System.Drawing.Size(261, 20);
			this.checkSubmitExceptions.TabIndex = 261;
			this.checkSubmitExceptions.Text = "Automatically submit unhandled exceptions";
			// 
			// butClearCode
			// 
			this.butClearCode.Location = new System.Drawing.Point(430, 282);
			this.butClearCode.Name = "butClearCode";
			this.butClearCode.Size = new System.Drawing.Size(43, 21);
			this.butClearCode.TabIndex = 258;
			this.butClearCode.Text = "Clear";
			this.butClearCode.Click += new System.EventHandler(this.butClearCode_Click);
			// 
			// butDecimal
			// 
			this.butDecimal.Location = new System.Drawing.Point(451, 311);
			this.butDecimal.Name = "butDecimal";
			this.butDecimal.Size = new System.Drawing.Size(23, 21);
			this.butDecimal.TabIndex = 255;
			this.butDecimal.Text = "...";
			this.butDecimal.Click += new System.EventHandler(this.butDecimal_Click);
			// 
			// UserControlMainWindowMisc
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.checkModuleSwitchBlur);
			this.Controls.Add(this.checkBackupIndexesDisabled);
			this.Controls.Add(this.textWebServiceServerName);
			this.Controls.Add(this.textAuditEntries);
			this.Controls.Add(this.textSyncCode);
			this.Controls.Add(this.textNumDecimals);
			this.Controls.Add(this.comboTrackClinic);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.checkAuditTrailUseReportingServer);
			this.Controls.Add(this.checkSubmitExceptions);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.butClearCode);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.butDecimal);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.labelTrackClinic);
			this.Name = "UserControlMainWindowMisc";
			this.Size = new System.Drawing.Size(494, 660);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox textWebServiceServerName;
		private System.Windows.Forms.Label label2;
		private UI.GroupBox groupBox2;
		private System.Windows.Forms.Label label4;
		private UI.Button butLanguages;
		private UI.CheckBox checkImeCompositionCompatibility;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textLanguageAndRegion;
		private UI.Button butPickLanguageAndRegion;
		private UI.GroupBox groupBox1;
		private System.Windows.Forms.Label label3;
		private ValidNum textProcessSigsIntervalInSecs;
		private System.Windows.Forms.Label label5;
		private ValidNum textSignalInactiveMinutes;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label10;
		private ValidNum textAlertInterval;
		private UI.CheckBox checkAuditTrailUseReportingServer;
		private UI.CheckBox checkSubmitExceptions;
		private ValidNum textAuditEntries;
		private System.Windows.Forms.Label label9;
		private UI.Button butClearCode;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox textSyncCode;
		private UI.Button butDecimal;
		private System.Windows.Forms.TextBox textNumDecimals;
		private System.Windows.Forms.Label label7;
		private UI.ComboBox comboTrackClinic;
		private System.Windows.Forms.Label labelTrackClinic;
		private ValidNum textInactiveAlert;
		private UI.CheckBox checkBackupIndexesDisabled;
		private UI.CheckBox checkModuleSwitchBlur;
	}
}
