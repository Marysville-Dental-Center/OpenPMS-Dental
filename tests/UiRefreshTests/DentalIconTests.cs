// Tests for package UI-8 (docs/specs/ui-refresh-plan.md, section 4.4 and section 7's UI-8 row; docs/port-notes/UI-8.md): the custom dental icons.
// Every icon of 4.4 has a Regular and a Filled manifest row of enum DentalIcon with a custom SVG at 16, 20, 24, 32 and 48 px; the SVGs are the
// drawing's output (tools/New-DentalIcons.ps1 -Check) and say they are the program's own; each icon draws non-empty at every slot of section
// 3.5 (the largest design that fits) on GDI+, Direct2D and WPF, on the OnlyGDI path (Direct2d.DrawIcon with NoD2D.txt) and in NoDpi mode, and
// the Filled twin has more ink; the default procedure buttons of db/init/new-database.sql carry the new pictures, 20x20 as the table stores them
// (UQ-7), and no code path gives them to an office's rows. IconRendererTests also draws every manifest row, these included.
// With OPMS_UI8_ICON_PICTURES set to a folder, DentalIconPictures_ForTheGallery writes gallery G-UI1 part 4's pictures there (before\ and
// after\, for tools/New-UiGallery.ps1). Everything is drawn off screen; no window, no database, no network. The Direct2D tests need
// Direct2dWrapper.dll (the full build) and are inconclusive without it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class DentalIconTests {
		///<summary>The folder gallery G-UI1 part 4's pictures are written to, when set.</summary>
		public const string VARIABLE_PICTURES="OPMS_UI8_ICON_PICTURES";
		///<summary>The icons of section 4.4, as their Regular member and file name stem, in manifest order.</summary>
		public static readonly string[][] ArrayIcons44=new string[][] {
			new[] { "Tooth","tooth" },new[] { "ChartMedical","chart_medical" },new[] { "PerioChart","perio_chart" },new[] { "PerioProbe","perio_probe" },
			new[] { "XrayBitewing","xray_bitewing" },new[] { "XrayPeriapical","xray_periapical" },new[] { "XrayPano","xray_pano" },
			new[] { "XrayCeph","xray_ceph" },new[] { "Sensor","sensor" },new[] { "PhosphorPlate","phosphor_plate" },new[] { "Mount","mount" },
			new[] { "IntraoralPhoto","intraoral_photo" },new[] { "Implant","implant" },new[] { "Ortho","ortho" },
			new[] { "ProcAmalgam","proc_amalgam" },new[] { "ProcComposite","proc_composite" },new[] { "ProcCrownPfm","proc_crown_pfm" },
			new[] { "ProcCrownCeramic","proc_crown_ceramic" },new[] { "ProcRct","proc_rct" },new[] { "ProcRctBuildUp","proc_rct_buildup" },
		};
		///<summary>The design sizes of every dental icon (4.4's 16, 20, 24 and 32 px grids, and 48 for the module bar at 150% and above).</summary>
		public static readonly int[] ArrayDesignSizes=new int[] { 16,20,24,32,48 };
		///<summary>The slots tested and the design section 3.5 picks for each: the largest design that fits, never stretched up.</summary>
		private static readonly int[][] _arraySlotDesigns=new int[][] {
			new[] { 12,16 },new[] { 16,16 },new[] { 20,20 },new[] { 22,20 },new[] { 24,24 },new[] { 28,24 },new[] { 32,32 },new[] { 40,32 },new[] { 48,48 },
			new[] { 64,48 },
		};
		///<summary>The line every custom SVG holds (tools/DentalIcons/DentalIconArt.cs, SVG_COMMENT).</summary>
		private const string SVG_MARK="Open PMS - Dental custom icon, original artwork, GPL-2.0-or-later";

		#region Helpers
		private static IEnumerable<DentalIcon> AllIcons() {
			return Enum.GetValues(typeof(DentalIcon)).Cast<DentalIcon>().Where(x => x!=DentalIcon.None);
		}

		///<summary>A white bitmap of the slot with the design filled in black with GDI+ (the OnlyGDI path's renderer).</summary>
		private static Bitmap DrawGdiOnWhite(IconDesign iconDesign,int sizeSlot) {
			Bitmap bitmap=new Bitmap(sizeSlot,sizeSlot,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);
				UiIcons.Draw(g,iconDesign,new Rectangle(0,0,sizeSlot,sizeSlot),Color.Black);
			}
			return bitmap;
		}

		///<summary>The ink of Direct2d.DrawIcon on a white bitmap, the slot at (3,2) of a slightly larger bitmap (as the program's controls call it).</summary>
		private static int[] DrawIconOnWhite(IconDesign iconDesign,int sizeSlot) {
			using(Bitmap bitmap=new Bitmap(sizeSlot+6,sizeSlot+4,PixelFormat.Format32bppArgb)) {
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.White);
					Direct2d.DrawIcon(g,iconDesign,new Rectangle(3,2,sizeSlot,sizeSlot),Color.Black,Color.White);
				}
				return IconRendererTests.GetInk(bitmap,isOnWhite:true);
			}
		}

		///<summary>A white bitmap of the slot with the design filled in black by the program's Direct2D wrapper.</summary>
		private static Bitmap DrawDirect2dOnWhite(IconDesign iconDesign,int sizeSlot) {
			Bitmap bitmap=new Bitmap(sizeSlot,sizeSlot,PixelFormat.Format32bppArgb);
			using(Direct2d direct2d=Direct2d.FromBitmap(bitmap)) {
				direct2d.BeginDraw();
				direct2d.Clear(Color.White);
				direct2d.FillIcon(iconDesign,new Rectangle(0,0,sizeSlot,sizeSlot),Color.Black);
				direct2d.EndDraw();
			}
			return bitmap;
		}

		///<summary>Runs a script of the repository's tools folder in Windows PowerShell; returns the exit code and the output.</summary>
		private static int RunTool(string scriptName,string arguments,out string output) {
			string script=Path.Combine(RepoPaths.Root(),"tools",scriptName);
			ProcessStartInfo processStartInfo=new ProcessStartInfo("powershell.exe","-NoProfile -ExecutionPolicy Bypass -File \""+script+"\" "+arguments);
			processStartInfo.UseShellExecute=false;
			processStartInfo.CreateNoWindow=true;
			processStartInfo.RedirectStandardOutput=true;
			processStartInfo.RedirectStandardError=true;
			using(Process process=Process.Start(processStartInfo)) {
				System.Threading.Tasks.Task<string> taskError=process.StandardError.ReadToEndAsync();
				string standardOutput=process.StandardOutput.ReadToEnd();
				Assert.IsTrue(process.WaitForExit(300000),scriptName+" did not end in 5 minutes.");
				output=standardOutput+taskError.Result;
				return process.ExitCode;
			}
		}

		///<summary>One default procedure button with a picture: its number and description in db/init/new-database.sql, its dental icon, and the
		///picture the file held before UI-8 (tests/UiRefreshTests/Data/procbutton-pictures-before.tsv).</summary>
		public sealed class ProcButtonPicture {
			public long ProcButtonNum;
			public string Description;
			public DentalIcon DentalIcon;
			public string PictureBefore;
		}

		public static List<ProcButtonPicture> ReadProcButtonPicturesBefore() {
			List<ProcButtonPicture> listPictures=new List<ProcButtonPicture>();
			foreach(string line in File.ReadAllLines(Path.Combine(RepoPaths.Root(),"tests","UiRefreshTests","Data","procbutton-pictures-before.tsv"))) {
				if(line.Trim()=="" || line.StartsWith("#")) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				Assert.AreEqual(4,arrayFields.Length,line);
				ProcButtonPicture procButtonPicture=new ProcButtonPicture();
				procButtonPicture.ProcButtonNum=long.Parse(arrayFields[0]);
				procButtonPicture.Description=arrayFields[1];
				procButtonPicture.DentalIcon=(DentalIcon)Enum.Parse(typeof(DentalIcon),arrayFields[2]);
				procButtonPicture.PictureBefore=arrayFields[3];
				listPictures.Add(procButtonPicture);
			}
			return listPictures;
		}

		///<summary>The rows of the procbutton INSERT of db/init/new-database.sql: number, description, the rest of the row, picture.</summary>
		public static List<string[]> ReadSeededProcButtons() {
			string text=File.ReadAllText(Path.Combine(RepoPaths.Root(),"db","init","new-database.sql"));
			Match matchInsert=Regex.Match(text,@"^INSERT INTO `procbutton` \(`ProcButtonNum`,`Description`,`ItemOrder`,`Category`,`ButtonImage`,`IsMultiVisit`\) VALUES\n(.*?);$",
				RegexOptions.Multiline|RegexOptions.Singleline);
			Assert.IsTrue(matchInsert.Success,"The procbutton INSERT of db/init/new-database.sql.");
			List<string[]> listRows=new List<string[]>();
			foreach(string line in matchInsert.Groups[1].Value.Split('\n')) {
				Match matchRow=Regex.Match(line,@"^\((\d+),'([^']*)',(\d+,\d+),'([A-Za-z0-9+/=]*)',(\d+)\),?$");
				Assert.IsTrue(matchRow.Success,"A procbutton row this test cannot read: "+line.Substring(0,Math.Min(120,line.Length)));
				listRows.Add(new[] { matchRow.Groups[1].Value,matchRow.Groups[2].Value,matchRow.Groups[3].Value+","+matchRow.Groups[5].Value,matchRow.Groups[4].Value });
			}
			return listRows;
		}

		///<summary>A picture from the database's base64, as a bitmap (as PIn.Bitmap reads it).</summary>
		private static Bitmap DecodePicture(string base64) {
			using(MemoryStream memoryStream=new MemoryStream(Convert.FromBase64String(base64))) {
				using(Image image=Image.FromStream(memoryStream)) {
					return new Bitmap(image);
				}
			}
		}
		#endregion Helpers

		#region Manifest and SVGs
		[TestMethod]
		public void Manifest_EveryIconOf44HasARegularRowThenItsFilledTwin_AndTheEnumFollowsTheRows() {
			List<IconRendererTests.ManifestRow> listRows=IconRendererTests.ReadManifest().Where(x => x.EnumName=="DentalIcon").ToList();
			Assert.AreEqual(ArrayIcons44.Length*2,listRows.Count,"A Regular and a Filled row for each icon of section 4.4.");
			for(int i=0;i<ArrayIcons44.Length;i++) {
				IconRendererTests.ManifestRow manifestRowRegular=listRows[i*2];
				IconRendererTests.ManifestRow manifestRowFilled=listRows[i*2+1];
				Assert.AreEqual(ArrayIcons44[i][0],manifestRowRegular.Name);
				Assert.AreEqual(ArrayIcons44[i][1],manifestRowRegular.Custom,manifestRowRegular.ToString());
				Assert.IsTrue(string.IsNullOrEmpty(manifestRowRegular.Fluent),manifestRowRegular+" is a custom icon.");
				Assert.AreEqual("regular",manifestRowRegular.Style,manifestRowRegular.ToString());
				Assert.AreEqual(ArrayIcons44[i][0]+UiIcons.DENTAL_FILLED_SUFFIX,manifestRowFilled.Name,"The Filled twin follows its Regular row.");
				Assert.AreEqual(ArrayIcons44[i][1]+"_filled",manifestRowFilled.Custom,manifestRowFilled.ToString());
				Assert.IsTrue(string.IsNullOrEmpty(manifestRowFilled.Fluent),manifestRowFilled+" is a custom icon.");
				Assert.AreEqual("filled",manifestRowFilled.Style,manifestRowFilled.ToString());
				foreach(IconRendererTests.ManifestRow manifestRow in new[] { manifestRowRegular,manifestRowFilled }) {
					CollectionAssert.AreEqual(ArrayDesignSizes,manifestRow.Sizes,manifestRow+": every design size.");
					Assert.AreEqual("Text",manifestRow.Role,manifestRow+": one colour, the text colour (UQ-4).");
				}
				DentalIcon dentalIcon=(DentalIcon)Enum.Parse(typeof(DentalIcon),manifestRowRegular.Name);
				Assert.AreEqual(manifestRowFilled.Name,UiIcons.GetFilled(dentalIcon).ToString());
				Assert.AreEqual(UiIcons.GetFilled(dentalIcon),UiIcons.GetFilled(UiIcons.GetFilled(dentalIcon)));
			}
			CollectionAssert.AreEqual(new[] { "None" }.Concat(listRows.Select(x => x.Name)).ToArray(),Enum.GetNames(typeof(DentalIcon)));
			Assert.AreEqual(0,(int)DentalIcon.None);
			Assert.AreEqual(0,UiIcons.GetDesigns(DentalIcon.None).Length);
			Assert.IsNull(UiIcons.GetDesign(DentalIcon.None,16));
			Assert.AreEqual(DentalIcon.None,UiIcons.GetFilled(DentalIcon.None));
			//Custom SVGs belong to DentalIcon rows only, and every file in icons/custom is in a row.
			List<IconRendererTests.ManifestRow> listAll=IconRendererTests.ReadManifest();
			Assert.IsTrue(listAll.Where(x => !string.IsNullOrEmpty(x.Custom)).All(x => x.EnumName=="DentalIcon"),"Only the dental icons are custom icons.");
			HashSet<string> hashSetFiles=new HashSet<string>(listRows.SelectMany(x => x.Sizes.Select(size => x.Custom+"_"+size+".svg")));
			string folderCustom=Path.Combine(RepoPaths.Root(),"icons","custom");
			CollectionAssert.AreEquivalent(hashSetFiles.ToArray(),Directory.GetFiles(folderCustom).Select(Path.GetFileName).ToArray());
		}

		[TestMethod]
		public void Svgs_AreTheDrawingsOutput_OriginalWork_OnePathEach() {
			string folderCustom=Path.Combine(RepoPaths.Root(),"icons","custom");
			foreach(string path in Directory.GetFiles(folderCustom,"*.svg")) {
				byte[] arrayBytes=File.ReadAllBytes(path);
				Assert.IsFalse(arrayBytes.Length>=3 && arrayBytes[0]==0xEF && arrayBytes[1]==0xBB,path+": no byte order mark.");
				string svg=Encoding.ASCII.GetString(arrayBytes);
				Assert.IsTrue(arrayBytes.All(x => x>=32 && x<127),path+": ASCII on one line, no line end (.gitattributes keeps the bytes).");
				int size=int.Parse(Regex.Match(Path.GetFileName(path),@"_(\d+)\.svg$").Groups[1].Value);
				StringAssert.StartsWith(svg,"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\""+size+"\" height=\""+size+"\" viewBox=\"0 0 "+size+" "+size+"\">",path);
				StringAssert.Contains(svg,"<!-- "+SVG_MARK,path+" says it is the program's own work.");
				Assert.AreEqual(1,Regex.Matches(svg,"<path ").Count,path+": one path.");
				Assert.IsTrue(Regex.IsMatch(svg,"<path d=\"M [-0-9. MLCZ]+\"/></svg>$"),path+": absolute M, L, C and Z only.");
			}
			int exitCode=RunTool("New-DentalIcons.ps1","-Check",out string output);
			Assert.AreEqual(0,exitCode,output);
			StringAssert.Contains(output,"is up to date (200 designs)");
		}

		[TestMethod]
		public void Readme_SaysTheCustomIconsAreOriginal() {
			string readme=File.ReadAllText(Path.Combine(RepoPaths.Root(),"icons","README.md"));
			StringAssert.Contains(readme,"## The custom icons");
			StringAssert.Contains(readme,"original");
			StringAssert.Contains(readme,"tools/New-DentalIcons.ps1");
			foreach(string[] arrayIcon in ArrayIcons44) {
				StringAssert.Contains(readme,"`"+arrayIcon[0]+"`","icons/README.md lists "+arrayIcon[0]+".");
			}
		}
		#endregion Manifest and SVGs

		#region Drawing
		[TestMethod]
		public void Gdi_EveryIconDrawsAtEverySlot_TheDesignForTheSlot_FilledHasMoreInk() {
			foreach(DentalIcon dentalIcon in AllIcons()) {
				CollectionAssert.AreEqual(ArrayDesignSizes,UiIcons.GetDesigns(dentalIcon).Select(x => x.Size).ToArray(),dentalIcon.ToString());
				foreach(int[] arraySlotDesign in _arraySlotDesigns) {
					int sizeSlot=arraySlotDesign[0];
					IconDesign iconDesign=UiIcons.GetDesign(dentalIcon,sizeSlot);
					Assert.AreEqual(arraySlotDesign[1],iconDesign.Size,dentalIcon+": the design for a "+sizeSlot+" px slot (section 3.5).");
					Rectangle rectangleDraw=UiIcons.GetDrawRectangle(iconDesign,new Rectangle(0,0,sizeSlot,sizeSlot));
					using(Bitmap bitmap=UiIcons.GetBitmap(dentalIcon,sizeSlot,Color.Black)) {
						int[] arrayInk=IconRendererTests.GetInk(bitmap,isOnWhite:false);
						int countInk=arrayInk.Count(x => x>=128);
						Assert.IsTrue(countInk>0,dentalIcon+" draws nothing at "+sizeSlot+" px.");
						Assert.IsTrue(countInk<sizeSlot*sizeSlot*0.75,dentalIcon+" fills most of the slot at "+sizeSlot+" px.");
						Rectangle rectangleAllowed=Rectangle.Inflate(rectangleDraw,1,1);
						for(int i=0;i<arrayInk.Length;i++) {
							if(arrayInk[i]>0) {
								Assert.IsTrue(rectangleAllowed.Contains(i%sizeSlot,i/sizeSlot),dentalIcon+" at "+sizeSlot+" px draws outside "+rectangleDraw);
							}
						}
					}
				}
			}
			foreach(string[] arrayIcon in ArrayIcons44) {
				DentalIcon dentalIcon=(DentalIcon)Enum.Parse(typeof(DentalIcon),arrayIcon[0]);
				foreach(int size in ArrayDesignSizes) {
					using(Bitmap bitmapRegular=UiIcons.GetBitmap(dentalIcon,size,Color.Black))
					using(Bitmap bitmapFilled=UiIcons.GetBitmap(UiIcons.GetFilled(dentalIcon),size,Color.Black)) {
						long sumRegular=IconRendererTests.GetInk(bitmapRegular,isOnWhite:false).Sum(x => (long)x);
						long sumFilled=IconRendererTests.GetInk(bitmapFilled,isOnWhite:false).Sum(x => (long)x);
						Assert.IsTrue(sumFilled>sumRegular,dentalIcon+" at "+size+" px: the Filled twin has more ink than the Regular icon.");
					}
				}
			}
			//The typed calls draw the same design as the design call, in the colour asked, the disabled one in TextDisabled.
			using(Bitmap bitmap=new Bitmap(24,24,PixelFormat.Format32bppArgb)) {
				using(Graphics g=Graphics.FromImage(bitmap)) {
					g.Clear(Color.Transparent);
					UiIcons.DrawDisabled(g,DentalIcon.Tooth,new Rectangle(0,0,24,24));
				}
				Color colorDisabled=UiTheme.GetColor(EnumUiColor.TextDisabled);
				int countFull=0;
				for(int y=0;y<24;y++) {
					for(int x=0;x<24;x++) {
						Color color=bitmap.GetPixel(x,y);
						if(color.A==255) {
							countFull++;
							Assert.AreEqual(colorDisabled.ToArgb(),color.ToArgb(),"Disabled is TextDisabled.");
						}
					}
				}
				Assert.IsTrue(countFull>0);
			}
		}

		[TestMethod]
		public void Direct2d_EveryIconDrawsAtEverySlot_AsGdiDoes_AndTheOnlyGdiPathDrawsTheSame() {
			IconRendererTests.RequireDirect2d();
			IconRendererTests.RunSta(() => {
				bool isOnlyGdiOld=IconLibrary.OnlyGDI;
				try {
					foreach(DentalIcon dentalIcon in AllIcons()) {
						foreach(int[] arraySlotDesign in _arraySlotDesigns) {
							int sizeSlot=arraySlotDesign[0];
							IconDesign iconDesign=UiIcons.GetDesign(dentalIcon,sizeSlot);
							using(Bitmap bitmapDirect2d=DrawDirect2dOnWhite(iconDesign,sizeSlot))
							using(Bitmap bitmapGdi=DrawGdiOnWhite(iconDesign,sizeSlot)) {
								int[] arrayInkDirect2d=IconRendererTests.GetInk(bitmapDirect2d,isOnWhite:true);
								Assert.IsTrue(arrayInkDirect2d.Count(x => x>=128)>0,dentalIcon+" draws nothing on Direct2D at "+sizeSlot+" px.");
								IconRendererTests.AssertSameShape(arrayInkDirect2d,IconRendererTests.GetInk(bitmapGdi,isOnWhite:true),
									dentalIcon+" at "+sizeSlot+" px, Direct2D and GDI+");
							}
							//Direct2d.DrawIcon, the call the program's controls make: Direct2D, then with NoD2D.txt (OnlyGDI) GDI+, the same shape.
							IconLibrary.OnlyGDI=false;
							int[] arrayInkDrawIcon=DrawIconOnWhite(iconDesign,sizeSlot);
							Assert.IsFalse(IconLibrary.OnlyGDI,"Direct2D failed and DrawIcon fell back to GDI+.");
							IconLibrary.OnlyGDI=true;
							int[] arrayInkOnlyGdi=DrawIconOnWhite(iconDesign,sizeSlot);
							IconRendererTests.AssertSameShape(arrayInkDrawIcon,arrayInkOnlyGdi,dentalIcon+" at "+sizeSlot+" px, Direct2D and OnlyGDI");
						}
					}
				}
				finally {
					IconLibrary.OnlyGDI=isOnlyGdiOld;
				}
			});
		}

		[TestMethod]
		public void NoDpi_EveryIconDrawsTheSamePixels() {
			//With NoDpi.txt the program runs DPI-unaware and lays out at 96 DPI: the same pixels as at 100% in a DPI-aware thread, on GDI+ and Direct2D.
			bool isDirect2d=true;
			try {
				IconRendererTests.RequireDirect2d();
			}
			catch(AssertInconclusiveException) {
				isDirect2d=false;
			}
			IconRendererTests.RunSta(() => {
				foreach(DentalIcon dentalIcon in AllIcons()) {
					foreach(int[] arraySlotDesign in _arraySlotDesigns) {
						int size96=arraySlotDesign[0];
						int[] arrayAware=IconRendererTests.RunInDpiContext(IconRendererTests.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2,() => {
							using(Bitmap bitmap=UiIcons.GetBitmap(dentalIcon,size96,Color.Black)) {
								return IconRendererTests.GetPixels(bitmap);
							}
						});
						int[] arrayUnaware=IconRendererTests.RunInDpiContext(IconRendererTests.DPI_AWARENESS_CONTEXT_UNAWARE,() => {
							using(Bitmap bitmap=UiIcons.GetBitmap(dentalIcon,size96,1f,Color.Black)) {
								return IconRendererTests.GetPixels(bitmap);
							}
						});
						CollectionAssert.AreEqual(arrayAware,arrayUnaware,dentalIcon+" at "+size96+" px, GDI+ DPI-aware and NoDpi");
						if(!isDirect2d) {
							continue;
						}
						IconDesign iconDesign=UiIcons.GetDesign(dentalIcon,size96);
						int[] arrayAwareDirect2d=IconRendererTests.RunInDpiContext(IconRendererTests.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2,() => {
							using(Bitmap bitmap=DrawDirect2dOnWhite(iconDesign,size96)) {
								return IconRendererTests.GetPixels(bitmap);
							}
						});
						int[] arrayUnawareDirect2d=IconRendererTests.RunInDpiContext(IconRendererTests.DPI_AWARENESS_CONTEXT_UNAWARE,() => {
							using(Bitmap bitmap=DrawDirect2dOnWhite(iconDesign,size96)) {
								return IconRendererTests.GetPixels(bitmap);
							}
						});
						CollectionAssert.AreEqual(arrayAwareDirect2d,arrayUnawareDirect2d,dentalIcon+" at "+size96+" px, Direct2D DPI-aware and NoDpi");
					}
				}
			});
			if(!isDirect2d) {
				Assert.Inconclusive("GDI+ checked; the Direct2D half needs Direct2dWrapper.dll (the full build).");
			}
		}

		[TestMethod]
		public void Wpf_EveryIconHasItsGeometryAtEverySlot_AndRendersAsGdiDraws() {
			//The geometry needs no rendering and is checked in every session; the pixels only where WPF renders (inconclusive otherwise).
			bool isRendered=false;
			IconRendererTests.RunSta(() => {
				isRendered=IconRendererTests.RendersWpf();
				foreach(DentalIcon dentalIcon in AllIcons()) {
					foreach(int[] arraySlotDesign in _arraySlotDesigns) {
						int sizeSlot=arraySlotDesign[0];
						IconDesign iconDesign=UiIcons.GetDesign(dentalIcon,sizeSlot);
						System.Windows.Media.Geometry geometry=UiIcons.GetGeometryWpf(iconDesign,sizeSlot,1);
						Assert.IsTrue(geometry.IsFrozen,dentalIcon+" "+sizeSlot);
						Assert.IsFalse(geometry.IsEmpty(),dentalIcon+" has no WPF geometry at "+sizeSlot+" px.");
						System.Windows.Rect rectBounds=geometry.Bounds;
						Assert.IsTrue(rectBounds.Left>=-0.01 && rectBounds.Top>=-0.01 && rectBounds.Right<=sizeSlot+0.01 && rectBounds.Bottom<=sizeSlot+0.01,
							dentalIcon+" leaves its slot at "+sizeSlot+" px: "+rectBounds);
						using(Bitmap bitmapGdi=UiIcons.GetBitmap(iconDesign,sizeSlot,Color.Black)) {
							int[] arrayInkGdi=IconRendererTests.GetInk(bitmapGdi,isOnWhite:false);
							int countMismatch=0;
							for(int i=0;i<arrayInkGdi.Length;i++) {
								if(arrayInkGdi[i]!=255 && arrayInkGdi[i]!=0) {
									continue;//an edge pixel
								}
								bool isInside=geometry.FillContains(new System.Windows.Point(i%sizeSlot+0.5,i/sizeSlot+0.5));
								if(isInside!=(arrayInkGdi[i]==255)) {
									countMismatch++;
								}
							}
							Assert.IsTrue(countMismatch<=Math.Max(1,sizeSlot*sizeSlot/100),dentalIcon+" at "+sizeSlot+" px: "+countMismatch
								+" pixel centres differ from GDI+.");
							if(!isRendered) {
								continue;
							}
							System.Windows.Media.DrawingVisual drawingVisual=new System.Windows.Media.DrawingVisual();
							using(System.Windows.Media.DrawingContext drawingContext=drawingVisual.RenderOpen()) {
								drawingContext.DrawGeometry(System.Windows.Media.Brushes.Black,null,geometry);
							}
							int[] arrayInkWpf=IconRendererTests.GetInk(IconRendererTests.RenderToBitmap(drawingVisual,sizeSlot),isOnWhite:false);
							IconRendererTests.AssertSameShape(arrayInkWpf,arrayInkGdi,dentalIcon+" at "+sizeSlot+" px, WPF and GDI+");
						}
					}
				}
				//At 150% a 16 DIP slot is 24 device pixels: the 24 design.
				System.Windows.Media.Geometry geometry150=UiIcons.GetGeometryWpf(UiIcons.GetDesign(DentalIcon.Tooth,24),16,1.5);
				Assert.IsTrue(geometry150.Bounds.Width>10 && geometry150.Bounds.Right<=16.01,geometry150.Bounds.ToString());
			});
			if(!isRendered) {
				Assert.Inconclusive("The WPF geometry of every dental icon is checked; WPF draws nothing in this session, so the WPF pixels wait for a "
					+"connected session.");
			}
		}
		#endregion Drawing

		#region The default procedure buttons of new databases (UQ-7)
		[TestMethod]
		public void SeededProcButtonPictures_Are20x20Bitmaps_OfTheDentalIcons_TheOtherRowsUnchanged() {
			List<ProcButtonPicture> listBefore=ReadProcButtonPicturesBefore();
			Assert.AreEqual(8,listBefore.Count,"The 8 default procedure buttons with a picture (UQ-7).");
			List<string[]> listRows=ReadSeededProcButtons();
			Assert.AreEqual(listBefore.Count,listRows.Count(x => x[3]!=""),"The buttons with a picture: the same 8, no more.");
			foreach(ProcButtonPicture procButtonPicture in listBefore) {
				string[] arrayRow=listRows.SingleOrDefault(x => x[0]==procButtonPicture.ProcButtonNum.ToString());
				Assert.IsNotNull(arrayRow,"procbutton "+procButtonPicture.ProcButtonNum);
				Assert.AreEqual(procButtonPicture.Description,arrayRow[1]);
				string picture=arrayRow[3];
				Assert.AreNotEqual(procButtonPicture.PictureBefore,picture,procButtonPicture.Description+" still has Open Dental's picture.");
				//The format the table stores today: the same 54 byte bitmap header (20x20, 32 bits, BI_RGB, 96 DPI), every pixel opaque.
				byte[] arrayBytes=Convert.FromBase64String(picture);
				byte[] arrayBytesBefore=Convert.FromBase64String(procButtonPicture.PictureBefore);
				Assert.AreEqual(arrayBytesBefore.Length,arrayBytes.Length,procButtonPicture.Description);
				CollectionAssert.AreEqual(arrayBytesBefore.Take(54).ToArray(),arrayBytes.Take(54).ToArray(),procButtonPicture.Description+": the bitmap header.");
				for(int i=54+3;i<arrayBytes.Length;i+=4) {
					Assert.AreEqual(255,arrayBytes[i],procButtonPicture.Description+": an opaque picture, as today's.");
				}
				using(Bitmap bitmap=DecodePicture(picture)) {
					Assert.AreEqual(20,bitmap.Width,procButtonPicture.Description);
					Assert.AreEqual(20,bitmap.Height,procButtonPicture.Description);
					//The icon's 20 px design in the light look's Text colour on white: the same shape as the renderer draws it now.
					using(Bitmap bitmapExpected=DecodePicture(UiIcons.GetProcButtonPicture(procButtonPicture.DentalIcon))) {
						IconRendererTests.AssertSameShape(IconRendererTests.GetInk(bitmap,isOnWhite:true),IconRendererTests.GetInk(bitmapExpected,isOnWhite:true),
							procButtonPicture.Description+": the picture of DentalIcon."+procButtonPicture.DentalIcon);
					}
				}
			}
			//On this PC the renderer gives the file's bytes exactly (tools/DevDbTool writes UiIcons.GetProcButtonPicture into the new database).
			foreach(ProcButtonPicture procButtonPicture in listBefore) {
				string picture=listRows.Single(x => x[0]==procButtonPicture.ProcButtonNum.ToString())[3];
				Assert.AreEqual(UiIcons.GetProcButtonPicture(procButtonPicture.DentalIcon),picture,procButtonPicture.Description+": the bytes of "
					+"UiIcons.GetProcButtonPicture(DentalIcon."+procButtonPicture.DentalIcon+").");
			}
		}

		[TestMethod]
		public void NoCodePathGivesTheNewPicturesToAnOfficesRows() {
			string root=RepoPaths.Root();
			List<ProcButtonPicture> listBefore=ReadProcButtonPicturesBefore();
			//1. ProcButtons.SetToDefault (Setup, Procedure Codes tools: it replaces the office's buttons when asked) keeps Open Dental's pictures.
			string procButtonsCs=File.ReadAllText(Path.Combine(root,"OpenPmsDentalBusiness","Data Interface","ProcButtons.cs"));
			foreach(ProcButtonPicture procButtonPicture in listBefore) {
				StringAssert.Contains(procButtonsCs,"'"+procButtonPicture.PictureBefore+"'",procButtonPicture.Description+": SetToDefault keeps today's picture.");
			}
			//2. No fork migration step and no convert script touches a procedure button's picture.
			foreach(string path in Directory.GetFiles(Path.Combine(root,"OpenPmsDentalBusiness","Misc","ForkMigrations"),"*.cs",SearchOption.AllDirectories)) {
				string code=File.ReadAllText(path);
				Assert.IsFalse(Regex.IsMatch(code,"procbutton",RegexOptions.IgnoreCase),path+" names the procbutton table: an office's rows must not change.");
				Assert.IsFalse(code.Contains("GetProcButtonPicture"),path+" uses the new pictures.");
			}
			//3. The new pictures are only in db/init/new-database.sql: no source file holds one, and only the init file's maker calls the renderer.
			List<string> listNewPictures=listBefore.Select(x => UiIcons.GetProcButtonPicture(x.DentalIcon)).Distinct().ToList();
			List<string> listSnippets=listNewPictures.Select(x => GetDistinctSnippet(x)).ToList();
			foreach(string snippet in listSnippets) {
				Assert.IsFalse(listBefore.Any(x => x.PictureBefore.Contains(snippet)),"The snippet "+snippet+" is not distinctive.");
			}
			List<string> listCallers=new List<string>();
			string[] arrayExtensions=new[] { ".cs",".sql",".ps1",".resx",".tsv" };
			string[] arrayFolders=new[] { "OpenPmsDental","OpenPmsDentalBusiness","CodeBase","DataConnectionBase","WpfControlsOD","tools","db","tests",
				"UnitTests","CentralManager","OpenPmsDentalServer" };
			foreach(string folder in arrayFolders) {
				string pathFolder=Path.Combine(root,folder);
				if(!Directory.Exists(pathFolder)) {
					continue;
				}
				foreach(string path in Directory.EnumerateFiles(pathFolder,"*",SearchOption.AllDirectories)) {
					string relative=path.Substring(root.Length+1).Replace('\\','/');
					if(!arrayExtensions.Contains(Path.GetExtension(path).ToLowerInvariant()) || Regex.IsMatch(relative,"/(bin|obj)/")) {
						continue;
					}
					string text=File.ReadAllText(path);
					if(relative!="db/init/new-database.sql") {
						foreach(string snippet in listSnippets) {
							Assert.IsFalse(text.Contains(snippet),relative+" holds a new procedure button picture: only db/init/new-database.sql may.");
						}
					}
					if(text.Contains("GetProcButtonPicture(") && Path.GetExtension(path)==".cs") {
						listCallers.Add(relative);
					}
				}
			}
			CollectionAssert.AreEquivalent(new[] { "CodeBase/UI/Icons/UiIcons.Dental.cs","tools/DevDbTool/InitSqlScrub.cs","tests/UiRefreshTests/DentalIconTests.cs" },
				listCallers,"Only the new database's maker (DevDbTool's scrub of the scratch database) gives the pictures to rows.");
			string textInitSql=File.ReadAllText(Path.Combine(root,"db","init","new-database.sql"));
			foreach(string picture in listNewPictures) {
				StringAssert.Contains(textInitSql,"'"+picture+"'","db/init/new-database.sql carries the new picture.");
			}
		}

		///<summary>The 48 characters of a picture's base64 with the most different characters: a part of the drawing, not of the white background
		///every picture shares ("////").</summary>
		private static string GetDistinctSnippet(string base64) {
			string snippetBest=base64.Substring(0,48);
			int countBest=-1;
			for(int i=72;i+48<=base64.Length;i+=4) {
				string snippet=base64.Substring(i,48);
				int count=snippet.Distinct().Count();
				if(count>countBest) {
					countBest=count;
					snippetBest=snippet;
				}
			}
			return snippetBest;
		}
		#endregion The default procedure buttons of new databases (UQ-7)

		#region Gallery G-UI1 part 4
		[TestMethod]
		public void DentalIconPictures_ForTheGallery() {
			//Every dental icon at every design size, Regular and Filled, in the Text colour on white, beside today's picture where there is one: the
			//probe, the image selector's radiograph and photo, and the default procedure buttons' pictures. Written only with OPMS_UI8_ICON_PICTURES
			//set; the drawing is checked every run. The module bar's chart pictures (Chart32G, Chart32W, ChartMed32) were pictured beside Tooth,
			//ToothFilled and ChartMedical here until the chart icons began to draw those dental icons (UI-8); ModuleBarChartIcons_ForTheGallery
			//pictures them now, and the gallery keeps the before pictures made with the Gen_*.cs pictures (docs/port-notes/UI-8.md, section 6).
			string folder=Environment.GetEnvironmentVariable(VARIABLE_PICTURES);
			bool isWriting=!string.IsNullOrEmpty(folder);
			Dictionary<string,EnumIcons> dictToday=new Dictionary<string,EnumIcons> {
				{ "PerioProbe",EnumIcons.Probe },{ "XrayPeriapical",EnumIcons.ImageSelectorXray },{ "IntraoralPhoto",EnumIcons.ImageSelectorPhoto },
			};
			Color colorText=UiTheme.GetColorLight(EnumUiColor.Text);
			IconRendererTests.RunSta(() => {
				foreach(DentalIcon dentalIcon in AllIcons()) {
					string name=dentalIcon.ToString();
					foreach(int size in ArrayDesignSizes) {
						using(Bitmap bitmapAfter=NewWhiteBitmap(size)) {
							using(Graphics g=Graphics.FromImage(bitmapAfter)) {
								UiIcons.Draw(g,dentalIcon,new Rectangle(0,0,size,size),colorText);
							}
							Assert.IsTrue(IconRendererTests.GetInk(bitmapAfter,isOnWhite:true).Any(x => x>=128),name+" at "+size+" px.");
							if(isWriting) {
								Save(bitmapAfter,Path.Combine(folder,"after","icons",name,size+"px.png"));
							}
						}
						EnumIcons enumIcons;
						if(!isWriting || !dictToday.TryGetValue(name,out enumIcons)) {
							continue;
						}
						using(Bitmap bitmapBefore=NewWhiteBitmap(size)) {
							using(Graphics g=Graphics.FromImage(bitmapBefore)) {
								IconLibrary.Draw(g,enumIcons,new Rectangle(0,0,size,size),Color.White,Color.White);
							}
							if(IconRendererTests.GetInk(bitmapBefore,isOnWhite:true).Any(x => x>0)) {
								Save(bitmapBefore,Path.Combine(folder,"before","icons",name,size+"px.png"));
							}
						}
					}
				}
				//The default procedure buttons: today's picture and the new one, as the Chart module shows them (20x20), and 4x for a closer look; and
				//the new pictures' base64 as tools/DevDbTool writes them into a new database (procbutton-pictures.tsv, beside before and after).
				StringBuilder stringBuilderTsv=new StringBuilder();
				foreach(ProcButtonPicture procButtonPicture in ReadProcButtonPicturesBefore()) {
					string fileName=procButtonPicture.ProcButtonNum+"-"+Regex.Replace(procButtonPicture.Description,"[^A-Za-z0-9]+","-")+".png";
					string pictureAfter=UiIcons.GetProcButtonPicture(procButtonPicture.DentalIcon);
					stringBuilderTsv.Append(procButtonPicture.ProcButtonNum+"\t"+procButtonPicture.Description+"\t"+procButtonPicture.DentalIcon+"\t"+pictureAfter+"\n");
					if(!isWriting) {
						continue;
					}
					using(Bitmap bitmapBefore=DecodePicture(procButtonPicture.PictureBefore))
					using(Bitmap bitmapAfter=DecodePicture(pictureAfter)) {
						Save(bitmapBefore,Path.Combine(folder,"before","procbuttons",fileName));
						Save(bitmapAfter,Path.Combine(folder,"after","procbuttons",fileName));
						using(Bitmap bitmapBefore4=Enlarge(bitmapBefore,4))
						using(Bitmap bitmapAfter4=Enlarge(bitmapAfter,4)) {
							Save(bitmapBefore4,Path.Combine(folder,"before","procbuttons-4x",fileName));
							Save(bitmapAfter4,Path.Combine(folder,"after","procbuttons-4x",fileName));
						}
					}
				}
				if(!isWriting) {
					return;
				}
				File.WriteAllText(Path.Combine(folder,"procbutton-pictures.tsv"),stringBuilderTsv.ToString(),new UTF8Encoding(false));
				//One sheet per design size: every icon, Regular above Filled, 8 px apart.
				foreach(int size in ArrayDesignSizes) {
					int cell=size+8;
					using(Bitmap bitmapSheet=new Bitmap(cell*ArrayIcons44.Length+8,cell*2+8,PixelFormat.Format32bppArgb)) {
						using(Graphics g=Graphics.FromImage(bitmapSheet)) {
							g.Clear(Color.White);
							for(int i=0;i<ArrayIcons44.Length;i++) {
								DentalIcon dentalIcon=(DentalIcon)Enum.Parse(typeof(DentalIcon),ArrayIcons44[i][0]);
								UiIcons.Draw(g,dentalIcon,new Rectangle(8+i*cell,8,size,size),colorText);
								UiIcons.Draw(g,UiIcons.GetFilled(dentalIcon),new Rectangle(8+i*cell,8+cell,size,size),colorText);
							}
						}
						Save(bitmapSheet,Path.Combine(folder,"after","sheets","sheet-"+size+"px.png"));
					}
				}
			});
			if(!isWriting) {
				Assert.IsTrue(true,"Set "+VARIABLE_PICTURES+" to a folder to write the gallery's pictures.");
			}
		}

		///<summary>The module bar's chart icons as ModuleBar draws them: the EnumIcons, the dental icon it draws since UI-8, its state (rest: on
		///ColorOD.Control; selected: on white) and the background.</summary>
		private static readonly (EnumIcons enumIcons,string dentalIcon,string state,Color colorBack)[] _arrayModuleBarChart=new[] {
			(EnumIcons.Chart32G,"Tooth","rest",Color.FromArgb(240,240,240)),
			(EnumIcons.Chart32W,"ToothFilled","selected",Color.White),
			(EnumIcons.ChartMed32,"ChartMedical","rest",Color.FromArgb(240,240,240)),
			(EnumIcons.ChartMed32,"ChartMedical","selected",Color.White),
		};

		[TestMethod]
		public void ModuleBarChartIcons_ForTheGallery() {
			//The module bar's chart icons (Chart32G at rest, Chart32W selected, ChartMed32 in the medical layout) drawn by IconLibrary.Draw as
			//ModuleBar.OnPaint draws them: a slot of LayoutManager.Scale(32) at 100, 125, 150 and 200%, over the button's background, and the 100%
			//one 4x for a closer look. Before UI-8's switch these were the Gen_*.cs pictures; since it, the dental icons Tooth, ToothFilled and
			//ChartMedical, the same pixels as UiIcons draws them. Written to <OPMS_UI8_ICON_PICTURES>\modulebar\ only when the variable is set; the
			//drawing is checked every run.
			string folder=Environment.GetEnvironmentVariable(VARIABLE_PICTURES);
			bool isWriting=!string.IsNullOrEmpty(folder);
			IconRendererTests.RunSta(() => {
				bool isOnlyGdiOld=IconLibrary.OnlyGDI;
				try {
					foreach((EnumIcons enumIcons,string dentalIcon,string state,Color colorBack) in _arrayModuleBarChart) {
						string name=enumIcons+"-"+dentalIcon+"-"+state;
						//The chart icon is its dental icon: the GDI+ fill (designMode) gives the dental icon's pixels in the Text colour, at every slot.
						Assert.AreEqual((DentalIcon)Enum.Parse(typeof(DentalIcon),dentalIcon),IconLibrary.GetDentalIcon(enumIcons),name);
						foreach(int[] arraySlotDesign in _arraySlotDesigns) {
							int slot=arraySlotDesign[0];
							using(Bitmap bitmapLibrary=NewWhiteBitmap(slot))
							using(Bitmap bitmapDental=NewWhiteBitmap(slot)) {
								using(Graphics g=Graphics.FromImage(bitmapLibrary)) {
									IconLibrary.Draw(g,enumIcons,new Rectangle(0,0,slot,slot),Color.White,Color.White,designMode:true);
								}
								using(Graphics g=Graphics.FromImage(bitmapDental)) {
									UiIcons.Draw(g,IconLibrary.GetDentalIcon(enumIcons),new Rectangle(0,0,slot,slot),EnumUiColor.Text);
								}
								CollectionAssert.AreEqual(IconRendererTests.GetPixels(bitmapDental),IconRendererTests.GetPixels(bitmapLibrary),name+" at "+slot+" px.");
							}
						}
						foreach(int percent in new[] { 100,125,150,200 }) {
							int size=32*percent/100;
							using(Bitmap bitmap=new Bitmap(size,size,PixelFormat.Format32bppArgb)) {
								bitmap.SetResolution(96,96);
								using(Graphics g=Graphics.FromImage(bitmap)) {
									g.Clear(colorBack);
									IconLibrary.Draw(g,enumIcons,new Rectangle(0,0,size,size),colorBack,colorBack);
								}
								Assert.IsTrue(IconRendererTests.GetInk(bitmap,isOnWhite:true).Any(x => x>=64),name+" at "+percent+"% draws nothing.");
								if(!isWriting) {
									continue;
								}
								Save(bitmap,Path.Combine(folder,"modulebar",name,percent+"pct-"+size+"px.png"));
								if(percent==100) {
									using(Bitmap bitmap4=Enlarge(bitmap,4)) {
										Save(bitmap4,Path.Combine(folder,"modulebar",name,"100pct-4x.png"));
									}
								}
							}
						}
					}
				}
				finally {
					IconLibrary.OnlyGDI=isOnlyGdiOld;
				}
			});
			if(!isWriting) {
				Assert.IsTrue(true,"Set "+VARIABLE_PICTURES+" to a folder to write the module bar's pictures.");
			}
		}

		private static Bitmap NewWhiteBitmap(int size) {
			Bitmap bitmap=new Bitmap(size,size,PixelFormat.Format32bppArgb);
			bitmap.SetResolution(96,96);
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(Color.White);
			}
			return bitmap;
		}

		private static Bitmap Enlarge(Bitmap bitmap,int factor) {
			Bitmap bitmapLarge=new Bitmap(bitmap.Width*factor,bitmap.Height*factor,PixelFormat.Format32bppArgb);
			bitmapLarge.SetResolution(96,96);
			using(Graphics g=Graphics.FromImage(bitmapLarge)) {
				g.InterpolationMode=InterpolationMode.NearestNeighbor;
				g.PixelOffsetMode=PixelOffsetMode.Half;
				g.DrawImage(bitmap,new Rectangle(0,0,bitmapLarge.Width,bitmapLarge.Height));
			}
			return bitmapLarge;
		}

		private static void Save(Bitmap bitmap,string path) {
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			bitmap.Save(path,ImageFormat.Png);
		}
		#endregion Gallery G-UI1 part 4
	}
}
