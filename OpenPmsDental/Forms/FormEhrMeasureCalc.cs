// The Meaningful Use measures window behind the EHR window's Measure Calc button (package EHR-4a of docs/specs/ehr-rebuild-plan.md, sections
// 9.1 and 9.2), rebuilt as open code on the open measure engine (EhrMeasures) from public sources: the CMS EHR Incentive Program specification
// sheets for eligible professionals (Stage 1 for 2014, Stage 2, Modified Stage 2), the 2015-2017 final rule and Open Dental's public manual. The
// numbers are for internal review only: this program is not certified, and they cannot be used to attest or to submit (E-1).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>Measure Calc: one provider's Meaningful Use measures for a reporting period and a stage, calculated one measure at a time on a
	///background thread, with Cancel between two measures. Double-clicking a measure opens its detail and patient list; a patient picked there is
	///passed up in SelectedPatNum, and the EHR window then switches the Chart to that patient.</summary>
	public partial class FormEhrMeasureCalc:FormODBase {
		///<summary>The patient picked in the detail window, or 0. Set together with DialogResult.OK.</summary>
		public long SelectedPatNum;
		///<summary>One entry per last and first name that has an ehrprovkey row.</summary>
		private List<EhrMeasureCalcProvider> _listEhrMeasureCalcProviders=new List<EhrMeasureCalcProvider>();
		///<summary>The measures calculated so far by the last refresh, in the engine's order.</summary>
		private List<EhrMeasure> _listEhrMeasures=new List<EhrMeasure>();
		///<summary>What the last refresh calculated: the grid, the detail and the printout use these, not the controls, which may have changed.</summary>
		private EhrMeasureStage _ehrMeasureStage;
		private DateTime _dateStart;
		private DateTime _dateEnd;
		private long _provNum;
		private string _provText="";
		///<summary>The thread of a refresh that is running, otherwise null.</summary>
		private ODThread _odThread;
		///<summary>Set by Cancel (or by closing the window), read by the thread between two measures.</summary>
		private volatile bool _isCancelRequested;
		private bool _isHeadingPrinted;
		private int _pagesPrinted;

		public FormEhrMeasureCalc() {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormEhrMeasureCalc_Load(object sender,EventArgs e) {
			labelInternalReview.Text=Lan.g(this,EhrMeasureCalcRules.INTERNAL_REVIEW_TEXT);
			comboStage.Items.Add(Lan.g(this,"Stage 1"),EhrMeasureStage.Stage1);
			comboStage.Items.Add(Lan.g(this,"Stage 2"),EhrMeasureStage.Stage2);
			comboStage.Items.Add(Lan.g(this,"Modified Stage 2"),EhrMeasureStage.ModifiedStage2);
			textDateStart.Value=new DateTime(DateTime.Today.Year,1,1);
			textDateEnd.Value=new DateTime(DateTime.Today.Year,12,31);
			_listEhrMeasureCalcProviders=EhrMeasureCalcRules.GetProviderChoices(EhrMeasures.GetReportingProviders(),Providers.GetDeepCopy(),
				Security.CurUser.ProvNum);
			for(int i=0;i<_listEhrMeasureCalcProviders.Count;i++) {
				comboProv.Items.Add(_listEhrMeasureCalcProviders[i].Text,_listEhrMeasureCalcProviders[i]);
			}
			if(_listEhrMeasureCalcProviders.Count==0) {
				comboStage.SetSelected(0);
				labelProgress.Text=Lan.g(this,"No provider is marked for EHR reporting. Add a row with the provider's last and first name in Setup, Chart, "
					+"EHR, Provider Keys.");
			}
			else {
				comboProv.SetSelected(EhrMeasureCalcRules.GetDefaultProviderIndex(_listEhrMeasureCalcProviders,Security.CurUser.ProvNum));
				SetStageFromProvider();
				labelProgress.Text=Lan.g(this,"Choose the provider, the reporting period and the stage, then click Refresh.");
			}
			FillGrid();
			SetControlsForRun(isRunning:false);
		}

		///<summary>The stage the provider's Meaningful Use stage gives, or the global setting for providers set to use it, as the EHR window does.</summary>
		private void SetStageFromProvider() {
			EhrMeasureCalcProvider ehrMeasureCalcProvider=comboProv.GetSelected<EhrMeasureCalcProvider>();
			if(ehrMeasureCalcProvider==null) {
				return;
			}
			Provider provider=Providers.GetProv(ehrMeasureCalcProvider.ProvNum);
			int ehrMuStage=provider==null ? 0 : provider.EhrMuStage;
			EhrMeasureStage ehrMeasureStage=EhrMeasureCalcRules.GetDefaultStage(ehrMuStage,PrefC.GetInt(PrefName.MeaningfulUseTwo));
			comboStage.SetSelected((int)ehrMeasureStage);
		}

		private void comboProv_SelectionChangeCommitted(object sender,EventArgs e) {
			SetStageFromProvider();
		}

		private void SetControlsForRun(bool isRunning) {
			butRefresh.Enabled=!isRunning && _listEhrMeasureCalcProviders.Count>0;
			butCancel.Enabled=isRunning;
			butPrint.Enabled=!isRunning && _listEhrMeasures.Count>0;
			comboProv.Enabled=!isRunning;
			comboStage.Enabled=!isRunning;
			textDateStart.ReadOnly=isRunning;
			textDateEnd.ReadOnly=isRunning;
		}

		private void butRefresh_Click(object sender,EventArgs e) {
			if(_odThread!=null) {
				return;
			}
			if(!textDateStart.IsValid() || !textDateEnd.IsValid()) {
				MsgBox.Show(this,"Please fix the dates first.");
				return;
			}
			DateTime dateStart=textDateStart.Value;
			DateTime dateEnd=textDateEnd.Value;
			string errorDates=EhrMeasureCalcRules.GetDateError(dateStart,dateEnd);
			if(errorDates!="") {
				MsgBox.Show(this,errorDates);
				return;
			}
			EhrMeasureCalcProvider ehrMeasureCalcProvider=comboProv.GetSelected<EhrMeasureCalcProvider>();
			if(ehrMeasureCalcProvider==null) {
				MsgBox.Show(this,"Please select a provider first.");
				return;
			}
			_ehrMeasureStage=comboStage.GetSelected<EhrMeasureStage>();
			_dateStart=dateStart;
			_dateEnd=dateEnd;
			_provNum=ehrMeasureCalcProvider.ProvNum;
			_provText=ehrMeasureCalcProvider.Text;
			_listEhrMeasures=new List<EhrMeasure>();
			labelNotes.Text=string.Join("\r\n",EhrMeasureCalcRules.GetPeriodNotes(_ehrMeasureStage,_dateStart,_dateEnd));
			FillGrid();
			_isCancelRequested=false;
			SetControlsForRun(isRunning:true);
			labelProgress.Text=Lan.g(this,"Starting...");
			//The engine's queries can take minutes on a large database. One measure at a time on this thread, so Cancel takes effect between two
			//measures and no work table is left half filled. No Join: the window stays responsive, and closing it only asks the thread to stop.
			EhrMeasureStage ehrMeasureStage=_ehrMeasureStage;
			long provNum=_provNum;
			//Set on the refresh's thread: the measure being calculated, and the error that stopped the refresh. ODThread runs the exit handler after
			//the exception handler, and the exit handler reads them on the window's thread once the refresh has stopped.
			int indexStarted=-1;
			int countTotal=0;
			EhrMeasureType ehrMeasureTypeStarted=EhrMeasureType.ProblemList;
			Exception exceptionRun=null;
			ODThread odThread=new ODThread(o => {
				RunCalculation(ehrMeasureStage,dateStart,dateEnd,provNum,() => _isCancelRequested,
					(index,count,ehrMeasureType) => {
						indexStarted=index;
						countTotal=count;
						ehrMeasureTypeStarted=ehrMeasureType;
						this.InvokeIfNotDisposed(() => {
							labelProgress.Text=Lan.g(this,"Calculating measure")+" "+(index+1).ToString()+" "+Lan.g(this,"of")+" "+count.ToString()+": "
								+EhrMeasureCalcRules.GetMeasureTitle(ehrMeasureStage,ehrMeasureType);
						});
					},
					(index,count,ehrMeasure) => this.InvokeIfNotDisposed(() => {
						_listEhrMeasures.Add(ehrMeasure);
						FillGrid();
					}));
			});
			odThread.Name="EhrMeasureCalc";
			odThread.AddExceptionHandler(ex => {
				exceptionRun=ex;
				this.InvokeIfNotDisposed(() => {
					FriendlyException.Show(Lan.g(this,"The measures could not be calculated."),ex);
				});
			});
			odThread.AddExitHandler(o => this.InvokeIfNotDisposed(() => {
				_odThread=null;
				//After an error the line says where the refresh stopped and keeps the error's message, never "Done".
				labelProgress.Text=EhrMeasureCalcRules.GetProgressEndText(exceptionRun,_isCancelRequested,_listEhrMeasures.Count,indexStarted,countTotal,
					indexStarted<0 ? "" : EhrMeasureCalcRules.GetMeasureTitle(ehrMeasureStage,ehrMeasureTypeStarted));
				SetControlsForRun(isRunning:false);
			}));
			_odThread=odThread;
			odThread.Start(true);
		}

		///<summary>One refresh, without the window: drops the engines' work tables older than one day, then calculates the stage's measures one at a
		///time through the engine (each a Middle Tier call of its own), checking funcIsCancelRequested before each. actionMeasureStarting gets the
		///index, the count and the type before a measure; actionMeasureDone the index, the count and the calculated measure after it. Returns the
		///measures calculated, fewer than the stage has when cancelled. Runs on the window's thread; the tests call it directly.</summary>
		public static List<EhrMeasure> RunCalculation(EhrMeasureStage ehrMeasureStage,DateTime dateStart,DateTime dateEnd,long provNum,
			Func<bool> funcIsCancelRequested,Action<int,int,EhrMeasureType> actionMeasureStarting,Action<int,int,EhrMeasure> actionMeasureDone)
		{
			EhrMeasures.DropOldWorkTables(EhrMeasures.WORK_TABLE_MAX_AGE_SECONDS);
			List<EhrMeasure> listEhrMeasuresToCalc=EhrMeasures.GetMeasuresToCalc(ehrMeasureStage);
			List<EhrMeasure> listEhrMeasuresDone=new List<EhrMeasure>();
			for(int i=0;i<listEhrMeasuresToCalc.Count;i++) {
				if(funcIsCancelRequested!=null && funcIsCancelRequested()) {
					break;
				}
				actionMeasureStarting?.Invoke(i,listEhrMeasuresToCalc.Count,listEhrMeasuresToCalc[i].MeasureType);
				EhrMeasure ehrMeasure=EhrMeasures.CalcMeasure(ehrMeasureStage,listEhrMeasuresToCalc[i],dateStart,dateEnd,provNum);
				listEhrMeasuresDone.Add(ehrMeasure);
				actionMeasureDone?.Invoke(i,listEhrMeasuresToCalc.Count,ehrMeasure);
			}
			return listEhrMeasuresDone;
		}

		private void FillGrid() {
			int reportingYear=EhrMeasureCalcRules.GetReportingYear(_dateStart);
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			gridMain.Columns.Add(new GridColumn(Lan.g("TableEhrMeasureCalc","Measure"),190));
			gridMain.Columns.Add(new GridColumn(Lan.g("TableEhrMeasureCalc","Numerator"),75,HorizontalAlignment.Center));
			gridMain.Columns.Add(new GridColumn(Lan.g("TableEhrMeasureCalc","Denominator"),80,HorizontalAlignment.Center));
			gridMain.Columns.Add(new GridColumn(Lan.g("TableEhrMeasureCalc","Percent"),230));
			gridMain.Columns.Add(new GridColumn(Lan.g("TableEhrMeasureCalc","Requirement"),200));
			gridMain.Columns.Add(new GridColumn(Lan.g("TableEhrMeasureCalc","Exclusion Count"),95,HorizontalAlignment.Center));
			gridMain.ListGridRows.Clear();
			for(int i=0;i<_listEhrMeasures.Count;i++) {
				EhrMeasure ehrMeasure=_listEhrMeasures[i];
				EhrMeasureRequirement ehrMeasureRequirement=EhrMeasureCalcRules.GetRequirement(_ehrMeasureStage,ehrMeasure.MeasureType,
					ehrMeasure.PercentThreshold,reportingYear);
				GridRow row=new GridRow();
				row.Cells.Add(EhrMeasureCalcRules.GetMeasureTitle(_ehrMeasureStage,ehrMeasure.MeasureType));
				row.Cells.Add(EhrMeasureCalcRules.GetCountText(ehrMeasure.Numerator));
				row.Cells.Add(EhrMeasureCalcRules.GetCountText(ehrMeasure.Denominator));
				row.Cells.Add(EhrMeasureCalcRules.GetResultText(_ehrMeasureStage,ehrMeasure.MeasureType,ehrMeasureRequirement,ehrMeasure.Numerator,
					ehrMeasure.Denominator));
				row.Cells.Add(ehrMeasureRequirement.GetText());
				row.Cells.Add(EhrMeasureCalcRules.GetExclusionCountText(ehrMeasure.ExclusionCount));
				if(EhrMeasureCalcRules.IsMet(ehrMeasureRequirement,ehrMeasure.Numerator,ehrMeasure.Denominator)) {
					row.ColorBackG=EhrMeasureCalcRules.ColorMet;
				}
				row.Tag=ehrMeasure;
				gridMain.ListGridRows.Add(row);
			}
			gridMain.EndUpdate();
		}

		private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			if(e.Row<0 || e.Row>=gridMain.ListGridRows.Count) {
				return;
			}
			EhrMeasure ehrMeasure=(EhrMeasure)gridMain.ListGridRows[e.Row].Tag;
			EhrMeasureRequirement ehrMeasureRequirement=EhrMeasureCalcRules.GetRequirement(_ehrMeasureStage,ehrMeasure.MeasureType,
				ehrMeasure.PercentThreshold,EhrMeasureCalcRules.GetReportingYear(_dateStart));
			using FormEhrMeasureCalcDetail formEhrMeasureCalcDetail=new FormEhrMeasureCalcDetail();
			formEhrMeasureCalcDetail.EhrMeasureCur=ehrMeasure;
			formEhrMeasureCalcDetail.EhrMeasureStageCur=_ehrMeasureStage;
			formEhrMeasureCalcDetail.DateStart=_dateStart;
			formEhrMeasureCalcDetail.DateEnd=_dateEnd;
			formEhrMeasureCalcDetail.ProvNum=_provNum;
			formEhrMeasureCalcDetail.ProvText=_provText;
			formEhrMeasureCalcDetail.RequirementText=ehrMeasureRequirement.GetText();
			formEhrMeasureCalcDetail.ResultText=EhrMeasureCalcRules.GetResultText(_ehrMeasureStage,ehrMeasure.MeasureType,ehrMeasureRequirement,
				ehrMeasure.Numerator,ehrMeasure.Denominator);
			formEhrMeasureCalcDetail.ShowDialog();
			if(!EhrMeasureCalcRules.IsPatientPicked(formEhrMeasureCalcDetail.DialogResult,formEhrMeasureCalcDetail.SelectedPatNum)) {
				return;
			}
			//Up to the EHR window, which closes with PatientSelect; the Chart then switches to the patient.
			_isCancelRequested=true;
			SelectedPatNum=formEhrMeasureCalcDetail.SelectedPatNum;
			DialogResult=DialogResult.OK;
			Close();
		}

		private void butCancel_Click(object sender,EventArgs e) {
			if(_odThread==null) {
				return;
			}
			_isCancelRequested=true;
			butCancel.Enabled=false;
			labelProgress.Text=Lan.g(this,"Stopping after the measure being calculated...");
		}

		private void butPrint_Click(object sender,EventArgs e) {
			if(_listEhrMeasures.Count==0) {
				MsgBox.Show(this,"Click Refresh first.");
				return;
			}
			_isHeadingPrinted=false;
			_pagesPrinted=0;
			PrinterL.TryPrintOrDebugRpPreview(pd_PrintPage,Lan.g(this,"Meaningful Use measures printed"),PrintoutOrientation.Landscape,
				isForcedPreview:true);
		}

		private void pd_PrintPage(object sender,PrintPageEventArgs e) {
			Rectangle bounds=e.MarginBounds;
			Graphics g=e.Graphics;
			int yPos=bounds.Top;
			int center=bounds.X+bounds.Width/2;
			if(!_isHeadingPrinted) {
				using Font fontHeading=new Font("Arial",13,FontStyle.Bold);
				using Font fontSubHeading=new Font("Arial",10,FontStyle.Bold);
				using Font fontNote=new Font("Arial",9);
				string text=Lan.g(this,"Meaningful Use Measures");
				g.DrawString(text,fontHeading,Brushes.Black,center-g.MeasureString(text,fontHeading).Width/2,yPos);
				yPos+=(int)g.MeasureString(text,fontHeading).Height;
				List<string> listLines=EhrMeasureCalcRules.GetPrintHeading(_provText,_dateStart,_dateEnd,comboStage.Items.GetTextShowingAt((int)_ehrMeasureStage));
				for(int i=0;i<listLines.Count;i++) {
					g.DrawString(listLines[i],fontSubHeading,Brushes.Black,center-g.MeasureString(listLines[i],fontSubHeading).Width/2,yPos);
					yPos+=(int)g.MeasureString(listLines[i],fontSubHeading).Height;
				}
				string textInternal=Lan.g(this,EhrMeasureCalcRules.INTERNAL_REVIEW_TEXT);
				RectangleF rectangleInternal=new RectangleF(bounds.X,yPos,bounds.Width,g.MeasureString(textInternal,fontNote,bounds.Width).Height);
				g.DrawString(textInternal,fontNote,Brushes.Black,rectangleInternal);
				yPos+=(int)rectangleInternal.Height+10;
				_isHeadingPrinted=true;
			}
			yPos=gridMain.PrintPage(g,_pagesPrinted,bounds,yPos);
			_pagesPrinted++;
			e.HasMorePages=yPos==-1;
		}

		private void butClose_Click(object sender,EventArgs e) {
			Close();
		}

		private void FormEhrMeasureCalc_FormClosing(object sender,FormClosingEventArgs e) {
			//A running refresh stops after its current measure; its results go nowhere (InvokeIfNotDisposed).
			_isCancelRequested=true;
		}
	}

	///<summary>How a measure's requirement is stated (section 9.2 of the EHR rebuild plan).</summary>
	public enum EhrMeasureRequirementKind {
		///<summary>Met when the percent is more than the threshold (the engine's "More than this percent").</summary>
		MoreThanPercent,
		///<summary>Met when at least one patient is in the numerator (Modified Stage 2 in 2015 and 2016, from the 2015-2017 rule).</summary>
		AtLeastOnePatient,
		///<summary>A yes/no attestation that is not calculated (Modified Stage 2 secure messaging in 2015): never shown as met.</summary>
		Attestation
	}

	///<summary>A measure's requirement for the window, by stage and reporting year.</summary>
	public class EhrMeasureRequirement {
		public EhrMeasureRequirementKind Kind;
		///<summary>For MoreThanPercent: the engine's threshold.</summary>
		public int PercentThreshold;

		public EhrMeasureRequirement(EhrMeasureRequirementKind kind,int percentThreshold) {
			Kind=kind;
			PercentThreshold=percentThreshold;
		}

		public string GetText() {
			switch(Kind) {
				case EhrMeasureRequirementKind.AtLeastOnePatient:
					return "At least 1 patient";
				case EhrMeasureRequirementKind.Attestation:
					return "Yes/no: the capability was fully enabled";
				default:
					return "More than "+PercentThreshold.ToString(CultureInfo.InvariantCulture)+"%";
			}
		}
	}

	///<summary>One entry of the provider list: the providers the engine counts together (an ehrprovkey row's last and first name), and the one whose
	///number is passed to the engine.</summary>
	public class EhrMeasureCalcProvider {
		///<summary>The provider passed to the engine: the logged-on user's provider when it is one of them, otherwise the first one that is not
		///hidden, otherwise the first.</summary>
		public long ProvNum;
		///<summary>Every provider of the entry, in ProvNum order.</summary>
		public List<long> ListProvNums=new List<long>();
		///<summary>"Last, First (Abbr1, Abbr2)".</summary>
		public string Text="";
	}

	///<summary>The rules of the Measure Calc window, as pure functions (section 9 of the EHR rebuild plan). They change nothing the engine returns:
	///the requirement of a Modified Stage 2 measure that depends on the year is applied here, by the year of the reporting period.</summary>
	public static class EhrMeasureCalcRules {
		///<summary>The internal-review line (E-1), at the top of the window and of the printout.</summary>
		public const string INTERNAL_REVIEW_TEXT="For internal review only. This program is not certified EHR technology. These numbers cannot be used to attest "
			+"or to submit to any program.";
		///<summary>The text of a numerator or denominator the engine did not calculate (-1).</summary>
		public const string NOT_CALCULATED="not calculated";
		///<summary>The result of a yes/no attestation.</summary>
		public const string ATTESTATION_NOT_CALCULATED="attestation, not calculated";
		///<summary>The result of a measure whose numerator came from the patient portal or its web mail, when it is not met.</summary>
		public const string DEPENDS_ON_REMOVED_PORTAL="depends on the removed patient portal";
		///<summary>The result when the denominator is 0.</summary>
		public const string NO_PATIENTS="no patients in the denominator";
		///<summary>The colour of a met row, the one the EHR window's grid uses.</summary>
		public static readonly Color ColorMet=Color.FromArgb(178,255,178);

		///<summary>The stage of FormEHR's rule: the provider's Meaningful Use stage 1, 2 or 3 gives Stage 1, Stage 2 or Modified Stage 2; any other
		///value (0: use the global setting) the MeaningfulUseTwo preference: 1 Stage 2, 2 Modified Stage 2, anything else Stage 1.</summary>
		public static EhrMeasureStage GetDefaultStage(int ehrMuStage,int meaningfulUseTwo) {
			switch(ehrMuStage) {
				case 1:
					return EhrMeasureStage.Stage1;
				case 2:
					return EhrMeasureStage.Stage2;
				case 3:
					return EhrMeasureStage.ModifiedStage2;
			}
			switch(meaningfulUseTwo) {
				case 1:
					return EhrMeasureStage.Stage2;
				case 2:
					return EhrMeasureStage.ModifiedStage2;
			}
			return EhrMeasureStage.Stage1;
		}

		///<summary>"" when the dates can be calculated, otherwise why not.</summary>
		public static string GetDateError(DateTime dateStart,DateTime dateEnd) {
			if(dateStart.Year<1880 || dateEnd.Year<1880) {
				return "Enter a start date and an end date.";
			}
			if(dateEnd<dateStart) {
				return "The end date is before the start date.";
			}
			return "";
		}

		///<summary>The year whose requirements apply: the year of the start date. A Modified Stage 2 period lies within one calendar year; a period
		///that spans two gets a note (GetPeriodNotes).</summary>
		public static int GetReportingYear(DateTime dateStart) {
			return dateStart.Year;
		}

		///<summary>The requirement of a measure. Modified Stage 2, by the 2015-2017 rule: patient electronic access measure 2 (view, download or
		///transmit; ElectronicCopy) is at least 1 patient in 2015 and 2016; secure messaging is a yes/no attestation that the capability was fully
		///enabled in 2015 and at least 1 patient in 2016. Every other measure, stage and year (2017 and later, and the years before 2015, when
		///Modified Stage 2 did not exist) is the engine's "more than this percent".</summary>
		public static EhrMeasureRequirement GetRequirement(EhrMeasureStage ehrMeasureStage,EhrMeasureType ehrMeasureType,int percentThreshold,
			int reportingYear)
		{
			if(ehrMeasureStage==EhrMeasureStage.ModifiedStage2) {
				if(ehrMeasureType==EhrMeasureType.ElectronicCopy && reportingYear.In(2015,2016)) {
					return new EhrMeasureRequirement(EhrMeasureRequirementKind.AtLeastOnePatient,percentThreshold);
				}
				if(ehrMeasureType==EhrMeasureType.SecureMessaging && reportingYear==2015) {
					return new EhrMeasureRequirement(EhrMeasureRequirementKind.Attestation,percentThreshold);
				}
				if(ehrMeasureType==EhrMeasureType.SecureMessaging && reportingYear==2016) {
					return new EhrMeasureRequirement(EhrMeasureRequirementKind.AtLeastOnePatient,percentThreshold);
				}
			}
			return new EhrMeasureRequirement(EhrMeasureRequirementKind.MoreThanPercent,percentThreshold);
		}

		///<summary>True for the measures whose numerator came from the patient portal and its web mail, which this program no longer has: patient
		///electronic access measure 1 (timely online access, ElectronicCopyAccess) in all three stages, which counts OnlineAccessProvided events that
		///nothing in this build writes any more (the lead's decision on the package's owner question 1); and patient electronic access measure 2
		///(view, download or transmit, ElectronicCopy) and secure messaging, in Stage 2 and Modified Stage 2.</summary>
		public static bool IsPortalMeasure(EhrMeasureStage ehrMeasureStage,EhrMeasureType ehrMeasureType) {
			if(ehrMeasureType==EhrMeasureType.ElectronicCopyAccess) {
				return true;
			}
			if(ehrMeasureStage==EhrMeasureStage.Stage1) {
				return false;
			}
			return ehrMeasureType.In(EhrMeasureType.ElectronicCopy,EhrMeasureType.SecureMessaging);
		}

		///<summary>Met (a green row): more than the threshold when the denominator is above 0 (numerator times 100 more than the threshold times the
		///denominator, in whole numbers); at least one patient in the numerator; never for an attestation or a value the engine did not
		///calculate.</summary>
		public static bool IsMet(EhrMeasureRequirement ehrMeasureRequirement,int numerator,int denominator) {
			if(numerator<0 || denominator<0) {
				return false;
			}
			switch(ehrMeasureRequirement.Kind) {
				case EhrMeasureRequirementKind.Attestation:
					return false;
				case EhrMeasureRequirementKind.AtLeastOnePatient:
					return numerator>=1;
				default:
					return denominator>0 && (long)numerator*100>(long)ehrMeasureRequirement.PercentThreshold*denominator;
			}
		}

		///<summary>The Percent column: "attestation, not calculated" for a yes/no row, "not calculated" for -1, "depends on the removed patient
		///portal" for a portal measure that is not met (instead of a 0%), "no patients in the denominator" for 0, otherwise the percent of
		///GetPercentText: one decimal, rounded down, and more decimals when a met row would otherwise show no more than its threshold.</summary>
		public static string GetResultText(EhrMeasureStage ehrMeasureStage,EhrMeasureType ehrMeasureType,EhrMeasureRequirement ehrMeasureRequirement,
			int numerator,int denominator)
		{
			if(ehrMeasureRequirement.Kind==EhrMeasureRequirementKind.Attestation) {
				return ATTESTATION_NOT_CALCULATED;
			}
			if(numerator<0 || denominator<0) {
				return NOT_CALCULATED;
			}
			if(IsPortalMeasure(ehrMeasureStage,ehrMeasureType) && !IsMet(ehrMeasureRequirement,numerator,denominator)) {
				return DEPENDS_ON_REMOVED_PORTAL;
			}
			if(denominator==0) {
				return NO_PATIENTS;
			}
			if(ehrMeasureRequirement.Kind==EhrMeasureRequirementKind.MoreThanPercent) {
				return GetPercentText(numerator,denominator,ehrMeasureRequirement.PercentThreshold);
			}
			return GetPercentText(numerator,denominator);
		}

		///<summary>The percent with at most one decimal, rounded down, so it never shows more than it is: 3 of 4 is "75%", 1 of 3 is "33.3%".</summary>
		public static string GetPercentText(int numerator,int denominator) {
			return GetPercentText(numerator,denominator,-1);
		}

		///<summary>The most decimals GetPercentText shows. Ten are enough for any int denominator: a percent above a whole-number threshold is above
		///it by at least 100/denominator of a hundredth, more than 1E-10 for a denominator below 2^31.</summary>
		private const int PERCENT_DECIMALS_MAX=10;

		///<summary>The percent rounded down (never shown as more than it is), with one decimal; when it is more than percentThreshold but one decimal
		///shows no more than the threshold, with as many decimals as it takes to show it above (4001 of 5000 against 80 is "80.02%", not "80%"). So a
		///met row's percent is visibly more than its threshold, and an unmet one is at or below it. -1 for no threshold: one decimal.</summary>
		public static string GetPercentText(int numerator,int denominator,int percentThreshold) {
			if(denominator<=0) {
				return "";
			}
			bool isAbove=percentThreshold>=0 && (long)numerator*100>(long)percentThreshold*denominator;
			decimal scale=1m;
			for(int decimals=1;decimals<=PERCENT_DECIMALS_MAX;decimals++) {
				scale*=10m;
				decimal value=Math.Floor((decimal)numerator*100m*scale/denominator)/scale;
				if(!isAbove || value>percentThreshold || decimals==PERCENT_DECIMALS_MAX) {
					return value.ToString("0."+new string('#',decimals),CultureInfo.CurrentCulture)+"%";
				}
			}
			return "";//Not reached.
		}

		///<summary>The progress line when a refresh ends. After an error: where the refresh stopped (the measure being calculated, its number and
		///name, or before the first), how many were calculated, and the error's message on one line, which stays shown after the error window is
		///closed; never "Done". Cancelled: "Cancelled after N measures." Otherwise "Done: N measures." countDone is the number calculated,
		///indexStarted the measure being calculated (-1 before the first), countTotal the stage's count.</summary>
		public static string GetProgressEndText(Exception exception,bool isCancelled,int countDone,int indexStarted,int countTotal,string titleStarted) {
			string count=countDone.ToString(CultureInfo.InvariantCulture);
			if(exception!=null) {
				string message=System.Text.RegularExpressions.Regex.Replace(exception.Message??"",@"\s+"," ").Trim();
				if(indexStarted<0) {
					return "Stopped with an error before the first measure; none was calculated. Error: "+message;
				}
				return "Stopped with an error at measure "+(indexStarted+1).ToString(CultureInfo.InvariantCulture)+" of "
					+countTotal.ToString(CultureInfo.InvariantCulture)+" ("+titleStarted+"); "+count+" measures were calculated. Error: "+message;
			}
			if(isCancelled) {
				return "Cancelled after "+count+" measures.";
			}
			return "Done: "+count+" measures.";
		}

		///<summary>A numerator or denominator: "not calculated" for -1.</summary>
		public static string GetCountText(int count) {
			if(count<0) {
				return NOT_CALCULATED;
			}
			return count.ToString();
		}

		///<summary>The exclusion count: empty for -1, which the engine returns when no count applies to the measure.</summary>
		public static string GetExclusionCountText(int exclusionCount) {
			if(exclusionCount<0) {
				return "";
			}
			return exclusionCount.ToString();
		}

		///<summary>The measure's name: MeasureTypeTitleHelper for Modified Stage 2, as the EHR window does, otherwise the type's name.</summary>
		public static string GetMeasureTitle(EhrMeasureStage ehrMeasureStage,EhrMeasureType ehrMeasureType) {
			if(ehrMeasureStage==EhrMeasureStage.ModifiedStage2) {
				return EhrMeasures.MeasureTypeTitleHelper(ehrMeasureType);
			}
			return ehrMeasureType.ToString();
		}

		///<summary>The year whose Modified Stage 2 requirements GetRequirement gives for a reporting year: that year from 2015 on; before 2015, when
		///Modified Stage 2 did not exist, 2017's (the engine's thresholds, which GetRequirement keeps for every year outside 2015 and 2016).</summary>
		public static int GetRequirementsYearShown(int reportingYear) {
			if(reportingYear<2015) {
				return 2017;
			}
			return reportingYear;
		}

		///<summary>The notes under the provider and the dates for a refresh. Modified Stage 2: one statement, when the period spans two calendar
		///years or starts before 2015, of the year whose requirements are shown (GetRequirementsYearShown), with the two-year warning in it; and the
		///alternate exclusions that are not modelled. Every stage: the rows that depend on the removed patient portal (IsPortalMeasure), by the
		///names the grid shows.</summary>
		public static List<string> GetPeriodNotes(EhrMeasureStage ehrMeasureStage,DateTime dateStart,DateTime dateEnd) {
			List<string> listNotes=new List<string>();
			if(ehrMeasureStage==EhrMeasureStage.ModifiedStage2) {
				bool isTwoYears=dateStart.Year!=dateEnd.Year;
				bool isBefore2015=dateStart.Year<2015;
				if(isTwoYears || isBefore2015) {
					string yearShown=GetRequirementsYearShown(GetReportingYear(dateStart)).ToString(CultureInfo.InvariantCulture);
					string note="";
					if(isTwoYears) {
						note+="Warning: the reporting period spans two calendar years, and a Modified Stage 2 reporting period lies within one. ";
					}
					if(isBefore2015) {
						note+="Modified Stage 2 began in 2015. The requirements of "+yearShown+" are shown.";
					}
					else {
						note+="The requirements of "+yearShown+", the year of the start date, are shown.";
					}
					listNotes.Add(note);
				}
				listNotes.Add("The alternate exclusions of 2015 for providers who were scheduled to report Stage 1 are not modelled.");
			}
			List<string> listPortalTitles=EhrMeasures.GetStageMeasureTypes(ehrMeasureStage).Where(x => IsPortalMeasure(ehrMeasureStage,x))
				.Select(x => GetMeasureTitle(ehrMeasureStage,x)).ToList();
			if(listPortalTitles.Count==1) {
				listNotes.Add(listPortalTitles[0]+" counts what the patient portal recorded, which this program no longer has; unless older data meets "
					+"it, its Percent says \""+DEPENDS_ON_REMOVED_PORTAL+"\".");
			}
			else if(listPortalTitles.Count>1) {
				listNotes.Add(string.Join(", ",listPortalTitles.Take(listPortalTitles.Count-1))+" and "+listPortalTitles.Last()+" count what the patient "
					+"portal recorded (online access given, views, downloads and transmissions, secure messages), which this program no longer has; "
					+"unless older data meets one, its Percent says \""+DEPENDS_ON_REMOVED_PORTAL+"\".");
			}
			return listNotes;
		}

		///<summary>The provider list from GetReportingProviders (ProvNum, KeyGroup) and the providers: one entry per KeyGroup, in the order of its
		///first provider; providers missing from the list are left out, and so is an entry with none left.</summary>
		public static List<EhrMeasureCalcProvider> GetProviderChoices(DataTable tableReporting,List<Provider> listProviders,long provNumCurUser) {
			List<EhrMeasureCalcProvider> listEhrMeasureCalcProviders=new List<EhrMeasureCalcProvider>();
			Dictionary<long,EhrMeasureCalcProvider> dictByKeyGroup=new Dictionary<long,EhrMeasureCalcProvider>();
			Dictionary<long,Provider> dictProviders=listProviders.GroupBy(x => x.ProvNum).ToDictionary(x => x.Key,x => x.First());
			for(int i=0;i<tableReporting.Rows.Count;i++) {
				long provNum=PIn.Long(tableReporting.Rows[i]["ProvNum"].ToString(),hasExceptions:false);
				long keyGroup=PIn.Long(tableReporting.Rows[i]["KeyGroup"].ToString(),hasExceptions:false);
				if(!dictProviders.ContainsKey(provNum)) {
					continue;
				}
				if(!dictByKeyGroup.TryGetValue(keyGroup,out EhrMeasureCalcProvider ehrMeasureCalcProvider)) {
					ehrMeasureCalcProvider=new EhrMeasureCalcProvider();
					dictByKeyGroup.Add(keyGroup,ehrMeasureCalcProvider);
					listEhrMeasureCalcProviders.Add(ehrMeasureCalcProvider);
				}
				ehrMeasureCalcProvider.ListProvNums.Add(provNum);
			}
			for(int i=0;i<listEhrMeasureCalcProviders.Count;i++) {
				EhrMeasureCalcProvider ehrMeasureCalcProvider=listEhrMeasureCalcProviders[i];
				ehrMeasureCalcProvider.ListProvNums.Sort();
				List<Provider> listProvidersEntry=ehrMeasureCalcProvider.ListProvNums.Select(x => dictProviders[x]).ToList();
				Provider provider=listProvidersEntry.FirstOrDefault(x => x.ProvNum==provNumCurUser && provNumCurUser!=0)
					?? listProvidersEntry.FirstOrDefault(x => !x.IsHidden)
					?? listProvidersEntry[0];
				ehrMeasureCalcProvider.ProvNum=provider.ProvNum;
				ehrMeasureCalcProvider.Text=listProvidersEntry[0].LName+", "+listProvidersEntry[0].FName
					+" ("+string.Join(", ",listProvidersEntry.Select(x => x.Abbr))+")";
			}
			return listEhrMeasureCalcProviders;
		}

		///<summary>The entry of the logged-on user's provider, otherwise the first; -1 for an empty list.</summary>
		public static int GetDefaultProviderIndex(List<EhrMeasureCalcProvider> listEhrMeasureCalcProviders,long provNumCurUser) {
			if(listEhrMeasureCalcProviders.Count==0) {
				return -1;
			}
			int index=listEhrMeasureCalcProviders.FindIndex(x => provNumCurUser!=0 && x.ListProvNums.Contains(provNumCurUser));
			return Math.Max(index,0);
		}

		///<summary>The lines under the printout's title: provider, reporting period, stage.</summary>
		public static List<string> GetPrintHeading(string provText,DateTime dateStart,DateTime dateEnd,string stageText) {
			return new List<string>() {
				"Provider: "+provText,
				"Reporting period: "+dateStart.ToShortDateString()+" to "+dateEnd.ToShortDateString(),
				"Stage: "+stageText
			};
		}

		///<summary>True when a window of the chain (the detail, then Measure Calc) closed with OK and a patient: the EHR window then closes with
		///PatientSelect for that patient.</summary>
		public static bool IsPatientPicked(DialogResult dialogResult,long selectedPatNum) {
			return dialogResult==DialogResult.OK && selectedPatNum!=0;
		}

		///<summary>The patient of an audit table row (the engine's PatNum column), or 0.</summary>
		public static long GetAuditPatNum(DataRow dataRow) {
			if(dataRow==null || !dataRow.Table.Columns.Contains("PatNum")) {
				return 0;
			}
			return PIn.Long(dataRow["PatNum"].ToString(),hasExceptions:false);
		}
	}
}
