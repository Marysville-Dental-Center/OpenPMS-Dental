// The real tabs and splitters of package UI-4b-2 (docs/port-notes/UI-4b.md) put in each state off screen (TabSplitControls), and the design sheet's
// drawing of each state at the controls' own geometry (TabSplitDrawings: SheetPainter's shapes with the light tokens, as DesignSheetControls draws
// the tab header, the tabs and the splitter, without the text). Used by TabSplitStateTests and by the G-UI1 part 2c state strips (TabSplitGallery).
// No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental.UI;

namespace UiRefreshTests {
	///<summary>The states a tab strip is drawn in: rest (the first tab selected), the third tab under the mouse, the third tab disabled, the second tab
	///with a page colour of its own (ColorTab, as the Account module's hidden splits tab), and (WPF) the second tab with keyboard focus.</summary>
	public enum TabPictureState {
		Rest,
		Hover,
		Disabled,
		ColorTab,
		Focused,
	}

	///<summary>The WinForms tab control with hover through its own mouse handler.</summary>
	public class StateTabControl : OpenPmsDental.UI.TabControl {
		public void Hover(int idx) {
			Rectangle rectangle=GetTabRect(idx);
			OnMouseMove(new MouseEventArgs(MouseButtons.None,0,rectangle.X+rectangle.Width/2,rectangle.Y+rectangle.Height/2,0));
		}
	}

	///<summary>The WinForms split container with the press (the start of a drag) through its own mouse handler.</summary>
	public class StateSplitContainer : OpenPmsDental.UI.SplitContainer {
		public void Press(Point point) {
			OnMouseDown(new MouseEventArgs(MouseButtons.Left,1,point.X,point.Y,0));
		}
	}

	///<summary>A tab strip's picture with the geometry it was drawn with, in device pixels from the control's top left corner.</summary>
	public class TabPicture : IDisposable {
		public Bitmap Bitmap;
		///<summary>Where each tab is drawn.</summary>
		public List<Rectangle> ListHeaders=new List<Rectangle>();
		public Rectangle RectangleStrip;
		public Rectangle RectanglePage;
		public EnumUiTabSide TabSide;
		public int IdxSelected;

		public void Dispose() {
			Bitmap?.Dispose();
		}
	}

	///<summary>A splitter's picture: the two panels and the splitter between them, in device pixels.</summary>
	public class SplitPicture : IDisposable {
		public Bitmap Bitmap;
		public Rectangle RectanglePanel1;
		public Rectangle RectanglePanel2;
		public Rectangle RectangleSplitter;
		public bool IsVertical;

		public void Dispose() {
			Bitmap?.Dispose();
		}
	}

	public static class TabSplitControls {
		public static readonly string[] ArrayTabTexts=new string[] { "General","Insurance","Notes" };
		public static readonly TabPictureState[] ArrayWinFormsStates=new TabPictureState[] { TabPictureState.Rest,TabPictureState.Hover,TabPictureState.Disabled,
			TabPictureState.ColorTab };
		public static readonly TabPictureState[] ArrayWpfStates=new TabPictureState[] { TabPictureState.Rest,TabPictureState.Hover,TabPictureState.Focused,
			TabPictureState.Disabled };
		///<summary>A page colour of its own (light coral, as ControlAccount colours its hidden splits tab).</summary>
		public static readonly Color ColorTabCustom=Color.LightCoral;

		#region WinForms
		///<summary>The control and its children drawn off screen (DesignSheetWindows.Capture), as a 32-bit ARGB picture.</summary>
		private static Bitmap CaptureArgb(Control control) {
			using(Bitmap bitmap=DesignSheetWindows.Capture(control,Rectangle.Empty)) {
				return bitmap.Clone(new Rectangle(Point.Empty,bitmap.Size),PixelFormat.Format32bppArgb);
			}
		}

		///<summary>A hidden form (never shown) with a panel in the Surface colour, the parent of the control under test.</summary>
		private static Form MakeHost(out System.Windows.Forms.Panel panel) {
			Form form=new Form();
			form.ShowInTaskbar=false;
			form.Size=new Size(900,600);
			panel=new System.Windows.Forms.Panel();
			panel.BackColor=SheetPainter.Light(EnumUiColor.Surface);
			panel.Bounds=new Rectangle(0,0,880,560);
			form.Controls.Add(panel);
			return form;
		}

