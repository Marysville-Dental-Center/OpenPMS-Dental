// The detail of one measure of the Measure Calc window (package EHR-4a of docs/specs/ehr-rebuild-plan.md, section 9.1): its texts, its
// exclusion count and the engine's audit list of patients. Double-clicking a patient passes them up to the EHR window, which switches the Chart.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Data;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>One measure of a Measure Calc refresh: Objective, Measure, the numerator, denominator and exclusion explanations, the exclusion count
	///with its description, and the audit grid of GetTable, GetTableMu2 or GetTableMu2Mod (patient, explanation, Met). A patient double-clicked
	///is returned in SelectedPatNum with DialogResult.OK.</summary>
	public partial class FormEhrMeasureCalcDetail:FormODBase {
		///<summary>The measure as Measure Calc calculated it.</summary>
		public EhrMeasure EhrMeasureCur;
		public EhrMeasureStage EhrMeasureStageCur;
		public DateTime DateStart;
		public DateTime DateEnd;
		///<summary>The provider Measure Calc passed to the engine.</summary>
		public long ProvNum;
		///<summary>The provider entry's text in Measure Calc.</summary>
		public string ProvText="";
		///<summary>The Requirement column of Measure Calc.</summary>
		public string RequirementText="";
		///<summary>The Percent column of Measure Calc.</summary>
		public string ResultText="";
		///<summary>The patient double-clicked, or 0. Set together with DialogResult.OK.</summary>
		public long SelectedPatNum;

		public FormEhrMeasureCalcDetail() {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormEhrMeasureCalcDetail_Load(object sender,EventArgs e) {
			labelInternalReview.Text=Lan.g("FormEhrMeasureCalc",EhrMeasureCalcRules.INTERNAL_REVIEW_TEXT);
			textMeasureName.Text=EhrMeasureCalcRules.GetMeasureTitle(EhrMeasureStageCur,EhrMeasureCur.MeasureType);
			textPeriod.Text=string.Join("; ",EhrMeasureCalcRules.GetPrintHeading(ProvText,DateStart,DateEnd,GetStageText()));
			textObjective.Text=EhrMeasureCur.Objective;
			textMeasure.Text=EhrMeasureCur.Measure;
			textNumeratorExplain.Text=EhrMeasureCur.NumeratorExplain;
			textDenominatorExplain.Text=EhrMeasureCur.DenominatorExplain;
			textExclusionExplain.Text=EhrMeasureCur.ExclusionExplain;
			if(EhrMeasureCur.ExclusionCount<0) {
				textExclusionCount.Text=Lan.g(this,"No count applies to this measure.");
			}
			else {
				textExclusionCount.Text=EhrMeasureCur.ExclusionCount.ToString()+"  "+EhrMeasureCur.ExclusionCountDescript;
			}
			textResult.Text=Lan.g(this,"Numerator")+" "+EhrMeasureCalcRules.GetCountText(EhrMeasureCur.Numerator)+", "
				+Lan.g(this,"denominator")+" "+EhrMeasureCalcRules.GetCountText(EhrMeasureCur.Denominator)+": "+ResultText+". "
				+Lan.g(this,"Requirement")+": "+RequirementText+".";
			FillGrid();
		}

		private string GetStageText() {
			switch(EhrMeasureStageCur) {
				case EhrMeasureStage.Stage2:
					return Lan.g("FormEhrMeasureCalc","Stage 2");
				case EhrMeasureStage.ModifiedStage2:
					return Lan.g("FormEhrMeasureCalc","Modified Stage 2");
				default:
					return Lan.g("FormEhrMeasureCalc","Stage 1");
			}
		}

		private void FillGrid() {
			Cursor=Cursors.WaitCursor;
			DataTable table;
			try {
				table=EhrMeasures.GetAuditTable(EhrMeasureStageCur,EhrMeasureCur.MeasureType,DateStart,DateEnd,ProvNum);
			}
			finally {
				Cursor=Cursors.Default;
			}
			gridAudit.BeginUpdate();
			gridAudit.Columns.Clear();
			gridAudit.Columns.Add(new GridColumn(Lan.g("TableEhrMeasureCalcAudit","PatNum"),60));
			gridAudit.Columns.Add(new GridColumn(Lan.g("TableEhrMeasureCalcAudit","Patient"),170));
			gridAudit.Columns.Add(new GridColumn(Lan.g("TableEhrMeasureCalcAudit","Explanation"),520));
			gridAudit.Columns.Add(new GridColumn(Lan.g("TableEhrMeasureCalcAudit","Met"),40,HorizontalAlignment.Center));
			gridAudit.ListGridRows.Clear();
			if(table!=null) {
				for(int i=0;i<table.Rows.Count;i++) {
					GridRow row=new GridRow();
					row.Cells.Add(table.Rows[i]["PatNum"].ToString());
					row.Cells.Add(table.Rows[i]["patientName"].ToString());
					row.Cells.Add(table.Rows[i]["explanation"].ToString());
					row.Cells.Add(table.Rows[i]["met"].ToString());
					row.Tag=EhrMeasureCalcRules.GetAuditPatNum(table.Rows[i]);
					gridAudit.ListGridRows.Add(row);
				}
			}
			gridAudit.EndUpdate();
			if(table==null) {
				labelHint.Text=Lan.g(this,"The engine has no patient list for this measure.");
			}
		}

		private void gridAudit_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			if(e.Row<0 || e.Row>=gridAudit.ListGridRows.Count) {
				return;
			}
			long patNum=(long)gridAudit.ListGridRows[e.Row].Tag;
			if(patNum==0) {
				return;
			}
			//As the 2014 quality measure window does: the patient goes up to Measure Calc and the EHR window, which closes with PatientSelect.
			SelectedPatNum=patNum;
			DialogResult=DialogResult.OK;
		}

		private void butClose_Click(object sender,EventArgs e) {
			Close();
		}
	}
}
