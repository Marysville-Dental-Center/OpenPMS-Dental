// The Direct certificate import compares keys by the certificates' key provider information and never opens another certificate's key to do
// so (DIRECT-CERT-FOLLOWUP round 4, part A, the owner's answer of 2026-11-13): the connection settings tool's two key changes of DCFU3,
// through the reader both now share (CodeBase's CertificateKeyProvInfo). Every store is a store of the current user made for the test and
// deleted afterwards (DirectTestStores); the computer's own NHINDPrivate is never opened. Every key the tests or the import store is a
// throwaway key of the current user, deleted afterwards; a key provider information that names a smart card or the machine key set is only
// written on a certificate of the test store, and the seams refuse to open it, so nothing is made, opened or deleted on a smart card or in the
// machine key set. No network. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace LibraryUpdateTests {
	[TestClass]
	public class DirectCertificateKeyProvInfoTests {
		private const string ADDRESS="pat.q@hisp.example.test";
		private const string ADDRESS_OTHER="front@clinic.example.test";
		private const string ADDRESS_THIRD="third@clinic.example.test";
		///<summary>A smart-card CSP's name, as a certificate of a smart card names its key; no such key is ever made or opened here.</summary>
		private const string PROVIDER_SMART_CARD="Microsoft Base Smart Card Crypto Provider";
		///<summary>PROV_RSA_FULL, the smart-card CSP's type.</summary>
		private const int PROV_RSA_FULL=1;

		[TestCleanup]
		public void TestCleanup() {
			DirectCertificateImport.ActionBeforePrivateKeyOpenForTests=null;
			DirectCertificateImport.ActionBeforeKeyProvInfoReadForTests=null;
			DirectCertificateImport.ActionAfterKeysStoredForTests=null;
		}

		///<summary>Round 4, part A (1), a key provider information that cannot be read (the seam makes the smart-card certificate's unreadable): that
		///certificate may use any key, so the import deletes none of the keys it stored and does not use, and the warning says which key was
		///kept and why, naming the certificate whose key could not be identified. Still no key of it is opened.</summary>
		[TestMethod]
		public void ImportPrivateFile_KeepsEveryUnusedKeyWhenAKeyOfTheStoreCannotBeIdentified() {
			using(DirectTestPki directTestPki=new DirectTestPki())
			using(DirectTestStores directTestStores=new DirectTestStores()) {
				List<DirectTestKey> listKeysStored=new List<DirectTestKey>();
				try {
					DirectTestPki.Issued issued=directTestPki.Issue(ADDRESS);
					DirectTestPki.Issued issuedThird=directTestPki.Issue(ADDRESS_THIRD);
					DirectTestPki.Issued issuedSmartCard=directTestPki.Issue(ADDRESS_OTHER);
					AddSmartCardCertificate(directTestStores,issuedSmartCard);
					List<string> listOpened=new List<string>();
					DirectCertificateImport.ActionBeforePrivateKeyOpenForTests=x509Certificate2 => {
						if(x509Certificate2.Thumbprint==issuedSmartCard.Thumbprint) {
							listOpened.Add(x509Certificate2.Subject);
							throw new CryptographicException("The test refuses to open the smart-card certificate's key.");
						}
					};
					DirectCertificateImport.ActionBeforeKeyProvInfoReadForTests=x509Certificate2 => {
						if(x509Certificate2.Thumbprint==issuedSmartCard.Thumbprint) {
							throw new InvalidOperationException("The test makes this key provider information unreadable.");
						}
					};
					Dictionary<string,DirectTestKey> dictKeys=new Dictionary<string,DirectTestKey>();
					DirectCertificateImport.ActionAfterKeysStoredForTests=x509Certificate2Collection => {
						foreach(X509Certificate2 x509Certificate2 in x509Certificate2Collection) {
							if(x509Certificate2.HasPrivateKey) {
								dictKeys[x509Certificate2.Thumbprint]=DirectTestKey.Of(x509Certificate2);
								listKeysStored.Add(dictKeys[x509Certificate2.Thumbprint]);
							}
						}
					};
					string pathPfx=directTestPki.WritePfxMany("pat-and-third.pfx",issued,issuedThird);
					DirectCertificateResult directCertificateResult=DirectCertificateImport.ImportPrivateFile(ADDRESS,pathPfx,directTestPki.Password,null,DateTime.UtcNow);
					Assert.IsTrue(directCertificateResult.IsImported,directCertificateResult.Error);
					Assert.AreEqual(0,listOpened.Count,"No key of the store is opened.");
					Assert.AreEqual("A private key this import stored could not be deleted ("+issuedThird.Subject+", "+issuedThird.Thumbprint+"): It was kept, "
						+"because the private key of a certificate that stays could not be identified, and it may be this one: "+issuedSmartCard.Subject+" ("
						+issuedSmartCard.Thumbprint+"), The test makes this key provider information unreadable.",directCertificateResult.Warning);
					Assert.IsTrue(dictKeys[issuedThird.Thumbprint].Exists(),"The unused key is kept (the test deletes it).");
					Assert.IsTrue(dictKeys[issued.Thumbprint].Exists(),"The imported certificate's key stays.");
				}
				finally {
					DirectCertificateImport.ActionBeforePrivateKeyOpenForTests=null;
					DirectCertificateImport.ActionBeforeKeyProvInfoReadForTests=null;
					DirectCertificateImport.ActionAfterKeysStoredForTests=null;
					DeleteKeys(listKeysStored);
				}
			}
		}

		///<summary>Round 4, part A (1): a certificate of the store (another address's) whose key provider information names a smart-card CSP and a
		///key container that is not there. A Direct import whose file has a key it does not use (another address's certificate with its key)
		///compares that key with the store's keys before it deletes it: by key provider information only, so the smart-card certificate's key is
		///never opened (counted by the seam, which refuses the open so that a smart-card CSP never asks for a card), the unused key is deleted, the
		///imported one stays, and the smart-card certificate stays as it was. Before round 4, GetKeyContainerName opened every key of the store
		///to compare containers.</summary>
		[TestMethod]
		public void ImportPrivateFile_OpensNoKeyOfAnotherCertificateOfTheStoreToCompareKeys() {
			using(DirectTestPki directTestPki=new DirectTestPki())
			using(DirectTestStores directTestStores=new DirectTestStores()) {
				List<DirectTestKey> listKeysStored=new List<DirectTestKey>();
				try {
					DirectTestPki.Issued issued=directTestPki.Issue(ADDRESS);
					DirectTestPki.Issued issuedThird=directTestPki.Issue(ADDRESS_THIRD);
					DirectTestPki.Issued issuedSmartCard=directTestPki.Issue(ADDRESS_OTHER);
					DirectTestKeyProvInfo directTestKeyProvInfoSmartCard=AddSmartCardCertificate(directTestStores,issuedSmartCard);
					List<string> listOpened=new List<string>();
					DirectCertificateImport.ActionBeforePrivateKeyOpenForTests=x509Certificate2 => {
						if(x509Certificate2.Thumbprint==issuedSmartCard.Thumbprint) {
							listOpened.Add(x509Certificate2.Subject);
							throw new CryptographicException("The test refuses to open the smart-card certificate's key.");
						}
					};
					Dictionary<string,DirectTestKey> dictKeys=new Dictionary<string,DirectTestKey>();
					DirectCertificateImport.ActionAfterKeysStoredForTests=x509Certificate2Collection => {
						dictKeys.Clear();
						foreach(X509Certificate2 x509Certificate2 in x509Certificate2Collection) {
							if(x509Certificate2.HasPrivateKey) {
								dictKeys[x509Certificate2.Thumbprint]=DirectTestKey.Of(x509Certificate2);
								listKeysStored.Add(dictKeys[x509Certificate2.Thumbprint]);
							}
						}
					};
					string pathPfx=directTestPki.WritePfxMany("pat-and-third.pfx",issued,issuedThird);
					DirectCertificateResult directCertificateResult=DirectCertificateImport.ImportPrivateFile(ADDRESS,pathPfx,directTestPki.Password,null,DateTime.UtcNow);
					Assert.IsTrue(directCertificateResult.IsImported,directCertificateResult.Error);
					Assert.AreEqual(0,listOpened.Count,"No key of another certificate of the store is opened to compare keys; opened: "
						+string.Join("; ",listOpened));
					Assert.AreEqual("",directCertificateResult.Warning);
					Assert.AreEqual(2,dictKeys.Count,"The import stored the file's two keys.");
					Assert.IsFalse(dictKeys[issuedThird.Thumbprint].Exists(),"The key the import does not use (another address's) is deleted.");
					Assert.IsTrue(dictKeys[issued.Thumbprint].Exists(),"The imported certificate's key stays.");
					CollectionAssert.AreEquivalent(new List<string> { issuedSmartCard.Thumbprint,issued.Thumbprint },
						directTestStores.GetThumbprints(directTestStores.NamePrivate));
					using(X509Certificate2 x509Certificate2SmartCard=FindStored(directTestStores.NamePrivate,issuedSmartCard.Thumbprint)) {
						Assert.IsTrue(DirectTestKeyProvInfo.Read(x509Certificate2SmartCard).IsSameAs(directTestKeyProvInfoSmartCard),
							"The smart-card certificate still names its key as before.");
					}
				}
				finally {
					DirectCertificateImport.ActionBeforePrivateKeyOpenForTests=null;
					DirectCertificateImport.ActionAfterKeysStoredForTests=null;
					DeleteKeys(listKeysStored);
				}
			}
		}

		///<summary>Round 4, part A (2): the store holds the address's certificate, and its entry's key provider information names its key in the
		///machine key set, while the import uses the user's (the test store is the current user's; for the Local Computer's store it is the
		///other way round, an administrator's own user key set, which the program's users cannot read). Check counts it as not in the store and
		///offers the import, without opening that key (counted by the seam, which refuses the open: nothing is opened in the machine key set);
		///the import gives the entry a key of its own, which works, and leaves the entry's earlier key untouched. Before round 4 the entry's key
		///was opened (IsKeyUsable) whatever key set it was in.</summary>
		[TestMethod]
		public void CheckAndImportPrivateFile_AnEntryWhoseKeyIsInAnotherKeySet_IsNotUsable_WithoutOpeningIt() {
			using(DirectTestPki directTestPki=new DirectTestPki())
			using(DirectTestStores directTestStores=new DirectTestStores()) {
				List<DirectTestKey> listKeys=new List<DirectTestKey>();
				try {
					DirectTestPki.Issued issued=directTestPki.Issue(ADDRESS);
					string pathPfx=directTestPki.WritePfx("pat.pfx",issued,isCaIncluded:false);
					//Every key either import stores is tracked as it is stored, so that the finally block deletes it whatever assert fails.
					DirectCertificateImport.ActionAfterKeysStoredForTests=x509Certificate2Collection => {
						foreach(X509Certificate2 x509Certificate2 in x509Certificate2Collection) {
							if(x509Certificate2.HasPrivateKey) {
								listKeys.Add(DirectTestKey.Of(x509Certificate2));
							}
						}
					};
					Assert.IsTrue(DirectCertificateImport.ImportPrivateFile(ADDRESS,pathPfx,directTestPki.Password,null,DateTime.UtcNow).IsImported);
					DirectTestKey directTestKeyFirst;
					using(X509Certificate2 x509Certificate2Entry=FindStored(directTestStores.NamePrivate,issued.Thumbprint)) {
						directTestKeyFirst=DirectTestKey.Of(x509Certificate2Entry);
					}
					Assert.IsTrue(directTestKeyFirst.Exists(),"The test's premise: the first import's key, in the user key set.");
					DirectTestKeyProvInfo directTestKeyProvInfoMachine=DirectTestKeyProvInfo.NameMachineKeySetForEntry(directTestStores.NamePrivate,issued.Thumbprint);
					List<string> listOpened=new List<string>();
					DirectCertificateImport.ActionBeforePrivateKeyOpenForTests=x509Certificate2 => {
						DirectTestKeyProvInfo directTestKeyProvInfo=DirectTestKeyProvInfo.Read(x509Certificate2);
						if(directTestKeyProvInfo.IsNamed && directTestKeyProvInfo.IsMachine) {
							listOpened.Add(x509Certificate2.Subject);
							throw new CryptographicException("The test refuses to open a key of the machine key set.");
						}
					};
					DirectCertificateResult directCertificateResultCheck=DirectCertificateImport.CheckPrivateFile(ADDRESS,pathPfx,directTestPki.Password,DateTime.UtcNow);
					Assert.AreEqual("",directCertificateResultCheck.Error,"Not refused as already there.");
					Assert.AreEqual(issued.Thumbprint,directCertificateResultCheck.ListDetails.Single().Thumbprint);
					Assert.AreEqual(0,directCertificateResultCheck.ListDetailsInStore.Count);
					Assert.AreEqual(0,listOpened.Count,"Check does not open a key in another key set than the import's.");
					DirectCertificateResult directCertificateResult=DirectCertificateImport.ImportPrivateFile(ADDRESS,pathPfx,directTestPki.Password,null,DateTime.UtcNow);
					Assert.IsTrue(directCertificateResult.IsImported,directCertificateResult.Error);
					Assert.AreEqual(0,listOpened.Count,"The import does not open it either.");
					DirectCertificateImport.ActionBeforePrivateKeyOpenForTests=null;
					CollectionAssert.AreEqual(new List<string> { issued.Thumbprint },directTestStores.GetThumbprints(directTestStores.NamePrivate));
					using(X509Certificate2 x509Certificate2Entry=FindStored(directTestStores.NamePrivate,issued.Thumbprint)) {
						DirectTestKeyProvInfo directTestKeyProvInfoNow=DirectTestKeyProvInfo.Read(x509Certificate2Entry);
						Assert.IsFalse(directTestKeyProvInfoNow.IsMachine,"The entry names the import's key, in the user key set.");
						Assert.IsFalse(directTestKeyProvInfoNow.IsSameAs(directTestKeyProvInfoMachine));
						Assert.IsTrue(DirectCertificateImport.IsKeyUsable(x509Certificate2Entry),"The entry's new key works.");
					}
					Assert.IsTrue(directTestKeyFirst.Exists(),"The entry's earlier key (the user key the test's machine-set name stood for) is untouched.");
				}
				finally {
					DirectCertificateImport.ActionBeforePrivateKeyOpenForTests=null;
					DirectCertificateImport.ActionAfterKeysStoredForTests=null;
					DeleteKeys(listKeys);
				}
			}
		}

		///<summary>Adds the public certificate to the test's private store and gives its entry a key provider information that names a smart-card
		///CSP and a key container of the current user that no CSP has (a name made for the test). Returns it, as read back.</summary>
		private static DirectTestKeyProvInfo AddSmartCardCertificate(DirectTestStores directTestStores,DirectTestPki.Issued issued) {
			using(X509Certificate2 x509Certificate2Public=new X509Certificate2(issued.RawData)) {
				directTestStores.AddPrivateWithoutKey(x509Certificate2Public);
			}
			DirectTestKeyProvInfo directTestKeyProvInfo=new DirectTestKeyProvInfo();
			directTestKeyProvInfo.IsNamed=true;
			directTestKeyProvInfo.ContainerName="OpenPmsDirectSmartCardTest_"+Guid.NewGuid().ToString("N");
			directTestKeyProvInfo.ProviderName=PROVIDER_SMART_CARD;
			directTestKeyProvInfo.ProviderType=PROV_RSA_FULL;
			directTestKeyProvInfo.KeySpec=1;//AT_KEYEXCHANGE
			DirectTestKeyProvInfo.WriteToStoreEntry(directTestStores.NamePrivate,issued.Thumbprint,directTestKeyProvInfo);
			using(X509Certificate2 x509Certificate2=FindStored(directTestStores.NamePrivate,issued.Thumbprint)) {
				DirectTestKeyProvInfo directTestKeyProvInfoRead=DirectTestKeyProvInfo.Read(x509Certificate2);
				Assert.IsTrue(directTestKeyProvInfoRead.IsSameAs(directTestKeyProvInfo),"The test's premise: the store keeps the smart-card key provider "
					+"information.");
				Assert.IsTrue(x509Certificate2.HasPrivateKey,"The test's premise: the certificate says it has a private key.");
				return directTestKeyProvInfoRead;
			}
		}

		///<summary>The store's certificate with this thumbprint, or null. The caller disposes it.</summary>
		internal static X509Certificate2 FindStored(string storeName,string thumbprint) {
			using(X509Store x509Store=new X509Store(storeName,StoreLocation.CurrentUser)) {
				x509Store.Open(OpenFlags.ReadOnly|OpenFlags.OpenExistingOnly);
				X509Certificate2Collection x509Certificate2Collection=x509Store.Certificates.Find(X509FindType.FindByThumbprint,thumbprint,false);
				return x509Certificate2Collection.Count==0 ? null : x509Certificate2Collection[0];
			}
		}

		///<summary>Deletes every key of the list that is still there, and fails when one is left.</summary>
		internal static void DeleteKeys(List<DirectTestKey> listKeys) {
			List<string> listLeft=new List<string>();
			foreach(DirectTestKey directTestKey in listKeys) {
				string problem=directTestKey.DeleteIfExists();
				if(problem!="") {
					listLeft.Add(problem);
				}
			}
			if(listLeft.Count>0) {
				Assert.Fail("A test key was left behind:\r\n"+string.Join("\r\n",listLeft));
			}
		}
	}

	///<summary>A throwaway key of the current user, found by the test from a certificate's key provider information (DirectTestKeyProvInfo), apart
	///from the code under test: a CNG key (provider type 0) or a CAPI key container, in the user key set only.</summary>
	public sealed class DirectTestKey {
		public string Name;
		public string Provider;
		public int ProviderType;
		public string Subject;

		///<summary>The key the certificate names. Fails for a key outside the user key set, which the tests never touch.</summary>
		public static DirectTestKey Of(X509Certificate2 x509Certificate2) {
			DirectTestKeyProvInfo directTestKeyProvInfo=DirectTestKeyProvInfo.Read(x509Certificate2);
			Assert.IsTrue(directTestKeyProvInfo.IsNamed,"The certificate names a key: "+x509Certificate2.Subject);
			Assert.IsFalse(directTestKeyProvInfo.IsMachine,"A test key is in the user key set: "+x509Certificate2.Subject);
			DirectTestKey directTestKey=new DirectTestKey();
			directTestKey.Name=directTestKeyProvInfo.ContainerName;
			directTestKey.Provider=directTestKeyProvInfo.ProviderName;
			directTestKey.ProviderType=(int)directTestKeyProvInfo.ProviderType;
			directTestKey.Subject=x509Certificate2.Subject;
			return directTestKey;
		}

		public bool Exists() {
			if(ProviderType==0) {
				return CngKey.Exists(Name,new CngProvider(Provider),CngKeyOpenOptions.Silent);
			}
			try {
				using(new RSACryptoServiceProvider(GetCspParameters())) {
				}
				return true;
			}
			catch(CryptographicException) {
				return false;//"Keyset does not exist".
			}
		}

		///<summary>Deletes the key if it is still there: "" when it is gone, else what is left. Never throws.</summary>
		public string DeleteIfExists() {
			try {
				if(Exists()) {
					if(ProviderType==0) {
						CngKey cngKey=CngKey.Open(Name,new CngProvider(Provider),CngKeyOpenOptions.Silent);
						cngKey.Delete();//Also disposes it.
					}
					else {
						using(RSACryptoServiceProvider rsaCryptoServiceProvider=new RSACryptoServiceProvider(GetCspParameters())) {
							rsaCryptoServiceProvider.PersistKeyInCsp=false;//Dispose then deletes the key container.
						}
					}
				}
				return Exists() ? "The test key "+Name+" ("+Provider+") of "+Subject+" is still there." : "";
			}
			catch(Exception ex) {
				return "The test key "+Name+" ("+Provider+") of "+Subject+" could not be deleted: "+ex.Message.Trim();
			}
		}

		private CspParameters GetCspParameters() {
			CspParameters cspParameters=new CspParameters(ProviderType,Provider,Name);
			cspParameters.Flags=CspProviderFlags.UseExistingKey|CspProviderFlags.NoPrompt;
			return cspParameters;
		}
	}

	///<summary>A certificate's key provider information (CERT_KEY_PROV_INFO_PROP_ID), read and written by the tests themselves, apart from the
	///code under test (CodeBase's CertificateKeyProvInfo), as DbConnectionSecurityTests' TestKeyProvInfo: the key container, provider, key set
	///and key spec, read without opening the key; written on a certificate of a test store only, with no provider parameters.</summary>
	public sealed class DirectTestKeyProvInfo {
		[DllImport("crypt32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CertGetCertificateContextProperty(IntPtr pCertContext,uint dwPropId,IntPtr pvData,ref uint pcbData);
		[DllImport("crypt32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CertSetCertificateContextProperty(IntPtr pCertContext,uint dwPropId,uint dwFlags,IntPtr pvData);
		private const uint CERT_KEY_PROV_INFO_PROP_ID=2;
		///<summary>CRYPT_MACHINE_KEYSET (CAPI), NCRYPT_MACHINE_KEY_FLAG (CNG): the same bit of dwFlags.</summary>
		public const uint CRYPT_MACHINE_KEYSET=0x20;

		[StructLayout(LayoutKind.Sequential)]
		private struct CRYPT_KEY_PROV_INFO {
			public IntPtr pwszContainerName;
			public IntPtr pwszProvName;
			public uint dwProvType;
			public uint dwFlags;
			public uint cProvParam;
			public IntPtr rgProvParam;
			public uint dwKeySpec;
		}

		///<summary>False when the certificate has no key provider information (no key).</summary>
		public bool IsNamed;
		public string ContainerName="";
		public string ProviderName="";
		public uint ProviderType;
		public uint Flags;
		public uint KeySpec;

		public bool IsMachine {
			get {
				return (Flags & CRYPT_MACHINE_KEYSET)!=0;
			}
		}

		public bool IsSameAs(DirectTestKeyProvInfo directTestKeyProvInfo) {
			return IsNamed==directTestKeyProvInfo.IsNamed && ContainerName==directTestKeyProvInfo.ContainerName
				&& ProviderName==directTestKeyProvInfo.ProviderName && ProviderType==directTestKeyProvInfo.ProviderType
				&& Flags==directTestKeyProvInfo.Flags && KeySpec==directTestKeyProvInfo.KeySpec;
		}

		///<summary>The certificate's key provider information. Throws when it cannot be read for another reason than its absence.</summary>
		public static DirectTestKeyProvInfo Read(X509Certificate2 x509Certificate2) {
			if(x509Certificate2.Handle==IntPtr.Zero) {
				throw new InvalidOperationException("The certificate was already released.");
			}
			DirectTestKeyProvInfo directTestKeyProvInfo=new DirectTestKeyProvInfo();
			uint byteCount=0;
			if(!CertGetCertificateContextProperty(x509Certificate2.Handle,CERT_KEY_PROV_INFO_PROP_ID,IntPtr.Zero,ref byteCount)) {
				int error=Marshal.GetLastWin32Error();
				if(error==unchecked((int)0x80092004)) {//CRYPT_E_NOT_FOUND
					return directTestKeyProvInfo;
				}
				throw new System.ComponentModel.Win32Exception(error);
			}
			IntPtr intPtrData=Marshal.AllocHGlobal((int)byteCount);
			try {
				if(!CertGetCertificateContextProperty(x509Certificate2.Handle,CERT_KEY_PROV_INFO_PROP_ID,intPtrData,ref byteCount)) {
					throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
				}
				CRYPT_KEY_PROV_INFO cryptKeyProvInfo=(CRYPT_KEY_PROV_INFO)Marshal.PtrToStructure(intPtrData,typeof(CRYPT_KEY_PROV_INFO));
				directTestKeyProvInfo.IsNamed=true;
				directTestKeyProvInfo.ContainerName=Marshal.PtrToStringUni(cryptKeyProvInfo.pwszContainerName)??"";
				directTestKeyProvInfo.ProviderName=Marshal.PtrToStringUni(cryptKeyProvInfo.pwszProvName)??"";
				directTestKeyProvInfo.ProviderType=cryptKeyProvInfo.dwProvType;
				directTestKeyProvInfo.Flags=cryptKeyProvInfo.dwFlags;
				directTestKeyProvInfo.KeySpec=cryptKeyProvInfo.dwKeySpec;
				return directTestKeyProvInfo;
			}
			finally {
				Marshal.FreeHGlobal(intPtrData);
				GC.KeepAlive(x509Certificate2);
			}
		}

		///<summary>Sets the key provider information on the entry of the certificate with this thumbprint in the current user's store of this name
		///(a test store), which keeps it.</summary>
		public static void WriteToStoreEntry(string storeName,string thumbprint,DirectTestKeyProvInfo directTestKeyProvInfo) {
			using(X509Store x509Store=new X509Store(storeName,StoreLocation.CurrentUser)) {
				x509Store.Open(OpenFlags.ReadWrite|OpenFlags.OpenExistingOnly);
				X509Certificate2Collection x509Certificate2Collection=x509Store.Certificates.Find(X509FindType.FindByThumbprint,thumbprint,false);
				try {
					Assert.AreEqual(1,x509Certificate2Collection.Count,"The test's premise: the store has the certificate.");
					directTestKeyProvInfo.Write(x509Certificate2Collection[0]);
				}
				finally {
					foreach(X509Certificate2 x509Certificate2 in x509Certificate2Collection) {
						x509Certificate2.Reset();
					}
				}
			}
		}

		///<summary>Makes the store entry of the certificate name its key container in the machine key set instead of the user's (the
		///CRYPT_MACHINE_KEYSET flag), as an entry whose key is in another key set than the test store's import's: nothing is made, opened or
		///deleted in the machine key set, and the user key the entry named stays for the test to delete. Returns it, as read back.</summary>
		public static DirectTestKeyProvInfo NameMachineKeySetForEntry(string storeName,string thumbprint) {
			DirectTestKeyProvInfo directTestKeyProvInfo;
			using(X509Certificate2 x509Certificate2=DirectCertificateKeyProvInfoTests.FindStored(storeName,thumbprint)) {
				directTestKeyProvInfo=Read(x509Certificate2);
			}
			Assert.IsTrue(directTestKeyProvInfo.IsNamed && !directTestKeyProvInfo.IsMachine,"The test's premise: the entry names a key of the user key set.");
			directTestKeyProvInfo.Flags|=CRYPT_MACHINE_KEYSET;
			WriteToStoreEntry(storeName,thumbprint,directTestKeyProvInfo);
			using(X509Certificate2 x509Certificate2=DirectCertificateKeyProvInfoTests.FindStored(storeName,thumbprint)) {
				DirectTestKeyProvInfo directTestKeyProvInfoNow=Read(x509Certificate2);
				Assert.IsTrue(directTestKeyProvInfoNow.IsSameAs(directTestKeyProvInfo),"The test's premise: the store keeps the entry's key provider "
					+"information as the test set it.");
				return directTestKeyProvInfoNow;
			}
		}

		///<summary>Sets this key provider information on the certificate's context.</summary>
		public void Write(X509Certificate2 x509Certificate2) {
			IntPtr intPtrContainer=Marshal.StringToHGlobalUni(ContainerName);
			IntPtr intPtrProvider=Marshal.StringToHGlobalUni(ProviderName);
			IntPtr intPtrData=IntPtr.Zero;
			try {
				CRYPT_KEY_PROV_INFO cryptKeyProvInfo=new CRYPT_KEY_PROV_INFO();
				cryptKeyProvInfo.pwszContainerName=intPtrContainer;
				cryptKeyProvInfo.pwszProvName=intPtrProvider;
				cryptKeyProvInfo.dwProvType=ProviderType;
				cryptKeyProvInfo.dwFlags=Flags;
				cryptKeyProvInfo.dwKeySpec=KeySpec;
				intPtrData=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CRYPT_KEY_PROV_INFO)));
				Marshal.StructureToPtr(cryptKeyProvInfo,intPtrData,false);
				if(!CertSetCertificateContextProperty(x509Certificate2.Handle,CERT_KEY_PROV_INFO_PROP_ID,0,intPtrData)) {
					throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
				}
			}
			finally {
				if(intPtrData!=IntPtr.Zero) {
					Marshal.FreeHGlobal(intPtrData);
				}
				Marshal.FreeHGlobal(intPtrContainer);
				Marshal.FreeHGlobal(intPtrProvider);
				GC.KeepAlive(x509Certificate2);
			}
		}
	}
}
