
namespace OpenPmsDental {
	partial class UserControlAccountRecAndRepCharges {
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
			this.groupRecurringCharges = new OpenPmsDental.UI.GroupBox();
			this.checkRecurringChargesShowInactive = new OpenPmsDental.UI.CheckBox();
			this.checkRecurringChargesInactivateDeclinedCards = new OpenPmsDental.UI.CheckBox();
			this.checkRecurPatBal0 = new OpenPmsDental.UI.CheckBox();
			this.label56 = new System.Windows.Forms.Label();
			this.comboRecurringChargePayType = new OpenPmsDental.UI.ComboBox();
			this.labelRecurringChargesAutomatedTime = new System.Windows.Forms.Label();
			this.textRecurringChargesTime = new OpenPmsDental.ValidTime();
			this.checkRecurringChargesAutomated = new OpenPmsDental.UI.CheckBox();
			this.checkRecurringChargesUseTransDate = new OpenPmsDental.UI.CheckBox();
			this.checkRecurChargPriProv = new OpenPmsDental.UI.CheckBox();
			this.groupRepeatingCharges = new OpenPmsDental.UI.GroupBox();
			this.labelRepeatingChargesAutomatedTime = new System.Windows.Forms.Label();
			this.textRepeatingChargesAutomatedTime = new OpenPmsDental.ValidTime();
			this.checkRepeatingChargesRunAging = new OpenPmsDental.UI.CheckBox();
			this.checkRepeatingChargesAutomated = new OpenPmsDental.UI.CheckBox();
			this.labelRecurChargPriProvDetails = new System.Windows.Forms.Label();
			this.labelRecurringChargesTimeDetails = new System.Windows.Forms.Label();
			this.labelRecurringChargesInactivateDeclinedCardsDetails = new System.Windows.Forms.Label();
			this.labelRepeatingChargesAutomatedTimeDetails = new System.Windows.Forms.Label();
			this.labelRecurringChargesUseTransDateDetails = new System.Windows.Forms.Label();
			this.checkRecurringChargesFailEmailEnabled = new OpenPmsDental.UI.CheckBox();
			this.butRecurringChargesFailEmailTemplate = new OpenPmsDental.UI.Button();
			this.checkRepeatingChargesAllowBackdating = new OpenPmsDental.UI.CheckBox();
			this.groupRecurringCharges.SuspendLayout();
			this.groupRepeatingCharges.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupRecurringCharges
			// 
			this.groupRecurringCharges.BackColor = System.Drawing.Color.White;
			this.groupRecurringCharges.Controls.Add(this.comboRecurringChargePayType);
			this.groupRecurringCharges.Controls.Add(this.textRecurringChargesTime);
			this.groupRecurringCharges.Controls.Add(this.checkRecurringChargesShowInactive);
			this.groupRecurringCharges.Controls.Add(this.checkRecurringChargesInactivateDeclinedCards);
			this.groupRecurringCharges.Controls.Add(this.checkRecurPatBal0);
			this.groupRecurringCharges.Controls.Add(this.label56);
			this.groupRecurringCharges.Controls.Add(this.labelRecurringChargesAutomatedTime);
			this.groupRecurringCharges.Controls.Add(this.checkRecurringChargesAutomated);
			this.groupRecurringCharges.Controls.Add(this.checkRecurringChargesUseTransDate);
			this.groupRecurringCharges.Controls.Add(this.checkRecurChargPriProv);
			this.groupRecurringCharges.Controls.Add(this.checkRecurringChargesFailEmailEnabled);
			this.groupRecurringCharges.Controls.Add(this.butRecurringChargesFailEmailTemplate);
			this.groupRecurringCharges.Location = new System.Drawing.Point(8, 8);
			this.groupRecurringCharges.Name = "groupRecurringCharges";
			this.groupRecurringCharges.Size = new System.Drawing.Size(478, 337);
			this.groupRecurringCharges.TabIndex = 14;
			this.groupRecurringCharges.Text = "Recurring Charges (payments)";
			// 
			// checkRecurringChargesShowInactive
			// 
			this.checkRecurringChargesShowInactive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRecurringChargesShowInactive.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRecurringChargesShowInactive.Location = new System.Drawing.Point(8, 87);
			this.checkRecurringChargesShowInactive.Name = "checkRecurringChargesShowInactive";
			this.checkRecurringChargesShowInactive.Size = new System.Drawing.Size(301, 20);
			this.checkRecurringChargesShowInactive.TabIndex = 241;
			this.checkRecurringChargesShowInactive.Text = "Recurring charges show inactive charges by default";
			// 
			// checkRecurringChargesInactivateDeclinedCards
			// 
			this.checkRecurringChargesInactivateDeclinedCards.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRecurringChargesInactivateDeclinedCards.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRecurringChargesInactivateDeclinedCards.Location = new System.Drawing.Point(8, 247);
			this.checkRecurringChargesInactivateDeclinedCards.Name = "checkRecurringChargesInactivateDeclinedCards";
			this.checkRecurringChargesInactivateDeclinedCards.Size = new System.Drawing.Size(234, 20);
			this.checkRecurringChargesInactivateDeclinedCards.TabIndex = 254;
			this.checkRecurringChargesInactivateDeclinedCards.Text = "Automatically inactivate declined cards";
			// 
			// checkRecurPatBal0
			// 
			this.checkRecurPatBal0.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRecurPatBal0.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRecurPatBal0.Location = new System.Drawing.Point(8, 215);
			this.checkRecurPatBal0.Name = "checkRecurPatBal0";
			this.checkRecurPatBal0.Size = new System.Drawing.Size(370, 20);
			this.checkRecurPatBal0.TabIndex = 246;
			this.checkRecurPatBal0.Text = "Allow recurring charges to run even if no family balance present";
			// 
			// label56
			// 
			this.label56.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label56.Location = new System.Drawing.Point(175, 180);
			this.label56.Name = "label56";
			this.label56.Size = new System.Drawing.Size(96, 23);
			this.label56.TabIndex = 253;
			this.label56.Text = "Pay type for CC";
			this.label56.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboRecurringChargePayType
			// 
			this.comboRecurringChargePayType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboRecurringChargePayType.Location = new System.Drawing.Point(277, 180);
			this.comboRecurringChargePayType.Name = "comboRecurringChargePayType";
			this.comboRecurringChargePayType.Size = new System.Drawing.Size(164, 23);
			this.comboRecurringChargePayType.TabIndex = 252;
			// 
			// labelRecurringChargesAutomatedTime
			// 
			this.labelRecurringChargesAutomatedTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelRecurringChargesAutomatedTime.Location = new System.Drawing.Point(210, 151);
			this.labelRecurringChargesAutomatedTime.Name = "labelRecurringChargesAutomatedTime";
			this.labelRecurringChargesAutomatedTime.Size = new System.Drawing.Size(156, 23);
			this.labelRecurringChargesAutomatedTime.TabIndex = 243;
			this.labelRecurringChargesAutomatedTime.Text = "Recurring charges run time";
			this.labelRecurringChargesAutomatedTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textRecurringChargesTime
			// 
			this.textRecurringChargesTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textRecurringChargesTime.Enabled = false;
			this.textRecurringChargesTime.Location = new System.Drawing.Point(372, 151);
			this.textRecurringChargesTime.Name = "textRecurringChargesTime";
			this.textRecurringChargesTime.Size = new System.Drawing.Size(68, 20);
			this.textRecurringChargesTime.TabIndex = 242;
			this.textRecurringChargesTime.Leave += new System.EventHandler(this.PromptRecurringRepeatingChargesTimes);
			// 
			// checkRecurringChargesAutomated
			// 
			this.checkRecurringChargesAutomated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRecurringChargesAutomated.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRecurringChargesAutomated.Location = new System.Drawing.Point(8, 119);
			this.checkRecurringChargesAutomated.Name = "checkRecurringChargesAutomated";
			this.checkRecurringChargesAutomated.Size = new System.Drawing.Size(218, 20);
			this.checkRecurringChargesAutomated.TabIndex = 240;
			this.checkRecurringChargesAutomated.Text = "Recurring charges run automatically";
			this.checkRecurringChargesAutomated.Click += new System.EventHandler(this.checkRecurringChargesAutomated_Click);
			// 
			// checkRecurringChargesUseTransDate
			// 
			this.checkRecurringChargesUseTransDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRecurringChargesUseTransDate.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRecurringChargesUseTransDate.Location = new System.Drawing.Point(8, 55);
			this.checkRecurringChargesUseTransDate.Name = "checkRecurringChargesUseTransDate";
			this.checkRecurringChargesUseTransDate.Size = new System.Drawing.Size(234, 20);
			this.checkRecurringChargesUseTransDate.TabIndex = 239;
			this.checkRecurringChargesUseTransDate.Text = "Recurring charges use transaction date";
			// 
			// checkRecurChargPriProv
			// 
			this.checkRecurChargPriProv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRecurChargPriProv.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRecurChargPriProv.Location = new System.Drawing.Point(8, 23);
			this.checkRecurChargPriProv.Name = "checkRecurChargPriProv";
			this.checkRecurChargPriProv.Size = new System.Drawing.Size(322, 20);
			this.checkRecurChargPriProv.TabIndex = 238;
			this.checkRecurChargPriProv.Text = "Recurring charges use primary provider instead of FIFO";
			// 
			// checkRecurringChargesFailEmailEnabled
			// 
			this.checkRecurringChargesFailEmailEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRecurringChargesFailEmailEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRecurringChargesFailEmailEnabled.Location = new System.Drawing.Point(8, 279);
			this.checkRecurringChargesFailEmailEnabled.Name = "checkRecurringChargesFailEmailEnabled";
			this.checkRecurringChargesFailEmailEnabled.Size = new System.Drawing.Size(259, 20);
			this.checkRecurringChargesFailEmailEnabled.TabIndex = 255;
			this.checkRecurringChargesFailEmailEnabled.Text = "Email patients when a recurring charge fails";
			// 
			// butRecurringChargesFailEmailTemplate
			// 
			this.butRecurringChargesFailEmailTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butRecurringChargesFailEmailTemplate.Location = new System.Drawing.Point(340, 305);
			this.butRecurringChargesFailEmailTemplate.Name = "butRecurringChargesFailEmailTemplate";
			this.butRecurringChargesFailEmailTemplate.Size = new System.Drawing.Size(100, 24);
			this.butRecurringChargesFailEmailTemplate.TabIndex = 256;
			this.butRecurringChargesFailEmailTemplate.Text = "Edit Template";
			this.butRecurringChargesFailEmailTemplate.Click += new System.EventHandler(this.butRecurringChargesFailEmailTemplate_Click);
			// 
			// groupRepeatingCharges
			// 
			this.groupRepeatingCharges.BackColor = System.Drawing.Color.White;
			this.groupRepeatingCharges.Controls.Add(this.textRepeatingChargesAutomatedTime);
			this.groupRepeatingCharges.Controls.Add(this.labelRepeatingChargesAutomatedTime);
			this.groupRepeatingCharges.Controls.Add(this.checkRepeatingChargesRunAging);
			this.groupRepeatingCharges.Controls.Add(this.checkRepeatingChargesAutomated);
			this.groupRepeatingCharges.Controls.Add(this.checkRepeatingChargesAllowBackdating);
			this.groupRepeatingCharges.Location = new System.Drawing.Point(8, 357);
			this.groupRepeatingCharges.Name = "groupRepeatingCharges";
			this.groupRepeatingCharges.Size = new System.Drawing.Size(478, 144);
			this.groupRepeatingCharges.TabIndex = 15;
			this.groupRepeatingCharges.Text = "Repeating Charges (procedures)";
			// 
			// labelRepeatingChargesAutomatedTime
			// 
			this.labelRepeatingChargesAutomatedTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelRepeatingChargesAutomatedTime.Location = new System.Drawing.Point(210, 87);
			this.labelRepeatingChargesAutomatedTime.Name = "labelRepeatingChargesAutomatedTime";
			this.labelRepeatingChargesAutomatedTime.Size = new System.Drawing.Size(156, 23);
			this.labelRepeatingChargesAutomatedTime.TabIndex = 243;
			this.labelRepeatingChargesAutomatedTime.Text = "Repeating charges run time";
			this.labelRepeatingChargesAutomatedTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textRepeatingChargesAutomatedTime
			// 
			this.textRepeatingChargesAutomatedTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textRepeatingChargesAutomatedTime.Enabled = false;
			this.textRepeatingChargesAutomatedTime.Location = new System.Drawing.Point(372, 87);
			this.textRepeatingChargesAutomatedTime.Name = "textRepeatingChargesAutomatedTime";
			this.textRepeatingChargesAutomatedTime.Size = new System.Drawing.Size(68, 20);
			this.textRepeatingChargesAutomatedTime.TabIndex = 243;
			this.textRepeatingChargesAutomatedTime.Leave += new System.EventHandler(this.PromptRecurringRepeatingChargesTimes);
			// 
			// checkRepeatingChargesRunAging
			// 
			this.checkRepeatingChargesRunAging.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRepeatingChargesRunAging.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRepeatingChargesRunAging.Location = new System.Drawing.Point(8, 23);
			this.checkRepeatingChargesRunAging.Name = "checkRepeatingChargesRunAging";
			this.checkRepeatingChargesRunAging.Size = new System.Drawing.Size(197, 20);
			this.checkRepeatingChargesRunAging.TabIndex = 239;
			this.checkRepeatingChargesRunAging.Text = "Run aging after posting charges";
			// 
			// checkRepeatingChargesAutomated
			// 
			this.checkRepeatingChargesAutomated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRepeatingChargesAutomated.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRepeatingChargesAutomated.Location = new System.Drawing.Point(8, 55);
			this.checkRepeatingChargesAutomated.Name = "checkRepeatingChargesAutomated";
			this.checkRepeatingChargesAutomated.Size = new System.Drawing.Size(221, 20);
			this.checkRepeatingChargesAutomated.TabIndex = 238;
			this.checkRepeatingChargesAutomated.Text = "Repeating charges run automatically";
			this.checkRepeatingChargesAutomated.Click += new System.EventHandler(this.checkRepeatingChargesAutomated_Click);
			// 
			// checkRepeatingChargesAllowBackdating
			// 
			this.checkRepeatingChargesAllowBackdating.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkRepeatingChargesAllowBackdating.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRepeatingChargesAllowBackdating.Location = new System.Drawing.Point(8, 116);
			this.checkRepeatingChargesAllowBackdating.Name = "checkRepeatingChargesAllowBackdating";
			this.checkRepeatingChargesAllowBackdating.Size = new System.Drawing.Size(246, 20);
			this.checkRepeatingChargesAllowBackdating.TabIndex = 244;
			this.checkRepeatingChargesAllowBackdating.Text = "Allow repeating charges to be backdated";
			// 
			// labelRecurChargPriProvDetails
			// 
			this.labelRecurChargPriProvDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelRecurChargPriProvDetails.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelRecurChargPriProvDetails.Location = new System.Drawing.Point(510, 8);
			this.labelRecurChargPriProvDetails.Name = "labelRecurChargPriProvDetails";
			this.labelRecurChargPriProvDetails.Size = new System.Drawing.Size(0, 0);
			this.labelRecurChargPriProvDetails.TabIndex = 334;
			this.labelRecurChargPriProvDetails.Text = "only used when Paysplit allocations is set to \'Rigorous\'";
			this.labelRecurChargPriProvDetails.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelRecurringChargesTimeDetails
			// 
			this.labelRecurringChargesTimeDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelRecurringChargesTimeDetails.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelRecurringChargesTimeDetails.Location = new System.Drawing.Point(498, 8);
			this.labelRecurringChargesTimeDetails.Name = "labelRecurringChargesTimeDetails";
			this.labelRecurringChargesTimeDetails.Size = new System.Drawing.Size(0, 0);
			this.labelRecurringChargesTimeDetails.TabIndex = 335;
			this.labelRecurringChargesTimeDetails.Text = "this should be set to run after Repeating charges";
			this.labelRecurringChargesTimeDetails.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelRecurringChargesInactivateDeclinedCardsDetails
			// 
			this.labelRecurringChargesInactivateDeclinedCardsDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelRecurringChargesInactivateDeclinedCardsDetails.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelRecurringChargesInactivateDeclinedCardsDetails.Location = new System.Drawing.Point(504, 8);
			this.labelRecurringChargesInactivateDeclinedCardsDetails.Name = "labelRecurringChargesInactivateDeclinedCardsDetails";
			this.labelRecurringChargesInactivateDeclinedCardsDetails.Size = new System.Drawing.Size(0, 0);
			this.labelRecurringChargesInactivateDeclinedCardsDetails.TabIndex = 336;
			this.labelRecurringChargesInactivateDeclinedCardsDetails.Text = "recommend checked";
			this.labelRecurringChargesInactivateDeclinedCardsDetails.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelRepeatingChargesAutomatedTimeDetails
			// 
			this.labelRepeatingChargesAutomatedTimeDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelRepeatingChargesAutomatedTimeDetails.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelRepeatingChargesAutomatedTimeDetails.Location = new System.Drawing.Point(492, 357);
			this.labelRepeatingChargesAutomatedTimeDetails.Name = "labelRepeatingChargesAutomatedTimeDetails";
			this.labelRepeatingChargesAutomatedTimeDetails.Size = new System.Drawing.Size(0, 0);
			this.labelRepeatingChargesAutomatedTimeDetails.TabIndex = 337;
			this.labelRepeatingChargesAutomatedTimeDetails.Text = "this should be set to run before Recurring charges";
			this.labelRepeatingChargesAutomatedTimeDetails.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelRecurringChargesUseTransDateDetails
			// 
			this.labelRecurringChargesUseTransDateDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelRecurringChargesUseTransDateDetails.ForeColor = System.Drawing.Color.MidnightBlue;
			this.labelRecurringChargesUseTransDateDetails.Location = new System.Drawing.Point(492, 8);
			this.labelRecurringChargesUseTransDateDetails.Name = "labelRecurringChargesUseTransDateDetails";
			this.labelRecurringChargesUseTransDateDetails.Size = new System.Drawing.Size(0, 0);
			this.labelRecurringChargesUseTransDateDetails.TabIndex = 338;
			this.labelRecurringChargesUseTransDateDetails.Text = "otherwise, use the date the charge is scheduled to process";
			this.labelRecurringChargesUseTransDateDetails.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// UserControlAccountRecAndRepCharges
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.labelRecurringChargesUseTransDateDetails);
			this.Controls.Add(this.labelRepeatingChargesAutomatedTimeDetails);
			this.Controls.Add(this.labelRecurringChargesInactivateDeclinedCardsDetails);
			this.Controls.Add(this.labelRecurringChargesTimeDetails);
			this.Controls.Add(this.labelRecurChargPriProvDetails);
			this.Controls.Add(this.groupRepeatingCharges);
			this.Controls.Add(this.groupRecurringCharges);
			this.Name = "UserControlAccountRecAndRepCharges";
			this.Size = new System.Drawing.Size(494, 660);
			this.groupRecurringCharges.ResumeLayout(false);
			this.groupRecurringCharges.PerformLayout();
			this.groupRepeatingCharges.ResumeLayout(false);
			this.groupRepeatingCharges.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.GroupBox groupRecurringCharges;
		private OpenPmsDental.UI.CheckBox checkRecurringChargesShowInactive;
		private OpenPmsDental.UI.CheckBox checkRecurringChargesInactivateDeclinedCards;
		private OpenPmsDental.UI.CheckBox checkRecurPatBal0;
		private System.Windows.Forms.Label label56;
		private UI.ComboBox comboRecurringChargePayType;
		private System.Windows.Forms.Label labelRecurringChargesAutomatedTime;
		private ValidTime textRecurringChargesTime;
		private OpenPmsDental.UI.CheckBox checkRecurringChargesAutomated;
		private OpenPmsDental.UI.CheckBox checkRecurringChargesUseTransDate;
		private OpenPmsDental.UI.CheckBox checkRecurChargPriProv;
		private UI.GroupBox groupRepeatingCharges;
		private System.Windows.Forms.Label labelRepeatingChargesAutomatedTime;
		private ValidTime textRepeatingChargesAutomatedTime;
		private OpenPmsDental.UI.CheckBox checkRepeatingChargesRunAging;
		private OpenPmsDental.UI.CheckBox checkRepeatingChargesAutomated;
		private System.Windows.Forms.Label labelRecurChargPriProvDetails;
		private System.Windows.Forms.Label labelRecurringChargesTimeDetails;
		private System.Windows.Forms.Label labelRecurringChargesInactivateDeclinedCardsDetails;
		private System.Windows.Forms.Label labelRepeatingChargesAutomatedTimeDetails;
		private System.Windows.Forms.Label labelRecurringChargesUseTransDateDetails;
		private OpenPmsDental.UI.CheckBox checkRecurringChargesFailEmailEnabled;
		private OpenPmsDental.UI.Button butRecurringChargesFailEmailTemplate;
		private OpenPmsDental.UI.CheckBox checkRepeatingChargesAllowBackdating;
	}
}
