namespace OpenPmsDental {
	partial class FormClaimAttachPasteDXC {
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
			this.butSend = new OpenPmsDental.UI.Button();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textClaimStatus = new OpenPmsDental.ODtextBox();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.contextMenuImageGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemReferralForm = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDiagnosticReport = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemExplanationOfBenefits = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemOtherAttachments = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemPeriodontalCharts = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemXRays = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDentalModels = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemRadiologyReports = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemIntraOralPhotograph = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemNarrative = new System.Windows.Forms.ToolStripMenuItem();
			this.labelClaimAttachWarning = new System.Windows.Forms.Label();
			this.buttonPasteAgain = new OpenPmsDental.UI.Button();
			this.textNarrative = new OpenPmsDental.ODtextBox();
			this.labelNarrative = new System.Windows.Forms.Label();
			this.labelCharCount = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// butSend
			// 
			this.butSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSend.Location = new System.Drawing.Point(1094, 447);
			this.butSend.Name = "butSend";
			this.butSend.Size = new System.Drawing.Size(75, 24);
			this.butSend.TabIndex = 19;
			this.butSend.Text = "&Send";
			this.butSend.Click += new System.EventHandler(this.butSend_Click);
			// 
			// pictureBox
			// 
			this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox.Location = new System.Drawing.Point(393, 8);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(468, 469);
			this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox.TabIndex = 22;
			this.pictureBox.TabStop = false;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Location = new System.Drawing.Point(879, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(135, 16);
			this.label1.TabIndex = 48;
			this.label1.Text = "Claim Validation Status";
			// 
			// textClaimStatus
			// 
			this.textClaimStatus.AcceptsTab = true;
			this.textClaimStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textClaimStatus.BackColor = System.Drawing.SystemColors.Control;
			this.textClaimStatus.DetectLinksEnabled = false;
			this.textClaimStatus.DetectUrls = false;
			this.textClaimStatus.Location = new System.Drawing.Point(879, 26);
			this.textClaimStatus.Name = "textClaimStatus";
			this.textClaimStatus.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.Claim;
			this.textClaimStatus.ReadOnly = true;
			this.textClaimStatus.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textClaimStatus.Size = new System.Drawing.Size(290, 131);
			this.textClaimStatus.TabIndex = 47;
			this.textClaimStatus.Text = "";
			// 
			// gridMain
			// 
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.gridMain.ContextMenuStrip = this.contextMenuImageGrid;
			this.gridMain.DoAddGridItemsToContextMenuStrip = true;
			this.gridMain.Location = new System.Drawing.Point(12, 8);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(375, 469);
			this.gridMain.TabIndex = 49;
			this.gridMain.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			this.gridMain.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellClick);
			// 
			// contextMenuImageGrid
			// 
			this.contextMenuImageGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemReferralForm,
            this.menuItemDiagnosticReport,
            this.menuItemExplanationOfBenefits,
            this.menuItemOtherAttachments,
            this.menuItemPeriodontalCharts,
            this.menuItemXRays,
            this.menuItemDentalModels,
            this.menuItemRadiologyReports,
            this.menuItemIntraOralPhotograph,
            this.menuItemNarrative});
			this.contextMenuImageGrid.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuImageGrid_Popup);
			// 
			// menuItemReferralForm
			// 
			this.menuItemReferralForm.Text = "Referral Form";
			this.menuItemReferralForm.Click += new System.EventHandler(this.menuItemReferralForm_Click);
			// 
			// menuItemDiagnosticReport
			// 
			this.menuItemDiagnosticReport.Text = "Diagnostic Report";
			this.menuItemDiagnosticReport.Click += new System.EventHandler(this.menuItemDiagnosticReport_Click);
			// 
			// menuItemExplanationOfBenefits
			// 
			this.menuItemExplanationOfBenefits.Text = "Explanation of Benefits";
			this.menuItemExplanationOfBenefits.Click += new System.EventHandler(this.menuItemExplanationOfBenefits_Click);
			// 
			// menuItemOtherAttachments
			// 
			this.menuItemOtherAttachments.Text = "Other Attachments";
			this.menuItemOtherAttachments.Click += new System.EventHandler(this.menuItemOtherAttachments_Click);
			// 
			// menuItemPeriodontalCharts
			// 
			this.menuItemPeriodontalCharts.Text = "Periodontal Charts";
			this.menuItemPeriodontalCharts.Click += new System.EventHandler(this.menuItemPeriodontalCharts_Click);
			// 
			// menuItemXRays
			// 
			this.menuItemXRays.Text = "X-Rays";
			this.menuItemXRays.Click += new System.EventHandler(this.menuItemXRays_Click);
			// 
			// menuItemDentalModels
			// 
			this.menuItemDentalModels.Text = "Dental Models";
			this.menuItemDentalModels.Click += new System.EventHandler(this.menuItemDentalModels_Click);
			// 
			// menuItemRadiologyReports
			// 
			this.menuItemRadiologyReports.Text = "Radiology Reports";
			this.menuItemRadiologyReports.Click += new System.EventHandler(this.menuItemRadiologyReports_Click);
			// 
			// menuItemIntraOralPhotograph
			// 
			this.menuItemIntraOralPhotograph.Text = "Intra-Oral Photograph";
			this.menuItemIntraOralPhotograph.Click += new System.EventHandler(this.menuItemIntraOralPhotograph_Click);
			// 
			// menuItemNarrative
			// 
			this.menuItemNarrative.Text = "Narrative";
			this.menuItemNarrative.Click += new System.EventHandler(this.menuItemNarrative_Click);
			// 
			// labelClaimAttachWarning
			// 
			this.labelClaimAttachWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.labelClaimAttachWarning.ForeColor = System.Drawing.Color.DarkRed;
			this.labelClaimAttachWarning.Location = new System.Drawing.Point(1037, 340);
			this.labelClaimAttachWarning.Name = "labelClaimAttachWarning";
			this.labelClaimAttachWarning.Size = new System.Drawing.Size(135, 103);
			this.labelClaimAttachWarning.TabIndex = 50;
			this.labelClaimAttachWarning.Text = "No claim attachment image category definition found.  Images will be saved using " +
    "the first image category.";
			// 
			// buttonPasteAgain
			// 
			this.buttonPasteAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonPasteAgain.Location = new System.Drawing.Point(1085, 414);
			this.buttonPasteAgain.Name = "buttonPasteAgain";
			this.buttonPasteAgain.Size = new System.Drawing.Size(87, 24);
			this.buttonPasteAgain.TabIndex = 51;
			this.buttonPasteAgain.Text = "Paste &Again";
			this.buttonPasteAgain.Click += new System.EventHandler(this.buttonPasteAgain_Click);
			// 
			// textNarrative
			// 
			this.textNarrative.AcceptsTab = true;
			this.textNarrative.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textNarrative.BackColor = System.Drawing.SystemColors.Window;
			this.textNarrative.DetectLinksEnabled = false;
			this.textNarrative.DetectUrls = false;
			this.textNarrative.Location = new System.Drawing.Point(879, 183);
			this.textNarrative.MaxLength = 2000;
			this.textNarrative.Name = "textNarrative";
			this.textNarrative.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.Claim;
			this.textNarrative.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textNarrative.Size = new System.Drawing.Size(290, 153);
			this.textNarrative.TabIndex = 55;
			this.textNarrative.Text = "";
			this.textNarrative.TextChanged += new System.EventHandler(this.textNarrative_TextChanged);
			// 
			// labelNarrative
			// 
			this.labelNarrative.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelNarrative.Location = new System.Drawing.Point(879, 161);
			this.labelNarrative.Name = "labelNarrative";
			this.labelNarrative.Size = new System.Drawing.Size(88, 18);
			this.labelNarrative.TabIndex = 56;
			this.labelNarrative.Text = "Narrative";
			this.labelNarrative.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// labelCharCount
			// 
			this.labelCharCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelCharCount.Location = new System.Drawing.Point(1094, 161);
			this.labelCharCount.Name = "labelCharCount";
			this.labelCharCount.Size = new System.Drawing.Size(76, 18);
			this.labelCharCount.TabIndex = 59;
			this.labelCharCount.Text = "/2000";
			this.labelCharCount.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			// 
			// FormClaimAttachPasteDXC
			// 
			this.ClientSize = new System.Drawing.Size(1184, 498);
			this.Controls.Add(this.labelCharCount);
			this.Controls.Add(this.labelNarrative);
			this.Controls.Add(this.textNarrative);
			this.Controls.Add(this.buttonPasteAgain);
			this.Controls.Add(this.labelClaimAttachWarning);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textClaimStatus);
			this.Controls.Add(this.pictureBox);
			this.Controls.Add(this.butSend);
			this.Name = "FormClaimAttachPasteDXC";
			this.Text = "Paste Attachment";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormClaimAttachPasteDXC_FormClosing);
			this.Load += new System.EventHandler(this.FormClaimAttachPasteDXC_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private UI.Button butSend;
		private System.Windows.Forms.PictureBox pictureBox;
		private System.Windows.Forms.Label label1;
		private ODtextBox textClaimStatus;
		private UI.GridOD gridMain;
		private System.Windows.Forms.Label labelClaimAttachWarning;
		private System.Windows.Forms.ContextMenuStrip contextMenuImageGrid;
		private System.Windows.Forms.ToolStripMenuItem menuItemReferralForm;
		private System.Windows.Forms.ToolStripMenuItem menuItemDiagnosticReport;
		private System.Windows.Forms.ToolStripMenuItem menuItemExplanationOfBenefits;
		private System.Windows.Forms.ToolStripMenuItem menuItemOtherAttachments;
		private System.Windows.Forms.ToolStripMenuItem menuItemPeriodontalCharts;
		private System.Windows.Forms.ToolStripMenuItem menuItemXRays;
		private System.Windows.Forms.ToolStripMenuItem menuItemDentalModels;
		private System.Windows.Forms.ToolStripMenuItem menuItemRadiologyReports;
		private System.Windows.Forms.ToolStripMenuItem menuItemIntraOralPhotograph;
		private System.Windows.Forms.ToolStripMenuItem menuItemNarrative;
		private UI.Button buttonPasteAgain;
		private ODtextBox textNarrative;
		private System.Windows.Forms.Label labelNarrative;
		private System.Windows.Forms.Label labelCharCount;
	}
}