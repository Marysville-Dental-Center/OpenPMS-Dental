// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ImagingCapture.Uvc;
using ImagingDeviceManager.Config;

namespace ImagingCapture.Camera {
	///<summary>Where a photo's pixels come from (design 8.4). The names are the [Camera] PhotoSource values.</summary>
	public enum CameraPhotoSource {
		///<summary>The newest full-resolution preview frame. Works on every camera, with or without a still pin. The default.</summary>
		Preview=0,
		///<summary>The still-pin sample, when the still pin is connected.</summary>
		Still=1,
	}

	///<summary>What a level input does (design 8.6). The names are the [Level.N] Action values. A held action acts while the level is active
	///(PausePreview, TurnOff), and held actions combine with '|' ("PausePreview|TurnOff"). Every other action acts once per press, as a
	///camera button: a setting action changes the level's Setting (a CameraSettingCatalog id, for example Gain), a program action does what
	///its key does in the camera session. Nothing here is specific to any camera; what a camera's buttons do comes from Learn or the user.</summary>
	public enum LevelAction {
		///<summary>Held: the preview pauses while the level is active.</summary>
		PausePreview=0,
		///<summary>Each press turns Setting off (Lowest, else the camera's minimum) or back on (the value it had, else the camera's default,
		///at most Highest).</summary>
		Toggle=1,
		///<summary>Each press moves a photo session to its next slot (the right arrow).</summary>
		NextSlot=2,
		///<summary>Each press moves a photo session to its previous slot (the left arrow).</summary>
		PreviousSlot=3,
		///<summary>Each press raises Setting one step: Steps steps (5 when empty) from the lowest step to Highest (else the camera's maximum).
		///An off setting goes on at the lowest step.</summary>
		Raise=4,
		///<summary>Each press lowers Setting one step, down to the lowest step; it never turns it off (Toggle does).</summary>
		Lower=5,
		///<summary>Each press sets Setting to Value, for example "Auto" or "1/64 s".</summary>
		SetValue=6,
		///<summary>Held: Setting is off while the level is active, and back at the value it had when the level ends.</summary>
		TurnOff=7,
		///<summary>Each press takes a photo, through the same debounce lockout as the capture button (TriggerEngine's press path).</summary>
		TakePhoto=8,
		///<summary>Each press retakes the last photo (Ctrl+Z).</summary>
		RetakeLast=9,
		///<summary>Each press freezes the preview or goes back to live (F).</summary>
		Freeze=10,
		///<summary>Each press starts or stops a video recording (R).</summary>
		Record=11,
		///<summary>Each press pauses or resumes the recording (P).</summary>
		PauseRecording=12,
		///<summary>Each press picks the teeth of the next photo (T).</summary>
		PickTeeth=13,
		///<summary>Each press ends the session, or first goes back to live when frozen (Esc).</summary>
		EndSession=14,
	}

	///<summary>The texts and kinds of the level actions, for capture.ini, the Learn window, the live test and the status notes.</summary>
	public static class LevelActions {
		///<summary>The steps between a setting's lowest step and its highest value when a level has no Steps (Raise, Lower).</summary>
		public const int StepsDefault=5;
		///<summary>The actions a camera button (a press, not a held state) can have, in the order Learn offers them: the setting actions,
		///then the program actions.</summary>
		public static readonly LevelAction[] ArrayButtonActions=new LevelAction[] { LevelAction.Toggle,LevelAction.Raise,LevelAction.Lower,
			LevelAction.SetValue,LevelAction.TakePhoto,LevelAction.NextSlot,LevelAction.PreviousSlot,LevelAction.RetakeLast,LevelAction.Freeze,
			LevelAction.Record,LevelAction.PauseRecording,LevelAction.PickTeeth,LevelAction.EndSession };

		///<summary>True for an action that acts while the level is active: PausePreview, TurnOff.</summary>
		public static bool IsHeldAction(LevelAction? action) {
			return action==LevelAction.PausePreview || action==LevelAction.TurnOff;
		}

		///<summary>True for an action that acts once per press (on the active edge) instead of while the level is active.</summary>
		public static bool IsButtonAction(LevelAction? action) {
			return action!=null && !IsHeldAction(action);
		}

		///<summary>True for an action that changes the level's Setting: Toggle, Raise, Lower, SetValue, TurnOff.</summary>
		public static bool IsSettingAction(LevelAction? action) {
			return action==LevelAction.Toggle || action==LevelAction.Raise || action==LevelAction.Lower || action==LevelAction.SetValue
				|| action==LevelAction.TurnOff;
		}

		///<summary>True for a button action of the program (take photo, next slot, ...), which the camera session carries out.</summary>
		public static bool IsProgramAction(LevelAction? action) {
			return IsButtonAction(action) && !IsSettingAction(action);
		}

