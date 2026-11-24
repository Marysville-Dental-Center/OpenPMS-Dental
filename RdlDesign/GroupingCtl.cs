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
using System.Xml;
using System.Text;

namespace fyiReporting.RdlDesign
{
	/// <summary>
	/// Grouping specification: used for DataRegions (List, Chart, Table, Matrix), DataSets, group instances
	/// </summary>
	internal class GroupingCtl : System.Windows.Forms.UserControl, IProperty
	{
		private DesignXmlDraw _Draw;
		private XmlNode _GroupingParent;
		private DataTable _DataTable;
//		private DGCBColumn dgtbGE;
		private DataGridViewTextBoxColumn dgtbGE;

		private System.Windows.Forms.Button bDelete;
		private System.Windows.Forms.Button bUp;
		private System.Windows.Forms.Button bDown;
		private System.Windows.Forms.DataGridView dgGroup;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cbLabelExpr;
		private System.Windows.Forms.ComboBox cbParentExpr;
		private System.Windows.Forms.CheckBox chkPBS;
		private System.Windows.Forms.CheckBox chkPBE;
		private System.Windows.Forms.CheckBox chkRepeatHeader;
		private System.Windows.Forms.CheckBox chkGrpHeader;
		private System.Windows.Forms.CheckBox chkRepeatFooter;
		private System.Windows.Forms.CheckBox chkGrpFooter;
		private System.Windows.Forms.Label lParent;
		private System.Windows.Forms.Button bValueExpr;
		private System.Windows.Forms.Button bLabelExpr;
		private System.Windows.Forms.Button bParentExpr;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		internal GroupingCtl(DesignXmlDraw dxDraw, XmlNode groupingParent)
		{
			_Draw = dxDraw;
			_GroupingParent = groupingParent;
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// Initialize form using the style node values
			InitValues();			
		}

