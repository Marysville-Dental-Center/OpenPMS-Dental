using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormMedPat {
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
			this.butOK = new OpenPmsDental.UI.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.textMedName = new System.Windows.Forms.TextBox();
			this.textGenericName = new System.Windows.Forms.TextBox();
			this.labelGenericName = new System.Windows.Forms.Label();
			this.textMedNote = new System.Windows.Forms.TextBox();
			this.labelGenericNotes = new System.Windows.Forms.Label();
			this.labelPatNote = new System.Windows.Forms.Label();
			this.groupBox1 = new OpenPmsDental.UI.GroupBox();
			this.butRxNormSelect = new OpenPmsDental.UI.Button();
			this.textRxNormDesc = new System.Windows.Forms.TextBox();
			this.labelRxNorm = new System.Windows.Forms.Label();
			this.labelEdit = new System.Windows.Forms.Label();
			this.butEdit = new OpenPmsDental.UI.Button();
			this.butRemove = new OpenPmsDental.UI.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.textPatNote = new OpenPmsDental.ODtextBox();
			this.groupOrder = new OpenPmsDental.UI.GroupBox();
			this.label8 = new System.Windows.Forms.Label();
			this.comboProv = new OpenPmsDental.UI.ComboBox();
			this.butTodayStop = new OpenPmsDental.UI.Button();
			this.butTodayStart = new OpenPmsDental.UI.Button();
			this.textDateStop = new OpenPmsDental.ValidDate();
			this.label7 = new System.Windows.Forms.Label();
			this.textDateStart = new OpenPmsDental.ValidDate();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupOrder.SuspendLayout();
			this.SuspendLayout();
			// 
			// butOK
			// 
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Location = new System.Drawing.Point(533, 539);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 24);
			this.butOK.TabIndex = 1;
			this.butOK.Text = "&Save";
			this.butOK.Click += new System.EventHandler(this.butSave_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(105, 50);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 23);
			this.label1.TabIndex = 2;
			this.label1.Text = "Drug Name";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textMedName
			// 
			this.textMedName.AutoSize = false;
			this.textMedName.Location = new System.Drawing.Point(183, 50);
			this.textMedName.Name = "textMedName";
			this.textMedName.ReadOnly = true;
			this.textMedName.Size = new System.Drawing.Size(348, 23);
			this.textMedName.TabIndex = 3;
			// 
			// textGenericName
			// 
			this.textGenericName.AutoSize = false;
			this.textGenericName.Location = new System.Drawing.Point(183, 77);
			this.textGenericName.Name = "textGenericName";
			this.textGenericName.ReadOnly = true;
			this.textGenericName.Size = new System.Drawing.Size(348, 23);
			this.textGenericName.TabIndex = 5;
			// 
			// labelGenericName
			// 
			this.labelGenericName.Location = new System.Drawing.Point(89, 77);
			this.labelGenericName.Name = "labelGenericName";
			this.labelGenericName.Size = new System.Drawing.Size(88, 23);
			this.labelGenericName.TabIndex = 4;
			this.labelGenericName.Text = "Generic Name";
			this.labelGenericName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textMedNote
			// 
			this.textMedNote.Location = new System.Drawing.Point(183, 104);
			this.textMedNote.Multiline = true;
			this.textMedNote.Name = "textMedNote";
			this.textMedNote.ReadOnly = true;
			this.textMedNote.Size = new System.Drawing.Size(348, 87);
			this.textMedNote.TabIndex = 7;
			// 
			// labelGenericNotes
			// 
			this.labelGenericNotes.Location = new System.Drawing.Point(69, 104);
			this.labelGenericNotes.Name = "labelGenericNotes";
			this.labelGenericNotes.Size = new System.Drawing.Size(108, 23);
			this.labelGenericNotes.TabIndex = 6;
			this.labelGenericNotes.Text = "Medication Notes";
			this.labelGenericNotes.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelPatNote
			// 
			this.labelPatNote.Location = new System.Drawing.Point(53, 35);
			this.labelPatNote.Name = "labelPatNote";
			this.labelPatNote.Size = new System.Drawing.Size(124, 43);
			this.labelPatNote.TabIndex = 8;
			this.labelPatNote.Text = "Notes for this Patient";
			this.labelPatNote.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.butRxNormSelect);
			this.groupBox1.Controls.Add(this.textRxNormDesc);
			this.groupBox1.Controls.Add(this.labelRxNorm);
			this.groupBox1.Controls.Add(this.labelEdit);
			this.groupBox1.Controls.Add(this.butEdit);
			this.groupBox1.Controls.Add(this.textMedNote);
			this.groupBox1.Controls.Add(this.textMedName);
			this.groupBox1.Controls.Add(this.textGenericName);
			this.groupBox1.Controls.Add(this.labelGenericName);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.labelGenericNotes);
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(600, 235);
			this.groupBox1.TabIndex = 10;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Medication";
			// 
			// butRxNormSelect
			// 
			this.butRxNormSelect.Location = new System.Drawing.Point(537, 23);
			this.butRxNormSelect.Name = "butRxNormSelect";
			this.butRxNormSelect.Size = new System.Drawing.Size(22, 23);
			this.butRxNormSelect.TabIndex = 65;
			this.butRxNormSelect.Text = "...";
			this.butRxNormSelect.Visible = false;
			this.butRxNormSelect.Click += new System.EventHandler(this.butRxNormSelect_Click);
			// 
			// textRxNormDesc
			// 
			this.textRxNormDesc.AutoSize = false;
			this.textRxNormDesc.Location = new System.Drawing.Point(183, 23);
			this.textRxNormDesc.Name = "textRxNormDesc";
			this.textRxNormDesc.ReadOnly = true;
			this.textRxNormDesc.Size = new System.Drawing.Size(348, 23);
			this.textRxNormDesc.TabIndex = 12;
			// 
			// labelRxNorm
			// 
			this.labelRxNorm.Location = new System.Drawing.Point(121, 23);
			this.labelRxNorm.Name = "labelRxNorm";
			this.labelRxNorm.Size = new System.Drawing.Size(56, 23);
			this.labelRxNorm.TabIndex = 13;
			this.labelRxNorm.Text = "RxNorm";
			this.labelRxNorm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelEdit
			// 
			this.labelEdit.Location = new System.Drawing.Point(264, 199);
			this.labelEdit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.labelEdit.Name = "labelEdit";
			this.labelEdit.Size = new System.Drawing.Size(204, 24);
			this.labelEdit.TabIndex = 11;
			this.labelEdit.Text = "(edit this medication for all patients)";
			// 
			// butEdit
			// 
			this.butEdit.Location = new System.Drawing.Point(183, 199);
			this.butEdit.Name = "butEdit";
			this.butEdit.Size = new System.Drawing.Size(75, 24);
			this.butEdit.TabIndex = 9;
			this.butEdit.Text = "&Edit";
			this.butEdit.Click += new System.EventHandler(this.butEdit_Click);
			// 
			// butRemove
			// 
			this.butRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butRemove.Location = new System.Drawing.Point(8, 467);
			this.butRemove.Name = "butRemove";
			this.butRemove.Size = new System.Drawing.Size(75, 24);
			this.butRemove.TabIndex = 8;
			this.butRemove.Text = "&Remove";
			this.butRemove.Click += new System.EventHandler(this.butRemove_Click);
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.Location = new System.Drawing.Point(89, 462);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(150, 34);
			this.label5.TabIndex = 10;
			this.label5.Text = "(remove this medication\r\nfrom this patient)";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// textPatNote
			// 
			this.textPatNote.AcceptsTab = true;
			this.textPatNote.AllowsCarriageReturns = false;
			this.textPatNote.BackColor = System.Drawing.SystemColors.Window;
			this.textPatNote.DetectLinksEnabled = false;
			this.textPatNote.DetectUrls = false;
			this.textPatNote.Location = new System.Drawing.Point(183, 35);
			this.textPatNote.MaxLength = 500;
			this.textPatNote.Name = "textPatNote";
			this.textPatNote.QuickPasteType = OpenPmsDentalBusiness.EnumQuickPasteType.MedicationPat;
			this.textPatNote.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textPatNote.Size = new System.Drawing.Size(348, 111);
			this.textPatNote.TabIndex = 11;
			this.textPatNote.Text = "";
			// 
			// groupOrder
			// 
			this.groupOrder.Controls.Add(this.label8);
			this.groupOrder.Controls.Add(this.comboProv);
			this.groupOrder.Controls.Add(this.butTodayStop);
			this.groupOrder.Controls.Add(this.butTodayStart);
			this.groupOrder.Controls.Add(this.textDateStop);
			this.groupOrder.Controls.Add(this.label7);
			this.groupOrder.Controls.Add(this.textDateStart);
			this.groupOrder.Controls.Add(this.label4);
			this.groupOrder.Controls.Add(this.labelPatNote);
			this.groupOrder.Controls.Add(this.textPatNote);
			this.groupOrder.Location = new System.Drawing.Point(8, 251);
			this.groupOrder.Name = "groupOrder";
			this.groupOrder.Size = new System.Drawing.Size(600, 208);
			this.groupOrder.TabIndex = 64;
			this.groupOrder.TabStop = false;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(121, 8);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(56, 23);
			this.label8.TabIndex = 32;
			this.label8.Text = "Provider";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboProv
			// 
			this.comboProv.Location = new System.Drawing.Point(183, 8);
			this.comboProv.Name = "comboProv";
			this.comboProv.Size = new System.Drawing.Size(208, 23);
			this.comboProv.TabIndex = 31;
			// 
			// butTodayStop
			// 
			this.butTodayStop.Location = new System.Drawing.Point(289, 177);
			this.butTodayStop.Name = "butTodayStop";
			this.butTodayStop.Size = new System.Drawing.Size(64, 23);
			this.butTodayStop.TabIndex = 17;
			this.butTodayStop.Text = "Today";
			this.butTodayStop.Click += new System.EventHandler(this.butTodayStop_Click);
			// 
			// butTodayStart
			// 
			this.butTodayStart.Location = new System.Drawing.Point(289, 150);
			this.butTodayStart.Name = "butTodayStart";
			this.butTodayStart.Size = new System.Drawing.Size(64, 23);
			this.butTodayStart.TabIndex = 16;
			this.butTodayStart.Text = "Today";
			this.butTodayStart.Click += new System.EventHandler(this.butTodayStart_Click);
			// 
			// textDateStop
			// 
			this.textDateStop.Location = new System.Drawing.Point(183, 177);
			this.textDateStop.Name = "textDateStop";
			this.textDateStop.Size = new System.Drawing.Size(100, 20);
			this.textDateStop.TabIndex = 15;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(113, 177);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(64, 23);
			this.label7.TabIndex = 14;
			this.label7.Text = "Date Stop";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDateStart
			// 
			this.textDateStart.Location = new System.Drawing.Point(183, 150);
			this.textDateStart.Name = "textDateStart";
			this.textDateStart.Size = new System.Drawing.Size(100, 20);
			this.textDateStart.TabIndex = 13;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(113, 150);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(64, 23);
			this.label4.TabIndex = 12;
			this.label4.Text = "Date Start";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FormMedPat
			// 
			this.AcceptButton = this.butOK;
			this.ClientSize = new System.Drawing.Size(616, 571);
			this.Controls.Add(this.groupOrder);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.butRemove);
			this.Controls.Add(this.label5);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormMedPat";
			this.ShowInTaskbar = false;
			this.Text = "Medication for Patient";
			this.Load += new System.EventHandler(this.FormMedPat_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupOrder.ResumeLayout(false);
			this.groupOrder.PerformLayout();
			this.ResumeLayout(false);

		}
		#endregion
		private OpenPmsDental.UI.Button butOK;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelGenericName;
		private System.Windows.Forms.Label labelGenericNotes;
		private System.Windows.Forms.Label labelPatNote;
		private OpenPmsDental.UI.GroupBox groupBox1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label labelEdit;
		private System.Windows.Forms.TextBox textMedName;
		private System.Windows.Forms.TextBox textGenericName;
		private System.Windows.Forms.TextBox textMedNote;
		private OpenPmsDental.UI.Button butRemove;
		private OpenPmsDental.UI.Button butEdit;
		private OpenPmsDental.ODtextBox textPatNote;
		private OpenPmsDental.UI.GroupBox groupOrder;
		private ValidDate textDateStop;
		private Label label7;
		private ValidDate textDateStart;
		private Label label4;
		private UI.Button butTodayStop;
		private UI.Button butTodayStart;
		private Label label8;
		private OpenPmsDental.UI.ComboBox comboProv;
		private TextBox textRxNormDesc;
		private Label labelRxNorm;
		private UI.Button butRxNormSelect;
	}
}
