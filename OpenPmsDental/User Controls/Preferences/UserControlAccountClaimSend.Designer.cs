
namespace OpenPmsDental {
	partial class UserControlAccountClaimSend {
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
			this.textInsWriteoffDescript = new System.Windows.Forms.TextBox();
			this.label17 = new System.Windows.Forms.Label();
			this.groupBoxClaimsMedical = new OpenPmsDental.UI.GroupBox();
			this.checkClaimMedTypeIsInstWhenInsPlanIsMedical = new OpenPmsDental.UI.CheckBox();
			this.checkEclaimsMedicalProvTreatmentAsOrdering = new OpenPmsDental.UI.CheckBox();
			this.groupBoxOD1 = new OpenPmsDental.UI.GroupBox();
			this.textClaimAttachPath = new System.Windows.Forms.TextBox();
			this.comboZeroDollarProcClaimBehavior = new OpenPmsDental.UI.ComboBox();
			this.checkEclaimsSubscIDUsesPatID = new OpenPmsDental.UI.CheckBox();
			this.checkPpoUseUcr = new OpenPmsDental.UI.CheckBox();
			this.checkClaimFormTreatDentSaysSigOnFile = new OpenPmsDental.UI.CheckBox();
			this.labelClaimAttachmentExportPath = new System.Windows.Forms.Label();
			this.checkEclaimsSeparateTreatProv = new OpenPmsDental.UI.CheckBox();
			this.labelCreatingClaimsZeroProcedures = new System.Windows.Forms.Label();
			this.checkPriClaimAllowSetToHoldUntilPriReceived = new OpenPmsDental.UI.CheckBox();
			this.groupBoxClaimIdPrefix = new OpenPmsDental.UI.GroupBox();
			this.butReplacements = new OpenPmsDental.UI.Button();
			this.textClaimIdPrefix = new System.Windows.Forms.TextBox();
			this.checkClaimsValidateACN = new OpenPmsDental.UI.CheckBox();
			this.checkCanadianPpoLabEst = new OpenPmsDental.UI.CheckBox();
			this.checkClaimEditRequireNoMissingData = new OpenPmsDental.UI.CheckBox();
			this.groupBoxClaimsMedical.SuspendLayout();
			this.groupBoxOD1.SuspendLayout();
			this.groupBoxClaimIdPrefix.SuspendLayout();
			this.SuspendLayout();
			// 
			// textInsWriteoffDescript
			// 
			this.textInsWriteoffDescript.AutoSize = false;
			this.textInsWriteoffDescript.Location = new System.Drawing.Point(256, 441);
			this.textInsWriteoffDescript.Name = "textInsWriteoffDescript";
			this.textInsWriteoffDescript.Size = new System.Drawing.Size(230, 23);
			this.textInsWriteoffDescript.TabIndex = 4;
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(24, 437);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(207, 32);
			this.label17.TabIndex = 284;
			this.label17.Text = "PPO write-off description (blank for \"Write-off\")";
			this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBoxClaimsMedical
			// 
			this.groupBoxClaimsMedical.Controls.Add(this.checkClaimMedTypeIsInstWhenInsPlanIsMedical);
			this.groupBoxClaimsMedical.Controls.Add(this.checkEclaimsMedicalProvTreatmentAsOrdering);
			this.groupBoxClaimsMedical.Location = new System.Drawing.Point(8, 354);
			this.groupBoxClaimsMedical.Name = "groupBoxClaimsMedical";
			this.groupBoxClaimsMedical.Size = new System.Drawing.Size(478, 75);
			this.groupBoxClaimsMedical.TabIndex = 3;
			this.groupBoxClaimsMedical.Text = "Claims Medical";
			// 
			// checkClaimMedTypeIsInstWhenInsPlanIsMedical
			// 
			this.checkClaimMedTypeIsInstWhenInsPlanIsMedical.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkClaimMedTypeIsInstWhenInsPlanIsMedical.Location = new System.Drawing.Point(8, 23);
			this.checkClaimMedTypeIsInstWhenInsPlanIsMedical.Name = "checkClaimMedTypeIsInstWhenInsPlanIsMedical";
			this.checkClaimMedTypeIsInstWhenInsPlanIsMedical.Size = new System.Drawing.Size(372, 20);
			this.checkClaimMedTypeIsInstWhenInsPlanIsMedical.TabIndex = 0;
			this.checkClaimMedTypeIsInstWhenInsPlanIsMedical.Text = "Set medical claims to institutional when using medical insurance";
			// 
			// checkEclaimsMedicalProvTreatmentAsOrdering
			// 
			this.checkEclaimsMedicalProvTreatmentAsOrdering.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEclaimsMedicalProvTreatmentAsOrdering.Location = new System.Drawing.Point(8, 47);
			this.checkEclaimsMedicalProvTreatmentAsOrdering.Name = "checkEclaimsMedicalProvTreatmentAsOrdering";
			this.checkEclaimsMedicalProvTreatmentAsOrdering.Size = new System.Drawing.Size(433, 20);
			this.checkEclaimsMedicalProvTreatmentAsOrdering.TabIndex = 1;
			this.checkEclaimsMedicalProvTreatmentAsOrdering.Text = "On medical e-claims, send treating provider as ordering provider by default";
			// 
			// groupBoxOD1
			// 
			this.groupBoxOD1.Controls.Add(this.checkClaimEditRequireNoMissingData);
			this.groupBoxOD1.Controls.Add(this.textClaimAttachPath);
			this.groupBoxOD1.Controls.Add(this.comboZeroDollarProcClaimBehavior);
			this.groupBoxOD1.Controls.Add(this.checkEclaimsSubscIDUsesPatID);
			this.groupBoxOD1.Controls.Add(this.checkPpoUseUcr);
			this.groupBoxOD1.Controls.Add(this.checkClaimFormTreatDentSaysSigOnFile);
			this.groupBoxOD1.Controls.Add(this.labelClaimAttachmentExportPath);
			this.groupBoxOD1.Controls.Add(this.checkEclaimsSeparateTreatProv);
			this.groupBoxOD1.Controls.Add(this.labelCreatingClaimsZeroProcedures);
			this.groupBoxOD1.Controls.Add(this.checkPriClaimAllowSetToHoldUntilPriReceived);
			this.groupBoxOD1.Controls.Add(this.groupBoxClaimIdPrefix);
			this.groupBoxOD1.Controls.Add(this.checkClaimsValidateACN);
			this.groupBoxOD1.Location = new System.Drawing.Point(8, 8);
			this.groupBoxOD1.Name = "groupBoxOD1";
			this.groupBoxOD1.Size = new System.Drawing.Size(478, 338);
			this.groupBoxOD1.TabIndex = 0;
			this.groupBoxOD1.Text = "Claims Send";
			// 
			// textClaimAttachPath
			// 
			this.textClaimAttachPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textClaimAttachPath.AutoSize = false;
			this.textClaimAttachPath.Location = new System.Drawing.Point(248, 47);
			this.textClaimAttachPath.Name = "textClaimAttachPath";
			this.textClaimAttachPath.Size = new System.Drawing.Size(222, 23);
			this.textClaimAttachPath.TabIndex = 1;
			// 
			// comboZeroDollarProcClaimBehavior
			// 
			this.comboZeroDollarProcClaimBehavior.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboZeroDollarProcClaimBehavior.Location = new System.Drawing.Point(248, 122);
			this.comboZeroDollarProcClaimBehavior.Name = "comboZeroDollarProcClaimBehavior";
			this.comboZeroDollarProcClaimBehavior.Size = new System.Drawing.Size(222, 23);
			this.comboZeroDollarProcClaimBehavior.TabIndex = 3;
			// 
			// checkEclaimsSubscIDUsesPatID
			// 
			this.checkEclaimsSubscIDUsesPatID.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEclaimsSubscIDUsesPatID.Location = new System.Drawing.Point(8, 98);
			this.checkEclaimsSubscIDUsesPatID.Name = "checkEclaimsSubscIDUsesPatID";
			this.checkEclaimsSubscIDUsesPatID.Size = new System.Drawing.Size(353, 20);
			this.checkEclaimsSubscIDUsesPatID.TabIndex = 293;
			this.checkEclaimsSubscIDUsesPatID.Text = "On e-claims, use Optional Patient ID instead of Subscriber ID";
			// 
			// checkPpoUseUcr
			// 
			this.checkPpoUseUcr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPpoUseUcr.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPpoUseUcr.Location = new System.Drawing.Point(8, 23);
			this.checkPpoUseUcr.Name = "checkPpoUseUcr";
			this.checkPpoUseUcr.Size = new System.Drawing.Size(372, 20);
			this.checkPpoUseUcr.TabIndex = 0;
			this.checkPpoUseUcr.Text = "Use standard provider fee for billed fee even if PPO fee is higher";
			// 
			// checkClaimFormTreatDentSaysSigOnFile
			// 
			this.checkClaimFormTreatDentSaysSigOnFile.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkClaimFormTreatDentSaysSigOnFile.Location = new System.Drawing.Point(8, 149);
			this.checkClaimFormTreatDentSaysSigOnFile.Name = "checkClaimFormTreatDentSaysSigOnFile";
			this.checkClaimFormTreatDentSaysSigOnFile.Size = new System.Drawing.Size(415, 20);
			this.checkClaimFormTreatDentSaysSigOnFile.TabIndex = 4;
			this.checkClaimFormTreatDentSaysSigOnFile.Text = "Claim form treating provider shows \'Signature On File\' rather than name";
			// 
			// labelClaimAttachmentExportPath
			// 
			this.labelClaimAttachmentExportPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelClaimAttachmentExportPath.Location = new System.Drawing.Point(24, 47);
			this.labelClaimAttachmentExportPath.Name = "labelClaimAttachmentExportPath";
			this.labelClaimAttachmentExportPath.Size = new System.Drawing.Size(218, 23);
			this.labelClaimAttachmentExportPath.TabIndex = 279;
			this.labelClaimAttachmentExportPath.Text = "Claim attachment export path";
			this.labelClaimAttachmentExportPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkEclaimsSeparateTreatProv
			// 
			this.checkEclaimsSeparateTreatProv.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEclaimsSeparateTreatProv.Location = new System.Drawing.Point(8, 74);
			this.checkEclaimsSeparateTreatProv.Name = "checkEclaimsSeparateTreatProv";
			this.checkEclaimsSeparateTreatProv.Size = new System.Drawing.Size(396, 20);
			this.checkEclaimsSeparateTreatProv.TabIndex = 2;
			this.checkEclaimsSeparateTreatProv.Text = "On e-claims, send treating provider info for each separate procedure";
			// 
			// labelCreatingClaimsZeroProcedures
			// 
			this.labelCreatingClaimsZeroProcedures.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelCreatingClaimsZeroProcedures.Location = new System.Drawing.Point(24, 122);
			this.labelCreatingClaimsZeroProcedures.Name = "labelCreatingClaimsZeroProcedures";
			this.labelCreatingClaimsZeroProcedures.Size = new System.Drawing.Size(218, 23);
			this.labelCreatingClaimsZeroProcedures.TabIndex = 292;
			this.labelCreatingClaimsZeroProcedures.Text = "Creating claims with $0 procedures";
			this.labelCreatingClaimsZeroProcedures.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkPriClaimAllowSetToHoldUntilPriReceived
			// 
			this.checkPriClaimAllowSetToHoldUntilPriReceived.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPriClaimAllowSetToHoldUntilPriReceived.Location = new System.Drawing.Point(8, 197);
			this.checkPriClaimAllowSetToHoldUntilPriReceived.Name = "checkPriClaimAllowSetToHoldUntilPriReceived";
			this.checkPriClaimAllowSetToHoldUntilPriReceived.Size = new System.Drawing.Size(331, 20);
			this.checkPriClaimAllowSetToHoldUntilPriReceived.TabIndex = 6;
			this.checkPriClaimAllowSetToHoldUntilPriReceived.Text = "Allow primary claim status to be \'Hold Until Pri Received\'";
			// 
			// groupBoxClaimIdPrefix
			// 
			this.groupBoxClaimIdPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxClaimIdPrefix.BackColor = System.Drawing.Color.White;
			this.groupBoxClaimIdPrefix.Controls.Add(this.butReplacements);
			this.groupBoxClaimIdPrefix.Controls.Add(this.textClaimIdPrefix);
			this.groupBoxClaimIdPrefix.Location = new System.Drawing.Point(8, 249);
			this.groupBoxClaimIdPrefix.Name = "groupBoxClaimIdPrefix";
			this.groupBoxClaimIdPrefix.Size = new System.Drawing.Size(462, 81);
			this.groupBoxClaimIdPrefix.TabIndex = 7;
			this.groupBoxClaimIdPrefix.Text = "Claim Identification Prefix";
			// 
			// butReplacements
			// 
			this.butReplacements.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butReplacements.Location = new System.Drawing.Point(363, 50);
			this.butReplacements.Name = "butReplacements";
			this.butReplacements.Size = new System.Drawing.Size(91, 23);
			this.butReplacements.TabIndex = 1;
			this.butReplacements.Text = "Replacements";
			this.butReplacements.UseVisualStyleBackColor = true;
			this.butReplacements.Click += new System.EventHandler(this.butReplacements_Click);
			// 
			// textClaimIdPrefix
			// 
			this.textClaimIdPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textClaimIdPrefix.AutoSize = false;
			this.textClaimIdPrefix.Location = new System.Drawing.Point(240, 23);
			this.textClaimIdPrefix.Name = "textClaimIdPrefix";
			this.textClaimIdPrefix.Size = new System.Drawing.Size(214, 23);
			this.textClaimIdPrefix.TabIndex = 0;
			this.textClaimIdPrefix.Validating += new System.ComponentModel.CancelEventHandler(this.textClaimIdPrefix_Validating);
			// 
			// checkClaimsValidateACN
			// 
			this.checkClaimsValidateACN.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkClaimsValidateACN.Location = new System.Drawing.Point(8, 173);
			this.checkClaimsValidateACN.Name = "checkClaimsValidateACN";
			this.checkClaimsValidateACN.Size = new System.Drawing.Size(347, 20);
			this.checkClaimsValidateACN.TabIndex = 5;
			this.checkClaimsValidateACN.Text = "Require ACN# in remarks on claims with ADDP group name";
			// 
			// checkCanadianPpoLabEst
			// 
			this.checkCanadianPpoLabEst.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkCanadianPpoLabEst.Location = new System.Drawing.Point(8, 473);
			this.checkCanadianPpoLabEst.Name = "checkCanadianPpoLabEst";
			this.checkCanadianPpoLabEst.Size = new System.Drawing.Size(299, 20);
			this.checkCanadianPpoLabEst.TabIndex = 5;
			this.checkCanadianPpoLabEst.Text = "Canadian PPO insurance plans create lab estimates";
			// 
			// checkClaimEditRequireNoMissingData
			// 
			this.checkClaimEditRequireNoMissingData.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkClaimEditRequireNoMissingData.Location = new System.Drawing.Point(8, 221);
			this.checkClaimEditRequireNoMissingData.Name = "checkClaimEditRequireNoMissingData";
			this.checkClaimEditRequireNoMissingData.Size = new System.Drawing.Size(235, 20);
			this.checkClaimEditRequireNoMissingData.TabIndex = 285;
			this.checkClaimEditRequireNoMissingData.Text = "Require claims to have no missing data";
			// 
			// UserControlAccountClaimSend
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.textInsWriteoffDescript);
			this.Controls.Add(this.groupBoxClaimsMedical);
			this.Controls.Add(this.groupBoxOD1);
			this.Controls.Add(this.label17);
			this.Controls.Add(this.checkCanadianPpoLabEst);
			this.Name = "UserControlAccountClaimSend";
			this.Size = new System.Drawing.Size(494, 660);
			this.groupBoxClaimsMedical.ResumeLayout(false);
			this.groupBoxOD1.ResumeLayout(false);
			this.groupBoxOD1.PerformLayout();
			this.groupBoxClaimIdPrefix.ResumeLayout(false);
			this.groupBoxClaimIdPrefix.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private OpenPmsDental.UI.CheckBox checkPriClaimAllowSetToHoldUntilPriReceived;
		private OpenPmsDental.UI.CheckBox checkCanadianPpoLabEst;
		private System.Windows.Forms.Label labelCreatingClaimsZeroProcedures;
		private UI.ComboBox comboZeroDollarProcClaimBehavior;
		private UI.GroupBox groupBoxClaimIdPrefix;
		private UI.Button butReplacements;
		private System.Windows.Forms.TextBox textClaimIdPrefix;
		private OpenPmsDental.UI.CheckBox checkClaimFormTreatDentSaysSigOnFile;
		private OpenPmsDental.UI.CheckBox checkClaimMedTypeIsInstWhenInsPlanIsMedical;
		private OpenPmsDental.UI.CheckBox checkEclaimsMedicalProvTreatmentAsOrdering;
		private System.Windows.Forms.Label labelClaimAttachmentExportPath;
		private System.Windows.Forms.TextBox textClaimAttachPath;
		private OpenPmsDental.UI.CheckBox checkClaimsValidateACN;
		private System.Windows.Forms.TextBox textInsWriteoffDescript;
		private System.Windows.Forms.Label label17;
		private UI.GroupBox groupBoxOD1;
		private OpenPmsDental.UI.CheckBox checkPpoUseUcr;
		private UI.GroupBox groupBoxClaimsMedical;
		private OpenPmsDental.UI.CheckBox checkEclaimsSeparateTreatProv;
		private UI.CheckBox checkEclaimsSubscIDUsesPatID;
		private UI.CheckBox checkClaimEditRequireNoMissingData;
	}
        
}

