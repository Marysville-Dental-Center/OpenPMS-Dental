// Gallery G-UI3-uiovf for package UI-OVF (docs/port-notes/UI-OVF.md; docs/specs/ui-refresh-plan.md section 8): the toolbars that overflow at a
// 1024 px window (Data\toolbar-overflow.tsv) before and after, at 100% and 150%: before, the buttons cut off at the right edge (the toolbar laid
// out wide and cut to its width, as main drew it); after, the » overflow button; and after with its drop-down open (drawn under the toolbar,
// right-aligned to the button, as it opens). Then the WPF buttons, check box, radio button and toolbar buttons with the keyboard focus ring, at
// 100% and 150% (the shared focus visual style laid over each, as WPF's adorner draws it while the control has keyboard focus: a control off
// screen cannot take it). Off screen. Opt-in: OPMS_UI_OVF_GALLERY names the folder; tools/New-UiGallery.ps1 then makes the page from it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;
using UiRefreshTests.Scan;

namespace UiRefreshTests.Toolbars {
	[TestClass]
	public class OverflowGallery {
		public const string VARIABLE_GALLERY="OPMS_UI_OVF_GALLERY";
		public static readonly float[] ArrayScales=new float[] { 1f,1.5f };

		[TestMethod]
		public void Gallery_OverflowedToolbarsAt1024_AndWpfFocus() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_GALLERY);
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: "+VARIABLE_GALLERY+" names the folder for gallery G-UI3-uiovf.");
			}
			Directory.CreateDirectory(folder);
			List<string> listWritten=new List<string>();
			List<string> listNotes=new List<string>();
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				UiTheme.InitWinForms();
				bool rendersWpf=ControlPictures.RendersWpf();
				Dictionary<string,string> dictListed=ToolBarFitTests.ReadOverflowList();
				foreach(ToolBarSource toolBarSource in ToolBarSources.ReadAll()) {
					foreach(Tuple<string,Font> tupleFont in ToolBarFitTests.GetFonts()) {
						if(!dictListed.ContainsKey(toolBarSource.Id+"\t"+tupleFont.Item1+"\t1024")) {
							continue;
						}
						foreach(float scale in ArrayScales) {
							string name=toolBarSource.Id+"-"+tupleFont.Item1+"-"+ToolBarPictureTests.GetScaleText(scale);
							int width96=toolBarSource.FuncWidth(ToolBarSources.GetModuleWidth(1024));
							if(toolBarSource.IsWpf) {
								if(!rendersWpf) {
									listNotes.Add(name+": WPF draws nothing in this session.");
									continue;
								}
								SaveWpf(folder,name,toolBarSource,width96,scale,listWritten);
								continue;
							}
							SaveToolBarOD(folder,name,toolBarSource,tupleFont,width96,scale,listWritten);
						}
					}
				}
				if(rendersWpf) {
					foreach(float scale in ArrayScales) {
						string path=Path.Combine(folder,"wpf-focus-"+ToolBarPictureTests.GetScaleText(scale)+".png");
						using(Bitmap bitmap=DrawWpfFocus(scale)) {
							bitmap.Save(path,ImageFormat.Png);
						}
						listWritten.Add(path);
					}
				}
				else {
					listNotes.Add("wpf-focus: WPF draws nothing in this session.");
				}
			});
			File.WriteAllText(Path.Combine(folder,"notes.txt"),string.Join("\r\n",listNotes.Concat(listWritten.Select(x => "written: "+Path.GetFileName(x)))));
			Assert.IsTrue(listWritten.Count>0,string.Join("\r\n",listNotes));
		}

		///<summary>The ToolBarOD at the scale (LayoutManager's scale and the font scaled, as G-UI2's states): before (laid out wide, cut to the width),
		///after, and after with the drop-down open.</summary>
		private static void SaveToolBarOD(string folder,string name,ToolBarSource toolBarSource,Tuple<string,Font> tupleFont,int width96,float scale,
			List<string> listWritten)
		{
			ImageList imageList=ToolBarModel.GetImageList(toolBarSource);
			Font font=tupleFont.Item2??new Font(UiTheme.FONT_FAMILY_TODAY,8.25f);
			Font fontScaled=new Font(font.FontFamily,font.Size*scale);
			int width=UiTheme.ScaleI(width96,scale);
			int height=UiTheme.ScaleI(ToolBarModel.HEIGHT_TOOLBAR,scale);
			UiTheme.SetFontForTest(tupleFont.Item2==null ? UiFontOption.Today : null);
			try {
				using(ToolBarOD toolBarOD=ToolBarModel.BuildToolBarOD(toolBarSource,imageList,width*3,fontScaled)) {
					toolBarOD.LayoutManager.SetScaleMS(scale);
					toolBarOD.Size=new Size(width*3,height);
					using(Bitmap bitmapWide=ToolBarPictureTests.Draw(toolBarOD))
					using(Bitmap bitmapBefore=bitmapWide.Clone(new Rectangle(0,0,width,height),bitmapWide.PixelFormat)) {
						Save(folder,name+"-before.png",bitmapBefore,listWritten);
					}
					toolBarOD.Size=new Size(width,height);
					using(Bitmap bitmapAfter=ToolBarPictureTests.Draw(toolBarOD)) {
						Save(folder,name+"-after.png",bitmapAfter,listWritten);
						ContextMenuStrip contextMenuStrip=toolBarOD.GetOverflowMenu();
						contextMenuStrip.PerformLayout();
						Size size=contextMenuStrip.GetPreferredSize(Size.Empty);
						contextMenuStrip.Size=size;
						contextMenuStrip.CreateControl();
						//The button pressed while its drop-down is open, as ShowOverflowMenu sets it.
						ODToolBarButton toolBarButtonOverflow=(ODToolBarButton)typeof(ToolBarOD).GetField("_toolBarButtonOverflow",
							System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(toolBarOD);
						toolBarButtonOverflow.State=OpenPmsDental.UI.ToolBarButtonState.Pressed;
						using(Bitmap bitmapPressed=ToolBarPictureTests.Draw(toolBarOD))
						using(Bitmap bitmapMenu=new Bitmap(size.Width,size.Height,PixelFormat.Format24bppRgb)) {
							contextMenuStrip.DrawToBitmap(bitmapMenu,new Rectangle(0,0,size.Width,size.Height));
							using(Bitmap bitmapOpen=Compose(bitmapPressed,bitmapMenu,toolBarOD.BoundsOverflow.Right)) {
								Save(folder,name+"-after-open.png",bitmapOpen,listWritten);
							}
						}
					}
				}
			}
			finally {
				UiTheme.SetFontForTest(null);
			}
		}

		///<summary>The WPF toolbar at the scale: before (laid out wide, cut to the width), after, and after with the drop-down open.</summary>
		private static void SaveWpf(string folder,string name,ToolBarSource toolBarSource,int width96,float scale,List<string> listWritten) {
			WpfControls.UI.ToolBar toolBarWide=ToolBarModel.BuildToolBarWpf(toolBarSource);
			using(Bitmap bitmapWide=ControlPictures.RenderWpf(toolBarWide,width96*3,ToolBarModel.HEIGHT_TOOLBAR,scale))
			using(Bitmap bitmapBefore=bitmapWide.Clone(new Rectangle(0,0,UiTheme.ScaleI(width96,scale),bitmapWide.Height),bitmapWide.PixelFormat)) {
				Save(folder,name+"-before.png",bitmapBefore,listWritten);
			}
			WpfControls.UI.ToolBar toolBar=ToolBarModel.BuildToolBarWpf(toolBarSource);
			using(Bitmap bitmapAfter=ControlPictures.RenderWpf(toolBar,width96,ToolBarModel.HEIGHT_TOOLBAR,scale)) {
				Save(folder,name+"-after.png",bitmapAfter,listWritten);
				WpfControls.UI.ContextMenu contextMenu=toolBar.GetOverflowMenu();
				double widthMenu=0;
				double heightMenu=0;
				for(int pass=0;pass<3;pass++) {//the shared column sizes settle after a first pass, as in a popup
					contextMenu.Measure(new System.Windows.Size(double.PositiveInfinity,double.PositiveInfinity));
					widthMenu=Math.Ceiling(contextMenu.DesiredSize.Width);
					heightMenu=Math.Ceiling(contextMenu.DesiredSize.Height);
					contextMenu.Arrange(new System.Windows.Rect(0,0,widthMenu,heightMenu));
					contextMenu.UpdateLayout();
				}
				//The button pressed while its drop-down is open, as ShowOverflowMenu sets it.
				typeof(WpfControls.UI.ToolBarButton).GetMethod("SetMenuOpen",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)
					.Invoke(toolBar.ToolBarButtonOverflow,new object[] { true });
				using(Bitmap bitmapPressed=ControlPictures.RenderWpf(toolBar,width96,ToolBarModel.HEIGHT_TOOLBAR,scale))
				using(Bitmap bitmapMenu=ControlPictures.RenderWpf(contextMenu,widthMenu,heightMenu,scale))
				using(Bitmap bitmapOpen=Compose(bitmapPressed,bitmapMenu,bitmapAfter.Width)) {
					Save(folder,name+"-after-open.png",bitmapOpen,listWritten);
				}
			}
		}

		///<summary>The toolbar with the drop-down under it, its right edge at xRight (the overflow button's right edge), on white.</summary>
		private static Bitmap Compose(Bitmap bitmapToolBar,Bitmap bitmapMenu,int xRight) {
			int width=Math.Max(bitmapToolBar.Width,bitmapMenu.Width);
			Bitmap bitmap=new Bitmap(width,bitmapToolBar.Height+bitmapMenu.Height+8,PixelFormat.Format24bppRgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);
				g.DrawImage(bitmapToolBar,0,0,bitmapToolBar.Width,bitmapToolBar.Height);
				g.DrawImage(bitmapMenu,Math.Max(0,xRight-bitmapMenu.Width),bitmapToolBar.Height,bitmapMenu.Width,bitmapMenu.Height);
			}
			return bitmap;
		}

		private static void Save(string folder,string fileName,Bitmap bitmap,List<string> listWritten) {
			string path=Path.Combine(folder,fileName);
			bitmap.Save(path,ImageFormat.Png);
			listWritten.Add(path);
		}

		///<summary>The WPF controls that take the keyboard focus since UI-OVF, each with the focus ring as WPF draws it (the shared style laid over
		///it): a button, the default (primary) button, a check box, a radio button, and a toolbar with a focused button and its overflow button.</summary>
		private static Bitmap DrawWpfFocus(float scale) {
			System.Windows.Controls.StackPanel stackPanel=new System.Windows.Controls.StackPanel();
			stackPanel.Orientation=System.Windows.Controls.Orientation.Horizontal;
			stackPanel.Background=System.Windows.Media.Brushes.White;
			WpfControls.UI.Button button=new WpfControls.UI.Button() { Text="Delete",Icon=WpfControls.UI.EnumIcons.DeleteX };
			stackPanel.Children.Add(WithFocus(button,84,24,onAccent:false));
			WpfControls.UI.Button buttonPrimary=new WpfControls.UI.Button() { Text="Save",IsPrimary=true };
			stackPanel.Children.Add(WithFocus(buttonPrimary,75,24,onAccent:true));
			stackPanel.Children.Add(WithFocus(new WpfControls.UI.CheckBox() { Text="Inactive",Checked=true },100,20,onAccent:false));
			stackPanel.Children.Add(WithFocus(new WpfControls.UI.RadioButton() { Text="Standard",Checked=true },100,20,onAccent:false));
			WpfControls.UI.ToolBar toolBar=new WpfControls.UI.ToolBar();
			toolBar.Width=170;
			toolBar.Height=ToolBarModel.HEIGHT_TOOLBAR;
			EventHandler eventHandler=(sender,e) => { };
			toolBar.Add("Print",eventHandler,WpfControls.UI.EnumIcons.Print);
			toolBar.Add("Delete",eventHandler,WpfControls.UI.EnumIcons.DeleteX);
			toolBar.Add("Info",eventHandler,WpfControls.UI.EnumIcons.Info);
			toolBar.Add("Export",eventHandler,WpfControls.UI.EnumIcons.Export);
			System.Windows.Controls.Grid gridToolBar=new System.Windows.Controls.Grid();
			gridToolBar.Margin=new System.Windows.Thickness(12,0,0,0);
			gridToolBar.Children.Add(toolBar);
			stackPanel.Children.Add(gridToolBar);
			double width=84+75+100+100+12+170+5*8;
			stackPanel.Measure(new System.Windows.Size(width,30));
			stackPanel.Arrange(new System.Windows.Rect(0,0,width,30));
			stackPanel.UpdateLayout();
			//The focus ring on the toolbar's overflow button, as on any focused toolbar button.
			WpfControls.UI.ToolBarButton toolBarButtonOverflow=toolBar.ToolBarButtonOverflow;
			System.Windows.Controls.Control controlFocus=NewFocusVisual(toolBarButtonOverflow,onAccent:false);
			controlFocus.Width=toolBarButtonOverflow.ActualWidth;
			controlFocus.Height=toolBarButtonOverflow.ActualHeight;
			controlFocus.HorizontalAlignment=System.Windows.HorizontalAlignment.Right;
			gridToolBar.Children.Add(controlFocus);
			stackPanel.Measure(new System.Windows.Size(width,30));
			stackPanel.Arrange(new System.Windows.Rect(0,0,width,30));
			stackPanel.UpdateLayout();
			return ControlPictures.RenderWpf(stackPanel,width,30,scale);
		}

		private static System.Windows.Controls.Control NewFocusVisual(System.Windows.FrameworkElement frameworkElement,bool onAccent) {
			System.Windows.Controls.Control controlFocus=new System.Windows.Controls.Control();
			controlFocus.Style=(System.Windows.Style)frameworkElement.FindResource(onAccent ? WpfControls.UI.UiLook.KeyFocusVisualStyleOnAccent : WpfControls.UI.UiLook.KeyFocusVisualStyle);
			controlFocus.IsHitTestVisible=false;
			return controlFocus;
		}

		///<summary>The control in a cell with the focus visual style laid over it at its bounds, 4 DIP around (8 between controls).</summary>
		private static System.Windows.FrameworkElement WithFocus(System.Windows.FrameworkElement frameworkElement,double width,double height,bool onAccent) {
			System.Windows.Controls.Grid grid=new System.Windows.Controls.Grid();
			grid.Width=width;
			grid.Height=height;
			grid.Margin=new System.Windows.Thickness(4,3,4,3);
			grid.Children.Add(frameworkElement);
			System.Windows.Controls.Control controlFocus=NewFocusVisual(frameworkElement,onAccent);
			controlFocus.Width=width;
			controlFocus.Height=height;
			grid.Children.Add(controlFocus);
			return grid;
		}
	}
}
