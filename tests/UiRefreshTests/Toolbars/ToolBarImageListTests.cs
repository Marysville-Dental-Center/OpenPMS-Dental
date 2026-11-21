// UI-9b-1's tests (docs/specs/ui-refresh-plan.md, section 7's UI-9b row and section 9; docs/port-notes/UI-9b.md): the module toolbars' ImageLists
// become lists of icons from the one icon factory (IconLibrary.GetImageList, through ToolBarOD.SetImageListIcons), and their buttons take an
// EnumIcons. A button set by ImageIndex still draws its picture (a plug-in's button through the six LayoutToolBar_end hooks, a program link's),
// at 100, 150 and 200%; an EnumIcons button draws its glyph at those sizes; the program link pictures of a synthetic program row (in the cache,
// no database) still show; each module's list keeps today's ImageIndex order as icons; the slice's toolbars have no ImageIndex button left.
// The gallery G-UI3 slice is opt-in: OPMS_UI9B_PICTURES names the folder for each module toolbar at 100, 150 and 200% (off screen, with a
// synthetic program link button and a plug-in button set by ImageIndex). No window is shown, no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using UiRefreshTests.Scan;

namespace UiRefreshTests.Toolbars {
	[TestClass]
	public class ToolBarImageListTests {
		///<summary>The folder the gallery's pictures are written to, when set.</summary>
		public const string VARIABLE_PICTURES="OPMS_UI9B_PICTURES";
		///<summary>The scales of the tests and the gallery (LayoutManager's scale).</summary>
		public static readonly float[] ArrayScales=new float[] { 1f,1.5f,2f };
		///<summary>The toolbars of the slice (ToolBarSources ids): the main toolbar and the module toolbars with an ImageList.</summary>
		public static readonly string[] ArraySliceIds=new[] { "main","appts","family","account","txplan","chart" };
		///<summary>The colour of the synthetic program link picture: a colour no icon and no toolbar state draws.</summary>
		private static readonly Color _colorProgramLink=Color.FromArgb(255,0,200,0);

		///<summary>Each module list's pictures by today's ImageIndex (the designers' streams before UI-9b-1, docs/port-notes/UI-9b.md), as the icon
		///of each picture's purpose: a plug-in's button set by ImageIndex shows the same purpose.</summary>
		public static readonly Dictionary<string,string[]> DictIndexOrder=new Dictionary<string,string[]> {
			{ "main",new[] { "PatSelect","CommLog","Email","TaskList","Label","Text" } },
			{ "appts",new[] { "Print","ApptLists","Call","Unscheduled","BreakAptX","Complete","DeleteX","Patient","Add","Recall" } },
			{ "family",new[] { "PatSelect","Recall","PatAdd","PatDelete","PatSetGuarantor","PatMoveFam","Insurance" } },
			{ "account",new[] { "PatSelect","Payment","Adjustment","Insurance","Print" } },
			{ "txplan",new[] { "PatSelect","Sync","Print","Add" } },
			{ "chart",new[] { "PatSelect","Rx","PerioChart","Anesthesia","CommLog","Print" } },
		};

		#region Helpers
		///<summary>A 22 x 22 picture of the synthetic program link: a square of _colorProgramLink with a transparent edge.</summary>
		public static Bitmap GetProgramLinkPicture() {
			Bitmap bitmap=new Bitmap(22,22,PixelFormat.Format32bppArgb);
			using(Graphics g=Graphics.FromImage(bitmap))
			using(SolidBrush solidBrush=new SolidBrush(_colorProgramLink)) {
				g.Clear(Color.Transparent);
				g.FillRectangle(solidBrush,3,3,16,16);
			}
			return bitmap;
		}

		///<summary>The picture as the program row's ButtonImage holds it: base 64 of the image file.</summary>
		private static string GetButtonImageText(Bitmap bitmap) {
			using(MemoryStream memoryStream=new MemoryStream()) {
				bitmap.Save(memoryStream,ImageFormat.Png);
				return Convert.ToBase64String(memoryStream.ToArray());
			}
		}

