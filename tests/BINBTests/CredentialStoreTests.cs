// Package S3a-5 (docs/specs/net10-plan.md, docs/port-notes/S3a-5.md): the saved Middle Tier logins in the Windows Credential Manager, and the
// one-time move of the old PasswordVault entries.
// SAFETY: these tests run on the developer's own Windows account. Every name they use starts with a per-run prefix
// "OpenPmsDentalTest:<guid>:" (Credential Manager targets, old-vault resources, the migration record). They create, read and delete only
// entries under that prefix, every listing is filtered by it before an entry is touched, the public static methods (which would run the
// program's real migration) are never called, and everything is removed in finally. All user names and passwords are synthetic.
// The S3a-5f tests (case, '|' in names, the migration's retry) use FakeOldVault, an old vault in memory, so they never touch the real vault.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PasswordVaultWrapper;

namespace BINBTests {
	///<summary>A credential store of one test, with names under its own prefix, and the clean-up of everything under that prefix. A test runs
	///its body through Run, which cleans up whatever the body did.</summary>
	internal sealed class TestCredentialStore {
		///<summary>Rounds of listing and deleting in the clean-up: Windows shows one casing of a name at a time.</summary>
		private const int MAX_CLEANUP_ROUNDS=20;
		public readonly string Prefix="OpenPmsDentalTest:"+Guid.NewGuid().ToString("N")+":";
		public readonly CredentialStore Store;
		///<summary>The old-vault resources this test wrote; only these are looked up and removed.</summary>
		private readonly List<string> _listVaultResources=new List<string>();
		///<summary>Clean-up problems a test of the clean-up itself adds (AddCleanUpProblem); no other test adds any.</summary>
		private readonly List<string> _listProblemsAdded=new List<string>();

		private TestCredentialStore() {
			Store=NewStore();
		}

		///<summary>Runs the test's body with a new store and then always cleans up first, and asserts after (S3a-FU1): a clean-up problem is
		///written to the test's output, and fails the test only when the body succeeded, so it never replaces the exception the body threw.
		///</summary>
		public static void Run(TestContext testContext,Action<TestCredentialStore> actionBody) {
			TestCredentialStore testCredentialStore=new TestCredentialStore();
			bool isBodyDone=false;
			try {
				actionBody(testCredentialStore);
				isBodyDone=true;
			}
			finally {
				List<string> listProblems=testCredentialStore.CleanUp();
				if(listProblems.Count>0) {
					testContext?.WriteLine("Clean-up of "+testCredentialStore.Prefix+"*: "+string.Join("; ",listProblems));
				}
				if(isBodyDone) {
					Assert.AreEqual(0,listProblems.Count,"Clean-up: "+string.Join("; ",listProblems));
				}
			}
		}

		///<summary>Only for the test of Run itself: a clean-up problem, as if a deletion had failed.</summary>
		public void AddCleanUpProblem(string problem) {
			_listProblemsAdded.Add(problem);
		}

		///<summary>Another store with the same names, as a later process of the program would have. Its old vault is the real one seen
		///through ScopedOldVault: the migration's RetrieveAll returns only this test's own resources, so no test lists the names of the other
		///entries of the Windows account's vault (owner rule, lead fix at S3a-5f's merge).</summary>
		public CredentialStore NewStore() {
			CredentialStore credentialStore=new CredentialStore(Prefix+"cm:",Prefix+"vault:",Prefix+"record");
			credentialStore.FuncOpenVault=() => {
				OldPasswordVault oldPasswordVault=OldPasswordVault.TryOpen();
				return oldPasswordVault==null ? null : new ScopedOldVault(oldPasswordVault,this);
			};
			return credentialStore;
		}

		///<summary>The old-vault resources this test wrote so far.</summary>
		public List<string> GetVaultResources() {
			return _listVaultResources.Distinct().ToList();
		}

		public string VaultResource(string uri) {
			return Prefix+"vault:"+uri;
		}

		///<summary>Writes an entry to the old vault under this test's prefix, as the program before S3a-5 did under its own name.</summary>
		public void AddToVault(OldPasswordVault oldPasswordVault,string resource,string userName,string password) {
			Assert.IsTrue(resource.StartsWith(Prefix,StringComparison.Ordinal),"A test writes only under its own prefix.");
			_listVaultResources.Add(resource);
			oldPasswordVault.Add(resource,userName,password);
		}

		///<summary>Removes everything of this test, each entry on its own (one that fails does not keep the others), and returns the problems
		///(S3a-5f); Run asserts on them.</summary>
		private List<string> CleanUp() {
			List<string> listProblems=new List<string>(_listProblemsAdded);
			//CredentialManager.List filters by the prefix (Windows' filter, then StartsWith again), so only this test's entries are deleted. Windows
			//shows one casing of a name at a time and deleting it uncovers the next, so the listing is repeated until it is empty: up to
			//MAX_CLEANUP_ROUNDS rounds that delete, and one more listing to see that nothing is left.
			for(int round=0;round<=MAX_CLEANUP_ROUNDS;round++) {
				List<CredentialEntry> listEntries;
				try {
					listEntries=CredentialManager.List(Prefix);
				}
				catch(Exception ex) {
					listProblems.Add("Listing "+Prefix+"*: "+ex.Message);
					break;
				}
				listEntries=listEntries.Where(x => x.TargetName.StartsWith(Prefix,StringComparison.OrdinalIgnoreCase)).ToList();
				if(listEntries.Count==0) {
					break;
				}
				if(round==MAX_CLEANUP_ROUNDS) {
					listProblems.Add("Still there after "+MAX_CLEANUP_ROUNDS+" rounds: "+string.Join(", ",listEntries.Select(x => x.TargetName)));
					break;
				}
				foreach(CredentialEntry credentialEntry in listEntries) {
					try {
						CredentialManager.Delete(credentialEntry.TargetName);
					}
					catch(Exception ex) {
						listProblems.Add("Deleting "+credentialEntry.TargetName+": "+ex.Message);
					}
				}
			}
			if(_listVaultResources.Count>0) {
				OldPasswordVault oldPasswordVault=null;
				try {
					oldPasswordVault=OldPasswordVault.TryOpen();
				}
				catch(Exception ex) {
					listProblems.Add("Opening the vault: "+ex.Message);
				}
				if(oldPasswordVault==null) {
					listProblems.Add("The vault could not be opened to remove: "+string.Join(", ",_listVaultResources.Distinct()));
				}
				else {
					foreach(string resource in _listVaultResources.Distinct()) {
						try {
							foreach(OldVaultEntry oldVaultEntry in oldPasswordVault.FindAllByResource(resource)) {
								if(oldVaultEntry.Resource.StartsWith(Prefix,StringComparison.Ordinal)) {
									oldPasswordVault.Remove(oldVaultEntry);
								}
							}
						}
						catch(Exception ex) {
							listProblems.Add("Removing the vault resource "+resource+": "+ex.Message);
						}
					}
				}
			}
			return listProblems;
		}
	}

	///<summary>The real old vault, limited to one test's own resources: RetrieveAll asks the vault by resource name for each resource the test
	///wrote (FindAllByResource) instead of listing every entry of the account. Lookups of another resource name return nothing, and a
	///password is only read for an entry of the test's own resources.</summary>
	internal sealed class ScopedOldVault : IOldVault {
		private readonly OldPasswordVault _oldPasswordVault;
		private readonly TestCredentialStore _testCredentialStore;

		public ScopedOldVault(OldPasswordVault oldPasswordVault,TestCredentialStore testCredentialStore) {
			_oldPasswordVault=oldPasswordVault;
			_testCredentialStore=testCredentialStore;
		}

