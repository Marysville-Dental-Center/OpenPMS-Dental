using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenPmsDentalBusiness;
using OpenPmsDental.UI;
using OpenPmsDental;

namespace OpenPmsDental {
	public partial class FormEhrNotPerformed:FormODBase {
		private List<EhrNotPerformed> listNotPerf;
		public Patient PatCur;

		public FormEhrNotPerformed() {
			InitializeComponent();
			InitializeLayoutManager();
		}

		private void FormEhrNotPerformed_Load(object sender,EventArgs e) {
			FillGrid();
		}

		private void FillGrid() {
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			GridColumn col=new GridColumn("Date",70);
			gridMain.Columns.Add(col);
			col=new GridColumn("Prov",50);
			gridMain.Columns.Add(col);
			col=new GridColumn("Item Not Performed",130);
			gridMain.Columns.Add(col);
			col=new GridColumn("Code",102);
			gridMain.Columns.Add(col);
			col=new GridColumn("Code Description",150);
			gridMain.Columns.Add(col);
			col=new GridColumn("Reason Code",80);
			gridMain.Columns.Add(col);
			col=new GridColumn("Reason Description",150);
			gridMain.Columns.Add(col);
			col=new GridColumn("Note",150);
			gridMain.Columns.Add(col);
			listNotPerf=EhrNotPerformeds.Refresh(PatCur.PatNum);
			List<EhrCode> listEhrCodes=EhrCodes.Listt;//Read once for every row
			gridMain.ListGridRows.Clear();
			GridRow row;
			for(int i=0;i<listNotPerf.Count;i++) {
				row=new GridRow();
				row.Cells.Add(listNotPerf[i].DateEntry.ToShortDateString());
				row.Cells.Add(Providers.GetAbbr(listNotPerf[i].ProvNum));
				//Item not performed------------------------------------------------------------
				//By role, catalog membership and the CVX table (EHR-7, section 8): a code of no item is "other", never a guessed item. The row keeps its
				//item, or null for "other", in its Tag, and the double-click reads the Tag, never this label.
				EhrNotPerformedItem? itemNotPerformed=CqmDataEntry.GetNotPerformedItem(listNotPerf[i].CodeValue,listNotPerf[i].CodeSystem,listEhrCodes);
				if(itemNotPerformed==null) {
					row.Cells.Add(Lan.g(this,"other"));
				}
				else {
					row.Cells.Add(itemNotPerformed.Value.ToString());
				}
				row.Tag=itemNotPerformed;
				//Code not performed------------------------------------------------------------
				row.Cells.Add(listNotPerf[i].CodeValue+" ("+listNotPerf[i].CodeSystem+")");
				//Description of code not performed---------------------------------------------
				string descript="";
				//to get description, first determine which table the code is from.  EhrNotPerformed is allowed to be CPT, CVX, LOINC, SNOMEDCT, and
				//since the BMI follow-up item (EHR-7, E-34) also HCPCS, ICD10CM and RXNORM.
				switch(listNotPerf[i].CodeSystem) {
					case "HCPCS":
						Hcpcs hcpcsCur=Hcpcses.GetByCode(listNotPerf[i].CodeValue);
						if(hcpcsCur!=null) {
							descript=hcpcsCur.DescriptionShort;
						}
						break;
					case "ICD10CM":
						Icd10 icd10Cur=Icd10s.GetByCode(listNotPerf[i].CodeValue);
						if(icd10Cur!=null) {
							descript=icd10Cur.Description;
						}
						break;
					case "RXNORM":
						descript=RxNorms.GetDescByRxCui(listNotPerf[i].CodeValue);
						break;
					case "CPT":
						Cpt cptCur=Cpts.GetByCode(listNotPerf[i].CodeValue);
						if(cptCur!=null) {
							descript=cptCur.Description;
						}
						break;
					case "CVX":
						Cvx cvxCur=Cvxs.GetOneFromDb(listNotPerf[i].CodeValue);
						if(cvxCur!=null) {
							descript=cvxCur.Description;
						}
						break;
					case "LOINC":
						Loinc lCur=Loincs.GetByCode(listNotPerf[i].CodeValue);
						if(lCur!=null) {
							descript=lCur.NameLongCommon;
						}
						break;
					case "SNOMEDCT":
						Snomed sCur=Snomeds.GetByCode(listNotPerf[i].CodeValue);
						if(sCur!=null) {
							descript=sCur.Description;
						}
						break;
				}
				row.Cells.Add(descript);
				//Reason Code-------------------------------------------------------------------
				row.Cells.Add(listNotPerf[i].CodeValueReason+" ("+listNotPerf[i].CodeSystemReason+")");
				//Reason Description------------------------------------------------------------
				descript="";
				if(listNotPerf[i].CodeValueReason!="") {
					//reason codes are only allowed to be SNOMEDCT codes
					Snomed sCur=Snomeds.GetByCode(listNotPerf[i].CodeValueReason);
					if(sCur!=null) {
						descript=sCur.Description;
					}
				}
				row.Cells.Add(descript);
				//Note--------------------------------------------------------------------------
				row.Cells.Add(listNotPerf[i].Note);
				gridMain.ListGridRows.Add(row);
			}
			gridMain.EndUpdate();
		}

		private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			using FormEhrNotPerformedEdit FormEE=new FormEhrNotPerformedEdit();
			FormEE.EhrNotPerfCur=listNotPerf[e.Row];
			//The row's item from its Tag (EHR-7): an "other" row (null) opens read-only, so the edit window never gets an index of no item.
			EhrNotPerformedItem? itemNotPerformed=gridMain.ListGridRows[e.Row].Tag as EhrNotPerformedItem?;
			if(itemNotPerformed==null) {
				FormEE.IsReadOnly=true;
			}
			else {
				FormEE.SelectedItemIndex=(int)itemNotPerformed.Value;
			}
			FormEE.ShowDialog();
			FillGrid();
		}

		private void butAdd_Click(object sender,EventArgs e) {
			List<string> listItems=new List<string>();
			for(int i=0;i<Enum.GetNames(typeof(EhrNotPerformedItem)).Length;i++) {
				listItems.Add(Enum.GetNames(typeof(EhrNotPerformedItem))[i]);
			}
			InputBox inputBox=new InputBox(Lan.g(this,"Select the item not being performed from the list below."),listItems);
			inputBox.ShowDialog();
			if(inputBox.IsDialogCancel) {
				return;
			}
			if(inputBox.SelectedIndex==-1) {
				MsgBox.Show(this,"You must select an item that is not being performed from the list of available items.");
				return;
			}
			using FormEhrNotPerformedEdit FormEE=new FormEhrNotPerformedEdit();
			FormEE.EhrNotPerfCur=new EhrNotPerformed();
			FormEE.EhrNotPerfCur.IsNew=true;
			FormEE.EhrNotPerfCur.PatNum=PatCur.PatNum;
			FormEE.EhrNotPerfCur.ProvNum=PatCur.PriProv;
			FormEE.EhrNotPerfCur.DateEntry=DateTime.Now;
			FormEE.SelectedItemIndex=inputBox.SelectedIndex;//Send in the int of index of selected item
			FormEE.ShowDialog();
			FillGrid();
		}

	}
}