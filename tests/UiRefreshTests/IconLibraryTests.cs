// Tests for package UI-7b (docs/specs/ui-refresh-plan.md, sections 3.5, 4.2 and 4.3, and section 7's UI-7b row; docs/port-notes/UI-7b.md): the
// EnumIcons of both IconLibrary folders draw Fluent icons from the one icon table. Every EnumIcons has a manifest row (but Chart32G, Chart32W and
// ChartMed32, which draw the custom dental icons Tooth, ToothFilled and ChartMedical since UI-8) and draws non-empty at every size through the
// libraries' own calls on GDI+, Direct2D and WPF, on the OnlyGDI path (NoD2D.txt) and in NoDpi mode; the module bar's icons have their Filled
// style; the role colours of UQ-4; the Gen_*.cs files are all gone (the three chart pictures' since UI-8). Everything is drawn off screen; no window. The Direct2D tests need Direct2dWrapper.dll (the full build).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.Scan;
using WinEnumIcons=OpenPmsDental.UI.EnumIcons;
using WinIconLibrary=OpenPmsDental.UI.IconLibrary;
using WpfEnumIcons=WpfControls.UI.EnumIcons;
using WpfIconLibrary=WpfControls.UI.IconLibrary;

namespace UiRefreshTests {
	[TestClass]
	public class IconLibraryTests {
		///<summary>The module bar's chart icons, which have no EnumIcons row and draw a custom dental icon since UI-8 (section 4.4), with the dental
		///icon each draws.</summary>
		private static readonly Dictionary<string,DentalIcon> _dictDentalIcons=new Dictionary<string,DentalIcon> {
			{ "Chart32G",DentalIcon.Tooth },{ "Chart32W",DentalIcon.ToothFilled },{ "ChartMed32",DentalIcon.ChartMedical },
		};
		///<summary>The module bar's icons, which have a Filled row (section 3.6: the selected module).</summary>
		private static readonly string[] _arrayModules=new[] { "Appt32","Family32","Account32","TreatPlan32","TreatPlanMed32","Imaging32","Manage32" };
		///<summary>UQ-4: the few role colours; every other icon is Text.</summary>
		private static readonly Dictionary<string,string> _dictRoles=new Dictionary<string,string> {
			{ "Add","Accent" },{ "Complete","Success" },{ "DeleteX","Danger" },{ "PatDelete","Danger" },{ "BreakAptX","Danger" },
		};

		private static IEnumerable<WinEnumIcons> WinIcons() {
			return Enum.GetValues(typeof(WinEnumIcons)).Cast<WinEnumIcons>().Where(x => x!=WinEnumIcons.None);
		}

		private static IEnumerable<WpfEnumIcons> WpfIcons() {
			return Enum.GetValues(typeof(WpfEnumIcons)).Cast<WpfEnumIcons>().Where(x => x!=WpfEnumIcons.None);
		}

