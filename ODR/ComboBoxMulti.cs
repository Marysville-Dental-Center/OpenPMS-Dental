using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace ODR
{
	/// <summary>
	/// Summary description for ComboBoxMulti.
	/// </summary>
	public class ComboBoxMulti : System.Windows.Forms.UserControl{
		private System.ComponentModel.IContainer components=null;
		private ArrayList items;
		private System.Windows.Forms.PictureBox dropButton;
		private bool droppedDown;
		private System.Windows.Forms.TextBox textMain;
		private System.Windows.Forms.ContextMenuStrip cMenu;
		private ArrayList selectedIndices;

		/// <summary></summary>
		public ComboBoxMulti(){
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			selectedIndices=new ArrayList();
			items=new ArrayList();
		}

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
				if(cMenu != null)
				{
					cMenu.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ComboBoxMulti));
			this.dropButton = new System.Windows.Forms.PictureBox();
			this.textMain = new System.Windows.Forms.TextBox();
			this.cMenu = new System.Windows.Forms.ContextMenuStrip();
			this.SuspendLayout();
			// 
			// dropButton
			// 
			this.dropButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.dropButton.Image = ((System.Drawing.Image)(resources.GetObject("dropButton.Image")));
			this.dropButton.Location = new System.Drawing.Point(102, 1);
			this.dropButton.Name = "dropButton";
			this.dropButton.Size = new System.Drawing.Size(17, 19);
			this.dropButton.TabIndex = 1;
			this.dropButton.TabStop = false;
			this.dropButton.Click += new System.EventHandler(this.dropButton_Click);
			// 
			// textMain
			// 
			this.textMain.BackColor = System.Drawing.Color.White;
			this.textMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textMain.Location = new System.Drawing.Point(3, 4);
			this.textMain.Name = "textMain";
			this.textMain.ReadOnly = true;
			this.textMain.Size = new System.Drawing.Size(95, 13);
			this.textMain.TabIndex = 2;
			this.textMain.Text = "";
			// 
			// ComboBoxMulti
			// 
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.textMain);
			this.Controls.Add(this.dropButton);
			this.Name = "ComboBoxMulti";
			this.Size = new System.Drawing.Size(120, 21);
			this.Load += new System.EventHandler(this.ComboBoxMulti_Load);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.ComboBoxMulti_Paint);
			this.Leave += new System.EventHandler(this.ComboBoxMulti_Leave);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.ComboBoxMulti_Layout);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>The items to display in the combo box.</summary>
		[Category("Data"),
			Description("The text of the items to display in the dropdown section.")
		]
		public ArrayList Items{
			get{
				return items;
			}
			set{
				items=value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the combo box is displaying its drop-down portion.
		/// </summary>
		public bool DroppedDown{
			get{
				return droppedDown;
			}
			set{
				droppedDown=value;
			}
		}

		///<summary>The indices of selected items.</summary>
		public ArrayList SelectedIndices{
			get{
				return selectedIndices;
			}
			set{
				selectedIndices=value;
			}
		}
		
		private void ComboBoxMulti_Paint(object sender, System.Windows.Forms.PaintEventArgs e) {
			e.Graphics.DrawRectangle(new Pen(Color.FromArgb(127,157,185))//blue
				,0,0,Width-1,Height-1);
		}

		private void dropButton_Click(object sender, System.EventArgs e) {
			textMain.Select();//this is critical to trigger the enter and leave events.
			if(droppedDown){
				droppedDown=false;
			}
			else{
				//show the list
				cMenu?.Dispose();
				cMenu=MakeMenu();
        cMenu.Show(this,new Point(0,20));
				/*
				listBox2=new ListBox();
				listBox2.Click += new System.EventHandler(listBox2_Click);
				listBox2.SelectionMode=SelectionMode.MultiExtended;
				//MessageBox.Show(items.Count.ToString());
				listBox2.Size=new Size(Width,14*items.Count);
				listBox2.Location=new Point(0,21);
				Height=listBox2.Height+21;
				//MessageBox.Show(Height.ToString());
				for(int i=0;i<items.Count;i++){
					listBox2.Items.Add(items[i]);
					if(selectedIndices.Contains(i)){
						listBox2.SetSelected(i,true);
					}
				}
				Controls.Add(listBox2);
				this.BringToFront();*/
				//((Control)Container).Height;
				//this.Location;
				droppedDown=true;
			}
		}

		///<summary>The list: one item per entry of Items, in order, each 14 high and as wide as the box less 18, painted by MenuItem_Paint (the
		///owner-drawn MenuItems of the WinForms ContextMenu this replaces had the same size and painting). Not shown here.</summary>
		private ContextMenuStrip MakeMenu(){
			ContextMenuStrip contextMenuStrip=new ContextMenuStrip();
			contextMenuStrip.ShowImageMargin=false;
			contextMenuStrip.ShowCheckMargin=false;
			contextMenuStrip.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(cMenu_Closing);
			ToolStripMenuItem menuItem;
			for(int i=0;i<items.Count;i++){
				menuItem=new ToolStripMenuItem(items[i].ToString(),null,new System.EventHandler(MenuItem_Click));
				menuItem.AutoSize=false;
				menuItem.Size=new Size(Width-18,14);//not sure why I have to subtract 18 to make it the proper width.
				menuItem.Paint += new System.Windows.Forms.PaintEventHandler(MenuItem_Paint);
				contextMenuStrip.Items.Add(menuItem);
			}
			return contextMenuStrip;
		}

		///<summary>A click toggles the item and the list stays open for the next one, as the ContextMenu did by showing itself again after each click.</summary>
		private void cMenu_Closing(object sender, System.Windows.Forms.ToolStripDropDownClosingEventArgs e){
			if(e.CloseReason==ToolStripDropDownCloseReason.ItemClicked){
				e.Cancel=true;
			}
		}

		private void MenuItem_Click(object sender, System.EventArgs e){
			ToolStripItem toolStripItem=(ToolStripItem)sender;
			int index=toolStripItem.Owner.Items.IndexOf(toolStripItem);
			if(selectedIndices.Contains(index)){
				//this item was already selected
				selectedIndices.Remove(index);
			}
			else{
				selectedIndices.Add(index);
			}
			FillText();
			toolStripItem.Owner.Invalidate();//the list stays open (cMenu_Closing): paint the item's new selection.
		}

		private void MenuItem_Paint(object sender, System.Windows.Forms.PaintEventArgs e){
			ToolStripItem toolStripItem=(ToolStripItem)sender;
			int index=toolStripItem.Owner.Items.IndexOf(toolStripItem);
			string myCaption=items[index].ToString();
			Brush brushBack;
			if(selectedIndices.Contains(index))
				brushBack=SystemBrushes.Highlight;
			else
				brushBack=SystemBrushes.Window;
			Rectangle bounds=new Rectangle(Point.Empty,toolStripItem.Size);
			e.Graphics.FillRectangle(brushBack,bounds);
			e.Graphics.DrawString(myCaption,toolStripItem.Font,Brushes.Black,bounds.X,bounds.Y);
		}

		private void FillText(){
			textMain.Text="";
			for(int i=0;i<selectedIndices.Count;i++){
				if(i>0)
					textMain.Text+=" OR ";
				textMain.Text+=items[(int)selectedIndices[i]];
			}
		}

		private void ComboBoxMulti_Layout(object sender, System.Windows.Forms.LayoutEventArgs e) {
			textMain.Width=Width-21;
		}

		//private void listBox2_Click(object sender, System.EventArgs e) {
		//	selectedIndices=new ArrayList();
		//	for(int i=0;i<listBox2.SelectedIndices.Count;i++){
		//		selectedIndices.Add(listBox2.SelectedIndices[i]);
		//	}
		//	FillText();
		//}

		private void ComboBoxMulti_Load(object sender, System.EventArgs e) {
			FillText();
		}

		private void ComboBoxMulti_Leave(object sender, System.EventArgs e) {
			droppedDown=false;
		}

		///<summary></summary>
		public void SetSelected(int index,bool setToValue){
			if(setToValue) {
				selectedIndices.Add(index);
			}
			else {
				selectedIndices.Remove(index);
			}
			FillText();//Since the selections probably changed, the text in the combobox display probably changed as well.
		}

		


	}
}
















