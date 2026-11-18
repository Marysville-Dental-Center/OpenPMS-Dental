// Gallery G-UI1 part 2d for package UI-4b-3 (docs/specs/ui-refresh-plan.md section 8; docs/port-notes/UI-4b.md): the real calendars, date pickers and
// text boxes with their state strips, WinForms above WPF, at 100, 125, 150, 175 and 200% (WinForms through LayoutManager's scale, WPF through its
// render scale), off screen. Opt-in: OPMS_UI_TEXT_GALLERY names the folder; tools/New-UiGallery.ps1 then makes the page from it (the port note has
// the commands).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UiRefreshTests {
	[TestClass]
	public class TextCalendarGallery {
		[TestMethod]
		public void Gallery_CalendarPickerAndTextStrips_WinFormsAboveWpf() {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_TEXT_GALLERY");
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: OPMS_UI_TEXT_GALLERY names the folder for the G-UI1 part 2d state strips.");
			}
			bool rendersWpf=false;
			int countStrips=0;
			IconRendererTests.RunSta(() => {
				rendersWpf=ControlPictures.RendersWpf();
				foreach(float scale in ControlStateGallery.ArrayScales) {
					string folderScale=Path.Combine(folder,"scale-"+(int)Math.Round(scale*100));
					Directory.CreateDirectory(folderScale);
					//Calendars: WinForms rest, hover (a day and the month-back button), focus, disabled, today selected; WPF rest, hover, disabled, today
					//selected.
					List<Bitmap> listCalendarsWinForms=TextCalendarControls.ArrayCalendarStatesWinForms.Select(x => TakeBitmap(TextCalendarControls.RenderWinFormsCalendar(x,true,scale))).ToList();
					List<Bitmap> listCalendarsWpf=rendersWpf ? TextCalendarControls.ArrayCalendarStatesWpf.Select(x => TakeBitmap(TextCalendarControls.RenderWpfCalendar(x,true,scale))).ToList()
						: new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"calendars.png"),listCalendarsWinForms,listCalendarsWpf,scale);
					countStrips++;
					//Date pickers: WinForms rest, hover (the button), disabled; WPF rest, hover, focus, disabled, error.
					List<Bitmap> listPickersWinForms=TextCalendarControls.ArrayPickerStatesWinForms.Select(x => TextCalendarControls.RenderWinFormsDatePicker(x,scale,out Rectangle _)).ToList();
					List<Bitmap> listPickersWpf=rendersWpf ? TextCalendarControls.ArrayPickerStatesWpf.Select(x => TextCalendarControls.RenderWpfDatePicker(x,true,scale)).ToList()
						: new List<Bitmap>();
					ControlStateGallery.SaveStrip(Path.Combine(folderScale,"datepickers.png"),listPickersWinForms,listPickersWpf,scale);
					countStrips++;
					if(!rendersWpf) {
						continue;
					}
					//The WPF text boxes (the WinForms ones are Windows text boxes and keep their look): two kinds per strip, a row each, their states left to
					//right (rest, hover, focus, disabled, then read only or the error).
					SaveTextStrip(Path.Combine(folderScale,"textbox-textrich-wpf.png"),"TextBox","TextRich",scale);
					SaveTextStrip(Path.Combine(folderScale,"textpassword-textvdate-wpf.png"),"TextPassword","TextVDate",scale);
					SaveTextStrip(Path.Combine(folderScale,"textvint-textvdouble-wpf.png"),"TextVInt","TextVDouble",scale);
					SaveTextStrip(Path.Combine(folderScale,"textvtime-wpf.png"),"TextVTime",null,scale);
					countStrips+=4;
				}
			});
			Assert.IsTrue(countStrips>0);
			if(!rendersWpf) {
				Assert.Inconclusive("Wrote the WinForms rows only: WPF draws nothing in this session.");
			}
		}

		private static void SaveTextStrip(string path,string kindTop,string kindBottom,float scale) {
			List<Bitmap> listTop=TextCalendarControls.GetTextStates(kindTop).Select(x => TextCalendarControls.RenderWpfText(kindTop,x,true,scale)).ToList();
			List<Bitmap> listBottom=kindBottom==null ? new List<Bitmap>()
				: TextCalendarControls.GetTextStates(kindBottom).Select(x => TextCalendarControls.RenderWpfText(kindBottom,x,true,scale)).ToList();
			ControlStateGallery.SaveStrip(path,listTop,listBottom,scale);
		}

		private static Bitmap TakeBitmap(CalendarPicture calendarPicture) {
			Bitmap bitmap=calendarPicture.Bitmap;
			calendarPicture.Bitmap=null;
			return bitmap;
		}
	}
}
