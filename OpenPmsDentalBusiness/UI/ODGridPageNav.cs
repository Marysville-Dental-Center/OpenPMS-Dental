using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;

namespace OpenPmsDental.UI {
	public class ODGridPageNav : UserControl {

		#region UI controls
		private TextBox textJumpToPage;
		private Panel panelPageLinks;
		private Button butFirst;
		private Button butLast;
		private LinkLabel linkOne;
		private LinkLabel linkTwo;
		private LinkLabel linkThree;
		private LinkLabel linkFour;
		private Button butRight;
		private Button butLeft;
		#endregion

		private GridOld _gridOld;
		private int _pageCur;
		///<summary>UI-12: the page numbers shown, -1 for an empty place (the grid's ListLinkVals).</summary>
		private List<int> _listPageVals=new List<int>();
		///<summary>UI-12: the cell under the mouse, or -1.</summary>
		private int _idxHover=-1;
		///<summary>UI-12: the cells of the bar: first, previous, the pages, next, last.</summary>
		private List<PageNavCell> _listCells=new List<PageNavCell>();
		private const int WIDTH_CELL=26;
		private const int GAP_CELL=2;

		[Category("OD"),Description("The grid that this control will navigate through.  This grid must have paging enabled.")]
		public GridOld GridOld {
			get { return _gridOld; }
			set { _gridOld=value; }
		}

		public ODGridPageNav() {
			//This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			//UI-12 (the owner's G-UI3 row "progress notes pagination sytling bad"): the bar is painted as flat buttons, the arrows as the Fluent
			//chevrons and the page numbers as buttons with the current page in the Accent fill, the height of the toolbar buttons. The stock
			//buttons, links and jump box stay as the designer made them (their handlers do the work) but are not shown; the keyboard still moves
			//left and right.
			foreach(Control control in Controls) {
				control.Visible=false;
			}
			SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.UserPaint|ControlStyles.Selectable,true);
			TabStop=true;
		}
		
