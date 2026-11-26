// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using ImagingCapture.Camera;
using ImagingCapture.Uvc;
using ImagingCapture.Video;
using ImagingDeviceManager.Config;
using OpenPmsDentalBusiness;

namespace ImagingCapture.Settings {
	///<summary>One camera setting the open camera offers, as the Camera settings window shows it.</summary>
	public sealed class CameraSettingRow {
		public CameraSettingDef Def { get; set; }
		public UvcControlRange Range { get; set; }
		///<summary>The value when the window read the camera (or last saved): Close puts it back.</summary>
		public int ValueFound { get; set; }
		public bool IsAutoFound { get; set; }
		///<summary>The camera's value now.</summary>
		public int Value { get; set; }
		public bool IsAuto { get; set; }
		///<summary>The value in [Camera.Settings], as written; null when none.</summary>
		public string SavedText { get; set; }
		///<summary>The camera's buttons are read from this control: shown, never written.</summary>
		public bool IsButtonControl { get; set; }
		///<summary>Save removes the saved value, so the camera keeps its own.</summary>
		public bool IsForgetPending { get; set; }

		///<summary>The value as people read it: "20", "Auto", "1/64 s", "60 Hz".</summary>
		public string Text => CameraSettingCatalog.Format(Def,Range,Value,IsAuto);
		///<summary>The value as Save writes it.</summary>
		public string FileText => CameraSettingCatalog.Format(Def,Range,Value,IsAuto,forFile:true);
		public string DefaultText => CameraSettingCatalog.Format(Def,Range,Range.Default,false);
		public string RangeText => CameraSettingCatalog.FormatRange(Def,Range);
		public CameraSettingKind Kind => CameraSettingCatalog.GetKind(Def,Range);
		///<summary>Changed on the camera since it was read or saved.</summary>
		public bool IsChanged => Value!=ValueFound || IsAuto!=IsAutoFound;
	}

	///<summary>What Save did.</summary>
	public sealed class CameraSettingsSaveResult {
		public bool IsSaved { get; set; }
		public bool IsLogged { get; set; }
		public string Error { get; set; }="";
		public List<CaptureSettingChange> ListChanges { get; set; }=new List<CaptureSettingChange>();
	}

	///<summary>The Camera settings window (design 8.7, decision 13 of 2026-11-26): every setting the open camera's driver offers, the same ones
	///its own property pages show, by name, with values people read (CameraSettingCatalog). A change goes to the camera at once, so the preview
	///shows it, as the driver's pages do; Undo and Close put back the values the window found. Save writes the values changed here to capture.ini
	///[Camera.Settings] (CameraSettingsFile), which the session applies each time the camera opens. Save needs the Setup permission and writes a
	///Setup security-log entry with the old and new values, as the sensor's Capture settings do. A control the camera's buttons are read from
	///is shown read-only. Every write goes through CameraSettingWriter and is logged. No database here: the store is the seam.</summary>
	public sealed class CameraSettingsModel {
		public const string LogPrefix="Camera settings saved: ";
		private readonly IFrameSource _frameSource;
		private readonly LocalCaptureConfig _config;
		private readonly HashSet<ControlId> _hashSetButtonControls;
		private readonly ICaptureSettingsStore _store;
		private readonly Action<string> _actionLog;
		private List<CameraSettingRow> _listRows=new List<CameraSettingRow>();

		///<param name="frameSource">The open camera.</param>
		///<param name="config">The session's capture.ini, which Save changes and saves.</param>
		///<param name="cameraConfig">The camera keys, for the controls the buttons use.</param>
		///<param name="cameraName">The camera's name, for the log.</param>
		///<param name="computerName">This computer, for the log.</param>
		public CameraSettingsModel(IFrameSource frameSource,LocalCaptureConfig config,CameraConfig cameraConfig,string cameraName,string computerName,
			ICaptureSettingsStore store,Action<string> actionLog)
		{
			_frameSource=frameSource??throw new ArgumentNullException(nameof(frameSource));
			_config=config??throw new ArgumentNullException(nameof(config));
			_hashSetButtonControls=CameraSettingsFile.GetButtonControls(cameraConfig);
			CameraName=cameraName??"";
			ComputerName=computerName??"";
			_store=store??throw new ArgumentNullException(nameof(store));
			_actionLog=actionLog;
		}

