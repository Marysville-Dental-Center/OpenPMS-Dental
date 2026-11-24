using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeBase;
using OpenPmsDentalBusiness;

namespace WpfControls.UI{
	///<summary></summary>
	public partial class ToolBar : System.Windows.Controls.UserControl{
/*
Jordan is the only one allowed to edit this file.
Height should be 25.
Typically anchor it L,T,R at the top of a window in the designer, just under the menu.  
We add all buttons in code, not in the designer.
Boilerplate for adding buttons:

using WpfControls.UI;
//using System.Windows.Controls;//Don't do this

		private void LayoutToolBar() {
			toolBarMain.Clear();//not usually necessary
			toolBarMain.Add(Lans.g(this,"Add"),Add_Click,EnumIcons.Add);
			toolBarMain.AddSeparator();
			toolBarMain.Add(Lans.g(this,"Edit"),Edit_Click,toolTipText:Lans.g(this,"Edit Selected Account"));
			ContextMenu contextMenu = new ContextMenu();
			contextMenu.Add(new MenuItem("Item1",menuItem1_Click));
			contextMenu.Add(new MenuItem("Item2",menuItem2_Click));
			contextMenu.Add(new MenuItem("Item3",menuItem3_Click));
			toolBarMain.Add(Lans.g(this,"Pick File"),toolBarButtonStyle:ToolBarButtonStyle.DropDownButton,contextMenuDropDown:contextMenu);
		}

Click event handlers look like this:
		private void Edit_Click(object sender,EventArgs e) { etc.

Notice that there is only one way to add an icon, unlike with buttons.
If you need a missing icon, Jordan will need to add it using the IconGenerator project.
WPF doesn't use anything like the WF ImageList, so all of those will go away during the conversion.
For button clicks, the old paradigm was one event for the entire toolbar.
The new way of doing it is a separate event for each button.

*/
		public ToolBar(){
			InitializeComponent();
			if(!DesignerProperties.GetIsInDesignMode(this)){
				stackPanel.Children.Clear();
				//UI-6 (docs/specs/ui-refresh-plan.md section 3.6, Toolbars): flat SurfaceSubtle with a 1 px Gridline line at the bottom.
				border.Background=UiTheme.GetBrushWpf(EnumUiColor.SurfaceSubtle);
				border.BorderBrush=UiTheme.GetBrushWpf(EnumUiColor.Gridline);
				//UI-OVF (docs/port-notes/UI-OVF.md): the » overflow button at the right end, over the panel, shown while buttons do not fit.
				_toolBarButtonOverflow=new ToolBarButton("",ToolBarButtonOverflow_Click,toolTipText:Lans.g("ToolBar","More buttons"));
				_toolBarButtonOverflow.SetGlyphOverflow();
				_toolBarButtonOverflow.Width=UiToolStripRenderer.WIDTH_OVERFLOW_BUTTON96;
				_toolBarButtonOverflow.HorizontalAlignment=HorizontalAlignment.Right;
				_toolBarButtonOverflow.Visibility=Visibility.Collapsed;
				gridMain.Children.Add(_toolBarButtonOverflow);
				stackPanel.OverflowChanged+=StackPanel_OverflowChanged;
				//One tab stop for the whole toolbar, as the WinForms toolbar; the arrow keys move between its buttons (OnPreviewKeyDown).
				KeyboardNavigation.SetTabNavigation(this,KeyboardNavigationMode.Once);
			}
		}

		///<summary>The » overflow button (UI-OVF). Null in the designer.</summary>
		private ToolBarButton _toolBarButtonOverflow;

		#region Properties
		///<summary>True when some buttons do not fit and the » overflow button shows at the right end (UI-OVF). From the last layout.</summary>
		public bool IsOverflowing=>stackPanel.IndexOverflowFirst>=0;

		///<summary>The » overflow button (UI-OVF), for tests and pictures.</summary>
		public ToolBarButton ToolBarButtonOverflow=>_toolBarButtonOverflow;

		#endregion Properties

		#region Methods

		public void Add(ToolBarButton toolBarButton){
			stackPanel.Children.Add(toolBarButton);
			SetTabStops();
		}

