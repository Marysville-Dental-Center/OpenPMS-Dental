using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormProcApptColorEdit {
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
			this.label1 = new System.Windows.Forms.Label();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.textCodeRange = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.panelColor = new System.Windows.Forms.Panel();
			this.labelBeforeTime = new System.Windows.Forms.Label();
			this.checkPrevDate = new OpenPmsDental.UI.CheckBox();
			this.butChange = new OpenPmsDental.UI.Button();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.butSave = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(54, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(76, 23);
			this.label1.TabIndex = 2;
			this.label1.Text = "Code Range";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textCodeRange
			// 
			this.textCodeRange.AutoSize = false;
			this.textCodeRange.Location = new System.Drawing.Point(136, 8);
			this.textCodeRange.Name = "textCodeRange";
			this.textCodeRange.Size = new System.Drawing.Size(200, 23);
			this.textCodeRange.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = false;
			this.label2.Location = new System.Drawing.Point(136, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(101, 20);
			this.label2.TabIndex = 16;
			this.label2.Text = "Ex: D1050-D1060";
			// 
			// panelColor
			// 
			this.panelColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.panelColor.Location = new System.Drawing.Point(136, 57);
			this.panelColor.Name = "panelColor";
			this.panelColor.Size = new System.Drawing.Size(24, 24);
			this.panelColor.TabIndex = 127;
			// 
			// labelBeforeTime
			// 
			this.labelBeforeTime.Location = new System.Drawing.Point(66, 57);
			this.labelBeforeTime.Name = "labelBeforeTime";
			this.labelBeforeTime.Size = new System.Drawing.Size(64, 23);
			this.labelBeforeTime.TabIndex = 126;
			this.labelBeforeTime.Text = "Text Color";
			this.labelBeforeTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkPrevDate
			// 
			this.checkPrevDate.AutoSize = false;
			this.checkPrevDate.Location = new System.Drawing.Point(136, 85);
			this.checkPrevDate.Name = "checkPrevDate";
			this.checkPrevDate.Size = new System.Drawing.Size(130, 20);
			this.checkPrevDate.TabIndex = 3;
			this.checkPrevDate.Text = "Show previous date";
			// 
			// butChange
			// 
			this.butChange.Location = new System.Drawing.Point(166, 57);
			this.butChange.Name = "butChange";
			this.butChange.Size = new System.Drawing.Size(75, 23);
			this.butChange.TabIndex = 2;
			this.butChange.Text = "Change";
			this.butChange.Click += new System.EventHandler(this.butChange_Click);
			// 
			// butDelete
			// 
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(6, 127);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(92, 24);
			this.butDelete.TabIndex = 6;
			this.butDelete.Text = "&Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(278, 127);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 4;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// FormProcApptColorEdit
			// 
			this.ClientSize = new System.Drawing.Size(361, 159);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.checkPrevDate);
			this.Controls.Add(this.butChange);
			this.Controls.Add(this.panelColor);
			this.Controls.Add(this.labelBeforeTime);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textCodeRange);
			this.Controls.Add(this.label1);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormProcApptColorEdit";
			this.Padding = new System.Windows.Forms.Padding(3);
			this.ShowInTaskbar = false;
			this.Text = "Edit Proc Appt Color";
			this.Load += new System.EventHandler(this.FormProcApptColorEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label1;
		private ColorDialog colorDialog1;
		private TextBox textCodeRange;
		private Label label2;
		private OpenPmsDental.UI.Button butDelete;
		private OpenPmsDental.UI.Button butChange;
		private Panel panelColor;
		private Label labelBeforeTime;
		private OpenPmsDental.UI.CheckBox checkPrevDate;
	}
}
