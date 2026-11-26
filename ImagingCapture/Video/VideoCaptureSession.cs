// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using ImagingCapture.Camera;
using ImagingCapture.Uvc;
using ImagingDeviceManager.Config;

namespace ImagingCapture.Video {
	///<summary>A photo the Video window may hand to its BitmapCaptured handler, or the reason there is none.</summary>
	public sealed class CaptureStart {
		private bool _isEnded;

		///<summary>None when Bitmap is set.</summary>
		public CaptureDropReason DropReason { get; }
		///<summary>A new bitmap that shares no memory with the camera or the preview: the clone the handler receives (design 8.10).
		///VideoCaptureSession.EndCapture disposes it after the handler returns (disposing a Bitmap twice is harmless, and the
		///handler may dispose it too). Null when the request was dropped.</summary>
		public Bitmap Bitmap { get; }
		///<summary>True when the pixels are the still-pin sample ([Camera] PhotoSource=Still), false for a preview frame.</summary>
		public bool IsFromStill { get; }
		///<summary>Which request this was.</summary>
		public CaptureRequestSource Source { get; }

		internal CaptureStart(CaptureRequestSource captureRequestSource,CaptureDropReason captureDropReason) {
			Source=captureRequestSource;
			DropReason=captureDropReason;
			_isEnded=true;
		}

		internal CaptureStart(CaptureRequestSource captureRequestSource,Bitmap bitmap,bool isFromStill) {
			Source=captureRequestSource;
			DropReason=CaptureDropReason.None;
			Bitmap=bitmap;
			IsFromStill=isFromStill;
		}

		public bool IsAccepted {
			get {
				return DropReason==CaptureDropReason.None;
			}
		}

		///<summary>True the first time it is called for an accepted start, so EndCapture releases the gate once.</summary>
		internal bool TryEnd() {
			if(_isEnded) {
				return false;
			}
			_isEnded=true;
			return true;
		}
	}

	///<summary>The camera side of the Video window (FormVideo, design 8.1-8.10), apart from the window so it can be tested with a
	///fake camera: the device list and the device choice, opening and starting the camera with the chosen stream format, the
	///trigger engine, camera errors and what the window shows for them, the photo hand-off rules (CaptureGate), the patient the
	///window is bound to (PatientBinding), and the camera keys of capture.ini.
	///
	///Device choice (design 8.2): the camera saved in capture.ini ([Camera] Moniker, then Name), otherwise the first device. The
	///user can pick any device, which is then saved. No name is matched against any list.
	///
	///Still pin: the camera is started with its still pin connected when [Trigger.Still] is enabled or [Camera] PhotoSource is
	///Still, and, for Learn, whenever the camera has one (PrepareForLearn), so a still-pin key can be learned. The preview never
	///depends on it: a camera that does not start with its still pin connected is started again without it (OpenCore).
	///
	///Photo source (design 8.4): a still-pin press uses the still sample when PhotoSource=Still and the still pin is connected.
	///Every other press, and every press with PhotoSource=Preview, uses the newest preview frame.
	///
	///Threading: create, call and dispose it on the UI thread, which is where the trigger engine posts its events. FrameReady is
	///raised on the camera's streaming thread and CameraError on a thread-pool thread: the window marshals both to the UI thread
	///(BeginInvoke) and then calls TakePreviewFrame or HandleCameraError.</summary>
	public sealed class VideoCaptureSession:IDisposable {
		#region Fields
		private readonly IUvcBackend _uvcBackend;
		private readonly LocalCaptureConfig _localCaptureConfig;
		private CameraConfig _cameraConfig;
		private List<UvcDeviceInfo> _listDevices=new List<UvcDeviceInfo>();
		///<summary>The chosen device, kept after the camera is closed or lost so Reconnect can open it again. Null when none.</summary>
		private UvcDeviceInfo _device;
		///<summary>Read by the streaming thread's FrameReady handler.</summary>
		private volatile IFrameSource _frameSource;
		private TriggerEngine _triggerEngine;
		private ModeChoice _modeChoice;
		///<summary>The preview frame taken last. Valid until the next TakeLatestFrame, which only this class calls, on the UI thread.</summary>
		private UvcFrame _frameLast;
		private VideoStatus _videoStatus=VideoStatusText.NoCamera();
		///<summary>True when the open camera did not start with its still pin connected and was started again without it (OpenCore).
		///Then a missing still pin is no reason to reopen it (NeedsReopen, PrepareForLearn); a reopen for another reason, or
		///Refresh, tries the still pin again.</summary>
		private bool _isStillPinStartFailed;
		private bool _isDisposed;
		private long _countPhotos;
		#endregion Fields

		#region Constructor
		///<summary>uvcBackend: the camera API. localCaptureConfig: the loaded capture.ini, read now and written by the settings
		///(it may come from a missing or corrupt file, which gives the defaults: first camera, no trigger). patNum: the main
		///window's patient when the window opened. Nothing is opened until Refresh.</summary>
		public VideoCaptureSession(IUvcBackend uvcBackend,LocalCaptureConfig localCaptureConfig,long patNum) {
			_uvcBackend=uvcBackend??throw new ArgumentNullException(nameof(uvcBackend));
			_localCaptureConfig=localCaptureConfig??throw new ArgumentNullException(nameof(localCaptureConfig));
			_cameraConfig=CameraConfig.Read(_localCaptureConfig);
			Gate=new CaptureGate();
			PatientBinding=new PatientBinding(patNum);
		}
		#endregion Constructor