		public void Add(string text,EventHandler eventHandlerClick=null,EnumIcons icon=EnumIcons.None,ToolBarButtonStyle toolBarButtonStyle=ToolBarButtonStyle.NormalButton,
			string toolTipText=null,ContextMenu contextMenuDropDown=null,object tag=null)
		{
			ToolBarButton toolBarButton = new ToolBarButton(text,eventHandlerClick,icon,toolBarButtonStyle,toolTipText,contextMenuDropDown,tag);
			stackPanel.Children.Add(toolBarButton);
			SetTabStops();
		}

		//public void Add(string text,string bitmapFileName,EventHandler eventHandlerClick){
		//	ToolBarButton toolBarButton = new ToolBarButton();
		//	toolBarButton.Text=text;
		//	toolBarButton.BitmapFileName=bitmapFileName;
		//	toolBarButton.Click+=eventHandlerClick;
		//	stackPanel.Children.Add(toolBarButton);
		//}

		///<summary>A separator: 7 x 24 as before, drawn as a 1 px Stroke line in its middle, 4 px in from the top and the bottom (UI-6).</summary>
		public void AddSeparator(){
			System.Windows.Controls.Grid gridSeparator=new System.Windows.Controls.Grid();
			gridSeparator.Width=7;
			gridSeparator.Height=24;
			Rectangle rectangle=new Rectangle();
			rectangle.Width=1;
			rectangle.Margin=new Thickness(0,UiTheme.INSET_TOOLBAR_SEPARATOR,0,UiTheme.INSET_TOOLBAR_SEPARATOR);
			rectangle.HorizontalAlignment=HorizontalAlignment.Center;
			rectangle.Fill=UiTheme.GetBrushWpf(EnumUiColor.Stroke);
			gridSeparator.Children.Add(rectangle);
			stackPanel.Children.Add(gridSeparator);
		}
		public void Clear(){
			stackPanel.Children.Clear();
		}

		///<summary>The toolbar's items in the overflow button's drop-down instead of on the bar, in order, separators included (UI-OVF). Empty when
		///every item fits. From the last layout.</summary>
		public List<UIElement> GetItemsOverflowed(){
			List<UIElement> listItems=new List<UIElement>();
			for(int i=0;i<stackPanel.Children.Count;i++){
				if(stackPanel.IsOverflowedIndex(i)){
					listItems.Add(stackPanel.Children[i]);
				}
			}
			return listItems;
		}

		///<summary>The overflow button's drop-down built from the items that do not fit (UI-OVF), not shown. Each button is an item with the same
		///text (an icon-only button's tooltip), icon, enabled state and tooltip; a toggle button is checked while pushed; a click is a click on the
		///button. A drop-down button's item has a sub-menu: the button's main part, a separator, then the items of its own menu (its Opened handlers
		///run first, so a menu filled on opening is filled), each of which raises the original item's Click. A label is a disabled item.</summary>
		public ContextMenu GetOverflowMenu(){
			ContextMenu contextMenu=new ContextMenu();
			List<UIElement> listItems=GetItemsOverflowed();
			for(int i=0;i<listItems.Count;i++){
				if(!(listItems[i] is ToolBarButton toolBarButton)){
					if(contextMenu.Items.Count>0 && !(contextMenu.Items[contextMenu.Items.Count-1] is System.Windows.Controls.Separator)){
						contextMenu.AddSeparator();
					}
					continue;
				}
				contextMenu.Add(NewOverflowItem(toolBarButton));
			}
			if(contextMenu.Items.Count>0 && contextMenu.Items[contextMenu.Items.Count-1] is System.Windows.Controls.Separator){
				contextMenu.Items.RemoveAt(contextMenu.Items.Count-1);
			}
			return contextMenu;
		}

