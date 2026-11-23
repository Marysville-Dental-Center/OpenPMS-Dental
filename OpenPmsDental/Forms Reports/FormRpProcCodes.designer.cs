using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormRpProcCodes {
		private System.ComponentModel.IContainer components = null;

		///<summary></summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if(components != null){
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
			this.butOK = new OpenPmsDental.UI.Button();
			this.listBoxFeeSched = new OpenPmsDental.UI.ListBox();
			this.radioCategories = new System.Windows.Forms.RadioButton();
			this.radioCode = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.listBoxClinics = new OpenPmsDental.UI.ListBox();
			this.listBoxProviders = new OpenPmsDental.UI.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.checkShowBlankFees = new OpenPmsDental.UI.CheckBox();
			this.groupBox5 = new OpenPmsDental.UI.GroupBox();
			this.groupBox5.SuspendLayout();
			this.SuspendLayout();
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(341, 287);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 26);
			this.butOK.TabIndex = 2;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// listBoxFeeSched
			// 
			this.listBoxFeeSched.Location = new System.Drawing.Point(8, 32);
			this.listBoxFeeSched.Name = "listBoxFeeSched";
			this.listBoxFeeSched.Size = new System.Drawing.Size(130, 173);
			this.listBoxFeeSched.TabIndex = 0;
			// 
			// radioCategories
			//
			this.radioCategories.Location = new System.Drawing.Point(8, 32);
			this.radioCategories.Name = "radioCategories";
			this.radioCategories.Size = new System.Drawing.Size(81, 20);
			this.radioCategories.TabIndex = 1;
			this.radioCategories.Text = "Categories";
			// 
			// radioCode
			// 
			this.radioCode.Checked = true;
			this.radioCode.Location = new System.Drawing.Point(8, 8);
			this.radioCode.Name = "radioCode";
			this.radioCode.Size = new System.Drawing.Size(53, 20);
			this.radioCode.TabIndex = 0;
			this.radioCode.TabStop = true;
			this.radioCode.Text = "Code";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(130, 22);
			this.label1.TabIndex = 4;
			this.label1.Text = "Fee Schedule";
			this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// listBoxClinics
			// 
			this.listBoxClinics.Location = new System.Drawing.Point(146, 32);
			this.listBoxClinics.Name = "listBoxClinics";
			this.listBoxClinics.Size = new System.Drawing.Size(130, 173);
			this.listBoxClinics.TabIndex = 5;
			// 
			// listBoxProviders
			// 
			this.listBoxProviders.Location = new System.Drawing.Point(284, 32);
			this.listBoxProviders.Name = "listBoxProviders";
			this.listBoxProviders.Size = new System.Drawing.Size(132, 173);
			this.listBoxProviders.TabIndex = 6;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(146, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(130, 22);
			this.label2.TabIndex = 7;
			this.label2.Text = "Clinic";
			this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(284, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(132, 22);
			this.label3.TabIndex = 8;
			this.label3.Text = "Provider";
			this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// checkShowBlankFees
			// 
			this.checkShowBlankFees.Location = new System.Drawing.Point(8, 209);
			this.checkShowBlankFees.Name = "checkShowBlankFees";
			this.checkShowBlankFees.Size = new System.Drawing.Size(111, 20);
			this.checkShowBlankFees.TabIndex = 9;
			this.checkShowBlankFees.Text = "Show blank fees";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.radioCategories);
			this.groupBox5.Controls.Add(this.radioCode);
			this.groupBox5.Location = new System.Drawing.Point(146, 209);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(114, 60);
			this.groupBox5.TabIndex = 31;
			this.groupBox5.TabStop = false;
			// 
			// FormRpProcCodes
			// 
			this.AcceptButton = this.butOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(424, 321);
			this.Controls.Add(this.groupBox5);
			this.Controls.Add(this.checkShowBlankFees);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.listBoxProviders);
			this.Controls.Add(this.listBoxClinics);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.listBoxFeeSched);
			this.Controls.Add(this.butOK);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormRpProcCodes";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Procedure Codes Report";
			this.Load += new System.EventHandler(this.FormRpProcCodes_Load);
			this.groupBox5.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
		private OpenPmsDental.UI.Button butOK;
		private OpenPmsDental.UI.ListBox listBoxFeeSched;
		private System.Windows.Forms.RadioButton radioCategories;
		private System.Windows.Forms.RadioButton radioCode;
		private Label label1;
		private OpenPmsDental.UI.ListBox listBoxClinics;
		private OpenPmsDental.UI.ListBox listBoxProviders;
		private Label label2;
		private Label label3;
		private OpenPmsDental.UI.CheckBox checkShowBlankFees;
		private OpenPmsDental.UI.GroupBox groupBox5;
	}
}
