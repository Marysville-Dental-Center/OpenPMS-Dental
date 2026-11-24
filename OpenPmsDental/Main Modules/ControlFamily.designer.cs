using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental.Bridges;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	partial class ControlFamily {

		private IContainer components=null;
		#region Dispose
		///<summary></summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if(components != null){
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		#endregion Dispose

		#region Component Designer generated code
		private void InitializeComponent(){
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlFamily));
			this.contextMenuInsurance = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemPlansForFam = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuDiscount = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemRemoveDiscount = new System.Windows.Forms.ToolStripMenuItem();
			this.gridSuperFam = new OpenPmsDental.UI.GridOD();
			this.gridRecall = new OpenPmsDental.UI.GridOD();
			this.gridFamily = new OpenPmsDental.UI.GridOD();
			this.gridPat = new OpenPmsDental.UI.GridOD();
			this.gridIns = new OpenPmsDental.UI.GridOD();
			this.gridPatientClones = new OpenPmsDental.UI.GridOD();
			this.pictureBoxPat = new OpenPmsDental.UI.ODPictureBox();
			this.toolBarMain = new OpenPmsDental.UI.ToolBarOD();
			this.splitContainerSuperClones = new OpenPmsDental.UI.SplitContainer();
			this.splitterPanel1 = new OpenPmsDental.UI.SplitterPanel();
			this.splitterPanel2 = new OpenPmsDental.UI.SplitterPanel();
			this.splitContainerSuperClones.SuspendLayout();
			this.splitterPanel1.SuspendLayout();
			this.splitterPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenuInsurance
			// 
			this.contextMenuInsurance.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemPlansForFam});
			// 
			// menuItemPlansForFam
			// 
			this.menuItemPlansForFam.Text = "Plans for Family";
			this.menuItemPlansForFam.Click += new System.EventHandler(this.menuPlansForFam_Click);
			// 
			// contextMenuDiscount
			// 
			this.contextMenuDiscount.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemRemoveDiscount});
			// 
			// menuItemRemoveDiscount
			// 
			this.menuItemRemoveDiscount.Text = "Drop Discount Plan";
			this.menuItemRemoveDiscount.Click += new System.EventHandler(this.menuItemRemoveDiscount_Click);
			// 
			// gridSuperFam
			// 
			this.gridSuperFam.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridSuperFam.Location = new System.Drawing.Point(0, 0);
			this.gridSuperFam.Name = "gridSuperFam";
			this.gridSuperFam.Size = new System.Drawing.Size(329, 285);
			this.gridSuperFam.TabIndex = 33;
			this.gridSuperFam.Title = "Super Family";
			this.gridSuperFam.TranslationName = "TableSuper";
			this.gridSuperFam.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridSuperFam_CellDoubleClick);
			this.gridSuperFam.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridSuperFam_CellClick);
			// 
			// gridRecall
			// 
			this.gridRecall.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridRecall.Location = new System.Drawing.Point(588, 29);
			this.gridRecall.Name = "gridRecall";
			this.gridRecall.Size = new System.Drawing.Size(351, 100);
			this.gridRecall.TabIndex = 32;
			this.gridRecall.Title = "Recall";
			this.gridRecall.TranslationName = "TableRecall";
			this.gridRecall.WrapText = false;
			this.gridRecall.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridRecall_CellDoubleClick);
			this.gridRecall.DoubleClick += new System.EventHandler(this.gridRecall_DoubleClick);
			// 
			// gridFamily
			// 
			this.gridFamily.ColorSelectedRow = System.Drawing.Color.DarkSalmon;
			this.gridFamily.Location = new System.Drawing.Point(104, 29);
			this.gridFamily.Name = "gridFamily";
			this.gridFamily.Size = new System.Drawing.Size(480, 100);
			this.gridFamily.TabIndex = 31;
			this.gridFamily.Title = "Family Members";
			this.gridFamily.TranslationName = "TableFamily";
			this.gridFamily.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridFamily_CellDoubleClick);
			this.gridFamily.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridFamily_CellClick);
			// 
			// gridPat
			// 
			this.gridPat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.gridPat.Location = new System.Drawing.Point(0, 133);
			this.gridPat.Name = "gridPat";
			this.gridPat.SelectionMode = OpenPmsDental.UI.GridSelectionMode.None;
			this.gridPat.Size = new System.Drawing.Size(252, 575);
			this.gridPat.TabIndex = 30;
			this.gridPat.Title = "Patient Information";
			this.gridPat.TranslationName = "TablePatient";
			this.gridPat.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridPat_CellDoubleClick);
			this.gridPat.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridPat_CellClick);
			this.gridPat.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gridPat_MouseDown);
			// 
			// gridIns
			// 
			this.gridIns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridIns.HScrollVisible = true;
			this.gridIns.Location = new System.Drawing.Point(256, 133);
			this.gridIns.Name = "gridIns";
			this.gridIns.SelectionMode = OpenPmsDental.UI.GridSelectionMode.None;
			this.gridIns.Size = new System.Drawing.Size(683, 575);
			this.gridIns.TabIndex = 29;
			this.gridIns.Title = "Insurance Plans";
			this.gridIns.TranslationName = "TableCoverage";
			this.gridIns.CellDoubleClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridIns_CellDoubleClick);
			// 
			// gridPatientClones
			// 
			this.gridPatientClones.Location = new System.Drawing.Point(0, 0);
			this.gridPatientClones.Name = "gridPatientClones";
			this.gridPatientClones.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridPatientClones.TabIndex = 34;
			this.gridPatientClones.Title = "Patient Clones";
			this.gridPatientClones.TranslationName = "TablePatientClones";
			this.gridPatientClones.CellClick += new OpenPmsDental.UI.ODGridClickEventHandler(this.gridPatientClone_CellClick);
			// 
			// pictureBoxPat
			// 
			this.pictureBoxPat.Location = new System.Drawing.Point(0, 29);
			this.pictureBoxPat.Name = "pictureBoxPat";
			this.pictureBoxPat.Size = new System.Drawing.Size(100, 100);
			this.pictureBoxPat.TabIndex = 28;
			this.pictureBoxPat.Text = "picturePat";
			this.pictureBoxPat.TextNullImage = "Patient Picture Unavailable";
			this.pictureBoxPat.DoubleClick += new System.EventHandler(this.pictureBoxPat_DoubleClick);
			// 
			// toolBarMain
			// 
			this.toolBarMain.Dock = System.Windows.Forms.DockStyle.Top;
			this.toolBarMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.toolBarMain.Location = new System.Drawing.Point(0, 0);
			this.toolBarMain.Name = "toolBarMain";
			this.toolBarMain.Size = new System.Drawing.Size(939, 25);
			this.toolBarMain.TabIndex = 19;
			this.toolBarMain.ButtonClick += new OpenPmsDental.UI.ODToolBarButtonClickEventHandler(this.ToolBarMain_ButtonClick);
			// 
			// splitContainerSuperClones
			// 
			this.splitContainerSuperClones.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.splitContainerSuperClones.Controls.Add(this.splitterPanel1);
			this.splitContainerSuperClones.Controls.Add(this.splitterPanel2);
			this.splitContainerSuperClones.Cursor = System.Windows.Forms.Cursors.Default;
			this.splitContainerSuperClones.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.splitContainerSuperClones.Location = new System.Drawing.Point(256, 133);
			this.splitContainerSuperClones.Name = "splitContainerSuperClones";
			this.splitContainerSuperClones.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.splitContainerSuperClones.Panel1 = this.splitterPanel1;
			this.splitContainerSuperClones.Panel1MinSize = 25;
			this.splitContainerSuperClones.Panel2 = this.splitterPanel2;
			this.splitContainerSuperClones.Panel2MinSize = 25;
			this.splitContainerSuperClones.Size = new System.Drawing.Size(329, 575);
			this.splitContainerSuperClones.SplitterDistance = 285;
			this.splitContainerSuperClones.TabIndex = 35;
			// 
			// splitterPanel1
			// 
			this.splitterPanel1.Controls.Add(this.gridSuperFam);
			this.splitterPanel1.Location = new System.Drawing.Point(0, 0);
			this.splitterPanel1.Name = "splitterPanel1";
			this.splitterPanel1.Size = new System.Drawing.Size(329, 285);
			this.splitterPanel1.TabIndex = 13;
			// 
			// splitterPanel2
			// 
			this.splitterPanel2.Controls.Add(this.gridPatientClones);
			this.splitterPanel2.Location = new System.Drawing.Point(0, 289);
			this.splitterPanel2.Name = "splitterPanel2";
			this.splitterPanel2.Size = new System.Drawing.Size(329, 290);
			this.splitterPanel2.TabIndex = 14;
			// 
			// ControlFamily
			// 
			this.Controls.Add(this.splitContainerSuperClones);
			this.Controls.Add(this.gridRecall);
			this.Controls.Add(this.gridFamily);
			this.Controls.Add(this.gridPat);
			this.Controls.Add(this.gridIns);
			this.Controls.Add(this.pictureBoxPat);
			this.Controls.Add(this.toolBarMain);
			this.Name = "ControlFamily";
			this.Size = new System.Drawing.Size(939, 708);
			this.splitContainerSuperClones.ResumeLayout(false);
			this.splitterPanel1.ResumeLayout(false);
			this.splitterPanel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private ContextMenuStrip contextMenuDiscount;
		private ContextMenuStrip contextMenuInsurance;
		private GridOD gridFamily;
		private GridOD gridIns;
		private GridOD gridPat;
		private GridOD gridPatientClones;
		private GridOD gridRecall;
		private GridOD gridSuperFam;
		private ToolStripMenuItem menuItemRemoveDiscount;
		private ToolStripMenuItem menuItemPlansForFam;
		private ODPictureBox pictureBoxPat;
		private UI.SplitContainer splitContainerSuperClones;
		private UI.SplitterPanel splitterPanel1;
		private UI.SplitterPanel splitterPanel2;
	}
}