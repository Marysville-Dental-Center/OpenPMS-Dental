// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ImagingCapture.Camera;
using ImagingCapture.Settings;
using ImagingCapture.Uvc;

namespace ImagingCapture.Video {
	///<summary>The setting actions of the camera's buttons and levels (design 8.6): Toggle, Raise, Lower and SetValue on each press, TurnOff while
	///a level is active, and Highest as a cap when the triggers start. Each works on the level's Setting through CameraSettingWriter and is
	///logged in the program log with the old and new value. A change made here lasts until the camera closes: the next open applies the saved
	///[Camera.Settings] again. A setting the camera cannot read is logged and left alone.
	///
	///Off is Lowest, else the camera's minimum. The value a setting had before it was turned off is kept by control, so Toggle and the end of a
	///TurnOff level bring it back (else the camera's default, else its maximum), never above Highest. Raise and Lower move Steps steps (5 when
	///empty) from the lowest step (one step above off) to Highest (else the camera's maximum); Raise turns an off setting on at the lowest
	///step, Lower stops there.</summary>
	public sealed class CameraSettingActions {
		private readonly Func<IFrameSource> _funcFrameSource;
		private readonly Action<string> _actionLog;
		///<summary>The value (and whether it was automatic) each control had before a button or level turned it off.</summary>
		private readonly Dictionary<ControlId,KeyValuePair<int,bool>> _dictOnValues=new Dictionary<ControlId,KeyValuePair<int,bool>>();

		///<summary>funcFrameSource: the open camera, or null. actionLog: the program log.</summary>
		public CameraSettingActions(Func<IFrameSource> funcFrameSource,Action<string> actionLog) {
			_funcFrameSource=funcFrameSource??throw new ArgumentNullException(nameof(funcFrameSource));
			_actionLog=actionLog;
		}

		///<summary>When the triggers start: every configured level with a Setting and a Highest whose setting is above it gets Highest, so the
		///setting starts no higher than capture.ini allows.</summary>
		public void LimitAtStart(CameraConfig cameraConfig) {
			IEnumerable<LevelInputConfig> levels=(cameraConfig?.Levels??new List<LevelInputConfig>())
				.Where(x => x!=null && x.IsLevelConfigured && x.Actions.Any(y => LevelActions.IsSettingAction(y)) && x.Highest!="");
			foreach(LevelInputConfig level in levels) {
				Reading reading;
				if(!TryRead(level,out reading) || !reading.HasHighest) {
					continue;
				}
				if(!reading.IsAuto && reading.Value>reading.ValueMax) {
					WriteValue(reading,reading.ValueMax,false,"limited to Highest");
				}
			}
		}

		///<summary>A level changed (UI thread): a setting button acts on its press, a TurnOff level on both edges.</summary>
		public void OnLevelChanged(LevelInputConfig level,bool isActive) {
			if(level==null || !level.IsLevelConfigured) {
				return;
			}
			if(isActive && LevelActions.IsButtonAction(level.Action) && LevelActions.IsSettingAction(level.Action)) {
				Reading reading;
				if(!TryRead(level,out reading)) {
					return;
				}
				switch(level.Action) {
					case LevelAction.Toggle:
						if(IsOn(reading)) {
							TurnOff(reading,"button");
						}
						else {
							TurnOn(reading,"button");
						}
						break;
					case LevelAction.Raise:
					case LevelAction.Lower:
						Step(reading,level,level.Action==LevelAction.Raise);
						break;
					case LevelAction.SetValue:
						SetValue(reading,level);
						break;
				}
				return;
			}
			if(level.HasAction(LevelAction.TurnOff)) {
				Reading reading;
				if(!TryRead(level,out reading)) {
					return;
				}
				string reason="level "+level.Number.ToString(CultureInfo.InvariantCulture)+(isActive ? " active" : " ended");
				if(isActive && IsOn(reading)) {
					TurnOff(reading,reason);
				}
				else if(!isActive && !IsOn(reading)) {
					TurnOn(reading,reason);
				}
			}
		}

		#region Actions
		private static bool IsOn(Reading reading) {
			return reading.IsAuto || reading.Value>reading.ValueOff;
		}

		private void TurnOff(Reading reading,string reason) {
			_dictOnValues[reading.Control]=new KeyValuePair<int,bool>(reading.Value,reading.IsAuto);
			WriteValue(reading,reading.ValueOff,false,"off ("+reason+")");
		}

		private void TurnOn(Reading reading,string reason) {
			KeyValuePair<int,bool> pairOn;
			int valueNew;
			bool isAutoNew=false;
			if(_dictOnValues.TryGetValue(reading.Control,out pairOn) && (pairOn.Value || pairOn.Key>reading.ValueOff)) {
				valueNew=pairOn.Key;
				isAutoNew=pairOn.Value && !reading.HasHighest;
			}
			else {
				valueNew=reading.Range.Default>reading.ValueOff ? reading.Range.Default : reading.Range.Max;
			}
			if(!isAutoNew) {
				valueNew=Math.Min(valueNew,reading.ValueMax);
				if(valueNew<=reading.ValueOff) {
					Log("Camera setting "+reading.Def.DisplayName+": Highest is at or below off, so it cannot be turned on.");
					return;
				}
			}
			WriteValue(reading,valueNew,isAutoNew,"on ("+reason+")");
		}

