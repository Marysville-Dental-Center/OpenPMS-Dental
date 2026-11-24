namespace OpenPmsDental{
	partial class FormSheetFieldGrid {
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
			this.butDelete = new OpenPmsDental.UI.Button();
			this.comboGrowthBehavior = new OpenPmsDental.UI.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.textHeight = new OpenPmsDental.ValidNum();
			this.label8 = new System.Windows.Forms.Label();
			this.textWidth = new OpenPmsDental.ValidNum();
			this.label7 = new System.Windows.Forms.Label();
			this.textYPos = new OpenPmsDental.ValidNum();
			this.label6 = new System.Windows.Forms.Label();
			this.textXPos = new OpenPmsDental.ValidNum();
			this.label5 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textGridType = new System.Windows.Forms.TextBox();
			this.checkBoxGrowthBehavior = new OpenPmsDental.UI.CheckBox();
			this.labelEnableGrowthBehavior = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(341, 251);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 251);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(77, 24);
			this.butDelete.TabIndex = 115;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// comboGrowthBehavior
			// 
			this.comboGrowthBehavior.Enabled = false;
			this.comboGrowthBehavior.Location = new System.Drawing.Point(214, 62);
			this.comboGrowthBehavior.Name = "comboGrowthBehavior";
			this.comboGrowthBehavior.Size = new System.Drawing.Size(196, 23);
			this.comboGrowthBehavior.TabIndex = 114;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(108, 62);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(100, 23);
			this.label9.TabIndex = 113;
			this.label9.Text = "Growth Behavior";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textHeight
			// 
			this.textHeight.Enabled = false;
			this.textHeight.Location = new System.Drawing.Point(214, 170);
			this.textHeight.MaxVal = 2000;
			this.textHeight.MinVal = 1;
			this.textHeight.Name = "textHeight";
			this.textHeight.Size = new System.Drawing.Size(68, 23);
			this.textHeight.TabIndex = 112;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(160, 170);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(48, 23);
			this.label8.TabIndex = 111;
			this.label8.Text = "Height";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textWidth
			// 
			this.textWidth.Enabled = false;
			this.textWidth.Location = new System.Drawing.Point(214, 143);
			this.textWidth.MaxVal = 2000;
			this.textWidth.MinVal = 1;
			this.textWidth.Name = "textWidth";
			this.textWidth.Size = new System.Drawing.Size(68, 23);
			this.textWidth.TabIndex = 110;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(164, 143);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(44, 23);
			this.label7.TabIndex = 109;
			this.label7.Text = "Width";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textYPos
			// 
			this.textYPos.Location = new System.Drawing.Point(214, 116);
			this.textYPos.MaxVal = 2000;
			this.textYPos.MinVal = -100;
			this.textYPos.Name = "textYPos";
			this.textYPos.Size = new System.Drawing.Size(68, 23);
			this.textYPos.TabIndex = 108;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(168, 116);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 23);
			this.label6.TabIndex = 107;
			this.label6.Text = "Y Pos";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textXPos
			// 
			this.textXPos.Location = new System.Drawing.Point(214, 89);
			this.textXPos.MaxVal = 2000;
			this.textXPos.MinVal = -100;
			this.textXPos.Name = "textXPos";
			this.textXPos.Size = new System.Drawing.Size(68, 23);
			this.textXPos.TabIndex = 106;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(168, 89);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(40, 23);
			this.label5.TabIndex = 105;
			this.label5.Text = "X Pos";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(144, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 23);
			this.label3.TabIndex = 116;
			this.label3.Text = "Grid Type";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textGridType
			// 
			this.textGridType.Enabled = false;
			this.textGridType.Location = new System.Drawing.Point(214, 8);
			this.textGridType.Name = "textGridType";
			this.textGridType.ReadOnly = true;
			this.textGridType.Size = new System.Drawing.Size(196, 23);
			this.textGridType.TabIndex = 127;
			// 
			// checkBoxGrowthBehavior
			// 
			this.checkBoxGrowthBehavior.Location = new System.Drawing.Point(214, 39);
			this.checkBoxGrowthBehavior.Name = "checkBoxGrowthBehavior";
			this.checkBoxGrowthBehavior.Size = new System.Drawing.Size(15, 14);
			this.checkBoxGrowthBehavior.TabIndex = 128;
			this.checkBoxGrowthBehavior.CheckedChanged += new System.EventHandler(this.checkBoxGrowthBehavior_CheckedChanged);
			// 
			// labelEnableGrowthBehavior
			// 
			this.labelEnableGrowthBehavior.Location = new System.Drawing.Point(20, 35);
			this.labelEnableGrowthBehavior.Name = "labelEnableGrowthBehavior";
			this.labelEnableGrowthBehavior.Size = new System.Drawing.Size(188, 23);
			this.labelEnableGrowthBehavior.TabIndex = 129;
			this.labelEnableGrowthBehavior.Text = "Enable Growth Behavior Selection";
			this.labelEnableGrowthBehavior.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FormSheetFieldGrid
			// 
			this.ClientSize = new System.Drawing.Size(424, 283);
			this.Controls.Add(this.labelEnableGrowthBehavior);
			this.Controls.Add(this.checkBoxGrowthBehavior);
			this.Controls.Add(this.textGridType);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.comboGrowthBehavior);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.textHeight);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textWidth);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.textYPos);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.textXPos);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.butSave);
			this.Name = "FormSheetFieldGrid";
			this.Text = "Edit Sheet Grid";
			this.Load += new System.EventHandler(this.FormSheetFieldGrid_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private UI.Button butDelete;
		private UI.ComboBox comboGrowthBehavior;
		private System.Windows.Forms.Label label9;
		private ValidNum textHeight;
		private System.Windows.Forms.Label label8;
		private ValidNum textWidth;
		private System.Windows.Forms.Label label7;
		private ValidNum textYPos;
		private System.Windows.Forms.Label label6;
		private ValidNum textXPos;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textGridType;
		private OpenPmsDental.UI.CheckBox checkBoxGrowthBehavior;
		private System.Windows.Forms.Label labelEnableGrowthBehavior;
	}
}