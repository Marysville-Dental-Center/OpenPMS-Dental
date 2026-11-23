// This file is part of the open source dental practice management software derived from Open Dental.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ImagingDeviceManager.Config {
	///<summary>The folders under %ProgramData%\OpenPMS-Dental\Imaging that hold patient images not yet filed into the patient's images:
	///Photos\unsaved (camera photos and videos that could not be saved), Video\recording (a recording in progress, or one a crash cut
	///off) and Capture (the X-ray capture journal, quarantine and resolved captures, and the capture log). Each gets an access list of
	///its own, not inherited from ProgramData: SYSTEM and Administrators Full control, and the people who use Open PMS - Dental
	///Modify, inherited by everything inside. The people who use it are the local group "OpenPMS-Dental Users" when an administrator
	///has made it, otherwise the local Users group. Nothing here creates that group or changes any other folder or setting.
	///
	///LocalCaptureConfig.EnsureImagingFolder sets the list when it creates one of these folders, or finds one without it and this
	///Windows user may change it (the folder's owner, or an elevated process). Setup sets it as administrator on folders that exist.</summary>
	public static class ImagingFolderAcl {
		///<summary>The local group whose members are the people who use Open PMS - Dental on this computer. Optional: an administrator
		///makes it. Setup (installer\OpenPmsDental.iss) names the same group.</summary>
		public const string GroupName="OpenPMS-Dental Users";
		///<summary>The private folders, relative to the Imaging folder. Everything under them is private too.</summary>
		public static readonly ReadOnlyCollection<string> ListPrivateRoots=new ReadOnlyCollection<string>(new[] {
			@"Photos\unsaved",
			@"Video\recording",
			"Capture",
		});
		private static readonly SecurityIdentifier _sidSystem=new SecurityIdentifier(WellKnownSidType.LocalSystemSid,null);
		private static readonly SecurityIdentifier _sidAdministrators=new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid,null);
		private static readonly SecurityIdentifier _sidUsers=new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid,null);

		///<summary>The private root that subfolderRelative is, or lies under ("Capture" for @"Capture\journal"), as written in
		///ListPrivateRoots. Null for any other folder, and for "" or null (the Imaging folder). '/' counts as '\', case is ignored.</summary>
		public static string GetPrivateRoot(string subfolderRelative) {
			string subfolder=Normalize(subfolderRelative);
			if(subfolder=="") {
				return null;
			}
			foreach(string root in ListPrivateRoots) {
				if(subfolder.Equals(root,StringComparison.OrdinalIgnoreCase)
					|| subfolder.StartsWith(root+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
				{
					return root;
				}
			}
			return null;
		}

		///<summary>The last look-up of the group, and when it was made (UTC). GetAccessSid reuses it for a minute: it is asked for every
		///capture folder on every write.</summary>
		private static SecurityIdentifier _sidAccessCached;
		private static DateTime _dateTimeCachedUtc=DateTime.MinValue;
		private static readonly object _lockCache=new object();

		///<summary>The SID that gets Modify: the local group GroupName when it exists on this computer, otherwise the local Users group
		///(S-1-5-32-545). Looked up at most once a minute. Never throws.</summary>
		public static SecurityIdentifier GetAccessSid() {
			lock(_lockCache) {
				DateTime dateTimeNowUtc=DateTime.UtcNow;
				if(_sidAccessCached==null || dateTimeNowUtc-_dateTimeCachedUtc>TimeSpan.FromMinutes(1) || dateTimeNowUtc<_dateTimeCachedUtc) {
					_sidAccessCached=GetAccessSid(LookUpGroup);
					_dateTimeCachedUtc=dateTimeNowUtc;
				}
				return _sidAccessCached;
			}
		}

		///<summary>True when this Windows user may use a private folder: sidAccess, Administrators or SYSTEM is an enabled group of this
		///process's token. An administrator's unelevated token does not count as Administrators. A user just added to the group counts
		///only after signing out and in again, since the token is made at sign-in. Never throws; false when it cannot be checked.</summary>
		public static bool IsCurrentUserAllowed(SecurityIdentifier sidAccess) {
			try {
				using(WindowsIdentity windowsIdentity=WindowsIdentity.GetCurrent()) {
					WindowsPrincipal windowsPrincipal=new WindowsPrincipal(windowsIdentity);
					return (sidAccess!=null && windowsPrincipal.IsInRole(sidAccess)) || windowsPrincipal.IsInRole(_sidAdministrators)
						|| _sidSystem.Equals(windowsIdentity.User);
				}
			}
			catch {
				return false;
			}
		}

		///<summary>True when folder, or a folder between it and folderTop (folderTop included), is a reparse point: a junction or symbolic
		///link, which anyone allowed to change the Imaging folder can put there. Setting an access list through one would change the
		///folder it points to, so nothing is set. Also true when folder is not inside folderTop, or when an attribute cannot be read.
		///Never throws.</summary>
		public static bool HasReparsePoint(string folderTop,string folder) {
			try {
				string pathTop=Path.GetFullPath(folderTop).TrimEnd(Path.DirectorySeparatorChar);
				string pathCur=Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar);
				if(!pathCur.Equals(pathTop,StringComparison.OrdinalIgnoreCase)
					&& !pathCur.StartsWith(pathTop+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				while(true) {
					if((File.GetAttributes(pathCur) & FileAttributes.ReparsePoint)!=0) {
						return true;
					}
					if(pathCur.Length<=pathTop.Length) {
						return false;
					}
					pathCur=Path.GetDirectoryName(pathCur);
				}
			}
			catch {
				return true;
			}
		}

		///<summary>GetAccessSid with the group look-up given, for tests. funcLookUpGroup returns the group's SID or null; an exception counts
		///as null.</summary>
		internal static SecurityIdentifier GetAccessSid(Func<SecurityIdentifier> funcLookUpGroup) {
			try {
				SecurityIdentifier sidGroup=funcLookUpGroup?.Invoke();
				if(sidGroup!=null) {
					return sidGroup;
				}
			}
			catch {
			}
			return _sidUsers;
		}

		///<summary>The local group GroupName's SID, or null when this computer has no such group. The computer name is given, so a domain
		///group of the same name is never used.</summary>
		private static SecurityIdentifier LookUpGroup() {
			try {
				return (SecurityIdentifier)new NTAccount(Environment.MachineName,GroupName).Translate(typeof(SecurityIdentifier));
			}
			catch {
				return null;
			}
		}

		///<summary>True when every Allow rule on the folder, its own or inherited, is for SYSTEM, Administrators or sidAccess, and SYSTEM and
		///Administrators have Full control and sidAccess Modify, each for the folder and everything in it, and sidAccess has no more than
		///Modify in any of its rules (Full control would let any member change the list). A folder inside a private folder passes through
		///the rules it inherits. Deny rules are ignored (they only take access away). False when the folder is missing or its access list
		///cannot be read. Never throws.</summary>
		public static bool IsPrivate(string folder,SecurityIdentifier sidAccess) {
			try {
				if(sidAccess==null || !Directory.Exists(folder)) {
					return false;
				}
				AuthorizationRuleCollection authorizationRuleCollection=new DirectoryInfo(folder).GetAccessControl(AccessControlSections.Access)
					.GetAccessRules(true,true,typeof(SecurityIdentifier));
				Dictionary<SecurityIdentifier,FileSystemRights> dictRights=new Dictionary<SecurityIdentifier,FileSystemRights> {
					{ _sidSystem,0 },
					{ _sidAdministrators,0 },
					{ sidAccess,0 },
				};
				const InheritanceFlags inheritanceFlagsBoth=InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit;
				foreach(FileSystemAccessRule fileSystemAccessRule in authorizationRuleCollection) {
					if(fileSystemAccessRule.AccessControlType!=AccessControlType.Allow) {
						continue;
					}
					SecurityIdentifier sid=fileSystemAccessRule.IdentityReference as SecurityIdentifier;
					if(sid==null || !dictRights.ContainsKey(sid)) {
						return false;
					}
					if(sid.Equals(sidAccess) && !sid.Equals(_sidSystem) && !sid.Equals(_sidAdministrators)
						&& (fileSystemAccessRule.FileSystemRights & ~(FileSystemRights.Modify|FileSystemRights.Synchronize))!=0)
					{
						return false;
					}
					if((fileSystemAccessRule.InheritanceFlags & inheritanceFlagsBoth)!=inheritanceFlagsBoth
						|| (fileSystemAccessRule.PropagationFlags & (PropagationFlags.InheritOnly|PropagationFlags.NoPropagateInherit))!=0)
					{
						continue;//applies to the folder only, or only to what is inside: not counted, but allowed (it is one of the three)
					}
					dictRights[sid]|=fileSystemAccessRule.FileSystemRights;
				}
				return HasAll(dictRights[_sidSystem],FileSystemRights.FullControl)
					&& HasAll(dictRights[_sidAdministrators],FileSystemRights.FullControl)
					&& HasAll(dictRights[sidAccess],FileSystemRights.Modify);
			}
			catch {
				return false;
			}
		}

		///<summary>Replaces the folder's access list with its own (not inherited) list: SYSTEM and Administrators Full control, sidAccess
		///Modify, each inherited by everything in the folder. Windows passes the new list on to the files and folders inside that inherit
		///their access. The owner is not changed. Works when this Windows user owns the folder or runs elevated. Returns "" or the error.
		///Never throws.</summary>
		public static string MakePrivate(string folder,SecurityIdentifier sidAccess) {
			try {
				if(sidAccess==null) {
					return "No group to give access to.";
				}
				DirectorySecurity directorySecurity=new DirectorySecurity();
				directorySecurity.SetAccessRuleProtection(true,false);
				const InheritanceFlags inheritanceFlagsBoth=InheritanceFlags.ContainerInherit|InheritanceFlags.ObjectInherit;
				directorySecurity.AddAccessRule(new FileSystemAccessRule(_sidSystem,FileSystemRights.FullControl,inheritanceFlagsBoth,
					PropagationFlags.None,AccessControlType.Allow));
				directorySecurity.AddAccessRule(new FileSystemAccessRule(_sidAdministrators,FileSystemRights.FullControl,inheritanceFlagsBoth,
					PropagationFlags.None,AccessControlType.Allow));
				if(!sidAccess.Equals(_sidSystem) && !sidAccess.Equals(_sidAdministrators)) {
					directorySecurity.AddAccessRule(new FileSystemAccessRule(sidAccess,FileSystemRights.Modify,inheritanceFlagsBoth,
						PropagationFlags.None,AccessControlType.Allow));
				}
				new DirectoryInfo(folder).SetAccessControl(directorySecurity);
				return "";
			}
			catch(Exception ex) {
				return ex.Message;
			}
		}

		private static bool HasAll(FileSystemRights fileSystemRights,FileSystemRights fileSystemRightsWanted) {
			return (fileSystemRights & fileSystemRightsWanted)==fileSystemRightsWanted;
		}

		private static string Normalize(string subfolderRelative) {
			return (subfolderRelative ?? "").Trim().Replace(Path.AltDirectorySeparatorChar,Path.DirectorySeparatorChar)
				.TrimEnd(Path.DirectorySeparatorChar);
		}
	}
}
