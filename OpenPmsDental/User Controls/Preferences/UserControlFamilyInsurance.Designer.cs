
namespace OpenPmsDental {
	partial class UserControlFamilyInsurance {
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
			this.groupBoxPerVisitProcedureCodes = new OpenPmsDental.UI.GroupBox();
			this.butPickPerVisitInsAmountProcCode = new OpenPmsDental.UI.Button();
			this.butPickPerVisitPatAmountProcCode = new OpenPmsDental.UI.Button();
			this.textPerVisitInsAmountProcCode = new System.Windows.Forms.TextBox();
			this.textPerVisitPatAmountProcCode = new System.Windows.Forms.TextBox();
			this.labelPerVisitInsAmount = new System.Windows.Forms.Label();
			this.labelPerVisitPatCopay = new System.Windows.Forms.Label();
			this.groupBoxCOB = new OpenPmsDental.UI.GroupBox();
			this.comboCobSendPaidByInsAt = new OpenPmsDental.UI.ComboBox();
			this.labelCobSendPaidByOtherInsAt = new System.Windows.Forms.Label();
			this.comboCobRule = new OpenPmsDental.UI.ComboBox();
			this.labelCobRule = new System.Windows.Forms.Label();
			this.groupBoxClaims = new OpenPmsDental.UI.GroupBox();
			this.checkClaimUseOverrideProcDescript = new OpenPmsDental.UI.CheckBox();
			this.checkClaimTrackingRequireError = new OpenPmsDental.UI.CheckBox();
			this.groupBoxOtherInsInfo = new OpenPmsDental.UI.GroupBox();
			this.checkInsPlanExclusionsUseUCR = new OpenPmsDental.UI.CheckBox();
			this.checkEnableZeroWriteoffOnLimitations = new OpenPmsDental.UI.CheckBox();
			this.checkEnableZeroWriteoffOnAnnualMax = new OpenPmsDental.UI.CheckBox();
			this.checkInsPlanExclusionsMarkDoNotBill = new OpenPmsDental.UI.CheckBox();
			this.checkInsDefaultShowUCRonClaims = new OpenPmsDental.UI.CheckBox();
			this.groupBoxInsuranceGeneral = new OpenPmsDental.UI.GroupBox();
			this.checkPatInitBillingTypeFromPriInsPlan = new OpenPmsDental.UI.CheckBox();
			this.checkInsurancePlansShared = new OpenPmsDental.UI.CheckBox();
			this.checkFixedBenefitBlankLikeZero = new OpenPmsDental.UI.CheckBox();
			this.checkCoPayFeeScheduleBlankLikeZero = new OpenPmsDental.UI.CheckBox();
			this.checkInsPPOsecWriteoffs = new OpenPmsDental.UI.CheckBox();
			this.checkPPOpercentage = new OpenPmsDental.UI.CheckBox();
			this.checkInsDefaultAssignmentOfBenefits = new OpenPmsDental.UI.CheckBox();
			this.checkOutOfNetworkBlankLikeZero = new OpenPmsDental.UI.CheckBox();
			this.groupBoxPerVisitProcedureCodes.SuspendLayout();
			this.groupBoxCOB.SuspendLayout();
			this.groupBoxClaims.SuspendLayout();
			this.groupBoxOtherInsInfo.SuspendLayout();
			this.groupBoxInsuranceGeneral.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBoxPerVisitProcedureCodes
			// 
			this.groupBoxPerVisitProcedureCodes.Controls.Add(this.textPerVisitInsAmountProcCode);
			this.groupBoxPerVisitProcedureCodes.Controls.Add(this.textPerVisitPatAmountProcCode);
			this.groupBoxPerVisitProcedureCodes.Controls.Add(this.butPickPerVisitInsAmountProcCode);
			this.groupBoxPerVisitProcedureCodes.Controls.Add(this.butPickPerVisitPatAmountProcCode);
			this.groupBoxPerVisitProcedureCodes.Controls.Add(this.labelPerVisitInsAmount);
			this.groupBoxPerVisitProcedureCodes.Controls.Add(this.labelPerVisitPatCopay);
			this.groupBoxPerVisitProcedureCodes.Location = new System.Drawing.Point(8, 562);
			this.groupBoxPerVisitProcedureCodes.Name = "groupBoxPerVisitProcedureCodes";
			this.groupBoxPerVisitProcedureCodes.Size = new System.Drawing.Size(478, 81);
			this.groupBoxPerVisitProcedureCodes.TabIndex = 379;
			this.groupBoxPerVisitProcedureCodes.Text = "Per Visit";
			// 
			// butPickPerVisitInsAmountProcCode
			// 
			this.butPickPerVisitInsAmountProcCode.Location = new System.Drawing.Point(330, 50);
			this.butPickPerVisitInsAmountProcCode.Name = "butPickPerVisitInsAmountProcCode";
			this.butPickPerVisitInsAmountProcCode.Size = new System.Drawing.Size(23, 23);
			this.butPickPerVisitInsAmountProcCode.TabIndex = 291;
			this.butPickPerVisitInsAmountProcCode.Text = "...";
			this.butPickPerVisitInsAmountProcCode.Click += new System.EventHandler(this.butPickPerVisitInsAmountProcCode_Click);
			// 
			// butPickPerVisitPatAmountProcCode
			// 
			this.butPickPerVisitPatAmountProcCode.Location = new System.Drawing.Point(330, 23);
			this.butPickPerVisitPatAmountProcCode.Name = "butPickPerVisitPatAmountProcCode";
			this.butPickPerVisitPatAmountProcCode.Size = new System.Drawing.Size(23, 23);
			this.butPickPerVisitPatAmountProcCode.TabIndex = 290;
			this.butPickPerVisitPatAmountProcCode.Text = "...";
			this.butPickPerVisitPatAmountProcCode.Click += new System.EventHandler(this.butPickPerVisitPatAmountProcCode_Click);
			// 
			// textPerVisitInsAmountProcCode
			// 
			this.textPerVisitInsAmountProcCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textPerVisitInsAmountProcCode.Location = new System.Drawing.Point(228, 50);
			this.textPerVisitInsAmountProcCode.Name = "textPerVisitInsAmountProcCode";
			this.textPerVisitInsAmountProcCode.Size = new System.Drawing.Size(96, 23);
			this.textPerVisitInsAmountProcCode.TabIndex = 289;
			this.textPerVisitInsAmountProcCode.TabStop = false;
			// 
			// textPerVisitPatAmountProcCode
			// 
			this.textPerVisitPatAmountProcCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textPerVisitPatAmountProcCode.Location = new System.Drawing.Point(228, 23);
			this.textPerVisitPatAmountProcCode.Name = "textPerVisitPatAmountProcCode";
			this.textPerVisitPatAmountProcCode.Size = new System.Drawing.Size(96, 23);
			this.textPerVisitPatAmountProcCode.TabIndex = 288;
			this.textPerVisitPatAmountProcCode.TabStop = false;
			// 
			// labelPerVisitInsAmount
			// 
			this.labelPerVisitInsAmount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelPerVisitInsAmount.Location = new System.Drawing.Point(24, 50);
			this.labelPerVisitInsAmount.Name = "labelPerVisitInsAmount";
			this.labelPerVisitInsAmount.Size = new System.Drawing.Size(198, 23);
			this.labelPerVisitInsAmount.TabIndex = 286;
			this.labelPerVisitInsAmount.Text = "Insurance procedure code";
			this.labelPerVisitInsAmount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelPerVisitPatCopay
			// 
			this.labelPerVisitPatCopay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelPerVisitPatCopay.Location = new System.Drawing.Point(24, 23);
			this.labelPerVisitPatCopay.Name = "labelPerVisitPatCopay";
			this.labelPerVisitPatCopay.Size = new System.Drawing.Size(198, 23);
			this.labelPerVisitPatCopay.TabIndex = 264;
			this.labelPerVisitPatCopay.Text = "Patient copay procedure code";
			this.labelPerVisitPatCopay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBoxCOB
			// 
			this.groupBoxCOB.Controls.Add(this.comboCobSendPaidByInsAt);
			this.groupBoxCOB.Controls.Add(this.comboCobRule);
			this.groupBoxCOB.Controls.Add(this.labelCobSendPaidByOtherInsAt);
			this.groupBoxCOB.Controls.Add(this.labelCobRule);
			this.groupBoxCOB.Location = new System.Drawing.Point(8, 473);
			this.groupBoxCOB.Name = "groupBoxCOB";
			this.groupBoxCOB.Size = new System.Drawing.Size(478, 81);
			this.groupBoxCOB.TabIndex = 376;
			this.groupBoxCOB.Text = "Coordination of Benefits (COB)";
			// 
			// comboCobSendPaidByInsAt
			// 
			this.comboCobSendPaidByInsAt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboCobSendPaidByInsAt.Location = new System.Drawing.Point(228, 50);
			this.comboCobSendPaidByInsAt.Name = "comboCobSendPaidByInsAt";
			this.comboCobSendPaidByInsAt.Size = new System.Drawing.Size(242, 23);
			this.comboCobSendPaidByInsAt.TabIndex = 287;
			// 
			// labelCobSendPaidByOtherInsAt
			// 
			this.labelCobSendPaidByOtherInsAt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelCobSendPaidByOtherInsAt.Location = new System.Drawing.Point(24, 50);
			this.labelCobSendPaidByOtherInsAt.Name = "labelCobSendPaidByOtherInsAt";
			this.labelCobSendPaidByOtherInsAt.Size = new System.Drawing.Size(198, 23);
			this.labelCobSendPaidByOtherInsAt.TabIndex = 286;
			this.labelCobSendPaidByOtherInsAt.Text = "Send Paid By Other Insurance At";
			this.labelCobSendPaidByOtherInsAt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboCobRule
			// 
			this.comboCobRule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboCobRule.Location = new System.Drawing.Point(228, 23);
			this.comboCobRule.Name = "comboCobRule";
			this.comboCobRule.Size = new System.Drawing.Size(242, 23);
			this.comboCobRule.TabIndex = 262;
			this.comboCobRule.SelectionChangeCommitted += new System.EventHandler(this.comboCobRule_SelectionChangeCommitted);
			// 
			// labelCobRule
			// 
			this.labelCobRule.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelCobRule.Location = new System.Drawing.Point(24, 23);
			this.labelCobRule.Name = "labelCobRule";
			this.labelCobRule.Size = new System.Drawing.Size(198, 23);
			this.labelCobRule.TabIndex = 264;
			this.labelCobRule.Text = "Coordination of Benefits (COB) rule";
			this.labelCobRule.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBoxClaims
			// 
			this.groupBoxClaims.Controls.Add(this.checkClaimUseOverrideProcDescript);
			this.groupBoxClaims.Controls.Add(this.checkClaimTrackingRequireError);
			this.groupBoxClaims.Location = new System.Drawing.Point(8, 390);
			this.groupBoxClaims.Name = "groupBoxClaims";
			this.groupBoxClaims.Size = new System.Drawing.Size(478, 75);
			this.groupBoxClaims.TabIndex = 280;
			this.groupBoxClaims.Text = "Claims";
			// 
			// checkClaimUseOverrideProcDescript
			// 
			this.checkClaimUseOverrideProcDescript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkClaimUseOverrideProcDescript.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkClaimUseOverrideProcDescript.Location = new System.Drawing.Point(8, 23);
			this.checkClaimUseOverrideProcDescript.Name = "checkClaimUseOverrideProcDescript";
			this.checkClaimUseOverrideProcDescript.Size = new System.Drawing.Size(399, 20);
			this.checkClaimUseOverrideProcDescript.TabIndex = 263;
			this.checkClaimUseOverrideProcDescript.Text = "Use the description for the charted procedure code on printed claims";
			// 
			// checkClaimTrackingRequireError
			// 
			this.checkClaimTrackingRequireError.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkClaimTrackingRequireError.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkClaimTrackingRequireError.Location = new System.Drawing.Point(8, 47);
			this.checkClaimTrackingRequireError.Name = "checkClaimTrackingRequireError";
			this.checkClaimTrackingRequireError.Size = new System.Drawing.Size(357, 20);
			this.checkClaimTrackingRequireError.TabIndex = 266;
			this.checkClaimTrackingRequireError.Text = "Require error code when adding claim custom tracking status";
			// 
			// groupBoxOtherInsInfo
			// 
			this.groupBoxOtherInsInfo.Controls.Add(this.checkInsPlanExclusionsUseUCR);
			this.groupBoxOtherInsInfo.Controls.Add(this.checkEnableZeroWriteoffOnLimitations);
			this.groupBoxOtherInsInfo.Controls.Add(this.checkEnableZeroWriteoffOnAnnualMax);
			this.groupBoxOtherInsInfo.Controls.Add(this.checkInsPlanExclusionsMarkDoNotBill);
			this.groupBoxOtherInsInfo.Location = new System.Drawing.Point(8, 259);
			this.groupBoxOtherInsInfo.Name = "groupBoxOtherInsInfo";
			this.groupBoxOtherInsInfo.Size = new System.Drawing.Size(478, 123);
			this.groupBoxOtherInsInfo.TabIndex = 279;
			this.groupBoxOtherInsInfo.Text = "Write-Offs for Non-Covered Services";
			// 
			// checkInsPlanExclusionsUseUCR
			// 
			this.checkInsPlanExclusionsUseUCR.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkInsPlanExclusionsUseUCR.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkInsPlanExclusionsUseUCR.Location = new System.Drawing.Point(8, 23);
			this.checkInsPlanExclusionsUseUCR.Name = "checkInsPlanExclusionsUseUCR";
			this.checkInsPlanExclusionsUseUCR.Size = new System.Drawing.Size(379, 20);
			this.checkInsPlanExclusionsUseUCR.TabIndex = 277;
			this.checkInsPlanExclusionsUseUCR.Text = "Ins plans with exclusions use standard provider fee (no write-offs)";
			// 
			// checkEnableZeroWriteoffOnLimitations
			// 
			this.checkEnableZeroWriteoffOnLimitations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkEnableZeroWriteoffOnLimitations.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEnableZeroWriteoffOnLimitations.Location = new System.Drawing.Point(8, 95);
			this.checkEnableZeroWriteoffOnLimitations.Name = "checkEnableZeroWriteoffOnLimitations";
			this.checkEnableZeroWriteoffOnLimitations.Size = new System.Drawing.Size(435, 20);
			this.checkEnableZeroWriteoffOnLimitations.TabIndex = 373;
			this.checkEnableZeroWriteoffOnLimitations.Text = "Ins plans use standard provider fee (no write-offs) at frequency or age limits";
			// 
			// checkEnableZeroWriteoffOnAnnualMax
			// 
			this.checkEnableZeroWriteoffOnAnnualMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkEnableZeroWriteoffOnAnnualMax.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEnableZeroWriteoffOnAnnualMax.Location = new System.Drawing.Point(8, 71);
			this.checkEnableZeroWriteoffOnAnnualMax.Name = "checkEnableZeroWriteoffOnAnnualMax";
			this.checkEnableZeroWriteoffOnAnnualMax.Size = new System.Drawing.Size(428, 20);
			this.checkEnableZeroWriteoffOnAnnualMax.TabIndex = 372;
			this.checkEnableZeroWriteoffOnAnnualMax.Text = "Ins plans use standard provider fee (no write-offs) when annual max is met";
			// 
			// checkInsPlanExclusionsMarkDoNotBill
			// 
			this.checkInsPlanExclusionsMarkDoNotBill.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkInsPlanExclusionsMarkDoNotBill.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkInsPlanExclusionsMarkDoNotBill.Location = new System.Drawing.Point(8, 47);
			this.checkInsPlanExclusionsMarkDoNotBill.Name = "checkInsPlanExclusionsMarkDoNotBill";
			this.checkInsPlanExclusionsMarkDoNotBill.Size = new System.Drawing.Size(283, 20);
			this.checkInsPlanExclusionsMarkDoNotBill.TabIndex = 276;
			this.checkInsPlanExclusionsMarkDoNotBill.Text = "Ins plans with exclusions mark as Do Not Bill Ins";
			// 
			// checkInsDefaultShowUCRonClaims
			// 
			this.checkInsDefaultShowUCRonClaims.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkInsDefaultShowUCRonClaims.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkInsDefaultShowUCRonClaims.Location = new System.Drawing.Point(8, 215);
			this.checkInsDefaultShowUCRonClaims.Name = "checkInsDefaultShowUCRonClaims";
			this.checkInsDefaultShowUCRonClaims.Size = new System.Drawing.Size(370, 20);
			this.checkInsDefaultShowUCRonClaims.TabIndex = 261;
			this.checkInsDefaultShowUCRonClaims.Text = "Insurance plans default to show standard provider fee on claims";
			this.checkInsDefaultShowUCRonClaims.Click += new System.EventHandler(this.checkInsDefaultShowUCRonClaims_Click);
			// 
			// groupBoxInsuranceGeneral
			// 
			this.groupBoxInsuranceGeneral.Controls.Add(this.checkOutOfNetworkBlankLikeZero);
			this.groupBoxInsuranceGeneral.Controls.Add(this.checkInsDefaultShowUCRonClaims);
			this.groupBoxInsuranceGeneral.Controls.Add(this.checkPatInitBillingTypeFromPriInsPlan);
			this.groupBoxInsuranceGeneral.Controls.Add(this.checkInsurancePlansShared);
			this.groupBoxInsuranceGeneral.Controls.Add(this.checkFixedBenefitBlankLikeZero);
			this.groupBoxInsuranceGeneral.Controls.Add(this.checkCoPayFeeScheduleBlankLikeZero);
			this.groupBoxInsuranceGeneral.Controls.Add(this.checkInsPPOsecWriteoffs);
			this.groupBoxInsuranceGeneral.Controls.Add(this.checkPPOpercentage);
			this.groupBoxInsuranceGeneral.Controls.Add(this.checkInsDefaultAssignmentOfBenefits);
			this.groupBoxInsuranceGeneral.Location = new System.Drawing.Point(8, 8);
			this.groupBoxInsuranceGeneral.Name = "groupBoxInsuranceGeneral";
			this.groupBoxInsuranceGeneral.Size = new System.Drawing.Size(478, 243);
			this.groupBoxInsuranceGeneral.TabIndex = 278;
			this.groupBoxInsuranceGeneral.Text = "Insurance General";
			// 
			// checkPatInitBillingTypeFromPriInsPlan
			// 
			this.checkPatInitBillingTypeFromPriInsPlan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPatInitBillingTypeFromPriInsPlan.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPatInitBillingTypeFromPriInsPlan.Location = new System.Drawing.Point(8, 191);
			this.checkPatInitBillingTypeFromPriInsPlan.Name = "checkPatInitBillingTypeFromPriInsPlan";
			this.checkPatInitBillingTypeFromPriInsPlan.Size = new System.Drawing.Size(361, 20);
			this.checkPatInitBillingTypeFromPriInsPlan.TabIndex = 298;
			this.checkPatInitBillingTypeFromPriInsPlan.Text = "Adding new primary insurance plan to patient sets billing type";
			// 
			// checkInsurancePlansShared
			// 
			this.checkInsurancePlansShared.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkInsurancePlansShared.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkInsurancePlansShared.Location = new System.Drawing.Point(8, 23);
			this.checkInsurancePlansShared.Name = "checkInsurancePlansShared";
			this.checkInsurancePlansShared.Size = new System.Drawing.Size(393, 20);
			this.checkInsurancePlansShared.TabIndex = 257;
			this.checkInsurancePlansShared.Text = "InsPlan option at bottom, \'Change Plan for all subscribers\', is default";
			// 
			// checkFixedBenefitBlankLikeZero
			// 
			this.checkFixedBenefitBlankLikeZero.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkFixedBenefitBlankLikeZero.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkFixedBenefitBlankLikeZero.Location = new System.Drawing.Point(8, 95);
			this.checkFixedBenefitBlankLikeZero.Name = "checkFixedBenefitBlankLikeZero";
			this.checkFixedBenefitBlankLikeZero.Size = new System.Drawing.Size(313, 20);
			this.checkFixedBenefitBlankLikeZero.TabIndex = 275;
			this.checkFixedBenefitBlankLikeZero.Text = "Fixed benefit fee schedules treat blank entries as zero";
			// 
			// checkCoPayFeeScheduleBlankLikeZero
			// 
			this.checkCoPayFeeScheduleBlankLikeZero.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkCoPayFeeScheduleBlankLikeZero.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkCoPayFeeScheduleBlankLikeZero.Location = new System.Drawing.Point(8, 71);
			this.checkCoPayFeeScheduleBlankLikeZero.Name = "checkCoPayFeeScheduleBlankLikeZero";
			this.checkCoPayFeeScheduleBlankLikeZero.Size = new System.Drawing.Size(278, 20);
			this.checkCoPayFeeScheduleBlankLikeZero.TabIndex = 260;
			this.checkCoPayFeeScheduleBlankLikeZero.Text = "Copay fee schedules treat blank entries as zero";
			// 
			// checkInsPPOsecWriteoffs
			// 
			this.checkInsPPOsecWriteoffs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkInsPPOsecWriteoffs.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkInsPPOsecWriteoffs.Location = new System.Drawing.Point(8, 167);
			this.checkInsPPOsecWriteoffs.Name = "checkInsPPOsecWriteoffs";
			this.checkInsPPOsecWriteoffs.Size = new System.Drawing.Size(268, 20);
			this.checkInsPPOsecWriteoffs.TabIndex = 270;
			this.checkInsPPOsecWriteoffs.Text = "Calculate secondary insurance PPO write-offs";
			// 
			// checkPPOpercentage
			// 
			this.checkPPOpercentage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPPOpercentage.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPPOpercentage.Location = new System.Drawing.Point(8, 47);
			this.checkPPOpercentage.Name = "checkPPOpercentage";
			this.checkPPOpercentage.Size = new System.Drawing.Size(335, 20);
			this.checkPPOpercentage.TabIndex = 258;
			this.checkPPOpercentage.Text = "Default new insurance plans to PPO Percentage plan type";
			// 
			// checkInsDefaultAssignmentOfBenefits
			// 
			this.checkInsDefaultAssignmentOfBenefits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkInsDefaultAssignmentOfBenefits.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkInsDefaultAssignmentOfBenefits.Location = new System.Drawing.Point(8, 143);
			this.checkInsDefaultAssignmentOfBenefits.Name = "checkInsDefaultAssignmentOfBenefits";
			this.checkInsDefaultAssignmentOfBenefits.Size = new System.Drawing.Size(290, 20);
			this.checkInsDefaultAssignmentOfBenefits.TabIndex = 267;
			this.checkInsDefaultAssignmentOfBenefits.Text = "Insurance plans default to assignment of benefits";
			this.checkInsDefaultAssignmentOfBenefits.Click += new System.EventHandler(this.checkInsDefaultAssignmentOfBenefits_Click);
			// 
			// checkOutOfNetworkBlankLikeZero
			// 
			this.checkOutOfNetworkBlankLikeZero.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkOutOfNetworkBlankLikeZero.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkOutOfNetworkBlankLikeZero.Location = new System.Drawing.Point(8, 119);
			this.checkOutOfNetworkBlankLikeZero.Name = "checkOutOfNetworkBlankLikeZero";
			this.checkOutOfNetworkBlankLikeZero.Size = new System.Drawing.Size(326, 20);
			this.checkOutOfNetworkBlankLikeZero.TabIndex = 299;
			this.checkOutOfNetworkBlankLikeZero.Text = "Out of network fee schedules treat blank entries as zero";
			// 
			// UserControlFamilyInsurance
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.groupBoxPerVisitProcedureCodes);
			this.Controls.Add(this.groupBoxCOB);
			this.Controls.Add(this.groupBoxClaims);
			this.Controls.Add(this.groupBoxOtherInsInfo);
			this.Controls.Add(this.groupBoxInsuranceGeneral);
			this.Name = "UserControlFamilyInsurance";
			this.Size = new System.Drawing.Size(494, 660);
			this.groupBoxPerVisitProcedureCodes.ResumeLayout(false);
			this.groupBoxPerVisitProcedureCodes.PerformLayout();
			this.groupBoxCOB.ResumeLayout(false);
			this.groupBoxClaims.ResumeLayout(false);
			this.groupBoxOtherInsInfo.ResumeLayout(false);
			this.groupBoxInsuranceGeneral.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private OpenPmsDental.UI.CheckBox checkInsurancePlansShared;
		private OpenPmsDental.UI.CheckBox checkCoPayFeeScheduleBlankLikeZero;
		private OpenPmsDental.UI.CheckBox checkInsDefaultShowUCRonClaims;
		private OpenPmsDental.UI.CheckBox checkInsPPOsecWriteoffs;
		private OpenPmsDental.UI.CheckBox checkInsDefaultAssignmentOfBenefits;
		private OpenPmsDental.UI.CheckBox checkClaimUseOverrideProcDescript;
		private OpenPmsDental.UI.CheckBox checkInsPlanExclusionsUseUCR;
		private OpenPmsDental.UI.CheckBox checkPPOpercentage;
		private OpenPmsDental.UI.CheckBox checkInsPlanExclusionsMarkDoNotBill;
		private OpenPmsDental.UI.CheckBox checkClaimTrackingRequireError;
		private OpenPmsDental.UI.CheckBox checkFixedBenefitBlankLikeZero;
		private UI.GroupBox groupBoxInsuranceGeneral;
		private OpenPmsDental.UI.CheckBox checkPatInitBillingTypeFromPriInsPlan;
		private UI.GroupBox groupBoxOtherInsInfo;
		private UI.GroupBox groupBoxClaims;
		private OpenPmsDental.UI.CheckBox checkEnableZeroWriteoffOnLimitations;
		private OpenPmsDental.UI.CheckBox checkEnableZeroWriteoffOnAnnualMax;
		private UI.GroupBox groupBoxCOB;
		private UI.ComboBox comboCobSendPaidByInsAt;
		private System.Windows.Forms.Label labelCobSendPaidByOtherInsAt;
		private UI.ComboBox comboCobRule;
		private System.Windows.Forms.Label labelCobRule;
		private OpenPmsDental.UI.GroupBox groupBoxPerVisitProcedureCodes;
		private System.Windows.Forms.Label labelPerVisitInsAmount;
		private System.Windows.Forms.Label labelPerVisitPatCopay;
		private System.Windows.Forms.TextBox textPerVisitInsAmountProcCode;
		private System.Windows.Forms.TextBox textPerVisitPatAmountProcCode;
		private UI.Button butPickPerVisitInsAmountProcCode;
		private UI.Button butPickPerVisitPatAmountProcCode;
		private UI.CheckBox checkOutOfNetworkBlankLikeZero;
	}
}
