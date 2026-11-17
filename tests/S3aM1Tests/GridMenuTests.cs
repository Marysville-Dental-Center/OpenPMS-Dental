// The right-click menus of GridOD and GridOld (package S3a-M1): the grid's own menu is a ContextMenuStrip now, and its items, their order,
// separators and enabled states are the ones the WinForms ContextMenu path (the bridge for S3a-M2/M3) put in a ContextMenu for the same grid,
// cell, row and selection. S3a-M1 compared the two; the bridges are gone (GridOld's with S3a-M2, GridOD's with S3a-M3b), and the expected lists
// are the ones the bridge gave. The grids are built in memory with a window handle for their layout and are never shown; the menus' Opening is
// raised directly, with the grid as its source control, instead of by a right click.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental.UI;

namespace S3aM1Tests {
	[TestClass]
	public class GridMenuTests {
		private const string LINK="Web - www.example.test";

		[TestInitialize]
		public void TestInitialize() {
			S3aM1Db.SetCultureUs();
		}

		#region Helpers
		///<summary>Three rows of two columns: row 0 has a link in its second cell, row 1 has an empty second cell, row 2 an empty first cell.</summary>
		private static T MakeGrid<T>(string name) where T:Control,new() {
			T grid=new T();
			grid.Name=name;
			grid.Size=new Size(300,200);
			List<GridRow> listGridRows=new List<GridRow> { new GridRow("Alpha","see www.example.test"),new GridRow("Beta",""),new GridRow("","Gamma") };
			if(grid is GridOD gridOD) {
				gridOD.BeginUpdate();
				gridOD.Columns.Add(new GridColumn("Name",100));
				gridOD.Columns.Add(new GridColumn("Note",150));
				gridOD.ListGridRows.AddRange(listGridRows);
				gridOD.EndUpdate();
			}
			else {
				GridOld gridOld=(GridOld)(Control)grid;
				gridOld.BeginUpdate();
				gridOld.Columns.Add(new GridColumn("Name",100));
				gridOld.Columns.Add(new GridColumn("Note",150));
				gridOld.ListGridRows.AddRange(listGridRows);
				gridOld.EndUpdate();
			}
			return grid;
		}

		private static int PointToRow(Control grid,int y) {
			return grid is GridOD gridOD ? gridOD.PointToRow(y) : ((GridOld)grid).PointToRow(y);
		}

		private static int PointToCol(Control grid,int x) {
			return grid is GridOD gridOD ? gridOD.PointToCol(x) : ((GridOld)grid).PointToCol(x);
		}

		///<summary>Sets what a right mouse down on the cell leaves behind (OnMouseDown): the clicked row, column and point and the right button, and
		///selects the row or not. Also used by M3LiveMenuTests.</summary>
		internal static void RightClickCell(Control grid,int row,int col,bool isRowSelected) {
			Point point=Point.Empty;
			bool isFound=false;
			for(int y=0;y<grid.Height && !isFound;y++) {
				if(PointToRow(grid,y)!=row) {
					continue;
				}
				for(int x=0;x<grid.Width;x++) {
					if(PointToCol(grid,x)==col) {
						point=new Point(x+2,y+2);
						isFound=true;
						break;
					}
				}
			}
			Assert.IsTrue(isFound,"No point of the grid lies in row "+row+", column "+col+".");
			Assert.AreEqual(row,PointToRow(grid,point.Y));
			Assert.AreEqual(col,PointToCol(grid,point.X));
			Priv.Set(grid,"_mouseDownRow",row);
			Priv.Set(grid,"_mouseDownCol",col);
			Priv.Set(grid,"_mouseClickLocation",point);
			Priv.Set(grid,"_mouseButtonLastPressed",MouseButtons.Right);
			if(grid is GridOD gridOD) {
				gridOD.SetSelected(row,isRowSelected);
			}
			else {
				((GridOld)grid).SetSelected(row,isRowSelected);
			}
		}

		///<summary>What OnMouseUp does on a right click when no caller set a menu, without showing the menu: the grid's own ContextMenuStrip.</summary>
		private static ContextMenuStrip SetOwnMenu(Control grid) {
			ContextMenuStrip contextMenuStrip=new ContextMenuStrip();
			Priv.Set(grid,"_contextMenuStripGrid",contextMenuStrip);
			grid.ContextMenuStrip=contextMenuStrip;
			return contextMenuStrip;
		}