		private void ODGridPageNav_Load(object sender,EventArgs e) {
			//grid might be null in the designer or when drawing in FormSheetDefEdit.cs
			if(GridOld!=null) {
				GridOld.PageChanged+=PagingChangeEventHandler;
			}
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.textJumpToPage = new System.Windows.Forms.TextBox();
			this.panelPageLinks = new System.Windows.Forms.Panel();
			this.linkOne = new System.Windows.Forms.LinkLabel();
			this.linkTwo = new System.Windows.Forms.LinkLabel();
			this.linkThree = new System.Windows.Forms.LinkLabel();
			this.linkFour = new System.Windows.Forms.LinkLabel();
			this.butFirst = new System.Windows.Forms.Button();
			this.butLast = new System.Windows.Forms.Button();
			this.butRight = new System.Windows.Forms.Button();
			this.butLeft = new System.Windows.Forms.Button();
			this.panelPageLinks.SuspendLayout();
			this.SuspendLayout();
			// 
			// textJumpToPage
			// 
			this.textJumpToPage.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.textJumpToPage.BackColor = System.Drawing.SystemColors.Window;
			this.textJumpToPage.Location = new System.Drawing.Point(123, 3);
			this.textJumpToPage.MaxLength = 10000;
			this.textJumpToPage.Name = "textJumpToPage";
			this.textJumpToPage.Size = new System.Drawing.Size(34, 20);
			this.textJumpToPage.TabIndex = 8;
			this.textJumpToPage.Text = "3";
			this.textJumpToPage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.textJumpToPage.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textJumpToPage_KeyUp);
			// 
			// panelPageLinks
			// 
			this.panelPageLinks.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.panelPageLinks.Controls.Add(this.linkOne);
			this.panelPageLinks.Controls.Add(this.linkTwo);
			this.panelPageLinks.Controls.Add(this.linkThree);
			this.panelPageLinks.Controls.Add(this.linkFour);
			this.panelPageLinks.Location = new System.Drawing.Point(55, 3);
			this.panelPageLinks.Name = "panelPageLinks";
			this.panelPageLinks.Size = new System.Drawing.Size(170, 20);
			this.panelPageLinks.TabIndex = 6;
			// 
			// linkOne
			// 
			this.linkOne.Location = new System.Drawing.Point(1, 1);
			this.linkOne.Name = "linkOne";
			this.linkOne.Size = new System.Drawing.Size(33, 17);
			this.linkOne.TabIndex = 9;
			this.linkOne.TabStop = true;
			this.linkOne.Text = "1";
			this.linkOne.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.linkOne.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkClickedEventHandler);
			// 
			// linkTwo
			// 
			this.linkTwo.Location = new System.Drawing.Point(34, 1);
			this.linkTwo.Name = "linkTwo";
			this.linkTwo.Size = new System.Drawing.Size(34, 17);
			this.linkTwo.TabIndex = 10;
			this.linkTwo.TabStop = true;
			this.linkTwo.Text = "2";
			this.linkTwo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.linkTwo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkClickedEventHandler);
			// 
			// linkThree
			// 
			this.linkThree.Location = new System.Drawing.Point(102, 1);
			this.linkThree.Name = "linkThree";
			this.linkThree.Size = new System.Drawing.Size(34, 17);
			this.linkThree.TabIndex = 11;
			this.linkThree.TabStop = true;
			this.linkThree.Text = "4";
			this.linkThree.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.linkThree.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkClickedEventHandler);
			// 
			// linkFour
			// 
			this.linkFour.Location = new System.Drawing.Point(136, 1);
			this.linkFour.Name = "linkFour";
			this.linkFour.Size = new System.Drawing.Size(33, 17);
			this.linkFour.TabIndex = 12;
			this.linkFour.TabStop = true;
			this.linkFour.Text = "5";
			this.linkFour.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.linkFour.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkClickedEventHandler);
			// 
			// butFirst
			// 
			this.butFirst.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butFirst.Location = new System.Drawing.Point(0, 0);
			this.butFirst.Name = "butFirst";
			this.butFirst.Size = new System.Drawing.Size(27, 26);
			this.butFirst.TabIndex = 4;
			this.butFirst.Text = "<<";
			this.butFirst.UseVisualStyleBackColor = true;
			this.butFirst.Click += new System.EventHandler(this.butFirst_Click);
			// 
			// butLast
			// 
			this.butLast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butLast.Location = new System.Drawing.Point(253, 0);
			this.butLast.Name = "butLast";
			this.butLast.Size = new System.Drawing.Size(27, 26);
			this.butLast.TabIndex = 9;
			this.butLast.Text = ">>";
			this.butLast.UseVisualStyleBackColor = true;
			this.butLast.Click += new System.EventHandler(this.butLast_Click);
			// 
			// butRight
			// 
			this.butRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butRight.Location = new System.Drawing.Point(227, 0);
			this.butRight.Name = "butRight";
			this.butRight.Size = new System.Drawing.Size(26, 26);
			this.butRight.TabIndex = 10;
			this.butRight.Text = ">";
			this.butRight.UseVisualStyleBackColor = true;
			this.butRight.Click += new System.EventHandler(this.butRight_Click);
			// 
			// butLeft
			// 
			this.butLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butLeft.Location = new System.Drawing.Point(27, 0);
			this.butLeft.Name = "butLeft";
			this.butLeft.Size = new System.Drawing.Size(26, 26);
			this.butLeft.TabIndex = 11;
			this.butLeft.Text = "<";
			this.butLeft.UseVisualStyleBackColor = true;
			this.butLeft.Click += new System.EventHandler(this.butLeft_Click);
			// 
			// ODGridPageNav
			// 
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.butLeft);
			this.Controls.Add(this.butRight);
			this.Controls.Add(this.butLast);
			this.Controls.Add(this.butFirst);
			this.Controls.Add(this.textJumpToPage);
			this.Controls.Add(this.panelPageLinks);
			this.MinimumSize = new System.Drawing.Size(143, 26);
			this.Name = "ODGridPageNav";
			this.Size = new System.Drawing.Size(280, 26);
			this.Load += new System.EventHandler(this.ODGridPageNav_Load);
			this.panelPageLinks.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion Component Designer generated code

		#region UI navigation events
		public void LinkClickedEventHandler(object sender,LinkLabelLinkClickedEventArgs e) {
			if(e.Link.LinkData==null) {
				return;//Most likely clicked a link when no data had been loaded yet.
			}
			GridOld?.NavigateToPage((int)e.Link.LinkData);
		}

		private void butFirst_Click(object sender,EventArgs e) {
			GridOld?.NavigateToPage(-1);//Safe even if value is not valid page.
		}

		private void butLeft_Click(object sender,EventArgs e) {
			GridOld?.NavigateToPage(_pageCur-1);
		}

		private void butRight_Click(object sender,EventArgs e) {
			GridOld?.NavigateToPage(_pageCur+1);
		}

		private void butLast_Click(object sender,EventArgs e) {
			GridOld?.NavigateToPage(999999);
		}
		
		private void textJumpToPage_KeyUp(object sender,KeyEventArgs e) {
			int newPage=0;
			switch(e.KeyCode) {
				case Keys.Left:
					newPage=(_pageCur-1);//Safe even if value is not valid page.
					break;
				case Keys.Right:
					newPage=(_pageCur+1);//Safe even if value is not valid page.;
					break;
				case Keys.Enter:
					newPage=PIn.Int(textJumpToPage.Text,false);//Safe even if value is not valid page.
					break;
				default:
				return;
			}
			GridOld?.NavigateToPage(newPage);
		}
		#endregion
		
		public void PagingChangeEventHandler(object sender, ODGridPageEventArgs e) {
			_pageCur=e.PageCur;
			_listPageVals=new List<int>(e.ListLinkVals);
			Invalidate();
			textJumpToPage.Text=POut.Int(e.PageCur);
			//We reuse the same controls and just change thier text and data to avoid flicker in UI.
			for(int i=0;i<panelPageLinks.Controls.Count;i++) {
				LinkLabel pageLink=(LinkLabel)panelPageLinks.Controls[i];
				int pageLinkVal=e.ListLinkVals[i];
				if(pageLinkVal==-1) {
					pageLink.Visible=false;
					continue;
				}
				pageLink.Visible=true;
				pageLink.Text=pageLinkVal.ToString();
				pageLink.Links[0].LinkData=pageLinkVal;
			}
		}

		#region UI-12 painted bar
		///<summary>One cell of the bar: an arrow (Page -1 and -2 for first and previous, -3 and -4 for next and last) or a page number.</summary>
		private class PageNavCell {
			public Rectangle Rectangle;
			public int Page;
		}

		///<summary>The cells, centred as a group: first, previous, the shown page numbers, next, last; each WIDTH_CELL wide (a page number wider
		///when its text needs it), GAP_CELL apart, the control's height.</summary>
		private List<PageNavCell> GetCells(Graphics g) {
			List<int> listPages=_listPageVals.Where(x => x!=-1).ToList();
			if(listPages.Count==0 && _pageCur>0) {
				listPages.Add(_pageCur);
			}
			List<int> listKeys=new List<int> { -1,-2 };
			listKeys.AddRange(listPages);
			listKeys.Add(-3);
			listKeys.Add(-4);
			int widthCell=ScaleI(WIDTH_CELL);
			int gap=ScaleI(GAP_CELL);
			List<int> listWidths=new List<int>();
			foreach(int key in listKeys) {
				int width=widthCell;
				if(key>0) {
					width=Math.Max(widthCell,TextRenderer.MeasureText(g,key.ToString(),Font).Width+ScaleI(8));
				}
				listWidths.Add(width);
			}
			int widthAll=listWidths.Sum()+gap*(listWidths.Count-1);
			int x=Math.Max(0,(Width-widthAll)/2);
			List<PageNavCell> listCells=new List<PageNavCell>();
			for(int i=0;i<listKeys.Count;i++) {
				listCells.Add(new PageNavCell { Rectangle=new Rectangle(x,0,listWidths[i],Height),Page=listKeys[i] });
				x+=listWidths[i]+gap;
			}
			return listCells;
		}

		private int ScaleI(int value96) {
			return (int)Math.Round(value96*DeviceDpi/96f);
		}

		protected override void OnPaint(PaintEventArgs e) {
			Graphics g=e.Graphics;
			Color colorParent=Parent?.BackColor ?? SystemColors.Control;
			if(colorParent.A<255) {
				colorParent=SystemColors.Control;
			}
			g.Clear(colorParent);
			g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			float scale=DeviceDpi/96f;
			_listCells=GetCells(g);
			for(int i=0;i<_listCells.Count;i++) {
				PageNavCell cell=_listCells[i];
				bool isCurrent=cell.Page>0 && cell.Page==_pageCur;
				if(isCurrent) {
					UiControlPaint.Fill(g,EnumUiColor.Accent,cell.Rectangle,UiTheme.CORNER_CONTROL,scale);
				}
				else if(i==_idxHover && Enabled) {
					UiControlPaint.Fill(g,EnumUiColor.SubtleHover,cell.Rectangle,UiTheme.CORNER_CONTROL,scale);
				}
				EnumUiColor uiColor=!Enabled ? EnumUiColor.TextDisabled : (isCurrent ? EnumUiColor.TextOnAccent : EnumUiColor.Text);
				if(cell.Page>0) {
					TextRenderer.DrawText(g,cell.Page.ToString(),Font,cell.Rectangle,UiTheme.GetColor(uiColor),
						TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.NoPrefix);
					continue;
				}
				int sizeGlyph=ScaleI(16);
				Rectangle rectangleGlyph=new Rectangle(cell.Rectangle.X+(cell.Rectangle.Width-sizeGlyph)/2,cell.Rectangle.Y+(cell.Rectangle.Height-sizeGlyph)/2,
					sizeGlyph,sizeGlyph);
				UiGlyph uiGlyph=(cell.Page==-1 || cell.Page==-2) ? UiGlyph.ChevronLeft : UiGlyph.ChevronRight;
				Color color=UiTheme.GetColor(uiColor);
				if(cell.Page==-1 || cell.Page==-4) {
					//First and last: two chevrons side by side.
					int offset=ScaleI(3);
					UiIcons.Draw(g,uiGlyph,new Rectangle(rectangleGlyph.X-offset,rectangleGlyph.Y,sizeGlyph,sizeGlyph),color);
					UiIcons.Draw(g,uiGlyph,new Rectangle(rectangleGlyph.X+offset,rectangleGlyph.Y,sizeGlyph,sizeGlyph),color);
				}
				else {
					UiIcons.Draw(g,uiGlyph,rectangleGlyph,color);
				}
			}
			if(Focused && ShowFocusCues) {
				UiControlPaint.FocusRing(g,new Rectangle(0,0,Width,Height),UiTheme.CORNER_CONTROL,scale);
			}
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			int idx=_listCells.FindIndex(x => x.Rectangle.Contains(e.Location));
			if(idx!=_idxHover) {
				_idxHover=idx;
				Invalidate();
			}
		}

		protected override void OnMouseLeave(EventArgs e) {
			base.OnMouseLeave(e);
			if(_idxHover!=-1) {
				_idxHover=-1;
				Invalidate();
			}
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			if(e.Button!=MouseButtons.Left) {
				return;
			}
			PageNavCell cell=_listCells.FirstOrDefault(x => x.Rectangle.Contains(e.Location));
			if(cell==null) {
				return;
			}
			switch(cell.Page) {
				case -1:
					butFirst_Click(this,EventArgs.Empty);
					break;
				case -2:
					butLeft_Click(this,EventArgs.Empty);
					break;
				case -3:
					butRight_Click(this,EventArgs.Empty);
					break;
				case -4:
					butLast_Click(this,EventArgs.Empty);
					break;
				default:
					GridOld?.NavigateToPage(cell.Page);
					break;
			}
		}

		protected override bool IsInputKey(Keys keyData) {
			if(keyData==Keys.Left || keyData==Keys.Right) {
				return true;
			}
			return base.IsInputKey(keyData);
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			if(e.KeyCode==Keys.Left) {
				GridOld?.NavigateToPage(_pageCur-1);
			}
			else if(e.KeyCode==Keys.Right) {
				GridOld?.NavigateToPage(_pageCur+1);
			}
		}

		protected override void OnGotFocus(EventArgs e) {
			base.OnGotFocus(e);
			Invalidate();
		}

		protected override void OnLostFocus(EventArgs e) {
			base.OnLostFocus(e);
			Invalidate();
		}
		#endregion UI-12 painted bar
	}


}
