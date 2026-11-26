// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ImagingCapture.Camera;
using ImagingCapture.Uvc;
using CodeBase;

namespace ImagingCapture.UI {
	///<summary>What FrmCaptureLearn is opened for.</summary>
	public enum CaptureLearnMode {
		///<summary>The Learn steps (design 8.5), ending with the live test and Save.</summary>
		Learn=0,
		///<summary>The live tiles of the saved configuration only. Nothing is learned or saved.</summary>
		ButtonTest=1,
	}

	///<summary>The pages of Learn, in order.</summary>
	public enum LearnPage {
		BaselineA=0,
		BaselineB=1,
		KeyCapture=2,
		///<summary>FB-1: the handpiece's other buttons (optional): light on/off, brighter, dimmer, next slot, previous slot.</summary>
		Buttons=3,
		Level=4,
		LiveTest=5,
	}

	///<summary>What is being recorded.</summary>
	public enum LearnRecordingKind {
		None=0,
		BaselineA,
		BaselineB,
		Key,
		Level,
		///<summary>FB-1: one of the other buttons, pressed PressCount times.</summary>
		Button,
	}

	///<summary>The page flow and decisions of the Learn window and the button test (design 8.5), with no UI and no camera: the
	///window feeds it readings (OnSample), still-sample times (OnStillSample) and the clock (Tick), all on LearnSampler's clock,
	///calls the navigation methods from its buttons, and shows the texts and flags. The analysis is LearnAnalyzer's, the test
	///page is LiveTestModel's. Not thread-safe; use it on the UI thread.
	///
	///Flow (Learn): Baseline A (Record, 3 s) - Baseline B (Record or Skip) - key capture (Record; stops by itself after the third
	///press, or Stop) - other buttons (FB-1, optional: pick what a button does, Record, three presses; again for each button; Skip
	///forgets them) - level input (Record and Stop, or Skip; pick one candidate or none) - live test (Save). Next is enabled once
	///the page's step is done; Back keeps results, but recording an earlier step again forgets the later ones (LearnAnalyzer
	///does the same). The video image category is offered on every page after the first.</summary>
	public sealed class CaptureLearnModel {
		#region Constants
		///<summary>After the third press has been released (or the third still sample has come), the key recording goes on for one
		///second before it stops by itself, so a still sample delivered just after the release, or a further press, is part of the
		///recording and is judged by the analysis instead of being cut off.</summary>
		public const int KeySettleMs=1000;
		///<summary>A key recording stops by itself after 30 s, so a camera without a usable key does not keep the window recording;
		///the result then says what was seen. Three deliberate presses take a few seconds.</summary>
		public const int KeyTimeoutMs=30000;
		///<summary>A level recording stops by itself after 30 s, which is ample to put the handpiece down and pick it up.</summary>
		public const int LevelTimeoutMs=30000;
		public const int PageCount=6;
		#endregion Constants

		#region Fields
		private readonly LearnAnalyzer _learnAnalyzer=new LearnAnalyzer();
		private readonly CameraConfig _cameraConfigCurrent;
		private readonly ImageCategoryRule _imageCategoryRule;
		private readonly bool _hasStillPin;
		private readonly bool _isStillPinConnected;
		private List<ControlId> _listControls;
		private List<ControlSample> _listSamples=new List<ControlSample>();
		private List<long> _listStillTimesMs=new List<long>();
		private long _timeRecordStartMs;
		///<summary>When the key recording first met its stop condition, or -1.</summary>
		private long _timeKeyDoneMs=-1;
		private KeyCaptureProgress _keyCaptureProgress;
		private bool _isBaselineBDone;
		private bool _isBaselineBSkipped;
		///<summary>The last Baseline B recording, usable or not, for the result text.</summary>
		private BaselineResult _baselineResultB;
		private bool _isLevelSkipped;
		private LevelCandidate _levelCandidateChosen;
		private CameraConfig _cameraConfigUnderTest;
		private LiveTestModel _liveTestModel;
		///<summary>The other buttons learned, in the order learned. Several may have the same action; a button's bits only once.</summary>
		private List<ButtonAssignment> _listButtons=new List<ButtonAssignment>();
		///<summary>What the next button recording is for.</summary>
		private LevelAction _buttonActionSelected=LevelAction.Toggle;
		///<summary>The setting the next setting button changes (a CameraSettingCatalog id); from capture.ini's first setting button, else chosen
		///here.</summary>
		private string _settingSelected="";
		///<summary>The highest value the next setting button gives its setting, as typed; from capture.ini, else "".</summary>
		private string _highestSelected="";
		///<summary>The value the next SetValue button sets, as typed.</summary>
		private string _valueSelected="";
		///<summary>Whether the level input also turns a setting off while resting (Action=PausePreview|TurnOff); from capture.ini.</summary>
		private bool _isTurnOffWhileResting;
		///<summary>The setting the level input turns off while resting: capture.ini's, else the first setting button's.</summary>
		private string _settingRest="";
		///<summary>FB-1: the last button recording's result, for the result text.</summary>
		private ButtonLearnResult _buttonLearnResultLast;
		private bool _isButtonsSkipped;
		#endregion Fields

