namespace OpenPmsDental{
	partial class FormAdvertisingPostcardsSetup {
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
			this.labelDescription = new System.Windows.Forms.Label();
			this.checkEnabled = new OpenPmsDental.UI.CheckBox();
			this.labelEnableHint = new System.Windows.Forms.Label();
			this.groupAccount = new OpenPmsDental.UI.GroupBox();
			this.labelTestResult = new System.Windows.Forms.Label();
			this.butTestConnection = new OpenPmsDental.UI.Button();
			this.labelSandbox = new System.Windows.Forms.Label();
			this.textChildRef = new System.Windows.Forms.TextBox();
			this.labelChildRef = new System.Windows.Forms.Label();
			this.comboEnvironment = new OpenPmsDental.UI.ComboBox();
			this.labelEnvironment = new System.Windows.Forms.Label();
			this.labelSecretHint = new System.Windows.Forms.Label();
			this.textApiSecret = new System.Windows.Forms.TextBox();
			this.labelApiSecret = new System.Windows.Forms.Label();
			this.textApiKey = new System.Windows.Forms.TextBox();
			this.labelApiKey = new System.Windows.Forms.Label();
			this.checkUseDefault = new OpenPmsDental.UI.CheckBox();
			this.comboClinic = new OpenPmsDental.UI.ComboBox();
			this.labelClinic = new System.Windows.Forms.Label();
			this.groupSending = new OpenPmsDental.UI.GroupBox();
			this.checkSendPatientReference = new OpenPmsDental.UI.CheckBox();
			this.checkUsePreferredName = new OpenPmsDental.UI.CheckBox();
			this.comboMailClass = new OpenPmsDental.UI.ComboBox();
			this.labelMailClass = new System.Windows.Forms.Label();
			this.groupPrivacy = new OpenPmsDental.UI.GroupBox();
			this.labelBaaHint = new System.Windows.Forms.Label();
			this.labelBaaConfirmed = new System.Windows.Forms.Label();
			this.checkBaaOnFile = new OpenPmsDental.UI.CheckBox();
			this.butSave = new OpenPmsDental.UI.Button();
			this.groupAccount.SuspendLayout();
			this.groupSending.SuspendLayout();
			this.groupPrivacy.SuspendLayout();
			this.SuspendLayout();
			//
			// labelDescription
			//
			this.labelDescription.Location = new System.Drawing.Point(8, 8);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(536, 49);
			this.labelDescription.TabIndex = 0;
			this.labelDescription.Text = "Mails postcards to patients you select, through the office\'s own PostcardMania (P" +
    "CM Integrations) account. Patient names and mailing addresses are sent to the vendor, which prints and mails the cards and bills each order to that account.";
			//
			// checkEnabled
			//
			this.checkEnabled.Location = new System.Drawing.Point(8, 59);
			this.checkEnabled.Name = "checkEnabled";
			this.checkEnabled.Size = new System.Drawing.Size(66, 20);
			this.checkEnabled.TabIndex = 1;
			this.checkEnabled.Text = "Enabled";
			//
			// labelEnableHint
			//
			this.labelEnableHint.Location = new System.Drawing.Point(30, 83);
			this.labelEnableHint.Name = "labelEnableHint";
			this.labelEnableHint.Size = new System.Drawing.Size(530, 32);
			this.labelEnableHint.TabIndex = 2;
			this.labelEnableHint.Text = "Turning this on needs an API key and secret, and a successful Test connection. Tools, Advertising, Postcards only shows while this is on.";
			//
			// groupAccount
			//
			this.groupAccount.ColorBackLabel = System.Drawing.Color.Empty;
			this.groupAccount.Controls.Add(this.labelTestResult);
			this.groupAccount.Controls.Add(this.butTestConnection);
			this.groupAccount.Controls.Add(this.labelSandbox);
			this.groupAccount.Controls.Add(this.textChildRef);
			this.groupAccount.Controls.Add(this.labelChildRef);
			this.groupAccount.Controls.Add(this.comboEnvironment);
			this.groupAccount.Controls.Add(this.labelEnvironment);
			this.groupAccount.Controls.Add(this.labelSecretHint);
			this.groupAccount.Controls.Add(this.textApiSecret);
			this.groupAccount.Controls.Add(this.labelApiSecret);
			this.groupAccount.Controls.Add(this.textApiKey);
			this.groupAccount.Controls.Add(this.labelApiKey);
			this.groupAccount.Controls.Add(this.checkUseDefault);
			this.groupAccount.Controls.Add(this.comboClinic);
			this.groupAccount.Controls.Add(this.labelClinic);
			this.groupAccount.Location = new System.Drawing.Point(8, 117);
			this.groupAccount.Name = "groupAccount";
			this.groupAccount.Size = new System.Drawing.Size(552, 264);
			this.groupAccount.TabIndex = 3;
			this.groupAccount.Text = "Account";
			//
			// labelTestResult
			//
			this.labelTestResult.Location = new System.Drawing.Point(256, 222);
			this.labelTestResult.Name = "labelTestResult";
			this.labelTestResult.Size = new System.Drawing.Size(274, 34);
			this.labelTestResult.TabIndex = 14;
			//
			// butTestConnection
			//
			this.butTestConnection.Location = new System.Drawing.Point(140, 227);
			this.butTestConnection.Name = "butTestConnection";
			this.butTestConnection.Size = new System.Drawing.Size(110, 24);
			this.butTestConnection.TabIndex = 13;
			this.butTestConnection.Text = "&Test connection";
			this.butTestConnection.Click += new System.EventHandler(this.butTestConnection_Click);
			//
			// labelSandbox
			//
			this.labelSandbox.ForeColor = System.Drawing.Color.Red;
			this.labelSandbox.Location = new System.Drawing.Point(140, 202);
			this.labelSandbox.Name = "labelSandbox";
			this.labelSandbox.Size = new System.Drawing.Size(390, 18);
			this.labelSandbox.TabIndex = 12;
			this.labelSandbox.Text = "Sandbox: nothing will be printed or mailed.";
			//
			// textChildRef
			//
			this.textChildRef.Location = new System.Drawing.Point(140, 175);
			this.textChildRef.MaxLength = 50;
			this.textChildRef.Name = "textChildRef";
			this.textChildRef.Size = new System.Drawing.Size(200, 20);
			this.textChildRef.TabIndex = 11;
			//
			// labelChildRef
			//
			this.labelChildRef.Location = new System.Drawing.Point(38, 175);
			this.labelChildRef.Name = "labelChildRef";
			this.labelChildRef.Size = new System.Drawing.Size(96, 23);
			this.labelChildRef.TabIndex = 10;
			this.labelChildRef.Text = "Child Reference";
			this.labelChildRef.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// comboEnvironment
			//
			this.comboEnvironment.Location = new System.Drawing.Point(140, 148);
			this.comboEnvironment.Name = "comboEnvironment";
			this.comboEnvironment.Size = new System.Drawing.Size(120, 23);
			this.comboEnvironment.TabIndex = 9;
			this.comboEnvironment.SelectionChangeCommitted += new System.EventHandler(this.comboEnvironment_SelectionChangeCommitted);
			//
			// labelEnvironment
			//
			this.labelEnvironment.Location = new System.Drawing.Point(54, 148);
			this.labelEnvironment.Name = "labelEnvironment";
			this.labelEnvironment.Size = new System.Drawing.Size(80, 23);
			this.labelEnvironment.TabIndex = 8;
			this.labelEnvironment.Text = "Environment";
			this.labelEnvironment.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// labelSecretHint
			//
			this.labelSecretHint.Location = new System.Drawing.Point(140, 128);
			this.labelSecretHint.Name = "labelSecretHint";
			this.labelSecretHint.Size = new System.Drawing.Size(390, 18);
			this.labelSecretHint.TabIndex = 7;
			this.labelSecretHint.Text = "A secret is saved. Leave this blank to keep it.";
			//
			// textApiSecret
			//
			this.textApiSecret.AutoSize = false;
			this.textApiSecret.Location = new System.Drawing.Point(140, 101);
			this.textApiSecret.Name = "textApiSecret";
			this.textApiSecret.Size = new System.Drawing.Size(368, 23);
			this.textApiSecret.TabIndex = 6;
			this.textApiSecret.UseSystemPasswordChar = true;
			//
			// labelApiSecret
			//
			this.labelApiSecret.Location = new System.Drawing.Point(70, 101);
			this.labelApiSecret.Name = "labelApiSecret";
			this.labelApiSecret.Size = new System.Drawing.Size(64, 23);
			this.labelApiSecret.TabIndex = 5;
			this.labelApiSecret.Text = "API Secret";
			this.labelApiSecret.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textApiKey
			//
			this.textApiKey.Location = new System.Drawing.Point(140, 74);
			this.textApiKey.Name = "textApiKey";
			this.textApiKey.Size = new System.Drawing.Size(368, 23);
			this.textApiKey.TabIndex = 4;
			//
			// labelApiKey
			//
			this.labelApiKey.Location = new System.Drawing.Point(82, 74);
			this.labelApiKey.Name = "labelApiKey";
			this.labelApiKey.Size = new System.Drawing.Size(52, 23);
			this.labelApiKey.TabIndex = 3;
			this.labelApiKey.Text = "API Key";
			this.labelApiKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// checkUseDefault
			//
			this.checkUseDefault.Location = new System.Drawing.Point(140, 50);
			this.checkUseDefault.Name = "checkUseDefault";
			this.checkUseDefault.Size = new System.Drawing.Size(152, 20);
			this.checkUseDefault.TabIndex = 2;
			this.checkUseDefault.Text = "Use the default account";
			this.checkUseDefault.Click += new System.EventHandler(this.checkUseDefault_Click);
			//
			// comboClinic
			//
			this.comboClinic.Location = new System.Drawing.Point(140, 23);
			this.comboClinic.Name = "comboClinic";
			this.comboClinic.Size = new System.Drawing.Size(200, 23);
			this.comboClinic.TabIndex = 1;
			this.comboClinic.SelectionChangeCommitted += new System.EventHandler(this.comboClinic_SelectionChangeCommitted);
			//
			// labelClinic
			//
			this.labelClinic.Location = new System.Drawing.Point(90, 23);
			this.labelClinic.Name = "labelClinic";
			this.labelClinic.Size = new System.Drawing.Size(44, 23);
			this.labelClinic.TabIndex = 0;
			this.labelClinic.Text = "Clinic";
			this.labelClinic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// groupSending
			//
			this.groupSending.ColorBackLabel = System.Drawing.Color.Empty;
			this.groupSending.Controls.Add(this.checkSendPatientReference);
			this.groupSending.Controls.Add(this.checkUsePreferredName);
			this.groupSending.Controls.Add(this.comboMailClass);
			this.groupSending.Controls.Add(this.labelMailClass);
			this.groupSending.Location = new System.Drawing.Point(8, 389);
			this.groupSending.Name = "groupSending";
			this.groupSending.Size = new System.Drawing.Size(552, 102);
			this.groupSending.TabIndex = 4;
			this.groupSending.Text = "Sending (all clinics)";
			//
			// checkSendPatientReference
			//
			this.checkSendPatientReference.Location = new System.Drawing.Point(140, 74);
			this.checkSendPatientReference.Name = "checkSendPatientReference";
			this.checkSendPatientReference.Size = new System.Drawing.Size(389, 20);
			this.checkSendPatientReference.TabIndex = 3;
			this.checkSendPatientReference.Text = "Send a patient reference (P and the patient number) with each card";
			//
			// checkUsePreferredName
			//
			this.checkUsePreferredName.Location = new System.Drawing.Point(140, 50);
			this.checkUsePreferredName.Name = "checkUsePreferredName";
			this.checkUsePreferredName.Size = new System.Drawing.Size(384, 20);
			this.checkUsePreferredName.TabIndex = 2;
			this.checkUsePreferredName.Text = "Address cards with the patient\'s preferred name when there is one";
			//
			// comboMailClass
			//
			this.comboMailClass.Location = new System.Drawing.Point(140, 23);
			this.comboMailClass.Name = "comboMailClass";
			this.comboMailClass.Size = new System.Drawing.Size(152, 23);
			this.comboMailClass.TabIndex = 1;
			//
			// labelMailClass
			//
			this.labelMailClass.Location = new System.Drawing.Point(26, 23);
			this.labelMailClass.Name = "labelMailClass";
			this.labelMailClass.Size = new System.Drawing.Size(108, 23);
			this.labelMailClass.TabIndex = 0;
			this.labelMailClass.Text = "Default Mail Class";
			this.labelMailClass.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// groupPrivacy
			//
			this.groupPrivacy.ColorBackLabel = System.Drawing.Color.Empty;
			this.groupPrivacy.Controls.Add(this.labelBaaHint);
			this.groupPrivacy.Controls.Add(this.labelBaaConfirmed);
			this.groupPrivacy.Controls.Add(this.checkBaaOnFile);
			this.groupPrivacy.Location = new System.Drawing.Point(8, 499);
			this.groupPrivacy.Name = "groupPrivacy";
			this.groupPrivacy.Size = new System.Drawing.Size(552, 109);
			this.groupPrivacy.TabIndex = 5;
			this.groupPrivacy.Text = "Privacy";
			//
			// labelBaaHint
			//
			this.labelBaaHint.Location = new System.Drawing.Point(26, 65);
			this.labelBaaHint.Name = "labelBaaHint";
			this.labelBaaHint.Size = new System.Drawing.Size(504, 36);
			this.labelBaaHint.TabIndex = 2;
			this.labelBaaHint.Text = "Check addresses, Estimate and Send stay blocked until this is ticked, because patient names and addresses go to the vendor.";
			//
			// labelBaaConfirmed
			//
			this.labelBaaConfirmed.Location = new System.Drawing.Point(26, 47);
			this.labelBaaConfirmed.Name = "labelBaaConfirmed";
			this.labelBaaConfirmed.Size = new System.Drawing.Size(504, 16);
			this.labelBaaConfirmed.TabIndex = 1;
			//
			// checkBaaOnFile
			//
			this.checkBaaOnFile.Location = new System.Drawing.Point(8, 23);
			this.checkBaaOnFile.Name = "checkBaaOnFile";
			this.checkBaaOnFile.Size = new System.Drawing.Size(374, 20);
			this.checkBaaOnFile.TabIndex = 0;
			this.checkBaaOnFile.Text = "A signed business associate agreement with the vendor is on file";
			//
			// butSave
			//
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(485, 620);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 6;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			//
			// FormAdvertisingPostcardsSetup
			//
			this.ClientSize = new System.Drawing.Size(568, 652);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.groupPrivacy);
			this.Controls.Add(this.groupSending);
			this.Controls.Add(this.groupAccount);
			this.Controls.Add(this.labelEnableHint);
			this.Controls.Add(this.checkEnabled);
			this.Controls.Add(this.labelDescription);
			this.Name = "FormAdvertisingPostcardsSetup";
			this.Text = "PostcardMania Setup";
			this.Load += new System.EventHandler(this.FormAdvertisingPostcardsSetup_Load);
			this.groupAccount.ResumeLayout(false);
			this.groupAccount.PerformLayout();
			this.groupSending.ResumeLayout(false);
			this.groupPrivacy.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label labelDescription;
		private OpenPmsDental.UI.CheckBox checkEnabled;
		private System.Windows.Forms.Label labelEnableHint;
		private OpenPmsDental.UI.GroupBox groupAccount;
		private System.Windows.Forms.Label labelTestResult;
		private OpenPmsDental.UI.Button butTestConnection;
		private System.Windows.Forms.Label labelSandbox;
		private System.Windows.Forms.TextBox textChildRef;
		private System.Windows.Forms.Label labelChildRef;
		private OpenPmsDental.UI.ComboBox comboEnvironment;
		private System.Windows.Forms.Label labelEnvironment;
		private System.Windows.Forms.Label labelSecretHint;
		private System.Windows.Forms.TextBox textApiSecret;
		private System.Windows.Forms.Label labelApiSecret;
		private System.Windows.Forms.TextBox textApiKey;
		private System.Windows.Forms.Label labelApiKey;
		private OpenPmsDental.UI.CheckBox checkUseDefault;
		private OpenPmsDental.UI.ComboBox comboClinic;
		private System.Windows.Forms.Label labelClinic;
		private OpenPmsDental.UI.GroupBox groupSending;
		private OpenPmsDental.UI.CheckBox checkSendPatientReference;
		private OpenPmsDental.UI.CheckBox checkUsePreferredName;
		private OpenPmsDental.UI.ComboBox comboMailClass;
		private System.Windows.Forms.Label labelMailClass;
		private OpenPmsDental.UI.GroupBox groupPrivacy;
		private System.Windows.Forms.Label labelBaaHint;
		private System.Windows.Forms.Label labelBaaConfirmed;
		private OpenPmsDental.UI.CheckBox checkBaaOnFile;
		private OpenPmsDental.UI.Button butSave;
	}
}