		///<summary>Raises the strip's Opening as showing it over grid does, and returns whether the opening stays cancelled.</summary>
		private static bool Open(ContextMenuStrip contextMenuStrip,Control grid) {
			SetSourceControl(contextMenuStrip,grid);
			//As ToolStripDropDown does before Opening: cancelled when no item will show (its DisplayedItems leaves out the items that are not Available).
			CancelEventArgs cancelEventArgs=new CancelEventArgs(!contextMenuStrip.Items.Cast<ToolStripItem>().Any(x => x.Available));
			Priv.Call(contextMenuStrip,"OnOpening",cancelEventArgs);
			return cancelEventArgs.Cancel;
		}

		///<summary>The source control of a ContextMenuStrip, which showing the menu sets. It keeps it in an internal member.</summary>
		private static void SetSourceControl(object menu,Control control) {
			foreach(string name in new[] { "SourceControlInternal","sourceControl","_sourceControl" }) {
				try {
					Priv.Set(menu,name,control);
					return;
				}
				catch(MissingMemberException) {
				}
			}
			Assert.Fail("No settable source control member on "+menu.GetType().Name+".");
		}

		///<summary>The items as text: "-" for a separator, the text otherwise, with " (disabled)" for an item that is not enabled and " (hidden)"
		///for one that is not visible (a strip's item that is not Available).</summary>
		private static List<string> Describe(ContextMenuStrip contextMenuStrip) {
			return contextMenuStrip.Items.Cast<ToolStripItem>()
				.Select(x => x is ToolStripSeparator ? "-" : x.Text+(x.Enabled ? "" : " (disabled)")+(x.Available ? "" : " (hidden)")).ToList();
		}

		///<summary>A caller's strip whose only item is hidden until the caller's own Opening shows it (ControlFamily's "View DOB").</summary>
		private static ContextMenuStrip MakeCallerStripHiddenOnly() {
			ContextMenuStrip contextMenuStrip=new ContextMenuStrip();
			ToolStripMenuItem toolStripMenuItem=new ToolStripMenuItem("View DOB");
			toolStripMenuItem.Visible=false;
			contextMenuStrip.Items.Add(toolStripMenuItem);
			return contextMenuStrip;
		}

		private static ContextMenuStrip MakeCallerStrip() {
			ContextMenuStrip contextMenuStrip=new ContextMenuStrip();
			contextMenuStrip.Items.Add(new ToolStripMenuItem("Edit"));
			contextMenuStrip.Items.Add(new ToolStripSeparator());
			contextMenuStrip.Items.Add(new ToolStripMenuItem("Delete"));
			return contextMenuStrip;
		}
		#endregion Helpers

		#region GridOD
		[TestMethod]
		public void GridOD_OwnMenu_FirstOpening_HasTheItemsOfTheWinFormsMenu() {
			using(GridOD grid=MakeGrid<GridOD>("gridMain")) {
				RightClickCell(grid,0,1,isRowSelected:true);
				ContextMenuStrip contextMenuStrip=SetOwnMenu(grid);
				Assert.IsFalse(Open(contextMenuStrip,grid),"The empty menu's opening is no longer cancelled once the grid adds its items.");
				//What the bridge's new WinForms ContextMenu (as OnMouseUp made it before S3a-M1) held for the same click.
				CollectionAssert.AreEqual(new List<string> { "Copy Cell Text","Copy Rows","-",LINK },Describe(contextMenuStrip));
			}
		}

		[TestMethod]
		public void GridOD_OwnMenu_NextOpenings_FollowTheClickedCellAndRow() {
			using(GridOD grid=MakeGrid<GridOD>("gridMain")) {
				ContextMenuStrip contextMenuStrip=SetOwnMenu(grid);
				//The lists the bridge's ContextMenu gave for the same clicks when S3a-M1 compared them.
				List<(int row,int col,bool isSelected,List<string> listExpected)> listClicks=new List<(int,int,bool,List<string>)> {
					(0,1,true,new List<string> { "Copy Cell Text","Copy Rows","-",LINK }),
					//An empty cell of a row that is not selected: both disabled, and the links of row 0 are gone (row 1 has none).
					(1,1,false,new List<string> { "Copy Cell Text (disabled)","Copy Rows (disabled)" }),
					(2,0,true,new List<string> { "Copy Cell Text (disabled)","Copy Rows" }),
					(2,1,false,new List<string> { "Copy Cell Text","Copy Rows (disabled)" }),
					(0,0,false,new List<string> { "Copy Cell Text","Copy Rows (disabled)","-",LINK }),
				};
				foreach((int row,int col,bool isSelected,List<string> listExpected) in listClicks) {
					RightClickCell(grid,row,col,isSelected);
					Assert.IsFalse(Open(contextMenuStrip,grid));
					CollectionAssert.AreEqual(listExpected,Describe(contextMenuStrip),"Row "+row+", column "+col+".");
				}
			}
		}

