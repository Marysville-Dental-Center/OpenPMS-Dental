namespace MySQLManager {
	partial class FormMySQLUserEdit {
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMySQLUserEdit));
			this.butOK = new System.Windows.Forms.Button();
			this.textUser = new System.Windows.Forms.TextBox();
			this.textPassword = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.butCancel = new System.Windows.Forms.Button();
			this.textRetypePassword = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.radioLow = new System.Windows.Forms.RadioButton();
			this.radioFull = new System.Windows.Forms.RadioButton();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(258, 253);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 23);
			this.butOK.TabIndex = 25;
			this.butOK.Text = "&OK";
			this.butOK.UseVisualStyleBackColor = true;
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// textUser
			// 
			this.textUser.Location = new System.Drawing.Point(148, 8);
			this.textUser.Name = "textUser";
			this.textUser.Size = new System.Drawing.Size(212, 23);
			this.textUser.TabIndex = 1;
			// 
			// textPassword
			// 
			this.textPassword.Location = new System.Drawing.Point(148, 35);
			this.textPassword.Name = "textPassword";
			this.textPassword.PasswordChar = '*';
			this.textPassword.Size = new System.Drawing.Size(212, 23);
			this.textPassword.TabIndex = 10;
			this.textPassword.UseSystemPasswordChar = true;
			this.textPassword.TextChanged += new System.EventHandler(this.textPassword_TextChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(34, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(108, 23);
			this.label3.TabIndex = 4;
			this.label3.Text = "MySQL Username";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butCancel
			// 
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butCancel.Location = new System.Drawing.Point(341, 253);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 23);
			this.butCancel.TabIndex = 30;
			this.butCancel.Text = "&Cancel";
			this.butCancel.UseVisualStyleBackColor = true;
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			// 
			// textRetypePassword
			// 
			this.textRetypePassword.Location = new System.Drawing.Point(148, 62);
			this.textRetypePassword.Name = "textRetypePassword";
			this.textRetypePassword.PasswordChar = '*';
			this.textRetypePassword.Size = new System.Drawing.Size(212, 23);
			this.textRetypePassword.TabIndex = 15;
			this.textRetypePassword.UseSystemPasswordChar = true;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(78, 35);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(64, 23);
			this.label5.TabIndex = 40;
			this.label5.Text = "Password";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(42, 62);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(100, 23);
			this.label6.TabIndex = 41;
			this.label6.Text = "Retype Password";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.radioLow);
			this.groupBox1.Controls.Add(this.radioFull);
			this.groupBox1.Location = new System.Drawing.Point(119, 93);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(130, 71);
			this.groupBox1.TabIndex = 20;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Permission Level";
			// 
			// radioLow
			// 
			this.radioLow.Location = new System.Drawing.Point(8, 45);
			this.radioLow.Name = "radioLow";
			this.radioLow.Size = new System.Drawing.Size(51, 18);
			this.radioLow.TabIndex = 45;
			this.radioLow.Text = "Low";
			this.radioLow.UseVisualStyleBackColor = true;
			// 
			// radioFull
			// 
			this.radioFull.Checked = true;
			this.radioFull.Location = new System.Drawing.Point(8, 23);
			this.radioFull.Name = "radioFull";
			this.radioFull.Size = new System.Drawing.Size(48, 18);
			this.radioFull.TabIndex = 44;
			this.radioFull.TabStop = true;
			this.radioFull.Text = "Full";
			this.radioFull.UseVisualStyleBackColor = true;
			// 
			// FormMySQLUserEdit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(408, 245);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.textRetypePassword);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.butCancel);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textUser);
			this.Controls.Add(this.textPassword);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormMySQLUserEdit";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "User Edit";
			this.Load += new System.EventHandler(this.FormMySQLUserManager_Load);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.TextBox textUser;
		private System.Windows.Forms.TextBox textPassword;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button butOK;
		private System.Windows.Forms.Button butCancel;
		private System.Windows.Forms.TextBox textRetypePassword;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioLow;
		private System.Windows.Forms.RadioButton radioFull;
	}
}

