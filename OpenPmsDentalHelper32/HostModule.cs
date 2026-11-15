// The helper's own module (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md, section 3): who it is, and how its pipe is secured.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace OpenPmsDentalHelper32 {
	///<summary>Ping (the protocol version, the process id, whether it is a 64-bit process, the runtime's version) and PipeSecurity (the owner and
	///DACL of the pipe in SDDL, read from the pipe itself), for the program's diagnostics and the tests.</summary>
	internal sealed class HostModule:IHelper32Module {
		private readonly PipeStream _pipe;

		public HostModule(PipeStream pipe) {
			_pipe=pipe;
		}

		public string Name {
			get {
				return Helper32Protocol.MODULE_HOST;
			}
		}

		public Helper32Reply Handle(Helper32Request request) {
			switch(request.Command) {
				case Helper32Protocol.COMMAND_PING:
					return Helper32Reply.Ok(new[] {
						Helper32Protocol.VERSION.ToString(CultureInfo.InvariantCulture),
						Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture),
						Environment.Is64BitProcess ? "1" : "0",
						Environment.Version.ToString()
					},null);
				case Helper32Protocol.COMMAND_PIPE_SECURITY:
					return Helper32Reply.Ok(new[] { GetPipeSecuritySddl() },null);
				default:
					throw new InvalidOperationException("The helper's "+Name+" module has no command "+request.Command+".");
			}
		}

		private string GetPipeSecuritySddl() {
			int result=NativeMethods.GetSecurityInfo(_pipe.SafePipeHandle,NativeMethods.SE_KERNEL_OBJECT,
				NativeMethods.OWNER_SECURITY_INFORMATION|NativeMethods.DACL_SECURITY_INFORMATION,
				out IntPtr _,out IntPtr _,out IntPtr _,out IntPtr _,out IntPtr securityDescriptor);
			if(result!=0) {
				throw new System.ComponentModel.Win32Exception(result);
			}
			try {
				if(!NativeMethods.ConvertSecurityDescriptorToStringSecurityDescriptor(securityDescriptor,NativeMethods.SDDL_REVISION_1,
					NativeMethods.OWNER_SECURITY_INFORMATION|NativeMethods.DACL_SECURITY_INFORMATION,out IntPtr sddl,out uint _))
				{
					throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
				}
				try {
					return Marshal.PtrToStringUni(sddl);
				}
				finally {
					NativeMethods.LocalFree(sddl);
				}
			}
			finally {
				NativeMethods.LocalFree(securityDescriptor);
			}
		}
	}
}
