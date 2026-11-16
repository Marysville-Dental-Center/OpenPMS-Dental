using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormGraphics {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGraphics));
			this.labelIntro = new System.Windows.Forms.Label();
			this.groupChart3DAll = new OpenPmsDental.UI.GroupBox();
			this.labelChart3DAll = new System.Windows.Forms.Label();
			this.radioChart3DAllUse = new System.Windows.Forms.RadioButton();
			this.radioChart3DAllDontUse = new System.Windows.Forms.RadioButton();
			this.groupChart3DThis = new OpenPmsDental.UI.GroupBox();
			this.radioChart3DThisGlobal = new System.Windows.Forms.RadioButton();
			this.radioChart3DThisUse = new System.Windows.Forms.RadioButton();
			this.radioChart3DThisDontUse = new System.Windows.Forms.RadioButton();
			this.groupStatus = new OpenPmsDental.UI.GroupBox();
			this.radioStatusAvailable = new System.Windows.Forms.RadioButton();
			this.radioStatusFellBack = new System.Windows.Forms.RadioButton();
			this.groupOlder = new OpenPmsDental.UI.GroupBox();
			this.radioSimpleChart = new System.Windows.Forms.RadioButton();
			this.labelOlder = new System.Windows.Forms.Label();
			this.groupOptions = new OpenPmsDental.UI.GroupBox();
			this.labelOptions = new System.Windows.Forms.Label();
			this.checkDrawInSoftware = new OpenPmsDental.UI.CheckBox();
			this.labelDrawInSoftware = new System.Windows.Forms.Label();
			this.groupPreview = new OpenPmsDental.UI.GroupBox();
			this.toothChartWrapper = new SparksToothChart.ToothChartWrapper();
			this.labelPreviewStatus = new System.Windows.Forms.Label();
			this.labelPreview = new System.Windows.Forms.Label();
			this.butSave = new OpenPmsDental.UI.Button();
			this.groupChart3DAll.SuspendLayout();
			this.groupChart3DThis.SuspendLayout();
			this.groupStatus.SuspendLayout();
			this.groupOlder.SuspendLayout();
			this.groupOptions.SuspendLayout();
			this.groupPreview.SuspendLayout();
			this.SuspendLayout();
			//
			// labelIntro
			//
			this.labelIntro.Location = new System.Drawing.Point(27, 9);
			this.labelIntro.Name = "labelIntro";
			this.labelIntro.Size = new System.Drawing.Size(834, 44);
			this.labelIntro.TabIndex = 0;
			this.labelIntro.Text = resources.GetString("labelIntro.Text");
			//
			// groupChart3DAll
			//
			this.groupChart3DAll.Controls.Add(this.labelChart3DAll);
			this.groupChart3DAll.Controls.Add(this.radioChart3DAllUse);
			this.groupChart3DAll.Controls.Add(this.radioChart3DAllDontUse);
			this.groupChart3DAll.Location = new System.Drawing.Point(28, 58);
			this.groupChart3DAll.Name = "groupChart3DAll";
			this.groupChart3DAll.Size = new System.Drawing.Size(548, 96);
			this.groupChart3DAll.TabIndex = 1;
			this.groupChart3DAll.TabStop = false;
			this.groupChart3DAll.Text = "3D Tooth Chart on all computers";
			//
			// labelChart3DAll
			//
			this.labelChart3DAll.Location = new System.Drawing.Point(12, 19);
			this.labelChart3DAll.Name = "labelChart3DAll";
			this.labelChart3DAll.Size = new System.Drawing.Size(520, 16);
			this.labelChart3DAll.TabIndex = 0;
			this.labelChart3DAll.Text = "This option affects all computers in the office.  All other options are specific to this computer.";
			//
			// radioChart3DAllUse
			//
			this.radioChart3DAllUse.Location = new System.Drawing.Point(15, 38);
			this.radioChart3DAllUse.Name = "radioChart3DAllUse";
			this.radioChart3DAllUse.Size = new System.Drawing.Size(520, 19);
			this.radioChart3DAllUse.TabIndex = 1;
			this.radioChart3DAllUse.TabStop = true;
			this.radioChart3DAllUse.Text = "Use if available.  Advantage is the detailed 3D tooth chart and the graphical perio chart.";
			this.radioChart3DAllUse.UseVisualStyleBackColor = true;
			this.radioChart3DAllUse.Click += new System.EventHandler(this.radioChart3D_Click);
			//
			// radioChart3DAllDontUse
			//
			this.radioChart3DAllDontUse.Location = new System.Drawing.Point(15, 58);
			this.radioChart3DAllDontUse.Name = "radioChart3DAllDontUse";
			this.radioChart3DAllDontUse.Size = new System.Drawing.Size(520, 31);
			this.radioChart3DAllDontUse.TabIndex = 2;
			this.radioChart3DAllDontUse.TabStop = true;
			this.radioChart3DAllDontUse.Text = "Do not use.  Advantage is that older or underpowered hardware might work better.  This is an easy first step for troubleshooting certain graphics problems.";
			this.radioChart3DAllDontUse.UseVisualStyleBackColor = true;
			this.radioChart3DAllDontUse.Click += new System.EventHandler(this.radioChart3D_Click);
			//
			// groupChart3DThis
			//
			this.groupChart3DThis.Controls.Add(this.radioChart3DThisGlobal);
			this.groupChart3DThis.Controls.Add(this.radioChart3DThisUse);
			this.groupChart3DThis.Controls.Add(this.radioChart3DThisDontUse);
			this.groupChart3DThis.Location = new System.Drawing.Point(28, 159);
			this.groupChart3DThis.Name = "groupChart3DThis";
			this.groupChart3DThis.Size = new System.Drawing.Size(180, 75);
			this.groupChart3DThis.TabIndex = 2;
			this.groupChart3DThis.TabStop = false;
			this.groupChart3DThis.Text = "3D Tooth Chart";
			//
			// radioChart3DThisGlobal
			//
			this.radioChart3DThisGlobal.Location = new System.Drawing.Point(15, 17);
			this.radioChart3DThisGlobal.Name = "radioChart3DThisGlobal";
			this.radioChart3DThisGlobal.Size = new System.Drawing.Size(159, 19);
			this.radioChart3DThisGlobal.TabIndex = 0;
			this.radioChart3DThisGlobal.TabStop = true;
			this.radioChart3DThisGlobal.Text = "Use the global setting";
			this.radioChart3DThisGlobal.UseVisualStyleBackColor = true;
			this.radioChart3DThisGlobal.Click += new System.EventHandler(this.radioChart3D_Click);
			//
			// radioChart3DThisUse
			//
			this.radioChart3DThisUse.Location = new System.Drawing.Point(15, 35);
			this.radioChart3DThisUse.Name = "radioChart3DThisUse";
			this.radioChart3DThisUse.Size = new System.Drawing.Size(159, 19);
			this.radioChart3DThisUse.TabIndex = 1;
			this.radioChart3DThisUse.TabStop = true;
			this.radioChart3DThisUse.Text = "Use if available";
			this.radioChart3DThisUse.UseVisualStyleBackColor = true;
			this.radioChart3DThisUse.Click += new System.EventHandler(this.radioChart3D_Click);
			//
			// radioChart3DThisDontUse
			//
			this.radioChart3DThisDontUse.Location = new System.Drawing.Point(15, 52);
			this.radioChart3DThisDontUse.Name = "radioChart3DThisDontUse";
			this.radioChart3DThisDontUse.Size = new System.Drawing.Size(159, 19);
			this.radioChart3DThisDontUse.TabIndex = 2;
			this.radioChart3DThisDontUse.TabStop = true;
			this.radioChart3DThisDontUse.Text = "Do not use";
			this.radioChart3DThisDontUse.UseVisualStyleBackColor = true;
			this.radioChart3DThisDontUse.Click += new System.EventHandler(this.radioChart3D_Click);
			//
			// groupStatus
			//
			this.groupStatus.Controls.Add(this.radioStatusAvailable);
			this.groupStatus.Controls.Add(this.radioStatusFellBack);
			this.groupStatus.Location = new System.Drawing.Point(216, 159);
			this.groupStatus.Name = "groupStatus";
			this.groupStatus.Size = new System.Drawing.Size(360, 75);
			this.groupStatus.TabIndex = 3;
			this.groupStatus.TabStop = false;
			this.groupStatus.Text = "3D Tooth Chart status on this computer";
			//
			// radioStatusAvailable
			//
			this.radioStatusAvailable.AutoCheck = false;
			this.radioStatusAvailable.Location = new System.Drawing.Point(12, 16);
			this.radioStatusAvailable.Name = "radioStatusAvailable";
			this.radioStatusAvailable.Size = new System.Drawing.Size(340, 19);
			this.radioStatusAvailable.TabIndex = 0;
			this.radioStatusAvailable.Text = "Available";
			this.radioStatusAvailable.UseVisualStyleBackColor = true;
			//
			// radioStatusFellBack
			//
			this.radioStatusFellBack.AutoCheck = false;
			this.radioStatusFellBack.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.radioStatusFellBack.Location = new System.Drawing.Point(12, 35);
			this.radioStatusFellBack.Name = "radioStatusFellBack";
			this.radioStatusFellBack.Size = new System.Drawing.Size(340, 36);
			this.radioStatusFellBack.TabIndex = 1;
			this.radioStatusFellBack.Text = "Fell back: the 3D tooth chart could not start on this computer, so the Simple 2D chart is used.";
			this.radioStatusFellBack.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.radioStatusFellBack.UseVisualStyleBackColor = true;
			//
			// groupOlder
			//
			this.groupOlder.Controls.Add(this.radioSimpleChart);
			this.groupOlder.Controls.Add(this.labelOlder);
			this.groupOlder.Location = new System.Drawing.Point(584, 159);
			this.groupOlder.Name = "groupOlder";
			this.groupOlder.Size = new System.Drawing.Size(277, 75);
			this.groupOlder.TabIndex = 4;
			this.groupOlder.TabStop = false;
			this.groupOlder.Text = "Older Tooth Chart Technology";
			//
			// radioSimpleChart
			//
			this.radioSimpleChart.Location = new System.Drawing.Point(15, 17);
			this.radioSimpleChart.Name = "radioSimpleChart";
			this.radioSimpleChart.Size = new System.Drawing.Size(250, 19);
			this.radioSimpleChart.TabIndex = 0;
			this.radioSimpleChart.TabStop = true;
			this.radioSimpleChart.Text = "Simple 2D (pie shapes)";
			this.radioSimpleChart.UseVisualStyleBackColor = true;
			//
			// labelOlder
			//
			this.labelOlder.Location = new System.Drawing.Point(15, 38);
			this.labelOlder.Name = "labelOlder";
			this.labelOlder.Size = new System.Drawing.Size(256, 32);
			this.labelOlder.TabIndex = 1;
			this.labelOlder.Text = "Used when the 3D tooth chart is not used.  The older 3D technologies were retired.";
			//
			// groupOptions
			//
			this.groupOptions.Controls.Add(this.labelOptions);
			this.groupOptions.Controls.Add(this.checkDrawInSoftware);
			this.groupOptions.Controls.Add(this.labelDrawInSoftware);
			this.groupOptions.Location = new System.Drawing.Point(28, 240);
			this.groupOptions.Name = "groupOptions";
			this.groupOptions.Size = new System.Drawing.Size(395, 150);
			this.groupOptions.TabIndex = 5;
			this.groupOptions.TabStop = false;
			this.groupOptions.Text = "Options For The 3D Tooth Chart";
			//
			// labelOptions
			//
			this.labelOptions.Location = new System.Drawing.Point(9, 18);
			this.labelOptions.Name = "labelOptions";
			this.labelOptions.Size = new System.Drawing.Size(378, 30);
			this.labelOptions.TabIndex = 0;
			this.labelOptions.Text = "Most users will never need to change any of these options.  These are only used when the 3D tooth chart is not working properly.";
			//
			// checkDrawInSoftware
			//
			this.checkDrawInSoftware.Location = new System.Drawing.Point(12, 54);
			this.checkDrawInSoftware.Name = "checkDrawInSoftware";
			this.checkDrawInSoftware.Size = new System.Drawing.Size(370, 18);
			this.checkDrawInSoftware.TabIndex = 1;
			this.checkDrawInSoftware.Text = "Draw in software";
			this.checkDrawInSoftware.Click += new System.EventHandler(this.checkDrawInSoftware_Click);
			//
			// labelDrawInSoftware
			//
			this.labelDrawInSoftware.Location = new System.Drawing.Point(28, 74);
			this.labelDrawInSoftware.Name = "labelDrawInSoftware";
			this.labelDrawInSoftware.Size = new System.Drawing.Size(359, 70);
			this.labelDrawInSoftware.TabIndex = 2;
			this.labelDrawInSoftware.Text = resources.GetString("labelDrawInSoftware.Text");
			//
			// groupPreview
			//
			this.groupPreview.Controls.Add(this.toothChartWrapper);
			this.groupPreview.Controls.Add(this.labelPreviewStatus);
			this.groupPreview.Controls.Add(this.labelPreview);
			this.groupPreview.Location = new System.Drawing.Point(431, 240);
			this.groupPreview.Name = "groupPreview";
			this.groupPreview.Size = new System.Drawing.Size(430, 378);
			this.groupPreview.TabIndex = 6;
			this.groupPreview.TabStop = false;
			this.groupPreview.Text = "Preview";
			//
			// toothChartWrapper
			//
			this.toothChartWrapper.Location = new System.Drawing.Point(10, 20);
			this.toothChartWrapper.Name = "toothChartWrapper";
			this.toothChartWrapper.Size = new System.Drawing.Size(410, 307);
			this.toothChartWrapper.TabIndex = 0;
			//
			// labelPreviewStatus
			//
			this.labelPreviewStatus.Location = new System.Drawing.Point(10, 331);
			this.labelPreviewStatus.Name = "labelPreviewStatus";
			this.labelPreviewStatus.Size = new System.Drawing.Size(410, 18);
			this.labelPreviewStatus.TabIndex = 1;
			//
			// labelPreview
			//
			this.labelPreview.Location = new System.Drawing.Point(10, 351);
			this.labelPreview.Name = "labelPreview";
			this.labelPreview.Size = new System.Drawing.Size(410, 18);
			this.labelPreview.TabIndex = 2;
			this.labelPreview.Text = "A sample chart with made-up work, drawn by this computer.";
			//
			// butSave
			//
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(786, 646);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 26);
			this.butSave.TabIndex = 7;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			//
			// FormGraphics
			//
			this.ClientSize = new System.Drawing.Size(892, 683);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.groupPreview);
			this.Controls.Add(this.groupOptions);
			this.Controls.Add(this.groupOlder);
			this.Controls.Add(this.groupStatus);
			this.Controls.Add(this.groupChart3DThis);
			this.Controls.Add(this.groupChart3DAll);
			this.Controls.Add(this.labelIntro);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormGraphics";
			this.ShowInTaskbar = false;
			this.Text = "Graphics Preferences";
			this.Load += new System.EventHandler(this.FormGraphics_Load);
			this.groupChart3DAll.ResumeLayout(false);
			this.groupChart3DThis.ResumeLayout(false);
			this.groupStatus.ResumeLayout(false);
			this.groupOlder.ResumeLayout(false);
			this.groupOptions.ResumeLayout(false);
			this.groupPreview.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
		private System.Windows.Forms.Label labelIntro;
		private OpenPmsDental.UI.GroupBox groupChart3DAll;
		private Label labelChart3DAll;
		private RadioButton radioChart3DAllUse;
		private RadioButton radioChart3DAllDontUse;
		private OpenPmsDental.UI.GroupBox groupChart3DThis;
		private RadioButton radioChart3DThisGlobal;
		private RadioButton radioChart3DThisUse;
		private RadioButton radioChart3DThisDontUse;
		private OpenPmsDental.UI.GroupBox groupStatus;
		private RadioButton radioStatusAvailable;
		private RadioButton radioStatusFellBack;
		private OpenPmsDental.UI.GroupBox groupOlder;
		private RadioButton radioSimpleChart;
		private Label labelOlder;
		private OpenPmsDental.UI.GroupBox groupOptions;
		private Label labelOptions;
		private OpenPmsDental.UI.CheckBox checkDrawInSoftware;
		private Label labelDrawInSoftware;
		private OpenPmsDental.UI.GroupBox groupPreview;
		private SparksToothChart.ToothChartWrapper toothChartWrapper;
		private Label labelPreviewStatus;
		private Label labelPreview;
		private OpenPmsDental.UI.Button butSave;
	}
}
