// UI-9b-3's tests (docs/specs/ui-refresh-plan.md, section 7's UI-9b row and section 9; docs/port-notes/UI-9b.md, section 4): the other trees' and
// lists' ImageLists (the task lists and the reminders grid, the setup wizard, the security tree, the auto notes, the email inbox) become lists of
// icons from the one icon factory. Each list keeps today's ImageIndex order; each tree's list is at the DPI size (16 px at 100%, 24 at 150%, 32 at
// 200%) and built again when the scale changes; a node set by ImageIndex or ImageKey still draws its picture; a grid column's list draws its pictures
// by the index in the cell; a label and a button show their picture; the state pictures (a task's new, read and done state, a permission granted,
// denied or partial, a selected information label) stay clearly distinguishable, by shape as well as by colour, and keep the role colour that
// carried the meaning. The gallery G-UI3 slice is opt-in: OPMS_UI9B_PICTURES names the folder for the pictures (trees, lists, toolbars, buttons), the
// before set from the extracted pictures of the old streams (OPMS_UI9B3_OLD_LISTS), all off screen, synthetic nodes, no patient data. No window is
// shown, no database.
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
using OpenPmsDental.User_Controls.SetupWizard;
using UiRefreshTests.Scan;

namespace UiRefreshTests.Toolbars {
	[TestClass]
	public class OtherTreesImageListTests {
		///<summary>The folder with the pictures of the old streams (&lt;window&gt;-&lt;list&gt;-NN.png), for the gallery's before set.</summary>
		public const string VARIABLE_OLD_LISTS="OPMS_UI9B3_OLD_LISTS";
		///<summary>The scales of the tests and the gallery.</summary>
		public static readonly float[] ArrayScales=new float[] { 1f,1.5f,2f };

		///<summary>Each list's pictures by today's index (the designers' streams before UI-9b-3, docs/port-notes/UI-9b.md, section 4) as the icon of
		///each picture's purpose, with the number of pictures the old stream held.</summary>
		private static readonly Dictionary<string,string[]> _dictOrder=new Dictionary<string,string[]> {
			{ "tasks",new[] { "TaskList","CheckBoxChecked","CheckBoxEmpty","TaskListNew","TaskNew" } },
			{ "tasks-toolbar",new[] { "TaskListAdd","Add" } },
			{ "reminders",new[] { "TaskList","CheckBoxChecked","CheckBoxEmpty","TaskListNew","TaskNew" } },
			{ "security",new[] { "ImageSelectorFolder","CheckBoxEmpty","CheckBoxChecked","CheckBoxPartial" } },
			{ "autonotes",new[] { "ImageSelectorFolder","ImageSelectorDoc" } },
			{ "autonotes-export",new[] { "ImageSelectorFolder","ImageSelectorDoc" } },
			{ "email-buttons",new[] { "Edit","Sync","Reply","Search","ReplyAll","Forward" } },
			{ "email-mailboxes",new[] { "Inbox","Send" } },
			{ "wizard",new[] { "Info" } },
			{ "wizard-features",new[] { "Info","InfoSelected" } },
			{ "wizard-feesched",new[] { "Info" } },
			{ "wizard-printer",new[] { "Info" } },
		};

		private static EnumIcons[] GetList(string id) {
			switch(id) {
				case "tasks": return UserControlTasks.ArrayIconsImageListTasks;
				case "tasks-toolbar": return UserControlTasks.ArrayIconsImageListMain;
				case "reminders": return ControlAppt.ArrayIconsImageListTasks;
				case "security": return UserControlSecurityTree.ArrayIconsImageListPerm;
				case "autonotes": return FormAutoNotes.ArrayIconsImageListTree;
				case "autonotes-export": return FormAutoNoteExport.ArrayIconsImageListTree;
				case "email-buttons": return FormEmailInbox.ArrayIconsImageListButtons;
				case "email-mailboxes": return FormEmailInbox.ArrayIconsImageListMailboxes;
				case "wizard": return FormSetupWizard.ArrayIconsImageList;
				case "wizard-features": return UserControlSetupWizFeatures.ArrayIconsImageList;
				case "wizard-feesched": return UserControlSetupWizFeeSched.ArrayIconsImageList;
				case "wizard-printer": return UserControlSetupWizPrinter.ArrayIconsImageList;
			}
			throw new ArgumentException(id);
		}

