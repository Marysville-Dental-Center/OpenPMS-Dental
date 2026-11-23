using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormEmailAddressEdit {
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
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.butSave = new OpenPmsDental.UI.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textSMTPserver = new System.Windows.Forms.TextBox();
			this.textSender = new System.Windows.Forms.TextBox();
			this.textUsername = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textPassword = new System.Windows.Forms.TextBox();
			this.labelPassword = new System.Windows.Forms.Label();
			this.textPort = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.checkSSL = new OpenPmsDental.UI.CheckBox();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.groupOutgoing = new OpenPmsDental.UI.GroupBox();
			this.label13 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.groupIncoming = new OpenPmsDental.UI.GroupBox();
			this.label14 = new System.Windows.Forms.Label();
			this.textSMTPserverIncoming = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.textPortIncoming = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.butRegisterCertificate = new OpenPmsDental.UI.Button();
			this.butPickUserod = new OpenPmsDental.UI.Button();
			this.labelUserod = new System.Windows.Forms.Label();
			this.textUserod = new System.Windows.Forms.TextBox();
			this.labelGmailStatusCaption = new System.Windows.Forms.Label();
			this.labelGmailStatus = new System.Windows.Forms.Label();
			this.groupGmail = new OpenPmsDental.UI.GroupBox();
			this.labelDownloadGmail = new System.Windows.Forms.Label();
			this.butAuthGoogle = new OpenPmsDental.UI.Button();
			this.checkUnreadGmail = new OpenPmsDental.UI.CheckBox();
			this.textParams = new System.Windows.Forms.TextBox();
			this.labelParams = new System.Windows.Forms.Label();
			this.checkDownloadGmail = new OpenPmsDental.UI.CheckBox();
			this.butClearTokensGmail = new OpenPmsDental.UI.Button();
			this.panelUserod = new System.Windows.Forms.Panel();
			this.radioPassword = new System.Windows.Forms.RadioButton();
			this.radioGmail = new System.Windows.Forms.RadioButton();
			this.radioMicrosoft = new System.Windows.Forms.RadioButton();
			this.groupAuthentication = new OpenPmsDental.UI.GroupBox();
			this.panelPassword = new System.Windows.Forms.Panel();
			this.groupMicrosoft = new OpenPmsDental.UI.GroupBox();
			this.butAuthMicrosoft = new OpenPmsDental.UI.Button();
			this.labelDownloadMicrosoft = new System.Windows.Forms.Label();
			this.checkDownloadMicrosoft = new OpenPmsDental.UI.CheckBox();
			this.labelMicrosoftStatus = new System.Windows.Forms.Label();
			this.butClearTokensMicrosoft = new OpenPmsDental.UI.Button();
			this.labelMicrosoftStatusCaption = new System.Windows.Forms.Label();
			this.groupOutgoing.SuspendLayout();
			this.groupIncoming.SuspendLayout();
			this.groupGmail.SuspendLayout();
			this.panelUserod.SuspendLayout();
			this.groupAuthentication.SuspendLayout();
			this.panelPassword.SuspendLayout();
			this.groupMicrosoft.SuspendLayout();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(1071, 597);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 6;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(182, 23);
			this.label1.TabIndex = 2;
			this.label1.Text = "Outgoing SMTP Server";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 145);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(182, 23);
			this.label2.TabIndex = 3;
			this.label2.Text = "E-mail address of sender";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textSMTPserver
			// 
			this.textSMTPserver.Location = new System.Drawing.Point(196, 23);
			this.textSMTPserver.Name = "textSMTPserver";
			this.textSMTPserver.Size = new System.Drawing.Size(187, 23);
			this.textSMTPserver.TabIndex = 1;
			// 
			// textSender
			// 
			this.textSender.Location = new System.Drawing.Point(196, 145);
			this.textSender.Name = "textSender";
			this.textSender.Size = new System.Drawing.Size(187, 23);
			this.textSender.TabIndex = 3;
			// 
			// textUsername
			// 
			this.textUsername.Location = new System.Drawing.Point(204, 8);
			this.textUsername.Name = "textUsername";
			this.textUsername.Size = new System.Drawing.Size(188, 23);
			this.textUsername.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(190, 23);
			this.label3.TabIndex = 0;
			this.label3.Text = "Username or email address";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPassword
			// 
			this.textPassword.Location = new System.Drawing.Point(70, 0);
			this.textPassword.Name = "textPassword";
			this.textPassword.PasswordChar = '*';
			this.textPassword.Size = new System.Drawing.Size(188, 23);
			this.textPassword.TabIndex = 2;
			// 
			// labelPassword
			// 
			this.labelPassword.Location = new System.Drawing.Point(0, 0);
			this.labelPassword.Name = "labelPassword";
			this.labelPassword.Size = new System.Drawing.Size(64, 23);
			this.labelPassword.TabIndex = 0;
			this.labelPassword.Text = "Password";
			this.labelPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPort
			// 
			this.textPort.Location = new System.Drawing.Point(196, 118);
			this.textPort.Name = "textPort";
			this.textPort.Size = new System.Drawing.Size(56, 23);
			this.textPort.TabIndex = 2;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 118);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(182, 23);
			this.label5.TabIndex = 22;
			this.label5.Text = "Outgoing Port";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(258, 118);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(192, 23);
			this.label6.TabIndex = 0;
			this.label6.Text = "Usually 587.  Sometimes 25 or 465.";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkSSL
			// 
			this.checkSSL.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkSSL.Location = new System.Drawing.Point(204, 35);
			this.checkSSL.Name = "checkSSL";
			this.checkSSL.Size = new System.Drawing.Size(65, 20);
			this.checkSSL.TabIndex = 3;
			this.checkSSL.Text = "Use SSL";
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 597);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 24);
			this.butDelete.TabIndex = 8;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// groupOutgoing
			// 
			this.groupOutgoing.ColorBackLabel = System.Drawing.Color.Empty;
			this.groupOutgoing.Controls.Add(this.label13);
			this.groupOutgoing.Controls.Add(this.label9);
			this.groupOutgoing.Controls.Add(this.textSMTPserver);
			this.groupOutgoing.Controls.Add(this.label1);
			this.groupOutgoing.Controls.Add(this.label2);
			this.groupOutgoing.Controls.Add(this.label6);
			this.groupOutgoing.Controls.Add(this.textSender);
			this.groupOutgoing.Controls.Add(this.textPort);
			this.groupOutgoing.Controls.Add(this.label5);
			this.groupOutgoing.Location = new System.Drawing.Point(581, 380);
			this.groupOutgoing.Name = "groupOutgoing";
			this.groupOutgoing.Size = new System.Drawing.Size(565, 176);
			this.groupOutgoing.TabIndex = 4;
			this.groupOutgoing.Text = "Outgoing Email Settings";
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(196, 48);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(187, 66);
			this.label13.TabIndex = 0;
			this.label13.Text = "smtp.comcast.net\r\nmailhost.mycompany.com \r\nmail.mycompany.com\r\nor similar...";
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(389, 145);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(168, 23);
			this.label9.TabIndex = 0;
			this.label9.Text = "(not used in encrypted email)";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupIncoming
			// 
			this.groupIncoming.ColorBackLabel = System.Drawing.Color.Empty;
			this.groupIncoming.Controls.Add(this.label14);
			this.groupIncoming.Controls.Add(this.textSMTPserverIncoming);
			this.groupIncoming.Controls.Add(this.label8);
			this.groupIncoming.Controls.Add(this.label10);
			this.groupIncoming.Controls.Add(this.textPortIncoming);
			this.groupIncoming.Controls.Add(this.label11);
			this.groupIncoming.Location = new System.Drawing.Point(8, 380);
			this.groupIncoming.Name = "groupIncoming";
			this.groupIncoming.Size = new System.Drawing.Size(565, 176);
			this.groupIncoming.TabIndex = 5;
			this.groupIncoming.Text = "Incoming Email Settings";
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(196, 48);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(187, 34);
			this.label14.TabIndex = 0;
			this.label14.Text = "pop.secureserver.net\r\nor similar...";
			// 
			// textSMTPserverIncoming
			// 
			this.textSMTPserverIncoming.Location = new System.Drawing.Point(196, 23);
			this.textSMTPserverIncoming.Name = "textSMTPserverIncoming";
			this.textSMTPserverIncoming.Size = new System.Drawing.Size(187, 23);
			this.textSMTPserverIncoming.TabIndex = 1;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(8, 23);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(182, 23);
			this.label8.TabIndex = 0;
			this.label8.Text = "Incoming POP3 Server";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(258, 86);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(164, 23);
			this.label10.TabIndex = 0;
			this.label10.Text = "Usually 110.  Sometimes 995.";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textPortIncoming
			// 
			this.textPortIncoming.Location = new System.Drawing.Point(196, 86);
			this.textPortIncoming.Name = "textPortIncoming";
			this.textPortIncoming.Size = new System.Drawing.Size(56, 23);
			this.textPortIncoming.TabIndex = 2;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(8, 86);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(182, 23);
			this.label11.TabIndex = 0;
			this.label11.Text = "Incoming Port";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butRegisterCertificate
			// 
			this.butRegisterCertificate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butRegisterCertificate.Location = new System.Drawing.Point(89, 597);
			this.butRegisterCertificate.Name = "butRegisterCertificate";
			this.butRegisterCertificate.Size = new System.Drawing.Size(122, 24);
			this.butRegisterCertificate.TabIndex = 9;
			this.butRegisterCertificate.Text = "Certificate";
			this.butRegisterCertificate.Click += new System.EventHandler(this.butRegisterCertificate_Click);
			// 
			// butPickUserod
			// 
			this.butPickUserod.Location = new System.Drawing.Point(420, 0);
			this.butPickUserod.Name = "butPickUserod";
			this.butPickUserod.Size = new System.Drawing.Size(24, 23);
			this.butPickUserod.TabIndex = 4;
			this.butPickUserod.Text = "...";
			this.butPickUserod.UseVisualStyleBackColor = true;
			this.butPickUserod.Visible = false;
			this.butPickUserod.Click += new System.EventHandler(this.butPickUserod_Click);
			// 
			// labelUserod
			// 
			this.labelUserod.Location = new System.Drawing.Point(154, 0);
			this.labelUserod.Name = "labelUserod";
			this.labelUserod.Size = new System.Drawing.Size(36, 23);
			this.labelUserod.TabIndex = 3;
			this.labelUserod.Text = "User";
			this.labelUserod.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textUserod
			// 
			this.textUserod.Location = new System.Drawing.Point(196, 0);
			this.textUserod.Name = "textUserod";
			this.textUserod.ReadOnly = true;
			this.textUserod.Size = new System.Drawing.Size(218, 23);
			this.textUserod.TabIndex = 0;
			//
			// labelGmailStatusCaption
			//
			this.labelGmailStatusCaption.Location = new System.Drawing.Point(8, 51);
			this.labelGmailStatusCaption.Name = "labelGmailStatusCaption";
			this.labelGmailStatusCaption.Size = new System.Drawing.Size(148, 20);
			this.labelGmailStatusCaption.TabIndex = 14;
			this.labelGmailStatusCaption.Text = "Status";
			this.labelGmailStatusCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// labelGmailStatus
			//
			this.labelGmailStatus.Location = new System.Drawing.Point(162, 53);
			this.labelGmailStatus.Name = "labelGmailStatus";
			this.labelGmailStatus.Size = new System.Drawing.Size(281, 52);
			this.labelGmailStatus.TabIndex = 15;
			this.labelGmailStatus.Text = "Not authorized.";
			//
			// groupGmail
			//
			this.groupGmail.ColorBackLabel = System.Drawing.Color.Empty;
			this.groupGmail.Controls.Add(this.labelDownloadGmail);
			this.groupGmail.Controls.Add(this.butAuthGoogle);
			this.groupGmail.Controls.Add(this.checkUnreadGmail);
			this.groupGmail.Controls.Add(this.textParams);
			this.groupGmail.Controls.Add(this.labelParams);
			this.groupGmail.Controls.Add(this.checkDownloadGmail);
			this.groupGmail.Controls.Add(this.butClearTokensGmail);
			this.groupGmail.Controls.Add(this.labelGmailStatusCaption);
			this.groupGmail.Controls.Add(this.labelGmailStatus);
			this.groupGmail.Location = new System.Drawing.Point(556, 23);
			this.groupGmail.Name = "groupGmail";
			this.groupGmail.Size = new System.Drawing.Size(574, 278);
			this.groupGmail.TabIndex = 16;
			this.groupGmail.Text = "Gmail";
			// 
			// labelDownloadGmail
			// 
			this.labelDownloadGmail.Location = new System.Drawing.Point(338, 135);
			this.labelDownloadGmail.Name = "labelDownloadGmail";
			this.labelDownloadGmail.Size = new System.Drawing.Size(172, 20);
			this.labelDownloadGmail.TabIndex = 32;
			this.labelDownloadGmail.Text = "Uncheck if sending email only";
			this.labelDownloadGmail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// butAuthGoogle
			// 
			this.butAuthGoogle.Location = new System.Drawing.Point(162, 23);
			this.butAuthGoogle.Name = "butAuthGoogle";
			this.butAuthGoogle.Size = new System.Drawing.Size(115, 24);
			this.butAuthGoogle.TabIndex = 24;
			this.butAuthGoogle.Text = "Authorize...";
			this.butAuthGoogle.Click += new System.EventHandler(this.butAuthGoogle_Click);
			// 
			// checkUnreadGmail
			// 
			this.checkUnreadGmail.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkUnreadGmail.Location = new System.Drawing.Point(162, 250);
			this.checkUnreadGmail.Name = "checkUnreadGmail";
			this.checkUnreadGmail.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkUnreadGmail.Size = new System.Drawing.Size(136, 20);
			this.checkUnreadGmail.TabIndex = 29;
			this.checkUnreadGmail.Text = "Get unread mail only";
			this.checkUnreadGmail.Click += new System.EventHandler(this.checkUnreadGmail_CheckedChanged);
			// 
			// textParams
			// 
			this.textParams.Location = new System.Drawing.Point(162, 159);
			this.textParams.Multiline = true;
			this.textParams.Name = "textParams";
			this.textParams.Size = new System.Drawing.Size(264, 87);
			this.textParams.TabIndex = 26;
			//
			// labelParams
			//
			this.labelParams.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelParams.Location = new System.Drawing.Point(8, 159);
			this.labelParams.Name = "labelParams";
			this.labelParams.Size = new System.Drawing.Size(148, 84);
			this.labelParams.TabIndex = 28;
			this.labelParams.Text = "Inbox Search Operators for Filtering\r\n(blank: in:inbox newer_than:30d)";
			this.labelParams.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// checkDownloadGmail
			// 
			this.checkDownloadGmail.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkDownloadGmail.Location = new System.Drawing.Point(162, 135);
			this.checkDownloadGmail.Name = "checkDownloadGmail";
			this.checkDownloadGmail.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkDownloadGmail.Size = new System.Drawing.Size(170, 20);
			this.checkDownloadGmail.TabIndex = 27;
			this.checkDownloadGmail.Text = "Download incoming emails";
			this.checkDownloadGmail.Click += new System.EventHandler(this.checkDownloadGmail_CheckedChanged);
			// 
			// butClearTokensGmail
			// 
			this.butClearTokensGmail.Location = new System.Drawing.Point(162, 107);
			this.butClearTokensGmail.Name = "butClearTokensGmail";
			this.butClearTokensGmail.Size = new System.Drawing.Size(115, 24);
			this.butClearTokensGmail.TabIndex = 16;
			this.butClearTokensGmail.Text = "Sign Out";
			this.butClearTokensGmail.UseVisualStyleBackColor = true;
			this.butClearTokensGmail.Click += new System.EventHandler(this.butClearTokensGmail_Click);
			// 
			// panelUserod
			// 
			this.panelUserod.Controls.Add(this.textUserod);
			this.panelUserod.Controls.Add(this.butPickUserod);
			this.panelUserod.Controls.Add(this.labelUserod);
			this.panelUserod.Location = new System.Drawing.Point(8, 564);
			this.panelUserod.Name = "panelUserod";
			this.panelUserod.Size = new System.Drawing.Size(444, 23);
			this.panelUserod.TabIndex = 20;
			// 
			// radioPassword
			// 
			this.radioPassword.Checked = true;
			this.radioPassword.Location = new System.Drawing.Point(8, 23);
			this.radioPassword.Name = "radioPassword";
			this.radioPassword.Size = new System.Drawing.Size(75, 20);
			this.radioPassword.TabIndex = 21;
			this.radioPassword.TabStop = true;
			this.radioPassword.Text = "Password";
			this.radioPassword.UseVisualStyleBackColor = true;
			this.radioPassword.Click += new System.EventHandler(this.radioPassword_Click);
			// 
			// radioGmail
			// 
			this.radioGmail.Location = new System.Drawing.Point(8, 47);
			this.radioGmail.Name = "radioGmail";
			this.radioGmail.Size = new System.Drawing.Size(102, 20);
			this.radioGmail.TabIndex = 22;
			this.radioGmail.Text = "Gmail (OAuth)";
			this.radioGmail.UseVisualStyleBackColor = true;
			this.radioGmail.Click += new System.EventHandler(this.radioGmail_Click);
			// 
			// radioMicrosoft
			// 
			this.radioMicrosoft.Location = new System.Drawing.Point(8, 71);
			this.radioMicrosoft.Name = "radioMicrosoft";
			this.radioMicrosoft.Size = new System.Drawing.Size(97, 20);
			this.radioMicrosoft.TabIndex = 23;
			this.radioMicrosoft.Text = "Microsoft 365";
			this.radioMicrosoft.UseVisualStyleBackColor = true;
			this.radioMicrosoft.Click += new System.EventHandler(this.radioMicrosoft_Click);
			// 
			// groupAuthentication
			// 
			this.groupAuthentication.ColorBackLabel = System.Drawing.Color.Empty;
			this.groupAuthentication.Controls.Add(this.panelPassword);
			this.groupAuthentication.Controls.Add(this.groupMicrosoft);
			this.groupAuthentication.Controls.Add(this.radioGmail);
			this.groupAuthentication.Controls.Add(this.radioMicrosoft);
			this.groupAuthentication.Controls.Add(this.radioPassword);
			this.groupAuthentication.Controls.Add(this.groupGmail);
			this.groupAuthentication.Location = new System.Drawing.Point(8, 63);
			this.groupAuthentication.Name = "groupAuthentication";
			this.groupAuthentication.Size = new System.Drawing.Size(1138, 309);
			this.groupAuthentication.TabIndex = 23;
			this.groupAuthentication.Text = "Email Authentication";
			// 
			// panelPassword
			// 
			this.panelPassword.Controls.Add(this.textPassword);
			this.panelPassword.Controls.Add(this.labelPassword);
			this.panelPassword.Location = new System.Drawing.Point(126, 22);
			this.panelPassword.Name = "panelPassword";
			this.panelPassword.Size = new System.Drawing.Size(258, 23);
			this.panelPassword.TabIndex = 21;
			// 
			// groupMicrosoft
			// 
			this.groupMicrosoft.ColorBackLabel = System.Drawing.Color.Empty;
			this.groupMicrosoft.Controls.Add(this.butAuthMicrosoft);
			this.groupMicrosoft.Controls.Add(this.labelDownloadMicrosoft);
			this.groupMicrosoft.Controls.Add(this.checkDownloadMicrosoft);
			this.groupMicrosoft.Controls.Add(this.labelMicrosoftStatus);
			this.groupMicrosoft.Controls.Add(this.butClearTokensMicrosoft);
			this.groupMicrosoft.Controls.Add(this.labelMicrosoftStatusCaption);
			this.groupMicrosoft.Location = new System.Drawing.Point(126, 53);
			this.groupMicrosoft.Name = "groupMicrosoft";
			this.groupMicrosoft.Size = new System.Drawing.Size(414, 163);
			this.groupMicrosoft.TabIndex = 30;
			this.groupMicrosoft.Text = "Microsoft 365";
			//
			// butAuthMicrosoft
			//
			this.butAuthMicrosoft.Location = new System.Drawing.Point(58, 23);
			this.butAuthMicrosoft.Name = "butAuthMicrosoft";
			this.butAuthMicrosoft.Size = new System.Drawing.Size(115, 24);
			this.butAuthMicrosoft.TabIndex = 25;
			this.butAuthMicrosoft.Text = "Sign in...";
			this.butAuthMicrosoft.Click += new System.EventHandler(this.butAuthMicrosoft_Click);
			//
			// labelDownloadMicrosoft
			// 
			this.labelDownloadMicrosoft.Location = new System.Drawing.Point(234, 135);
			this.labelDownloadMicrosoft.Name = "labelDownloadMicrosoft";
			this.labelDownloadMicrosoft.Size = new System.Drawing.Size(172, 20);
			this.labelDownloadMicrosoft.TabIndex = 31;
			this.labelDownloadMicrosoft.Text = "Uncheck if sending email only";
			this.labelDownloadMicrosoft.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkDownloadMicrosoft
			// 
			this.checkDownloadMicrosoft.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkDownloadMicrosoft.Location = new System.Drawing.Point(58, 135);
			this.checkDownloadMicrosoft.Name = "checkDownloadMicrosoft";
			this.checkDownloadMicrosoft.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkDownloadMicrosoft.Size = new System.Drawing.Size(170, 20);
			this.checkDownloadMicrosoft.TabIndex = 30;
			this.checkDownloadMicrosoft.Text = "Download incoming emails";
			this.checkDownloadMicrosoft.CheckedChanged += new System.EventHandler(this.checkDownloadMicrosoft_CheckedChanged);
			//
			// labelMicrosoftStatus
			//
			this.labelMicrosoftStatus.Location = new System.Drawing.Point(58, 53);
			this.labelMicrosoftStatus.Name = "labelMicrosoftStatus";
			this.labelMicrosoftStatus.Size = new System.Drawing.Size(200, 52);
			this.labelMicrosoftStatus.TabIndex = 11;
			this.labelMicrosoftStatus.Text = "Not signed in.";
			//
			// butClearTokensMicrosoft
			// 
			this.butClearTokensMicrosoft.Location = new System.Drawing.Point(58, 107);
			this.butClearTokensMicrosoft.Name = "butClearTokensMicrosoft";
			this.butClearTokensMicrosoft.Size = new System.Drawing.Size(115, 24);
			this.butClearTokensMicrosoft.TabIndex = 16;
			this.butClearTokensMicrosoft.Text = "Sign Out";
			this.butClearTokensMicrosoft.UseVisualStyleBackColor = true;
			this.butClearTokensMicrosoft.Click += new System.EventHandler(this.butClearTokensMicrosoft_Click);
			// 
			// labelMicrosoftStatusCaption
			//
			this.labelMicrosoftStatusCaption.Location = new System.Drawing.Point(8, 51);
			this.labelMicrosoftStatusCaption.Name = "labelMicrosoftStatusCaption";
			this.labelMicrosoftStatusCaption.Size = new System.Drawing.Size(44, 20);
			this.labelMicrosoftStatusCaption.TabIndex = 14;
			this.labelMicrosoftStatusCaption.Text = "Status";
			this.labelMicrosoftStatusCaption.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FormEmailAddressEdit
			// 
			this.AcceptButton = this.butSave;
			this.ClientSize = new System.Drawing.Size(1154, 629);
			this.Controls.Add(this.groupAuthentication);
			this.Controls.Add(this.panelUserod);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textUsername);
			this.Controls.Add(this.butRegisterCertificate);
			this.Controls.Add(this.groupIncoming);
			this.Controls.Add(this.groupOutgoing);
			this.Controls.Add(this.checkSSL);
			this.Controls.Add(this.butDelete);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormEmailAddressEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Email Address";
			this.Load += new System.EventHandler(this.FormEmailAddress_Load);
			this.groupOutgoing.ResumeLayout(false);
			this.groupOutgoing.PerformLayout();
			this.groupIncoming.ResumeLayout(false);
			this.groupIncoming.PerformLayout();
			this.groupGmail.ResumeLayout(false);
			this.groupGmail.PerformLayout();
			this.panelUserod.ResumeLayout(false);
			this.panelUserod.PerformLayout();
			this.groupAuthentication.ResumeLayout(false);
			this.panelPassword.ResumeLayout(false);
			this.panelPassword.PerformLayout();
			this.groupMicrosoft.ResumeLayout(false);
			this.groupMicrosoft.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textSMTPserver;
		private System.Windows.Forms.TextBox textSender;
		private TextBox textUsername;
		private Label label3;
		private TextBox textPassword;
		private Label labelPassword;
		private TextBox textPort;
		private Label label5;
		private Label label6;
		private OpenPmsDental.UI.CheckBox checkSSL;
		private UI.Button butDelete;
		private OpenPmsDental.UI.GroupBox groupOutgoing;
		private OpenPmsDental.UI.GroupBox groupIncoming;
		private TextBox textSMTPserverIncoming;
		private Label label8;
		private Label label10;
		private TextBox textPortIncoming;
		private Label label11;
		private Label label9;
		private UI.Button butRegisterCertificate;
		private Label label13;
		private Label label14;
		private UI.Button butPickUserod;
		private Label labelUserod;
		private TextBox textUserod;
		private Label labelGmailStatusCaption;
		private Label labelGmailStatus;
		private OpenPmsDental.UI.GroupBox groupGmail;
		private UI.Button butClearTokensGmail;
		private Panel panelUserod;
		private RadioButton radioPassword;
		private RadioButton radioGmail;
		private RadioButton radioMicrosoft;
		private UI.GroupBox groupAuthentication;
		private UI.CheckBox checkUnreadGmail;
		private TextBox textParams;
		private Label labelParams;
		private UI.CheckBox checkDownloadGmail;
		private UI.GroupBox groupMicrosoft;
		private UI.CheckBox checkDownloadMicrosoft;
		private Label labelMicrosoftStatus;
		private UI.Button butClearTokensMicrosoft;
		private Label labelMicrosoftStatusCaption;
		private Label labelDownloadGmail;
		private Panel panelPassword;
		private Label labelDownloadMicrosoft;
		private UI.Button butAuthGoogle;
		private UI.Button butAuthMicrosoft;
	}
}