		///<summary>UI.TabControl (300 x 100 at 96 DPI; LeftHorizontal 300 x 140) with the three tabs, the first selected, in the state. Without text the
		///tabs keep the rectangles their texts gave them (the texts are emptied after the layout, without a new one).</summary>
		public static TabPicture RenderWinFormsTabs(TabPictureState tabPictureState,EnumTabAlignment tabAlignment,bool withText,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateTabControl tabControl=new StateTabControl();
				tabControl.LayoutManager.SetScaleMS(scale);
				tabControl.TabAlignment=tabAlignment;
				int height96=tabAlignment==EnumTabAlignment.LeftHorizontal ? 140 : 100;
				tabControl.Bounds=new Rectangle(10,10,UiTheme.ScaleI(300,scale),UiTheme.ScaleI(height96,scale));
				foreach(string text in ArrayTabTexts) {
					tabControl.TabPages.Add(new OpenPmsDental.UI.TabPage(text));
				}
				panel.Controls.Add(tabControl);
				IntPtr handle=tabControl.Handle;
				tabControl.SelectedIndex=0;
				switch(tabPictureState) {
					case TabPictureState.Disabled:
						tabControl.TabPages[2].Enabled=false;
						break;
					case TabPictureState.ColorTab:
						tabControl.TabPages[1].ColorTab=ColorTabCustom;
						break;
				}
				tabControl.LayoutTabs();
				DesignSheetWindows.CreateHandles(tabControl);
				if(tabPictureState==TabPictureState.Hover) {
					tabControl.Hover(2);
				}
				List<Rectangle> listRectangles=tabControl.ListRectanglesTabs.ToList();
				if(!withText) {
					foreach(OpenPmsDental.UI.TabPage tabPage in tabControl.TabPages) {
						ListStateControls.SetPrivate(tabPage,"_text","");
					}
				}
				TabPicture tabPicture=new TabPicture();
				tabPicture.Bitmap=CaptureArgb(tabControl);
				if(!tabControl.ListRectanglesTabs.SequenceEqual(listRectangles)) {
					throw new Exception("The tabs were laid out again while they were drawn.");
				}
				int heightTabs=tabControl.LayoutManager.Scale(20);
				int width=tabControl.Width;
				int height=tabControl.Height;
				tabPicture.IdxSelected=0;
				switch(tabAlignment) {
					case EnumTabAlignment.Bottom:
						tabPicture.TabSide=EnumUiTabSide.Bottom;
						tabPicture.RectanglePage=new Rectangle(0,0,width,height-heightTabs+1);
						tabPicture.RectangleStrip=new Rectangle(0,height-heightTabs+1,width,heightTabs-1);
						tabPicture.ListHeaders=listRectangles.Select(x => new Rectangle(x.X,x.Y+1,x.Width,x.Height)).ToList();
						break;
					case EnumTabAlignment.LeftHorizontal:
						int xLeft=tabControl.LayoutManager.Scale(70)-1;
						tabPicture.TabSide=EnumUiTabSide.Left;
						tabPicture.RectanglePage=new Rectangle(xLeft,0,width-xLeft,height);
						tabPicture.RectangleStrip=new Rectangle(0,0,xLeft,height);
						tabPicture.ListHeaders=listRectangles;
						break;
					default:
						tabPicture.TabSide=EnumUiTabSide.Top;
						tabPicture.RectanglePage=new Rectangle(0,heightTabs-1,width,height-heightTabs+1);
						tabPicture.RectangleStrip=new Rectangle(0,0,width,heightTabs-1);
						tabPicture.ListHeaders=listRectangles;
						break;
				}
				return tabPicture;
			}
		}

