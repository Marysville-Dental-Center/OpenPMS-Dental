namespace OpenPmsDental{
	partial class FormRecurringChargesHistory {
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
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemGoTo = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemOpenPayment = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemViewError = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDeletePending = new System.Windows.Forms.ToolStripMenuItem();
			this.butRefresh = new OpenPmsDental.UI.Button();
			this.datePicker = new OpenPmsDental.UI.ODDateRangePicker();
			this.groupView = new OpenPmsDental.UI.GroupBox();
			this.comboAutomated = new OpenPmsDental.UI.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.comboStatuses = new OpenPmsDental.UI.ComboBox();
			this.comboClinics = new OpenPmsDental.UI.ComboBoxClinicPicker();
			this.groupView.SuspendLayout();
			this.SuspendLayout();
			// 
			// gridMain
			// 
			this.gridMain.AllowSortingByColumn = true;
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.Location = new System.Drawing.Point(20, 97);
			this.gridMain.Name = "gridMain";
			this.gridMain.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridMain.Size = new System.Drawing.Size(1005, 604);
			this.gridMain.TabIndex = 30;
			this.gridMain.Title = "Recurring Charges";
			this.gridMain.TranslationName = "TableRecurring";
			// 
			// contextMenu
			// 
			this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemGoTo,
            this.menuItemOpenPayment,
            this.menuItemViewError,
            this.menuItemDeletePending});
			this.contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu_Popup);
			// 
			// menuItemGoTo
			// 
			this.menuItemGoTo.Text = "Go To Account";
			this.menuItemGoTo.Click += new System.EventHandler(this.menuItemGoTo_Click);
			// 
			// menuItemOpenPayment
			// 
			this.menuItemOpenPayment.Text = "Open Payment";
			this.menuItemOpenPayment.Click += new System.EventHandler(this.menuItemOpenPayment_Click);
			// 
			// menuItemViewError
			// 
			this.menuItemViewError.Text = "View Error Message";
			this.menuItemViewError.Click += new System.EventHandler(this.menuItemViewError_Click);
			// 
			// menuItemDeletePending
			// 
			this.menuItemDeletePending.Text = "Delete Pending Charge";
			this.menuItemDeletePending.Click += new System.EventHandler(this.menuItemDeletePending_Click);
			// 
			// butRefresh
			// 
			this.butRefresh.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butRefresh.Location = new System.Drawing.Point(707, 23);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(75, 24);
			this.butRefresh.TabIndex = 32;
			this.butRefresh.Text = "&Refresh";
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// datePicker
			// 
			this.datePicker.BackColor = System.Drawing.Color.Transparent;
			this.datePicker.EnableWeekButtons = false;
			this.datePicker.IsVertical = true;
			this.datePicker.Location = new System.Drawing.Point(8, 23);
			this.datePicker.MaximumSize = new System.Drawing.Size(0, 185);
			this.datePicker.MinimumSize = new System.Drawing.Size(0, 22);
			this.datePicker.Name = "datePicker";
			this.datePicker.Size = new System.Drawing.Size(180, 46);
			this.datePicker.TabIndex = 33;
			this.datePicker.CalendarClosed += new System.EventHandler(this.FilterChanged);
			this.datePicker.Leave += new System.EventHandler(this.FilterChanged);
			// 
			// groupView
			// 
			this.groupView.Controls.Add(this.comboAutomated);
			this.groupView.Controls.Add(this.label2);
			this.groupView.Controls.Add(this.label1);
			this.groupView.Controls.Add(this.comboStatuses);
			this.groupView.Controls.Add(this.comboClinics);
			this.groupView.Controls.Add(this.butRefresh);
			this.groupView.Controls.Add(this.datePicker);
			this.groupView.Location = new System.Drawing.Point(20, 8);
			this.groupView.Name = "groupView";
			this.groupView.Size = new System.Drawing.Size(790, 81);
			this.groupView.TabIndex = 36;
			this.groupView.TabStop = false;
			this.groupView.Text = "View";
			// 
			// comboAutomated
			// 
			this.comboAutomated.Location = new System.Drawing.Point(517, 23);
			this.comboAutomated.Name = "comboAutomated";
			this.comboAutomated.Size = new System.Drawing.Size(184, 23);
			this.comboAutomated.TabIndex = 42;
			this.comboAutomated.SelectionChangeCommitted += new System.EventHandler(this.FilterChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(439, 23);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 41;
			this.label2.Text = "Automated";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(195, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(44, 23);
			this.label1.TabIndex = 39;
			this.label1.Text = "Status";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboStatuses
			// 
			this.comboStatuses.BackColor = System.Drawing.SystemColors.Window;
			this.comboStatuses.Location = new System.Drawing.Point(245, 23);
			this.comboStatuses.Name = "comboStatuses";
			this.comboStatuses.SelectionModeMulti = true;
			this.comboStatuses.Size = new System.Drawing.Size(160, 23);
			this.comboStatuses.TabIndex = 38;
			this.comboStatuses.SelectionChangeCommitted += new System.EventHandler(this.FilterChanged);
			// 
			// comboClinics
			// 
			this.comboClinics.IncludeAll = true;
			this.comboClinics.IncludeUnassigned = true;
			this.comboClinics.Location = new System.Drawing.Point(197, 50);
			this.comboClinics.Name = "comboClinics";
			this.comboClinics.IsMultiSelect = true;
			this.comboClinics.Size = new System.Drawing.Size(208, 23);
			this.comboClinics.TabIndex = 34;
			this.comboClinics.SelectionChangeCommitted += new System.EventHandler(this.FilterChanged);
			// 
			// FormRecurringChargesHistory
			// 
			this.ClientSize = new System.Drawing.Size(1044, 714);
			this.Controls.Add(this.groupView);
			this.Controls.Add(this.gridMain);
			this.Name = "FormRecurringChargesHistory";
			this.Text = "Recurring Charge History";
			this.Load += new System.EventHandler(this.FormRecurringChargesHistory_Load);
			this.groupView.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private UI.GridOD gridMain;
		private UI.Button butRefresh;
		private UI.ODDateRangePicker datePicker;
		private OpenPmsDental.UI.GroupBox groupView;
		private System.Windows.Forms.ContextMenuStrip contextMenu;
		private System.Windows.Forms.ToolStripMenuItem menuItemGoTo;
		private System.Windows.Forms.ToolStripMenuItem menuItemOpenPayment;
		private System.Windows.Forms.ToolStripMenuItem menuItemViewError;
		private System.Windows.Forms.ToolStripMenuItem menuItemDeletePending;
		private UI.ComboBoxClinicPicker comboClinics;
		private System.Windows.Forms.Label label1;
		private UI.ComboBox comboStatuses;
		private System.Windows.Forms.Label label2;
		private OpenPmsDental.UI.ComboBox comboAutomated;
	}
}