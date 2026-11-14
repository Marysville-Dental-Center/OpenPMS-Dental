// The connection settings tool's record of the client certificate it imported (DIRECT-CERT-FOLLOWUP round 4, part C, the owner's answer of
// 2026-11-13): it names the key the tool's import gave the store's entry, a record of the old format (a thumbprint line only) is still read
// and is rewritten by the next save, a malformed one reads as none, and a recorded certificate whose entry someone gave another key by hand is
// not the tool's: its key gets no read access for the users and is never deleted. Through DbSettingsRunner.Apply with the test store
// (ClientCertificateTestStore, StoreNameForTests) and throwaway keys of the current user only; never LocalMachine\My, CurrentUser\My or a real
// certificate. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using DataConnectionBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalDbSettings;

namespace DbConnectionSecurityTests {
	[TestClass]
	public class ClientCertificateRecordTests {
		private static readonly TestKeyKind[] _arrayKinds=new[] { TestKeyKind.CapiRsa,TestKeyKind.CngRsa,TestKeyKind.CngEcdsa };
		private const string THUMB="00112233445566778899AABBCCDDEEFF00112233";
		private const string FORMAT_LINE="Open PMS - Dental imported client certificate, format 2";
		private string _folder;

		[TestInitialize]
		public void SetUp() {
			_folder=TestHelpers.NewTempFolder();
		}

		[TestCleanup]
		public void CleanUp() {
			ClientCertificateStore.ActionAfterKeysStoredForTests=null;
			ClientCertificateStore.ActionInsteadOfUsersReadGrantForTests=null;
			ClientCertificateStore.StoreNameForTests=null;
			TestHelpers.DeleteFolder(_folder);
			ClientCertificateTestStore.AssertCleanedUp();
		}