		///<summary>Opens the overflow button's drop-down below it, right-aligned to it, and shows the button pressed while it is open (UI-OVF).</summary>
		public void ShowOverflowMenu(){
			if(!IsOverflowing){
				return;
			}
			ContextMenu contextMenu=GetOverflowMenu();
			if(contextMenu.Items.Count==0){
				return;
			}
			contextMenu.PlacementTarget=_toolBarButtonOverflow;
			contextMenu.Placement=System.Windows.Controls.Primitives.PlacementMode.Custom;
			contextMenu.CustomPopupPlacementCallback=(sizePopup,sizeTarget,pointOffset) => new System.Windows.Controls.Primitives.CustomPopupPlacement[] {
				new System.Windows.Controls.Primitives.CustomPopupPlacement(new Point(sizeTarget.Width-sizePopup.Width,sizeTarget.Height),
					System.Windows.Controls.Primitives.PopupPrimaryAxis.Vertical)
			};
			contextMenu.Closed+=(sender,e) => _toolBarButtonOverflow.SetMenuOpen(false);
			_toolBarButtonOverflow.SetMenuOpen(true);
			contextMenu.IsOpen=true;
		}

		public void SetEnabled(string tag,bool boolVal){
			for(int i=0;i<stackPanel.Children.Count;i++){
				if(stackPanel.Children[i].GetType()!=typeof(ToolBarButton)){
					continue;
				}
				//We tried using enums for tags, but that fails because of equality comparison technical details.
				if(tag!=((ToolBarButton)stackPanel.Children[i]).Tag?.ToString()){
					continue;
				}
				stackPanel.Children[i].IsEnabled=boolVal;
				return;
			}
		}

		public void SetEnabledAll(bool boolVal){
			for(int i=0;i<stackPanel.Children.Count;i++){
				if(stackPanel.Children[i].GetType()!=typeof(ToolBarButton)){
					continue;
				}
				stackPanel.Children[i].IsEnabled=boolVal;
			}
		}
		#endregion Methods

		#region Methods - private
		private void ToolBarButtonOverflow_Click(object sender,EventArgs e){
			ShowOverflowMenu();
		}

		private void StackPanel_OverflowChanged(object sender,EventArgs e){
			_toolBarButtonOverflow.Visibility=IsOverflowing ? Visibility.Visible : Visibility.Collapsed;
			SetTabStops();
			//A button that went into the drop-down while it had the focus hands it to the overflow button.
			if(IsOverflowing && Keyboard.FocusedElement is ToolBarButton toolBarButtonFocused && toolBarButtonFocused!=_toolBarButtonOverflow
				&& stackPanel.Children.Contains(toolBarButtonFocused) && stackPanel.IsOverflowedIndex(stackPanel.Children.IndexOf(toolBarButtonFocused)))
			{
				_toolBarButtonOverflow.Focus();
			}
		}

		///<summary>The toolbar is one tab stop (TabNavigation Once): Tab lands on its first button, or the one the keyboard was on last (UI-OVF). A
		///label ("Scan:") and a button in the overflow button's drop-down are no tab stops, so Tab never lands on one; the arrow keys skip them too
		///(GetKeyboardButtons). A disabled or collapsed button is skipped by WPF itself.</summary>
		private void SetTabStops(){
			if(_toolBarButtonOverflow==null){
				return;//the designer
			}
			for(int i=0;i<stackPanel.Children.Count;i++){
				if(stackPanel.Children[i] is ToolBarButton toolBarButton){
					toolBarButton.IsTabStop=toolBarButton.HasClick && !stackPanel.IsOverflowedIndex(i);
				}
			}
		}

