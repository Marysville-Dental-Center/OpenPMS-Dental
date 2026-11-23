using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormClaimPayBatch {
		/// <summary>
		/// Required designer variable.
		/// </summary>
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormClaimPayBatch));
			this.menuRightAttached = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemGotoAccount = new System.Windows.Forms.ToolStripMenuItem();
			this.menuRightOut = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemGotoOut = new System.Windows.Forms.ToolStripMenuItem();
			this.groupFilters = new OpenPmsDental.UI.GroupBox();
			this.butCarrierPick = new OpenPmsDental.UI.Button();
			this.textCarrier = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.textName = new System.Windows.Forms.TextBox();
			this.textClaimID = new System.Windows.Forms.TextBox();
			this.butRefresh = new OpenPmsDental.UI.Button();
			this.butDown = new OpenPmsDental.UI.Button();
			this.textTotal = new OpenPmsDental.ValidDouble();
			this.butAttach = new OpenPmsDental.UI.Button();
			this.textEobIsScanned = new System.Windows.Forms.TextBox();
			this.butView = new OpenPmsDental.UI.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.butSave = new OpenPmsDental.UI.Button();
			this.labelInstruct2 = new System.Windows.Forms.Label();
			this.butUp = new OpenPmsDental.UI.Button();
			this.labelInstruct1 = new System.Windows.Forms.Label();
			this.butDetach = new OpenPmsDental.UI.Button();
			this.gridOut = new OpenPmsDental.UI.GridOD();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.label13 = new System.Windows.Forms.Label();
			this.textPayGroup = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.textPayType = new System.Windows.Forms.TextBox();
			this.labelClinic = new System.Windows.Forms.Label();
			this.textDateIssued = new OpenPmsDental.ValidDate();
			this.label2 = new System.Windows.Forms.Label();
			this.labelDateIssued = new System.Windows.Forms.Label();
			this.butClaimPayEdit = new OpenPmsDental.UI.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.textClinic = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textCarrierName = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.textNote = new System.Windows.Forms.TextBox();
			this.textCheckNum = new System.Windows.Forms.TextBox();
			this.textBankBranch = new System.Windows.Forms.TextBox();
			this.textAmount = new OpenPmsDental.ValidDouble();
			this.textDate = new OpenPmsDental.ValidDate();
			this.gridAttached = new OpenPmsDental.UI.GridOD();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.butViewEra = new OpenPmsDental.UI.Button();
			this.groupFilters.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuRightAttached
			// 
			this.menuRightAttached.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemGotoAccount});
			// 
			// menuItemGotoAccount
			// 
			this.menuItemGotoAccount.Text = "Go to Account";
			this.menuItemGotoAccount.Click += new System.EventHandler(this.menuItemGotoAccount_Click);
			// 
			// menuRightOut
			// 
			this.menuRightOut.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemGotoOut});
			// 
			// menuItemGotoOut
			// 
			this.menuItemGotoOut.Text = "Go to Account";
			this.menuItemGotoOut.Click += new System.EventHandler(this.menuItemGotoOut_Click);
			// 
			// groupFilters
			// 
			this.groupFilters.Controls.Add(this.butCarrierPick);
			this.groupFilters.Controls.Add(this.textCarrier);
			this.groupFilters.Controls.Add(this.label12);
			this.groupFilters.Controls.Add(this.label9);
			this.groupFilters.Controls.Add(this.label11);
			this.groupFilters.Controls.Add(this.textName);
			this.groupFilters.Controls.Add(this.textClaimID);
			this.groupFilters.Location = new System.Drawing.Point(232, 400);
			this.groupFilters.Name = "groupFilters";
			this.groupFilters.Size = new System.Drawing.Size(734, 54);
			this.groupFilters.TabIndex = 119;
			this.groupFilters.TabStop = false;
			this.groupFilters.Text = "Filters";
			// 
			// butCarrierPick
			// 
			this.butCarrierPick.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butCarrierPick.Location = new System.Drawing.Point(218, 23);
			this.butCarrierPick.Name = "butCarrierPick";
			this.butCarrierPick.Size = new System.Drawing.Size(24, 23);
			this.butCarrierPick.TabIndex = 120;
			this.butCarrierPick.Text = "...";
			this.butCarrierPick.Click += new System.EventHandler(this.butCarrierPick_Click);
			// 
			// textCarrier
			// 
			this.textCarrier.Location = new System.Drawing.Point(102, 23);
			this.textCarrier.Name = "textCarrier";
			this.textCarrier.Size = new System.Drawing.Size(110, 23);
			this.textCarrier.TabIndex = 112;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(258, 23);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(44, 23);
			this.label12.TabIndex = 118;
			this.label12.Text = "Name";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(48, 23);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(48, 23);
			this.label9.TabIndex = 113;
			this.label9.Text = "Carrier";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(432, 23);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(56, 23);
			this.label11.TabIndex = 117;
			this.label11.Text = "Claim ID";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textName
			// 
			this.textName.Location = new System.Drawing.Point(308, 23);
			this.textName.Name = "textName";
			this.textName.Size = new System.Drawing.Size(108, 23);
			this.textName.TabIndex = 113;
			// 
			// textClaimID
			// 
			this.textClaimID.Location = new System.Drawing.Point(494, 23);
			this.textClaimID.Name = "textClaimID";
			this.textClaimID.Size = new System.Drawing.Size(108, 23);
			this.textClaimID.TabIndex = 114;
			// 
			// butRefresh
			// 
			this.butRefresh.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butRefresh.Location = new System.Drawing.Point(488, 368);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(60, 24);
			this.butRefresh.TabIndex = 114;
			this.butRefresh.Text = "Refresh";
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// butDown
			// 
			this.butDown.Icon = OpenPmsDental.UI.EnumIcons.Down;
			this.butDown.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDown.Location = new System.Drawing.Point(277, 368);
			this.butDown.Name = "butDown";
			this.butDown.Size = new System.Drawing.Size(39, 24);
			this.butDown.TabIndex = 104;
			this.butDown.Text = "#";
			this.butDown.Click += new System.EventHandler(this.butDown_Click);
			// 
			// textTotal
			// 
			this.textTotal.Location = new System.Drawing.Point(885, 368);
			this.textTotal.MaxVal = 100000000D;
			this.textTotal.MinVal = -100000000D;
			this.textTotal.Name = "textTotal";
			this.textTotal.ReadOnly = true;
			this.textTotal.Size = new System.Drawing.Size(81, 23);
			this.textTotal.TabIndex = 0;
			this.textTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// butAttach
			// 
			this.butAttach.Icon = OpenPmsDental.UI.EnumIcons.Up;
			this.butAttach.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAttach.Location = new System.Drawing.Point(554, 368);
			this.butAttach.Name = "butAttach";
			this.butAttach.Size = new System.Drawing.Size(75, 24);
			this.butAttach.TabIndex = 111;
			this.butAttach.Text = "Attach";
			this.butAttach.Click += new System.EventHandler(this.butAttach_Click);
			// 
			// textEobIsScanned
			// 
			this.textEobIsScanned.Location = new System.Drawing.Point(144, 585);
			this.textEobIsScanned.MaxLength = 25;
			this.textEobIsScanned.Name = "textEobIsScanned";
			this.textEobIsScanned.ReadOnly = true;
			this.textEobIsScanned.Size = new System.Drawing.Size(72, 23);
			this.textEobIsScanned.TabIndex = 110;
			// 
			// butView
			// 
			this.butView.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butView.Location = new System.Drawing.Point(144, 612);
			this.butView.Name = "butView";
			this.butView.Size = new System.Drawing.Size(72, 24);
			this.butView.TabIndex = 109;
			this.butView.Text = "View EOB";
			this.butView.Click += new System.EventHandler(this.butView_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(46, 585);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(92, 23);
			this.label1.TabIndex = 108;
			this.label1.Text = "EOB is Scanned";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(891, 644);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 107;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// labelInstruct2
			// 
			this.labelInstruct2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelInstruct2.Location = new System.Drawing.Point(8, 29);
			this.labelInstruct2.Name = "labelInstruct2";
			this.labelInstruct2.Size = new System.Drawing.Size(208, 523);
			this.labelInstruct2.TabIndex = 105;
			this.labelInstruct2.Text = resources.GetString("labelInstruct2.Text");
			// 
			// butUp
			// 
			this.butUp.Icon = OpenPmsDental.UI.EnumIcons.Up;
			this.butUp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butUp.Location = new System.Drawing.Point(232, 368);
			this.butUp.Name = "butUp";
			this.butUp.Size = new System.Drawing.Size(39, 24);
			this.butUp.TabIndex = 103;
			this.butUp.Text = "#";
			this.butUp.Click += new System.EventHandler(this.butUp_Click);
			// 
			// labelInstruct1
			// 
			this.labelInstruct1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelInstruct1.Location = new System.Drawing.Point(8, 8);
			this.labelInstruct1.Name = "labelInstruct1";
			this.labelInstruct1.Size = new System.Drawing.Size(208, 17);
			this.labelInstruct1.TabIndex = 102;
			this.labelInstruct1.Text = "Instructions";
			this.labelInstruct1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// butDetach
			// 
			this.butDetach.Icon = OpenPmsDental.UI.EnumIcons.Down;
			this.butDetach.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDetach.Location = new System.Drawing.Point(635, 368);
			this.butDetach.Name = "butDetach";
			this.butDetach.Size = new System.Drawing.Size(75, 24);
			this.butDetach.TabIndex = 101;
			this.butDetach.Text = "Detach";
			this.butDetach.Click += new System.EventHandler(this.butDetach_Click);
			// 
			// gridOut
			// 
			this.gridOut.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.gridOut.Location = new System.Drawing.Point(232, 462);
			this.gridOut.Name = "gridOut";
			this.gridOut.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridOut.Size = new System.Drawing.Size(734, 174);
			this.gridOut.TabIndex = 99;
			this.gridOut.Title = "All Outstanding Claims";
			this.gridOut.TranslationName = "TableClaimPaySplits";
			this.gridOut.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridOut_CellDoubleClick);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label13);
			this.groupBox1.Controls.Add(this.textPayGroup);
			this.groupBox1.Controls.Add(this.label10);
			this.groupBox1.Controls.Add(this.textPayType);
			this.groupBox1.Controls.Add(this.labelClinic);
			this.groupBox1.Controls.Add(this.textDateIssued);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.labelDateIssued);
			this.groupBox1.Controls.Add(this.butClaimPayEdit);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.textClinic);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.textCarrierName);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.textNote);
			this.groupBox1.Controls.Add(this.textCheckNum);
			this.groupBox1.Controls.Add(this.textBankBranch);
			this.groupBox1.Controls.Add(this.textAmount);
			this.groupBox1.Controls.Add(this.textDate);
			this.groupBox1.Location = new System.Drawing.Point(232, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(734, 135);
			this.groupBox1.TabIndex = 98;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Payment Details";
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(444, 104);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(44, 23);
			this.label13.TabIndex = 101;
			this.label13.Text = "Group";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPayGroup
			// 
			this.textPayGroup.AutoSize = false;
			this.textPayGroup.Location = new System.Drawing.Point(494, 104);
			this.textPayGroup.MaxLength = 25;
			this.textPayGroup.Name = "textPayGroup";
			this.textPayGroup.ReadOnly = true;
			this.textPayGroup.Size = new System.Drawing.Size(141, 23);
			this.textPayGroup.TabIndex = 100;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(444, 77);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(44, 23);
			this.label10.TabIndex = 99;
			this.label10.Text = "Type";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPayType
			// 
			this.textPayType.AutoSize = false;
			this.textPayType.Location = new System.Drawing.Point(494, 77);
			this.textPayType.MaxLength = 25;
			this.textPayType.Name = "textPayType";
			this.textPayType.ReadOnly = true;
			this.textPayType.Size = new System.Drawing.Size(141, 23);
			this.textPayType.TabIndex = 98;
			// 
			// labelClinic
			// 
			this.labelClinic.Location = new System.Drawing.Point(8, 23);
			this.labelClinic.Name = "labelClinic";
			this.labelClinic.Size = new System.Drawing.Size(88, 23);
			this.labelClinic.TabIndex = 91;
			this.labelClinic.Text = "Clinic";
			this.labelClinic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDateIssued
			// 
			this.textDateIssued.AutoSize = false;
			this.textDateIssued.Location = new System.Drawing.Point(102, 77);
			this.textDateIssued.Name = "textDateIssued";
			this.textDateIssued.ReadOnly = true;
			this.textDateIssued.Size = new System.Drawing.Size(100, 23);
			this.textDateIssued.TabIndex = 96;
			this.textDateIssued.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(218, 50);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(84, 23);
			this.label2.TabIndex = 33;
			this.label2.Text = "Note";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDateIssued
			// 
			this.labelDateIssued.Location = new System.Drawing.Point(8, 77);
			this.labelDateIssued.Name = "labelDateIssued";
			this.labelDateIssued.Size = new System.Drawing.Size(88, 23);
			this.labelDateIssued.TabIndex = 97;
			this.labelDateIssued.Text = "Date Issued";
			this.labelDateIssued.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butClaimPayEdit
			// 
			this.butClaimPayEdit.Location = new System.Drawing.Point(651, 104);
			this.butClaimPayEdit.Name = "butClaimPayEdit";
			this.butClaimPayEdit.Size = new System.Drawing.Size(75, 24);
			this.butClaimPayEdit.TabIndex = 6;
			this.butClaimPayEdit.Text = "Edit";
			this.butClaimPayEdit.Click += new System.EventHandler(this.butClaimPayEdit_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(218, 104);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(84, 23);
			this.label3.TabIndex = 34;
			this.label3.Text = "Bank-Branch";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(218, 77);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(84, 23);
			this.label4.TabIndex = 35;
			this.label4.Text = "Check #";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textClinic
			// 
			this.textClinic.AutoSize = false;
			this.textClinic.Location = new System.Drawing.Point(102, 23);
			this.textClinic.MaxLength = 25;
			this.textClinic.Name = "textClinic";
			this.textClinic.ReadOnly = true;
			this.textClinic.Size = new System.Drawing.Size(100, 23);
			this.textClinic.TabIndex = 93;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 104);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(88, 23);
			this.label5.TabIndex = 36;
			this.label5.Text = "Amount";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textCarrierName
			// 
			this.textCarrierName.AutoSize = false;
			this.textCarrierName.Location = new System.Drawing.Point(308, 23);
			this.textCarrierName.MaxLength = 25;
			this.textCarrierName.Name = "textCarrierName";
			this.textCarrierName.ReadOnly = true;
			this.textCarrierName.Size = new System.Drawing.Size(327, 23);
			this.textCarrierName.TabIndex = 93;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(8, 50);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(88, 23);
			this.label6.TabIndex = 37;
			this.label6.Text = "Payment Date";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(218, 23);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(84, 23);
			this.label7.TabIndex = 94;
			this.label7.Text = "Carrier Name";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textNote
			// 
			this.textNote.Location = new System.Drawing.Point(308, 50);
			this.textNote.MaxLength = 255;
			this.textNote.Multiline = true;
			this.textNote.Name = "textNote";
			this.textNote.ReadOnly = true;
			this.textNote.Size = new System.Drawing.Size(327, 23);
			this.textNote.TabIndex = 3;
			// 
			// textCheckNum
			// 
			this.textCheckNum.AutoSize = false;
			this.textCheckNum.Location = new System.Drawing.Point(308, 77);
			this.textCheckNum.MaxLength = 25;
			this.textCheckNum.Name = "textCheckNum";
			this.textCheckNum.ReadOnly = true;
			this.textCheckNum.Size = new System.Drawing.Size(120, 23);
			this.textCheckNum.TabIndex = 1;
			// 
			// textBankBranch
			// 
			this.textBankBranch.AutoSize = false;
			this.textBankBranch.Location = new System.Drawing.Point(308, 104);
			this.textBankBranch.MaxLength = 25;
			this.textBankBranch.Name = "textBankBranch";
			this.textBankBranch.ReadOnly = true;
			this.textBankBranch.Size = new System.Drawing.Size(120, 23);
			this.textBankBranch.TabIndex = 2;
			// 
			// textAmount
			// 
			this.textAmount.AutoSize = false;
			this.textAmount.Location = new System.Drawing.Point(102, 104);
			this.textAmount.MaxVal = 100000000D;
			this.textAmount.MinVal = -100000000D;
			this.textAmount.Name = "textAmount";
			this.textAmount.ReadOnly = true;
			this.textAmount.Size = new System.Drawing.Size(100, 23);
			this.textAmount.TabIndex = 0;
			this.textAmount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textDate
			// 
			this.textDate.AutoSize = false;
			this.textDate.Location = new System.Drawing.Point(102, 50);
			this.textDate.Name = "textDate";
			this.textDate.ReadOnly = true;
			this.textDate.Size = new System.Drawing.Size(100, 23);
			this.textDate.TabIndex = 3;
			this.textDate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// gridAttached
			// 
			this.gridAttached.Location = new System.Drawing.Point(232, 151);
			this.gridAttached.Name = "gridAttached";
			this.gridAttached.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridAttached.Size = new System.Drawing.Size(734, 209);
			this.gridAttached.TabIndex = 95;
			this.gridAttached.Title = "Attached to this Payment";
			this.gridAttached.TranslationName = "TableClaimPaySplits";
			this.gridAttached.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridAttached_CellDoubleClick);
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 644);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(79, 24);
			this.butDelete.TabIndex = 52;
			this.butDelete.Text = "&Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(787, 368);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(92, 23);
			this.label8.TabIndex = 36;
			this.label8.Text = "Total Payments";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butViewEra
			// 
			this.butViewEra.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butViewEra.Location = new System.Drawing.Point(66, 612);
			this.butViewEra.Name = "butViewEra";
			this.butViewEra.Size = new System.Drawing.Size(72, 24);
			this.butViewEra.TabIndex = 120;
			this.butViewEra.Text = "View ERA";
			this.butViewEra.Click += new System.EventHandler(this.butViewEra_Click);
			// 
			// FormClaimPayBatch
			// 
			this.ClientSize = new System.Drawing.Size(974, 676);
			this.Controls.Add(this.butViewEra);
			this.Controls.Add(this.groupFilters);
			this.Controls.Add(this.butRefresh);
			this.Controls.Add(this.butDown);
			this.Controls.Add(this.textTotal);
			this.Controls.Add(this.butAttach);
			this.Controls.Add(this.textEobIsScanned);
			this.Controls.Add(this.butView);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.labelInstruct2);
			this.Controls.Add(this.butUp);
			this.Controls.Add(this.labelInstruct1);
			this.Controls.Add(this.butDetach);
			this.Controls.Add(this.gridOut);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.gridAttached);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.label8);
			this.MinimizeBox = false;
			this.Name = "FormClaimPayBatch";
			this.Text = "Insurance Payment (EOB)";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormClaimPayBatch_FormClosing);
			this.Load += new System.EventHandler(this.FormClaimPayEdit_Load);
			this.groupFilters.ResumeLayout(false);
			this.groupFilters.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
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
		private OpenPmsDental.UI.Button butDelete;
		private System.Windows.Forms.Label labelClinic;
		private System.Windows.Forms.TextBox textCarrierName;
		private System.Windows.Forms.Label label7;
		private OpenPmsDental.UI.GridOD gridAttached;
		private ValidDate textDateIssued;
		private Label labelDateIssued;
		private TextBox textClinic;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private UI.Button butClaimPayEdit;
		private OpenPmsDental.UI.GridOD gridOut;
		private UI.Button butDetach;
		private ValidDouble textTotal;
		private Label label8;
		private Label labelInstruct1;
		private UI.Button butDown;
		private UI.Button butUp;
		private Label labelInstruct2;
		private System.Windows.Forms.ContextMenuStrip menuRightAttached;
		private System.Windows.Forms.ToolStripMenuItem menuItemGotoAccount;
		private System.Windows.Forms.ContextMenuStrip menuRightOut;
		private System.Windows.Forms.ToolStripMenuItem menuItemGotoOut;
		private UI.Button butSave;
		private Label label1;
		private UI.Button butView;
		private TextBox textEobIsScanned;
		private UI.Button butAttach;
		private TextBox textCarrier;
		private Label label9;
		private UI.Button butRefresh;
		private Label label10;
		private TextBox textPayType;
		private TextBox textClaimID;
		private TextBox textName;
		private Label label11;
		private Label label12;
		private OpenPmsDental.UI.GroupBox groupFilters;
		private Label label13;
		private TextBox textPayGroup;
		private UI.Button butCarrierPick;
		private UI.Button butViewEra;
	}
}
