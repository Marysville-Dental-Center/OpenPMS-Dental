namespace OpenPmsDental{
	partial class FormCodeSystemsImport {
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
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.butImport = new OpenPmsDental.UI.Button();
			this.butImportValueSets = new OpenPmsDental.UI.Button();
			this.gridStatus = new OpenPmsDental.UI.GridOD();
			this.labelStatus = new System.Windows.Forms.Label();
			this.SuspendLayout();
			//
			// gridMain
			//
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.EditableAcceptsCR = true;
			this.gridMain.Location = new System.Drawing.Point(12, 8);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(857, 130);
			this.gridMain.TabIndex = 27;
			this.gridMain.Title = "Code Systems";
			this.gridMain.TranslationName = "TableCodeSystems";
			//
			// butImport
			//
			this.butImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butImport.Enabled = false;
			this.butImport.Location = new System.Drawing.Point(759, 555);
			this.butImport.Name = "butImport";
			this.butImport.Size = new System.Drawing.Size(110, 24);
			this.butImport.TabIndex = 3;
			this.butImport.Text = "&Import CPT File";
			this.butImport.Click += new System.EventHandler(this.butImport_Click);
			//
			// butImportValueSets
			//
			this.butImportValueSets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butImportValueSets.Enabled = false;
			this.butImportValueSets.Location = new System.Drawing.Point(623, 555);
			this.butImportValueSets.Name = "butImportValueSets";
			this.butImportValueSets.Size = new System.Drawing.Size(130, 24);
			this.butImportValueSets.TabIndex = 4;
			this.butImportValueSets.Text = "Import &Value Sets";
			this.butImportValueSets.Click += new System.EventHandler(this.butImportValueSets_Click);
			//
			// gridStatus
			//
			this.gridStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridStatus.Location = new System.Drawing.Point(12, 178);
			this.gridStatus.Name = "gridStatus";
			this.gridStatus.Size = new System.Drawing.Size(857, 373);
			this.gridStatus.TabIndex = 28;
			this.gridStatus.Title = "Code-System Status";
			this.gridStatus.TranslationName = "TableCodeSystemStatus";
			//
			// labelStatus
			//
			this.labelStatus.Location = new System.Drawing.Point(12, 142);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(857, 34);
			this.labelStatus.TabIndex = 29;
			this.labelStatus.Text = "The value sets name codes of the code systems below. A code counts for the EHR pick lists and the quality measures only when its code system's table is loaded: load each table directly into the database (CPT with the import above). In the Release is the version the imported release names.";
			//
			// FormCodeSystemsImport
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(881, 606);
			this.Controls.Add(this.labelStatus);
			this.Controls.Add(this.gridStatus);
			this.Controls.Add(this.butImportValueSets);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.butImport);
			this.Name = "FormCodeSystemsImport";
			this.Text = "Code System Importer";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormCodeSystemsImport_FormClosing);
			this.Load += new System.EventHandler(this.FormCodeSystemsImport_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private OpenPmsDental.UI.Button butImport;
		private UI.GridOD gridMain;
		private OpenPmsDental.UI.Button butImportValueSets;
		private UI.GridOD gridStatus;
		private System.Windows.Forms.Label labelStatus;
	}
}