		#region Constructor
		///<summary>cameraConfigCurrent is what capture.ini holds now: Learn keeps its [Camera] values and PollMs, and the button test
		///shows its triggers. categoryDefNum is the video image category now (0: the category selected in Imaging), and
		///isCategoryEditable whether the user may change it. hasStillPin and isStillPinConnected describe the opened camera.</summary>
		public CaptureLearnModel(CaptureLearnMode mode,CameraConfig cameraConfigCurrent,ImageCategoryRule imageCategoryRule,long categoryDefNum,
			bool isCategoryEditable,bool hasStillPin,bool isStillPinConnected)
		{
			Mode=mode;
			_cameraConfigCurrent=(cameraConfigCurrent??new CameraConfig()).Clone();
			_imageCategoryRule=imageCategoryRule??throw new ArgumentNullException(nameof(imageCategoryRule));
			CategoryDefNum=categoryDefNum;
			CategoryDefNumOriginal=categoryDefNum;
			IsCategoryEditable=isCategoryEditable;
			_hasStillPin=hasStillPin;
			_isStillPinConnected=isStillPinConnected;
			LevelInputConfig levelSetting=_cameraConfigCurrent.Levels.FirstOrDefault(x => x!=null && LevelActions.IsButtonAction(x.Action)
				&& LevelActions.IsSettingAction(x.Action) && x.SettingDef!=null);
			_settingSelected=levelSetting?.SettingDef.Id??"";
			_highestSelected=levelSetting?.Highest??"";
			LevelInputConfig levelRest=_cameraConfigCurrent.Levels.FirstOrDefault(x => x!=null && x.Enabled && x.HasAction(LevelAction.TurnOff)
				&& x.SettingDef!=null);
			_isTurnOffWhileResting=levelRest!=null;
			_settingRest=levelRest?.SettingDef.Id??"";
			if(Mode==CaptureLearnMode.ButtonTest) {
				Page=LearnPage.LiveTest;
				_cameraConfigUnderTest=_cameraConfigCurrent.Clone();
				//Only the controls the saved configuration uses are read.
				_liveTestModel=new LiveTestModel(_cameraConfigUnderTest,new List<ControlId>());
				_listControls=_liveTestModel.GetControls();
			}
		}
		#endregion Constructor

		#region Properties
		public CaptureLearnMode Mode { get; }
		public LearnPage Page { get; private set; }
		public LearnRecordingKind RecordingKind { get; private set; }
		public LearnAnalyzer Analyzer {
			get {
				return _learnAnalyzer;
			}
		}

		///<summary>Incremented whenever the page, a result or a choice changes (not for every reading), so the window refills its
		///lists only then.</summary>
		public int Version { get; private set; }

		///<summary>Incremented whenever a new LiveTestModel is made, so the window rebuilds its tiles.</summary>
		public int LiveTestVersion { get; private set; }

		public bool IsRecording {
			get {
				return RecordingKind!=LearnRecordingKind.None;
			}
		}

		///<summary>True once the controls to read are known (SetControls). Recording needs them.</summary>
		public bool IsReady {
			get {
				return _listControls!=null;
			}
		}

		///<summary>The controls read, or null before SetControls.</summary>
		public IReadOnlyList<ControlId> Controls {
			get {
				return _listControls;
			}
		}

		///<summary>What the sampler should read: null in Learn (it finds every readable control), the configured controls in the
		///button test.</summary>
		public List<ControlId> GetControlsToRead() {
			if(Mode==CaptureLearnMode.ButtonTest) {
				return _listControls.ToList();
			}
			return null;
		}

		///<summary>The live test of the configuration under test, or null when not on the test page.</summary>
		public LiveTestModel LiveTest {
			get {
				return _liveTestModel;
			}
		}

		///<summary>The configuration the test page shows: Learn's result with the chosen level, or the saved one in the button test.
		///Null when not on the test page.</summary>
		public CameraConfig ConfigUnderTest {
			get {
				return _cameraConfigUnderTest;
			}
		}

		///<summary>Live progress of the key recording.</summary>
		public KeyCaptureProgress KeyProgress {
			get {
				return _keyCaptureProgress;
			}
		}

		///<summary>Still samples in the current or last recording.</summary>
		public int StillSampleCount {
			get {
				return _listStillTimesMs.Count;
			}
		}
		#endregion Properties

		#region Controls
		///<summary>The controls the sampler found. Ignored after the first call and in the button test.</summary>
		public void SetControls(IEnumerable<ControlId> controls) {
			if(_listControls!=null) {
				return;
			}
			_listControls=(controls??Enumerable.Empty<ControlId>()).Distinct().ToList();
			Version++;
		}
		#endregion Controls

		#region Enabled flags
		public bool CanGoBack {
			get {
				return Mode==CaptureLearnMode.Learn && !IsRecording && Page>LearnPage.BaselineA;
			}
		}

		public bool CanGoNext {
			get {
				if(Mode!=CaptureLearnMode.Learn || IsRecording) {
					return false;
				}
				return IsPageDone(Page);
			}
		}

		public bool CanRecord {
			get {
				return Mode==CaptureLearnMode.Learn && IsReady && !IsRecording && Page!=LearnPage.LiveTest
					&& (Page!=LearnPage.Buttons || GetButtonProblem()=="");
			}
		}

		///<summary>Key and level recordings are stopped by the user; baselines run their fixed time.</summary>
		public bool CanStop {
			get {
				return RecordingKind==LearnRecordingKind.Key || RecordingKind==LearnRecordingKind.Level || RecordingKind==LearnRecordingKind.Button;
			}
		}

		public bool CanSkip {
			get {
				return Mode==CaptureLearnMode.Learn && !IsRecording && (Page==LearnPage.BaselineB || Page==LearnPage.Buttons || Page==LearnPage.Level);
			}
		}

		///<summary>FB-1: the action and light setting pickers of the Other buttons page.</summary>
		public bool IsButtonPickerVisible {
			get {
				return Mode==CaptureLearnMode.Learn && Page==LearnPage.Buttons;
			}
		}

		///<summary>Save is on the test page of Learn only, once a configuration was built and nothing is recording.</summary>
		public bool CanSave {
			get {
				return Mode==CaptureLearnMode.Learn && Page==LearnPage.LiveTest && !IsRecording && _cameraConfigUnderTest!=null;
			}
		}

		public bool IsKeyPickerVisible {
			get {
				return Mode==CaptureLearnMode.Learn && Page==LearnPage.KeyCapture && _learnAnalyzer.KeyResult!=null
					&& _learnAnalyzer.KeyResult.Candidates.Count>1;
			}
		}

		public bool IsLevelPickerVisible {
			get {
				return Mode==CaptureLearnMode.Learn && Page==LearnPage.Level;
			}
		}

		///<summary>The raw value rows show on Learn's test page; the button test shows the tiles only.</summary>
		public bool IsValueGridVisible {
			get {
				return Mode==CaptureLearnMode.Learn && Page==LearnPage.LiveTest;
			}
		}

		public bool IsTilesVisible {
			get {
				return Page==LearnPage.LiveTest;
			}
		}

