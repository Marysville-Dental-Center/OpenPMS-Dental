using System;
using System.Windows.Forms;
using OpenPmsDentalBusiness;
using CodeBase;
using OpenPmsDental.UI;

namespace OpenPmsDental{
	///<summary>File > Graphics, the tooth chart settings, on Open Dental's layout (TC-8b, docs/specs/wpf-tooth-chart-plan.md section 10, rows ENG-1,
	///ENG-2, ENG-3 and NEW-1; Open Dental's window: https://www.opendental.com/manual/graphics.html) with the WPF 3D chart in DirectX 11's place: the
	///global "3D Tooth Chart on all computers" setting, this computer's "Use the global setting / Use if available / Do not use", the status on this
	///computer ("Available", or "Fell back: " and the reason), "Older Tooth Chart Technology" with Simple 2D (the one older technology left), the
	///options with "Draw in software" (ticked by default: the smoothed picture; unticked: the quick live view), and a live preview of what the choices
	///give on this computer. The settings and the save rule are the relay's (ToothChartRelay), so this window names no engine. Opened from Setup >
	///Computers for another computer, it saves without testing, as before.</summary>
	public partial class FormGraphics : FormODBase {
		public ComputerPref ComputerPrefCur;
		///<summary>True when we are editing another computers ComputerPrefs.</summary>
		private bool _isRemoteEdit;
		///<summary>True once Load has set the choices, so the preview is drawn only from then on.</summary>
		private bool _isLoaded;
		///<summary>The global setting as Load found it, the "before" of the save rule.</summary>
		private bool _isChart3DOnAllComputersLoaded;
		///<summary>True when the computer being edited runs Linux, which keeps the Simple 2D chart.</summary>
		private bool _isUnix;

