// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.IO;

namespace CodeBase {
	///<summary>The Windows system folder as a process of either bitness must name it (S3a-12 of the .NET 10 move). WOW64 redirects a 32-bit
	///process on 64-bit Windows from System32 to SysWOW64, the 32-bit system folder; such a process reaches the 64-bit tools through the
	///Sysnative alias, which exists only for a 32-bit process. A 64-bit process, and any process on 32-bit Windows, uses System32 itself.
	///(Microsoft Learn, "File System Redirector".)</summary>
	public static class ODSystemFolders {
		///<summary>True only in a 32-bit process on 64-bit Windows, the one case where WOW64 redirects System32 and where Sysnative exists, and
		///where Wow64DisableWow64FsRedirection has anything to turn off.</summary>
		public static bool IsWow64Process() {
			return IsWow64Process(Environment.Is64BitOperatingSystem,Environment.Is64BitProcess);
		}

		///<summary>IsWow64Process for a given operating system and process bitness, for tests.</summary>
		public static bool IsWow64Process(bool is64BitOperatingSystem,bool is64BitProcess) {
			return is64BitOperatingSystem && !is64BitProcess;
		}

		///<summary>The folder of the 64-bit (native) Windows tools as this process must name it: the Windows folder's Sysnative in a 32-bit
		///process on 64-bit Windows, System32 otherwise.</summary>
		public static string GetNativeSystemFolder() {
			return GetNativeSystemFolder(Environment.GetFolderPath(Environment.SpecialFolder.Windows),IsWow64Process());
		}

		///<summary>GetNativeSystemFolder for a given Windows folder and WOW64 state, for tests.</summary>
		public static string GetNativeSystemFolder(string folderWindows,bool isWow64Process) {
			return Path.Combine(folderWindows,isWow64Process ? "sysnative" : "system32");
		}
	}
}
