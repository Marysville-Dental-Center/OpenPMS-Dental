// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ImagingCapture.Camera;
using ImagingCapture.Uvc;
using ImagingCapture.Uvc.Interop;
using ImagingCapture.Video;
using ImagingDeviceManager.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImagingCapture.Tests {
	///<summary>VideoCaptureSession, the camera side of FormVideo (design 8.1-8.10), against FakeUvcBackend and a temporary
	///capture.ini. The devices and modes are made up.</summary>
	[TestClass]
	public class VideoCaptureSessionTests {
		private const long PAT_NUM=42;
		private string _folder;
		private string _path;
		private FakeUvcBackend _fakeUvcBackend;
		private List<string> _listLog;
		private bool _isWindowEnabled;
		private VideoCaptureSession _videoCaptureSession;

		[TestInitialize]
		public void Init() {
			_folder=Path.Combine(Path.GetTempPath(),"ImagingCaptureTests_"+Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(_folder);
			_path=Path.Combine(_folder,"capture.ini");
			_fakeUvcBackend=new FakeUvcBackend();
			_fakeUvcBackend.ListModes.Add(new UvcMode(0,640,480,Ds.MEDIASUBTYPE_MJPG,24,333333));
			_fakeUvcBackend.ListModes.Add(new UvcMode(1,1280,720,Ds.MEDIASUBTYPE_MJPG,24,333333));
			_fakeUvcBackend.ListModes.Add(new UvcMode(2,1280,720,Ds.MEDIASUBTYPE_YUY2,16,1000000));
			_listLog=new List<string>();
			_isWindowEnabled=true;
		}

		[TestCleanup]
		public void Cleanup() {
			_videoCaptureSession?.Dispose();
			try {
				foreach(string path in Directory.GetFiles(_folder,"*",SearchOption.AllDirectories)) {
					File.SetAttributes(path,FileAttributes.Normal);
				}
				Directory.Delete(_folder,true);
			}
			catch {
			}
		}

		private void WriteIni(string text) {
			File.WriteAllText(_path,text);
		}

		private VideoCaptureSession CreateSession(long patNum=PAT_NUM) {
			_videoCaptureSession=new VideoCaptureSession(_fakeUvcBackend,LocalCaptureConfig.Load(_path),patNum);
			_videoCaptureSession.ActionLog=x => _listLog.Add(x);
			_videoCaptureSession.FuncIsWindowEnabled=() => _isWindowEnabled;
			return _videoCaptureSession;
		}

		private FakeFrameSource OpenedLast {
			get {
				return _fakeUvcBackend.ListOpened.Last();
			}
		}

		private static void PushFrame(FakeFrameSource fakeFrameSource,int rgb) {
			fakeFrameSource.PushFrame(8,6,(x,y) => rgb);
		}

		#region Devices and opening
		[TestMethod]
		public void NoCamera_ShowsNoCameraAndRefresh() {
			VideoCaptureSession videoCaptureSession=CreateSession();
			VideoStatus videoStatus=videoCaptureSession.Refresh();
			Assert.AreEqual(VideoStatusKind.NoCamera,videoStatus.Kind);
			Assert.AreEqual(VideoStatusAction.Refresh,videoStatus.Action);
			Assert.IsNull(videoCaptureSession.Device);
			_fakeUvcBackend.AddDevice();
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming,"Refresh finds a camera plugged in later.");
		}

		[TestMethod]
		public void FirstDevice_WhenNothingSaved_DefaultMode() {
			_fakeUvcBackend.AddDevice();
			_fakeUvcBackend.AddDevice();
			VideoCaptureSession videoCaptureSession=CreateSession();
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming);
			Assert.AreSame(_fakeUvcBackend.ListDevices[0],videoCaptureSession.Device);
			Assert.AreEqual(1,OpenedLast.ModeStarted.Index,"ModeSelector's default: the largest at 15 frames/s or more, MJPG first.");
			Assert.IsFalse(OpenedLast.IsStillPinConnected,"No still trigger and PhotoSource=Preview: the still pin is not connected.");
			Assert.IsNotNull(videoCaptureSession.TriggerEngine);
			Assert.IsTrue(videoCaptureSession.TriggerEngine.IsRunning);
		}

		[TestMethod]
		public void SavedDevice_Preselected_AndOverrideApplied() {
			_fakeUvcBackend.AddDevice();
			UvcDeviceInfo device2=_fakeUvcBackend.AddDevice();
			WriteIni("[Camera]\r\nMoniker="+device2.MonikerDisplayName+"\r\nName=Other\r\nWidth=1280\r\nHeight=720\r\nSubtype=YUY2\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming);
			Assert.AreSame(device2,videoCaptureSession.Device);
			Assert.AreEqual(2,OpenedLast.ModeStarted.Index);
			Assert.IsTrue(videoCaptureSession.ModeChoice.IsOverrideMatched);
		}

		[TestMethod]
		public void SavedDeviceMissing_FirstDevice_NoNameRule() {
			_fakeUvcBackend.AddDevice("Camera X");
			_fakeUvcBackend.AddDevice("Camera Y");
			WriteIni("[Camera]\r\nMoniker=@device:gone\r\nName=Camera Y\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			videoCaptureSession.Refresh();
			Assert.AreSame(_fakeUvcBackend.ListDevices[1],videoCaptureSession.Device,"The saved name finds the camera on another port.");
			WriteIni("[Camera]\r\nMoniker=@device:gone\r\nName=Not here\r\n");
			Assert.AreSame(_fakeUvcBackend.ListDevices[0],VideoCaptureSession.ChooseDevice(_fakeUvcBackend.ListDevices,null,
				CameraConfig.Read(LocalCaptureConfig.Load(_path))));
			Assert.IsNull(VideoCaptureSession.ChooseDevice(new List<UvcDeviceInfo>(),null,new CameraConfig()));
		}

		[TestMethod]
		public void SelectDevice_OpensAndRemembers() {
			_fakeUvcBackend.AddDevice();
			UvcDeviceInfo device2=_fakeUvcBackend.AddDevice();
			VideoCaptureSession videoCaptureSession=CreateSession();
			videoCaptureSession.Refresh();
			FakeFrameSource fakeFrameSourceFirst=OpenedLast;
			string error;
			string warning;
			Assert.IsTrue(videoCaptureSession.SelectDevice(device2,out error,out warning).IsStreaming);
			Assert.AreEqual("",error);
			Assert.AreEqual("",warning);
			Assert.IsTrue(fakeFrameSourceFirst.IsDisposed,"Only one camera is held at a time.");
			Assert.AreSame(device2,OpenedLast.Device);
			CameraConfig cameraConfigSaved=CameraConfig.Read(LocalCaptureConfig.Load(_path));
			Assert.AreEqual(device2.MonikerDisplayName,cameraConfigSaved.Moniker);
			Assert.AreEqual(device2.FriendlyName,cameraConfigSaved.Name);
			int countOpened=_fakeUvcBackend.ListOpened.Count;
			videoCaptureSession.SelectDevice(device2,out error,out warning);
			Assert.AreEqual(countOpened,_fakeUvcBackend.ListOpened.Count,"Picking the device already streaming does nothing.");
			//The current device is kept on Refresh even though the saved one comes first.
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming);
			Assert.AreSame(device2,videoCaptureSession.Device);
		}

		[TestMethod]
		public void InUse_OnOpenOrStart_NothingLeftOpen() {
			_fakeUvcBackend.AddDevice();
			_fakeUvcBackend.OpenErrorKind=UvcErrorKind.InUse;
			VideoCaptureSession videoCaptureSession=CreateSession();
			VideoStatus videoStatus=videoCaptureSession.Refresh();
			Assert.AreEqual(VideoStatusKind.InUse,videoStatus.Kind);
			Assert.AreEqual(VideoStatusText.TextInUse,videoStatus.Text);
			Assert.IsNull(videoCaptureSession.FrameSource);
			Assert.IsTrue(_listLog.Any(x => x.Contains("InUse")),"One log line.");
			_fakeUvcBackend.OpenErrorKind=UvcErrorKind.None;
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming,"Refresh after the other program closed.");
		}

		[TestMethod]
		public void StartFailure_DisposesTheSource() {
			FakeUvcBackend fakeUvcBackend=new FakeUvcBackend();
			fakeUvcBackend.AddDevice();
			StartFailingBackend startFailingBackend=new StartFailingBackend(fakeUvcBackend,UvcErrorKind.AccessDenied);
			using(VideoCaptureSession videoCaptureSessionFailing=new VideoCaptureSession(startFailingBackend,LocalCaptureConfig.Load(_path),PAT_NUM)) {
				VideoStatus videoStatus=videoCaptureSessionFailing.Refresh();
				Assert.AreEqual(VideoStatusKind.AccessDenied,videoStatus.Kind);
				Assert.IsTrue(fakeUvcBackend.ListOpened.Single().IsDisposed);
				Assert.IsNull(videoCaptureSessionFailing.FrameSource);
			}
		}

		[TestMethod]
		public void StillPinStopsTheStart_StartedAgainWithThePreviewOnly() {
			_fakeUvcBackend.HasStillPin=true;
			_fakeUvcBackend.StartErrorKindWithStillPin=UvcErrorKind.Failed;
			_fakeUvcBackend.AddDevice();
			WriteIni("[Trigger.Still]\r\nEnabled=1\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming,"Design 8.3: the preview never depends on the still pin.");
			Assert.AreEqual(2,_fakeUvcBackend.ListOpened.Count);
			Assert.IsTrue(_fakeUvcBackend.ListOpened[0].IsDisposed,"The start with the still pin leaves nothing open.");
			FakeFrameSource fakeFrameSource=OpenedLast;
			Assert.AreSame(fakeFrameSource,videoCaptureSession.FrameSource);
			Assert.IsTrue(fakeFrameSource.IsRunning);
			Assert.IsFalse(fakeFrameSource.IsStillPinConnected);
			Assert.IsTrue(_listLog.Any(x => x.Contains("without the still pin")),"One log line says why.");
			Assert.AreEqual(TriggerInputState.Incomplete,videoCaptureSession.TriggerEngine.StillState,"The still trigger is off, with its reason.");
			Assert.IsFalse(videoCaptureSession.NeedsReopen(),"The missing still pin is no reason to reopen.");
			int countOpened=_fakeUvcBackend.ListOpened.Count;
			Assert.IsTrue(videoCaptureSession.PrepareForLearn().IsStreaming);
			Assert.AreEqual(countOpened,_fakeUvcBackend.ListOpened.Count,"Learn uses the running preview.");
			Assert.IsTrue(videoCaptureSession.FinishLearn(false).IsStreaming);
			Assert.AreEqual(countOpened,_fakeUvcBackend.ListOpened.Count);
			Assert.IsNotNull(videoCaptureSession.TriggerEngine);
		}

		[TestMethod]
		public void Learn_StillPinStopsTheStart_LearnOpensOnThePreview() {
			_fakeUvcBackend.HasStillPin=true;
			_fakeUvcBackend.AddDevice();
			VideoCaptureSession videoCaptureSession=CreateSession();
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming);
			Assert.IsFalse(OpenedLast.IsStillPinConnected,"Nothing in capture.ini wants the still pin.");
			_fakeUvcBackend.StartErrorKindWithStillPin=UvcErrorKind.Failed;
			Assert.IsTrue(videoCaptureSession.PrepareForLearn().IsStreaming,"Not reported as a failed camera.");
			Assert.AreEqual(3,_fakeUvcBackend.ListOpened.Count,"Reopened with the still pin for Learn, which did not start, then without it.");
			Assert.IsFalse(OpenedLast.IsStillPinConnected);
			Assert.IsTrue(OpenedLast.IsRunning);
			Assert.IsNull(videoCaptureSession.TriggerEngine);
			Assert.IsTrue(videoCaptureSession.FinishLearn(false).IsStreaming);
			Assert.IsNotNull(videoCaptureSession.TriggerEngine);
			Assert.IsTrue(videoCaptureSession.PrepareForLearn().IsStreaming);
			Assert.AreEqual(3,_fakeUvcBackend.ListOpened.Count,"The next Learn does not try the still pin again.");
		}

		[TestMethod]
		public void StartFailure_WithAndWithoutStillPin_NothingLeftOpen() {
			FakeUvcBackend fakeUvcBackend=new FakeUvcBackend();
			fakeUvcBackend.HasStillPin=true;
			fakeUvcBackend.AddDevice();
			WriteIni("[Camera]\r\nPhotoSource=Still\r\n");
			StartFailingBackend startFailingBackend=new StartFailingBackend(fakeUvcBackend,UvcErrorKind.AccessDenied);
			using(VideoCaptureSession videoCaptureSessionFailing=new VideoCaptureSession(startFailingBackend,LocalCaptureConfig.Load(_path),PAT_NUM)) {
				Assert.AreEqual(VideoStatusKind.AccessDenied,videoCaptureSessionFailing.Refresh().Kind);
				Assert.AreEqual(2,fakeUvcBackend.ListOpened.Count,"Tried once more without the still pin.");
				Assert.IsTrue(fakeUvcBackend.ListOpened.All(x => x.IsDisposed));
				Assert.IsNull(videoCaptureSessionFailing.FrameSource);
			}
		}

		[TestMethod]
		public void CameraError_DisconnectedAndReconnect_StaleErrorIgnored() {
			_fakeUvcBackend.AddDevice();
			VideoCaptureSession videoCaptureSession=CreateSession();
			List<object> listSenders=new List<object>();
			videoCaptureSession.CameraError+=(sender,e) => listSenders.Add(sender);
			videoCaptureSession.Refresh();
			FakeFrameSource fakeFrameSource=OpenedLast;
			fakeFrameSource.RaiseError(UvcErrorKind.DeviceLost);
			Assert.AreSame(fakeFrameSource,listSenders.Single(),"The sender is the frame source, for HandleCameraError.");
			VideoStatus videoStatus=videoCaptureSession.HandleCameraError(fakeFrameSource,new UvcErrorEventArgs(UvcErrorKind.DeviceLost,0,"x"));
			Assert.AreEqual(VideoStatusKind.Disconnected,videoStatus.Kind);
			Assert.AreEqual(VideoStatusAction.Reconnect,videoStatus.Action);
			Assert.IsTrue(fakeFrameSource.IsDisposed,"The graph is stopped and the camera released.");
			Assert.IsNull(videoCaptureSession.TriggerEngine);
			Assert.IsNotNull(videoCaptureSession.Device,"The device is kept for Reconnect.");
			Assert.IsNull(videoCaptureSession.HandleCameraError(fakeFrameSource,new UvcErrorEventArgs(UvcErrorKind.ErrorAbort,0,"late")),
				"An error from a camera that is no longer open is ignored.");
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming,"Reconnect.");
			Assert.AreNotSame(fakeFrameSource,OpenedLast);
		}

		[TestMethod]
		public void DeviceNames_DuplicatesNumbered() {
			List<UvcDeviceInfo> listDevices=new List<UvcDeviceInfo>() {
				new UvcDeviceInfo("Cam","","@a"),new UvcDeviceInfo("Cam","","@b"),new UvcDeviceInfo("Other\0junk","","@c"),new UvcDeviceInfo("","","@d"),
			};
			CollectionAssert.AreEqual(new[] { "Cam","Cam (2)","Other","Camera" },VideoCaptureSession.GetDeviceNames(listDevices));
		}
		#endregion Devices and opening

		#region Photos
		[TestMethod]
		public void Capture_ClonesTheNewestFrame_GuardUntilEnd() {
			_fakeUvcBackend.AddDevice();
			VideoCaptureSession videoCaptureSession=CreateSession();
			videoCaptureSession.Refresh();
			FakeFrameSource fakeFrameSource=OpenedLast;
			Assert.AreEqual(CaptureDropReason.NoPicture,videoCaptureSession.BeginCapture(CaptureRequestSource.Button,null).DropReason);
			Assert.IsFalse(videoCaptureSession.Gate.IsHandlerRunning,"No picture releases the gate.");
			PushFrame(fakeFrameSource,0x112233);
			UvcFrame framePreview=videoCaptureSession.TakePreviewFrame();
			Assert.IsNotNull(framePreview);
			PushFrame(fakeFrameSource,0x445566);
			CaptureStart captureStart=videoCaptureSession.BeginCapture(CaptureRequestSource.Button,null);
			Assert.IsTrue(captureStart.IsAccepted);
			Assert.IsFalse(captureStart.IsFromStill);
			Assert.AreEqual(Color.FromArgb(0x44,0x55,0x66).ToArgb(),captureStart.Bitmap.GetPixel(3,3).ToArgb(),"The newest frame, not the one shown.");
			Assert.AreEqual(CaptureDropReason.Busy,videoCaptureSession.BeginCapture(CaptureRequestSource.Trigger,null).DropReason,
				"No new photo while the handler runs.");
			Bitmap bitmap=captureStart.Bitmap;
			bitmap.Dispose();//What ControlImageDisplay does with the bitmap it receives.
			videoCaptureSession.EndCapture(captureStart);
			videoCaptureSession.EndCapture(captureStart);
			Assert.IsFalse(videoCaptureSession.Gate.IsHandlerRunning);
			TestUtil.AssertThrows<ArgumentException>(() => { int width=bitmap.Width; });
			CaptureStart captureStart2=videoCaptureSession.BeginCapture(CaptureRequestSource.SpaceInWindow,null);
			Assert.IsTrue(captureStart2.IsAccepted,"The last frame is used again when no newer one came.");
			Assert.AreEqual(Color.FromArgb(0x44,0x55,0x66).ToArgb(),captureStart2.Bitmap.GetPixel(0,0).ToArgb());
			videoCaptureSession.EndCapture(captureStart2);
			Assert.AreEqual(2,videoCaptureSession.CountPhotos);
		}

		[TestMethod]
		public void Capture_DropRules() {
			_fakeUvcBackend.AddDevice();
			WriteIni("[Camera]\r\nMainWindowSpace=0\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			videoCaptureSession.Refresh();
			PushFrame(OpenedLast,0x10);
			Assert.AreEqual(CaptureDropReason.MainWindowSpaceOff,videoCaptureSession.BeginCapture(CaptureRequestSource.SpaceInMainWindow,null).DropReason);
			_isWindowEnabled=false;
			Assert.AreEqual(CaptureDropReason.WindowDisabled,videoCaptureSession.BeginCapture(CaptureRequestSource.Button,null).DropReason);
			_isWindowEnabled=true;
			videoCaptureSession.Gate.BeginClosing();
			Assert.AreEqual(CaptureDropReason.Closing,videoCaptureSession.BeginCapture(CaptureRequestSource.Button,null).DropReason);
			Assert.AreEqual(0,videoCaptureSession.CountPhotos);
			VideoCaptureSession videoCaptureSessionNoPatient=new VideoCaptureSession(_fakeUvcBackend,LocalCaptureConfig.Load(_path),0);
			Assert.AreEqual(CaptureDropReason.NoPatient,videoCaptureSessionNoPatient.BeginCapture(CaptureRequestSource.Button,null).DropReason);
			videoCaptureSessionNoPatient.Dispose();
		}

		[TestMethod]
		public void PhotoSource_StillUsesTheStillSample() {
			_fakeUvcBackend.HasStillPin=true;
			_fakeUvcBackend.AddDevice();
			WriteIni("[Camera]\r\nPhotoSource=Still\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			videoCaptureSession.Refresh();
			FakeFrameSource fakeFrameSource=OpenedLast;
			Assert.IsTrue(fakeFrameSource.IsStillPinConnected,"PhotoSource=Still connects the still pin.");
			PushFrame(fakeFrameSource,0x010101);
			UvcFrame frameStill=new UvcFrame(4,4);
			for(int i=0;i<frameStill.Pixels.Length;i+=4) {
				frameStill.Pixels[i]=0x30;
				frameStill.Pixels[i+3]=0xFF;
			}
			CaptureStart captureStart=videoCaptureSession.BeginCapture(CaptureRequestSource.Trigger,frameStill);
			Assert.IsTrue(captureStart.IsFromStill);
			Assert.AreEqual(4,captureStart.Bitmap.Width);
			videoCaptureSession.EndCapture(captureStart);
			CaptureStart captureStartButton=videoCaptureSession.BeginCapture(CaptureRequestSource.Button,null);
			Assert.IsFalse(captureStartButton.IsFromStill,"The Capture button has no still sample: the preview frame is used.");
			Assert.AreEqual(8,captureStartButton.Bitmap.Width);
			videoCaptureSession.EndCapture(captureStartButton);
			Assert.IsFalse(VideoCaptureSession.ShouldUseStillFrame(CameraPhotoSource.Preview,true,frameStill));
			Assert.IsFalse(VideoCaptureSession.ShouldUseStillFrame(CameraPhotoSource.Still,false,frameStill));
			Assert.IsTrue(VideoCaptureSession.ShouldUseStillFrame(CameraPhotoSource.Still,true,frameStill));
		}

		[TestMethod]
		public void StillTrigger_PressedWhileEnabled_IgnoredWhileDisabled() {
			_fakeUvcBackend.HasStillPin=true;
			_fakeUvcBackend.AddDevice();
			WriteIni("[Trigger]\r\nDebounceMs=1\r\n[Trigger.Still]\r\nEnabled=1\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			int countPressed=0;
			int countIgnored=0;
			videoCaptureSession.TriggerPressed+=(sender,e) => countPressed++;
			videoCaptureSession.TriggerPressIgnored+=(sender,e) => countIgnored++;
			videoCaptureSession.Refresh();
			FakeFrameSource fakeFrameSource=OpenedLast;
			Assert.IsTrue(fakeFrameSource.IsStillPinConnected,"[Trigger.Still] Enabled=1 connects the still pin.");
			Assert.AreEqual(TriggerInputState.Armed,videoCaptureSession.TriggerEngine.StillState);
			fakeFrameSource.PushStill(4,4,(x,y) => 0);
			Assert.AreEqual(1,countPressed);
			System.Threading.Thread.Sleep(5);
			_isWindowEnabled=false;
			fakeFrameSource.PushStill(4,4,(x,y) => 0);
			Assert.AreEqual(1,countPressed,"Presses are ignored while the window is disabled.");
			Assert.AreEqual(1,countIgnored);
			Assert.IsTrue(_listLog.Any(x => x.Contains("ignored")));
			CollectionAssert.AreEqual(new[] { "Still-pin camera button: on" },videoCaptureSession.GetTriggerNotes());
			videoCaptureSession.StopTriggers();
			System.Threading.Thread.Sleep(5);
			_isWindowEnabled=true;
			fakeFrameSource.PushStill(4,4,(x,y) => 0);
			Assert.AreEqual(1,countPressed,"Nothing after the triggers stopped.");
			CollectionAssert.AreEqual(new[] { TriggerNotes.TextNotRunning },videoCaptureSession.GetTriggerNotes(),
				"With the engine stopped the note says the buttons work while the camera runs.");
		}
		#endregion Photos

		#region Setting buttons
		//A handpiece whose buttons are active-low bits of CameraControl Roll: bit 0 the rest sensor, bit 2 an on/off button, bit 5 raise, bit 1
		//lower, all on the camera setting Gain (VideoProcAmp 9), range 0-255 with default 0 in the fake, so the first "on" is the maximum,
		//limited by Highest.
		private const int ROLL_IDLE=0x3F;
		private const int ROLL_LIGHT_PRESSED=0x3B;
		private const int ROLL_RESTING=0x3E;
		private const int ROLL_UP_PRESSED=0x1F;
		private const int ROLL_DOWN_PRESSED=0x3D;

		private VideoCaptureSession OpenWithLight(int gain,string lightMax,bool isRestLevel,string extraIni="") {
			_fakeUvcBackend.AddDevice();
			string highest=lightMax==null ? "" : "Highest="+lightMax+"\r\n";
			WriteIni("[Trigger]\r\nDebounceMs=1\r\n"
				+(isRestLevel ? "[Level.1]\r\nEnabled=1\r\nControl=Roll\r\nBits=0\r\nActiveLevel=Low\r\nAction=PausePreview|TurnOff\r\nSetting=Gain\r\n"+highest : "")
				+"[Level.2]\r\nEnabled=1\r\nControl=Roll\r\nBits=2\r\nActiveLevel=Low\r\nAction=Toggle\r\nSetting=Gain\r\n"+highest
				+"[Level.3]\r\nEnabled=1\r\nControl=Roll\r\nBits=5\r\nActiveLevel=Low\r\nAction=Raise\r\nSetting=Gain\r\n"+highest
				+"[Level.4]\r\nEnabled=1\r\nControl=Roll\r\nBits=1\r\nActiveLevel=Low\r\nAction=Lower\r\nSetting=Gain\r\n"+highest
				+extraIni);
			VideoCaptureSession videoCaptureSession=CreateSession();
			//The fake's controls exist before the camera opens: the backend hands them to each frame source it opens.
			_fakeUvcBackend.ActionOnOpen=x => {
				x.SetControlValue(UvcControlInterface.CameraControl,2,ROLL_IDLE);
				x.SetControlValue(UvcControlInterface.VideoProcAmp,9,gain);
			};
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming);
			return videoCaptureSession;
		}

		private void Poll(VideoCaptureSession videoCaptureSession,int roll) {
			OpenedLast.SetControlValue(UvcControlInterface.CameraControl,2,roll);
			videoCaptureSession.TriggerEngine.PollOnce();
			System.Threading.Thread.Sleep(5);
		}

		private void Press(VideoCaptureSession videoCaptureSession,int rollPressed=ROLL_LIGHT_PRESSED) {
			Poll(videoCaptureSession,rollPressed);
			Poll(videoCaptureSession,ROLL_IDLE);
		}

		private List<int> GetLightWrites() {
			return OpenedLast.ListWrites.Where(x => x.Interface==UvcControlInterface.VideoProcAmp && x.Property==9).Select(x => x.Value).ToList();
		}

		[TestMethod]
		public void Light_AboveLightMaxAtOpen_IsLimited() {
			OpenWithLight(64,"20",false);
			CollectionAssert.AreEqual(new List<int> { 20 },GetLightWrites(),"The light starts no brighter than LightMax.");
			StringAssert.Contains(string.Join("\n",_listLog),"Camera setting Gain limited to Highest: 64 -> 20.");
		}

		[TestMethod]
		public void Light_AtOrBelowLightMax_OrNoLightMax_NotWrittenAtOpen() {
			OpenWithLight(12,"20",false);
			Assert.AreEqual(0,GetLightWrites().Count,"Below LightMax: left as it is.");
			_videoCaptureSession.Dispose();
			_fakeUvcBackend=new FakeUvcBackend();
			OpenWithLight(200,null,false);
			Assert.AreEqual(0,GetLightWrites().Count,"Without LightMax nothing is written at open.");
		}

		[TestMethod]
		public void Light_Button_TogglesAndRestoresTheValue() {
			VideoCaptureSession videoCaptureSession=OpenWithLight(12,"20",false);
			Poll(videoCaptureSession,ROLL_IDLE);
			Press(videoCaptureSession);
			Press(videoCaptureSession);
			CollectionAssert.AreEqual(new List<int> { 0,12 },GetLightWrites(),"Off, then back to the value it had.");
		}

		[TestMethod]
		public void Light_Button_TurningOnNeverAboveLightMax() {
			//Off at open (0): the first "on" would be the camera's maximum (the fake's default is 0), and LightMax limits it.
			VideoCaptureSession videoCaptureSession=OpenWithLight(0,"20",false);
			Poll(videoCaptureSession,ROLL_IDLE);
			Press(videoCaptureSession);
			CollectionAssert.AreEqual(new List<int> { 20 },GetLightWrites());
		}

		[TestMethod]
		public void Light_Button_WithoutLightMax_UsesTheCameraRange() {
			VideoCaptureSession videoCaptureSession=OpenWithLight(0,null,false);
			Poll(videoCaptureSession,ROLL_IDLE);
			Press(videoCaptureSession);
			CollectionAssert.AreEqual(new List<int> { 255 },GetLightWrites());
		}

		[TestMethod]
		public void Light_Brighter_StepsUpToLightMax_FromOff() {
			//LightMax 20, minimum 0: five steps of 4.
			VideoCaptureSession videoCaptureSession=OpenWithLight(0,"20",false);
			Poll(videoCaptureSession,ROLL_IDLE);
			for(int i=0;i<6;i++) {
				Press(videoCaptureSession,ROLL_UP_PRESSED);
			}
			CollectionAssert.AreEqual(new List<int> { 4,8,12,16,20 },GetLightWrites(),"Off: on at the lowest step; at LightMax: nothing more.");
			StringAssert.Contains(string.Join("\n",_listLog),"Camera setting Gain raised (button): 16 -> 20.");
		}

		[TestMethod]
		public void Light_Dimmer_StopsAtTheLowestStep_AndNothingWhenOff() {
			VideoCaptureSession videoCaptureSession=OpenWithLight(14,"20",false);
			Poll(videoCaptureSession,ROLL_IDLE);
			for(int i=0;i<4;i++) {
				Press(videoCaptureSession,ROLL_DOWN_PRESSED);
			}
			CollectionAssert.AreEqual(new List<int> { 10,6,4 },GetLightWrites(),"Dimmer never turns the light off.");
			Press(videoCaptureSession);
			Press(videoCaptureSession,ROLL_DOWN_PRESSED);
			CollectionAssert.AreEqual(new List<int> { 10,6,4,0 },GetLightWrites(),"Off by the on/off button; dimmer then does nothing.");
		}

		[TestMethod]
		public void Light_BrightnessChosen_KeptThroughOffAndOn() {
			VideoCaptureSession videoCaptureSession=OpenWithLight(20,"20",true);
			Poll(videoCaptureSession,ROLL_IDLE);
			Press(videoCaptureSession,ROLL_DOWN_PRESSED);
			Press(videoCaptureSession);
			Press(videoCaptureSession);
			Poll(videoCaptureSession,ROLL_RESTING);
			Poll(videoCaptureSession,ROLL_IDLE);
			CollectionAssert.AreEqual(new List<int> { 16,0,16,0,16 },GetLightWrites(),"The on/off button and the holder bring back the brightness chosen.");
		}

		[TestMethod]
		public void Light_Brighter_WithoutLightMax_StepsAFifthOfTheRange() {
			VideoCaptureSession videoCaptureSession=OpenWithLight(0,null,false);
			Poll(videoCaptureSession,ROLL_IDLE);
			Press(videoCaptureSession,ROLL_UP_PRESSED);
			Press(videoCaptureSession,ROLL_UP_PRESSED);
			CollectionAssert.AreEqual(new List<int> { 51,102 },GetLightWrites(),"Range 0-255: steps of 51.");
		}

		[TestMethod]
		public void Light_RestLevel_OffWhileResting_OnWhenPickedUp() {
			VideoCaptureSession videoCaptureSession=OpenWithLight(15,"20",true);
			Poll(videoCaptureSession,ROLL_IDLE);
			Poll(videoCaptureSession,ROLL_RESTING);
			CollectionAssert.AreEqual(new List<int> { 0 },GetLightWrites(),"Put down: the light goes off.");
			Poll(videoCaptureSession,ROLL_IDLE);
			CollectionAssert.AreEqual(new List<int> { 0,15 },GetLightWrites(),"Picked up: back to the value it had.");
			StringAssert.Contains(string.Join("\n",_listLog),"Camera setting Gain off (level 1 active): 15 -> 0.");
		}

		[TestMethod]
		public void Light_RestLevel_PickedUpAfterTheButtonTurnedItOff_ComesOn() {
			VideoCaptureSession videoCaptureSession=OpenWithLight(15,"20",true);
			Poll(videoCaptureSession,ROLL_IDLE);
			Press(videoCaptureSession);
			Poll(videoCaptureSession,ROLL_RESTING);
			Poll(videoCaptureSession,ROLL_IDLE);
			CollectionAssert.AreEqual(new List<int> { 0,15 },GetLightWrites(),"Off by the button; resting leaves it off; picked up turns it on.");
		}

		[TestMethod]
		public void Light_RestingWhenTheCameraOpens_GoesOff() {
			VideoCaptureSession videoCaptureSession=OpenWithLight(15,null,true);
			Poll(videoCaptureSession,ROLL_RESTING);
			CollectionAssert.AreEqual(new List<int> { 0 },GetLightWrites(),"Already in its holder at the first reading.");
		}

		[TestMethod]
		public void Light_RestLevelWithoutASetting_DoesNothing() {
			_fakeUvcBackend.AddDevice();
			WriteIni("[Level.1]\r\nEnabled=1\r\nControl=Roll\r\nBits=0\r\nActiveLevel=Low\r\nAction=PausePreview|TurnOff\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			_fakeUvcBackend.ActionOnOpen=x => {
				x.SetControlValue(UvcControlInterface.CameraControl,2,ROLL_IDLE);
				x.SetControlValue(UvcControlInterface.VideoProcAmp,9,15);
			};
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming);
			Poll(videoCaptureSession,ROLL_IDLE);
			Poll(videoCaptureSession,ROLL_RESTING);
			Assert.AreEqual(0,GetLightWrites().Count,"TurnOff without a Setting is not configured: nothing is written.");
		}

		[TestMethod]
		public void SettingButtons_WorkOnAnySetting() {
			//The same actions on Brightness (VideoProcAmp 0), range 10-110 in steps of 5, and SetValue on Exposure (CameraControl 4).
			_fakeUvcBackend.AddDevice();
			WriteIni("[Trigger]\r\nDebounceMs=1\r\n"
				+"[Level.1]\r\nEnabled=1\r\nControl=Roll\r\nBits=5\r\nActiveLevel=Low\r\nAction=Raise\r\nSetting=Brightness\r\nSteps=2\r\n"
				+"[Level.2]\r\nEnabled=1\r\nControl=Roll\r\nBits=2\r\nActiveLevel=Low\r\nAction=SetValue\r\nSetting=Exposure\r\nValue=Auto\r\n"
				+"[Level.3]\r\nEnabled=1\r\nControl=Roll\r\nBits=1\r\nActiveLevel=Low\r\nAction=Toggle\r\nSetting=Brightness\r\nLowest=20\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			_fakeUvcBackend.ActionOnOpen=x => {
				x.SetControlValue(UvcControlInterface.CameraControl,2,ROLL_IDLE);
				x.SetControlRange(UvcControlInterface.VideoProcAmp,0,10,110,5,60,UvcControls.FlagsManual);
				x.SetControlValue(UvcControlInterface.VideoProcAmp,0,30);
				x.SetControlRange(UvcControlInterface.CameraControl,4,-11,0,1,-6,UvcControls.FlagsAuto|UvcControls.FlagsManual);
				x.SetControlValue(UvcControlInterface.CameraControl,4,-6);
			};
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming);
			Poll(videoCaptureSession,ROLL_IDLE);
			Press(videoCaptureSession,ROLL_UP_PRESSED);
			Press(videoCaptureSession,ROLL_UP_PRESSED);
			Press(videoCaptureSession,ROLL_LIGHT_PRESSED);
			Press(videoCaptureSession,ROLL_DOWN_PRESSED);
			Press(videoCaptureSession,ROLL_DOWN_PRESSED);
			CollectionAssert.AreEqual(new[] { "Brightness=80","Brightness=110","Exposure=Auto","Brightness=20","Brightness=110" },
				OpenedLast.ListWrites.Select(x => x.ToString()).ToArray(),
				"Steps of 50 (two steps from 10 to 110) up to the maximum, Auto, then off at Lowest and back on.");
			StringAssert.Contains(string.Join("\n",_listLog),"Camera setting Exposure set (button): 1/64 s -> Auto.");
		}

		[TestMethod]
		public void TakePhotoButton_TakesPhotosThroughTheLockout() {
			_fakeUvcBackend.AddDevice();
			WriteIni("[Trigger]\r\nDebounceMs=1000\r\n[Level.1]\r\nEnabled=1\r\nControl=Roll\r\nBits=2\r\nActiveLevel=Low\r\nAction=TakePhoto\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			_fakeUvcBackend.ActionOnOpen=x => x.SetControlValue(UvcControlInterface.CameraControl,2,ROLL_IDLE);
			int countPressed=0;
			List<LevelAction> listLevelActions=new List<LevelAction>();
			videoCaptureSession.TriggerPressed+=(sender,e) => {
				countPressed++;
				Assert.AreEqual(TriggerSource.Button,e.Source);
			};
			videoCaptureSession.LevelChanged+=(sender,e) => listLevelActions.Add(e.Action);
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming);
			Poll(videoCaptureSession,ROLL_IDLE);
			Press(videoCaptureSession);
			Press(videoCaptureSession);
			Assert.AreEqual(1,countPressed,"One photo per press; a second press within the lockout is a bounce.");
			Assert.AreEqual(0,OpenedLast.ListWrites.Count);
			Assert.IsFalse(listLevelActions.Any(x => CameraButtonAction.GetCommand(new LevelChangedEventArgs(1,true,x,false))!=null),
				"TakePhoto is no window command: it comes as TriggerPressed.");
		}

		[TestMethod]
		public void ProgramButtons_AskTheWindowOncePerPress() {
			_fakeUvcBackend.AddDevice();
			WriteIni("[Trigger]\r\nDebounceMs=1\r\n[Level.1]\r\nEnabled=1\r\nControl=Roll\r\nBits=2\r\nActiveLevel=Low\r\nAction=NextSlot\r\n"
				+"[Level.2]\r\nEnabled=1\r\nControl=Roll\r\nBits=5\r\nActiveLevel=Low\r\nAction=Freeze\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			_fakeUvcBackend.ActionOnOpen=x => x.SetControlValue(UvcControlInterface.CameraControl,2,ROLL_IDLE);
			List<LevelAction> listCommands=new List<LevelAction>();
			videoCaptureSession.LevelChanged+=(sender,e) => {
				LevelAction? command=CameraButtonAction.GetCommand(e);
				if(command!=null) {
					listCommands.Add(command.Value);
				}
			};
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming);
			Poll(videoCaptureSession,ROLL_IDLE);
			Press(videoCaptureSession);
			Press(videoCaptureSession,ROLL_UP_PRESSED);
			Press(videoCaptureSession);
			CollectionAssert.AreEqual(new[] { LevelAction.NextSlot,LevelAction.Freeze,LevelAction.NextSlot },listCommands.ToArray(),
				"One command per press, none on the release.");
			Assert.AreEqual(0,OpenedLast.ListWrites.Count,"Program buttons write no camera setting.");
		}
		#endregion Setting buttons

		#region Saved camera settings
		[TestMethod]
		public void SavedSettings_AppliedAtOpen_RefusalLoggedAndTheRestApply() {
			_fakeUvcBackend.AddDevice();
			WriteIni("[Trigger.ControlPoll]\r\nEnabled=1\r\nControl=Roll\r\nBits=3\r\nActiveLevel=Low\r\n"
				+"[Camera.Settings]\r\nGain=20\r\nContrast=40\r\nExposure=1/64 s\r\nPowerLineFrequency=60 Hz\r\nBrightness=Auto\r\nRoll=5\r\n"
				+"Sharpness=\r\nFocus=12\r\nShine=3\r\nSaturation=abc\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			_fakeUvcBackend.ActionOnOpen=x => {
				x.SetControlValue(UvcControlInterface.CameraControl,2,ROLL_IDLE);
				x.SetControlValue(UvcControlInterface.VideoProcAmp,9,64);
				x.SetControlValue(UvcControlInterface.VideoProcAmp,1,10);
				x.SetControlRange(UvcControlInterface.CameraControl,4,-11,0,1,-6,UvcControls.FlagsAuto|UvcControls.FlagsManual);
				x.SetControlValue(UvcControlInterface.CameraControl,4,-3);
				x.SetControlRange(UvcControlInterface.VideoProcAmp,UvcControls.VideoProcAmpPowerLineFrequency,0,3,1,1,UvcControls.FlagsManual);
				x.SetControlValue(UvcControlInterface.VideoProcAmp,UvcControls.VideoProcAmpPowerLineFrequency,1);
				x.SetControlRange(UvcControlInterface.VideoProcAmp,0,0,255,1,128,UvcControls.FlagsAuto|UvcControls.FlagsManual);
				x.SetControlValue(UvcControlInterface.VideoProcAmp,0,128);
				x.SetControlValue(UvcControlInterface.VideoProcAmp,2,7);
				x.FuncWriteStatus=(controlInterface,property,value) => controlInterface==UvcControlInterface.VideoProcAmp && property==1
					? UvcControlStatus.Failed : UvcControlStatus.Ok;
			};
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming);
			CollectionAssert.AreEqual(new[] { "Brightness=Auto","Gain=20","PowerLineFrequency=2","Exposure=-6" },
				OpenedLast.ListWrites.Select(x => x.ToString()).ToArray(),"In catalog order; the refused Contrast, the button's Roll and the rest not written.");
			string log=string.Join("\n",_listLog);
			StringAssert.Contains(log,"Camera setting Gain: 64 -> 20 (saved in capture.ini).");
			StringAssert.Contains(log,"Camera setting Exposure: 1/8 s -> 1/64 s (saved in capture.ini).");
			StringAssert.Contains(log,"Camera setting Power line frequency: 50 Hz -> 60 Hz (saved in capture.ini).");
			StringAssert.Contains(log,"Camera setting Contrast: 10 -> 40 (saved in capture.ini) failed (Failed");
			StringAssert.Contains(log,"Camera setting Roll (5 in capture.ini) was left alone: the camera's buttons are read from it.");
			StringAssert.Contains(log,"Camera setting Focus (12 in capture.ini) was not applied: this camera does not offer it");
			StringAssert.Contains(log,"[Camera.Settings] Shine is not a camera setting");
			StringAssert.Contains(log,"Camera setting Saturation (abc in capture.ini) was not applied:");
			Assert.IsTrue(videoCaptureSession.TriggerEngine!=null,"The triggers start after the settings.");
		}

		[TestMethod]
		public void SavedSettings_AlreadyAsSaved_NotWritten() {
			_fakeUvcBackend.AddDevice();
			WriteIni("[Camera.Settings]\r\nGain=20\r\n");
			VideoCaptureSession videoCaptureSession=CreateSession();
			_fakeUvcBackend.ActionOnOpen=x => x.SetControlValue(UvcControlInterface.VideoProcAmp,9,20);
			Assert.IsTrue(videoCaptureSession.Refresh().IsStreaming);
			Assert.AreEqual(0,OpenedLast.ListWrites.Count);
		}
		#endregion Saved camera settings

		#region Settings
		[TestMethod]
		public void Resolution_SavedAndReopened() {
			_fakeUvcBackend.AddDevice();
			VideoCaptureSession videoCaptureSession=CreateSession();
			videoCaptureSession.Refresh();
			FakeFrameSource fakeFrameSourceFirst=OpenedLast;
			Assert.AreEqual(1,fakeFrameSourceFirst.ModeStarted.Index);
			Assert.IsFalse(videoCaptureSession.NeedsReopen());
			UvcMode mode640=fakeFrameSourceFirst.Modes[0];
			string error;
			string warning;
			Assert.IsTrue(videoCaptureSession.SaveSettings(x => ResolutionChoice.Apply(x,mode640),out error,out warning),error);
			Assert.AreEqual("",warning);
			Assert.IsTrue(videoCaptureSession.NeedsReopen());
			Assert.IsTrue(videoCaptureSession.ApplySettings().IsStreaming);
			Assert.IsTrue(fakeFrameSourceFirst.IsDisposed);
			Assert.AreEqual(0,OpenedLast.ModeStarted.Index);
			CameraConfig cameraConfigSaved=CameraConfig.Read(LocalCaptureConfig.Load(_path));
			Assert.AreEqual(640,cameraConfigSaved.Width);
			Assert.AreEqual(480,cameraConfigSaved.Height);
			Assert.AreEqual("MJPG",cameraConfigSaved.Subtype);
			Assert.AreEqual(_fakeUvcBackend.ListDevices[0].MonikerDisplayName,cameraConfigSaved.Moniker,"The settings name the camera they belong to.");
			int countOpened=_fakeUvcBackend.ListOpened.Count;
			Assert.IsTrue(videoCaptureSession.SaveSettings(x => x.MainWindowSpace=false,out error,out warning));
			Assert.IsFalse(videoCaptureSession.NeedsReopen());
			videoCaptureSession.ApplySettings();
			Assert.AreEqual(countOpened,_fakeUvcBackend.ListOpened.Count,"A setting that does not change the stream restarts only the triggers.");
			Assert.IsFalse(videoCaptureSession.CameraConfig.MainWindowSpace);
		}

		[TestMethod]
		public void SaveSettings_SavedToTheFallback_ReturnsTheWarning() {
			_fakeUvcBackend.AddDevice();
			UvcDeviceInfo device2=_fakeUvcBackend.AddDevice();
			WriteIni("[Camera]\r\n");
			//The ProgramData file exists but cannot be written: the save goes to the per-user file, which is not read while it exists.
			File.SetAttributes(_path,FileAttributes.ReadOnly);
			string pathFallback=Path.Combine(_folder,"user","capture.ini");
			_videoCaptureSession=new VideoCaptureSession(_fakeUvcBackend,LocalCaptureConfig.Load(_path,pathFallback),PAT_NUM);
			_videoCaptureSession.ActionLog=x => _listLog.Add(x);
			VideoCaptureSession videoCaptureSession=_videoCaptureSession;
			videoCaptureSession.Refresh();
			string error;
			string warning;
			Assert.IsTrue(videoCaptureSession.SaveSettings(x => x.MainWindowSpace=false,out error,out warning),error);
			Assert.AreEqual("",error);
			StringAssert.Contains(warning,"take effect only after");
			Assert.IsTrue(_listLog.Any(x => x.Contains(warning)),"The warning is logged.");
			Assert.IsFalse(videoCaptureSession.CameraConfig.MainWindowSpace,"The value applies in this window now.");
			Assert.IsTrue(videoCaptureSession.SelectDevice(device2,out error,out warning).IsStreaming);
			Assert.AreEqual("",error);
			StringAssert.Contains(warning,"take effect only after","The camera choice gets the same warning.");
		}

		[TestMethod]
		public void Learn_StillPinConnectedForLearn_TriggersStoppedThenRestarted() {
			_fakeUvcBackend.HasStillPin=true;
			_fakeUvcBackend.AddDevice();
			VideoCaptureSession videoCaptureSession=CreateSession();
			videoCaptureSession.Refresh();
			FakeFrameSource fakeFrameSourceFirst=OpenedLast;
			Assert.IsFalse(fakeFrameSourceFirst.IsStillPinConnected);
			Assert.IsTrue(videoCaptureSession.PrepareForLearn().IsStreaming);
			Assert.IsNull(videoCaptureSession.TriggerEngine,"Learn's key presses take no photo.");
			Assert.IsTrue(fakeFrameSourceFirst.IsDisposed);
			FakeFrameSource fakeFrameSourceLearn=OpenedLast;
			Assert.IsTrue(fakeFrameSourceLearn.IsStillPinConnected,"Reopened with the still pin, so a still-pin key can be learned.");
			Assert.AreEqual(fakeFrameSourceFirst.ModeStarted.Index,fakeFrameSourceLearn.ModeStarted.Index,"Same stream format.");
			//What the Learn window's Save does: write and save the same loaded capture.ini.
			CameraConfig cameraConfigLearned=CameraConfig.Read(videoCaptureSession.LocalCaptureConfig);
			cameraConfigLearned.StillEnabled=true;
			cameraConfigLearned.DebounceMs=400;
			string error;
			Assert.IsTrue(cameraConfigLearned.Write(videoCaptureSession.LocalCaptureConfig,out error),error);
			Assert.IsTrue(videoCaptureSession.LocalCaptureConfig.Save());
			Assert.IsTrue(videoCaptureSession.FinishLearn(true).IsStreaming);
			Assert.AreSame(fakeFrameSourceLearn,OpenedLast,"The still pin is already connected: no reopen.");
			Assert.IsNotNull(videoCaptureSession.TriggerEngine);
			Assert.AreEqual(400,videoCaptureSession.TriggerEngine.DebounceMs,"The new engine uses the learned values.");
			Assert.AreEqual(TriggerInputState.Armed,videoCaptureSession.TriggerEngine.StillState);
			Assert.AreEqual(_fakeUvcBackend.ListDevices[0].MonikerDisplayName,CameraConfig.Read(LocalCaptureConfig.Load(_path)).Moniker,
				"The camera the values were learned on is saved with them.");
		}

		[TestMethod]
		public void Learn_Cancelled_RestartsTriggers() {
			_fakeUvcBackend.AddDevice();
			VideoCaptureSession videoCaptureSession=CreateSession();
			videoCaptureSession.Refresh();
			int countOpened=_fakeUvcBackend.ListOpened.Count;
			videoCaptureSession.PrepareForLearn();
			Assert.AreEqual(countOpened,_fakeUvcBackend.ListOpened.Count,"No still pin: no reopen.");
			Assert.IsNull(videoCaptureSession.TriggerEngine);
			videoCaptureSession.FinishLearn(false);
			Assert.IsNotNull(videoCaptureSession.TriggerEngine);
			Assert.IsFalse(File.Exists(_path),"Cancel writes nothing.");
		}
		#endregion Settings

		[TestMethod]
		public void Dispose_ReleasesTheCamera_EndCaptureStillSafe() {
			_fakeUvcBackend.AddDevice();
			VideoCaptureSession videoCaptureSession=CreateSession();
			videoCaptureSession.Refresh();
			FakeFrameSource fakeFrameSource=OpenedLast;
			PushFrame(fakeFrameSource,0x20);
			CaptureStart captureStart=videoCaptureSession.BeginCapture(CaptureRequestSource.Button,null);
			Assert.IsTrue(captureStart.IsAccepted);
			//The handler's message box let the patient change: the window closes while the handler still runs.
			videoCaptureSession.Dispose();
			Assert.IsTrue(fakeFrameSource.IsDisposed);
			Assert.IsTrue(videoCaptureSession.Gate.IsClosing);
			Assert.AreEqual(CaptureDropReason.Closing,videoCaptureSession.BeginCapture(CaptureRequestSource.Trigger,null).DropReason);
			Bitmap bitmap=captureStart.Bitmap;
			videoCaptureSession.EndCapture(captureStart);
			TestUtil.AssertThrows<ArgumentException>(() => { int width=bitmap.Width; });
			videoCaptureSession.Dispose();
			TestUtil.AssertThrows<ObjectDisposedException>(() => videoCaptureSession.Refresh());
		}

		///<summary>Opens through a FakeUvcBackend, but every Start fails with the given kind.</summary>
		private sealed class StartFailingBackend:IUvcBackend {
			private readonly FakeUvcBackend _fakeUvcBackend;
			private readonly UvcErrorKind _uvcErrorKind;

			public StartFailingBackend(FakeUvcBackend fakeUvcBackend,UvcErrorKind uvcErrorKind) {
				_fakeUvcBackend=fakeUvcBackend;
				_uvcErrorKind=uvcErrorKind;
			}

			public string Name {
				get {
					return "Fake start failure";
				}
			}

			public List<UvcDeviceInfo> GetDevices() {
				return _fakeUvcBackend.GetDevices();
			}

			public IFrameSource Open(UvcDeviceInfo device) {
				FakeFrameSource fakeFrameSource=(FakeFrameSource)_fakeUvcBackend.Open(device);
				fakeFrameSource.StartErrorKind=_uvcErrorKind;
				return fakeFrameSource;
			}
		}
	}
}