		///<summary>True when the step of a page is done, so Next may leave it.</summary>
		private bool IsPageDone(LearnPage page) {
			switch(page) {
				case LearnPage.BaselineA:
					return _learnAnalyzer.BaselineA!=null && _learnAnalyzer.BaselineA.IsUsable;
				case LearnPage.BaselineB:
					return _isBaselineBDone;
				case LearnPage.KeyCapture:
					return _learnAnalyzer.KeyResult!=null;
				case LearnPage.Buttons:
				case LearnPage.Level:
					return true;
				default:
					return false;
			}
		}
		#endregion Enabled flags

		#region Navigation
		///<summary>Goes to the next page when CanGoNext. Entering the test page builds the configuration under test.</summary>
		public bool GoNext() {
			if(!CanGoNext) {
				return false;
			}
			Page=Page+1;
			if(Page==LearnPage.LiveTest) {
				BuildLiveTest();
			}
			Version++;
			return true;
		}

		///<summary>Goes to the previous page when CanGoBack. Results are kept; leaving the test page drops its configuration.</summary>
		public bool GoBack() {
			if(!CanGoBack) {
				return false;
			}
			if(Page==LearnPage.LiveTest) {
				_liveTestModel=null;
				_cameraConfigUnderTest=null;
			}
			Page=Page-1;
			Version++;
			return true;
		}

		///<summary>Baseline B: goes on without it. Level: goes on without a level input. Then moves to the next page.</summary>
		public bool Skip() {
			if(!CanSkip) {
				return false;
			}
			if(Page==LearnPage.BaselineB) {
				_learnAnalyzer.SkipBaselineB();
				_isBaselineBDone=true;
				_isBaselineBSkipped=true;
				_baselineResultB=null;
				ClearLevelChoice();
				ClearButtons();
			}
			else if(Page==LearnPage.Buttons) {
				ClearButtons();
				_isButtonsSkipped=true;
			}
			else {
				_levelCandidateChosen=null;
				_isLevelSkipped=true;
			}
			Version++;
			return GoNext();
		}
		#endregion Navigation

		#region Recording
		///<summary>Starts recording this page's step at nowMs (LearnSampler.NowMs). Readings and still samples from then on are kept.</summary>
		public bool StartRecording(long nowMs) {
			if(!CanRecord) {
				return false;
			}
			switch(Page) {
				case LearnPage.BaselineA:
					RecordingKind=LearnRecordingKind.BaselineA;
					break;
				case LearnPage.BaselineB:
					RecordingKind=LearnRecordingKind.BaselineB;
					break;
				case LearnPage.KeyCapture:
					RecordingKind=LearnRecordingKind.Key;
					break;
				case LearnPage.Buttons:
					RecordingKind=LearnRecordingKind.Button;
					break;
				default:
					RecordingKind=LearnRecordingKind.Level;
					break;
			}
			_timeRecordStartMs=nowMs;
			_timeKeyDoneMs=-1;
			_keyCaptureProgress=new KeyCaptureProgress(0,false);
			_listSamples=new List<ControlSample>();
			_listStillTimesMs=new List<long>();
			Version++;
			return true;
		}

		///<summary>Stops a key or level recording and analyzes it.</summary>
		public bool StopRecording() {
			if(!CanStop) {
				return false;
			}
			FinishRecording();
			return true;
		}

		///<summary>One reading from the sampler. Kept while recording; fed to the live test on the test page.</summary>
		public void OnSample(ControlSample sample) {
			if(sample==null) {
				return;
			}
			if(!IsRecording) {
				if(Page==LearnPage.LiveTest && _liveTestModel!=null) {
					_liveTestModel.Update(sample);
				}
				return;
			}
			if(sample.TimeMs<_timeRecordStartMs) {
				return;//taken before Record was clicked
			}
			if(IsBaselineRecording() && sample.TimeMs-_timeRecordStartMs>=LearnAnalyzer.BaselineMs) {
				FinishRecording();
				return;
			}
			_listSamples.Add(sample);
			if(RecordingKind==LearnRecordingKind.Key) {
				_keyCaptureProgress=_learnAnalyzer.GetKeyCaptureProgress(_listSamples);
				UpdateKeyDone(sample.TimeMs);
			}
			else if(RecordingKind==LearnRecordingKind.Button) {
				//Every button counts here, a used one too, so pressing one already in use ends the recording and the result says so.
				_keyCaptureProgress=_learnAnalyzer.GetKeyCaptureProgress(_listSamples);
				UpdateKeyDone(sample.TimeMs);
			}
			CheckTime(sample.TimeMs);
		}

		///<summary>A still-pin sample at timeMs. Kept while recording; fed to the live test on the test page.</summary>
		public void OnStillSample(long timeMs) {
			if(!IsRecording) {
				if(Page==LearnPage.LiveTest && _liveTestModel!=null) {
					_liveTestModel.OnStillSample(timeMs);
				}
				return;
			}
			if(timeMs<_timeRecordStartMs) {
				return;
			}
			_listStillTimesMs.Add(timeMs);
			if(RecordingKind==LearnRecordingKind.Key) {
				UpdateKeyDone(timeMs);
			}
		}

		///<summary>The clock, from the window's timer: ends recordings whose time is up even when readings stop coming.</summary>
		public void Tick(long nowMs) {
			if(!IsRecording) {
				return;
			}
			CheckTime(nowMs);
		}

		private bool IsBaselineRecording() {
			return RecordingKind==LearnRecordingKind.BaselineA || RecordingKind==LearnRecordingKind.BaselineB;
		}

		///<summary>The key recording is done when no key is held and the third press was released or the third still sample came.</summary>
		private void UpdateKeyDone(long nowMs) {
			bool isStillDone=RecordingKind==LearnRecordingKind.Key && _listStillTimesMs.Count>=LearnAnalyzer.PressCount;
			bool isDone=!_keyCaptureProgress.IsKeyDown
				&& (_keyCaptureProgress.PressesSeen>=LearnAnalyzer.PressCount || isStillDone);
			if(!isDone) {
				_timeKeyDoneMs=-1;
				return;
			}
			if(_timeKeyDoneMs<0) {
				_timeKeyDoneMs=nowMs;
			}
		}

