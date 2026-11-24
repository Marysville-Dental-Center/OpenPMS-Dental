namespace OpenPmsDental{
	partial class FormConfirmationSetup {
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
			this.butSave = new OpenPmsDental.UI.Button();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.comboStatusTextMessagedConfirm = new OpenPmsDental.UI.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.comboStatusEmailedConfirm = new OpenPmsDental.UI.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.checkGroupFamilies = new OpenPmsDental.UI.CheckBox();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(781, 539);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 97;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// gridMain
			// 
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.Location = new System.Drawing.Point(8, 8);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(848, 484);
			this.gridMain.TabIndex = 87;
			this.gridMain.Title = "Messages";
			this.gridMain.TranslationName = "TableConfirmMsgs";
			this.gridMain.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			// 
			// comboStatusTextMessagedConfirm
			// 
			this.comboStatusTextMessagedConfirm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboStatusTextMessagedConfirm.Location = new System.Drawing.Point(226, 539);
			this.comboStatusTextMessagedConfirm.Name = "comboStatusTextMessagedConfirm";
			this.comboStatusTextMessagedConfirm.Size = new System.Drawing.Size(233, 23);
			this.comboStatusTextMessagedConfirm.TabIndex = 91;
			// 
			// label6
			// 
			this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label6.Location = new System.Drawing.Point(8, 539);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(212, 23);
			this.label6.TabIndex = 89;
			this.label6.Text = "Status for text messaged confirmation";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboStatusEmailedConfirm
			// 
			this.comboStatusEmailedConfirm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboStatusEmailedConfirm.Location = new System.Drawing.Point(226, 504);
			this.comboStatusEmailedConfirm.Name = "comboStatusEmailedConfirm";
			this.comboStatusEmailedConfirm.Size = new System.Drawing.Size(233, 23);
			this.comboStatusEmailedConfirm.TabIndex = 90;
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.Location = new System.Drawing.Point(8, 504);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(212, 23);
			this.label5.TabIndex = 88;
			this.label5.Text = "Status for e-mailed confirmation";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkGroupFamilies
			// 
			this.checkGroupFamilies.Location = new System.Drawing.Point(752, 505);
			this.checkGroupFamilies.Name = "checkGroupFamilies";
			this.checkGroupFamilies.Size = new System.Drawing.Size(104, 20);
			this.checkGroupFamilies.TabIndex = 99;
			this.checkGroupFamilies.Text = "Group Families";
			// 
			// FormConfirmationSetup
			// 
			this.ClientSize = new System.Drawing.Size(864, 571);
			this.Controls.Add(this.checkGroupFamilies);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.comboStatusTextMessagedConfirm);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.comboStatusEmailedConfirm);
			this.Controls.Add(this.label5);
			this.Name = "FormConfirmationSetup";
			this.Text = "Setup Confirmation";
			this.Load += new System.EventHandler(this.FormConfirmationSetup_Load);
			this.ResumeLayout(false);

		}

		#endregion
		
		private UI.GridOD gridMain;
		private OpenPmsDental.UI.ComboBox comboStatusTextMessagedConfirm;
		private System.Windows.Forms.Label label6;
		private OpenPmsDental.UI.ComboBox comboStatusEmailedConfirm;
		private System.Windows.Forms.Label label5;
		private UI.Button butSave;
		private OpenPmsDental.UI.CheckBox checkGroupFamilies;
	}
}