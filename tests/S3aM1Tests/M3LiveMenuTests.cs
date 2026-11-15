// Package S3a-M3a: representative converted menus and grids built in memory (never shown, on an STA thread): forms whose designer menus are
// ContextMenuStrips on their grids with the grid's own items, the code-built menus of ODcodeBox and SheetComboBox, the grid that unhooks a strip
// it no longer shows, and the User Query window's DataGridView (columns, cell text, alignment). Nothing needs a database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace S3aM1Tests {
	[TestClass]
	public class M3LiveMenuTests {
		///<summary>Compares each designer menu of the form with the expected list of M3DesignerMenuTests (from main's WinForms menus), on the live
		///form, and checks that the grids show the strip with the grid's own items.</summary>
		private static void AssertFormMenus(Form form,string designerFile,params (string grid,string menu)[] arrayGrids) {
			foreach(M3DesignerMenuTests.ExpectedMenu expectedMenu in M3DesignerMenuTests.DictExpected[designerFile]) {
				ContextMenuStrip contextMenuStrip=Priv.Get(form,expectedMenu.Name) as ContextMenuStrip;
				Assert.IsNotNull(contextMenuStrip,expectedMenu.Name);
				CollectionAssert.AreEqual(expectedMenu.ArrayItems,M2Support.Describe(contextMenuStrip),
					expectedMenu.Name+": "+string.Join(" | ",M2Support.Describe(contextMenuStrip)));
				List<string> listOpening=M2Support.GetOpeningHandlerNames(contextMenuStrip).Where(x => x!="ContextMenuStrip_Opening").ToList();
				CollectionAssert.AreEqual(expectedMenu.Opening==null ? new List<string>() : new List<string> { expectedMenu.Opening },listOpening,expectedMenu.Name);
			}
			foreach((string grid,string menu) in arrayGrids) {
				GridOD gridOD=(GridOD)Priv.Get(form,grid);
				Assert.AreSame(Priv.Get(form,menu),gridOD.ContextMenuStrip,grid);
				Assert.IsTrue(gridOD.DoAddGridItemsToContextMenuStrip,grid);
				Assert.IsNull(gridOD.ContextMenu,grid+": no WinForms ContextMenu.");
				Assert.IsTrue(M2Support.GetOpeningHandlerNames(gridOD.ContextMenuStrip).Contains("ContextMenuStrip_Opening"),grid+": the grid's Opening.");
			}
		}

		#region Forms
		[TestMethod]
		public void FormRadOrderList_GridMenu_KeepsItsItemsAndTheGridItems() {
			M2Support.RunSta(() => {
				using(FormRadOrderList formRadOrderList=new FormRadOrderList(new Userod())) {
					AssertFormMenus(formRadOrderList,@"OpenPmsDental\Forms\FormRadOrderList.designer.cs",("gridMain","menuRightClick"));
					//Opened over the grid: its two items, then the grid's separator and Copy Cell Text and Copy Rows.
					GridOD gridMain=(GridOD)Priv.Get(formRadOrderList,"gridMain");
					ContextMenuStrip contextMenuStrip=gridMain.ContextMenuStrip;
					Assert.IsFalse(M2Support.Open(contextMenuStrip,gridMain));
					CollectionAssert.AreEqual(new[] { "See Chart","See Family","-","Copy Cell Text","Copy Rows" },
						contextMenuStrip.Items.Cast<ToolStripItem>().Select(x => x is ToolStripSeparator ? "-" : x.Text).ToArray());
				}
			});
		}

		[TestMethod]
		public void FormRecurringChargesHistory_GridMenu_HiddenItemsAndOpening() {
			M2Support.RunSta(() => {
				using(FormRecurringChargesHistory form=new FormRecurringChargesHistory()) {
					AssertFormMenus(form,@"OpenPmsDental\Forms\FormRecurringChargesHistory.designer.cs",("gridMain","contextMenu"));
					//The form's Opening (was the Popup) runs before the grid's, as the Popup did: the designer subscribes it first.
					List<string> listOpening=M2Support.GetOpeningHandlerNames((ContextMenuStrip)Priv.Get(form,"contextMenu"));
					CollectionAssert.AreEqual(new[] { "contextMenu_Popup","ContextMenuStrip_Opening" },listOpening);
				}
			});
		}

		[TestMethod]
		public void FormScreenGroupEdit_GridMenu_IndexOfTheClickedItem() {
			M2Support.RunSta(() => {
				using(FormScreenGroupEdit form=new FormScreenGroupEdit(new ScreenGroup())) {
					AssertFormMenus(form,@"OpenPmsDental\Forms\FormScreenGroupEdit.designer.cs",("gridScreenPats","patContextMenu"));
					//patContextMenuItem_Click takes the permission from the item's index among the strip's items (was MenuItems.IndexOf).
					ContextMenuStrip contextMenuStrip=(ContextMenuStrip)Priv.Get(form,"patContextMenu");
					Assert.AreEqual((int)PatScreenPerm.Refused,contextMenuStrip.Items.IndexOf((ToolStripItem)Priv.Get(form,"menuItemRefused")));
				}
			});
		}
		#endregion Forms

		#region Controls
		[TestMethod]
		public void ODcodeBox_Menu_TheSameItemsAndShortcuts() {
			M2Support.RunSta(() => {
				using(ODcodeBox oDcodeBox=new ODcodeBox(true)) {
					Assert.IsNull(oDcodeBox.ContextMenu,"No WinForms ContextMenu.");
					//The WinForms Shortcut values CtrlX ... CtrlShiftR, as Keys (KeysConverter writes them as Describe does).
					KeysConverter keysConverter=new KeysConverter();
					string ctrlShiftC=keysConverter.ConvertToInvariantString(Keys.Control|Keys.Shift|Keys.C);
					string ctrlShiftR=keysConverter.ConvertToInvariantString(Keys.Control|Keys.Shift|Keys.R);
					CollectionAssert.AreEqual(new[] { "Cut ->menuItemCut_Click [Ctrl+X]","Copy ->menuItemCopy_Click [Ctrl+C]","Paste ->menuItemPaste_Click [Ctrl+V]",
						"Select All ->menuItemSelectAll_Click [Ctrl+A]","Comment Selection ->menuItemAddComment_Click ["+ctrlShiftC+"]",
						"Remove Comment From Selection ->menuItemRemoveComment_Click ["+ctrlShiftR+"]" },M2Support.Describe(oDcodeBox.ContextMenuStrip));
					StringAssert.Contains(ctrlShiftC,"Shift");
				}
				using(ODcodeBox oDcodeBox=new ODcodeBox(false)) {
					Assert.AreEqual(4,oDcodeBox.ContextMenuStrip.Items.Count,"The comment items only for query text.");
				}
			});
		}

		[TestMethod]
		public void SheetComboBox_Menu_EscapedOptions_ClickSelects() {
			M2Support.RunSta(() => {
				using(SheetComboBox sheetComboBox=new SheetComboBox("A;A|B&C|-")) {
					ContextMenuStrip contextMenuStrip=(ContextMenuStrip)Priv.Get(sheetComboBox,"_contextMenu");
					//Menu items, never separators: "&" doubled and a lone "-" as "&-", as for the WinForms MenuItem.
					Assert.IsTrue(contextMenuStrip.Items.Cast<ToolStripItem>().All(x => x is ToolStripMenuItem));
					CollectionAssert.AreEqual(new[] { "A","B&&C","&-" },contextMenuStrip.Items.Cast<ToolStripItem>().Select(x => x.Text).ToArray());
					contextMenuStrip.Items[2].PerformClick();
					Assert.AreEqual("-",sheetComboBox.SelectedOption,"The option of the clicked item, by its index.");
					contextMenuStrip.Items[1].PerformClick();
					Assert.AreEqual("B&C",sheetComboBox.SelectedOption);
				}
			});
		}

		[TestMethod]
		public void GridOD_AnotherStrip_UnhooksTheStripItHadBefore() {
			//FormTreatPlanCurEdit sets a shared strip on the clicked grid and a null strip on both grids at each mouse down.
			M2Support.RunSta(() => {
				using(GridOD gridOD=new GridOD())
				using(ContextMenuStrip contextMenuStrip1=new ContextMenuStrip())
				using(ContextMenuStrip contextMenuStrip2=new ContextMenuStrip()) {
					gridOD.ContextMenuStrip=contextMenuStrip1;
					gridOD.DoAddGridItemsToContextMenuStrip=true;
					CollectionAssert.Contains(M2Support.GetOpeningHandlerNames(contextMenuStrip1),"ContextMenuStrip_Opening");
					gridOD.ContextMenuStrip=contextMenuStrip2;
					CollectionAssert.DoesNotContain(M2Support.GetOpeningHandlerNames(contextMenuStrip1),"ContextMenuStrip_Opening");
					Assert.AreEqual(1,M2Support.GetOpeningHandlerNames(contextMenuStrip2).Count(x => x=="ContextMenuStrip_Opening"));
					gridOD.ContextMenuStrip=null;
					CollectionAssert.DoesNotContain(M2Support.GetOpeningHandlerNames(contextMenuStrip2),"ContextMenuStrip_Opening");
					gridOD.ContextMenuStrip=contextMenuStrip1;
					gridOD.DoAddGridItemsToContextMenuStrip=true;//again: still once
					Assert.AreEqual(1,M2Support.GetOpeningHandlerNames(contextMenuStrip1).Count(x => x=="ContextMenuStrip_Opening"));
				}
			});
		}
		#endregion Controls

		#region User Query grid
		[TestMethod]
		public void FormQuery_Grid_ColumnsFromTheTable_TextAndAlignment() {
			M2Support.RunSta(() => {
				using(FormQuery formQuery=new FormQuery(null)) {
					DataGridView dataGridView=(DataGridView)Priv.Get(formQuery,"grid2");
					Assert.IsTrue(dataGridView.ReadOnly);
					Assert.IsFalse(dataGridView.AllowUserToAddRows,"The DataGrid had no new row: the grid is read-only.");
					Assert.IsFalse(dataGridView.AutoGenerateColumns);
					Assert.IsNotNull(Priv.Get(formQuery,"labelGridCaption"),"The DataGrid's caption, as a label above the grid.");
					DataTable table=new DataTable();
					table.Columns.Add("PatNum",typeof(long));
					table.Columns.Add("IsActive",typeof(bool));
					table.Columns.Add("Photo",typeof(byte[]));
					table.Columns.Add("LName",typeof(string));
					table.Rows.Add(12L,true,new byte[] { 1,2,3 },"Doe");
					table.Rows.Add(13L,false,DBNull.Value,DBNull.Value);
					formQuery.BindGrid(table);
					CollectionAssert.AreEqual(new[] { "PatNum","IsActive","Photo","LName" },dataGridView.Columns.Cast<DataGridViewColumn>().Select(x => x.HeaderText).ToArray());
					CollectionAssert.AreEqual(new[] { "PatNum","IsActive","Photo","LName" },
						dataGridView.Columns.Cast<DataGridViewColumn>().Select(x => x.DataPropertyName).ToArray());
					Assert.IsInstanceOfType(dataGridView.Columns[1],typeof(DataGridViewCheckBoxColumn),"A bool column is a check box, as the DataGrid made it.");
					Assert.IsInstanceOfType(dataGridView.Columns[2],typeof(DataGridViewTextBoxColumn),"A blob is text, never an image column.");
					Assert.IsTrue(dataGridView.Columns.Cast<DataGridViewColumn>().All(x => x.Width==75 && x.SortMode==DataGridViewColumnSortMode.Automatic));
					//The printed text of a cell (the DataGrid's grid2[row,col].ToString()), in the grid's order.
					Assert.AreEqual("12",Priv.Call(formQuery,"GetGridCellText",0,0));
					Assert.AreEqual("Doe",Priv.Call(formQuery,"GetGridCellText",0,3));
					Assert.AreEqual("",Priv.Call(formQuery,"GetGridCellText",1,3),"DBNull prints as empty text.");
					table.DefaultView.Sort="PatNum DESC";
					Assert.AreEqual("13",Priv.Call(formQuery,"GetGridCellText",0,0),"The grid's order is the view's.");
					formQuery.BindGrid(new DataTable());
					Assert.AreEqual(0,dataGridView.Columns.Count,"A new table, new columns.");
				}
			});
			Assert.AreEqual(DataGridViewContentAlignment.MiddleRight,FormQuery.GetCellAlignment(HorizontalAlignment.Right));
			Assert.AreEqual(DataGridViewContentAlignment.MiddleCenter,FormQuery.GetCellAlignment(HorizontalAlignment.Center));
			Assert.AreEqual(DataGridViewContentAlignment.MiddleLeft,FormQuery.GetCellAlignment(HorizontalAlignment.Left));
		}
		#endregion User Query grid
	}
}
