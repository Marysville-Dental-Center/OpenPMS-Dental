// Builds the program's own toolbars and menu from what ToolBarSources read (package UI-6, docs/port-notes/UI-6.md), off screen: a real ToolBarOD
// with the module's ImageList (read from the module's own resources, as its designer does) and the module's buttons, a real WPF ToolBar for the
// Imaging module, and the main menu's top-level items. The widths come from the controls' own layout code (ToolBarOD.ComputeButtonsizes, the WPF
// layout, MenuStripOD.LayoutItems's formula), so the tests measure what the program would show. No window is shown, no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OpenPmsDental.UI;

namespace UiRefreshTests.Toolbars {
	///<summary>One item laid out: its caption and its left and right edges at 100%.</summary>
	public class LaidOutItem {
		public string Text;
		public int Left;
		public int Right;
		public bool IsSeparator;
		public bool IsConditional;
	}

	public static class ToolBarModel {
		///<summary>A toolbar's height in the program (the designers' 25 px).</summary>
		public const int HEIGHT_TOOLBAR=25;

		///<summary>The ImageList a designer gives the toolbar, read from the type's resources (its ImageStream), or null.</summary>
		public static ImageList GetImageList(ToolBarSource toolBarSource) {
			if(toolBarSource.TypeImageList==null) {
				return null;
			}
			Type type=typeof(ToolBarOD).Assembly.GetType(toolBarSource.TypeImageList,throwOnError:true);
			ComponentResourceManager componentResourceManager=new ComponentResourceManager(type);
			ImageListStreamer imageListStreamer=(ImageListStreamer)componentResourceManager.GetObject(toolBarSource.FieldImageList+".ImageStream");
			if(imageListStreamer==null) {
				throw new Exception(toolBarSource.TypeImageList+" has no "+toolBarSource.FieldImageList+".ImageStream resource.");
			}
			ImageList imageList=new ImageList();
			imageList.ImageStream=imageListStreamer;
			return imageList;
		}

		///<summary>A ToolBarOD of the width with the toolbar's buttons, in the font (null: the toolbar's own). The caller disposes it.</summary>
		public static ToolBarOD BuildToolBarOD(ToolBarSource toolBarSource,ImageList imageList,int width,Font font) {
			ToolBarOD toolBarOD=new ToolBarOD();
			if(font!=null) {
				toolBarOD.Font=font;
			}
			toolBarOD.ImageList=imageList;
			toolBarOD.Size=new Size(width,HEIGHT_TOOLBAR);
			foreach(ToolBarButtonSource toolBarButtonSource in toolBarSource.ListButtons) {
				toolBarOD.Buttons.Add(NewButton(toolBarButtonSource));
			}
			return toolBarOD;
		}

		///<summary>The ODToolBarButton the source makes.</summary>
		public static ODToolBarButton NewButton(ToolBarButtonSource toolBarButtonSource) {
			ODToolBarButtonStyle style=(ODToolBarButtonStyle)Enum.Parse(typeof(ODToolBarButtonStyle),toolBarButtonSource.Style);
			if(style==ODToolBarButtonStyle.Separator) {
				return new ODToolBarButton(ODToolBarButtonStyle.Separator);
			}
			ODToolBarButton odToolBarButton;
			if(toolBarButtonSource.Icon!="") {
				odToolBarButton=new ODToolBarButton(toolBarButtonSource.Text,(EnumIcons)Enum.Parse(typeof(EnumIcons),toolBarButtonSource.Icon),"",toolBarButtonSource.Text);
			}
			else {
				odToolBarButton=new ODToolBarButton(toolBarButtonSource.Text,toolBarButtonSource.ImageIndex,"",toolBarButtonSource.Text);
			}
			odToolBarButton.Style=style;
			return odToolBarButton;
		}

