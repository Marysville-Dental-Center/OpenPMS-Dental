using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormTransactionEdit {
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
			this.labelDate = new System.Windows.Forms.Label();
			this.labelDateTimeEntered = new System.Windows.Forms.Label();
			this.textDateTimeEntered = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textDebit = new System.Windows.Forms.TextBox();
			this.textCredit = new System.Windows.Forms.TextBox();
			this.checkMemoSame = new OpenPmsDental.UI.CheckBox();
			this.checkSimple = new OpenPmsDental.UI.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.panelSimple = new System.Windows.Forms.Panel();
			this.butChange = new OpenPmsDental.UI.Button();
			this.textAccount = new System.Windows.Forms.TextBox();
			this.textMemo = new System.Windows.Forms.TextBox();
			this.textCheckNumber = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.butNextCheckNumber = new OpenPmsDental.UI.Button();
			this.labelPayee = new System.Windows.Forms.Label();
			this.textPayee = new System.Windows.Forms.TextBox();
			this.labelNotes = new System.Windows.Forms.Label();
			this.textNotes = new System.Windows.Forms.TextBox();
			this.checkNeedsReview = new OpenPmsDental.UI.CheckBox();
			this.textAmount = new OpenPmsDental.ValidDouble();
			this.panelCompound = new System.Windows.Forms.Panel();
			this.butExport = new OpenPmsDental.UI.Button();
			this.butAdd = new OpenPmsDental.UI.Button();
			this.gridMain = new OpenPmsDental.UI.GridOD();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.butOpenInvoice = new OpenPmsDental.UI.Button();
			this.butAttachInvoice = new OpenPmsDental.UI.Button();
			this.textSourceInvoice = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.textSourcePay = new System.Windows.Forms.TextBox();
			this.butAttachPay = new OpenPmsDental.UI.Button();
			this.textSourceDeposit = new System.Windows.Forms.TextBox();
			this.butAttachDep = new OpenPmsDental.UI.Button();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.textDate = new OpenPmsDental.ValidDate();
			this.butSave = new OpenPmsDental.UI.Button();
			this.butPrintCheck = new OpenPmsDental.UI.Button();
			this.labelReconcileDate = new System.Windows.Forms.Label();
			this.textReconcileDate = new System.Windows.Forms.TextBox();
			this.textDateTimeEdited = new System.Windows.Forms.TextBox();
			this.labelDateTimeEdited = new System.Windows.Forms.Label();
			this.textUserEntered = new System.Windows.Forms.TextBox();
			this.labelUserEntered = new System.Windows.Forms.Label();
			this.textUserEdited = new System.Windows.Forms.TextBox();
			this.labelUserEdited = new System.Windows.Forms.Label();
			this.panelSimple.SuspendLayout();
			this.panelCompound.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// labelDate
			// 
			this.labelDate.Location = new System.Drawing.Point(149, 62);
			this.labelDate.Name = "labelDate";
			this.labelDate.Size = new System.Drawing.Size(36, 23);
			this.labelDate.TabIndex = 4;
			this.labelDate.Text = "Date";
			this.labelDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDateTimeEntered
			// 
			this.labelDateTimeEntered.Location = new System.Drawing.Point(69, 8);
			this.labelDateTimeEntered.Name = "labelDateTimeEntered";
			this.labelDateTimeEntered.Size = new System.Drawing.Size(116, 23);
			this.labelDateTimeEntered.TabIndex = 5;
			this.labelDateTimeEntered.Text = "Date/Time  Entered";
			this.labelDateTimeEntered.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDateTimeEntered
			// 
			this.textDateTimeEntered.Location = new System.Drawing.Point(191, 8);
			this.textDateTimeEntered.Name = "textDateTimeEntered";
			this.textDateTimeEntered.ReadOnly = true;
			this.textDateTimeEntered.Size = new System.Drawing.Size(148, 23);
			this.textDateTimeEntered.TabIndex = 6;
			this.textDateTimeEntered.TabStop = false;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(99, 211);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(44, 23);
			this.label3.TabIndex = 9;
			this.label3.Text = "Totals";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDebit
			// 
			this.textDebit.Location = new System.Drawing.Point(149, 211);
			this.textDebit.Name = "textDebit";
			this.textDebit.ReadOnly = true;
			this.textDebit.Size = new System.Drawing.Size(72, 23);
			this.textDebit.TabIndex = 10;
			// 
			// textCredit
			// 
			this.textCredit.Location = new System.Drawing.Point(227, 211);
			this.textCredit.Name = "textCredit";
			this.textCredit.ReadOnly = true;
			this.textCredit.Size = new System.Drawing.Size(72, 23);
			this.textCredit.TabIndex = 11;
			// 
			// checkMemoSame
			// 
			this.checkMemoSame.Location = new System.Drawing.Point(305, 212);
			this.checkMemoSame.Name = "checkMemoSame";
			this.checkMemoSame.Size = new System.Drawing.Size(130, 20);
			this.checkMemoSame.TabIndex = 1;
			this.checkMemoSame.Text = "Memo Same For All";
			// 
			// checkSimple
			// 
			this.checkSimple.Location = new System.Drawing.Point(297, 90);
			this.checkSimple.Name = "checkSimple";
			this.checkSimple.Size = new System.Drawing.Size(59, 20);
			this.checkSimple.TabIndex = 6;
			this.checkSimple.Text = "Simple";
			this.checkSimple.Click += new System.EventHandler(this.checkSimple_Click);
			// 
			// checkNeedsReview
			// 
			this.checkNeedsReview.Location = new System.Drawing.Point(297, 63);
			this.checkNeedsReview.Name = "checkNeedsReview";
			this.checkNeedsReview.Size = new System.Drawing.Size(99, 20);
			this.checkNeedsReview.TabIndex = 27;
			this.checkNeedsReview.Text = "Needs Review";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(72, 35);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(92, 23);
			this.label5.TabIndex = 21;
			this.label5.Text = "Other Account";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(116, 62);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(48, 23);
			this.label4.TabIndex = 19;
			this.label4.Text = "Memo";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(108, 8);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(56, 23);
			this.label6.TabIndex = 5;
			this.label6.Text = "Amount";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panelSimple
			// 
			this.panelSimple.Controls.Add(this.textNotes);
			this.panelSimple.Controls.Add(this.labelNotes);
			this.panelSimple.Controls.Add(this.textPayee);
			this.panelSimple.Controls.Add(this.labelPayee);
			this.panelSimple.Controls.Add(this.butNextCheckNumber);
			this.panelSimple.Controls.Add(this.butChange);
			this.panelSimple.Controls.Add(this.textAccount);
			this.panelSimple.Controls.Add(this.textMemo);
			this.panelSimple.Controls.Add(this.textCheckNumber);
			this.panelSimple.Controls.Add(this.label7);
			this.panelSimple.Controls.Add(this.textAmount);
			this.panelSimple.Controls.Add(this.label5);
			this.panelSimple.Controls.Add(this.label6);
			this.panelSimple.Controls.Add(this.label4);
			this.panelSimple.Location = new System.Drawing.Point(21, 116);
			this.panelSimple.Name = "panelSimple";
			this.panelSimple.Size = new System.Drawing.Size(494, 233);
			this.panelSimple.TabIndex = 0;
			// 
			// butChange
			// 
			this.butChange.Location = new System.Drawing.Point(408, 35);
			this.butChange.Name = "butChange";
			this.butChange.Size = new System.Drawing.Size(75, 23);
			this.butChange.TabIndex = 3;
			this.butChange.Text = "Change";
			this.butChange.Click += new System.EventHandler(this.butChange_Click);
			// 
			// textAccount
			// 
			this.textAccount.Location = new System.Drawing.Point(170, 35);
			this.textAccount.Name = "textAccount";
			this.textAccount.ReadOnly = true;
			this.textAccount.Size = new System.Drawing.Size(232, 23);
			this.textAccount.TabIndex = 4;
			this.textAccount.TabStop = false;
			// 
			// textMemo
			// 
			this.textMemo.Location = new System.Drawing.Point(170, 62);
			this.textMemo.Multiline = true;
			this.textMemo.Name = "textMemo";
			this.textMemo.Size = new System.Drawing.Size(199, 43);
			this.textMemo.TabIndex = 2;
			// 
			// textCheckNumber
			// 
			this.textCheckNumber.Location = new System.Drawing.Point(170, 109);
			this.textCheckNumber.Name = "textCheckNumber";
			this.textCheckNumber.Size = new System.Drawing.Size(132, 23);
			this.textCheckNumber.TabIndex = 1;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(72, 109);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(92, 23);
			this.label7.TabIndex = 23;
			this.label7.Text = "Check Number";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butNextCheckNumber
			// 
			this.butNextCheckNumber.Location = new System.Drawing.Point(308, 109);
			this.butNextCheckNumber.Name = "butNextCheckNumber";
			this.butNextCheckNumber.Size = new System.Drawing.Size(75, 23);
			this.butNextCheckNumber.TabIndex = 24;
			this.butNextCheckNumber.Text = "Next";
			this.butNextCheckNumber.Visible = false;
			this.butNextCheckNumber.Click += new System.EventHandler(this.butNextCheckNumber_Click);
			// 
			// labelPayee
			// 
			this.labelPayee.Location = new System.Drawing.Point(120, 136);
			this.labelPayee.Name = "labelPayee";
			this.labelPayee.Size = new System.Drawing.Size(44, 23);
			this.labelPayee.TabIndex = 25;
			this.labelPayee.Text = "Payee";
			this.labelPayee.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPayee
			// 
			this.textPayee.Location = new System.Drawing.Point(170, 136);
			this.textPayee.MaxLength = 255;
			this.textPayee.Name = "textPayee";
			this.textPayee.Size = new System.Drawing.Size(252, 23);
			this.textPayee.TabIndex = 26;
			// 
			// labelNotes
			// 
			this.labelNotes.Location = new System.Drawing.Point(120, 163);
			this.labelNotes.Name = "labelNotes";
			this.labelNotes.Size = new System.Drawing.Size(44, 23);
			this.labelNotes.TabIndex = 27;
			this.labelNotes.Text = "Notes";
			this.labelNotes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textNotes
			// 
			this.textNotes.AcceptsReturn = true;
			this.textNotes.Location = new System.Drawing.Point(170, 163);
			this.textNotes.MaxLength = 65535;
			this.textNotes.Multiline = true;
			this.textNotes.Name = "textNotes";
			this.textNotes.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textNotes.Size = new System.Drawing.Size(252, 62);
			this.textNotes.TabIndex = 28;
			// 
			// textAmount
			// 
			this.textAmount.Location = new System.Drawing.Point(170, 8);
			this.textAmount.MaxVal = 100000000D;
			this.textAmount.MinVal = -100000000D;
			this.textAmount.Name = "textAmount";
			this.textAmount.Size = new System.Drawing.Size(88, 23);
			this.textAmount.TabIndex = 0;
			// 
			// panelCompound
			// 
			this.panelCompound.Controls.Add(this.butExport);
			this.panelCompound.Controls.Add(this.butAdd);
			this.panelCompound.Controls.Add(this.gridMain);
			this.panelCompound.Controls.Add(this.label3);
			this.panelCompound.Controls.Add(this.textDebit);
			this.panelCompound.Controls.Add(this.textCredit);
			this.panelCompound.Controls.Add(this.checkMemoSame);
			this.panelCompound.Location = new System.Drawing.Point(42, 116);
			this.panelCompound.Name = "panelCompound";
			this.panelCompound.Size = new System.Drawing.Size(524, 252);
			this.panelCompound.TabIndex = 7;
			// 
			// butExport
			// 
			this.butExport.Icon = OpenPmsDental.UI.EnumIcons.Export;
			this.butExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butExport.Location = new System.Drawing.Point(441, 211);
			this.butExport.Name = "butExport";
			this.butExport.Size = new System.Drawing.Size(75, 23);
			this.butExport.TabIndex = 2;
			this.butExport.Text = "Export";
			this.butExport.Click += new System.EventHandler(this.butExport_Click);
			// 
			// butAdd
			// 
			this.butAdd.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(8, 211);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(75, 23);
			this.butAdd.TabIndex = 0;
			this.butAdd.Text = "Add";
			this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
			// 
			// gridMain
			// 
			this.gridMain.Location = new System.Drawing.Point(8, 8);
			this.gridMain.Name = "gridMain";
			this.gridMain.Size = new System.Drawing.Size(498, 199);
			this.gridMain.TabIndex = 2;
			this.gridMain.Title = "Splits";
			this.gridMain.TranslationName = "TableTransSplits";
			this.gridMain.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox1.Controls.Add(this.butOpenInvoice);
			this.groupBox1.Controls.Add(this.butAttachInvoice);
			this.groupBox1.Controls.Add(this.textSourceInvoice);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.textSourcePay);
			this.groupBox1.Controls.Add(this.butAttachPay);
			this.groupBox1.Controls.Add(this.textSourceDeposit);
			this.groupBox1.Controls.Add(this.butAttachDep);
			this.groupBox1.Location = new System.Drawing.Point(89, 392);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(444, 108);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.Text = "Source Documents";
			// 
			// butOpenInvoice
			// 
			this.butOpenInvoice.Enabled = false;
			this.butOpenInvoice.Location = new System.Drawing.Point(388, 77);
			this.butOpenInvoice.Name = "butOpenInvoice";
			this.butOpenInvoice.Size = new System.Drawing.Size(48, 23);
			this.butOpenInvoice.TabIndex = 16;
			this.butOpenInvoice.Text = "Open";
			this.butOpenInvoice.Click += new System.EventHandler(this.butOpenInvoice_Click);
			// 
			// butAttachInvoice
			// 
			this.butAttachInvoice.Location = new System.Drawing.Point(324, 77);
			this.butAttachInvoice.Name = "butAttachInvoice";
			this.butAttachInvoice.Size = new System.Drawing.Size(58, 23);
			this.butAttachInvoice.TabIndex = 15;
			this.butAttachInvoice.Text = "Attach";
			this.butAttachInvoice.Click += new System.EventHandler(this.butAttachInvoice_Click);
			// 
			// textSourceInvoice
			// 
			this.textSourceInvoice.Location = new System.Drawing.Point(102, 77);
			this.textSourceInvoice.Name = "textSourceInvoice";
			this.textSourceInvoice.ReadOnly = true;
			this.textSourceInvoice.Size = new System.Drawing.Size(216, 23);
			this.textSourceInvoice.TabIndex = 14;
			this.textSourceInvoice.TabStop = false;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(44, 77);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(52, 23);
			this.label1.TabIndex = 13;
			this.label1.Text = "Invoice";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(36, 50);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(60, 23);
			this.label9.TabIndex = 12;
			this.label9.Text = "Payment";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(44, 23);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(52, 23);
			this.label8.TabIndex = 11;
			this.label8.Text = "Deposit";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textSourcePay
			// 
			this.textSourcePay.Location = new System.Drawing.Point(102, 50);
			this.textSourcePay.Multiline = true;
			this.textSourcePay.Name = "textSourcePay";
			this.textSourcePay.ReadOnly = true;
			this.textSourcePay.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textSourcePay.Size = new System.Drawing.Size(232, 23);
			this.textSourcePay.TabIndex = 10;
			// 
			// butAttachPay
			// 
			this.butAttachPay.Location = new System.Drawing.Point(340, 50);
			this.butAttachPay.Name = "butAttachPay";
			this.butAttachPay.Size = new System.Drawing.Size(58, 23);
			this.butAttachPay.TabIndex = 1;
			this.butAttachPay.Text = "Attach";
			this.butAttachPay.Click += new System.EventHandler(this.butAttachPay_Click);
			// 
			// textSourceDeposit
			// 
			this.textSourceDeposit.Location = new System.Drawing.Point(102, 23);
			this.textSourceDeposit.Name = "textSourceDeposit";
			this.textSourceDeposit.ReadOnly = true;
			this.textSourceDeposit.Size = new System.Drawing.Size(232, 23);
			this.textSourceDeposit.TabIndex = 8;
			this.textSourceDeposit.TabStop = false;
			// 
			// butAttachDep
			// 
			this.butAttachDep.Location = new System.Drawing.Point(340, 23);
			this.butAttachDep.Name = "butAttachDep";
			this.butAttachDep.Size = new System.Drawing.Size(58, 23);
			this.butAttachDep.TabIndex = 0;
			this.butAttachDep.Text = "Attach";
			this.butAttachDep.Click += new System.EventHandler(this.butAttachDep_Click);
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 474);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 26);
			this.butDelete.TabIndex = 3;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// textDate
			// 
			this.textDate.Location = new System.Drawing.Point(191, 62);
			this.textDate.Name = "textDate";
			this.textDate.Size = new System.Drawing.Size(100, 20);
			this.textDate.TabIndex = 1;
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(566, 472);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 26);
			this.butSave.TabIndex = 2;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// butPrintCheck
			// 
			this.butPrintCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butPrintCheck.Location = new System.Drawing.Point(562, 472);
			this.butPrintCheck.Name = "butPrintCheck";
			this.butPrintCheck.Size = new System.Drawing.Size(75, 26);
			this.butPrintCheck.TabIndex = 27;
			this.butPrintCheck.Text = "Print Check";
			this.butPrintCheck.Visible = false;
			this.butPrintCheck.Click += new System.EventHandler(this.butPrintCheck_Click);
			// 
			// labelReconcileDate
			// 
			this.labelReconcileDate.Location = new System.Drawing.Point(93, 89);
			this.labelReconcileDate.Name = "labelReconcileDate";
			this.labelReconcileDate.Size = new System.Drawing.Size(92, 23);
			this.labelReconcileDate.TabIndex = 19;
			this.labelReconcileDate.Text = "Reconcile Date";
			this.labelReconcileDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelReconcileDate.Visible = false;
			// 
			// textReconcileDate
			// 
			this.textReconcileDate.Location = new System.Drawing.Point(191, 89);
			this.textReconcileDate.Name = "textReconcileDate";
			this.textReconcileDate.ReadOnly = true;
			this.textReconcileDate.Size = new System.Drawing.Size(100, 20);
			this.textReconcileDate.TabIndex = 20;
			this.textReconcileDate.Visible = false;
			// 
			// textDateTimeEdited
			// 
			this.textDateTimeEdited.Location = new System.Drawing.Point(191, 35);
			this.textDateTimeEdited.Name = "textDateTimeEdited";
			this.textDateTimeEdited.ReadOnly = true;
			this.textDateTimeEdited.Size = new System.Drawing.Size(148, 23);
			this.textDateTimeEdited.TabIndex = 22;
			this.textDateTimeEdited.TabStop = false;
			// 
			// labelDateTimeEdited
			// 
			this.labelDateTimeEdited.Location = new System.Drawing.Point(81, 35);
			this.labelDateTimeEdited.Name = "labelDateTimeEdited";
			this.labelDateTimeEdited.Size = new System.Drawing.Size(104, 23);
			this.labelDateTimeEdited.TabIndex = 21;
			this.labelDateTimeEdited.Text = "Date/Time Edited";
			this.labelDateTimeEdited.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textUserEntered
			// 
			this.textUserEntered.Location = new System.Drawing.Point(441, 8);
			this.textUserEntered.Name = "textUserEntered";
			this.textUserEntered.ReadOnly = true;
			this.textUserEntered.Size = new System.Drawing.Size(100, 20);
			this.textUserEntered.TabIndex = 24;
			this.textUserEntered.TabStop = false;
			// 
			// labelUserEntered
			// 
			this.labelUserEntered.Location = new System.Drawing.Point(355, 8);
			this.labelUserEntered.Name = "labelUserEntered";
			this.labelUserEntered.Size = new System.Drawing.Size(80, 23);
			this.labelUserEntered.TabIndex = 23;
			this.labelUserEntered.Text = "User Entered";
			this.labelUserEntered.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textUserEdited
			// 
			this.textUserEdited.Location = new System.Drawing.Point(441, 35);
			this.textUserEdited.Name = "textUserEdited";
			this.textUserEdited.ReadOnly = true;
			this.textUserEdited.Size = new System.Drawing.Size(100, 20);
			this.textUserEdited.TabIndex = 26;
			this.textUserEdited.TabStop = false;
			// 
			// labelUserEdited
			// 
			this.labelUserEdited.Location = new System.Drawing.Point(363, 35);
			this.labelUserEdited.Name = "labelUserEdited";
			this.labelUserEdited.Size = new System.Drawing.Size(72, 23);
			this.labelUserEdited.TabIndex = 25;
			this.labelUserEdited.Text = "User Edited";
			this.labelUserEdited.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FormTransactionEdit
			// 
			this.ClientSize = new System.Drawing.Size(649, 508);
			this.Controls.Add(this.textUserEdited);
			this.Controls.Add(this.labelUserEdited);
			this.Controls.Add(this.textUserEntered);
			this.Controls.Add(this.labelUserEntered);
			this.Controls.Add(this.textDateTimeEdited);
			this.Controls.Add(this.labelDateTimeEdited);
			this.Controls.Add(this.textReconcileDate);
			this.Controls.Add(this.labelReconcileDate);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.panelSimple);
			this.Controls.Add(this.panelCompound);
			this.Controls.Add(this.checkNeedsReview);
			this.Controls.Add(this.checkSimple);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.textDateTimeEntered);
			this.Controls.Add(this.labelDateTimeEntered);
			this.Controls.Add(this.labelDate);
			this.Controls.Add(this.textDate);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.butPrintCheck);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormTransactionEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Transaction";
			this.Load += new System.EventHandler(this.FormTransactionEdit_Load);
			this.panelSimple.ResumeLayout(false);
			this.panelSimple.PerformLayout();
			this.panelCompound.ResumeLayout(false);
			this.panelCompound.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private OpenPmsDental.UI.Button butPrintCheck;
		private OpenPmsDental.UI.GridOD gridMain;
		private ValidDate textDate;
		private Label labelDate;
		private Label labelDateTimeEntered;
		private TextBox textDateTimeEntered;
		private OpenPmsDental.UI.Button butAttachDep;
		private OpenPmsDental.UI.Button butDelete;
		private Label label3;
		private TextBox textDebit;
		private TextBox textCredit;
		private OpenPmsDental.UI.CheckBox checkMemoSame;
		private OpenPmsDental.UI.Button butAdd;
		private OpenPmsDental.UI.CheckBox checkSimple;
		private Label label5;
		private Label label4;
		private ValidDouble textAmount;
		private Label label6;
		private Panel panelSimple;
		private Panel panelCompound;
		private TextBox textCheckNumber;
		private Label label7;
		private TextBox textMemo;
		private OpenPmsDental.UI.Button butChange;
		private TextBox textAccount;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private TextBox textSourceDeposit;
		private Label label9;
		private Label label8;
		private TextBox textSourcePay;
		private OpenPmsDental.UI.Button butAttachPay;
		private Label labelReconcileDate;
		private TextBox textReconcileDate;
		private TextBox textDateTimeEdited;
		private Label labelDateTimeEdited;
		private TextBox textUserEntered;
		private Label labelUserEntered;
		private TextBox textUserEdited;
		private Label labelUserEdited;
		private UI.Button butExport;
		private TextBox textSourceInvoice;
		private Label label1;
		private UI.Button butOpenInvoice;
		private UI.Button butAttachInvoice;
		private UI.Button butNextCheckNumber;
		private Label labelPayee;
		private TextBox textPayee;
		private Label labelNotes;
		private TextBox textNotes;
		private OpenPmsDental.UI.CheckBox checkNeedsReview;
	}
}
