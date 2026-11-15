using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;
using SparksToothChart.Wpf3D;

namespace SparksToothChart {
	///<summary>This is the old tooth chart control.  It "wraps" three different old tooth charts: OpenGL, DirectX9, and 2D, and the WPF 3D chart
	///(Engine, docs/specs/wpf-tooth-chart-plan.md), which no caller chooses yet.</summary>
	public partial class ToothChartWrapper:UserControl {
		private string[] selectedTeeth;
		///<summary>True for hardware graphics, false for software graphics.</summary>
		private bool hardwareMode=false;
		private ToothChartOpenGL toothChartOpenGL;
		private ToothChartDirectX toothChartDirectX;
		private int preferredPixelFormatNum;
		private ToothChartDirectX.DirectXDeviceFormat deviceFormat=null;
		private DrawingMode drawMode;
		private ToothChartData tcData;
		///<summary>The engine that draws: the three DrawingMode engines, or Wpf3D. drawMode is its stored view (Wpf3D reads as DirectX).</summary>
		private ToothChartEngine _engine;
		///<summary>Wpf3D: the engine control that shows the renderer's image: the on-screen chart until the live view is made, when the wrapper is drawn
		///in software, and when the live view could not start.</summary>
		private ToothChartImageHost toothChartImageHost;
		///<summary>Wpf3D: the live view on screen (O-19 (b), TC-3), made by the first rebuild on screen of a wrapper shown on an STA thread and not drawn
		///in software; null until then, and on every hidden, MTA or software wrapper.</summary>
		private ToothChartWpfView toothChartWpfView;
		///<summary>Wpf3D: the per-computer "Draw in software" switch (WpfDrawInSoftware).</summary>
		private bool _isWpfDrawnInSoftware;
		///<summary>Wpf3D: set when the live view failed to start on this wrapper, which then shows the software render; cleared when the engine is
		///released.</summary>
		private bool _isWpfLiveViewFailed;
		///<summary>Wpf3D: this wrapper's last 3D layer image, released on Dispose and when the engine changes.</summary>
		private ToothChartLayerCache _layerCacheWpf;
		///<summary>Wpf3D: set by every Invalidate (OnInvalidated), cleared when a scene is built (section 5.5), GetBitmap's included.</summary>
		private bool _isWpfSceneDirty;
		///<summary>Wpf3D: set by every mark, like _isWpfSceneDirty, but cleared only by the on-screen rebuild: a GetBitmap builds a scene without
		///updating the screen, so a wrapper refilled while hidden and then asked for its bitmap still rebuilds its screen image when it is shown.</summary>
		private bool _isWpfScreenStale;
		///<summary>Wpf3D: set when the first mark of a burst posts the on-screen rebuild; cleared by that rebuild, and when the handle goes (which can
		///take the posted rebuild with it).</summary>
		private bool _isWpfRebuildPosted;
		///<summary>Wpf3D: the newest picture asked for with RequestBitmapInBackground that has neither arrived nor been dropped, or null. UI thread
		///only.</summary>
		private ToothChartBitmapRequest _bitmapRequestLatest;
		///<summary>Wpf3D: the newest request whose scene no job has taken yet, or null. Set on the UI thread and taken by the render thread when it
		///starts a job (TakeBitmapScene), both under _lockBitmapRequest.</summary>
		private ToothChartBitmapRequest _bitmapRequestWaiting;
		///<summary>Wpf3D: counts the background jobs given up (the handle went, or the engine was released). A job takes a scene, and its result is
		///used, only while its generation is current. Changed under _lockBitmapRequest.</summary>
		private int _generationBitmapJob;
		///<summary>Guards _bitmapRequestWaiting and _generationBitmapJob: held only to read or set them, never while waiting for anything.</summary>
		private readonly object _lockBitmapRequest=new object();
		///<summary>Wpf3D: set when the start of a background job is posted; cleared when the posted start runs, or the job is given up.</summary>
		private bool _isBitmapStartPosted;
		///<summary>Wpf3D: set while a background job is queued or running on the render thread; cleared when its result reaches the UI thread, or the
		///job is given up.</summary>
		private bool _isBitmapJobInFlight;
		///<summary>Wpf3D: how many background pictures the renderer made for this wrapper, for tests.</summary>
		private int _countBitmapsMadeInBackground;
		///<summary>Wpf3D, with the live view: while a background job waits for WPF to draw the refill's frame (StartBitmapJobAfterLiveViewFrame), the
		///handler on CompositionTarget.Rendering, the live view's Dispatcher it was added on, and the timer that starts the job anyway; null otherwise.</summary>
		private EventHandler _eventHandlerRenderingBitmapJob;
		private System.Windows.Threading.Dispatcher _dispatcherBitmapJob;
		private System.Windows.Forms.Timer _timerBitmapJob;
		///<summary>Wpf3D, with the live view: WPF's render passes since the wait for the frame began or since the newest rebuild on screen, whichever came
		///later; the job starts in the second.</summary>
		private int _countRenderPassesBitmapJob;
		///<summary>The longest a background job waits for WPF to draw the live view's frame before it starts anyway: a window WPF does not draw (a
		///minimized one, a locked session's) raises no render pass.</summary>
		public const int MillisecondsMaxWaitForFrame=1000;
		///<summary>For tests only, false in the program: the wait for WPF's frame counts no render pass, as for a window WPF does not draw, so the job
		///starts after MillisecondsMaxWaitForFrame.</summary>
		public static bool IsRenderPassIgnoredForTests;
		///<summary>For the times only, false in the program: the background job does not wait for WPF's frame (it starts as soon as the posted start
		///runs, as TC-3b first built it), so the times can compare both.</summary>
		public static bool IsWaitForFrameSkippedForTests;
		private static List<string> _listFallbackMessages=new List<string>();
		///<summary>Every fallback message of this process, counted on past the 1000 that _listFallbackMessages keeps.</summary>
		private static int _countFallbackMessages;
		///<summary>The fallback messages of this process handed to the ToothChart log.</summary>
		private static int _countFallbackLinesWritten;
		private static readonly object _lockFallback=new object();

		///<summary></summary>
		[Category("Action"),Description("Occurs when the mouse goes up ending a drawing segment.")]
		public event ToothChartDrawEventHandler SegmentDrawn=null;
		///<summary></summary>
		[Category("Action"),Description("Occurs when the mouse goes up committing tooth selection.")]
		public event ToothChartSelectionEventHandler ToothSelectionsChanged=null;
		///<summary>A free-text label (ToothInitialType.Text) was dragged with CursorTool.MoveText to a new point in the chart's 410x307 space. The WPF
		///chart raises it on mouse up after a drag (TC-5, OnTextMoved); the other engines do not. The relay passes it on, and the Chart module saves
		///the move.</summary>
		[Category("Action"),Description("Occurs when the mouse goes up after moving a text label.")]
		public event ToothChartTextMovedEventHandler TextMoved=null;
		///<summary>The Draw tab's Change Color (CursorTool.ColorChanger) was pressed on a free-text label (ToothInitialType.Text), which now has the
		///pen colour (TC-5; Open Dental's manual: "change the color of an existing drawing or text"). The WPF chart raises it on mouse down; the
		///other engines do not. The relay passes it on, and the Chart module saves the colour.</summary>
		[Category("Action"),Description("Occurs when Change Color is pressed on a text label.")]
		public event ToothChartTextColorChangedEventHandler TextColorChanged=null;

		public ToothChartWrapper() {
			drawMode=DrawingMode.Simple2D;
			_engine=ToothChartEngine.Simple2D;
			tcData=new ToothChartData();
			InitializeComponent();
			ResetControls();
			Disposed+=ToothChartWrapper_Disposed;
		}

		#region Properties

		///<summary>The stored kind of the engine (computerpref.GraphicsSimple). Setting it starts that engine, falling back as GetFallbackDrawMode
		///says. While Engine is Wpf3D this reads DirectX: the stored value that the WPF chart replaces (TC-8 maps 0 and 2 to it), so a caller that
		///saves DrawMode never turns a computer to the 2D chart.</summary>
		public DrawingMode DrawMode{
			get{
				return drawMode;
			}
			set{
				if(Environment.OSVersion.Platform==PlatformID.Unix) {
					return;//disallow changing from simpleMode if platform is Unix
				}
				//do not break out if not changing mode.  ContrChart.InitializeOnStartup assumes this code will always run.
				if(_engine==ToothChartEngine.DirectX && value!=DrawingMode.DirectX){
					//If switching from from DirectX to another drawing mode,
					//then we need to cleanup DirectX resources in case the
					//chart is never switched back to DirectX mode.
					DisposeDirectX();//Calls CleanupDirectX() and device.Dispose().
				}
				if(_engine==ToothChartEngine.Wpf3D) {
					DisposeWpf3D();
				}
				//A mode that cannot start falls back to the next one (DirectX, OpenGL, Simple 2D) without an error dialog, logged once per process.
				//Read DrawMode afterwards to learn which mode started.
				drawMode=StartFirstWorkingMode(value,IsDirectXRuntimeAvailable(),TryStartDrawMode,LogFallbackOnce);
				_engine=(ToothChartEngine)drawMode;
			}
		}

		///<summary>The engine that draws the chart. Setting DirectX, Simple2D or OpenGL is the same as setting DrawMode. Setting Wpf3D starts the WPF
		///chart (docs/specs/wpf-tooth-chart-plan.md, section 5): its render thread and a check image the first time in the process, then its engine
		///control, the image host; the live view follows with the first rebuild on screen of a wrapper shown on an STA thread (WpfDrawInSoftware,
		///TC-3). When it cannot start,
		///the chart falls back to Simple2D with one log line (LogFallbackOnce). Read Engine afterwards to learn which engine started. In memory only:
		///no caller chooses Wpf3D yet (TC-8 maps the stored values to it).</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ToothChartEngine Engine {
			get {
				return _engine;
			}
			set {
				if(Environment.OSVersion.Platform==PlatformID.Unix) {
					return;//as DrawMode: Unix keeps the 2D chart
				}
				if(value!=ToothChartEngine.Wpf3D) {
					DrawMode=(DrawingMode)value;
					return;
				}
				if(_engine==ToothChartEngine.DirectX) {
					DisposeDirectX();
				}
				if(_engine==ToothChartEngine.Wpf3D) {
					DisposeWpf3D();
				}
				try {
					ToothChartRenderer.Instance.EnsureStarted();
					_engine=ToothChartEngine.Wpf3D;
					drawMode=DrawingMode.DirectX;
					ResetControls();
				}
				catch(Exception ex) {
					DisposeWpf3D();
					LogFallbackOnce(ToothChartFallbackKind.Wpf3DStart,"The "+ToothChartEngine.Wpf3D+" tooth chart could not start ("+ex.GetType().Name+": "
						+ex.Message+"). Using "+ToothChartEngine.Simple2D+".");
					_engine=ToothChartEngine.Simple2D;
					drawMode=DrawingMode.Simple2D;
					ResetControls();
				}
			}
		}

