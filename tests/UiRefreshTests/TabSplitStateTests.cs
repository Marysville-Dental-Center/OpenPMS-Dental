// The control state pictures of package UI-4b-2 (docs/specs/ui-refresh-plan.md, section 7's UI-4b-2 line and section 3.6; docs/port-notes/UI-4b.md):
// the WinForms UI.TabControl (rest, hover, a disabled tab, a tab with its own page colour; tabs on top, at the bottom and on the left), the WPF
// TabControl (rest, hover, keyboard focus, a disabled tab), the WinForms UI.SplitContainer (vertical and horizontal, at rest and dragging) and the
// WPF SplitContainer's GridSplitter (at rest and dragging), each drawn off screen and compared with the design sheet's drawing of the state at the
// control's own geometry (TabSplitDrawings) by ChartImageCompare's tolerance, as UI-4a and UI-4b-1 do: WinForms every pixel, WPF the solid pixels.
// Then with text: each tab's text pixels reach its token. And the WPF template's own triggers give the hover and the focus ring the tests set by
// hand. No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;

namespace UiRefreshTests {
	[TestClass]
	public class TabSplitStateTests {
		#region WinForms
		[TestMethod]
		public void WinForms_TabControl_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(TabPictureState tabPictureState in TabSplitControls.ArrayWinFormsStates) {
					CompareWinFormsTabs(listFailures,tabPictureState,EnumTabAlignment.Top);
				}
				CompareWinFormsTabs(listFailures,TabPictureState.Rest,EnumTabAlignment.Bottom);
				CompareWinFormsTabs(listFailures,TabPictureState.Hover,EnumTabAlignment.LeftHorizontal);
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		private static void CompareWinFormsTabs(List<string> listFailures,TabPictureState tabPictureState,EnumTabAlignment tabAlignment) {
			string name="WinForms TabControl "+tabAlignment+" "+tabPictureState;
			using(TabPicture tabPictureChrome=TabSplitControls.RenderWinFormsTabs(tabPictureState,tabAlignment,false,1f))
			using(TabPicture tabPictureText=TabSplitControls.RenderWinFormsTabs(tabPictureState,tabAlignment,true,1f))
			using(Bitmap bitmapExpected=TabSplitDrawings.Tabs(tabPictureChrome,tabPictureState,1f,SheetPainter.Light(EnumUiColor.Surface))) {
				ControlStateTests.CompareAll(listFailures,name,bitmapExpected,tabPictureChrome.Bitmap);
				CheckTabTexts(listFailures,name,tabPictureChrome,tabPictureText,tabPictureState);
			}
		}

