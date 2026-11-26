// The client certificate in the computer's certificate store (LocalMachine\My), its private key not exportable.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using CodeBase;
using DataConnectionBase;

namespace OpenPmsDentalDbSettings {
	///<summary>Whether the store held the client certificate before ImportPfx, and what became of its entry's earlier key: the caller says so
	///when it undoes the import (DbSettingsRunner, a save that fails after the import). An entry whose earlier key was left aside gets it back
	///(RemoveImported, from ImportedCertificate.KeyBefore: DIRECT-CERT-FOLLOWUP round 4, part B); any other entry that took the import's key is
	///removed whole.</summary>
	public enum ClientCertificateEntryBefore {
		///<summary>The store did not hold the certificate: the import added it.</summary>
		None,
		///<summary>The store held it, and no earlier key of its entry was left aside: the entry kept its key (the tool's own), or had no usable
		///key of anyone else's (no key provider information at all, or the tool's own key that no longer opens, or a key an old-format record
		///claims) and took the import's.</summary>
		InStore,
		///<summary>The store held it, and its entry took the import's key, leaving its earlier key untouched: a key in another key set, one that
		///could not be identified, a usable key the tool did not create (also one given by hand to a certificate the tool's record names), or a key
		///in the import's key set that the tool did not create and that does not open (a smart card that is not inserted, a key that needs a PIN,
		///a deleted key file). The import's warning says where that key is, and ImportPfx's ImportedCertificate.KeyBefore holds what the entry
		///named before (null only when that could not be read at all).</summary>
		InStoreKeyLeft,
	}

	///<summary>The client certificate (owner decision Q2): one shared certificate on every workstation, or one per computer later, in
	///LocalMachine\My, read by MySqlConnector by thumbprint. A .pfx is imported with MachineKeySet and PersistKeySet and never Exportable, and
	///BUILTIN\Users get read access to its private key, because the program runs as the signed-in user (docs/specs/db-connection-security.md
	///section 4.3). So the tool never leaves a key the users may read behind, and never touches a key it did not create (owner decision of
	///2026-11-02).</summary>
	public static class ClientCertificateStore {
		///<summary>Extended Key Usage OID of TLS client authentication.</summary>
		private const string OID_CLIENT_AUTHENTICATION="1.3.6.1.5.5.7.3.2";
		///<summary>How to make a .pfx with one client certificate, for the messages that refuse a file: docs/admin/db-connection-security.md section 3.
		///</summary>
		private const string PFX_HOW_TO="Make a .pfx with only the client certificate and its private key, and the office CA certificate without "
			+"its key, as docs/admin/db-connection-security.md section 3 does: openssl pkcs12 -export -in client-cert.pem -inkey client-key.pem -certfile "
			+"office-ca.pem -out client.pfx";
		///<summary>The tests only: called with the file's certificates right after ImportPfx stored their keys (PersistKeySet), before anything
		///else of the import; throwing there is the import failing after the keys were stored. Null in the tool.</summary>
		internal static Action<X509Certificate2Collection> ActionAfterKeysStoredForTests;
		///<summary>The tests only: called with the client certificate right before ImportPfx adds it to the store (not when the store already
		///holds it with a usable key, which stays as it is); a CryptographicException there is a store that refuses it. Null in the tool.</summary>
		internal static Action<X509Certificate2> ActionBeforeStoreAddForTests;
		///<summary>The tests only: called with each certificate right before its private key is deleted (TryDeletePrivateKey); throwing there is a
		///key that cannot be deleted. Null in the tool.</summary>
		internal static Action<X509Certificate2> ActionBeforeKeyDeleteForTests;
		///<summary>The tests only: called with the certificate each time the tool is about to open its private key (X509Certificate2.PrivateKey,
		///GetRSAPrivateKey, GetECDsaPrivateKey), so that a test sees which keys are opened: never one of a certificate of the store only to
		///compare key containers (ReadKeyProvInfo). Null in the tool.</summary>
		internal static Action<X509Certificate2> ActionBeforePrivateKeyOpenForTests;
		///<summary>The tests only: called with the certificate right before its key provider information is read (ReadKeyProvInfo); throwing
		///there is a property that cannot be read. Null in the tool.</summary>
		internal static Action<X509Certificate2> ActionBeforeKeyProvInfoReadForTests;
		///<summary>The tests only: called instead of the CAPI delete (PersistKeyInCsp=false and Clear) of DeleteCapiKey; doing nothing there is a
		///CRYPT_DELETEKEYSET that fails without a word, which the check after the delete must report. Null in the tool.</summary>
		internal static Action<RSACryptoServiceProvider> ActionInsteadOfCapiKeyDeleteForTests;
		///<summary>The tests only: called instead of GrantUsersReadToPrivateKey with each certificate whose private key the tool would give
		///BUILTIN\Users read access to, also for the tests' store of the current user, where the tool grants nothing otherwise; so a test sees
		///which keys the tool makes readable for the users in LocalMachine\My (never one it did not create). Throwing there is a grant that fails.
		///Null in the tool.</summary>
		internal static Action<X509Certificate2> ActionInsteadOfUsersReadGrantForTests;
		///<summary>The tests only: called with the store's entry instead of writing its earlier key provider information back (PutBackEarlierKey);
		///throwing there is a store that refuses the put-back, doing nothing is one that drops it without a word. Null in the tool.</summary>
		internal static Action<X509Certificate2> ActionInsteadOfKeyProvInfoPutBackForTests;
		///<summary>The tests only: called with the key provider information of the tool's key right before it is deleted by its name
		///(TryDeleteKeyNamed: a key the store's entry no longer names, after a put-back, say), once the key has opened; throwing there is a key
		///that cannot be deleted, which stays. Null in the tool.</summary>
		internal static Action<CertificateKeyProvInfo> ActionBeforeKeyNamedDeleteForTests;
		///<summary>The tests only: called with the key provider information of each key the tool is about to delete by its name (DeleteToolKey),
		///before anything looks for it or opens it, so that a test sees which keys the tool takes for its own to delete by name (never one it did
		///not create). Throwing there is a key that cannot be deleted, which stays. Null in the tool.</summary>
		internal static Action<CertificateKeyProvInfo> ActionBeforeKeyDeleteByNameForTests;
		///<summary>The tests only: called right before PutBackEarlierKey reads the store anew to check the entry it wrote the earlier key
		///provider information back onto; throwing there is a store that cannot be read again (DCFU4 lead fix round, decision 5). Null in the
		///tool.</summary>
		internal static Action ActionBeforePutBackCheckForTests;
		///<summary>The tests only: the name of a store of the current user (the tests' ClientCertificateTestStore) that the tool's public methods
		///(ImportPfx, RemoveImported, FindInStore) use instead of LocalMachine\My, so that DbSettingsRunner.Apply can be tested without the
		///computer's store; the import then uses the user's key set and gives no read access, as the internal overloads do for such a store.
		///Null in the tool.</summary>
		internal static string StoreNameForTests;

		///<summary>Opens the .pfx without storing its key anywhere (EphemeralKeySet) and returns its client certificate (GetClientCertificate), to
		///check the file and its password before anything is imported. Throws DbSettingsException (exit code 2).</summary>
		public static X509Certificate2 LoadPfxForCheck(string pfxPath,string password) {
			X509Certificate2Collection x509Certificate2Collection=ImportCollection(pfxPath,password,X509KeyStorageFlags.EphemeralKeySet);
			return GetClientCertificate(x509Certificate2Collection,pfxPath);
		}

		///<summary>Imports the client certificate of the .pfx (GetClientCertificate) into LocalMachine\My (private key not exportable) and grants
		///BUILTIN\Users read access to its private key. Reading the file with PersistKeySet stores the key of every certificate of the file that
		///has one; the import deletes again the ones it does not use, as DirectCertificateImport does: the keys of the file's other certificates
		///(its CA's, for example), and every key it stored, the client certificate's too, when the store refuses the certificate, or when the
		///store already holds it with a usable key in the machine key set that is the tool's own (its entry then stays as it is, with that key:
		///KeepCertificateInStore). The tool's own (IsEntryKeyTools): the key this import stored, or the key the tool's record names
		///(importedCertificateRecorded, ImportedCertificateRecord.ReadRecord; null for none) for that certificate; a record of the old format names
		///no key, and then its thumbprint alone makes the entry's key the tool's, as before round 4. A usable key the tool did not create, also one
		///an administrator gave a recorded certificate by hand, is left untouched, and never made readable for the users: the entry takes the
		///import's key, as one with a key in another key set does (IsEntryKeyUsable). Never a key that a certificate of the store uses
		///(DeleteKeysNotInStore). Returns its thumbprint; warning is "" or says what the program's users may lack, which key could not be deleted,
		///and which earlier key of the store's entry was left. isStoreKeyImported: the store's entry uses the private key this import stored, so
		///the tool made the key the certificate now has (DbSettingsRunner then records it). entryBefore: whether the store held the certificate
		///before, and whether its entry's earlier key was left aside (the warning then says where). importedCertificate: what the record names for
		///this certificate once the save records it (its key, and the entry's earlier key provider information that the import left aside, which
		///the undo or a later removal puts back: RemoveImported). warningEntryKeyLeft: the sentence that says where the store entry's earlier key,
		///which the import left aside, is ("" for none), which this overload gives apart from warning: an undo that puts that key back on the
		///entry leaves the sentence out, since the entry uses that key again (DbSettingsRunner). Needs administrator rights. Throws
		///DbSettingsException (exit code 2 for the file, 3 for the store), whose message also names a key that could not be deleted again.
		///</summary>
		public static string ImportPfx(string pfxPath,string password,ImportedCertificate importedCertificateRecorded,out string warning,
			out string warningEntryKeyLeft,out bool isStoreKeyImported,out ClientCertificateEntryBefore entryBefore,
			out ImportedCertificate importedCertificate)
		{
			return ImportPfx(pfxPath,password,GetToolStoreLocation(),GetToolStoreName(),importedCertificateRecorded,out warning,out warningEntryKeyLeft,
				out isStoreKeyImported,out entryBefore,out importedCertificate);
		}

		///<summary>ImportPfx into the store with this location and name, with no record and without isStoreKeyImported.</summary>
		internal static string ImportPfx(string pfxPath,string password,StoreLocation storeLocation,string storeName,out string warning) {
			return ImportPfx(pfxPath,password,storeLocation,storeName,"",out warning,out bool isStoreKeyImported);
		}

		///<summary>ImportPfx into the store with this location and name, with no record.</summary>
		internal static string ImportPfx(string pfxPath,string password,StoreLocation storeLocation,string storeName,out string warning,
			out bool isStoreKeyImported)
		{
			return ImportPfx(pfxPath,password,storeLocation,storeName,"",out warning,out isStoreKeyImported);
		}

		///<summary>ImportPfx into the store with this location and name, with a record of this thumbprint that names no key (as one of the old
		///format; "" for none). The tool's is LocalMachine\My: the machine key set, and the users' read access to the key. The tests use a store of
		///the current user with a name of its own, which they make and delete: the current user's key set, and no read access for the users (the
		///key is the user's own), as DirectCertificateImport does for a store of the current user.</summary>
		internal static string ImportPfx(string pfxPath,string password,StoreLocation storeLocation,string storeName,string thumbprintRecorded,
			out string warning,out bool isStoreKeyImported)
		{
			ImportedCertificate importedCertificateRecorded=string.IsNullOrWhiteSpace(thumbprintRecorded) ? null
				: ImportedCertificate.FromThumbprint(thumbprintRecorded);
			string thumbprint=ImportPfx(pfxPath,password,storeLocation,storeName,importedCertificateRecorded,out string warningRest,
				out string warningEntryKeyLeft,out isStoreKeyImported,out ClientCertificateEntryBefore entryBefore,out ImportedCertificate importedCertificate);
			warning=AppendSentence(warningEntryKeyLeft,warningRest);//The whole warning, the entry's earlier key first.
			return thumbprint;
		}

		///<summary>ImportPfx into the store with this location and name (see the public ImportPfx, which also says what warningEntryKeyLeft is,
		///apart from warning).</summary>
		internal static string ImportPfx(string pfxPath,string password,StoreLocation storeLocation,string storeName,
			ImportedCertificate importedCertificateRecorded,out string warning,out string warningEntryKeyLeft,out bool isStoreKeyImported,
			out ClientCertificateEntryBefore entryBefore,out ImportedCertificate importedCertificate)
		{
			X509KeyStorageFlags x509KeyStorageFlags=X509KeyStorageFlags.PersistKeySet;
			if(storeLocation==StoreLocation.LocalMachine) {
				x509KeyStorageFlags|=X509KeyStorageFlags.MachineKeySet;//The program's users on this computer use the key, not only this user.
			}
			else {
				x509KeyStorageFlags|=X509KeyStorageFlags.UserKeySet;
			}
			string storeText=storeLocation+"\\"+storeName;
			using(X509Store x509Store=new X509Store(storeName,storeLocation)) {
				//Opened before any key is stored: a store that cannot be opened leaves nothing behind.
				try {
					x509Store.Open(OpenFlags.ReadWrite);
				}
				catch(Exception ex) when(IsStoreException(ex)) {
					throw new DbSettingsException(DbSettingsRunner.EXIT_WRITE_FAILED,"The client certificate could not be added to the computer's "
						+"certificate store ("+storeText+"): "+ex.Message.Trim());
				}
				X509Certificate2Collection x509Certificate2Collection=ImportCollection(pfxPath,password,x509KeyStorageFlags);
				try {
					return AddClientCertificate(x509Store,storeText,x509Certificate2Collection,pfxPath,storeLocation==StoreLocation.LocalMachine,
						importedCertificateRecorded,out warning,out warningEntryKeyLeft,out isStoreKeyImported,out entryBefore,out importedCertificate);
				}
				finally {
					ResetAll(x509Certificate2Collection);//The keys in use stay in the key set (PersistKeySet); only these objects are released.
				}
			}
		}

