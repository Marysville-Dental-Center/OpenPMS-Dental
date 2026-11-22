// UI-9b-4's tests (docs/specs/ui-refresh-plan.md, section 7's UI-9b row and section 9; docs/port-notes/UI-9b.md, section 5): the remaining forms' toolbars
// and lists become lists of icons from the one icon factory, and GridOD draws a column's list that is at the grid's scale pixel for pixel. Each list keeps
// today's ImageIndex order (the arrays of the windows); no designer loads the converted streams; each toolbar form gives its toolbars the list of icons
// and its buttons an EnumIcons; a grid column that takes a factory gets a list at the grid's scale (16, 24 and 32 px at 100, 150 and 200%), asks for it
// again when the scale changes, draws it pixel for pixel, and keeps the row height of the old 96 DPI list; a column with a 96 DPI list draws as before
// (scaled by the grid). The gallery G-UI3 slice is opt-in: OPMS_UI9B_PICTURES names the folder for the pictures (each converted toolbar list as a strip of
// buttons, the grid columns of UI-9b-3 and UI-9b-4, the dashboard's tab pictures), the before set from the extracted pictures of the old streams
// (OPMS_UI9B4_OLD_LISTS), all off screen, synthetic data. No window is shown, no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using UiRefreshTests.Scan;

namespace UiRefreshTests.Toolbars {
	[TestClass]
	public class RemainingFormsImageListTests {
		///<summary>The folder with the pictures of the old streams (&lt;window&gt;-&lt;list&gt;-NN.png), for the gallery's before set.</summary>
		public const string VARIABLE_OLD_LISTS="OPMS_UI9B4_OLD_LISTS";
		public static readonly float[] ArrayScales=new float[] { 1f,1.5f,2f };

		///<summary>One converted list: the file (the window's .cs, relative to the repository), the array's name, the list's name in the old designer, the
		///size of its pictures, the toolbars that take it (none: the list had no toolbar), and the purposes in today's ImageIndex order.</summary>
		public class ListSpec {
			public string File;
			public string ArrayName;
			public string ListName;
			public int Size;
			public string[] Toolbars;
			public string[] Order;
		}

		private static ListSpec Spec(string file,string arrayName,string listName,int size,string[] toolbars,string order) {
			return new ListSpec { File=file,ArrayName=arrayName,ListName=listName,Size=size,Toolbars=toolbars,Order=order.Split(',') };
		}

		private const string F="OpenPmsDental/Forms/";
		private const string EDIT="Sync,Save,DeleteX,Cut,Copy,Paste,Undo,Link,LinkExternal,Heading1,Heading2,Heading3,Bold,Italic,TextColor,Table,Image,TextFont,SaveDraft,Settings";
		private const string EDIT_LIST="Sync,Save,DeleteX,Cut,Copy,Paste,Undo,Link,LinkExternal,Heading1,Heading2,Heading3,Bold,Italic,TextColor,Table,Image,TextFont,SaveDraft";
		private const string RDL="Print,ArrowLeft,ArrowRight,Export,Pdf,ZoomIn,ZoomOut";

