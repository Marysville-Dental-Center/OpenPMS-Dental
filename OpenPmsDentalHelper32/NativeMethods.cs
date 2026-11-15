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
	}
}
