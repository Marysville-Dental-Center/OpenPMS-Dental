// UI-6's WPF part (docs/specs/ui-refresh-plan.md, section 3.6; docs/port-notes/UI-6.md): WpfControls.UI.MenuItem and ContextMenu take the
// new look's templates from Themes\Generic.xaml, and the WPF toolbar's buttons show the token colours of each state. Built and laid out without a
// window (WPF's layout and templates need none); no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UiRefreshTests.Toolbars {
	[TestClass]
	public class WpfMenuToolBarTests {
		[TestMethod]
		public void WpfMenuItemAndContextMenu_UseTheGenericTemplates() {
			StaRunnerHelpers.RunSta(() => {
				WpfControls.UI.ContextMenu contextMenu=new WpfControls.UI.ContextMenu();
				WpfControls.UI.MenuItem menuItemChecked=new WpfControls.UI.MenuItem("Show inactive",(sender,e) => { }) { IsChecked=true };
				WpfControls.UI.MenuItem menuItemDisabled=new WpfControls.UI.MenuItem("Export",(sender,e) => { }) { IsEnabled=false };
				WpfControls.UI.MenuItem menuItemSub=new WpfControls.UI.MenuItem("More");
				menuItemSub.Add(new WpfControls.UI.MenuItem("Sub",(sender,e) => { }));
				contextMenu.Add(menuItemChecked);
				contextMenu.AddSeparator();
				contextMenu.Add(menuItemDisabled);
				contextMenu.Add(menuItemSub);
				contextMenu.Measure(new Size(double.PositiveInfinity,double.PositiveInfinity));
				contextMenu.Arrange(new Rect(contextMenu.DesiredSize));
				contextMenu.UpdateLayout();
				Border borderMenu=(Border)VisualTreeHelper.GetChild(contextMenu,0);
				Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.SurfaceRaised),((SolidColorBrush)borderMenu.Background).Color,"The context menu is SurfaceRaised.");
				Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.Stroke),((SolidColorBrush)borderMenu.BorderBrush).Color,"with a Stroke border.");
				Path pathCheck=(Path)menuItemChecked.Template.FindName("check",menuItemChecked);
				Assert.IsNotNull(pathCheck,"The item has the new template (Generic.xaml).");
				Assert.AreEqual(Visibility.Visible,pathCheck.Visibility,"A checked item shows the Fluent check mark.");
				Assert.IsFalse(pathCheck.Data.IsEmpty(),"The check mark has its geometry.");
				Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.TextDisabled),((SolidColorBrush)menuItemDisabled.Foreground).Color,"A disabled item is TextDisabled.");
				Path pathArrow=(Path)menuItemSub.Template.FindName("arrow",menuItemSub);
				Assert.AreEqual(Visibility.Visible,pathArrow.Visibility,"A sub-menu shows the Fluent chevron.");
				Path pathArrowItem=(Path)menuItemChecked.Template.FindName("arrow",menuItemChecked);
				Assert.AreEqual(Visibility.Collapsed,pathArrowItem.Visibility,"An item without a sub-menu has no arrow.");
			});
		}

		[TestMethod]
		public void WpfToolBarButton_StatesUseTheTokens() {
			StaRunnerHelpers.RunSta(() => {
				EventHandler eventHandler=(sender,e) => { };
				WpfControls.UI.ToolBarButton toolBarButton=new WpfControls.UI.ToolBarButton("Edit",eventHandler);
				Border borderOverlay=(Border)toolBarButton.FindName("borderOverlay");
				Assert.AreEqual(Colors.Transparent,((SolidColorBrush)borderOverlay.Background).Color,"At rest the button shows no shape.");
				Assert.AreEqual(new CornerRadius(UiTheme.CORNER_CONTROL),borderOverlay.CornerRadius);
				SetState(toolBarButton,"_isHover");
				Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.SubtleHover),((SolidColorBrush)borderOverlay.Background).Color,"Hover is SubtleHover.");
				SetState(toolBarButton,"_isPressed");
				Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.SubtlePressed),((SolidColorBrush)borderOverlay.Background).Color,"Pressed is SubtlePressed.");
				WpfControls.UI.ToolBarButton toolBarButtonToggle=new WpfControls.UI.ToolBarButton("Crop",eventHandler,WpfControls.UI.EnumIcons.None,
					WpfControls.UI.ToolBarButtonStyle.ToggleButton);
				toolBarButtonToggle.IsTogglePushed=true;
				SetState(toolBarButtonToggle,null);
				Border borderToggle=(Border)toolBarButtonToggle.FindName("borderOverlay");
				Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.AccentSubtle),((SolidColorBrush)borderToggle.Background).Color,"Toggled is AccentSubtle");
				Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.Accent),((SolidColorBrush)borderToggle.BorderBrush).Color,"with an Accent border.");
				WpfControls.UI.ToolBarButton toolBarButtonDrop=new WpfControls.UI.ToolBarButton("Print",eventHandler,WpfControls.UI.EnumIcons.Print,
					WpfControls.UI.ToolBarButtonStyle.DropDownButton);
				Path pathChevron=(Path)toolBarButtonDrop.FindName("pathChevron");
				Assert.AreEqual(Visibility.Visible,pathChevron.Visibility);
				Assert.IsFalse(pathChevron.Data.IsEmpty(),"The drop-down arrow is the Fluent chevron.");
				toolBarButtonDrop.IsEnabled=false;
				TextBlock textBlock=(TextBlock)toolBarButtonDrop.FindName("textBlock");
				Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.TextDisabled),((SolidColorBrush)textBlock.Foreground).Color,"Disabled is TextDisabled.");
				Grid gridImage=(Grid)toolBarButtonDrop.FindName("gridImage");
				Path pathIcon=(Path)gridImage.Children[0];
				Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.TextDisabled),((SolidColorBrush)pathIcon.Fill).Color,"and so is its icon.");
			});
		}

		private static void SetState(WpfControls.UI.ToolBarButton toolBarButton,string field) {
			if(field!=null) {
				typeof(WpfControls.UI.ToolBarButton).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(toolBarButton,true);
			}
			typeof(WpfControls.UI.ToolBarButton).GetMethod("SetColors",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(toolBarButton,null);
		}
	}
}
