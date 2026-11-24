namespace OpenPmsDental{
	partial class FormEhrLabImageEdit {
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
			this.picturePreview = new System.Windows.Forms.PictureBox();
			this.checkWaitingForImages = new OpenPmsDental.UI.CheckBox();
			this.splitContainer = new OpenPmsDental.UI.SplitContainer();
			this.splitterPanel1 = new OpenPmsDental.UI.SplitterPanel();
			this.splitterPanel2 = new OpenPmsDental.UI.SplitterPanel();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.butSave = new OpenPmsDental.UI.Button();
			((System.ComponentModel.ISupportInitialize)(this.picturePreview)).BeginInit();
			this.splitContainer.SuspendLayout();
			this.splitterPanel1.SuspendLayout();
			this.splitterPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// picturePreview
			// 
			this.picturePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.picturePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.picturePreview.Location = new System.Drawing.Point(8, 8);
			this.picturePreview.Name = "picturePreview";
			this.picturePreview.Size = new System.Drawing.Size(759, 310);
			this.picturePreview.TabIndex = 19;
			this.picturePreview.TabStop = false;
			// 
			// checkWaitingForImages
			// 
			this.checkWaitingForImages.Location = new System.Drawing.Point(8, 8);
			this.checkWaitingForImages.Name = "checkWaitingForImages";
			this.checkWaitingForImages.Size = new System.Drawing.Size(476, 20);
			this.checkWaitingForImages.TabIndex = 266;
			this.checkWaitingForImages.Text = "Waiting For Images. This status will only be applied if there are no images attac" +
    "hed.";
			// 
			// splitContainer
			// 
			this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer.Controls.Add(this.splitterPanel1);
			this.splitContainer.Controls.Add(this.splitterPanel2);
			this.splitContainer.Cursor = System.Windows.Forms.Cursors.Default;
			this.splitContainer.Location = new System.Drawing.Point(8, 32);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.splitContainer.Panel1 = this.splitterPanel1;
			this.splitContainer.Panel2 = this.splitterPanel2;
			this.splitContainer.Size = new System.Drawing.Size(791, 549);
			this.splitContainer.SplitterDistance = 193;
			this.splitContainer.TabIndex = 268;
			this.splitContainer.SplitterMoved += new System.EventHandler(this.splitContainer_SplitterMoved);
			this.splitContainer.Resize += new System.EventHandler(this.splitContainer_Resize);
			// 
			// splitterPanel1
			// 
			this.splitterPanel1.Controls.Add(this.gridMain);
			this.splitterPanel1.Location = new System.Drawing.Point(8, 8);
			this.splitterPanel1.Name = "splitterPanel1";
			this.splitterPanel1.Size = new System.Drawing.Size(775, 203);
			this.splitterPanel1.TabIndex = 13;
			// 
			// splitterPanel2
			// 
			this.splitterPanel2.Controls.Add(this.picturePreview);
			this.splitterPanel2.Location = new System.Drawing.Point(8, 215);
			this.splitterPanel2.Name = "splitterPanel2";
			this.splitterPanel2.Size = new System.Drawing.Size(775, 326);
			this.splitterPanel2.TabIndex = 14;
			// 
			// gridMain
			// 
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.Location = new System.Drawing.Point(8, 8);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(759, 187);
			this.gridMain.TabIndex = 18;
			this.gridMain.Title = "Available Images";
			this.gridMain.TranslationName = "TableWikiSearchPages";
			this.gridMain.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellClick);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(724, 593);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 3;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// FormEhrLabImageEdit
			// 
			this.ClientSize = new System.Drawing.Size(807, 625);
			this.Controls.Add(this.splitContainer);
			this.Controls.Add(this.checkWaitingForImages);
			this.Controls.Add(this.butSave);
			this.Name = "FormEhrLabImageEdit";
			this.Text = "Lab Order Image Edit";
			this.Load += new System.EventHandler(this.FormEhrLabeImageEdit_Load);
			((System.ComponentModel.ISupportInitialize)(this.picturePreview)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitterPanel1.ResumeLayout(false);
			this.splitterPanel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.PictureBox picturePreview;
		private UI.GridOD gridMain;
		private OpenPmsDental.UI.CheckBox checkWaitingForImages;
		private UI.SplitContainer splitContainer;
		private UI.SplitterPanel splitterPanel1;
		private UI.SplitterPanel splitterPanel2;
	}
}