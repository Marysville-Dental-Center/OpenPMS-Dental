// Package S3a-5 (docs/specs/net10-plan.md, docs/port-notes/S3a-5.md): the saved Middle Tier logins in the Windows Credential Manager, and the
// one-time move of the old PasswordVault entries.
// SAFETY: these tests run on the developer's own Windows account. Every name they use starts with a per-run prefix
// "OpenPmsDentalTest:<guid>:" (Credential Manager targets, old-vault resources, the migration record). They create, read and delete only
// entries under that prefix, every listing is filtered by it before an entry is touched, the public static methods (which would run the
// program's real migration) are never called, and everything is removed in finally. All user names and passwords are synthetic.
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
	///<summary>A credential store of one test, with names under its own prefix, and the clean-up of everything under that prefix.</summary>
	internal sealed class TestCredentialStore : IDisposable {
		public readonly string Prefix="OpenPmsDentalTest:"+Guid.NewGuid().ToString("N")+":";
		public readonly CredentialStore Store;
		///<summary>The old-vault resources this test wrote; only these are looked up and removed.</summary>
		private readonly List<string> _listVaultResources=new List<string>();

		public TestCredentialStore() {
			Store=NewStore();
		}

		///<summary>Another store with the same names, as a later process of the program would have.</summary>
		public CredentialStore NewStore() {
			return new CredentialStore(Prefix+"cm:",Prefix+"vault:",Prefix+"record");
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

		public void Dispose() {
			//CredentialManager.List filters by the prefix (Windows' filter, then StartsWith again), so only this test's entries are deleted.
			foreach(CredentialEntry credentialEntry in CredentialManager.List(Prefix)) {
				Assert.IsTrue(credentialEntry.TargetName.StartsWith(Prefix,StringComparison.Ordinal));
				CredentialManager.Delete(credentialEntry.TargetName);
			}
			if(_listVaultResources.Count==0) {
				return;
			}
			OldPasswordVault oldPasswordVault=OldPasswordVault.TryOpen();
			foreach(string resource in _listVaultResources.Distinct()) {
				foreach(OldVaultEntry oldVaultEntry in oldPasswordVault.FindAllByResource(resource)) {
					if(oldVaultEntry.Resource.StartsWith(Prefix,StringComparison.Ordinal)) {
						oldPasswordVault.Remove(oldVaultEntry);
					}
				}
			}
		}
	}

	[TestClass]
	public class CredentialStoreTests {
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
			using(TestCredentialStore testCredentialStore=new TestCredentialStore()) {
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
			}
		}

		[TestMethod]
		public void Login_IsFoundForItsOwnAddressOnly() {
			using(TestCredentialStore testCredentialStore=new TestCredentialStore()) {
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
			}
		}

		[TestMethod]
		public void BlankUserOrPassword_Throws_AndNothingIsWritten() {
			using(TestCredentialStore testCredentialStore=new TestCredentialStore()) {
				CredentialStore credentialStore=testCredentialStore.Store;
				credentialStore.FuncOpenVault=() => null;
				Assert.ThrowsExactly<ArgumentException>(() => credentialStore.WritePassword(URI,USER,""));
				Assert.ThrowsExactly<ArgumentException>(() => credentialStore.WritePassword(URI,USER,null));
				Assert.ThrowsExactly<ArgumentException>(() => credentialStore.WritePassword(URI,"",PASSWORD));
				Assert.ThrowsExactly<ArgumentException>(() => credentialStore.WritePassword(URI,USER,new string('x',CredentialManager.MAX_SECRET_BYTES/2+1)));
				//Only the migration's record is there.
				CollectionAssert.AreEqual(new[] { testCredentialStore.Prefix+"record" },CredentialManager.List(testCredentialStore.Prefix).Select(x => x.TargetName).ToArray());
				credentialStore.WritePassword(URI,USER,new string('x',CredentialManager.MAX_SECRET_BYTES/2));
				Assert.AreEqual(CredentialManager.MAX_SECRET_BYTES/2,credentialStore.RetrievePassword(URI,USER).Length);
			}
		}

		[TestMethod]
		public void Migration_CopiesTheVaultEntries_OnFirstUse_AndRunsOnce() {
			OldPasswordVault oldPasswordVault=OpenVaultOrInconclusive();
			using(TestCredentialStore testCredentialStore=new TestCredentialStore()) {
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
			}
		}

		[TestMethod]
		public void Migration_NeverReplacesALoginAlreadyThere_AndCopiesOnlyItsOwnResourceName() {
			OldPasswordVault oldPasswordVault=OpenVaultOrInconclusive();
			using(TestCredentialStore testCredentialStore=new TestCredentialStore()) {
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
			}
		}

		[TestMethod]
		public void Clear_AlsoRemovesTheOldVaultEntriesOfTheAddress() {
			OldPasswordVault oldPasswordVault=OpenVaultOrInconclusive();
			using(TestCredentialStore testCredentialStore=new TestCredentialStore()) {
				string uriOther="https://mt2.example.test/ServiceMain.asmx";
				testCredentialStore.AddToVault(oldPasswordVault,testCredentialStore.VaultResource(URI),USER,PASSWORD);
				testCredentialStore.AddToVault(oldPasswordVault,testCredentialStore.VaultResource(uriOther),USER,PASSWORD);
				CredentialStore credentialStore=testCredentialStore.Store;
				credentialStore.ClearCredentials(URI);
				Assert.IsFalse(credentialStore.TryRetrieveUserName(URI,out string _));
				Assert.AreEqual(0,oldPasswordVault.FindAllByResource(testCredentialStore.VaultResource(URI)).Count,"A cleared login cannot come back.");
				Assert.AreEqual(1,oldPasswordVault.FindAllByResource(testCredentialStore.VaultResource(uriOther)).Count);
				Assert.AreEqual(PASSWORD,credentialStore.RetrievePassword(uriOther,USER));
			}
		}

		[TestMethod]
		public void Migration_WithoutAVault_RecordsThatThereWasNothingToRead() {
			using(TestCredentialStore testCredentialStore=new TestCredentialStore()) {
				CredentialStore credentialStore=testCredentialStore.Store;
				credentialStore.FuncOpenVault=() => null;
				credentialStore.WritePassword(URI,USER,PASSWORD);
				StringAssert.Contains(credentialStore.GetMigrationRecord(),"Copied 0, already there 0, failed 0, PasswordVault not available");
				Assert.AreEqual(PASSWORD,credentialStore.RetrievePassword(URI,USER));
			}
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
