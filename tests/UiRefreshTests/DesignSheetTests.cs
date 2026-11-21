// The design sheet, gallery G-UI1 part 1 (package UI-1, docs/specs/ui-refresh-plan.md sections 7 and 8, docs/port-notes/UI-1.md): a test without
// a database that renders the sheet's pages off screen (colours, contrast, type, sizes, every control state at 100 to 200%, the options UQ-1 to
// UQ-4, the high-contrast mapping) and the five real windows in today's font and the font options, into OPMS_UI_SHEET_OUT (default: design-sheet
// in the test's output folder):
//   after\sheet\*.png                     the sheet's pages (only "after": they have no "before")
//   before\windows-9pt-17px\*.png         today's windows, the "before" of the chosen options
//   after\windows-9pt-17px\*.png          the windows in Segoe UI 9 with 17 px grid rows (UQ-1 a, UQ-2 a)
//   before\windows-8.25pt-16px\*.png      today's windows again, the "before" of the other options
//   after\windows-8.25pt-16px\*.png       the windows in Segoe UI 8.25 with 16 px grid rows (UQ-1 b, UQ-2 b)
//   after\windows-wpf\FrmProviderPick.png the WPF window (already Segoe UI), when this session can render WPF
//   notes.txt                             what was drawn, the grid row heights measured, and anything that waits for a connected session
// tools\New-UiGallery.ps1 -Before <out>\before -After <out>\after -Name G-UI1 makes the gallery (the port note gives the command). Nothing is
// shown on the desktop.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;
using UiRefreshTests.Scan;
using UiRefreshTests.TextFit;

namespace UiRefreshTests {
	[TestClass]
	public class DesignSheetTests {
		public const string VARIABLE_SHEET_OUT="OPMS_UI_SHEET_OUT";
		private const string MARKER="design-sheet.txt";
		private static readonly object _lockFolder=new object();
		private static bool _isFolderReady;

		[TestMethod]
		public void DesignSheet_RendersEveryPage() {
			string folderSheet=Path.Combine(GetFolderOut(),"after","sheet");
			List<string> listFiles=new List<string>();
			StaRunner.Run(() => {
				listFiles.Add(DesignSheetPages.DrawColours(folderSheet));
				listFiles.Add(DesignSheetPages.DrawContrastTable(folderSheet));
				listFiles.Add(DesignSheetPages.DrawType(folderSheet));
				listFiles.Add(DesignSheetPages.DrawSizes(folderSheet));
				foreach(float scale in DesignSheetPages.ArrayScales) {
					listFiles.Add(DesignSheetPages.DrawControls(folderSheet,scale));
				}
				listFiles.Add(DesignSheetPages.DrawOptions(folderSheet));
				listFiles.Add(DesignSheetPages.DrawHighContrast(folderSheet));
			});
			Assert.AreEqual(11,listFiles.Count);
			foreach(string path in listFiles) {
				Assert.IsTrue(File.Exists(path),path);
				using(Bitmap bitmap=new Bitmap(path)) {
					Assert.IsTrue(bitmap.Width>=1000 && bitmap.Height>=300,path+" is "+bitmap.Width+" x "+bitmap.Height);
					Assert.IsTrue(DesignSheetWindows.CountColors(bitmap,24)>=24,path+" looks blank.");
				}
			}
			//The control pages scale with the scale: 200% is twice as wide as 100%.
			using(Bitmap bitmap100=new Bitmap(listFiles.Single(x => x.EndsWith("controls-100.png"))))
			using(Bitmap bitmap200=new Bitmap(listFiles.Single(x => x.EndsWith("controls-200.png")))) {
				Assert.AreEqual(bitmap100.Width*2,bitmap200.Width,2);
				Assert.IsTrue(bitmap200.Height>bitmap100.Height*1.8);
			}
			WriteNotes("sheet",listFiles.Select(x => "drawn: "+Path.GetFileName(x)).ToList());
		}

