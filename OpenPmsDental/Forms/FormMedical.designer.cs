using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormMedical {
		private System.ComponentModel.IContainer components = null;

		///<summary></summary>
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
			this.components = new System.ComponentModel.Container();
			this.butSave = new OpenPmsDental.UI.Button();
			this.butAdd = new OpenPmsDental.UI.Button();
			this.butAddDisease = new OpenPmsDental.UI.Button();
			this.gridMeds = new OpenPmsDental.UI.GridOD();
			this.gridDiseases = new OpenPmsDental.UI.GridOD();
			this.checkDiscontinued = new OpenPmsDental.UI.CheckBox();
			this.gridAllergies = new OpenPmsDental.UI.GridOD();
			this.butAddAllergy = new OpenPmsDental.UI.Button();
			this.checkShowInactiveAllergies = new OpenPmsDental.UI.CheckBox();
			this.butPrint = new OpenPmsDental.UI.Button();
			this.checkShowInactiveProblems = new OpenPmsDental.UI.CheckBox();
			this.gridFamilyHealth = new OpenPmsDental.UI.GridOD();
			this.butAddFamilyHistory = new OpenPmsDental.UI.Button();
			this.tabControlFormMedical = new OpenPmsDental.UI.TabControl();
			this.tabMedical = new OpenPmsDental.UI.TabPage();
			this.label4 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.textMedical = new OpenPmsDental.ODtextBox();
			this.butPrintMedical = new OpenPmsDental.UI.Button();
			this.textService = new OpenPmsDental.ODtextBox();
			this.textMedUrgNote = new OpenPmsDental.ODtextBox();
			this.checkPremed = new OpenPmsDental.UI.CheckBox();
			this.groupMedsDocumented = new OpenPmsDental.UI.GroupBox();
			this.radioMedsDocumentedNo = new System.Windows.Forms.RadioButton();
			this.radioMedsDocumentedYes = new System.Windows.Forms.RadioButton();
			this.label6 = new System.Windows.Forms.Label();
			this.textMedicalComp = new OpenPmsDental.ODtextBox();
			this.tabProblems = new OpenPmsDental.UI.TabPage();
			this.tabMedications = new OpenPmsDental.UI.TabPage();
			this.tabAllergies = new OpenPmsDental.UI.TabPage();
			this.tabFamHealthHist = new OpenPmsDental.UI.TabPage();
			this.tabVitalSigns = new OpenPmsDental.UI.TabPage();
			this.butGrowthChart = new OpenPmsDental.UI.Button();
			this.butAddVitalSign = new OpenPmsDental.UI.Button();
			this.gridVitalSigns = new OpenPmsDental.UI.GridOD();
			this.tabTobaccoUse = new OpenPmsDental.UI.TabPage();
			this.tabControl1 = new OpenPmsDental.UI.TabControl();
			this.tabPage1 = new OpenPmsDental.UI.TabPage();
			this.radioRecentStatuses = new System.Windows.Forms.RadioButton();
			this.radioUserStatuses = new System.Windows.Forms.RadioButton();
			this.label9 = new System.Windows.Forms.Label();
			this.comboTobaccoStatus = new System.Windows.Forms.ComboBox();
			this.gridAssessments = new OpenPmsDental.UI.GridOD();
			this.labelTobaccoStatus = new System.Windows.Forms.Label();
			this.radioAllStatuses = new System.Windows.Forms.RadioButton();
			this.butAddAssessment = new OpenPmsDental.UI.Button();
			this.comboAssessmentType = new System.Windows.Forms.ComboBox();
			this.textDateAssessed = new System.Windows.Forms.TextBox();
			this.radioNonUserStatuses = new System.Windows.Forms.RadioButton();
			this.label11 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.tabPage2 = new OpenPmsDental.UI.TabPage();
			this.radioRecentInterventions = new System.Windows.Forms.RadioButton();
			this.comboInterventionCode = new System.Windows.Forms.ComboBox();
			this.checkPatientDeclined = new OpenPmsDental.UI.CheckBox();
			this.label8 = new System.Windows.Forms.Label();
			this.radioAllInterventions = new System.Windows.Forms.RadioButton();
			this.textDateIntervention = new System.Windows.Forms.TextBox();
			this.gridInterventions = new OpenPmsDental.UI.GridOD();
			this.label5 = new System.Windows.Forms.Label();
			this.radioMedInterventions = new System.Windows.Forms.RadioButton();
			this.label7 = new System.Windows.Forms.Label();
			this.butAddIntervention = new OpenPmsDental.UI.Button();
			this.radioCounselInterventions = new System.Windows.Forms.RadioButton();
			this.label12 = new System.Windows.Forms.Label();
			this.comboSmokeStatus = new System.Windows.Forms.ComboBox();
			this.label13 = new System.Windows.Forms.Label();
			this.tabControlFormMedical.SuspendLayout();
			this.tabMedical.SuspendLayout();
			this.groupMedsDocumented.SuspendLayout();
			this.tabProblems.SuspendLayout();
			this.tabMedications.SuspendLayout();
			this.tabAllergies.SuspendLayout();
			this.tabFamHealthHist.SuspendLayout();
			this.tabVitalSigns.SuspendLayout();
			this.tabTobaccoUse.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(820, 484);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 25);
			this.butSave.TabIndex = 1;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// butAdd
			// 
			this.butAdd.AdjustImageLocation = new System.Drawing.Point(0, 1);
			this.butAdd.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(8, 8);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(123, 23);
			this.butAdd.TabIndex = 51;
			this.butAdd.Text = "&Add Medication";
			this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
			// 
			// butAddDisease
			// 
			this.butAddDisease.AdjustImageLocation = new System.Drawing.Point(0, 1);
			this.butAddDisease.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAddDisease.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAddDisease.Location = new System.Drawing.Point(8, 8);
			this.butAddDisease.Name = "butAddDisease";
			this.butAddDisease.Size = new System.Drawing.Size(99, 23);
			this.butAddDisease.TabIndex = 58;
			this.butAddDisease.Text = "Add Problem";
			this.butAddDisease.Click += new System.EventHandler(this.butAddProblem_Click);
			// 
			// gridMeds
			// 
			this.gridMeds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMeds.Location = new System.Drawing.Point(8, 36);
			this.gridMeds.Name = "gridMeds";
			this.gridMeds.Size = new System.Drawing.Size(775, 389);
			this.gridMeds.TabIndex = 59;
			this.gridMeds.Title = "Medications";
			this.gridMeds.TranslationName = "TableMedications";
			this.gridMeds.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMeds_CellDoubleClick);
			this.gridMeds.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMeds_CellClick);
			// 
			// gridDiseases
			// 
			this.gridDiseases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridDiseases.Location = new System.Drawing.Point(8, 35);
			this.gridDiseases.Name = "gridDiseases";
			this.gridDiseases.Size = new System.Drawing.Size(775, 389);
			this.gridDiseases.TabIndex = 60;
			this.gridDiseases.Title = "Problems";
			this.gridDiseases.TranslationName = "TableDiseases";
			this.gridDiseases.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridDiseases_CellDoubleClick);
			this.gridDiseases.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridDiseases_CellClick);
			// 
			// checkDiscontinued
			// 
			this.checkDiscontinued.Location = new System.Drawing.Point(137, 10);
			this.checkDiscontinued.Name = "checkDiscontinued";
			this.checkDiscontinued.Size = new System.Drawing.Size(197, 20);
			this.checkDiscontinued.TabIndex = 61;
			this.checkDiscontinued.Tag = "";
			this.checkDiscontinued.Text = "Show Discontinued Medications";
			this.checkDiscontinued.CheckedChanged += new System.EventHandler(this.checkDiscontinued_CheckChanged);
			// 
			// gridAllergies
			// 
			this.gridAllergies.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridAllergies.Location = new System.Drawing.Point(8, 35);
			this.gridAllergies.Name = "gridAllergies";
			this.gridAllergies.Size = new System.Drawing.Size(775, 389);
			this.gridAllergies.TabIndex = 63;
			this.gridAllergies.Title = "Allergies";
			this.gridAllergies.TranslationName = "TableDiseases";
			this.gridAllergies.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridAllergies_CellDoubleClick);
			this.gridAllergies.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridAllergies_CellClick);
			// 
			// butAddAllergy
			// 
			this.butAddAllergy.AdjustImageLocation = new System.Drawing.Point(0, 1);
			this.butAddAllergy.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAddAllergy.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAddAllergy.Location = new System.Drawing.Point(8, 8);
			this.butAddAllergy.Name = "butAddAllergy";
			this.butAddAllergy.Size = new System.Drawing.Size(98, 23);
			this.butAddAllergy.TabIndex = 64;
			this.butAddAllergy.Text = "Add Allergy";
			this.butAddAllergy.Click += new System.EventHandler(this.butAddAllergy_Click);
			// 
			// checkShowInactiveAllergies
			// 
			this.checkShowInactiveAllergies.Location = new System.Drawing.Point(155, 9);
			this.checkShowInactiveAllergies.Name = "checkShowInactiveAllergies";
			this.checkShowInactiveAllergies.Size = new System.Drawing.Size(148, 20);
			this.checkShowInactiveAllergies.TabIndex = 65;
			this.checkShowInactiveAllergies.Tag = "";
			this.checkShowInactiveAllergies.Text = "Show Inactive Allergies";
			this.checkShowInactiveAllergies.CheckedChanged += new System.EventHandler(this.checkShowInactiveAllergies_CheckedChanged);
			// 
			// butPrint
			// 
			this.butPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butPrint.Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons.Print,16,16);
			this.butPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPrint.Location = new System.Drawing.Point(665, 8);
			this.butPrint.Name = "butPrint";
			this.butPrint.Size = new System.Drawing.Size(116, 24);
			this.butPrint.TabIndex = 66;
			this.butPrint.Text = "Print Medications";
			this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
			// 
			// checkShowInactiveProblems
			// 
			this.checkShowInactiveProblems.Location = new System.Drawing.Point(155, 9);
			this.checkShowInactiveProblems.Name = "checkShowInactiveProblems";
			this.checkShowInactiveProblems.Size = new System.Drawing.Size(152, 20);
			this.checkShowInactiveProblems.TabIndex = 65;
			this.checkShowInactiveProblems.Tag = "";
			this.checkShowInactiveProblems.Text = "Show Inactive Problems";
			this.checkShowInactiveProblems.CheckedChanged += new System.EventHandler(this.checkShowInactiveProblems_CheckedChanged);
			// 
			// gridFamilyHealth
			// 
			this.gridFamilyHealth.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridFamilyHealth.Location = new System.Drawing.Point(8, 35);
			this.gridFamilyHealth.Name = "gridFamilyHealth";
			this.gridFamilyHealth.Size = new System.Drawing.Size(775, 389);
			this.gridFamilyHealth.TabIndex = 69;
			this.gridFamilyHealth.Title = "Family Health History";
			this.gridFamilyHealth.TranslationName = "TableDiseases";
			this.gridFamilyHealth.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridFamilyHealth_CellDoubleClick);
			// 
			// butAddFamilyHistory
			// 
			this.butAddFamilyHistory.AdjustImageLocation = new System.Drawing.Point(0, 1);
			this.butAddFamilyHistory.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAddFamilyHistory.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAddFamilyHistory.Location = new System.Drawing.Point(8, 8);
			this.butAddFamilyHistory.Name = "butAddFamilyHistory";
			this.butAddFamilyHistory.Size = new System.Drawing.Size(137, 23);
			this.butAddFamilyHistory.TabIndex = 70;
			this.butAddFamilyHistory.Text = "Add Family History";
			this.butAddFamilyHistory.Click += new System.EventHandler(this.butAddFamilyHistory_Click);
			// 
			// tabControlFormMedical
			// 
			this.tabControlFormMedical.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControlFormMedical.Controls.Add(this.tabMedical);
			this.tabControlFormMedical.Controls.Add(this.tabProblems);
			this.tabControlFormMedical.Controls.Add(this.tabMedications);
			this.tabControlFormMedical.Controls.Add(this.tabAllergies);
			this.tabControlFormMedical.Controls.Add(this.tabFamHealthHist);
			this.tabControlFormMedical.Controls.Add(this.tabVitalSigns);
			this.tabControlFormMedical.Controls.Add(this.tabTobaccoUse);
			this.tabControlFormMedical.Location = new System.Drawing.Point(8, 8);
			this.tabControlFormMedical.Name = "tabControlFormMedical";
			this.tabControlFormMedical.Size = new System.Drawing.Size(887, 464);
			this.tabControlFormMedical.TabIndex = 73;
			this.tabControlFormMedical.Selecting += new System.EventHandler<int>(this.tabControlFormMedical_Selecting);
			// 
			// tabMedical
			// 
			this.tabMedical.Controls.Add(this.label4);
			this.tabMedical.Controls.Add(this.label2);
			this.tabMedical.Controls.Add(this.label3);
			this.tabMedical.Controls.Add(this.label1);
			this.tabMedical.Controls.Add(this.textMedical);
			this.tabMedical.Controls.Add(this.butPrintMedical);
			this.tabMedical.Controls.Add(this.textService);
			this.tabMedical.Controls.Add(this.textMedUrgNote);
			this.tabMedical.Controls.Add(this.checkPremed);
			this.tabMedical.Controls.Add(this.groupMedsDocumented);
			this.tabMedical.Controls.Add(this.label6);
			this.tabMedical.Controls.Add(this.textMedicalComp);
			this.tabMedical.Location = new System.Drawing.Point(2, 21);
			this.tabMedical.Name = "tabMedical";
			this.tabMedical.Padding = new System.Windows.Forms.Padding(3);
			this.tabMedical.Size = new System.Drawing.Size(883, 441);
			this.tabMedical.TabIndex = 4;
			this.tabMedical.Text = "Medical Info";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 139);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(131, 18);
			this.label4.TabIndex = 85;
			this.label4.Text = "Medical Summary";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 36);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(131, 18);
			this.label2.TabIndex = 86;
			this.label2.Text = "Med Urgent";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(288, 28);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(151, 18);
			this.label3.TabIndex = 87;
			this.label3.Text = "Service Notes";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(143, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(390, 18);
			this.label1.TabIndex = 93;
			this.label1.Text = "To print medications, use button in Medications tab.";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textMedical
			// 
			this.textMedical.AcceptsTab = true;
			this.textMedical.BackColor = System.Drawing.SystemColors.Window;
			this.textMedical.DetectLinksEnabled = false;
			this.textMedical.DetectUrls = false;
			this.textMedical.Location = new System.Drawing.Point(8, 159);
			this.textMedical.Name = "textMedical";
			this.textMedical.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.MedicalSummary;
			this.textMedical.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textMedical.Size = new System.Drawing.Size(275, 77);
			this.textMedical.TabIndex = 2;
			this.textMedical.Text = "";
			// 
			// butPrintMedical
			// 
			this.butPrintMedical.Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons.Print,16,16);
			this.butPrintMedical.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPrintMedical.Location = new System.Drawing.Point(8, 8);
			this.butPrintMedical.Name = "butPrintMedical";
			this.butPrintMedical.Size = new System.Drawing.Size(112, 24);
			this.butPrintMedical.TabIndex = 92;
			this.butPrintMedical.Text = "Print Medical";
			this.butPrintMedical.Click += new System.EventHandler(this.butPrintMedical_Click);
			// 
			// textService
			// 
			this.textService.AcceptsTab = true;
			this.textService.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textService.BackColor = System.Drawing.SystemColors.Window;
			this.textService.DetectLinksEnabled = false;
			this.textService.DetectUrls = false;
			this.textService.Location = new System.Drawing.Point(289, 63);
			this.textService.Name = "textService";
			this.textService.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.ServiceNotes;
			this.textService.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textService.Size = new System.Drawing.Size(490, 171);
			this.textService.TabIndex = 3;
			this.textService.Text = "";
			// 
			// textMedUrgNote
			// 
			this.textMedUrgNote.AcceptsTab = true;
			this.textMedUrgNote.BackColor = System.Drawing.SystemColors.Window;
			this.textMedUrgNote.DetectLinksEnabled = false;
			this.textMedUrgNote.DetectUrls = false;
			this.textMedUrgNote.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textMedUrgNote.ForeColor = System.Drawing.Color.Red;
			this.textMedUrgNote.Location = new System.Drawing.Point(8, 63);
			this.textMedUrgNote.Name = "textMedUrgNote";
			this.textMedUrgNote.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.MedicalUrgent;
			this.textMedUrgNote.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textMedUrgNote.Size = new System.Drawing.Size(275, 72);
			this.textMedUrgNote.TabIndex = 1;
			this.textMedUrgNote.Text = "";
			// 
			// checkPremed
			// 
			this.checkPremed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkPremed.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkPremed.Location = new System.Drawing.Point(537, 32);
			this.checkPremed.Name = "checkPremed";
			this.checkPremed.Size = new System.Drawing.Size(171, 20);
			this.checkPremed.TabIndex = 5;
			this.checkPremed.Text = "Premedicate (PAC or other)";
			// 
			// groupMedsDocumented
			// 
			this.groupMedsDocumented.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupMedsDocumented.Controls.Add(this.radioMedsDocumentedNo);
			this.groupMedsDocumented.Controls.Add(this.radioMedsDocumentedYes);
			this.groupMedsDocumented.Location = new System.Drawing.Point(716, 8);
			this.groupMedsDocumented.Name = "groupMedsDocumented";
			this.groupMedsDocumented.Size = new System.Drawing.Size(159, 51);
			this.groupMedsDocumented.TabIndex = 6;
			this.groupMedsDocumented.Text = "Current Meds Documented";
			// 
			// radioMedsDocumentedNo
			// 
			this.radioMedsDocumentedNo.Location = new System.Drawing.Point(56, 23);
			this.radioMedsDocumentedNo.Name = "radioMedsDocumentedNo";
			this.radioMedsDocumentedNo.Size = new System.Drawing.Size(41, 20);
			this.radioMedsDocumentedNo.TabIndex = 1;
			this.radioMedsDocumentedNo.Text = "No";
			this.radioMedsDocumentedNo.UseVisualStyleBackColor = true;
			// 
			// radioMedsDocumentedYes
			// 
			this.radioMedsDocumentedYes.Checked = true;
			this.radioMedsDocumentedYes.Location = new System.Drawing.Point(8, 23);
			this.radioMedsDocumentedYes.Name = "radioMedsDocumentedYes";
			this.radioMedsDocumentedYes.Size = new System.Drawing.Size(42, 20);
			this.radioMedsDocumentedYes.TabIndex = 0;
			this.radioMedsDocumentedYes.TabStop = true;
			this.radioMedsDocumentedYes.Text = "Yes";
			this.radioMedsDocumentedYes.UseVisualStyleBackColor = true;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 240);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(607, 18);
			this.label6.TabIndex = 82;
			this.label6.Text = "Medical History - Complete and Detailed (does not show in chart)";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textMedicalComp
			// 
			this.textMedicalComp.AcceptsTab = true;
			this.textMedicalComp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textMedicalComp.BackColor = System.Drawing.SystemColors.Window;
			this.textMedicalComp.DetectLinksEnabled = false;
			this.textMedicalComp.DetectUrls = false;
			this.textMedicalComp.Location = new System.Drawing.Point(8, 260);
			this.textMedicalComp.Name = "textMedicalComp";
			this.textMedicalComp.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.MedicalHistory;
			this.textMedicalComp.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textMedicalComp.Size = new System.Drawing.Size(769, 173);
			this.textMedicalComp.TabIndex = 4;
			this.textMedicalComp.Text = "";
			// 
			// tabProblems
			// 
			this.tabProblems.Controls.Add(this.gridDiseases);
			this.tabProblems.Controls.Add(this.butAddDisease);
			this.tabProblems.Controls.Add(this.checkShowInactiveProblems);
			this.tabProblems.Location = new System.Drawing.Point(2, 21);
			this.tabProblems.Name = "tabProblems";
			this.tabProblems.Padding = new System.Windows.Forms.Padding(3);
			this.tabProblems.Size = new System.Drawing.Size(791, 432);
			this.tabProblems.TabIndex = 0;
			this.tabProblems.Text = "Problems";
			// 
			// tabMedications
			// 
			this.tabMedications.Controls.Add(this.butAdd);
			this.tabMedications.Controls.Add(this.gridMeds);
			this.tabMedications.Controls.Add(this.checkDiscontinued);
			this.tabMedications.Controls.Add(this.butPrint);
			this.tabMedications.Location = new System.Drawing.Point(2, 21);
			this.tabMedications.Name = "tabMedications";
			this.tabMedications.Padding = new System.Windows.Forms.Padding(3);
			this.tabMedications.Size = new System.Drawing.Size(791, 433);
			this.tabMedications.TabIndex = 1;
			this.tabMedications.Text = "Medications";
			// 
			// tabAllergies
			// 
			this.tabAllergies.Controls.Add(this.gridAllergies);
			this.tabAllergies.Controls.Add(this.checkShowInactiveAllergies);
			this.tabAllergies.Controls.Add(this.butAddAllergy);
			this.tabAllergies.Location = new System.Drawing.Point(2, 21);
			this.tabAllergies.Name = "tabAllergies";
			this.tabAllergies.Padding = new System.Windows.Forms.Padding(3);
			this.tabAllergies.Size = new System.Drawing.Size(791, 432);
			this.tabAllergies.TabIndex = 2;
			this.tabAllergies.Text = "Allergies";
			// 
			// tabFamHealthHist
			// 
			this.tabFamHealthHist.Controls.Add(this.gridFamilyHealth);
			this.tabFamHealthHist.Controls.Add(this.butAddFamilyHistory);
			this.tabFamHealthHist.Location = new System.Drawing.Point(2, 21);
			this.tabFamHealthHist.Name = "tabFamHealthHist";
			this.tabFamHealthHist.Padding = new System.Windows.Forms.Padding(3);
			this.tabFamHealthHist.Size = new System.Drawing.Size(791, 432);
			this.tabFamHealthHist.TabIndex = 3;
			this.tabFamHealthHist.Text = "Family Health History";
			// 
			// tabVitalSigns
			// 
			this.tabVitalSigns.Controls.Add(this.butGrowthChart);
			this.tabVitalSigns.Controls.Add(this.butAddVitalSign);
			this.tabVitalSigns.Controls.Add(this.gridVitalSigns);
			this.tabVitalSigns.Location = new System.Drawing.Point(2, 21);
			this.tabVitalSigns.Name = "tabVitalSigns";
			this.tabVitalSigns.Padding = new System.Windows.Forms.Padding(3);
			this.tabVitalSigns.Size = new System.Drawing.Size(791, 432);
			this.tabVitalSigns.TabIndex = 5;
			this.tabVitalSigns.Text = "Vital Signs";
			// 
			// butGrowthChart
			// 
			this.butGrowthChart.AdjustImageLocation = new System.Drawing.Point(0, 1);
			this.butGrowthChart.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butGrowthChart.Location = new System.Drawing.Point(124, 8);
			this.butGrowthChart.Name = "butGrowthChart";
			this.butGrowthChart.Size = new System.Drawing.Size(92, 23);
			this.butGrowthChart.TabIndex = 72;
			this.butGrowthChart.Text = "Growth Chart";
			this.butGrowthChart.Click += new System.EventHandler(this.butGrowthChart_Click);
			// 
			// butAddVitalSign
			// 
			this.butAddVitalSign.AdjustImageLocation = new System.Drawing.Point(0, 1);
			this.butAddVitalSign.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAddVitalSign.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAddVitalSign.Location = new System.Drawing.Point(8, 8);
			this.butAddVitalSign.Name = "butAddVitalSign";
			this.butAddVitalSign.Size = new System.Drawing.Size(110, 23);
			this.butAddVitalSign.TabIndex = 71;
			this.butAddVitalSign.Text = "Add Vital Sign";
			this.butAddVitalSign.Click += new System.EventHandler(this.butAddVitalSign_Click);
			// 
			// gridVitalSigns
			// 
			this.gridVitalSigns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridVitalSigns.Location = new System.Drawing.Point(8, 35);
			this.gridVitalSigns.Name = "gridVitalSigns";
			this.gridVitalSigns.Size = new System.Drawing.Size(775, 389);
			this.gridVitalSigns.TabIndex = 4;
			this.gridVitalSigns.Title = "Vital Signs";
			this.gridVitalSigns.TranslationName = "TableVitals";
			this.gridVitalSigns.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridVitalSigns_CellDoubleClick);
			// 
			// tabTobaccoUse
			// 
			this.tabTobaccoUse.Controls.Add(this.tabControl1);
			this.tabTobaccoUse.Controls.Add(this.label12);
			this.tabTobaccoUse.Controls.Add(this.comboSmokeStatus);
			this.tabTobaccoUse.Controls.Add(this.label13);
			this.tabTobaccoUse.Location = new System.Drawing.Point(2, 21);
			this.tabTobaccoUse.Name = "tabTobaccoUse";
			this.tabTobaccoUse.Padding = new System.Windows.Forms.Padding(3);
			this.tabTobaccoUse.Size = new System.Drawing.Size(788, 430);
			this.tabTobaccoUse.TabIndex = 0;
			this.tabTobaccoUse.Text = "Tobacco Use";
			// 
			// tabControl1
			// 
			this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Location = new System.Drawing.Point(8, 39);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.Size = new System.Drawing.Size(772, 354);
			this.tabControl1.TabIndex = 7;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.radioRecentStatuses);
			this.tabPage1.Controls.Add(this.radioUserStatuses);
			this.tabPage1.Controls.Add(this.label9);
			this.tabPage1.Controls.Add(this.comboTobaccoStatus);
			this.tabPage1.Controls.Add(this.gridAssessments);
			this.tabPage1.Controls.Add(this.labelTobaccoStatus);
			this.tabPage1.Controls.Add(this.radioAllStatuses);
			this.tabPage1.Controls.Add(this.butAddAssessment);
			this.tabPage1.Controls.Add(this.comboAssessmentType);
			this.tabPage1.Controls.Add(this.textDateAssessed);
			this.tabPage1.Controls.Add(this.radioNonUserStatuses);
			this.tabPage1.Controls.Add(this.label11);
			this.tabPage1.Controls.Add(this.label10);
			this.tabPage1.Location = new System.Drawing.Point(2, 21);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(768, 331);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Tobacco Use Assessment";
			// 
			// radioRecentStatuses
			// 
			this.radioRecentStatuses.Location = new System.Drawing.Point(294, 62);
			this.radioRecentStatuses.Name = "radioRecentStatuses";
			this.radioRecentStatuses.Size = new System.Drawing.Size(72, 20);
			this.radioRecentStatuses.TabIndex = 6;
			this.radioRecentStatuses.TabStop = true;
			this.radioRecentStatuses.Text = "Frequent";
			this.radioRecentStatuses.CheckedChanged += new System.EventHandler(this.radioTobaccoStatuses_CheckedChanged);
			// 
			// radioUserStatuses
			// 
			this.radioUserStatuses.Location = new System.Drawing.Point(159, 62);
			this.radioUserStatuses.Name = "radioUserStatuses";
			this.radioUserStatuses.Size = new System.Drawing.Size(48, 20);
			this.radioUserStatuses.TabIndex = 4;
			this.radioUserStatuses.TabStop = true;
			this.radioUserStatuses.Text = "User";
			this.radioUserStatuses.CheckedChanged += new System.EventHandler(this.radioTobaccoStatuses_CheckedChanged);
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(8, 63);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(100, 18);
			this.label9.TabIndex = 0;
			this.label9.Text = "Filter Statuses By";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboTobaccoStatus
			// 
			this.comboTobaccoStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboTobaccoStatus.DropDownWidth = 325;
			this.comboTobaccoStatus.FormattingEnabled = true;
			this.comboTobaccoStatus.Location = new System.Drawing.Point(140, 86);
			this.comboTobaccoStatus.MaxDropDownItems = 30;
			this.comboTobaccoStatus.Name = "comboTobaccoStatus";
			this.comboTobaccoStatus.Size = new System.Drawing.Size(260, 21);
			this.comboTobaccoStatus.TabIndex = 7;
			this.comboTobaccoStatus.SelectionChangeCommitted += new System.EventHandler(this.comboTobaccoStatus_SelectionChangeCommitted);
			// 
			// gridAssessments
			// 
			this.gridAssessments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridAssessments.Location = new System.Drawing.Point(8, 140);
			this.gridAssessments.Name = "gridAssessments";
			this.gridAssessments.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridAssessments.Size = new System.Drawing.Size(738, 183);
			this.gridAssessments.TabIndex = 9;
			this.gridAssessments.Title = "Assessment History";
			this.gridAssessments.TranslationName = "TableAssessment";
			this.gridAssessments.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridAssessments_CellDoubleClick);
			// 
			// labelTobaccoStatus
			// 
			this.labelTobaccoStatus.Location = new System.Drawing.Point(42, 86);
			this.labelTobaccoStatus.Name = "labelTobaccoStatus";
			this.labelTobaccoStatus.Size = new System.Drawing.Size(92, 23);
			this.labelTobaccoStatus.TabIndex = 0;
			this.labelTobaccoStatus.Text = "Tobacco Status";
			this.labelTobaccoStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// radioAllStatuses
			// 
			this.radioAllStatuses.Location = new System.Drawing.Point(114, 62);
			this.radioAllStatuses.Name = "radioAllStatuses";
			this.radioAllStatuses.Size = new System.Drawing.Size(39, 20);
			this.radioAllStatuses.TabIndex = 3;
			this.radioAllStatuses.TabStop = true;
			this.radioAllStatuses.Text = "All";
			this.radioAllStatuses.CheckedChanged += new System.EventHandler(this.radioTobaccoStatuses_CheckedChanged);
			// 
			// butAddAssessment
			// 
			this.butAddAssessment.Location = new System.Drawing.Point(140, 113);
			this.butAddAssessment.Name = "butAddAssessment";
			this.butAddAssessment.Size = new System.Drawing.Size(100, 23);
			this.butAddAssessment.TabIndex = 8;
			this.butAddAssessment.Text = "Add Assessment";
			this.butAddAssessment.Click += new System.EventHandler(this.butAssessed_Click);
			// 
			// comboAssessmentType
			// 
			this.comboAssessmentType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboAssessmentType.DropDownWidth = 350;
			this.comboAssessmentType.FormattingEnabled = true;
			this.comboAssessmentType.Location = new System.Drawing.Point(140, 35);
			this.comboAssessmentType.MaxDropDownItems = 30;
			this.comboAssessmentType.Name = "comboAssessmentType";
			this.comboAssessmentType.Size = new System.Drawing.Size(260, 21);
			this.comboAssessmentType.TabIndex = 2;
			// 
			// textDateAssessed
			// 
			this.textDateAssessed.Location = new System.Drawing.Point(140, 8);
			this.textDateAssessed.Name = "textDateAssessed";
			this.textDateAssessed.ReadOnly = true;
			this.textDateAssessed.Size = new System.Drawing.Size(140, 20);
			this.textDateAssessed.TabIndex = 1;
			// 
			// radioNonUserStatuses
			// 
			this.radioNonUserStatuses.Location = new System.Drawing.Point(213, 62);
			this.radioNonUserStatuses.Name = "radioNonUserStatuses";
			this.radioNonUserStatuses.Size = new System.Drawing.Size(75, 20);
			this.radioNonUserStatuses.TabIndex = 5;
			this.radioNonUserStatuses.TabStop = true;
			this.radioNonUserStatuses.Text = "Non-user";
			this.radioNonUserStatuses.CheckedChanged += new System.EventHandler(this.radioTobaccoStatuses_CheckedChanged);
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(46, 8);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(88, 23);
			this.label11.TabIndex = 0;
			this.label11.Text = "Date Assessed";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(30, 35);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(104, 23);
			this.label10.TabIndex = 0;
			this.label10.Text = "Assessment Type";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.radioRecentInterventions);
			this.tabPage2.Controls.Add(this.comboInterventionCode);
			this.tabPage2.Controls.Add(this.checkPatientDeclined);
			this.tabPage2.Controls.Add(this.label8);
			this.tabPage2.Controls.Add(this.radioAllInterventions);
			this.tabPage2.Controls.Add(this.textDateIntervention);
			this.tabPage2.Controls.Add(this.gridInterventions);
			this.tabPage2.Controls.Add(this.label5);
			this.tabPage2.Controls.Add(this.radioMedInterventions);
			this.tabPage2.Controls.Add(this.label7);
			this.tabPage2.Controls.Add(this.butAddIntervention);
			this.tabPage2.Controls.Add(this.radioCounselInterventions);
			this.tabPage2.Location = new System.Drawing.Point(2, 21);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(768, 328);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Cessation Intervention";
			// 
			// radioRecentInterventions
			// 
			this.radioRecentInterventions.Location = new System.Drawing.Point(281, 35);
			this.radioRecentInterventions.Name = "radioRecentInterventions";
			this.radioRecentInterventions.Size = new System.Drawing.Size(72, 20);
			this.radioRecentInterventions.TabIndex = 5;
			this.radioRecentInterventions.TabStop = true;
			this.radioRecentInterventions.Text = "Frequent";
			this.radioRecentInterventions.CheckedChanged += new System.EventHandler(this.radioInterventions_CheckedChanged);
			// 
			// comboInterventionCode
			// 
			this.comboInterventionCode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboInterventionCode.DropDownWidth = 340;
			this.comboInterventionCode.FormattingEnabled = true;
			this.comboInterventionCode.Location = new System.Drawing.Point(140, 59);
			this.comboInterventionCode.MaxDropDownItems = 30;
			this.comboInterventionCode.Name = "comboInterventionCode";
			this.comboInterventionCode.Size = new System.Drawing.Size(260, 21);
			this.comboInterventionCode.TabIndex = 6;
			this.comboInterventionCode.SelectionChangeCommitted += new System.EventHandler(this.comboInterventionCode_SelectionChangeCommitted);
			// 
			// checkPatientDeclined
			// 
			this.checkPatientDeclined.Location = new System.Drawing.Point(140, 86);
			this.checkPatientDeclined.Name = "checkPatientDeclined";
			this.checkPatientDeclined.Size = new System.Drawing.Size(111, 20);
			this.checkPatientDeclined.TabIndex = 7;
			this.checkPatientDeclined.Text = "Patient Declined";
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(26, 59);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(108, 23);
			this.label8.TabIndex = 0;
			this.label8.Text = "Intervention Code";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// radioAllInterventions
			// 
			this.radioAllInterventions.Location = new System.Drawing.Point(107, 35);
			this.radioAllInterventions.Name = "radioAllInterventions";
			this.radioAllInterventions.Size = new System.Drawing.Size(39, 20);
			this.radioAllInterventions.TabIndex = 2;
			this.radioAllInterventions.TabStop = true;
			this.radioAllInterventions.Text = "All";
			this.radioAllInterventions.CheckedChanged += new System.EventHandler(this.radioInterventions_CheckedChanged);
			// 
			// textDateIntervention
			// 
			this.textDateIntervention.Location = new System.Drawing.Point(140, 8);
			this.textDateIntervention.Name = "textDateIntervention";
			this.textDateIntervention.ReadOnly = true;
			this.textDateIntervention.Size = new System.Drawing.Size(140, 20);
			this.textDateIntervention.TabIndex = 1;
			// 
			// gridInterventions
			// 
			this.gridInterventions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridInterventions.Location = new System.Drawing.Point(8, 137);
			this.gridInterventions.Name = "gridInterventions";
			this.gridInterventions.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridInterventions.Size = new System.Drawing.Size(738, 183);
			this.gridInterventions.TabIndex = 9;
			this.gridInterventions.Title = "Intervention History";
			this.gridInterventions.TranslationName = "TableIntervention";
			this.gridInterventions.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridInterventions_CellDoubleClick);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(38, 8);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(96, 23);
			this.label5.TabIndex = 0;
			this.label5.Text = "Date Intervened";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// radioMedInterventions
			// 
			this.radioMedInterventions.Location = new System.Drawing.Point(152, 35);
			this.radioMedInterventions.Name = "radioMedInterventions";
			this.radioMedInterventions.Size = new System.Drawing.Size(49, 20);
			this.radioMedInterventions.TabIndex = 3;
			this.radioMedInterventions.TabStop = true;
			this.radioMedInterventions.Text = "Med";
			this.radioMedInterventions.CheckedChanged += new System.EventHandler(this.radioInterventions_CheckedChanged);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(8, 36);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(93, 18);
			this.label7.TabIndex = 0;
			this.label7.Text = "Filter Codes By";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butAddIntervention
			// 
			this.butAddIntervention.Location = new System.Drawing.Point(140, 110);
			this.butAddIntervention.Name = "butAddIntervention";
			this.butAddIntervention.Size = new System.Drawing.Size(100, 23);
			this.butAddIntervention.TabIndex = 8;
			this.butAddIntervention.Text = "Add Intervention";
			this.butAddIntervention.Click += new System.EventHandler(this.butIntervention_Click);
			// 
			// radioCounselInterventions
			// 
			this.radioCounselInterventions.Location = new System.Drawing.Point(207, 35);
			this.radioCounselInterventions.Name = "radioCounselInterventions";
			this.radioCounselInterventions.Size = new System.Drawing.Size(68, 20);
			this.radioCounselInterventions.TabIndex = 4;
			this.radioCounselInterventions.TabStop = true;
			this.radioCounselInterventions.Text = "Counsel";
			this.radioCounselInterventions.CheckedChanged += new System.EventHandler(this.radioInterventions_CheckedChanged);
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(380, 10);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(383, 18);
			this.label12.TabIndex = 4;
			this.label12.Text = "Used for calculating MU measures.";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboSmokeStatus
			// 
			this.comboSmokeStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboSmokeStatus.FormattingEnabled = true;
			this.comboSmokeStatus.Location = new System.Drawing.Point(150, 8);
			this.comboSmokeStatus.MaxDropDownItems = 30;
			this.comboSmokeStatus.Name = "comboSmokeStatus";
			this.comboSmokeStatus.Size = new System.Drawing.Size(224, 23);
			this.comboSmokeStatus.TabIndex = 6;
			this.comboSmokeStatus.SelectionChangeCommitted += new System.EventHandler(this.comboSmokeStatus_SelectionChangeCommitted);
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(8, 8);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(136, 23);
			this.label13.TabIndex = 5;
			this.label13.Text = "Current Smoking Status";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FormMedical
			// 
			this.AcceptButton = this.butSave;
			this.ClientSize = new System.Drawing.Size(903, 517);
			this.Controls.Add(this.tabControlFormMedical);
			this.Controls.Add(this.butSave);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormMedical";
			this.ShowInTaskbar = false;
			this.Text = "Medical";
			this.Load += new System.EventHandler(this.FormMedical_Load);
			this.ResizeEnd += new System.EventHandler(this.FormMedical_ResizeEnd);
			this.tabControlFormMedical.ResumeLayout(false);
			this.tabMedical.ResumeLayout(false);
			this.groupMedsDocumented.ResumeLayout(false);
			this.tabProblems.ResumeLayout(false);
			this.tabMedications.ResumeLayout(false);
			this.tabAllergies.ResumeLayout(false);
			this.tabFamHealthHist.ResumeLayout(false);
			this.tabVitalSigns.ResumeLayout(false);
			this.tabTobaccoUse.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		private OpenPmsDental.UI.Button butSave;
		private OpenPmsDental.UI.Button butAdd;
		private OpenPmsDental.UI.Button butAddDisease;// Required designer variable.
		private OpenPmsDental.UI.GridOD gridMeds;
		private OpenPmsDental.UI.GridOD gridDiseases;
		private OpenPmsDental.UI.CheckBox checkDiscontinued;
		private UI.GridOD gridAllergies;
		private UI.Button butAddAllergy;
		private OpenPmsDental.UI.CheckBox checkShowInactiveAllergies;
		private UI.Button butPrint;
		private OpenPmsDental.UI.CheckBox checkShowInactiveProblems;
		private UI.GridOD gridFamilyHealth;
		private UI.Button butAddFamilyHistory;
		private OpenPmsDental.UI.TabControl tabControlFormMedical;
		private OpenPmsDental.UI.TabPage tabProblems;
		private OpenPmsDental.UI.TabPage tabMedications;
		private OpenPmsDental.UI.TabPage tabAllergies;
		private OpenPmsDental.UI.TabPage tabFamHealthHist;
		private OpenPmsDental.UI.TabPage tabMedical;
		private Label label4;
		private Label label2;
		private Label label3;
		private Label label1;
		private ODtextBox textMedical;
		private UI.Button butPrintMedical;
		private ODtextBox textService;
		private ODtextBox textMedUrgNote;
		private OpenPmsDental.UI.CheckBox checkPremed;
		private OpenPmsDental.UI.GroupBox groupMedsDocumented;
		private RadioButton radioMedsDocumentedNo;
		private RadioButton radioMedsDocumentedYes;
		private Label label6;
		private ODtextBox textMedicalComp;
		private OpenPmsDental.UI.TabPage tabVitalSigns;
		private UI.Button butAddVitalSign;
		private UI.GridOD gridVitalSigns;
		private UI.Button butGrowthChart;
		private OpenPmsDental.UI.TabPage tabTobaccoUse;
		private Label label12;
		private ComboBox comboSmokeStatus;
		private Label label13;
		private OpenPmsDental.UI.TabControl tabControl1;
		private OpenPmsDental.UI.TabPage tabPage1;
		private RadioButton radioRecentStatuses;
		private RadioButton radioUserStatuses;
		private Label label9;
		private ComboBox comboTobaccoStatus;
		private UI.GridOD gridAssessments;
		private Label labelTobaccoStatus;
		private RadioButton radioAllStatuses;
		private UI.Button butAddAssessment;
		private ComboBox comboAssessmentType;
		private TextBox textDateAssessed;
		private RadioButton radioNonUserStatuses;
		private Label label11;
		private Label label10;
		private OpenPmsDental.UI.TabPage tabPage2;
		private RadioButton radioRecentInterventions;
		private ComboBox comboInterventionCode;
		private OpenPmsDental.UI.CheckBox checkPatientDeclined;
		private Label label8;
		private RadioButton radioAllInterventions;
		private TextBox textDateIntervention;
		private UI.GridOD gridInterventions;
		private Label label5;
		private RadioButton radioMedInterventions;
		private Label label7;
		private UI.Button butAddIntervention;
		private RadioButton radioCounselInterventions;
	}
}
