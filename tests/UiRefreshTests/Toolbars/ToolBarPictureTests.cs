// The pictures of UI-6 (docs/specs/ui-refresh-plan.md, section 7's UI-6 row and section 8's G-UI2; docs/port-notes/UI-6.md): each module
// toolbar and the menu bar as a strip, and every state of the toolbar, menu, module bar, button panel and light signal grid, drawn off screen
// by the controls' own painting (DrawToBitmap; nothing is shown). The same test made the before set on main's code and makes the after set on
// the package's branch, so both come from one rule. Each run checks that every picture has ink; with OPMS_UI6_PICTURES set to a folder it
// also writes the PNGs there (strips\, states\, and toolbar-fit.tsv) for tools/New-UiGallery.ps1. The WPF toolbar is drawn with
// RenderTargetBitmap, which draws nothing while the desktop session is disconnected: its picture is then left out and said so. No database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using UiRefreshTests.Scan;

namespace UiRefreshTests.Toolbars {
	[TestClass]
	public class ToolBarPictureTests {
		///<summary>The folder the pictures are written to, when set.</summary>
		public const string VARIABLE_PICTURES="OPMS_UI6_PICTURES";
		///<summary>The window width the module strips are drawn at (the harness's main window, 1366 x 768).</summary>
		public const int WIDTH_WINDOW_STRIPS=1366;
		///<summary>The scales of the state pictures (LayoutManager's scale, as G-UI1 part 2).</summary>
		public static readonly float[] ArrayScales=new float[] { 1f,1.25f,1.5f,2f };

		///<summary>A picture: its path under the folder, and the bitmap.</summary>
		public class Picture {
			public string Name;
			public Bitmap Bitmap;
		}

