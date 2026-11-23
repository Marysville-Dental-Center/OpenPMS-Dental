using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormInsFilingCodeEdit {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components=null;

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
			this.label1 = new System.Windows.Forms.Label();
			this.textDescription = new System.Windows.Forms.TextBox();
			this.textEclaimCode = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.gridInsFilingCodeSubtypes = new OpenPmsDental.UI.GridOD();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.butSave = new OpenPmsDental.UI.Button();
			this.butAdd = new OpenPmsDental.UI.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.comboGroup = new OpenPmsDental.UI.ComboBox();
			this.checkExcludeOtherCoverage = new OpenPmsDental.UI.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(82, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 23);
			this.label1.TabIndex = 2;
			this.label1.Text = "Description";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDescription
			// 
			this.textDescription.Location = new System.Drawing.Point(160, 8);
			this.textDescription.Name = "textDescription";
			this.textDescription.Size = new System.Drawing.Size(292, 23);
			this.textDescription.TabIndex = 0;
			this.textDescription.TextChanged += new System.EventHandler(this.textDescription_TextChanged);
			// 
			// textEclaimCode
			// 
			this.textEclaimCode.Location = new System.Drawing.Point(160, 35);
			this.textEclaimCode.MaxLength = 255;
			this.textEclaimCode.Name = "textEclaimCode";
			this.textEclaimCode.Size = new System.Drawing.Size(156, 23);
			this.textEclaimCode.TabIndex = 1;
			this.textEclaimCode.TextChanged += new System.EventHandler(this.textEclaimCode_TextChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(74, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 23);
			this.label2.TabIndex = 99;
			this.label2.Text = "Eclaim Code";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// gridInsFilingCodeSubtypes
			// 
			this.gridInsFilingCodeSubtypes.Location = new System.Drawing.Point(160, 113);
			this.gridInsFilingCodeSubtypes.Name = "gridInsFilingCodeSubtypes";
			this.gridInsFilingCodeSubtypes.Size = new System.Drawing.Size(291, 160);
			this.gridInsFilingCodeSubtypes.TabIndex = 4;
			this.gridInsFilingCodeSubtypes.Title = "Insurance Filing Code Subtypes";
			this.gridInsFilingCodeSubtypes.TranslationName = "TableInsFilingCodeSubtypes";
			this.gridInsFilingCodeSubtypes.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridInsFilingCodeSubtypes_CellDoubleClick);
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(27, 341);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(81, 26);
			this.butDelete.TabIndex = 8;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(424, 341);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 26);
			this.butSave.TabIndex = 6;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// butAdd
			// 
			this.butAdd.Enabled = false;
			this.butAdd.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(371, 277);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(80, 24);
			this.butAdd.TabIndex = 5;
			this.butAdd.Text = "&Add";
			this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(110, 62);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(44, 23);
			this.label3.TabIndex = 103;
			this.label3.Text = "Group";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboGroup
			// 
			this.comboGroup.Location = new System.Drawing.Point(160, 62);
			this.comboGroup.Name = "comboGroup";
			this.comboGroup.Size = new System.Drawing.Size(156, 23);
			this.comboGroup.TabIndex = 2;
			// 
			// checkExcludeOtherCoverage
			// 
			this.checkExcludeOtherCoverage.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkExcludeOtherCoverage.Location = new System.Drawing.Point(160, 89);
			this.checkExcludeOtherCoverage.Name = "checkExcludeOtherCoverage";
			this.checkExcludeOtherCoverage.Size = new System.Drawing.Size(264, 20);
			this.checkExcludeOtherCoverage.TabIndex = 3;
			this.checkExcludeOtherCoverage.Text = "Exclude  \'Other Coverage\' on Primary Claims";
			// 
			// FormInsFilingCodeEdit
			// 
			this.ClientSize = new System.Drawing.Size(507, 375);
			this.Controls.Add(this.checkExcludeOtherCoverage);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.comboGroup);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.butAdd);
			this.Controls.Add(this.gridInsFilingCodeSubtypes);
			this.Controls.Add(this.textEclaimCode);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.textDescription);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormInsFilingCodeEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Claim Filing Code";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormInsFilingCodeEdit_FormClosing);
			this.Load += new System.EventHandler(this.FormInsFilingCodeEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textDescription;
		private OpenPmsDental.UI.Button butDelete;
		private System.Windows.Forms.TextBox textEclaimCode;
		private System.Windows.Forms.Label label2;
		private OpenPmsDental.UI.GridOD gridInsFilingCodeSubtypes;
		private OpenPmsDental.UI.Button butAdd;
		private Label label3;
		private UI.ComboBox comboGroup;
		private OpenPmsDental.UI.CheckBox checkExcludeOtherCoverage;
	}
}