		///<summary>ImportPfx after the file's keys were stored: adds the client certificate to the open store, deletes the keys of the file's
		///other certificates, and gives the users read access to its key (isMachine, which is also the key set the import stored the keys in).
		///warningEntryKeyLeft: the sentence that says where the store entry's earlier key, which the import left aside, is ("" for none), apart
		///from warning, which says everything else. A
		///client certificate that the store already holds with a usable key in that key set that is the tool's own (IsEntryKeyTools) is not added
		///again (KeepCertificateInStore). When anything fails before the certificate is in the store, every key the import stored is deleted
		///again and DbSettingsException says why, and which key could not be deleted. The catch lets the critical exceptions through
		///(IsCriticalException), where DirectCertificateImport.AddPrivate catches every one: docs/port-notes/DIRECT-CERT-FOLLOWUP.md, last round,
		///item 5. importedCertificate: what the record is to name (see the public ImportPfx).</summary>
		private static string AddClientCertificate(X509Store x509Store,string storeText,X509Certificate2Collection x509Certificate2Collection,
			string pfxPath,bool isMachine,ImportedCertificate importedCertificateRecorded,out string warning,out string warningEntryKeyLeft,
			out bool isStoreKeyImported,out ClientCertificateEntryBefore entryBefore,out ImportedCertificate importedCertificate)
		{
			warning="";
			warningEntryKeyLeft="";
			isStoreKeyImported=false;
			entryBefore=ClientCertificateEntryBefore.None;
			importedCertificate=null;
			//Every certificate of the file with a private key: the import just stored its key, in a key container of its own (a key that two
			//certificates of the file share, in one).
			List<X509Certificate2> listKeysStored=x509Certificate2Collection.Cast<X509Certificate2>().Where(x => x.HasPrivateKey).ToList();
			X509Certificate2 x509Certificate2;
			X509Certificate2 x509Certificate2InStore=null;
			//The store's entry's earlier key, when it is in another key set, cannot be identified, or is a usable key the tool did not create: left
			//untouched, and the warning says where.
			string warningEntryKey="";
			//The kept entry's key is the one this import stored (IsEntryKeyTools).
			bool isEntryKeyImported=false;
			//The store entry's key provider information as it was before the import, read without opening the key; null without an entry.
			CertificateKeyProvInfo keyProvInfoBefore=null;
			//The tool's record names this certificate.
			bool isRecorded=false;
			//An earlier key left aside goes into the record exactly as it was read (KeyBefore below), so that the undo or a later save names it on
			//the entry again: the warning then must not offer to delete it (DCFU4 second fix round, review finding 2). Only together with the key
			//this import gave the entry (KeyTool): a record with an earlier key but no tool key leaves the tool unable to tell whether the key its
			//entry names later is its own (DCFU4 lead fix round, decision 1).
			bool isEarlierKeyRecorded=false;
			//The key provider information of the key this import stored for the client certificate, read once, without opening the key.
			CertificateKeyProvInfo keyProvInfoImport=null;
			try {
				ActionAfterKeysStoredForTests?.Invoke(x509Certificate2Collection);
				x509Certificate2=GetClientCertificate(x509Certificate2Collection,pfxPath);
				keyProvInfoImport=ReadKeyProvInfo(x509Certificate2);
				isRecorded=importedCertificateRecorded!=null && string.Equals(DbConnectionConfig.NormalizeThumbprint(importedCertificateRecorded.Thumbprint),
					x509Certificate2.Thumbprint,StringComparison.OrdinalIgnoreCase);
				x509Certificate2InStore=FindInStoreNow(x509Store,x509Certificate2.Thumbprint);
				if(x509Certificate2InStore!=null) {
					entryBefore=ClientCertificateEntryBefore.InStore;
					keyProvInfoBefore=ReadKeyProvInfo(x509Certificate2InStore);
				}
				isEarlierKeyRecorded=keyProvInfoBefore!=null && keyProvInfoBefore.IsFieldsRead && IsKeyNamedAndKnown(keyProvInfoImport);
				if(x509Certificate2InStore!=null && !IsEntryKeyUsable(x509Certificate2InStore,isMachine,storeText,isEarlierKeyRecorded,out warningEntryKey)) {
					if(warningEntryKey=="" && IsEntryKeyNotOpenedForeign(keyProvInfoBefore,x509Certificate2,isRecorded ? importedCertificateRecorded : null,
						isMachine))
					{
						//A key in the import's key set that does not open and that the tool did not create (a smart card that is not inserted, a key
						//that needs a PIN or that the account may not open, a deleted key file): left aside like any other key the tool did not create,
						//and put back by the undo or a later removal (the owner's answer (b) of 2026-11-13).
						warningEntryKey=GetEntryKeyNotOpenedText(keyProvInfoBefore,storeText,isEarlierKeyRecorded);
					}
					x509Certificate2InStore.Reset();
					x509Certificate2InStore=null;
				}
				if(x509Certificate2InStore!=null && !IsEntryKeyTools(x509Certificate2InStore,x509Certificate2,isRecorded ? importedCertificateRecorded : null,
					isMachine,storeText,isEarlierKeyRecorded,out warningEntryKey,out isEntryKeyImported))
				{
					x509Certificate2InStore.Reset();
					x509Certificate2InStore=null;
				}
				if(x509Certificate2InStore==null) {
					ActionBeforeStoreAddForTests?.Invoke(x509Certificate2);
					x509Store.Add(x509Certificate2);
				}
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				x509Certificate2InStore?.Reset();
				//Nothing was added: every key the import stored goes again, but none a certificate of the store uses.
				string notDeleted=GetKeysNotDeletedText(listKeysStored,x509Store,new List<X509Certificate2>(),
					"A private key the tool stored could not be deleted again");
				if(ex is DbSettingsException) {
					throw new DbSettingsException(((DbSettingsException)ex).ExitCode,ex.Message+notDeleted);
				}
				if(IsStoreException(ex)) {
					throw new DbSettingsException(DbSettingsRunner.EXIT_WRITE_FAILED,"The client certificate could not be added to the computer's "
						+"certificate store ("+storeText+"): "+EndSentence(ex.Message)+notDeleted);
				}
				throw new DbSettingsException(DbSettingsRunner.EXIT_WRITE_FAILED,"The client certificate could not be imported: "
					+EndSentence(ex.Message)+notDeleted);
			}
			importedCertificate=ImportedCertificate.FromThumbprint(x509Certificate2.Thumbprint);
			if(x509Certificate2InStore!=null) {
				try {
					//The entry stays with its key, the tool's own: the record names that key (an old-format record is rewritten so), and the earlier
					//key provider information it recorded, if any, stays recorded. A record with an earlier key but no tool key (an import before the
					//DCFU4 lead fix round that could not read its own key), or an entry whose key cannot be read, keeps the record as it was: the tool
					//cannot tell which key is its own, so it records none as its own.
					if(IsKeyNamedAndKnown(keyProvInfoBefore) && !(isRecorded && IsToolKeyUnknown(importedCertificateRecorded))) {
						importedCertificate.KeyTool=keyProvInfoBefore;
						importedCertificate.KeyBefore=isRecorded ? importedCertificateRecorded.KeyBefore : null;
					}
					else if(isRecorded) {
						importedCertificate.KeyTool=importedCertificateRecorded.KeyTool;
						importedCertificate.KeyBefore=importedCertificateRecorded.KeyBefore;
					}
					return KeepCertificateInStore(x509Store,x509Certificate2InStore,listKeysStored,isMachine,isEntryKeyImported,out warning,
						out isStoreKeyImported);
				}
				finally {
					x509Certificate2InStore.Reset();
				}
			}
			isStoreKeyImported=true;//Added: the store's entry has the key this import stored.
			importedCertificate.KeyTool=IsKeyNamedAndKnown(keyProvInfoImport) ? keyProvInfoImport : null;
			if(warningEntryKey!="") {
				entryBefore=ClientCertificateEntryBefore.InStoreKeyLeft;//The entry's earlier key is left; the warning says where.
				//Exactly as it was, for the undo or a later removal to put back (RemoveImported); none when it could not be read at all, or when the
				//import's own key could not be read (isEarlierKeyRecorded), and the warning then says so.
				importedCertificate.KeyBefore=isEarlierKeyRecorded ? keyProvInfoBefore : null;
				if(!isEarlierKeyRecorded && keyProvInfoBefore!=null && keyProvInfoBefore.IsFieldsRead) {
					warningEntryKey=AppendSentence(warningEntryKey,GetImportKeyUnreadText(keyProvInfoImport,"the entry's earlier key"));
				}
			}
			else if(entryBefore==ClientCertificateEntryBefore.InStore && isRecorded) {
				//The tool's own entry without a usable key took the import's key: what the entry had before the tool's first import stays recorded,
				//together with the import's key; without that key the record keeps no earlier key, and the warning says so.
				if(importedCertificate.KeyTool!=null) {
					importedCertificate.KeyBefore=importedCertificateRecorded.KeyBefore;
				}
				else if(importedCertificateRecorded.KeyBefore!=null) {
					warningEntryKey=GetImportKeyUnreadText(keyProvInfoImport,"the key the entry had before the tool's first import ("
						+importedCertificateRecorded.KeyBefore.Describe()+")");
				}
			}
			warningEntryKeyLeft=warningEntryKey;//Apart from warning: an undo that puts that key back leaves it out (DbSettingsRunner).
			if(isRecorded && importedCertificateRecorded.KeyTool!=null && IsInToolKeySet(importedCertificateRecorded.KeyTool,isMachine)
				&& !CertificateKeyProvInfo.IsSameKeyStrict(importedCertificateRecorded.KeyTool,importedCertificate.KeyTool))
			{
				//The key the tool gave this certificate before is no longer its entry's (re-keyed by hand, or a key that no longer opens): the tool's
				//own, which the users may read, so it goes too, unless a certificate of the store names it. A recorded key outside the tool's key set
				//is not the tool's (the tool never makes one there), so it is never deleted.
				warning=AppendSentence(warning,DeleteToolKeyNamed(importedCertificateRecorded.KeyTool,x509Store,"The private key this tool gave the client "
					+"certificate "+x509Certificate2.Thumbprint+" before, which its entry no longer names, could not be deleted"));
			}
			//The file's other certificates with a key (its CA's, for example): their keys were just stored and are not used, unless the client
			//certificate, or a certificate of the store, uses the same key container.
			List<X509Certificate2> listUnused=listKeysStored.FindAll(x => x!=x509Certificate2);
			warning=AppendSentence(warning,GetKeysNotDeletedText(listUnused,x509Store,new List<X509Certificate2> { x509Certificate2 },
				"The client certificate was imported, but a private key of another certificate of the file, which the tool stored and does not use, "
				+"could not be deleted").Trim());
			if(IsUsersReadGranted(isMachine)) {
				try {
					GrantUsersRead(x509Certificate2);//The key this import stored.
				}
				catch(Exception ex) {
					warning=AppendSentence(warning,"The client certificate was imported, but the users of this computer may not be able to use "
						+"its private key: "+EndSentence(ex.Message)+" In certlm.msc, Personal > Certificates, right-click it, All Tasks > Manage "
						+"Private Keys, and give Users Read.");
				}
			}
			return x509Certificate2.Thumbprint;
		}

		///<summary>True when the import gives BUILTIN\Users read access to the key it keeps: for LocalMachine\My (isMachine), and for the tests'
		///store when they watch the grants (ActionInsteadOfUsersReadGrantForTests).</summary>
		private static bool IsUsersReadGranted(bool isMachine) {
			return isMachine || ActionInsteadOfUsersReadGrantForTests!=null;
		}

		///<summary>GrantUsersReadToPrivateKey, or the tests' ActionInsteadOfUsersReadGrantForTests in its place. Only ever for a key the tool
		///created: the one an import stored, or that of the certificate the tool's record names (IsEntryKeyTools).</summary>
		private static void GrantUsersRead(X509Certificate2 x509Certificate2) {
			if(ActionInsteadOfUsersReadGrantForTests!=null) {
				ActionInsteadOfUsersReadGrantForTests(x509Certificate2);
				return;
			}
			GrantUsersReadToPrivateKey(x509Certificate2);
		}

		///<summary>True when the store's entry of the client certificate, whose key is usable (IsEntryKeyUsable), has a key the tool created, so
		///that the entry can stay as it is and its key may get the users' read access (KeepCertificateInStore): the entry names the key container
		///this import stored the client certificate's key in, through the same provider (isEntryKeyImported, IsSameKeyStrict: a CAPI key whose
		///container the entry names, deleted before and stored there again by this import), or the tool's record names the certificate
		///(importedCertificateRecorded, null when it names another or none: the tool imported it before) and the key its import gave the entry
		///then (IsSameKeyStrict; DIRECT-CERT-FOLLOWUP round 4, part C). A record of the old format names no key: then the thumbprint alone makes
		///the entry's key the tool's, as before round 4 (ImportedCertificateRecord says why that is the safe side). Any other usable key is not the
		///tool's (an administrator put the certificate there with certlm.msc or Import-PfxCertificate, or gave a recorded certificate another key
		///by hand, say): the owner's rule of 2026-11-02 forbids touching it, so it never gets the users' read access; the entry takes the import's
		///key instead, and warningEntryKey says that the earlier key was left untouched and where it is, and either that the tool names it on the
		///entry again (isEarlierKeyRecorded: the record keeps it) or how to delete it. Read from the key provider information, without opening a
		///key. A recorded key outside the tool's key set (IsInToolKeySet) never makes the entry's key the tool's.</summary>
		private static bool IsEntryKeyTools(X509Certificate2 x509Certificate2InStore,X509Certificate2 x509Certificate2,
			ImportedCertificate importedCertificateRecorded,bool isMachine,string storeText,bool isEarlierKeyRecorded,out string warningEntryKey,
			out bool isEntryKeyImported)
		{
			warningEntryKey="";
			CertificateKeyProvInfo keyProvInfoEntry=ReadKeyProvInfo(x509Certificate2InStore);
			CertificateKeyProvInfo keyProvInfoImport=ReadKeyProvInfo(x509Certificate2);
			isEntryKeyImported=CertificateKeyProvInfo.IsSameKeyStrict(keyProvInfoEntry,keyProvInfoImport);
			if(isEntryKeyImported) {
				return true;
			}
			if(importedCertificateRecorded!=null && importedCertificateRecorded.KeyTool==null) {
				return true;//A record that names no key (the old format): the thumbprint decides, as before round 4.
			}
			if(importedCertificateRecorded!=null && IsInToolKeySet(importedCertificateRecorded.KeyTool,isMachine)
				&& CertificateKeyProvInfo.IsSameKeyStrict(keyProvInfoEntry,importedCertificateRecorded.KeyTool))
			{
				return true;
			}
			if(keyProvInfoEntry.IsUnknown) {
				//IsEntryKeyUsable already turned such an entry away; this keeps the warning true if that changes.
				warningEntryKey=GetEntryKeyLeftText(keyProvInfoEntry,isMachine,storeText,isEarlierKeyRecorded);
				return false;
			}
			if(importedCertificateRecorded!=null) {
				warningEntryKey=GetEntryKeyRekeyedText(keyProvInfoEntry,importedCertificateRecorded.KeyTool,isMachine,storeText,isEarlierKeyRecorded);
				return false;
			}
			warningEntryKey=GetEntryKeyNotToolsText(keyProvInfoEntry,isMachine,storeText,isEarlierKeyRecorded);
			return false;
		}

		///<summary>The warning for the store entry of a certificate the tool's record names, whose usable private key in the import's key set is
		///not the one the record names (IsEntryKeyTools): someone gave the entry that key by hand, so it is not the tool's; the import gave the
		///entry the key it stored, and left the entry's earlier key untouched, without giving the users read access to it, where it is
		///(GetEntryKeyWhereText).</summary>
		private static string GetEntryKeyRekeyedText(CertificateKeyProvInfo keyProvInfoEntry,CertificateKeyProvInfo keyProvInfoTool,bool isMachine,
			string storeText,bool isEarlierKeyRecorded)
		{
			return "The tool's record names the client certificate, but its entry in the computer's certificate store ("+storeText+") names another "
				+"private key than the one the tool gave it ("+keyProvInfoTool.Describe()+"): someone gave the entry that key by hand, so it is not the "
				+"tool's"+(isMachine ? ", and the tool gives the program's users no read access to it" : "")+". The import gave the entry the private key "
				+"it stored, and left the entry's earlier key untouched: "+GetEntryKeyWhereText(keyProvInfoEntry,isEarlierKeyRecorded);
		}

