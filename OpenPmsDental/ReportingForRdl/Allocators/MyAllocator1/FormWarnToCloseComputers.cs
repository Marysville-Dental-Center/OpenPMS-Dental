using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OpenPmsDental.Reporting.Allocators.MyAllocator1
{
	/// <summary>
	/// DialogResult values are either Yes or No (cancel is indicated by No)
	/// </summary>
	public partial class FormWarnToCloseComputers :FormODBase
	{
		public FormWarnToCloseComputers()
		{
			InitializeComponent();
			InitializeLayoutManager();
		}

		private void butYes_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Yes;
		}

		private void butNoCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.No;
		}
	}
}
