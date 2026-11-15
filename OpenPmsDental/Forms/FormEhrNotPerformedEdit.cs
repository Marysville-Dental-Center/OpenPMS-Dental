using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	public partial class FormEhrNotPerformedEdit:FormODBase {
		public EhrNotPerformed EhrNotPerfCur;
		public int SelectedItemIndex;
		public bool IsDateReadOnly;
		///<summary>A saved row whose code is of no item (an "other" row of FormEhrNotPerformed, EHR-7): its code, reason, date and note are shown,
		///and nothing can be changed or saved.</summary>
		public bool IsReadOnly;
		private List<EhrCode> listEhrCodesReason;

		public FormEhrNotPerformedEdit() {
			InitializeComponent();
			InitializeLayoutManager();
			SelectedItemIndex=-1;//this will be set to the value of the enum EhrNotPerformedItem when this form is called
			Lan.F(this);
		}

		///<summary>If using the Add button on FormEhrNotPerformed, an input box will allow the user to select from the list of available items that are not being performed.  The SelectedItemIndex will hold the index of the item selected wich corresponds to the enum EhrNotPerformedItem.  We will use this selected item index to set the EhrNotPerformed code and code system.</summary>
		private void FormEhrNotPerformedEdit_Load(object sender,EventArgs e) {
			if(IsDateReadOnly) {
				textDate.ReadOnly=true;
			}
			if(IsReadOnly || !Enum.IsDefined(typeof(EhrNotPerformedItem),SelectedItemIndex)) {//An "other" row, or no item (EHR-7)
				if(EhrNotPerfCur.IsNew) {//Should never happen: a new row always has its item
					DialogResult=DialogResult.Cancel;
					return;
				}
				FillReadOnly();
				return;
			}
			EhrNotPerformedItem itemNotPerformed=(EhrNotPerformedItem)SelectedItemIndex;
			//The codes come from the 2027 catalog (EHR-7, section 8): a direct-reference-code role (BMI exam, current medications), the catalog's value
			//sets (tobacco screening, the BMI follow-up of E-34), or the program's CVX table (influenza, E-25).
			if(itemNotPerformed==EhrNotPerformedItem.InfluenzaVaccination) {
				radioMedReason.Visible=true;
				radioPatReason.Visible=true;
				radioSysReason.Visible=false;//The 2027 release has no value set of system reasons; a saved one is kept (FillReasonList)
				groupItem.Text=Lan.g(this,"Item Not Performed (no 2027 quality measure uses it)");
			}
			List<EhrCode> listEhrCodes=CqmDataEntry.GetNotPerformedCodes(itemNotPerformed);
			if(listEhrCodes.Count==0 && EhrNotPerfCur.IsNew) {//Happens until the office imports the value sets (Setup, Chart, EHR, Code System Importer), or while the code system's table of these codes is not loaded
				if(itemNotPerformed==EhrNotPerformedItem.InfluenzaVaccination) {
					MsgBox.Show(this,"The CVX table in your database is empty.  You should run the Code System Importer tool in Setup | Chart | EHR.");
				}
				else {
					MsgBox.Show(this,"The codes used for Not Performed items do not exist in the table in your database.  You should run the Code System Importer tool in Setup | Chart | EHR.");
				}
				DialogResult=DialogResult.Cancel;
				return;
			}
			if(EhrNotPerfCur.IsNew) {//if new, CodeValue and CodeSystem are not set, might have to select one
				if(listEhrCodes.Count==1) {//only one code in the selected value set, use it
					EhrNotPerfCur.CodeValue=listEhrCodes[0].CodeValue;
					EhrNotPerfCur.CodeSystem=listEhrCodes[0].CodeSystem;
				}
				else {
					List<string> listCodeDescripts=new List<string>();
					for(int i=0;i<listEhrCodes.Count;i++) {
						listCodeDescripts.Add(listEhrCodes[i].CodeValue+" - "+listEhrCodes[i].Description);
					}
					string prompt=Lan.g(this,"Select the "+Enum.GetNames(typeof(EhrNotPerformedItem))[SelectedItemIndex]+" not being performed from the list below.");
					if(SelectedItemIndex==(int)EhrNotPerformedItem.InfluenzaVaccination) {
						//chooseItem.comboSelection.DropDownWidth=730;
						prompt+="\r\n"+Lan.g(this,"No 2027 quality measure uses this item (the influenza measure was retired); it is kept so that its rows stay readable.");
					}
					InputBox inputBox=new InputBox(prompt,listCodeDescripts);
					inputBox.ShowDialog();
					if(inputBox.IsDialogCancel) {
						DialogResult=DialogResult.Cancel;
						return;
					}
					if(inputBox.SelectedIndex==-1) {
						MsgBox.Show(this,"You must select the "+Enum.GetNames(typeof(EhrNotPerformedItem))[SelectedItemIndex]+" not being performed.");
						DialogResult=DialogResult.Cancel;
						return;
					}
					EhrNotPerfCur.CodeValue=listEhrCodes[inputBox.SelectedIndex].CodeValue;
					EhrNotPerfCur.CodeSystem=listEhrCodes[inputBox.SelectedIndex].CodeSystem;
				}
			}
			for(int i=0;i<listEhrCodes.Count;i++) {
				if(listEhrCodes[i].CodeValue==EhrNotPerfCur.CodeValue && listEhrCodes[i].CodeSystem==EhrNotPerfCur.CodeSystem) {
					textDescription.Text=listEhrCodes[i].Description;
				}
			}
			textCode.Text=EhrNotPerfCur.CodeValue;
			textCodeSystem.Text=EhrNotPerfCur.CodeSystem;
			textDate.Text=EhrNotPerfCur.DateEntry.ToShortDateString();
			textNote.Text=EhrNotPerfCur.Note;
			FillReasonList();
			if(comboCodeReason.SelectedIndex>0) {
				textCodeSystemReason.Text=listEhrCodesReason[comboCodeReason.SelectedIndex-1].CodeSystem;
				textDescriptionReason.Text=listEhrCodesReason[comboCodeReason.SelectedIndex-1].Description;
			}
		}

		///<summary>An "other" row (EHR-7): its saved code, reason, date and note, with nothing to change and no save. Its code is of no item this
		///program knows, or the office has not imported the 2027 release yet, so no pick list is filled. Before the import the window says so: the
		///row's item is then unknown only because the value sets are missing, and importing the release opens the row.</summary>
		private void FillReadOnly() {
			if(CqmDataEntry.IsReleaseImported()) {
				Text=Lan.g(this,"Not Performed Item with Reason")+" - "+Lan.g(this,"read-only, the code is of no item");
			}
			else {
				Text=Lan.g(this,"Not Performed Item with Reason")+" - "+Lan.g(this,"read-only, the 2027 value sets are not imported");
				textDescription.Text=Lan.g(this,"The EHR value sets of the 2027 release are not imported (Setup | Chart | EHR, Code System Importer); until then this row's item is not known.");
			}
			textCode.Text=EhrNotPerfCur.CodeValue;
			textCodeSystem.Text=EhrNotPerfCur.CodeSystem;
			textDate.Text=EhrNotPerfCur.DateEntry.ToShortDateString();
			textNote.Text=EhrNotPerfCur.Note;
			textDate.ReadOnly=true;
			textNote.ReadOnly=true;
			listEhrCodesReason=new List<EhrCode>();
			comboCodeReason.Items.Clear();
			comboCodeReason.Items.Add("none");
			comboCodeReason.SelectedIndex=0;
			if(EhrNotPerfCur.CodeValueReason!="") {
				comboCodeReason.Items.Add(EhrNotPerfCur.CodeValueReason);
				comboCodeReason.SelectedIndex=1;
				textCodeSystemReason.Text=EhrNotPerfCur.CodeSystemReason;
				Snomed snomed=Snomeds.GetByCode(EhrNotPerfCur.CodeValueReason);
				if(snomed!=null) {
					textDescriptionReason.Text=snomed.Description;
				}
			}
			comboCodeReason.Enabled=false;
			butSave.Enabled=false;
			butDelete.Enabled=false;
		}

		private void FillReasonList() {
			//The reasons of the 2027 catalog per item (EHR-7, section 8; CqmDataEntry.GetNotPerformedReasonOids).
			EhrNotPerformedItem itemNotPerformed=(EhrNotPerformedItem)SelectedItemIndex;
			List<string> listValueSetOIDsReason=CqmDataEntry.GetNotPerformedReasonOids(itemNotPerformed,isPatientReason:false);
			if(itemNotPerformed==EhrNotPerformedItem.InfluenzaVaccination) {
				List<string> listValueSetOIDsPatient=CqmDataEntry.GetNotPerformedReasonOids(itemNotPerformed,isPatientReason:true);
				if(radioPatReason.Checked) {
					listValueSetOIDsReason=listValueSetOIDsPatient;
				}
				else if(!radioMedReason.Checked) {//if new or loading a previously saved item not performed, no radio is selected, set the appropriate radio
					radioMedReason.Checked=true;//default to medical reason list if new, or if the saved reason is in neither list
					if(!EhrNotPerfCur.IsNew && EhrCodes.GetForValueSetOIDs(listValueSetOIDsPatient,true)
						.Any(x => x.CodeValue==EhrNotPerfCur.CodeValueReason && x.CodeSystem==EhrNotPerfCur.CodeSystemReason))
					{
						radioPatReason.Checked=true;
						listValueSetOIDsReason=listValueSetOIDsPatient;
					}
				}
			}
			listEhrCodesReason=EhrCodes.GetForValueSetOIDs(listValueSetOIDsReason,true);//these are all SNOMEDCT codes and will only show if they exist in the snomed table.
			//A saved reason that is in none of the 2027 lists (a 2014 reason, a system reason) stays the row's reason, so saving again never drops it.
			if(!EhrNotPerfCur.IsNew && EhrNotPerfCur.CodeValueReason!=""
				&& !listEhrCodesReason.Any(x => x.CodeValue==EhrNotPerfCur.CodeValueReason && x.CodeSystem==EhrNotPerfCur.CodeSystemReason))
			{
				EhrCode ehrCodeSaved=new EhrCode();
				ehrCodeSaved.CodeValue=EhrNotPerfCur.CodeValueReason;
				ehrCodeSaved.CodeSystem=EhrNotPerfCur.CodeSystemReason;
				Snomed snomed=Snomeds.GetByCode(EhrNotPerfCur.CodeValueReason);
				ehrCodeSaved.Description=snomed==null ? "" : snomed.Description;
				listEhrCodesReason.Add(ehrCodeSaved);
			}
			if(listEhrCodesReason.Count==0) {
				MsgBox.Show(this,"There are no codes in the database for reasons not performed.  You must run the Code System Importer tool in Setup | Chart | EHR to import the SNOMEDCT table in order to enter a valid reason.");
			}
			comboCodeReason.Items.Clear();
			comboCodeReason.Items.Add("none");
			comboCodeReason.SelectedIndex=0;//default to 'none' if no reason set for the not performed item
			for(int i=0;i<listEhrCodesReason.Count;i++) {
				comboCodeReason.Items.Add(listEhrCodesReason[i].CodeValue);
				if(EhrNotPerfCur.CodeValueReason==listEhrCodesReason[i].CodeValue && EhrNotPerfCur.CodeSystemReason==listEhrCodesReason[i].CodeSystem) {
					comboCodeReason.SelectedIndex=i+1;//+1 for 'none'
					textCodeSystemReason.Text=listEhrCodesReason[i].CodeSystem;
					textDescriptionReason.Text=listEhrCodesReason[i].Description;
				}
			}
		}

		private void radioReasonMed_Click(object sender,EventArgs e) {
			textCodeSystemReason.Clear();
			textDescriptionReason.Clear();
			FillReasonList();
		}

		private void radioReasonPat_Click(object sender,EventArgs e) {
			textCodeSystemReason.Clear();
			textDescriptionReason.Clear();
			FillReasonList();
		}

		private void radioReasonSys_Click(object sender,EventArgs e) {
			textCodeSystemReason.Clear();
			textDescriptionReason.Clear();
			FillReasonList();
		}

		private void comboReasonCode_SelectionChangeCommitted(object sender,EventArgs e) {
			if(comboCodeReason.SelectedIndex==0) {
				textCodeSystemReason.Clear();
				textDescriptionReason.Clear();
			}
			else {
				textCodeSystemReason.Text=listEhrCodesReason[comboCodeReason.SelectedIndex-1].CodeSystem;
				textDescriptionReason.Text=listEhrCodesReason[comboCodeReason.SelectedIndex-1].Description;
			}
		}

		private void comboCodeReason_DropDown(object sender,EventArgs e) {
			int selectedIndex=comboCodeReason.SelectedIndex;
			comboCodeReason.Items.Clear();
			comboCodeReason.Items.Add("none");
			for(int i=0;i<listEhrCodesReason.Count;i++) {
				comboCodeReason.Items.Add(listEhrCodesReason[i].CodeValue+" - "+listEhrCodesReason[i].Description);
			}
			comboCodeReason.SelectedIndex=selectedIndex;
		}

		private void comboCodeReason_DropDownClosed(object sender,EventArgs e) {
			int selectedIndex=comboCodeReason.SelectedIndex;
			comboCodeReason.Items.Clear();
			comboCodeReason.Items.Add("none");
			for(int i=0;i<listEhrCodesReason.Count;i++) {
				comboCodeReason.Items.Add(listEhrCodesReason[i].CodeValue);
			}
			comboCodeReason.SelectedIndex=selectedIndex;
		}

		private void butDelete_Click(object sender,EventArgs e) {
			if(EhrNotPerfCur.IsNew) {
				DialogResult=DialogResult.Cancel;
				return;
			}
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Delete?")) {
				return;
			}
			Vitalsign vitCur=Vitalsigns.GetFromEhrNotPerformedNum(EhrNotPerfCur.EhrNotPerformedNum);
			if(vitCur!=null) {
				if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Deleting this will remove it from the vitalsign exam it refers to.\r\nDelete anyway?")) {
					return;
				}
				vitCur.EhrNotPerformedNum=0;
				Vitalsigns.Update(vitCur);
			}
			EhrNotPerformeds.Delete(EhrNotPerfCur.EhrNotPerformedNum);
			DialogResult=DialogResult.Cancel;
		}

		private void butSave_Click(object sender,EventArgs e) {
			//validate--------------------------------------
			DateTime date;
			if(textDate.Text=="") {
				MsgBox.Show(this,"Please enter a date.");
				return;
			}
			try {
				date=DateTime.Parse(textDate.Text);
			}
			catch {
				MsgBox.Show(this,"Please fix date first.");
				return;
			}
			//we force the date to match the item not being performed (like vitalsign exam) by making the date text box read only if launched from other item.  Users can still manually add a not performed item from FormEhrNotPerformed by pressing Add and choose any valid date they wish, but it will not be linked to an item.
			string codeValReas="";
			string codeSysReas="";
			if(comboCodeReason.SelectedIndex<1) {//selected 'none' or possibly still -1 (although -1 should never happen)
				if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"If you do not select one of the reasons provided it may be harder to meet your Clinical Quality Measures.  Are you sure you want to continue without selecting a valid reason for not performing the "+Enum.GetNames(typeof(EhrNotPerformedItem))[SelectedItemIndex]+"?")) {
					return;
				}
				codeValReas="";
				codeSysReas="";
			}
			else {
				codeValReas=listEhrCodesReason[comboCodeReason.SelectedIndex-1].CodeValue;//SelectedIndex-1 to account for 'none'
				codeSysReas=listEhrCodesReason[comboCodeReason.SelectedIndex-1].CodeSystem;
			}
			//save--------------------------------------
			EhrNotPerfCur.DateEntry=date;
			EhrNotPerfCur.CodeValueReason=codeValReas;
			EhrNotPerfCur.CodeSystemReason=codeSysReas;
			EhrNotPerfCur.Note=textNote.Text;
			if(EhrNotPerfCur.IsNew) {
				EhrNotPerfCur.EhrNotPerformedNum=EhrNotPerformeds.Insert(EhrNotPerfCur);
			}
			else {
				EhrNotPerformeds.Update(EhrNotPerfCur);
			}
			DialogResult=DialogResult.OK;
		}

	}
}