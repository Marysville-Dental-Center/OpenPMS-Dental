using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeBase;

namespace WpfControls.UI{
	///<summary></summary>
	public partial class ToolBarButton : UserControl{
		//public object Tag;//already has one
		private EnumIcons _icon;
		//private string _bitmapFileName;
		///<summary>If the button is a dropdown, then this is the context menu that will show.</summary>
		public ContextMenu ContextMenuDropDown;
		///<summary>The mouse is over the button (SubtleHover).</summary>
		private bool _isHover;
		///<summary>The left mouse button went down on the button and is still down (SubtlePressed).</summary>
		private bool _isPressed;
		///<summary>Only used if style is ToggleButton. AccentSubtle with an Accent border. No hover effect when a toggle is pushed.</summary>
		public bool IsTogglePushed;
		///<summary>Whether the icon is drawn in TextDisabled now, so it is drawn again only when that changes.</summary>
		private bool _isIconDisabled;
		private ToolBarButtonStyle _toolBarButtonStyle;
		private ToolTip _toolTip;
		private string _toolTipText;

		///<summary>The » glyph's two chevrons when this is a toolbar's overflow button (UI-OVF, SetGlyphOverflow), else empty.</summary>
		private List<System.Windows.Shapes.Path> _listPathsOverflow=new List<System.Windows.Shapes.Path>();
		///<summary>The menu this button opened is open (the overflow drop-down, UI-OVF): drawn pressed meanwhile.</summary>
		private bool _isMenuOpen;

		public ToolBarButton(){
			InitializeComponent();
			grid.ColumnDefinitions[2].Width=new GridLength(0);//hide the dropdown button
			pathChevron.Data=UiIcons.GetGeometryWpf(UiGlyph.ChevronDown,UiTheme.SIZE_CHEVRON);
			pathChevron.Visibility=Visibility.Collapsed;
			IsEnabledChanged+=This_IsEnabledChanged;
			//UI-OVF: reachable from the keyboard like the WinForms toolbar's buttons. The toolbar is one tab stop (ToolBar: TabNavigation Once) and
			//the arrow keys move between its buttons; the shared focus ring shows only with keyboard cues (FocusVisualStyle, section 3.6).
			Focusable=true;
			UiLook.SetFocusVisualStyle(this,onAccent:false);
			SetColors();
		}

		public ToolBarButton(string text,EventHandler eventHandlerClick,EnumIcons icon=EnumIcons.None,ToolBarButtonStyle toolBarButtonStyle=ToolBarButtonStyle.NormalButton,
			string toolTipText=null,ContextMenu contextMenuDropDown=null,object tag=null)
			:this()
		{
			Text=text;
			Click+=eventHandlerClick;
			Icon=icon;
			ToolBarButtonStyle=toolBarButtonStyle;
			SetToolTipText(toolTipText);
			ContextMenuDropDown=contextMenuDropDown;
			Tag=tag;
		}

		public event EventHandler Click;

		#region Properties
		//<summary>There's no need for this. All icons should be actual icons.</summary>
		//public string BitmapFileName {
		//	get {
		//		return _bitmapFileName;
		//	}
		//	set {
		//		_bitmapFileName=value;
		//		gridImage.Children.Clear();
		//		if(string.IsNullOrEmpty(_bitmapFileName)) {
		//			SetMargins();
		//			return;
		//		}
		//		Uri uri = new Uri("pack://application:,,,/WPFControlsOD;component/Resources/"+_bitmapFileName);
		//		BitmapImage bitmapImage = new BitmapImage(uri);
		//		Image image = new Image();
		//		image.Source=bitmapImage;
		//		gridImage.Children.Add(image);
		//		gridImage.Width=22;
		//		gridImage.Height=22;
		//		SetMargins();
		//	}
		//}

		public EnumIcons Icon{
			get{
				return _icon;
			}
			set{
				_icon=value;
				gridImage.Children.Clear();
				if(_icon==EnumIcons.None){
					SetMargins();
					return;
				}
				gridImage.Width=22;
				gridImage.Height=22;
				_isIconDisabled=!IsEnabled;
				DrawIcon();
				SetMargins();
			}
		}

		public string Text{
			get{
				return textBlock.Text;
			}
			set{
				textBlock.Text=value;
				SetMargins();
			}
		}

