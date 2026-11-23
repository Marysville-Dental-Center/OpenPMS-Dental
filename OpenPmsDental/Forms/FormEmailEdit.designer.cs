namespace OpenPmsDental{
	partial class FormEmailEdit {
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
			this.textContentEmail = new OpenPmsDental.ODcodeBox();
			this.webViewEmail = new CodeBase.Controls.ODWebView2();
			this.butSave = new OpenPmsDental.UI.Button();
			this.toolBarBottom = new OpenPmsDental.UI.ToolBarOD();
			this.toolBarTop = new OpenPmsDental.UI.ToolBarOD();
			this.labelPreview = new System.Windows.Forms.Label();
			this.labelPlainText = new System.Windows.Forms.Label();
			this.menuAutographDropdown = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.checkIsRaw = new OpenPmsDental.UI.CheckBox();
			this.SuspendLayout();
			// 
			// textContentEmail
			// 
			this.textContentEmail.AcceptsTab = true;
			this.textContentEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.textContentEmail.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textContentEmail.Location = new System.Drawing.Point(8, 78);
			this.textContentEmail.Name = "textContentEmail";
			this.textContentEmail.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textContentEmail.Size = new System.Drawing.Size(603, 577);
			this.textContentEmail.TabIndex = 86;
			this.textContentEmail.Text = "";
			this.textContentEmail.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textContentEmail_KeyPress);
			this.textContentEmail.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.textContentEmail_MouseDoubleClick);
			// 
			// webViewEmail
			// 
			this.webViewEmail.AllowExternalDrop = false;
			this.webViewEmail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.webViewEmail.Location = new System.Drawing.Point(619, 78);
			this.webViewEmail.Name = "webViewEmail";
			this.webViewEmail.Size = new System.Drawing.Size(603, 577);
			this.webViewEmail.TabIndex = 86;
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(1147, 663);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 25);
			this.butSave.TabIndex = 87;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// toolBarBottom
			// 
			this.toolBarBottom.Dock = System.Windows.Forms.DockStyle.Top;
			this.toolBarBottom.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.toolBarBottom.Location = new System.Drawing.Point(0, 25);
			this.toolBarBottom.Name = "toolBarBottom";
			this.toolBarBottom.Size = new System.Drawing.Size(1230, 25);
			this.toolBarBottom.TabIndex = 83;
			this.toolBarBottom.ButtonClick += new OpenPmsDental.UI.ODToolBarButtonClickEventHandler(this.toolBarBottom_ButtonClick);
			// 
			// toolBarTop
			// 
			this.toolBarTop.Dock = System.Windows.Forms.DockStyle.Top;
			this.toolBarTop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.toolBarTop.Location = new System.Drawing.Point(0, 0);
			this.toolBarTop.Name = "toolBarTop";
			this.toolBarTop.Size = new System.Drawing.Size(1230, 25);
			this.toolBarTop.TabIndex = 4;
			this.toolBarTop.ButtonClick += new OpenPmsDental.UI.ODToolBarButtonClickEventHandler(this.toolBarTop_ButtonClick);
			// 
			// labelPreview
			// 
			this.labelPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.labelPreview.Location = new System.Drawing.Point(619, 58);
			this.labelPreview.Name = "labelPreview";
			this.labelPreview.Size = new System.Drawing.Size(63, 18);
			this.labelPreview.TabIndex = 89;
			this.labelPreview.Text = "Preview";
			this.labelPreview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelPlainText
			// 
			this.labelPlainText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.labelPlainText.Location = new System.Drawing.Point(8, 58);
			this.labelPlainText.Name = "labelPlainText";
			this.labelPlainText.Size = new System.Drawing.Size(163, 18);
			this.labelPlainText.TabIndex = 88;
			this.labelPlainText.Text = "Plain Text/HTML";
			this.labelPlainText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkIsRaw
			// 
			this.checkIsRaw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkIsRaw.Location = new System.Drawing.Point(8, 666);
			this.checkIsRaw.Name = "checkIsRaw";
			this.checkIsRaw.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkIsRaw.Size = new System.Drawing.Size(319, 18);
			this.checkIsRaw.TabIndex = 90;
			this.checkIsRaw.Text = "Use Raw HTML (don\'t use master template)";
			this.checkIsRaw.CheckedChanged += new System.EventHandler(this.checkIsRaw_CheckedChanged);
			// 
			// FormEmailEdit
			// 
			this.ClientSize = new System.Drawing.Size(1230, 696);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.checkIsRaw);
			this.Controls.Add(this.textContentEmail);
			this.Controls.Add(this.webViewEmail);
			this.Controls.Add(this.toolBarBottom);
			this.Controls.Add(this.toolBarTop);
			this.Controls.Add(this.labelPreview);
			this.Controls.Add(this.labelPlainText);
			this.Name = "FormEmailEdit";
			this.Text = "Email HTML Edit";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEmailEdit_FormClosing);
			this.Load += new System.EventHandler(this.FormEmailEdit_Load);
			this.SizeChanged += new System.EventHandler(this.FormEmailEdit_SizeChanged);
			this.ResumeLayout(false);

		}

		#endregion
		private UI.ToolBarOD toolBarTop;
		private UI.ToolBarOD toolBarBottom;
		private UI.Button butSave;
		private ODcodeBox textContentEmail;
		private CodeBase.Controls.ODWebView2 webViewEmail;
		private System.Windows.Forms.Label labelPreview;
		private System.Windows.Forms.Label labelPlainText;
		private System.Windows.Forms.ContextMenuStrip menuAutographDropdown;
		private OpenPmsDental.UI.CheckBox checkIsRaw;
	}
}