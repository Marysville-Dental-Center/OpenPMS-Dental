using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormRecallSetup {
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
            this.gridMain = new OpenPmsDental.UI.GridOD();
            this.radioExcludeFutureYes = new System.Windows.Forms.RadioButton();
            this.label8 = new System.Windows.Forms.Label();
            this.radioExcludeFutureNo = new System.Windows.Forms.RadioButton();
            this.textPostcardsPerSheet = new System.Windows.Forms.TextBox();
            this.groupBox4 = new OpenPmsDental.UI.GroupBox();
            this.radioUseEmailFalse = new System.Windows.Forms.RadioButton();
            this.radioUseEmailTrue = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new OpenPmsDental.UI.GroupBox();
            this.textDown = new OpenPmsDental.ValidDouble();
            this.label12 = new System.Windows.Forms.Label();
            this.textRight = new OpenPmsDental.ValidDouble();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox3 = new OpenPmsDental.UI.GroupBox();
            this.textDaysFuture = new OpenPmsDental.ValidNum();
            this.textDaysPast = new OpenPmsDental.ValidNum();
            this.checkGroupFamilies = new OpenPmsDental.UI.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.comboStatusMailedRecall = new OpenPmsDental.UI.ComboBox();
            this.label26 = new System.Windows.Forms.Label();
            this.groupBox1 = new OpenPmsDental.UI.GroupBox();
            this.textDaysSecondReminder = new OpenPmsDental.ValidNum();
            this.textDaysFirstReminder = new OpenPmsDental.ValidNum();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textMaxReminders = new OpenPmsDental.ValidNum();
            this.label4 = new System.Windows.Forms.Label();
            this.comboStatusEmailedRecall = new OpenPmsDental.UI.ComboBox();
            this.listTypes = new OpenPmsDental.UI.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkReturnAdd = new OpenPmsDental.UI.CheckBox();
            this.butSave = new OpenPmsDental.UI.Button();
            this.comboStatusEmailTextRecall = new OpenPmsDental.UI.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboStatusTextedRecall = new OpenPmsDental.UI.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textMaxAutoReminders = new OpenPmsDental.ValidNum();
            this.label7 = new System.Windows.Forms.Label();
            this.comboLanguage = new OpenPmsDental.UI.ComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridMain
            // 
            this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridMain.Location = new System.Drawing.Point(8, 35);
            this.gridMain.Name = "gridMain";
            this.gridMain.Size = new System.Drawing.Size(909, 437);
            this.gridMain.TabIndex = 67;
            this.gridMain.Title = "Messages";
            this.gridMain.TranslationName = "TableRecallMsgs";
            this.gridMain.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
            // 
            // radioExcludeFutureYes
            // 
            this.radioExcludeFutureYes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.radioExcludeFutureYes.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.radioExcludeFutureYes.Location = new System.Drawing.Point(422, 613);
            this.radioExcludeFutureYes.Name = "radioExcludeFutureYes";
            this.radioExcludeFutureYes.Size = new System.Drawing.Size(206, 20);
            this.radioExcludeFutureYes.TabIndex = 72;
            this.radioExcludeFutureYes.Text = "Exclude from list if any future appt";
            this.radioExcludeFutureYes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.radioExcludeFutureYes.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label8.Location = new System.Drawing.Point(40, 584);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(164, 23);
            this.label8.TabIndex = 19;
            this.label8.Text = "Postcards per sheet (1,3,or 4)";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // radioExcludeFutureNo
            // 
            this.radioExcludeFutureNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.radioExcludeFutureNo.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.radioExcludeFutureNo.Location = new System.Drawing.Point(422, 589);
            this.radioExcludeFutureNo.Name = "radioExcludeFutureNo";
            this.radioExcludeFutureNo.Size = new System.Drawing.Size(210, 20);
            this.radioExcludeFutureNo.TabIndex = 71;
            this.radioExcludeFutureNo.Text = "Exclude from list if recall scheduled";
            this.radioExcludeFutureNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.radioExcludeFutureNo.UseVisualStyleBackColor = true;
            // 
            // textPostcardsPerSheet
            // 
            this.textPostcardsPerSheet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textPostcardsPerSheet.AutoSize = false;
            this.textPostcardsPerSheet.Location = new System.Drawing.Point(210, 584);
            this.textPostcardsPerSheet.Name = "textPostcardsPerSheet";
            this.textPostcardsPerSheet.Size = new System.Drawing.Size(32, 23);
            this.textPostcardsPerSheet.TabIndex = 18;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.radioUseEmailFalse);
            this.groupBox4.Controls.Add(this.radioUseEmailTrue);
            this.groupBox4.Location = new System.Drawing.Point(691, 565);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(226, 75);
            this.groupBox4.TabIndex = 70;
            this.groupBox4.Text = "Use email if";
            // 
            // radioUseEmailFalse
            // 
            this.radioUseEmailFalse.Location = new System.Drawing.Point(8, 47);
            this.radioUseEmailFalse.Name = "radioUseEmailFalse";
            this.radioUseEmailFalse.Size = new System.Drawing.Size(192, 20);
            this.radioUseEmailFalse.TabIndex = 1;
            this.radioUseEmailFalse.Text = "Email is preferred recall method";
            this.radioUseEmailFalse.UseVisualStyleBackColor = true;
            // 
            // radioUseEmailTrue
            // 
            this.radioUseEmailTrue.Location = new System.Drawing.Point(8, 23);
            this.radioUseEmailTrue.Name = "radioUseEmailTrue";
            this.radioUseEmailTrue.Size = new System.Drawing.Size(120, 20);
            this.radioUseEmailTrue.TabIndex = 0;
            this.radioUseEmailTrue.Text = "Has email address";
            this.radioUseEmailTrue.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.textDown);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.textRight);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Location = new System.Drawing.Point(691, 476);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(226, 81);
            this.groupBox2.TabIndex = 48;
            this.groupBox2.Text = "Adjust Postcard Position in Inches";
            // 
            // textDown
            // 
            this.textDown.AutoSize = false;
            this.textDown.Location = new System.Drawing.Point(146, 50);
            this.textDown.MaxVal = 100000000D;
            this.textDown.MinVal = -100000000D;
            this.textDown.Name = "textDown";
            this.textDown.Size = new System.Drawing.Size(72, 23);
            this.textDown.TabIndex = 6;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(96, 50);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(44, 23);
            this.label12.TabIndex = 5;
            this.label12.Text = "Down";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRight
            // 
            this.textRight.AutoSize = false;
            this.textRight.Location = new System.Drawing.Point(146, 23);
            this.textRight.MaxVal = 100000000D;
            this.textRight.MinVal = -100000000D;
            this.textRight.Name = "textRight";
            this.textRight.Size = new System.Drawing.Size(72, 23);
            this.textRight.TabIndex = 4;
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(100, 23);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(40, 23);
            this.label13.TabIndex = 4;
            this.label13.Text = "Right";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.textDaysFuture);
            this.groupBox3.Controls.Add(this.textDaysPast);
            this.groupBox3.Controls.Add(this.checkGroupFamilies);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Location = new System.Drawing.Point(422, 476);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(261, 105);
            this.groupBox3.TabIndex = 54;
            this.groupBox3.Text = "Recall List Default View";
            // 
            // textDaysFuture
            // 
            this.textDaysFuture.AutoSize = false;
            this.textDaysFuture.Location = new System.Drawing.Point(192, 74);
            this.textDaysFuture.MaxVal = 10000;
            this.textDaysFuture.Name = "textDaysFuture";
            this.textDaysFuture.ShowZero = false;
            this.textDaysFuture.Size = new System.Drawing.Size(52, 23);
            this.textDaysFuture.TabIndex = 66;
            // 
            // textDaysPast
            // 
            this.textDaysPast.AutoSize = false;
            this.textDaysPast.Location = new System.Drawing.Point(192, 47);
            this.textDaysPast.MaxVal = 10000;
            this.textDaysPast.Name = "textDaysPast";
            this.textDaysPast.ShowZero = false;
            this.textDaysPast.Size = new System.Drawing.Size(52, 23);
            this.textDaysPast.TabIndex = 65;
            // 
            // checkGroupFamilies
            // 
            this.checkGroupFamilies.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.checkGroupFamilies.Location = new System.Drawing.Point(8, 23);
            this.checkGroupFamilies.Name = "checkGroupFamilies";
            this.checkGroupFamilies.Size = new System.Drawing.Size(104, 20);
            this.checkGroupFamilies.TabIndex = 49;
            this.checkGroupFamilies.Text = "Group Families";
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(14, 47);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(172, 23);
            this.label14.TabIndex = 50;
            this.label14.Text = "Days Past (e.g. 545, blank, etc)";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(74, 74);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(112, 23);
            this.label15.TabIndex = 52;
            this.label15.Text = "Days Future (e.g. 7)";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label25
            // 
            this.label25.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label25.Location = new System.Drawing.Point(72, 476);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(132, 23);
            this.label25.TabIndex = 57;
            this.label25.Text = "Status for mailed recall";
            this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboStatusMailedRecall
            // 
            this.comboStatusMailedRecall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboStatusMailedRecall.Location = new System.Drawing.Point(210, 476);
            this.comboStatusMailedRecall.Name = "comboStatusMailedRecall";
            this.comboStatusMailedRecall.Size = new System.Drawing.Size(208, 23);
            this.comboStatusMailedRecall.TabIndex = 58;
            // 
            // label26
            // 
            this.label26.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label26.Location = new System.Drawing.Point(64, 503);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(140, 23);
            this.label26.TabIndex = 59;
            this.label26.Text = "Status for emailed recall";
            this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textDaysSecondReminder);
            this.groupBox1.Controls.Add(this.textDaysFirstReminder);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(422, 644);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(261, 81);
            this.groupBox1.TabIndex = 65;
            this.groupBox1.Text = "Also show in list if # of days since";
            // 
            // textDaysSecondReminder
            // 
            this.textDaysSecondReminder.AutoSize = false;
            this.textDaysSecondReminder.Location = new System.Drawing.Point(192, 50);
            this.textDaysSecondReminder.MaxVal = 10000;
            this.textDaysSecondReminder.Name = "textDaysSecondReminder";
            this.textDaysSecondReminder.ShowZero = false;
            this.textDaysSecondReminder.Size = new System.Drawing.Size(52, 23);
            this.textDaysSecondReminder.TabIndex = 66;
            // 
            // textDaysFirstReminder
            // 
            this.textDaysFirstReminder.AutoSize = false;
            this.textDaysFirstReminder.Location = new System.Drawing.Point(192, 23);
            this.textDaysFirstReminder.MaxVal = 10000;
            this.textDaysFirstReminder.Name = "textDaysFirstReminder";
            this.textDaysFirstReminder.ShowZero = false;
            this.textDaysFirstReminder.Size = new System.Drawing.Size(52, 23);
            this.textDaysFirstReminder.TabIndex = 65;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(90, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 23);
            this.label2.TabIndex = 50;
            this.label2.Text = "Initial Reminder";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(26, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(160, 23);
            this.label3.TabIndex = 52;
            this.label3.Text = "Second (or more) Reminder";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textMaxReminders
            // 
            this.textMaxReminders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textMaxReminders.AutoSize = false;
            this.textMaxReminders.Location = new System.Drawing.Point(837, 683);
            this.textMaxReminders.MaxVal = 10000;
            this.textMaxReminders.Name = "textMaxReminders";
            this.textMaxReminders.ShowZero = false;
            this.textMaxReminders.Size = new System.Drawing.Size(52, 23);
            this.textMaxReminders.TabIndex = 68;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Location = new System.Drawing.Point(687, 683);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(144, 23);
            this.label4.TabIndex = 67;
            this.label4.Text = "Max # Reminders (e.g. 4)";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboStatusEmailedRecall
            // 
            this.comboStatusEmailedRecall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboStatusEmailedRecall.Location = new System.Drawing.Point(210, 503);
            this.comboStatusEmailedRecall.Name = "comboStatusEmailedRecall";
            this.comboStatusEmailedRecall.Size = new System.Drawing.Size(208, 23);
            this.comboStatusEmailedRecall.TabIndex = 60;
            // 
            // listTypes
            // 
            this.listTypes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.listTypes.Location = new System.Drawing.Point(210, 644);
            this.listTypes.Name = "listTypes";
            this.listTypes.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
            this.listTypes.Size = new System.Drawing.Size(120, 82);
            this.listTypes.TabIndex = 64;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Location = new System.Drawing.Point(47, 644);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(157, 47);
            this.label1.TabIndex = 63;
            this.label1.Text = "Types to show in recall list (typically just prophy, perio, and user-added types)" +
    "";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // checkReturnAdd
            // 
            this.checkReturnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkReturnAdd.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.checkReturnAdd.Location = new System.Drawing.Point(210, 611);
            this.checkReturnAdd.Name = "checkReturnAdd";
            this.checkReturnAdd.Size = new System.Drawing.Size(134, 20);
            this.checkReturnAdd.TabIndex = 43;
            this.checkReturnAdd.Text = "Show return address";
            // 
            // butSave
            // 
            this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butSave.Location = new System.Drawing.Point(842, 738);
            this.butSave.Name = "butSave";
            this.butSave.Size = new System.Drawing.Size(75, 24);
            this.butSave.TabIndex = 3;
            this.butSave.Text = "&Save";
            this.butSave.Click += new System.EventHandler(this.butSave_Click);
            // 
            // comboStatusEmailTextRecall
            // 
            this.comboStatusEmailTextRecall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboStatusEmailTextRecall.Location = new System.Drawing.Point(210, 557);
            this.comboStatusEmailTextRecall.Name = "comboStatusEmailTextRecall";
            this.comboStatusEmailTextRecall.Size = new System.Drawing.Size(208, 23);
            this.comboStatusEmailTextRecall.TabIndex = 78;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.Location = new System.Drawing.Point(8, 557);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(196, 23);
            this.label5.TabIndex = 77;
            this.label5.Text = "Status for emailed and texted recall";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboStatusTextedRecall
            // 
            this.comboStatusTextedRecall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboStatusTextedRecall.Location = new System.Drawing.Point(210, 530);
            this.comboStatusTextedRecall.Name = "comboStatusTextedRecall";
            this.comboStatusTextedRecall.Size = new System.Drawing.Size(208, 23);
            this.comboStatusTextedRecall.TabIndex = 76;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.Location = new System.Drawing.Point(76, 530);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(128, 23);
            this.label6.TabIndex = 75;
            this.label6.Text = "Status for texted recall";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textMaxAutoReminders
            // 
            this.textMaxAutoReminders.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textMaxAutoReminders.AutoSize = false;
            this.textMaxAutoReminders.Location = new System.Drawing.Point(837, 656);
            this.textMaxAutoReminders.MaxVal = 10000;
            this.textMaxAutoReminders.Name = "textMaxAutoReminders";
            this.textMaxAutoReminders.ShowZero = false;
            this.textMaxAutoReminders.Size = new System.Drawing.Size(52, 23);
            this.textMaxAutoReminders.TabIndex = 80;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.Location = new System.Drawing.Point(699, 656);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(132, 23);
            this.label7.TabIndex = 79;
            this.label7.Text = "Max # Auto Reminders";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboLanguage
            // 
            this.comboLanguage.Location = new System.Drawing.Point(78, 8);
            this.comboLanguage.Name = "comboLanguage";
            this.comboLanguage.Size = new System.Drawing.Size(168, 23);
            this.comboLanguage.TabIndex = 81;
            this.comboLanguage.Text = "comboBox1";
            this.comboLanguage.SelectionChangeCommitted += new System.EventHandler(this.comboLanguage_SelectionChangeCommitted);
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = false;
            this.labelLanguage.Location = new System.Drawing.Point(8, 8);
            this.labelLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(64, 23);
            this.labelLanguage.TabIndex = 82;
            this.labelLanguage.Text = "Language";
            // 
            // FormRecallSetup
            // 
            this.ClientSize = new System.Drawing.Size(925, 770);
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.comboLanguage);
            this.Controls.Add(this.textMaxReminders);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textMaxAutoReminders);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.comboStatusEmailTextRecall);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboStatusTextedRecall);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.gridMain);
            this.Controls.Add(this.radioExcludeFutureYes);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.butSave);
            this.Controls.Add(this.radioExcludeFutureNo);
            this.Controls.Add(this.textPostcardsPerSheet);
            this.Controls.Add(this.checkReturnAdd);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listTypes);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.comboStatusEmailedRecall);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.comboStatusMailedRecall);
            this.Controls.Add(this.label25);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormRecallSetup";
            this.ShowInTaskbar = false;
            this.Text = "Setup Recall";
            this.Load += new System.EventHandler(this.FormRecallSetup_Load);
            this.groupBox4.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox textPostcardsPerSheet;
		private OpenPmsDental.UI.CheckBox checkReturnAdd;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private ValidDouble textDown;
		private Label label12;
		private ValidDouble textRight;
		private Label label13;
		private OpenPmsDental.UI.CheckBox checkGroupFamilies;
		private Label label14;
		private Label label15;
		private OpenPmsDental.UI.GroupBox groupBox3;
		private Label label25;
		private UI.ComboBox comboStatusMailedRecall;
		private UI.ComboBox comboStatusEmailedRecall;
		private Label label26;
		private UI.ListBox listTypes;
		private Label label1;
		private ValidNum textDaysPast;
		private ValidNum textDaysFuture;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private ValidNum textDaysSecondReminder;
		private ValidNum textDaysFirstReminder;
		private Label label2;
		private Label label3;
		private OpenPmsDental.UI.GridOD gridMain;
		private ValidNum textMaxReminders;//""= infinite, 0=disabled;
		private Label label4;
		private OpenPmsDental.UI.GroupBox groupBox4;
		private RadioButton radioUseEmailFalse;
		private RadioButton radioUseEmailTrue;
		private RadioButton radioExcludeFutureNo;
		private RadioButton radioExcludeFutureYes;
		private UI.ComboBox comboStatusEmailTextRecall;
		private Label label5;
		private UI.ComboBox comboStatusTextedRecall;
		private Label label6;
		private ValidNum textMaxAutoReminders;
		private Label label7;
		private UI.ComboBox comboLanguage;
		private Label labelLanguage;
	}
}
