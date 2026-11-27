// Gallery G-UI1 part 2f for package UI-4c-2 (docs/specs/ui-refresh-plan.md section 8; docs/port-notes/UI-4c.md): the real progress bars, colour
// picker, database integrity warnings, picture box and signature box frame with their state strips, WinForms above WPF, at 100, 125, 150, 175 and
// 200% (WinForms through LayoutManager's scale, WPF through its render scale), off screen. Opt-in: OPMS_UI_DIALOG_GALLERY names the folder;
// tools/New-UiGallery.ps1 then makes the page from it (the port note has the commands).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UiRefreshTests {
	[TestClass]
	public class DialogGallery {
		[TestMethod]
		public void Gallery_DialogStrips_WinFormsAboveWpf() {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_DIALOG_GALLERY");
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: OPMS_UI_DIALOG_GALLERY names the folder for the G-UI1 part 2f state strips.");
			}
			bool rendersWpf=false;
			int countStrips=0;
			IconRendererTests.RunSta(() => {
				rendersWpf=ControlPictures.RendersWpf();
				foreach(float scale in ControlStateGallery.ArrayScales) {
					string folderScale=Path.Combine(folder,"scale-"+(int)Math.Round(scale*100));
					Directory.CreateDirectory(folderScale);
					//Progress bars: WinForms OdProgressBar rest (62%), empty, full, disabled, with its target line; WPF ProgressBarFlat rest, empty, full,
					//disabled, indeterminate (the marquee's segment at the left end).
					List<Bitmap> listProgressWinForms=DialogControls.ArrayProgressStatesWinForms.Select(x => DialogControls.RenderWinFormsProgress(x,scale)).ToList();
					List<Bitmap> listProgressWpf=rendersWpf ? DialogControls.ArrayProgressStatesWpf.Select(x => DialogControls.RenderWpfProgress(x,scale)).ToList()
						: new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"progressbars.png"),listProgressWinForms,listProgressWpf,scale);
					countStrips++;
					//The colour picker (WinForms only): rest, hover, focus, disabled, None.
					List<Bitmap> listPickers=DialogControls.ArraySwatchStates.Select(x => DialogControls.RenderWinFormsColorPicker(x,scale,out Rectangle _)).ToList();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"colorpicker.png"),listPickers,new List<Bitmap>(),scale);
					countStrips++;
					if(scale!=1f) {
						continue;//the picture box and the signature box draw at the screen's DPI, not LayoutManager's scale: once at 100%
					}
					//The picture box: its hint with the border, a picture with the border, a picture without it.
					List<Bitmap> listPictureBoxes=new List<Bitmap> {
						DialogControls.RenderWinFormsPictureBox(hasBorder:true,withText:true),
						DialogControls.RenderWinFormsPictureBox(hasBorder:true,withText:false,image:DialogControls.MakeSyntheticPicture()),
						DialogControls.RenderWinFormsPictureBox(hasBorder:false,withText:false,image:DialogControls.MakeSyntheticPicture()),
					};
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"picturebox.png"),listPictureBoxes,new List<Bitmap>(),scale);
					countStrips++;
					//The signature box that the WPF windows host, with a synthetic signature: the new frame, and office content's (the eForm) unchanged one.
					List<Bitmap> listSignatures=new List<Bitmap>();
					foreach(bool isPinned in new[] { false,true }) {
						listSignatures.Add(DialogControls.RenderSignatureBox(isPinned,out bool _,out Bitmap bitmapSignature));
						bitmapSignature.Dispose();
					}
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"signaturebox.png"),listSignatures,new List<Bitmap>(),scale);
					countStrips++;
				}
			});
			Assert.IsTrue(countStrips>0);
			if(!rendersWpf) {
				Assert.Inconclusive("Wrote the WinForms rows only: WPF draws nothing in this session.");
			}
		}
	}
}