		private bool IsOwn(string resource) {
			return resource!=null && resource.StartsWith(_testCredentialStore.Prefix,StringComparison.OrdinalIgnoreCase);
		}

		public List<OldVaultEntry> RetrieveAll() {
			List<OldVaultEntry> listEntries=new List<OldVaultEntry>();
			foreach(string resource in _testCredentialStore.GetVaultResources()) {
				listEntries.AddRange(_oldPasswordVault.FindAllByResource(resource).Where(x => IsOwn(x.Resource)));
			}
			return listEntries;
		}

		public List<OldVaultEntry> FindAllByResource(string resource) {
			if(!IsOwn(resource)) {
				return new List<OldVaultEntry>();
			}
			return _oldPasswordVault.FindAllByResource(resource).Where(x => IsOwn(x.Resource)).ToList();
		}

		public string RetrievePassword(OldVaultEntry oldVaultEntry) {
			Assert.IsTrue(IsOwn(oldVaultEntry.Resource),"A test reads only its own vault entries.");
			return _oldPasswordVault.RetrievePassword(oldVaultEntry);
		}

		public void Remove(OldVaultEntry oldVaultEntry) {
			Assert.IsTrue(IsOwn(oldVaultEntry.Resource),"A test removes only its own vault entries.");
			_oldPasswordVault.Remove(oldVaultEntry);
		}
	}

	///<summary>An old vault in memory (S3a-5f): entries the test adds, single entries whose password cannot be read, and what was asked of it.
	///It never touches the real vault.</summary>
	internal sealed class FakeOldVault : IOldVault {
		private readonly List<OldVaultEntry> _listEntries=new List<OldVaultEntry>();
		private readonly Dictionary<OldVaultEntry,string> _dictPasswords=new Dictionary<OldVaultEntry,string>();
		///<summary>Resources whose password read throws, as a vault entry that cannot be read.</summary>
		public readonly HashSet<string> SetResourcesFailing=new HashSet<string>();
		///<summary>The resources whose password was read, in order.</summary>
		public readonly List<string> ListResourcesRead=new List<string>();
		public int CountRetrieveAll;
		///<summary>RetrieveAll throws, as a vault that opens but cannot be listed (S3a-FU1).</summary>
		public bool IsRetrieveAllFailing;
		///<summary>Remove throws, as an entry the vault refuses to remove (S3a-FU1).</summary>
		public bool IsRemoveFailing;

		public void Add(string resource,string userName,string password) {
			OldVaultEntry oldVaultEntry=new OldVaultEntry { Resource=resource,UserName=userName,Credential=new object() };
			_listEntries.Add(oldVaultEntry);
			_dictPasswords[oldVaultEntry]=password;
		}

		public List<OldVaultEntry> RetrieveAll() {
			CountRetrieveAll++;
			if(IsRetrieveAllFailing) {
				throw new System.Runtime.InteropServices.COMException("Synthetic: the vault cannot be listed.",unchecked((int)0x80004005));
			}
			return _listEntries.ToList();
		}

		public List<OldVaultEntry> FindAllByResource(string resource) {
			return _listEntries.Where(x => string.Equals(x.Resource,resource,StringComparison.OrdinalIgnoreCase)).ToList();
		}

		public string RetrievePassword(OldVaultEntry oldVaultEntry) {
			ListResourcesRead.Add(oldVaultEntry.Resource);
			if(SetResourcesFailing.Contains(oldVaultEntry.Resource)) {
				throw new System.Runtime.InteropServices.COMException("Synthetic: the entry cannot be read.",unchecked((int)0x80004005));
			}
			return _dictPasswords[oldVaultEntry];
		}

		public void Remove(OldVaultEntry oldVaultEntry) {
			if(IsRemoveFailing) {
				throw new System.Runtime.InteropServices.COMException("Synthetic: the entry cannot be removed.",unchecked((int)0x80004005));
			}
			_listEntries.Remove(oldVaultEntry);
			_dictPasswords.Remove(oldVaultEntry);
		}
	}

	[TestClass]
	public class CredentialStoreTests {
		///<summary>The test's output, where TestCredentialStore.Run writes clean-up problems.</summary>
		public TestContext TestContext { get; set; }
		private const string URI="https://mt.example.test/OpenPmsDentalServer/ServiceMain.asmx";
		private const string USER="synthetic-user";
		private const string PASSWORD="synthetic-password-1";

		///<summary>The old vault, or Inconclusive where this Windows has none (the migration then only records that there was nothing to read).
		///</summary>
		private static OldPasswordVault OpenVaultOrInconclusive() {
			OldPasswordVault oldPasswordVault=OldPasswordVault.TryOpen();
			if(oldPasswordVault==null) {
				Assert.Inconclusive("The Windows Runtime PasswordVault cannot be opened on this computer.");
			}
			return oldPasswordVault;
		}

