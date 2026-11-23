using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormMedicationEdit {
		///<summary>Required designer variable.</summary>
		private System.ComponentModel.IContainer components = null;

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
			this.label1 = new System.Windows.Forms.Label();
			this.textGenericName = new System.Windows.Forms.TextBox();
			this.textMedName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textNotes = new OpenPmsDental.ODtextBox();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.comboPatients = new OpenPmsDental.UI.ComboBox();
			this.labelPatients = new System.Windows.Forms.Label();
			this.labelBrands = new System.Windows.Forms.Label();
			this.comboBrands = new OpenPmsDental.UI.ComboBox();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.labelPatientAllergy = new System.Windows.Forms.Label();
			this.comboPatientAllergy = new OpenPmsDental.UI.ComboBox();
			this.labelRxNorm = new System.Windows.Forms.Label();
			this.textRxNormDesc = new System.Windows.Forms.TextBox();
			this.butRxNormSelect = new OpenPmsDental.UI.Button();
			this.checkHidden = new OpenPmsDental.UI.CheckBox();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(538, 449);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 25);
			this.butSave.TabIndex = 2;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(97, 62);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(84, 23);
			this.label1.TabIndex = 2;
			this.label1.Text = "Generic name";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textGenericName
			// 
			this.textGenericName.Location = new System.Drawing.Point(187, 62);
			this.textGenericName.Name = "textGenericName";
			this.textGenericName.ReadOnly = true;
			this.textGenericName.Size = new System.Drawing.Size(212, 23);
			this.textGenericName.TabIndex = 3;
			// 
			// textMedName
			// 
			this.textMedName.Location = new System.Drawing.Point(187, 35);
			this.textMedName.Name = "textMedName";
			this.textMedName.Size = new System.Drawing.Size(426, 23);
			this.textMedName.TabIndex = 0;
			this.textMedName.TextChanged += new System.EventHandler(this.textMedName_TextChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(109, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 6;
			this.label2.Text = "Drug name";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(54, 98);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(127, 32);
			this.label3.TabIndex = 8;
			this.label3.Text = "Notes\r\n(for generic only)";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textNotes
			// 
			this.textNotes.AcceptsTab = true;
			this.textNotes.BackColor = System.Drawing.SystemColors.Window;
			this.textNotes.DetectLinksEnabled = false;
			this.textNotes.DetectUrls = false;
			this.textNotes.Location = new System.Drawing.Point(187, 98);
			this.textNotes.Name = "textNotes";
			this.textNotes.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.MedicationEdit;
			this.textNotes.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textNotes.Size = new System.Drawing.Size(426, 194);
			this.textNotes.TabIndex = 9;
			this.textNotes.Text = "";
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 448);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(84, 26);
			this.butDelete.TabIndex = 32;
			this.butDelete.Text = "&Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(405, 62);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(208, 32);
			this.label4.TabIndex = 33;
			this.label4.Text = "Make sure you spell it right, because you can\'t ever change it.";
			// 
			// comboPatients
			// 
			this.comboPatients.Location = new System.Drawing.Point(179, 23);
			this.comboPatients.Name = "comboPatients";
			this.comboPatients.Size = new System.Drawing.Size(418, 23);
			this.comboPatients.TabIndex = 34;
			// 
			// labelPatients
			// 
			this.labelPatients.Location = new System.Drawing.Point(61, 23);
			this.labelPatients.Name = "labelPatients";
			this.labelPatients.Size = new System.Drawing.Size(112, 23);
			this.labelPatients.TabIndex = 35;
			this.labelPatients.Text = "Patient medication";
			this.labelPatients.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelBrands
			// 
			this.labelBrands.Location = new System.Drawing.Point(125, 77);
			this.labelBrands.Name = "labelBrands";
			this.labelBrands.Size = new System.Drawing.Size(48, 23);
			this.labelBrands.TabIndex = 37;
			this.labelBrands.Text = "Brands";
			this.labelBrands.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboBrands
			// 
			this.comboBrands.Location = new System.Drawing.Point(179, 77);
			this.comboBrands.Name = "comboBrands";
			this.comboBrands.Size = new System.Drawing.Size(418, 23);
			this.comboBrands.TabIndex = 36;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.labelPatientAllergy);
			this.groupBox1.Controls.Add(this.comboPatientAllergy);
			this.groupBox1.Controls.Add(this.labelPatients);
			this.groupBox1.Controls.Add(this.comboPatients);
			this.groupBox1.Controls.Add(this.labelBrands);
			this.groupBox1.Controls.Add(this.comboBrands);
			this.groupBox1.Location = new System.Drawing.Point(8, 300);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(605, 108);
			this.groupBox1.TabIndex = 38;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Dependencies";
			// 
			// labelPatientAllergy
			// 
			this.labelPatientAllergy.Location = new System.Drawing.Point(85, 50);
			this.labelPatientAllergy.Name = "labelPatientAllergy";
			this.labelPatientAllergy.Size = new System.Drawing.Size(88, 23);
			this.labelPatientAllergy.TabIndex = 39;
			this.labelPatientAllergy.Text = "Patient allergy";
			this.labelPatientAllergy.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboPatientAllergy
			// 
			this.comboPatientAllergy.Location = new System.Drawing.Point(179, 50);
			this.comboPatientAllergy.Name = "comboPatientAllergy";
			this.comboPatientAllergy.Size = new System.Drawing.Size(418, 23);
			this.comboPatientAllergy.TabIndex = 38;
			// 
			// labelRxNorm
			// 
			this.labelRxNorm.Location = new System.Drawing.Point(125, 8);
			this.labelRxNorm.Name = "labelRxNorm";
			this.labelRxNorm.Size = new System.Drawing.Size(56, 23);
			this.labelRxNorm.TabIndex = 6;
			this.labelRxNorm.Text = "RxNorm";
			this.labelRxNorm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textRxNormDesc
			// 
			this.textRxNormDesc.Location = new System.Drawing.Point(187, 8);
			this.textRxNormDesc.Name = "textRxNormDesc";
			this.textRxNormDesc.ReadOnly = true;
			this.textRxNormDesc.Size = new System.Drawing.Size(398, 23);
			this.textRxNormDesc.TabIndex = 0;
			this.textRxNormDesc.TextChanged += new System.EventHandler(this.textMedName_TextChanged);
			// 
			// butRxNormSelect
			// 
			this.butRxNormSelect.Location = new System.Drawing.Point(591, 8);
			this.butRxNormSelect.Name = "butRxNormSelect";
			this.butRxNormSelect.Size = new System.Drawing.Size(22, 23);
			this.butRxNormSelect.TabIndex = 2;
			this.butRxNormSelect.Text = "...";
			this.butRxNormSelect.Click += new System.EventHandler(this.butRxNorm_Click);
			// 
			// checkHidden
			// 
			this.checkHidden.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkHidden.Location = new System.Drawing.Point(187, 416);
			this.checkHidden.Name = "checkHidden";
			this.checkHidden.Size = new System.Drawing.Size(62, 20);
			this.checkHidden.TabIndex = 8;
			this.checkHidden.Text = "Hidden";
			// 
			// FormMedicationEdit
			// 
			this.AcceptButton = this.butSave;
			this.ClientSize = new System.Drawing.Size(621, 482);
			this.Controls.Add(this.checkHidden);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.textNotes);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textRxNormDesc);
			this.Controls.Add(this.textMedName);
			this.Controls.Add(this.labelRxNorm);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textGenericName);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butRxNormSelect);
			this.Controls.Add(this.butSave);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormMedicationEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Medication";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormMedicationEdit_Closing);
			this.Load += new System.EventHandler(this.MedicationEdit_Load);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textGenericName;
		private System.Windows.Forms.TextBox textMedName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private OpenPmsDental.ODtextBox textNotes;
		private OpenPmsDental.UI.Button butDelete;
		private System.Windows.Forms.Label label4;
		private OpenPmsDental.UI.ComboBox comboPatients;
		private System.Windows.Forms.Label labelBrands;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private System.Windows.Forms.Label labelPatients;
		private OpenPmsDental.UI.ComboBox comboBrands;//does not allow changing to non generic.
		private Label labelRxNorm;
		private TextBox textRxNormDesc;
		private UI.Button butRxNormSelect;
		private Label labelPatientAllergy;
		private OpenPmsDental.UI.ComboBox comboPatientAllergy;
		private OpenPmsDental.UI.CheckBox checkHidden;
	}
}
