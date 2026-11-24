
namespace OpenPmsDental {
	partial class UserControlEnterpriseGeneral {
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
			this.checkDatabaseGlobalVariablesDontSet = new OpenPmsDental.UI.CheckBox();
			this.checkEnterpriseCommlogOmitDefaults = new OpenPmsDental.UI.CheckBox();
			this.checkEnableEmailAddressAutoComplete = new OpenPmsDental.UI.CheckBox();
			this.checkUpdateAlterLargeTablesDirectly = new OpenPmsDental.UI.CheckBox();
			this.groupPatientSelect = new OpenPmsDental.UI.GroupBox();
			this.checkEnterpriseAllowRefreshWhileTyping = new OpenPmsDental.UI.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textEnterpriseExactMatchPhoneNumDigits = new OpenPmsDental.ValidNum();
			this.checkEnterpriseExactMatchPhone = new OpenPmsDental.UI.CheckBox();
			this.checkPatientSelectFilterRestrictedClinics = new OpenPmsDental.UI.CheckBox();
			this.butSyncPhNums = new OpenPmsDental.UI.Button();
			this.checkPatientPhoneUsePhonenumberTable = new OpenPmsDental.UI.CheckBox();
			this.checkPatientSelectSearchWithEmptyParams = new OpenPmsDental.UI.CheckBox();
			this.labelPatSelectPauseMs = new System.Windows.Forms.Label();
			this.textPatientSelectSearchPauseMs = new OpenPmsDental.ValidNum();
			this.labelPatSelectMinChars = new System.Windows.Forms.Label();
			this.textPatientSelectSearchMinChars = new OpenPmsDental.ValidNum();
			this.checkUserNameManualEntry = new OpenPmsDental.UI.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.checkUpdateStreamlinePassword = new OpenPmsDental.UI.CheckBox();
			this.textProcessSigsIntervalInSecs = new OpenPmsDental.ValidNum();
			this.textSecurityLogOffAfterMinutes = new System.Windows.Forms.TextBox();
			this.checkDatabaseMaintenanceSkipCheckTable = new OpenPmsDental.UI.CheckBox();
			this.checkDatabaseMaintenanceDisableOptimize = new OpenPmsDental.UI.CheckBox();
			this.label10 = new System.Windows.Forms.Label();
			this.checkPasswordsMustBeStrong = new OpenPmsDental.UI.CheckBox();
			this.textSignalInactiveMinutes = new OpenPmsDental.ValidNum();
			this.label12 = new System.Windows.Forms.Label();
			this.checkSecurityLockIncludesAdmin = new OpenPmsDental.UI.CheckBox();
			this.checkPasswordsWeakChangeToStrong = new OpenPmsDental.UI.CheckBox();
			this.checkPasswordsStrongIncludeSpecial = new OpenPmsDental.UI.CheckBox();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.textSecurityLockDate = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.butChange = new OpenPmsDental.UI.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.textSecurityLockDays = new System.Windows.Forms.TextBox();
			this.labelGlobalDateLockDisabled = new System.Windows.Forms.Label();
			this.checkHasClinicsEnabled = new OpenPmsDental.UI.CheckBox();
			this.groupPatientSelect.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkDatabaseGlobalVariablesDontSet
			// 
			this.checkDatabaseGlobalVariablesDontSet.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkDatabaseGlobalVariablesDontSet.Location = new System.Drawing.Point(8, 756);
			this.checkDatabaseGlobalVariablesDontSet.Name = "checkDatabaseGlobalVariablesDontSet";
			this.checkDatabaseGlobalVariablesDontSet.Size = new System.Drawing.Size(373, 20);
			this.checkDatabaseGlobalVariablesDontSet.TabIndex = 314;
			this.checkDatabaseGlobalVariablesDontSet.Text = "Disable setting SQL global variables (Used for hosted databases)";
			// 
			// checkEnterpriseCommlogOmitDefaults
			// 
			this.checkEnterpriseCommlogOmitDefaults.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEnterpriseCommlogOmitDefaults.Location = new System.Drawing.Point(8, 732);
			this.checkEnterpriseCommlogOmitDefaults.Name = "checkEnterpriseCommlogOmitDefaults";
			this.checkEnterpriseCommlogOmitDefaults.Size = new System.Drawing.Size(200, 20);
			this.checkEnterpriseCommlogOmitDefaults.TabIndex = 313;
			this.checkEnterpriseCommlogOmitDefaults.Text = "Commlog fields blank by default";
			// 
			// checkEnableEmailAddressAutoComplete
			// 
			this.checkEnableEmailAddressAutoComplete.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEnableEmailAddressAutoComplete.Location = new System.Drawing.Point(8, 708);
			this.checkEnableEmailAddressAutoComplete.Name = "checkEnableEmailAddressAutoComplete";
			this.checkEnableEmailAddressAutoComplete.Size = new System.Drawing.Size(221, 20);
			this.checkEnableEmailAddressAutoComplete.TabIndex = 312;
			this.checkEnableEmailAddressAutoComplete.Text = "Enable email address auto-complete";
			// 
			// checkUpdateAlterLargeTablesDirectly
			// 
			this.checkUpdateAlterLargeTablesDirectly.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkUpdateAlterLargeTablesDirectly.Enabled = false;
			this.checkUpdateAlterLargeTablesDirectly.Location = new System.Drawing.Point(8, 353);
			this.checkUpdateAlterLargeTablesDirectly.Name = "checkUpdateAlterLargeTablesDirectly";
			this.checkUpdateAlterLargeTablesDirectly.Size = new System.Drawing.Size(200, 20);
			this.checkUpdateAlterLargeTablesDirectly.TabIndex = 311;
			this.checkUpdateAlterLargeTablesDirectly.Text = "Update alter large tables directly";
			// 
			// groupPatientSelect
			// 
			this.groupPatientSelect.Controls.Add(this.checkEnterpriseAllowRefreshWhileTyping);
			this.groupPatientSelect.Controls.Add(this.label2);
			this.groupPatientSelect.Controls.Add(this.textEnterpriseExactMatchPhoneNumDigits);
			this.groupPatientSelect.Controls.Add(this.checkEnterpriseExactMatchPhone);
			this.groupPatientSelect.Controls.Add(this.checkPatientSelectFilterRestrictedClinics);
			this.groupPatientSelect.Controls.Add(this.butSyncPhNums);
			this.groupPatientSelect.Controls.Add(this.checkPatientPhoneUsePhonenumberTable);
			this.groupPatientSelect.Controls.Add(this.checkPatientSelectSearchWithEmptyParams);
			this.groupPatientSelect.Controls.Add(this.labelPatSelectPauseMs);
			this.groupPatientSelect.Controls.Add(this.textPatientSelectSearchPauseMs);
			this.groupPatientSelect.Controls.Add(this.labelPatSelectMinChars);
			this.groupPatientSelect.Controls.Add(this.textPatientSelectSearchMinChars);
			this.groupPatientSelect.Location = new System.Drawing.Point(8, 453);
			this.groupPatientSelect.Name = "groupPatientSelect";
			this.groupPatientSelect.Size = new System.Drawing.Size(478, 247);
			this.groupPatientSelect.TabIndex = 310;
			this.groupPatientSelect.Text = "Patient Select";
			// 
			// checkEnterpriseAllowRefreshWhileTyping
			// 
			this.checkEnterpriseAllowRefreshWhileTyping.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEnterpriseAllowRefreshWhileTyping.Location = new System.Drawing.Point(8, 219);
			this.checkEnterpriseAllowRefreshWhileTyping.Name = "checkEnterpriseAllowRefreshWhileTyping";
			this.checkEnterpriseAllowRefreshWhileTyping.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkEnterpriseAllowRefreshWhileTyping.Size = new System.Drawing.Size(310, 20);
			this.checkEnterpriseAllowRefreshWhileTyping.TabIndex = 292;
			this.checkEnterpriseAllowRefreshWhileTyping.Text = "Allow Refresh While Typing in Select Patient Window";
			this.checkEnterpriseAllowRefreshWhileTyping.Click += new System.EventHandler(this.checkEnterpriseAllowRefreshWhileTyping_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 192);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(218, 23);
			this.label2.TabIndex = 294;
			this.label2.Text = "# of digits";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textEnterpriseExactMatchPhoneNumDigits
			// 
			this.textEnterpriseExactMatchPhoneNumDigits.Enabled = false;
			this.textEnterpriseExactMatchPhoneNumDigits.Location = new System.Drawing.Point(248, 192);
			this.textEnterpriseExactMatchPhoneNumDigits.MaxVal = 25;
			this.textEnterpriseExactMatchPhoneNumDigits.MinVal = 1;
			this.textEnterpriseExactMatchPhoneNumDigits.Name = "textEnterpriseExactMatchPhoneNumDigits";
			this.textEnterpriseExactMatchPhoneNumDigits.Size = new System.Drawing.Size(40, 20);
			this.textEnterpriseExactMatchPhoneNumDigits.TabIndex = 293;
			this.textEnterpriseExactMatchPhoneNumDigits.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// checkEnterpriseExactMatchPhone
			// 
			this.checkEnterpriseExactMatchPhone.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEnterpriseExactMatchPhone.Location = new System.Drawing.Point(8, 168);
			this.checkEnterpriseExactMatchPhone.Name = "checkEnterpriseExactMatchPhone";
			this.checkEnterpriseExactMatchPhone.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkEnterpriseExactMatchPhone.Size = new System.Drawing.Size(265, 20);
			this.checkEnterpriseExactMatchPhone.TabIndex = 292;
			this.checkEnterpriseExactMatchPhone.Text = "Only match patients by exact phone number";
			this.checkEnterpriseExactMatchPhone.CheckedChanged += new System.EventHandler(this.checkExactMatchPhoneNum_CheckedChanged);
			// 
			// checkPatientSelectFilterRestrictedClinics
			// 
			this.checkPatientSelectFilterRestrictedClinics.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPatientSelectFilterRestrictedClinics.Location = new System.Drawing.Point(8, 144);
			this.checkPatientSelectFilterRestrictedClinics.Name = "checkPatientSelectFilterRestrictedClinics";
			this.checkPatientSelectFilterRestrictedClinics.Size = new System.Drawing.Size(356, 20);
			this.checkPatientSelectFilterRestrictedClinics.TabIndex = 290;
			this.checkPatientSelectFilterRestrictedClinics.Text = "Hide patients from restricted clinics when viewing \"All\" clinics";
			this.checkPatientSelectFilterRestrictedClinics.Click += new System.EventHandler(this.checkPatientSelectFilterRestrictedClinics_Click);
			// 
			// butSyncPhNums
			// 
			this.butSyncPhNums.Location = new System.Drawing.Point(399, 119);
			this.butSyncPhNums.Name = "butSyncPhNums";
			this.butSyncPhNums.Size = new System.Drawing.Size(49, 21);
			this.butSyncPhNums.TabIndex = 292;
			this.butSyncPhNums.Text = "Sync";
			this.butSyncPhNums.Click += new System.EventHandler(this.butSyncPhNums_Click);
			// 
			// checkPatientPhoneUsePhonenumberTable
			// 
			this.checkPatientPhoneUsePhonenumberTable.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPatientPhoneUsePhonenumberTable.Location = new System.Drawing.Point(8, 119);
			this.checkPatientPhoneUsePhonenumberTable.Name = "checkPatientPhoneUsePhonenumberTable";
			this.checkPatientPhoneUsePhonenumberTable.Size = new System.Drawing.Size(385, 20);
			this.checkPatientPhoneUsePhonenumberTable.TabIndex = 291;
			this.checkPatientPhoneUsePhonenumberTable.Text = "Store patient phone numbers in a separate table for patient search";
			this.checkPatientPhoneUsePhonenumberTable.Click += new System.EventHandler(this.checkPatientPhoneUsePhoneNumberTable_Click);
			// 
			// checkPatientSelectSearchWithEmptyParams
			// 
			this.checkPatientSelectSearchWithEmptyParams.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPatientSelectSearchWithEmptyParams.Checked = true;
			this.checkPatientSelectSearchWithEmptyParams.CheckState = System.Windows.Forms.CheckState.Indeterminate;
			this.checkPatientSelectSearchWithEmptyParams.Location = new System.Drawing.Point(8, 95);
			this.checkPatientSelectSearchWithEmptyParams.Name = "checkPatientSelectSearchWithEmptyParams";
			this.checkPatientSelectSearchWithEmptyParams.Size = new System.Drawing.Size(275, 20);
			this.checkPatientSelectSearchWithEmptyParams.TabIndex = 290;
			this.checkPatientSelectSearchWithEmptyParams.Text = "Search and fill grid with all empty search fields";
			// 
			// labelPatSelectPauseMs
			// 
			this.labelPatSelectPauseMs.Location = new System.Drawing.Point(24, 59);
			this.labelPatSelectPauseMs.Name = "labelPatSelectPauseMs";
			this.labelPatSelectPauseMs.Size = new System.Drawing.Size(218, 32);
			this.labelPatSelectPauseMs.TabIndex = 281;
			this.labelPatSelectPauseMs.Text = "Wait time in milliseconds after a character is entered before grid is filled";
			this.labelPatSelectPauseMs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textPatientSelectSearchPauseMs
			// 
			this.textPatientSelectSearchPauseMs.Location = new System.Drawing.Point(248, 63);
			this.textPatientSelectSearchPauseMs.MaxVal = 10000;
			this.textPatientSelectSearchPauseMs.MinVal = 1;
			this.textPatientSelectSearchPauseMs.Name = "textPatientSelectSearchPauseMs";
			this.textPatientSelectSearchPauseMs.Size = new System.Drawing.Size(40, 20);
			this.textPatientSelectSearchPauseMs.TabIndex = 280;
			this.textPatientSelectSearchPauseMs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// labelPatSelectMinChars
			// 
			this.labelPatSelectMinChars.Location = new System.Drawing.Point(24, 23);
			this.labelPatSelectMinChars.Name = "labelPatSelectMinChars";
			this.labelPatSelectMinChars.Size = new System.Drawing.Size(218, 32);
			this.labelPatSelectMinChars.TabIndex = 279;
			this.labelPatSelectMinChars.Text = "The number of characters entered into the search fields before filling the grid";
			this.labelPatSelectMinChars.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textPatientSelectSearchMinChars
			// 
			this.textPatientSelectSearchMinChars.Location = new System.Drawing.Point(248, 27);
			this.textPatientSelectSearchMinChars.MaxVal = 10;
			this.textPatientSelectSearchMinChars.MinVal = 1;
			this.textPatientSelectSearchMinChars.Name = "textPatientSelectSearchMinChars";
			this.textPatientSelectSearchMinChars.Size = new System.Drawing.Size(40, 20);
			this.textPatientSelectSearchMinChars.TabIndex = 278;
			this.textPatientSelectSearchMinChars.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// checkUserNameManualEntry
			// 
			this.checkUserNameManualEntry.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkUserNameManualEntry.Location = new System.Drawing.Point(8, 140);
			this.checkUserNameManualEntry.Name = "checkUserNameManualEntry";
			this.checkUserNameManualEntry.Size = new System.Drawing.Size(205, 20);
			this.checkUserNameManualEntry.TabIndex = 309;
			this.checkUserNameManualEntry.Text = "Manually enter log on credentials";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 413);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(226, 32);
			this.label3.TabIndex = 298;
			this.label3.Text = "Process signal interval in seconds.  Leave blank to disable autorefresh";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkUpdateStreamlinePassword
			// 
			this.checkUpdateStreamlinePassword.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkUpdateStreamlinePassword.Enabled = false;
			this.checkUpdateStreamlinePassword.Location = new System.Drawing.Point(8, 329);
			this.checkUpdateStreamlinePassword.Name = "checkUpdateStreamlinePassword";
			this.checkUpdateStreamlinePassword.Size = new System.Drawing.Size(179, 20);
			this.checkUpdateStreamlinePassword.TabIndex = 307;
			this.checkUpdateStreamlinePassword.Text = "Update Streamline Password";
			// 
			// textProcessSigsIntervalInSecs
			// 
			this.textProcessSigsIntervalInSecs.Location = new System.Drawing.Point(256, 417);
			this.textProcessSigsIntervalInSecs.MaxVal = 1000000;
			this.textProcessSigsIntervalInSecs.MinVal = 1;
			this.textProcessSigsIntervalInSecs.Name = "textProcessSigsIntervalInSecs";
			this.textProcessSigsIntervalInSecs.ShowZero = false;
			this.textProcessSigsIntervalInSecs.Size = new System.Drawing.Size(72, 23);
			this.textProcessSigsIntervalInSecs.TabIndex = 300;
			this.textProcessSigsIntervalInSecs.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textProcessSigsIntervalInSecs.Validating += new System.ComponentModel.CancelEventHandler(this.textProcessSigsIntervalInSecs_Validating);
			// 
			// textSecurityLogOffAfterMinutes
			// 
			this.textSecurityLogOffAfterMinutes.Location = new System.Drawing.Point(256, 108);
			this.textSecurityLogOffAfterMinutes.Name = "textSecurityLogOffAfterMinutes";
			this.textSecurityLogOffAfterMinutes.Size = new System.Drawing.Size(28, 23);
			this.textSecurityLogOffAfterMinutes.TabIndex = 306;
			this.textSecurityLogOffAfterMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// checkDatabaseMaintenanceSkipCheckTable
			// 
			this.checkDatabaseMaintenanceSkipCheckTable.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkDatabaseMaintenanceSkipCheckTable.Enabled = false;
			this.checkDatabaseMaintenanceSkipCheckTable.Location = new System.Drawing.Point(8, 281);
			this.checkDatabaseMaintenanceSkipCheckTable.Name = "checkDatabaseMaintenanceSkipCheckTable";
			this.checkDatabaseMaintenanceSkipCheckTable.Size = new System.Drawing.Size(145, 20);
			this.checkDatabaseMaintenanceSkipCheckTable.TabIndex = 295;
			this.checkDatabaseMaintenanceSkipCheckTable.Text = "DBM Skip Check Table";
			// 
			// checkDatabaseMaintenanceDisableOptimize
			// 
			this.checkDatabaseMaintenanceDisableOptimize.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkDatabaseMaintenanceDisableOptimize.Enabled = false;
			this.checkDatabaseMaintenanceDisableOptimize.Location = new System.Drawing.Point(8, 257);
			this.checkDatabaseMaintenanceDisableOptimize.Name = "checkDatabaseMaintenanceDisableOptimize";
			this.checkDatabaseMaintenanceDisableOptimize.Size = new System.Drawing.Size(145, 20);
			this.checkDatabaseMaintenanceDisableOptimize.TabIndex = 294;
			this.checkDatabaseMaintenanceDisableOptimize.Text = "DBM Disable Optimize";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(24, 104);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(226, 32);
			this.label10.TabIndex = 308;
			this.label10.Text = "Automatic logoff time in minutes (0 to disable)";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkPasswordsMustBeStrong
			// 
			this.checkPasswordsMustBeStrong.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPasswordsMustBeStrong.Location = new System.Drawing.Point(8, 8);
			this.checkPasswordsMustBeStrong.Name = "checkPasswordsMustBeStrong";
			this.checkPasswordsMustBeStrong.Size = new System.Drawing.Size(165, 20);
			this.checkPasswordsMustBeStrong.TabIndex = 297;
			this.checkPasswordsMustBeStrong.Text = "Passwords must be strong";
			// 
			// textSignalInactiveMinutes
			// 
			this.textSignalInactiveMinutes.Location = new System.Drawing.Point(256, 381);
			this.textSignalInactiveMinutes.MaxVal = 1000000;
			this.textSignalInactiveMinutes.MinVal = 1;
			this.textSignalInactiveMinutes.Name = "textSignalInactiveMinutes";
			this.textSignalInactiveMinutes.ShowZero = false;
			this.textSignalInactiveMinutes.Size = new System.Drawing.Size(72, 23);
			this.textSignalInactiveMinutes.TabIndex = 304;
			this.textSignalInactiveMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textSignalInactiveMinutes.Validating += new System.ComponentModel.CancelEventHandler(this.textSignalInactiveMinutes_Validating);
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(24, 377);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(226, 32);
			this.label12.TabIndex = 303;
			this.label12.Text = "Disable signal interval after this many minutes of user inactivity";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkSecurityLockIncludesAdmin
			// 
			this.checkSecurityLockIncludesAdmin.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkSecurityLockIncludesAdmin.Location = new System.Drawing.Point(8, 80);
			this.checkSecurityLockIncludesAdmin.Name = "checkSecurityLockIncludesAdmin";
			this.checkSecurityLockIncludesAdmin.Size = new System.Drawing.Size(177, 20);
			this.checkSecurityLockIncludesAdmin.TabIndex = 305;
			this.checkSecurityLockIncludesAdmin.Text = "Lock includes administrators";
			// 
			// checkPasswordsWeakChangeToStrong
			// 
			this.checkPasswordsWeakChangeToStrong.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPasswordsWeakChangeToStrong.Location = new System.Drawing.Point(8, 56);
			this.checkPasswordsWeakChangeToStrong.Name = "checkPasswordsWeakChangeToStrong";
			this.checkPasswordsWeakChangeToStrong.Size = new System.Drawing.Size(221, 20);
			this.checkPasswordsWeakChangeToStrong.TabIndex = 301;
			this.checkPasswordsWeakChangeToStrong.Text = "Force password change if not strong";
			// 
			// checkPasswordsStrongIncludeSpecial
			// 
			this.checkPasswordsStrongIncludeSpecial.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPasswordsStrongIncludeSpecial.Location = new System.Drawing.Point(8, 32);
			this.checkPasswordsStrongIncludeSpecial.Name = "checkPasswordsStrongIncludeSpecial";
			this.checkPasswordsStrongIncludeSpecial.Size = new System.Drawing.Size(256, 20);
			this.checkPasswordsStrongIncludeSpecial.TabIndex = 299;
			this.checkPasswordsStrongIncludeSpecial.Text = "Strong passwords require special character";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.textSecurityLockDate);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.butChange);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.textSecurityLockDays);
			this.groupBox2.Controls.Add(this.labelGlobalDateLockDisabled);
			this.groupBox2.Location = new System.Drawing.Point(8, 168);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(478, 81);
			this.groupBox2.TabIndex = 302;
			this.groupBox2.Text = "Global Lock";
			// 
			// textSecurityLockDate
			// 
			this.textSecurityLockDate.AutoSize = false;
			this.textSecurityLockDate.Location = new System.Drawing.Point(248, 23);
			this.textSecurityLockDate.Name = "textSecurityLockDate";
			this.textSecurityLockDate.ReadOnly = true;
			this.textSecurityLockDate.Size = new System.Drawing.Size(80, 23);
			this.textSecurityLockDate.TabIndex = 260;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(24, 23);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(218, 23);
			this.label7.TabIndex = 259;
			this.label7.Text = "Lock Date";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// butChange
			// 
			this.butChange.Location = new System.Drawing.Point(334, 23);
			this.butChange.Name = "butChange";
			this.butChange.Size = new System.Drawing.Size(70, 23);
			this.butChange.TabIndex = 258;
			this.butChange.Text = "Change";
			this.butChange.Click += new System.EventHandler(this.butChange_Click);
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(24, 50);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(218, 23);
			this.label8.TabIndex = 261;
			this.label8.Text = "Lock Days";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textSecurityLockDays
			// 
			this.textSecurityLockDays.Location = new System.Drawing.Point(248, 50);
			this.textSecurityLockDays.Name = "textSecurityLockDays";
			this.textSecurityLockDays.ReadOnly = true;
			this.textSecurityLockDays.Size = new System.Drawing.Size(80, 23);
			this.textSecurityLockDays.TabIndex = 262;
			this.textSecurityLockDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// labelGlobalDateLockDisabled
			// 
			this.labelGlobalDateLockDisabled.Location = new System.Drawing.Point(334, 47);
			this.labelGlobalDateLockDisabled.Name = "labelGlobalDateLockDisabled";
			this.labelGlobalDateLockDisabled.Size = new System.Drawing.Size(136, 32);
			this.labelGlobalDateLockDisabled.TabIndex = 266;
			this.labelGlobalDateLockDisabled.Text = "(Disabled from Central Management Tool)";
			this.labelGlobalDateLockDisabled.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelGlobalDateLockDisabled.Visible = false;
			// 
			// checkHasClinicsEnabled
			// 
			this.checkHasClinicsEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkHasClinicsEnabled.Enabled = false;
			this.checkHasClinicsEnabled.Location = new System.Drawing.Point(8, 305);
			this.checkHasClinicsEnabled.Name = "checkHasClinicsEnabled";
			this.checkHasClinicsEnabled.Size = new System.Drawing.Size(198, 20);
			this.checkHasClinicsEnabled.TabIndex = 296;
			this.checkHasClinicsEnabled.Text = "Clinics (multiple office locations)";
			// 
			// UserControlEnterpriseGeneral
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.checkDatabaseGlobalVariablesDontSet);
			this.Controls.Add(this.checkEnterpriseCommlogOmitDefaults);
			this.Controls.Add(this.checkEnableEmailAddressAutoComplete);
			this.Controls.Add(this.checkUpdateAlterLargeTablesDirectly);
			this.Controls.Add(this.groupPatientSelect);
			this.Controls.Add(this.checkUserNameManualEntry);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.checkUpdateStreamlinePassword);
			this.Controls.Add(this.textProcessSigsIntervalInSecs);
			this.Controls.Add(this.textSecurityLogOffAfterMinutes);
			this.Controls.Add(this.checkDatabaseMaintenanceSkipCheckTable);
			this.Controls.Add(this.checkDatabaseMaintenanceDisableOptimize);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.checkPasswordsMustBeStrong);
			this.Controls.Add(this.textSignalInactiveMinutes);
			this.Controls.Add(this.label12);
			this.Controls.Add(this.checkSecurityLockIncludesAdmin);
			this.Controls.Add(this.checkPasswordsWeakChangeToStrong);
			this.Controls.Add(this.checkPasswordsStrongIncludeSpecial);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.checkHasClinicsEnabled);
			this.Name = "UserControlEnterpriseGeneral";
			this.Size = new System.Drawing.Size(494, 784);
			this.groupPatientSelect.ResumeLayout(false);
			this.groupPatientSelect.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.CheckBox checkDatabaseGlobalVariablesDontSet;
		private UI.CheckBox checkEnterpriseCommlogOmitDefaults;
		private UI.CheckBox checkEnableEmailAddressAutoComplete;
		private UI.CheckBox checkUpdateAlterLargeTablesDirectly;
		private UI.GroupBox groupPatientSelect;
		private UI.CheckBox checkEnterpriseAllowRefreshWhileTyping;
		private System.Windows.Forms.Label label2;
		private ValidNum textEnterpriseExactMatchPhoneNumDigits;
		private UI.CheckBox checkEnterpriseExactMatchPhone;
		private UI.CheckBox checkPatientSelectFilterRestrictedClinics;
		private UI.Button butSyncPhNums;
		private UI.CheckBox checkPatientPhoneUsePhonenumberTable;
		private UI.CheckBox checkPatientSelectSearchWithEmptyParams;
		private System.Windows.Forms.Label labelPatSelectPauseMs;
		private ValidNum textPatientSelectSearchPauseMs;
		private System.Windows.Forms.Label labelPatSelectMinChars;
		private ValidNum textPatientSelectSearchMinChars;
		private UI.CheckBox checkUserNameManualEntry;
		private System.Windows.Forms.Label label3;
		private UI.CheckBox checkUpdateStreamlinePassword;
		private ValidNum textProcessSigsIntervalInSecs;
		private System.Windows.Forms.TextBox textSecurityLogOffAfterMinutes;
		private UI.CheckBox checkDatabaseMaintenanceSkipCheckTable;
		private UI.CheckBox checkDatabaseMaintenanceDisableOptimize;
		private System.Windows.Forms.Label label10;
		private UI.CheckBox checkPasswordsMustBeStrong;
		private ValidNum textSignalInactiveMinutes;
		private System.Windows.Forms.Label label12;
		private UI.CheckBox checkSecurityLockIncludesAdmin;
		private UI.CheckBox checkPasswordsWeakChangeToStrong;
		private UI.CheckBox checkPasswordsStrongIncludeSpecial;
		private UI.GroupBox groupBox2;
		private System.Windows.Forms.TextBox textSecurityLockDate;
		private System.Windows.Forms.Label label7;
		private UI.Button butChange;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox textSecurityLockDays;
		private System.Windows.Forms.Label labelGlobalDateLockDisabled;
		private UI.CheckBox checkHasClinicsEnabled;
	}
}