		#region Events
		///<summary>A preview frame is ready (streaming thread). Marshal to the UI thread, then call TakePreviewFrame.</summary>
		public event EventHandler FrameReady;
		///<summary>The camera reported an error (thread-pool thread). The sender is the frame source: marshal to the UI thread and
		///pass both to HandleCameraError.</summary>
		public event EventHandler<UvcErrorEventArgs> CameraError;
		///<summary>A camera trigger fired while the window was enabled: take a photo (UI thread).</summary>
		public event EventHandler<TriggerPressEventArgs> TriggerPressed;
		///<summary>A camera trigger fired while the window was disabled (UI thread). No photo.</summary>
		public event EventHandler<TriggerPressEventArgs> TriggerPressIgnored;
		///<summary>A level input changed (UI thread). IsPausePreviewActive says whether to pause the preview.</summary>
		public event EventHandler<LevelChangedEventArgs> LevelChanged;
		#endregion Events

		#region Properties
		public CaptureGate Gate { get; }
		public PatientBinding PatientBinding { get; }

		///<summary>The camera keys of capture.ini as last read or saved.</summary>
		public CameraConfig CameraConfig {
			get {
				return _cameraConfig;
			}
		}

		public LocalCaptureConfig LocalCaptureConfig {
			get {
				return _localCaptureConfig;
			}
		}

		///<summary>The devices found by the last Refresh.</summary>
		public IReadOnlyList<UvcDeviceInfo> Devices {
			get {
				return _listDevices;
			}
		}

		///<summary>The chosen device, or null.</summary>
		public UvcDeviceInfo Device {
			get {
				return _device;
			}
		}

		///<summary>The open camera, or null.</summary>
		public IFrameSource FrameSource {
			get {
				return _frameSource;
			}
		}

		///<summary>The running trigger engine, or null (no camera, or stopped for Learn).</summary>
		public TriggerEngine TriggerEngine {
			get {
				return _triggerEngine;
			}
		}

		///<summary>How the stream format was chosen for the open camera, or null.</summary>
		public ModeChoice ModeChoice {
			get {
				return _modeChoice;
			}
		}

		///<summary>What the window shows about the camera. Streaming while the preview runs.</summary>
		public VideoStatus Status {
			get {
				return _videoStatus;
			}
		}

		public bool IsStreaming {
			get {
				IFrameSource frameSource=_frameSource;
				return frameSource!=null && frameSource.IsRunning;
			}
		}

		///<summary>Photos handed to the handler so far.</summary>
		public long CountPhotos {
			get {
				return _countPhotos;
			}
		}

		///<summary>Whether the window is enabled now (design 8.4: presses are ignored while it is disabled). Null means enabled.
		///Called on the UI thread; an exception counts as disabled.</summary>
		public Func<bool> FuncIsWindowEnabled { get; set; }

		///<summary>Receives one line for the log for each camera problem, ignored or dropped request and settings failure. Null: no log.</summary>
		public Action<string> ActionLog { get; set; }
		#endregion Properties

		#region Devices and opening
		///<summary>Closes the camera, lists the devices again and opens one: the chosen device when it is still present, else the
		///saved one, else the first (design 8.2). Used for the first start, Refresh and Reconnect.</summary>
		public VideoStatus Refresh() {
			ThrowIfDisposed();
			CloseCamera();
			List<UvcDeviceInfo> listDevices;
			try {
				listDevices=(_uvcBackend.GetDevices()??new List<UvcDeviceInfo>()).Where(x => x!=null).ToList();
			}
			catch(Exception ex) {
				_listDevices=new List<UvcDeviceInfo>();
				UvcException uvcException=ex as UvcException;
				_videoStatus=VideoStatusText.ForListFailure(uvcException?.Kind??UvcErrorKind.Failed,ex.HResult);
				Log("Listing the cameras failed: "+ex.Message);
				return _videoStatus;
			}
			_listDevices=listDevices;
			UvcDeviceInfo device=ChooseDevice(listDevices,_device,_cameraConfig);
			if(device==null) {
				_device=null;
				_videoStatus=VideoStatusText.NoCamera();
				return _videoStatus;
			}
			return OpenCore(device,false,true);
		}

		///<summary>Opens the device the user picked and, when that works, saves it as [Camera] Moniker and Name. error and warning
		///are SaveSettings's, both "" when nothing was saved. Does nothing when it is the device already streaming.</summary>
		public VideoStatus SelectDevice(UvcDeviceInfo device,out string error,out string warning) {
			ThrowIfDisposed();
			error="";
			warning="";
			if(device==null) {
				return _videoStatus;
			}
			if(_device!=null && device.IsSameDevice(_device) && IsStreaming) {
				return _videoStatus;
			}
			VideoStatus videoStatus=OpenCore(device,false,true);
			if(videoStatus.IsStreaming) {
				SaveSettings(null,out error,out warning);
			}
			return _videoStatus;
		}

