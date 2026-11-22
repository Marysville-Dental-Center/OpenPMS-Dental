namespace OpenPmsDental {
	partial class FormRpCustomAging {
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
			this.butRefresh = new OpenPmsDental.UI.Button();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.goToAccountToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.butPrint = new OpenPmsDental.UI.Button();
			this.gridTotals = new OpenPmsDental.UI.GridOD();
			this.checkExcludeBadAddresses = new OpenPmsDental.UI.CheckBox();
			this.checkAllClin = new OpenPmsDental.UI.CheckBox();
			this.checkAllProv = new OpenPmsDental.UI.CheckBox();
			this.checkAllBillType = new OpenPmsDental.UI.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.labelClinic = new System.Windows.Forms.Label();
			this.listBoxClins = new OpenPmsDental.UI.ListBox();
			this.listBoxProvs = new OpenPmsDental.UI.ListBox();
			this.listBoxBillTypes = new OpenPmsDental.UI.ListBox();
			this.checkAgeCredits = new OpenPmsDental.UI.CheckBox();
			this.groupBox4 = new OpenPmsDental.UI.GroupBox();
			this.radioWriteoffClaimDate = new System.Windows.Forms.RadioButton();
			this.radioWriteoffInsPayDate = new System.Windows.Forms.RadioButton();
			this.radioWriteoffProcDate = new System.Windows.Forms.RadioButton();
			this.checkExcludeInactive = new OpenPmsDental.UI.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.groupGroupBy = new OpenPmsDental.UI.GroupBox();
			this.radioGroupByPat = new System.Windows.Forms.RadioButton();
			this.radioGroupByFam = new System.Windows.Forms.RadioButton();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.radioExcludeNeg = new System.Windows.Forms.RadioButton();
			this.radioIncludeNeg = new System.Windows.Forms.RadioButton();
			this.radioShowOnlyNeg = new System.Windows.Forms.RadioButton();
			this.textDate = new OpenPmsDental.ValidDate();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox3 = new OpenPmsDental.UI.GroupBox();
			this.radio30 = new System.Windows.Forms.RadioButton();
			this.radio90 = new System.Windows.Forms.RadioButton();
			this.radio60 = new System.Windows.Forms.RadioButton();
			this.radioAny = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.checkAgePayPlanProduction = new OpenPmsDental.UI.CheckBox();
			this.checkAgeWriteoffEsts = new OpenPmsDental.UI.CheckBox();
			this.checkAgeProcedures = new OpenPmsDental.UI.CheckBox();
			this.checkAgePayPlanCharges = new OpenPmsDental.UI.CheckBox();
			this.checkAgePatPayments = new OpenPmsDental.UI.CheckBox();
			this.checkAgeWriteoffs = new OpenPmsDental.UI.CheckBox();
			this.checkAgeInsPayments = new OpenPmsDental.UI.CheckBox();
			this.checkAgeInsEsts = new OpenPmsDental.UI.CheckBox();
			this.checkAgeAdjustments = new OpenPmsDental.UI.CheckBox();
			this.checkExcludeArchive = new OpenPmsDental.UI.CheckBox();
			this.contextMenuStrip.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupGroupBy.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// butRefresh
			// 
			this.butRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butRefresh.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butRefresh.Location = new System.Drawing.Point(718, 8);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(75, 24);
			this.butRefresh.TabIndex = 86;
			this.butRefresh.Text = "Refresh";
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// gridMain
			// 
			this.gridMain.AllowSortingByColumn = true;
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.ContextMenuStrip = this.contextMenuStrip;
			this.gridMain.Location = new System.Drawing.Point(9, 8);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(703, 695);
			this.gridMain.TabIndex = 96;
			this.gridMain.Title = "Aging";
			this.gridMain.TranslationName = "TableCustomAging";
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.goToAccountToolStripMenuItem});
			this.contextMenuStrip.Name = "contextMenuStrip";
			this.contextMenuStrip.Size = new System.Drawing.Size(153, 26);
			// 
			// goToAccountToolStripMenuItem
			// 
			this.goToAccountToolStripMenuItem.Name = "goToAccountToolStripMenuItem";
			this.goToAccountToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.goToAccountToolStripMenuItem.Text = "Go To Account";
			this.goToAccountToolStripMenuItem.Click += new System.EventHandler(this.goToAccountToolStripMenuItem_Click);
			// 
			// butPrint
			// 
			this.butPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butPrint.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butPrint.Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons.Print,16,16);
			this.butPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPrint.Location = new System.Drawing.Point(718, 787);
			this.butPrint.Name = "butPrint";
			this.butPrint.Size = new System.Drawing.Size(75, 24);
			this.butPrint.TabIndex = 97;
			this.butPrint.Text = "Print";
			this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
			// 
			// gridTotals
			// 
			this.gridTotals.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridTotals.ContextMenuStrip = this.contextMenuStrip;
			this.gridTotals.HeadersVisible = false;
			this.gridTotals.Location = new System.Drawing.Point(9, 707);
			this.gridTotals.Name = "gridTotals";
			this.gridTotals.SelectionMode = OpenPmsDental.UI.GridSelectionMode.None;
			this.gridTotals.Size = new System.Drawing.Size(703, 20);
			this.gridTotals.TabIndex = 98;
			this.gridTotals.Title = "Aging";
			this.gridTotals.TitleVisible = false;
			this.gridTotals.TranslationName = "TableCustomAgingTotals";
			// 
			// checkExcludeBadAddresses
			// 
			this.checkExcludeBadAddresses.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkExcludeBadAddresses.Location = new System.Drawing.Point(884, 732);
			this.checkExcludeBadAddresses.Name = "checkExcludeBadAddresses";
			this.checkExcludeBadAddresses.Size = new System.Drawing.Size(145, 20);
			this.checkExcludeBadAddresses.TabIndex = 117;
			this.checkExcludeBadAddresses.Text = "Exclude bad addresses";
			// 
			// checkAllClin
			// 
			this.checkAllClin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAllClin.Checked = true;
			this.checkAllClin.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAllClin.Location = new System.Drawing.Point(884, 445);
			this.checkAllClin.Name = "checkAllClin";
			this.checkAllClin.Size = new System.Drawing.Size(133, 20);
			this.checkAllClin.TabIndex = 116;
			this.checkAllClin.Text = "All (includes hidden)";
			// 
			// checkAllProv
			// 
			this.checkAllProv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAllProv.Checked = true;
			this.checkAllProv.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAllProv.Location = new System.Drawing.Point(882, 230);
			this.checkAllProv.Name = "checkAllProv";
			this.checkAllProv.Size = new System.Drawing.Size(36, 20);
			this.checkAllProv.TabIndex = 115;
			this.checkAllProv.Text = "All";
			// 
			// checkAllBillType
			// 
			this.checkAllBillType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAllBillType.Checked = true;
			this.checkAllBillType.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAllBillType.Location = new System.Drawing.Point(884, 36);
			this.checkAllBillType.Name = "checkAllBillType";
			this.checkAllBillType.Size = new System.Drawing.Size(36, 20);
			this.checkAllBillType.TabIndex = 114;
			this.checkAllBillType.Text = "All";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.Location = new System.Drawing.Point(881, 11);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(139, 17);
			this.label3.TabIndex = 113;
			this.label3.Text = "Billing Types";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelClinic
			// 
			this.labelClinic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelClinic.Location = new System.Drawing.Point(882, 405);
			this.labelClinic.Name = "labelClinic";
			this.labelClinic.Size = new System.Drawing.Size(139, 17);
			this.labelClinic.TabIndex = 112;
			this.labelClinic.Text = "Clinics";
			this.labelClinic.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// listBoxClins
			// 
			this.listBoxClins.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.listBoxClins.Location = new System.Drawing.Point(884, 469);
			this.listBoxClins.Name = "listBoxClins";
			this.listBoxClins.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listBoxClins.Size = new System.Drawing.Size(157, 147);
			this.listBoxClins.TabIndex = 111;
			this.listBoxClins.Click += new System.EventHandler(this.listBoxClins_Click);
			// 
			// listBoxProvs
			// 
			this.listBoxProvs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.listBoxProvs.Location = new System.Drawing.Point(882, 254);
			this.listBoxProvs.Name = "listBoxProvs";
			this.listBoxProvs.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listBoxProvs.Size = new System.Drawing.Size(157, 147);
			this.listBoxProvs.TabIndex = 110;
			this.listBoxProvs.Click += new System.EventHandler(this.listBoxProvs_Click);
			// 
			// listBoxBillTypes
			// 
			this.listBoxBillTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.listBoxBillTypes.Location = new System.Drawing.Point(882, 60);
			this.listBoxBillTypes.Name = "listBoxBillTypes";
			this.listBoxBillTypes.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listBoxBillTypes.Size = new System.Drawing.Size(157, 147);
			this.listBoxBillTypes.TabIndex = 109;
			this.listBoxBillTypes.Click += new System.EventHandler(this.listBoxBillTypes_Click);
			// 
			// checkAgeCredits
			// 
			this.checkAgeCredits.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAgeCredits.Location = new System.Drawing.Point(884, 620);
			this.checkAgeCredits.Name = "checkAgeCredits";
			this.checkAgeCredits.Size = new System.Drawing.Size(86, 20);
			this.checkAgeCredits.TabIndex = 108;
			this.checkAgeCredits.Text = "Age Credits";
			// 
			// groupBox4
			// 
			this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox4.Controls.Add(this.radioWriteoffClaimDate);
			this.groupBox4.Controls.Add(this.radioWriteoffInsPayDate);
			this.groupBox4.Controls.Add(this.radioWriteoffProcDate);
			this.groupBox4.Location = new System.Drawing.Point(721, 576);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(157, 128);
			this.groupBox4.TabIndex = 103;
			this.groupBox4.Text = "Age Write-offs by";
			// 
			// radioWriteoffClaimDate
			// 
			this.radioWriteoffClaimDate.Location = new System.Drawing.Point(8, 71);
			this.radioWriteoffClaimDate.Name = "radioWriteoffClaimDate";
			this.radioWriteoffClaimDate.Size = new System.Drawing.Size(141, 49);
			this.radioWriteoffClaimDate.TabIndex = 2;
			this.radioWriteoffClaimDate.Text = "Initial claim date for est\r\nIns pay date for adj";
			this.radioWriteoffClaimDate.UseVisualStyleBackColor = true;
			// 
			// radioWriteoffInsPayDate
			// 
			this.radioWriteoffInsPayDate.Location = new System.Drawing.Point(8, 23);
			this.radioWriteoffInsPayDate.Name = "radioWriteoffInsPayDate";
			this.radioWriteoffInsPayDate.Size = new System.Drawing.Size(117, 20);
			this.radioWriteoffInsPayDate.TabIndex = 1;
			this.radioWriteoffInsPayDate.Text = "Ins Payment Date";
			this.radioWriteoffInsPayDate.UseVisualStyleBackColor = true;
			// 
			// radioWriteoffProcDate
			// 
			this.radioWriteoffProcDate.Checked = true;
			this.radioWriteoffProcDate.Location = new System.Drawing.Point(8, 47);
			this.radioWriteoffProcDate.Name = "radioWriteoffProcDate";
			this.radioWriteoffProcDate.Size = new System.Drawing.Size(106, 20);
			this.radioWriteoffProcDate.TabIndex = 0;
			this.radioWriteoffProcDate.TabStop = true;
			this.radioWriteoffProcDate.Text = "Procedure Date";
			this.radioWriteoffProcDate.UseVisualStyleBackColor = true;
			// 
			// checkExcludeInactive
			// 
			this.checkExcludeInactive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkExcludeInactive.Location = new System.Drawing.Point(884, 644);
			this.checkExcludeInactive.Name = "checkExcludeInactive";
			this.checkExcludeInactive.Size = new System.Drawing.Size(156, 20);
			this.checkExcludeInactive.TabIndex = 107;
			this.checkExcludeInactive.Text = "Exclude inactive patients";
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label4.Location = new System.Drawing.Point(882, 211);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(139, 17);
			this.label4.TabIndex = 106;
			this.label4.Text = "Providers";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupGroupBy
			// 
			this.groupGroupBy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupGroupBy.Controls.Add(this.radioGroupByPat);
			this.groupGroupBy.Controls.Add(this.radioGroupByFam);
			this.groupGroupBy.Location = new System.Drawing.Point(721, 708);
			this.groupGroupBy.Name = "groupGroupBy";
			this.groupGroupBy.Size = new System.Drawing.Size(157, 75);
			this.groupGroupBy.TabIndex = 102;
			this.groupGroupBy.Text = "Group By";
			// 
			// radioGroupByPat
			// 
			this.radioGroupByPat.Location = new System.Drawing.Point(8, 47);
			this.radioGroupByPat.Name = "radioGroupByPat";
			this.radioGroupByPat.Size = new System.Drawing.Size(77, 20);
			this.radioGroupByPat.TabIndex = 1;
			this.radioGroupByPat.Text = "Individual";
			this.radioGroupByPat.UseVisualStyleBackColor = true;
			// 
			// radioGroupByFam
			// 
			this.radioGroupByFam.Checked = true;
			this.radioGroupByFam.Location = new System.Drawing.Point(8, 23);
			this.radioGroupByFam.Name = "radioGroupByFam";
			this.radioGroupByFam.Size = new System.Drawing.Size(60, 20);
			this.radioGroupByFam.TabIndex = 0;
			this.radioGroupByFam.TabStop = true;
			this.radioGroupByFam.Text = "Family";
			this.radioGroupByFam.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.radioExcludeNeg);
			this.groupBox2.Controls.Add(this.radioIncludeNeg);
			this.groupBox2.Controls.Add(this.radioShowOnlyNeg);
			this.groupBox2.Location = new System.Drawing.Point(721, 342);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(157, 99);
			this.groupBox2.TabIndex = 105;
			this.groupBox2.Text = "Negative Balances";
			// 
			// radioExcludeNeg
			// 
			this.radioExcludeNeg.Checked = true;
			this.radioExcludeNeg.Location = new System.Drawing.Point(8, 23);
			this.radioExcludeNeg.Name = "radioExcludeNeg";
			this.radioExcludeNeg.Size = new System.Drawing.Size(65, 20);
			this.radioExcludeNeg.TabIndex = 4;
			this.radioExcludeNeg.TabStop = true;
			this.radioExcludeNeg.Text = "Exclude";
			// 
			// radioIncludeNeg
			// 
			this.radioIncludeNeg.Location = new System.Drawing.Point(8, 47);
			this.radioIncludeNeg.Name = "radioIncludeNeg";
			this.radioIncludeNeg.Size = new System.Drawing.Size(64, 20);
			this.radioIncludeNeg.TabIndex = 5;
			this.radioIncludeNeg.Text = "Include";
			// 
			// radioShowOnlyNeg
			// 
			this.radioShowOnlyNeg.Location = new System.Drawing.Point(8, 71);
			this.radioShowOnlyNeg.Name = "radioShowOnlyNeg";
			this.radioShowOnlyNeg.Size = new System.Drawing.Size(82, 20);
			this.radioShowOnlyNeg.TabIndex = 6;
			this.radioShowOnlyNeg.Text = "Show Only";
			// 
			// textDate
			// 
			this.textDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textDate.Location = new System.Drawing.Point(721, 60);
			this.textDate.Name = "textDate";
			this.textDate.Size = new System.Drawing.Size(157, 20);
			this.textDate.TabIndex = 101;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.Location = new System.Drawing.Point(718, 37);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(160, 17);
			this.label1.TabIndex = 100;
			this.label1.Text = "As Of Date";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.radio30);
			this.groupBox3.Controls.Add(this.radio90);
			this.groupBox3.Controls.Add(this.radio60);
			this.groupBox3.Controls.Add(this.radioAny);
			this.groupBox3.Location = new System.Drawing.Point(721, 445);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(157, 123);
			this.groupBox3.TabIndex = 104;
			this.groupBox3.Text = "Age of Account";
			// 
			// radio30
			// 
			this.radio30.Location = new System.Drawing.Point(8, 47);
			this.radio30.Name = "radio30";
			this.radio30.Size = new System.Drawing.Size(93, 20);
			this.radio30.TabIndex = 1;
			this.radio30.Text = "Over 30 Days";
			// 
			// radio90
			// 
			this.radio90.Location = new System.Drawing.Point(8, 95);
			this.radio90.Name = "radio90";
			this.radio90.Size = new System.Drawing.Size(93, 20);
			this.radio90.TabIndex = 3;
			this.radio90.Text = "Over 90 Days";
			// 
			// radio60
			// 
			this.radio60.Location = new System.Drawing.Point(8, 71);
			this.radio60.Name = "radio60";
			this.radio60.Size = new System.Drawing.Size(93, 20);
			this.radio60.TabIndex = 2;
			this.radio60.Text = "Over 60 Days";
			// 
			// radioAny
			// 
			this.radioAny.Checked = true;
			this.radioAny.Location = new System.Drawing.Point(8, 23);
			this.radioAny.Name = "radioAny";
			this.radioAny.Size = new System.Drawing.Size(90, 20);
			this.radioAny.TabIndex = 0;
			this.radioAny.TabStop = true;
			this.radioAny.Text = "Any Balance";
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.checkAgePayPlanProduction);
			this.groupBox1.Controls.Add(this.checkAgeWriteoffEsts);
			this.groupBox1.Controls.Add(this.checkAgeProcedures);
			this.groupBox1.Controls.Add(this.checkAgePayPlanCharges);
			this.groupBox1.Controls.Add(this.checkAgePatPayments);
			this.groupBox1.Controls.Add(this.checkAgeWriteoffs);
			this.groupBox1.Controls.Add(this.checkAgeInsPayments);
			this.groupBox1.Controls.Add(this.checkAgeInsEsts);
			this.groupBox1.Controls.Add(this.checkAgeAdjustments);
			this.groupBox1.Location = new System.Drawing.Point(721, 91);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(157, 243);
			this.groupBox1.TabIndex = 99;
			this.groupBox1.Text = "Include the following:";
			// 
			// checkAgePayPlanProduction
			// 
			this.checkAgePayPlanProduction.Checked = true;
			this.checkAgePayPlanProduction.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAgePayPlanProduction.Location = new System.Drawing.Point(8, 95);
			this.checkAgePayPlanProduction.Name = "checkAgePayPlanProduction";
			this.checkAgePayPlanProduction.Size = new System.Drawing.Size(132, 20);
			this.checkAgePayPlanProduction.TabIndex = 18;
			this.checkAgePayPlanProduction.Text = "Pay Plan Production";
			// 
			// checkAgeWriteoffEsts
			// 
			this.checkAgeWriteoffEsts.Location = new System.Drawing.Point(8, 215);
			this.checkAgeWriteoffEsts.Name = "checkAgeWriteoffEsts";
			this.checkAgeWriteoffEsts.Size = new System.Drawing.Size(127, 20);
			this.checkAgeWriteoffEsts.TabIndex = 17;
			this.checkAgeWriteoffEsts.Text = "Write-off Estimates";
			// 
			// checkAgeProcedures
			// 
			this.checkAgeProcedures.Checked = true;
			this.checkAgeProcedures.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAgeProcedures.Location = new System.Drawing.Point(8, 23);
			this.checkAgeProcedures.Name = "checkAgeProcedures";
			this.checkAgeProcedures.Size = new System.Drawing.Size(106, 20);
			this.checkAgeProcedures.TabIndex = 9;
			this.checkAgeProcedures.Text = "Procedure Fees";
			// 
			// checkAgePayPlanCharges
			// 
			this.checkAgePayPlanCharges.Checked = true;
			this.checkAgePayPlanCharges.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAgePayPlanCharges.Location = new System.Drawing.Point(8, 71);
			this.checkAgePayPlanCharges.Name = "checkAgePayPlanCharges";
			this.checkAgePayPlanCharges.Size = new System.Drawing.Size(116, 20);
			this.checkAgePayPlanCharges.TabIndex = 16;
			this.checkAgePayPlanCharges.Text = "Pay Plan Charges";
			// 
			// checkAgePatPayments
			// 
			this.checkAgePatPayments.Checked = true;
			this.checkAgePatPayments.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAgePatPayments.Location = new System.Drawing.Point(8, 119);
			this.checkAgePatPayments.Name = "checkAgePatPayments";
			this.checkAgePatPayments.Size = new System.Drawing.Size(117, 20);
			this.checkAgePatPayments.TabIndex = 11;
			this.checkAgePatPayments.Text = "Patient Payments";
			// 
			// checkAgeWriteoffs
			// 
			this.checkAgeWriteoffs.Checked = true;
			this.checkAgeWriteoffs.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAgeWriteoffs.Location = new System.Drawing.Point(8, 191);
			this.checkAgeWriteoffs.Name = "checkAgeWriteoffs";
			this.checkAgeWriteoffs.Size = new System.Drawing.Size(77, 20);
			this.checkAgeWriteoffs.TabIndex = 15;
			this.checkAgeWriteoffs.Text = "Write-offs";
			// 
			// checkAgeInsPayments
			// 
			this.checkAgeInsPayments.Checked = true;
			this.checkAgeInsPayments.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAgeInsPayments.Location = new System.Drawing.Point(8, 143);
			this.checkAgeInsPayments.Name = "checkAgeInsPayments";
			this.checkAgeInsPayments.Size = new System.Drawing.Size(131, 20);
			this.checkAgeInsPayments.TabIndex = 12;
			this.checkAgeInsPayments.Text = "Insurance Payments";
			// 
			// checkAgeInsEsts
			// 
			this.checkAgeInsEsts.Location = new System.Drawing.Point(8, 167);
			this.checkAgeInsEsts.Name = "checkAgeInsEsts";
			this.checkAgeInsEsts.Size = new System.Drawing.Size(130, 20);
			this.checkAgeInsEsts.TabIndex = 14;
			this.checkAgeInsEsts.Text = "Insurance Estimates";
			// 
			// checkAgeAdjustments
			// 
			this.checkAgeAdjustments.Checked = true;
			this.checkAgeAdjustments.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAgeAdjustments.Location = new System.Drawing.Point(8, 47);
			this.checkAgeAdjustments.Name = "checkAgeAdjustments";
			this.checkAgeAdjustments.Size = new System.Drawing.Size(91, 20);
			this.checkAgeAdjustments.TabIndex = 13;
			this.checkAgeAdjustments.Text = "Adjustments";
			// 
			// checkExcludeArchive
			// 
			this.checkExcludeArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkExcludeArchive.Location = new System.Drawing.Point(884, 708);
			this.checkExcludeArchive.Name = "checkExcludeArchive";
			this.checkExcludeArchive.Size = new System.Drawing.Size(160, 20);
			this.checkExcludeArchive.TabIndex = 119;
			this.checkExcludeArchive.Text = "Exclude archived patients";
			// 
			// FormRpCustomAging
			// 
			this.ClientSize = new System.Drawing.Size(1059, 819);
			this.Controls.Add(this.checkExcludeArchive);
			this.Controls.Add(this.checkExcludeBadAddresses);
			this.Controls.Add(this.checkAllClin);
			this.Controls.Add(this.checkAllProv);
			this.Controls.Add(this.checkAllBillType);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.labelClinic);
			this.Controls.Add(this.listBoxClins);
			this.Controls.Add(this.listBoxProvs);
			this.Controls.Add(this.listBoxBillTypes);
			this.Controls.Add(this.checkAgeCredits);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.checkExcludeInactive);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.groupGroupBy);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.textDate);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.gridTotals);
			this.Controls.Add(this.butPrint);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.butRefresh);
			this.Name = "FormRpCustomAging";
			this.Text = "Custom Aging Report";
			this.Load += new System.EventHandler(this.FormRpCustomAging_Load);
			this.contextMenuStrip.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupGroupBy.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private UI.Button butRefresh;
		private UI.GridOD gridMain;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem goToAccountToolStripMenuItem;
		private UI.Button butPrint;
		private UI.GridOD gridTotals;
		private OpenPmsDental.UI.CheckBox checkExcludeBadAddresses;
		private OpenPmsDental.UI.CheckBox checkAllClin;
		private OpenPmsDental.UI.CheckBox checkAllProv;
		private OpenPmsDental.UI.CheckBox checkAllBillType;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label labelClinic;
		private OpenPmsDental.UI.ListBox listBoxClins;
		private OpenPmsDental.UI.ListBox listBoxProvs;
		private OpenPmsDental.UI.ListBox listBoxBillTypes;
		private OpenPmsDental.UI.CheckBox checkAgeCredits;
		private OpenPmsDental.UI.GroupBox groupBox4;
		private System.Windows.Forms.RadioButton radioWriteoffInsPayDate;
		private System.Windows.Forms.RadioButton radioWriteoffProcDate;
		private OpenPmsDental.UI.CheckBox checkExcludeInactive;
		private System.Windows.Forms.Label label4;
		private OpenPmsDental.UI.GroupBox groupGroupBy;
		private System.Windows.Forms.RadioButton radioGroupByPat;
		private System.Windows.Forms.RadioButton radioGroupByFam;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private System.Windows.Forms.RadioButton radioExcludeNeg;
		private System.Windows.Forms.RadioButton radioIncludeNeg;
		private System.Windows.Forms.RadioButton radioShowOnlyNeg;
		private ValidDate textDate;
		private System.Windows.Forms.Label label1;
		private OpenPmsDental.UI.GroupBox groupBox3;
		private System.Windows.Forms.RadioButton radio30;
		private System.Windows.Forms.RadioButton radio90;
		private System.Windows.Forms.RadioButton radio60;
		private System.Windows.Forms.RadioButton radioAny;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private OpenPmsDental.UI.CheckBox checkAgePayPlanProduction;
		private OpenPmsDental.UI.CheckBox checkAgeWriteoffEsts;
		private OpenPmsDental.UI.CheckBox checkAgeProcedures;
		private OpenPmsDental.UI.CheckBox checkAgePayPlanCharges;
		private OpenPmsDental.UI.CheckBox checkAgePatPayments;
		private OpenPmsDental.UI.CheckBox checkAgeWriteoffs;
		private OpenPmsDental.UI.CheckBox checkAgeInsPayments;
		private OpenPmsDental.UI.CheckBox checkAgeInsEsts;
		private OpenPmsDental.UI.CheckBox checkAgeAdjustments;
		private OpenPmsDental.UI.CheckBox checkExcludeArchive;
		private System.Windows.Forms.RadioButton radioWriteoffClaimDate;
	}
}