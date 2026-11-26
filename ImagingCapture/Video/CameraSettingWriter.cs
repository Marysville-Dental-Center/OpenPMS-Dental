// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using ImagingCapture.Camera;
using ImagingCapture.Settings;
using ImagingCapture.Uvc;
using ImagingDeviceManager.Config;

namespace ImagingCapture.Video {
	///<summary>The one place the program writes a camera setting (design 8.7, decision 13 of 2026-11-26): the values saved in capture.ini
	///[Camera.Settings] when the camera opens (ApplySaved), the camera buttons' setting actions (CameraSettingActions) and the Camera settings
	///window (CameraSettingsModel). A unit test checks that IFrameSource.WriteControl is called from nowhere else. Every write is logged by its
	///caller with the setting's name and its old and new value as people read them.</summary>
	public static class CameraSettingWriter {
		///<summary>Writes value (manual) or the camera's automatic mode. Closed without a frame source.</summary>
		public static UvcControlValue Write(IFrameSource frameSource,CameraSettingDef cameraSettingDef,int value,bool isAuto) {
			if(frameSource==null || cameraSettingDef==null) {
				return new UvcControlValue(UvcControlStatus.Closed,value,0,0);
			}
			return frameSource.WriteControl(cameraSettingDef.Interface,cameraSettingDef.Property,value,isAuto);
		}

		///<summary>"" when the write worked, else " failed (Unsupported, 0x80070490)" for the log.</summary>
		public static string GetFailureText(UvcControlValue controlValueWritten) {
			if(controlValueWritten.IsOk) {
				return "";
			}
			return " failed ("+controlValueWritten.Status+", "+UvcErrors.FormatHResult(controlValueWritten.HResult)+")";
		}

		///<summary>True when the control value says the camera sets it automatically.</summary>
		public static bool IsAuto(UvcControlValue controlValue) {
			return (controlValue.Flags & UvcControls.FlagsAuto)!=0;
		}

		///<summary>Applies [Camera.Settings] to the open camera: each saved value the camera supports, except a control the camera's buttons
		///use. A value the camera already has is not written. Returns one log line for each write, each value left alone and why, and each
		///refusal; the other values still apply. Never throws.</summary>
		public static List<string> ApplySaved(IFrameSource frameSource,LocalCaptureConfig config,CameraConfig cameraConfig) {
			List<string> listLines=new List<string>();
			if(frameSource==null || config==null) {
				return listLines;
			}
			try {
				List<string> listUnknown=new List<string>();
				List<KeyValuePair<CameraSettingDef,string>> listSaved=CameraSettingsFile.Read(config,listUnknown);
				foreach(string key in listUnknown) {
					listLines.Add("["+CameraSettingsFile.Section+"] "+key+" is not a camera setting and was left out.");
				}
				HashSet<ControlId> hashSetButtonControls=CameraSettingsFile.GetButtonControls(cameraConfig);
				foreach(KeyValuePair<CameraSettingDef,string> pair in listSaved) {
					CameraSettingDef cameraSettingDef=pair.Key;
					string label="Camera setting "+cameraSettingDef.DisplayName;
					if(hashSetButtonControls.Contains(CameraSettingsFile.GetControl(cameraSettingDef))) {
						listLines.Add(label+" ("+pair.Value+" in capture.ini) was left alone: the camera's buttons are read from it.");
						continue;
					}
					UvcControlRange controlRange=frameSource.ReadControlRange(cameraSettingDef.Interface,cameraSettingDef.Property);
					UvcControlValue controlValue=frameSource.ReadControl(cameraSettingDef.Interface,cameraSettingDef.Property);
					if(!controlRange.IsOk || !controlValue.IsOk) {
						listLines.Add(label+" ("+pair.Value+" in capture.ini) was not applied: this camera does not offer it ("
							+(controlRange.IsOk ? controlValue.Status : controlRange.Status)+").");
						continue;
					}
					int value;
					bool isAuto;
					string error;
					if(!CameraSettingCatalog.TryParse(cameraSettingDef,controlRange,pair.Value,out value,out isAuto,out error)) {
						listLines.Add(label+" ("+pair.Value+" in capture.ini) was not applied: "+error);
						continue;
					}
					bool isAutoOld=IsAuto(controlValue);
					if(isAuto==isAutoOld && (isAuto || value==controlValue.Value)) {
						continue;
					}
					UvcControlValue controlValueWritten=Write(frameSource,cameraSettingDef,value,isAuto);
					listLines.Add(label+": "+CameraSettingCatalog.Format(cameraSettingDef,controlRange,controlValue.Value,isAutoOld)+" -> "
						+CameraSettingCatalog.Format(cameraSettingDef,controlRange,value,isAuto)+" (saved in capture.ini)"
						+(controlValueWritten.IsOk ? "." : GetFailureText(controlValueWritten)+": the camera refused it."));
				}
			}
			catch(Exception ex) {
				listLines.Add("Applying the saved camera settings failed: "+ex.Message);
			}
			return listLines;
		}
	}
}
