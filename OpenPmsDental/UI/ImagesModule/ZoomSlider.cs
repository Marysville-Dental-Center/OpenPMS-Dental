#region using
using CodeBase;
using OpenPmsDentalBusiness;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
#endregion using

namespace OpenPmsDental.UI{
	public partial class ZoomSlider : Control{
		#region Fields
		public LayoutManagerForms LayoutManager=new LayoutManagerForms();
		///<summary>This is the zoom level that's calculated in SetValueInitialFit and then constantly used to derive other values like _maximum.</summary>
		private int _fit;
		private Font _font=new Font("Segoe UI",9f);
		private bool _textHitEsc;
		///<summary>Mouse down was on slider. Either a drag or a click.</summary>
		private bool _isMouseDownOnSlider;
		private bool _isMouseDown;
		///<summary>This changes constantly and is not under external control.  It starts out as twice _fit, but then increases more each time user hits "plus".</summary>
		private int _maximum=100;
		///<summary>Current position of mouse on control.  -1,-1 indicates not over control.</summary>
		private Point _pointMouse=new Point(-1,-1);
		///<summary>Point where mouse was initially down.  If dragging, it will get the original position.</summary>
		private Point _pointMouseDown;
		///<summary>The rectangle defining the Fit button</summary>
		private Rectangle _rectangleFit;
		///<summary>The rectangle defining the minus button</summary>
		private Rectangle _rectangleMinus;
		///<summary>The rectangle defining the plus button</summary>
		private Rectangle _rectanglePlus;
		///<summary>The rectangle defining the slider that contains the current zoom text.  Always falls within rectangleTickline.  Since user can only drag in whole pixel increments, we won't use a RectangleF.</summary>
		private Rectangle _rectangleSlider;
		///<summary>The rectangle defining tick line itself.  We never draw a rectangle around it, but it's useful for lots of math. It spans the height of the entire control. It extends left and right to the extent of the tick line.</summary>
		private Rectangle _rectangleTickLine;
		///<summary>The rectangle defining the tickline plus left and right padding. So the entire cell.</summary>
		private Rectangle _rectangleTickLineMargin;
		///<summary>The rectangle defining the 100 button</summary>
		private Rectangle _rectangle100;
		private TextBox textBoxEdit;
		private TimeSpan _timeMouseDown;
		///<summary>When mouse down to start dragging slider, we aren't necessarily in the center.  This keeps track of offset.</summary>
		private int _xOffsetMouseDownSlider;
		///<summary>Property backer.  Internally, this must be a float for better accuracy.  We round it for viewing and for the public property</summary>
		private float _value=1;
		#endregion Fields

		#region Constructor
		///<summary></summary>
		public ZoomSlider(){
			InitializeComponent();
			textBoxEdit=new TextBox();
			textBoxEdit.Font=new Font("Segoe UI",9f);//gets resized automatically by LayoutManager
			textBoxEdit.Visible=false;
			textBoxEdit.LostFocus += TextBoxEdit_LostFocus;
			textBoxEdit.KeyDown += TextBoxEdit_KeyDown;
			textBoxEdit.KeyUp += TextBoxEdit_KeyUp;
			textBoxEdit.PreviewKeyDown += TextBoxEdit_PreviewKeyDown;
			Controls.Add(textBoxEdit);
			DoubleBuffered=true;
		}
		#endregion Constructor

		#region Properties
		///<summary>The numeric zoom value, greater than zero.  To set, use SetValueInitialFit or SetValueAndMax method.</summary>
		[Browsable(false)]
		public int Value {
			get {
				if(Round(_value)<1){
					return 1;
				}
				return Round(_value);
			}
			//set { 
			//	_value=value;
			//	this.Invalidate();
			//}
		}

		protected override Size DefaultSize {
			get {
				return new Size(200,25);
				//heavily optimized for about 25 to 26px high, or equivalently scaled
			}
		}
		#endregion Properties

		#region Events
		///<summary></summary>
		[Category("OD")]
		[Description("Occurs when user presses Fit (or 100) button because parent form might need to recenter image.")]
		public event EventHandler FitPressed=null;

		///<summary></summary>
		[Category("OD")]
		[Description("Occurs when user changes the zoom level.  It fires repeatedly while dragging.")]
		public event EventHandler Zoomed=null;
		#endregion Events