		private void Step(Reading reading,LevelInputConfig level,bool isRaise) {
			if(reading.ValueMax<=reading.ValueOff) {
				Log("Camera setting "+reading.Def.DisplayName+": Highest is at or below off, so it cannot be raised or lowered.");
				return;
			}
			int steps=level.Steps>0 ? level.Steps.Value : LevelActions.StepsDefault;
			int step=Math.Max(Math.Max(1,reading.Range.Step),(reading.ValueMax-reading.ValueOff)/steps);
			int valueLowest=Math.Min(reading.ValueMax,reading.ValueOff+step);
			int valueNew;
			if(isRaise) {
				valueNew=!IsOn(reading) ? valueLowest : Math.Min(reading.ValueMax,reading.Value+step);
			}
			else {
				if(!IsOn(reading)) {
					return;
				}
				valueNew=Math.Min(reading.ValueMax,Math.Max(valueLowest,reading.Value-step));
			}
			valueNew=Snap(valueNew,reading.Range);
			if(valueNew==reading.Value && !reading.IsAuto) {
				return;
			}
			_dictOnValues[reading.Control]=new KeyValuePair<int,bool>(valueNew,false);
			WriteValue(reading,valueNew,false,isRaise ? "raised (button)" : "lowered (button)");
		}

		private void SetValue(Reading reading,LevelInputConfig level) {
			int value;
			bool isAuto;
			string error;
			if(!CameraSettingCatalog.TryParse(reading.Def,reading.Range,level.Value,out value,out isAuto,out error)) {
				Log("Camera setting "+reading.Def.DisplayName+": Value="+level.Value+" of [Level."+level.Number.ToString(CultureInfo.InvariantCulture)
					+"] was not set: "+error);
				return;
			}
			if(isAuto==reading.IsAuto && (isAuto || value==reading.Value)) {
				return;
			}
			WriteValue(reading,value,isAuto,"set (button)");
		}
		#endregion Actions

		#region Helpers
		///<summary>One setting as the camera has it now, with the level's off and highest values within the camera's range.</summary>
		private sealed class Reading {
			public CameraSettingDef Def;
			public ControlId Control;
			public UvcControlRange Range;
			public int Value;
			public bool IsAuto;
			public int ValueOff;
			public int ValueMax;
			public bool HasHighest;
		}

		///<summary>Reads the level's setting, its range, Lowest and Highest; false, logged, when it cannot.</summary>
		private bool TryRead(LevelInputConfig level,out Reading reading) {
			reading=null;
			IFrameSource frameSource=_funcFrameSource();
			CameraSettingDef cameraSettingDef=level.SettingDef;
			if(frameSource==null || cameraSettingDef==null) {
				return false;
			}
			UvcControlValue controlValue=frameSource.ReadControl(cameraSettingDef.Interface,cameraSettingDef.Property);
			UvcControlRange controlRange=frameSource.ReadControlRange(cameraSettingDef.Interface,cameraSettingDef.Property);
			if(!controlValue.IsOk || !controlRange.IsOk) {
				Log("Camera setting "+cameraSettingDef.DisplayName+" cannot be read ("+(controlValue.IsOk ? controlRange.Status : controlValue.Status)
					+"); the button leaves it as it is.");
				return false;
			}
			reading=new Reading {
				Def=cameraSettingDef,
				Control=CameraSettingsFile.GetControl(cameraSettingDef),
				Range=controlRange,
				Value=controlValue.Value,
				IsAuto=CameraSettingWriter.IsAuto(controlValue),
				ValueOff=controlRange.Min,
				ValueMax=controlRange.Max,
			};
			int valueParsed;
			bool isAuto;
			string error;
			if(level.Lowest!="") {
				if(CameraSettingCatalog.TryParse(cameraSettingDef,controlRange,level.Lowest,out valueParsed,out isAuto,out error) && !isAuto) {
					reading.ValueOff=valueParsed;
				}
				else {
					Log("Camera setting "+cameraSettingDef.DisplayName+": Lowest="+level.Lowest+" is not used: "+(error!="" ? error : "Auto is not a value."));
				}
			}
			if(level.Highest!="") {
				if(CameraSettingCatalog.TryParse(cameraSettingDef,controlRange,level.Highest,out valueParsed,out isAuto,out error) && !isAuto) {
					reading.ValueMax=valueParsed;
					reading.HasHighest=true;
				}
				else {
					Log("Camera setting "+cameraSettingDef.DisplayName+": Highest="+level.Highest+" is not used: "+(error!="" ? error : "Auto is not a value."));
				}
			}
			return true;
		}

		///<summary>value on the camera's step grid (from its minimum), within its range.</summary>
		private static int Snap(int value,UvcControlRange controlRange) {
			value=Math.Max(controlRange.Min,Math.Min(controlRange.Max,value));
			if(controlRange.Step>1) {
				value=controlRange.Min+(value-controlRange.Min)/controlRange.Step*controlRange.Step;
			}
			return value;
		}

		private void WriteValue(Reading reading,int valueNew,bool isAutoNew,string what) {
			IFrameSource frameSource=_funcFrameSource();
			if(frameSource==null) {
				return;
			}
			UvcControlValue controlValueWritten=CameraSettingWriter.Write(frameSource,reading.Def,valueNew,isAutoNew);
			Log("Camera setting "+reading.Def.DisplayName+" "+what+": "+CameraSettingCatalog.Format(reading.Def,reading.Range,reading.Value,reading.IsAuto)
				+" -> "+CameraSettingCatalog.Format(reading.Def,reading.Range,valueNew,isAutoNew)+(controlValueWritten.IsOk ? "." : CameraSettingWriter.GetFailureText(controlValueWritten)+"."));
		}

		private void Log(string line) {
			try {
				_actionLog?.Invoke(line);
			}
			catch {
				//A log failure must never stop the camera.
			}
		}
		#endregion Helpers
	}
}