		[TestMethod]
		public void WinForms_SplitContainer_RestAndDragging_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(bool isVertical in new[] { true,false }) {
					foreach(bool isPressed in new[] { false,true }) {
						string name="WinForms SplitContainer "+(isVertical ? "vertical" : "horizontal")+(isPressed ? " dragging" : " rest");
						using(SplitPicture splitPicture=TabSplitControls.RenderWinFormsSplitter(isVertical,isPressed,1f))
						using(Bitmap bitmapExpected=TabSplitDrawings.WinFormsSplitter(splitPicture,isPressed,1f)) {
							if(splitPicture.RectangleSplitter.IsEmpty) {
								listFailures.Add(name+": no splitter between the panels.");
								continue;
							}
							ControlStateTests.CompareAll(listFailures,name,bitmapExpected,splitPicture.Bitmap);
						}
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_SplitContainer_ColoursAWindowSets_StillWin() {
			IconRendererTests.RunSta(() => {
				using(OpenPmsDental.UI.SplitContainer splitContainer=new OpenPmsDental.UI.SplitContainer()) {
					Assert.AreEqual(SheetPainter.Light(EnumUiColor.Surface).ToArgb(),splitContainer.GetColorSplitterBack().ToArgb(),"The default Silver gives Surface.");
					Assert.AreEqual(SheetPainter.Light(EnumUiColor.Stroke).ToArgb(),splitContainer.GetColorBorderLine().ToArgb(),"The default Silver gives Stroke.");
					Color colorWindow=Color.FromArgb(252,253,254);
					splitContainer.ColorSplitter=colorWindow;
					splitContainer.ColorBorder=colorWindow;
					Assert.AreEqual(colorWindow.ToArgb(),splitContainer.GetColorSplitterBack().ToArgb(),"FormTaskEdit's own splitter colour wins.");
					Assert.AreEqual(colorWindow.ToArgb(),splitContainer.GetColorBorderLine().ToArgb(),"FormTaskEdit's own border colour wins.");
				}
			});
		}
		#endregion WinForms

		#region WPF
		[TestMethod]
		public void Wpf_TabControl_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(TabPictureState tabPictureState in TabSplitControls.ArrayWpfStates) {
					string name="WPF TabControl "+tabPictureState;
					using(TabPicture tabPictureChrome=TabSplitControls.RenderWpfTabs(tabPictureState,false,1f))
					using(TabPicture tabPictureText=TabSplitControls.RenderWpfTabs(tabPictureState,true,1f))
					using(Bitmap bitmapExpected=TabSplitDrawings.Tabs(tabPictureChrome,tabPictureState,1f,Color.White)) {
						ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,tabPictureChrome.Bitmap);
						CheckTabTexts(listFailures,name,tabPictureChrome,tabPictureText,tabPictureState);
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_TabPage_TemplateGivesTheHoverAndTheFocusRing() {
			ControlStateTests.RunWpf(() => {
				WpfControls.UI.TabControl tabControl=new WpfControls.UI.TabControl();
				WpfControls.UI.TabPage tabPage=new WpfControls.UI.TabPage();
				tabPage.Text="General";
				tabControl.Items.Add(tabPage);
				tabControl.Measure(new System.Windows.Size(200,80));
				tabControl.Arrange(new System.Windows.Rect(0,0,200,80));
				tabControl.UpdateLayout();
				//Hover: the template's IsMouseOver trigger sets the header's border to SubtleHover.
				System.Windows.Setter setterHover=tabPage.Template.Triggers.OfType<System.Windows.Trigger>()
					.Where(x => x.Property==System.Windows.UIElement.IsMouseOverProperty)
					.SelectMany(x => x.Setters.OfType<System.Windows.Setter>()).SingleOrDefault(x => x.TargetName=="mainBorder");
				Assert.IsNotNull(setterHover,"The TabPage template has no IsMouseOver trigger on its border.");
				Assert.AreEqual(SheetPainter.Light(EnumUiColor.SubtleHover).ToArgb(),ToDrawing(((System.Windows.Media.SolidColorBrush)setterHover.Value).Color).ToArgb());
				//Focus: the shared ring of UiLook (2 px Accent, 4 px corners).
				Assert.IsNotNull(tabPage.FocusVisualStyle,"The TabPage has no focus visual style.");
				Assert.AreSame(tabPage.FindResource(WpfControls.UI.UiLook.KeyFocusVisualStyle),tabPage.FocusVisualStyle);
			});
		}

		[TestMethod]
		public void Wpf_SplitContainer_RestAndDragging_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(bool isDragging in new[] { false,true }) {
					string name="WPF SplitContainer"+(isDragging ? " dragging" : " rest");
					using(SplitPicture splitPicture=TabSplitControls.RenderWpfSplitter(isDragging,1f))
					using(Bitmap bitmapExpected=TabSplitDrawings.WpfSplitter(splitPicture,isDragging,1f)) {
						ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,splitPicture.Bitmap);
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}
		#endregion WPF

		#region Helpers
		///<summary>Each tab's text pixels (where the picture with text differs from the one without, inside the tab) include pixels of its token.</summary>
		private static void CheckTabTexts(List<string> listFailures,string name,TabPicture tabPictureChrome,TabPicture tabPictureText,TabPictureState tabPictureState) {
			Bitmap bitmapChrome=tabPictureChrome.Bitmap;
			Bitmap bitmapText=tabPictureText.Bitmap;
			for(int i=0;i<tabPictureChrome.ListHeaders.Count;i++) {
				Rectangle rectangle=tabPictureChrome.ListHeaders[i];
				rectangle.Intersect(new Rectangle(Point.Empty,bitmapChrome.Size));
				using(Bitmap bitmapChromeTab=bitmapChrome.Clone(rectangle,PixelFormat.Format32bppArgb))
				using(Bitmap bitmapTextTab=bitmapText.Clone(rectangle,PixelFormat.Format32bppArgb)) {
					ControlStateTests.CheckText(listFailures,name+" tab "+TabSplitControls.ArrayTabTexts[i],bitmapChromeTab,bitmapTextTab,
						TabSplitDrawings.TabTextColor(tabPictureChrome,tabPictureState,i));
				}
			}
		}

		private static Color ToDrawing(System.Windows.Media.Color color) {
			return Color.FromArgb(color.A,color.R,color.G,color.B);
		}
		#endregion Helpers
	}
}
