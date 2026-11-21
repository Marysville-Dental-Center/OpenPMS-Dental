// UI-9b-2's tests (docs/specs/ui-refresh-plan.md, section 7's UI-9b row and section 9; docs/port-notes/UI-9b.md, section 3): the imaging
// module's two ImageLists (ControlImages) become lists of icons from the one icon factory. The tools list is given to both image toolbars
// (ToolBarOD.SetImageListIcons) and keeps today's ImageIndex order; the tree's list is built at the DPI size (IconLibrary.GetImageListForSize)
// and again when the DPI changes, with today's order and its keys; a node set by ImageIndex or ImageKey still draws its picture.
// The gallery G-UI3 slice is opt-in: OPMS_UI9B_PICTURES names the folder for the pictures (toolbars\imaging-*, trees\imaging-tree), the
// before set from the extracted pictures of the old streams (OPMS_UI9B2_OLD_LISTS), all off screen, synthetic nodes, no patient data.
// No window is shown, no database.
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
using UiRefreshTests.Scan;

namespace UiRefreshTests.Toolbars {
	[TestClass]
	public class ImagingImageListTests {
		///<summary>The folder with the pictures of the old streams (tools-NN.png, tree-NN.png), for the gallery's before set.</summary>
		public const string VARIABLE_OLD_LISTS="OPMS_UI9B2_OLD_LISTS";
		///<summary>The scales of the tests and the gallery.</summary>
		public static readonly float[] ArrayScales=new float[] { 1f,1.5f,2f };
		///<summary>The imaging toolbars' ids in ToolBarSources.</summary>
		public static readonly string[] ArrayIds=new[] { "imaging-paint","imaging-draw" };

		///<summary>The tools list's pictures by today's ImageIndex (imageListTools2 before UI-9b-2, docs/port-notes/UI-9b.md), as the icon of each
		///picture's purpose.</summary>
		public static readonly string[] ArrayOrderTools=new[] { "PatSelect","Print","DeleteX","Info","Scan","Import","Paste","Crop","ZoomIn","ZoomOut",
			"Hand","Flip","RotateL","RotateR","ImageSelectorDoc","ScanPhoto","ScanXray","Copy","ScanMulti","Export","ArrowsAll" };
		///<summary>The tree list's pictures by today's ImageIndex (imageListTree before UI-9b-2).</summary>
		public static readonly string[] ArrayOrderTree=new[] { "UsbStick","ImageSelectorFolder","ImageSelectorDoc","ImageSelectorXray","ImageSelectorPhoto",
			"ImageSelectorFile","ImageSelectorMount","ImageSelectorFolderWeb" };
		///<summary>The buttons that took a picture by index before UI-9b-2: caption and index.</summary>
		private static readonly Dictionary<string,int> _dictOldIndex=new Dictionary<string,int> {
			{ "Crop",7 },{ "Pan",10 },{ "Adj",20 },{ "FlipH",11 },{ "-90",12 },{ "+90",13 }
		};

		#region Helpers
		private static ToolBarOD NewToolBar(float scale,int width96) {
			ToolBarOD toolBarOD=new ToolBarOD();
			toolBarOD.LayoutManager.SetScaleMS(scale);
			toolBarOD.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f*scale);
			toolBarOD.Size=new Size(UiTheme.ScaleI(width96,scale),UiTheme.ScaleI(ToolBarModel.HEIGHT_TOOLBAR,scale));
			return toolBarOD;
		}

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

		private static bool IsDark(Color color) {
			return color.R+color.G+color.B<3*128;
		}

		///<summary>The ink of a tree's picture: a pixel that is not the tree's white background.</summary>
		private static bool IsInk(Color color) {
			return color.R+color.G+color.B<3*235;
		}

		private static string GetPath(string folder,string name) {
			string path=Path.Combine(folder,name);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			return path;
		}