		///<summary></summary>
		public FormGraphics(){
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormGraphics_Load(object sender,EventArgs e) {
			if(ComputerPrefCur==null) {
				ComputerPrefCur=ComputerPrefs.LocalComputer;
			}
			else {
				if(ComputerPrefCur!=ComputerPrefs.LocalComputer) {
					_isRemoteEdit=true;
				}
				MsgBox.Show(this,"Warning, editing another computers graphical settings should be done from that computer to ensure the selected settings work."
					+" We do not recommend editing this way. If you make changes for another computer you should still verifiy that they work on that machine.");
					SecurityLogs.MakeLogEntry(EnumPermType.GraphicsRemoteEdit,0,"Edited graphical settings for "+ComputerPrefCur.ComputerName);
			}
			Text+=" - "+ComputerPrefCur.ComputerName;
			if(ComputerPrefCur.ComputerOS==PlatformOD.Undefined){//Remote computer has not updated yet.
				MsgBox.Show(this,"Selected computer needs to be updated before being able to remotely change its graphical settings.");
				DialogResult=DialogResult.Cancel;
				return;
			}
			//The global setting and this computer's, Open Dental's own rows (ENG-2); "Use the global setting" for anything but Yes and No.
			_isChart3DOnAllComputersLoaded=ToothChartRelay.IsChart3DOnAllComputers();
			radioChart3DAllUse.Checked=_isChart3DOnAllComputersLoaded;
			radioChart3DAllDontUse.Checked=!_isChart3DOnAllComputersLoaded;
			YN chart3DThisComputer=ToothChartRelay.GetChart3DThisComputer(ComputerPrefCur);
			radioChart3DThisUse.Checked=chart3DThisComputer==YN.Yes;
			radioChart3DThisDontUse.Checked=chart3DThisComputer==YN.No;
			radioChart3DThisGlobal.Checked=!radioChart3DThisUse.Checked && !radioChart3DThisDontUse.Checked;
			//Simple 2D is the one older technology left, so it is always the older choice.
			radioSimpleChart.Checked=true;
			checkDrawInSoftware.Checked=ToothChartRelay.IsDrawnInSoftware(ComputerPrefCur);
			FillStatus();
			if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {//Medical wording, as Open Dental's own Load words its older choices and options there.
				radioSimpleChart.Text="Use Simple Graphics";
				groupOptions.Text="Options For 3D Graphics";
				labelOptions.Text="Most users will never need to change any of these options.  These are only used when the 3D graphics are not working "
					+"properly.";
			}
			if(ComputerPrefCur.ComputerOS==PlatformOD.Unix) {//Force simple mode on Unix systems.
				MsgBox.Show(this,"Linux users must use simple tooth chart.");
				_isUnix=true;
				groupChart3DThis.Enabled=false;
			}
			if(_isRemoteEdit) {
				labelPreview.Text=Lan.g(this,"The preview is drawn by this computer, not by the computer being edited.");
			}
			_isLoaded=true;
			RefreshPreview();
		}

		///<summary>The status on this computer (NEW-1), as Open Dental's status group shows it: "Available" checked, or the second line checked and
		///saying why the 3D tooth chart fell back. It cannot be changed by hand. For another computer it is still this computer's, as Open Dental's.</summary>
		private void FillStatus() {
			string status=ToothChartRelay.GetChart3DStatus();
			if(status==ToothChartRelay.Chart3DStatusAvailable) {
				radioStatusAvailable.Checked=true;
				radioStatusFellBack.Checked=false;
				return;
			}
			radioStatusAvailable.Checked=false;
			radioStatusFellBack.Checked=true;
			radioStatusFellBack.Text=Lan.g(this,ToothChartRelay.Chart3DStatusFellBackPrefix.Trim())+" "
				+status.Substring(ToothChartRelay.Chart3DStatusFellBackPrefix.Length);
		}

		///<summary>This computer's choice in the window: Unknown for "Use the global setting".</summary>
		private YN GetChart3DThisComputerChosen() {
			if(radioChart3DThisUse.Checked) {
				return YN.Yes;
			}
			if(radioChart3DThisDontUse.Checked) {
				return YN.No;
			}
			return YN.Unknown;
		}

		///<summary>True when the window's choices use the 3D tooth chart on the computer being edited (ToothChartRelay.IsChart3DUsed).</summary>
		private bool Is3DChartChosen() {
			if(_isUnix) {
				return false;
			}
			return ToothChartRelay.IsChart3DUsed(radioChart3DAllUse.Checked,GetChart3DThisComputerChosen());
		}

		///<summary>Draws the preview with the window's choices (ToothChartRelay.ApplyGraphicsPreview), so a computer whose graphics show no 3D chart shows
		///it at once, and says which chart started. The check box applies to the 3D chart only.</summary>
		private void RefreshPreview() {
			if(!_isLoaded) {
				return;
			}
			bool is3DChart=Is3DChartChosen();
			checkDrawInSoftware.Enabled=is3DChart;
			ToothChartRelay.ApplyGraphicsPreview(toothChartWrapper,is3DChart,checkDrawInSoftware.Checked);
			labelPreviewStatus.Text=Lan.g(this,ToothChartRelay.GetGraphicsPreviewStatus(toothChartWrapper,is3DChart));
		}

		private void radioChart3D_Click(object sender,EventArgs e) {
			RefreshPreview();
		}

		private void checkDrawInSoftware_Click(object sender,EventArgs e) {
			RefreshPreview();
		}

		private void butSave_Click(object sender,System.EventArgs e) {
			bool isChart3DOnAllComputers=radioChart3DAllUse.Checked;
			//ComputerPrefCur doesn't change until here, so make the old copy exactly when we need it instead of when the form is created.
			ComputerPref computerPrefOld=ComputerPrefCur.Copy();
			//Only what a choice changes is written: this computer's 3D setting, GraphicsSimple when the choices change the chart the computer draws (1 for
			//Simple 2D; 0 over a stored 1 for the 3D chart), and the live view's value for "Draw in software" unticked. Nothing is tested first: the 3D
			//chart falls back to the 2D chart on its own, logged once.
			ToothChartRelay.SetGraphicsChoice(ComputerPrefCur,_isChart3DOnAllComputersLoaded,isChart3DOnAllComputers,GetChart3DThisComputerChosen(),
				checkDrawInSoftware.Checked);
			ComputerPrefs.Update(ComputerPrefCur,computerPrefOld);
			if(ToothChartRelay.SetChart3DOnAllComputers(isChart3DOnAllComputers)) {
				SecurityLogs.MakeLogEntry(EnumPermType.GraphicsEdit,0,"3D Tooth Chart on all computers set to "
					+(isChart3DOnAllComputers ? "Use if available" : "Do not use"));
				DataValid.SetInvalid(InvalidType.Prefs);
			}
			DialogResult=DialogResult.OK;
		}

	}
}
