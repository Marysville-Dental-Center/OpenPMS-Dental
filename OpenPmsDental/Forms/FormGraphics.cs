using System;
using System.Windows.Forms;
using OpenPmsDentalBusiness;
using CodeBase;
using OpenPmsDental.UI;

namespace OpenPmsDental{
	///<summary>File > Graphics, the computer's tooth chart setting, rebuilt in TC-8 (docs/specs/wpf-tooth-chart-plan.md, the TC-8 row): two choices,
	///"3D chart" (the WPF chart) and "Simple 2D chart", the per-computer "Draw in software" check box of the 3D chart, and a live preview of what
	///the choices give on this computer. No DirectX device format grid and no OpenGL pixel format grid: the old engines no longer draw. Saving
	///follows the relay's save rule (ToothChartRelay.SetGraphicsChoice); the chart settings themselves are the relay's, so this window names no
	///engine. Opened from Setup > Computers for another computer, it saves without testing, as before.</summary>
	public partial class FormGraphics : FormODBase {
		public ComputerPref ComputerPrefCur;
		///<summary>True when we are editing another computers ComputerPrefs.</summary>
		private bool _isRemoteEdit;
		///<summary>True once Load has set the choices, so the preview is drawn only from then on.</summary>
		private bool _isLoaded;

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
			//A stored DirectX (0) or OpenGL (2) choice starts the 3D chart, and Simple 2D (1) stays the 2D chart (O-30).
			radio3DChart.Checked=ToothChartRelay.Is3DChart(ComputerPrefCur);
			radioSimpleChart.Checked=!radio3DChart.Checked;
			checkDrawInSoftware.Checked=ToothChartRelay.IsDrawnInSoftware(ComputerPrefCur);
			if(ComputerPrefCur.ComputerOS==PlatformOD.Unix) {//Force simple mode on Unix systems.
				MsgBox.Show(this,"Linux users must use simple tooth chart.");
				radio3DChart.Enabled=false;
				radioSimpleChart.Checked=true;
			}
			if(_isRemoteEdit) {
				labelPreview.Text=Lan.g(this,"The preview is drawn by this computer, not by the computer being edited.");
			}
			_isLoaded=true;
			RefreshPreview();
		}

		///<summary>Draws the preview with the window's choices (ToothChartRelay.ApplyGraphicsPreview), so a computer whose graphics show no 3D chart shows
		///it at once, and says which chart started. The check box applies to the 3D chart only.</summary>
		private void RefreshPreview() {
			if(!_isLoaded) {
				return;
			}
			checkDrawInSoftware.Enabled=radio3DChart.Checked;
			ToothChartRelay.ApplyGraphicsPreview(toothChartWrapper,radio3DChart.Checked,checkDrawInSoftware.Checked);
			labelPreviewStatus.Text=Lan.g(this,ToothChartRelay.GetGraphicsPreviewStatus(toothChartWrapper,radio3DChart.Checked));
		}

		private void radio3DChart_Click(object sender,EventArgs e) {
			RefreshPreview();
		}

		private void radioSimpleChart_Click(object sender,EventArgs e) {
			RefreshPreview();
		}

		private void checkDrawInSoftware_Click(object sender,EventArgs e) {
			RefreshPreview();
		}

		private void butSave_Click(object sender,System.EventArgs e) {
			//ComputerPrefCur doesn't change until here, so make the old copy exactly when we need it instead of when the form is created.
			ComputerPref computerPrefOld=ComputerPrefCur.Copy();
			//"Simple 2D chart" writes 1; "3D chart" writes 0 only when the stored value was 1, so a stored 0 or 2 is kept; "Draw in software" writes or
			//clears its value in DirectXFormat. Nothing is tested first: the 3D chart falls back to the 2D chart on its own, logged once.
			ToothChartRelay.SetGraphicsChoice(ComputerPrefCur,radio3DChart.Checked,checkDrawInSoftware.Checked);
			ComputerPrefs.Update(ComputerPrefCur,computerPrefOld);
			DialogResult=DialogResult.OK;
		}

	}
}
