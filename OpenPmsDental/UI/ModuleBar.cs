using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	/// <summary>The button bar along the left side for the modules.</summary>
	public class ModuleBar : System.Windows.Forms.Control{
		#region Fields - Public
		///<summary>Just holds the scaling factor.</summary>
		public LayoutManagerForms LayoutManager=new LayoutManagerForms();
		#endregion Fields - Public

		#region Fields - Private
		/// <summary>Required designer variable.</summary>
		private System.ComponentModel.Container components = null;
		//The colours are UiTheme's tokens (UI-6, docs/specs/ui-refresh-plan.md section 3.6): shared per thread, never disposed here.
		///<summary>The button under the mouse, -1 for none. Set by the mouse events; the painting reads it.</summary>
		private int currentHot=-1;
		///<summary>True while the left mouse button is down after a press on the bar (the pressed look of the hot button).</summary>
		private bool _isMouseDown;
		///<summary>At 96dpi</summary>
		//private int _heightButton=39+26;//typical button is 49x65
		private int _heightButton=39+18;//typical button is 49x57
		///<summary></summary>
		private List<ModuleBarButton> _listButtons;
		///<summary>Property backer</summary>
		private int _selectedIndex=-1;
		///<summary>Used when click event is cancelled.</summary>
		private int _selectedIndexPrevious;
		///<summary>Class level variable, to avoid allocating and disposing memory repeatedly every frame.</summary>
		private StringFormat _stringFormat;
		///<summary>This is needed to fix artifacts from Direct2D.</summary>
		Timer timer;
		#endregion Fields - Private

		#region Constructor
		///<summary></summary>
		public ModuleBar(){
			InitializeComponent();
			DoubleBuffered=true;
			//Rectangle gradientRect=new Rectangle(myButton.Bounds.X,myButton.Bounds.Y+myButton.Bounds.Height-10,myButton.Bounds.Width,10);
			//_brushHot=new LinearGradientBrush(new PointF(0,0),new PointF(0,10),_outlookSelectedBrush.Color,_outlookPressedBrush.Color);
			_stringFormat=new StringFormat();
			_stringFormat.Alignment=StringAlignment.Center;
			_stringFormat.LineAlignment=StringAlignment.Center;
			_listButtons=new List<ModuleBarButton>();
			_listButtons.Add(new ModuleBarButton(EnumModuleType.Appointments,"Appts",GetIcon(EnumModuleType.Appointments)));//0
			_listButtons.Add(new ModuleBarButton(EnumModuleType.Family,"Family",GetIcon(EnumModuleType.Family)));           //1
			_listButtons.Add(new ModuleBarButton(EnumModuleType.Account,"Account",GetIcon(EnumModuleType.Account)));        //2
			_listButtons.Add(new ModuleBarButton(EnumModuleType.TreatPlan,"Tx Plan",GetIcon(EnumModuleType.TreatPlan)));//3
			_listButtons.Add(new ModuleBarButton(EnumModuleType.Chart,"Chart",GetIcon(EnumModuleType.Chart)));              //4
			_listButtons.Add(new ModuleBarButton(EnumModuleType.Imaging,"Imaging",GetIcon(EnumModuleType.Imaging)));           //5
			_listButtons.Add(new ModuleBarButton(EnumModuleType.Manage,"Manage",GetIcon(EnumModuleType.Manage)));           //6
			_selectedIndex=0;
			ComputeButtonSizes();
			timer=new Timer();
			timer.Interval=1000;//redraw every 1 second to remove artifacts
			timer.Enabled=true;
			timer.Tick+=timer_Tick;
		}
		#endregion Constructor

		#region Component Designer generated code
		/// <summary>Clean up any resources being used.</summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			this.ResumeLayout(false);

		}
		#endregion Component Designer generated code

		#region Events - Raise
		///<summary></summary>
		[Category("OD")]
		[Description("Occurs when a module button is clicked.")]
		public event ButtonClickedEventHandler ButtonClicked = null;
		///<summary></summary>
		protected void OnButtonClicked(ModuleBarButton myButton,bool myCancel){
			if(ButtonClicked != null){
				//previousSelected=SelectedIndex;
				ButtonClicked_EventArgs oArgs = new ButtonClicked_EventArgs(myButton,myCancel);
				ButtonClicked(this,oArgs);
				if(oArgs.Cancel){
					_selectedIndex=_selectedIndexPrevious;
					Invalidate();
				}
			}
		}
		#endregion Events - Raise

		#region Properties
		///<summary></summary>
		[Browsable(false)]
		[DefaultValue(EnumModuleType.None)]
		public EnumModuleType SelectedModule{
			get{
				if(_selectedIndex==-1){
					return EnumModuleType.None;
				}
				return _listButtons[_selectedIndex].ModuleType;
			}
			set{
				ModuleBarButton moduleBarButton=_listButtons.FirstOrDefault(x=>x.ModuleType==value);
				if(moduleBarButton==null){
					return;
				}
				_selectedIndex=_listButtons.IndexOf(moduleBarButton);
			}
		}

		///<summary>Only used in 3 places where it can't be avoided because of the business layer.</summary>
		[Browsable(false)]
		[DefaultValue(-1)]
		public int SelectedIndex{
			get{
				return _selectedIndex;
			}
			set{
				_selectedIndex=value;
				Invalidate();
			}
		}
		#endregion

		#region Methods - Public
		///<summary>Needed in just a few areas for backward compatibility.</summary>
		public int IndexOf(EnumModuleType moduleType){
			if(moduleType==EnumModuleType.None){
				return -1;
			}
			ModuleBarButton moduleBarButton=_listButtons.FirstOrDefault(x=>x.ModuleType==moduleType);
			if(moduleBarButton==null){
				return -1;
			}
			return _listButtons.IndexOf(moduleBarButton);
		}

		/// <summary>Fixes theme image and text translation for any existing buttons.</summary>
		public void RefreshButtons() {
			bool isMedical=Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum);
			for(int i=0;i<_listButtons.Count;i++){
				_listButtons[i].Icon=GetIcon(_listButtons[i].ModuleType,isMedical);
				//Image null is ok and allowed
				switch(_listButtons[i].ModuleType){
					case EnumModuleType.Appointments:
						_listButtons[i].Caption=Lan.g(this,"Appts");
						break;
					case EnumModuleType.Family:
						_listButtons[i].Caption=Lan.g(this,"Family");
						break;
					case EnumModuleType.Account:
						_listButtons[i].Caption=Lan.g(this,"Account");
						break;
					case EnumModuleType.TreatPlan:
						_listButtons[i].Caption=Lan.g(this,"Tx Plan");
						break;
					case EnumModuleType.Chart:
						_listButtons[i].Caption=Lan.g(this,"Chart");
						if(PrefC.GetBool(PrefName.EasyHideClinical)) {
							_listButtons[i].Caption=Lan.g(this,"Procs");
						}
						_listButtons[i].Icon2=EnumIcons.Chart32W;
						if(isMedical){
							_listButtons[i].Icon2=EnumIcons.ChartMed32;
						}
						break;
					case EnumModuleType.Imaging:
						_listButtons[i].Caption=Lan.g(this,"Imaging");
						break;
					case EnumModuleType.Manage:
						_listButtons[i].Caption=Lan.g(this,"Manage");
						break;
				}
			}
			Invalidate();
		}

		///<summary></summary>
		public void SetVisible(EnumModuleType moduleType,bool isVisible){
			ModuleBarButton moduleBarButton=_listButtons.FirstOrDefault(x=>x.ModuleType==moduleType);
			if(moduleBarButton==null){
				return;
			}
			moduleBarButton.Visible=isVisible;
			Invalidate();
		}
		#endregion Methods - Public

		#region Methods - OnPaint
		/// <summary>Triggered every time the control decides to repaint itself.</summary>
		protected override void OnPaint(PaintEventArgs pe) {
			Graphics g=pe.Graphics;
			g.SmoothingMode=SmoothingMode.HighQuality;
			//The bar (UI-6, section 3.6, Module bar): flat SurfaceSubtle with a 1 px Gridline divider on its right, no lines between the buttons.
			g.Clear(UiTheme.SurfaceSubtle);
			try {
				ComputeButtonSizes();
				int widthLine=UiTheme.GetLineWidth(LayoutManager.ScaleMy());
				g.FillRectangle(UiTheme.GetBrush(EnumUiColor.Gridline),Width-widthLine,0,widthLine,Height);
				for(int i=0;i<_listButtons.Count;i++) {
					bool isHot=(currentHot==i);
					bool isPressed=(_isMouseDown && isHot);
					bool isSelected=(i==_selectedIndex);
					DrawButton(_listButtons[i],isHot,isPressed,isSelected,g);
				}
			}
			catch(Exception ex) {
				//We had one customer who was receiving overflow exceptions because the ClientRetangle provided by the system was invalid,
				//due to a graphics device hardware state change when loading the Dexis client application via our Dexis bridge.
				//If we receive an invalid ClientRectangle, then we will simply not draw the button for a frame or two until the system has initialized.
				//A couple of frames later the system should return to normal operation and we will be able to draw the button again.
				ex.DoNothing();
			}
			// Calling the base class OnPaint
			//base.OnPaint(pe);
		}

		/// <summary>Draws one button (UI-6, section 3.6, Module bar). isHot: Is the mouse currently hovering over this button. isPressed: Is the left
		/// mouse button currently down on this button. isSelected: Is this the currently selected button. The shape is inset 4 px from the button's
		/// sides and 2 px from its top and bottom, with 6 px corners: SubtleHover on hover, SubtlePressed pressed; the selected module SurfaceRaised
		/// with a 3 px Accent mark on the left and its icon in its Filled style in Accent. Other icons are Regular in Text; captions in Text.</summary>
		private void DrawButton(ModuleBarButton button,bool isHot,bool isPressed,bool isSelected,Graphics g){
			if(!button.Visible) {
				return;//the bar's background shows
			}
			float scale=LayoutManager.ScaleMy();
			Rectangle rectangleShape=Rectangle.Inflate(button.Bounds,-LayoutManager.Scale(UiTheme.INSET_MODULE_HOVER),-LayoutManager.Scale(2));
			int radius=UiTheme.GetCornerRadius(UiTheme.CORNER_MODULE_HOVER,scale);
			Color colorIcon=UiTheme.Text;
			bool isFilled=false;
			if(isSelected) {
				UiToolStripRenderer.FillRounded(g,EnumUiColor.SurfaceRaised,rectangleShape,radius);
				g.FillRectangle(UiTheme.GetBrush(EnumUiColor.Accent),button.Bounds.X,rectangleShape.Y,LayoutManager.Scale(UiTheme.LINE_MODULE_MARK),rectangleShape.Height);
				colorIcon=UiTheme.Accent;
				isFilled=true;
			}
			else if(isPressed) {
				UiToolStripRenderer.FillRounded(g,EnumUiColor.SubtlePressed,rectangleShape,radius);
			}
			else if(isHot) {
				UiToolStripRenderer.FillRounded(g,EnumUiColor.SubtleHover,rectangleShape,radius);
			}
			//Image: the Fluent icon drawn transparently over the shape (no box behind it); the Chart module draws UI-8's tooth (Chart32G the Tooth,
			//selected Chart32W the ToothFilled in Accent; the medical layout the ChartMedical).
			Rectangle imgRect=new Rectangle((Width-LayoutManager.Scale(32))/2,button.Bounds.Y+4,LayoutManager.Scale(32),LayoutManager.Scale(32));
			EnumIcons icon=button.Icon;
			if(isSelected && button.Icon2!=EnumIcons.None){
				icon=button.Icon2;//the chart module has two images
			}
			ToolBarOD.DrawIconTransparent(g,icon,imgRect,colorIcon,isFilled);
			//Text
			using Font font=new Font(UiTheme.FontModuleCaption.FontFamily,LayoutManager.ScaleFontODZoom(UiTheme.FontModuleCaption.SizeInPoints));//The module caption token (UI-2a-5): Arial 8 until UI-2b.
			Rectangle textRect = new Rectangle(button.Bounds.X-1,imgRect.Bottom,button.Bounds.Width+2,button.Bounds.Bottom-imgRect.Bottom);
			g.DrawString(button.Caption,font,UiTheme.GetBrush(EnumUiColor.Text),textRect,_stringFormat);
		}

		//private void DrawImageVector(ModuleBarButton button,Direct2d d){
		//	Rectangle imgRect=new Rectangle((Width-LayoutManager.Scale(32))/2,button.Bounds.Y+4,LayoutManager.Scale(32),LayoutManager.Scale(32));
		//	IconLibrary.Draw(null,d,button.Icon,imgRect);
		//}
		#endregion Methods - OnPaint

		#region Methods - Override
		///<summary></summary>
		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e){
			base.OnMouseDown(e);
			if(e.Button==MouseButtons.Left){
				_isMouseDown=true;
			}
			//Graphics g=this.CreateGraphics();
			if(currentHot != -1){
				//redraw current button to give feedback on mouse down.
				Invalidate(); //just invalidate to force a repaint
			}
		}

		///<summary></summary>
		protected override void OnMouseLeave(System.EventArgs e){
			base.OnMouseLeave(e);
			if(currentHot!=-1){
				//undraw previous button
				Invalidate(); //just invalidate to force a repaint.
			}
			currentHot=-1;		
		}

		///<summary></summary>
		protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e){
			base.OnMouseMove(e);
			int hotBut=GetButtonI(new Point(e.X,e.Y));
			if(hotBut != currentHot){
				Invalidate(); //just invalidate to force a repaint.
				currentHot=hotBut;
			}			
		}	

		//<summary></summary>
		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e){
			base.OnMouseUp(e);
			if(e.Button != MouseButtons.Left){
				return;
			}
			_isMouseDown=false;
			int selectedBut=GetButtonI(new Point(e.X,e.Y));
			if(selectedBut==-1){
				return;
			}
			if(!_listButtons[selectedBut].Visible){
				return;
			}
			//int oldSelected=SelectedIndex;
			_selectedIndexPrevious=_selectedIndex;
			_selectedIndex=selectedBut;
			Invalidate(); //just invalidate to force a repaint
			OnButtonClicked(_listButtons[_selectedIndex],false);
		}

		///<summary></summary>
		protected override void OnSizeChanged(System.EventArgs e){
			base.OnSizeChanged(e);
			Invalidate();
			if(timer.Enabled){
				timer.Enabled=false;//restart the timer
			}
			timer.Interval=100;//a bit faster than normal
			timer.Enabled=true;
		}
		#endregion Methods - Override

		#region Methods - Private
		private void ComputeButtonSizes(){
			// Calculates button sizes and maybe more later
			//int barTop = 1;
			Graphics g = this.CreateGraphics();
			int top=0;
			int width=this.Width-2;//Designed for a Width of 51
			//int textHeight=0;
			for(int i=0;i<_listButtons.Count;i++){
				//--- Look if multiline text, if is add extra Height to button.
				//SizeF textSize = g.MeasureString(Buttons[i].Caption,textFont,width+2);
				//textHeight = (int)(Math.Ceiling(textSize.Height));
				//if(textHeight<26)
				//	textHeight=26;//default to height of 2 lines of text for uniformity.
				_listButtons[i].Bounds=new Rectangle(0,top,width,LayoutManager.Scale(_heightButton));//39+26);
				top+=LayoutManager.Scale(_heightButton)+1;//39+26+1;
			}
			g.Dispose();
		}

		private int GetButtonI(Point myPoint){
			for(int i=0;i<_listButtons.Count;i++){
				//Item item = activeBar.Items[it];
				if(_listButtons[i].Bounds.Contains(myPoint)){
					return i;
				}
			}
			return -1;
		}

		private EnumIcons GetIcon(EnumModuleType moduleType,bool isMedical=false){
			switch(moduleType){
				case EnumModuleType.Appointments:
					return EnumIcons.Appt32;
				case EnumModuleType.Family:
					return EnumIcons.Family32;
				case EnumModuleType.Account:
					return EnumIcons.Account32;
				case EnumModuleType.TreatPlan:
					if(isMedical){
						return EnumIcons.TreatPlanMed32;
					}
					return EnumIcons.TreatPlan32;
				case EnumModuleType.Chart:
					if(isMedical){
						return EnumIcons.ChartMed32;
					}
					return EnumIcons.Chart32G;
				case EnumModuleType.Imaging:
					return EnumIcons.Imaging32;
				case EnumModuleType.Manage:
					return EnumIcons.Manage32;
				default:
					return EnumIcons.None;
			}
		}

		private void timer_Tick(object sender,EventArgs e) {
			//this happens soon after the last resize and also every second
			Invalidate();
			timer.Interval=1000;//set it back to the longer interval
		}

		/*
		///<summary>Can return -1, which usually indicates that it will be handled by program resource instead of image list.</summary>
		private int GetImageIdx(EnumModuleType moduleType,bool isMedical) {
			if(IsAlternateIcons) {
				switch(moduleType){
					case EnumModuleType.Appointments:
						return 9;
					case EnumModuleType.Family:
						return 10;
					case EnumModuleType.Account:
						return 11;
					case EnumModuleType.TreatPlan:
						return 12;//for ecw, also
					case EnumModuleType.Chart:
						return 13;//for ecw, also
					case EnumModuleType.Imaging:
						return 14;
					case EnumModuleType.Manage:
						return 15;
					default:
						return -1;
				}
			}
			else{//normal non-flat style
				switch(moduleType){
					case EnumModuleType.Appointments:
						return 0;
					case EnumModuleType.Family:
						return -1;
					case EnumModuleType.Account:
						return 2;
					case EnumModuleType.TreatPlan:
						if(isMedical){
							return 7;
						}
						return 3;
					case EnumModuleType.Chart:
						if(isMedical){
							return 8;
						}
						return 4;
					case EnumModuleType.Imaging:
						return 5;
					case EnumModuleType.Manage:
						return 6;
					default:
						return -1;
				}
			}
		}*/
		#endregion Methods - Private



	}

	#region Class ModuleButton
	///<summary>Lightweight, just to keep track of a few fields.</summary>
	public class ModuleBarButton{
		///<summary>The selected button's old gradient. Not drawn or made since UI-6 (the flat look); kept for code that reads it. Always null.</summary>
		public LinearGradientBrush BrushHot;
		///<summary></summary>
		public Rectangle Bounds;
		///<summary></summary>
		public string Caption;
		///<summary></summary>
		///
		public EnumIcons Icon;
		///<summary>Just used by Chart module to show bitmap with white background instead of default gray. This was required because the Chart icon doesn't look good around the edges when drawn on a white background, so we have to have a second chart icon specifically for when the white background (Chart module selected) is showing.</summary>
		public EnumIcons Icon2;
		///<summary></summary>
		public EnumModuleType ModuleType;
		///<summary></summary>
		public bool Visible=true;

		///<summary></summary>
		public ModuleBarButton(EnumModuleType moduleType,string caption,EnumIcons icon){
			Caption=caption;
			ModuleType=moduleType;
			Icon=icon;
		}
	}
	#endregion Class ModuleButton

	#region EventArgs
	///<summary></summary>
	public class ButtonClicked_EventArgs{
		private ModuleBarButton outlookButton;
		private bool cancel;

		///<summary></summary>
		public ButtonClicked_EventArgs(ModuleBarButton myButton,bool myCancel){
			outlookButton=myButton;
		}

		///<summary></summary>
		public ModuleBarButton OutlookButton{
			get{
				return outlookButton;
			}
		}

		///<summary>Set true to cancel the event.</summary>
		public bool Cancel{
			get{
				return cancel;
			}
			set{
				cancel=value;
			}
		}
	}

	///<summary></summary>
	public delegate void ButtonClickedEventHandler(object sender,ButtonClicked_EventArgs e);
	#endregion EventArgs


}







