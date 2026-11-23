namespace OpenPmsDental{
	partial class FormSheetOutputFormat {
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
			this.label1 = new System.Windows.Forms.Label();
			this.checkEmailPat = new OpenPmsDental.UI.CheckBox();
			this.textEmailPat = new System.Windows.Forms.TextBox();
			this.textEmail2 = new System.Windows.Forms.TextBox();
			this.checkEmail2 = new OpenPmsDental.UI.CheckBox();
			this.textPaperCopies = new OpenPmsDental.ValidNum();
			this.butOK = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(58, 10);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(134, 18);
			this.label1.TabIndex = 83;
			this.label1.Text = "Paper copies";
			this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// checkEmailPat
			// 
			this.checkEmailPat.Location = new System.Drawing.Point(8, 35);
			this.checkEmailPat.Name = "checkEmailPat";
			this.checkEmailPat.Size = new System.Drawing.Size(417, 20);
			this.checkEmailPat.TabIndex = 85;
			this.checkEmailPat.Text = "E-mail to patient (not recommended for sensitive information):";
			this.checkEmailPat.Click += new System.EventHandler(this.checkEmailPat_Click);
			// 
			// textEmailPat
			// 
			this.textEmailPat.Location = new System.Drawing.Point(24, 59);
			this.textEmailPat.Name = "textEmailPat";
			this.textEmailPat.Size = new System.Drawing.Size(401, 23);
			this.textEmailPat.TabIndex = 86;
			// 
			// textEmail2
			// 
			this.textEmail2.Location = new System.Drawing.Point(24, 114);
			this.textEmail2.Name = "textEmail2";
			this.textEmail2.Size = new System.Drawing.Size(401, 23);
			this.textEmail2.TabIndex = 88;
			// 
			// checkEmail2
			// 
			this.checkEmail2.Location = new System.Drawing.Point(8, 90);
			this.checkEmail2.Name = "checkEmail2";
			this.checkEmail2.Size = new System.Drawing.Size(417, 20);
			this.checkEmail2.TabIndex = 87;
			this.checkEmail2.Text = "E-mail to referral (not recommended for sensitive information):";
			this.checkEmail2.Click += new System.EventHandler(this.checkEmail2_Click);
			// 
			// textPaperCopies
			// 
			this.textPaperCopies.Location = new System.Drawing.Point(8, 8);
			this.textPaperCopies.MaxVal = 255;
			this.textPaperCopies.MinVal = 0;
			this.textPaperCopies.Name = "textPaperCopies";
			this.textPaperCopies.Size = new System.Drawing.Size(44, 23);
			this.textPaperCopies.TabIndex = 84;
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(350, 199);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 24);
			this.butOK.TabIndex = 3;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// FormSheetOutputFormat
			// 
			this.ClientSize = new System.Drawing.Size(433, 231);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.textEmail2);
			this.Controls.Add(this.checkEmail2);
			this.Controls.Add(this.textEmailPat);
			this.Controls.Add(this.checkEmailPat);
			this.Controls.Add(this.textPaperCopies);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.label1);
			this.Name = "FormSheetOutputFormat";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Sheet Output";
			this.Load += new System.EventHandler(this.FormSheetOutputFormat_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butOK;
		private System.Windows.Forms.Label label1;
		private ValidNum textPaperCopies;
		private OpenPmsDental.UI.CheckBox checkEmailPat;
		private System.Windows.Forms.TextBox textEmailPat;
		private System.Windows.Forms.TextBox textEmail2;
		private OpenPmsDental.UI.CheckBox checkEmail2;
	}
}