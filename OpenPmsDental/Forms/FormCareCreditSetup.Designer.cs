namespace OpenPmsDental {
	partial class FormCareCreditSetup {
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
			this.checkEnabled = new OpenPmsDental.UI.CheckBox();
			this.groupClinicSettings = new OpenPmsDental.UI.GroupBox();
			this.labelQSBatch = new System.Windows.Forms.Label();
			this.listBoxQSOptions = new OpenPmsDental.UI.ListBox();
			this.labelMerchantNumberClinic = new System.Windows.Forms.Label();
			this.textMerchantNumberClinic = new System.Windows.Forms.TextBox();
			this.groupQSBatch = new OpenPmsDental.UI.GroupBox();
			this.comboBoxDaysOut = new OpenPmsDental.UI.ComboBox();
			this.labelQSDays = new System.Windows.Forms.Label();
			this.comboPaymentType = new OpenPmsDental.UI.ComboBox();
			this.labelPaymentType = new System.Windows.Forms.Label();
			this.groupPromotions = new OpenPmsDental.UI.GroupBox();
			this.butPromotions = new OpenPmsDental.UI.Button();
			this.comboClinics = new OpenPmsDental.UI.ComboBoxClinicPicker();
			this.comboPatFieldDef = new OpenPmsDental.UI.ComboBox();
			this.labelPatFieldType = new System.Windows.Forms.Label();
			this.textMerchantNumberPractice = new System.Windows.Forms.TextBox();
			this.labelMerchantNumberPractice = new System.Windows.Forms.Label();
			this.checkMerchantNumByProv = new OpenPmsDental.UI.CheckBox();
			this.labelMerchantClosedDescription = new System.Windows.Forms.Label();
			this.comboPatFieldPreApprvAmt = new OpenPmsDental.UI.ComboBox();
			this.labelPreApprvAmt = new System.Windows.Forms.Label();
			this.comboPatFieldAvailableCredit = new OpenPmsDental.UI.ComboBox();
			this.labelAvailableCredit = new System.Windows.Forms.Label();
			this.labelClientId = new System.Windows.Forms.Label();
			this.textClientId = new System.Windows.Forms.TextBox();
			this.labelClientSecret = new System.Windows.Forms.Label();
			this.textClientSecret = new System.Windows.Forms.TextBox();
			this.labelEnvironment = new System.Windows.Forms.Label();
			this.comboEnvironment = new OpenPmsDental.UI.ComboBox();
			this.labelApiBaseUrl = new System.Windows.Forms.Label();
			this.textApiBaseUrl = new System.Windows.Forms.TextBox();
			this.labelIhsBaseUrl = new System.Windows.Forms.Label();
			this.textIhsBaseUrl = new System.Windows.Forms.TextBox();
			this.labelPartnerCode = new System.Windows.Forms.Label();
			this.textPartnerCode = new System.Windows.Forms.TextBox();
			this.labelTokenPath = new System.Windows.Forms.Label();
			this.textTokenPath = new System.Windows.Forms.TextBox();
			this.labelPrefillPath = new System.Windows.Forms.Label();
			this.textPrefillPath = new System.Windows.Forms.TextBox();
			this.labelPullbackPath = new System.Windows.Forms.Label();
			this.textPullbackPath = new System.Windows.Forms.TextBox();
			this.labelBatchPath = new System.Windows.Forms.Label();
			this.textBatchPath = new System.Windows.Forms.TextBox();
			this.labelSessionParam = new System.Windows.Forms.Label();
			this.textSessionParam = new System.Windows.Forms.TextBox();
			this.labelClientAuth = new System.Windows.Forms.Label();
			this.comboClientAuth = new OpenPmsDental.UI.ComboBox();
			this.labelScope = new System.Windows.Forms.Label();
			this.textScope = new System.Windows.Forms.TextBox();
			this.groupConnection = new OpenPmsDental.UI.GroupBox();
			this.butTestConnection = new OpenPmsDental.UI.Button();
			this.labelConnectionStatus = new System.Windows.Forms.Label();
			this.groupBackground = new OpenPmsDental.UI.GroupBox();
			this.labelJobComputer = new System.Windows.Forms.Label();
			this.butRunHere = new OpenPmsDental.UI.Button();
			this.labelProcessTime = new System.Windows.Forms.Label();
			this.textProcessTime = new System.Windows.Forms.TextBox();
			this.labelPullbackTime = new System.Windows.Forms.Label();
			this.textPullbackTime = new System.Windows.Forms.TextBox();
			this.labelTimeFormat = new System.Windows.Forms.Label();
			this.groupClinicSettings.SuspendLayout();
			this.groupQSBatch.SuspendLayout();
			this.groupPromotions.SuspendLayout();
			this.groupConnection.SuspendLayout();
			this.groupBackground.SuspendLayout();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(947, 599);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 7;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// checkEnabled
			// 
			this.checkEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEnabled.Location = new System.Drawing.Point(210, 8);
			this.checkEnabled.Name = "checkEnabled";
			this.checkEnabled.Size = new System.Drawing.Size(66, 20);
			this.checkEnabled.TabIndex = 0;
			this.checkEnabled.Text = "Enabled";
			// 
			// groupClinicSettings
			// 
			this.groupClinicSettings.Controls.Add(this.labelQSBatch);
			this.groupClinicSettings.Controls.Add(this.listBoxQSOptions);
			this.groupClinicSettings.Controls.Add(this.labelMerchantNumberClinic);
			this.groupClinicSettings.Controls.Add(this.textMerchantNumberClinic);
			this.groupClinicSettings.Controls.Add(this.groupQSBatch);
			this.groupClinicSettings.Controls.Add(this.comboPaymentType);
			this.groupClinicSettings.Controls.Add(this.labelPaymentType);
			this.groupClinicSettings.Controls.Add(this.groupPromotions);
			this.groupClinicSettings.Location = new System.Drawing.Point(24, 195);
			this.groupClinicSettings.Name = "groupClinicSettings";
			this.groupClinicSettings.Size = new System.Drawing.Size(580, 234);
			this.groupClinicSettings.TabIndex = 6;
			this.groupClinicSettings.Text = "Clinic Settings";
			// 
			// labelQSBatch
			// 
			this.labelQSBatch.Location = new System.Drawing.Point(8, 144);
			this.labelQSBatch.Name = "labelQSBatch";
			this.labelQSBatch.Size = new System.Drawing.Size(160, 16);
			this.labelQSBatch.TabIndex = 276;
			this.labelQSBatch.Text = "Quickscreen Feature";
			this.labelQSBatch.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// listBoxQSBatch
			// 
			this.listBoxQSOptions.Location = new System.Drawing.Point(8, 162);
			this.listBoxQSOptions.Name = "listBoxQSBatch";
			this.listBoxQSOptions.Size = new System.Drawing.Size(92, 64);
			this.listBoxQSOptions.TabIndex = 277;
			this.listBoxQSOptions.Text = "listQSBatch";
			this.listBoxQSOptions.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listQSBatch_SelectionChanged);
			// 
			// labelMerchantNumberClinic
			// 
			this.labelMerchantNumberClinic.Location = new System.Drawing.Point(68, 117);
			this.labelMerchantNumberClinic.Name = "labelMerchantNumberClinic";
			this.labelMerchantNumberClinic.Size = new System.Drawing.Size(112, 23);
			this.labelMerchantNumberClinic.TabIndex = 276;
			this.labelMerchantNumberClinic.Text = "Merchant Number";
			this.labelMerchantNumberClinic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textMerchantNumberClinic
			// 
			this.textMerchantNumberClinic.Location = new System.Drawing.Point(186, 117);
			this.textMerchantNumberClinic.Name = "textMerchantNumberClinic";
			this.textMerchantNumberClinic.Size = new System.Drawing.Size(220, 20);
			this.textMerchantNumberClinic.TabIndex = 275;
			this.textMerchantNumberClinic.Leave += new System.EventHandler(this.textMerchantNumberClinic_Leave);
			// 
			// groupQSBatch
			// 
			this.groupQSBatch.Controls.Add(this.comboBoxDaysOut);
			this.groupQSBatch.Controls.Add(this.labelQSDays);
			this.groupQSBatch.Location = new System.Drawing.Point(106, 162);
			this.groupQSBatch.Name = "groupQSBatch";
			this.groupQSBatch.Size = new System.Drawing.Size(466, 54);
			this.groupQSBatch.TabIndex = 6;
			this.groupQSBatch.Text = "Quickscreen Settings";
			// 
			// comboBoxDaysOut
			// 
			this.comboBoxDaysOut.Location = new System.Drawing.Point(8, 23);
			this.comboBoxDaysOut.Name = "comboBoxDaysOut";
			this.comboBoxDaysOut.Size = new System.Drawing.Size(40, 23);
			this.comboBoxDaysOut.TabIndex = 0;
			this.comboBoxDaysOut.Text = "comboBoxPlus1";
			// 
			// labelBatchDays
			// 
			this.labelQSDays.Location = new System.Drawing.Point(54, 25);
			this.labelQSDays.Name = "labelQSDays";
			this.labelQSDays.Size = new System.Drawing.Size(404, 19);
			this.labelQSDays.TabIndex = 208;
			this.labelQSDays.Text = "Number of days out to check for pre-approvals for future appointments.";
			this.labelQSDays.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboPaymentType
			// 
			this.comboPaymentType.Location = new System.Drawing.Point(186, 23);
			this.comboPaymentType.Name = "comboPaymentType";
			this.comboPaymentType.Size = new System.Drawing.Size(220, 23);
			this.comboPaymentType.TabIndex = 0;
			this.comboPaymentType.Text = "comboPaymentType";
			// 
			// labelPaymentType
			// 
			this.labelPaymentType.Location = new System.Drawing.Point(92, 23);
			this.labelPaymentType.Name = "labelPaymentType";
			this.labelPaymentType.Size = new System.Drawing.Size(88, 23);
			this.labelPaymentType.TabIndex = 18;
			this.labelPaymentType.Text = "Payment Type";
			this.labelPaymentType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupPromotions
			// 
			this.groupPromotions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupPromotions.Controls.Add(this.butPromotions);
			this.groupPromotions.Location = new System.Drawing.Point(186, 54);
			this.groupPromotions.Name = "groupPromotions";
			this.groupPromotions.Size = new System.Drawing.Size(220, 55);
			this.groupPromotions.TabIndex = 4;
			this.groupPromotions.Text = "CareCredit admin";
			// 
			// butPromotions
			// 
			this.butPromotions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butPromotions.Location = new System.Drawing.Point(73, 23);
			this.butPromotions.Name = "butPromotions";
			this.butPromotions.Size = new System.Drawing.Size(75, 24);
			this.butPromotions.TabIndex = 0;
			this.butPromotions.Text = "Manage";
			this.butPromotions.Click += new System.EventHandler(this.butPromotions_Click);
			// 
			// comboClinics
			// 
			this.comboClinics.HqDescription = "Headquarters";
			this.comboClinics.IncludeUnassigned = true;
			this.comboClinics.Location = new System.Drawing.Point(167, 164);
			this.comboClinics.Name = "comboClinics";
			this.comboClinics.Size = new System.Drawing.Size(251, 23);
			this.comboClinics.TabIndex = 2;
			this.comboClinics.SelectionChangeCommitted += new System.EventHandler(this.comboClinics_SelectionChangeCommitted);
			// 
			// comboPatFieldDef
			// 
			this.comboPatFieldDef.Location = new System.Drawing.Point(210, 59);
			this.comboPatFieldDef.Name = "comboPatFieldDef";
			this.comboPatFieldDef.Size = new System.Drawing.Size(212, 23);
			this.comboPatFieldDef.TabIndex = 3;
			this.comboPatFieldDef.Text = "comboBoxPlus1";
			// 
			// labelPatFieldType
			// 
			this.labelPatFieldType.Location = new System.Drawing.Point(20, 59);
			this.labelPatFieldType.Name = "labelPatFieldType";
			this.labelPatFieldType.Size = new System.Drawing.Size(184, 23);
			this.labelPatFieldType.TabIndex = 270;
			this.labelPatFieldType.Text = "Pre-Approval Status Patient Field";
			this.labelPatFieldType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textMerchantNumberPractice
			// 
			this.textMerchantNumberPractice.Location = new System.Drawing.Point(210, 32);
			this.textMerchantNumberPractice.Name = "textMerchantNumberPractice";
			this.textMerchantNumberPractice.Size = new System.Drawing.Size(212, 23);
			this.textMerchantNumberPractice.TabIndex = 5;
			this.textMerchantNumberPractice.Leave += new System.EventHandler(this.textMerchantNumberPractice_Leave);
			// 
			// labelMerchantNumberPractice
			// 
			this.labelMerchantNumberPractice.Location = new System.Drawing.Point(92, 32);
			this.labelMerchantNumberPractice.Name = "labelMerchantNumberPractice";
			this.labelMerchantNumberPractice.Size = new System.Drawing.Size(112, 23);
			this.labelMerchantNumberPractice.TabIndex = 274;
			this.labelMerchantNumberPractice.Text = "Merchant Number";
			this.labelMerchantNumberPractice.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkMerchantNumByProv
			// 
			this.checkMerchantNumByProv.Location = new System.Drawing.Point(210, 140);
			this.checkMerchantNumByProv.Name = "checkMerchantNumByProv";
			this.checkMerchantNumByProv.Size = new System.Drawing.Size(186, 20);
			this.checkMerchantNumByProv.TabIndex = 4;
			this.checkMerchantNumByProv.Text = "Merchant number by provider";
			this.checkMerchantNumByProv.CheckedChanged += new System.EventHandler(this.checkMerchantNumByProv_CheckedChanged);
			// 
			// labelMerchantClosedDescription
			// 
			this.labelMerchantClosedDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelMerchantClosedDescription.ForeColor = System.Drawing.Color.Red;
			this.labelMerchantClosedDescription.Location = new System.Drawing.Point(8, 589);
			this.labelMerchantClosedDescription.Name = "labelMerchantClosedDescription";
			this.labelMerchantClosedDescription.Size = new System.Drawing.Size(634, 31);
			this.labelMerchantClosedDescription.TabIndex = 275;
			this.labelMerchantClosedDescription.Text = "The selected Merchant Number has been closed.\r\nPlease enter a new one or refer to" +
    " the manual for more information.";
			this.labelMerchantClosedDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboPatFieldPreApprvAmt
			// 
			this.comboPatFieldPreApprvAmt.Location = new System.Drawing.Point(210, 86);
			this.comboPatFieldPreApprvAmt.Name = "comboPatFieldPreApprvAmt";
			this.comboPatFieldPreApprvAmt.Size = new System.Drawing.Size(212, 23);
			this.comboPatFieldPreApprvAmt.TabIndex = 276;
			this.comboPatFieldPreApprvAmt.Text = "comboBoxPlus1";
			// 
			// labelPreApprvAmt
			// 
			this.labelPreApprvAmt.Location = new System.Drawing.Point(8, 86);
			this.labelPreApprvAmt.Name = "labelPreApprvAmt";
			this.labelPreApprvAmt.Size = new System.Drawing.Size(196, 23);
			this.labelPreApprvAmt.TabIndex = 277;
			this.labelPreApprvAmt.Text = "Pre-Approval Amount Patient Field";
			this.labelPreApprvAmt.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboPatFieldAvailableCredit
			// 
			this.comboPatFieldAvailableCredit.Location = new System.Drawing.Point(210, 113);
			this.comboPatFieldAvailableCredit.Name = "comboPatFieldAvailableCredit";
			this.comboPatFieldAvailableCredit.Size = new System.Drawing.Size(212, 23);
			this.comboPatFieldAvailableCredit.TabIndex = 278;
			this.comboPatFieldAvailableCredit.Text = "comboBoxPlus1";
			// 
			// labelAvailableCredit
			// 
			this.labelAvailableCredit.Location = new System.Drawing.Point(40, 113);
			this.labelAvailableCredit.Name = "labelAvailableCredit";
			this.labelAvailableCredit.Size = new System.Drawing.Size(164, 23);
			this.labelAvailableCredit.TabIndex = 279;
			this.labelAvailableCredit.Text = "Available Credit Patient Field";
			this.labelAvailableCredit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// groupConnection
			//
			this.groupConnection.Controls.Add(this.labelClientId);
			this.groupConnection.Controls.Add(this.textClientId);
			this.groupConnection.Controls.Add(this.labelClientSecret);
			this.groupConnection.Controls.Add(this.textClientSecret);
			this.groupConnection.Controls.Add(this.labelEnvironment);
			this.groupConnection.Controls.Add(this.comboEnvironment);
			this.groupConnection.Controls.Add(this.labelApiBaseUrl);
			this.groupConnection.Controls.Add(this.textApiBaseUrl);
			this.groupConnection.Controls.Add(this.labelIhsBaseUrl);
			this.groupConnection.Controls.Add(this.textIhsBaseUrl);
			this.groupConnection.Controls.Add(this.labelPartnerCode);
			this.groupConnection.Controls.Add(this.textPartnerCode);
			this.groupConnection.Controls.Add(this.labelTokenPath);
			this.groupConnection.Controls.Add(this.textTokenPath);
			this.groupConnection.Controls.Add(this.labelPrefillPath);
			this.groupConnection.Controls.Add(this.textPrefillPath);
			this.groupConnection.Controls.Add(this.labelPullbackPath);
			this.groupConnection.Controls.Add(this.textPullbackPath);
			this.groupConnection.Controls.Add(this.labelBatchPath);
			this.groupConnection.Controls.Add(this.textBatchPath);
			this.groupConnection.Controls.Add(this.labelSessionParam);
			this.groupConnection.Controls.Add(this.textSessionParam);
			this.groupConnection.Controls.Add(this.labelClientAuth);
			this.groupConnection.Controls.Add(this.comboClientAuth);
			this.groupConnection.Controls.Add(this.labelScope);
			this.groupConnection.Controls.Add(this.textScope);
			this.groupConnection.Controls.Add(this.butTestConnection);
			this.groupConnection.Controls.Add(this.labelConnectionStatus);
			this.groupConnection.Location = new System.Drawing.Point(619, 8);
			this.groupConnection.Name = "groupConnection";
			this.groupConnection.Size = new System.Drawing.Size(403, 446);
			this.groupConnection.TabIndex = 8;
			this.groupConnection.Text = "Connection (values from Synchrony's implementation guide)";
			// 
			// labelClientId
			// 
			this.labelClientId.Location = new System.Drawing.Point(98, 23);
			this.labelClientId.Name = "labelClientId";
			this.labelClientId.Size = new System.Drawing.Size(56, 23);
			this.labelClientId.TabIndex = 100;
			this.labelClientId.Text = "Client ID";
			this.labelClientId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textClientId
			// 
			this.textClientId.Location = new System.Drawing.Point(160, 23);
			this.textClientId.Name = "textClientId";
			this.textClientId.Size = new System.Drawing.Size(224, 23);
			this.textClientId.TabIndex = 0;
			// 
			// labelClientSecret
			// 
			this.labelClientSecret.Location = new System.Drawing.Point(78, 50);
			this.labelClientSecret.Name = "labelClientSecret";
			this.labelClientSecret.Size = new System.Drawing.Size(76, 23);
			this.labelClientSecret.TabIndex = 101;
			this.labelClientSecret.Text = "Client secret";
			this.labelClientSecret.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textClientSecret
			// 
			this.textClientSecret.Location = new System.Drawing.Point(160, 50);
			this.textClientSecret.Name = "textClientSecret";
			this.textClientSecret.Size = new System.Drawing.Size(224, 23);
			this.textClientSecret.TabIndex = 1;
			this.textClientSecret.UseSystemPasswordChar = true;
			// 
			// labelEnvironment
			// 
			this.labelEnvironment.Location = new System.Drawing.Point(74, 77);
			this.labelEnvironment.Name = "labelEnvironment";
			this.labelEnvironment.Size = new System.Drawing.Size(80, 23);
			this.labelEnvironment.TabIndex = 102;
			this.labelEnvironment.Text = "Environment";
			this.labelEnvironment.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboEnvironment
			// 
			this.comboEnvironment.Location = new System.Drawing.Point(160, 77);
			this.comboEnvironment.Name = "comboEnvironment";
			this.comboEnvironment.Size = new System.Drawing.Size(224, 23);
			this.comboEnvironment.TabIndex = 2;
			// 
			// labelApiBaseUrl
			// 
			this.labelApiBaseUrl.Location = new System.Drawing.Point(74, 104);
			this.labelApiBaseUrl.Name = "labelApiBaseUrl";
			this.labelApiBaseUrl.Size = new System.Drawing.Size(80, 23);
			this.labelApiBaseUrl.TabIndex = 103;
			this.labelApiBaseUrl.Text = "API base URL";
			this.labelApiBaseUrl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textApiBaseUrl
			// 
			this.textApiBaseUrl.Location = new System.Drawing.Point(160, 104);
			this.textApiBaseUrl.Name = "textApiBaseUrl";
			this.textApiBaseUrl.Size = new System.Drawing.Size(224, 23);
			this.textApiBaseUrl.TabIndex = 3;
			// 
			// labelIhsBaseUrl
			// 
			this.labelIhsBaseUrl.Location = new System.Drawing.Point(22, 131);
			this.labelIhsBaseUrl.Name = "labelIhsBaseUrl";
			this.labelIhsBaseUrl.Size = new System.Drawing.Size(132, 23);
			this.labelIhsBaseUrl.TabIndex = 104;
			this.labelIhsBaseUrl.Text = "Hosted-page base URL";
			this.labelIhsBaseUrl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textIhsBaseUrl
			// 
			this.textIhsBaseUrl.Location = new System.Drawing.Point(160, 131);
			this.textIhsBaseUrl.Name = "textIhsBaseUrl";
			this.textIhsBaseUrl.Size = new System.Drawing.Size(224, 23);
			this.textIhsBaseUrl.TabIndex = 4;
			// 
			// labelPartnerCode
			// 
			this.labelPartnerCode.Location = new System.Drawing.Point(74, 158);
			this.labelPartnerCode.Name = "labelPartnerCode";
			this.labelPartnerCode.Size = new System.Drawing.Size(80, 23);
			this.labelPartnerCode.TabIndex = 105;
			this.labelPartnerCode.Text = "Partner code";
			this.labelPartnerCode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPartnerCode
			// 
			this.textPartnerCode.Location = new System.Drawing.Point(160, 158);
			this.textPartnerCode.Name = "textPartnerCode";
			this.textPartnerCode.Size = new System.Drawing.Size(224, 23);
			this.textPartnerCode.TabIndex = 5;
			// 
			// labelTokenPath
			// 
			this.labelTokenPath.Location = new System.Drawing.Point(82, 185);
			this.labelTokenPath.Name = "labelTokenPath";
			this.labelTokenPath.Size = new System.Drawing.Size(72, 23);
			this.labelTokenPath.TabIndex = 106;
			this.labelTokenPath.Text = "Token path";
			this.labelTokenPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textTokenPath
			// 
			this.textTokenPath.Location = new System.Drawing.Point(160, 185);
			this.textTokenPath.Name = "textTokenPath";
			this.textTokenPath.Size = new System.Drawing.Size(224, 23);
			this.textTokenPath.TabIndex = 6;
			// 
			// labelPrefillPath
			// 
			this.labelPrefillPath.Location = new System.Drawing.Point(86, 212);
			this.labelPrefillPath.Name = "labelPrefillPath";
			this.labelPrefillPath.Size = new System.Drawing.Size(68, 23);
			this.labelPrefillPath.TabIndex = 107;
			this.labelPrefillPath.Text = "Prefill path";
			this.labelPrefillPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPrefillPath
			// 
			this.textPrefillPath.Location = new System.Drawing.Point(160, 212);
			this.textPrefillPath.Name = "textPrefillPath";
			this.textPrefillPath.Size = new System.Drawing.Size(224, 23);
			this.textPrefillPath.TabIndex = 7;
			// 
			// labelPullbackPath
			// 
			this.labelPullbackPath.Location = new System.Drawing.Point(70, 239);
			this.labelPullbackPath.Name = "labelPullbackPath";
			this.labelPullbackPath.Size = new System.Drawing.Size(84, 23);
			this.labelPullbackPath.TabIndex = 108;
			this.labelPullbackPath.Text = "Pullback path";
			this.labelPullbackPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPullbackPath
			// 
			this.textPullbackPath.Location = new System.Drawing.Point(160, 239);
			this.textPullbackPath.Name = "textPullbackPath";
			this.textPullbackPath.Size = new System.Drawing.Size(224, 23);
			this.textPullbackPath.TabIndex = 8;
			// 
			// labelBatchPath
			// 
			this.labelBatchPath.Location = new System.Drawing.Point(86, 266);
			this.labelBatchPath.Name = "labelBatchPath";
			this.labelBatchPath.Size = new System.Drawing.Size(68, 23);
			this.labelBatchPath.TabIndex = 109;
			this.labelBatchPath.Text = "Batch path";
			this.labelBatchPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBatchPath
			// 
			this.textBatchPath.Location = new System.Drawing.Point(160, 266);
			this.textBatchPath.Name = "textBatchPath";
			this.textBatchPath.Size = new System.Drawing.Size(224, 23);
			this.textBatchPath.TabIndex = 9;
			// 
			// labelSessionParam
			// 
			this.labelSessionParam.Location = new System.Drawing.Point(46, 293);
			this.labelSessionParam.Name = "labelSessionParam";
			this.labelSessionParam.Size = new System.Drawing.Size(108, 23);
			this.labelSessionParam.TabIndex = 110;
			this.labelSessionParam.Text = "Session parameter";
			this.labelSessionParam.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textSessionParam
			// 
			this.textSessionParam.Location = new System.Drawing.Point(160, 293);
			this.textSessionParam.Name = "textSessionParam";
			this.textSessionParam.Size = new System.Drawing.Size(224, 23);
			this.textSessionParam.TabIndex = 10;
			// 
			// labelClientAuth
			// 
			this.labelClientAuth.Location = new System.Drawing.Point(30, 320);
			this.labelClientAuth.Name = "labelClientAuth";
			this.labelClientAuth.Size = new System.Drawing.Size(124, 23);
			this.labelClientAuth.TabIndex = 111;
			this.labelClientAuth.Text = "Client authentication";
			this.labelClientAuth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboClientAuth
			// 
			this.comboClientAuth.Location = new System.Drawing.Point(160, 320);
			this.comboClientAuth.Name = "comboClientAuth";
			this.comboClientAuth.Size = new System.Drawing.Size(224, 23);
			this.comboClientAuth.TabIndex = 11;
			// 
			// labelScope
			// 
			this.labelScope.Location = new System.Drawing.Point(54, 347);
			this.labelScope.Name = "labelScope";
			this.labelScope.Size = new System.Drawing.Size(100, 23);
			this.labelScope.TabIndex = 112;
			this.labelScope.Text = "Scope (optional)";
			this.labelScope.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textScope
			// 
			this.textScope.Location = new System.Drawing.Point(160, 347);
			this.textScope.Name = "textScope";
			this.textScope.Size = new System.Drawing.Size(224, 23);
			this.textScope.TabIndex = 12;
			//
			// butTestConnection
			//
			this.butTestConnection.Location = new System.Drawing.Point(160, 374);
			this.butTestConnection.Name = "butTestConnection";
			this.butTestConnection.Size = new System.Drawing.Size(110, 24);
			this.butTestConnection.TabIndex = 20;
			this.butTestConnection.Text = "Test connection";
			this.butTestConnection.Click += new System.EventHandler(this.butTestConnection_Click);
			//
			// labelConnectionStatus
			//
			this.labelConnectionStatus.Location = new System.Drawing.Point(8, 402);
			this.labelConnectionStatus.Name = "labelConnectionStatus";
			this.labelConnectionStatus.Size = new System.Drawing.Size(383, 36);
			this.labelConnectionStatus.TabIndex = 21;
			this.labelConnectionStatus.Text = "";
			this.labelConnectionStatus.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			//
			// groupBackground
			//
			this.groupBackground.Controls.Add(this.labelJobComputer);
			this.groupBackground.Controls.Add(this.butRunHere);
			this.groupBackground.Controls.Add(this.labelProcessTime);
			this.groupBackground.Controls.Add(this.textProcessTime);
			this.groupBackground.Controls.Add(this.labelPullbackTime);
			this.groupBackground.Controls.Add(this.textPullbackTime);
			this.groupBackground.Controls.Add(this.labelTimeFormat);
			this.groupBackground.Location = new System.Drawing.Point(619, 462);
			this.groupBackground.Name = "groupBackground";
			this.groupBackground.Size = new System.Drawing.Size(403, 115);
			this.groupBackground.TabIndex = 9;
			this.groupBackground.Text = "Background tasks (batch QuickScreen and result checks)";
			//
			// labelJobComputer
			//
			this.labelJobComputer.Location = new System.Drawing.Point(169, 23);
			this.labelJobComputer.Name = "labelJobComputer";
			this.labelJobComputer.Size = new System.Drawing.Size(100, 30);
			this.labelJobComputer.TabIndex = 0;
			this.labelJobComputer.Text = "Run on: (not set)";
			this.labelJobComputer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// butRunHere
			//
			this.butRunHere.Location = new System.Drawing.Point(275, 26);
			this.butRunHere.Name = "butRunHere";
			this.butRunHere.Size = new System.Drawing.Size(120, 23);
			this.butRunHere.TabIndex = 1;
			this.butRunHere.Text = "Run on this computer";
			this.butRunHere.Click += new System.EventHandler(this.butRunHere_Click);
			//
			// labelProcessTime
			//
			this.labelProcessTime.Location = new System.Drawing.Point(42, 57);
			this.labelProcessTime.Name = "labelProcessTime";
			this.labelProcessTime.Size = new System.Drawing.Size(112, 23);
			this.labelProcessTime.TabIndex = 2;
			this.labelProcessTime.Text = "Batch process time";
			this.labelProcessTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textProcessTime
			//
			this.textProcessTime.Location = new System.Drawing.Point(160, 57);
			this.textProcessTime.Name = "textProcessTime";
			this.textProcessTime.Size = new System.Drawing.Size(60, 20);
			this.textProcessTime.TabIndex = 3;
			//
			// labelPullbackTime
			//
			this.labelPullbackTime.Location = new System.Drawing.Point(22, 84);
			this.labelPullbackTime.Name = "labelPullbackTime";
			this.labelPullbackTime.Size = new System.Drawing.Size(132, 23);
			this.labelPullbackTime.TabIndex = 4;
			this.labelPullbackTime.Text = "First batch result check";
			this.labelPullbackTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textPullbackTime
			//
			this.textPullbackTime.Location = new System.Drawing.Point(160, 84);
			this.textPullbackTime.Name = "textPullbackTime";
			this.textPullbackTime.Size = new System.Drawing.Size(60, 20);
			this.textPullbackTime.TabIndex = 5;
			//
			// labelTimeFormat
			//
			this.labelTimeFormat.Location = new System.Drawing.Point(226, 59);
			this.labelTimeFormat.Name = "labelTimeFormat";
			this.labelTimeFormat.Size = new System.Drawing.Size(163, 18);
			this.labelTimeFormat.TabIndex = 6;
			this.labelTimeFormat.Text = "24-hour time, e.g. 07:20";
			this.labelTimeFormat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// FormCareCreditSetup
			//
			this.ClientSize = new System.Drawing.Size(1030, 631);
			this.Controls.Add(this.groupBackground);
			this.Controls.Add(this.groupConnection);
			this.Controls.Add(this.comboPatFieldAvailableCredit);
			this.Controls.Add(this.labelAvailableCredit);
			this.Controls.Add(this.comboPatFieldPreApprvAmt);
			this.Controls.Add(this.labelPreApprvAmt);
			this.Controls.Add(this.labelMerchantClosedDescription);
			this.Controls.Add(this.checkMerchantNumByProv);
			this.Controls.Add(this.labelMerchantNumberPractice);
			this.Controls.Add(this.textMerchantNumberPractice);
			this.Controls.Add(this.comboPatFieldDef);
			this.Controls.Add(this.labelPatFieldType);
			this.Controls.Add(this.comboClinics);
			this.Controls.Add(this.groupClinicSettings);
			this.Controls.Add(this.checkEnabled);
			this.Controls.Add(this.butSave);
			this.Name = "FormCareCreditSetup";
			this.Text = "CareCredit Setup";
			this.Load += new System.EventHandler(this.FormCareCreditSetup_Load);
			this.groupClinicSettings.ResumeLayout(false);
			this.groupClinicSettings.PerformLayout();
			this.groupQSBatch.ResumeLayout(false);
			this.groupPromotions.ResumeLayout(false);
			this.groupConnection.ResumeLayout(false);
			this.groupConnection.PerformLayout();
			this.groupBackground.ResumeLayout(false);
			this.groupBackground.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.Button butSave;
		private OpenPmsDental.UI.CheckBox checkEnabled;
		private OpenPmsDental.UI.GroupBox groupClinicSettings;
		private UI.ComboBoxClinicPicker comboClinics;
		private System.Windows.Forms.Label labelPaymentType;
		private UI.ComboBox comboPaymentType;
		private OpenPmsDental.UI.GroupBox groupPromotions;
		private UI.Button butPromotions;
		private OpenPmsDental.UI.GroupBox groupQSBatch;
		private System.Windows.Forms.Label labelQSDays;
		private UI.ComboBox comboBoxDaysOut;
		private UI.ComboBox comboPatFieldDef;
		private System.Windows.Forms.Label labelPatFieldType;
		private System.Windows.Forms.TextBox textMerchantNumberPractice;
		private System.Windows.Forms.Label labelMerchantNumberPractice;
		private OpenPmsDental.UI.CheckBox checkMerchantNumByProv;
		private System.Windows.Forms.Label labelMerchantNumberClinic;
		private System.Windows.Forms.TextBox textMerchantNumberClinic;
		private System.Windows.Forms.Label labelMerchantClosedDescription;
		private UI.ComboBox comboPatFieldPreApprvAmt;
		private System.Windows.Forms.Label labelPreApprvAmt;
		private UI.ComboBox comboPatFieldAvailableCredit;
		private System.Windows.Forms.Label labelAvailableCredit;
		private UI.ListBox listBoxQSOptions;
		private System.Windows.Forms.Label labelQSBatch;
		private OpenPmsDental.UI.GroupBox groupConnection;
		private System.Windows.Forms.Label labelClientId;
		private System.Windows.Forms.TextBox textClientId;
		private System.Windows.Forms.Label labelClientSecret;
		private System.Windows.Forms.TextBox textClientSecret;
		private System.Windows.Forms.Label labelEnvironment;
		private UI.ComboBox comboEnvironment;
		private System.Windows.Forms.Label labelApiBaseUrl;
		private System.Windows.Forms.TextBox textApiBaseUrl;
		private System.Windows.Forms.Label labelIhsBaseUrl;
		private System.Windows.Forms.TextBox textIhsBaseUrl;
		private System.Windows.Forms.Label labelPartnerCode;
		private System.Windows.Forms.TextBox textPartnerCode;
		private System.Windows.Forms.Label labelTokenPath;
		private System.Windows.Forms.TextBox textTokenPath;
		private System.Windows.Forms.Label labelPrefillPath;
		private System.Windows.Forms.TextBox textPrefillPath;
		private System.Windows.Forms.Label labelPullbackPath;
		private System.Windows.Forms.TextBox textPullbackPath;
		private System.Windows.Forms.Label labelBatchPath;
		private System.Windows.Forms.TextBox textBatchPath;
		private System.Windows.Forms.Label labelSessionParam;
		private System.Windows.Forms.TextBox textSessionParam;
		private System.Windows.Forms.Label labelClientAuth;
		private UI.ComboBox comboClientAuth;
		private System.Windows.Forms.Label labelScope;
		private System.Windows.Forms.TextBox textScope;
		private UI.Button butTestConnection;
		private System.Windows.Forms.Label labelConnectionStatus;
		private OpenPmsDental.UI.GroupBox groupBackground;
		private System.Windows.Forms.Label labelJobComputer;
		private UI.Button butRunHere;
		private System.Windows.Forms.Label labelProcessTime;
		private System.Windows.Forms.TextBox textProcessTime;
		private System.Windows.Forms.Label labelPullbackTime;
		private System.Windows.Forms.TextBox textPullbackTime;
		private System.Windows.Forms.Label labelTimeFormat;
	}
}