		///<summary>UI.SplitContainer (120 x 60 at 96 DPI, the splitter 56 px from the start, its default 4 px wide) with its two panels, at rest or
		///pressed (dragging).</summary>
		public static SplitPicture RenderWinFormsSplitter(bool isVertical,bool isPressed,float scale) {
			System.Windows.Forms.Panel panel;
			using(Form form=MakeHost(out panel)) {
				StateSplitContainer splitContainer=new StateSplitContainer();
				splitContainer.LayoutManager.SetScaleMS(scale);
				OpenPmsDental.UI.SplitterPanel splitterPanel1=new OpenPmsDental.UI.SplitterPanel();
				OpenPmsDental.UI.SplitterPanel splitterPanel2=new OpenPmsDental.UI.SplitterPanel();
				splitContainer.SuspendLayout();
				splitContainer.Controls.Add(splitterPanel1);
				splitContainer.Controls.Add(splitterPanel2);
				splitContainer.Orientation=isVertical ? Orientation.Vertical : Orientation.Horizontal;
				splitContainer.Panel1=splitterPanel1;
				splitContainer.Panel2=splitterPanel2;
				splitContainer.Bounds=new Rectangle(10,10,UiTheme.ScaleI(120,scale),UiTheme.ScaleI(60,scale));
				panel.Controls.Add(splitContainer);
				splitContainer.ResumeLayout(false);
				splitContainer.SplitterDistance=UiTheme.ScaleI(isVertical ? 56 : 28,scale);
				IntPtr handle=splitContainer.Handle;
				splitContainer.PerformLayout();
				SplitPicture splitPicture=new SplitPicture();
				splitPicture.IsVertical=isVertical;
				splitPicture.RectanglePanel1=splitterPanel1.Bounds;
				splitPicture.RectanglePanel2=splitterPanel2.Bounds;
				splitPicture.RectangleSplitter=splitContainer.GetRectangleSplitter();
				if(isPressed) {
					splitContainer.Press(new Point(splitPicture.RectangleSplitter.X+1,splitPicture.RectangleSplitter.Y+1));
				}
				splitPicture.Bitmap=CaptureArgb(splitContainer);
				return splitPicture;
			}
		}
		#endregion WinForms

		#region WPF
		///<summary>The WPF TabControl (300 x 100) with the three tabs, the first selected, in the state. The headers are TextBlocks, hidden for the
		///picture without text, so the tabs keep their size. Hover is set on the header's border as the template's IsMouseOver trigger sets it
		///(IsMouseOver cannot be set; TabSplitStateTests checks the trigger); Focused lays the TabPage's focus visual style over the second tab, as
		///WPF's adorner draws it.</summary>
		public static TabPicture RenderWpfTabs(TabPictureState tabPictureState,bool withText,float scale) {
			WpfControls.UI.TabControl tabControl=new WpfControls.UI.TabControl();
			tabControl.Width=300;
			tabControl.Height=100;
			List<WpfControls.UI.TabPage> listTabPages=new List<WpfControls.UI.TabPage>();
			foreach(string text in ArrayTabTexts) {
				WpfControls.UI.TabPage tabPage=new WpfControls.UI.TabPage();
				System.Windows.Controls.TextBlock textBlock=new System.Windows.Controls.TextBlock();
				textBlock.Text=text;
				textBlock.Visibility=withText ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
				tabPage.Header=textBlock;
				tabPage.Content=new System.Windows.Controls.Grid();
				tabControl.Items.Add(tabPage);
				listTabPages.Add(tabPage);
			}
			tabControl.SelectedIndex=0;
			if(tabPictureState==TabPictureState.Disabled) {
				listTabPages[2].IsEnabled=false;
			}
			System.Windows.Controls.Grid grid=new System.Windows.Controls.Grid();
			grid.Width=300;
			grid.Height=100;
			grid.Children.Add(tabControl);
			grid.Measure(new System.Windows.Size(300,100));
			grid.Arrange(new System.Windows.Rect(0,0,300,100));
			grid.UpdateLayout();
			if(tabPictureState==TabPictureState.Hover) {
				System.Windows.Controls.Border border=(System.Windows.Controls.Border)listTabPages[2].Template.FindName("mainBorder",listTabPages[2]);
				border.Background=WpfControls.UI.TabControl.BrushSubtleHover;
			}
			TabPicture tabPicture=new TabPicture();
			tabPicture.TabSide=EnumUiTabSide.Top;
			tabPicture.IdxSelected=0;
			foreach(WpfControls.UI.TabPage tabPage in listTabPages) {
				tabPicture.ListHeaders.Add(GetRectangle(tabPage,tabControl,scale));
			}
			System.Windows.FrameworkElement frameworkElementHeaderPanel=(System.Windows.FrameworkElement)tabControl.Template.FindName("headerPanel",tabControl);
			Rectangle rectangleHeaderPanel=GetRectangle(frameworkElementHeaderPanel,tabControl,scale);
			int widthPixels=UiTheme.ScaleI(300,scale);
			int heightPixels=(int)Math.Ceiling(100*scale-0.001);
			tabPicture.RectangleStrip=new Rectangle(0,0,widthPixels,rectangleHeaderPanel.Bottom);
			tabPicture.RectanglePage=Rectangle.FromLTRB(0,rectangleHeaderPanel.Bottom,widthPixels,heightPixels);
			if(tabPictureState==TabPictureState.Focused) {
				Rectangle rectangleFocus=tabPicture.ListHeaders[1];
				System.Windows.Controls.Control controlFocus=new System.Windows.Controls.Control();
				controlFocus.Style=listTabPages[1].FocusVisualStyle;
				controlFocus.HorizontalAlignment=System.Windows.HorizontalAlignment.Left;
				controlFocus.VerticalAlignment=System.Windows.VerticalAlignment.Top;
				controlFocus.Margin=new System.Windows.Thickness(rectangleFocus.X/scale,rectangleFocus.Y/scale,0,0);
				controlFocus.Width=rectangleFocus.Width/scale;
				controlFocus.Height=rectangleFocus.Height/scale;
				grid.Children.Add(controlFocus);
				grid.UpdateLayout();
			}
			tabPicture.Bitmap=ControlPictures.RenderWpf(grid,300,100,scale);
			return tabPicture;
		}

