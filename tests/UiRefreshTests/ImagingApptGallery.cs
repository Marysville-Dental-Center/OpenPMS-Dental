// Gallery G-UI1 part 2e for package UI-4c-1 (docs/specs/ui-refresh-plan.md section 8; docs/port-notes/UI-4c.md): the real imaging and appointment
// chrome with its state strips, WinForms above WPF, at 100, 125, 150, 175 and 200% (WinForms through LayoutManager's scale, WPF through its render
// scale), off screen. Opt-in: OPMS_UI_IMAGING_GALLERY names the folder; tools/New-UiGallery.ps1 then makes the page from it (the port note has the
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
	public class ImagingApptGallery {
		[TestMethod]
		public void Gallery_ImagingAndAppointmentChromeStrips_WinFormsAboveWpf() {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_IMAGING_GALLERY");
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: OPMS_UI_IMAGING_GALLERY names the folder for the G-UI1 part 2e state strips.");
			}
			bool rendersWpf=false;
			int countStrips=0;
			IconRendererTests.RunSta(() => {
				rendersWpf=ControlPictures.RendersWpf();
				List<Bitmap> listEmpty=new List<Bitmap>();
				foreach(float scale in ControlStateGallery.ArrayScales) {
					string folderScale=Path.Combine(folder,"scale-"+(int)Math.Round(scale*100));
					Directory.CreateDirectory(folderScale);
					//The appointment module's day and week toggle (Day selected, the mouse on Week, Week selected) and the pinboard (empty, with two
					//appointments, which keep the appointment book's look).
					List<Bitmap> listToggle=ImagingApptControls.ArrayToggleStates.Select(x => Take(ImagingApptControls.RenderToggle(x,true,scale))).ToList();
					List<Bitmap> listPinBoard=ImagingApptControls.ArrayPinBoardStates.Select(x => Take(ImagingApptControls.RenderPinBoard(x,true,scale))).ToList();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"toggle-pinboard-winforms.png"),listToggle,listPinBoard,scale);
					countStrips++;
					//The appointment edit window's provider slider: rest, the mouse on the slider, dragging it, the typing cursor.
					List<Bitmap> listProv=ImagingApptControls.ArrayProvSliderStates.Select(x => Take(ImagingApptControls.RenderProvSlider(x,true,scale))).ToList();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"provslider-winforms.png"),listProv,listEmpty,scale);
					countStrips++;
					//The unmounted bar: WinForms above WPF.
					List<Bitmap> listUnmountedWpf=rendersWpf ? new List<Bitmap> { Take(ImagingApptControls.RenderWpfUnmountedBar(true,scale)) } : new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"unmountedbar.png"),new List<Bitmap> { Take(ImagingApptControls.RenderUnmountedBar(true,scale)) },
						listUnmountedWpf,scale);
					countStrips++;
					//The windowing slider at its real width (154): rest, the left handle under the mouse, the range under the mouse, dragging the right
					//handle, disabled.
					List<Bitmap> listWindowing=ImagingApptControls.ArrayWindowingStates.Select(x => Take(ImagingApptControls.RenderWindowingSlider(x,scale,154))).ToList();
					List<Bitmap> listWindowingWpf=rendersWpf ? ImagingApptControls.ArrayWindowingStates.Select(x => Take(ImagingApptControls.RenderWpfWindowingSlider(x,scale,154))).ToList()
						: new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"windowingslider.png"),listWindowing,listWindowingWpf,scale);
					countStrips++;
					//The zoom slider: rest, the minus under the mouse, the Fit cell pressed, the value box under the mouse, dragging it, disabled.
					List<Bitmap> listZoom=ImagingApptControls.ArrayZoomStates.Select(x => Take(ImagingApptControls.RenderZoomSlider(x,true,scale))).ToList();
					List<Bitmap> listZoomWpf=rendersWpf ? ImagingApptControls.ArrayZoomStates.Select(x => Take(ImagingApptControls.RenderWpfZoomSlider(x,true,scale))).ToList()
						: new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"zoomslider.png"),listZoom,listZoomWpf,scale);
					countStrips++;
					if(!rendersWpf) {
						continue;
					}
					//The image dock's header (WPF only): selected (rest, maximise and close under the mouse) above not selected (rest, minimise under the
					//mouse, the Windows menu open) and empty.
					List<Bitmap> listDockSelected=new[] { ChromeState.Rest,ChromeState.Hover,ChromeState.HoverClose }
						.Select(x => Take(ImagingApptControls.RenderWpfDockHeader(x,true,scale))).ToList();
					List<Bitmap> listDockUnselected=new[] { ChromeState.Unselected,ChromeState.UnselectedHover,ChromeState.WindowsOpen,ChromeState.Empty }
						.Select(x => Take(ImagingApptControls.RenderWpfDockHeader(x,true,scale))).ToList();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"dockheader-wpf.png"),listDockSelected,listDockUnselected,scale);
					countStrips++;
					//The image selector (an item selected; the same with the mouse on another item) and the floating windows' menu (a snap area under
					//the mouse).
					List<Bitmap> listSelector=new[] { -1,1 }.Select(x => ControlPictures.RenderWpf(ImagingApptControls.MakeImageSelector(x),227,200,scale)).ToList();
					System.Windows.Shapes.Path pathRest;
					System.Windows.Shapes.Path pathHover;
					listSelector.Add(ControlPictures.RenderWpf(ImagingApptControls.MakeFloatWindowsGrid(out pathRest,out pathHover),345,195,scale));
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"imageselector-floatwindows-wpf.png"),listEmpty,listSelector,scale);
					countStrips++;
				}
			});
			Assert.IsTrue(countStrips>0);
			if(!rendersWpf) {
				Assert.Inconclusive("Wrote the WinForms rows only: WPF draws nothing in this session.");
			}
		}

		private static Bitmap Take(ChromePicture chromePicture) {
			Bitmap bitmap=chromePicture.Bitmap;
			chromePicture.Bitmap=null;
			if(chromePicture.DictValues.TryGetValue("bitmaps",out object value) && value is List<Bitmap> listBitmaps) {
				listBitmaps.ForEach(x => x.Dispose());
			}
			return bitmap;
		}
	}
}
