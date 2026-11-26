// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImagingCapture.Camera {
	///<summary>What a tile on the live test page stands for.</summary>
	public enum LiveTileKind {
		StillTrigger=0,
		ControlTrigger=1,
		Level=2,
	}

	///<summary>One control's latest raw value on the live test page.</summary>
	public sealed class LiveValueRow {
		public ControlId Control { get; }
		///<summary>"CameraControl 6 (Focus)".</summary>
		public string Label { get; }
		///<summary>False until the control is read successfully, and after a reading that failed.</summary>
		public bool IsReadable { get; internal set; }
		public int Value { get; internal set; }
		///<summary>"" when not readable.</summary>
		public string ValueDecimal { get; internal set; }="";
		///<summary>The bit numbers that are on, "0 1 2 4 5" or "none"; "" when not readable. No hex on screen (design 8.5).</summary>
		public string ValueBitsOn { get; internal set; }="";
		///<summary>True when the trigger or a level of the configuration under test reads this control.</summary>
		public bool IsUsed { get; internal set; }

		internal LiveValueRow(ControlId control) {
			Control=control;
			Label=control.ToString();
		}
	}

	///<summary>One tile on the live test page: lit while its trigger or level is active.</summary>
	public sealed class LiveTile {
		public LiveTileKind Kind { get; }
		///<summary>The N of [Level.N]; 0 for the triggers.</summary>
		public int LevelNumber { get; }
		///<summary>"Capture: still pin", "Capture: control", "Level 1: pause preview", "Button: light on/off".</summary>
		public string Label { get; }
		///<summary>True when the configuration under test turns this input on with complete values. An unconfigured still tile still
		///lights when the still pin fires, to show that it responds.</summary>
		public bool IsConfigured { get; }
		public bool IsLit { get; internal set; }
		///<summary>Presses from this trigger that passed the shared debounce gate, or presses of a button. Always 0 for other levels and
		///unconfigured triggers.</summary>
		public int PressCount { get; internal set; }
		///<summary>FB-1: true for a level whose action acts once per press (a camera button): the tile counts presses.</summary>
		public bool IsButton { get; internal set; }

		internal LiveTile(LiveTileKind kind,int levelNumber,string label,bool isConfigured) {
			Kind=kind;
			LevelNumber=levelNumber;
			Label=label;
			IsConfigured=isConfigured;
		}
	}

	///<summary>The model behind Learn's live test page (design 8.5): each control's raw value in decimal, hex and binary, and a tile
	///per trigger and level of the configuration under test that lights while it is active. Presses go through a DebounceGate
	///like TriggerEngine's, so PhotoCount is what Capture would have taken: the acceptance check that 50 presses give 50 photos,
	///and that putting the handpiece down and picking it up gives none. Pure: the window feeds it readings (ControlSampler) and
	///still-sample times. Not thread-safe; use it on the UI thread.</summary>
	public sealed class LiveTestModel {
		private readonly List<LiveValueRow> _listRows=new List<LiveValueRow>();
		private readonly List<LiveTile> _listTiles=new List<LiveTile>();
		private readonly DebounceGate _debounceGate;
		private readonly ControlBinding _bindingControl;
		private readonly Dictionary<int,ControlBinding> _dictLevelBindings=new Dictionary<int,ControlBinding>();
		private readonly LiveTile _tileStill;
		private readonly LiveTile _tileControl;
		private bool _hasControlReading;
		///<summary>The levels read at least once, so a button held when the test starts is not counted.</summary>
		private readonly HashSet<int> _setLevelsRead=new HashSet<int>();
		///<summary>The levels whose button takes a photo.</summary>
		private readonly HashSet<int> _setLevelsTakePhoto=new HashSet<int>();
		private bool _hasStillSample;
		private long _timeStillMs;

		///<summary>cameraConfig is the configuration under test (for example LearnAnalyzer.BuildConfig's result); controls are the rows
		///to show, usually ControlSampler.FindReadable. Controls the configuration uses are added when missing.</summary>
		public LiveTestModel(CameraConfig cameraConfig,IEnumerable<ControlId> controls) {
			cameraConfig=cameraConfig??new CameraConfig();
			DebounceMs=cameraConfig.EffectiveDebounceMs;
			_debounceGate=new DebounceGate(DebounceMs);
			_tileStill=new LiveTile(LiveTileKind.StillTrigger,0,"Capture: still pin",cameraConfig.StillEnabled);
			_listTiles.Add(_tileStill);
			ControlInputConfig controlPoll=cameraConfig.ControlPoll??new ControlInputConfig();
			_bindingControl=controlPoll.IsConfigured ? controlPoll.GetBinding() : null;
			_tileControl=new LiveTile(LiveTileKind.ControlTrigger,0,"Capture: control",_bindingControl!=null);
			_listTiles.Add(_tileControl);
			foreach(LevelInputConfig levelInputConfig in (cameraConfig.Levels??new List<LevelInputConfig>()).Where(x => x!=null).OrderBy(x => x.Number)) {
				bool isConfigured=levelInputConfig.IsLevelConfigured;
				if(isConfigured) {
					_dictLevelBindings[levelInputConfig.Number]=levelInputConfig.GetBinding();
				}
				//A button is named by what it does, and counts presses.
				bool isButton=LevelActions.IsButtonAction(levelInputConfig.Action);
				string label=isButton
					? "Button: "+LevelActions.GetText(levelInputConfig)
					: "Level "+levelInputConfig.Number+": "+LevelActions.GetText(levelInputConfig);
				if(isConfigured && levelInputConfig.Action==LevelAction.TakePhoto) {
					_setLevelsTakePhoto.Add(levelInputConfig.Number);
				}
				LiveTile liveTile=new LiveTile(LiveTileKind.Level,levelInputConfig.Number,label,isConfigured);
				liveTile.IsButton=isButton;
				_listTiles.Add(liveTile);
			}
			List<ControlId> listControls=(controls??Enumerable.Empty<ControlId>()).Distinct().ToList();
			List<ControlBinding> listBindingsUsed=_dictLevelBindings.Values.ToList();
			if(_bindingControl!=null) {
				listBindingsUsed.Add(_bindingControl);
			}
			foreach(ControlBinding binding in listBindingsUsed) {
				if(!listControls.Contains(binding.Control)) {
					listControls.Add(binding.Control);
				}
			}
			foreach(ControlId control in listControls.OrderBy(x => x.SortKey)) {
				LiveValueRow row=new LiveValueRow(control);
				row.IsUsed=listBindingsUsed.Any(x => x.Control==control);
				_listRows.Add(row);
			}
		}

		///<summary>The lockout the page counts presses with ([Trigger] DebounceMs of the configuration under test, else 300).</summary>
		public int DebounceMs { get; }

		public IReadOnlyList<LiveValueRow> Rows {
			get {
				return _listRows;
			}
		}

		///<summary>The still tile, the control tile, then one per [Level.N] by N.</summary>
		public IReadOnlyList<LiveTile> Tiles {
			get {
				return _listTiles;
			}
		}

		///<summary>Presses from the configured triggers that passed the debounce gate: the photos Capture would have taken.</summary>
		public int PhotoCount { get; private set; }

		///<summary>Every control the configuration under test reads, for ControlSampler.Read. The window reads all rows' controls.</summary>
		public List<ControlId> GetControls() {
			return _listRows.Select(x => x.Control).ToList();
		}

		///<summary>Takes one reading: updates the rows, the control and level tiles, the press counts, and puts out the still tile once
		///its lockout has passed.</summary>
		public void Update(ControlSample sample) {
			if(sample==null) {
				return;
			}
			foreach(LiveValueRow row in _listRows) {
				int value;
				row.IsReadable=sample.TryGetValue(row.Control,out value);
				row.Value=row.IsReadable ? value : 0;
				row.ValueDecimal=row.IsReadable ? ControlValueFormat.ToDecimal(value) : "";
				row.ValueBitsOn=row.IsReadable ? ControlValueFormat.ToBitsOn(value) : "";
			}
			if(_bindingControl!=null) {
				int value;
				if(sample.TryGetValue(_bindingControl.Control,out value)) {
					bool isActive=_bindingControl.IsActive(value);
					if(_hasControlReading && isActive && !_tileControl.IsLit && _debounceGate.TryAccept(sample.TimeMs)) {
						_tileControl.PressCount++;
						PhotoCount++;
					}
					_tileControl.IsLit=isActive;
					_hasControlReading=true;
				}
			}
			foreach(LiveTile tile in _listTiles.Where(x => x.Kind==LiveTileKind.Level)) {
				ControlBinding binding;
				int value;
				if(_dictLevelBindings.TryGetValue(tile.LevelNumber,out binding) && sample.TryGetValue(binding.Control,out value)) {
					bool isActive=binding.IsActive(value);
					if(tile.IsButton && isActive && !tile.IsLit && _setLevelsRead.Contains(tile.LevelNumber)) {
						tile.PressCount++;
						//A TakePhoto button takes photos through the same lockout as the triggers (TriggerEngine).
						if(_setLevelsTakePhoto.Contains(tile.LevelNumber) && _debounceGate.TryAccept(sample.TimeMs)) {
							PhotoCount++;
						}
					}
					tile.IsLit=isActive;
					_setLevelsRead.Add(tile.LevelNumber);
				}
			}
			if(_hasStillSample && sample.TimeMs-_timeStillMs>=DebounceMs) {
				_tileStill.IsLit=false;
			}
		}

		///<summary>A still-pin sample arrived: lights the still tile for one lockout and, when the still trigger is configured, counts
		///a press if the debounce gate lets it through.</summary>
		public void OnStillSample(long timeMs) {
			_hasStillSample=true;
			_timeStillMs=timeMs;
			_tileStill.IsLit=true;
			if(_tileStill.IsConfigured && _debounceGate.TryAccept(timeMs)) {
				_tileStill.PressCount++;
				PhotoCount++;
			}
		}
	}
}
