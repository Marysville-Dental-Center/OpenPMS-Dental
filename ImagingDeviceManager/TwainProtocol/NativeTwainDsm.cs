// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ImagingDeviceManager.TwainProtocol {
	///<summary>The TWAIN Data Source Manager installed on this computer. Prefers the version 2 manager (TWAINDSM.dll, from the
	///TWAIN Working Group, installed by most current scanner drivers) and falls back to the legacy twain_32.dll that ships
	///with Windows; a 64-bit process uses only the 64-bit TWAINDSM.dll of the system folder (GetCandidatePaths). Libraries are loaded by full
	///path so that a stray DLL in the current directory is never picked up.</summary>
	internal sealed class NativeTwainDsm:ITwainDsm {
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate ushort DsmEntryProc(IntPtr origin,IntPtr dest,uint dg,ushort dat,ushort msg,IntPtr data);
		private IntPtr _hModule;
		private DsmEntryProc _dsmEntry;

		public string LibraryName { get; private set; }

		private NativeTwainDsm() {
		}

		///<summary>The full paths tried by this process, in order.</summary>
		public static List<string> GetCandidatePaths() {
			return GetCandidatePaths(Environment.Is64BitProcess);
		}

		///<summary>The full paths tried, in order, by a process of the given bitness. A 32-bit process: TWAINDSM.dll in the program folder, then in
		///the system folder (SysWOW64 for a 32-bit process on 64-bit Windows), then the legacy twain_32.dll in the Windows folder. A 64-bit process:
		///only the 64-bit TWAINDSM.dll in the system folder (System32), never twain_32.dll, which is 32-bit only and cannot load there, and no copy
		///beside the program.</summary>
		public static List<string> GetCandidatePaths(bool is64BitProcess) {
			List<string> listPaths=new List<string>();
			if(is64BitProcess) {
				listPaths.Add(Path.Combine(Environment.SystemDirectory,"TWAINDSM.dll"));
				return listPaths;
			}
			listPaths.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"TWAINDSM.dll"));
			listPaths.Add(Path.Combine(Environment.SystemDirectory,"TWAINDSM.dll"));//SysWOW64 for a 32-bit process on 64-bit Windows
			string windowsDir=Environment.GetEnvironmentVariable("windir");
			if(string.IsNullOrEmpty(windowsDir)) {
				windowsDir=Path.GetDirectoryName(Environment.SystemDirectory);
			}
			listPaths.Add(Path.Combine(windowsDir,"twain_32.dll"));
			return listPaths;
		}

		///<summary>Loads the first data source manager found, in either bitness (GetCandidatePaths). Returns null and sets errorText if none can be
		///loaded.</summary>
		public static NativeTwainDsm Load(out string errorText) {
			errorText="";
			bool is64BitProcess=Environment.Is64BitProcess;
			foreach(string path in GetCandidatePaths(is64BitProcess)) {
				if(!File.Exists(path)) {
					continue;
				}
				IntPtr hModule=NativeMethods.LoadLibrary(path);
				if(hModule==IntPtr.Zero) {
					errorText="Could not load "+path+" (Windows error "+Marshal.GetLastWin32Error()+").";
					continue;
				}
				IntPtr procAddress=NativeMethods.GetProcAddress(hModule,"DSM_Entry");
				if(procAddress==IntPtr.Zero) {
					NativeMethods.FreeLibrary(hModule);
					errorText=path+" does not export DSM_Entry.";
					continue;
				}
				NativeTwainDsm dsm=new NativeTwainDsm();
				dsm._hModule=hModule;
				dsm._dsmEntry=(DsmEntryProc)Marshal.GetDelegateForFunctionPointer(procAddress,typeof(DsmEntryProc));
				dsm.LibraryName=Path.GetFileName(path);
				return dsm;
			}
			if(errorText=="" && is64BitProcess) {
				errorText="No 64-bit TWAIN Data Source Manager is installed (TWAINDSM.dll in "+Environment.SystemDirectory+"). Install the 64-bit "
					+"TWAIN Data Source Manager and the scanner's 64-bit TWAIN driver.";
			}
			if(errorText=="") {
				errorText="No TWAIN Data Source Manager is installed (TWAINDSM.dll or twain_32.dll). Install the scanner's TWAIN driver.";
			}
			return null;
		}

		public ushort Entry(IntPtr origin,IntPtr dest,uint dg,ushort dat,ushort msg,IntPtr data) {
			if(_dsmEntry==null) {
				throw new ObjectDisposedException(nameof(NativeTwainDsm));
			}
			return _dsmEntry(origin,dest,dg,dat,msg,data);
		}

		public void Dispose() {
			_dsmEntry=null;
			if(_hModule!=IntPtr.Zero) {
				NativeMethods.FreeLibrary(_hModule);
				_hModule=IntPtr.Zero;
			}
		}
	}
}