		#region Methods - Event Handlers
		private void TextBoxEdit_KeyDown(object sender, KeyEventArgs e){
			if(e.KeyCode==Keys.Enter){
				try{
					SetValueAndMax(int.Parse(textBoxEdit.Text));
				}
				catch{}
				textBoxEdit.Visible=false;
			}
			Zoomed?.Invoke(this, new EventArgs());
		}

		private void TextBoxEdit_KeyUp(object sender, KeyEventArgs e){
			int widthText=TextRenderer.MeasureText(textBoxEdit.Text,textBoxEdit.Font).Width;
			if(textBoxEdit.Width<widthText+6){
				textBoxEdit.Width=widthText+6;
				//textBoxEdit.ScrollToCaret();
			}
			Invalidate();
		}

		private void TextBoxEdit_LostFocus(object sender, EventArgs e){
			if(!_textHitEsc){
				try{
					SetValueAndMax(int.Parse(textBoxEdit.Text));
				}
				catch{
				}
			}
			textBoxEdit.Visible=false;
			Invalidate();
			Zoomed?.Invoke(this, new EventArgs());
		}

		private void TextBoxEdit_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e){
			if(e.KeyCode==Keys.Escape){
				_textHitEsc=true;
				textBoxEdit.Visible=false;
			}
			Invalidate();
		}
		#endregion Methods - Event Handlers

