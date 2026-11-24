using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormProcCodeNew {
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
			this.textNewCode = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.textDescription = new System.Windows.Forms.TextBox();
			this.labelListType = new System.Windows.Forms.Label();
			this.listType = new OpenPmsDental.UI.ListBox();
			this.label4 = new System.Windows.Forms.Label();
			this.textAbbreviation = new System.Windows.Forms.TextBox();
			this.checkNoBillIns = new OpenPmsDental.UI.CheckBox();
			this.checkIsHygiene = new OpenPmsDental.UI.CheckBox();
			this.checkIsProsth = new OpenPmsDental.UI.CheckBox();
			this.label5 = new System.Windows.Forms.Label();
			this.comboPaintType = new OpenPmsDental.UI.ComboBox();
			this.comboTreatArea = new OpenPmsDental.UI.ComboBox();
			this.labelTreatArea = new System.Windows.Forms.Label();
			this.comboCategory = new OpenPmsDental.UI.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.textCodePrevious = new System.Windows.Forms.TextBox();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label9 = new System.Windows.Forms.Label();
			this.butDefault = new OpenPmsDental.UI.Button();
			this.butAnother = new OpenPmsDental.UI.Button();
			this.butOK = new OpenPmsDental.UI.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// textNewCode
			// 
			this.textNewCode.Location = new System.Drawing.Point(168, 23);
			this.textNewCode.MaxLength = 15;
			this.textNewCode.Name = "textNewCode";
			this.textNewCode.Size = new System.Drawing.Size(144, 23);
			this.textNewCode.TabIndex = 0;
			this.textNewCode.TextChanged += new System.EventHandler(this.textNewCode_TextChanged);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(66, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 23);
			this.label1.TabIndex = 3;
			this.label1.Text = "Procedure Code";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(90, 50);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 6;
			this.label2.Text = "Description";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDescription
			// 
			this.textDescription.Location = new System.Drawing.Point(168, 50);
			this.textDescription.MaxLength = 255;
			this.textDescription.Name = "textDescription";
			this.textDescription.Size = new System.Drawing.Size(356, 20);
			this.textDescription.TabIndex = 1;
			// 
			// labelListType
			// 
			this.labelListType.Location = new System.Drawing.Point(8, 8);
			this.labelListType.Name = "labelListType";
			this.labelListType.Size = new System.Drawing.Size(209, 18);
			this.labelListType.TabIndex = 7;
			this.labelListType.Text = "Type";
			this.labelListType.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// listType
			// 
			this.listType.Location = new System.Drawing.Point(15, 28);
			this.listType.Name = "listType";
			this.listType.Size = new System.Drawing.Size(218, 381);
			this.listType.TabIndex = 8;
			this.listType.Click += new System.EventHandler(this.listType_Click);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(82, 77);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(80, 23);
			this.label4.TabIndex = 10;
			this.label4.Text = "Abbreviation";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textAbbreviation
			// 
			this.textAbbreviation.Location = new System.Drawing.Point(168, 77);
			this.textAbbreviation.MaxLength = 50;
			this.textAbbreviation.Name = "textAbbreviation";
			this.textAbbreviation.Size = new System.Drawing.Size(80, 20);
			this.textAbbreviation.TabIndex = 2;
			// 
			// checkNoBillIns
			// 
			this.checkNoBillIns.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkNoBillIns.Location = new System.Drawing.Point(433, 175);
			this.checkNoBillIns.Name = "checkNoBillIns";
			this.checkNoBillIns.Size = new System.Drawing.Size(191, 20);
			this.checkNoBillIns.TabIndex = 4;
			this.checkNoBillIns.Text = "Do not usually bill to insurance";
			// 
			// checkIsHygiene
			// 
			this.checkIsHygiene.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIsHygiene.Location = new System.Drawing.Point(433, 199);
			this.checkIsHygiene.Name = "checkIsHygiene";
			this.checkIsHygiene.Size = new System.Drawing.Size(138, 20);
			this.checkIsHygiene.TabIndex = 5;
			this.checkIsHygiene.Text = "Is Hygiene Procedure";
			// 
			// checkIsProsth
			// 
			this.checkIsProsth.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkIsProsth.Location = new System.Drawing.Point(433, 223);
			this.checkIsProsth.Name = "checkIsProsth";
			this.checkIsProsth.Size = new System.Drawing.Size(89, 20);
			this.checkIsProsth.TabIndex = 6;
			this.checkIsProsth.Text = "Is Prosthesis";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(359, 247);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(68, 23);
			this.label5.TabIndex = 15;
			this.label5.Text = "Paint Type";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboPaintType
			// 
			this.comboPaintType.Location = new System.Drawing.Point(433, 247);
			this.comboPaintType.Name = "comboPaintType";
			this.comboPaintType.Size = new System.Drawing.Size(164, 23);
			this.comboPaintType.TabIndex = 7;
			// 
			// comboTreatArea
			// 
			this.comboTreatArea.Location = new System.Drawing.Point(433, 274);
			this.comboTreatArea.Name = "comboTreatArea";
			this.comboTreatArea.Size = new System.Drawing.Size(164, 23);
			this.comboTreatArea.TabIndex = 8;
			// 
			// labelTreatArea
			// 
			this.labelTreatArea.Location = new System.Drawing.Point(335, 274);
			this.labelTreatArea.Name = "labelTreatArea";
			this.labelTreatArea.Size = new System.Drawing.Size(92, 23);
			this.labelTreatArea.TabIndex = 17;
			this.labelTreatArea.Text = "Treatment Area";
			this.labelTreatArea.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboCategory
			// 
			this.comboCategory.Location = new System.Drawing.Point(433, 301);
			this.comboCategory.Name = "comboCategory";
			this.comboCategory.Size = new System.Drawing.Size(164, 23);
			this.comboCategory.TabIndex = 9;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(367, 301);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(60, 23);
			this.label7.TabIndex = 19;
			this.label7.Text = "Category";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(287, 28);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(140, 23);
			this.label8.TabIndex = 21;
			this.label8.Text = "Previously Entered Code";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textCodePrevious
			// 
			this.textCodePrevious.Location = new System.Drawing.Point(433, 28);
			this.textCodePrevious.MaxLength = 15;
			this.textCodePrevious.Name = "textCodePrevious";
			this.textCodePrevious.ReadOnly = true;
			this.textCodePrevious.Size = new System.Drawing.Size(164, 23);
			this.textCodePrevious.TabIndex = 20;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.textNewCode);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.textDescription);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.textAbbreviation);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Location = new System.Drawing.Point(265, 59);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(541, 108);
			this.groupBox1.TabIndex = 22;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Edit these three fields for each new code";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panel1.Location = new System.Drawing.Point(8, 417);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(798, 2);
			this.panel1.TabIndex = 23;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(8, 427);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(777, 36);
			this.label9.TabIndex = 24;
			this.label9.Visible = false;
			// 
			// butDefault
			// 
			this.butDefault.Location = new System.Drawing.Point(8, 465);
			this.butDefault.Name = "butDefault";
			this.butDefault.Size = new System.Drawing.Size(104, 26);
			this.butDefault.TabIndex = 25;
			this.butDefault.Text = "Set to Default";
			this.butDefault.Visible = false;
			this.butDefault.Click += new System.EventHandler(this.butDefault_Click);
			// 
			// butAnother
			// 
			this.butAnother.Location = new System.Drawing.Point(495, 328);
			this.butAnother.Name = "butAnother";
			this.butAnother.Size = new System.Drawing.Size(114, 26);
			this.butAnother.TabIndex = 10;
			this.butAnother.Text = "Add, then another";
			this.butAnother.Click += new System.EventHandler(this.butAnother_Click);
			// 
			// butOK
			// 
			this.butOK.Location = new System.Drawing.Point(615, 328);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 26);
			this.butOK.TabIndex = 11;
			this.butOK.Text = "Add";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// FormProcCodeNew
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(814, 512);
			this.Controls.Add(this.butDefault);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.textCodePrevious);
			this.Controls.Add(this.comboCategory);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.comboTreatArea);
			this.Controls.Add(this.labelTreatArea);
			this.Controls.Add(this.comboPaintType);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.checkIsProsth);
			this.Controls.Add(this.checkIsHygiene);
			this.Controls.Add(this.checkNoBillIns);
			this.Controls.Add(this.listType);
			this.Controls.Add(this.labelListType);
			this.Controls.Add(this.butAnother);
			this.Controls.Add(this.butOK);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormProcCodeNew";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "New Code";
			this.Load += new System.EventHandler(this.FormProcCodeNew_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private OpenPmsDental.UI.Button butOK;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox textNewCode;
		private OpenPmsDental.UI.Button butAnother;
		private Label label2;
		public TextBox textDescription;
		private Label labelListType;
		private OpenPmsDental.UI.ListBox listType;
		private Label label4;
		public TextBox textAbbreviation;
		private OpenPmsDental.UI.CheckBox checkNoBillIns;
		private OpenPmsDental.UI.CheckBox checkIsHygiene;
		private OpenPmsDental.UI.CheckBox checkIsProsth;
		private Label label5;
		private OpenPmsDental.UI.ComboBox comboPaintType;
		private OpenPmsDental.UI.ComboBox comboTreatArea;
		private Label labelTreatArea;
		private OpenPmsDental.UI.ComboBox comboCategory;
		private Label label7;
		private Label label8;
		public TextBox textCodePrevious;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private Panel panel1;
		private Label label9;
		private OpenPmsDental.UI.Button butDefault;
	}
}
