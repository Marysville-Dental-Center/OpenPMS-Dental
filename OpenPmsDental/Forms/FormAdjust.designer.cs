namespace OpenPmsDental {
	partial class FormAdjust {
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
		private void InitializeComponent(){
			this.label1 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.labelAdditions = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.butOK = new OpenPmsDental.UI.Button();
			this.textAdjDate = new OpenPmsDental.ValidDate();
			this.labelSubtractions = new System.Windows.Forms.Label();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.textAmount = new OpenPmsDental.ValidDouble();
			this.listTypePos = new OpenPmsDental.UI.ListBox();
			this.listTypeNeg = new OpenPmsDental.UI.ListBox();
			this.textProcDate = new OpenPmsDental.ValidDate();
			this.label7 = new System.Windows.Forms.Label();
			this.textDateEntry = new OpenPmsDental.ValidDate();
			this.label8 = new System.Windows.Forms.Label();
			this.butPickProv = new OpenPmsDental.UI.Button();
			this.comboProv = new OpenPmsDental.UI.ComboBox();
			this.comboClinic = new OpenPmsDental.UI.ComboBoxClinicPicker();
			this.textNote = new OpenPmsDental.ODtextBox();
			this.groupProcedure = new OpenPmsDental.UI.GroupBox();
			this.labelProcDisabled = new System.Windows.Forms.Label();
			this.butEditAnyway = new OpenPmsDental.UI.Button();
			this.textProcWriteoff = new System.Windows.Forms.TextBox();
			this.labelEditAnyway = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.textProcTooth = new System.Windows.Forms.TextBox();
			this.labelProcTooth = new System.Windows.Forms.Label();
			this.textProcProv = new System.Windows.Forms.TextBox();
			this.textProcDescription = new System.Windows.Forms.TextBox();
			this.textProcDate2 = new System.Windows.Forms.TextBox();
			this.labelProcRemain = new System.Windows.Forms.Label();
			this.textProcAdjCur = new System.Windows.Forms.TextBox();
			this.textProcPatPaid = new System.Windows.Forms.TextBox();
			this.textProcAdj = new System.Windows.Forms.TextBox();
			this.textProcInsEst = new System.Windows.Forms.TextBox();
			this.textProcInsPaid = new System.Windows.Forms.TextBox();
			this.textProcFee = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.label19 = new System.Windows.Forms.Label();
			this.butDetachProc = new OpenPmsDental.UI.Button();
			this.butAttachProc = new OpenPmsDental.UI.Button();
			this.checkOnlyTsiExcludedAdjTypes = new OpenPmsDental.UI.CheckBox();
			this.radioFixedAmount = new System.Windows.Forms.RadioButton();
			this.radioPercentOfFee = new System.Windows.Forms.RadioButton();
			this.labelPercentOfFeeAmount = new System.Windows.Forms.Label();
			this.groupProcedure.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 35);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Adjustment Date";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 449);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 18);
			this.label4.TabIndex = 3;
			this.label4.Text = "Note";
			this.label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 89);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(100, 23);
			this.label5.TabIndex = 4;
			this.label5.Text = "Amount";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelAdditions
			// 
			this.labelAdditions.Location = new System.Drawing.Point(347, 8);
			this.labelAdditions.Name = "labelAdditions";
			this.labelAdditions.Size = new System.Drawing.Size(184, 18);
			this.labelAdditions.TabIndex = 5;
			this.labelAdditions.Text = "Additions";
			this.labelAdditions.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 116);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 23);
			this.label2.TabIndex = 10;
			this.label2.Text = "Provider";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(648, 556);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 24);
			this.butOK.TabIndex = 6;
			this.butOK.Text = "&Save";
			this.butOK.Click += new System.EventHandler(this.butSave_Click);
			// 
			// textAdjDate
			// 
			this.textAdjDate.Location = new System.Drawing.Point(114, 35);
			this.textAdjDate.Name = "textAdjDate";
			this.textAdjDate.Size = new System.Drawing.Size(80, 23);
			this.textAdjDate.TabIndex = 8;
			// 
			// labelSubtractions
			// 
			this.labelSubtractions.Location = new System.Drawing.Point(539, 8);
			this.labelSubtractions.Name = "labelSubtractions";
			this.labelSubtractions.Size = new System.Drawing.Size(184, 18);
			this.labelSubtractions.TabIndex = 16;
			this.labelSubtractions.Text = "Subtractions";
			this.labelSubtractions.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Location = new System.Drawing.Point(8, 556);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 24);
			this.butDelete.TabIndex = 17;
			this.butDelete.Text = "&Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// textAmount
			// 
			this.textAmount.Location = new System.Drawing.Point(114, 89);
			this.textAmount.MaxVal = 100000000D;
			this.textAmount.MinVal = -100000000D;
			this.textAmount.Name = "textAmount";
			this.textAmount.Size = new System.Drawing.Size(80, 23);
			this.textAmount.TabIndex = 0;
			this.textAmount.Validating += new System.ComponentModel.CancelEventHandler(this.textAmount_Validating);
			this.textAmount.TextChanged += new System.EventHandler(this.textAmount_TextChanged);
			// 
			// listTypePos
			// 
			this.listTypePos.Location = new System.Drawing.Point(347, 28);
			this.listTypePos.Name = "listTypePos";
			this.listTypePos.Size = new System.Drawing.Size(184, 162);
			this.listTypePos.TabIndex = 3;
			this.listTypePos.SelectedIndexChanged += new System.EventHandler(this.listTypePos_SelectedIndexChanged);
			// 
			// listTypeNeg
			// 
			this.listTypeNeg.Location = new System.Drawing.Point(539, 28);
			this.listTypeNeg.Name = "listTypeNeg";
			this.listTypeNeg.Size = new System.Drawing.Size(184, 162);
			this.listTypeNeg.TabIndex = 4;
			this.listTypeNeg.SelectedIndexChanged += new System.EventHandler(this.listTypeNeg_SelectedIndexChanged);
			// 
			// textProcDate
			// 
			this.textProcDate.Location = new System.Drawing.Point(114, 62);
			this.textProcDate.Name = "textProcDate";
			this.textProcDate.Size = new System.Drawing.Size(80, 23);
			this.textProcDate.TabIndex = 9;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(8, 62);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(100, 23);
			this.label7.TabIndex = 18;
			this.label7.Text = "(procedure date)";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDateEntry
			// 
			this.textDateEntry.Location = new System.Drawing.Point(114, 8);
			this.textDateEntry.Name = "textDateEntry";
			this.textDateEntry.ReadOnly = true;
			this.textDateEntry.Size = new System.Drawing.Size(80, 23);
			this.textDateEntry.TabIndex = 21;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(8, 8);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(100, 23);
			this.label8.TabIndex = 20;
			this.label8.Text = "Entry Date";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butPickProv
			// 
			this.butPickProv.Location = new System.Drawing.Point(272, 116);
			this.butPickProv.Name = "butPickProv";
			this.butPickProv.Size = new System.Drawing.Size(24, 23);
			this.butPickProv.TabIndex = 165;
			this.butPickProv.Text = "...";
			this.butPickProv.Click += new System.EventHandler(this.butPickProv_Click);
			// 
			// comboProv
			// 
			this.comboProv.Location = new System.Drawing.Point(114, 116);
			this.comboProv.Name = "comboProv";
			this.comboProv.Size = new System.Drawing.Size(152, 23);
			this.comboProv.TabIndex = 1;
			// 
			// comboClinic
			// 
			this.comboClinic.IncludeUnassigned = true;
			this.comboClinic.Location = new System.Drawing.Point(77, 143);
			this.comboClinic.Name = "comboClinic";
			this.comboClinic.Size = new System.Drawing.Size(219, 23);
			this.comboClinic.TabIndex = 2;
			this.comboClinic.SelectionChangeCommitted += new System.EventHandler(this.ComboClinic_SelectionChangeCommitted);
			// 
			// textNote
			// 
			this.textNote.AcceptsTab = true;
			this.textNote.BackColor = System.Drawing.SystemColors.Window;
			this.textNote.DetectLinksEnabled = false;
			this.textNote.DetectUrls = false;
			this.textNote.Location = new System.Drawing.Point(8, 469);
			this.textNote.Name = "textNote";
			this.textNote.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.Adjustment;
			this.textNote.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textNote.Size = new System.Drawing.Size(715, 79);
			this.textNote.TabIndex = 5;
			this.textNote.Text = "";
			// 
			// groupProcedure
			// 
			this.groupProcedure.Controls.Add(this.labelProcDisabled);
			this.groupProcedure.Controls.Add(this.butEditAnyway);
			this.groupProcedure.Controls.Add(this.textProcWriteoff);
			this.groupProcedure.Controls.Add(this.labelEditAnyway);
			this.groupProcedure.Controls.Add(this.label16);
			this.groupProcedure.Controls.Add(this.textProcTooth);
			this.groupProcedure.Controls.Add(this.labelProcTooth);
			this.groupProcedure.Controls.Add(this.textProcProv);
			this.groupProcedure.Controls.Add(this.textProcDescription);
			this.groupProcedure.Controls.Add(this.textProcDate2);
			this.groupProcedure.Controls.Add(this.labelProcRemain);
			this.groupProcedure.Controls.Add(this.textProcAdjCur);
			this.groupProcedure.Controls.Add(this.textProcPatPaid);
			this.groupProcedure.Controls.Add(this.textProcAdj);
			this.groupProcedure.Controls.Add(this.textProcInsEst);
			this.groupProcedure.Controls.Add(this.textProcInsPaid);
			this.groupProcedure.Controls.Add(this.textProcFee);
			this.groupProcedure.Controls.Add(this.label13);
			this.groupProcedure.Controls.Add(this.label12);
			this.groupProcedure.Controls.Add(this.label11);
			this.groupProcedure.Controls.Add(this.label10);
			this.groupProcedure.Controls.Add(this.label9);
			this.groupProcedure.Controls.Add(this.label14);
			this.groupProcedure.Controls.Add(this.label15);
			this.groupProcedure.Controls.Add(this.label17);
			this.groupProcedure.Controls.Add(this.label18);
			this.groupProcedure.Controls.Add(this.label19);
			this.groupProcedure.Controls.Add(this.butDetachProc);
			this.groupProcedure.Controls.Add(this.butAttachProc);
			this.groupProcedure.Location = new System.Drawing.Point(8, 198);
			this.groupProcedure.Name = "groupProcedure";
			this.groupProcedure.Size = new System.Drawing.Size(715, 243);
			this.groupProcedure.TabIndex = 166;
			this.groupProcedure.Text = "Procedure";
			// 
			// labelProcDisabled
			// 
			this.labelProcDisabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelProcDisabled.ForeColor = System.Drawing.Color.Firebrick;
			this.labelProcDisabled.Location = new System.Drawing.Point(106, 51);
			this.labelProcDisabled.Name = "labelProcDisabled";
			this.labelProcDisabled.Size = new System.Drawing.Size(236, 69);
			this.labelProcDisabled.TabIndex = 169;
			this.labelProcDisabled.Text = "Procedures cannot be attached to adjustments unless all payments or payment plans" +
    " for the adjustment are removed.";
			this.labelProcDisabled.Visible = false;
			// 
			// butEditAnyway
			// 
			this.butEditAnyway.Location = new System.Drawing.Point(264, 209);
			this.butEditAnyway.Name = "butEditAnyway";
			this.butEditAnyway.Size = new System.Drawing.Size(75, 24);
			this.butEditAnyway.TabIndex = 167;
			this.butEditAnyway.Text = "Edit Anyway";
			this.butEditAnyway.Visible = false;
			this.butEditAnyway.Click += new System.EventHandler(this.butEditAnyway_Click);
			// 
			// textProcWriteoff
			// 
			this.textProcWriteoff.AutoSize = false;
			this.textProcWriteoff.Location = new System.Drawing.Point(627, 50);
			this.textProcWriteoff.Name = "textProcWriteoff";
			this.textProcWriteoff.ReadOnly = true;
			this.textProcWriteoff.Size = new System.Drawing.Size(80, 23);
			this.textProcWriteoff.TabIndex = 50;
			this.textProcWriteoff.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// labelEditAnyway
			// 
			this.labelEditAnyway.Location = new System.Drawing.Point(8, 207);
			this.labelEditAnyway.Name = "labelEditAnyway";
			this.labelEditAnyway.Size = new System.Drawing.Size(250, 28);
			this.labelEditAnyway.TabIndex = 168;
			this.labelEditAnyway.Text = "This adjustment is attached to a procedure and should not be edited";
			this.labelEditAnyway.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelEditAnyway.Visible = false;
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(521, 50);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(100, 23);
			this.label16.TabIndex = 49;
			this.label16.Text = "Write-offs";
			this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textProcTooth
			// 
			this.textProcTooth.AutoSize = false;
			this.textProcTooth.Location = new System.Drawing.Point(106, 105);
			this.textProcTooth.Name = "textProcTooth";
			this.textProcTooth.ReadOnly = true;
			this.textProcTooth.Size = new System.Drawing.Size(44, 23);
			this.textProcTooth.TabIndex = 46;
			// 
			// labelProcTooth
			// 
			this.labelProcTooth.Location = new System.Drawing.Point(28, 105);
			this.labelProcTooth.Name = "labelProcTooth";
			this.labelProcTooth.Size = new System.Drawing.Size(72, 23);
			this.labelProcTooth.TabIndex = 45;
			this.labelProcTooth.Text = "Tooth";
			this.labelProcTooth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textProcProv
			// 
			this.textProcProv.AutoSize = false;
			this.textProcProv.Location = new System.Drawing.Point(106, 78);
			this.textProcProv.Name = "textProcProv";
			this.textProcProv.ReadOnly = true;
			this.textProcProv.Size = new System.Drawing.Size(80, 23);
			this.textProcProv.TabIndex = 44;
			// 
			// textProcDescription
			// 
			this.textProcDescription.AutoSize = false;
			this.textProcDescription.Location = new System.Drawing.Point(106, 132);
			this.textProcDescription.Name = "textProcDescription";
			this.textProcDescription.ReadOnly = true;
			this.textProcDescription.Size = new System.Drawing.Size(240, 23);
			this.textProcDescription.TabIndex = 43;
			// 
			// textProcDate2
			// 
			this.textProcDate2.AutoSize = false;
			this.textProcDate2.Location = new System.Drawing.Point(106, 51);
			this.textProcDate2.Name = "textProcDate2";
			this.textProcDate2.ReadOnly = true;
			this.textProcDate2.Size = new System.Drawing.Size(80, 23);
			this.textProcDate2.TabIndex = 42;
			// 
			// labelProcRemain
			// 
			this.labelProcRemain.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelProcRemain.Location = new System.Drawing.Point(627, 212);
			this.labelProcRemain.Name = "labelProcRemain";
			this.labelProcRemain.Size = new System.Drawing.Size(80, 23);
			this.labelProcRemain.TabIndex = 41;
			this.labelProcRemain.Text = "$0.00";
			this.labelProcRemain.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textProcAdjCur
			// 
			this.textProcAdjCur.AutoSize = false;
			this.textProcAdjCur.Location = new System.Drawing.Point(627, 185);
			this.textProcAdjCur.Name = "textProcAdjCur";
			this.textProcAdjCur.ReadOnly = true;
			this.textProcAdjCur.Size = new System.Drawing.Size(80, 23);
			this.textProcAdjCur.TabIndex = 40;
			this.textProcAdjCur.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textProcPatPaid
			// 
			this.textProcPatPaid.AutoSize = false;
			this.textProcPatPaid.Location = new System.Drawing.Point(627, 158);
			this.textProcPatPaid.Name = "textProcPatPaid";
			this.textProcPatPaid.ReadOnly = true;
			this.textProcPatPaid.Size = new System.Drawing.Size(80, 23);
			this.textProcPatPaid.TabIndex = 39;
			this.textProcPatPaid.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textProcAdj
			// 
			this.textProcAdj.AutoSize = false;
			this.textProcAdj.Location = new System.Drawing.Point(627, 131);
			this.textProcAdj.Name = "textProcAdj";
			this.textProcAdj.ReadOnly = true;
			this.textProcAdj.Size = new System.Drawing.Size(80, 23);
			this.textProcAdj.TabIndex = 38;
			this.textProcAdj.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textProcInsEst
			// 
			this.textProcInsEst.AutoSize = false;
			this.textProcInsEst.Location = new System.Drawing.Point(627, 104);
			this.textProcInsEst.Name = "textProcInsEst";
			this.textProcInsEst.ReadOnly = true;
			this.textProcInsEst.Size = new System.Drawing.Size(80, 23);
			this.textProcInsEst.TabIndex = 37;
			this.textProcInsEst.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textProcInsPaid
			// 
			this.textProcInsPaid.AutoSize = false;
			this.textProcInsPaid.Location = new System.Drawing.Point(627, 77);
			this.textProcInsPaid.Name = "textProcInsPaid";
			this.textProcInsPaid.ReadOnly = true;
			this.textProcInsPaid.Size = new System.Drawing.Size(80, 23);
			this.textProcInsPaid.TabIndex = 36;
			this.textProcInsPaid.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textProcFee
			// 
			this.textProcFee.AutoSize = false;
			this.textProcFee.Location = new System.Drawing.Point(627, 23);
			this.textProcFee.Name = "textProcFee";
			this.textProcFee.ReadOnly = true;
			this.textProcFee.Size = new System.Drawing.Size(80, 23);
			this.textProcFee.TabIndex = 35;
			this.textProcFee.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(521, 185);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(100, 23);
			this.label13.TabIndex = 34;
			this.label13.Text = "This Adjustment";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(521, 212);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(100, 23);
			this.label12.TabIndex = 33;
			this.label12.Text = "Remaining";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(521, 158);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(100, 23);
			this.label11.TabIndex = 32;
			this.label11.Text = "Patient Paid";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(521, 131);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(100, 23);
			this.label10.TabIndex = 31;
			this.label10.Text = "Adjustments";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(521, 104);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(100, 23);
			this.label9.TabIndex = 30;
			this.label9.Text = "Ins Est";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(521, 77);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(100, 23);
			this.label14.TabIndex = 29;
			this.label14.Text = "Ins Paid";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(521, 23);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(100, 23);
			this.label15.TabIndex = 28;
			this.label15.Text = "Fee";
			this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(28, 78);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(72, 23);
			this.label17.TabIndex = 27;
			this.label17.Text = "Provider";
			this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(28, 132);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(72, 23);
			this.label18.TabIndex = 26;
			this.label18.Text = "Description";
			this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(28, 51);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(72, 23);
			this.label19.TabIndex = 25;
			this.label19.Text = "Date";
			this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butDetachProc
			// 
			this.butDetachProc.Location = new System.Drawing.Point(89, 23);
			this.butDetachProc.Name = "butDetachProc";
			this.butDetachProc.Size = new System.Drawing.Size(75, 24);
			this.butDetachProc.TabIndex = 9;
			this.butDetachProc.Text = "Detach";
			this.butDetachProc.Click += new System.EventHandler(this.butDetachProc_Click);
			// 
			// butAttachProc
			// 
			this.butAttachProc.Location = new System.Drawing.Point(8, 23);
			this.butAttachProc.Name = "butAttachProc";
			this.butAttachProc.Size = new System.Drawing.Size(75, 24);
			this.butAttachProc.TabIndex = 8;
			this.butAttachProc.Text = "Attach";
			this.butAttachProc.Click += new System.EventHandler(this.butAttachProc_Click);
			// 
			// checkOnlyTsiExcludedAdjTypes
			// 
			this.checkOnlyTsiExcludedAdjTypes.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkOnlyTsiExcludedAdjTypes.Location = new System.Drawing.Point(114, 170);
			this.checkOnlyTsiExcludedAdjTypes.Name = "checkOnlyTsiExcludedAdjTypes";
			this.checkOnlyTsiExcludedAdjTypes.Size = new System.Drawing.Size(217, 20);
			this.checkOnlyTsiExcludedAdjTypes.TabIndex = 171;
			this.checkOnlyTsiExcludedAdjTypes.Text = "Only TSI excluded adjustment types";
			this.checkOnlyTsiExcludedAdjTypes.CheckedChanged += new System.EventHandler(this.checkOnlyTsiExcludedAdjTypes_Checked);
			// 
			// radioFixedAmount
			// 
			this.radioFixedAmount.Checked = true;
			this.radioFixedAmount.Location = new System.Drawing.Point(210, 36);
			this.radioFixedAmount.Name = "radioFixedAmount";
			this.radioFixedAmount.Size = new System.Drawing.Size(99, 20);
			this.radioFixedAmount.TabIndex = 172;
			this.radioFixedAmount.TabStop = true;
			this.radioFixedAmount.Text = "Fixed Amount";
			this.radioFixedAmount.UseVisualStyleBackColor = true;
			// 
			// radioPercentOfFee
			// 
			this.radioPercentOfFee.Enabled = false;
			this.radioPercentOfFee.Location = new System.Drawing.Point(210, 63);
			this.radioPercentOfFee.Name = "radioPercentOfFee";
			this.radioPercentOfFee.Size = new System.Drawing.Size(100, 20);
			this.radioPercentOfFee.TabIndex = 173;
			this.radioPercentOfFee.Text = "Percent of Fee";
			this.radioPercentOfFee.UseVisualStyleBackColor = true;
			this.radioPercentOfFee.CheckedChanged += new System.EventHandler(this.radioPercentOfFee_CheckedChanged);
			// 
			// labelPercentOfFeeAmount
			// 
			this.labelPercentOfFeeAmount.Location = new System.Drawing.Point(210, 89);
			this.labelPercentOfFeeAmount.Name = "labelPercentOfFeeAmount";
			this.labelPercentOfFeeAmount.Size = new System.Drawing.Size(56, 23);
			this.labelPercentOfFeeAmount.TabIndex = 174;
			this.labelPercentOfFeeAmount.Text = "Amount";
			this.labelPercentOfFeeAmount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelPercentOfFeeAmount.Visible = false;
			// 
			// FormAdjust
			// 
			this.ClientSize = new System.Drawing.Size(731, 588);
			this.Controls.Add(this.labelPercentOfFeeAmount);
			this.Controls.Add(this.radioPercentOfFee);
			this.Controls.Add(this.radioFixedAmount);
			this.Controls.Add(this.checkOnlyTsiExcludedAdjTypes);
			this.Controls.Add(this.groupProcedure);
			this.Controls.Add(this.butPickProv);
			this.Controls.Add(this.comboProv);
			this.Controls.Add(this.comboClinic);
			this.Controls.Add(this.textDateEntry);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textProcDate);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.textNote);
			this.Controls.Add(this.listTypeNeg);
			this.Controls.Add(this.listTypePos);
			this.Controls.Add(this.textAmount);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.labelSubtractions);
			this.Controls.Add(this.textAdjDate);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.labelAdditions);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormAdjust";
			this.ShowInTaskbar = false;
			this.Text = "Edit Adjustment";
			this.Load += new System.EventHandler(this.FormAdjust_Load);
			this.groupProcedure.ResumeLayout(false);
			this.groupProcedure.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label labelAdditions;
		private System.Windows.Forms.Label label2;
		private OpenPmsDental.UI.Button butOK;
		private System.Windows.Forms.Label labelSubtractions;
		private OpenPmsDental.UI.Button butDelete;
		private OpenPmsDental.ValidDouble textAmount;
		private OpenPmsDental.UI.ListBox listTypePos;
		private OpenPmsDental.UI.ListBox listTypeNeg;
		private OpenPmsDental.ODtextBox textNote;
		private OpenPmsDental.ValidDate textProcDate;
		private System.Windows.Forms.Label label7;
		private OpenPmsDental.ValidDate textAdjDate;
		private OpenPmsDental.ValidDate textDateEntry;
		private System.Windows.Forms.Label label8;
		private OpenPmsDental.UI.Button butPickProv;
		private UI.ComboBox comboProv;
		private UI.ComboBoxClinicPicker comboClinic;
		private OpenPmsDental.UI.GroupBox groupProcedure;
		private System.Windows.Forms.TextBox textProcWriteoff;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.TextBox textProcTooth;
		private System.Windows.Forms.Label labelProcTooth;
		private System.Windows.Forms.TextBox textProcProv;
		private System.Windows.Forms.TextBox textProcDescription;
		private System.Windows.Forms.TextBox textProcDate2;
		private System.Windows.Forms.Label labelProcRemain;
		private System.Windows.Forms.TextBox textProcAdjCur;
		private System.Windows.Forms.TextBox textProcPatPaid;
		private System.Windows.Forms.TextBox textProcAdj;
		private System.Windows.Forms.TextBox textProcInsEst;
		private System.Windows.Forms.TextBox textProcInsPaid;
		private System.Windows.Forms.TextBox textProcFee;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label19;
		private OpenPmsDental.UI.Button butDetachProc;
		private OpenPmsDental.UI.Button butAttachProc;
		private OpenPmsDental.UI.Button butEditAnyway;
		private System.Windows.Forms.Label labelEditAnyway;
		private System.Windows.Forms.Label labelProcDisabled;
		private UI.CheckBox checkOnlyTsiExcludedAdjTypes;
		private System.Windows.Forms.RadioButton radioFixedAmount;
		private System.Windows.Forms.RadioButton radioPercentOfFee;
		private System.Windows.Forms.Label labelPercentOfFeeAmount;
	}
}
