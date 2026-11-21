// The print guard for UI-2b's pins (docs/port-notes/UI-2b.md, section 1): printouts and documents that drew with an inherited font before the
// flip (the window's font or the exe's default font) keep today's font. Each output is rendered with today's font tokens and default font and with
// UI-2b's (Guard), and must be the same bytes; without its pin each would follow the body font and differ. The equipment list's printed total
// (FormEquipment.pd_PrintPage), a digital signature's picture (SignatureBox.GetSigImage, which sheets print and save), on its own and in a
// window, and the dashboard graph's printed legend (Legend.PrintCopy, drawn as FormPrintSettings.MakePage draws it). Nothing is printed and no
// window is shown; the equipment list reads the scratch database ui2a3_* (no rows are added).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalGraph;

namespace PrintGuardTests {
	[TestClass]
	public class PinnedFontPrintGuardTests {
		[TestMethod]
		public void Equipment_PrintedList_IsTheSameWithUi2bFonts() {
			//The window as the program builds it, its grid filled by its own FillGrid, and its page event: the heading (Arial), the grid (its print
			//fonts) and the "Total Est Value" line, which drew with the window's font before the flip.
			Guard.AssertSameWithUi2bFonts("Equipment_Printout",() => {
				using(FormEquipment formEquipment=new FormEquipment()) {
					DocumentPrintGuardTests.Invoke(typeof(FormEquipment),formEquipment,"FillGrid");
					return Guard.DrawToPixels(1100,850,g => {
						PrintPageEventArgs printPageEventArgs=new PrintPageEventArgs(g,new Rectangle(50,40,1000,770),new Rectangle(0,0,1100,850),new PageSettings());
						DocumentPrintGuardTests.Invoke(typeof(FormEquipment),formEquipment,"pd_PrintPage",null,printPageEventArgs);
						Assert.IsFalse(printPageEventArgs.HasMorePages,"One page: the total is on it.");
					});
				}
			},PrintGuardDb.Begin);
		}

		[TestMethod]
		public void Signature_DigitalSignaturePicture_IsTheSameWithUi2bFonts() {
			const string TEXT="Digitally Signed by SYN - Synthetic, Alex\r\nDate Signed: 1/6/2026 9:00 AM";
			//On its own, as SheetDrawingJob builds a signature box for a sheet's picture (the exe's default font).
			Guard.AssertSameWithUi2bFonts("Signature_DigitalSig",() => {
				using(SignatureBox signatureBox=new SignatureBox()) {
					signatureBox.Size=new Size(362,79);
					signatureBox.SetDigitalSig(TEXT);
					using(Bitmap bitmap=(Bitmap)signatureBox.GetSigImage(true)) {
						return Guard.GetPixels(bitmap);
					}
				}
			});
			//In a window under LayoutManager (the window's font, the body token), as a window's or a filled sheet's signature box.
			Guard.AssertSameWithUi2bFonts("Signature_DigitalSigInWindow",() => {
				SignatureBox signatureBox=new SignatureBox();
				signatureBox.Size=new Size(362,79);
				using(HostFormOD hostFormOD=new HostFormOD(signatureBox)) {
					signatureBox.SetDigitalSig(TEXT);
					using(Bitmap bitmap=(Bitmap)signatureBox.GetSigImage(true)) {
						return Guard.GetPixels(bitmap);
					}
				}
			});
		}

		[TestMethod]
		public void GraphLegend_PrintCopy_IsTheSameWithUi2bFonts() {
			//The legend's print copy, drawn with DrawToBitmap into the page's picture as FormPrintSettings.MakePage draws it (printed and exported as
			//PDF). The copy has no parent: before the flip it drew in the exe's default font.
			Guard.AssertSameWithUi2bFonts("GraphLegend_PrintCopy",() => {
				List<ODGraphLegendItem> listLegendItems=new List<ODGraphLegendItem> {
					new ODGraphLegendItem { ItemName="Production",ItemColor=Color.SteelBlue },
					new ODGraphLegendItem { ItemName="Collections",ItemColor=Color.DarkOrange },
					new ODGraphLegendItem { ItemName="Synthetic adjustments",ItemColor=Color.SeaGreen },
				};
				using(Legend legend=new Legend()) {
					legend.SetLegendItems(listLegendItems);
					using(Legend legendCopy=legend.PrintCopy()) {
						legendCopy.Size=new Size(600,60);
						using(Bitmap bitmap=new Bitmap(legendCopy.Width,legendCopy.Height)) {
							bitmap.SetResolution(96,96);
							legendCopy.DrawToBitmap(bitmap,new Rectangle(0,0,legendCopy.Width,legendCopy.Height));
							return Guard.GetPixels(bitmap);
						}
					}
				}
			});
		}
	}
}
