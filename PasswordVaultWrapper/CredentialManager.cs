// The Windows Credential Manager through advapi32.dll (package S3a-5 of docs/specs/net10-plan.md): generic credentials of the signed-in
// Windows user on this computer. Pointer-size safe (IntPtr fields), so it runs unchanged in the 64-bit program of stage 3b.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace PasswordVaultWrapper {
	///<summary>One generic credential as the Credential Manager returns it.</summary>
	internal class CredentialEntry {
		public string TargetName;
		public string UserName;
		///<summary>The secret, decoded from UTF-16. Empty when the entry was listed without it.</summary>
		public string Secret;
		public DateTime LastWrittenUtc;
	}

	///<summary>Thin wrappers over CredWriteW, CredReadW, CredDeleteW and CredEnumerateW for CRED_TYPE_GENERIC. Each method throws a
	///Win32Exception on a Windows error other than "not found".</summary>
	internal static class CredentialManager {
		private const int CRED_TYPE_GENERIC=1;
		///<summary>Persists for this Windows user on this computer only; never roams to another computer.</summary>
		private const int CRED_PERSIST_LOCAL_MACHINE=2;
		public const int ERROR_NOT_FOUND=1168;
		///<summary>CRED_MAX_CREDENTIAL_BLOB_SIZE: 5*512 bytes, 1280 UTF-16 characters.</summary>
		public const int MAX_SECRET_BYTES=5*512;

		[StructLayout(LayoutKind.Sequential)]
		private struct CREDENTIAL {
			public int Flags;
			public int Type;
			public IntPtr TargetName;
			public IntPtr Comment;
			public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
			public int CredentialBlobSize;
			public IntPtr CredentialBlob;
			public int Persist;
			public int AttributeCount;
			public IntPtr Attributes;
			public IntPtr TargetAlias;
			public IntPtr UserName;
		}

		[DllImport("advapi32.dll",EntryPoint="CredWriteW",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CredWrite(ref CREDENTIAL credential,int flags);

		[DllImport("advapi32.dll",EntryPoint="CredReadW",CharSet=CharSet.Unicode,SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CredRead(string targetName,int type,int reservedFlag,out IntPtr credential);

		[DllImport("advapi32.dll",EntryPoint="CredDeleteW",CharSet=CharSet.Unicode,SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CredDelete(string targetName,int type,int flags);

		[DllImport("advapi32.dll",EntryPoint="CredEnumerateW",CharSet=CharSet.Unicode,SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CredEnumerate(string filter,int flags,out int count,out IntPtr credentials);

		[DllImport("advapi32.dll",EntryPoint="CredFree")]
		private static extern void CredFree(IntPtr buffer);

		///<summary>Creates or replaces the generic credential with this target name. Throws for a blank target or a secret over the limit.</summary>
		public static void Write(string targetName,string userName,string secret) {
			if(string.IsNullOrEmpty(targetName)) {
				throw new ArgumentException("A credential needs a target name.",nameof(targetName));
			}
			byte[] arrayBytes=Encoding.Unicode.GetBytes(secret??"");
			if(arrayBytes.Length>MAX_SECRET_BYTES) {
				throw new ArgumentException("The Windows Credential Manager stores at most "+(MAX_SECRET_BYTES/2)+" characters.",nameof(secret));
			}
			IntPtr ptrTarget=IntPtr.Zero;
			IntPtr ptrUser=IntPtr.Zero;
			IntPtr ptrBlob=IntPtr.Zero;
			try {
				ptrTarget=Marshal.StringToCoTaskMemUni(targetName);
				if(!string.IsNullOrEmpty(userName)) {
					ptrUser=Marshal.StringToCoTaskMemUni(userName);
				}
				if(arrayBytes.Length>0) {
					ptrBlob=Marshal.AllocCoTaskMem(arrayBytes.Length);
					Marshal.Copy(arrayBytes,0,ptrBlob,arrayBytes.Length);
				}
				CREDENTIAL credential=new CREDENTIAL();
				credential.Type=CRED_TYPE_GENERIC;
				credential.TargetName=ptrTarget;
				credential.UserName=ptrUser;
				credential.CredentialBlobSize=arrayBytes.Length;
				credential.CredentialBlob=ptrBlob;
				credential.Persist=CRED_PERSIST_LOCAL_MACHINE;
				if(!CredWrite(ref credential,0)) {
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
			finally {
				if(ptrBlob!=IntPtr.Zero) {
					Marshal.Copy(new byte[arrayBytes.Length],0,ptrBlob,arrayBytes.Length);//Leave no copy of the secret in freed memory.
					Marshal.FreeCoTaskMem(ptrBlob);
				}
				if(ptrUser!=IntPtr.Zero) {
					Marshal.FreeCoTaskMem(ptrUser);
				}
				if(ptrTarget!=IntPtr.Zero) {
					Marshal.FreeCoTaskMem(ptrTarget);
				}
				Array.Clear(arrayBytes,0,arrayBytes.Length);
			}
		}

		///<summary>The generic credential with exactly this target name, with its secret; null when there is none.</summary>
		public static CredentialEntry Read(string targetName) {
			IntPtr ptrCredential;
			if(!CredRead(targetName,CRED_TYPE_GENERIC,0,out ptrCredential)) {
				int error=Marshal.GetLastWin32Error();
				if(error==ERROR_NOT_FOUND) {
					return null;
				}
				throw new Win32Exception(error);
			}
			try {
				return ToEntry(ptrCredential,doReadSecret:true);
			}
			finally {
				CredFree(ptrCredential);
			}
		}

		///<summary>The generic credential with this target name, without its secret (the blob is never decoded); null when there is none. Used
		///where only the existence or the user name of an entry matters.</summary>
		public static CredentialEntry ReadInfo(string targetName) {
			IntPtr ptrCredential;
			if(!CredRead(targetName,CRED_TYPE_GENERIC,0,out ptrCredential)) {
				int error=Marshal.GetLastWin32Error();
				if(error==ERROR_NOT_FOUND) {
					return null;
				}
				throw new Win32Exception(error);
			}
			try {
				return ToEntry(ptrCredential,doReadSecret:false);
			}
			finally {
				CredFree(ptrCredential);
			}
		}

		///<summary>Deletes the generic credential with exactly this target name. Returns false when there was none.</summary>
		public static bool Delete(string targetName) {
			if(CredDelete(targetName,CRED_TYPE_GENERIC,0)) {
				return true;
			}
			int error=Marshal.GetLastWin32Error();
			if(error==ERROR_NOT_FOUND) {
				return false;
			}
			throw new Win32Exception(error);
		}

		///<summary>The generic credentials whose target name starts with the prefix (Windows filters by "prefix*", and every entry is checked again
		///here), without their secrets. The prefix must not be blank: nothing outside a caller's own names is ever listed.</summary>
		public static List<CredentialEntry> List(string targetPrefix) {
			if(string.IsNullOrEmpty(targetPrefix)) {
				throw new ArgumentException("A prefix is required.",nameof(targetPrefix));
			}
			List<CredentialEntry> listEntries=new List<CredentialEntry>();
			int count;
			IntPtr ptrArray;
			if(!CredEnumerate(targetPrefix+"*",0,out count,out ptrArray)) {
				int error=Marshal.GetLastWin32Error();
				if(error==ERROR_NOT_FOUND) {
					return listEntries;
				}
				throw new Win32Exception(error);
			}
			try {
				for(int i=0;i<count;i++) {
					IntPtr ptrCredential=Marshal.ReadIntPtr(ptrArray,i*IntPtr.Size);
					CREDENTIAL credential=(CREDENTIAL)Marshal.PtrToStructure(ptrCredential,typeof(CREDENTIAL));
					if(credential.Type!=CRED_TYPE_GENERIC) {
						continue;
					}
					string targetName=Marshal.PtrToStringUni(credential.TargetName)??"";
					//Windows matches target names (and its filter) without regard to case (S3a-5f).
					if(!targetName.StartsWith(targetPrefix,StringComparison.OrdinalIgnoreCase)) {
						continue;
					}
					listEntries.Add(ToEntry(ptrCredential,doReadSecret:false));
				}
			}
			finally {
				CredFree(ptrArray);
			}
			return listEntries;
		}

		private static CredentialEntry ToEntry(IntPtr ptrCredential,bool doReadSecret) {
			CREDENTIAL credential=(CREDENTIAL)Marshal.PtrToStructure(ptrCredential,typeof(CREDENTIAL));
			CredentialEntry credentialEntry=new CredentialEntry();
			credentialEntry.TargetName=Marshal.PtrToStringUni(credential.TargetName)??"";
			credentialEntry.UserName=credential.UserName==IntPtr.Zero ? "" : Marshal.PtrToStringUni(credential.UserName);
			credentialEntry.Secret="";
			if(doReadSecret && credential.CredentialBlob!=IntPtr.Zero && credential.CredentialBlobSize>0) {
				byte[] arrayBytes=new byte[credential.CredentialBlobSize];
				Marshal.Copy(credential.CredentialBlob,arrayBytes,0,arrayBytes.Length);
				credentialEntry.Secret=Encoding.Unicode.GetString(arrayBytes);
				Array.Clear(arrayBytes,0,arrayBytes.Length);
			}
			long fileTime=((long)(uint)credential.LastWritten.dwHighDateTime<<32)|(uint)credential.LastWritten.dwLowDateTime;
			credentialEntry.LastWrittenUtc=fileTime>0 ? DateTime.FromFileTimeUtc(fileTime) : DateTime.MinValue;
			return credentialEntry;
		}
	}
}