		///<summary>The device to open from devices: current when it is still present, else the saved camera (CameraConfig.FindSavedDevice),
		///else the first device. Null when devices is empty.</summary>
		public static UvcDeviceInfo ChooseDevice(IEnumerable<UvcDeviceInfo> devices,UvcDeviceInfo current,CameraConfig cameraConfig) {
			List<UvcDeviceInfo> listDevices=(devices??Enumerable.Empty<UvcDeviceInfo>()).Where(x => x!=null).ToList();
			if(current!=null) {
				UvcDeviceInfo deviceCurrent=listDevices.FirstOrDefault(x => x.IsSameDevice(current));
				if(deviceCurrent!=null) {
					return deviceCurrent;
				}
			}
			UvcDeviceInfo deviceSaved=(cameraConfig??new CameraConfig()).FindSavedDevice(listDevices);
			return deviceSaved??listDevices.FirstOrDefault();
		}

		///<summary>The names to list, in the same order. Two devices with the same name (two identical cameras) get " (2)", " (3)"
		///and so on after the first, so the user can tell the entries apart. An empty name shows as "Camera".</summary>
		public static List<string> GetDeviceNames(IEnumerable<UvcDeviceInfo> devices) {
			List<string> listNames=new List<string>();
			Dictionary<string,int> dictCounts=new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
			foreach(UvcDeviceInfo device in devices??Enumerable.Empty<UvcDeviceInfo>()) {
				string name=CameraConfig.CutAtNul(device?.FriendlyName).Trim();
				if(name=="") {
					name="Camera";
				}
				int count;
				dictCounts.TryGetValue(name,out count);
				count++;
				dictCounts[name]=count;
				listNames.Add(count==1 ? name : name+" ("+count.ToString(CultureInfo.InvariantCulture)+")");
			}
			return listNames;
		}

		///<summary>Whether the camera is started with its still pin connected outside Learn: when the still trigger is enabled or the
		///photo source is the still pin.</summary>
		public static bool GetConnectStillPin(CameraConfig cameraConfig) {
			if(cameraConfig==null) {
				return false;
			}
			return cameraConfig.StillEnabled || cameraConfig.PhotoSource==CameraPhotoSource.Still;
		}

		///<summary>Opens and starts device. On failure nothing stays open and the status says why. The preview never depends on the
		///still pin (design 8.3): the backend leaves the still pin out when it cannot be connected, and when a camera started with
		///its still pin connected fails to start anyway (the graph does not run, for example), it is opened again and started with
		///the preview only.</summary>
		private VideoStatus OpenCore(UvcDeviceInfo device,bool isForLearn,bool startTriggers) {
			CloseCamera();
			_device=device;
			_isStillPinStartFailed=false;
			bool isStillPinRequested;
			Exception exception=TryOpenAndStart(device,isForLearn,true,out isStillPinRequested);
			if(exception!=null && isStillPinRequested) {
				Log("The camera did not start with its still pin connected ("+UvcErrors.FormatHResult(exception.HResult)+"): "+exception.Message
					+" Starting it again without the still pin.");
				exception=TryOpenAndStart(device,isForLearn,false,out isStillPinRequested);
				if(exception==null) {
					_isStillPinStartFailed=true;
					Log("The camera runs without its still pin. The preview works without it.");
				}
			}
			if(exception!=null) {
				UvcException uvcException=exception as UvcException;
				UvcErrorKind uvcErrorKind=uvcException?.Kind??UvcErrors.Classify(exception.HResult);
				if(uvcErrorKind==UvcErrorKind.None) {
					uvcErrorKind=UvcErrorKind.Failed;
				}
				_videoStatus=VideoStatusText.ForOpenFailure(uvcErrorKind,exception.HResult);
				Log("Opening the camera failed ("+uvcErrorKind+", "+UvcErrors.FormatHResult(exception.HResult)+"): "+exception.Message);
				return _videoStatus;
			}
			_frameLast=null;
			_videoStatus=VideoStatusText.Streaming();
			if(startTriggers) {
				StartTriggers();
			}
			return _videoStatus;
		}

