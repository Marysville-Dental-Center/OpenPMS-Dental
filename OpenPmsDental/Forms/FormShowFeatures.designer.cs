using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPmsDental {
	partial class FormShowFeatures {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.butSave = new OpenPmsDental.UI.Button();
			this.checkCapitation = new OpenPmsDental.UI.CheckBox();
			this.checkMedicaid = new OpenPmsDental.UI.CheckBox();
			this.checkClinical = new OpenPmsDental.UI.CheckBox();
			this.checkBasicModules = new OpenPmsDental.UI.CheckBox();
			this.checkPublicHealth = new OpenPmsDental.UI.CheckBox();
			this.checkEnableClinics = new OpenPmsDental.UI.CheckBox();
			this.checkDentalSchools = new OpenPmsDental.UI.CheckBox();
			this.checkRepeatCharges = new OpenPmsDental.UI.CheckBox();
			this.checkInsurance = new OpenPmsDental.UI.CheckBox();
			this.checkHospitals = new OpenPmsDental.UI.CheckBox();
			this.checkMedicalIns = new OpenPmsDental.UI.CheckBox();
			this.checkEhr = new OpenPmsDental.UI.CheckBox();
			this.checkSuperFam = new OpenPmsDental.UI.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.checkPatClone = new OpenPmsDental.UI.CheckBox();
			this.checkShowEnterprise = new OpenPmsDental.UI.CheckBox();
			this.checkShowReactivations = new OpenPmsDental.UI.CheckBox();
			this.checkEraShowControlId = new OpenPmsDental.UI.CheckBox();
			this.groupEnterprise = new OpenPmsDental.UI.GroupBox();
			this.groupFamilyModule = new OpenPmsDental.UI.GroupBox();
			this.groupGeneral = new OpenPmsDental.UI.GroupBox();
			this.groupInsurance = new OpenPmsDental.UI.GroupBox();
			this.groupSpecialUseOnly = new OpenPmsDental.UI.GroupBox();
			this.groupChargeTools = new OpenPmsDental.UI.GroupBox();
			this.groupBoxCharges = new OpenPmsDental.UI.GroupBox();
			this.radioBillingFinance = new System.Windows.Forms.RadioButton();
			this.radioLateCharges = new System.Windows.Forms.RadioButton();
			this.groupEnterprise.SuspendLayout();
			this.groupFamilyModule.SuspendLayout();
			this.groupGeneral.SuspendLayout();
			this.groupInsurance.SuspendLayout();
			this.groupSpecialUseOnly.SuspendLayout();
			this.groupChargeTools.SuspendLayout();
			this.groupBoxCharges.SuspendLayout();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(435, 412);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 19;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// checkCapitation
			// 
			this.checkCapitation.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkCapitation.Location = new System.Drawing.Point(8, 23);
			this.checkCapitation.Name = "checkCapitation";
			this.checkCapitation.Size = new System.Drawing.Size(79, 20);
			this.checkCapitation.TabIndex = 9;
			this.checkCapitation.Text = "Capitation";
			// 
			// checkMedicaid
			// 
			this.checkMedicaid.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkMedicaid.Location = new System.Drawing.Point(8, 47);
			this.checkMedicaid.Name = "checkMedicaid";
			this.checkMedicaid.Size = new System.Drawing.Size(73, 20);
			this.checkMedicaid.TabIndex = 10;
			this.checkMedicaid.Text = "Medicaid";
			// 
			// checkClinical
			// 
			this.checkClinical.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkClinical.Location = new System.Drawing.Point(8, 47);
			this.checkClinical.Name = "checkClinical";
			this.checkClinical.Size = new System.Drawing.Size(209, 20);
			this.checkClinical.TabIndex = 14;
			this.checkClinical.Text = "Clinical (computers in operatories)";
			// 
			// checkBasicModules
			// 
			this.checkBasicModules.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkBasicModules.Location = new System.Drawing.Point(8, 23);
			this.checkBasicModules.Name = "checkBasicModules";
			this.checkBasicModules.Size = new System.Drawing.Size(129, 20);
			this.checkBasicModules.TabIndex = 13;
			this.checkBasicModules.Text = "Basic Modules Only";
			// 
			// checkPublicHealth
			// 
			this.checkPublicHealth.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPublicHealth.Location = new System.Drawing.Point(8, 143);
			this.checkPublicHealth.Name = "checkPublicHealth";
			this.checkPublicHealth.Size = new System.Drawing.Size(94, 20);
			this.checkPublicHealth.TabIndex = 18;
			this.checkPublicHealth.Text = "Public Health";
			this.checkPublicHealth.Click += new System.EventHandler(this.checkPublicHealth_Click);
			// 
			// checkEnableClinics
			// 
			this.checkEnableClinics.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEnableClinics.Location = new System.Drawing.Point(8, 23);
			this.checkEnableClinics.Name = "checkEnableClinics";
			this.checkEnableClinics.Size = new System.Drawing.Size(198, 20);
			this.checkEnableClinics.TabIndex = 3;
			this.checkEnableClinics.Text = "Clinics (multiple office locations)";
			this.checkEnableClinics.Click += new System.EventHandler(this.checkEnableClinics_Click);
			// 
			// checkDentalSchools
			// 
			this.checkDentalSchools.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkDentalSchools.Location = new System.Drawing.Point(8, 71);
			this.checkDentalSchools.Name = "checkDentalSchools";
			this.checkDentalSchools.Size = new System.Drawing.Size(103, 20);
			this.checkDentalSchools.TabIndex = 15;
			this.checkDentalSchools.Text = "Dental Schools";
			this.checkDentalSchools.Click += new System.EventHandler(this.checkRestart_Click);
			// 
			// checkRepeatCharges
			// 
			this.checkRepeatCharges.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRepeatCharges.Location = new System.Drawing.Point(8, 91);
			this.checkRepeatCharges.Name = "checkRepeatCharges";
			this.checkRepeatCharges.Size = new System.Drawing.Size(125, 20);
			this.checkRepeatCharges.TabIndex = 2;
			this.checkRepeatCharges.Text = "Repeating Charges";
			this.checkRepeatCharges.Click += new System.EventHandler(this.checkRepeatCharges_Click);
			// 
			// checkInsurance
			// 
			this.checkInsurance.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkInsurance.Location = new System.Drawing.Point(8, 95);
			this.checkInsurance.Name = "checkInsurance";
			this.checkInsurance.Size = new System.Drawing.Size(98, 20);
			this.checkInsurance.TabIndex = 12;
			this.checkInsurance.Text = "Use Insurance";
			this.checkInsurance.Click += new System.EventHandler(this.checkRestart_Click);
			// 
			// checkHospitals
			// 
			this.checkHospitals.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkHospitals.Location = new System.Drawing.Point(8, 119);
			this.checkHospitals.Name = "checkHospitals";
			this.checkHospitals.Size = new System.Drawing.Size(73, 20);
			this.checkHospitals.TabIndex = 17;
			this.checkHospitals.Text = "Hospitals";
			// 
			// checkMedicalIns
			// 
			this.checkMedicalIns.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkMedicalIns.Location = new System.Drawing.Point(8, 71);
			this.checkMedicalIns.Name = "checkMedicalIns";
			this.checkMedicalIns.Size = new System.Drawing.Size(121, 20);
			this.checkMedicalIns.TabIndex = 11;
			this.checkMedicalIns.Text = "Medical Insurance";
			// 
			// checkEhr
			// 
			this.checkEhr.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEhr.Location = new System.Drawing.Point(8, 95);
			this.checkEhr.Name = "checkEhr";
			this.checkEhr.Size = new System.Drawing.Size(45, 20);
			this.checkEhr.TabIndex = 16;
			this.checkEhr.Text = "EHR";
			this.checkEhr.Click += new System.EventHandler(this.checkEhr_Click);
			// 
			// checkSuperFam
			// 
			this.checkSuperFam.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkSuperFam.Location = new System.Drawing.Point(8, 47);
			this.checkSuperFam.Name = "checkSuperFam";
			this.checkSuperFam.Size = new System.Drawing.Size(101, 20);
			this.checkSuperFam.TabIndex = 7;
			this.checkSuperFam.Text = "Super Families";
			this.checkSuperFam.Click += new System.EventHandler(this.checkRestart_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(501, 18);
			this.label1.TabIndex = 30;
			this.label1.Text = "The following settings will affect all computers.";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// checkPatClone
			// 
			this.checkPatClone.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPatClone.Location = new System.Drawing.Point(8, 23);
			this.checkPatClone.Name = "checkPatClone";
			this.checkPatClone.Size = new System.Drawing.Size(95, 20);
			this.checkPatClone.TabIndex = 6;
			this.checkPatClone.Text = "Patient Clone";
			this.checkPatClone.Click += new System.EventHandler(this.checkRestart_Click);
			// 
			// checkShowEnterprise
			// 
			this.checkShowEnterprise.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkShowEnterprise.Location = new System.Drawing.Point(8, 47);
			this.checkShowEnterprise.Name = "checkShowEnterprise";
			this.checkShowEnterprise.Size = new System.Drawing.Size(112, 20);
			this.checkShowEnterprise.TabIndex = 4;
			this.checkShowEnterprise.Text = "Enterprise Setup";
			// 
			// checkShowReactivations
			// 
			this.checkShowReactivations.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkShowReactivations.Location = new System.Drawing.Point(8, 23);
			this.checkShowReactivations.Name = "checkShowReactivations";
			this.checkShowReactivations.Size = new System.Drawing.Size(111, 20);
			this.checkShowReactivations.TabIndex = 8;
			this.checkShowReactivations.Text = "Reactivation List";
			this.checkShowReactivations.Click += new System.EventHandler(this.checkRestart_Click);
			// 
			// checkEraShowControlId
			// 
			this.checkEraShowControlId.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkEraShowControlId.Location = new System.Drawing.Point(8, 71);
			this.checkEraShowControlId.Name = "checkEraShowControlId";
			this.checkEraShowControlId.Size = new System.Drawing.Size(134, 20);
			this.checkEraShowControlId.TabIndex = 5;
			this.checkEraShowControlId.Text = "ERA Control ID Filter";
			// 
			// groupEnterprise
			// 
			this.groupEnterprise.Controls.Add(this.checkEnableClinics);
			this.groupEnterprise.Controls.Add(this.checkEraShowControlId);
			this.groupEnterprise.Controls.Add(this.checkShowEnterprise);
			this.groupEnterprise.Location = new System.Drawing.Point(8, 159);
			this.groupEnterprise.Name = "groupEnterprise";
			this.groupEnterprise.Size = new System.Drawing.Size(245, 99);
			this.groupEnterprise.TabIndex = 24;
			this.groupEnterprise.TabStop = false;
			this.groupEnterprise.Text = "Enterprise";
			// 
			// groupFamilyModule
			// 
			this.groupFamilyModule.Controls.Add(this.checkSuperFam);
			this.groupFamilyModule.Controls.Add(this.checkPatClone);
			this.groupFamilyModule.Location = new System.Drawing.Point(8, 266);
			this.groupFamilyModule.Name = "groupFamilyModule";
			this.groupFamilyModule.Size = new System.Drawing.Size(245, 75);
			this.groupFamilyModule.TabIndex = 25;
			this.groupFamilyModule.TabStop = false;
			this.groupFamilyModule.Text = "Family Module";
			// 
			// groupGeneral
			// 
			this.groupGeneral.Controls.Add(this.checkShowReactivations);
			this.groupGeneral.Location = new System.Drawing.Point(8, 349);
			this.groupGeneral.Name = "groupGeneral";
			this.groupGeneral.Size = new System.Drawing.Size(245, 51);
			this.groupGeneral.TabIndex = 26;
			this.groupGeneral.TabStop = false;
			this.groupGeneral.Text = "General";
			// 
			// groupInsurance
			// 
			this.groupInsurance.Controls.Add(this.checkCapitation);
			this.groupInsurance.Controls.Add(this.checkMedicaid);
			this.groupInsurance.Controls.Add(this.checkMedicalIns);
			this.groupInsurance.Controls.Add(this.checkInsurance);
			this.groupInsurance.Location = new System.Drawing.Point(261, 28);
			this.groupInsurance.Name = "groupInsurance";
			this.groupInsurance.Size = new System.Drawing.Size(249, 123);
			this.groupInsurance.TabIndex = 27;
			this.groupInsurance.TabStop = false;
			this.groupInsurance.Text = "Insurance";
			// 
			// groupSpecialUseOnly
			// 
			this.groupSpecialUseOnly.Controls.Add(this.checkBasicModules);
			this.groupSpecialUseOnly.Controls.Add(this.checkClinical);
			this.groupSpecialUseOnly.Controls.Add(this.checkDentalSchools);
			this.groupSpecialUseOnly.Controls.Add(this.checkHospitals);
			this.groupSpecialUseOnly.Controls.Add(this.checkPublicHealth);
			this.groupSpecialUseOnly.Controls.Add(this.checkEhr);
			this.groupSpecialUseOnly.Location = new System.Drawing.Point(261, 159);
			this.groupSpecialUseOnly.Name = "groupSpecialUseOnly";
			this.groupSpecialUseOnly.Size = new System.Drawing.Size(249, 171);
			this.groupSpecialUseOnly.TabIndex = 28;
			this.groupSpecialUseOnly.TabStop = false;
			this.groupSpecialUseOnly.Text = "Special Use Only";
			// 
			// groupChargeTools
			// 
			this.groupChargeTools.Controls.Add(this.groupBoxCharges);
			this.groupChargeTools.Controls.Add(this.checkRepeatCharges);
			this.groupChargeTools.Location = new System.Drawing.Point(8, 28);
			this.groupChargeTools.Name = "groupChargeTools";
			this.groupChargeTools.Size = new System.Drawing.Size(245, 123);
			this.groupChargeTools.TabIndex = 23;
			this.groupChargeTools.TabStop = false;
			this.groupChargeTools.Text = "Charge Tools";
			// 
			// groupBoxCharges
			// 
			this.groupBoxCharges.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
			this.groupBoxCharges.Controls.Add(this.radioBillingFinance);
			this.groupBoxCharges.Controls.Add(this.radioLateCharges);
			this.groupBoxCharges.Location = new System.Drawing.Point(8, 23);
			this.groupBoxCharges.Name = "groupBoxCharges";
			this.groupBoxCharges.Size = new System.Drawing.Size(229, 60);
			this.groupBoxCharges.TabIndex = 29;
			this.groupBoxCharges.TabStop = false;
			// 
			// radioBillingFinance
			// 
			this.radioBillingFinance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioBillingFinance.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.radioBillingFinance.Location = new System.Drawing.Point(8, 8);
			this.radioBillingFinance.Name = "radioBillingFinance";
			this.radioBillingFinance.Size = new System.Drawing.Size(150, 20);
			this.radioBillingFinance.TabIndex = 0;
			this.radioBillingFinance.TabStop = true;
			this.radioBillingFinance.Text = "Billing/Finance Charges";
			this.radioBillingFinance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.radioBillingFinance.UseVisualStyleBackColor = true;
			// 
			// radioLateCharges
			// 
			this.radioLateCharges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.radioLateCharges.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.radioLateCharges.Location = new System.Drawing.Point(8, 32);
			this.radioLateCharges.Name = "radioLateCharges";
			this.radioLateCharges.Size = new System.Drawing.Size(93, 20);
			this.radioLateCharges.TabIndex = 1;
			this.radioLateCharges.TabStop = true;
			this.radioLateCharges.Text = "Late Charges";
			this.radioLateCharges.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.radioLateCharges.UseVisualStyleBackColor = true;
			// 
			// FormShowFeatures
			// 
			this.ClientSize = new System.Drawing.Size(518, 444);
			this.Controls.Add(this.groupChargeTools);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.groupSpecialUseOnly);
			this.Controls.Add(this.groupInsurance);
			this.Controls.Add(this.groupGeneral);
			this.Controls.Add(this.groupFamilyModule);
			this.Controls.Add(this.groupEnterprise);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormShowFeatures";
			this.ShowInTaskbar = false;
			this.Text = "Show Features";
			this.Load += new System.EventHandler(this.FormShowFeatures_Load);
			this.groupEnterprise.ResumeLayout(false);
			this.groupFamilyModule.ResumeLayout(false);
			this.groupGeneral.ResumeLayout(false);
			this.groupInsurance.ResumeLayout(false);
			this.groupSpecialUseOnly.ResumeLayout(false);
			this.groupChargeTools.ResumeLayout(false);
			this.groupBoxCharges.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private OpenPmsDental.UI.CheckBox checkClinical;
		private OpenPmsDental.UI.CheckBox checkBasicModules;
		private OpenPmsDental.UI.CheckBox checkPublicHealth;
		private OpenPmsDental.UI.CheckBox checkEnableClinics;
		private OpenPmsDental.UI.CheckBox checkDentalSchools;
		private OpenPmsDental.UI.CheckBox checkRepeatCharges;
		private OpenPmsDental.UI.CheckBox checkInsurance;
		private OpenPmsDental.UI.CheckBox checkHospitals;
		private OpenPmsDental.UI.CheckBox checkMedicalIns;
		private OpenPmsDental.UI.CheckBox checkEhr;
		private OpenPmsDental.UI.CheckBox checkSuperFam;
		private System.Windows.Forms.Label label1;
		private OpenPmsDental.UI.CheckBox checkPatClone;
		///<summary>Keeps track the clinic preference state when the window was loaded.</summary>
		private OpenPmsDental.UI.CheckBox checkShowEnterprise;
		private OpenPmsDental.UI.CheckBox checkMedicaid;
		private OpenPmsDental.UI.CheckBox checkCapitation;
		private OpenPmsDental.UI.CheckBox checkShowReactivations;
		private OpenPmsDental.UI.CheckBox checkEraShowControlId;
		private OpenPmsDental.UI.GroupBox groupEnterprise;
		private OpenPmsDental.UI.GroupBox groupFamilyModule;
		private OpenPmsDental.UI.GroupBox groupGeneral;
		private OpenPmsDental.UI.GroupBox groupInsurance;
		private OpenPmsDental.UI.GroupBox groupSpecialUseOnly;
		private OpenPmsDental.UI.GroupBox groupChargeTools;
		private System.Windows.Forms.RadioButton radioLateCharges;
		private System.Windows.Forms.RadioButton radioBillingFinance;
		private OpenPmsDental.UI.GroupBox groupBoxCharges;
	}
}