		///<summary>True for a key provider information that names a key the tool can record: read, with a key container.</summary>
		private static bool IsKeyNamedAndKnown(CertificateKeyProvInfo certificateKeyProvInfo) {
			return certificateKeyProvInfo!=null && certificateKeyProvInfo.IsNamed && !certificateKeyProvInfo.IsUnknown;
		}

		///<summary>True for a record that names the entry's earlier key but not the key the tool gave the entry (KeyBefore without KeyTool):
		///written before the DCFU4 lead fix round by an import that could not read its own key's provider information. The tool cannot tell
		///whether the key such an entry names is its own, so it never takes that key for its own to record or to delete (decision 1).</summary>
		internal static bool IsToolKeyUnknown(ImportedCertificate importedCertificate) {
			return importedCertificate!=null && importedCertificate.KeyTool==null && importedCertificate.KeyBefore!=null;
		}

		///<summary>The sentence for an import that could not read its own key's provider information (keyProvInfoImport), so that its record keeps
		///no earlier key (whatEarlier: "the entry's earlier key", say) together with no tool key (DCFU4 lead fix round, decision 1): the undo or a
		///later removal then removes the entry with the key it names, as for a record that names no key.</summary>
		private static string GetImportKeyUnreadText(CertificateKeyProvInfo keyProvInfoImport,string whatEarlier) {
			string why=(keyProvInfoImport==null || !keyProvInfoImport.IsNamed) ? "it names no private key"
				: (keyProvInfoImport.Problem??"").Trim().TrimEnd('.');
			return "The tool could not read which private key its import gave the entry ("+why+"), so its record does not keep "+whatEarlier+": "
				+"undoing this import, or a later save that stops using this certificate, removes the entry from the store with the key it names then, "
				+"and does not name the earlier key on it again.";
		}

		///<summary>True when the key provider information names a key in the key set the tool's imports use (isMachine: the machine's for
		///LocalMachine\My; the user's for the tests' store). The tool never creates a key anywhere else, so a recorded key outside it is never the
		///tool's (a record edited by hand), and no key outside it is ever claimed, made readable for the users or deleted by its name.</summary>
		private static bool IsInToolKeySet(CertificateKeyProvInfo certificateKeyProvInfo,bool isMachine) {
			return certificateKeyProvInfo!=null && certificateKeyProvInfo.IsMachine==isMachine;
		}

		///<summary>True when the store's entry of the client certificate can stay as it is (KeepCertificateInStore): its key provider information
		///names a key in the key set the import uses (the machine's for LocalMachine\My, isMachine; the user's for the tests' store), read without
		///opening the key, and only then the key opens and signs or decrypts (IsKeyUsable). An entry whose key is in another key set (an
		///administrator's own user key set, say, which the program's users cannot read) or cannot be identified is not usable, and
		///warningEntryKey says that the import gives the entry its own key and leaves the earlier one untouched, and where that one is
		///(GetEntryKeyWhereText, isEarlierKeyRecorded). An entry without a key, or with one that does not open, is not usable either, without a
		///warning here: AddClientCertificate then leaves aside, with a warning, a key that does not open and that the tool did not create
		///(IsEntryKeyNotOpenedForeign).</summary>
		private static bool IsEntryKeyUsable(X509Certificate2 x509Certificate2InStore,bool isMachine,string storeText,bool isEarlierKeyRecorded,
			out string warningEntryKey)
		{
			warningEntryKey="";
			CertificateKeyProvInfo keyProvInfoEntry=ReadKeyProvInfo(x509Certificate2InStore);
			if(!keyProvInfoEntry.IsNamed) {
				return false;
			}
			if(keyProvInfoEntry.IsUnknown || !IsInToolKeySet(keyProvInfoEntry,isMachine)) {
				warningEntryKey=GetEntryKeyLeftText(keyProvInfoEntry,isMachine,storeText,isEarlierKeyRecorded);
				return false;
			}
			return IsKeyUsable(x509Certificate2InStore);
		}

		///<summary>True when the store entry's key, which is in the import's key set and does not open (IsEntryKeyUsable gave no warning), is a key
		///the tool did not create, so that the import leaves it aside and the undo or a later removal puts it back (RemoveImported), as for a usable
		///key the tool did not create (the owner's answer (b) of 2026-11-13): a smart card that is not inserted, a TPM or other key that needs a
		///PIN, one whose rights keep this account out, or a deleted key file. Its key provider information was read before the import, field by
		///field (keyProvInfoBefore.IsFieldsRead, so that it can be written back exactly), and names a key container. Not the tool's own: not the
		///key the import just stored (x509Certificate2), and not the key the tool's record names (importedCertificateRecorded, null when the record
		///names another certificate or none) by IsSameKeyStrict; a record of the old format, which names no key, claims the entry's key by its
		///thumbprint alone, as before round 4 (ImportedCertificateRecord), so that entry is not left aside either. For the tool's own key that no
		///longer opens, the entry takes the import's key as before, the record keeps what the entry had before the tool's first import, and the dead
		///key is deleted by its name (DeleteToolKeyNamed). Without key provider information the entry had no key: as before, the whole entry is
		///the tool's to remove. A recorded key outside the tool's key set (isMachine, IsInToolKeySet) is never the tool's. Opens no key.</summary>
		private static bool IsEntryKeyNotOpenedForeign(CertificateKeyProvInfo keyProvInfoBefore,X509Certificate2 x509Certificate2,
			ImportedCertificate importedCertificateRecorded,bool isMachine)
		{
			if(keyProvInfoBefore==null || !keyProvInfoBefore.IsFieldsRead || !IsKeyNamedAndKnown(keyProvInfoBefore)) {
				return false;
			}
			if(CertificateKeyProvInfo.IsSameKeyStrict(keyProvInfoBefore,ReadKeyProvInfo(x509Certificate2))) {
				return false;//The key the import just stored: the tool's own.
			}
			if(importedCertificateRecorded!=null && importedCertificateRecorded.KeyTool==null) {
				return false;//A record of the old format: the thumbprint claims the entry's key, as before round 4.
			}
			if(importedCertificateRecorded!=null && IsInToolKeySet(importedCertificateRecorded.KeyTool,isMachine)
				&& CertificateKeyProvInfo.IsSameKeyStrict(keyProvInfoBefore,importedCertificateRecorded.KeyTool))
			{
				return false;//The key the tool gave this certificate before, which no longer opens.
			}
			return true;
		}

		///<summary>The warning for a store entry of the client certificate whose private key, in the import's key set, does not open and is not
		///the tool's (IsEntryKeyNotOpenedForeign): the import gave the entry the key it stored and left the entry's earlier key untouched, where it
		///is. No command to delete it: it may be on a smart card, or be a key that needs a PIN, which only its owner should delete; and the tool's
		///record keeps it (KeyBefore: this warning is given only for an entry whose key provider information was read field by field), so the
		///undo of a failed save, or a later save that stops using the certificate, names it on the entry again (RemoveImported), unless the
		///import could not read its own key (isEarlierKeyRecorded false: then the record keeps no earlier key, and AddClientCertificate says so).
		///</summary>
		private static string GetEntryKeyNotOpenedText(CertificateKeyProvInfo keyProvInfoEntry,string storeText,bool isEarlierKeyRecorded) {
			return "The client certificate was already in the computer's certificate store ("+storeText+"), but the private key its entry names "
				+"does not open for this tool (a key that was deleted, a smart card that is not inserted, a key that needs a PIN, or one this account "
				+"may not use), and the tool did not create it. The import gave the entry the private key it stored, and left the entry's earlier key, "
				+"wherever it is, untouched: provider "+keyProvInfoEntry.ProviderName+", key container "+keyProvInfoEntry.ContainerName+", in "
				+GetKeySetText(keyProvInfoEntry)+"."+(isEarlierKeyRecorded ? GetEarlierKeyRecordedText() : "");
		}

		///<summary>The sentence, after a space, that says that the tool's record keeps the entry's earlier key and names it on the entry again, so
		///that nobody deletes it meanwhile (DCFU4 second fix round, review finding 2): the entry would then name a key that is not there.</summary>
		private static string GetEarlierKeyRecordedText() {
			return " The tool's record keeps it, and the tool names that key on the entry again when this import is undone or a later save stops "
				+"using this certificate, so do not delete that key while the record names it.";
		}

		///<summary>The warning for a store entry of the client certificate whose private key is in another key set than the import's, or cannot be
		///identified (IsEntryKeyUsable): the import gave the entry the key it stored, and the entry's earlier key was left untouched, where it is
		///(GetEntryKeyWhereText, isEarlierKeyRecorded). The tool deletes no key it did not make.</summary>
		private static string GetEntryKeyLeftText(CertificateKeyProvInfo keyProvInfoEntry,bool isMachine,string storeText,bool isEarlierKeyRecorded) {
			string start="The client certificate was already in the computer's certificate store ("+storeText+"), ";
			if(keyProvInfoEntry.IsUnknown) {
				return start+"but the private key its entry names could not be identified: "+EndSentence(keyProvInfoEntry.Problem)+" The import gave "
					+"the entry the private key it stored, and left the entry's earlier key, if there is one, untouched."
					+(isEarlierKeyRecorded ? GetEarlierKeyRecordedText() : "");
			}
			string keySetEntry=GetKeySetText(keyProvInfoEntry);
			string keySetImport=isMachine ? "the computer's key set" : "the current user's key set";
			return start+"with its private key in "+keySetEntry+(isMachine ? ", which the program's users cannot use" : "")+", not in "+keySetImport
				+", which the import uses. The import gave the entry the private key it stored, and left the entry's earlier key untouched: "
				+GetEntryKeyWhereText(keyProvInfoEntry,isEarlierKeyRecorded);
		}

		///<summary>The warning for a store entry of the client certificate with a usable private key in the import's key set that the tool did
		///not create (IsEntryKeyTools): the import gave the entry the key it stored, and left the entry's earlier key untouched, without giving the
		///users read access to it, where it is (GetEntryKeyWhereText, isEarlierKeyRecorded).</summary>
		private static string GetEntryKeyNotToolsText(CertificateKeyProvInfo keyProvInfoEntry,bool isMachine,string storeText,bool isEarlierKeyRecorded) {
			return "The client certificate was already in the computer's certificate store ("+storeText+") with a usable private key that this "
				+"tool did not import"+(isMachine ? ", so the tool gives the program's users no read access to that key" : "")+". The import gave the "
				+"entry the private key it stored, and left the entry's earlier key untouched: "+GetEntryKeyWhereText(keyProvInfoEntry,isEarlierKeyRecorded);
		}

		///<summary>"the computer's key set", or the user key set of the account that put the entry's key there.</summary>
		private static string GetKeySetText(CertificateKeyProvInfo keyProvInfo) {
			return keyProvInfo.IsMachine ? "the computer's key set" : "the user key set of the Windows account that put it there";
		}

		///<summary>Where the entry's earlier key is: "provider ..., key container ..., in ....", then, when the import records it as the entry's
		///earlier key (isEarlierKeyRecorded: its key provider information was read field by field, ImportedCertificate.KeyBefore), that the tool
		///names it on the entry again, so it must not be deleted meanwhile (GetEarlierKeyRecordedText; the DCFU4 second fix round's review
		///finding 2: a deleted key would leave the restored entry naming a key that is not there). Only when nothing could be recorded, the
		///certutil command that deletes it: "Who can delete it, once no other certificate uses it: certutil ...".</summary>
		private static string GetEntryKeyWhereText(CertificateKeyProvInfo keyProvInfoEntry,bool isEarlierKeyRecorded) {
			string where="provider "+keyProvInfoEntry.ProviderName+", key container "+keyProvInfoEntry.ContainerName+", in "+GetKeySetText(keyProvInfoEntry)
				+".";
			if(isEarlierKeyRecorded) {
				return where+GetEarlierKeyRecordedText();
			}
			string who=keyProvInfoEntry.IsMachine ? "An administrator" : "That account";
			return where+" "+who+" can delete it, once no other certificate uses it: "+GetCertutilDeleteKeyText(keyProvInfoEntry)+".";
		}

		///<summary>AddClientCertificate for a client certificate that the store already holds with a usable key in the import's key set that is
		///the tool's own (IsEntryKeyTools; as when setup runs again with the same .pfx): the store's entry stays as it is, with its key, as
		///DirectCertificateImport leaves such a certificate; the store's add would give the entry the key the import just stored and leave the key
		///it had on the computer, named by no certificate and readable by the users. So every key this import stored is deleted again, the client
		///certificate's own too; DeleteKeysNotInStore keeps the entry's key, which a certificate of the store uses, so no key that was there
		///before is deleted. The users get read access to the entry's key (isMachine), the tool's own, as to an imported one: the program reads
		///the certificate as the signed-in user. Returns its thumbprint; warning is "" or says which key could not be deleted and what the users
		///may lack. isStoreKeyImported: isEntryKeyImported, the entry names the key container the import stored the client certificate's key in,
		///so the entry's key is the one this import made (one the record names is recorded already).</summary>
		private static string KeepCertificateInStore(X509Store x509Store,X509Certificate2 x509Certificate2InStore,List<X509Certificate2> listKeysStored,
			bool isMachine,bool isEntryKeyImported,out string warning,out bool isStoreKeyImported)
		{
			isStoreKeyImported=isEntryKeyImported;
			string start="The client certificate was already in the computer's certificate store with a usable private key and stays as it is, but ";
			warning=GetKeysNotDeletedText(listKeysStored,x509Store,new List<X509Certificate2>(),start+"a private key of the file, which the tool "
				+"stored and does not use, could not be deleted").Trim();
			if(IsUsersReadGranted(isMachine)) {
				try {
					GrantUsersRead(x509Certificate2InStore);//The tool's own key (IsEntryKeyTools).
				}
				catch(Exception ex) {
					warning=AppendSentence(warning,start+"the users of this computer may not be able to use its private key: "+EndSentence(ex.Message)
						+" In certlm.msc, Personal > Certificates, right-click it, All Tasks > Manage Private Keys, and give Users Read.");
				}
			}
			return x509Certificate2InStore.Thumbprint;
		}

		///<summary>The store's certificate with this thumbprint, or null, read anew through a handle of its own, as DeleteKeysNotInStore reads
		///the store: the caller's open handle may keep the certificates as it read them when it was opened. The caller resets it. Throws what
		///opening the store throws.</summary>
		private static X509Certificate2 FindInStoreNow(X509Store x509Store,string thumbprint) {
			using(X509Store x509StoreNow=new X509Store(x509Store.Name,x509Store.Location)) {
				x509StoreNow.Open(OpenFlags.ReadOnly|OpenFlags.OpenExistingOnly);
				X509Certificate2 x509Certificate2Found=null;
				foreach(X509Certificate2 x509Certificate2 in x509StoreNow.Certificates) {
					if(x509Certificate2Found==null && string.Equals(x509Certificate2.Thumbprint,thumbprint,StringComparison.OrdinalIgnoreCase)) {
						x509Certificate2Found=x509Certificate2;
						continue;
					}
					x509Certificate2.Reset();//The others are released at once.
				}
				return x509Certificate2Found;
			}
		}

