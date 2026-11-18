// The print guard for the eForm PDF (package UI-2a-3): the pages FrmEFormFillEdit's Save to Images makes (CtrlEFormFill.Pd_PrintPage through
// WpfControls.PrinterL.CreateFixedDocument, each page rendered at 200 dpi with RenderTargetBitmap and placed on a PDF page) are identical with
// today's font tokens and with UI-2b's. It needs WPF rendering, so it runs only with OPMS_PRINT_GUARD_WPF=1, in a connected session (the
// lead's steps are in docs/port-notes/UI-2a.md, section UI-2a-3). The internal Patient Registration eForm, filled for a synthetic patient in
// the scratch database ui2a3_*; nothing is printed and no window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.Drawing;
using OpenPmsDentalBusiness;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PrintGuardTests {
	[TestClass]
	public class EFormPdfGuardTests {
		[TestMethod]
		public void EForm_PdfPages_AreTheSameWithUi2bFonts() {
			if(Environment.GetEnvironmentVariable("OPMS_PRINT_GUARD_WPF")!="1") {
				Assert.Inconclusive("The eForm PDF needs WPF rendering: set OPMS_PRINT_GUARD_WPF=1 in a connected session.");
			}
			long patNum=0;
			StaRunner.Run(() => {
				PrintGuardDb.Begin();
				Patient patient=new Patient { LName="Printguard",FName="Eform",PatStatus=PatientStatus.Patient,Birthdate=new DateTime(1990,1,1),ChartNumber="" };
				patNum=Patients.Insert(patient,false);
			});
			Guard.AssertSameWithUi2bFonts("EForm_PdfPages",() => {
				EForm eForm=EForms.CreateEFormFromEFormDef(EFormInternal.GetEFormDef(EnumEFormInternalType.PatientRegistration),patNum,EnumEFormStatus.None);
				eForm.DateTimeShown=new DateTime(2026,11,17,9,0,0);
				EFormFiller.FillFields(eForm);
				CtrlEFormFill ctrlEFormFill=MakeFill(eForm);
				ctrlEFormFill.FillFieldsFromControls();
				Printout printout=new Printout();
				printout.FuncPrintPage=ctrlEFormFill.Pd_PrintPage;
				printout.thicknessMarginInches=new Thickness(0.5);
				ctrlEFormFill.PagesPrinted=0;
				WpfControls.PrinterL.CreateFixedDocument(printout);
				ctrlEFormFill.RefreshLayout();
				List<byte> listBytes=new List<byte>();
				foreach(PageContent pageContent in printout.FixedDocument_.Pages) {
					//FrmEFormFillEdit.butSaveImage: each page at 200 dpi, as a PNG, then drawn on a PDF page.
					FixedPage fixedPage=pageContent.Child;
					fixedPage.Measure(new Size(fixedPage.Width,fixedPage.Height));
					fixedPage.Arrange(new Rect(new Size(fixedPage.Width,fixedPage.Height)));
					fixedPage.UpdateLayout();
					double dpi=200;
					RenderTargetBitmap renderTarget=new RenderTargetBitmap((int)(fixedPage.ActualWidth*dpi/96),(int)(fixedPage.ActualHeight*dpi/96),dpi,dpi,
						PixelFormats.Pbgra32);
					renderTarget.Render(fixedPage);
					int stride=renderTarget.PixelWidth*4;
					byte[] arrayPixels=new byte[stride*renderTarget.PixelHeight];
					renderTarget.CopyPixels(arrayPixels,stride,0);
					listBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(renderTarget.PixelWidth+"x"+renderTarget.PixelHeight+"\n"));
					listBytes.AddRange(arrayPixels);
				}
				Assert.IsTrue(listBytes.Count>0,"The eForm printed no page.");
				return listBytes.ToArray();
			},PrintGuardDb.Begin);
		}

		///<summary>The fill control as FrmEFormFillEdit sets it up on Load and sizes it, laid out without a window (tests\EFormPrintTests).</summary>
		private static CtrlEFormFill MakeFill(EForm eForm) {
			CtrlEFormFill ctrlEFormFill=new CtrlEFormFill();
			ctrlEFormFill.ListEFormFields=eForm.ListEFormFields;
			ctrlEFormFill.PatNum=eForm.PatNum;
			ctrlEFormFill.ShowLabelsBold=eForm.ShowLabelsBold;
			ctrlEFormFill.SpaceBelowEachField=eForm.SpaceBelowEachField;
			ctrlEFormFill.SpaceToRightEachField=eForm.SpaceToRightEachField;
			ctrlEFormFill.Zoom=1;
			double width=eForm.MaxWidth+17+2;
			double height=650;
			ctrlEFormFill.Width=width;
			ctrlEFormFill.Height=height;
			ctrlEFormFill.Measure(new Size(width,height));
			ctrlEFormFill.Arrange(new Rect(new Size(width,height)));
			ctrlEFormFill.UpdateLayout();
			ctrlEFormFill.RefreshLayout();
			ctrlEFormFill.UpdateLayout();
			ctrlEFormFill.FillFieldsFromControls();
			ctrlEFormFill.RefreshLayout();
			return ctrlEFormFill;
		}
	}
}