		///<summary>The WPF SplitContainer (120 x 61, so the panels get whole pixels at 100%) with its rows (a panel, the GridSplitter 5 high with Background Silver as today's XAML, a panel), at
		///rest or dragging (GridSplitter.IsDragging, set through its key).</summary>
		public static SplitPicture RenderWpfSplitter(bool isDragging,float scale) {
			WpfControls.UI.SplitContainer splitContainer=new WpfControls.UI.SplitContainer();
			splitContainer.Width=120;
			splitContainer.Height=61;
			splitContainer.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
			splitContainer.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height=System.Windows.GridLength.Auto });
			splitContainer.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
			WpfControls.UI.Panel panel1=new WpfControls.UI.Panel();
			panel1.HorizontalAlignment=System.Windows.HorizontalAlignment.Stretch;
			panel1.VerticalAlignment=System.Windows.VerticalAlignment.Stretch;
			System.Windows.Controls.GridSplitter gridSplitter=new System.Windows.Controls.GridSplitter();
			gridSplitter.Height=5;
			gridSplitter.HorizontalAlignment=System.Windows.HorizontalAlignment.Stretch;
			gridSplitter.Background=System.Windows.Media.Brushes.Silver;
			System.Windows.Controls.Grid.SetRow(gridSplitter,1);
			WpfControls.UI.Panel panel2=new WpfControls.UI.Panel();
			panel2.HorizontalAlignment=System.Windows.HorizontalAlignment.Stretch;
			panel2.VerticalAlignment=System.Windows.VerticalAlignment.Stretch;
			System.Windows.Controls.Grid.SetRow(panel2,2);
			splitContainer.Children.Add(panel1);
			splitContainer.Children.Add(gridSplitter);
			splitContainer.Children.Add(panel2);
			splitContainer.Measure(new System.Windows.Size(120,61));
			splitContainer.Arrange(new System.Windows.Rect(0,0,120,61));
			splitContainer.UpdateLayout();
			if(isDragging) {
				FieldInfo fieldInfoKey=typeof(System.Windows.Controls.Primitives.Thumb).GetField("IsDraggingPropertyKey",BindingFlags.Static|BindingFlags.NonPublic);
				gridSplitter.SetValue((System.Windows.DependencyPropertyKey)fieldInfoKey.GetValue(null),true);
				splitContainer.UpdateLayout();
			}
			SplitPicture splitPicture=new SplitPicture();
			splitPicture.IsVertical=false;
			splitPicture.RectanglePanel1=GetRectangle(panel1,splitContainer,scale);
			splitPicture.RectanglePanel2=GetRectangle(panel2,splitContainer,scale);
			splitPicture.RectangleSplitter=GetRectangle(gridSplitter,splitContainer,scale);
			splitPicture.Bitmap=ControlPictures.RenderWpf(splitContainer,120,61,scale);
			return splitPicture;
		}

		public static Rectangle GetRectangle(System.Windows.FrameworkElement frameworkElement,System.Windows.Media.Visual visualAncestor,float scale) {
			System.Windows.Point point=frameworkElement.TranslatePoint(new System.Windows.Point(0,0),(System.Windows.UIElement)visualAncestor);
			return UiTheme.SnapRectangle(new RectangleF((float)point.X*scale,(float)point.Y*scale,(float)frameworkElement.ActualWidth*scale,
				(float)frameworkElement.ActualHeight*scale));
		}
		#endregion WPF
	}

	///<summary>The design sheet's drawing of the tabs and splitters at the controls' geometry: DesignSheetControls.DrawTabHeader, DrawTabsComposite and
	///DrawSplitter's shapes and tokens, without the text.</summary>
	public static class TabSplitDrawings {
		private static Bitmap Start(Size size,Color colorBack,float scale,out Graphics g,out SheetPainter p) {
			Bitmap bitmap=new Bitmap(size.Width,size.Height,PixelFormat.Format32bppArgb);
			g=Graphics.FromImage(bitmap);
			g.Clear(colorBack);
			p=new SheetPainter(g,scale,SheetPainter.Light);
			return bitmap;
		}

		///<summary>The tab whose look the state changes: the third for hover and disabled, the second for its own colour and focus.</summary>
		public static int GetIdxState(TabPictureState tabPictureState) {
			return tabPictureState==TabPictureState.ColorTab || tabPictureState==TabPictureState.Focused ? 1 : 2;
		}

		///<summary>The token of a tab's text in the state: Text on the selected tab and on a page colour, TextDisabled on the disabled one,
		///TextSecondary otherwise.</summary>
		public static Color TabTextColor(TabPicture tabPicture,TabPictureState tabPictureState,int idx) {
			if(idx==tabPicture.IdxSelected) {
				return SheetPainter.Light(EnumUiColor.Text);
			}
			if(idx==GetIdxState(tabPictureState) && tabPictureState==TabPictureState.Disabled) {
				return SheetPainter.Light(EnumUiColor.TextDisabled);
			}
			if(idx==GetIdxState(tabPictureState) && tabPictureState==TabPictureState.ColorTab) {
				return SheetPainter.Light(EnumUiColor.Text);
			}
			return SheetPainter.Light(EnumUiColor.TextSecondary);
		}

		///<summary>Tabs (section 3.6): the strip SurfaceSubtle, the page Surface with its 1 px Stroke border, the selected tab's 2 px Accent line along
		///the edge facing the page, SubtleHover (4 px corners) on the tab under the mouse, the page colour on its tab, the focus ring on the focused
		///one. colorBack: the picture's background (the control's parent).</summary>
		public static Bitmap Tabs(TabPicture tabPicture,TabPictureState tabPictureState,float scale,Color colorBack) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(tabPicture.Bitmap.Size,colorBack,scale,out g,out p);
			using(g)
			using(p) {
				p.Fill(EnumUiColor.SurfaceSubtle,tabPicture.RectangleStrip,0);
				p.Fill(EnumUiColor.Surface,tabPicture.RectanglePage,0);
				p.Outline(EnumUiColor.Stroke,tabPicture.RectanglePage,p.LineWidth,0);
				int idxState=GetIdxState(tabPictureState);
				for(int i=0;i<tabPicture.ListHeaders.Count;i++) {
					Rectangle rectangle=tabPicture.ListHeaders[i];
					if(i==idxState && tabPictureState==TabPictureState.Hover) {
						p.Fill(EnumUiColor.SubtleHover,rectangle,UiTheme.CORNER_CONTROL);
					}
					if(i==idxState && tabPictureState==TabPictureState.ColorTab) {
						p.Fill(TabSplitControls.ColorTabCustom,rectangle,UiTheme.CORNER_CONTROL);
					}
					if(i==tabPicture.IdxSelected) {
						int width=p.S(UiTheme.LINE_TAB_SELECTED);
						Rectangle rectangleLine;
						switch(tabPicture.TabSide) {
							case EnumUiTabSide.Bottom:
								rectangleLine=new Rectangle(rectangle.X,rectangle.Y,rectangle.Width,width);
								break;
							case EnumUiTabSide.Left:
								rectangleLine=new Rectangle(rectangle.Right-width,rectangle.Y,width,rectangle.Height);
								break;
							default:
								rectangleLine=new Rectangle(rectangle.X,rectangle.Bottom-width,rectangle.Width,width);
								break;
						}
						p.Fill(EnumUiColor.Accent,rectangleLine,0);
					}
					if(i==idxState && tabPictureState==TabPictureState.Focused) {
						p.FocusRing(rectangle,UiTheme.CORNER_CONTROL);
					}
				}
			}
			return bitmap;
		}

		///<summary>The WinForms split container (section 3.6): Surface around the panels, the panels' outer edges in 1 px Stroke, the splitter's 1 px line
		///along its middle, Gridline at rest and Accent while dragging. The panels show their parent's Surface.</summary>
		public static Bitmap WinFormsSplitter(SplitPicture splitPicture,bool isPressed,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(splitPicture.Bitmap.Size,SheetPainter.Light(EnumUiColor.Surface),scale,out g,out p);
			using(g)
			using(p) {
				int w=p.LineWidth;
				Rectangle r1=splitPicture.RectanglePanel1;
				Rectangle r2=splitPicture.RectanglePanel2;
				if(splitPicture.IsVertical) {
					Edges(p,r1,top:true,bottom:true,left:true,right:false,w);
					Edges(p,r2,top:true,bottom:true,left:false,right:true,w);
				}
				else {
					Edges(p,r1,top:true,bottom:false,left:true,right:true,w);
					Edges(p,r2,top:false,bottom:true,left:true,right:true,w);
				}
				Splitter(p,splitPicture,isPressed);
			}
			return bitmap;
		}

		///<summary>The WPF split container: the panels white (their own background), the splitter Surface with the line along its middle.</summary>
		public static Bitmap WpfSplitter(SplitPicture splitPicture,bool isDragging,float scale) {
			Graphics g;
			SheetPainter p;
			Bitmap bitmap=Start(splitPicture.Bitmap.Size,Color.White,scale,out g,out p);
			using(g)
			using(p) {
				Splitter(p,splitPicture,isDragging);
			}
			return bitmap;
		}

		private static void Splitter(SheetPainter p,SplitPicture splitPicture,bool isDragging) {
			Rectangle rectangle=splitPicture.RectangleSplitter;
			p.Fill(EnumUiColor.Surface,rectangle,0);
			EnumUiColor uiColorLine=isDragging ? EnumUiColor.Accent : EnumUiColor.Gridline;
			if(splitPicture.IsVertical) {
				int x=rectangle.X+(rectangle.Width-p.LineWidth)/2;
				p.Line(uiColorLine,x,rectangle.Y,x,rectangle.Bottom,p.LineWidth);
			}
			else {
				int y=rectangle.Y+(rectangle.Height-p.LineWidth)/2;
				p.Line(uiColorLine,rectangle.X,y,rectangle.Right,y,p.LineWidth);
			}
		}

		private static void Edges(SheetPainter p,Rectangle rectangle,bool top,bool bottom,bool left,bool right,int w) {
			if(top) {
				p.Fill(EnumUiColor.Stroke,new Rectangle(rectangle.X,rectangle.Y,rectangle.Width,w),0);
			}
			if(bottom) {
				p.Fill(EnumUiColor.Stroke,new Rectangle(rectangle.X,rectangle.Bottom-w,rectangle.Width,w),0);
			}
			if(left) {
				p.Fill(EnumUiColor.Stroke,new Rectangle(rectangle.X,rectangle.Y,w,rectangle.Height),0);
			}
			if(right) {
				p.Fill(EnumUiColor.Stroke,new Rectangle(rectangle.Right-w,rectangle.Y,w,rectangle.Height),0);
			}
		}
	}
}