		///<summary>"pause preview", "turn on/off", "take a photo", ...; "no action" for null.</summary>
		public static string GetText(LevelAction? action) {
			switch(action) {
				case LevelAction.PausePreview:
					return "pause preview";
				case LevelAction.Toggle:
					return "turn on/off";
				case LevelAction.Raise:
					return "raise";
				case LevelAction.Lower:
					return "lower";
				case LevelAction.SetValue:
					return "set to a value";
				case LevelAction.TurnOff:
					return "turn off while active";
				case LevelAction.TakePhoto:
					return "take a photo";
				case LevelAction.NextSlot:
					return "next slot";
				case LevelAction.PreviousSlot:
					return "previous slot";
				case LevelAction.RetakeLast:
					return "retake the last photo";
				case LevelAction.Freeze:
					return "freeze or go live";
				case LevelAction.Record:
					return "start or stop recording";
				case LevelAction.PauseRecording:
					return "pause or resume recording";
				case LevelAction.PickTeeth:
					return "pick teeth";
				case LevelAction.EndSession:
					return "end the session";
				default:
					return "no action";
			}
		}

		///<summary>What an action does to a setting, by its display name: "turn Gain on/off", "raise Gain", "set Exposure to Auto", "turn Gain
		///off while active". The plain text (GetText) for the other actions.</summary>
		public static string GetText(LevelAction? action,string settingName,string value="") {
			settingName=string.IsNullOrWhiteSpace(settingName) ? "the setting" : settingName.Trim();
			switch(action) {
				case LevelAction.Toggle:
					return "turn "+settingName+" on/off";
				case LevelAction.Raise:
					return "raise "+settingName;
				case LevelAction.Lower:
					return "lower "+settingName;
				case LevelAction.SetValue:
					return "set "+settingName+" to "+(string.IsNullOrWhiteSpace(value) ? "a value" : value.Trim());
				case LevelAction.TurnOff:
					return "turn "+settingName+" off while active";
				default:
					return GetText(action);
			}
		}

		///<summary>What a level does, all its actions: "pause preview and turn Gain off while active", "raise Gain", "next slot".</summary>
		public static string GetText(LevelInputConfig levelInputConfig) {
			if(levelInputConfig==null || levelInputConfig.Actions.Count==0) {
				return GetText((LevelAction?)null);
			}
			string settingName=levelInputConfig.SettingDef?.DisplayName??levelInputConfig.Setting;
			return string.Join(" and ",levelInputConfig.Actions.Select(x => GetText(x,settingName,levelInputConfig.Value)));
		}

		///<summary>The actions of an Action value: one name, or held actions joined with '|' (case ignored). Null when empty or when any part
		///is unknown.</summary>
		public static List<LevelAction> Parse(string text) {
			List<LevelAction> listActions=new List<LevelAction>();
			foreach(string part in (text??"").Split('|')) {
				string name=part.Trim();
				LevelAction action;
				if(name=="" || int.TryParse(name,out _) || !Enum.TryParse(name,true,out action) || !Enum.IsDefined(typeof(LevelAction),action)) {
					return null;
				}
				if(!listActions.Contains(action)) {
					listActions.Add(action);
				}
			}
			return listActions.Count==0 ? null : listActions;
		}

		///<summary>The Action value: the names joined with '|'; "" for none.</summary>
		public static string Format(IEnumerable<LevelAction> actions) {
			return string.Join("|",(actions??Enumerable.Empty<LevelAction>()).Select(x => x.ToString()));
		}
	}

	///<summary>The camera sections of capture.ini (design 9.3): [Camera], [Trigger], [Trigger.Still], [Trigger.ControlPoll] and
	///[Level.N]. A snapshot: Read takes the values from a LocalCaptureConfig, the caller changes them, and Write puts the changed
	///ones back (the caller then saves the LocalCaptureConfig).
	///
	///Provenance (design 4 and 8.4): no trigger or level value has a default. Control, Bits and ActiveLevel are
	///null until the Learn tool or the user fills them in, and an empty, missing or invalid value means not configured. The only
	///camera defaults are PollMs, the DebounceMs fallback, the learned-debounce rule, the photo source and the format rule in
	///ModeSelector, each with its justification written next to it.</summary>
	public sealed class CameraConfig {
		#region Constants
		public const string SectionCamera="Camera";
		public const string SectionTrigger="Trigger";
		public const string SectionTriggerStill="Trigger.Still";
		public const string SectionTriggerControlPoll="Trigger.ControlPoll";
		///<summary>Level sections are [Level.N], N a positive whole number written without leading zeros.</summary>
		public const string SectionLevelPrefix="Level.";

		///<summary>[Trigger.ControlPoll] PollMs default: 50 ms, which is 20 Hz. That is about twice the rate needed to see the shortest
		///deliberate human key press (roughly 100 ms). Chosen for that reason alone (design 8.4).</summary>
		public const int PollMsDefault=50;
		///<summary>PollMs values are held to 10-1000 ms. Below 10 ms the reads only add load on the camera's worker thread, since no
		///deliberate press is that short; above 1000 ms a deliberate press would be missed.</summary>
		public const int PollMsMin=10;
		public const int PollMsMax=1000;
		///<summary>Debounce lockout when nothing was learned: 300 ms, a value above typical key bounce and below a deliberate second
		///press (design 8.4).</summary>
		public const int DebounceMsFallback=300;
		///<summary>Learn sets DebounceMs to DebounceLearnedFactor times the longest press it observed, and at least DebounceMsLearnedMin
		///(design 8.4): a lockout twice as long as any observed press cannot end while that press is still held, and the floor
		///keeps a quick tap from giving a lockout shorter than key bounce.</summary>
		public const int DebounceLearnedFactor=2;
		public const int DebounceMsLearnedMin=200;
		///<summary>A DebounceMs above 60 s reads as invalid (a typing error), so the fallback applies.</summary>
		public const int DebounceMsMax=60000;
		#endregion Constants

