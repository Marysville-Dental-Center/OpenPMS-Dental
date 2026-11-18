// The eForm fill surface keeps today's look of its text fields (package UI-4b-3, docs/port-notes/UI-4b.md; docs/specs/ui-refresh-plan.md sections 1
// and 7): CtrlEFormFill builds WpfControls.UI.TextBox (plain, multi-line and underlined), TextVDate (plain and underlined) and TextRich fields, and
// sets ContentLook.IsPinned on its root, so those keep pinned copies of today's look while the rest of the program takes the new one. A synthetic
// set of the fields (no database) is laid out without a window with the pin set, rendered by WPF's software renderer and compared with the picture
// recorded on this branch's first commit, before UI-4b-3 changed any text control (Data/text-pinned-before.png), with the tolerance of
// ChartImageCompare. OPMS_UI_TEXT_PINNED_BEFORE_WRITE=1 records the picture again (only on the code before a look change).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToothChartImageTests;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class TextPinnedLookTests {
		public const string PATH_BEFORE=@"tests\UiRefreshTests\Data\text-pinned-before.png";
		public const int WIDTH=300;
		public const int HEIGHT=150;

		[TestMethod]
		public void EFormTextFields_Pinned_LookAsBefore() {
			string inconclusive=null;
			Bitmap bitmapActual=null;
			IconRendererTests.RunSta(() => {
				if(!ControlPictures.RendersWpf()) {
					inconclusive="WPF draws nothing in this session: the pinned text fields' picture waits for a session WPF renders in.";
					return;
				}
				bitmapActual=RenderFields(isPinned:true);
			});
			if(inconclusive!=null) {
				Assert.Inconclusive(inconclusive);
			}
			using(bitmapActual) {
				string pathBefore=Path.Combine(RepoPaths.Root(),PATH_BEFORE);
				if(Environment.GetEnvironmentVariable("OPMS_UI_TEXT_PINNED_BEFORE_WRITE")=="1") {
					PngWriter.Save(bitmapActual,pathBefore);
					Assert.Inconclusive("Recorded "+pathBefore+".");
				}
				Assert.IsTrue(File.Exists(pathBefore),pathBefore+" is missing.");
				using(Bitmap bitmapLoaded=new Bitmap(pathBefore))
				using(Bitmap bitmapBefore=new Bitmap(bitmapLoaded)) {
					ChartImageCompareResult result=ChartImageCompare.Compare(bitmapBefore,bitmapActual);
					if(!result.IsPass) {
						string folder=ControlPictures.GetOutFolder("text-pinned");
						ChartImageCompare.WriteFailureImages(folder,"text-pinned",bitmapBefore,bitmapActual);
						Assert.Fail("The pinned text fields changed: "+result+" Pictures in "+folder+".");
					}
				}
			}
		}

		///<summary>The eForm's text field kinds on a white canvas (WIDTH x HEIGHT DIP at 96 DPI): a TextBox with text, a disabled one, an underlined one,
		///a multi-line one; a TextVDate with a date, an underlined one and one showing its error mark; a TextRich. isPinned sets ContentLook.IsPinned
		///on the canvas as CtrlEFormFill does on its root. STA thread only.</summary>
		public static Bitmap RenderFields(bool isPinned) {
			System.Windows.Controls.Canvas canvas=new System.Windows.Controls.Canvas();
			canvas.Background=System.Windows.Media.Brushes.White;
			if(isPinned) {
				WpfControls.UI.ContentLook.SetIsPinned(canvas,true);
			}
			WpfControls.UI.TextBox textBox=new WpfControls.UI.TextBox { Width=120,Height=20,Text="Pinned 123" };
			Add(canvas,textBox,10,10);
			WpfControls.UI.TextBox textBoxDisabled=new WpfControls.UI.TextBox { Width=120,Height=20,Text="Disabled" };
			textBoxDisabled.IsEnabled=false;
			Add(canvas,textBoxDisabled,150,10);
			WpfControls.UI.TextBox textBoxUnderline=new WpfControls.UI.TextBox { Width=120,Height=20,Text="Underline" };
			textBoxUnderline.IsUnderline=true;
			Add(canvas,textBoxUnderline,10,40);
			WpfControls.UI.TextBox textBoxMulti=new WpfControls.UI.TextBox { Width=120,Height=36,IsMultiline=true,Text="Line one\r\nLine two" };
			Add(canvas,textBoxMulti,150,40);
			WpfControls.UI.TextVDate textVDate=new WpfControls.UI.TextVDate { Width=100,Height=20,Text="11/17/2026" };
			Add(canvas,textVDate,10,70);
			WpfControls.UI.TextVDate textVDateUnderline=new WpfControls.UI.TextVDate { Width=100,Height=20,Text="11/18/2026" };
			textVDateUnderline.IsUnderline=true;
			Add(canvas,textVDateUnderline,150,84);
			WpfControls.UI.TextVDate textVDateError=new WpfControls.UI.TextVDate { Width=100,Height=20,Text="13/45" };
			ListStateControls.CallPrivate(textVDateError,"SetError","Invalid date.");
			Add(canvas,textVDateError,10,96);
			//Spell check off before the text is set: it reads a preference (no database here); the quick paste notes it looks for come from an empty
			//cache.
			WpfControls.UI.TextRich textRich=new WpfControls.UI.TextRich { Width=120,Height=30,SpellCheckIsEnabled=false };
			OpenPmsDentalBusiness.QuickPasteCats.FillCacheFromTable(OpenPmsDentalBusiness.Crud.QuickPasteCatCrud.ListToTable(
				new System.Collections.Generic.List<OpenPmsDentalBusiness.QuickPasteCat>()));
			try {
				textRich.Text="Rich text";
			}
			finally {
				OpenPmsDentalBusiness.QuickPasteCats.ClearCache();
			}
			Add(canvas,textRich,150,112);
			canvas.Width=WIDTH;
			canvas.Height=HEIGHT;
			canvas.Measure(new System.Windows.Size(WIDTH,HEIGHT));
			canvas.Arrange(new System.Windows.Rect(0,0,WIDTH,HEIGHT));
			canvas.UpdateLayout();
			return ControlPictures.RenderWpf(canvas,WIDTH,HEIGHT,1f);
		}

		private static void Add(System.Windows.Controls.Canvas canvas,System.Windows.UIElement uIElement,double x,double y) {
			System.Windows.Controls.Canvas.SetLeft(uIElement,x);
			System.Windows.Controls.Canvas.SetTop(uIElement,y);
			canvas.Children.Add(uIElement);
		}
	}
}