		[TestMethod]
		public void Pictures_EveryToolbarMenuAndModuleBarState() {
			List<Picture> listPictures=new List<Picture>();
			List<string> listNotes=new List<string>();
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				//The program's start-up call on its UI thread (every exe's Main, UI-6): the token renderer for the stock menus of this thread.
				UiTheme.InitWinForms();
				listPictures.AddRange(DrawModuleStrips(listNotes));
				foreach(float scale in ArrayScales) {
					listPictures.Add(DrawToolbarStates(scale));
					listPictures.Add(DrawModuleBar(scale));
					listPictures.Add(DrawButtonPanel(scale));
					listPictures.Add(DrawLightSignals(scale));
				}
				listPictures.Add(DrawMenuBar());
				listPictures.Add(DrawMenuDropDown());
				foreach(Picture picture in new[] { DrawWpfToolbarStates(),DrawWpfMenus() }) {
					if(picture.Bitmap==null) {
						listNotes.Add(picture.Name+": not drawn; WPF draws nothing while the desktop session is disconnected (a connected run draws it).");
						continue;
					}
					listPictures.Add(picture);
				}
			});
			string folder=Environment.GetEnvironmentVariable(VARIABLE_PICTURES);
			List<string> listBlank=new List<string>();
			foreach(Picture picture in listPictures) {
				if(!HasInk(picture.Bitmap)) {
					listBlank.Add(picture.Name);
				}
				if(!string.IsNullOrWhiteSpace(folder)) {
					string path=Path.Combine(folder,picture.Name.Replace('/','\\'));
					Directory.CreateDirectory(Path.GetDirectoryName(path));
					picture.Bitmap.Save(path,ImageFormat.Png);
				}
				picture.Bitmap.Dispose();
			}
			if(!string.IsNullOrWhiteSpace(folder)) {
				File.WriteAllLines(Path.Combine(folder,"picture-notes.txt"),listNotes);
			}
			foreach(string note in listNotes) {
				Console.WriteLine(note);
			}
			Assert.AreEqual(0,listBlank.Count,"Pictures without ink: "+string.Join(", ",listBlank));
		}

		#region Toolbars
		///<summary>Each module toolbar at the strips' window width in today's font, with the module's own ImageList.</summary>
		public static List<Picture> DrawModuleStrips(List<string> listNotes) {
			List<Picture> list=new List<Picture>();
			foreach(ToolBarSource toolBarSource in ToolBarSources.ReadAll()) {
				int width=toolBarSource.FuncWidth(ToolBarSources.GetModuleWidth(WIDTH_WINDOW_STRIPS));
				if(toolBarSource.IsWpf) {
					Bitmap bitmapWpf=DrawWpfToolBar(toolBarSource,width);
					if(bitmapWpf==null) {
						listNotes.Add("strips/"+toolBarSource.Id+".png: not drawn; WPF draws nothing while the desktop session is disconnected (a connected run draws it).");
						continue;
					}
					list.Add(new Picture { Name="strips/"+toolBarSource.Id+".png",Bitmap=bitmapWpf });
					continue;
				}
				ImageList imageList=ToolBarModel.GetImageList(toolBarSource);
				using(ToolBarOD toolBarOD=ToolBarModel.BuildToolBarOD(toolBarSource,imageList,width,null)) {
					list.Add(new Picture { Name="strips/"+toolBarSource.Id+".png",Bitmap=Draw(toolBarOD) });
				}
			}
			return list;
		}

		///<summary>One toolbar with a button in each state: rest, hover, pressed, the drop-down part pressed, toggled, disabled, error (IsRed), error on
		///hover, notify (a drop-down's NotificationText), a label, an icon button at rest and on hover, an ImageList button, separators.</summary>
		public static Picture DrawToolbarStates(float scale) {
			ToolBarOD toolBarOD=new ToolBarOD();
			toolBarOD.LayoutManager.SetScaleMS(scale);
			toolBarOD.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f*scale);
			ImageList imageList=ToolBarModel.GetImageList(ToolBarSources.ReadAll().Single(x => x.Id=="account"));
			toolBarOD.ImageList=imageList;
			toolBarOD.Size=new Size(UiTheme.ScaleI(1300,scale),UiTheme.ScaleI(ToolBarModel.HEIGHT_TOOLBAR,scale));
			AddButton(toolBarOD,"Rest",ODToolBarButtonStyle.NormalButton,OpenPmsDental.UI.ToolBarButtonState.Normal);
			AddButton(toolBarOD,"Hover",ODToolBarButtonStyle.NormalButton,OpenPmsDental.UI.ToolBarButtonState.Hover);
			AddButton(toolBarOD,"Pressed",ODToolBarButtonStyle.NormalButton,OpenPmsDental.UI.ToolBarButtonState.Pressed);
			toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
			AddButton(toolBarOD,"Drop rest",ODToolBarButtonStyle.DropDownButton,OpenPmsDental.UI.ToolBarButtonState.Normal);
			AddButton(toolBarOD,"Drop hover",ODToolBarButtonStyle.DropDownButton,OpenPmsDental.UI.ToolBarButtonState.Hover);
			AddButton(toolBarOD,"Drop pressed",ODToolBarButtonStyle.DropDownButton,OpenPmsDental.UI.ToolBarButtonState.DropPressed);
			ODToolBarButton odToolBarButton=AddButton(toolBarOD,"Notify",ODToolBarButtonStyle.DropDownButton,OpenPmsDental.UI.ToolBarButtonState.Normal);
			odToolBarButton.NotificationText="3";
			toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
			odToolBarButton=AddButton(toolBarOD,"Toggled",ODToolBarButtonStyle.ToggleButton,OpenPmsDental.UI.ToolBarButtonState.Normal);
			odToolBarButton.IsTogglePushed=true;
			AddButton(toolBarOD,"Toggle",ODToolBarButtonStyle.ToggleButton,OpenPmsDental.UI.ToolBarButtonState.Normal);
			odToolBarButton=AddButton(toolBarOD,"Disabled",ODToolBarButtonStyle.DropDownButton,OpenPmsDental.UI.ToolBarButtonState.Normal);
			odToolBarButton.Enabled=false;
			odToolBarButton=AddButton(toolBarOD,"Error",ODToolBarButtonStyle.NormalButton,OpenPmsDental.UI.ToolBarButtonState.Normal);
			odToolBarButton.IsRed=true;
			odToolBarButton=AddButton(toolBarOD,"Error hover",ODToolBarButtonStyle.NormalButton,OpenPmsDental.UI.ToolBarButtonState.Hover);
			odToolBarButton.IsRed=true;
			toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
			AddButton(toolBarOD,"Label:",ODToolBarButtonStyle.Label,OpenPmsDental.UI.ToolBarButtonState.Normal);
			odToolBarButton=new ODToolBarButton("Icon",EnumIcons.Add,"","Icon");
			toolBarOD.Buttons.Add(odToolBarButton);
			odToolBarButton=new ODToolBarButton("Icon hover",EnumIcons.CommLog,"","IconHover");
			odToolBarButton.State=OpenPmsDental.UI.ToolBarButtonState.Hover;
			toolBarOD.Buttons.Add(odToolBarButton);
			odToolBarButton=new ODToolBarButton("",EnumIcons.Recall,"","IconOnly");
			toolBarOD.Buttons.Add(odToolBarButton);
			odToolBarButton=new ODToolBarButton("Icon off",EnumIcons.DeleteX,"","IconDisabled");
			odToolBarButton.Enabled=false;
			toolBarOD.Buttons.Add(odToolBarButton);
			toolBarOD.Buttons.Add(new ODToolBarButton("Image list",1,"","ImageList"));
			using(toolBarOD) {
				return new Picture { Name="states/toolbar-"+GetScaleText(scale)+".png",Bitmap=Draw(toolBarOD) };
			}
		}

		private static ODToolBarButton AddButton(ToolBarOD toolBarOD,string text,ODToolBarButtonStyle style,OpenPmsDental.UI.ToolBarButtonState state) {
			ODToolBarButton odToolBarButton=new ODToolBarButton(text,-1,"",text);
			odToolBarButton.Style=style;
			odToolBarButton.State=state;
			toolBarOD.Buttons.Add(odToolBarButton);
			return odToolBarButton;
		}

		///<summary>The WPF toolbar drawn with RenderTargetBitmap at its size; null when WPF drew nothing (a disconnected session).</summary>
		public static Bitmap DrawWpfToolBar(ToolBarSource toolBarSource,int width) {
			WpfControls.UI.ToolBar toolBar=ToolBarModel.BuildToolBarWpf(toolBarSource);
			toolBar.Width=width;
			toolBar.Height=ToolBarModel.HEIGHT_TOOLBAR;
			toolBar.Measure(new System.Windows.Size(width,ToolBarModel.HEIGHT_TOOLBAR));
			toolBar.Arrange(new System.Windows.Rect(0,0,width,ToolBarModel.HEIGHT_TOOLBAR));
			toolBar.UpdateLayout();
			System.Windows.Media.Imaging.RenderTargetBitmap renderTargetBitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(width,ToolBarModel.HEIGHT_TOOLBAR,96,96,
				System.Windows.Media.PixelFormats.Pbgra32);
			renderTargetBitmap.Render(toolBar);
			System.Windows.Media.Imaging.PngBitmapEncoder pngBitmapEncoder=new System.Windows.Media.Imaging.PngBitmapEncoder();
			pngBitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTargetBitmap));
			using(MemoryStream memoryStream=new MemoryStream()) {
				pngBitmapEncoder.Save(memoryStream);
				memoryStream.Position=0;
				using(Bitmap bitmapRead=new Bitmap(memoryStream)) {
					Bitmap bitmap=new Bitmap(width,ToolBarModel.HEIGHT_TOOLBAR,PixelFormat.Format24bppRgb);
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(Color.White);
						g.DrawImage(bitmapRead,0,0,width,ToolBarModel.HEIGHT_TOOLBAR);
					}
					bool isEmpty=true;
					for(int x=0;x<bitmapRead.Width && isEmpty;x+=3) {
						for(int y=0;y<bitmapRead.Height;y++) {
							if(bitmapRead.GetPixel(x,y).A!=0) {
								isEmpty=false;
								break;
							}
						}
					}
					if(isEmpty) {
						bitmap.Dispose();
						return null;
					}
					return bitmap;
				}
			}
		}
		#endregion Toolbars

		#region WPF
		///<summary>The WPF toolbar (WpfControls.UI.ToolBar) with a button in each state: rest, hover, pressed, toggled, disabled, a drop-down, icons,
		///a separator. The hover and pressed looks are set as the mouse events set them. Bitmap null when WPF drew nothing.</summary>
		public static Picture DrawWpfToolbarStates() {
			WpfControls.UI.ToolBar toolBar=new WpfControls.UI.ToolBar();
			EventHandler eventHandler=(sender,e) => { };
			toolBar.Add("Rest",eventHandler);
			WpfControls.UI.ToolBarButton toolBarButtonHover=new WpfControls.UI.ToolBarButton("Hover",eventHandler);
			toolBar.Add(toolBarButtonHover);
			WpfControls.UI.ToolBarButton toolBarButtonPressed=new WpfControls.UI.ToolBarButton("Pressed",eventHandler);
			toolBar.Add(toolBarButtonPressed);
			toolBar.AddSeparator();
			toolBar.Add("Drop",eventHandler,WpfControls.UI.EnumIcons.Print,WpfControls.UI.ToolBarButtonStyle.DropDownButton);
			WpfControls.UI.ToolBarButton toolBarButtonToggled=new WpfControls.UI.ToolBarButton("Toggled",eventHandler,WpfControls.UI.EnumIcons.None,
				WpfControls.UI.ToolBarButtonStyle.ToggleButton);
			toolBarButtonToggled.IsTogglePushed=true;
			toolBar.Add(toolBarButtonToggled);
			WpfControls.UI.ToolBarButton toolBarButtonDisabled=new WpfControls.UI.ToolBarButton("Disabled",eventHandler,WpfControls.UI.EnumIcons.DeleteX);
			toolBarButtonDisabled.IsEnabled=false;
			toolBar.Add(toolBarButtonDisabled);
			toolBar.Add("Info",eventHandler,WpfControls.UI.EnumIcons.Info);
			toolBar.Add("",eventHandler,WpfControls.UI.EnumIcons.ScanPhoto);
			SetWpfButtonState(toolBarButtonHover,"_isHover");
			SetWpfButtonState(toolBarButtonPressed,"_isPressed");
			SetWpfButtonState(toolBarButtonToggled,null);
			return new Picture { Name="states/toolbar-wpf.png",Bitmap=RenderWpf(toolBar,600,ToolBarModel.HEIGHT_TOOLBAR) };
		}

		private static void SetWpfButtonState(WpfControls.UI.ToolBarButton toolBarButton,string field) {
			if(field!=null) {
				typeof(WpfControls.UI.ToolBarButton).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic)?.SetValue(toolBarButton,true);
			}
			typeof(WpfControls.UI.ToolBarButton).GetMethod("SetColors",BindingFlags.Instance|BindingFlags.NonPublic)?.Invoke(toolBarButton,null);
		}

		///<summary>A WPF context menu (WpfControls.UI.ContextMenu, drawn without its popup) with items at rest, a checked one, a separator, a disabled
		///one, one with a sub-menu and one with a shortcut, beside the WPF menu bar. Bitmap null when WPF drew nothing.</summary>
		public static Picture DrawWpfMenus() {
			WpfControls.UI.Menu menu=new WpfControls.UI.Menu();
			menu.Width=260;
			foreach(string text in new[] { "File","Setup","Reports","Help" }) {
				WpfControls.UI.MenuItem menuItemTop=new WpfControls.UI.MenuItem(text);
				menuItemTop.Add(new WpfControls.UI.MenuItem("Item"));
				menu.Add(menuItemTop);
			}
			Bitmap bitmapMenu=RenderWpf(menu,270,30);
			//A context menu cannot have a parent: laid out and drawn on its own.
			WpfControls.UI.ContextMenu contextMenu=new WpfControls.UI.ContextMenu();
			contextMenu.Add(new WpfControls.UI.MenuItem("Standard",(sender,e) => { }));
			contextMenu.AddSeparator();
			contextMenu.Add(new WpfControls.UI.MenuItem("Show inactive",(sender,e) => { }) { IsChecked=true });
			contextMenu.Add(new WpfControls.UI.MenuItem("Export (disabled)",(sender,e) => { }) { IsEnabled=false });
			WpfControls.UI.MenuItem menuItemSub=new WpfControls.UI.MenuItem("More");
			menuItemSub.Add(new WpfControls.UI.MenuItem("Sub item",(sender,e) => { }));
			contextMenu.Add(menuItemSub);
			contextMenu.Add(new WpfControls.UI.MenuItem("Print",(sender,e) => { },"P"));
			int width=0;
			int height=0;
			for(int pass=0;pass<3;pass++) {//the shared column sizes settle after a first pass, as in a popup
				contextMenu.Measure(new System.Windows.Size(double.PositiveInfinity,double.PositiveInfinity));
				width=(int)Math.Ceiling(contextMenu.DesiredSize.Width);
				height=(int)Math.Ceiling(contextMenu.DesiredSize.Height);
				contextMenu.Arrange(new System.Windows.Rect(0,0,width,height));
				contextMenu.UpdateLayout();
			}
			Bitmap bitmapContext=RenderToBitmap(contextMenu,width,height);
			if(bitmapMenu==null || bitmapContext==null) {
				bitmapMenu?.Dispose();
				bitmapContext?.Dispose();
				return new Picture { Name="strips/menu-wpf.png",Bitmap=null };
			}
			Bitmap bitmap=new Bitmap(bitmapMenu.Width+12+bitmapContext.Width,Math.Max(bitmapMenu.Height,bitmapContext.Height),PixelFormat.Format24bppRgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);
				g.DrawImage(bitmapMenu,0,0);
				g.DrawImage(bitmapContext,bitmapMenu.Width+12,0);
			}
			bitmapMenu.Dispose();
			bitmapContext.Dispose();
			return new Picture { Name="strips/menu-wpf.png",Bitmap=bitmap };
		}

		///<summary>An element that is laid out already, drawn with RenderTargetBitmap over white; null when WPF drew nothing.</summary>
		public static Bitmap RenderToBitmap(System.Windows.Media.Visual visual,int width,int height) {
			System.Windows.Media.Imaging.RenderTargetBitmap renderTargetBitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(width,height,96,96,
				System.Windows.Media.PixelFormats.Pbgra32);
			renderTargetBitmap.Render(visual);
			System.Windows.Media.Imaging.PngBitmapEncoder pngBitmapEncoder=new System.Windows.Media.Imaging.PngBitmapEncoder();
			pngBitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTargetBitmap));
			using(MemoryStream memoryStream=new MemoryStream()) {
				pngBitmapEncoder.Save(memoryStream);
				memoryStream.Position=0;
				using(Bitmap bitmapRead=new Bitmap(memoryStream)) {
					bool isEmpty=true;
					for(int x=0;x<bitmapRead.Width && isEmpty;x++) {
						for(int y=0;y<bitmapRead.Height;y+=2) {
							if(bitmapRead.GetPixel(x,y).A!=0) {
								isEmpty=false;
								break;
							}
						}
					}
					if(isEmpty) {
						return null;
					}
					Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format24bppRgb);
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(Color.White);
						g.DrawImage(bitmapRead,0,0,width,height);
					}
					return bitmap;
				}
			}
		}

		///<summary>Lays the element out at the size and draws it with RenderTargetBitmap on white; null when WPF drew nothing.</summary>
		public static Bitmap RenderWpf(System.Windows.FrameworkElement frameworkElement,int width,int height) {
			System.Windows.Controls.Grid gridRoot=new System.Windows.Controls.Grid { Width=width,Height=height,Background=System.Windows.Media.Brushes.White };
			frameworkElement.HorizontalAlignment=System.Windows.HorizontalAlignment.Left;
			frameworkElement.VerticalAlignment=System.Windows.VerticalAlignment.Top;
			gridRoot.Children.Add(frameworkElement);
			gridRoot.Measure(new System.Windows.Size(width,height));
			gridRoot.Arrange(new System.Windows.Rect(0,0,width,height));
			gridRoot.UpdateLayout();
			System.Windows.Media.Imaging.RenderTargetBitmap renderTargetBitmap=new System.Windows.Media.Imaging.RenderTargetBitmap(width,height,96,96,
				System.Windows.Media.PixelFormats.Pbgra32);
			renderTargetBitmap.Render(gridRoot);
			System.Windows.Media.Imaging.PngBitmapEncoder pngBitmapEncoder=new System.Windows.Media.Imaging.PngBitmapEncoder();
			pngBitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTargetBitmap));
			using(MemoryStream memoryStream=new MemoryStream()) {
				pngBitmapEncoder.Save(memoryStream);
				memoryStream.Position=0;
				using(Bitmap bitmapRead=new Bitmap(memoryStream)) {
					if(bitmapRead.GetPixel(0,0).A==0) {
						return null;//the white root drew nothing
					}
					Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format24bppRgb);
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.DrawImage(bitmapRead,0,0,width,height);
					}
					return bitmap;
				}
			}
		}
		#endregion WPF

		#region Menus
		///<summary>The main menu's top-level items in a MenuOD of the strips' window width; the Reports item selected (the hover look).</summary>
		public static Picture DrawMenuBar() {
			MenuOD menuOD=new MenuOD();
			menuOD.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f);
			menuOD.Size=new Size(ToolBarSources.GetMenuWidth(WIDTH_WINDOW_STRIPS),24);
			foreach(MenuItemSource menuItemSource in ToolBarSources.ReadMainMenu()) {
				MenuItemOD menuItemOD=new MenuItemOD(menuItemSource.Text);
				menuItemOD.Add(new MenuItemOD("Item"));
				menuOD.Add(menuItemOD);
			}
			using(menuOD) {
				using(Form form=new Form()) {
					form.Controls.Add(menuOD);
					menuOD.CreateControl();
					ToolStrip toolStrip=(ToolStrip)menuOD.Controls[0];
					ToolStripItem toolStripItemReports=toolStrip.Items.Cast<ToolStripItem>().FirstOrDefault(x => x.Text=="&Reports");
					toolStripItemReports?.Select();
					return new Picture { Name="strips/menu.png",Bitmap=DesignSheetWindows.Capture(menuOD,Rectangle.Empty) };
				}
			}
		}

		///<summary>A drop-down menu (a ContextMenuStrip, as the 102 stock context menus): items at rest, one selected (hover), a checked one, a
		///separator, a disabled one, one with a sub-menu arrow and one with a shortcut.</summary>
		public static Picture DrawMenuDropDown() {
			using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip()) {
				contextMenuStrip.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f);
				contextMenuStrip.Items.Add(new ToolStripMenuItem("Standard"));
				ToolStripMenuItem toolStripMenuItemHover=new ToolStripMenuItem("Custom (hover)");
				contextMenuStrip.Items.Add(toolStripMenuItemHover);
				contextMenuStrip.Items.Add(new ToolStripSeparator());
				contextMenuStrip.Items.Add(new ToolStripMenuItem("Show inactive") { Checked=true });
				contextMenuStrip.Items.Add(new ToolStripMenuItem("Export (disabled)") { Enabled=false });
				ToolStripMenuItem toolStripMenuItemSub=new ToolStripMenuItem("More");
				toolStripMenuItemSub.DropDownItems.Add(new ToolStripMenuItem("Sub item"));
				contextMenuStrip.Items.Add(toolStripMenuItemSub);
				contextMenuStrip.Items.Add(new ToolStripMenuItem("Print",null,null,Keys.Control|Keys.P));
				contextMenuStrip.PerformLayout();
				Size size=contextMenuStrip.GetPreferredSize(Size.Empty);
				contextMenuStrip.Size=size;
				contextMenuStrip.CreateControl();
				toolStripMenuItemHover.Select();
				Bitmap bitmap=new Bitmap(size.Width,size.Height,PixelFormat.Format24bppRgb);
				contextMenuStrip.DrawToBitmap(bitmap,new Rectangle(0,0,size.Width,size.Height));
				return new Picture { Name="strips/menu-dropdown.png",Bitmap=bitmap };
			}
		}
		#endregion Menus

		#region Module bar, button panel, light signals
		///<summary>The module bar at the scale with Account selected.</summary>
		public static Picture DrawModuleBar(float scale) {
			using(ModuleBar moduleBar=new ModuleBar()) {
				moduleBar.LayoutManager.SetScaleMS(scale);
				moduleBar.Size=new Size(UiTheme.ScaleI(51,scale),UiTheme.ScaleI(7*58,scale));
				moduleBar.SelectedModule=EnumModuleType.Account;
				return new Picture { Name="states/modulebar-"+GetScaleText(scale)+".png",Bitmap=Draw(moduleBar) };
			}
		}

		///<summary>A button panel (the Chart module's quick buttons) with labels and buttons on two rows.</summary>
		public static Picture DrawButtonPanel(float scale) {
			using(ODButtonPanel odButtonPanel=new ODButtonPanel()) {
				odButtonPanel.LayoutManager.SetScaleMS(scale);
				odButtonPanel.Size=new Size(UiTheme.ScaleI(320,scale),UiTheme.ScaleI(40,scale));
				odButtonPanel.BeginUpdate();
				string[][] arrayRows=new string[][] { new[] { "L:Exams","B:Periodic","B:Comp","B:Limited" },new[] { "L:Radiographs","B:BWX","B:FMX","B:Pano" } };
				for(int y=0;y<arrayRows.Length;y++) {
					for(int x=0;x<arrayRows[y].Length;x++) {
						ODPanelItem odPanelItem=new ODPanelItem();
						odPanelItem.Text=arrayRows[y][x].Substring(2);
						odPanelItem.ItemType=arrayRows[y][x].StartsWith("L") ? ODPanelItemType.Label : ODPanelItemType.Button;
						odPanelItem.YPos=y;
						odPanelItem.ItemOrder=x;
						odButtonPanel.ListODPanelItems.Add(odPanelItem);
					}
				}
				odButtonPanel.EndUpdate();
				return new Picture { Name="states/buttonpanel-"+GetScaleText(scale)+".png",Bitmap=Draw(odButtonPanel) };
			}
		}

		///<summary>The light signal grid with five buttons: two unlit, one lit (an office colour), one lit on hover, one unlit on hover.</summary>
		public static Picture DrawLightSignals(float scale) {
			using(LightSignalGrid lightSignalGrid=new LightSignalGrid()) {
				lightSignalGrid.LayoutManager.SetScaleMS(scale);
				lightSignalGrid.Size=new Size(UiTheme.ScaleI(50,scale),UiTheme.ScaleI(5*25+1,scale));
				SigButDef[] arraySigButDefs=new SigButDef[] {
					new SigButDef { SigButDefNum=1,ButtonIndex=0,ButtonText="Doctor" },
					new SigButDef { SigButDefNum=2,ButtonIndex=1,ButtonText="Hygiene" },
					new SigButDef { SigButDefNum=3,ButtonIndex=2,ButtonText="Front" },
					new SigButDef { SigButDefNum=4,ButtonIndex=3,ButtonText="Lab" },
					new SigButDef { SigButDefNum=5,ButtonIndex=4,ButtonText="Steril" },
				};
				lightSignalGrid.SetButtons(arraySigButDefs);
				lightSignalGrid.SetButtonActive(2,Color.FromArgb(255,128,128),null);
				lightSignalGrid.SetButtonActive(3,Color.FromArgb(128,255,128),null);
				List<SignalButtonState> listStates=(List<SignalButtonState>)typeof(LightSignalGrid).GetField("_listSignalButtonStates",BindingFlags.Instance|BindingFlags.NonPublic)
					.GetValue(lightSignalGrid);
				listStates[3].State=OpenPmsDental.UI.ToolBarButtonState.Hover;
				listStates[4].State=OpenPmsDental.UI.ToolBarButtonState.Hover;
				return new Picture { Name="states/lightsignals-"+GetScaleText(scale)+".png",Bitmap=Draw(lightSignalGrid) };
			}
		}
		#endregion Module bar, button panel, light signals

		#region Helpers
		///<summary>The control's own painting into a bitmap of its size (DrawToBitmap creates its handle; nothing is shown).</summary>
		public static Bitmap Draw(Control control) {
			Bitmap bitmap=new Bitmap(control.Width,control.Height,PixelFormat.Format24bppRgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.Magenta);
			}
			control.DrawToBitmap(bitmap,new Rectangle(0,0,control.Width,control.Height));
			return bitmap;
		}

		public static string GetScaleText(float scale) {
			return ((int)Math.Round(scale*100)).ToString();
		}

		///<summary>True when the picture has at least two colours (a control that drew only its background has one).</summary>
		public static bool HasInk(Bitmap bitmap) {
			HashSet<int> hashSetColors=new HashSet<int>();
			for(int x=0;x<bitmap.Width;x+=Math.Max(1,bitmap.Width/200)) {
				for(int y=0;y<bitmap.Height;y++) {
					hashSetColors.Add(bitmap.GetPixel(x,y).ToArgb());
					if(hashSetColors.Count>2) {
						return true;
					}
				}
			}
			return false;
		}
		#endregion Helpers
	}
}
