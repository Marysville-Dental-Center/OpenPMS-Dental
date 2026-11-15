using System;

namespace PasswordVaultWrapper {
	///<summary>The saved Middle Tier logins of the signed-in Windows user on this computer. Since S3a-5 of the .NET 10 move (docs/port-notes/S3a-5.md)
	///they are generic credentials in the Windows Credential Manager (advapi32.dll), which .NET 10 can call too; before, they were in the
	///Windows Runtime PasswordVault, which needed the Windows SDK's Windows.winmd. The first use in a process moves the old vault's entries
	///over once per Windows user (CredentialStore.EnsureMigrated), so nobody has to type a saved password again. The class keeps its name and
	///its four methods for the callers (CentralConnections, the Central Manager). Callers keep their try-catch: every method but
	///TryRetrieveUserName can throw.</summary>
	public class WindowsPasswordVaultWrapper {
		///<summary>Clears all Middle Tier credentials saved for this address for the currently signed in Windows user.</summary>
		public static void ClearCredentials(string uri) {
			CredentialStore.Default.ClearCredentials(uri);
		}

		///<summary>This method will throw an exception if you pass it a blank user name or password. Windows cannot store a blank password. Callers
		///of this method should consider this scenario. Throws exceptions.</summary>
		public static void WritePassword(string uri,string username,string password) {
			CredentialStore.Default.WritePassword(uri,username,password);//The Credential Manager encrypts the password for this Windows user.
		}

		///<summary>Retrieves the first Middle Tier username saved for the URI passed in.</summary>
		public static bool TryRetrieveUserName(string uri,out string username) {
			return CredentialStore.Default.TryRetrieveUserName(uri,out username);
		}

		///<summary>An exception will be thrown if the password cannot be found. Callers of this method should consider this scenario.</summary>
		public static string RetrievePassword(string uri,string username) {
			//This will only return the password if it has been saved under the current Windows user.
			return CredentialStore.Default.RetrievePassword(uri,username);
		}
	}
}
