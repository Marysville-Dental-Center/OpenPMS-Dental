using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormClaimPayEdit {
		private System.ComponentModel.IContainer components = null;

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
			this.textAmount = new OpenPmsDental.ValidDouble();
			this.textDate = new OpenPmsDental.ValidDate();
			this.textBankBranch = new System.Windows.Forms.TextBox();
			this.textCheckNum = new System.Windows.Forms.TextBox();
			this.textNote = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.butSave = new OpenPmsDental.UI.Button();
			this.comboClinic = new OpenPmsDental.UI.ComboBoxClinicPicker();
			this.labelClinic = new System.Windows.Forms.Label();
			this.textCarrierName = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.labelDateIssued = new System.Windows.Forms.Label();
			this.textDateIssued = new OpenPmsDental.ValidDate();
			this.label1 = new System.Windows.Forms.Label();
			this.butCarrierSelect = new OpenPmsDental.UI.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.comboPayType = new OpenPmsDental.UI.ComboBox();
			this.label11 = new System.Windows.Forms.Label();
			this.butPickPaymentGroup = new OpenPmsDental.UI.Button();
			this.comboPayGroup = new OpenPmsDental.UI.ComboBox();
			this.labelClaimPaymentGroup = new System.Windows.Forms.Label();
			this.panelXcharge = new OpenPmsDental.UI.Button();
			this.groupPrepaid = new OpenPmsDental.UI.GroupBox();
			this.butPaySimple = new OpenPmsDental.UI.Button();
			this.groupBoxDeposit = new OpenPmsDental.UI.GroupBox();
			this.labelDepositAccountNum = new System.Windows.Forms.Label();
			this.comboDepositAccountNum = new OpenPmsDental.UI.ComboBox();
			this.textBoxBatchNum = new System.Windows.Forms.TextBox();
			this.butDepositEdit = new OpenPmsDental.UI.Button();
			this.labelBatchNum = new System.Windows.Forms.Label();
			this.labelDepositDate = new System.Windows.Forms.Label();
			this.validDepositDate = new OpenPmsDental.ValidDate();
			this.labelDepositAmount = new System.Windows.Forms.Label();
			this.validDoubleDepositAmt = new OpenPmsDental.ValidDouble();
			this.labelRequiredFields = new System.Windows.Forms.Label();
			this.panelEdgeExpress = new OpenPmsDental.UI.Button();
			this.butPayConnect = new OpenPmsDental.UI.Button();
			this.groupPrepaid.SuspendLayout();
			this.groupBoxDeposit.SuspendLayout();
			this.SuspendLayout();
			// 
			// textAmount
			// 
			this.textAmount.AutoSize = false;
			this.textAmount.Location = new System.Drawing.Point(152, 143);
			this.textAmount.MaxVal = 100000000D;
			this.textAmount.MinVal = -100000000D;
			this.textAmount.Name = "textAmount";
			this.textAmount.Size = new System.Drawing.Size(68, 23);
			this.textAmount.TabIndex = 3;
			this.textAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textAmount.Leave += new System.EventHandler(this.TextAmount_Leave);
			// 
			// textDate
			// 
			this.textDate.AutoSize = false;
			this.textDate.Location = new System.Drawing.Point(152, 89);
			this.textDate.Name = "textDate";
			this.textDate.Size = new System.Drawing.Size(68, 23);
			this.textDate.TabIndex = 1;
			this.textDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBankBranch
			// 
			this.textBankBranch.AutoSize = false;
			this.textBankBranch.Location = new System.Drawing.Point(152, 197);
			this.textBankBranch.MaxLength = 25;
			this.textBankBranch.Name = "textBankBranch";
			this.textBankBranch.Size = new System.Drawing.Size(100, 23);
			this.textBankBranch.TabIndex = 5;
			// 
			// textCheckNum
			// 
			this.textCheckNum.AutoSize = false;
			this.textCheckNum.Location = new System.Drawing.Point(152, 170);
			this.textCheckNum.MaxLength = 25;
			this.textCheckNum.Name = "textCheckNum";
			this.textCheckNum.Size = new System.Drawing.Size(100, 23);
			this.textCheckNum.TabIndex = 4;
			// 
			// textNote
			// 
			this.textNote.AcceptsReturn = true;
			this.textNote.Location = new System.Drawing.Point(152, 269);
			this.textNote.MaxLength = 255;
			this.textNote.Multiline = true;
			this.textNote.Name = "textNote";
			this.textNote.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textNote.Size = new System.Drawing.Size(339, 73);
			this.textNote.TabIndex = 7;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(18, 89);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(128, 23);
			this.label6.TabIndex = 37;
			this.label6.Text = "Payment Posting Date";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(90, 143);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(56, 23);
			this.label5.TabIndex = 36;
			this.label5.Text = "Amount";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(90, 170);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 23);
			this.label4.TabIndex = 35;
			this.label4.Text = "Check #";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(66, 197);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 23);
			this.label3.TabIndex = 34;
			this.label3.Text = "Bank-Branch";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(106, 269);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 23);
			this.label2.TabIndex = 33;
			this.label2.Text = "Note";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(437, 586);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 5;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// comboClinic
			// 
			this.comboClinic.ForceShowUnassigned = true;
			this.comboClinic.HqDescription = "None";
			this.comboClinic.IncludeUnassigned = true;
			this.comboClinic.Location = new System.Drawing.Point(152, 8);
			this.comboClinic.Name = "comboClinic";
			this.comboClinic.ShowLabel = false;
			this.comboClinic.Size = new System.Drawing.Size(208, 23);
			this.comboClinic.TabIndex = 0;
			// 
			// labelClinic
			// 
			this.labelClinic.Location = new System.Drawing.Point(102, 8);
			this.labelClinic.Name = "labelClinic";
			this.labelClinic.Size = new System.Drawing.Size(44, 23);
			this.labelClinic.TabIndex = 91;
			this.labelClinic.Text = "Clinic";
			this.labelClinic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textCarrierName
			// 
			this.textCarrierName.AutoSize = false;
			this.textCarrierName.Location = new System.Drawing.Point(152, 224);
			this.textCarrierName.MaxLength = 25;
			this.textCarrierName.Name = "textCarrierName";
			this.textCarrierName.Size = new System.Drawing.Size(264, 23);
			this.textCarrierName.TabIndex = 6;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(62, 224);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(84, 23);
			this.label7.TabIndex = 94;
			this.label7.Text = "Carrier Name";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDateIssued
			// 
			this.labelDateIssued.Location = new System.Drawing.Point(22, 116);
			this.labelDateIssued.Name = "labelDateIssued";
			this.labelDateIssued.Size = new System.Drawing.Size(124, 23);
			this.labelDateIssued.TabIndex = 37;
			this.labelDateIssued.Text = "Check EFT Issue Date";
			this.labelDateIssued.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDateIssued
			// 
			this.textDateIssued.AutoSize = false;
			this.textDateIssued.Location = new System.Drawing.Point(152, 116);
			this.textDateIssued.Name = "textDateIssued";
			this.textDateIssued.Size = new System.Drawing.Size(68, 23);
			this.textDateIssued.TabIndex = 2;
			this.textDateIssued.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(226, 118);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 18);
			this.label1.TabIndex = 95;
			this.label1.Text = "(optional)";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// butCarrierSelect
			// 
			this.butCarrierSelect.Location = new System.Drawing.Point(422, 224);
			this.butCarrierSelect.Name = "butCarrierSelect";
			this.butCarrierSelect.Size = new System.Drawing.Size(69, 23);
			this.butCarrierSelect.TabIndex = 8;
			this.butCarrierSelect.Text = "Pick";
			this.butCarrierSelect.Click += new System.EventHandler(this.butCarrierSelect_Click);
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(152, 251);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(200, 16);
			this.label8.TabIndex = 96;
			this.label8.Text = "(does not need to be exact)";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(258, 199);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(94, 18);
			this.label9.TabIndex = 97;
			this.label9.Text = "(optional)";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(258, 172);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(94, 18);
			this.label10.TabIndex = 98;
			this.label10.Text = "(optional)";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// comboPayType
			// 
			this.comboPayType.Location = new System.Drawing.Point(152, 35);
			this.comboPayType.Name = "comboPayType";
			this.comboPayType.Size = new System.Drawing.Size(208, 23);
			this.comboPayType.TabIndex = 99;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(58, 35);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(88, 23);
			this.label11.TabIndex = 100;
			this.label11.Text = "Payment Type";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butPickPaymentGroup
			// 
			this.butPickPaymentGroup.Location = new System.Drawing.Point(366, 62);
			this.butPickPaymentGroup.Name = "butPickPaymentGroup";
			this.butPickPaymentGroup.Size = new System.Drawing.Size(23, 23);
			this.butPickPaymentGroup.TabIndex = 103;
			this.butPickPaymentGroup.Text = "...";
			this.butPickPaymentGroup.Click += new System.EventHandler(this.butPickPaymentGroup_Click);
			// 
			// comboPayGroup
			// 
			this.comboPayGroup.Location = new System.Drawing.Point(152, 62);
			this.comboPayGroup.Name = "comboPayGroup";
			this.comboPayGroup.Size = new System.Drawing.Size(208, 23);
			this.comboPayGroup.TabIndex = 102;
			// 
			// labelClaimPaymentGroup
			// 
			this.labelClaimPaymentGroup.Location = new System.Drawing.Point(18, 62);
			this.labelClaimPaymentGroup.Name = "labelClaimPaymentGroup";
			this.labelClaimPaymentGroup.Size = new System.Drawing.Size(128, 23);
			this.labelClaimPaymentGroup.TabIndex = 101;
			this.labelClaimPaymentGroup.Text = "Claim Payment Group";
			this.labelClaimPaymentGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panelXcharge
			// 
			this.panelXcharge.Location = new System.Drawing.Point(8, 23);
			this.panelXcharge.Name = "panelXcharge";
			this.panelXcharge.Size = new System.Drawing.Size(59, 26);
			this.panelXcharge.TabIndex = 119;
			this.panelXcharge.Text = "X-Charge";
			this.panelXcharge.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelXcharge_MouseClick);
			// 
			// groupPrepaid
			// 
			this.groupPrepaid.Controls.Add(this.butPayConnect);
			this.groupPrepaid.Controls.Add(this.panelEdgeExpress);
			this.groupPrepaid.Controls.Add(this.butPaySimple);
			this.groupPrepaid.Controls.Add(this.panelXcharge);
			this.groupPrepaid.Location = new System.Drawing.Point(152, 350);
			this.groupPrepaid.Name = "groupPrepaid";
			this.groupPrepaid.Size = new System.Drawing.Size(339, 57);
			this.groupPrepaid.TabIndex = 120;
			this.groupPrepaid.TabStop = false;
			this.groupPrepaid.Text = "Virtual Credit Card Payment";
			// 
			// butPaySimple
			// 
			this.butPaySimple.Location = new System.Drawing.Point(236, 23);
			this.butPaySimple.Name = "butPaySimple";
			this.butPaySimple.Size = new System.Drawing.Size(76, 26);
			this.butPaySimple.TabIndex = 131;
			this.butPaySimple.Text = "PaySimple";
			this.butPaySimple.MouseClick += new System.Windows.Forms.MouseEventHandler(this.butPaySimple_MouseClick);
			// 
			// groupBoxDeposit
			// 
			this.groupBoxDeposit.Controls.Add(this.labelDepositAccountNum);
			this.groupBoxDeposit.Controls.Add(this.comboDepositAccountNum);
			this.groupBoxDeposit.Controls.Add(this.textBoxBatchNum);
			this.groupBoxDeposit.Controls.Add(this.butDepositEdit);
			this.groupBoxDeposit.Controls.Add(this.labelBatchNum);
			this.groupBoxDeposit.Controls.Add(this.labelDepositDate);
			this.groupBoxDeposit.Controls.Add(this.validDepositDate);
			this.groupBoxDeposit.Controls.Add(this.labelDepositAmount);
			this.groupBoxDeposit.Controls.Add(this.validDoubleDepositAmt);
			this.groupBoxDeposit.Location = new System.Drawing.Point(8, 415);
			this.groupBoxDeposit.Name = "groupBoxDeposit";
			this.groupBoxDeposit.Size = new System.Drawing.Size(504, 135);
			this.groupBoxDeposit.TabIndex = 121;
			this.groupBoxDeposit.TabStop = false;
			this.groupBoxDeposit.Text = "Deposit Details";
			// 
			// labelDepositAccountNum
			// 
			this.labelDepositAccountNum.Location = new System.Drawing.Point(10, 104);
			this.labelDepositAccountNum.Name = "labelDepositAccountNum";
			this.labelDepositAccountNum.Size = new System.Drawing.Size(128, 23);
			this.labelDepositAccountNum.TabIndex = 111;
			this.labelDepositAccountNum.Text = "Auto Deposit Account";
			this.labelDepositAccountNum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboDepositAccountNum
			// 
			this.comboDepositAccountNum.Location = new System.Drawing.Point(144, 104);
			this.comboDepositAccountNum.Name = "comboDepositAccountNum";
			this.comboDepositAccountNum.Size = new System.Drawing.Size(208, 23);
			this.comboDepositAccountNum.TabIndex = 110;
			// 
			// textBoxBatchNum
			// 
			this.textBoxBatchNum.AutoSize = false;
			this.textBoxBatchNum.Location = new System.Drawing.Point(144, 77);
			this.textBoxBatchNum.MaxLength = 25;
			this.textBoxBatchNum.Name = "textBoxBatchNum";
			this.textBoxBatchNum.Size = new System.Drawing.Size(208, 23);
			this.textBoxBatchNum.TabIndex = 109;
			// 
			// butDepositEdit
			// 
			this.butDepositEdit.Location = new System.Drawing.Point(410, 23);
			this.butDepositEdit.Name = "butDepositEdit";
			this.butDepositEdit.Size = new System.Drawing.Size(69, 23);
			this.butDepositEdit.TabIndex = 107;
			this.butDepositEdit.Text = "Edit";
			this.butDepositEdit.UseVisualStyleBackColor = true;
			this.butDepositEdit.Visible = false;
			this.butDepositEdit.Click += new System.EventHandler(this.butDepositEdit_Click);
			// 
			// labelBatchNum
			// 
			this.labelBatchNum.Location = new System.Drawing.Point(86, 77);
			this.labelBatchNum.Name = "labelBatchNum";
			this.labelBatchNum.Size = new System.Drawing.Size(52, 23);
			this.labelBatchNum.TabIndex = 108;
			this.labelBatchNum.Text = "Batch #";
			this.labelBatchNum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDepositDate
			// 
			this.labelDepositDate.Location = new System.Drawing.Point(102, 23);
			this.labelDepositDate.Name = "labelDepositDate";
			this.labelDepositDate.Size = new System.Drawing.Size(36, 23);
			this.labelDepositDate.TabIndex = 105;
			this.labelDepositDate.Text = "Date";
			this.labelDepositDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// validDepositDate
			// 
			this.validDepositDate.AutoSize = false;
			this.validDepositDate.Location = new System.Drawing.Point(144, 23);
			this.validDepositDate.Name = "validDepositDate";
			this.validDepositDate.Size = new System.Drawing.Size(68, 23);
			this.validDepositDate.TabIndex = 102;
			this.validDepositDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// labelDepositAmount
			// 
			this.labelDepositAmount.Location = new System.Drawing.Point(82, 50);
			this.labelDepositAmount.Name = "labelDepositAmount";
			this.labelDepositAmount.Size = new System.Drawing.Size(56, 23);
			this.labelDepositAmount.TabIndex = 106;
			this.labelDepositAmount.Text = "Amount";
			this.labelDepositAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// validDoubleDepositAmt
			// 
			this.validDoubleDepositAmt.Enabled = false;
			this.validDoubleDepositAmt.AutoSize = false;
			this.validDoubleDepositAmt.Location = new System.Drawing.Point(144, 50);
			this.validDoubleDepositAmt.MaxVal = 100000000D;
			this.validDoubleDepositAmt.MinVal = -100000000D;
			this.validDoubleDepositAmt.Name = "validDoubleDepositAmt";
			this.validDoubleDepositAmt.Size = new System.Drawing.Size(68, 23);
			this.validDoubleDepositAmt.TabIndex = 104;
			this.validDoubleDepositAmt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// labelRequiredFields
			// 
			this.labelRequiredFields.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelRequiredFields.Location = new System.Drawing.Point(152, 558);
			this.labelRequiredFields.Name = "labelRequiredFields";
			this.labelRequiredFields.Size = new System.Drawing.Size(180, 14);
			this.labelRequiredFields.TabIndex = 122;
			this.labelRequiredFields.Text = "* Indicates Required Field";
			this.labelRequiredFields.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.labelRequiredFields.Visible = false;
			// 
			// panelEdgeExpress
			// 
			this.panelEdgeExpress.Location = new System.Drawing.Point(73, 23);
			this.panelEdgeExpress.Name = "panelEdgeExpress";
			this.panelEdgeExpress.Size = new System.Drawing.Size(75, 26);
			this.panelEdgeExpress.TabIndex = 132;
			this.panelEdgeExpress.Text = "EdgeExpress";
			this.panelEdgeExpress.Visible = false;
			this.panelEdgeExpress.MouseClick += new System.Windows.Forms.MouseEventHandler(this.panelEdgeExpress_MouseClick);
			// 
			// butPayConnect
			// 
			this.butPayConnect.Location = new System.Drawing.Point(154, 23);
			this.butPayConnect.Name = "butPayConnect";
			this.butPayConnect.Size = new System.Drawing.Size(76, 26);
			this.butPayConnect.TabIndex = 132;
			this.butPayConnect.Text = "PayConnect";
			this.butPayConnect.Click += new System.EventHandler(this.butPayConnect_Click);
			// 
			// FormClaimPayEdit
			// 
			this.AcceptButton = this.butSave;
			this.ClientSize = new System.Drawing.Size(520, 618);
			this.Controls.Add(this.labelRequiredFields);
			this.Controls.Add(this.groupBoxDeposit);
			this.Controls.Add(this.groupPrepaid);
			this.Controls.Add(this.butPickPaymentGroup);
			this.Controls.Add(this.comboPayGroup);
			this.Controls.Add(this.labelClaimPaymentGroup);
			this.Controls.Add(this.comboPayType);
			this.Controls.Add(this.label11);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textCarrierName);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.comboClinic);
			this.Controls.Add(this.labelClinic);
			this.Controls.Add(this.textAmount);
			this.Controls.Add(this.textDateIssued);
			this.Controls.Add(this.textDate);
			this.Controls.Add(this.textBankBranch);
			this.Controls.Add(this.textCheckNum);
			this.Controls.Add(this.textNote);
			this.Controls.Add(this.labelDateIssued);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.butCarrierSelect);
			this.Controls.Add(this.butSave);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormClaimPayEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Insurance Payment";
			this.FormClosing += new FormClosingEventHandler(this.FormClaimPayEdit_FormClosing);
			this.Load += new System.EventHandler(this.FormClaimPayEdit_Load);
			this.groupPrepaid.ResumeLayout(false);
			this.groupBoxDeposit.ResumeLayout(false);
			this.groupBoxDeposit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private OpenPmsDental.ValidDouble textAmount;
		private OpenPmsDental.ValidDate textDate;
		private System.Windows.Forms.TextBox textBankBranch;
		private System.Windows.Forms.TextBox textCheckNum;
		private System.Windows.Forms.TextBox textNote;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private OpenPmsDental.UI.Button butSave;
		private UI.ComboBoxClinicPicker comboClinic;
		private System.Windows.Forms.Label labelClinic;
		private System.Windows.Forms.TextBox textCarrierName;
		private System.Windows.Forms.Label label7;
		private Label labelDateIssued;
		private ValidDate textDateIssued;
		private UI.Button butCarrierSelect;
		private Label label8;
		private Label label9;
		private Label label10;
		private OpenPmsDental.UI.ComboBox comboPayType;
		private Label label11;
		private Label label1;
		private UI.Button butPickPaymentGroup;
		private OpenPmsDental.UI.ComboBox comboPayGroup;
		private Label labelClaimPaymentGroup;
		private OpenPmsDental.UI.Button panelXcharge;
		private OpenPmsDental.UI.GroupBox groupPrepaid;
		private OpenPmsDental.UI.GroupBox groupBoxDeposit;
		private ValidDate validDepositDate;
		private ValidDouble validDoubleDepositAmt;
		private Label labelDepositAmount;
		private Label labelDepositDate;
		private UI.Button butDepositEdit;
		private Label labelBatchNum;
		private TextBox textBoxBatchNum;
		private Label labelDepositAccountNum;
		private UI.ComboBox comboDepositAccountNum;
		private OpenPmsDental.UI.Button butPaySimple;
		private Label labelRequiredFields;
		private OpenPmsDental.UI.Button panelEdgeExpress;
		private OpenPmsDental.UI.Button butPayConnect;
	}
}
