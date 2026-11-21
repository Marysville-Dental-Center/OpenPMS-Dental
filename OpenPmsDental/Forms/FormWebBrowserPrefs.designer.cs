namespace OpenPmsDental{
	partial class FormWebBrowserPrefs {
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
			this.webView = new CodeBase.Controls.ODWebView2();
			((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
			this.SuspendLayout();
			// 
			// webView
			// 
			this.webView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.webView.CreationProperties = null;
			this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
			this.webView.Location = new System.Drawing.Point(12, 12);
			this.webView.Name = "webView";
			this.webView.Size = new System.Drawing.Size(360, 157);
			this.webView.TabIndex = 4;
			this.webView.Visible = false;
			this.webView.ZoomFactor = 1D;
			//
			// FormWebBrowserPrefs
			//
			this.ClientSize = new System.Drawing.Size(384, 211);
			this.Controls.Add(this.webView);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.Name = "FormWebBrowserPrefs";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "";
			this.Load += new System.EventHandler(this.FormWebBrowserPrefs_Load);
			((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private CodeBase.Controls.ODWebView2 webView;
	}
}