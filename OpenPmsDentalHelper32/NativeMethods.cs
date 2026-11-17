// The Windows functions the 32-bit helper program calls (S3a-13 of the .NET 10 move). Pointer-sized values are IntPtr, so the declarations
// hold for a 64-bit build too.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace OpenPmsDentalHelper32 {
	internal static class NativeMethods {
		public const uint SEM_FAILCRITICALERRORS=0x0001;
		public const uint SEM_NOGPFAULTERRORBOX=0x0002;
		public const uint SEM_NOOPENFILEERRORBOX=0x8000;
		public const int SE_KERNEL_OBJECT=6;
		public const uint OWNER_SECURITY_INFORMATION=0x1;
		public const uint DACL_SECURITY_INFORMATION=0x4;
		public const uint SDDL_REVISION_1=1;

		///<summary>https://learn.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-seterrormode (this process only).</summary>
		[DllImport("kernel32.dll")]
		public static extern uint SetErrorMode(uint mode);

		///<summary>https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getnamedpipeserverprocessid</summary>
		[DllImport("kernel32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetNamedPipeServerProcessId(SafePipeHandle pipe,out uint serverProcessId);

		///<summary>https://learn.microsoft.com/en-us/windows/win32/api/aclapi/nf-aclapi-getsecurityinfo; the descriptor is freed with LocalFree.
		///</summary>
		[DllImport("advapi32.dll")]
		public static extern int GetSecurityInfo(SafeHandle handle,int objectType,uint securityInfo,out IntPtr sidOwner,out IntPtr sidGroup,
			out IntPtr dacl,out IntPtr sacl,out IntPtr securityDescriptor);

		[DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ConvertSecurityDescriptorToStringSecurityDescriptor(IntPtr securityDescriptor,uint revision,uint securityInfo,
			out IntPtr stringSecurityDescriptor,out uint length);

		[DllImport("kernel32.dll")]
		public static extern IntPtr LocalFree(IntPtr memory);

		//The message-pumping wait between two requests (S3a-14, MessagePumpingStream).
		public const uint INFINITE=0xFFFFFFFF;
		public const uint WAIT_OBJECT_0=0;
		public const uint WAIT_FAILED=0xFFFFFFFF;
		public const uint QS_ALLINPUT=0x04FF;
		public const uint MWMO_INPUTAVAILABLE=0x0004;
		public const uint PM_REMOVE=0x0001;

		[StructLayout(LayoutKind.Sequential)]
		public struct MSG {
			public IntPtr hwnd;
			public uint message;
			public IntPtr wParam;
			public IntPtr lParam;
			public uint time;
			public int ptX;
			public int ptY;
		}

		///<summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-msgwaitformultipleobjectsex</summary>
		[DllImport("user32.dll",SetLastError=true)]
		public static extern uint MsgWaitForMultipleObjectsEx(uint count,IntPtr[] handles,uint milliseconds,uint wakeMask,uint flags);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool PeekMessage(out MSG msg,IntPtr hwnd,uint messageFilterMin,uint messageFilterMax,uint removeMessage);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool TranslateMessage(ref MSG msg);

		[DllImport("user32.dll")]
		public static extern IntPtr DispatchMessage(ref MSG msg);

		///<summary>https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setforegroundwindow (Schick's exam window; the program
		///allows the helper to set the foreground window first, Helper32Client.AllowSetForeground).</summary>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT {
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		///<summary>The program's window, over which Topaz's signing window is centered.</summary>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(IntPtr hWnd,out RECT rect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindow(IntPtr hWnd);
	}
}
