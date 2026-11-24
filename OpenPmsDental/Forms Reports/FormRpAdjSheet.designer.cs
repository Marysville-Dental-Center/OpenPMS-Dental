using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormRpAdjSheet {
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
			this.butOK = new OpenPmsDental.UI.Button();
			this.date2 = new OpenPmsDental.UI.MonthCalendarOD();
			this.date1 = new OpenPmsDental.UI.MonthCalendarOD();
			this.labelTO = new System.Windows.Forms.Label();
			this.listProv = new OpenPmsDental.UI.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.listType = new OpenPmsDental.UI.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.checkAllProv = new OpenPmsDental.UI.CheckBox();
			this.checkAllClin = new OpenPmsDental.UI.CheckBox();
			this.listClin = new OpenPmsDental.UI.ListBox();
			this.labelClin = new System.Windows.Forms.Label();
			this.checkAllAdjs = new OpenPmsDental.UI.CheckBox();
			this.SuspendLayout();
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(640, 414);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 26);
			this.butOK.TabIndex = 3;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// date2
			// 
			this.date2.AllowKeyboard = true;
			this.date2.Location = new System.Drawing.Point(288, 26);
			this.date2.MaxSelectionCount = 7;
			this.date2.Name = "date2";
			this.date2.Size = new System.Drawing.Size(227, 162);
			this.date2.TabIndex = 2;
			// 
			// date1
			// 
			this.date1.AllowKeyboard = true;
			this.date1.Location = new System.Drawing.Point(29, 26);
			this.date1.MaxSelectionCount = 7;
			this.date1.Name = "date1";
			this.date1.Size = new System.Drawing.Size(227, 162);
			this.date1.TabIndex = 1;
			// 
			// labelTO
			// 
			this.labelTO.Location = new System.Drawing.Point(258, 26);
			this.labelTO.Name = "labelTO";
			this.labelTO.Size = new System.Drawing.Size(28, 23);
			this.labelTO.TabIndex = 28;
			this.labelTO.Text = "TO";
			this.labelTO.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// listProv
			// 
			this.listProv.Location = new System.Drawing.Point(533, 50);
			this.listProv.Name = "listProv";
			this.listProv.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listProv.Size = new System.Drawing.Size(181, 147);
			this.listProv.TabIndex = 36;
			this.listProv.Click += new System.EventHandler(this.listProv_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(530, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 16);
			this.label1.TabIndex = 35;
			this.label1.Text = "Providers";
			// 
			// listType
			// 
			this.listType.Location = new System.Drawing.Point(29, 255);
			this.listType.Name = "listType";
			this.listType.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listType.Size = new System.Drawing.Size(227, 147);
			this.listType.TabIndex = 39;
			this.listType.Click += new System.EventHandler(this.listType_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(29, 210);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 17);
			this.label2.TabIndex = 38;
			this.label2.Text = "Adjustment Types";
			// 
			// checkAllProv
			// 
			this.checkAllProv.Checked = true;
			this.checkAllProv.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAllProv.Location = new System.Drawing.Point(533, 26);
			this.checkAllProv.Name = "checkAllProv";
			this.checkAllProv.Size = new System.Drawing.Size(36, 20);
			this.checkAllProv.TabIndex = 48;
			this.checkAllProv.Text = "All";
			this.checkAllProv.Click += new System.EventHandler(this.checkAllProv_Click);
			// 
			// checkAllClin
			// 
			this.checkAllClin.Location = new System.Drawing.Point(262, 231);
			this.checkAllClin.Name = "checkAllClin";
			this.checkAllClin.Size = new System.Drawing.Size(134, 20);
			this.checkAllClin.TabIndex = 51;
			this.checkAllClin.Text = "All (Includes hidden)";
			this.checkAllClin.Click += new System.EventHandler(this.checkAllClin_Click);
			// 
			// listClin
			// 
			this.listClin.Location = new System.Drawing.Point(262, 255);
			this.listClin.Name = "listClin";
			this.listClin.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listClin.Size = new System.Drawing.Size(154, 147);
			this.listClin.TabIndex = 50;
			this.listClin.Click += new System.EventHandler(this.listClin_Click);
			// 
			// labelClin
			// 
			this.labelClin.Location = new System.Drawing.Point(262, 210);
			this.labelClin.Name = "labelClin";
			this.labelClin.Size = new System.Drawing.Size(104, 17);
			this.labelClin.TabIndex = 49;
			this.labelClin.Text = "Clinics";
			this.labelClin.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// checkAllAdjs
			// 
			this.checkAllAdjs.Location = new System.Drawing.Point(29, 231);
			this.checkAllAdjs.Name = "checkAllAdjs";
			this.checkAllAdjs.Size = new System.Drawing.Size(36, 20);
			this.checkAllAdjs.TabIndex = 52;
			this.checkAllAdjs.Text = "All";
			this.checkAllAdjs.Click += new System.EventHandler(this.checkAllAdjs_Click);
			// 
			// FormRpAdjSheet
			// 
			this.ClientSize = new System.Drawing.Size(723, 448);
			this.Controls.Add(this.checkAllAdjs);
			this.Controls.Add(this.checkAllClin);
			this.Controls.Add(this.listClin);
			this.Controls.Add(this.labelClin);
			this.Controls.Add(this.checkAllProv);
			this.Controls.Add(this.listType);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.listProv);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.date2);
			this.Controls.Add(this.date1);
			this.Controls.Add(this.labelTO);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormRpAdjSheet";
			this.ShowInTaskbar = false;
			this.Text = "Daily Adjustment Report";
			this.Load += new System.EventHandler(this.FormDailyAdjustment_Load);
			this.ResumeLayout(false);

		}
		#endregion
		private OpenPmsDental.UI.Button butOK;
		private OpenPmsDental.UI.MonthCalendarOD date2;
		private OpenPmsDental.UI.MonthCalendarOD date1;
		private System.Windows.Forms.Label labelTO;
		private OpenPmsDental.UI.ListBox listProv;
		private Label label1;
		private OpenPmsDental.UI.ListBox listType;
		private Label label2;
		private OpenPmsDental.UI.CheckBox checkAllProv;
		private OpenPmsDental.UI.CheckBox checkAllClin;
		private OpenPmsDental.UI.ListBox listClin;
		private Label labelClin;
		private OpenPmsDental.UI.CheckBox checkAllAdjs;
	}
}