		[TestMethod]
		public void DesignSheet_RealWindows_InTodaysFontAndTheOptions() {
			string folderOut=GetFolderOut();
			string folderBefore=Path.Combine(folderOut,"before");
			string folderAfter=Path.Combine(folderOut,"after");
			List<string> listNotes=new List<string>();
			List<string> listFiles=new List<string>();
			string pathFrm=null;
			StaRunner.Run(() => {
				listFiles.AddRange(DesignSheetWindows.Render(folderBefore,folderAfter,listNotes));
				try {
					pathFrm=DesignSheetWindows.RenderFrm(folderAfter,listNotes);
				}
				catch(Exception exception) {
					listNotes.Add("FrmProviderPick could not be rendered in this session ("+exception.GetType().Name+": "+exception.Message
						+"): the WPF window waits for the next connected session.");
				}
			});
			//Two options, four pictures each (FormPatientEdit, FormProcEdit, ControlAccount and its toolbar with the grid), before and after.
			Assert.AreEqual(16,listFiles.Count,string.Join("\r\n",listFiles));
			foreach(string path in listFiles) {
				using(Bitmap bitmap=new Bitmap(path)) {
					Assert.IsTrue(DesignSheetWindows.CountColors(bitmap,16)>=16,path+" looks blank.");
					Assert.IsTrue(DesignSheetWindows.CountDarkPixels(bitmap)>=500,path+" has no text or outlines: the controls were not drawn.");
				}
			}
			//The options change the pictures; today's pictures are the same for both options.
			foreach(string name in new[] { "FormPatientEdit.png","FormProcEdit.png","ControlAccount.png","ControlAccount-toolbar-and-grid.png" }) {
				string pathToday=Path.Combine(folderBefore,DesignSheetWindows.OptionChosen.Folder,name);
				Assert.IsTrue(AreSamePixels(pathToday,Path.Combine(folderBefore,DesignSheetWindows.OptionOther.Folder,name)),"Today's "+name+" twice the same.");
				Assert.IsFalse(AreSamePixels(pathToday,Path.Combine(folderAfter,DesignSheetWindows.OptionChosen.Folder,name)),name+" in Segoe UI 9 differs.");
				Assert.IsFalse(AreSamePixels(pathToday,Path.Combine(folderAfter,DesignSheetWindows.OptionOther.Folder,name)),name+" in Segoe UI 8.25 differs.");
			}
			if(pathFrm!=null) {
				listNotes.Add("drawn: "+DesignSheetWindows.FOLDER_WPF+"/FrmProviderPick.png (WPF, without a window)");
			}
			listNotes.AddRange(listFiles.Select(x => "drawn: "+x.Substring(folderOut.Length+1).Replace('\\','/')));
			WriteNotes("windows",listNotes);
		}

