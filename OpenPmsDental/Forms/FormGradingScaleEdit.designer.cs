namespace OpenPmsDental{
	partial class FormGradingScaleEdit {
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
			this.label2 = new System.Windows.Forms.Label();
			this.textDescription = new System.Windows.Forms.TextBox();
			this.labelPercent = new System.Windows.Forms.Label();
			this.butSave = new OpenPmsDental.UI.Button();
			this.butAdd = new OpenPmsDental.UI.Button();
			this.comboScaleType = new OpenPmsDental.UI.ComboBox();
			this.labelScaleType = new System.Windows.Forms.Label();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.labelWarning = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(59, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 127;
			this.label2.Text = "Description";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDescription
			// 
			this.textDescription.Location = new System.Drawing.Point(137, 8);
			this.textDescription.MaxLength = 255;
			this.textDescription.Name = "textDescription";
			this.textDescription.Size = new System.Drawing.Size(180, 23);
			this.textDescription.TabIndex = 1;
			// 
			// labelPercent
			// 
			this.labelPercent.Location = new System.Drawing.Point(235, 38);
			this.labelPercent.Name = "labelPercent";
			this.labelPercent.Size = new System.Drawing.Size(177, 17);
			this.labelPercent.TabIndex = 129;
			this.labelPercent.Text = "Assumes 0-100% scale.";
			this.labelPercent.Visible = false;
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(340, 339);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 6;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// butAdd
			// 
			this.butAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butAdd.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(340, 62);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(75, 24);
			this.butAdd.TabIndex = 4;
			this.butAdd.Text = "Add";
			this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
			// 
			// comboScaleType
			// 
			this.comboScaleType.Location = new System.Drawing.Point(137, 35);
			this.comboScaleType.Name = "comboScaleType";
			this.comboScaleType.Size = new System.Drawing.Size(92, 23);
			this.comboScaleType.TabIndex = 2;
			this.comboScaleType.SelectionChangeCommitted += new System.EventHandler(this.comboScaleType_SelectionChangeCommitted);
			// 
			// labelScaleType
			// 
			this.labelScaleType.Location = new System.Drawing.Point(63, 35);
			this.labelScaleType.Name = "labelScaleType";
			this.labelScaleType.Size = new System.Drawing.Size(68, 23);
			this.labelScaleType.TabIndex = 132;
			this.labelScaleType.Text = "Scale Type";
			this.labelScaleType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// gridMain
			// 
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.Location = new System.Drawing.Point(8, 62);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(324, 248);
			this.gridMain.TabIndex = 8;
			this.gridMain.Title = "Grading Scale Items";
			this.gridMain.TranslationName = "TableGrading";
			this.gridMain.DoubleClick += new System.EventHandler(this.gridMain_DoubleClick);
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 339);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 24);
			this.butDelete.TabIndex = 5;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// labelWarning
			// 
			this.labelWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelWarning.ForeColor = System.Drawing.Color.Red;
			this.labelWarning.Location = new System.Drawing.Point(89, 329);
			this.labelWarning.Name = "labelWarning";
			this.labelWarning.Size = new System.Drawing.Size(245, 34);
			this.labelWarning.TabIndex = 130;
			this.labelWarning.Text = "Grading scale items are only used for PickList scale types.";
			this.labelWarning.Visible = false;
			// 
			// FormGradingScaleEdit
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(423, 371);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.comboScaleType);
			this.Controls.Add(this.labelScaleType);
			this.Controls.Add(this.labelWarning);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.labelPercent);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textDescription);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.butAdd);
			this.Name = "FormGradingScaleEdit";
			this.Text = "Grading Scale Edit";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormGradingScaleEdit_FormClosing);
			this.Load += new System.EventHandler(this.FormGradingScaleEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private UI.GridOD gridMain;
		private UI.Button butAdd;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textDescription;
		private System.Windows.Forms.Label labelPercent;
		private UI.Button butSave;
		private OpenPmsDental.UI.ComboBox comboScaleType;
		private System.Windows.Forms.Label labelScaleType;
		private UI.Button butDelete;
		private System.Windows.Forms.Label labelWarning;
	}
}