		///<summary>Deletes the private keys of these certificates that no certificate of the store or of listInUse uses (DeleteKeysNotInStore),
		///and returns a sentence for each one that could not be deleted (start, the certificate, why, and where its key is left), each after a
		///space; "" when all were.</summary>
		private static string GetKeysNotDeletedText(List<X509Certificate2> listX509Certificate2s,X509Store x509Store,
			List<X509Certificate2> listInUse,string start)
		{
			//The files are named before the keys are deleted: a key that is half deleted may no longer say where it was. These are the keys the
			//tool stored or imported itself, which it opens to delete anyway.
			List<string> listPaths=listX509Certificate2s.Select(x => TryGetPrivateKeyFilePath(x)).ToList();
			List<KeyNotDeleted> listNotDeleted=DeleteKeysNotInStore(x509Store,listX509Certificate2s,listInUse);
			string text="";
			for(int i=0;i<listX509Certificate2s.Count;i++) {
				if(listNotDeleted[i]==null) {
					continue;
				}
				text+=" "+start+" ("+listX509Certificate2s[i].Subject+", "+listX509Certificate2s[i].Thumbprint+"): "+EndSentence(listNotDeleted[i].Why)
					+GetKeyLeftText(listPaths[i],listNotDeleted[i].IsMaybeInUse);
			}
			return text;
		}

		///<summary>Why a private key was not deleted (DeleteKeysNotInUse), and whether it was kept because a certificate of the store may use it
		///(its key could not be identified, or the store could not be read): then the user must not simply delete it.</summary>
		private sealed class KeyNotDeleted {
			public string Why;
			public bool IsMaybeInUse;

			public KeyNotDeleted(string why,bool isMaybeInUse) {
				Why=why;
				IsMaybeInUse=isMaybeInUse;
			}
		}

		///<summary>Removes the client certificate this tool imported itself (the record, importedCertificate) from LocalMachine\My and deletes the
		///private key the tool gave it, so that no key that BUILTIN\Users may read is left behind. Only for the client certificate the tool's record
		///names and the saved settings no longer use (DbSettingsRunner.PlanImportedCertificate, owner decision of 2026-11-02), or that a failed save
		///undoes: a certificate the tool did not import is never removed. Returns "" when it is gone (or was not there), else a warning that says
		///what is left and how to remove it by hand. Never throws.</summary>
		public static string RemoveImported(ImportedCertificate importedCertificate) {
			return RemoveImported(importedCertificate,GetToolStoreLocation(),GetToolStoreName(),out bool isEarlierKeyPutBack,out bool isRecordStillNeeded);
		}

		///<summary>RemoveImported, which also says whether the store's entry names its earlier key again (isEarlierKeyPutBack: the record's
		///KeyBefore was written back and read back on the entry, whatever became of the tool's key), so that the undo of a failed save can leave
		///out the import's warning about that key, which the entry uses again (DbSettingsRunner), and whether the record must still name the
		///certificate (isRecordStillNeeded: a put-back that failed, or could not be checked, while the entry may still name the key the tool gave
		///it, which the users can read; DCFU4 lead fix round, decision 2), so that the caller keeps it and a later save tries again.</summary>
		public static string RemoveImported(ImportedCertificate importedCertificate,out bool isEarlierKeyPutBack,out bool isRecordStillNeeded) {
			return RemoveImported(importedCertificate,GetToolStoreLocation(),GetToolStoreName(),out isEarlierKeyPutBack,out isRecordStillNeeded);
		}

		///<summary>RemoveImported of a record of this thumbprint that names no key (as one of the old format), for the store with this location
		///and name.</summary>
		internal static string RemoveImported(string thumbprint,StoreLocation storeLocation,string storeName) {
			if(string.IsNullOrEmpty(thumbprint)) {
				return "";
			}
			return RemoveImported(ImportedCertificate.FromThumbprint(thumbprint),storeLocation,storeName);
		}

		///<summary>RemoveImported for the store with this location and name: the tool's is LocalMachine\My; the tests use a store of the current
		///user with a name of its own, which they make and delete, never the computer's or the user's real stores. The store's entry is the
		///tool's only when its key is the one the record names (IsSameKeyStrict, read without opening it), or the record names no key (the old
		///format: the thumbprint decides, as before round 4). Then the certificate goes first, then its key, as DirectCertificateImport removes a
		///replaced certificate (RemoveReplaced, and Undo): a certificate the store does not let go of keeps a key that works, and the key is
		///deleted only when no certificate left in the store uses the same key container (DeleteKeysNotInStore). When only the key could not be
		///deleted, the warning says that the certificate was removed and where its key file is, which certlm.msc no longer shows. An entry that
		///names another key (someone gave the certificate another key by hand) is not the tool's: it stays, with that key, the warning names it,
		///and only the key the record names is deleted, unless a certificate of the store names it (DeleteToolKeyNamed); so is that key when the
		///certificate is no longer in the store. A record whose key is outside the key set the tool imports into (IsInToolKeySet: a record edited
		///by hand, since the tool never makes such a key) claims nothing: the certificate and every key stay, and the warning says so.</summary>
		internal static string RemoveImported(ImportedCertificate importedCertificate,StoreLocation storeLocation,string storeName) {
			return RemoveImported(importedCertificate,storeLocation,storeName,out bool isEarlierKeyPutBack,out bool isRecordStillNeeded);
		}

		///<summary>RemoveImported for the store with this location and name (see above), which also says whether the store's entry names its
		///earlier key again (isEarlierKeyPutBack, PutBackEarlierKey), and whether the record must still name the certificate
		///(isRecordStillNeeded, see the public overload). A record that names the entry's earlier key but not the tool's (IsToolKeyUnknown) gets
		///the earlier key put back and no key deleted: the tool cannot tell whether the key the entry names is its own (DCFU4 lead fix round,
		///decision 1). An entry that names the record's earlier key already (a put-back that could not be checked before) is the tool's put-back:
		///only the tool's key is deleted.</summary>
		internal static string RemoveImported(ImportedCertificate importedCertificate,StoreLocation storeLocation,string storeName,
			out bool isEarlierKeyPutBack,out bool isRecordStillNeeded)
		{
			isEarlierKeyPutBack=false;
			isRecordStillNeeded=false;
			string thumbprint=importedCertificate==null ? "" : DbConnectionConfig.NormalizeThumbprint(importedCertificate.Thumbprint);
			if(thumbprint=="") {
				return "";
			}
			string storeText=storeLocation+"\\"+storeName;
			string startToolKey="The private key this tool gave the client certificate "+thumbprint+" could not be deleted";
			try {
				using(X509Store x509Store=new X509Store(storeName,storeLocation)) {
					x509Store.Open(OpenFlags.ReadWrite|OpenFlags.OpenExistingOnly);
					X509Certificate2Collection x509Certificate2CollectionFound=x509Store.Certificates.Find(X509FindType.FindByThumbprint,
						thumbprint,false);
					try {
						if(importedCertificate.KeyTool!=null && !IsInToolKeySet(importedCertificate.KeyTool,storeLocation==StoreLocation.LocalMachine)) {
							//A key outside the key set the tool imports into, which the tool never creates (a record edited by hand): nothing of it is the
							//tool's, so nothing is removed or deleted.
							if(x509Certificate2CollectionFound.Count==0) {
								return "";
							}
							return GetRecordKeyOutsideKeySetText(thumbprint,storeText,importedCertificate.KeyTool);
						}
						if(x509Certificate2CollectionFound.Count==0) {
							//No longer in the store (removed by hand, which can leave its key): the key the tool gave it goes, unless another names it.
							return DeleteToolKeyNamed(importedCertificate.KeyTool,x509Store,startToolKey);
						}
						X509Certificate2 x509Certificate2Entry=x509Certificate2CollectionFound[0];
						CertificateKeyProvInfo keyProvInfoEntry=ReadKeyProvInfo(x509Certificate2Entry);
						if(importedCertificate.KeyTool!=null && importedCertificate.KeyBefore!=null && keyProvInfoEntry.IsFieldsRead
							&& !keyProvInfoEntry.IsUnknown && IsPutBack(keyProvInfoEntry,importedCertificate.KeyBefore))
						{
							//The entry names its earlier key already: an earlier put-back whose check could not read the store (decision 5). The tool's
							//key, which no certificate names now, goes.
							isEarlierKeyPutBack=true;
							return GetPutBackDoneText(DeleteToolKey(importedCertificate.KeyTool,x509Store),importedCertificate.KeyTool,importedCertificate.KeyBefore,
								thumbprint,storeText);
						}
						if(importedCertificate.KeyTool!=null && !CertificateKeyProvInfo.IsSameKeyStrict(keyProvInfoEntry,importedCertificate.KeyTool)) {
							return AppendSentence(GetEntryNotToolsText(thumbprint,storeText,keyProvInfoEntry,importedCertificate.KeyTool),
								DeleteToolKeyNamed(importedCertificate.KeyTool,x509Store,startToolKey));
						}
						if(importedCertificate.KeyBefore!=null) {
							//The store held the certificate before the tool's import, with an earlier key the import left aside: the entry stays, and
							//gets that key back (DIRECT-CERT-FOLLOWUP round 4, part B).
							//Without a tool key in the record (IsToolKeyUnknown), the key the entry names now may be the tool's or someone else's: none is
							//deleted (decision 1).
							return PutBackEarlierKey(x509Store,x509Certificate2Entry,keyProvInfoEntry,importedCertificate.KeyTool!=null,
								importedCertificate.KeyBefore,thumbprint,storeText,out isEarlierKeyPutBack,out isRecordStillNeeded);
						}
						List<X509Certificate2> listRemoved=new List<X509Certificate2>();
						foreach(X509Certificate2 x509Certificate2 in x509Certificate2CollectionFound) {
							x509Store.Remove(x509Certificate2);
							listRemoved.Add(x509Certificate2);
						}
						//Once it is out of the store: another certificate of the store may use the same key container.
						return GetKeysNotDeletedText(listRemoved,x509Store,new List<X509Certificate2>(),"The client certificate that this tool "
							+"imported before was removed from the computer's certificate store ("+storeText+"), but its private key could not be "
							+"deleted").Trim();
					}
					finally {
						ResetAll(x509Certificate2CollectionFound);
					}
				}
			}
			catch(Exception ex) when(IsStoreException(ex) || ex is IOException) {
				//The certificate and its key are both still there. certlm.msc's Delete would leave the key on the computer. A record with an earlier
				//key to put back is kept, so that a later save tries again (DbSettingsRunner: decision 2).
				isRecordStillNeeded=importedCertificate.KeyBefore!=null;
				return "The client certificate "+thumbprint+" that this tool imported before could not be removed from the computer's "
					+"certificate store ("+storeText+"): "+EndSentence(ex.Message)+" Remove it with its private key as an administrator, in "
					+"PowerShell: Remove-Item Cert:\\"+storeText+"\\"+thumbprint+" -DeleteKey (certlm.msc's Delete leaves the key).";
			}
		}

		///<summary>RemoveImported of an entry the store held before the tool's import, whose earlier key the import left aside (the owner's answer
		///(b) of 2026-11-13: the computer is left exactly as the administrator had it): writes the entry's earlier key provider information back
		///onto the store's entry (keyProvInfoBefore, exactly the fields read before the import: CertificateKeyProvInfo.Write, which is
		///CertSetCertificateContextProperty on the store's certificate context), reads the entry anew to check it, and only then deletes the key
		///the tool gave it (keyProvInfoEntry, the entry's key until now, when isToolKeyKnown: the record names it as the tool's; DeleteToolKey,
		///unless a certificate of the store names it). Without a tool key in the record (isToolKeyKnown false, DCFU4 lead fix round, decision 1)
		///no key is deleted, and the text names the key the entry named until then, which may be the users-readable key the tool gave it, and how
		///an administrator deletes it. The earlier key itself is never opened, altered or deleted, and the entry stays in the store. When the
		///put-back fails, the tool's key is deleted only if the entry no longer names it (then the certificate would otherwise name a key that the
		///users can read and no record names); while the entry still names it, it stays so that the certificate keeps working, the text says what
		///is left and how to put it right, and isRecordStillNeeded asks the caller to keep the record, so that a later save tries again (decision
		///2). When the store cannot be read again to check the entry, the tool cannot tell which key the entry names: no key is deleted (the
		///certificate could otherwise name a key that is gone), the text says so, and the record is kept (decision 5). The tool's key is deleted
		///first and the text then says what became of it (DeleteToolKey: deleted, not there, kept because another certificate of the store names
		///it, not known, or not deleted and why). isPutBack: the entry names its earlier key again. Returns what was done, or what is left. Never
		///throws.</summary>
		private static string PutBackEarlierKey(X509Store x509Store,X509Certificate2 x509Certificate2Entry,CertificateKeyProvInfo keyProvInfoEntry,
			bool isToolKeyKnown,CertificateKeyProvInfo keyProvInfoBefore,string thumbprint,string storeText,out bool isPutBack,out bool isRecordStillNeeded)
		{
			isPutBack=false;
			isRecordStillNeeded=false;
			CertificateKeyProvInfo keyProvInfoTool=isToolKeyKnown ? keyProvInfoEntry : null;
			string start=GetPutBackStartText(thumbprint,storeText);
			string why=null;
			try {
				ActionInsteadOfKeyProvInfoPutBackForTests?.Invoke(x509Certificate2Entry);
				if(ActionInsteadOfKeyProvInfoPutBackForTests==null) {
					keyProvInfoBefore.Write(x509Certificate2Entry);
				}
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				why=ex.Message.Trim();
			}
			CertificateKeyProvInfo keyProvInfoNow=null;
			string whyNotChecked=null;
			try {
				ActionBeforePutBackCheckForTests?.Invoke();
				using(X509Certificate2 x509Certificate2Now=FindInStoreNow(x509Store,thumbprint)) {
					keyProvInfoNow=x509Certificate2Now==null ? null : ReadKeyProvInfo(x509Certificate2Now);
				}
			}
			catch(Exception ex) when(IsStoreException(ex)) {
				whyNotChecked=ex.Message.Trim();
			}
			string howToCheck=" Check the entry as an administrator (certutil "+(x509Store.Location==StoreLocation.CurrentUser ? "-user " : "")+"-store \""
				+x509Store.Name+"\" "+thumbprint+") and, if it does not name its earlier key, import the certificate with that key again.";
			if(whyNotChecked!=null) {
				//Whatever the write did, the tool does not know which key the entry names now: it deletes none, and the record is kept.
				isRecordStillNeeded=true;
				return GetPutBackNotCheckedText(thumbprint,storeText,keyProvInfoBefore,keyProvInfoEntry,isToolKeyKnown,why,whyNotChecked)+howToCheck;
			}
			if(why==null && keyProvInfoNow!=null && IsPutBack(keyProvInfoNow,keyProvInfoBefore)) {
				isPutBack=true;
				if(!isToolKeyKnown) {
					return start+": its entry names its earlier private key again ("+keyProvInfoBefore.Describe()+"), as before that import. "
						+GetToolKeyUnknownText(keyProvInfoEntry)+GetPutBackHedgeText(storeText,thumbprint);
				}
				return GetPutBackDoneText(DeleteToolKey(keyProvInfoTool,x509Store),keyProvInfoTool,keyProvInfoBefore,thumbprint,storeText);
			}
			if(why==null) {
				why="Windows reported no error, but the entry does not name it afterwards.";
			}
			string notPutBack=start+", but its earlier private key ("+keyProvInfoBefore.Describe()+") could not be named on its entry again: "+EndSentence(why);
			if(keyProvInfoNow!=null && CertificateKeyProvInfo.IsSameKeyStrict(keyProvInfoNow,keyProvInfoEntry)) {
				//The entry still names the key it named until now: it stays, so that the certificate still works, and so does the record, so that a
				//later save tries again (decision 2).
				isRecordStillNeeded=true;
				if(!isToolKeyKnown) {
					return notPutBack+" The entry still names the private key it named until then ("+keyProvInfoEntry.Describe()+"), which may be the one "
						+"the tool gave it (the tool's record does not say which key that was), so the tool left both keys and the entry as they are."
						+howToCheck;
				}
				return notPutBack+" The entry still names the private key the tool gave it ("+keyProvInfoTool.Describe()+"), which the computer's users "
					+"can read, so the tool left both keys and the entry as they are. To put the entry back as it was, as an administrator, delete the "
					+"tool's key ("+GetCertutilDeleteKeyText(keyProvInfoTool)+"), then import the certificate with its earlier key again.";
			}
			string entryNow=(keyProvInfoNow==null) ? "is no longer in the store"
				: "names "+(keyProvInfoNow.IsNamed ? (keyProvInfoNow.IsUnknown ? "a private key that could not be identified" : keyProvInfoNow.Describe()) : "no private key");
			if(!isToolKeyKnown) {
				return notPutBack+" The entry now "+entryNow+". "+GetToolKeyUnknownText(keyProvInfoEntry)+howToCheck;
			}
			//The entry no longer names the tool's key, which the users can read and no record will name: it goes, unless another certificate names it.
			ToolKeyDeletion toolKeyDeletionLeft=DeleteToolKey(keyProvInfoTool,x509Store);
			return notPutBack+" The entry now "+entryNow+". "+GetToolKeyFateText(toolKeyDeletionLeft,keyProvInfoTool)+howToCheck;
		}

