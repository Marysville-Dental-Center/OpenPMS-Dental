// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using ImagingCapture.Camera;
using ImagingCapture.Photo;
using ImagingCapture.Uvc;
using ImagingCapture.Uvc.DirectShow;
using ImagingCapture.Video;
using ImagingDeviceManager.Config;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>What a camera session leaves for the module to select: the photo mount it filled, or the last quick photo.</summary>
	public sealed class CameraSessionEndedEventArgs:EventArgs {
		public long MountNum { get; set; }
		public long DocNumLast { get; set; }
		public int CountSaved { get; set; }
		///<summary>Photos of this session still in it (not deleted or retaken): 0 lets the module remove a mount the session made.</summary>
		public int CountShotsLeft { get; set; }
		///<summary>Photos that could not be saved and were kept on this computer: their mount must stay, for filing them into their slots.</summary>
		public int CountKept { get; set; }
		///<summary>True when the session ended because the patient changed.</summary>
		public bool IsPatientChange { get; set; }
		///<summary>True when the session ended because the user left the Imaging module: the module is not refreshed.</summary>
		public bool IsModuleLeft { get; set; }
	}

	///<summary>The camera session inside the Imaging module (owner imaging design, camera and video, section 2; IMG-P4b): it covers the module
	///while it runs. A blue banner names the session's patient; the live picture shows what to shoot next ("Next: Upper occlusal (mirror shot)");
	///the right panel shows the photo mount being filled. Capture (button, Space, or the camera's learned button), skip forward and back (arrows),
	///a click on a slot, and End (Esc). Every photo is saved the moment it is taken, into the slot chosen before it was taken. A patient change
	///anywhere ends the session. Camera problems show inside the live picture with one action. Controls are built in code (no designer file).
	///Camera side: VideoCaptureSession, as the Video window uses it.</summary>
	public class ControlCameraSession:UserControl {
		#region Fields
		private readonly IPhotoStore _store;
		private readonly Func<int,int> _funcScale;
		private Patient _patient;
		private Mount _mount;
		private long _categoryDefNum;
		private PhotoSessionModel _model;
		private VideoCaptureSession _videoCaptureSession;
		private int _isFramePosted;
		private bool _isPreviewPaused;
		private bool _isEnded;
		private long _docNumLast;
		///<summary>A small picture of each photo saved in a slot, by MountItemNum, for the mount panel.</summary>
		private readonly Dictionary<long,Bitmap> _dictThumbs=new Dictionary<long,Bitmap>();
		///<summary>The quick photos of this session, newest last.</summary>
		private readonly List<Bitmap> _listThumbsQuick=new List<Bitmap>();
		///<summary>The DocNum of each of _listThumbsQuick.</summary>
		private readonly List<long> _listDocNumsQuick=new List<long>();
		///<summary>Freeze (F, IMG-P4b-2): the frozen picture as the camera gave it (not flipped), saved by the next Capture; null while live.</summary>
		private Bitmap _bitmapFrozen;
		///<summary>True when the live picture on screen is flipped (a mirror slot), so a frozen copy is flipped back before it is saved.</summary>
		private bool _isLiveFlipped;
		///<summary>The next slot when the picture was frozen.</summary>
		private int _idxFrozen=-1;
		private Panel panelFlash;
		private System.Windows.Forms.Timer timerFlash;
		private UI.Button butFreeze;
		private UI.Button butTeeth;
		private ContextMenuStrip contextMenuShot;
		///<summary>The shot the right-click menu acts on.</summary>
		private PhotoShot _shotMenu;
		///<summary>The recording running (IMG-P4c), or null.</summary>
		private VideoRecorder _recorder;
		///<summary>The frame source the recording taps; its tap is cleared when the recording stops.</summary>
		private IFrameSource _frameSourceRecording;
		private string _pathRecording="";
		private DateTime _dateTimeRecordingStarted;
		private string _teethRecording="";
		private Label labelRec;
		private UI.Button butRecord;
		private System.Windows.Forms.Timer timerRec;
		///<summary>The DocNum Ctrl+Z may retake: the photo saved last, once; 0 after it was retaken or another action.</summary>
		private long _docNumUndoable;
		///<summary>A short click, made once (design section 2: a soft click on every shot).</summary>
		private static byte[] _arrayBytesClick;
		///<summary>Idle release (design section 2, "Session timeout"): checks every 30 seconds.</summary>
		private System.Windows.Forms.Timer timerIdle;
		private DateTime _dateTimeActivity=DateTime.Now;
		///<summary>True while the camera is released for being idle; Capture, Space or Wake opens it again.</summary>
		private bool _isAsleep;
		private int _countKept;
		private VideoStatusAction _videoStatusAction;
		private Label labelBanner;
		private Panel panelLive;
		private PictureBox pictureLive;
		private Label labelGuide;
		///<summary>A failure that must not be missed (a photo not saved), over the bottom of the live picture until the next photo is saved.</summary>
		private Label labelAlert;
		private Label labelStatus;
		private UI.Button butStatusAction;
		private Panel panelBar;
		private UI.Button butCapture;
		private UI.Button butBack;
		private UI.Button butSkip;
		private UI.Button butEnd;
		private Label labelInfo;
		private Panel panelRight;
		private Label labelTitle;
		private Label labelProgress;
		private PanelMount panelMount;
		private Label labelHint;
		private Label labelCamera;
		#endregion Fields

		///<summary>Raised once when the session ends (End, Esc, a patient change, or the module closing it). The camera is released before.
		///</summary>
		public event EventHandler<CameraSessionEndedEventArgs> EventEnded;
		///<summary>F11: the module puts the session on the whole monitor, or back into the module (owner design C1).</summary>
		public event EventHandler EventFullScreenToggle;

		public ControlCameraSession(IPhotoStore store,Func<int,int> funcScale) {
			_store=store??throw new ArgumentNullException(nameof(store));
			_funcScale=funcScale??(x => x);
			DoubleBuffered=true;
			BackColor=UiTheme.Surface;
			Font=UiTheme.FontBody;
			BuildControls();
		}

		#region Properties
		public Patient Patient => _patient;
		public Mount Mount => _mount;
		public PhotoSessionModel Model => _model;
		public bool IsEnded => _isEnded;
		public bool IsFrozen => _bitmapFrozen!=null;
		///<summary>Plays a click after each saved photo. A setting turns it off later (IMG-P4e).</summary>
		public bool IsClickOn { get; set; }=true;
		///<summary>Minutes without a photo or a key after which the camera is released (its light goes off). A setting later (IMG-P4e).</summary>
		public int MinutesIdle { get; set; }=10;
		public bool IsAsleep => _isAsleep;
		public bool IsRecording => _recorder!=null;
		///<summary>Shows stored tooth numbers in the office's numbering (Tooth.DisplayRange). Replaceable for pictures made without a database.
		///</summary>
		public Func<string,string> FuncDisplayTeeth { get; set; }=x => Tooth.DisplayRange(x);
		#endregion Properties

		#region Methods - Start and End
		///<summary>Starts a session for the patient: filling mount's photo slots, or quick photos when mount is null (categoryDefNum: where they go).
		///Returns "" or why it cannot start (no photo slot). The camera opens after the control is shown.</summary>
		public string Start(Patient patient,Mount mount,long categoryDefNum) {
			if(patient==null || patient.PatNum==0) {
				return Lan.g(this,"Select a patient first.");
			}
			_patient=patient;
			_categoryDefNum=categoryDefNum;
			if(mount==null) {
				_model=PhotoSessionModel.CreateQuickPhotos();
			}
			else {
				PhotoMountState photoMountState=_store.GetMountState(mount.MountNum);
				if(photoMountState==null || photoMountState.ListSlots.Count==0) {
					return Lan.g(this,"This mount has no photo slots. Photos never go into an X-ray mount.");
				}
				_mount=photoMountState.Mount;
				_categoryDefNum=_mount.DocCategory;
				_model=new PhotoSessionModel(photoMountState.ListSlots,_mount.FlipOnAcquire);
			}
			labelBanner.Text=GetBannerText();
			labelTitle.Text=_mount==null ? Lan.g(this,"Quick photos") : _mount.Description;
			RefreshGuide();
			LocalCaptureConfig localCaptureConfig=LocalCaptureConfig.Load();
			_videoCaptureSession=new VideoCaptureSession(new DirectShowUvcBackend(),localCaptureConfig,patient.PatNum);
			_videoCaptureSession.FuncIsWindowEnabled=IsEnabledForCapture;
			_videoCaptureSession.FrameReady+=videoCaptureSession_FrameReady;
			_videoCaptureSession.CameraError+=videoCaptureSession_CameraError;
			_videoCaptureSession.TriggerPressed+=videoCaptureSession_TriggerPressed;
			_videoCaptureSession.LevelChanged+=videoCaptureSession_LevelChanged;
			ODEvent.Fired+=ODEvent_Fired;
			VideoFiles.RecoverStale();//a recording left by a program that stopped goes to the unsaved photos
			ShowStatus(VideoStatusText.Starting());
			_dateTimeActivity=DateTime.Now;
			timerIdle.Start();
			return "";
		}

		///<summary>Opens the camera; call after the control is shown, so "Starting the camera" is painted first.</summary>
		public void OpenCamera() {
			RefreshCamera();
			Focus();
		}

		///<summary>Ends the session: releases the camera and raises EventEnded once. Everything is saved already.</summary>
		public void End(bool isPatientChange=false,bool isModuleLeft=false) {
			if(_isEnded) {
				return;
			}
			_isEnded=true;
			ReleaseCamera();
			EventEnded?.Invoke(this,new CameraSessionEndedEventArgs {
				MountNum=_mount?.MountNum??0,
				DocNumLast=_docNumLast,
				CountSaved=_model?.CountSaved??0,
				CountShotsLeft=_model?.ListShots.Count??0,
				CountKept=_countKept,
				IsPatientChange=isPatientChange,
				IsModuleLeft=isModuleLeft,
			});
		}

		private void ReleaseCamera() {
			StopRecording();//the clip is completed and saved to the session's patient first (design section 4)
			timerIdle?.Stop();
			ODEvent.Fired-=ODEvent_Fired;
			VideoCaptureSession videoCaptureSession=_videoCaptureSession;
			_videoCaptureSession=null;
			if(videoCaptureSession!=null) {
				videoCaptureSession.Gate.BeginClosing();
				videoCaptureSession.FrameReady-=videoCaptureSession_FrameReady;
				videoCaptureSession.CameraError-=videoCaptureSession_CameraError;
				videoCaptureSession.TriggerPressed-=videoCaptureSession_TriggerPressed;
				videoCaptureSession.LevelChanged-=videoCaptureSession_LevelChanged;
				videoCaptureSession.Dispose();
			}
			if(!IsDisposed && pictureLive!=null) {
				SetLiveImage(null);
			}
		}

		protected override void Dispose(bool disposing) {
			if(disposing) {
				ReleaseCamera();
				foreach(Bitmap bitmap in _dictThumbs.Values) {
					bitmap.Dispose();
				}
				_dictThumbs.Clear();
				foreach(Bitmap bitmap in _listThumbsQuick) {
					bitmap.Dispose();
				}
				_listThumbsQuick.Clear();
				_listDocNumsQuick.Clear();
				_bitmapFrozen?.Dispose();
				_bitmapFrozen=null;
				timerFlash?.Dispose();
				timerIdle?.Dispose();
				timerRec?.Dispose();
				contextMenuShot?.Dispose();
			}
			base.Dispose(disposing);
		}
		#endregion Methods - Start and End

		#region Methods - Event Handlers
		///<summary>Space, the arrows and Esc work wherever the focus is inside the session.</summary>
		protected override bool ProcessCmdKey(ref Message msg,Keys keyData) {
			_dateTimeActivity=DateTime.Now;
			switch(keyData) {
				case Keys.F11:
					EventFullScreenToggle?.Invoke(this,EventArgs.Empty);
					return true;
				case Keys.R:
					if((msg.LParam.ToInt64() & 0x40000000)==0) {//a held R does not start and stop again
						ToggleRecording();
					}
					return true;
				case Keys.P:
					if((msg.LParam.ToInt64() & 0x40000000)==0) {
						TogglePause();
					}
					return true;
				case Keys.Space:
					//A held Space repeats: only the first press takes a photo, or one held key would fill the whole mount.
					if((msg.LParam.ToInt64() & 0x40000000)==0) {
						RequestCapture(CaptureRequestSource.SpaceInWindow,null);
					}
					return true;
				case Keys.Right:
					Skip(isForward:true);
					return true;
				case Keys.Left:
					Skip(isForward:false);
					return true;
				case Keys.Escape:
					if(IsFrozen) {
						Unfreeze();//Esc goes back to live first (design section 2)
						return true;
					}
					End();
					return true;
				case Keys.F:
					ToggleFreeze();
					return true;
				case Keys.T:
					PickTeethNext();
					return true;
				case Keys.Control|Keys.Z:
					RetakeLast();
					return true;
			}
			return base.ProcessCmdKey(ref msg,keyData);
		}

		protected override void OnLayout(LayoutEventArgs e) {
			base.OnLayout(e);
			LayoutControls();
		}

		///<summary>ODEvent can be fired on any thread. A patient change closes the gate at once, so no photo is taken from here on, and ends the
		///session on the UI thread; photos already taken stay with the session's patient.</summary>
		private void ODEvent_Fired(ODEventArgs e) {
			VideoCaptureSession videoCaptureSession=_videoCaptureSession;
			if(videoCaptureSession==null || !videoCaptureSession.PatientBinding.IsPatientChange(e)) {
				return;
			}
			videoCaptureSession.Gate.BeginClosing();
			try {
				BeginInvoke((Action)(() => End(isPatientChange:true)));
			}
			catch(Exception ex) {
				ex.DoNothing();//already closing
			}
		}

		private void videoCaptureSession_FrameReady(object sender,EventArgs e) {
			if(Interlocked.Exchange(ref _isFramePosted,1)==1) {
				return;
			}
			try {
				BeginInvoke((Action)ShowNewFrame);
			}
			catch(Exception ex) {
				ex.DoNothing();//the handle is gone
				Interlocked.Exchange(ref _isFramePosted,0);
			}
		}

		private void videoCaptureSession_CameraError(object sender,UvcErrorEventArgs e) {
			try {
				BeginInvoke((Action)(() => HandleCameraError(sender,e)));
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
		}

		private void videoCaptureSession_LevelChanged(object sender,LevelChangedEventArgs e) {
			_isPreviewPaused=e.IsPausePreviewActive;
			ShowStatus(_videoCaptureSession?.Status);
		}

		private void videoCaptureSession_TriggerPressed(object sender,TriggerPressEventArgs e) {
			RequestCapture(CaptureRequestSource.Trigger,e.StillFrame);
		}

		private void butCapture_Click(object sender,EventArgs e) {
			RequestCapture(CaptureRequestSource.Button,null);
		}

		private void butStatusAction_Click(object sender,EventArgs e) {
			RefreshCamera();
		}

		private void panelMount_MouseClick(object sender,MouseEventArgs e) {
			_dateTimeActivity=DateTime.Now;
			if(_model==null) {
				return;
			}
			if(e.Button==MouseButtons.Right) {
				ShowShotMenu(e.Location);
				return;
			}
			int idx=panelMount.HitTestSlot(e.Location);
			if(idx<0) {
				return;
			}
			if(!_model.SelectSlot(idx)) {
				ShowInfo(Lan.g(this,"That slot has a photo already."));
				return;
			}
			UnfreezeIfSlotChanged();
			ShowInfo("");
			RefreshGuide();
		}
		#endregion Methods - Event Handlers

		#region Methods - Capture
		///<summary>Takes the photo: the slot is planned first, then the camera's frame is saved into it at once. The gate drops requests while a
		///photo is being saved, while the session ends, without a camera, and while the main window is disabled under another window.</summary>
		public void RequestCapture(CaptureRequestSource captureRequestSource,UvcFrame frameStill) {
			VideoCaptureSession videoCaptureSession=_videoCaptureSession;
			if(videoCaptureSession==null || _model==null || _isEnded) {
				return;
			}
			_dateTimeActivity=DateTime.Now;
			if(_isAsleep) {
				RefreshCamera();//one press wakes the camera; the next takes the photo
				return;
			}
			PhotoShotPlan photoShotPlan=_model.PlanShot();
			if(photoShotPlan==null) {
				ShowInfo(Lan.g(this,"Every slot is filled. End the session (Esc)."));
				return;
			}
			bool isFrozen=IsFrozen;
			CaptureStart captureStart=isFrozen ? videoCaptureSession.BeginCaptureFrozen(captureRequestSource,_bitmapFrozen)
				: videoCaptureSession.BeginCapture(captureRequestSource,frameStill);
			if(!captureStart.IsAccepted) {
				ShowInfo(Lan.g(this,CaptureGate.GetDropText(captureStart.DropReason)));
				return;
			}
			try {
				PhotoSaveResult photoSaveResult=PhotoSaver.Save(_store,captureStart.Bitmap,photoShotPlan,_patient.PatNum,_categoryDefNum);
				if(photoSaveResult.Document==null) {
					KeepUnsaved(captureStart.Bitmap,photoShotPlan,photoSaveResult.Error);
					return;
				}
				_docNumLast=photoSaveResult.Document.DocNum;
				if(!photoSaveResult.IsPlaced) {
					//Saved, but unmounted: the slot is still empty, so it stays the next one and the photo can be retaken.
					ShowInfo(photoSaveResult.Error);
					return;
				}
				_model.MarkSaved(photoShotPlan,photoSaveResult.Document.DocNum);
				_docNumUndoable=photoSaveResult.Document.DocNum;
				AddThumb(photoShotPlan,captureStart.Bitmap,photoSaveResult.Document.DocNum);
				if(isFrozen) {
					Unfreeze();
				}
				Flash();
				string textSaved=photoShotPlan.GuideLabel!="" ? Lan.g(this,"Saved")+": "+photoShotPlan.GuideLabel : Lan.g(this,"Photo saved");
				ShowInfo(textSaved);
				ShowAlert("");
				RefreshGuide();
			}
			finally {
				videoCaptureSession.EndCapture(captureStart);
			}
		}

		private void Skip(bool isForward) {
			_dateTimeActivity=DateTime.Now;
			if(_model==null || _model.IsQuickPhotos) {
				return;
			}
			bool isMoved=isForward ? _model.SkipForward() : _model.SkipBack();
			if(isMoved) {
				UnfreezeIfSlotChanged();
			}
			ShowInfo(isMoved ? "" : Lan.g(this,"No other empty slot."));
			RefreshGuide();
		}

		///<summary>The quick photos' small pictures: the newest 12.</summary>
		private void AddQuickThumb(Bitmap bitmapThumb,long docNum) {
			_listThumbsQuick.Add(bitmapThumb);
			_listDocNumsQuick.Add(docNum);
			while(_listThumbsQuick.Count>12) {
				_listThumbsQuick[0].Dispose();
				_listThumbsQuick.RemoveAt(0);
				_listDocNumsQuick.RemoveAt(0);
			}
		}

		///<summary>A small copy of the saved photo as it will show: flipped for a mirror shot and rotated as the slot says.</summary>
		private void AddThumb(PhotoShotPlan photoShotPlan,Bitmap bitmap,long docNum) {
			Bitmap bitmapThumb=MakeThumb(bitmap,photoShotPlan.IsFlipped,photoShotPlan.DegreesRotated,_funcScale(160));
			if(bitmapThumb==null) {
				return;
			}
			if(photoShotPlan.MountItemNum==0) {
				AddQuickThumb(bitmapThumb,docNum);
				return;
			}
			if(_dictThumbs.TryGetValue(photoShotPlan.MountItemNum,out Bitmap bitmapOld)) {
				bitmapOld.Dispose();
			}
			_dictThumbs[photoShotPlan.MountItemNum]=bitmapThumb;
		}

		///<summary>A copy no wider or taller than sizeMax, flipped and rotated. Null when the copy cannot be made.</summary>
		public static Bitmap MakeThumb(Bitmap bitmap,bool isFlipped,int degreesRotated,int sizeMax) {
			if(bitmap==null || bitmap.Width<1 || bitmap.Height<1) {
				return null;
			}
			try {
				float scale=Math.Min(1f,Math.Min((float)sizeMax/bitmap.Width,(float)sizeMax/bitmap.Height));
				Bitmap bitmapThumb=new Bitmap(Math.Max(1,(int)(bitmap.Width*scale)),Math.Max(1,(int)(bitmap.Height*scale)));
				using(Graphics g=Graphics.FromImage(bitmapThumb)) {
					g.InterpolationMode=InterpolationMode.HighQualityBilinear;
					g.DrawImage(bitmap,0,0,bitmapThumb.Width,bitmapThumb.Height);
				}
				if(isFlipped) {
					bitmapThumb.RotateFlip(RotateFlipType.RotateNoneFlipX);
				}
				switch(((degreesRotated%360)+360)%360) {
					case 90:
						bitmapThumb.RotateFlip(RotateFlipType.Rotate90FlipNone);
						break;
					case 180:
						bitmapThumb.RotateFlip(RotateFlipType.Rotate180FlipNone);
						break;
					case 270:
						bitmapThumb.RotateFlip(RotateFlipType.Rotate270FlipNone);
						break;
				}
				return bitmapThumb;
			}
			catch(Exception ex) {
				ex.DoNothing();//out of memory for a thumbnail: the photo is saved anyway
				return null;
			}
		}
		#endregion Methods - Capture

		#region Methods - Video recording (IMG-P4c)
		///<summary>R: starts or stops a recording. Needs the live picture and 2 GB free on this computer.</summary>
		private void ToggleRecording() {
			_dateTimeActivity=DateTime.Now;
			if(IsRecording) {
				StopRecording();
				return;
			}
			if(_isAsleep || _videoCaptureSession==null || !_videoCaptureSession.IsStreaming || _videoCaptureSession.FrameSource==null) {
				ShowInfo(Lan.g(this,"Recording needs the live picture."));
				return;
			}
			if(!VideoFiles.IsEnoughSpace(VideoFiles.GetBytesFree(VideoFiles.GetRecordingFolder()??""))) {
				ShowAlert(Lan.g(this,"Video is off until there is at least 2 GB free on this computer. Photos still work."));
				return;
			}
			PhotoUnsaved photoUnsaved=new PhotoUnsaved {
				PatNum=_patient.PatNum,
				PatientName=_patient.GetNameLF(),
				MountNum=_mount?.MountNum??0,
				Slot=PhotoSessionModel.VideoLabel,
				ToothNumbers=_model.GetTeethNext(),
				CategoryDefNum=_categoryDefNum,
				DateTimeTaken=DateTime.Now,
				Reason="Recording",
			};
			string error;
			string path=VideoFiles.BeginRecordingFile(photoUnsaved,out error);
			if(path==null) {
				ShowAlert(Lan.g(this,"The video could not be started")+": "+error);
				return;
			}
			double framesPerSecond=_videoCaptureSession.ModeChoice?.Mode?.MaxFramesPerSecond??30;
			VideoRecorder recorder=new VideoRecorder((width,height) => Mp4Writer.Create(path,width,height,framesPerSecond),framesPerSecond);
			recorder.Start();
			_recorder=recorder;
			_pathRecording=path;
			_dateTimeRecordingStarted=photoUnsaved.DateTimeTaken;
			_teethRecording=photoUnsaved.ToothNumbers;
			_frameSourceRecording=_videoCaptureSession.FrameSource;
			_frameSourceRecording.FrameTap=recorder.OfferFrame;
			timerRec.Start();
			ShowAlert("");
			ShowInfo(Lan.g(this,"Recording. R stops, P pauses, Space still takes a photo."));
			RefreshRecording();
		}

		private void TogglePause() {
			_dateTimeActivity=DateTime.Now;
			VideoRecorder recorder=_recorder;
			if(recorder==null) {
				return;
			}
			if(recorder.IsPaused) {
				recorder.Resume();
			}
			else {
				recorder.Pause();
			}
			RefreshRecording();
		}

		///<summary>Stops the recording, completes the file and imports it into the session patient's images (unmounted, with the teeth and the
		///time it started). When the import fails the file is kept on this computer for Camera, Unsaved photos. Safe to call when not
		///recording.</summary>
		private void StopRecording() {
			VideoRecorder recorder=_recorder;
			if(recorder==null) {
				return;
			}
			_recorder=null;
			timerRec?.Stop();
			if(_frameSourceRecording!=null) {
				_frameSourceRecording.FrameTap=null;
				_frameSourceRecording=null;
			}
			VideoRecordingResult result;
			Cursor cursorOld=Cursor.Current;
			Cursor.Current=Cursors.WaitCursor;
			try {
				result=recorder.Stop();
			}
			finally {
				recorder.Dispose();
				Cursor.Current=cursorOld;
			}
			if(IsDisposed) {
				return;
			}
			RefreshRecording();
			if(result.IsTimedOut) {
				//the file may still be open: it is not imported now; the next session moves it to the unsaved photos
				ShowAlert(Lan.g(this,"The video is still being finished. It will be under Camera, Unsaved photos after the next camera session starts."));
				return;
			}
			if(result.FramesWritten==0) {
				VideoFiles.Delete(_pathRecording);
				ShowInfo(Lan.g(this,"Nothing was recorded.")+(result.Error=="" ? "" : " "+result.Error));
				return;
			}
			string error;
			Document document=VideoFiles.Import(_store,_pathRecording,_patient.PatNum,_categoryDefNum,_teethRecording,_dateTimeRecordingStarted,
				result.Duration,out error);
			if(document==null) {
				string errorMove;
				string pathKept=VideoFiles.MoveToUnsaved(_pathRecording,error,out errorMove,duration:result.Duration);
				if(pathKept==null) {
					ShowAlert(error+" "+Lan.g(this,"The file stays in")+" "+_pathRecording+": "+errorMove);
					return;
				}
				_countKept++;
				ShowAlert(Lan.g(this,"Video could not be saved. It is kept on this computer; Camera, Unsaved photos files it later.")+"\r\n"+error);
				return;
			}
			VideoFiles.Delete(_pathRecording);
			_docNumLast=document.DocNum;
			_model.AddVideo(document.DocNum);
			if(result.FrameFirst!=null) {
				//IMG-P4d: the tree's picture of the video, its first frame with a play mark and its length
				try {
					string patFolder=ImageStore.GetPatientFolder(_patient,ImageStore.GetPreferredAtoZpath());
					using(Bitmap bitmapFrame=result.FrameFirst.ToBitmap()) {
						VideoMedia.SaveThumbnail(document,patFolder,bitmapFrame,(int)result.Duration.TotalMilliseconds);
					}
				}
				catch(Exception ex) {
					ex.DoNothing();//the player saves it when the video is first shown
				}
			}
			if(_model.IsQuickPhotos && result.FrameFirst!=null) {
				using(Bitmap bitmapFirst=result.FrameFirst.ToBitmap()) {
					Bitmap bitmapThumb=MakeThumb(bitmapFirst,false,0,_funcScale(160));
					if(bitmapThumb!=null) {
						MarkAsVideo(bitmapThumb);
						AddQuickThumb(bitmapThumb,document.DocNum);
					}
				}
			}
			string text=Lan.g(this,"Video saved")+" ("+VideoFiles.FormatDuration(result.Duration)+")";
			if(result.IsLimitReached) {
				text+=". "+Lan.g(this,"It stopped at the limit of")+" "+VideoFiles.FormatDuration(TimeSpan.FromSeconds(recorder.LimitSeconds))+".";
			}
			if(result.FramesDropped>0) {
				text+=" "+result.FramesDropped+" "+Lan.g(this,"frames left out (the computer was busy).");
			}
			ShowInfo(text);
			if(error!="" || result.Error!="") {
				ShowAlert((error+" "+result.Error).Trim());
			}
			Flash();
			RefreshGuide();
		}

		///<summary>A play mark on a video's small picture.</summary>
		private static void MarkAsVideo(Bitmap bitmap) {
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.SmoothingMode=SmoothingMode.AntiAlias;
				int size=Math.Max(10,Math.Min(bitmap.Width,bitmap.Height)/3);
				Point pointCenter=new Point(bitmap.Width/2,bitmap.Height/2);
				using(Brush brushBack=new SolidBrush(Color.FromArgb(160,0,0,0))) {
					g.FillEllipse(brushBack,pointCenter.X-size/2,pointCenter.Y-size/2,size,size);
				}
				Point[] arrayPoints={ new Point(pointCenter.X-size/6,pointCenter.Y-size/4),new Point(pointCenter.X-size/6,pointCenter.Y+size/4),
					new Point(pointCenter.X+size/4,pointCenter.Y) };
				g.FillPolygon(Brushes.White,arrayPoints);
			}
		}

		private void timerRec_Tick(object sender,EventArgs e) {
			VideoRecorder recorder=_recorder;
			if(recorder==null) {
				timerRec.Stop();
				return;
			}
			_dateTimeActivity=DateTime.Now;//recording is activity
			if(recorder.IsLimitReached || recorder.IsFailed) {
				StopRecording();//a failed writer (no encoder, disk full) ends it at once and says why
				return;
			}
			RefreshRecording();
		}

		///<summary>The red REC line over the live picture and the Record button's text.</summary>
		private void RefreshRecording() {
			if(labelRec==null) {
				return;
			}
			VideoRecorder recorder=_recorder;
			butRecord.Text=recorder==null ? Lan.g(this,"Record (R)") : Lan.g(this,"Stop recording (R)");
			if(recorder==null) {
				labelRec.Visible=false;
				LayoutControls();
				return;
			}
			string limit=VideoFiles.FormatDuration(TimeSpan.FromSeconds(recorder.LimitSeconds));
			string text=(recorder.IsPaused ? Lan.g(this,"PAUSED") : "\u25CF "+Lan.g(this,"REC"))+" "+VideoFiles.FormatDuration(recorder.Elapsed)+" / "+limit;
			if(recorder.IsNearLimit && !recorder.IsPaused) {
				text+="  "+Lan.g(this,"stops at")+" "+limit;
			}
			labelRec.Text=text;
			labelRec.Visible=true;
			LayoutControls();
		}
		#endregion Methods - Video recording (IMG-P4c)

		#region Methods - Unsaved photos and idle release
		///<summary>A photo whose save failed is kept on this computer (PhotoQuarantine) for Camera, Unsaved photos; the slot stays empty and next.
		///</summary>
		private void KeepUnsaved(Bitmap bitmap,PhotoShotPlan photoShotPlan,string errorSave) {
			PhotoUnsaved photoUnsaved=new PhotoUnsaved {
				PatNum=_patient.PatNum,
				PatientName=_patient.GetNameLF(),
				MountNum=_mount?.MountNum??0,
				MountItemNum=photoShotPlan.MountItemNum,
				Slot=photoShotPlan.GuideLabel,
				IsFlipped=photoShotPlan.IsFlipped,
				DegreesRotated=photoShotPlan.DegreesRotated,
				ToothNumbers=photoShotPlan.ToothNumbers,
				CategoryDefNum=_categoryDefNum,
				DateTimeTaken=DateTime.Now,
				Reason=errorSave,
			};
			string error;
			string path=PhotoQuarantine.Write(bitmap,photoUnsaved,out error);
			if(path==null) {
				ShowAlert(errorSave+" "+Lan.g(this,"It could not be kept on this computer either")+": "+error);
				ShowInfo("");
				return;
			}
			_countKept++;
			ShowAlert(Lan.g(this,"Photo could not be saved. It is kept on this computer; Camera, Unsaved photos files it later.")+"\r\n"+errorSave);
			ShowInfo("");
		}

		private void timerIdle_Tick(object sender,EventArgs e) {
			if(_isAsleep || _isEnded || _videoCaptureSession==null || !_videoCaptureSession.IsStreaming) {
				return;
			}
			if(!IsEnabledForCapture() || IsRecording) {
				_dateTimeActivity=DateTime.Now;//a window over the session (the tooth picker, a question) is someone at work
				return;
			}
			if((DateTime.Now-_dateTimeActivity).TotalMinutes<MinutesIdle) {
				return;
			}
			Sleep();
		}

		///<summary>Releases the camera after MinutesIdle without a photo or a key (design section 2): its light goes off. Capture, Space or Wake
		///opens it again. The session and its patient stay.</summary>
		public void Sleep() {
			if(_videoCaptureSession==null || _isAsleep) {
				return;
			}
			Unfreeze();
			_videoCaptureSession.CloseCamera();
			_isAsleep=true;
			SetLiveImage(null);
			labelStatus.Text=Lan.g(this,"The camera was released after")+" "+MinutesIdle+" "+Lan.g(this,"minutes without a photo (its light is off).")+"\r\n"
				+Lan.g(this,"Press Capture or Space to wake it.");
			labelStatus.Visible=true;
			butStatusAction.Text=Lan.g(this,"Wake");
			butStatusAction.Visible=true;
			butCapture.Enabled=true;//Capture wakes it
			LayoutControls();
		}
		#endregion Methods - Unsaved photos and idle release

		#region Methods - Freeze, Teeth, Retake, Shot menu
		///<summary>F: freezes the picture on screen (yellow border); Capture saves it, F or Esc goes back to live. Not while the camera has no
		///picture.</summary>
		private void ToggleFreeze() {
			_dateTimeActivity=DateTime.Now;
			if(IsFrozen) {
				Unfreeze();
				return;
			}
			if(pictureLive.Image==null || _videoCaptureSession==null || !_videoCaptureSession.IsStreaming) {
				ShowInfo(Lan.g(this,"No picture to freeze."));
				return;
			}
			try {
				_bitmapFrozen=new Bitmap(pictureLive.Image);
				if(_isLiveFlipped) {
					_bitmapFrozen.RotateFlip(RotateFlipType.RotateNoneFlipX);//saved as the camera gave it; the slot's flip is set on the document
				}
			}
			catch(Exception ex) {
				ex.DoNothing();//out of memory: stay live
				_bitmapFrozen=null;
				return;
			}
			_idxFrozen=_model?.IdxNext??-1;
			ShowInfo(Lan.g(this,"Frozen: Capture saves it, F goes live."));
			RefreshGuide();
		}

		private void Unfreeze() {
			if(_bitmapFrozen==null) {
				return;
			}
			_bitmapFrozen.Dispose();
			_bitmapFrozen=null;
			RefreshGuide();
		}

		///<summary>T: the teeth of the next photo, with the slot's teeth offered (the tooth picker of IMG-P2).</summary>
		private void PickTeethNext() {
			_dateTimeActivity=DateTime.Now;
			if(_model==null || _isEnded || (!_model.IsQuickPhotos && _model.SlotNext==null)) {
				return;
			}
			ToothNumberingNomenclature toothNumberingNomenclature=(ToothNumberingNomenclature)PrefC.GetInt(PrefName.UseInternationalToothNumbers);
			using FormToothPicker formToothPicker=new FormToothPicker(_model.GetTeethNext(),_model.SlotNext?.ToothNumbers,toothNumberingNomenclature);
			if(formToothPicker.ShowDialog()!=DialogResult.OK) {
				return;
			}
			_model.SetTeethNext(formToothPicker.ToothNumbers);
			ShowInfo("");
			RefreshGuide();
		}

		///<summary>Ctrl+Z: retakes the last photo of a photo mount.</summary>
		private void RetakeLast() {
			if(_model==null || _model.IsQuickPhotos) {
				ShowInfo(Lan.g(this,"Right-click a photo to delete it."));
				return;
			}
			PhotoShot shot=_model.ListShots.LastOrDefault(x => x.IdxSlot>=0 && x.DocNum==_docNumUndoable && _docNumUndoable!=0);
			if(shot==null) {
				ShowInfo(Lan.g(this,"Ctrl+Z retakes the photo just taken. Right-click a photo to retake an earlier one."));
				return;
			}
			Retake(shot);
		}

		///<summary>Retake (design section 2; as the X-ray session's Retake, design 7.11): the photo moves out of its slot to an unmounted item
		///"Retake: slot", nothing is deleted, and the slot is the next one. Needs Image Delete, as the design says for Retake and Delete.</summary>
		private void Retake(PhotoShot shot) {
			if(!Security.IsAuthorized(EnumPermType.ImageDelete,suppressMessage:true)) {
				ShowInfo(Lan.g(this,"Retake needs the Image Delete permission."));
				return;
			}
			Document document=_store.GetDocument(shot.DocNum);
			string slotLabel=shot.IdxSlot>=0 && shot.IdxSlot<_model.ListSlots.Count ? _model.ListSlots[shot.IdxSlot].GetLabel() : shot.GuideLabel;
			if(document!=null) {
				try {
					_store.MoveToRetake(document,_mount.MountNum,slotLabel,_patient.PatNum);
				}
				catch(Exception ex) {
					ShowInfo(Lan.g(this,"Could not retake")+" "+slotLabel+": "+ex.Message);
					return;
				}
			}
			RemoveThumb(shot);
			_model.RemoveShot(shot);
			_docNumUndoable=0;
			UnfreezeIfSlotChanged();
			ShowInfo(document==null ? Lan.g(this,"Retake")+": "+slotLabel+"."
				: Lan.g(this,"Retake")+": "+slotLabel+". "+Lan.g(this,"The earlier photo is kept unmounted in this mount."));
			RefreshGuide();
		}

		private void ShowShotMenu(Point point) {
			_dateTimeActivity=DateTime.Now;
			PhotoShot shot=null;
			if(_model.IsQuickPhotos) {
				int idxQuick=panelMount.HitTestQuick(point);
				if(idxQuick>=0 && idxQuick<_listDocNumsQuick.Count) {
					shot=_model.ListShots.FirstOrDefault(x => x.DocNum==_listDocNumsQuick[idxQuick]);
				}
			}
			else {
				int idx=panelMount.HitTestSlot(point);
				if(idx<0) {
					return;
				}
				shot=_model.GetShotInSlot(idx);
			}
			if(shot==null) {
				ShowInfo(Lan.g(this,"The right-click menu works on photos taken in this session."));
				return;
			}
			_shotMenu=shot;
			contextMenuShot.Items[0].Visible=!_model.IsQuickPhotos;//Retake: a slot only
			contextMenuShot.Items[2].Visible=!shot.IsVideo;//Rotate: photos only
			contextMenuShot.Show(panelMount,point);
		}

		///<summary>Teeth of a photo taken in this session, as the module's Teeth (ImageEdit, logged).</summary>
		private void EditTeethShot(PhotoShot shot) {
			Document document=_store.GetDocument(shot.DocNum);
			if(document==null || !ImageSignaturePermissions.IsAuthorizedToEdit(document)) {
				return;
			}
			ToothNumberingNomenclature toothNumberingNomenclature=(ToothNumberingNomenclature)PrefC.GetInt(PrefName.UseInternationalToothNumbers);
			string toothNumbersSlot=shot.IdxSlot>=0 && shot.IdxSlot<_model.ListSlots.Count ? _model.ListSlots[shot.IdxSlot].ToothNumbers : null;
			using FormToothPicker formToothPicker=new FormToothPicker(document.ToothNumbers,toothNumbersSlot,toothNumberingNomenclature);
			if(formToothPicker.ShowDialog()!=DialogResult.OK || formToothPicker.ToothNumbers==document.ToothNumbers) {
				return;
			}
			Document documentOld=document.Copy();
			document.ToothNumbers=formToothPicker.ToothNumbers;
			try {
				_store.UpdateDocument(document,documentOld);
				_store.LogEdit(_patient.PatNum,"Document Edited: "+document.FileName+" teeth changed from '"+Tooth.DisplayRange(documentOld.ToothNumbers)
					+"' to '"+Tooth.DisplayRange(document.ToothNumbers)+"'",document.DocNum);
			}
			catch(Exception ex) {
				ShowInfo(Lan.g(this,"Could not change the teeth")+": "+ex.Message);
				return;
			}
			ShowInfo(Lan.g(this,"Teeth changed."));
		}

		///<summary>Rotates a photo of this session 90 degrees clockwise (a display setting, as the module's Rotate Right; ImageEdit, logged).</summary>
		private void RotateShot(PhotoShot shot) {
			Document document=_store.GetDocument(shot.DocNum);
			if(document==null || !ImageSignaturePermissions.IsAuthorizedToEdit(document)) {
				return;
			}
			Document documentOld=document.Copy();
			document.DegreesRotated=(((document.DegreesRotated%360)+360)%360+90)%360;
			try {
				_store.UpdateDocument(document,documentOld);
				_store.LogEdit(_patient.PatNum,"Document Edited: "+document.FileName+" rotated to "+document.DegreesRotated+" degrees",document.DocNum);
			}
			catch(Exception ex) {
				ShowInfo(Lan.g(this,"Could not rotate")+": "+ex.Message);
				return;
			}
			Bitmap bitmapThumb=GetThumb(shot);
			bitmapThumb?.RotateFlip(RotateFlipType.Rotate90FlipNone);
			panelMount.Invalidate();
			ShowInfo(Lan.g(this,"Rotated."));
		}

		///<summary>Deletes a photo of this session after asking (Image Delete; ImageStore.DeleteDocuments). Its slot is empty and next again.</summary>
		private void DeleteShot(PhotoShot shot) {
			Document document=_store.GetDocument(shot.DocNum);
			if(document==null) {
				RemoveThumb(shot);
				_model.RemoveShot(shot);
				AfterShotRemoved(shot);
				RefreshGuide();
				return;
			}
			if(!ImageSignaturePermissions.IsAuthorizedToDelete(document)) {
				return;
			}
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Delete this photo?")) {
				return;
			}
			try {
				_store.DeleteDocument(document,_patient.PatNum);
			}
			catch(Exception ex) {
				ShowInfo(Lan.g(this,"Could not delete the photo")+": "+ex.Message);
				return;
			}
			RemoveThumb(shot);
			_model.RemoveShot(shot);
			AfterShotRemoved(shot);
			ShowInfo(Lan.g(this,"Photo deleted."));
			RefreshGuide();
		}

		///<summary>After a delete: the module selects a photo that still exists when the session ends, and Ctrl+Z cannot reach a deleted one.
		///</summary>
		private void AfterShotRemoved(PhotoShot shot) {
			if(_docNumLast==shot.DocNum) {
				_docNumLast=_model.ListShots.LastOrDefault()?.DocNum??0;
			}
			if(_docNumUndoable==shot.DocNum) {
				_docNumUndoable=0;
			}
			UnfreezeIfSlotChanged();
		}

		///<summary>A frozen picture belongs to the slot it was frozen for (its flip shows on screen): when the next slot changes, the session goes
		///back to live.</summary>
		private void UnfreezeIfSlotChanged() {
			if(IsFrozen && _model!=null && _model.IdxNext!=_idxFrozen) {
				Unfreeze();
			}
		}

		private Bitmap GetThumb(PhotoShot shot) {
			if(shot.MountItemNum!=0) {
				return _dictThumbs.TryGetValue(shot.MountItemNum,out Bitmap bitmap) ? bitmap : null;
			}
			int idx=_listDocNumsQuick.IndexOf(shot.DocNum);
			return idx>=0 ? _listThumbsQuick[idx] : null;
		}

		private void RemoveThumb(PhotoShot shot) {
			if(shot.MountItemNum!=0) {
				if(_dictThumbs.TryGetValue(shot.MountItemNum,out Bitmap bitmap)) {
					bitmap.Dispose();
					_dictThumbs.Remove(shot.MountItemNum);
				}
				return;
			}
			int idx=_listDocNumsQuick.IndexOf(shot.DocNum);
			if(idx>=0) {
				_listThumbsQuick[idx].Dispose();
				_listThumbsQuick.RemoveAt(idx);
				_listDocNumsQuick.RemoveAt(idx);
			}
		}

		///<summary>The confirmation of every shot (design section 2): a short white flash over the live picture and a soft click.</summary>
		private void Flash() {
			panelFlash.Bounds=pictureLive.Bounds;
			panelFlash.Visible=true;
			panelFlash.BringToFront();
			timerFlash.Stop();
			timerFlash.Start();
			if(IsClickOn) {
				PlayClick();
			}
		}

		private static void PlayClick() {
			try {
				if(_arrayBytesClick==null) {
					_arrayBytesClick=PhotoSounds.MakeClickWav();
				}
				byte[] arrayBytes=_arrayBytesClick;
				//PlaySync on a pool thread: the player and its bytes live until the 30 ms click has played (an asynchronous Play from memory
				//could outlive a disposed player).
				System.Threading.Tasks.Task.Run(() => {
					try {
						using MemoryStream memoryStream=new MemoryStream(arrayBytes);
						using System.Media.SoundPlayer soundPlayer=new System.Media.SoundPlayer(memoryStream);
						soundPlayer.PlaySync();
					}
					catch(Exception exPlay) {
						exPlay.DoNothing();//no sound device
					}
				});
			}
			catch(Exception ex) {
				ex.DoNothing();//no sound device: the flash still shows
			}
		}

		#endregion Methods - Freeze, Teeth, Retake, Shot menu

		#region Methods - Camera
		private void RefreshCamera() {
			if(_videoCaptureSession==null || _videoCaptureSession.Gate.IsClosing) {
				return;
			}
			StopRecording();//the camera is opened anew: the recording ends with what it has
			Unfreeze();
			_isAsleep=false;
			_dateTimeActivity=DateTime.Now;
			ShowStatus(VideoStatusText.Starting());
			Update();
			VideoStatus videoStatus;
			try {
				videoStatus=_videoCaptureSession.Refresh();
			}
			catch(Exception ex) {
				videoStatus=new VideoStatus(VideoStatusKind.Failed,"The camera could not be opened.",ex.Message,VideoStatusAction.Refresh);
			}
			_isPreviewPaused=false;
			ShowStatus(videoStatus);
			labelCamera.Text=GetCameraText();
		}

		private void ShowNewFrame() {
			Interlocked.Exchange(ref _isFramePosted,0);
			if(IsDisposed || _videoCaptureSession==null || _isPreviewPaused || IsFrozen) {
				return;
			}
			UvcFrame frame=_videoCaptureSession.TakePreviewFrame();
			if(frame==null) {
				return;
			}
			Bitmap bitmap;
			try {
				bitmap=frame.ToBitmap();
			}
			catch(Exception ex) {
				ex.DoNothing();//out of memory for one frame: the next one tries again
				return;
			}
			_isLiveFlipped=_model?.IsNextFlipped==true;
			if(_isLiveFlipped) {
				bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);//what you see is what is saved (design section 2, mirror shots)
			}
			SetLiveImage(bitmap);
		}

		private void SetLiveImage(Bitmap bitmap) {
			Image imageOld=pictureLive.Image;
			pictureLive.Image=bitmap;
			imageOld?.Dispose();
		}

		private void HandleCameraError(object sender,UvcErrorEventArgs uvcErrorEventArgs) {
			if(IsDisposed || _videoCaptureSession==null) {
				return;
			}
			if(_frameSourceRecording!=null && sender==_frameSourceRecording) {
				StopRecording();//the camera stopped: the clip is saved up to here
			}
			VideoStatus videoStatus=_videoCaptureSession.HandleCameraError(sender,uvcErrorEventArgs);
			if(videoStatus==null) {
				return;//from a camera that is no longer open
			}
			_isPreviewPaused=false;
			ShowStatus(videoStatus);
		}

		///<summary>The camera's state inside the live picture: hidden while it streams; otherwise its problem in plain words with one action.
		///</summary>
		private void ShowStatus(VideoStatus videoStatus) {
			if(IsDisposed) {
				return;
			}
			bool isStreaming=videoStatus!=null && videoStatus.IsStreaming;
			if(isStreaming || videoStatus==null) {
				labelStatus.Text=Lan.g(this,"Preview paused");
				labelStatus.Visible=_isPreviewPaused;
				butStatusAction.Visible=false;
				butCapture.Enabled=videoStatus!=null;
			}
			else {
				string text=GetStatusText(videoStatus);
				labelStatus.Text=text;
				labelStatus.Visible=true;
				_videoStatusAction=videoStatus.Action;
				butStatusAction.Text=GetActionText(videoStatus);
				butStatusAction.Visible=videoStatus.Kind!=VideoStatusKind.Starting;
				butCapture.Enabled=false;
				if(videoStatus.IsProblem) {
					Unfreeze();//the frozen picture is gone from the screen: it is not saved
					SetLiveImage(null);
				}
			}
			LayoutControls();
		}

		///<summary>The state in the words of the design's error cards (section 6).</summary>
		private string GetStatusText(VideoStatus videoStatus) {
			switch(videoStatus.Kind) {
				case VideoStatusKind.Starting:
					return Lan.g(this,"Starting the camera...");
				case VideoStatusKind.NoCamera:
					return Lan.g(this,"No camera found.")+"\r\n"+Lan.g(this,"Plug the camera in. It must also show a picture in Windows' Camera app.");
				case VideoStatusKind.InUse:
					return Lan.g(this,"Camera is in use by another program.")+"\r\n"
						+Lan.g(this,"Close the other camera program (for example the camera maker's software), then try again.");
				case VideoStatusKind.Disconnected:
					return Lan.g(this,"Camera disconnected.")+"\r\n"+Lan.g(this,"Photos taken so far are saved. Plug it back in to carry on.");
			}
			string text=Lan.g(this,videoStatus.Text);
			if(videoStatus.Detail!="") {
				text+="\r\n"+videoStatus.Detail;
			}
			return text;
		}

		private string GetActionText(VideoStatus videoStatus) {
			switch(videoStatus.Kind) {
				case VideoStatusKind.NoCamera:
					return Lan.g(this,"Look again");
				case VideoStatusKind.InUse:
					return Lan.g(this,"Try again");
			}
			return videoStatus.Action==VideoStatusAction.Reconnect ? Lan.g(this,"Reconnect") : Lan.g(this,"Try again");
		}

		private string GetCameraText() {
			VideoCaptureSession videoCaptureSession=_videoCaptureSession;
			if(videoCaptureSession?.Device==null) {
				return Lan.g(this,"Camera")+": "+Lan.g(this,"none");
			}
			string text=Lan.g(this,"Camera")+": "+videoCaptureSession.Device.FriendlyName;
			if(videoCaptureSession.ModeChoice?.Mode!=null) {
				text+="\r\n"+Lan.g(this,"Picture")+": "+videoCaptureSession.ModeChoice.Mode.ToString();
			}
			return text;
		}

		///<summary>False while the session cannot take a photo: not shown, or its window is disabled under a modal window.</summary>
		private bool IsEnabledForCapture() {
			if(IsDisposed || Disposing || !IsHandleCreated || !Visible || !Enabled || _isEnded) {
				return false;
			}
			Form form=FindForm();
			return form!=null && form.IsHandleCreated && IsWindowEnabled(form.Handle);
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindowEnabled(IntPtr hWnd);
		#endregion Methods - Camera

		#region Methods - Show
		private string GetBannerText() {
			string text=_patient.GetNameLF();
			if(_patient.Birthdate.Year>1880) {
				text+="  |  "+Lan.g(this,"DOB")+" "+_patient.Birthdate.ToShortDateString();
			}
			text+="  |  "+(_patient.ChartNumber!="" ? Lan.g(this,"Chart")+" "+_patient.ChartNumber : Lan.g(this,"PatNum")+" "+_patient.PatNum.ToString());
			text+="      "+(_mount==null ? Lan.g(this,"Quick photos") : _mount.Description)+"  |  "+DateTime.Today.ToShortDateString();
			return text;
		}

		private void RefreshGuide() {
			if(_model==null) {
				return;
			}
			//Built from slot labels, teeth and counts: not translation keys (Lan.g would add a language row for each).
			labelGuide.Text=(IsFrozen ? Lan.g(this,"FROZEN")+"  " : "")+_model.GetGuideText(FuncDisplayTeeth);
			panelLive.BackColor=IsFrozen ? Color.FromArgb(255,210,74) : Color.FromArgb(14,16,18);//the frozen picture has a yellow border
			labelProgress.Text=_model.GetProgressText();
			labelHint.Text=_model.IsQuickPhotos ? Lan.g(this,"Each photo is saved unmounted in the photo category. Right-click a photo: Teeth, Rotate, Delete.")
				: Lan.g(this,"Yellow is next. Click an empty slot to shoot it next. Right-click a photo: Retake, Teeth, Rotate, Delete. Ctrl+Z retakes the last.");
			panelMount.SetSlots(_model,_mount,_dictThumbs,_listThumbsQuick);
			if(butFreeze!=null) {
				butFreeze.Text=IsFrozen ? Lan.g(this,"Live (F)") : Lan.g(this,"Freeze (F)");
			}
			LayoutControls();
		}

		///<summary>The red notice over the live picture; "" hides it.</summary>
		private void ShowAlert(string text) {
			if(IsDisposed) {
				return;
			}
			labelAlert.Text=text;
			labelAlert.Visible=text!="";
			LayoutControls();
		}

		///<summary>A short note under the picture, for example why a press did nothing. "" clears it.</summary>
		private void ShowInfo(string text) {
			if(IsDisposed) {
				return;
			}
			labelInfo.Text=text;
		}
		#endregion Methods - Show

		#region Methods - Layout
		private void BuildControls() {
			SuspendLayout();
			labelBanner=new Label { AutoSize=false,BackColor=UiTheme.Accent,ForeColor=UiTheme.TextOnAccent,TextAlign=ContentAlignment.MiddleLeft,
				Padding=new Padding(_funcScale(10),0,0,0),Font=UiTheme.FontBody,UseMnemonic=false };
			panelLive=new Panel { BackColor=Color.FromArgb(14,16,18) };
			pictureLive=new PictureBox { SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.FromArgb(14,16,18) };
			labelGuide=new Label { AutoSize=false,BackColor=Color.FromArgb(40,44,50),ForeColor=Color.White,Font=UiTheme.FontBody,UseMnemonic=false,
				TextAlign=ContentAlignment.MiddleLeft,Padding=new Padding(_funcScale(8),0,_funcScale(8),0) };
			labelStatus=new Label { AutoSize=false,BackColor=Color.FromArgb(14,16,18),ForeColor=Color.FromArgb(232,236,241),Font=UiTheme.FontBody,
				TextAlign=ContentAlignment.MiddleCenter,UseMnemonic=false,Visible=false };
			butStatusAction=new UI.Button { Text=Lan.g(this,"Try again"),Visible=false,TabStop=false };
			butStatusAction.Click+=butStatusAction_Click;
			panelLive.Controls.Add(butStatusAction);
			panelLive.Controls.Add(labelStatus);
			panelLive.Controls.Add(labelGuide);
			labelAlert=new Label { AutoSize=false,BackColor=UiTheme.DangerSubtle,ForeColor=UiTheme.Danger,Font=UiTheme.FontBody,UseMnemonic=false,
				TextAlign=ContentAlignment.MiddleLeft,Padding=new Padding(_funcScale(8),0,_funcScale(8),0),Visible=false };
			panelLive.Controls.Add(labelAlert);
			panelLive.Controls.Add(pictureLive);
			panelBar=new Panel { BackColor=UiTheme.SurfaceSubtle };
			butCapture=new UI.Button { Text=Lan.g(this,"Capture (Space)"),TabStop=false };
			butCapture.Click+=butCapture_Click;
			butFreeze=new UI.Button { Text=Lan.g(this,"Freeze (F)"),TabStop=false };
			butFreeze.Click+=(sender,e) => ToggleFreeze();
			butTeeth=new UI.Button { Text=Lan.g(this,"Teeth (T)"),TabStop=false };
			butTeeth.Click+=(sender,e) => PickTeethNext();
			butRecord=new UI.Button { Text=Lan.g(this,"Record (R)"),TabStop=false };
			butRecord.Click+=(sender,e) => ToggleRecording();
			labelRec=new Label { AutoSize=false,BackColor=Color.FromArgb(40,44,50),ForeColor=Color.FromArgb(255,107,107),Font=UiTheme.FontBody,
				UseMnemonic=false,TextAlign=ContentAlignment.MiddleCenter,Visible=false };
			timerRec=new System.Windows.Forms.Timer { Interval=250 };
			timerRec.Tick+=timerRec_Tick;
			butBack=new UI.Button { Text=Lan.g(this,"Previous (Left)"),TabStop=false };
			butBack.Click+=(sender,e) => Skip(isForward:false);
			butSkip=new UI.Button { Text=Lan.g(this,"Skip (Right)"),TabStop=false };
			butSkip.Click+=(sender,e) => Skip(isForward:true);
			butEnd=new UI.Button { Text=Lan.g(this,"End (Esc)"),TabStop=false };
			butEnd.Click+=(sender,e) => End();
			labelInfo=new Label { AutoSize=false,ForeColor=UiTheme.TextSecondary,BackColor=UiTheme.SurfaceSubtle,Font=UiTheme.FontBody,
				TextAlign=ContentAlignment.MiddleLeft,UseMnemonic=false,AutoEllipsis=true };
			panelBar.Controls.AddRange(new Control[] { butCapture,butFreeze,butTeeth,butRecord,butBack,butSkip,butEnd,labelInfo });
			panelLive.Controls.Add(labelRec);
			panelFlash=new Panel { BackColor=Color.White,Visible=false };
			panelLive.Controls.Add(panelFlash);
			timerIdle=new System.Windows.Forms.Timer { Interval=30000 };
			timerIdle.Tick+=timerIdle_Tick;
			timerFlash=new System.Windows.Forms.Timer { Interval=120 };
			timerFlash.Tick+=(sender,e) => {
				timerFlash.Stop();
				panelFlash.Visible=false;
			};
			contextMenuShot=new ContextMenuStrip();
			contextMenuShot.Items.Add(Lan.g(this,"Retake"),null,(sender,e) => { if(_shotMenu!=null) Retake(_shotMenu); });
			contextMenuShot.Items.Add(Lan.g(this,"Teeth..."),null,(sender,e) => { if(_shotMenu!=null) EditTeethShot(_shotMenu); });
			contextMenuShot.Items.Add(Lan.g(this,"Rotate right"),null,(sender,e) => { if(_shotMenu!=null) RotateShot(_shotMenu); });
			contextMenuShot.Items.Add(Lan.g(this,"Delete..."),null,(sender,e) => { if(_shotMenu!=null) DeleteShot(_shotMenu); });
			panelRight=new Panel { BackColor=UiTheme.SurfaceRaised };
			labelTitle=new Label { AutoSize=false,Font=UiTheme.FontTitle,ForeColor=UiTheme.Text,UseMnemonic=false,AutoEllipsis=true };
			labelProgress=new Label { AutoSize=false,Font=UiTheme.FontBody,ForeColor=UiTheme.TextSecondary,UseMnemonic=false };
			panelMount=new PanelMount();
			panelMount.MouseClick+=panelMount_MouseClick;
			labelHint=new Label { AutoSize=false,Font=UiTheme.FontBody,ForeColor=UiTheme.TextSecondary,UseMnemonic=false };
			labelCamera=new Label { AutoSize=false,Font=UiTheme.FontBody,ForeColor=UiTheme.Text,UseMnemonic=false,AutoEllipsis=true };
			panelRight.Controls.AddRange(new Control[] { labelTitle,labelProgress,panelMount,labelHint,labelCamera });
			Controls.Add(panelLive);
			Controls.Add(panelBar);
			Controls.Add(panelRight);
			Controls.Add(labelBanner);
			ResumeLayout(false);
		}

		private void LayoutControls() {
			if(labelBanner==null || Width<1 || Height<1) {
				return;
			}
			int pad=_funcScale(8);
			int heightBanner=_funcScale(28);
			int widthRight=Math.Min(_funcScale(260),Width/2);
			int heightBar=_funcScale(40);
			labelBanner.Bounds=new Rectangle(0,0,Width,heightBanner);
			panelRight.Bounds=new Rectangle(Width-widthRight,heightBanner,widthRight,Height-heightBanner);
			int widthLeft=Width-widthRight;
			panelBar.Bounds=new Rectangle(0,Height-heightBar,widthLeft,heightBar);
			panelLive.Bounds=new Rectangle(0,heightBanner,widthLeft,Math.Max(0,Height-heightBanner-heightBar));
			//live area
			int inset=IsFrozen ? _funcScale(4) : 0;
			pictureLive.Bounds=new Rectangle(inset,inset,Math.Max(0,panelLive.Width-2*inset),Math.Max(0,panelLive.Height-2*inset));
			//the guide box fits its text, wrapping when the live area is narrow
			int widthGuideMax=Math.Max(_funcScale(60),panelLive.Width-2*pad);
			int widthPadding=labelGuide.Padding.Horizontal+_funcScale(14);//the label's own margins
			Size sizeGuide=TextRenderer.MeasureText(labelGuide.Text==""? " " : labelGuide.Text,labelGuide.Font,new Size(widthGuideMax-widthPadding,0),
				TextFormatFlags.WordBreak|TextFormatFlags.NoPrefix);
			labelGuide.Bounds=new Rectangle(pad,pad,Math.Min(widthGuideMax,sizeGuide.Width+widthPadding),Math.Max(_funcScale(26),sizeGuide.Height+_funcScale(8)));
			labelGuide.Visible=labelGuide.Text!="";
			int widthStatus=Math.Min(panelLive.Width-2*pad,_funcScale(460));
			labelStatus.Bounds=new Rectangle((panelLive.Width-widthStatus)/2,panelLive.Height/2-_funcScale(50),widthStatus,_funcScale(70));
			butStatusAction.Bounds=new Rectangle((panelLive.Width-_funcScale(110))/2,labelStatus.Bottom+pad,_funcScale(110),_funcScale(26));
			if(labelStatus.Visible) {
				labelStatus.BringToFront();
				butStatusAction.BringToFront();
			}
			labelGuide.BringToFront();
			if(labelRec.Visible) {
				int widthRec=TextRenderer.MeasureText(labelRec.Text,labelRec.Font).Width+_funcScale(20);
				labelRec.Bounds=new Rectangle(Math.Max(pad,panelLive.Width-widthRec-pad),pad,widthRec,_funcScale(26));
				labelRec.BringToFront();
			}
			if(labelAlert.Visible) {
				int widthAlert=Math.Max(_funcScale(60),panelLive.Width-2*pad);
				Size sizeAlert=TextRenderer.MeasureText(labelAlert.Text,labelAlert.Font,new Size(widthAlert-labelAlert.Padding.Horizontal-_funcScale(14),0),
					TextFormatFlags.WordBreak|TextFormatFlags.NoPrefix);
				int heightAlert=sizeAlert.Height+_funcScale(12);
				labelAlert.Bounds=new Rectangle(pad,Math.Max(0,panelLive.Height-heightAlert-pad),widthAlert,heightAlert);
				labelAlert.BringToFront();
			}
			//bar
			int x=pad;
			int heightButton=_funcScale(26);
			int y=(heightBar-heightButton)/2;
			bool isMount=_model!=null && !_model.IsQuickPhotos;
			butBack.Visible=isMount;
			butSkip.Visible=isMount;
			foreach(UI.Button button in new[] { butCapture,butFreeze,butTeeth,butRecord,butBack,butSkip,butEnd }) {
				if(!button.Visible) {
					continue;
				}
				int widthButton=TextRenderer.MeasureText(button.Text,UiTheme.FontBody).Width+_funcScale(24);
				button.Bounds=new Rectangle(x,y,widthButton,heightButton);
				x+=widthButton+pad;
			}
			labelInfo.Bounds=new Rectangle(x,y,Math.Max(0,widthLeft-x-pad),heightButton);
			//right panel
			int widthInner=panelRight.Width-2*pad;
			labelTitle.Bounds=new Rectangle(pad,pad,widthInner,_funcScale(24));
			labelProgress.Bounds=new Rectangle(pad,labelTitle.Bottom,widthInner,_funcScale(20));
			int heightMount=Math.Max(_funcScale(80),Math.Min(widthInner*3/4,panelRight.Height/2));
			panelMount.Bounds=new Rectangle(pad,labelProgress.Bottom+pad,widthInner,heightMount);
			labelHint.Bounds=new Rectangle(pad,panelMount.Bottom+pad,widthInner,_funcScale(66));
			labelCamera.Bounds=new Rectangle(pad,labelHint.Bottom+pad,widthInner,_funcScale(48));
		}
		#endregion Methods - Layout

		///<summary>The photo mount being filled, drawn small: each photo slot at its place, its photo when taken, the next slot outlined in yellow;
		///for quick photos, the photos of the session in a grid. A click picks a slot (HitTestSlot).</summary>
		private class PanelMount:Panel {
			private PhotoSessionModel _model;
			private Mount _mount;
			private Dictionary<long,Bitmap> _dictThumbs;
			private List<Bitmap> _listThumbsQuick;
			private readonly List<Rectangle> _listRectanglesSlots=new List<Rectangle>();
			private readonly List<Rectangle> _listRectanglesQuick=new List<Rectangle>();

			///<summary>The index in the quick photos under the point, or -1.</summary>
			public int HitTestQuick(Point point) {
				for(int i=0;i<_listRectanglesQuick.Count;i++) {
					if(_listRectanglesQuick[i].Contains(point)) {
						return i;
					}
				}
				return -1;
			}

			public PanelMount() {
				DoubleBuffered=true;
				BackColor=Color.FromArgb(28,32,38);
				ResizeRedraw=true;
			}

			public void SetSlots(PhotoSessionModel model,Mount mount,Dictionary<long,Bitmap> dictThumbs,List<Bitmap> listThumbsQuick) {
				_model=model;
				_mount=mount;
				_dictThumbs=dictThumbs;
				_listThumbsQuick=listThumbsQuick;
				Invalidate();
			}

			///<summary>The index in the model's slots under the point, or -1.</summary>
			public int HitTestSlot(Point point) {
				for(int i=0;i<_listRectanglesSlots.Count;i++) {
					if(_listRectanglesSlots[i].Contains(point)) {
						return i;
					}
				}
				return -1;
			}

			protected override void OnPaint(PaintEventArgs e) {
				base.OnPaint(e);
				_listRectanglesSlots.Clear();
				_listRectanglesQuick.Clear();
				if(_model==null) {
					return;
				}
				Graphics g=e.Graphics;
				g.InterpolationMode=InterpolationMode.HighQualityBilinear;
				if(_model.IsQuickPhotos) {
					PaintQuick(g);
					return;
				}
				int widthMount=Math.Max(1,_mount?.Width??1);
				int heightMount=Math.Max(1,_mount?.Height??1);
				float scale=Math.Min((float)(Width-4)/widthMount,(float)(Height-4)/heightMount);
				float xOffset=(Width-widthMount*scale)/2;
				float yOffset=(Height-heightMount*scale)/2;
				using Brush brushEmpty=new SolidBrush(Color.FromArgb(47,53,61));
				using Pen penNext=new Pen(Color.FromArgb(255,210,74),3);
				for(int i=0;i<_model.ListSlots.Count;i++) {
					PhotoSlot slot=_model.ListSlots[i];
					Rectangle rectangle=Rectangle.Round(new RectangleF(xOffset+slot.Xpos*scale+1,yOffset+slot.Ypos*scale+1,slot.Width*scale-2,slot.Height*scale-2));
					_listRectanglesSlots.Add(rectangle);
					if(_dictThumbs!=null && _dictThumbs.TryGetValue(slot.MountItemNum,out Bitmap bitmap)) {
						g.DrawImage(bitmap,FitInside(bitmap.Size,rectangle));
					}
					else {
						g.FillRectangle(brushEmpty,rectangle);
						if(slot.IsFilled) {
							TextRenderer.DrawText(g,"\u2713",UiTheme.FontBody,rectangle,Color.FromArgb(108,197,138),
								TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
						}
					}
					TextRenderer.DrawText(g,slot.GetLabel(),UiTheme.FontBody,new Rectangle(rectangle.X+2,rectangle.Bottom-16,rectangle.Width-4,16),
						Color.FromArgb(207,214,222),TextFormatFlags.Left|TextFormatFlags.Bottom|TextFormatFlags.EndEllipsis|TextFormatFlags.NoPrefix);
					if(i==_model.IdxNext) {
						g.DrawRectangle(penNext,rectangle);
					}
				}
			}

			private void PaintQuick(Graphics g) {
				if(_listThumbsQuick==null || _listThumbsQuick.Count==0) {
					TextRenderer.DrawText(g,Lan.g(nameof(ControlCameraSession),"No photos yet."),UiTheme.FontBody,ClientRectangle,Color.FromArgb(207,214,222),
						TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
					return;
				}
				int columns=3;
				int widthCell=(Width-4)/columns;
				int heightCell=widthCell*3/4;
				for(int i=0;i<_listThumbsQuick.Count;i++) {
					Rectangle rectangle=new Rectangle(2+(i%columns)*widthCell,2+(i/columns)*heightCell,widthCell-2,heightCell-2);
					if(rectangle.Bottom>Height) {
						break;
					}
					_listRectanglesQuick.Add(rectangle);
					g.DrawImage(_listThumbsQuick[i],FitInside(_listThumbsQuick[i].Size,rectangle));
				}
			}

			private static Rectangle FitInside(Size size,Rectangle rectangle) {
				float scale=Math.Min((float)rectangle.Width/Math.Max(1,size.Width),(float)rectangle.Height/Math.Max(1,size.Height));
				int width=(int)(size.Width*scale);
				int height=(int)(size.Height*scale);
				return new Rectangle(rectangle.X+(rectangle.Width-width)/2,rectangle.Y+(rectangle.Height-height)/2,width,height);
			}
		}
	}
}