		///<summary>Part C: a record of format 2 is read, and every malformed one reads as none (a line of another name, a name twice, a key's lines
		///incomplete, a key set that contradicts the flags, an unknown escape, another format's first line). A record of the old format (one line
		///of 40 hex digits, spaces or colons allowed) is still read; one with another line is not.</summary>
		[TestMethod]
		public void Record_OfFormat2AndOfTheOldFormat_IsRead_AndAMalformedOne_ReadsAsNone() {
			string caCopy=DbConnectionConfig.GetOfficeCaCopyPath(Path.Combine(_folder,"program"));
			string pathRecord=ImportedCertificateRecord.GetPath(caCopy);
			ImportedCertificateRecord.Write(pathRecord,THUMB);//Made protected, as the tool makes it; the test then writes its text.
			string toolKey="tool.container=OpenPmsDbSecTest_c%25d\r\ntool.provider=Microsoft Software Key Storage Provider\r\ntool.providertype=0\r\n"
				+"tool.keyset=machine\r\ntool.flags=00000020\r\ntool.keyspec=1\r\n";
			string earlierKey="earlier.container=admin-key\r\nearlier.provider=Microsoft Enhanced RSA and AES Cryptographic Provider\r\n"
				+"earlier.providertype=24\r\nearlier.keyset=user\r\nearlier.flags=00000000\r\nearlier.keyspec=1\r\n";
			Dictionary<string,string> dictRead=new Dictionary<string,string>();
			dictRead["format 2, the thumbprint only"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\n";
			dictRead["format 2, with the tool's key"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\n"+toolKey;
			dictRead["format 2, with both keys and a provider parameter"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\n"+toolKey+earlierKey
				+"earlier.param=7,00000000,0A0B\r\n";
			dictRead["format 2, LF line ends"]=(FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\n"+toolKey).Replace("\r\n","\n");
			dictRead["the old format"]=THUMB+"\r\n";
			dictRead["the old format, as Windows shows a thumbprint"]=string.Join(" ",Enumerable.Range(0,20).Select(x => THUMB.Substring(x*2,2)))
				.ToLowerInvariant();
			foreach(KeyValuePair<string,string> keyValuePair in dictRead) {
				File.WriteAllText(pathRecord,keyValuePair.Value);
				Assert.AreEqual(THUMB,ImportedCertificateRecord.Read(pathRecord),keyValuePair.Key);
			}
			Dictionary<string,string> dictNone=new Dictionary<string,string>();
			dictNone["a line of another name"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\nsomething=else\r\n";
			dictNone["the thumbprint twice"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\nthumbprint="+THUMB+"\r\n";
			dictNone["no thumbprint"]=FORMAT_LINE+"\r\n"+toolKey;
			dictNone["a thumbprint of 39 digits"]=FORMAT_LINE+"\r\nthumbprint="+THUMB.Substring(1)+"\r\n";
			dictNone["the tool's key incomplete"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\n"+toolKey.Replace("tool.keyspec=1\r\n","");
			dictNone["a key set that contradicts the flags"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\n"+toolKey.Replace("tool.flags=00000020","tool.flags=00000000");
			dictNone["an unknown key set"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\n"+toolKey.Replace("keyset=machine","keyset=both");
			dictNone["an unknown escape"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\n"+toolKey.Replace("%25","%41");
			dictNone["the tool's key without a container"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\n"+toolKey.Replace("OpenPmsDbSecTest_c%25d","");
			dictNone["a provider parameter without its key"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\nearlier.param=7,00000000,0A0B\r\n";
			dictNone["another format"]=FORMAT_LINE.Replace("format 2","format 3")+"\r\nthumbprint="+THUMB+"\r\n";
			dictNone["a line without a name"]=FORMAT_LINE+"\r\nthumbprint="+THUMB+"\r\n"+THUMB+"\r\n";
			dictNone["the old format with another line"]=THUMB+"\r\n"+THUMB+"\r\n";
			dictNone["the old format with other text"]="thumbprint "+THUMB+"\r\n";
			dictNone["empty"]="";
			foreach(KeyValuePair<string,string> keyValuePair in dictNone) {
				File.WriteAllText(pathRecord,keyValuePair.Value);
				Assert.AreEqual("",ImportedCertificateRecord.Read(pathRecord),keyValuePair.Key);
			}
		}

		///<summary>Part C: a record of the old format (the test writes the thumbprint line over the record an import wrote) is read: a save that
		///keeps the certificate rewrites it in format 2, naming the key the store's entry has (the key the tool's import made), read without
		///opening it; and a save with the password then removes the certificate with that key, as before. Before round 4 the save wrote the
		///thumbprint line again. For each kind of key.</summary>
		[TestMethod]
		public void Apply_ARecordOfTheOldFormat_IsRead_AndRewrittenInFormat2ByTheNextSave() {
			foreach(TestKeyKind testKeyKind in _arrayKinds) {
				string label=testKeyKind.ToString();
				string folder=Path.Combine(_folder,testKeyKind+"-old-format");
				Directory.CreateDirectory(folder);
				string config=TestHelpers.WriteConfig(folder,"ComputerName","localhost","Database","opendental","User","root","Password","");
				string caCopy=DbConnectionConfig.GetOfficeCaCopyPath(Path.Combine(folder,"program"));
				string pathRecord=ImportedCertificateRecord.GetPath(caCopy);
				using(ClientCertificateTestStore clientCertificateTestStore=new ClientCertificateTestStore()) {
					string pathPfx=clientCertificateTestStore.WritePfx(folder,testKeyKind,out string thumbprintClient);
					Dictionary<string,TestKey> dictKeys=null;
					ClientCertificateStore.ActionAfterKeysStoredForTests=x509Certificate2Collection => {
						dictKeys=clientCertificateTestStore.TrackKeys(x509Certificate2Collection);
					};
					ClientCertificateStore.StoreNameForTests=clientCertificateTestStore.Name;
					DbSettingsResult dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Certificate,PfxFile=pathPfx,
						PfxPassword=clientCertificateTestStore.Password },config,caCopy);
					ClientCertificateStore.ActionAfterKeysStoredForTests=null;
					Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
					TestKey testKeyImport=dictKeys[thumbprintClient];
					File.WriteAllText(pathRecord,thumbprintClient+"\r\n");//The old format.
					Assert.AreEqual(thumbprintClient,ImportedCertificateRecord.Read(pathRecord),label+": the old format is read.");
					//A save that keeps the certificate.
					dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest(),config,caCopy);
					Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
					Assert.AreEqual("Saved the connection settings for the database opendental on localhost (client certificate).",dbSettingsResult.Message,
						label);
					List<string> listLines=File.ReadAllLines(pathRecord).ToList();
					Assert.AreEqual(FORMAT_LINE,listLines[0],label+": rewritten in format 2.");
					CollectionAssert.Contains(listLines,"thumbprint="+thumbprintClient,label);
					Assert.IsTrue(listLines.Any(x => string.Equals(x,"tool.container="+testKeyImport.Name,StringComparison.OrdinalIgnoreCase)),
						label+": it names the key the store's entry has, the one the tool's import made: "+string.Join(" | ",listLines));
					CollectionAssert.Contains(listLines,"tool.keyset=user",label+": in the test store's key set.");
					Assert.IsFalse(listLines.Any(x => x.StartsWith("earlier.")),label+": no earlier key.");
					Assert.AreEqual(thumbprintClient,ImportedCertificateRecord.Read(pathRecord),label);
					//A save with the password removes it with that key.
					dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Password,DbPassword="" },config,caCopy);
					ClientCertificateStore.StoreNameForTests=null;
					Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
					Assert.AreEqual("Saved the connection settings for the database opendental on localhost (password).",dbSettingsResult.Message,label);
					Assert.IsFalse(File.Exists(pathRecord),label+": no record is left.");
					Assert.AreEqual(0,clientCertificateTestStore.GetThumbprints().Count,label+": the certificate is removed.");
					Assert.IsFalse(testKeyImport.Exists(),label+": with the key the tool made.");
					Assert.IsFalse(File.Exists(testKeyImport.FilePath),label+": its file is gone.");
				}
			}
		}

		///<summary>Part C: the tool imports a certificate (recorded, with its key), then someone gives its store entry another key by hand (the test
		///store adds the same .pfx again, so the entry names a key the tool did not make). (1) The same .pfx imported again with the tool: the
		///entry's key is not the tool's, so the grant seam never sees it; the entry takes the new import's key, the warning names the entry's
		///earlier key as given by hand, that key and its file stay, and the key the tool made first, which no certificate names any more, is
		///deleted. (2) Instead, a save with the password: the entry is not the tool's, so it stays, with that key and its file, the warning names
		///it, and only the key the tool made is deleted. Before round 4 the record named the thumbprint only, so the tool took the hand-given key
		///for its own: it granted it on the re-import and deleted it with the certificate on the save. For each kind of key.</summary>
		[TestMethod]
		public void Apply_ARecordedCertificateThatWasReKeyedByHand_IsNotTheTools() {
			foreach(TestKeyKind testKeyKind in _arrayKinds) {
				foreach(bool isReimport in new[] { true,false }) {
					string label=testKeyKind+(isReimport ? ", imported again" : ", a save with the password");
					string folder=Path.Combine(_folder,testKeyKind+(isReimport ? "-rekeyed-reimport" : "-rekeyed-password"));
					Directory.CreateDirectory(folder);
					string config=TestHelpers.WriteConfig(folder,"ComputerName","localhost","Database","opendental","User","root","Password","");
					string caCopy=DbConnectionConfig.GetOfficeCaCopyPath(Path.Combine(folder,"program"));
					string pathRecord=ImportedCertificateRecord.GetPath(caCopy);
					using(ClientCertificateTestStore clientCertificateTestStore=new ClientCertificateTestStore()) {
						string pathPfx=clientCertificateTestStore.WritePfx(folder,testKeyKind,out string thumbprintClient);
						Dictionary<string,TestKey> dictKeys=null;
						ClientCertificateStore.ActionAfterKeysStoredForTests=x509Certificate2Collection => {
							dictKeys=clientCertificateTestStore.TrackKeys(x509Certificate2Collection);
						};
						ClientCertificateStore.StoreNameForTests=clientCertificateTestStore.Name;
						DbSettingsResult dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Certificate,PfxFile=pathPfx,
							PfxPassword=clientCertificateTestStore.Password },config,caCopy);
						Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
						TestKey testKeyTool=dictKeys[thumbprintClient];
						Assert.AreEqual(thumbprintClient,ImportedCertificateRecord.Read(pathRecord),label+": recorded.");
						//Someone gives the entry another key by hand.
						TestKey testKeyByHand=clientCertificateTestStore.AddCertificate(pathPfx);
						using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
							Assert.IsTrue(string.Equals(TestKeyProvInfo.Read(x509Certificate2Entry).ContainerName,testKeyByHand.Name,StringComparison.OrdinalIgnoreCase),
								label+": the test's premise, the entry names the key given by hand.");
						}
						Assert.IsFalse(string.Equals(testKeyByHand.Name,testKeyTool.Name,StringComparison.OrdinalIgnoreCase),label+": the test's premise, another key.");
						Assert.IsTrue(clientCertificateTestStore.CanSign(thumbprintClient),label+": the test's premise, it is usable.");
						List<string> listGranted=new List<string>();
						ClientCertificateStore.ActionInsteadOfUsersReadGrantForTests=x509Certificate2 => {
							listGranted.Add(TestKeyProvInfo.Read(x509Certificate2).ContainerName);
						};
						if(isReimport) {
							dictKeys=null;
							dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Certificate,PfxFile=pathPfx,
								PfxPassword=clientCertificateTestStore.Password },config,caCopy);
						}
						else {
							dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Password,DbPassword="" },config,caCopy);
						}
						ClientCertificateStore.ActionAfterKeysStoredForTests=null;
						ClientCertificateStore.ActionInsteadOfUsersReadGrantForTests=null;
						ClientCertificateStore.StoreNameForTests=null;
						Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
						Assert.IsFalse(listGranted.Any(x => string.Equals(x,testKeyByHand.Name,StringComparison.OrdinalIgnoreCase)),
							label+": the key given by hand never gets the users' read access: "+string.Join(", ",listGranted));
						Assert.IsTrue(testKeyByHand.Exists(),label+": the key given by hand stays (the store's clean-up deletes it).");
						Assert.IsTrue(File.Exists(testKeyByHand.FilePath),label+": and its file: "+testKeyByHand.FilePath);
						Assert.IsFalse(testKeyTool.Exists(),label+": the key the tool made first, which no certificate names any more, is deleted.");
						Assert.IsFalse(File.Exists(testKeyTool.FilePath),label+": its file is gone: "+testKeyTool.FilePath);
						StringAssert.Contains(dbSettingsResult.Message,"key container "+testKeyByHand.Name,label+": the warning names the key given by hand.");
						if(isReimport) {
							StringAssert.Contains(dbSettingsResult.Message,"The tool's record names the client certificate, but its entry in the computer's "
								+"certificate store (CurrentUser\\"+clientCertificateTestStore.Name+") names another private key than the one the tool gave it (",
								label);
							Assert.IsNotNull(dictKeys,label+": the import stored its key.");
							TestKey testKeyImportAgain=dictKeys[thumbprintClient];
							CollectionAssert.AreEqual(new List<string> { testKeyImportAgain.Name },listGranted,label+": only the new import's key is granted.");
							using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
								Assert.IsTrue(string.Equals(TestKeyProvInfo.Read(x509Certificate2Entry).ContainerName,testKeyImportAgain.Name,
									StringComparison.OrdinalIgnoreCase),label+": the entry took the new import's key.");
							}
							Assert.IsTrue(testKeyImportAgain.Exists(),label+": the new import's key stays (the store's clean-up deletes it).");
							Assert.AreEqual(thumbprintClient,ImportedCertificateRecord.Read(pathRecord),label+": recorded, with the new import's key.");
							continue;
						}
						StringAssert.StartsWith(dbSettingsResult.Message,"Saved the connection settings for the database opendental on localhost (password). "
							+"The client certificate "+thumbprintClient+" that this tool imported before stays in the computer's certificate store (CurrentUser\\"
							+clientCertificateTestStore.Name+"): its entry names another private key (",label);
						CollectionAssert.AreEqual(new List<string> { thumbprintClient },clientCertificateTestStore.GetThumbprints(),label+": the entry stays.");
						using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
							Assert.IsTrue(string.Equals(TestKeyProvInfo.Read(x509Certificate2Entry).ContainerName,testKeyByHand.Name,StringComparison.OrdinalIgnoreCase),
								label+": it still names the key given by hand.");
						}
						Assert.AreEqual("",ImportedCertificateRecord.Read(pathRecord),label+": no record is left.");
						Assert.AreEqual(0,listGranted.Count,label);
					}
				}
			}
		}
	}
}
