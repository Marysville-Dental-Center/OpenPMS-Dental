using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormTaskListEdit {
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
			this.components = new System.ComponentModel.Container();
			this.butSave = new OpenPmsDental.UI.Button();
			this.labelDescription = new System.Windows.Forms.Label();
			this.labelGlobalFilter = new System.Windows.Forms.Label();
			this.comboGlobalFilter = new OpenPmsDental.UI.ComboBox();
			this.textDescript = new System.Windows.Forms.TextBox();
			this.labelDate = new System.Windows.Forms.Label();
			this.labelDate2 = new System.Windows.Forms.Label();
			this.labelDateType = new System.Windows.Forms.Label();
			this.listDateType = new OpenPmsDental.UI.ListBox();
			this.checkFromNum = new OpenPmsDental.UI.CheckBox();
			this.textDateTL = new OpenPmsDental.ValidDate();
			this.listObjectType = new OpenPmsDental.UI.ListBox();
			this.labelObjectType = new System.Windows.Forms.Label();
			this.textTaskListNum = new System.Windows.Forms.TextBox();
			this.labelTaskListNum = new System.Windows.Forms.Label();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(379, 240);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 26);
			this.butSave.TabIndex = 4;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// labelDescription
			// 
			this.labelDescription.Location = new System.Drawing.Point(49, 8);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(72, 23);
			this.labelDescription.TabIndex = 2;
			this.labelDescription.Text = "Description";
			this.labelDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelGlobalFilter
			// 
			this.labelGlobalFilter.Location = new System.Drawing.Point(81, 82);
			this.labelGlobalFilter.Name = "labelGlobalFilter";
			this.labelGlobalFilter.Size = new System.Drawing.Size(40, 23);
			this.labelGlobalFilter.TabIndex = 138;
			this.labelGlobalFilter.Text = "Filter";
			this.labelGlobalFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboGlobalFilter
			// 
			this.comboGlobalFilter.Location = new System.Drawing.Point(127, 82);
			this.comboGlobalFilter.Name = "comboGlobalFilter";
			this.comboGlobalFilter.Size = new System.Drawing.Size(120, 23);
			this.comboGlobalFilter.TabIndex = 137;
			this.comboGlobalFilter.SelectionChangeCommitted += new System.EventHandler(this.comboGlobalFilter_SelectionChangeCommitted);
			// 
			// textDescript
			// 
			this.textDescript.Location = new System.Drawing.Point(127, 8);
			this.textDescript.Name = "textDescript";
			this.textDescript.Size = new System.Drawing.Size(292, 23);
			this.textDescript.TabIndex = 0;
			// 
			// labelDate
			// 
			this.labelDate.Location = new System.Drawing.Point(85, 109);
			this.labelDate.Name = "labelDate";
			this.labelDate.Size = new System.Drawing.Size(36, 23);
			this.labelDate.TabIndex = 4;
			this.labelDate.Text = "Date";
			this.labelDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labelDate2
			// 
			this.labelDate2.Location = new System.Drawing.Point(221, 109);
			this.labelDate2.Name = "labelDate2";
			this.labelDate2.Size = new System.Drawing.Size(185, 32);
			this.labelDate2.TabIndex = 6;
			this.labelDate2.Text = "Leave blank unless you want this list to show on a dated list";
			// 
			// labelDateType
			// 
			this.labelDateType.Location = new System.Drawing.Point(57, 145);
			this.labelDateType.Name = "labelDateType";
			this.labelDateType.Size = new System.Drawing.Size(64, 23);
			this.labelDateType.TabIndex = 7;
			this.labelDateType.Text = "Date Type";
			this.labelDateType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// listDateType
			// 
			this.listDateType.Location = new System.Drawing.Point(127, 145);
			this.listDateType.Name = "listDateType";
			this.listDateType.Size = new System.Drawing.Size(120, 56);
			this.listDateType.TabIndex = 2;
			// 
			// checkFromNum
			// 
			this.checkFromNum.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkFromNum.Location = new System.Drawing.Point(127, 205);
			this.checkFromNum.Name = "checkFromNum";
			this.checkFromNum.Size = new System.Drawing.Size(121, 20);
			this.checkFromNum.TabIndex = 3;
			this.checkFromNum.Text = "Is From Repeating";
			// 
			// textDateTL
			// 
			this.textDateTL.Location = new System.Drawing.Point(127, 109);
			this.textDateTL.Name = "textDateTL";
			this.textDateTL.Size = new System.Drawing.Size(88, 23);
			this.textDateTL.TabIndex = 1;
			// 
			// listObjectType
			// 
			this.listObjectType.Location = new System.Drawing.Point(127, 35);
			this.listObjectType.Name = "listObjectType";
			this.listObjectType.Size = new System.Drawing.Size(120, 43);
			this.listObjectType.TabIndex = 15;
			// 
			// labelObjectType
			// 
			this.labelObjectType.Location = new System.Drawing.Point(45, 35);
			this.labelObjectType.Name = "labelObjectType";
			this.labelObjectType.Size = new System.Drawing.Size(76, 23);
			this.labelObjectType.TabIndex = 16;
			this.labelObjectType.Text = "Object Type";
			this.labelObjectType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textTaskListNum
			// 
			this.textTaskListNum.Location = new System.Drawing.Point(366, 82);
			this.textTaskListNum.Name = "textTaskListNum";
			this.textTaskListNum.ReadOnly = true;
			this.textTaskListNum.Size = new System.Drawing.Size(56, 23);
			this.textTaskListNum.TabIndex = 136;
			this.textTaskListNum.Visible = false;
			// 
			// labelTaskListNum
			// 
			this.labelTaskListNum.Location = new System.Drawing.Point(280, 82);
			this.labelTaskListNum.Name = "labelTaskListNum";
			this.labelTaskListNum.Size = new System.Drawing.Size(80, 23);
			this.labelTaskListNum.TabIndex = 135;
			this.labelTaskListNum.Text = "TaskListNum";
			this.labelTaskListNum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelTaskListNum.Visible = false;
			// 
			// errorProvider1
			// 
			this.errorProvider1.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			this.errorProvider1.ContainerControl = this;
			// 
			// FormTaskListEdit
			// 
			this.ClientSize = new System.Drawing.Size(462, 274);
			this.Controls.Add(this.comboGlobalFilter);
			this.Controls.Add(this.labelGlobalFilter);
			this.Controls.Add(this.textTaskListNum);
			this.Controls.Add(this.labelTaskListNum);
			this.Controls.Add(this.listObjectType);
			this.Controls.Add(this.labelObjectType);
			this.Controls.Add(this.textDateTL);
			this.Controls.Add(this.textDescript);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.checkFromNum);
			this.Controls.Add(this.listDateType);
			this.Controls.Add(this.labelDateType);
			this.Controls.Add(this.labelDate2);
			this.Controls.Add(this.labelDate);
			this.Controls.Add(this.labelDescription);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormTaskListEdit";
			this.ShowInTaskbar = false;
			this.Text = "Task List";
			this.Load += new System.EventHandler(this.FormTaskListEdit_Load);
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label labelDescription;
		private System.Windows.Forms.Label labelDate;
		private System.Windows.Forms.Label labelDate2;
		private System.Windows.Forms.Label labelDateType;
		private OpenPmsDental.UI.ListBox listDateType;
		private System.Windows.Forms.TextBox textDescript;
		private OpenPmsDental.ValidDate textDateTL;
		private OpenPmsDental.UI.CheckBox checkFromNum;
		private OpenPmsDental.UI.ListBox listObjectType;
		private System.Windows.Forms.Label labelObjectType;
		private TextBox textTaskListNum;
		private Label labelTaskListNum;
		private UI.ComboBox comboGlobalFilter;
		private Label labelGlobalFilter;
		private ErrorProvider errorProvider1;
	}
}
