// The control state pictures of package UI-4b-1 (docs/specs/ui-refresh-plan.md, section 7's UI-4b-1 line and section 3.6; docs/port-notes/UI-4b.md):
// each combo box, picker and list of the package, WinForms and WPF, drawn off screen in each state the design sheet gives it (rest, hover, focus,
// disabled, selected with and without focus), against the design sheet's drawing of that state at the control's own bounds (ListStateDrawings),
// by the tolerance of ChartImageCompare, as UI-4a's ControlStateTests do: WinForms every pixel, WPF the pixels the drawing paints in a solid token
// colour away from round edges. Then with text: its pixels reach the state's text token, and no item text starts in the selection bar's columns
// (section 3.2). No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToothChartImageTests;

namespace UiRefreshTests {
	[TestClass]
	public class ListControlStateTests {
		#region WinForms
		[TestMethod]
		public void WinForms_ComboBox_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(SheetState sheetState in ListStates.ArrayComboStates) {
					string name="WinForms ComboBox "+sheetState;
					using(Bitmap bitmapChrome=ListStateControls.RenderWinFormsComboBox(sheetState,"",1f))
					using(Bitmap bitmapText=ListStateControls.RenderWinFormsComboBox(sheetState,"Hygienist",1f))
					using(Bitmap bitmapExpected=ListStateDrawings.ComboBox(bitmapChrome.Size,new Rectangle(Point.Empty,bitmapChrome.Size),sheetState,1f)) {
						ControlStateTests.CompareAll(listFailures,name,bitmapExpected,bitmapChrome);
						ControlStateTests.CheckText(listFailures,name,bitmapChrome,bitmapText,ListStateDrawings.ComboTextColor(sheetState));
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_ClinicPicker_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(SheetState sheetState in ListStates.ArrayComboStates) {
					string name="WinForms ComboBoxClinicPicker "+sheetState;
					Rectangle rectangleBox;
					using(Bitmap bitmapChrome=ListStateControls.RenderWinFormsClinicPicker(sheetState,"",1f,out rectangleBox))
					using(Bitmap bitmapText=ListStateControls.RenderWinFormsClinicPicker(sheetState,"Clinic",1f,out rectangleBox))
					using(Bitmap bitmapExpected=ListStateDrawings.ComboBox(bitmapChrome.Size,rectangleBox,sheetState,1f)) {
						ControlStateTests.CompareAll(listFailures,name,bitmapExpected,bitmapChrome);
						//The label shows the same whether the picker is enabled or not, in Text.
						ControlStateTests.CheckText(listFailures,name+" label",bitmapChrome,bitmapText,SheetPainter.Light(EnumUiColor.Text));
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_ListBox_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(SheetState sheetState in ListStates.ArrayListStates) {
					string name="WinForms ListBox "+sheetState;
					using(ListPicture listPictureChrome=ListStateControls.RenderWinFormsListBox(sheetState,false,1f))
					using(ListPicture listPictureText=ListStateControls.RenderWinFormsListBox(sheetState,true,1f))
					using(Bitmap bitmapExpected=ListStateDrawings.List(listPictureChrome.Bitmap.Size,listPictureChrome.ListItems,
						new Rectangle(Point.Empty,listPictureChrome.Bitmap.Size),sheetState,1f,SheetPainter.Light(EnumUiColor.SurfaceRaised)))
					{
						ControlStateTests.CompareAll(listFailures,name,bitmapExpected,listPictureChrome.Bitmap);
						ControlStateTests.CheckText(listFailures,name,listPictureChrome.Bitmap,listPictureText.Bitmap,ListStateDrawings.ItemTextColor(sheetState));
						CheckTextClearOfBar(listFailures,name,listPictureChrome,listPictureText,1f);
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void WinForms_DropDown_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			IconRendererTests.RunSta(() => {
				foreach(SheetState sheetState in ListStates.ArrayDropDownStates) {
					string name="WinForms FormComboPicker "+sheetState;
					using(ListPicture listPictureChrome=ListStateControls.RenderWinFormsDropDown(sheetState,false,1f))
					using(ListPicture listPictureText=ListStateControls.RenderWinFormsDropDown(sheetState,true,1f))
					using(Bitmap bitmapExpected=DropDown(listPictureChrome,sheetState,1f)) {
						ControlStateTests.CompareAll(listFailures,name,bitmapExpected,listPictureChrome.Bitmap);
						ControlStateTests.CheckText(listFailures,name,listPictureChrome.Bitmap,listPictureText.Bitmap,ListStateDrawings.ItemTextColor(sheetState));
						CheckTextClearOfBar(listFailures,name,listPictureChrome,listPictureText,1f);
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		///<summary>The drop-down: its "combobox" on top in the hover look (the pointer is on it), the list below with its border and rows.</summary>
		private static Bitmap DropDown(ListPicture listPicture,SheetState sheetState,float scale) {
			Size size=listPicture.Bitmap.Size;
			int heightCombo=UiTheme.ScaleI(21,scale);
			Rectangle rectangleList=new Rectangle(0,heightCombo,size.Width,size.Height-heightCombo);
			Bitmap bitmap=ListStateDrawings.List(size,listPicture.ListItems,rectangleList,sheetState,scale,SheetPainter.Light(EnumUiColor.SurfaceRaised));
			using(Graphics g=Graphics.FromImage(bitmap))
			using(SheetPainter p=new SheetPainter(g,scale,SheetPainter.Light)) {
				ListStateDrawings.DrawComboBox(p,new Rectangle(0,0,size.Width,heightCombo),SheetState.Hover);
			}
			return bitmap;
		}
		#endregion WinForms

		#region WPF
		[TestMethod]
		public void Wpf_ComboBox_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(SheetState sheetState in ListStates.ArrayComboStates) {
					string name="WPF ComboBox "+sheetState;
					using(Bitmap bitmapChrome=ListStateControls.RenderWpfComboBox(sheetState,"",1f))
					using(Bitmap bitmapText=ListStateControls.RenderWpfComboBox(sheetState,"Hygienist",1f))
					using(Bitmap bitmapExpected=ListStateDrawings.ComboBox(bitmapChrome.Size,new Rectangle(Point.Empty,bitmapChrome.Size),sheetState,1f,colorBack:Color.White)) {
						ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,bitmapChrome);
						ControlStateTests.CheckText(listFailures,name,bitmapChrome,bitmapText,ListStateDrawings.ComboTextColor(sheetState));
					}
					//Text longer than the box is cut left of the chevron (where the WinForms ComboBox clips it), never drawn under it.
					using(Bitmap bitmapChrome=ListStateControls.RenderWpfComboBox(sheetState,"",1f))
					using(Bitmap bitmapLong=ListStateControls.RenderWpfComboBox(sheetState,TEXT_LONG,1f)) {
						CheckTextClearOfChevron(listFailures,name+" long text",bitmapChrome,bitmapLong,new Rectangle(Point.Empty,bitmapChrome.Size),1f);
					}
				}
				foreach(SheetState sheetState in new[] { SheetState.Normal,SheetState.Hover,SheetState.Disabled }) {
					string name="WPF ComboClinic "+sheetState;
					Rectangle rectangleBox;
					using(Bitmap bitmapChrome=ListStateControls.RenderWpfComboClinic(sheetState,"",1f,out rectangleBox))
					using(Bitmap bitmapText=ListStateControls.RenderWpfComboClinic(sheetState,"Clinic",1f,out rectangleBox))
					using(Bitmap bitmapExpected=ListStateDrawings.ComboBox(bitmapChrome.Size,rectangleBox,sheetState,1f,colorBack:Color.White)) {
						ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,bitmapChrome);
						if(sheetState!=SheetState.Disabled) {
							ControlStateTests.CheckText(listFailures,name+" label",bitmapChrome,bitmapText,SheetPainter.Light(EnumUiColor.Text));
						}
					}
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_ListBoxAndDropDown_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(SheetState sheetState in ListStates.ArrayWpfListStates) {
					CompareWpfList(listFailures,"WPF ListBox "+sheetState,sheetState,withText => ListStateControls.RenderWpfListBox(sheetState,withText,1f),true);
					if(sheetState==SheetState.Disabled) {
						continue;//the drop-down is never disabled
					}
					CompareWpfList(listFailures,"WPF WindowComboPicker "+sheetState,sheetState,withText => ListStateControls.RenderWpfDropDown(sheetState,withText,1f),true);
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		[TestMethod]
		public void Wpf_ListViewAndTreeView_EveryState_AsTheDesignSheet() {
			List<string> listFailures=new List<string>();
			ControlStateTests.RunWpf(() => {
				foreach(SheetState sheetState in ListStates.ArrayWpfListStates) {
					CompareWpfList(listFailures,"WPF ListView "+sheetState,sheetState,withText => ListStateControls.RenderWpfListView(sheetState,withText,1f),true);
					CompareWpfList(listFailures,"WPF TreeView "+sheetState,sheetState,withText => ListStateControls.RenderWpfTreeView(sheetState,withText,1f),true);
				}
			});
			ControlStateTests.AssertNoFailures(listFailures);
		}

		private static void CompareWpfList(List<string> listFailures,string name,SheetState sheetState,Func<bool,ListPicture> funcRender,bool hasBorder) {
			using(ListPicture listPictureChrome=funcRender(false))
			using(ListPicture listPictureText=funcRender(true))
			using(Bitmap bitmapExpected=ListStateDrawings.List(listPictureChrome.Bitmap.Size,listPictureChrome.ListItems,
				hasBorder ? new Rectangle(Point.Empty,listPictureChrome.Bitmap.Size) : Rectangle.Empty,sheetState,1f,Color.White))
			{
				ControlStateTests.CompareSolid(listFailures,name,bitmapExpected,listPictureChrome.Bitmap);
				ControlStateTests.CheckText(listFailures,name,listPictureChrome.Bitmap,listPictureText.Bitmap,ListStateDrawings.ItemTextColor(sheetState));
				CheckTextClearOfBar(listFailures,name,listPictureChrome,listPictureText,1f);
			}
		}
		#endregion WPF

		#region Helpers
		///<summary>A combo box text far wider than the 121 px test box.</summary>
		private const string TEXT_LONG="Hygienist, Dental Assistant and Front Desk Coordinator";

		///<summary>No text pixel (where the picture with text differs from the one without) falls in the chevron's ink columns: the chevron's square
		///(UiListPaint.GetChevronRectangle) less 2 px at each side, the Fluent ChevronDown's ink, at any height of the box.</summary>
		public static void CheckTextClearOfChevron(List<string> listFailures,string name,Bitmap bitmapChrome,Bitmap bitmapText,Rectangle rectangleBox,float scale) {
			Rectangle rectangleChevron=UiListPaint.GetChevronRectangle(rectangleBox,scale);
			int inset=UiTheme.ScaleI(2,scale);
			for(int y=rectangleBox.Top;y<rectangleBox.Bottom;y++) {
				for(int x=rectangleChevron.Left+inset;x<rectangleChevron.Right-inset;x++) {
					if(bitmapChrome.GetPixel(x,y).ToArgb()!=bitmapText.GetPixel(x,y).ToArgb()) {
						listFailures.Add(name+": text reaches into the chevron's columns at "+x+","+y+".");
						return;
					}
				}
			}
		}

		///<summary>Section 3.2: no item text starts in the selection bar's columns (the row's first 2 px at 96 DPI, scaled): the text's pixels (where
		///the picture with text differs from the one without) stay right of the bar in every row.</summary>
		public static void CheckTextClearOfBar(List<string> listFailures,string name,ListPicture listPictureChrome,ListPicture listPictureText,float scale) {
			foreach(Rectangle rectangleRow in listPictureChrome.ListItems) {
				Rectangle rectangleBar=ListStateDrawings.SelectionBar(rectangleRow,scale);
				rectangleBar.Intersect(new Rectangle(Point.Empty,listPictureChrome.Bitmap.Size));
				for(int y=rectangleBar.Top;y<rectangleBar.Bottom;y++) {
					for(int x=rectangleBar.Left;x<rectangleBar.Right;x++) {
						if(listPictureChrome.Bitmap.GetPixel(x,y).ToArgb()!=listPictureText.Bitmap.GetPixel(x,y).ToArgb()) {
							listFailures.Add(name+": item text reaches into the selection bar's columns at "+x+","+y+".");
							return;
						}
					}
				}
			}
		}
		#endregion Helpers
	}
}
