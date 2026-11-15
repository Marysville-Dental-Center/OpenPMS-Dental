// The saved Middle Tier logins in the Windows Credential Manager, and the one-time move of the old PasswordVault entries (package S3a-5 of
// docs/specs/net10-plan.md; docs/port-notes/S3a-5.md, its follow-up S3a-5f in section 7).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PasswordVaultWrapper {
	///<summary>What the one-time migration did, as written to its record. A record with a retry line (S3a-5f) is not complete yet: the entries
	///it names failed to copy and are tried again on a later start, at most MAX_TRIES starts in all.</summary>
	internal class MigrationResult {
		///<summary>The starts that copy, the first one included. After the last one the record is complete, whatever failed.</summary>
		public const int MAX_TRIES=5;
		///<summary>At most this many failed entries are named for a retry, so the record stays within the Credential Manager's 1280 characters.
		///</summary>
		public const int MAX_RETRY_IDS=50;
		private const string RETRY_LINE_START="Retry: ";

		public int CountCopied;
		public int CountAlreadyThere;
		///<summary>The entries that failed in the latest try (still not copied).</summary>
		public int CountFailed;
		///<summary>False only on a Windows older than 8, which has no PasswordVault (nothing can ever be read there).</summary>
		public bool IsVaultAvailable;
		///<summary>The tries done, the latest one included.</summary>
		public int CountTries=1;
		///<summary>The entries (GetEntryId) tried again on the next start; empty when the migration is complete.</summary>
		public List<string> ListRetryIds=new List<string>();

		///<summary>The record's text after its time. The first line keeps the S3a-5 wording; the retry line follows only while entries are left to
		///retry.</summary>
		public override string ToString() {
			string text="Copied "+CountCopied+", already there "+CountAlreadyThere+", failed "+CountFailed
				+(IsVaultAvailable ? "" : ", PasswordVault not available")
				+(CountTries>1 ? ", in "+CountTries+" tries" : "");
			if(ListRetryIds.Count>0) {
				text+="\n"+RETRY_LINE_START+CountTries+" of "+MAX_TRIES+" tries done; on the next start: "+string.Join(",",ListRetryIds);
			}
			return text;
		}

		///<summary>The migration that a record leaves to retry, or null when the record is complete: it has no retry line (as every S3a-5 record),
		///or one this release cannot read (never retried, so a damaged record cannot make every start read the vault).</summary>
		public static MigrationResult GetRetry(string record) {
			if(record==null) {
				return null;
			}
			Match matchRetry=Regex.Match(record,"^"+Regex.Escape(RETRY_LINE_START)+@"(\d+) of \d+ tries done; on the next start: ([0-9a-f]{16}(,[0-9a-f]{16})*)\r?$",
				RegexOptions.Multiline|RegexOptions.CultureInvariant);
			Match matchCounts=Regex.Match(record,@"Copied (\d+), already there (\d+), failed (\d+)",RegexOptions.CultureInvariant);
			if(!matchRetry.Success || !matchCounts.Success) {
				return null;
			}
			MigrationResult migrationResult=new MigrationResult();
			try {
				migrationResult.CountTries=int.Parse(matchRetry.Groups[1].Value,CultureInfo.InvariantCulture);
				migrationResult.CountCopied=int.Parse(matchCounts.Groups[1].Value,CultureInfo.InvariantCulture);
				migrationResult.CountAlreadyThere=int.Parse(matchCounts.Groups[2].Value,CultureInfo.InvariantCulture);
				migrationResult.CountFailed=int.Parse(matchCounts.Groups[3].Value,CultureInfo.InvariantCulture);
			}
			catch(OverflowException) {
				return null;
			}
			if(migrationResult.CountTries<1 || migrationResult.CountTries>=MAX_TRIES) {
				return null;
			}
			migrationResult.IsVaultAvailable=true;
			migrationResult.ListRetryIds=matchRetry.Groups[2].Value.Split(',').ToList();
			return migrationResult;
		}

		///<summary>The record's name for one old-vault entry: the first 16 hexadecimal digits of the SHA-256 of its resource and user name. It names
		///the entry without its address or user in clear, and never involves the password.</summary>
		public static string GetEntryId(string resource,string userName) {
			using(SHA256 sha256=SHA256.Create()) {
				byte[] arrayHash=sha256.ComputeHash(Encoding.UTF8.GetBytes(resource+"\n"+userName));
				return string.Concat(arrayHash.Take(8).Select(x => x.ToString("x2",CultureInfo.InvariantCulture)));
			}
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
		///<summary>Between the address and the user name in a target name, when neither contains it.</summary>
		private const string SEPARATOR="|";
		///<summary>Between the escaped address and the escaped user name, when either contains SEPARATOR (S3a-5f). Escape never writes it: each
		///'%' it writes is followed by "25" or "7C".</summary>
		private const string SEPARATOR_ESCAPED="%_";

		public static readonly CredentialStore Default=new CredentialStore(TARGET_PREFIX,VAULT_RESOURCE_PREFIX,MIGRATION_RECORD_TARGET);

		public readonly string TargetPrefix;
		public readonly string VaultResourcePrefix;
		public readonly string MigrationRecordTarget;
		///<summary>Opens the old vault; replaced by the tests to simulate a vault that cannot be opened or that fails single entries.</summary>
		internal Func<IOldVault> FuncOpenVault=OldPasswordVault.TryOpen;
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

		///<summary>The target name of one login. An address and user name without '|' give "prefix+uri|user", byte for byte the name S3a-5 wrote,
		///so the saved logins are found as before. With a '|' in either, both are escaped ('%' as %25, '|' as %7C) and joined by "%_": that name
		///has no '|' after the prefix, so it never equals a plain one, and its first "%_" is the separator, so no two logins share it.</summary>
		public string GetTargetName(string uri,string userName) {
			if(!HasSeparator(uri,userName)) {
				return TargetPrefix+uri+SEPARATOR+userName;
			}
			return TargetPrefix+Escape(uri)+SEPARATOR_ESCAPED+Escape(userName);
		}

		///<summary>The name S3a-5 gave a login whose address or user name contains '|' (the read fallback of such a login). It is ambiguous by
		///itself; the entry's own user name tells which login it is.</summary>
		internal string GetTargetNameUnescaped(string uri,string userName) {
			return TargetPrefix+uri+SEPARATOR+userName;
		}

		private static bool HasSeparator(string uri,string userName) {
			return (uri??"").Contains(SEPARATOR) || (userName??"").Contains(SEPARATOR);
		}

		private static string Escape(string text) {
			return (text??"").Replace("%","%25").Replace(SEPARATOR,"%7C");
		}

		///<summary>Windows compares target names, so also the user names in them, without regard to case: so does the store.</summary>
		private static bool IsSameName(string name1,string name2) {
			return string.Equals(name1,name2,StringComparison.OrdinalIgnoreCase);
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
			if(!HasSeparator(uri,userName)) {
				return;
			}
			//Saved again under the escaped name: the entry S3a-5 saved under the unescaped one goes, when it is this login's (its own user name).
			try {
				string targetNameUnescaped=GetTargetNameUnescaped(uri,userName);
				CredentialEntry credentialEntry=CredentialManager.ReadInfo(targetNameUnescaped);
				if(credentialEntry!=null && IsSameName(credentialEntry.UserName,userName)) {
					CredentialManager.Delete(targetNameUnescaped);
				}
			}
			catch(Exception) {
				//The new login is saved; an old entry left behind is found as the same user and cleared with the address.
			}
		}

		///<summary>The user names saved for this address, oldest saved first, each once.</summary>
		public List<string> GetUserNames(string uri) {
			EnsureMigrated();
			return ListLogins(uri).OrderBy(x => x.LastWrittenUtc).Select(x => x.UserName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
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
			if(credentialEntry!=null && IsSameName(credentialEntry.UserName,userName)) {
				return credentialEntry.Secret;
			}
			if(HasSeparator(uri,userName)) {
				//Read fallback: saved by S3a-5 under the unescaped name. Its own user name must be this one before its secret is read.
				string targetNameUnescaped=GetTargetNameUnescaped(uri,userName);
				CredentialEntry credentialEntryInfo=CredentialManager.ReadInfo(targetNameUnescaped);
				if(credentialEntryInfo!=null && IsSameName(credentialEntryInfo.UserName,userName)) {
					credentialEntry=CredentialManager.Read(targetNameUnescaped);
					if(credentialEntry!=null && IsSameName(credentialEntry.UserName,userName)) {
						return credentialEntry.Secret;
					}
				}
			}
			throw new Win32Exception(CredentialManager.ERROR_NOT_FOUND);//"Element not found.", as the old vault said.
		}

		///<summary>Deletes every login saved for this address, and the old vault's entries of the address too, so the migration cannot bring a
		///cleared login back.</summary>
		public void ClearCredentials(string uri) {
			EnsureMigrated();
			foreach(CredentialEntry credentialEntry in ListLogins(uri)) {
				CredentialManager.Delete(credentialEntry.TargetName);
			}
			try {
				IOldVault oldVault=FuncOpenVault();
				if(oldVault!=null) {
					foreach(OldVaultEntry oldVaultEntry in oldVault.FindAllByResource(VaultResourcePrefix+uri)) {
						if(IsSameName(oldVaultEntry.Resource,VaultResourcePrefix+uri)) {
							oldVault.Remove(oldVaultEntry);
						}
					}
				}
			}
			catch(Exception) {
				//The old vault is best effort: the login is gone from the Credential Manager, which is the store the program reads.
			}
		}

		///<summary>The logins of exactly this address: the entries under the prefix whose target name is the one their own user name gives, or,
		///for an address or user name with '|', the unescaped name S3a-5 gave it.</summary>
		private List<CredentialEntry> ListLogins(string uri) {
			return CredentialManager.List(TargetPrefix)
				.Where(x => x.UserName!="" && IsLoginOf(x,uri))
				.ToList();
		}

		private bool IsLoginOf(CredentialEntry credentialEntry,string uri) {
			if(IsSameName(credentialEntry.TargetName,GetTargetName(uri,credentialEntry.UserName))) {
				return true;
			}
			return HasSeparator(uri,credentialEntry.UserName) && IsSameName(credentialEntry.TargetName,GetTargetNameUnescaped(uri,credentialEntry.UserName));
		}

		///<summary>True when a login of this user for this address is saved, under either of its names, checked without reading its secret.
		///</summary>
		private bool LoginExists(string uri,string userName) {
			if(CredentialManager.ReadInfo(GetTargetName(uri,userName))!=null) {
				return true;//Whatever is saved under the name is never replaced by the migration.
			}
			if(!HasSeparator(uri,userName)) {
				return false;
			}
			CredentialEntry credentialEntry=CredentialManager.ReadInfo(GetTargetNameUnescaped(uri,userName));
			return credentialEntry!=null && IsSameName(credentialEntry.UserName,userName);
		}

		///<summary>The migration's record ("<UTC time>: <result>"), or null when the migration has not run for this Windows user.</summary>
		public string GetMigrationRecord() {
			CredentialEntry credentialEntry=CredentialManager.Read(MigrationRecordTarget);
			return credentialEntry?.Secret;
		}

		///<summary>Runs the migration once per Windows user: the first call in a process looks for the record, and without one copies the old
		///vault's entries. A record that leaves entries to retry has them tried again (S3a-5f). Errors never reach the caller; without a record
		///the next process tries again.</summary>
		public void EnsureMigrated() {
			lock(_lock) {
				if(_isMigrationChecked) {
					return;
				}
				_isMigrationChecked=true;
				try {
					string record=GetMigrationRecord();
					MigrationResult migrationResultRetry=null;
					if(record!=null) {
						migrationResultRetry=MigrationResult.GetRetry(record);
						if(migrationResultRetry==null) {
							return;//Complete.
						}
					}
#if NETFRAMEWORK
					MigrationResult migrationResult=MigrateFromVault(migrationResultRetry);
					if(migrationResult!=null) {
						CredentialManager.Write(MigrationRecordTarget,"PasswordVault migration",
							DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture)+" UTC: "+migrationResult);
					}
#endif
					//On .NET 5 and later the old vault cannot be read: no record, the bridge release (.NET Framework) moved the logins first.
				}
				catch(Exception) {
					//No record written, or the previous one kept: the next process tries again.
				}
			}
		}

		///<summary>Copies the old vault's entries under this store's resource prefix into the Credential Manager, never over a login already
		///there. With a previous try that left entries to retry, only those are tried again, and its counts carry on. Returns null when the vault
		///could not be opened or listed (no record then, or the previous one kept: the next process tries again). Only a Windows older than 8,
		///which has no vault, gives a result without reading it. Only the entries under the prefix are read beyond their names.</summary>
		internal MigrationResult MigrateFromVault(MigrationResult migrationResultRetry=null) {
			MigrationResult migrationResult=new MigrationResult();
			if(migrationResultRetry!=null) {
				migrationResult.CountCopied=migrationResultRetry.CountCopied;
				migrationResult.CountAlreadyThere=migrationResultRetry.CountAlreadyThere;
				migrationResult.CountTries=migrationResultRetry.CountTries+1;
			}
			if(FuncIsOsWithoutVault()) {
				return migrationResult;//IsVaultAvailable false: nothing can ever be read here.
			}
			IOldVault oldVault;
			try {
				oldVault=FuncOpenVault();
			}
			catch(Exception) {
				return null;
			}
			if(oldVault==null) {
				//Windows 8 and later always have the vault, so this is an error that may pass (the Credential Manager service not started yet
				//early in a logon, an RPC or COM error): no record, so one unlucky start cannot leave the saved logins behind.
				return null;
			}
			migrationResult.IsVaultAvailable=true;
			List<OldVaultEntry> listOwn;
			try {
				listOwn=oldVault.RetrieveAll()
					.Where(x => x.Resource.StartsWith(VaultResourcePrefix,StringComparison.Ordinal) && x.Resource.Length>VaultResourcePrefix.Length
						&& x.UserName!="")
					.ToList();
			}
			catch(Exception) {
				return null;
			}
			if(migrationResultRetry!=null) {
				listOwn=listOwn.Where(x => migrationResultRetry.ListRetryIds.Contains(MigrationResult.GetEntryId(x.Resource,x.UserName))).ToList();
			}
			List<string> listFailedIds=new List<string>();
			foreach(OldVaultEntry oldVaultEntry in listOwn) {
				try {
					string uri=oldVaultEntry.Resource.Substring(VaultResourcePrefix.Length);
					if(LoginExists(uri,oldVaultEntry.UserName)) {
						migrationResult.CountAlreadyThere++;//Saved again since: the newer login stays.
						continue;
					}
					string password=oldVaultEntry.Credential==null ? "" : oldVault.RetrievePassword(oldVaultEntry);
					if(password=="") {
						migrationResult.CountFailed++;
						listFailedIds.Add(MigrationResult.GetEntryId(oldVaultEntry.Resource,oldVaultEntry.UserName));
						continue;
					}
					CredentialManager.Write(GetTargetName(uri,oldVaultEntry.UserName),oldVaultEntry.UserName,password);
					migrationResult.CountCopied++;
				}
				catch(Exception) {
					migrationResult.CountFailed++;
					listFailedIds.Add(MigrationResult.GetEntryId(oldVaultEntry.Resource,oldVaultEntry.UserName));
				}
			}
			if(migrationResult.CountTries<MigrationResult.MAX_TRIES) {
				migrationResult.ListRetryIds=listFailedIds.Distinct().Take(MigrationResult.MAX_RETRY_IDS).ToList();
			}
			return migrationResult;
		}
	}
}