		#region Methods - Override On...
		protected override void OnPaint(PaintEventArgs pe){
			base.OnPaint(pe);
			if(_maximum<5){
				return;
			}
			Graphics g=pe.Graphics;
			g.SmoothingMode=SmoothingMode.HighQuality;
			g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
			if(DesignMode){
				//because the first DrawLine keeps crashing otherwise
				g.DrawRectangle(UiTheme.GetPen(EnumUiColor.StrokeStrong),0,0,Width-1,Height-1);
				StringFormat format=new StringFormat();
				format.Alignment=StringAlignment.Center;
				format.LineAlignment=StringAlignment.Center;
				g.DrawString(this.Name,Font,UiTheme.GetBrush(EnumUiColor.Text),new Rectangle(0,0,Width,Height),format);
				return;
			}
			//Calculate a few rectangles
			using Font fontForMeasure=new Font(_font.FontFamily,LayoutManager.ScaleMS(_font.Size));
			int widthOne=(int)g.MeasureString("1",fontForMeasure).Width+2;//because "1" is the minimum value
			//_maximum won't change while mouse is down because drag can't take _value to larger than _maximum
			int widthMax=(int)g.MeasureString(_maximum.ToString(),fontForMeasure).Width+2;
			int widthValue=(int)g.MeasureString(Round(_value).ToString(),fontForMeasure).Width+2;
			int bufferLeft=widthOne/2;
			int bufferRight=widthMax/2;
			_rectangleTickLine=new Rectangle(_rectangleTickLineMargin.Left+bufferLeft,0,_rectangleTickLineMargin.Width-bufferLeft-bufferRight,Height);
			//xVal is the x position on the slider, in pixels, of _value.
			int xValuePixels=_rectangleTickLine.Left
				+(int)(((float)_rectangleTickLine.Right-_rectangleTickLine.Left)//how many pixels in tick line
				/(_maximum-1) *_value);
			_rectangleSlider=new Rectangle(xValuePixels-(widthValue/2),LayoutManager.Scale(9),widthValue,Height-LayoutManager.Scale(11));
			//The new look (UI-4c-1, docs/specs/ui-refresh-plan.md section 3.6; docs/port-notes/UI-4c.md): a toolbar strip (SurfaceSubtle with a 1 px
			//Gridline at the bottom, was a gradient); the minus, plus, Fit and 100 cells as toolbar buttons (SubtleHover under the mouse, SubtlePressed
			//pressed, 4 px corners, were outlines and a gradient); the dividers 1 px Stroke; the minus and plus Fluent glyphs; the tick line StrokeStrong;
			//the value box an input (SurfaceRaised with a StrokeStrong outline, StrokeStrongHover under the mouse, Accent while dragged); the texts and the
			//pointer Text (TextDisabled when disabled). Every rectangle and text where it was.
			float scale=LayoutManager.ScaleMy();
			int widthLine=UiTheme.GetLineWidth(scale);
			g.Clear(UiTheme.GetColor(EnumUiColor.SurfaceSubtle));
			EnumUiColor uiColorText=UiSliderPaint.GetText(Enabled);
			Rectangle[] arrayRectanglesCells=new Rectangle[] { _rectangleMinus,_rectanglePlus,_rectangleFit,_rectangle100 };
			for(int i=0;i<arrayRectanglesCells.Length;i++){
				EnumUiColor? uiColorCell=UiSliderPaint.GetCellFill(GetCellState(arrayRectanglesCells[i]));
				if(uiColorCell!=null){
					UiControlPaint.Fill(g,uiColorCell.Value,arrayRectanglesCells[i],UiTheme.CORNER_CONTROL,scale);
				}
			}
			//The dividers where today's 1 px lines were, the full height
			int[] arrayXDividers=GetXDividers();
			for(int i=0;i<arrayXDividers.Length;i++){
				UiControlPaint.Fill(g,EnumUiColor.Stroke,new Rectangle(arrayXDividers[i],0,widthLine,Height),UiTheme.CORNER_SQUARE,scale);
			}
			//Minus and plus: the Fluent glyphs, centred in their cells (these two cells are 17x25 unscaled)
			UiSliderPaint.DrawZoomGlyph(g,UiSliderPaint.GetGlyphRectangle(_rectangleMinus,scale),isPlus:false,uiColorText);
			UiSliderPaint.DrawZoomGlyph(g,UiSliderPaint.GetGlyphRectangle(_rectanglePlus,scale),isPlus:true,uiColorText);
			Brush brushText=UiTheme.GetBrush(uiColorText);
			if(IsTextShown){
				g.DrawString("Fit",_font,brushText,_rectangleFit.Left+5,_rectangleFit.Top+4);
				g.DrawString("100",_font,brushText,_rectangle100.Left+3,_rectangle100.Top+4);
			}
			//Tick line, where today's 1 px lines were (DrawLine covered both end points)
			int yTickLine=LayoutManager.Scale(4);
			int xFitPixels=(int)(((float)_rectangleTickLine.Right-_rectangleTickLine.Left)//how many pixels to the Fit tick line
				/(_maximum-1) *_fit);
			Rectangle[] arrayRectanglesTicks=GetRectanglesTickLine(xFitPixels,yTickLine,widthLine);
			for(int i=0;i<arrayRectanglesTicks.Length;i++){
				UiControlPaint.Fill(g,EnumUiColor.StrokeStrong,arrayRectanglesTicks[i],UiTheme.CORNER_SQUARE,scale);
			}
			//Slider: the value box, its outline where today's DrawRectangle put it
			UiControlPaint.Fill(g,EnumUiColor.SurfaceRaised,_rectangleSlider,UiTheme.CORNER_SQUARE,scale);
			UiControlPaint.Outline(g,GetValueBoxOutline(),new Rectangle(_rectangleSlider.X,_rectangleSlider.Y,_rectangleSlider.Width+1,_rectangleSlider.Height+1),
				widthLine,UiTheme.CORNER_SQUARE,scale);
			string sVal=Round(_value).ToString();
			if(IsTextShown){
				g.DrawString(sVal,_font,brushText,_rectangleSlider.X+LayoutManager.ScaleF(1),_rectangleSlider.Y-LayoutManager.ScaleF(1));
			}
			PointF[] points=new PointF[3];
			points[0]=new PointF(xValuePixels,yTickLine);//top point
			points[1]=new PointF(xValuePixels-LayoutManager.ScaleF(2.5f),yTickLine+LayoutManager.ScaleF(5f));//left point
			points[2]=new PointF(xValuePixels+LayoutManager.ScaleF(2.5f),yTickLine+LayoutManager.ScaleF(5f));//right point
			g.FillPolygon(brushText,points);
			//The bottom line, as a toolbar's
			UiControlPaint.Fill(g,EnumUiColor.Gridline,new Rectangle(0,Height-widthLine,Width,widthLine),UiTheme.CORNER_SQUARE,scale);
		}

		///<summary>Whether the texts (Fit, 100 and the value) are drawn. Always true; the state tests draw the shapes alone to compare them.</summary>
		protected virtual bool IsTextShown{
			get{
				return true;
			}
		}

		///<summary>The state of a cell (minus, plus, Fit, 100): pressed while the mouse is down on it, hover under the mouse, at rest otherwise;
		///disabled when the slider is.</summary>
		protected EnumUiButtonState GetCellState(Rectangle rectangleCell){
			if(!Enabled){
				return EnumUiButtonState.Disabled;
			}
			if(_isMouseDown){
				return rectangleCell.Contains(_pointMouseDown) ? EnumUiButtonState.Pressed : EnumUiButtonState.Normal;
			}
			return rectangleCell.Contains(_pointMouse) ? EnumUiButtonState.Hover : EnumUiButtonState.Normal;
		}

