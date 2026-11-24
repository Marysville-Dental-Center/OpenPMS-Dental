using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenPmsDental {
	public partial class FormProcButtonEdit {
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProcButtonEdit));
			this.listAutoCodes = new OpenPmsDental.UI.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textDescript = new System.Windows.Forms.TextBox();
			this.butSave = new OpenPmsDental.UI.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.listADA = new OpenPmsDental.UI.ListBox();
			this.label4 = new System.Windows.Forms.Label();
			this.butAdd = new OpenPmsDental.UI.Button();
			this.butDelete = new OpenPmsDental.UI.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.comboCategory = new OpenPmsDental.UI.ComboBox();
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.label6 = new System.Windows.Forms.Label();
			this.butImport = new OpenPmsDental.UI.Button();
			this.butClear = new OpenPmsDental.UI.Button();
			this.listView = new System.Windows.Forms.ListView();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.label7 = new System.Windows.Forms.Label();
			this.checkMultiVisit = new OpenPmsDental.UI.CheckBox();
			this.butDown = new OpenPmsDental.UI.Button();
			this.butUp = new OpenPmsDental.UI.Button();
			this.label8 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// listAutoCodes
			// 
			this.listAutoCodes.Location = new System.Drawing.Point(217, 166);
			this.listAutoCodes.Name = "listAutoCodes";
			this.listAutoCodes.SelectionMode = OpenPmsDental.UI.SelectionMode.MultiExtended;
			this.listAutoCodes.Size = new System.Drawing.Size(158, 339);
			this.listAutoCodes.TabIndex = 1;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 23);
			this.label1.TabIndex = 25;
			this.label1.Text = "Description";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDescript
			// 
			this.textDescript.Location = new System.Drawing.Point(86, 8);
			this.textDescript.Name = "textDescript";
			this.textDescript.Size = new System.Drawing.Size(316, 23);
			this.textDescript.TabIndex = 24;
			// 
			// butSave
			// 
			this.butSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSave.Location = new System.Drawing.Point(704, 517);
			this.butSave.Name = "butSave";
			this.butSave.Size = new System.Drawing.Size(75, 24);
			this.butSave.TabIndex = 26;
			this.butSave.Text = "&Save";
			this.butSave.Click += new System.EventHandler(this.butSave_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 147);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(160, 17);
			this.label2.TabIndex = 29;
			this.label2.Text = "Add Procedure Codes";
			this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(217, 147);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(158, 17);
			this.label3.TabIndex = 31;
			this.label3.Text = "Highlight Auto Codes";
			this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// listADA
			// 
			this.listADA.Location = new System.Drawing.Point(8, 166);
			this.listADA.Name = "listADA";
			this.listADA.Size = new System.Drawing.Size(160, 339);
			this.listADA.TabIndex = 32;
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label4.Location = new System.Drawing.Point(8, 126);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(367, 17);
			this.label4.TabIndex = 35;
			this.label4.Text = "Add any number of procedure codes and Auto Codes";
			this.label4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// butAdd
			// 
			this.butAdd.Icon = OpenPmsDental.UI.EnumIcons.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(8, 517);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(75, 24);
			this.butAdd.TabIndex = 36;
			this.butAdd.Text = "&Add";
			this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
			// 
			// butDelete
			// 
			this.butDelete.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(89, 517);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(75, 24);
			this.butDelete.TabIndex = 37;
			this.butDelete.Text = "&Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(8, 35);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(72, 23);
			this.label5.TabIndex = 38;
			this.label5.Text = "Category";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboCategory
			// 
			this.comboCategory.Location = new System.Drawing.Point(86, 35);
			this.comboCategory.Name = "comboCategory";
			this.comboCategory.Size = new System.Drawing.Size(215, 23);
			this.comboCategory.TabIndex = 39;
			// 
			// pictureBox
			// 
			this.pictureBox.BackColor = System.Drawing.SystemColors.Window;
			this.pictureBox.Location = new System.Drawing.Point(577, 36);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(20, 20);
			this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox.TabIndex = 40;
			this.pictureBox.TabStop = false;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(487, 35);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(84, 23);
			this.label6.TabIndex = 41;
			this.label6.Text = "Image (20x20)";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// butImport
			// 
			this.butImport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butImport.Location = new System.Drawing.Point(684, 35);
			this.butImport.Name = "butImport";
			this.butImport.Size = new System.Drawing.Size(75, 23);
			this.butImport.TabIndex = 42;
			this.butImport.Text = "Import";
			this.butImport.Click += new System.EventHandler(this.butImport_Click);
			// 
			// butClear
			// 
			this.butClear.Icon = OpenPmsDental.UI.EnumIcons.DeleteX;
			this.butClear.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butClear.Location = new System.Drawing.Point(603, 35);
			this.butClear.Name = "butClear";
			this.butClear.Size = new System.Drawing.Size(75, 23);
			this.butClear.TabIndex = 43;
			this.butClear.Text = "Clear";
			this.butClear.Click += new System.EventHandler(this.butClear_Click);
			// 
			// listView
			// 
			this.listView.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.listView.HideSelection = false;
			this.listView.Location = new System.Drawing.Point(487, 81);
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(292, 141);
			this.listView.SmallImageList = this.imageList;
			this.listView.TabIndex = 44;
			this.listView.UseCompatibleStateImageBehavior = false;
			this.listView.View = System.Windows.Forms.View.SmallIcon;
			this.listView.ItemActivate += new System.EventHandler(this.listView_ItemActivate);
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "procButtonAmalgam.gif");
			this.imageList.Images.SetKeyName(1, "procButtonComp.gif");
			this.imageList.Images.SetKeyName(2, "procButtonCrown.gif");
			this.imageList.Images.SetKeyName(3, "procButtonExtract.gif");
			this.imageList.Images.SetKeyName(4, "procButtonPA.gif");
			this.imageList.Images.SetKeyName(5, "procButtonRCT.gif");
			this.imageList.Images.SetKeyName(6, "procButtonRCTbuPFM.gif");
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(487, 62);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(292, 17);
			this.label7.TabIndex = 45;
			this.label7.Text = "Or pick an image from this list";
			this.label7.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// checkMultiVisit
			// 
			this.checkMultiVisit.Location = new System.Drawing.Point(86, 62);
			this.checkMultiVisit.Name = "checkMultiVisit";
			this.checkMultiVisit.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.checkMultiVisit.Size = new System.Drawing.Size(153, 20);
			this.checkMultiVisit.TabIndex = 46;
			this.checkMultiVisit.Text = "Group for multiple visits";
			// 
			// butDown
			// 
			this.butDown.Icon = OpenPmsDental.UI.EnumIcons.Down;
			this.butDown.Location = new System.Drawing.Point(176, 339);
			this.butDown.Name = "butDown";
			this.butDown.Size = new System.Drawing.Size(33, 26);
			this.butDown.TabIndex = 49;
			this.butDown.Click += new System.EventHandler(this.butDown_Click);
			// 
			// butUp
			// 
			this.butUp.Icon = OpenPmsDental.UI.EnumIcons.Up;
			this.butUp.Location = new System.Drawing.Point(176, 309);
			this.butUp.Name = "butUp";
			this.butUp.Size = new System.Drawing.Size(33, 26);
			this.butUp.TabIndex = 50;
			this.butUp.Click += new System.EventHandler(this.butUp_Click);
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(86, 84);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(316, 34);
			this.label8.TabIndex = 51;
			this.label8.Text = "(completed procs in the group will show as \"In Process\" until all complete)";
			this.label8.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// FormProcButtonEdit
			// 
			this.AcceptButton = this.butSave;
			this.ClientSize = new System.Drawing.Size(787, 549);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.butDown);
			this.Controls.Add(this.butUp);
			this.Controls.Add(this.checkMultiVisit);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.listView);
			this.Controls.Add(this.butClear);
			this.Controls.Add(this.butImport);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.pictureBox);
			this.Controls.Add(this.comboCategory);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.butAdd);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.listADA);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.butSave);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textDescript);
			this.Controls.Add(this.listAutoCodes);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormProcButtonEdit";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Edit Procedure Button";
			this.Load += new System.EventHandler(this.FormChartProcedureEntryEdit_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private OpenPmsDental.UI.ListBox listAutoCodes;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textDescript;
		private OpenPmsDental.UI.Button butSave;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private OpenPmsDental.UI.ListBox listADA;
		private OpenPmsDental.UI.Button butAdd;
		private OpenPmsDental.UI.Button butDelete;
		private System.Windows.Forms.Label label4;
		private Label label5;
		private OpenPmsDental.UI.ComboBox comboCategory;
		private PictureBox pictureBox;
		private Label label6;
		private OpenPmsDental.UI.Button butImport;
		private OpenPmsDental.UI.Button butClear;
		private ListView listView;
		private Label label7;
		private ImageList imageList;
		private OpenPmsDental.UI.CheckBox checkMultiVisit;
		private UI.Button butDown;
		private UI.Button butUp;
		private Label label8;
	}
}
