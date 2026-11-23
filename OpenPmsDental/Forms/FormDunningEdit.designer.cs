using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormDunningEdit {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDunningEdit));
			this.label1 = new System.Windows.Forms.Label();
			this.textDunMessage = new System.Windows.Forms.TextBox();
			this.listBillType = new OpenPmsDental.UI.ListBox();
			this.label8 = new System.Windows.Forms.Label();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.labelDaysInAdvance = new System.Windows.Forms.Label();
			this.textDaysInAdvance = new OpenPmsDental.ValidNum();
			this.radio30 = new System.Windows.Forms.RadioButton();
			this.radio90 = new System.Windows.Forms.RadioButton();
			this.radio60 = new System.Windows.Forms.RadioButton();
			this.radioAny = new System.Windows.Forms.RadioButton();
			this.groupBox2 = new OpenPmsDental.UI.GroupBox();
			this.radioY = new System.Windows.Forms.RadioButton();
			this.radioN = new System.Windows.Forms.RadioButton();
			this.radioU = new System.Windows.Forms.RadioButton();
			this.textMessageBold = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox3 = new OpenPmsDental.UI.GroupBox();
			this.groupBox4 = new OpenPmsDental.UI.GroupBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textEmailBody = new System.Windows.Forms.TextBox();
			this.textEmailSubject = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.labelEmailType = new System.Windows.Forms.Label();
			this.comboEmailType = new OpenPmsDental.UI.ComboBox();
			this.butEditHtml = new OpenPmsDental.UI.Button();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.butSave = new OpenPmsDental.UI.Button();
			this.comboClinics = new OpenPmsDental.UI.ComboBoxClinicPicker();
			this.checkSuperFamily = new OpenPmsDental.UI.CheckBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(148, 17);
			this.label1.TabIndex = 2;
			this.label1.Text = "Message";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textDunMessage
			// 
			this.textDunMessage.AcceptsReturn = true;
			this.textDunMessage.AcceptsTab = true;
			this.textDunMessage.Location = new System.Drawing.Point(8, 42);
			this.textDunMessage.Multiline = true;
			this.textDunMessage.Name = "textDunMessage";
			this.textDunMessage.Size = new System.Drawing.Size(428, 89);
			this.textDunMessage.TabIndex = 0;
			// 
			// listBillType
			// 
			this.listBillType.Location = new System.Drawing.Point(8, 39);
			this.listBillType.Name = "listBillType";
			this.listBillType.Size = new System.Drawing.Size(158, 233);
			this.listBillType.TabIndex = 113;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(8, 20);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(158, 17);
			this.label8.TabIndex = 114;
			this.label8.Text = "Billing Type:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.labelDaysInAdvance);
			this.groupBox1.Controls.Add(this.textDaysInAdvance);
			this.groupBox1.Controls.Add(this.radio30);
			this.groupBox1.Controls.Add(this.radio90);
			this.groupBox1.Controls.Add(this.radio60);
			this.groupBox1.Controls.Add(this.radioAny);
			this.groupBox1.Location = new System.Drawing.Point(174, 39);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(278, 126);
			this.groupBox1.TabIndex = 115;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Age of Account";
			// 
			// labelDaysInAdvance
			// 
			this.labelDaysInAdvance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelDaysInAdvance.Location = new System.Drawing.Point(155, 95);
			this.labelDaysInAdvance.Name = "labelDaysInAdvance";
			this.labelDaysInAdvance.Size = new System.Drawing.Size(76, 23);
			this.labelDaysInAdvance.TabIndex = 121;
			this.labelDaysInAdvance.Text = "Days in Adv";
			this.labelDaysInAdvance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDaysInAdvance
			// 
			this.textDaysInAdvance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.textDaysInAdvance.Location = new System.Drawing.Point(238, 95);
			this.textDaysInAdvance.MaxVal = 2147483647;
			this.textDaysInAdvance.MinVal = 0;
			this.textDaysInAdvance.Name = "textDaysInAdvance";
			this.textDaysInAdvance.Size = new System.Drawing.Size(32, 23);
			this.textDaysInAdvance.TabIndex = 4;
			this.textDaysInAdvance.ShowZero = false;
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
			this.radio90.Location = new System.Drawing.Point(8, 96);
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
			this.radioAny.Location = new System.Drawing.Point(8, 23);
			this.radioAny.Name = "radioAny";
			this.radioAny.Size = new System.Drawing.Size(90, 20);
			this.radioAny.TabIndex = 0;
			this.radioAny.Text = "Any Balance";
			this.radioAny.CheckedChanged += new System.EventHandler(this.radioAny_CheckedChanged);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.radioY);
			this.groupBox2.Controls.Add(this.radioN);
			this.groupBox2.Controls.Add(this.radioU);
			this.groupBox2.Location = new System.Drawing.Point(174, 173);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(278, 99);
			this.groupBox2.TabIndex = 117;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Insurance Payment Pending";
			// 
			// radioY
			// 
			this.radioY.Location = new System.Drawing.Point(8, 47);
			this.radioY.Name = "radioY";
			this.radioY.Size = new System.Drawing.Size(42, 20);
			this.radioY.TabIndex = 1;
			this.radioY.TabStop = true;
			this.radioY.Text = "Yes";
			// 
			// radioN
			// 
			this.radioN.Location = new System.Drawing.Point(8, 71);
			this.radioN.Name = "radioN";
			this.radioN.Size = new System.Drawing.Size(41, 20);
			this.radioN.TabIndex = 2;
			this.radioN.TabStop = true;
			this.radioN.Text = "No";
			// 
			// radioU
			// 
			this.radioU.Checked = true;
			this.radioU.Location = new System.Drawing.Point(8, 23);
			this.radioU.Name = "radioU";
			this.radioU.Size = new System.Drawing.Size(103, 20);
			this.radioU.TabIndex = 0;
			this.radioU.TabStop = true;
			this.radioU.Text = "Doesn\'t Matter";
			// 
			// textMessageBold
			// 
			this.textMessageBold.AcceptsReturn = true;
			this.textMessageBold.AcceptsTab = true;
			this.textMessageBold.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textMessageBold.ForeColor = System.Drawing.Color.DarkRed;
			this.textMessageBold.Location = new System.Drawing.Point(8, 154);
			this.textMessageBold.Multiline = true;
			this.textMessageBold.Name = "textMessageBold";
			this.textMessageBold.Size = new System.Drawing.Size(428, 89);
			this.textMessageBold.TabIndex = 118;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 135);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(148, 17);
			this.label2.TabIndex = 119;
			this.label2.Text = "Bold Message";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.textMessageBold);
			this.groupBox3.Controls.Add(this.textDunMessage);
			this.groupBox3.Controls.Add(this.label2);
			this.groupBox3.Location = new System.Drawing.Point(8, 280);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(444, 251);
			this.groupBox3.TabIndex = 118;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Statement Notes";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.butEditHtml);
			this.groupBox4.Controls.Add(this.comboEmailType);
			this.groupBox4.Controls.Add(this.labelEmailType);
			this.groupBox4.Controls.Add(this.label10);
			this.groupBox4.Controls.Add(this.label3);
			this.groupBox4.Controls.Add(this.textEmailBody);
			this.groupBox4.Controls.Add(this.textEmailSubject);
			this.groupBox4.Controls.Add(this.label4);
			this.groupBox4.Location = new System.Drawing.Point(460, 39);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(442, 492);
			this.groupBox4.TabIndex = 119;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Email Statement Override";
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(8, 23);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(426, 62);
			this.label10.TabIndex = 250;
			this.label10.Text = resources.GetString("label10.Text");
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 87);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(412, 17);
			this.label3.TabIndex = 2;
			this.label3.Text = "Subject";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textEmailBody
			// 
			this.textEmailBody.AcceptsReturn = true;
			this.textEmailBody.AcceptsTab = true;
			this.textEmailBody.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.textEmailBody.ForeColor = System.Drawing.Color.Black;
			this.textEmailBody.Location = new System.Drawing.Point(8, 160);
			this.textEmailBody.Multiline = true;
			this.textEmailBody.Name = "textEmailBody";
			this.textEmailBody.Size = new System.Drawing.Size(426, 324);
			this.textEmailBody.TabIndex = 118;
			// 
			// textEmailSubject
			// 
			this.textEmailSubject.AcceptsReturn = true;
			this.textEmailSubject.AcceptsTab = true;
			this.textEmailSubject.Location = new System.Drawing.Point(8, 106);
			this.textEmailSubject.MaxLength = 200;
			this.textEmailSubject.Multiline = true;
			this.textEmailSubject.Name = "textEmailSubject";
			this.textEmailSubject.Size = new System.Drawing.Size(426, 23);
			this.textEmailSubject.TabIndex = 0;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 136);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(60, 17);
			this.label4.TabIndex = 119;
			this.label4.Text = "Body";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelEmailType
			// 
			this.labelEmailType.Location = new System.Drawing.Point(108, 133);
			this.labelEmailType.Name = "labelEmailType";
			this.labelEmailType.Size = new System.Drawing.Size(68, 23);
			this.labelEmailType.TabIndex = 251;
			this.labelEmailType.Text = "Email type";
			this.labelEmailType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboEmailType
			// 
			this.comboEmailType.Location = new System.Drawing.Point(182, 133);
			this.comboEmailType.Name = "comboEmailType";
			this.comboEmailType.Size = new System.Drawing.Size(140, 23);
			this.comboEmailType.TabIndex = 252;
			// 
			// butEditHtml
			// 
			this.butEditHtml.Location = new System.Drawing.Point(330, 133);
			this.butEditHtml.Name = "butEditHtml";
			this.butEditHtml.Size = new System.Drawing.Size(100, 23);
			this.butEditHtml.TabIndex = 253;
			this.butEditHtml.Text = "Edit HTML...";
			this.butEditHtml.Click += new System.EventHandler(this.butEditHtml_Click);
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(8, 539);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(81, 24);
			this.butDelete.TabIndex = 4;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(826, 539);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(76, 24);
			this.butSave.TabIndex = 8;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// comboClinics
			// 
			this.comboClinics.IncludeUnassigned = true;
			this.comboClinics.IncludeAll = true;
			this.comboClinics.Location = new System.Drawing.Point(727, 8);
			this.comboClinics.Name = "comboClinics";
			this.comboClinics.Size = new System.Drawing.Size(175, 23);
			this.comboClinics.TabIndex = 255;
			// 
			// checkSuperFamily
			// 
			this.checkSuperFamily.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkSuperFamily.Location = new System.Drawing.Point(460, 9);
			this.checkSuperFamily.Name = "checkSuperFamily";
			this.checkSuperFamily.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkSuperFamily.Size = new System.Drawing.Size(92, 20);
			this.checkSuperFamily.TabIndex = 258;
			this.checkSuperFamily.Text = "Super Family";
			this.checkSuperFamily.Visible = false;
			// 
			// FormDunningEdit
			// 
			this.ClientSize = new System.Drawing.Size(910, 571);
			this.Controls.Add(this.checkSuperFamily);
			this.Controls.Add(this.comboClinics);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.listBillType);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.butSave);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormDunningEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Dunning Message";
			this.Load += new System.EventHandler(this.FormDunningEdit_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label1;
		private OpenPmsDental.UI.Button butDelete;
		private OpenPmsDental.UI.ListBox listBillType;
		private System.Windows.Forms.Label label8;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radio30;
		private System.Windows.Forms.RadioButton radio90;
		private System.Windows.Forms.RadioButton radio60;
		private System.Windows.Forms.RadioButton radioAny;
		private System.Windows.Forms.TextBox textDunMessage;
		private OpenPmsDental.UI.GroupBox groupBox2;
		private System.Windows.Forms.RadioButton radioY;
		private System.Windows.Forms.RadioButton radioN;
		private System.Windows.Forms.RadioButton radioU;
		private TextBox textMessageBold;
		private Label label2;
		private OpenPmsDental.UI.GroupBox groupBox3;
		private OpenPmsDental.UI.GroupBox groupBox4;
		private Label label3;
		private TextBox textEmailBody;
		private TextBox textEmailSubject;
		private Label label4;
		private Label label10;
		private Label labelEmailType;
		private UI.ComboBox comboEmailType;
		private UI.Button butEditHtml;
		private Label labelDaysInAdvance;
		private ValidNum textDaysInAdvance;
		private UI.ComboBoxClinicPicker comboClinics;
		private OpenPmsDental.UI.CheckBox checkSuperFamily;
	}
}