		public ToolBarButtonStyle ToolBarButtonStyle {
			get => _toolBarButtonStyle;
			set{
				_toolBarButtonStyle=value;
				if(_toolBarButtonStyle==ToolBarButtonStyle.DropDownButton){
					grid.ColumnDefinitions[2].Width=new GridLength(14);
					pathChevron.Visibility=Visibility.Visible;
				}
				else{
					grid.ColumnDefinitions[2].Width=new GridLength(0);
					pathChevron.Visibility=Visibility.Collapsed;
				}
			}
		}
		#endregion Properties

		public void SetBitmap(System.Drawing.Bitmap bitmap){
			using MemoryStream memoryStream = new MemoryStream();
			bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
			memoryStream.Position = 0;
			BitmapImage bitmapImage=new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.StreamSource = memoryStream;
			bitmapImage.CacheOption = BitmapCacheOption.OnLoad;//makes it load into memory during EndInit
			bitmapImage.EndInit();
			bitmapImage.Freeze(); //for use in another thread
			Image image=new Image();
			image.Source=bitmapImage;
			gridImage.Children.Clear();
			gridImage.Width=22;
			gridImage.Height=22;
			image.Width=22;
			image.Height=22;
			gridImage.Children.Add(image);
			SetMargins();
		}

		///<summary>The button's tooltip text, or null.</summary>
		public string ToolTipText=>_toolTipText;

		///<summary>True when the button has a click handler (a button without one is a label: "Scan:").</summary>
		public bool HasClick=>Click!=null;

		///<summary>Makes this the toolbar's » overflow button (UI-OVF): no text, the » glyph (two 12 px Fluent chevrons right, the second 4 px to the
		///right of the first, as UiToolStripRenderer.DrawOverflowGlyph) in the 22 px image slot, in the text's colour of the state.</summary>
		internal void SetGlyphOverflow(){
			gridImage.Children.Clear();
			gridImage.Width=22;
			gridImage.Height=22;
			_listPathsOverflow.Clear();
			for(int i=0;i<2;i++){
				System.Windows.Shapes.Path path=new System.Windows.Shapes.Path();
				path.Data=UiIcons.GetGeometryWpf(UiGlyph.ChevronRight,UiTheme.SIZE_CHEVRON);
				path.Width=UiTheme.SIZE_CHEVRON;
				path.Height=UiTheme.SIZE_CHEVRON;
				path.Stretch=Stretch.None;
				path.HorizontalAlignment=HorizontalAlignment.Left;
				path.VerticalAlignment=VerticalAlignment.Center;
				path.Margin=new Thickness(3+4*i,0,0,0);//(22-12-4)/2=3: the pair centred in the slot
				path.IsHitTestVisible=false;
				gridImage.Children.Add(path);
				_listPathsOverflow.Add(path);
			}
			SetMargins();
			SetColors();
		}

		///<summary>Draws the button pressed while the menu it opened is open (UI-OVF).</summary>
		internal void SetMenuOpen(bool isMenuOpen){
			_isMenuOpen=isMenuOpen;
			SetColors();
		}

		///<summary>A copy of the button's picture for a menu item (UI-OVF, the overflow drop-down), at the size: the icon drawn again, or the
		///bitmap's source. Null when the button has none.</summary>
		internal UIElement GetMenuIcon(double size){
			if(_icon!=EnumIcons.None){
				System.Windows.Controls.Grid gridIcon=new System.Windows.Controls.Grid();
				gridIcon.Width=size;
				gridIcon.Height=size;
				IconLibrary.DrawWpf(_icon,gridIcon);
				return gridIcon;
			}
			for(int i=0;i<gridImage.Children.Count;i++){
				if(gridImage.Children[i] is Image image && image.Source!=null){
					Image imageCopy=new Image();
					imageCopy.Source=image.Source;
					imageCopy.Width=size;
					imageCopy.Height=size;
					return imageCopy;
				}
			}
			return null;
		}

		///<summary>What a click on the button's main part does (the mouse, the keyboard and the toolbar's overflow drop-down): a toggle button flips,
		///then Click. Nothing when disabled or without a click handler (a label).</summary>
		public void PerformClick(){
			if(!IsEnabled || Click==null){
				return;
			}
			if(ToolBarButtonStyle==ToolBarButtonStyle.ToggleButton){
				IsTogglePushed=!IsTogglePushed;
				SetColors();
			}
			Click?.Invoke(this,new EventArgs());
		}

