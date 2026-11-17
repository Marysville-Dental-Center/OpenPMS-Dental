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
		///<summary>The entries still not copied: those that failed in the latest try, and those not named for a retry (CountNotNamed).</summary>
		public int CountFailed;
		///<summary>False only on a Windows older than 8, which has no PasswordVault (nothing can ever be read there).</summary>
		public bool IsVaultAvailable;
		///<summary>The tries done, the latest one included.</summary>
		public int CountTries=1;
		///<summary>The tries (retry starts) whose vault could not be opened or listed (S3a-FU1). Each counts as a try, so a vault that stays
		///unreadable ends the retries after MAX_TRIES starts.</summary>
		public int CountTriesWithoutVault;
		///<summary>The failed entries past the first MAX_RETRY_IDS, which the retry line does not name (S3a-FU1): they get no retry, and stay
		///counted in CountFailed.</summary>
		public int CountNotNamed;
		///<summary>The entries (GetEntryId) tried again on the next start; empty when the migration is complete.</summary>
		public List<string> ListRetryIds=new List<string>();

		///<summary>The record's text after its time. The first line keeps the S3a-5 wording; the retry line follows only while entries are left to
		///retry.</summary>
		public override string ToString() {
			string withoutVault=(CountTriesWithoutVault>0 ? " ("+CountTriesWithoutVault+" without the vault)" : "");
			string text="Copied "+CountCopied+", already there "+CountAlreadyThere+", failed "+CountFailed
				+(IsVaultAvailable ? "" : ", PasswordVault not available")
				+(CountTries>1 ? ", in "+CountTries+" tries"+withoutVault : "");
			if(ListRetryIds.Count>0) {
				text+="\n"+RETRY_LINE_START+CountTries+" of "+MAX_TRIES+" tries done"+withoutVault+"; on the next start: "+string.Join(",",ListRetryIds)
					+(CountNotNamed>0 ? ", "+CountNotNamed+" not named" : "");
			}
			return text;
		}

		///<summary>The migration that a record leaves to retry, or null when the record is complete: it has no retry line (as every S3a-5 record),
		///or one this release cannot read (never retried, so a damaged record cannot make every start read the vault).</summary>
		public static MigrationResult GetRetry(string record) {
			if(record==null) {
				return null;
			}
			//S3a-5f's retry line, with S3a-FU1's optional " (k without the vault)" after the tries and ", m not named" after the names.
			Match matchRetry=Regex.Match(record,"^"+Regex.Escape(RETRY_LINE_START)
				+@"(\d+) of \d+ tries done(?: \((\d+) without the vault\))?; on the next start: ([0-9a-f]{16}(?:,[0-9a-f]{16})*)(?:, (\d+) not named)?\r?$",
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
				if(matchRetry.Groups[2].Success) {
					migrationResult.CountTriesWithoutVault=int.Parse(matchRetry.Groups[2].Value,CultureInfo.InvariantCulture);
				}
				if(matchRetry.Groups[4].Success) {
					migrationResult.CountNotNamed=int.Parse(matchRetry.Groups[4].Value,CultureInfo.InvariantCulture);
				}
			}
			catch(OverflowException) {
				return null;
			}
			if(migrationResult.CountTries<1 || migrationResult.CountTries>=MAX_TRIES) {
				return null;
			}
			migrationResult.IsVaultAvailable=true;
			migrationResult.ListRetryIds=matchRetry.Groups[3].Value.Split(',').ToList();
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
		///<summary>Whether EnsureMigrated moves the old vault's entries: the .NET Framework build (the bridge release) does; on .NET 5 and later
		///the old vault cannot be read, so the .NET 10 build does not (the bridge moved the logins first). The tests turn it on to run the bridge's
		///migration with their fake vaults on .NET 10 too (S3b-5), and check that it is off there by default.</summary>
		internal bool IsMigrationRun=
#if NETFRAMEWORK
			true;
#else
			false;
#endif
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

		///<summary>The name to save a login under: the one it is already saved under, in that casing, or else the computed one. Windows keeps a
		///credential for each casing written, but reads, lists and deletes by the name without regard to case and shows only one of the casings;
		///a login saved in a second casing would stay hidden behind the first (seen on Windows 11, S3a-5f).</summary>
		private static string GetSavedTargetName(string targetName) {
			CredentialEntry credentialEntry=CredentialManager.ReadInfo(targetName);
			return credentialEntry==null ? targetName : credentialEntry.TargetName;
		}

		///<summary>Deleting the casing Windows shows brings the next hidden one into view, so deletions are repeated up to this many times.
		///</summary>
		private const int MAX_CASE_VARIANTS=20;

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
			CredentialManager.Write(GetSavedTargetName(GetTargetName(uri,userName)),userName,password);
			if(!HasSeparator(uri,userName)) {
				return;
			}
			//Saved again under the escaped name: the entry S3a-5 saved under the unescaped one goes, when it is this login's (its own user name).
			try {
				string targetNameUnescaped=GetTargetNameUnescaped(uri,userName);
				for(int i=0;i<MAX_CASE_VARIANTS;i++) {
					CredentialEntry credentialEntry=CredentialManager.ReadInfo(targetNameUnescaped);
					if(credentialEntry==null || !IsSameName(credentialEntry.UserName,userName)) {
						break;
					}
					CredentialManager.Delete(credentialEntry.TargetName);
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
		///cleared login back. The address's entries are also dropped from a migration record that names them for a retry (S3a-FU1), so a retry
		///cannot copy a login back whose old-vault entry could not be removed.</summary>
		public void ClearCredentials(string uri) {
			EnsureMigrated();
			HashSet<string> setEntryIds=new HashSet<string>();
			//Again until none is left: a deleted login may uncover the same name in another casing (GetSavedTargetName).
			for(int i=0;i<MAX_CASE_VARIANTS;i++) {
				List<CredentialEntry> listLogins=ListLogins(uri);
				if(listLogins.Count==0) {
					break;
				}
				foreach(CredentialEntry credentialEntry in listLogins) {
					//The old-vault entry of this login, as the migration names it, also when the vault cannot be opened now.
					setEntryIds.Add(MigrationResult.GetEntryId(VaultResourcePrefix+uri,credentialEntry.UserName));
					CredentialManager.Delete(credentialEntry.TargetName);
				}
			}
			try {
				IOldVault oldVault=FuncOpenVault();
				if(oldVault!=null) {
					List<OldVaultEntry> listOldVaultEntries=oldVault.FindAllByResource(VaultResourcePrefix+uri)
						.Where(x => IsSameName(x.Resource,VaultResourcePrefix+uri))
						.ToList();
					//Named before any removal, so an entry whose removal fails cannot come back through a retry.
					foreach(OldVaultEntry oldVaultEntry in listOldVaultEntries) {
						setEntryIds.Add(MigrationResult.GetEntryId(oldVaultEntry.Resource,oldVaultEntry.UserName));
					}
					foreach(OldVaultEntry oldVaultEntry in listOldVaultEntries) {
						try {
							oldVault.Remove(oldVaultEntry);
						}
						catch(Exception) {
							//The next entry is still removed; this one is no longer named for a retry.
						}
					}
				}
			}
			catch(Exception) {
				//The old vault is best effort: the login is gone from the Credential Manager, which is the store the program reads.
			}
			DropRetryIds(setEntryIds);
		}

		///<summary>Removes these entries (MigrationResult.GetEntryId) from the migration record's retry line, when it names any (S3a-FU1). They no
		///longer count as failed; a record left with none to retry is complete. The record keeps its time. Best effort: an error leaves the
		///record as it was.</summary>
		private void DropRetryIds(HashSet<string> setEntryIds) {
			if(setEntryIds.Count==0) {
				return;
			}
			try {
				string record=GetMigrationRecord();
				MigrationResult migrationResult=MigrationResult.GetRetry(record);
				if(migrationResult==null) {
					return;//No record, or a complete one: nothing is retried.
				}
				int countDropped=migrationResult.ListRetryIds.RemoveAll(x => setEntryIds.Contains(x));
				if(countDropped==0) {
					return;
				}
				migrationResult.CountFailed=Math.Max(migrationResult.CountNotNamed,migrationResult.CountFailed-countDropped);
				int indexTimeEnd=record.IndexOf(RECORD_TIME_END,StringComparison.Ordinal);
				string time=(indexTimeEnd<0) ? GetRecordTime() : record.Substring(0,indexTimeEnd);
				WriteMigrationRecord(time,migrationResult);
			}
			catch(Exception) {
				//The record stays as it was.
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

		///<summary>Between the record's time and its result.</summary>
		private const string RECORD_TIME_END=" UTC: ";

		///<summary>Now, as the record writes its time (UTC).</summary>
		private static string GetRecordTime() {
			return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss",CultureInfo.InvariantCulture);
		}

		///<summary>Writes the migration's record: "<time> UTC: <result>".</summary>
		private void WriteMigrationRecord(string time,MigrationResult migrationResult) {
			CredentialManager.Write(MigrationRecordTarget,"PasswordVault migration",time+RECORD_TIME_END+migrationResult);
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
					//On .NET 5 and later the old vault cannot be read: no record, the bridge release (.NET Framework) moved the logins first
					//(IsMigrationRun).
					if(IsMigrationRun) {
						MigrationResult migrationResult=MigrateFromVault(migrationResultRetry);
						if(migrationResult!=null) {
							WriteMigrationRecord(GetRecordTime(),migrationResult);
						}
					}
				}
				catch(Exception) {
					//No record written, or the previous one kept: the next process tries again.
				}
			}
		}

		///<summary>Copies the old vault's entries under this store's resource prefix into the Credential Manager, never over a login already
		///there. With a previous try that left entries to retry, only those are tried again, and its counts carry on. Returns null when the vault
		///could not be opened or listed on the first try (no record then: the next process tries again); on a retry that start counts as a try
		///without the vault (S3a-FU1), so an unreadable vault ends the retries after MAX_TRIES starts. Only a Windows older than 8, which has no
		///vault, gives a result without reading it. Only the entries under the prefix are read beyond their names.</summary>
		internal MigrationResult MigrateFromVault(MigrationResult migrationResultRetry=null) {
			MigrationResult migrationResult=new MigrationResult();
			if(migrationResultRetry!=null) {
				migrationResult.CountCopied=migrationResultRetry.CountCopied;
				migrationResult.CountAlreadyThere=migrationResultRetry.CountAlreadyThere;
				migrationResult.CountTries=migrationResultRetry.CountTries+1;
				migrationResult.CountTriesWithoutVault=migrationResultRetry.CountTriesWithoutVault;
				migrationResult.CountNotNamed=migrationResultRetry.CountNotNamed;
			}
			if(FuncIsOsWithoutVault()) {
				return migrationResult;//IsVaultAvailable false: nothing can ever be read here.
			}
			IOldVault oldVault;
			try {
				oldVault=FuncOpenVault();
			}
			catch(Exception) {
				return GetResultWithoutVault(migrationResult,migrationResultRetry);
			}
			if(oldVault==null) {
				//Windows 8 and later always have the vault, so this is an error that may pass (the Credential Manager service not started yet
				//early in a logon, an RPC or COM error): no record, so one unlucky start cannot leave the saved logins behind.
				return GetResultWithoutVault(migrationResult,migrationResultRetry);
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
				return GetResultWithoutVault(migrationResult,migrationResultRetry);
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
			//The entries an earlier try left unnamed are still not copied, and still get no retry.
			migrationResult.CountFailed+=migrationResult.CountNotNamed;
			if(migrationResult.CountTries<MigrationResult.MAX_TRIES) {
				List<string> listFailedIdsDistinct=listFailedIds.Distinct().ToList();
				migrationResult.ListRetryIds=listFailedIdsDistinct.Take(MigrationResult.MAX_RETRY_IDS).ToList();
				migrationResult.CountNotNamed+=listFailedIdsDistinct.Count-migrationResult.ListRetryIds.Count;
			}
			return migrationResult;
		}

		///<summary>The result of a start whose vault could not be opened or listed: null on the first try (no record, the next process tries
		///again, section 2 of the port note); on a retry (S3a-FU1) the previous record carried on with this start counted as a try without the
		///vault, the same entries named for the next start, or none after the last try.</summary>
		private static MigrationResult GetResultWithoutVault(MigrationResult migrationResult,MigrationResult migrationResultRetry) {
			if(migrationResultRetry==null) {
				return null;
			}
			migrationResult.IsVaultAvailable=true;
			migrationResult.CountFailed=migrationResultRetry.CountFailed;
			migrationResult.CountTriesWithoutVault++;
			if(migrationResult.CountTries<MigrationResult.MAX_TRIES) {
				migrationResult.ListRetryIds=migrationResultRetry.ListRetryIds.ToList();
			}
			return migrationResult;
		}
	}
}