		#region Helpers
		private static bool IsInk(Color color) {
			return color.R+color.G+color.B<3*250;
		}

		private static bool IsDark(Color color) {
			return color.R+color.G+color.B<3*128;
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

		///<summary>The shape of an icon's picture: which pixels are mostly opaque.</summary>
		private static bool[] GetMask(EnumIcons icon,int size) {
			using(Bitmap bitmap=IconLibrary.GetBitmap(icon,size,size)) {
				bool[] arrayMask=new bool[size*size];
				for(int y=0;y<size;y++) {
					for(int x=0;x<size;x++) {
						arrayMask[y*size+x]=bitmap.GetPixel(x,y).A>=128;
					}
				}
				return arrayMask;
			}
		}

		private static string GetPath(string folder,string name) {
			string path=Path.Combine(folder,name);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			return path;
		}

		private static string GetScaleText(float scale) {
			return ((int)Math.Round(scale*100)).ToString();
		}

		///<summary>A TreeView of synthetic nodes, one for each picture of the list: the first by ImageIndex, the second by ImageKey, the rest by index.
		///At the scale, in today's font scaled, drawn off screen.</summary>
		private static Bitmap DrawTree(ImageList imageList,float scale,bool isByKey,int heightItem96) {
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
				treeView.ItemHeight=UiTheme.ScaleI(heightItem96,scale);
				treeView.Size=new Size(UiTheme.ScaleI(200,scale),treeView.ItemHeight*imageList.Images.Count+4);
				treeView.CreateControl();
				Bitmap bitmap=new Bitmap(treeView.Width,treeView.Height);
				treeView.DrawToBitmap(bitmap,new Rectangle(Point.Empty,treeView.Size));
				return bitmap;
			}
		}

		///<summary>A grid of the scale with a first column that shows the list's pictures by the index in the cell (the task lists' and the setup wizard's
		///way), one row for each index, a description in the next column.</summary>
		private static Bitmap DrawGrid(ImageList imageList,float scale,string title,int countRows) {
			using(OpenPmsDental.UI.GridOD grid=new OpenPmsDental.UI.GridOD()) {
				grid.TranslationName="";
				grid.Size=new Size(UiTheme.ScaleI(200,scale),UiTheme.ScaleI(28+16*countRows+6,scale));
				grid.SetScaleAndZoom(1,scale);
				grid.BeginUpdate();
				grid.Title=title;
				grid.Columns.Clear();
				OpenPmsDental.UI.GridColumn gridColumn=new OpenPmsDental.UI.GridColumn("",17);
				gridColumn.ImageList=imageList;
				grid.Columns.Add(gridColumn);
				grid.Columns.Add(new OpenPmsDental.UI.GridColumn("Description",150));
				grid.ListGridRows.Clear();
				for(int i=0;i<countRows;i++) {
					grid.ListGridRows.Add(new OpenPmsDental.UI.GridRow(i.ToString(),"Synthetic "+(i+1)));
				}
				grid.EndUpdate();
				Bitmap bitmap=new Bitmap(grid.Width,grid.Height,PixelFormat.Format32bppArgb);
				grid.DrawToBitmap(bitmap,new Rectangle(0,0,grid.Width,grid.Height));
				return bitmap;
			}
		}

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

		///<summary>The tasks toolbar's buttons as UserControlTasks.LayoutToolBar adds them that take a picture (the others have none).</summary>
		private static void AddTaskToolBarButtons(ToolBarOD toolBarOD,bool isIcon) {
			toolBarOD.Buttons.Add(new ODToolBarButton("Options",-1,"","Options"));
			if(isIcon) {
				toolBarOD.Buttons.Add(new ODToolBarButton("Add Task List",EnumIcons.TaskListAdd,"","AddList"));
				toolBarOD.Buttons.Add(new ODToolBarButton("Add Task",EnumIcons.Add,"","AddTask"));
			}
			else {
				toolBarOD.Buttons.Add(new ODToolBarButton("Add Task List",0,"","AddList"));
				toolBarOD.Buttons.Add(new ODToolBarButton("Add Task",1,"","AddTask"));
			}
			toolBarOD.Buttons.Add(new ODToolBarButton("Search",-1,"","Search"));
		}

