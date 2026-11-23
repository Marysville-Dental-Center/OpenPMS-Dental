namespace OpenPmsDental {
	partial class FormEhrMeasureCalc {
		///<summary>Required designer variable.</summary>
		private System.ComponentModel.IContainer components = null;

		///<summary>Clean up any resources being used.</summary>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		///<summary>Required method for Designer support - do not modify the contents of this method with the code editor.</summary>
		private void InitializeComponent() {
			this.labelProv = new System.Windows.Forms.Label();
			this.comboProv = new OpenPmsDental.UI.ComboBox();
			this.labelDateStart = new System.Windows.Forms.Label();
			this.textDateStart = new OpenPmsDental.ValidDate();
			this.labelDateEnd = new System.Windows.Forms.Label();
			this.textDateEnd = new OpenPmsDental.ValidDate();
			this.labelStage = new System.Windows.Forms.Label();
			this.comboStage = new OpenPmsDental.UI.ComboBox();
			this.butRefresh = new OpenPmsDental.UI.Button();
			this.labelInternalReview = new System.Windows.Forms.Label();
			this.labelNotes = new System.Windows.Forms.Label();
			this.labelProgress = new System.Windows.Forms.Label();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.butCancel = new OpenPmsDental.UI.Button();
			this.butPrint = new OpenPmsDental.UI.Button();
			this.butClose = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			//
			// labelProv
			//
			this.labelProv.Location = new System.Drawing.Point(13, 8);
			this.labelProv.Name = "labelProv";
			this.labelProv.Size = new System.Drawing.Size(56, 23);
			this.labelProv.TabIndex = 0;
			this.labelProv.Text = "Provider";
			this.labelProv.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// comboProv
			//
			this.comboProv.Location = new System.Drawing.Point(75, 8);
			this.comboProv.Name = "comboProv";
			this.comboProv.Size = new System.Drawing.Size(300, 23);
			this.comboProv.TabIndex = 1;
			this.comboProv.SelectionChangeCommitted += new System.EventHandler(this.comboProv_SelectionChangeCommitted);
			//
			// labelDateStart
			//
			this.labelDateStart.Location = new System.Drawing.Point(392, 8);
			this.labelDateStart.Name = "labelDateStart";
			this.labelDateStart.Size = new System.Drawing.Size(40, 23);
			this.labelDateStart.TabIndex = 2;
			this.labelDateStart.Text = "From";
			this.labelDateStart.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textDateStart
			//
			this.textDateStart.Location = new System.Drawing.Point(438, 8);
			this.textDateStart.Name = "textDateStart";
			this.textDateStart.Size = new System.Drawing.Size(84, 23);
			this.textDateStart.TabIndex = 3;
			//
			// labelDateEnd
			//
			this.labelDateEnd.Location = new System.Drawing.Point(538, 8);
			this.labelDateEnd.Name = "labelDateEnd";
			this.labelDateEnd.Size = new System.Drawing.Size(24, 23);
			this.labelDateEnd.TabIndex = 4;
			this.labelDateEnd.Text = "to";
			this.labelDateEnd.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textDateEnd
			//
			this.textDateEnd.Location = new System.Drawing.Point(568, 8);
			this.textDateEnd.Name = "textDateEnd";
			this.textDateEnd.Size = new System.Drawing.Size(84, 23);
			this.textDateEnd.TabIndex = 5;
			//
			// labelStage
			//
			this.labelStage.Location = new System.Drawing.Point(668, 8);
			this.labelStage.Name = "labelStage";
			this.labelStage.Size = new System.Drawing.Size(40, 23);
			this.labelStage.TabIndex = 6;
			this.labelStage.Text = "Stage";
			this.labelStage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// comboStage
			//
			this.comboStage.Location = new System.Drawing.Point(714, 8);
			this.comboStage.Name = "comboStage";
			this.comboStage.Size = new System.Drawing.Size(140, 23);
			this.comboStage.TabIndex = 7;
			//
			// butRefresh
			//
			this.butRefresh.Location = new System.Drawing.Point(860, 8);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(75, 23);
			this.butRefresh.TabIndex = 8;
			this.butRefresh.Text = "&Refresh";
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			//
			// labelInternalReview
			//
			this.labelInternalReview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelInternalReview.ForeColor = System.Drawing.Color.Firebrick;
			this.labelInternalReview.Location = new System.Drawing.Point(8, 35);
			this.labelInternalReview.Name = "labelInternalReview";
			this.labelInternalReview.Size = new System.Drawing.Size(984, 18);
			this.labelInternalReview.TabIndex = 9;
			//
			// labelNotes
			//
			this.labelNotes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelNotes.Location = new System.Drawing.Point(8, 55);
			this.labelNotes.Name = "labelNotes";
			this.labelNotes.Size = new System.Drawing.Size(984, 80);
			this.labelNotes.TabIndex = 10;
			//
			// labelProgress
			//
			this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelProgress.Location = new System.Drawing.Point(8, 137);
			this.labelProgress.Name = "labelProgress";
			this.labelProgress.Size = new System.Drawing.Size(984, 30);
			this.labelProgress.TabIndex = 11;
			//
			// gridMain
			//
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.Location = new System.Drawing.Point(8, 169);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(984, 427);
			this.gridMain.TabIndex = 12;
			this.gridMain.Title = "Meaningful Use Measures (double-click a measure for its patients)";
			this.gridMain.TranslationName = "TableEhrMeasureCalc";
			this.gridMain.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			//
			// butCancel
			//
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butCancel.Location = new System.Drawing.Point(8, 608);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 24);
			this.butCancel.TabIndex = 13;
			this.butCancel.Text = "Cancel";
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			//
			// butPrint
			//
			this.butPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPrint.Location = new System.Drawing.Point(96, 608);
			this.butPrint.Name = "butPrint";
			this.butPrint.Size = new System.Drawing.Size(80, 24);
			this.butPrint.TabIndex = 14;
			this.butPrint.Text = "&Print";
			this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
			//
			// butClose
			//
			this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butClose.Location = new System.Drawing.Point(917, 608);
			this.butClose.Name = "butClose";
			this.butClose.Size = new System.Drawing.Size(75, 24);
			this.butClose.TabIndex = 15;
			this.butClose.Text = "&Close";
			this.butClose.Click += new System.EventHandler(this.butClose_Click);
			//
			// FormEhrMeasureCalc
			//
			this.ClientSize = new System.Drawing.Size(1000, 640);
			this.Controls.Add(this.butClose);
			this.Controls.Add(this.butPrint);
			this.Controls.Add(this.butCancel);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.labelProgress);
			this.Controls.Add(this.labelNotes);
			this.Controls.Add(this.labelInternalReview);
			this.Controls.Add(this.butRefresh);
			this.Controls.Add(this.comboStage);
			this.Controls.Add(this.labelStage);
			this.Controls.Add(this.textDateEnd);
			this.Controls.Add(this.labelDateEnd);
			this.Controls.Add(this.textDateStart);
			this.Controls.Add(this.labelDateStart);
			this.Controls.Add(this.comboProv);
			this.Controls.Add(this.labelProv);
			this.Name = "FormEhrMeasureCalc";
			this.Text = "Measure Calc";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEhrMeasureCalc_FormClosing);
			this.Load += new System.EventHandler(this.FormEhrMeasureCalc_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelProv;
		private OpenPmsDental.UI.ComboBox comboProv;
		private System.Windows.Forms.Label labelDateStart;
		private OpenPmsDental.ValidDate textDateStart;
		private System.Windows.Forms.Label labelDateEnd;
		private OpenPmsDental.ValidDate textDateEnd;
		private System.Windows.Forms.Label labelStage;
		private OpenPmsDental.UI.ComboBox comboStage;
		private OpenPmsDental.UI.Button butRefresh;
		private System.Windows.Forms.Label labelInternalReview;
		private System.Windows.Forms.Label labelNotes;
		private System.Windows.Forms.Label labelProgress;
		private OpenPmsDental.UI.GridOD gridMain;
		private OpenPmsDental.UI.Button butCancel;
		private OpenPmsDental.UI.Button butPrint;
		private OpenPmsDental.UI.Button butClose;
	}
}