		///<summary>The drop-down's item for a button (UI-OVF, GetOverflowMenu).</summary>
		private MenuItem NewOverflowItem(ToolBarButton toolBarButton){
			string text=toolBarButton.Text;
			if(string.IsNullOrEmpty(text)){
				text=toolBarButton.ToolTipText??"";//an icon-only button: its tooltip names it
			}
			text=text.Replace("_","__");//the button's text has no access keys
			MenuItem menuItem=new MenuItem(text);
			menuItem.Tag=toolBarButton;
			menuItem.IsEnabled=toolBarButton.IsEnabled && toolBarButton.HasClick;
			menuItem.IsChecked=toolBarButton.ToolBarButtonStyle==ToolBarButtonStyle.ToggleButton && toolBarButton.IsTogglePushed;
			menuItem.Icon=toolBarButton.GetMenuIcon(16);
			if(!string.IsNullOrEmpty(toolBarButton.ToolTipText)){
				menuItem.ToolTip=toolBarButton.ToolTipText;
			}
			if(toolBarButton.ToolBarButtonStyle==ToolBarButtonStyle.DropDownButton && toolBarButton.ContextMenuDropDown!=null){
				System.Windows.Controls.ContextMenu contextMenuDropDown=toolBarButton.ContextMenuDropDown;
				contextMenuDropDown.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.ContextMenu.OpenedEvent,contextMenuDropDown));//a menu filled on opening
				MenuItem menuItemMain=new MenuItem(text);
				menuItemMain.Tag=toolBarButton;
				menuItemMain.IsEnabled=toolBarButton.HasClick;
				menuItemMain.Click+=(sender,e) => toolBarButton.PerformClick();
				menuItem.Items.Add(menuItemMain);
				menuItem.Items.Add(new System.Windows.Controls.Separator());
				AddProxyItems(menuItem.Items,contextMenuDropDown.Items);
				return menuItem;
			}
			menuItem.Click+=(sender,e) => toolBarButton.PerformClick();
			return menuItem;
		}

		///<summary>Copies of a menu's items for a sub-menu of the overflow drop-down (an item cannot have two parents): the same header, enabled and
		///checked state and gesture text; a click raises the original item's Click (a checkable one flips first, as a click on it does).</summary>
		private static void AddProxyItems(System.Windows.Controls.ItemCollection itemCollectionTarget,System.Windows.Controls.ItemCollection itemCollectionSource){
			for(int i=0;i<itemCollectionSource.Count;i++){
				if(itemCollectionSource[i] is System.Windows.Controls.Separator){
					itemCollectionTarget.Add(new System.Windows.Controls.Separator());
					continue;
				}
				if(!(itemCollectionSource[i] is System.Windows.Controls.MenuItem menuItemSource) || menuItemSource.Visibility!=Visibility.Visible){
					continue;
				}
				MenuItem menuItemProxy=new MenuItem();
				menuItemProxy.Header=menuItemSource.Header is string ? menuItemSource.Header : menuItemSource.Header?.ToString();
				menuItemProxy.IsEnabled=menuItemSource.IsEnabled;
				menuItemProxy.IsChecked=menuItemSource.IsChecked;
				menuItemProxy.InputGestureText=menuItemSource.InputGestureText;
				menuItemProxy.Tag=menuItemSource;
				if(menuItemSource.HasItems){
					AddProxyItems(menuItemProxy.Items,menuItemSource.Items);
				}
				else{
					menuItemProxy.Click+=(sender,e) => {
						if(menuItemSource.IsCheckable){
							menuItemSource.IsChecked=!menuItemSource.IsChecked;
						}
						menuItemSource.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent,menuItemSource));
					};
				}
				itemCollectionTarget.Add(menuItemProxy);
			}
		}

		///<summary>The buttons the arrow keys stop on (UI-OVF), in order: the enabled buttons with a click on the bar, then the overflow button.</summary>
		private List<ToolBarButton> GetKeyboardButtons(){
			List<ToolBarButton> listButtons=new List<ToolBarButton>();
			for(int i=0;i<stackPanel.Children.Count;i++){
				if(stackPanel.IsOverflowedIndex(i)){
					break;
				}
				if(stackPanel.Children[i] is ToolBarButton toolBarButton && toolBarButton.IsEnabled && toolBarButton.HasClick
					&& toolBarButton.Visibility==Visibility.Visible)
				{
					listButtons.Add(toolBarButton);
				}
			}
			if(IsOverflowing){
				listButtons.Add(_toolBarButtonOverflow);
			}
			return listButtons;
		}

		///<summary>Left, Right, Home and End move the keyboard between the toolbar's buttons (UI-OVF), as on the WinForms toolbar; Enter, Space and
		///Down on the overflow button open its drop-down. The buttons handle their own Enter, Space and Down.</summary>
		protected override void OnPreviewKeyDown(KeyEventArgs e){
			base.OnPreviewKeyDown(e);
			if(e.Handled || Keyboard.Modifiers!=ModifierKeys.None || _toolBarButtonOverflow==null){
				return;
			}
			List<ToolBarButton> listButtons=GetKeyboardButtons();
			int index=listButtons.IndexOf(Keyboard.FocusedElement as ToolBarButton);
			if(index<0){
				return;
			}
			switch(e.Key){
				case Key.Left:
					listButtons[index<=0 ? listButtons.Count-1 : index-1].Focus();
					break;
				case Key.Right:
					listButtons[(index+1)%listButtons.Count].Focus();
					break;
				case Key.Home:
					listButtons[0].Focus();
					break;
				case Key.End:
					listButtons[listButtons.Count-1].Focus();
					break;
				case Key.Enter:
				case Key.Space:
				case Key.Down:
					if(listButtons[index]!=_toolBarButtonOverflow){
						return;
					}
					ShowOverflowMenu();
					break;
				default:
					return;
			}
			e.Handled=true;
		}
		#endregion Methods - private


	}

	///<summary>The toolbar's items panel (UI-OVF, docs/port-notes/UI-OVF.md), in place of the StackPanel it was: the items left to right at their
	///own widths and the toolbar's height, as the StackPanel laid them out. When they need more than the width it has, the items from the first one
	///that does not fit left of the overflow button (WIDTH_OVERFLOW_BUTTON96 at the right end) are not shown (arranged to nothing): the toolbar's
	///overflow button holds them. A separator just before the cut goes with them; nothing overflows when only separators would.</summary>
	public class ToolBarItemsPanel:System.Windows.Controls.Panel{
		///<summary>The index of the first item in the overflow button's drop-down; -1 when every item fits.</summary>
		public int IndexOverflowFirst{ get; private set; }=-1;

		///<summary>Raised when IndexOverflowFirst changes (the toolbar shows or hides its overflow button).</summary>
		public event EventHandler OverflowChanged;

		///<summary>True when the item at the index is in the overflow button's drop-down.</summary>
		public bool IsOverflowedIndex(int index){
			return IndexOverflowFirst>=0 && index>=IndexOverflowFirst;
		}

		protected override Size MeasureOverride(Size sizeAvailable){
			double widthTotal=0;
			double heightMax=0;
			for(int i=0;i<InternalChildren.Count;i++){
				UIElement uIElement=InternalChildren[i];
				uIElement.Measure(new Size(double.PositiveInfinity,sizeAvailable.Height));
				widthTotal+=uIElement.DesiredSize.Width;
				heightMax=Math.Max(heightMax,uIElement.DesiredSize.Height);
			}
			int indexOverflowFirst=-1;
			bool isButtonPastEdge=false;//a button, not only a separator, runs past the right edge
			if(!double.IsInfinity(sizeAvailable.Width) && widthTotal>sizeAvailable.Width+0.5){
				double xRight=0;
				for(int i=0;i<InternalChildren.Count;i++){
					xRight+=InternalChildren[i].DesiredSize.Width;
					if(InternalChildren[i] is ToolBarButton && xRight>sizeAvailable.Width+0.5){
						isButtonPastEdge=true;
						break;
					}
				}
			}
			if(isButtonPastEdge){
				double xLimit=sizeAvailable.Width-UiToolStripRenderer.WIDTH_OVERFLOW_BUTTON96;
				double x=0;
				int index=0;
				while(index<InternalChildren.Count && x+InternalChildren[index].DesiredSize.Width<=xLimit+0.5){
					x+=InternalChildren[index].DesiredSize.Width;
					index++;
				}
				while(index>0 && !(InternalChildren[index-1] is ToolBarButton)){
					index--;
				}
				indexOverflowFirst=index;
			}
			if(indexOverflowFirst!=IndexOverflowFirst){
				IndexOverflowFirst=indexOverflowFirst;
				OverflowChanged?.Invoke(this,EventArgs.Empty);
			}
			if(double.IsInfinity(sizeAvailable.Width)){
				return new Size(widthTotal,heightMax);
			}
			return new Size(Math.Min(widthTotal,sizeAvailable.Width),heightMax);
		}

		protected override Size ArrangeOverride(Size sizeFinal){
			double x=0;
			for(int i=0;i<InternalChildren.Count;i++){
				UIElement uIElement=InternalChildren[i];
				if(IsOverflowedIndex(i)){
					uIElement.Arrange(new Rect(0,0,0,0));//not shown: clipped to nothing and not hit
					continue;
				}
				double width=uIElement.DesiredSize.Width;
				uIElement.Arrange(new Rect(x,0,width,sizeFinal.Height));
				x+=width;
			}
			return sizeFinal;
		}
	}

}