		///<summary>Opens the drop-down button's menu below it (the arrow's click). False when there is none.</summary>
		public bool ShowDropDown(){
			if(ToolBarButtonStyle!=ToolBarButtonStyle.DropDownButton || ContextMenuDropDown==null || !IsEnabled){
				return false;
			}
			ContextMenuDropDown.PlacementTarget=this;
			ContextMenuDropDown.Placement=System.Windows.Controls.Primitives.PlacementMode.Bottom;
			ContextMenuDropDown.IsOpen=true;
			return true;
		}

		///<summary>Enter and Space click the focused button; Down (or Alt+Down) opens a drop-down button's menu (UI-OVF). The toolbar handles the
		///arrow keys between its buttons and its overflow button's keys.</summary>
		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			if(e.Handled){
				return;
			}
			Key key=e.Key==Key.System ? e.SystemKey : e.Key;
			if((key==Key.Enter || key==Key.Space) && Keyboard.Modifiers==ModifierKeys.None){
				if(Click!=null){
					PerformClick();
					e.Handled=true;
				}
				return;
			}
			if(key==Key.Down && (Keyboard.Modifiers==ModifierKeys.None || Keyboard.Modifiers==ModifierKeys.Alt)){
				if(ShowDropDown()){
					e.Handled=true;
				}
			}
		}

		public void SetToolTipText(string toolTipText){
			if(string.IsNullOrEmpty(toolTipText)){
				return;
			}
			_toolTipText=toolTipText;
			_toolTip=new ToolTip();
			_toolTip.SetControlAndAction(this,ToolTipSetString);
			_toolTip.TimeSpanDelay=TimeSpan.FromSeconds(0.3);
		}

		///<summary></summary>
		private void ToolTipSetString(FrameworkElement frameworkElement,Point point) {
			if(point.X>grid.ColumnDefinitions[0].ActualWidth+grid.ColumnDefinitions[1].ActualWidth){
				//in dropdown area
				_toolTip.SetString(this,"");
				return;
			}
			_toolTip.SetString(this,_toolTipText);
		}

		private void SetMargins(){
			if(gridImage.Children.Count==0){//no icon
				gridImage.Margin=new Thickness(0);
				if(string.IsNullOrEmpty(textBlock.Text)){//neither icon nor text is not a likely scenario
					textBlock.Margin=new Thickness(4,0,4,0);//I guess leave some space so it doesn't completely collapse
					//this is also the default in the designer
				}
				else{
					//just text
					textBlock.Margin=new Thickness(4,0,4,0);
				}
			}
			else{
				//icon present
				if(string.IsNullOrEmpty(textBlock.Text)){
					//just icon
					gridImage.Margin=new Thickness(1,0,1,0);
					textBlock.Margin=new Thickness(0);
				}
				else{
					//both icon and text
					gridImage.Margin=new Thickness(2,0,2,0);
					textBlock.Margin=new Thickness(0,0,4,0);
				}
			}
		}

		protected override void OnMouseDown(MouseButtonEventArgs e) {
			base.OnMouseDown(e);
			if(_isHover){
				_isHover=false;
			}
			if(e.ChangedButton==MouseButton.Left && Click!=null){
				_isPressed=true;
			}
			SetColors();
			if(ToolBarButtonStyle==ToolBarButtonStyle.DropDownButton){
				if(e.GetPosition(this).X>=grid.ColumnDefinitions[0].ActualWidth+grid.ColumnDefinitions[1].ActualWidth){
					if(ContextMenuDropDown!=null){//there is a dropdown menu to display
						//default Placement is mouse point with no offset.
						ContextMenuDropDown.PlacementTarget=this;//relative to this button
						ContextMenuDropDown.Placement=System.Windows.Controls.Primitives.PlacementMode.Bottom;//Also aligns to left edge. No right align available. It looks good.
						ContextMenuDropDown.IsOpen=true;
					}
					return;
				}
			}
			if(ToolBarButtonStyle==ToolBarButtonStyle.ToggleButton){
				IsTogglePushed=!IsTogglePushed;
				SetColors();
				//Click will fire below
			}
			Click?.Invoke(this,new EventArgs());
		}

