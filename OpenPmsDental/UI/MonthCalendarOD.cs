using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI{
	//Jordan is the only one allowed to edit this file.

	///<summary>A replacement for the MS MonthCalendar control.</summary>
	public partial class MonthCalendarOD : Control{
		#region Fields - Public
		///<summary>Set this to false to prevent user from clicking the top text to manually set date. Useful, for example, if this monthCalendar is a dropdown, so there's already a box for that.</summary>
		public bool AllowClickingTopText=true;
		public LayoutManagerForms LayoutManager=new LayoutManagerForms();
		#endregion Fields - Public

		#region Fields -Private
		///<summary>The dates that are showing in each cell.  This is a 7x6 array.</summary>
		private DateTime[,] _arrayDates;
		///<summary>Unfortunately, this has the same name as a MS event for selecting a date.  So this can't be used publicly unless I'm careful.</summary>
		private DateTime _dateSelected;
		private float _heightCell;
		///<summary>Needs to be scaled each time.</summary>
		private int _heightFooter96=21;
		///<summary>Needs to be scaled each time.</summary>
		private int _heightHeader96=34;
		///<summary>Because we can go to different months without changing the selected date. We always use the first day of the month. Can't be changed externally.</summary>
		private DateTime _dateMonthShowing;
		///<summary>Identifies the current hot hover cell as the mouse moves around.  Row 0 is never used because that's the days of week. 0,0 indicates no hot.</summary>
		private Point _pointHotHover;
		///<summary>26x33</summary>
		private Rectangle _rectangleMonthLeft;
		private Rectangle _rectangleTopText;
		private Rectangle _rectangleMonthRight;
		private Rectangle _rectangleTodayHover;
		private Rectangle _rectangleYearLeft;
		private Rectangle _rectangleYearRight;
		///<summary>x Position of left of each cell.  Always 7 columns</summary>
		private float[] _xPos;
		///<summary>y Position of top of each cell.  Top row is days of week, then always 6 rows of numbers</summary>
		private float[] _yPos;
		#endregion Fields - Private

		#region Constructor
		public MonthCalendarOD(){
			InitializeComponent();
			DoubleBuffered=true;
			_dateSelected=DateTime.Today;
			_dateMonthShowing=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
			CalculateRectangles();
		}
		#endregion Constructor

		#region Events - Public Raise
		///<summary>Occurs when user clicks to change date.  Does not fire in response to programmatic data changes.</summary>
		[Category("OD")]
		[Description("Occurs when user clicks to change date.  Does not fire in response to programmatic data changes.")]
		public event EventHandler DateChanged;
		#endregion Events - Public Raise

		#region Properties
		protected override Size DefaultSize => new Size(227, 162);
		#endregion Properties

		#region Methods Override
		protected override void OnMouseDown(MouseEventArgs e){
			/*
			if(e.Y<_yPos[1]){
				return;
			}
			if(e.Y>Height-LayoutManager.Scale(_heightFooter96)){
				return;
			}
			if(_pointHotHover==new Point(0,0)){
				return;
			}*/
			if(_pointHotHover!=new Point(0,0)){
				_dateSelected=_arrayDates[_pointHotHover.X,_pointHotHover.Y-1];
				DateChanged?.Invoke(this,new EventArgs());
				if(_dateSelected.Month!=_dateMonthShowing.Month){
					_dateMonthShowing=new DateTime(_dateSelected.Year,_dateSelected.Month,1);
				}
				Invalidate();
			}
			if(_rectangleTopText.Contains(e.Location) && AllowClickingTopText) {
				FrmDatePicker frmDatePicker=new FrmDatePicker();
				frmDatePicker.DateEntered=_dateSelected;
				frmDatePicker.WidthForm=Width;
				frmDatePicker.PointStartLocation=PointToScreen(
					new Point(0,_rectangleTopText.Bottom+LayoutManager.Scale(5)));
				frmDatePicker.ShowDialog();
				if(frmDatePicker.IsDialogOK) {
					_dateSelected=frmDatePicker.DateEntered;
					if(_dateSelected.Month!=_dateMonthShowing.Month || _dateSelected.Year!=_dateMonthShowing.Year){
						_dateMonthShowing=new DateTime(_dateSelected.Year,_dateSelected.Month,1);
					}
					DateChanged?.Invoke(this,new EventArgs());
					Invalidate();
				}
			}
			if(_rectangleMonthLeft.Contains(e.Location)){
				_dateMonthShowing=_dateMonthShowing.AddMonths(-1);
				Invalidate();
			}
			if(_rectangleMonthRight.Contains(e.Location)){
				_dateMonthShowing=_dateMonthShowing.AddMonths(1);
				Invalidate();
			}
			if(_rectangleYearLeft.Contains(e.Location)){
				_dateMonthShowing=_dateMonthShowing.AddYears(-1);
				Invalidate();
			}
			if(_rectangleYearRight.Contains(e.Location)){
				_dateMonthShowing=_dateMonthShowing.AddYears(1);
				Invalidate();
			}
			if(_rectangleTodayHover.Contains(e.Location)){
				_dateSelected=DateTime.Today;
				DateChanged?.Invoke(this,new EventArgs());
				if(DateTime.Today.Month!=_dateMonthShowing.Month){
					_dateMonthShowing=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
				}
				Invalidate();
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseLeave(EventArgs e){
			_pointHotHover=new Point();
			Invalidate();
			base.OnMouseLeave(e);
		}

		protected override void OnMouseMove(MouseEventArgs e){
			if(_yPos is null){
				return;
			}
			int hotRow=0;
			if(e.Y>_yPos[1] && e.Y<Height-LayoutManager.Scale(_heightFooter96)){
				for(int i=2;i<_yPos.Length;i++){
					if(e.Y<_yPos[i]){
						hotRow=i-1;
						break;
					}
					hotRow=i;//last row;
				}
			}
			int hotCol=0;
			for(int i=1;i<_xPos.Length;i++){
				if(e.X<_xPos[i]){
					hotCol=i-1;
					break;
				}
				hotCol=i;//last col
			}
			if(hotRow==0){
				_pointHotHover=new Point();
			}
			else{
				_pointHotHover=new Point(hotCol,hotRow);
			}
			Invalidate();
			base.OnMouseMove(e);
		}

		protected override void OnFontChanged(EventArgs e){
			base.OnFontChanged(e);
			CalculateRectangles();
			Invalidate();
		}

		protected override void OnEnabledChanged(EventArgs e){
			base.OnEnabledChanged(e);
			Invalidate();
		}

		protected override void OnGotFocus(EventArgs e){
			base.OnGotFocus(e);
			Invalidate();//the focus ring on the selected day
		}

		protected override void OnLostFocus(EventArgs e){
			base.OnLostFocus(e);
			Invalidate();
		}

		protected override void OnSizeChanged(EventArgs e){
			base.OnSizeChanged(e);
			CalculateRectangles();
			Invalidate();
		}
		#endregion Methods Overide

		#region Method OnPaint
		protected override void OnPaint(PaintEventArgs pe){
			Graphics g=pe.Graphics;
			base.OnPaint(pe);
			//The new look (UI-4b-3, docs/specs/ui-refresh-plan.md section 3.6): drawn from the UiTheme tokens with UiCalendarPaint's shapes. Every
			//rectangle and text position is as before; only the colours, the corners and the arrows' glyphs changed.
			float scale=LayoutManager.ScaleMy();
			g.FillRectangle(UiTheme.GetBrush(EnumUiColor.SurfaceRaised),ClientRectangle);
			//Calculate cell sizes========================================================================================
			float widthCell=Width/7f;
			_heightCell=(Height-LayoutManager.Scale(_heightHeader96)-LayoutManager.Scale(_heightFooter96))/7f;
			_yPos=new float[7];
			for(int i=0;i<7;i++){
				_yPos[i]=LayoutManager.Scale(_heightHeader96)+_heightCell*(float)i;
			}
			_xPos=new float[7];
			for(int i=0;i<7;i++){
				_xPos[i]=(float)Width/7f*(float)i;
			}
			//Fill arrayDates=============================================================================================
			_arrayDates=new DateTime[7,6];
			//First day of week is not Sunday in China, for example.  So:
			DayOfWeek firstDayOfWeek=DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek;
			//Which day does the first fall on?
			DayOfWeek dayOfFirst=(DayOfWeek)((int)_dateMonthShowing.DayOfWeek-(int)firstDayOfWeek);
			DateTime dateFilling=_dateMonthShowing.AddDays(-(int)dayOfFirst);
			int row=0;
			int col=0;
			while(true){
				_arrayDates[col,row]=dateFilling;
				dateFilling=dateFilling.AddDays(1);
				if(col==6){
					col=0;
					row++;
				}
				else{
					col++;
				}
				if(row==6){
					break;
				}
			}
			//Horizontal line below weekdays: 1 px Gridline==================================================================
			UiControlPaint.Fill(g,EnumUiColor.Gridline,GetRectangleLineUnderDays(),UiTheme.CORNER_SQUARE,scale);
			//Hover effect: SubtleHover with 4 px corners=====================================================================
			if(_pointHotHover!=new Point() && Enabled){
				UiCalendarPaint.DrawHover(g,GetRectangleDay(_pointHotHover.X,_pointHotHover.Y),scale);
			}
			//Date selected: Accent with 4 px corners=========================================================================
			//if(_monthShowing.Month==_dateSelected.Month){//date is showing. Can't do this.  Could be showing dimmed.
			Rectangle rectangleSelected=Rectangle.Empty;
			for(int c=0;c<7;c++){
				for(int r=0;r<6;r++){
					if(_arrayDates[c,r]!=_dateSelected){
						continue;
					}
					rectangleSelected=GetRectangleDay(c,r+1);
					UiCalendarPaint.DrawSelectedDay(g,rectangleSelected,Enabled,scale);
				}
			}
			//}
			//Today: a 1 px Accent ring (today orange)=========================================================================
			//if(_monthShowing.Month==DateTime.Today.Month){//today is showing. Can't do this. Could be dimmed
				for(int c=0;c<7;c++){
					for(int r=0;r<6;r++){
						if(_arrayDates[c,r]!=DateTime.Today){
							continue;
						}
						UiCalendarPaint.DrawToday(g,GetRectangleDay(c,r+1),isSelected:_arrayDates[c,r]==_dateSelected,scale);
					}
				}
			//}
			//Focus: the ring on the selected day (an accent fill, section 3.6), with focus and keyboard cues==================
			if(IsFocusRingShown && Enabled && !rectangleSelected.IsEmpty){
				UiCalendarPaint.DrawSelectedDayFocus(g,rectangleSelected,scale);
			}
			//Hover buttons================================================================================================
			Point pointMouse=GetPointMouse();
			if(Enabled){
				if(_rectangleMonthLeft.Contains(pointMouse)){
					UiCalendarPaint.DrawHover(g,_rectangleMonthLeft,scale);
				}
				if(_rectangleMonthRight.Contains(pointMouse)){
					UiCalendarPaint.DrawHover(g,_rectangleMonthRight,scale);
				}
				if(_rectangleYearLeft.Contains(pointMouse)){
					UiCalendarPaint.DrawHover(g,_rectangleYearLeft,scale);
				}
				if(_rectangleYearRight.Contains(pointMouse)){
					UiCalendarPaint.DrawHover(g,_rectangleYearRight,scale);
				}
			}
			EnumUiColor uiColorText=Enabled ? EnumUiColor.Text : EnumUiColor.TextDisabled;
			//Header======================================================================================================
			//alignment is centered, left right, but we need to tightly control vertical because we only have a few pixels to work with.
			using StringFormat stringFormat=new StringFormat(){Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Near };
			int topCellBuffer=(int)((_heightCell-LayoutManager.ScaleMS(Font.Height))/2f);
			string strMonth=_dateMonthShowing.ToString("MMMM yyyy");
			Rectangle rectangle=new Rectangle(0,topCellBuffer+LayoutManager.Scale(11),Width,LayoutManager.Scale(_heightHeader96));
			//Hover top text: SubtleHover behind it (was blue text)===========================================================
			SizeF sizeFStrMonth=g.MeasureString(strMonth,Font);
			sizeFStrMonth=new SizeF(LayoutManager.ScaleMS(sizeFStrMonth.Width),LayoutManager.ScaleMS(sizeFStrMonth.Height));
			//Draw the rectangle based on the text size and location with some buffer around the text
			_rectangleTopText=new Rectangle(x:(Width/2-(int)sizeFStrMonth.Width/2)-LayoutManager.Scale(10),
				y:(LayoutManager.Scale(33)/2-(int)sizeFStrMonth.Height/2)-LayoutManager.Scale(5),
				width:(int)sizeFStrMonth.Width+LayoutManager.Scale(20),
				height:(int)sizeFStrMonth.Height+LayoutManager.Scale(10));
			if(_rectangleTopText.Contains(pointMouse) && AllowClickingTopText && Enabled){
				UiCalendarPaint.DrawHover(g,_rectangleTopText,scale);
			}
			if(IsTextShown){
				g.DrawString(strMonth,Font,UiTheme.GetBrush(uiColorText),rectangle,stringFormat);
			}
			//Days of week: TextSecondary=====================================================================================
			for(int i=0;i<7 && IsTextShown;i++){
				rectangle=new Rectangle((int)_xPos[i],(int)_yPos[0]+topCellBuffer,(int)widthCell,(int)_heightCell);
				string str=null;
				//First day of week is not Sunday in China, for example.  So:
				int intDay=i;
				intDay+=(int)firstDayOfWeek;
				if(intDay>6){
					intDay-=7;
				}
				str=DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName((DayOfWeek)intDay);
				g.DrawString(str,Font,UiTheme.GetBrush(Enabled ? EnumUiColor.TextSecondary : EnumUiColor.TextDisabled),rectangle,stringFormat);
			}
			//Year and month arrows: the Fluent chevrons where the triangles were, the letters Y and M beside them as before===========
			UiCalendarPaint.DrawArrow(g,GetRectangleArrow(EnumCalendarArrow.YearBack),isForward:false,uiColorText);
			UiCalendarPaint.DrawArrow(g,GetRectangleArrow(EnumCalendarArrow.MonthBack),isForward:false,uiColorText);
			UiCalendarPaint.DrawArrow(g,GetRectangleArrow(EnumCalendarArrow.MonthForward),isForward:true,uiColorText);
			UiCalendarPaint.DrawArrow(g,GetRectangleArrow(EnumCalendarArrow.YearForward),isForward:true,uiColorText);
			if(IsTextShown){
				SolidBrush brushText=UiTheme.GetBrush(uiColorText);
				g.DrawString("Y",Font,brushText,LayoutManager.ScaleF(10),LayoutManager.ScaleF(11));
				g.DrawString("M",Font,brushText,LayoutManager.ScaleF(37),LayoutManager.ScaleF(11));
				g.DrawString("M",Font,brushText,Width-LayoutManager.ScaleF(50),LayoutManager.ScaleF(11));
				g.DrawString("Y",Font,brushText,Width-LayoutManager.ScaleF(22),LayoutManager.ScaleF(11));
			}
			//Dates: Text, TextOnAccent on the selected day, TextDisabled for the months around================================
			for(int c=0;c<7 && IsTextShown;c++){
				for(int r=0;r<6;r++){
					RectangleF rectangleF=new RectangleF(_xPos[c],(int)_yPos[r+1]+topCellBuffer,
						widthCell,_heightCell);//_heightCell is sort of ignored because we are controling y
					EnumUiColor uiColorDay=UiCalendarPaint.GetDayText(isSelected:_arrayDates[c,r]==_dateSelected,
						isMonthShown:_arrayDates[c,r].Month==_dateMonthShowing.Month,Enabled);
					g.DrawString(_arrayDates[c,r].ToString("%d"),Font,UiTheme.GetBrush(uiColorDay),rectangleF,stringFormat);
				}
			}
			//Hover footer================================================================================================
			string s=Lan.g("Calendar","Today:")+" "+DateTime.Today.ToShortDateString();
			int widthBoxAndText=(int)widthCell-LayoutManager.Scale(15)+(int)LayoutManager.ScaleMS(g.MeasureString(s,Font).Width);
			if(_rectangleTodayHover.Contains(pointMouse) && Enabled){
				UiCalendarPaint.DrawHover(g,_rectangleTodayHover,scale);
			}
			//Footer: the today mark as the today ring=====================================================================
			int x=Width/2-widthBoxAndText/2;
			int heightFooter=LayoutManager.Scale(_heightFooter96);
			rectangle=new Rectangle(x,
				y:Height-heightFooter+LayoutManager.Scale(4),
				width:(int)widthCell-LayoutManager.Scale(15),
				height:heightFooter-LayoutManager.Scale(11));
				//(int)_heightCell);
			UiCalendarPaint.DrawToday(g,rectangle,isSelected:false,scale);
			x+=(int)widthCell-LayoutManager.Scale(13);
			if(IsTextShown){
				g.DrawString(s,Font,UiTheme.GetBrush(uiColorText),x,Height-heightFooter+LayoutManager.Scale(2));
			}
			//Lines: the outline, 1 px Stroke (was Gray 150)=================================================================
			//g.DrawLine(penFaint,0,Height-heightFooter,Width,Height-heightFooter);
			UiControlPaint.Outline(g,EnumUiColor.Stroke,ClientRectangle,UiTheme.GetLineWidth(scale),UiTheme.CORNER_SQUARE,scale);
		}
		#endregion Methods OnPaint

		#region Methods - Look
		///<summary>The four arrows of the header, back and forward by a year or a month.</summary>
		protected enum EnumCalendarArrow{
			YearBack,
			MonthBack,
			MonthForward,
			YearForward,
		}

		///<summary>Whether the focus ring is drawn on the selected day: with focus and keyboard cues (section 3.6). Virtual for the state tests.</summary>
		protected virtual bool IsFocusRingShown{
			get{
				return Focused && ShowFocusCues;
			}
		}

		///<summary>Whether the texts are drawn. Always true; the state tests draw the shapes alone to compare them with the design sheet.</summary>
		protected virtual bool IsTextShown{
			get{
				return true;
			}
		}

		///<summary>The mouse position in client coordinates, for the hover of the header's buttons. Virtual for the state tests.</summary>
		protected virtual Point GetPointMouse(){
			return PointToClient(Control.MousePosition);
		}

		///<summary>The day's rectangle in a column and a row of the cell grid (row 0 is the weekdays, so the dates are rows 1 to 6): the cell a little
		///narrower, as the hover, the selected day and today were always drawn. Valid after the first paint.</summary>
		protected Rectangle GetRectangleDay(int column,int rowIndex){
			float widthCell=Width/7f;
			return new Rectangle((int)_xPos[column]+LayoutManager.Scale(4),(int)_yPos[rowIndex],(int)widthCell-LayoutManager.Scale(8),(int)_heightCell);
		}

		///<summary>The 1 px Gridline under the weekdays. Valid after the first paint.</summary>
		protected Rectangle GetRectangleLineUnderDays(){
			return new Rectangle(0,(int)(_yPos[1]-1),Width,UiTheme.GetLineWidth(LayoutManager.ScaleMy()));
		}

		///<summary>The chevron's square of an arrow, centred where the triangle was drawn before.</summary>
		protected Rectangle GetRectangleArrow(EnumCalendarArrow calendarArrow){
			float scale=LayoutManager.ScaleMy();
			PointF pointCentre;
			switch(calendarArrow){
				case EnumCalendarArrow.YearBack:
					pointCentre=new PointF(LayoutManager.ScaleF(6),LayoutManager.ScaleF(17));
					break;
				case EnumCalendarArrow.MonthBack:
					pointCentre=new PointF(LayoutManager.ScaleF(32),LayoutManager.ScaleF(17));
					break;
				case EnumCalendarArrow.MonthForward:
					pointCentre=new PointF(Width-LayoutManager.ScaleF(33),LayoutManager.ScaleF(17));
					break;
				default:
					pointCentre=new PointF(Width-LayoutManager.ScaleF(7),LayoutManager.ScaleF(17));
					break;
			}
			return UiCalendarPaint.GetGlyphRectangle(pointCentre,scale);
		}
		#endregion Methods - Look

		#region Methods - Public
		///<summary>Use this in place of MonthCalendar.SelectionStart.</summary>
		public DateTime GetDateSelected(){
			return _dateSelected;
		}

		public Size GetDefaultSize(){
			return DefaultSize;
		}

		///<summary>Use this in place of MonthCalendar.SetDate().</summary>
		public void SetDateSelected(DateTime date){
			_dateSelected=date;
			_dateMonthShowing=new DateTime(_dateSelected.Year,_dateSelected.Month,1);
			Invalidate();
		}
		#endregion Methods - Public

		#region Methods - Private
		private void CalculateRectangles(){
			_rectangleYearLeft=new Rectangle(0,0,LayoutManager.Scale(26),LayoutManager.Scale(33));
			_rectangleMonthLeft=new Rectangle(LayoutManager.Scale(26),0,LayoutManager.Scale(26),LayoutManager.Scale(33));
			_rectangleMonthRight=new Rectangle(Width-LayoutManager.Scale(52),0,LayoutManager.Scale(26),LayoutManager.Scale(33));
			_rectangleYearRight=new Rectangle(Width-LayoutManager.Scale(26),0,LayoutManager.Scale(26),LayoutManager.Scale(33));
			float widthCell=Width/7f;
			string s=Lan.g("Calendar","Today:")+" "+DateTime.Today.ToShortDateString();
			int widthBoxAndText=(int)widthCell-LayoutManager.Scale(15)+TextRenderer.MeasureText(s,Font).Width;
			int widthTodayHover=widthBoxAndText+LayoutManager.Scale(10);
			int heightFooter=LayoutManager.Scale(_heightFooter96);
			_rectangleTodayHover=new Rectangle(Width/2-widthTodayHover/2,
				y:Height-heightFooter,
				width:widthTodayHover,
				height:heightFooter);
		}
		#endregion Methods - Private

	}
}
