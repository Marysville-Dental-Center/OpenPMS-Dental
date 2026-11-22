using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormEmployeeEdit {
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

		private void InitializeComponent(){
			this.textLName = new System.Windows.Forms.TextBox();
			this.textFName = new System.Windows.Forms.TextBox();
			this.textMI = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.checkIsHidden = new OpenPmsDental.UI.CheckBox();
			this.butSave = new OpenPmsDental.UI.Button();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.textPhoneExt = new System.Windows.Forms.TextBox();
			this.labelPhoneExt = new System.Windows.Forms.Label();
			this.textPayrollID = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textWirelessPhone = new OpenPmsDental.ValidPhone();
			this.label2 = new System.Windows.Forms.Label();
			this.textEmailWork = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textEmailPersonal = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.checkIsFurloughed = new OpenPmsDental.UI.CheckBox();
			this.checkIsWorkingHome = new OpenPmsDental.UI.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.comboReportsTo = new OpenPmsDental.UI.ComboBox();
			this.labelTitle = new System.Windows.Forms.Label();
			this.comboTitle = new OpenPmsDental.UI.ComboBox();
			this.SuspendLayout();
			// 
			// textLName
			// 
			this.textLName.Location = new System.Drawing.Point(150, 32);
			this.textLName.MaxLength = 100;
			this.textLName.Name = "textLName";
			this.textLName.Size = new System.Drawing.Size(176, 23);
			this.textLName.TabIndex = 0;
			// 
			// textFName
			// 
			this.textFName.Location = new System.Drawing.Point(150, 59);
			this.textFName.MaxLength = 100;
			this.textFName.Name = "textFName";
			this.textFName.Size = new System.Drawing.Size(176, 23);
			this.textFName.TabIndex = 1;
			// 
			// textMI
			// 
			this.textMI.Location = new System.Drawing.Point(150, 86);
			this.textMI.MaxLength = 100;
			this.textMI.Name = "textMI";
			this.textMI.Size = new System.Drawing.Size(88, 20);
			this.textMI.TabIndex = 2;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(76, 32);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(68, 23);
			this.label10.TabIndex = 31;
			this.label10.Text = "Last Name";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(76, 59);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(68, 23);
			this.label8.TabIndex = 29;
			this.label8.Text = "First Name";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(116, 86);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(28, 23);
			this.label7.TabIndex = 28;
			this.label7.Text = "MI";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkIsHidden
			// 
			this.checkIsHidden.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIsHidden.Location = new System.Drawing.Point(150, 8);
			this.checkIsHidden.Name = "checkIsHidden";
			this.checkIsHidden.Size = new System.Drawing.Size(62, 20);
			this.checkIsHidden.TabIndex = 14;
			this.checkIsHidden.Text = "Hidden";
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(375, 398);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 25);
			this.butSave.TabIndex = 12;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(45, 398);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(81, 25);
			this.butDelete.TabIndex = 13;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// textPhoneExt
			// 
			this.textPhoneExt.Location = new System.Drawing.Point(150, 140);
			this.textPhoneExt.MaxLength = 100;
			this.textPhoneExt.Name = "textPhoneExt";
			this.textPhoneExt.Size = new System.Drawing.Size(72, 23);
			this.textPhoneExt.TabIndex = 4;
			// 
			// labelPhoneExt
			// 
			this.labelPhoneExt.Location = new System.Drawing.Point(76, 140);
			this.labelPhoneExt.Name = "labelPhoneExt";
			this.labelPhoneExt.Size = new System.Drawing.Size(68, 23);
			this.labelPhoneExt.TabIndex = 40;
			this.labelPhoneExt.Text = "Phone Ext.";
			this.labelPhoneExt.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPayrollID
			// 
			this.textPayrollID.Location = new System.Drawing.Point(150, 113);
			this.textPayrollID.MaxLength = 100;
			this.textPayrollID.Name = "textPayrollID";
			this.textPayrollID.Size = new System.Drawing.Size(88, 20);
			this.textPayrollID.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(80, 113);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 23);
			this.label1.TabIndex = 42;
			this.label1.Text = "Payroll ID";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textWirelessPhone
			// 
			this.textWirelessPhone.Location = new System.Drawing.Point(150, 167);
			this.textWirelessPhone.MaxLength = 30;
			this.textWirelessPhone.Name = "textWirelessPhone";
			this.textWirelessPhone.Size = new System.Drawing.Size(272, 23);
			this.textWirelessPhone.TabIndex = 5;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(52, 167);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(92, 23);
			this.label2.TabIndex = 44;
			this.label2.Text = "Wireless Phone";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textEmailWork
			// 
			this.textEmailWork.Location = new System.Drawing.Point(150, 194);
			this.textEmailWork.MaxLength = 100;
			this.textEmailWork.Name = "textEmailWork";
			this.textEmailWork.Size = new System.Drawing.Size(272, 20);
			this.textEmailWork.TabIndex = 6;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(72, 194);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 23);
			this.label3.TabIndex = 46;
			this.label3.Text = "Email Work";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textEmailPersonal
			// 
			this.textEmailPersonal.Location = new System.Drawing.Point(150, 221);
			this.textEmailPersonal.MaxLength = 100;
			this.textEmailPersonal.Name = "textEmailPersonal";
			this.textEmailPersonal.Size = new System.Drawing.Size(272, 20);
			this.textEmailPersonal.TabIndex = 7;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(56, 221);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 23);
			this.label4.TabIndex = 48;
			this.label4.Text = "Email Personal";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkIsFurloughed
			// 
			this.checkIsFurloughed.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIsFurloughed.Location = new System.Drawing.Point(150, 302);
			this.checkIsFurloughed.Name = "checkIsFurloughed";
			this.checkIsFurloughed.Size = new System.Drawing.Size(85, 20);
			this.checkIsFurloughed.TabIndex = 10;
			this.checkIsFurloughed.Text = "Furloughed";
			// 
			// checkIsWorkingHome
			// 
			this.checkIsWorkingHome.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIsWorkingHome.Location = new System.Drawing.Point(150, 326);
			this.checkIsWorkingHome.Name = "checkIsWorkingHome";
			this.checkIsWorkingHome.Size = new System.Drawing.Size(137, 20);
			this.checkIsWorkingHome.TabIndex = 11;
			this.checkIsWorkingHome.Text = "Working From Home";
			this.checkIsWorkingHome.Visible = false;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(76, 248);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(68, 23);
			this.label5.TabIndex = 49;
			this.label5.Text = "Reports to";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboReportsTo
			// 
			this.comboReportsTo.Location = new System.Drawing.Point(150, 248);
			this.comboReportsTo.Name = "comboReportsTo";
			this.comboReportsTo.Size = new System.Drawing.Size(176, 23);
			this.comboReportsTo.TabIndex = 8;
			// 
			// labelTitle
			// 
			this.labelTitle.Location = new System.Drawing.Point(108, 275);
			this.labelTitle.Name = "labelTitle";
			this.labelTitle.Size = new System.Drawing.Size(36, 23);
			this.labelTitle.TabIndex = 50;
			this.labelTitle.Text = "Title";
			this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboTitle
			// 
			this.comboTitle.Location = new System.Drawing.Point(150, 275);
			this.comboTitle.Name = "comboTitle";
			this.comboTitle.Size = new System.Drawing.Size(176, 23);
			this.comboTitle.TabIndex = 9;
			// 
			// FormEmployeeEdit
			// 
			this.AcceptButton = this.butSave;
			this.ClientSize = new System.Drawing.Size(458, 431);
			this.Controls.Add(this.comboTitle);
			this.Controls.Add(this.labelTitle);
			this.Controls.Add(this.comboReportsTo);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.checkIsWorkingHome);
			this.Controls.Add(this.checkIsFurloughed);
			this.Controls.Add(this.textEmailPersonal);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textEmailWork);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textWirelessPhone);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textPayrollID);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textPhoneExt);
			this.Controls.Add(this.labelPhoneExt);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.checkIsHidden);
			this.Controls.Add(this.textLName);
			this.Controls.Add(this.textFName);
			this.Controls.Add(this.textMI);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label7);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormEmployeeEdit";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Employee Edit";
			this.Load += new System.EventHandler(this.FormEmployeeEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private System.Windows.Forms.TextBox textLName;
		private System.Windows.Forms.TextBox textFName;
		private System.Windows.Forms.TextBox textMI;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private OpenPmsDental.UI.Button butSave;
		private OpenPmsDental.UI.CheckBox checkIsHidden;
		private OpenPmsDental.UI.Button butDelete;
		private TextBox textPhoneExt;
		private Label labelPhoneExt;
		private TextBox textPayrollID;
		private Label label1;
		private Label label2;
		private TextBox textEmailWork;
		private Label label3;
		private TextBox textEmailPersonal;
		private Label label4;
		private OpenPmsDental.UI.CheckBox checkIsFurloughed;
		private OpenPmsDental.UI.CheckBox checkIsWorkingHome;
		private Label label5;
		private UI.ComboBox comboReportsTo;
		private Label labelTitle;
		private UI.ComboBox comboTitle;
		private ValidPhone textWirelessPhone;
	}
}
