namespace OpenPmsDental{
	partial class FormEhrCarePlanEdit {
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
			this.textSnomedGoal = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textInstructions = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textDate = new System.Windows.Forms.TextBox();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.butSnomedGoalSelect = new OpenPmsDental.UI.Button();
			this.butSave = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			// 
			// textSnomedGoal
			// 
			this.textSnomedGoal.Location = new System.Drawing.Point(122, 35);
			this.textSnomedGoal.Name = "textSnomedGoal";
			this.textSnomedGoal.ReadOnly = true;
			this.textSnomedGoal.Size = new System.Drawing.Size(269, 23);
			this.textSnomedGoal.TabIndex = 25;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 35);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(108, 23);
			this.label1.TabIndex = 26;
			this.label1.Text = "SNOMED CT Goal";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textInstructions
			// 
			this.textInstructions.Location = new System.Drawing.Point(122, 62);
			this.textInstructions.Multiline = true;
			this.textInstructions.Name = "textInstructions";
			this.textInstructions.Size = new System.Drawing.Size(290, 227);
			this.textInstructions.TabIndex = 29;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(40, 62);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(76, 23);
			this.label2.TabIndex = 30;
			this.label2.Text = "Instructions";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(80, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(36, 23);
			this.label3.TabIndex = 32;
			this.label3.Text = "Date";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDate
			// 
			this.textDate.Location = new System.Drawing.Point(122, 8);
			this.textDate.Name = "textDate";
			this.textDate.Size = new System.Drawing.Size(96, 23);
			this.textDate.TabIndex = 33;
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 323);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 24);
			this.butDelete.TabIndex = 31;
			this.butDelete.Text = "&Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butSnomedGoalSelect
			// 
			this.butSnomedGoalSelect.Location = new System.Drawing.Point(397, 35);
			this.butSnomedGoalSelect.Name = "butSnomedGoalSelect";
			this.butSnomedGoalSelect.Size = new System.Drawing.Size(22, 23);
			this.butSnomedGoalSelect.TabIndex = 27;
			this.butSnomedGoalSelect.Text = "...";
			this.butSnomedGoalSelect.Click += new System.EventHandler(this.butSnomedGoalSelect_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(344, 323);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// FormEhrCarePlanEdit
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(427, 355);
			this.Controls.Add(this.textDate);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textInstructions);
			this.Controls.Add(this.textSnomedGoal);
			this.Controls.Add(this.butSnomedGoalSelect);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butSave);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "FormEhrCarePlanEdit";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Care Plan Edit";
			this.Load += new System.EventHandler(this.FormEhrCarePlanEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.TextBox textSnomedGoal;
		private UI.Button butSnomedGoalSelect;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textInstructions;
		private System.Windows.Forms.Label label2;
		private UI.Button butDelete;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textDate;
	}
}