		///<summary>"The client certificate T was in the computer's certificate store (S) before the tool's import gave it a private key": how every
		///put-back text begins.</summary>
		private static string GetPutBackStartText(string thumbprint,string storeText) {
			return "The client certificate "+thumbprint+" was in the computer's certificate store ("+storeText+") before the tool's import gave it a "
				+"private key";
		}

		///<summary>The text of a put-back the entry was checked for (or found done): the entry names its earlier key again, what became of the
		///tool's key (only a key DeleteToolKey deleted is said to be deleted), and what to do if the earlier key was deleted since.</summary>
		private static string GetPutBackDoneText(ToolKeyDeletion toolKeyDeletion,CertificateKeyProvInfo keyProvInfoTool,CertificateKeyProvInfo keyProvInfoBefore,
			string thumbprint,string storeText)
		{
			string putBack=GetPutBackStartText(thumbprint,storeText)+": its entry names its earlier private key again ("+keyProvInfoBefore.Describe()
				+"), as before that import";
			//The earlier key is not opened to check that it is still there: it may be on a smart card or a TPM, or need a PIN.
			string hedge=GetPutBackHedgeText(storeText,thumbprint);
			if(toolKeyDeletion.ToolKeyFate==ToolKeyFate.Deleted) {
				return putBack+", and the private key the tool gave it was deleted."+hedge;
			}
			return putBack+". "+GetToolKeyFateText(toolKeyDeletion,keyProvInfoTool)+hedge;
		}

		///<summary>The text of a put-back whose entry could not be checked, because the store could not be read again (whyNotChecked; the DCFU4
		///lead fix round, decision 5): the earlier key was written on the entry (whyWrite null) or the write failed (whyWrite), and the tool
		///cannot tell whether the entry names that key or still the one it named until then (keyProvInfoEntry: the tool's, when isToolKeyKnown),
		///so it deleted no key and left the entry as it is. Never that the entry "is no longer in the store": that is not known.</summary>
		internal static string GetPutBackNotCheckedText(string thumbprint,string storeText,CertificateKeyProvInfo keyProvInfoBefore,
			CertificateKeyProvInfo keyProvInfoEntry,bool isToolKeyKnown,string whyWrite,string whyNotChecked)
		{
			string written=whyWrite==null ? ", and its earlier private key ("+keyProvInfoBefore.Describe()+") was written on its entry again, but "
				: ", but its earlier private key ("+keyProvInfoBefore.Describe()+") could not be named on its entry again ("+whyWrite.Trim().TrimEnd('.')
				+"), and ";
			string keyUntilNow=isToolKeyKnown ? "the private key the tool gave it ("+keyProvInfoEntry.Describe()+"), which the computer's users can read"
				: "the private key it named until then ("+keyProvInfoEntry.Describe()+"), which may be the one the tool gave it";
			return GetPutBackStartText(thumbprint,storeText)+written+"the store could not be read again to check the entry: "+EndSentence(whyNotChecked)
				+" So the tool cannot tell whether the entry names its earlier private key again or still "+keyUntilNow+"; it deleted no key, so that "
				+"the certificate does not name a key that is gone, and left the entry as it is.";
		}

		///<summary>The sentences for a put-back of a record that names no tool key (IsToolKeyUnknown; the DCFU4 lead fix round, decision 1): which
		///key the entry named until then (keyProvInfoEntry), that it may be the key the tool gave it, which the users can read, or one someone gave
		///the entry since, that the tool deleted no key, and how an administrator deletes it once no certificate names it.</summary>
		private static string GetToolKeyUnknownText(CertificateKeyProvInfo keyProvInfoEntry) {
			string unknown="The tool's record does not say which private key the tool gave the entry, so that key may be the one the tool gave it, which "
				+"the computer's users can read, or one someone gave the entry since: the tool deleted no key.";
			if(keyProvInfoEntry==null || !keyProvInfoEntry.IsNamed) {
				return "Until then the entry named no private key. The tool's record does not say which private key the tool gave the entry, so the tool "
					+"deleted no key: an administrator can find it with certutil -key and delete it once no certificate names it.";
			}
			if(keyProvInfoEntry.IsUnknown) {
				return "Until then the entry named a private key that could not be identified ("+(keyProvInfoEntry.Problem??"").Trim().TrimEnd('.')+"). "
					+unknown+" Once no certificate names that key, an administrator can find it with certutil -key and delete it if it is not someone else's.";
			}
			return "Until then the entry named another private key ("+keyProvInfoEntry.Describe()+"). "+unknown+" Once no certificate names that key, an "
				+"administrator can delete it if it is not someone else's: "+GetCertutilDeleteKeyText(keyProvInfoEntry)+".";
		}

		///<summary>The sentence, after a space, that follows a put-back (PutBackEarlierKey): the tool names the earlier key without opening it
		///(it may be hardware-backed), so if someone deleted that key since the import (against the import's warning), the entry now names a key
		///that is not there, and is removed by hand (DCFU4 second fix round, review finding 2). Without -DeleteKey: the key is gone then, and if it
		///is not, it is a key the tool did not create.</summary>
		private static string GetPutBackHedgeText(string storeText,string thumbprint) {
			return " If someone deleted that earlier key since, the entry names a key that is not there: then remove the certificate as an "
				+"administrator, in PowerShell: Remove-Item Cert:\\"+storeText+"\\"+thumbprint+".";
		}

		///<summary>The sentence that says what became of the private key the tool gave the store's entry, after RemoveImported gave the entry its
		///earlier key back, or tried to (DeleteToolKey): only a key that DeleteToolKey deleted is said to be deleted.</summary>
		private static string GetToolKeyFateText(ToolKeyDeletion toolKeyDeletion,CertificateKeyProvInfo keyProvInfoTool) {
			switch(toolKeyDeletion.ToolKeyFate) {
				case ToolKeyFate.Deleted:
					return "The private key the tool gave it ("+keyProvInfoTool.Describe()+") was deleted.";
				case ToolKeyFate.NotThere:
					return "The private key the tool gave it ("+keyProvInfoTool.Describe()+") is no longer on this computer.";
				case ToolKeyFate.InUse:
					return "The private key the tool gave it ("+keyProvInfoTool.Describe()+") is kept, because another certificate of the store names it.";
				case ToolKeyFate.Unknown:
					string whyUnknown=(keyProvInfoTool==null || !keyProvInfoTool.IsNamed) ? "the entry named no private key"
						: (keyProvInfoTool.Problem??"").Trim().TrimEnd('.');
					return "The private key the tool gave it could not be identified ("+whyUnknown+"), so the tool deleted none: if no certificate uses "
						+"it, an administrator can find it with certutil -key and delete it.";
				default:
					return "The private key the tool gave it ("+keyProvInfoTool.Describe()+") could not be deleted: "+toolKeyDeletion.Why;
			}
		}

		///<summary>True when the entry's key provider information read after the put-back names the earlier key: the very same fields, or (should
		///Windows keep the flags otherwise) the same container, provider and type, key set and key spec.</summary>
		private static bool IsPutBack(CertificateKeyProvInfo keyProvInfoNow,CertificateKeyProvInfo keyProvInfoBefore) {
			if(CertificateKeyProvInfo.IsSameExactly(keyProvInfoNow,keyProvInfoBefore)) {
				return true;
			}
			return keyProvInfoNow.IsNamed && keyProvInfoNow.ContainerName==keyProvInfoBefore.ContainerName
				&& string.Equals(keyProvInfoNow.ProviderName,keyProvInfoBefore.ProviderName,StringComparison.OrdinalIgnoreCase)
				&& keyProvInfoNow.ProviderType==keyProvInfoBefore.ProviderType && keyProvInfoNow.IsMachine==keyProvInfoBefore.IsMachine
				&& keyProvInfoNow.KeySpec==keyProvInfoBefore.KeySpec;
		}

		///<summary>"certutil [-user] -csp "P" -delkey "C"" for this key.</summary>
		private static string GetCertutilDeleteKeyText(CertificateKeyProvInfo keyProvInfo) {
			return "certutil "+(keyProvInfo.IsMachine ? "" : "-user ")+"-csp \""+keyProvInfo.ProviderName+"\" -delkey \""+keyProvInfo.ContainerName+"\"";
		}

		///<summary>The sentence for a record of a certificate whose entry still waits for its earlier key (a put-back that failed or could not be
		///checked, which a later save tries again while the record names it: the DCFU4 lead fix round, decision 2) when the undo of a failed save
		///writes the record of the certificate the settings use over it (one record names one certificate; DbSettingsRunner, fix round 5): it
		///names that certificate, its earlier key, that no later save tries the put-back again, and how an administrator puts it right (the
		///certutil -delkey command of the key the tool gave it, then the import with its earlier key; without a tool key in the record, how to find
		///the key the entry names). In the tool's store. Never throws.</summary>
		public static string GetPutBackNotTriedAgainText(ImportedCertificate importedCertificatePending) {
			string thumbprint=DbConnectionConfig.NormalizeThumbprint(importedCertificatePending.Thumbprint);
			StoreLocation storeLocation=GetToolStoreLocation();
			string storeName=GetToolStoreName();
			string storeText=storeLocation+"\\"+storeName;
			string howToCheck="certutil "+(storeLocation==StoreLocation.CurrentUser ? "-user " : "")+"-store \""+storeName+"\" "+thumbprint;
			string text="The tool's record named the client certificate "+thumbprint+" as well, whose entry in the computer's certificate store ("
				+storeText+") was still to get its earlier private key ("+importedCertificatePending.KeyBefore.Describe()+") back; the record names one "
				+"certificate only, so no later save tries that put-back again. ";
			if(importedCertificatePending.KeyTool!=null) {
				return text+"Its entry may still name the private key the tool gave it ("+importedCertificatePending.KeyTool.Describe()+"), which the "
					+"computer's users can read. Check the entry as an administrator ("+howToCheck+"); to put it back as it was, delete the tool's key ("
					+GetCertutilDeleteKeyText(importedCertificatePending.KeyTool)+"), then import the certificate "+thumbprint+" with its earlier key again.";
			}
			return text+"Its entry may still name a private key the tool gave it, which the computer's users can read (the tool's record does not say "
				+"which key that was). Check the entry as an administrator ("+howToCheck+"); if it does not name its earlier key, import the certificate "
				+thumbprint+" with that key again, and once no certificate names the key the entry named before, delete it if it is not someone else's "
				+"(certutil -delkey).";
		}

		///<summary>The warning for the store entry of the certificate the tool's record names when the entry no longer names the key the record
		///names (RemoveImported): it stays, with the key someone gave it by hand, which is not the tool's to delete.</summary>
		private static string GetEntryNotToolsText(string thumbprint,string storeText,CertificateKeyProvInfo keyProvInfoEntry,
			CertificateKeyProvInfo keyProvInfoTool)
		{
			string keyEntry;
			if(!keyProvInfoEntry.IsNamed) {
				keyEntry="no private key";
			}
			else if(keyProvInfoEntry.IsUnknown) {
				keyEntry="a private key that could not be identified ("+keyProvInfoEntry.Problem.TrimEnd('.')+")";
			}
			else {
				keyEntry="another private key ("+keyProvInfoEntry.Describe()+")";
			}
			return "The client certificate "+thumbprint+" that this tool imported before stays in the computer's certificate store ("+storeText+"): "
				+"its entry names "+keyEntry+", not the one the tool gave it ("+keyProvInfoTool.Describe()+"), so someone changed it by hand, and the "
				+"tool leaves it as it is. If no settings use it, remove it with its private key as an administrator, in PowerShell: Remove-Item "
				+"Cert:\\"+storeText+"\\"+thumbprint+" -DeleteKey.";
		}

		///<summary>The warning for the certificate the tool's record names with a key outside the key set the tool imports into (RemoveImported):
		///the tool never creates such a key, so the record was edited by hand, and the tool leaves the certificate and every key as they are.
		///</summary>
		private static string GetRecordKeyOutsideKeySetText(string thumbprint,string storeText,CertificateKeyProvInfo keyProvInfoTool) {
			return "The tool's record names the client certificate "+thumbprint+" with a private key outside the key set the tool imports into ("
				+keyProvInfoTool.Describe()+"), which the tool never creates, so the tool leaves the certificate in the computer's certificate store ("
				+storeText+") and every key as they are. If no settings use it, remove it as an administrator, in PowerShell: Remove-Item Cert:\\"
				+storeText+"\\"+thumbprint+", and delete its private key only if you know that no one else's certificate uses it.";
		}

