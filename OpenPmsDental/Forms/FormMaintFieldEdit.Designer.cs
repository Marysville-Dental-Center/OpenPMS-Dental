namespace OpenPmsDental {
	partial class FormMaintFieldEdit {
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
			this.labelName = new System.Windows.Forms.Label();
			this.textName = new System.Windows.Forms.TextBox();
			this.labelType = new System.Windows.Forms.Label();
			this.comboType = new OpenPmsDental.UI.ComboBox();
			this.labelUnit = new System.Windows.Forms.Label();
			this.textUnit = new System.Windows.Forms.TextBox();
			this.checkMin = new OpenPmsDental.UI.CheckBox();
			this.textMin = new System.Windows.Forms.TextBox();
			this.checkMax = new OpenPmsDental.UI.CheckBox();
			this.textMax = new System.Windows.Forms.TextBox();
			this.labelLimits = new System.Windows.Forms.Label();
			this.checkRequired = new OpenPmsDental.UI.CheckBox();
			this.butSave = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			//
			// labelName
			//
			this.labelName.Location = new System.Drawing.Point(47, 8);
			this.labelName.Name = "labelName";
			this.labelName.Size = new System.Drawing.Size(72, 23);
			this.labelName.TabIndex = 0;
			this.labelName.Text = "Field name";
			this.labelName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textName
			//
			this.textName.Location = new System.Drawing.Point(125, 8);
			this.textName.MaxLength = 255;
			this.textName.Name = "textName";
			this.textName.Size = new System.Drawing.Size(248, 23);
			this.textName.TabIndex = 1;
			//
			// labelType
			//
			this.labelType.Location = new System.Drawing.Point(83, 35);
			this.labelType.Name = "labelType";
			this.labelType.Size = new System.Drawing.Size(36, 23);
			this.labelType.TabIndex = 2;
			this.labelType.Text = "Type";
			this.labelType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// comboType
			//
			this.comboType.Location = new System.Drawing.Point(125, 35);
			this.comboType.Name = "comboType";
			this.comboType.Size = new System.Drawing.Size(152, 23);
			this.comboType.TabIndex = 3;
			this.comboType.SelectionChangeCommitted += new System.EventHandler(this.comboType_SelectionChangeCommitted);
			//
			// labelUnit
			//
			this.labelUnit.Location = new System.Drawing.Point(83, 62);
			this.labelUnit.Name = "labelUnit";
			this.labelUnit.Size = new System.Drawing.Size(36, 23);
			this.labelUnit.TabIndex = 4;
			this.labelUnit.Text = "Unit";
			this.labelUnit.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textUnit
			//
			this.textUnit.Location = new System.Drawing.Point(125, 62);
			this.textUnit.MaxLength = 255;
			this.textUnit.Name = "textUnit";
			this.textUnit.Size = new System.Drawing.Size(100, 20);
			this.textUnit.TabIndex = 5;
			//
			// checkMin
			//
			this.checkMin.Location = new System.Drawing.Point(42, 90);
			this.checkMin.Name = "checkMin";
			this.checkMin.Size = new System.Drawing.Size(75, 20);
			this.checkMin.TabIndex = 6;
			this.checkMin.Text = "Minimum";
			this.checkMin.Click += new System.EventHandler(this.checkLimit_Click);
			//
			// textMin
			//
			this.textMin.Location = new System.Drawing.Point(125, 89);
			this.textMin.Name = "textMin";
			this.textMin.Size = new System.Drawing.Size(100, 20);
			this.textMin.TabIndex = 7;
			//
			// checkMax
			//
			this.checkMax.Location = new System.Drawing.Point(42, 117);
			this.checkMax.Name = "checkMax";
			this.checkMax.Size = new System.Drawing.Size(77, 20);
			this.checkMax.TabIndex = 8;
			this.checkMax.Text = "Maximum";
			this.checkMax.Click += new System.EventHandler(this.checkLimit_Click);
			//
			// textMax
			//
			this.textMax.Location = new System.Drawing.Point(125, 116);
			this.textMax.Name = "textMax";
			this.textMax.Size = new System.Drawing.Size(100, 20);
			this.textMax.TabIndex = 9;
			//
			// labelLimits
			//
			this.labelLimits.Location = new System.Drawing.Point(231, 89);
			this.labelLimits.Name = "labelLimits";
			this.labelLimits.Size = new System.Drawing.Size(150, 48);
			this.labelLimits.TabIndex = 10;
			this.labelLimits.Text = "Limits are inclusive. A value outside them is flagged.";
			//
			// checkRequired
			//
			this.checkRequired.Location = new System.Drawing.Point(125, 143);
			this.checkRequired.Name = "checkRequired";
			this.checkRequired.Size = new System.Drawing.Size(200, 20);
			this.checkRequired.TabIndex = 11;
			this.checkRequired.Text = "Required to record a completion";
			//
			// butSave
			//
			this.butSave.Location = new System.Drawing.Point(310, 200);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 12;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			//
			// FormMaintFieldEdit
			//
			this.ClientSize = new System.Drawing.Size(393, 232);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.checkRequired);
			this.Controls.Add(this.labelLimits);
			this.Controls.Add(this.textMax);
			this.Controls.Add(this.checkMax);
			this.Controls.Add(this.textMin);
			this.Controls.Add(this.checkMin);
			this.Controls.Add(this.textUnit);
			this.Controls.Add(this.labelUnit);
			this.Controls.Add(this.comboType);
			this.Controls.Add(this.labelType);
			this.Controls.Add(this.textName);
			this.Controls.Add(this.labelName);
			this.Name = "FormMaintFieldEdit";
			this.Text = "Result Field";
			this.Load += new System.EventHandler(this.FormMaintFieldEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelName;
		private System.Windows.Forms.TextBox textName;
		private System.Windows.Forms.Label labelType;
		private OpenPmsDental.UI.ComboBox comboType;
		private System.Windows.Forms.Label labelUnit;
		private System.Windows.Forms.TextBox textUnit;
		private OpenPmsDental.UI.CheckBox checkMin;
		private System.Windows.Forms.TextBox textMin;
		private OpenPmsDental.UI.CheckBox checkMax;
		private System.Windows.Forms.TextBox textMax;
		private System.Windows.Forms.Label labelLimits;
		private OpenPmsDental.UI.CheckBox checkRequired;
		private OpenPmsDental.UI.Button butSave;
	}
}