		///<summary>Opens device and starts it with the chosen stream format, and with its still pin when the camera has one, isFirstTry
		///is true, and Learn or the settings want it (isStillPinRequested says whether Start was asked to connect it). Returns null
		///when the camera streams; otherwise nothing stays open and the exception is returned.</summary>
		private Exception TryOpenAndStart(UvcDeviceInfo device,bool isForLearn,bool isFirstTry,out bool isStillPinRequested) {
			isStillPinRequested=false;
			IFrameSource frameSource=null;
			try {
				frameSource=_uvcBackend.Open(device);
				ModeChoice modeChoice=ModeSelector.Select(frameSource.Modes,_cameraConfig.Width,_cameraConfig.Height,_cameraConfig.Subtype);
				if(isFirstTry && modeChoice.HasOverride && !modeChoice.IsOverrideMatched) {
					Log("The saved resolution (Width "+_cameraConfig.Width+", Height "+_cameraConfig.Height+", Subtype "+_cameraConfig.Subtype
						+") is not offered by this camera. Using "+(modeChoice.Mode?.ToString()??"its current format")+".");
				}
				isStillPinRequested=isFirstTry && frameSource.HasStillPin && (isForLearn || GetConnectStillPin(_cameraConfig));
				frameSource.FrameReady+=frameSource_FrameReady;
				frameSource.Error+=frameSource_Error;
				_frameSource=frameSource;
				_modeChoice=modeChoice;
				frameSource.Start(modeChoice.Mode,isStillPinRequested);
				if(isStillPinRequested && !frameSource.IsStillPinConnected) {
					Log("The camera's still pin could not be connected. The preview works without it.");
				}
				return null;
			}
			catch(Exception ex) {
				_frameSource=null;
				_modeChoice=null;
				if(frameSource!=null) {
					frameSource.FrameReady-=frameSource_FrameReady;
					frameSource.Error-=frameSource_Error;
					DisposeQuietly(frameSource);
				}
				return ex;
			}
		}

		///<summary>Stops the triggers and the graph and releases the camera (its COM objects are released on Dispose of the frame
		///source). The chosen device is kept. Safe to call when nothing is open.</summary>
		public void CloseCamera() {
			StopTriggers();
			IFrameSource frameSource=_frameSource;
			_frameSource=null;
			_modeChoice=null;
			_frameLast=null;
			if(frameSource==null) {
				return;
			}
			frameSource.FrameReady-=frameSource_FrameReady;
			frameSource.Error-=frameSource_Error;
			DisposeQuietly(frameSource);
		}

		private void DisposeQuietly(IFrameSource frameSource) {
			try {
				frameSource.Dispose();
			}
			catch(Exception ex) {
				//Releasing a camera must not end the window; the next open reports whatever is still wrong.
				Log("Closing the camera failed: "+ex.Message);
			}
		}

		///<summary>For a camera error raised by sender (on any thread, marshaled here): when sender is the open camera, closes it and
		///returns the status to show (Camera disconnected and Reconnect, or the error). Null when the error came from a camera that is
		///no longer open.</summary>
		public VideoStatus HandleCameraError(object sender,UvcErrorEventArgs uvcErrorEventArgs) {
			if(_isDisposed || sender==null || sender!=_frameSource || uvcErrorEventArgs==null) {
				return null;
			}
			Log("Camera error ("+uvcErrorEventArgs.Kind+", "+UvcErrors.FormatHResult(uvcErrorEventArgs.HResult)+"): "+uvcErrorEventArgs.Message);
			CloseCamera();
			_videoStatus=VideoStatusText.ForStreamError(uvcErrorEventArgs.Kind,uvcErrorEventArgs.HResult);
			return _videoStatus;
		}

		private void frameSource_FrameReady(object sender,EventArgs e) {
			if(sender!=_frameSource) {
				return;
			}
			FrameReady?.Invoke(this,EventArgs.Empty);
		}

		private void frameSource_Error(object sender,UvcErrorEventArgs e) {
			CameraError?.Invoke(sender,e);
		}
		#endregion Devices and opening

		#region Preview and photos
		///<summary>The newest preview frame not yet taken, or null (UI thread only). It stays valid until the next call, so the window
		///converts it at once (UvcFrame.ToBitmap) and disposes the bitmap it replaces.</summary>
		public UvcFrame TakePreviewFrame() {
			IFrameSource frameSource=_frameSource;
			if(frameSource==null) {
				return null;
			}
			UvcFrame frame=frameSource.TakeLatestFrame();
			if(frame!=null) {
				_frameLast=frame;
			}
			return frame;
		}

		///<summary>Whether a press uses its still sample: PhotoSource=Still, the still pin is connected and the press carries one.</summary>
		public static bool ShouldUseStillFrame(CameraPhotoSource cameraPhotoSource,bool isStillPinConnected,UvcFrame frameStill) {
			return cameraPhotoSource==CameraPhotoSource.Still && isStillPinConnected && frameStill!=null;
		}

