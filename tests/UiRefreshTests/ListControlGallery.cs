// Gallery G-UI1 part 2b for package UI-4b-1 (docs/specs/ui-refresh-plan.md section 8; docs/port-notes/UI-4b.md): the real combo boxes, pickers and
// lists with their state strips, WinForms above WPF, at 100, 125, 150, 175 and 200% (WinForms through LayoutManager's scale, WPF through its render
// scale), off screen. Opt-in: OPMS_UI_LISTS_GALLERY names the folder; tools/New-UiGallery.ps1 then makes the page from it (the port note has the
// commands).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UiRefreshTests {
	[TestClass]
	public class ListControlGallery {
		[TestMethod]
		public void Gallery_ListStateStrips_WinFormsBesideWpf() {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_LISTS_GALLERY");
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: OPMS_UI_LISTS_GALLERY names the folder for the G-UI1 part 2b state strips.");
			}
			bool rendersWpf=false;
			int countStrips=0;
			IconRendererTests.RunSta(() => {
				rendersWpf=ControlPictures.RendersWpf();
				foreach(float scale in ControlStateGallery.ArrayScales) {
					string folderScale=Path.Combine(folder,"scale-"+(int)Math.Round(scale*100));
					Directory.CreateDirectory(folderScale);
					//The combo box: rest, hover, focus (the line inside the bottom edge), disabled.
					List<Bitmap> listComboWinForms=ListStates.ArrayComboStates.Select(x => ListStateControls.RenderWinFormsComboBox(x,"Hygienist",scale)).ToList();
					List<Bitmap> listComboWpf=rendersWpf ? ListStates.ArrayComboStates.Select(x => ListStateControls.RenderWpfComboBox(x,"Hygienist",scale)).ToList()
						: new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"combobox.png"),listComboWinForms,listComboWpf,scale);
					countStrips++;
					//The clinic pickers: the label and the box (WinForms ComboBoxClinicPicker, WPF ComboClinic).
					Rectangle rectangleBox;
					List<Bitmap> listClinicWinForms=ListStates.ArrayComboStates.Select(x => ListStateControls.RenderWinFormsClinicPicker(x,"Clinic",scale,out rectangleBox)).ToList();
					List<Bitmap> listClinicWpf=rendersWpf ? new[] { SheetState.Normal,SheetState.Hover,SheetState.Disabled }
						.Select(x => ListStateControls.RenderWpfComboClinic(x,"Clinic",scale,out rectangleBox)).ToList() : new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"clinic-picker.png"),listClinicWinForms,listClinicWpf,scale);
					countStrips++;
					//The list box: rest, hover, focus (the focused row's outline), disabled, selected with focus, selected without (the first row).
					List<Bitmap> listListWinForms=ListStates.ArrayListStates.Select(x => TakeBitmap(ListStateControls.RenderWinFormsListBox(x,true,scale))).ToList();
					List<Bitmap> listListWpf=rendersWpf ? ListStates.ArrayWpfListStates.Select(x => TakeBitmap(ListStateControls.RenderWpfListBox(x,true,scale))).ToList()
						: new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"listbox.png"),listListWinForms,listListWpf,scale);
					countStrips++;
					//The combo boxes' drop-down lists: rest, hover, selected with focus (as while it is open), selected without.
					List<Bitmap> listDropWinForms=ListStates.ArrayDropDownStates.Select(x => TakeBitmap(ListStateControls.RenderWinFormsDropDown(x,true,scale))).ToList();
					List<Bitmap> listDropWpf=rendersWpf ? ListStates.ArrayDropDownStates.Select(x => TakeBitmap(ListStateControls.RenderWpfDropDown(x,true,scale))).ToList()
						: new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"dropdown.png"),listDropWinForms,listDropWpf,scale);
					countStrips++;
					if(!rendersWpf) {
						continue;
					}
					//WPF only: the thumbnail ListView in the first row, the TreeView in the second.
					List<Bitmap> listListView=ListStates.ArrayWpfListStates.Select(x => TakeBitmap(ListStateControls.RenderWpfListView(x,true,scale))).ToList();
					List<Bitmap> listTreeView=ListStates.ArrayWpfListStates.Select(x => TakeBitmap(ListStateControls.RenderWpfTreeView(x,true,scale))).ToList();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"listview-treeview-wpf.png"),listListView,listTreeView,scale);
					countStrips++;
				}
			});
			Assert.IsTrue(countStrips>0);
			if(!rendersWpf) {
				Assert.Inconclusive("Wrote the WinForms rows only: WPF draws nothing in this session.");
			}
		}

		///<summary>The picture of a list picture; the caller owns it.</summary>
		private static Bitmap TakeBitmap(ListPicture listPicture) {
			Bitmap bitmap=listPicture.Bitmap;
			listPicture.Bitmap=null;
			return bitmap;
		}
	}
}