		///<summary>A TreeView of synthetic nodes, one for each picture of the list: the first by ImageIndex, the second by ImageKey, the rest by index.
		///At the scale, in today's font scaled, drawn off screen.</summary>
		private static Bitmap DrawTree(ImageList imageList,float scale,bool isByKey) {
			using(TreeView treeView=new TreeView()) {
				treeView.BorderStyle=BorderStyle.None;
				treeView.ShowLines=false;
				treeView.ShowRootLines=false;
				treeView.BackColor=Color.White;
				treeView.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f*scale);
				treeView.ImageList=imageList;
				for(int i=0;i<imageList.Images.Count;i++) {
					TreeNode treeNode=new TreeNode("Synthetic "+(i+1));
					if(isByKey && imageList.Images.Keys.Count>i && imageList.Images.Keys[i]!="") {
						treeNode.ImageKey=imageList.Images.Keys[i];
						treeNode.SelectedImageKey=imageList.Images.Keys[i];
					}
					else {
						treeNode.ImageIndex=i;
						treeNode.SelectedImageIndex=i;
					}
					treeView.Nodes.Add(treeNode);
				}
				treeView.Size=new Size(UiTheme.ScaleI(200,scale),Math.Max(treeView.ItemHeight,imageList.ImageSize.Height+2)*imageList.Images.Count+4);
				treeView.CreateControl();
				Bitmap bitmap=new Bitmap(treeView.Width,treeView.Height);
				treeView.DrawToBitmap(bitmap,new Rectangle(Point.Empty,treeView.Size));
				return bitmap;
			}
		}
		#endregion Helpers

		[TestMethod]
		public void ImagingLists_KeepTodaysIndexOrder_AsIcons() {
			CollectionAssert.AreEqual(ArrayOrderTools,ControlImages.ArrayIconsImageList.Select(x => x.ToString()).ToArray(),"The tools list's purposes in today's ImageIndex order.");
			CollectionAssert.AreEqual(ArrayOrderTree,ControlImages.ArrayIconsImageListTree.Select(x => x.ToString()).ToArray(),"The tree list's purposes in today's ImageIndex order.");
			foreach(EnumIcons icon in ControlImages.ArrayIconsImageList.Concat(ControlImages.ArrayIconsImageListTree)) {
				Assert.IsTrue(IconLibrary.GetDesigns(icon).Length>0,icon+" has no design.");
			}
			List<ToolBarSource> listToolBars=ToolBarSources.ReadAll();
			foreach(string id in ArrayIds) {
				EnumIcons[] arrayIcons=ToolBarModel.GetIconsImageList(listToolBars.Single(x => x.Id==id));
				Assert.IsNotNull(arrayIcons,id+": ControlImages has no "+ToolBarModel.FIELD_ICONS_IMAGE_LIST+".");
				CollectionAssert.AreEqual(ArrayOrderTools,arrayIcons.Select(x => x.ToString()).ToArray(),id);
			}
		}

		[TestMethod]
		public void ImagingToolbars_HaveNoImageIndexButtonLeft_TheirButtonsTakeAnIcon() {
			List<ToolBarSource> listToolBars=ToolBarSources.ReadAll();
			List<string> listLeft=new List<string>();
			foreach(string id in ArrayIds) {
				ToolBarSource toolBarSource=listToolBars.Single(x => x.Id==id);
				listLeft.AddRange(toolBarSource.ListButtons.Where(x => x.ImageIndex>=0).Select(x => id+": "+x.Text+" (ImageIndex "+x.ImageIndex+")"));
			}
			Assert.AreEqual(0,listLeft.Count,"Buttons of the imaging toolbars that still take a picture by ImageIndex:\r\n"+string.Join("\r\n",listLeft));
			ToolBarSource toolBarSourcePaint=listToolBars.Single(x => x.Id=="imaging-paint");
			foreach(KeyValuePair<string,string> pair in new Dictionary<string,string> { { "Crop","Crop" },{ "Pan","Hand" },{ "Adj","ArrowsAll" },{ "FlipH","Flip" },{ "-90","RotateL" },{ "+90","RotateR" } }) {
				Assert.AreEqual(pair.Value,toolBarSourcePaint.ListButtons.Single(x => x.Text==pair.Key).Icon,pair.Key+": its icon.");
			}
		}

		[TestMethod]
		public void ImagingToolbars_DrawTheirIcons_AndAnImageIndexButton_At100_150_200() {
			ToolBarSource toolBarSource=ToolBarSources.ReadAll().Single(x => x.Id=="imaging-paint");
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				foreach(float scale in ArrayScales) {
					string what=" at "+(int)(scale*100)+"%";
					using(ToolBarOD toolBarOD=NewToolBar(scale,674)) {
						ToolBarModel.SetImageList(toolBarOD,toolBarSource);
						Assert.IsTrue(toolBarOD.IsImageListIcons,"The toolbar holds a list of icons"+what+".");
						Assert.AreEqual(ControlImages.ArrayIconsImageList.Length,toolBarOD.ImageList.Images.Count);
						Assert.AreEqual(UiTheme.ScaleI(22,scale),toolBarOD.ImageList.ImageSize.Width,"The list is at the toolbar's scale"+what+".");
						foreach(ToolBarButtonSource toolBarButtonSource in toolBarSource.ListButtons) {
							toolBarOD.Buttons.Add(ToolBarModel.NewButton(toolBarButtonSource));
						}
						//A plug-in's button picks a picture by number: index 7 is the Crop purpose.
						ODToolBarButton buttonPlugIn=new ODToolBarButton("Plug-in",7,"","PlugIn");
						toolBarOD.Buttons.Add(buttonPlugIn);
						using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
							foreach(string text in _dictOldIndex.Keys) {
								ODToolBarButton odToolBarButton=toolBarOD.Buttons.Cast<ODToolBarButton>().Single(x => x.Text==text);
								Assert.IsTrue(CountColor(bitmap,GetImageRectangle(toolBarOD,odToolBarButton),IsDark)>UiTheme.ScaleI(10,scale),text+" draws its icon"+what+".");
							}
							Assert.IsTrue(CountColor(bitmap,GetImageRectangle(toolBarOD,buttonPlugIn),IsDark)>UiTheme.ScaleI(10,scale),"The button set by ImageIndex 7 draws the Crop picture"+what+".");
						}
					}
				}
			});
		}

		[TestMethod]
		public void TreeList_IsAtTheDpiSize_AndBuiltAgainWhenTheDpiChanges() {
			IconRendererTests.RunSta(() => {
				ImageList imageList=null;
				int countDisposed=0;
				foreach(int size in new[] { 16,24,32,24 }) {
					ImageList imageListBefore=imageList;
					EventHandler eventHandler=(sender,e) => countDisposed++;
					if(imageListBefore!=null) {
						imageListBefore.Disposed+=eventHandler;
					}
					imageList=IconLibrary.GetImageListForSize(imageList,ControlImages.ArrayIconsImageListTree,size);
					Assert.AreEqual(new Size(size,size),imageList.ImageSize,"The tree's list at the "+size+" px slot.");
					Assert.AreEqual(ColorDepth.Depth32Bit,imageList.ColorDepth);
					Assert.AreEqual(ControlImages.ArrayIconsImageListTree.Length,imageList.Images.Count);
					for(int i=0;i<ControlImages.ArrayIconsImageListTree.Length;i++) {
						Assert.AreEqual(i,imageList.Images.IndexOfKey(ControlImages.ArrayIconsImageListTree[i].ToString()),"The key of index "+i+".");
					}
					Assert.AreNotSame(imageListBefore,imageList,"A new size gives a new list.");
					//The same size gives the same list.
					Assert.AreSame(imageList,IconLibrary.GetImageListForSize(imageList,ControlImages.ArrayIconsImageListTree,size));
				}
				Assert.AreEqual(3,countDisposed,"The old list is disposed at each change.");
				imageList.Dispose();
				//The size follows the scale: 16 px at 100%, 24 at 150%, 32 at 200%.
				Assert.AreEqual(new[] { 16,24,32 },ArrayScales.Select(x => UiTheme.ScaleI(16,x)).ToArray());
			});
		}

		[TestMethod]
		public void TreeNode_SetByImageIndexOrImageKey_StillDrawsItsPicture_At100_150_200() {
			IconRendererTests.RunSta(() => {
				foreach(float scale in ArrayScales) {
					int size=UiTheme.ScaleI(16,scale);
					using(ImageList imageList=IconLibrary.GetImageListForSize(null,ControlImages.ArrayIconsImageListTree,size)) {
						foreach(bool isByKey in new[] { false,true }) {
							using(Bitmap bitmap=DrawTree(imageList,scale,isByKey)) {
								int heightRow=bitmap.Height/imageList.Images.Count;
								//Every node's row has ink in its picture's column (the first 2 + size px), whatever picks the picture.
								for(int i=0;i<imageList.Images.Count;i++) {
									Rectangle rectangle=new Rectangle(0,i*heightRow,size+8,heightRow);
									Assert.IsTrue(CountColor(bitmap,rectangle,IsInk)>=size,"Node "+i+" ("+(isByKey?"ImageKey":"ImageIndex")+") draws its picture at "+(int)(scale*100)+"%.");
								}
							}
						}
					}
				}
			});
		}

		///<summary>The G-UI3 slice's pictures (opt-in): the two image toolbars with every button their code can add and a plug-in button set by
		///ImageIndex 7 (today's Crop picture), and the tree with synthetic nodes, at 100, 150 and 200%, off screen. The before set draws the extracted
		///pictures of the old streams (OPMS_UI9B2_OLD_LISTS) through the toolbar's ImageList property and a 16 px tree list, as before UI-9b-2.</summary>
		[TestMethod]
		public void Gallery_ImagingToolbarsAndTree() {
			string folder=Environment.GetEnvironmentVariable(ToolBarImageListTests.VARIABLE_PICTURES);
			if(string.IsNullOrWhiteSpace(folder)) {
				Assert.Inconclusive("Opt-in: set "+ToolBarImageListTests.VARIABLE_PICTURES+" to a folder for the G-UI3 slice's pictures.");
			}
			string folderOld=Environment.GetEnvironmentVariable(VARIABLE_OLD_LISTS);
			bool isBefore=Path.GetFileName(folder.TrimEnd('\\','/'))=="before";
			if(isBefore && string.IsNullOrWhiteSpace(folderOld)) {
				Assert.Inconclusive("The before set needs "+VARIABLE_OLD_LISTS+" (the pictures of the old streams).");
			}
			List<ToolBarSource> listToolBars=ToolBarSources.ReadAll();
			List<string> listWritten=new List<string>();
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				UiTheme.InitWinForms();
				foreach(string id in ArrayIds) {
					ToolBarSource toolBarSource=listToolBars.Single(x => x.Id==id);
					int width96=toolBarSource.FuncWidth(ToolBarSources.GetModuleWidth(ToolBarPictureTests.WIDTH_WINDOW_STRIPS));
					foreach(float scale in ArrayScales) {
						using(ToolBarOD toolBarOD=NewToolBar(scale,width96+120)) {
							if(isBefore) {
								ImageList imageListOld=new ImageList();
								imageListOld.ColorDepth=ColorDepth.Depth32Bit;
								imageListOld.ImageSize=new Size(22,22);
								for(int i=0;i<21;i++) {
									using(Bitmap bitmap=new Bitmap(Path.Combine(folderOld,"tools-"+i.ToString("00")+".png"))) {
										imageListOld.Images.Add(new Bitmap(bitmap));
									}
								}
								toolBarOD.ImageList=imageListOld;//a list set by the caller: 96 DPI pictures, drawn scaled, as before
								foreach(ToolBarButtonSource toolBarButtonSource in toolBarSource.ListButtons) {
									ToolBarButtonSource copy=toolBarButtonSource.Copy();
									if(copy.Icon!="" && _dictOldIndex.ContainsKey(copy.Text)) {
										copy.Icon="";
										copy.ImageIndex=_dictOldIndex[copy.Text];
									}
									toolBarOD.Buttons.Add(ToolBarModel.NewButton(copy));
								}
							}
							else {
								ToolBarModel.SetImageList(toolBarOD,toolBarSource);
								foreach(ToolBarButtonSource toolBarButtonSource in toolBarSource.ListButtons) {
									toolBarOD.Buttons.Add(ToolBarModel.NewButton(toolBarButtonSource));
								}
							}
							toolBarOD.Buttons.Add(new ODToolBarButton("Plug-in",7,"","PlugIn"));
							string path=GetPath(folder,"toolbars\\"+id+"-"+(int)(scale*100)+".png");
							using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
								bitmap.Save(path,ImageFormat.Png);
							}
							listWritten.Add(path);
						}
					}
				}
				foreach(float scale in ArrayScales) {
					ImageList imageList;
					if(isBefore) {
						imageList=new ImageList();
						imageList.ColorDepth=ColorDepth.Depth32Bit;
						imageList.ImageSize=new Size(16,16);
						for(int i=0;i<8;i++) {
							using(Bitmap bitmap=new Bitmap(Path.Combine(folderOld,"tree-"+i.ToString("00")+".png"))) {
								imageList.Images.Add(new Bitmap(bitmap));
							}
						}
					}
					else {
						imageList=IconLibrary.GetImageListForSize(null,ControlImages.ArrayIconsImageListTree,UiTheme.ScaleI(16,scale));
					}
					using(imageList) {
						string path=GetPath(folder,"trees\\imaging-tree-"+(int)(scale*100)+".png");
						using(Bitmap bitmap=DrawTree(imageList,scale,false)) {
							bitmap.Save(path,ImageFormat.Png);
						}
						listWritten.Add(path);
					}
				}
			});
			Console.WriteLine(string.Join("\r\n",listWritten));
			Assert.AreEqual(ArrayIds.Length*ArrayScales.Length+ArrayScales.Length,listWritten.Count);
		}
	}
}
