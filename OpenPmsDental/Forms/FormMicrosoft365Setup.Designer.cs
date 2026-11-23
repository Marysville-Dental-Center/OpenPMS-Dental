namespace OpenPmsDental {
	partial class FormMicrosoft365Setup {
		///<summary>Required designer variable.</summary>
		private System.ComponentModel.IContainer components = null;

		///<summary>Clean up any resources being used.</summary>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		///<summary>Required method for Designer support - do not modify the contents of this method with the code editor.</summary>
		private void InitializeComponent() {
			this.checkEnabled = new OpenPmsDental.UI.CheckBox();
			this.labelDescription = new System.Windows.Forms.Label();
			this.labelClientId = new System.Windows.Forms.Label();
			this.textClientId = new System.Windows.Forms.TextBox();
			this.labelTenantId = new System.Windows.Forms.Label();
			this.textTenantId = new System.Windows.Forms.TextBox();
			this.groupAdvanced = new OpenPmsDental.UI.GroupBox();
			this.labelRedirectUri = new System.Windows.Forms.Label();
			this.textRedirectUri = new System.Windows.Forms.TextBox();
			this.labelSignInMethod = new System.Windows.Forms.Label();
			this.comboSignInMethod = new OpenPmsDental.UI.ComboBox();
			this.checkAllowPersonal = new OpenPmsDental.UI.CheckBox();
			this.labelPersonalWarning = new System.Windows.Forms.Label();
			this.labelSteps = new System.Windows.Forms.Label();
			this.textSteps = new System.Windows.Forms.TextBox();
			this.butSave = new OpenPmsDental.UI.Button();
			this.groupAdvanced.SuspendLayout();
			this.SuspendLayout();
			//
			// checkEnabled
			//
			this.checkEnabled.Location = new System.Drawing.Point(175, 8);
			this.checkEnabled.Name = "checkEnabled";
			this.checkEnabled.Size = new System.Drawing.Size(66, 20);
			this.checkEnabled.TabIndex = 0;
			this.checkEnabled.Text = "Enabled";
			//
			// labelDescription
			//
			this.labelDescription.Location = new System.Drawing.Point(8, 32);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(496, 47);
			this.labelDescription.TabIndex = 1;
			this.labelDescription.Text = "Sends email through Microsoft Graph for addresses set to Microsoft sign-in in Setup > Email, and downloads their inbox when the address asks for that. It uses an app registration in the office\'s own Microsoft Entra tenant. There is no client secret.";
			//
			// labelClientId
			//
			this.labelClientId.Location = new System.Drawing.Point(41, 81);
			this.labelClientId.Name = "labelClientId";
			this.labelClientId.Size = new System.Drawing.Size(128, 23);
			this.labelClientId.TabIndex = 2;
			this.labelClientId.Text = "Application (client) ID";
			this.labelClientId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textClientId
			//
			this.textClientId.Location = new System.Drawing.Point(175, 81);
			this.textClientId.Name = "textClientId";
			this.textClientId.Size = new System.Drawing.Size(328, 23);
			this.textClientId.TabIndex = 3;
			//
			// labelTenantId
			//
			this.labelTenantId.Location = new System.Drawing.Point(49, 108);
			this.labelTenantId.Name = "labelTenantId";
			this.labelTenantId.Size = new System.Drawing.Size(120, 23);
			this.labelTenantId.TabIndex = 4;
			this.labelTenantId.Text = "Directory (tenant) ID";
			this.labelTenantId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textTenantId
			//
			this.textTenantId.Location = new System.Drawing.Point(175, 108);
			this.textTenantId.Name = "textTenantId";
			this.textTenantId.Size = new System.Drawing.Size(328, 23);
			this.textTenantId.TabIndex = 5;
			//
			// groupAdvanced
			//
			this.groupAdvanced.Controls.Add(this.labelRedirectUri);
			this.groupAdvanced.Controls.Add(this.textRedirectUri);
			this.groupAdvanced.Controls.Add(this.labelSignInMethod);
			this.groupAdvanced.Controls.Add(this.comboSignInMethod);
			this.groupAdvanced.Controls.Add(this.checkAllowPersonal);
			this.groupAdvanced.Location = new System.Drawing.Point(8, 139);
			this.groupAdvanced.Name = "groupAdvanced";
			this.groupAdvanced.Size = new System.Drawing.Size(504, 105);
			this.groupAdvanced.TabIndex = 6;
			this.groupAdvanced.Text = "Advanced";
			//
			// labelRedirectUri
			//
			this.labelRedirectUri.Location = new System.Drawing.Point(85, 23);
			this.labelRedirectUri.Name = "labelRedirectUri";
			this.labelRedirectUri.Size = new System.Drawing.Size(76, 23);
			this.labelRedirectUri.TabIndex = 0;
			this.labelRedirectUri.Text = "Redirect URI";
			this.labelRedirectUri.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textRedirectUri
			//
			this.textRedirectUri.Location = new System.Drawing.Point(167, 23);
			this.textRedirectUri.Name = "textRedirectUri";
			this.textRedirectUri.Size = new System.Drawing.Size(320, 20);
			this.textRedirectUri.TabIndex = 1;
			//
			// labelSignInMethod
			//
			this.labelSignInMethod.Location = new System.Drawing.Point(65, 50);
			this.labelSignInMethod.Name = "labelSignInMethod";
			this.labelSignInMethod.Size = new System.Drawing.Size(96, 23);
			this.labelSignInMethod.TabIndex = 2;
			this.labelSignInMethod.Text = "Sign-in method";
			this.labelSignInMethod.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// comboSignInMethod
			//
			this.comboSignInMethod.Location = new System.Drawing.Point(167, 50);
			this.comboSignInMethod.Name = "comboSignInMethod";
			this.comboSignInMethod.Size = new System.Drawing.Size(200, 23);
			this.comboSignInMethod.TabIndex = 3;
			//
			// checkAllowPersonal
			//
			this.checkAllowPersonal.Location = new System.Drawing.Point(167, 77);
			this.checkAllowPersonal.Name = "checkAllowPersonal";
			this.checkAllowPersonal.Size = new System.Drawing.Size(210, 20);
			this.checkAllowPersonal.TabIndex = 4;
			this.checkAllowPersonal.Text = "Allow personal Microsoft accounts";
			this.checkAllowPersonal.CheckedChanged += new System.EventHandler(this.checkAllowPersonal_CheckedChanged);
			//
			// labelPersonalWarning
			//
			this.labelPersonalWarning.ForeColor = System.Drawing.Color.DarkRed;
			this.labelPersonalWarning.Location = new System.Drawing.Point(8, 404);
			this.labelPersonalWarning.Name = "labelPersonalWarning";
			this.labelPersonalWarning.Size = new System.Drawing.Size(496, 32);
			this.labelPersonalWarning.TabIndex = 7;
			this.labelPersonalWarning.Text = "Microsoft\'s HIPAA business associate agreement covers its business online services. Do not use personal Microsoft accounts for patient information.";
			this.labelPersonalWarning.Visible = false;
			//
			// labelSteps
			//
			this.labelSteps.Location = new System.Drawing.Point(8, 252);
			this.labelSteps.Name = "labelSteps";
			this.labelSteps.Size = new System.Drawing.Size(496, 18);
			this.labelSteps.TabIndex = 8;
			this.labelSteps.Text = "Registering the app in the office\'s tenant";
			//
			// textSteps
			//
			this.textSteps.BackColor = System.Drawing.SystemColors.Control;
			this.textSteps.Location = new System.Drawing.Point(8, 272);
			this.textSteps.Multiline = true;
			this.textSteps.Name = "textSteps";
			this.textSteps.ReadOnly = true;
			this.textSteps.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textSteps.Size = new System.Drawing.Size(495, 128);
			this.textSteps.TabIndex = 9;
			this.textSteps.TabStop = false;
			//
			// butSave
			//
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(437, 454);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 10;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			//
			// FormMicrosoft365Setup
			//
			this.ClientSize = new System.Drawing.Size(520, 486);
			this.Controls.Add(this.checkEnabled);
			this.Controls.Add(this.labelDescription);
			this.Controls.Add(this.labelClientId);
			this.Controls.Add(this.textClientId);
			this.Controls.Add(this.labelTenantId);
			this.Controls.Add(this.textTenantId);
			this.Controls.Add(this.groupAdvanced);
			this.Controls.Add(this.labelPersonalWarning);
			this.Controls.Add(this.labelSteps);
			this.Controls.Add(this.textSteps);
			this.Controls.Add(this.butSave);
			this.Name = "FormMicrosoft365Setup";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Microsoft 365 Email Setup";
			this.Load += new System.EventHandler(this.FormMicrosoft365Setup_Load);
			this.groupAdvanced.ResumeLayout(false);
			this.groupAdvanced.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.CheckBox checkEnabled;
		private System.Windows.Forms.Label labelDescription;
		private System.Windows.Forms.Label labelClientId;
		private System.Windows.Forms.TextBox textClientId;
		private System.Windows.Forms.Label labelTenantId;
		private System.Windows.Forms.TextBox textTenantId;
		private OpenPmsDental.UI.GroupBox groupAdvanced;
		private System.Windows.Forms.Label labelRedirectUri;
		private System.Windows.Forms.TextBox textRedirectUri;
		private System.Windows.Forms.Label labelSignInMethod;
		private OpenPmsDental.UI.ComboBox comboSignInMethod;
		private OpenPmsDental.UI.CheckBox checkAllowPersonal;
		private System.Windows.Forms.Label labelPersonalWarning;
		private System.Windows.Forms.Label labelSteps;
		private System.Windows.Forms.TextBox textSteps;
		private OpenPmsDental.UI.Button butSave;
	}
}
