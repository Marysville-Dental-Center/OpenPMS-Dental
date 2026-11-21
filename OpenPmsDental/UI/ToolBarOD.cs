using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;

namespace OpenPmsDental.UI{
/*
Jordan is the only one allowed to edit this file.
Typically dock it to the top of a form in the designer.  
Unlike the MS menu, we don't add menu items in the designer.
Boilerplate for adding menu items:

using OpenPmsDental.UI;

		private void LayoutToolBar() {
			toolBarMain.Buttons.Clear();
			toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this,"Add"),EnumIcons.Add,"","Add"));
			toolBarMain.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
			toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this,"Edit"),1,Lan.g(this,"Edit Selected Account"),"Edit"));
			toolBarMain.Buttons.Add(new ODToolBarButton(Lan.g(this,"Info"),bitmap,"","Info"));
		}

		private void toolBarMain_ButtonClick(object sender,OpenPmsDental.UI.ODToolBarButtonClickEventArgs e) {
			switch(e.Button.Tag.ToString()) {
				case "Add":
					Add_Click();//or, put short block of code here
					break;
				case "Edit":
					Edit_Click();
					break;
			}
		}

Notice that there are 3 ways to add images. Examples of each are above.
An ImageIndex picks a picture of the toolbar's ImageList. Since UI-9b a module gives its toolbar a list of icons once (toolBarMain.SetImageListIcons,
made by IconLibrary.GetImageList at the toolbar's scale) and its own buttons take an EnumIcons; the list stays for the program links (ProgramL) and
plug-ins, which add pictures by key and set ImageIndex.
Boilerplate for the new WpfControls.UI.ToolBar is over in ToolBar.xaml.cs
*/

	///<summary>Open Dental Toolbar. Jordan is the only one allowed to edit this file.</summary>
	[DefaultEvent("ButtonClick")]
	public class ToolBarOD : System.Windows.Forms.UserControl{
		#region Fields
		private ODToolBarButtonCollection buttons=new ODToolBarButtonCollection();
		private ImageList imageList;
		///<summary>The icons SetImageListIcons built the ImageList from (UI-9b), or null when the ImageList is a 96 DPI list set by ImageList (a
		///designer stream). With icons the list is at the toolbar's scale (SIZE_IMAGE_LIST96 scaled) and is built again when the scale changes.</summary>
		private EnumIcons[] _arrayIconsImageList;
		///<summary>The size of an ImageList picture at 96 DPI: the module toolbars' 22 px pictures, the same slot an Icon button draws in, so a
		///button keeps its width when its picture becomes an icon.</summary>
		public const int SIZE_IMAGE_LIST96=22;
		private bool _isMouseDown;
		///<summary>Just holds the scaling factor.</summary>
		public LayoutManagerForms LayoutManager=new LayoutManagerForms();
		///<summary>A hot button is either: 1.The button that the mouseDown happened on, regardless of the current position of the mouse, or 2.If the mouse is not down, the button in State.Hover. Keeping track of which one is hot allows faster painting during mouse events.</summary>
		///<summary>Either 1 or 2, depending on the scale.  Float thicknesses don't look good.  Not all lines are affected, only the ones that need to be seen.</summary>
		private int _lineThickness;
		private ODToolBarButton _toolBarButtonHot;
		private ToolTip toolTip1;
		///<summary>This can be set from anywhere to affect all toolbars simultaneously.</summary>
		private ValidNum textPageNav;
		//The colours, pens and brushes are UiTheme's tokens (UI-6, docs/specs/ui-refresh-plan.md section 3.6): shared per thread, never disposed here.
		//string formats are disposed
		private StringFormat _stringFormatCenter=null;
		private StringFormat _stringFormatLeft=null;
		///<summary>This is temporarily used to fix artifacts from Direct2D.</summary>
		Timer timer;
		private int _widthDropDown=15;
		#endregion Fields

		#region Constructor
		///<summary></summary>
		public ToolBarOD(){
			InitializeComponent();// This call is required by the Windows.Forms Form Designer.
			Font=(Font)UiTheme.FontBody.Clone();//The body token (UI-2a-5): Microsoft Sans Serif 8.25 until UI-2b.
			this.DoubleBuffered=true;
			toolTip1 = new ToolTip();
			toolTip1.InitialDelay=1100;
			_stringFormatCenter=new StringFormat();
			_stringFormatCenter.Alignment=StringAlignment.Center;
			_stringFormatCenter.LineAlignment=StringAlignment.Center;
			_stringFormatLeft=new StringFormat();
			_stringFormatLeft.Alignment=StringAlignment.Near;
			_stringFormatLeft.LineAlignment=StringAlignment.Center;
			timer=new Timer();
			timer.Interval=1000;//redraw every 1 second to remove artifacts
			timer.Enabled=true;
			timer.Tick+=timer_Tick;
		}
		#endregion Constructor