		///<summary>A toolbar at the scale, in today's font scaled, laid out by drawing it.</summary>
		private static ToolBarOD NewToolBar(float scale,int width96) {
			ToolBarOD toolBarOD=new ToolBarOD();
			toolBarOD.LayoutManager.SetScaleMS(scale);
			toolBarOD.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f*scale);
			toolBarOD.Size=new Size(UiTheme.ScaleI(width96,scale),UiTheme.ScaleI(ToolBarModel.HEIGHT_TOOLBAR,scale));
			return toolBarOD;
		}

		///<summary>The rectangle ToolBarOD.DrawButton draws a button's picture or icon in: 3 px in, 1 px down, the 22 px slot scaled.</summary>
		private static Rectangle GetImageRectangle(ToolBarOD toolBarOD,ODToolBarButton odToolBarButton) {
			int size=toolBarOD.LayoutManager.Scale(ToolBarOD.SIZE_IMAGE_LIST96);
			return new Rectangle(odToolBarButton.Bounds.X+toolBarOD.LayoutManager.Scale(3),odToolBarButton.Bounds.Y+toolBarOD.LayoutManager.Scale(1),size,size);
		}

		private static int CountColor(Bitmap bitmap,Rectangle rectangle,Func<Color,bool> funcIs) {
			int count=0;
			for(int y=Math.Max(rectangle.Top,0);y<Math.Min(rectangle.Bottom,bitmap.Height);y++) {
				for(int x=Math.Max(rectangle.Left,0);x<Math.Min(rectangle.Right,bitmap.Width);x++) {
					if(funcIs(bitmap.GetPixel(x,y))) {
						count++;
					}
				}
			}
			return count;
		}

		private static bool IsProgramLinkColor(Color color) {
			return Math.Abs(color.R-_colorProgramLink.R)<=8 && Math.Abs(color.G-_colorProgramLink.G)<=8 && Math.Abs(color.B-_colorProgramLink.B)<=8;
		}

		///<summary>Dark pixels: an icon in the Text colour on the toolbar's light background.</summary>
		private static bool IsDark(Color color) {
			return color.R+color.G+color.B<3*128;
		}
		#endregion Helpers

		[TestMethod]
		public void IconFactory_MakesADpiSizedListOfIcons_WithTheirKeys() {
			EnumIcons[] arrayIcons=new[] { EnumIcons.Print,EnumIcons.Payment,EnumIcons.PerioChart,EnumIcons.DeleteX };
			IconRendererTests.RunSta(() => {
				foreach(int size in new[] { 22,27,33,44 }) {
					using(ImageList imageList=IconLibrary.GetImageList(arrayIcons,size)) {
						Assert.AreEqual(ColorDepth.Depth32Bit,imageList.ColorDepth);
						Assert.AreEqual(new Size(size,size),imageList.ImageSize);
						Assert.AreEqual(arrayIcons.Length,imageList.Images.Count);
						for(int i=0;i<arrayIcons.Length;i++) {
							Assert.AreEqual(i,imageList.Images.IndexOfKey(arrayIcons[i].ToString()),arrayIcons[i]+": its member name is its key.");
							using(Bitmap bitmap=(Bitmap)imageList.Images[i])
							using(Bitmap bitmapExpected=IconLibrary.GetBitmap(arrayIcons[i],size,size)) {
								Assert.AreEqual(new Size(size,size),bitmap.Size);
								int countInk=CountColor(bitmap,new Rectangle(Point.Empty,bitmap.Size),x => x.A>=128);
								Assert.IsTrue(countInk>0,arrayIcons[i]+" is empty at "+size+" px.");
								Assert.AreEqual(CountColor(bitmapExpected,new Rectangle(Point.Empty,bitmap.Size),x => x.A>=128),countInk,
									arrayIcons[i]+" at "+size+" px: the list holds the icon's GetBitmap (the design for the size, centred).");
							}
						}
						//A list takes more pictures after it is made, by key (the program links).
						imageList.Images.Add("7Synthetic",GetProgramLinkPicture());
						Assert.AreEqual(arrayIcons.Length,imageList.Images.IndexOfKey("7Synthetic"));
					}
				}
			});
		}