		[TestMethod]
		public void Login_IsWrittenReadReplacedAndCleared() {
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				CredentialStore credentialStore=testCredentialStore.Store;
				Assert.IsFalse(credentialStore.TryRetrieveUserName(URI,out string userNameNone));
				Assert.AreEqual("",userNameNone);
				credentialStore.WritePassword(URI,USER,PASSWORD);
				Assert.IsTrue(credentialStore.TryRetrieveUserName(URI,out string userName));
				Assert.AreEqual(USER,userName);
				Assert.AreEqual(PASSWORD,credentialStore.RetrievePassword(URI,USER));
				//Saved again with another password: replaced, still one login.
				credentialStore.WritePassword(URI,USER,"synthetic-password-2");
				Assert.AreEqual("synthetic-password-2",credentialStore.RetrievePassword(URI,USER));
				CollectionAssert.AreEqual(new[] { USER },credentialStore.GetUserNames(URI));
				//A second user for the same address, as PasswordVault allowed.
				credentialStore.WritePassword(URI,"synthetic-user-2","synthetic-password-3");
				CollectionAssert.AreEquivalent(new[] { USER,"synthetic-user-2" },credentialStore.GetUserNames(URI));
				Assert.AreEqual("synthetic-password-3",credentialStore.RetrievePassword(URI,"synthetic-user-2"));
				//Where it is stored: a generic credential named after the address and the user, with the user's name on it.
				CredentialEntry credentialEntry=CredentialManager.Read(testCredentialStore.Prefix+"cm:"+URI+"|"+USER);
				Assert.IsNotNull(credentialEntry);
				Assert.AreEqual(USER,credentialEntry.UserName);
				credentialStore.ClearCredentials(URI);
				Assert.IsFalse(credentialStore.TryRetrieveUserName(URI,out string _));
				Win32Exception win32Exception=Assert.ThrowsExactly<Win32Exception>(() => credentialStore.RetrievePassword(URI,USER));
				Assert.AreEqual(CredentialManager.ERROR_NOT_FOUND,win32Exception.NativeErrorCode);
				//Nothing to clear is no error any more.
				credentialStore.ClearCredentials(URI);
			});
		}

		[TestMethod]
		public void Login_IsFoundForItsOwnAddressOnly() {
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				CredentialStore credentialStore=testCredentialStore.Store;
				string uriLonger="https://mt.example.test:8443/ServiceMain.asmx";
				string uriShort="https://mt.example.test";
				credentialStore.WritePassword(uriLonger,"synthetic-user-long","synthetic-password-long");
				Assert.IsFalse(credentialStore.TryRetrieveUserName(uriShort,out string _),"An address that starts another one does not find its login.");
				credentialStore.WritePassword(uriShort,USER,PASSWORD);
				CollectionAssert.AreEqual(new[] { USER },credentialStore.GetUserNames(uriShort));
				CollectionAssert.AreEqual(new[] { "synthetic-user-long" },credentialStore.GetUserNames(uriLonger));
				credentialStore.ClearCredentials(uriShort);
				Assert.AreEqual("synthetic-password-long",credentialStore.RetrievePassword(uriLonger,"synthetic-user-long"),"Clearing one address leaves the other.");
				//A user name that is not the one on the entry is not found, even where the target name would match.
				Assert.ThrowsExactly<Win32Exception>(() => credentialStore.RetrievePassword(uriLonger,"synthetic-user-other"));
			});
		}

		[TestMethod]
		public void BlankUserOrPassword_Throws_AndNothingIsWritten() {
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				CredentialStore credentialStore=testCredentialStore.Store;
				credentialStore.FuncOpenVault=() => null;
				Assert.ThrowsExactly<ArgumentException>(() => credentialStore.WritePassword(URI,USER,""));
				Assert.ThrowsExactly<ArgumentException>(() => credentialStore.WritePassword(URI,USER,null));
				Assert.ThrowsExactly<ArgumentException>(() => credentialStore.WritePassword(URI,"",PASSWORD));
				Assert.ThrowsExactly<ArgumentException>(() => credentialStore.WritePassword(URI,USER,new string('x',CredentialManager.MAX_SECRET_BYTES/2+1)));
				//Nothing is there, not even the migration's record (a vault that cannot be opened leaves none).
				CollectionAssert.AreEqual(new string[0],CredentialManager.List(testCredentialStore.Prefix).Select(x => x.TargetName).ToArray());
				credentialStore.WritePassword(URI,USER,new string('x',CredentialManager.MAX_SECRET_BYTES/2));
				Assert.AreEqual(CredentialManager.MAX_SECRET_BYTES/2,credentialStore.RetrievePassword(URI,USER).Length);
			});
		}

		[TestMethod]
		public void Migration_CopiesTheVaultEntries_OnFirstUse_AndRunsOnce() {
			OldPasswordVault oldPasswordVault=OpenVaultOrInconclusive();
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				testCredentialStore.AddToVault(oldPasswordVault,testCredentialStore.VaultResource(URI),USER,PASSWORD);
				CredentialStore credentialStore=testCredentialStore.Store;
				Assert.IsNull(credentialStore.GetMigrationRecord(),"No record before the first use.");
				//The first use of the store: the entry written through PasswordVault is readable through the Credential Manager.
				Assert.IsTrue(credentialStore.TryRetrieveUserName(URI,out string userName));
				Assert.AreEqual(USER,userName);
				Assert.AreEqual(PASSWORD,credentialStore.RetrievePassword(URI,USER));
				Assert.AreEqual(PASSWORD,CredentialManager.Read(testCredentialStore.Prefix+"cm:"+URI+"|"+USER).Secret);
				string record=credentialStore.GetMigrationRecord();
				Assert.IsNotNull(record);
				StringAssert.Contains(record,"Copied 1, already there 0, failed 0");
				StringAssert.Contains(record," UTC: ");
				//The old entry stays where it was (an older release still finds it).
				Assert.AreEqual(1,oldPasswordVault.FindAllByResource(testCredentialStore.VaultResource(URI)).Count);
				//Runs once: an entry the old vault gets later is not copied, neither in this process nor in the next one.
				string uriLater="https://mt2.example.test/ServiceMain.asmx";
				testCredentialStore.AddToVault(oldPasswordVault,testCredentialStore.VaultResource(uriLater),USER,"synthetic-password-later");
				Assert.IsFalse(credentialStore.TryRetrieveUserName(uriLater,out string _));
				CredentialStore credentialStoreNextProcess=testCredentialStore.NewStore();
				Assert.IsFalse(credentialStoreNextProcess.TryRetrieveUserName(uriLater,out string _));
				Assert.AreEqual(record,credentialStoreNextProcess.GetMigrationRecord(),"The record is written once.");
			});
		}

		[TestMethod]
		public void Migration_NeverReplacesALoginAlreadyThere_AndCopiesOnlyItsOwnResourceName() {
			OldPasswordVault oldPasswordVault=OpenVaultOrInconclusive();
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				//The same address and user saved in the Credential Manager already (newer), and an entry of another resource name.
				CredentialManager.Write(testCredentialStore.Prefix+"cm:"+URI+"|"+USER,USER,"synthetic-password-newer");
				testCredentialStore.AddToVault(oldPasswordVault,testCredentialStore.VaultResource(URI),USER,"synthetic-password-older");
				string resourceOther=testCredentialStore.Prefix+"other:"+URI;
				testCredentialStore.AddToVault(oldPasswordVault,resourceOther,USER,"synthetic-password-other");
				//The store's resource name without an address after it is not a login either.
				testCredentialStore.AddToVault(oldPasswordVault,testCredentialStore.VaultResource(""),USER,"synthetic-password-noaddress");
				CredentialStore credentialStore=testCredentialStore.Store;
				Assert.AreEqual("synthetic-password-newer",credentialStore.RetrievePassword(URI,USER));
				StringAssert.Contains(credentialStore.GetMigrationRecord(),"Copied 0, already there 1, failed 0");
				List<CredentialEntry> listEntries=CredentialManager.List(testCredentialStore.Prefix);
				CollectionAssert.AreEquivalent(new[] { testCredentialStore.Prefix+"cm:"+URI+"|"+USER,testCredentialStore.Prefix+"record" },
					listEntries.Select(x => x.TargetName).ToArray(),"Nothing of the other resource name was copied.");
			});
		}

		[TestMethod]
		public void Clear_AlsoRemovesTheOldVaultEntriesOfTheAddress() {
			OldPasswordVault oldPasswordVault=OpenVaultOrInconclusive();
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				string uriOther="https://mt2.example.test/ServiceMain.asmx";
				testCredentialStore.AddToVault(oldPasswordVault,testCredentialStore.VaultResource(URI),USER,PASSWORD);
				testCredentialStore.AddToVault(oldPasswordVault,testCredentialStore.VaultResource(uriOther),USER,PASSWORD);
				CredentialStore credentialStore=testCredentialStore.Store;
				credentialStore.ClearCredentials(URI);
				Assert.IsFalse(credentialStore.TryRetrieveUserName(URI,out string _));
				Assert.AreEqual(0,oldPasswordVault.FindAllByResource(testCredentialStore.VaultResource(URI)).Count,"A cleared login cannot come back.");
				Assert.AreEqual(1,oldPasswordVault.FindAllByResource(testCredentialStore.VaultResource(uriOther)).Count);
				Assert.AreEqual(PASSWORD,credentialStore.RetrievePassword(uriOther,USER));
			});
		}

		[TestMethod]
		public void Migration_WhenTheVaultCannotBeOpened_LeavesNoRecord_AndTheNextProcessTriesAgain() {
			//On Windows 8 and later the vault is always there, so failing to open it (null, or an exception from Activator.CreateInstance) is an
			//error that may pass, such as the Credential Manager service not started yet early in a logon: no record, so the logins still move later.
			Func<OldPasswordVault>[] arrayFuncsOpen=new Func<OldPasswordVault>[] {
				() => null,
				() => throw new System.Runtime.InteropServices.COMException("Synthetic: the RPC server is unavailable.",unchecked((int)0x800706BA)),
			};
			foreach(Func<OldPasswordVault> funcOpen in arrayFuncsOpen) {
				TestCredentialStore.Run(TestContext,testCredentialStore => {
					CredentialStore credentialStore=testCredentialStore.Store;
					int countOpens=0;
					credentialStore.FuncIsOsWithoutVault=() => false;
					credentialStore.FuncOpenVault=() => { countOpens++; return funcOpen(); };
					credentialStore.WritePassword(URI,USER,PASSWORD);
					Assert.IsNull(credentialStore.GetMigrationRecord(),"No record: the migration is not done.");
					Assert.AreEqual(PASSWORD,credentialStore.RetrievePassword(URI,USER),"The logins work without the vault.");
					Assert.IsTrue(credentialStore.TryRetrieveUserName(URI,out string userName));
					Assert.AreEqual(USER,userName);
					Assert.AreEqual(1,countOpens,"One try per process, not one per call.");
					//The next process tries again; a failed open costs that process one try and writes no record either.
					CredentialStore credentialStoreNextProcess=testCredentialStore.NewStore();
					int countOpensNext=0;
					credentialStoreNextProcess.FuncIsOsWithoutVault=() => false;
					credentialStoreNextProcess.FuncOpenVault=() => { countOpensNext++; return funcOpen(); };
					Assert.AreEqual(PASSWORD,credentialStoreNextProcess.RetrievePassword(URI,USER));
					Assert.AreEqual(1,countOpensNext,"The next process tried to open the vault again.");
					Assert.IsNull(credentialStoreNextProcess.GetMigrationRecord());
					credentialStoreNextProcess.WritePassword(URI,USER,"synthetic-password-2");
					Assert.AreEqual("synthetic-password-2",credentialStoreNextProcess.RetrievePassword(URI,USER));
				});
			}
		}

		[TestMethod]
		public void Migration_OnAWindowsWithoutTheVault_RecordsThatThereWasNothingToRead() {
			//Only a Windows older than 8, which has no PasswordVault, records the migration without reading the vault.
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				CredentialStore credentialStore=testCredentialStore.Store;
				credentialStore.FuncIsOsWithoutVault=() => true;
				credentialStore.FuncOpenVault=() => throw new AssertFailedException("The vault is not opened on a Windows without it.");
				credentialStore.WritePassword(URI,USER,PASSWORD);
				StringAssert.Contains(credentialStore.GetMigrationRecord(),"Copied 0, already there 0, failed 0, PasswordVault not available");
				Assert.AreEqual(PASSWORD,credentialStore.RetrievePassword(URI,USER));
			});
		}

		///<summary>A start of the program (a new store with the test's names) whose old vault is the given fake: the real vault is not touched.
		///</summary>
		private static CredentialStore StartWithVault(TestCredentialStore testCredentialStore,IOldVault oldVault) {
			CredentialStore credentialStore=testCredentialStore.NewStore();
			credentialStore.FuncIsOsWithoutVault=() => false;
			credentialStore.FuncOpenVault=() => oldVault;
			return credentialStore;
		}

		[TestMethod]
		public void Login_IsFoundWhateverTheCaseOfTheAddressOrUser() {
			//S3a-5f item 1: Windows matches target names without regard to case, so the store does too.
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				CredentialStore credentialStore=StartWithVault(testCredentialStore,new FakeOldVault());
				string uriUpper=URI.ToUpperInvariant();
				string userUpper=USER.ToUpperInvariant();
				credentialStore.WritePassword(URI,USER,PASSWORD);
				Assert.IsTrue(credentialStore.TryRetrieveUserName(uriUpper,out string userName),"A differently-cased address finds its login.");
				Assert.AreEqual(USER,userName);
				Assert.AreEqual(PASSWORD,credentialStore.RetrievePassword(uriUpper,USER));
				Assert.AreEqual(PASSWORD,credentialStore.RetrievePassword(URI,userUpper));
				//Saved again in another casing: Windows keeps one credential, and the store shows one login.
				credentialStore.WritePassword(uriUpper,userUpper,"synthetic-password-2");
				Assert.AreEqual(1,CredentialManager.List(testCredentialStore.Prefix+"cm:").Count);
				Assert.AreEqual(1,credentialStore.GetUserNames(URI).Count);
				Assert.AreEqual("synthetic-password-2",credentialStore.RetrievePassword(URI,USER));
				Assert.AreEqual(testCredentialStore.Prefix+"cm:"+URI+"|"+USER,CredentialManager.List(testCredentialStore.Prefix+"cm:")[0].TargetName,
					"Saved again under the casing it has, not a second one that Windows would hide.");
				credentialStore.ClearCredentials(uriUpper);
				Assert.AreEqual(0,CredentialManager.List(testCredentialStore.Prefix+"cm:").Count,"Clearing in another casing clears the login.");
				Assert.IsFalse(credentialStore.TryRetrieveUserName(URI,out string _));
				//Two casings written directly (as the S3a-5 store could): Windows shows one; clearing removes both, the hidden one too.
				CredentialManager.Write(testCredentialStore.Prefix+"cm:"+URI+"|"+USER,USER,"synthetic-password-3");
				CredentialManager.Write(testCredentialStore.Prefix+"cm:"+uriUpper+"|"+userUpper,userUpper,"synthetic-password-4");
				Assert.IsTrue(credentialStore.TryRetrieveUserName(URI,out string _));
				credentialStore.ClearCredentials(URI);
				Assert.IsNull(CredentialManager.ReadInfo(testCredentialStore.Prefix+"cm:"+URI+"|"+USER),"No casing is left.");
				Assert.AreEqual(0,CredentialManager.List(testCredentialStore.Prefix+"cm:").Count);
			});
		}

		[TestMethod]
		public void TargetName_IsUnambiguousWithAPipe_AndUnchangedWithout() {
			//S3a-5f item 2. Names are computed only; nothing is written. The store has the per-run names of every other test (S3a-FU1).
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				CredentialStore credentialStore=testCredentialStore.NewStore();
				string prefix=credentialStore.TargetPrefix;
				Assert.AreEqual(testCredentialStore.Prefix+"cm:",prefix);
				//Without '|' the name is byte for byte the one S3a-5 wrote, a backslash and '%' included, so the saved logins are found as before.
				Assert.AreEqual(prefix+URI+"|"+USER,credentialStore.GetTargetName(URI,USER));
				Assert.AreEqual(prefix+@"https://mt.example.test/a%20b%7C|DOMAIN\user",credentialStore.GetTargetName("https://mt.example.test/a%20b%7C",@"DOMAIN\user"));
				Assert.AreEqual(prefix+"https://mt.example.test/a%_b|c",credentialStore.GetTargetName("https://mt.example.test/a%_b","c"));
				//With '|' in either part: escaped, no '|' after the prefix.
				Assert.AreEqual(prefix+"https://mt.example.test/a%7Cb%_c",credentialStore.GetTargetName("https://mt.example.test/a|b","c"));
				Assert.AreEqual(prefix+"https://mt.example.test/a%_b%7Cc",credentialStore.GetTargetName("https://mt.example.test/a","b|c"));
				Assert.AreEqual(prefix+@"https://mt.example.test/a%2520b%_DOMAIN\x%7Cy",credentialStore.GetTargetName("https://mt.example.test/a%20b",@"DOMAIN\x|y"));
				//No two logins share a name, also as Windows compares them (case-insensitively).
				string[][] arrayLogins=new[] {
					new[] { "a|b","c" },new[] { "a","b|c" },new[] { "a|","b" },new[] { "a","|b" },new[] { "a||","b" },new[] { "a|","|b" },new[] { "a","||b" },
					new[] { "a%7Cb","c" },new[] { "a%7Cb|","c" },new[] { "a","%7Cb|c" },new[] { "a%_b","c|" },new[] { "a","%_b|c" },new[] { "a%","_b|c" },
					new[] { "a%25","b|" },new[] { "a%","25b|" },new[] { "a","b" },new[] { @"a\","b|" },new[] { "A|B","c" },
				};
				List<string> listNames=arrayLogins.Select(x => credentialStore.GetTargetName(x[0],x[1])).ToList();
				Assert.AreEqual(arrayLogins.Length-1,listNames.Distinct(StringComparer.OrdinalIgnoreCase).Count(),
					"Only \"A|B\"+\"c\" shares the name of \"a|b\"+\"c\", as Windows would: "+string.Join("  ",listNames));
				Assert.AreEqual(arrayLogins.Length,listNames.Distinct(StringComparer.Ordinal).Count());
			});
		}

		[TestMethod]
		public void Login_WithAPipe_IsStoredUnambiguously() {
			//S3a-5f item 2: the address "a|b" with the user "c" and the address "a" with the user "b|c" are two logins.
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				CredentialStore credentialStore=StartWithVault(testCredentialStore,new FakeOldVault());
				string uriA="https://mt.example.test/a";
				string uriAB="https://mt.example.test/a|b";
				credentialStore.WritePassword(uriAB,"c","synthetic-password-ab");
				credentialStore.WritePassword(uriA,"b|c","synthetic-password-a");
				Assert.AreEqual("synthetic-password-ab",credentialStore.RetrievePassword(uriAB,"c"));
				Assert.AreEqual("synthetic-password-a",credentialStore.RetrievePassword(uriA,"b|c"));
				CollectionAssert.AreEqual(new[] { "c" },credentialStore.GetUserNames(uriAB));
				CollectionAssert.AreEqual(new[] { "b|c" },credentialStore.GetUserNames(uriA));
				CollectionAssert.AreEquivalent(new[] { credentialStore.GetTargetName(uriAB,"c"),credentialStore.GetTargetName(uriA,"b|c") },
					CredentialManager.List(testCredentialStore.Prefix+"cm:").Select(x => x.TargetName).ToArray());
				credentialStore.ClearCredentials(uriA);
				Assert.IsFalse(credentialStore.TryRetrieveUserName(uriA,out string _));
				Assert.AreEqual("synthetic-password-ab",credentialStore.RetrievePassword(uriAB,"c"),"Clearing one leaves the other.");
			});
		}

		[TestMethod]
		public void Login_WithAPipe_SavedUnderTheS3a5Name_IsStillFound_AndMovesWhenSavedAgain() {
			//S3a-5f item 2, the read fallback. S3a-5 saved both logins under the one name "...a|b|c"; the entry's own user name tells which it is.
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				CredentialStore credentialStore=StartWithVault(testCredentialStore,new FakeOldVault());
				string uriA="https://mt.example.test/a";
				string uriAB="https://mt.example.test/a|b";
				string targetNameS3a5=testCredentialStore.Prefix+"cm:"+uriAB+"|c";
				Assert.AreEqual(targetNameS3a5,credentialStore.GetTargetNameUnescaped(uriA,"b|c"),"The S3a-5 name was ambiguous.");
				CredentialManager.Write(targetNameS3a5,"c","synthetic-password-old");
				Assert.AreEqual("synthetic-password-old",credentialStore.RetrievePassword(uriAB,"c"));
				CollectionAssert.AreEqual(new[] { "c" },credentialStore.GetUserNames(uriAB));
				Assert.IsFalse(credentialStore.TryRetrieveUserName(uriA,out string _),"Its own user name says it is not the login of \"a\" + \"b|c\".");
				Assert.ThrowsExactly<Win32Exception>(() => credentialStore.RetrievePassword(uriA,"b|c"));
				//Saved again: under the escaped name, and the S3a-5 entry of this login goes.
				credentialStore.WritePassword(uriAB,"c","synthetic-password-new");
				CollectionAssert.AreEqual(new[] { credentialStore.GetTargetName(uriAB,"c") },
					CredentialManager.List(testCredentialStore.Prefix+"cm:").Select(x => x.TargetName).ToArray());
				Assert.AreEqual("synthetic-password-new",credentialStore.RetrievePassword(uriAB,"c"));
				//The same S3a-5 name holding the other login: found for that one only, and not deleted when the first login is saved.
				CredentialManager.Write(targetNameS3a5,"b|c","synthetic-password-old-a");
				Assert.AreEqual("synthetic-password-old-a",credentialStore.RetrievePassword(uriA,"b|c"));
				CollectionAssert.AreEqual(new[] { "b|c" },credentialStore.GetUserNames(uriA));
				CollectionAssert.AreEqual(new[] { "c" },credentialStore.GetUserNames(uriAB));
				credentialStore.WritePassword(uriAB,"c","synthetic-password-newer");
				Assert.IsNotNull(CredentialManager.ReadInfo(targetNameS3a5),"Another login's entry stays.");
				//Clearing the address deletes its S3a-5 entry too.
				credentialStore.ClearCredentials(uriA);
				Assert.IsNull(CredentialManager.ReadInfo(targetNameS3a5));
				Assert.AreEqual("synthetic-password-newer",credentialStore.RetrievePassword(uriAB,"c"));
			});
		}

		[TestMethod]
		public void Migration_RetriesOnlyTheEntriesThatFailed_OnLaterStarts() {
			//S3a-5f item 3: a fake vault whose one entry cannot be read; the record names it, later starts retry it alone.
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				FakeOldVault fakeOldVault=new FakeOldVault();
				string uriFailing="https://mt2.example.test/ServiceMain.asmx";
				string resourceFailing=testCredentialStore.VaultResource(uriFailing);
				fakeOldVault.Add(testCredentialStore.VaultResource(URI),USER,PASSWORD);
				fakeOldVault.Add(resourceFailing,USER,"synthetic-password-2");
				fakeOldVault.SetResourcesFailing.Add(resourceFailing);
				CredentialStore credentialStore=StartWithVault(testCredentialStore,fakeOldVault);
				Assert.AreEqual(PASSWORD,credentialStore.RetrievePassword(URI,USER));
				Assert.IsFalse(credentialStore.TryRetrieveUserName(uriFailing,out string _));
				string record=credentialStore.GetMigrationRecord();
				StringAssert.Contains(record,"Copied 1, already there 0, failed 1\nRetry: 1 of 5 tries done; on the next start: "
					+MigrationResult.GetEntryId(resourceFailing,USER));
				Assert.IsFalse(record.Contains("mt2.example.test"),"The record names the entry by a hash, not by its address.");
				//The next start, still failing: only the failed entry is read again; an entry the vault got since is not copied by a retry.
				string uriLater="https://mt3.example.test/ServiceMain.asmx";
				fakeOldVault.Add(testCredentialStore.VaultResource(uriLater),USER,"synthetic-password-later");
				fakeOldVault.ListResourcesRead.Clear();
				credentialStore=StartWithVault(testCredentialStore,fakeOldVault);
				Assert.IsFalse(credentialStore.TryRetrieveUserName(uriFailing,out string _));
				CollectionAssert.AreEqual(new[] { resourceFailing },fakeOldVault.ListResourcesRead);
				StringAssert.Contains(credentialStore.GetMigrationRecord(),"Copied 1, already there 0, failed 1, in 2 tries\nRetry: 2 of 5 tries done");
				//The start after: the entry can be read now, so it is copied and the record is complete.
				fakeOldVault.SetResourcesFailing.Clear();
				fakeOldVault.ListResourcesRead.Clear();
				credentialStore=StartWithVault(testCredentialStore,fakeOldVault);
				Assert.AreEqual("synthetic-password-2",credentialStore.RetrievePassword(uriFailing,USER));
				CollectionAssert.AreEqual(new[] { resourceFailing },fakeOldVault.ListResourcesRead);
				record=credentialStore.GetMigrationRecord();
				StringAssert.EndsWith(record,"Copied 2, already there 0, failed 0, in 3 tries");
				Assert.IsFalse(credentialStore.TryRetrieveUserName(uriLater,out string _));
				Assert.IsNull(MigrationResult.GetRetry(record));
				//Complete: the vault is not read again.
				int countRetrieveAll=fakeOldVault.CountRetrieveAll;
				credentialStore=StartWithVault(testCredentialStore,fakeOldVault);
				Assert.IsTrue(credentialStore.TryRetrieveUserName(URI,out string _));
				Assert.AreEqual(countRetrieveAll,fakeOldVault.CountRetrieveAll);
				Assert.AreEqual(record,credentialStore.GetMigrationRecord());
			});
		}

		[TestMethod]
		public void Migration_StopsRetryingAfterFiveStarts() {
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				FakeOldVault fakeOldVault=new FakeOldVault();
				string resourceFailing=testCredentialStore.VaultResource(URI);
				fakeOldVault.Add(resourceFailing,USER,PASSWORD);
				fakeOldVault.SetResourcesFailing.Add(resourceFailing);
				for(int start=1;start<=MigrationResult.MAX_TRIES;start++) {
					CredentialStore credentialStore=StartWithVault(testCredentialStore,fakeOldVault);
					Assert.IsFalse(credentialStore.TryRetrieveUserName(URI,out string _));
					string record=credentialStore.GetMigrationRecord();
					Assert.AreEqual(start,fakeOldVault.ListResourcesRead.Count,"Start "+start+": one more try.");
					if(start<MigrationResult.MAX_TRIES) {
						StringAssert.Contains(record,"Retry: "+start+" of 5 tries done");
					}
					else {
						StringAssert.EndsWith(record,"Copied 0, already there 0, failed 1, in 5 tries");
						Assert.IsNull(MigrationResult.GetRetry(record));
					}
				}
				int countRetrieveAll=fakeOldVault.CountRetrieveAll;
				CredentialStore credentialStoreAfter=StartWithVault(testCredentialStore,fakeOldVault);
				Assert.IsFalse(credentialStoreAfter.TryRetrieveUserName(URI,out string _));
				Assert.AreEqual(countRetrieveAll,fakeOldVault.CountRetrieveAll,"After the fifth start the vault is not read again.");
			});
		}

		[TestMethod]
		public void Migration_NamesAtMost50_AndSaysHowManyAreNotNamed() {
			//S3a-FU1 (S3a-5f leftover 1): 53 entries fail; 50 are named for a retry, the record says 3 are not, and those stay counted as failed.
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				FakeOldVault fakeOldVault=new FakeOldVault();
				List<string> listResources=Enumerable.Range(0,MigrationResult.MAX_RETRY_IDS+3)
					.Select(x => testCredentialStore.VaultResource("https://mt"+x+".example.test/ServiceMain.asmx")).ToList();
				foreach(string resource in listResources) {
					fakeOldVault.Add(resource,USER,"synthetic-password-"+resource.Length);
					fakeOldVault.SetResourcesFailing.Add(resource);
				}
				CredentialStore credentialStore=StartWithVault(testCredentialStore,fakeOldVault);
				credentialStore.EnsureMigrated();
				string record=credentialStore.GetMigrationRecord();
				StringAssert.Contains(record,"Copied 0, already there 0, failed 53\nRetry: 1 of 5 tries done; on the next start: ");
				StringAssert.EndsWith(record,", 3 not named");
				MigrationResult migrationResult=MigrationResult.GetRetry(record);
				Assert.AreEqual(MigrationResult.MAX_RETRY_IDS,migrationResult.ListRetryIds.Count);
				//The next start, still failing, retries the 50 named only; the 3 others stay failed and unnamed.
				fakeOldVault.ListResourcesRead.Clear();
				StartWithVault(testCredentialStore,fakeOldVault).EnsureMigrated();
				Assert.AreEqual(MigrationResult.MAX_RETRY_IDS,fakeOldVault.ListResourcesRead.Count);
				record=credentialStore.GetMigrationRecord();
				StringAssert.Contains(record,"Copied 0, already there 0, failed 53, in 2 tries\nRetry: 2 of 5 tries done; on the next start: ");
				StringAssert.EndsWith(record,", 3 not named");
				//Readable now: the 50 named are copied; the record is complete and still counts the 3 as failed.
				fakeOldVault.SetResourcesFailing.Clear();
				fakeOldVault.ListResourcesRead.Clear();
				StartWithVault(testCredentialStore,fakeOldVault).EnsureMigrated();
				Assert.AreEqual(MigrationResult.MAX_RETRY_IDS,fakeOldVault.ListResourcesRead.Count);
				record=credentialStore.GetMigrationRecord();
				StringAssert.EndsWith(record,"Copied 50, already there 0, failed 3, in 3 tries");
				Assert.IsNull(MigrationResult.GetRetry(record));
				Assert.AreEqual(MigrationResult.MAX_RETRY_IDS,CredentialManager.List(testCredentialStore.Prefix+"cm:").Count);
			});
		}

		[TestMethod]
		public void Migration_RetryStartsWithoutTheVault_CountAsTries() {
			//S3a-FU1 (S3a-5f leftover 3): on a retry, a vault that cannot be opened (null or an exception) or listed is a try without the vault,
			//so the retries end after five starts instead of going on at every start. The first try without the vault still writes no record.
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				FakeOldVault fakeOldVault=new FakeOldVault();
				string resourceFailing=testCredentialStore.VaultResource("https://mt2.example.test/ServiceMain.asmx");
				fakeOldVault.Add(testCredentialStore.VaultResource(URI),USER,PASSWORD);
				fakeOldVault.Add(resourceFailing,USER,"synthetic-password-2");
				fakeOldVault.SetResourcesFailing.Add(resourceFailing);
				string entryId=MigrationResult.GetEntryId(resourceFailing,USER);
				StartWithVault(testCredentialStore,fakeOldVault).EnsureMigrated();
				CredentialStore credentialStore=testCredentialStore.NewStore();
				StringAssert.EndsWith(credentialStore.GetMigrationRecord(),"Copied 1, already there 0, failed 1\nRetry: 1 of 5 tries done; on the next start: "+entryId);
				//Start 2: the vault cannot be opened.
				CredentialStore credentialStoreStart=StartWithVault(testCredentialStore,null);
				credentialStoreStart.EnsureMigrated();
				StringAssert.EndsWith(credentialStore.GetMigrationRecord(),
					"Copied 1, already there 0, failed 1, in 2 tries (1 without the vault)\nRetry: 2 of 5 tries done (1 without the vault); on the next start: "+entryId);
				//Start 3: opening it throws.
				credentialStoreStart=testCredentialStore.NewStore();
				credentialStoreStart.FuncIsOsWithoutVault=() => false;
				credentialStoreStart.FuncOpenVault=() => throw new System.Runtime.InteropServices.COMException("Synthetic: the RPC server is unavailable.",unchecked((int)0x800706BA));
				credentialStoreStart.EnsureMigrated();
				StringAssert.Contains(credentialStore.GetMigrationRecord(),"\nRetry: 3 of 5 tries done (2 without the vault); on the next start: "+entryId);
				//Start 4: it opens but cannot be listed.
				fakeOldVault.IsRetrieveAllFailing=true;
				StartWithVault(testCredentialStore,fakeOldVault).EnsureMigrated();
				StringAssert.Contains(credentialStore.GetMigrationRecord(),"\nRetry: 4 of 5 tries done (3 without the vault); on the next start: "+entryId);
				//Start 5, the last try, without the vault again: the record is complete, the entry still counted as failed.
				StartWithVault(testCredentialStore,null).EnsureMigrated();
				string record=credentialStore.GetMigrationRecord();
				StringAssert.EndsWith(record,"Copied 1, already there 0, failed 1, in 5 tries (4 without the vault)");
				Assert.IsNull(MigrationResult.GetRetry(record));
				//Afterwards the vault is not even opened.
				int countOpens=0;
				credentialStoreStart=testCredentialStore.NewStore();
				credentialStoreStart.FuncIsOsWithoutVault=() => false;
				credentialStoreStart.FuncOpenVault=() => { countOpens++; return fakeOldVault; };
				credentialStoreStart.EnsureMigrated();
				Assert.AreEqual(0,countOpens);
				Assert.AreEqual(record,credentialStore.GetMigrationRecord());
			});
		}

		[TestMethod]
		public void Clear_DuringARetry_DropsTheAddressFromTheRecord() {
			//S3a-FU1 (S3a-5f leftover 2): a login cleared while its old-vault entry waits for a retry is not copied back by the retry, also when
			//the vault refuses to remove the entry, or cannot be opened at the clear (the entry then named by the user of the cleared login).
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				FakeOldVault fakeOldVault=new FakeOldVault();
				string uriOther="https://mt2.example.test/ServiceMain.asmx";
				string resource=testCredentialStore.VaultResource(URI);
				string resourceOther=testCredentialStore.VaultResource(uriOther);
				fakeOldVault.Add(resource,USER,PASSWORD);
				fakeOldVault.Add(resourceOther,USER,"synthetic-password-2");
				fakeOldVault.SetResourcesFailing.Add(resource);
				fakeOldVault.SetResourcesFailing.Add(resourceOther);
				StartWithVault(testCredentialStore,fakeOldVault).EnsureMigrated();
				CredentialStore credentialStoreRecord=testCredentialStore.NewStore();
				StringAssert.Contains(credentialStoreRecord.GetMigrationRecord(),"failed 2\nRetry: 1 of 5 tries done");
				//Start 2, still failing; the address is cleared and the vault refuses the removal.
				fakeOldVault.IsRemoveFailing=true;
				CredentialStore credentialStore=StartWithVault(testCredentialStore,fakeOldVault);
				credentialStore.ClearCredentials(URI);
				Assert.AreEqual(1,fakeOldVault.FindAllByResource(resource).Count,"The fake kept the entry, as a vault that refuses the removal.");
				StringAssert.EndsWith(credentialStoreRecord.GetMigrationRecord(),
					"Copied 0, already there 0, failed 1, in 2 tries\nRetry: 2 of 5 tries done; on the next start: "+MigrationResult.GetEntryId(resourceOther,USER));
				//Start 3, readable now: only the other address is copied; the cleared one does not come back.
				fakeOldVault.SetResourcesFailing.Clear();
				fakeOldVault.ListResourcesRead.Clear();
				credentialStore=StartWithVault(testCredentialStore,fakeOldVault);
				Assert.IsFalse(credentialStore.TryRetrieveUserName(URI,out string _),"The cleared login is not copied back.");
				Assert.AreEqual("synthetic-password-2",credentialStore.RetrievePassword(uriOther,USER));
				CollectionAssert.AreEqual(new[] { resourceOther },fakeOldVault.ListResourcesRead);
				StringAssert.EndsWith(credentialStoreRecord.GetMigrationRecord(),"Copied 1, already there 0, failed 0, in 3 tries");
			});
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				FakeOldVault fakeOldVault=new FakeOldVault();
				string resource=testCredentialStore.VaultResource(URI);
				fakeOldVault.Add(resource,USER,PASSWORD);
				fakeOldVault.SetResourcesFailing.Add(resource);
				StartWithVault(testCredentialStore,fakeOldVault).EnsureMigrated();
				//Start 2 without the vault: the user saves the login by hand, then clears it.
				CredentialStore credentialStore=StartWithVault(testCredentialStore,null);
				credentialStore.WritePassword(URI,USER,"synthetic-password-typed");
				credentialStore.ClearCredentials(URI);
				string record=credentialStore.GetMigrationRecord();
				StringAssert.EndsWith(record,"Copied 0, already there 0, failed 0, in 2 tries (1 without the vault)");
				Assert.IsNull(MigrationResult.GetRetry(record),"Nothing is left to retry.");
				//Start 3 with a readable vault: complete, so the vault is not read and the cleared login does not come back.
				fakeOldVault.SetResourcesFailing.Clear();
				int countRetrieveAll=fakeOldVault.CountRetrieveAll;
				credentialStore=StartWithVault(testCredentialStore,fakeOldVault);
				Assert.IsFalse(credentialStore.TryRetrieveUserName(URI,out string _));
				Assert.AreEqual(countRetrieveAll,fakeOldVault.CountRetrieveAll);
			});
		}

		[TestMethod]
		public void Run_CleansUp_AndAProblemNeverHidesTheBodysException() {
			//S3a-FU1 (S3a-5f leftover 4): a clean-up problem fails a test whose body succeeded, and never replaces the exception of a body that
			//threw. The problems here are added by the test; the entry it writes is under its own prefix and deleted by the clean-up.
			string prefix=null;
			InvalidOperationException invalidOperationException=Assert.ThrowsExactly<InvalidOperationException>(() =>
				TestCredentialStore.Run(TestContext,testCredentialStore => {
					prefix=testCredentialStore.Prefix;
					CredentialManager.Write(testCredentialStore.Prefix+"cm:"+URI+"|"+USER,USER,PASSWORD);
					testCredentialStore.AddCleanUpProblem("Synthetic clean-up problem.");
					throw new InvalidOperationException("Synthetic: the body failed.");
				}));
			Assert.AreEqual("Synthetic: the body failed.",invalidOperationException.Message);
			Assert.AreEqual(0,CredentialManager.List(prefix).Count,"Cleaned up although the body threw.");
			string messageFailed=null;
			try {
				TestCredentialStore.Run(TestContext,testCredentialStore => testCredentialStore.AddCleanUpProblem("Synthetic clean-up problem."));
			}
			catch(AssertFailedException ex) {
				messageFailed=ex.Message;
			}
			Assert.IsNotNull(messageFailed,"A clean-up problem fails a test whose body succeeded.");
			StringAssert.Contains(messageFailed,"Synthetic clean-up problem.");
		}

		[TestMethod]
		public void MigrationRecord_OfS3a5_OrDamaged_IsComplete() {
			//Records are parsed only; nothing is written.
			Assert.IsNull(MigrationResult.GetRetry("2026-11-15 10:00:00 UTC: Copied 1, already there 0, failed 1"),"An S3a-5 record is complete.");
			Assert.IsNull(MigrationResult.GetRetry("2026-11-15 10:00:00 UTC: Copied 0, already there 0, failed 0, PasswordVault not available"));
			Assert.IsNull(MigrationResult.GetRetry("2026-11-15 10:00:00 UTC: Copied 1\nRetry: 1 of 5 tries done; on the next start: zz"),"Damaged.");
			Assert.IsNull(MigrationResult.GetRetry("2026-11-15 10:00:00 UTC: Copied 0, already there 0, failed 1\nRetry: 5 of 5 tries done; on the next start: 0123456789abcdef"));
			MigrationResult migrationResult=MigrationResult.GetRetry(
				"2026-11-15 10:00:00 UTC: Copied 3, already there 1, failed 2, in 2 tries\nRetry: 2 of 5 tries done; on the next start: 0123456789abcdef,fedcba9876543210");
			Assert.IsNotNull(migrationResult);
			Assert.AreEqual(3,migrationResult.CountCopied);
			Assert.AreEqual(1,migrationResult.CountAlreadyThere);
			Assert.AreEqual(2,migrationResult.CountTries);
			CollectionAssert.AreEqual(new[] { "0123456789abcdef","fedcba9876543210" },migrationResult.ListRetryIds);
			//The record of many failed entries stays within the Credential Manager's limit.
			MigrationResult migrationResultMany=new MigrationResult { CountFailed=500,IsVaultAvailable=true,CountTries=4,
				CountTriesWithoutVault=3,CountNotNamed=450,
				ListRetryIds=Enumerable.Range(0,MigrationResult.MAX_RETRY_IDS).Select(x => MigrationResult.GetEntryId("r"+x,"u")).ToList() };
			Assert.IsTrue(("2026-11-15 10:00:00 UTC: "+migrationResultMany).Length<=CredentialManager.MAX_SECRET_BYTES/2);
			//S3a-FU1: the entries past the 50 named, and the tries without the vault, are written and read back.
			StringAssert.Contains(migrationResultMany.ToString(),"failed 500, in 4 tries (3 without the vault)\nRetry: 4 of 5 tries done (3 without the vault); on the next start: ");
			StringAssert.EndsWith(migrationResultMany.ToString(),", 450 not named");
			MigrationResult migrationResultRead=MigrationResult.GetRetry("2026-11-15 10:00:00 UTC: "+migrationResultMany);
			Assert.IsNotNull(migrationResultRead);
			Assert.AreEqual(3,migrationResultRead.CountTriesWithoutVault);
			Assert.AreEqual(450,migrationResultRead.CountNotNamed);
			Assert.AreEqual(500,migrationResultRead.CountFailed);
			Assert.AreEqual(4,migrationResultRead.CountTries);
			CollectionAssert.AreEqual(migrationResultMany.ListRetryIds,migrationResultRead.ListRetryIds);
			Assert.AreEqual(0,migrationResult.CountTriesWithoutVault,"An S3a-5f retry line has neither part.");
			Assert.AreEqual(0,migrationResult.CountNotNamed);
		}

		[TestMethod]
		public void Migration_AlreadyThere_IsCheckedWithoutReadingTheSecret() {
			//S3a-5f item 4.
			TestCredentialStore.Run(TestContext,testCredentialStore => {
				string targetName=testCredentialStore.Prefix+"cm:"+URI+"|"+USER;
				CredentialManager.Write(targetName,USER,"synthetic-password-newer");
				CredentialEntry credentialEntry=CredentialManager.ReadInfo(targetName);
				Assert.AreEqual(USER,credentialEntry.UserName);
				Assert.AreEqual("",credentialEntry.Secret,"ReadInfo never decodes the secret.");
				Assert.IsNull(CredentialManager.ReadInfo(testCredentialStore.Prefix+"cm:none"));
				//A login with '|' saved under its S3a-5 name is there too.
				string uriAB="https://mt.example.test/a|b";
				CredentialManager.Write(testCredentialStore.Prefix+"cm:"+uriAB+"|c","c","synthetic-password-old");
				FakeOldVault fakeOldVault=new FakeOldVault();
				fakeOldVault.Add(testCredentialStore.VaultResource(URI),USER,"synthetic-password-older");
				fakeOldVault.Add(testCredentialStore.VaultResource(uriAB),"c","synthetic-password-older");
				CredentialStore credentialStore=StartWithVault(testCredentialStore,fakeOldVault);
				Assert.AreEqual("synthetic-password-newer",credentialStore.RetrievePassword(URI,USER));
				StringAssert.Contains(credentialStore.GetMigrationRecord(),"Copied 0, already there 2, failed 0");
				Assert.AreEqual(0,fakeOldVault.ListResourcesRead.Count,"No vault password is read for a login already there.");
			});
			//The check and the copy read no secret of the Credential Manager.
			string source=TestPaths.ReadSource("PasswordVaultWrapper","CredentialStore.cs");
			int indexStart=source.IndexOf("private bool LoginExists(",StringComparison.Ordinal);
			int indexEnd=source.IndexOf("public string GetMigrationRecord(",StringComparison.Ordinal);
			Assert.IsTrue(indexStart>0 && indexEnd>indexStart);
			Assert.IsFalse(source.Substring(indexStart,indexEnd-indexStart).Contains("CredentialManager.Read("));
			indexStart=source.IndexOf("internal MigrationResult MigrateFromVault(",StringComparison.Ordinal);
			Assert.IsTrue(indexStart>0);
			Assert.IsFalse(source.Substring(indexStart).Contains("CredentialManager.Read("));
		}

		[TestMethod]
		public void IsOsWithoutVault_IsFalseHere() {
			//The tests run on Windows 10 or 11; a mistaken "no vault" here would record every migration as done with nothing copied.
			Assert.IsFalse(OldPasswordVault.IsOsWithoutVault(),Environment.OSVersion.VersionString);
		}

		[TestMethod]
		public void TheProgram_UsesItsOwnNames_ThroughTheSamePublicInterface() {
			//The names are checked without calling the public methods, which would run the program's real migration on this account.
			Assert.AreEqual("OpenPmsDental Middle Tier:",CredentialStore.Default.TargetPrefix);
			Assert.AreEqual("OpenDental Middle Tier:",CredentialStore.Default.VaultResourcePrefix,"The resource name the program wrote to PasswordVault.");
			Assert.AreEqual("OpenPmsDental Middle Tier migration from PasswordVault",CredentialStore.Default.MigrationRecordTarget);
			Type type=typeof(WindowsPasswordVaultWrapper);
			Assert.IsTrue(type.IsPublic);
			string[] arraySignatures=type.GetMethods(BindingFlags.Public|BindingFlags.Static|BindingFlags.DeclaredOnly)
				.Select(x => x.ReturnType.Name+" "+x.Name+"("+string.Join(",",x.GetParameters().Select(y => y.ParameterType.Name))+")")
				.OrderBy(x => x,StringComparer.Ordinal).ToArray();
			CollectionAssert.AreEqual(new[] { "Boolean TryRetrieveUserName(String,String&)","String RetrievePassword(String,String)",
				"Void ClearCredentials(String)","Void WritePassword(String,String,String)" },arraySignatures,string.Join("; ",arraySignatures));
			string source=TestPaths.ReadSource("PasswordVaultWrapper","WindowsPasswordVaultWrapper.cs");
			foreach(string call in new[] { "CredentialStore.Default.ClearCredentials(uri)","CredentialStore.Default.WritePassword(uri,username,password)",
				"CredentialStore.Default.TryRetrieveUserName(uri,out username)","CredentialStore.Default.RetrievePassword(uri,username)" })
			{
				StringAssert.Contains(source,call);
			}
		}

		[TestMethod]
		public void TheWrapper_NeedsNoWindowsRuntimeMetadata() {
			//Windows.winmd is gone: no reference to Windows Runtime metadata, no WinRT using, nothing next to the test.
			Assembly assembly=typeof(WindowsPasswordVaultWrapper).Assembly;
			List<string> listWinRt=assembly.GetReferencedAssemblies()
				.Where(x => x.ContentType==AssemblyContentType.WindowsRuntime || x.Name.StartsWith("Windows",StringComparison.OrdinalIgnoreCase))
				.Select(x => x.FullName).ToList();
			Assert.AreEqual(0,listWinRt.Count,string.Join("; ",listWinRt));
			Assert.IsFalse(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Windows.winmd")));
			foreach(string path in TestPaths.GetFiles(Path.Combine(TestPaths.RepoRoot,"PasswordVaultWrapper"),".cs")) {
				Assert.IsFalse(File.ReadAllText(path).Contains("using Windows."),path);
			}
		}
	}
}