		private void CheckTime(long nowMs) {
			if(!IsRecording) {
				return;
			}
			long elapsedMs=nowMs-_timeRecordStartMs;
			bool isTimeUp;
			switch(RecordingKind) {
				case LearnRecordingKind.Key:
				case LearnRecordingKind.Button:
					isTimeUp=elapsedMs>=KeyTimeoutMs || (_timeKeyDoneMs>=0 && nowMs-_timeKeyDoneMs>=KeySettleMs);
					break;
				case LearnRecordingKind.Level:
					isTimeUp=elapsedMs>=LevelTimeoutMs;
					break;
				default:
					isTimeUp=elapsedMs>=LearnAnalyzer.BaselineMs;
					break;
			}
			if(isTimeUp) {
				FinishRecording();
			}
		}

		///<summary>Analyzes what was recorded. A new baseline or key recording forgets the steps after it, as LearnAnalyzer does.</summary>
		private void FinishRecording() {
			LearnRecordingKind recordingKind=RecordingKind;
			RecordingKind=LearnRecordingKind.None;
			List<ControlSample> listSamples=_listSamples;
			List<long> listStillTimesMs=_listStillTimesMs;
			switch(recordingKind) {
				case LearnRecordingKind.BaselineA:
					_learnAnalyzer.SetBaselineA(listSamples);
					_isBaselineBDone=false;
					_isBaselineBSkipped=false;
					_baselineResultB=null;
					ClearLevelChoice();
					ClearButtons();
					break;
				case LearnRecordingKind.BaselineB:
					_baselineResultB=_learnAnalyzer.SetBaselineB(listSamples);
					_isBaselineBDone=_baselineResultB.IsUsable;
					_isBaselineBSkipped=false;
					ClearLevelChoice();
					ClearButtons();
					break;
				case LearnRecordingKind.Key:
					_learnAnalyzer.SetKeyCapture(listSamples,listStillTimesMs);
					ClearLevelChoice();
					ClearButtons();
					break;
				case LearnRecordingKind.Button:
					FinishButton(listSamples);
					break;
				case LearnRecordingKind.Level:
					_learnAnalyzer.SetLevelCapture(listSamples);
					_isLevelSkipped=false;
					//The user recorded the level on purpose, so its best candidate is preselected; None stays one click away.
					_levelCandidateChosen=GetLevelCandidates().FirstOrDefault(x => x==_learnAnalyzer.LevelResult.Chosen);
					break;
			}
			Version++;
		}

		private void ClearLevelChoice() {
			_levelCandidateChosen=null;
			_isLevelSkipped=false;
		}
		#endregion Recording

		#region Other buttons
		///<summary>The buttons learned, in the order learned.</summary>
		public IReadOnlyList<ButtonAssignment> Buttons {
			get {
				return _listButtons;
			}
		}

		///<summary>What the next button recording is for.</summary>
		public LevelAction ButtonActionSelected {
			get {
				return _buttonActionSelected;
			}
		}

		///<summary>The setting the next setting button changes (a CameraSettingCatalog id), or "" when none is chosen.</summary>
		public string SettingSelected {
			get {
				return _settingSelected;
			}
		}

		///<summary>The highest value the next setting button gives its setting, as typed; "" for the camera's maximum.</summary>
		public string HighestSelected {
			get {
				return _highestSelected;
			}
		}

		///<summary>The value the next SetValue button sets, as typed.</summary>
		public string ValueSelected {
			get {
				return _valueSelected;
			}
		}

		///<summary>Sets the highest value of the next setting button (as typed; "" for none), on the Other buttons page.</summary>
		public bool SetHighest(string highest) {
			if(Mode!=CaptureLearnMode.Learn || IsRecording || Page!=LearnPage.Buttons) {
				return false;
			}
			string highestNew=(highest??"").Trim();
			if(_highestSelected!=highestNew) {
				_highestSelected=highestNew;
				Version++;
			}
			return true;
		}

		///<summary>Sets the value of the next SetValue button (as typed), on the Other buttons page.</summary>
		public bool SetValue(string value) {
			if(Mode!=CaptureLearnMode.Learn || IsRecording || Page!=LearnPage.Buttons) {
				return false;
			}
			string valueNew=(value??"").Trim();
			if(_valueSelected!=valueNew) {
				_valueSelected=valueNew;
				Version++;
			}
			return true;
		}

		///<summary>Whether the level input also turns a setting off while resting.</summary>
		public bool IsTurnOffWhileResting {
			get {
				return _isTurnOffWhileResting;
			}
		}

		///<summary>The setting the level input turns off while resting: capture.ini's, else the first setting button's; "" when none.</summary>
		public string SettingRest {
			get {
				if(_settingRest!="") {
					return _settingRest;
				}
				return _listButtons.FirstOrDefault(x => x.Setting!="")?.Setting??"";
			}
		}

		///<summary>"Gain" for the setting the level input turns off while resting; "" when none.</summary>
		public string SettingRestName {
			get {
				CameraSettingDef cameraSettingDef;
				return CameraSettingCatalog.TryGet(SettingRest,out cameraSettingDef) ? cameraSettingDef.DisplayName : SettingRest;
			}
		}

		///<summary>A setting can go off while resting, on the Level page, when a level input is chosen and a setting is known.</summary>
		public bool IsTurnOffWhileRestingAvailable {
			get {
				return Mode==CaptureLearnMode.Learn && Page==LearnPage.Level && _levelCandidateChosen!=null && SettingRest!="";
			}
		}

		///<summary>Sets whether the level input also turns SettingRest off while resting, on the Level page.</summary>
		public bool SetTurnOffWhileResting(bool isTurnOff) {
			if(Mode!=CaptureLearnMode.Learn || IsRecording || Page!=LearnPage.Level) {
				return false;
			}
			if(_isTurnOffWhileResting!=isTurnOff) {
				_isTurnOffWhileResting=isTurnOff;
				Version++;
			}
			return true;
		}

