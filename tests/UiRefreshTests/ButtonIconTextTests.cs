// UI-12 slice 9 (docs/port-notes/UI-12.md, "Slice 9 (batch 4)"): the program's button with an icon and a text and the designer's default
// ImageAlign (MiddleCenter) draws its text beside the icon when the text fits, as the icon buttons with ImageAlign MiddleLeft do; before, it
// drew the icon alone at any width (FormMaintBoard's Add Item at 85 and 100 px). No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;

namespace UiRefreshTests {
	[TestClass]
	public class ButtonIconTextTests {
		[TestMethod]
		public void IsTextShownBesideIcon_WhenTheTextFits() {
			Assert.IsTrue(OpenPmsDental.UI.Button.IsTextShownBesideIcon(85,22,48f,6),"Add Item (48 px) beside a 22 px icon in 85 px.");
			Assert.IsTrue(OpenPmsDental.UI.Button.IsTextShownBesideIcon(76,22,48f,6),"Exactly the room: 76 - 22 - 6 = 48.");
			Assert.IsFalse(OpenPmsDental.UI.Button.IsTextShownBesideIcon(75,22,48f,6),"One pixel short: the icon alone.");
			Assert.IsFalse(OpenPmsDental.UI.Button.IsTextShownBesideIcon(24,22,20f,6),"An icon button 24 px wide keeps its icon alone.");
			Assert.IsFalse(OpenPmsDental.UI.Button.IsTextShownBesideIcon(85,22,0f,6),"No text.");
		}

		[TestMethod]
		public void IconButtonCentred_DrawsItsTextWhenItFits() {
			foreach(int width in new[] { 85,100 }) {
				using(Font font=new Font("Segoe UI",9f))
				using(OpenPmsDental.UI.Button button=new OpenPmsDental.UI.Button { Font=font,Icon=EnumIcons.Add,Text="Add Item",Size=new Size(width,24) })
				using(Bitmap bitmap=new Bitmap(width,24,PixelFormat.Format24bppRgb)) {
					Assert.AreEqual(ContentAlignment.MiddleCenter,button.ImageAlign,"The designer's default, as FormMaintBoard's Add Item has it.");
					button.DrawToBitmap(bitmap,new Rectangle(0,0,width,24));
					//The text is centred in the room right of the icon (22 px): its right half lies past the middle of that room, where the icon
					//alone, centred on the button, never reaches.
					int middle=(22+width)/2;
					Assert.IsTrue(HasDarker(bitmap,new Rectangle(middle+4,6,16,12),128),"Add Item at "+width+" px draws its text right of the icon.");
				}
			}
		}

		[TestMethod]
		public void IconButtonCentred_TooNarrow_DrawsTheIconAlone() {
			using(Font font=new Font("Segoe UI",9f))
			using(OpenPmsDental.UI.Button button=new OpenPmsDental.UI.Button { Font=font,Icon=EnumIcons.Add,Text="Add Item",Size=new Size(40,24) })
			using(Bitmap bitmap=new Bitmap(40,24,PixelFormat.Format24bppRgb)) {
				button.DrawToBitmap(bitmap,new Rectangle(0,0,40,24));
				//The icon alone, centred: 9 to 31 px. Nothing is drawn in the 6 px at each side inside the border.
				Assert.IsFalse(HasDarker(bitmap,new Rectangle(2,4,5,16),128),"Nothing left of the centred icon.");
				Assert.IsFalse(HasDarker(bitmap,new Rectangle(33,4,5,16),128),"No text right of the centred icon.");
			}
		}

		///<summary>True when a pixel in the rectangle is darker than the level on every channel (an antialiased glyph reaches its colour in part).</summary>
		private static bool HasDarker(Bitmap bitmap,Rectangle rectangle,int level) {
			rectangle.Intersect(new Rectangle(0,0,bitmap.Width,bitmap.Height));
			for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
				for(int x=rectangle.Left;x<rectangle.Right;x++) {
					Color color=bitmap.GetPixel(x,y);
					if(color.R<level && color.G<level && color.B<level) {
						return true;
					}
				}
			}
			return false;
		}
	}
}
