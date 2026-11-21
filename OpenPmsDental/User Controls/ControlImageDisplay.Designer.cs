namespace OpenPmsDental {
	partial class ControlImageDisplay {
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing) {
				components?.Dispose();
				_odWebView2Print?.Dispose();//S0-F: in no window, so not among the controls this disposes.
				_bitmapRaw?.Dispose();
				if(_bitmapArrayShowing!=null && _bitmapArrayShowing.Length>0){
					for(int i=0;i<_bitmapArrayShowing.Length;i++){
						_bitmapArrayShowing[i]?.Dispose();
						_bitmapArrayShowing[i]=null;
					}
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.menuPanelMain = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemCopyWithoutDrawings = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemFlipHoriz = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemRotateLeft = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemRotateRight = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemRotate180 = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemInfo = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemTasks = new System.Windows.Forms.ToolStripMenuItem();
			this.panelMain = new OpenPmsDental.UI.ControlDoubleBuffered();
			this.SuspendLayout();
			// 
			// menuPanelMain
			// 
			this.menuPanelMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemCopy,
            this.menuItemCopyWithoutDrawings,
            this.menuItemPaste,
            this.menuItemFlipHoriz,
            this.menuItemRotateLeft,
            this.menuItemRotateRight,
            this.menuItemRotate180,
            this.menuItemDelete,
            this.menuItemInfo,
            this.menuItemTasks});
			// 
			// menuItemCopy
			// 
			this.menuItemCopy.Text = "Copy";
			this.menuItemCopy.Click += new System.EventHandler(this.menuPanelMain_Click);
			// 
			// menuItemCopyWithoutDrawings
			// 
			this.menuItemCopyWithoutDrawings.Text = "Copy without drawings";
			this.menuItemCopyWithoutDrawings.Click += new System.EventHandler(this.menuPanelMain_Click);
			// 
			// menuItemPaste
			// 
			this.menuItemPaste.Text = "Paste";
			this.menuItemPaste.Click += new System.EventHandler(this.menuPanelMain_Click);
			// 
			// menuItemFlipHoriz
			// 
			this.menuItemFlipHoriz.Text = "Flip Horizontally";
			this.menuItemFlipHoriz.Click += new System.EventHandler(this.menuPanelMain_Click);
			// 
			// menuItemRotateLeft
			// 
			this.menuItemRotateLeft.Text = "Rotate Left";
			this.menuItemRotateLeft.Click += new System.EventHandler(this.menuPanelMain_Click);
			// 
			// menuItemRotateRight
			// 
			this.menuItemRotateRight.Text = "Rotate Right";
			this.menuItemRotateRight.Click += new System.EventHandler(this.menuPanelMain_Click);
			// 
			// menuItemRotate180
			// 
			this.menuItemRotate180.Text = "Rotate 180";
			this.menuItemRotate180.Click += new System.EventHandler(this.menuPanelMain_Click);
			// 
			// menuItemDelete
			// 
			this.menuItemDelete.Text = "Delete";
			this.menuItemDelete.Click += new System.EventHandler(this.menuPanelMain_Click);
			// 
			// menuItemInfo
			// 
			this.menuItemInfo.Text = "Info";
			this.menuItemInfo.Click += new System.EventHandler(this.menuPanelMain_Click);
			// 
			// menuItemTasks
			// 
			this.menuItemTasks.Text = "Find Tasks";
			this.menuItemTasks.Click += new System.EventHandler(this.menuPanelMain_Click);
			// 
			// panelMain
			// 
			this.panelMain.AllowDrop = true;
			this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelMain.Location = new System.Drawing.Point(0, 0);
			this.panelMain.Name = "panelMain";
			this.panelMain.Size = new System.Drawing.Size(150, 170);
			this.panelMain.TabIndex = 1;
			this.panelMain.DragDrop += new System.Windows.Forms.DragEventHandler(this.panelMain_DragDrop);
			this.panelMain.DragEnter += new System.Windows.Forms.DragEventHandler(this.panelMain_DragEnter);
			this.panelMain.Paint += new System.Windows.Forms.PaintEventHandler(this.panelMain_Paint);
			this.panelMain.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.panelMain_MouseDoubleClick);
			this.panelMain.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panelMain_MouseDown);
			this.panelMain.MouseLeave += new System.EventHandler(this.panelMain_MouseLeave);
			this.panelMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panelMain_MouseMove);
			this.panelMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panelMain_MouseUp);
			// 
			// ControlImageDisplay
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.panelMain);
			this.Name = "ControlImageDisplay";
			this.Size = new System.Drawing.Size(150, 170);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ControlImageDisplay_KeyDown);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ControlImageDisplay_KeyUp);
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.ContextMenuStrip menuPanelMain;
		private System.Windows.Forms.ToolStripMenuItem menuItemCopy;
		private System.Windows.Forms.ToolStripMenuItem menuItemCopyWithoutDrawings;
		private System.Windows.Forms.ToolStripMenuItem menuItemPaste;
		private System.Windows.Forms.ToolStripMenuItem menuItemFlipHoriz;
		private System.Windows.Forms.ToolStripMenuItem menuItemRotateLeft;
		private System.Windows.Forms.ToolStripMenuItem menuItemRotateRight;
		private System.Windows.Forms.ToolStripMenuItem menuItemRotate180;
		private System.Windows.Forms.ToolStripMenuItem menuItemDelete;
		private System.Windows.Forms.ToolStripMenuItem menuItemInfo;
		private System.Windows.Forms.ToolStripMenuItem menuItemTasks;
		public UI.ControlDoubleBuffered panelMain;
	}
}
