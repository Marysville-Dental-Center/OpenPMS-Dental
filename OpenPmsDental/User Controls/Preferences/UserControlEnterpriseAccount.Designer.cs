
namespace OpenPmsDental {
	partial class UserControlEnterpriseAccount {
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
			this.checkAgingCalculateOnBatchClaimReceipt = new OpenPmsDental.UI.CheckBox();
			this.label13 = new System.Windows.Forms.Label();
			this.label27 = new System.Windows.Forms.Label();
			this.checkAgingReportShowAgePatPayplanPayments = new OpenPmsDental.UI.CheckBox();
			this.textBillingElectBatchMax = new OpenPmsDental.ValidNum();
			this.comboPayPlansVersion = new OpenPmsDental.UI.ComboBox();
			this.checkClaimReportReceivedByService = new OpenPmsDental.UI.CheckBox();
			this.comboRigorousAdjustments = new OpenPmsDental.UI.ComboBox();
			this.checkPaymentWindowDefaultHideSplits = new OpenPmsDental.UI.CheckBox();
			this.textClaimReportReceiveInterval = new System.Windows.Forms.TextBox();
			this.label41 = new System.Windows.Forms.Label();
			this.labelAgingServiceTimeDue = new System.Windows.Forms.Label();
			this.comboRigorousAccounting = new OpenPmsDental.UI.ComboBox();
			this.label39 = new System.Windows.Forms.Label();
			this.checkBillingShowTransSinceBalZero = new OpenPmsDental.UI.CheckBox();
			this.labelReportheckUnits = new System.Windows.Forms.Label();
			this.textAgingServiceTimeDue = new OpenPmsDental.ValidDate();
			this.radioReceiveAtAnInterval = new System.Windows.Forms.RadioButton();
			this.comboPaymentClinicSetting = new OpenPmsDental.UI.ComboBox();
			this.radioReceiveAtASetTime = new System.Windows.Forms.RadioButton();
			this.checkPaymentsPromptForPayType = new OpenPmsDental.UI.CheckBox();
			this.textClaimReportReceiveTime = new OpenPmsDental.ValidTime();
			this.label38 = new System.Windows.Forms.Label();
			this.groupBoxClaimIdPrefix = new OpenPmsDental.UI.GroupBox();
			this.butReplacements = new OpenPmsDental.UI.Button();
			this.textClaimIdPrefix = new System.Windows.Forms.TextBox();
			this.groupBoxClaimIdPrefix.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkAgingCalculateOnBatchClaimReceipt
			// 
			this.checkAgingCalculateOnBatchClaimReceipt.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAgingCalculateOnBatchClaimReceipt.Location = new System.Drawing.Point(8, 8);
			this.checkAgingCalculateOnBatchClaimReceipt.Name = "checkAgingCalculateOnBatchClaimReceipt";
			this.checkAgingCalculateOnBatchClaimReceipt.Size = new System.Drawing.Size(309, 20);
			this.checkAgingCalculateOnBatchClaimReceipt.TabIndex = 303;
			this.checkAgingCalculateOnBatchClaimReceipt.Text = "Aging calculated on receipt of batch claim payments";
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(24, 434);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(226, 32);
			this.label13.TabIndex = 302;
			this.label13.Text = "Max number of statements per batch (0 for no limit)";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label27
			// 
			this.label27.Location = new System.Drawing.Point(24, 407);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(226, 23);
			this.label27.TabIndex = 293;
			this.label27.Text = "Pay Plan charge logic";
			this.label27.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkAgingReportShowAgePatPayplanPayments
			// 
			this.checkAgingReportShowAgePatPayplanPayments.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAgingReportShowAgePatPayplanPayments.Enabled = false;
			this.checkAgingReportShowAgePatPayplanPayments.Location = new System.Drawing.Point(8, 32);
			this.checkAgingReportShowAgePatPayplanPayments.Name = "checkAgingReportShowAgePatPayplanPayments";
			this.checkAgingReportShowAgePatPayplanPayments.Size = new System.Drawing.Size(276, 20);
			this.checkAgingReportShowAgePatPayplanPayments.TabIndex = 279;
			this.checkAgingReportShowAgePatPayplanPayments.Text = "Aging Report Show Age Pat Payplan Payments";
			// 
			// textBillingElectBatchMax
			// 
			this.textBillingElectBatchMax.Location = new System.Drawing.Point(256, 438);
			this.textBillingElectBatchMax.Name = "textBillingElectBatchMax";
			this.textBillingElectBatchMax.Size = new System.Drawing.Size(60, 20);
			this.textBillingElectBatchMax.TabIndex = 283;
			this.textBillingElectBatchMax.Text = "0";
			this.textBillingElectBatchMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBillingElectBatchMax.Validating += new System.ComponentModel.CancelEventHandler(this.textBillingElectBatchMax_Validating);
			// 
			// comboPayPlansVersion
			// 
			this.comboPayPlansVersion.Location = new System.Drawing.Point(256, 407);
			this.comboPayPlansVersion.Name = "comboPayPlansVersion";
			this.comboPayPlansVersion.Size = new System.Drawing.Size(230, 23);
			this.comboPayPlansVersion.TabIndex = 292;
			this.comboPayPlansVersion.Visible = false;
			this.comboPayPlansVersion.SelectionChangeCommitted += new System.EventHandler(this.comboPayPlansVersion_ChangeCommitted);
			// 
			// checkClaimReportReceivedByService
			// 
			this.checkClaimReportReceivedByService.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkClaimReportReceivedByService.Location = new System.Drawing.Point(8, 251);
			this.checkClaimReportReceivedByService.Name = "checkClaimReportReceivedByService";
			this.checkClaimReportReceivedByService.Size = new System.Drawing.Size(168, 20);
			this.checkClaimReportReceivedByService.TabIndex = 301;
			this.checkClaimReportReceivedByService.TabStop = false;
			this.checkClaimReportReceivedByService.Text = "Receive Reports by Service";
			// 
			// comboRigorousAdjustments
			// 
			this.comboRigorousAdjustments.Location = new System.Drawing.Point(256, 356);
			this.comboRigorousAdjustments.Name = "comboRigorousAdjustments";
			this.comboRigorousAdjustments.Size = new System.Drawing.Size(230, 23);
			this.comboRigorousAdjustments.TabIndex = 289;
			// 
			// checkPaymentWindowDefaultHideSplits
			// 
			this.checkPaymentWindowDefaultHideSplits.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPaymentWindowDefaultHideSplits.Location = new System.Drawing.Point(8, 383);
			this.checkPaymentWindowDefaultHideSplits.Name = "checkPaymentWindowDefaultHideSplits";
			this.checkPaymentWindowDefaultHideSplits.Size = new System.Drawing.Size(283, 20);
			this.checkPaymentWindowDefaultHideSplits.TabIndex = 284;
			this.checkPaymentWindowDefaultHideSplits.Text = "Hide paysplits from payment window by default";
			// 
			// textClaimReportReceiveInterval
			// 
			this.textClaimReportReceiveInterval.Location = new System.Drawing.Point(256, 275);
			this.textClaimReportReceiveInterval.MaxLength = 2147483647;
			this.textClaimReportReceiveInterval.Name = "textClaimReportReceiveInterval";
			this.textClaimReportReceiveInterval.Size = new System.Drawing.Size(28, 23);
			this.textClaimReportReceiveInterval.TabIndex = 299;
			// 
			// label41
			// 
			this.label41.Location = new System.Drawing.Point(24, 356);
			this.label41.Name = "label41";
			this.label41.Size = new System.Drawing.Size(226, 23);
			this.label41.TabIndex = 288;
			this.label41.Text = "Adjustments Allocation";
			this.label41.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelAgingServiceTimeDue
			// 
			this.labelAgingServiceTimeDue.Location = new System.Drawing.Point(24, 56);
			this.labelAgingServiceTimeDue.Name = "labelAgingServiceTimeDue";
			this.labelAgingServiceTimeDue.Size = new System.Drawing.Size(226, 23);
			this.labelAgingServiceTimeDue.TabIndex = 294;
			this.labelAgingServiceTimeDue.Text = "Aging Service Time Due";
			this.labelAgingServiceTimeDue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboRigorousAccounting
			// 
			this.comboRigorousAccounting.Location = new System.Drawing.Point(256, 329);
			this.comboRigorousAccounting.Name = "comboRigorousAccounting";
			this.comboRigorousAccounting.Size = new System.Drawing.Size(230, 23);
			this.comboRigorousAccounting.TabIndex = 287;
			// 
			// label39
			// 
			this.label39.Location = new System.Drawing.Point(24, 329);
			this.label39.Name = "label39";
			this.label39.Size = new System.Drawing.Size(226, 23);
			this.label39.TabIndex = 285;
			this.label39.Text = "Paysplits Allocation";
			this.label39.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkBillingShowTransSinceBalZero
			// 
			this.checkBillingShowTransSinceBalZero.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkBillingShowTransSinceBalZero.Location = new System.Drawing.Point(8, 134);
			this.checkBillingShowTransSinceBalZero.Name = "checkBillingShowTransSinceBalZero";
			this.checkBillingShowTransSinceBalZero.Size = new System.Drawing.Size(240, 20);
			this.checkBillingShowTransSinceBalZero.TabIndex = 290;
			this.checkBillingShowTransSinceBalZero.Text = "Show all transactions since zero balance";
			// 
			// labelReportheckUnits
			// 
			this.labelReportheckUnits.Location = new System.Drawing.Point(290, 275);
			this.labelReportheckUnits.Name = "labelReportheckUnits";
			this.labelReportheckUnits.Size = new System.Drawing.Size(104, 23);
			this.labelReportheckUnits.TabIndex = 300;
			this.labelReportheckUnits.Text = "minutes (5 to 60)";
			this.labelReportheckUnits.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textAgingServiceTimeDue
			// 
			this.textAgingServiceTimeDue.Location = new System.Drawing.Point(256, 56);
			this.textAgingServiceTimeDue.Name = "textAgingServiceTimeDue";
			this.textAgingServiceTimeDue.ReadOnly = true;
			this.textAgingServiceTimeDue.Size = new System.Drawing.Size(230, 23);
			this.textAgingServiceTimeDue.TabIndex = 295;
			// 
			// radioReceiveAtAnInterval
			// 
			this.radioReceiveAtAnInterval.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.radioReceiveAtAnInterval.Checked = true;
			this.radioReceiveAtAnInterval.Location = new System.Drawing.Point(8, 276);
			this.radioReceiveAtAnInterval.Name = "radioReceiveAtAnInterval";
			this.radioReceiveAtAnInterval.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.radioReceiveAtAnInterval.Size = new System.Drawing.Size(136, 20);
			this.radioReceiveAtAnInterval.TabIndex = 297;
			this.radioReceiveAtAnInterval.TabStop = true;
			this.radioReceiveAtAnInterval.Text = "Receive at an interval";
			this.radioReceiveAtAnInterval.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.radioReceiveAtAnInterval.UseVisualStyleBackColor = true;
			this.radioReceiveAtAnInterval.CheckedChanged += new System.EventHandler(this.radioInterval_CheckedChanged);
			// 
			// comboPaymentClinicSetting
			// 
			this.comboPaymentClinicSetting.Location = new System.Drawing.Point(256, 83);
			this.comboPaymentClinicSetting.Name = "comboPaymentClinicSetting";
			this.comboPaymentClinicSetting.Size = new System.Drawing.Size(230, 23);
			this.comboPaymentClinicSetting.TabIndex = 281;
			this.comboPaymentClinicSetting.SelectionChangeCommitted += new System.EventHandler(this.comboPaymentClinicSetting_ChangeCommitted);
			// 
			// radioReceiveAtASetTime
			// 
			this.radioReceiveAtASetTime.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.radioReceiveAtASetTime.Location = new System.Drawing.Point(8, 303);
			this.radioReceiveAtASetTime.Name = "radioReceiveAtASetTime";
			this.radioReceiveAtASetTime.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.radioReceiveAtASetTime.Size = new System.Drawing.Size(132, 20);
			this.radioReceiveAtASetTime.TabIndex = 298;
			this.radioReceiveAtASetTime.Text = "Receive at a set time";
			this.radioReceiveAtASetTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.radioReceiveAtASetTime.UseVisualStyleBackColor = true;
			// 
			// checkPaymentsPromptForPayType
			// 
			this.checkPaymentsPromptForPayType.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPaymentsPromptForPayType.Location = new System.Drawing.Point(8, 110);
			this.checkPaymentsPromptForPayType.Name = "checkPaymentsPromptForPayType";
			this.checkPaymentsPromptForPayType.Size = new System.Drawing.Size(219, 20);
			this.checkPaymentsPromptForPayType.TabIndex = 280;
			this.checkPaymentsPromptForPayType.Text = "Payments prompt for Payment Type";
			this.checkPaymentsPromptForPayType.Click += new System.EventHandler(this.checkPaymentsPromptForPayType_Click);
			// 
			// textClaimReportReceiveTime
			// 
			this.textClaimReportReceiveTime.Enabled = false;
			this.textClaimReportReceiveTime.Location = new System.Drawing.Point(256, 302);
			this.textClaimReportReceiveTime.Name = "textClaimReportReceiveTime";
			this.textClaimReportReceiveTime.Size = new System.Drawing.Size(230, 23);
			this.textClaimReportReceiveTime.TabIndex = 296;
			// 
			// label38
			// 
			this.label38.Location = new System.Drawing.Point(24, 83);
			this.label38.Name = "label38";
			this.label38.Size = new System.Drawing.Size(226, 23);
			this.label38.TabIndex = 282;
			this.label38.Text = "Patient Payments Use";
			this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBoxClaimIdPrefix
			// 
			this.groupBoxClaimIdPrefix.Controls.Add(this.butReplacements);
			this.groupBoxClaimIdPrefix.Controls.Add(this.textClaimIdPrefix);
			this.groupBoxClaimIdPrefix.Location = new System.Drawing.Point(8, 162);
			this.groupBoxClaimIdPrefix.Name = "groupBoxClaimIdPrefix";
			this.groupBoxClaimIdPrefix.Size = new System.Drawing.Size(478, 81);
			this.groupBoxClaimIdPrefix.TabIndex = 291;
			this.groupBoxClaimIdPrefix.Text = "Claim Identification Prefix";
			// 
			// butReplacements
			// 
			this.butReplacements.Location = new System.Drawing.Point(377, 50);
			this.butReplacements.Name = "butReplacements";
			this.butReplacements.Size = new System.Drawing.Size(93, 23);
			this.butReplacements.TabIndex = 240;
			this.butReplacements.Text = "Replacements";
			this.butReplacements.UseVisualStyleBackColor = true;
			this.butReplacements.Click += new System.EventHandler(this.butReplacements_Click);
			// 
			// textClaimIdPrefix
			// 
			this.textClaimIdPrefix.Location = new System.Drawing.Point(248, 23);
			this.textClaimIdPrefix.Name = "textClaimIdPrefix";
			this.textClaimIdPrefix.Size = new System.Drawing.Size(222, 23);
			this.textClaimIdPrefix.TabIndex = 238;
			this.textClaimIdPrefix.Validating += new System.ComponentModel.CancelEventHandler(this.textClaimIdPrefix_Validating);
			// 
			// UserControlEnterpriseAccount
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.checkAgingCalculateOnBatchClaimReceipt);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.label27);
			this.Controls.Add(this.checkAgingReportShowAgePatPayplanPayments);
			this.Controls.Add(this.textBillingElectBatchMax);
			this.Controls.Add(this.comboPayPlansVersion);
			this.Controls.Add(this.checkClaimReportReceivedByService);
			this.Controls.Add(this.comboRigorousAdjustments);
			this.Controls.Add(this.checkPaymentWindowDefaultHideSplits);
			this.Controls.Add(this.textClaimReportReceiveInterval);
			this.Controls.Add(this.label41);
			this.Controls.Add(this.labelAgingServiceTimeDue);
			this.Controls.Add(this.comboRigorousAccounting);
			this.Controls.Add(this.label39);
			this.Controls.Add(this.checkBillingShowTransSinceBalZero);
			this.Controls.Add(this.labelReportheckUnits);
			this.Controls.Add(this.textAgingServiceTimeDue);
			this.Controls.Add(this.radioReceiveAtAnInterval);
			this.Controls.Add(this.comboPaymentClinicSetting);
			this.Controls.Add(this.radioReceiveAtASetTime);
			this.Controls.Add(this.checkPaymentsPromptForPayType);
			this.Controls.Add(this.textClaimReportReceiveTime);
			this.Controls.Add(this.label38);
			this.Controls.Add(this.groupBoxClaimIdPrefix);
			this.Name = "UserControlEnterpriseAccount";
			this.Size = new System.Drawing.Size(494, 660);
			this.groupBoxClaimIdPrefix.ResumeLayout(false);
			this.groupBoxClaimIdPrefix.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.CheckBox checkAgingCalculateOnBatchClaimReceipt;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label27;
		private UI.CheckBox checkAgingReportShowAgePatPayplanPayments;
		private ValidNum textBillingElectBatchMax;
		private UI.ComboBox comboPayPlansVersion;
		private UI.CheckBox checkClaimReportReceivedByService;
		private UI.ComboBox comboRigorousAdjustments;
		private UI.CheckBox checkPaymentWindowDefaultHideSplits;
		private System.Windows.Forms.TextBox textClaimReportReceiveInterval;
		private System.Windows.Forms.Label label41;
		private System.Windows.Forms.Label labelAgingServiceTimeDue;
		private UI.ComboBox comboRigorousAccounting;
		private System.Windows.Forms.Label label39;
		private UI.CheckBox checkBillingShowTransSinceBalZero;
		private System.Windows.Forms.Label labelReportheckUnits;
		private ValidDate textAgingServiceTimeDue;
		private System.Windows.Forms.RadioButton radioReceiveAtAnInterval;
		private UI.ComboBox comboPaymentClinicSetting;
		private System.Windows.Forms.RadioButton radioReceiveAtASetTime;
		private UI.CheckBox checkPaymentsPromptForPayType;
		private ValidTime textClaimReportReceiveTime;
		private System.Windows.Forms.Label label38;
		private UI.GroupBox groupBoxClaimIdPrefix;
		private UI.Button butReplacements;
		private System.Windows.Forms.TextBox textClaimIdPrefix;
	}
}
