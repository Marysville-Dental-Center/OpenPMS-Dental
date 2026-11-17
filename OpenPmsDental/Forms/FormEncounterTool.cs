using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	public partial class FormEncounterTool:FormODBase {
		private List<string> _listRecommendedEncCodes;
		public int SelectedIdxEncounterList=-1;
		public string EncCodeValue;
		public string CodeDescription;
		public string EncCodeSystem;

		public FormEncounterTool() {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormEncounterTool_Load(object sender,EventArgs e) {
			FillRecEncCodesList();
			for(int i=0;i<_listRecommendedEncCodes.Count;i++) {
				comboEncCodes.Items.Add(_listRecommendedEncCodes[i]);
			}
			if(_listRecommendedEncCodes.Count==0) {
				MsgBox.Show(this,"There are no recommended encounter codes: they are the SNOMED CT codes of the imported qualifying-encounter value sets that "
					+"the snomed table holds.  Import the EHR value sets and the SNOMED CT table by running the Code System Importer tool found in Setup | Chart | EHR.");
			}
			if(SelectedIdxEncounterList>=comboEncCodes.Items.Count) {//Should never happen: FormEhrSettings fills its list from the same codes
				SelectedIdxEncounterList=-1;
			}
			comboEncCodes.SelectedIndex=SelectedIdxEncounterList;
			textEncCodeValue.Text=EncCodeValue;
			textEncCodeDescript.Text=CodeDescription;
			labelEncWarning.Visible=false;
		}

		private void FillRecEncCodesList() {
			//All of the recommended codes are SNOMEDCT codes: those of the office's imported qualifying-encounter value sets that its snomed table
			//holds (EHR-7, E-32), as FormEhrSettings lists them, in place of the nine SNOMED CT codes picked for the 2014 versions.
			_listRecommendedEncCodes=CqmDataEntry.GetRecommendedEncounterCodes().Select(x => x.SnomedCode).ToList();
		}

		private void comboEncCodes_SelectionChangeCommitted(object sender,EventArgs e) {
			EncCodeSystem="SNOMEDCT";
			textEncCodeValue.Text="";
			Snomed snomed=Snomeds.GetByCode(comboEncCodes.SelectedItem.ToString());
			textEncCodeDescript.Text=snomed.Description;
			labelEncWarning.Visible=false;
		}

		private void butEncSnomed_Click(object sender,EventArgs e) {
			using FormSnomeds formSnomeds=new FormSnomeds();
			formSnomeds.IsSelectionMode=true;
			formSnomeds.ShowDialog();
			if(formSnomeds.DialogResult!=DialogResult.OK) {
				return;
			}
			EncCodeSystem="SNOMEDCT";
			for(int i=1;i<comboEncCodes.Items.Count;i++) {
				if(formSnomeds.SnomedSelected.SnomedCode==comboEncCodes.Items[i].ToString()) {//if they selected one of the recommended codes, select in list
					comboEncCodes.SelectedIndex=i;
					textEncCodeValue.Clear();
					textEncCodeDescript.Text=formSnomeds.SnomedSelected.Description;
					labelEncWarning.Visible=false;
					return;
				}
			}
			comboEncCodes.SelectedIndex=-1;
			textEncCodeValue.Text=formSnomeds.SnomedSelected.SnomedCode;
			textEncCodeDescript.Text=formSnomeds.SnomedSelected.Description;
			labelEncWarning.Visible=true;
		}

		private void butEncHcpcs_Click(object sender,EventArgs e) {
			using FormHcpcs formHcpcs=new FormHcpcs();
			formHcpcs.IsSelectionMode=true;
			formHcpcs.ShowDialog();
			if(formHcpcs.DialogResult!=DialogResult.OK) {
				return;
			}
			EncCodeSystem="HCPCS";
			comboEncCodes.SelectedIndex=-1;
			textEncCodeValue.Text=formHcpcs.HcpcsSelected.HcpcsCode;
			textEncCodeDescript.Text=formHcpcs.HcpcsSelected.DescriptionShort;
			labelEncWarning.Visible=true;
		}

		private void butEncCdt_Click(object sender,EventArgs e) {
			using FormProcCodes formProcCodes=new FormProcCodes();
			formProcCodes.IsSelectionMode=true;
			formProcCodes.ShowDialog();
			if(formProcCodes.DialogResult!=DialogResult.OK) {
				return;
			}
			EncCodeSystem="CDT";
			comboEncCodes.SelectedIndex=-1;
			ProcedureCode procedureCode=ProcedureCodes.GetProcCode(formProcCodes.CodeNumSelected);
			textEncCodeValue.Text=procedureCode.ProcCode;
			textEncCodeDescript.Text=procedureCode.Descript;
			labelEncWarning.Visible=true;
		}

		private void butEncCpt_Click(object sender,EventArgs e) {
			using FormCpts formCpts=new FormCpts();
			formCpts.IsSelectionMode=true;
			formCpts.ShowDialog();
			if(formCpts.DialogResult!=DialogResult.OK) {
				return;
			}
			EncCodeSystem="CPT";
			comboEncCodes.SelectedIndex=-1;
			textEncCodeValue.Text=formCpts.CptSelected.CptCode;
			textEncCodeDescript.Text=formCpts.CptSelected.Description;
			labelEncWarning.Visible=true;
		}

		private void butRun_Click(object sender,EventArgs e) {
			if(!textDateStart.IsValid()
				|| !textDateEnd.IsValid()
				|| textDateStart.Text==""
				|| textDateEnd.Text=="") 
			{
				MsgBox.Show(this,"Please enter a valid date range.");
				return;
			}
			if(comboEncCodes.SelectedIndex==-1
				&& textEncCodeValue.Text=="") {
				MsgBox.Show(this,"Please select a code.");
				return;
			}
			string codeValue;
			if(comboEncCodes.SelectedIndex==-1) {
				codeValue=textEncCodeValue.Text;
			}
			else {
				codeValue=comboEncCodes.SelectedItem.ToString();
			}
			long insertedEncs=Encounters.InsertEncsFromProcDates(PIn.Date(textDateStart.Text),PIn.Date(textDateEnd.Text),codeValue,EncCodeSystem);
			MessageBox.Show(Lan.g("FormEncounterTool","Number of encounters inserted:")+" "+insertedEncs.ToString());
			if(PrefC.GetString(PrefName.CQMDefaultEncounterCodeValue)=="none") {
				if(MsgBox.Show(this,MsgBoxButtons.YesNo,"Do you want to set this code as the default encounter code?")) {
					Prefs.UpdateString(PrefName.CQMDefaultEncounterCodeValue,codeValue);
					Prefs.UpdateString(PrefName.CQMDefaultEncounterCodeSystem,EncCodeSystem);
				}
			}
			DialogResult=DialogResult.OK;
		}

	}
}