		///<summary>The value box's outline: Accent while dragged, StrokeStrongHover under the mouse, StrokeStrong at rest, Stroke when disabled.</summary>
		protected EnumUiColor GetValueBoxOutline(){
			if(!Enabled){
				return EnumUiColor.Stroke;
			}
			if(_isMouseDown){
				return _isMouseDownOnSlider ? EnumUiColor.Accent : EnumUiColor.StrokeStrong;
			}
			return _rectangleSlider.Contains(_pointMouse) ? EnumUiColor.StrokeStrongHover : EnumUiColor.StrokeStrong;
		}

		///<summary>The x of each divider: the left edge and the right edge of each cell, as today.</summary>
		protected int[] GetXDividers(){
			return new int[] { 0,_rectangleMinus.Right,_rectangleTickLineMargin.Right,_rectanglePlus.Right,_rectangleFit.Right,_rectangle100.Right };
		}

		///<summary>The tick line's pieces in whole pixels: the line along the top and the short marks at the start, at Fit and at the end.</summary>
		protected Rectangle[] GetRectanglesTickLine(int xFitPixels,int yTickLine,int widthLine){
			return new Rectangle[] {
				new Rectangle(_rectangleTickLine.X,yTickLine,_rectangleTickLine.Width+widthLine,widthLine),
				new Rectangle(_rectangleTickLine.X,0,widthLine,yTickLine+widthLine),
				new Rectangle(_rectangleTickLine.Left+xFitPixels,0,widthLine,yTickLine+widthLine),
				new Rectangle(_rectangleTickLine.Right,0,widthLine,yTickLine+widthLine)
			};
		}

		protected override void OnEnabledChanged(EventArgs e){

			base.OnEnabledChanged(e);
		}

		protected override void OnMouseLeave(EventArgs e){
			_pointMouse=new Point(-1,-1);
			Invalidate();
			base.OnMouseLeave(e);
		}

		protected override void OnMouseDown(MouseEventArgs e){
			//both mouse buttons are treated the same, so you could have two mouse downs and two mouse ups.
			if(_isMouseDown){
				return;//ignore the second one
			}
			if(textBoxEdit.Visible){
				TextBoxEdit_LostFocus(this,new EventArgs());
			}
			_isMouseDown=true;
			_timeMouseDown=DateTime.Now.TimeOfDay;
			_isMouseDownOnSlider=false;//unless set true down below
			_pointMouseDown=e.Location;
			if(_rectangleMinus.Contains(_pointMouseDown)){
				_value=_value*.5f;
				if(_value<1){
					_value=1;
				}
				Zoomed?.Invoke(this, new EventArgs());
			}
			if(_rectanglePlus.Contains(_pointMouseDown)){
				SetValueAndMax(_value+_maximum*.25f);
				Zoomed?.Invoke(this, new EventArgs());
			}
			if(_rectangleFit.Contains(_pointMouseDown)){
				SetValueAndMax(_fit);
				FitPressed?.Invoke(this, new EventArgs());
				Zoomed?.Invoke(this, new EventArgs());
			}
			if(_rectangle100.Contains(_pointMouseDown)){
				SetValueAndMax(100);
				FitPressed?.Invoke(this, new EventArgs());
				Zoomed?.Invoke(this, new EventArgs());
			}
			if(_rectangleTickLineMargin.Contains(_pointMouseDown)){
				if(_rectangleSlider.Contains(_pointMouseDown)){
					_isMouseDownOnSlider=true;
					_xOffsetMouseDownSlider=_rectangleSlider.X+_rectangleSlider.Width/2-e.X;
				}
				else if(_rectangleTickLine.Contains(_pointMouseDown)){
					//single click to change value
					int xPixels=e.X-_rectangleTickLine.Left;
					_value=(float)xPixels/_rectangleTickLine.Width*_maximum;
					Zoomed?.Invoke(this, new EventArgs());
				}
			}
			Invalidate();
			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e){
			_pointMouse=e.Location;
			if(_isMouseDown){
				if(_isMouseDownOnSlider){
					//They are dragging
					//as they move, we change _value
					if(e.X+_xOffsetMouseDownSlider<_rectangleTickLine.Left){
						_value=1;
					}
					else if(e.X+_xOffsetMouseDownSlider>_rectangleTickLine.Right){
						_value=_maximum;
					}
					else{
						int xPixels=e.X+_xOffsetMouseDownSlider-_rectangleTickLine.Left;
						_value=(float)xPixels/_rectangleTickLine.Width*_maximum;
					}
					Zoomed?.Invoke(this, new EventArgs());
				}
			}
			Invalidate();
			base.OnMouseMove(e);
		}

