namespace OpenPmsDental{
	partial class FormBenefitFrequencyEdit {
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
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label24 = new System.Windows.Forms.Label();
			this.listBoxCodeGroup = new OpenPmsDental.UI.ListBox();
			this.textNumber = new OpenPmsDental.ValidNum();
			this.butSave = new OpenPmsDental.UI.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.listBoxTimePeriod = new OpenPmsDental.UI.ListBox();
			this.listBoxTreatArea = new OpenPmsDental.UI.ListBox();
			this.label4 = new System.Windows.Forms.Label();
			this.checkPat = new OpenPmsDental.UI.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(346, 84);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(68, 23);
			this.label1.TabIndex = 198;
			this.label1.Text = "Frequency";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(31, 57);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(76, 23);
			this.label3.TabIndex = 196;
			this.label3.Text = "Code Group";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label24
			// 
			this.label24.Location = new System.Drawing.Point(354, 57);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(60, 23);
			this.label24.TabIndex = 197;
			this.label24.Text = "Quantity";
			this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// listBoxCodeGroup
			// 
			this.listBoxCodeGroup.Location = new System.Drawing.Point(113, 57);
			this.listBoxCodeGroup.Name = "listBoxCodeGroup";
			this.listBoxCodeGroup.Size = new System.Drawing.Size(173, 277);
			this.listBoxCodeGroup.TabIndex = 199;
			this.listBoxCodeGroup.TabStop = false;
			// 
			// textNumber
			// 
			this.textNumber.Location = new System.Drawing.Point(420, 57);
			this.textNumber.MinVal = 1;
			this.textNumber.Name = "textNumber";
			this.textNumber.Size = new System.Drawing.Size(40, 23);
			this.textNumber.TabIndex = 194;
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(610, 378);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(12, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(425, 47);
			this.label2.TabIndex = 200;
			this.label2.Text = "This is not normally used. Instead, edit frequencies directly in the grid. But th" +
    "is can be used if you need to add a second frequency for the same Code Group.";
			// 
			// listBoxTimePeriod
			// 
			this.listBoxTimePeriod.Location = new System.Drawing.Point(420, 84);
			this.listBoxTimePeriod.Name = "listBoxTimePeriod";
			this.listBoxTimePeriod.Size = new System.Drawing.Size(173, 61);
			this.listBoxTimePeriod.TabIndex = 201;
			this.listBoxTimePeriod.TabStop = false;
			// 
			// listBoxTreatArea
			// 
			this.listBoxTreatArea.Location = new System.Drawing.Point(420, 149);
			this.listBoxTreatArea.Name = "listBoxTreatArea";
			this.listBoxTreatArea.Size = new System.Drawing.Size(173, 173);
			this.listBoxTreatArea.TabIndex = 202;
			this.listBoxTreatArea.TabStop = false;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(322, 149);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(92, 23);
			this.label4.TabIndex = 203;
			this.label4.Text = "Treatment Area";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkPat
			// 
			this.checkPat.Location = new System.Drawing.Point(76, 338);
			this.checkPat.Name = "checkPat";
			this.checkPat.Size = new System.Drawing.Size(455, 20);
			this.checkPat.TabIndex = 204;
			this.checkPat.Text = "Patient Override  (Rare. Usually if percentages are different for family members)" +
    "";
			// 
			// FormBenefitFrequencyEdit
			// 
			this.ClientSize = new System.Drawing.Size(693, 410);
			this.Controls.Add(this.checkPat);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.listBoxTreatArea);
			this.Controls.Add(this.listBoxTimePeriod);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.listBoxCodeGroup);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textNumber);
			this.Controls.Add(this.label24);
			this.Controls.Add(this.butSave);
			this.Name = "FormBenefitFrequencyEdit";
			this.Text = "Edit Benefit Frequency";
			this.Load += new System.EventHandler(this.FormBenefitFrequencyEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label3;
		private ValidNum textNumber;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.Label label1;
		private UI.ListBox listBoxCodeGroup;
		private System.Windows.Forms.Label label2;
		private UI.ListBox listBoxTimePeriod;
		private UI.ListBox listBoxTreatArea;
		private System.Windows.Forms.Label label4;
		private UI.CheckBox checkPat;
	}
}