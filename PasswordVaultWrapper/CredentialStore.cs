// The saved Middle Tier logins in the Windows Credential Manager, and the one-time move of the old PasswordVault entries (package S3a-5 of
// docs/specs/net10-plan.md; docs/port-notes/S3a-5.md).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace PasswordVaultWrapper {
	///<summary>What the one-time migration did, as written to its record.</summary>
	internal class MigrationResult {
		public int CountCopied;
		public int CountAlreadyThere;
		public int CountFailed;
		///<summary>False only on a Windows older than 8, which has no PasswordVault (nothing can ever be read there).</summary>
		public bool IsVaultAvailable;

		public override string ToString() {
			return "Copied "+CountCopied+", already there "+CountAlreadyThere+", failed "+CountFailed
				+(IsVaultAvailable ? "" : ", PasswordVault not available");
		}
	}

	///<summary>A set of names in the Credential Manager: the target prefix of the logins, the old vault's resource prefix that the migration copies
	///from, and the target of the migration's record. The program uses Default; the tests use their own per-run prefix, so they never see or
	///change anything else.</summary>
	internal class CredentialStore {
		///<summary>The program's names. The old vault's prefix is the one Open Dental and this program wrote before S3a-5.</summary>
		public const string TARGET_PREFIX="OpenPmsDental Middle Tier:";
		public const string VAULT_RESOURCE_PREFIX="OpenDental Middle Tier:";
		public const string MIGRATION_RECORD_TARGET="OpenPmsDental Middle Tier migration from PasswordVault";
		///<summary>Between the address and the user name in a target name.</summary>
		private const string SEPARATOR="|";

		public static readonly CredentialStore Default=new CredentialStore(TARGET_PREFIX,VAULT_RESOURCE_PREFIX,MIGRATION_RECORD_TARGET);

		public readonly string TargetPrefix;
		public readonly string VaultResourcePrefix;
		public readonly string MigrationRecordTarget;
		///<summary>Opens the old vault; replaced by the tests to simulate a vault that cannot be opened.</summary>
		internal Func<OldPasswordVault> FuncOpenVault=OldPasswordVault.TryOpen;
		///<summary>True on a Windows older than 8, which has no PasswordVault; replaced by the tests.</summary>
		internal Func<bool> FuncIsOsWithoutVault=OldPasswordVault.IsOsWithoutVault;
		private readonly object _lock=new object();
		private bool _isMigrationChecked;

		public CredentialStore(string targetPrefix,string vaultResourcePrefix,string migrationRecordTarget) {
			if(string.IsNullOrEmpty(targetPrefix) || string.IsNullOrEmpty(vaultResourcePrefix) || string.IsNullOrEmpty(migrationRecordTarget)) {
				throw new ArgumentException("Every name of a credential store is required.");
			}
			TargetPrefix=targetPrefix;
			VaultResourcePrefix=vaultResourcePrefix;
			MigrationRecordTarget=migrationRecordTarget;
		}

		///<summary>The target name of one login.</summary>
		public string GetTargetName(string uri,string userName) {
			return TargetPrefix+uri+SEPARATOR+userName;
		}

		///<summary>Saves or replaces the login of this user for this address. Throws for a blank user name or password, which Windows did not store
		///in the old vault either.</summary>
		public void WritePassword(string uri,string userName,string password) {
			EnsureMigrated();
			if(string.IsNullOrEmpty(userName)) {
				throw new ArgumentException("A blank user name cannot be saved.",nameof(userName));
			}
			if(string.IsNullOrEmpty(password)) {
				throw new ArgumentException("Windows cannot save a blank password.",nameof(password));
			}
			CredentialManager.Write(GetTargetName(uri,userName),userName,password);
		}

		///<summary>The user names saved for this address, oldest saved first.</summary>
		public List<string> GetUserNames(string uri) {
			EnsureMigrated();
			return ListLogins(uri).OrderBy(x => x.LastWrittenUtc).Select(x => x.UserName).ToList();
		}

		///<summary>The first user name saved for this address; false when there is none or the Credential Manager cannot be read.</summary>
		public bool TryRetrieveUserName(string uri,out string userName) {
			userName=string.Empty;
			List<string> listUserNames;
			try {
				listUserNames=GetUserNames(uri);
			}
			catch(Exception) {
				return false;
			}
			if(listUserNames.Count==0) {
				return false;
			}
			userName=listUserNames[0];
			return true;
		}

		///<summary>The saved password of this user for this address. Throws when there is none.</summary>
		public string RetrievePassword(string uri,string userName) {
			EnsureMigrated();
			CredentialEntry credentialEntry=CredentialManager.Read(GetTargetName(uri,userName));
			if(credentialEntry==null || credentialEntry.UserName!=userName) {
				throw new Win32Exception(CredentialManager.ERROR_NOT_FOUND);//"Element not found.", as the old vault said.
			}
			return credentialEntry.Secret;
		}

		///<summary>Deletes every login saved for this address, and the old vault's entries of the address too, so the migration cannot bring a
		///cleared login back.</summary>
		public void ClearCredentials(string uri) {
			EnsureMigrated();
			foreach(CredentialEntry credentialEntry in ListLogins(uri)) {
				CredentialManager.Delete(credentialEntry.TargetName);
			}
			try {
				OldPasswordVault oldPasswordVault=FuncOpenVault();
				if(oldPasswordVault!=null) {
					foreach(OldVaultEntry oldVaultEntry in oldPasswordVault.FindAllByResource(VaultResourcePrefix+uri)) {
						if(oldVaultEntry.Resource==VaultResourcePrefix+uri) {
							oldPasswordVault.Remove(oldVaultEntry);
						}
					}
				}
			}
			catch(Exception) {
				//The old vault is best effort: the login is gone from the Credential Manager, which is the store the program reads.
			}
		}

		///<summary>The logins of exactly this address: the entries under the prefix whose target name is the one their own user name gives.</summary>
		private List<CredentialEntry> ListLogins(string uri) {
			return CredentialManager.List(TargetPrefix)
				.Where(x => x.UserName!="" && x.TargetName==GetTargetName(uri,x.UserName))
				.ToList();
		}

		///<summary>The migration's record ("<UTC time>: <result>"), or null when the migration has not run for this Windows user.</summary>
		public string GetMigrationRecord() {
			CredentialEntry credentialEntry=CredentialManager.Read(MigrationRecordTarget);
			return credentialEntry?.Secret;
		}

		///<summary>Runs the migration once per Windows user: the first call in a process looks for the record, and without one copies the old
		///vault's entries. Errors never reach the caller; without a record the next process tries again.</summary>
		public void EnsureMigrated() {
			lock(_lock) {
				if(_isMigrationChecked) {
					return;
				}
				_isMigrationChecked=true;
				try {
					if(GetMigrationRecord()!=null) {
						return;
					}
#if NETFRAMEWORK
					MigrationResult migrationResult=MigrateFromVault();
					if(migrationResult!=null) {
						CredentialManager.Write(MigrationRecordTarget,"PasswordVault migration",
							DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture)+" UTC: "+migrationResult);
					}
#endif
					//On .NET 5 and later the old vault cannot be read: no record, the bridge release (.NET Framework) moved the logins first.
				}
				catch(Exception) {
					//No record written: the next process tries again.
				}
			}
		}

		///<summary>Copies the old vault's entries under this store's resource prefix into the Credential Manager, never over a login already
		///there. Returns null when the vault could not be opened or listed (no record then: the next process tries again). Only a Windows older
		///than 8, which has no vault, gives a result without reading it. Only the entries under the prefix are read beyond their names.</summary>
		internal MigrationResult MigrateFromVault() {
			MigrationResult migrationResult=new MigrationResult();
			if(FuncIsOsWithoutVault()) {
				return migrationResult;//IsVaultAvailable false: nothing can ever be read here.
			}
			OldPasswordVault oldPasswordVault;
			try {
				oldPasswordVault=FuncOpenVault();
			}
			catch(Exception) {
				return null;
			}
			if(oldPasswordVault==null) {
				//Windows 8 and later always have the vault, so this is an error that may pass (the Credential Manager service not started yet
				//early in a logon, an RPC or COM error): no record, so one unlucky start cannot leave the saved logins behind.
				return null;
			}
			migrationResult.IsVaultAvailable=true;
			List<OldVaultEntry> listOwn;
			try {
				listOwn=oldPasswordVault.RetrieveAll()
					.Where(x => x.Resource.StartsWith(VaultResourcePrefix,StringComparison.Ordinal) && x.Resource.Length>VaultResourcePrefix.Length
						&& x.UserName!="")
					.ToList();
			}
			catch(Exception) {
				return null;
			}
			foreach(OldVaultEntry oldVaultEntry in listOwn) {
				try {
					string uri=oldVaultEntry.Resource.Substring(VaultResourcePrefix.Length);
					string targetName=GetTargetName(uri,oldVaultEntry.UserName);
					if(CredentialManager.Read(targetName)!=null) {
						migrationResult.CountAlreadyThere++;//Saved again since: the newer login stays.
						continue;
					}
					string password=oldVaultEntry.Credential==null ? "" : oldPasswordVault.RetrievePassword(oldVaultEntry);
					if(password=="") {
						migrationResult.CountFailed++;
						continue;
					}
					CredentialManager.Write(targetName,oldVaultEntry.UserName,password);
					migrationResult.CountCopied++;
				}
				catch(Exception) {
					migrationResult.CountFailed++;
				}
			}
			return migrationResult;
		}
	}
}