		///<summary>Chooses what the next button recording is for (one of LevelActions.ArrayButtonActions).</summary>
		public bool SelectButtonAction(LevelAction action) {
			if(Mode!=CaptureLearnMode.Learn || IsRecording || Page!=LearnPage.Buttons || !LevelActions.ArrayButtonActions.Contains(action)) {
				return false;
			}
			if(_buttonActionSelected!=action) {
				_buttonActionSelected=action;
				_buttonLearnResultLast=null;
				Version++;
			}
			return true;
		}

		///<summary>Chooses the setting of the next setting button (a CameraSettingCatalog id or display name), or "" for none.</summary>
		public bool SetSetting(string setting) {
			if(Mode!=CaptureLearnMode.Learn || IsRecording || Page!=LearnPage.Buttons) {
				return false;
			}
			string settingNew="";
			if(!string.IsNullOrWhiteSpace(setting)) {
				CameraSettingDef cameraSettingDef;
				if(!CameraSettingCatalog.TryGet(setting,out cameraSettingDef)) {
					return false;
				}
				settingNew=cameraSettingDef.Id;
			}
			if(_settingSelected!=settingNew) {
				_settingSelected=settingNew;
				Version++;
			}
			return true;
		}

		///<summary>The settings a setting button can change: those of the catalog the camera reports, plus the one chosen, in catalog order.</summary>
		public List<CameraSettingDef> GetSettingChoices() {
			List<ControlId> listControls=_listControls??new List<ControlId>();
			return CameraSettingCatalog.All.Where(x => listControls.Contains(new ControlId(x.Interface,x.Property))
				|| string.Equals(x.Id,_settingSelected,StringComparison.OrdinalIgnoreCase)).ToList();
		}

		///<summary>Forgets the button learned last.</summary>
		public bool ForgetLastButton() {
			if(Mode!=CaptureLearnMode.Learn || IsRecording || Page!=LearnPage.Buttons) {
				return false;
			}
			if(_listButtons.Count>0) {
				_listButtons.RemoveAt(_listButtons.Count-1);
				_buttonLearnResultLast=null;
				Version++;
			}
			return true;
		}

		///<summary>The capture key and the buttons learned: a button recording never learns their bits again.</summary>
		private List<ControlBinding> GetBindingsTaken() {
			List<ControlBinding> listBindings=_listButtons.Select(x => x.Binding).ToList();
			if(KeyChosen!=null) {
				listBindings.Add(KeyChosen.Binding);
			}
			return listBindings;
		}

		///<summary>Adds the button a recording found, with the selected action, setting, highest value and value.</summary>
		private void FinishButton(List<ControlSample> listSamples) {
			_buttonLearnResultLast=_learnAnalyzer.SetButtonCapture(listSamples,GetBindingsTaken());
			_isButtonsSkipped=false;
			if(_buttonLearnResultLast.Chosen==null) {
				return;
			}
			_listButtons.Add(new ButtonAssignment(_buttonActionSelected,_buttonLearnResultLast.Chosen.Binding,_settingSelected,_highestSelected,_valueSelected));
		}

		///<summary>"" when a button with the selected action can be recorded, else what is missing (a setting action needs its setting, SetValue
		///its value).</summary>
		public string GetButtonProblem() {
			if(LevelActions.IsSettingAction(_buttonActionSelected) && _settingSelected=="") {
				return "Choose the setting this button changes.";
			}
			if(_buttonActionSelected==LevelAction.SetValue && _valueSelected=="") {
				return "Type the value this button sets.";
			}
			return "";
		}

		private void ClearButtons() {
			_listButtons=new List<ButtonAssignment>();
			_buttonLearnResultLast=null;
			_isButtonsSkipped=false;
		}

		///<summary>The setting the level input turns off while resting, or null when it does not.</summary>
		private string GetSettingTurnOffForConfig() {
			return _isTurnOffWhileResting && SettingRest!="" ? SettingRest : null;
		}
		#endregion Other buttons

		#region Choices
		///<summary>The key candidates of the key step, in LearnAnalyzer's order; KeyChosen is the one in use. Empty before the key step.</summary>
		public IReadOnlyList<KeyCandidate> GetKeyCandidates() {
			return _learnAnalyzer.KeyResult?.Candidates??new List<KeyCandidate>();
		}

		public KeyCandidate KeyChosen {
			get {
				return _learnAnalyzer.KeyResult?.Chosen;
			}
		}

		///<summary>Uses another key candidate. A level choice that would share the new key's bits is dropped.</summary>
		public bool ChooseKey(KeyCandidate keyCandidate) {
			if(Mode!=CaptureLearnMode.Learn || IsRecording || Page!=LearnPage.KeyCapture) {
				return false;
			}
			if(!_learnAnalyzer.ChooseKey(keyCandidate)) {
				return false;
			}
			if(_levelCandidateChosen!=null && !GetLevelCandidates().Contains(_levelCandidateChosen)) {
				_levelCandidateChosen=null;
			}
			//FB-1: a button that shares the new key's bits is forgotten.
			_listButtons.RemoveAll(x => x.Binding.Control==keyCandidate.Binding.Control && (x.Binding.Mask & keyCandidate.Binding.Mask)!=0);
			Version++;
			return true;
		}

		///<summary>The level inputs offered: those the level step found, then those Baseline B found that are not already listed.
		///A candidate that uses bits of the chosen key on the same control is never offered, so a level can never share the key's
		///bits.</summary>
		public List<LevelCandidate> GetLevelCandidates() {
			List<LevelCandidate> listCandidates=new List<LevelCandidate>();
			if(_learnAnalyzer.LevelResult!=null) {
				listCandidates.AddRange(_learnAnalyzer.LevelResult.Candidates);
			}
			foreach(LevelCandidate levelCandidate in _learnAnalyzer.BaselineLevelCandidates) {
				if(listCandidates.Any(x => IsSameBinding(x.Binding,levelCandidate.Binding))) {
					continue;
				}
				listCandidates.Add(levelCandidate);
			}
			ControlBinding bindingKey=KeyChosen?.Binding;
			if(bindingKey!=null) {
				listCandidates=listCandidates.Where(x => x.Binding.Control!=bindingKey.Control || (x.Binding.Mask & bindingKey.Mask)==0).ToList();
			}
			//FB-1: nor one that shares a learned button's bits.
			foreach(ButtonAssignment buttonAssignment in _listButtons) {
				listCandidates=listCandidates.Where(x => x.Binding.Control!=buttonAssignment.Binding.Control
					|| (x.Binding.Mask & buttonAssignment.Binding.Mask)==0).ToList();
			}
			return listCandidates;
		}

