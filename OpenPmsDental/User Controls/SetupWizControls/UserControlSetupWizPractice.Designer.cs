namespace OpenPmsDental.User_Controls.SetupWizard {
	partial class UserControlSetupWizPractice {
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserControlSetupWizPractice));
			this.textFax = new OpenPmsDental.ValidPhone();
			this.textPhone = new OpenPmsDental.ValidPhone();
			this.label19 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.textPracticeTitle = new System.Windows.Forms.TextBox();
			this.listProvider = new OpenPmsDental.UI.ListBox();
			this.label10 = new System.Windows.Forms.Label();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.textZip = new System.Windows.Forms.TextBox();
			this.textST = new System.Windows.Forms.TextBox();
			this.textCity = new System.Windows.Forms.TextBox();
			this.textAddress2 = new System.Windows.Forms.TextBox();
			this.textAddress = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.butAdvanced = new OpenPmsDental.UI.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// textFax
			// 
			this.textFax.IsFormattingEnabled = false;
			this.textFax.Location = new System.Drawing.Point(94, 62);
			this.textFax.Name = "textFax";
			this.textFax.Size = new System.Drawing.Size(562, 23);
			this.textFax.TabIndex = 100;
			this.textFax.Validated += new System.EventHandler(this.Control_Validated);
			// 
			// textPhone
			// 
			this.textPhone.IsFormattingEnabled = false;
			this.textPhone.Location = new System.Drawing.Point(94, 35);
			this.textPhone.Name = "textPhone";
			this.textPhone.Size = new System.Drawing.Size(562, 23);
			this.textPhone.TabIndex = 99;
			this.textPhone.Validated += new System.EventHandler(this.Control_Validated);
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(8, 62);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(80, 23);
			this.label19.TabIndex = 91;
			this.label19.Text = "Fax";
			this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(8, 35);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(80, 23);
			this.label9.TabIndex = 92;
			this.label9.Text = "Phone";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPracticeTitle
			// 
			this.textPracticeTitle.Location = new System.Drawing.Point(94, 8);
			this.textPracticeTitle.Name = "textPracticeTitle";
			this.textPracticeTitle.Size = new System.Drawing.Size(562, 23);
			this.textPracticeTitle.TabIndex = 98;
			// 
			// listProvider
			// 
			this.listProvider.Items.AddRange(new object[] {
            ""});
			this.listProvider.Location = new System.Drawing.Point(672, 28);
			this.listProvider.Name = "listProvider";
			this.listProvider.Size = new System.Drawing.Size(250, 227);
			this.listProvider.TabIndex = 106;
			this.listProvider.Validated += new System.EventHandler(this.Control_Validated);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(672, 8);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(250, 18);
			this.label10.TabIndex = 95;
			this.label10.Text = "Default Provider";
			this.label10.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label16);
			this.groupBox2.Controls.Add(this.textZip);
			this.groupBox2.Controls.Add(this.textST);
			this.groupBox2.Controls.Add(this.textCity);
			this.groupBox2.Controls.Add(this.textAddress2);
			this.groupBox2.Controls.Add(this.textAddress);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Location = new System.Drawing.Point(8, 93);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(656, 162);
			this.groupBox2.TabIndex = 101;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Physical Treating Address";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 77);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 8;
			this.label2.Text = "City";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 104);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 23);
			this.label1.TabIndex = 7;
			this.label1.Text = "State";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 131);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(72, 23);
			this.label6.TabIndex = 6;
			this.label6.Text = "Zip Code";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(8, 50);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(72, 23);
			this.label16.TabIndex = 0;
			this.label16.Text = "Address 2";
			this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textZip
			// 
			this.textZip.Location = new System.Drawing.Point(86, 131);
			this.textZip.Name = "textZip";
			this.textZip.Size = new System.Drawing.Size(104, 23);
			this.textZip.TabIndex = 5;
			// 
			// textST
			// 
			this.textST.Location = new System.Drawing.Point(86, 104);
			this.textST.Name = "textST";
			this.textST.Size = new System.Drawing.Size(52, 23);
			this.textST.TabIndex = 4;
			// 
			// textCity
			// 
			this.textCity.Location = new System.Drawing.Point(86, 77);
			this.textCity.Name = "textCity";
			this.textCity.Size = new System.Drawing.Size(554, 23);
			this.textCity.TabIndex = 3;
			// 
			// textAddress2
			// 
			this.textAddress2.Location = new System.Drawing.Point(86, 50);
			this.textAddress2.Name = "textAddress2";
			this.textAddress2.Size = new System.Drawing.Size(554, 23);
			this.textAddress2.TabIndex = 2;
			// 
			// textAddress
			// 
			this.textAddress.Location = new System.Drawing.Point(86, 23);
			this.textAddress.Name = "textAddress";
			this.textAddress.Size = new System.Drawing.Size(554, 23);
			this.textAddress.TabIndex = 1;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 23);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(72, 23);
			this.label5.TabIndex = 0;
			this.label5.Text = "Address";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 23);
			this.label3.TabIndex = 90;
			this.label3.Text = "Practice Title";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butAdvanced
			// 
			this.butAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butAdvanced.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdvanced.Location = new System.Drawing.Point(840, 287);
			this.butAdvanced.Name = "butAdvanced";
			this.butAdvanced.Size = new System.Drawing.Size(82, 24);
			this.butAdvanced.TabIndex = 108;
			this.butAdvanced.Text = "Advanced";
			this.butAdvanced.Click += new System.EventHandler(this.butAdvanced_Click);
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.Location = new System.Drawing.Point(237, 263);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(685, 20);
			this.label4.TabIndex = 107;
			this.label4.Text = "Further modifications to this list can be made by going to Setup -> Practice, or " +
    "clicking \"Advanced\".";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// UserControlSetupWizPractice
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.butAdvanced);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textFax);
			this.Controls.Add(this.textPhone);
			this.Controls.Add(this.label19);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.textPracticeTitle);
			this.Controls.Add(this.listProvider);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.label3);
			this.Name = "UserControlSetupWizPractice";
			this.Size = new System.Drawing.Size(930, 530);
			this.Load += new System.EventHandler(this.UserControlSetupWizPractice_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ValidPhone textFax;
		private ValidPhone textPhone;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox textPracticeTitle;
		private OpenPmsDental.UI.ListBox listProvider;
		private System.Windows.Forms.Label label10;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.TextBox textZip;
		private System.Windows.Forms.TextBox textST;
		private System.Windows.Forms.TextBox textCity;
		private System.Windows.Forms.TextBox textAddress2;
		private System.Windows.Forms.TextBox textAddress;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label6;
		private UI.Button butAdvanced;
		private System.Windows.Forms.Label label4;
	}
}
