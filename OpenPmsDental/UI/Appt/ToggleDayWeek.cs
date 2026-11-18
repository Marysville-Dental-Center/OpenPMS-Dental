using CodeBase;
using OpenPmsDentalBusiness;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;

namespace OpenPmsDental.UI {
	//Jordan is the only one allowed to edit this file.

	///<summary>A toggle for Day/Week in the Appt module. Maybe add Month later.</summary>
	public partial class ToggleDayWeek : Control {
		#region Fields - Public
		public LayoutManagerForms LayoutManager=new LayoutManagerForms();
		#endregion Fields - Public

		#region Fields - Private
		private int _hoverIndex=-1;
		private bool _isMouseDown;
		private List<string> Items;
		///<summary></summary>
		private long _selectedIndex;
		#endregion Fields - Private

		#region Constructors
		public ToggleDayWeek() {
			InitializeComponent();
			DoubleBuffered=true;
			Size=new Size(80,35);//same as default
			Items=new List<string>();
			Items.Add("Day");
			Items.Add("Week");
		}
		#endregion Constructors

		#region Events - Public Raise
		///<summary></summary>
		[Category("OD")]
		[Description("Occurs when user clicks Day.")]
		public event EventHandler DayClick;

		///<summary></summary>
		[Category("OD")]
		[Description("Occurs when user clicks Week.")]
		public event EventHandler WeekClick;
		#endregion Events - Public Raise

		#region Event - Event Handlers - OnPaint
		protected override void OnPaint(PaintEventArgs e){
			//The new look (UI-4c-1, docs/specs/ui-refresh-plan.md section 3.6; docs/port-notes/UI-4c.md): SurfaceRaised with a 1 px Stroke border,
			//each row a toggle as the toolbar's (SubtleHover under the mouse, the selected one AccentSubtle with a 1 px Accent outline, 4 px corners,
			//inside the row), its text and its little day or week box in Text. Every rectangle and text position as before.
			Graphics g=e.Graphics;
			float scale=LayoutManager.ScaleMy();
			g.Clear(UiTheme.GetColor(EnumUiColor.SurfaceRaised));
			StringFormat stringFormat=new StringFormat(StringFormatFlags.NoWrap);
			stringFormat.LineAlignment=StringAlignment.Center;
			stringFormat.Alignment=StringAlignment.Near;//left
			int heightLine=Height/Items.Count;
			int widthLine=UiTheme.GetLineWidth(scale);
			for(int i=0; i<Items.Count; i++) {
				bool isSelected=false;
				if(_selectedIndex==i){
					isSelected=true;
				}
				Rectangle rectangleItem=GetRectangleItem(i);
				EnumUiColor uiColorText=EnumUiColor.Text;
				if(isSelected){
					UiControlPaint.Fill(g,EnumUiColor.AccentSubtle,rectangleItem,UiTheme.CORNER_CONTROL,scale);
					UiControlPaint.Outline(g,EnumUiColor.Accent,rectangleItem,widthLine,UiTheme.CORNER_CONTROL,scale);
					uiColorText=EnumUiColor.TextSelected;
				}
				else if(_hoverIndex==i){
					UiControlPaint.Fill(g,EnumUiColor.SubtleHover,rectangleItem,UiTheme.CORNER_CONTROL,scale);
				}
				Rectangle rectangleText=new Rectangle(Width/2,i*heightLine+1,Width/2,heightLine);
				if(IsTextShown){
					g.DrawString(Items[i],this.Font,UiTheme.GetBrush(uiColorText),rectangleText,stringFormat);
				}
				//The day or week box: where today's 1 px rectangle was (DrawRectangle covers one pixel more to the right and below).
				UiControlPaint.Outline(g,uiColorText,GetRectangleGlyph(i),widthLine,UiTheme.CORNER_SQUARE,scale);
			}
			UiControlPaint.Outline(g,EnumUiColor.Stroke,new Rectangle(0,0,Width,Height),widthLine,UiTheme.CORNER_SQUARE,scale);
		}

		///<summary>Whether the texts are drawn. Always true; the state tests draw the shapes alone to compare them.</summary>
		protected virtual bool IsTextShown{
			get{
				return true;
			}
		}

		///<summary>The toggle shape of a row (hover and selection): the row inside the border and 1 px from the other row, at the scale.</summary>
		protected Rectangle GetRectangleItem(int index){
			int heightLine=Height/Items.Count;
			int inset=LayoutManager.Scale(2);
			int insetY=LayoutManager.Scale(1);
			return new Rectangle(inset,index*heightLine+insetY,Width-inset*2,heightLine-insetY*2);
		}

		///<summary>The little day or week box of a row, as today's DrawRectangle covered it.</summary>
		protected Rectangle GetRectangleGlyph(int index){
			int heightLine=Height/Items.Count;
			if(index==0){//day
				return new Rectangle(LayoutManager.Scale(15),LayoutManager.Scale(5),LayoutManager.Scale(10)+1,LayoutManager.Scale(10)+1);
			}
			return new Rectangle(LayoutManager.Scale(8),heightLine+LayoutManager.Scale(5),LayoutManager.Scale(25)+1,LayoutManager.Scale(10)+1);
		}
		#endregion Event - Event Handlers - OnPaint

		#region Events - Event Handlers - Mouse
		protected override void OnMouseDown(MouseEventArgs e){
			Focus();
			_isMouseDown=true;
			int heightLine=Height/Items.Count;
			_selectedIndex=(e.Location.Y/heightLine);
			Invalidate();
			if(_selectedIndex==0){
				DayClick?.Invoke(this,new EventArgs());
			}
			else{
				WeekClick?.Invoke(this,new EventArgs());
			}
			base.OnMouseDown(e);//at end so that index will be selected before mouse down fires somewhere else (e.g. FormApptEdit).  Matches MS.
			//but, sometimes, if an event from above resulted in a dialog, then there will be no mouse up event.  Handle that below.
			MouseButtons mouseButtons=Control.MouseButtons;//introducing variable for debugging because this state is not preserved at break points.
			if(mouseButtons==MouseButtons.None){
				_isMouseDown=false;
			}
		}

		protected override void OnMouseLeave(EventArgs e){
			base.OnMouseLeave(e);
			_hoverIndex=-1;
			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs e){
			base.OnMouseMove(e);
			int heightLine=Height/Items.Count;
			//subtract 3 from the location to take border offset into account
			_hoverIndex=(e.Location.Y/heightLine);
			Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			_isMouseDown=false;
			Invalidate();
		}
		#endregion Events - Event Handlers - Mouse

		#region Methods - Event Handlers
		
		#endregion Methods - Event Handlers

		#region Properties - override
		protected override Size DefaultSize => new Size(80,35);
		#endregion Properties - override

		#region Methods - Public
		public void SetDay(){
			_selectedIndex=0;
			Invalidate();
		}

		public void SetWeek(){
			_selectedIndex=1;
			Invalidate();
		}
		#endregion Methods - Public

	
	}


}


