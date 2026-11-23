namespace OpenPmsDental{
	partial class FormInsBlueBookRules {
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
			this.checkEnableAnonFeeShare = new OpenPmsDental.UI.CheckBox();
			this.checkUsePlanNumInHierarchy = new OpenPmsDental.UI.CheckBox();
			this.listAllowedFeeMethod = new OpenPmsDental.UI.ListBox();
			this.labelAllowedFeeMethod = new System.Windows.Forms.Label();
			this.labelBlueBookFeature = new System.Windows.Forms.Label();
			this.listBlueBookFeature = new OpenPmsDental.UI.ListBox();
			this.butDown = new OpenPmsDental.UI.Button();
			this.butUp = new OpenPmsDental.UI.Button();
			this.labelArrowButtons = new System.Windows.Forms.Label();
			this.textUcrFeePercent = new OpenPmsDental.ValidNum();
			this.labelUcrFeePercent = new System.Windows.Forms.Label();
			this.groupBlueBookSettings = new OpenPmsDental.UI.GroupBox();
			this.gridInsBlueBookRules = new OpenPmsDental.UI.GridOD();
			this.groupBlueBookSettings.SuspendLayout();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(577, 375);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.ButSave_Click);
			// 
			// checkEnableAnonFeeShare
			// 
			this.checkEnableAnonFeeShare.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.checkEnableAnonFeeShare.Location = new System.Drawing.Point(430, 234);
			this.checkEnableAnonFeeShare.Name = "checkEnableAnonFeeShare";
			this.checkEnableAnonFeeShare.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkEnableAnonFeeShare.Size = new System.Drawing.Size(183, 20);
			this.checkEnableAnonFeeShare.TabIndex = 260;
			this.checkEnableAnonFeeShare.Text = "Allow anonymous fee sharing";
			// 
			// checkUsePlanNumInHierarchy
			// 
			this.checkUsePlanNumInHierarchy.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.checkUsePlanNumInHierarchy.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkUsePlanNumInHierarchy.Location = new System.Drawing.Point(8, 23);
			this.checkUsePlanNumInHierarchy.Name = "checkUsePlanNumInHierarchy";
			this.checkUsePlanNumInHierarchy.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkUsePlanNumInHierarchy.Size = new System.Drawing.Size(192, 20);
			this.checkUsePlanNumInHierarchy.TabIndex = 261;
			this.checkUsePlanNumInHierarchy.Text = "Use Insurance Plan in hierarchy";
			this.checkUsePlanNumInHierarchy.Click += new System.EventHandler(this.checkUsePlanNumInHierarchy_Click);
			// 
			// listAllowedFeeMethod
			// 
			this.listAllowedFeeMethod.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.listAllowedFeeMethod.Location = new System.Drawing.Point(10, 207);
			this.listAllowedFeeMethod.Name = "listAllowedFeeMethod";
			this.listAllowedFeeMethod.Size = new System.Drawing.Size(145, 56);
			this.listAllowedFeeMethod.TabIndex = 262;
			// 
			// labelAllowedFeeMethod
			// 
			this.labelAllowedFeeMethod.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.labelAllowedFeeMethod.Location = new System.Drawing.Point(8, 187);
			this.labelAllowedFeeMethod.Name = "labelAllowedFeeMethod";
			this.labelAllowedFeeMethod.Size = new System.Drawing.Size(145, 18);
			this.labelAllowedFeeMethod.TabIndex = 263;
			this.labelAllowedFeeMethod.Text = "*Allowed Fee Method";
			this.labelAllowedFeeMethod.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// labelBlueBookFeature
			// 
			this.labelBlueBookFeature.Location = new System.Drawing.Point(8, 8);
			this.labelBlueBookFeature.Name = "labelBlueBookFeature";
			this.labelBlueBookFeature.Size = new System.Drawing.Size(124, 18);
			this.labelBlueBookFeature.TabIndex = 265;
			this.labelBlueBookFeature.Text = "Blue Book Feature";
			this.labelBlueBookFeature.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// listBlueBookFeature
			// 
			this.listBlueBookFeature.Location = new System.Drawing.Point(8, 28);
			this.listBlueBookFeature.Name = "listBlueBookFeature";
			this.listBlueBookFeature.Size = new System.Drawing.Size(203, 56);
			this.listBlueBookFeature.TabIndex = 264;
			this.listBlueBookFeature.SelectedIndexChanged += new System.EventHandler(this.listBlueBookFeature_SelectedIndexChanged);
			// 
			// butDown
			// 
			this.butDown.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.butDown.Icon = OpenPmsDental.UI.EnumIcons.Down;
			this.butDown.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDown.Location = new System.Drawing.Point(516, 119);
			this.butDown.Name = "butDown";
			this.butDown.Size = new System.Drawing.Size(75, 24);
			this.butDown.TabIndex = 269;
			this.butDown.Text = "&Down";
			this.butDown.Click += new System.EventHandler(this.ButDown_Click);
			// 
			// butUp
			// 
			this.butUp.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.butUp.Icon = OpenPmsDental.UI.EnumIcons.Up;
			this.butUp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butUp.Location = new System.Drawing.Point(516, 91);
			this.butUp.Name = "butUp";
			this.butUp.Size = new System.Drawing.Size(75, 24);
			this.butUp.TabIndex = 268;
			this.butUp.Text = "&Up";
			this.butUp.Click += new System.EventHandler(this.ButUp_Click);
			// 
			// labelArrowButtons
			// 
			this.labelArrowButtons.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.labelArrowButtons.Location = new System.Drawing.Point(475, 147);
			this.labelArrowButtons.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			this.labelArrowButtons.Name = "labelArrowButtons";
			this.labelArrowButtons.Size = new System.Drawing.Size(157, 36);
			this.labelArrowButtons.TabIndex = 270;
			this.labelArrowButtons.Text = "Select a rule and use arrow\r\nbuttons to adjust priority.\r\n";
			// 
			// textUcrFeePercent
			// 
			this.textUcrFeePercent.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.textUcrFeePercent.Location = new System.Drawing.Point(430, 207);
			this.textUcrFeePercent.MaxVal = 100;
			this.textUcrFeePercent.MinVal = 0;
			this.textUcrFeePercent.Name = "textUcrFeePercent";
			this.textUcrFeePercent.Size = new System.Drawing.Size(40, 23);
			this.textUcrFeePercent.TabIndex = 271;
			this.textUcrFeePercent.ShowZero = false;
			// 
			// labelUcrFeePercent
			// 
			this.labelUcrFeePercent.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.labelUcrFeePercent.Location = new System.Drawing.Point(252, 207);
			this.labelUcrFeePercent.Name = "labelUcrFeePercent";
			this.labelUcrFeePercent.Size = new System.Drawing.Size(172, 23);
			this.labelUcrFeePercent.TabIndex = 272;
			this.labelUcrFeePercent.Text = "Standard Provider Fee Percent";
			this.labelUcrFeePercent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBlueBookSettings
			// 
			this.groupBlueBookSettings.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.groupBlueBookSettings.Controls.Add(this.gridInsBlueBookRules);
			this.groupBlueBookSettings.Controls.Add(this.checkUsePlanNumInHierarchy);
			this.groupBlueBookSettings.Controls.Add(this.listAllowedFeeMethod);
			this.groupBlueBookSettings.Controls.Add(this.labelArrowButtons);
			this.groupBlueBookSettings.Controls.Add(this.textUcrFeePercent);
			this.groupBlueBookSettings.Controls.Add(this.checkEnableAnonFeeShare);
			this.groupBlueBookSettings.Controls.Add(this.butDown);
			this.groupBlueBookSettings.Controls.Add(this.butUp);
			this.groupBlueBookSettings.Controls.Add(this.labelAllowedFeeMethod);
			this.groupBlueBookSettings.Controls.Add(this.labelUcrFeePercent);
			this.groupBlueBookSettings.Location = new System.Drawing.Point(8, 92);
			this.groupBlueBookSettings.Name = "groupBlueBookSettings";
			this.groupBlueBookSettings.Size = new System.Drawing.Size(644, 271);
			this.groupBlueBookSettings.TabIndex = 273;
			this.groupBlueBookSettings.TabStop = false;
			this.groupBlueBookSettings.Text = "Blue Book Settings";
			// 
			// gridInsBlueBookRules
			// 
			this.gridInsBlueBookRules.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.gridInsBlueBookRules.Location = new System.Drawing.Point(10, 47);
			this.gridInsBlueBookRules.Name = "gridInsBlueBookRules";
			this.gridInsBlueBookRules.Size = new System.Drawing.Size(459, 135);
			this.gridInsBlueBookRules.TabIndex = 266;
			this.gridInsBlueBookRules.Title = "Insurance Blue Book Rules Hierarchy";
			this.gridInsBlueBookRules.TranslationName = "TableInsuranceBlueBookRules";
			this.gridInsBlueBookRules.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.GridInsBlueBookRules_CellDoubleClick);
			// 
			// FormInsBlueBookRules
			// 
			this.ClientSize = new System.Drawing.Size(660, 407);
			this.Controls.Add(this.groupBlueBookSettings);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.labelBlueBookFeature);
			this.Controls.Add(this.listBlueBookFeature);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FormInsBlueBookRules";
			this.Text = "Insurance Blue Book Setup";
			this.Load += new System.EventHandler(this.FormInsBlueBookRules_Load);
			this.groupBlueBookSettings.ResumeLayout(false);
			this.groupBlueBookSettings.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private OpenPmsDental.UI.CheckBox checkEnableAnonFeeShare;
		private OpenPmsDental.UI.CheckBox checkUsePlanNumInHierarchy;
		private OpenPmsDental.UI.ListBox listAllowedFeeMethod;
		private System.Windows.Forms.Label labelAllowedFeeMethod;
		private System.Windows.Forms.Label labelBlueBookFeature;
		private OpenPmsDental.UI.ListBox listBlueBookFeature;
		private UI.GridOD gridInsBlueBookRules;
		private UI.Button butDown;
		private UI.Button butUp;
		private System.Windows.Forms.Label labelArrowButtons;
		private ValidNum textUcrFeePercent;
		private System.Windows.Forms.Label labelUcrFeePercent;
		private OpenPmsDental.UI.GroupBox groupBlueBookSettings;
	}
}