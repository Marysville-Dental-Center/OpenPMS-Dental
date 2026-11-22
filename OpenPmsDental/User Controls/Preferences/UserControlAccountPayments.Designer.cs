
namespace OpenPmsDental {
	partial class UserControlAccountPayments {
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
			this.groupBoxUnearnedIncome = new OpenPmsDental.UI.GroupBox();
			this.checkShowAllocateUnearnedPaymentPrompt = new OpenPmsDental.UI.CheckBox();
			this.checkAllowPrepayProvider = new OpenPmsDental.UI.CheckBox();
			this.comboUnallocatedSplits = new OpenPmsDental.UI.ComboBox();
			this.label28 = new System.Windows.Forms.Label();
			this.groupBoxPayPlans = new OpenPmsDental.UI.GroupBox();
			this.checkPayPlanRequireLockForAPR = new OpenPmsDental.UI.CheckBox();
			this.checkPayPlansExcludePastActivity = new OpenPmsDental.UI.CheckBox();
			this.labelTermsAndConditions = new System.Windows.Forms.Label();
			this.butPayPlanTermsAndConditions = new OpenPmsDental.UI.Button();
			this.checkPayPlansUseSheets = new OpenPmsDental.UI.CheckBox();
			this.checkPayPlanSaveSignedPdf = new OpenPmsDental.UI.CheckBox();
			this.label39 = new System.Windows.Forms.Label();
			this.comboPayPlansVersion = new OpenPmsDental.UI.ComboBox();
			this.comboDppUnearnedType = new OpenPmsDental.UI.ComboBox();
			this.checkHideDueNow = new OpenPmsDental.UI.CheckBox();
			this.label59 = new System.Windows.Forms.Label();
			this.label27 = new System.Windows.Forms.Label();
			this.textDynamicPayPlan = new OpenPmsDental.ValidTime();
			this.groupBoxPayments = new OpenPmsDental.UI.GroupBox();
			this.checkOnlinePaymentsMarkAsProcessed = new OpenPmsDental.UI.CheckBox();
			this.checkStoreCCTokens = new OpenPmsDental.UI.CheckBox();
			this.checkPaymentCompletedDisableMerchantButtons = new OpenPmsDental.UI.CheckBox();
			this.comboPaymentClinicSetting = new OpenPmsDental.UI.ComboBox();
			this.checkIncTxfrTreatNegProdAsIncome = new OpenPmsDental.UI.CheckBox();
			this.checkIncomeTransfersLimitToPatPayments = new OpenPmsDental.UI.CheckBox();
			this.label38 = new System.Windows.Forms.Label();
			this.checkAllowEmailCCReceipt = new OpenPmsDental.UI.CheckBox();
			this.checkPaymentsPromptForPayType = new OpenPmsDental.UI.CheckBox();
			this.checkPayPlanItemDateShowProc = new OpenPmsDental.UI.CheckBox();
			this.groupBoxUnearnedIncome.SuspendLayout();
			this.groupBoxPayPlans.SuspendLayout();
			this.groupBoxPayments.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxUnearnedIncome
			// 
			this.groupBoxUnearnedIncome.Controls.Add(this.comboUnallocatedSplits);
			this.groupBoxUnearnedIncome.Controls.Add(this.checkShowAllocateUnearnedPaymentPrompt);
			this.groupBoxUnearnedIncome.Controls.Add(this.checkAllowPrepayProvider);
			this.groupBoxUnearnedIncome.Controls.Add(this.label28);
			this.groupBoxUnearnedIncome.Location = new System.Drawing.Point(8, 539);
			this.groupBoxUnearnedIncome.Name = "groupBoxUnearnedIncome";
			this.groupBoxUnearnedIncome.Size = new System.Drawing.Size(478, 106);
			this.groupBoxUnearnedIncome.TabIndex = 323;
			this.groupBoxUnearnedIncome.Text = "Unearned Income";
			// 
			// checkShowAllocateUnearnedPaymentPrompt
			// 
			this.checkShowAllocateUnearnedPaymentPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkShowAllocateUnearnedPaymentPrompt.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkShowAllocateUnearnedPaymentPrompt.Location = new System.Drawing.Point(8, 78);
			this.checkShowAllocateUnearnedPaymentPrompt.Name = "checkShowAllocateUnearnedPaymentPrompt";
			this.checkShowAllocateUnearnedPaymentPrompt.Size = new System.Drawing.Size(366, 20);
			this.checkShowAllocateUnearnedPaymentPrompt.TabIndex = 307;
			this.checkShowAllocateUnearnedPaymentPrompt.Text = "Prompt user to allocate unearned income after creating a claim";
			// 
			// checkAllowPrepayProvider
			// 
			this.checkAllowPrepayProvider.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAllowPrepayProvider.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAllowPrepayProvider.Location = new System.Drawing.Point(8, 23);
			this.checkAllowPrepayProvider.Name = "checkAllowPrepayProvider";
			this.checkAllowPrepayProvider.Size = new System.Drawing.Size(275, 20);
			this.checkAllowPrepayProvider.TabIndex = 303;
			this.checkAllowPrepayProvider.Text = "Allow assigning unearned income to providers";
			// 
			// comboUnallocatedSplits
			// 
			this.comboUnallocatedSplits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboUnallocatedSplits.Location = new System.Drawing.Point(315, 49);
			this.comboUnallocatedSplits.Name = "comboUnallocatedSplits";
			this.comboUnallocatedSplits.Size = new System.Drawing.Size(124, 23);
			this.comboUnallocatedSplits.TabIndex = 281;
			// 
			// label28
			// 
			this.label28.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label28.Location = new System.Drawing.Point(49, 49);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(260, 23);
			this.label28.TabIndex = 282;
			this.label28.Text = "Default unearned type for unallocated paysplits";
			this.label28.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxPayPlans
			// 
			this.groupBoxPayPlans.Controls.Add(this.checkPayPlanItemDateShowProc);
			this.groupBoxPayPlans.Controls.Add(this.checkPayPlanRequireLockForAPR);
			this.groupBoxPayPlans.Controls.Add(this.comboPayPlansVersion);
			this.groupBoxPayPlans.Controls.Add(this.comboDppUnearnedType);
			this.groupBoxPayPlans.Controls.Add(this.textDynamicPayPlan);
			this.groupBoxPayPlans.Controls.Add(this.checkPayPlansExcludePastActivity);
			this.groupBoxPayPlans.Controls.Add(this.labelTermsAndConditions);
			this.groupBoxPayPlans.Controls.Add(this.butPayPlanTermsAndConditions);
			this.groupBoxPayPlans.Controls.Add(this.checkPayPlansUseSheets);
			this.groupBoxPayPlans.Controls.Add(this.checkPayPlanSaveSignedPdf);
			this.groupBoxPayPlans.Controls.Add(this.label39);
			this.groupBoxPayPlans.Controls.Add(this.checkHideDueNow);
			this.groupBoxPayPlans.Controls.Add(this.label59);
			this.groupBoxPayPlans.Controls.Add(this.label27);
			this.groupBoxPayPlans.Location = new System.Drawing.Point(8, 242);
			this.groupBoxPayPlans.Name = "groupBoxPayPlans";
			this.groupBoxPayPlans.Size = new System.Drawing.Size(478, 289);
			this.groupBoxPayPlans.TabIndex = 322;
			this.groupBoxPayPlans.Text = "Pay Plans";
			// 
			// checkPayPlanRequireLockForAPR
			// 
			this.checkPayPlanRequireLockForAPR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPayPlanRequireLockForAPR.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPayPlanRequireLockForAPR.Location = new System.Drawing.Point(8, 237);
			this.checkPayPlanRequireLockForAPR.Name = "checkPayPlanRequireLockForAPR";
			this.checkPayPlanRequireLockForAPR.Size = new System.Drawing.Size(424, 20);
			this.checkPayPlanRequireLockForAPR.TabIndex = 322;
			this.checkPayPlanRequireLockForAPR.Text = "Require permanent lock to save a Payment Plan if there is an APR entered";
			// checkPayPlansExcludePastActivity
			// 
			this.checkPayPlansExcludePastActivity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPayPlansExcludePastActivity.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPayPlansExcludePastActivity.Location = new System.Drawing.Point(8, 23);
			this.checkPayPlansExcludePastActivity.Name = "checkPayPlansExcludePastActivity";
			this.checkPayPlansExcludePastActivity.Size = new System.Drawing.Size(273, 20);
			this.checkPayPlansExcludePastActivity.TabIndex = 310;
			this.checkPayPlansExcludePastActivity.Text = "Payment Plans exclude past activity by default";
			// 
			// labelTermsAndConditions
			// 
			this.labelTermsAndConditions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelTermsAndConditions.Location = new System.Drawing.Point(162, 208);
			this.labelTermsAndConditions.Name = "labelTermsAndConditions";
			this.labelTermsAndConditions.Size = new System.Drawing.Size(204, 23);
			this.labelTermsAndConditions.TabIndex = 321;
			this.labelTermsAndConditions.Text = "Payment Plan Terms and Conditions";
			this.labelTermsAndConditions.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butPayPlanTermsAndConditions
			// 
			this.butPayPlanTermsAndConditions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butPayPlanTermsAndConditions.Location = new System.Drawing.Point(372, 208);
			this.butPayPlanTermsAndConditions.Name = "butPayPlanTermsAndConditions";
			this.butPayPlanTermsAndConditions.Size = new System.Drawing.Size(68, 23);
			this.butPayPlanTermsAndConditions.TabIndex = 317;
			this.butPayPlanTermsAndConditions.Text = "Edit";
			this.butPayPlanTermsAndConditions.Click += new System.EventHandler(this.butPayPlanTermsAndConditions_Click);
			// 
			// checkPayPlansUseSheets
			// 
			this.checkPayPlansUseSheets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPayPlansUseSheets.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPayPlansUseSheets.Location = new System.Drawing.Point(8, 47);
			this.checkPayPlansUseSheets.Name = "checkPayPlansUseSheets";
			this.checkPayPlansUseSheets.Size = new System.Drawing.Size(200, 20);
			this.checkPayPlansUseSheets.TabIndex = 311;
			this.checkPayPlansUseSheets.Text = "Pay Plans use Sheets for printing";
			// 
			// checkPayPlanSaveSignedPdf
			// 
			this.checkPayPlanSaveSignedPdf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPayPlanSaveSignedPdf.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPayPlanSaveSignedPdf.Location = new System.Drawing.Point(8, 71);
			this.checkPayPlanSaveSignedPdf.Name = "checkPayPlanSaveSignedPdf";
			this.checkPayPlanSaveSignedPdf.Size = new System.Drawing.Size(367, 20);
			this.checkPayPlanSaveSignedPdf.TabIndex = 312;
			this.checkPayPlanSaveSignedPdf.Text = "In eClipboard and ODTouch, save signed Payment Plans as PDF";
			// 
			// label39
			// 
			this.label39.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label39.Location = new System.Drawing.Point(76, 179);
			this.label39.Name = "label39";
			this.label39.Size = new System.Drawing.Size(164, 23);
			this.label39.TabIndex = 320;
			this.label39.Text = "Payment plan unearned type";
			this.label39.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboPayPlansVersion
			// 
			this.comboPayPlansVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboPayPlansVersion.Location = new System.Drawing.Point(241, 97);
			this.comboPayPlansVersion.Name = "comboPayPlansVersion";
			this.comboPayPlansVersion.Size = new System.Drawing.Size(194, 21);
			this.comboPayPlansVersion.TabIndex = 313;
			this.comboPayPlansVersion.Visible = false;
			this.comboPayPlansVersion.SelectionChangeCommitted += new System.EventHandler(this.comboPayPlansVersion_SelectionChangeCommitted);
			// 
			// comboDppUnearnedType
			// 
			this.comboDppUnearnedType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboDppUnearnedType.Location = new System.Drawing.Point(246, 179);
			this.comboDppUnearnedType.Name = "comboDppUnearnedType";
			this.comboDppUnearnedType.Size = new System.Drawing.Size(192, 23);
			this.comboDppUnearnedType.TabIndex = 316;
			this.comboDppUnearnedType.Text = "comboBoxOD1";
			// 
			// checkHideDueNow
			// 
			this.checkHideDueNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkHideDueNow.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkHideDueNow.Location = new System.Drawing.Point(8, 124);
			this.checkHideDueNow.Name = "checkHideDueNow";
			this.checkHideDueNow.Size = new System.Drawing.Size(235, 20);
			this.checkHideDueNow.TabIndex = 314;
			this.checkHideDueNow.Text = "Hide \"Due Now\" in Payment Plans Grid";
			// 
			// label59
			// 
			this.label59.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label59.Location = new System.Drawing.Point(262, 150);
			this.label59.Name = "label59";
			this.label59.Size = new System.Drawing.Size(104, 23);
			this.label59.TabIndex = 319;
			this.label59.Text = "Pay Plan run time";
			this.label59.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label27
			// 
			this.label27.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label27.Location = new System.Drawing.Point(8, 100);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(427, 17);
			this.label27.TabIndex = 318;
			this.label27.Text = "Pay Plan charge logic";
			this.label27.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDynamicPayPlan
			// 
			this.textDynamicPayPlan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textDynamicPayPlan.Location = new System.Drawing.Point(372, 150);
			this.textDynamicPayPlan.Name = "textDynamicPayPlan";
			this.textDynamicPayPlan.Size = new System.Drawing.Size(68, 20);
			this.textDynamicPayPlan.TabIndex = 315;
			// 
			// groupBoxPayments
			// 
			this.groupBoxPayments.Controls.Add(this.comboPaymentClinicSetting);
			this.groupBoxPayments.Controls.Add(this.checkOnlinePaymentsMarkAsProcessed);
			this.groupBoxPayments.Controls.Add(this.checkStoreCCTokens);
			this.groupBoxPayments.Controls.Add(this.checkPaymentCompletedDisableMerchantButtons);
			this.groupBoxPayments.Controls.Add(this.checkIncTxfrTreatNegProdAsIncome);
			this.groupBoxPayments.Controls.Add(this.checkIncomeTransfersLimitToPatPayments);
			this.groupBoxPayments.Controls.Add(this.label38);
			this.groupBoxPayments.Controls.Add(this.checkAllowEmailCCReceipt);
			this.groupBoxPayments.Controls.Add(this.checkPaymentsPromptForPayType);
			this.groupBoxPayments.Location = new System.Drawing.Point(8, 8);
			this.groupBoxPayments.Name = "groupBoxPayments";
			this.groupBoxPayments.Size = new System.Drawing.Size(478, 226);
			this.groupBoxPayments.TabIndex = 307;
			this.groupBoxPayments.Text = "Payments";
			// 
			// checkOnlinePaymentsMarkAsProcessed
			// 
			this.checkOnlinePaymentsMarkAsProcessed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkOnlinePaymentsMarkAsProcessed.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkOnlinePaymentsMarkAsProcessed.Location = new System.Drawing.Point(8, 174);
			this.checkOnlinePaymentsMarkAsProcessed.Name = "checkOnlinePaymentsMarkAsProcessed";
			this.checkOnlinePaymentsMarkAsProcessed.Size = new System.Drawing.Size(217, 20);
			this.checkOnlinePaymentsMarkAsProcessed.TabIndex = 308;
			this.checkOnlinePaymentsMarkAsProcessed.Text = "Mark online payments as processed";
			// 
			// checkStoreCCTokens
			// 
			this.checkStoreCCTokens.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkStoreCCTokens.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkStoreCCTokens.Location = new System.Drawing.Point(8, 23);
			this.checkStoreCCTokens.Name = "checkStoreCCTokens";
			this.checkStoreCCTokens.Size = new System.Drawing.Size(229, 20);
			this.checkStoreCCTokens.TabIndex = 280;
			this.checkStoreCCTokens.Text = "Automatically store credit card tokens";
			// 
			// checkPaymentCompletedDisableMerchantButtons
			// 
			this.checkPaymentCompletedDisableMerchantButtons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPaymentCompletedDisableMerchantButtons.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPaymentCompletedDisableMerchantButtons.Location = new System.Drawing.Point(8, 150);
			this.checkPaymentCompletedDisableMerchantButtons.Name = "checkPaymentCompletedDisableMerchantButtons";
			this.checkPaymentCompletedDisableMerchantButtons.Size = new System.Drawing.Size(300, 20);
			this.checkPaymentCompletedDisableMerchantButtons.TabIndex = 306;
			this.checkPaymentCompletedDisableMerchantButtons.Text = "Disable merchant buttons for completed payments";
			// 
			// comboPaymentClinicSetting
			// 
			this.comboPaymentClinicSetting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboPaymentClinicSetting.Location = new System.Drawing.Point(277, 49);
			this.comboPaymentClinicSetting.Name = "comboPaymentClinicSetting";
			this.comboPaymentClinicSetting.Size = new System.Drawing.Size(164, 23);
			this.comboPaymentClinicSetting.TabIndex = 290;
			this.comboPaymentClinicSetting.SelectionChangeCommitted += new System.EventHandler(this.comboPaymentClinicSetting_ChangeCommitted);
			// 
			// checkIncTxfrTreatNegProdAsIncome
			// 
			this.checkIncTxfrTreatNegProdAsIncome.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkIncTxfrTreatNegProdAsIncome.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIncTxfrTreatNegProdAsIncome.Location = new System.Drawing.Point(8, 126);
			this.checkIncTxfrTreatNegProdAsIncome.Name = "checkIncTxfrTreatNegProdAsIncome";
			this.checkIncTxfrTreatNegProdAsIncome.Size = new System.Drawing.Size(314, 20);
			this.checkIncTxfrTreatNegProdAsIncome.TabIndex = 305;
			this.checkIncTxfrTreatNegProdAsIncome.Text = "Income transfers treat negative production as income";
			// 
			// checkIncomeTransfersLimitToPatPayments
			// 
			this.checkIncomeTransfersLimitToPatPayments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkIncomeTransfersLimitToPatPayments.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIncomeTransfersLimitToPatPayments.Location = new System.Drawing.Point(8, 198);
			this.checkIncomeTransfersLimitToPatPayments.Name = "checkIncomeTransfersLimitToPatPayments";
			this.checkIncomeTransfersLimitToPatPayments.Size = new System.Drawing.Size(381, 20);
			this.checkIncomeTransfersLimitToPatPayments.TabIndex = 309;
			this.checkIncomeTransfersLimitToPatPayments.Text = "Limit manual income transfers to the amount of patient payments";
			// 
			// label38
			// 
			this.label38.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label38.Location = new System.Drawing.Point(75, 49);
			this.label38.Margin = new System.Windows.Forms.Padding(0);
			this.label38.Name = "label38";
			this.label38.Size = new System.Drawing.Size(196, 23);
			this.label38.TabIndex = 291;
			this.label38.Text = "Default Clinic for patient payments";
			this.label38.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkAllowEmailCCReceipt
			// 
			this.checkAllowEmailCCReceipt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAllowEmailCCReceipt.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAllowEmailCCReceipt.Location = new System.Drawing.Point(8, 102);
			this.checkAllowEmailCCReceipt.Name = "checkAllowEmailCCReceipt";
			this.checkAllowEmailCCReceipt.Size = new System.Drawing.Size(210, 20);
			this.checkAllowEmailCCReceipt.TabIndex = 292;
			this.checkAllowEmailCCReceipt.Text = "Allow emailing credit card receipts";
			// 
			// checkPaymentsPromptForPayType
			// 
			this.checkPaymentsPromptForPayType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPaymentsPromptForPayType.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPaymentsPromptForPayType.Location = new System.Drawing.Point(8, 78);
			this.checkPaymentsPromptForPayType.Name = "checkPaymentsPromptForPayType";
			this.checkPaymentsPromptForPayType.Size = new System.Drawing.Size(219, 20);
			this.checkPaymentsPromptForPayType.TabIndex = 284;
			this.checkPaymentsPromptForPayType.Text = "Payments prompt for Payment Type";
			this.checkPaymentsPromptForPayType.Click += new System.EventHandler(this.checkPaymentsPromptForPayType_Click);
			// 
			// checkPayPlanItemDateShowProc
			// 
			this.checkPayPlanItemDateShowProc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPayPlanItemDateShowProc.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPayPlanItemDateShowProc.Location = new System.Drawing.Point(8, 261);
			this.checkPayPlanItemDateShowProc.Name = "checkPayPlanItemDateShowProc";
			this.checkPayPlanItemDateShowProc.Size = new System.Drawing.Size(240, 20);
			this.checkPayPlanItemDateShowProc.TabIndex = 331;
			this.checkPayPlanItemDateShowProc.Text = "Use date of production as date showing";
			// 
			// UserControlAccountPayments
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.groupBoxUnearnedIncome);
			this.Controls.Add(this.groupBoxPayPlans);
			this.Controls.Add(this.groupBoxPayments);
			this.Name = "UserControlAccountPayments";
			this.Size = new System.Drawing.Size(494, 660);
			this.groupBoxUnearnedIncome.ResumeLayout(false);
			this.groupBoxPayPlans.ResumeLayout(false);
			this.groupBoxPayPlans.PerformLayout();
			this.groupBoxPayments.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private OpenPmsDental.UI.CheckBox checkPaymentCompletedDisableMerchantButtons;
		private OpenPmsDental.UI.CheckBox checkIncTxfrTreatNegProdAsIncome;
		private OpenPmsDental.UI.CheckBox checkIncomeTransfersLimitToPatPayments;
		private OpenPmsDental.UI.CheckBox checkStoreCCTokens;
		private UI.ComboBox comboPaymentClinicSetting;
		private System.Windows.Forms.Label label38;
		private OpenPmsDental.UI.CheckBox checkPaymentsPromptForPayType;
		private OpenPmsDental.UI.CheckBox checkAllowPrepayProvider;
		private UI.ComboBox comboUnallocatedSplits;
		private System.Windows.Forms.Label label28;
		private OpenPmsDental.UI.CheckBox checkAllowEmailCCReceipt;
		private UI.GroupBox groupBoxPayments;
		private UI.Button butPayPlanTermsAndConditions;
		private System.Windows.Forms.Label labelTermsAndConditions;
		private OpenPmsDental.UI.CheckBox checkPayPlansExcludePastActivity;
		private OpenPmsDental.UI.CheckBox checkPayPlanSaveSignedPdf;
		private OpenPmsDental.UI.CheckBox checkPayPlansUseSheets;
		private System.Windows.Forms.Label label39;
		private OpenPmsDental.UI.CheckBox checkHideDueNow;
		private UI.ComboBox comboDppUnearnedType;
		private UI.ComboBox comboPayPlansVersion;
		private System.Windows.Forms.Label label59;
		private System.Windows.Forms.Label label27;
		private ValidTime textDynamicPayPlan;
		private UI.GroupBox groupBoxPayPlans;
		private OpenPmsDental.UI.CheckBox checkShowAllocateUnearnedPaymentPrompt;
		private UI.GroupBox groupBoxUnearnedIncome;
		private OpenPmsDental.UI.CheckBox checkOnlinePaymentsMarkAsProcessed;
		private UI.CheckBox checkPayPlanRequireLockForAPR;
		private UI.CheckBox checkPayPlanItemDateShowProc;
	}
}
