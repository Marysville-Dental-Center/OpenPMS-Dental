using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CodeBase;

namespace WpfControls.UI {
	//instructions for how to use are in Menu file.
	public class MenuItem : System.Windows.Controls.MenuItem{
		#region Fields
		private string _shortcut="";
		private RoutedEventHandler _click;
		#endregion Fields

		#region Constructors
		///<summary>The menu items take their template from Themes\Generic.xaml (UI-6, docs/specs/ui-refresh-plan.md section 3.6): the new look.</summary>
		static MenuItem(){
			DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuItem),new FrameworkPropertyMetadata(typeof(MenuItem)));
		}

		public MenuItem(){
			VerticalAlignment=VerticalAlignment.Stretch;
		}

		public MenuItem(string text){
			VerticalAlignment=VerticalAlignment.Stretch;
			Header=text;
		}

		public MenuItem(string text,RoutedEventHandler click,string shortcut="",object tag=null){
			VerticalAlignment=VerticalAlignment.Stretch;
			Header=text;
			Shortcut=shortcut;
			Tag=tag;
			_click=click;
			Click+=(sender,routedEventArgs)=>_click?.Invoke(sender,new RoutedEventArgs());
		}
		#endregion Constructors

		#region Properties - Static, for Generic.xaml
		//The token brushes and glyphs the menu templates of Themes\Generic.xaml use through x:Static (UI-6): UiTheme's frozen WPF brushes (their
		//high-contrast partners while Windows runs a high-contrast theme when the template is loaded) and UI-7a's Fluent glyphs.
		public static SolidColorBrush BrushText { get { return UiTheme.GetBrushWpf(EnumUiColor.Text); } }
		public static SolidColorBrush BrushTextDisabled { get { return UiTheme.GetBrushWpf(EnumUiColor.TextDisabled); } }
		public static SolidColorBrush BrushSurface { get { return UiTheme.GetBrushWpf(EnumUiColor.Surface); } }
		public static SolidColorBrush BrushSurfaceRaised { get { return UiTheme.GetBrushWpf(EnumUiColor.SurfaceRaised); } }
		public static SolidColorBrush BrushSubtleHover { get { return UiTheme.GetBrushWpf(EnumUiColor.SubtleHover); } }
		public static SolidColorBrush BrushSubtlePressed { get { return UiTheme.GetBrushWpf(EnumUiColor.SubtlePressed); } }
		public static SolidColorBrush BrushStroke { get { return UiTheme.GetBrushWpf(EnumUiColor.Stroke); } }
		public static SolidColorBrush BrushGridline { get { return UiTheme.GetBrushWpf(EnumUiColor.Gridline); } }
		public static SolidColorBrush BrushAccent { get { return UiTheme.GetBrushWpf(EnumUiColor.Accent); } }
		public static SolidColorBrush BrushAccentSubtle { get { return UiTheme.GetBrushWpf(EnumUiColor.AccentSubtle); } }
		///<summary>The check mark of a checked item: the Fluent Checkmark in a 16 DIP slot (the menus' icon column).</summary>
		public static Geometry GeometryCheckmark { get { return UiIcons.GetGeometryWpf(UiGlyph.Checkmark,UiTheme.MENU_ICON_COLUMN); } }
		///<summary>The sub-menu arrow: the Fluent ChevronRight in a 12 DIP slot.</summary>
		public static Geometry GeometryChevronRight { get { return UiIcons.GetGeometryWpf(UiGlyph.ChevronRight,UiTheme.SIZE_CHEVRON); } }
		#endregion Properties - Static, for Generic.xaml

		#region Properties
		///<summary>Use a single capital letter. Example "W". This will show in the UI as "Ctrl+W" and that key combo will trigger the click event. No support for other modifier keys.</summary>
		public string Shortcut {
			get {
				return _shortcut;
			}
			set {
				_shortcut=value;
				if(string.IsNullOrEmpty(_shortcut)){
					InputGestureText="";
					return;
				}
				InputGestureText="Ctrl+"+_shortcut;//this does nothing except display the text.
			}
		}

		public string Text {
			//There are plans to enhance this so that the header is composed of image-text-shortcut
			get {
				return Header.ToString();
			}
			set {
				Header=value;
			}
		}
		#endregion Properties

		#region Methods public
		public void Add(MenuItem menuItem){
			Items.Add(menuItem);
		}

		public void Add(string text,RoutedEventHandler click){
			MenuItem menuItem=new MenuItem(text,click);
			Items.Add(menuItem);
		}

		public void AddSeparator(){
			Items.Add(new System.Windows.Controls.Separator());
		}

		public void ClearClickEvent(){
			if(_click!=null) {
				Click-=_click;
				_click=null;
			}
		}

		public List<MenuItem> GetMenuItems(){
			List<MenuItem> listMenuItems=new List<MenuItem>();
			for(int i=0;i<Items.Count;i++){
				if(Items[i] is System.Windows.Controls.Separator){
					continue;
				}
				listMenuItems.Add((MenuItem)Items[i]);
			}
			return listMenuItems;
		}
		#endregion Methods public
	}
}