		[TestMethod]
		public void GridFonts_RowsFollowTheOption() {
			StaRunner.Run(() => {
				UiTheme.SetFontForTest(UiFontOption.Today);//today's grid (14 px rows); the program's tokens are UI-2b's since the flip
				try {
					using(Form form=new Form())
					using(GridOD gridOD=new GridOD()) {
						form.ClientSize=new Size(400,300);
						gridOD.Bounds=new Rectangle(0,0,400,300);
						gridOD.WrapText=false;
						form.Controls.Add(gridOD);
						DesignSheetWindows.CreateHandles(form);
						gridOD.BeginUpdate();
						gridOD.Columns.Add(new GridColumn("Description",200));
						gridOD.ListGridRows.Add(new GridRow("Periodic oral evaluation"));
						gridOD.ListGridRows.Add(new GridRow("Prophylaxis, adult"));
						gridOD.EndUpdate();
						Assert.AreEqual(14,DesignSheetWindows.GetFirstRowHeight(gridOD),"Today: sans serif 8.5, 14 px rows (section 5.3).");
						DesignSheetWindows.SetGridFonts(gridOD,UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY);
						Assert.AreEqual(UiTheme.GRID_ROW_HEIGHT,DesignSheetWindows.GetFirstRowHeight(gridOD),"UQ-2 (a): 17 px rows at Segoe UI 9.");
						DesignSheetWindows.SetGridFonts(gridOD,UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY_COMPACT);
						Assert.AreEqual(UiTheme.GRID_ROW_HEIGHT_COMPACT,DesignSheetWindows.GetFirstRowHeight(gridOD),"UQ-2 (b): 16 px rows at Segoe UI 8.25.");
						Assert.IsFalse(form.Visible,"Never shown.");
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
			});
		}

		[TestMethod]
		public void DesignSheet_Families_CoverSection36() {
			List<SheetFamily> listFamilies=DesignSheetControls.GetFamilies();
			foreach(string name in new[] { "Button","Default button","Check box","Radio button","Combo box","List item","Grid row","Toolbar button","Menu item",
				"Module bar button","Tab header","Calendar day","Splitter","Minimise, maximise","Close" })
			{
				Assert.IsTrue(listFamilies.Any(x => x.Name==name),name);
			}
			Assert.AreEqual(8,DesignSheetControls.ArrayStateNames.Length,"Normal, hover, pressed, focused, disabled, checked, selected, selected without focus.");
			CollectionAssert.AreEqual(new[] { 1f,1.25f,1.5f,1.75f,2f },DesignSheetPages.ArrayScales);
			//Every family draws its normal state, and the states the plan names for it.
			StaRunner.Run(() => {
				using(Bitmap bitmap=new Bitmap(200,80))
				using(Graphics g=Graphics.FromImage(bitmap))
				using(SheetPainter painter=new SheetPainter(g,1f,SheetPainter.Light)) {
					Rectangle cell=new Rectangle(0,0,DesignSheetControls.CELL_WIDTH,DesignSheetControls.CELL_HEIGHT);
					foreach(SheetFamily family in listFamilies) {
						Assert.IsNotNull(family.Draw(painter,cell,SheetState.Normal),family.Name);
					}
					Assert.IsNotNull(listFamilies.Single(x => x.Name=="Grid row").Draw(painter,cell,SheetState.SelectedNoFocus),"The selection bar without focus.");
					Assert.IsNotNull(listFamilies.Single(x => x.Name=="Toolbar button").Draw(painter,cell,SheetState.Checked),"The toggled toolbar button.");
					Assert.AreEqual("indeterminate",listFamilies.Single(x => x.Name=="Check box").Draw(painter,cell,SheetState.Selected));
				}
			});
		}

		private static bool AreSamePixels(string path1,string path2) {
			using(Bitmap bitmap1=new Bitmap(path1))
			using(Bitmap bitmap2=new Bitmap(path2)) {
				if(bitmap1.Size!=bitmap2.Size) {
					return false;
				}
				for(int y=0;y<bitmap1.Height;y++) {
					for(int x=0;x<bitmap1.Width;x++) {
						if(bitmap1.GetPixel(x,y).ToArgb()!=bitmap2.GetPixel(x,y).ToArgb()) {
							return false;
						}
					}
				}
				return true;
			}
		}

		///<summary>OPMS_UI_SHEET_OUT, else the test's output folder's design-sheet. Emptied once per test run when it holds the marker of an earlier
		///sheet (or is empty); any other folder is refused.</summary>
		public static string GetFolderOut() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_SHEET_OUT);
			if(string.IsNullOrWhiteSpace(folder)) {
				folder=Path.Combine(AppContext.BaseDirectory,"design-sheet");
			}
			folder=Path.GetFullPath(folder.Trim());
			lock(_lockFolder) {
				if(_isFolderReady) {
					return folder;
				}
				if(Directory.Exists(folder) && Directory.EnumerateFileSystemEntries(folder).Any()) {
					if(!File.Exists(Path.Combine(folder,MARKER))) {
						Assert.Fail(folder+" is not empty and is not an earlier design sheet ("+MARKER+"): name an empty folder in "+VARIABLE_SHEET_OUT+".");
					}
					foreach(string sub in new[] { "before","after" }) {
						if(Directory.Exists(Path.Combine(folder,sub))) {
							Directory.Delete(Path.Combine(folder,sub),true);
						}
					}
					foreach(string file in Directory.EnumerateFiles(folder,"notes-*.txt")) {
						File.Delete(file);
					}
				}
				Directory.CreateDirectory(folder);
				File.WriteAllText(Path.Combine(folder,MARKER),"The design sheet of package UI-1 (tests/UiRefreshTests/DesignSheetTests.cs); a later run empties this folder.\r\n");
				_isFolderReady=true;
			}
			return folder;
		}

		private static void WriteNotes(string name,List<string> listLines) {
			File.WriteAllLines(Path.Combine(GetFolderOut(),"notes-"+name+".txt"),listLines);
		}
	}
}