		///<summary>Deletes the private key the tool's record names (keyProvInfoTool, a key the tool's import made), by its name in its key set,
		///when no certificate of the store names it (IsKeyNamedInStore) and it is still there. For a key the store's entry no longer names: the
		///certificate was re-keyed by hand, removed by hand, or given its earlier key back (RemoveImported, AddClientCertificate). "" when it was
		///deleted, was not there, or a certificate of the store names it, or the tool does not know which key it is; else start, why, and where
		///the key is left (DeleteToolKey tells these apart). Never throws.</summary>
		private static string DeleteToolKeyNamed(CertificateKeyProvInfo keyProvInfoTool,X509Store x509Store,string start) {
			ToolKeyDeletion toolKeyDeletion=DeleteToolKey(keyProvInfoTool,x509Store);
			if(toolKeyDeletion.ToolKeyFate!=ToolKeyFate.Failed) {
				return "";
			}
			return start+" ("+keyProvInfoTool.Describe()+"): "+toolKeyDeletion.Why;
		}

		///<summary>What became of the tool's key that DeleteToolKey was to delete.</summary>
		private enum ToolKeyFate {
			///<summary>Deleted now: it no longer opens, and its file is gone.</summary>
			Deleted,
			///<summary>Not there to delete: it does not open (deleted before, or never stored).</summary>
			NotThere,
			///<summary>Kept, because a certificate of the store names it.</summary>
			InUse,
			///<summary>Not deleted, because the tool does not know which key it is: no key provider information, or one that names no key or could
			///not be read.</summary>
			Unknown,
			///<summary>Not deleted: the delete failed, or whether a certificate of the store uses it could not be told (ToolKeyDeletion.Why).
			///</summary>
			Failed,
		}

		///<summary>DeleteToolKey's result: what became of the key and, for Failed, why and where the key is left.</summary>
		private sealed class ToolKeyDeletion {
			public ToolKeyFate ToolKeyFate;
			///<summary>For Failed: why, ending with a full stop, then where the key is left; "" otherwise.</summary>
			public string Why;

			public ToolKeyDeletion(ToolKeyFate toolKeyFate,string why) {
				ToolKeyFate=toolKeyFate;
				Why=why;
			}
		}

		///<summary>Deletes the private key the tool's record names (keyProvInfoTool, a key the tool's import made), by its name in its key set,
		///when no certificate of the store names it (IsKeyNamedInStore) and it is still there, and says what became of it. Never throws.</summary>
		private static ToolKeyDeletion DeleteToolKey(CertificateKeyProvInfo keyProvInfoTool,X509Store x509Store) {
			if(!IsKeyNamedAndKnown(keyProvInfoTool)) {
				return new ToolKeyDeletion(ToolKeyFate.Unknown,"");
			}
			string pathKey=null;
			try {
				ActionBeforeKeyDeleteByNameForTests?.Invoke(keyProvInfoTool);
				string whyKept=IsKeyNamedInStore(x509Store,keyProvInfoTool);
				if(whyKept==null) {
					return new ToolKeyDeletion(ToolKeyFate.InUse,"");//A certificate of the store names it.
				}
				if(whyKept!="") {
					return new ToolKeyDeletion(ToolKeyFate.Failed,EndSentence(whyKept)+GetKeyLeftText(TryGetKeyFilePath(keyProvInfoTool),true));
				}
				string error=TryDeleteKeyNamed(keyProvInfoTool,out pathKey,out bool isThere);
				if(error!="") {
					return new ToolKeyDeletion(ToolKeyFate.Failed,EndSentence(error)+GetKeyLeftText(pathKey,false));
				}
				return new ToolKeyDeletion(isThere ? ToolKeyFate.Deleted : ToolKeyFate.NotThere,"");
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				return new ToolKeyDeletion(ToolKeyFate.Failed,EndSentence(ex.Message)+GetKeyLeftText(pathKey,false));
			}
		}

		///<summary>Whether a certificate of the store, read anew, names this key: null when one does (the same key container in the same key set,
		///through a CAPI provider for a CAPI key, or the same CNG provider for a CNG key: CAPI providers share their containers, CNG and CAPI keys
		///live apart), "" when none does, else why it cannot be told (a certificate whose key provider information cannot be read, which may name
		///it; or the store cannot be read). Opens no key.</summary>
		private static string IsKeyNamedInStore(X509Store x509Store,CertificateKeyProvInfo keyProvInfo) {
			X509Certificate2Collection x509Certificate2CollectionStore;
			try {
				using(X509Store x509StoreNow=new X509Store(x509Store.Name,x509Store.Location)) {
					x509StoreNow.Open(OpenFlags.ReadOnly|OpenFlags.OpenExistingOnly);
					x509Certificate2CollectionStore=x509StoreNow.Certificates;
				}
			}
			catch(Exception ex) when(IsStoreException(ex)) {
				return "The store could not be read to see whether another certificate uses the key: "+ex.Message.Trim();
			}
			try {
				foreach(X509Certificate2 x509Certificate2 in x509Certificate2CollectionStore) {
					CertificateKeyProvInfo keyProvInfoStore=ReadKeyProvInfo(x509Certificate2);
					if(!keyProvInfoStore.IsNamed) {
						continue;
					}
					if(keyProvInfoStore.IsUnknown) {
						return "It was kept, because the private key of the certificate "+x509Certificate2.Subject+" ("+x509Certificate2.Thumbprint+"), "
							+"which stays, could not be identified ("+keyProvInfoStore.Problem+"), and it may be this one.";
					}
					bool isSameFamily=(keyProvInfoStore.ProviderType==0)==(keyProvInfo.ProviderType==0)
						&& (keyProvInfo.ProviderType!=0 || string.Equals(keyProvInfoStore.ProviderName,keyProvInfo.ProviderName,StringComparison.OrdinalIgnoreCase));
					if(isSameFamily && string.Equals(keyProvInfoStore.Identity,keyProvInfo.Identity,StringComparison.OrdinalIgnoreCase)) {
						return null;
					}
				}
				return "";
			}
			finally {
				ResetAll(x509Certificate2CollectionStore);
			}
		}

		///<summary>Deletes the key with this key provider information by its name, in its key set: a CAPI key container through CAPI
		///(DeleteCapiKey, which checks that it is gone), a CNG key through CNG (checked the same way). The tool's own key only: the key a record
		///names. "" when it is gone, or was not there (isThere: it opened, so it was there to delete); else why not. pathKey: its file, named
		///before the delete (null when it cannot be). Opens the key silently (NoPrompt, Silent). Throws what opening the key throws, other than
		///its absence, and what ActionBeforeKeyNamedDeleteForTests throws.</summary>
		private static string TryDeleteKeyNamed(CertificateKeyProvInfo keyProvInfo,out string pathKey,out bool isThere) {
			pathKey=null;
			isThere=false;
			if(keyProvInfo.ProviderType!=0) {
				RSACryptoServiceProvider rsaCryptoServiceProvider;
				try {
					rsaCryptoServiceProvider=new RSACryptoServiceProvider(GetCspParameters(keyProvInfo));
				}
				catch(CryptographicException) {
					return "";//"Keyset does not exist": nothing to delete.
				}
				isThere=true;
				try {
					pathKey=GetCapiKeyFilePath(keyProvInfo.IsMachine,rsaCryptoServiceProvider.CspKeyContainerInfo.UniqueKeyContainerName);
					ActionBeforeKeyNamedDeleteForTests?.Invoke(keyProvInfo);
				}
				catch(Exception) {
					rsaCryptoServiceProvider.Dispose();//Opened by name (PersistKeyInCsp): the container stays.
					throw;
				}
				return DeleteCapiKey(rsaCryptoServiceProvider);
			}
			CngProvider cngProvider=new CngProvider(keyProvInfo.ProviderName);
			CngKeyOpenOptions cngKeyOpenOptions=CngKeyOpenOptions.Silent|(keyProvInfo.IsMachine ? CngKeyOpenOptions.MachineKey : CngKeyOpenOptions.None);
			if(!CngKey.Exists(keyProvInfo.ContainerName,cngProvider,cngKeyOpenOptions)) {
				return "";
			}
			isThere=true;
			CngKey cngKey=CngKey.Open(keyProvInfo.ContainerName,cngProvider,cngKeyOpenOptions);
			try {
				pathKey=GetCngKeyFilePath(keyProvInfo.IsMachine,cngKey.UniqueName);
				ActionBeforeKeyNamedDeleteForTests?.Invoke(keyProvInfo);
			}
			catch(Exception) {
				cngKey.Dispose();//Disposing a key opened by name leaves it.
				throw;
			}
			cngKey.Delete();//Also disposes it.
			bool isFileThere=pathKey!=null && File.Exists(pathKey);
			bool isOpening=CngKey.Exists(keyProvInfo.ContainerName,cngProvider,cngKeyOpenOptions);
			if(!isOpening && !isFileThere) {
				return "";
			}
			return "Windows reported no error, but the key is still there.";
		}

		///<summary>The file of the key with this key provider information, or null when it cannot be named (it does not open). Never throws.
		///</summary>
		private static string TryGetKeyFilePath(CertificateKeyProvInfo keyProvInfo) {
			try {
				if(keyProvInfo.ProviderType!=0) {
					using(RSACryptoServiceProvider rsaCryptoServiceProvider=new RSACryptoServiceProvider(GetCspParameters(keyProvInfo))) {
						return GetCapiKeyFilePath(keyProvInfo.IsMachine,rsaCryptoServiceProvider.CspKeyContainerInfo.UniqueKeyContainerName);
					}
				}
				CngKeyOpenOptions cngKeyOpenOptions=CngKeyOpenOptions.Silent|(keyProvInfo.IsMachine ? CngKeyOpenOptions.MachineKey : CngKeyOpenOptions.None);
				using(CngKey cngKey=CngKey.Open(keyProvInfo.ContainerName,new CngProvider(keyProvInfo.ProviderName),cngKeyOpenOptions)) {
					return GetCngKeyFilePath(keyProvInfo.IsMachine,cngKey.UniqueName);
				}
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				return null;
			}
		}

		///<summary>The CAPI parameters that open an existing key container of this key provider information, in its key set, silently: opened by
		///name, so disposing the object leaves the container.</summary>
		private static CspParameters GetCspParameters(CertificateKeyProvInfo keyProvInfo) {
			CspParameters cspParameters=new CspParameters(keyProvInfo.ProviderType,keyProvInfo.ProviderName,keyProvInfo.ContainerName);
			if(keyProvInfo.KeySpec==1 || keyProvInfo.KeySpec==2) {
				cspParameters.KeyNumber=(int)keyProvInfo.KeySpec;//AT_KEYEXCHANGE or AT_SIGNATURE.
			}
			cspParameters.Flags=CspProviderFlags.UseExistingKey|CspProviderFlags.NoPrompt;
			if(keyProvInfo.IsMachine) {
				cspParameters.Flags|=CspProviderFlags.UseMachineKeyStore;
			}
			return cspParameters;
		}

		///<summary>The record to write for a certificate the saved settings keep using without a new import: as read, or, for a record that names
		///no key (the old format), with the key its store entry names now, but only when the old rule would claim that key for the tool's on a
		///re-import (ImportedCertificateRecord): its key provider information names a key in the tool's key set (the machine's for LocalMachine\My;
		///read without opening the key), and only then the key opens and signs or decrypts (IsKeyUsable), as IsEntryKeyUsable checks before
		///IsEntryKeyTools lets an old-format record claim an entry. From then on only that key is the tool's. An entry whose key is in another key
		///set (an administrator's own user key set, say), cannot be identified, or does not open silently (a smart card, a TPM key that needs a
		///PIN, a deleted key) is one an administrator may have re-keyed by hand: the old rule never claimed such a key, so the record keeps naming
		///no key (it is written in format 2 all the same), and the old-format handling goes on until an import records a key under the strict rule
		///(DCFU4 second fix round, review finding 1). Unchanged when the entry or its key provider information cannot be read. Never throws.
		///</summary>
		public static ImportedCertificate CompleteRecord(ImportedCertificate importedCertificate) {
			if(importedCertificate==null || importedCertificate.KeyTool!=null) {
				return importedCertificate;
			}
			if(IsToolKeyUnknown(importedCertificate)) {
				//A record with an earlier key but no tool key (DCFU4 lead fix round, decision 1): the entry's key may be someone else's by now, so it is
				//never recorded as the tool's; the record stays as it is, and its removal puts the earlier key back and deletes no key.
				return importedCertificate;
			}
			try {
				using(X509Certificate2 x509Certificate2=FindInStore(importedCertificate.Thumbprint)) {
					if(x509Certificate2==null) {
						return importedCertificate;
					}
					CertificateKeyProvInfo keyProvInfo=ReadKeyProvInfo(x509Certificate2);
					if(!IsKeyNamedAndKnown(keyProvInfo) || !IsInToolKeySet(keyProvInfo,GetToolStoreLocation()==StoreLocation.LocalMachine)) {
						return importedCertificate;
					}
					if(!IsKeyUsable(x509Certificate2)) {
						return importedCertificate;
					}
					ImportedCertificate importedCertificateComplete=ImportedCertificate.FromThumbprint(importedCertificate.Thumbprint);
					importedCertificateComplete.KeyTool=keyProvInfo;
					importedCertificateComplete.KeyBefore=importedCertificate.KeyBefore;
					return importedCertificateComplete;
				}
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				return importedCertificate;
			}
		}

		///<summary>Deletes the private key of each certificate of listX509Certificate2s, but not one that a certificate of the store, or of
		///listInUse, still uses (DeleteKeysNotInUse), as DirectCertificateImport.DeleteKeysNotInStore does. The store is read anew, not through
		///the caller's open handle, which keeps the certificates as it read them when it was opened: a certificate another program put into the
		///store since counts too. The certificates read from the store are released again; their keys are never opened (ReadKeyProvInfo). Why
		///each key could not be deleted, in the order of listX509Certificate2s (null when it was deleted, it had none, or another certificate
		///uses it); when the store can't be read, no key is deleted.</summary>
		private static List<KeyNotDeleted> DeleteKeysNotInStore(X509Store x509Store,List<X509Certificate2> listX509Certificate2s,
			List<X509Certificate2> listInUse)
		{
			if(listX509Certificate2s.Count==0) {
				return new List<KeyNotDeleted>();
			}
			X509Certificate2Collection x509Certificate2CollectionStore;
			try {
				using(X509Store x509StoreNow=new X509Store(x509Store.Name,x509Store.Location)) {
					x509StoreNow.Open(OpenFlags.ReadOnly|OpenFlags.OpenExistingOnly);
					x509Certificate2CollectionStore=x509StoreNow.Certificates;//The certificates stay valid after the store is closed.
				}
			}
			catch(Exception ex) when(IsStoreException(ex)) {
				KeyNotDeleted keyNotDeleted=new KeyNotDeleted("The store could not be read to see whether another certificate uses the key: "
					+ex.Message.Trim(),true);
				return listX509Certificate2s.Select(x => x.HasPrivateKey ? keyNotDeleted : null).ToList();
			}
			try {
				List<X509Certificate2> listInUseAll=x509Certificate2CollectionStore.Cast<X509Certificate2>().ToList();
				listInUseAll.AddRange(listInUse);
				return DeleteKeysNotInUse(listX509Certificate2s,listInUseAll);
			}
			finally {
				ResetAll(x509Certificate2CollectionStore);
			}
		}

