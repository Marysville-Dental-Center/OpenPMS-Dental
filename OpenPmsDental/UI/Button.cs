using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI {
	//Jordan is the only one allowed to edit this file.

	///<summary>Custom button used extensively throughout OD. Typically shows an image and text.  Image is usually 22x22.  Height is usually 24.</summary>
	public partial class Button : System.Windows.Forms.Button {
		#region Fields - Public
		///<summary>Just holds the scaling factor.</summary>
		public LayoutManagerForms LayoutManager=new LayoutManagerForms();
		#endregion Fields - Public

		#region Fields - Private Static Colors
		//The button itself draws from the UiTheme tokens (UI-4a, docs/specs/ui-refresh-plan.md section 3.6). These colours are today's look of
		//DrawSimpleButton, which ODButtonPanel uses; UI-6 restyles the button panel.
		private static Color _colorBorder=Color.FromArgb(28,81,128);
		private static Color _colorText=Color.FromArgb(0,0,0);
		///<summary>The white glow that shows .5 pixels to the lower right behind the text.</summary>
		private static Color _colorTextGlow=Color.FromArgb(255,255,255);
		//Since these are static, one shared copy of each will persist for the life of the program
		private static Pen _penBorder=new Pen(_colorBorder);
		private static Brush _brushText=new SolidBrush(_colorText);
		private static Brush _brushTextGlow=new SolidBrush(_colorTextGlow);
		///<summary>The AcceptButtons that keep the plain look, as "FormName.buttonName": the ones the owner turns down in gallery G-UI3 (section 3.6).
		///Empty until those answers.</summary>
		private static readonly string[] _arrayAcceptButtonsPlain=new string[0];
		#endregion Fields - Private Static Colors
		
		#region Fields - Private
		private ODButtonState _buttonState=ODButtonState.Normal;
		private bool _bCanClick=false;
		private Point _adjustImageLocation=new Point(0,0);
		private EnumIcons _icon=EnumIcons.None;
		///<summary>When changing the text, this keeps track of the previous value. This lets us avoid overhead for initializing.</summary>
		private string TextOld;
		#endregion Fields - Private	

		#region Constructor
		///<summary></summary>
		public Button() {
			//This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			DoubleBuffered=true;
		}
		#endregion Constructor

		#region Enums
		///<summary></summary>
		private enum ODButtonState {
			///<summary></summary>
			Normal,
			///<summary></summary>
			Hover,
			///<summary>Mouse down. Not a permanent toggle state.</summary>
			Pressed
		}
		#endregion Enums

		#region Properties
		///<summary></summary>
		[Category("OD"),Description("Allows slight adjustments of image location, usually just one or two pixels.")]
		[DefaultValue(typeof(Point),"0,0")]
		public Point AdjustImageLocation {
			get {
				return _adjustImageLocation;
			}
			set { 
				_adjustImageLocation=value;
				this.Invalidate();
			}
		}

		///<summary></summary>
		[Category("OD"),Description("This is replacing Image.")]
		[DefaultValue(0)]
		public EnumIcons Icon {
			get {
				return _icon;
			}
			set { 
				_icon=value;
				this.Invalidate();
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size(75,24);
			}
		}

		///<summary>True when this button is its form's AcceptButton, which draws as the default button (section 3.6: filled Accent with white text),
		///unless it is on the list of AcceptButtons the owner keeps plain. Only the look: Enter is handled by the form as before.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsPrimary {
			get {
				Form form=FindForm();
				if(form==null || form.AcceptButton!=this) {
					return false;
				}
				string name=form.GetType().Name+"."+Name;
				return Array.IndexOf(_arrayAcceptButtonsPlain,name)==-1;
			}
		}

		///<summary>Whether the focus ring shows (section 3.6): the button has focus and Windows asks for keyboard cues, so a mouse click draws no
		///ring.</summary>
		protected virtual bool IsFocusRingShown {
			get {
				return Focused && ShowFocusCues;
			}
		}

		#endregion Properties

		#region Methods - Private
		private static StringFormat GetStringFormat(ContentAlignment contentAlignment) {
			if(!Enum.IsDefined(typeof(ContentAlignment),(int)contentAlignment)) {
				throw new InvalidEnumArgumentException("contentAlignment",(int)contentAlignment,typeof(ContentAlignment));
			}
			StringFormat stringFormat=new StringFormat();
			switch(contentAlignment) {
				case ContentAlignment.MiddleCenter:
					stringFormat.LineAlignment=StringAlignment.Center;
					stringFormat.Alignment=StringAlignment.Center;
					break;
				case ContentAlignment.MiddleLeft:
					stringFormat.LineAlignment=StringAlignment.Center;
					stringFormat.Alignment=StringAlignment.Near;
					break;
				case ContentAlignment.MiddleRight:
					stringFormat.LineAlignment=StringAlignment.Center;
					stringFormat.Alignment=StringAlignment.Far;
					break;
				case ContentAlignment.TopCenter:
					stringFormat.LineAlignment=StringAlignment.Near;
					stringFormat.Alignment=StringAlignment.Center;
					break;
				case ContentAlignment.TopLeft:
					stringFormat.LineAlignment=StringAlignment.Near;
					stringFormat.Alignment=StringAlignment.Near;
					break;
				case ContentAlignment.TopRight:
					stringFormat.LineAlignment=StringAlignment.Near;
					stringFormat.Alignment=StringAlignment.Far;
					break;
				case ContentAlignment.BottomCenter:
					stringFormat.LineAlignment=StringAlignment.Far;
					stringFormat.Alignment=StringAlignment.Center;
					break;
				case ContentAlignment.BottomLeft:
					stringFormat.LineAlignment=StringAlignment.Far;
					stringFormat.Alignment=StringAlignment.Near;
					break;
				case ContentAlignment.BottomRight:
					stringFormat.LineAlignment=StringAlignment.Far;
					stringFormat.Alignment=StringAlignment.Far;
					break;
			}
			return stringFormat;
		}
		#endregion Methods - Private

		#region Methods - Override On...
		///<summary></summary>
		protected override void OnClick(EventArgs ea) {
			this.Capture=false;
			_bCanClick=false;
			if(this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition))) {
				_buttonState=ODButtonState.Hover;
			}
			else {
				_buttonState=ODButtonState.Normal;
			}
			this.Invalidate();
			base.OnClick(ea);
		}

		///<summary></summary>
		protected override void OnMouseEnter(EventArgs ea) {
			base.OnMouseEnter(ea);
			_buttonState=ODButtonState.Hover;
			this.Invalidate();
		}

		///<summary></summary>
		protected override void OnMouseDown(MouseEventArgs mea) {
			base.OnMouseDown(mea);
			if(mea.Button==MouseButtons.Left) {
				_bCanClick=true;
				_buttonState=ODButtonState.Pressed;
				this.Invalidate();
			}
		}

		///<summary></summary>
		protected override void OnMouseMove(MouseEventArgs mea) {
			base.OnMouseMove(mea);
			if(ClientRectangle.Contains(mea.X, mea.Y)) {
				if(_buttonState==ODButtonState.Hover && this.Capture && !_bCanClick) {
					_bCanClick=true;
					_buttonState=ODButtonState.Pressed;
					this.Invalidate();
				}
			}
			else {
				if(_buttonState==ODButtonState.Pressed) {
					_bCanClick=false;
					_buttonState=ODButtonState.Hover;
					this.Invalidate();
				}
			}
		}

		///<summary></summary>
		protected override void OnMouseLeave(EventArgs ea) {
			base.OnMouseLeave(ea);
			_buttonState=ODButtonState.Normal;
			this.Invalidate();
		}

		///<summary></summary>
		protected override void OnEnabledChanged(EventArgs ea) {
			base.OnEnabledChanged(ea);
			_buttonState=ODButtonState.Normal;
			this.Invalidate();
		}

		///<summary></summary>
		protected override void OnTextChanged(EventArgs e) {
			base.OnTextChanged(e);
			//The only point of this override is to automatically widen a button if the foreign language translation would take more space than available.
			if(TextOld==null || TextOld==""){//There's no way to test the internal "IsLayoutSuspended", which is what I would rather have.
				//probably initially setting text value, so just skip this.
				TextOld=this.Text;
				return;
			}
			TextOld=this.Text;
			if(Text=="" || IsDisposed) {
				return;
			}
			try {
				int buffer=6;
				int textWidth=0;
				//jordan- The three lines below cause this button to not show in FormSheetDefs because that form is set to be dpi unaware.
				//We don't know why.
				//using(Graphics g = this.CreateGraphics()) {
				//	textWidth=(int)g.MeasureString(Text,Font).Width;
				//}
				textWidth=TextRenderer.MeasureText(Text,Font).Width;
				int oldWidth=Width;
				if(this.Image==null) {
					if(Width<textWidth+buffer) {
						LayoutManager.MoveWidth(this,textWidth+buffer);
					}
				}
				else {
					if(Width<textWidth+Image.Size.Width+buffer) {
						LayoutManager.MoveWidth(this,textWidth+Image.Size.Width+buffer);
					}
				}
//todo: this is broken for LayoutManager, but only noticed for foreign language and is rare
				if((Anchor&AnchorStyles.Right)==AnchorStyles.Right) {
					//to be perfect, it would exclude if anchored left also
					//this works even if no change in width
					Left+=oldWidth-Width;
				}
			}
			catch{
				//This method may fail if window handle has not been created yet. 
				//Can happen if invoked from a separate thread due to a race condition.
				//Nothing we can do about it and not worth crashing the program so ignore it.
			}
			Invalidate();
		}
		#endregion Methods - Override On...

		#region Method - OnPaint
		///<summary>Section 3.6: flat SurfaceRaised with a 1 px Stroke border and 4 px corners, ButtonHover on hover, ButtonPressed pressed; the form's
		///AcceptButton filled Accent (AccentHover, AccentPressed) with white text; disabled #F5F5F5 with TextDisabled; the focus ring with keyboard
		///cues. No gradient and no text glow. The text and the icon stay where they were.</summary>
		protected override void OnPaint(PaintEventArgs p) {
			this.OnPaintBackground(p);//the parent's colour at the rounded corners
			Graphics g=p.Graphics;
			EnumUiButtonState buttonState=GetUiButtonState();
			bool isPrimary=IsPrimary;
			EnumUiColor uiColorText=UiControlPaint.DrawButton(g,ClientRectangle,buttonState,isPrimary,IsFocusRingShown,LayoutManager.ScaleMy());
			g.SmoothingMode=SmoothingMode.HighQuality;
			DrawTextAndImage(g,uiColorText,UiTheme.GetColor(UiControlPaint.GetButtonFill(buttonState,isPrimary)));
		}

		///<summary>The state the button draws in: disabled wins, then pressed and hover.</summary>
		private EnumUiButtonState GetUiButtonState() {
			if(!Enabled) {
				return EnumUiButtonState.Disabled;
			}
			if(_buttonState==ODButtonState.Pressed) {
				return EnumUiButtonState.Pressed;
			}
			if(_buttonState==ODButtonState.Hover) {
				return EnumUiButtonState.Hover;
			}
			return EnumUiButtonState.Normal;
		}

		///<summary>The text in the token colour of the state, and the image or icon where they were.</summary>
		private void DrawTextAndImage(Graphics g,EnumUiColor uiColorText,Color colorFill) {
			//A token brush: cached per thread by UiTheme, never disposed here.
			SolidBrush brushText=UiTheme.GetBrush(uiColorText);
			StringFormat stringFormat=GetStringFormat(this.TextAlign);
			if(ShowKeyboardCues) {
				stringFormat.HotkeyPrefix=System.Drawing.Text.HotkeyPrefix.Show;
			}
			else {
				stringFormat.HotkeyPrefix=System.Drawing.Text.HotkeyPrefix.Hide;
			}
			if(Image==null && Icon==EnumIcons.None){
				g.DrawString(this.Text,this.Font,brushText,this.ClientRectangle,stringFormat);
				stringFormat?.Dispose();
				return;
			}
			//Image:
			Rectangle rectangleText=new Rectangle();
			Rectangle rectangleImage;
			Point pointImage= new Point(6, 4);
			Size sizeScaled;
			if(Icon!=EnumIcons.None){
				sizeScaled=new Size(LayoutManager.Scale(22),LayoutManager.Scale(22));
			}
			else{
				sizeScaled=new Size(LayoutManager.Scale(Image.Width),LayoutManager.Scale(Image.Height));
			}
			switch(this.ImageAlign) {
				case ContentAlignment.MiddleLeft:
					pointImage.X=6;
					pointImage.Y=this.ClientRectangle.Height/2-sizeScaled.Height/2;
					rectangleText.Width=this.ClientRectangle.Width-sizeScaled.Width;
					rectangleText.Height=this.ClientRectangle.Height;
					rectangleText.X=sizeScaled.Width;
					rectangleText.Y=0;
					break;
				case ContentAlignment.MiddleRight:
					rectangleText.Width=this.ClientRectangle.Width-sizeScaled.Width-8;
					rectangleText.Height=this.ClientRectangle.Height;
					rectangleText.X=0;
					rectangleText.Y=0;
					pointImage.X=rectangleText.Width;
					rectangleText.Width+=this._adjustImageLocation.X;
					pointImage.Y=this.ClientRectangle.Height/2-sizeScaled.Height/2;
					break;
				case ContentAlignment.MiddleCenter:// no text in this alignment
					pointImage.X=(this.ClientRectangle.Width-sizeScaled.Width)/2;
					pointImage.Y=(this.ClientRectangle.Height-sizeScaled.Height)/2;
					rectangleText.Width=0;
					rectangleText.Height=0;
					rectangleText.X=this.ClientRectangle.Width;
					rectangleText.Y=this.ClientRectangle.Height;
					break;
			}
			pointImage.X+=_adjustImageLocation.X;
			pointImage.Y+=_adjustImageLocation.Y;
			//Direct2D draws the icon into a bitmap over a background of its own: the button's flat fill, so the two match. The icon itself is drawn
			//as today until UI-7b and UI-9a give the buttons Fluent icons (section 3.6).
			Color colorGradientTop=colorFill;
			Color colorGradientBottom=colorFill;
			if(this.Enabled) {
				rectangleImage=new Rectangle(pointImage,sizeScaled);
				if(Icon!=EnumIcons.None){
					IconLibrary.Draw(g,Icon,rectangleImage,colorGradientTop,colorGradientBottom,DesignMode);
				}
				else{
					g.DrawImage((Bitmap)Image,rectangleImage);
				}
			}
			else {
				rectangleImage=new Rectangle(pointImage,sizeScaled);
				if(Icon!=EnumIcons.None){
					IconLibrary.DrawDisabled(g,Icon,rectangleImage);
				}
				else{
					Bitmap bitmapDisabled=new Bitmap(Image.Width,Image.Height);//disabled bitmap is old small size
					Graphics gfx=Graphics.FromImage(bitmapDisabled);
					//System.Windows.Forms.ControlPaint.DrawImageDisabled(g,this.Image,pointImage.X,pointImage.Y,BackColor);
					ControlPaint.DrawImageDisabled(gfx,Image,0,0,SystemColors.Control);
					g.DrawImage(bitmapDisabled,rectangleImage);
					gfx.Dispose();
					bitmapDisabled.Dispose();
				}
			}
			if(this.ImageAlign!=ContentAlignment.MiddleCenter) {
				g.DrawString(this.Text,this.Font,brushText,rectangleText,stringFormat);
			}
			stringFormat?.Dispose();
		}

		///<summary>Used by ODButtonPanel.  Let Button.cs do this drawing because this is where the colors are stored.  No image. Text in middle center.  No hover effect.</summary>
		public static void DrawSimpleButton(Graphics g,Rectangle rectangle,string text,Font font,Brush brush){
			RectangleF recOutline=new RectangleF(rectangle.X,rectangle.Y,rectangle.Width-1.2f,rectangle.Height-1.2f);
			float radius=3;
			GraphicsPath graphicsPath=OpenPmsDentalBusiness.GraphicsHelper.GetRoundedPath(recOutline,radius);
			g.FillPath(brush,graphicsPath);
			g.DrawPath(_penBorder,graphicsPath);
			graphicsPath.Dispose();
			StringFormat stringFormat=GetStringFormat(ContentAlignment.MiddleCenter);
			RectangleF rectangleTextGlow;
			rectangleTextGlow=new RectangleF(rectangle.X+.5f,rectangle.Y+.5f,rectangle.Width,rectangle.Height);
			g.DrawString(text,font,_brushTextGlow,rectangleTextGlow,stringFormat);
			g.DrawString(text,font,_brushText,rectangle,stringFormat);
			stringFormat.Dispose();
		}
		#endregion Method - OnPaint

	}

}

//[\t ]+this\.\w+\.AdjustImageLocation ?= ?new System\.Drawing\.Point\(0,0\);\r\n
//this.butOK.AdjustImageLocation = new System.Drawing.Point(0, 0);