using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>The image viewer window. A double-click on an image in the Imaging module (a filled mount slot, a single image, or an image in the tree) opens that one image here, in an ordinary window at normal size: never full screen and never opened maximised (owner, 2026-11-22). A double-click or Esc closes it, and the mount then selects the image last shown. Left and Right step through the mount's images, the wheel zooms at the pointer, a drag pans and Home fits the image again. The image shows as the module shows it (rotation, flip, crop, brightness/contrast, drawings); the window changes nothing. One window is reused for every double-click.</summary>
	public class FormImageViewer:FormODBase {
		#region Fields - Private
		///<summary>The image surface. The window's two controls are made in code (BuildControls), so the window has no designer file for the UI
		///rollout's designer scans.</summary>
		private ControlDoubleBuffered canvas;
		///<summary>The line over the image: its place in the mount, when it was taken, its teeth, and the keys.</summary>
		private Label labelInfo;
		///<summary>Bounds of the last viewer window closed in its normal state, this run of the program. Empty until then.</summary>
		private static Rectangle _rectangleLast=Rectangle.Empty;
		private Bitmap _bitmap;
		private ControlImageDisplay _controlImageDisplay;
		///<summary>Index in the source's documents of the image showing; -1 for none.</summary>
		private int _idxShowing=-1;
		private bool _isPanning;
		///<summary>True from SetSource until the next Activated, which then needs no second render of the image just loaded.</summary>
		private bool _isJustLoaded;
		///<summary>The source's indexes the window can show, in viewing order.</summary>
		private List<int> _listIdxs=new List<int>();
		private NodeTypeAndKey _nodeTypeAndKeySource;
		private Point _pointMouseDown;
		///<summary>The image centre's offset from the canvas centre, in canvas pixels.</summary>
		private PointF _pointFPan;
		private PointF _pointFPanMouseDown;
		///<summary>1 is the image fitted to the canvas.</summary>
		private float _zoom=1;
		private const float ZOOM_MAX=32;
		private const float ZOOM_MIN=0.25f;
		private const float ZOOM_STEP=1.2f;
		#endregion Fields - Private

		#region Constructor
		public FormImageViewer() {
			BuildControls();
			InitializeLayoutManager();
			Lan.F(this);
		}

		///<summary>Makes the window's controls: the info line docked at the top (8 px side padding, the grid's container padding) and the image
		///surface filling the rest.</summary>
		private void BuildControls(){
			SuspendLayout();
			labelInfo=new Label();
			labelInfo.Dock=DockStyle.Top;
			labelInfo.Name="labelInfo";
			labelInfo.Padding=new Padding(8,0,8,0);
			labelInfo.Size=new Size(800,30);
			labelInfo.TabIndex=0;
			labelInfo.TextAlign=ContentAlignment.MiddleLeft;
			labelInfo.Font=UiTheme.FontBody;
			labelInfo.ForeColor=UiTheme.TextSecondary;
			labelInfo.BackColor=UiTheme.SurfaceSubtle;
			canvas=new ControlDoubleBuffered();
			canvas.Dock=DockStyle.Fill;
			canvas.Name="canvas";
			canvas.TabIndex=1;
			canvas.Paint+=canvas_Paint;
			canvas.MouseDoubleClick+=canvas_MouseDoubleClick;
			canvas.MouseDown+=canvas_MouseDown;
			canvas.MouseMove+=canvas_MouseMove;
			canvas.MouseUp+=canvas_MouseUp;
			canvas.MouseWheel+=canvas_MouseWheel;
			canvas.Resize+=canvas_Resize;
			ClientSize=new Size(800,550);
			Controls.Add(canvas);
			Controls.Add(labelInfo);
			Name="FormImageViewer";
			ShowIcon=false;
			Text="Image";
			Activated+=FormImageViewer_Activated;
			FormClosing+=FormImageViewer_FormClosing;
			FormClosed+=FormImageViewer_FormClosed;
			ResumeLayout(false);
		}
		#endregion Constructor

		#region Methods - Public
		///<summary>Shows the image at idx of controlImageDisplay. Called for every double-click, also while the window is open.</summary>
		public void SetSource(ControlImageDisplay controlImageDisplay,int idx){
			if(_controlImageDisplay!=null){
				_controlImageDisplay.Disposed-=controlImageDisplay_Disposed;
			}
			_controlImageDisplay=controlImageDisplay;
			_controlImageDisplay.Disposed+=controlImageDisplay_Disposed;
			_nodeTypeAndKeySource=controlImageDisplay.GetNodeTypeAndKey();
			ShowIdx(idx);
			//The Activated that follows (Show, or Activate on reuse) needs no second render; an active window gets no Activated.
			_isJustLoaded=Form.ActiveForm!=this;
		}

		///<summary>Places a window that is not showing yet: the last bounds used this run if they are still on a screen, otherwise about two thirds of the screen the Imaging module is on, centred on it.</summary>
		public void SetStartBounds(Control controlOnScreen){
			StartPosition=FormStartPosition.Manual;
			if(!_rectangleLast.IsEmpty && Array.Exists(System.Windows.Forms.Screen.AllScreens,x => x.WorkingArea.IntersectsWith(_rectangleLast))){
				Bounds=_rectangleLast;
				return;
			}
			Rectangle rectangleArea=System.Windows.Forms.Screen.FromControl(controlOnScreen).WorkingArea;
			int width=rectangleArea.Width*2/3;
			int height=rectangleArea.Height*2/3;
			Bounds=new Rectangle(rectangleArea.Left+(rectangleArea.Width-width)/2,rectangleArea.Top+(rectangleArea.Height-height)/2,width,height);
		}
		#endregion Methods - Public

		#region Methods - Event Handlers
		private void canvas_MouseDoubleClick(object sender,MouseEventArgs e) {
			if(e.Button!=MouseButtons.Left){
				return;
			}
			Close();
		}

		private void canvas_MouseDown(object sender,MouseEventArgs e) {
			if(e.Button!=MouseButtons.Left){
				return;
			}
			_isPanning=true;
			_pointMouseDown=e.Location;
			_pointFPanMouseDown=_pointFPan;
		}

		private void canvas_MouseMove(object sender,MouseEventArgs e) {
			if(!_isPanning){
				return;
			}
			_pointFPan=new PointF(_pointFPanMouseDown.X+e.X-_pointMouseDown.X,_pointFPanMouseDown.Y+e.Y-_pointMouseDown.Y);
			canvas.Invalidate();
		}

		private void canvas_MouseUp(object sender,MouseEventArgs e) {
			_isPanning=false;
		}

		private void canvas_MouseWheel(object sender,MouseEventArgs e) {
			ZoomAt(e.Delta>0 ? ZOOM_STEP : 1/ZOOM_STEP,e.Location);
		}

		private void canvas_Paint(object sender,PaintEventArgs e) {
			Graphics g=e.Graphics;
			g.Clear(canvas.BackColor);
			if(_bitmap==null){
				//The background is the mount's colour (often black) or white, so the note takes the theme's text colour that reads on it.
				Color colorText=canvas.BackColor.GetBrightness()<0.5f ? UiTheme.TextOnAccent : UiTheme.TextSecondary;
				TextRenderer.DrawText(g,Lan.g(this,"This image is not loaded."),UiTheme.FontBody,canvas.ClientRectangle,colorText,
					TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
				return;
			}
			float scale=GetScaleFit()*_zoom;
			g.InterpolationMode=InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode=PixelOffsetMode.HighQuality;
			g.TranslateTransform(canvas.Width/2f+_pointFPan.X,canvas.Height/2f+_pointFPan.Y);
			g.ScaleTransform(scale,scale);
			g.DrawImage(_bitmap,-_bitmap.Width/2f,-_bitmap.Height/2f,_bitmap.Width,_bitmap.Height);
		}

		private void canvas_Resize(object sender,EventArgs e) {
			canvas.Invalidate();
		}

		private void controlImageDisplay_Disposed(object sender,EventArgs e) {
			//The image left the module (another image was opened in its place, or the patient changed).
			if(!IsDisposed && !Disposing){
				CloseLater();
			}
		}

		private void FormImageViewer_Activated(object sender,EventArgs e) {
			//Picks up changes made in the module while this window was in the background, such as a rotation or brightness.
			if(_isJustLoaded){
				_isJustLoaded=false;
				return;
			}
			ShowIdx(_idxShowing,keepZoom:true);
		}

		private void FormImageViewer_FormClosing(object sender,FormClosingEventArgs e) {
			if(WindowState==FormWindowState.Normal){
				_rectangleLast=Bounds;
			}
			if(IsSourceValid()){
				_controlImageDisplay.SelectFromViewer(_idxShowing);
			}
		}

		private void FormImageViewer_FormClosed(object sender,FormClosedEventArgs e) {
			if(_controlImageDisplay!=null){
				_controlImageDisplay.Disposed-=controlImageDisplay_Disposed;
			}
			_bitmap?.Dispose();
			_bitmap=null;
		}
		#endregion Methods - Event Handlers

		#region Methods - Private
		///<summary>Closes the window once the current message is handled. A window that was never shown has no handle yet and is disposed at once.</summary>
		private void CloseLater(){
			if(IsDisposed || Disposing){
				return;
			}
			if(!IsHandleCreated){
				//Never shown, so FormClosed will not run its clean-up.
				if(_controlImageDisplay!=null){
					_controlImageDisplay.Disposed-=controlImageDisplay_Disposed;
				}
				Dispose();
				return;
			}
			BeginInvoke(new Action(Close));
		}

		private float GetScaleFit(){
			if(_bitmap==null || _bitmap.Width==0 || _bitmap.Height==0){
				return 1;
			}
			return Math.Min((float)canvas.Width/_bitmap.Width,(float)canvas.Height/_bitmap.Height);
		}

		///<summary>False when the image has left the module, so the window has nothing to show.</summary>
		private bool IsSourceValid(){
			if(_controlImageDisplay==null || _controlImageDisplay.IsDisposed){
				return false;
			}
			return _controlImageDisplay.GetNodeTypeAndKey().IsMatching(_nodeTypeAndKeySource);
		}

		protected override bool ProcessCmdKey(ref Message msg,Keys keyData) {
			switch(keyData){
				case Keys.Escape:
					Close();
					return true;
				case Keys.Left:
					Step(-1);
					return true;
				case Keys.Right:
					Step(1);
					return true;
				case Keys.Home:
					ResetZoom();
					return true;
				case Keys.Add:
				case Keys.Oemplus:
					ZoomAt(ZOOM_STEP,new Point(canvas.Width/2,canvas.Height/2));
					return true;
				case Keys.Subtract:
				case Keys.OemMinus:
					ZoomAt(1/ZOOM_STEP,new Point(canvas.Width/2,canvas.Height/2));
					return true;
			}
			return base.ProcessCmdKey(ref msg,keyData);
		}

		private void ResetZoom(){
			_zoom=1;
			_pointFPan=PointF.Empty;
			canvas.Invalidate();
		}

		///<summary>Loads the image at idx from the source. Closes the window when the image has left the module. With keepZoom, the zoom and pan stay when idx is the image already showing.</summary>
		private void ShowIdx(int idx,bool keepZoom=false){
			if(!IsSourceValid()){
				CloseLater();
				return;
			}
			_listIdxs=_controlImageDisplay.GetViewerIndexes();
			if(_listIdxs.Count==0){
				CloseLater();
				return;
			}
			if(!_listIdxs.Contains(idx)){
				idx=_listIdxs[0];
			}
			if(!keepZoom || idx!=_idxShowing){
				_zoom=1;
				_pointFPan=PointF.Empty;
			}
			_idxShowing=idx;
			_bitmap?.Dispose();
			_bitmap=_controlImageDisplay.GetViewerBitmap(idx);
			canvas.BackColor=_controlImageDisplay.GetViewerBackColor();
			Text=_controlImageDisplay.GetViewerTitle();
			List<string> listParts=new List<string>();
			if(_listIdxs.Count>1){
				listParts.Add(Lan.g(this,"Image")+" "+(_listIdxs.IndexOf(idx)+1)+" "+Lan.g(this,"of")+" "+_listIdxs.Count);
			}
			string details=_controlImageDisplay.GetViewerText(idx);
			if(details!=""){
				listParts.Add(details);
			}
			if(_listIdxs.Count>1){
				listParts.Add(Lan.g(this,"Left/Right: previous/next image"));
			}
			listParts.Add(Lan.g(this,"Wheel: zoom, Home: fit"));
			listParts.Add(Lan.g(this,"Double-click or Esc: close"));
			labelInfo.Text=string.Join("   |   ",listParts);
			canvas.Invalidate();
		}

		///<summary>Moves to the previous (-1) or next (1) image of the mount. Stops at the first and last.</summary>
		private void Step(int direction){
			int pos=_listIdxs.IndexOf(_idxShowing)+direction;
			if(pos<0 || pos>_listIdxs.Count-1){
				return;
			}
			ShowIdx(_listIdxs[pos]);
		}

		///<summary>Multiplies the zoom by factor, keeping the image point under pointCanvas where it is.</summary>
		private void ZoomAt(float factor,Point pointCanvas){
			if(_bitmap==null){
				return;
			}
			float zoomNew=Math.Max(ZOOM_MIN,Math.Min(ZOOM_MAX,_zoom*factor));
			float ratio=zoomNew/_zoom;
			PointF pointFFromCentre=new PointF(pointCanvas.X-canvas.Width/2f,pointCanvas.Y-canvas.Height/2f);
			_pointFPan=new PointF(pointFFromCentre.X-(pointFFromCentre.X-_pointFPan.X)*ratio,pointFFromCentre.Y-(pointFFromCentre.Y-_pointFPan.Y)*ratio);
			_zoom=zoomNew;
			canvas.Invalidate();
		}
		#endregion Methods - Private
	}
}
