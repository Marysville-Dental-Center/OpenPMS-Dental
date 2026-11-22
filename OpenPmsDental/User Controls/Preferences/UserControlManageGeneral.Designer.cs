
namespace OpenPmsDental {
	partial class UserControlManageGeneral {
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
			this.checkScheduleProvEmpSelectAll = new OpenPmsDental.UI.CheckBox();
			this.checkAccountingInvoiceAttachmentsSaveInDatabase = new OpenPmsDental.UI.CheckBox();
			this.groupBoxTimeCards = new OpenPmsDental.UI.GroupBox();
			this.comboTimeCardOvertimeFirstDayOfWeek = new OpenPmsDental.UI.ComboBox();
			this.checkTimeCardUseLocal = new OpenPmsDental.UI.CheckBox();
			this.label16 = new System.Windows.Forms.Label();
			this.checkTimeCardADP = new OpenPmsDental.UI.CheckBox();
			this.checkClockEventAllowBreak = new OpenPmsDental.UI.CheckBox();
			this.groupBoxDeposits = new OpenPmsDental.UI.GroupBox();
			this.comboDepositSoftware = new OpenPmsDental.UI.ComboBox();
			this.checkShowAutoDeposit = new OpenPmsDental.UI.CheckBox();
			this.labelDepositSoftware = new System.Windows.Forms.Label();
			this.groupBoxPrescriptions = new OpenPmsDental.UI.GroupBox();
			this.checkRxHideProvsWithoutDEA = new OpenPmsDental.UI.CheckBox();
			this.groupBoxClaims = new OpenPmsDental.UI.GroupBox();
			this.textClaimsReceivedDays = new OpenPmsDental.ValidNum();
			this.checkClaimsSendWindowValidateOnLoad = new OpenPmsDental.UI.CheckBox();
			this.labelClaimsReceivedDays = new System.Windows.Forms.Label();
			this.checkClaimPaymentBatchOnly = new OpenPmsDental.UI.CheckBox();
			this.groupBoxERA = new OpenPmsDental.UI.GroupBox();
			this.comboEraWriteoff = new OpenPmsDental.UI.ComboBox();
			this.textEraNoAutoProcessCarcCodes = new System.Windows.Forms.TextBox();
			this.comboEraDefaultPaymentType = new OpenPmsDental.UI.ComboBox();
			this.comboFwtPaymentType = new OpenPmsDental.UI.ComboBox();
			this.comboAchPaymentType = new OpenPmsDental.UI.ComboBox();
			this.comboEraCheckPaymentType = new OpenPmsDental.UI.ComboBox();
			this.comboEraAutomation = new OpenPmsDental.UI.ComboBox();
			this.labelWriteOffs = new System.Windows.Forms.Label();
			this.labelEraNoAutoProcessCarcCodes = new System.Windows.Forms.Label();
			this.labelERADefault = new System.Windows.Forms.Label();
			this.labelFWT = new System.Windows.Forms.Label();
			this.labelACH = new System.Windows.Forms.Label();
			this.labelCHK = new System.Windows.Forms.Label();
			this.checkEraOneClaimPerPage = new OpenPmsDental.UI.CheckBox();
			this.checkIncludeEraWOPercCoPay = new OpenPmsDental.UI.CheckBox();
			this.labelEraAutomation = new System.Windows.Forms.Label();
			this.checkEraAllowTotalPayment = new OpenPmsDental.UI.CheckBox();
			this.checkEraAutoSplitCarcDenials = new OpenPmsDental.UI.CheckBox();
			this.groupBoxTimeCards.SuspendLayout();
			this.groupBoxDeposits.SuspendLayout();
			this.groupBoxPrescriptions.SuspendLayout();
			this.groupBoxClaims.SuspendLayout();
			this.groupBoxERA.SuspendLayout();
			this.SuspendLayout();
			// 
			// checkScheduleProvEmpSelectAll
			// 
			this.checkScheduleProvEmpSelectAll.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkScheduleProvEmpSelectAll.Location = new System.Drawing.Point(8, 735);
			this.checkScheduleProvEmpSelectAll.Name = "checkScheduleProvEmpSelectAll";
			this.checkScheduleProvEmpSelectAll.Size = new System.Drawing.Size(399, 20);
			this.checkScheduleProvEmpSelectAll.TabIndex = 201;
			this.checkScheduleProvEmpSelectAll.Text = "Automatically select all providers/employees when loading schedules";
			// 
			// checkAccountingInvoiceAttachmentsSaveInDatabase
			// 
			this.checkAccountingInvoiceAttachmentsSaveInDatabase.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAccountingInvoiceAttachmentsSaveInDatabase.Location = new System.Drawing.Point(8, 759);
			this.checkAccountingInvoiceAttachmentsSaveInDatabase.Name = "checkAccountingInvoiceAttachmentsSaveInDatabase";
			this.checkAccountingInvoiceAttachmentsSaveInDatabase.Size = new System.Drawing.Size(290, 20);
			this.checkAccountingInvoiceAttachmentsSaveInDatabase.TabIndex = 308;
			this.checkAccountingInvoiceAttachmentsSaveInDatabase.Text = "Save accounting invoice attachments in database";
			// 
			// groupBoxTimeCards
			// 
			this.groupBoxTimeCards.Controls.Add(this.comboTimeCardOvertimeFirstDayOfWeek);
			this.groupBoxTimeCards.Controls.Add(this.checkTimeCardUseLocal);
			this.groupBoxTimeCards.Controls.Add(this.label16);
			this.groupBoxTimeCards.Controls.Add(this.checkTimeCardADP);
			this.groupBoxTimeCards.Controls.Add(this.checkClockEventAllowBreak);
			this.groupBoxTimeCards.Location = new System.Drawing.Point(8, 601);
			this.groupBoxTimeCards.Name = "groupBoxTimeCards";
			this.groupBoxTimeCards.Size = new System.Drawing.Size(478, 126);
			this.groupBoxTimeCards.TabIndex = 307;
			this.groupBoxTimeCards.Text = "Time Cards";
			// 
			// comboTimeCardOvertimeFirstDayOfWeek
			// 
			this.comboTimeCardOvertimeFirstDayOfWeek.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboTimeCardOvertimeFirstDayOfWeek.Location = new System.Drawing.Point(296, 23);
			this.comboTimeCardOvertimeFirstDayOfWeek.Name = "comboTimeCardOvertimeFirstDayOfWeek";
			this.comboTimeCardOvertimeFirstDayOfWeek.Size = new System.Drawing.Size(174, 23);
			this.comboTimeCardOvertimeFirstDayOfWeek.TabIndex = 198;
			// 
			// checkTimeCardUseLocal
			// 
			this.checkTimeCardUseLocal.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkTimeCardUseLocal.Location = new System.Drawing.Point(8, 98);
			this.checkTimeCardUseLocal.Name = "checkTimeCardUseLocal";
			this.checkTimeCardUseLocal.Size = new System.Drawing.Size(162, 20);
			this.checkTimeCardUseLocal.TabIndex = 296;
			this.checkTimeCardUseLocal.Text = "Time Cards use local time";
			// 
			// label16
			// 
			this.label16.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label16.Location = new System.Drawing.Point(24, 23);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(266, 23);
			this.label16.TabIndex = 202;
			this.label16.Text = "Time Card first day of week for overtime";
			this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkTimeCardADP
			// 
			this.checkTimeCardADP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkTimeCardADP.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkTimeCardADP.Location = new System.Drawing.Point(8, 50);
			this.checkTimeCardADP.Name = "checkTimeCardADP";
			this.checkTimeCardADP.Size = new System.Drawing.Size(223, 20);
			this.checkTimeCardADP.TabIndex = 199;
			this.checkTimeCardADP.Text = "ADP export includes employee name";
			// 
			// checkClockEventAllowBreak
			// 
			this.checkClockEventAllowBreak.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkClockEventAllowBreak.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkClockEventAllowBreak.Location = new System.Drawing.Point(8, 74);
			this.checkClockEventAllowBreak.Name = "checkClockEventAllowBreak";
			this.checkClockEventAllowBreak.Size = new System.Drawing.Size(177, 20);
			this.checkClockEventAllowBreak.TabIndex = 295;
			this.checkClockEventAllowBreak.Text = "Allow paid 30 minute breaks";
			// 
			// groupBoxDeposits
			// 
			this.groupBoxDeposits.Controls.Add(this.comboDepositSoftware);
			this.groupBoxDeposits.Controls.Add(this.checkShowAutoDeposit);
			this.groupBoxDeposits.Controls.Add(this.labelDepositSoftware);
			this.groupBoxDeposits.Location = new System.Drawing.Point(8, 515);
			this.groupBoxDeposits.Name = "groupBoxDeposits";
			this.groupBoxDeposits.Size = new System.Drawing.Size(478, 78);
			this.groupBoxDeposits.TabIndex = 306;
			this.groupBoxDeposits.Text = "Deposits";
			// 
			// comboDepositSoftware
			// 
			this.comboDepositSoftware.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboDepositSoftware.Location = new System.Drawing.Point(296, 47);
			this.comboDepositSoftware.Name = "comboDepositSoftware";
			this.comboDepositSoftware.Size = new System.Drawing.Size(174, 23);
			this.comboDepositSoftware.TabIndex = 298;
			this.comboDepositSoftware.SelectionChangeCommitted += new System.EventHandler(this.comboDepositSoftware_SelectionChangeCommitted);
			// 
			// checkShowAutoDeposit
			// 
			this.checkShowAutoDeposit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkShowAutoDeposit.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkShowAutoDeposit.Location = new System.Drawing.Point(8, 23);
			this.checkShowAutoDeposit.Name = "checkShowAutoDeposit";
			this.checkShowAutoDeposit.Size = new System.Drawing.Size(240, 20);
			this.checkShowAutoDeposit.TabIndex = 294;
			this.checkShowAutoDeposit.Text = "Insurance payments create auto deposit";
			// 
			// labelDepositSoftware
			// 
			this.labelDepositSoftware.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelDepositSoftware.Location = new System.Drawing.Point(24, 47);
			this.labelDepositSoftware.Name = "labelDepositSoftware";
			this.labelDepositSoftware.Size = new System.Drawing.Size(266, 23);
			this.labelDepositSoftware.TabIndex = 300;
			this.labelDepositSoftware.Text = "Deposit Software";
			this.labelDepositSoftware.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBoxPrescriptions
			// 
			this.groupBoxPrescriptions.Controls.Add(this.checkRxHideProvsWithoutDEA);
			this.groupBoxPrescriptions.Location = new System.Drawing.Point(8, 456);
			this.groupBoxPrescriptions.Name = "groupBoxPrescriptions";
			this.groupBoxPrescriptions.Size = new System.Drawing.Size(478, 51);
			this.groupBoxPrescriptions.TabIndex = 305;
			this.groupBoxPrescriptions.Text = "Prescriptions";
			// 
			// checkRxHideProvsWithoutDEA
			// 
			this.checkRxHideProvsWithoutDEA.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRxHideProvsWithoutDEA.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRxHideProvsWithoutDEA.Location = new System.Drawing.Point(8, 23);
			this.checkRxHideProvsWithoutDEA.Name = "checkRxHideProvsWithoutDEA";
			this.checkRxHideProvsWithoutDEA.Size = new System.Drawing.Size(460, 20);
			this.checkRxHideProvsWithoutDEA.TabIndex = 302;
			this.checkRxHideProvsWithoutDEA.Text = "Hide providers without DEA number from making (non-electronic)  prescriptions";
			// 
			// groupBoxClaims
			// 
			this.groupBoxClaims.Controls.Add(this.textClaimsReceivedDays);
			this.groupBoxClaims.Controls.Add(this.checkClaimsSendWindowValidateOnLoad);
			this.groupBoxClaims.Controls.Add(this.labelClaimsReceivedDays);
			this.groupBoxClaims.Controls.Add(this.checkClaimPaymentBatchOnly);
			this.groupBoxClaims.Location = new System.Drawing.Point(8, 337);
			this.groupBoxClaims.Name = "groupBoxClaims";
			this.groupBoxClaims.Size = new System.Drawing.Size(478, 111);
			this.groupBoxClaims.TabIndex = 304;
			this.groupBoxClaims.Text = "Claims";
			// 
			// textClaimsReceivedDays
			// 
			this.textClaimsReceivedDays.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textClaimsReceivedDays.Location = new System.Drawing.Point(296, 51);
			this.textClaimsReceivedDays.MaxVal = 50000;
			this.textClaimsReceivedDays.MinVal = 1;
			this.textClaimsReceivedDays.Name = "textClaimsReceivedDays";
			this.textClaimsReceivedDays.ShowZero = false;
			this.textClaimsReceivedDays.Size = new System.Drawing.Size(60, 20);
			this.textClaimsReceivedDays.TabIndex = 290;
			this.textClaimsReceivedDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// checkClaimsSendWindowValidateOnLoad
			// 
			this.checkClaimsSendWindowValidateOnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkClaimsSendWindowValidateOnLoad.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkClaimsSendWindowValidateOnLoad.Location = new System.Drawing.Point(8, 23);
			this.checkClaimsSendWindowValidateOnLoad.Name = "checkClaimsSendWindowValidateOnLoad";
			this.checkClaimsSendWindowValidateOnLoad.Size = new System.Drawing.Size(341, 20);
			this.checkClaimsSendWindowValidateOnLoad.TabIndex = 200;
			this.checkClaimsSendWindowValidateOnLoad.Text = "Claims Send window validate on load (can cause slowness)";
			// 
			// labelClaimsReceivedDays
			// 
			this.labelClaimsReceivedDays.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelClaimsReceivedDays.Location = new System.Drawing.Point(24, 47);
			this.labelClaimsReceivedDays.MaximumSize = new System.Drawing.Size(1000, 300);
			this.labelClaimsReceivedDays.Name = "labelClaimsReceivedDays";
			this.labelClaimsReceivedDays.Size = new System.Drawing.Size(266, 32);
			this.labelClaimsReceivedDays.TabIndex = 299;
			this.labelClaimsReceivedDays.Text = "Show $0 claim payments in Batch Ins for number days";
			this.labelClaimsReceivedDays.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkClaimPaymentBatchOnly
			// 
			this.checkClaimPaymentBatchOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkClaimPaymentBatchOnly.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkClaimPaymentBatchOnly.Location = new System.Drawing.Point(8, 83);
			this.checkClaimPaymentBatchOnly.Name = "checkClaimPaymentBatchOnly";
			this.checkClaimPaymentBatchOnly.Size = new System.Drawing.Size(326, 20);
			this.checkClaimPaymentBatchOnly.TabIndex = 291;
			this.checkClaimPaymentBatchOnly.Text = "Finalize claim payments in Batch Insurance window only";
			// 
			// groupBoxERA
			// 
			this.groupBoxERA.Controls.Add(this.comboEraWriteoff);
			this.groupBoxERA.Controls.Add(this.textEraNoAutoProcessCarcCodes);
			this.groupBoxERA.Controls.Add(this.comboEraDefaultPaymentType);
			this.groupBoxERA.Controls.Add(this.comboFwtPaymentType);
			this.groupBoxERA.Controls.Add(this.comboAchPaymentType);
			this.groupBoxERA.Controls.Add(this.comboEraCheckPaymentType);
			this.groupBoxERA.Controls.Add(this.comboEraAutomation);
			this.groupBoxERA.Controls.Add(this.labelWriteOffs);
			this.groupBoxERA.Controls.Add(this.labelEraNoAutoProcessCarcCodes);
			this.groupBoxERA.Controls.Add(this.labelERADefault);
			this.groupBoxERA.Controls.Add(this.labelFWT);
			this.groupBoxERA.Controls.Add(this.labelACH);
			this.groupBoxERA.Controls.Add(this.labelCHK);
			this.groupBoxERA.Controls.Add(this.checkEraOneClaimPerPage);
			this.groupBoxERA.Controls.Add(this.checkIncludeEraWOPercCoPay);
			this.groupBoxERA.Controls.Add(this.labelEraAutomation);
			this.groupBoxERA.Controls.Add(this.checkEraAllowTotalPayment);
			this.groupBoxERA.Controls.Add(this.checkEraAutoSplitCarcDenials);
			this.groupBoxERA.Location = new System.Drawing.Point(8, 8);
			this.groupBoxERA.Name = "groupBoxERA";
			this.groupBoxERA.Size = new System.Drawing.Size(478, 321);
			this.groupBoxERA.TabIndex = 303;
			this.groupBoxERA.Text = "ERA";
			// 
			// comboEraWriteoff
			// 
			this.comboEraWriteoff.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboEraWriteoff.Location = new System.Drawing.Point(296, 99);
			this.comboEraWriteoff.Name = "comboEraWriteoff";
			this.comboEraWriteoff.Size = new System.Drawing.Size(174, 23);
			this.comboEraWriteoff.TabIndex = 301;
			// 
			// textEraNoAutoProcessCarcCodes
			// 
			this.textEraNoAutoProcessCarcCodes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textEraNoAutoProcessCarcCodes.Location = new System.Drawing.Point(296, 266);
			this.textEraNoAutoProcessCarcCodes.Name = "textEraNoAutoProcessCarcCodes";
			this.textEraNoAutoProcessCarcCodes.Size = new System.Drawing.Size(174, 23);
			this.textEraNoAutoProcessCarcCodes.TabIndex = 312;
			// 
			// comboEraDefaultPaymentType
			// 
			this.comboEraDefaultPaymentType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboEraDefaultPaymentType.Location = new System.Drawing.Point(296, 239);
			this.comboEraDefaultPaymentType.Name = "comboEraDefaultPaymentType";
			this.comboEraDefaultPaymentType.Size = new System.Drawing.Size(174, 23);
			this.comboEraDefaultPaymentType.TabIndex = 308;
			// 
			// comboFwtPaymentType
			// 
			this.comboFwtPaymentType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboFwtPaymentType.Location = new System.Drawing.Point(296, 212);
			this.comboFwtPaymentType.Name = "comboFwtPaymentType";
			this.comboFwtPaymentType.Size = new System.Drawing.Size(174, 23);
			this.comboFwtPaymentType.TabIndex = 306;
			// 
			// comboAchPaymentType
			// 
			this.comboAchPaymentType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboAchPaymentType.Location = new System.Drawing.Point(296, 185);
			this.comboAchPaymentType.Name = "comboAchPaymentType";
			this.comboAchPaymentType.Size = new System.Drawing.Size(174, 23);
			this.comboAchPaymentType.TabIndex = 304;
			// 
			// comboEraCheckPaymentType
			// 
			this.comboEraCheckPaymentType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboEraCheckPaymentType.Location = new System.Drawing.Point(296, 158);
			this.comboEraCheckPaymentType.Name = "comboEraCheckPaymentType";
			this.comboEraCheckPaymentType.Size = new System.Drawing.Size(174, 23);
			this.comboEraCheckPaymentType.TabIndex = 302;
			// 
			// comboEraAutomation
			// 
			this.comboEraAutomation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboEraAutomation.Location = new System.Drawing.Point(296, 131);
			this.comboEraAutomation.Name = "comboEraAutomation";
			this.comboEraAutomation.Size = new System.Drawing.Size(174, 23);
			this.comboEraAutomation.TabIndex = 297;
			// 
			// labelWriteOffs
			// 
			this.labelWriteOffs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelWriteOffs.Location = new System.Drawing.Point(24, 95);
			this.labelWriteOffs.Name = "labelWriteOffs";
			this.labelWriteOffs.Size = new System.Drawing.Size(266, 32);
			this.labelWriteOffs.TabIndex = 312;
			this.labelWriteOffs.Text = "Auto-processing posts write-offs on non-primary claims";
			this.labelWriteOffs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelEraNoAutoProcessCarcCodes
			// 
			this.labelEraNoAutoProcessCarcCodes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelEraNoAutoProcessCarcCodes.Location = new System.Drawing.Point(24, 266);
			this.labelEraNoAutoProcessCarcCodes.Name = "labelEraNoAutoProcessCarcCodes";
			this.labelEraNoAutoProcessCarcCodes.Size = new System.Drawing.Size(266, 23);
			this.labelEraNoAutoProcessCarcCodes.TabIndex = 311;
			this.labelEraNoAutoProcessCarcCodes.Text = "Don\'t auto-process claims w/ these CARCs";
			this.labelEraNoAutoProcessCarcCodes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelERADefault
			// 
			this.labelERADefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelERADefault.Location = new System.Drawing.Point(24, 239);
			this.labelERADefault.Name = "labelERADefault";
			this.labelERADefault.Size = new System.Drawing.Size(266, 23);
			this.labelERADefault.TabIndex = 309;
			this.labelERADefault.Text = "Default payment type";
			this.labelERADefault.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelFWT
			// 
			this.labelFWT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelFWT.Location = new System.Drawing.Point(24, 212);
			this.labelFWT.Name = "labelFWT";
			this.labelFWT.Size = new System.Drawing.Size(266, 23);
			this.labelFWT.TabIndex = 307;
			this.labelFWT.Text = "FWT payment type";
			this.labelFWT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelACH
			// 
			this.labelACH.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelACH.Location = new System.Drawing.Point(24, 185);
			this.labelACH.Name = "labelACH";
			this.labelACH.Size = new System.Drawing.Size(266, 23);
			this.labelACH.TabIndex = 305;
			this.labelACH.Text = "ACH payment type";
			this.labelACH.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelCHK
			// 
			this.labelCHK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelCHK.Location = new System.Drawing.Point(24, 158);
			this.labelCHK.Name = "labelCHK";
			this.labelCHK.Size = new System.Drawing.Size(266, 23);
			this.labelCHK.TabIndex = 303;
			this.labelCHK.Text = "CHK payment type";
			this.labelCHK.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkEraOneClaimPerPage
			// 
			this.checkEraOneClaimPerPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkEraOneClaimPerPage.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEraOneClaimPerPage.Location = new System.Drawing.Point(8, 23);
			this.checkEraOneClaimPerPage.Name = "checkEraOneClaimPerPage";
			this.checkEraOneClaimPerPage.Size = new System.Drawing.Size(187, 20);
			this.checkEraOneClaimPerPage.TabIndex = 292;
			this.checkEraOneClaimPerPage.Text = "ERA prints one page per claim";
			// 
			// checkIncludeEraWOPercCoPay
			// 
			this.checkIncludeEraWOPercCoPay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkIncludeEraWOPercCoPay.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIncludeEraWOPercCoPay.Location = new System.Drawing.Point(8, 47);
			this.checkIncludeEraWOPercCoPay.Name = "checkIncludeEraWOPercCoPay";
			this.checkIncludeEraWOPercCoPay.Size = new System.Drawing.Size(408, 20);
			this.checkIncludeEraWOPercCoPay.TabIndex = 293;
			this.checkIncludeEraWOPercCoPay.Text = "ERA posts write-offs for Category Percentage and Medicaid/Flat Copay";
			// 
			// labelEraAutomation
			// 
			this.labelEraAutomation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelEraAutomation.Location = new System.Drawing.Point(24, 131);
			this.labelEraAutomation.Name = "labelEraAutomation";
			this.labelEraAutomation.Size = new System.Drawing.Size(266, 23);
			this.labelEraAutomation.TabIndex = 301;
			this.labelEraAutomation.Text = "ERA Automation";
			this.labelEraAutomation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkEraAllowTotalPayment
			// 
			this.checkEraAllowTotalPayment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkEraAllowTotalPayment.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEraAllowTotalPayment.Location = new System.Drawing.Point(8, 71);
			this.checkEraAllowTotalPayment.Name = "checkEraAllowTotalPayment";
			this.checkEraAllowTotalPayment.Size = new System.Drawing.Size(161, 20);
			this.checkEraAllowTotalPayment.TabIndex = 296;
			this.checkEraAllowTotalPayment.Text = "ERA allow total payments";
			// 
			// checkEraAutoSplitCarcDenials
			//
			this.checkEraAutoSplitCarcDenials.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkEraAutoSplitCarcDenials.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEraAutoSplitCarcDenials.Location = new System.Drawing.Point(8, 293);
			this.checkEraAutoSplitCarcDenials.Name = "checkEraAutoSplitCarcDenials";
			this.checkEraAutoSplitCarcDenials.Size = new System.Drawing.Size(307, 20);
			this.checkEraAutoSplitCarcDenials.TabIndex = 313;
			this.checkEraAutoSplitCarcDenials.Text = "Auto split ERAs with CARC denial codes listed above";
			//
			// UserControlManageGeneral
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.checkAccountingInvoiceAttachmentsSaveInDatabase);
			this.Controls.Add(this.groupBoxTimeCards);
			this.Controls.Add(this.groupBoxDeposits);
			this.Controls.Add(this.groupBoxPrescriptions);
			this.Controls.Add(this.groupBoxClaims);
			this.Controls.Add(this.groupBoxERA);
			this.Controls.Add(this.checkScheduleProvEmpSelectAll);
			this.Name = "UserControlManageGeneral";
			this.Size = new System.Drawing.Size(494, 787);
			this.groupBoxTimeCards.ResumeLayout(false);
			this.groupBoxDeposits.ResumeLayout(false);
			this.groupBoxPrescriptions.ResumeLayout(false);
			this.groupBoxClaims.ResumeLayout(false);
			this.groupBoxClaims.PerformLayout();
			this.groupBoxERA.ResumeLayout(false);
			this.groupBoxERA.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private OpenPmsDental.UI.CheckBox checkScheduleProvEmpSelectAll;
		private OpenPmsDental.UI.CheckBox checkClaimsSendWindowValidateOnLoad;
		private OpenPmsDental.UI.CheckBox checkTimeCardADP;
		private UI.ComboBox comboTimeCardOvertimeFirstDayOfWeek;
		private System.Windows.Forms.Label label16;
		private OpenPmsDental.UI.CheckBox checkRxHideProvsWithoutDEA;
		private UI.ComboBox comboEraAutomation;
		private System.Windows.Forms.Label labelEraAutomation;
		private UI.ComboBox comboDepositSoftware;
		private System.Windows.Forms.Label labelDepositSoftware;
		private OpenPmsDental.UI.CheckBox checkEraAllowTotalPayment;
		private OpenPmsDental.UI.CheckBox checkIncludeEraWOPercCoPay;
		private OpenPmsDental.UI.CheckBox checkClockEventAllowBreak;
		private ValidNum textClaimsReceivedDays;
		private OpenPmsDental.UI.CheckBox checkShowAutoDeposit;
		private OpenPmsDental.UI.CheckBox checkEraOneClaimPerPage;
		private OpenPmsDental.UI.CheckBox checkClaimPaymentBatchOnly;
		private System.Windows.Forms.Label labelClaimsReceivedDays;
		private UI.GroupBox groupBoxERA;
		private UI.GroupBox groupBoxClaims;
		private UI.GroupBox groupBoxPrescriptions;
		private UI.GroupBox groupBoxDeposits;
		private UI.GroupBox groupBoxTimeCards;
		private OpenPmsDental.UI.CheckBox checkAccountingInvoiceAttachmentsSaveInDatabase;
		private System.Windows.Forms.Label labelERADefault;
		private UI.ComboBox comboEraDefaultPaymentType;
		private System.Windows.Forms.Label labelFWT;
		private UI.ComboBox comboFwtPaymentType;
		private System.Windows.Forms.Label labelACH;
		private UI.ComboBox comboAchPaymentType;
		private System.Windows.Forms.Label labelCHK;
		private UI.ComboBox comboEraCheckPaymentType;
		private UI.ComboBox comboEraWriteoff;
		private System.Windows.Forms.Label labelWriteOffs;
		private System.Windows.Forms.Label labelEraNoAutoProcessCarcCodes;
		private System.Windows.Forms.TextBox textEraNoAutoProcessCarcCodes;
		private UI.CheckBox checkTimeCardUseLocal;
		private OpenPmsDental.UI.CheckBox checkEraAutoSplitCarcDenials;
	}
}
