namespace OpenPmsDental.User_Controls {
	partial class UserControlReportSetup {
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.labelInstructions = new System.Windows.Forms.Label();
			this.labelAllowedCount = new System.Windows.Forms.Label();
			this.labelUserGroup = new System.Windows.Forms.Label();
			this.comboUserGroup = new System.Windows.Forms.ComboBox();
			this.gridReports = new OpenPmsDental.UI.GridOD();
			this.butDown = new OpenPmsDental.UI.Button();
			this.butUp = new OpenPmsDental.UI.Button();
			this.butSetAll = new OpenPmsDental.UI.Button();
			this.butSetNone = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			//
			// labelInstructions
			//
			this.labelInstructions.Location = new System.Drawing.Point(8, 8);
			this.labelInstructions.Name = "labelInstructions";
			this.labelInstructions.Size = new System.Drawing.Size(340, 31);
			this.labelInstructions.TabIndex = 221;
			this.labelInstructions.Text = "Click a display name to rename the report, or a box to mark it.\r\nUp and Down move the selected report within its category.";
			this.labelInstructions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			//
			// labelAllowedCount
			//
			this.labelAllowedCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelAllowedCount.Location = new System.Drawing.Point(369, 8);
			this.labelAllowedCount.Name = "labelAllowedCount";
			this.labelAllowedCount.Size = new System.Drawing.Size(148, 23);
			this.labelAllowedCount.TabIndex = 222;
			this.labelAllowedCount.Text = "Reports allowed: 0 / 0";
			this.labelAllowedCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// labelUserGroup
			//
			this.labelUserGroup.Location = new System.Drawing.Point(8, 8);
			this.labelUserGroup.Name = "labelUserGroup";
			this.labelUserGroup.Size = new System.Drawing.Size(72, 23);
			this.labelUserGroup.TabIndex = 224;
			this.labelUserGroup.Text = "User Group";
			this.labelUserGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// comboUserGroup
			//
			this.comboUserGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboUserGroup.Location = new System.Drawing.Point(86, 8);
			this.comboUserGroup.Name = "comboUserGroup";
			this.comboUserGroup.Size = new System.Drawing.Size(133, 23);
			this.comboUserGroup.TabIndex = 225;
			this.comboUserGroup.SelectionChangeCommitted += new System.EventHandler(this.comboUserGroup_SelectionChangeCommitted);
			//
			// gridReports
			//
			this.gridReports.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridReports.Location = new System.Drawing.Point(8, 47);
			this.gridReports.Name = "gridReports";
			this.gridReports.SelectionMode = OpenPmsDental.UI.GridSelectionMode.OneCell;
			this.gridReports.Size = new System.Drawing.Size(509, 552);
			this.gridReports.TabIndex = 214;
			this.gridReports.Title = "Reports";
			this.gridReports.TranslationName = "TableReportSetup";
			this.gridReports.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridReports_CellClick);
			this.gridReports.CellLeave += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridReports_CellLeave);
			//
			// butDown
			//
			this.butDown.Icon = OpenPmsDental.UI.EnumIcons.Down;
			this.butDown.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butDown.Location = new System.Drawing.Point(442, 8);
			this.butDown.Name = "butDown";
			this.butDown.Size = new System.Drawing.Size(75, 24);
			this.butDown.TabIndex = 220;
			this.butDown.Text = "Down";
			this.butDown.Click += new System.EventHandler(this.butDown_Click);
			//
			// butUp
			//
			this.butUp.Icon = OpenPmsDental.UI.EnumIcons.Up;
			this.butUp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.butUp.Location = new System.Drawing.Point(361, 8);
			this.butUp.Name = "butUp";
			this.butUp.Size = new System.Drawing.Size(75, 24);
			this.butUp.TabIndex = 219;
			this.butUp.Text = "Up";
			this.butUp.Click += new System.EventHandler(this.butUp_Click);
			//
			// butSetAll
			//
			this.butSetAll.Location = new System.Drawing.Point(225, 8);
			this.butSetAll.Name = "butSetAll";
			this.butSetAll.Size = new System.Drawing.Size(66, 23);
			this.butSetAll.TabIndex = 223;
			this.butSetAll.Text = "Set All";
			this.butSetAll.Click += new System.EventHandler(this.butSetAll_Click);
			//
			// butSetNone
			//
			this.butSetNone.Location = new System.Drawing.Point(297, 8);
			this.butSetNone.Name = "butSetNone";
			this.butSetNone.Size = new System.Drawing.Size(66, 23);
			this.butSetNone.TabIndex = 226;
			this.butSetNone.Text = "Set None";
			this.butSetNone.Click += new System.EventHandler(this.butSetNone_Click);
			//
			// UserControlReportSetup
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.butSetNone);
			this.Controls.Add(this.comboUserGroup);
			this.Controls.Add(this.labelUserGroup);
			this.Controls.Add(this.gridReports);
			this.Controls.Add(this.labelAllowedCount);
			this.Controls.Add(this.labelInstructions);
			this.Controls.Add(this.butDown);
			this.Controls.Add(this.butUp);
			this.Controls.Add(this.butSetAll);
			this.Name = "UserControlReportSetup";
			this.Size = new System.Drawing.Size(525, 607);
			this.ResumeLayout(false);

		}

		#endregion

		private UI.Button butUp;
		private UI.Button butDown;
		private System.Windows.Forms.Label labelInstructions;
		private System.Windows.Forms.Label labelAllowedCount;
		private UI.GridOD gridReports;
		private UI.Button butSetAll;
		private System.Windows.Forms.Label labelUserGroup;
		private System.Windows.Forms.ComboBox comboUserGroup;
		private UI.Button butSetNone;
	}
}
