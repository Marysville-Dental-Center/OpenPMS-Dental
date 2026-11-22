// UI-13 (docs/port-notes/UI-13.md, the owner's G-UI3-ui12p rows "Scrolling still using old style" on FormProvEdit and "Scrolling bar style is
// old" on ControlAccount, and "make sure you modernize all element types"): every scroll bar the program shows gets the one Fluent look of
// UiScrollPaint (CodeBase/UI): the stock scroll bar controls (the grid's two, the program's list box's, the combo box picker's, the old tables',
// the appointment book's, DataGridView's) and the scroll bars Windows draws in a window's frame (multi-line text boxes and rich text boxes,
// stock list boxes, tree views, list views, scrolling panels, the print preview). Windows keeps all of the scroll bar's behaviour: the hit
// test, the arrows' repeat, the page clicks, the thumb drag, the keys, the wheel, the events and the values are Windows' own. Only the picture
// changes: after Windows has drawn a bar (in the same message, so the screen shows one frame), the look is painted over it from the bar's own
// geometry (GetScrollBarInfo: the bar's rectangle, the arrow length, the thumb's start and end), so what is drawn is exactly where Windows hits.
// Windows draws a scroll bar outside WM_PAINT too (a SetScrollInfo with redraw, the hot tracking under the mouse, a drag): after every message
// the bars' geometry and the mouse state are compared with what was painted last and painted again when they changed. UiInputFrame.AttachTree
// attaches it with the input frames. A bar Windows does not show (hidden, off screen) is not painted.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeBase;

namespace OpenPmsDental.UI {
	///<summary>Paints UiScrollPaint's look over a stock scroll bar control or over the scroll bars in a window's frame.</summary>
	public class UiScrollLook : NativeWindow {
		#region Fields
		private const int WM_SIZE=0x0005;
		private const int WM_SETFOCUS=0x0007;
		private const int WM_KILLFOCUS=0x0008;
		private const int WM_ENABLE=0x000A;
		private const int WM_PAINT=0x000F;
		private const int WM_WINDOWPOSCHANGED=0x0047;
		private const int WM_STYLECHANGED=0x007D;
		private const int WM_NCDESTROY=0x0082;
		private const int WM_NCPAINT=0x0085;
		private const int WM_NCACTIVATE=0x0086;
		private const int WM_NCMOUSEMOVE=0x00A0;
		private const int WM_NCLBUTTONDOWN=0x00A1;
		private const int WM_NCLBUTTONDBLCLK=0x00A3;
		private const int WM_HSCROLL=0x0114;
		private const int WM_VSCROLL=0x0115;
		private const int WM_MOUSEMOVE=0x0200;
		private const int WM_LBUTTONDOWN=0x0201;
		private const int WM_LBUTTONDBLCLK=0x0203;
		private const int WM_CAPTURECHANGED=0x0215;
		private const int WM_MOUSELEAVE=0x02A3;
		private const int WM_NCMOUSELEAVE=0x02A2;
		private const int WM_PRINT=0x0317;
		private const int WM_PRINTCLIENT=0x0318;
		private const int WM_THEMECHANGED=0x031A;
		private const int WM_REFLECT=0x2000;
		private const int PRF_NONCLIENT=0x0002;
		private const int HTHSCROLL=6;
		private const int HTVSCROLL=7;
		private const int OBJID_CLIENT=-4;
		private const int OBJID_VSCROLL=-5;
		private const int OBJID_HSCROLL=-6;
		private const int STATE_SYSTEM_UNAVAILABLE=0x0001;
		private const int STATE_SYSTEM_INVISIBLE=0x8000;
		private const int STATE_SYSTEM_OFFSCREEN=0x10000;
		private const int TME_LEAVE=0x0002;
		private const int TME_NONCLIENT=0x0010;
		private static readonly ConditionalWeakTable<Control,UiScrollLook> _table=new ConditionalWeakTable<Control,UiScrollLook>();
		private readonly Control _control;
		///<summary>True for a stock scroll bar control (the bar is its client area), false for a window whose frame has scroll bars.</summary>
		private readonly bool _isScrollBarControl;
		private bool _isMouseOver;
		///<summary>The bar pressed while Windows tracks a click on it (OBJID_VSCROLL, OBJID_HSCROLL or OBJID_CLIENT), 0 when none.</summary>
		private int _objectIdPressed;
		private EnumUiScrollPart _partPressed=EnumUiScrollPart.None;
		///<summary>What was painted last (both bars' geometry and states), to paint again only when it changed.</summary>
		private string _stateLast="";
		///<summary>True while the look reads the bars or paints: the messages it sends itself (SBM_GETSCROLLBARINFO to a scroll bar control) pass
		///straight to Windows.</summary>
		private bool _isBusy;
		#endregion Fields

