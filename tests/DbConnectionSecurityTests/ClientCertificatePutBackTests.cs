// Undoing an import puts the store entry's earlier key back (DIRECT-CERT-FOLLOWUP round 4, part B, the owner's answer (b) of 2026-11-13):
// when the computer's store already held the client certificate and the tool's import gave its entry the tool's key, leaving the entry's
// earlier key aside (a key in another key set, or a usable key the tool did not create), a save that fails after the import, and a later save
// that stops using the certificate, write the entry's earlier key provider information back onto it and delete only the tool's key: the entry
// stays, as it was. Through DbSettingsRunner.Apply with the test store (ClientCertificateTestStore, StoreNameForTests) and throwaway keys of
// the current user only; never LocalMachine\My, CurrentUser\My or a real certificate. Licensed under the GNU General Public License, version
// 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalDbSettings;

namespace DbConnectionSecurityTests {
	///<summary>The store entry's earlier key in a put-back case.</summary>
	public enum EarlierKey {
		///<summary>A key in another key set than the import's (the test makes the entry name the machine key set).</summary>
		OtherKeySet,
		///<summary>A usable key the tool did not create.</summary>
		Foreign,
		///<summary>A smart-card CSP's key container of the user key set that is not there (a smart card that is not inserted): it does not open,
		///and the test's seam refuses to open it, so that no smart-card CSP is ever asked for a card.</summary>
		SmartCardAbsent,
		///<summary>A software CAPI provider's key container of the user key set that does not exist (a deleted key file).</summary>
		SoftwareAbsent,
	}

	[TestClass]
	public class ClientCertificatePutBackTests {
		private static readonly TestKeyKind[] _arrayKinds=new[] { TestKeyKind.CapiRsa,TestKeyKind.CngRsa,TestKeyKind.CngEcdsa };
		///<summary>A smart-card CSP's name, as a certificate of a smart card names its key (the Direct tests' too); no such key is ever opened.
		///</summary>
		private const string PROVIDER_SMART_CARD="Microsoft Base Smart Card Crypto Provider";
		///<summary>PROV_RSA_FULL, the smart-card CSP's type.</summary>
		private const uint PROV_RSA_FULL=1;
		private const string PROVIDER_CAPI="Microsoft Enhanced RSA and AES Cryptographic Provider";
		///<summary>PROV_RSA_AES.</summary>
		private const uint PROV_RSA_AES=24;
		private string _folder;

		[TestInitialize]
		public void SetUp() {
			_folder=TestHelpers.NewTempFolder();
		}

		[TestCleanup]
		public void CleanUp() {
			ClientCertificateStore.ActionAfterKeysStoredForTests=null;
			ClientCertificateStore.ActionInsteadOfKeyProvInfoPutBackForTests=null;
			ClientCertificateStore.ActionBeforeKeyNamedDeleteForTests=null;
			ClientCertificateStore.ActionBeforePrivateKeyOpenForTests=null;
			ClientCertificateStore.StoreNameForTests=null;
			TestHelpers.DeleteFolder(_folder);
			ClientCertificateTestStore.AssertCleanedUp();
		}

		///<summary>Part B: the store holds the client certificate, put there another way, with an earlier key: (a) its entry names that key in
		///another key set than the import's (the test makes the test store's entry name the machine key set), or (b) a usable key the tool did not
		///create. The tool's import gives the entry its own key. Then (1) the save fails (FreeDentalConfig.xml held open), or (2) the save succeeds
		///and a later save with the password stops using the certificate. Either way the entry stays, names its earlier key again, exactly as it
		///did (container, provider, type, flags, key spec), the earlier key and its file are untouched, the tool's key and its file are gone, no
		///record is left, and the message says what was put back. The import's warning names the earlier key; the error of a failed save whose
		///undo put it back leaves that sentence (with its certutil -delkey command) out, since the entry uses that key again (the fix round's
		///review finding 4). Before round 4 the whole entry was removed. For each kind of key.</summary>
		[TestMethod]
		public void Apply_UndoingAnImportOverAnEntryWithAnEarlierKey_PutsThatKeyBack() {
			foreach(TestKeyKind testKeyKind in _arrayKinds) {
				foreach(EarlierKey earlierKey in new[] { EarlierKey.OtherKeySet,EarlierKey.Foreign }) {
					foreach(bool isFailedSave in new[] { true,false }) {
						string label=testKeyKind+", "+earlierKey+(isFailedSave ? ", a failed save" : ", a later save with the password");
						string folder=Path.Combine(_folder,testKeyKind+"-"+earlierKey+(isFailedSave ? "-failed" : "-later"));
						AssertPutBack(testKeyKind,earlierKey,isFailedSave,folder,label);
					}
				}
			}
		}