		///<summary>A list of the old pictures (&lt;window&gt;-&lt;list&gt;-NN.png) at their size, as the old stream held them.</summary>
		private static ImageList LoadOldList(string folder,string window,string list,int count,int size) {
			ImageList imageList=new ImageList();
			imageList.ColorDepth=ColorDepth.Depth32Bit;
			imageList.ImageSize=new Size(size,size);
			for(int i=0;i<count;i++) {
				using(Bitmap bitmap=new Bitmap(Path.Combine(folder,window+"-"+list+"-"+i.ToString("00")+".png"))) {
					imageList.Images.Add(new Bitmap(bitmap));
				}
			}
			return imageList;
		}

		private static Bitmap LoadOld(string folder,string window,string list,int index) {
			using(Bitmap bitmap=new Bitmap(Path.Combine(folder,window+"-"+list+"-"+index.ToString("00")+".png"))) {
				return new Bitmap(bitmap);
			}
		}
		#endregion Helpers

		[TestMethod]
		public void Lists_KeepTodaysIndexOrder_AsIcons() {
			foreach(KeyValuePair<string,string[]> pair in _dictOrder) {
				EnumIcons[] arrayIcons=GetList(pair.Key);
				CollectionAssert.AreEqual(pair.Value,arrayIcons.Select(x => x.ToString()).ToArray(),pair.Key+": the purposes in today's ImageIndex order.");
				foreach(EnumIcons icon in arrayIcons) {
					Assert.IsTrue(IconLibrary.GetDesigns(icon).Length>0,pair.Key+": "+icon+" has no design.");
				}
			}
			//The task pictures are the same in the task lists and the reminders grid.
			CollectionAssert.AreEqual(UserControlTasks.ArrayIconsImageListTasks,ControlAppt.ArrayIconsImageListTasks);
		}

		[TestMethod]
		public void Designers_HaveNoStreamLeft_ButtonsAndToolbarTakeTheirIcons() {
			string root=RepoPaths.Root();
			string[] arrayDesigners=new[] {
				"OpenPmsDental\\User Controls\\UserControlTasks.Designer.cs","OpenPmsDental\\Main Modules\\ControlAppt.Designer.cs",
				"OpenPmsDental\\User Controls\\UserControlSecurityTree.Designer.cs","OpenPmsDental\\Forms\\FormAutoNotes.designer.cs","OpenPmsDental\\Forms\\FormAutoNoteExport.Designer.cs",
				"OpenPmsDental\\Forms\\FormEmailInbox.designer.cs","OpenPmsDental\\Forms\\FormSetupWizard.designer.cs","OpenPmsDental\\Forms\\FormSetupWizardProgress.designer.cs",
				"OpenPmsDental\\User Controls\\SetupWizControls\\UserControlSetupWizDefinitions.Designer.cs","OpenPmsDental\\User Controls\\SetupWizControls\\UserControlSetupWizFeatures.Designer.cs",
				"OpenPmsDental\\User Controls\\SetupWizControls\\UserControlSetupWizFeeSched.Designer.cs","OpenPmsDental\\User Controls\\SetupWizControls\\UserControlSetupWizPractice.Designer.cs",
				"OpenPmsDental\\User Controls\\SetupWizControls\\UserControlSetupWizPrinter.Designer.cs","OpenPmsDental\\User Controls\\SetupWizControls\\UserControlSetupWizSchedule.Designer.cs",
			};
			foreach(string designer in arrayDesigners) {
				string text=File.ReadAllText(Path.Combine(root,designer));
				Assert.IsFalse(text.Contains("ImageListStreamer"),designer+" still loads an ImageList stream.");
				Assert.IsFalse(text.Contains("System.Windows.Forms.ImageList"),designer+" still has an ImageList.");
			}
			string textEmail=File.ReadAllText(Path.Combine(root,"OpenPmsDental\\Forms\\FormEmailInbox.designer.cs"));
			foreach(KeyValuePair<string,string> pair in new Dictionary<string,string> { { "butCompose","Edit" },{ "butRefresh","Sync" },{ "butReply","Reply" },{ "butSearch","Search" },{ "butReplyAll","ReplyAll" },{ "butForward","Forward" } }) {
				Assert.IsTrue(textEmail.Contains("this."+pair.Key+".Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons."+pair.Value+",16,16);"),pair.Key);
			}
			string textProgress=File.ReadAllText(Path.Combine(root,"OpenPmsDental\\Forms\\FormSetupWizardProgress.designer.cs"));
			foreach(KeyValuePair<string,string> pair in new Dictionary<string,string> { { "butSkip","ChevronDoubleRight" },{ "butNext","ChevronRight" },{ "butBack","ChevronLeft" } }) {
				Assert.IsTrue(textProgress.Contains("this."+pair.Key+".Image = OpenPmsDental.UI.IconLibrary.GetBitmap(OpenPmsDental.UI.EnumIcons."+pair.Value+",16,16);"),pair.Key);
			}
			string textTasks=File.ReadAllText(Path.Combine(root,"OpenPmsDental\\User Controls\\UserControlTasks.cs"));
			Assert.IsTrue(textTasks.Contains("ToolBarMain.SetImageListIcons(ArrayIconsImageListMain);"));
			Assert.IsTrue(textTasks.Contains("\"Add Task List\"),EnumIcons.TaskListAdd,"));
			Assert.IsTrue(textTasks.Contains("\"Add Task\"),EnumIcons.Add,"));
		}

