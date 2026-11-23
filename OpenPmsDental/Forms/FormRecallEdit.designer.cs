using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormRecallEdit {
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
			this.textDatePrevious = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textDateDueCalc = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.textWeeks = new OpenPmsDental.ValidNum();
			this.label7 = new System.Windows.Forms.Label();
			this.textDays = new OpenPmsDental.ValidNum();
			this.label6 = new System.Windows.Forms.Label();
			this.textMonths = new OpenPmsDental.ValidNum();
			this.label5 = new System.Windows.Forms.Label();
			this.textYears = new OpenPmsDental.ValidNum();
			this.label4 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.comboStatus = new OpenPmsDental.UI.ComboBox();
			this.checkIsDisabled = new OpenPmsDental.UI.CheckBox();
			this.label9 = new System.Windows.Forms.Label();
			this.comboType = new OpenPmsDental.UI.ComboBox();
			this.label10 = new System.Windows.Forms.Label();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.textBalance = new OpenPmsDental.ValidDouble();
			this.textDisableDate = new OpenPmsDental.ValidDate();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.textScheduledDate = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.butSave = new OpenPmsDental.UI.Button();
			this.textDateDue = new OpenPmsDental.ValidDate();
			this.checkASAP = new OpenPmsDental.UI.CheckBox();
			this.textNote = new OpenPmsDental.ODtextBox();
			this.labelPattern = new System.Windows.Forms.Label();
			this.textPattern = new System.Windows.Forms.TextBox();
			this.labelPatternDesc = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// textDatePrevious
			// 
			this.textDatePrevious.Location = new System.Drawing.Point(183, 182);
			this.textDatePrevious.Name = "textDatePrevious";
			this.textDatePrevious.ReadOnly = true;
			this.textDatePrevious.Size = new System.Drawing.Size(84, 23);
			this.textDatePrevious.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(93, 182);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(84, 23);
			this.label1.TabIndex = 2;
			this.label1.Text = "Previous Date";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(57, 209);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 23);
			this.label2.TabIndex = 4;
			this.label2.Text = "Calculated Due Date";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDateDueCalc
			// 
			this.textDateDueCalc.Location = new System.Drawing.Point(183, 209);
			this.textDateDueCalc.Name = "textDateDueCalc";
			this.textDateDueCalc.ReadOnly = true;
			this.textDateDueCalc.Size = new System.Drawing.Size(84, 23);
			this.textDateDueCalc.TabIndex = 3;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(81, 236);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(96, 23);
			this.label3.TabIndex = 5;
			this.label3.Text = "Actual Due Date";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.textWeeks);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.textDays);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.textMonths);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.textYears);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Location = new System.Drawing.Point(78, 39);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(170, 135);
			this.groupBox1.TabIndex = 7;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Recall Interval";
			// 
			// textWeeks
			// 
			this.textWeeks.AutoSize = false;
			this.textWeeks.Location = new System.Drawing.Point(105, 77);
			this.textWeeks.MaxVal = 255;
			this.textWeeks.MinVal = 0;
			this.textWeeks.Name = "textWeeks";
			this.textWeeks.Size = new System.Drawing.Size(52, 23);
			this.textWeeks.TabIndex = 10;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(51, 77);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(48, 23);
			this.label7.TabIndex = 11;
			this.label7.Text = "Weeks";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDays
			// 
			this.textDays.AutoSize = false;
			this.textDays.Location = new System.Drawing.Point(105, 104);
			this.textDays.MaxVal = 255;
			this.textDays.MinVal = 0;
			this.textDays.Name = "textDays";
			this.textDays.Size = new System.Drawing.Size(52, 23);
			this.textDays.TabIndex = 12;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(63, 104);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(36, 23);
			this.label6.TabIndex = 9;
			this.label6.Text = "Days";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textMonths
			// 
			this.textMonths.Location = new System.Drawing.Point(105, 50);
			this.textMonths.MaxVal = 255;
			this.textMonths.MinVal = 0;
			this.textMonths.Name = "textMonths";
			this.textMonths.Size = new System.Drawing.Size(52, 23);
			this.textMonths.TabIndex = 8;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(47, 50);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(52, 23);
			this.label5.TabIndex = 7;
			this.label5.Text = "Months";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textYears
			// 
			this.textYears.Location = new System.Drawing.Point(105, 23);
			this.textYears.MaxVal = 127;
			this.textYears.MinVal = 0;
			this.textYears.Name = "textYears";
			this.textYears.Size = new System.Drawing.Size(52, 23);
			this.textYears.TabIndex = 6;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(59, 23);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 23);
			this.label4.TabIndex = 5;
			this.label4.Text = "Years";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(133, 290);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(44, 23);
			this.label8.TabIndex = 8;
			this.label8.Text = "Status";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboStatus
			// 
			this.comboStatus.Location = new System.Drawing.Point(183, 290);
			this.comboStatus.Name = "comboStatus";
			this.comboStatus.Size = new System.Drawing.Size(188, 23);
			this.comboStatus.TabIndex = 9;
			// 
			// checkIsDisabled
			// 
			this.checkIsDisabled.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIsDisabled.Location = new System.Drawing.Point(8, 23);
			this.checkIsDisabled.Name = "checkIsDisabled";
			this.checkIsDisabled.Size = new System.Drawing.Size(110, 20);
			this.checkIsDisabled.TabIndex = 10;
			this.checkIsDisabled.Text = "Always Disabled";
			this.checkIsDisabled.Click += new System.EventHandler(this.checkIsDisabled_Click);
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(60, 368);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(117, 62);
			this.label9.TabIndex = 11;
			this.label9.Text = "Administrative Note (this note will get deleted every time recall gets reset)";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboType
			// 
			this.comboType.Location = new System.Drawing.Point(183, 8);
			this.comboType.Name = "comboType";
			this.comboType.Size = new System.Drawing.Size(188, 23);
			this.comboType.TabIndex = 17;
			this.comboType.SelectionChangeCommitted += new System.EventHandler(this.comboType_SelectionChangeCommitted);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(141, 8);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(36, 23);
			this.label10.TabIndex = 16;
			this.label10.Text = "Type";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.textBalance);
			this.groupBox2.Controls.Add(this.textDisableDate);
			this.groupBox2.Controls.Add(this.label12);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Controls.Add(this.checkIsDisabled);
			this.groupBox2.Location = new System.Drawing.Point(423, 39);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(243, 135);
			this.groupBox2.TabIndex = 18;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Disable Recall";
			// 
			// textBalance
			// 
			this.textBalance.BackColor = System.Drawing.SystemColors.Window;
			this.textBalance.ForeColor = System.Drawing.SystemColors.WindowText;
			this.textBalance.Location = new System.Drawing.Point(152, 51);
			this.textBalance.MaxVal = 100000000D;
			this.textBalance.MinVal = -100000000D;
			this.textBalance.Name = "textBalance";
			this.textBalance.Size = new System.Drawing.Size(83, 23);
			this.textBalance.TabIndex = 19;
			// 
			// textDisableDate
			// 
			this.textDisableDate.Location = new System.Drawing.Point(152, 83);
			this.textDisableDate.Name = "textDisableDate";
			this.textDisableDate.Size = new System.Drawing.Size(83, 23);
			this.textDisableDate.TabIndex = 13;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(82, 83);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(64, 23);
			this.label12.TabIndex = 12;
			this.label12.Text = "Until Date";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(8, 47);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(138, 32);
			this.label11.TabIndex = 11;
			this.label11.Text = "Until family Account balance is below";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textScheduledDate
			// 
			this.textScheduledDate.Location = new System.Drawing.Point(183, 263);
			this.textScheduledDate.Name = "textScheduledDate";
			this.textScheduledDate.ReadOnly = true;
			this.textScheduledDate.Size = new System.Drawing.Size(84, 23);
			this.textScheduledDate.TabIndex = 19;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(81, 263);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(96, 23);
			this.label13.TabIndex = 20;
			this.label13.Text = "Scheduled Date";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(26, 492);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(83, 24);
			this.butDelete.TabIndex = 14;
			this.butDelete.Text = "&Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(640, 492);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 13;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// textDateDue
			// 
			this.textDateDue.Location = new System.Drawing.Point(183, 236);
			this.textDateDue.Name = "textDateDue";
			this.textDateDue.Size = new System.Drawing.Size(84, 23);
			this.textDateDue.TabIndex = 6;
			// 
			// checkASAP
			// 
			this.checkASAP.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkASAP.Location = new System.Drawing.Point(183, 344);
			this.checkASAP.Name = "checkASAP";
			this.checkASAP.Size = new System.Drawing.Size(105, 20);
			this.checkASAP.TabIndex = 185;
			this.checkASAP.Text = "Schedule ASAP";
			// 
			// textNote
			// 
			this.textNote.AcceptsTab = true;
			this.textNote.BackColor = System.Drawing.SystemColors.Window;
			this.textNote.DetectLinksEnabled = false;
			this.textNote.DetectUrls = false;
			this.textNote.Location = new System.Drawing.Point(183, 368);
			this.textNote.Name = "textNote";
			this.textNote.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.Recall;
			this.textNote.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textNote.Size = new System.Drawing.Size(350, 112);
			this.textNote.TabIndex = 15;
			this.textNote.Text = "";
			// 
			// labelPattern
			// 
			this.labelPattern.Location = new System.Drawing.Point(49, 317);
			this.labelPattern.Name = "labelPattern";
			this.labelPattern.Size = new System.Drawing.Size(128, 23);
			this.labelPattern.TabIndex = 186;
			this.labelPattern.Text = "Time Pattern Override";
			this.labelPattern.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPattern
			// 
			this.textPattern.Location = new System.Drawing.Point(183, 317);
			this.textPattern.Name = "textPattern";
			this.textPattern.Size = new System.Drawing.Size(164, 23);
			this.textPattern.TabIndex = 187;
			// 
			// labelPatternDesc
			// 
			this.labelPatternDesc.Location = new System.Drawing.Point(353, 319);
			this.labelPatternDesc.Name = "labelPatternDesc";
			this.labelPatternDesc.Size = new System.Drawing.Size(100, 19);
			this.labelPatternDesc.TabIndex = 188;
			this.labelPatternDesc.Text = "(only /\'s and X\'s)";
			this.labelPatternDesc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// FormRecallEdit
			// 
			this.ClientSize = new System.Drawing.Size(723, 524);
			this.Controls.Add(this.labelPatternDesc);
			this.Controls.Add(this.textPattern);
			this.Controls.Add(this.labelPattern);
			this.Controls.Add(this.checkASAP);
			this.Controls.Add(this.textScheduledDate);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.comboType);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.textNote);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.textDateDue);
			this.Controls.Add(this.textDateDueCalc);
			this.Controls.Add(this.textDatePrevious);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.comboStatus);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormRecallEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Recall";
			this.Load += new System.EventHandler(this.FormRecallEdit_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private OpenPmsDental.ValidNum textYears;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private OpenPmsDental.UI.ComboBox comboStatus;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox textDatePrevious;
		private OpenPmsDental.ValidNum textMonths;
		private OpenPmsDental.ValidNum textDays;
		private OpenPmsDental.ValidNum textWeeks;
		private OpenPmsDental.UI.CheckBox checkIsDisabled;
		private OpenPmsDental.UI.Button butSave;
		private OpenPmsDental.UI.Button butDelete;
		private System.Windows.Forms.TextBox textDateDueCalc;
		private OpenPmsDental.ValidDate textDateDue;
		private OpenPmsDental.ODtextBox textNote;
		private OpenPmsDental.UI.ComboBox comboType;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private Label label11;
		private ValidDate textDisableDate;
		private Label label12;
		private ValidDouble textBalance;
		private TextBox textScheduledDate;
		private Label label13;
		private OpenPmsDental.UI.CheckBox checkASAP;
		private Label label10;
		private Label labelPattern;
		private TextBox textPattern;
		private Label labelPatternDesc;
	}
}