		private void InitValues()
		{
			// Initialize the DataGrid columns
			//			dgtbGE = new DGCBColumn(ComboBoxStyle.DropDown);
			dgtbGE = new DataGridViewTextBoxColumn();

			this.dgGroup.Columns.AddRange(new DataGridViewColumn[] {
															this.dgtbGE});
			// 
			// dgtbGE
			// 
			dgtbGE.HeaderText = "Expression";
			dgtbGE.DataPropertyName = "Expression";
			dgtbGE.Width = 175;
			// Get the parent's dataset name
//			string dataSetName = _Draw.GetDataSetNameValue(_GroupingParent);
//
//			string[] fields = _Draw.GetFields(dataSetName, true);
//			if (fields != null)
//				dgtbGE.CB.Items.AddRange(fields);

			// Initialize the DataTable
			_DataTable = new DataTable();
			_DataTable.Columns.Add(new DataColumn("Expression", typeof(string)));

			string[] rowValues = new string[1];
			XmlNode grouping = _Draw.GetNamedChildNode(_GroupingParent, "Grouping");

			// Handle the group expressions
			XmlNode groupingExs = _Draw.GetNamedChildNode(grouping, "GroupExpressions");

			if (groupingExs != null)
			foreach (XmlNode gNode in groupingExs.ChildNodes)
			{
				if (gNode.NodeType != XmlNodeType.Element || 
						gNode.Name != "GroupExpression")
					continue;
				rowValues[0] = gNode.InnerText;

				_DataTable.Rows.Add(rowValues);
			}
			this.dgGroup.DataSource = _DataTable;
		//	ts.PreferredRowHeight = dgtbGE.CB.Height;
			dgGroup.Columns[0].Width = 330;
			DataGridViewSetup.SetNotSortable(this.dgGroup);
			DataGridViewSetup.CommitOnLeave(this.dgGroup);
			DataGridViewSetup.KeepEmptyText(this.dgGroup);

			//
			if (grouping == null)
			{
				this.tbName.Text = "";
				this.cbParentExpr.Text =  "";
				this.cbLabelExpr.Text =  "";
			}
			else
			{
				this.chkPBE.Checked = _Draw.GetElementValue(grouping, "PageBreakAtEnd", "false").ToLower() == "true";
				this.chkPBS.Checked = _Draw.GetElementValue(grouping, "PageBreakAtStart", "false").ToLower() == "true";

				this.tbName.Text = _Draw.GetElementAttribute(grouping, "Name", "");
				this.cbParentExpr.Text =  _Draw.GetElementValue(grouping, "Parent", "");
				this.cbLabelExpr.Text =  _Draw.GetElementValue(grouping, "Label", "");
			}

			if (_GroupingParent.Name == "TableGroup")
			{
				XmlNode repeat;
				repeat = DesignXmlDraw.FindNextInHierarchy(_GroupingParent, "Header", "RepeatOnNewPage");
				if (repeat != null)
					this.chkRepeatHeader.Checked = repeat.InnerText.ToLower() == "true";
				repeat = DesignXmlDraw.FindNextInHierarchy(_GroupingParent, "Footer", "RepeatOnNewPage");
				if (repeat != null)
					this.chkRepeatFooter.Checked = repeat.InnerText.ToLower() == "true";
				this.chkGrpHeader.Checked = _Draw.GetNamedChildNode(_GroupingParent, "Header") != null;
				this.chkGrpFooter.Checked = _Draw.GetNamedChildNode(_GroupingParent, "Footer") != null;
			}
			else
			{
				this.chkRepeatFooter.Visible = false;
				this.chkRepeatHeader.Visible = false;
				this.chkGrpFooter.Visible = false;
				this.chkGrpHeader.Visible = false;
			}
			if (_GroupingParent.Name == "DynamicColumns" ||
				_GroupingParent.Name == "DynamicRows")
			{
				this.chkPBE.Visible = this.chkPBS.Visible = false;
			}
			else if (_GroupingParent.Name == "DynamicSeries" ||
				_GroupingParent.Name == "DynamicCategories")
			{
				this.chkPBE.Visible = this.chkPBS.Visible = false;
				this.cbParentExpr.Visible = this.lParent.Visible = false;
				this.cbLabelExpr.Text =  _Draw.GetElementValue(_GroupingParent, "Label", "");
			}

			// load label and parent controls with fields
			string dsn = _Draw.GetDataSetNameValue(_GroupingParent);
			if (dsn != null)	// found it
			{
				string[] f = _Draw.GetFields(dsn, true);
				this.cbParentExpr.Items.AddRange(f);
				this.cbLabelExpr.Items.AddRange(f);
			}

		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.dgGroup = new System.Windows.Forms.DataGridView();
			this.bDelete = new System.Windows.Forms.Button();
			this.bUp = new System.Windows.Forms.Button();
			this.bDown = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.tbName = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.cbLabelExpr = new System.Windows.Forms.ComboBox();
			this.cbParentExpr = new System.Windows.Forms.ComboBox();
			this.lParent = new System.Windows.Forms.Label();
			this.chkPBS = new System.Windows.Forms.CheckBox();
			this.chkPBE = new System.Windows.Forms.CheckBox();
			this.chkRepeatHeader = new System.Windows.Forms.CheckBox();
			this.chkGrpHeader = new System.Windows.Forms.CheckBox();
			this.chkRepeatFooter = new System.Windows.Forms.CheckBox();
			this.chkGrpFooter = new System.Windows.Forms.CheckBox();
			this.bValueExpr = new System.Windows.Forms.Button();
			this.bLabelExpr = new System.Windows.Forms.Button();
			this.bParentExpr = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dgGroup)).BeginInit();
			this.SuspendLayout();
			// 
			// dgGroup
			// 
			this.dgGroup.AutoGenerateColumns = false;
			this.dgGroup.DataMember = "";
			this.dgGroup.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
			this.dgGroup.Location = new System.Drawing.Point(8, 53);
			this.dgGroup.Name = "dgGroup";
			this.dgGroup.Size = new System.Drawing.Size(378, 104);
			this.dgGroup.TabIndex = 1;
			// 
			// bDelete
			// 
			this.bDelete.Location = new System.Drawing.Point(392, 80);
			this.bDelete.Name = "bDelete";
			this.bDelete.Size = new System.Drawing.Size(48, 23);
			this.bDelete.TabIndex = 2;
			this.bDelete.Text = "Delete";
			this.bDelete.Click += new System.EventHandler(this.bDelete_Click);
			// 
			// bUp
			// 
			this.bUp.Location = new System.Drawing.Point(392, 107);
			this.bUp.Name = "bUp";
			this.bUp.Size = new System.Drawing.Size(48, 23);
			this.bUp.TabIndex = 3;
			this.bUp.Text = "Up";
			this.bUp.Click += new System.EventHandler(this.bUp_Click);
			// 
			// bDown
			// 
			this.bDown.Location = new System.Drawing.Point(392, 134);
			this.bDown.Name = "bDown";
			this.bDown.Size = new System.Drawing.Size(48, 23);
			this.bDown.TabIndex = 4;
			this.bDown.Text = "Down";
			this.bDown.Click += new System.EventHandler(this.bDown_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 8);
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(44, 23);
			this.label1.TabIndex = 5;
			this.label1.Text = "Name";
			// 
			// tbName
			// 
			this.tbName.Location = new System.Drawing.Point(62, 8);
			this.tbName.Name = "tbName";
			this.tbName.Size = new System.Drawing.Size(324, 23);
			this.tbName.TabIndex = 0;
			this.tbName.Text = "textBox1";
			this.tbName.Validating += new System.ComponentModel.CancelEventHandler(this.tbName_Validating);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 165);
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 23);
			this.label2.TabIndex = 7;
			this.label2.Text = "Label";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 35);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(60, 16);
			this.label3.TabIndex = 9;
			this.label3.Text = "Group By";
			// 
			// cbLabelExpr
			// 
			this.cbLabelExpr.Location = new System.Drawing.Point(62, 165);
			this.cbLabelExpr.Name = "cbLabelExpr";
			this.cbLabelExpr.Size = new System.Drawing.Size(324, 23);
			this.cbLabelExpr.TabIndex = 5;
			this.cbLabelExpr.Text = "comboBox1";
			// 
			// cbParentExpr
			// 
			this.cbParentExpr.Location = new System.Drawing.Point(62, 192);
			this.cbParentExpr.Name = "cbParentExpr";
			this.cbParentExpr.Size = new System.Drawing.Size(324, 23);
			this.cbParentExpr.TabIndex = 6;
			this.cbParentExpr.Text = "comboBox1";
			// 
			// lParent
			// 
			this.lParent.Location = new System.Drawing.Point(8, 192);
			this.lParent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lParent.Name = "lParent";
			this.lParent.Size = new System.Drawing.Size(48, 23);
			this.lParent.TabIndex = 11;
			this.lParent.Text = "Parent";
			// 
			// chkPBS
			// 
			this.chkPBS.Location = new System.Drawing.Point(62, 219);
			this.chkPBS.Name = "chkPBS";
			this.chkPBS.Size = new System.Drawing.Size(129, 18);
			this.chkPBS.TabIndex = 7;
			this.chkPBS.Text = "Page Break at Start";
			// 
			// chkPBE
			// 
			this.chkPBE.Location = new System.Drawing.Point(212, 219);
			this.chkPBE.Name = "chkPBE";
			this.chkPBE.Size = new System.Drawing.Size(125, 18);
			this.chkPBE.TabIndex = 8;
			this.chkPBE.Text = "Page Break at End";
			// 
			// chkRepeatHeader
			// 
			this.chkRepeatHeader.Location = new System.Drawing.Point(212, 241);
			this.chkRepeatHeader.Name = "chkRepeatHeader";
			this.chkRepeatHeader.Size = new System.Drawing.Size(141, 18);
			this.chkRepeatHeader.TabIndex = 13;
			this.chkRepeatHeader.Text = "Repeat group header";
			// 
			// chkGrpHeader
			// 
			this.chkGrpHeader.Location = new System.Drawing.Point(62, 241);
			this.chkGrpHeader.Name = "chkGrpHeader";
			this.chkGrpHeader.Size = new System.Drawing.Size(144, 18);
			this.chkGrpHeader.TabIndex = 12;
			this.chkGrpHeader.Text = "Include group header";
			// 
			// chkRepeatFooter
			// 
			this.chkRepeatFooter.Location = new System.Drawing.Point(212, 263);
			this.chkRepeatFooter.Name = "chkRepeatFooter";
			this.chkRepeatFooter.Size = new System.Drawing.Size(137, 18);
			this.chkRepeatFooter.TabIndex = 15;
			this.chkRepeatFooter.Text = "Repeat group footer";
			// 
			// chkGrpFooter
			// 
			this.chkGrpFooter.Location = new System.Drawing.Point(62, 263);
			this.chkGrpFooter.Name = "chkGrpFooter";
			this.chkGrpFooter.Size = new System.Drawing.Size(140, 18);
			this.chkGrpFooter.TabIndex = 14;
			this.chkGrpFooter.Text = "Include group footer";
			// 
			// bValueExpr
			// 
			this.bValueExpr.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.bValueExpr.Location = new System.Drawing.Point(392, 53);
			this.bValueExpr.Name = "bValueExpr";
			this.bValueExpr.Size = new System.Drawing.Size(22, 23);
			this.bValueExpr.TabIndex = 16;
			this.bValueExpr.Tag = "value";
			this.bValueExpr.Text = "fx";
			this.bValueExpr.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bValueExpr.Click += new System.EventHandler(this.bValueExpr_Click);
			// 
			// bLabelExpr
			// 
			this.bLabelExpr.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.bLabelExpr.Location = new System.Drawing.Point(392, 165);
			this.bLabelExpr.Name = "bLabelExpr";
			this.bLabelExpr.Size = new System.Drawing.Size(22, 23);
			this.bLabelExpr.TabIndex = 17;
			this.bLabelExpr.Tag = "label";
			this.bLabelExpr.Text = "fx";
			this.bLabelExpr.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bLabelExpr.Click += new System.EventHandler(this.bExpr_Click);
			// 
			// bParentExpr
			// 
			this.bParentExpr.Font = new System.Drawing.Font("Arial", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.bParentExpr.Location = new System.Drawing.Point(392, 192);
			this.bParentExpr.Name = "bParentExpr";
			this.bParentExpr.Size = new System.Drawing.Size(22, 23);
			this.bParentExpr.TabIndex = 18;
			this.bParentExpr.Tag = "parent";
			this.bParentExpr.Text = "fx";
			this.bParentExpr.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.bParentExpr.Click += new System.EventHandler(this.bExpr_Click);
			// 
			// GroupingCtl
			// 
			this.Controls.Add(this.bParentExpr);
			this.Controls.Add(this.bLabelExpr);
			this.Controls.Add(this.bValueExpr);
			this.Controls.Add(this.chkRepeatFooter);
			this.Controls.Add(this.chkGrpFooter);
			this.Controls.Add(this.chkRepeatHeader);
			this.Controls.Add(this.chkGrpHeader);
			this.Controls.Add(this.chkPBE);
			this.Controls.Add(this.chkPBS);
			this.Controls.Add(this.cbParentExpr);
			this.Controls.Add(this.lParent);
			this.Controls.Add(this.cbLabelExpr);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.tbName);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.bDown);
			this.Controls.Add(this.bUp);
			this.Controls.Add(this.bDelete);
			this.Controls.Add(this.dgGroup);
			this.Name = "GroupingCtl";
			this.Size = new System.Drawing.Size(488, 304);
			((System.ComponentModel.ISupportInitialize)(this.dgGroup)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		public bool IsValid()
		{
			DataGridViewSetup.CommitEdits(this.dgGroup);
			// Check to see if we have an expression
			bool bRows=HasRows();

			// If no rows and no data 
			if (!bRows && this.tbName.Text.Trim().Length == 0)
			{
				if (_GroupingParent.Name == "Details" ||
					_GroupingParent.Name == "List")
					return true;

				MessageBox.Show("Group must be defined.", "Grouping");
				return false;
			}

			// Grouping must have name
			XmlNode grouping = _Draw.GetNamedChildNode(_GroupingParent, "Grouping");
			string nerr = _Draw.GroupingNameCheck(grouping, this.tbName.Text);
			if (nerr != null)
			{
				MessageBox.Show(nerr, "Group Name in Error");
				return false;
			}

			if (!bRows)
			{
				MessageBox.Show("No expressions have been defined for the group.", "Group");
				return false;
			}

			if (_GroupingParent.Name != "DynamicSeries")
				return true;
			// DynamicSeries grouping must have a label for the legend
			if (this.cbLabelExpr.Text.Length > 0)
				return true;

			MessageBox.Show("Chart series must have label defined for the legend.", "Chart");

			return false;
		}

		private bool HasRows()
		{
			bool bRows=false;
			foreach (DataRow dr in _DataTable.Rows)
			{
				if (dr[0] == DBNull.Value)
					continue;
				string ge = (string) dr[0];
				if (ge.Length <= 0)
					continue;
				bRows = true;
				break;
			}
			return bRows;
		}

		public void Apply()
		{
			DataGridViewSetup.CommitEdits(this.dgGroup);
			if (!HasRows())		// No expressions in grouping; get rid of grouping
			{
				_Draw.RemoveElement(_GroupingParent, "Grouping");	// can't have a grouping
				return;
			}

			// Get the group
			XmlNode grouping = _Draw.GetCreateNamedChildNode(_GroupingParent, "Grouping");

			_Draw.SetGroupName(grouping, tbName.Text.Trim());
			
			// Handle the label
			if (_GroupingParent.Name == "DynamicSeries" ||
				_GroupingParent.Name == "DynamicCategories")
			{
				if (this.cbLabelExpr.Text.Length > 0)
					_Draw.SetElement(_GroupingParent, "Label", cbLabelExpr.Text);
				else
					_Draw.RemoveElement(_GroupingParent, "Label");
			}
			else
			{
				if (this.cbLabelExpr.Text.Length > 0)
					_Draw.SetElement(grouping, "Label", cbLabelExpr.Text);
				else
					_Draw.RemoveElement(grouping, "Label");

				_Draw.SetElement(grouping, "PageBreakAtStart", this.chkPBS.Checked? "true": "false");
				_Draw.SetElement(grouping, "PageBreakAtEnd", this.chkPBE.Checked? "true": "false");
				if (cbParentExpr.Text.Length > 0)
					_Draw.SetElement(grouping, "Parent", cbParentExpr.Text);
				else
					_Draw.RemoveElement(grouping, "Parent");
			}


			// Loop thru and add all the group expressions
			XmlNode grpExprs = _Draw.GetCreateNamedChildNode(grouping, "GroupExpressions");
			grpExprs.RemoveAll();
			string firstexpr=null;
			foreach (DataRow dr in _DataTable.Rows)
			{
				if (dr[0] == DBNull.Value)
					continue;
				string ge = (string) dr[0];
				if (ge.Length <= 0)
					continue;
				_Draw.CreateElement(grpExprs, "GroupExpression", ge);
				if (firstexpr == null)
					firstexpr = ge;
			}
			if (!grpExprs.HasChildNodes)
			{	// With no group expressions there are no groups
				grouping.RemoveChild(grpExprs);
				grouping.ParentNode.RemoveChild(grouping);
				grouping = null;
			}

			if (_GroupingParent.Name == "TableGroup" && grouping != null)
			{
				if (this.chkGrpHeader.Checked)
				{
					XmlNode header = _Draw.GetCreateNamedChildNode(_GroupingParent, "Header");
					_Draw.SetElement(header, "RepeatOnNewPage", chkRepeatHeader.Checked? "true": "false");
					XmlNode tblRows = _Draw.GetCreateNamedChildNode(header, "TableRows");
					if (!tblRows.HasChildNodes)
					{	// We need to create a row
						_Draw.InsertTableRow(tblRows);
					}
				}
				else
				{
					_Draw.RemoveElement(_GroupingParent, "Header");
				}

				if (this.chkGrpFooter.Checked)
				{
					XmlNode footer = _Draw.GetCreateNamedChildNode(_GroupingParent, "Footer");
					_Draw.SetElement(footer, "RepeatOnNewPage", chkRepeatFooter.Checked? "true": "false");
					XmlNode tblRows = _Draw.GetCreateNamedChildNode(footer, "TableRows");
					if (!tblRows.HasChildNodes)
					{	// We need to create a row
						_Draw.InsertTableRow(tblRows);
					}
				}
				else
				{
					_Draw.RemoveElement(_GroupingParent, "Footer");
				}
			}
			else if (_GroupingParent.Name == "DynamicColumns" ||
					 _GroupingParent.Name == "DynamicRows")
			{
				XmlNode ritems = _Draw.GetNamedChildNode(_GroupingParent, "ReportItems");
				if (ritems == null)
					ritems = _Draw.GetCreateNamedChildNode(_GroupingParent, "ReportItems");
				XmlNode item = ritems.FirstChild;
				if (item == null)
				{
					item = _Draw.GetCreateNamedChildNode(ritems, "Textbox");
					XmlNode vnode = _Draw.GetCreateNamedChildNode(item, "Value");
					vnode.InnerText = firstexpr == null? "": firstexpr;
				}
			}
		}

		private void bDelete_Click(object sender, System.EventArgs e)
		{
			int cr = DataGridViewSetup.GetCurrentRowIndex(dgGroup);
			if (cr < 0 || !DataGridViewSetup.IsCurrentRowData(dgGroup))		// no row, or the new row at the end (no row in the table yet)
				return;
			else if (cr == 0)
			{
				DataRow dr = _DataTable.Rows[0];
				dr[0] = null;
			}

			this._DataTable.Rows.RemoveAt(cr);
		}

		private void bUp_Click(object sender, System.EventArgs e)
		{
			int cr = DataGridViewSetup.GetCurrentRowIndex(dgGroup);
			if (cr <= 0)		// already at the top
				return;
			if (cr >= _DataTable.Rows.Count)
				return;			// the new row at the end, which has no row in the table yet (CLEAN-1)
			
			SwapRow(_DataTable.Rows[cr-1], _DataTable.Rows[cr]);
			DataGridViewSetup.SetCurrentRowIndex(dgGroup, cr-1);
		}

		private void bDown_Click(object sender, System.EventArgs e)
		{
			int cr = DataGridViewSetup.GetCurrentRowIndex(dgGroup);
			if (cr < 0)			// invalid index
				return;
			if (cr + 1 >= _DataTable.Rows.Count)
				return;			// already at end
			
			SwapRow(_DataTable.Rows[cr+1], _DataTable.Rows[cr]);
			DataGridViewSetup.SetCurrentRowIndex(dgGroup, cr+1);
		}

		private void SwapRow(DataRow tdr, DataRow fdr)
		{
			// column 1, the table's only column (the swaps of a second and third column threw, so Up and Down failed; CLEAN-1)
			object save = tdr[0];
			tdr[0] = fdr[0];
			fdr[0] = save;
			return;
		}

		private void tbName_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			bool bRows=HasRows();

			// If no rows and no data in name it's ok
			if (!bRows && this.tbName.Text.Trim().Length == 0)
				return;

			if (!ReportNames.IsNameValid(tbName.Text))
			{
				e.Cancel = true;
				MessageBox.Show(string.Format("{0} is an invalid name.", tbName.Text), "Name");
			}
		}

		private void bValueExpr_Click(object sender, System.EventArgs e)
		{
			DataGridViewSetup.CommitEdits(dgGroup);
			int cr = DataGridViewSetup.GetCurrentRowIndex(dgGroup);
			if (cr < 0 || cr >= _DataTable.Rows.Count)
			{	// No rows yet (or the new row at the end, which has no row in the table); create one
				string[] rowValues = new string[1];
				rowValues[0] = null;

				_DataTable.Rows.Add(rowValues);
				cr = _DataTable.Rows.Count-1;
			}
			DataGridViewCell dgc = dgGroup.CurrentCell;
			int cc = dgc == null? 0: dgc.ColumnIndex;
			DataRow dr = _DataTable.Rows[cr];
			string cv = dr[cc] as string;

			DialogExprEditor ee = new DialogExprEditor(_Draw, cv, _GroupingParent, false);
			DialogResult dlgr = ee.ShowDialog();
			if (dlgr == DialogResult.OK)
				dr[cc] = ee.Expression;

		}

		private void bExpr_Click(object sender, System.EventArgs e)
		{
			Button b = sender as Button;
			if (b == null)
				return;
			Control c = null;
			switch (b.Tag as string)
			{
				case "label":
					c = this.cbLabelExpr;
					break;
				case "parent":
					c = this.cbParentExpr;
					break;
			}

			if (c == null)
				return;

			DialogExprEditor ee = new DialogExprEditor(_Draw, c.Text, _GroupingParent, false);
			DialogResult dr = ee.ShowDialog();
			if (dr == DialogResult.OK)
				c.Text = ee.Expression;
			return;
		}

	}
}
