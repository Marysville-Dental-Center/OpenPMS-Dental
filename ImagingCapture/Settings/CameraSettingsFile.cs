// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using ImagingCapture.Camera;
using ImagingCapture.Uvc;
using ImagingDeviceManager.Config;

namespace ImagingCapture.Settings {
	///<summary>capture.ini [Camera.Settings] (design 8.7, 9.3): the camera settings saved in the Camera settings window, one key per setting by
	///its CameraSettingCatalog id, the value as people write it: Gain=20, Exposure=Auto, Exposure=1/64 s, PowerLineFrequency=60 Hz,
	///BacklightCompensation=On. The session applies them each time the camera opens (CameraSettingWriter.ApplySaved), because many cameras
	///forget their settings when unplugged. A missing or empty key means "leave it as the camera has it".</summary>
	public static class CameraSettingsFile {
		public const string Section="Camera.Settings";

		///<summary>The saved values by setting, trimmed, in catalog order. Missing and empty keys are left out; a key that names no setting is
		///added to listUnknown when given.</summary>
		public static List<KeyValuePair<CameraSettingDef,string>> Read(LocalCaptureConfig config,List<string> listUnknown=null) {
			Dictionary<CameraSettingDef,string> dictValues=new Dictionary<CameraSettingDef,string>();
			if(config==null) {
				return new List<KeyValuePair<CameraSettingDef,string>>();
			}
			foreach(string key in config.GetKeyNames(Section)) {
				CameraSettingDef cameraSettingDef;
				if(!CameraSettingCatalog.TryGet(key,out cameraSettingDef)) {
					listUnknown?.Add(key);
					continue;
				}
				string value=config.GetString(Section,key).Trim();
				if(value!="") {
					dictValues[cameraSettingDef]=value;
				}
			}
			return CameraSettingCatalog.All.Where(x => dictValues.ContainsKey(x)).Select(x => new KeyValuePair<CameraSettingDef,string>(x,dictValues[x])).ToList();
		}

		///<summary>The saved value of one setting, or null when it has none.</summary>
		public static string GetSaved(LocalCaptureConfig config,CameraSettingDef cameraSettingDef) {
			if(config==null || cameraSettingDef==null) {
				return null;
			}
			foreach(string key in config.GetKeyNames(Section)) {
				CameraSettingDef cameraSettingDefKey;
				if(CameraSettingCatalog.TryGet(key,out cameraSettingDefKey) && cameraSettingDefKey==cameraSettingDef) {
					string value=config.GetString(Section,key).Trim();
					return value=="" ? null : value;
				}
			}
			return null;
		}

		///<summary>The controls the camera's buttons are read from ([Trigger.ControlPoll] and every enabled [Level.N]). Writing one could break
		///the buttons, so they are never written: the window shows them read-only and the saved values leave them alone.</summary>
		public static HashSet<ControlId> GetButtonControls(CameraConfig cameraConfig) {
			HashSet<ControlId> hashSetControls=new HashSet<ControlId>();
			if(cameraConfig==null) {
				return hashSetControls;
			}
			List<ControlInputConfig> listInputs=new List<ControlInputConfig> { cameraConfig.ControlPoll };
			listInputs.AddRange(cameraConfig.Levels??new List<LevelInputConfig>());
			foreach(ControlInputConfig controlInputConfig in listInputs) {
				if(controlInputConfig==null || !controlInputConfig.Enabled || controlInputConfig.Interface==null || controlInputConfig.Property==null) {
					continue;
				}
				hashSetControls.Add(new ControlId(controlInputConfig.Interface.Value,controlInputConfig.Property.Value));
			}
			return hashSetControls;
		}

		public static ControlId GetControl(CameraSettingDef cameraSettingDef) {
			return new ControlId(cameraSettingDef.Interface,cameraSettingDef.Property);
		}
	}
}