		public string CameraName { get; }
		public string ComputerName { get; }
		public IReadOnlyList<CameraSettingRow> ListRows => _listRows;
		///<summary>A value changed on the camera or a saved value to remove since the last read or save.</summary>
		public bool HasChanges => _listRows.Any(x => x.IsChanged || x.IsForgetPending);
		public string FilePath => _config.PathLoaded??_config.PathPrimary??"";

		///<summary>Reads every catalog setting the camera offers (its range and value read); the others are left out.</summary>
		public void Load() {
			_listRows=new List<CameraSettingRow>();
			foreach(CameraSettingDef cameraSettingDef in CameraSettingCatalog.All) {
				UvcControlRange controlRange=_frameSource.ReadControlRange(cameraSettingDef.Interface,cameraSettingDef.Property);
				UvcControlValue controlValue=_frameSource.ReadControl(cameraSettingDef.Interface,cameraSettingDef.Property);
				if(!controlRange.IsOk || !controlValue.IsOk) {
					continue;
				}
				bool isAuto=CameraSettingWriter.IsAuto(controlValue);
				_listRows.Add(new CameraSettingRow {
					Def=cameraSettingDef,
					Range=controlRange,
					ValueFound=controlValue.Value,
					IsAutoFound=isAuto,
					Value=controlValue.Value,
					IsAuto=isAuto,
					SavedText=CameraSettingsFile.GetSaved(_config,cameraSettingDef),
					IsButtonControl=_hashSetButtonControls.Contains(CameraSettingsFile.GetControl(cameraSettingDef)),
				});
			}
		}

		#region Methods - editing
		///<summary>Sets a row to text as people write it ("20", "Auto", "1/64 s"): checks it, writes it to the camera and reads it back. False with
		///error when the text is not a value, the row is read-only or the camera refused it.</summary>
		public bool SetText(int idx,string text,out string error) {
			error="";
			CameraSettingRow row=GetRow(idx);
			if(row==null) {
				error="No setting is selected.";
				return false;
			}
			int value;
			bool isAuto;
			if(!CameraSettingCatalog.TryParse(row.Def,row.Range,text,out value,out isAuto,out error)) {
				return false;
			}
			return WriteRow(row,value,isAuto,"window",out error);
		}

		///<summary>Sets a row to the camera's default value.</summary>
		public bool SetCameraDefault(int idx,out string error) {
			CameraSettingRow row=GetRow(idx);
			if(row==null) {
				error="No setting is selected.";
				return false;
			}
			return WriteRow(row,row.Range.Default,false,"camera default",out error);
		}

		///<summary>Marks a row's saved value for removal on Save (true), or keeps it (false). Only for a row with a saved value.</summary>
		public bool SetForget(int idx,bool isForget) {
			CameraSettingRow row=GetRow(idx);
			if(row==null || row.SavedText==null) {
				return false;
			}
			row.IsForgetPending=isForget;
			return true;
		}

		///<summary>Puts back every value changed since the camera was read or saved, and keeps the saved values. Returns the problems.</summary>
		public List<string> UndoChanges() {
			List<string> listErrors=new List<string>();
			foreach(CameraSettingRow row in _listRows) {
				row.IsForgetPending=false;
				if(!row.IsChanged) {
					continue;
				}
				string error;
				if(!WriteRow(row,row.ValueFound,row.IsAutoFound,"put back",out error)) {
					listErrors.Add(row.Def.DisplayName+": "+error);
				}
			}
			return listErrors;
		}

		private CameraSettingRow GetRow(int idx) {
			return idx>=0 && idx<_listRows.Count ? _listRows[idx] : null;
		}

