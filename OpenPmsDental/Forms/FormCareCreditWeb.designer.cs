using CodeBase.Controls;

namespace OpenPmsDental{
	partial class FormCareCreditWeb {
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
			this.ToolBarMain = new OpenPmsDental.UI.ToolBarOD();
			this.webViewMain = new CodeBase.Controls.ODWebView2();
			((System.ComponentModel.ISupportInitialize)(this.webViewMain)).BeginInit();
			this.SuspendLayout();
			// 
			// ToolBarMain
			// 
			this.ToolBarMain.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToolBarMain.Location = new System.Drawing.Point(0, 0);
			this.ToolBarMain.Name = "ToolBarMain";
			this.ToolBarMain.Size = new System.Drawing.Size(974, 25);
			this.ToolBarMain.TabIndex = 32;
			this.ToolBarMain.ButtonClick += new OpenPmsDental.UI.ODToolBarButtonClickEventHandler(this.ToolBarMain_ButtonClick);
			// 
			// webViewMain
			// 
			this.webViewMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.webViewMain.CreationProperties = null;
			this.webViewMain.DefaultBackgroundColor = System.Drawing.Color.White;
			this.webViewMain.Location = new System.Drawing.Point(8, 33);
			this.webViewMain.Name = "webViewMain";
			this.webViewMain.Size = new System.Drawing.Size(974, 669);
			this.webViewMain.TabIndex = 33;
			this.webViewMain.ZoomFactor = 1D;
			this.webViewMain.NavigationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(this.webViewMain_NavigationCompleted);
			// 
			// FormCareCreditWeb2
			// 
			this.ClientSize = new System.Drawing.Size(990, 710);
			this.Controls.Add(this.webViewMain);
			this.Controls.Add(this.ToolBarMain);
			this.Cursor = System.Windows.Forms.Cursors.Default;
			this.Name = "FormCareCreditWeb2";
			this.Text = "Web Browser";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Load += new System.EventHandler(this.FormCareCreditWeb_Load);
			((System.ComponentModel.ISupportInitialize)(this.webViewMain)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private UI.ToolBarOD ToolBarMain;
		public ODWebView2 webViewMain;
	}
}