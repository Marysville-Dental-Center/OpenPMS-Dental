// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using ImagingCapture.Settings;
using ImagingCapture.Uvc;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using WpfControls.UI;

namespace ImagingCapture.UI {
	///<summary>Camera picture settings (design 8.7): every setting the open camera's driver offers, by name and with readable values, opened from
	///Camera settings in the camera session. All decisions are in CameraSettingsModel; this window shows it. Opening and saving need Setup.</summary>
	public partial class FrmCameraSettingsControls:FrmODBase {
		#region Fields
		private static readonly Color _colorReadOnly=Color.FromRgb(110,110,110);
		private readonly CameraSettingsModel _model;
		private int _idxSelected=-1;
		///<summary>The values behind comboValue's items, in order.</summary>
		private List<string> _listComboValues=new List<string>();
		private bool _isFilling;
		#endregion Fields

		public FrmCameraSettingsControls(CameraSettingsModel model) {
			InitializeComponent();
			_model=model??throw new ArgumentNullException(nameof(model));
			Load+=FrmCameraSettingsControls_Load;
			FormClosing+=FrmCameraSettingsControls_FormClosing;
			gridSettings.SelectionCommitted+=gridSettings_SelectionCommitted;
			gridSettings.CellClick+=gridSettings_CellClick;
			comboValue.SelectionChangeCommitted+=comboValue_SelectionChangeCommitted;
		}

		///<summary>True after a save in this window.</summary>
		public bool IsSaved { get; private set; }

		#region Methods - Event Handlers
		private void FrmCameraSettingsControls_Load(object sender,EventArgs e) {
			Lang.F(this);
			_model.Load();
			_idxSelected=_model.ListRows.Count>0 ? 0 : -1;
			RefreshView();
		}

		private void FrmCameraSettingsControls_FormClosing(object sender,CancelEventArgs e) {
			if(!_model.HasChanges) {
				return;
			}
			if(!MsgBox.Show(MsgBoxButtons.OKCancel,Lang.g(this,"Close without saving? The camera gets back the values it had."))) {
				e.Cancel=true;
				return;
			}
			List<string> listErrors=_model.UndoChanges();
			if(listErrors.Count>0) {
				MsgBox.Show(Lang.g(this,"Some values could not be put back:")+"\r\n"+string.Join("\r\n",listErrors));
			}
		}

		private void gridSettings_SelectionCommitted(object sender,EventArgs e) {
			SelectRow(gridSettings.GetSelectedIndex());
		}

		private void gridSettings_CellClick(object sender,GridClickEventArgs e) {
			SelectRow(gridSettings.GetSelectedIndex());
		}

		private void comboValue_SelectionChangeCommitted(object sender,EventArgs e) {
			int idx=comboValue.SelectedIndex;
			if(_isFilling || _idxSelected<0 || idx<0 || idx>=_listComboValues.Count) {
				return;
			}
			string error;
			_model.SetText(_idxSelected,_listComboValues[idx],out error);
			labelError.Text=error;
			RefreshView();
		}

		private void butApply_Click(object sender,EventArgs e) {
			if(_idxSelected<0) {
				return;
			}
			string error;
			_model.SetText(_idxSelected,textValue.Text,out error);
			labelError.Text=error;
			RefreshView();
		}

		private void butDefault_Click(object sender,EventArgs e) {
			string error;
			_model.SetCameraDefault(_idxSelected,out error);
			labelError.Text=error;
			RefreshView();
		}

		private void butForget_Click(object sender,EventArgs e) {
			CameraSettingRow row=GetRowSelected();
			if(row==null) {
				return;
			}
			_model.SetForget(_idxSelected,!row.IsForgetPending);
			RefreshView();
		}

		private void butUndo_Click(object sender,EventArgs e) {
			List<string> listErrors=_model.UndoChanges();
			labelError.Text=string.Join(" ",listErrors);
			RefreshView();
		}