		///<summary>Decides a photo request (CaptureGate) and, when it is accepted, copies the photo into a new bitmap: the still sample
		///(see ShouldUseStillFrame) or the newest preview frame. The window raises BitmapCaptured with CaptureStart.Bitmap and then
		///calls EndCapture, also when the handler throws. Until EndCapture every request is Busy.</summary>
		public CaptureStart BeginCapture(CaptureRequestSource captureRequestSource,UvcFrame frameStill) {
			bool isWindowEnabled=GetIsWindowEnabled();
			CaptureDropReason captureDropReason=Gate.TryBegin(captureRequestSource,isWindowEnabled,_cameraConfig.MainWindowSpace,PatientBinding.PatNum!=0,
				!_isDisposed && IsStreaming);
			if(captureDropReason!=CaptureDropReason.None) {
				LogDrop(captureRequestSource,captureDropReason);
				return new CaptureStart(captureRequestSource,captureDropReason);
			}
			IFrameSource frameSource=_frameSource;
			bool isFromStill=ShouldUseStillFrame(_cameraConfig.PhotoSource,frameSource!=null && frameSource.IsStillPinConnected,frameStill);
			UvcFrame frame;
			if(isFromStill) {
				frame=frameStill;
			}
			else {
				TakePreviewFrame();
				frame=_frameLast;
			}
			if(frame==null) {
				Gate.End();
				LogDrop(captureRequestSource,CaptureDropReason.NoPicture);
				return new CaptureStart(captureRequestSource,CaptureDropReason.NoPicture);
			}
			Bitmap bitmap;
			try {
				bitmap=frame.ToBitmap();
			}
			catch(Exception ex) {
				Gate.End();
				Log("Copying the photo failed: "+ex.Message);
				return new CaptureStart(captureRequestSource,CaptureDropReason.Failed);
			}
			_countPhotos++;
			return new CaptureStart(captureRequestSource,bitmap,isFromStill);
		}

		///<summary>BeginCapture for a picture the window froze (IMG-P4b-2, Freeze): the same gate, without needing the stream (it may have stopped
		///since), and the photo is a copy of bitmapFrozen. EndCapture releases it as usual.</summary>
		public CaptureStart BeginCaptureFrozen(CaptureRequestSource captureRequestSource,Bitmap bitmapFrozen) {
			bool isWindowEnabled=GetIsWindowEnabled();
			CaptureDropReason captureDropReason=Gate.TryBegin(captureRequestSource,isWindowEnabled,_cameraConfig.MainWindowSpace,PatientBinding.PatNum!=0,
				!_isDisposed);
			if(captureDropReason!=CaptureDropReason.None) {
				LogDrop(captureRequestSource,captureDropReason);
				return new CaptureStart(captureRequestSource,captureDropReason);
			}
			if(bitmapFrozen==null) {
				Gate.End();
				LogDrop(captureRequestSource,CaptureDropReason.NoPicture);
				return new CaptureStart(captureRequestSource,CaptureDropReason.NoPicture);
			}
			Bitmap bitmap;
			try {
				bitmap=new Bitmap(bitmapFrozen);
			}
			catch(Exception ex) {
				Gate.End();
				Log("Copying the frozen photo failed: "+ex.Message);
				return new CaptureStart(captureRequestSource,CaptureDropReason.Failed);
			}
			_countPhotos++;
			return new CaptureStart(captureRequestSource,bitmap,false);
		}

		///<summary>After the handler returned: disposes the photo's bitmap and releases the gate. Safe to call more than once, for a
		///dropped request, and after Dispose.</summary>
		public void EndCapture(CaptureStart captureStart) {
			if(captureStart==null) {
				return;
			}
			try {
				captureStart.Bitmap?.Dispose();
			}
			catch(Exception ex) {
				Log("Releasing the photo failed: "+ex.Message);
			}
			if(captureStart.TryEnd()) {
				Gate.End();
			}
		}

		private bool GetIsWindowEnabled() {
			Func<bool> funcIsWindowEnabled=FuncIsWindowEnabled;
			if(funcIsWindowEnabled==null) {
				return true;
			}
			try {
				return funcIsWindowEnabled();
			}
			catch {
				return false;
			}
		}

		private void LogDrop(CaptureRequestSource captureRequestSource,CaptureDropReason captureDropReason) {
			if(CaptureGate.IsLogged(captureDropReason)) {
				Log("Photo request ("+captureRequestSource+") dropped: "+captureDropReason+".");
			}
		}
		#endregion Preview and photos

		#region Triggers
		///<summary>Starts a trigger engine on the open camera with the triggers and levels of capture.ini. Does nothing without a
		///camera or when one runs. Call on the UI thread (the engine posts its events there).</summary>
		public void StartTriggers() {
			IFrameSource frameSource=_frameSource;
			if(_isDisposed || frameSource==null || _triggerEngine!=null) {
				return;
			}
			LimitLight();
			TriggerEngine triggerEngine=new TriggerEngine(frameSource,_cameraConfig);
			triggerEngine.FuncIsConsumerEnabled=GetIsWindowEnabled;
			triggerEngine.Pressed+=triggerEngine_Pressed;
			triggerEngine.PressIgnored+=triggerEngine_PressIgnored;
			triggerEngine.LevelChanged+=triggerEngine_LevelChanged;
			triggerEngine.Warning+=triggerEngine_Warning;
			_triggerEngine=triggerEngine;
			triggerEngine.Start();
		}

		///<summary>Stops and drops the trigger engine; presses it saw but did not deliver are dropped. A paused preview should resume:
		///the window clears its pause when it calls this.</summary>
		public void StopTriggers() {
			TriggerEngine triggerEngine=_triggerEngine;
			_triggerEngine=null;
			if(triggerEngine==null) {
				return;
			}
			triggerEngine.Pressed-=triggerEngine_Pressed;
			triggerEngine.PressIgnored-=triggerEngine_PressIgnored;
			triggerEngine.LevelChanged-=triggerEngine_LevelChanged;
			triggerEngine.Warning-=triggerEngine_Warning;
			triggerEngine.Dispose();
		}

