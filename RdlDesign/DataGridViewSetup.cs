// S3a-M3 (net10 plan): the WinForms DataGrid of the report designer's dialogs became a DataGridView. The DataGrid's current row and its
// table style's AllowSorting have no member of the same name on a DataGridView; these helpers keep the dialogs' code close to what it was.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Data;
using System.Windows.Forms;

namespace fyiReporting.RdlDesign
{
	/// <summary>Helpers for the DataGridViews that replaced the WinForms DataGrid in the designer's dialogs (S3a-M3).</summary>
	internal static class DataGridViewSetup
	{
		/// <summary>The index of the current row, as DataGrid.CurrentRowIndex gave it: -1 without a current cell. The new row at the end (where a
		/// row is typed in) has the index Rows.Count-1; IsNewRow tells it apart.</summary>
		internal static int GetCurrentRowIndex(DataGridView dataGridView)
		{
			if (dataGridView.CurrentCell == null)
				return -1;
			return dataGridView.CurrentCell.RowIndex;
		}

		/// <summary>Makes a row the current one, in the current column (the first column without a current cell), as setting
		/// DataGrid.CurrentRowIndex did. An index outside the rows is ignored.</summary>
		internal static void SetCurrentRowIndex(DataGridView dataGridView, int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= dataGridView.Rows.Count || dataGridView.Columns.Count == 0)
				return;
			int columnIndex = dataGridView.CurrentCell == null ? 0 : dataGridView.CurrentCell.ColumnIndex;
			dataGridView.CurrentCell = dataGridView.Rows[rowIndex].Cells[columnIndex];
		}

		/// <summary>True if the current row is a row of the data (not the new row at the end).</summary>
		internal static bool IsCurrentRowData(DataGridView dataGridView)
		{
			int rowIndex = GetCurrentRowIndex(dataGridView);
			return rowIndex >= 0 && !dataGridView.Rows[rowIndex].IsNewRow;
		}

		/// <summary>The columns cannot be sorted by a click on their heading (the table style's AllowSorting = false).</summary>
		internal static void SetNotSortable(DataGridView dataGridView)
		{
			foreach (DataGridViewColumn dataGridViewColumn in dataGridView.Columns)
				dataGridViewColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
		}

		/// <summary>Ends the edit of the current cell and the current row, so that what was typed is in the bound DataTable (a row typed into the
		/// new row is added to it). The dialogs call this before they read their tables, as the DataGrid's edit ended when it lost the focus.</summary>
		internal static void CommitEdits(DataGridView dataGridView)
		{
			if (dataGridView == null)
				return;
			if (!dataGridView.IsCurrentCellDirty && !dataGridView.IsCurrentRowDirty)
				return;		// nothing typed: an untouched new row is not added
			if (!dataGridView.EndEdit())
				return;		// the cell's value is not valid; the cell keeps the edit
			if (dataGridView.DataSource == null || dataGridView.BindingContext == null)
				return;
			BindingManagerBase bindingManagerBase = dataGridView.BindingContext[dataGridView.DataSource, dataGridView.DataMember];
			if (bindingManagerBase != null)
				bindingManagerBase.EndCurrentEdit();
		}

		/// <summary>Ends the edit when the grid loses the focus (to another control of the dialog, its OK button among them), as the DataGrid did
		/// (it ended the current row's edit in its OnLeave).</summary>
		internal static void CommitOnLeave(DataGridView dataGridView)
		{
			dataGridView.Leave += (sender, e) => CommitEdits(dataGridView);
		}

		/// <summary>A text cell bound to a string column that is emptied stores "" in the DataTable, as the DataGrid's text column did (its
		/// NullText was "(null)", so an empty text was the string ""). A DataGridView parses an empty text to its null value, DBNull by default;
		/// this sets the null value of the text and combo box columns bound to a string column to "". The cells of a new row that are not typed
		/// into are not parsed, so they stay DBNull as before. The columns are set now and again whenever a column is added, a column's
		/// DataPropertyName changes or the grid gets another DataSource, so the order of the dialog's set-up does not matter.</summary>
		internal static void KeepEmptyText(DataGridView dataGridView)
		{
			SetEmptyTextNullValues(dataGridView);
			dataGridView.ColumnAdded += (sender, e) => SetEmptyTextNullValues(dataGridView);
			dataGridView.ColumnDataPropertyNameChanged += (sender, e) => SetEmptyTextNullValues(dataGridView);
			dataGridView.DataSourceChanged += (sender, e) => SetEmptyTextNullValues(dataGridView);
		}

		private static void SetEmptyTextNullValues(DataGridView dataGridView)
		{
			DataTable dataTable = dataGridView.DataSource as DataTable;
			if (dataTable == null && dataGridView.DataSource is DataView)
				dataTable = ((DataView) dataGridView.DataSource).Table;
			if (dataTable == null)
				return;		// not bound yet: DataSourceChanged sets the columns
			foreach (DataGridViewColumn dataGridViewColumn in dataGridView.Columns)
			{
				if (!(dataGridViewColumn is DataGridViewTextBoxColumn) && !(dataGridViewColumn is DGCBColumn))
					continue;
				if (string.IsNullOrEmpty(dataGridViewColumn.DataPropertyName))
					continue;
				DataColumn dataColumn = dataTable.Columns[dataGridViewColumn.DataPropertyName];
				if (dataColumn == null || dataColumn.DataType != typeof(string))
					continue;
				if ("".Equals(dataGridViewColumn.DefaultCellStyle.DataSourceNullValue))
					continue;
				dataGridViewColumn.DefaultCellStyle.DataSourceNullValue = "";
			}
		}
	}
}
