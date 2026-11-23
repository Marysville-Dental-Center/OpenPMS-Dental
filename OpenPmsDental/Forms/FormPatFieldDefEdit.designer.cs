using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormPatFieldDefEdit {
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
			this.butSave = new OpenPmsDental.UI.Button();
			this.textName = new System.Windows.Forms.TextBox();
			this.buttonDelete = new OpenPmsDental.UI.Button();
			this.labelStatus = new System.Windows.Forms.Label();
			this.comboFieldType = new OpenPmsDental.UI.ComboBox();
			this.labelFieldType = new System.Windows.Forms.Label();
			this.checkHidden = new OpenPmsDental.UI.CheckBox();
			this.gridPickListItems = new OpenPmsDental.UI.GridOD();
			this.butDown = new OpenPmsDental.UI.Button();
			this.butUp = new OpenPmsDental.UI.Button();
			this.butAdd = new OpenPmsDental.UI.Button();
			this.butMerge = new OpenPmsDental.UI.Button();
			this.SuspendLayout();
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(364, 412);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 1;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// textName
			// 
			this.textName.Location = new System.Drawing.Point(9, 27);
			this.textName.Name = "textName";
			this.textName.Size = new System.Drawing.Size(336, 20);
			this.textName.TabIndex = 0;
			// 
			// buttonDelete
			// 
			this.buttonDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.buttonDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonDelete.Location = new System.Drawing.Point(8, 412);
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.Size = new System.Drawing.Size(82, 24);
			this.buttonDelete.TabIndex = 3;
			this.buttonDelete.Text = "&Delete";
			this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
			// 
			// labelStatus
			// 
			this.labelStatus.Location = new System.Drawing.Point(9, 8);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(143, 17);
			this.labelStatus.TabIndex = 81;
			this.labelStatus.Text = "Field Name";
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// comboFieldType
			// 
			this.comboFieldType.BackColor = System.Drawing.SystemColors.Window;
			this.comboFieldType.Location = new System.Drawing.Point(9, 73);
			this.comboFieldType.Name = "comboFieldType";
			this.comboFieldType.Size = new System.Drawing.Size(184, 23);
			this.comboFieldType.TabIndex = 82;
			this.comboFieldType.SelectedIndexChanged += new System.EventHandler(this.comboFieldType_SelectedIndexChanged);
			// 
			// labelFieldType
			// 
			this.labelFieldType.Location = new System.Drawing.Point(9, 54);
			this.labelFieldType.Name = "labelFieldType";
			this.labelFieldType.Size = new System.Drawing.Size(336, 17);
			this.labelFieldType.TabIndex = 83;
			this.labelFieldType.Text = "Field Type";
			this.labelFieldType.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// checkHidden
			// 
			this.checkHidden.CheckAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.checkHidden.Location = new System.Drawing.Point(199, 74);
			this.checkHidden.Name = "checkHidden";
			this.checkHidden.Size = new System.Drawing.Size(62, 20);
			this.checkHidden.TabIndex = 86;
			this.checkHidden.Text = "Hidden";
			// 
			// gridPickListItems
			// 
			this.gridPickListItems.AllowSortingByColumn = true;
			this.gridPickListItems.Location = new System.Drawing.Point(9, 100);
			this.gridPickListItems.Name = "gridPickListItems";
			this.gridPickListItems.SelectionMode = OpenPmsDental.UI.GridSelectionMode.MultiExtended;
			this.gridPickListItems.Size = new System.Drawing.Size(336, 300);
			this.gridPickListItems.TabIndex = 87;
			this.gridPickListItems.Title = "Pick List Items";
			this.gridPickListItems.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridPickListItems_CellDoubleClick);
			// 
			// butDown
			// 
			this.butDown.Icon = OpenPmsDental.UI.EnumIcons.Down;
			this.butDown.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDown.Location = new System.Drawing.Point(360, 198);
			this.butDown.Name = "butDown";
			this.butDown.Size = new System.Drawing.Size(75, 24);
			this.butDown.TabIndex = 164;
			this.butDown.Text = "&Down";
			this.butDown.Click += new System.EventHandler(this.butDown_Click);
			// 
			// butUp
			// 
			this.butUp.Icon = OpenPmsDental.UI.EnumIcons.Up;
			this.butUp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butUp.Location = new System.Drawing.Point(360, 170);
			this.butUp.Name = "butUp";
			this.butUp.Size = new System.Drawing.Size(75, 24);
			this.butUp.TabIndex = 163;
			this.butUp.Text = "&Up";
			this.butUp.Click += new System.EventHandler(this.butUp_Click);
			// 
			// butAdd
			// 
			this.butAdd.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(360, 140);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(75, 26);
			this.butAdd.TabIndex = 167;
			this.butAdd.Text = "&Add";
			this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
			// 
			// butMerge
			// 
			this.butMerge.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butMerge.Location = new System.Drawing.Point(360, 226);
			this.butMerge.Name = "butMerge";
			this.butMerge.Size = new System.Drawing.Size(75, 24);
			this.butMerge.TabIndex = 169;
			this.butMerge.Text = "&Merge";
			this.butMerge.Click += new System.EventHandler(this.butMerge_Click);
			// 
			// FormPatFieldDefEdit
			// 
			this.ClientSize = new System.Drawing.Size(447, 444);
			this.Controls.Add(this.butMerge);
			this.Controls.Add(this.butAdd);
			this.Controls.Add(this.butDown);
			this.Controls.Add(this.butUp);
			this.Controls.Add(this.gridPickListItems);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.checkHidden);
			this.Controls.Add(this.labelFieldType);
			this.Controls.Add(this.comboFieldType);
			this.Controls.Add(this.labelStatus);
			this.Controls.Add(this.buttonDelete);
			this.Controls.Add(this.textName);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormPatFieldDefEdit";
			this.ShowInTaskbar = false;
			this.Text = "Edit Patient Field Def";
			this.Load += new System.EventHandler(this.FormPatFieldDefEdit_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.TextBox textName;
		private OpenPmsDental.UI.Button buttonDelete;
		private Label labelStatus;
		private UI.ComboBox comboFieldType;
		private Label labelFieldType;
		private OpenPmsDental.UI.CheckBox checkHidden;
		private UI.GridOD gridPickListItems;
		private UI.Button butDown;
		private UI.Button butUp;
		private UI.Button butAdd;
		private UI.Button butMerge;
	}
}
