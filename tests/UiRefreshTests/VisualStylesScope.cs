// UI-13 fix round (docs/port-notes/UI-13.md): the program runs with Windows' visual styles (comctl32 version 6, Application.EnableVisualStyles in
// each exe's Main); this test process does not, so the native parts the looks leave to Windows (tree and list views, the month calendar, the
// stock list's items) drew their Windows-classic picture in the gallery. This scope activates a comctl32 version 6 activation context on the
// calling thread while it is open: every window created on that thread meanwhile is the version 6 control the program shows. It changes no
// process-wide setting (Application.UseVisualStyles stays as it is), so the other tests of the run draw as before.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace UiRefreshTests {
	///<summary>A comctl32 version 6 activation context on this thread while open (the windows created meanwhile have Windows' visual styles).
	///</summary>
	public sealed class VisualStylesScope : IDisposable {
		private const string MANIFEST="<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n"
			+"<assembly xmlns=\"urn:schemas-microsoft-com:asm.v1\" manifestVersion=\"1.0\">\r\n"
			+"<dependency><dependentAssembly><assemblyIdentity type=\"win32\" name=\"Microsoft.Windows.Common-Controls\" version=\"6.0.0.0\""
			+" processorArchitecture=\"*\" publicKeyToken=\"6595b64144ccf1df\" language=\"*\"/></dependentAssembly></dependency>\r\n"
			+"</assembly>\r\n";
		private static readonly IntPtr INVALID_HANDLE_VALUE=new IntPtr(-1);
		private IntPtr _hActCtx=IntPtr.Zero;
		private IntPtr _cookie=IntPtr.Zero;

		///<summary>True when the context is active (false when Windows refused it: the windows then draw as without the scope).</summary>
		public bool IsActive {
			get { return _cookie!=IntPtr.Zero; }
		}

		public VisualStylesScope() {
			string path=Path.Combine(Path.GetTempPath(),"opms-uirefresh-comctl6.manifest");
			try {
				File.WriteAllText(path,MANIFEST);
				ACTCTX actCtx=new ACTCTX();
				actCtx.cbSize=Marshal.SizeOf(typeof(ACTCTX));
				actCtx.lpSource=path;
				_hActCtx=CreateActCtxW(ref actCtx);
				if(_hActCtx==INVALID_HANDLE_VALUE) {
					_hActCtx=IntPtr.Zero;
					return;
				}
				if(!ActivateActCtx(_hActCtx,out _cookie)) {
					_cookie=IntPtr.Zero;
				}
			}
			catch(Exception) {
				_cookie=IntPtr.Zero;
			}
		}

		public void Dispose() {
			if(_cookie!=IntPtr.Zero) {
				DeactivateActCtx(0,_cookie);
				_cookie=IntPtr.Zero;
			}
			if(_hActCtx!=IntPtr.Zero) {
				ReleaseActCtx(_hActCtx);
				_hActCtx=IntPtr.Zero;
			}
		}

		[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
		private struct ACTCTX {
			public int cbSize;
			public uint dwFlags;
			public string lpSource;
			public ushort wProcessorArchitecture;
			public ushort wLangId;
			public string lpAssemblyDirectory;
			public IntPtr lpResourceName;
			public string lpApplicationName;
			public IntPtr hModule;
		}

		[DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
		private static extern IntPtr CreateActCtxW(ref ACTCTX actctx);

		[DllImport("kernel32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool ActivateActCtx(IntPtr hActCtx,out IntPtr lpCookie);

		[DllImport("kernel32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeactivateActCtx(uint dwFlags,IntPtr ulCookie);

		[DllImport("kernel32.dll")]
		private static extern void ReleaseActCtx(IntPtr hActCtx);
	}
}