		///<summary>The fix round's review findings 1 and 3: an entry whose key provider information names a key in the import's key set (the user
		///key set of the test store) that does not open and that the tool did not create: (a) a smart-card CSP's key container that no CSP has (a
		///smart card that is not inserted; the test's seam refuses to open it, so that no CSP asks for a card), (b) a software CAPI provider's
		///container that does not exist (a deleted key file). The import gives the entry its own key and its warning names the earlier key; a
		///failed save, or a later save with the password, puts that key provider information back: the entry stays, names that key again
		///exactly, the tool's key and its file are gone, and no record is left. Before the fix both removed the whole entry, and the import gave
		///no warning. For each kind of key of the file.</summary>
		[TestMethod]
		public void Apply_UndoingAnImportOverAnEntryWhoseKeyDoesNotOpen_PutsThatKeyBack() {
			foreach(TestKeyKind testKeyKind in _arrayKinds) {
				foreach(EarlierKey earlierKey in new[] { EarlierKey.SmartCardAbsent,EarlierKey.SoftwareAbsent }) {
					foreach(bool isFailedSave in new[] { true,false }) {
						string label=testKeyKind+", "+earlierKey+(isFailedSave ? ", a failed save" : ", a later save with the password");
						string folder=Path.Combine(_folder,testKeyKind+"-"+earlierKey+(isFailedSave ? "-failed" : "-later"));
						AssertPutBack(testKeyKind,earlierKey,isFailedSave,folder,label);
					}
				}
			}
		}

		///<summary>The fix round's review finding 2: the put-back succeeds, then the delete of the tool's key fails (a seam throws once the key has
		///opened, as a key that cannot be deleted). The message says that the entry names its earlier key again and that the tool's key could not
		///be deleted, why, and where its file is; never that it was deleted. The tool's key and its file stay (the store's clean-up deletes them),
		///and the entry names its earlier key. Over a usable key the tool did not create, for a failed save and a later save, for each kind of key.
		///</summary>
		[TestMethod]
		public void Apply_APutBackWhoseToolKeyDeleteFails_SaysTheKeyIsLeft() {
			foreach(TestKeyKind testKeyKind in _arrayKinds) {
				foreach(bool isFailedSave in new[] { true,false }) {
					string label=testKeyKind+(isFailedSave ? ", a failed save" : ", a later save with the password")+", the delete fails";
					string folder=Path.Combine(_folder,testKeyKind+"-deletefails"+(isFailedSave ? "-failed" : "-later"));
					AssertPutBack(testKeyKind,EarlierKey.Foreign,isFailedSave,folder,label,isToolKeyDeleteFailing:true);
				}
			}
		}

