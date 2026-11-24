// UI-PLAIN-2 (docs/port-notes/UI-PLAIN.md section 8; the owner on the UI-PLAIN gallery, 2026-11-23: "window title is the old style. Not
// matching the rest"): the program's title bar and frame on a plain window (a Form that is not a FormODBase), drawn by the painter FormODBase
// draws with (UiFramePainter), with FormODBase's sizes: the title band 26 px at 96 dpi, its font, the caption buttons, the outline and
// Windows 11's rounded corners and border colour, the invisible resize borders left, right and below.
//
// UI-PLAIN-2 fix round: the frame takes the window at Load, not when its handle is made: Windows Forms' old AutoScale (a window whose
// designer code sets only AutoScaleBaseSize: the report designer's dialogs, ODR's FormParameterInput) sets the window's ClientSize in
// Form.OnLoad, before the Load handlers, and turns it into a window size with Windows' standard caption and borders; with the frame already
// on, that client area came out a caption taller. The title band shows the program icon (IconProgram) where the window has no icon of its
// own or has a dialog or tool window frame, as FormODBase shows it on every window; never the Windows Forms default icon.
//
// How: like FormODBase, the window keeps its Windows styles (so the taskbar entry, the icon, the system menu, Alt+Space, modal dialogs and
// the fixed borders of a dialog that is not resizable stay Windows') and its client area covers the title band (WM_NCCALCSIZE takes only the
// invisible resize borders, and on a maximized window the border that hangs past the screen on top too). FormODBase moves its controls into
// a PanelClient below the band; a plain window keeps its controls where they are and moves the content down instead, once, when it loads
// (the band's height is added to the window and to its Padding, which the docked controls keep to, and every other control on the window
// moves down by it as its anchors say). The band is painted on the window's surface (its Paint event). WM_NCHITTEST tells Windows what
// the band is: the caption (Windows drags the window, snaps it, maximizes it on a double-click and shows the system menu on a right-click),
// the top resize edge and corner, and the caption buttons (HTMINBUTTON, HTMAXBUTTON: Windows 11's snap layouts; HTCLOSE), whose clicks this
// class turns into the system commands Windows' own buttons send. A per-monitor dpi change takes the content back up, lets Windows Forms scale
// the window, and moves it down by the new band.
//
// Not for: an MDI child (Windows' MDI frame), a window without a border, a window that is not top level, a window while the gallery draws its
// before pictures (UiLooks.IsOffForGallery), or every window when NoCustomBorders.txt is next to the exe (FormODBase's own switch).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CodeBase {
	///<summary>The program's title bar and frame on a plain window (UiPlainWindow.Attach turns it on).</summary>
	public static class UiPlainFrame {
		#region Constants
		///<summary>The title band's height at 96 dpi: LayoutManagerForms' _heightTitleBar96, FormODBase's GetHeightTitleBar.</summary>
		public const int HEIGHT_TITLE_BAR_96=26;
		///<summary>The painter's space right of the close button at 96 dpi: FormODBase's WidthBorder (Scale(15)).</summary>
		public const int WIDTH_RIGHT_96=15;
		///<summary>The resize edge inside the title band at 96 dpi: LayoutManagerForms' _widthBorder96 (FormODBase's PanelBorders_MouseDown).</summary>
		public const int WIDTH_RESIZE_EDGE_96=5;
		public const int HTCLIENT=1;
		public const int HTCAPTION=2;
		public const int HTMINBUTTON=8;
		public const int HTMAXBUTTON=9;
		public const int HTLEFT=10;
		public const int HTRIGHT=11;
		public const int HTTOP=12;
		public const int HTTOPLEFT=13;
		public const int HTTOPRIGHT=14;
		public const int HTCLOSE=20;
		private const int WM_MOUSEMOVE=0x0200;
		private const int WM_NCCALCSIZE=0x0083;
		private const int WM_NCHITTEST=0x0084;
		private const int WM_NCMOUSEMOVE=0x00A0;
		private const int WM_NCLBUTTONDOWN=0x00A1;
		private const int WM_NCLBUTTONUP=0x00A2;
		private const int WM_NCLBUTTONDBLCLK=0x00A3;
		private const int WM_NCMOUSELEAVE=0x02A2;
		private const int WM_SYSCOMMAND=0x0112;
		private const int WM_DPICHANGED=0x02E0;
		private const int SC_MINIMIZE=0xF020;
		private const int SC_MAXIMIZE=0xF030;
		private const int SC_CLOSE=0xF060;
		private const int SC_RESTORE=0xF120;
		private const int SWP_NOSIZE=0x0001;
		private const int SWP_NOMOVE=0x0002;
		private const int SWP_NOZORDER=0x0004;
		private const int SWP_NOACTIVATE=0x0010;
		private const int SWP_FRAMECHANGED=0x0020;
		private const int SWP_NOOWNERZORDER=0x0200;
		private const uint TME_LEAVE=0x00000002;
		private const uint TME_NONCLIENT=0x00000010;
		#endregion Constants

		#region Fields
		private static readonly ConditionalWeakTable<Form,FrameWindow> _table=new ConditionalWeakTable<Form,FrameWindow>();
		///<summary>Null until read: true when NoCustomBorders.txt is next to the exe (FormODBase.AreBordersMS's file), which keeps Windows' frame.</summary>
		private static bool? _areBordersMS;
		///<summary>For the gallery harness, which draws in a 96 dpi process: the scale of a window's frame instead of its dpi. Null: the dpi.</summary>
		public static Func<Form,float> FuncGetScaleForTest;
		///<summary>For the gallery harness, whose exe is the test host: the program icon to show instead of IconProgram's. Null: IconProgram's.
		///</summary>
		public static Icon IconProgramForTest;
		///<summary>The program icon once read (IconProgram).</summary>
		private static Icon _iconProgram;
		private static bool _isIconProgramRead;
		///<summary>Windows Forms' default window icon (Form.DefaultIcon, internal) once read; null if it cannot be read.</summary>
		private static Icon _iconDefault;
		private static bool _isIconDefaultRead;
		///<summary>The manifest resource name of branding/app.ico in CodeBase (xCodeBase.csproj): the program icon FormODBase shows.</summary>
		private const string ICON_RESOURCE_NAME="CodeBase.Branding.app.ico";
		#endregion Fields

		#region Methods - Public
		///<summary>Gives the window the program's title bar and frame from its handle on (UiPlainWindow.Attach calls it once, in the constructor).
		///Calling it again changes nothing.</summary>
		public static void Attach(Form form) {
			if(form==null || UiLooks.IsOffForGallery || AreBordersMS || _table.TryGetValue(form,out _)) {
				return;
			}
			FrameWindow frameWindow=new FrameWindow(form);
			_table.Add(form,frameWindow);
			frameWindow.Hook();
		}

		///<summary>True when NoCustomBorders.txt is next to the exe: every window keeps Windows' frame, as FormODBase.AreBordersMS does.</summary>
		public static bool AreBordersMS {
			get {
				if(_areBordersMS==null) {
					try {
						_areBordersMS=File.Exists(Path.Combine(Application.StartupPath,"NoCustomBorders.txt"));
					}
					catch(Exception) {
						_areBordersMS=false;
					}
				}
				return _areBordersMS.Value;
			}
			set {
				_areBordersMS=value;
			}
		}

		///<summary>True when Attach was called for the window.</summary>
		public static bool IsAttached(Form form) {
			return form!=null && _table.TryGetValue(form,out _);
		}

		///<summary>True while the window has the frame: its handle hooked, its client area over the title band.</summary>
		public static bool IsOn(Form form) {
			return form!=null && _table.TryGetValue(form,out FrameWindow frameWindow) && frameWindow.IsOn;
		}

		///<summary>The title band's height the window's content is moved down by (0 before it loads or without the frame).</summary>
		public static int GetContentOffset(Form form) {
			if(form==null || !_table.TryGetValue(form,out FrameWindow frameWindow)) {
				return 0;
			}
			return frameWindow.Offset;
		}

		///<summary>What the window's Load does for the frame, for the harness, which builds a window without showing it: the handle (the frame
		///hooks it) and the content moved down below the title band.</summary>
		public static void LayOutAsOnLoad(Form form) {
			if(form==null || !_table.TryGetValue(form,out FrameWindow frameWindow)) {
				return;
			}
			IntPtr handle=form.Handle;//makes the handle
			frameWindow.TurnOnAtLoad();
		}

		///<summary>The program icon the title band shows on a window without an icon of its own (or with a dialog or tool window frame), read
		///once per process: the program icon CodeBase embeds (branding/app.ico, BrandAssets.AppIcon's file), else, in an assembly that compiles
		///these files in without it (RdlDesign, UpdateFileCopier), the exe's own icon when the exe has one. Null when there is none: the band then
		///shows no icon rather than Windows Forms' default one. IconProgramForTest in the gallery harness.</summary>
		public static Icon IconProgram {
			get {
				if(IconProgramForTest!=null) {
					return IconProgramForTest;
				}
				if(!_isIconProgramRead) {
					_iconProgram=ReadIconProgram();
					_isIconProgramRead=true;
				}
				return _iconProgram;
			}
		}

		///<summary>True for Windows Forms' default window icon (the two windows of a form that set no icon of its own).</summary>
		public static bool IsDefaultIcon(Icon icon) {
			if(icon==null) {
				return false;
			}
			if(!_isIconDefaultRead) {
				try {
					PropertyInfo propertyInfo=typeof(Form).GetProperty("DefaultIcon",BindingFlags.NonPublic|BindingFlags.Static);
					_iconDefault=propertyInfo?.GetValue(null) as Icon;
				}
				catch(Exception) {
					_iconDefault=null;
				}
				_isIconDefaultRead=true;
			}
			return _iconDefault!=null && ReferenceEquals(icon,_iconDefault);
		}

		///<summary>The icon the title band shows: the window's own icon on a window with Windows' normal frame that set one, else the program icon
		///(IconProgram), as FormODBase shows the program icon on every window, dialogs included. Never Windows Forms' default icon.</summary>
		public static Icon GetBandIcon(Form form) {
			Icon icon=form.Icon;
			bool isDefault=IsDefaultIcon(icon);
			bool isDialogFrame=form.FormBorderStyle==FormBorderStyle.FixedDialog
				|| form.FormBorderStyle==FormBorderStyle.FixedToolWindow || form.FormBorderStyle==FormBorderStyle.SizableToolWindow;
			if(icon!=null && !isDefault && !isDialogFrame) {
				return icon;
			}
			Icon iconProgram=IconProgram;
			if(iconProgram!=null) {
				return iconProgram;
			}
			return isDefault ? null : icon;
		}

		///<summary>The scale of the window's frame: its dpi over 96 (FuncGetScaleForTest in the gallery harness).</summary>
		public static float GetScale(Form form) {
			if(FuncGetScaleForTest!=null) {
				float scaleTest=FuncGetScaleForTest(form);
				if(scaleTest>0) {
					return scaleTest;
				}
			}
			int dpi=form.DeviceDpi;
			return dpi<=0 ? 1f : dpi/96f;
		}

		///<summary>A 96 dpi length at the scale, rounded as LayoutManager.Scale rounds.</summary>
		public static int Scale(float val96,float scale) {
			return (int)Math.Round(val96*scale);
		}

		///<summary>The title band's height in device pixels: Scale(26).</summary>
		public static int GetHeightTitleBar(Form form) {
			return Scale(HEIGHT_TITLE_BAR_96,GetScale(form));
		}

		///<summary>The window's client area for a window rectangle (WM_NCCALCSIZE): the window less the invisible resize borders left, right and
		///below (UiFramePainter.GetWidthBorderMS, FormODBase's), its top the window's top; a maximized window, which Windows lets hang past the
		///screen by that border on every side, less it on top too, so its title band starts at the screen's top row.</summary>
		public static Rectangle GetClientRectangle(Rectangle rectangleWindow,bool isMaximized,float scale) {
			int widthBorder=UiFramePainter.GetWidthBorderMS(scale);
			int top=isMaximized ? widthBorder : 0;
			return Rectangle.FromLTRB(rectangleWindow.Left+widthBorder,rectangleWindow.Top+top,rectangleWindow.Right-widthBorder,rectangleWindow.Bottom-widthBorder);
		}

		///<summary>What UiFramePainter needs for the window's frame, as FormODBase.GetFramePaintArgs gives it for a window of the same client size:
		///the window's size with its invisible borders (on a maximized window the size of a normal window with that client area, so the band and
		///its buttons lie as on a normal one), its text, caption buttons, icon and hover, the band 26 px and the space right of the close button 15
		///px at its scale, the title font at the scale, and the part the screen shows. Coordinates: FormODBase's PanelBorders, which sit at (-1,-1)
		///of the client area.</summary>
		public static FramePaintArgs GetFramePaintArgs(Form form) {
			float scale=GetScale(form);
			int widthBorder=UiFramePainter.GetWidthBorderMS(scale);
			Size sizeClient=form.ClientSize;
			FramePaintArgs framePaintArgs=new FramePaintArgs();
			framePaintArgs.Width=sizeClient.Width+widthBorder*2;
			framePaintArgs.Height=sizeClient.Height+widthBorder;
			framePaintArgs.Text=form.Text??"";
			framePaintArgs.ControlBox=form.ControlBox;
			framePaintArgs.MaximizeBox=form.MaximizeBox;
			framePaintArgs.MinimizeBox=form.MinimizeBox;
			framePaintArgs.IsMaximized=form.WindowState==FormWindowState.Maximized;
			//The icon as FormODBase shows it, on every window whatever its border (its ShowIcon): the window's own icon, else the program icon
			//(GetBandIcon); never Windows Forms' default icon.
			framePaintArgs.ShowIcon=form.ShowIcon;
			framePaintArgs.Icon=GetBandIcon(form);
			if(_table.TryGetValue(form,out FrameWindow frameWindow)) {
				framePaintArgs.IsHotX=frameWindow.HitHot==HTCLOSE;
				framePaintArgs.IsHotMax=frameWindow.HitHot==HTMAXBUTTON;
				framePaintArgs.IsHotMin=frameWindow.HitHot==HTMINBUTTON;
			}
			framePaintArgs.FrameLook=EnumFrameLook.Window;
			framePaintArgs.IsMainWindowTitle=false;
			framePaintArgs.HeightTitleBar=Scale(HEIGHT_TITLE_BAR_96,scale);
			framePaintArgs.WidthBorder=Scale(WIDTH_RIGHT_96,scale);
			framePaintArgs.MaxInset=0;
			framePaintArgs.Scale=val96 => Scale(val96,scale);
			framePaintArgs.ScaleF=val96 => val96*scale;
			//FormODBase: the title in 8.25 at the program's zoom (none here; Windows draws the point size at the screen's dpi), its height at the
			//screen's scale. The gallery harness, a 96 dpi process, draws the point size at its scale as such a screen would.
			float scaleFont=FuncGetScaleForTest==null ? 1f : scale;
			framePaintArgs.TitleFonts=UiFramePainter.GetTitleFonts(form,val96 => val96*scaleFont,val96 => val96*scale);
			Rectangle rectanglePanelBorders=new Rectangle(-1,-1,framePaintArgs.Width-widthBorder*2+1,framePaintArgs.Height-widthBorder);
			framePaintArgs.RectangleOnScreen=UiFramePainter.GetRectangleOnScreen(new Size(framePaintArgs.Width,framePaintArgs.Height),false,scale,rectanglePanelBorders);
			return framePaintArgs;
		}

		///<summary>The title band in the window's client coordinates: its whole width, the band's height from the top.</summary>
		public static Rectangle GetTitleBarRectangle(Form form) {
			return new Rectangle(0,0,form.ClientSize.Width,GetHeightTitleBar(form));
		}

		///<summary>Paints the title band into g (the window's client coordinates): FormODBase's frame from UiFramePainter, clipped to the band. The
		///stock colours (the tokens; a plain window has no office Misc Colors).</summary>
		public static void PaintTitleBar(Form form,Graphics g) {
			System.Drawing.Drawing2D.GraphicsState graphicsState=g.Save();
			try {
				g.SetClip(GetTitleBarRectangle(form),System.Drawing.Drawing2D.CombineMode.Intersect);
				g.TranslateTransform(-1,-1);
				UiFramePainter.Paint(g,GetFramePaintArgs(form),UiFramePainter.GetWindowColorsStock());
			}
			finally {
				g.Restore(graphicsState);
			}
		}

		///<summary>What the point (the window's client coordinates) is to Windows (WM_NCHITTEST): in the title band the caption buttons (HTCLOSE,
		///HTMAXBUTTON, HTMINBUTTON, at the painter's hit areas), on a resizable window that is not maximized the top resize edge and the left edge
		///and corner inside the band (FormODBase's PanelBorders_MouseDown: 5 px at 96 dpi), else the caption (HTCAPTION); below the band
		///HTCLIENT. Windows answers for the invisible borders outside the client area.</summary>
		public static int HitTest(Form form,Point pointClient) {
			int heightTitleBar=GetHeightTitleBar(form);
			if(pointClient.Y<0 || pointClient.Y>=heightTitleBar || pointClient.X<0 || pointClient.X>=form.ClientSize.Width) {
				return HTCLIENT;
			}
			FramePaintArgs framePaintArgs=GetFramePaintArgs(form);
			FramePaintResult framePaintResult=UiFramePainter.GetButtons(framePaintArgs);
			Point point=new Point(pointClient.X+1,pointClient.Y+1);//the PanelBorders' coordinates
			bool isMaximized=form.WindowState==FormWindowState.Maximized;
			bool isSizable=form.FormBorderStyle==FormBorderStyle.Sizable || form.FormBorderStyle==FormBorderStyle.SizableToolWindow;
			if(!isMaximized && isSizable) {
				int widthEdge=framePaintArgs.Scale(WIDTH_RESIZE_EDGE_96);
				if(point.X<widthEdge+2 && point.Y<widthEdge+2) {
					return HTTOPLEFT;
				}
				if(point.X>framePaintArgs.Width-widthEdge-1 && point.Y<widthEdge+2) {
					return HTTOPRIGHT;
				}
				if(point.X<widthEdge+1) {
					return HTLEFT;
				}
				if(point.X>framePaintArgs.Width-widthEdge-1) {
					return HTRIGHT;
				}
				if(point.Y<widthEdge) {
					return HTTOP;
				}
			}
			if(form.ControlBox && framePaintResult.RectangleHitX.Contains(point)) {
				return HTCLOSE;
			}
			if(form.ControlBox && form.MaximizeBox && framePaintResult.RectangleHitMax.Contains(point)) {
				return HTMAXBUTTON;
			}
			if(form.ControlBox && form.MinimizeBox && framePaintResult.RectangleHitMin.Contains(point)) {
				return HTMINBUTTON;
			}
			return HTCAPTION;
		}

		///<summary>The system command a click on a caption button sends, as Windows' own button sends it: SC_CLOSE, SC_MINIMIZE, SC_MAXIMIZE or
		///SC_RESTORE (on a maximized window). 0 for any other hit.</summary>
		public static int GetSysCommand(int hit,bool isMaximized) {
			switch(hit) {
				case HTCLOSE:
					return SC_CLOSE;
				case HTMINBUTTON:
					return SC_MINIMIZE;
				case HTMAXBUTTON:
					return isMaximized ? SC_RESTORE : SC_MAXIMIZE;
				default:
					return 0;
			}
		}

		///<summary>The new top and height of a control on the window when the content's top moves down by offset and the client area grows by grow
		///(both may be negative), as its anchors keep it: anchored to the top only, it moves with the top; to the bottom only, with the bottom;
		///to both, its top moves with the top and its bottom with the bottom; to neither, its middle stays in the middle of the content.</summary>
		public static Rectangle GetShiftedBounds(Rectangle bounds,AnchorStyles anchorStyles,int offset,int grow) {
			bool isTop=(anchorStyles & AnchorStyles.Top)!=0;
			bool isBottom=(anchorStyles & AnchorStyles.Bottom)!=0;
			int changeHeight=grow-offset;//the content area's height change
			if(isTop && isBottom) {
				return new Rectangle(bounds.X,bounds.Y+offset,bounds.Width,Math.Max(0,bounds.Height+changeHeight));
			}
			if(isBottom) {
				return new Rectangle(bounds.X,bounds.Y+grow,bounds.Width,bounds.Height);
			}
			if(isTop) {
				return new Rectangle(bounds.X,bounds.Y+offset,bounds.Width,bounds.Height);
			}
			return new Rectangle(bounds.X,bounds.Y+offset+changeHeight/2,bounds.Width,bounds.Height);
		}
		#endregion Methods - Public

		///<summary>The program icon embedded in this assembly (CodeBase: branding/app.ico), else the exe's own icon when the exe has one (an exe
		///without one would give Windows' generic program icon), else null.</summary>
		private static Icon ReadIconProgram() {
			try {
				using(Stream stream=typeof(UiPlainFrame).Assembly.GetManifestResourceStream(ICON_RESOURCE_NAME)) {
					if(stream!=null) {
						return new Icon(stream);
					}
				}
			}
			catch(Exception) {
				//The exe's icon below.
			}
			try {
				string pathExe=Application.ExecutablePath;
				if(File.Exists(pathExe) && ExtractIconEx(pathExe,-1,null,null,0)>0) {
					return Icon.ExtractAssociatedIcon(pathExe);
				}
			}
			catch(Exception) {
				//No icon: the band shows none.
			}
			return null;
		}

		#region Win32
		[DllImport("shell32.dll",CharSet=CharSet.Unicode)]
		private static extern int ExtractIconEx(string lpszFile,int nIconIndex,IntPtr[] phiconLarge,IntPtr[] phiconSmall,int nIcons);

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT {
			public int Left,Top,Right,Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WINDOWPOS {
			public IntPtr hWnd,hWndInsertAfter;
			public int x,y,cx,cy,flags;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct NCCALCSIZE_PARAMS {
			public RECT rgrc0,rgrc1,rgrc2;
			public IntPtr lppos;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct TRACKMOUSEEVENT {
			public int cbSize;
			public uint dwFlags;
			public IntPtr hwndTrack;
			public uint dwHoverTime;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT {
			public int X,Y;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WINDOWPLACEMENT {
			public int length;
			public int flags;
			public int showCmd;
			public POINT ptMinPosition;
			public POINT ptMaxPosition;
			public RECT rcNormalPosition;
		}

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hWnd,IntPtr hWndInsertAfter,int x,int y,int cx,int cy,int uFlags);

		[DllImport("user32.dll")]
		private static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT trackMouseEvent);

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd,int msg,IntPtr wParam,IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool IsZoomed(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern bool GetWindowPlacement(IntPtr hWnd,ref WINDOWPLACEMENT windowPlacement);

		[DllImport("user32.dll")]
		private static extern bool SetWindowPlacement(IntPtr hWnd,ref WINDOWPLACEMENT windowPlacement);
		#endregion Win32

		#region FrameWindow
		///<summary>One plain window's frame: the window's messages first (a NativeWindow on its handle), its title band painted, its content
		///moved below the band.</summary>
		private class FrameWindow:NativeWindow {
			private readonly Form _form;
			///<summary>The window's controls moved down by Offset (each control once; a control taken off the window is moved back).</summary>
			private readonly HashSet<Control> _setMoved=new HashSet<Control>();
			///<summary>True once the window loaded and the frame took it (TurnOnAtLoad): a handle made again later is hooked at once and keeps the
			///window's size.</summary>
			private bool _isLoaded;
			///<summary>The hit code of the caption button a mouse button went down on (WM_NCLBUTTONDOWN), 0 for none.</summary>
			private int _hitPressed;
			private bool _isTrackingLeave;
			///<summary>True while a dpi change moves the content (no ControlAdded bookkeeping).</summary>
			private bool _isMovingContent;
			public int HitHot;
			public bool IsOn;
			public int Offset;

			public FrameWindow(Form form) {
				_form=form;
			}

			public void Hook() {
				_form.HandleCreated+=Form_HandleCreated;
				_form.HandleDestroyed+=Form_HandleDestroyed;
				_form.Load+=Form_Load;
				_form.Paint+=Form_Paint;
				_form.TextChanged+=Form_InvalidateTitleBar;
				_form.SizeChanged+=Form_InvalidateTitleBar;
				_form.ControlAdded+=Form_ControlAdded;
				_form.ControlRemoved+=Form_ControlRemoved;
				if(_form.IsHandleCreated && _form.Visible) {
					//Attached to a window already shown: its Load is past.
					TurnOnAtLoad();
				}
			}

			///<summary>True for a window that gets the frame: top level, with a border, not an MDI child.</summary>
			private bool IsWanted() {
				return _form.TopLevel && !_form.IsMdiChild && _form.FormBorderStyle!=FormBorderStyle.None && !UiLooks.IsOffForGallery;
			}

			private void Form_HandleCreated(object sender,EventArgs e) {
				//Before Load the window keeps Windows' frame, so Windows Forms' scaling in Form.OnLoad sizes it as it always did (TurnOnAtLoad); a
				//handle made again after Load is hooked at once.
				if(_isLoaded) {
					TurnOn();
				}
			}

			private void Form_HandleDestroyed(object sender,EventArgs e) {
				IsOn=false;
				_isTrackingLeave=false;
				HitHot=0;
				_hitPressed=0;
			}

			///<summary>At Load (after Form.OnLoad's scaling and the window's own Load handlers, before the window shows), once: the frame takes the
			///window, whose size is set so the client area Windows Forms gave it (the designer's ClientSize, scaled) is kept within the frame's
			///borders (a centred window keeps its centre), then the content moves below the band (ApplyContent).</summary>
			public void TurnOnAtLoad() {
				if(_isLoaded || !IsWanted() || !_form.IsHandleCreated) {
					return;
				}
				_isLoaded=true;
				Size sizeClient=_form.ClientSize;
				Rectangle boundsBefore=_form.Bounds;
				TurnOn();
				if(!IsOn) {
					return;
				}
				if(_form.WindowState==FormWindowState.Normal) {
					SetClientSize(sizeClient);
					if(IsCentred()) {
						Size size=_form.Size;
						_form.Location=new Point(boundsBefore.X+(boundsBefore.Width-size.Width)/2,boundsBefore.Y+(boundsBefore.Height-size.Height)/2);
					}
				}
				ApplyContent();
			}

			private bool IsCentred() {
				return _form.StartPosition==FormStartPosition.CenterScreen || _form.StartPosition==FormStartPosition.CenterParent;
			}

			///<summary>Hooks the handle: the client area over the title band from now on (SWP_FRAMECHANGED makes Windows ask WM_NCCALCSIZE again)
			///and Windows 11's corners and border colour. The window's size stays (TurnOnAtLoad sets it once).</summary>
			private void TurnOn() {
				if(!IsWanted() || !_form.IsHandleCreated) {
					return;
				}
				try {
					if(Handle!=_form.Handle) {
						if(Handle!=IntPtr.Zero) {
							ReleaseHandle();
						}
						AssignHandle(_form.Handle);
					}
					IsOn=true;
					UiFramePainter.ApplyDwmCorners(_form.Handle,false,UiFramePainter.GetWindowColorsStock().Outline);
					SetWindowPos(_form.Handle,IntPtr.Zero,0,0,0,0,SWP_FRAMECHANGED|SWP_NOMOVE|SWP_NOSIZE|SWP_NOZORDER|SWP_NOACTIVATE|SWP_NOOWNERZORDER);
				}
				catch(Exception) {
					//The frame is only a picture and a hit test: the window works with Windows' frame.
					IsOn=false;
				}
			}

			///<summary>The window's size for a client area of the size: the invisible borders left, right and below added.</summary>
			private void SetClientSize(Size sizeClient) {
				int widthBorder=UiFramePainter.GetWidthBorderMS(GetScale(_form));
				Size size=new Size(sizeClient.Width+widthBorder*2,sizeClient.Height+widthBorder);
				if(_form.Size!=size) {
					_form.Size=size;
				}
			}

			private void Form_Load(object sender,EventArgs e) {
				TurnOnAtLoad();
			}

			///<summary>Moves the content down below the title band, once: the band's height added to the window (or, on a maximized window, to the
			///size it restores to) and to its Padding (the docked controls), and every other control of the window moved as its anchors say
			///(GetShiftedBounds). A centred window moves up by half the band, so it stays centred.</summary>
			public void ApplyContent() {
				if(!IsOn || Offset>0) {
					return;
				}
				int offset=GetHeightTitleBar(_form);
				int grow=0;
				if(_form.WindowState==FormWindowState.Normal) {
					grow=offset;
				}
				else {
					WINDOWPLACEMENT windowPlacement=new WINDOWPLACEMENT();
					windowPlacement.length=Marshal.SizeOf(typeof(WINDOWPLACEMENT));
					if(GetWindowPlacement(_form.Handle,ref windowPlacement)) {
						windowPlacement.rcNormalPosition.Bottom+=offset;
						SetWindowPlacement(_form.Handle,ref windowPlacement);
					}
				}
				MoveContent(offset,grow,isCentred:IsCentred());
				Offset=offset;
				_form.Invalidate(GetTitleBarRectangle(_form));
			}

			///<summary>Moves the content's top by offset (negative: up) with the client area grown by grow: the window's size, the Padding's top,
			///and each control on the window that is not docked, as its anchors say.</summary>
			private void MoveContent(int offset,int grow,bool isCentred) {
				_isMovingContent=true;
				try {
					List<Tuple<Control,Rectangle>> listBounds=new List<Tuple<Control,Rectangle>>();
					foreach(Control control in _form.Controls) {
						if(control.Dock==DockStyle.None) {
							listBounds.Add(Tuple.Create(control,control.Bounds));
						}
					}
					if(grow!=0) {
						Point location=_form.Location;
						_form.Size=new Size(_form.Width,_form.Height+grow);
						if(isCentred) {
							_form.Location=new Point(location.X,location.Y-grow/2);
						}
					}
					Padding padding=_form.Padding;
					_form.Padding=new Padding(padding.Left,Math.Max(0,padding.Top+offset),padding.Right,padding.Bottom);
					foreach(Tuple<Control,Rectangle> tuple in listBounds) {
						tuple.Item1.Bounds=GetShiftedBounds(tuple.Item2,tuple.Item1.Anchor,offset,grow);
						if(offset>0) {
							_setMoved.Add(tuple.Item1);
						}
						else {
							_setMoved.Remove(tuple.Item1);
						}
					}
				}
				finally {
					_isMovingContent=false;
				}
			}

			private void Form_ControlAdded(object sender,ControlEventArgs e) {
				if(Offset==0 || _isMovingContent || e.Control==null || e.Control.Dock!=DockStyle.None || _setMoved.Contains(e.Control)) {
					return;
				}
				e.Control.Top+=Offset;
				_setMoved.Add(e.Control);
			}

			private void Form_ControlRemoved(object sender,ControlEventArgs e) {
				if(_isMovingContent || e.Control==null || !_setMoved.Contains(e.Control)) {
					return;
				}
				_setMoved.Remove(e.Control);
				if(e.Control.Dock==DockStyle.None) {
					e.Control.Top-=Offset;
				}
			}

			private void Form_Paint(object sender,PaintEventArgs e) {
				if(!IsOn || Offset==0) {
					return;
				}
				if(!e.ClipRectangle.IntersectsWith(GetTitleBarRectangle(_form))) {
					return;
				}
				PaintTitleBar(_form,e.Graphics);
			}

			private void Form_InvalidateTitleBar(object sender,EventArgs e) {
				if(IsOn && Offset>0) {
					_form.Invalidate(GetTitleBarRectangle(_form));
				}
			}

			private void SetHot(int hit) {
				if(hit!=HTCLOSE && hit!=HTMAXBUTTON && hit!=HTMINBUTTON) {
					hit=0;
				}
				if(hit==HitHot) {
					return;
				}
				HitHot=hit;
				_form.Invalidate(GetTitleBarRectangle(_form));
			}

			protected override void WndProc(ref Message m) {
				if(!IsOn) {
					base.WndProc(ref m);
					return;
				}
				switch(m.Msg) {
					case WM_NCCALCSIZE:
						if(m.WParam!=IntPtr.Zero) {
							NCCALCSIZE_PARAMS ncCalcSizeParams=(NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam,typeof(NCCALCSIZE_PARAMS));
							RECT rect=ncCalcSizeParams.rgrc0;
							Rectangle rectangle=GetClientRectangle(Rectangle.FromLTRB(rect.Left,rect.Top,rect.Right,rect.Bottom),IsZoomed(m.HWnd),GetScale(_form));
							ncCalcSizeParams.rgrc0=new RECT { Left=rectangle.Left,Top=rectangle.Top,Right=rectangle.Right,Bottom=rectangle.Bottom };
							Marshal.StructureToPtr(ncCalcSizeParams,m.LParam,false);
							m.Result=IntPtr.Zero;
							return;
						}
						break;
					case WM_NCHITTEST:
						base.WndProc(ref m);
						if(m.Result.ToInt64()==HTCLIENT && Offset>0) {
							Point pointScreen=new Point(unchecked((short)(long)m.LParam),unchecked((short)((long)m.LParam>>16)));
							m.Result=new IntPtr(HitTest(_form,_form.PointToClient(pointScreen)));
						}
						return;
					case WM_NCMOUSEMOVE:
						SetHot(m.WParam.ToInt32());
						if(!_isTrackingLeave) {
							TRACKMOUSEEVENT trackMouseEvent=new TRACKMOUSEEVENT();
							trackMouseEvent.cbSize=Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
							trackMouseEvent.dwFlags=TME_LEAVE|TME_NONCLIENT;
							trackMouseEvent.hwndTrack=m.HWnd;
							_isTrackingLeave=TrackMouseEvent(ref trackMouseEvent);
						}
						break;
					case WM_NCMOUSELEAVE:
						_isTrackingLeave=false;
						SetHot(0);
						break;
					case WM_MOUSEMOVE:
						SetHot(0);
						break;
					case WM_NCLBUTTONDOWN:
					case WM_NCLBUTTONDBLCLK:
						if(GetSysCommand(m.WParam.ToInt32(),false)!=0) {
							//A caption button: Windows' own button tracking would draw Windows' button over the band. The click is the button up.
							_hitPressed=m.WParam.ToInt32();
							m.Result=IntPtr.Zero;
							return;
						}
						_hitPressed=0;
						break;
					case WM_NCLBUTTONUP:
						if(_hitPressed!=0) {
							int hit=m.WParam.ToInt32();
							bool isSame=hit==_hitPressed;
							_hitPressed=0;
							if(isSame) {
								m.Result=IntPtr.Zero;
								SetHot(0);
								SendMessage(m.HWnd,WM_SYSCOMMAND,new IntPtr(GetSysCommand(hit,IsZoomed(m.HWnd))),IntPtr.Zero);
								return;
							}
						}
						break;
					case WM_DPICHANGED:
						OnDpiChanged(ref m);
						return;
				}
				base.WndProc(ref m);
			}

			///<summary>A per-monitor dpi change: the content goes back up to the client area's top, Windows Forms scales the window and its
			///controls to the new dpi and gives it Windows' suggested bounds (the old window, band included, at the new scale), then the content
			///moves down by the new band, within that window.</summary>
			private void OnDpiChanged(ref Message m) {
				int offsetOld=Offset;
				if(offsetOld>0) {
					MoveContent(-offsetOld,0,isCentred:false);
					Offset=0;
				}
				base.WndProc(ref m);
				if(offsetOld>0 && IsOn && !_form.IsDisposed) {
					int offsetNew=GetHeightTitleBar(_form);
					MoveContent(offsetNew,0,isCentred:false);
					Offset=offsetNew;
				}
				if(!_form.IsDisposed) {
					_form.Invalidate();
				}
			}
		}
		#endregion FrameWindow
	}
}
