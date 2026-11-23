using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormRpOutstandingIns {
		private System.ComponentModel.IContainer components = null;

		///<summary></summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if(components != null){
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		private void InitializeComponent() {
			this.labelProv = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.butRefresh = new OpenPmsDental.UI.Button();
			this.butExport = new OpenPmsDental.UI.Button();
			this.comboBoxMultiProv = new OpenPmsDental.UI.ComboBox();
			this.butPrint = new OpenPmsDental.UI.Button();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.checkIgnoreCustom = new OpenPmsDental.UI.CheckBox();
			this.textCarrier = new System.Windows.Forms.TextBox();
			this.labelCarrier = new System.Windows.Forms.Label();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.comboClinics = new OpenPmsDental.UI.ComboBoxClinicPicker();
			this.label13 = new System.Windows.Forms.Label();
			this.comboPreauthOptions = new OpenPmsDental.UI.ComboBox();
			this.tabControlDate = new OpenPmsDental.UI.TabControl();
			this.tabDaysOld = new OpenPmsDental.UI.TabPage();
			this.textDaysOldMax = new OpenPmsDental.ValidNum();
			this.labelDaysOldMax = new System.Windows.Forms.Label();
			this.textDaysOldMin = new OpenPmsDental.ValidNum();
			this.labelDateMinMaxAdvice = new System.Windows.Forms.Label();
			this.labelDaysOldMin = new System.Windows.Forms.Label();
			this.tabDateRange = new OpenPmsDental.UI.TabPage();
			this.dateRangePicker = new OpenPmsDental.UI.ODDateRangePicker();
			this.comboDateFilterBy = new OpenPmsDental.UI.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.comboErrorDef = new OpenPmsDental.UI.ComboBox();
			this.butMine = new OpenPmsDental.UI.Button();
			this.comboUserAssigned = new OpenPmsDental.UI.ComboBox();
			this.butPickUser = new OpenPmsDental.UI.Button();
			this.labelForUser = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.comboLastClaimTrack = new OpenPmsDental.UI.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.buttonUpdateCustomTrack = new OpenPmsDental.UI.Button();
			this.labelClaimCount = new System.Windows.Forms.Label();
			this.groupBoxUpdateCustomTracking = new OpenPmsDental.UI.GroupBox();
			this.labelCustomTracking = new System.Windows.Forms.Label();
			this.butAssignUser = new OpenPmsDental.UI.Button();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.textGroupName = new System.Windows.Forms.TextBox();
			this.textGroupNumber = new System.Windows.Forms.TextBox();
			this.textCarrierPhone = new OpenPmsDental.ValidPhone();
			this.textCarrierName = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox3 = new OpenPmsDental.UI.GroupBox();
			this.textPatDOB = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.textSubscriberID = new System.Windows.Forms.TextBox();
			this.textSubscriberDOB = new System.Windows.Forms.TextBox();
			this.textSubscriberName = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.butZeroClaims = new OpenPmsDental.UI.Button();
			this.groupBox2.SuspendLayout();
			this.tabControlDate.SuspendLayout();
			this.tabDaysOld.SuspendLayout();
			this.tabDateRange.SuspendLayout();
			this.groupBoxUpdateCustomTracking.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelProv
			// 
			this.labelProv.Location = new System.Drawing.Point(570, 23);
			this.labelProv.Name = "labelProv";
			this.labelProv.Size = new System.Drawing.Size(72, 23);
			this.labelProv.TabIndex = 48;
			this.labelProv.Text = "Treat Provs";
			this.labelProv.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Location = new System.Drawing.Point(1121, 653);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 23);
			this.label2.TabIndex = 46;
			this.label2.Text = "Total";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.Location = new System.Drawing.Point(1167, 653);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(76, 23);
			this.textBox1.TabIndex = 56;
			// 
			// butRefresh
			// 
			this.butRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butRefresh.Location = new System.Drawing.Point(1116, 8);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(82, 24);
			this.butRefresh.TabIndex = 58;
			this.butRefresh.Text = "&Refresh";
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// butExport
			// 
			this.butExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butExport.Icon = OpenPmsDental.UI.EnumIcons.Export;
			this.butExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butExport.Location = new System.Drawing.Point(8, 693);
			this.butExport.Name = "butExport";
			this.butExport.Size = new System.Drawing.Size(75, 24);
			this.butExport.TabIndex = 57;
			this.butExport.Text = "&Export";
			this.butExport.Click += new System.EventHandler(this.butExport_Click);
			// 
			// comboBoxMultiProv
			// 
			this.comboBoxMultiProv.BackColor = System.Drawing.SystemColors.Window;
			this.comboBoxMultiProv.Location = new System.Drawing.Point(648, 23);
			this.comboBoxMultiProv.Name = "comboBoxMultiProv";
			this.comboBoxMultiProv.SelectionModeMulti = true;
			this.comboBoxMultiProv.Size = new System.Drawing.Size(160, 23);
			this.comboBoxMultiProv.TabIndex = 53;
			this.comboBoxMultiProv.SelectionChangeCommitted += new System.EventHandler(this.comboBoxMultiProv_SelectionChangeCommitted);
			// 
			// butPrint
			// 
			this.butPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPrint.Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons.Print,16,16);
			this.butPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPrint.Location = new System.Drawing.Point(8, 721);
			this.butPrint.Name = "butPrint";
			this.butPrint.Size = new System.Drawing.Size(75, 24);
			this.butPrint.TabIndex = 52;
			this.butPrint.Text = "&Print";
			this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
			// 
			// gridMain
			// 
			this.gridMain.AllowSortingByColumn = true;
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.HScrollVisible = true;
			this.gridMain.Location = new System.Drawing.Point(8, 130);
			this.gridMain.Name = "gridMain";
			this.gridMain.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridMain.Size = new System.Drawing.Size(1235, 519);
			this.gridMain.TabIndex = 1;
			this.gridMain.Title = "Claims";
			this.gridMain.TranslationName = "TableOustandingInsClaims";
			this.gridMain.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			this.gridMain.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellClick);
			// 
			// checkIgnoreCustom
			// 
			this.checkIgnoreCustom.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIgnoreCustom.Location = new System.Drawing.Point(406, 77);
			this.checkIgnoreCustom.Name = "checkIgnoreCustom";
			this.checkIgnoreCustom.Size = new System.Drawing.Size(153, 20);
			this.checkIgnoreCustom.TabIndex = 61;
			this.checkIgnoreCustom.Text = "Ignore Custom Tracking";
			// 
			// textCarrier
			// 
			this.textCarrier.AutoSize = false;
			this.textCarrier.Location = new System.Drawing.Point(1000, 50);
			this.textCarrier.Name = "textCarrier";
			this.textCarrier.Size = new System.Drawing.Size(90, 23);
			this.textCarrier.TabIndex = 105;
			// 
			// labelCarrier
			// 
			this.labelCarrier.Location = new System.Drawing.Point(946, 50);
			this.labelCarrier.Name = "labelCarrier";
			this.labelCarrier.Size = new System.Drawing.Size(48, 23);
			this.labelCarrier.TabIndex = 106;
			this.labelCarrier.Text = "Carrier";
			this.labelCarrier.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.comboClinics);
			this.groupBox2.Controls.Add(this.label13);
			this.groupBox2.Controls.Add(this.comboPreauthOptions);
			this.groupBox2.Controls.Add(this.tabControlDate);
			this.groupBox2.Controls.Add(this.comboDateFilterBy);
			this.groupBox2.Controls.Add(this.checkIgnoreCustom);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.comboErrorDef);
			this.groupBox2.Controls.Add(this.butMine);
			this.groupBox2.Controls.Add(this.comboUserAssigned);
			this.groupBox2.Controls.Add(this.butPickUser);
			this.groupBox2.Controls.Add(this.labelForUser);
			this.groupBox2.Controls.Add(this.textCarrier);
			this.groupBox2.Controls.Add(this.labelCarrier);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.comboLastClaimTrack);
			this.groupBox2.Controls.Add(this.labelProv);
			this.groupBox2.Controls.Add(this.comboBoxMultiProv);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Location = new System.Drawing.Point(8, 8);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(1102, 118);
			this.groupBox2.TabIndex = 248;
			this.groupBox2.Text = "Filters";
			// 
			// comboClinics
			// 
			this.comboClinics.IncludeAll = true;
			this.comboClinics.IncludeHiddenInAll = true;
			this.comboClinics.IncludeUnassigned = true;
			this.comboClinics.Location = new System.Drawing.Point(600, 50);
			this.comboClinics.Name = "comboClinics";
			this.comboClinics.IsMultiSelect = true;
			this.comboClinics.Size = new System.Drawing.Size(208, 23);
			this.comboClinics.TabIndex = 267;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(300, 50);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(100, 23);
			this.label13.TabIndex = 266;
			this.label13.Text = "Preauth Options";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboPreauthOptions
			// 
			this.comboPreauthOptions.Location = new System.Drawing.Point(406, 50);
			this.comboPreauthOptions.Name = "comboPreauthOptions";
			this.comboPreauthOptions.Size = new System.Drawing.Size(160, 23);
			this.comboPreauthOptions.TabIndex = 265;
			this.comboPreauthOptions.SelectionChangeCommitted += new System.EventHandler(this.comboPreauthOptions_SelectionChangeCommitted);
			// 
			// tabControlDate
			// 
			this.tabControlDate.Controls.Add(this.tabDaysOld);
			this.tabControlDate.Controls.Add(this.tabDateRange);
			this.tabControlDate.Location = new System.Drawing.Point(8, 23);
			this.tabControlDate.Name = "tabControlDate";
			this.tabControlDate.PaddingTabPages = 1;
			this.tabControlDate.Size = new System.Drawing.Size(254, 87);
			this.tabControlDate.TabIndex = 264;
			this.tabControlDate.SelectedIndexChanged += new System.EventHandler(this.tabControlDate_SelectedIndexChanged);
			// 
			// tabDaysOld
			// 
			this.tabDaysOld.Controls.Add(this.textDaysOldMax);
			this.tabDaysOld.Controls.Add(this.labelDaysOldMax);
			this.tabDaysOld.Controls.Add(this.textDaysOldMin);
			this.tabDaysOld.Controls.Add(this.labelDateMinMaxAdvice);
			this.tabDaysOld.Controls.Add(this.labelDaysOldMin);
			this.tabDaysOld.Location = new System.Drawing.Point(1, 20);
			this.tabDaysOld.Name = "tabDaysOld";
			this.tabDaysOld.Padding = new System.Windows.Forms.Padding(3);
			this.tabDaysOld.Size = new System.Drawing.Size(252, 66);
			this.tabDaysOld.TabIndex = 0;
			this.tabDaysOld.Text = "Days Old";
			// 
			// textDaysOldMax
			// 
			this.textDaysOldMax.AutoSize = false;
			this.textDaysOldMax.Location = new System.Drawing.Point(106, 35);
			this.textDaysOldMax.MaxVal = 50000;
			this.textDaysOldMax.Name = "textDaysOldMax";
			this.textDaysOldMax.ShowZero = false;
			this.textDaysOldMax.Size = new System.Drawing.Size(60, 23);
			this.textDaysOldMax.TabIndex = 57;
			this.textDaysOldMax.Text = "0";
			// 
			// labelDaysOldMax
			// 
			this.labelDaysOldMax.Location = new System.Drawing.Point(56, 35);
			this.labelDaysOldMax.Name = "labelDaysOldMax";
			this.labelDaysOldMax.Size = new System.Drawing.Size(44, 23);
			this.labelDaysOldMax.TabIndex = 55;
			this.labelDaysOldMax.Text = "(max)";
			this.labelDaysOldMax.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDaysOldMin
			// 
			this.textDaysOldMin.AutoSize = false;
			this.textDaysOldMin.Location = new System.Drawing.Point(106, 8);
			this.textDaysOldMin.MaxVal = 50000;
			this.textDaysOldMin.Name = "textDaysOldMin";
			this.textDaysOldMin.ShowZero = false;
			this.textDaysOldMin.Size = new System.Drawing.Size(60, 23);
			this.textDaysOldMin.TabIndex = 58;
			this.textDaysOldMin.Text = "30";
			// 
			// labelDateMinMaxAdvice
			// 
			this.labelDateMinMaxAdvice.Location = new System.Drawing.Point(174, 8);
			this.labelDateMinMaxAdvice.Name = "labelDateMinMaxAdvice";
			this.labelDateMinMaxAdvice.Size = new System.Drawing.Size(70, 49);
			this.labelDateMinMaxAdvice.TabIndex = 59;
			this.labelDateMinMaxAdvice.Text = "(leave both blank to show all)";
			this.labelDateMinMaxAdvice.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labelDaysOldMin
			// 
			this.labelDaysOldMin.Location = new System.Drawing.Point(8, 8);
			this.labelDaysOldMin.Name = "labelDaysOldMin";
			this.labelDaysOldMin.Size = new System.Drawing.Size(92, 23);
			this.labelDaysOldMin.TabIndex = 56;
			this.labelDaysOldMin.Text = "Days Old (min)";
			this.labelDaysOldMin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabDateRange
			// 
			this.tabDateRange.Controls.Add(this.dateRangePicker);
			this.tabDateRange.Location = new System.Drawing.Point(1, 20);
			this.tabDateRange.Name = "tabDateRange";
			this.tabDateRange.Padding = new System.Windows.Forms.Padding(3);
			this.tabDateRange.Size = new System.Drawing.Size(245, 66);
			this.tabDateRange.TabIndex = 1;
			this.tabDateRange.Text = "Date Range";
			// 
			// dateRangePicker
			// 
			this.dateRangePicker.BackColor = System.Drawing.Color.Transparent;
			this.dateRangePicker.EnableWeekButtons = false;
			this.dateRangePicker.IsVertical = true;
			this.dateRangePicker.Location = new System.Drawing.Point(8, 8);
			this.dateRangePicker.MaximumSize = new System.Drawing.Size(0, 185);
			this.dateRangePicker.MinimumSize = new System.Drawing.Size(0, 22);
			this.dateRangePicker.Name = "dateRangePicker";
			this.dateRangePicker.Size = new System.Drawing.Size(229, 50);
			this.dateRangePicker.TabIndex = 0;
			this.dateRangePicker.CalendarClosed += new System.EventHandler(this.dateRangePicker_CalendarClosed);
			this.dateRangePicker.CalendarSelectionChanged += new System.EventHandler(this.dateRangePicker_CalendarSelectionChanged);
			// 
			// comboDateFilterBy
			// 
			this.comboDateFilterBy.Location = new System.Drawing.Point(406, 23);
			this.comboDateFilterBy.Name = "comboDateFilterBy";
			this.comboDateFilterBy.Size = new System.Drawing.Size(160, 23);
			this.comboDateFilterBy.TabIndex = 262;
			this.comboDateFilterBy.SelectionChangeCommitted += new System.EventHandler(this.ComboDateFilterBy_SelectionChangeCommitted);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(878, 77);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(116, 23);
			this.label3.TabIndex = 108;
			this.label3.Text = "Last Error Definition";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboErrorDef
			// 
			this.comboErrorDef.Location = new System.Drawing.Point(1000, 77);
			this.comboErrorDef.Name = "comboErrorDef";
			this.comboErrorDef.Size = new System.Drawing.Size(90, 23);
			this.comboErrorDef.TabIndex = 107;
			// 
			// butMine
			// 
			this.butMine.Location = new System.Drawing.Point(817, 77);
			this.butMine.Name = "butMine";
			this.butMine.Size = new System.Drawing.Size(51, 23);
			this.butMine.TabIndex = 111;
			this.butMine.Text = "Mine";
			this.butMine.Click += new System.EventHandler(this.butMine_Click);
			// 
			// comboUserAssigned
			// 
			this.comboUserAssigned.BackColor = System.Drawing.SystemColors.Window;
			this.comboUserAssigned.Location = new System.Drawing.Point(648, 77);
			this.comboUserAssigned.Name = "comboUserAssigned";
			this.comboUserAssigned.SelectionModeMulti = true;
			this.comboUserAssigned.Size = new System.Drawing.Size(136, 23);
			this.comboUserAssigned.TabIndex = 110;
			// 
			// butPickUser
			// 
			this.butPickUser.Location = new System.Drawing.Point(790, 77);
			this.butPickUser.Name = "butPickUser";
			this.butPickUser.Size = new System.Drawing.Size(23, 23);
			this.butPickUser.TabIndex = 109;
			this.butPickUser.Text = "...";
			this.butPickUser.Click += new System.EventHandler(this.butPickUser_Click);
			// 
			// labelForUser
			// 
			this.labelForUser.Location = new System.Drawing.Point(586, 77);
			this.labelForUser.Name = "labelForUser";
			this.labelForUser.Size = new System.Drawing.Size(56, 23);
			this.labelForUser.TabIndex = 108;
			this.labelForUser.Text = "For User";
			this.labelForUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(834, 23);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(160, 23);
			this.label4.TabIndex = 63;
			this.label4.Text = "Last Custom Tracking Status";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboLastClaimTrack
			// 
			this.comboLastClaimTrack.Location = new System.Drawing.Point(1000, 23);
			this.comboLastClaimTrack.Name = "comboLastClaimTrack";
			this.comboLastClaimTrack.Size = new System.Drawing.Size(90, 23);
			this.comboLastClaimTrack.TabIndex = 62;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(268, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(132, 23);
			this.label1.TabIndex = 263;
			this.label1.Text = "Date Range Applies To";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// buttonUpdateCustomTrack
			// 
			this.buttonUpdateCustomTrack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonUpdateCustomTrack.Location = new System.Drawing.Point(8, 57);
			this.buttonUpdateCustomTrack.Name = "buttonUpdateCustomTrack";
			this.buttonUpdateCustomTrack.Size = new System.Drawing.Size(140, 24);
			this.buttonUpdateCustomTrack.TabIndex = 249;
			this.buttonUpdateCustomTrack.Text = "Update Custom Tracking";
			this.buttonUpdateCustomTrack.Click += new System.EventHandler(this.buttonUpdateCustomTrack_Click);
			// 
			// labelClaimCount
			// 
			this.labelClaimCount.Location = new System.Drawing.Point(166, 57);
			this.labelClaimCount.Name = "labelClaimCount";
			this.labelClaimCount.Size = new System.Drawing.Size(100, 24);
			this.labelClaimCount.TabIndex = 250;
			this.labelClaimCount.Text = "claims";
			this.labelClaimCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBoxUpdateCustomTracking
			// 
			this.groupBoxUpdateCustomTracking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBoxUpdateCustomTracking.Controls.Add(this.labelCustomTracking);
			this.groupBoxUpdateCustomTracking.Controls.Add(this.buttonUpdateCustomTrack);
			this.groupBoxUpdateCustomTracking.Controls.Add(this.labelClaimCount);
			this.groupBoxUpdateCustomTracking.Location = new System.Drawing.Point(821, 653);
			this.groupBoxUpdateCustomTracking.Name = "groupBoxUpdateCustomTracking";
			this.groupBoxUpdateCustomTracking.Size = new System.Drawing.Size(280, 89);
			this.groupBoxUpdateCustomTracking.TabIndex = 254;
			this.groupBoxUpdateCustomTracking.Text = "Custom Tracking";
			// 
			// labelCustomTracking
			// 
			this.labelCustomTracking.Location = new System.Drawing.Point(8, 23);
			this.labelCustomTracking.Name = "labelCustomTracking";
			this.labelCustomTracking.Size = new System.Drawing.Size(220, 32);
			this.labelCustomTracking.TabIndex = 252;
			this.labelCustomTracking.Text = "Clicking Update will change the \r\nstatus of all of the claims in the grid.";
			this.labelCustomTracking.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// butAssignUser
			// 
			this.butAssignUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butAssignUser.Location = new System.Drawing.Point(8, 665);
			this.butAssignUser.Name = "butAssignUser";
			this.butAssignUser.Size = new System.Drawing.Size(75, 24);
			this.butAssignUser.TabIndex = 110;
			this.butAssignUser.Text = "Assign User";
			this.butAssignUser.Click += new System.EventHandler(this.butAssignUser_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.textGroupName);
			this.groupBox1.Controls.Add(this.textGroupNumber);
			this.groupBox1.Controls.Add(this.textCarrierPhone);
			this.groupBox1.Controls.Add(this.textCarrierName);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Location = new System.Drawing.Point(167, 653);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(321, 135);
			this.groupBox1.TabIndex = 255;
			this.groupBox1.Text = "Carrier/Plan Info";
			// 
			// textGroupName
			// 
			this.textGroupName.AutoSize = false;
			this.textGroupName.Location = new System.Drawing.Point(116, 104);
			this.textGroupName.Name = "textGroupName";
			this.textGroupName.ReadOnly = true;
			this.textGroupName.Size = new System.Drawing.Size(192, 23);
			this.textGroupName.TabIndex = 259;
			// 
			// textGroupNumber
			// 
			this.textGroupNumber.AutoSize = false;
			this.textGroupNumber.Location = new System.Drawing.Point(116, 77);
			this.textGroupNumber.Name = "textGroupNumber";
			this.textGroupNumber.ReadOnly = true;
			this.textGroupNumber.Size = new System.Drawing.Size(192, 23);
			this.textGroupNumber.TabIndex = 258;
			// 
			// textCarrierPhone
			// 
			this.textCarrierPhone.AutoSize = false;
			this.textCarrierPhone.Location = new System.Drawing.Point(116, 50);
			this.textCarrierPhone.Name = "textCarrierPhone";
			this.textCarrierPhone.ReadOnly = true;
			this.textCarrierPhone.Size = new System.Drawing.Size(192, 23);
			this.textCarrierPhone.TabIndex = 257;
			// 
			// textCarrierName
			// 
			this.textCarrierName.AutoSize = false;
			this.textCarrierName.Location = new System.Drawing.Point(116, 23);
			this.textCarrierName.Name = "textCarrierName";
			this.textCarrierName.ReadOnly = true;
			this.textCarrierName.Size = new System.Drawing.Size(192, 23);
			this.textCarrierName.TabIndex = 256;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(26, 104);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(84, 23);
			this.label8.TabIndex = 255;
			this.label8.Text = "Group Name:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(22, 50);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(88, 23);
			this.label7.TabIndex = 254;
			this.label7.Text = "Carrier Phone:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(14, 77);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(96, 23);
			this.label6.TabIndex = 253;
			this.label6.Text = "Group Number:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(58, 23);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(52, 23);
			this.label5.TabIndex = 252;
			this.label5.Text = "Carrier:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox3.Controls.Add(this.textPatDOB);
			this.groupBox3.Controls.Add(this.label9);
			this.groupBox3.Controls.Add(this.textSubscriberID);
			this.groupBox3.Controls.Add(this.textSubscriberDOB);
			this.groupBox3.Controls.Add(this.textSubscriberName);
			this.groupBox3.Controls.Add(this.label10);
			this.groupBox3.Controls.Add(this.label11);
			this.groupBox3.Controls.Add(this.label12);
			this.groupBox3.Location = new System.Drawing.Point(494, 653);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(321, 135);
			this.groupBox3.TabIndex = 260;
			this.groupBox3.Text = "Patient/Subscriber Info";
			// 
			// textPatDOB
			// 
			this.textPatDOB.AutoSize = false;
			this.textPatDOB.Location = new System.Drawing.Point(118, 23);
			this.textPatDOB.Name = "textPatDOB";
			this.textPatDOB.ReadOnly = true;
			this.textPatDOB.Size = new System.Drawing.Size(192, 23);
			this.textPatDOB.TabIndex = 260;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(32, 23);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(80, 23);
			this.label9.TabIndex = 259;
			this.label9.Text = "Patient DOB:";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textSubscriberID
			// 
			this.textSubscriberID.AutoSize = false;
			this.textSubscriberID.Location = new System.Drawing.Point(118, 104);
			this.textSubscriberID.Name = "textSubscriberID";
			this.textSubscriberID.ReadOnly = true;
			this.textSubscriberID.Size = new System.Drawing.Size(192, 23);
			this.textSubscriberID.TabIndex = 258;
			// 
			// textSubscriberDOB
			// 
			this.textSubscriberDOB.AutoSize = false;
			this.textSubscriberDOB.Location = new System.Drawing.Point(118, 77);
			this.textSubscriberDOB.Name = "textSubscriberDOB";
			this.textSubscriberDOB.ReadOnly = true;
			this.textSubscriberDOB.Size = new System.Drawing.Size(192, 23);
			this.textSubscriberDOB.TabIndex = 257;
			// 
			// textSubscriberName
			// 
			this.textSubscriberName.AutoSize = false;
			this.textSubscriberName.Location = new System.Drawing.Point(118, 50);
			this.textSubscriberName.Name = "textSubscriberName";
			this.textSubscriberName.ReadOnly = true;
			this.textSubscriberName.Size = new System.Drawing.Size(192, 23);
			this.textSubscriberName.TabIndex = 256;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(16, 77);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(96, 23);
			this.label10.TabIndex = 254;
			this.label10.Text = "Subscriber DOB:";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(28, 104);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(84, 23);
			this.label11.TabIndex = 253;
			this.label11.Text = "Subscriber ID:";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(8, 50);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(104, 23);
			this.label12.TabIndex = 252;
			this.label12.Text = "Subscriber Name:";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butZeroClaims
			// 
			this.butZeroClaims.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butZeroClaims.Location = new System.Drawing.Point(92, 665);
			this.butZeroClaims.Name = "butZeroClaims";
			this.butZeroClaims.Size = new System.Drawing.Size(75, 24);
			this.butZeroClaims.TabIndex = 261;
			this.butZeroClaims.Text = "Zero Claims";
			this.butZeroClaims.Click += new System.EventHandler(this.butZeroClaims_Click);
			// 
			// FormRpOutstandingIns
			// 
			this.ClientSize = new System.Drawing.Size(1251, 796);
			this.Controls.Add(this.butZeroClaims);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.butAssignUser);
			this.Controls.Add(this.groupBoxUpdateCustomTracking);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.butExport);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.butPrint);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.butRefresh);
			this.Controls.Add(this.gridMain);
			this.Name = "FormRpOutstandingIns";
			this.Text = "Outstanding Insurance Claims";
			this.Load += new System.EventHandler(this.FormRpOutIns_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.tabControlDate.ResumeLayout(false);
			this.tabDaysOld.ResumeLayout(false);
			this.tabDaysOld.PerformLayout();
			this.tabDateRange.ResumeLayout(false);
			this.groupBoxUpdateCustomTracking.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#region Designer Variables
		private UI.GridOD gridMain;
		private Label labelProv;
		private UI.Button butPrint;
		private UI.ComboBox comboBoxMultiProv;
		private Label label2;
		private TextBox textBox1;
		private UI.Button butExport;
		private UI.Button butRefresh;
		private OpenPmsDental.UI.CheckBox checkIgnoreCustom;
		private TextBox textCarrier;
		private Label labelCarrier;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private UI.Button buttonUpdateCustomTrack;
		private Label labelClaimCount;
		private Label label4;
		private UI.ComboBox comboLastClaimTrack;
		private OpenPmsDental.UI.GroupBox groupBoxUpdateCustomTracking;
		private Label labelCustomTracking;
		private Label label3;
		private UI.ComboBox comboErrorDef;
		private Label labelForUser;
		private UI.Button butPickUser;
		private UI.Button butAssignUser;
		private UI.ComboBox comboUserAssigned;
		private UI.Button butMine;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private TextBox textGroupName;
		private TextBox textGroupNumber;
		private ValidPhone textCarrierPhone;
		private TextBox textCarrierName;
		private Label label8;
		private Label label7;
		private Label label6;
		private Label label5;
		private OpenPmsDental.UI.GroupBox groupBox3;
		private TextBox textSubscriberID;
		private TextBox textSubscriberDOB;
		private TextBox textSubscriberName;
		private Label label10;
		private Label label11;
		private Label label12;
		private TextBox textPatDOB;
		private Label label9;
		private Label label1;
		private UI.ComboBox comboDateFilterBy;
		private OpenPmsDental.UI.TabControl tabControlDate;
		private OpenPmsDental.UI.TabPage tabDaysOld;
		private ValidNum textDaysOldMax;
		private Label labelDaysOldMax;
		private Label labelDateMinMaxAdvice;
		private Label labelDaysOldMin;
		private OpenPmsDental.UI.TabPage tabDateRange;
		private ValidNum textDaysOldMin;
		private UI.ODDateRangePicker dateRangePicker;
		private Label label13;
		private UI.ComboBox comboPreauthOptions;
		private UI.ComboBoxClinicPicker comboClinics;
		#endregion

		private UI.Button butZeroClaims;
	}
}