		///<summary>The accepted level input, or null for none.</summary>
		public LevelCandidate LevelChosen {
			get {
				return _levelCandidateChosen;
			}
		}

		///<summary>Accepts one of GetLevelCandidates, or null for no level input.</summary>
		public bool ChooseLevel(LevelCandidate levelCandidate) {
			if(Mode!=CaptureLearnMode.Learn || IsRecording || Page!=LearnPage.Level) {
				return false;
			}
			if(levelCandidate!=null && !GetLevelCandidates().Contains(levelCandidate)) {
				return false;
			}
			_levelCandidateChosen=levelCandidate;
			Version++;
			return true;
		}

		private static bool IsSameBinding(ControlBinding binding1,ControlBinding binding2) {
			return binding1.Control==binding2.Control && binding1.Mask==binding2.Mask && binding1.ActiveLevel==binding2.ActiveLevel;
		}
		#endregion Choices

		#region Live test
		private void BuildLiveTest() {
			_cameraConfigUnderTest=_learnAnalyzer.BuildConfig(_cameraConfigCurrent,_levelCandidateChosen,_listButtons,GetSettingTurnOffForConfig());
			_liveTestModel=new LiveTestModel(_cameraConfigUnderTest,_listControls??new List<ControlId>());
			LiveTestVersion++;
		}

		///<summary>Starts the test page's counts again.</summary>
		public void ResetLiveTest() {
			if(Page!=LearnPage.LiveTest || _cameraConfigUnderTest==null) {
				return;
			}
			_liveTestModel=new LiveTestModel(_cameraConfigUnderTest,Mode==CaptureLearnMode.ButtonTest ? new List<ControlId>() : (_listControls??new List<ControlId>()));
			LiveTestVersion++;
			Version++;
		}

		///<summary>The configuration to write to capture.ini, or null when Save is not possible.</summary>
		public CameraConfig GetConfigToSave() {
			if(!CanSave) {
				return null;
			}
			return _cameraConfigUnderTest.Clone();
		}
		#endregion Live test

		#region Video image category
		///<summary>The video image category chosen in the window. 0 means the category selected in Imaging.</summary>
		public long CategoryDefNum { get; private set; }
		///<summary>The video image category when the window opened.</summary>
		public long CategoryDefNumOriginal { get; }
		///<summary>False when the user may not change the category (it needs the Setup permission).</summary>
		public bool IsCategoryEditable { get; }

		///<summary>Every page of Learn after the first offers the category (design 8.5). The button test does not.</summary>
		public bool IsCategoryVisible {
			get {
				return Mode==CaptureLearnMode.Learn && Page!=LearnPage.BaselineA;
			}
		}

		public bool IsCategoryChanged {
			get {
				return CategoryDefNum!=CategoryDefNumOriginal;
			}
		}

		public bool SetCategory(long defNum) {
			if(!IsCategoryEditable || defNum<0) {
				return false;
			}
			if(CategoryDefNum!=defNum) {
				CategoryDefNum=defNum;
				Version++;
			}
			return true;
		}

		///<summary>"" or the patient picture warning for the chosen category (ImageCategoryRule).</summary>
		public string GetCategoryWarning() {
			return _imageCategoryRule.GetWarning(CategoryDefNum);
		}
		#endregion Video image category

		#region Texts
		///<summary>"Step 2 of 5: Baseline B, resting (optional)", or the button test's title.</summary>
		public string GetStepTitle() {
			if(Mode==CaptureLearnMode.ButtonTest) {
				return "Button test";
			}
			string name;
			switch(Page) {
				case LearnPage.BaselineA:
					name="Baseline A, held";
					break;
				case LearnPage.BaselineB:
					name="Baseline B, resting (optional)";
					break;
				case LearnPage.KeyCapture:
					name="Capture key";
					break;
				case LearnPage.Buttons:
					name="Other buttons (optional)";
					break;
				case LearnPage.Level:
					name="Level input (optional)";
					break;
				default:
					name="Test and save";
					break;
			}
			return "Step "+((int)Page+1).ToString(CultureInfo.InvariantCulture)+" of "+PageCount.ToString(CultureInfo.InvariantCulture)+": "+name;
		}

		public string GetInstructions() {
			if(Mode==CaptureLearnMode.ButtonTest) {
				return "Press the camera's key: the Capture tile lights and its count goes up by one for each press. A button tile counts "
					+"the presses of its button, and a level tile shows whether the level input is active. Nothing is changed or saved here.";
			}
			switch(Page) {
				case LearnPage.BaselineA:
					return "Hold the handpiece in your hand the way you hold it to take a photo. Do not press any key. "
						+"Click Record and keep it like that for "+(LearnAnalyzer.BaselineMs/1000).ToString(CultureInfo.InvariantCulture)+" seconds.";
				case LearnPage.BaselineB:
					return "Put the handpiece in the resting position you use between photos (on the tray, or in a holder if there is one). "
						+"Click Record and leave it there for "+(LearnAnalyzer.BaselineMs/1000).ToString(CultureInfo.InvariantCulture)+" seconds. "
						+"Anything that changes between held and resting is never used as a key. Click Skip if there is no resting position.";
				case LearnPage.KeyCapture:
					string text="Hold the handpiece as in step 1. Click Record, then press and release the key you want to use for Capture, "
						+LearnAnalyzer.PressCount.ToString(CultureInfo.InvariantCulture)+" times, with a short pause between presses. "
						+"Recording stops by itself after the last press.";
					if(_hasStillPin && !_isStillPinConnected) {
						text+=" The camera has a still pin, but it is not connected in this preview, so a still-pin key cannot be learned now.";
					}
					return text;
				case LearnPage.Buttons:
					return "Optional: other buttons on the handpiece can change a camera setting (turn it on and off, raise or lower it, set it to "
						+"a value) or do what a key does in the session (take a photo, next or previous slot, retake, freeze, record, ...). "
						+"Choose what a button does and, for a setting, which one, click Record, then press and release that button "
						+LearnAnalyzer.PressCount.ToString(CultureInfo.InvariantCulture)+" times. Repeat for each button. Click Next when done, or Skip "
						+"to use none.";
				case LearnPage.Level:
					return "Optional: a level input pauses the preview while the handpiece rests, and can also turn a setting off then. Click "
						+"Record, put the handpiece down in its resting position, wait two seconds, pick it up again, then click Stop. Then choose the "
						+"level input below, or None. Click Skip to go on without one.";
				default:
					return "Press the key: the Capture tile lights, and the photo count goes up by one for each press. Press each other button: "
						+"its tile counts the presses, and the photo count must not change. Put the handpiece down and pick it up a few times: the "
						+"photo count must not change. Then click Save.";
			}
		}