		///<summary>This data object holds nearly all information about what to draw.  It is normally acted on by public methods of the wrapper instead of being accessed directly.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ToothChartData TcData {
			get {
				return tcData;
			}
			set {
				if(_engine==ToothChartEngine.Simple2D) {
					toothChart2D.TcData=value;
				}
				else if(_engine==ToothChartEngine.DirectX) {
					toothChartDirectX.SetTcData(value);//Frees the old models' buffers in video memory and makes the new ones'.
				}
				else if(_engine==ToothChartEngine.OpenGL) {
					toothChartOpenGL.TcData=value;
				}
				else if(_engine==ToothChartEngine.Wpf3D) {
					toothChartImageHost.TcData=value;
					if(toothChartWpfView!=null) {
						toothChartWpfView.TcData=value;
					}
				}
				tcData=value;//Must set last so old tcdata can be cleaned up first.
				if(_engine==ToothChartEngine.Wpf3D) {
					MarkWpfSceneDirty();
				}
			}
		}

		///<summary>Valid values are 1-32 and A-Z.</summary>
		public List<string> SelectedTeeth {
			get {
				return tcData.SelectedTeeth;
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorBackground {
			get {
				return tcData.ColorBackground;
			}
			set {
				tcData.ColorBackground=value;
				Invalidate();
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorText{
			set {
				tcData.ColorText=value;
				Invalidate();
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorTextHighlight {
			set {
				tcData.ColorTextHighlight=value;
				Invalidate();
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorBackHighlight {
			set {
				tcData.ColorBackHighlight=value;
				Invalidate();
			}
		}

		///<summary>Set to true when using hardware rendering in OpenGL, and false otherwise. This will have no effect when in simple 2D graphics mode.</summary>
		[Browsable(false)]
		public bool UseHardware{
			get{
				return hardwareMode;
			}
			set{
				hardwareMode=value;
			}
		}

		[Browsable(false)]
		public bool AutoFinish{
			get{
				if(drawMode==DrawingMode.OpenGL) {
					return toothChartOpenGL.autoFinish;
				}
				else {
					return false;
				}
			}
			set{
				if(drawMode==DrawingMode.OpenGL) {
					toothChartOpenGL.autoFinish=value;
				}
			}
		}

		[Browsable(false)]
		public int PreferredPixelFormatNumber{
			get{
				return preferredPixelFormatNum;
			}
			set{
				preferredPixelFormatNum=value;
			}
		}

		[Browsable(false)]
		public ToothChartDirectX.DirectXDeviceFormat DeviceFormat {
			get {
				return deviceFormat;
			}
			set {
				deviceFormat=value;
			}
		}

		[Browsable(false)]
		public CursorTool CursorTool{
			get{
				return tcData.CursorTool;
			}
			set{
				tcData.CursorTool=value;
				//The embedded .cur files (Pen.cur, EraseCircle.cur, ColorChanger.cur), the arrow for the pointer; MoveText keeps the cursor it had, as
				//before. Loaded from this assembly whatever the runtime type, so a subclass of the wrapper gets them too. Made once per thread and kept
				//(ToothChartMouse.GetCursor, ToothChartWpfView.GetCursorWpf): a cursor that cannot be made is the arrow, logged once, and is tried again at
				//the next choice of its tool, so choosing a tool never throws.
				try {
					if(tcData.CursorTool==CursorTool.Pointer) {
						this.Cursor=Cursors.Default;
					}
					else if(tcData.CursorTool!=CursorTool.MoveText) {
						this.Cursor=ToothChartMouse.GetCursor(tcData.CursorTool);
					}
					if(toothChartWpfView!=null) {
						toothChartWpfView.SetCursorTool(tcData.CursorTool);//the live view's WPF content has a cursor of its own
					}
				}
				catch(Exception ex) {
					ex.DoNothing();//neither call throws; the tool is chosen whatever happens to its cursor
				}
				//if(drawMode!=DrawingMode.Simple2D) {
				//	toothChartOpenGL.CursorTool=value;
				//}
			}
		}

		///<summary>For the freehand drawing tool.</summary>
		[Browsable(false)]
		public Color ColorDrawing{
			set{
				tcData.ColorDrawing=value;
			}
		}
		
		[Browsable(false)]
		public bool PerioMode {
			get {
				return tcData.PerioMode;
			}
			set {
				if(_engine!=ToothChartEngine.DirectX && value==true) {
					throw new Exception("Only allowed in DirectX");//the WPF chart draws perio from TC-7
				}
				tcData.PerioMode=value;
				Invalidate();
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorBleeding {
			set {
				tcData.ColorBleeding=value;
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorSuppuration {
			set {
				tcData.ColorSuppuration=value;
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorFurcations {
			set {
				tcData.ColorFurcations=value;
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorFurcationsRed {
			set {
				tcData.ColorFurcationsRed=value;
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorGingivalMargin {
			set {
				tcData.ColorGingivalMargin=value;
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorCAL {
			set {
				tcData.ColorCAL=value;
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorMGJ {
			set {
				tcData.ColorMGJ=value;
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorProbing {
			set {
				tcData.ColorProbing=value;
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public Color ColorProbingRed {
			set {
				tcData.ColorProbingRed=value;
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public int RedLimitProbing {
			set {
				tcData.RedLimitProbing=value;
			}
		}

		///<summary></summary>
		[Browsable(false)]
		public int RedLimitFurcations {
			set {
				tcData.RedLimitFurcations=value;
			}
		}

		#endregion Properties

		protected override void OnInvalidated(InvalidateEventArgs e) {
			base.OnInvalidated(e);
			if(_engine==ToothChartEngine.Simple2D) {
				toothChart2D.Invalidate();
			}
			else if(_engine==ToothChartEngine.DirectX) {
				toothChartDirectX.Invalidate();
			}
			else if(_engine==ToothChartEngine.OpenGL) {
				toothChartOpenGL.Invalidate();
				//toothChartOpenGL.TaoDraw();
			}
			else if(_engine==ToothChartEngine.Wpf3D) {
				MarkWpfSceneDirty();
			}
		}

		private void ResetControls(){
			selectedTeeth=new string[0];
			this.Controls.Clear();
			if(_engine==ToothChartEngine.Wpf3D) {
				//The image host is a plain WinForms control, and no WPF object is made here: a wrapper made on any thread (the dashboard's MTA worker
				//included) gets the WPF chart. The renderer makes the images on its own thread. The live view comes with the first rebuild on screen of a
				//wrapper shown on an STA thread (RebuildWpfOnScreen, StartWpfLiveViewIfShown).
				//The font DirectX's PrepareForDirectX sets, which callers may read to measure text as the DirectX chart did.
				tcData.Font=new System.Drawing.Font("Arial",9f);
				_layerCacheWpf=new ToothChartLayerCache();
				toothChartImageHost=new ToothChartImageHost();
				toothChartImageHost.Dock=System.Windows.Forms.DockStyle.Fill;
				toothChartImageHost.Location=new System.Drawing.Point(0,0);
				toothChartImageHost.Name="toothChartImageHost";
				toothChartImageHost.SegmentDrawn+=new ToothChartDrawEventHandler(toothChart_SegmentDrawn);
				toothChartImageHost.ToothSelectionsChanged+=new ToothChartSelectionEventHandler(toothChart_ToothSelectionsChanged);
				toothChartImageHost.TextMoved+=new ToothChartTextMovedEventHandler(toothChart_TextMoved);
				toothChartImageHost.TextColorChanged+=new ToothChartTextColorChangedEventHandler(toothChart_TextColorChanged);
				toothChartImageHost.TcData=tcData;
				toothChartImageHost.LayerCache=_layerCacheWpf;
				toothChartImageHost.ActionChartChanged=Invalidate;
				toothChartImageHost.FuncGetFallbackBitmap=GetSimple2DBitmap;
				toothChartImageHost.SuspendLayout();
				this.Controls.Add(toothChartImageHost);
				ResetTeeth();
				toothChartImageHost.ResumeLayout();
				_isWpfSceneDirty=true;
				_isWpfScreenStale=true;
				PostWpfRebuildIfShown();//a wrapper that is already shown when it starts Wpf3D: its rebuild makes the live view
				return;
			}
			if(drawMode==DrawingMode.Simple2D){
				//this.Invalidate();
				toothChart2D=new ToothChart2D();
				toothChart2D.Dock = System.Windows.Forms.DockStyle.Fill;
				toothChart2D.Location = new System.Drawing.Point(0,0);
				toothChart2D.Name = "toothChart2D";
				//toothChart2D.Size = new System.Drawing.Size(719,564);//unnecessary?
				toothChart2D.SegmentDrawn+=new ToothChartDrawEventHandler(toothChart_SegmentDrawn);
				toothChart2D.ToothSelectionsChanged+=new ToothChartSelectionEventHandler(toothChart_ToothSelectionsChanged);
				toothChart2D.TcData=tcData;
				toothChart2D.SuspendLayout();
				this.Controls.Add(toothChart2D);
				ResetTeeth();
				toothChart2D.InitializeGraphics();
				toothChart2D.ResumeLayout();
			}
			else if(drawMode==DrawingMode.DirectX){
				//I noticed that this code executes when the program starts, then also when the Chart module is selected for the first time.
				//Thus the Chart graphic was loading twice before the user could see it.
				bool isInitialized=true;
				if(toothChartDirectX==null) {
					isInitialized=false;
				}
				if(isInitialized) {
					//Since the control is already initialized, reuse it.  This helps the load time of the Chart module and helps to prevent an issue.
					//This flag helps prevent a red X on the tooth chart when the Chart module is left open and the Windows user is switched then switched back.
				}
				else {
					toothChartDirectX=new ToothChartDirectX();//(hardwareMode,preferredPixelFormatNum);
				}
				//preferredPixelFormatNum=toothChart.SelectedPixelFormatNumber;
				//toothChartDirectX.ColorText=colorText;
				toothChartDirectX.Dock = System.Windows.Forms.DockStyle.Fill;
				toothChartDirectX.Location = new System.Drawing.Point(0,0);
				toothChartDirectX.Name = "toothChartDirectX";
				//toothChartDirectX.Size = new System.Drawing.Size(719,564);//unnecessary?
				toothChartDirectX.SegmentDrawn+=new ToothChartDrawEventHandler(toothChart_SegmentDrawn);
				toothChartDirectX.ToothSelectionsChanged+=new ToothChartSelectionEventHandler(toothChart_ToothSelectionsChanged);
				toothChartDirectX.TcData=tcData;
				toothChartDirectX.SuspendLayout();//Might help with the MDA debug error we used to get (if the option wasn't disabled in our compilers).
				this.Controls.Add(toothChartDirectX);
				ResetTeeth();
				toothChartDirectX.deviceFormat=deviceFormat;
				if(!isInitialized) {
					toothChartDirectX.InitializeGraphics();
				}
				toothChartDirectX.ResumeLayout();//Might help with the MDA debug error we used to get (if the option wasn't disabled in our compilers).
			}
			else if(drawMode==DrawingMode.OpenGL){
				toothChartOpenGL=new ToothChartOpenGL(hardwareMode,preferredPixelFormatNum);
				preferredPixelFormatNum=toothChartOpenGL.SelectedPixelFormatNumber;
				//toothChartOpenGL.ColorText=colorText;
				toothChartOpenGL.Dock = System.Windows.Forms.DockStyle.Fill;
				toothChartOpenGL.Location = new System.Drawing.Point(0,0);
				toothChartOpenGL.Name = "toothChartOpenGL";
				//toothChartOpenGL.Size = new System.Drawing.Size(719,564);//unnecessary?
				toothChartOpenGL.TcData=tcData;
				toothChartOpenGL.SegmentDrawn+=new ToothChartDrawEventHandler(toothChart_SegmentDrawn);
				toothChartOpenGL.ToothSelectionsChanged+=new ToothChartSelectionEventHandler(toothChart_ToothSelectionsChanged);
				toothChartOpenGL.SuspendLayout();
				this.Controls.Add(toothChartOpenGL);
				ResetTeeth();
				toothChartOpenGL.InitializeGraphics();//MakeDisplayLists();
				toothChartOpenGL.ResumeLayout();
			}
		}

		#region Draw mode fallback
		///<summary>The D3DX 9 helper library from Microsoft's legacy DirectX End-User Runtime. The DirectX chart needs it for its fonts and mesh
		///optimisation (SharpDX.Direct3D9.Font, D3DX.OptimizeFaces). Windows itself ships d3d9.dll but not this file.</summary>
		public const string D3DX_DLL="d3dx9_43.dll";
		private static bool? _isDirectXRuntimeAvailable;
		///<summary>The fallback kinds whose first message has been written to the log in this process.</summary>
		private static readonly HashSet<ToothChartFallbackKind> _hashSetKindsLogged=new HashSet<ToothChartFallbackKind>();

		///<summary>The mode the chart tries next when drawingMode cannot start: DirectX falls back to OpenGL, OpenGL to Simple 2D. Simple 2D has no
		///fallback and returns itself.</summary>
		public static DrawingMode GetFallbackDrawMode(DrawingMode drawingMode) {
			if(drawingMode==DrawingMode.DirectX) {
				return DrawingMode.OpenGL;
			}
			return DrawingMode.Simple2D;
		}

		///<summary>True when D3DX_DLL can be loaded in this process, so the DirectX chart can start. A clean Windows install lacks it. Checked once
		///per process.</summary>
		public static bool IsDirectXRuntimeAvailable() {
			if(_isDirectXRuntimeAvailable==null) {
				_isDirectXRuntimeAvailable=CanLoadLibrary(D3DX_DLL);
			}
			return _isDirectXRuntimeAvailable.Value;
		}

		///<summary>Starts the first mode that works, beginning with drawingModeRequested and following GetFallbackDrawMode: DirectX, then OpenGL,
		///then Simple 2D. DirectX is not tried when isDirectXRuntimeAvailable is false. funcTryStart starts one mode and returns true, or returns false
		///or throws when that mode cannot start. Simple 2D is the last resort and is not caught, as before. actionLog gets one line for each mode
		///that was skipped or failed. Returns the mode that started.</summary>
		public static DrawingMode StartFirstWorkingMode(DrawingMode drawingModeRequested,bool isDirectXRuntimeAvailable,Func<DrawingMode,bool> funcTryStart,
			Action<string> actionLog)
		{
			DrawingMode drawingMode=drawingModeRequested;
			if(drawingMode==DrawingMode.DirectX && !isDirectXRuntimeAvailable) {
				actionLog?.Invoke("The DirectX tooth chart cannot start because "+D3DX_DLL+" (Microsoft's DirectX End-User Runtime) is not installed. "
					+"Using "+GetFallbackDrawMode(drawingMode)+".");
				drawingMode=GetFallbackDrawMode(drawingMode);
			}
			while(drawingMode!=DrawingMode.Simple2D) {
				string reason="";
				bool isStarted=false;
				try {
					isStarted=funcTryStart(drawingMode);
				}
				catch(Exception ex) {
					reason=" ("+ex.GetType().Name+": "+ex.Message+")";
				}
				if(isStarted) {
					return drawingMode;
				}
				actionLog?.Invoke("The "+drawingMode+" tooth chart could not start"+reason+". Using "+GetFallbackDrawMode(drawingMode)+".");
				drawingMode=GetFallbackDrawMode(drawingMode);
			}
			funcTryStart(DrawingMode.Simple2D);
			return DrawingMode.Simple2D;
		}

		///<summary>Starts drawingMode on this wrapper. Throws when it cannot start, after releasing a half-made DirectX control so the next attempt at
		///DirectX starts clean.</summary>
		private bool TryStartDrawMode(DrawingMode drawingMode) {
			drawMode=drawingMode;
			_engine=(ToothChartEngine)drawingMode;
			try {
				ResetControls();
			}
			catch {
				if(drawingMode==DrawingMode.DirectX) {
					DisposeDirectX();
				}
				throw;
			}
			return true;
		}

		private void DisposeDirectX() {
			if(toothChartDirectX==null) {
				return;
			}
			try {
				Controls.Remove(toothChartDirectX);
				toothChartDirectX.Dispose();
			}
			catch(Exception ex) {
				ex.DoNothing();//A device that never started can fail to clean up; it is dropped either way.
			}
			toothChartDirectX=null;
		}

		///<summary>A fallback of a DrawingMode engine (ToothChartFallbackKind.DrawMode), as StartFirstWorkingMode reports it.</summary>
		internal static void LogFallbackOnce(string message) {
			LogFallbackOnce(ToothChartFallbackKind.DrawMode,message);
		}

		///<summary>Writes the first fallback of each kind in this process to the ToothChart log and ignores the rest of that kind, so a computer
		///without DirectX does not fill the log every time a chart is drawn, and a computer that has written one kind still writes the first of
		///another (a DirectX fallback line does not hide the WPF chart's first failure). Every message, written or not, is kept for
		///GetFallbackMessages. The WPF chart's renderer and image host log through here too.</summary>
		internal static void LogFallbackOnce(ToothChartFallbackKind kind,string message) {
			lock(_lockFallback) {
				if(_listFallbackMessages.Count>=1000) {
					_listFallbackMessages.RemoveAt(0);//a computer without DirectX adds one for every chart; the list stays small
				}
				_listFallbackMessages.Add(message);
				_countFallbackMessages++;
				if(!_hashSetKindsLogged.Add(kind)) {
					return;
				}
				_countFallbackLinesWritten++;
			}
			try {
				CodeBase.Logger.WriteLine(message,"ToothChart");
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
		}

		///<summary>The fallback messages of this process so far, in order, including those LogFallbackOnce did not write because one was written
		///already: the last 1000. For tests that check a chart started or drew without falling back, GetCountFallbackMessages and
		///GetFallbackMessagesSince, because once 1000 are kept the count of this list no longer grows.</summary>
		public static List<string> GetFallbackMessages() {
			lock(_lockFallback) {
				return new List<string>(_listFallbackMessages);
			}
		}

		///<summary>How many fallback messages this process has handed to the ToothChart log (CodeBase.Logger, which writes nothing in a unit test
		///process), for tests.</summary>
		public static int GetCountFallbackLinesWritten() {
			lock(_lockFallback) {
				return _countFallbackLinesWritten;
			}
		}

		///<summary>How many fallback messages this process has had so far, counting those GetFallbackMessages no longer keeps. Taken before a call
		///and passed to GetFallbackMessagesSince afterwards, it tells whether that call fell back: a WPF render that fails leaves Engine at Wpf3D and
		///returns the 2D chart's image, so only a new message tells.</summary>
		public static int GetCountFallbackMessages() {
			lock(_lockFallback) {
				return _countFallbackMessages;
			}
		}

		///<summary>The fallback messages added since GetCountFallbackMessages returned countBefore, in order, or an empty list. When more were added
		///than GetFallbackMessages keeps, the kept ones.</summary>
		public static List<string> GetFallbackMessagesSince(int countBefore) {
			lock(_lockFallback) {
				int countNew=Math.Min(_countFallbackMessages-countBefore,_listFallbackMessages.Count);
				if(countNew<=0) {
					return new List<string>();
				}
				return _listFallbackMessages.GetRange(_listFallbackMessages.Count-countNew,countNew);
			}
		}

		///<summary>True when ex is an error of the chart's graphics device: today SharpDX's exception (or a subclass), thrown by the DirectX chart when
		///its device is lost or a picture cannot be read back. Lets callers catch it without naming SharpDX:
		///catch(Exception ex) when(ToothChartWrapper.IsGraphicsDeviceException(ex)). False for null and for every other exception.</summary>
		public static bool IsGraphicsDeviceException(Exception ex) {
			return ex is SharpDX.SharpDXException;
		}

		///<summary>CAUTION: Runs slowly. May take minutes. Returns the string "invalid" if no suitable Direct X format can be found. Each format is
		///tried on an invisible chart on controlHost (TestDirectXFormat). Moved from FormGraphics (TC-2).</summary>
		public static string GetPreferredDirectXFormat(Control controlHost) {
			if(!ToothChartWrapper.IsDirectXRuntimeAvailable()) {
				return "invalid";//No format can work without d3dx9_43.dll, so don't spend minutes trying each one.
			}
			ToothChartDirectX.DirectXDeviceFormat[] directXDeviceFormatArray=ToothChartDirectX.GetStandardDeviceFormats();
			for(int i=0;i<directXDeviceFormatArray.Length;i++) {
				if(TestDirectXFormat(controlHost,directXDeviceFormatArray[i].ToString())) {
					return directXDeviceFormatArray[i].ToString();
				}
			}
			return "invalid";
		}

		///<summary>Returns true if the given directXFormat works for a DirectX tooth chart on the local computer: an invisible chart on controlHost
		///starts DirectX with it and gives a picture. Moved from FormGraphics (TC-2).</summary>
		public static bool TestDirectXFormat(Control controlHost,string directXFormat) {
			using(ToothChartWrapper toothChartWrapperTest=new ToothChartWrapper()) {//using blocks: this project is C# 7.3
				toothChartWrapperTest.Visible=false;
				//We add the invisible tooth chart to our form so that the device context will initialize properly
				//and our device creation test will then be accurate.
				controlHost.Controls.Add(toothChartWrapperTest);
				toothChartWrapperTest.DeviceFormat=new ToothChartDirectX.DirectXDeviceFormat(directXFormat);
				toothChartWrapperTest.DrawMode=DrawingMode.DirectX;//Creates the device.
				if(toothChartWrapperTest.DrawMode!=DrawingMode.DirectX) {
					//The chart falls back to OpenGL, then Simple 2D, when DirectX cannot start.
					controlHost.Controls.Remove(toothChartWrapperTest);
					return false;
				}
				//Now we check to be sure that one can draw and retrieve a screen shot from a DirectX control
				//using the specified device format.
				try {
					using(Bitmap bitmap=toothChartWrapperTest.GetBitmap()) {
					}
				}
				catch {
					controlHost.Controls.Remove(toothChartWrapperTest);
					return false;
				}
				controlHost.Controls.Remove(toothChartWrapperTest);
				return true;
			}
		}

		private static bool CanLoadLibrary(string fileName) {
			try {
				IntPtr intPtrModule=LoadLibrary(fileName);
				if(intPtrModule==IntPtr.Zero) {
					return false;
				}
				FreeLibrary(intPtrModule);
				return true;
			}
			catch(Exception ex) {
				ex.DoNothing();
				return false;
			}
		}

		[System.Runtime.InteropServices.DllImport("kernel32.dll",CharSet=System.Runtime.InteropServices.CharSet.Unicode,SetLastError=true)]
		private static extern IntPtr LoadLibrary(string lpFileName);

		[System.Runtime.InteropServices.DllImport("kernel32.dll",SetLastError=true)]
		private static extern bool FreeLibrary(IntPtr hModule);
		#endregion Draw mode fallback

		#region Wpf3D
		///<summary>Marks the WPF chart's scene dirty (every Invalidate comes here, section 5.5). On a wrapper that is shown, the first mark of a burst
		///posts one rebuild to the UI thread's message queue, and later marks post nothing; a refill's hundreds of Set calls give one image. A wrapper
		///that is not shown (the hidden forms of prints, sheets and the dashboard) posts nothing: GetBitmap builds its scene. The rebuild is posted
		///rather than tied to the next paint, because the engine control covers the whole wrapper, which may then get no paint of its own. Every mark
		///also marks the screen stale (_isWpfScreenStale), which only the rebuild clears.</summary>
		private void MarkWpfSceneDirty() {
			_isWpfSceneDirty=true;
			_isWpfScreenStale=true;
			PostWpfRebuildIfShown();
		}

		///<summary>On a wrapper that is shown, posts the on-screen rebuild unless one is posted already.</summary>
		private void PostWpfRebuildIfShown() {
			if(_isWpfRebuildPosted || toothChartImageHost==null || !IsShownForWpf()) {
				return;
			}
			_isWpfRebuildPosted=true;
			try {
				BeginInvoke(new Action(RebuildWpfOnScreen));
			}
			catch(InvalidOperationException ex) {
				ex.DoNothing();//the handle went away; nothing is shown, so nothing needs posting
				_isWpfRebuildPosted=false;
			}
		}

		///<summary>Wpf3D: a wrapper that comes on screen with a stale screen image (data set while it was hidden, even when a GetBitmap has built its scene
		///since), or that still lacks the live view it should show, posts its rebuild now, so it does not wait for the next Invalidate to show the chart.
		///The live view is not made here: the rebuild makes it when the message loop runs it (RebuildWpfOnScreen), so a wrapper that is shown and
		///disposed before that (the print wrapper ControlTreat.PrepImageForPrinting adds to the shown Treatment Plan module) never makes one.</summary>
		protected override void OnVisibleChanged(EventArgs e) {
			base.OnVisibleChanged(e);
			if(_engine==ToothChartEngine.Wpf3D && (_isWpfScreenStale || IsWpfLiveViewWanted())) {
				PostWpfRebuildIfShown();
			}
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);
			if(_engine==ToothChartEngine.Wpf3D && (_isWpfScreenStale || IsWpfLiveViewWanted())) {
				PostWpfRebuildIfShown();
			}
			if(_engine==ToothChartEngine.Wpf3D) {
				AskAgainForBitmapRequest();//a background picture whose job went with the old handle (GiveUpBitmapJob)
			}
		}

		#region Wpf3D live view and the software switch
		///<summary>The value of computerpref.DirectXFormat that turns the WPF chart's on-screen view to the software render on a computer (O-19 (b)):
		///empty, or a DirectX device format, means the live view. TC-8 stores and reads it; an older build finds no ';' in it and keeps its default
		///DirectX format (ToothChartDirectX.DirectXDeviceFormat).</summary>
		public const string DirectXFormatWpfSoftware="WpfSoftware";

		///<summary>True when a stored computerpref.DirectXFormat value is DirectXFormatWpfSoftware: the computer draws the WPF chart in software.</summary>
		public static bool IsWpfSoftwareFormat(string directXFormat) {
			return directXFormat!=null && directXFormat.Trim()==DirectXFormatWpfSoftware;
		}

		///<summary>The per-computer "Draw in software" switch of the WPF chart (O-19 (b); TC-8 sets it from computerpref.DirectXFormat and shows it in
		///Setup > Graphics). Off (the default): a shown wrapper on an STA thread shows the live view, which WPF's render thread draws as it draws every
		///WPF window of the process. On: it shows the renderer's image of the scene, the same image as GetBitmap's (the 3D layer supersampled and
		///smoothed), which no graphics driver can change (for a PC whose driver draws no WPF 3D, dotnet/wpf#7076). In OpenPmsDental.exe both are
		///software: the program sets RenderOptions.ProcessRenderMode to SoftwareOnly for the whole process (ProgramEntry.Main, from upstream), so the
		///live view is WPF's software rasterizer at 1x with no edge smoothing, not the graphics card, and the switch chooses between that and the
		///smoothed image (owner question O-34). Images (GetBitmap, prints, sheets, the dashboard) are the renderer's either way. Changing it on a
		///shown wrapper switches the view at once: on, the live view goes and the image host shows the chart's background until the software render of
		///the current data arrives (never the image it had before the live view, which may be another patient's chart); off, the posted rebuild makes
		///the live view and shows the current data in it.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool WpfDrawInSoftware {
			get {
				return _isWpfDrawnInSoftware;
			}
			set {
				if(_isWpfDrawnInSoftware==value) {
					return;
				}
				_isWpfDrawnInSoftware=value;
				if(_engine!=ToothChartEngine.Wpf3D) {
					return;
				}
				if(value) {
					bool hadLiveView=toothChartWpfView!=null;
					DisposeWpfLiveView();
					if(toothChartImageHost!=null) {
						if(hadLiveView) {
							toothChartImageHost.ClearImage();//its image is from before the live view, or none
						}
						toothChartImageHost.Visible=true;
					}
				}
				_isWpfScreenStale=true;
				PostWpfRebuildIfShown();
			}
		}

		///<summary>The WPF chart's live view, or null (not shown yet or its first rebuild on screen not run yet, hidden, on an MTA thread, drawn in
		///software, failed, or in another engine), for tests.</summary>
		[Browsable(false)]
		public ToothChartWpfView LiveViewWpf {
			get {
				return toothChartWpfView;
			}
		}

		///<summary>True while the wrapper should show a live view and has none: in Wpf3D, not drawn in software, no live view failed on it, and on an STA
		///thread. Whether it is shown is checked apart (IsShownForWpf).</summary>
		private bool IsWpfLiveViewWanted() {
			return _engine==ToothChartEngine.Wpf3D && !_isWpfDrawnInSoftware && !_isWpfLiveViewFailed && toothChartWpfView==null && toothChartImageHost!=null
				&& System.Threading.Thread.CurrentThread.GetApartmentState()==System.Threading.ApartmentState.STA;
		}

		///<summary>Makes the live view when the wrapper is shown and wants one (IsWpfLiveViewWanted). Only the on-screen rebuild calls it, and shows the
		///scene in the new view right after: the view is made when the message loop first runs the wrapper's rebuild, so a wrapper that is disposed
		///before that (a print wrapper added to a shown module and disposed after its GetBitmap) never makes one, and the view's first frame already
		///has the chart (its background is TcData's from the start). It takes the image host's place on screen (the image host stays, hidden, for the
		///software switch). When it cannot start, the wrapper keeps the image host and logs once (Wpf3DLiveView, FallBackFromWpfLiveView); it does not
		///try again until the engine is started again.</summary>
		private void StartWpfLiveViewIfShown() {
			if(!IsWpfLiveViewWanted() || !IsShownForWpf()) {
				return;
			}
			ToothChartWpfView toothChartWpfViewNew=null;
			try {
				toothChartWpfViewNew=new ToothChartWpfView();
				toothChartWpfViewNew.Dock=System.Windows.Forms.DockStyle.Fill;
				toothChartWpfViewNew.Location=new System.Drawing.Point(0,0);
				toothChartWpfViewNew.Name="toothChartWpfView";
				toothChartWpfViewNew.TcData=tcData;
				toothChartWpfViewNew.ActionChartChanged=Invalidate;
				toothChartWpfViewNew.SegmentDrawn+=new ToothChartDrawEventHandler(toothChart_SegmentDrawn);
				toothChartWpfViewNew.ToothSelectionsChanged+=new ToothChartSelectionEventHandler(toothChart_ToothSelectionsChanged);
				toothChartWpfViewNew.TextMoved+=new ToothChartTextMovedEventHandler(toothChart_TextMoved);
				toothChartWpfViewNew.TextColorChanged+=new ToothChartTextColorChangedEventHandler(toothChart_TextColorChanged);
				toothChartWpfViewNew.SetCursorTool(tcData.CursorTool);
				this.Controls.Add(toothChartWpfViewNew);
				toothChartWpfViewNew.BringToFront();
				toothChartWpfView=toothChartWpfViewNew;
				toothChartImageHost.Visible=false;
			}
			catch(Exception ex) {
				FallBackFromWpfLiveView(toothChartWpfViewNew,"start",ex);
			}
		}

		///<summary>The live view failed: it could not start (toothChartWpfViewFailed is the half-made view, or null) or could not show a scene. The wrapper
		///releases it, shows the image host from now on, cleared first so no older image shows until the software render of the current data arrives, and
		///logs once (Wpf3DLiveView). It makes no live view again until the engine is started again (DisposeWpf3D clears _isWpfLiveViewFailed).</summary>
		private void FallBackFromWpfLiveView(ToothChartWpfView toothChartWpfViewFailed,string action,Exception ex) {
			_isWpfLiveViewFailed=true;
			if(toothChartWpfViewFailed!=null) {
				ReleaseWpfLiveView(toothChartWpfViewFailed);
			}
			if(toothChartWpfView==toothChartWpfViewFailed) {
				toothChartWpfView=null;
			}
			if(toothChartImageHost!=null) {
				toothChartImageHost.ClearImage();
				toothChartImageHost.Visible=true;
			}
			LogFallbackOnce(ToothChartFallbackKind.Wpf3DLiveView,"The "+ToothChartEngine.Wpf3D+" tooth chart's live view could not "+action+" ("+ex.GetType().Name
				+": "+ex.Message+"). Showing its software render on screen.");
		}

		///<summary>Releases the live view, if there is one.</summary>
		private void DisposeWpfLiveView() {
			if(toothChartWpfView==null) {
				return;
			}
			ReleaseWpfLiveView(toothChartWpfView);
			toothChartWpfView=null;
		}

		///<summary>Takes a live view off the wrapper, detaches its WPF content (Child=null: ElementHost.Dispose leaves the content attached to the host's
		///adapter, with its cached 3D layer and models) and disposes it. Each step on its own, so one that throws does not keep the others from running.</summary>
		private void ReleaseWpfLiveView(ToothChartWpfView toothChartWpfViewOld) {
			try {
				Controls.Remove(toothChartWpfViewOld);
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
			try {
				toothChartWpfViewOld.Child=null;
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
			try {
				toothChartWpfViewOld.Dispose();
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
		}
		#endregion Wpf3D live view and the software switch

		///<summary>Wpf3D: DestroyHandle drops what BeginInvoke posted (RecreateHandle keeps it), so a posted rebuild may never run. The posted mark is
		///cleared, and the screen stays stale until a rebuild runs: the next mark, or the new handle, posts again. A kept rebuild that still runs
		///only renders once more. A background picture's job is given up the same way (GiveUpBitmapJob), and the new handle asks for the newest
		///request again.</summary>
		protected override void OnHandleDestroyed(EventArgs e) {
			_isWpfRebuildPosted=false;
			GiveUpBitmapJob();
			base.OnHandleDestroyed(e);
		}

		///<summary>True when the wrapper is on screen: it has a handle and it and its parents are visible.</summary>
		private bool IsShownForWpf() {
			return !IsDisposed && !Disposing && IsHandleCreated && Visible;
		}

		///<summary>The posted rebuild: always renders the on-screen image from the current data, even when a GetBitmap in between has built the scene
		///and cleared the dirty mark (a refill that ends in the dashboard's GetBitmap still updates the screen). Only this clears the posted mark
		///(besides the loss of the handle) and the stale mark. The first rebuild of a shown wrapper that wants a live view makes it (lazily: never before
		///the message loop runs). A live view whose ShowScene throws is released and the image host shows the scene instead, logged once.</summary>
		private void RebuildWpfOnScreen() {
			_isWpfRebuildPosted=false;
			if(_engine!=ToothChartEngine.Wpf3D || toothChartImageHost==null || IsDisposed) {
				return;
			}
			_isWpfScreenStale=false;
			ToothScene toothScene=BuildWpfScene();
			StartWpfLiveViewIfShown();
			if(toothChartWpfView!=null) {
				ToothChartWpfView toothChartWpfViewShown=toothChartWpfView;
				try {
					toothChartWpfViewShown.ShowScene(toothScene);//the live view (O-19 (b)); WPF's render thread draws it, in software in the program (O-34)
					_countRenderPassesBitmapJob=0;//a background picture still waiting waits for this new frame (StartBitmapJobAfterLiveViewFrame)
					return;
				}
				catch(Exception ex) {
					FallBackFromWpfLiveView(toothChartWpfViewShown,"show the chart",ex);
				}
			}
			toothChartImageHost.ShowScene(toothScene);
		}

		///<summary>The scene of the current data; clears the dirty mark.</summary>
		private ToothScene BuildWpfScene() {
			ToothScene toothScene=ToothSceneBuilder.Build(tcData);
			_isWpfSceneDirty=false;
			return toothScene;
		}

		///<summary>True while the WPF chart's data has changed since its last scene, for tests.</summary>
		[Browsable(false)]
		public bool IsWpfSceneDirty {
			get {
				return _isWpfSceneDirty;
			}
		}

		///<summary>The WPF chart's engine control, or null in the other engines, for tests.</summary>
		[Browsable(false)]
		public ToothChartImageHost ImageHostWpf {
			get {
				return toothChartImageHost;
			}
		}

		///<summary>GetBitmap of the WPF chart: the renderer's image of a scene built now from TcData, as the DirectX chart's GetBitmap draws TcData
		///as it is (so a change that skipped Invalidate is in it too). On any thread: the wrapper builds only the scene. A render that fails twice
		///gives the 2D chart's image of the same data, logged once, so a print never comes out without a chart.</summary>
		private Bitmap GetWpf3DBitmap() {
			ToothScene toothScene=BuildWpfScene();
			try {
				return ToothChartRenderer.Instance.Render(toothScene,_layerCacheWpf).ToBitmap();
			}
			catch(Exception ex) {
				LogFallbackOnce(ToothChartFallbackKind.Wpf3DRender,"The "+ToothChartEngine.Wpf3D+" tooth chart could not draw its image ("+ex.GetType().Name
					+": "+ex.Message+"). Using "+ToothChartEngine.Simple2D+" for this image.");
				return GetSimple2DBitmap();
			}
		}

		///<summary>The 2D chart's image of TcData at the wrapper's size, in the WPF chart's pixel format (Format24bppRgb).</summary>
		private Bitmap GetSimple2DBitmap() {
			using(ToothChart2D toothChart2DFallback=new ToothChart2D()) {
				toothChart2DFallback.Size=Size;
				toothChart2DFallback.TcData=tcData;
				toothChart2DFallback.InitializeGraphics();
				using(Bitmap bitmap2D=toothChart2DFallback.GetBitmap()) {
					Bitmap bitmap=new Bitmap(bitmap2D.Width,bitmap2D.Height,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.DrawImageUnscaled(bitmap2D,0,0);
					}
					return bitmap;
				}
			}
		}

		#region Wpf3D pictures made in the background
		///<summary>The picture GetBitmap would return now, made in the background so the caller does not wait for it (TC-3b: the Chart module's
		///picture for the Patient Dashboard, ControlChart.FillToothChart). Only the WPF chart does this. The scene of the current data is built now, on
		///this thread, as GetBitmap builds it (no WPF object is made here), and the renderer makes the picture on its own thread, at this size, with
		///this wrapper's 3D layer cache: the same renderer, size, colours and scene as GetBitmap's. The job is started by a message posted to this
		///thread, so the on-screen rebuild that the same refill posted earlier goes first, and with the live view after WPF has committed its frame
		///(StartBitmapJobAfterLiveViewFrame). The picture reaches the request's action on this thread when
		///it arrives (ToothChartBitmapRequest). Only the newest request is made: a newer one drops the older, and the render thread takes the newest
		///scene only when it starts the job, so a burst of refills makes one picture (and one more when a render was already running when the burst
		///began; its picture is dropped). A render that fails twice gives the 2D chart's picture of the data as it is when the failure arrives, logged
		///once, as GetBitmap falls back. Returns null, asking for nothing, when the picture cannot be made this way: another engine (DirectX, OpenGL and
		///Simple 2D keep their GetBitmap until TC-8), a wrapper with no handle to post to (the hidden forms of prints, sheets and the dashboard's own
		///chart), a call off the wrapper's thread, or an empty size. The caller then calls GetBitmap.</summary>
		public ToothChartBitmapRequest RequestBitmapInBackground() {
			if(_engine!=ToothChartEngine.Wpf3D || toothChartImageHost==null || IsDisposed || Disposing || !IsHandleCreated || InvokeRequired) {
				return null;
			}
			if(tcData.SizeControl.Width<1 || tcData.SizeControl.Height<1) {//the scene's size (ToothSceneBuilder)
				return null;
			}
			ToothScene toothScene=BuildWpfScene();
			ToothChartBitmapRequest toothChartBitmapRequest=new ToothChartBitmapRequest(toothScene);
			_bitmapRequestLatest?.Drop();
			_bitmapRequestLatest=toothChartBitmapRequest;
			lock(_lockBitmapRequest) {
				_bitmapRequestWaiting=toothChartBitmapRequest;
			}
			if(!PostBitmapJobStart()) {
				DropBitmapRequests();
				return null;
			}
			return toothChartBitmapRequest;
		}

		///<summary>How many background pictures (RequestBitmapInBackground) the renderer has made for this wrapper, for tests: the jobs whose result
		///reached this wrapper with a scene, whether it was handed on or dropped for a newer request.</summary>
		[Browsable(false)]
		public int CountBitmapsMadeInBackground {
			get {
				return _countBitmapsMadeInBackground;
			}
		}

		///<summary>Posts the start of a background job, unless one is posted or in flight (that job starts the next when its result arrives). False when
		///nothing could be posted (the handle went).</summary>
		private bool PostBitmapJobStart() {
			if(_isBitmapStartPosted || _isBitmapJobInFlight) {
				return true;
			}
			_isBitmapStartPosted=true;
			try {
				BeginInvoke(new Action(StartBitmapJobAfterLiveViewFrame));
				return true;
			}
			catch(InvalidOperationException ex) {
				ex.DoNothing();//the handle went away
				_isBitmapStartPosted=false;
				return false;
			}
		}

		///<summary>The posted start, which runs after the refill's on-screen rebuild. With the live view on screen the job waits until WPF has drawn the
		///refill's frame: WPF draws a frame of the process and the renderer's picture one after the other, so a picture started first, or right after
		///the frame was committed, held the frame back until the picture was done (measured in TC-3b). WPF raises CompositionTarget.Rendering in each
		///render pass on this thread: the first pass after this start commits the refill's frame, and WPF prepares the next only once its render thread
		///has presented that frame, so the job starts in the second pass. A newer refill's rebuild while the job waits counts again from its frame
		///(RebuildWpfOnScreen), so a burst of refills is drawn before its one picture. A window WPF does not draw (minimized, a locked session) raises
		///no pass, so the job starts after MillisecondsMaxWaitForFrame at the latest. Without a live view (the image host, a hidden wrapper) the job
		///starts now. The Dispatcher is the live view's, never one made here.</summary>
		private void StartBitmapJobAfterLiveViewFrame() {
			System.Windows.Threading.Dispatcher dispatcher=null;
			if(toothChartWpfView!=null) {
				dispatcher=System.Windows.Threading.Dispatcher.FromThread(System.Threading.Thread.CurrentThread);
			}
			if(dispatcher==null || dispatcher.HasShutdownStarted || IsWaitForFrameSkippedForTests) {
				StartBitmapJob();
				return;
			}
			if(_eventHandlerRenderingBitmapJob!=null) {
				return;//waiting already
			}
			_countRenderPassesBitmapJob=0;
			//Each handler acts only while it is the current wait's: a pass raised from a list taken before the wait ended does nothing.
			EventHandler eventHandlerRendering=null;
			eventHandlerRendering=(sender,e) => {
				if(eventHandlerRendering!=_eventHandlerRenderingBitmapJob || IsRenderPassIgnoredForTests) {
					return;
				}
				_countRenderPassesBitmapJob++;
				if(_countRenderPassesBitmapJob<2) {
					return;//the pass that commits the refill's frame (or a newer refill's: its rebuild counts again)
				}
				StopWaitingForFrame();
				StartBitmapJob();
			};
			System.Windows.Forms.Timer timer=null;
			_eventHandlerRenderingBitmapJob=eventHandlerRendering;
			_dispatcherBitmapJob=dispatcher;
			try {
				timer=new System.Windows.Forms.Timer();
				_timerBitmapJob=timer;
				timer.Interval=MillisecondsMaxWaitForFrame;
				timer.Tick+=(sender,e) => {
					if(timer!=_timerBitmapJob) {
						return;
					}
					StopWaitingForFrame();
					StartBitmapJob();
				};
				System.Windows.Media.CompositionTarget.Rendering+=eventHandlerRendering;
				timer.Start();
			}
			catch(Exception ex) {
				ex.DoNothing();
				StopWaitingForFrame();
				StartBitmapJob();
			}
		}

		///<summary>Ends the wait for WPF's frame (StartBitmapJobAfterLiveViewFrame): the handler leaves CompositionTarget.Rendering, on the live view's
		///thread, where it was added (a static event: a handler left on it would keep this wrapper and keep WPF drawing frames), and the timer goes.</summary>
		private void StopWaitingForFrame() {
			EventHandler eventHandlerRendering=_eventHandlerRenderingBitmapJob;
			System.Windows.Threading.Dispatcher dispatcher=_dispatcherBitmapJob;
			_eventHandlerRenderingBitmapJob=null;
			_dispatcherBitmapJob=null;
			if(eventHandlerRendering!=null && dispatcher!=null) {
				try {
					if(dispatcher.CheckAccess()) {
						System.Windows.Media.CompositionTarget.Rendering-=eventHandlerRendering;
					}
					else if(!dispatcher.HasShutdownStarted) {
						dispatcher.BeginInvoke(new Action(() => System.Windows.Media.CompositionTarget.Rendering-=eventHandlerRendering));
					}
				}
				catch(Exception ex) {
					ex.DoNothing();
				}
			}
			if(_timerBitmapJob!=null) {
				try {
					_timerBitmapJob.Stop();
					_timerBitmapJob.Dispose();
				}
				catch(Exception ex) {
					ex.DoNothing();
				}
				_timerBitmapJob=null;
			}
		}

		///<summary>True while a background job waits for WPF to draw the live view's frame, for tests.</summary>
		[Browsable(false)]
		public bool IsBitmapJobWaitingForFrame {
			get {
				return _eventHandlerRenderingBitmapJob!=null;
			}
		}

		///<summary>On the UI thread: queues a job that takes the newest waiting scene when the render thread starts it (RenderLatestAsync).</summary>
		private void StartBitmapJob() {
			_isBitmapStartPosted=false;
			if(_isBitmapJobInFlight || _engine!=ToothChartEngine.Wpf3D || IsDisposed) {
				return;
			}
			int generation;
			lock(_lockBitmapRequest) {
				if(_bitmapRequestWaiting==null) {
					return;
				}
				generation=_generationBitmapJob;
			}
			_isBitmapJobInFlight=true;
			try {
				ToothChartRenderer.Instance.RenderLatestAsync(() => TakeBitmapScene(generation),_layerCacheWpf,
					(scene,result) => OnBitmapJobDone(scene,result,generation));
			}
			catch(Exception ex) {
				//The render thread could not start: the waiting request gets what a failed render gives, the 2D chart's picture.
				ToothChartRenderResult resultFailed=new ToothChartRenderResult();
				resultFailed.Exception=ex;
				ReceiveBitmapJob(TakeBitmapScene(generation),resultFailed,generation);
			}
		}

		///<summary>First in a background job, on the render thread: the newest waiting request's scene, taken so that a later request waits for the next
		///job; null when none waits or the job was given up (a job given up must not take the scene of a request made after it).</summary>
		private ToothScene TakeBitmapScene(int generation) {
			lock(_lockBitmapRequest) {
				if(generation!=_generationBitmapJob || _bitmapRequestWaiting==null) {
					return null;
				}
				ToothScene toothScene=_bitmapRequestWaiting.Scene;
				_bitmapRequestWaiting=null;
				return toothScene;
			}
		}

		///<summary>On the render thread (or, for a job a shutdown aborted, the thread that ended it): hands the result to the UI thread without waiting,
		///or drops it when the wrapper is gone or has no handle (OnHandleDestroyed then gave the job up, and OnHandleCreated asks again).</summary>
		private void OnBitmapJobDone(ToothScene scene,ToothChartRenderResult result,int generation) {
			if(IsDisposed || Disposing || !IsHandleCreated) {
				return;
			}
			try {
				BeginInvoke(new Action(() => ReceiveBitmapJob(scene,result,generation)));
			}
			catch(InvalidOperationException ex) {
				ex.DoNothing();//the handle went away between the check and the call (ObjectDisposedException included)
			}
		}

		///<summary>On the UI thread: hands the picture to the newest request when the job rendered that request's scene, drops the result of a scene a
		///newer request replaced, and starts the next job when a request waits.</summary>
		private void ReceiveBitmapJob(ToothScene scene,ToothChartRenderResult result,int generation) {
			lock(_lockBitmapRequest) {
				if(generation!=_generationBitmapJob) {
					return;//a job given up when the handle went or the engine was released
				}
			}
			_isBitmapJobInFlight=false;
			if(IsDisposed || _engine!=ToothChartEngine.Wpf3D) {
				return;
			}
			if(scene!=null) {
				_countBitmapsMadeInBackground++;
			}
			ToothChartBitmapRequest toothChartBitmapRequest=_bitmapRequestLatest;
			try {
				if(scene!=null && toothChartBitmapRequest!=null && toothChartBitmapRequest.Scene==scene) {
					_bitmapRequestLatest=null;
					Bitmap bitmap=GetBitmapOfBackgroundResult(result);
					if(bitmap==null) {
						toothChartBitmapRequest.Drop();
					}
					else {
						toothChartBitmapRequest.Complete(bitmap);
					}
				}
			}
			finally {
				//Whatever the hand-off did, a request that waits still gets its job.
				bool isWaiting;
				lock(_lockBitmapRequest) {
					isWaiting=_bitmapRequestWaiting!=null;
				}
				if(isWaiting) {
					StartBitmapJob();//a newer request came while the job ran, or the job was aborted before it took one
				}
			}
		}

		///<summary>The picture of a background job's result, as GetBitmap returns it (GetWpf3DBitmap): the renderer's image, or, after a render that
		///failed twice or a bitmap of its pixels that could not be made (GDI+ under memory or handle pressure in the 32-bit process), the 2D chart's
		///image of the data (as it is now), logged once; null when neither could be made. Never throws: it runs in a message posted to the UI thread,
		///where an exception would reach Application.ThreadException and end the program.</summary>
		private Bitmap GetBitmapOfBackgroundResult(ToothChartRenderResult result) {
			Exception exception=result?.Exception;
			if(result!=null && result.Exception==null && result.PixelsBgra!=null) {
				try {
					return result.ToBitmap();
				}
				catch(Exception ex) {
					exception=ex;//the 2D picture below, as GetWpf3DBitmap gives when ToBitmap throws
				}
			}
			LogFallbackOnce(ToothChartFallbackKind.Wpf3DRender,"The "+ToothChartEngine.Wpf3D+" tooth chart could not draw its picture in the background ("
				+exception?.GetType().Name+": "+exception?.Message+"). Using "+ToothChartEngine.Simple2D+" for this picture.");
			try {
				return GetSimple2DBitmap();
			}
			catch(Exception ex) {
				ex.DoNothing();
				return null;
			}
		}

		///<summary>The handle went: a background job in flight or posted can no longer hand its result back (DestroyHandle drops what BeginInvoke
		///posted), so it is given up, and its late result, or its scene if it has not taken one yet, is left alone. The newest request stays, and the
		///new handle asks for it again (OnHandleCreated).</summary>
		private void GiveUpBitmapJob() {
			lock(_lockBitmapRequest) {
				_generationBitmapJob++;
			}
			StopWaitingForFrame();
			_isBitmapJobInFlight=false;
			_isBitmapStartPosted=false;
		}

		///<summary>A new handle: the newest request that has not arrived waits again and its job is posted.</summary>
		private void AskAgainForBitmapRequest() {
			if(_bitmapRequestLatest==null) {
				return;
			}
			lock(_lockBitmapRequest) {
				_bitmapRequestWaiting=_bitmapRequestLatest;
			}
			if(!PostBitmapJobStart()) {
				DropBitmapRequests();
			}
		}

		///<summary>Drops every background request (the engine is released, or nothing could be posted): no picture comes for them, and a job in flight is
		///given up.</summary>
		private void DropBitmapRequests() {
			lock(_lockBitmapRequest) {
				_generationBitmapJob++;
				_bitmapRequestWaiting=null;
			}
			StopWaitingForFrame();
			_isBitmapJobInFlight=false;
			_isBitmapStartPosted=false;
			_bitmapRequestLatest?.Drop();
			_bitmapRequestLatest=null;
		}
		#endregion Wpf3D pictures made in the background

		///<summary>Releases the WPF chart's engine controls (the image host and the live view), its 3D layer image and its background requests.</summary>
		private void DisposeWpf3D() {
			DropBitmapRequests();
			if(_layerCacheWpf!=null) {
				_layerCacheWpf.Dispose();
				_layerCacheWpf=null;
			}
			DisposeWpfLiveView();
			_isWpfLiveViewFailed=false;
			if(toothChartImageHost!=null) {
				try {
					Controls.Remove(toothChartImageHost);
					toothChartImageHost.Dispose();
				}
				catch(Exception ex) {
					ex.DoNothing();
				}
				toothChartImageHost=null;
			}
			_isWpfSceneDirty=false;
			_isWpfScreenStale=false;
			_isWpfRebuildPosted=false;
		}

		private void ToothChartWrapper_Disposed(object sender,EventArgs e) {
			DisposeWpf3D();
		}
		#endregion Wpf3D

		//This step usually happens when the DrawMode property is set on the tooth chart wrapper, by way of the ResetControls() function.
		///<summary>Not normally used unless we are just trying to make a copy of an existing directX control.</summary>
		//public void InitializeDirectXGraphics() {
		//  toothChartDirectX.InitializeGraphics();
		//}

		#region Public Methods

		///<summary>If ListToothGraphics is empty, then this fills it, including the complex process of loading all drawing points from local resources.  Or if not empty, then this resets all 32+20 teeth to default postitions, no restorations, etc. Primary teeth set to visible false.  Also clears selected.  Should surround with SuspendLayout / ResumeLayout.</summary>
		public void ResetTeeth() {
			//selectedTeeth=new string[0];
			//this will only happen once when program first loads.  Unfortunately, there is no way to tell what the drawMode is going to be when loading the graphics from the file.  So any other initialization must happen in resetControls.
			if(tcData.ListToothGraphics.Count==0) {
				tcData.ListToothGraphics.Clear();
				ToothGraphic tooth;
				for(int i=1;i<=32;i++) {
					tooth=new ToothGraphic(i.ToString());
					tooth.Visible=true;
					tcData.ListToothGraphics.Add(tooth);
					//primary
					if(Tooth.PermToPri(i.ToString())!="") {
						tooth=new ToothGraphic(Tooth.PermToPri(i.ToString()));
						tooth.Visible=false;
						tcData.ListToothGraphics.Add(tooth);
					}
				}
				tooth=new ToothGraphic("implant");
				tcData.ListToothGraphics.Add(tooth);
			}
			else {//list was already initially filled, but now user needs to reset it.
				for(int i=0;i<tcData.ListToothGraphics.Count;i++) {//loop through all perm and pri teeth.
					tcData.ListToothGraphics[i].Reset();
				}
			}
			tcData.SelectedTeeth.Clear();
			tcData.DrawingSegmentList=new List<ToothInitial>();
			tcData.ListFreeTexts=new List<ToothChartFreeText>();
			tcData.PointList=new List<PointF>();
			Invalidate();
		}

		///<summary>Moves position of tooth.  Rotations first in order listed, then translations.  Tooth doesn't get moved immediately, just when painting.  All changes are cumulative and are in addition to any previous translations and rotations.</summary>
		public void MoveTooth(string toothID,float rotate,float tipM,float tipB,float shiftM,float shiftO,float shiftB) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].ShiftM+=shiftM;
			tcData.ListToothGraphics[toothID].ShiftO+=shiftO;
			tcData.ListToothGraphics[toothID].ShiftB+=shiftB;
			tcData.ListToothGraphics[toothID].Rotate+=rotate;
			tcData.ListToothGraphics[toothID].TipM+=tipM;
			tcData.ListToothGraphics[toothID].TipB+=tipB;
			Invalidate();
		}

		///<summary>Sets the specified permanent tooth to primary. Works as follows: Sets ShowPrimaryLetter to true for the perm tooth.  Makes pri tooth visible=true.  Also repositions perm tooth by translating -Y.  Moves primary tooth slightly to M or D sometimes for better alignment.  And if 2nd primary molar, then because of the larger size, it must move all perm molars to distal.</summary>
		public void SetPrimary(string toothID) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			if(Tooth.IsPrimary(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].ShiftO-=12;
			if(ToothGraphic.IsValidToothID(Tooth.PermToPri(toothID))) {//if there's a primary tooth at this location
				tcData.ListToothGraphics[Tooth.PermToPri(toothID)].Visible=true;//show the primary.
				tcData.ListToothGraphics[toothID].ShowPrimaryLetter=true;
			}		
			//first pri mand molars, shift slightly to M
			if(toothID=="21") {
				tcData.ListToothGraphics["J"].ShiftM+=0.5f;
			}
			if(toothID=="28") {
				tcData.ListToothGraphics["S"].ShiftM+=0.5f;
			}
			//second pri molars are huge, so shift distally for space
			//and move all the perm molars distally too
			if(toothID=="4") {
				tcData.ListToothGraphics["A"].ShiftM-=0.5f;
				tcData.ListToothGraphics["1"].ShiftM-=1;
				tcData.ListToothGraphics["2"].ShiftM-=1;
				tcData.ListToothGraphics["3"].ShiftM-=1;
			}
			if(toothID=="13") {
				tcData.ListToothGraphics["J"].ShiftM-=0.5f;
				tcData.ListToothGraphics["14"].ShiftM-=1;
				tcData.ListToothGraphics["15"].ShiftM-=1;
				tcData.ListToothGraphics["16"].ShiftM-=1;
			}
			if(toothID=="20") {
				tcData.ListToothGraphics["K"].ShiftM-=1.2f;
				tcData.ListToothGraphics["17"].ShiftM-=2.3f;
				tcData.ListToothGraphics["18"].ShiftM-=2.3f;
				tcData.ListToothGraphics["19"].ShiftM-=2.3f;
			}
			if(toothID=="29") {
				tcData.ListToothGraphics["T"].ShiftM-=1.2f;
				tcData.ListToothGraphics["30"].ShiftM-=2.3f;
				tcData.ListToothGraphics["31"].ShiftM-=2.3f;
				tcData.ListToothGraphics["32"].ShiftM-=2.3f;
			}
			Invalidate();
		}

		///<summary>This is used for crowns and for retainers.  Crowns will be visible on missing teeth with implants.  Crowns are visible on F and O views, unlike ponics which are only visible on F view.  If the tooth is not visible, that should be set before this call, because then, this will set the root invisible. Tooth numbers 1-32 or A-T.  Supernumeraries not supported here yet.</summary>
		public void SetCrown(string toothID,Color color) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].IsCrown=true;
			if(!tcData.ListToothGraphics[toothID].Visible) {//tooth not visible, so set root invisible.
				tcData.ListToothGraphics[toothID].SetGroupVisibility(ToothGroupType.Cementum,false);
			}
			tcData.ListToothGraphics[toothID].SetSurfaceColors("MODBLFIV",color);
			tcData.ListToothGraphics[toothID].SetGroupColor(ToothGroupType.Enamel,color);
			tcData.ListToothGraphics[toothID].SetGroupColor(ToothGroupType.EnamelF,color);
			this.Invalidate();
		}

		///<summary>A series of color settings will result in the last ones entered overriding earlier entries.</summary>
		public void SetSurfaceColors(string toothID,string surfaces,Color color) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].SetSurfaceColors(surfaces,color);
			Invalidate();
		}

		///<summary>Used for missing teeth.  This should always be done before setting restorations, because a pontic will cause the tooth to become visible again except for the root.  So if setMissing after a pontic, then the pontic can't show.</summary>
		public void SetMissing(string toothID) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].Visible=false;
			Invalidate();
		}

		///<summary>This is just the same as SetMissing, except that it also hides the number from showing.  This is used, for example, if premolars are missing, and ortho has completely closed the space.  User will not be able to select this tooth because the number is hidden.</summary>
		public void SetHidden(string toothID) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].Visible=false;
			tcData.ListToothGraphics[toothID].HideNumber=true;
			Invalidate();
		}

		///<summary>This is used for any pontic, including bridges, full dentures, and partials.  It is usually used on a tooth that has already been set invisible.  This routine cuases the tooth to show again, but the root needs to be invisible.  Then, it sets the entire crown to the specified color.  If the tooth is already visible, then it does not set the root invisible.
		///Tooth numbers 1-32 or A-J or T-K.  Supernumeraries not supported here yet.</summary>
		public void SetPontic(string toothID,Color color) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].IsPontic=true;
			if(!tcData.ListToothGraphics[toothID].Visible) {
				//tooth not visible, but since IsPontic changes the visibility behavior of the tooth, we need to set the root invisible.
				tcData.ListToothGraphics[toothID].SetGroupVisibility(ToothGroupType.Cementum,false);
			}
			tcData.ListToothGraphics[toothID].SetSurfaceColors("MODBLFIV",color);
			tcData.ListToothGraphics[toothID].SetGroupColor(ToothGroupType.Enamel,color);
			tcData.ListToothGraphics[toothID].SetGroupColor(ToothGroupType.EnamelF,color);
			Invalidate();
		}

		///<summary>Root canals are initially not visible.  This routine sets the canals visible, changes the color to the one specified, and also sets the cementum for the tooth to be semitransparent so that the canals can be seen.  Also sets the IsRCT flag for the tooth to true.</summary>
		public void SetRCT(string toothID,Color color) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].IsRCT=true;
			tcData.ListToothGraphics[toothID].colorRCT=color;
			Invalidate();
		}

		///<summary>This draws a big red extraction X right on top of the tooth.  It's up to the calling application to figure out when it's appropriate to do this.  Even if the tooth has been marked invisible, there's a good chance that this will still get drawn because a tooth can be set visible again for the drawing the pontic.  So the calling application needs to figure out when it's appropriate to draw the X, and not set this otherwise.</summary>
		public void SetBigX(string toothID,Color color) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].DrawBigX=true;
			tcData.ListToothGraphics[toothID].colorX=color;
			Invalidate();
		}

		///<summary>Set this tooth to show a BU or post.</summary>
		public void SetBU(string toothID,Color color) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			//TcData.ListToothGraphics[toothID].IsBU=true;
			//TcData.ListToothGraphics[toothID].colorBU=color;
			//Buildups are now just another group, so
			tcData.ListToothGraphics[toothID].SetGroupVisibility(ToothGroupType.Buildup,true);
			tcData.ListToothGraphics[toothID].SetGroupColor(ToothGroupType.Buildup,color);
			Invalidate();
		}

		///<summary>Set this tooth to show an implant</summary>
		public void SetImplant(string toothID,Color color) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].IsImplant=true;
			tcData.ListToothGraphics[toothID].colorImplant=color;
			Invalidate();
		}

		///<summary>Set this tooth to show a sealant</summary>
		public void SetSealant(string toothID,Color color) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].IsSealant=true;
			tcData.ListToothGraphics[toothID].colorSealant=color;
			Invalidate();
		}

		///<summary>This will mostly only be successful on certain anterior teeth.   For others, it will just show F coloring.</summary>
		public void SetVeneer(string toothID,Color color) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].SetSurfaceColors("BFV",color);
			tcData.ListToothGraphics[toothID].SetGroupColor(ToothGroupType.EnamelF,color);
			tcData.ListToothGraphics[toothID].SetGroupColor(ToothGroupType.DF,color);
			tcData.ListToothGraphics[toothID].SetGroupColor(ToothGroupType.MF,color);
			tcData.ListToothGraphics[toothID].SetGroupColor(ToothGroupType.IF,color);
			Invalidate();
		}

		///<summary>Set this tooth to show a 'W' to indicate that the tooth is being watched.</summary>
		public void SetWatch(string toothID,Color color) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].Watch=true;
			tcData.ListToothGraphics[toothID].colorWatch=color;
			Invalidate();
		}

		///<summary>Paint type Text with text other than "W" (O-22, TC-5): the text is drawn beyond the root tip, in the watch mark's band, in the
		///colour. "W" is the watch mark (SetWatch; the relay chooses). A second call for the same tooth replaces the first; empty text draws nothing.
		///The WPF chart and the 2D chart draw it; DirectX and OpenGL do not (they leave with TC-9a).</summary>
		public void SetText(string toothID,Color color,string text) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].PaintText=text??"";
			tcData.ListToothGraphics[toothID].colorPaintText=color;
			Invalidate();
		}

		///<summary>A free-text label (a toothinitial row of type Text, TC-5) at location, the top left of its text in the chart's 410x307 drawing space,
		///in the colour. toothInitialNum names the row: a label added again with the same non-zero number replaces the first, and a move with
		///CursorTool.MoveText raises TextMoved with it. The WPF chart and the 2D chart draw the labels; only the WPF chart moves them.</summary>
		public void AddText(string text,PointF location,Color color,long toothInitialNum) {
			ToothChartFreeText freeText=new ToothChartFreeText(text,location,color,toothInitialNum);
			int index=toothInitialNum==0 ? -1 : tcData.ListFreeTexts.FindIndex(x => x.ToothInitialNum==toothInitialNum);
			if(index>=0) {
				tcData.ListFreeTexts[index]=freeText;
			}
			else {
				tcData.ListFreeTexts.Add(freeText);
			}
			Invalidate();
		}

		///<summary></summary>
		public void AddDrawingSegment(ToothInitial drawingSegment) {
			bool alreadyAdded=false;
			for(int i=0;i<tcData.DrawingSegmentList.Count;i++) {
				if(tcData.DrawingSegmentList[i].DrawingSegment==drawingSegment.DrawingSegment) {
					alreadyAdded=true;
					break;
				}
			}
			if(!alreadyAdded){
				tcData.DrawingSegmentList.Add(drawingSegment);
			}
			Invalidate();
			//toothChartOpenGL.AddDrawingSegment(drawingSegment);
		}

		///<summary>Returns a bitmap of what is showing in the control.  Used for printing.</summary>
		public Bitmap GetBitmap() {
			/*
			Bitmap bmp=new Bitmap(this.Width,this.Height);
			Graphics g=Graphics.FromImage(dummy);
			PaintEventArgs e=new PaintEventArgs(g,new Rectangle(0,0,dummy.Width,dummy.Height));
			if(simpleMode) {
				OnPaint(e);
				return dummy;
			}
			toothChartOpenGL.Render(e);
			Bitmap result=toothChartOpenGL.ReadFrontBuffer();
			g.Dispose();
			return result;
			return null;*/
			if(_engine==ToothChartEngine.Wpf3D) {
				return GetWpf3DBitmap();
			}
			if(drawMode==DrawingMode.Simple2D) {
				return toothChart2D.GetBitmap();
			}
			if(drawMode==DrawingMode.OpenGL) {
				return toothChartOpenGL.GetBitmap();
			}
			if(drawMode==DrawingMode.DirectX) {
				return toothChartDirectX.GetBitmap();
			}
			return null;
		}

		public void SetToothNumberingNomenclature(ToothNumberingNomenclature nomenclature) {
			tcData.ToothNumberingNomenclature=nomenclature;
			Invalidate();
		}

		public void SetMobility(string toothID,string mobility,Color color) {
			if(!ToothGraphic.IsValidToothID(toothID)) {
				return;
			}
			tcData.ListToothGraphics[toothID].Mobility=mobility;
			tcData.ListToothGraphics[toothID].colorMobility=color;
			Invalidate();
		}

		public void AddPerioMeasure(int intTooth,PerioSequenceType sequenceType,int mb,int b,int db,int ml,int l, int dl) {
			PerioMeasure pm=new PerioMeasure();
			pm.MBvalue=mb;
			pm.Bvalue=b;
			pm.DBvalue=db;
			pm.MLvalue=ml;
			pm.Lvalue=l;
			pm.DLvalue=dl;
			pm.IntTooth=intTooth;
			pm.SequenceType=sequenceType;
			tcData.ListPerioMeasure.Add(pm);
		}

		public void AddPerioMeasure(PerioMeasure pm) {
			tcData.ListPerioMeasure.Add(pm);
		}

		#endregion

		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
			tcData.SizeControl=this.Size;
			Invalidate();//Wpf3D: marks the scene dirty, and the next image has the new size
			if(_engine==ToothChartEngine.DirectX){
				//Fire the resize event for the DirectX tooth chart.
				//For some reason the Resize() and Size() events don't fire on the DirectX control
				//if you create them through the designer. Perhaps there is something wrong, but this
				//works for now.
				toothChartDirectX.SetSize(this.Size);
			}
		}

		public void SetSelected(string tooth_id,bool setValue) {
			tcData.SetSelected(tooth_id,setValue);
			Invalidate();
		}

		///<summary></summary>
		protected void OnSegmentDrawn(string drawingSegment){
			ToothChartDrawEventArgs tArgs=new ToothChartDrawEventArgs(drawingSegment);
			if(SegmentDrawn!=null){
				SegmentDrawn(this,tArgs);
			}
		}

		///<summary></summary>
		protected void OnToothSelectionsChanged(){
			if(ToothSelectionsChanged!=null){
				ToothSelectionsChanged(this);
			}
		}

		private void toothChart_SegmentDrawn(object sender,ToothChartDrawEventArgs e) {
			OnSegmentDrawn(e.DrawingSegement);
		}

		private void toothChart_ToothSelectionsChanged(object sender) {
			OnToothSelectionsChanged();
		}

		///<summary>Raises TextMoved: the free-text label toothInitialNum was dragged to pointF, in the chart's 410x307 space. The WPF chart's engine
		///controls raise it (the image host and the live view, from the shared mouse class, TC-5).</summary>
		internal void OnTextMoved(long toothInitialNum,PointF pointF) {
			TextMoved?.Invoke(this,new ToothChartTextMovedEventArgs(toothInitialNum,pointF));
		}

		private void toothChart_TextMoved(object sender,ToothChartTextMovedEventArgs e) {
			OnTextMoved(e.ToothInitialNum,e.Location);
		}

		private void toothChart_TextColorChanged(object sender,ToothChartTextColorChangedEventArgs e) {
			TextColorChanged?.Invoke(this,e);
		}

		//public void SimulateMouseDown(MouseEventArgs e) {

		//}
		

	}

	public enum CursorTool{
		Pointer,
		Pen,
		Eraser,
		ColorChanger,
		///<summary>Only supported in Sparks3D</summary>
		MoveText
	}

	///<summary></summary>
	public delegate void ToothChartDrawEventHandler(object sender,ToothChartDrawEventArgs e);
	///<summary></summary>
	public delegate void ToothChartSelectionEventHandler(object sender);
	///<summary>TextMoved: a free-text label was moved.</summary>
	public delegate void ToothChartTextMovedEventHandler(object sender,ToothChartTextMovedEventArgs e);
	///<summary>TextColorChanged: a free-text label was given the pen colour.</summary>
	public delegate void ToothChartTextColorChangedEventHandler(object sender,ToothChartTextColorChangedEventArgs e);

	///<summary>The label a TextMoved moved (toothinitial.ToothInitialNum) and where it was dropped, in the chart's 410x307 space, as the mouse gave
	///it: not clamped or rounded (ToothChartRelay.MoveTextLabel does that before saving).</summary>
	public class ToothChartTextMovedEventArgs {
		public long ToothInitialNum { get; }
		public PointF Location { get; }

		public ToothChartTextMovedEventArgs(long toothInitialNum,PointF location) {
			ToothInitialNum=toothInitialNum;
			Location=location;
		}
	}

	///<summary>The label a TextColorChanged recoloured (toothinitial.ToothInitialNum) and its new colour, the pen colour of the Draw tab
	///(ToothChartData.ColorDrawing).</summary>
	public class ToothChartTextColorChangedEventArgs {
		public long ToothInitialNum { get; }
		public Color ColorText { get; }

		public ToothChartTextColorChangedEventArgs(long toothInitialNum,Color colorText) {
			ToothInitialNum=toothInitialNum;
			ColorText=colorText;
		}
	}
	
	///<summary></summary>
	public class ToothChartDrawEventArgs{
		private string drawingSegment;
		//private bool isInsert;

		///<summary></summary>
		public ToothChartDrawEventArgs(string drawingSeg){//,bool isInsert){
			this.drawingSegment=drawingSeg;
			//this.isInsert=isInsert;
		}

		///<summary></summary>
		public string DrawingSegement{
			get{ 
				return drawingSegment;
			}
		}

		/*//<summary></summary>
		public bool IsInsert{
			get{ 
				return isInsert;
			}
		}*/
	}

	
}
