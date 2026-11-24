using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	public partial class FormCalendar:FormODBase {
		public DateTime DateSelected;
		public DateTime MinDate;

		public FormCalendar() {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormDatePicker_Load(object sender,EventArgs e) {
			monthCalendar.SetDate(DateSelected);
			monthCalendar.MinDate=MinDate;
			//UI-CAL: Windows' calendar raised DateChanged when MinDate moved the selection; MonthCalendarOD raises no event for the program's changes.
			if(monthCalendar.SelectionStart!=DateSelected.Date) {
				DateSelected=monthCalendar.SelectionStart;
			}
		}

		private void monthCalendar_DateChanged(object sender,EventArgs e) {
			DateSelected=monthCalendar.SelectionStart;
		}

		private void butOK_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.OK;
		}

	}
}