		#region [Camera]
		///<summary>[Camera] Moniker: the DirectShow moniker display name of the chosen camera. "" when none.</summary>
		public string Moniker { get; set; }="";
		///<summary>[Camera] Name: the camera's friendly name, used to find it again when its moniker changes (another USB port). ""
		///when none.</summary>
		public string Name { get; set; }="";
		///<summary>[Camera] Width, 0 = any (ModeSelector).</summary>
		public int Width { get; set; }
		///<summary>[Camera] Height, 0 = any.</summary>
		public int Height { get; set; }
		///<summary>[Camera] Subtype: "auto" (the default), "MJPG", "YUY2", "RGB24", or another FourCC that ModeSelector compares with
		///UvcMode.SubtypeName.</summary>
		public string Subtype { get; set; }=ModeSelector.SubtypeAuto;
		///<summary>[Camera] PhotoSource, default Preview (design 8.4).</summary>
		public CameraPhotoSource PhotoSource { get; set; }=CameraPhotoSource.Preview;
		///<summary>[Camera] MainWindowSpace, default 1: Space typed in the main window takes a photo (design 8.10).</summary>
		public bool MainWindowSpace { get; set; }=true;
		#endregion [Camera]

		#region [Trigger]
		///<summary>[Trigger] DebounceMs: the lockout after a press, shared by every trigger. Null when empty, missing or invalid (not
		///a whole number from 1 to DebounceMsMax). Learn writes it; see EffectiveDebounceMs.</summary>
		public int? DebounceMs { get; set; }
		///<summary>[Trigger.Still] Enabled, default 0: each still-pin sample is one press.</summary>
		public bool StillEnabled { get; set; }
		///<summary>[Trigger.ControlPoll] Enabled, Control, Bits and ActiveLevel. Never null.</summary>
		public ControlInputConfig ControlPoll { get; set; }=new ControlInputConfig();
		///<summary>[Trigger.ControlPoll] PollMs, default PollMsDefault. Missing, empty, invalid and non-positive values read as the
		///default. See EffectivePollMs.</summary>
		public int PollMs { get; set; }=PollMsDefault;
		#endregion [Trigger]

		#region [Level.N]
		///<summary>Every [Level.N] section, by ascending N. Never null. Write disables (Enabled=0, values emptied) a level that is in
		///the file but no longer in this list.</summary>
		public List<LevelInputConfig> Levels { get; set; }=new List<LevelInputConfig>();
		#endregion [Level.N]

		#region Effective values
		///<summary>DebounceMs when set (the value Learn saved, or one typed into the file), otherwise DebounceMsFallback.</summary>
		public int EffectiveDebounceMs {
			get {
				return DebounceMs ?? DebounceMsFallback;
			}
		}

		///<summary>PollMs held to PollMsMin-PollMsMax.</summary>
		public int EffectivePollMs {
			get {
				return Math.Min(PollMsMax,Math.Max(PollMsMin,PollMs));
			}
		}

		///<summary>DebounceMs for the longest press Learn observed: DebounceLearnedFactor times it, at least DebounceMsLearnedMin, and at
		///most DebounceMsMax.</summary>
		public static int GetLearnedDebounceMs(int longestPressMs) {
			long debounceMs=(long)Math.Max(0,longestPressMs)*DebounceLearnedFactor;
			return (int)Math.Min(DebounceMsMax,Math.Max(DebounceMsLearnedMin,debounceMs));
		}

		///<summary>The level with this number, or null.</summary>
		public LevelInputConfig GetLevel(int number) {
			return Levels.FirstOrDefault(x => x!=null && x.Number==number);
		}
		#endregion Effective values

		#region Camera choice
		///<summary>Takes the camera's moniker and name, cut at the first NUL (strings from native code may carry one). The name is also
		///made safe for an INI value: ';' and control characters become spaces.</summary>
		public void SetDevice(UvcDeviceInfo device) {
			if(device==null) {
				Moniker="";
				Name="";
				return;
			}
			Moniker=CutAtNul(device.MonikerDisplayName).Trim();
			Name=ToSafeName(device.FriendlyName);
		}

		///<summary>The saved camera among devices: the first with the saved moniker (ignoring case), else the first whose name, made
		///safe as in SetDevice, equals the saved name. Null when neither is saved or found; the caller then takes the first device
		///(design 8.2). No name is matched against any list.</summary>
		public UvcDeviceInfo FindSavedDevice(IEnumerable<UvcDeviceInfo> devices) {
			List<UvcDeviceInfo> listDevices=(devices??Enumerable.Empty<UvcDeviceInfo>()).Where(x => x!=null).ToList();
			string moniker=(Moniker??"").Trim();
			if(moniker!="") {
				UvcDeviceInfo device=listDevices.FirstOrDefault(x => string.Equals(CutAtNul(x.MonikerDisplayName).Trim(),moniker,StringComparison.OrdinalIgnoreCase));
				if(device!=null) {
					return device;
				}
			}
			string name=(Name??"").Trim();
			if(name!="") {
				return listDevices.FirstOrDefault(x => string.Equals(ToSafeName(x.FriendlyName),name,StringComparison.OrdinalIgnoreCase));
			}
			return null;
		}

