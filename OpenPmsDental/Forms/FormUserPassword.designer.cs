using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormUserPassword {
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
			this.labelNew = new System.Windows.Forms.Label();
			this.textPassword = new System.Windows.Forms.TextBox();
			this.textUserName = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textCurrent = new System.Windows.Forms.TextBox();
			this.labelCurrent = new System.Windows.Forms.Label();
			this.checkShow = new OpenPmsDental.UI.CheckBox();
			this.butSave = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			// 
			// labelNew
			// 
			this.labelNew.Location = new System.Drawing.Point(78, 62);
			this.labelNew.Name = "labelNew";
			this.labelNew.Size = new System.Drawing.Size(88, 23);
			this.labelNew.TabIndex = 2;
			this.labelNew.Text = "New Password";
			this.labelNew.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPassword
			// 
			this.textPassword.Location = new System.Drawing.Point(172, 62);
			this.textPassword.Name = "textPassword";
			this.textPassword.PasswordChar = '*';
			this.textPassword.Size = new System.Drawing.Size(204, 23);
			this.textPassword.TabIndex = 1;
			// 
			// textUserName
			// 
			this.textUserName.Location = new System.Drawing.Point(172, 8);
			this.textUserName.Name = "textUserName";
			this.textUserName.ReadOnly = true;
			this.textUserName.Size = new System.Drawing.Size(204, 23);
			this.textUserName.TabIndex = 5;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(130, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(36, 23);
			this.label3.TabIndex = 6;
			this.label3.Text = "User";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textCurrent
			// 
			this.textCurrent.Location = new System.Drawing.Point(172, 35);
			this.textCurrent.Name = "textCurrent";
			this.textCurrent.PasswordChar = '*';
			this.textCurrent.Size = new System.Drawing.Size(204, 23);
			this.textCurrent.TabIndex = 0;
			// 
			// labelCurrent
			// 
			this.labelCurrent.Location = new System.Drawing.Point(62, 35);
			this.labelCurrent.Name = "labelCurrent";
			this.labelCurrent.Size = new System.Drawing.Size(104, 23);
			this.labelCurrent.TabIndex = 8;
			this.labelCurrent.Text = "Current Password";
			this.labelCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkShow
			// 
			this.checkShow.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkShow.Location = new System.Drawing.Point(172, 89);
			this.checkShow.Name = "checkShow";
			this.checkShow.Size = new System.Drawing.Size(52, 20);
			this.checkShow.TabIndex = 9;
			this.checkShow.Text = "Show";
			this.checkShow.Click += new System.EventHandler(this.checkShow_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(325, 122);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 26);
			this.butSave.TabIndex = 1;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// FormUserPassword
			// 
			this.ClientSize = new System.Drawing.Size(408, 156);
			this.Controls.Add(this.checkShow);
			this.Controls.Add(this.textCurrent);
			this.Controls.Add(this.labelCurrent);
			this.Controls.Add(this.textUserName);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textPassword);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.labelNew);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormUserPassword";
			this.ShowInTaskbar = false;
			this.Text = "Change Password";
			this.Load += new System.EventHandler(this.FormUserPassword_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label labelNew;
		private System.Windows.Forms.TextBox textPassword;
		private TextBox textUserName;
		private Label label3;
		private TextBox textCurrent;
		private Label labelCurrent;
		private OpenPmsDental.UI.CheckBox checkShow;
	}
}
