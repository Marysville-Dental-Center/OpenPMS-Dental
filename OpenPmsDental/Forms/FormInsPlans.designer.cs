using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormInsPlans {
		private System.ComponentModel.IContainer components = null;// Required designer variable.

		///<summary></summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if(components != null){
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		private void InitializeComponent(){
			this.textPlanNum = new System.Windows.Forms.TextBox();
			this.labelInsPlanID = new System.Windows.Forms.Label();
			this.butGetAll = new OpenPmsDental.UI.Button();
			this.butHide = new OpenPmsDental.UI.Button();
			this.checkShowHidden = new OpenPmsDental.UI.CheckBox();
			this.textTrojanID = new System.Windows.Forms.TextBox();
			this.labelTrojanID = new System.Windows.Forms.Label();
			this.butMerge = new OpenPmsDental.UI.Button();
			this.textGroupNum = new System.Windows.Forms.TextBox();
			this.labelGroupNum = new System.Windows.Forms.Label();
			this.textGroupName = new System.Windows.Forms.TextBox();
			this.labelGroupName = new System.Windows.Forms.Label();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.textCarrier = new System.Windows.Forms.TextBox();
			this.labelCarrier = new System.Windows.Forms.Label();
			this.textEmployer = new System.Windows.Forms.TextBox();
			this.labelEmployer = new System.Windows.Forms.Label();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.radioOrderCarrier = new System.Windows.Forms.RadioButton();
			this.radioOrderEmp = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textPlanNum
			// 
			this.textPlanNum.AutoSize = false;
			this.textPlanNum.Location = new System.Drawing.Point(542, 50);
			this.textPlanNum.Name = "textPlanNum";
			this.textPlanNum.Size = new System.Drawing.Size(140, 23);
			this.textPlanNum.TabIndex = 31;
			// 
			// labelInsPlanID
			// 
			this.labelInsPlanID.Location = new System.Drawing.Point(468, 50);
			this.labelInsPlanID.Name = "labelInsPlanID";
			this.labelInsPlanID.Size = new System.Drawing.Size(68, 23);
			this.labelInsPlanID.TabIndex = 32;
			this.labelInsPlanID.Text = "Ins Plan ID";
			this.labelInsPlanID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butGetAll
			// 
			this.butGetAll.Location = new System.Drawing.Point(851, 23);
			this.butGetAll.Name = "butGetAll";
			this.butGetAll.Size = new System.Drawing.Size(75, 24);
			this.butGetAll.TabIndex = 30;
			this.butGetAll.Text = "Get All";
			this.butGetAll.Click += new System.EventHandler(this.butGetAll_Click);
			// 
			// butHide
			// 
			this.butHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butHide.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butHide.Location = new System.Drawing.Point(88, 680);
			this.butHide.Name = "butHide";
			this.butHide.Size = new System.Drawing.Size(84, 24);
			this.butHide.TabIndex = 28;
			this.butHide.Text = "Hide Unused";
			this.butHide.Click += new System.EventHandler(this.butHide_Click);
			// 
			// checkShowHidden
			// 
			this.checkShowHidden.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkShowHidden.Location = new System.Drawing.Point(851, 51);
			this.checkShowHidden.Name = "checkShowHidden";
			this.checkShowHidden.Size = new System.Drawing.Size(95, 20);
			this.checkShowHidden.TabIndex = 27;
			this.checkShowHidden.Text = "Show Hidden";
			// 
			// textTrojanID
			// 
			this.textTrojanID.AutoSize = false;
			this.textTrojanID.Location = new System.Drawing.Point(542, 23);
			this.textTrojanID.Name = "textTrojanID";
			this.textTrojanID.Size = new System.Drawing.Size(140, 23);
			this.textTrojanID.TabIndex = 25;
			// 
			// labelTrojanID
			// 
			this.labelTrojanID.Location = new System.Drawing.Point(476, 23);
			this.labelTrojanID.Name = "labelTrojanID";
			this.labelTrojanID.Size = new System.Drawing.Size(60, 23);
			this.labelTrojanID.TabIndex = 26;
			this.labelTrojanID.Text = "Trojan ID";
			this.labelTrojanID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butMerge
			// 
			this.butMerge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butMerge.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butMerge.Location = new System.Drawing.Point(8, 680);
			this.butMerge.Name = "butMerge";
			this.butMerge.Size = new System.Drawing.Size(74, 24);
			this.butMerge.TabIndex = 24;
			this.butMerge.Text = "Combine";
			this.butMerge.Click += new System.EventHandler(this.butMerge_Click);
			// 
			// textGroupNum
			// 
			this.textGroupNum.AutoSize = false;
			this.textGroupNum.Location = new System.Drawing.Point(90, 50);
			this.textGroupNum.Name = "textGroupNum";
			this.textGroupNum.Size = new System.Drawing.Size(140, 23);
			this.textGroupNum.TabIndex = 20;
			// 
			// labelGroupNum
			// 
			this.labelGroupNum.Location = new System.Drawing.Point(8, 50);
			this.labelGroupNum.Name = "labelGroupNum";
			this.labelGroupNum.Size = new System.Drawing.Size(76, 23);
			this.labelGroupNum.TabIndex = 23;
			this.labelGroupNum.Text = "Group Num";
			this.labelGroupNum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textGroupName
			// 
			this.textGroupName.AutoSize = false;
			this.textGroupName.Location = new System.Drawing.Point(322, 50);
			this.textGroupName.Name = "textGroupName";
			this.textGroupName.Size = new System.Drawing.Size(140, 23);
			this.textGroupName.TabIndex = 21;
			// 
			// labelGroupName
			// 
			this.labelGroupName.Location = new System.Drawing.Point(236, 50);
			this.labelGroupName.Name = "labelGroupName";
			this.labelGroupName.Size = new System.Drawing.Size(80, 23);
			this.labelGroupName.TabIndex = 22;
			this.labelGroupName.Text = "Group Name";
			this.labelGroupName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// gridMain
			// 
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.AllowSortingByColumn = true;
			this.gridMain.HScrollVisible = true;
			this.gridMain.Location = new System.Drawing.Point(8, 98);
			this.gridMain.Name = "gridMain";
			this.gridMain.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridMain.Size = new System.Drawing.Size(971, 578);
			this.gridMain.TabIndex = 19;
			this.gridMain.Title = "Insurance Plans";
			this.gridMain.TranslationName = "TableInsurancePlans";
			this.gridMain.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			// 
			// textCarrier
			// 
			this.textCarrier.AutoSize = false;
			this.textCarrier.Location = new System.Drawing.Point(322, 23);
			this.textCarrier.Name = "textCarrier";
			this.textCarrier.Size = new System.Drawing.Size(140, 23);
			this.textCarrier.TabIndex = 0;
			// 
			// labelCarrier
			// 
			this.labelCarrier.Location = new System.Drawing.Point(268, 23);
			this.labelCarrier.Name = "labelCarrier";
			this.labelCarrier.Size = new System.Drawing.Size(48, 23);
			this.labelCarrier.TabIndex = 17;
			this.labelCarrier.Text = "Carrier";
			this.labelCarrier.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textEmployer
			// 
			this.textEmployer.AutoSize = false;
			this.textEmployer.Location = new System.Drawing.Point(90, 23);
			this.textEmployer.Name = "textEmployer";
			this.textEmployer.Size = new System.Drawing.Size(140, 23);
			this.textEmployer.TabIndex = 1;
			// 
			// labelEmployer
			// 
			this.labelEmployer.Location = new System.Drawing.Point(20, 23);
			this.labelEmployer.Name = "labelEmployer";
			this.labelEmployer.Size = new System.Drawing.Size(64, 23);
			this.labelEmployer.TabIndex = 15;
			this.labelEmployer.Text = "Employer";
			this.labelEmployer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.radioOrderCarrier);
			this.groupBox2.Controls.Add(this.radioOrderEmp);
			this.groupBox2.Location = new System.Drawing.Point(690, 23);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(157, 51);
			this.groupBox2.TabIndex = 2;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Order By";
			// 
			// radioOrderCarrier
			// 
			this.radioOrderCarrier.Location = new System.Drawing.Point(89, 23);
			this.radioOrderCarrier.Name = "radioOrderCarrier";
			this.radioOrderCarrier.Size = new System.Drawing.Size(60, 20);
			this.radioOrderCarrier.TabIndex = 1;
			this.radioOrderCarrier.Text = "Carrier";
			this.radioOrderCarrier.Click += new System.EventHandler(this.radioOrderCarrier_Click);
			// 
			// radioOrderEmp
			// 
			this.radioOrderEmp.Checked = true;
			this.radioOrderEmp.Location = new System.Drawing.Point(8, 23);
			this.radioOrderEmp.Name = "radioOrderEmp";
			this.radioOrderEmp.Size = new System.Drawing.Size(75, 20);
			this.radioOrderEmp.TabIndex = 0;
			this.radioOrderEmp.TabStop = true;
			this.radioOrderEmp.Text = "Employer";
			this.radioOrderEmp.Click += new System.EventHandler(this.radioOrderEmp_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.textEmployer);
			this.groupBox1.Controls.Add(this.butGetAll);
			this.groupBox1.Controls.Add(this.textPlanNum);
			this.groupBox1.Controls.Add(this.labelEmployer);
			this.groupBox1.Controls.Add(this.checkShowHidden);
			this.groupBox1.Controls.Add(this.labelInsPlanID);
			this.groupBox1.Controls.Add(this.textCarrier);
			this.groupBox1.Controls.Add(this.labelCarrier);
			this.groupBox1.Controls.Add(this.groupBox2);
			this.groupBox1.Controls.Add(this.textGroupName);
			this.groupBox1.Controls.Add(this.textGroupNum);
			this.groupBox1.Controls.Add(this.textTrojanID);
			this.groupBox1.Controls.Add(this.labelGroupName);
			this.groupBox1.Controls.Add(this.labelTrojanID);
			this.groupBox1.Controls.Add(this.labelGroupNum);
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(971, 82);
			this.groupBox1.TabIndex = 33;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Filter";
			// 
			// FormInsPlans
			// 
			this.ClientSize = new System.Drawing.Size(987, 712);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.butHide);
			this.Controls.Add(this.butMerge);
			this.Controls.Add(this.gridMain);
			this.Name = "FormInsPlans";
			this.ShowInTaskbar = false;
			this.Text = "Insurance Plans";
			this.Load += new System.EventHandler(this.FormInsTemplates_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion
		private OpenPmsDental.UI.GroupBox groupBox2;
		private System.Windows.Forms.RadioButton radioOrderCarrier;
		private System.Windows.Forms.RadioButton radioOrderEmp;
		private Label labelEmployer;
		private TextBox textEmployer;
		private TextBox textCarrier;
		private Label labelCarrier;
		private OpenPmsDental.UI.GridOD gridMain;
		private TextBox textGroupNum;
		private Label labelGroupNum;
		private TextBox textGroupName;
		private Label labelGroupName;
		private OpenPmsDental.UI.Button butMerge;
		private TextBox textTrojanID;
		private Label labelTrojanID;
		private OpenPmsDental.UI.CheckBox checkShowHidden;
		private OpenPmsDental.UI.Button butHide;
		private UI.Button butGetAll;
		private TextBox textPlanNum;
		private Label labelInsPlanID;
		private OpenPmsDental.UI.GroupBox groupBox1;
	}
}
