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
	public partial class MonthCalendarOD : Control,IUiDropCalendar{
		#region Fields - Public
		///<summary>Set this to false to prevent user from clicking the top text to manually set date. Useful, for example, if this monthCalendar is a dropdown, so there's already a box for that.</summary>
		public bool AllowClickingTopText=true;
		///<summary>UI-CAL: the keyboard of Windows' calendar: the arrows move the selected day by a day or a week, Page Up and Page Down by a month,
		///Home and End to the first and last day of its month, with Shift they extend the range (MaxSelectionCount above 1). Off by default, so the
		///appointment module's calendar and ODDatePicker's keep their behaviour; the calendars that replaced Windows' ones and the date pickers'
		///drop-down turn it on.</summary>
		public bool AllowKeyboard=false;
		public LayoutManagerForms LayoutManager=new LayoutManagerForms();
		#endregion Fields - Public

		#region Fields -Private
		///<summary>The dates that are showing in each cell.  This is a 7x6 array.</summary>
		private DateTime[,] _arrayDates;
		///<summary>UI-CAL: the day the range was started from (the click, or the selection before Shift), which a drag or Shift extends from.</summary>
		private DateTime _dateAnchor;
		///<summary>UI-CAL: the day the keyboard and a drag move (the end of the range away from the anchor), drawn with the focus ring.</summary>
		private DateTime _dateCursor;
		///<summary>UI-CAL: the selectable range (MinDate, MaxDate); every day by default.</summary>
		private DateTime _dateMax=DateTime.MaxValue.Date;
		private DateTime _dateMin=DateTime.MinValue;
		///<summary>Unfortunately, this has the same name as a MS event for selecting a date.  So this can't be used publicly unless I'm careful.
		///UI-CAL: the first day of the selection (SelectionStart); _dateSelectionEnd its last, the same day unless MaxSelectionCount is above 1.</summary>
		private DateTime _dateSelected;
		private DateTime _dateSelectionEnd;
		///<summary>UI-CAL: a left button down on a day: the selection follows the mouse (MaxSelectionCount above 1) and DateSelected is raised on the
		///button's release, as Windows' calendar does.</summary>
		private bool _isMouseSelecting;
		private int _maxSelectionCount=1;
		///<summary>UI-CAL-2: the font the text is drawn in (GetFontDraw), in pixels at the calendar's scale; made again when the size changes.</summary>
		private Font _fontDraw;
		///<summary>UI-CAL-2: see FontSize96.</summary>
		private float _fontSize96;
		private float _heightCell;
		///<summary>Needs to be scaled each time.</summary>
		private int _heightFooter96=21;
		///<summary>Needs to be scaled each time.</summary>
		private int _heightHeader96=34;
		///<summary>Because we can go to different months without changing the selected date. We always use the first day of the month. Can't be changed externally.</summary>
		private DateTime _dateMonthShowing;
		///<summary>Identifies the current hot hover cell as the mouse moves around.  Row 0 is never used because that's the days of week. 0,0 indicates no hot.</summary>
		private Point _pointHotHover;
		///<summary>The footer's today mark, as last drawn (read by the calendar state tests, tests/UiRefreshTests/TextCalendarStates.cs).</summary>
		private Rectangle _rectangleFooterMark;
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
			_dateSelectionEnd=DateTime.Today;
			_dateAnchor=DateTime.Today;
			_dateCursor=DateTime.Today;
			_dateMonthShowing=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
			CalculateRectangles();
		}
		#endregion Constructor

		#region Events - Public Raise
		///<summary>Occurs when user clicks to change date.  Does not fire in response to programmatic data changes.</summary>
		[Category("OD")]
		[Description("Occurs when user clicks to change date.  Does not fire in response to programmatic data changes.")]
		public event EventHandler DateChanged;

		///<summary>UI-CAL: as Windows' MonthCalendar.DateSelected: the user chose a day explicitly, on the mouse button's release over a day (or
		///after a drag over several), on Today, or a date typed after clicking the month's name. Not the keyboard's moves (they raise DateChanged).
		///Does not fire in response to programmatic changes.</summary>
		[Category("OD")]
		[Description("Occurs when the user chooses a day with the mouse.  Does not fire in response to programmatic data changes.")]
		public event DateRangeEventHandler DateSelected;
		#endregion Events - Public Raise

		#region Properties
		protected override Size DefaultSize => new Size(227, 162);

		///<summary>UI-CAL-2: the size in points of the calendar's text at 96 dpi and 100% zoom, which the calendar scales by its LayoutManager as it
		///scales its cells (GetFontDraw). 0 (the default): the size the window's LayoutManager recorded for the calendar (its designer font), else its
		///Font as LayoutManager sets the fonts of a window's controls (scaled by the zoom only). The date pickers' drop-down sets it from the picker
		///(LayoutManagerForms.CreateDropCalendar).</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public float FontSize96{
			get{
				return _fontSize96;
			}
			set{
				_fontSize96=value;
				CalculateRectangles();
				Invalidate();
			}
		}

		///<summary>UI-CAL: as Windows' MonthCalendar.MaxSelectionCount: the most days the selection can span. 1 (one day) by default, unlike
		///Windows' 7, so the existing uses keep one day.</summary>
		[Category("OD")]
		[DefaultValue(1)]
		public int MaxSelectionCount{
			get{
				return _maxSelectionCount;
			}
			set{
				if(value<1){
					throw new ArgumentOutOfRangeException(nameof(MaxSelectionCount));
				}
				_maxSelectionCount=value;
				SetSelection(_dateSelected,_dateSelectionEnd);
			}
		}

		///<summary>UI-CAL: as Windows' MonthCalendar.MaxDate: no day after it can be selected (drawn as the other months' days).</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DateTime MaxDate{
			get{
				return _dateMax;
			}
			set{
				_dateMax=value.Date;
				if(_dateMin>_dateMax){
					_dateMin=_dateMax;
				}
				SetSelection(_dateSelected,_dateSelectionEnd);
			}
		}

		///<summary>UI-CAL: as Windows' MonthCalendar.MinDate: no day before it can be selected (drawn as the other months' days). A selection
		///before it moves to it.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DateTime MinDate{
			get{
				return _dateMin;
			}
			set{
				_dateMin=value.Date;
				if(_dateMax<_dateMin){
					_dateMax=_dateMin;
				}
				SetSelection(_dateSelected,_dateSelectionEnd);
			}
		}

		///<summary>UI-CAL: as Windows' MonthCalendar.SelectionEnd: the last day of the selection. A day before SelectionStart moves the start to it;
		///a range longer than MaxSelectionCount moves the start up to it.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DateTime SelectionEnd{
			get{
				return _dateSelectionEnd;
			}
			set{
				DateTime dateStart=_dateSelected;
				if(value.Date<dateStart){
					dateStart=value.Date;
				}
				if((value.Date-dateStart).TotalDays>=_maxSelectionCount){
					dateStart=value.Date.AddDays(-(_maxSelectionCount-1));
				}
				SetSelection(dateStart,value.Date);
			}
		}

		///<summary>UI-CAL: as Windows' MonthCalendar.SelectionRange.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public SelectionRange SelectionRange{
			get{
				return new SelectionRange(_dateSelected,_dateSelectionEnd);
			}
			set{
				SetSelectionRange(value.Start,value.End);
			}
		}

		///<summary>UI-CAL: as Windows' MonthCalendar.SelectionStart: the first day of the selection (the selected day). A day after SelectionEnd
		///moves the end to it; a range longer than MaxSelectionCount moves the end back to it.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DateTime SelectionStart{
			get{
				return _dateSelected;
			}
			set{
				DateTime dateEnd=_dateSelectionEnd;
				if(value.Date>dateEnd){
					dateEnd=value.Date;
				}
				if((dateEnd-value.Date).TotalDays>=_maxSelectionCount){
					dateEnd=value.Date.AddDays(_maxSelectionCount-1);
				}
				SetSelection(value.Date,dateEnd);
			}
		}
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
			if(AllowKeyboard && !Focused && CanFocus){
				Focus();//UI-CAL: as Windows' calendar, a click gives it the keyboard
			}
			if(_pointHotHover!=new Point(0,0)){
				DateTime dateClicked=_arrayDates[_pointHotHover.X,_pointHotHover.Y-1];
				//UI-CAL: a day outside MinDate and MaxDate is not selectable, as in Windows' calendar.
				if(IsSelectable(dateClicked)){
					if(_maxSelectionCount>1 && (ModifierKeys & Keys.Shift)==Keys.Shift){
						//Shift extends the range from the anchor, as in Windows' calendar.
						SetRangeFromAnchor(dateClicked);
					}
					else{
						_dateSelected=dateClicked;
						_dateSelectionEnd=dateClicked;
						_dateAnchor=dateClicked;
						_dateCursor=dateClicked;
					}
					_isMouseSelecting=true;
					DateChanged?.Invoke(this,new EventArgs());
					if(dateClicked.Month!=_dateMonthShowing.Month){
						_dateMonthShowing=new DateTime(dateClicked.Year,dateClicked.Month,1);
					}
					Invalidate();
				}
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
					_dateSelectionEnd=_dateSelected;
					_dateAnchor=_dateSelected;
					_dateCursor=_dateSelected;
					if(_dateSelected.Month!=_dateMonthShowing.Month || _dateSelected.Year!=_dateMonthShowing.Year){
						_dateMonthShowing=new DateTime(_dateSelected.Year,_dateSelected.Month,1);
					}
					DateChanged?.Invoke(this,new EventArgs());
					DateSelected?.Invoke(this,new DateRangeEventArgs(_dateSelected,_dateSelectionEnd));
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
			if(_rectangleTodayHover.Contains(e.Location) && IsSelectable(DateTime.Today)){
				_dateSelected=DateTime.Today;
				_dateSelectionEnd=DateTime.Today;
				_dateAnchor=DateTime.Today;
				_dateCursor=DateTime.Today;
				_isMouseSelecting=true;
				DateChanged?.Invoke(this,new EventArgs());
				if(DateTime.Today.Month!=_dateMonthShowing.Month){
					_dateMonthShowing=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
				}
				Invalidate();
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e){
			//UI-CAL: the explicit choice, as Windows' calendar raises it: on the release of the button that selected a day or Today.
			if(_isMouseSelecting){
				_isMouseSelecting=false;
				DateSelected?.Invoke(this,new DateRangeEventArgs(_dateSelected,_dateSelectionEnd));
			}
			base.OnMouseUp(e);
		}

		protected override void OnMouseCaptureChanged(EventArgs e){
			_isMouseSelecting=false;
			base.OnMouseCaptureChanged(e);
		}

		protected override bool IsInputKey(Keys keyData){
			if(AllowKeyboard){
				switch(keyData & ~Keys.Shift){
					case Keys.Left:
					case Keys.Right:
					case Keys.Up:
					case Keys.Down:
					case Keys.PageUp:
					case Keys.PageDown:
					case Keys.Home:
					case Keys.End:
						return true;
				}
			}
			return base.IsInputKey(keyData);
		}

		protected override void OnKeyDown(KeyEventArgs e){
			base.OnKeyDown(e);
			if(!AllowKeyboard || e.Handled || e.Alt || e.Control){
				return;
			}
			DateTime dateTo;
			switch(e.KeyCode){
				case Keys.Left:
					dateTo=AddDaysSafe(_dateCursor,-1);
					break;
				case Keys.Right:
					dateTo=AddDaysSafe(_dateCursor,1);
					break;
				case Keys.Up:
					dateTo=AddDaysSafe(_dateCursor,-7);
					break;
				case Keys.Down:
					dateTo=AddDaysSafe(_dateCursor,7);
					break;
				case Keys.PageUp:
					dateTo=_dateCursor.Date>DateTime.MinValue.AddMonths(1) ? _dateCursor.AddMonths(-1) : _dateCursor;
					break;
				case Keys.PageDown:
					dateTo=_dateCursor.Date<DateTime.MaxValue.AddMonths(-1) ? _dateCursor.AddMonths(1) : _dateCursor;
					break;
				case Keys.Home:
					dateTo=new DateTime(_dateCursor.Year,_dateCursor.Month,1);
					break;
				case Keys.End:
					dateTo=new DateTime(_dateCursor.Year,_dateCursor.Month,DateTime.DaysInMonth(_dateCursor.Year,_dateCursor.Month));
					break;
				default:
					return;
			}
			e.Handled=true;
			MoveCursor(dateTo,isExtending:e.Shift);
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
			//UI-CAL: a drag over the days selects the range from the day pressed, as in Windows' calendar (MaxSelectionCount above 1).
			if(_isMouseSelecting && _maxSelectionCount>1 && e.Button==MouseButtons.Left && hotRow!=0 && _arrayDates!=null){
				DateTime dateHover=_arrayDates[hotCol,hotRow-1];
				if(IsSelectable(dateHover) && dateHover!=_dateCursor){
					SetRangeFromAnchor(dateHover);
					DateChanged?.Invoke(this,new EventArgs());
				}
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
			if(_pointHotHover!=new Point() && Enabled && IsSelectable(_arrayDates[_pointHotHover.X,_pointHotHover.Y-1])){
				UiCalendarPaint.DrawHover(g,GetRectangleDay(_pointHotHover.X,_pointHotHover.Y),scale);
			}
			//Date selected: Accent with 4 px corners=========================================================================
			//if(_monthShowing.Month==_dateSelected.Month){//date is showing. Can't do this.  Could be showing dimmed.
			//UI-CAL: every day of a range (MaxSelectionCount above 1) as the selected day; the focus ring on the day the keyboard moves.
			Rectangle rectangleSelected=Rectangle.Empty;
			for(int c=0;c<7;c++){
				for(int r=0;r<6;r++){
					if(!IsDaySelected(_arrayDates[c,r])){
						continue;
					}
					Rectangle rectangleDay=GetRectangleDay(c,r+1);
					if(rectangleSelected.IsEmpty || _arrayDates[c,r]==_dateCursor.Date){
						rectangleSelected=rectangleDay;
					}
					UiCalendarPaint.DrawSelectedDay(g,rectangleDay,Enabled,scale);
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
						UiCalendarPaint.DrawToday(g,GetRectangleDay(c,r+1),isSelected:IsDaySelected(_arrayDates[c,r]),scale);
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
			//UI-CAL-2: the text in pixels at the calendar's scale (GetFontDraw), so it grows with the cells by the same factor; measured as drawn.
			Font fontDraw=GetFontDraw();
			//Header======================================================================================================
			//alignment is centered, left right, but we need to tightly control vertical because we only have a few pixels to work with.
			using StringFormat stringFormat=new StringFormat(){Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Near };
			int topCellBuffer=(int)((_heightCell-GetFontHeight(fontDraw))/2f);
			string strMonth=_dateMonthShowing.ToString("MMMM yyyy");
			Rectangle rectangle=new Rectangle(0,topCellBuffer+LayoutManager.Scale(11),Width,LayoutManager.Scale(_heightHeader96));
			//Hover top text: SubtleHover behind it (was blue text)===========================================================
			SizeF sizeFStrMonth=g.MeasureString(strMonth,fontDraw);
			//Draw the rectangle based on the text size and location with some buffer around the text
			_rectangleTopText=new Rectangle(x:(Width/2-(int)sizeFStrMonth.Width/2)-LayoutManager.Scale(10),
				y:(LayoutManager.Scale(33)/2-(int)sizeFStrMonth.Height/2)-LayoutManager.Scale(5),
				width:(int)sizeFStrMonth.Width+LayoutManager.Scale(20),
				height:(int)sizeFStrMonth.Height+LayoutManager.Scale(10));
			if(_rectangleTopText.Contains(pointMouse) && AllowClickingTopText && Enabled){
				UiCalendarPaint.DrawHover(g,_rectangleTopText,scale);
			}
			if(IsTextShown){
				g.DrawString(strMonth,fontDraw,UiTheme.GetBrush(uiColorText),rectangle,stringFormat);
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
				g.DrawString(str,fontDraw,UiTheme.GetBrush(Enabled ? EnumUiColor.TextSecondary : EnumUiColor.TextDisabled),rectangle,stringFormat);
			}
			//Year and month arrows: the Fluent chevrons where the triangles were, the letters Y and M beside them as before===========
			UiCalendarPaint.DrawArrow(g,GetRectangleArrow(EnumCalendarArrow.YearBack),isForward:false,uiColorText);
			UiCalendarPaint.DrawArrow(g,GetRectangleArrow(EnumCalendarArrow.MonthBack),isForward:false,uiColorText);
			UiCalendarPaint.DrawArrow(g,GetRectangleArrow(EnumCalendarArrow.MonthForward),isForward:true,uiColorText);
			UiCalendarPaint.DrawArrow(g,GetRectangleArrow(EnumCalendarArrow.YearForward),isForward:true,uiColorText);
			if(IsTextShown){
				SolidBrush brushText=UiTheme.GetBrush(uiColorText);
				g.DrawString("Y",fontDraw,brushText,LayoutManager.ScaleF(10),LayoutManager.ScaleF(11));
				g.DrawString("M",fontDraw,brushText,LayoutManager.ScaleF(37),LayoutManager.ScaleF(11));
				g.DrawString("M",fontDraw,brushText,Width-LayoutManager.ScaleF(50),LayoutManager.ScaleF(11));
				g.DrawString("Y",fontDraw,brushText,Width-LayoutManager.ScaleF(22),LayoutManager.ScaleF(11));
			}
			//Dates: Text, TextOnAccent on the selected day, TextDisabled for the months around================================
			for(int c=0;c<7 && IsTextShown;c++){
				for(int r=0;r<6;r++){
					RectangleF rectangleF=new RectangleF(_xPos[c],(int)_yPos[r+1]+topCellBuffer,
						widthCell,_heightCell);//_heightCell is sort of ignored because we are controling y
					//UI-CAL: a day outside MinDate and MaxDate as the other months' days (it cannot be selected).
					EnumUiColor uiColorDay=UiCalendarPaint.GetDayText(isSelected:IsDaySelected(_arrayDates[c,r]),
						isMonthShown:_arrayDates[c,r].Month==_dateMonthShowing.Month && IsSelectable(_arrayDates[c,r]),Enabled);
					g.DrawString(_arrayDates[c,r].ToString("%d"),fontDraw,UiTheme.GetBrush(uiColorDay),rectangleF,stringFormat);
				}
			}
			//Hover footer================================================================================================
			string s=Lan.g("Calendar","Today:")+" "+DateTime.Today.ToShortDateString();
			int widthBoxAndText=(int)widthCell-LayoutManager.Scale(15)+(int)g.MeasureString(s,fontDraw).Width;
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
			_rectangleFooterMark=rectangle;
			UiCalendarPaint.DrawToday(g,rectangle,isSelected:false,scale);
			x+=(int)widthCell-LayoutManager.Scale(13);
			if(IsTextShown){
				g.DrawString(s,fontDraw,UiTheme.GetBrush(uiColorText),x,Height-heightFooter+LayoutManager.Scale(2));
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

		///<summary>UI-CAL-2: the font the calendar draws its text in: its font's size at 96 dpi (GetFontSize96Drawn) times its scale (LayoutManager.ScaleMy:
		///the monitor's dpi times the zoom), in pixels. The text grows with the cells by the same factor on every monitor and in a picture: a font in
		///points grows with the graphics' dpi instead, Windows' system dpi on a screen (not the dpi of the monitor the window is on) and 96 in a
		///picture, so the 150% calendar drew 100% text in the gallery and a plain window's drop-down drew 150% text in a 100% calendar.</summary>
		protected Font GetFontDraw(){
			float sizePixels=GetFontSize96Drawn()*96f/72f*LayoutManager.ScaleMy();
			Font font=Font;
			if(_fontDraw!=null && _fontDraw.Size==sizePixels && _fontDraw.Style==font.Style && _fontDraw.Name==font.Name){
				return _fontDraw;
			}
			_fontDraw?.Dispose();
			_fontDraw=new Font(font.FontFamily,sizePixels,font.Style,GraphicsUnit.Pixel);
			return _fontDraw;
		}

		///<summary>UI-CAL-2: the size in points at 96 dpi and 100% zoom the text is drawn at (see FontSize96).</summary>
		public float GetFontSize96Drawn(){
			if(_fontSize96>0){
				return _fontSize96;
			}
			float fontSize96=LayoutManager.GetFontSize96(this);
			if(fontSize96>0){
				return fontSize96;
			}
			return Font.SizeInPoints/LayoutManager.ScaleMyFont();
		}

		///<summary>UI-CAL-2: the height of a line of the font in pixels, rounded up as Font.Height rounds it (at 100% the same as Font.Height at 96 dpi).</summary>
		private static int GetFontHeight(Font fontDraw){
			return (int)Math.Ceiling(fontDraw.GetHeight(96f));
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

		///<summary>UI-CAL: the day in a column and a row of the dates (row 0 is the first row of dates, GetRectangleDay's row 1). Valid after the
		///first paint. For the behaviour tests.</summary>
		protected DateTime GetDateInCell(int column,int row){
			return _arrayDates[column,row];
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
			_dateSelectionEnd=date;
			_dateAnchor=date;
			_dateCursor=date;
			_dateMonthShowing=new DateTime(_dateSelected.Year,_dateSelected.Month,1);
			Invalidate();
		}

		///<summary>UI-CAL: as Windows' MonthCalendar.SetDate: selects the one day (moved into MinDate and MaxDate) and shows its month. Raises no
		///event.</summary>
		public void SetDate(DateTime date){
			SetSelection(date.Date,date.Date);
		}

		///<summary>UI-CAL: as Windows' MonthCalendar.SetSelectionRange: the range from the earlier day to the later, cut to MaxSelectionCount days
		///from the earlier and moved into MinDate and MaxDate. Raises no event.</summary>
		public void SetSelectionRange(DateTime date1,DateTime date2){
			DateTime dateStart=date1.Date<=date2.Date ? date1.Date : date2.Date;
			DateTime dateEnd=date1.Date<=date2.Date ? date2.Date : date1.Date;
			SetSelection(dateStart,dateEnd);
		}
		#endregion Methods - Public

		#region Methods - Selection
		///<summary>UI-CAL: whether the day is in MinDate and MaxDate.</summary>
		private bool IsSelectable(DateTime date){
			return date.Date>=_dateMin && date.Date<=_dateMax;
		}

		///<summary>UI-CAL: whether the day (a cell's date, midnight) is in the selection. With one day, as before: the cell's date equals the selected
		///date.</summary>
		private bool IsDaySelected(DateTime date){
			return date>=_dateSelected && date<=_dateSelectionEnd;
		}

		private static DateTime AddDaysSafe(DateTime date,int days){
			if(days<0 && (date-DateTime.MinValue).TotalDays<-days){
				return date;
			}
			if(days>0 && (DateTime.MaxValue-date).TotalDays<days+1){
				return date;
			}
			return date.AddDays(days);
		}

		private static DateTime Clamp(DateTime date,DateTime dateMin,DateTime dateMax){
			if(date.Date<dateMin){
				return dateMin;
			}
			if(date.Date>dateMax){
				return dateMax;
			}
			return date;
		}

		///<summary>UI-CAL: sets the selection from dateStart to dateEnd (dateEnd not before dateStart), cut to MaxSelectionCount days from the start,
		///moved into MinDate and MaxDate, and shows the month of its start when neither end's month is showing; the anchor is its start, the
		///cursor its end. A programmatic change: raises no event, as Windows' calendar raises none the program listens to here.</summary>
		private void SetSelection(DateTime dateStart,DateTime dateEnd){
			dateStart=Clamp(dateStart,_dateMin,_dateMax);
			dateEnd=Clamp(dateEnd,_dateMin,_dateMax);
			if(dateEnd<dateStart){
				dateEnd=dateStart;
			}
			if((dateEnd.Date-dateStart.Date).TotalDays>=_maxSelectionCount){
				dateEnd=dateStart.AddDays(_maxSelectionCount-1);
			}
			_dateSelected=dateStart;
			_dateSelectionEnd=dateEnd;
			_dateAnchor=dateStart;
			_dateCursor=dateEnd;
			bool isStartShowing=dateStart.Year==_dateMonthShowing.Year && dateStart.Month==_dateMonthShowing.Month;
			bool isEndShowing=dateEnd.Year==_dateMonthShowing.Year && dateEnd.Month==_dateMonthShowing.Month;
			if(!isStartShowing && !isEndShowing){
				_dateMonthShowing=new DateTime(dateStart.Year,dateStart.Month,1);
			}
			Invalidate();
		}

		///<summary>UI-CAL: the range from the anchor to the day (Shift with a click or a key, a drag), at most MaxSelectionCount days: the anchor
		///stays, the day is the cursor. Raises no event.</summary>
		private void SetRangeFromAnchor(DateTime date){
			DateTime dateAnchor=_dateAnchor.Date;
			int daysMax=_maxSelectionCount-1;
			if((date.Date-dateAnchor).TotalDays>daysMax){
				date=dateAnchor.AddDays(daysMax);
			}
			if((dateAnchor-date.Date).TotalDays>daysMax){
				date=dateAnchor.AddDays(-daysMax);
			}
			date=Clamp(date.Date,_dateMin,_dateMax);
			_dateSelected=date<dateAnchor ? date : dateAnchor;
			_dateSelectionEnd=date<dateAnchor ? dateAnchor : date;
			_dateCursor=date;
			Invalidate();
		}

		///<summary>UI-CAL: the keyboard's move to the day: with Shift (and MaxSelectionCount above 1) the range from the anchor, else the one day.
		///Shows the day's month and raises DateChanged when the selection changed.</summary>
		private void MoveCursor(DateTime date,bool isExtending){
			date=Clamp(date.Date,_dateMin,_dateMax);
			DateTime dateStartOld=_dateSelected;
			DateTime dateEndOld=_dateSelectionEnd;
			if(isExtending && _maxSelectionCount>1){
				SetRangeFromAnchor(date);
			}
			else{
				_dateSelected=date;
				_dateSelectionEnd=date;
				_dateAnchor=date;
				_dateCursor=date;
			}
			if(_dateCursor.Year!=_dateMonthShowing.Year || _dateCursor.Month!=_dateMonthShowing.Month){
				_dateMonthShowing=new DateTime(_dateCursor.Year,_dateCursor.Month,1);
			}
			Invalidate();
			if(_dateSelected!=dateStartOld || _dateSelectionEnd!=dateEndOld){
				DateChanged?.Invoke(this,new EventArgs());
			}
		}
		#endregion Methods - Selection

		#region Methods - Private
		private void CalculateRectangles(){
			_rectangleYearLeft=new Rectangle(0,0,LayoutManager.Scale(26),LayoutManager.Scale(33));
			_rectangleMonthLeft=new Rectangle(LayoutManager.Scale(26),0,LayoutManager.Scale(26),LayoutManager.Scale(33));
			_rectangleMonthRight=new Rectangle(Width-LayoutManager.Scale(52),0,LayoutManager.Scale(26),LayoutManager.Scale(33));
			_rectangleYearRight=new Rectangle(Width-LayoutManager.Scale(26),0,LayoutManager.Scale(26),LayoutManager.Scale(33));
			float widthCell=Width/7f;
			string s=Lan.g("Calendar","Today:")+" "+DateTime.Today.ToShortDateString();
			int widthBoxAndText=(int)widthCell-LayoutManager.Scale(15)+TextRenderer.MeasureText(s,GetFontDraw()).Width;
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
