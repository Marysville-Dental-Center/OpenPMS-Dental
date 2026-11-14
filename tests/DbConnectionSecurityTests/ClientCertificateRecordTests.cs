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
using System.Security.Cryptography;
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
			ClientCertificateStore.ActionBeforePrivateKeyOpenForTests=null;
			ClientCertificateStore.ActionBeforeKeyDeleteByNameForTests=null;
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

		///<summary>Part C: a record written in format 2 reads back field for field (the tool's key, the entry's earlier key with a provider
		///parameter, and names with "%", CR and LF in them), its file starts with the format line, and a record of the old format reads as its
		///thumbprint with no key named.</summary>
		[TestMethod]
		public void Record_WrittenInFormat2_ReadsBackFieldForField() {
			string caCopy=DbConnectionConfig.GetOfficeCaCopyPath(Path.Combine(_folder,"program"));
			string pathRecord=ImportedCertificateRecord.GetPath(caCopy);
			ImportedCertificate importedCertificate=ImportedCertificate.FromThumbprint(THUMB.ToLowerInvariant());
			importedCertificate.KeyTool=new CodeBase.CertificateKeyProvInfo { IsNamed=true,IsFieldsRead=true,ContainerName="tool 100%\r\nkey",
				ProviderName="Microsoft Software Key Storage Provider",ProviderType=0,Flags=0x20,KeySpec=0 };
			importedCertificate.KeyBefore=new CodeBase.CertificateKeyProvInfo { IsNamed=true,IsFieldsRead=true,ContainerName="admin-key",
				ProviderName="Microsoft Enhanced RSA and AES Cryptographic Provider",ProviderType=24,Flags=0x21,KeySpec=1 };
			importedCertificate.KeyBefore.ListProvParams.Add(new CodeBase.CertificateKeyProvParam { Param=7,Flags=1,Data=new byte[] { 0x0A,0xFF } });
			ImportedCertificateRecord.Write(pathRecord,importedCertificate);
			Assert.AreEqual(FORMAT_LINE,File.ReadAllLines(pathRecord)[0]);
			ImportedCertificate importedCertificateRead=ImportedCertificateRecord.ReadRecord(pathRecord);
			Assert.IsNotNull(importedCertificateRead);
			Assert.IsFalse(importedCertificateRead.IsOldFormat);
			Assert.AreEqual(THUMB,importedCertificateRead.Thumbprint);
			Assert.IsTrue(CodeBase.CertificateKeyProvInfo.IsSameExactly(importedCertificate.KeyTool,importedCertificateRead.KeyTool),"The tool's key.");
			Assert.IsTrue(importedCertificateRead.KeyTool.IsMachine);
			Assert.IsTrue(CodeBase.CertificateKeyProvInfo.IsSameExactly(importedCertificate.KeyBefore,importedCertificateRead.KeyBefore),"The earlier key.");
			Assert.IsTrue(CodeBase.CertificateKeyProvInfo.IsSameKeyStrict(importedCertificate.KeyTool,importedCertificateRead.KeyTool));
			//Without keys.
			ImportedCertificateRecord.Write(pathRecord,THUMB);
			importedCertificateRead=ImportedCertificateRecord.ReadRecord(pathRecord);
			Assert.AreEqual(THUMB,importedCertificateRead.Thumbprint);
			Assert.IsNull(importedCertificateRead.KeyTool);
			Assert.IsNull(importedCertificateRead.KeyBefore);
			//The old format: the thumbprint, no key named.
			File.WriteAllText(pathRecord,THUMB+"\r\n");
			importedCertificateRead=ImportedCertificateRecord.ReadRecord(pathRecord);
			Assert.IsTrue(importedCertificateRead.IsOldFormat);
			Assert.AreEqual(THUMB,importedCertificateRead.Thumbprint);
			Assert.IsNull(importedCertificateRead.KeyTool);
			ImportedCertificateRecord.Write(pathRecord,(ImportedCertificate)null);
			Assert.IsFalse(File.Exists(pathRecord));
			Assert.IsNull(ImportedCertificateRecord.ReadRecord(pathRecord));
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

		///<summary>The DCFU4 second fix round's review finding 1: a record of the old format names no key, and the save that rewrites it
		///(CompleteRecord) must name the entry's key only when the old rule would have claimed it on a re-import: a key in the import's key set
		///that is usable. (1) The tool imports the certificate; the test replaces the record with its thumbprint line (an older tool's record).
		///(2) Someone gives the entry another key by hand (the test store adds the same .pfx again), and (a) makes the entry name it in another
		///key set than the import's (the machine key set: only the flag is changed, nothing is made, opened or deleted there), or (b) the key
		///does not open for the tool (the seam refuses it, as a key that needs a PIN). (3) A save without an import rewrites the record: it names
		///no key. (4) The same .pfx imported again: no key is deleted by its name (the seam sees none), the hand-given key and its file stay; in
		///(a) the warning names that key, says that the tool names it on the entry again, and gives no certutil -delkey command (finding 2), and
		///the record keeps it as the entry's earlier key. (5) A save with the password: in (a) the entry names the hand-given key again, exactly,
		///and that key and its file still exist; the result says what was put back and what to do if that key was deleted since; in (b) the
		///certificate goes with the second import's key, as the old rule did. The hand-given key and its file stay throughout. Before the fix the
		///save of (3) recorded the hand-given key as the tool's, so the re-import of (4) deleted it by its name. For each kind of key.</summary>
		[TestMethod]
		public void Apply_AnOldFormatRecordWhoseEntryWasReKeyedByHand_ClaimsNoKeyTheOldRuleDidNot() {
			foreach(TestKeyKind testKeyKind in _arrayKinds) {
				foreach(bool isOtherKeySet in new[] { false,true }) {
					string label=testKeyKind+(isOtherKeySet ? ", a hand-given key in another key set" : ", a hand-given key that does not open");
					string folder=Path.Combine(_folder,testKeyKind+(isOtherKeySet ? "-old-rekeyed-keyset" : "-old-rekeyed-noopen"));
					Directory.CreateDirectory(folder);
					string config=TestHelpers.WriteConfig(folder,"ComputerName","localhost","Database","opendental","User","root","Password","");
					string caCopy=DbConnectionConfig.GetOfficeCaCopyPath(Path.Combine(folder,"program"));
					string pathRecord=ImportedCertificateRecord.GetPath(caCopy);
					using(ClientCertificateTestStore clientCertificateTestStore=new ClientCertificateTestStore()) {
						string storeText="CurrentUser\\"+clientCertificateTestStore.Name;
						string pathPfx=clientCertificateTestStore.WritePfx(folder,testKeyKind,out string thumbprintClient);
						Dictionary<string,TestKey> dictKeys=null;
						ClientCertificateStore.ActionAfterKeysStoredForTests=x509Certificate2Collection => {
							dictKeys=clientCertificateTestStore.TrackKeys(x509Certificate2Collection);
						};
						ClientCertificateStore.StoreNameForTests=clientCertificateTestStore.Name;
						//(1)
						DbSettingsResult dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Certificate,PfxFile=pathPfx,
							PfxPassword=clientCertificateTestStore.Password },config,caCopy);
						Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+" (1): "+dbSettingsResult.Message);
						File.WriteAllText(pathRecord,thumbprintClient+"\r\n");//The old format.
						//(2)
						TestKey testKeyByHand=clientCertificateTestStore.AddCertificate(pathPfx);
						TestKeyProvInfo testKeyProvInfoByHand;
						if(isOtherKeySet) {
							testKeyProvInfoByHand=clientCertificateTestStore.NameMachineKeySetForEntry(thumbprintClient);
						}
						else {
							using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
								testKeyProvInfoByHand=TestKeyProvInfo.Read(x509Certificate2Entry);
							}
							ClientCertificateStore.ActionBeforePrivateKeyOpenForTests=x509Certificate2 => {
								if(string.Equals(TestKeyProvInfo.Read(x509Certificate2).ContainerName,testKeyByHand.Name,StringComparison.OrdinalIgnoreCase)) {
									throw new CryptographicException("The test refuses to open the hand-given key (it needs a PIN).");
								}
							};
						}
						Assert.IsTrue(string.Equals(testKeyProvInfoByHand.ContainerName,testKeyByHand.Name,StringComparison.OrdinalIgnoreCase),
							label+": the test's premise, the entry names the hand-given key's container.");
						List<string> listDeletesByName=new List<string>();
						ClientCertificateStore.ActionBeforeKeyDeleteByNameForTests=keyProvInfo => {
							if(string.Equals(keyProvInfo.ContainerName,testKeyByHand.Name,StringComparison.OrdinalIgnoreCase)) {
								listDeletesByName.Add(keyProvInfo.Describe());
								throw new CryptographicException("The test refuses to delete the hand-given key by its name.");
							}
						};
						//(3) A save without an import.
						dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest(),config,caCopy);
						Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+" (3): "+dbSettingsResult.Message);
						List<string> listLinesSave=File.ReadAllLines(pathRecord).ToList();
						//(4) The same .pfx again.
						dictKeys=null;
						dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Certificate,PfxFile=pathPfx,
							PfxPassword=clientCertificateTestStore.Password },config,caCopy);
						Assert.AreEqual(0,listDeletesByName.Count,label+" (4): no key the tool did not create is deleted by its name: "
							+string.Join("; ",listDeletesByName)+" | "+dbSettingsResult.Message);
						Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+" (4): "+dbSettingsResult.Message);
						Assert.AreEqual(FORMAT_LINE,listLinesSave[0],label+" (3): rewritten in format 2.");
						Assert.IsFalse(listLinesSave.Any(x => x.StartsWith("tool.")),label+" (3): the record names no key: "+string.Join(" | ",listLinesSave));
						Assert.IsTrue(testKeyByHand.Exists(),label+" (4): the hand-given key stays.");
						Assert.IsTrue(File.Exists(testKeyByHand.FilePath),label+" (4): and its file: "+testKeyByHand.FilePath);
						Assert.IsFalse(dbSettingsResult.Message.Contains("-delkey"),label+" (4): "+dbSettingsResult.Message);
						Assert.IsNotNull(dictKeys,label+" (4): the import stored its key.");
						TestKey testKeyImport=dictKeys[thumbprintClient];
						ImportedCertificate importedCertificateRead=ImportedCertificateRecord.ReadRecord(pathRecord);
						Assert.IsNotNull(importedCertificateRead,label+" (4)");
						Assert.IsTrue(string.Equals(importedCertificateRead.KeyTool.ContainerName,testKeyImport.Name,StringComparison.OrdinalIgnoreCase),
							label+" (4): the record names the import's key.");
						if(isOtherKeySet) {
							StringAssert.Contains(dbSettingsResult.Message,"left the entry's earlier key untouched: provider "+testKeyProvInfoByHand.ProviderName
								+", key container "+testKeyProvInfoByHand.ContainerName+", in the computer's key set. The tool's record keeps it, and the tool names "
								+"that key on the entry again when this import is undone or a later save stops using this certificate, so do not delete that "
								+"key while the record names it.",label+" (4)");
							Assert.IsNotNull(importedCertificateRead.KeyBefore,label+" (4): the record keeps the entry's earlier key.");
							Assert.IsTrue(string.Equals(importedCertificateRead.KeyBefore.ContainerName,testKeyByHand.Name,StringComparison.OrdinalIgnoreCase)
								&& importedCertificateRead.KeyBefore.IsMachine,label+" (4): as the entry named it.");
						}
						else {
							Assert.AreEqual("Saved the connection settings for the database opendental on localhost (client certificate).",dbSettingsResult.Message,
								label+" (4): the old rule, the entry without a usable key takes the import's.");
							Assert.IsNull(importedCertificateRead.KeyBefore,label+" (4)");
						}
						//(5) A save with the password.
						dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Password,DbPassword="" },config,caCopy);
						ClientCertificateStore.ActionAfterKeysStoredForTests=null;
						ClientCertificateStore.ActionBeforePrivateKeyOpenForTests=null;
						ClientCertificateStore.ActionBeforeKeyDeleteByNameForTests=null;
						ClientCertificateStore.StoreNameForTests=null;
						Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+" (5): "+dbSettingsResult.Message);
						Assert.AreEqual(0,listDeletesByName.Count,label+" (5): "+string.Join("; ",listDeletesByName));
						Assert.IsFalse(File.Exists(pathRecord),label+" (5): no record is left.");
						Assert.IsFalse(testKeyImport.Exists(),label+" (5): the second import's key is deleted.");
						Assert.IsFalse(File.Exists(testKeyImport.FilePath),label+" (5): its file is gone.");
						Assert.IsTrue(testKeyByHand.Exists(),label+" (5): the hand-given key still exists (the store's clean-up deletes it).");
						Assert.IsTrue(File.Exists(testKeyByHand.FilePath),label+" (5): and its file: "+testKeyByHand.FilePath);
						if(isOtherKeySet) {
							Assert.AreEqual("Saved the connection settings for the database opendental on localhost (password). The client certificate "
								+thumbprintClient+" was in the computer's certificate store ("+storeText+") before the tool's import gave it a private key: its "
								+"entry names its earlier private key again (provider "+testKeyProvInfoByHand.ProviderName+", key container "
								+testKeyProvInfoByHand.ContainerName+", in the computer's key set), as before that import, and the private key the tool gave it "
								+"was deleted."+GetPutBackHedge(storeText,thumbprintClient),dbSettingsResult.Message,label+" (5)");
							CollectionAssert.AreEqual(new List<string> { thumbprintClient },clientCertificateTestStore.GetThumbprints(),label+" (5): the entry stays.");
							using(X509Certificate2 x509Certificate2Entry=clientCertificateTestStore.Find(thumbprintClient)) {
								Assert.IsTrue(TestKeyProvInfo.Read(x509Certificate2Entry).IsSameAs(testKeyProvInfoByHand),label+" (5): it names the hand-given key "
									+"again, exactly.");
							}
						}
						else {
							Assert.AreEqual("Saved the connection settings for the database opendental on localhost (password).",dbSettingsResult.Message,
								label+" (5)");
							Assert.AreEqual(0,clientCertificateTestStore.GetThumbprints().Count,label+" (5): the certificate is removed, as the old rule did.");
						}
					}
				}
			}
		}

		///<summary>The DCFU4 second fix round's review finding 1, its guard: a record of format 2 whose tool key is outside the key set the tool
		///imports into (the test writes the record an import wrote again with the machine key set; the tool never makes such a key, so only a
		///record edited by hand can say so). A save with the password takes nothing of it for the tool's: no key is deleted by its name, the entry
		///stays with its key, the result says why and how to remove it by hand, and the record is gone. Before the fix the tool deleted that key
		///by its name (the seam saw it). For each kind of key.</summary>
		[TestMethod]
		public void Apply_ARecordWhoseToolKeyIsOutsideTheImportsKeySet_IsNotTheTools() {
			foreach(TestKeyKind testKeyKind in _arrayKinds) {
				string label=testKeyKind.ToString();
				string folder=Path.Combine(_folder,testKeyKind+"-tool-key-outside");
				Directory.CreateDirectory(folder);
				string config=TestHelpers.WriteConfig(folder,"ComputerName","localhost","Database","opendental","User","root","Password","");
				string caCopy=DbConnectionConfig.GetOfficeCaCopyPath(Path.Combine(folder,"program"));
				string pathRecord=ImportedCertificateRecord.GetPath(caCopy);
				using(ClientCertificateTestStore clientCertificateTestStore=new ClientCertificateTestStore()) {
					string storeText="CurrentUser\\"+clientCertificateTestStore.Name;
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
					ImportedCertificate importedCertificate=ImportedCertificateRecord.ReadRecord(pathRecord);
					Assert.IsFalse(importedCertificate.KeyTool.IsMachine,label+": the test's premise, the import's key is in the user key set.");
					importedCertificate.KeyTool.Flags|=0x20;//CRYPT_MACHINE_KEYSET
					ImportedCertificateRecord.Write(pathRecord,importedCertificate);
					Assert.IsTrue(ImportedCertificateRecord.ReadRecord(pathRecord).KeyTool.IsMachine,label+": the test's premise, the record says so.");
					List<string> listDeletesByName=new List<string>();
					ClientCertificateStore.ActionBeforeKeyDeleteByNameForTests=keyProvInfo => {
						listDeletesByName.Add(keyProvInfo.Describe());
						throw new CryptographicException("The test refuses every delete by name.");
					};
					dbSettingsResult=DbSettingsRunner.Apply(new DbSettingsRequest() { Auth=DbSettingsAuth.Password,DbPassword="" },config,caCopy);
					ClientCertificateStore.ActionBeforeKeyDeleteByNameForTests=null;
					ClientCertificateStore.StoreNameForTests=null;
					Assert.AreEqual(DbSettingsRunner.EXIT_OK,dbSettingsResult.ExitCode,label+": "+dbSettingsResult.Message);
					Assert.AreEqual(0,listDeletesByName.Count,label+": no key is deleted by its name: "+string.Join("; ",listDeletesByName));
					StringAssert.Contains(dbSettingsResult.Message,"The tool's record names the client certificate "+thumbprintClient+" with a private key "
						+"outside the key set the tool imports into (",label);
					StringAssert.Contains(dbSettingsResult.Message,"so the tool leaves the certificate in the computer's certificate store ("+storeText
						+") and every key as they are.",label);
					CollectionAssert.AreEqual(new List<string> { thumbprintClient },clientCertificateTestStore.GetThumbprints(),label+": the entry stays.");
					Assert.IsTrue(testKeyImport.Exists(),label+": its key stays (the store's clean-up deletes it).");
					Assert.IsTrue(File.Exists(testKeyImport.FilePath),label+": and its file.");
					Assert.IsFalse(File.Exists(pathRecord),label+": no record is left.");
				}
			}
		}

		///<summary>The sentence that follows a put-back: what to do if the earlier key was deleted since (the DCFU4 second fix round's review
		///finding 2).</summary>
		public static string GetPutBackHedge(string storeText,string thumbprint) {
			return " If someone deleted that earlier key since, the entry names a key that is not there: then remove the certificate as an "
				+"administrator, in PowerShell: Remove-Item Cert:\\"+storeText+"\\"+thumbprint+".";
		}
	}
}
