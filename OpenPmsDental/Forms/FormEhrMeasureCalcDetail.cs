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
	///with its description, and the audit grid of GetTable, GetTableMu2 or GetTableMu2Mod (patient, explanation, Met). The audit table is the one
	///Measure Calc passes in (fetched by an earlier opening of this measure's detail), otherwise it is fetched on an ODThread, with Cancel, the
	///patient grid off meanwhile, and the answer brought back through InvokeIfNotDisposed. A patient double-clicked is returned in SelectedPatNum
	///with DialogResult.OK.</summary>
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
		///<summary>The measure's audit table (null: the engine has no patient list for the measure), valid when HasTableAudit is true. Measure Calc
		///sets both when it has the table; otherwise the detail fetches it and sets both, and Measure Calc keeps it for the next opening.</summary>
		public DataTable TableAudit;
		public bool HasTableAudit;
		///<summary>The thread fetching the audit table, otherwise null.</summary>
		private ODThread _odThread;
		///<summary>Set by Cancel (or by closing the window): the table the thread brings back is not shown.</summary>
		private volatile bool _isCancelRequested;

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
			if(HasTableAudit) {
				FillGrid();
				SetControlsForLoading(isLoading:false);
				return;
			}
			FillGrid();
			StartLoading();
		}

		private void SetControlsForLoading(bool isLoading) {
			gridAudit.Enabled=!isLoading;
			butCancel.Enabled=isLoading;
		}

		///<summary>Fetches the audit table on an ODThread: the engine's query can take as long as the measure did. The window stays responsive;
		///Cancel and closing the window only stop waiting, and the answer then goes nowhere.</summary>
		private void StartLoading() {
			_isCancelRequested=false;
			SetControlsForLoading(isLoading:true);
			labelHint.Text=Lan.g(this,"Loading the patient list...");
			EhrMeasureStage ehrMeasureStage=EhrMeasureStageCur;
			EhrMeasureType ehrMeasureType=EhrMeasureCur.MeasureType;
			DateTime dateStart=DateStart;
			DateTime dateEnd=DateEnd;
			long provNum=ProvNum;
			DataTable tableLoaded=null;
			Exception exceptionLoad=null;
			ODThread odThread=new ODThread(o => {
				tableLoaded=EhrMeasures.GetAuditTable(ehrMeasureStage,ehrMeasureType,dateStart,dateEnd,provNum);
			});
			odThread.Name="EhrMeasureCalcDetail";
			odThread.AddExceptionHandler(ex => {
				exceptionLoad=ex;
				this.InvokeIfNotDisposed(() => {
					if(!_isCancelRequested) {
						FriendlyException.Show(Lan.g(this,"The patient list could not be loaded."),ex);
					}
				});
			});
			odThread.AddExitHandler(o => this.InvokeIfNotDisposed(() => {
				_odThread=null;
				if(_isCancelRequested) {
					return;//Cancel already said so and put the controls back.
				}
				SetControlsForLoading(isLoading:false);
				if(exceptionLoad!=null) {
					labelHint.Text=Lan.g(this,"The patient list could not be loaded:")+" "+exceptionLoad.Message;
					return;
				}
				TableAudit=tableLoaded;
				HasTableAudit=true;
				FillGrid();
			}));
			_odThread=odThread;
			odThread.Start(true);
		}

		private void butCancel_Click(object sender,EventArgs e) {
			if(_odThread==null) {
				return;
			}
			_isCancelRequested=true;
			SetControlsForLoading(isLoading:false);
			butCancel.Enabled=false;
			labelHint.Text=Lan.g(this,"Cancelled: the patient list was not loaded. Close this window and double-click the measure again to load it.");
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

		///<summary>The audit grid from TableAudit: empty until the table is there.</summary>
		private void FillGrid() {
			DataTable table=HasTableAudit ? TableAudit : null;
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
			if(!HasTableAudit) {
				return;
			}
			if(table==null) {
				labelHint.Text=Lan.g(this,"The engine has no patient list for this measure.");
			}
			else {
				labelHint.Text=Lan.g(this,"Double-click a patient to close these windows and switch the Chart to that patient.");
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

		private void FormEhrMeasureCalcDetail_FormClosing(object sender,FormClosingEventArgs e) {
			//A fetch still running ends on its own; its table goes nowhere (InvokeIfNotDisposed).
			_isCancelRequested=true;
		}
	}
}
