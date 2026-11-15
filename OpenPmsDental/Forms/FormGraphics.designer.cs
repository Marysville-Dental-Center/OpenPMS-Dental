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
			this.groupToothChart = new OpenPmsDental.UI.GroupBox();
			this.radio3DChart = new System.Windows.Forms.RadioButton();
			this.radioSimpleChart = new System.Windows.Forms.RadioButton();
			this.checkDrawInSoftware = new OpenPmsDental.UI.CheckBox();
			this.labelDrawInSoftware = new System.Windows.Forms.Label();
			this.groupPreview = new OpenPmsDental.UI.GroupBox();
			this.toothChartWrapper = new SparksToothChart.ToothChartWrapper();
			this.labelPreviewStatus = new System.Windows.Forms.Label();
			this.labelPreview = new System.Windows.Forms.Label();
			this.butSave = new OpenPmsDental.UI.Button();
			this.groupToothChart.SuspendLayout();
			this.groupPreview.SuspendLayout();
			this.SuspendLayout();
			//
			// groupToothChart
			//
			this.groupToothChart.Controls.Add(this.radio3DChart);
			this.groupToothChart.Controls.Add(this.radioSimpleChart);
			this.groupToothChart.Location = new System.Drawing.Point(20, 12);
			this.groupToothChart.Name = "groupToothChart";
			this.groupToothChart.Size = new System.Drawing.Size(300, 64);
			this.groupToothChart.TabIndex = 2;
			this.groupToothChart.TabStop = false;
			this.groupToothChart.Text = "Tooth Chart";
			//
			// radio3DChart
			//
			this.radio3DChart.Location = new System.Drawing.Point(12, 17);
			this.radio3DChart.Name = "radio3DChart";
			this.radio3DChart.Size = new System.Drawing.Size(270, 19);
			this.radio3DChart.TabIndex = 0;
			this.radio3DChart.TabStop = true;
			this.radio3DChart.Text = "3D chart";
			this.radio3DChart.UseVisualStyleBackColor = true;
			this.radio3DChart.Click += new System.EventHandler(this.radio3DChart_Click);
			//
			// radioSimpleChart
			//
			this.radioSimpleChart.Location = new System.Drawing.Point(12, 38);
			this.radioSimpleChart.Name = "radioSimpleChart";
			this.radioSimpleChart.Size = new System.Drawing.Size(270, 19);
			this.radioSimpleChart.TabIndex = 1;
			this.radioSimpleChart.TabStop = true;
			this.radioSimpleChart.Text = "Simple 2D chart";
			this.radioSimpleChart.UseVisualStyleBackColor = true;
			this.radioSimpleChart.Click += new System.EventHandler(this.radioSimpleChart_Click);
			//
			// checkDrawInSoftware
			//
			this.checkDrawInSoftware.Location = new System.Drawing.Point(20, 84);
			this.checkDrawInSoftware.Name = "checkDrawInSoftware";
			this.checkDrawInSoftware.Size = new System.Drawing.Size(430, 18);
			this.checkDrawInSoftware.TabIndex = 3;
			this.checkDrawInSoftware.Text = "Draw in software";
			this.checkDrawInSoftware.Click += new System.EventHandler(this.checkDrawInSoftware_Click);
			//
			// labelDrawInSoftware
			//
			this.labelDrawInSoftware.Location = new System.Drawing.Point(36, 103);
			this.labelDrawInSoftware.Name = "labelDrawInSoftware";
			this.labelDrawInSoftware.Size = new System.Drawing.Size(544, 30);
			this.labelDrawInSoftware.TabIndex = 4;
			this.labelDrawInSoftware.Text = resources.GetString("labelDrawInSoftware.Text");
			//
			// groupPreview
			//
			this.groupPreview.Controls.Add(this.toothChartWrapper);
			this.groupPreview.Controls.Add(this.labelPreviewStatus);
			this.groupPreview.Controls.Add(this.labelPreview);
			this.groupPreview.Location = new System.Drawing.Point(20, 140);
			this.groupPreview.Name = "groupPreview";
			this.groupPreview.Size = new System.Drawing.Size(430, 378);
			this.groupPreview.TabIndex = 5;
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
			this.butSave.Location = new System.Drawing.Point(505, 492);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 26);
			this.butSave.TabIndex = 1;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			//
			// FormGraphics
			//
			this.ClientSize = new System.Drawing.Size(600, 530);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.groupPreview);
			this.Controls.Add(this.labelDrawInSoftware);
			this.Controls.Add(this.checkDrawInSoftware);
			this.Controls.Add(this.groupToothChart);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormGraphics";
			this.ShowInTaskbar = false;
			this.Text = "Graphics Preferences";
			this.Load += new System.EventHandler(this.FormGraphics_Load);
			this.groupToothChart.ResumeLayout(false);
			this.groupPreview.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private OpenPmsDental.UI.GroupBox groupToothChart;
		private RadioButton radio3DChart;
		private RadioButton radioSimpleChart;
		private OpenPmsDental.UI.CheckBox checkDrawInSoftware;
		private Label labelDrawInSoftware;
		private OpenPmsDental.UI.GroupBox groupPreview;
		private SparksToothChart.ToothChartWrapper toothChartWrapper;
		private Label labelPreviewStatus;
		private Label labelPreview;
	}
}
