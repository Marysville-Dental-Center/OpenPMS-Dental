using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI {
	///<summary></summary>
	public delegate void ODButtonPanelEventHandler(object sender,ODButtonPanelEventArgs e);

	///<summary>Allows for a button panel that is customizable and generated from data in the DB.</summary>
	public partial class ODButtonPanel:UserControl {
		public LayoutManagerForms LayoutManager=new LayoutManagerForms();
		///<summary>Fixed 18 at 96dpi</summary>
		private int _heightRow=18;
		private Point _pointMouseClick;
		public List<ODPanelItem> ListODPanelItems;
		public bool IsUpdating;
		///<summary>The last item that was clicked on. Can be null.</summary>
		public ODPanelItem SelectedItem;
		///<summary>The last row that was clicked on. -1 if no row selected.</summary>
		public int SelectedRow;
		//The colours are UiTheme's tokens (UI-6, docs/specs/ui-refresh-plan.md section 3.6): shared per thread, never disposed here.
		///<summary>The button item under the mouse, or null. Set by the mouse events; the painting reads it.</summary>
		private ODPanelItem _odPanelItemHot;
		///<summary>True while the left mouse button is down after a press on the panel (the pressed look of the hot button).</summary>
		private bool _isMouseDown;
		///<summary>Ignore the public font.  We want to control font internally.</summary>
		private Font _font;

		#region EventHandlers
		///<summary></summary>
		[Category("Action"),Description("Occurs when a button item is single clicked.")]
		public event ODButtonPanelEventHandler ItemClickBut=null;
		///<summary></summary>
		[Category("Action"),Description("Occurs when a row is double clicked.")]
		public event ODButtonPanelEventHandler RowDoubleClick=null;
		#endregion

		public ODButtonPanel() {
			InitializeComponent();
			Font=LayoutManagerForms.FontInitial;
			ListODPanelItems=new List<ODPanelItem>();
			SelectedRow=-1;
			DoubleBuffered=true;
		}

		///<summary></summary>
		protected override void OnLoad(EventArgs e) {
			base.OnLoad(e);
		}

		///<summary></summary>
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
		}

		protected override void OnSizeChanged(EventArgs e) {
			base.OnSizeChanged(e);
			_font=new Font(UiTheme.FontButtonPanel.FontFamily,LayoutManager.ScaleFontODZoom(UiTheme.FontButtonPanel.SizeInPoints));//The button panel's token (UI-2a-5).
			_heightRow=LayoutManager.Scale(18);
			Invalidate();
		}

		#region Computations
		///<summary>Computes the position of each column and the overall width.  Called from endUpdate and also from OnPaint.</summary>
		private void ComputeWidthsAndLocations(Graphics g) {
			ListODPanelItems.Sort(ODPanelItem.SortYX);
			for(int i=0;i<ListODPanelItems.Count;i++) {
				if(string.IsNullOrWhiteSpace(ListODPanelItems[i].Text)) {
					ListODPanelItems[i].ItemWidth=LayoutManager.Scale(ListODPanelItems[i].Text.Length*3+8);//measure each whitespace character as a fixed 3px width. +8px padding.
				}
				else{
					ListODPanelItems[i].ItemWidth=(int)g.MeasureString(ListODPanelItems[i].Text,_font).Width+LayoutManager.Scale(8);
				}
				ListODPanelItems[i].Location.Y=ListODPanelItems[i].YPos*_heightRow;
				//Then set xPos using previous cell's position and width:
				if(i>0 && ListODPanelItems[i].YPos==ListODPanelItems[i-1].YPos) {//Previous item was on the same row. First item on each row is 0
					ListODPanelItems[i].Location.X=ListODPanelItems[i-1].Location.X+ListODPanelItems[i-1].ItemWidth;
				}
			}
		}

		///<summary>Returns the panel item clicked on. Returns null if no item found.</summary>
		public ODPanelItem PointToItem(Point loc) {
			ODPanelItem retVal=null;
			retVal=ListODPanelItems.Find(item =>
				item.YPos==(loc.Y/_heightRow) //item is on the clicked row
				&& item.Location.X<loc.X //point is to to the right of the left side
				&& item.Location.X+item.ItemWidth>loc.X); //point is the the left of the right side
			return retVal;
		}
		#endregion Painting

		#region Painting
		///<summary></summary>
		protected override void OnPaintBackground(PaintEventArgs pea) {
			//base.OnPaintBackground (pea);
			//don't paint background.  This reduces flickering.
		}

		///<summary>Runs any time the control is invalidated.</summary>
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
			if(IsUpdating) {
				return;
			}
			if(Width<1 || Height<1) {
				return;
			}
			try {
				e.Graphics.SmoothingMode=SmoothingMode.HighQuality;
				ComputeWidthsAndLocations(e.Graphics);
				DrawBackG(e.Graphics);
				DrawItems(e.Graphics);
				DrawOutline(e.Graphics);
			}
			catch(Exception ex) {
				//We had one customer who was receiving overflow exceptions because the ClientRetangle provided by the system was invalid,
				//due to a graphics device hardware state change when loading the Dexis client application via our Dexis bridge.
				//If we receive an invalid ClientRectangle, then we will simply not draw the button for a frame or two until the system has initialized.
				//A couple of frames later the system should return to normal operation and we will be able to draw the button again.
				ex.DoNothing();
			}
		}

		///<summary>Draws the background: flat SurfaceRaised (UI-6; the old shadow on the top and left is gone).</summary>
		private void DrawBackG(Graphics g) {
			g.FillRectangle(UiTheme.GetBrush(EnumUiColor.SurfaceRaised),0,0,Width,Height);
		}

		private void DrawItems(Graphics g) {
			for(int i=0;i<ListODPanelItems.Count;i++) {
				switch(ListODPanelItems[i].ItemType) {
					case ODPanelItemType.Button:
						DrawItemBut(g,ListODPanelItems[i]);
						break;
					case ODPanelItemType.Label:
						DrawItemLabel(g,ListODPanelItems[i]);
						break;
				}
			}
		}

		private void DrawItemLabel(Graphics g,ODPanelItem item) {
			RectangleF itemRect=new RectangleF(
				item.Location.X,item.Location.Y,
				item.ItemWidth,_heightRow);
			g.FillRectangle(UiTheme.GetBrush(EnumUiColor.SurfaceRaised),itemRect);
			g.DrawString(item.Text,_font,UiTheme.GetBrush(EnumUiColor.Text),itemRect,
				new StringFormat { Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center });
		}

		///<summary>A button item as the new look's buttons (UI-6, section 3.6, Buttons): flat SurfaceRaised with a 1 px Stroke border and 4 px corners,
		///ButtonHover on hover and ButtonPressed pressed, the text in Text; in the item's bounds as before.</summary>
		private void DrawItemBut(Graphics g,ODPanelItem odPanelItem) {
			Rectangle recOutline=new Rectangle(odPanelItem.Location.X,odPanelItem.Location.Y,odPanelItem.ItemWidth-1,_heightRow-1);
			float scale=LayoutManager.ScaleMy();
			int radius=UiTheme.GetCornerRadius(UiTheme.CORNER_CONTROL,scale);
			EnumUiColor uiColorFill=EnumUiColor.SurfaceRaised;
			if(odPanelItem==_odPanelItemHot) {
				uiColorFill=_isMouseDown ? EnumUiColor.ButtonPressed : EnumUiColor.ButtonHover;
			}
			UiToolStripRenderer.FillRounded(g,uiColorFill,recOutline,radius);
			UiToolStripRenderer.OutlineRounded(g,EnumUiColor.Stroke,recOutline,radius,UiTheme.GetLineWidth(scale));
			using(StringFormat stringFormat=new StringFormat { Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center }) {
				g.DrawString(odPanelItem.Text,_font,UiTheme.GetBrush(EnumUiColor.Text),recOutline,stringFormat);
			}
		}

		///<summary>Draws the 1 px Stroke outline around the entire control.</summary>
		private void DrawOutline(Graphics g) {
			int widthLine=UiTheme.GetLineWidth(LayoutManager.ScaleMy());
			Brush brushOutline=UiTheme.GetBrush(EnumUiColor.Stroke);
			g.FillRectangle(brushOutline,0,0,Width,widthLine);
			g.FillRectangle(brushOutline,0,Height-widthLine,Width,widthLine);
			g.FillRectangle(brushOutline,0,0,widthLine,Height);
			g.FillRectangle(brushOutline,Width-widthLine,0,widthLine,Height);
		}
		#endregion

		#region Single Click
		protected override void OnClick(EventArgs e) {
			base.OnClick(e);
			SelectedItem=PointToItem(_pointMouseClick);
			SelectedRow=_pointMouseClick.Y/_heightRow;
			if(SelectedItem!=null) {
				//OnClickItem(SelectedItem);
				switch(SelectedItem.ItemType) {
					case ODPanelItemType.Button:
						OnClickButton(SelectedItem);
						break;
					//case ODPanelItemType.Label:
					//	OnClickLabel(SelectedItem);
					//	break;
				}
			}
			//OnRowClick(SelectedRow);
		}

		private void OnClickButton(ODPanelItem itemClick) {
			ODButtonPanelEventArgs pArgs=new ODButtonPanelEventArgs(SelectedItem,SelectedRow,MouseButtons.Left);
			if(ItemClickBut!=null) {
				ItemClickBut(this,pArgs);
			}
		}
		#endregion Single Click

		#region Double Click
		///<summary></summary>
		protected override void OnDoubleClick(EventArgs e) {
			base.OnDoubleClick(e);
			SelectedItem=PointToItem(_pointMouseClick);
			SelectedRow=_pointMouseClick.Y/_heightRow;
			//if(SelectedItem!=null) {
			//	OnDoubleClickItem(SelectedItem);
			//	switch(SelectedItem.ItemType) {
			//		case ODPanelItemType.Button:
			//			OnDoubleClickButton(SelectedItem);
			//			break;
			//		case ODPanelItemType.Label:
			//			OnDoubleClickLabel(SelectedItem);
			//			break;
			//	}
			//}
			OnRowDoubleClick(_pointMouseClick.Y/_heightRow);
		}

		private void OnRowDoubleClick(int p) {
			ODButtonPanelEventArgs pArgs=new ODButtonPanelEventArgs(SelectedItem,SelectedRow,MouseButtons.Left);
			if(RowDoubleClick!=null) {
				RowDoubleClick(this,pArgs);
			}
		}
		#endregion Double Click

		#region MouseEvents

		///<summary></summary>
		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
			_pointMouseClick=new Point(e.X,e.Y);
			//ODPanelItem item=PointToItem(_pointMouseClick);
			//int row=e.Y/_rowHeight;
			if(e.Button==MouseButtons.Left) {
				_isMouseDown=true;
				Invalidate();
			}
		}

		///<summary>Tracks the button under the mouse for the hover look (UI-6).</summary>
		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			ODPanelItem odPanelItem=PointToItem(e.Location);
			if(odPanelItem!=null && odPanelItem.ItemType!=ODPanelItemType.Button) {
				odPanelItem=null;
			}
			if(odPanelItem!=_odPanelItemHot) {
				_odPanelItemHot=odPanelItem;
				Invalidate();
			}
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if(e.Button==MouseButtons.Left && _isMouseDown) {
				_isMouseDown=false;
				Invalidate();
			}
		}

		protected override void OnMouseLeave(EventArgs e) {
			base.OnMouseLeave(e);
			if(_odPanelItemHot!=null) {
				_odPanelItemHot=null;
				Invalidate();
			}
		}

		#endregion MouseEvents

		#region BeginEndUpdate
		///<summary>Call this before adding any rows.  You would typically call Rows.Clear after this.</summary>
		public void BeginUpdate() {
			IsUpdating=true;
		}

		///<summary>Must be called after adding rows.  This computes the columns, computes the rows, lays out the scrollbars, clears SelectedIndices, and invalidates.</summary>
		public void EndUpdate() {
			using(Graphics g=this.CreateGraphics()) {
				ComputeWidthsAndLocations(g);
			}
			IsUpdating=false;
			Invalidate();
		}
		#endregion BeginEndUpdate

	}

	///<summary></summary>
	public class ODButtonPanelEventArgs {
		private ODPanelItem item;
		private int row;
		private MouseButtons button;

		///<summary></summary>
		public ODButtonPanelEventArgs(ODPanelItem item,int row,MouseButtons button) {
			this.item=item;
			this.row=row;
			this.button=button;
		}

		///<summary>Can be null.</summary>
		public ODPanelItem Item {
			get {
				return item;
			}
		}

		///<summary></summary>
		public int Row {
			get {
				return row;
			}
		}

		///<summary>Gets which mouse button was pressed.</summary>
		public MouseButtons Button {
			get {
				return button;
			}
		}

	}

}