		#region Designer
		///<summary>Required designer variable.</summary>
		private System.ComponentModel.Container components = null;

		///<summary>Clean up any resources being used.</summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				components?.Dispose();
				_stringFormatCenter?.Dispose();
				_stringFormatLeft?.Dispose();
				for(int i=0;i<Buttons.Count;i++){
					Buttons[i].Dispose();
				}
				if(_arrayIconsImageList!=null){
					imageList?.Dispose();//the list of icons SetImageListIcons made is the toolbar's own
				}
			}
			base.Dispose( disposing );
		}
		#endregion Designer

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// ToolBarOD
			// 
			this.DoubleBuffered = true;
			this.Name = "ToolBarOD";
			this.ResumeLayout(false);

		}
		#endregion

		#region Events - Raise
		///<summary></summary>
		protected void OnButtonClicked(ODToolBarButton toolBarButton){
			if(toolBarButton.DateTimeLastClicked.AddMilliseconds(SystemInformation.DoubleClickTime)>DateTime.Now) {
				return;//ignore a second click that happens too quickly
			}
			toolBarButton.DateTimeLastClicked=DateTime.Now;
			if(toolBarButton.EventHandlerClick==null){
				ODToolBarButtonClickEventArgs toolBarButtonClickEventArgs=new ODToolBarButtonClickEventArgs(toolBarButton);
				ButtonClick?.Invoke(this,toolBarButtonClickEventArgs);
				return;
			}
			toolBarButton.EventHandlerClick.Invoke(this,new EventArgs());
		}
		[Category("OD")]
		[Description("Occurs when a button is clicked.")]
		public event ODToolBarButtonClickEventHandler ButtonClick=null;

		///<summary>Raised when the dropdown arrow of a DropDownButton is clicked and the button has no dropdown menu: the arrow then opens a window
		///instead of a menu (the Account module's Statement button since Open Dental 26.2, P262-BILLING). A button with a menu still shows its menu
		///and never raises this.</summary>
		protected void OnButtonDropDownClicked(ODToolBarButton toolBarButton){
			if(toolBarButton.DateTimeLastClicked.AddMilliseconds(SystemInformation.DoubleClickTime)>DateTime.Now) {
				return;//ignore a second click that happens too quickly
			}
			toolBarButton.DateTimeLastClicked=DateTime.Now;
			ButtonDropDownClick?.Invoke(this,new ODToolBarButtonClickEventArgs(toolBarButton));
		}
		[Category("OD")]
		[Description("Occurs when the dropdown arrow of a dropdown button without a dropdown menu is clicked.")]
		public event ODToolBarButtonClickEventHandler ButtonDropDownClick=null;

		///<summary></summary>
		private void OnPageNav(int pageNum){
			PageNav?.Invoke(this,new ODToolBarButtonPageNavEventArgs(pageNum));
		}
		[Category("OD")]
		[Description("Occurs when user types number and hits Enter in a page navigation textbox.")]
		public event ODToolBarButtonPageNavEventHandler PageNav=null;
		#endregion Events - Raise

		#region Methods - Event Handlers
		private void TextPageNav_KeyDown(object sender,KeyEventArgs e) {
			base.OnKeyDown(e);
			if(e.KeyCode==Keys.Enter && textPageNav.IsValid()) {
				int pageNum=textPageNav.MaxVal;
				try {
					pageNum=Int32.Parse(textPageNav.Text);
				}
				catch(Exception) {
					return;
				}
				OnPageNav(pageNum);
			}
		}

		private void timer_Tick(object sender,EventArgs e) {
			//this happens soon after the last resize
			Invalidate();
			if(timer.Interval==1000){
				timer.Enabled=false;//so it only runs twice
			}
			timer.Interval=1000;//set it back to the longer interval
		}
		#endregion Methods - Event Handlers

		#region Methods - On Override
		///<summary></summary>
		protected override void OnLoad(EventArgs e){
			base.OnLoad(e);
			this.InitializeBrushes();
		}

		///<summary>Change the button to a pressed state.</summary>
		protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e){
			base.OnMouseDown(e);
			if((e.Button & MouseButtons.Left)!=MouseButtons.Left){
				return;
			}
			_isMouseDown=true;
			ODToolBarButton button=HitTest(e.X,e.Y);
			if(button==null){//if there is no current hover button
				return;//don't set a hotButton
			}
			//if(!button.Enabled){
			//	return;//disabled buttons don't respond
			//}
			_toolBarButtonHot=button;
			if(button.Style==ODToolBarButtonStyle.DropDownButton
				&& HitTestDrop(button,e.X,e.Y))
			{
				button.State=ToolBarButtonState.DropPressed;
			}
			else{
				button.State=ToolBarButtonState.Pressed;
				
			}
			Invalidate();//button.Bounds);
		}

		///<summary>Resets button appearance. This will also deactivate the button if it has been pressed but not released. A pressed button will still be hot, however, so that if the mouse enters again, it will behave properly.  Repaints only if necessary.</summary>
		protected override void OnMouseLeave(System.EventArgs e){
			base.OnMouseLeave(e);
			if(_isMouseDown){//mouse is down
				//if a button is hot, it will remain so, even if leave.  As long as mouse is down.
				//,so do nothing.
				//Also, if a button is not hot, nothing will happen when leave
				//,so do nothing.
			}
			else{//mouse is not down
				if(_toolBarButtonHot!=null){//if there was a previous hotButton
					_toolBarButtonHot.State=ToolBarButtonState.Normal;
					Invalidate();//_toolBarButtonHot.Bounds);
					_toolBarButtonHot=null;
				}
			}
		}

		///<summary>This should only happen when mouse enters. Only causes a repaint if needed.</summary>
		protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e){
      base.OnMouseMove(e);
			if(_isMouseDown){
				//regardless of whether a button is hot, nothing changes until the mouse is released.
				//a hot(pressed) button remains so, and no buttons are hot when hover
				//,so do nothing
				return;
			}
			//mouse is not down
			ODToolBarButton toolBarButtonOver=HitTest(e.X,e.Y);
			if(_toolBarButtonHot!=null){//first handle any old hot button
				if(_toolBarButtonHot!=toolBarButtonOver){//if we have moved to hover over a new button, or to hover over nothing
					_toolBarButtonHot.State=ToolBarButtonState.Normal;
					Invalidate();//_toolBarButtonHot.Bounds);
				}
			}
			if(toolBarButtonOver!=null){//then, the new button
				if(_toolBarButtonHot!=toolBarButtonOver){//if we have moved to hover over a new button
					toolTip1.SetToolTip(this,toolBarButtonOver.ToolTipText);
					toolBarButtonOver.State=ToolBarButtonState.Hover;
					Invalidate();//toolBarButtonOver.Bounds);
				}
				else{//Still hovering over the same button as before
					//do nothing.
				}
			}
			else{
				toolTip1.SetToolTip(this,"");
			}
			_toolBarButtonHot=toolBarButtonOver;//this might be null if hovering over nothing.
		}

		///<summary>Change button to hover state and repaint if needed.</summary>
		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e){
			base.OnMouseUp(e);
      if((e.Button & MouseButtons.Left)!=MouseButtons.Left){
				return;
			}
			//Make sure we registered a MouseDown event inside of OD, otherwise, we might mistakenly act on a mouse_up that did not begin inside OD.
			if(!_isMouseDown) {
				return;
			}
			_isMouseDown=false;
			ODToolBarButton toolBarButton=HitTest(e.X,e.Y);
			if(_toolBarButtonHot==null){//if there was not a previous hotButton
				//do nothing
			}	
			else{//there was a hotButton
				_toolBarButtonHot.State=ToolBarButtonState.Normal;
				//but can't set it null yet, because still need it for testing
				Invalidate();//_toolBarButtonHot.Bounds);//invalidate the old button
				//CLICK: 
				if(_toolBarButtonHot==toolBarButton){//if mouse was released over the same button as it was depressed
					if(!toolBarButton.Enabled){
						//disabled buttons don't respond at all
					}
					else if(toolBarButton.Style==ODToolBarButtonStyle.DropDownButton//if current button is dropdown
						&& HitTestDrop(toolBarButton,e.X,e.Y))//and we are in the dropdown area on the right
					{
						if(toolBarButton.ContextMenuStripDropDown!=null){//there is a dropdown menu strip to display, below the button
							_toolBarButtonHot=null;
							toolBarButton.State=ToolBarButtonState.Normal;
							Invalidate();
							toolBarButton.ContextMenuStripDropDown.Show(this,new Point(toolBarButton.Bounds.X,toolBarButton.Bounds.Y+toolBarButton.Bounds.Height));
						}
						if(toolBarButton.ContextMenuStripDropDown==null){//the arrow opens a window (P262-BILLING)
							_toolBarButtonHot=null;
							toolBarButton.State=ToolBarButtonState.Normal;
							Invalidate();
							OnButtonDropDownClicked(toolBarButton);
						}
					}
					else if(toolBarButton.Style==ODToolBarButtonStyle.ToggleButton){//if current button is a toggle button
						if(toolBarButton.IsTogglePushed){
							toolBarButton.IsTogglePushed=false;
						}
						else{
							toolBarButton.IsTogglePushed=true;
						}
						OnButtonClicked(toolBarButton);
					}
					else if(toolBarButton.Style==ODToolBarButtonStyle.Label){
						//lables do not respond with click
					}
					else{
						OnButtonClicked(toolBarButton);
					}
					return;//the button will not revert back to hover
				}//end of click section
				else{//there was a hot button, but it did not turn into a click
					_toolBarButtonHot=null;
				}
			}
			if(toolBarButton!=null){//no click, and now there is a hover button, not the same as original button.
				//this section could easily be deleted, since all the user has to do is move the mouse slightly.
				toolBarButton.State=ToolBarButtonState.Hover;
				_toolBarButtonHot=toolBarButton;//set the current hover button to be the new hotbutton
				Invalidate();//toolBarButton.Bounds);
			}
		}

		///<summary>Runs any time the control is invalidated.</summary>
		protected override void OnPaint(System.Windows.Forms.PaintEventArgs e){
			if(DesignMode){
				e.Graphics.DrawRectangle(Pens.SlateGray,0,0,Width-1,Height-1);
				StringFormat format=new StringFormat();
				format.Alignment=StringAlignment.Center;
				format.LineAlignment=StringAlignment.Center;
				e.Graphics.DrawString(this.Name,Font,Brushes.Black,new Rectangle(0,0,Width,Height),format);
				return;
			}
			//e.Graphics.SmoothingMode=SmoothingMode.HighQuality;
			//OnPaint gets called a lot and the button collection can change while it is in the middle of drawing which can crash the program.
			//It's easier to just swallow any random exception that occurs because OnPaint will most likely get fired again real soon.
			ODException.SwallowAnyException(() => {
				ComputeButtonsizes(e.Graphics);
				DrawBackground(e.Graphics);
				for(int i=0;i<buttons.Count;i++) {
					DrawButton(e.Graphics,buttons[i]);
				}
				//For quite some time, the Paint event was never fired here.  Added by Sam and AndrewD so we can subscribe to the Paint event.
				base.OnPaint(e);
			});
		}

		protected override void OnSizeChanged(EventArgs e){
			base.OnSizeChanged(e);
			InitializeBrushes();
			if(timer.Enabled){
				timer.Enabled=false;//restart the timer
			}
			timer.Interval=100;//a bit faster than normal
			timer.Enabled=true;
		}
		#endregion Methods - On Override

		#region Properties
		///<summary>Gets the collection of ODToolBarButton controls assigned to the toolbar control.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] 
		public ODToolBarButtonCollection Buttons{
			get{
				return buttons;
			}
			//set{
			//}	
		}

		protected override Size DefaultSize => new Size(300,25);

		///<summary>Gets or sets the collection of images available to the toolbar buttons (ImageIndex). A list set here is taken as 96 DPI pictures,
		///drawn scaled as before; SetImageListIcons gives the toolbar a list of icons at its scale instead (UI-9b). Program links
		///(ProgramL.LoadToolBar) and plug-ins (the LayoutToolBar_end hooks) add their pictures to this list, by key, and set ImageIndex.</summary>
		[Category("OD")]
		[Description("Gets or sets the collection of images available to the toolbar buttons.")]
		[DefaultValue(null)]
		public ImageList ImageList{
			get{
				return imageList;
			}
			set{
				if(value!=imageList){
					_arrayIconsImageList=null;//a list of the caller's: 96 DPI pictures
				}
				imageList=value;
			}
		}
		#endregion Properties

		#region Methods - Public
		///<summary>Gives the toolbar an ImageList of icons (UI-9b, docs/port-notes/UI-9b.md), made by the icon factory IconLibrary.GetImageList at
		///the toolbar's scaled 22 px slot (SIZE_IMAGE_LIST96), each icon under its member's name as the key. A module passes its pictures' purposes
		///in today's ImageIndex order, so a button that a plug-in sets by ImageIndex (the LayoutToolBar_end hooks) still shows the same picture's
		///icon. Program links and plug-ins add their own pictures to ImageList after it, by key, as before; the list draws them at the scale too.
		///When the scale changes (DPI, zoom) the list is built again at the new size before the next paint, with those added pictures copied over
		///under their keys and at their indexes. The toolbar owns the list.</summary>
		public void SetImageListIcons(params EnumIcons[] arrayIcons){
			ImageList imageListOld=imageList;
			imageList=IconLibrary.GetImageList(arrayIcons,LayoutManager.Scale(SIZE_IMAGE_LIST96));
			_arrayIconsImageList=arrayIcons;
			imageListOld?.Dispose();
		}
		#endregion Methods - Public

		#region Methods - Private
		///<summary>The size of the ImageList's pictures at 96 DPI: SIZE_IMAGE_LIST96 for a list of icons (it holds them at the scale), else the
		///list's own size (a 96 DPI list, scaled when drawn).</summary>
		private Size GetImageListSize96(){
			if(_arrayIconsImageList!=null){
				return new Size(SIZE_IMAGE_LIST96,SIZE_IMAGE_LIST96);
			}
			return imageList.ImageSize;
		}

		///<summary>Builds the list of icons again when the toolbar's scale no longer matches its size (SetImageListIcons): the icons at the new
		///size, then the pictures added after them (program links, plug-ins) copied over, each under its key, so every ImageIndex and key stays.</summary>
		private void RescaleImageListIcons(){
			if(_arrayIconsImageList==null || imageList==null){
				return;
			}
			int sizePixels=Math.Max(LayoutManager.Scale(SIZE_IMAGE_LIST96),1);
			if(imageList.ImageSize.Width==sizePixels && imageList.ImageSize.Height==sizePixels){
				return;
			}
			ImageList imageListOld=imageList;
			ImageList imageListNew=IconLibrary.GetImageList(_arrayIconsImageList,sizePixels);
			for(int i=_arrayIconsImageList.Length;i<imageListOld.Images.Count;i++){
				Image image=imageListOld.Images[i];//a copy the list makes; the new list keeps it
				string key=imageListOld.Images.Keys[i];
				if(string.IsNullOrEmpty(key)){
					imageListNew.Images.Add(image);
				}
				else{
					imageListNew.Images.Add(key,image);
				}
			}
			imageList=imageListNew;
			imageListOld.Dispose();
		}

		private void ComputeButtonsizes(Graphics g){
			RescaleImageListIcons();
			_widthDropDown=LayoutManager.Scale(15);//The width of the dropdown rectangle where the triangle shows.
			int xPos=0;
			int width;
			for(int i=0;i<buttons.Count;i++){
				if(buttons[i].Style==ODToolBarButtonStyle.Separator) {
					width=LayoutManager.Scale(4);
				}
				else if(buttons[i].Style==ODToolBarButtonStyle.PageNav) {
					//Set the size of the button to the width measurement of "0000/0000" with some padding.
					//The page navigation is tailored to print previews and users shouldn't have reports much larger than 9999 pages.
					width=(int)g.MeasureString("0000/0000",Font).Width+LayoutManager.Scale(12);
				}
				else if(buttons[i].Bitmap!=null){
					int widthImage=LayoutManager.Scale(buttons[i].Bitmap.Width);
					if(buttons[i].Text==""){
						width=widthImage+LayoutManager.Scale(7);//slightly wider than high for better 'feel'
					}
					else{
						width=widthImage+(int)g.MeasureString(buttons[i].Text,Font).Width+LayoutManager.Scale(8);
					}
				}
				else if(buttons[i].Icon!=EnumIcons.None){//button with Icon
					int widthImage=LayoutManager.Scale(22);
					if(buttons[i].Text==""){
						width=widthImage+LayoutManager.Scale(7);//slightly wider than high for better 'feel'
					}
					else{
						width=widthImage+(int)g.MeasureString(buttons[i].Text,Font).Width+LayoutManager.Scale(8);
					}
				}
				else if(imageList==null || buttons[i].ImageIndex==-1) {//Normal button
					width=(int)g.MeasureString(buttons[i].Text,Font).Width+LayoutManager.Scale(6);
				}
				else {//Image List button (being deprecated)
					int widthImage=LayoutManager.Scale(GetImageListSize96().Width);
					if(buttons[i].Text==""){
						width=widthImage+LayoutManager.Scale(7);//slightly wider than high for better 'feel'
					}
					else{
						width=widthImage+(int)g.MeasureString(buttons[i].Text,Font).Width+LayoutManager.Scale(8);
					}
				}
				//Check to see if this button needs a drop down.
				if(buttons[i].Style==ODToolBarButtonStyle.DropDownButton){
					if(buttons[i].Text==""){
						width-=LayoutManager.Scale(6);//10//this way the main button part can be zero width if we just want the dropdown and nothing else.
					}
					width+=_widthDropDown;
				}
				buttons[i].Bounds=new Rectangle(new Point(xPos,0),new Size(width,Height-1));
				xPos+=buttons[i].Bounds.Width;
			}
		}

		///<summary>The toolbar's background (UI-6, section 3.6, Toolbars): flat SurfaceSubtle with a 1 px Gridline line at the bottom.</summary>
		private void DrawBackground(Graphics g){
			g.FillRectangle(UiTheme.GetBrush(EnumUiColor.SurfaceSubtle),ClientRectangle);
			int widthLine=UiTheme.GetLineWidth(LayoutManager.ScaleMy());
			g.FillRectangle(UiTheme.GetBrush(EnumUiColor.Gridline),0,Height-widthLine,Width,widthLine);
		}

		///<summary>Draws one button in the new look (UI-6, section 3.6, Toolbars), inside its bounds, which keep today's width formula
		///(ComputeButtonsizes, section 3.3): transparent at rest; SubtleHover on hover and SubtlePressed pressed, with 4 px corners, 1 px in from the
		///bounds; toggled AccentSubtle with a 1 px Accent border; error (IsRed) DangerSubtle with Danger text; notify (a drop-down's NotificationText)
		///WarningSubtle with Warning text; disabled TextDisabled; the drop-down arrow a 12 px Fluent chevron; separators a 1 px Stroke line, 4 px in
		///from the top and the bottom. No gradients and no outlines between buttons.</summary>
		private void DrawButton(Graphics g,ODToolBarButton button){
			float scale=LayoutManager.ScaleMy();
			int widthLine=UiTheme.GetLineWidth(scale);
			if(button.Style==ODToolBarButtonStyle.Separator){
				int inset=LayoutManager.Scale(UiTheme.INSET_TOOLBAR_SEPARATOR);
				g.FillRectangle(UiTheme.GetBrush(EnumUiColor.Stroke),button.Bounds.X+(button.Bounds.Width-widthLine)/2,button.Bounds.Y+inset,
					widthLine,Math.Max(0,button.Bounds.Height-2*inset));
				return;
			}
			bool isNotify=(button.Style==ODToolBarButtonStyle.DropDownButton && !string.IsNullOrWhiteSpace(button.NotificationText));//Notifies even when disabled.
			bool isLabel=button.Style==ODToolBarButtonStyle.Label || button.Style==ODToolBarButtonStyle.PageNav;
			//The shape and the text colour of the state.
			EnumUiColor? uiColorFill=null;
			EnumUiColor? uiColorOutline=null;
			EnumUiColor uiColorText=EnumUiColor.Text;
			bool isDropPressed=false;
			if(!button.Enabled){
				uiColorText=EnumUiColor.TextDisabled;
				if(isNotify){
					uiColorFill=EnumUiColor.WarningSubtle;
				}
			}
			else if(isLabel){
				//No shape: labels and the page navigation do not respond to the mouse.
			}
			else if(button.Style==ODToolBarButtonStyle.ToggleButton && button.IsTogglePushed){
				uiColorFill=button.IsRed ? EnumUiColor.DangerSubtle : EnumUiColor.AccentSubtle;
				uiColorOutline=button.IsRed ? EnumUiColor.Danger : EnumUiColor.Accent;
				uiColorText=button.IsRed ? EnumUiColor.Danger : EnumUiColor.TextSelected;
			}
			else if(button.IsRed){//error colours. Only used for DoseSpot
				uiColorFill=EnumUiColor.DangerSubtle;
				uiColorText=EnumUiColor.Danger;
				if(button.State!=ToolBarButtonState.Normal){
					uiColorOutline=EnumUiColor.Danger;
				}
			}
			else if(isNotify){
				uiColorFill=EnumUiColor.WarningSubtle;
				uiColorText=EnumUiColor.Warning;
				if(button.State!=ToolBarButtonState.Normal){
					uiColorOutline=EnumUiColor.Warning;
				}
			}
			else{
				switch(button.State){
					case ToolBarButtonState.Hover:
						uiColorFill=EnumUiColor.SubtleHover;
						break;
					case ToolBarButtonState.Pressed:
						uiColorFill=EnumUiColor.SubtlePressed;
						break;
					case ToolBarButtonState.DropPressed:
						uiColorFill=EnumUiColor.SubtleHover;
						isDropPressed=true;
						break;
				}
			}
			Rectangle rectangleShape=Rectangle.Inflate(button.Bounds,-LayoutManager.Scale(1),-LayoutManager.Scale(1));
			int radius=UiTheme.GetCornerRadius(UiTheme.CORNER_CONTROL,scale);
			if(uiColorFill!=null){
				UiToolStripRenderer.FillRounded(g,uiColorFill.Value,rectangleShape,radius);
			}
			if(isDropPressed){
				//The drop-down part pressed: SubtlePressed on the right of the shape, within its corners.
				GraphicsState graphicsState=g.Save();
				g.SetClip(new Rectangle(button.Bounds.Right-_widthDropDown,button.Bounds.Y,_widthDropDown,button.Bounds.Height));
				UiToolStripRenderer.FillRounded(g,EnumUiColor.SubtlePressed,rectangleShape,radius);
				g.Restore(graphicsState);
			}
			if(uiColorOutline!=null){
				UiToolStripRenderer.OutlineRounded(g,uiColorOutline.Value,rectangleShape,radius,widthLine);
			}
			Brush brushText=UiTheme.GetBrush(uiColorText);
			//draw image and/or text
			Rectangle rectangleText;
			Rectangle rectangleImage;
			int textWidth=button.Bounds.Width;
			if(button.Style==ODToolBarButtonStyle.DropDownButton){
				textWidth-=_widthDropDown;
			}
			Bitmap bitmap=null;
			Size sizeImage=new Size();
			if(button.Icon!=EnumIcons.None){
				sizeImage=new Size(LayoutManager.Scale(22),LayoutManager.Scale(22));
			}
			else if(button.Bitmap!=null){
				bitmap=button.Bitmap;
				sizeImage=new Size(LayoutManager.Scale(bitmap.Width),LayoutManager.Scale(bitmap.Height));
			}
			else if(imageList!=null && button.ImageIndex!=-1 && button.ImageIndex<imageList.Images.Count){
				bitmap=(Bitmap)imageList.Images[button.ImageIndex];
				Size sizeImage96=GetImageListSize96();//a list of icons is at the scale already: drawn pixel for pixel
				sizeImage=new Size(LayoutManager.Scale(sizeImage96.Width),LayoutManager.Scale(sizeImage96.Height));
			}
			if(button.Icon!=EnumIcons.None || bitmap!=null){//draw image and text
				//The image keeps its place in every state (no shift when pressed: the flat look shows the press by its colour).
				rectangleImage=new Rectangle(button.Bounds.X+LayoutManager.Scale(3),button.Bounds.Y+LayoutManager.Scale(1),sizeImage.Width,sizeImage.Height);
				if(button.Icon!=EnumIcons.None){
					if(button.Enabled){
						DrawIconTransparent(g,button.Icon,rectangleImage,IconLibrary.GetColor(button.Icon));
					}
					else{
						IconLibrary.DrawDisabled(g,button.Icon,rectangleImage);
					}
				}
				else if(!button.Enabled){
					Bitmap bitmapDisabled=new Bitmap(bitmap.Width,bitmap.Height);//disabled bitmap is old small size
					Graphics gfx=Graphics.FromImage(bitmapDisabled);
					ControlPaint.DrawImageDisabled(gfx,bitmap,0,0,UiTheme.SurfaceSubtle);
					g.DrawImage(bitmapDisabled,rectangleImage);
					gfx.Dispose();
					bitmapDisabled.Dispose();//An alternative would be to build the image once, and store it as part of the button
				}
				else{
					g.DrawImage(bitmap,rectangleImage);
				}
				rectangleText=new Rectangle(button.Bounds.X+sizeImage.Width+LayoutManager.Scale(3),button.Bounds.Y,
					textWidth-sizeImage.Width-LayoutManager.Scale(3),button.Bounds.Height);
			}
			else{//only draw text
				rectangleText=new Rectangle(button.Bounds.X,button.Bounds.Y,textWidth,button.Bounds.Height);
			}
			if(imageList!=null && button.ImageIndex!=-1){//if there is an image
				g.DrawString(button.Text,Font,brushText,rectangleText,_stringFormatLeft);
			}
			else{
				g.DrawString(button.Text,Font,brushText,rectangleText,_stringFormatCenter);
				//For page navigation buttons we ALWAYS show the text box so that users know they can type in the box to change pages.
				if(button.Enabled && button.Style==ODToolBarButtonStyle.PageNav) {
					//Because we do not display zeros to users (e.g. 001 / 135) and instead leave them off (e.g. 1 / 135) the width will be lopsided
					//We need to draw each side of the page navigation (and pad a little) individually.
					SizeF size = g.MeasureString("/",Font);
					g.DrawString(button.PageValue.ToString(),Font,brushText
						,new Rectangle(rectangleText.Location,new Size((rectangleText.Width/2)-(int)(size.Width/2)-2,rectangleText.Height))
						,new StringFormat() { Alignment=StringAlignment.Far,LineAlignment=StringAlignment.Center });
					g.DrawString(button.PageMax.ToString(),Font,brushText
						,new Rectangle(new Point(rectangleText.X+(rectangleText.Width/2)+(int)(size.Width/2)+2,rectangleText.Y)
						,new Size((rectangleText.Width/2)-(int)(size.Width/2),rectangleText.Height))
						,new StringFormat() { Alignment=StringAlignment.Near,LineAlignment=StringAlignment.Center });
					//Only add the text box for page navigation once.
					//However, we need to add the text box here in the paint otherwise we don't know how large to make the text box.
					if(textPageNav==null) {
						textPageNav=new ValidNum();
						textPageNav.Font=Font;
						textPageNav.Size=new Size(button.Bounds.Width/2-LayoutManager.Scale(10),button.Bounds.Height);
						//height gets ignored, but is used below
						textPageNav.KeyDown+=TextPageNav_KeyDown;
						textPageNav.MinVal=1;//There is no such thing as 0 pages in a preview, always set min to 1.
						textPageNav.TextAlign=HorizontalAlignment.Right;
						LayoutManager.Add(textPageNav,this);
					}
					int xCenter=button.Bounds.X+button.Bounds.Width/2;
					int yCenter=button.Bounds.Y+button.Bounds.Height/2;
					int wText=button.Bounds.Width/2-LayoutManager.Scale(10);
					LayoutManager.Move(textPageNav,new Rectangle(
						xCenter-wText-LayoutManager.Scale(4),yCenter-textPageNav.Height/2,wText,textPageNav.Height));
					//LayoutManager.MoveLocation(textPageNav,new Point(button.Bounds.X+1,button.Bounds.Y+2));
					//Nav text changes constantly so we need to update it when redrawing.
					textPageNav.Text=button.PageValue.ToString();
					if(button.PageMax!=0) {
						textPageNav.MaxVal=button.PageMax;
					}
				}
			}
			if(button.Style==ODToolBarButtonStyle.DropDownButton){
				int adjDown=0;//The distance to push the chevron down to show the notification text.
				if(isNotify) {
					adjDown=LayoutManager.Scale(6);
					//Draw the notification text.
					Size sizeText=TextRenderer.MeasureText(button.NotificationText,Font);
					g.DrawString(button.NotificationText,Font,brushText,
						button.Bounds.X+button.Bounds.Width+1-(_widthDropDown+sizeText.Width)/2f,button.Bounds.Y+2+sizeText.Height/2f,_stringFormatLeft);
				}
				//The 12 px Fluent chevron (UI-7a's glyph), centred in the drop-down part, in the text's colour.
				int sizeChevron=LayoutManager.Scale(UiTheme.SIZE_CHEVRON);
				Rectangle rectangleChevron=new Rectangle(button.Bounds.Right-_widthDropDown+(_widthDropDown-sizeChevron)/2,
					button.Bounds.Y+(button.Bounds.Height-sizeChevron)/2+adjDown,sizeChevron,sizeChevron);
				UiIcons.Draw(g,UiGlyph.ChevronDown,rectangleChevron,UiTheme.GetColor(uiColorText));
			}
		}

		///<summary>Draws an EnumIcons icon in the colour, transparently over whatever the control has drawn under it (GDI+, CodeBase.UiIcons, the
		///same shapes antialiased), so no box shows behind the icon on a hover, pressed or toggled shape: IconLibrary.Draw fills an opaque Direct2D
		///bitmap over the colours it is given (UI-7b's review), which would show its square inside a rounded shape. isFilled: the icon's Filled style
		///(the selected module). Every icon is a vector design: the chart's icons draw UI-8's custom dental icons (IconLibrary.GetDentalIcon), Chart32W
		///with isFilled the ToothFilled.</summary>
		internal static void DrawIconTransparent(Graphics g,EnumIcons icon,Rectangle rectangle,Color color,bool isFilled=false){
			if(icon==EnumIcons.None){
				return;
			}
			UiIcons.Draw(g,IconLibrary.GetDesign(icon,Math.Min(rectangle.Width,rectangle.Height),isFilled),rectangle,color);
		}

		///<summary>Returns the button that contains these coordinates, or null if no hit.</summary>
		private ODToolBarButton HitTest(int x,int y){
			for(int i=0;i<buttons.Count;i++){
				if(buttons[i].Bounds.Contains(x,y)){
					return buttons[i];
				}
			}
			return null;
		}

		private bool HitTestDrop(ODToolBarButton button,int x,int y){
			Rectangle dropRect=new Rectangle(button.Bounds.X+button.Bounds.Width-_widthDropDown,button.Bounds.Y
				,_widthDropDown,button.Bounds.Height);
			if(dropRect.Contains(x,y)){
				return true;
			}
			return false;
		}

		///<summary>Sets the line width for the scale (UiTheme.GetLineWidth: 1 px up to 175%, 2 px at 200%). The colours are tokens: nothing to make.</summary>
		private void InitializeBrushes(){
			_lineThickness=UiTheme.GetLineWidth(LayoutManager.ScaleMy());
		}
		#endregion Methods - Private

	}

	#region Other Classes
	///<summary></summary>
	public delegate void ODToolBarButtonClickEventHandler(object sender,ODToolBarButtonClickEventArgs e);
	///<summary></summary>
	public delegate void ODToolBarButtonPageNavEventHandler(object sender,ODToolBarButtonPageNavEventArgs e);

	///<summary></summary>
	public class ODToolBarButtonClickEventArgs{
		private ODToolBarButton button;
	
		public ODToolBarButtonClickEventArgs(ODToolBarButton myButton){
			button=myButton;
		}

		///<summary></summary>
		public ODToolBarButton Button{
			get{ 
				return button;
			}
		}
	}	
	
	///<summary></summary>
	public class ODToolBarButtonPageNavEventArgs{
		private int _navValue;
		public ODToolBarButtonPageNavEventArgs(int navValue){
			_navValue=navValue;
		}

		///<summary></summary>
		public int NavValue{
			get{ 
				return _navValue;
			}
		}
	}
	#endregion Other Classes
}