		[TestMethod]
		public void GridOD_CallersStrip_ShowsOnlyItsOwnItems_AsBefore() {
			using(GridOD grid=MakeGrid<GridOD>("gridMain"))
			using(ContextMenuStrip contextMenuStrip=MakeCallerStrip()) {
				grid.ContextMenuStrip=contextMenuStrip;
				RightClickCell(grid,0,1,isRowSelected:true);
				Assert.IsFalse(Open(contextMenuStrip,grid));
				CollectionAssert.AreEqual(new List<string> { "Edit","-","Delete" },Describe(contextMenuStrip));
				Assert.IsFalse(grid.DoAddGridItemsToContextMenuStrip);
			}
		}

		[TestMethod]
		public void GridOD_CallersStrip_WithDoAddGridItems_GetsTheItemsTheCallersContextMenuGets() {
			foreach(bool isFlagSetFirst in new[] { true,false }) {
				using(GridOD grid=MakeGrid<GridOD>("gridMain"))
				using(ContextMenuStrip contextMenuStrip=MakeCallerStrip()) {
					if(isFlagSetFirst) {
						grid.DoAddGridItemsToContextMenuStrip=true;
						grid.ContextMenuStrip=contextMenuStrip;
					}
					else {
						grid.ContextMenuStrip=contextMenuStrip;
						grid.DoAddGridItemsToContextMenuStrip=true;
					}
					//What the bridge put in the caller's ContextMenu (Edit, a separator, Delete) for the same clicks.
					List<string> listExpected=new List<string> { "Edit","-","Delete","-","Copy Cell Text","Copy Rows","-",LINK };
					for(int i=0;i<2;i++) {//the second opening adds nothing twice
						RightClickCell(grid,0,1,isRowSelected:true);
						Assert.IsFalse(Open(contextMenuStrip,grid));
						CollectionAssert.AreEqual(listExpected,Describe(contextMenuStrip),"Flag set first: "+isFlagSetFirst+", opening "+i+".");
					}
					//Turned off again: the strip shows its own items and the ones already added, and nothing changes them any more.
					grid.DoAddGridItemsToContextMenuStrip=false;
					RightClickCell(grid,1,1,isRowSelected:false);
					Open(contextMenuStrip,grid);
					CollectionAssert.AreEqual(listExpected,Describe(contextMenuStrip));
				}
			}
		}

		[TestMethod]
		public void GridOD_SharedStrip_OnlyTheGridThatWasClickedAddsItems() {
			using(GridOD gridA=MakeGrid<GridOD>("gridA"))
			using(GridOD gridB=MakeGrid<GridOD>("gridB"))
			using(ContextMenuStrip contextMenuStrip=MakeCallerStrip()) {
				gridA.DoAddGridItemsToContextMenuStrip=true;
				gridB.DoAddGridItemsToContextMenuStrip=true;
				gridA.ContextMenuStrip=contextMenuStrip;
				gridB.ContextMenuStrip=contextMenuStrip;
				RightClickCell(gridA,1,1,isRowSelected:false);//gridA would disable both
				RightClickCell(gridB,0,1,isRowSelected:true);
				Open(contextMenuStrip,gridB);
				CollectionAssert.AreEqual(new List<string> { "Edit","-","Delete","-","Copy Cell Text","Copy Rows","-",LINK },Describe(contextMenuStrip));
			}
		}

		[TestMethod]
		public void GridOD_OwnMenu_IsDisposedWithTheGrid() {
			GridOD grid=MakeGrid<GridOD>("gridMain");
			ContextMenuStrip contextMenuStrip=SetOwnMenu(grid);
			grid.Dispose();
			Assert.IsTrue(contextMenuStrip.IsDisposed);
		}

