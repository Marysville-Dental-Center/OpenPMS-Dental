// The print guard for the grids (package UI-2a-3): GridOD's and GridOld's printout (PrintPage, every page, and a second printout of the same
// grid) and their sheet output (SheetDrawTitle, SheetDrawHeader and SheetDrawRow into a bitmap, their PDF variants into a PDF, and the sheet
// design canvas's grid) are identical with today's font tokens and with UI-2b's. The positive controls show the override reaches the grids'
// screen fonts, so the guard compares something that could differ. No database, no window shown: the grids are built off screen.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PrintGuardTests {
	[TestClass]
	public class GridPrintGuardTests {
		private const int PAGE_WIDTH=850;
		private const int PAGE_HEIGHT=1100;

		#region GridOD
		[TestMethod]
		public void GridOD_PrintPage_IsTheSameWithUi2bFonts() {
			Guard.AssertSameWithUi2bFonts("GridOD_PrintPage",() => {
				using(GridOD grid=NewGridOD(isForSheets:false)) {
					List<byte> listBytes=new List<byte>();
					//First printout, every page, then the same grid printed again (the heights it keeps from the first).
					listBytes.AddRange(PrintAllPages((g,page,bounds) => grid.PrintPage(g,page,bounds,100)));
					listBytes.AddRange(PrintAllPages((g,page,bounds) => grid.PrintPage(g,page,bounds,100)));
					return listBytes.ToArray();
				}
			});
		}

		[TestMethod]
		public void GridOD_PrintPage_AtZoom120_IsTheSameWithUi2bFonts() {
			Guard.AssertSameWithUi2bFonts("GridOD_PrintPage_Zoom120",() => {
				using(GridOD grid=NewGridOD(isForSheets:false)) {
					grid.SetScaleAndZoom(1,1.2f);//PrintPage prints at 100% and puts the zoom back
					return PrintAllPages((g,page,bounds) => grid.PrintPage(g,page,bounds,100,HasHeaderSpaceOnEveryPage:true)).ToArray();
				}
			});
		}

		[TestMethod]
		public void GridOD_SheetOutput_IsTheSameWithUi2bFonts() {
			Guard.AssertSameWithUi2bFonts("GridOD_SheetBitmap",() => {
				using(GridOD grid=NewGridOD(isForSheets:true)) {
					return Guard.DrawToPixels(PAGE_WIDTH,PAGE_HEIGHT*2,g => DrawSheetGrid(grid,g,null));
				}
			});
			Guard.AssertSameWithUi2bFonts("GridOD_SheetPdf",() => {
				using(GridOD grid=NewGridOD(isForSheets:true)) {
					PdfDocument pdfDocument=new PdfDocument();
					PdfPage pdfPage=pdfDocument.AddPage();
					pdfPage.Width=XUnit.FromPoint(612);
					pdfPage.Height=XUnit.FromPoint(792*2);
					using(XGraphics xGraphics=XGraphics.FromPdfPage(pdfPage)) {
						DrawSheetGrid(grid,null,xGraphics);
					}
					return Guard.GetPdfPages(pdfDocument);
				}
			});
		}

		[TestMethod]
		public void GridOD_SheetCanvasGrid_IsTheSameWithUi2bFonts() {
			//FormSheetDefEdit's canvas: a grid with one dummy row, drawn with the sheet methods (IsForDocument, as CreateGrid sets it).
			Guard.AssertSameWithUi2bFonts("GridOD_SheetCanvas",() => {
				using(GridOD grid=new GridOD()) {
					grid.IsForDocument=true;
					grid.TranslationName="";
					grid.Width=0;
					grid.BeginUpdate();
					grid.Columns.Clear();
					foreach(int width in new[] { 70,300,60,90 }) {
						grid.Columns.Add(new GridColumn("Column "+width,width));
						grid.Width+=width;
					}
					GridRow gridRow=new GridRow();
					for(int i=0;i<4;i++) {
						gridRow.Cells.Add(" ");
					}
					grid.ListGridRows.Add(gridRow);
					grid.EndUpdate();
					return Guard.DrawToPixels(600,120,g => {
						grid.SheetDrawTitle(g,10,10,1);
						grid.SheetDrawHeader(g,10,28,1);
						grid.SheetDrawRow(0,g,10,43,false,true);
						g.DrawRectangle(Pens.Black,10,43,grid.Width,grid.ListGridRows[0].State.HeightMain);//the row's height, as the canvas lays it out
					});
				}
			});
		}

		[TestMethod]
		public void GridOD_ScreenFonts_FollowTheTokens_PrintFontsStay() {
			//The positive control: the screen picture changes with UI-2b's tokens, so the guard compares fonts that could differ.
			byte[][] arrayOutputs=Guard.RenderTodayAndUi2b(() => {
				using(GridOD grid=NewGridOD(isForSheets:false)) {
					using(Bitmap bitmap=new Bitmap(grid.Width,grid.Height)) {
						grid.DrawToBitmap(bitmap,new Rectangle(0,0,grid.Width,grid.Height));
						return Guard.GetPixels(bitmap);
					}
				}
			});
			Assert.IsFalse(arrayOutputs[0].SequenceEqual(arrayOutputs[1]),"GridOD's screen picture takes the font tokens (FontGrid, FontTitle).");
			string fontsToday=null;
			string fontsUi2b=null;
			StaRunner.Run(() => {
				using(GridOD grid=new GridOD()) {
					fontsToday=DescribeFonts(grid);
				}
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					using(GridOD grid=new GridOD()) {
						fontsUi2b=DescribeFonts(grid);
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
			});
			string printToday="print: Microsoft Sans Serif 8.5 Regular, Microsoft Sans Serif 8.5 Bold, Microsoft Sans Serif 8.5 Bold, Microsoft Sans Serif 10 Bold";
			Assert.AreEqual("screen: Microsoft Sans Serif 8.5 Regular, Microsoft Sans Serif 8.5 Bold, Microsoft Sans Serif 8.5 Bold, Microsoft Sans Serif 10 Bold; "
				+printToday,fontsToday,"With today's tokens the screen fonts are the print fonts: today's sans serif 8.5 and 10 Bold.");
			Assert.AreEqual("screen: Segoe UI 9 Regular, Segoe UI 9 Bold, Segoe UI 9 Bold, Segoe UI Semibold 10 Regular; "+printToday,fontsUi2b,
				"With UI-2b's tokens the screen fonts follow them and the print fonts stay today's.");
		}
		#endregion GridOD

		#region GridOld
		[TestMethod]
		public void GridOld_PrintPage_IsTheSameWithUi2bFonts() {
			Guard.AssertSameWithUi2bFonts("GridOld_PrintPage",() => {
				using(GridOld grid=NewGridOld(isForSheets:false)) {
					List<byte> listBytes=new List<byte>();
					listBytes.AddRange(PrintAllPages((g,page,bounds) => grid.PrintPage(g,page,bounds,100,true)));
					listBytes.AddRange(PrintAllPages((g,page,bounds) => grid.PrintPage(g,page,bounds,100,true)));
					return listBytes.ToArray();
				}
			});
		}

		[TestMethod]
		public void GridOld_SheetOutput_IsTheSameWithUi2bFonts() {
			Guard.AssertSameWithUi2bFonts("GridOld_SheetBitmap",() => {
				using(GridOld grid=NewGridOld(isForSheets:true)) {
					return Guard.DrawToPixels(PAGE_WIDTH,PAGE_HEIGHT,g => {
						grid.SheetDrawTitle(g,20,20);
						grid.SheetDrawHeader(g,20,38);
						int y=53;
						for(int i=0;i<grid.ListGridRows.Count;i++) {
							grid.SheetDrawRow(i,g,20,y,i==grid.ListGridRows.Count-1,true,true);
							y+=grid.ListGridRows[i].State.HeightMain+grid.ListGridRows[i].State.HeightNote;
						}
					});
				}
			});
			Guard.AssertSameWithUi2bFonts("GridOld_SheetPdf",() => {
				using(GridOld grid=NewGridOld(isForSheets:true)) {
					PdfDocument pdfDocument=new PdfDocument();
					PdfPage pdfPage=pdfDocument.AddPage();
					using(XGraphics xGraphics=XGraphics.FromPdfPage(pdfPage)) {
						//No title: GridOld's SheetDrawTitleX fills with a gradient XBrush, which PDFsharp 6 does not draw (the program draws every sheet
						//grid with GridOD; GridOld prints only the Chart module's progress notes, through PrintPage).
						grid.SheetDrawHeaderX(xGraphics,20,38);
						int y=53;
						for(int i=0;i<grid.ListGridRows.Count && y<1000;i++) {
							grid.SheetDrawRowX(i,xGraphics,20,y,i==grid.ListGridRows.Count-1,true);
							y+=grid.ListGridRows[i].State.HeightMain+grid.ListGridRows[i].State.HeightNote;
						}
					}
					return Guard.GetPdfPages(pdfDocument);
				}
			});
		}

		[TestMethod]
		public void GridOld_ScreenFonts_FollowTheTokens_PrintFontsStay() {
			byte[][] arrayOutputs=Guard.RenderTodayAndUi2b(() => {
				using(GridOld grid=NewGridOld(isForSheets:false)) {
					using(Bitmap bitmap=new Bitmap(grid.Width,grid.Height)) {
						grid.DrawToBitmap(bitmap,new Rectangle(0,0,grid.Width,grid.Height));
						return Guard.GetPixels(bitmap);
					}
				}
			});
			Assert.IsFalse(arrayOutputs[0].SequenceEqual(arrayOutputs[1]),"GridOld's screen picture takes the font tokens (FontGrid, FontTitle).");
		}
		#endregion GridOld

		#region Helpers
		///<summary>Prints every page of a printout into its own white letter-size bitmap at 96 dpi, as a printer's page event would, and returns the
		///pages' pixels one after another. funcPrintPage returns -1 while there are more pages.</summary>
		private static List<byte> PrintAllPages(Func<Graphics,int,Rectangle,int> funcPrintPage) {
			List<byte> listBytes=new List<byte>();
			Rectangle bounds=new Rectangle(50,50,PAGE_WIDTH-100,PAGE_HEIGHT-100);
			for(int page=0;page<20;page++) {
				int yPos=0;
				listBytes.AddRange(Guard.DrawToPixels(PAGE_WIDTH,PAGE_HEIGHT,g => yPos=funcPrintPage(g,page,bounds)));
				if(yPos!=-1) {
					return listBytes;
				}
			}
			Assert.Fail("The printout did not end within 20 pages.");
			return listBytes;
		}

		///<summary>Draws an IsForSheets grid as SheetDrawingJob.DrawFieldGridHelper does: each sheet row with its title and header.</summary>
		private static void DrawSheetGrid(GridOD grid,Graphics g,XGraphics xGraphics) {
			for(int i=0;i<grid.ListGridSheetRows.Count;i++) {
				GridSheetRow gridSheetRow=grid.ListGridSheetRows[i];
				int yAdj=0;
				if(gridSheetRow.IsTitleRow) {
					if(xGraphics==null) {
						grid.SheetDrawTitle(g,20,gridSheetRow.YPos,1);
					}
					else {
						grid.SheetDrawTitleX(xGraphics,20,gridSheetRow.YPos);
					}
					yAdj+=18;
				}
				if(gridSheetRow.IsHeaderRow) {
					if(xGraphics==null) {
						grid.SheetDrawHeader(g,20,gridSheetRow.YPos+yAdj,1);
					}
					else {
						grid.SheetDrawHeaderX(xGraphics,20,gridSheetRow.YPos+yAdj);
					}
					yAdj+=15;
				}
				if(xGraphics==null) {
					grid.SheetDrawRow(i,g,20,gridSheetRow.YPos+yAdj,gridSheetRow.IsBottomRow,true,true,1);
				}
				else {
					grid.SheetDrawRowX(i,xGraphics,20,gridSheetRow.YPos+yAdj,gridSheetRow.IsBottomRow,true);
				}
			}
		}

		///<summary>A grid of 40 synthetic rows: wrapped text, bold rows and cells, a coloured cell, notes across columns 1 to 3, a right-aligned
		///column and a two-line header. For sheets it is set up as SheetDrawingJob sets up a sheet grid (two pages of a letter sheet).</summary>
		private static GridOD NewGridOD(bool isForSheets) {
			GridOD grid=new GridOD();
			grid.TranslationName="";
			grid.Size=new Size(560,400);
			grid.BeginUpdate();
			grid.Title=isForSheets ? "ProcsWithFee" : "Print guard";
			grid.HasMultilineHeaders=true;
			grid.NoteSpanStart=1;
			grid.NoteSpanStop=3;
			if(isForSheets) {
				grid.IsForSheets=true;
				grid.VScrollVisible=false;
				grid.SheetYPos=40;
				grid.SheetTopMargin=40;
				grid.SheetBottomMargin=60;
				grid.SheetPageHeight=PAGE_HEIGHT;
			}
			grid.Columns.Clear();
			grid.Columns.Add(new GridColumn("Date",80));
			grid.Columns.Add(new GridColumn("Description",260));
			grid.Columns.Add(new GridColumn("Amount",80,HorizontalAlignment.Right));
			grid.Columns.Add(new GridColumn("Code\r\nand tooth",110,HorizontalAlignment.Center));
			grid.ListGridRows.Clear();
			grid.ListGridRows.AddRange(NewRows());
			grid.EndUpdate();
			return grid;
		}

		private static GridOld NewGridOld(bool isForSheets) {
			GridOld grid=new GridOld();
			grid.TranslationName="";
			grid.Size=new Size(560,400);
			grid.BeginUpdate();
			grid.Title="Progress notes";
			grid.NoteSpanStart=1;
			grid.NoteSpanStop=3;
			grid.IsForSheets=isForSheets;
			grid.Columns.Clear();
			grid.Columns.Add(new GridColumn("Date",80));
			grid.Columns.Add(new GridColumn("Description",260));
			grid.Columns.Add(new GridColumn("Amount",80,HorizontalAlignment.Right));
			grid.Columns.Add(new GridColumn("Code",110,HorizontalAlignment.Center));
			grid.ListGridRows.Clear();
			grid.ListGridRows.AddRange(NewRows());
			grid.EndUpdate();
			return grid;
		}

		private static List<GridRow> NewRows() {
			List<GridRow> listGridRows=new List<GridRow>();
			for(int i=0;i<40;i++) {
				GridRow gridRow=new GridRow();
				gridRow.Cells.Add(new DateTime(2026,1,5).AddDays(i).ToString("MM/dd/yyyy"));
				if(i%5==0) {
					gridRow.Cells.Add("A long synthetic description that wraps onto a second line of the description column, row "+i);
				}
				else {
					gridRow.Cells.Add("Synthetic row "+i);
				}
				GridCell gridCell=new GridCell((i*12.5).ToString("F2"));
				if(i%3==0) {
					gridCell.Bold=YN.Yes;
				}
				if(i%7==0) {
					gridCell.ColorText=Color.DarkRed;
				}
				gridRow.Cells.Add(gridCell);
				gridRow.Cells.Add("D"+(1110+i));
				if(i%4==0) {
					gridRow.Note="A synthetic note for row "+i+" that runs across the note columns of the grid.";
				}
				gridRow.Bold=(i%6==0);
				listGridRows.Add(gridRow);
			}
			return listGridRows;
		}

		///<summary>The grid's screen fonts (cell, cell bold, header, title) and print fonts (the same four), as "family size style".</summary>
		private static string DescribeFonts(object grid) {
			Func<string,string> funcFont=name => {
				Font font=(Font)grid.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(grid);
				return font.Name+" "+font.SizeInPoints.ToString(System.Globalization.CultureInfo.InvariantCulture)+" "+font.Style;
			};
			return "screen: "+string.Join(", ",new[] { "_fontCell","_fontCellBold","_fontHeader","_fontTitle" }.Select(funcFont))
				+"; print: "+string.Join(", ",new[] { "_fontPrintCell","_fontPrintCellBold","_fontPrintHeader","_fontPrintTitle" }.Select(funcFont));
		}
		#endregion Helpers
	}
}