		private static int[] InkOnWhite(Action<Graphics,Rectangle> actionDraw,int size) {
			using(Bitmap bitmap=new Bitmap(size,size,PixelFormat.Format32bppArgb)) {
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.White);
					actionDraw(g,new Rectangle(0,0,size,size));
				}
				return IconRendererTests.GetInk(bitmap,isOnWhite:true);
			}
		}

		#region Manifest
		[TestMethod]
		public void Manifest_EveryEnumIconsHasARow_TheModulesAFilledRow_TheRolesOfUq4() {
			Assert.AreEqual(92,WinIcons().Count(),"The WinForms EnumIcons: UI-7b's 38, UI-9a-1's 10 Sync (owner, 2026-11-18) and UI-9a-3's 6, UI-9a-4's 1 and UI-9a-5's 4 (Copy, Paste, ExportExcel, QueryView) and UI-9a-6's 14 (PinOff, Calculator, Backup, Deposit, Insurance, TaskList, Move, TableDeleteRow, TableDeleteColumn, TableInsertRow, TableInsertColumn, ChartAdd, Pen, Signature), and UI-9a-7's 18 (the report designer's toolbar).");
			Assert.AreEqual(103,WpfIcons().Count(),"The WPF EnumIcons: UI-7b's 53, UI-9a-1's 8 Sync (owner, 2026-11-18) and UI-9a-3's 6 and UI-9a-4's 1 and UI-9a-5's 2 and UI-9a-6's 14 and UI-9a-7's 18.");
			List<IconRendererTests.ManifestRow> listRows=IconRendererTests.ReadManifest().Where(x => x.EnumName=="EnumIcons").ToList();
			HashSet<string> hashSetMembers=new HashSet<string>(WinIcons().Select(x => x.ToString()).Concat(WpfIcons().Select(x => x.ToString())));
			Assert.AreEqual(103,hashSetMembers.Count,"Every WinForms member is also a WPF member.");
			foreach(string member in hashSetMembers) {
				IconRendererTests.ManifestRow manifestRow=listRows.SingleOrDefault(x => x.Name==member);
				if(_dictDentalIcons.ContainsKey(member)) {
					Assert.IsNull(manifestRow,member+" draws the dental icon "+_dictDentalIcons[member]+" (UI-8) and has no EnumIcons row.");
					continue;
				}
				Assert.IsNotNull(manifestRow,"EnumIcons."+member+" has no row in icons/manifest.json.");
				Assert.AreEqual("regular",manifestRow.Style,member+": Regular for everything (section 4.3).");
				Assert.IsTrue(!string.IsNullOrEmpty(manifestRow.Fluent),member+" is a Fluent icon.");
				string role;
				if(!_dictRoles.TryGetValue(member,out role)) {
					role="Text";
				}
				Assert.AreEqual(role,manifestRow.Role,member+": the role colour of UQ-4.");
			}
			foreach(IconRendererTests.ManifestRow manifestRow in listRows) {
				if(hashSetMembers.Contains(manifestRow.Name)) {
					continue;
				}
				Assert.IsTrue(manifestRow.Name.EndsWith(WinIconLibrary.FILLED_SUFFIX),manifestRow+" is no EnumIcons member and no Filled row.");
				string member=manifestRow.Name.Substring(0,manifestRow.Name.Length-WinIconLibrary.FILLED_SUFFIX.Length);
				Assert.IsTrue(_arrayModules.Contains(member),manifestRow+": only the module bar's icons have a Filled row.");
				IconRendererTests.ManifestRow manifestRowRegular=listRows.Single(x => x.Name==member);
				Assert.AreEqual("filled",manifestRow.Style,manifestRow.ToString());
				Assert.AreEqual(manifestRowRegular.Fluent,manifestRow.Fluent,manifestRow+" is the Filled style of the same Fluent icon.");
				CollectionAssert.AreEqual(manifestRowRegular.Sizes,manifestRow.Sizes,manifestRow.ToString());
			}
			foreach(string module in _arrayModules) {
				Assert.IsTrue(listRows.Any(x => x.Name==module+WinIconLibrary.FILLED_SUFFIX),module+" has no Filled row.");
				//The module bar's 32 px slot draws the 32 design (section 3.5), never a smaller one.
				Assert.IsTrue(listRows.Single(x => x.Name==module).Sizes.Contains(32),module+" has no 32 px design.");
			}
		}

		[TestMethod]
		public void Libraries_ShareTheTable_TheChartIconsDrawTheirDentalIcons() {
			foreach(WinEnumIcons winEnumIcons in WinIcons()) {
				Assert.IsTrue(WinIconLibrary.IsVector(winEnumIcons),winEnumIcons.ToString());
				Assert.IsTrue(WinIconLibrary.GetDesigns(winEnumIcons).Length>0,winEnumIcons.ToString());
				DentalIcon dentalIcon;
				bool isDental=_dictDentalIcons.TryGetValue(winEnumIcons.ToString(),out dentalIcon);
				Assert.AreEqual(isDental ? dentalIcon : DentalIcon.None,WinIconLibrary.GetDentalIcon(winEnumIcons),winEnumIcons.ToString());
				if(isDental) {
					CollectionAssert.AreEqual(UiIcons.GetDesigns(dentalIcon),WinIconLibrary.GetDesigns(winEnumIcons),winEnumIcons+": the dental icon's designs.");
					CollectionAssert.AreEqual(UiIcons.GetDesigns(UiIcons.GetFilled(dentalIcon)),WinIconLibrary.GetDesigns(winEnumIcons,isFilled:true),
						winEnumIcons+": the dental icon's Filled twin.");
				}
				WpfEnumIcons wpfEnumIcons=(WpfEnumIcons)Enum.Parse(typeof(WpfEnumIcons),winEnumIcons.ToString());
				CollectionAssert.AreEqual(WinIconLibrary.GetDesigns(winEnumIcons),WpfIconLibrary.GetDesigns(wpfEnumIcons),winEnumIcons+": the same designs.");
				IconDesign[] arrayFilled=WinIconLibrary.GetDesigns(winEnumIcons,isFilled:true);
				CollectionAssert.AreEqual(arrayFilled,WpfIconLibrary.GetDesigns(wpfEnumIcons,isFilled:true),winEnumIcons+": the same Filled designs.");
				bool isModule=_arrayModules.Contains(winEnumIcons.ToString()) || (isDental && UiIcons.GetFilled(dentalIcon)!=dentalIcon);
				Assert.AreEqual(isModule,!arrayFilled.SequenceEqual(WinIconLibrary.GetDesigns(winEnumIcons)),
					winEnumIcons+": a Filled style of its own only for the module bar's icons (the others fall back to Regular; Chart32W is the Filled tooth).");
			}
			foreach(WpfEnumIcons wpfEnumIcons in WpfIcons()) {
				Assert.IsTrue(WpfIconLibrary.IsWPF(wpfEnumIcons),wpfEnumIcons.ToString());
				Assert.IsTrue(WpfIconLibrary.IsVector(wpfEnumIcons),wpfEnumIcons.ToString());
				DentalIcon dentalIcon;
				bool isDental=_dictDentalIcons.TryGetValue(wpfEnumIcons.ToString(),out dentalIcon);
				Assert.AreEqual(isDental ? dentalIcon : DentalIcon.None,WpfIconLibrary.GetDentalIcon(wpfEnumIcons),wpfEnumIcons.ToString());
			}
			Assert.AreEqual(0,WinIconLibrary.GetDesigns(WinEnumIcons.None).Length);
			Assert.AreEqual(0,WpfIconLibrary.GetDesigns(WpfEnumIcons.None).Length);
		}

		[TestMethod]
		public void GenFiles_AllGone() {
			//The plan: the 38 + 37 Gen_*.cs files go once their EnumIcons draw from the table (UI-7b); the three chart pictures' and IconSelector.cs
			//went when the chart icons began to draw the dental icons (UI-8).
			string root=RepoPaths.Root();
			CollectionAssert.AreEquivalent(new[] { "IconLibrary.cs" },
				Directory.GetFiles(Path.Combine(root,"OpenPmsDental","UI","IconLibrary")).Select(Path.GetFileName).ToArray());
			CollectionAssert.AreEquivalent(new[] { "DrawSvg.cs","IconLibrary.cs" },
				Directory.GetFiles(Path.Combine(root,"WpfControlsOD","UI","IconLibrary")).Select(Path.GetFileName).ToArray());
		}
		#endregion Manifest

		#region GDI+
		[TestMethod]
		public void Gdi_EveryEnumIconsDrawsAtEverySize_InItsRoleColour_DisabledInTextDisabled() {
			IconRendererTests.RunSta(() => {
				foreach(WinEnumIcons winEnumIcons in WinIcons()) {
					foreach(int size in IconRendererTests.ArraySlotSizes) {
						//designMode: the GDI+ fill, as the designer and OnlyGDI draw.
						int[] arrayInk=InkOnWhite((g,rectangle) => WinIconLibrary.Draw(g,winEnumIcons,rectangle,Color.White,Color.White,designMode:true),size);
						Assert.IsTrue(arrayInk.Any(x => x>=64),winEnumIcons+" draws nothing on GDI+ at "+size+" px.");
						int[] arrayInkDisabled=InkOnWhite((g,rectangle) => WinIconLibrary.DrawDisabled(g,winEnumIcons,rectangle),size);
						Assert.IsTrue(arrayInkDisabled.Any(x => x>=16),winEnumIcons+" draws nothing disabled at "+size+" px.");
					}
					//The colour: a 24 px drawing on a transparent bitmap holds the role colour at full alpha.
					Color colorRole=WinIconLibrary.GetColor(winEnumIcons);
					Assert.AreEqual(UiIcons.GetRoleColor(WinIconLibrary.GetDesigns(winEnumIcons)[0]).ToArgb(),colorRole.ToArgb());
					AssertDrawsInColour(g => WinIconLibrary.Draw(g,winEnumIcons,new Rectangle(0,0,24,24),Color.White,Color.White,designMode:true),colorRole,
						winEnumIcons.ToString());
					AssertDrawsInColour(g => WinIconLibrary.DrawDisabled(g,winEnumIcons,new Rectangle(0,0,24,24)),UiTheme.GetColor(EnumUiColor.TextDisabled),
						winEnumIcons+" disabled");
				}
			});
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Accent).ToArgb(),WinIconLibrary.GetColor(WinEnumIcons.Add).ToArgb());
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Danger).ToArgb(),WinIconLibrary.GetColor(WinEnumIcons.DeleteX).ToArgb());
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Success).ToArgb(),WinIconLibrary.GetColor(WinEnumIcons.Complete).ToArgb());
			Assert.AreEqual(UiTheme.GetColor(EnumUiColor.Text).ToArgb(),WinIconLibrary.GetColor(WinEnumIcons.Patient).ToArgb());
		}

		///<summary>The drawing on a transparent 24 px bitmap has pixels of exactly the colour (the solid inside of the shape).</summary>
		private static void AssertDrawsInColour(Action<Graphics> actionDraw,Color color,string what) {
			using(Bitmap bitmap=new Bitmap(24,24,PixelFormat.Format32bppArgb)) {
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.Transparent);
					actionDraw(g);
				}
				//On a transparent bitmap GDI+ keeps the brush's colour in every drawn pixel and puts the coverage in the alpha (a thin stroke may cover no
				//pixel fully), so every clearly drawn pixel has the colour, give or take rounding.
				List<Color> listDrawn=IconRendererTests.GetPixels(bitmap).Select(x => Color.FromArgb(x)).Where(x => x.A>=128).ToList();
				Assert.IsTrue(listDrawn.Count>0,what+" draws nothing.");
				Color colorOther=listDrawn.FirstOrDefault(x => Math.Abs(x.R-color.R)>2 || Math.Abs(x.G-color.G)>2 || Math.Abs(x.B-color.B)>2);
				Assert.IsTrue(colorOther.IsEmpty,what+" has a pixel of "+colorOther+", not "+color+".");
			}
		}

		[TestMethod]
		public void Gdi_DrawColored_DrawsTheFilledStyleInTheCallersColour() {
			IconRendererTests.RunSta(() => {
				bool isOnlyGdiOld=WinIconLibrary.OnlyGDI;
				WinIconLibrary.OnlyGDI=true;
				try {
					Color colorAccent=UiTheme.GetColor(EnumUiColor.Accent);
					foreach(string module in _arrayModules) {
						WinEnumIcons winEnumIcons=(WinEnumIcons)Enum.Parse(typeof(WinEnumIcons),module);
						AssertDrawsInColour(g => WinIconLibrary.DrawColored(g,winEnumIcons,new Rectangle(0,0,24,24),colorAccent,Color.White,isFilled:true),colorAccent,
							module+" Filled");
						int[] arrayInkRegular=InkOnWhite((g,rectangle) => WinIconLibrary.DrawColored(g,winEnumIcons,rectangle,Color.Black,Color.White),32);
						int[] arrayInkFilled=InkOnWhite((g,rectangle) => WinIconLibrary.DrawColored(g,winEnumIcons,rectangle,Color.Black,Color.White,isFilled:true),32);
						Assert.IsTrue(arrayInkFilled.Sum()>arrayInkRegular.Sum(),module+": the Filled style has more ink than the Regular.");
					}
				}
				finally {
					WinIconLibrary.OnlyGDI=isOnlyGdiOld;
				}
			});
		}

		[TestMethod]
		public void WinFormsDrawWpf_GivesAnImageOfTheIconsWidth() {
			IconRendererTests.RunSta(() => {
				foreach(WinEnumIcons winEnumIcons in WinIcons()) {
					System.Windows.Media.Imaging.BitmapImage bitmapImage=WinIconLibrary.DrawWpf(winEnumIcons);
					Assert.IsNotNull(bitmapImage,winEnumIcons.ToString());
					Assert.AreEqual(WinIconLibrary.Width(winEnumIcons),bitmapImage.PixelWidth,winEnumIcons.ToString());
					System.Windows.Media.Imaging.FormatConvertedBitmap formatConvertedBitmap=new System.Windows.Media.Imaging.FormatConvertedBitmap(bitmapImage,
						System.Windows.Media.PixelFormats.Bgra32,null,0);
					byte[] arrayBytes=new byte[formatConvertedBitmap.PixelWidth*formatConvertedBitmap.PixelHeight*4];
					formatConvertedBitmap.CopyPixels(arrayBytes,formatConvertedBitmap.PixelWidth*4,0);
					Assert.IsTrue(Enumerable.Range(0,arrayBytes.Length/4).Any(i => arrayBytes[i*4+3]>=128),winEnumIcons+": the WPF image is empty.");
				}
				Assert.IsNull(WinIconLibrary.DrawWpf(WinEnumIcons.None));
			});
		}
		#endregion GDI+

		#region Direct2D and OnlyGDI
		[TestMethod]
		public void Direct2d_EveryEnumIconsDrawsAtEverySize_AsTheOnlyGdiPathDoes() {
			IconRendererTests.RequireDirect2d();
			IconRendererTests.RunSta(() => {
				bool isOnlyGdiOld=WinIconLibrary.OnlyGDI;
				try {
					foreach(WinEnumIcons winEnumIcons in WinIcons()) {
						foreach(int size in IconRendererTests.ArraySlotSizes) {
							WinIconLibrary.OnlyGDI=false;
							int[] arrayInkDirect2d=InkOnWhite((g,rectangle) => WinIconLibrary.Draw(g,winEnumIcons,rectangle,Color.White,Color.White),size);
							Assert.IsFalse(WinIconLibrary.OnlyGDI,"Direct2D failed and IconLibrary fell back to GDI+.");
							Assert.IsTrue(arrayInkDirect2d.Any(x => x>=64),winEnumIcons+" draws nothing on Direct2D at "+size+" px.");
							WinIconLibrary.OnlyGDI=true;
							int[] arrayInkGdi=InkOnWhite((g,rectangle) => WinIconLibrary.Draw(g,winEnumIcons,rectangle,Color.White,Color.White),size);
							IconRendererTests.AssertSameShape(arrayInkDirect2d,arrayInkGdi,winEnumIcons+" at "+size+" px, Direct2D and OnlyGDI");
							//The Direct2D overload the program's Direct2D controls call.
							using(Bitmap bitmap=new Bitmap(size,size,PixelFormat.Format32bppArgb)) {
								using(OpenPmsDental.UI.Direct2d direct2d=OpenPmsDental.UI.Direct2d.FromBitmap(bitmap)) {
									direct2d.BeginDraw();
									direct2d.Clear(Color.White);
									WinIconLibrary.Draw(direct2d,winEnumIcons,new Rectangle(0,0,size,size));
									direct2d.EndDraw();
								}
								Assert.IsTrue(IconRendererTests.GetInk(bitmap,isOnWhite:true).Any(x => x>=64),winEnumIcons+" draws nothing with Draw(Direct2d) at "+size+" px.");
							}
						}
					}
				}
				finally {
					WinIconLibrary.OnlyGDI=isOnlyGdiOld;
				}
			});
		}
		#endregion Direct2D and OnlyGDI

		#region NoDpi
		[TestMethod]
		public void NoDpi_EveryEnumIconsDrawsTheSamePixels() {
			//With NoDpi.txt the program runs DPI-unaware and lays out at 96 DPI: a slot is its 96 DPI size. The icons are the same pixels as at 100% in
			//a DPI-aware thread, on GDI+ (OnlyGDI) and on Direct2D.
			bool isDirect2d=true;
			try {
				IconRendererTests.RequireDirect2d();
			}
			catch(AssertInconclusiveException) {
				isDirect2d=false;
			}
			IconRendererTests.RunSta(() => {
				bool isOnlyGdiOld=WinIconLibrary.OnlyGDI;
				try {
					foreach(bool isOnlyGdi in isDirect2d ? new[] { true,false } : new[] { true }) {
						WinIconLibrary.OnlyGDI=isOnlyGdi;
						foreach(WinEnumIcons winEnumIcons in WinIcons()) {
							foreach(int size in IconRendererTests.ArraySlotSizes) {
								int[] arrayAware=IconRendererTests.RunInDpiContext(IconRendererTests.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2,
									() => InkOnWhite((g,rectangle) => WinIconLibrary.Draw(g,winEnumIcons,rectangle,Color.White,Color.White),size));
								int[] arrayUnaware=IconRendererTests.RunInDpiContext(IconRendererTests.DPI_AWARENESS_CONTEXT_UNAWARE,
									() => InkOnWhite((g,rectangle) => WinIconLibrary.Draw(g,winEnumIcons,rectangle,Color.White,Color.White),size));
								CollectionAssert.AreEqual(arrayAware,arrayUnaware,winEnumIcons+" at "+size+" px, "+(isOnlyGdi ? "GDI+" : "Direct2D")+", DPI-aware and NoDpi");
							}
						}
					}
				}
				finally {
					WinIconLibrary.OnlyGDI=isOnlyGdiOld;
				}
			});
			if(!isDirect2d) {
				Assert.Inconclusive("GDI+ checked; the Direct2D half needs Direct2dWrapper.dll (the full build).");
			}
		}
		#endregion NoDpi

		#region WPF
		[TestMethod]
		public void Wpf_EveryEnumIconsAddsANonEmptyPathAtEverySize() {
			//Geometry only, which needs no rendering: it holds in a disconnected session too. The pixels of every design on WPF are checked by
			//IconRendererTests.Wpf_EveryManifestRowRendersAtEverySize_AsGdiDraws in a connected session.
			IconRendererTests.RunSta(() => {
				foreach(WpfEnumIcons wpfEnumIcons in WpfIcons()) {
					foreach(int size in IconRendererTests.ArraySlotSizes) {
						System.Windows.Controls.Grid grid=new System.Windows.Controls.Grid();
						grid.Width=size;
						grid.Height=size;
						WpfIconLibrary.DrawWpf(wpfEnumIcons,grid);
						Assert.AreEqual(1,grid.Children.Count,wpfEnumIcons.ToString());
						System.Windows.Shapes.Path path=grid.Children[0] as System.Windows.Shapes.Path;
						Assert.IsNotNull(path,wpfEnumIcons+" is not a Path.");
						Assert.IsTrue(path.Data.IsFrozen,wpfEnumIcons.ToString());
						Assert.IsFalse(path.Data.IsEmpty(),wpfEnumIcons+" has no geometry at "+size+" px.");
						System.Windows.Rect rectBounds=path.Data.Bounds;
						Assert.IsTrue(rectBounds.Width>size/4.0 || rectBounds.Height>size/4.0,wpfEnumIcons+" is tiny at "+size+" px: "+rectBounds);
						Assert.IsTrue(rectBounds.Left>=-0.01 && rectBounds.Top>=-0.01 && rectBounds.Right<=size+0.01 && rectBounds.Bottom<=size+0.01,
							wpfEnumIcons+" leaves its slot at "+size+" px: "+rectBounds);
						System.Windows.Media.SolidColorBrush solidColorBrush=path.Fill as System.Windows.Media.SolidColorBrush;
						Assert.IsNotNull(solidColorBrush,wpfEnumIcons.ToString());
						Color colorRole=UiIcons.GetRoleColor(WpfIconLibrary.GetDesigns(wpfEnumIcons)[0]);
						Assert.AreEqual(colorRole.ToArgb(),Color.FromArgb(solidColorBrush.Color.A,solidColorBrush.Color.R,solidColorBrush.Color.G,solidColorBrush.Color.B).ToArgb(),
							wpfEnumIcons+": the role colour.");
					}
				}
				//The Filled style and a brush of the caller's.
				System.Windows.Controls.Grid gridFilled=new System.Windows.Controls.Grid();
				gridFilled.Width=32;
				gridFilled.Height=32;
				WpfIconLibrary.DrawWpf(WpfEnumIcons.Appt32,gridFilled,System.Windows.Media.Brushes.Red,isFilled:true);
				System.Windows.Shapes.Path pathFilled=(System.Windows.Shapes.Path)gridFilled.Children[0];
				Assert.AreSame(System.Windows.Media.Brushes.Red,pathFilled.Fill);
				Assert.AreSame(UiIcons.GetGeometryWpf(UiIcons.GetDesign(WpfIconLibrary.GetDesigns(WpfEnumIcons.Appt32,isFilled:true),32),32,
					System.Windows.Media.VisualTreeHelper.GetDpi(gridFilled).PixelsPerDip),pathFilled.Data,"The Filled design's cached geometry.");
			});
		}
		#endregion WPF
	}
}