		[TestMethod]
		public void GridOD_CallersStrip_WithDoAddGridItems_AndOnlyHiddenItems_Opens() {
			using(GridOD grid=MakeGrid<GridOD>("gridPat"))
			using(ContextMenuStrip contextMenuStrip=MakeCallerStripHiddenOnly()) {
				//Without the grid items, the strip of hidden items stays cancelled, as any such strip does.
				grid.ContextMenuStrip=contextMenuStrip;
				RightClickCell(grid,1,0,isRowSelected:true);
				Assert.IsTrue(Open(contextMenuStrip,grid));
				CollectionAssert.AreEqual(new List<string> { "View DOB (hidden)" },Describe(contextMenuStrip));
				//With them, it opens with the items the bridge's ContextMenu showed (as FormMedical's grids, whose one item is hidden on most rows).
				grid.DoAddGridItemsToContextMenuStrip=true;
				List<string> listExpected=new List<string> { "View DOB (hidden)","-","Copy Cell Text","Copy Rows" };
				for(int i=0;i<2;i++) {
					RightClickCell(grid,1,0,isRowSelected:true);
					Assert.IsFalse(Open(contextMenuStrip,grid),"Opening "+i+": the grid's items show, so the menu opens.");
					CollectionAssert.AreEqual(listExpected,Describe(contextMenuStrip),"Opening "+i+".");
				}
			}
		}

		[TestMethod]
		public void GridOD_RightMouseUp_MakesItsOwnMenuOnlyWithoutACallersStrip() {
			//OnMouseUp makes the grid's own strip only when no ContextMenuStrip is set and ShowContextMenu is true (before S3a-M3b it also checked
			//for a WinForms ContextMenu). The cases that make no menu are run here; making one also shows it, which a test must not.
			using(GridOD grid=MakeGrid<GridOD>("gridMain"))
			using(ContextMenuStrip contextMenuStrip=MakeCallerStrip()) {
				grid.DoAddGridItemsToContextMenuStrip=true;
				grid.ContextMenuStrip=contextMenuStrip;
				RightClickCell(grid,0,1,isRowSelected:true);
				Priv.Call(grid,"OnMouseUp",new MouseEventArgs(MouseButtons.Right,1,5,5,0));
				Assert.AreSame(contextMenuStrip,grid.ContextMenuStrip,"A caller's strip stays the grid's menu.");
				Assert.IsNull(Priv.Get(grid,"_contextMenuStripGrid"),"No menu of the grid's own.");
			}
			using(GridOD grid=MakeGrid<GridOD>("gridMain")) {
				grid.ShowContextMenu=false;
				RightClickCell(grid,0,1,isRowSelected:true);
				Priv.Call(grid,"OnMouseUp",new MouseEventArgs(MouseButtons.Right,1,5,5,0));
				Assert.IsNull(grid.ContextMenuStrip,"ShowContextMenu=false: no menu.");
				Assert.IsNull(Priv.Get(grid,"_contextMenuStripGrid"));
			}
		}

		[TestMethod]
		public void GridOD_PopupHelper_Lan_HaveNoWinFormsMenuBridge() {
			//S3a-M3b removed the regions "Bridge for S3a-M3b": GridOD's ContextMenu path, PopupHelper's MenuItem links and Lan's translation of a
			//grid's ContextMenu. What covers the paths that replaced them: the strip tests above and M3LiveMenuTests' FormMedical test (GridOD),
			//MenuPartsTests (PopupHelper), LanToolStripTests (Lan).
			BindingFlags bindingFlags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly;
			foreach(string name in new[] { "OnContextMenuChanged","ContextMenu_Popup","ContextMenuAddPopupLinks","IsWinFormsContextMenuSet" }) {
				Assert.AreEqual(0,typeof(GridOD).GetMethods(bindingFlags).Count(x => x.Name==name),"GridOD."+name);
			}
			Assert.IsNull(typeof(GridOD).GetField("_listMenuItemsLinks",bindingFlags));
			Assert.AreEqual(0,typeof(OpenPmsDentalBusiness.UI.PopupHelper).GetMethods(bindingFlags).Count(x => x.Name=="GetContextMenuItemLinks"));
			foreach(string name in new[] { "TranslateGridWinFormsContextMenu","TranslateMenuItems" }) {
				Assert.AreEqual(0,typeof(OpenPmsDental.Lan).GetMethods(bindingFlags).Count(x => x.Name==name),"Lan."+name);
			}
			//No field or method of the three has a WinForms menu type in its signature.
			Type[] arrayTypesMenu=new[] { typeof(Menu),typeof(List<MenuItem>) };
			Func<Type,bool> isMenuType=x => arrayTypesMenu.Any(y => y.IsAssignableFrom(x));
			foreach(Type type in new[] { typeof(GridOD),typeof(OpenPmsDentalBusiness.UI.PopupHelper),typeof(OpenPmsDental.Lan) }) {
				foreach(FieldInfo fieldInfo in type.GetFields(bindingFlags)) {
					Assert.IsFalse(isMenuType(fieldInfo.FieldType),type.Name+"."+fieldInfo.Name);
				}
				foreach(MethodInfo methodInfo in type.GetMethods(bindingFlags)) {
					Assert.IsFalse(isMenuType(methodInfo.ReturnType) || methodInfo.GetParameters().Any(x => isMenuType(x.ParameterType)),type.Name+"."+methodInfo.Name);
				}
			}
		}
		#endregion GridOD