		protected override void OnMouseUp(MouseButtonEventArgs e) {
			base.OnMouseUp(e);
			if(_isPressed){
				_isPressed=false;
				_isHover=Click!=null && IsMouseOver;
				SetColors();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			base.OnMouseMove(e);
			bool isMouseDown=Mouse.LeftButton==MouseButtonState.Pressed;
			if(isMouseDown){
				//regardless of whether a button is hot, nothing changes until the mouse is released.
				//a hot(pressed) button remains so, and no buttons are hot when hover,so do nothing
				return;
			}
			//mouse is not down
			_isPressed=false;
			if(Click is null){
				//if no click event, then we don't show any hover effect. It's just a label.
				_isHover=false;
			}
			else{
				_isHover=true;
			}
			SetColors();
		}

		///<summary>Resets button appearance. This will also deactivate the button if it has been pressed but not released. A pressed button will still be hot, however, so that if the mouse enters again, it will behave properly.  Repaints only if necessary.</summary>
		protected override void OnMouseLeave(MouseEventArgs e){
			base.OnMouseLeave(e);
			bool isMouseDown=Mouse.LeftButton==MouseButtonState.Pressed;
			if(isMouseDown){
				//if a button is hot, it will remain so, even if leave.  As long as mouse is down,so do nothing.
				//Also, if a button is not hot, nothing will happen when leave,so do nothing.
				return;
			}
			//mouse is not down
			_isHover=false;
			_isPressed=false;
			SetColors();
		}

		private void This_IsEnabledChanged(object sender,DependencyPropertyChangedEventArgs e) {
			//This is nice because it gets hit when changing the property in the designer.
			SetColors();
		}

		///<summary>The state's look (UI-6, docs/specs/ui-refresh-plan.md section 3.6, Toolbars): transparent at rest; SubtleHover on hover and
		///SubtlePressed pressed (borderOverlay: 4 px corners, 1 px in from the bounds); toggled AccentSubtle with a 1 px Accent border; disabled
		///TextDisabled, the icon too. Text and chevron in Text.</summary>
		private void SetColors(){
			borderOverlay.BorderThickness=new Thickness(1);
			borderOverlay.BorderBrush=Brushes.Transparent;
			borderOverlay.Background=Brushes.Transparent;
			borderDropDown.Background=Brushes.Transparent;
			Brush brushText=UiTheme.GetBrushWpf(EnumUiColor.Text);
			if(!IsEnabled){
				brushText=UiTheme.GetBrushWpf(EnumUiColor.TextDisabled);
			}
			else if(ToolBarButtonStyle==ToolBarButtonStyle.ToggleButton && IsTogglePushed){
				//if toggled on, it will not respond to hover
				borderOverlay.Background=UiTheme.GetBrushWpf(EnumUiColor.AccentSubtle);
				borderOverlay.BorderBrush=UiTheme.GetBrushWpf(EnumUiColor.Accent);
				brushText=UiTheme.GetBrushWpf(EnumUiColor.TextSelected);
			}
			else if(_isPressed || _isMenuOpen){
				borderOverlay.Background=UiTheme.GetBrushWpf(EnumUiColor.SubtlePressed);
			}
			else if(_isHover){
				borderOverlay.Background=UiTheme.GetBrushWpf(EnumUiColor.SubtleHover);
			}
			textBlock.Foreground=brushText;
			pathChevron.Fill=brushText;
			for(int i=0;i<_listPathsOverflow.Count;i++){
				_listPathsOverflow[i].Fill=brushText;
			}
			if(_icon!=EnumIcons.None && _isIconDisabled==IsEnabled){
				_isIconDisabled=!IsEnabled;
				DrawIcon();
			}
		}

		///<summary>The icon in its role colour, or in TextDisabled while the button is disabled.</summary>
		private void DrawIcon(){
			gridImage.Children.Clear();
			if(_isIconDisabled){
				IconLibrary.DrawWpf(_icon,gridImage,UiTheme.GetBrushWpf(EnumUiColor.TextDisabled),false);
				return;
			}
			IconLibrary.DrawWpf(_icon,gridImage);
		}
	}

	///<summary>Just like Forms.ToolBarButtonStyle, except includes some extras.</summary>
	public enum ToolBarButtonStyle{
		///<summary>A standard button</summary>
		NormalButton,
		///<summary>A button with a dropdown menu list on the right.</summary>
		DropDownButton,
		///<summary>Toggles between pushed and not pushed when clicked.</summary>
		ToggleButton,
		//<summary></summary>
		//Separator,//no need for this
		//<summary>Not clickable. Just text where a button would normally be. Can also include an image.</summary>
		//Label,//not yet implemented
		//<summary>Editable textbox that fires page nav events. Includes a label after the textbox to show total pages.</summary>
		//PageNav,//not yet implemented
	}
}