		///<summary>The trigger lines for the settings (TriggerNotes).</summary>
		public List<string> GetTriggerNotes() {
			return TriggerNotes.GetLines(_triggerEngine,_cameraConfig);
		}

		private void triggerEngine_Pressed(object sender,TriggerPressEventArgs e) {
			if(sender!=_triggerEngine) {
				return;
			}
			TriggerPressed?.Invoke(this,e);
		}

		private void triggerEngine_PressIgnored(object sender,TriggerPressEventArgs e) {
			if(sender!=_triggerEngine) {
				return;
			}
			Log("Camera button press ("+e.Source+") ignored while the window was disabled.");
			TriggerPressIgnored?.Invoke(this,e);
		}

		private void triggerEngine_LevelChanged(object sender,LevelChangedEventArgs e) {
			if(sender!=_triggerEngine) {
				return;
			}
			LevelInputConfig level=_cameraConfig?.Levels.FirstOrDefault(x => x.Number==e.Number);
			if(LevelActions.IsLightAction(e.Action) && e.IsActive) {
				if(level?.LightProperty!=null) {
					if(e.Action==LevelAction.ToggleLight) {
						ToggleLight(level);
					}
					else {
						StepLight(level,e.Action==LevelAction.LightUp);
					}
				}
			}
			else if(level!=null && level.LightOffWhileActive && !LevelActions.IsButtonAction(e.Action)) {
				LevelInputConfig levelLight=GetLightLevel();
				if(levelLight!=null) {
					SetLightForRest(levelLight,e.IsActive);
				}
			}
			LevelChanged?.Invoke(this,e);
		}

		///<summary>FB-1: the value the light property had before the light was last turned off, by property; turning it on restores it.</summary>
		private readonly Dictionary<int,int> _dictLightOnValues=new Dictionary<int,int>();

		///<summary>FB-1: the number of steps between the light's lowest step and its highest value (LightUp, LightDown).</summary>
		public const int LightStepCount=5;

		///<summary>FB-1: the configured light button: the first enabled ToggleLight level, else the first LightUp or LightDown level, with
		///a LightProperty; null when none.</summary>
		private LevelInputConfig GetLightLevel() {
			List<LevelInputConfig> listLevels=(_cameraConfig?.Levels??new List<LevelInputConfig>())
				.Where(x => x!=null && x.IsLevelConfigured && LevelActions.IsLightAction(x.Action)).ToList();
			return listLevels.FirstOrDefault(x => x.Action==LevelAction.ToggleLight)??listLevels.FirstOrDefault();
		}

		///<summary>FB-1: when the camera starts its triggers with a light button whose LightMax is set and the light property is above it,
		///the property is set to LightMax, so the light starts no brighter than capture.ini allows.</summary>
		private void LimitLight() {
			LevelInputConfig levelLight=GetLightLevel();
			if(levelLight?.LightMax==null) {
				return;
			}
			int property=levelLight.LightProperty.Value;
			UvcControlValue controlValue;
			UvcControlRange controlRange;
			if(!TryReadLight(property,out controlValue,out controlRange)) {
				return;
			}
			int valueMax=GetLightOnValueMax(levelLight,controlRange);
			if(controlValue.Value>valueMax) {
				WriteLight(property,controlValue.Value,valueMax,"limited to LightMax");
			}
		}

		///<summary>FB-1: the camera's M button (a [Level.N] with Action=ToggleLight). Above its minimum the light goes off (the minimum);
		///at its minimum it comes back on, at the value it had before, else its default, else its maximum, and never above LightMax. A camera
		///that does not support the property is logged and left alone.</summary>
		private void ToggleLight(LevelInputConfig levelLight) {
			int property=levelLight.LightProperty.Value;
			UvcControlValue controlValue;
			UvcControlRange controlRange;
			if(!TryReadLight(property,out controlValue,out controlRange)) {
				return;
			}
			if(controlValue.Value>controlRange.Min) {
				TurnLightOff(property,controlValue.Value,controlRange,"button");
			}
			else {
				TurnLightOn(levelLight,controlValue.Value,controlRange,"button");
			}
		}

