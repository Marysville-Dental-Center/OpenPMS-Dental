/* ====================================================================
    Copyright (C) 2004-2006  fyiReporting Software, LLC

    This file is part of the fyiReporting RDL project.
	
    The RDL project is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

    For additional information, email info@fyireporting.com or visit
    the website www.fyiReporting.com.
*/

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace fyiReporting.RdlDesign
{
	/// <summary>
	/// A grid column whose cells are edited with a combo box: a drop-down list, or an editable combo box (ComboBoxStyle.DropDown) where any
	/// text may be typed. CB holds the items and the style. S3a-M3: it was a column style of the WinForms DataGrid (a DataGridColumnStyle that
	/// moved a ComboBox over the cell being edited); it is now a DataGridView column with a cell whose editing control is that combo box. The
	/// cell's value is the text; an emptied cell stores "" in a string column (DataGridViewSetup.KeepEmptyText), as with the DataGrid.
	/// </summary>
	internal class DGCBColumn : DataGridViewColumn
	{
		private ComboBox _cb = new ComboBox();

		public DGCBColumn() : this(ComboBoxStyle.DropDownList)
		{
		}

		public DGCBColumn(ComboBoxStyle cbs) : base(new DGCBCell())
		{
			_cb.Visible = false;
			_cb.DropDownStyle = cbs;
		}

		/// <summary>The combo box whose Items and DropDownStyle the editing combo box of a cell takes when the cell is edited.</summary>
		internal ComboBox CB
		{
			get {return _cb;}
		}

		public override DataGridViewCell CellTemplate
		{
			get {return base.CellTemplate;}
			set
			{
				if (value != null && !(value is DGCBCell))
					throw new InvalidCastException("The cell template of a DGCBColumn must be a DGCBCell.");
				base.CellTemplate = value;
			}
		}

		public override object Clone()
		{
			DGCBColumn dgcbColumn = (DGCBColumn) base.Clone();
			dgcbColumn._cb.DropDownStyle = _cb.DropDownStyle;
			foreach (object item in _cb.Items)
				dgcbColumn._cb.Items.Add(item);
			return dgcbColumn;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_cb.Dispose();
			base.Dispose(disposing);
		}
	}

	/// <summary>A cell of a DGCBColumn: shown as its text, edited with a DGCBEditingControl.</summary>
	internal class DGCBCell : DataGridViewTextBoxCell
	{
		public override Type EditType
		{
			get {return typeof(DGCBEditingControl);}
		}

		public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
		{
			DGCBEditingControl editingControl = DataGridView.EditingControl as DGCBEditingControl;
			if (editingControl == null)
			{
				base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
				return;
			}
			DGCBColumn dgcbColumn = OwningColumn as DGCBColumn;
			editingControl.IsInitializing = true;
			try
			{
				base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
				editingControl.Items.Clear();
				editingControl.DropDownStyle = dgcbColumn == null? ComboBoxStyle.DropDownList: dgcbColumn.CB.DropDownStyle;
				if (dgcbColumn != null)
				{
					foreach (object item in dgcbColumn.CB.Items)
						editingControl.Items.Add(item);
				}
				editingControl.Text = initialFormattedValue as string ?? "";
				if (editingControl.DropDownStyle != ComboBoxStyle.DropDownList)
					editingControl.Select(editingControl.Text.Length, 0);
			}
			finally
			{
				editingControl.IsInitializing = false;
			}
		}
	}

	/// <summary>The combo box that edits a DGCBCell.</summary>
	internal class DGCBEditingControl : ComboBox, IDataGridViewEditingControl
	{
		/// <summary>True while the cell fills the combo box, so that filling it does not mark the cell as changed.</summary>
		internal bool IsInitializing;

		public DGCBEditingControl()
		{
			TabStop = false;
		}

		public DataGridView EditingControlDataGridView { get; set; }

		public object EditingControlFormattedValue
		{
			get {return Text;}
			set {Text = value as string ?? "";}
		}

		public int EditingControlRowIndex { get; set; }

		public bool EditingControlValueChanged { get; set; }

		public Cursor EditingPanelCursor
		{
			get {return Cursors.Default;}
		}

		public bool RepositionEditingControlOnValueChange
		{
			get {return false;}
		}

		public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
		{
			Font = dataGridViewCellStyle.Font;
			ForeColor = dataGridViewCellStyle.ForeColor;
			BackColor = dataGridViewCellStyle.BackColor;
		}

		public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
		{
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Down:
				case Keys.Up:
				case Keys.PageDown:
				case Keys.PageUp:
					return true;		// the combo box moves through its items
				case Keys.Left:
				case Keys.Right:
				case Keys.Home:
				case Keys.End:
					return DropDownStyle != ComboBoxStyle.DropDownList;		// the caret, in an editable combo box
				case Keys.Enter:
				case Keys.Escape:
					return DroppedDown;		// closes the list first
			}
			return !dataGridViewWantsInputKey;
		}

		public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
		{
			return EditingControlFormattedValue;
		}

		public void PrepareEditingControlForEdit(bool selectAll)
		{
			if (selectAll && DropDownStyle != ComboBoxStyle.DropDownList)
				SelectAll();
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			NotifyValueChanged();
		}

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			base.OnSelectedIndexChanged(e);
			NotifyValueChanged();
		}

		private void NotifyValueChanged()
		{
			if (IsInitializing || EditingControlDataGridView == null)
				return;
			EditingControlValueChanged = true;
			EditingControlDataGridView.NotifyCurrentCellDirty(true);
		}
	}
}