		private void butSave_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(EnumPermType.Setup)) {
				return;
			}
			List<CaptureSettingChange> listChanges=_model.GetPendingChanges();
			if(listChanges.Count==0) {
				MsgBox.Show(Lang.g(this,"Nothing to save: no value was changed here."));
				return;
			}
			string question=Lang.g(this,"Save these camera settings to capture.ini on this computer? They are applied each time the camera opens.")
				+"\r\n\r\n"+string.Join("\r\n",listChanges.Select(x => x.Key+": "+(x.OldValue=="" ? Lang.g(this,"(not saved)") : x.OldValue)+" -> "
					+(x.NewValue=="" ? Lang.g(this,"(not saved: the camera keeps its own)") : x.NewValue)))
				+"\r\n\r\n"+Lang.g(this,"The save is recorded in the security log.");
			if(!MsgBox.Show(MsgBoxButtons.OKCancel,question)) {
				return;
			}
			CameraSettingsSaveResult result=_model.Save();
			if(result.IsSaved) {
				IsSaved=true;
			}
			RefreshView();
			if(!result.IsSaved || !result.IsLogged) {
				MsgBox.Show(result.Error);
				return;
			}
			MsgBox.Show(Lang.g(this,"The camera settings were saved and recorded in the security log."));
		}

		private void butClose_Click(object sender,EventArgs e) {
			Close();
		}
		#endregion Methods - Event Handlers

		#region Methods - view
		private CameraSettingRow GetRowSelected() {
			return _idxSelected>=0 && _idxSelected<_model.ListRows.Count ? _model.ListRows[_idxSelected] : null;
		}

		private void SelectRow(int idx) {
			if(idx<0 || idx>=_model.ListRows.Count || idx==_idxSelected) {
				return;
			}
			_idxSelected=idx;
			labelError.Text="";
			FillEditor();
		}

		private void RefreshView() {
			_isFilling=true;
			try {
				labelCamera.Text=Lang.g(this,"Camera")+": "+_model.CameraName+". "+Lang.g(this,"This computer")+": "+_model.ComputerName+".";
				FillGrid();
				if(_idxSelected>=0) {
					gridSettings.SetSelected(_idxSelected);
				}
				FillEditor();
				butSave.IsEnabled=_model.HasChanges;
				butUndo.IsEnabled=_model.HasChanges;
			}
			finally {
				_isFilling=false;
			}
		}

		private void FillGrid() {
			gridSettings.BeginUpdate();
			gridSettings.Columns.Clear();
			gridSettings.Columns.Add(new GridColumn(Lang.g(this,"Setting"),170));
			gridSettings.Columns.Add(new GridColumn(Lang.g(this,"Value"),100));
			gridSettings.Columns.Add(new GridColumn(Lang.g(this,"Camera default"),100));
			gridSettings.Columns.Add(new GridColumn(Lang.g(this,"Range"),190));
			gridSettings.Columns.Add(new GridColumn(Lang.g(this,"Saved"),90));
			GridColumn gridColumnNote=new GridColumn(Lang.g(this,"Note"),200);
			gridColumnNote.IsWidthDynamic=true;
			gridSettings.Columns.Add(gridColumnNote);
			gridSettings.ListGridRows.Clear();
			foreach(CameraSettingRow row in _model.ListRows) {
				GridRow gridRow=new GridRow();
				gridRow.Cells.Add(row.Def.DisplayName);
				gridRow.Cells.Add(row.Text);
				gridRow.Cells.Add(row.DefaultText);
				gridRow.Cells.Add(row.RangeText);
				gridRow.Cells.Add(row.SavedText??"");
				gridRow.Cells.Add(GetNote(row));
				if(row.IsButtonControl) {
					gridRow.ColorText=_colorReadOnly;
				}
				gridRow.Bold=row.IsChanged || row.IsForgetPending;
				gridSettings.ListGridRows.Add(gridRow);
			}
			gridSettings.EndUpdate();
		}

		private string GetNote(CameraSettingRow row) {
			List<string> listNotes=new List<string>();
			if(row.IsButtonControl) {
				listNotes.Add(Lang.g(this,"used by the camera's buttons"));
			}
			if(row.IsChanged) {
				listNotes.Add(Lang.g(this,"changed, was")+" "+CameraSettingCatalog.Format(row.Def,row.Range,row.ValueFound,row.IsAutoFound));
			}
			if(row.IsForgetPending) {
				listNotes.Add(Lang.g(this,"Save removes the saved value"));
			}
			return string.Join("; ",listNotes);
		}

		///<summary>A list for On/Off, choices and Auto; a text box for a number.</summary>
		private void FillEditor() {
			bool isFillingBefore=_isFilling;
			_isFilling=true;
			try {
				CameraSettingRow row=GetRowSelected();
				bool isList=row!=null && row.Kind!=CameraSettingKind.Number;
				comboValue.Visible=isList;
				textValue.Visible=row!=null && !isList;
				butApply.Visible=row!=null && !isList;
				bool isEditable=row!=null && !row.IsButtonControl;
				comboValue.IsEnabled=isEditable;
				textValue.IsEnabled=isEditable;
				butApply.IsEnabled=isEditable;
				butDefault.IsEnabled=isEditable;
				butForget.IsEnabled=row!=null && row.SavedText!=null;
				butForget.Text=row!=null && row.IsForgetPending ? Lang.g(this,"Keep saved") : Lang.g(this,"Don't save");
				if(row!=null && isList) {
					_listComboValues=CameraSettingCatalog.GetChoices(row.Def,row.Range);
					if(!_listComboValues.Contains(row.Text)) {
						_listComboValues.Add(row.Text);
					}
					comboValue.Items.Clear();
					foreach(string value in _listComboValues) {
						comboValue.Items.Add(value);
					}
					comboValue.SelectedIndex=_listComboValues.IndexOf(row.Text);
				}
				else if(row!=null) {
					textValue.Text=row.Text;
				}
				if(row==null) {
					labelSelected.Text="";
					labelHelp.Text="";
					labelRange.Text="";
					return;
				}
				labelSelected.Text=row.Def.DisplayName+(row.IsButtonControl ? " ("+Lang.g(this,"used by the camera's buttons")+")" : "");
				labelHelp.Text=row.Def.Help+(row.Kind==CameraSettingKind.Number && row.Range.CanAuto ? " "+Lang.g(this,"Type Auto to let the camera set it.") : "");
				labelRange.Text=Lang.g(this,"Range")+": "+row.RangeText+"; "+Lang.g(this,"camera default")+": "+row.DefaultText
					+(row.SavedText==null ? "" : "; "+Lang.g(this,"saved")+": "+row.SavedText);
			}
			finally {
				_isFilling=isFillingBefore;
			}
		}
		#endregion Methods - view
	}
}