		///<summary>The text before the first NUL character. "" for null.</summary>
		public static string CutAtNul(string text) {
			if(text==null) {
				return "";
			}
			int idxNul=text.IndexOf('\0');
			return idxNul<0 ? text : text.Substring(0,idxNul);
		}

		///<summary>Cut at NUL, ';' and control characters replaced by spaces, trimmed.</summary>
		private static string ToSafeName(string name) {
			StringBuilder stringBuilder=new StringBuilder();
			foreach(char c in CutAtNul(name)) {
				stringBuilder.Append(c==';' || char.IsControl(c) ? ' ' : c);
			}
			return stringBuilder.ToString().Trim();
		}
		#endregion Camera choice

		#region Read
		///<summary>Reads the camera sections. Never throws. A null config gives the defaults: no camera, no trigger, no level.</summary>
		public static CameraConfig Read(LocalCaptureConfig config) {
			CameraConfig cameraConfig=new CameraConfig();
			if(config==null) {
				return cameraConfig;
			}
			cameraConfig.Moniker=CutAtNul(config.GetString(SectionCamera,"Moniker")).Trim();
			cameraConfig.Name=CutAtNul(config.GetString(SectionCamera,"Name")).Trim();
			cameraConfig.Width=Math.Max(0,config.GetInt(SectionCamera,"Width",0));
			cameraConfig.Height=Math.Max(0,config.GetInt(SectionCamera,"Height",0));
			string subtype=config.GetString(SectionCamera,"Subtype").Trim();
			cameraConfig.Subtype=subtype=="" ? ModeSelector.SubtypeAuto : subtype;
			cameraConfig.PhotoSource=ParseEnum<CameraPhotoSource>(config.GetString(SectionCamera,"PhotoSource")) ?? CameraPhotoSource.Preview;
			cameraConfig.MainWindowSpace=config.GetBool(SectionCamera,"MainWindowSpace",true);
			cameraConfig.DebounceMs=ParseDebounceMs(config.GetString(SectionTrigger,"DebounceMs"));
			cameraConfig.StillEnabled=config.GetBool(SectionTriggerStill,"Enabled",false);
			cameraConfig.ControlPoll=new ControlInputConfig();
			ReadControlInput(config,SectionTriggerControlPoll,cameraConfig.ControlPoll);
			int pollMs=config.GetInt(SectionTriggerControlPoll,"PollMs",PollMsDefault);
			cameraConfig.PollMs=pollMs>0 ? pollMs : PollMsDefault;
			foreach(int number in GetLevelNumbers(config)) {
				LevelInputConfig levelInputConfig=new LevelInputConfig(number);
				string section=GetLevelSection(number);
				ReadControlInput(config,section,levelInputConfig);
				//A missing Action key means the design's default action; an empty or unknown one means none (not configured).
				levelInputConfig.Actions=config.ContainsKey(section,"Action")
					? LevelActions.Parse(config.GetString(section,"Action"))
					: new List<LevelAction> { LevelAction.PausePreview };
				levelInputConfig.Setting=CutAtNul(config.GetString(section,"Setting")).Trim();
				levelInputConfig.Highest=CutAtNul(config.GetString(section,"Highest")).Trim();
				levelInputConfig.Lowest=CutAtNul(config.GetString(section,"Lowest")).Trim();
				levelInputConfig.Value=CutAtNul(config.GetString(section,"Value")).Trim();
				int steps=config.GetInt(section,"Steps",-1);
				levelInputConfig.Steps=steps>0 ? steps : (int?)null;
				cameraConfig.Levels.Add(levelInputConfig);
			}
			return cameraConfig;
		}

		///<summary>The binding as written: Control (the control's name, for example Roll) and Bits (the bit numbers, "3" or "0,2"); a file
		///written before them has Interface, Property and Mask (hex), which are still read when Control or Bits is missing.</summary>
		private static void ReadControlInput(LocalCaptureConfig config,string section,ControlInputConfig controlInputConfig) {
			controlInputConfig.Enabled=config.GetBool(section,"Enabled",false);
			if(config.ContainsKey(section,"Control")) {
				UvcControlInterface controlInterface;
				int property;
				bool isFound=UvcControls.TryGetByName(config.GetString(section,"Control"),out controlInterface,out property);
				controlInputConfig.Interface=isFound ? controlInterface : (UvcControlInterface?)null;
				controlInputConfig.Property=isFound ? property : (int?)null;
			}
			else {
				controlInputConfig.Interface=ParseEnum<UvcControlInterface>(config.GetString(section,"Interface"));
				controlInputConfig.Property=ParseProperty(config.GetString(section,"Property"));
			}
			if(config.ContainsKey(section,"Bits")) {
				int mask;
				controlInputConfig.Mask=ControlValueFormat.TryParseBits(config.GetString(section,"Bits"),out mask) ? mask : (int?)null;
			}
			else {
				controlInputConfig.Mask=ParseMask(config.GetString(section,"Mask"));
			}
			controlInputConfig.ActiveLevel=ParseEnum<ControlLevel>(config.GetString(section,"ActiveLevel"));
		}