		///<summary>Deletes the private key of each certificate of listX509Certificate2s, each key container once, but none that a certificate of
		///listInUse uses: one .pfx can give two of its certificates one key (a renewal with the same key), which Windows stores once, in one
		///container, for both. Containers are compared by their key provider information (ReadKeyProvInfo, CertificateKeyProvInfo.Identity), all read
		///before the first is deleted, and no key of listInUse is opened: a key on a TPM or a smart card could ask for its PIN, or fail. A
		///certificate whose key cannot be identified may use any of them, so then none is deleted, and each says why. Why each key could not be
		///deleted, in the order of listX509Certificate2s: null when it was deleted, it had none, or a certificate of listInUse uses it.</summary>
		private static List<KeyNotDeleted> DeleteKeysNotInUse(List<X509Certificate2> listX509Certificate2s,List<X509Certificate2> listInUse) {
			//Every container first: once a container is deleted, the other certificates that used it no longer say which one it was.
			List<CertificateKeyProvInfo> listKeyProvInfos=listX509Certificate2s.Select(x => ReadKeyProvInfo(x)).ToList();
			List<CertificateKeyProvInfo> listKeyProvInfosInUse=listInUse.Select(x => ReadKeyProvInfo(x)).ToList();
			string whyUnknownInUse=null;
			int indexUnknown=listKeyProvInfosInUse.FindIndex(x => x.IsUnknown);
			if(indexUnknown>=0) {
				whyUnknownInUse="It was kept, because the private key of the certificate "+listInUse[indexUnknown].Subject+" ("
					+listInUse[indexUnknown].Thumbprint+"), which stays, could not be identified ("+listKeyProvInfosInUse[indexUnknown].Problem
					+"), and it may be this one.";
			}
			HashSet<string> hashSetInUse=new HashSet<string>(listKeyProvInfosInUse.Where(x => x.IsNamed && !x.IsUnknown).Select(x => x.Identity),
				StringComparer.OrdinalIgnoreCase);
			HashSet<string> hashSetDeleted=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			List<KeyNotDeleted> listNotDeleted=new List<KeyNotDeleted>();
			for(int i=0;i<listX509Certificate2s.Count;i++) {
				CertificateKeyProvInfo keyProvInfo=listKeyProvInfos[i];
				if(!keyProvInfo.IsNamed) {
					listNotDeleted.Add(null);//No key stored for it.
					continue;
				}
				if(keyProvInfo.IsUnknown) {
					listNotDeleted.Add(new KeyNotDeleted("It was kept, because its key container could not be identified ("+keyProvInfo.Problem+"), so "
						+"a certificate of the store may use it.",true));
					continue;
				}
				if(whyUnknownInUse!=null) {
					listNotDeleted.Add(new KeyNotDeleted(whyUnknownInUse,true));
					continue;
				}
				if(hashSetInUse.Contains(keyProvInfo.Identity) || hashSetDeleted.Contains(keyProvInfo.Identity)) {
					listNotDeleted.Add(null);//In use by a certificate that stays, or deleted already for another certificate of the list.
					continue;
				}
				string error=TryDeletePrivateKey(listX509Certificate2s[i]);
				if(error=="") {
					hashSetDeleted.Add(keyProvInfo.Identity);
					listNotDeleted.Add(null);
					continue;
				}
				listNotDeleted.Add(new KeyNotDeleted(error,false));
			}
			return listNotDeleted;
		}

		///<summary>The certificate's key provider information (CERT_KEY_PROV_INFO_PROP_ID), which names its private key's container, provider and
		///key set, read without opening the key (CodeBase's CertificateKeyProvInfo.Read, which the Direct certificate import shares): a key on a
		///TPM or a smart card is never asked for its PIN to compare containers. A certificate without the property names no key; one whose
		///property cannot be read gives Problem. The seam ActionBeforeKeyProvInfoReadForTests is called right before the read. Never throws:
		///anything but a critical exception (IsCriticalException) is a property that cannot be read.</summary>
		internal static CertificateKeyProvInfo ReadKeyProvInfo(X509Certificate2 x509Certificate2) {
			return CertificateKeyProvInfo.Read(x509Certificate2,ActionBeforeKeyProvInfoReadForTests);
		}

		///<summary>Deletes the private key of the certificate from the key set it lives in: a CAPI key container (RSA) or a CNG key (RSA or ECDSA),
		///as DirectCertificateImport.TryDeletePrivateKey does. A CAPI key is deleted through CAPI (GetCapiRsaPrivateKey, DeleteCapiKey): .NET
		///Framework's GetRSAPrivateKey opens it through CNG, as an RSACng whose Key.Delete returns without deleting the CAPI container. The whole
		///container goes, also for another certificate that uses the same key, so the callers ask DeleteKeysNotInUse first. "" when deleted (or
		///there was none), else why not. Never throws: anything but a critical exception (IsCriticalException) is a key not deleted.</summary>
		internal static string TryDeletePrivateKey(X509Certificate2 x509Certificate2) {
			try {
				if(!x509Certificate2.HasPrivateKey) {
					return "";
				}
				ActionBeforeKeyDeleteForTests?.Invoke(x509Certificate2);
				RSACryptoServiceProvider rsaCryptoServiceProviderCapi=GetCapiRsaPrivateKey(x509Certificate2);
				if(rsaCryptoServiceProviderCapi!=null) {
					return DeleteCapiKey(rsaCryptoServiceProviderCapi);
				}
				using(RSA rsa=OpenRsaPrivateKey(x509Certificate2)) {
					if(rsa is RSACng) {
						((RSACng)rsa).Key.Delete();
						return "";
					}
					if(rsa is RSACryptoServiceProvider) {//How .NET (not .NET Framework) gives a CAPI key.
						return DeleteCapiKey((RSACryptoServiceProvider)rsa);
					}
					if(rsa!=null) {
						return "";
					}
				}
				using(ECDsa ecdsa=OpenECDsaPrivateKey(x509Certificate2)) {
					if(ecdsa is ECDsaCng) {
						((ECDsaCng)ecdsa).Key.Delete();
					}
				}
				return "";
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				return ex.Message.Trim();//Windows' texts end with a line break.
			}
		}

		///<summary>Deletes a CAPI key container in the key set the key lives in: PersistKeyInCsp=false, then Clear (CRYPT_DELETEKEYSET). Clear
		///does not say when Windows did not delete it, so afterwards the container must no longer open (CspParameters with UseExistingKey, in its
		///key set, silent) and its file must be gone (GetCapiKeyFilePath, named before the delete). "" when it is gone, else what is still there.
		///Throws what reading the container's information throws.</summary>
		private static string DeleteCapiKey(RSACryptoServiceProvider rsaCryptoServiceProvider) {
			CspKeyContainerInfo cspKeyContainerInfo=rsaCryptoServiceProvider.CspKeyContainerInfo;
			string providerName=cspKeyContainerInfo.ProviderName;
			int providerType=cspKeyContainerInfo.ProviderType;
			string containerName=cspKeyContainerInfo.KeyContainerName;
			bool isMachine=cspKeyContainerInfo.MachineKeyStore;
			KeyNumber keyNumber=cspKeyContainerInfo.KeyNumber;
			string pathKey=GetCapiKeyFilePath(isMachine,cspKeyContainerInfo.UniqueKeyContainerName);
			if(ActionInsteadOfCapiKeyDeleteForTests!=null) {
				ActionInsteadOfCapiKeyDeleteForTests(rsaCryptoServiceProvider);
			}
			else {
				//Clear then deletes the key container, in the key set the certificate names for its key (user or machine).
				rsaCryptoServiceProvider.PersistKeyInCsp=false;
				rsaCryptoServiceProvider.Clear();
			}
			bool isOpening=DoesCapiKeyContainerOpen(providerName,providerType,containerName,isMachine,keyNumber);
			bool isFileThere=pathKey!=null && File.Exists(pathKey);
			if(!isOpening && !isFileThere) {
				return "";
			}
			string still;
			if(isOpening && isFileThere) {
				still="its key container still opens, and its file is still there";
			}
			else if(isOpening) {
				still="its key container still opens";
			}
			else {
				still="its file is still there";
			}
			return "Windows reported no error, but the key is still there: "+still+".";
		}

		///<summary>True when the CAPI key container opens in its key set (UseExistingKey, so nothing is made; NoPrompt, so nothing asks), which
		///leaves it as it is (opened by name, it persists).</summary>
		private static bool DoesCapiKeyContainerOpen(string providerName,int providerType,string containerName,bool isMachine,KeyNumber keyNumber) {
			CspParameters cspParameters=new CspParameters(providerType,providerName,containerName);
			cspParameters.KeyNumber=(int)keyNumber;
			cspParameters.Flags=CspProviderFlags.UseExistingKey|CspProviderFlags.NoPrompt;
			if(isMachine) {
				cspParameters.Flags|=CspProviderFlags.UseMachineKeyStore;
			}
			try {
				using(new RSACryptoServiceProvider(cspParameters)) {
				}
				return true;
			}
			catch(CryptographicException) {
				return false;//"Keyset does not exist".
			}
		}

		///<summary>The certificate's RSA private key when Windows keeps it in a CAPI key container, else null (a CNG key, an ECDSA key, no key, or
		///one that does not open: the CNG path then says why). A .pfx that names no CNG provider (OpenSSL writes none) or a CAPI provider is
		///imported into a CAPI container, and .NET Framework's GetRSAPrivateKey opens that through CNG: an RSACng whose Key.Delete leaves the CAPI
		///container, and whose Key.UniqueName is no file of the CNG folder (DIRECT-CERT, docs/port-notes/DIRECT-CERT.md). The key object comes
		///with the certificate's own key provider information, so it opens the container in the key set the key lives in. The certificate keeps
		///the object (X509Certificate2.PrivateKey), so it is not disposed here. Never throws: anything but a critical exception
		///(IsCriticalException) gives null, as in DirectCertificateImport's copy.</summary>
		private static RSACryptoServiceProvider GetCapiRsaPrivateKey(X509Certificate2 x509Certificate2) {
			try {
				ActionBeforePrivateKeyOpenForTests?.Invoke(x509Certificate2);
				//Through its key provider information, as the .NET Framework's PrivateKey opened it: .NET opens a CAPI key through CNG first (an
				//RSACng), so its PrivateKey no longer tells the two apart (S3b-5; also SYSLIB0028). The caller owns the object now.
				return CertificateKeyProvInfo.OpenCapiRsaPrivateKey(x509Certificate2);
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				return null;//.NET Framework: "Invalid provider type specified" for a CNG key, NotSupportedException for an ECDSA one.
			}
		}

		///<summary>GetRSAPrivateKey, after ActionBeforePrivateKeyOpenForTests.</summary>
		private static RSA OpenRsaPrivateKey(X509Certificate2 x509Certificate2) {
			ActionBeforePrivateKeyOpenForTests?.Invoke(x509Certificate2);
			return x509Certificate2.GetRSAPrivateKey();
		}

		///<summary>GetECDsaPrivateKey, after ActionBeforePrivateKeyOpenForTests.</summary>
		private static ECDsa OpenECDsaPrivateKey(X509Certificate2 x509Certificate2) {
			ActionBeforePrivateKeyOpenForTests?.Invoke(x509Certificate2);
			return x509Certificate2.GetECDsaPrivateKey();
		}

		///<summary>True when the certificate's private key can be used on this computer: it opens (an RSA or ECDSA key) and signs a test digest,
		///or, for an RSA key that does not sign, decrypts what its public key encrypted. False without a key, or with one that does not open
		///(deleted, no read access, another user's). HasPrivateKey alone only says that the certificate names a key. A copy of
		///DirectCertificateImport.IsKeyUsable. Never throws: anything but a critical exception (IsCriticalException) means not usable.</summary>
		internal static bool IsKeyUsable(X509Certificate2 x509Certificate2) {
			try {
				if(!x509Certificate2.HasPrivateKey) {
					return false;
				}
				byte[] byteArrayValue=Encoding.ASCII.GetBytes("Open PMS - Dental client certificate key check");
				byte[] byteArrayDigest;
				using(SHA256 sha256=SHA256.Create()) {
					byteArrayDigest=sha256.ComputeHash(byteArrayValue);
				}
				using(RSA rsa=OpenRsaPrivateKey(x509Certificate2)) {
					if(rsa!=null) {
						return CanSign(rsa,byteArrayValue,byteArrayDigest) || CanDecrypt(rsa,x509Certificate2,byteArrayValue);
					}
				}
				using(ECDsa ecdsa=OpenECDsaPrivateKey(x509Certificate2)) {
					if(ecdsa!=null) {
						ecdsa.SignHash(byteArrayDigest);
						return true;
					}
				}
				return false;
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				return false;
			}
		}

		private static bool CanSign(RSA rsa,byte[] byteArrayValue,byte[] byteArrayDigest) {
			try {
				rsa.SignHash(byteArrayDigest,HashAlgorithmName.SHA256,RSASignaturePadding.Pkcs1);
				return true;
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				//An old CAPI provider without SHA-256, or a key that only decrypts (the decryption is tried next).
			}
			try {
				byte[] byteArrayDigestSha1;
				using(SHA1 sha1=SHA1.Create()) {
					byteArrayDigestSha1=sha1.ComputeHash(byteArrayValue);
				}
				rsa.SignHash(byteArrayDigestSha1,HashAlgorithmName.SHA1,RSASignaturePadding.Pkcs1);
				return true;
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				return false;
			}
		}

		private static bool CanDecrypt(RSA rsa,X509Certificate2 x509Certificate2,byte[] byteArrayValue) {
			try {
				using(RSA rsaPublic=x509Certificate2.GetRSAPublicKey()) {
					byte[] byteArrayEncrypted=rsaPublic.Encrypt(byteArrayValue,RSAEncryptionPadding.Pkcs1);
					return rsa.Decrypt(byteArrayEncrypted,RSAEncryptionPadding.Pkcs1).SequenceEqual(byteArrayValue);
				}
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				return false;
			}
		}

		///<summary>True for the exceptions that a method documented "Never throws" lets through: OutOfMemoryException, StackOverflowException and
		///ThreadAbortException, after which the tool can't go on safely (DirectCertificateImport.IsCriticalException). Every other one is an
		///answer (not deleted, no name).</summary>
		internal static bool IsCriticalException(Exception ex) {
			return ex is OutOfMemoryException || ex is StackOverflowException || ex is ThreadAbortException;
		}

		///<summary>The certificate with this thumbprint in LocalMachine\My, or null.</summary>
		public static X509Certificate2 FindInStore(string thumbprint) {
			if(string.IsNullOrEmpty(thumbprint)) {
				return null;
			}
			using(X509Store x509Store=new X509Store(GetToolStoreName(),GetToolStoreLocation())) {
				x509Store.Open(OpenFlags.ReadOnly|OpenFlags.OpenExistingOnly);
				X509Certificate2Collection x509Certificate2Collection=x509Store.Certificates.Find(X509FindType.FindByThumbprint,thumbprint,false);
				return x509Certificate2Collection.Count==0 ? null : x509Certificate2Collection[0];
			}
		}

		///<summary>The tool's store: LocalMachine, or the current user's for the tests' StoreNameForTests.</summary>
		private static StoreLocation GetToolStoreLocation() {
			return StoreNameForTests==null ? StoreLocation.LocalMachine : StoreLocation.CurrentUser;
		}