		private UiScrollLook(Control control) {
			_control=control;
			_isScrollBarControl=control is ScrollBar;
			if(control.IsHandleCreated) {
				AssignHandle(control.Handle);
			}
			control.HandleCreated+=(sender,e) => AssignHandle(_control.Handle);
			control.HandleDestroyed+=(sender,e) => ReleaseHandle();
		}

		#region Methods - Public
		///<summary>True for the controls whose scroll bars get the look: a stock scroll bar control (VScrollBar, HScrollBar, the ones inside the
		///program's grids and lists too), a multi-line text box or rich text box, a stock list box (and checked list box), a tree view, a list
		///view, a scrolling panel or user control (AutoScroll), a print preview. Not a window (FormODBase draws its own frame).</summary>
		public static bool IsScrolled(Control control) {
			if(control==null || control is Form) {
				return false;
			}
			if(control is ScrollBar) {
				return true;
			}
			if(control is TextBoxBase textBoxBase) {
				return textBoxBase.Multiline;
			}
			if(control is System.Windows.Forms.ListBox || control is TreeView || control is ListView || control is PrintPreviewControl) {
				return true;
			}
			if(control is ScrollableControl scrollableControl) {
				return scrollableControl.AutoScroll;
			}
			return false;
		}

		public static void Attach(Control control) {
			if(control==null || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiScrollLook(control));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>Paints the look of the control's visible scroll bars into g, whose origin is the control's window's top left corner (as
		///WM_PRINT draws). For a picture of a control that is painted without WM_PRINT (the gallery harness paints text boxes itself).</summary>
		public static void PaintOver(Graphics g,Control control,Point pointWindowOrigin) {
			if(control==null || !control.IsHandleCreated) {
				return;
			}
			bool isScrollBarControl=control is ScrollBar;
			System.Drawing.Drawing2D.GraphicsState graphicsState=g.Save();
			try {
				g.TranslateTransform(pointWindowOrigin.X,pointWindowOrigin.Y);
				foreach(BarInfo barInfo in GetBars(control.Handle,isScrollBarControl,control is VScrollBar)) {
					DrawBar(g,barInfo,EnumUiScrollPart.None,EnumUiScrollPart.None,control.Enabled);
				}
			}
			finally {
				g.Restore(graphicsState);
			}
		}
		#endregion Methods - Public

		#region Methods - Private
		protected override void WndProc(ref Message m) {
			if(_isBusy) {
				base.WndProc(ref m);
				return;
			}
			int msg=m.Msg;
			bool isPress=false;
			try {
				if(_isScrollBarControl && (msg==WM_LBUTTONDOWN || msg==WM_LBUTTONDBLCLK)) {
					//Windows tracks the click inside this message (its own loop): the pressed part is drawn pressed until it returns.
					isPress=true;
					_objectIdPressed=OBJID_CLIENT;
					_partPressed=GetPartAt(OBJID_CLIENT,Cursor.Position);
				}
				else if(!_isScrollBarControl && (msg==WM_NCLBUTTONDOWN || msg==WM_NCLBUTTONDBLCLK)
					&& ((int)m.WParam==HTVSCROLL || (int)m.WParam==HTHSCROLL))
				{
					isPress=true;
					_objectIdPressed=(int)m.WParam==HTVSCROLL ? OBJID_VSCROLL : OBJID_HSCROLL;
					_partPressed=GetPartAt(_objectIdPressed,Cursor.Position);
				}
				if(isPress) {
					_isBusy=true;
					PaintOnWindow(force:true);
				}
			}
			catch(Exception) {
				//The look is only a picture: Windows' own drawing stays when it cannot be painted.
			}
			_isBusy=false;
			base.WndProc(ref m);
			try {
				if(msg==WM_NCDESTROY) {
					return;
				}
				_isBusy=true;
				if(isPress) {
					_objectIdPressed=0;
					_partPressed=EnumUiScrollPart.None;
				}
				switch(msg) {
					case WM_PRINT:
						if(_isScrollBarControl || ((int)m.LParam & PRF_NONCLIENT)!=0) {
							//Windows may leave its own printing's clip on the device context: the bars paint with none (as UiInputFrame).
							int saved=SaveDC(m.WParam);
							try {
								SelectClipRgn(m.WParam,IntPtr.Zero);
								using(Graphics g=Graphics.FromHdc(m.WParam)) {
									PaintBars(g,isForPrint:true);
								}
							}
							finally {
								RestoreDC(m.WParam,saved);
							}
						}
						return;
					case WM_PRINTCLIENT:
						if(_isScrollBarControl) {
							using(Graphics g=Graphics.FromHdc(m.WParam)) {
								PaintBars(g,isForPrint:true);
							}
						}
						return;
					case WM_MOUSEMOVE:
					case WM_NCMOUSEMOVE:
						if(!_isMouseOver) {
							_isMouseOver=true;
							TrackLeave(msg==WM_NCMOUSEMOVE);
						}
						break;
					case WM_MOUSELEAVE:
					case WM_NCMOUSELEAVE:
						_isMouseOver=false;
						break;
				}
				PaintOnWindow(force:IsRedrawMessage(msg) || isPress);
			}
			catch(Exception) {
				//As above.
			}
			finally {
				_isBusy=false;
			}
		}

		///<summary>The messages after which Windows may have drawn the bars whatever their geometry: paint again.</summary>
		private static bool IsRedrawMessage(int msg) {
			switch(msg) {
				case WM_PAINT:
				case WM_NCPAINT:
				case WM_SIZE:
				case WM_ENABLE:
				case WM_SETFOCUS:
				case WM_KILLFOCUS:
				case WM_WINDOWPOSCHANGED:
				case WM_STYLECHANGED:
				case WM_NCACTIVATE:
				case WM_CAPTURECHANGED:
				case WM_THEMECHANGED:
				case WM_VSCROLL:
				case WM_HSCROLL:
				case WM_REFLECT+WM_VSCROLL:
				case WM_REFLECT+WM_HSCROLL:
					return true;
			}
			return false;
		}

		private void TrackLeave(bool isNonClient) {
			TRACKMOUSEEVENT trackMouseEvent=new TRACKMOUSEEVENT();
			trackMouseEvent.cbSize=Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
			trackMouseEvent.dwFlags=TME_LEAVE|(isNonClient ? TME_NONCLIENT : 0);
			trackMouseEvent.hwndTrack=Handle;
			TrackMouseEvent(ref trackMouseEvent);
		}

		///<summary>The part of a bar under a point in screen coordinates.</summary>
		private EnumUiScrollPart GetPartAt(int objectId,Point pointScreen) {
			foreach(BarInfo barInfo in GetBars(Handle,_isScrollBarControl,_control is VScrollBar)) {
				if(barInfo.ObjectId!=objectId) {
					continue;
				}
				Rectangle rectangleScreen=barInfo.RectangleScreen;
				return UiScrollPaint.GetPart(pointScreen,rectangleScreen,barInfo.IsVertical,barInfo.LengthArrow,barInfo.ThumbStart,barInfo.ThumbEnd);
			}
			return EnumUiScrollPart.None;
		}

		///<summary>Paints the bars on the window when forced or when anything drawn changed since the last paint.</summary>
		private void PaintOnWindow(bool force) {
			if(Handle==IntPtr.Zero) {
				return;
			}
			BarInfo[] arrayBarInfos=GetBars(Handle,_isScrollBarControl,_control is VScrollBar);
			Point pointCursor=Cursor.Position;
			string state="";
			for(int i=0;i<arrayBarInfos.Length;i++) {
				BarInfo barInfo=arrayBarInfos[i];
				EnumUiScrollPart partHot=GetHotPart(barInfo,pointCursor);
				EnumUiScrollPart partPressed=_objectIdPressed==barInfo.ObjectId ? _partPressed : EnumUiScrollPart.None;
				state+=barInfo.ObjectId+":"+barInfo.RectangleWindow+","+barInfo.LengthArrow+","+barInfo.ThumbStart+","+barInfo.ThumbEnd+","
					+barInfo.IsDisabled+","+partHot+","+partPressed+";";
			}
			state+=_control.Enabled;
			if(!force && state==_stateLast) {
				return;
			}
			_stateLast=state;
			if(arrayBarInfos.Length==0) {
				return;
			}
			IntPtr hdc=GetWindowDC(Handle);
			if(hdc==IntPtr.Zero) {
				return;
			}
			try {
				using(Graphics g=Graphics.FromHdc(hdc)) {
					PaintBars(g,arrayBarInfos,pointCursor,isForPrint:false);
				}
			}
			finally {
				ReleaseDC(Handle,hdc);
			}
		}

		private void PaintBars(Graphics g,bool isForPrint) {
			PaintBars(g,GetBars(Handle,_isScrollBarControl,_control is VScrollBar),Cursor.Position,isForPrint);
		}

		private void PaintBars(Graphics g,BarInfo[] arrayBarInfos,Point pointCursor,bool isForPrint) {
			foreach(BarInfo barInfo in arrayBarInfos) {
				EnumUiScrollPart partHot=isForPrint ? EnumUiScrollPart.None : GetHotPart(barInfo,pointCursor);
				EnumUiScrollPart partPressed=(!isForPrint && _objectIdPressed==barInfo.ObjectId) ? _partPressed : EnumUiScrollPart.None;
				DrawBar(g,barInfo,partHot,partPressed,_control.Enabled);
			}
			if(!_isScrollBarControl && arrayBarInfos.Length==2) {
				//The square between two frame bars (the size box): the track's colour.
				Rectangle rectangleV=arrayBarInfos[0].IsVertical ? arrayBarInfos[0].RectangleWindow : arrayBarInfos[1].RectangleWindow;
				Rectangle rectangleH=arrayBarInfos[0].IsVertical ? arrayBarInfos[1].RectangleWindow : arrayBarInfos[0].RectangleWindow;
				Rectangle rectangleCorner=new Rectangle(rectangleV.X,rectangleH.Y,rectangleV.Width,rectangleH.Height);
				using(SolidBrush brush=new SolidBrush(UiTheme.GetColor(UiScrollPaint.GetTrackColor()))) {
					g.FillRectangle(brush,rectangleCorner);
				}
			}
		}

		private EnumUiScrollPart GetHotPart(BarInfo barInfo,Point pointCursor) {
			if(!_isMouseOver && _objectIdPressed==0) {
				return EnumUiScrollPart.None;
			}
			return UiScrollPaint.GetPart(pointCursor,barInfo.RectangleScreen,barInfo.IsVertical,barInfo.LengthArrow,barInfo.ThumbStart,barInfo.ThumbEnd);
		}

		private static void DrawBar(Graphics g,BarInfo barInfo,EnumUiScrollPart partHot,EnumUiScrollPart partPressed,bool isControlEnabled) {
			bool isEnabled=isControlEnabled && !barInfo.IsDisabled;
			UiScrollPaint.Draw(g,barInfo.RectangleWindow,barInfo.IsVertical,isEnabled,barInfo.LengthArrow,barInfo.ThumbStart,barInfo.ThumbEnd,
				partHot,partPressed,Color.Empty);
		}

		///<summary>The visible bars of a window: the client bar of a scroll bar control, or the frame's vertical and horizontal bars (vertical
		///first).</summary>
		private static BarInfo[] GetBars(IntPtr hWnd,bool isScrollBarControl,bool isVerticalControl) {
			if(hWnd==IntPtr.Zero || !GetWindowRect(hWnd,out RECT rectWindow)) {
				return new BarInfo[0];
			}
			if(isScrollBarControl) {
				BarInfo barInfo=GetBar(hWnd,OBJID_CLIENT,isVerticalControl,rectWindow);
				return barInfo==null ? new BarInfo[0] : new BarInfo[] { barInfo };
			}
			BarInfo barInfoV=GetBar(hWnd,OBJID_VSCROLL,true,rectWindow);
			BarInfo barInfoH=GetBar(hWnd,OBJID_HSCROLL,false,rectWindow);
			if(barInfoV!=null && barInfoH!=null) {
				return new BarInfo[] { barInfoV,barInfoH };
			}
			if(barInfoV!=null) {
				return new BarInfo[] { barInfoV };
			}
			if(barInfoH!=null) {
				return new BarInfo[] { barInfoH };
			}
			return new BarInfo[0];
		}

		private static BarInfo GetBar(IntPtr hWnd,int objectId,bool isVertical,RECT rectWindow) {
			SCROLLBARINFO scrollBarInfo=new SCROLLBARINFO();
			scrollBarInfo.cbSize=Marshal.SizeOf(typeof(SCROLLBARINFO));
			scrollBarInfo.rgstate=new int[6];
			if(!GetScrollBarInfo(hWnd,objectId,ref scrollBarInfo)) {
				return null;
			}
			int stateBar=scrollBarInfo.rgstate[0];
			if((stateBar & (STATE_SYSTEM_INVISIBLE|STATE_SYSTEM_OFFSCREEN))!=0) {
				return null;
			}
			RECT rect=scrollBarInfo.rcScrollBar;
			if(rect.Right<=rect.Left || rect.Bottom<=rect.Top) {
				return null;
			}
			BarInfo barInfo=new BarInfo();
			barInfo.ObjectId=objectId;
			barInfo.IsVertical=isVertical;
			barInfo.RectangleScreen=Rectangle.FromLTRB(rect.Left,rect.Top,rect.Right,rect.Bottom);
			barInfo.RectangleWindow=Rectangle.FromLTRB(rect.Left-rectWindow.Left,rect.Top-rectWindow.Top,rect.Right-rectWindow.Left,rect.Bottom-rectWindow.Top);
			barInfo.LengthArrow=scrollBarInfo.dxyLineButton;
			barInfo.ThumbStart=scrollBarInfo.xyThumbTop;
			barInfo.ThumbEnd=scrollBarInfo.xyThumbBottom;
			barInfo.IsDisabled=(stateBar & STATE_SYSTEM_UNAVAILABLE)!=0;
			//No thumb when Windows shows none: a disabled bar, or a track too short for one (its rgstate marks the thumb invisible).
			if((scrollBarInfo.rgstate[3] & (STATE_SYSTEM_INVISIBLE|STATE_SYSTEM_OFFSCREEN))!=0) {
				barInfo.ThumbStart=0;
				barInfo.ThumbEnd=0;
			}
			return barInfo;
		}

		private class BarInfo {
			public int ObjectId;
			public bool IsVertical;
			public Rectangle RectangleScreen;
			public Rectangle RectangleWindow;
			public int LengthArrow;
			public int ThumbStart;
			public int ThumbEnd;
			public bool IsDisabled;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT {
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct SCROLLBARINFO {
			public int cbSize;
			public RECT rcScrollBar;
			public int dxyLineButton;
			public int xyThumbTop;
			public int xyThumbBottom;
			public int reserved;
			[MarshalAs(UnmanagedType.ByValArray,SizeConst=6)]
			public int[] rgstate;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct TRACKMOUSEEVENT {
			public int cbSize;
			public int dwFlags;
			public IntPtr hwndTrack;
			public int dwHoverTime;
		}

		[DllImport("user32.dll")]
		private static extern bool GetScrollBarInfo(IntPtr hWnd,int idObject,ref SCROLLBARINFO psbi);

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd,out RECT lpRect);

		[DllImport("user32.dll")]
		private static extern IntPtr GetWindowDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern int ReleaseDC(IntPtr hWnd,IntPtr hDC);

		[DllImport("gdi32.dll")]
		private static extern int SaveDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern bool RestoreDC(IntPtr hdc,int nSavedDC);

		[DllImport("gdi32.dll")]
		private static extern int SelectClipRgn(IntPtr hdc,IntPtr hrgn);

		[DllImport("user32.dll")]
		private static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);
		#endregion Methods - Private
	}
}
