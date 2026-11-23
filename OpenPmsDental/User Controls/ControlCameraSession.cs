// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
		///<summary>True when the session ended because the patient changed.</summary>
		public bool IsPatientChange { get; set; }
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
		private VideoStatusAction _videoStatusAction;
		private Label labelBanner;
		private Panel panelLive;
		private PictureBox pictureLive;
		private Label labelGuide;
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
			ShowStatus(VideoStatusText.Starting());
			return "";
		}

		///<summary>Opens the camera; call after the control is shown, so "Starting the camera" is painted first.</summary>
		public void OpenCamera() {
			RefreshCamera();
			Focus();
		}

		///<summary>Ends the session: releases the camera and raises EventEnded once. Everything is saved already.</summary>
		public void End(bool isPatientChange=false) {
			if(_isEnded) {
				return;
			}
			_isEnded=true;
			ReleaseCamera();
			EventEnded?.Invoke(this,new CameraSessionEndedEventArgs {
				MountNum=_mount?.MountNum??0,
				DocNumLast=_docNumLast,
				CountSaved=_model?.CountSaved??0,
				IsPatientChange=isPatientChange,
			});
		}

		private void ReleaseCamera() {
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
			}
			base.Dispose(disposing);
		}
		#endregion Methods - Start and End

		#region Methods - Event Handlers
		///<summary>Space, the arrows and Esc work wherever the focus is inside the session.</summary>
		protected override bool ProcessCmdKey(ref Message msg,Keys keyData) {
			switch(keyData) {
				case Keys.Space:
					RequestCapture(CaptureRequestSource.SpaceInWindow,null);
					return true;
				case Keys.Right:
					Skip(isForward:true);
					return true;
				case Keys.Left:
					Skip(isForward:false);
					return true;
				case Keys.Escape:
					End();
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
			int idx=panelMount.HitTestSlot(e.Location);
			if(idx<0 || _model==null) {
				return;
			}
			if(!_model.SelectSlot(idx)) {
				ShowInfo(Lan.g(this,"That slot has a photo already."));
				return;
			}
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
			PhotoShotPlan photoShotPlan=_model.PlanShot();
			if(photoShotPlan==null) {
				ShowInfo(Lan.g(this,"Every slot is filled. End the session (Esc)."));
				return;
			}
			CaptureStart captureStart=videoCaptureSession.BeginCapture(captureRequestSource,frameStill);
			if(!captureStart.IsAccepted) {
				ShowInfo(Lan.g(this,CaptureGate.GetDropText(captureStart.DropReason)));
				return;
			}
			try {
				PhotoSaveResult photoSaveResult=PhotoSaver.Save(_store,captureStart.Bitmap,photoShotPlan,_patient.PatNum,_categoryDefNum);
				if(photoSaveResult.Document==null) {
					ShowInfo(Lan.g(this,photoSaveResult.Error));
					return;
				}
				_docNumLast=photoSaveResult.Document.DocNum;
				_model.MarkSaved(photoShotPlan);
				AddThumb(photoShotPlan,captureStart.Bitmap);
				string textSaved=photoShotPlan.GuideLabel!="" ? Lan.g(this,"Saved")+": "+photoShotPlan.GuideLabel : Lan.g(this,"Photo saved");
				ShowInfo(photoSaveResult.Error=="" ? textSaved : Lan.g(this,photoSaveResult.Error));
				RefreshGuide();
			}
			finally {
				videoCaptureSession.EndCapture(captureStart);
			}
		}

		private void Skip(bool isForward) {
			if(_model==null || _model.IsQuickPhotos) {
				return;
			}
			bool isMoved=isForward ? _model.SkipForward() : _model.SkipBack();
			ShowInfo(isMoved ? "" : Lan.g(this,"No other empty slot."));
			RefreshGuide();
		}

		///<summary>A small copy of the saved photo as it will show: flipped for a mirror shot and rotated as the slot says.</summary>
		private void AddThumb(PhotoShotPlan photoShotPlan,Bitmap bitmap) {
			Bitmap bitmapThumb=MakeThumb(bitmap,photoShotPlan.IsFlipped,photoShotPlan.DegreesRotated,_funcScale(160));
			if(bitmapThumb==null) {
				return;
			}
			if(photoShotPlan.MountItemNum==0) {
				_listThumbsQuick.Add(bitmapThumb);
				while(_listThumbsQuick.Count>12) {
					_listThumbsQuick[0].Dispose();
					_listThumbsQuick.RemoveAt(0);
				}
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

		#region Methods - Camera
		private void RefreshCamera() {
			if(_videoCaptureSession==null || _videoCaptureSession.Gate.IsClosing) {
				return;
			}
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
			if(IsDisposed || _videoCaptureSession==null || _isPreviewPaused) {
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
			if(_model?.SlotNext?.IsMirror==true) {
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
			labelGuide.Text=Lan.g(this,_model.GetGuideText(FuncDisplayTeeth));
			labelProgress.Text=Lan.g(this,_model.GetProgressText());
			labelHint.Text=_model.IsQuickPhotos ? Lan.g(this,"Each photo is saved unmounted in the photo category.")
				: Lan.g(this,"Yellow is next. Click an empty slot to shoot it next. Arrows skip a slot.");
			panelMount.SetSlots(_model,_mount,_dictThumbs,_listThumbsQuick);
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
			panelLive.Controls.Add(pictureLive);
			panelBar=new Panel { BackColor=UiTheme.SurfaceSubtle };
			butCapture=new UI.Button { Text=Lan.g(this,"Capture (Space)"),TabStop=false };
			butCapture.Click+=butCapture_Click;
			butBack=new UI.Button { Text=Lan.g(this,"Previous slot (Left)"),TabStop=false };
			butBack.Click+=(sender,e) => Skip(isForward:false);
			butSkip=new UI.Button { Text=Lan.g(this,"Skip slot (Right)"),TabStop=false };
			butSkip.Click+=(sender,e) => Skip(isForward:true);
			butEnd=new UI.Button { Text=Lan.g(this,"End (Esc)"),TabStop=false };
			butEnd.Click+=(sender,e) => End();
			labelInfo=new Label { AutoSize=false,ForeColor=UiTheme.TextSecondary,BackColor=UiTheme.SurfaceSubtle,Font=UiTheme.FontBody,
				TextAlign=ContentAlignment.MiddleLeft,UseMnemonic=false,AutoEllipsis=true };
			panelBar.Controls.AddRange(new Control[] { butCapture,butBack,butSkip,butEnd,labelInfo });
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
			pictureLive.Bounds=new Rectangle(0,0,panelLive.Width,panelLive.Height);
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
			//bar
			int x=pad;
			int heightButton=_funcScale(26);
			int y=(heightBar-heightButton)/2;
			foreach(UI.Button button in new[] { butCapture,butBack,butSkip,butEnd }) {
				int widthButton=TextRenderer.MeasureText(button.Text,UiTheme.FontBody).Width+_funcScale(24);
				button.Bounds=new Rectangle(x,y,widthButton,heightButton);
				x+=widthButton+pad;
			}
			bool isMount=_model!=null && !_model.IsQuickPhotos;
			butBack.Visible=isMount;
			butSkip.Visible=isMount;
			if(!isMount) {
				butEnd.Left=butCapture.Right+pad;
				x=butEnd.Right+pad;
			}
			labelInfo.Bounds=new Rectangle(x,y,Math.Max(0,widthLeft-x-pad),heightButton);
			//right panel
			int widthInner=panelRight.Width-2*pad;
			labelTitle.Bounds=new Rectangle(pad,pad,widthInner,_funcScale(24));
			labelProgress.Bounds=new Rectangle(pad,labelTitle.Bottom,widthInner,_funcScale(20));
			int heightMount=Math.Max(_funcScale(80),Math.Min(widthInner*3/4,panelRight.Height/2));
			panelMount.Bounds=new Rectangle(pad,labelProgress.Bottom+pad,widthInner,heightMount);
			labelHint.Bounds=new Rectangle(pad,panelMount.Bottom+pad,widthInner,_funcScale(48));
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
