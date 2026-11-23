namespace OpenPmsDental {
	partial class FormEtrans835s {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing&&(components!=null)) {
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
			this.smsService1 = new OpenPmsDental.CallFireService.SMSService();
			this.listStatus = new OpenPmsDental.UI.ListBox();
			this.butRefresh = new OpenPmsDental.UI.Button();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.textRangeMin = new OpenPmsDental.ValidNum();
			this.textRangeMax = new OpenPmsDental.ValidNum();
			this.labelDaysOldMax = new System.Windows.Forms.Label();
			this.labelDaysOldMin = new System.Windows.Forms.Label();
			this.labelCheckTrace = new System.Windows.Forms.Label();
			this.labelCarrier = new System.Windows.Forms.Label();
			this.labelStatus = new System.Windows.Forms.Label();
			this.dateRangePicker = new OpenPmsDental.UI.ODDateRangePicker();
			this.labelControlId = new System.Windows.Forms.Label();
			this.comboClinics = new OpenPmsDental.UI.ComboBoxClinicPicker();
			this.textControlId = new OpenPmsDental.ODtextBox();
			this.textCarrier = new OpenPmsDental.ODtextBox();
			this.textCheckTrace = new OpenPmsDental.ODtextBox();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.checkAutomatableCarriersOnly = new OpenPmsDental.UI.CheckBox();
			this.checkShowFinalizedOnly = new OpenPmsDental.UI.CheckBox();
			this.butAutoProcessedEras = new OpenPmsDental.UI.Button();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// smsService1
			// 
			this.smsService1.Credentials = null;
			this.smsService1.Url = "https://www.callfire.com/service/SMSService";
			this.smsService1.UseDefaultCredentials = false;
			// 
			// listStatus
			// 
			this.listStatus.Location = new System.Drawing.Point(852, 8);
			this.listStatus.Name = "listStatus";
			this.listStatus.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listStatus.Size = new System.Drawing.Size(120, 56);
			this.listStatus.TabIndex = 106;
			this.listStatus.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listStatus_MouseUp);
			// 
			// butRefresh
			// 
			this.butRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butRefresh.Location = new System.Drawing.Point(951, 68);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(75, 24);
			this.butRefresh.TabIndex = 82;
			this.butRefresh.Text = "Refresh";
			this.butRefresh.UseVisualStyleBackColor = true;
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(253)))), ((int)(((byte)(254)))));
			this.groupBox2.Controls.Add(this.textRangeMin);
			this.groupBox2.Controls.Add(this.textRangeMax);
			this.groupBox2.Controls.Add(this.labelDaysOldMax);
			this.groupBox2.Controls.Add(this.labelDaysOldMin);
			this.groupBox2.Location = new System.Drawing.Point(8, 35);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(266, 54);
			this.groupBox2.TabIndex = 104;
			this.groupBox2.Text = "Amount Range (leave both blank to show all)";
			// 
			// textRangeMin
			// 
			this.textRangeMin.CausesValidation = false;
			this.textRangeMin.Location = new System.Drawing.Point(46, 23);
			this.textRangeMin.MaxVal = 50000;
			this.textRangeMin.Name = "textRangeMin";
			this.textRangeMin.Size = new System.Drawing.Size(60, 23);
			this.textRangeMin.TabIndex = 8;
			// 
			// textRangeMax
			// 
			this.textRangeMax.CausesValidation = false;
			this.textRangeMax.Location = new System.Drawing.Point(164, 23);
			this.textRangeMax.MaxVal = 50000;
			this.textRangeMax.Name = "textRangeMax";
			this.textRangeMax.Size = new System.Drawing.Size(60, 23);
			this.textRangeMax.TabIndex = 9;
			// 
			// labelDaysOldMax
			// 
			this.labelDaysOldMax.Location = new System.Drawing.Point(122, 23);
			this.labelDaysOldMax.Name = "labelDaysOldMax";
			this.labelDaysOldMax.Size = new System.Drawing.Size(36, 23);
			this.labelDaysOldMax.TabIndex = 99;
			this.labelDaysOldMax.Text = "Max";
			this.labelDaysOldMax.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDaysOldMin
			// 
			this.labelDaysOldMin.Location = new System.Drawing.Point(8, 23);
			this.labelDaysOldMin.Name = "labelDaysOldMin";
			this.labelDaysOldMin.Size = new System.Drawing.Size(32, 23);
			this.labelDaysOldMin.TabIndex = 100;
			this.labelDaysOldMin.Text = "Min";
			this.labelDaysOldMin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCheckTrace
			// 
			this.labelCheckTrace.Location = new System.Drawing.Point(491, 35);
			this.labelCheckTrace.Name = "labelCheckTrace";
			this.labelCheckTrace.Size = new System.Drawing.Size(128, 23);
			this.labelCheckTrace.TabIndex = 96;
			this.labelCheckTrace.Text = "Check# or EFT Trace#";
			this.labelCheckTrace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCarrier
			// 
			this.labelCarrier.Location = new System.Drawing.Point(491, 8);
			this.labelCarrier.Name = "labelCarrier";
			this.labelCarrier.Size = new System.Drawing.Size(128, 23);
			this.labelCarrier.TabIndex = 84;
			this.labelCarrier.Text = "Carrier";
			this.labelCarrier.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelStatus
			// 
			this.labelStatus.Location = new System.Drawing.Point(802, 8);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(44, 23);
			this.labelStatus.TabIndex = 86;
			this.labelStatus.Text = "Status";
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// dateRangePicker
			// 
			this.dateRangePicker.BackColor = System.Drawing.Color.Transparent;
			this.dateRangePicker.Location = new System.Drawing.Point(8, 8);
			this.dateRangePicker.MinimumSize = new System.Drawing.Size(453, 22);
			this.dateRangePicker.Name = "dateRangePicker";
			this.dateRangePicker.Size = new System.Drawing.Size(453, 24);
			this.dateRangePicker.TabIndex = 108;
			// 
			// labelControlId
			// 
			this.labelControlId.Location = new System.Drawing.Point(290, 62);
			this.labelControlId.Name = "labelControlId";
			this.labelControlId.Size = new System.Drawing.Size(68, 23);
			this.labelControlId.TabIndex = 109;
			this.labelControlId.Text = "Control ID";
			this.labelControlId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboClinics
			// 
			this.comboClinics.IncludeAll = true;
			this.comboClinics.IncludeUnassigned = true;
			this.comboClinics.Location = new System.Drawing.Point(577, 62);
			this.comboClinics.Name = "comboClinics";
			this.comboClinics.IsMultiSelect = true;
			this.comboClinics.Size = new System.Drawing.Size(209, 23);
			this.comboClinics.TabIndex = 111;
			// 
			// textControlId
			// 
			this.textControlId.AcceptsTab = true;
			this.textControlId.BackColor = System.Drawing.SystemColors.Window;
			this.textControlId.DetectLinksEnabled = false;
			this.textControlId.DetectUrls = false;
			this.textControlId.Location = new System.Drawing.Point(364, 62);
			this.textControlId.Name = "textControlId";
			this.textControlId.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.InsPlan;
			this.textControlId.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			this.textControlId.Size = new System.Drawing.Size(111, 23);
			this.textControlId.TabIndex = 110;
			this.textControlId.Text = "";
			// 
			// textCarrier
			// 
			this.textCarrier.AcceptsTab = true;
			this.textCarrier.BackColor = System.Drawing.SystemColors.Window;
			this.textCarrier.DetectLinksEnabled = false;
			this.textCarrier.DetectUrls = false;
			this.textCarrier.Location = new System.Drawing.Point(625, 8);
			this.textCarrier.Multiline = false;
			this.textCarrier.Name = "textCarrier";
			this.textCarrier.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.InsPlan;
			this.textCarrier.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textCarrier.Size = new System.Drawing.Size(161, 23);
			this.textCarrier.TabIndex = 105;
			this.textCarrier.Text = "";
			// 
			// textCheckTrace
			// 
			this.textCheckTrace.AcceptsTab = true;
			this.textCheckTrace.BackColor = System.Drawing.SystemColors.Window;
			this.textCheckTrace.DetectLinksEnabled = false;
			this.textCheckTrace.DetectUrls = false;
			this.textCheckTrace.Location = new System.Drawing.Point(625, 35);
			this.textCheckTrace.Multiline = false;
			this.textCheckTrace.Name = "textCheckTrace";
			this.textCheckTrace.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.FinancialNotes;
			this.textCheckTrace.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textCheckTrace.Size = new System.Drawing.Size(161, 23);
			this.textCheckTrace.TabIndex = 10;
			this.textCheckTrace.Text = "";
			// 
			// gridMain
			// 
			this.gridMain.AllowSortingByColumn = true;
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.Location = new System.Drawing.Point(8, 100);
			this.gridMain.Name = "gridMain";
			this.gridMain.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridMain.Size = new System.Drawing.Size(1018, 556);
			this.gridMain.TabIndex = 8;
			this.gridMain.Title = "ERAs";
			this.gridMain.TranslationName = "TableEtrans835s";
			this.gridMain.DoubleClick += new System.EventHandler(this.gridMain_DoubleClick);
			// 
			// checkAutomatableCarriersOnly
			// 
			this.checkAutomatableCarriersOnly.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAutomatableCarriersOnly.Location = new System.Drawing.Point(802, 68);
			this.checkAutomatableCarriersOnly.Name = "checkAutomatableCarriersOnly";
			this.checkAutomatableCarriersOnly.Size = new System.Drawing.Size(167, 20);
			this.checkAutomatableCarriersOnly.TabIndex = 270;
			this.checkAutomatableCarriersOnly.Text = "Automatable Carriers Only";
			// 
			// checkShowFinalizedOnly
			// 
			this.checkShowFinalizedOnly.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkShowFinalizedOnly.Location = new System.Drawing.Point(802, 68);
			this.checkShowFinalizedOnly.Name = "checkShowFinalizedOnly";
			this.checkShowFinalizedOnly.Size = new System.Drawing.Size(167, 20);
			this.checkShowFinalizedOnly.TabIndex = 271;
			this.checkShowFinalizedOnly.Text = "Finalized Only";
			// 
			// butAutoProcessedEras
			// 
			this.butAutoProcessedEras.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butAutoProcessedEras.Location = new System.Drawing.Point(8, 664);
			this.butAutoProcessedEras.Name = "butAutoProcessedEras";
			this.butAutoProcessedEras.Size = new System.Drawing.Size(149, 24);
			this.butAutoProcessedEras.TabIndex = 272;
			this.butAutoProcessedEras.Text = "Auto Processed Report";
			this.butAutoProcessedEras.UseVisualStyleBackColor = true;
			this.butAutoProcessedEras.Click += new System.EventHandler(this.butAutoProcessedEras_Click);
			// 
			// FormEtrans835s
			// 
			this.ClientSize = new System.Drawing.Size(1034, 696);
			this.Controls.Add(this.butAutoProcessedEras);
			this.Controls.Add(this.checkShowFinalizedOnly);
			this.Controls.Add(this.checkAutomatableCarriersOnly);
			this.Controls.Add(this.comboClinics);
			this.Controls.Add(this.textControlId);
			this.Controls.Add(this.labelControlId);
			this.Controls.Add(this.dateRangePicker);
			this.Controls.Add(this.listStatus);
			this.Controls.Add(this.textCarrier);
			this.Controls.Add(this.butRefresh);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.textCheckTrace);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.labelCheckTrace);
			this.Controls.Add(this.labelCarrier);
			this.Controls.Add(this.labelStatus);
			this.Name = "FormEtrans835s";
			this.Text = "Electronic EOBs - ERA 835s";
			this.Load += new System.EventHandler(this.FormEtrans835s_Load);
			this.Shown += new System.EventHandler(this.FormEtrans835s_Shown);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion
		private UI.Button butRefresh;
		private System.Windows.Forms.Label labelCheckTrace;
		private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.Label labelCarrier;
		private ODtextBox textCheckTrace;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private ValidNum textRangeMin;
		private ValidNum textRangeMax;
		private System.Windows.Forms.Label labelDaysOldMax;
		private System.Windows.Forms.Label labelDaysOldMin;
		private ODtextBox textCarrier;
		private CallFireService.SMSService smsService1;
		private OpenPmsDental.UI.ListBox listStatus;
		private UI.GridOD gridMain;
		private UI.ODDateRangePicker dateRangePicker;
		private ODtextBox textControlId;
		private System.Windows.Forms.Label labelControlId;
		private UI.ComboBoxClinicPicker comboClinics;
		private OpenPmsDental.UI.CheckBox checkAutomatableCarriersOnly;
		private OpenPmsDental.UI.CheckBox checkShowFinalizedOnly;
		private UI.Button butAutoProcessedEras;
	}
}