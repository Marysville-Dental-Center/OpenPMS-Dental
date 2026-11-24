namespace OpenPmsDental{
	partial class FormEncounterTool {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEncounterTool));
			this.butRun = new OpenPmsDental.UI.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.textDateEnd = new OpenPmsDental.ValidDate();
			this.textDateStart = new OpenPmsDental.ValidDate();
			this.labelEncWarning = new System.Windows.Forms.Label();
			this.textEncCodeDescript = new System.Windows.Forms.TextBox();
			this.butEncCpt = new OpenPmsDental.UI.Button();
			this.comboEncCodes = new OpenPmsDental.UI.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textEncCodeValue = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.butEncHcpcs = new OpenPmsDental.UI.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.butEncSnomed = new OpenPmsDental.UI.Button();
			this.butEncCdt = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			// 
			// butRun
			// 
			this.butRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butRun.Location = new System.Drawing.Point(375, 309);
			this.butRun.Name = "butRun";
			this.butRun.Size = new System.Drawing.Size(75, 24);
			this.butRun.TabIndex = 8;
			this.butRun.Text = "&Run";
			this.butRun.Click += new System.EventHandler(this.butRun_Click);
			// 
			// label8
			// 
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.Location = new System.Drawing.Point(72, 57);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(64, 23);
			this.label8.TabIndex = 166;
			this.label8.Text = "Start Date";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label7.Location = new System.Drawing.Point(234, 57);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(60, 23);
			this.label7.TabIndex = 165;
			this.label7.Text = "End Date";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDateEnd
			// 
			this.textDateEnd.Location = new System.Drawing.Point(300, 57);
			this.textDateEnd.Name = "textDateEnd";
			this.textDateEnd.Size = new System.Drawing.Size(76, 23);
			this.textDateEnd.TabIndex = 152;
			// 
			// textDateStart
			// 
			this.textDateStart.Location = new System.Drawing.Point(142, 57);
			this.textDateStart.Name = "textDateStart";
			this.textDateStart.Size = new System.Drawing.Size(76, 23);
			this.textDateStart.TabIndex = 151;
			// 
			// labelEncWarning
			// 
			this.labelEncWarning.ForeColor = System.Drawing.Color.Red;
			this.labelEncWarning.Location = new System.Drawing.Point(34, 250);
			this.labelEncWarning.Name = "labelEncWarning";
			this.labelEncWarning.Size = new System.Drawing.Size(422, 47);
			this.labelEncWarning.TabIndex = 164;
			this.labelEncWarning.Text = "Warning: In order for patients to be considered for CQM calculations, you will ha" +
    "ve to manually create encounters with a qualified code specific to each measure." +
    "";
			// 
			// textEncCodeDescript
			// 
			this.textEncCodeDescript.AcceptsTab = true;
			this.textEncCodeDescript.Location = new System.Drawing.Point(142, 111);
			this.textEncCodeDescript.MaxLength = 2147483647;
			this.textEncCodeDescript.Multiline = true;
			this.textEncCodeDescript.Name = "textEncCodeDescript";
			this.textEncCodeDescript.ReadOnly = true;
			this.textEncCodeDescript.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textEncCodeDescript.Size = new System.Drawing.Size(308, 46);
			this.textEncCodeDescript.TabIndex = 158;
			// 
			// butEncCpt
			// 
			this.butEncCpt.Location = new System.Drawing.Point(306, 195);
			this.butEncCpt.Name = "butEncCpt";
			this.butEncCpt.Size = new System.Drawing.Size(75, 24);
			this.butEncCpt.TabIndex = 156;
			this.butEncCpt.Text = "CPT";
			this.butEncCpt.Click += new System.EventHandler(this.butEncCpt_Click);
			// 
			// comboEncCodes
			// 
			this.comboEncCodes.Location = new System.Drawing.Point(142, 84);
			this.comboEncCodes.Name = "comboEncCodes";
			this.comboEncCodes.Size = new System.Drawing.Size(160, 23);
			this.comboEncCodes.TabIndex = 153;
			this.comboEncCodes.SelectionChangeCommitted += new System.EventHandler(this.comboEncCodes_SelectionChangeCommitted);
			// 
			// label5
			// 
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label5.Location = new System.Drawing.Point(96, 223);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(40, 23);
			this.label5.TabIndex = 163;
			this.label5.Text = "Code";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textEncCodeValue
			// 
			this.textEncCodeValue.Location = new System.Drawing.Point(142, 223);
			this.textEncCodeValue.Name = "textEncCodeValue";
			this.textEncCodeValue.ReadOnly = true;
			this.textEncCodeValue.Size = new System.Drawing.Size(160, 23);
			this.textEncCodeValue.TabIndex = 162;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(432, 47);
			this.label1.TabIndex = 167;
			this.label1.Text = resources.GetString("label1.Text");
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(64, 111);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 159;
			this.label2.Text = "Description";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(8, 84);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(128, 23);
			this.label4.TabIndex = 160;
			this.label4.Text = "Recommended Codes";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butEncHcpcs
			// 
			this.butEncHcpcs.Location = new System.Drawing.Point(387, 195);
			this.butEncHcpcs.Name = "butEncHcpcs";
			this.butEncHcpcs.Size = new System.Drawing.Size(75, 24);
			this.butEncHcpcs.TabIndex = 157;
			this.butEncHcpcs.Text = "HCPCS";
			this.butEncHcpcs.Click += new System.EventHandler(this.butEncHcpcs_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(142, 161);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(326, 32);
			this.label3.TabIndex = 161;
			this.label3.Text = "Choosing a code not in the recommended list might make it more difficult to incre" +
    "ase your CQM percentages.";
			// 
			// butEncSnomed
			// 
			this.butEncSnomed.Location = new System.Drawing.Point(142, 195);
			this.butEncSnomed.Name = "butEncSnomed";
			this.butEncSnomed.Size = new System.Drawing.Size(77, 24);
			this.butEncSnomed.TabIndex = 154;
			this.butEncSnomed.Text = "SNOMED CT";
			this.butEncSnomed.Click += new System.EventHandler(this.butEncSnomed_Click);
			// 
			// butEncCdt
			// 
			this.butEncCdt.Location = new System.Drawing.Point(225, 195);
			this.butEncCdt.Name = "butEncCdt";
			this.butEncCdt.Size = new System.Drawing.Size(75, 24);
			this.butEncCdt.TabIndex = 155;
			this.butEncCdt.Text = "CDT";
			this.butEncCdt.Click += new System.EventHandler(this.butEncCdt_Click);
			// 
			// FormEncounterTool
			// 
			this.AllowDrop = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(476, 341);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.textDateEnd);
			this.Controls.Add(this.textDateStart);
			this.Controls.Add(this.labelEncWarning);
			this.Controls.Add(this.textEncCodeDescript);
			this.Controls.Add(this.butEncCpt);
			this.Controls.Add(this.comboEncCodes);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.textEncCodeValue);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.butEncHcpcs);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.butEncSnomed);
			this.Controls.Add(this.butEncCdt);
			this.Controls.Add(this.butRun);
			this.Name = "FormEncounterTool";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Insert Encounters";
			this.Load += new System.EventHandler(this.FormEncounterTool_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butRun;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private ValidDate textDateEnd;
		private ValidDate textDateStart;
		private System.Windows.Forms.Label labelEncWarning;
		private System.Windows.Forms.TextBox textEncCodeDescript;
		private UI.Button butEncCpt;
		private OpenPmsDental.UI.ComboBox comboEncCodes;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textEncCodeValue;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private UI.Button butEncHcpcs;
		private System.Windows.Forms.Label label3;
		private UI.Button butEncSnomed;
		private UI.Button butEncCdt;
	}
}