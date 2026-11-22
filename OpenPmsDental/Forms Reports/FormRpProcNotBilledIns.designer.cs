namespace OpenPmsDental {
	partial class FormRpProcNotBilledIns {
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
			this.butPrint = new OpenPmsDental.UI.Button();
			this.checkMedical = new OpenPmsDental.UI.CheckBox();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.butNewClaims = new OpenPmsDental.UI.Button();
			this.contextMenuGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemGoToAccount = new System.Windows.Forms.ToolStripMenuItem();
			this.checkAutoGroupProcs = new OpenPmsDental.UI.CheckBox();
			this.butRefresh = new OpenPmsDental.UI.Button();
			this.butSelectAll = new OpenPmsDental.UI.Button();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.listBoxShowProcsBeforeIns = new OpenPmsDental.UI.ListBox();
			this.checkExcludeProcCodes = new OpenPmsDental.UI.CheckBox();
			this.checkOnlyProcCodes = new OpenPmsDental.UI.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textProcedureCodes = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.comboClinics = new OpenPmsDental.UI.ComboBoxClinicPicker();
			this.checkShowProcsInProcess = new OpenPmsDental.UI.CheckBox();
			this.dateRangePicker = new OpenPmsDental.UI.ODDateRangePicker();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// butPrint
			// 
			this.butPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPrint.Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons.Print,16,16);
			this.butPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPrint.Location = new System.Drawing.Point(8, 664);
			this.butPrint.Name = "butPrint";
			this.butPrint.Size = new System.Drawing.Size(75, 24);
			this.butPrint.TabIndex = 3;
			this.butPrint.Text = "Print";
			this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
			// 
			// checkMedical
			// 
			this.checkMedical.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkMedical.Location = new System.Drawing.Point(767, 67);
			this.checkMedical.Name = "checkMedical";
			this.checkMedical.Size = new System.Drawing.Size(173, 20);
			this.checkMedical.TabIndex = 11;
			this.checkMedical.Text = "Include Medical Procedures";
			this.checkMedical.Visible = false;
			// 
			// gridMain
			// 
			this.gridMain.AllowSortingByColumn = true;
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.Location = new System.Drawing.Point(8, 143);
			this.gridMain.Name = "gridMain";
			this.gridMain.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridMain.Size = new System.Drawing.Size(1003, 513);
			this.gridMain.TabIndex = 69;
			this.gridMain.Title = "Procedures Not Billed";
			this.gridMain.TranslationName = "TableProcedures";
			// 
			// butNewClaims
			// 
			this.butNewClaims.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butNewClaims.Location = new System.Drawing.Point(513, 664);
			this.butNewClaims.Name = "butNewClaims";
			this.butNewClaims.Size = new System.Drawing.Size(94, 24);
			this.butNewClaims.TabIndex = 71;
			this.butNewClaims.Text = "Create Claims";
			this.butNewClaims.UseVisualStyleBackColor = true;
			this.butNewClaims.Click += new System.EventHandler(this.butNewClaims_Click);
			// 
			// contextMenuGrid
			// 
			this.contextMenuGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemGoToAccount});
			// 
			// menuItemGoToAccount
			// 
			this.menuItemGoToAccount.Text = "Go To Account";
			this.menuItemGoToAccount.Click += new System.EventHandler(this.menuItemGridGoToAccount_Click);
			// 
			// checkAutoGroupProcs
			// 
			this.checkAutoGroupProcs.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkAutoGroupProcs.Location = new System.Drawing.Point(767, 23);
			this.checkAutoGroupProcs.Name = "checkAutoGroupProcs";
			this.checkAutoGroupProcs.Size = new System.Drawing.Size(199, 20);
			this.checkAutoGroupProcs.TabIndex = 72;
			this.checkAutoGroupProcs.Text = "Automatically Group Procedures";
			// 
			// butRefresh
			// 
			this.butRefresh.Location = new System.Drawing.Point(920, 95);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(75, 24);
			this.butRefresh.TabIndex = 73;
			this.butRefresh.Text = "Refresh";
			this.butRefresh.UseVisualStyleBackColor = true;
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// butSelectAll
			// 
			this.butSelectAll.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butSelectAll.Location = new System.Drawing.Point(432, 664);
			this.butSelectAll.Name = "butSelectAll";
			this.butSelectAll.Size = new System.Drawing.Size(75, 24);
			this.butSelectAll.TabIndex = 74;
			this.butSelectAll.Text = "Select All";
			this.butSelectAll.UseVisualStyleBackColor = true;
			this.butSelectAll.Click += new System.EventHandler(this.butSelectAll_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.listBoxShowProcsBeforeIns);
			this.groupBox2.Controls.Add(this.checkExcludeProcCodes);
			this.groupBox2.Controls.Add(this.checkOnlyProcCodes);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.butRefresh);
			this.groupBox2.Controls.Add(this.textProcedureCodes);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.comboClinics);
			this.groupBox2.Controls.Add(this.checkShowProcsInProcess);
			this.groupBox2.Controls.Add(this.dateRangePicker);
			this.groupBox2.Controls.Add(this.checkMedical);
			this.groupBox2.Controls.Add(this.checkAutoGroupProcs);
			this.groupBox2.Location = new System.Drawing.Point(8, 8);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(1003, 127);
			this.groupBox2.TabIndex = 77;
			this.groupBox2.Text = "Filters";
			// 
			// listBoxShowProcsBeforeIns
			// 
			this.listBoxShowProcsBeforeIns.Location = new System.Drawing.Point(73, 23);
			this.listBoxShowProcsBeforeIns.Name = "listBoxShowProcsBeforeIns";
			this.listBoxShowProcsBeforeIns.Size = new System.Drawing.Size(388, 58);
			this.listBoxShowProcsBeforeIns.TabIndex = 83;
			this.listBoxShowProcsBeforeIns.TabStop = false;
			// 
			// checkExcludeProcCodes
			// 
			this.checkExcludeProcCodes.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkExcludeProcCodes.Location = new System.Drawing.Point(587, 99);
			this.checkExcludeProcCodes.Name = "checkExcludeProcCodes";
			this.checkExcludeProcCodes.Size = new System.Drawing.Size(64, 20);
			this.checkExcludeProcCodes.TabIndex = 79;
			this.checkExcludeProcCodes.Text = "Exclude";
			this.checkExcludeProcCodes.Click += new System.EventHandler(this.checkExclude_Click);
			// 
			// checkOnlyProcCodes
			// 
			this.checkOnlyProcCodes.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkOnlyProcCodes.Location = new System.Drawing.Point(587, 77);
			this.checkOnlyProcCodes.Name = "checkOnlyProcCodes";
			this.checkOnlyProcCodes.Size = new System.Drawing.Size(48, 20);
			this.checkOnlyProcCodes.TabIndex = 78;
			this.checkOnlyProcCodes.Text = "Only";
			this.checkOnlyProcCodes.Click += new System.EventHandler(this.checkOnly_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(667, 77);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 36);
			this.label2.TabIndex = 82;
			this.label2.Text = "separate multiple by commas";
			// 
			// textProcedureCodes
			// 
			this.textProcedureCodes.AutoSize = false;
			this.textProcedureCodes.Location = new System.Drawing.Point(587, 50);
			this.textProcedureCodes.Name = "textProcedureCodes";
			this.textProcedureCodes.Size = new System.Drawing.Size(164, 23);
			this.textProcedureCodes.TabIndex = 81;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(477, 50);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 23);
			this.label1.TabIndex = 80;
			this.label1.Text = "Procedure Codes";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboClinics
			// 
			this.comboClinics.IncludeAll = true;
			this.comboClinics.IncludeHiddenInAll = true;
			this.comboClinics.IncludeUnassigned = true;
			this.comboClinics.Location = new System.Drawing.Point(550, 23);
			this.comboClinics.Name = "comboClinics";
			this.comboClinics.IsMultiSelect = true;
			this.comboClinics.Size = new System.Drawing.Size(201, 23);
			this.comboClinics.TabIndex = 77;
			// 
			// checkShowProcsInProcess
			// 
			this.checkShowProcsInProcess.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkShowProcsInProcess.Location = new System.Drawing.Point(767, 45);
			this.checkShowProcsInProcess.Name = "checkShowProcsInProcess";
			this.checkShowProcsInProcess.Size = new System.Drawing.Size(175, 20);
			this.checkShowProcsInProcess.TabIndex = 76;
			this.checkShowProcsInProcess.Text = "Show Procedures In Process";
			// 
			// dateRangePicker
			// 
			this.dateRangePicker.BackColor = System.Drawing.Color.Transparent;
			this.dateRangePicker.EnableWeekButtons = false;
			this.dateRangePicker.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.dateRangePicker.Location = new System.Drawing.Point(8, 85);
			this.dateRangePicker.MaximumSize = new System.Drawing.Size(0, 185);
			this.dateRangePicker.MinimumSize = new System.Drawing.Size(453, 22);
			this.dateRangePicker.Name = "dateRangePicker";
			this.dateRangePicker.Size = new System.Drawing.Size(453, 22);
			this.dateRangePicker.TabIndex = 0;
			// 
			// FormRpProcNotBilledIns
			// 
			this.AcceptButton = this.butPrint;
			this.ClientSize = new System.Drawing.Size(1019, 696);
			this.Controls.Add(this.butSelectAll);
			this.Controls.Add(this.butNewClaims);
			this.Controls.Add(this.butPrint);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.groupBox2);
			this.Name = "FormRpProcNotBilledIns";
			this.Text = "Procedures Not Billed to Insurance";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormRpProcNotBilledIns_FormClosing);
			this.Load += new System.EventHandler(this.FormProcNotAttach_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion
		private OpenPmsDental.UI.Button butPrint;
		private UI.Button butNewClaims;
		private OpenPmsDental.UI.CheckBox checkMedical;
		private System.Windows.Forms.ContextMenuStrip contextMenuGrid;
		private System.Windows.Forms.ToolStripMenuItem menuItemGoToAccount;
		private UI.GridOD gridMain;
		private OpenPmsDental.UI.CheckBox checkAutoGroupProcs;
		private UI.Button butRefresh;
		private UI.Button butSelectAll;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private UI.ODDateRangePicker dateRangePicker;
		private OpenPmsDental.UI.CheckBox checkShowProcsInProcess;
		private UI.ComboBoxClinicPicker comboClinics;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textProcedureCodes;
		private System.Windows.Forms.Label label1;
		private UI.CheckBox checkExcludeProcCodes;
		private UI.CheckBox checkOnlyProcCodes;
		private UI.ListBox listBoxShowProcsBeforeIns;
	}
}