		///<summary>Each list's pictures by today's index (the designers' streams before UI-9b-4) as the icon of each picture's purpose.</summary>
		public static readonly ListSpec[] ArrayLists=new[] {
			Spec(F+"FormAccounting.cs","ArrayIconsImageList","imageListMain",22,new[] { "ToolBarMain" },"Add,Edit,Export"),
			Spec(F+"FormAccountPick.cs","ArrayIconsImageListMain","imageListMain",22,new string[0],"Add,Edit"),
			Spec(F+"FormCareCreditWeb.cs","ArrayIconsImageList","imageList1",22,new[] { "ToolBarMain" },"ArrowLeft,ArrowRight"),
			Spec(F+"FormWebView.cs","ArrayIconsImageList","imageList1",22,new[] { "ToolBarMain" },"ArrowLeft,ArrowRight"),
			Spec(F+"FormWebBrowserPrefs.cs","ArrayIconsImageList1","imageList1",22,new string[0],"ArrowLeft,ArrowRight"),
			Spec(F+"FormClaimsSend.cs","ArrayIconsImageList","imageList1",22,new[] { "ToolBarMain","ToolBarHistory" },"Preview,NewDocument,Print,Complete,Send,DocumentText,Label"),
			Spec(F+"FormJournal.cs","ArrayIconsImageList","imageListMain",22,new[] { "ToolBarMain" },"Add,Print,Export"),
			Spec(F+"FormTPsign.cs","ArrayIconsImageList","imageListMain",22,new[] { "ToolBarMain" },"Print,ArrowLeft,ArrowRight"),
			Spec(F+"FormEmailEdit.cs","ArrayIconsImageList","imageListMain",22,new[] { "toolBarTop","toolBarBottom" },EDIT),
			Spec(F+"FormWikiEdit.cs","ArrayIconsImageList","imageListMain",22,new[] { "ToolBarMain","toolBar2" },EDIT_LIST+",LockOpen,LockClosed"),
			Spec(F+"FormWiki.cs","ArrayIconsImageList","imageListMain",22,new[] { "ToolBarMain" },"ArrowLeft,ArrowRight,Settings,Home,Edit,Print,Rename,Archive,History,Import,Add,Copy,Search,ListBullets,NewDocument"),
			Spec(F+"FormRecordAudio.cs","ArrayIconsImageList","imageListMain",22,new string[0],"Record,Stop"),
			Spec(F+"FormEtrans835s.cs","ArrayIconsImageListCalendar","imageListCalendar",10,new string[0],"ChevronDown,ChevronUp"),
			Spec("OpenPmsDental/Forms Reports/FormRpProcOverpaid.cs","ArrayIconsImageListCalendar","imageListCalendar",10,new string[0],"ChevronDown,ChevronUp"),
			Spec("OpenPmsDental/Forms Reports/FormRpDPPOvercharged.cs","ArrayIconsImageListCalendar","imageListCalendar",10,new string[0],"ChevronDown,ChevronUp"),
			Spec("OpenPmsDental/Forms Reports/FormRpProcNotBilledIns.cs","ArrayIconsImageListCalendar","imageListCalendar",10,new string[0],"ChevronDown,ChevronUp"),
			Spec("OpenPmsDental/ReportingComplex/FormReportComplex.cs","ArrayIconsImageList","imageListMain",22,new[] { "ToolBarMain" },"Print,ArrowLeft,ArrowRight,Export,ZoomIn,ZoomOut"),
			Spec("OpenPmsDental/ReportingForRdl/FormReportCustom.cs","ArrayIconsImageList","imageListMain",22,new[] { "ToolBarMain" },RDL),
			Spec("OpenPmsDental/ReportingForRdl/FormReportForRdl.cs","ArrayIconsImageList","imageListMain",22,new[] { "ToolBarMain" },RDL),
			Spec("OpenPmsDental/UI/FormPrintPreview.cs","ArrayIconsImageList","imageListMain",22,new[] { "ToolBarMain" },"Print,ArrowLeft,ArrowRight"),
			Spec(F+"FormCDSIntervention.cs","ArrayIconsImageListInfoButton","imageListInfoButton",16,new string[0],"Info"),
			Spec(F+"FormEhrLabOrderEdit2014.cs","ArrayIconsImageListInfoButton","imageListInfoButton",16,new string[0],"Info"),
			Spec(F+"FormMedical.cs","ArrayIconsImageListInfoButton","imageListInfoButton",16,new string[0],"Info"),
			Spec(F+"FormSnomeds.cs","ArrayIconsImageListInfoButton","imageListInfoButton",16,new string[0],"Info"),
			Spec("CentralManager/FormCentralSecurity.cs","ArrayIconsImageListPerm","imageListPerm",15,new string[0],"ImageSelectorFolder,CheckBoxEmpty,CheckBoxChecked,CheckBoxPartial"),
			Spec("UnitTests/UI Testing/Form4kTests.cs","ArrayIconsImageList","imageListMain",22,new[] { "ToolBarMain" },"Add,Edit,Export"),
			Spec("UnitTests/UI Testing/FormTestAllControls.cs","ArrayIconsImageList","imageList",22,new string[0],"Edit"),
		};

		#region Helpers
		private static string ReadText(string file) {
			return File.ReadAllText(Path.Combine(RepoPaths.Root(),file.Replace('/',Path.DirectorySeparatorChar)));
		}

		private static string GetDesignerPath(string file) {
			string path=Path.Combine(RepoPaths.Root(),file.Replace('/',Path.DirectorySeparatorChar));
			string folder=Path.GetDirectoryName(path);
			string name=Path.GetFileNameWithoutExtension(path);
			return Directory.GetFiles(folder,name+".*.cs").First(x => Path.GetFileName(x).EndsWith(".designer.cs",StringComparison.OrdinalIgnoreCase));
		}

		private static string[] GetArrayFromSource(ListSpec spec) {
			Match match=Regex.Match(ReadText(spec.File),@"public\s+static\s+readonly\s+(?:OpenPmsDental\.UI\.)?EnumIcons\[\]\s+"+spec.ArrayName+@"\s*=\s*new\s+(?:OpenPmsDental\.UI\.)?EnumIcons\[\]\s*\{(?<a>[^}]*)\}");
			Assert.IsTrue(match.Success,spec.File+": no "+spec.ArrayName);
			return match.Groups["a"].Value.Split(',').Select(x => x.Trim().Replace("OpenPmsDental.UI.","").Replace("EnumIcons.","")).Where(x => x!="").ToArray();
		}

		private static string GetScaleText(float scale) {
			return ((int)Math.Round(scale*100)).ToString();
		}

		private static string GetPath(string folder,string name) {
			string path=Path.Combine(folder,name);
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			return path;
		}

		private static bool IsInk(Color color) {
			return color.R+color.G+color.B<3*250;
		}