		///<summary>The tool's store: My, or the tests' StoreNameForTests.</summary>
		private static string GetToolStoreName() {
			return StoreNameForTests??StoreName.My.ToString();
		}

		///<summary>"subject, expires yyyy-MM-dd" for the window and messages.</summary>
		public static string Describe(X509Certificate2 x509Certificate2) {
			if(x509Certificate2==null) {
				return "";
			}
			return x509Certificate2.Subject+", expires "+x509Certificate2.NotAfter.ToString("yyyy-MM-dd");
		}

		///<summary>Throws DbSettingsException (exit code 2) when the certificate cannot be the client certificate: no private key, expired, or an
		///Extended Key Usage without client authentication.</summary>
		public static void CheckClientCertificate(X509Certificate2 x509Certificate2,string source) {
			if(!x509Certificate2.HasPrivateKey) {
				throw new DbSettingsException(DbSettingsRunner.EXIT_BAD_VALUE,"The certificate "+Describe(x509Certificate2)+" ("+source+") has no "
					+"private key on this computer.");
			}
			if(x509Certificate2.NotAfter<DateTime.Now) {
				throw new DbSettingsException(DbSettingsRunner.EXIT_BAD_VALUE,"The certificate "+Describe(x509Certificate2)+" ("+source+") has expired.");
			}
			if(!IsForClientAuthentication(x509Certificate2)) {
				throw new DbSettingsException(DbSettingsRunner.EXIT_BAD_VALUE,"The certificate "+Describe(x509Certificate2)+" ("+source+") is not for "
					+"client authentication (its Extended Key Usage has no clientAuth).");
			}
		}

		///<summary>True when the certificate may be used for TLS client authentication: no Extended Key Usage, or one with clientAuth.</summary>
		private static bool IsForClientAuthentication(X509Certificate2 x509Certificate2) {
			X509EnhancedKeyUsageExtension x509EnhancedKeyUsageExtension=x509Certificate2.Extensions.OfType<X509EnhancedKeyUsageExtension>()
				.FirstOrDefault();
			return x509EnhancedKeyUsageExtension==null
				|| x509EnhancedKeyUsageExtension.EnhancedKeyUsages.Cast<Oid>().Any(x => x.Value==OID_CLIENT_AUTHENTICATION);
		}

		///<summary>True for a certificate authority's certificate: Basic Constraints with CA=true.</summary>
		private static bool IsCertificateAuthority(X509Certificate2 x509Certificate2) {
			return x509Certificate2.Extensions.OfType<X509BasicConstraintsExtension>().Any(x => x.CertificateAuthority);
		}

		private static X509Certificate2Collection ImportCollection(string pfxPath,string password,X509KeyStorageFlags x509KeyStorageFlags) {
			if(!File.Exists(pfxPath)) {
				throw new DbSettingsException(DbSettingsRunner.EXIT_BAD_VALUE,"The client certificate file "+pfxPath+" does not exist.");
			}
			X509Certificate2Collection x509Certificate2Collection=new X509Certificate2Collection();
			try {
				//Never X509KeyStorageFlags.Exportable: the private key cannot be exported from this computer again (owner decision Q2).
				//X509CertificateLoader in place of Import (SYSLIB0057, S3b-5), without the loader's default limits (Pkcs12LoaderLimits.DangerousNoLimits,
				//as DirectCertificateImport.ImportCertificates since S3b-3): those drop the storage provider and key name the .pfx names, which decide
				//where Windows stores the key (the CAPI container the tool's records and DbConnectionSecurityTests expect), and refuse files Import read.
				x509Certificate2Collection.AddRange(X509CertificateLoader.LoadPkcs12CollectionFromFile(pfxPath,password??"",x509KeyStorageFlags,
					Pkcs12LoaderLimits.DangerousNoLimits));
			}
			catch(CryptographicException) {
				//The exception text is Windows' own and names no secret, but "wrong password" is what it nearly always means.
				string message="The client certificate file "+pfxPath+" cannot be opened: the password is wrong, or it is not a .pfx file.";
				if((x509KeyStorageFlags&X509KeyStorageFlags.PersistKeySet)!=0) {
					//.NET gives no certificate when the import throws, so the tool knows of no key to delete; Windows may have stored one of the
					//file's keys before it failed (as DirectCertificateImport says).
					message+=" Nothing was imported. Windows may have left a private key of the file behind on this computer, which the tool cannot "
						+"find to delete.";
				}
				throw new DbSettingsException(DbSettingsRunner.EXIT_BAD_VALUE,message);
			}
			return x509Certificate2Collection;
		}

		///<summary>The client certificate of the .pfx (the lead's decision of DCFU3): of the file's certificates with a private key, those that
		///are not certificate authorities (IsCertificateAuthority) are the candidates, and of them the ones for client authentication
		///(IsForClientAuthentication) are preferred. Exactly one must be left, which is then checked (CheckClientCertificate); none, or several,
		///are refused before anything goes into the store, with what the file holds (subjects and thumbprints) and how to make a .pfx with one
		///client certificate. So a CA certificate that a file lists first with its key is never taken for the client certificate. Throws
		///DbSettingsException (exit code 2).</summary>
		private static X509Certificate2 GetClientCertificate(X509Certificate2Collection x509Certificate2Collection,string pfxPath) {
			List<X509Certificate2> listAll=x509Certificate2Collection.Cast<X509Certificate2>().ToList();
			List<X509Certificate2> listKeyed=listAll.FindAll(x => x.HasPrivateKey);
			if(listKeyed.Count==0) {
				throw new DbSettingsException(DbSettingsRunner.EXIT_BAD_VALUE,"The client certificate file "+pfxPath+" holds no certificate with a "
					+"private key. "+PFX_HOW_TO+".");
			}
			List<X509Certificate2> listCandidates=listKeyed.FindAll(x => !IsCertificateAuthority(x));
			List<X509Certificate2> listClientAuth=listCandidates.FindAll(x => IsForClientAuthentication(x));
			List<X509Certificate2> listChosen=listClientAuth.Count>0 ? listClientAuth : listCandidates;
			if(listChosen.Count==1) {
				//One candidate that is not for client authentication: CheckClientCertificate says so.
				CheckClientCertificate(listChosen[0],pfxPath);
				return listChosen[0];
			}
			string found;
			if(listCandidates.Count==0) {
				found="holds no client certificate: each of its certificates with a private key is a certificate authority (CA=true in its Basic "
					+"Constraints).";
			}
			else if(listClientAuth.Count==0) {
				found="holds no client certificate: none of its certificates with a private key that are not certificate authorities is for client "
					+"authentication.";
			}
			else {
				found="holds more than one certificate that can be the client certificate (with a private key, not a certificate authority, and for "
					+"client authentication): "+string.Join("; ",listChosen.Select(x => x.Subject+" ("+x.Thumbprint+")"))+".";
			}
			throw new DbSettingsException(DbSettingsRunner.EXIT_BAD_VALUE,"The client certificate file "+pfxPath+" "+found+" It holds: "
				+string.Join("; ",listAll.Select(x => DescribeForRefusal(x)))+". "+PFX_HOW_TO+".");
		}

		///<summary>"subject (thumbprint, what it is)" of a certificate of a refused .pfx (GetClientCertificate).</summary>
		private static string DescribeForRefusal(X509Certificate2 x509Certificate2) {
			string what;
			if(!x509Certificate2.HasPrivateKey) {
				what="without a private key";
			}
			else if(IsCertificateAuthority(x509Certificate2)) {
				what="with a private key, a certificate authority";
			}
			else if(IsForClientAuthentication(x509Certificate2)) {
				what="with a private key, for client authentication";
			}
			else {
				what="with a private key, not for client authentication";
			}
			return x509Certificate2.Subject+" ("+x509Certificate2.Thumbprint+", "+what+")";
		}

		///<summary>Gives BUILTIN\Users read access to the private key file of the certificate (GetPrivateKeyFilePath): the import's MachineKeySet
		///puts a CNG key under %ProgramData%\Microsoft\Crypto\Keys and a CAPI key under %ProgramData%\Microsoft\Crypto\RSA\MachineKeys. Throws
		///when the file is not found or its rights cannot be changed.</summary>
		private static void GrantUsersReadToPrivateKey(X509Certificate2 x509Certificate2) {
			string pathKey=GetPrivateKeyFilePath(x509Certificate2);
			if(pathKey==null || !File.Exists(pathKey)) {
				throw new FileNotFoundException("The private key file was not found.");
			}
			SecurityIdentifier securityIdentifierUsers=new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid,null);
			FileInfo fileInfoKey=new FileInfo(pathKey);
			FileSecurity fileSecurity=fileInfoKey.GetAccessControl();
			fileSecurity.AddAccessRule(new FileSystemAccessRule(securityIdentifierUsers,FileSystemRights.Read,AccessControlType.Allow));
			fileInfoKey.SetAccessControl(fileSecurity);
		}

		///<summary>The file of the certificate's private key, in the key set it lives in (GetCapiKeyFilePath, GetCngKeyFilePath). A CAPI key is
		///named through CAPI (GetCapiRsaPrivateKey), as DirectCertificateImport does: the RSACng that .NET Framework's GetRSAPrivateKey gives for it
		///has a UniqueName that is no file of the CNG folder. Opens the key: only for a key the tool stored or imported itself, or the one it
		///gives the users read access to. Null without a key or for another kind of key. Throws CryptographicException for a key that does not
		///open.</summary>
		internal static string GetPrivateKeyFilePath(X509Certificate2 x509Certificate2) {
			if(!x509Certificate2.HasPrivateKey) {
				return null;
			}
			RSACryptoServiceProvider rsaCryptoServiceProviderCapi=GetCapiRsaPrivateKey(x509Certificate2);
			if(rsaCryptoServiceProviderCapi!=null) {
				CspKeyContainerInfo cspKeyContainerInfoCapi=rsaCryptoServiceProviderCapi.CspKeyContainerInfo;
				return GetCapiKeyFilePath(cspKeyContainerInfoCapi.MachineKeyStore,cspKeyContainerInfoCapi.UniqueKeyContainerName);
			}
			//The CngKey objects are disposed too: .NET gives a new one for each Key (.NET Framework its own, which the using of rsa or ecdsa then
			//finds disposed).
			using(RSA rsa=OpenRsaPrivateKey(x509Certificate2)) {
				if(rsa is RSACng) {
					using(CngKey cngKey=((RSACng)rsa).Key) {
						return GetCngKeyFilePath(cngKey.IsMachineKey,cngKey.UniqueName);
					}
				}
				if(rsa is RSACryptoServiceProvider) {//How .NET (not .NET Framework) gives a CAPI key.
					CspKeyContainerInfo cspKeyContainerInfo=((RSACryptoServiceProvider)rsa).CspKeyContainerInfo;
					return GetCapiKeyFilePath(cspKeyContainerInfo.MachineKeyStore,cspKeyContainerInfo.UniqueKeyContainerName);
				}
				if(rsa!=null) {
					return null;
				}
			}
			using(ECDsa ecdsa=OpenECDsaPrivateKey(x509Certificate2)) {
				if(ecdsa is ECDsaCng) {
					using(CngKey cngKey=((ECDsaCng)ecdsa).Key) {
						return GetCngKeyFilePath(cngKey.IsMachineKey,cngKey.UniqueName);
					}
				}
			}
			return null;
		}

		///<summary>GetPrivateKeyFilePath, or null when the key does not open. Never throws: anything but a critical exception (IsCriticalException)
		///gives null.</summary>
		private static string TryGetPrivateKeyFilePath(X509Certificate2 x509Certificate2) {
			try {
				return GetPrivateKeyFilePath(x509Certificate2);
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				return null;
			}
		}

		///<summary>Where a private key that could not be deleted is left, after a sentence that says so: its file, named before the delete
		///(TryGetPrivateKeyFilePath). Once the certificate is out of the store, certlm.msc no longer shows the key. isMaybeInUse: the key was kept
		///because a certificate of the store may use it, so it is deleted by hand only once none does.</summary>
		private static string GetKeyLeftText(string pathKey,bool isMaybeInUse) {
			if(string.IsNullOrEmpty(pathKey)) {
				return " The key is left on this computer; its file could not be named.";
			}
			if(isMaybeInUse) {
				return " The key is left on this computer in "+pathKey+": delete that file as an administrator only once no certificate of the store "
					+"uses it.";
			}
			return " The key is left on this computer in "+pathKey+": delete that file as an administrator.";
		}

		///<summary>The file of a CAPI RSA key container: %ProgramData%\Microsoft\Crypto\RSA\MachineKeys for the machine key set (the tool's import),
		///%APPDATA%\Microsoft\Crypto\RSA\(the current user's SID) for the user's.</summary>
		internal static string GetCapiKeyFilePath(bool isMachineKey,string uniqueKeyContainerName) {
			if(string.IsNullOrEmpty(uniqueKeyContainerName)) {
				return null;
			}
			if(isMachineKey) {
				return Path.Combine(GetCryptoFolder(true),"RSA","MachineKeys",uniqueKeyContainerName);
			}
			using(WindowsIdentity windowsIdentity=WindowsIdentity.GetCurrent()) {
				return Path.Combine(GetCryptoFolder(false),"RSA",windowsIdentity.User.Value,uniqueKeyContainerName);
			}
		}

		///<summary>The file of a CNG key: %ProgramData%\Microsoft\Crypto\Keys for the machine key set (the tool's import),
		///%APPDATA%\Microsoft\Crypto\Keys for the user's. Null for an ephemeral key, which has no file.</summary>
		internal static string GetCngKeyFilePath(bool isMachineKey,string uniqueName) {
			if(string.IsNullOrEmpty(uniqueName)) {
				return null;
			}
			return Path.Combine(GetCryptoFolder(isMachineKey),"Keys",uniqueName);
		}

		private static string GetCryptoFolder(bool isMachineKey) {
			Environment.SpecialFolder specialFolder=isMachineKey ? Environment.SpecialFolder.CommonApplicationData
				: Environment.SpecialFolder.ApplicationData;
			return Path.Combine(Environment.GetFolderPath(specialFolder),"Microsoft","Crypto");
		}

		private static bool IsStoreException(Exception ex) {
			return ex is CryptographicException || ex is UnauthorizedAccessException || ex is SecurityException;
		}

		///<summary>Releases the certificate objects. Their keys stay where they are (PersistKeySet, or a store's).</summary>
		private static void ResetAll(X509Certificate2Collection x509Certificate2Collection) {
			foreach(X509Certificate2 x509Certificate2 in x509Certificate2Collection) {
				x509Certificate2.Reset();
			}
		}

		///<summary>The text, trimmed, ending with a full stop, so that the next sentence can follow it.</summary>
		private static string EndSentence(string text) {
			text=(text??"").Trim();
			if(text=="" || text.EndsWith(".") || text.EndsWith("!") || text.EndsWith("?")) {
				return text;
			}
			return text+".";
		}

		///<summary>The sentence after the text, with a space between them; either may be blank.</summary>
		private static string AppendSentence(string text,string sentence) {
			if(string.IsNullOrEmpty(sentence)) {
				return text;
			}
			return string.IsNullOrEmpty(text) ? sentence : text+" "+sentence;
		}
	}
}
