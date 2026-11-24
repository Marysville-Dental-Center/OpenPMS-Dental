using OpenPmsDental;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace WpfControls.UI{
	/*
Jordan is the only one allowed to edit this file.

How to use the CheckBox control:
-Height should be about 20
-MS has no CheckChanged event, so we don't either.  Use Click.
-If disabled, then Click event won't fire. (still researching this, and we may need to let it fire anyway in some cases)
-We may need to distinguish between mouse events on the box vs text.
-Still need to build hover effect.
-Still need to look into enabled and autocheck.
-Click event handlers usually look like this:
		private void butEdit_Click(object sender,EventArgs e) { etc.

*/
	///<summary></summary>
	public partial class RadioButton : UserControl,IContentLook{
		private EnumCheckAlign _checkAlign;
		private bool _checked=false;
		///<summary>The mouse is over the control (the hover outline of the new look; today's look has no hover).</summary>
		private bool _isHover;

		public RadioButton(){
			InitializeComponent();
			//Width=100;
			//Height=20;
			grid.MouseDown+=grid_MouseDown;
			grid.MouseMove+=grid_MouseMove;
			grid.MouseLeave+=grid_MouseLeave;
			IsEnabledChanged+=RadioButton_IsEnabledChanged;
			UiLook.SetFocusVisualStyle(this,onAccent:false);
			//UI-OVF (docs/port-notes/UI-OVF.md): a tab stop like the WinForms radio button; Space checks it and the arrow keys move to the previous or
			//next radio button of its group, checking it (OnKeyDown); the shared focus ring shows only with keyboard cues (section 3.6).
			Focusable=true;
			IsTabStop=true;
			//UI-11-final (docs/port-notes/UI-11-final.md): a group of radio buttons (the radio buttons of one parent) is one tab stop, as in WinForms:
			//the checked one, else the first in tab order; the arrow keys move inside the group (OnKeyDown).
			Loaded+=(sender,e) => UpdateGroupTabStops(Parent);
			IsVisibleChanged+=(sender,e) => UpdateGroupTabStops(Parent);
			SetColors();
		}

		public event EventHandler Click;

		#region Properties
		[Category("OD")]
		[Description("")]
		[DefaultValue(EnumCheckAlign.MiddleLeft)]
		public EnumCheckAlign CheckAlign{
			get{
				return _checkAlign;
			}
			set{
				_checkAlign=value;
				if(_checkAlign==EnumCheckAlign.TopLeft){
					grid.ColumnDefinitions.Clear();
					ColumnDefinition columnDefinition;
					columnDefinition=new ColumnDefinition();
					columnDefinition.Width=new GridLength(1,GridUnitType.Auto);//ellipse
					grid.ColumnDefinitions.Add(columnDefinition);
					columnDefinition=new ColumnDefinition();
					columnDefinition.Width=new GridLength(1,GridUnitType.Star);//text
					grid.ColumnDefinitions.Add(columnDefinition);
					System.Windows.Controls.Grid.SetColumn(border,0);
					System.Windows.Controls.Grid.SetColumn(textBlock,1);
					textBlock.TextAlignment=TextAlignment.Left;
					border.VerticalAlignment=VerticalAlignment.Top;
					textBlock.VerticalAlignment=VerticalAlignment.Top;
				}
				if(_checkAlign==EnumCheckAlign.MiddleLeft){
					grid.ColumnDefinitions.Clear();
					ColumnDefinition columnDefinition;
					columnDefinition=new ColumnDefinition();
					columnDefinition.Width=new GridLength(1,GridUnitType.Auto);//ellipse
					grid.ColumnDefinitions.Add(columnDefinition);
					columnDefinition=new ColumnDefinition();
					columnDefinition.Width=new GridLength(1,GridUnitType.Star);//text
					grid.ColumnDefinitions.Add(columnDefinition);
					System.Windows.Controls.Grid.SetColumn(border,0);
					System.Windows.Controls.Grid.SetColumn(textBlock,1);
					textBlock.TextAlignment=TextAlignment.Left;
					border.VerticalAlignment=VerticalAlignment.Center;
					textBlock.VerticalAlignment=VerticalAlignment.Center;
				}
				if(_checkAlign==EnumCheckAlign.MiddleRight){
					grid.ColumnDefinitions.Clear();
					ColumnDefinition columnDefinition;
					columnDefinition=new ColumnDefinition();
					columnDefinition.Width=new GridLength(1,GridUnitType.Star);//text
					grid.ColumnDefinitions.Add(columnDefinition);
					columnDefinition=new ColumnDefinition();
					columnDefinition.Width=new GridLength(1,GridUnitType.Auto);//ellipse
					grid.ColumnDefinitions.Add(columnDefinition);
					System.Windows.Controls.Grid.SetColumn(border,1);
					System.Windows.Controls.Grid.SetColumn(textBlock,0);
					textBlock.TextAlignment=TextAlignment.Right;
					border.VerticalAlignment=VerticalAlignment.Center;
					textBlock.VerticalAlignment=VerticalAlignment.Center;
				}
			}
		}

		[Category("OD")]
		[Description("")]
		[DefaultValue(false)]
		public bool Checked {
			//for convenience, named same as WinForms version.
			get{
				return _checked;
			}
			set{
				if(_checked==value){
					return; //no change
				}
				SetCheckedAndUncheckSiblings(value);
				//UI-11-final: the group's one tab stop follows the checked radio button.
				UpdateGroupTabStops(Parent);
			}
		}

		///<summary>Sets the checked state; checking unchecks the other radio buttons of the same parent.</summary>
		private void SetCheckedAndUncheckSiblings(bool value){
			_checked=value;
			if(!value){//unchecking. This can only happen programmatically
				ellipseSmallBlack.Visibility=Visibility.Collapsed;
				SetColors();
				return;
			}
			//everything below here is for checking it.
			ellipseSmallBlack.Visibility=Visibility.Visible;
			SetColors();
			//uncheck all other radio buttons that are siblings
			if(DesignerProperties.GetIsInDesignMode(this)){
				return;
			}
			//List<RadioButton> listRadioButtons=new List<RadioButton>();
			//Type typeParent=Parent.GetType();//UI.Panel, UI.GroupBox, or UserControl
			//Type typeBase=typeParent.BaseType;//ItemsControl
			//Type typeBase2=typeBase.BaseType;//FrameworkElement
			if(Parent is ItemsControl itemsControl){//UI.Panel or UI.GroupBox
				ItemCollection itemCollection=itemsControl.Items;
				for(int i=0;i<itemCollection.Count;i++){
					if(itemCollection[i]==this){
						continue;
					}
					if(itemCollection[i] is RadioButton radioButton){
						radioButton.Checked=false;
					}
				}
				return;
			}
			UIElementCollection uIElementCollection=null;
			if(Parent is System.Windows.Controls.Grid grid){
				//grids are always used as the content of FrmODBase, and this code will make it work for all grids
				uIElementCollection=grid.Children;
			}
			if(Parent is WrapPanel wrapPanel){//used in EForms layout
				uIElementCollection=wrapPanel.Children;
			}
			if(uIElementCollection==null){
				return;
			}
			for(int i=0;i<uIElementCollection.Count;i++){
				if(uIElementCollection[i]==this){
					continue;
				}
				if(uIElementCollection[i] is RadioButton radioButton){
					radioButton.Checked=false;
				}
			}
		}

		[Category("OD")]
		[DefaultValue(true)]
		public new bool IsEnabled{
			//This doesn't actually ever get hit. 
			//It's just here to move IsEnabled down into the OD category.
			get{
				return base.IsEnabled;
			}
			set{
				base.IsEnabled=value;
			}
		}

		[Category("OD")]
		[DefaultValue(int.MaxValue)]
		[Description("Use this instead of TabIndex.")]
		public int TabIndexOD{
			//For now, this is just to move it down into the OD category,
			//but later, there are plans to enhance it.
			//Because TabIndex is an Advanced Property, we had to give it a new name to keep it out of Advanced Property area.
			get{
				return TabIndex;
			}
			set{
				TabIndex=value;
				//InvalidateVisual();//if we want the new TabIndex value to show immediately. But there's a performance hit, so no.
			}
		}
		
		[Category("OD")]
		public string Text {
			get =>textBlock.Text;
			set=>textBlock.Text=value;
		}

		///<summary>This property is for convenience. It toggles the Visibility property between Visible and Collapsed.</summary>
		[Browsable(false)]
		public bool Visible{
			get{
				if(Visibility==Visibility.Visible){
					return true;
				}
				return false;//Hidden or Collapsed
			}
			set{
				if(value){
					Visibility=Visibility.Visible;
					return;
				}
				Visibility=Visibility.Collapsed;
			}
		}
		#endregion Properties

		private void grid_MouseDown(object sender,MouseButtonEventArgs e) {
			Checked=true;//this also unchecks any other sibling radiobuttons
			Click?.Invoke(this,new EventArgs());
		}

		///<summary>Space checks the focused radio button; the arrow keys move to the previous (Up, Left) or next (Down, Right) enabled radio button
		///of the same parent, in its order there, and check it, as Windows does (UI-OVF).</summary>
		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			if(e.Handled || !IsEnabled || Keyboard.Modifiers!=ModifierKeys.None){
				return;
			}
			if(e.Key==Key.Space){
				e.Handled=true;
				Checked=true;
				Click?.Invoke(this,new EventArgs());
				return;
			}
			int step=0;
			if(e.Key==Key.Up || e.Key==Key.Left){
				step=-1;
			}
			if(e.Key==Key.Down || e.Key==Key.Right){
				step=1;
			}
			if(step==0){
				return;
			}
			List<RadioButton> listRadioButtons=GetGroup();
			int index=listRadioButtons.IndexOf(this);
			if(index<0 || listRadioButtons.Count<2){
				return;
			}
			e.Handled=true;
			RadioButton radioButton=listRadioButtons[(index+step+listRadioButtons.Count)%listRadioButtons.Count];
			radioButton.Focus();
			radioButton.Checked=true;
			radioButton.Click?.Invoke(radioButton,new EventArgs());
		}

		///<summary>The enabled, visible radio buttons of this one's parent (the group Checked unchecks), in their order there.</summary>
		private List<RadioButton> GetGroup(){
			return GetRadioButtons(Parent).Where(x => x.IsEnabled && x.Visibility==Visibility.Visible).ToList();
		}

		///<summary>Every radio button of the parent (a UI.Panel, UI.GroupBox or other ItemsControl, a Grid or another panel), in its order there.</summary>
		private static List<RadioButton> GetRadioButtons(DependencyObject dependencyObjectParent){
			List<RadioButton> listRadioButtons=new List<RadioButton>();
			System.Collections.IEnumerable enumerable=null;
			if(dependencyObjectParent is ItemsControl itemsControl){
				enumerable=itemsControl.Items;
			}
			else if(dependencyObjectParent is System.Windows.Controls.Panel panel){
				enumerable=panel.Children;
			}
			if(enumerable==null){
				return listRadioButtons;
			}
			foreach(object item in enumerable){
				if(item is RadioButton radioButton){
					listRadioButtons.Add(radioButton);
				}
			}
			return listRadioButtons;
		}

		///<summary>UI-11-final: makes the radio buttons of the parent one tab stop, as WinForms does: the checked one when it is enabled and visible,
		///else the first enabled, visible one in tab order (TabIndex, then the parent's order); the others are no tab stops. A parent with no such
		///radio button leaves every one a tab stop. UiTabOrder orders the radio buttons whether or not they are the stop, and calls this again once
		///the indexes are set.</summary>
		public static void UpdateGroupTabStops(DependencyObject dependencyObjectParent){
			if(dependencyObjectParent==null){
				return;
			}
			List<RadioButton> listRadioButtons=GetRadioButtons(dependencyObjectParent);
			List<RadioButton> listCandidates=listRadioButtons.Where(x => x.IsEnabled && x.Visibility==Visibility.Visible).ToList();
			RadioButton radioButtonStop=listCandidates.FirstOrDefault(x => x.Checked);
			if(radioButtonStop==null){
				radioButtonStop=listCandidates.OrderBy(x => KeyboardNavigation.GetTabIndex(x)).ThenBy(x => listCandidates.IndexOf(x)).FirstOrDefault();
			}
			for(int i=0;i<listRadioButtons.Count;i++){
				listRadioButtons[i].IsTabStop=radioButtonStop==null || listRadioButtons[i]==radioButtonStop;
			}
		}

		private void RadioButton_IsEnabledChanged(object sender,DependencyPropertyChangedEventArgs e) {
			//This is nice because it gets hit when changing the property in the designer.
			SetColors();
			UpdateGroupTabStops(Parent);
		}

		private void grid_MouseMove(object sender,MouseEventArgs e) {
			if(_isHover){
				return;
			}
			_isHover=true;
			SetColors();
		}

		private void grid_MouseLeave(object sender,MouseEventArgs e) {
			_isHover=false;
			SetColors();
		}

		void IContentLook.ApplyLook(){
			SetColors();
		}

		///<summary>Section 2.1: drawn like the Windows 11 radio button in the tokens, so it sits beside the stock WinForms ones: a 12 DIP circle,
		///SurfaceRaised with a 1 px StrokeStrong outline (StrokeStrongHover on hover, TextDisabled when disabled); checked, an Accent circle with a
		///white centre dot (TextDisabled when disabled). Text in Text or TextDisabled.</summary>
		private void SetColorsNew(){
			ellipseOutline.Visibility=Visibility.Collapsed;
			ellipseSmallBlack.Opacity=0;
			ellipseNew.Visibility=Visibility.Visible;
			if(_checked){
				SolidColorBrush brushFill=UiLook.Brush(IsEnabled ? CodeBase.EnumUiColor.Accent : CodeBase.EnumUiColor.TextDisabled);
				ellipseNew.Fill=brushFill;
				ellipseNew.Stroke=brushFill;
				ellipseDotNew.Fill=UiLook.Brush(CodeBase.EnumUiColor.TextOnAccent);
				ellipseDotNew.Visibility=Visibility.Visible;
			}
			else{
				CodeBase.EnumUiColor uiColorBorder=CodeBase.EnumUiColor.StrokeStrong;
				if(!IsEnabled){
					uiColorBorder=CodeBase.EnumUiColor.TextDisabled;
				}
				else if(_isHover){
					uiColorBorder=CodeBase.EnumUiColor.StrokeStrongHover;
				}
				ellipseNew.Fill=UiLook.Brush(CodeBase.EnumUiColor.SurfaceRaised);
				ellipseNew.Stroke=UiLook.Brush(uiColorBorder);
				ellipseDotNew.Visibility=Visibility.Collapsed;
			}
			textBlock.Foreground=UiLook.Brush(IsEnabled ? CodeBase.EnumUiColor.Text : CodeBase.EnumUiColor.TextDisabled);
		}

		private void SetColors(){
			if(!ContentLook.GetIsPinned(this)){
				SetColorsNew();
				return;
			}
			//Today's look, unchanged, for office content (the eForm fill surface keeps its look, section 1).
			ellipseNew.Visibility=Visibility.Collapsed;
			ellipseDotNew.Visibility=Visibility.Collapsed;
			ellipseOutline.Visibility=Visibility.Visible;
			ellipseSmallBlack.Opacity=1;
			if(IsEnabled){
				ellipseOutline.Stroke=new SolidColorBrush(Color.FromRgb(111,111,111));//6F6F6F
				textBlock.Foreground=Brushes.Black;//not hit very often. Usually black because of default.
			}
			else{
				ellipseOutline.Stroke=new SolidColorBrush(OpenPmsDental.ColorOD.Gray_Wpf(170));
				textBlock.Foreground=new SolidColorBrush(OpenPmsDental.ColorOD.Gray_Wpf(170));
			}
		}
	}
}
