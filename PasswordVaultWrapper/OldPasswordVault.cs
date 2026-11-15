// The Windows Runtime PasswordVault that the program used before S3a-5 (docs/specs/net10-plan.md), read only for the one-time move to the
// Credential Manager. On .NET Framework the Windows Runtime types are found through the operating system's own metadata, so no Windows.winmd
// reference is needed: the type is looked up by name and called by reflection. .NET 5 and later have no such support, so there the vault is
// never opened (TryOpen returns null).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PasswordVaultWrapper {
	///<summary>One entry of the old vault, listed without its password.</summary>
	internal class OldVaultEntry {
		public string Resource;
		public string UserName;
		///<summary>The Windows Runtime PasswordCredential object.</summary>
		public object Credential;
	}

	///<summary>What the store needs of the old vault; OldPasswordVault is the real one, the tests give a fake (S3a-5f: a vault that fails
	///single entries).</summary>
	internal interface IOldVault {
		List<OldVaultEntry> RetrieveAll();
		List<OldVaultEntry> FindAllByResource(string resource);
		string RetrievePassword(OldVaultEntry oldVaultEntry);
		void Remove(OldVaultEntry oldVaultEntry);
	}

	internal class OldPasswordVault : IOldVault {
		private const string TYPE_VAULT="Windows.Security.Credentials.PasswordVault, Windows.Security.Credentials, ContentType=WindowsRuntime";
		private const string TYPE_CREDENTIAL="Windows.Security.Credentials.PasswordCredential, Windows.Security.Credentials, ContentType=WindowsRuntime";
		///<summary>HRESULT_FROM_WIN32(ERROR_NOT_FOUND): what the vault throws when it has no entry to return.</summary>
		public const int HRESULT_NOT_FOUND=unchecked((int)0x80070490);

		private readonly Type _typeVault;
		private readonly Type _typeCredential;
		private readonly object _vault;

		private OldPasswordVault(Type typeVault,Type typeCredential,object vault) {
			_typeVault=typeVault;
			_typeCredential=typeCredential;
			_vault=vault;
		}

		///<summary>The old vault of the signed-in Windows user, or null where it cannot be opened: always on .NET 5 and later; on .NET Framework
		///for any error (on Windows 8 and later that is an error that may pass, see IsOsWithoutVault).</summary>
		public static OldPasswordVault TryOpen() {
#if NETFRAMEWORK
			try {
				Type typeVault=Type.GetType(TYPE_VAULT,throwOnError:false);
				Type typeCredential=Type.GetType(TYPE_CREDENTIAL,throwOnError:false);
				if(typeVault==null || typeCredential==null) {
					return null;
				}
				return new OldPasswordVault(typeVault,typeCredential,Activator.CreateInstance(typeVault));
			}
			catch(Exception) {
				return null;
			}
#else
			return null;
#endif
		}

		///<summary>True on a Windows older than 8 (version 6.2), which has no PasswordVault. A program without a compatibility manifest is told
		///6.2 on Windows 8.1 and later, so the test holds there too.</summary>
		public static bool IsOsWithoutVault() {
			return Environment.OSVersion.Platform!=PlatformID.Win32NT || Environment.OSVersion.Version<new Version(6,2);
		}

		///<summary>Every entry of the vault, names only (the vault returns no password until RetrievePassword is called). An empty vault gives an
		///empty list. Throws the vault's own exception for any other error.</summary>
		public List<OldVaultEntry> RetrieveAll() {
			return ToEntries(() => Call(_typeVault,_vault,"RetrieveAll"));
		}

		///<summary>The entries of one resource, names only; empty when there are none.</summary>
		public List<OldVaultEntry> FindAllByResource(string resource) {
			return ToEntries(() => Call(_typeVault,_vault,"FindAllByResource",resource));
		}

		///<summary>The password of one entry. Callers filter by their own resource name before they call this.</summary>
		public string RetrievePassword(OldVaultEntry oldVaultEntry) {
			Call(_typeCredential,oldVaultEntry.Credential,"RetrievePassword");
			return (string)GetProperty(_typeCredential,oldVaultEntry.Credential,"Password")??"";
		}

		///<summary>Adds an entry (the tests write their own entries this way, under their own prefix).</summary>
		public void Add(string resource,string userName,string password) {
			object credential=Activator.CreateInstance(_typeCredential,resource,userName,password);
			Call(_typeVault,_vault,"Add",credential);
		}

		public void Remove(OldVaultEntry oldVaultEntry) {
			Call(_typeVault,_vault,"Remove",oldVaultEntry.Credential);
		}

		private List<OldVaultEntry> ToEntries(Func<object> funcList) {
			List<OldVaultEntry> listEntries=new List<OldVaultEntry>();
			object list;
			try {
				list=funcList();
			}
			catch(Exception ex) {
				if(ex.HResult==HRESULT_NOT_FOUND) {
					return listEntries;
				}
				throw;
			}
			if(list==null) {
				return listEntries;
			}
			foreach(object credential in (IEnumerable)list) {
				OldVaultEntry oldVaultEntry=new OldVaultEntry();
				oldVaultEntry.Resource=(string)GetProperty(_typeCredential,credential,"Resource")??"";
				oldVaultEntry.UserName=(string)GetProperty(_typeCredential,credential,"UserName")??"";
				oldVaultEntry.Credential=credential;
				listEntries.Add(oldVaultEntry);
			}
			return listEntries;
		}

		///<summary>Calls a method by reflection and throws the vault's own exception instead of the TargetInvocationException around it.</summary>
		private static object Call(Type type,object target,string methodName,params object[] arrayArgs) {
			//By name and count: a credential object's own run-time type may be the COM wrapper rather than the projected class.
			MethodInfo methodInfo=type.GetMethods().FirstOrDefault(x => x.Name==methodName && x.GetParameters().Length==arrayArgs.Length);
			if(methodInfo==null) {
				throw new MissingMethodException(type.FullName,methodName);
			}
			try {
				return methodInfo.Invoke(target,arrayArgs);
			}
			catch(TargetInvocationException ex) when(ex.InnerException!=null) {
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw;//Not reached.
			}
		}

		private static object GetProperty(Type type,object target,string propertyName) {
			try {
				return type.GetProperty(propertyName).GetValue(target);
			}
			catch(TargetInvocationException ex) when(ex.InnerException!=null) {
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				throw;//Not reached.
			}
		}
	}
}
