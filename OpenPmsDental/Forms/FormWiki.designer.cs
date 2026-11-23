namespace OpenPmsDental{
	partial class FormWiki {
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
			this.components = new System.ComponentModel.Container();
			this.labelStatus = new System.Windows.Forms.Label();
			this.ToolBarMain = new OpenPmsDental.UI.ToolBarOD();
			this.webViewWiki = new CodeBase.Controls.ODWebView2();
			this.menuHomeDropDown = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemHomePageSave = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.webViewWiki)).BeginInit();
			this.SuspendLayout();
			// 
			// labelStatus
			// 
			this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelStatus.Location = new System.Drawing.Point(-3, 702);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(947, 18);
			this.labelStatus.TabIndex = 73;
			this.labelStatus.Text = "Status Bar";
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// ToolBarMain
			// 
			this.ToolBarMain.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToolBarMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.ToolBarMain.Location = new System.Drawing.Point(0, 0);
			this.ToolBarMain.Name = "ToolBarMain";
			this.ToolBarMain.Size = new System.Drawing.Size(944, 25);
			this.ToolBarMain.TabIndex = 72;
			this.ToolBarMain.ButtonClick += new OpenPmsDental.UI.ODToolBarButtonClickEventHandler(this.ToolBarMain_ButtonClick);
			//
			// webViewWiki
			//
			this.webViewWiki.AllowExternalDrop = false;
			this.webViewWiki.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.webViewWiki.AreBrowserKeysEnabled = true;
			this.webViewWiki.CreationProperties = null;
			this.webViewWiki.DefaultBackgroundColor = System.Drawing.Color.White;
			this.webViewWiki.Location = new System.Drawing.Point(8, 33);
			this.webViewWiki.Name = "webViewWiki";
			this.webViewWiki.Size = new System.Drawing.Size(944, 657);
			this.webViewWiki.TabIndex = 0;
			this.webViewWiki.ZoomFactor = 1D;
			this.webViewWiki.LinkClicked += new System.EventHandler<CodeBase.Controls.ODWebView2LinkEventArgs>(this.webViewWiki_LinkClicked);
			this.webViewWiki.StatusTextChanged += new System.EventHandler(this.webViewWiki_StatusTextChanged);
			//
			// menuHomeDropDown
			// 
			this.menuHomeDropDown.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemHomePageSave});
			// 
			// menuItemHomePageSave
			// 
			this.menuItemHomePageSave.Text = "Save As Home Page";
			this.menuItemHomePageSave.Click += new System.EventHandler(this.menuItemHomePageSave_Click);
			// 
			// FormWiki
			// 
			this.ClientSize = new System.Drawing.Size(960, 728);
			this.Controls.Add(this.labelStatus);
			this.Controls.Add(this.ToolBarMain);
			this.Controls.Add(this.webViewWiki);
			this.Name = "FormWiki";
			this.Text = "Wiki";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormWiki_FormClosing);
			this.Load += new System.EventHandler(this.FormWiki_Load);
			this.ResizeEnd += new System.EventHandler(this.FormWiki_ResizeEnd);
			((System.ComponentModel.ISupportInitialize)(this.webViewWiki)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CodeBase.Controls.ODWebView2 webViewWiki;
		private UI.ToolBarOD ToolBarMain;
		private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.ContextMenuStrip menuHomeDropDown;
		private System.Windows.Forms.ToolStripMenuItem menuItemHomePageSave;
	}
}