		private static int CountInk(Bitmap bitmap,Rectangle rectangle) {
			int count=0;
			for(int y=Math.Max(rectangle.Top,0);y<Math.Min(rectangle.Bottom,bitmap.Height);y++) {
				for(int x=Math.Max(rectangle.Left,0);x<Math.Min(rectangle.Right,bitmap.Width);x++) {
					if(IsInk(bitmap.GetPixel(x,y))) {
						count++;
					}
				}
			}
			return count;
		}

		///<summary>A grid of the scale with a first column that shows a list's pictures by the index in the cell, one row for each index, a description next.
		///actionColumn sets the column's list (a 96 DPI list, or a factory).</summary>
		private static OpenPmsDental.UI.GridOD NewGrid(float scale,int countRows,Action<OpenPmsDental.UI.GridColumn> actionColumn) {
			OpenPmsDental.UI.GridOD grid=new OpenPmsDental.UI.GridOD();
			grid.TranslationName="";
			grid.Size=new Size(UiTheme.ScaleI(200,scale),UiTheme.ScaleI(28+17*countRows+6,scale));
			grid.SetScaleAndZoom(1,scale);
			grid.BeginUpdate();
			grid.Title="Synthetic";
			grid.Columns.Clear();
			OpenPmsDental.UI.GridColumn gridColumn=new OpenPmsDental.UI.GridColumn("",17);
			actionColumn(gridColumn);
			grid.Columns.Add(gridColumn);
			grid.Columns.Add(new OpenPmsDental.UI.GridColumn("Description",150));
			grid.ListGridRows.Clear();
			for(int i=0;i<countRows;i++) {
				grid.ListGridRows.Add(new OpenPmsDental.UI.GridRow(i.ToString(),"Synthetic "+(i+1)));
			}
			grid.EndUpdate();
			return grid;
		}

		private static Bitmap Draw(Control control) {
			Bitmap bitmap=new Bitmap(control.Width,control.Height,PixelFormat.Format32bppArgb);
			control.DrawToBitmap(bitmap,new Rectangle(0,0,control.Width,control.Height));
			return bitmap;
		}

		///<summary>True when every fully opaque pixel of the picture is in the bitmap at the offset (pixel for pixel, exactly).</summary>
		private static bool IsPictureAt(Bitmap bitmap,Bitmap picture,int offsetX,int offsetY) {
			int countOpaque=0;
			for(int y=0;y<picture.Height;y++) {
				for(int x=0;x<picture.Width;x++) {
					Color color=picture.GetPixel(x,y);
					if(color.A!=255) {
						continue;
					}
					countOpaque++;
					if(offsetX+x>=bitmap.Width || offsetY+y>=bitmap.Height || bitmap.GetPixel(offsetX+x,offsetY+y).ToArgb()!=color.ToArgb()) {
						return false;
					}
				}
			}
			return countOpaque>0;
		}
		#endregion Helpers

		[TestMethod]
		public void Lists_KeepTodaysIndexOrder_AsIcons() {
			foreach(ListSpec spec in ArrayLists) {
				string[] arrayMembers=GetArrayFromSource(spec);
				CollectionAssert.AreEqual(spec.Order,arrayMembers,spec.File+" "+spec.ArrayName+": the purposes in today's ImageIndex order.");
				foreach(string member in arrayMembers) {
					Assert.IsTrue(Enum.IsDefined(typeof(EnumIcons),member),spec.File+": EnumIcons has no "+member);
					Assert.IsTrue(IconLibrary.GetDesigns((EnumIcons)Enum.Parse(typeof(EnumIcons),member)).Length>0,spec.File+": "+member+" has no design.");
				}
			}
			//The compiled arrays are the source's (the program's own windows; CentralManager and UnitTests are checked by their source).
			Assembly assembly=typeof(FormAccounting).Assembly;
			foreach(ListSpec spec in ArrayLists.Where(x => x.File.StartsWith("OpenPmsDental/"))) {
				string name=Path.GetFileNameWithoutExtension(spec.File);
				Type type=assembly.GetTypes().First(x => x.Name==name);
				EnumIcons[] arrayIcons=(EnumIcons[])type.GetField(spec.ArrayName,BindingFlags.Public|BindingFlags.Static).GetValue(null);
				CollectionAssert.AreEqual(spec.Order,arrayIcons.Select(x => x.ToString()).ToArray(),name);
			}
		}

