// The processes of this machine with their parents (a Toolhelp snapshot), for the checks that no helper a test started is left behind.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Helper32Tests.Support {
	public sealed class ProcessEntry {
		public int ProcessId;
		public int ParentProcessId;
		public string ExeFile;
	}

	public static class ProcessList {
		///<summary>Every process running now, with its parent's id and its exe's file name.</summary>
		public static List<ProcessEntry> GetAll() {
			List<ProcessEntry> listEntries=new List<ProcessEntry>();
			IntPtr snapshot=CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS,0);
			if(snapshot==INVALID_HANDLE_VALUE) {
				throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
			}
			try {
				PROCESSENTRY32W entry=new PROCESSENTRY32W();
				entry.dwSize=(uint)Marshal.SizeOf(typeof(PROCESSENTRY32W));
				if(!Process32FirstW(snapshot,ref entry)) {
					return listEntries;
				}
				do {
					listEntries.Add(new ProcessEntry { ProcessId=(int)entry.th32ProcessID,ParentProcessId=(int)entry.th32ParentProcessID,
						ExeFile=entry.szExeFile });
				}
				while(Process32NextW(snapshot,ref entry));
			}
			finally {
				CloseHandle(snapshot);
			}
			return listEntries;
		}

		///<summary>The processes whose exe's file name starts with prefix (case-insensitive) and that are this process or one of its descendants.
		///</summary>
		public static List<ProcessEntry> GetInOwnTree(string prefix) {
			List<ProcessEntry> listAll=GetAll();
			HashSet<int> hashSetTree=new HashSet<int> { Process.GetCurrentProcess().Id };
			bool isAdded=true;
			while(isAdded) {
				isAdded=false;
				foreach(ProcessEntry entry in listAll) {
					if(!hashSetTree.Contains(entry.ProcessId) && hashSetTree.Contains(entry.ParentProcessId) && entry.ProcessId!=entry.ParentProcessId) {
						hashSetTree.Add(entry.ProcessId);
						isAdded=true;
					}
				}
			}
			return listAll.Where(x => hashSetTree.Contains(x.ProcessId) && x.ProcessId!=Process.GetCurrentProcess().Id
				&& x.ExeFile.StartsWith(prefix,StringComparison.OrdinalIgnoreCase)).ToList();
		}

		///<summary>The processes whose exe's file name is exactly exeFile (case-insensitive), anywhere on the machine.</summary>
		public static List<ProcessEntry> GetNamed(string exeFile) {
			return GetAll().Where(x => string.Equals(x.ExeFile,exeFile,StringComparison.OrdinalIgnoreCase)).ToList();
		}

		///<summary>The parent's id of processId, or 0 when it is not running.</summary>
		public static int GetParentId(int processId) {
			ProcessEntry entry=GetAll().FirstOrDefault(x => x.ProcessId==processId);
			return entry==null ? 0 : entry.ParentProcessId;
		}

		private const uint TH32CS_SNAPPROCESS=0x00000002;
		private static readonly IntPtr INVALID_HANDLE_VALUE=new IntPtr(-1);

		[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]
		private struct PROCESSENTRY32W {
			public uint dwSize;
			public uint cntUsage;
			public uint th32ProcessID;
			public UIntPtr th32DefaultHeapID;
			public uint th32ModuleID;
			public uint cntThreads;
			public uint th32ParentProcessID;
			public int pcPriClassBase;
			public uint dwFlags;
			[MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)]
			public string szExeFile;
		}

		[DllImport("kernel32.dll",SetLastError=true)]
		private static extern IntPtr CreateToolhelp32Snapshot(uint flags,uint processId);

		[DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool Process32FirstW(IntPtr snapshot,ref PROCESSENTRY32W entry);

		[DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool Process32NextW(IntPtr snapshot,ref PROCESSENTRY32W entry);

		[DllImport("kernel32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle(IntPtr handle);
	}
}
