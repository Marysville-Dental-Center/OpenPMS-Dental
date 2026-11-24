namespace OpenPmsDental{
	partial class FormApptPrintSetup {
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
			this.butSave = new OpenPmsDental.UI.Button();
			this.butOK = new OpenPmsDental.UI.Button();
			this.butPreview = new OpenPmsDental.UI.Button();
			this.groupBoxPrintRouting = new OpenPmsDental.UI.GroupBox();
			this.butSelectedView = new OpenPmsDental.UI.Button();
			this.butSelectedDay = new OpenPmsDental.UI.Button();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.groupBoxOD2 = new OpenPmsDental.UI.GroupBox();
			this.radioLessColor = new System.Windows.Forms.RadioButton();
			this.radioFullColor = new System.Windows.Forms.RadioButton();
			this.radioGrayscale = new System.Windows.Forms.RadioButton();
			this.groupBoxOD1 = new OpenPmsDental.UI.GroupBox();
			this.radioPortrait = new System.Windows.Forms.RadioButton();
			this.radioLandscape = new System.Windows.Forms.RadioButton();
			this.comboStop = new OpenPmsDental.UI.ComboBox();
			this.comboStart = new OpenPmsDental.UI.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textFontSize = new OpenPmsDental.ValidNum();
			this.labelStopTime = new System.Windows.Forms.Label();
			this.labelStartTime = new System.Windows.Forms.Label();
			this.labelFontSize = new System.Windows.Forms.Label();
			this.textColumnsPerPage = new OpenPmsDental.ValidNum();
			this.labelColumnsPerPage = new System.Windows.Forms.Label();
			this.groupBoxPrintRouting.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBoxOD2.SuspendLayout();
			this.groupBoxOD1.SuspendLayout();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butSave.Location = new System.Drawing.Point(339, 267);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 11;
			this.butSave.Text = "Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(153, 315);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 24);
			this.butOK.TabIndex = 13;
			this.butOK.Text = "&Print";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// butPreview
			// 
			this.butPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butPreview.Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons.Preview,17,16);
			this.butPreview.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butPreview.Location = new System.Drawing.Point(9, 315);
			this.butPreview.Name = "butPreview";
			this.butPreview.Size = new System.Drawing.Size(75, 24);
			this.butPreview.TabIndex = 12;
			this.butPreview.Text = "&Preview";
			this.butPreview.Click += new System.EventHandler(this.butPreview_Click);
			// 
			// groupBoxPrintRouting
			// 
			this.groupBoxPrintRouting.Controls.Add(this.butSelectedView);
			this.groupBoxPrintRouting.Controls.Add(this.butSelectedDay);
			this.groupBoxPrintRouting.Location = new System.Drawing.Point(8, 363);
			this.groupBoxPrintRouting.Name = "groupBoxPrintRouting";
			this.groupBoxPrintRouting.Size = new System.Drawing.Size(422, 83);
			this.groupBoxPrintRouting.TabIndex = 1;
			this.groupBoxPrintRouting.Text = "Print Routing Slips";
			// 
			// butSelectedView
			// 
			this.butSelectedView.Location = new System.Drawing.Point(21, 51);
			this.butSelectedView.Name = "butSelectedView";
			this.butSelectedView.Size = new System.Drawing.Size(120, 24);
			this.butSelectedView.TabIndex = 1;
			this.butSelectedView.Text = "Current View Only";
			this.butSelectedView.UseVisualStyleBackColor = true;
			this.butSelectedView.Click += new System.EventHandler(this.butCurrentView_Click);
			// 
			// butSelectedDay
			// 
			this.butSelectedDay.Location = new System.Drawing.Point(21, 23);
			this.butSelectedDay.Name = "butSelectedDay";
			this.butSelectedDay.Size = new System.Drawing.Size(120, 24);
			this.butSelectedDay.TabIndex = 0;
			this.butSelectedDay.Text = "All for Day";
			this.butSelectedDay.UseVisualStyleBackColor = true;
			this.butSelectedDay.Click += new System.EventHandler(this.butAllForDay_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.groupBoxOD2);
			this.groupBox1.Controls.Add(this.groupBoxOD1);
			this.groupBox1.Controls.Add(this.comboStop);
			this.groupBox1.Controls.Add(this.butPreview);
			this.groupBox1.Controls.Add(this.comboStart);
			this.groupBox1.Controls.Add(this.butSave);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.butOK);
			this.groupBox1.Controls.Add(this.textFontSize);
			this.groupBox1.Controls.Add(this.labelStopTime);
			this.groupBox1.Controls.Add(this.labelStartTime);
			this.groupBox1.Controls.Add(this.labelFontSize);
			this.groupBox1.Controls.Add(this.textColumnsPerPage);
			this.groupBox1.Controls.Add(this.labelColumnsPerPage);
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(422, 347);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.Text = "Print Appointments";
			// 
			// groupBoxOD2
			// 
			this.groupBoxOD2.Controls.Add(this.radioLessColor);
			this.groupBoxOD2.Controls.Add(this.radioFullColor);
			this.groupBoxOD2.Controls.Add(this.radioGrayscale);
			this.groupBoxOD2.Location = new System.Drawing.Point(53, 135);
			this.groupBoxOD2.Name = "groupBoxOD2";
			this.groupBoxOD2.Size = new System.Drawing.Size(130, 93);
			this.groupBoxOD2.TabIndex = 9;
			this.groupBoxOD2.Text = "Color";
			// 
			// radioLessColor
			// 
			this.radioLessColor.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.radioLessColor.Location = new System.Drawing.Point(8, 45);
			this.radioLessColor.Name = "radioLessColor";
			this.radioLessColor.Size = new System.Drawing.Size(77, 18);
			this.radioLessColor.TabIndex = 1;
			this.radioLessColor.TabStop = true;
			this.radioLessColor.Text = "Less color";
			this.radioLessColor.UseVisualStyleBackColor = true;
			// 
			// radioFullColor
			// 
			this.radioFullColor.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.radioFullColor.Location = new System.Drawing.Point(8, 23);
			this.radioFullColor.Name = "radioFullColor";
			this.radioFullColor.Size = new System.Drawing.Size(74, 18);
			this.radioFullColor.TabIndex = 0;
			this.radioFullColor.TabStop = true;
			this.radioFullColor.Text = "Full color";
			this.radioFullColor.UseVisualStyleBackColor = true;
			// 
			// radioGrayscale
			// 
			this.radioGrayscale.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.radioGrayscale.Location = new System.Drawing.Point(8, 67);
			this.radioGrayscale.Name = "radioGrayscale";
			this.radioGrayscale.Size = new System.Drawing.Size(75, 18);
			this.radioGrayscale.TabIndex = 2;
			this.radioGrayscale.TabStop = true;
			this.radioGrayscale.Text = "Grayscale";
			this.radioGrayscale.UseVisualStyleBackColor = true;
			// 
			// groupBoxOD1
			// 
			this.groupBoxOD1.Controls.Add(this.radioPortrait);
			this.groupBoxOD1.Controls.Add(this.radioLandscape);
			this.groupBoxOD1.Location = new System.Drawing.Point(53, 236);
			this.groupBoxOD1.Name = "groupBoxOD1";
			this.groupBoxOD1.Size = new System.Drawing.Size(130, 71);
			this.groupBoxOD1.TabIndex = 10;
			this.groupBoxOD1.Text = "Orientation";
			// 
			// radioPortrait
			// 
			this.radioPortrait.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.radioPortrait.Location = new System.Drawing.Point(8, 45);
			this.radioPortrait.Name = "radioPortrait";
			this.radioPortrait.Size = new System.Drawing.Size(64, 18);
			this.radioPortrait.TabIndex = 1;
			this.radioPortrait.Text = "Portrait";
			this.radioPortrait.UseVisualStyleBackColor = true;
			// 
			// radioLandscape
			// 
			this.radioLandscape.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.radioLandscape.Location = new System.Drawing.Point(8, 23);
			this.radioLandscape.Name = "radioLandscape";
			this.radioLandscape.Size = new System.Drawing.Size(81, 18);
			this.radioLandscape.TabIndex = 0;
			this.radioLandscape.Text = "Landscape";
			this.radioLandscape.UseVisualStyleBackColor = true;
			// 
			// comboStop
			// 
			this.comboStop.Location = new System.Drawing.Point(153, 50);
			this.comboStop.Name = "comboStop";
			this.comboStop.Size = new System.Drawing.Size(144, 23);
			this.comboStop.TabIndex = 3;
			// 
			// comboStart
			// 
			this.comboStart.Location = new System.Drawing.Point(153, 23);
			this.comboStart.Name = "comboStart";
			this.comboStart.Size = new System.Drawing.Size(144, 23);
			this.comboStart.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(207, 108);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(128, 15);
			this.label3.TabIndex = 8;
			this.label3.Text = "Between 2 and 50";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textFontSize
			// 
			this.textFontSize.Location = new System.Drawing.Point(153, 104);
			this.textFontSize.MaxVal = 50;
			this.textFontSize.MinVal = 2;
			this.textFontSize.Name = "textFontSize";
			this.textFontSize.Size = new System.Drawing.Size(48, 23);
			this.textFontSize.TabIndex = 7;
			// 
			// labelStopTime
			// 
			this.labelStopTime.Location = new System.Drawing.Point(83, 50);
			this.labelStopTime.Name = "labelStopTime";
			this.labelStopTime.Size = new System.Drawing.Size(64, 23);
			this.labelStopTime.TabIndex = 2;
			this.labelStopTime.Text = "Stop time";
			this.labelStopTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelStartTime
			// 
			this.labelStartTime.Location = new System.Drawing.Point(83, 23);
			this.labelStartTime.Name = "labelStartTime";
			this.labelStartTime.Size = new System.Drawing.Size(64, 23);
			this.labelStartTime.TabIndex = 0;
			this.labelStartTime.Text = "Start time";
			this.labelStartTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelFontSize
			// 
			this.labelFontSize.Location = new System.Drawing.Point(87, 104);
			this.labelFontSize.Name = "labelFontSize";
			this.labelFontSize.Size = new System.Drawing.Size(60, 23);
			this.labelFontSize.TabIndex = 6;
			this.labelFontSize.Text = "Font size";
			this.labelFontSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textColumnsPerPage
			// 
			this.textColumnsPerPage.Location = new System.Drawing.Point(153, 77);
			this.textColumnsPerPage.Name = "textColumnsPerPage";
			this.textColumnsPerPage.ShowZero = false;
			this.textColumnsPerPage.Size = new System.Drawing.Size(48, 23);
			this.textColumnsPerPage.TabIndex = 5;
			// 
			// labelColumnsPerPage
			// 
			this.labelColumnsPerPage.Location = new System.Drawing.Point(23, 77);
			this.labelColumnsPerPage.Name = "labelColumnsPerPage";
			this.labelColumnsPerPage.Size = new System.Drawing.Size(124, 23);
			this.labelColumnsPerPage.TabIndex = 4;
			this.labelColumnsPerPage.Text = "Operatories per page";
			this.labelColumnsPerPage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FormApptPrintSetup
			// 
			this.ClientSize = new System.Drawing.Size(438, 454);
			this.Controls.Add(this.groupBoxPrintRouting);
			this.Controls.Add(this.groupBox1);
			this.Name = "FormApptPrintSetup";
			this.Text = "Appt Print Setup";
			this.Load += new System.EventHandler(this.FormApptPrintSetup_Load);
			this.groupBoxPrintRouting.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBoxOD2.ResumeLayout(false);
			this.groupBoxOD1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private OpenPmsDental.UI.Button butOK;
		private UI.Button butSave;
		private UI.Button butPreview;
		private OpenPmsDental.UI.GroupBox groupBoxPrintRouting;
		private UI.Button butSelectedView;
		private UI.Button butSelectedDay;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private OpenPmsDental.UI.ComboBox comboStop;
		private OpenPmsDental.UI.ComboBox comboStart;
		private System.Windows.Forms.Label label3;
		private ValidNum textFontSize;
		private System.Windows.Forms.Label labelStopTime;
		private System.Windows.Forms.Label labelStartTime;
		private System.Windows.Forms.Label labelFontSize;
		private ValidNum textColumnsPerPage;
		private System.Windows.Forms.Label labelColumnsPerPage;
		private System.Windows.Forms.RadioButton radioFullColor;
		private System.Windows.Forms.RadioButton radioGrayscale;
		private System.Windows.Forms.RadioButton radioLessColor;
		private UI.GroupBox groupBoxOD1;
		private System.Windows.Forms.RadioButton radioPortrait;
		private System.Windows.Forms.RadioButton radioLandscape;
		private UI.GroupBox groupBoxOD2;
	}
}