		[TestMethod]
		public void Designers_HaveNoStreamLeft_ToolbarsTakeTheListAndButtonsTheirIcons() {
			foreach(ListSpec spec in ArrayLists) {
				string textDesigner=File.ReadAllText(GetDesignerPath(spec.File));
				Assert.IsFalse(textDesigner.Contains("\""+spec.ListName+".ImageStream\""),spec.File+": the designer still loads the stream of "+spec.ListName);
				Assert.IsFalse(Regex.IsMatch(textDesigner,@"ImageList\s+"+spec.ListName+@"\s*;"),spec.File+": the designer still has the field "+spec.ListName);
				Assert.IsFalse(Regex.IsMatch(textDesigner,@"\.ImageList\s*=\s*this\."+spec.ListName+@"\s*;"),spec.File+": a control still takes "+spec.ListName);
				string pathResx=Path.ChangeExtension(Path.Combine(RepoPaths.Root(),spec.File.Replace('/',Path.DirectorySeparatorChar)),".resx");
				Assert.IsFalse(File.ReadAllText(pathResx).Contains("name=\""+spec.ListName+".ImageStream\""),spec.File+": the .resx still has the stream.");
				string text=ReadText(spec.File);
				foreach(string toolbar in spec.Toolbars) {
					Assert.IsTrue(text.Contains(toolbar+".SetImageListIcons(ArrayIconsImageList);"),spec.File+": "+toolbar+" does not take the list of icons.");
				}
				if(spec.Toolbars.Length>0) {
					string code=string.Join("\n",text.Split('\n').Where(x => !x.TrimStart().StartsWith("//")));
					Assert.IsFalse(Regex.IsMatch(code,@"new\s+ODToolBarButton\((?:[^,()\r\n]|\([^()\r\n]*\))*,\s*\d+\s*,"),spec.File+": a toolbar button still takes a picture by number.");
					Assert.IsFalse(Regex.IsMatch(code,@"\.ImageIndex\s*=\s*\d+"),spec.File+": a button still sets a picture by number.");
				}
			}
		}

		[TestMethod]
		public void StateAndDirectionPictures_StayDistinguishable_ByShape() {
			IconRendererTests.RunSta(() => {
				foreach(ListSpec spec in ArrayLists) {
					string[] arrayMembers=spec.Order.Where(x => x!="DeleteX").ToArray();
					foreach(int size in new[] { 16,24,32 }) {
						for(int a=0;a<arrayMembers.Length;a++) {
							for(int b=a+1;b<arrayMembers.Length;b++) {
								if(arrayMembers[a]==arrayMembers[b]) {
									continue;
								}
								bool[] maskA=GetMask((EnumIcons)Enum.Parse(typeof(EnumIcons),arrayMembers[a]),size);
								bool[] maskB=GetMask((EnumIcons)Enum.Parse(typeof(EnumIcons),arrayMembers[b]),size);
								Assert.IsFalse(maskA.SequenceEqual(maskB),spec.File+": "+arrayMembers[a]+" and "+arrayMembers[b]+" look alike at "+size+" px.");
							}
						}
					}
				}
			});
		}

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