		[TestMethod]
		public void ImageIndexButton_StillDrawsItsPicture_At100_150_200() {
			IconRendererTests.RunSta(() => {
				foreach(float scale in ArrayScales) {
					using(ToolBarOD toolBarOD=NewToolBar(scale,400)) {
						toolBarOD.SetImageListIcons(EnumIcons.PatSelect,EnumIcons.Payment);
						Assert.AreEqual(UiTheme.ScaleI(22,scale),toolBarOD.ImageList.ImageSize.Width,"The list of icons is at the toolbar's scale.");
						//A plug-in or program link picture, added by key after the icons, as ProgramL.LoadToolBar does.
						toolBarOD.ImageList.Images.Add("7Synthetic",GetProgramLinkPicture());
						ODToolBarButton buttonIcon=new ODToolBarButton("Plug-in",1,"","PlugIn");
						ODToolBarButton buttonPicture=new ODToolBarButton("Link",toolBarOD.ImageList.Images.IndexOfKey("7Synthetic"),"","Link");
						toolBarOD.Buttons.Add(buttonIcon);
						toolBarOD.Buttons.Add(buttonPicture);
						string what=" at "+(int)(scale*100)+"%";
						using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
							Assert.IsTrue(CountColor(bitmap,GetImageRectangle(toolBarOD,buttonIcon),IsDark)>UiTheme.ScaleI(10,scale),"The button set by ImageIndex 1 draws the list's Payment icon"+what+".");
							Rectangle rectanglePicture=GetImageRectangle(toolBarOD,buttonPicture);
							int countGreen=CountColor(bitmap,rectanglePicture,IsProgramLinkColor);
							int sizeSquare=UiTheme.ScaleI(16,scale);
							Assert.IsTrue(countGreen>=sizeSquare*sizeSquare*8/10,"The button set by ImageIndex draws the added picture"+what+": "+countGreen+" px.");
							Assert.AreEqual(0,CountColor(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height),IsProgramLinkColor)-countGreen,"Only inside its slot"+what+".");
						}
						//Bounds: the button's width is today's formula with the 22 px picture (ComputeButtonsizes).
						using(Bitmap bitmapMeasure=new Bitmap(1,1))
						using(Graphics g=Graphics.FromImage(bitmapMeasure)) {
							int widthExpected=toolBarOD.LayoutManager.Scale(22)+(int)g.MeasureString("Link",toolBarOD.Font).Width+toolBarOD.LayoutManager.Scale(8);
							Assert.AreEqual(widthExpected,buttonPicture.Bounds.Width,"The ImageIndex button's width"+what+".");
						}
					}
				}
			});
		}

		[TestMethod]
		public void ImageList_BuiltAgainAtAScaleChange_KeepsTheAddedPicturesByKeyAndIndex() {
			IconRendererTests.RunSta(() => {
				using(ToolBarOD toolBarOD=NewToolBar(1f,400)) {
					toolBarOD.SetImageListIcons(EnumIcons.PatSelect,EnumIcons.Payment,EnumIcons.Print);
					toolBarOD.ImageList.Images.Add("7Synthetic",GetProgramLinkPicture());
					toolBarOD.ImageList.Images.Add(GetProgramLinkPicture());//a plug-in's picture without a key
					ODToolBarButton odToolBarButton=new ODToolBarButton("Link",3,"","Link");
					toolBarOD.Buttons.Add(odToolBarButton);
					ToolBarPictureTests.Draw(toolBarOD).Dispose();
					Assert.AreEqual(22,toolBarOD.ImageList.ImageSize.Width);
					toolBarOD.LayoutManager.SetScaleMS(1.5f);//a DPI or zoom change
					toolBarOD.Size=new Size(600,UiTheme.ScaleI(ToolBarModel.HEIGHT_TOOLBAR,1.5f));
					using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
						Assert.AreEqual(33,toolBarOD.ImageList.ImageSize.Width,"Built again at the new scale before the paint.");
						Assert.AreEqual(5,toolBarOD.ImageList.Images.Count);
						Assert.AreEqual(1,toolBarOD.ImageList.Images.IndexOfKey("Payment"));
						Assert.AreEqual(3,toolBarOD.ImageList.Images.IndexOfKey("7Synthetic"),"The program link picture keeps its key and its index.");
						Assert.IsTrue(CountColor(bitmap,GetImageRectangle(toolBarOD,odToolBarButton),IsProgramLinkColor)>0,"Still drawn after the change.");
					}
				}
			});
		}

		[TestMethod]
		public void ImageListSetByTheCaller_StaysA96DpiList_ScaledWhenDrawn() {
			//A designer's or plug-in's own ImageList (no SetImageListIcons): its pictures are 96 DPI pictures, drawn scaled, as before UI-9b.
			IconRendererTests.RunSta(() => {
				using(ToolBarOD toolBarOD=NewToolBar(2f,400))
				using(ImageList imageList=IconLibrary.GetImageList(new EnumIcons[0],22)) {
					imageList.Images.Add("7Synthetic",GetProgramLinkPicture());
					toolBarOD.ImageList=imageList;
					ODToolBarButton odToolBarButton=new ODToolBarButton("Link",0,"","Link");
					toolBarOD.Buttons.Add(odToolBarButton);
					using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
						Assert.AreSame(imageList,toolBarOD.ImageList,"The caller's list is not rebuilt.");
						Assert.AreEqual(22,imageList.ImageSize.Width);
						int countGreen=CountColor(bitmap,GetImageRectangle(toolBarOD,odToolBarButton),IsProgramLinkColor);
						Assert.IsTrue(countGreen>=32*32*8/10,"The 22 px picture drawn in the 44 px slot: "+countGreen+" px.");
					}
				}
			});
		}

		[TestMethod]
		public void IconButton_DrawsItsGlyph_At100_150_200() {
			IconRendererTests.RunSta(() => {
				foreach(float scale in ArrayScales) {
					foreach(EnumIcons icon in new[] { EnumIcons.Payment,EnumIcons.Rx,EnumIcons.PerioChart,EnumIcons.Print }) {
						using(ToolBarOD toolBarOD=NewToolBar(scale,300)) {
							ODToolBarButton odToolBarButton=new ODToolBarButton("Icon",icon,"","Icon");
							toolBarOD.Buttons.Add(odToolBarButton);
							using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
								Rectangle rectangle=GetImageRectangle(toolBarOD,odToolBarButton);
								IconDesign iconDesign=IconLibrary.GetDesign(icon,rectangle.Width);
								Assert.IsNotNull(iconDesign,icon.ToString());
								//The design of 3.5 for the slot: 20 px at 100% (22 px slot), 32 at 150% (33 px) and 200% (44 px) where the icon has one.
								int sizeExpected=IconLibrary.GetDesigns(icon).Where(x => x.Size<=rectangle.Width).Max(x => x.Size);
								Assert.AreEqual(sizeExpected,iconDesign.Size,icon+" at "+(int)(scale*100)+"%");
								//The same pixels as the glyph drawn on the toolbar's background in the icon's role colour.
								using(Bitmap bitmapExpected=new Bitmap(rectangle.Width,rectangle.Height,PixelFormat.Format24bppRgb)) {
									using(Graphics g=Graphics.FromImage(bitmapExpected)) {
										g.Clear(UiTheme.GetColor(EnumUiColor.SurfaceSubtle));
										UiIcons.Draw(g,iconDesign,new Rectangle(0,0,rectangle.Width,rectangle.Height),IconLibrary.GetColor(icon));
									}
									int countDiff=0;
									for(int y=0;y<rectangle.Height;y++) {
										for(int x=0;x<rectangle.Width;x++) {
											Color colorA=bitmap.GetPixel(rectangle.X+x,rectangle.Y+y);
											Color colorB=bitmapExpected.GetPixel(x,y);
											if(Math.Abs(colorA.R-colorB.R)+Math.Abs(colorA.G-colorB.G)+Math.Abs(colorA.B-colorB.B)>12) {
												countDiff++;
											}
										}
									}
									Assert.AreEqual(0,countDiff,icon+" at "+(int)(scale*100)+"%: the button's slot differs from the glyph in "+countDiff+" px.");
									Assert.IsTrue(CountColor(bitmapExpected,new Rectangle(Point.Empty,bitmapExpected.Size),IsDark)>0,icon+": the glyph has ink.");
								}
							}
						}
					}
				}
			});
		}

		[TestMethod]
		public void ProgramLinkPicture_FromASyntheticProgramRow_ShowsOnTheModuleToolbar() {
			ToolBarSource toolBarSource=ToolBarSources.ReadAll().Single(x => x.Id=="account");
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				//The program row, its toolbar button row and the clinics preference, in the caches only (no database).
				Program program=new Program { ProgramNum=7,ProgName=ProgramName.Xcharge.ToString(),ProgDesc="Synthetic link",Enabled=true,Path="",CommandLine="",
					Note="",PluginDllName="",ButtonImage="",FileTemplate="",FilePath="",CustErr="" };
				using(Bitmap bitmapPicture=GetProgramLinkPicture()) {
					program.ButtonImage=GetButtonImageText(bitmapPicture);
				}
				Assert.IsFalse(ProgramLinksDiscontinued.IsDiscontinued(program),"A program link that still shows.");
				try {
					Prefs.FillCacheFromTable(OpenPmsDentalBusiness.Crud.PrefCrud.ListToTable(new List<Pref> {
						new Pref { PrefNum=1,PrefName=PrefName.EasyNoClinics.ToString(),ValueString="1",Comments="" } }));
					Programs.FillCacheFromTable(OpenPmsDentalBusiness.Crud.ProgramCrud.ListToTable(new List<Program> { program }));
					ToolButItems.FillCacheFromTable(OpenPmsDentalBusiness.Crud.ToolButItemCrud.ListToTable(new List<ToolButItem> {
						new ToolButItem { ToolButItemNum=1,ProgramNum=7,ToolBar=EnumToolBar.AccountModule,ButtonText="Synthetic Link" } }));
					foreach(float scale in ArrayScales) {
						using(ToolBarOD toolBarOD=NewToolBar(scale,1300)) {
							ToolBarModel.SetImageList(toolBarOD,toolBarSource);
							foreach(ToolBarButtonSource toolBarButtonSource in toolBarSource.ListButtons) {
								toolBarOD.Buttons.Add(ToolBarModel.NewButton(toolBarButtonSource));
							}
							int countIcons=toolBarOD.ImageList.Images.Count;
							ProgramL.LoadToolBar(toolBarOD,EnumToolBar.AccountModule);
							ODToolBarButton buttonLink=toolBarOD.Buttons.Cast<ODToolBarButton>().Single(x => x.Tag is Program);
							Assert.AreEqual("Synthetic Link",buttonLink.Text);
							Assert.AreEqual(countIcons,toolBarOD.ImageList.Images.IndexOfKey("7"+program.ProgName),"The picture goes after the module's icons, by key.");
							Assert.AreEqual(countIcons,buttonLink.ImageIndex,"ProgramL looks the picture up by key (ProgramL.cs) and sets ImageIndex.");
							using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
								int countGreen=CountColor(bitmap,GetImageRectangle(toolBarOD,buttonLink),IsProgramLinkColor);
								int sizeSquare=UiTheme.ScaleI(16,scale);
								Assert.IsTrue(countGreen>=sizeSquare*sizeSquare*7/10,"The program link picture shows at "+(int)(scale*100)+"%: "+countGreen+" px.");
							}
							//Laid out again (a module's LayoutToolBar runs ProgramL again): the picture is replaced by key, the index stays.
							int indexLink=toolBarOD.Buttons.IndexOf(buttonLink);
							toolBarOD.Buttons.RemoveAt(indexLink);
							toolBarOD.Buttons.RemoveAt(indexLink-1);//its separator
							ProgramL.LoadToolBar(toolBarOD,EnumToolBar.AccountModule);
							Assert.AreEqual(countIcons+1,toolBarOD.ImageList.Images.Count,"Replaced, not added twice.");
						}
					}
				}
				finally {
					Programs.FillCacheFromTable(OpenPmsDentalBusiness.Crud.ProgramCrud.ListToTable(new List<Program>()));
					ToolButItems.FillCacheFromTable(OpenPmsDentalBusiness.Crud.ToolButItemCrud.ListToTable(new List<ToolButItem>()));
				}
			});
		}

		[TestMethod]
		public void ModuleImageLists_KeepTodaysIndexOrder_AsIcons() {
			List<ToolBarSource> listToolBars=ToolBarSources.ReadAll();
			foreach(string id in ArraySliceIds) {
				ToolBarSource toolBarSource=listToolBars.Single(x => x.Id==id);
				EnumIcons[] arrayIcons=ToolBarModel.GetIconsImageList(toolBarSource);
				Assert.IsNotNull(arrayIcons,id+": "+toolBarSource.TypeImageList+" has no "+ToolBarModel.FIELD_ICONS_IMAGE_LIST+" (UI-9b-1).");
				CollectionAssert.AreEqual(DictIndexOrder[id],arrayIcons.Select(x => x.ToString()).ToArray(),id+": the pictures' purposes in today's ImageIndex order.");
				foreach(EnumIcons icon in arrayIcons) {
					Assert.IsTrue(IconLibrary.GetDesigns(icon).Length>0,id+": "+icon+" has no design.");
				}
			}
		}

		[TestMethod]
		public void SliceToolbars_HaveNoImageIndexButtonLeft_TheirButtonsTakeAnIcon() {
			List<ToolBarSource> listToolBars=ToolBarSources.ReadAll();
			List<string> listLeft=new List<string>();
			foreach(string id in ArraySliceIds) {
				ToolBarSource toolBarSource=listToolBars.Single(x => x.Id==id);
				listLeft.AddRange(toolBarSource.ListButtons.Where(x => x.ImageIndex>=0).Select(x => id+": "+x.Text+" (ImageIndex "+x.ImageIndex+")"));
			}
			Assert.AreEqual(0,listLeft.Count,"Buttons of the slice that still take a picture by ImageIndex:\r\n"+string.Join("\r\n",listLeft));
		}

		///<summary>The G-UI3 slice's pictures (opt-in): each toolbar of the slice with every button its code can add, a synthetic program link button
		///and a plug-in button set by ImageIndex 0, at 100, 150 and 200%, off screen (toolbars\<id>-<scale>.png). Run at the commit before the
		///conversion for the before set and at the branch head for the after set; tools/New-UiGallery.ps1 pairs them by name.</summary>
		[TestMethod]
		public void Gallery_ModuleToolbars() {
			string folder=Environment.GetEnvironmentVariable(VARIABLE_PICTURES);
			if(string.IsNullOrWhiteSpace(folder)) {
				Assert.Inconclusive("Opt-in: set "+VARIABLE_PICTURES+" to a folder for the G-UI3 slice's pictures.");
			}
			List<ToolBarSource> listToolBars=ToolBarSources.ReadAll();
			List<string> listWritten=new List<string>();
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				UiTheme.InitWinForms();
				foreach(string id in ArraySliceIds) {
					ToolBarSource toolBarSource=listToolBars.Single(x => x.Id==id);
					int width96=toolBarSource.FuncWidth(ToolBarSources.GetModuleWidth(ToolBarPictureTests.WIDTH_WINDOW_STRIPS));
					foreach(float scale in ArrayScales) {
						using(ToolBarOD toolBarOD=NewToolBar(scale,width96+220)) {
							ToolBarModel.SetImageList(toolBarOD,toolBarSource);
							foreach(ToolBarButtonSource toolBarButtonSource in toolBarSource.ListButtons) {
								toolBarOD.Buttons.Add(ToolBarModel.NewButton(toolBarButtonSource));
							}
							toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
							toolBarOD.ImageList.Images.Add("7Synthetic",GetProgramLinkPicture());
							toolBarOD.Buttons.Add(new ODToolBarButton("Program link",toolBarOD.ImageList.Images.IndexOfKey("7Synthetic"),"","Link"));
							toolBarOD.Buttons.Add(new ODToolBarButton("Plug-in",0,"","PlugIn"));
							string name="toolbars\\"+id+"-"+(int)(scale*100)+".png";
							string path=Path.Combine(folder,name);
							Directory.CreateDirectory(Path.GetDirectoryName(path));
							using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
								bitmap.Save(path,ImageFormat.Png);
							}
							listWritten.Add(name);
						}
					}
				}
			});
			Console.WriteLine(string.Join("\r\n",listWritten));
			Assert.AreEqual(ArraySliceIds.Length*ArrayScales.Length,listWritten.Count);
		}
	}
}
