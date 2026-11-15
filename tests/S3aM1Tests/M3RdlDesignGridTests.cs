// Package S3a-M3a: the report designer's grids, WinForms DataGrids before, are DataGridViews bound to the same DataTables. The designer (RdlDesign,
// RdlDesigner.exe) is loaded from its build folder, since no test project references it; its classes are internal and reached by reflection.
// The dialogs are built in memory and never shown (their handles are created for the grids' editing). Checked: the columns (names, headings,
// widths, read-only, not sortable), an edit typed into a cell that reaches the table when the grid loses the focus (as the DataGrid's edit did),
// an untouched new row that does not, the dialog's own validation after the edit, the combo box column DGCBColumn (items, style, and a drop-down
// list that takes only its items), and the main menu's shortcut lookup that raises the menus' Opening handlers first.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace S3aM1Tests {
	[TestClass]
	public class M3RdlDesignGridTests {
		private const BindingFlags FLAGS=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
		private static Assembly _assemblyRdlDesign;

		///<summary>RdlDesigner.exe from its build folder (Debug, else any configuration). Inconclusive when it has not been built.</summary>
		private static Assembly GetRdlDesign() {
			if(_assemblyRdlDesign!=null) {
				return _assemblyRdlDesign;
			}
			string pathBin=M2Support.RepoFile("RdlDesign","bin");
			string path=Path.Combine(pathBin,"Debug","RdlDesigner.exe");
			if(!File.Exists(path) && Directory.Exists(pathBin)) {
				path=Directory.EnumerateFiles(pathBin,"RdlDesigner.exe",SearchOption.AllDirectories).FirstOrDefault()??path;
			}
			if(!File.Exists(path)) {
				Assert.Inconclusive("RdlDesigner.exe is not built ("+path+"); build ReportDesigner first.");
			}
			_assemblyRdlDesign=Assembly.LoadFrom(path);
			return _assemblyRdlDesign;
		}

		private static Type GetType(string name) {
			Type type=GetRdlDesign().GetType("fyiReporting.RdlDesign."+name,throwOnError:false);
			Assert.IsNotNull(type,name);
			return type;
		}

		private static object Create(string typeName,params object[] arrayArgs) {
			try {
				return Activator.CreateInstance(GetType(typeName),FLAGS,null,arrayArgs,null);
			}
			catch(TargetInvocationException ex) {
				throw ex.InnerException;
			}
		}

		private static object CallStatic(string typeName,string method,params object[] arrayArgs) {
			try {
				return GetType(typeName).GetMethod(method,FLAGS).Invoke(null,arrayArgs);
			}
			catch(TargetInvocationException ex) {
				throw ex.InnerException;
			}
		}

		///<summary>A List of an internal type of the designer, filled with the items.</summary>
		private static IList MakeList(string typeName,params object[] arrayItems) {
			IList list=(IList)Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(GetType(typeName)));
			foreach(object item in arrayItems) {
				list.Add(item);
			}
			return list;
		}

		///<summary>Types text into a cell as the user does: makes it current, begins the edit, sets the editing control's text.</summary>
		private static void TypeInto(DataGridView dataGridView,int row,int col,string text) {
			dataGridView.CurrentCell=dataGridView.Rows[row].Cells[col];
			Assert.IsTrue(dataGridView.BeginEdit(false),"The edit begins in row "+row+", column "+col+".");
			dataGridView.EditingControl.Text=text;
			Assert.IsTrue(dataGridView.IsCurrentCellDirty,"Typing marks the cell as changed.");
		}

		[TestMethod]
		public void DialogValidValues_TypedRow_ReachesTheTable_UntouchedNewRowDoesNot() {
			M2Support.RunSta(() => {
				using(Form form=(Form)Create("DialogValidValues",new object[] { null })) {
					IntPtr handle=form.Handle;
					DataGridView dataGridView=(DataGridView)Priv.Get(form,"dgParms");
					IntPtr handleGrid=dataGridView.Handle;
					DataTable dataTable=(DataTable)Priv.Get(form,"_DataTable");
					CollectionAssert.AreEqual(new[] { "Value","Label" },dataGridView.Columns.Cast<DataGridViewColumn>().Select(x => x.HeaderText).ToArray());
					CollectionAssert.AreEqual(new[] { "Value","Label" },dataGridView.Columns.Cast<DataGridViewColumn>().Select(x => x.DataPropertyName).ToArray());
					CollectionAssert.AreEqual(new[] { 140,140 },dataGridView.Columns.Cast<DataGridViewColumn>().Select(x => x.Width).ToArray());
					Assert.IsTrue(dataGridView.Columns.Cast<DataGridViewColumn>().All(x => x.SortMode==DataGridViewColumnSortMode.NotSortable),"AllowSorting = false.");
					Assert.IsFalse(dataGridView.AutoGenerateColumns);
					//Only the new row; making it current and committing without typing adds nothing.
					Assert.AreEqual(1,dataGridView.Rows.Count);
					dataGridView.CurrentCell=dataGridView.Rows[0].Cells[0];
					CallStatic("DataGridViewSetup","CommitEdits",dataGridView);
					Assert.AreEqual(0,dataTable.Rows.Count,"An untouched new row is not added.");
					Assert.IsFalse((bool)CallStatic("DataGridViewSetup","IsCurrentRowData",dataGridView),"The new row has no row in the table.");
					//A value typed into the new row reaches the table when the grid loses the focus (the OK button).
					TypeInto(dataGridView,0,0,"North");
					Priv.Call(dataGridView,"OnLeave",EventArgs.Empty);
					Assert.AreEqual(1,dataTable.Rows.Count);
					Assert.AreEqual("North",dataTable.Rows[0]["Value"]);
					IEnumerable listValues=(IEnumerable)Priv.Get(form,"ValidValues");
					object item=listValues.Cast<object>().Single();
					Assert.AreEqual("North",Priv.Get(item,"Value"));
					Assert.IsNull(Priv.Get(item,"Label"));
					//Delete on a data row removes it; on the new row it does nothing (the DataGrid threw there).
					DataGridViewSetupSetCurrent(dataGridView,0);
					Priv.Call(form,"bDelete_Click",form,EventArgs.Empty);
					Assert.AreEqual(0,dataTable.Rows.Count);
					DataGridViewSetupSetCurrent(dataGridView,dataGridView.Rows.Count-1);
					Priv.Call(form,"bDelete_Click",form,EventArgs.Empty);
					Assert.AreEqual(0,dataTable.Rows.Count);
				}
			});
		}

		private static void DataGridViewSetupSetCurrent(DataGridView dataGridView,int row) {
			CallStatic("DataGridViewSetup","SetCurrentRowIndex",dataGridView,row);
			Assert.AreEqual(row,(int)CallStatic("DataGridViewSetup","GetCurrentRowIndex",dataGridView));
		}

		[TestMethod]
		public void DrillParametersDialog_ReadOnlyName_EditedValue_PassesItsValidation() {
			M2Support.RunSta(() => {
				object drillParameter=Create("DrillParameter","Clinic","","false");
				using(Form form=(Form)Create("DrillParametersDialog","report1",MakeList("DrillParameter",drillParameter))) {
					IntPtr handle=form.Handle;
					DataGridView dataGridView=(DataGridView)Priv.Get(form,"dgParms");
					IntPtr handleGrid=dataGridView.Handle;
					DataTable dataTable=(DataTable)Priv.Get(form,"_DataTable");
					CollectionAssert.AreEqual(new[] { "Parameter Name","Value","Omit" },dataGridView.Columns.Cast<DataGridViewColumn>().Select(x => x.HeaderText).ToArray());
					CollectionAssert.AreEqual(new[] { 140,140,70 },dataGridView.Columns.Cast<DataGridViewColumn>().Select(x => x.Width).ToArray());
					Assert.AreEqual(2,dataGridView.Rows.Count,"The parameter and the new row.");
					Assert.AreEqual("Clinic",dataGridView.Rows[0].Cells[0].Value);
					//The name column is read-only: no edit begins there.
					dataGridView.CurrentCell=dataGridView.Rows[0].Cells[0];
					Assert.IsFalse(dataGridView.BeginEdit(false),"Parameter Name is read-only.");
					//The value, typed in and committed (the grid loses the focus), satisfies the dialog's check that every parameter has a value.
					TypeInto(dataGridView,0,1,"7");
					Priv.Call(dataGridView,"OnLeave",EventArgs.Empty);
					Assert.AreEqual("7",dataTable.Rows[0]["Value"]);
					CancelEventArgs cancelEventArgs=new CancelEventArgs();
					Priv.Call(form,"DrillParametersDialog_Validating",form,cancelEventArgs);
					Assert.IsFalse(cancelEventArgs.Cancel,"Every parameter has a value.");
					IEnumerable listParameters=(IEnumerable)Priv.Get(form,"DrillParameters");
					Assert.AreEqual("7",Priv.Get(listParameters.Cast<object>().Single(),"ParameterValue"));
				}
			});
		}

		[TestMethod]
		public void DGCBColumn_EditsWithItsComboBox_DropDownListTakesOnlyItsItems() {
			M2Support.RunSta(() => {
				using(Form form=new Form()) {
					DataGridView dataGridView=new DataGridView();//disposed with the form
					form.Controls.Add(dataGridView);
					dataGridView.AutoGenerateColumns=false;
					DataGridViewColumn columnList=(DataGridViewColumn)Create("DGCBColumn");
					DataGridViewColumn columnEditable=(DataGridViewColumn)Create("DGCBColumn",ComboBoxStyle.DropDown);
					ComboBox comboBoxList=(ComboBox)Priv.Get(columnList,"CB");
					ComboBox comboBoxEditable=(ComboBox)Priv.Get(columnEditable,"CB");
					Assert.AreEqual(ComboBoxStyle.DropDownList,comboBoxList.DropDownStyle,"The parameterless column is a drop-down list, as before.");
					Assert.AreEqual(ComboBoxStyle.DropDown,comboBoxEditable.DropDownStyle);
					comboBoxList.Items.AddRange(new object[] { "Ascending","Descending" });
					comboBoxEditable.Items.AddRange(new object[] { "=Fields!A.Value","=Fields!B.Value" });
					columnList.DataPropertyName="Direction";
					columnEditable.DataPropertyName="Expression";
					dataGridView.Columns.AddRange(new[] { columnList,columnEditable });
					DataTable dataTable=new DataTable();
					dataTable.Columns.Add("Direction",typeof(string));
					dataTable.Columns.Add("Expression",typeof(string));
					dataTable.Rows.Add("Ascending","=Fields!A.Value");
					dataGridView.DataSource=dataTable;
					IntPtr handle=form.Handle;
					IntPtr handleGrid=dataGridView.Handle;
					//The editing control is a combo box with the column's items and style.
					dataGridView.CurrentCell=dataGridView.Rows[0].Cells[0];
					Assert.IsTrue(dataGridView.BeginEdit(false));
					ComboBox comboBoxEditing=(ComboBox)dataGridView.EditingControl;
					Assert.AreEqual("DGCBEditingControl",comboBoxEditing.GetType().Name);
					CollectionAssert.AreEqual(new[] { "Ascending","Descending" },comboBoxEditing.Items.Cast<object>().ToArray());
					Assert.AreEqual(ComboBoxStyle.DropDownList,comboBoxEditing.DropDownStyle);
					Assert.AreEqual("Ascending",comboBoxEditing.Text,"The cell's text is selected in the list.");
					Assert.IsFalse(dataGridView.IsCurrentCellDirty,"Filling the combo box does not mark the cell as changed.");
					//A drop-down list takes only its items: a text that is not one of them is not taken (the selection stays).
					comboBoxEditing.Text="Sideways";
					Assert.AreEqual("Ascending",comboBoxEditing.Text);
					Assert.IsFalse(dataGridView.IsCurrentCellDirty,"Nothing was taken.");
					comboBoxEditing.SelectedIndex=1;
					Assert.IsTrue(dataGridView.IsCurrentCellDirty);
					Assert.IsTrue(dataGridView.EndEdit());
					//An editable combo box takes any text.
					dataGridView.CurrentCell=dataGridView.Rows[0].Cells[1];
					Assert.IsTrue(dataGridView.BeginEdit(false));
					ComboBox comboBoxEditing2=(ComboBox)dataGridView.EditingControl;
					Assert.AreEqual(ComboBoxStyle.DropDown,comboBoxEditing2.DropDownStyle);
					CollectionAssert.AreEqual(new[] { "=Fields!A.Value","=Fields!B.Value" },comboBoxEditing2.Items.Cast<object>().ToArray());
					comboBoxEditing2.Text="=Fields!C.Value";
					CallStatic("DataGridViewSetup","CommitEdits",dataGridView);
					Assert.AreEqual("Descending",dataTable.Rows[0]["Direction"]);
					Assert.AreEqual("=Fields!C.Value",dataTable.Rows[0]["Expression"]);
					//A cloned column keeps the items and the style (the DataGridView clones columns, as for the cell template).
					DataGridViewColumn columnClone=(DataGridViewColumn)columnEditable.Clone();
					ComboBox comboBoxClone=(ComboBox)Priv.Get(columnClone,"CB");
					Assert.AreEqual(ComboBoxStyle.DropDown,comboBoxClone.DropDownStyle);
					Assert.AreEqual(2,comboBoxClone.Items.Count);
					columnClone.Dispose();
					dataGridView.CurrentCell=null;//leaves the cell before the form and its grid are disposed
				}
			});
		}

		[TestMethod]
		public void RdlDesigner_ShortcutPath_TheMenusThatHoldTheItem() {
			M2Support.RunSta(() => {
				using(MenuStrip menuStrip=new MenuStrip()) {
					ToolStripMenuItem menuFile=new ToolStripMenuItem("&File");
					ToolStripMenuItem menuSave=new ToolStripMenuItem("&Save",null,(sender,e) => { },Keys.Control|Keys.S);
					ToolStripMenuItem menuExport=new ToolStripMenuItem("Export");
					ToolStripMenuItem menuPdf=new ToolStripMenuItem("PDF...",null,(sender,e) => { },Keys.Control|Keys.Shift|Keys.P);
					menuExport.DropDownItems.Add(menuPdf);
					menuFile.DropDownItems.AddRange(new ToolStripItem[] { menuSave,new ToolStripSeparator(),menuExport });
					ToolStripMenuItem menuEdit=new ToolStripMenuItem("&Edit");
					menuEdit.DropDownItems.Add(new ToolStripMenuItem("Find Next",null,(sender,e) => { },Keys.F3));
					menuStrip.Items.AddRange(new ToolStripItem[] { menuFile,menuEdit });
					Type typeDesigner=GetType("RdlDesigner");
					MethodInfo methodInfo=typeDesigner.GetMethod("FindShortcutPath",FLAGS);
					Assert.IsNotNull(methodInfo);
					System.Collections.Generic.List<ToolStripMenuItem> listPath=(System.Collections.Generic.List<ToolStripMenuItem>)methodInfo.Invoke(null,new object[] { menuStrip.Items,Keys.Control|Keys.S });
					CollectionAssert.AreEqual(new[] { menuFile },listPath);
					listPath=(System.Collections.Generic.List<ToolStripMenuItem>)methodInfo.Invoke(null,new object[] { menuStrip.Items,Keys.Control|Keys.Shift|Keys.P });
					CollectionAssert.AreEqual(new[] { menuFile,menuExport },listPath);
					listPath=(System.Collections.Generic.List<ToolStripMenuItem>)methodInfo.Invoke(null,new object[] { menuStrip.Items,Keys.F3 });
					CollectionAssert.AreEqual(new[] { menuEdit },listPath);
					Assert.IsNull(methodInfo.Invoke(null,new object[] { menuStrip.Items,Keys.Control|Keys.Q }),"No item has the key.");
					Assert.IsNull(methodInfo.Invoke(null,new object[] { menuStrip.Items,Keys.None }));
				}
			});
		}

		[TestMethod]
		public void RdlDesigner_Source_MainMenuIsAMenuStrip() {
			string source=M2Support.ReadRepoFile("RdlDesign","RdlDesigner.cs");
			StringAssert.Contains(source,"menuMain = new MenuStrip();");
			StringAssert.Contains(source,"menuMain.MdiWindowListItem = menuWindow;");
			StringAssert.Contains(source,"this.Controls.Add(menuMain);");
			StringAssert.Contains(source,"this.MainMenuStrip = menuMain;");
			Assert.IsFalse(source.Contains("this.Menu = "),"No WinForms MainMenu.");
			//Each menu's Popup handler is its DropDownOpening handler, and the shortcut lookup raises the same handlers.
			foreach((string menu,string handler) in new[] { ("menuFile","menuFile_Popup"),("menuEdit","menuEdit_Popup"),("menuView","menuView_Popup"),
				("menuData","menuData_Popup"),("menuFormat","menuFormat_Popup"),("menuTools","menuTools_Popup"),("menuWindow","menuWnd_Popup") })
			{
				Assert.IsTrue(System.Text.RegularExpressions.Regex.IsMatch(source,
					System.Text.RegularExpressions.Regex.Escape(menu)+@"\.DropDownOpening \+= ?new EventHandler\((?:this\.)?"+handler+@"\);"),menu);
				StringAssert.Contains(source,"_dictMenuPopups.Add("+menu+", this."+handler+");");
			}
			//The shortcuts of the File menu, as Keys (the WinForms Shortcut values).
			StringAssert.Contains(source,"menuSave = new ToolStripMenuItem(\"&Save\", null, new EventHandler(this.menuFileSave_Click), (Keys.Control|Keys.S));");
			StringAssert.Contains(source,"menuViewRDL = new ToolStripMenuItem(\"RDL Text\", null, new EventHandler(this.menuViewRDL_Click), (Keys.Shift|Keys.F7));");
			StringAssert.Contains(source,"menuCascade = new ToolStripMenuItem(\"&Cascade\", null, new EventHandler(this.menuWndCascade_Click), (Keys.Control|Keys.Shift|Keys.J));");
		}
	}
}