		///<summary>FB-1: the brighter and dimmer buttons (LightUp, LightDown). The light moves one step, a fifth (LightStepCount) of the way
		///from its minimum to its highest value (LightMax, else the camera's maximum), at least 1. Brighter turns an off light on at the
		///lowest step and stops at the highest value; dimmer stops at the lowest step and does nothing while the light is off. Turning
		///the light off and on again keeps the brightness chosen.</summary>
		private void StepLight(LevelInputConfig levelLight,bool isBrighter) {
			int property=levelLight.LightProperty.Value;
			UvcControlValue controlValue;
			UvcControlRange controlRange;
			if(!TryReadLight(property,out controlValue,out controlRange)) {
				return;
			}
			int valueMax=GetLightOnValueMax(levelLight,controlRange);
			int step=Math.Max(1,(valueMax-controlRange.Min)/LightStepCount);
			int valueLowest=Math.Min(valueMax,controlRange.Min+step);
			int valueOld=controlValue.Value;
			int valueNew;
			if(valueMax<=controlRange.Min) {
				Log("Light: LightMax "+levelLight.LightMax+" is at or below the minimum of "+UvcControls.GetPropertyName(UvcControlInterface.VideoProcAmp,property)
					+" ("+controlRange.Min+"), so the light cannot be made brighter or dimmer.");
				return;
			}
			if(isBrighter) {
				valueNew=valueOld<=controlRange.Min ? valueLowest : Math.Min(valueMax,valueOld+step);
			}
			else {
				if(valueOld<=controlRange.Min) {
					return;
				}
				valueNew=Math.Min(valueMax,Math.Max(valueLowest,valueOld-step));
			}
			if(valueNew==valueOld) {
				return;
			}
			_dictLightOnValues[property]=valueNew;
			WriteLight(property,valueOld,valueNew,isBrighter ? "brighter (button)" : "dimmer (button)");
		}

		///<summary>FB-1: a level with LightOffWhileActive (the handpiece resting): while it is active the light is off, and when it ends the
		///light comes back on, whatever the light button did in between.</summary>
		private void SetLightForRest(LevelInputConfig levelLight,bool isResting) {
			int property=levelLight.LightProperty.Value;
			UvcControlValue controlValue;
			UvcControlRange controlRange;
			if(!TryReadLight(property,out controlValue,out controlRange)) {
				return;
			}
			if(isResting) {
				if(controlValue.Value>controlRange.Min) {
					TurnLightOff(property,controlValue.Value,controlRange,"resting");
				}
			}
			else if(controlValue.Value<=controlRange.Min) {
				TurnLightOn(levelLight,controlValue.Value,controlRange,"picked up");
			}
		}

		private void TurnLightOff(int property,int valueOld,UvcControlRange controlRange,string reason) {
			_dictLightOnValues[property]=valueOld;
			WriteLight(property,valueOld,controlRange.Min,"off ("+reason+")");
		}

		private void TurnLightOn(LevelInputConfig levelLight,int valueOld,UvcControlRange controlRange,string reason) {
			int property=levelLight.LightProperty.Value;
			int valueNew;
			if(!_dictLightOnValues.TryGetValue(property,out valueNew) || valueNew<=controlRange.Min) {
				valueNew=controlRange.Default>controlRange.Min ? controlRange.Default : controlRange.Max;
			}
			valueNew=Math.Min(valueNew,GetLightOnValueMax(levelLight,controlRange));
			if(valueNew<=controlRange.Min) {
				Log("Light: LightMax "+levelLight.LightMax+" is at or below the minimum of "+UvcControls.GetPropertyName(UvcControlInterface.VideoProcAmp,property)
					+" ("+controlRange.Min+"), so the light cannot be turned on.");
				return;
			}
			WriteLight(property,valueOld,valueNew,"on ("+reason+")");
		}

		///<summary>The highest value the light may get: LightMax when set, within the camera's range.</summary>
		private static int GetLightOnValueMax(LevelInputConfig levelLight,UvcControlRange controlRange) {
			if(levelLight.LightMax==null) {
				return controlRange.Max;
			}
			return Math.Max(controlRange.Min,Math.Min(controlRange.Max,levelLight.LightMax.Value));
		}

		///<summary>Reads the light property and its range; false, logged, when the camera cannot (the light is then left alone).</summary>
		private bool TryReadLight(int property,out UvcControlValue controlValue,out UvcControlRange controlRange) {
			controlValue=default(UvcControlValue);
			controlRange=default(UvcControlRange);
			IFrameSource frameSource=_frameSource;
			if(frameSource==null || _isDisposed) {
				return false;
			}
			controlValue=frameSource.ReadControl(UvcControlInterface.VideoProcAmp,property);
			controlRange=frameSource.ReadControlRange(UvcControlInterface.VideoProcAmp,property);
			if(!controlValue.IsOk || !controlRange.IsOk) {
				Log("Light: the camera's "+UvcControls.GetPropertyName(UvcControlInterface.VideoProcAmp,property)+" cannot be read ("
					+(controlValue.IsOk ? controlRange.Status : controlValue.Status)+"); the light is left as it is.");
				return false;
			}
			return true;
		}

		///<summary>FB-1: the only camera write of the session (design 8.7): the light property, for the light buttons (on/off, brighter,
		///dimmer), the rest level and LightMax. Logged with the old and new value.</summary>
		private void WriteLight(int property,int valueOld,int valueNew,string what) {
			IFrameSource frameSource=_frameSource;
			if(frameSource==null || _isDisposed) {
				return;
			}
			UvcControlValue controlValueWritten=frameSource.WriteLightControl(property,valueNew);
			Log("Light "+what+": "+UvcControls.GetPropertyName(UvcControlInterface.VideoProcAmp,property)+" "+valueOld+" -> "+valueNew
				+(controlValueWritten.IsOk ? "." : " failed ("+controlValueWritten.Status+", "+UvcErrors.FormatHResult(controlValueWritten.HResult)+")."));
		}

