using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormLaboratoryEdit {
		/// <summary>
		/// Required designer variable.
		/// </summary>
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
			this.label8 = new System.Windows.Forms.Label();
			this.textEmail = new System.Windows.Forms.TextBox();
			this.textZip = new System.Windows.Forms.TextBox();
			this.textState = new System.Windows.Forms.TextBox();
			this.textCity = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textAddress = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.comboSlip = new OpenPmsDental.UI.ComboBox();
			this.label21 = new System.Windows.Forms.Label();
			this.textNotes = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textDescription = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.textWirelessPhone = new OpenPmsDental.ValidPhone();
			this.butDeleteTurnaround = new OpenPmsDental.UI.Button();
			this.butAdd = new OpenPmsDental.UI.Button();
			this.textPhone = new OpenPmsDental.ValidPhone();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.butSave = new OpenPmsDental.UI.Button();
			this.checkIsHidden = new OpenPmsDental.UI.CheckBox();
			this.SuspendLayout();
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(60, 143);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(40, 23);
			this.label8.TabIndex = 142;
			this.label8.Text = "Email";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textEmail
			// 
			this.textEmail.Location = new System.Drawing.Point(106, 143);
			this.textEmail.MaxLength = 255;
			this.textEmail.Name = "textEmail";
			this.textEmail.Size = new System.Drawing.Size(296, 23);
			this.textEmail.TabIndex = 141;
			// 
			// textZip
			// 
			this.textZip.Location = new System.Drawing.Point(322, 116);
			this.textZip.MaxLength = 255;
			this.textZip.Name = "textZip";
			this.textZip.Size = new System.Drawing.Size(80, 23);
			this.textZip.TabIndex = 140;
			// 
			// textState
			// 
			this.textState.Location = new System.Drawing.Point(268, 116);
			this.textState.MaxLength = 255;
			this.textState.Name = "textState";
			this.textState.Size = new System.Drawing.Size(48, 23);
			this.textState.TabIndex = 139;
			// 
			// textCity
			// 
			this.textCity.Location = new System.Drawing.Point(106, 116);
			this.textCity.MaxLength = 255;
			this.textCity.Name = "textCity";
			this.textCity.Size = new System.Drawing.Size(156, 23);
			this.textCity.TabIndex = 137;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(28, 116);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(72, 23);
			this.label7.TabIndex = 138;
			this.label7.Text = "City, ST Zip";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textAddress
			// 
			this.textAddress.Location = new System.Drawing.Point(106, 89);
			this.textAddress.MaxLength = 255;
			this.textAddress.Name = "textAddress";
			this.textAddress.Size = new System.Drawing.Size(296, 23);
			this.textAddress.TabIndex = 135;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(44, 89);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(56, 23);
			this.label6.TabIndex = 136;
			this.label6.Text = "Address";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 62);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(92, 23);
			this.label5.TabIndex = 134;
			this.label5.Text = "Wireless Phone";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(48, 309);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(52, 23);
			this.label4.TabIndex = 132;
			this.label4.Text = "Lab Slip";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboSlip
			// 
			this.comboSlip.Location = new System.Drawing.Point(106, 309);
			this.comboSlip.Name = "comboSlip";
			this.comboSlip.Size = new System.Drawing.Size(276, 23);
			this.comboSlip.TabIndex = 131;
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(388, 311);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(283, 18);
			this.label21.TabIndex = 130;
			this.label21.Text = "(custom lab slips may be added in Sheets)";
			this.label21.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// textNotes
			// 
			this.textNotes.Location = new System.Drawing.Point(106, 170);
			this.textNotes.MaxLength = 30000;
			this.textNotes.Multiline = true;
			this.textNotes.Name = "textNotes";
			this.textNotes.Size = new System.Drawing.Size(296, 111);
			this.textNotes.TabIndex = 2;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(56, 170);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(44, 23);
			this.label3.TabIndex = 101;
			this.label3.Text = "Notes";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(52, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 23);
			this.label2.TabIndex = 99;
			this.label2.Text = "Phone";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDescription
			// 
			this.textDescription.Location = new System.Drawing.Point(106, 8);
			this.textDescription.Name = "textDescription";
			this.textDescription.Size = new System.Drawing.Size(296, 23);
			this.textDescription.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(28, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 23);
			this.label1.TabIndex = 2;
			this.label1.Text = "Description";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// gridMain
			// 
			this.gridMain.Location = new System.Drawing.Point(104, 336);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(597, 261);
			this.gridMain.TabIndex = 128;
			this.gridMain.Title = "Turnaround Times";
			this.gridMain.TranslationName = "TableLabTurnaround";
			this.gridMain.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			// 
			// textWirelessPhone
			// 
			this.textWirelessPhone.Location = new System.Drawing.Point(106, 62);
			this.textWirelessPhone.MaxLength = 255;
			this.textWirelessPhone.Name = "textWirelessPhone";
			this.textWirelessPhone.Size = new System.Drawing.Size(156, 23);
			this.textWirelessPhone.TabIndex = 133;
			// 
			// butDeleteTurnaround
			// 
			this.butDeleteTurnaround.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDeleteTurnaround.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDeleteTurnaround.Location = new System.Drawing.Point(8, 364);
			this.butDeleteTurnaround.Name = "butDeleteTurnaround";
			this.butDeleteTurnaround.Size = new System.Drawing.Size(90, 24);
			this.butDeleteTurnaround.TabIndex = 129;
			this.butDeleteTurnaround.Text = "Delete";
			this.butDeleteTurnaround.Click += new System.EventHandler(this.butDeleteTurnaround_Click);
			// 
			// butAdd
			// 
			this.butAdd.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(8, 336);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(90, 24);
			this.butAdd.TabIndex = 127;
			this.butAdd.Text = "Add";
			this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
			// 
			// textPhone
			// 
			this.textPhone.Location = new System.Drawing.Point(106, 35);
			this.textPhone.MaxLength = 255;
			this.textPhone.Name = "textPhone";
			this.textPhone.Size = new System.Drawing.Size(156, 23);
			this.textPhone.TabIndex = 1;
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 609);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(81, 24);
			this.butDelete.TabIndex = 4;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(626, 609);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 8;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// checkIsHidden
			// 
			this.checkIsHidden.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIsHidden.Location = new System.Drawing.Point(106, 285);
			this.checkIsHidden.Name = "checkIsHidden";
			this.checkIsHidden.Size = new System.Drawing.Size(74, 20);
			this.checkIsHidden.TabIndex = 143;
			this.checkIsHidden.Text = "Is Hidden";
			// 
			// FormLaboratoryEdit
			// 
			this.ClientSize = new System.Drawing.Size(709, 641);
			this.Controls.Add(this.checkIsHidden);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textEmail);
			this.Controls.Add(this.textZip);
			this.Controls.Add(this.textState);
			this.Controls.Add(this.textCity);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.textAddress);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.textWirelessPhone);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.comboSlip);
			this.Controls.Add(this.label21);
			this.Controls.Add(this.butDeleteTurnaround);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.butAdd);
			this.Controls.Add(this.textNotes);
			this.Controls.Add(this.textPhone);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.textDescription);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormLaboratoryEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Laboratory";
			this.Load += new System.EventHandler(this.FormLaboratoryEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textDescription;
		private OpenPmsDental.UI.Button butDelete;
		private System.Windows.Forms.TextBox textNotes;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private OpenPmsDental.UI.Button butAdd;
		private OpenPmsDental.UI.GridOD gridMain;
		private OpenPmsDental.UI.Button butDeleteTurnaround;
		private OpenPmsDental.UI.ComboBox comboSlip;
		private Label label21;
		private Label label4;
		private Label label5;
		private TextBox textAddress;
		private Label label6;
		private TextBox textCity;
		private Label label7;
		private TextBox textState;
		private TextBox textZip;
		private TextBox textEmail;
		private Label label8;
		private OpenPmsDental.UI.CheckBox checkIsHidden;
		private ValidPhone textPhone;
		private ValidPhone textWirelessPhone;
	}
}
