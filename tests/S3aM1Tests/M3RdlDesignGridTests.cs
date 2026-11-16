// Package S3a-M3a: the report designer's grids, WinForms DataGrids before, are DataGridViews bound to the same DataTables. The designer (RdlDesign,
// RdlDesigner.dll, the managed assembly beside its apphost RdlDesigner.exe on .NET; S3b-2's handoff, S3b-5) is loaded from its build folder,
// since no test project references it; its classes are internal and reached by reflection.
// The dialogs are built in memory and never shown (their handles are created for the grids' editing). Checked: the columns (names, headings,
// widths, read-only, not sortable), an edit typed into a cell that reaches the table when the grid loses the focus (as the DataGrid's edit did),
// an untouched new row that does not, the dialog's own validation after the edit, the combo box column DGCBColumn (items, style, and a drop-down
// list that takes only its items), an emptied cell that stores "" as the DataGrid did (QueryParametersCtl with DataSetsCtl.Apply, and
// DialogValidValues), the main menu's shortcut lookup that raises the menus' Opening handlers first, and (CLEAN-1) Up and Down on the sort
// and group rows, and a new sort row that is Ascending.
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

		///<summary>RdlDesigner.dll from its build folder (Debug, else any configuration): on .NET the managed assembly is the .dll, and
		///RdlDesigner.exe is the native apphost, which Assembly.LoadFrom refuses (BadImageFormatException). Inconclusive when it has not been built.
		///</summary>
		private static Assembly GetRdlDesign() {
			if(_assemblyRdlDesign!=null) {
				return _assemblyRdlDesign;
			}
			string pathBin=M2Support.RepoFile("RdlDesign","bin");
			string path=Path.Combine(pathBin,"Debug","RdlDesigner.dll");
			if(!File.Exists(path) && Directory.Exists(pathBin)) {
				path=Directory.EnumerateFiles(pathBin,"RdlDesigner.dll",SearchOption.AllDirectories).FirstOrDefault()??path;
			}
			if(!File.Exists(path)) {
				Assert.Inconclusive("RdlDesigner.dll is not built ("+path+"); build ReportDesigner first.");
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

		///<summary>A minimal report (no namespace) with one data source and one data set that has one query parameter and one field.</summary>
		private const string REPORT_QUERY_PARAMETER=
			"<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
			+"<Report>"
			+"<DataSources><DataSource Name=\"DS1\"><ConnectionProperties><DataProvider>SQL</DataProvider><ConnectString></ConnectString>"
			+"</ConnectionProperties></DataSource></DataSources>"
			+"<DataSets><DataSet Name=\"Data\"><Query><DataSourceName>DS1</DataSourceName><CommandText>SELECT 1</CommandText>"
			+"<QueryParameters><QueryParameter Name=\"@Clinic\"><Value>=Parameters!Clinic.Value</Value></QueryParameter></QueryParameters>"
			+"</Query><Fields><Field Name=\"A\"><DataField>A</DataField></Field></Fields></DataSet></DataSets>"
			+"<Body><ReportItems></ReportItems><Height>1in</Height></Body>"
			+"<Width>8.5in</Width>"
			+"</Report>";

		[TestMethod]
		public void QueryParametersCtl_EmptiedValue_StoresEmptyString_DataSetsCtlApplyWritesIt() {
			M2Support.RunSta(() => {
				System.Xml.XmlDocument xmlDocument=new System.Xml.XmlDocument();
				xmlDocument.LoadXml(REPORT_QUERY_PARAMETER);
				using(UserControl draw=(UserControl)Activator.CreateInstance(GetType("DesignXmlDraw"),nonPublic:true))//its static constructor makes Create ambiguous
				using(Form form=new Form()) {
					Priv.Set(draw,"ReportDocument",xmlDocument);
					System.Xml.XmlNode nodeDataSet=xmlDocument.SelectSingleNode("/Report/DataSets/DataSet");
					UserControl dataSetsCtl=(UserControl)Create("DataSetsCtl",draw,nodeDataSet);
					UserControl queryParametersCtl=(UserControl)Create("QueryParametersCtl",draw,Priv.Get(dataSetsCtl,"DSV"));
					form.Controls.Add(dataSetsCtl);//disposed with the form
					form.Controls.Add(queryParametersCtl);
					IntPtr handle=form.Handle;
					DataGridView dataGridView=(DataGridView)Priv.Get(queryParametersCtl,"dgParms");
					IntPtr handleGrid=dataGridView.Handle;
					DataTable dataTable=(DataTable)Priv.Get(Priv.Get(dataSetsCtl,"DSV"),"QueryParameters");
					Assert.AreEqual("=Parameters!Clinic.Value",dataTable.Rows[0]["Value"]);
					//The text columns bound to the table's string columns store "" for an emptied cell, as the DataGrid did.
					Assert.IsTrue(dataGridView.Columns.Cast<DataGridViewColumn>().All(x => "".Equals(x.DefaultCellStyle.DataSourceNullValue)),
						"Each string column's null value is \"\".");
					//The user clears the parameter's value and the grid loses the focus (OK or Apply).
					TypeInto(dataGridView,0,1,"");
					Priv.Call(dataGridView,"OnLeave",EventArgs.Empty);
					Assert.AreEqual("",dataTable.Rows[0]["Value"],"The emptied value is \"\", not DBNull.");
					//A parameter typed into the new row with only its name: the value cell was not typed into and stays DBNull.
					TypeInto(dataGridView,1,0,"@Other");
					Priv.Call(dataGridView,"OnLeave",EventArgs.Empty);
					Assert.AreEqual(2,dataTable.Rows.Count);
					Assert.AreEqual("@Other",dataTable.Rows[1]["Name"]);
					Assert.AreEqual(DBNull.Value,dataTable.Rows[1]["Value"],"An untouched cell of the new row stays DBNull.");
					//Apply writes both parameters with an empty Value and does not throw (the emptied value threw an InvalidCastException; the
					//row with only a name threw on main too).
					Priv.Call(dataSetsCtl,"Apply");
					System.Xml.XmlNodeList nodeListParameters=xmlDocument.SelectNodes("/Report/DataSets/DataSet/Query/QueryParameters/QueryParameter");
					Assert.AreEqual(2,nodeListParameters.Count);
					Assert.AreEqual("@Clinic",nodeListParameters[0].Attributes["Name"].Value);
					Assert.AreEqual("",nodeListParameters[0].SelectSingleNode("Value").InnerText);
					Assert.AreEqual("@Other",nodeListParameters[1].Attributes["Name"].Value);
					Assert.AreEqual("",nodeListParameters[1].SelectSingleNode("Value").InnerText);
					//The fields grid is set the same way: an emptied field name is "", not DBNull.
					DataGridView dataGridViewFields=(DataGridView)Priv.Get(dataSetsCtl,"dgFields");
					Assert.IsTrue(dataGridViewFields.Columns.Cast<DataGridViewColumn>().All(x => "".Equals(x.DefaultCellStyle.DataSourceNullValue)));
					dataGridView.CurrentCell=null;//leaves the cell before the form and its grids are disposed
				}
			});
		}

		///<summary>A minimal report (no namespace) with one data set and a list that groups by two expressions and sorts by two, the first
		///descending.</summary>
		private const string REPORT_LIST_GROUPING_SORTING=
			"<?xml version=\"1.0\" encoding=\"UTF-8\"?>"
			+"<Report>"
			+"<DataSources><DataSource Name=\"DS1\"><ConnectionProperties><DataProvider>SQL</DataProvider><ConnectString></ConnectString>"
			+"</ConnectionProperties></DataSource></DataSources>"
			+"<DataSets><DataSet Name=\"Data\"><Query><DataSourceName>DS1</DataSourceName><CommandText>SELECT 1</CommandText></Query>"
			+"<Fields><Field Name=\"A\"><DataField>A</DataField></Field><Field Name=\"B\"><DataField>B</DataField></Field></Fields></DataSet></DataSets>"
			+"<Body><ReportItems><List Name=\"List1\"><DataSetName>Data</DataSetName>"
			+"<Grouping Name=\"Group1\"><GroupExpressions><GroupExpression>=Fields!A.Value</GroupExpression>"
			+"<GroupExpression>=Fields!B.Value</GroupExpression></GroupExpressions></Grouping>"
			+"<Sorting><SortBy><SortExpression>=Fields!A.Value</SortExpression><Direction>Descending</Direction></SortBy>"
			+"<SortBy><SortExpression>=Fields!B.Value</SortExpression><Direction>Ascending</Direction></SortBy></Sorting>"
			+"<ReportItems></ReportItems></List></ReportItems><Height>1in</Height></Body>"
			+"<Width>8.5in</Width>"
			+"</Report>";

		///<summary>CLEAN-1 (S3a-M3's lead note): Up and Down on the sort rows swap the rows' two columns; they threw before, since SwapRow also
		///swapped a third column the table does not have. Up on the new row at the end does nothing (it threw too). A sort row typed into the
		///new row with only an expression is Ascending and shows checked, as Apply writes it (it showed unchecked, as if descending).</summary>
		[TestMethod]
		public void SortingCtl_UpDown_SwapTheRows_NewRowIsAscending() {
			M2Support.RunSta(() => {
				System.Xml.XmlDocument xmlDocument=new System.Xml.XmlDocument();
				xmlDocument.LoadXml(REPORT_LIST_GROUPING_SORTING);
				using(UserControl draw=(UserControl)Activator.CreateInstance(GetType("DesignXmlDraw"),nonPublic:true))//its static constructor makes Create ambiguous
				using(Form form=new Form()) {
					Priv.Set(draw,"ReportDocument",xmlDocument);
					System.Xml.XmlNode nodeList=xmlDocument.SelectSingleNode("/Report/Body/ReportItems/List");
					UserControl sortingCtl=(UserControl)Create("SortingCtl",draw,nodeList);
					form.Controls.Add(sortingCtl);//disposed with the form
					IntPtr handle=form.Handle;
					DataGridView dataGridView=(DataGridView)Priv.Get(sortingCtl,"dgSorting");
					IntPtr handleGrid=dataGridView.Handle;
					DataTable dataTable=(DataTable)Priv.Get(sortingCtl,"_DataTable");
					Assert.AreEqual(2,dataTable.Rows.Count);
					//Up on the second row: the rows trade places, both columns.
					DataGridViewSetupSetCurrent(dataGridView,1);
					Priv.Call(sortingCtl,"bUp_Click",sortingCtl,EventArgs.Empty);
					Assert.AreEqual("=Fields!B.Value",dataTable.Rows[0]["SortExpression"]);
					Assert.AreEqual(true,dataTable.Rows[0]["Direction"]);
					Assert.AreEqual("=Fields!A.Value",dataTable.Rows[1]["SortExpression"]);
					Assert.AreEqual(false,dataTable.Rows[1]["Direction"]);
					Assert.AreEqual(0,(int)CallStatic("DataGridViewSetup","GetCurrentRowIndex",dataGridView),"The moved row stays current.");
					//Down moves it back.
					Priv.Call(sortingCtl,"bDown_Click",sortingCtl,EventArgs.Empty);
					Assert.AreEqual("=Fields!A.Value",dataTable.Rows[0]["SortExpression"]);
					Assert.AreEqual(false,dataTable.Rows[0]["Direction"]);
					Assert.AreEqual("=Fields!B.Value",dataTable.Rows[1]["SortExpression"]);
					Assert.AreEqual(1,(int)CallStatic("DataGridViewSetup","GetCurrentRowIndex",dataGridView));
					//Up and Down on the new row at the end change nothing.
					DataGridViewSetupSetCurrent(dataGridView,2);
					Priv.Call(sortingCtl,"bUp_Click",sortingCtl,EventArgs.Empty);
					Priv.Call(sortingCtl,"bDown_Click",sortingCtl,EventArgs.Empty);
					Assert.AreEqual(2,dataTable.Rows.Count);
					Assert.AreEqual("=Fields!A.Value",dataTable.Rows[0]["SortExpression"]);
					Assert.AreEqual("=Fields!B.Value",dataTable.Rows[1]["SortExpression"]);
					//A sort row typed into the new row with only its expression is Ascending, and its box is checked.
					TypeInto(dataGridView,2,0,"=Fields!C.Value");
					Priv.Call(dataGridView,"OnLeave",EventArgs.Empty);
					Assert.AreEqual(3,dataTable.Rows.Count);
					Assert.AreEqual(true,dataTable.Rows[2]["Direction"],"A new sort row is Ascending.");
					Assert.AreEqual(true,dataGridView.Rows[2].Cells[1].Value,"Its Sort Ascending box shows checked.");
					//Apply writes the three rows in their order, with their directions.
					Priv.Call(sortingCtl,"Apply");
					System.Xml.XmlNodeList nodeListSortBy=xmlDocument.SelectNodes("/Report/Body/ReportItems/List/Sorting/SortBy");
					CollectionAssert.AreEqual(new[] { "=Fields!A.Value","=Fields!B.Value","=Fields!C.Value" },
						nodeListSortBy.Cast<System.Xml.XmlNode>().Select(x => x.SelectSingleNode("SortExpression").InnerText).ToArray());
					CollectionAssert.AreEqual(new[] { "Descending","Ascending","Ascending" },
						nodeListSortBy.Cast<System.Xml.XmlNode>().Select(x => x.SelectSingleNode("Direction").InnerText).ToArray());
					dataGridView.CurrentCell=null;//leaves the cell before the form and its grids are disposed
				}
			});
		}

		///<summary>CLEAN-1 (S3a-M3's note for the lead): Up and Down on the group expressions swap the rows; they threw before, since SwapRow also
		///swapped a second and a third column the one-column table does not have. Up on the new row at the end does nothing.</summary>
		[TestMethod]
		public void GroupingCtl_UpDown_SwapTheRows() {
			M2Support.RunSta(() => {
				System.Xml.XmlDocument xmlDocument=new System.Xml.XmlDocument();
				xmlDocument.LoadXml(REPORT_LIST_GROUPING_SORTING);
				using(UserControl draw=(UserControl)Activator.CreateInstance(GetType("DesignXmlDraw"),nonPublic:true))//its static constructor makes Create ambiguous
				using(Form form=new Form()) {
					Priv.Set(draw,"ReportDocument",xmlDocument);
					System.Xml.XmlNode nodeList=xmlDocument.SelectSingleNode("/Report/Body/ReportItems/List");
					UserControl groupingCtl=(UserControl)Create("GroupingCtl",draw,nodeList);
					form.Controls.Add(groupingCtl);//disposed with the form
					IntPtr handle=form.Handle;
					DataGridView dataGridView=(DataGridView)Priv.Get(groupingCtl,"dgGroup");
					IntPtr handleGrid=dataGridView.Handle;
					DataTable dataTable=(DataTable)Priv.Get(groupingCtl,"_DataTable");
					Assert.AreEqual(2,dataTable.Rows.Count);
					DataGridViewSetupSetCurrent(dataGridView,1);
					Priv.Call(groupingCtl,"bUp_Click",groupingCtl,EventArgs.Empty);
					CollectionAssert.AreEqual(new[] { "=Fields!B.Value","=Fields!A.Value" },dataTable.Rows.Cast<DataRow>().Select(x => (string)x[0]).ToArray());
					Assert.AreEqual(0,(int)CallStatic("DataGridViewSetup","GetCurrentRowIndex",dataGridView),"The moved row stays current.");
					Priv.Call(groupingCtl,"bDown_Click",groupingCtl,EventArgs.Empty);
					CollectionAssert.AreEqual(new[] { "=Fields!A.Value","=Fields!B.Value" },dataTable.Rows.Cast<DataRow>().Select(x => (string)x[0]).ToArray());
					DataGridViewSetupSetCurrent(dataGridView,2);
					Priv.Call(groupingCtl,"bUp_Click",groupingCtl,EventArgs.Empty);
					Priv.Call(groupingCtl,"bDown_Click",groupingCtl,EventArgs.Empty);
					CollectionAssert.AreEqual(new[] { "=Fields!A.Value","=Fields!B.Value" },dataTable.Rows.Cast<DataRow>().Select(x => (string)x[0]).ToArray());
					dataGridView.CurrentCell=null;//leaves the cell before the form and its grids are disposed
				}
			});
		}

		[TestMethod]
		public void DialogValidValues_EmptiedLabel_IsEmptyString() {
			M2Support.RunSta(() => {
				object item=Create("ParameterValueItem");
				Priv.Set(item,"Value","North");
				Priv.Set(item,"Label","N");
				IList list=MakeList("ParameterValueItem",item);
				using(Form form=(Form)Create("DialogValidValues",new object[] { list })) {
					IntPtr handle=form.Handle;
					DataGridView dataGridView=(DataGridView)Priv.Get(form,"dgParms");
					IntPtr handleGrid=dataGridView.Handle;
					DataTable dataTable=(DataTable)Priv.Get(form,"_DataTable");
					TypeInto(dataGridView,0,1,"");
					Priv.Call(dataGridView,"OnLeave",EventArgs.Empty);
					Assert.AreEqual("",dataTable.Rows[0]["Label"]);
					object itemOut=((IEnumerable)Priv.Get(form,"ValidValues")).Cast<object>().Single();
					Assert.AreEqual("North",Priv.Get(itemOut,"Value"));
					Assert.AreEqual("",Priv.Get(itemOut,"Label"),"An emptied label is \"\" (the DataGrid's), not null.");
					dataGridView.CurrentCell=null;
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