		///<summary>What is happening now: recording progress, or the photo count on the test page. "" otherwise.</summary>
		public string GetProgressText(long nowMs) {
			long elapsedMs=Math.Max(0,nowMs-_timeRecordStartMs);
			switch(RecordingKind) {
				case LearnRecordingKind.BaselineA:
				case LearnRecordingKind.BaselineB:
					return "Recording: "+FormatSeconds(Math.Min(elapsedMs,LearnAnalyzer.BaselineMs))+" of "+FormatSeconds(LearnAnalyzer.BaselineMs)+" s. Keep still.";
				case LearnRecordingKind.Button:
					return "Presses seen: "+_keyCaptureProgress.PressesSeen.ToString(CultureInfo.InvariantCulture)
						+" of "+LearnAnalyzer.PressCount.ToString(CultureInfo.InvariantCulture)+(_keyCaptureProgress.IsKeyDown ? " (button held)" : "")+".";
				case LearnRecordingKind.Key:
					string text="Presses seen: "+_keyCaptureProgress.PressesSeen.ToString(CultureInfo.InvariantCulture)
						+" of "+LearnAnalyzer.PressCount.ToString(CultureInfo.InvariantCulture)+(_keyCaptureProgress.IsKeyDown ? " (key held)" : "")+".";
					if(_isStillPinConnected) {
						text+=" Still pin samples: "+_listStillTimesMs.Count.ToString(CultureInfo.InvariantCulture)+".";
					}
					return text;
				case LearnRecordingKind.Level:
					return "Recording: "+FormatSeconds(elapsedMs)+" s. Put the handpiece down, wait, pick it up, then click Stop.";
			}
			if(Page==LearnPage.LiveTest && _liveTestModel!=null) {
				return "Photos Capture would take: "+_liveTestModel.PhotoCount.ToString(CultureInfo.InvariantCulture);
			}
			if(!IsReady) {
				return "Reading the camera's controls...";
			}
			return "";
		}

		///<summary>What the current page's step found, with the analysis notes. "" before the step was recorded.</summary>
		public string GetResultText() {
			if(Page==LearnPage.LiveTest) {
				return GetConfigSummary(_cameraConfigUnderTest);
			}
			if(IsRecording) {
				return "";
			}
			switch(Page) {
				case LearnPage.BaselineA:
					return GetBaselineAText();
				case LearnPage.BaselineB:
					return GetBaselineBText();
				case LearnPage.KeyCapture:
					return GetKeyText();
				case LearnPage.Buttons:
					return GetButtonsText();
				default:
					return GetLevelText();
			}
		}

		private string GetBaselineAText() {
			BaselineResult baselineResult=_learnAnalyzer.BaselineA;
			if(baselineResult==null) {
				if(IsReady && _listControls.Count==0) {
					return "The camera reports no readable control, so only a still-pin key can be learned.";
				}
				return "";
			}
			if(!baselineResult.IsUsable) {
				return baselineResult.Problem;
			}
			List<string> listLines=new List<string>();
			listLines.Add("Readings taken: "+baselineResult.SampleCount.ToString(CultureInfo.InvariantCulture)+".");
			if(baselineResult.StableValues.Count==0) {
				listLines.Add("No camera control kept a steady value, so no control can be used as a key. Only a still-pin key can be learned.");
			}
			else {
				listLines.Add("Steady controls: "+JoinControls(baselineResult.StableValues.Keys)+".");
			}
			if(baselineResult.Drifting.Count>0) {
				listLines.Add("Changed on their own, not used: "+JoinControls(baselineResult.Drifting)+".");
			}
			if(baselineResult.Unreadable.Count>0) {
				listLines.Add("Not always readable, not used: "+JoinControls(baselineResult.Unreadable)+".");
			}
			return string.Join("\r\n",listLines);
		}

		private string GetBaselineBText() {
			if(_isBaselineBSkipped) {
				return "Skipped: no resting position was recorded.";
			}
			if(_baselineResultB==null) {
				return "";
			}
			if(!_baselineResultB.IsUsable) {
				return _baselineResultB.Problem;
			}
			List<string> listLines=new List<string>();
			listLines.Add("Readings taken: "+_baselineResultB.SampleCount.ToString(CultureInfo.InvariantCulture)+".");
			if(_learnAnalyzer.LevelBits.Count==0) {
				listLines.Add("No control differs between held and resting.");
			}
			else {
				listLines.Add("Differ between held and resting: "
					+string.Join("; ",_learnAnalyzer.LevelBits.OrderBy(x => x.Key.SortKey).Select(x => x.Key+" bits "+ControlValueFormat.ToBitsOn(x.Value)))
					+". These bits are never used for a key, and are offered as a level input.");
			}
			List<ControlId> listUsable=_learnAnalyzer.GetUsableControls();
			List<ControlId> listExcluded=_learnAnalyzer.BaselineA.StableValues.Keys.Where(x => !listUsable.Contains(x)).OrderBy(x => x.SortKey).ToList();
			if(listExcluded.Count>0) {
				listLines.Add("Not steady at rest, not used: "+JoinControls(listExcluded)+".");
			}
			return string.Join("\r\n",listLines);
		}