		protected override void OnMouseUp(MouseEventArgs e){
			if(!_isMouseDown){
				return;//ignore the second one
			}
			_isMouseDown=false;
			if(_isMouseDownOnSlider){
				//if they didn't hold the mouse down for very long and didn't drag very far, then this is a click to edit value
				if(DateTime.Now.TimeOfDay<_timeMouseDown.Add(TimeSpan.FromSeconds(1))//held button down for less than one second
					&& e.X>_pointMouseDown.X-3 && e.X<_pointMouseDown.X+3)//and moved less than 3px
				{
					textBoxEdit.Location=new Point(_rectangleSlider.X-1,_rectangleSlider.Y-2);
					textBoxEdit.Width=_rectangleSlider.Width+4;
					textBoxEdit.Visible=true;
					_textHitEsc=false;
					textBoxEdit.Text=Round(_value).ToString();
					textBoxEdit.Select();
				}
				//if they did drag, nothing to do
			}
			Invalidate();
			base.OnMouseUp(e);
		}

		protected override void OnSizeChanged(EventArgs e) {
			base.OnSizeChanged(e);
			CalculateScale();
		}
		#endregion Methods - Override On...

		#region Methods
		private void CalculateScale(){
			_font?.Dispose();
			_font=new Font("Segoe UI",LayoutManager.ScaleFontODZoom(9f));
			//recalculate most rectangles
			_rectangleMinus=new Rectangle(0,0,LayoutManager.Scale(17),Height-2);
			_rectangle100=new Rectangle(Width-LayoutManager.Scale(30),0,LayoutManager.Scale(29),Height-2);
			_rectangleFit=new Rectangle(_rectangle100.Left-LayoutManager.Scale(27),0,LayoutManager.Scale(27),Height-2);
			_rectanglePlus=new Rectangle(_rectangleFit.Left-LayoutManager.Scale(17),0,LayoutManager.Scale(17),Height-2);
			_rectangleTickLineMargin=new Rectangle(_rectangleMinus.Right,0,_rectanglePlus.Left-_rectangleMinus.Right,Height-2);
			//rectangleTickLine and rectangleSlider are far too complex to calculate here.  We do those on each paint.			
		}

		private int Round(float num){
			return (int)Math.Round(num);
		}

		public void SetLayoutManager(LayoutManagerForms layoutManager){
			LayoutManager=layoutManager;
			CalculateScale();//needed for initial load when main monitor is not 100%
		}

		///<summary>These parameters are required because initial center zoom is always to "fit". This is also the only way to set Enabled to true.  If degreesRotated is is 90 or 270, it will swap width and height when doing the calc.</summary>
		public void SetValueInitialFit(Size sizeCanvas,Size sizeImage,float degreesRotated){
			_fit=(int)(ImageTools.CalcScaleFit(sizeCanvas,sizeImage,degreesRotated)*100);
			_value=_fit;
			_maximum=2*_fit;//starting point
			if(Round(_value)>_maximum){
				_maximum=Round(_value);
			}
			Enabled=true;
			Invalidate();
		}

		///<summary>The number passed in here is a zoom change, like -15.  It was calculated externally based on existing _value for proportionality.</summary>
		public void SetByWheel(float deltaZoom){
			SetValueAndMax(_value+deltaZoom);   
			Invalidate();
		}

		///<summary>Sets value.  If value is greater than Max, then Max will grow.</summary>
		public void SetValueAndMax(float newval){
			//Converting to a float causes the int.maxvalue to still wrap around to a negative value, removed 100 from the maximum to avoid this error.
			if(newval>=int.MaxValue){
				newval=(float)int.MaxValue-100;
			}
			_value=newval;
			if(_value<1){
				_value=1;
			}
			if(Round(_value)>_maximum){
				_maximum=Round(_value);
			}
			Invalidate();
		}
		#endregion Methods



	

	}
}