		///<summary>True when the section has a binding in the keys written before Control and Bits.</summary>
		private static bool HasOldBindingKeys(LocalCaptureConfig config,string section) {
			return new[] { "Interface","Property","Mask" }.Any(x => config.GetString(section,x).Trim()!="");
		}

		///<summary>The N of every [Level.N] section, ascending, without duplicates.</summary>
		private static List<int> GetLevelNumbers(LocalCaptureConfig config) {
			List<int> listNumbers=new List<int>();
			foreach(string section in config.GetSectionNames()) {
				if(!section.StartsWith(SectionLevelPrefix,StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				string digits=section.Substring(SectionLevelPrefix.Length);
				int number;
				if(!int.TryParse(digits,NumberStyles.None,CultureInfo.InvariantCulture,out number) || number<=0
					|| number.ToString(CultureInfo.InvariantCulture)!=digits)
				{
					continue;
				}
				if(!listNumbers.Contains(number)) {
					listNumbers.Add(number);
				}
			}
			listNumbers.Sort();
			return listNumbers;
		}

		///<summary>"Level.1" for 1.</summary>
		public static string GetLevelSection(int number) {
			return SectionLevelPrefix+number.ToString(CultureInfo.InvariantCulture);
		}

		///<summary>The enum value whose name equals the text, ignoring case and surrounding whitespace. Null for empty text, numbers
		///and unknown names.</summary>
		private static T? ParseEnum<T>(string text) where T:struct {
			string trimmed=(text??"").Trim();
			if(trimmed=="") {
				return null;
			}
			foreach(string name in Enum.GetNames(typeof(T))) {
				if(string.Equals(name,trimmed,StringComparison.OrdinalIgnoreCase)) {
					return (T)Enum.Parse(typeof(T),name);
				}
			}
			return null;
		}

		///<summary>A non-negative decimal property number, or null. Whether it is valid for the interface is checked by
		///ControlInputConfig.GetBinding.</summary>
		private static int? ParseProperty(string text) {
			int property;
			if(int.TryParse((text??"").Trim(),NumberStyles.None,CultureInfo.InvariantCulture,out property)) {
				return property;
			}
			return null;
		}

		///<summary>A mask in hex (0x prefix) or decimal, or null when empty, invalid or 0.</summary>
		private static int? ParseMask(string text) {
			int mask;
			if(ControlValueFormat.TryParse(text,out mask) && mask!=0) {
				return mask;
			}
			return null;
		}

		private static int? ParseDebounceMs(string text) {
			int debounceMs;
			if(int.TryParse((text??"").Trim(),NumberStyles.None,CultureInfo.InvariantCulture,out debounceMs) && debounceMs>=1 && debounceMs<=DebounceMsMax) {
				return debounceMs;
			}
			return null;
		}
		#endregion Read

		#region Write
		///<summary>Sets, in config, every camera key whose value differs from what config holds now (as Read would see it), so keys
		///that did not change keep their text, comments and invalid values. A level in config that is not in Levels is disabled and
		///its values emptied. Nothing is saved: the caller calls config.Save. Returns false, with error, and sets nothing when a value
		///cannot be written to an INI line (a moniker with ';' or a control character).</summary>
		public bool Write(LocalCaptureConfig config,out string error) {
			error="";
			if(config==null) {
				error="No capture settings to write to.";
				return false;
			}
			CameraConfig cameraConfigOld=Read(config);
			List<string[]> listSets=new List<string[]>();
			string moniker=CutAtNul(Moniker).Trim();
			string name=ToSafeName(Name);
			AddIfChanged(listSets,SectionCamera,"Moniker",cameraConfigOld.Moniker,moniker);
			AddIfChanged(listSets,SectionCamera,"Name",cameraConfigOld.Name,name);
			AddIfChanged(listSets,SectionCamera,"Width",cameraConfigOld.Width,Math.Max(0,Width));
			AddIfChanged(listSets,SectionCamera,"Height",cameraConfigOld.Height,Math.Max(0,Height));
			string subtype=string.IsNullOrWhiteSpace(Subtype) ? ModeSelector.SubtypeAuto : Subtype.Trim();
			if(!string.Equals(subtype,cameraConfigOld.Subtype,StringComparison.OrdinalIgnoreCase)) {
				listSets.Add(new[] { SectionCamera,"Subtype",subtype });
			}
			AddIfChanged(listSets,SectionCamera,"PhotoSource",cameraConfigOld.PhotoSource.ToString(),PhotoSource.ToString());
			AddIfChanged(listSets,SectionCamera,"MainWindowSpace",cameraConfigOld.MainWindowSpace,MainWindowSpace);
			AddIfChanged(listSets,SectionTrigger,"DebounceMs",FormatNullable(cameraConfigOld.DebounceMs),FormatNullable(DebounceMs));
			AddIfChanged(listSets,SectionTriggerStill,"Enabled",cameraConfigOld.StillEnabled,StillEnabled);
			AddControlInput(listSets,SectionTriggerControlPoll,cameraConfigOld.ControlPoll,ControlPoll??new ControlInputConfig(),
				HasOldBindingKeys(config,SectionTriggerControlPoll));
			int pollMs=PollMs>0 ? PollMs : PollMsDefault;
			AddIfChanged(listSets,SectionTriggerControlPoll,"PollMs",cameraConfigOld.PollMs,pollMs);
			List<LevelInputConfig> listLevels=(Levels??new List<LevelInputConfig>()).Where(x => x!=null).ToList();
			foreach(int number in listLevels.Select(x => x.Number).Union(cameraConfigOld.Levels.Select(x => x.Number)).OrderBy(x => x)) {
				if(number<=0) {
					error="Level numbers start at 1.";
					return false;
				}
				string section=GetLevelSection(number);
				LevelInputConfig levelOld=cameraConfigOld.GetLevel(number);
				LevelInputConfig levelListed=listLevels.FirstOrDefault(x => x.Number==number);
				LevelInputConfig levelNew=levelListed ?? new LevelInputConfig(number) { Action=levelOld?.Action };
				AddControlInput(listSets,section,levelOld??new LevelInputConfig(number) { Action=null },levelNew,HasOldBindingKeys(config,section));
				string actionOld=levelOld==null ? "" : LevelActions.Format(levelOld.Actions);
				string actionNew=LevelActions.Format(levelNew.Actions);
				//A new section always gets its Action line, so the file shows what the level does.
				if(levelOld==null || actionOld!=actionNew) {
					listSets.Add(new[] { section,"Action",actionNew });
				}
				//The setting a setting action changes and its options. A level that is not listed (turned off) keeps them as the file has them.
				if(levelListed!=null) {
					AddIfChanged(listSets,section,"Setting",levelOld?.Setting??"",(levelListed.Setting??"").Trim());
					AddIfChanged(listSets,section,"Highest",levelOld?.Highest??"",(levelListed.Highest??"").Trim());
					AddIfChanged(listSets,section,"Lowest",levelOld?.Lowest??"",(levelListed.Lowest??"").Trim());
					AddIfChanged(listSets,section,"Steps",FormatNullable(levelOld?.Steps),FormatNullable(levelListed.Steps>0 ? levelListed.Steps : null));
					AddIfChanged(listSets,section,"Value",levelOld?.Value??"",(levelListed.Value??"").Trim());
				}
			}
			foreach(string[] arraySet in listSets) {
				if(!IsWritableValue(arraySet[2])) {
					error="The value of ["+arraySet[0]+"] "+arraySet[1]+" cannot be saved in capture.ini, because it contains ';' or a control character.";
					return false;
				}
			}
			foreach(string[] arraySet in listSets) {
				if(!config.Set(arraySet[0],arraySet[1],arraySet[2])) {
					error=config.LastError;
					return false;
				}
			}
			return true;
		}

		///<summary>Write, then config.Save, for a window that shares config with another one (the Learn window and the Video
		///window). When the write or the save fails, the camera values config held before are written back into it, so that the
		///other window's next save does not save and apply values the user was told were not saved. A [Level.N] section the
		///write added stays in config then, disabled and empty. Returns false, with error, on failure. On success error is "" and
		///warning is "" or the warning Save left in config.LastError (for example that the values went to the per-user file).</summary>
		public bool WriteAndSave(LocalCaptureConfig config,out string error,out string warning) {
			warning="";
			if(config==null) {
				error="No capture settings to write to.";
				return false;
			}
			CameraConfig cameraConfigBefore=Read(config);
			if(Write(config,out error)) {
				if(config.Save()) {
					warning=config.LastError;
					return true;
				}
				error=config.LastError;
			}
			//Values Read from config can be written back, except text an editor put in the file that Set refuses (a control
			//character), which Write reports without setting anything.
			string errorRestore;
			cameraConfigBefore.Write(config,out errorRestore);
			return false;
		}

		///<summary>The binding is written as Control (the name) and Bits (the bit numbers). When the file has it in the keys written before them
		///(Interface, Property, Mask) and it changes, it is written in the new keys and the old ones are emptied; unchanged, it stays as it is.</summary>
		private static void AddControlInput(List<string[]> listSets,string section,ControlInputConfig controlInputOld,ControlInputConfig controlInputNew,
			bool hasOldBindingKeys)
		{
			AddIfChanged(listSets,section,"Enabled",controlInputOld.Enabled,controlInputNew.Enabled);
			string controlOld=FormatControl(controlInputOld);
			string controlNew=FormatControl(controlInputNew);
			string bitsOld=controlInputOld.Mask==null ? "" : ControlValueFormat.ToBitsText(controlInputOld.Mask.Value);
			string bitsNew=controlInputNew.Mask==null || controlInputNew.Mask==0 ? "" : ControlValueFormat.ToBitsText(controlInputNew.Mask.Value);
			if(hasOldBindingKeys) {
				if(controlOld!=controlNew || bitsOld!=bitsNew) {
					listSets.Add(new[] { section,"Control",controlNew });
					listSets.Add(new[] { section,"Bits",bitsNew });
					listSets.Add(new[] { section,"Interface","" });
					listSets.Add(new[] { section,"Property","" });
					listSets.Add(new[] { section,"Mask","" });
				}
			}
			else {
				AddIfChanged(listSets,section,"Control",controlOld,controlNew);
				AddIfChanged(listSets,section,"Bits",bitsOld,bitsNew);
			}
			AddIfChanged(listSets,section,"ActiveLevel",FormatNullable(controlInputOld.ActiveLevel),FormatNullable(controlInputNew.ActiveLevel));
		}

		///<summary>The control's name ("Roll"), or "" when the interface or property is missing or not standard.</summary>
		private static string FormatControl(ControlInputConfig controlInputConfig) {
			if(controlInputConfig.Interface==null || controlInputConfig.Property==null
				|| !UvcControls.IsValidProperty(controlInputConfig.Interface.Value,controlInputConfig.Property.Value))
			{
				return "";
			}
			return UvcControls.GetPropertyName(controlInputConfig.Interface.Value,controlInputConfig.Property.Value);
		}

		private static void AddIfChanged(List<string[]> listSets,string section,string key,string valueOld,string valueNew) {
			if(valueOld!=valueNew) {
				listSets.Add(new[] { section,key,valueNew });
			}
		}

		private static void AddIfChanged(List<string[]> listSets,string section,string key,int valueOld,int valueNew) {
			if(valueOld!=valueNew) {
				listSets.Add(new[] { section,key,valueNew.ToString(CultureInfo.InvariantCulture) });
			}
		}

		private static void AddIfChanged(List<string[]> listSets,string section,string key,bool valueOld,bool valueNew) {
			if(valueOld!=valueNew) {
				listSets.Add(new[] { section,key,valueNew ? "1" : "0" });
			}
		}

		///<summary>"" for null, otherwise the invariant text (enum names, decimal numbers).</summary>
		private static string FormatNullable<T>(T? value) where T:struct {
			if(value==null) {
				return "";
			}
			IFormattable formattable=value.Value as IFormattable;
			if(formattable!=null && !(value.Value is Enum)) {
				return formattable.ToString(null,CultureInfo.InvariantCulture);
			}
			return value.Value.ToString();
		}

		///<summary>False for a value that LocalCaptureConfig.Set would refuse: ';' or a control character other than tab.</summary>
		private static bool IsWritableValue(string value) {
			foreach(char c in value??"") {
				if(c==';' || (char.IsControl(c) && c!='\t')) {
					return false;
				}
			}
			return true;
		}
		#endregion Write

		///<summary>A deep copy.</summary>
		public CameraConfig Clone() {
			CameraConfig cameraConfig=(CameraConfig)MemberwiseClone();
			cameraConfig.ControlPoll=(ControlPoll??new ControlInputConfig()).Clone();
			cameraConfig.Levels=(Levels??new List<LevelInputConfig>()).Where(x => x!=null).Select(x => x.CloneLevel()).ToList();
			return cameraConfig;
		}
	}

	///<summary>A control input in capture.ini: [Trigger.ControlPoll], or the shared part of [Level.N]. A null value means the key is
	///empty, missing or invalid, which means not configured. Nothing here has a default: the values come from the Learn tool
	///(design 8.4, 8.5).</summary>
	public class ControlInputConfig {
		///<summary>Enabled, default 0.</summary>
		public bool Enabled { get; set; }
		///<summary>Interface: CameraControl or VideoProcAmp.</summary>
		public UvcControlInterface? Interface { get; set; }
		///<summary>Property: CameraControl 0-6 or VideoProcAmp 0-9 (the Windows SDK numbering), in decimal.</summary>
		public int? Property { get; set; }
		///<summary>Mask: the bits of the value that matter, written in hex ("0x10"). Never 0.</summary>
		public int? Mask { get; set; }
		///<summary>ActiveLevel: High or Low (see ControlLevel).</summary>
		public ControlLevel? ActiveLevel { get; set; }

		///<summary>True when Enabled and the values make a valid binding.</summary>
		public bool IsConfigured {
			get {
				return Enabled && GetBinding()!=null;
			}
		}

		///<summary>The binding the values describe when they are all present and valid, whether or not Enabled; otherwise null.</summary>
		public ControlBinding GetBinding() {
			if(Interface==null || Property==null || Mask==null || Mask==0 || ActiveLevel==null) {
				return null;
			}
			ControlId control=new ControlId(Interface.Value,Property.Value);
			if(!control.IsValid) {
				return null;
			}
			return new ControlBinding(control,Mask.Value,ActiveLevel.Value);
		}

		///<summary>Takes the values of a binding. Null clears them.</summary>
		public void SetBinding(ControlBinding controlBinding) {
			Interface=controlBinding?.Control.Interface;
			Property=controlBinding?.Control.Property;
			Mask=controlBinding?.Mask;
			ActiveLevel=controlBinding?.ActiveLevel;
		}

		///<summary>"" when the input is off or complete. Otherwise, for an enabled input, which values are missing or invalid, for the
		///log and the settings note.</summary>
		public virtual string GetProblem() {
			if(!Enabled) {
				return "";
			}
			List<string> listMissing=new List<string>();
			if(Interface==null || Property==null || !UvcControls.IsValidProperty(Interface.Value,Property.Value)) {
				listMissing.Add("Control");
			}
			if(Mask==null || Mask==0) {
				listMissing.Add("Bits");
			}
			if(ActiveLevel==null) {
				listMissing.Add("ActiveLevel");
			}
			if(listMissing.Count==0) {
				return "";
			}
			return "it is enabled, but "+string.Join(", ",listMissing)+(listMissing.Count==1 ? " is" : " are")+" empty or invalid. Run Learn to set it up.";
		}

		public ControlInputConfig Clone() {
			return (ControlInputConfig)MemberwiseClone();
		}
	}

	///<summary>One [Level.N] section: a generic level input with one action (design 8.6). Off by default.</summary>
	public sealed class LevelInputConfig:ControlInputConfig {
		///<summary>The N of [Level.N], 1 or more.</summary>
		public int Number { get; }
		private List<LevelAction> _listActions=new List<LevelAction> { LevelAction.PausePreview };

		///<summary>The first of Actions, or null when there is none. Setting it makes it the only action.</summary>
		public LevelAction? Action {
			get {
				return _listActions==null || _listActions.Count==0 ? (LevelAction?)null : _listActions[0];
			}
			set {
				_listActions=value==null ? null : new List<LevelAction> { value.Value };
			}
		}

		///<summary>Action: one action, or held actions joined with '|'. A missing key reads as PausePreview, the design's default; an empty or
		///unknown value reads as none, and the level then does nothing.</summary>
		public IReadOnlyList<LevelAction> Actions {
			get {
				return (IReadOnlyList<LevelAction>)_listActions??new List<LevelAction>();
			}
			set {
				_listActions=value==null || value.Count==0 ? null : value.Distinct().ToList();
			}
		}

		///<summary>Setting: the camera setting a setting action changes, by its CameraSettingCatalog id ("Gain", "Exposure"; a display name
		///also reads). "" when none.</summary>
		public string Setting { get; set; }="";
		///<summary>Highest: the highest value Toggle, Raise and TurnOff give Setting, as people write it ("20", "1/64 s"); when the camera's
		///triggers start with Setting above it, Setting is set to it. "" is the camera's maximum.</summary>
		public string Highest { get; set; }="";
		///<summary>Lowest: the value Toggle and TurnOff give Setting to turn it off. "" is the camera's minimum.</summary>
		public string Lowest { get; set; }="";
		///<summary>Steps: the steps of Raise and Lower between the lowest step and Highest; null is LevelActions.StepsDefault.</summary>
		public int? Steps { get; set; }
		///<summary>Value: what SetValue sets Setting to, as people write it ("Auto", "60 Hz", "1/64 s").</summary>
		public string Value { get; set; }="";

		public LevelInputConfig(int number) {
			Number=number;
		}

		public bool HasAction(LevelAction action) {
			return _listActions!=null && _listActions.Contains(action);
		}

		///<summary>The setting Setting names, or null when it is empty or unknown.</summary>
		public CameraSettingDef SettingDef {
			get {
				CameraSettingDef cameraSettingDef;
				return CameraSettingCatalog.TryGet(Setting,out cameraSettingDef) ? cameraSettingDef : null;
			}
		}

		public override string GetProblem() {
			string problem=base.GetProblem();
			if(problem!="" || !Enabled) {
				return problem;
			}
			return GetActionProblem();
		}

		///<summary>"" when the actions and their keys are complete, else why not.</summary>
		private string GetActionProblem() {
			if(_listActions==null || _listActions.Count==0) {
				return "it is enabled, but Action is empty or unknown.";
			}
			if(_listActions.Count>1 && _listActions.Any(x => !LevelActions.IsHeldAction(x))) {
				return "Action="+LevelActions.Format(_listActions)+" combines a button action with others; only PausePreview and TurnOff combine.";
			}
			if(_listActions.Any(x => LevelActions.IsSettingAction(x)) && SettingDef==null) {
				return "it is enabled with Action="+LevelActions.Format(_listActions)+", but Setting (the camera setting, for example Gain) is "
					+(string.IsNullOrWhiteSpace(Setting) ? "empty." : "unknown: "+Setting+".");
			}
			if(HasAction(LevelAction.SetValue) && string.IsNullOrWhiteSpace(Value)) {
				return "it is enabled with Action=SetValue, but Value is empty.";
			}
			return "";
		}

		///<summary>True when Enabled, the binding is valid and the actions and their keys are complete.</summary>
		public bool IsLevelConfigured {
			get {
				return IsConfigured && GetActionProblem()=="";
			}
		}

		public LevelInputConfig CloneLevel() {
			LevelInputConfig levelInputConfig=(LevelInputConfig)MemberwiseClone();
			levelInputConfig._listActions=_listActions==null ? null : new List<LevelAction>(_listActions);
			return levelInputConfig;
		}
	}
}