		[TestMethod]
		public void ToolbarLists_BuildWithTheirIcons_AndAnImageIndexButtonStillDraws_At100_150_200() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				foreach(float scale in ArrayScales) {
					foreach(ListSpec spec in ArrayLists.Where(x => x.Toolbars.Length>0)) {
						EnumIcons[] arrayIcons=spec.Order.Select(x => (EnumIcons)Enum.Parse(typeof(EnumIcons),x)).ToArray();
						using(ToolBarOD toolBarOD=new ToolBarOD()) {
							toolBarOD.LayoutManager.SetScaleMS(scale);
							toolBarOD.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f*scale);
							toolBarOD.Size=new Size(UiTheme.ScaleI(40+arrayIcons.Length*60,scale),UiTheme.ScaleI(ToolBarModel.HEIGHT_TOOLBAR,scale));
							toolBarOD.SetImageListIcons(arrayIcons);
							//The first button by number (as a plug-in does), the second by its icon: both draw a picture.
							toolBarOD.Buttons.Add(new ODToolBarButton("A",0,"","A"));
							toolBarOD.Buttons.Add(new ODToolBarButton("B",arrayIcons[arrayIcons.Length-1],"","B"));
							using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
								Assert.IsTrue(toolBarOD.IsImageListIcons,spec.File);
								foreach(ODToolBarButton button in toolBarOD.Buttons.Cast<ODToolBarButton>()) {
									int size=toolBarOD.LayoutManager.Scale(ToolBarOD.SIZE_IMAGE_LIST96);
									Rectangle rectangle=new Rectangle(button.Bounds.X+toolBarOD.LayoutManager.Scale(3),button.Bounds.Y+toolBarOD.LayoutManager.Scale(1),size,size);
									Assert.IsTrue(CountInk(bitmap,rectangle)>=UiTheme.ScaleI(8,scale),spec.File+": "+button.Text+" draws its picture at "+GetScaleText(scale)+"%.");
								}
							}
						}
					}
				}
			});
		}

		[TestMethod]
		public void GridColumnFactory_GivesAListAtTheScale_DrawsItPixelForPixel_At100_150_200() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				EnumIcons[] arrayIcons=new[] { EnumIcons.TaskList,EnumIcons.TaskNew,EnumIcons.CheckBoxChecked };
				foreach(float scale in ArrayScales) {
					ImageList imageList=null;
					List<int> listSizes=new List<int>();
					using(OpenPmsDental.UI.GridOD grid=NewGrid(scale,arrayIcons.Length,column => {
						column.ImageListFactory=size => {
							listSizes.Add(size);
							imageList=IconLibrary.GetImageListForSize(imageList,arrayIcons,size);
							return imageList;
						};
					})) {
						int sizeExpected=UiTheme.ScaleI(16,scale);
						Assert.IsTrue(listSizes.Count>=1 && listSizes.All(x => x==sizeExpected),"The factory is asked for the 16 px slot at "+GetScaleText(scale)+"%: "+string.Join(",",listSizes));
						Assert.AreEqual(new Size(sizeExpected,sizeExpected),imageList.ImageSize);
						Assert.IsTrue(grid.Columns[0].IsImageListAtScale);
						Assert.AreSame(imageList,grid.Columns[0].ImageList);
						using(Bitmap bitmap=Draw(grid)) {
							//TaskNew (the solid square, row 1) has opaque pixels: each is in the grid exactly as the list holds it, at a whole pixel offset.
							using(Bitmap bitmapPicture=new Bitmap(imageList.Images[1])) {
								bool isFound=false;
								for(int offsetY=0;offsetY<bitmap.Height-bitmapPicture.Height && !isFound;offsetY++) {
									for(int offsetX=0;offsetX<=UiTheme.ScaleI(8,scale) && !isFound;offsetX++) {
										isFound=IsPictureAt(bitmap,bitmapPicture,offsetX,offsetY);
									}
								}
								Assert.IsTrue(isFound,"The grid draws the list's picture pixel for pixel at "+GetScaleText(scale)+"%.");
							}
						}
					}
					imageList?.Dispose();
				}
			});
		}

		[TestMethod]
		public void GridColumnFactory_IsAskedAgainWhenTheScaleChanges_AndKeepsTheRowHeight() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				EnumIcons[] arrayIcons=new[] { EnumIcons.TaskList,EnumIcons.TaskNew };
				ImageList imageList=null;
				List<int> listSizes=new List<int>();
				using(OpenPmsDental.UI.GridOD grid=NewGrid(1f,arrayIcons.Length,column => column.ImageListFactory=size => {
					listSizes.Add(size);
					imageList=IconLibrary.GetImageListForSize(imageList,arrayIcons,size);
					return imageList;
				})) {
					int[] arrayHeightsList=new int[3];
					float[] arrayScales=new[] { 1f,1.5f,2f };
					for(int i=0;i<arrayScales.Length;i++) {
						grid.SetScaleAndZoom(1,arrayScales[i]);
						arrayHeightsList[i]=grid.ListGridRows[0].State.HeightMain;
						Assert.AreEqual(UiTheme.ScaleI(16,arrayScales[i]),grid.Columns[0].ImageList.ImageSize.Width,"the list follows the scale "+arrayScales[i]);
					}
					CollectionAssert.AreEqual(new[] { 16,24,32 },listSizes.Distinct().ToArray(),"asked at each new scale, not at every layout");
					//The old way (a 96 DPI list scaled by the grid) gives the same row height at each scale. The grid is made and scaled as the one
					//above (UI-2b: in Segoe UI 9 a grid filled at a scale and one scaled after filling can differ by a pixel until their rows are measured).
					for(int i=0;i<arrayScales.Length;i++) {
						using(ImageList imageListOld=IconLibrary.GetImageList(arrayIcons,16))
						using(OpenPmsDental.UI.GridOD gridOld=NewGrid(1f,arrayIcons.Length,column => column.ImageList=imageListOld)) {
							gridOld.SetScaleAndZoom(1,arrayScales[i]);
							Assert.IsFalse(gridOld.Columns[0].IsImageListAtScale,"a 96 DPI list stays a 96 DPI list");
							Assert.AreSame(imageListOld,gridOld.Columns[0].ImageList);
							Assert.AreEqual(gridOld.ListGridRows[0].State.HeightMain,arrayHeightsList[i],"row height at "+arrayScales[i]);
						}
					}
				}
				imageList?.Dispose();
			});
		}

		[TestMethod]
		public void GridColumn96DpiList_StillDrawsScaledByTheGrid() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				EnumIcons[] arrayIcons=new[] { EnumIcons.TaskNew };
				foreach(float scale in ArrayScales) {
					using(ImageList imageList=IconLibrary.GetImageList(arrayIcons,16))
					using(OpenPmsDental.UI.GridOD grid=NewGrid(scale,1,column => column.ImageList=imageList))
					using(Bitmap bitmap=Draw(grid)) {
						//The solid square spans the slot scaled by the grid: its ink box is about ScaleI(16)*(12/16)... at least a third of the slot's width, at most the slot.
						int slot=UiTheme.ScaleI(16,scale);
						int originY=UiTheme.ScaleI(28,scale);
						Rectangle rectangle=new Rectangle(1,originY,slot+2,slot+4);
						int count=CountInk(bitmap,rectangle);
						Assert.IsTrue(count>=(slot/2)*(slot/2),"the 96 DPI picture is scaled to the slot at "+GetScaleText(scale)+"%: "+count);
					}
				}
			});
		}

		[TestMethod]
		public void UiNine_b3_GridListsTakeTheFactory_AndTheFormsOfTheInfoButtonToo() {
			Assert.IsTrue(ReadText("OpenPmsDental/User Controls/UserControlTasks.cs").Contains("gridColumn.ImageListFactory=GetImageListGrid;"));
			Assert.IsTrue(ReadText("OpenPmsDental/Main Modules/ControlAppt.cs").Contains("col.ImageListFactory=GetImageListTasks;"));
			Assert.IsTrue(ReadText("OpenPmsDental/Forms/FormSetupWizard.cs").Contains("ImageListFactory=GetImageListInfo"));
			foreach(string form in new[] { "FormCDSIntervention","FormEhrLabOrderEdit2014","FormMedical","FormSnomeds" }) {
				string text=ReadText("OpenPmsDental/Forms/"+form+".cs");
				Assert.IsTrue(text.Contains("ImageListFactory=GetImageListInfoButton;"),form);
				Assert.IsFalse(Regex.IsMatch(text,@"\.ImageList=imageListInfoButton"),form);
			}
		}

		[TestMethod]
		public void RecordAudioButton_ShowsTheRecordAndStopIcons() {
			string text=ReadText("OpenPmsDental/Forms/FormRecordAudio.cs");
			Assert.IsTrue(text.Contains("butStart.Image=IconLibrary.GetBitmap(EnumIcons.Stop,22,22);"));
			Assert.IsTrue(text.Contains("butStart.Image=IconLibrary.GetBitmap(EnumIcons.Record,22,22);"));
			Assert.IsFalse(text.Contains("imageListMain"));
		}

		[TestMethod]
		public void DashboardTabPictures_AreIcons_AtTheDpiSize() {
			string text=ReadText("OpenPmsDentalGraph/Dashboard/DashboardTabCtrl.cs");
			Assert.IsTrue(text.Contains("UiIcons.GetBitmap(\"EnumIcons\",\"Add\""));
			Assert.IsTrue(text.Contains("UiIcons.GetBitmap(\"EnumIcons\",\"DeleteX\""));
			Assert.IsFalse(File.ReadAllText(GetDesignerPath("OpenPmsDentalGraph/Dashboard/DashboardTabCtrl.cs")).Contains("ImageListStreamer"));
			//The list the factory builds: 11 px at 96 DPI, 30 px tall, in 32-bit colour, one picture for each state, the highlighted one differing from the plain one.
			IconRendererTests.RunSta(() => {
				foreach(int size in new[] { 11,17,22 }) {
					using(Bitmap bitmapAdd=UiIcons.GetBitmap("EnumIcons","Add",size,30))
					using(Bitmap bitmapDelete=UiIcons.GetBitmap("EnumIcons","DeleteX",size,30))
					using(ImageList imageList=UiIcons.GetImageList(size,30,new[] { "addTab","deleteTab" },new[] { (Bitmap)bitmapAdd.Clone(),(Bitmap)bitmapDelete.Clone() })) {
						Assert.AreEqual(new Size(size,30),imageList.ImageSize);
						Assert.AreEqual(ColorDepth.Depth32Bit,imageList.ColorDepth);
						Assert.AreEqual(0,imageList.Images.IndexOfKey("addTab"));
						Assert.AreEqual(1,imageList.Images.IndexOfKey("deleteTab"));
						Assert.IsTrue(CountInk(bitmapAdd,new Rectangle(0,0,size,30))>0);
						Assert.IsTrue(CountInk(bitmapDelete,new Rectangle(0,0,size,30))>0);
					}
				}
			});
		}

		///<summary>Sets the toolbar's width to its buttons (the UI-9b-4 fix, docs/port-notes/UI-9b.md, section 5.8): the buttons are laid out by a
		///draw (ToolBarOD measures them when it paints), then the width is the last button's right edge and 10 px at the scale. The first gallery gave
		///each strip 10 + 40 px per button at 96 DPI, which the two-digit labels outgrow: the wiki's last button was cut and the editors' last one
		///(20 and 21 buttons) left out of the picture.</summary>
		public static void FitToButtons(ToolBarOD toolBarOD,float scale) {
			using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
				//laid out
			}
			int right=0;
			for(int i=0;i<toolBarOD.Buttons.Count;i++) {
				right=Math.Max(right,toolBarOD.Buttons[i].Bounds.Right);
			}
			toolBarOD.Width=right+UiTheme.ScaleI(10,scale);
		}

		///<summary>Makes the gallery's grid tall enough for all its rows (the UI-9b-4 fix, docs/port-notes/UI-9b.md, section 5.8): the first gallery's
		///height (28 + 17 px a row at 96 DPI) was measured at the old font; at UI-2b's Segoe UI 9 pt the rows are taller, so the last row (the
		///wizard's and the info buttons' only row) was cut. Grows the grid a pixel at a time until it no longer scrolls.</summary>
		public static void FitGridToRows(OpenPmsDental.UI.GridOD grid) {
			for(int i=0;i<400;i++) {
				using(Bitmap bitmap=Draw(grid)) {
					//laid out
				}
				grid.ScrollToEnd();
				bool isScrolled=grid.ScrollValue>0;
				grid.ScrollToTop();
				if(!isScrolled) {
					return;
				}
				grid.Height+=1;
			}
			Assert.Fail("The grid still scrolls at "+grid.Height+" px.");
		}

		///<summary>The gallery's strips hold every button (the UI-9b-4 fix: the first gallery cut the wiki's and the editors' last buttons), and the real
		///wiki toolbar, with the buttons its LayoutToolBar adds (Lists included) and today's labels at the program font, fits the window's designer
		///width (944 px at 96 DPI, docked) at 100, 150 and 200%: the cut was the picture's, not the window's. The optional Read-Only Copy text button
		///is measured and written to the output only.</summary>
		[TestMethod]
		public void GalleryStrips_HoldEveryButton_AndTheWikiToolbarFitsItsWindow_At100_150_200() {
			IconRendererTests.RunSta(() => {
				NoDatabaseGuard.Install();
				UiTheme.InitWinForms();
				foreach(float scale in ArrayScales) {
					foreach(ListSpec spec in ArrayLists.Where(x => x.Toolbars.Length>0)) {
						EnumIcons[] arrayIcons=spec.Order.Select(x => (EnumIcons)Enum.Parse(typeof(EnumIcons),x)).ToArray();
						using(ToolBarOD toolBarOD=new ToolBarOD()) {
							toolBarOD.LayoutManager.SetScaleMS(scale);
							toolBarOD.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f*scale);
							toolBarOD.Size=new Size(UiTheme.ScaleI(10+arrayIcons.Length*40,scale),UiTheme.ScaleI(ToolBarModel.HEIGHT_TOOLBAR,scale));
							toolBarOD.SetImageListIcons(arrayIcons);
							for(int i=0;i<arrayIcons.Length;i++) {
								toolBarOD.Buttons.Add(new ODToolBarButton(i.ToString(),arrayIcons[i],"",i.ToString()));
							}
							FitToButtons(toolBarOD,scale);
							using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
								ODToolBarButton buttonLast=toolBarOD.Buttons[toolBarOD.Buttons.Count-1];
								Assert.IsTrue(buttonLast.Bounds.Right<=bitmap.Width,spec.File+" at "+GetScaleText(scale)+"%: the last button ends at "+buttonLast.Bounds.Right
									+", the picture at "+bitmap.Width+".");
							}
						}
					}
					//The wiki's toolbar as FormWiki.LayoutToolBar makes it (Lists and Read-Only Copy are optional: both added here).
					using(ToolBarOD toolBarOD=new ToolBarOD()) {
						toolBarOD.LayoutManager.SetScaleMS(scale);
						toolBarOD.Font=new Font(UiTheme.FONT_FAMILY,UiTheme.FONT_SIZE_BODY*scale);
						toolBarOD.Size=new Size(UiTheme.ScaleI(944,scale),UiTheme.ScaleI(25,scale));
						toolBarOD.SetImageListIcons(FormWiki.ArrayIconsImageList);
						toolBarOD.Buttons.Add(new ODToolBarButton("Back",EnumIcons.ArrowLeft,"","Back"));
						toolBarOD.Buttons.Add(new ODToolBarButton("Fwd",EnumIcons.ArrowRight,"","Forward"));
						toolBarOD.Buttons.Add(new ODToolBarButton("Setup",EnumIcons.Settings,"","Setup"));
						toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
						toolBarOD.Buttons.Add(new ODToolBarButton("Home",EnumIcons.Home,"","Home"));
						toolBarOD.Buttons.Add(new ODToolBarButton("Edit",EnumIcons.Edit,"","Edit"));
						toolBarOD.Buttons.Add(new ODToolBarButton("Print",EnumIcons.Print,"","Print"));
						toolBarOD.Buttons.Add(new ODToolBarButton("Rename",EnumIcons.Rename,"","Rename"));
						toolBarOD.Buttons.Add(new ODToolBarButton("Archive",EnumIcons.Archive,"","Archive"));
						toolBarOD.Buttons.Add(new ODToolBarButton("Hist",EnumIcons.History,"","History"));
						toolBarOD.Buttons.Add(new ODToolBarButton("Drafts",EnumIcons.NewDocument,"","Drafts"));
						toolBarOD.Buttons.Add(new ODToolBarButton("In-Links",EnumIcons.Import,"","Inc Links"));
						toolBarOD.Buttons.Add(new ODToolBarButton(ODToolBarButtonStyle.Separator));
						toolBarOD.Buttons.Add(new ODToolBarButton("Add",EnumIcons.Add,"","Add"));
						toolBarOD.Buttons.Add(new ODToolBarButton("Lists",EnumIcons.ListBullets,"","Lists"));
						toolBarOD.Buttons.Add(new ODToolBarButton("Search",EnumIcons.Search,"","Search"));
						using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
							ODToolBarButton buttonLast=toolBarOD.Buttons[toolBarOD.Buttons.Count-1];
							Assert.IsTrue(buttonLast.Bounds.Right<=toolBarOD.Width,"The wiki toolbar at "+GetScaleText(scale)+"% ends at "+buttonLast.Bounds.Right
								+", its window at "+toolBarOD.Width+".");
						}
						//Read-Only Copy (a text button, only when the office keeps a read-only copy) goes past the designer width at the program font:
						//reported for UI-12's fit work (docs/port-notes/UI-9b.md, section 5.8), not a picture of this package.
						toolBarOD.Buttons.Add(new ODToolBarButton("Read-Only Copy",-1,"","ReadOnlyCopy"));
						using(Bitmap bitmap=ToolBarPictureTests.Draw(toolBarOD)) {
							Console.WriteLine("Wiki toolbar at "+GetScaleText(scale)+"% with Read-Only Copy: ends at "+toolBarOD.Buttons[toolBarOD.Buttons.Count-1].Bounds.Right
								+" of "+toolBarOD.Width+" px.");
						}
					}
				}
			});
		}

		///<summary>The G-UI3 slice's pictures (opt-in): each converted toolbar list as a toolbar with one button for each picture, the grid columns that
		///show a list's pictures (UI-9b-3's task lists, reminders and setup wizard, and UI-9b-4's info buttons), at 100, 150 and 200%, off screen. The
		///before set draws the extracted pictures of the old streams (OPMS_UI9B4_OLD_LISTS) for the toolbars and the 96 DPI list for the grids.</summary>
		[TestMethod]
		public void Gallery_RemainingFormsAndGridColumns() {
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
				foreach(float scale in ArrayScales) {
					string scaleText=GetScaleText(scale);
					//Toolbars: one picture for each distinct list (the lists the windows share, as the email and wiki editors, are drawn once).
					foreach(ListSpec spec in ArrayLists.Where(x => x.Toolbars.Length>0 && !x.File.StartsWith("UnitTests") && x.File!="OpenPmsDental/ReportingForRdl/FormReportForRdl.cs"
						&& x.File!="OpenPmsDental/Forms/FormWebView.cs" && x.File!="OpenPmsDental/UI/FormPrintPreview.cs" && x.File!="OpenPmsDental/Forms/FormTPsign.cs"))
					{
						string window=Path.GetFileNameWithoutExtension(spec.File);
						EnumIcons[] arrayIcons=spec.Order.Select(x => (EnumIcons)Enum.Parse(typeof(EnumIcons),x)).ToArray();
						using(ToolBarOD toolBarOD=new ToolBarOD()) {
							toolBarOD.LayoutManager.SetScaleMS(scale);
							toolBarOD.Font=new Font(UiTheme.FONT_FAMILY_TODAY,8.25f*scale);
							toolBarOD.Size=new Size(UiTheme.ScaleI(10+arrayIcons.Length*40,scale),UiTheme.ScaleI(ToolBarModel.HEIGHT_TOOLBAR,scale));
							if(isBefore) {
								ImageList imageListOld=new ImageList();
								imageListOld.ColorDepth=ColorDepth.Depth32Bit;
								imageListOld.ImageSize=new Size(spec.Size,spec.Size);
								for(int i=0;i<arrayIcons.Length;i++) {
									using(Bitmap bitmap=new Bitmap(Path.Combine(folderOld,window+"-"+spec.ListName+"-"+i.ToString("00")+".png"))) {
										imageListOld.Images.Add(new Bitmap(bitmap));
									}
								}
								toolBarOD.ImageList=imageListOld;
								for(int i=0;i<arrayIcons.Length;i++) {
									toolBarOD.Buttons.Add(new ODToolBarButton(i.ToString(),i,"",i.ToString()));
								}
							}
							else {
								toolBarOD.SetImageListIcons(arrayIcons);
								for(int i=0;i<arrayIcons.Length;i++) {
									toolBarOD.Buttons.Add(new ODToolBarButton(i.ToString(),arrayIcons[i],"",i.ToString()));
								}
							}
							FitToButtons(toolBarOD,scale);
							actionSave("toolbars\\"+window+"-"+scaleText+".png",ToolBarPictureTests.Draw(toolBarOD));
						}
					}
					//Grid columns: the 96 DPI list scaled by the grid (soft at 150 and 200%) before, the list at the grid's scale after.
					foreach(KeyValuePair<string,EnumIcons[]> pair in new Dictionary<string,EnumIcons[]> {
						{ "tasks",UserControlTasks.ArrayIconsImageListTasks },{ "reminders",ControlAppt.ArrayIconsImageListTasks },{ "wizard",FormSetupWizard.ArrayIconsImageList },
						{ "infobutton",FormMedical.ArrayIconsImageListInfoButton } })
					{
						ImageList imageList=null;
						OpenPmsDental.UI.GridOD grid;
						if(isBefore) {
							imageList=IconLibrary.GetImageList(pair.Value,16);
							grid=NewGrid(scale,pair.Value.Length,column => column.ImageList=imageList);
						}
						else {
							grid=NewGrid(scale,pair.Value.Length,column => column.ImageListFactory=size => {
								imageList=IconLibrary.GetImageListForSize(imageList,pair.Value,size);
								return imageList;
							});
						}
						using(grid) {
							FitGridToRows(grid);
							actionSave("grids\\"+pair.Key+"-"+scaleText+".png",Draw(grid));
						}
						imageList?.Dispose();
					}
				}
			});
			Console.WriteLine(string.Join("\r\n",listWritten));
			Assert.IsTrue(listWritten.Count>0);
		}
	}
}
