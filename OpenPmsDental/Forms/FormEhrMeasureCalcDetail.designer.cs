namespace OpenPmsDental {
	partial class FormEhrMeasureCalcDetail {
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
			this.labelInternalReview = new System.Windows.Forms.Label();
			this.labelMeasureName = new System.Windows.Forms.Label();
			this.textMeasureName = new System.Windows.Forms.TextBox();
			this.labelPeriod = new System.Windows.Forms.Label();
			this.textPeriod = new System.Windows.Forms.TextBox();
			this.labelObjective = new System.Windows.Forms.Label();
			this.textObjective = new System.Windows.Forms.TextBox();
			this.labelMeasure = new System.Windows.Forms.Label();
			this.textMeasure = new System.Windows.Forms.TextBox();
			this.labelNumeratorExplain = new System.Windows.Forms.Label();
			this.textNumeratorExplain = new System.Windows.Forms.TextBox();
			this.labelDenominatorExplain = new System.Windows.Forms.Label();
			this.textDenominatorExplain = new System.Windows.Forms.TextBox();
			this.labelExclusionExplain = new System.Windows.Forms.Label();
			this.textExclusionExplain = new System.Windows.Forms.TextBox();
			this.labelExclusionCount = new System.Windows.Forms.Label();
			this.textExclusionCount = new System.Windows.Forms.TextBox();
			this.labelResult = new System.Windows.Forms.Label();
			this.textResult = new System.Windows.Forms.TextBox();
			this.labelHint = new System.Windows.Forms.Label();
			this.gridAudit = new OpenPmsDental.UI.GridOD();
			this.butClose = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			//
			// labelInternalReview
			//
			this.labelInternalReview.ForeColor = System.Drawing.Color.Firebrick;
			this.labelInternalReview.Location = new System.Drawing.Point(12, 8);
			this.labelInternalReview.Name = "labelInternalReview";
			this.labelInternalReview.Size = new System.Drawing.Size(836, 18);
			this.labelInternalReview.TabIndex = 0;
			//
			// labelMeasureName
			//
			this.labelMeasureName.Location = new System.Drawing.Point(12, 30);
			this.labelMeasureName.Name = "labelMeasureName";
			this.labelMeasureName.Size = new System.Drawing.Size(140, 18);
			this.labelMeasureName.TabIndex = 1;
			this.labelMeasureName.Text = "Measure type";
			this.labelMeasureName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textMeasureName
			//
			this.textMeasureName.Location = new System.Drawing.Point(155, 29);
			this.textMeasureName.Name = "textMeasureName";
			this.textMeasureName.ReadOnly = true;
			this.textMeasureName.Size = new System.Drawing.Size(693, 20);
			this.textMeasureName.TabIndex = 2;
			//
			// labelPeriod
			//
			this.labelPeriod.Location = new System.Drawing.Point(12, 54);
			this.labelPeriod.Name = "labelPeriod";
			this.labelPeriod.Size = new System.Drawing.Size(140, 18);
			this.labelPeriod.TabIndex = 3;
			this.labelPeriod.Text = "Calculated for";
			this.labelPeriod.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textPeriod
			//
			this.textPeriod.Location = new System.Drawing.Point(155, 53);
			this.textPeriod.Name = "textPeriod";
			this.textPeriod.ReadOnly = true;
			this.textPeriod.Size = new System.Drawing.Size(693, 20);
			this.textPeriod.TabIndex = 4;
			//
			// labelObjective
			//
			this.labelObjective.Location = new System.Drawing.Point(12, 78);
			this.labelObjective.Name = "labelObjective";
			this.labelObjective.Size = new System.Drawing.Size(140, 18);
			this.labelObjective.TabIndex = 5;
			this.labelObjective.Text = "Objective";
			this.labelObjective.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textObjective
			//
			this.textObjective.Location = new System.Drawing.Point(155, 77);
			this.textObjective.Multiline = true;
			this.textObjective.Name = "textObjective";
			this.textObjective.ReadOnly = true;
			this.textObjective.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textObjective.Size = new System.Drawing.Size(693, 44);
			this.textObjective.TabIndex = 6;
			//
			// labelMeasure
			//
			this.labelMeasure.Location = new System.Drawing.Point(12, 126);
			this.labelMeasure.Name = "labelMeasure";
			this.labelMeasure.Size = new System.Drawing.Size(140, 18);
			this.labelMeasure.TabIndex = 7;
			this.labelMeasure.Text = "Measure";
			this.labelMeasure.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textMeasure
			//
			this.textMeasure.Location = new System.Drawing.Point(155, 125);
			this.textMeasure.Multiline = true;
			this.textMeasure.Name = "textMeasure";
			this.textMeasure.ReadOnly = true;
			this.textMeasure.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textMeasure.Size = new System.Drawing.Size(693, 44);
			this.textMeasure.TabIndex = 8;
			//
			// labelNumeratorExplain
			//
			this.labelNumeratorExplain.Location = new System.Drawing.Point(12, 174);
			this.labelNumeratorExplain.Name = "labelNumeratorExplain";
			this.labelNumeratorExplain.Size = new System.Drawing.Size(140, 18);
			this.labelNumeratorExplain.TabIndex = 9;
			this.labelNumeratorExplain.Text = "Numerator";
			this.labelNumeratorExplain.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textNumeratorExplain
			//
			this.textNumeratorExplain.Location = new System.Drawing.Point(155, 173);
			this.textNumeratorExplain.Multiline = true;
			this.textNumeratorExplain.Name = "textNumeratorExplain";
			this.textNumeratorExplain.ReadOnly = true;
			this.textNumeratorExplain.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textNumeratorExplain.Size = new System.Drawing.Size(693, 44);
			this.textNumeratorExplain.TabIndex = 10;
			//
			// labelDenominatorExplain
			//
			this.labelDenominatorExplain.Location = new System.Drawing.Point(12, 222);
			this.labelDenominatorExplain.Name = "labelDenominatorExplain";
			this.labelDenominatorExplain.Size = new System.Drawing.Size(140, 18);
			this.labelDenominatorExplain.TabIndex = 11;
			this.labelDenominatorExplain.Text = "Denominator";
			this.labelDenominatorExplain.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textDenominatorExplain
			//
			this.textDenominatorExplain.Location = new System.Drawing.Point(155, 221);
			this.textDenominatorExplain.Multiline = true;
			this.textDenominatorExplain.Name = "textDenominatorExplain";
			this.textDenominatorExplain.ReadOnly = true;
			this.textDenominatorExplain.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textDenominatorExplain.Size = new System.Drawing.Size(693, 44);
			this.textDenominatorExplain.TabIndex = 12;
			//
			// labelExclusionExplain
			//
			this.labelExclusionExplain.Location = new System.Drawing.Point(12, 270);
			this.labelExclusionExplain.Name = "labelExclusionExplain";
			this.labelExclusionExplain.Size = new System.Drawing.Size(140, 18);
			this.labelExclusionExplain.TabIndex = 13;
			this.labelExclusionExplain.Text = "Exclusion";
			this.labelExclusionExplain.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textExclusionExplain
			//
			this.textExclusionExplain.Location = new System.Drawing.Point(155, 269);
			this.textExclusionExplain.Multiline = true;
			this.textExclusionExplain.Name = "textExclusionExplain";
			this.textExclusionExplain.ReadOnly = true;
			this.textExclusionExplain.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textExclusionExplain.Size = new System.Drawing.Size(693, 44);
			this.textExclusionExplain.TabIndex = 14;
			//
			// labelExclusionCount
			//
			this.labelExclusionCount.Location = new System.Drawing.Point(12, 318);
			this.labelExclusionCount.Name = "labelExclusionCount";
			this.labelExclusionCount.Size = new System.Drawing.Size(140, 18);
			this.labelExclusionCount.TabIndex = 15;
			this.labelExclusionCount.Text = "Exclusion count";
			this.labelExclusionCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textExclusionCount
			//
			this.textExclusionCount.Location = new System.Drawing.Point(155, 317);
			this.textExclusionCount.Name = "textExclusionCount";
			this.textExclusionCount.ReadOnly = true;
			this.textExclusionCount.Size = new System.Drawing.Size(693, 20);
			this.textExclusionCount.TabIndex = 16;
			//
			// labelResult
			//
			this.labelResult.Location = new System.Drawing.Point(12, 342);
			this.labelResult.Name = "labelResult";
			this.labelResult.Size = new System.Drawing.Size(140, 18);
			this.labelResult.TabIndex = 17;
			this.labelResult.Text = "Result";
			this.labelResult.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textResult
			//
			this.textResult.Location = new System.Drawing.Point(155, 341);
			this.textResult.Name = "textResult";
			this.textResult.ReadOnly = true;
			this.textResult.Size = new System.Drawing.Size(693, 20);
			this.textResult.TabIndex = 18;
			//
			// labelHint
			//
			this.labelHint.Location = new System.Drawing.Point(12, 366);
			this.labelHint.Name = "labelHint";
			this.labelHint.Size = new System.Drawing.Size(836, 18);
			this.labelHint.TabIndex = 19;
			this.labelHint.Text = "Double-click a patient to close these windows and switch the Chart to that patient.";
			//
			// gridAudit
			//
			this.gridAudit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridAudit.Location = new System.Drawing.Point(12, 388);
			this.gridAudit.Name = "gridAudit";
			this.gridAudit.Size = new System.Drawing.Size(836, 270);
			this.gridAudit.TabIndex = 20;
			this.gridAudit.Title = "Patients";
			this.gridAudit.TranslationName = "TableEhrMeasureCalcAudit";
			this.gridAudit.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridAudit_CellDoubleClick);
			//
			// butClose
			//
			this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butClose.Location = new System.Drawing.Point(773, 666);
			this.butClose.Name = "butClose";
			this.butClose.Size = new System.Drawing.Size(75, 24);
			this.butClose.TabIndex = 21;
			this.butClose.Text = "&Close";
			this.butClose.Click += new System.EventHandler(this.butClose_Click);
			//
			// FormEhrMeasureCalcDetail
			//
			this.ClientSize = new System.Drawing.Size(860, 700);
			this.Controls.Add(this.butClose);
			this.Controls.Add(this.gridAudit);
			this.Controls.Add(this.labelHint);
			this.Controls.Add(this.textResult);
			this.Controls.Add(this.labelResult);
			this.Controls.Add(this.textExclusionCount);
			this.Controls.Add(this.labelExclusionCount);
			this.Controls.Add(this.textExclusionExplain);
			this.Controls.Add(this.labelExclusionExplain);
			this.Controls.Add(this.textDenominatorExplain);
			this.Controls.Add(this.labelDenominatorExplain);
			this.Controls.Add(this.textNumeratorExplain);
			this.Controls.Add(this.labelNumeratorExplain);
			this.Controls.Add(this.textMeasure);
			this.Controls.Add(this.labelMeasure);
			this.Controls.Add(this.textObjective);
			this.Controls.Add(this.labelObjective);
			this.Controls.Add(this.textPeriod);
			this.Controls.Add(this.labelPeriod);
			this.Controls.Add(this.textMeasureName);
			this.Controls.Add(this.labelMeasureName);
			this.Controls.Add(this.labelInternalReview);
			this.Name = "FormEhrMeasureCalcDetail";
			this.Text = "Measure Detail";
			this.Load += new System.EventHandler(this.FormEhrMeasureCalcDetail_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelInternalReview;
		private System.Windows.Forms.Label labelMeasureName;
		private System.Windows.Forms.TextBox textMeasureName;
		private System.Windows.Forms.Label labelPeriod;
		private System.Windows.Forms.TextBox textPeriod;
		private System.Windows.Forms.Label labelObjective;
		private System.Windows.Forms.TextBox textObjective;
		private System.Windows.Forms.Label labelMeasure;
		private System.Windows.Forms.TextBox textMeasure;
		private System.Windows.Forms.Label labelNumeratorExplain;
		private System.Windows.Forms.TextBox textNumeratorExplain;
		private System.Windows.Forms.Label labelDenominatorExplain;
		private System.Windows.Forms.TextBox textDenominatorExplain;
		private System.Windows.Forms.Label labelExclusionExplain;
		private System.Windows.Forms.TextBox textExclusionExplain;
		private System.Windows.Forms.Label labelExclusionCount;
		private System.Windows.Forms.TextBox textExclusionCount;
		private System.Windows.Forms.Label labelResult;
		private System.Windows.Forms.TextBox textResult;
		private System.Windows.Forms.Label labelHint;
		private OpenPmsDental.UI.GridOD gridAudit;
		private OpenPmsDental.UI.Button butClose;
	}
}