		private bool WriteRow(CameraSettingRow row,int value,bool isAuto,string why,out string error) {
			error="";
			if(row.IsButtonControl) {
				error="The camera's buttons are read from "+row.Def.DisplayName+", so it is not changed here.";
				return false;
			}
			if(isAuto==row.IsAuto && (isAuto || value==row.Value)) {
				return true;
			}
			string textOld=row.Text;
			UvcControlValue controlValueWritten=CameraSettingWriter.Write(_frameSource,row.Def,value,isAuto);
			UvcControlValue controlValueRead=_frameSource.ReadControl(row.Def.Interface,row.Def.Property);
			if(controlValueRead.IsOk) {
				row.Value=controlValueRead.Value;
				row.IsAuto=CameraSettingWriter.IsAuto(controlValueRead);
			}
			Log("Camera setting "+row.Def.DisplayName+" ("+why+"): "+textOld+" -> "+CameraSettingCatalog.Format(row.Def,row.Range,value,isAuto)
				+(controlValueWritten.IsOk ? "." : CameraSettingWriter.GetFailureText(controlValueWritten)+"."));
			if(!controlValueWritten.IsOk) {
				error="The camera refused it ("+controlValueWritten.Status+").";
				return false;
			}
			return true;
		}
		#endregion Methods - editing

		#region Methods - save
		///<summary>What Save writes: each value changed here (section Camera.Settings, key the setting's id) and each saved value removed.</summary>
		public List<CaptureSettingChange> GetPendingChanges() {
			List<CaptureSettingChange> listChanges=new List<CaptureSettingChange>();
			foreach(CameraSettingRow row in _listRows) {
				string valueNew;
				if(row.IsForgetPending) {
					valueNew="";
				}
				else if(row.IsChanged && !row.IsButtonControl) {
					valueNew=row.FileText;
				}
				else {
					continue;
				}
				if(valueNew==(row.SavedText??"")) {
					continue;
				}
				listChanges.Add(new CaptureSettingChange { Section=CameraSettingsFile.Section,Key=row.Def.Id,OldValue=row.SavedText??"",NewValue=valueNew,
					IsGated=true });
			}
			return listChanges;
		}

		///<summary>Saves the changes to capture.ini (Setup permission) and writes a Setup security-log entry with them. Afterwards the values on the
		///camera are the ones Close keeps. Never throws.</summary>
		public CameraSettingsSaveResult Save() {
			CameraSettingsSaveResult result=new CameraSettingsSaveResult();
			try {
				if(!_store.IsAuthorized(EnumPermType.Setup)) {
					result.Error="Not authorized for Setup: the camera settings were not saved.";
					return result;
				}
				List<CaptureSettingChange> listChanges=GetPendingChanges();
				result.ListChanges=listChanges;
				foreach(CaptureSettingChange change in listChanges) {
					if(!_config.Set(change.Section,change.Key,change.NewValue)) {
						result.Error="Nothing was saved: "+_config.LastError;
						return result;
					}
				}
				if(listChanges.Count>0 && !_config.Save()) {
					result.Error="The camera settings could not be saved: "+_config.LastError;
					return result;
				}
				result.IsSaved=true;
				foreach(CameraSettingRow row in _listRows) {
					row.ValueFound=row.Value;
					row.IsAutoFound=row.IsAuto;
					row.IsForgetPending=false;
					row.SavedText=CameraSettingsFile.GetSaved(_config,row.Def);
				}
				if(listChanges.Count==0) {
					result.IsLogged=true;
					return result;
				}
				try {
					_store.MakeLogEntry(EnumPermType.Setup,0,GetLogText(listChanges),0);
					result.IsLogged=true;
				}
				catch(Exception ex) {
					result.Error="The camera settings were saved, but the security-log entry could not be written: "+ex.Message;
				}
			}
			catch(Exception ex) {
				result.Error="The camera settings could not be saved: "+ex.Message;
			}
			return result;
		}

		///<summary>"Camera settings saved: Computer=..; Camera=..; File=..; [Camera.Settings] Gain: (empty) -> 20; ...".</summary>
		public string GetLogText(List<CaptureSettingChange> listChanges) {
			return LogPrefix+"Computer="+ComputerName+"; Camera="+CameraName+"; File="+FilePath+"; "
				+string.Join("; ",(listChanges??new List<CaptureSettingChange>()).Select(x => x.GetText()));
		}
		#endregion Methods - save

		private void Log(string line) {
			try {
				_actionLog?.Invoke(line);
			}
			catch {
				//A log failure must never stop the window.
			}
		}
	}
}
