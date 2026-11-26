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

	///<summary>What a level input does while it is active (design 8.6). The names are the [Level.N] Action values.</summary>
	public enum LevelAction {
		///<summary>Pause the preview while the level is active.</summary>
		PausePreview=0,
		///<summary>FB-1: each press (the level becoming active) turns the handpiece light off or back on: the VideoProcAmp property
		///LightProperty of the same [Level.N] goes to its minimum, or back to the value it had (its default the first time), at most
		///LightMax. Which control bit is the button and which property drives the light come from Learn, in capture.ini.</summary>
		ToggleLight=1,
		///<summary>FB-1: each press moves a photo session to its next slot, as the right arrow does. Does nothing outside a photo
		///session.</summary>
		NextSlot=2,
		///<summary>FB-1: each press moves a photo session to its previous slot, as the left arrow does. Does nothing outside a photo
		///session.</summary>
		PreviousSlot=3,
		///<summary>FB-1: each press makes the handpiece light one step brighter (LightProperty, at most LightMax). Turns it on at the
		///lowest step when it is off.</summary>
		LightUp=4,
		///<summary>FB-1: each press makes the handpiece light one step dimmer (LightProperty), down to the lowest step; it never turns
		///the light off (ToggleLight does).</summary>
		LightDown=5,
	}

	///<summary>FB-1: the texts and kinds of the level actions, for the Learn window, the live test and the status notes.</summary>
	public static class LevelActions {
		///<summary>The actions a camera button (a press, not a held state) can have, in the order Learn offers them.</summary>
		public static readonly LevelAction[] ArrayButtonActions=new LevelAction[] { LevelAction.ToggleLight,LevelAction.LightUp,LevelAction.LightDown,
			LevelAction.NextSlot,LevelAction.PreviousSlot };

		///<summary>True for an action that acts once per press (on the active edge) instead of while the level is active.</summary>
		public static bool IsButtonAction(LevelAction? action) {
			return IsLightAction(action) || action==LevelAction.NextSlot || action==LevelAction.PreviousSlot;
		}

		///<summary>True for an action that writes the light (LightProperty): on/off, brighter, dimmer.</summary>
		public static bool IsLightAction(LevelAction? action) {
			return action==LevelAction.ToggleLight || action==LevelAction.LightUp || action==LevelAction.LightDown;
		}

		///<summary>"pause preview", "light on/off", "next slot", "previous slot"; "no action" for null.</summary>
		public static string GetText(LevelAction? action) {
			switch(action) {
				case LevelAction.PausePreview:
					return "pause preview";
				case LevelAction.ToggleLight:
					return "light on/off";
				case LevelAction.LightUp:
					return "light brighter";
				case LevelAction.LightDown:
					return "light dimmer";
				case LevelAction.NextSlot:
					return "next slot";
				case LevelAction.PreviousSlot:
					return "previous slot";
				default:
					return "no action";
			}
		}
	}

	///<summary>The camera sections of capture.ini (design 9.3): [Camera], [Trigger], [Trigger.Still], [Trigger.ControlPoll] and
	///[Level.N]. A snapshot: Read takes the values from a LocalCaptureConfig, the caller changes them, and Write puts the changed
	///ones back (the caller then saves the LocalCaptureConfig).
	///
	///Provenance (design 4 and 8.4): no trigger or level value has a default. Interface, Property, Mask and ActiveLevel are
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
		///<summary>[Trigger.ControlPoll] Enabled, Interface, Property, Mask and ActiveLevel. Never null.</summary>
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
				levelInputConfig.Action=config.ContainsKey(section,"Action")
					? ParseEnum<LevelAction>(config.GetString(section,"Action"))
					: LevelAction.PausePreview;
				int lightProperty=config.GetInt(section,"LightProperty",-1);
				levelInputConfig.LightProperty=UvcControls.IsValidProperty(UvcControlInterface.VideoProcAmp,lightProperty) ? lightProperty : (int?)null;
				int lightMax=config.GetInt(section,"LightMax",-1);
				levelInputConfig.LightMax=lightMax>=0 ? lightMax : (int?)null;
				levelInputConfig.LightOffWhileActive=config.GetBool(section,"LightOffWhileActive",false);
				cameraConfig.Levels.Add(levelInputConfig);
			}
			return cameraConfig;
		}

		private static void ReadControlInput(LocalCaptureConfig config,string section,ControlInputConfig controlInputConfig) {
			controlInputConfig.Enabled=config.GetBool(section,"Enabled",false);
			controlInputConfig.Interface=ParseEnum<UvcControlInterface>(config.GetString(section,"Interface"));
			controlInputConfig.Property=ParseProperty(config.GetString(section,"Property"));
			controlInputConfig.Mask=ParseMask(config.GetString(section,"Mask"));
			controlInputConfig.ActiveLevel=ParseEnum<ControlLevel>(config.GetString(section,"ActiveLevel"));
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
			AddControlInput(listSets,SectionTriggerControlPoll,cameraConfigOld.ControlPoll,ControlPoll??new ControlInputConfig());
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
				AddControlInput(listSets,section,levelOld??new LevelInputConfig(number) { Action=null },levelNew);
				string actionOld=levelOld==null ? "" : FormatNullable(levelOld.Action);
				string actionNew=FormatNullable(levelNew.Action);
				//A new section always gets its Action line, so the file shows what the level does.
				if(levelOld==null || actionOld!=actionNew) {
					listSets.Add(new[] { section,"Action",actionNew });
				}
				//FB-1: Learn sets the light property of a light button. A level without one keeps the key as the file has it.
				if(levelNew.LightProperty!=null && levelNew.LightProperty!=levelOld?.LightProperty) {
					listSets.Add(new[] { section,"LightProperty",FormatNullable(levelNew.LightProperty) });
				}
				//FB-1: the light's limit and the rest level. A level that is not listed (turned off) keeps them as the file has them.
				if(levelListed!=null) {
					AddIfChanged(listSets,section,"LightMax",FormatNullable(levelOld?.LightMax),FormatNullable(levelListed.LightMax));
					if(levelOld!=null || levelListed.LightOffWhileActive) {
						AddIfChanged(listSets,section,"LightOffWhileActive",levelOld?.LightOffWhileActive??false,levelListed.LightOffWhileActive);
					}
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

		private static void AddControlInput(List<string[]> listSets,string section,ControlInputConfig controlInputOld,ControlInputConfig controlInputNew) {
			AddIfChanged(listSets,section,"Enabled",controlInputOld.Enabled,controlInputNew.Enabled);
			AddIfChanged(listSets,section,"Interface",FormatNullable(controlInputOld.Interface),FormatNullable(controlInputNew.Interface));
			AddIfChanged(listSets,section,"Property",FormatNullable(controlInputOld.Property),FormatNullable(controlInputNew.Property));
			string maskOld=controlInputOld.Mask==null ? "" : ControlValueFormat.ToHex(controlInputOld.Mask.Value);
			string maskNew=controlInputNew.Mask==null || controlInputNew.Mask==0 ? "" : ControlValueFormat.ToHex(controlInputNew.Mask.Value);
			AddIfChanged(listSets,section,"Mask",maskOld,maskNew);
			AddIfChanged(listSets,section,"ActiveLevel",FormatNullable(controlInputOld.ActiveLevel),FormatNullable(controlInputNew.ActiveLevel));
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
			if(Interface==null) {
				listMissing.Add("Interface");
			}
			if(Property==null || (Interface!=null && !UvcControls.IsValidProperty(Interface.Value,Property.Value))) {
				listMissing.Add("Property");
			}
			if(Mask==null || Mask==0) {
				listMissing.Add("Mask");
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
		///<summary>Action. A missing key reads as PausePreview, the design's default; an empty or unknown value reads as null, and the
		///level then does nothing.</summary>
		public LevelAction? Action { get; set; }=LevelAction.PausePreview;
		///<summary>FB-1: [Level.N] LightProperty, the VideoProcAmp property (0-9) Action=ToggleLight turns off and on; null when missing or
		///invalid (the level then does nothing). Learn sets it for a light button; Write leaves the key as it is in the file when null.</summary>
		public int? LightProperty { get; set; }
		///<summary>FB-1: [Level.N] LightMax, the highest value the light gets on a light level (ToggleLight, LightUp, LightDown): when the
		///camera opens with the light property above it, it is set to LightMax, and turning the light on or brighter never goes above it. Null (missing, empty or negative): the
		///camera's own range is the limit.</summary>
		public int? LightMax { get; set; }
		///<summary>FB-1: [Level.N] LightOffWhileActive=1, on a level that is held (not a button): while the level is active the light of the
		///light level is off, and when it ends the light comes back on. For a handpiece's rest or cradle sensor, with
		///Action=PausePreview. Ignored on a button level and when no light level is configured.</summary>
		public bool LightOffWhileActive { get; set; }

		public LevelInputConfig(int number) {
			Number=number;
		}

		public override string GetProblem() {
			string problem=base.GetProblem();
			if(problem!="" || !Enabled) {
				return problem;
			}
			if(LevelActions.IsLightAction(Action) && LightProperty==null) {
				return "it is enabled with Action="+Action+", but LightProperty (the VideoProcAmp property of the light, 0-9) is empty or invalid.";
			}
			if(Action!=null) {
				return problem;
			}
			return "it is enabled, but Action is empty or unknown.";
		}

		///<summary>True when Enabled, the binding is valid and the action is known.</summary>
		public bool IsLevelConfigured {
			get {
				return IsConfigured && Action!=null && (!LevelActions.IsLightAction(Action) || LightProperty!=null);
			}
		}

		public LevelInputConfig CloneLevel() {
			return (LevelInputConfig)MemberwiseClone();
		}
	}
}
