using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormRpWriteoffSheet {
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
			this.date2 = new OpenPmsDental.UI.MonthCalendarOD();
			this.date1 = new OpenPmsDental.UI.MonthCalendarOD();
			this.labelTO = new System.Windows.Forms.Label();
			this.listProv = new OpenPmsDental.UI.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox3 = new OpenPmsDental.UI.GroupBox();
			this.radioWriteoffClaim = new System.Windows.Forms.RadioButton();
			this.label5 = new System.Windows.Forms.Label();
			this.radioWriteoffProc = new System.Windows.Forms.RadioButton();
			this.radioWriteoffPay = new System.Windows.Forms.RadioButton();
			this.butOK = new OpenPmsDental.UI.Button();
			this.checkAllProv = new OpenPmsDental.UI.CheckBox();
			this.checkAllClin = new OpenPmsDental.UI.CheckBox();
			this.listClin = new OpenPmsDental.UI.ListBox();
			this.labelClin = new System.Windows.Forms.Label();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// date2
			// 
			this.date2.AllowKeyboard = true;
			this.date2.Location = new System.Drawing.Point(238, 8);
			this.date2.MaxSelectionCount = 7;
			this.date2.Size = new System.Drawing.Size(227, 162);
			this.date2.Name = "date2";
			this.date2.TabIndex = 2;
			// 
			// date1
			// 
			this.date1.AllowKeyboard = true;
			this.date1.Location = new System.Drawing.Point(8, 8);
			this.date1.MaxSelectionCount = 7;
			this.date1.Size = new System.Drawing.Size(227, 162);
			this.date1.Name = "date1";
			this.date1.TabIndex = 1;
			// 
			// labelTO
			// 
			this.labelTO.Location = new System.Drawing.Point(204, 84);
			this.labelTO.Name = "labelTO";
			this.labelTO.Size = new System.Drawing.Size(28, 23);
			this.labelTO.TabIndex = 28;
			this.labelTO.Text = "TO";
			this.labelTO.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// listProv
			// 
			this.listProv.Location = new System.Drawing.Point(679, 50);
			this.listProv.Name = "listProv";
			this.listProv.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listProv.Size = new System.Drawing.Size(228, 134);
			this.listProv.TabIndex = 36;
			this.listProv.Click += new System.EventHandler(this.listProv_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(679, 8);
			this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(228, 18);
			this.label1.TabIndex = 35;
			this.label1.Text = "Providers";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.radioWriteoffClaim);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Controls.Add(this.radioWriteoffProc);
			this.groupBox3.Controls.Add(this.radioWriteoffPay);
			this.groupBox3.Location = new System.Drawing.Point(8, 192);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(522, 120);
			this.groupBox3.TabIndex = 47;
			this.groupBox3.Text = "Show Insurance Write-offs";
			// 
			// radioWriteoffClaim
			// 
			this.radioWriteoffClaim.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.radioWriteoffClaim.Location = new System.Drawing.Point(8, 71);
			this.radioWriteoffClaim.Name = "radioWriteoffClaim";
			this.radioWriteoffClaim.Size = new System.Drawing.Size(506, 20);
			this.radioWriteoffClaim.TabIndex = 3;
			this.radioWriteoffClaim.Text = "Using initial claim date for write-off estimates, insurance pay date for write-of" +
    "f adjustments\r\n";
			this.radioWriteoffClaim.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.radioWriteoffClaim.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 95);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(284, 17);
			this.label5.TabIndex = 2;
			this.label5.Text = "(This is discussed in the PPO section of the manual)";
			// 
			// radioWriteoffProc
			// 
			this.radioWriteoffProc.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.radioWriteoffProc.Location = new System.Drawing.Point(8, 47);
			this.radioWriteoffProc.Name = "radioWriteoffProc";
			this.radioWriteoffProc.Size = new System.Drawing.Size(141, 20);
			this.radioWriteoffProc.TabIndex = 1;
			this.radioWriteoffProc.Text = "Using procedure date.";
			this.radioWriteoffProc.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.radioWriteoffProc.UseVisualStyleBackColor = true;
			// 
			// radioWriteoffPay
			// 
			this.radioWriteoffPay.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.radioWriteoffPay.Checked = true;
			this.radioWriteoffPay.Location = new System.Drawing.Point(8, 23);
			this.radioWriteoffPay.Name = "radioWriteoffPay";
			this.radioWriteoffPay.Size = new System.Drawing.Size(188, 20);
			this.radioWriteoffPay.TabIndex = 0;
			this.radioWriteoffPay.TabStop = true;
			this.radioWriteoffPay.Text = "Using insurance payment date.";
			this.radioWriteoffPay.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.radioWriteoffPay.UseVisualStyleBackColor = true;
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(832, 384);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 26);
			this.butOK.TabIndex = 3;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// checkAllProv
			// 
			this.checkAllProv.Checked = true;
			this.checkAllProv.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAllProv.Location = new System.Drawing.Point(679, 28);
			this.checkAllProv.Name = "checkAllProv";
			this.checkAllProv.Size = new System.Drawing.Size(36, 20);
			this.checkAllProv.TabIndex = 48;
			this.checkAllProv.Text = "All";
			this.checkAllProv.Click += new System.EventHandler(this.checkAllProv_Click);
			// 
			// checkAllClin
			// 
			this.checkAllClin.Location = new System.Drawing.Point(444, 28);
			this.checkAllClin.Name = "checkAllClin";
			this.checkAllClin.Size = new System.Drawing.Size(134, 20);
			this.checkAllClin.TabIndex = 54;
			this.checkAllClin.Text = "All (Includes hidden)";
			this.checkAllClin.Click += new System.EventHandler(this.checkAllClin_Click);
			// 
			// listClin
			// 
			this.listClin.Location = new System.Drawing.Point(444, 50);
			this.listClin.Name = "listClin";
			this.listClin.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listClin.Size = new System.Drawing.Size(227, 134);
			this.listClin.TabIndex = 53;
			this.listClin.Click += new System.EventHandler(this.listClin_Click);
			// 
			// labelClin
			// 
			this.labelClin.Location = new System.Drawing.Point(444, 8);
			this.labelClin.Name = "labelClin";
			this.labelClin.Size = new System.Drawing.Size(227, 18);
			this.labelClin.TabIndex = 52;
			this.labelClin.Text = "Clinics";
			this.labelClin.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// FormRpWriteoffSheet
			// 
			this.ClientSize = new System.Drawing.Size(915, 418);
			this.Controls.Add(this.checkAllClin);
			this.Controls.Add(this.listClin);
			this.Controls.Add(this.labelClin);
			this.Controls.Add(this.checkAllProv);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.listProv);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.date2);
			this.Controls.Add(this.date1);
			this.Controls.Add(this.labelTO);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormRpWriteoffSheet";
			this.ShowInTaskbar = false;
			this.Text = "Daily Write-off Report";
			this.Load += new System.EventHandler(this.FormDailyWriteoff_Load);
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
		private OpenPmsDental.UI.Button butOK;
		private OpenPmsDental.UI.MonthCalendarOD date2;
		private OpenPmsDental.UI.MonthCalendarOD date1;
		private System.Windows.Forms.Label labelTO;
		private OpenPmsDental.UI.ListBox listProv;
		private Label label1;
		private OpenPmsDental.UI.GroupBox groupBox3;
		private Label label5;
		private RadioButton radioWriteoffProc;
		private RadioButton radioWriteoffPay;
		private OpenPmsDental.UI.CheckBox checkAllProv;
		private OpenPmsDental.UI.CheckBox checkAllClin;
		private OpenPmsDental.UI.ListBox listClin;
		private Label labelClin;
		private RadioButton radioWriteoffClaim;
	}
}
