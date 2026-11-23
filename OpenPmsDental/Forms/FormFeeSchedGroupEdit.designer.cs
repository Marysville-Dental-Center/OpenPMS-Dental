namespace OpenPmsDental{
	partial class FormFeeSchedGroupEdit {
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
			this.textDescription = new System.Windows.Forms.TextBox();
			this.labelDescription = new System.Windows.Forms.Label();
			this.butRight = new OpenPmsDental.UI.Button();
			this.butLeft = new OpenPmsDental.UI.Button();
			this.gridGroup = new OpenPmsDental.UI.GridOD();
			this.gridAvailable = new OpenPmsDental.UI.GridOD();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.comboFeeSched = new OpenPmsDental.UI.ComboBox();
			this.labelFeeSched = new System.Windows.Forms.Label();
			this.butPickFeeSched = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(506, 394);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// textDescription
			// 
			this.textDescription.Location = new System.Drawing.Point(230, 8);
			this.textDescription.Name = "textDescription";
			this.textDescription.Size = new System.Drawing.Size(152, 20);
			this.textDescription.TabIndex = 4;
			// 
			// labelDescription
			// 
			this.labelDescription.Location = new System.Drawing.Point(152, 8);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(72, 23);
			this.labelDescription.TabIndex = 5;
			this.labelDescription.Text = "Description";
			this.labelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butRight
			// 
			this.butRight.Icon = OpenPmsDental.UI.EnumIcons.ArrowRight;
			this.butRight.Location = new System.Drawing.Point(277, 194);
			this.butRight.Name = "butRight";
			this.butRight.Size = new System.Drawing.Size(35, 26);
			this.butRight.TabIndex = 54;
			this.butRight.Click += new System.EventHandler(this.butRight_Click);
			// 
			// butLeft
			// 
			this.butLeft.AdjustImageLocation = new System.Drawing.Point(-1, 0);
			this.butLeft.Icon = OpenPmsDental.UI.EnumIcons.ArrowLeft;
			this.butLeft.Location = new System.Drawing.Point(277, 228);
			this.butLeft.Name = "butLeft";
			this.butLeft.Size = new System.Drawing.Size(35, 26);
			this.butLeft.TabIndex = 55;
			this.butLeft.Click += new System.EventHandler(this.butLeft_Click);
			// 
			// gridGroup
			// 
			this.gridGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.gridGroup.Location = new System.Drawing.Point(320, 62);
			this.gridGroup.Name = "gridGroup";
			this.gridGroup.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridGroup.Size = new System.Drawing.Size(261, 324);
			this.gridGroup.TabIndex = 56;
			this.gridGroup.Title = "Clinics in Group";
			this.gridGroup.TranslationName = "Table Group Clinics ";
			// 
			// gridAvailable
			// 
			this.gridAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.gridAvailable.Location = new System.Drawing.Point(8, 62);
			this.gridAvailable.Name = "gridAvailable";
			this.gridAvailable.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridAvailable.Size = new System.Drawing.Size(261, 324);
			this.gridAvailable.TabIndex = 57;
			this.gridAvailable.Title = "Available Clinics";
			this.gridAvailable.TranslationName = "Table Clinics Available";
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 394);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(83, 24);
			this.butDelete.TabIndex = 58;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// comboFeeSched
			// 
			this.comboFeeSched.Location = new System.Drawing.Point(230, 35);
			this.comboFeeSched.Name = "comboFeeSched";
			this.comboFeeSched.Size = new System.Drawing.Size(128, 23);
			this.comboFeeSched.TabIndex = 6;
			this.comboFeeSched.SelectedIndexChanged += new System.EventHandler(this.comboFeeSched_SelectedIndexChanged);
			// 
			// labelFeeSched
			// 
			this.labelFeeSched.Location = new System.Drawing.Point(144, 35);
			this.labelFeeSched.Name = "labelFeeSched";
			this.labelFeeSched.Size = new System.Drawing.Size(80, 23);
			this.labelFeeSched.TabIndex = 7;
			this.labelFeeSched.Text = "Fee Schedule";
			this.labelFeeSched.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butPickFeeSched
			// 
			this.butPickFeeSched.Location = new System.Drawing.Point(364, 35);
			this.butPickFeeSched.Name = "butPickFeeSched";
			this.butPickFeeSched.Size = new System.Drawing.Size(23, 23);
			this.butPickFeeSched.TabIndex = 59;
			this.butPickFeeSched.Text = "...";
			this.butPickFeeSched.Click += new System.EventHandler(this.butPickFeeSched_Click);
			// 
			// FormFeeSchedGroupEdit
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(589, 426);
			this.Controls.Add(this.butPickFeeSched);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.gridAvailable);
			this.Controls.Add(this.gridGroup);
			this.Controls.Add(this.butLeft);
			this.Controls.Add(this.butRight);
			this.Controls.Add(this.labelFeeSched);
			this.Controls.Add(this.comboFeeSched);
			this.Controls.Add(this.labelDescription);
			this.Controls.Add(this.textDescription);
			this.Controls.Add(this.butSave);
			this.Name = "FormFeeSchedGroupEdit";
			this.Text = "Fee Schedule Group Edit";
			this.Load += new System.EventHandler(this.FormFeeSchedGroupEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.TextBox textDescription;
		private System.Windows.Forms.Label labelDescription;
		private UI.Button butRight;
		private UI.Button butLeft;
		private UI.GridOD gridGroup;
		private UI.GridOD gridAvailable;
		private UI.Button butDelete;
		private UI.ComboBox comboFeeSched;
		private System.Windows.Forms.Label labelFeeSched;
		private UI.Button butPickFeeSched;
	}
}