		[TestMethod]
		public void StatePictures_StayDistinguishable_ByShapeAndKeepTheirMeaningColour() {
			IconRendererTests.RunSta(() => {
				foreach(string id in new[] { "tasks","security","wizard-features" }) {
					EnumIcons[] arrayIcons=GetList(id);
					foreach(int size in new[] { 16,24,32 }) {
						for(int a=0;a<arrayIcons.Length;a++) {
							for(int b=a+1;b<arrayIcons.Length;b++) {
								Assert.IsFalse(GetMask(arrayIcons[a],size).SequenceEqual(GetMask(arrayIcons[b],size)),
									id+": "+arrayIcons[a]+" and "+arrayIcons[b]+" have the same shape at "+size+" px (colour alone would carry the state).");
							}
						}
					}
				}
				//The colour that carried the meaning is the icon's role colour: green for done and granted, orange for new.
				foreach(KeyValuePair<EnumIcons,EnumUiColor> pair in new Dictionary<EnumIcons,EnumUiColor> {
					{ EnumIcons.CheckBoxChecked,EnumUiColor.Success },{ EnumIcons.CheckBoxPartial,EnumUiColor.Success },
					{ EnumIcons.TaskNew,EnumUiColor.Warning },{ EnumIcons.TaskListNew,EnumUiColor.Warning },{ EnumIcons.InfoSelected,EnumUiColor.Accent } })
				{
					Color color=UiTheme.GetColor(pair.Value);
					Assert.AreEqual(color.ToArgb(),IconLibrary.GetColor(pair.Key).ToArgb(),pair.Key+": its role colour.");
					using(Bitmap bitmap=IconLibrary.GetBitmap(pair.Key,16,16)) {
						int countSolid=0;
						for(int y=0;y<16;y++) {
							for(int x=0;x<16;x++) {
								Color colorPixel=bitmap.GetPixel(x,y);
								if(colorPixel.A>=128) {
									countSolid++;
									Assert.IsTrue(Math.Abs(colorPixel.R-color.R)<=2 && Math.Abs(colorPixel.G-color.G)<=2 && Math.Abs(colorPixel.B-color.B)<=2,pair.Key+": ink in the role colour.");
								}
							}
						}
						Assert.IsTrue(countSolid>=20,pair.Key+" draws its picture.");
					}
				}
				Assert.AreNotEqual(UiTheme.GetColor(EnumUiColor.Success).ToArgb(),UiTheme.GetColor(EnumUiColor.Warning).ToArgb());
			});
		}

		[TestMethod]
		public void TreeLists_AreAtTheDpiSize_AndBuiltAgainWhenTheDpiChanges() {
			IconRendererTests.RunSta(() => {
				foreach(string id in new[] { "tasks","security","autonotes","autonotes-export" }) {
					EnumIcons[] arrayIcons=GetList(id);
					ImageList imageList=null;
					int countDisposed=0;
					foreach(int size in new[] { 16,24,32,24 }) {
						ImageList imageListBefore=imageList;
						if(imageListBefore!=null) {
							imageListBefore.Disposed+=(sender,e) => countDisposed++;
						}
						imageList=IconLibrary.GetImageListForSize(imageList,arrayIcons,size);
						Assert.AreEqual(new Size(size,size),imageList.ImageSize,id+": the list at the "+size+" px slot.");
						Assert.AreEqual(ColorDepth.Depth32Bit,imageList.ColorDepth);
						Assert.AreEqual(arrayIcons.Length,imageList.Images.Count);
						for(int i=0;i<arrayIcons.Length;i++) {
							Assert.AreEqual(arrayIcons[i].ToString(),imageList.Images.Keys[i],id+": the key of index "+i+".");
						}
						Assert.AreSame(imageList,IconLibrary.GetImageListForSize(imageList,arrayIcons,size),"The same size gives the same list.");
					}
					Assert.AreEqual(3,countDisposed,id+": the old list is disposed at each change.");
					imageList.Dispose();
				}
				CollectionAssert.AreEqual(new[] { 16,24,32 },ArrayScales.Select(x => UiTheme.ScaleI(16,x)).ToArray());
			});
		}

