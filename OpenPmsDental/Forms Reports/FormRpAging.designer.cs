using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormRpAging {
		private System.ComponentModel.IContainer components = null;

		///<summary></summary>
		protected override void Dispose(bool disposing){
			if(disposing){
				if(components != null){
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent(){
			this.butOK = new OpenPmsDental.UI.Button();
			this.groupAgeOfAccount = new OpenPmsDental.UI.GroupBox();
			this.radio30 = new System.Windows.Forms.RadioButton();
			this.radio90 = new System.Windows.Forms.RadioButton();
			this.radio60 = new System.Windows.Forms.RadioButton();
			this.radioAny = new System.Windows.Forms.RadioButton();
			this.label1 = new System.Windows.Forms.Label();
			this.textDate = new OpenPmsDental.ValidDate();
			this.listBillType = new OpenPmsDental.UI.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.checkIncludeNeg = new OpenPmsDental.UI.CheckBox();
			this.groupIncludePats = new OpenPmsDental.UI.GroupBox();
			this.checkIncludeInsNoBal = new OpenPmsDental.UI.CheckBox();
			this.checkOnlyNeg = new OpenPmsDental.UI.CheckBox();
			this.checkExcludeInactive = new OpenPmsDental.UI.CheckBox();
			this.listProv = new OpenPmsDental.UI.ListBox();
			this.label3 = new System.Windows.Forms.Label();
			this.checkProvAll = new OpenPmsDental.UI.CheckBox();
			this.checkBillTypesAll = new OpenPmsDental.UI.CheckBox();
			this.checkBadAddress = new OpenPmsDental.UI.CheckBox();
			this.checkAllClin = new OpenPmsDental.UI.CheckBox();
			this.listClin = new OpenPmsDental.UI.ListBox();
			this.labelClin = new System.Windows.Forms.Label();
			this.checkHasDateLastPay = new OpenPmsDental.UI.CheckBox();
			this.groupGroupBy = new OpenPmsDental.UI.GroupBox();
			this.radioGroupByPat = new System.Windows.Forms.RadioButton();
			this.radioGroupByFam = new System.Windows.Forms.RadioButton();
			this.checkAgeWriteoffs = new OpenPmsDental.UI.CheckBox();
			this.checkExcludeArchive = new OpenPmsDental.UI.CheckBox();
			this.butQuery = new OpenPmsDental.UI.Button();
			this.groupOnlyShow = new OpenPmsDental.UI.GroupBox();
			this.checkOnlyInsNoBal = new OpenPmsDental.UI.CheckBox();
			this.groupExcludePats = new OpenPmsDental.UI.GroupBox();
			this.checkAgePatPayPlanPayments = new OpenPmsDental.UI.CheckBox();
			this.labelFutureTrans = new System.Windows.Forms.Label();
			this.checkBoxExcludeIncomeTransfers = new OpenPmsDental.UI.CheckBox();
			this.labelAgingExcludeTransfers = new System.Windows.Forms.Label();
			this.groupAgeOfAccount.SuspendLayout();
			this.groupIncludePats.SuspendLayout();
			this.groupGroupBy.SuspendLayout();
			this.groupOnlyShow.SuspendLayout();
			this.groupExcludePats.SuspendLayout();
			this.SuspendLayout();
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(901, 424);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 26);
			this.butOK.TabIndex = 18;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// groupAgeOfAccount
			// 
			this.groupAgeOfAccount.Controls.Add(this.radio30);
			this.groupAgeOfAccount.Controls.Add(this.radio90);
			this.groupAgeOfAccount.Controls.Add(this.radio60);
			this.groupAgeOfAccount.Controls.Add(this.radioAny);
			this.groupAgeOfAccount.Location = new System.Drawing.Point(12, 39);
			this.groupAgeOfAccount.Name = "groupAgeOfAccount";
			this.groupAgeOfAccount.Size = new System.Drawing.Size(209, 123);
			this.groupAgeOfAccount.TabIndex = 2;
			this.groupAgeOfAccount.Text = "Age of Account";
			// 
			// radio30
			// 
			this.radio30.Location = new System.Drawing.Point(8, 47);
			this.radio30.Name = "radio30";
			this.radio30.Size = new System.Drawing.Size(93, 20);
			this.radio30.TabIndex = 1;
			this.radio30.Text = "Over 30 Days";
			// 
			// radio90
			// 
			this.radio90.Location = new System.Drawing.Point(8, 95);
			this.radio90.Name = "radio90";
			this.radio90.Size = new System.Drawing.Size(93, 20);
			this.radio90.TabIndex = 3;
			this.radio90.Text = "Over 90 Days";
			// 
			// radio60
			// 
			this.radio60.Location = new System.Drawing.Point(8, 71);
			this.radio60.Name = "radio60";
			this.radio60.Size = new System.Drawing.Size(93, 20);
			this.radio60.TabIndex = 2;
			this.radio60.Text = "Over 60 Days";
			// 
			// radioAny
			// 
			this.radioAny.Checked = true;
			this.radioAny.Location = new System.Drawing.Point(8, 23);
			this.radioAny.Name = "radioAny";
			this.radioAny.Size = new System.Drawing.Size(90, 20);
			this.radioAny.TabIndex = 0;
			this.radioAny.TabStop = true;
			this.radioAny.Text = "Any Balance";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(122, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "As Of Date";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textDate
			// 
			this.textDate.Location = new System.Drawing.Point(12, 8);
			this.textDate.Name = "textDate";
			this.textDate.Size = new System.Drawing.Size(104, 23);
			this.textDate.TabIndex = 1;
			// 
			// listBillType
			// 
			this.listBillType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listBillType.Location = new System.Drawing.Point(466, 51);
			this.listBillType.Name = "listBillType";
			this.listBillType.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listBillType.Size = new System.Drawing.Size(163, 264);
			this.listBillType.TabIndex = 12;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.Location = new System.Drawing.Point(466, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(163, 17);
			this.label2.TabIndex = 0;
			this.label2.Text = "Billing Types";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkIncludeNeg
			// 
			this.checkIncludeNeg.Location = new System.Drawing.Point(8, 23);
			this.checkIncludeNeg.Name = "checkIncludeNeg";
			this.checkIncludeNeg.Size = new System.Drawing.Size(122, 20);
			this.checkIncludeNeg.TabIndex = 0;
			this.checkIncludeNeg.Text = "Negative balances";
			this.checkIncludeNeg.Click += new System.EventHandler(this.checkIncludeNeg_Click);
			// 
			// groupIncludePats
			// 
			this.groupIncludePats.Controls.Add(this.checkIncludeInsNoBal);
			this.groupIncludePats.Controls.Add(this.checkIncludeNeg);
			this.groupIncludePats.Location = new System.Drawing.Point(229, 170);
			this.groupIncludePats.Name = "groupIncludePats";
			this.groupIncludePats.Size = new System.Drawing.Size(233, 75);
			this.groupIncludePats.TabIndex = 9;
			this.groupIncludePats.Text = "Include Patients With";
			// 
			// checkIncludeInsNoBal
			// 
			this.checkIncludeInsNoBal.Location = new System.Drawing.Point(8, 47);
			this.checkIncludeInsNoBal.Name = "checkIncludeInsNoBal";
			this.checkIncludeInsNoBal.Size = new System.Drawing.Size(217, 20);
			this.checkIncludeInsNoBal.TabIndex = 1;
			this.checkIncludeInsNoBal.Text = "Insurance estimates and no balance";
			this.checkIncludeInsNoBal.Click += new System.EventHandler(this.checkIncludeInsNoBal_Click);
			// 
			// checkOnlyNeg
			// 
			this.checkOnlyNeg.Location = new System.Drawing.Point(8, 23);
			this.checkOnlyNeg.Name = "checkOnlyNeg";
			this.checkOnlyNeg.Size = new System.Drawing.Size(122, 20);
			this.checkOnlyNeg.TabIndex = 0;
			this.checkOnlyNeg.Text = "Negative balances";
			this.checkOnlyNeg.Click += new System.EventHandler(this.checkOnlyNeg_Click);
			// 
			// checkExcludeInactive
			// 
			this.checkExcludeInactive.Location = new System.Drawing.Point(8, 23);
			this.checkExcludeInactive.Name = "checkExcludeInactive";
			this.checkExcludeInactive.Size = new System.Drawing.Size(100, 20);
			this.checkExcludeInactive.TabIndex = 0;
			this.checkExcludeInactive.Text = "Inactive status";
			// 
			// listProv
			// 
			this.listProv.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listProv.Location = new System.Drawing.Point(644, 53);
			this.listProv.Name = "listProv";
			this.listProv.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listProv.Size = new System.Drawing.Size(163, 264);
			this.listProv.TabIndex = 14;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.Location = new System.Drawing.Point(644, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(163, 17);
			this.label3.TabIndex = 0;
			this.label3.Text = "Providers";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkProvAll
			// 
			this.checkProvAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkProvAll.Checked = true;
			this.checkProvAll.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkProvAll.Location = new System.Drawing.Point(644, 29);
			this.checkProvAll.Name = "checkProvAll";
			this.checkProvAll.Size = new System.Drawing.Size(36, 20);
			this.checkProvAll.TabIndex = 13;
			this.checkProvAll.Text = "All";
			this.checkProvAll.Click += new System.EventHandler(this.checkProvAll_Click);
			// 
			// checkBillTypesAll
			// 
			this.checkBillTypesAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBillTypesAll.Checked = true;
			this.checkBillTypesAll.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBillTypesAll.Location = new System.Drawing.Point(466, 27);
			this.checkBillTypesAll.Name = "checkBillTypesAll";
			this.checkBillTypesAll.Size = new System.Drawing.Size(36, 20);
			this.checkBillTypesAll.TabIndex = 11;
			this.checkBillTypesAll.Text = "All";
			this.checkBillTypesAll.Click += new System.EventHandler(this.checkBillTypesAll_Click);
			// 
			// checkBadAddress
			// 
			this.checkBadAddress.Location = new System.Drawing.Point(8, 71);
			this.checkBadAddress.Name = "checkBadAddress";
			this.checkBadAddress.Size = new System.Drawing.Size(171, 20);
			this.checkBadAddress.TabIndex = 2;
			this.checkBadAddress.Text = "Bad addresses (no zipcode)";
			// 
			// checkAllClin
			// 
			this.checkAllClin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.checkAllClin.Location = new System.Drawing.Point(813, 29);
			this.checkAllClin.Name = "checkAllClin";
			this.checkAllClin.Size = new System.Drawing.Size(133, 20);
			this.checkAllClin.TabIndex = 15;
			this.checkAllClin.Text = "All (includes hidden)";
			this.checkAllClin.Click += new System.EventHandler(this.checkAllClin_Click);
			// 
			// listClin
			// 
			this.listClin.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listClin.Location = new System.Drawing.Point(813, 53);
			this.listClin.Name = "listClin";
			this.listClin.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listClin.Size = new System.Drawing.Size(163, 264);
			this.listClin.TabIndex = 16;
			this.listClin.Click += new System.EventHandler(this.listClin_Click);
			// 
			// labelClin
			// 
			this.labelClin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelClin.Location = new System.Drawing.Point(813, 8);
			this.labelClin.Name = "labelClin";
			this.labelClin.Size = new System.Drawing.Size(163, 17);
			this.labelClin.TabIndex = 0;
			this.labelClin.Text = "Clinics";
			this.labelClin.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkHasDateLastPay
			// 
			this.checkHasDateLastPay.Location = new System.Drawing.Point(12, 277);
			this.checkHasDateLastPay.Name = "checkHasDateLastPay";
			this.checkHasDateLastPay.Size = new System.Drawing.Size(219, 20);
			this.checkHasDateLastPay.TabIndex = 6;
			this.checkHasDateLastPay.Text = "Show last payment date (landscape)";
			// 
			// groupGroupBy
			// 
			this.groupGroupBy.Controls.Add(this.radioGroupByPat);
			this.groupGroupBy.Controls.Add(this.radioGroupByFam);
			this.groupGroupBy.Location = new System.Drawing.Point(12, 170);
			this.groupGroupBy.Name = "groupGroupBy";
			this.groupGroupBy.Size = new System.Drawing.Size(209, 75);
			this.groupGroupBy.TabIndex = 3;
			this.groupGroupBy.Text = "Group By";
			// 
			// radioGroupByPat
			// 
			this.radioGroupByPat.Location = new System.Drawing.Point(8, 47);
			this.radioGroupByPat.Name = "radioGroupByPat";
			this.radioGroupByPat.Size = new System.Drawing.Size(77, 20);
			this.radioGroupByPat.TabIndex = 1;
			this.radioGroupByPat.Text = "Individual";
			this.radioGroupByPat.UseVisualStyleBackColor = true;
			// 
			// radioGroupByFam
			// 
			this.radioGroupByFam.Checked = true;
			this.radioGroupByFam.Location = new System.Drawing.Point(8, 23);
			this.radioGroupByFam.Name = "radioGroupByFam";
			this.radioGroupByFam.Size = new System.Drawing.Size(60, 20);
			this.radioGroupByFam.TabIndex = 0;
			this.radioGroupByFam.TabStop = true;
			this.radioGroupByFam.Text = "Family";
			this.radioGroupByFam.UseVisualStyleBackColor = true;
			this.radioGroupByFam.CheckedChanged += new System.EventHandler(this.radioGroupByFam_CheckedChanged);
			// 
			// checkAgeWriteoffs
			// 
			this.checkAgeWriteoffs.Location = new System.Drawing.Point(12, 253);
			this.checkAgeWriteoffs.Name = "checkAgeWriteoffs";
			this.checkAgeWriteoffs.Size = new System.Drawing.Size(150, 20);
			this.checkAgeWriteoffs.TabIndex = 4;
			this.checkAgeWriteoffs.Text = "Age write-off estimates";
			// 
			// checkExcludeArchive
			// 
			this.checkExcludeArchive.Location = new System.Drawing.Point(8, 47);
			this.checkExcludeArchive.Name = "checkExcludeArchive";
			this.checkExcludeArchive.Size = new System.Drawing.Size(106, 20);
			this.checkExcludeArchive.TabIndex = 1;
			this.checkExcludeArchive.Text = "Archived status";
			// 
			// butQuery
			// 
			this.butQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butQuery.Location = new System.Drawing.Point(8, 424);
			this.butQuery.Name = "butQuery";
			this.butQuery.Size = new System.Drawing.Size(91, 26);
			this.butQuery.TabIndex = 17;
			this.butQuery.Text = "Generate Query";
			this.butQuery.Click += new System.EventHandler(this.butGenerateQuery_Click);
			// 
			// groupOnlyShow
			// 
			this.groupOnlyShow.Controls.Add(this.checkOnlyInsNoBal);
			this.groupOnlyShow.Controls.Add(this.checkOnlyNeg);
			this.groupOnlyShow.Location = new System.Drawing.Point(235, 253);
			this.groupOnlyShow.Name = "groupOnlyShow";
			this.groupOnlyShow.Size = new System.Drawing.Size(227, 95);
			this.groupOnlyShow.TabIndex = 10;
			this.groupOnlyShow.Text = "Only Patients With";
			// 
			// checkOnlyInsNoBal
			// 
			this.checkOnlyInsNoBal.Location = new System.Drawing.Point(8, 47);
			this.checkOnlyInsNoBal.Name = "checkOnlyInsNoBal";
			this.checkOnlyInsNoBal.Size = new System.Drawing.Size(211, 40);
			this.checkOnlyInsNoBal.TabIndex = 1;
			this.checkOnlyInsNoBal.Text = "Insurance estimates and no balance";
			this.checkOnlyInsNoBal.Click += new System.EventHandler(this.checkOnlyInsNoBal_Click);
			// 
			// groupExcludePats
			// 
			this.groupExcludePats.Controls.Add(this.checkExcludeInactive);
			this.groupExcludePats.Controls.Add(this.checkBadAddress);
			this.groupExcludePats.Controls.Add(this.checkExcludeArchive);
			this.groupExcludePats.Location = new System.Drawing.Point(229, 39);
			this.groupExcludePats.Name = "groupExcludePats";
			this.groupExcludePats.Size = new System.Drawing.Size(233, 123);
			this.groupExcludePats.TabIndex = 8;
			this.groupExcludePats.Text = "Exclude Patients With";
			// 
			// checkAgePatPayPlanPayments
			// 
			this.checkAgePatPayPlanPayments.Location = new System.Drawing.Point(18, 367);
			this.checkAgePatPayPlanPayments.Name = "checkAgePatPayPlanPayments";
			this.checkAgePatPayPlanPayments.Size = new System.Drawing.Size(225, 18);
			this.checkAgePatPayPlanPayments.TabIndex = 7;
			this.checkAgePatPayPlanPayments.Text = "Age patient payments to payment plans";
			this.checkAgePatPayPlanPayments.Visible = false;
			// 
			// labelFutureTrans
			// 
			this.labelFutureTrans.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.labelFutureTrans.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.labelFutureTrans.Location = new System.Drawing.Point(644, 321);
			this.labelFutureTrans.Name = "labelFutureTrans";
			this.labelFutureTrans.Size = new System.Drawing.Size(267, 18);
			this.labelFutureTrans.TabIndex = 0;
			this.labelFutureTrans.Text = "Future dated transactions are allowed";
			this.labelFutureTrans.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelFutureTrans.Visible = false;
			// 
			// checkBoxExcludeIncomeTransfers
			// 
			this.checkBoxExcludeIncomeTransfers.Location = new System.Drawing.Point(18, 352);
			this.checkBoxExcludeIncomeTransfers.Name = "checkBoxExcludeIncomeTransfers";
			this.checkBoxExcludeIncomeTransfers.Size = new System.Drawing.Size(158, 20);
			this.checkBoxExcludeIncomeTransfers.TabIndex = 220;
			this.checkBoxExcludeIncomeTransfers.Text = "Exclude income transfers";
			this.checkBoxExcludeIncomeTransfers.CheckedChanged += new System.EventHandler(this.checkBoxExcludeIncomeTransfers_CheckedChanged);
			// 
			// labelAgingExcludeTransfers
			// 
			this.labelAgingExcludeTransfers.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.labelAgingExcludeTransfers.Location = new System.Drawing.Point(233, 352);
			this.labelAgingExcludeTransfers.Name = "labelAgingExcludeTransfers";
			this.labelAgingExcludeTransfers.Size = new System.Drawing.Size(202, 40);
			this.labelAgingExcludeTransfers.TabIndex = 222;
			this.labelAgingExcludeTransfers.Text = "Aging buckets in this report may not match other areas of the program";
			this.labelAgingExcludeTransfers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelAgingExcludeTransfers.Visible = false;
			// 
			// FormRpAging
			// 
			this.AcceptButton = this.butOK;
			this.ClientSize = new System.Drawing.Size(984, 458);
			this.Controls.Add(this.labelAgingExcludeTransfers);
			this.Controls.Add(this.checkBoxExcludeIncomeTransfers);
			this.Controls.Add(this.labelFutureTrans);
			this.Controls.Add(this.checkAgePatPayPlanPayments);
			this.Controls.Add(this.groupExcludePats);
			this.Controls.Add(this.groupOnlyShow);
			this.Controls.Add(this.butQuery);
			this.Controls.Add(this.checkAgeWriteoffs);
			this.Controls.Add(this.groupGroupBy);
			this.Controls.Add(this.checkHasDateLastPay);
			this.Controls.Add(this.checkAllClin);
			this.Controls.Add(this.listClin);
			this.Controls.Add(this.labelClin);
			this.Controls.Add(this.checkBillTypesAll);
			this.Controls.Add(this.checkProvAll);
			this.Controls.Add(this.listProv);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.groupIncludePats);
			this.Controls.Add(this.textDate);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.listBillType);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.groupAgeOfAccount);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormRpAging";
			this.ShowInTaskbar = false;
			this.Text = "Aging of Accounts Receivable Report";
			this.Load += new System.EventHandler(this.FormRpAging_Load);
			this.groupAgeOfAccount.ResumeLayout(false);
			this.groupIncludePats.ResumeLayout(false);
			this.groupGroupBy.ResumeLayout(false);
			this.groupOnlyShow.ResumeLayout(false);
			this.groupExcludePats.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butOK;
		private OpenPmsDental.UI.GroupBox groupAgeOfAccount;
		private System.Windows.Forms.Label label1;
		private OpenPmsDental.ValidDate textDate;
		private OpenPmsDental.UI.ListBox listBillType;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton radio30;
		private System.Windows.Forms.RadioButton radio90;
		private System.Windows.Forms.RadioButton radio60;
		private OpenPmsDental.UI.CheckBox checkIncludeNeg;
		private OpenPmsDental.UI.GroupBox groupIncludePats;
		private OpenPmsDental.UI.CheckBox checkOnlyNeg;
		private OpenPmsDental.UI.CheckBox checkExcludeInactive;
		private OpenPmsDental.UI.ListBox listProv;
		private Label label3;
		private OpenPmsDental.UI.CheckBox checkProvAll;
		private OpenPmsDental.UI.CheckBox checkBillTypesAll;
		private OpenPmsDental.UI.CheckBox checkBadAddress;
		private OpenPmsDental.UI.CheckBox checkAllClin;
		private OpenPmsDental.UI.ListBox listClin;
		private Label labelClin;
		private System.Windows.Forms.RadioButton radioAny;
		private OpenPmsDental.UI.CheckBox checkHasDateLastPay;
		private OpenPmsDental.UI.GroupBox groupGroupBy;
		private RadioButton radioGroupByPat;
		private RadioButton radioGroupByFam;
		private OpenPmsDental.UI.CheckBox checkAgeWriteoffs;
		private OpenPmsDental.UI.CheckBox checkExcludeArchive;
		private UI.Button butQuery;
		private OpenPmsDental.UI.CheckBox checkIncludeInsNoBal;
		private OpenPmsDental.UI.GroupBox groupOnlyShow;
		private OpenPmsDental.UI.CheckBox checkOnlyInsNoBal;
		private OpenPmsDental.UI.GroupBox groupExcludePats;
		private OpenPmsDental.UI.CheckBox checkAgePatPayPlanPayments;
		private Label labelFutureTrans;
		private OpenPmsDental.UI.CheckBox checkBoxExcludeIncomeTransfers;
		private Label labelAgingExcludeTransfers;
	}
}