		private string GetKeyText() {
			KeyLearnResult keyLearnResult=_learnAnalyzer.KeyResult;
			if(keyLearnResult==null) {
				return "";
			}
			List<string> listLines=new List<string>();
			if(keyLearnResult.Chosen!=null) {
				listLines.Add("Key found: "+keyLearnResult.Chosen.Binding+". Longest press "
					+keyLearnResult.Chosen.LongestPressMs.ToString(CultureInfo.InvariantCulture)+" ms, so the lockout after a press is "
					+keyLearnResult.DebounceMs.Value.ToString(CultureInfo.InvariantCulture)+" ms.");
			}
			if(keyLearnResult.IsStillPinKey) {
				listLines.Add("The still pin sent one sample per press, so the still-pin trigger is on.");
			}
			listLines.AddRange(keyLearnResult.Notes);
			return string.Join("\r\n",listLines);
		}

		private string GetButtonsText() {
			List<string> listLines=new List<string>();
			if(_isButtonsSkipped) {
				listLines.Add("Skipped: no other buttons.");
			}
			if(_buttonLearnResultLast!=null) {
				if(_buttonLearnResultLast.Chosen!=null) {
					listLines.Add("Button found: "+_buttonLearnResultLast.Chosen.Binding+".");
				}
				listLines.AddRange(_buttonLearnResultLast.Notes);
			}
			if(_listButtons.Count==0) {
				listLines.Add("No button learned yet. Choose what a button does, then record its presses.");
			}
			foreach(ButtonAssignment buttonAssignment in _listButtons) {
				string text=Capitalize(buttonAssignment.ToString());
				if(buttonAssignment.Highest!="") {
					text+=", at most "+buttonAssignment.Highest;
				}
				listLines.Add(text+".");
			}
			string problem=GetButtonProblem();
			if(problem!="") {
				listLines.Add(problem);
			}
			return string.Join("\r\n",listLines);
		}

		///<summary>"Turn Gain on/off" for "turn Gain on/off".</summary>
		private static string Capitalize(string text) {
			return string.IsNullOrEmpty(text) ? "" : char.ToUpperInvariant(text[0])+text.Substring(1);
		}

		private string GetLevelText() {
			List<string> listLines=new List<string>();
			if(_isLevelSkipped) {
				listLines.Add("Skipped: no level input.");
			}
			else if(_learnAnalyzer.LevelResult!=null) {
				listLines.AddRange(_learnAnalyzer.LevelResult.Notes);
			}
			int countFromBaseline=GetLevelCandidates().Count(x => x.Source==LevelCandidateSource.BaselineB);
			if(countFromBaseline>0) {
				listLines.Add("Held and resting differ in "+countFromBaseline.ToString(CultureInfo.InvariantCulture)
					+(countFromBaseline==1 ? " bit, which is" : " bits, which are")+" offered below even without a recording.");
			}
			bool isTurnOff=GetSettingTurnOffForConfig()!=null;
			listLines.Add("Level input: "+(_levelCandidateChosen==null ? "none"
				: _levelCandidateChosen.Binding+", active while resting (the preview pauses"+(isTurnOff ? ", "+SettingRestName+" goes off" : "")+")")+".");
			return string.Join("\r\n",listLines);
		}

		///<summary>What a configuration's triggers and levels are, for the test page.</summary>
		public static string GetConfigSummary(CameraConfig cameraConfig) {
			if(cameraConfig==null) {
				return "";
			}
			List<string> listLines=new List<string>();
			listLines.Add("Still-pin trigger: "+(cameraConfig.StillEnabled ? "on" : "off")+".");
			ControlInputConfig controlPoll=cameraConfig.ControlPoll??new ControlInputConfig();
			if(controlPoll.IsConfigured) {
				listLines.Add("Control trigger: "+controlPoll.GetBinding()+", read every "+cameraConfig.EffectivePollMs.ToString(CultureInfo.InvariantCulture)+" ms.");
			}
			else if(controlPoll.GetProblem()!="") {
				listLines.Add("Control trigger: "+controlPoll.GetProblem());
			}
			else {
				listLines.Add("Control trigger: none.");
			}
			listLines.Add("Lockout after a press: "+cameraConfig.EffectiveDebounceMs.ToString(CultureInfo.InvariantCulture)+" ms"
				+(cameraConfig.DebounceMs==null ? " (default)." : "."));
			List<LevelInputConfig> listLevels=(cameraConfig.Levels??new List<LevelInputConfig>()).Where(x => x!=null && x.Enabled).OrderBy(x => x.Number).ToList();
			if(listLevels.Count==0) {
				listLines.Add("Level input: none.");
			}
			foreach(LevelInputConfig levelInputConfig in listLevels) {
				string problem=levelInputConfig.GetProblem();
				//A button says what each press does, a level what it does while active.
				bool isButton=LevelActions.IsButtonAction(levelInputConfig.Action);
				string does=LevelActions.GetText(levelInputConfig);
				if(isButton) {
					does+=" on each press";
				}
				else if(!does.EndsWith("while active")) {
					does+=" while active";
				}
				does+=(levelInputConfig.Highest!="" ? " (at most "+levelInputConfig.Highest+")" : "")+".";
				string name=isButton ? "Button" : "Level";
				listLines.Add(name+" "+levelInputConfig.Number.ToString(CultureInfo.InvariantCulture)+": "
					+(problem=="" ? levelInputConfig.GetBinding()+", "+does : problem));
			}
			if(!cameraConfig.StillEnabled && !controlPoll.IsConfigured) {
				listLines.Add("No camera trigger is set up: photos are taken with the Capture button or Space only.");
			}
			return string.Join("\r\n",listLines);
		}

		private static string JoinControls(IEnumerable<ControlId> controls) {
			return string.Join("; ",controls.OrderBy(x => x.SortKey).Select(x => x.ToString()));
		}

		///<summary>"3.0" for 3000.</summary>
		private static string FormatSeconds(long ms) {
			return (ms/1000.0).ToStringNetFx("0.0",CultureInfo.InvariantCulture);
		}
		#endregion Texts
	}
}
