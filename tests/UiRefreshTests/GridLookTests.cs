// The grid pictures of package UI-5 (docs/specs/ui-refresh-plan.md, section 7's UI-5 row, sections 3.2, 3.6 and the owner's answer UQ-2;
// docs/port-notes/UI-5.md): GridOD, GridOld and the WPF Grid drawn off screen with a title, a header, plain rows, a hovered row, a selected row with
// and without focus (its selection bar), the focused row's outline and a row the program coloured, and each read back pixel by pixel against the
// tokens: title Accent with white text, header SurfaceSubtle with Stroke lines, rows SurfaceRaised with Gridline lines, hover RowHover, selection
// AccentSubtle with the bar (Accent with focus, SelectionBarNoFocus without), the outline Accent, the coloured row its own colour, the border
// Stroke. The title, header and row heights are measured from the pictures: today's 18, 15 and 14 px with today's fonts, and 20, 18 and 17 px
// with UI-2b's Segoe UI (the token override), inside the grid's unchanged bounds. Also the India check (the rupee sign in the grid fonts).
// The opt-in Gallery_GridStrips writes the grid part of G-UI1 part 2 (100-200%, WinForms above WPF) and the grid strips for G-UI2 (today's font
// beside UI-2b's). No window is shown, no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToothChartImageTests;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	///<summary>GridOD with its focus looks forced (a hidden grid cannot take focus) and hover through its own mouse handler.</summary>
	public class StateGridOD : OpenPmsDental.UI.GridOD {
		public bool IsFocusForced;
		public bool IsFocusedRowForced;

		protected override bool IsSelectionFocused {
			get { return IsFocusForced; }
		}

		protected override bool IsFocusedRowShown {
			get { return IsFocusedRowForced; }
		}

		public void HoverAt(int y) {
			OnMouseMove(new MouseEventArgs(MouseButtons.None,0,40,y,0));
		}
	}

	///<summary>GridOld with its focus looks forced.</summary>
	public class StateGridOld : OpenPmsDental.UI.GridOld {
		public bool IsFocusForced;
		public bool IsFocusedRowForced;

		protected override bool IsSelectionFocused {
			get { return IsFocusForced; }
		}

		protected override bool IsFocusedRowShown {
			get { return IsFocusedRowForced; }
		}
	}

	///<summary>The WPF Grid's private hover, as its own mouse move would set it (the handler needs a real mouse event). The focus looks and the
	///bar's DPI scale go through Grid.SetFocusLookForTest: a XAML UserControl cannot be subclassed from another assembly.</summary>
	public static class StateGridWpf {
		public static void HoverRow(WpfControls.UI.Grid grid,int index) {
			typeof(WpfControls.UI.Grid).GetField("_hoverRow",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(grid,index);
			typeof(WpfControls.UI.Grid).GetMethod("DrawRowHover",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(grid,null);
		}
	}

	[TestClass]
	public class GridLookTests {
		///<summary>The rows of the design sheet's grid composite (DesignSheetControls.DrawGridComposite), each on one line (row 3's description
		///shortened so it does not wrap): row 2 is selected, row 3 the program coloured, row 4 hovered.</summary>
		private static readonly string[][] _arrayRows=new string[][] {
			new string[] { "10/14/2026","D0120","Periodic exam","45.00" },
			new string[] { "10/14/2026","D1110","Prophylaxis, adult","98.00" },
			new string[] { "10/14/2026","D0274","Bitewings, four films","62.00" },
			new string[] { "11/02/2026","D2392","Composite, two surfaces","185.00" },
			new string[] { "11/02/2026","","Payment","-120.00" },
			new string[] { "11/09/2026","D0220","Periapical, first film","28.00" } };
		private static readonly string[] _arrayHeadings=new string[] { "Date","Code","Description","Amount" };
		private static readonly int[] _arrayWidths=new int[] { 80,52,148,66 };
		///<summary>The colour the program gives row 3 (a definition colour, for example): it must stay.</summary>
		private static readonly Color _colorRowProgram=Color.FromArgb(255,236,204);
		public const int INDEX_SELECTED=2;
		public const int INDEX_COLOURED=3;
		public const int INDEX_HOVER=4;
		///<summary>A column where no text reaches (the first column's right end): every picture is read along it.</summary>
		private const int X_PROBE=74;
		private const int WIDTH_GRID=360;
		private const int HEIGHT_GRID=200;

		#region WinForms GridOD
		[TestMethod]
		public void WinForms_GridOD_TodaysFont_EveryState_InTheTokens() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(bool hasFocus in new[] { true,false }) {
					using(Bitmap bitmap=RenderGridOD(hasFocus,isFocusedRowShown:hasFocus,1f,out Size sizeBefore,out Size sizeAfter)) {
						Assert.AreEqual(sizeBefore,sizeAfter,"The grid's bounds changed.");
						CheckWinForms(listFailures,"GridOD "+(hasFocus ? "with focus" : "without focus"),bitmap,hasFocus,hasFocus,18,15,14,isHover:true);
					}
				}
			});
			AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_GridOD_Ui2bFont_HeightsFollowTheFont() {
			//UQ-2: rows 17 px at Segoe UI 9; the header 18 (Segoe UI 9 Bold) and the title 20 (Segoe UI Semibold 10), inside the same bounds.
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					using(Bitmap bitmap=RenderGridOD(hasFocus:true,isFocusedRowShown:false,1f,out Size sizeBefore,out Size sizeAfter)) {
						Assert.AreEqual(sizeBefore,sizeAfter,"The grid's bounds changed.");
						CheckWinForms(listFailures,"GridOD UI-2b",bitmap,hasFocus:true,isFocusedRowShown:false,20,18,17,isHover:true);
					}
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
			});
			AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_GridOD_ColorsAWindowSets_Stay() {
			//A window's own ColorSelectedRow (the Family and Account modules' DarkSalmon) and a row's text colour stay; the default black text is Text.
			IconRendererTests.RunSta(() => {
				using(StateGridOD grid=NewGridOD(1f)) {
					grid.ColorSelectedRow=Color.DarkSalmon;
					grid.SetSelected(INDEX_SELECTED,true);
					using(Bitmap bitmap=Draw(grid)) {
						int yRow=GetRowMiddleY(bitmap,INDEX_SELECTED,18,15,14);
						AssertNear(Color.DarkSalmon,bitmap.GetPixel(X_PROBE,yRow),0,"the window's selection colour");
						AssertNear(UiTheme.GetColor(EnumUiColor.SelectionBarNoFocus),bitmap.GetPixel(1,yRow),0,"the bar on the window's colour");
					}
				}
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Text).ToArgb(),OpenPmsDental.UI.GridScreenLook.GetTextColor(Color.Black,false).ToArgb());
				Assert.AreEqual(Color.Red.ToArgb(),OpenPmsDental.UI.GridScreenLook.GetTextColor(Color.Red,false).ToArgb());
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.AccentSubtle).ToArgb(),
					OpenPmsDental.UI.GridScreenLook.GetSelectionFill(Color.FromArgb(205,220,235),Color.FromArgb(205,220,235)).ToArgb());
				Assert.AreEqual(Color.DarkSalmon.ToArgb(),OpenPmsDental.UI.GridScreenLook.GetSelectionFill(Color.DarkSalmon,Color.FromArgb(205,220,235)).ToArgb());
				Assert.AreEqual(UiTheme.GetColor(EnumUiColor.RowHover).ToArgb(),OpenPmsDental.UI.GridScreenLook.BlendHover(Color.White).ToArgb());
			});
		}

		[TestMethod]
		public void WinForms_GridOD_ScaledBarAndBorder_FollowTheSnappingRule() {
			//Section 3.3: the bar round(2 x scale) device pixels (2, 2, 3, 4, 4), the border max(1, floor(scale)) (1 to 175%, 2 at 200%).
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(float scale in ControlStateGallery.ArrayScales) {
					using(StateGridOD grid=NewGridOD(scale)) {
						grid.IsFocusForced=true;
						grid.SetSelected(INDEX_SELECTED,true);
						using(Bitmap bitmap=Draw(grid)) {
							int yRow=FindRowOfColor(bitmap,X_PROBE,UiTheme.GetColor(EnumUiColor.AccentSubtle));
							if(yRow<0) {
								listFailures.Add(scale+": no selected row found.");
								continue;
							}
							int widthBorder=CountRun(bitmap,0,yRow,1,0,UiTheme.GetColor(EnumUiColor.Stroke));
							int widthBar=CountRun(bitmap,widthBorder,yRow,1,0,UiTheme.GetColor(EnumUiColor.Accent));
							if(widthBorder!=UiTheme.GetLineWidth(scale)) {
								listFailures.Add(scale+": border "+widthBorder+" px, expected "+UiTheme.GetLineWidth(scale)+".");
							}
							if(widthBar!=UiTheme.GetFocusWidth(scale)) {
								listFailures.Add(scale+": selection bar "+widthBar+" px, expected "+UiTheme.GetFocusWidth(scale)+".");
							}
						}
					}
				}
			});
			AssertNoFailures(listFailures);
		}
		#endregion WinForms GridOD

		#region WinForms GridOld
		[TestMethod]
		public void WinForms_GridOld_TodaysFont_TitleHeaderSelection_InTheTokens() {
			//GridOld (the Chart module's progress notes) has no hover; everything else as GridOD.
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(bool hasFocus in new[] { true,false }) {
					using(StateGridOld grid=new StateGridOld()) {
						grid.TranslationName="";
						grid.Size=new Size(WIDTH_GRID,HEIGHT_GRID);
						Size sizeBefore=grid.Size;
						Fill(grid);
						grid.IsFocusForced=hasFocus;
						grid.IsFocusedRowForced=hasFocus;
						grid.SetSelected(INDEX_SELECTED,true);
						using(Bitmap bitmap=Draw(grid)) {
							Assert.AreEqual(sizeBefore,grid.Size,"The grid's bounds changed.");
							CheckWinForms(listFailures,"GridOld "+(hasFocus ? "with focus" : "without focus"),bitmap,hasFocus,hasFocus,18,15,14,isHover:false);
						}
					}
				}
			});
			AssertNoFailures(listFailures);
		}
		#endregion WinForms GridOld

		#region WPF Grid
		[TestMethod]
		public void Wpf_Grid_EveryState_InTheTokens() {
			List<string> listFailures=new List<string>();
			bool rendersWpf=true;
			IconRendererTests.RunSta(() => {
				rendersWpf=ControlPictures.RendersWpf();
				if(!rendersWpf) {
					return;
				}
				foreach(bool hasFocus in new[] { true,false }) {
					using(Bitmap bitmap=RenderGridWpf(hasFocus,hasFocus,1f)) {
						CheckWpf(listFailures,"WPF Grid "+(hasFocus ? "with focus" : "without focus"),bitmap,hasFocus,hasFocus);
					}
				}
			});
			if(!rendersWpf) {
				Assert.Inconclusive("WPF draws nothing in this session.");
			}
			AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_Grid_HeightsFollowTheFont() {
			//18 DIP for the 14 DIP title and 15 for the 11.5 DIP header, as before (the WPF fonts do not change at UI-2b); larger fonts give larger bars.
			Assert.AreEqual(18,WpfControls.UI.GridLook.GetHeightBar(14));
			Assert.AreEqual(15,WpfControls.UI.GridLook.GetHeightBar(11.5));
			Assert.IsTrue(WpfControls.UI.GridLook.GetHeightBar(16)>18);
			Assert.AreEqual(2.0,WpfControls.UI.GridLook.GetSelectionBarWidth(1),1e-9);
			Assert.AreEqual(3/1.5,WpfControls.UI.GridLook.GetSelectionBarWidth(1.5),1e-9,"3 device pixels at 150%");
			System.Windows.Media.Color colorOverlay=WpfControls.UI.GridLook.GetOverlayOnWhite(UiTheme.GetColorWpf(EnumUiColor.RowHover));
			Color colorOnWhite=BlendOnWhite(colorOverlay);
			AssertNear(UiTheme.GetColor(EnumUiColor.RowHover),colorOnWhite,1,"the hover layer over white");
			Assert.AreEqual(UiTheme.GetColorWpf(EnumUiColor.AccentSubtle),WpfControls.UI.GridLook.GetSelectionColor(System.Windows.Media.Color.FromRgb(205,220,235)));
			Assert.AreEqual(System.Windows.Media.Colors.DarkSalmon,WpfControls.UI.GridLook.GetSelectionColor(System.Windows.Media.Colors.DarkSalmon));
		}
		#endregion WPF Grid

		#region India
		[TestMethod]
		public void GridFonts_DrawTheRupeeSign_SoTheIndiaArialSwitchIsGone() {
			//Plan section 5.1: GridOD switched its screen fonts to Arial in India; the likely reason was the rupee sign (U+20B9). Segoe UI (UI-2b) and
			//today's Microsoft Sans Serif both have it, so the screen switch is gone; the print fonts keep theirs (printouts keep their look).
			foreach(string family in new[] { UiTheme.FONT_FAMILY,UiTheme.FONT_FAMILY_SEMIBOLD,UiTheme.FONT_FAMILY_TODAY }) {
				System.Windows.Media.Typeface typeface=new System.Windows.Media.Typeface(family);
				Assert.IsTrue(typeface.TryGetGlyphTypeface(out System.Windows.Media.GlyphTypeface glyphTypeface),family+" is not installed.");
				Assert.IsTrue(glyphTypeface.CharacterToGlyphMap.ContainsKey(0x20B9),family+" has no rupee sign.");
			}
			string code=File.ReadAllText(Path.Combine(RepoPaths.Root(),@"OpenPmsDentalBusiness\UI\GridOD.cs"));
			Assert.IsFalse(code.Contains("_fontCell=new Font(\"Arial\""),"GridOD's screen font still switches to Arial.");
			StringAssert.Contains(code,"_fontPrintCell=new Font(\"Arial\"","GridOD's print font must keep its switch.");
		}
		#endregion India

		#region Gallery
		[TestMethod]
		public void Gallery_GridStrips_WinFormsBesideWpf() {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_GRID_GALLERY");
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: OPMS_UI_GRID_GALLERY names the folder for the grid strips of G-UI1 part 2 and G-UI2.");
			}
			bool rendersWpf=false;
			IconRendererTests.RunSta(() => {
				rendersWpf=ControlPictures.RendersWpf();
				//G-UI1 part 2: at each scale, the grid with focus (the focused row outlined) and without, WinForms above WPF.
				foreach(float scale in ControlStateGallery.ArrayScales) {
					List<Bitmap> listWinForms=new List<Bitmap>();
					List<Bitmap> listWpf=new List<Bitmap>();
					foreach(bool hasFocus in new[] { true,false }) {
						listWinForms.Add(RenderGridOD(hasFocus,hasFocus,scale,out Size sizeBefore,out Size sizeAfter));
						if(rendersWpf) {
							listWpf.Add(RenderGridWpf(hasFocus,hasFocus,scale));
						}
					}
					SaveStrip(Path.Combine(folder,"g-ui1-part2","scale-"+(int)Math.Round(scale*100),"grid.png"),new List<List<Bitmap>> { listWinForms,listWpf },scale);
				}
				//G-UI2: the WinForms grid with today's font, then with UI-2b's (rows 14 and 17 px); the WPF grid (its fonts do not change).
				List<Bitmap> listFonts=new List<Bitmap>();
				listFonts.Add(RenderGridOD(true,false,1f,out Size size1,out Size size2));
				UiTheme.SetFontForTest(UiFontOption.UI2b);
				try {
					listFonts.Add(RenderGridOD(true,false,1f,out size1,out size2));
				}
				finally {
					UiTheme.SetFontForTest(null);
				}
				List<Bitmap> listWpfFonts=rendersWpf ? new List<Bitmap> { RenderGridWpf(true,false,1f) } : new List<Bitmap>();
				SaveStrip(Path.Combine(folder,"g-ui2","grid-today-and-ui2b.png"),new List<List<Bitmap>> { listFonts,listWpfFonts },1f);
			});
			if(!rendersWpf) {
				Assert.Inconclusive("Wrote the WinForms rows only: WPF draws nothing in this session.");
			}
		}
		#endregion Gallery

		#region Helpers - rendering
		///<summary>A grid like the design sheet's: 360 x 200 at 100% (times the scale, which LayoutManager gives the grid as a zoom here, so its fonts
		///scale with it in a 96 DPI picture), the title, four columns, six rows, row 2 selected, row 3 coloured, row 4 hovered.</summary>
		public static Bitmap RenderGridOD(bool hasFocus,bool isFocusedRowShown,float scale,out Size sizeBefore,out Size sizeAfter) {
			using(StateGridOD grid=NewGridOD(scale)) {
				sizeBefore=grid.Size;
				grid.IsFocusForced=hasFocus;
				grid.IsFocusedRowForced=isFocusedRowShown;
				grid.SetSelected(INDEX_SELECTED,true);
				OpenPmsDental.UI.GridRow gridRowHover=grid.ListGridRows[INDEX_HOVER];
				int yHover=GetOriginY(grid,scale)+gridRowHover.State.YPos+gridRowHover.State.HeightMain/2;
				grid.HoverAt(yHover);
				Bitmap bitmap=Draw(grid);
				sizeAfter=grid.Size;
				return bitmap;
			}
		}

		private static StateGridOD NewGridOD(float scale) {
			StateGridOD grid=new StateGridOD();
			grid.TranslationName="";
			grid.Size=new Size(UiTheme.ScaleI(WIDTH_GRID,scale),UiTheme.ScaleI(HEIGHT_GRID,scale));
			grid.SetScaleAndZoom(1,scale);
			Fill(grid);
			return grid;
		}

		private static void Fill(OpenPmsDental.UI.GridOD grid) {
			grid.BeginUpdate();
			grid.Title="Account";
			grid.Columns.Clear();
			for(int c=0;c<_arrayHeadings.Length;c++) {
				grid.Columns.Add(new OpenPmsDental.UI.GridColumn(_arrayHeadings[c],_arrayWidths[c],c==3 ? HorizontalAlignment.Right : HorizontalAlignment.Left));
			}
			grid.ListGridRows.Clear();
			grid.ListGridRows.AddRange(NewRows());
			grid.EndUpdate();
		}

		private static void Fill(OpenPmsDental.UI.GridOld grid) {
			grid.BeginUpdate();
			grid.Title="Progress Notes";
			grid.Columns.Clear();
			for(int c=0;c<_arrayHeadings.Length;c++) {
				grid.Columns.Add(new OpenPmsDental.UI.GridColumn(_arrayHeadings[c],_arrayWidths[c],c==3 ? HorizontalAlignment.Right : HorizontalAlignment.Left));
			}
			grid.ListGridRows.Clear();
			grid.ListGridRows.AddRange(NewRows());
			grid.EndUpdate();
		}

		private static List<OpenPmsDental.UI.GridRow> NewRows() {
			List<OpenPmsDental.UI.GridRow> listGridRows=new List<OpenPmsDental.UI.GridRow>();
			for(int r=0;r<_arrayRows.Length;r++) {
				OpenPmsDental.UI.GridRow gridRow=new OpenPmsDental.UI.GridRow(_arrayRows[r]);
				if(r==INDEX_COLOURED) {
					gridRow.ColorBackG=_colorRowProgram;
				}
				listGridRows.Add(gridRow);
			}
			return listGridRows;
		}

		///<summary>GridOD's OriginY (below the title and header) at the scale, from the heights the font tokens give.</summary>
		private static int GetOriginY(OpenPmsDental.UI.GridOD grid,float scale) {
			return (int)Math.Round((OpenPmsDental.UI.GridScreenLook.GetHeightTitle96()+OpenPmsDental.UI.GridScreenLook.GetHeightHeader96())*scale);
		}

		private static Bitmap Draw(Control control) {
			Bitmap bitmap=new Bitmap(control.Width,control.Height,PixelFormat.Format32bppArgb);
			control.DrawToBitmap(bitmap,new Rectangle(0,0,control.Width,control.Height));
			return bitmap;
		}

		///<summary>The WPF grid like the WinForms one (360 x 200 DIP), rendered at the scale.</summary>
		public static Bitmap RenderGridWpf(bool hasFocus,bool isFocusedRowShown,float scale) {
			WpfControls.UI.Grid grid=new WpfControls.UI.Grid();
			grid.Width=WIDTH_GRID;
			grid.Height=HEIGHT_GRID;
			grid.Measure(new System.Windows.Size(WIDTH_GRID,HEIGHT_GRID));
			grid.Arrange(new System.Windows.Rect(0,0,WIDTH_GRID,HEIGHT_GRID));
			grid.UpdateLayout();
			grid.BeginUpdate();
			grid.Title="Account";
			grid.Columns.Clear();
			for(int c=0;c<_arrayHeadings.Length;c++) {
				grid.Columns.Add(new WpfControls.UI.GridColumn(_arrayHeadings[c],_arrayWidths[c],
					c==3 ? System.Windows.HorizontalAlignment.Right : System.Windows.HorizontalAlignment.Left));
			}
			grid.ListGridRows.Clear();
			for(int r=0;r<_arrayRows.Length;r++) {
				WpfControls.UI.GridRow gridRow=new WpfControls.UI.GridRow();
				foreach(string text in _arrayRows[r]) {
					gridRow.Cells.Add(text);
				}
				if(r==INDEX_COLOURED) {
					gridRow.ColorBackG=System.Windows.Media.Color.FromRgb(_colorRowProgram.R,_colorRowProgram.G,_colorRowProgram.B);
				}
				grid.ListGridRows.Add(gridRow);
			}
			grid.EndUpdate();
			grid.SetFocusLookForTest(hasFocus,isFocusedRowShown,scale);
			grid.SetSelected(INDEX_SELECTED,true);
			StateGridWpf.HoverRow(grid,INDEX_HOVER);
			grid.UpdateLayout();
			return ControlPictures.RenderWpf(grid,WIDTH_GRID,HEIGHT_GRID,scale);
		}

		///<summary>Cells of each row side by side on Surface, the rows one above the other. Disposes the cells.</summary>
		private static void SaveStrip(string path,List<List<Bitmap>> listRowsAll,float scale) {
			int gap=UiTheme.ScaleI(8,scale);
			List<List<Bitmap>> listRows=listRowsAll.Where(x => x.Count>0).ToList();
			int width=listRows.Max(x => x.Sum(y => y.Width)+gap*(x.Count+1));
			int height=listRows.Sum(x => x.Max(y => y.Height))+gap*(listRows.Count+1);
			using(Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb))
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(UiTheme.GetColorLight(EnumUiColor.Surface));
				int y=gap;
				foreach(List<Bitmap> listRow in listRows) {
					int x=gap;
					foreach(Bitmap bitmapCell in listRow) {
						g.DrawImageUnscaled(bitmapCell,x,y);
						x+=bitmapCell.Width+gap;
					}
					y+=listRow.Max(b => b.Height)+gap;
				}
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				PngWriter.Save(bitmap,path);
			}
			foreach(Bitmap bitmapCell in listRowsAll.SelectMany(x => x)) {
				bitmapCell.Dispose();
			}
		}
		#endregion Helpers - rendering

		#region Helpers - checks
		///<summary>Reads a WinForms grid picture at 100% down the probe column and checks every part against the tokens and the expected heights.</summary>
		private static void CheckWinForms(List<string> listFailures,string name,Bitmap bitmap,bool hasFocus,bool isFocusedRowShown,int heightTitle,
			int heightHeader,int heightRow,bool isHover)
		{
			Color colorAccent=UiTheme.GetColor(EnumUiColor.Accent);
			Color colorStroke=UiTheme.GetColor(EnumUiColor.Stroke);
			//The outer border: Stroke all around.
			Check(listFailures,name+" border left",bitmap,0,HEIGHT_GRID/2,colorStroke,0);
			Check(listFailures,name+" border top",bitmap,X_PROBE,0,colorStroke,0);
			Check(listFailures,name+" border right",bitmap,bitmap.Width-1,HEIGHT_GRID/2,colorStroke,0);
			//The title: flat Accent from y 1 for heightTitle rows, white text in the middle of it.
			int countTitle=CountRun(bitmap,X_PROBE,1,0,1,colorAccent);
			if(countTitle!=heightTitle) {
				listFailures.Add(name+": the title is "+countTitle+" px tall, expected "+heightTitle+".");
			}
			if(!HasColorNear(bitmap,new Rectangle(1,1,bitmap.Width-2,heightTitle),UiTheme.GetColor(EnumUiColor.TextOnAccent),40)) {
				listFailures.Add(name+": the title has no white text.");
			}
			//The header: SurfaceSubtle, then its Stroke bottom line; a Stroke separator at each column start but the first.
			int yHeader=1+heightTitle;
			int countHeader=CountRun(bitmap,X_PROBE,yHeader,0,1,UiTheme.GetColor(EnumUiColor.SurfaceSubtle));
			if(countHeader+1!=heightHeader) {
				listFailures.Add(name+": the header is "+(countHeader+1)+" px tall with its line, expected "+heightHeader+".");
			}
			int yHeaderLine=yHeader+countHeader;
			Check(listFailures,name+" header bottom line",bitmap,X_PROBE,yHeaderLine,colorStroke,0);
			Check(listFailures,name+" header separator",bitmap,_arrayWidths[0],yHeader+2,colorStroke,0);
			if(!HasColorNear(bitmap,new Rectangle(1,yHeader,bitmap.Width-2,countHeader),UiTheme.GetColor(EnumUiColor.Text),60)) {
				listFailures.Add(name+": the header has no text in the Text colour.");
			}
			//The rows: each heightRow px from one gridline to the next.
			for(int r=0;r<_arrayRows.Length;r++) {
				int yTop=yHeaderLine+r*heightRow;//the gridline above the row
				int yMiddle=yTop+heightRow/2;
				string nameRow=name+" row "+r;
				Color colorFill=UiTheme.GetColor(EnumUiColor.SurfaceRaised);
				if(r==INDEX_SELECTED) {
					colorFill=UiTheme.GetColor(EnumUiColor.AccentSubtle);
				}
				else if(r==INDEX_COLOURED) {
					colorFill=_colorRowProgram;
				}
				else if(r==INDEX_HOVER && isHover) {
					colorFill=UiTheme.GetColor(EnumUiColor.RowHover);
				}
				Check(listFailures,nameRow+" fill",bitmap,X_PROBE,yMiddle,colorFill,0);
				Color colorLower=r==_arrayRows.Length-1 ? colorStroke : UiTheme.GetColor(EnumUiColor.Gridline);
				Check(listFailures,nameRow+" lower line",bitmap,X_PROBE,yTop+heightRow,colorLower,0);
				Check(listFailures,nameRow+" column line",bitmap,_arrayWidths[0],yMiddle,UiTheme.GetColor(EnumUiColor.Gridline),0);
				if(r!=INDEX_SELECTED) {
					Check(listFailures,nameRow+" no bar",bitmap,1,yMiddle,colorFill,0);
					continue;
				}
				//The selection bar: 2 px inside the row's first pixels (x 1 and 2, after the border), Accent with focus, SelectionBarNoFocus without.
				Color colorBar=UiTheme.GetColor(hasFocus ? EnumUiColor.Accent : EnumUiColor.SelectionBarNoFocus);
				Check(listFailures,nameRow+" bar x1",bitmap,1,yMiddle,colorBar,0);
				Check(listFailures,nameRow+" bar x2",bitmap,2,yMiddle,colorBar,0);
				//The focused row's outline: Accent on the row's first line inside it, or the fill.
				Check(listFailures,nameRow+" focused outline",bitmap,X_PROBE,yTop+1,isFocusedRowShown ? colorAccent : colorFill,0);
				Check(listFailures,nameRow+" focused outline bottom",bitmap,X_PROBE,yTop+heightRow-1,isFocusedRowShown ? colorAccent : colorFill,0);
			}
			//No cell text starts under the selection bar (section 3.2: the bar sits in the row's first 2 px, after the border): in the plain first
			//row, the first dark pixel of the first column's text is right of it.
			int xInkFirst=-1;
			for(int x=1;x<_arrayWidths[0] && xInkFirst==-1;x++) {
				for(int y=yHeaderLine+1;y<yHeaderLine+heightRow;y++) {
					Color colorPixel=bitmap.GetPixel(x,y);
					if(colorPixel.R+colorPixel.G+colorPixel.B<3*160) {
						xInkFirst=x;
						break;
					}
				}
			}
			if(xInkFirst!=-1 && xInkFirst<1+UiTheme.GetFocusWidth(1f)) {
				listFailures.Add(name+": the first column's text starts at x "+xInkFirst+", under the selection bar (x 1 to "+UiTheme.GetFocusWidth(1f)+").");
			}
			//The text: dark Text-coloured glyphs in the rows.
			int yRows=yHeaderLine+1;
			if(!HasColorNear(bitmap,new Rectangle(1,yRows,bitmap.Width-2,heightRow*2),UiTheme.GetColor(EnumUiColor.Text),60)) {
				listFailures.Add(name+": the rows have no text in the Text colour.");
			}
			//Below the rows: SurfaceRaised.
			int yBelow=yHeaderLine+_arrayRows.Length*heightRow+3;
			if(yBelow<bitmap.Height-2) {
				Check(listFailures,name+" below the rows",bitmap,X_PROBE,yBelow,UiTheme.GetColor(EnumUiColor.SurfaceRaised),0);
			}
		}

		///<summary>Reads a WPF grid picture at 100%: border, title (18), header (15 with its line), rows (17), hover, selection, bar, outline.</summary>
		private static void CheckWpf(List<string> listFailures,string name,Bitmap bitmap,bool hasFocus,bool isFocusedRowShown) {
			const int tol=3;
			Color colorStroke=UiTheme.GetColor(EnumUiColor.Stroke);
			Check(listFailures,name+" border left",bitmap,0,HEIGHT_GRID/2,colorStroke,tol);
			Check(listFailures,name+" border top",bitmap,X_PROBE,0,colorStroke,tol);
			int countTitle=CountRun(bitmap,X_PROBE,1,0,1,UiTheme.GetColor(EnumUiColor.Accent),tol);
			if(countTitle!=18) {
				listFailures.Add(name+": the title is "+countTitle+" px tall, expected 18.");
			}
			if(!HasColorNear(bitmap,new Rectangle(1,1,bitmap.Width-2,18),UiTheme.GetColor(EnumUiColor.TextOnAccent),40)) {
				listFailures.Add(name+": the title has no white text.");
			}
			int countHeader=CountRun(bitmap,X_PROBE,19,0,1,UiTheme.GetColor(EnumUiColor.SurfaceSubtle),tol);
			if(countHeader+1!=15) {
				listFailures.Add(name+": the header is "+(countHeader+1)+" px tall with its line, expected 15.");
			}
			Check(listFailures,name+" header bottom line",bitmap,X_PROBE,19+countHeader,colorStroke,tol);
			int yRows=34;//border 1, title 18, header 15
			const int heightRow=17;//UQ-2: the WPF rows are already 17 DIP (Segoe UI 11.5)
			for(int r=0;r<_arrayRows.Length;r++) {
				int yTop=yRows+r*heightRow;
				int yMiddle=yTop+heightRow/2;
				string nameRow=name+" row "+r;
				Color colorFill=UiTheme.GetColor(EnumUiColor.SurfaceRaised);
				if(r==INDEX_SELECTED) {
					colorFill=UiTheme.GetColor(EnumUiColor.AccentSubtle);
				}
				else if(r==INDEX_COLOURED) {
					colorFill=_colorRowProgram;
				}
				else if(r==INDEX_HOVER) {
					colorFill=UiTheme.GetColor(EnumUiColor.RowHover);
				}
				Check(listFailures,nameRow+" fill",bitmap,X_PROBE,yMiddle,colorFill,tol);
				if(r!=INDEX_SELECTED) {
					Check(listFailures,nameRow+" no bar",bitmap,1,yMiddle,colorFill,tol);
					continue;
				}
				Color colorBar=UiTheme.GetColor(hasFocus ? EnumUiColor.Accent : EnumUiColor.SelectionBarNoFocus);
				Check(listFailures,nameRow+" bar x1",bitmap,1,yMiddle,colorBar,tol);
				Check(listFailures,nameRow+" bar x2",bitmap,2,yMiddle,colorBar,tol);
				Check(listFailures,nameRow+" after the bar",bitmap,3,yTop+2,colorFill,tol);
				if(isFocusedRowShown) {
					Check(listFailures,nameRow+" focused outline",bitmap,X_PROBE,yTop,UiTheme.GetColor(EnumUiColor.Accent),tol);
				}
				else if(IsNear(bitmap.GetPixel(X_PROBE,yTop),UiTheme.GetColor(EnumUiColor.Accent),40)) {
					//The row's top pixel shares WPF's antialiased gridline of the row above, so without the outline it is only "not Accent".
					listFailures.Add(nameRow+": an outline without the focused row's look.");
				}
			}
		}

		private static int GetRowMiddleY(Bitmap bitmap,int indexRow,int heightTitle,int heightHeader,int heightRow) {
			return heightTitle+heightHeader+indexRow*heightRow+heightRow/2;
		}

		///<summary>The first y down the column x whose pixel is the colour, or -1.</summary>
		private static int FindRowOfColor(Bitmap bitmap,int x,Color color) {
			for(int y=0;y<bitmap.Height;y++) {
				if(IsNear(bitmap.GetPixel(x,y),color,0)) {
					return y;
				}
			}
			return -1;
		}

		///<summary>How many pixels from (x,y) in steps of (dx,dy) are the colour.</summary>
		private static int CountRun(Bitmap bitmap,int x,int y,int dx,int dy,Color color,int tolerance=0) {
			int count=0;
			while(x>=0 && y>=0 && x<bitmap.Width && y<bitmap.Height && IsNear(bitmap.GetPixel(x,y),color,tolerance)) {
				count++;
				x+=dx;
				y+=dy;
			}
			return count;
		}

		///<summary>Whether a pixel in the rectangle is within tolerance of the colour.</summary>
		private static bool HasColorNear(Bitmap bitmap,Rectangle rectangle,Color color,int tolerance) {
			rectangle.Intersect(new Rectangle(0,0,bitmap.Width,bitmap.Height));
			for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
				for(int x=rectangle.Left;x<rectangle.Right;x++) {
					if(IsNear(bitmap.GetPixel(x,y),color,tolerance)) {
						return true;
					}
				}
			}
			return false;
		}

		private static void Check(List<string> listFailures,string name,Bitmap bitmap,int x,int y,Color expected,int tolerance) {
			if(x<0 || y<0 || x>=bitmap.Width || y>=bitmap.Height) {
				listFailures.Add(name+": ("+x+","+y+") is outside the picture.");
				return;
			}
			Color actual=bitmap.GetPixel(x,y);
			if(!IsNear(actual,expected,tolerance)) {
				listFailures.Add(name+": ("+x+","+y+") is "+Hex(actual)+", expected "+Hex(expected)+".");
			}
		}

		private static void AssertNear(Color expected,Color actual,int tolerance,string what) {
			Assert.IsTrue(IsNear(actual,expected,tolerance),what+": "+Hex(actual)+", expected "+Hex(expected)+".");
		}

		private static bool IsNear(Color actual,Color expected,int tolerance) {
			return Math.Abs(actual.R-expected.R)<=tolerance && Math.Abs(actual.G-expected.G)<=tolerance && Math.Abs(actual.B-expected.B)<=tolerance;
		}

		private static Color BlendOnWhite(System.Windows.Media.Color color) {
			double a=color.A/255.0;
			return Color.FromArgb((int)Math.Round(color.R*a+255*(1-a)),(int)Math.Round(color.G*a+255*(1-a)),(int)Math.Round(color.B*a+255*(1-a)));
		}

		private static string Hex(Color color) {
			return "#"+color.R.ToString("X2")+color.G.ToString("X2")+color.B.ToString("X2");
		}

		private static void AssertNoFailures(List<string> listFailures) {
			Assert.AreEqual(0,listFailures.Count,listFailures.Count+" differences:\r\n"+string.Join("\r\n",listFailures.Take(60)));
		}
		#endregion Helpers - checks
	}
}
