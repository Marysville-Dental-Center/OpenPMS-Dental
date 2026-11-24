namespace OpenPmsDental {
	partial class FormCCDPrint {
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
			this.printPreviewControl1 = new System.Windows.Forms.PrintPreviewControl();
			this.labelPage = new System.Windows.Forms.Label();
			this.butForward = new OpenPmsDental.UI.Button();
			this.butBack = new OpenPmsDental.UI.Button();
			this.butPrint = new OpenPmsDental.UI.Button();
			this.butPrintDentist = new OpenPmsDental.UI.Button();
			this.butOverride = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			// 
			// printPreviewControl1
			// 
			this.printPreviewControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.printPreviewControl1.Location = new System.Drawing.Point(0, 0);
			this.printPreviewControl1.Name = "printPreviewControl1";
			this.printPreviewControl1.Size = new System.Drawing.Size(888, 650);
			this.printPreviewControl1.TabIndex = 0;
			// 
			// labelPage
			// 
			this.labelPage.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.labelPage.AutoSize = false;
			this.labelPage.Location = new System.Drawing.Point(372, 662);
			this.labelPage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labelPage.Name = "labelPage";
			this.labelPage.Size = new System.Drawing.Size(44, 24);
			this.labelPage.TabIndex = 3;
			this.labelPage.Text = "0/0";
			// 
			// butForward
			// 
			this.butForward.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butForward.AutoSize = false;
			this.butForward.Location = new System.Drawing.Point(422, 662);
			this.butForward.Name = "butForward";
			this.butForward.Size = new System.Drawing.Size(75, 24);
			this.butForward.TabIndex = 6;
			this.butForward.Text = "Forward";
			this.butForward.UseVisualStyleBackColor = true;
			this.butForward.Click += new System.EventHandler(this.butFwd_Click);
			// 
			// butBack
			// 
			this.butBack.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butBack.AutoSize = false;
			this.butBack.Location = new System.Drawing.Point(291, 662);
			this.butBack.Name = "butBack";
			this.butBack.Size = new System.Drawing.Size(75, 24);
			this.butBack.TabIndex = 5;
			this.butBack.Text = "Back";
			this.butBack.UseVisualStyleBackColor = true;
			this.butBack.Click += new System.EventHandler(this.butBack_Click);
			// 
			// butPrint
			// 
			this.butPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butPrint.Location = new System.Drawing.Point(642, 662);
			this.butPrint.Name = "butPrint";
			this.butPrint.Size = new System.Drawing.Size(116, 24);
			this.butPrint.TabIndex = 4;
			this.butPrint.Text = "Print Patient Copy";
			this.butPrint.UseVisualStyleBackColor = true;
			this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
			// 
			// butPrintDentist
			// 
			this.butPrintDentist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butPrintDentist.Location = new System.Drawing.Point(764, 662);
			this.butPrintDentist.Name = "butPrintDentist";
			this.butPrintDentist.Size = new System.Drawing.Size(116, 24);
			this.butPrintDentist.TabIndex = 7;
			this.butPrintDentist.Text = "Print Dentist Copy";
			this.butPrintDentist.UseVisualStyleBackColor = true;
			this.butPrintDentist.Click += new System.EventHandler(this.butPrintDentist_Click);
			// 
			// butOverride
			// 
			this.butOverride.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butOverride.Location = new System.Drawing.Point(8, 662);
			this.butOverride.Name = "butOverride";
			this.butOverride.Size = new System.Drawing.Size(163, 24);
			this.butOverride.TabIndex = 8;
			this.butOverride.Text = "Write Amounts To Claim";
			this.butOverride.UseVisualStyleBackColor = true;
			this.butOverride.Click += new System.EventHandler(this.butOverride_Click);
			// 
			// FormCCDPrint
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(888, 694);
			this.Controls.Add(this.butOverride);
			this.Controls.Add(this.butPrintDentist);
			this.Controls.Add(this.butForward);
			this.Controls.Add(this.butBack);
			this.Controls.Add(this.butPrint);
			this.Controls.Add(this.labelPage);
			this.Controls.Add(this.printPreviewControl1);
			this.Name = "FormCCDPrint";
			this.Text = "Carrier Response";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Resize += new System.EventHandler(this.FormCCDPrint_Resize);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PrintPreviewControl printPreviewControl1;
		private System.Windows.Forms.Label labelPage;
		private UI.Button butPrint;
		private UI.Button butBack;
		private UI.Button butForward;
		private UI.Button butPrintDentist;
		private UI.Button butOverride;
	}
}