		private void triggerEngine_Warning(object sender,TriggerWarningEventArgs e) {
			Log(e.Message);
		}
		#endregion Triggers

		#region Settings
		///<summary>Changes the camera keys of capture.ini and saves the file: reads the current values, records the open device as
		///[Camera] Moniker and Name, applies change (null: only the device) and writes what changed. On success, and also on failure,
		///CameraConfig is read again from the loaded settings. Does not reopen the camera: call ApplySettings after.
		///Returns false, with error, when nothing was saved. On success error is "" and warning is "" or the warning
		///LocalCaptureConfig.Save left, which the window shows: for example that the values went to the per-user file and do not
		///take effect the next time the window opens while the ProgramData file exists.</summary>
		public bool SaveSettings(Action<CameraConfig> change,out string error,out string warning) {
			warning="";
			CameraConfig cameraConfig=CameraConfig.Read(_localCaptureConfig);
			if(_device!=null) {
				cameraConfig.SetDevice(_device);
			}
			change?.Invoke(cameraConfig);
			if(!cameraConfig.Write(_localCaptureConfig,out error)) {
				Log("Saving the camera settings failed: "+error);
				_cameraConfig=CameraConfig.Read(_localCaptureConfig);
				return false;
			}
			bool isSaved=_localCaptureConfig.Save();
			string lastError=_localCaptureConfig.LastError;
			_cameraConfig=CameraConfig.Read(_localCaptureConfig);
			if(!isSaved) {
				error=lastError;
				Log("Saving capture.ini failed: "+error);
				return false;
			}
			error="";
			warning=lastError;
			if(warning!="") {
				Log("capture.ini was saved with a warning: "+warning);
			}
			return true;
		}

		///<summary>True when the open camera does not match the settings: another stream format is chosen now, or the still pin is
		///wanted but not connected (unless the camera did not start with it, see OpenCore). Also true when a device is chosen but
		///not open.</summary>
		public bool NeedsReopen() {
			IFrameSource frameSource=_frameSource;
			if(frameSource==null) {
				return _device!=null;
			}
			ModeChoice modeChoice=ModeSelector.Select(frameSource.Modes,_cameraConfig.Width,_cameraConfig.Height,_cameraConfig.Subtype);
			UvcMode modeStarted=frameSource.ModeStarted;
			if(modeChoice.Mode==null) {
				if(modeStarted!=null) {
					return true;
				}
			}
			else if(modeStarted==null || !modeChoice.Mode.IsSameFormat(modeStarted)) {
				return true;
			}
			return frameSource.HasStillPin && GetConnectStillPin(_cameraConfig) && !frameSource.IsStillPinConnected && !_isStillPinStartFailed;
		}

		///<summary>After the settings changed: reopens the camera when NeedsReopen (the format and still choice are fixed once a
		///camera is started), otherwise restarts the triggers with the new values.</summary>
		public VideoStatus ApplySettings() {
			ThrowIfDisposed();
			if(_device!=null && NeedsReopen()) {
				return OpenCore(_device,false,true);
			}
			StopTriggers();
			StartTriggers();
			return _videoStatus;
		}

		///<summary>Before the Learn window (design 8.5): stops the triggers, so Learn's key presses take no photo, and reopens the
		///camera with its still pin connected when it has one that is not, so a still-pin key can be learned (not when the camera
		///did not start with it: see OpenCore). Returns the status: Learn can open only while it is Streaming.</summary>
		public VideoStatus PrepareForLearn() {
			ThrowIfDisposed();
			StopTriggers();
			IFrameSource frameSource=_frameSource;
			if(frameSource!=null && frameSource.HasStillPin && !frameSource.IsStillPinConnected && !_isStillPinStartFailed && _device!=null) {
				return OpenCore(_device,true,false);
			}
			return _videoStatus;
		}

		///<summary>After the Learn window closed: when it saved (isSaved), reads capture.ini again and records the device the values
		///were learned on; then restarts the triggers, or reopens the camera when the settings need it.</summary>
		public VideoStatus FinishLearn(bool isSaved) {
			ThrowIfDisposed();
			if(isSaved) {
				_cameraConfig=CameraConfig.Read(_localCaptureConfig);
				//Learn has shown how its own save went; this one only records the device, usually with nothing to write.
				string error;
				string warning;
				SaveSettings(null,out error,out warning);
			}
			if(_frameSource==null) {
				return _videoStatus;
			}
			return ApplySettings();
		}
		#endregion Settings

		private void Log(string line) {
			try {
				ActionLog?.Invoke(line);
			}
			catch {
				//A log failure must never stop the camera.
			}
		}

		private void ThrowIfDisposed() {
			if(_isDisposed) {
				throw new ObjectDisposedException(nameof(VideoCaptureSession));
			}
		}

		///<summary>Closes the gate (every later request is dropped as Closing), stops the triggers, stops the graph and releases the
		///camera. Safe to call more than once, and while a photo handler is running: EndCapture still disposes that photo.</summary>
		public void Dispose() {
			Gate.BeginClosing();
			if(_isDisposed) {
				return;
			}
			_isDisposed=true;
			CloseCamera();
		}
	}
}
