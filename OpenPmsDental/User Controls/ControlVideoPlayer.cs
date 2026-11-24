// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using CodeBase;
using CodeBase.Controls;
using ImagingCapture.Video;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>Plays a video document inside the Imaging module (IMG-P4d; owner imaging design, camera and video, section 4 "Watching a video"),
	///in place of the image. The picture is the program's browser control (ODWebView2, Microsoft Edge), which plays H.264 MP4 from the A to Z
	///folder, also from the office server's share, as it shows PDFs; the bar below drives it: play/pause, frame by frame, half speed, a scrub
	///bar with the trim, Set start / Set end / Clear trim (stored in documentadjust: the file is never cut; ImageEdit), and Save frame as photo
	///(the frame read at full size by Media Foundation, VideoFrameReader; a new photo with the same teeth, "From video #30 at 0:17";
	///ImageCreate). The first time a video is shown, its first frame becomes the tree's picture. Controls are built in code (no designer file).
	///</summary>
	public class ControlVideoPlayer:UserControl {
		#region Fields
		///<summary>The state of the page's video element: position and length in seconds, and whether it is paused.</summary>
		private const string ScriptPoll="(function(){var v=document.querySelector('video');if(!v){return '';}"
			+"return v.currentTime+','+(v.paused?1:0)+','+(isFinite(v.duration)?v.duration:0)+','+location.href;})()";
		private readonly Func<int,int> _funcScale;
		private ODWebView2 _odWebView2;
		private PanelScrub panelScrub;
		private Panel panelBar;
		private UI.Button butPlay;
		private UI.Button butFrameBack;
		private UI.Button butFrameForward;
		private UI.Button butSpeed;
		private UI.Button butTrimStart;
		private UI.Button butTrimEnd;
		private UI.Button butTrimClear;
		private UI.Button butSaveFrame;
		private Label labelTime;
		private Label labelMessage;
		private System.Windows.Forms.Timer timerPoll;
		private Document _document;
		private Patient _patient;
		private string _patFolder="";
		private string _pathLocal="";
		private int _durationMs;
		private int _positionMs;
		private int _trimStartMs;
		private int _trimEndMs;
		private bool _isPaused=true;
		private bool _isOpened;
		private bool _isHalfSpeed;
		private bool _isPolling;
		///<summary>Counts ShowVideo and Stop: an answer of the browser that comes back after either belongs to the page before and is dropped.
		///</summary>
		private int _generation;
		///<summary>The address of the video being shown: an answer from another page (the previous one, still loaded) is ignored.</summary>
		private Uri _uriExpected;
		#endregion Fields

		///<summary>A photo was saved from a frame (its DocNum): the module refreshes its tree.</summary>
		public event EventHandler<long> EventFrameSaved;
		///<summary>The tree's picture of the video was saved: the module shows it.</summary>
		public event EventHandler EventThumbnailSaved;

		public ControlVideoPlayer(Func<int,int> funcScale) {
			_funcScale=funcScale??(x => x);
			BackColor=Color.Black;
			Font=UiTheme.FontBody;
			BuildControls();
		}

		public Document Document => _document;
		public int DurationMs => _durationMs;
		public int PositionMs => _positionMs;
		public int TrimStartMs => _trimStartMs;
		public int TrimEndMs => _trimEndMs;
		public bool IsOpened => _isOpened;
		public bool IsPlaying => !_isPaused;
		///<summary>The error shown under the video, or "".</summary>
		public string MessageText => labelMessage.Text;

		#region Methods - Show and Stop
		///<summary>Shows the video at pathLocal (a file this computer can read: the A to Z file, or a copy downloaded for cloud or database storage).
		///</summary>
		public void ShowVideo(Document document,Patient patient,string patFolder,string pathLocal) {
			Stop(isNavigateBlank:false);//the next navigation replaces the page; a blank one in between could block it
			_document=document;
			_patient=patient;
			_patFolder=patFolder??"";
			_pathLocal=pathLocal??"";
			_durationMs=0;
			_positionMs=0;
			_isPaused=true;
			_isOpened=false;
			_isHalfSpeed=false;
			DocumentAdjust documentAdjust=null;
			try {
				documentAdjust=DocumentAdjusts.GetForDoc(document.DocNum);
			}
			catch(Exception ex) {
				ex.DoNothing();//no trim then
			}
			_trimStartMs=documentAdjust?.TrimStartMs??0;
			_trimEndMs=documentAdjust?.TrimEndMs??0;
			ShowMessage("");
			if(_pathLocal=="" || !File.Exists(_pathLocal)) {
				ShowMessage(Lan.g(this,"The video file was not found")+": "+document.FileName);
				RefreshControls();
				return;
			}
			RefreshControls();
			_uriExpected=new Uri(_pathLocal);
			_=NavigateAsync(_pathLocal,_generation);
			int generation=_generation;
			BeginInvoke((Action)(() => {
				if(generation==_generation) {
					SaveThumbnailIfMissing();
				}
			}));
		}

		private async System.Threading.Tasks.Task NavigateAsync(string path,int generation) {
			try {
				if(_odWebView2.CoreWebView2==null) {
					await _odWebView2.Init();//tells the user once when the WebView2 Runtime is missing
				}
				if(_odWebView2.CoreWebView2==null || _pathLocal!=path || generation!=_generation) {
					return;
				}
				//Edge's own menu and keys could save the patient's file outside Export (ImageExport): the bar below is the only control
				_odWebView2.CoreWebView2.Settings.AreDefaultContextMenusEnabled=false;
				_odWebView2.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled=false;
				_odWebView2.ODWebView2Navigate(_uriExpected.AbsoluteUri);//Edge's own player for the file
				timerPoll.Start();
			}
			catch(Exception ex) {
				ShowMessage(Lan.g(this,"The video cannot be shown here")+": "+ex.Message);
			}
		}

		///<summary>Stops playing and leaves the file (a blank page), so it is not held open (a delete needs that).</summary>
		public void Stop() {
			Stop(isNavigateBlank:true);
		}

		private void Stop(bool isNavigateBlank) {
			_generation++;
			timerPoll?.Stop();
			_isOpened=false;
			_isPaused=true;
			_uriExpected=null;
			if(isNavigateBlank && _odWebView2?.CoreWebView2!=null && _pathLocal!="") {
				try {
					_odWebView2.ODWebView2Navigate("about:blank");
				}
				catch(Exception ex) {
					ex.DoNothing();
				}
			}
			_pathLocal="";
		}

		///<summary>Stop, then gives the browser up to a second to let go of the file (before a delete).</summary>
		public void StopAndRelease() {
			bool isShowing=_pathLocal!="";
			Stop();
			if(!isShowing) {
				return;
			}
			DateTime dateTimeEnd=DateTime.Now.AddMilliseconds(1000);
			while(DateTime.Now<dateTimeEnd) {
				Application.DoEvents();
				System.Threading.Thread.Sleep(20);
			}
		}

		protected override void Dispose(bool disposing) {
			if(disposing) {
				timerPoll?.Stop();
				timerPoll?.Dispose();
			}
			base.Dispose(disposing);
		}
		#endregion Methods - Show and Stop

		#region Methods - Player
		///<summary>Reads the video element's position, pause state and length; the first time the length is known, goes to the trim start. Stops
		///at the trim end.</summary>
		private async void timerPoll_Tick(object sender,EventArgs e) {
			if(_isPolling || _odWebView2.CoreWebView2==null || _pathLocal=="") {
				return;
			}
			_isPolling=true;
			int generation=_generation;
			try {
				string result=await _odWebView2.CoreWebView2.ExecuteScriptAsync(ScriptPoll);
				double seconds;
				bool isPaused;
				double durationSeconds;
				string address;
				if(generation!=_generation || !TryParsePoll(result,out seconds,out isPaused,out durationSeconds,out address) || !IsExpectedPage(address)) {
					return;//the previous page answered, or another video was chosen meanwhile
				}
				_positionMs=(int)(seconds*1000);
				_isPaused=isPaused;
				if(durationSeconds>0) {
					_durationMs=(int)(durationSeconds*1000);
				}
				if(!_isOpened && _durationMs>0) {
					//Edge starts a video by itself: it opens paused at the start (of the trim), and only then counts as open
					await RunScript("v.pause();v.currentTime="+ToSeconds(VideoMedia.GetPlayStartMs(_trimStartMs,_durationMs))+";"
						//no controls of Edge's own (its menu offers a download), and the picture filling the area
						+"v.controls=false;v.removeAttribute('controls');v.setAttribute('controlsList','nodownload nofullscreen noremoteplayback');"
						+"v.disablePictureInPicture=true;var st=document.createElement('style');"
						+"st.textContent='video::-webkit-media-controls{display:none !important}';document.head.appendChild(st);"
						+"document.body.style.margin='0';document.body.style.background='black';"
						+"v.style.width='100vw';v.style.height='100vh';v.style.maxWidth='none';v.style.maxHeight='none';v.style.objectFit='contain';");
					if(generation!=_generation) {
						return;
					}
					_isPaused=true;
					_positionMs=VideoMedia.GetPlayStartMs(_trimStartMs,_durationMs);
					_isOpened=true;
					RefreshControls();
					return;
				}
				int playEndMs=VideoMedia.GetPlayEndMs(_trimEndMs,_durationMs);
				if(!_isPaused && playEndMs>0 && _positionMs>=playEndMs) {
					await RunScript("v.pause();v.currentTime="+ToSeconds(playEndMs)+";");
					_isPaused=true;
				}
				RefreshControls();
			}
			catch(Exception ex) {
				ex.DoNothing();//the page is changing
			}
			finally {
				_isPolling=false;
			}
		}

		///<summary>True when address (the page's location) is the video being shown.</summary>
		private bool IsExpectedPage(string address) {
			Uri uri;
			return _uriExpected!=null && Uri.TryCreate(address,UriKind.Absolute,out uri) && Uri.Compare(uri,_uriExpected,UriComponents.AbsoluteUri,
				UriFormat.Unescaped,StringComparison.OrdinalIgnoreCase)==0;
		}

		///<summary>The poll script's answer: "12.5,1,42.0,file:///..." (as a JSON string). False for none (no video element yet).</summary>
		public static bool TryParsePoll(string result,out double seconds,out bool isPaused,out double durationSeconds,out string address) {
			seconds=0;
			isPaused=true;
			durationSeconds=0;
			address="";
			string text=(result??"").Trim();
			if(text.Length>=2 && text.StartsWith("\"") && text.EndsWith("\"")) {
				text=text.Substring(1,text.Length-2).Replace("\\\\","\\").Replace("\\\"","\"");//a JSON string
			}
			string[] arrayParts=text.Split(new[] { ',' },4);
			if(arrayParts.Length!=4) {
				return false;
			}
			address=arrayParts[3];
			if(!double.TryParse(arrayParts[0],NumberStyles.Float,CultureInfo.InvariantCulture,out seconds)
				|| !double.TryParse(arrayParts[2],NumberStyles.Float,CultureInfo.InvariantCulture,out durationSeconds))
			{
				return false;
			}
			isPaused=arrayParts[1]=="1";
			return true;
		}

		private static string ToSeconds(int milliseconds) {
			return (milliseconds/1000d).ToString("0.###",CultureInfo.InvariantCulture);
		}

		///<summary>Runs a statement on the page's video element v (scripts the program runs itself work whatever the page may run).</summary>
		private async System.Threading.Tasks.Task<string> RunScript(string statement) {
			try {
				if(IsDisposed || _odWebView2.CoreWebView2==null) {
					return "";
				}
				return await _odWebView2.CoreWebView2.ExecuteScriptAsync("(function(){var v=document.querySelector('video');if(v){"+statement+"}})()");
			}
			catch(Exception ex) {
				ex.DoNothing();//the browser closed or the page is changing
				return "";
			}
		}

		private async void TogglePlay() {
			if(!_isOpened) {
				return;
			}
			if(!_isPaused) {
				await RunScript("v.pause();");
				_isPaused=true;
				RefreshControls();
				return;
			}
			int playStartMs=VideoMedia.GetPlayStartMs(_trimStartMs,_durationMs);
			int playEndMs=VideoMedia.GetPlayEndMs(_trimEndMs,_durationMs);
			string seek=_positionMs<playStartMs || (playEndMs>0 && _positionMs>=playEndMs-50) ? "v.currentTime="+ToSeconds(playStartMs)+";" : "";
			await RunScript(seek+"v.play();");
			_isPaused=false;
			RefreshControls();
		}

		///<summary>One frame back or forward (about 1/30 s), paused.</summary>
		private async void StepFrame(int direction) {
			if(!_isOpened) {
				return;
			}
			int positionMs=Math.Max(0,Math.Min(_durationMs,_positionMs+direction*33));
			await RunScript("v.pause();v.currentTime="+ToSeconds(positionMs)+";");
			_isPaused=true;
			_positionMs=positionMs;
			RefreshControls();
		}

		private async void ToggleSpeed() {
			if(!_isOpened) {
				return;
			}
			_isHalfSpeed=!_isHalfSpeed;
			await RunScript("v.playbackRate="+(_isHalfSpeed ? "0.5" : "1")+";");
			RefreshControls();
		}

		private async void Seek(int positionMs) {
			if(!_isOpened) {
				return;
			}
			_positionMs=Math.Max(0,Math.Min(positionMs,_durationMs));
			await RunScript("v.currentTime="+ToSeconds(_positionMs)+";");
			RefreshControls();
		}

		///<summary>Set start / Set end / Clear trim: stored in documentadjust (ImageEdit; a signed video also needs Signed Image Edit) and logged.
		///</summary>
		private void SetTrim(int? startMs,int? endMs) {
			if(_document==null || !_isOpened) {
				return;
			}
			if(!ImageSignaturePermissions.IsAuthorizedToEdit(_document)) {
				return;
			}
			int trimStartMs;
			int trimEndMs;
			string error;
			if(!VideoMedia.TryMakeTrim(startMs??_trimStartMs,endMs??_trimEndMs,_durationMs,out trimStartMs,out trimEndMs,out error)) {
				ShowMessage(Lan.g(this,error));
				return;
			}
			try {
				DocumentAdjust documentAdjust=DocumentAdjusts.GetForDoc(_document.DocNum)??DocumentAdjust.FromSettings(_document.DocNum,new ImageAdjustSettings());
				documentAdjust.TrimStartMs=trimStartMs;
				documentAdjust.TrimEndMs=trimEndMs;
				DocumentAdjusts.Upsert(documentAdjust);
				string trim=VideoMedia.DescribeTrim(trimStartMs,trimEndMs,_durationMs);
				SecurityLogs.MakeLogEntry(EnumPermType.ImageEdit,_document.PatNum,"Video trimmed: "+_document.FileName+" "
					+(trim=="" ? "trim cleared" : "plays "+trim),_document.DocNum,DateTime.MinValue);
			}
			catch(Exception ex) {
				ShowMessage(Lan.g(this,"The trim could not be saved")+": "+ex.Message);
				return;
			}
			_trimStartMs=trimStartMs;
			_trimEndMs=trimEndMs;
			ShowMessage("");
			RefreshControls();
		}

		///<summary>Save frame as photo (design section 4): the frame at the position, read at full size, as a new photo unmounted in the video's
		///category with its teeth and "From video #30 at 0:17". ImageCreate.</summary>
		private async void SaveFrame() {
			if(_document==null || _patient==null || !_isOpened || _pathLocal=="") {
				return;
			}
			if(!Security.IsAuthorized(EnumPermType.ImageCreate)) {
				return;
			}
			int generation=_generation;
			string answer=await RunScript("v.pause();return v.currentTime;");
			if(generation!=_generation) {
				return;
			}
			_isPaused=true;
			int positionMs=_positionMs;
			double secondsPaused;
			if(double.TryParse(answer,NumberStyles.Float,CultureInfo.InvariantCulture,out secondsPaused)) {
				positionMs=(int)Math.Round(secondsPaused*1000);//where the pause landed, not the last poll
				_positionMs=positionMs;
			}
			Bitmap bitmap;
			Cursor=Cursors.WaitCursor;
			try {
				int durationMs;
				bitmap=VideoFrameReader.ReadFrame(_pathLocal,positionMs,out durationMs);
			}
			catch(Exception ex) {
				ShowMessage(Lan.g(this,"The frame could not be read")+": "+ex.Message);
				return;
			}
			finally {
				Cursor=Cursors.Default;
			}
			Document documentPhoto;
			using(bitmap) {
				try {
					documentPhoto=ImageStore.Import(bitmap,_document.DocCategory,ImageType.Photo,_patient);
					Document documentOld=documentPhoto.Copy();
					documentPhoto.Description=VideoMedia.GetFrameDescription(_document.DocNum,positionMs);
					documentPhoto.ToothNumbers=_document.ToothNumbers??"";
					Documents.Update(documentPhoto,documentOld);
				}
				catch(Exception ex) {
					ShowMessage(Lan.g(this,"The photo could not be saved")+": "+ex.Message);
					return;
				}
			}
			ShowMessage("");
			labelTime.Text=Lan.g(this,"Saved as a photo")+": "+documentPhoto.Description;
			EventFrameSaved?.Invoke(this,documentPhoto.DocNum);
		}

		///<summary>The first time a video is shown (no tree picture yet, A to Z folder): its first frame becomes the tree's picture.</summary>
		private void SaveThumbnailIfMissing() {
			if(_document==null || _pathLocal=="" || IsDisposed) {
				return;
			}
			try {
				string pathThumbnail=VideoMedia.GetThumbnailPath(_document,_patFolder);
				if(pathThumbnail==null || File.Exists(pathThumbnail)) {
					return;
				}
				int durationMs;
				using(Bitmap bitmap=VideoFrameReader.ReadFrame(_pathLocal,0,out durationMs)) {
					if(VideoMedia.SaveThumbnail(_document,_patFolder,bitmap,durationMs)) {
						EventThumbnailSaved?.Invoke(this,EventArgs.Empty);
					}
				}
			}
			catch(Exception ex) {
				ex.DoNothing();//the tree keeps "not available"
			}
		}
		#endregion Methods - Player

		#region Methods - Show
		private void ShowMessage(string text) {
			labelMessage.Text=text;
			labelMessage.Visible=text!="";
			LayoutControls();
		}

		private void RefreshControls() {
			butPlay.Text=_isPaused ? Lan.g(this,"Play") : Lan.g(this,"Pause");
			butSpeed.Text=_isHalfSpeed ? "1x" : "0.5x";
			foreach(UI.Button button in new[] { butPlay,butFrameBack,butFrameForward,butSpeed,butTrimStart,butTrimEnd,butSaveFrame }) {
				button.Enabled=_isOpened;
			}
			butTrimClear.Enabled=_isOpened && (_trimStartMs>0 || _trimEndMs>0);
			string text=VideoMedia.FormatTime(_positionMs)+" / "+VideoMedia.FormatTime(_durationMs);
			string trim=VideoMedia.DescribeTrim(_trimStartMs,_trimEndMs,_durationMs);
			if(trim!="") {
				text+="   "+Lan.g(this,"trim")+" "+trim;
			}
			if(_document!=null) {
				if(!string.IsNullOrEmpty(_document.ToothNumbers)) {
					text+="   "+Lan.g(this,"teeth")+" "+Tooth.DisplayRange(_document.ToothNumbers);
				}
				text+="   "+Lan.g(this,"recorded")+" "+_document.DateCreated.ToString("g");
			}
			labelTime.Text=text;
			panelScrub.SetPositions(_positionMs,_durationMs,_trimStartMs,_trimEndMs);
		}

		private void BuildControls() {
			SuspendLayout();
			_odWebView2=new ODWebView2();
			_odWebView2.DoBlockNavigation=true;
			_odWebView2.SetScriptEnabled(false);//the file's page runs nothing of its own; the bar's statements are run by the program
			panelScrub=new PanelScrub();
			panelScrub.EventSeek+=(sender,positionMs) => Seek(positionMs);
			panelBar=new Panel { BackColor=UiTheme.SurfaceSubtle };
			butPlay=new UI.Button { Text=Lan.g(this,"Play"),TabStop=false };
			butPlay.Click+=(sender,e) => TogglePlay();
			butFrameBack=new UI.Button { Text=Lan.g(this,"< Frame"),TabStop=false };
			butFrameBack.Click+=(sender,e) => StepFrame(-1);
			butFrameForward=new UI.Button { Text=Lan.g(this,"Frame >"),TabStop=false };
			butFrameForward.Click+=(sender,e) => StepFrame(1);
			butSpeed=new UI.Button { Text="0.5x",TabStop=false };
			butSpeed.Click+=(sender,e) => ToggleSpeed();
			butTrimStart=new UI.Button { Text=Lan.g(this,"Set start"),TabStop=false };
			butTrimStart.Click+=(sender,e) => SetTrim(_positionMs,null);
			butTrimEnd=new UI.Button { Text=Lan.g(this,"Set end"),TabStop=false };
			butTrimEnd.Click+=(sender,e) => SetTrim(null,_positionMs);
			butTrimClear=new UI.Button { Text=Lan.g(this,"Clear trim"),TabStop=false };
			butTrimClear.Click+=(sender,e) => SetTrim(0,0);
			butSaveFrame=new UI.Button { Text=Lan.g(this,"Save frame as photo"),TabStop=false };
			butSaveFrame.Click+=(sender,e) => SaveFrame();
			labelTime=new Label { AutoSize=false,ForeColor=UiTheme.TextSecondary,BackColor=UiTheme.SurfaceSubtle,TextAlign=ContentAlignment.MiddleLeft,
				UseMnemonic=false,AutoEllipsis=true };
			labelMessage=new Label { AutoSize=false,ForeColor=UiTheme.Danger,BackColor=UiTheme.DangerSubtle,TextAlign=ContentAlignment.MiddleLeft,
				UseMnemonic=false,Visible=false,Padding=new Padding(_funcScale(8),0,_funcScale(8),0) };
			panelBar.Controls.AddRange(new Control[] { panelScrub,butPlay,butFrameBack,butFrameForward,butSpeed,butTrimStart,butTrimEnd,butTrimClear,
				butSaveFrame,labelTime,labelMessage });
			timerPoll=new System.Windows.Forms.Timer { Interval=200 };
			timerPoll.Tick+=timerPoll_Tick;
			Controls.Add(_odWebView2);
			Controls.Add(panelBar);
			ResumeLayout(false);
		}

		protected override void OnLayout(LayoutEventArgs e) {
			base.OnLayout(e);
			LayoutControls();
		}

		private void LayoutControls() {
			if(panelBar==null || Width<1 || Height<1) {
				return;
			}
			//UI-12 slice 16: the bar on the grid: UiGrid.PAD from its edges, UiGrid.ITEM_GAP between the scrub bar, the buttons and the message
			//and between the buttons (6 px everywhere before). The video above it is drawn by the web view as before.
			int pad=_funcScale(UiGrid.PAD);
			int gap=_funcScale(UiGrid.ITEM_GAP);
			int heightButton=_funcScale(26);
			int heightScrub=_funcScale(24);
			int heightMessage=labelMessage.Visible ? _funcScale(26) : 0;
			int heightBar=pad+heightScrub+gap+heightButton+pad+(heightMessage>0 ? heightMessage+gap : 0);
			panelBar.Bounds=new Rectangle(0,Math.Max(0,Height-heightBar),Width,heightBar);
			_odWebView2.Bounds=new Rectangle(0,0,Width,Math.Max(0,Height-heightBar));
			panelScrub.Bounds=new Rectangle(pad,pad,Math.Max(0,Width-2*pad),heightScrub);
			int x=pad;
			int y=panelScrub.Bottom+gap;
			foreach(UI.Button button in new[] { butPlay,butFrameBack,butFrameForward,butSpeed,butTrimStart,butTrimEnd,butTrimClear,butSaveFrame }) {
				int widthButton=TextRenderer.MeasureText(button==butSpeed ? "0.5x" : button==butPlay ? "Pause" : button.Text,UiTheme.FontBody).Width+_funcScale(22);
				button.Bounds=new Rectangle(x,y,widthButton,heightButton);
				x+=widthButton+gap;
			}
			labelTime.Bounds=new Rectangle(x,y,Math.Max(0,Width-x-pad),heightButton);
			labelMessage.Bounds=new Rectangle(pad,y+heightButton+gap,Math.Max(0,Width-2*pad),heightMessage);
		}
		#endregion Methods - Show

		///<summary>The scrub bar: the track, the trimmed part highlighted, the trim ends, and the position; a click or a drag seeks.</summary>
		private class PanelScrub:Panel {
			private int _positionMs;
			private int _durationMs;
			private int _trimStartMs;
			private int _trimEndMs;
			private bool _isDragging;

			public event EventHandler<int> EventSeek;

			public PanelScrub() {
				DoubleBuffered=true;
				ResizeRedraw=true;
				BackColor=UiTheme.SurfaceSubtle;
				Cursor=Cursors.Hand;
			}

			public void SetPositions(int positionMs,int durationMs,int trimStartMs,int trimEndMs) {
				_positionMs=positionMs;
				_durationMs=durationMs;
				_trimStartMs=trimStartMs;
				_trimEndMs=trimEndMs;
				Invalidate();
			}

			private float XFromMs(int ms) {
				int widthTrack=Math.Max(1,Width-8);
				return 4+(_durationMs<=0 ? 0 : (float)widthTrack*Math.Max(0,Math.Min(ms,_durationMs))/_durationMs);
			}

			private int MsFromX(int x) {
				int widthTrack=Math.Max(1,Width-8);
				return _durationMs<=0 ? 0 : (int)((long)_durationMs*Math.Max(0,Math.Min(widthTrack,x-4))/widthTrack);
			}

			protected override void OnPaint(PaintEventArgs e) {
				base.OnPaint(e);
				Graphics g=e.Graphics;
				int yMid=Height/2;
				using(Brush brushTrack=new SolidBrush(UiTheme.Stroke)) {
					g.FillRectangle(brushTrack,4,yMid-2,Math.Max(1,Width-8),4);
				}
				if(_durationMs<=0) {
					return;
				}
				float xStart=XFromMs(VideoMedia.GetPlayStartMs(_trimStartMs,_durationMs));
				float xEnd=XFromMs(VideoMedia.GetPlayEndMs(_trimEndMs,_durationMs));
				using(Brush brushPlayed=new SolidBrush(UiTheme.Accent)) {
					g.FillRectangle(brushPlayed,xStart,yMid-2,Math.Max(1,xEnd-xStart),4);
				}
				using(Brush brushTrim=new SolidBrush(Color.FromArgb(214,160,0))) {
					if(_trimStartMs>0) {
						g.FillRectangle(brushTrim,xStart-2,yMid-8,4,16);
					}
					if(_trimEndMs>0) {
						g.FillRectangle(brushTrim,xEnd-2,yMid-8,4,16);
					}
				}
				float xPosition=XFromMs(_positionMs);
				using(Brush brushPosition=new SolidBrush(UiTheme.Text)) {
					g.FillRectangle(brushPosition,xPosition-1,yMid-10,3,20);
				}
			}

			protected override void OnMouseDown(MouseEventArgs e) {
				base.OnMouseDown(e);
				if(e.Button==MouseButtons.Left) {
					_isDragging=true;
					EventSeek?.Invoke(this,MsFromX(e.X));
				}
			}

			protected override void OnMouseMove(MouseEventArgs e) {
				base.OnMouseMove(e);
				if(_isDragging) {
					EventSeek?.Invoke(this,MsFromX(e.X));
				}
			}

			protected override void OnMouseUp(MouseEventArgs e) {
				base.OnMouseUp(e);
				_isDragging=false;
			}
		}
	}
}
