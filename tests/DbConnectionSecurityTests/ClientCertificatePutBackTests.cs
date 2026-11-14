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
	[TestClass]
	public class ClientCertificatePutBackTests {
		private static readonly TestKeyKind[] _arrayKinds=new[] { TestKeyKind.CapiRsa,TestKeyKind.CngRsa,TestKeyKind.CngEcdsa };
		private string _folder;

		[TestInitialize]
		public void SetUp() {
			_folder=TestHelpers.NewTempFolder();
		}

		[TestCleanup]
		public void CleanUp() {
			ClientCertificateStore.ActionAfterKeysStoredForTests=null;
			ClientCertificateStore.StoreNameForTests=null;
			TestHelpers.DeleteFolder(_folder);
			ClientCertificateTestStore.AssertCleanedUp();
		}

		///<summary>Part B: the store holds the client certificate, put there another way, with an earlier key: (a) its entry names that key in
		///another key set than the import's (the test makes the test store's entry name the machine key set), or (b) a usable key the tool did not
		///create. The tool's import gives the entry its own key. Then (1) the save fails (FreeDentalConfig.xml held open), or (2) the save succeeds
		///and a later save with the password stops using the certificate. Either way the entry stays, names its earlier key again, exactly as it
		///did (container, provider, type, flags, key spec), the earlier key and its file are untouched, the tool's key and its file are gone, no
		///record is left, and the message says what was put back. Before round 4 the whole entry was removed. For each kind of key.</summary>
		[TestMethod]
		public void Apply_UndoingAnImportOverAnEntryWithAnEarlierKey_PutsThatKeyBack() {
			foreach(TestKeyKind testKeyKind in _arrayKinds) {
				foreach(bool isOtherKeySet in new[] { true,false }) {
					foreach(bool isFailedSave in new[] { true,false }) {
						string label=testKeyKind+(isOtherKeySet ? ", another key set" : ", a usable key the tool did not create")
							+(isFailedSave ? ", a failed save" : ", a later save with the password");
						string folder=Path.Combine(_folder,testKeyKind+(isOtherKeySet ? "-keyset" : "-foreign")+(isFailedSave ? "-failed" : "-later"));
						AssertPutBack(testKeyKind,isOtherKeySet,isFailedSave,folder,label);
					}
				}
			}
		}

		private static void AssertPutBack(TestKeyKind testKeyKind,bool isOtherKeySet,bool isFailedSave,string folder,string label) {
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
				DbSettingsResult dbSettingsResult;
				if(isFailedSave) {
					//Held open as a program that reads it would: others may read it, not replace it.
					using(new FileStream(config,FileMode.Open,FileAccess.Read,FileShare.Read)) {
						dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Certificate,PfxFile=pathPfx,
							PfxPassword=clientCertificateTestStore.Password },config,caCopy);
					}
					Assert.AreEqual(DbSettingsRunner.EXIT_WRITE_FAILED,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
					StringAssert.StartsWith(dbSettingsResult.Message,"The connection settings could not be saved: ",label);
				}
				else {
					dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Certificate,PfxFile=pathPfx,
						PfxPassword=clientCertificateTestStore.Password },config,caCopy);
					Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
					Assert.AreEqual(thumbprintClient,ImportedCertificateRecord.Read(pathRecord),label+": the import is recorded.");
					using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
						Assert.IsFalse(TestKeyProvInfo.Read(x509Certificate2Entry).IsSameAs(testKeyProvInfoBefore),label+": the entry took the import's key.");
					}
					dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Password,DbPassword="" },config,caCopy);
					Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
				}
				ClientCertificateStore.ActionAfterKeysStoredForTests=null;
				ClientCertificateStore.StoreNameForTests=null;
				Assert.IsNotNull(dictKeys,label+": the import stored its key.");
				TestKey testKeyTool=dictKeys[thumbprintClient];
				string storeText="CurrentUser\\"+clientCertificateTestStore.Name;
				StringAssert.EndsWith(dbSettingsResult.Message," The client certificate "+thumbprintClient+" was in the computer's certificate store ("
					+storeText+") before the tool's import gave it a private key: its entry names its earlier private key again (provider "
					+testKeyProvInfoBefore.ProviderName+", key container "+testKeyProvInfoBefore.ContainerName+", in "
					+(isOtherKeySet ? "the computer's key set" : "a user key set")+"), as before that import, and the private key the tool gave it was "
					+"deleted.",label);
				CollectionAssert.AreEqual(new List<string> { thumbprintClient },clientCertificateTestStore.GetThumbprints(),label+": the entry stays.");
				using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
					TestKeyProvInfo testKeyProvInfoNow=TestKeyProvInfo.Read(x509Certificate2Entry);
					Assert.IsTrue(testKeyProvInfoNow.IsSameAs(testKeyProvInfoBefore),label+": the entry names its earlier key again, exactly: "
						+testKeyProvInfoNow.ContainerName+", "+testKeyProvInfoNow.ProviderName+", "+testKeyProvInfoNow.Flags);
				}
				if(!isOtherKeySet) {
					Assert.IsTrue(clientCertificateTestStore.CanSign(thumbprintClient),label+": and signs with it.");
				}
				Assert.IsTrue(testKeyFirst.Exists(),label+": the earlier key is untouched (the store's clean-up deletes it).");
				Assert.IsTrue(File.Exists(testKeyFirst.FilePath),label+": and its file: "+testKeyFirst.FilePath);
				Assert.IsFalse(testKeyTool.Exists(),label+": the tool's key is gone.");
				Assert.IsFalse(File.Exists(testKeyTool.FilePath),label+": its file is gone: "+testKeyTool.FilePath);
				Assert.AreEqual("",ImportedCertificateRecord.Read(pathRecord),label+": no record is left.");
				Assert.AreEqual("",DbConnectionConfig.ReadFile(config).ClientCertThumbprint,label+": the settings use no certificate.");
			}
		}
	}
}
