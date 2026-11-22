using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormRpProcOverpaid {
		private System.ComponentModel.IContainer components = null;

		///<summary></summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.butPrint = new OpenPmsDental.UI.Button();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.contextMenuGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemGoToAccount = new System.Windows.Forms.ToolStripMenuItem();
			this.butRefresh = new OpenPmsDental.UI.Button();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.checkExcludeOverpaidWithIns = new OpenPmsDental.UI.CheckBox();
			this.butCurrent = new OpenPmsDental.UI.Button();
			this.butAll = new OpenPmsDental.UI.Button();
			this.butFind = new OpenPmsDental.UI.Button();
			this.textPatient = new System.Windows.Forms.TextBox();
			this.labelPatient = new System.Windows.Forms.Label();
			this.labelProv = new System.Windows.Forms.Label();
			this.comboBoxMultiProv = new OpenPmsDental.UI.ComboBox();
			this.comboBoxMultiClinics = new OpenPmsDental.UI.ComboBoxClinicPicker();
			this.dateRangePicker = new OpenPmsDental.UI.ODDateRangePicker();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// butPrint
			// 
			this.butPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPrint.Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons.Print,16,16);
			this.butPrint.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPrint.Location = new System.Drawing.Point(12, 678);
			this.butPrint.Name = "butPrint";
			this.butPrint.Size = new System.Drawing.Size(75, 26);
			this.butPrint.TabIndex = 3;
			this.butPrint.Text = "Print";
			this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
			// 
			// gridMain
			// 
			this.gridMain.AllowSortingByColumn = true;
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.Location = new System.Drawing.Point(12, 93);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(991, 581);
			this.gridMain.TabIndex = 69;
			this.gridMain.Title = "Procedures Overpaid";
			this.gridMain.TranslationName = "TableProcedures";
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
			// butRefresh
			// 
			this.butRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butRefresh.Location = new System.Drawing.Point(935, 8);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(75, 26);
			this.butRefresh.TabIndex = 73;
			this.butRefresh.Text = "Refresh";
			this.butRefresh.UseVisualStyleBackColor = true;
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.checkExcludeOverpaidWithIns);
			this.groupBox2.Controls.Add(this.butCurrent);
			this.groupBox2.Controls.Add(this.butAll);
			this.groupBox2.Controls.Add(this.butFind);
			this.groupBox2.Controls.Add(this.textPatient);
			this.groupBox2.Controls.Add(this.labelPatient);
			this.groupBox2.Controls.Add(this.labelProv);
			this.groupBox2.Controls.Add(this.comboBoxMultiProv);
			this.groupBox2.Controls.Add(this.comboBoxMultiClinics);
			this.groupBox2.Controls.Add(this.dateRangePicker);
			this.groupBox2.Location = new System.Drawing.Point(12, 8);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(917, 81);
			this.groupBox2.TabIndex = 77;
			this.groupBox2.Text = "Filters";
			// 
			// checkExcludeOverpaidWithIns
			// 
			this.checkExcludeOverpaidWithIns.Location = new System.Drawing.Point(8, 51);
			this.checkExcludeOverpaidWithIns.Name = "checkExcludeOverpaidWithIns";
			this.checkExcludeOverpaidWithIns.Size = new System.Drawing.Size(252, 20);
			this.checkExcludeOverpaidWithIns.TabIndex = 80;
			this.checkExcludeOverpaidWithIns.Text = "Exclude if no Insurance Payment/Write-off";
			// 
			// butCurrent
			// 
			this.butCurrent.Location = new System.Drawing.Point(708, 50);
			this.butCurrent.Name = "butCurrent";
			this.butCurrent.Size = new System.Drawing.Size(63, 23);
			this.butCurrent.TabIndex = 78;
			this.butCurrent.Text = "Current";
			this.butCurrent.Click += new System.EventHandler(this.butCurrent_Click);
			// 
			// butAll
			// 
			this.butAll.Location = new System.Drawing.Point(846, 50);
			this.butAll.Name = "butAll";
			this.butAll.Size = new System.Drawing.Size(63, 23);
			this.butAll.TabIndex = 77;
			this.butAll.Text = "All";
			this.butAll.Click += new System.EventHandler(this.butAll_Click);
			// 
			// butFind
			// 
			this.butFind.Location = new System.Drawing.Point(777, 50);
			this.butFind.Name = "butFind";
			this.butFind.Size = new System.Drawing.Size(63, 23);
			this.butFind.TabIndex = 76;
			this.butFind.Text = "Find";
			this.butFind.Click += new System.EventHandler(this.butFind_Click);
			// 
			// textPatient
			// 
			this.textPatient.AutoSize = false;
			this.textPatient.Location = new System.Drawing.Point(708, 23);
			this.textPatient.Name = "textPatient";
			this.textPatient.Size = new System.Drawing.Size(201, 23);
			this.textPatient.TabIndex = 75;
			// 
			// labelPatient
			// 
			this.labelPatient.Location = new System.Drawing.Point(654, 23);
			this.labelPatient.Name = "labelPatient";
			this.labelPatient.Size = new System.Drawing.Size(48, 23);
			this.labelPatient.TabIndex = 74;
			this.labelPatient.Text = "Patient";
			this.labelPatient.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelProv
			// 
			this.labelProv.Location = new System.Drawing.Point(415, 27);
			this.labelProv.Name = "labelProv";
			this.labelProv.Size = new System.Drawing.Size(57, 18);
			this.labelProv.TabIndex = 73;
			this.labelProv.Text = "Providers";
			this.labelProv.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboBoxMultiProv
			// 
			this.comboBoxMultiProv.BackColor = System.Drawing.SystemColors.Window;
			this.comboBoxMultiProv.Location = new System.Drawing.Point(478, 23);
			this.comboBoxMultiProv.Name = "comboBoxMultiProv";
			this.comboBoxMultiProv.SelectionModeMulti = true;
			this.comboBoxMultiProv.Size = new System.Drawing.Size(160, 23);
			this.comboBoxMultiProv.TabIndex = 72;
			this.comboBoxMultiProv.SelectionChangeCommitted += new System.EventHandler(this.comboBoxMultiProv_SelectionChangeCommitted);
			// 
			// comboBoxMultiClinics
			// 
			this.comboBoxMultiClinics.IncludeAll = true;
			this.comboBoxMultiClinics.IncludeHiddenInAll = true;
			this.comboBoxMultiClinics.IncludeUnassigned = true;
			this.comboBoxMultiClinics.Location = new System.Drawing.Point(432, 50);
			this.comboBoxMultiClinics.Name = "comboBoxMultiClinics";
			this.comboBoxMultiClinics.IsMultiSelect = true;
			this.comboBoxMultiClinics.Size = new System.Drawing.Size(197, 23);
			this.comboBoxMultiClinics.TabIndex = 71;
			// 
			// dateRangePicker
			// 
			this.dateRangePicker.BackColor = System.Drawing.Color.Transparent;
			this.dateRangePicker.EnableWeekButtons = false;
			this.dateRangePicker.Location = new System.Drawing.Point(8, 23);
			this.dateRangePicker.MaximumSize = new System.Drawing.Size(0, 185);
			this.dateRangePicker.MinimumSize = new System.Drawing.Size(453, 22);
			this.dateRangePicker.Name = "dateRangePicker";
			this.dateRangePicker.Size = new System.Drawing.Size(453, 22);
			this.dateRangePicker.TabIndex = 0;
			// 
			// FormRpProcOverpaid
			// 
			this.AcceptButton = this.butPrint;
			this.ClientSize = new System.Drawing.Size(1018, 712);
			this.Controls.Add(this.butRefresh);
			this.Controls.Add(this.butPrint);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.groupBox2);
			this.Name = "FormRpProcOverpaid";
			this.Text = "Procedures Overpaid Report";
			this.Load += new System.EventHandler(this.FormProcOverpaid_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion

		private System.Windows.Forms.ContextMenuStrip contextMenuGrid;
		private System.Windows.Forms.ToolStripMenuItem menuItemGoToAccount;
		private UI.Button butRefresh;
		private UI.Button butPrint;
		private UI.GridOD gridMain;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private UI.ODDateRangePicker dateRangePicker;
		private UI.ComboBoxClinicPicker comboBoxMultiClinics;
		private UI.ComboBox comboBoxMultiProv;
		private Label labelProv;
		private UI.Button butCurrent;
		private UI.Button butAll;
		private UI.Button butFind;
		private TextBox textPatient;
		private Label labelPatient;
		private OpenPmsDental.UI.CheckBox checkExcludeOverpaidWithIns;
	}
}
