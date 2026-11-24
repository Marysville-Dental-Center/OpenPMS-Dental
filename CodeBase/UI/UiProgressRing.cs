// The busy indicator of the new look (docs/specs/ui-refresh-plan.md, section 4.5; package UI-LOAD, docs/port-notes/UI-LOAD.md): an indeterminate
// Fluent progress ring drawn with GDI+ in the Accent token on the control's own background, so it is sharp at every DPI. It replaces the
// animated pictures the program showed while it waited (the graph's loadingAnim.gif, the allocator warning's frame animator).
// - The ring is 32 px across with a 4 px round-capped stroke at 96 DPI (Fluent's ProgressRing; DiameterAt96 64 for Fluent's large ring),
//   scaled by UiLooks.GetScale and never larger than the control; centred in the control.
// - Indeterminate: one arc whose length breathes (20 to 260 degrees) while it turns, FRAMES_CYCLE frames of FRAME_MS each, so a cycle is
//   two seconds. The timer runs only while the control is in a window and visible (it has a parent and a window handle, and Visible, which
//   is false while any parent is hidden) and animation is on; hiding the control, taking it out of its parent, destroying its handle or
//   disposing it stops the timer. A ring that was built but never put in a window (no handle) does no work and holds nothing.
// - With Windows' "Show animations in Windows" off (SPI_GETCLIENTAREAANIMATION) nothing moves: a static ring, the whole circle as a track in
//   AccentSubtle with a quarter arc in Accent.
// - In a high-contrast theme the colours are Windows': the background Window, the arc Highlight (Accent's partner), the static track
//   WindowFrame.
// Windows Forms and System.Drawing only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CodeBase {
	///<summary>The new look's busy indicator: an indeterminate progress ring in the Accent token, animated by a timer only while visible, static
	///when Windows' animations are off. Show it where the program waits; hide it (Visible=false) when the wait is over.</summary>
	public class UiProgressRing : Control {
		///<summary>The ring's diameter at 96 DPI by default (Fluent's ProgressRing); DiameterAt96 sets another (Fluent's large ring is 64).</summary>
		public const int DIAMETER_96=32;
		///<summary>The ring's stroke at 96 DPI.</summary>
		public const int STROKE_96=4;
		///<summary>The time of one frame: about 30 frames a second.</summary>
		public const int FRAME_MS=33;
		///<summary>The frames of one cycle of the arc (two seconds).</summary>
		public const int FRAMES_CYCLE=60;
		///<summary>The shortest and the longest arc, in degrees.</summary>
		public const float SWEEP_MIN=20f;
		public const float SWEEP_MAX=260f;
		///<summary>The arc of the static ring (animations off), in degrees.</summary>
		public const float SWEEP_STATIC=90f;
		///<summary>SystemParametersInfo's SPI_GETCLIENTAREAANIMATION: Windows' "Show animations in Windows".</summary>
		private const int SPI_GETCLIENTAREAANIMATION=0x1042;
		///<summary>Guards the subscription to SystemEvents and the cached animation state.</summary>
		private static readonly object _lock=new object();
		///<summary>0 until first read; then 1 (animations off) or 2 (on), read again on UserPreferenceChanged.</summary>
		private static volatile int _animationState;
		private static bool _isSubscribed;
		///<summary>Test only (SetAnimationForTest): 0 none, 1 off, 2 on.</summary>
		private static volatile int _animationStateForTest;
		private readonly Timer _timer;
		private int _frame;
		private int _diameterAt96=DIAMETER_96;

		public UiProgressRing() {
			SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.UserPaint
				|ControlStyles.SupportsTransparentBackColor,true);
			SetStyle(ControlStyles.Selectable,false);
			TabStop=false;
			_timer=new Timer();
			_timer.Interval=FRAME_MS;
			_timer.Tick+=Timer_Tick;
		}

		#region Properties
		///<summary>The ring's diameter at 96 DPI: 32 (Fluent's ProgressRing), 64 for Fluent's large ring where the old picture was that big.
		///Scaled with the window; never larger than the control.</summary>
		[Category("Appearance")]
		[DefaultValue(DIAMETER_96)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public int DiameterAt96 {
			get { return _diameterAt96; }
			set {
				int diameter=Math.Max(4,value);
				if(_diameterAt96==diameter) {
					return;
				}
				_diameterAt96=diameter;
				Invalidate();
			}
		}

		///<summary>The frame of the animation now (0 to FRAMES_CYCLE-1). The timer advances it; a gallery or a test sets it to draw one frame.</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Frame {
			get { return _frame; }
			set {
				int frame=value%FRAMES_CYCLE;
				if(frame<0) {
					frame+=FRAMES_CYCLE;
				}
				if(_frame==frame) {
					return;
				}
				_frame=frame;
				Invalidate();
			}
		}

		///<summary>True while the timer runs: the control has a parent and a window handle, is visible, is not disposed, and Windows'
		///animations are on.</summary>
		[Browsable(false)]
		public bool IsAnimating {
			get { return _timer.Enabled; }
		}

		///<summary>Windows' "Show animations in Windows" (SPI_GETCLIENTAREAANIMATION), read once and again whenever Windows reports a changed user
		///preference. True when it cannot be read.</summary>
		public static bool IsAnimationOn {
			get {
				int stateForTest=_animationStateForTest;
				if(stateForTest!=0) {
					return stateForTest==2;
				}
				int state=_animationState;
				if(state!=0) {
					return state==2;
				}
				lock(_lock) {
					if(!_isSubscribed) {
						_isSubscribed=true;
						try {
							SystemEvents.UserPreferenceChanged+=SystemEvents_UserPreferenceChanged;
						}
						catch(Exception) {
							//No SystemEvents in this process: the value is read once and kept.
						}
					}
					bool isOn=ReadAnimationOn();
					_animationState=isOn ? 2 : 1;
					return isOn;
				}
			}
		}
		#endregion Properties

		#region Animation
		///<summary>Test only: Windows' animations as on (true) or off (false) for every ring of the process; null reads Windows again. Refuses
		///(throws) outside a test process.</summary>
		public static void SetAnimationForTest(bool? isOn) {
			if(!UiTheme.IsTestProcess()) {
				throw new InvalidOperationException("UiProgressRing.SetAnimationForTest is for test processes only.");
			}
			_animationStateForTest=isOn==null ? 0 : (isOn.Value ? 2 : 1);
		}

		private static void SystemEvents_UserPreferenceChanged(object sender,UserPreferenceChangedEventArgs e) {
			lock(_lock) {
				_animationState=ReadAnimationOn() ? 2 : 1;
			}
		}

		private static bool ReadAnimationOn() {
			try {
				bool isOn=true;
				if(SystemParametersInfo(SPI_GETCLIENTAREAANIMATION,0,ref isOn,0)) {
					return isOn;
				}
			}
			catch(Exception) {
				//Not on this Windows: animate.
			}
			return true;
		}

		[DllImport("user32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SystemParametersInfo(int uiAction,int uiParam,[MarshalAs(UnmanagedType.Bool)] ref bool pvParam,int fWinIni);

		///<summary>Starts the timer while the control is in a window (a parent and a window handle), visible, and animations are on; stops it
		///otherwise. Called whenever visibility, the parent or the handle changes. Without a handle the timer never runs: a ring that is built but
		///never shown (a graph made for a drag that is cancelled) would otherwise tick on the UI thread and, through the enabled timer, keep
		///itself and everything around it alive.</summary>
		public void UpdateAnimation() {
			bool isRun=!IsDisposed && !Disposing && Parent!=null && IsHandleCreated && Visible && IsAnimationOn;
			if(_timer.Enabled==isRun) {
				return;
			}
			_timer.Enabled=isRun;
			Invalidate();
		}

		private void Timer_Tick(object sender,EventArgs e) {
			if(IsDisposed || Parent==null || !IsHandleCreated || !Visible || !IsAnimationOn) {
				UpdateAnimation();
				return;
			}
			Frame=_frame+1;
		}

		protected override void OnVisibleChanged(EventArgs e) {
			base.OnVisibleChanged(e);
			UpdateAnimation();
		}

		protected override void OnParentChanged(EventArgs e) {
			base.OnParentChanged(e);
			UpdateAnimation();
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated(e);
			UpdateAnimation();
		}

		protected override void OnHandleDestroyed(EventArgs e) {
			base.OnHandleDestroyed(e);
			//IsHandleCreated is still true while the handle is being destroyed, so the timer is stopped here; OnHandleCreated starts it again
			//when the handle is recreated.
			_timer.Stop();
		}

		protected override void Dispose(bool disposing) {
			if(disposing) {
				_timer.Stop();
				_timer.Tick-=Timer_Tick;
				_timer.Dispose();
			}
			base.Dispose(disposing);
		}
		#endregion Animation

		#region Painting
		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			bool isHighContrast=UiTheme.IsHighContrast;
			if(isHighContrast) {
				e.Graphics.Clear(SystemColors.Window);
			}
			DrawRing(e.Graphics,ClientRectangle,UiLooks.GetScale(this),_frame,isAnimated:IsAnimationOn,isHighContrast,_diameterAt96);
		}

		///<summary>The ring's square inside the client rectangle: the diameter at 96 DPI scaled (never larger than the rectangle), centred.</summary>
		public static Rectangle GetRingRectangle(Rectangle rectangleClient,float scale,int diameter96=DIAMETER_96) {
			int diameter=Math.Min(UiTheme.ScaleI(diameter96,scale),Math.Min(rectangleClient.Width,rectangleClient.Height));
			diameter=Math.Max(0,diameter);
			int x=rectangleClient.X+(rectangleClient.Width-diameter)/2;
			int y=rectangleClient.Y+(rectangleClient.Height-diameter)/2;
			return new Rectangle(x,y,diameter,diameter);
		}

		///<summary>The stroke at a scale: STROKE_96 scaled, at least 1, and no wider than a quarter of the ring.</summary>
		public static int GetStrokeWidth(float scale,int diameter) {
			int width=Math.Max(1,UiTheme.ScaleI(STROKE_96,scale));
			return Math.Max(1,Math.Min(width,diameter/4));
		}

		///<summary>The arc of a frame: its start and sweep in degrees (GDI+'s, clockwise from three o'clock). The arc's middle turns twice a cycle
		///while its length breathes from SWEEP_MIN to SWEEP_MAX and back, so frame FRAMES_CYCLE is frame 0 again.</summary>
		public static void GetArc(int frame,out float angleStart,out float angleSweep) {
			double t=((frame%FRAMES_CYCLE+FRAMES_CYCLE)%FRAMES_CYCLE)/(double)FRAMES_CYCLE;
			double sin=Math.Sin(Math.PI*t);
			angleSweep=(float)(SWEEP_MIN+(SWEEP_MAX-SWEEP_MIN)*sin*sin);
			angleStart=(float)((-90+720*t-angleSweep/2)%360);
		}

		///<summary>Draws the ring into the rectangle (the background is the caller's): the frame's arc in Accent, or, when isAnimated is false, the
		///static ring (an AccentSubtle track with a quarter arc in Accent). In high contrast Highlight on a WindowFrame track.</summary>
		public static void DrawRing(Graphics g,Rectangle rectangleClient,float scale,int frame,bool isAnimated,bool isHighContrast,
			int diameter96=DIAMETER_96)
		{
			Rectangle rectangleRing=GetRingRectangle(rectangleClient,scale,diameter96);
			if(rectangleRing.Width<2) {
				return;
			}
			int widthStroke=GetStrokeWidth(scale,rectangleRing.Width);
			RectangleF rectangleArc=new RectangleF(rectangleRing.X+widthStroke/2f,rectangleRing.Y+widthStroke/2f,
				rectangleRing.Width-widthStroke,rectangleRing.Height-widthStroke);
			Color colorArc=UiTheme.GetColor(EnumUiColor.Accent,isHighContrast);
			SmoothingMode smoothingMode=g.SmoothingMode;
			PixelOffsetMode pixelOffsetMode=g.PixelOffsetMode;
			g.SmoothingMode=SmoothingMode.AntiAlias;
			g.PixelOffsetMode=PixelOffsetMode.HighQuality;
			try {
				if(!isAnimated) {
					Color colorTrack=isHighContrast ? SystemColors.WindowFrame : UiTheme.GetColor(EnumUiColor.AccentSubtle,false);
					using(Pen penTrack=new Pen(colorTrack,widthStroke)) {
						g.DrawEllipse(penTrack,rectangleArc);
					}
					using(Pen penArc=new Pen(colorArc,widthStroke)) {
						penArc.StartCap=LineCap.Round;
						penArc.EndCap=LineCap.Round;
						g.DrawArc(penArc,rectangleArc,-90f,SWEEP_STATIC);
					}
					return;
				}
				GetArc(frame,out float angleStart,out float angleSweep);
				using(Pen pen=new Pen(colorArc,widthStroke)) {
					pen.StartCap=LineCap.Round;
					pen.EndCap=LineCap.Round;
					g.DrawArc(pen,rectangleArc,angleStart,angleSweep);
				}
			}
			finally {
				g.SmoothingMode=smoothingMode;
				g.PixelOffsetMode=pixelOffsetMode;
			}
		}
		#endregion Painting
	}
}
