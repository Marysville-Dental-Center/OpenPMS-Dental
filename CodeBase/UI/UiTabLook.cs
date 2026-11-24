// UI-PLAIN (docs/port-notes/UI-PLAIN.md): the stock tab control (System.Windows.Forms.TabControl, not the program's UI.TabControl: the
// dashboard's DashboardTabCtrl and any made in code) drawn like the program's tab control (UI.TabControl, UI-4b, section 3.6): the strip on
// SurfaceSubtle, the page's 1 px Stroke border, the tabs transparent with TextSecondary, SubtleHover under the mouse, the selected one Text with
// the 2 px Accent line (UiTabPaint). Painted over Windows' picture after each WM_PAINT and into WM_PRINT, at the tab rectangles Windows
// reports (TabControl.GetTabRect), so a click selects the tab drawn there; Windows keeps the selection, the keys, the events and the pages. The
// pages (stock TabPage) show the colour behind the tab control, as the program's pages do. Only the top strip with normal tabs (the only kind
// the plain windows use): bottom, side and button tabs and owner-drawn tab controls keep Windows' drawing.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CodeBase {
	///<summary>Paints the program's tab look over a stock top tab control.</summary>
	public class UiTabLook : NativeWindow {
		#region Fields
		private const int WM_PAINT=0x000F;
		private const int WM_NCDESTROY=0x0082;
		private const int WM_MOUSEMOVE=0x0200;
		private const int WM_MOUSELEAVE=0x02A3;
		private const int WM_PRINT=0x0317;
		private const int WM_PRINTCLIENT=0x0318;
		private const int TME_LEAVE=0x0002;
		private static readonly ConditionalWeakTable<Control,UiTabLook> _table=new ConditionalWeakTable<Control,UiTabLook>();
		private readonly System.Windows.Forms.TabControl _tabControl;
		///<summary>The tab under the mouse, -1 for none.</summary>
		private int _idxHover=-1;
		private bool _isMouseTracked;
		private bool _isBusy;
		#endregion Fields

		private UiTabLook(System.Windows.Forms.TabControl tabControl) {
			_tabControl=tabControl;
			if(tabControl.IsHandleCreated) {
				AssignHandle(tabControl.Handle);
			}
			tabControl.HandleCreated+=(sender,e) => AssignHandle(_tabControl.Handle);
			tabControl.HandleDestroyed+=(sender,e) => ReleaseHandle();
			foreach(Control control in tabControl.Controls) {
				SetPageLook(control);
			}
			//The dashboard adds its pages in code.
			tabControl.ControlAdded+=(sender,e) => SetPageLook(e.Control);
		}

		#region Methods - Public
		///<summary>True for a stock tab control drawn by Windows with normal tabs along its top, that its owner does not draw.</summary>
		public static bool IsDrawn(Control control) {
			if(control==null || control.GetType()!=typeof(System.Windows.Forms.TabControl)) {
				return false;
			}
			System.Windows.Forms.TabControl tabControl=(System.Windows.Forms.TabControl)control;
			return tabControl.Alignment==TabAlignment.Top && tabControl.Appearance==TabAppearance.Normal && tabControl.DrawMode==TabDrawMode.Normal
				&& tabControl.RightToLeftLayout==false && !UiButtonLook.IsPaintedByOwner(control);
		}

		public static void Attach(Control control) {
			if(!IsDrawn(control) || _table.TryGetValue(control,out _)) {
				return;
			}
			_table.Add(control,new UiTabLook((System.Windows.Forms.TabControl)control));
		}

		public static bool IsAttached(Control control) {
			return control!=null && _table.TryGetValue(control,out _);
		}

		///<summary>The whole tab control in the look in g (origin at its top left): the colour behind it, the strip as tall as Windows' tabs, the
		///page's border under it, and each tab at Windows' rectangle. The pages are child windows that draw themselves over the page area.</summary>
		public static void PaintTabControl(Graphics g,System.Windows.Forms.TabControl tabControl,int idxHover) {
			float scale=UiLooks.GetScale(tabControl);
			Rectangle rectangle=new Rectangle(0,0,tabControl.Width,tabControl.Height);
			using(SolidBrush brush=new SolidBrush(GetColorBehind(tabControl))) {
				g.FillRectangle(brush,rectangle);
			}
			int bottomStrip=0;
			for(int i=0;i<tabControl.TabCount;i++) {
				bottomStrip=Math.Max(bottomStrip,tabControl.GetTabRect(i).Bottom);
			}
			if(bottomStrip>0) {
				UiTabPaint.DrawStrip(g,new Rectangle(0,0,tabControl.Width,bottomStrip),scale);
			}
			Rectangle rectanglePage=new Rectangle(0,Math.Max(0,bottomStrip-1),tabControl.Width,tabControl.Height-Math.Max(0,bottomStrip-1));
			UiTabPaint.DrawPageBorder(g,rectanglePage,scale);
			for(int i=0;i<tabControl.TabCount;i++) {
				Rectangle rectangleHeader=tabControl.GetTabRect(i);
				System.Windows.Forms.TabPage tabPage=tabControl.TabPages[i];
				bool isEnabled=tabControl.Enabled && tabPage.Enabled;
				EnumUiColor uiColorText=UiTabPaint.DrawHeader(g,rectangleHeader,EnumUiTabSide.Top,isSelected:tabControl.SelectedIndex==i,isHover:idxHover==i,isEnabled,
					Color.Empty,scale);
				TextRenderer.DrawText(g,tabPage.Text,tabControl.Font,rectangleHeader,UiTheme.GetColor(uiColorText),
					TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.EndEllipsis);
			}
		}

		///<summary>The colour behind the tab control: its parent's (the program's pages and strip stand on the window's colour).</summary>
		public static Color GetColorBehind(Control control) {
			return control.Parent==null ? SystemColors.Control : UiLooks.GetBackColor(control.Parent);
		}
		#endregion Methods - Public

		#region Methods - Private
		///<summary>A stock tab page shows the colour behind its tab control (Windows' themed page or classic grey before), unless the window chose
		///its own colour.</summary>
		private void SetPageLook(Control control) {
			if(!(control is System.Windows.Forms.TabPage tabPage) || control.GetType()!=typeof(System.Windows.Forms.TabPage)) {
				return;
			}
			if(tabPage.UseVisualStyleBackColor || UiPlainWindow.IsOldBackground(tabPage.BackColor)) {
				tabPage.BackColor=GetColorBehind(_tabControl);
			}
		}

		protected override void WndProc(ref Message m) {
			if(_isBusy) {
				base.WndProc(ref m);
				return;
			}
			int msg=m.Msg;
			base.WndProc(ref m);
			_isBusy=true;
			try {
				switch(msg) {
					case WM_NCDESTROY:
						return;
					case WM_PRINT:
					case WM_PRINTCLIENT:
						using(Graphics g=Graphics.FromHdc(m.WParam)) {
							PaintTabControl(g,_tabControl,-1);
						}
						return;
					case WM_PAINT:
						PaintOnWindow();
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
						SetHover(GetTabAt(new Point((short)((long)m.LParam & 0xFFFF),(short)(((long)m.LParam>>16) & 0xFFFF))));
						return;
					case WM_MOUSELEAVE:
						_isMouseTracked=false;
						SetHover(-1);
						return;
				}
			}
			catch(Exception) {
				//The look is only a picture: Windows' own drawing stays when it cannot be painted.
			}
			finally {
				_isBusy=false;
			}
		}

		private int GetTabAt(Point point) {
			for(int i=0;i<_tabControl.TabCount;i++) {
				if(_tabControl.GetTabRect(i).Contains(point)) {
					return i;
				}
			}
			return -1;
		}

		private void SetHover(int idxHover) {
			if(idxHover==_idxHover) {
				return;
			}
			_idxHover=idxHover;
			PaintOnWindow();
		}

		///<summary>Paints over Windows' picture on screen; the pages are child windows outside the tab control's drawing (WS_CLIPCHILDREN).</summary>
		private void PaintOnWindow() {
			if(Handle==IntPtr.Zero || !_tabControl.Visible) {
				return;
			}
			using(Graphics g=_tabControl.CreateGraphics()) {
				PaintTabControl(g,_tabControl,_idxHover);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct TRACKMOUSEEVENT {
			public int cbSize;
			public int dwFlags;
			public IntPtr hwndTrack;
			public int dwHoverTime;
		}

		[DllImport("user32.dll")]
		private static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);
		#endregion Methods - Private
	}
}