		///<summary>Lays the buttons out with the toolbar's own ComputeButtonsizes (as its OnPaint does) and returns them in order.</summary>
		public static List<LaidOutItem> LayOut(ToolBarOD toolBarOD,ToolBarSource toolBarSource) {
			MethodInfo methodInfo=typeof(ToolBarOD).GetMethod("ComputeButtonsizes",BindingFlags.Instance|BindingFlags.NonPublic);
			using(Bitmap bitmap=new Bitmap(1,1))
			using(Graphics g=Graphics.FromImage(bitmap)) {
				methodInfo.Invoke(toolBarOD,new object[] { g });
			}
			List<LaidOutItem> list=new List<LaidOutItem>();
			for(int i=0;i<toolBarOD.Buttons.Count;i++) {
				ODToolBarButton odToolBarButton=toolBarOD.Buttons[i];
				list.Add(new LaidOutItem { Text=odToolBarButton.Text,Left=odToolBarButton.Bounds.Left,Right=odToolBarButton.Bounds.Right,
					IsSeparator=odToolBarButton.Style==ODToolBarButtonStyle.Separator,IsConditional=toolBarSource.ListButtons[i].IsConditional });
			}
			return list;
		}

		///<summary>The WPF toolbar with the buttons, measured by WPF's layout (no window: layout runs without one). Call on an STA thread.</summary>
		public static List<LaidOutItem> LayOutWpf(ToolBarSource toolBarSource) {
			WpfControls.UI.ToolBar toolBar=BuildToolBarWpf(toolBarSource);
			toolBar.Measure(new System.Windows.Size(double.PositiveInfinity,HEIGHT_TOOLBAR));
			System.Windows.Controls.StackPanel stackPanel=(System.Windows.Controls.StackPanel)toolBar.FindName("stackPanel");
			List<LaidOutItem> list=new List<LaidOutItem>();
			double x=0;
			for(int i=0;i<stackPanel.Children.Count;i++) {
				System.Windows.UIElement uIElement=stackPanel.Children[i];
				double width=uIElement.DesiredSize.Width;
				ToolBarButtonSource toolBarButtonSource=toolBarSource.ListButtons[i];
				list.Add(new LaidOutItem { Text=toolBarButtonSource.Text,Left=(int)Math.Round(x),Right=(int)Math.Round(x+width),
					IsSeparator=toolBarButtonSource.Style=="Separator",IsConditional=toolBarButtonSource.IsConditional });
				x+=width;
			}
			return list;
		}

		///<summary>A WPF ToolBar with the source's buttons. Call on an STA thread.</summary>
		public static WpfControls.UI.ToolBar BuildToolBarWpf(ToolBarSource toolBarSource) {
			WpfControls.UI.ToolBar toolBar=new WpfControls.UI.ToolBar();
			foreach(ToolBarButtonSource toolBarButtonSource in toolBarSource.ListButtons) {
				if(toolBarButtonSource.Style=="Separator") {
					toolBar.AddSeparator();
					continue;
				}
				WpfControls.UI.EnumIcons icon=toolBarButtonSource.Icon=="" ? WpfControls.UI.EnumIcons.None
					: (WpfControls.UI.EnumIcons)Enum.Parse(typeof(WpfControls.UI.EnumIcons),toolBarButtonSource.Icon);
				WpfControls.UI.ToolBarButtonStyle style=(WpfControls.UI.ToolBarButtonStyle)Enum.Parse(typeof(WpfControls.UI.ToolBarButtonStyle),toolBarButtonSource.Style);
				EventHandler eventHandler=(sender,e) => { };
				toolBar.Add(toolBarButtonSource.Text,eventHandler,icon,style);
			}
			return toolBar;
		}

		///<summary>The main menu's top-level items laid out by MenuStripOD.LayoutItems's formula at 100%: the text's MeasureString width in the font
		///plus 10, one after the other from 0.</summary>
		public static List<LaidOutItem> LayOutMenu(List<MenuItemSource> listItems,Font font) {
			List<LaidOutItem> list=new List<LaidOutItem>();
			int x=0;
			using(Bitmap bitmap=new Bitmap(1,1))
			using(Graphics g=Graphics.FromImage(bitmap)) {
				foreach(MenuItemSource menuItemSource in listItems) {
					int width=(int)g.MeasureString(menuItemSource.Text,font).Width+10;
					list.Add(new LaidOutItem { Text=menuItemSource.Text,Left=x,Right=x+width,IsConditional=menuItemSource.IsConditional });
					x+=width;
				}
			}
			return list;
		}

		///<summary>The items whose right edge passes the width, as "caption" (separators skipped).</summary>
		public static List<string> GetOverflow(List<LaidOutItem> list,int width) {
			return list.Where(x => !x.IsSeparator && x.Right>width).Select(x => x.Text).ToList();
		}
	}
}
