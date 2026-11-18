// Gallery G-UI1 part 2c for package UI-4b-2 (docs/specs/ui-refresh-plan.md section 8; docs/port-notes/UI-4b.md): the real tabs and splitters with
// their state strips, WinForms above WPF, at 100, 125, 150, 175 and 200% (WinForms through LayoutManager's scale, WPF through its render scale), off
// screen. Opt-in: OPMS_UI_TABS_GALLERY names the folder; tools/New-UiGallery.ps1 then makes the page from it (the port note has the commands). The
// four windows' tab strips before and after (G-UI2, G-UI3) come from TabStripFitTests with OPMS_UI_TABS_WINDOWS.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;

namespace UiRefreshTests {
	[TestClass]
	public class TabSplitGallery {
		[TestMethod]
		public void Gallery_TabAndSplitterStrips_WinFormsBesideWpf() {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_TABS_GALLERY");
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: OPMS_UI_TABS_GALLERY names the folder for the G-UI1 part 2c state strips.");
			}
			bool rendersWpf=false;
			int countStrips=0;
			IconRendererTests.RunSta(() => {
				rendersWpf=ControlPictures.RendersWpf();
				foreach(float scale in ControlStateGallery.ArrayScales) {
					string folderScale=Path.Combine(folder,"scale-"+(int)Math.Round(scale*100));
					Directory.CreateDirectory(folderScale);
					//Tabs on top: rest (the first selected), hover (the third), a disabled tab (the third), a page colour (the second); WPF: rest, hover,
					//keyboard focus (the second), a disabled tab.
					List<Bitmap> listTabsWinForms=TabSplitControls.ArrayWinFormsStates.Select(x => TakeBitmap(TabSplitControls.RenderWinFormsTabs(x,EnumTabAlignment.Top,true,scale))).ToList();
					List<Bitmap> listTabsWpf=rendersWpf ? TabSplitControls.ArrayWpfStates.Select(x => TakeBitmap(TabSplitControls.RenderWpfTabs(x,true,scale))).ToList()
						: new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"tabs.png"),listTabsWinForms,listTabsWpf,scale);
					countStrips++;
					//The other WinForms tab places: at the bottom (the Chart module's images) and on the left (FormEmailInbox), rest and hover.
					List<Bitmap> listBottom=new[] { TabPictureState.Rest,TabPictureState.Hover }.Select(x => TakeBitmap(TabSplitControls.RenderWinFormsTabs(x,EnumTabAlignment.Bottom,true,scale))).ToList();
					List<Bitmap> listLeft=new[] { TabPictureState.Rest,TabPictureState.Hover }.Select(x => TakeBitmap(TabSplitControls.RenderWinFormsTabs(x,EnumTabAlignment.LeftHorizontal,true,scale))).ToList();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"tabs-bottom-left-winforms.png"),listBottom,listLeft,scale);
					countStrips++;
					//Splitters: WinForms vertical rest and dragging, horizontal rest and dragging; WPF rest and dragging.
					List<Bitmap> listSplitWinForms=new List<Bitmap>();
					foreach(bool isVertical in new[] { true,false }) {
						foreach(bool isPressed in new[] { false,true }) {
							listSplitWinForms.Add(TakeBitmap(TabSplitControls.RenderWinFormsSplitter(isVertical,isPressed,scale)));
						}
					}
					List<Bitmap> listSplitWpf=rendersWpf ? new[] { false,true }.Select(x => TakeBitmap(TabSplitControls.RenderWpfSplitter(x,scale))).ToList()
						: new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"splitters.png"),listSplitWinForms,listSplitWpf,scale);
					countStrips++;
				}
			});
			Assert.IsTrue(countStrips>0);
			if(!rendersWpf) {
				Assert.Inconclusive("Wrote the WinForms rows only: WPF draws nothing in this session.");
			}
		}

		private static Bitmap TakeBitmap(TabPicture tabPicture) {
			Bitmap bitmap=tabPicture.Bitmap;
			tabPicture.Bitmap=null;
			return bitmap;
		}

		private static Bitmap TakeBitmap(SplitPicture splitPicture) {
			Bitmap bitmap=splitPicture.Bitmap;
			splitPicture.Bitmap=null;
			return bitmap;
		}
	}
}