		[TestMethod]
		public void SecurityTreeControl_BuildsItsListAtTheScale_AndAgainWhenTheScaleChanges() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				using(UserControlSecurityTree control=new UserControlSecurityTree()) {
					TreeView treeView=control.Controls.OfType<TreeView>().Single();
					Assert.IsNotNull(treeView.ImageList,"The tree has a list of icons from the constructor on.");
					Assert.AreEqual(new Size(16,16),treeView.ImageList.ImageSize);
					Assert.AreEqual(4,treeView.ImageList.Images.Count);
					ImageList imageListFirst=treeView.ImageList;
					int widthStart=control.Width;
					foreach(float scale in new[] { 1.5f,2f,1f }) {
						control.LayoutManager=new LayoutManagerForms();
						control.LayoutManager.SetScaleMS(scale);
						control.Width=++widthStart;//the SizeChanged that follows a scale change
						int size=UiTheme.ScaleI(16,scale);
						Assert.AreEqual(new Size(size,size),treeView.ImageList.ImageSize,"The tree's list follows the scale "+scale+".");
						Assert.AreEqual(size,treeView.ItemHeight,"The rows are as high as the picture.");
						Assert.AreEqual(4,treeView.ImageList.Images.Count);
					}
					Assert.AreNotSame(imageListFirst,treeView.ImageList);
					Assert.AreEqual(new Size(16,16),treeView.ImageList.ImageSize);
				}
			});
		}

		[TestMethod]
		public void TreeNode_SetByImageIndexOrImageKey_StillDrawsItsPicture_At100_150_200() {
			IconRendererTests.RunSta(() => {
				foreach(string id in new[] { "tasks","security","autonotes" }) {
					foreach(float scale in ArrayScales) {
						int size=UiTheme.ScaleI(16,scale);
						using(ImageList imageList=IconLibrary.GetImageListForSize(null,GetList(id),size)) {
							foreach(bool isByKey in new[] { false,true }) {
								using(Bitmap bitmap=DrawTree(imageList,scale,isByKey,16)) {
									int heightRow=(bitmap.Height-4)/imageList.Images.Count;
									for(int i=0;i<imageList.Images.Count;i++) {
										Rectangle rectangle=new Rectangle(0,i*heightRow,size+8,heightRow);
										Assert.IsTrue(CountColor(bitmap,rectangle,IsInk)>=size,id+": node "+i+" ("+(isByKey?"ImageKey":"ImageIndex")+") draws its picture at "+(int)(scale*100)+"%.");
									}
								}
							}
						}
					}
				}
			});
		}

		[TestMethod]
		public void GridColumnLists_DrawTheirPictureByTheIndexInTheCell_At100_150_200() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				foreach(string id in new[] { "tasks","reminders","wizard" }) {
					EnumIcons[] arrayIcons=GetList(id);
					using(ImageList imageList=IconLibrary.GetImageList(arrayIcons,16)) {
						Assert.AreEqual(new Size(16,16),imageList.ImageSize,id+": the grid draws a column's pictures at the scale of the window, so the list is the 96 DPI size.");
						foreach(float scale in ArrayScales) {
							using(Bitmap bitmap=DrawGrid(imageList,scale,id,arrayIcons.Length)) {
								//Below the title and the header, each row's first column holds ink (its picture), whatever the scale.
								int originY=UiTheme.ScaleI(28,scale);
								int heightRow=(bitmap.Height-originY-UiTheme.ScaleI(6,scale))/arrayIcons.Length;
								for(int i=0;i<arrayIcons.Length;i++) {
									Rectangle rectangle=new Rectangle(2,originY+i*heightRow+2,UiTheme.ScaleI(14,scale),heightRow-4);//inside the gridlines
									Assert.IsTrue(CountColor(bitmap,rectangle,IsInk)>=UiTheme.ScaleI(6,scale),id+": row "+i+" draws its picture at "+(int)(scale*100)+"%.");
								}
							}
						}
					}
				}
			});
		}

		[TestMethod]
		public void TasksToolbar_DrawsItsIcons_AndAnImageIndexButton_At100_150_200() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				foreach(float scale in ArrayScales) {
					string what=" at "+(int)(scale*100)+"%";
					using(ToolBarOD toolBarOD=NewToolBar(scale,500)) {
						toolBarOD.SetImageListIcons(UserControlTasks.ArrayIconsImageListMain);
						Assert.IsTrue(toolBarOD.IsImageListIcons,"The toolbar holds a list of icons"+what+".");
						Assert.AreEqual(2,toolBarOD.ImageList.Images.Count);
						Assert.AreEqual(UiTheme.ScaleI(22,scale),toolBarOD.ImageList.ImageSize.Width,"The list is at the toolbar's scale"+what+".");
						AddTaskToolBarButtons(toolBarOD,true);
						//A plug-in's button picks a picture by number: index 0 is the Add Task List purpose.
						ODToolBarButton buttonPlugIn=new ODToolBarButton("Plug-in",0,"","PlugIn");
						toolBarOD.Buttons.Add(buttonPlugIn);
						using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
							foreach(string text in new[] { "Add Task List","Add Task" }) {
								ODToolBarButton odToolBarButton=toolBarOD.Buttons.Cast<ODToolBarButton>().Single(x => x.Text==text);
								Assert.IsTrue(CountColor(bitmap,GetImageRectangle(toolBarOD,odToolBarButton),IsInk)>UiTheme.ScaleI(10,scale),text+" draws its icon"+what+".");
							}
							Assert.IsTrue(CountColor(bitmap,GetImageRectangle(toolBarOD,buttonPlugIn),IsInk)>UiTheme.ScaleI(10,scale),"The button set by ImageIndex 0 draws its picture"+what+".");
						}
					}
				}
			});
		}

		[TestMethod]
		public void InformationLabels_ShowTheirPicture_AndTheSelectedOneDiffers() {
			IconRendererTests.RunSta(() => {
				using(ImageList imageList=IconLibrary.GetImageList(UserControlSetupWizFeatures.ArrayIconsImageList,16)) {
					Bitmap[] arrayBitmaps=new Bitmap[2];
					for(int i=0;i<2;i++) {
						using(Label label=new Label()) {
							label.Size=new Size(20,20);
							label.BackColor=Color.White;
							label.ImageList=imageList;
							label.ImageIndex=i;
							label.CreateControl();
							arrayBitmaps[i]=new Bitmap(20,20);
							label.DrawToBitmap(arrayBitmaps[i],new Rectangle(0,0,20,20));
						}
						Assert.IsTrue(CountColor(arrayBitmaps[i],new Rectangle(0,0,20,20),IsInk)>=20,"The label draws its picture, index "+i+".");
					}
					int countDifferent=0;
					for(int y=0;y<20;y++) {
						for(int x=0;x<20;x++) {
							if(arrayBitmaps[0].GetPixel(x,y)!=arrayBitmaps[1].GetPixel(x,y)) {
								countDifferent++;
							}
						}
					}
					Assert.IsTrue(countDifferent>=20,"The selected label's picture differs from the unselected one's.");
					arrayBitmaps[0].Dispose();
					arrayBitmaps[1].Dispose();
				}
			});
		}

		[TestMethod]
		public void IconButtons_DrawTheirPicture_At100_150_200() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				UiTheme.InitWinForms();
				foreach(float scale in ArrayScales) {
					foreach(EnumIcons icon in FormEmailInbox.ArrayIconsImageListButtons.Concat(new[] { EnumIcons.ChevronLeft,EnumIcons.ChevronRight,EnumIcons.ChevronDoubleRight })) {
						using(OpenPmsDental.UI.Button button=new OpenPmsDental.UI.Button()) {
							button.LayoutManager.SetScaleMS(scale);
							button.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f*scale);
							button.Size=new Size(UiTheme.ScaleI(75,scale),UiTheme.ScaleI(20,scale));
							button.Text="Text";
							button.Image=IconLibrary.GetBitmap(icon,16,16);
							button.ImageAlign=ContentAlignment.MiddleLeft;
							using(Bitmap bitmap=ToolBarPictureTests.Draw(button)) {
								Rectangle rectangle=new Rectangle(UiTheme.ScaleI(6,scale),0,UiTheme.ScaleI(16,scale),bitmap.Height);
								Assert.IsTrue(CountColor(bitmap,rectangle,IsDark)>=UiTheme.ScaleI(6,scale),icon+": the button draws its picture at "+(int)(scale*100)+"%.");
							}
						}
					}
				}
			});
		}

		///<summary>The G-UI3 slice's pictures (opt-in): each changed tree, grid list, toolbar and button group at 100, 150 and 200%, off screen, with
		///synthetic nodes. The before set draws the extracted pictures of the old streams (OPMS_UI9B3_OLD_LISTS) as before UI-9b-3.</summary>
		[TestMethod]
		public void Gallery_OtherTreesAndLists() {
			string folder=Environment.GetEnvironmentVariable(ToolBarImageListTests.VARIABLE_PICTURES);
			if(string.IsNullOrWhiteSpace(folder)) {
				Assert.Inconclusive("Opt-in: set "+ToolBarImageListTests.VARIABLE_PICTURES+" to a folder for the G-UI3 slice's pictures.");
			}
			string folderOld=Environment.GetEnvironmentVariable(VARIABLE_OLD_LISTS);
			bool isBefore=Path.GetFileName(folder.TrimEnd('\\','/'))=="before";
			if(isBefore && string.IsNullOrWhiteSpace(folderOld)) {
				Assert.Inconclusive("The before set needs "+VARIABLE_OLD_LISTS+" (the pictures of the old streams).");
			}
			List<string> listWritten=new List<string>();
			Action<string,Bitmap> actionSave=(name,bitmap) => {
				string path=GetPath(folder,name);
				using(bitmap) {
					bitmap.Save(path,ImageFormat.Png);
				}
				listWritten.Add(path);
			};
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				UiTheme.InitWinForms();
				//Trees: the old lists were 16 px (15 px for the security tree) whatever the scale; the new ones follow the scale.
				foreach(float scale in ArrayScales) {
					string scaleText=GetScaleText(scale);
					foreach(string[] arrayTree in new[] { new[] { "tasks","UserControlTasks","imageListTree","5","16","18" },new[] { "security","UserControlSecurityTree","imageListPerm","4","15","16" },
						new[] { "autonotes","FormAutoNotes","imageListTree","2","16","16" } })
					{
						ImageList imageList;
						int heightItem96=int.Parse(arrayTree[5]);
						if(isBefore) {
							imageList=LoadOldList(folderOld,arrayTree[1],arrayTree[2],int.Parse(arrayTree[3]),int.Parse(arrayTree[4]));
							heightItem96=arrayTree[0]=="security" ? 15 : 16;
						}
						else {
							imageList=IconLibrary.GetImageListForSize(null,GetList(arrayTree[0]),UiTheme.ScaleI(16,scale));
						}
						using(imageList) {
							actionSave("trees\\"+arrayTree[0]+"-tree-"+scaleText+".png",DrawTree(imageList,scale,false,heightItem96));
						}
					}
					//Grid columns: both draw a 16 px list, the grid scales it.
					foreach(string[] arrayGrid in new[] { new[] { "tasks","UserControlTasks","imageListTree","5" },new[] { "wizard","FormSetupWizard","imageList1","1" } }) {
						ImageList imageList=isBefore ? LoadOldList(folderOld,arrayGrid[1],arrayGrid[2],int.Parse(arrayGrid[3]),16) : IconLibrary.GetImageList(GetList(arrayGrid[0]),16);
						using(imageList) {
							actionSave("lists\\"+arrayGrid[0]+"-grid-"+scaleText+".png",DrawGrid(imageList,scale,arrayGrid[0],int.Parse(arrayGrid[3])));
						}
					}
					//The tasks toolbar, with a plug-in button set by ImageIndex 0.
					using(ToolBarOD toolBarOD=NewToolBar(scale,360)) {
						if(isBefore) {
							ImageList imageListOld=LoadOldList(folderOld,"UserControlTasks","imageListMain",2,22);
							toolBarOD.ImageList=imageListOld;
							AddTaskToolBarButtons(toolBarOD,false);
						}
						else {
							toolBarOD.SetImageListIcons(UserControlTasks.ArrayIconsImageListMain);
							AddTaskToolBarButtons(toolBarOD,true);
						}
						toolBarOD.Buttons.Add(new ODToolBarButton("Plug-in",0,"","PlugIn"));
						actionSave("toolbars\\tasks-"+scaleText+".png",ToolBarPictureTests.Draw(toolBarOD));
					}
					//The email inbox's six buttons and the setup wizard's three, 75 x 20 and 75 x 23 at 100%, side by side.
					List<Bitmap> listButtons=new List<Bitmap>();
					string[] arrayTexts=new[] { "Compose","Refresh","Reply","Search","Reply All","Forward","Skip","Next","Back" };
					EnumIcons[] arrayIcons=FormEmailInbox.ArrayIconsImageListButtons.Concat(new[] { EnumIcons.ChevronDoubleRight,EnumIcons.ChevronRight,EnumIcons.ChevronLeft }).ToArray();
					for(int i=0;i<arrayTexts.Length;i++) {
						using(OpenPmsDental.UI.Button button=new OpenPmsDental.UI.Button()) {
							button.LayoutManager.SetScaleMS(scale);
							button.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f*scale);
							button.Size=new Size(UiTheme.ScaleI(75,scale),UiTheme.ScaleI(i<6?20:23,scale));
							button.Text=arrayTexts[i];
							button.ImageAlign=i==6||i==7 ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft;
							if(isBefore) {
								string window=i<6 ? "FormEmailInbox" : "FormSetupWizardProgress";
								string list=i<6 ? "imageListButtonSmall" : "imageList1";
								int index=i<6 ? i : (i==6 ? 0 : (i==7 ? 1 : 2));
								button.Image=LoadOld(folderOld,window,list,index);
							}
							else {
								button.Image=IconLibrary.GetBitmap(arrayIcons[i],16,16);
							}
							listButtons.Add(ToolBarPictureTests.Draw(button));
						}
					}
					int widthAll=listButtons.Sum(x => x.Width+UiTheme.ScaleI(6,scale));
					Bitmap bitmapButtons=new Bitmap(widthAll,listButtons.Max(x => x.Height)+UiTheme.ScaleI(8,scale));
					using(Graphics g=Graphics.FromImage(bitmapButtons)) {
						g.Clear(SystemColors.Control);
						int x=UiTheme.ScaleI(3,scale);
						foreach(Bitmap bitmapButton in listButtons) {
							g.DrawImage(bitmapButton,x,UiTheme.ScaleI(4,scale));
							x+=bitmapButton.Width+UiTheme.ScaleI(6,scale);
							bitmapButton.Dispose();
						}
					}
					actionSave("buttons\\email-and-wizard-"+scaleText+".png",bitmapButtons);
					//The setup wizard's information labels: unselected and selected, 20 x 20 at 100%, 16 px pictures (a label draws its picture at its own size).
					ImageList imageListInfo=isBefore ? LoadOldList(folderOld,"UserControlSetupWizFeatures","imageList1",2,16) : IconLibrary.GetImageList(UserControlSetupWizFeatures.ArrayIconsImageList,16);
					using(imageListInfo) {
						Bitmap bitmapLabels=new Bitmap(UiTheme.ScaleI(64,scale),UiTheme.ScaleI(28,scale));
						using(Graphics g=Graphics.FromImage(bitmapLabels)) {
							g.Clear(Color.White);
						}
						for(int i=0;i<2;i++) {
							using(Label label=new Label()) {
								label.Size=new Size(UiTheme.ScaleI(20,scale),UiTheme.ScaleI(20,scale));
								label.BackColor=Color.White;
								label.ImageList=imageListInfo;
								label.ImageIndex=i;
								label.CreateControl();
								using(Bitmap bitmapLabel=new Bitmap(label.Width,label.Height)) {
									label.DrawToBitmap(bitmapLabel,new Rectangle(0,0,label.Width,label.Height));
									using(Graphics g=Graphics.FromImage(bitmapLabels)) {
										g.DrawImage(bitmapLabel,UiTheme.ScaleI(4,scale)+i*UiTheme.ScaleI(28,scale),UiTheme.ScaleI(4,scale));
									}
								}
							}
						}
						actionSave("lists\\wizard-info-labels-"+scaleText+".png",bitmapLabels);
					}
				}
			});
			Console.WriteLine(string.Join("\r\n",listWritten));
			Assert.AreEqual(ArrayScales.Length*8,listWritten.Count);
		}
	}
}
