using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormChartView {
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
			this.label2 = new System.Windows.Forms.Label();
			this.listAvailable = new OpenPmsDental.UI.ListBox();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox7 = new OpenPmsDental.UI.GroupBox();
			this.checkCommSuperFamily = new OpenPmsDental.UI.CheckBox();
			this.checkSheets = new OpenPmsDental.UI.CheckBox();
			this.checkTasks = new OpenPmsDental.UI.CheckBox();
			this.checkEmail = new OpenPmsDental.UI.CheckBox();
			this.checkCommFamily = new OpenPmsDental.UI.CheckBox();
			this.checkAppt = new OpenPmsDental.UI.CheckBox();
			this.checkLabCase = new OpenPmsDental.UI.CheckBox();
			this.checkRx = new OpenPmsDental.UI.CheckBox();
			this.checkComm = new OpenPmsDental.UI.CheckBox();
			this.groupBox6 = new OpenPmsDental.UI.GroupBox();
			this.checkShowCn = new OpenPmsDental.UI.CheckBox();
			this.checkShowE = new OpenPmsDental.UI.CheckBox();
			this.checkShowR = new OpenPmsDental.UI.CheckBox();
			this.checkShowC = new OpenPmsDental.UI.CheckBox();
			this.checkShowTP = new OpenPmsDental.UI.CheckBox();
			this.checkShowTeeth = new OpenPmsDental.UI.CheckBox();
			this.checkNotes = new OpenPmsDental.UI.CheckBox();
			this.checkAudit = new OpenPmsDental.UI.CheckBox();
			this.textBoxViewDesc = new System.Windows.Forms.TextBox();
			this.groupBoxProperties = new OpenPmsDental.UI.GroupBox();
			this.checkTPChart = new OpenPmsDental.UI.CheckBox();
			this.butShowNone = new OpenPmsDental.UI.Button();
			this.butShowAll = new OpenPmsDental.UI.Button();
			this.labelDescription = new System.Windows.Forms.Label();
			this.listProcStatusCodes = new OpenPmsDental.UI.ListBox();
			this.labelProcStatus = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.comboDatesShowing = new OpenPmsDental.UI.ComboBox();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.butSave = new OpenPmsDental.UI.Button();
			this.butRight = new OpenPmsDental.UI.Button();
			this.butLeft = new OpenPmsDental.UI.Button();
			this.butDown = new OpenPmsDental.UI.Button();
			this.butUp = new OpenPmsDental.UI.Button();
			this.butDefault = new OpenPmsDental.UI.Button();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.groupBox7.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.groupBoxProperties.SuspendLayout();
			this.SuspendLayout();
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(124, 405);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(213, 16);
			this.label2.TabIndex = 5;
			this.label2.Text = "Sets entire list to the default.";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// listAvailable
			// 
			this.listAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.listAvailable.IntegralHeight = false;
			this.listAvailable.Location = new System.Drawing.Point(388, 429);
			this.listAvailable.Name = "listAvailable";
			this.listAvailable.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listAvailable.Size = new System.Drawing.Size(158, 336);
			this.listAvailable.TabIndex = 15;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(385, 404);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(158, 17);
			this.label3.TabIndex = 16;
			this.label3.Text = "Available Fields";
			this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// groupBox7
			// 
			this.groupBox7.Controls.Add(this.checkCommSuperFamily);
			this.groupBox7.Controls.Add(this.checkSheets);
			this.groupBox7.Controls.Add(this.checkTasks);
			this.groupBox7.Controls.Add(this.checkEmail);
			this.groupBox7.Controls.Add(this.checkCommFamily);
			this.groupBox7.Controls.Add(this.checkAppt);
			this.groupBox7.Controls.Add(this.checkLabCase);
			this.groupBox7.Controls.Add(this.checkRx);
			this.groupBox7.Controls.Add(this.checkComm);
			this.groupBox7.Location = new System.Drawing.Point(145, 8);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Size = new System.Drawing.Size(125, 243);
			this.groupBox7.TabIndex = 64;
			this.groupBox7.TabStop = false;
			this.groupBox7.Text = "Object Types";
			// 
			// checkCommSuperFamily
			// 
			this.checkCommSuperFamily.Checked = true;
			this.checkCommSuperFamily.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkCommSuperFamily.Location = new System.Drawing.Point(24, 95);
			this.checkCommSuperFamily.Name = "checkCommSuperFamily";
			this.checkCommSuperFamily.Size = new System.Drawing.Size(92, 20);
			this.checkCommSuperFamily.TabIndex = 20;
			this.checkCommSuperFamily.Text = "Super Family";
			this.checkCommSuperFamily.Click += new System.EventHandler(this.checkCommSuperFamily_Click);
			// 
			// checkSheets
			// 
			this.checkSheets.Checked = true;
			this.checkSheets.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkSheets.Location = new System.Drawing.Point(8, 215);
			this.checkSheets.Name = "checkSheets";
			this.checkSheets.Size = new System.Drawing.Size(58, 20);
			this.checkSheets.TabIndex = 219;
			this.checkSheets.Text = "Sheets";
			this.checkSheets.Click += new System.EventHandler(this.checkSheets_Click);
			// 
			// checkTasks
			// 
			this.checkTasks.Checked = true;
			this.checkTasks.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkTasks.Location = new System.Drawing.Point(8, 119);
			this.checkTasks.Name = "checkTasks";
			this.checkTasks.Size = new System.Drawing.Size(52, 20);
			this.checkTasks.TabIndex = 218;
			this.checkTasks.Text = "Tasks";
			this.checkTasks.Click += new System.EventHandler(this.checkTasks_Click);
			// 
			// checkEmail
			// 
			this.checkEmail.Checked = true;
			this.checkEmail.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkEmail.Location = new System.Drawing.Point(8, 143);
			this.checkEmail.Name = "checkEmail";
			this.checkEmail.Size = new System.Drawing.Size(52, 20);
			this.checkEmail.TabIndex = 217;
			this.checkEmail.Text = "Email";
			this.checkEmail.Click += new System.EventHandler(this.checkEmail_Click);
			// 
			// checkCommFamily
			// 
			this.checkCommFamily.Checked = true;
			this.checkCommFamily.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkCommFamily.Location = new System.Drawing.Point(24, 71);
			this.checkCommFamily.Name = "checkCommFamily";
			this.checkCommFamily.Size = new System.Drawing.Size(57, 20);
			this.checkCommFamily.TabIndex = 20;
			this.checkCommFamily.Text = "Family";
			this.checkCommFamily.Click += new System.EventHandler(this.checkCommFamily_Click);
			// 
			// checkAppt
			// 
			this.checkAppt.Checked = true;
			this.checkAppt.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAppt.Location = new System.Drawing.Point(8, 23);
			this.checkAppt.Name = "checkAppt";
			this.checkAppt.Size = new System.Drawing.Size(100, 20);
			this.checkAppt.TabIndex = 20;
			this.checkAppt.Text = "Appointments";
			this.checkAppt.Click += new System.EventHandler(this.checkAppt_Click);
			// 
			// checkLabCase
			// 
			this.checkLabCase.Checked = true;
			this.checkLabCase.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkLabCase.Location = new System.Drawing.Point(8, 167);
			this.checkLabCase.Name = "checkLabCase";
			this.checkLabCase.Size = new System.Drawing.Size(76, 20);
			this.checkLabCase.TabIndex = 17;
			this.checkLabCase.Text = "Lab Cases";
			this.checkLabCase.Click += new System.EventHandler(this.checkLabCase_Click);
			// 
			// checkRx
			// 
			this.checkRx.Checked = true;
			this.checkRx.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkRx.Location = new System.Drawing.Point(8, 191);
			this.checkRx.Name = "checkRx";
			this.checkRx.Size = new System.Drawing.Size(36, 20);
			this.checkRx.TabIndex = 8;
			this.checkRx.Text = "Rx";
			this.checkRx.Click += new System.EventHandler(this.checkRx_Click);
			// 
			// checkComm
			// 
			this.checkComm.Checked = true;
			this.checkComm.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkComm.Location = new System.Drawing.Point(8, 47);
			this.checkComm.Name = "checkComm";
			this.checkComm.Size = new System.Drawing.Size(76, 20);
			this.checkComm.TabIndex = 16;
			this.checkComm.Text = "Commlog";
			this.checkComm.Click += new System.EventHandler(this.checkComm_Click);
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.checkShowCn);
			this.groupBox6.Controls.Add(this.checkShowE);
			this.groupBox6.Controls.Add(this.checkShowR);
			this.groupBox6.Controls.Add(this.checkShowC);
			this.groupBox6.Controls.Add(this.checkShowTP);
			this.groupBox6.Location = new System.Drawing.Point(8, 8);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(121, 147);
			this.groupBox6.TabIndex = 63;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Procedures";
			// 
			// checkShowCn
			// 
			this.checkShowCn.Checked = true;
			this.checkShowCn.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkShowCn.Location = new System.Drawing.Point(8, 119);
			this.checkShowCn.Name = "checkShowCn";
			this.checkShowCn.Size = new System.Drawing.Size(81, 20);
			this.checkShowCn.TabIndex = 15;
			this.checkShowCn.Text = "Conditions";
			this.checkShowCn.Click += new System.EventHandler(this.checkShowCn_Click);
			// 
			// checkShowE
			// 
			this.checkShowE.Checked = true;
			this.checkShowE.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkShowE.Location = new System.Drawing.Point(8, 71);
			this.checkShowE.Name = "checkShowE";
			this.checkShowE.Size = new System.Drawing.Size(64, 20);
			this.checkShowE.TabIndex = 10;
			this.checkShowE.Text = "Existing";
			this.checkShowE.Click += new System.EventHandler(this.checkShowE_Click);
			// 
			// checkShowR
			// 
			this.checkShowR.Checked = true;
			this.checkShowR.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkShowR.Location = new System.Drawing.Point(8, 95);
			this.checkShowR.Name = "checkShowR";
			this.checkShowR.Size = new System.Drawing.Size(69, 20);
			this.checkShowR.TabIndex = 14;
			this.checkShowR.Text = "Referred";
			this.checkShowR.Click += new System.EventHandler(this.checkShowR_Click);
			// 
			// checkShowC
			// 
			this.checkShowC.Checked = true;
			this.checkShowC.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkShowC.Location = new System.Drawing.Point(8, 47);
			this.checkShowC.Name = "checkShowC";
			this.checkShowC.Size = new System.Drawing.Size(83, 20);
			this.checkShowC.TabIndex = 9;
			this.checkShowC.Text = "Completed";
			this.checkShowC.Click += new System.EventHandler(this.checkShowC_Click);
			// 
			// checkShowTP
			// 
			this.checkShowTP.Checked = true;
			this.checkShowTP.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkShowTP.Location = new System.Drawing.Point(8, 23);
			this.checkShowTP.Name = "checkShowTP";
			this.checkShowTP.Size = new System.Drawing.Size(77, 20);
			this.checkShowTP.TabIndex = 8;
			this.checkShowTP.Text = "Treat Plan";
			this.checkShowTP.Click += new System.EventHandler(this.checkShowTP_Click);
			// 
			// checkShowTeeth
			// 
			this.checkShowTeeth.Location = new System.Drawing.Point(145, 259);
			this.checkShowTeeth.Name = "checkShowTeeth";
			this.checkShowTeeth.Size = new System.Drawing.Size(103, 20);
			this.checkShowTeeth.TabIndex = 61;
			this.checkShowTeeth.Text = "Selected Teeth";
			this.checkShowTeeth.Click += new System.EventHandler(this.checkShowTeeth_Click);
			// 
			// checkNotes
			// 
			this.checkNotes.AllowDrop = true;
			this.checkNotes.Checked = true;
			this.checkNotes.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkNotes.Location = new System.Drawing.Point(8, 163);
			this.checkNotes.Name = "checkNotes";
			this.checkNotes.Size = new System.Drawing.Size(82, 20);
			this.checkNotes.TabIndex = 58;
			this.checkNotes.Text = "Proc Notes";
			this.checkNotes.Click += new System.EventHandler(this.checkNotes_Click);
			// 
			// checkAudit
			// 
			this.checkAudit.Location = new System.Drawing.Point(145, 283);
			this.checkAudit.Name = "checkAudit";
			this.checkAudit.Size = new System.Drawing.Size(52, 20);
			this.checkAudit.TabIndex = 62;
			this.checkAudit.Text = "Audit";
			this.checkAudit.Click += new System.EventHandler(this.checkAudit_Click);
			// 
			// textBoxViewDesc
			// 
			this.textBoxViewDesc.Location = new System.Drawing.Point(125, 8);
			this.textBoxViewDesc.Name = "textBoxViewDesc";
			this.textBoxViewDesc.Size = new System.Drawing.Size(368, 23);
			this.textBoxViewDesc.TabIndex = 1;
			// 
			// groupBoxProperties
			// 
			this.groupBoxProperties.Controls.Add(this.checkTPChart);
			this.groupBoxProperties.Controls.Add(this.groupBox6);
			this.groupBoxProperties.Controls.Add(this.butShowNone);
			this.groupBoxProperties.Controls.Add(this.butShowAll);
			this.groupBoxProperties.Controls.Add(this.checkAudit);
			this.groupBoxProperties.Controls.Add(this.groupBox7);
			this.groupBoxProperties.Controls.Add(this.checkNotes);
			this.groupBoxProperties.Controls.Add(this.checkShowTeeth);
			this.groupBoxProperties.Location = new System.Drawing.Point(27, 62);
			this.groupBoxProperties.Name = "groupBoxProperties";
			this.groupBoxProperties.Size = new System.Drawing.Size(278, 335);
			this.groupBoxProperties.TabIndex = 67;
			this.groupBoxProperties.TabStop = false;
			// 
			// checkTPChart
			// 
			this.checkTPChart.Location = new System.Drawing.Point(145, 307);
			this.checkTPChart.Name = "checkTPChart";
			this.checkTPChart.Size = new System.Drawing.Size(77, 20);
			this.checkTPChart.TabIndex = 65;
			this.checkTPChart.Text = "Is TP View";
			this.checkTPChart.Click += new System.EventHandler(this.checkTPChart_Click);
			// 
			// butShowNone
			// 
			this.butShowNone.Location = new System.Drawing.Point(67, 187);
			this.butShowNone.Name = "butShowNone";
			this.butShowNone.Size = new System.Drawing.Size(58, 23);
			this.butShowNone.TabIndex = 60;
			this.butShowNone.Text = "None";
			this.butShowNone.Click += new System.EventHandler(this.butShowNone_Click);
			// 
			// butShowAll
			// 
			this.butShowAll.Location = new System.Drawing.Point(8, 187);
			this.butShowAll.Name = "butShowAll";
			this.butShowAll.Size = new System.Drawing.Size(53, 23);
			this.butShowAll.TabIndex = 59;
			this.butShowAll.Text = "All";
			this.butShowAll.Click += new System.EventHandler(this.butShowAll_Click);
			// 
			// labelDescription
			// 
			this.labelDescription.Location = new System.Drawing.Point(47, 8);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(72, 23);
			this.labelDescription.TabIndex = 69;
			this.labelDescription.Text = "Description";
			this.labelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// listProcStatusCodes
			// 
			this.listProcStatusCodes.Location = new System.Drawing.Point(388, 81);
			this.listProcStatusCodes.Name = "listProcStatusCodes";
			this.listProcStatusCodes.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listProcStatusCodes.Size = new System.Drawing.Size(158, 186);
			this.listProcStatusCodes.TabIndex = 70;
			this.listProcStatusCodes.Visible = false;
			this.listProcStatusCodes.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listProcStatusCodes_MouseUp);
			// 
			// labelProcStatus
			// 
			this.labelProcStatus.Location = new System.Drawing.Point(388, 62);
			this.labelProcStatus.Name = "labelProcStatus";
			this.labelProcStatus.Size = new System.Drawing.Size(158, 17);
			this.labelProcStatus.TabIndex = 71;
			this.labelProcStatus.Text = "Statuses";
			this.labelProcStatus.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.labelProcStatus.Visible = false;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(27, 35);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(92, 23);
			this.label1.TabIndex = 72;
			this.label1.Text = "Dates Showing";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboDatesShowing
			// 
			this.comboDatesShowing.Location = new System.Drawing.Point(125, 35);
			this.comboDatesShowing.Name = "comboDatesShowing";
			this.comboDatesShowing.Size = new System.Drawing.Size(148, 23);
			this.comboDatesShowing.TabIndex = 73;
			this.comboDatesShowing.SelectedIndexChanged += new System.EventHandler(this.comboDatesShowing_SelectedIndexChanged);
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(584, 520);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 24);
			this.butDelete.TabIndex = 68;
			this.butDelete.Text = "&Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(600, 781);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 56;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// butRight
			// 
			this.butRight.Icon = OpenPmsDental.UI.EnumIcons.ArrowRight;
			this.butRight.Location = new System.Drawing.Point(335, 521);
			this.butRight.Name = "butRight";
			this.butRight.Size = new System.Drawing.Size(35, 24);
			this.butRight.TabIndex = 55;
			this.butRight.Click += new System.EventHandler(this.butRight_Click);
			// 
			// butLeft
			// 
			this.butLeft.AdjustImageLocation = new System.Drawing.Point(-1, 0);
			this.butLeft.Icon = OpenPmsDental.UI.EnumIcons.ArrowLeft;
			this.butLeft.Location = new System.Drawing.Point(335, 493);
			this.butLeft.Name = "butLeft";
			this.butLeft.Size = new System.Drawing.Size(35, 24);
			this.butLeft.TabIndex = 54;
			this.butLeft.Click += new System.EventHandler(this.butLeft_Click);
			// 
			// butDown
			// 
			this.butDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDown.Icon = OpenPmsDental.UI.EnumIcons.Down;
			this.butDown.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDown.Location = new System.Drawing.Point(124, 781);
			this.butDown.Name = "butDown";
			this.butDown.Size = new System.Drawing.Size(82, 24);
			this.butDown.TabIndex = 14;
			this.butDown.Text = "&Down";
			this.butDown.Click += new System.EventHandler(this.butDown_Click);
			// 
			// butUp
			// 
			this.butUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butUp.Icon = OpenPmsDental.UI.EnumIcons.Up;
			this.butUp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butUp.Location = new System.Drawing.Point(27, 781);
			this.butUp.Name = "butUp";
			this.butUp.Size = new System.Drawing.Size(82, 24);
			this.butUp.TabIndex = 13;
			this.butUp.Text = "&Up";
			this.butUp.Click += new System.EventHandler(this.butUp_Click);
			// 
			// butDefault
			// 
			this.butDefault.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDefault.Location = new System.Drawing.Point(27, 401);
			this.butDefault.Name = "butDefault";
			this.butDefault.Size = new System.Drawing.Size(91, 24);
			this.butDefault.TabIndex = 4;
			this.butDefault.Text = "Set to Default";
			this.butDefault.Click += new System.EventHandler(this.butDefault_Click);
			// 
			// gridMain
			// 
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.gridMain.Location = new System.Drawing.Point(27, 429);
			this.gridMain.Name = "gridMain";
			this.gridMain.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridMain.Size = new System.Drawing.Size(292, 336);
			this.gridMain.TabIndex = 3;
			this.gridMain.Title = "Fields Showing";
			this.gridMain.TranslationName = "FormDisplayFields";
			this.gridMain.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			// 
			// FormChartView
			// 
			this.ClientSize = new System.Drawing.Size(683, 813);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.comboDatesShowing);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.labelProcStatus);
			this.Controls.Add(this.listProcStatusCodes);
			this.Controls.Add(this.labelDescription);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.groupBoxProperties);
			this.Controls.Add(this.textBoxViewDesc);
			this.Controls.Add(this.butRight);
			this.Controls.Add(this.butLeft);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.listAvailable);
			this.Controls.Add(this.butDown);
			this.Controls.Add(this.butUp);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.butDefault);
			this.Controls.Add(this.gridMain);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormChartView";
			this.ShowInTaskbar = false;
			this.Text = "Chart View Edit";
			this.Load += new System.EventHandler(this.FormChartView_Load);
			this.groupBox7.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.groupBoxProperties.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.GridOD gridMain;
		private OpenPmsDental.UI.Button butDefault;
		private Label label2;
		private OpenPmsDental.UI.Button butDown;
		private OpenPmsDental.UI.Button butUp;
		private OpenPmsDental.UI.ListBox listAvailable;
		private Label label3;
		private OpenPmsDental.UI.Button butRight;
		private OpenPmsDental.UI.Button butLeft;
		private OpenPmsDental.UI.Button butSave;
		private OpenPmsDental.UI.GroupBox groupBox7;
		private OpenPmsDental.UI.CheckBox checkSheets;
		private OpenPmsDental.UI.CheckBox checkTasks;
		private OpenPmsDental.UI.CheckBox checkEmail;
		private OpenPmsDental.UI.CheckBox checkCommFamily;
		private OpenPmsDental.UI.CheckBox checkAppt;
		private OpenPmsDental.UI.CheckBox checkLabCase;
		private OpenPmsDental.UI.CheckBox checkRx;
		private OpenPmsDental.UI.CheckBox checkComm;
		private OpenPmsDental.UI.GroupBox groupBox6;
		private OpenPmsDental.UI.CheckBox checkShowCn;
		private OpenPmsDental.UI.CheckBox checkShowE;
		private OpenPmsDental.UI.CheckBox checkShowR;
		private OpenPmsDental.UI.CheckBox checkShowC;
		private OpenPmsDental.UI.CheckBox checkShowTP;
		private OpenPmsDental.UI.CheckBox checkShowTeeth;
		private OpenPmsDental.UI.CheckBox checkNotes;
		private OpenPmsDental.UI.CheckBox checkAudit;
		private OpenPmsDental.UI.Button butShowAll;
		private OpenPmsDental.UI.Button butShowNone;
		private TextBox textBoxViewDesc;
		private OpenPmsDental.UI.GroupBox groupBoxProperties;
		private OpenPmsDental.UI.Button butDelete;
		private Label labelDescription;
		private OpenPmsDental.UI.ListBox listProcStatusCodes;
		private Label labelProcStatus;
		private Label label1;
		private OpenPmsDental.UI.ComboBox comboDatesShowing;
		private OpenPmsDental.UI.CheckBox checkTPChart;
		private OpenPmsDental.UI.CheckBox checkCommSuperFamily;
	}
}
