using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormCarrierEdit {
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
			this.textCarrierName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textPhone = new OpenPmsDental.ValidPhone();
			this.label3 = new System.Windows.Forms.Label();
			this.textAddress = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textAddress2 = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.labelElectID = new System.Windows.Forms.Label();
			this.textElectID = new System.Windows.Forms.TextBox();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.comboPlans = new OpenPmsDental.UI.ComboBox();
			this.textPlans = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.textCity = new System.Windows.Forms.TextBox();
			this.textState = new System.Windows.Forms.TextBox();
			this.textZip = new System.Windows.Forms.TextBox();
			this.labelCitySt = new System.Windows.Forms.Label();
			this.checkIsCDAnet = new OpenPmsDental.UI.CheckBox();
			this.groupCDAnet = new OpenPmsDental.UI.GroupBox();
			this.label14 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.textEncryptionMethod = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.groupBox3 = new OpenPmsDental.UI.GroupBox();
			this.check09 = new OpenPmsDental.UI.CheckBox();
			this.check01 = new OpenPmsDental.UI.CheckBox();
			this.check03m = new OpenPmsDental.UI.CheckBox();
			this.check03 = new OpenPmsDental.UI.CheckBox();
			this.check07 = new OpenPmsDental.UI.CheckBox();
			this.check06 = new OpenPmsDental.UI.CheckBox();
			this.check04 = new OpenPmsDental.UI.CheckBox();
			this.check05 = new OpenPmsDental.UI.CheckBox();
			this.check02 = new OpenPmsDental.UI.CheckBox();
			this.check08 = new OpenPmsDental.UI.CheckBox();
			this.textModemReconcile = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.textModemSummary = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.textModem = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.comboNetwork = new OpenPmsDental.UI.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.textVersion = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.checkIsHidden = new OpenPmsDental.UI.CheckBox();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.butSave = new OpenPmsDental.UI.Button();
			this.textCarrierNum = new System.Windows.Forms.TextBox();
			this.label21 = new System.Windows.Forms.Label();
			this.labelCarrierGroupName = new System.Windows.Forms.Label();
			this.labelColor = new System.Windows.Forms.Label();
			this.comboCarrierGroupName = new OpenPmsDental.UI.ComboBox();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.radioBenefitSendsPat = new System.Windows.Forms.RadioButton();
			this.radioBenefitSendsIns = new System.Windows.Forms.RadioButton();
			this.labelSendElectronically = new System.Windows.Forms.Label();
			this.comboSendElectronically = new OpenPmsDental.UI.ComboBox();
			this.checkRealTimeEligibility = new OpenPmsDental.UI.CheckBox();
			this.odColorPickerBack = new OpenPmsDental.UI.ODColorPicker();
			this.comboCobSendPaidByInsAt = new OpenPmsDental.UI.ComboBox();
			this.labelCobSendPaidByOtherInsAt = new System.Windows.Forms.Label();
			this.comboEraAutomation = new OpenPmsDental.UI.ComboBox();
			this.labelEraAutomation = new System.Windows.Forms.Label();
			this.comboOrthoConsolidate = new OpenPmsDental.UI.ComboBox();
			this.labelOrthoConsolidate = new System.Windows.Forms.Label();
			this.labelPreAuthCodes = new System.Windows.Forms.Label();
			this.textPreAuthCodes = new System.Windows.Forms.TextBox();
			this.butPreAuthCodes = new OpenPmsDental.UI.Button();
			this.groupBox1.SuspendLayout();
			this.groupCDAnet.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// textCarrierName
			// 
			this.textCarrierName.AutoSize = false;
			this.textCarrierName.Location = new System.Drawing.Point(210, 35);
			this.textCarrierName.MaxLength = 255;
			this.textCarrierName.Name = "textCarrierName";
			this.textCarrierName.Size = new System.Drawing.Size(224, 23);
			this.textCarrierName.TabIndex = 1;
			this.textCarrierName.TextChanged += new System.EventHandler(this.textCarrierName_TextChanged);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(156, 62);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(48, 23);
			this.label2.TabIndex = 0;
			this.label2.Text = "Phone";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPhone
			// 
			this.textPhone.AutoSize = false;
			this.textPhone.Location = new System.Drawing.Point(210, 62);
			this.textPhone.MaxLength = 255;
			this.textPhone.Name = "textPhone";
			this.textPhone.Size = new System.Drawing.Size(156, 23);
			this.textPhone.TabIndex = 2;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(148, 89);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(56, 23);
			this.label3.TabIndex = 0;
			this.label3.Text = "Address";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textAddress
			// 
			this.textAddress.AutoSize = false;
			this.textAddress.Location = new System.Drawing.Point(210, 89);
			this.textAddress.MaxLength = 255;
			this.textAddress.Name = "textAddress";
			this.textAddress.Size = new System.Drawing.Size(292, 23);
			this.textAddress.TabIndex = 3;
			this.textAddress.TextChanged += new System.EventHandler(this.textAddress_TextChanged);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(144, 116);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 23);
			this.label4.TabIndex = 0;
			this.label4.Text = "Address2";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textAddress2
			// 
			this.textAddress2.AutoSize = false;
			this.textAddress2.Location = new System.Drawing.Point(210, 116);
			this.textAddress2.MaxLength = 255;
			this.textAddress2.Name = "textAddress2";
			this.textAddress2.Size = new System.Drawing.Size(292, 23);
			this.textAddress2.TabIndex = 4;
			this.textAddress2.TextChanged += new System.EventHandler(this.textAddress2_TextChanged);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(120, 35);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(84, 23);
			this.label6.TabIndex = 0;
			this.label6.Text = "Carrier Name";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelElectID
			// 
			this.labelElectID.Location = new System.Drawing.Point(36, 170);
			this.labelElectID.Name = "labelElectID";
			this.labelElectID.Size = new System.Drawing.Size(168, 23);
			this.labelElectID.TabIndex = 0;
			this.labelElectID.Text = "Electronic ID";
			this.labelElectID.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textElectID
			// 
			this.textElectID.AutoSize = false;
			this.textElectID.Location = new System.Drawing.Point(210, 170);
			this.textElectID.Name = "textElectID";
			this.textElectID.Size = new System.Drawing.Size(60, 23);
			this.textElectID.TabIndex = 8;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.comboPlans);
			this.groupBox1.Controls.Add(this.textPlans);
			this.groupBox1.Controls.Add(this.label9);
			this.groupBox1.Location = new System.Drawing.Point(8, 490);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(494, 54);
			this.groupBox1.TabIndex = 17;
			this.groupBox1.Text = "In Use By";
			// 
			// comboPlans
			// 
			this.comboPlans.Location = new System.Drawing.Point(244, 23);
			this.comboPlans.Name = "comboPlans";
			this.comboPlans.Size = new System.Drawing.Size(242, 23);
			this.comboPlans.TabIndex = 1;
			// 
			// textPlans
			// 
			this.textPlans.BackColor = System.Drawing.Color.White;
			this.textPlans.Location = new System.Drawing.Point(202, 23);
			this.textPlans.Name = "textPlans";
			this.textPlans.ReadOnly = true;
			this.textPlans.Size = new System.Drawing.Size(36, 23);
			this.textPlans.TabIndex = 0;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(80, 23);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(116, 23);
			this.label9.TabIndex = 0;
			this.label9.Text = "Ins Plan Subscribers";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textCity
			// 
			this.textCity.AutoSize = false;
			this.textCity.Location = new System.Drawing.Point(210, 143);
			this.textCity.MaxLength = 255;
			this.textCity.Name = "textCity";
			this.textCity.Size = new System.Drawing.Size(156, 23);
			this.textCity.TabIndex = 5;
			this.textCity.TextChanged += new System.EventHandler(this.textCity_TextChanged);
			// 
			// textState
			// 
			this.textState.AutoSize = false;
			this.textState.Location = new System.Drawing.Point(372, 143);
			this.textState.MaxLength = 255;
			this.textState.Name = "textState";
			this.textState.Size = new System.Drawing.Size(64, 23);
			this.textState.TabIndex = 6;
			this.textState.TextChanged += new System.EventHandler(this.textState_TextChanged);
			// 
			// textZip
			// 
			this.textZip.AutoSize = false;
			this.textZip.Location = new System.Drawing.Point(442, 143);
			this.textZip.MaxLength = 255;
			this.textZip.Name = "textZip";
			this.textZip.Size = new System.Drawing.Size(60, 23);
			this.textZip.TabIndex = 7;
			// 
			// labelCitySt
			// 
			this.labelCitySt.Location = new System.Drawing.Point(60, 143);
			this.labelCitySt.Name = "labelCitySt";
			this.labelCitySt.Size = new System.Drawing.Size(144, 23);
			this.labelCitySt.TabIndex = 0;
			this.labelCitySt.Text = "City, ST, Zip";
			this.labelCitySt.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkIsCDAnet
			// 
			this.checkIsCDAnet.Location = new System.Drawing.Point(210, 592);
			this.checkIsCDAnet.Name = "checkIsCDAnet";
			this.checkIsCDAnet.Size = new System.Drawing.Size(116, 20);
			this.checkIsCDAnet.TabIndex = 18;
			this.checkIsCDAnet.Text = "Is CDAnet Carrier";
			this.checkIsCDAnet.Click += new System.EventHandler(this.checkIsCDAnet_Click);
			// 
			// groupCDAnet
			// 
			this.groupCDAnet.Controls.Add(this.label14);
			this.groupCDAnet.Controls.Add(this.label12);
			this.groupCDAnet.Controls.Add(this.textEncryptionMethod);
			this.groupCDAnet.Controls.Add(this.label13);
			this.groupCDAnet.Controls.Add(this.label11);
			this.groupCDAnet.Controls.Add(this.groupBox3);
			this.groupCDAnet.Controls.Add(this.textModemReconcile);
			this.groupCDAnet.Controls.Add(this.label10);
			this.groupCDAnet.Controls.Add(this.textModemSummary);
			this.groupCDAnet.Controls.Add(this.label8);
			this.groupCDAnet.Controls.Add(this.textModem);
			this.groupCDAnet.Controls.Add(this.label7);
			this.groupCDAnet.Controls.Add(this.comboNetwork);
			this.groupCDAnet.Controls.Add(this.label5);
			this.groupCDAnet.Controls.Add(this.textVersion);
			this.groupCDAnet.Controls.Add(this.label1);
			this.groupCDAnet.Location = new System.Drawing.Point(8, 620);
			this.groupCDAnet.Name = "groupCDAnet";
			this.groupCDAnet.Size = new System.Drawing.Size(686, 325);
			this.groupCDAnet.TabIndex = 19;
			this.groupCDAnet.Text = "CDAnet";
			// 
			// label14
			// 
			this.label14.Cursor = System.Windows.Forms.Cursors.Default;
			this.label14.Location = new System.Drawing.Point(8, 104);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(325, 34);
			this.label14.TabIndex = 0;
			this.label14.Text = "The values in this section are set and maintained by clicking \"Update Carriers\" i" +
    "n the Carriers list";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(248, 80);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(92, 17);
			this.label12.TabIndex = 0;
			this.label12.Text = "(1, 2, or 3)";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textEncryptionMethod
			// 
			this.textEncryptionMethod.Enabled = false;
			this.textEncryptionMethod.Location = new System.Drawing.Point(202, 77);
			this.textEncryptionMethod.Name = "textEncryptionMethod";
			this.textEncryptionMethod.Size = new System.Drawing.Size(40, 23);
			this.textEncryptionMethod.TabIndex = 2;
			this.textEncryptionMethod.TabStop = false;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(80, 77);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(116, 23);
			this.label13.TabIndex = 0;
			this.label13.Text = "Encryption Method";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(248, 53);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(92, 17);
			this.label11.TabIndex = 0;
			this.label11.Text = "(02 or 04)";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBox3
			// 
			this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.groupBox3.Controls.Add(this.check09);
			this.groupBox3.Controls.Add(this.check01);
			this.groupBox3.Controls.Add(this.check03m);
			this.groupBox3.Controls.Add(this.check03);
			this.groupBox3.Controls.Add(this.check07);
			this.groupBox3.Controls.Add(this.check06);
			this.groupBox3.Controls.Add(this.check04);
			this.groupBox3.Controls.Add(this.check05);
			this.groupBox3.Controls.Add(this.check02);
			this.groupBox3.Controls.Add(this.check08);
			this.groupBox3.Location = new System.Drawing.Point(346, 50);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(332, 267);
			this.groupBox3.TabIndex = 6;
			this.groupBox3.Text = "Supported Transaction Types";
			// 
			// check09
			// 
			this.check09.Enabled = false;
			this.check09.Location = new System.Drawing.Point(8, 239);
			this.check09.Name = "check09";
			this.check09.Size = new System.Drawing.Size(91, 20);
			this.check09.TabIndex = 278;
			this.check09.TabStop = false;
			this.check09.Text = "Attachments";
			// 
			// check01
			// 
			this.check01.Checked = true;
			this.check01.CheckState = System.Windows.Forms.CheckState.Checked;
			this.check01.Enabled = false;
			this.check01.Location = new System.Drawing.Point(8, 23);
			this.check01.Name = "check01";
			this.check01.Size = new System.Drawing.Size(53, 20);
			this.check01.TabIndex = 0;
			this.check01.TabStop = false;
			this.check01.Text = "Claim";
			// 
			// check03m
			// 
			this.check03m.Enabled = false;
			this.check03m.Location = new System.Drawing.Point(8, 143);
			this.check03m.Name = "check03m";
			this.check03m.Size = new System.Drawing.Size(181, 20);
			this.check03m.TabIndex = 5;
			this.check03m.TabStop = false;
			this.check03m.Text = "Predetermination Multi-page";
			// 
			// check03
			// 
			this.check03.Enabled = false;
			this.check03.Location = new System.Drawing.Point(8, 119);
			this.check03.Name = "check03";
			this.check03.Size = new System.Drawing.Size(184, 20);
			this.check03.TabIndex = 4;
			this.check03.TabStop = false;
			this.check03.Text = "Predetermination Single Page";
			// 
			// check07
			// 
			this.check07.Enabled = false;
			this.check07.Location = new System.Drawing.Point(8, 71);
			this.check07.Name = "check07";
			this.check07.Size = new System.Drawing.Size(147, 20);
			this.check07.TabIndex = 2;
			this.check07.TabStop = false;
			this.check07.Text = "COB Claim Transaction";
			// 
			// check06
			// 
			this.check06.Enabled = false;
			this.check06.Location = new System.Drawing.Point(8, 215);
			this.check06.Name = "check06";
			this.check06.Size = new System.Drawing.Size(216, 20);
			this.check06.TabIndex = 8;
			this.check06.TabStop = false;
			this.check06.Text = "Request for Payment Reconciliation";
			// 
			// check04
			// 
			this.check04.Enabled = false;
			this.check04.Location = new System.Drawing.Point(8, 167);
			this.check04.Name = "check04";
			this.check04.Size = new System.Drawing.Size(282, 20);
			this.check04.TabIndex = 6;
			this.check04.TabStop = false;
			this.check04.Text = "Request for Outstanding Transactions [Mailbox]";
			// 
			// check05
			// 
			this.check05.Enabled = false;
			this.check05.Location = new System.Drawing.Point(8, 191);
			this.check05.Name = "check05";
			this.check05.Size = new System.Drawing.Size(220, 20);
			this.check05.TabIndex = 7;
			this.check05.TabStop = false;
			this.check05.Text = "Request for Summary Reconciliation";
			// 
			// check02
			// 
			this.check02.Enabled = false;
			this.check02.Location = new System.Drawing.Point(8, 95);
			this.check02.Name = "check02";
			this.check02.Size = new System.Drawing.Size(102, 20);
			this.check02.TabIndex = 3;
			this.check02.TabStop = false;
			this.check02.Text = "Claim Reversal";
			// 
			// check08
			// 
			this.check08.Enabled = false;
			this.check08.Location = new System.Drawing.Point(8, 47);
			this.check08.Name = "check08";
			this.check08.Size = new System.Drawing.Size(138, 20);
			this.check08.TabIndex = 1;
			this.check08.TabStop = false;
			this.check08.Text = "Eligibility Transaction";
			// 
			// textModemReconcile
			// 
			this.textModemReconcile.Location = new System.Drawing.Point(202, 208);
			this.textModemReconcile.Name = "textModemReconcile";
			this.textModemReconcile.Size = new System.Drawing.Size(120, 23);
			this.textModemReconcile.TabIndex = 5;
			this.textModemReconcile.TabStop = false;
			this.textModemReconcile.Visible = false;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(11, 204);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(185, 32);
			this.label10.TabIndex = 0;
			this.label10.Text = "Modem Phone Number - Request for Payment Reconciliation";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label10.Visible = false;
			// 
			// textModemSummary
			// 
			this.textModemSummary.Location = new System.Drawing.Point(202, 172);
			this.textModemSummary.Name = "textModemSummary";
			this.textModemSummary.Size = new System.Drawing.Size(120, 23);
			this.textModemSummary.TabIndex = 4;
			this.textModemSummary.TabStop = false;
			this.textModemSummary.Visible = false;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(8, 167);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(188, 33);
			this.label8.TabIndex = 0;
			this.label8.Text = "Modem Phone Number - Request for Summary Reconciliation";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label8.Visible = false;
			// 
			// textModem
			// 
			this.textModem.Location = new System.Drawing.Point(202, 140);
			this.textModem.Name = "textModem";
			this.textModem.Size = new System.Drawing.Size(120, 23);
			this.textModem.TabIndex = 3;
			this.textModem.TabStop = false;
			this.textModem.Visible = false;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(56, 140);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(140, 23);
			this.label7.TabIndex = 0;
			this.label7.Text = "Modem Phone Number";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label7.Visible = false;
			// 
			// comboNetwork
			// 
			this.comboNetwork.Enabled = false;
			this.comboNetwork.Location = new System.Drawing.Point(202, 23);
			this.comboNetwork.Name = "comboNetwork";
			this.comboNetwork.Size = new System.Drawing.Size(260, 23);
			this.comboNetwork.TabIndex = 0;
			this.comboNetwork.TabStop = false;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(140, 23);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(56, 23);
			this.label5.TabIndex = 0;
			this.label5.Text = "Network";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textVersion
			// 
			this.textVersion.Enabled = false;
			this.textVersion.Location = new System.Drawing.Point(202, 50);
			this.textVersion.Name = "textVersion";
			this.textVersion.Size = new System.Drawing.Size(40, 23);
			this.textVersion.TabIndex = 1;
			this.textVersion.TabStop = false;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(100, 50);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Version Number";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkIsHidden
			// 
			this.checkIsHidden.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIsHidden.Location = new System.Drawing.Point(210, 332);
			this.checkIsHidden.Name = "checkIsHidden";
			this.checkIsHidden.Size = new System.Drawing.Size(62, 20);
			this.checkIsHidden.TabIndex = 13;
			this.checkIsHidden.Text = "Hidden";
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 957);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(78, 26);
			this.butDelete.TabIndex = 20;
			this.butDelete.Text = "&Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(616, 957);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(78, 26);
			this.butSave.TabIndex = 21;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// textCarrierNum
			// 
			this.textCarrierNum.BackColor = System.Drawing.SystemColors.Control;
			this.textCarrierNum.AutoSize = false;
			this.textCarrierNum.Location = new System.Drawing.Point(210, 8);
			this.textCarrierNum.Name = "textCarrierNum";
			this.textCarrierNum.ReadOnly = true;
			this.textCarrierNum.Size = new System.Drawing.Size(156, 23);
			this.textCarrierNum.TabIndex = 0;
			this.textCarrierNum.TabStop = false;
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(144, 8);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(60, 23);
			this.label21.TabIndex = 0;
			this.label21.Text = "Carrier ID";
			this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelCarrierGroupName
			// 
			this.labelCarrierGroupName.Location = new System.Drawing.Point(120, 305);
			this.labelCarrierGroupName.Name = "labelCarrierGroupName";
			this.labelCarrierGroupName.Size = new System.Drawing.Size(84, 23);
			this.labelCarrierGroupName.TabIndex = 0;
			this.labelCarrierGroupName.Text = "Carrier Group";
			this.labelCarrierGroupName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelCarrierGroupName.Visible = false;
			// 
			// labelColor
			// 
			this.labelColor.Location = new System.Drawing.Point(8, 380);
			this.labelColor.Name = "labelColor";
			this.labelColor.Size = new System.Drawing.Size(196, 23);
			this.labelColor.TabIndex = 0;
			this.labelColor.Text = "Appt Text Back Color (black=none)";
			this.labelColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboCarrierGroupName
			// 
			this.comboCarrierGroupName.Location = new System.Drawing.Point(210, 305);
			this.comboCarrierGroupName.Name = "comboCarrierGroupName";
			this.comboCarrierGroupName.Size = new System.Drawing.Size(224, 23);
			this.comboCarrierGroupName.TabIndex = 12;
			this.comboCarrierGroupName.Visible = false;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.radioBenefitSendsPat);
			this.groupBox2.Controls.Add(this.radioBenefitSendsIns);
			this.groupBox2.Location = new System.Drawing.Point(210, 407);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(292, 75);
			this.groupBox2.TabIndex = 16;
			this.groupBox2.Text = "Import Benefit Coinsurance";
			// 
			// radioBenefitSendsPat
			// 
			this.radioBenefitSendsPat.Location = new System.Drawing.Point(8, 23);
			this.radioBenefitSendsPat.Name = "radioBenefitSendsPat";
			this.radioBenefitSendsPat.Size = new System.Drawing.Size(194, 20);
			this.radioBenefitSendsPat.TabIndex = 0;
			this.radioBenefitSendsPat.TabStop = true;
			this.radioBenefitSendsPat.Text = "Carrier sends patient % (default)";
			this.radioBenefitSendsPat.UseVisualStyleBackColor = true;
			// 
			// radioBenefitSendsIns
			// 
			this.radioBenefitSendsIns.Location = new System.Drawing.Point(8, 47);
			this.radioBenefitSendsIns.Name = "radioBenefitSendsIns";
			this.radioBenefitSendsIns.Size = new System.Drawing.Size(160, 20);
			this.radioBenefitSendsIns.TabIndex = 1;
			this.radioBenefitSendsIns.TabStop = true;
			this.radioBenefitSendsIns.Text = "Carrier sends insurance %";
			this.radioBenefitSendsIns.UseVisualStyleBackColor = true;
			// 
			// labelSendElectronically
			// 
			this.labelSendElectronically.Location = new System.Drawing.Point(92, 197);
			this.labelSendElectronically.Name = "labelSendElectronically";
			this.labelSendElectronically.Size = new System.Drawing.Size(112, 23);
			this.labelSendElectronically.TabIndex = 0;
			this.labelSendElectronically.Text = "Send Electronically";
			this.labelSendElectronically.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboSendElectronically
			// 
			this.comboSendElectronically.Location = new System.Drawing.Point(210, 197);
			this.comboSendElectronically.Name = "comboSendElectronically";
			this.comboSendElectronically.Size = new System.Drawing.Size(292, 23);
			this.comboSendElectronically.TabIndex = 9;
			// 
			// checkRealTimeEligibility
			// 
			this.checkRealTimeEligibility.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkRealTimeEligibility.Location = new System.Drawing.Point(210, 356);
			this.checkRealTimeEligibility.Name = "checkRealTimeEligibility";
			this.checkRealTimeEligibility.Size = new System.Drawing.Size(197, 20);
			this.checkRealTimeEligibility.TabIndex = 14;
			this.checkRealTimeEligibility.Text = "Is trusted for real-time eligibility";
			// 
			// odColorPickerBack
			// 
			this.odColorPickerBack.BackgroundColor = System.Drawing.Color.Empty;
			this.odColorPickerBack.Location = new System.Drawing.Point(210, 381);
			this.odColorPickerBack.Name = "odColorPickerBack";
			this.odColorPickerBack.Size = new System.Drawing.Size(74, 21);
			this.odColorPickerBack.TabIndex = 15;
			// 
			// comboCobSendPaidByInsAt
			// 
			this.comboCobSendPaidByInsAt.Location = new System.Drawing.Point(210, 224);
			this.comboCobSendPaidByInsAt.Name = "comboCobSendPaidByInsAt";
			this.comboCobSendPaidByInsAt.Size = new System.Drawing.Size(128, 23);
			this.comboCobSendPaidByInsAt.TabIndex = 10;
			// 
			// labelCobSendPaidByOtherInsAt
			// 
			this.labelCobSendPaidByOtherInsAt.Location = new System.Drawing.Point(20, 224);
			this.labelCobSendPaidByOtherInsAt.Name = "labelCobSendPaidByOtherInsAt";
			this.labelCobSendPaidByOtherInsAt.Size = new System.Drawing.Size(184, 23);
			this.labelCobSendPaidByOtherInsAt.TabIndex = 272;
			this.labelCobSendPaidByOtherInsAt.Text = "Send Paid By Other Insurance At";
			this.labelCobSendPaidByOtherInsAt.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboEraAutomation
			// 
			this.comboEraAutomation.Location = new System.Drawing.Point(210, 251);
			this.comboEraAutomation.Name = "comboEraAutomation";
			this.comboEraAutomation.Size = new System.Drawing.Size(180, 23);
			this.comboEraAutomation.TabIndex = 11;
			// 
			// labelEraAutomation
			// 
			this.labelEraAutomation.Location = new System.Drawing.Point(104, 251);
			this.labelEraAutomation.Name = "labelEraAutomation";
			this.labelEraAutomation.Size = new System.Drawing.Size(100, 23);
			this.labelEraAutomation.TabIndex = 274;
			this.labelEraAutomation.Text = "ERA Automation";
			this.labelEraAutomation.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboOrthoConsolidate
			// 
			this.comboOrthoConsolidate.Location = new System.Drawing.Point(210, 278);
			this.comboOrthoConsolidate.Name = "comboOrthoConsolidate";
			this.comboOrthoConsolidate.Size = new System.Drawing.Size(180, 23);
			this.comboOrthoConsolidate.TabIndex = 277;
			// 
			// labelOrthoConsolidate
			// 
			this.labelOrthoConsolidate.Location = new System.Drawing.Point(20, 278);
			this.labelOrthoConsolidate.Name = "labelOrthoConsolidate";
			this.labelOrthoConsolidate.Size = new System.Drawing.Size(184, 23);
			this.labelOrthoConsolidate.TabIndex = 276;
			this.labelOrthoConsolidate.Text = "Consolidate Ortho Ins Payments";
			this.labelOrthoConsolidate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// labelPreAuthCodes
			//
			this.labelPreAuthCodes.Location = new System.Drawing.Point(22, 552);
			this.labelPreAuthCodes.Name = "labelPreAuthCodes";
			this.labelPreAuthCodes.Size = new System.Drawing.Size(182, 34);
			this.labelPreAuthCodes.TabIndex = 0;
			this.labelPreAuthCodes.Text = "Procedures requiring preauthorization";
			this.labelPreAuthCodes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// textPreAuthCodes
			//
			this.textPreAuthCodes.Location = new System.Drawing.Point(210, 552);
			this.textPreAuthCodes.Multiline = true;
			this.textPreAuthCodes.Name = "textPreAuthCodes";
			this.textPreAuthCodes.Size = new System.Drawing.Size(285, 36);
			this.textPreAuthCodes.TabIndex = 278;
			//
			// butPreAuthCodes
			//
			this.butPreAuthCodes.Location = new System.Drawing.Point(501, 558);
			this.butPreAuthCodes.Name = "butPreAuthCodes";
			this.butPreAuthCodes.Size = new System.Drawing.Size(24, 23);
			this.butPreAuthCodes.TabIndex = 279;
			this.butPreAuthCodes.Text = "...";
			this.butPreAuthCodes.Click += new System.EventHandler(this.butPreAuthCodes_Click);
			//
			// FormCarrierEdit
			//
			this.ClientSize = new System.Drawing.Size(702, 991);
			this.Controls.Add(this.butPreAuthCodes);
			this.Controls.Add(this.textPreAuthCodes);
			this.Controls.Add(this.labelPreAuthCodes);
			this.Controls.Add(this.comboOrthoConsolidate);
			this.Controls.Add(this.labelOrthoConsolidate);
			this.Controls.Add(this.comboEraAutomation);
			this.Controls.Add(this.labelEraAutomation);
			this.Controls.Add(this.comboCobSendPaidByInsAt);
			this.Controls.Add(this.labelCobSendPaidByOtherInsAt);
			this.Controls.Add(this.odColorPickerBack);
			this.Controls.Add(this.checkRealTimeEligibility);
			this.Controls.Add(this.labelSendElectronically);
			this.Controls.Add(this.comboSendElectronically);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.comboCarrierGroupName);
			this.Controls.Add(this.labelColor);
			this.Controls.Add(this.labelCarrierGroupName);
			this.Controls.Add(this.textCarrierNum);
			this.Controls.Add(this.label21);
			this.Controls.Add(this.checkIsHidden);
			this.Controls.Add(this.groupCDAnet);
			this.Controls.Add(this.textCity);
			this.Controls.Add(this.textState);
			this.Controls.Add(this.textZip);
			this.Controls.Add(this.textElectID);
			this.Controls.Add(this.textAddress2);
			this.Controls.Add(this.textAddress);
			this.Controls.Add(this.textPhone);
			this.Controls.Add(this.textCarrierName);
			this.Controls.Add(this.labelCitySt);
			this.Controls.Add(this.checkIsCDAnet);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.labelElectID);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.butSave);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormCarrierEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Carrier";
			this.Load += new System.EventHandler(this.FormCarrierEdit_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupCDAnet.ResumeLayout(false);
			this.groupCDAnet.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label labelElectID;
		private System.Windows.Forms.TextBox textCarrierName;
		private ValidPhone textPhone;
		private System.Windows.Forms.TextBox textAddress;
		private System.Windows.Forms.TextBox textAddress2;
		private System.Windows.Forms.TextBox textElectID;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private OpenPmsDental.UI.ComboBox comboPlans;
		private System.Windows.Forms.TextBox textPlans;
		private System.Windows.Forms.Label label9;
		private OpenPmsDental.UI.Button butDelete;
		private System.Windows.Forms.TextBox textCity;
		private System.Windows.Forms.TextBox textState;
		private System.Windows.Forms.TextBox textZip;
		private System.Windows.Forms.Label labelCitySt;
		private OpenPmsDental.UI.CheckBox checkIsCDAnet;
		private OpenPmsDental.UI.GroupBox groupCDAnet;
		private TextBox textModemReconcile;
		private Label label10;
		private TextBox textModemSummary;
		private Label label8;
		private TextBox textModem;
		private Label label7;
		private OpenPmsDental.UI.ComboBox comboNetwork;
		private Label label5;
		private TextBox textVersion;
		private Label label1;
		private OpenPmsDental.UI.GroupBox groupBox3;
		private OpenPmsDental.UI.CheckBox check08;
		private OpenPmsDental.UI.CheckBox check03;
		private OpenPmsDental.UI.CheckBox check07;
		private OpenPmsDental.UI.CheckBox check06;
		private OpenPmsDental.UI.CheckBox check04;
		private OpenPmsDental.UI.CheckBox check05;
		private OpenPmsDental.UI.CheckBox check02;
		private OpenPmsDental.UI.CheckBox checkIsHidden;
		private Label label11;
		private OpenPmsDental.UI.CheckBox check03m;
		private Label label12;
		private TextBox textEncryptionMethod;
		private Label label13;
		private OpenPmsDental.UI.CheckBox check01;
		private Label label14;
		private TextBox textCarrierNum;
		private Label label21;
		private Label labelCarrierGroupName;
		private Label labelColor;
		private UI.ComboBox comboCarrierGroupName;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private RadioButton radioBenefitSendsIns;
		private RadioButton radioBenefitSendsPat;
		private Label labelSendElectronically;
		private UI.ComboBox comboSendElectronically;
		private UI.ODColorPicker odColorPickerBack;
		private UI.ComboBox comboCobSendPaidByInsAt;
		private Label labelCobSendPaidByOtherInsAt;
		private OpenPmsDental.UI.CheckBox checkRealTimeEligibility;
		private UI.ComboBox comboEraAutomation;
		private Label labelEraAutomation;
		private UI.ComboBox comboOrthoConsolidate;
		private Label labelOrthoConsolidate;
		private UI.CheckBox check09;
		private Label labelPreAuthCodes;
		private TextBox textPreAuthCodes;
		private OpenPmsDental.UI.Button butPreAuthCodes;
	}
}
