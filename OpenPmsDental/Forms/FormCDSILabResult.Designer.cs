namespace OpenPmsDental {
	partial class FormCDSILabResult {
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
			this.butOk = new System.Windows.Forms.Button();
			this.butDelete = new System.Windows.Forms.Button();
			this.textLoinc = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textObsValue = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textLoincDescription = new System.Windows.Forms.TextBox();
			this.butLoinc = new System.Windows.Forms.Button();
			this.comboComparator = new OpenPmsDental.UI.ComboBox();
			this.comboUnits = new OpenPmsDental.UI.ComboBox();
			this.groupBox5 = new OpenPmsDental.UI.GroupBox();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textSnomedDescription = new System.Windows.Forms.TextBox();
			this.butSnomed = new System.Windows.Forms.Button();
			this.textSnomed = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.checkAllResults = new OpenPmsDental.UI.CheckBox();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.groupBox5.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// butOk
			// 
			this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOk.Location = new System.Drawing.Point(328, 328);
			this.butOk.Name = "butOk";
			this.butOk.Size = new System.Drawing.Size(75, 28);
			this.butOk.TabIndex = 5;
			this.butOk.Text = "&OK";
			this.butOk.UseVisualStyleBackColor = true;
			this.butOk.Click += new System.EventHandler(this.butOk_Click);
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Location = new System.Drawing.Point(8, 328);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 28);
			this.butDelete.TabIndex = 7;
			this.butDelete.Text = "Delete";
			this.butDelete.UseVisualStyleBackColor = true;
			// 
			// textLoinc
			// 
			this.textLoinc.Location = new System.Drawing.Point(116, 8);
			this.textLoinc.Name = "textLoinc";
			this.textLoinc.ReadOnly = true;
			this.textLoinc.Size = new System.Drawing.Size(104, 23);
			this.textLoinc.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(62, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 23);
			this.label1.TabIndex = 2;
			this.label1.Text = "LOINC";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textObsValue
			// 
			this.textObsValue.Location = new System.Drawing.Point(165, 23);
			this.textObsValue.Name = "textObsValue";
			this.textObsValue.Size = new System.Drawing.Size(144, 23);
			this.textObsValue.TabIndex = 3;
			this.textObsValue.TextChanged += new System.EventHandler(this.textObsValue_TextChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(38, 35);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 23);
			this.label3.TabIndex = 222;
			this.label3.Text = "Description";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textLoincDescription
			// 
			this.textLoincDescription.Location = new System.Drawing.Point(116, 35);
			this.textLoincDescription.Multiline = true;
			this.textLoincDescription.Name = "textLoincDescription";
			this.textLoincDescription.Size = new System.Drawing.Size(287, 49);
			this.textLoincDescription.TabIndex = 221;
			// 
			// butLoinc
			// 
			this.butLoinc.Location = new System.Drawing.Point(226, 8);
			this.butLoinc.Name = "butLoinc";
			this.butLoinc.Size = new System.Drawing.Size(24, 23);
			this.butLoinc.TabIndex = 223;
			this.butLoinc.Text = "...";
			this.butLoinc.UseVisualStyleBackColor = true;
			this.butLoinc.Click += new System.EventHandler(this.butLoinc_Click);
			// 
			// comboComparator
			// 
			this.comboComparator.Location = new System.Drawing.Point(108, 23);
			this.comboComparator.Name = "comboComparator";
			this.comboComparator.Size = new System.Drawing.Size(51, 23);
			this.comboComparator.TabIndex = 224;
			// 
			// comboUnits
			// 
			this.comboUnits.Location = new System.Drawing.Point(315, 23);
			this.comboUnits.Name = "comboUnits";
			this.comboUnits.Size = new System.Drawing.Size(76, 23);
			this.comboUnits.TabIndex = 225;
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.label5);
			this.groupBox5.Controls.Add(this.comboUnits);
			this.groupBox5.Controls.Add(this.textObsValue);
			this.groupBox5.Controls.Add(this.comboComparator);
			this.groupBox5.Location = new System.Drawing.Point(8, 151);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(399, 54);
			this.groupBox5.TabIndex = 226;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Numeric Results";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(62, 23);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(40, 23);
			this.label5.TabIndex = 226;
			this.label5.Text = "Value";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.textSnomedDescription);
			this.groupBox1.Controls.Add(this.butSnomed);
			this.groupBox1.Controls.Add(this.textSnomed);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Location = new System.Drawing.Point(8, 213);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(399, 107);
			this.groupBox1.TabIndex = 227;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Microbiology Results";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(30, 50);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(72, 23);
			this.label4.TabIndex = 226;
			this.label4.Text = "Description";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textSnomedDescription
			// 
			this.textSnomedDescription.Location = new System.Drawing.Point(108, 50);
			this.textSnomedDescription.Multiline = true;
			this.textSnomedDescription.Name = "textSnomedDescription";
			this.textSnomedDescription.Size = new System.Drawing.Size(277, 49);
			this.textSnomedDescription.TabIndex = 225;
			// 
			// butSnomed
			// 
			this.butSnomed.Location = new System.Drawing.Point(314, 23);
			this.butSnomed.Name = "butSnomed";
			this.butSnomed.Size = new System.Drawing.Size(24, 23);
			this.butSnomed.TabIndex = 224;
			this.butSnomed.Text = "...";
			this.butSnomed.UseVisualStyleBackColor = true;
			this.butSnomed.Click += new System.EventHandler(this.butSnomed_Click);
			// 
			// textSnomed
			// 
			this.textSnomed.Location = new System.Drawing.Point(108, 23);
			this.textSnomed.Name = "textSnomed";
			this.textSnomed.ReadOnly = true;
			this.textSnomed.Size = new System.Drawing.Size(200, 20);
			this.textSnomed.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(22, 23);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "SNOMED CT";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkAllResults
			// 
			this.checkAllResults.Location = new System.Drawing.Point(104, 23);
			this.checkAllResults.Name = "checkAllResults";
			this.checkAllResults.Size = new System.Drawing.Size(277, 20);
			this.checkAllResults.TabIndex = 228;
			this.checkAllResults.Tag = "";
			this.checkAllResults.Text = "Trigger intervention reguardless of result value";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.checkAllResults);
			this.groupBox2.Location = new System.Drawing.Point(8, 92);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(399, 51);
			this.groupBox2.TabIndex = 227;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "All Results";
			// 
			// FormCDSILabResult
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(415, 364);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.groupBox5);
			this.Controls.Add(this.butLoinc);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textLoincDescription);
			this.Controls.Add(this.textLoinc);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.butOk);
			this.Name = "FormCDSILabResult";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Clinical Decision Support Lab";
			this.Load += new System.EventHandler(this.FormLabResultEdit_Load);
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button butOk;
		private System.Windows.Forms.Button butDelete;
		private System.Windows.Forms.TextBox textLoinc;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textObsValue;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textLoincDescription;
		private System.Windows.Forms.Button butLoinc;
		private OpenPmsDental.UI.ComboBox comboComparator;
		private OpenPmsDental.UI.ComboBox comboUnits;
		private OpenPmsDental.UI.GroupBox groupBox5;
		private System.Windows.Forms.Label label5;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textSnomedDescription;
		private System.Windows.Forms.Button butSnomed;
		private System.Windows.Forms.TextBox textSnomed;
		private System.Windows.Forms.Label label2;
		private OpenPmsDental.UI.CheckBox checkAllResults;
		private OpenPmsDental.UI.GroupBox groupBox2;
	}
}