		#region GridOld
		//GridOld's WinForms ContextMenu path (the bridge) went with S3a-M2, which converted its last caller (ControlChart's gridProg). The lists
		//below are the ones the bridge's ContextMenu gave for the same clicks when S3a-M1 compared them.
		[TestMethod]
		public void GridOld_OwnMenu_HasTheItemsOfTheWinFormsMenu_AtEveryOpening() {
			using(GridOld grid=MakeGrid<GridOld>("gridProg")) {
				ContextMenuStrip contextMenuStrip=SetOwnMenu(grid);
				List<(int row,int col,List<string> listExpected)> listClicks=new List<(int,int,List<string>)> {
					(0,1,new List<string> { "Copy Cell Text","-",LINK }),
					(1,1,new List<string> { "Copy Cell Text (disabled)" }),
					(0,0,new List<string> { "Copy Cell Text","-",LINK }),
				};
				bool isFirst=true;
				foreach((int row,int col,List<string> listExpected) in listClicks) {
					RightClickCell(grid,row,col,isRowSelected:true);
					bool isCancelled=Open(contextMenuStrip,grid);
					if(isFirst) {
						Assert.IsFalse(isCancelled,"The empty menu's opening is no longer cancelled once the grid adds its items.");
						isFirst=false;
					}
					CollectionAssert.AreEqual(listExpected,Describe(contextMenuStrip),"Row "+row+", column "+col+".");
				}
			}
		}

		[TestMethod]
		public void GridOld_CallersStrip_DefaultAndWithDoAddGridItems() {
			using(GridOld grid=MakeGrid<GridOld>("gridProg"))
			using(ContextMenuStrip contextMenuStrip=MakeCallerStrip()) {
				grid.ContextMenuStrip=contextMenuStrip;
				RightClickCell(grid,0,1,isRowSelected:true);
				Open(contextMenuStrip,grid);
				CollectionAssert.AreEqual(new List<string> { "Edit","-","Delete" },Describe(contextMenuStrip),"A caller's strip as before.");
				//ControlChart's gridProg after S3a-M2: its menu as a strip, with the grid items.
				grid.DoAddGridItemsToContextMenuStrip=true;
				Open(contextMenuStrip,grid);
				List<string> listExpected=new List<string> { "Edit","-","Delete","-","Copy Cell Text","-",LINK };
				CollectionAssert.AreEqual(listExpected,Describe(contextMenuStrip));
			}
		}

		[TestMethod]
		public void GridOld_CallersStrip_WithDoAddGridItems_AndOnlyHiddenItems_Opens() {
			using(GridOld grid=MakeGrid<GridOld>("gridPtInfo"))
			using(ContextMenuStrip contextMenuStrip=MakeCallerStripHiddenOnly()) {
				grid.ContextMenuStrip=contextMenuStrip;
				RightClickCell(grid,1,0,isRowSelected:true);
				Assert.IsTrue(Open(contextMenuStrip,grid),"Without the grid items the strip of hidden items stays cancelled.");
				grid.DoAddGridItemsToContextMenuStrip=true;
				List<string> listExpected=new List<string> { "View DOB (hidden)","-","Copy Cell Text" };//GridOld has no "Copy Rows", as before
				for(int i=0;i<2;i++) {
					RightClickCell(grid,1,0,isRowSelected:true);
					Assert.IsFalse(Open(contextMenuStrip,grid),"Opening "+i+": the grid's items show, so the menu opens.");
					CollectionAssert.AreEqual(listExpected,Describe(contextMenuStrip),"Opening "+i+".");
				}
			}
		}
		#endregion GridOld
	}
}
