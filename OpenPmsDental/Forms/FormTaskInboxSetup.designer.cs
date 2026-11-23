namespace OpenPmsDental{
	partial class FormTaskInboxSetup {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTaskInboxSetup));
			this.label1 = new System.Windows.Forms.Label();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.butSave = new OpenPmsDental.UI.Button();
			this.listMain = new OpenPmsDental.UI.ListBox();
			this.butSet = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(655, 114);
			this.label1.TabIndex = 4;
			this.label1.Text = resources.GetString("label1.Text");
			// 
			// gridMain
			// 
			this.gridMain.Location = new System.Drawing.Point(8, 130);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(444, 369);
			this.gridMain.TabIndex = 5;
			this.gridMain.Title = null;
			this.gridMain.TranslationName = "TableInboxAssignments";
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(588, 507);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// listMain
			// 
			this.listMain.Location = new System.Drawing.Point(543, 130);
			this.listMain.Name = "listMain";
			this.listMain.Size = new System.Drawing.Size(120, 369);
			this.listMain.TabIndex = 6;
			// 
			// butSet
			// 
			this.butSet.Icon = OpenPmsDental.UI.EnumIcons.ArrowLeft;
			this.butSet.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butSet.Location = new System.Drawing.Point(460, 130);
			this.butSet.Name = "butSet";
			this.butSet.Size = new System.Drawing.Size(75, 24);
			this.butSet.TabIndex = 7;
			this.butSet.Text = "Set";
			this.butSet.Click += new System.EventHandler(this.butSet_Click);
			// 
			// FormTaskInboxSetup
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(671, 539);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.butSet);
			this.Controls.Add(this.listMain);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.label1);
			this.Name = "FormTaskInboxSetup";
			this.Text = "Task Inbox Setup";
			this.Load += new System.EventHandler(this.FormTaskInboxSetup_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label1;
		private OpenPmsDental.UI.GridOD gridMain;
		private OpenPmsDental.UI.ListBox listMain;
		private OpenPmsDental.UI.Button butSet;
	}
}