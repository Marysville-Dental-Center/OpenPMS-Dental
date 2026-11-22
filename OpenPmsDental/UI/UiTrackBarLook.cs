// UI-13 (docs/port-notes/UI-13.md, the owner's G-UI3-ui12p row "Modernize horizontal sliding bar" on ControlChart): the stock track bar
// (System.Windows.Forms.TrackBar: ControlChart's procedure date slider and the sheet editor's) drawn in the Windows 11 Fluent slider look of
// UiSliderPaint (CodeBase/UI): a thin rounded track in Stroke, the part from the minimum to the thumb in Accent, a round thumb (an Accent dot in a
// white ring), quiet tick marks in StrokeStrong where Windows puts its ticks. Windows keeps all of the track bar's behaviour (the keys, the
// clicks, the drag, the values and events); after Windows has painted it (in the same message) the whole client area is painted again in the
// look from Windows' own geometry (TBM_GETCHANNELRECT, TBM_GETTHUMBRECT), so the thumb is drawn where Windows hits it. Windows repaints a track
// bar outside WM_PAINT too (a value set in code, the mouse): after every message the geometry and the mouse state are compared with what was
// painted last and painted again when they changed. UiInputFrame.AttachOne attaches it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI {
	///<summary>Paints UiSliderPaint's slider look over a stock TrackBar.</summary>
	public class UiTrackBarLook : NativeWindow {
		#region Fields
		private const int WM_PAINT=0x000F;
		private const int WM_NCDESTROY=0x0082;
		private const int WM_MOUSEMOVE=0x0200;
		private const int WM_LBUTTONDOWN=0x0201;
		private const int WM_LBUTTONUP=0x0202;
		private const int WM_CAPTURECHANGED=0x0215;
		private const int WM_MOUSELEAVE=0x02A3;
		private const int WM_PRINT=0x0317;
		private const int WM_PRINTCLIENT=0x0318;
		private const int WM_USER=0x0400;
		private const int TBM_GETTHUMBRECT=WM_USER+25;
		private const int TBM_GETCHANNELRECT=WM_USER+26;
		private const int TME_LEAVE=0x0002;
		private static readonly LayoutManagerForms _layoutManager=new LayoutManagerForms();
		private static readonly ConditionalWeakTable<Control,UiTrackBarLook> _table=new ConditionalWeakTable<Control,UiTrackBarLook>();
		private readonly TrackBar _trackBar;
		private bool _isHover;
		private bool _isPressed;
		private bool _isMouseTracked;
		private string _stateLast="";
		#endregion Fields

		private UiTrackBarLook(TrackBar trackBar) {
			_trackBar=trackBar;
			if(trackBar.IsHandleCreated) {
				AssignHandle(trackBar.Handle);
			}
			trackBar.HandleCreated+=(sender,e) => AssignHandle(_trackBar.Handle);
			trackBar.HandleDestroyed+=(sender,e) => ReleaseHandle();
		}

		#region Methods - Public
		public static void Attach(Control control) {
			if(!(control is TrackBar trackBar) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiTrackBarLook(trackBar));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>The whole track bar in the look in g (origin at the control's top left): the background, the track and its filled part, the
		///ticks and the thumb, from the geometry Windows reports for the control's handle.</summary>
		public static void PaintTrackBar(Graphics g,TrackBar trackBar,EnumUiButtonState buttonStateThumb,bool isFocused) {
			Geometry geometry=GetGeometry(trackBar);
			Color colorBack=trackBar.BackColor;
			if(colorBack.A<255) {
				colorBack=trackBar.Parent?.BackColor ?? SystemColors.Control;
				if(colorBack.A<255) {
					colorBack=SystemColors.Control;
				}
			}
			using(SolidBrush brush=new SolidBrush(colorBack)) {
				g.FillRectangle(brush,new Rectangle(0,0,trackBar.Width,trackBar.Height));
			}
			if(geometry==null) {
				return;
			}
			float scale=GetScale(trackBar);
			bool isEnabled=trackBar.Enabled;
			UiSliderPaint.DrawSliderTrack(g,geometry.PointStart,geometry.PointEnd,geometry.PointThumb,geometry.IsVertical,isEnabled,scale);
			if(trackBar.TickStyle!=TickStyle.None && trackBar.Maximum>trackBar.Minimum) {
				int frequency=Math.Max(1,trackBar.TickFrequency);
				for(int value=trackBar.Minimum;;value+=frequency) {
					if(value>trackBar.Maximum) {
						value=trackBar.Maximum;
					}
					Point point=geometry.GetPoint(value,trackBar.Minimum,trackBar.Maximum);
					if(trackBar.TickStyle==TickStyle.BottomRight || trackBar.TickStyle==TickStyle.Both) {
						Point pointTick=geometry.IsVertical ? new Point(geometry.RectangleThumb.Right+1,point.Y) : new Point(point.X,geometry.RectangleThumb.Bottom+1);
						UiSliderPaint.DrawSliderTick(g,pointTick,geometry.IsVertical,isAfter:true,isEnabled,scale);
					}
					if(trackBar.TickStyle==TickStyle.TopLeft || trackBar.TickStyle==TickStyle.Both) {
						Point pointTick=geometry.IsVertical ? new Point(geometry.RectangleThumb.Left-1,point.Y) : new Point(point.X,geometry.RectangleThumb.Top-1);
						UiSliderPaint.DrawSliderTick(g,pointTick,geometry.IsVertical,isAfter:false,isEnabled,scale);
					}
					if(value==trackBar.Maximum) {
						break;
					}
				}
			}
			UiSliderPaint.DrawSliderThumb(g,geometry.PointThumb,isEnabled ? buttonStateThumb : EnumUiButtonState.Disabled,isFocused && isEnabled,scale);
		}
		#endregion Methods - Public

		#region Methods - Private
		///<summary>The scale of the look: the program's dpi and zoom, as the radio button's look takes it.</summary>
		private static float GetScale(Control control) {
			return Math.Max(1f,_layoutManager.ScaleMy());
		}

		protected override void WndProc(ref Message m) {
			int msg=m.Msg;
			base.WndProc(ref m);
			try {
				switch(msg) {
					case WM_NCDESTROY:
						return;
					case WM_PRINT:
					case WM_PRINTCLIENT:
						using(Graphics g=Graphics.FromHdc(m.WParam)) {
							PaintTrackBar(g,_trackBar,EnumUiButtonState.Normal,isFocused:false);
						}
						return;
					case WM_MOUSEMOVE:
						if(!_isMouseTracked) {
							_isMouseTracked=true;
							TRACKMOUSEEVENT trackMouseEvent=new TRACKMOUSEEVENT();
							trackMouseEvent.cbSize=Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
							trackMouseEvent.dwFlags=TME_LEAVE;
							trackMouseEvent.hwndTrack=Handle;
							TrackMouseEvent(ref trackMouseEvent);
						}
						_isHover=IsOnThumb(m.LParam);
						break;
					case WM_LBUTTONDOWN:
						_isPressed=IsOnThumb(m.LParam);
						break;
					case WM_LBUTTONUP:
					case WM_CAPTURECHANGED:
						_isPressed=false;
						break;
					case WM_MOUSELEAVE:
						_isMouseTracked=false;
						_isHover=false;
						break;
				}
				PaintOnWindow(force:msg==WM_PAINT);
			}
			catch(Exception) {
				//The look is only a picture: Windows' own drawing stays when it cannot be painted.
			}
		}

		private bool IsOnThumb(IntPtr lParam) {
			Point point=new Point((short)((long)lParam & 0xFFFF),(short)(((long)lParam>>16) & 0xFFFF));
			Geometry geometry=GetGeometry(_trackBar);
			if(geometry==null) {
				return false;
			}
			Rectangle rectangleThumb=UiSliderPaint.GetSliderThumbRectangle(geometry.PointThumb,GetScale(_trackBar));
			return rectangleThumb.Contains(point) || geometry.RectangleThumb.Contains(point);
		}

		private EnumUiButtonState GetThumbState() {
			if(_isPressed) {
				return EnumUiButtonState.Pressed;
			}
			return _isHover ? EnumUiButtonState.Hover : EnumUiButtonState.Normal;
		}

		private void PaintOnWindow(bool force) {
			if(Handle==IntPtr.Zero) {
				return;
			}
			Geometry geometry=GetGeometry(_trackBar);
			string state=(geometry==null ? "" : geometry.RectangleThumb+","+geometry.PointStart+","+geometry.PointEnd)+","+_trackBar.Size+","
				+_trackBar.Enabled+","+_trackBar.Focused+","+GetThumbState();
			if(!force && state==_stateLast) {
				return;
			}
			_stateLast=state;
			using(Graphics g=_trackBar.CreateGraphics()) {
				PaintTrackBar(g,_trackBar,GetThumbState(),_trackBar.Focused);
			}
		}

		///<summary>The track bar's geometry from Windows: the channel's centre line from the thumb's centre at the minimum to its centre at the
		///maximum, and the thumb. Null when Windows has none.</summary>
		private static Geometry GetGeometry(TrackBar trackBar) {
			if(!trackBar.IsHandleCreated) {
				return null;
			}
			RECT rectChannel=new RECT();
			RECT rectThumb=new RECT();
			SendMessage(trackBar.Handle,TBM_GETCHANNELRECT,IntPtr.Zero,ref rectChannel);
			SendMessage(trackBar.Handle,TBM_GETTHUMBRECT,IntPtr.Zero,ref rectThumb);
			Rectangle rectangleThumb=Rectangle.FromLTRB(rectThumb.Left,rectThumb.Top,rectThumb.Right,rectThumb.Bottom);
			if(rectangleThumb.Width<=0 || rectangleThumb.Height<=0) {
				return null;
			}
			Geometry geometry=new Geometry();
			geometry.IsVertical=trackBar.Orientation==Orientation.Vertical;
			geometry.RectangleThumb=rectangleThumb;
			Point pointThumb=new Point(rectangleThumb.X+rectangleThumb.Width/2,rectangleThumb.Y+rectangleThumb.Height/2);
			if(geometry.IsVertical) {
				//Windows reports a vertical track bar's channel as if it were horizontal: its left and right are the channel's top and bottom.
				int top=rectChannel.Left;
				int bottom=rectChannel.Right;
				int x=(rectChannel.Top+rectChannel.Bottom)/2;
				int half=rectangleThumb.Height/2;
				geometry.PointStart=new Point(x,top+half);
				geometry.PointEnd=new Point(x,Math.Max(top+half,bottom-(rectangleThumb.Height-half)));
				geometry.PointThumb=new Point(x,pointThumb.Y);
			}
			else {
				int y=(rectChannel.Top+rectChannel.Bottom)/2;
				int half=rectangleThumb.Width/2;
				geometry.PointStart=new Point(rectChannel.Left+half,y);
				geometry.PointEnd=new Point(Math.Max(rectChannel.Left+half,rectChannel.Right-(rectangleThumb.Width-half)),y);
				geometry.PointThumb=new Point(pointThumb.X,y);
			}
			return geometry;
		}

		private class Geometry {
			public bool IsVertical;
			public Rectangle RectangleThumb;
			public Point PointStart;
			public Point PointEnd;
			public Point PointThumb;

			///<summary>The point on the centre line for a value between minimum and maximum.</summary>
			public Point GetPoint(int value,int minimum,int maximum) {
				double fraction=maximum>minimum ? (value-minimum)/(double)(maximum-minimum) : 0;
				if(IsVertical) {
					return new Point(PointStart.X,PointStart.Y+(int)Math.Round((PointEnd.Y-PointStart.Y)*fraction));
				}
				return new Point(PointStart.X+(int)Math.Round((PointEnd.X-PointStart.X)*fraction),PointStart.Y);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT {
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct TRACKMOUSEEVENT {
			public int cbSize;
			public int dwFlags;
			public IntPtr hwndTrack;
			public int dwHoverTime;
		}

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd,int msg,IntPtr wParam,ref RECT lParam);

		[DllImport("user32.dll")]
		private static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);
		#endregion Methods - Private
	}
}
