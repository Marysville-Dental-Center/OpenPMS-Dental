namespace OpenPmsDental{
	partial class FormImageCatMerge {
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
			this.butMerge = new OpenPmsDental.UI.Button();
			this.groupBoxInto = new OpenPmsDental.UI.GroupBox();
			this.buttonChangeInto = new OpenPmsDental.UI.Button();
			this.textBoxInto = new System.Windows.Forms.TextBox();
			this.labelInto = new System.Windows.Forms.Label();
			this.groupBoxFrom = new OpenPmsDental.UI.GroupBox();
			this.buttonChangeFrom = new OpenPmsDental.UI.Button();
			this.textBoxFrom = new System.Windows.Forms.TextBox();
			this.labelFrom = new System.Windows.Forms.Label();
			this.groupBoxInto.SuspendLayout();
			this.groupBoxFrom.SuspendLayout();
			this.SuspendLayout();
			// 
			// butMerge
			// 
			this.butMerge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butMerge.Enabled = false;
			this.butMerge.Location = new System.Drawing.Point(517,217);
			this.butMerge.Name = "butMerge";
			this.butMerge.Size = new System.Drawing.Size(75, 24);
			this.butMerge.TabIndex = 3;
			this.butMerge.Text = "&Merge";
			this.butMerge.Click += new System.EventHandler(this.butMerge_Click);
			// 
			// groupBoxInto
			// 
			this.groupBoxInto.Controls.Add(this.buttonChangeInto);
			this.groupBoxInto.Controls.Add(this.textBoxInto);
			this.groupBoxInto.Controls.Add(this.labelInto);
			this.groupBoxInto.Location = new System.Drawing.Point(8, 8);
			this.groupBoxInto.Name = "groupBoxInto";
			this.groupBoxInto.Size = new System.Drawing.Size(584, 76);
			this.groupBoxInto.TabIndex = 4;
			this.groupBoxInto.TabStop = false;
			this.groupBoxInto.Text = "Image Category to merge into. The Image Category below will be merged into this I" +
    "mage Category.";
			// 
			// buttonChangeInto
			// 
			this.buttonChangeInto.Location = new System.Drawing.Point(501, 35);
			this.buttonChangeInto.Name = "buttonChangeInto";
			this.buttonChangeInto.Size = new System.Drawing.Size(75, 24);
			this.buttonChangeInto.TabIndex = 2;
			this.buttonChangeInto.Text = "Change";
			this.buttonChangeInto.UseVisualStyleBackColor = true;
			this.buttonChangeInto.Click += new System.EventHandler(this.buttonChangeInto_Click);
			// 
			// textBoxInto
			// 
			this.textBoxInto.Location = new System.Drawing.Point(8, 45);
			this.textBoxInto.Name = "textBoxInto";
			this.textBoxInto.ReadOnly = true;
			this.textBoxInto.Size = new System.Drawing.Size(236, 23);
			this.textBoxInto.TabIndex = 1;
			// 
			// labelInto
			// 
			this.labelInto.AutoSize = false;
			this.labelInto.Location = new System.Drawing.Point(8, 23);
			this.labelInto.Name = "labelInto";
			this.labelInto.Size = new System.Drawing.Size(132, 20);
			this.labelInto.TabIndex = 0;
			this.labelInto.Text = "Image Category Name";
			// 
			// groupBoxFrom
			// 
			this.groupBoxFrom.Controls.Add(this.buttonChangeFrom);
			this.groupBoxFrom.Controls.Add(this.textBoxFrom);
			this.groupBoxFrom.Controls.Add(this.labelFrom);
			this.groupBoxFrom.Location = new System.Drawing.Point(8, 112);
			this.groupBoxFrom.Name = "groupBoxFrom";
			this.groupBoxFrom.Size = new System.Drawing.Size(584, 76);
			this.groupBoxFrom.TabIndex = 5;
			this.groupBoxFrom.TabStop = false;
			this.groupBoxFrom.Text = "Image Category to merge from. This Image Category will be merged into the Image C" +
    "ategory above.";
			// 
			// buttonChangeFrom
			// 
			this.buttonChangeFrom.Location = new System.Drawing.Point(501, 35);
			this.buttonChangeFrom.Name = "buttonChangeFrom";
			this.buttonChangeFrom.Size = new System.Drawing.Size(75, 24);
			this.buttonChangeFrom.TabIndex = 3;
			this.buttonChangeFrom.Text = "Change";
			this.buttonChangeFrom.UseVisualStyleBackColor = true;
			this.buttonChangeFrom.Click += new System.EventHandler(this.buttonChangeFrom_Click);
			// 
			// textBoxFrom
			// 
			this.textBoxFrom.Location = new System.Drawing.Point(8, 45);
			this.textBoxFrom.Name = "textBoxFrom";
			this.textBoxFrom.ReadOnly = true;
			this.textBoxFrom.Size = new System.Drawing.Size(236, 23);
			this.textBoxFrom.TabIndex = 2;
			// 
			// labelFrom
			// 
			this.labelFrom.AutoSize = false;
			this.labelFrom.Location = new System.Drawing.Point(8, 23);
			this.labelFrom.Name = "labelFrom";
			this.labelFrom.Size = new System.Drawing.Size(132, 20);
			this.labelFrom.TabIndex = 2;
			this.labelFrom.Text = "Image Category Name";
			// 
			// FormImageCatMerge
			// 
			this.ClientSize = new System.Drawing.Size(600, 249);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.groupBoxFrom);
			this.Controls.Add(this.groupBoxInto);
			this.Controls.Add(this.butMerge);
			this.Name = "FormImageCatMerge";
			this.Text = "Merge Image Categories";
			this.groupBoxInto.ResumeLayout(false);
			this.groupBoxInto.PerformLayout();
			this.groupBoxFrom.ResumeLayout(false);
			this.groupBoxFrom.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private OpenPmsDental.UI.Button butMerge;
		private OpenPmsDental.UI.GroupBox groupBoxInto;
		private UI.Button buttonChangeInto;
		private System.Windows.Forms.TextBox textBoxInto;
		private System.Windows.Forms.Label labelInto;
		private OpenPmsDental.UI.GroupBox groupBoxFrom;
		private UI.Button buttonChangeFrom;
		private System.Windows.Forms.TextBox textBoxFrom;
		private System.Windows.Forms.Label labelFrom;
	}
}