		///<summary>Part B, a put-back that fails (a seam stands in for the write: it throws, as a store that refuses it, or does nothing, as one that
		///drops it without a word): the entry still names the tool's key, so that key stays (the certificate keeps working), and so do the entry
		///and the earlier key; the message says what could not be put back, that both keys and the entry were left, and how to put it right with
		///the certutil command that deletes the tool's key. For each kind of key, over a usable key the tool did not create.</summary>
		[TestMethod]
		public void Apply_APutBackThatFails_LeavesTheToolsKeyWhileTheEntryStillNamesIt() {
			foreach(TestKeyKind testKeyKind in _arrayKinds) {
				foreach(bool isThrown in new[] { true,false }) {
					string label=testKeyKind+(isThrown ? ", refused" : ", dropped");
					string folder=Path.Combine(_folder,testKeyKind+(isThrown ? "-refused" : "-dropped"));
					Directory.CreateDirectory(folder);
					string config=TestHelpers.WriteConfig(folder,"ComputerName","localhost","Database","opendental","User","root","Password","");
					string caCopy=DbConnectionConfig.GetOfficeCaCopyPath(Path.Combine(folder,"program"));
					using(ClientCertificateTestStore clientCertificateTestStore=new ClientCertificateTestStore()) {
						string pathPfx=clientCertificateTestStore.WritePfx(folder,testKeyKind,out string thumbprintClient);
						TestKey testKeyFirst=clientCertificateTestStore.AddCertificate(pathPfx);
						TestKeyProvInfo testKeyProvInfoFirst;
						using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
							testKeyProvInfoFirst=TestKeyProvInfo.Read(x509Certificate2Entry);
						}
						Dictionary<string,TestKey> dictKeys=null;
						ClientCertificateStore.ActionAfterKeysStoredForTests=x509Certificate2Collection => {
							dictKeys=clientCertificateTestStore.TrackKeys(x509Certificate2Collection);
						};
						ClientCertificateStore.StoreNameForTests=clientCertificateTestStore.Name;
						DbSettingsResult dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Certificate,PfxFile=pathPfx,
							PfxPassword=clientCertificateTestStore.Password },config,caCopy);
						Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
						TestKey testKeyTool=dictKeys[thumbprintClient];
						TestKeyProvInfo testKeyProvInfoTool;
						using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
							testKeyProvInfoTool=TestKeyProvInfo.Read(x509Certificate2Entry);
						}
						ClientCertificateStore.ActionInsteadOfKeyProvInfoPutBackForTests=x509Certificate2 => {
							if(isThrown) {
								throw new CryptographicException("The test's store refuses the put-back.");
							}
						};
						dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Password,DbPassword="" },config,caCopy);
						ClientCertificateStore.ActionInsteadOfKeyProvInfoPutBackForTests=null;
						ClientCertificateStore.ActionAfterKeysStoredForTests=null;
						ClientCertificateStore.StoreNameForTests=null;
						Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
						string why=isThrown ? "The test's store refuses the put-back." : "Windows reported no error, but the entry does not name it afterwards.";
						StringAssert.Contains(dbSettingsResult.Message,"before the tool's import gave it a private key, but its earlier private key (provider "
							+testKeyProvInfoFirst.ProviderName+", key container "+testKeyProvInfoFirst.ContainerName+", in a user key set) could not be named on its entry again: "+why
							+" The entry still names the private key the tool gave it (",label);
						StringAssert.Contains(dbSettingsResult.Message,"so the tool left both keys and the entry as they are. To put the entry back as it "
							+"was, as an administrator, delete the tool's key (certutil -user -csp \""+testKeyProvInfoTool.ProviderName+"\" -delkey \""+testKeyProvInfoTool.ContainerName
							+"\"), then import the certificate with its earlier key again.",label);
						CollectionAssert.AreEqual(new List<string> { thumbprintClient },clientCertificateTestStore.GetThumbprints(),label+": the entry stays.");
						using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
							Assert.IsTrue(string.Equals(TestKeyProvInfo.Read(x509Certificate2Entry).ContainerName,testKeyTool.Name,StringComparison.OrdinalIgnoreCase),
								label+": it still names the tool's key.");
						}
						Assert.IsTrue(testKeyTool.Exists(),label+": the tool's key stays, while the entry names it (the store's clean-up deletes it).");
						Assert.IsTrue(File.Exists(testKeyTool.FilePath),label+": and its file.");
						Assert.IsTrue(testKeyFirst.Exists(),label+": the earlier key is untouched.");
						Assert.IsTrue(clientCertificateTestStore.CanSign(thumbprintClient),label+": the certificate still works.");
					}
				}
			}
		}

		///<summary>True when a CAPI key container of the current user with this name opens through the software CAPI provider (opened only to
		///know; a container that is not there is not made).</summary>
		private static bool IsCapiContainerThere(string containerName) {
			CspParameters cspParameters=new CspParameters((int)PROV_RSA_AES,PROVIDER_CAPI,containerName);
			cspParameters.Flags=CspProviderFlags.UseExistingKey|CspProviderFlags.NoPrompt;
			try {
				using(new RSACryptoServiceProvider(cspParameters)) {
				}
				return true;
			}
			catch(CryptographicException) {
				return false;//"Keyset does not exist".
			}
		}

		private static void AssertPutBack(TestKeyKind testKeyKind,EarlierKey earlierKey,bool isFailedSave,string folder,string label,
			bool isToolKeyDeleteFailing=false)
		{
			bool isOtherKeySet=earlierKey==EarlierKey.OtherKeySet;
			bool isAbsent=earlierKey==EarlierKey.SmartCardAbsent || earlierKey==EarlierKey.SoftwareAbsent;
			Directory.CreateDirectory(folder);
			string config=TestHelpers.WriteConfig(folder,"ComputerName","localhost","Database","opendental","User","root","Password","");
			string caCopy=DbConnectionConfig.GetOfficeCaCopyPath(Path.Combine(folder,"program"));
			string pathRecord=ImportedCertificateRecord.GetPath(caCopy);
			using(ClientCertificateTestStore clientCertificateTestStore=new ClientCertificateTestStore()) {
				string pathPfx=clientCertificateTestStore.WritePfx(folder,testKeyKind,out string thumbprintClient);
				TestKey testKeyFirst=clientCertificateTestStore.AddCertificate(pathPfx);//Put there another way, not by the tool.
				TestKeyProvInfo testKeyProvInfoBefore;
				if(isOtherKeySet) {
					testKeyProvInfoBefore=clientCertificateTestStore.NameMachineKeySetForEntry(thumbprintClient);
				}
				else if(earlierKey==EarlierKey.SmartCardAbsent) {
					testKeyProvInfoBefore=clientCertificateTestStore.NameAbsentKeyForEntry(thumbprintClient,PROVIDER_SMART_CARD,PROV_RSA_FULL);
					ClientCertificateStore.ActionBeforePrivateKeyOpenForTests=x509Certificate2 => {
						if(TestKeyProvInfo.Read(x509Certificate2).ProviderName==PROVIDER_SMART_CARD) {
							throw new CryptographicException("The test refuses to open the smart-card key (no card is inserted).");
						}
					};
				}
				else if(earlierKey==EarlierKey.SoftwareAbsent) {
					testKeyProvInfoBefore=clientCertificateTestStore.NameAbsentKeyForEntry(thumbprintClient,PROVIDER_CAPI,PROV_RSA_AES);
					Assert.IsFalse(IsCapiContainerThere(testKeyProvInfoBefore.ContainerName),label+": the test's premise, the container does not exist.");
				}
				else {
					Assert.IsTrue(clientCertificateTestStore.CanSign(thumbprintClient),label+": the test's premise, the entry's key is usable.");
					using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
						testKeyProvInfoBefore=TestKeyProvInfo.Read(x509Certificate2Entry);
					}
				}
				Dictionary<string,TestKey> dictKeys=null;
				ClientCertificateStore.ActionAfterKeysStoredForTests=x509Certificate2Collection => {
					dictKeys=clientCertificateTestStore.TrackKeys(x509Certificate2Collection);
				};
				ClientCertificateStore.StoreNameForTests=clientCertificateTestStore.Name;
				string toolKeyDeleteRefused="The test's tool key refuses to be deleted.";
				if(isToolKeyDeleteFailing) {
					ClientCertificateStore.ActionBeforeKeyNamedDeleteForTests=keyProvInfo => {
						throw new CryptographicException(toolKeyDeleteRefused);
					};
				}
				//The sentence of the import's warning that names the entry's earlier key.
				string warningEntryKey=isAbsent ? "but the private key its entry names does not open for this tool" : "left the entry's earlier key untouched";
				DbSettingsResult dbSettingsResult;
				if(isFailedSave) {
					//Held open as a program that reads it would: others may read it, not replace it.
					using(new FileStream(config,FileMode.Open,FileAccess.Read,FileShare.Read)) {
						dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Certificate,PfxFile=pathPfx,
							PfxPassword=clientCertificateTestStore.Password },config,caCopy);
					}
					Assert.AreEqual(DbSettingsRunner.EXIT_WRITE_FAILED,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
					StringAssert.StartsWith(dbSettingsResult.Message,"The connection settings could not be saved: ",label);
					//Finding 4: the entry uses its earlier key again, so the import's sentence about it (and its delete command) is left out.
					Assert.IsFalse(dbSettingsResult.Message.Contains(warningEntryKey),label+": "+dbSettingsResult.Message);
					Assert.IsFalse(dbSettingsResult.Message.Contains("-delkey \""+testKeyProvInfoBefore.ContainerName),label+": "+dbSettingsResult.Message);
				}
				else {
					dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Certificate,PfxFile=pathPfx,
						PfxPassword=clientCertificateTestStore.Password },config,caCopy);
					Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
					StringAssert.Contains(dbSettingsResult.Message,warningEntryKey,label+": the import's warning names the entry's earlier key.");
					StringAssert.Contains(dbSettingsResult.Message,"key container "+testKeyProvInfoBefore.ContainerName,label);
					Assert.AreEqual(thumbprintClient,ImportedCertificateRecord.Read(pathRecord),label+": the import is recorded.");
					using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
						Assert.IsFalse(TestKeyProvInfo.Read(x509Certificate2Entry).IsSameAs(testKeyProvInfoBefore),label+": the entry took the import's key.");
					}
					dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Password,DbPassword="" },config,caCopy);
					Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
				}
				ClientCertificateStore.ActionAfterKeysStoredForTests=null;
				ClientCertificateStore.ActionBeforeKeyNamedDeleteForTests=null;
				ClientCertificateStore.ActionBeforePrivateKeyOpenForTests=null;
				ClientCertificateStore.StoreNameForTests=null;
				Assert.IsNotNull(dictKeys,label+": the import stored its key.");
				TestKey testKeyTool=dictKeys[thumbprintClient];
				string storeText="CurrentUser\\"+clientCertificateTestStore.Name;
				string putBack=" The client certificate "+thumbprintClient+" was in the computer's certificate store ("+storeText+") before the tool's "
					+"import gave it a private key: its entry names its earlier private key again (provider "+testKeyProvInfoBefore.ProviderName+", key "
					+"container "+testKeyProvInfoBefore.ContainerName+", in "+(isOtherKeySet ? "the computer's key set" : "a user key set")+"), as before "
					+"that import";
				if(isToolKeyDeleteFailing) {
					StringAssert.Contains(dbSettingsResult.Message,putBack+". The private key the tool gave it (provider ",label);
					//Windows may spell a key's names in another case than the test's view of the key: compared without case.
					string messageLower=dbSettingsResult.Message.ToLowerInvariant();
					StringAssert.Contains(messageLower,("key container "+testKeyTool.Name+", in a user key set) could not be deleted: "+toolKeyDeleteRefused
						+" The key is left on this computer in "+testKeyTool.FilePath+": delete that file as an administrator.").ToLowerInvariant(),label+": "
						+dbSettingsResult.Message);
					Assert.IsFalse(dbSettingsResult.Message.Contains("was deleted"),label+": "+dbSettingsResult.Message);
					Assert.IsTrue(testKeyTool.Exists(),label+": the tool's key stays (the store's clean-up deletes it).");
					Assert.IsTrue(File.Exists(testKeyTool.FilePath),label+": and its file: "+testKeyTool.FilePath);
				}
				else {
					StringAssert.EndsWith(dbSettingsResult.Message,putBack+", and the private key the tool gave it was deleted."
						+ClientCertificateRecordTests.GetPutBackHedge(storeText,thumbprintClient),label);
					Assert.IsFalse(testKeyTool.Exists(),label+": the tool's key is gone.");
					Assert.IsFalse(File.Exists(testKeyTool.FilePath),label+": its file is gone: "+testKeyTool.FilePath);
				}
				CollectionAssert.AreEqual(new List<string> { thumbprintClient },clientCertificateTestStore.GetThumbprints(),label+": the entry stays.");
				using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
					TestKeyProvInfo testKeyProvInfoNow=TestKeyProvInfo.Read(x509Certificate2Entry);
					Assert.IsTrue(testKeyProvInfoNow.IsSameAs(testKeyProvInfoBefore),label+": the entry names its earlier key again, exactly: "
						+testKeyProvInfoNow.ContainerName+", "+testKeyProvInfoNow.ProviderName+", "+testKeyProvInfoNow.Flags);
				}
				if(earlierKey==EarlierKey.Foreign) {
					Assert.IsTrue(clientCertificateTestStore.CanSign(thumbprintClient),label+": and signs with it.");
				}
				if(earlierKey==EarlierKey.SoftwareAbsent) {
					Assert.IsFalse(IsCapiContainerThere(testKeyProvInfoBefore.ContainerName),label+": the tool made no container of the earlier key's name.");
				}
				//The key the test first stored for the entry: the earlier key itself, or, for an entry the test made name another key, the key that
				//name stands for, or a key no certificate names any more; the tool never touches it either way.
				Assert.IsTrue(testKeyFirst.Exists(),label+": the earlier key is untouched (the store's clean-up deletes it).");
				Assert.IsTrue(File.Exists(testKeyFirst.FilePath),label+": and its file: "+testKeyFirst.FilePath);
				Assert.AreEqual("",ImportedCertificateRecord.Read(pathRecord),label+": no record is left.");
				Assert.AreEqual("",DbConnectionConfig.ReadFile(config).ClientCertThumbprint,label+": the settings use no certificate.");
			}
		}
	}
}
