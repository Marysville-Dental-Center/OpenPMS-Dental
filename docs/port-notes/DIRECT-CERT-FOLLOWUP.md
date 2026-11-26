# Port note: DIRECT-CERT-FOLLOWUP (the owner's answers to DIRECT-CERT's questions)

Branch `fix/directcert-followup-1`, cut from `main` (81e701df5, with DIRECT-CERT merged). The owner approved all three parts on
2026-11-09: the CAPI key fix for the connection settings tool, the deletion of the empty Customer Updates shell, and the two other
questions of DIRECT-CERT decided as built. No schema change, no new enumeration value stored anywhere, no new third-party file, no
download. Synthetic certificates and throwaway keys only; no network.

## 1. The connection settings tool deletes and names a CAPI key through CAPI

### What DIRECT-CERT found

DIRECT-CERT's third fix round (7cab838be, its tests 78777bf2e, `docs/port-notes/DIRECT-CERT.md`, "Found by the tests, fixed in
7cab838be" and "CAPI and CNG keys"): Windows stores the private key of a `.pfx` that names no CNG provider (OpenSSL writes none, so most
providers' files) in a CAPI key container, and so a `.pfx` that names a CAPI provider. .NET Framework's `GetRSAPrivateKey` opens such a
key **through CNG**: it gives an `RSACng`, not an `RSACryptoServiceProvider`. That `RSACng`'s `Key.Delete` returns without deleting the
CAPI container, and its `Key.UniqueName` is no file of the CNG folder (`Crypto\Keys`). DIRECT-CERT's fix: look for the CAPI key first,
as the certificate's own `PrivateKey` (`X509Certificate2.PrivateKey as RSACryptoServiceProvider`, made from the certificate's key provider
information, so it opens the container in the key set the key really lives in: user or machine), and delete it through CAPI
(`PersistKeyInCsp=false`, then `Clear`, which deletes the container) and name its file through `CspKeyContainerInfo`
(`UniqueKeyContainerName`, under `Crypto\RSA\MachineKeys` for the machine key set).

### What was wrong in `ClientCertificateStore`

`OpenPmsDentalDbSettings/ClientCertificateStore.cs` (the connection settings tool's client certificate, owner decision Q2 and the answer
(b) of 2026-11-02) had the pattern DIRECT-CERT fixed: `DeletePrivateKey` and `GetPrivateKeyFilePath` asked `GetRSAPrivateKey` and
branched on `RSACng` first, `RSACryptoServiceProvider` second. On .NET Framework the second branch was never reached for a CAPI key:

- `RemoveImported` (a client certificate the tool imported and the saved settings no longer use) took the certificate out of
  LocalMachine\My, but `RSACng.Key.Delete` left its CAPI key container in `%ProgramData%\Microsoft\Crypto\RSA\MachineKeys`, on the
  computer, with no certificate left to find it by.
- `ImportPfx` named the key file in the CNG folder, which has no such file, so `GrantUsersReadToPrivateKey` failed and the import warned
  "the users of this computer may not be able to use its private key: The private key file was not found." The program, which runs as
  the signed-in user, then could not use the client certificate until an administrator gave Users read in certlm.msc.

Measured on this PC (Windows 11, .NET Framework 4.8) with throwaway keys of the current user only (no certificate store), before
writing the tests: a certificate whose key is in a CAPI container of the "Microsoft Enhanced RSA and AES Cryptographic Provider" gives an
`RSACng` from `GetRSAPrivateKey` (its CNG provider name is the CAPI provider's); `CngKey.Exists` is true before and after its
`Key.Delete`, and the container's file under `%APPDATA%\Microsoft\Crypto\RSA\<SID>` is still there; no file of its `UniqueName` is in
`Crypto\Keys`. Deleted through the certificate's `PrivateKey` (`PersistKeyInCsp=false`, `Clear`), the file is gone and the container no
longer opens through CAPI. A CNG RSA and a CNG ECDSA key are deleted by `Key.Delete` (file gone, `CngKey.Exists` false). The tests below
pin each of these.

### The fix (`ClientCertificateStore`)

- `GetCapiRsaPrivateKey` (as DIRECT-CERT's: `PrivateKey as RSACryptoServiceProvider`, null for a CNG key, "Invalid provider type
  specified", or an ECDSA key, `NotSupportedException`) comes first in `DeletePrivateKey` (`PersistKeyInCsp=false`, `Clear`) and in
  `GetPrivateKeyFilePath` (`CspKeyContainerInfo.UniqueKeyContainerName`). The CNG branches follow as before; the `RSACryptoServiceProvider`
  branch after `GetRSAPrivateKey` stays for .NET, which gives a CAPI key so.
- `GetPrivateKeyFilePath` names the file **in the key set the key lives in** (`CspKeyContainerInfo.MachineKeyStore`, `CngKey.IsMachineKey`):
  `GetCapiKeyFilePath` gives `%ProgramData%\Microsoft\Crypto\RSA\MachineKeys\<unique name>` for the machine key set (the tool's import,
  `MachineKeySet`) and `%APPDATA%\Microsoft\Crypto\RSA\<the user's SID>\<unique name>` for the user's; `GetCngKeyFilePath` gives
  `Crypto\Keys\<unique name>` of `%ProgramData%` or `%APPDATA%`. Before, a CAPI key was always named in `RSA\MachineKeys` and a CNG key in
  `%ProgramData%`'s `Keys`; for the tool's own imports (always the machine key set) the result is the same, so nothing changes for them
  but the CAPI case.
- The key objects of `GetRSAPrivateKey` and `GetECDsaPrivateKey`, and the `CngKey` read for a file name, are disposed, as DIRECT-CERT's are.
- `RemoveImported(thumbprint)` is the tool's (LocalMachine\My, as before; `DbSettingsRunner` calls it so). An internal overload takes the
  store's location and name, so the tests use a store of the current user with a name of its own instead of CurrentUser\My (the old test
  used CurrentUser\My, a real store of this PC).

### Every other place that deletes or names a private key

Searched the whole repository (`PersistKeyInCsp`, `CspKeyContainerInfo`, `Key.Delete`, `CngKey`, `UniqueKeyContainerName`,
`KeyContainerName`, `PersistKeySet`, `MachineKeySet`, `UserKeySet`, `DeleteKey`, in `.cs` and `.ps1` files):

| Place | What it does | Verdict |
|---|---|---|
| `OpenPmsDentalDbSettings/ClientCertificateStore.cs` `DeletePrivateKey`, `GetPrivateKeyFilePath` | delete and name the client certificate's key | **Fixed** (above). |
| `OpenPmsDentalBusiness/Email/DirectCertificateImport.cs` `TryDeletePrivateKey`, `GetKeyContainerName` | delete and name the Direct certificate's key | Safe: DIRECT-CERT's own fix, the CAPI key first through the certificate's `PrivateKey`, so in its key set; `GetCapiKeyContainerName` and `GetCngKeyContainerName` include user or machine. |
| `DirectCertificateImport.GetMachinePrivateKeyFilePath` | names the key file for the users' read access | Safe: the CAPI key first; it names only the machine folders, and it is called only for the Local Computer store (`EmailMessages.CertStoreLocation==StoreLocation.LocalMachine`), whose import uses `MachineKeySet` (the same condition sets the flags). |
| `tests/LibraryUpdateTests/DirectCertificateImportTests.cs` `KeyLocation` | the tests' check that a key still exists | Named a CAPI key through CAPI already; it opened it without the key set (always the user's). **Changed:** it records the key set (`MachineKeyStore`, `IsMachineKey`) and opens the key in it. The tests' keys are all the user's, so they behave as before. |
| `tests/DbConnectionSecurityTests/OwnerAnswersTests.cs` `GetKeyName`, `KeyExists` | the old test's check | Named a CAPI key through `GetRSAPrivateKey` (the CNG view), opened a CAPI container without the key set, and used CurrentUser\My. **Replaced** by `ClientCertificateTestStore` and `TestKey` (below). |
| `OpenPmsDentalBusiness/WebBridges/PDMP/PdmpCertificate.cs` `Load` (`UserKeySet`), `CodeBase/Utilities/VendorDirect/VendorHttp.cs` `LoadClientCertificate` (`EphemeralKeySet`, or `UserKeySet`) | load a client certificate for one TLS call | Safe, not the same pattern: no `PersistKeySet`, so Windows' key is temporary and .NET Framework deletes it when the certificate is disposed (the callers dispose it); nothing is deleted or named by hand. |
| `tests/DbConnectionSecurityTests/DevDbTlsScriptTests.cs` `AssertKeyMatches` | `CngKey.Import` of a PKCS#8 file | Safe: an ephemeral key in memory; nothing is stored. |
| `ImagingCapture/UI/FrmCaptureSession.xaml.cs`, `WpfControlsOD/.../CtrlEFormFill.xaml.cs` `Key.Delete` | the keyboard's Delete key | Not a private key. |
| `third-party/PDFsharp` tests | PDFsharp's own signing tests | Not this program's code (a submodule); not touched. |
| `.ps1` scripts | | None deletes or names a private key. |

## 2. The empty Customer Updates shell is removed

Every member of Open Dental's "Customer Updates" web service had been deleted with its last caller, the e-mail certificate registration
last (DIRECT-CERT). What was left, `OpenPmsDentalBusiness/WebServices/CustomerUpdatesProxy.cs` (35 lines: the empty `ICustomerUpdates`,
the empty `CustomerUpdatesNoHq` and `CustomerUpdatesProxy` with `MockCustomerUpdates` and `GetWebServiceInstance`), had no caller in the
program. The owner deleted it (a deliberate deletion, so the tests that pinned the shell go with it).

Every reference, found by searching the whole repository (code, project files, tools, docs):

- **Deleted:** the file. `OpenPmsDentalBusiness.csproj` compiles its folder by the SDK's default glob and lists no item for it, so no
  project file changes. `HqNotAvailableException` (in `WebServiceMainHQNoHq.cs`) stays: `WebServiceMainHQNoHq` throws it.
- **Tests that pinned the shell, changed to check it is gone:**
  - `NoHqTests/NoHqTests.cs`: `CustomerUpdates_FactoryReturnsNoHqAndEveryMethodThrows` (the factory gave `CustomerUpdatesNoHq`, every
    method threw) and `CustomerUpdates_VendorMembersAreRemoved` (eight member names) become `CustomerUpdates_TheEmptyShellIsRemoved`: the
    three types do not resolve (`TypeLookup.AssertRemoved`, which first resolves `WebServiceMainHQProxy` the same way), the file is gone,
    and no `.cs` file of `OpenPmsDentalBusiness`, `OpenPmsDental`, `WpfControlsOD`, `CodeBase`, `OpenPmsDentalServer` or `UnitTests` names
    them (each folder must exist). The `SetUp` no longer resets `MockCustomerUpdates`.
  - `NoHqTests/HqToolsRemovedTests.cs` `HqTools_HostedServiceMembersOfTheHqWebServiceAreRemoved`: the three types do not resolve, instead
    of six members of the interface and `SendAndReceiveUpdateRequestXml` of the proxy.
  - `tests/VendorDirectTests/BridgesRemoval/BridgesDllRemovedTests.cs` `HqProvisioningLeftovers_AreGone`: `ICustomerUpdates` does not
    resolve, instead of having no `RequestCustomerID`.
- **Kept, still passing:** the source checks that a file does not name `CustomerUpdatesProxy` or `CustomerUpdates`
  (`DirectCertificateImportTests` for the certificate window, `CanadaCleanupTests`, `ErxSourceTests`, `OIDInternalsTests`,
  `QboLayoutTests`): they check other files, and stay true. No `LibraryUpdateTests` pin named the types.
- **Not changed:** `tools/project-baseline/items.txt` still has `OpenPmsDentalBusiness|Compile|WebServices\CustomerUpdatesProxy.cs|`. The
  record is taken by `tools/Get-ProjectBaseline.ps1` after a full build, and the lead re-records it after the merges; this is the one
  item this package removes on purpose. The design notes of `docs/specs/vendor-direct/` describe the shell as it was when they were
  written and are left as they are; `WebServiceCustomerUpdates` in comments, `DocumentationBuilder` and the database conversions is Open
  Dental's server, not the shell.

## 3. The owner's two other answers

Recorded in `docs/port-notes/DIRECT-CERT.md` ("The owner's answers (2026-11-09, DIRECT-CERT-FOLLOWUP)"), its O-13 entry in
`docs/specs/net10-plan.md` section 12, and `docs/decisions/owner-questions.md` ("Direct certificates follow-up"):

- (a) `BUILTIN\Users` keep read access to the Direct private key, as built.
- (b) A replaced certificate's private key is deleted only if the certificate window imported it, as built.

No code changed for these.

## Also changed

- `docs/specs/db-connection-security.md` section 4.3: a CAPI key is deleted and named through CAPI.
- `docs/release-notes/next.md` ("A replaced client certificate is removed"): for every kind of key, and the users' read access to a CAPI
  key's file. Database connection security is new in the next version, so this is no fix of a released behaviour.
- `docs/specs/net10-plan.md`: the spot check that names `CustomerUpdatesProxy` says it was deleted.

## Tests

`tests/DbConnectionSecurityTests` (SDK-style, every `.cs` file of its folder compiled; no project file change):

- `ClientCertificateTestStore.cs` (helper): a store of the current user with a name of its own (`OpenPmsDbSecTest_<guid>`), made on the
  first add and deleted on `Dispose` (`CertUnregisterSystemStore`), and throwaway keys of the current user's key set, each imported from a
  `.pfx` made in memory as the tool imports one (`PersistKeySet`, never `Exportable`; `UserKeySet` instead of `MachineKeySet`, which needs
  administrator rights): **a CAPI key** (made in a scratch CAPI container of the "Microsoft Enhanced RSA and AES Cryptographic Provider",
  which is deleted before the import, so the `.pfx` names a CAPI provider and Windows stores its key in a CAPI container), **a CNG RSA
  key** and **a CNG ECDSA key** (ephemeral CNG keys, so the `.pfx` names the CNG provider). `TestKey` finds each key by itself, apart from
  the code under test (a CAPI key through CAPI), opens it through its own key store (CAPI or CNG) in its key set, and names its file;
  `Dispose` deletes every key still there. Nothing opens LocalMachine\My, CurrentUser\My or any other real store, and no real certificate
  or key is touched. The `.pfx` password is made at run time and kept in memory.
- `ClientCertificateKeyTests.RemoveImported_DeletesTheKeyOfEachKind_AndNothingElse`: for each of the three kinds, two certificates in the
  store; `RemoveImported` of one gives no warning, the store holds only the other, the removed one's key no longer opens (through CAPI
  for the CAPI key, CNG for the others) and its file is gone; the other's key and file stay and it still signs; a certificate that is not
  there and a blank thumbprint give nothing.
- `ClientCertificateKeyTests.CapiKey_IsNamedAndDeletedThroughCapi_NotThroughTheCngViewOfIt`: DIRECT-CERT's finding for the tool's key, on
  .NET Framework: `GetRSAPrivateKey` gives the CAPI key as an `RSACng` whose `UniqueName` is no file of the CNG folder, and whose
  `Key.Delete` (the tool's old way, done once by hand) leaves the container and its file; `GetPrivateKeyFilePath` names the CAPI file, and
  `RemoveImported` deletes the container and the file.
- `ClientCertificateKeyTests.GetPrivateKeyFilePath_NamesTheKeyFileInTheKeySetOfTheKey`: the machine folders (`%ProgramData%`'s
  `RSA\MachineKeys` and `Keys`) and the user's (`%APPDATA%`'s `RSA\<SID>` and `Keys`); no name, no file; for each kind the path the test
  found by itself, which exists; a certificate without its key gives none.
- `OwnerAnswersTests.RemoveImported_TakesTheCertificateAndItsPrivateKeyOutOfTheStore`: as before (the certificate of
  `TestHelpers.NewCertificate`, a CNG key after the import), in the test store instead of CurrentUser\My.

After each run no key file of the tests is left under `%APPDATA%\Microsoft\Crypto\Keys` or `RSA\<SID>`, and no `OpenPmsDbSecTest_` store
is left (checked by hand after the runs). The experiment before the tests left one CNG key behind when its script stopped; it was found by
its file (created by the script seconds before, RSA 2048, opened by the name in it) and deleted.

Targeted runs (`run-tests.ps1 -Mode Targeted`), after the full lane build (0 errors):

| Assembly | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| NoHqTests | 57 | 57 | 0 | 0 |
| VendorDirectTests | 922 | 922 | 0 | 0 |
| LibraryUpdateTests | 126 | 126 | 0 | 0 |
| DbConnectionSecurityTests | 153 | 143 | 0 | 10 |

The ten skipped DbConnectionSecurityTests are the DevDbTls server tests, inconclusive until the local MariaDB runs with TLS (DB-TLS), as
before this package.

Not tested here: the machine key set and LocalMachine\My, which need administrator rights (the launch checks below).

## Launch checks for the lead

On a test computer, with a test database, as a Windows administrator, and with generated certificates only. A client certificate of a
CAPI key (in `Cert:\CurrentUser\My` only to export it):

```powershell
$cert = New-SelfSignedCertificate -Type Custom -Subject "CN=synthetic-workstations" -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" `
  -KeySpec KeyExchange -KeyUsage DigitalSignature,KeyEncipherment -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2") `
  -CertStoreLocation Cert:\CurrentUser\My -NotAfter (Get-Date).AddDays(30)
$password = Read-Host -AsSecureString -Prompt "A throwaway password"
Export-PfxCertificate -Cert $cert -FilePath .\client-capi.pfx -Password $password
Remove-Item ("Cert:\CurrentUser\My\" + $cert.Thumbprint) -DeleteKey
```

Without `-Provider` and `-KeySpec` the same command makes a CNG key: export that one as `client-cng.pfx` (the second check). These are
self-signed, so the database server does not accept them; the checks are about the key only, and the connection settings are put back
afterwards.

1. **A CAPI import.** With the connection settings tool, import `client-capi.pfx` (window, or `/pfx=` with `PfxPassword` in a
   `/secretsfile`): no warning about the users' access. In certlm.msc, Personal > Certificates holds it; All Tasks > Manage Private Keys
   shows Users with Read; its key file is in `%ProgramData%\Microsoft\Crypto\RSA\MachineKeys` (the file named by
   `certutil -store My <thumbprint>`'s "Unique container name").
2. **A CNG import** (DCFU2). Import `client-cng.pfx` the same way: the result has no warning that "the users of this computer may not be
   able to use its private key". In certlm.msc, All Tasks > Manage Private Keys on it shows Users with Read, and so does the Security tab
   of its key file in `%ProgramData%\Microsoft\Crypto\Keys` (the file named by `certutil -store My <thumbprint>`'s "Unique container
   name"; `certutil` shows the provider as "Microsoft Software Key Storage Provider").
3. Save the settings with another certificate (the other generated `.pfx`) or with the password: the tool removes the first certificate,
   and its key file is gone from `RSA\MachineKeys`. The same with the CNG one replaced in turn: its file is gone from
   `%ProgramData%\Microsoft\Crypto\Keys`.
4. **A `.pfx` with its CA certificate's key** (DCFU2). Make one, as above, in `Cert:\CurrentUser\My` only to export it (CNG keys):

   ```powershell
   $ca = New-SelfSignedCertificate -Type Custom -Subject "CN=synthetic-office-ca" -KeyUsage CertSign,CRLSign `
     -TextExtension @("2.5.29.19={text}CA=true") -CertStoreLocation Cert:\CurrentUser\My -NotAfter (Get-Date).AddDays(60)
   $client = New-SelfSignedCertificate -Type Custom -Subject "CN=synthetic-workstations" -Signer $ca -KeyUsage DigitalSignature,KeyEncipherment `
     -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2") -CertStoreLocation Cert:\CurrentUser\My -NotAfter (Get-Date).AddDays(30)
   $both = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2Collection
   [void]$both.Add($client); [void]$both.Add($ca)
   $plain = Read-Host -Prompt "A throwaway password"   # Export takes the password as plain text
   [IO.File]::WriteAllBytes("$PWD\client-with-ca.pfx", $both.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx, $plain))
   Remove-Item ("Cert:\CurrentUser\My\" + $client.Thumbprint) -DeleteKey; Remove-Item ("Cert:\CurrentUser\My\" + $ca.Thumbprint) -DeleteKey
   ```

   List the files of `%ProgramData%\Microsoft\Crypto\Keys`, import `client-with-ca.pfx` with the tool, and list them again: one new file
   only, the client certificate's (its "Unique container name"); LocalMachine\My holds the client certificate and not the CA's.
5. **The same `.pfx` again** (DCFU2 fix round). While LocalMachine\My holds a certificate the tool imported (`client-capi.pfx`, then
   `client-cng.pfx`), note its "Unique container name" (`certutil -store My <thumbprint>`) and list the files of
   `%ProgramData%\Microsoft\Crypto\Keys` and `RSA\MachineKeys`; import the same file again with the tool (or run setup again with the same
   `/DBPFX=`), and list them again: no new file, the same "Unique container name", no warning, and Manage Private Keys still shows Users
   with Read.
6. **The client certificate is chosen by what it is** (DCFU3, decision 1). Make `ca-first.pfx` as in check 4, but with the CA's
   certificate added to `$both` first (`[void]$both.Add($ca); [void]$both.Add($client)`), and `two-clients.pfx` with two client
   certificates made as `$client` is but without `-Signer` (two `New-SelfSignedCertificate` calls, both added to a new `$both`, no CA).
   List the files of `%ProgramData%\Microsoft\Crypto\Keys`; import `ca-first.pfx`: LocalMachine\My holds the client certificate
   (`CN=synthetic-workstations`), not the CA's, and one new key file only. Import `two-clients.pfx`: refused before anything is imported
   (the check reads the file without storing a key), naming both certificates with their thumbprints; no new key file, nothing new in
   LocalMachine\My.
7. **An entry whose key is in the administrator's own key set** (DCFU3, decisions 2 and 6). With the settings on the password and no copy
   of `client-cng.pfx`'s certificate in LocalMachine\My (remove one the checks above left with `Remove-Item ... -DeleteKey`), put that
   certificate into LocalMachine\My with its key in your own user key set, as a script that reads it without `MachineKeySet` would:

   ```powershell
   $plain = Read-Host -Prompt "The throwaway password"
   $c = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2("$PWD\client-cng.pfx", $plain, "UserKeySet,PersistKeySet")
   $s = New-Object System.Security.Cryptography.X509Certificates.X509Store("My", "LocalMachine"); $s.Open("ReadWrite"); $s.Add($c); $s.Close()
   ```

   `certutil -store My <thumbprint>` names its key container ("Key Container"). Import `client-cng.pfx` with the tool: the result says
   that the entry's private key was in the user key set of the account that put it there, that the import gave it the private key it
   stored, and that the earlier key was left untouched, with its provider and container, and ends "The tool's record keeps it, and the
   tool names that key on the entry again when this import is undone or a later save stops using this certificate, so do not delete that
   key while the record names it." (no `certutil -delkey` command: DCFU4 second fix round, finding 2). `certutil -store My <thumbprint>` now names a new container, whose file in `%ProgramData%\Microsoft\Crypto\Keys` gives Users
   Read; `ConnectionSettings\imported-client-certificate.txt` of the program folder starts with `Open PMS - Dental imported client
   certificate, format 2` and holds the thumbprint, the new key (`tool.container=`, `tool.keyset=machine`) and the earlier one
   (`earlier.container=`, `earlier.keyset=user`). Keep the earlier key for check 8.
8. **The tool gives the entry its earlier key back** (DCFU3, decision 6; DCFU4 round 4, part B). Save the settings with the password: the
   result says that the certificate "was in the computer's certificate store (LocalMachine\My) before the tool's import gave it a private
   key: its entry names its earlier private key again (...), as before that import, and the private key the tool gave it was deleted",
   then "If someone deleted that earlier key since, the entry names a key that is not there: then remove the certificate as an
   administrator, in PowerShell: Remove-Item Cert:\LocalMachine\My\<thumbprint>." LocalMachine\My still holds the certificate,
   `certutil -store My <thumbprint>` names the container of check 7's first `certutil` again, `certutil -user -key "<container>"` still
   finds that key, the new key file of check 7 is gone, and so is `imported-client-certificate.txt`. Then remove the certificate with
   `Remove-Item Cert:\LocalMachine\My\<thumbprint>` and delete the earlier key as yourself with `certutil -user -csp "<provider>" -delkey
   "<container>"` (the provider and container check 7's result named).
9. **No PIN for another certificate's key** (DCFU3, decision 3), on a test computer with a TPM: put a certificate with a TPM key into
   LocalMachine\My (`New-SelfSignedCertificate -Subject "CN=synthetic-tpm" -Provider "Microsoft Platform Crypto Provider"
   -CertStoreLocation Cert:\LocalMachine\My`), then do checks 4 and 3 again: no prompt, no error, and the TPM certificate stays; remove it
   afterwards with `Remove-Item Cert:\LocalMachine\My\<thumbprint> -DeleteKey`.
10. **A usable key the tool did not create** (DCFU3 fix round, review finding 2). With the settings on the password and no copy of
   `client-cng.pfx`'s certificate in LocalMachine\My, put it there as an administrator would, with a machine key:
   `Import-PfxCertificate -FilePath .\client-cng.pfx -CertStoreLocation Cert:\LocalMachine\My -Password $password`. Note its "Unique
   container name" (`certutil -store My <thumbprint>`) and check the Security tab of that file in `%ProgramData%\Microsoft\Crypto\Keys`:
   no Users. Import `client-cng.pfx` with the tool: the result says that the certificate was already there "with a usable private key
   that this tool did not import, so the tool gives the program's users no read access to that key", that the import gave the entry the
   key it stored, and names the earlier key (provider "Microsoft Software Key Storage Provider", the noted container) with the sentence
   that asks that it not be deleted while the record names it, and no `certutil -delkey` command (DCFU4 second fix round).
   `certutil -store My <thumbprint>` names a new container, whose file gives Users Read; the earlier file still has no Users;
   `imported-client-certificate.txt` holds the thumbprint and both keys. Import the same file again: no warning, the same container, no new
   key file. Save with the password (DCFU4 round 4, part B): the result says the entry names its earlier private key again, and ends with
   the sentence about a key deleted since (check 8); the certificate is still in LocalMachine\My, `certutil -store My <thumbprint>` names the earlier container again, the new key file is gone,
   and the earlier file is still there, still without Users. Remove it with `Remove-Item Cert:\LocalMachine\My\<thumbprint> -DeleteKey`.
11. **A save that fails after the import** (DCFU3 fix round, review finding 1). With the settings on the password, list the files of
   `%ProgramData%\Microsoft\Crypto\Keys`, hold `FreeDentalConfig.xml` open in another PowerShell window
   (`$f = [IO.File]::Open("<program folder>\FreeDentalConfig.xml", "Open", "Read", "Read")`), and import `client-cng.pfx` with the tool:
   it fails with exit code 3, saying that the settings could not be saved and that the client certificate this save imported was removed
   again with its private key. LocalMachine\My does not hold it, the key files are as listed, and `imported-client-certificate.txt` is
   absent. `$f.Close()`, then import it again: it is saved and recorded.

   **Over a certificate an administrator put there** (DCFU3 second fix round, review finding 2). Save with the password (the tool removes
   the certificate), put `client-cng.pfx`'s certificate back as check 10 does (`Import-PfxCertificate`), note its "Unique container name"
   and list the files of `%ProgramData%\Microsoft\Crypto\Keys`. Hold `FreeDentalConfig.xml` open again and import `client-cng.pfx` with
   the tool: it fails with exit code 3. The error says that the settings could not be saved, and ends (DCFU4 round 4, part B): "The
   client certificate <thumbprint> was in the computer's certificate store (LocalMachine\My) before the tool's import gave it a private
   key: its entry names its earlier private key again (...), as before that import, and the private key the tool gave it was deleted. If
   someone deleted that earlier key since, the entry names a key that is not there: then remove the certificate as an administrator, in
   PowerShell: Remove-Item Cert:\LocalMachine\My\<thumbprint>." LocalMachine\My still holds the certificate, `certutil -store My
   <thumbprint>` names the noted container again, the key files are as
   listed (the earlier one still there, its Security tab still without Users), and `imported-client-certificate.txt` is absent.
   `$f.Close()`, and remove the certificate with `Remove-Item Cert:\LocalMachine\My\<thumbprint> -DeleteKey`. Since the DCFU4 fix round
   (finding 4) the error no longer gives the import's warning about the earlier key (the certificate was already there "with a usable
   private key that this tool did not import", with a `certutil ... -delkey` command): the entry uses that key again.
12. **Two providers with one container name** (DCFU3 second fix round, review finding 1). With the settings on the password and no copy of
   `client-capi.pfx`'s certificate in LocalMachine\My, put it there with a key of the CNG software provider, as an administrator might:
   `certutil -csp "Microsoft Software Key Storage Provider" -importpfx .\client-capi.pfx` (it asks for the password). `certutil -store My
   <thumbprint>` shows the provider and the "Key Container" name; note both. Check that `RSA\MachineKeys` has no CAPI container of that
   name (`certutil -csp "Microsoft Enhanced RSA and AES Cryptographic Provider" -key "<container>"` finds none). Import `client-capi.pfx`
   with the tool: the result says the certificate was already there "with a usable private key that this tool did not import", and names
   the earlier key (the Software Key Storage Provider and the noted container) with the sentence that asks that it not be deleted while
   the record names it, and no `certutil -delkey` command (DCFU4 second fix round). `certutil -store My <thumbprint>`
   now shows the CAPI provider; its key file in `RSA\MachineKeys` gives Users Read, and the earlier file in `%ProgramData%\Microsoft\Crypto\Keys`
   still has no Users; `imported-client-certificate.txt` holds the thumbprint. Save with the password (DCFU4 round 4, part B): the
   result ends with the sentence about a key deleted since (check 8), the certificate stays, `certutil -store My <thumbprint>` shows the Software Key Storage Provider and the noted container again, the CAPI key
   file is gone, and the earlier file is still there. Remove it with `Remove-Item Cert:\LocalMachine\My\<thumbprint> -DeleteKey`. (Whether
   the CAPI key takes the name the `.pfx` records is Windows' choice; the tests measured that it does. If it takes another name, this is
   check 10 again.)
13. Put the connection settings back, remove what is left with `Remove-Item Cert:\LocalMachine\My\<thumbprint> -DeleteKey`, delete the
   `.pfx` files and forget the passwords. Do checks 14 to 19 (DCFU4 round 4 and its fix rounds, in their sections at the end of this note) before this step.

The refused import and a key that cannot be deleted (DCFU2), a CAPI delete that Windows does not do (DCFU3, decision 4), a key provider
information that cannot be read (DCFU3, decision 3) and rights of `FreeDentalConfig.xml` that cannot be reset (DCFU3 fix round) are
checked by the tests only: Windows gives no simple way to make LocalMachine\My
refuse a certificate, a key refuse its deletion, or a certificate's property refuse to be read.

## For the .NET 10 switch package

`OpenPmsDentalDbSettings` is a .NET Framework 4.8 project today; when it moves:

- **The CAPI key.** `GetCapiRsaPrivateKey` reads `X509Certificate2.PrivateKey`, obsolete on .NET (SYSLIB0028). .NET's `GetRSAPrivateKey`
  is said to give an `RSACryptoServiceProvider` for a CAPI key, which the existing branches of `TryDeletePrivateKey` (`DeletePrivateKey`
  before DCFU2; `DeleteCapiKey` since DCFU3) and `GetPrivateKeyFilePath` handle (`GetKeyContainerName` is gone since DCFU3: containers
  are compared by `ReadKeyProvInfo`, a `crypt32.dll` call on `X509Certificate2.Handle` that .NET keeps); verify it with `ClientCertificateKeyTests` and
  `ClientCertificateImportTests` (the CAPI cases), then drop `GetCapiRsaPrivateKey`, as DIRECT-CERT's note says for its own copy.
  `CapiKey_IsNamedAndDeletedThroughCapi_NotThroughTheCngViewOfIt` shows the CNG view only when `GetRSAPrivateKey` gives an `RSACng`, so
  on .NET it checks the CAPI deletion alone.
- **CNG keys.** On .NET, `RSACng.Key` and `ECDsaCng.Key` return a new `CngKey` on each call; `GetPrivateKeyFilePath` disposes it and
  `Key.Delete()` disposes the one it deletes.
- **Loading.** `X509Certificate2Collection.Import(path,password,flags)` (`ImportCollection`) is obsolete from .NET 9 (SYSLIB0057):
  `X509CertificateLoader.LoadPkcs12CollectionFromFile`, with the same flags, never `Exportable`. The test helper's
  `new X509Certificate2(byte[],password,flags)` moves to `X509CertificateLoader.LoadPkcs12` the same way.
- **Key file rights.** `File.GetAccessControl` and `File.SetAccessControl` (`GrantUsersReadToPrivateKey`) are `FileInfo` extension methods
  of `FileSystemAclExtensions` on .NET, as DIRECT-CERT's note says.

## Noticed, not changed (for the lead)

- `ImportPfx` reads the `.pfx` with `PersistKeySet`, which writes the key of every certificate of the file that has one, and adds only
  the client certificate: the keys of the file's other certificates stay in the machine key set, and when the store refuses the client
  certificate its own key stays too. DIRECT-CERT's import deletes such keys ("What an import stores and takes back"); the tool does not.
  Not part of the three approved parts.
- `RemoveImported` takes the certificate out of the store before it deletes the key. When the key cannot be deleted, the warning says the
  certificate "could not be removed from the computer's certificate store" and to remove it in certlm.msc, but the certificate is already
  gone and only its key is left, which certlm.msc can no longer reach.

(Both done in the follow-up round, below.)

## Follow-up round (branch `fix/dcfu2-1`, 2026-11-09)

Cut from `fix/directcert-followup-1` (b1f47d940). The lead decided the two questions above (the same behaviour as DIRECT-CERT's import,
`OpenPmsDentalBusiness/Email/DirectCertificateImport.cs`) and passed on the reviewer's smaller points. The plan is edbc07c2a, the code
fbfcc27f7, the tests 5c895b8ea (with one more change of the code, item 1), these documents the commit after them. No schema change, no
new enumeration value stored anywhere, no new third-party file, no download.

### 1. `ImportPfx` deletes the keys it stored and does not use

`X509Certificate2Collection.Import(..., MachineKeySet | PersistKeySet)` stores the key of every certificate of the `.pfx` that has one,
each in a key container of its own (one container for a key that two certificates of the file share). DirectCertificateImport's way
("What an import stores and takes back", `PrivateImportRun`, `DeleteKeysNotInStore`, `DeleteKeysNotInUse`, `GetKeyContainerName`), now
the tool's too (`ClientCertificateStore`, a copy: `OpenPmsDentalDbSettings` references neither `OpenPmsDentalBusiness` nor anything that
has it):

- The keys this import stored are the file's certificates with a key, right after the read; only those are ever deleted, never a key
  that was on the computer before.
- The store is opened before the keys are stored, so a store that cannot be opened (no administrator rights) leaves nothing behind.
- **After the client certificate is in the store**, the keys of the file's other certificates (its CA's, for example) are deleted, but
  none that the client certificate or a certificate of the store uses (compared by key container: kind, provider, user or machine, unique
  name; each container once). A key that cannot be deleted gives the warning "The client certificate was imported, but a private key of
  another certificate of the file, which the tool stored and does not use, could not be deleted (subject, thumbprint): why. The key is
  left on this computer in <file>: delete that file as an administrator."
- **When anything stops the import before the certificate is in the store** (the store refuses it, the file's check refuses it, anything
  else), every key it stored is deleted, the client certificate's too, with the same exception for a key a certificate of the store uses.
  The error keeps its text and exit code; a key that cannot be deleted adds "A private key the tool stored could not be deleted again
  (subject, thumbprint): why. The key is left on this computer in <file>: delete that file as an administrator."
- **A `PersistKeySet` read that fails** gives no certificate, so no key is known; as DirectCertificateImport says, the error adds "Nothing
  was imported. Windows may have left a private key of the file behind on this computer, which the tool cannot find to delete." (Not
  for the check's `EphemeralKeySet` read, which stores nothing.)
- The store is read anew for the comparison (a new read-only handle), not through the import's open handle, which may keep the
  certificates it read when it was opened (Windows caches a system store per handle and resynchronises it on request,
  `CertControlStore`): a certificate another program put into the store meanwhile counts too. Added while writing the test that gives a
  certificate of the store the key the import just stored; the test was not run without it, so whether the open handle would have
  missed that certificate is not measured. DirectCertificateImport reads its open handle; nothing there needs it (nothing else adds to
  its store during an import).
- The users' read access to the key (`GrantUsersReadToPrivateKey`) is given for LocalMachine only, as DirectCertificateImport gives it;
  the tests' store of the current user imports into the user's key set (`UserKeySet`) and gives none (the key is the user's own).
- **A certificate the store already holds with a usable key** is not added again, and every key the import stored is deleted: added in
  the fix round below (this round still added it, which left the entry's earlier key behind).

The certificate the tool takes is still the file's first with a private key (`GetClientCertificate`, unchanged); see Noticed below.

### 2. `RemoveImported` when only the key cannot be deleted

**Order kept: the certificate first, then its key**, as DirectCertificateImport removes a replaced certificate (`RemoveReplaced`: the
store's `Remove`, then `DeleteKeysNotInStore` once they are all out) and takes an import back (`Undo`: a certificate the store does not
let go of keeps its key, "so that the store's entry still works"). The reasons hold for the tool: deleting the key first and then
failing to remove the certificate would leave in LocalMachine\My a certificate that says it has a private key (`HasPrivateKey`, which
`CheckClientCertificate` checks for `/thumbprint=`) whose key is gone; and the key is deleted only when no certificate left in the store
uses the same key container, which can be seen only once the certificate is out (the same `DeleteKeysNotInStore`; new for
`RemoveImported`, which before deleted the key whatever else used it).

**The warning says what happened.** When only the key delete fails: "The client certificate that this tool imported before was removed
from the computer's certificate store (LocalMachine\My), but its private key could not be deleted (subject, thumbprint): why. The key is
left on this computer in <file>: delete that file as an administrator." The file is named before the delete is tried
(`GetPrivateKeyFilePath`); one that cannot be named says so. When the certificate itself cannot be removed, both are still there and the
warning says so, as before, but no longer sends the user to certlm.msc, whose Delete leaves the private key: "Remove it with its private
key as an administrator, in PowerShell: Remove-Item Cert:\LocalMachine\My\<thumbprint> -DeleteKey (certlm.msc's Delete leaves the key)."

`TryDeletePrivateKey` (was `DeletePrivateKey`, which threw) gives why a key was not deleted and never throws, as DirectCertificateImport's.
A test seam (`ActionBeforeKeyDeleteForTests`) makes the delete fail.

### 3 to 7. The reviewer's smaller points

3. `ClientCertificateTestStore`: `CertUnregisterSystemStore`'s result is checked (a store never made gives ERROR_FILE_NOT_FOUND, measured,
   and is no problem), and the store's absence is verified afterwards. A throwaway key is recorded before it is inspected:
   `TestKey.Track` adds the entry to the store's list, keeps a copy of the certificate (the same certificate context, so the same key
   provider information), and only then finds the key; a key it never found is deleted through that copy at the clean-up. `Dispose`
   never throws, so it never hides a test's own failure: what it could not delete (a key, the store) is written to the test's output and
   kept for `ClientCertificateTestStore.AssertCleanedUp`, which `ClientCertificateKeyTests`, `ClientCertificateImportTests` and
   `OwnerAnswersTests` call after every test (`TestCleanup`).
4. `CapiKey_IsNamedAndDeletedThroughCapi_NotThroughTheCngViewOfIt`: the three checks of Windows' own behaviour (the CNG view's file name,
   and that `RSACng.Key.Delete` leaves the CAPI container and its file) say in their failure messages that they pin Windows 11 and .NET
   Framework 4.8, that a later Windows deleting or naming a CAPI key through its CNG view (the key storage provider) makes them fail while
   the tool is still right, and that then they, not the tool, change.
5. `GetCapiRsaPrivateKey` catches every exception but the critical ones (`IsCriticalException`: `OutOfMemoryException`,
   `StackOverflowException`, `ThreadAbortException`), as DirectCertificateImport's copy; so do the new `TryDeletePrivateKey`,
   `GetKeyContainerName` and `TryGetPrivateKeyFilePath`.
6. The launch checks have an explicit CNG import (check 2) and a `.pfx` with its CA's key (check 4), above.
7. `docs/specs/db-connection-security.md` section 4.3 (one line of 238 characters) and `ClientCertificateStore.GetCryptoFolder` (and the
   summary above it) are wrapped to the files' width.

Also changed: `docs/specs/db-connection-security.md` section 4.3 says what the import deletes and what `RemoveImported` reports, and
`docs/release-notes/next.md` ("A replaced client certificate is removed") that no other key of the `.pfx` is left and that a key left is
named with its file.

### Tests

`tests/DbConnectionSecurityTests` (every `.cs` file of its folder compiled; no project file change), each for a CAPI key, a CNG RSA key
and a CNG ECDSA key, in the test store with throwaway keys of the current user:

- `ClientCertificateImportTests` (new), through `ImportPfx`'s internal overload (the test store, the user's key set) and two seams
  (`ActionAfterKeysStoredForTests`, where the test records every key the import stored so that its clean-up deletes it whatever the code
  does, and `ActionBeforeStoreAddForTests`):
  - `ImportPfx_DeletesTheKeyOfTheFilesCaCertificate_AndKeepsTheClientCertificates`: a `.pfx` with a client certificate and the CA
    certificate that issued it, each with its key (`WritePfxWithCa`): the import stores both, takes the client certificate (its
    thumbprint), adds only it, gives no warning; the CA's key no longer opens and its file is gone; the client's stays and signs.
  - `ImportPfx_ARefusedImportDeletesEveryKeyItStored`: (1) the store refuses: the error, exit code 3, both keys and files gone, the store
    empty; (2) anything else stops it after the keys were stored: "The client certificate could not be imported: ..."; (3) the file's
    check refuses it after the keys were stored: its text and exit code 2 stay; (4) the store refuses and the CA's key cannot be deleted
    (a seam): the error names it, its subject and thumbprint and its file, which is still there, and the client's key is gone; (5) a wrong
    password: "Windows may have left a private key...", while the check's read (`LoadPfxForCheck`) does not say so.
  - `ImportPfx_NeverDeletesAKeyThatACertificateOfTheStoreUses`: the test gives a certificate of the store the key the import stored for
    the CA certificate (`AddCertificateUsingKeyOf`, checked by the key's file): after an import, and after a refused one (the client's key
    deleted), that key stays and the certificate signs.
- `ClientCertificateKeyTests`:
  - `RemoveImported_WhenOnlyTheKeyIsNotDeleted_SaysTheCertificateWasRemovedAndWhereItsKeyIs`: the seam makes the key delete fail; the
    warning is the text above with the test's own finding of the key's file, has no "could not be removed", the store is empty, and the key
    and its file are still there (the store's clean-up deletes them).
  - `RemoveImported_KeepsAKeyThatAnotherCertificateOfTheStoreUses`: a second certificate of the store with the same key container: the
    key stays and that certificate signs; when it is removed too, the key and its file go.
  - The three checks of Windows' behaviour in `CapiKey_IsNamedAndDeletedThroughCapi_NotThroughTheCngViewOfIt` (item 4).

After each run, no key file of the tests was left under `%APPDATA%\Microsoft\Crypto\Keys` or `RSA\<SID>` and no `OpenPmsDbSecTest_` store
under `HKCU\Software\Microsoft\SystemCertificates` (the file names of the two folders and the stores listed before the first run and after
each run, and compared; no new name). The test classes' own check (`AssertCleanedUp`) found nothing either.

Targeted runs (`run-tests.ps1 -Mode Targeted`), after the full lane build (0 errors):

| Assembly | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| NoHqTests | 57 | 57 | 0 | 0 |
| LibraryUpdateTests | 126 | 126 | 0 | 0 |
| DbConnectionSecurityTests | 158 | 148 | 0 | 10 |

The ten skipped DbConnectionSecurityTests are the DevDbTls server tests, inconclusive until the local MariaDB runs with TLS, as before.
DbConnectionSecurityTests has five tests more than after the first round (153): the three of `ClientCertificateImportTests` and the two
new ones of `ClientCertificateKeyTests`.

Not tested here, as before: the machine key set and LocalMachine\My, which need administrator rights (the launch checks above).

### Noticed, not changed (for the lead)

- `GetClientCertificate` (the check's and the import's) takes the file's first certificate with a private key. A `.pfx` that lists a CA
  certificate with its key first, and has a client certificate after it, gives the CA certificate, which passes `CheckClientCertificate`
  when it has no Extended Key Usage (most CA certificates have none); the window then shows the CA's subject before the import. The
  tests' files list the client certificate first (and .NET Framework's read keeps that order: the tool took the client certificate).
  Preferring a certificate with the client authentication purpose, or one that is not a CA, would change which certificate the tool
  takes: not part of this round.
- **Corrected, and fixed in the fix round below.** This item said: a `.pfx` whose certificate LocalMachine\My already has
  (`isPfxCertInStoreBefore`, "which the tool then never records or removes"): the import stores a new key for it and the store's add
  gives the store's entry that key, the key the entry had before stays on the computer, named by no certificate, and "the import never
  deletes a key that was there before, by the rule of this round"; "not measured for the tool (it needs LocalMachine\My)". The leak was
  real (measured in the fix round). The rest was wrong: the tool's own `.pfx` imported again keeps its record
  (`PlanImportedCertificate`), so the tool does record such a certificate and later removes it, with only its latest key; the rule of
  this round did not force the leak, since DirectCertificateImport does not add a certificate its store already holds with a usable key
  and deletes the key its read stored, which deletes no key that was there before; and `ImportPfx`'s internal overload shows the case in
  the test store, without LocalMachine\My.
- Building this branch through the lane needs `ImportDirectoryPackagesProps=false` in the environment: the worktree sits inside the main
  checkout, whose `Directory.Packages.props` (S2-1, central package versions, merged on `main` after this branch was cut) NuGet otherwise
  finds above the worktree. Merged with `main`, the branch has its own copy and needs nothing.

## Fix round (branch `fix/dcfu2-2`, 2026-11-09)

Cut from `fix/dcfu2-1` (363ed6c69). The review found that the follow-up round's last "Noticed" item (corrected above) was a key left
behind that the round's rule did not force; the lead decided to port DirectCertificateImport's rule into `AddClientCertificate`. The code
is 2d3b48fbc, the tests 82a6b009a, this note the commit after them. No schema change, no new enumeration value stored anywhere, no new
third-party file, no download.

### What was wrong

`AddClientCertificate` always called the store's `Add`. For a `.pfx` whose certificate LocalMachine\My already holds with a usable key,
the add gives the store's entry the key the import has just stored (the entry takes the added certificate's properties, as DIRECT-CERT's
third round found for the add), and the key the entry used before stays on the computer, named by no certificate. The realistic case:
setup run again on an existing install with the same `/DBPFX=` (`installer/OpenPmsDental.iss`, `DbSettingsParams`, passes it on as
`/pfx=`). `isPfxCertInStoreBefore` is true, and `PlanImportedCertificate` keeps the record, since it is the tool's own certificate
imported again; the new key gets Users Read, and the old one had it from the first import. Every re-run left one more copy, and when the
office later replaced the certificate, `RemoveImported` deleted only the entry's latest key: the earlier copies of the old, unexpired
private key stayed in `%ProgramData%\Microsoft\Crypto\Keys` or `RSA\MachineKeys`, readable by every user, which the owner decision of
2026-11-02 (no key the users may read is left behind) rules out.

Measured in the test store before the fix (the first new test below, against the follow-up round's import): after the import of the same
`.pfx`, the entry's key file was the import's (`...\Crypto\RSA\<SID>\1d756b95...`), not the one it had (`aefd1367...`), which still
existed.

### The fix: DirectCertificateImport's rule

DirectCertificateImport does not import a certificate its store already holds with a usable key (`CheckPrivateAgainstStore`: the entry
"stays as it is"), and deletes again the key its read stored for it (DIRECT-CERT.md, "The keys of the certificates it does not import
(another address's, one already in the store) are deleted again at once"). `ClientCertificateStore` now does the same:

- After the file's check (`GetClientCertificate`), `AddClientCertificate` looks for the client certificate's thumbprint in the store,
  read anew (`FindInStoreNow`, through a handle of its own, as `DeleteKeysNotInStore` reads it).
- An entry whose key opens and signs, or decrypts (`IsKeyUsable`, a copy of DirectCertificateImport's, which never throws) is not added
  again (`KeepCertificateInStore`). Every key this import stored, the client certificate's own too, is deleted with
  `GetKeysNotDeletedText(listKeysStored, x509Store, new List<X509Certificate2>(), ...)`; `DeleteKeysNotInStore` keeps the entry's key,
  which a certificate of the store uses, so no key that was there before is deleted. For LocalMachine the users get read access to the
  entry's key (`GrantUsersReadToPrivateKey` on the store's certificate), as to an imported one. The thumbprint is returned.
- Only when there is no entry, or its key cannot be used (deleted, or it does not open), is the certificate added, as before; the entry
  then takes the import's key.
- The kept case's warnings: "The client certificate was already in the computer's certificate store with a usable private key and stays
  as it is, but a private key of the file, which the tool stored and does not use, could not be deleted (subject, thumbprint): why. The key
  is left on this computer in <file>: delete that file as an administrator." and "..., but the users of this computer may not be able to
  use its private key: why. In certlm.msc, Personal > Certificates, right-click it, All Tasks > Manage Private Keys, and give Users Read."
- `DbSettingsRunner` is unchanged: the record follows the thumbprint as before (the tool's own certificate imported again keeps its
  record; a certificate put there another way is never recorded), and the entry the record names keeps the one key `RemoveImported`
  deletes when the certificate is replaced.

### Measured: where Windows stores the key of a `.pfx` imported again

Two diagnostics with throwaway keys of the current user (no certificate store), on this Windows 11, every key deleted again afterwards:
a `.pfx` written from a CAPI key records the name of its container, and the import (`UserKeySet`, `PersistKeySet`) names the container
it stores the key in after it when no container has that name; when one has (an earlier import's key still there), the import takes a
new name (a GUID). So the key of a CAPI `.pfx` imported again after the first import's key was deleted lands in the very container the
store's entry names: the entry's key is usable again, nothing is added, and the import's key is the entry's, which
`DeleteKeysNotInStore` keeps. A `.pfx` written from an ephemeral CNG key records no name, and each import gets a new one. The second test
below covers both ways.

### Tests

`tests/DbConnectionSecurityTests/ClientCertificateImportTests.cs`, each for a CAPI key, a CNG RSA key and a CNG ECDSA key, in the test
store with throwaway keys of the current user (`ImportPfx`'s internal overload):

- `ImportPfx_OfACertificateTheStoreHasWithAUsableKey_KeepsItsEntryAndKey_AndDeletesTheKeyItStored`: the test writes a `.pfx` and keeps it
  on disk (`WritePfx`), adds its certificate to the store from it (`AddCertificate(string)`), then imports the same file. The first key
  still exists, with its file, and is still the entry's key (`GetPrivateKeyFilePath` of the store's certificate against the test's own
  finding of the key); the key the import stored (tracked through `ActionAfterKeysStoredForTests`) is another one, and is deleted (it no
  longer opens, its file is gone); the certificate signs; no warning; the store's add was not called (`ActionBeforeStoreAddForTests`);
  one entry. (2) The same import again, with the seam that makes the key delete fail: the warning above, naming the import's key and its
  file, which is still there (the store's clean-up deletes it), and the entry still has its first key and signs. Before the fix the test
  failed (the measurement above).
- `ImportPfx_OfACertificateTheStoreHasWithoutAUsableKey_GivesItTheKeyTheImportStored`: (1) the test adds the certificate from one file,
  deletes the entry's key (the entry still says it has one; `IsKeyUsable` says it cannot be used), and imports another file of the same
  certificate and key (`WritePfxTwice`: for a CAPI key the second file records another scratch container, so the import's key gets a
  container of its own): the certificate is added, and the entry uses the import's key, which stays and signs; no warning. (2) The
  entry's key deleted again and the same file imported again: added exactly when the import's key is not in the container the entry
  names (CAPI: the same container, not added; CNG: a new key, added), and either way the entry uses the import's key, which stays, and
  signs.

`ClientCertificateTestStore`: `WritePfx`, `WritePfxTwice` and `AddCertificate(string)` (a `.pfx` file, imported as `AddCertificate`
imports one).

After each run, no key file of the tests was left under `%APPDATA%\Microsoft\Crypto\Keys` or `RSA\<SID>` and no `OpenPmsDbSecTest_` store
under `HKCU\Software\Microsoft\SystemCertificates` (listed before the first run and after each run, and compared: no new name), and the
test classes' own check (`AssertCleanedUp`) found nothing.

Targeted runs (`run-tests.ps1 -Mode Targeted`), after the full lane build (0 errors):

| Assembly | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| NoHqTests | 57 | 57 | 0 | 0 |
| LibraryUpdateTests | 126 | 126 | 0 | 0 |
| VendorDirectTests | 922 | 922 | 0 | 0 |
| DbConnectionSecurityTests | 160 | 150 | 0 | 10 |

The ten skipped DbConnectionSecurityTests are the DevDbTls server tests, inconclusive until the local MariaDB runs with TLS, as before.
DbConnectionSecurityTests has two tests more than after the follow-up round (158).

Not tested here, as before: the machine key set and LocalMachine\My, and so the users' read access to the kept entry's key (launch check
5).

### Noticed, not changed (for the lead)

- **An entry of LocalMachine\My whose key is in a user's key set.** `IsKeyUsable` runs as the administrator who runs the tool. A
  certificate put into LocalMachine\My another way, with its key in that administrator's own key set (a script that adds a certificate
  read without `MachineKeySet`, say), passes it, so the entry now stays, and the users' Read goes on a key file in that administrator's
  profile, which they cannot reach: the program cannot use the certificate for them. Before this round the add gave the entry the import's
  machine key. DirectCertificateImport has the same rule for its LocalMachine store. Counting an entry as usable only when its key is in
  the key set the import uses (the machine's, for the tool) would add it again in that case (and leave the administrator's own key, which
  the users cannot read). Not done: the lead's rule is DirectCertificateImport's; not testable here (it needs LocalMachine\My).
- **An entry without a usable key that the tool did not import** takes the import's key, as before this round, and the tool does not
  record that certificate (`isPfxCertInStoreBefore`, and no record of its own), so it does not remove that key when the certificate is
  replaced. Unchanged.

(Both done in the last round, below.)

## Last round (branch `fix/dcfu3-1`, 2026-11-09)

Cut from `fix/dcfu2-2` (e62d93b96). The owner's rule (2026-11-02, `docs/specs/db-connection-security.md` section 4.3): the tool gives
BUILTIN\Users read access to the client certificate's private key, so it must never leave a key the users can read behind, and must never
touch a key it did not create. The lead's decisions, the plan:

1. **Which certificate of the `.pfx` is the client certificate** (`GetClientCertificate`, the check's and the import's): the candidates are
   the certificates with a private key that are not certificate authorities (no Basic Constraints with CA=true); among them those whose
   Extended Key Usage has clientAuth, or that have none, are preferred. Exactly one is the client certificate; none or several are refused
   before anything is put into the store, naming what the file holds (subjects and thumbprints) and how to make a `.pfx` with one client
   certificate. Tests: a `.pfx` that lists its CA certificate with its key first imports the client certificate and keeps that one's key;
   one with two client certificates is refused; one with only a CA certificate is refused.
2. **A usable store entry** (`KeepCertificateInStore`) also needs its key in the key set the import uses (the machine's for
   LocalMachine\My, the user's for the test store), read from the certificate's key provider information. An entry whose key is in another
   key set is added again with the import's key, and the warning says that the entry's earlier key was left untouched, and where it is.
   Test: an entry of the test store whose key provider information names the other key set.
3. **No key of a certificate of the store is opened** to compare key containers (`DeleteKeysNotInStore`, `DeleteKeysNotInUse`, and the
   check of the entry's key set): each certificate's key container, provider and key set come from its `CERT_KEY_PROV_INFO_PROP_ID`
   (`CertGetCertificateContextProperty`), never `PrivateKey`, `GetRSAPrivateKey` or `GetECDsaPrivateKey`. A certificate whose property
   cannot be read uses an unknown key: no key is deleted then, and the result says so. Tests: a seam that sees every key the code opens;
   a seam that makes a certificate's property unreadable.
4. **A CAPI delete is checked** (`TryDeletePrivateKey`): `PersistKeyInCsp=false` and `Clear` do not report a failed `CRYPT_DELETEKEYSET`,
   so afterwards the container must no longer open (`CspParameters` with `UseExistingKey`, in its key set) and its file must be gone; a
   key still there is reported. Test with a seam that leaves the container.
5. **The review's points:** the test store's `TestKey` reports a key whose certificate copy could not be made, instead of reading the
   missing copy as nothing to delete; the spec's advice for a certificate the tool never removes is the `Remove-Item ... -DeleteKey`
   command, not certlm.msc; this note says why `AddClientCertificate`'s catch lets the critical exceptions through where
   `DirectCertificateImport.AddPrivate` catches everything.
6. **An entry without a usable key that the tool did not import** and that takes the import's key is recorded as the tool's own
   (`ImportPfx` says whether the store's entry uses the key the import stored; `PlanImportedCertificate` records it then), so that a later
   replace removes it with the key the tool created. Test: through `DbSettingsRunner.Apply` with the test store (a seam), an import over
   such an entry, then a save with the password, which removes the certificate and that key.

Tests never touch this PC's real stores or keys: only the test store and throwaway keys of the current user, deleted afterwards and checked
after every run. What only an administrator can check on LocalMachine\My goes into the launch checks.

The code is 60c400734 and the tests 958c30192. The spec, release notes and launch checks 6 to 9 are ee16aa868: they were written on
2026-11-09, then committed on 2026-11-13 when the round was resumed after the run that wrote them stopped. `main` was merged in
bcb3ba515, and this note is the commit after those. No schema change, no new enumeration value stored anywhere, no new third-party file,
no download.

### The six decisions, against the code and the tests

All six are done. Where each one is (in `OpenPmsDentalDbSettings`, tests in `tests/DbConnectionSecurityTests`):

1. **The client certificate** (`ClientCertificateStore.GetClientCertificate`, `IsCertificateAuthority`, `DescribeForRefusal`,
   `PFX_HOW_TO`). Of the file's certificates with a private key, those without Basic Constraints CA=true are the candidates. Of them, the
   ones for client authentication (`IsForClientAuthentication`: no Extended Key Usage, or one with clientAuth) are taken if there are any.
   Exactly one left is checked (`CheckClientCertificate`, so a single candidate that is not for client authentication is refused as
   such). None or several are refused with exit code 2: "The client certificate file <path> holds no client certificate: ..." or "...
   holds more than one certificate that can be the client certificate (...): <subject> (<thumbprint>); ...". The message goes on with
   "It holds: <subject> (<thumbprint>, with a private key, a certificate authority); ..." and ends with the `openssl pkcs12 -export` line of
   `docs/admin/db-connection-security.md` section 3. The check (`LoadPfxForCheck`, `EphemeralKeySet`) refuses the file before anything is
   stored. The import refuses it the same way after its read, and deletes every key the read stored. Tests:
   `ImportPfx_TakesTheClientCertificate_NotTheCaCertificateTheFileListsFirst` and
   `ImportPfx_RefusesAFileWithoutExactlyOneClientCertificate`. The CA-first test fails on the code before this round: the test CA has no
   Extended Key Usage, so the first certificate with a key (the CA's) passed `CheckClientCertificate`.
2. **A usable entry** (`IsEntryKeyUsable`, `GetEntryKeyLeftText`). The entry's key provider information is read first:
   - No key named: not usable (as before).
   - A key in the other key set, or one that cannot be identified: not usable, with this warning: "The client certificate was already in
     the computer's certificate store (LocalMachine\My), with its private key in the user key set of the Windows account that put it
     there, which the program's users cannot use, not in the computer's key set, which the import uses. The import gave the entry the
     private key it stored, and left the entry's earlier key untouched: provider <name>, key container <name>, in <key set>. That account
     can delete it, once no other certificate uses it: certutil -user -csp "<provider>" -delkey "<container>"."
   - A key in the import's key set: only then is the key opened, to sign or decrypt (`IsKeyUsable`).

   The entry's earlier key is never opened or deleted. Test:
   `ImportPfx_OfACertificateWhoseEntryHasItsKeyInAnotherKeySet_GivesItTheImportsKey_AndLeavesTheEarlierOne`.
3. **No key of the store opened to compare** (`ReadKeyProvInfo`, `KeyProvInfo`, `DeleteKeysNotInUse`; `GetKeyContainerName` is gone).
   `ReadKeyProvInfo` reads `CERT_KEY_PROV_INFO_PROP_ID` with `CertGetCertificateContextProperty` on the certificate's context
   (`X509Certificate2.Handle`). It takes the container, the provider, the provider type and the machine-key-set bit of `dwFlags`
   (`CRYPT_MACHINE_KEYSET` and `NCRYPT_MACHINE_KEY_FLAG` are both 0x20). `CRYPT_E_NOT_FOUND` means a certificate without a key. Any other
   failure, an empty container name or an exception means an unknown key. `DeleteKeysNotInUse` reads all of them before the first delete
   and compares key set and container. It leaves the provider out: Microsoft's software CAPI providers share containers, so two names of
   one container count as one key, and a key is kept rather than deleted. When a certificate that stays has an unknown key, no key is
   deleted. Each warning then says "It was kept, because the private key of the certificate <subject> (<thumbprint>), which stays, could
   not be identified (<why>), and it may be this one." and "delete that file as an administrator only once no certificate of the store
   uses it". The tool opens only these keys:
   - the ones it stored;
   - the one it removes (its own, recorded);
   - the client certificate's own entry in the import's key set (decision 2).

   Every opening goes through `GetCapiRsaPrivateKey`, `OpenRsaPrivateKey` or `OpenECDsaPrivateKey`, which call the seam
   `ActionBeforePrivateKeyOpenForTests`. Test: `DeleteKeys_OpenNoKeyOfAnotherCertificateOfTheStore_AndKeepEveryKeyWhenOnesKeyIsUnknown`.
4. **A CAPI delete checked** (`DeleteCapiKey`, `DoesCapiKeyContainerOpen`). `DeleteCapiKey` first reads the container's provider, type,
   name, key set, key number and file (`GetCapiKeyFilePath` of its unique name). Then it deletes the key (`PersistKeyInCsp=false`,
   `Clear`), opens the container again (`UseExistingKey | NoPrompt`, plus `UseMachineKeyStore` for the machine key set) and looks for the
   file. A key that is still there is reported as "Windows reported no error, but the key is still there: its key container still opens,
   and its file is still there." (or one of the two), followed by where the key is left. The seam `ActionInsteadOfCapiKeyDeleteForTests`
   stands in for a delete that does nothing. Test: `RemoveImported_ACapiKeyThatIsStillThereAfterItsDelete_IsReported`.
5. **The review's points.**
   - `ClientCertificateTestStore`: `TestKey.Track` takes the thumbprint and subject before it copies the certificate. When the copy could
     not be made, `DeleteThroughCertificate` says "The test key of <subject> (<thumbprint>) was never located, and the copy of its
     certificate could not be made, so the clean-up cannot find it to delete it. ..." instead of "" (nothing to delete).
   - The spec (section 4.3) gives `Remove-Item Cert:\LocalMachine\My\<thumbprint> -DeleteKey` for a certificate the tool never removes.
   - **Why `AddClientCertificate`'s catch is `when(!IsCriticalException(ex))`.** The tool's "never throws" helpers
     (`TryDeletePrivateKey`, `ReadKeyProvInfo`, `IsKeyUsable`) all let `OutOfMemoryException`, `StackOverflowException` and
     `ThreadAbortException` through. After one of those nothing the tool does is reliable, and the clean-up in that catch deletes keys,
     the riskiest thing the tool does. So the process ends and setup sees a failed tool, not a result that may be wrong. The owner's rule
     still holds: nothing in that catch's `try` gives the users read access to a key (`GrantUsersReadToPrivateKey` runs only after the
     certificate is in the store), so a key left by a critical exception is one the users cannot read.
     `DirectCertificateImport.AddPrivate` catches everything because it runs inside the program, which must turn every failure into the
     certificate window's result and undo (`PrivateImportRun.Undo`). Its own helpers filter the same way the tool's do.
6. **The record** (`ImportPfx`'s `isStoreKeyImported`, `DbSettingsRunner.PlanImportedCertificate`). `isStoreKeyImported` is true when the
   certificate was added, because the entry then has the import's key. When the entry is kept, it is true only when the entry's key
   provider information names the very container the import stored the client certificate's key in. (A CAPI `.pfx` imported again lands
   in the container the entry names: the fix round's measurement.) `DbSettingsRunner` records the new thumbprint when
   `isPfxImport && isStoreKeyImported`, and the check before the import (`FindInStore`) is gone. So a later save that stops using the
   certificate removes it with the key the tool made. An entry with a usable key of its own is still never recorded or removed. Tests:
   `Apply_AnImportOverAnEntryWithoutAUsableKey_IsRecorded_AndALaterSaveRemovesItWithTheKeyTheToolMade` and
   `OwnerAnswersTests.Plan_RecordsACertificateWhoseEntryTookTheImportsKey_EvenOneTheToolDidNotPutThere`.

The follow-up round's first "Noticed" item (the first certificate with a key taken for the client certificate) is decision 1.

### The merge with `main`

`main` at fa6f501dc, 136 commits, was merged with `--no-ff`. It is stage 2 of the .NET 10 plan: central package management (the
`Directory.Packages.props` and `packages.lock.json` files), MySqlConnector 2.6.2, DB-SEC 3's `OfficeCaServerVerifier`, and Required dlls
trimmed. The one conflict, `docs/decisions/owner-questions.md`, keeps both sides: this branch's DIRECT-CERT follow-up answers, then `main`'s
"Pending items answered on 2026-11-09". The branch now builds with its own `Directory.Packages.props`, and the lane restored from the
offline package folder the repository configures. No package was added and no version changed. The fix round's last "Noticed" item (a
lane build of this branch needs `ImportDirectoryPackagesProps=false`) no longer applies: the branch has its own file, and a stop file at
`.claude/worktrees/Directory.Packages.props` ends NuGet.props' search for worktrees in any case. None of `main`'s DB-SEC 3 tests failed
on the merged branch, so nothing of this branch was changed for them.

### Tests

After the merge: one full lane build (`OpenDental.Local.slnf`, with its restore) with 0 errors, then Targeted runs
(`run-tests.ps1 -Mode Targeted`, results `dcfu3-1-a.txt` and `dcfu3-1-b.txt`):

| Assembly | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| NoHqTests | 57 | 57 | 0 | 0 |
| DbConnectionSecurityTests | 212 | 201 | 0 | 11 |
| LibraryUpdateTests | 126 | 126 | 0 | 0 |
| VendorDirectTests | 922 | 922 | 0 | 0 |
| LibraryBindingTests | 17 | 17 | 0 | 0 |
| ProjectFileTests | 132 | 131 | 0 | 1 |

DbConnectionSecurityTests skips 11:
- the ten DevDbTls server tests, which are inconclusive until the local MariaDB runs with TLS, as before;
- `main`'s `NonLoopbackServer_VerifyFull_X509User_WithACapiKeyAndWithACngKey`, which `main`'s stage 2 full run skips too.

It has 212 tests: `main`'s 195 (its stage 2 full run) plus the 17 tests of the DIRECT-CERT-FOLLOWUP rounds that `main` does not have
yet. Seven of those 17 are this round's: the six named above and the Plan test. ProjectFileTests skips `EveryProjectOfTheSolutionFilter_EvaluatesTheItemsMSBuildRecorded`,
which runs only when its variable asks for the project baseline record (the stage 2 gate's run).

Clean-up: before the first run and after the last, these were listed and compared, with no new name and none missing:
- the key files under `%APPDATA%\Microsoft\Crypto\Keys` and `RSA\<SID>`;
- the stores under `HKCU\Software\Microsoft\SystemCertificates`, so no `OpenPmsDbSecTest_` store is left.

LocalMachine\My and CurrentUser\My hold no `synthetic` certificate (listed, read only). The tests set `StoreNameForTests` to the test
store whenever they go through the tool's public methods, and never use LocalMachine\My.

### Launch checks (administrator only, LocalMachine\My)

Checks 6 to 9 of "Launch checks for the lead" above are this round's. They need an administrator and LocalMachine\My, which the tests
never touch:
- Check 6: the client certificate is chosen by what it is (a CA-first `.pfx`, and a `.pfx` with two client certificates).
- Check 7: an entry whose key is in the administrator's own key set (the warning, the new machine key with Users Read, the record).
- Check 8: the tool removes that certificate with the key it made.
- Check 9: no PIN prompt for a TPM key of another certificate of LocalMachine\My (needs a computer with a TPM).

These are checked by the tests only, through seams: a CAPI delete that fails without a word, and a key provider information that cannot
be read.

### Noticed, not changed (for the lead)

- **`DirectCertificateImport` still opens the keys of its store's certificates to compare containers.** `DeleteKeysNotInUse` does it
  through `GetKeyContainerName` (`OpenPmsDentalBusiness/Email/DirectCertificateImport.cs`), and its usable-entry check has no key-set
  test. Its store is CurrentUser's by default and LocalMachine's when `EmailMessages.CertStoreLocation` says so, so a smart-card or TPM
  certificate in that store could prompt or fail there. Decisions 2 and 3 would port to it as they are (`ReadKeyProvInfo` is
  self-contained), together with VendorDirectTests' import tests. Not part of this round, which covers the tool only.
- **The client certificate's own entry is still opened** when its key is in the import's key set (`IsKeyUsable`, then
  `GrantUsersReadToPrivateKey` for a kept entry). That is the certificate being imported, whose key the `.pfx` carries, so in practice it
  is a software key. A copy of it on a TPM or smart card in the machine key set would be opened and could prompt.
- **Two keys with one name in one key set under different providers** (a software key and a TPM key, say) count as one key.
  `KeyProvInfo.Identity` leaves the provider out on purpose, for the CAPI providers that share containers. The result is a key kept, never
  one deleted, and container names are GUIDs in practice.

(The first item is done in round 4, below: the owner answered on 2026-11-13.)

## Fix round (branch `fix/dcfu3-2`, 2026-11-13)

Cut from `fix/dcfu3-1` (6c26f1191) to fix two review findings against the owner's rule of 2026-11-02 (`docs/specs/db-connection-security.md`
section 4.3): the tool never leaves a key the users can read behind, and its record covers every key it creates; it never touches a key it
did not create. Code f7d54c0e9, tests 2ed4d964b, spec, release notes and launch checks 4cd687c9f, this note the commit after. No schema
change, no enumeration value, no new third-party file, no package or version change.

### 1. A save that fails after the import (`DbSettingsRunner.ApplyOrThrow`)

**What was wrong.** `ImportPfx` gives the store's entry a new key that the users can read. The record was written only after
`SaveFile` and `ResetConfigFileRights`, and either can throw: `SaveFile` when `FreeDentalConfig.xml` is held open by another program or
an antivirus scan, `ResetConfigFileRights` when its rights cannot be set. Then no record named the certificate, and the tool never
removed it or its key. For an OpenSSL `.pfx` a retry did not repair it: the entry now had a usable key that was not the import's, so it
was kept and not recorded. When `ResetConfigFileRights` failed, the settings already used the new thumbprint while the record still named
the old one. The next save then neither removed the old certificate nor recorded the new one. The gap was there before DCFU3.

**The fix.**
- `isStoreKeyImported` and `thumbprintImported` are set only once `ImportPfx` has returned.
- A save that throws before the settings are saved, after such an import, goes to `UndoImportOfFailedSave`:
  - When neither the settings as they still are nor the record name the imported certificate, it is removed with its key
    (`RemoveImported`). The error ends with "The client certificate this save imported was removed from the computer's certificate store
    again, with its private key.", or with `RemoveImported`'s warning when something is left.
  - When the current settings name it, it stays and is recorded at once (as `PlanImportedCertificate` would record it). When the record
    names it already, nothing changes. A record that cannot be written is named in the error, with the `Remove-Item ... -DeleteKey`
    command.
  - An exception that is not a `DbSettingsException` is turned into one (exit code 3, "The connection settings could not be saved: ..."),
    so the undo's text reaches the user. Critical exceptions (`IsCriticalException`) go through untouched, as in the import.
- When only `ResetConfigFileRights` fails, the settings are saved. So `PlanImportedCertificate`, the record and `RemoveImported` of the
  replaced certificate run as after any save, and then the error (exit code 3, the same text as before, plus any warning of those steps)
  is returned. The next save finds the record and the settings in step.

### 2. A usable key the tool did not create (`ClientCertificateStore.KeepCertificateInStore`)

**What was wrong.** A client certificate already in LocalMachine\My with a usable machine key that another route put there (certlm.msc,
`Import-PfxCertificate`, or case 1 above) was kept, and `KeepCertificateInStore` gave BUILTIN\Users read access to that key. Its
`isStoreKeyImported` was false, since the key was in another container than the import's, so it was not recorded either. A later save
removed nothing, and the administrator's key stayed readable by the users for good. The grant came from the DCFU2 fix round, and spec
4.3 still described it.

**The fix.** `ImportPfx` takes the tool's recorded thumbprint (`DbSettingsRunner` passes `ImportedCertificateRecord.Read`). A usable
entry is kept only when its key is the tool's own (`IsEntryKeyTools`):
- the entry names the container this import stored the key in (`isEntryKeyImported`, the former check in `KeepCertificateInStore`); or
- the record names the certificate: the tool imported it before, with the key its entry took then (setup run again with the same file).

Only then does the entry stay as it is, its key get the grant, and every key the import stored get deleted. Any other usable entry is
treated like decision 2's other key set. The certificate is added with the import's key, which gets the grant. `isStoreKeyImported` is
true, so the import is recorded. The entry's earlier key is never opened for the grant, never deleted, and the tool never gives the users
read access to it. The warning (`GetEntryKeyNotToolsText`) says: "The client certificate was already in the computer's certificate store
(LocalMachine\My) with a usable private key that this tool did not import, so the tool gives the program's users no read access to that
key. The import gave the entry the private key it stored, and left the entry's earlier key untouched: provider <name>, key container
<name>, in the computer's key set. An administrator can delete it, once no other certificate uses it: certutil -csp "<provider>" -delkey
"<container>"." The other-key-set warning shares its last part (`GetEntryKeyWhereText`, `GetKeySetText`) and reads as before.

Every grant goes through `GrantUsersRead`, which calls the new seam `ActionInsteadOfUsersReadGrantForTests` in place of
`GrantUsersReadToPrivateKey` when a test sets it, also for the test store (`IsUsersReadGranted`), so a test sees which keys the tool
would make readable in LocalMachine\My. Spec 4.3 now says what "the tool's own" means and that a usable key the tool did not create is
left and never granted.

### Tests

New, in `ClientCertificateImportTests`, for each kind of key (CAPI RSA, CNG RSA, CNG ECDSA), through `DbSettingsRunner.Apply` with the test
store (`StoreNameForTests`):
- `Apply_ASaveThatFailsAfterTheImport_UndoesTheImportOrRecordsIt`:
  1. `FreeDentalConfig.xml` held open with `FileShare.Read`, so `SaveFile`'s `File.Replace` fails with a sharing violation: exit code 3,
     the test store is empty, the import's key no longer opens and its file is gone, nothing is recorded, the settings are unchanged.
  2. The same, when the settings already name the certificate (its entry had no usable key): it stays with the import's key and is
     recorded; a save with the password then removes it with that key.
  3. The rights reset failing after the save (new seam `DbSettingsRunner.ActionBeforeConfigFileRightsResetForTests`): the error, the
     settings and the record name the new certificate, and a save with the password removes it with its key.
- `Apply_AnImportOverAUsableKeyTheToolDidNotCreate_NeverGrantsTheUsersThatKey_AndRecordsTheImport`: an entry with a usable key the test put
  there. The grant seam sees only the import's key, never the entry's earlier one. The certificate is added, the import recorded, the
  warning is the one above, and the earlier key and its file stay. Importing the same `.pfx` again (now recorded) keeps the entry, grants
  the tool's own key only, adds nothing and deletes the second import's key. A save with the password removes the certificate with the
  tool's key and leaves the earlier key.

Both fail on the code before this round: case 1 leaves the certificate and key in the store with no record, and the kept entry was never
added again. The old public `ImportPfx` had no record parameter, so the tests cannot be compiled against it unchanged; that is why this
was checked by reading, not by a run.

Changed:
- `ImportPfx_OfACertificateTheStoreHasWithAUsableKey_KeepsItsEntryAndKey_AndDeletesTheKeyItStored` now passes the record (setup run again
  with a file the tool imported). It also checks that `isStoreKeyImported` is false: the entry keeps its own key.
- `Apply_AnImportOverAnEntryWithoutAUsableKey_IsRecorded_AndALaterSaveRemovesItWithTheKeyTheToolMade` keeps its unusable-key case only. Its
  usable-key case ("left as it is and not recorded") was the behaviour this round removes; the new test above covers it.

One full lane build (`OpenDental.Local.slnf`) with 0 errors, then Targeted runs (results `dcfu3-2-b.txt` and `dcfu3-2-c.txt`):

| Assembly | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| NoHqTests | 57 | 57 | 0 | 0 |
| DbConnectionSecurityTests | 214 | 203 | 0 | 11 |
| LibraryUpdateTests | 126 | 126 | 0 | 0 |
| VendorDirectTests | 922 | 922 | 0 | 0 |
| LibraryBindingTests | 17 | 17 | 0 | 0 |
| ProjectFileTests | 132 | 131 | 0 | 1 |

The skips are the last round's: the ten DevDbTls server tests, `NonLoopbackServer_VerifyFull_X509User_WithACapiKeyAndWithACngKey`, and
ProjectFileTests' baseline test. DbConnectionSecurityTests has the two new tests more. Clean-up was checked as in the last round. The key
files under `%APPDATA%\Microsoft\Crypto\Keys` and `RSA\<SID>`, and the stores under `HKCU\Software\Microsoft\SystemCertificates`, were
listed before the first run and after the last: no name new, none missing. LocalMachine\My and CurrentUser\My hold no `synthetic`
certificate (read only).

### Launch checks (administrator only, LocalMachine\My)

Checks 10 and 11 of "Launch checks for the lead" are this round's: a certificate an administrator put into LocalMachine\My with a machine
key (`Import-PfxCertificate`), and an import while `FreeDentalConfig.xml` is held open. The put-back step is now 12. A rights reset that
fails is checked by the tests only.

### Noticed, not changed (for the lead)

- **Undoing or later removing an import removes the whole store entry.** That includes a certificate that was in the store before and took
  the import's key: one without a usable key, with its key in another key set, or (now) with a usable key the tool did not create.
  `RemoveImported` deletes the certificate and the import's key. The entry's earlier key is left, as the import's warning said: after a
  successful save, in the result; after a save that fails, in the error, which gives that warning before saying that the certificate that
  was already in the store was removed (the second fix round below). Decision 6 does the same on a later save. Putting back the entry's earlier key provider information instead would mean the tool writes to a
  certificate it did not import.
- **The record names a thumbprint, not a key container.** If an administrator gives a recorded certificate's entry another key by hand
  (certlm.msc's import of the same certificate with another key), the tool takes that key for its own: on a re-import it keeps it and
  grants the users read access to it, and a later save deletes it. Recording the container name next to the thumbprint, and checking
  it in `IsEntryKeyTools`, would close this. It changes the record file's format, so it is left to the lead.

(The first two items went to the owner, who answered on 2026-11-13; both are done in round 4, below.)
- **The office CA copy is not undone** when the save fails after `OfficeCaFile.WriteCopy`, as before. It is a public certificate in an
  administrator-only folder, and the next save writes it again.
- **A failed rights reset still ends with exit code 3**, although the settings were saved and the record is now in step. Setup treats that
  as the tool failing (exit code 10), which is right: the file may still give Users rights.

## Second fix round (branch `fix/dcfu3-3`, 2026-11-13)

Cut from `fix/dcfu3-2` (af09803f9) for two more review findings against the owner's rule of 2026-11-02. Code 29f61a2ce, tests c59e5dc18,
spec, release notes and launch checks b0331c85c, this note the commit after. No schema change, no enumeration value stored anywhere, no new
third-party file, no package or version change.

### 1. Two providers with one container name (`ClientCertificateStore.IsEntryKeyTools`)

**What was wrong.** `IsEntryKeyTools` decided that the store entry's key was the one this import stored (`isEntryKeyImported`) by
`KeyProvInfo.Identity`, which is the key set and the container name only. It leaves the provider out on purpose, for
`DeleteKeysNotInUse`, where a false match only keeps a key. Here a false match makes the tool claim a key it did not create. An
administrator can put the client certificate into LocalMachine\My with a key of another provider under the container name the `.pfx`
records (`certutil -csp "Microsoft Software Key Storage Provider" -importpfx` of a CAPI `.pfx`, say). The tool's CAPI import then finds no
CAPI container of that name and stores its key there (the measurement of the DCFU2 fix round). The entry was kept and the CNG key granted
to the users. The import's CAPI key stayed with no certificate naming it, because `DeleteKeysNotInStore` saw a certificate "using" that
Identity. The thumbprint was recorded, and a later save deleted the administrator's CNG key through `RemoveImported`. The new test
confirmed every step of this on the code before the fix: the import stored its key in the CAPI container of the CNG key's name.

**The fix.** The ownership claim goes through `IsSameKeyStrict`: both key provider informations named and readable, the same Identity,
the same provider type and the same provider name (ignoring case). A CNG key (type 0) never matches a CAPI one. A false negative is safe:
the entry goes down the add path and takes the import's key, which only the import made, and the warning names the earlier key. Identity
itself is unchanged, so `DeleteKeysNotInUse` still keeps a key on a match that ignores the provider. An entry that would name the
import's CAPI container through another Microsoft software CSP (Enhanced against Enhanced RSA and AES) would now count as not the tool's:
the entry takes the import's key, and the warning names the other one. That is the safe side.

### 2. The import's warning on a failed save (`DbSettingsRunner.ApplyOrThrow`, `UndoImportOfFailedSave`)

**What was wrong.** The catch for a save that fails after the import built its error from the exception and `UndoImportOfFailedSave`
only. The `warning` that `ImportPfx` returned was lost. That warning holds the store entry's earlier key with its `certutil -delkey`
command, a key of the file that could not be deleted, and a failed grant. Over a certificate an administrator had put into LocalMachine\My
with a working key (launch check 10), the entry took the import's key. The failed save then removed the whole entry, and the error said
only that "the client certificate this save imported" was removed. The administrator's key was left with no certificate naming it, and
nothing said that the entry was there before or where its key is.

**The fix.**
- The error gives the import's warning between the save's failure and the undo's text.
- `ImportPfx` returns `ClientCertificateEntryBefore` (an out value, never stored): `None` (the store did not hold the certificate),
  `InStore` (it did, and no earlier key was left aside), or `InStoreKeyLeft` (the entry took the import's key and its earlier key was
  left; the warning says where).
- When the undo removes the certificate, its text depends on that value:
  - `InStoreKeyLeft`: "The client certificate was already in the computer's certificate store before this save; it was removed from the
    store, with the private key this save gave it, and its earlier key was left untouched, where said above."
  - `InStore`: "... before this save, without a private key it could use; it was removed from the store, with the private key this save
    gave it."
  - `None`: the earlier sentence ("The client certificate this save imported was removed ...").
- Whether the undo should instead put back the entry's earlier key provider information is owner question 2 of this review, separate from
  this fix (see "Noticed" of the fix round above: the tool would then write to a certificate it did not import).

### Tests

New, in `ClientCertificateImportTests`:
- `Apply_AnEntryWithACngKeyNamedAsTheImportsCapiContainer_IsNotTakenForTheToolsKey`, through `DbSettingsRunner.Apply` with the test store.
  `ClientCertificateTestStore.AddCngEntryAndWriteCapiPfxOfItsName` adds a certificate whose entry has a CNG Software KSP key named N in the
  user key set. It then writes a CAPI `.pfx` of the same certificate and key that records container N. That CAPI container is made only to
  write the file, after checking that none had the name, and deleted again before the import. The premise is checked: the import stores
  its key in CAPI N. The grant seam sees one key, the import's CAPI key, never the CNG one. The certificate is added with the import's key
  and recorded, and the warning names the CNG key with `certutil -user -csp "Microsoft Software Key Storage Provider" -delkey`. A later
  save with the password removes the certificate with the import's key, and the CNG key and its file stay.
- `Apply_ASaveThatFailsAfterTheImport_UndoesTheImportOrRecordsIt` case 4, for each kind of key: an entry with a usable key the tool did
  not create (`AddCertificate(pathPfx)`), and `FreeDentalConfig.xml` held open. The error ends with the `GetEntryKeyNotToolsText`
  sentence (with the `certutil -user -csp ... -delkey ...` command) followed by the `InStoreKeyLeft` sentence, and does not say "this save
  imported". The store is empty, the earlier key and its file stay, the import's key and its file are gone, nothing is recorded, and the
  settings are unchanged.

Both were run against the code before the fix (its two files checked out from af09803f9, built and run, then restored; results
`dcfu3-3-before-fix.txt`). Both fail there: the first because the import kept the entry with no warning, the second because the error
lacked the warning.

One full lane build (`OpenDental.Local.slnf`) with 0 errors, then one Targeted run (results `dcfu3-3-b.txt`):

| Assembly | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| NoHqTests | 57 | 57 | 0 | 0 |
| DbConnectionSecurityTests | 215 | 204 | 0 | 11 |
| LibraryUpdateTests | 126 | 126 | 0 | 0 |
| VendorDirectTests | 922 | 922 | 0 | 0 |
| LibraryBindingTests | 17 | 17 | 0 | 0 |
| ProjectFileTests | 132 | 131 | 0 | 1 |

The skips are the earlier rounds'. After each run nothing was left behind: no `OpenPmsDbSecTest_` store under
`HKCU\Software\Microsoft\SystemCertificates`, no `synthetic` certificate in CurrentUser\My or LocalMachine\My (read only), and no key file
under `%APPDATA%\Microsoft\Crypto\Keys` or `RSA\<SID>` written since the run started.

### Launch checks (administrator only, LocalMachine\My)

Check 11 of "Launch checks for the lead" has a second part: the failed save over a certificate an administrator put there with
`Import-PfxCertificate`. Check 12 is new: two providers with one container name (`certutil -csp "Microsoft Software Key Storage Provider"
-importpfx` of `client-capi.pfx`). The put-back step is now 13.

### Noticed, not changed (for the lead)

- **Whether `certutil -importpfx -csp <KSP>` names its key after the container a CAPI `.pfx` records** has not been measured here. That
  needs LocalMachine\My or a KSP import of the current user, which the tests do not do. The tests reach the same pair of keys from the
  other side: Windows' CAPI import takes the recorded name when no CAPI container has it. Check 12 says what to do if certutil picks
  another name.
- **The failed save's error can be long.** It now holds the save's failure, the import's warning (which can name several keys) and the
  undo's text. It is accurate, but the lead may want it laid out in lines where the settings window shows it.

## Round 4 (branch `fix/dcfu4-1`, 2026-11-14)

Cut from `main` (f15956ad5). The owner answered the last round's three questions on 2026-11-13, "all three as recommended"
(`docs/decisions/owner-questions.md`, "The connection settings tool's client certificate (DIRECT-CERT-FOLLOWUP, last round, answered on 2026-11-13)"):

1. **The Direct certificate window's import gets the tool's two key changes**: keys compared by their key provider information, never by
   opening a key; and a store entry is "already there" only when its key is in the key set the import uses. Part A below.
2. **Undoing an import puts the store entry's earlier key back (answer (b))**, so the store is as the administrator had it. Part B below.
3. **The tool's record also holds the key's container and provider.** Part C below.

The owner's standing rule (2026-11-02, spec 4.3) still frames every part: the tools give BUILTIN\Users read access to a private key they
import, so they never leave a Users-readable key behind and never touch a key they did not create.

Commits, in order:

| Commit | What |
|---|---|
| 635e0d28b | A (1): the key provider information reader and the strict comparison move to CodeBase |
| 355e84b4f | A (2): the Direct import's key-open seam and part A's tests, which fail before the change |
| 5db5fe919 | A (3): the Direct import's change |
| baf9f8ece | C (1): part C's tests, which fail before the change |
| c18579d51 | C (2): the record names the key the tool created; a re-keyed entry is not the tool's |
| 8fbfef0db | B (1): part B's test, which fails before the change |
| 71995a9a9 | B (2): the put-back |
| 81a5ccfa2 | docs (1): spec 4.3, release notes, code comments |

This note, the launch checks and the owner-questions section are the commit after them. C was built before B because B's earlier key
provider information lives in C's record between saves. No schema change, no enumeration value stored anywhere, no new third-party file,
no package or version change, no download.

### One implementation of the key provider information (CodeBase)

`CodeBase/Utilities/CertificateKeyProvInfo.cs` (SDK-style project: no project-file edit) holds what `ClientCertificateStore` had as
`KeyProvInfo`, `ReadKeyProvInfo`, `IsSameKeyStrict` and the `crypt32.dll` P/Invoke, now public for both tools:

- `CertificateKeyProvInfo.Read(certificate, actionBeforeReadForTests)` reads `CERT_KEY_PROV_INFO_PROP_ID` with
  `CertGetCertificateContextProperty` on `X509Certificate2.Handle`, never opening the key. It keeps every field (container, provider,
  type, the whole `dwFlags`, `dwKeySpec`, and the provider parameters, rarely any), and `IsFieldsRead` says whether they were read, also
  for a property that names no container (then `Problem` is set, as before). The seam is a parameter, so each tool keeps its own
  (`ClientCertificateStore.ActionBeforeKeyProvInfoReadForTests`, `DirectCertificateImport.ActionBeforeKeyProvInfoReadForTests`).
- `Identity` (key set and container, no provider: the comparison before a delete, where a false match only keeps a key) and
  `IsSameKeyStrict` (also the provider type and name: every claim that a key is a tool's own) are as they were.
- New: `Write(certificate)` sets the fields back with `CertSetCertificateContextProperty` (part B); `IsSameExactly` compares every field;
  `Describe()` gives "provider P, key container C, in the computer's key set" (or "in a user key set") for the messages.

`ClientCertificateStore.ReadKeyProvInfo` is a one-line wrapper that passes the tool's seam; its own struct, constants and P/Invoke are gone.

### A. The Direct certificate import

`OpenPmsDentalBusiness/Email/DirectCertificateImport.cs`:

- **Keys are compared by key provider information.** `DeleteKeysNotInUse` reads every certificate's key provider information before the
  first delete (`ReadKeyProvInfo`, the CodeBase reader after the seam) and compares `Identity`, as the tool does. `GetKeyContainerName`,
  `GetCapiKeyContainerName` and `GetCngKeyContainerName` are gone: they opened each key of the store (`PrivateKey`, `GetRSAPrivateKey`,
  `GetECDsaPrivateKey`), and the store can be the Local Computer's, where a TPM or smart-card key could ask for a PIN or fail. A
  certificate whose key provider information cannot be read uses an unknown key: no key is deleted then, and the warning (or, for an
  import that stops, the "Not removed from this computer again" list) says "A private key this import stored could not be deleted
  (subject, thumbprint): It was kept, because the private key of a certificate that stays could not be identified, and it may be this
  one: <subject> (<thumbprint>), <why>"; one of the import's own keys that cannot be identified: "It was kept, because its key container
  could not be identified, so a certificate of the store may use it: <why>". A certificate whose information names no key had no key
  stored for it (before, its delete was tried and could fail).
- **"Already in the store" checks the key set first.** `CheckPrivateAgainstStore` (the only such check; `PrivateImportRun.Add` only notes
  the entry an add replaces) counts a store entry of the file's certificate as there with its key only through `IsEntryKeyUsable`: its key
  provider information names a key, readable, in the key set the import uses (machine for `EmailMessages.CertStoreLocation` LocalMachine,
  user for CurrentUser), and only then is the key opened to sign or decrypt (`IsKeyUsable`). An entry in another key set is imported
  again: the store's add gives the entry the import's key, and the earlier key is left untouched (see Noticed).
- **Seams**: `ActionBeforePrivateKeyOpenForTests`, called before every open (`IsKeyUsable`, `TryDeletePrivateKey`,
  `GetCapiRsaPrivateKey`, `GetMachinePrivateKeyFilePath`), and `ActionBeforeKeyProvInfoReadForTests`.
- **The display path is kept.** `DirectCertificateDetails.Create` calls `IsKeyUsable`, which opens the key (`GetRSAPrivateKey` or
  `GetECDsaPrivateKey`, then signs a digest or decrypts a test value). It compares no containers. It is called for: the file's
  certificates (read with `EphemeralKeySet`, so in memory: no device); the store's certificates for this address or its domain, for the
  window's list (`GetPrivateCertificates`) and the check's replaced and kept lists (`CheckPrivateAgainstStore`); never another address's.
  That is acceptable: these are the address's own Direct certificates, which the window exists to show with "private key usable or not",
  and which the program itself opens to sign and decrypt every Direct message of the address, so a key that would prompt there prompts at
  every send anyway. A Direct provider issues a `.pfx`, so these keys are software keys in practice. If an office ever keeps a Direct
  certificate on a smart card, the window's list would prompt for it (Noticed).

### C. The tool's record names the key it created

`ImportedCertificateRecord` (`ConnectionSettings\imported-client-certificate.txt`, administrator-only, protected as before) now reads and
writes an `ImportedCertificate`: `Thumbprint`, `KeyTool` (the key the tool's import gave the store's entry), `KeyBefore` (part B) and
`IsOldFormat`. `Read(path)` still gives the thumbprint, `ReadRecord(path)` the record; `Write(path, ImportedCertificate)` writes it,
`Write(path, thumbprint)` writes one with no key named.

**The format** (documented in the class summary), UTF-8 without a byte order mark, CR LF, one item per line, versioned by its first line:

```
Open PMS - Dental imported client certificate, format 2
thumbprint=<40 hex digits>
tool.container=<text>
tool.provider=<text>
tool.providertype=<decimal; 0 for a CNG key storage provider>
tool.keyset=machine|user
tool.flags=<8 hex digits: dwFlags, whose 0x20 bit is the key set>
tool.keyspec=<decimal>
earlier.container=<text>
earlier.provider=<text>
earlier.providertype=<decimal>
earlier.keyset=machine|user
earlier.flags=<8 hex digits>
earlier.keyspec=<decimal>
earlier.param=<decimal>,<8 hex digits>,<hex bytes>      (zero or more)
```

The `tool.` group and the `earlier.` group are each all six lines or none. Text values escape `%`, CR and LF as `%25`, `%0D`, `%0A`. Read
defensively: a line of another name, a name twice, a group with a line missing, a key set that contradicts the flags, a tool key without a
container, a provider parameter without its key, a bad number or escape, or another first line (a later format included) reads as none,
and with none the tool removes nothing, as before.

**The old format** (a thumbprint line only, every record before round 4) is still read: one line of 40 hex digits, spaces, colons or tabs
allowed between them (stricter than before, which took the hex digits of any text). It is a thumbprint with no key named. How such a record
decides whether a store key is the tool's: as before round 4, by the thumbprint alone (`IsEntryKeyTools` and `RemoveImported`). That never
lets the tool claim a key the old rule did not already claim, and it is the safe side: the record only ever named a certificate whose entry
took the key the tool's import stored, so its key then was the tool's, Users-readable; claiming nothing would leave that key on the computer
for good, against the owner's rule. The next save that writes the record (every save does) rewrites it in format 2: with this save's
import, from the import; without one, through `ClientCertificateStore.CompleteRecord`, which names the key the entry has at that moment, read
without opening it. From then on only that key is the tool's. Database connection security is new in the next version, so old-format
records exist only on test installs.

**The claim.** With a key named, `IsEntryKeyTools` claims the entry's key only when the recorded key matches it by `IsSameKeyStrict` (or the
entry names the key this import stored, as before). A recorded certificate whose entry someone re-keyed by hand is then not the tool's: no
Users grant, the entry takes the import's key, and the warning names the hand-given key: "The tool's record names the client certificate,
but its entry in the computer's certificate store (LocalMachine\My) names another private key than the one the tool gave it (provider ...,
key container ..., in ...): someone gave the entry that key by hand, so it is not the tool's, and the tool gives the program's users no
read access to it. The import gave the entry the private key it stored, and left the entry's earlier key untouched: <where, certutil>."
`RemoveImported` leaves such an entry, with its key: "The client certificate <thumbprint> that this tool imported before stays in the
computer's certificate store (...): its entry names another private key (...), not the one the tool gave it (...), so someone changed it
by hand, and the tool leaves it as it is. If no settings use it, remove it with its private key as an administrator, in PowerShell:
Remove-Item Cert:\...\<thumbprint> -DeleteKey."

**The key the tool made, once no entry names it**, is the tool's own and Users-readable, so it goes (`DeleteToolKeyNamed`): by its name in
its key set (`TryDeleteKeyNamed`: CAPI through `CspParameters` with `UseExistingKey | NoPrompt`, then `DeleteCapiKey`, which checks that
it is gone; CNG through `CngKey` with `Silent`, checked the same way), only when no certificate of the store names it (`IsKeyNamedInStore`,
read anew, no key opened) and it is still there. That happens after a re-import over a re-keyed (or no longer usable) recorded entry, when
`RemoveImported` leaves a re-keyed entry, after a put-back (part B), and when the recorded certificate is no longer in the store at all
(removed by hand; before, nothing happened). `IsKeyNamedInStore` counts a CAPI key and a CNG key of one name as two keys (CAPI providers
share containers, but CAPI and CNG keys live apart), unlike `DeleteKeysNotInUse`, which keeps a key on any `Identity` match: otherwise the
tool's CAPI key of the second fix round's two-providers case would stay once the entry names the CNG key again. A certificate whose key
provider information cannot be read keeps the key, and the warning says why and where it is.

`ImportPfx` now takes the record (`ImportedCertificate`, null for none) and returns what to record (`out ImportedCertificate`): the kept
entry's key, or the import's key after an add; `KeyBefore` from the entry before the import (part B), or the record's own when the tool's
recorded entry without a usable key takes the import's key. `DbSettingsRunner` reads the record with `ReadRecord`, writes the import's
record, or the record as it was (`CompleteRecord`), and passes the record to `RemoveImported` and to `UndoImportOfFailedSave`, which also
rewrites a record that named the certificate already (the entry's key is the import's now). The internal `ImportPfx` and `RemoveImported`
overloads that take a thumbprint make a record with no key named, so the earlier tests keep their meaning.

### B. Undoing an import puts the entry's earlier key back

**Where the earlier key comes from.** `AddClientCertificate` reads the store entry's key provider information before anything else
(`keyProvInfoBefore`). When the entry takes the import's key while leaving its earlier key aside (`ClientCertificateEntryBefore.InStoreKeyLeft`:
a key in another key set, an unidentified key, or a usable key the tool did not create, including one given by hand to a recorded
certificate), the returned record's `KeyBefore` is exactly those fields, when they could be read (`IsFieldsRead`). It goes into the record
(`earlier.` lines), so it survives between saves. An entry that had no key provider information (`InStore` with no key) has no `KeyBefore`:
the whole entry it took is the tool's to remove, as built. (The fix round below, `fix/dcfu4-2`, adds a key in the import's key set that
does not open and that the tool did not create to the keys left aside, and changes the failed save's error and the texts about the tool's
key.)

**The put-back** (`ClientCertificateStore.PutBackEarlierKey`, from `RemoveImported` when the entry is the tool's and the record has a
`KeyBefore`; so both for the undo of a failed save, `UndoImportOfFailedSave`, and for a later save that stops using the certificate):

1. `KeyBefore.Write` on the store's own certificate context (the store opened for writing, so the store keeps it).
2. The entry is read anew (`FindInStoreNow`) and must name the earlier key (`IsPutBack`: the very same fields, or, should Windows keep the
   flags otherwise, the same container, provider and type, key set and key spec).
3. Only then is the tool's key deleted (`DeleteToolKeyNamed`, the key the entry named until then). The entry stays in the store; the earlier
   key is never opened, altered or deleted.

The message: "The client certificate <thumbprint> was in the computer's certificate store (<store>) before the tool's import gave it a
private key: its entry names its earlier private key again (provider P, key container C, in <key set>), as before that import, and the
private key the tool gave it was deleted." After a save that fails it follows the import's warning in the error; after a later save, the
result.

**When the put-back fails** (Windows refuses the write, or the entry does not name the earlier key afterwards): while the entry still names
the tool's key (`IsSameKeyStrict`), that key stays, so that the certificate keeps working, and the text says so: "... but its earlier
private key (...) could not be named on its entry again: <why> The entry still names the private key the tool gave it (...), which the
computer's users can read, so the tool left both keys and the entry as they are. To put the entry back as it was, as an administrator,
delete the tool's key (certutil [-user] -csp "P" -delkey "C"), then import the certificate with its earlier key again." When the entry no
longer names it, the tool's key is deleted, and the text says what the entry names now and how to check it (`certutil -store My
<thumbprint>`). A test seam stands in for the write (`ActionInsteadOfKeyProvInfoPutBackForTests`).

`UndoImportOfFailedSave`'s own sentence for an entry whose earlier key was left aside now applies only when what the entry named could not
be read at all (no `KeyBefore`): "... what its entry named before could not be read, so it could not be put back: it was removed from the
store, with the private key this save gave it, and its earlier key was left untouched, where said above."

### Tests

New, all through seams and test stores of the current user, with throwaway keys of the current user only:

- `tests/LibraryUpdateTests/DirectCertificateKeyProvInfoTests.cs` (part A), with its own key provider information reader and writer
  (`DirectTestKeyProvInfo`) and key finder (`DirectTestKey`), apart from the code under test:
  - `ImportPrivateFile_OpensNoKeyOfAnotherCertificateOfTheStoreToCompareKeys`: another address's certificate in the test's private store
    whose key provider information names "Microsoft Base Smart Card Crypto Provider" and a container no CSP has; an import of a file with
    an unused key (another address's certificate with its key). No key of that certificate is opened (the seam counts, and refuses the open,
    so a smart-card CSP is never asked for a card), the unused key is deleted, the imported key stays, the smart-card certificate is as it
    was.
  - `CheckAndImportPrivateFile_AnEntryWhoseKeyIsInAnotherKeySet_IsNotUsable_WithoutOpeningIt`: the address's entry made to name the machine
    key set (only its flag is changed; nothing is made, opened or deleted in the machine key set); Check offers the import and Import gives
    the entry a working key of its own, without opening the machine-set key, and the user key the entry named before stays.
  - `ImportPrivateFile_KeepsEveryUnusedKeyWhenAKeyOfTheStoreCannotBeIdentified`: the smart-card certificate's key provider information made
    unreadable (seam): the unused key is kept and the warning says why.
- `tests/DbConnectionSecurityTests/ClientCertificateRecordTests.cs` (part C):
  - `Record_OfFormat2AndOfTheOldFormat_IsRead_AndAMalformedOne_ReadsAsNone`: six records read, fifteen malformed ones read as none.
  - `Record_WrittenInFormat2_ReadsBackFieldForField`: both keys, a provider parameter, names with `%`, CR and LF; the old format reads with
    no key named.
  - `Apply_ARecordOfTheOldFormat_IsRead_AndRewrittenInFormat2ByTheNextSave`: for each kind of key, the record an import wrote replaced by its
    thumbprint line; a save that keeps the certificate rewrites it in format 2 naming the entry's key (the tool's); a save with the password
    removes the certificate with that key.
  - `Apply_ARecordedCertificateThatWasReKeyedByHand_IsNotTheTools`: for each kind of key, the tool's import, then the same `.pfx` added to
    the test store again (the entry names a key the tool did not make); (1) a re-import with the tool never grants that key, the entry takes
    the new import's key, the warning names the hand-given key, which stays with its file, and the tool's first key is deleted; (2) a save
    with the password leaves the entry with the hand-given key, says so, and deletes only the tool's key.
- `tests/DbConnectionSecurityTests/ClientCertificatePutBackTests.cs` (part B):
  - `Apply_UndoingAnImportOverAnEntryWithAnEarlierKey_PutsThatKeyBack`: for each kind of key, an entry with its earlier key in another key
    set (`NameMachineKeySetForEntry`) and one with a usable key the tool did not create, each with a failed save (`FreeDentalConfig.xml` held
    open) and with a later save with the password: the entry stays and names its earlier key again, field for field (`TestKeyProvInfo.IsSameAs`,
    new), the earlier key and its file are untouched, the tool's key and its file are gone, no record is left, and the message ends with the
    put-back sentence.
  - `Apply_APutBackThatFails_LeavesTheToolsKeyWhileTheEntryStillNamesIt`: for each kind of key, the put-back refused (the seam throws) or
    dropped (it does nothing): the entry, the tool's key (with its file) and the earlier key all stay, the certificate still signs, and the
    message names both keys and the certutil command.

Changed, because part B changed what they expected (the whole entry removed): `Apply_ASaveThatFailsAfterTheImport_UndoesTheImportOrRecordsIt`
case 4, `Apply_AnImportOverAUsableKeyTheToolDidNotCreate_NeverGrantsTheUsersThatKey_AndRecordsTheImport` step 3, and
`Apply_AnEntryWithACngKeyNamedAsTheImportsCapiContainer_IsNotTakenForTheToolsKey`'s later save now expect the entry to stay with its earlier
key and the tool's key to be gone. `DirectCertificateImportTests.KeyChecks_NeverThrowButLetTheCriticalExceptionsThrough` and the code-text
checks follow the removal of `GetKeyContainerName` (it checks `IsEntryKeyUsable` instead, and that the business layer compares no keys by
opening them).

**Against the code before each change** (as the last round did; result files under `.git/od-tools/test-results/`):

| Part | Run | Code | Result |
|---|---|---|---|
| A | `dcfu4-1-a-before.txt` | 355e84b4f (the seam only) | both A tests fail: one key opened each (the smart-card certificate's; the machine-set entry's) |
| C | `dcfu4-1-c-before.txt` | 5db5fe919's tool files under baf9f8ece's tests | all three fail: the format-2 record reads as none; the save writes the thumbprint line again; the re-import grants the hand-given key |
| B | `dcfu4-1-b-before.txt` | c18579d51 | the put-back test fails: the whole entry is removed |

The third A test, the C round-trip test and the put-back failure test use members that did not exist before, so they were not run there.

**After the change:** one full lane build (`odbuild.ps1`, the whole solution) with 0 errors, then Targeted runs (`dcfu4-1-final-a.txt`,
`dcfu4-1-final-b.txt`):

| Assembly | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| NoHqTests | 57 | 57 | 0 | 0 |
| DbConnectionSecurityTests | 221 | 210 | 0 | 11 |
| LibraryUpdateTests | 129 | 129 | 0 | 0 |
| VendorDirectTests | 922 | 922 | 0 | 0 |
| LibraryBindingTests | 17 | 17 | 0 | 0 |
| ProjectFileTests | 132 | 131 | 0 | 1 |

The skips are the earlier rounds': the ten DevDbTls server tests and `NonLoopbackServer_VerifyFull_X509User_WithACapiKeyAndWithACngKey`, and
ProjectFileTests' baseline test. DbConnectionSecurityTests has six tests more than after the second fix round (215), LibraryUpdateTests
three. Clean-up: before the first run and after every run, the key files under `%APPDATA%\Microsoft\Crypto\Keys` and `RSA\<SID>`, the stores
under `HKCU\Software\Microsoft\SystemCertificates`, and the `synthetic` or `example.test` certificates of CurrentUser\My and LocalMachine\My
(read only) were listed and compared: no name new, none missing; the test classes' own check (`AssertCleanedUp`, `DeleteKeys`) found
nothing. No test opens LocalMachine\My or a real key; the smart-card and machine-set key provider informations are only names written on
certificates of the test stores.

### Launch checks (administrator only)

Checks 7, 8, 10, 11 (its second part) and 12 of "Launch checks for the lead" now expect the put-back (part B); check 13 says to do these
first. On the same test computer, as a Windows administrator, with the `.pfx` files and passwords of the checks above:

14. **A recorded certificate given another key by hand** (part C). With the settings on the password, import `client-cng.pfx` with the tool
    and note its "Unique container name" (`certutil -store My <thumbprint>`). Give its entry another key by hand:
    `Import-PfxCertificate -FilePath .\client-cng.pfx -CertStoreLocation Cert:\LocalMachine\My -Password $password`; `certutil -store My
    <thumbprint>` now names another container (note it), whose file in `%ProgramData%\Microsoft\Crypto\Keys` has no Users. (a) Import
    `client-cng.pfx` with the tool again: the result begins "The tool's record names the client certificate, but its entry in the
    computer's certificate store (LocalMachine\My) names another private key than the one the tool gave it", and names the hand-given key
    with the sentence that asks that it not be deleted while the record names it (no `certutil -delkey` command, DCFU4 second fix round);
    the hand-given key's file still has no Users and is still there; the first noted file is gone; the entry names a third container whose file gives Users Read;
    `imported-client-certificate.txt` has `earlier.container=` the hand-given container. Save with the password: the entry names the
    hand-given key again, the result ends with the sentence about a key deleted since (check 8), the hand-given key's file is still there,
    and the third file is gone. (b) Remove the certificate (`Remove-Item ... -DeleteKey`), then repeat up to the key
    given by hand, and save with the password: the result says the certificate "stays in the computer's certificate store" with the
    hand-given key; LocalMachine\My still holds it, that key's file is still there without Users, the tool's key file is gone, and
    `imported-client-certificate.txt` is absent. Remove it with `Remove-Item Cert:\LocalMachine\My\<thumbprint> -DeleteKey`.
15. **A record of the old format** (part C). With `client-cng.pfx` imported by the tool, replace the record's text with its thumbprint line
    only (`Set-Content -Encoding ascii "<program folder>\ConnectionSettings\imported-client-certificate.txt" <thumbprint>`; the file keeps
    its owner and rights). Open the tool and save the settings unchanged: the file starts with `Open PMS - Dental imported client
    certificate, format 2` and its `tool.container=` is the entry's "Unique container name"'s container (`certutil -store My <thumbprint>`,
    "Key Container"). Save with the password: the certificate and its key file are gone.
16. **The Direct import beside a TPM certificate** (part A), on a test computer with a TPM, the program started as administrator on a test
    database: put a certificate with a TPM key for another address into the Local Computer's Direct store:
    `$c=New-SelfSignedCertificate -Subject "CN=synthetic-direct-tpm" -TextExtension @("2.5.29.17={text}email=tpm@synthetic.example.test")
    -Provider "Microsoft Platform Crypto Provider" -CertStoreLocation Cert:\LocalMachine\My`, then `Move-Item ("Cert:\LocalMachine\My\"+$c.Thumbprint)
    Cert:\LocalMachine\NHINDPrivate`. In Edit Email Address, Certificate, import a test Direct `.pfx` that holds another certificate with its
    key (its CA's, or another address's): no PIN prompt, no error, the TPM certificate stays. Remove it with `Remove-Item
    Cert:\LocalMachine\NHINDPrivate\<thumbprint> -DeleteKey`.
17. **A Direct entry whose key is in another key set** (part A). Put the test Direct `.pfx`'s certificate into `LocalMachine\NHINDPrivate`
    with its key in your own user key set (check 7's snippet with `"NHINDPrivate"` in place of `"My"`), and note its "Key Container"
    (`certutil -store NHINDPrivate <thumbprint>`). In the certificate window, Check offers the import (not "already in this computer's
    Direct certificate store"), and Import gives the entry a key of the machine key set (`certutil -store NHINDPrivate <thumbprint>` names
    another container, whose file in `%ProgramData%\Microsoft\Crypto` gives Users Read). Delete your own earlier key with
    `certutil -user -csp "<provider>" -delkey "<container>"`, and remove the certificate with `Remove-Item ... -DeleteKey`.

Checked by the tests only: a key provider information that cannot be read (both tools), a put-back that Windows refuses or drops, and
which keys are opened (the seams).

### For the .NET 10 switch package

`CertificateKeyProvInfo` is plain `crypt32.dll` interop on `X509Certificate2.Handle`, which .NET keeps; `Marshal.PtrToStructure(IntPtr,
Type)` and `Marshal.SizeOf(Type)` work there too (the generic forms are the newer style). `TryDeleteKeyNamed` and `TryGetKeyFilePath` use
`CspParameters`, `RSACryptoServiceProvider` and `CngKey`, Windows-only on .NET as everything here. Check `ClientCertificatePutBackTests` and
`ClientCertificateRecordTests` there with the other key tests.

### Noticed, not changed (for the lead)

- **Fixed in the lead fix round below (decision 4).** **A Direct entry whose key is in another key set** is imported again: the store's add gives the entry the import's key (with
  `FRIENDLY_NAME_IMPORTED`), and the earlier key stays where it was, untouched, but unlike the tool the window says nothing about it. A
  failed import's `PrivateImportRun.Undo` already puts the store's earlier copy back; a later replacement deletes only the import's key.
  A warning naming the earlier key, as the tool's, would be a small addition.
- **The certificate window's list opens the address's certificates' keys** (the display path, part A above): a Direct certificate kept on
  a smart card for the address would prompt there.
- **Fixed in the fix round below.** **An entry with a named key that does not open, which the tool did not record** (`InStore`, not `InStoreKeyLeft`) takes the import's key
  and is still removed whole by the undo or a later removal: the answer was about an earlier key left aside. Putting such a dead key name
  back would also leave the entry as it was; left to the lead.
- **Fixed in the lead fix round below (decision 2, owner question 1 of the round).** **A put-back that fails while the entry still names the tool's key** leaves that Users-readable key, and the save then deletes the record
  (the settings no longer use the certificate), so the tool no longer knows the key; the message gives the command that deletes it. Keeping
  the record in that case would let a later save try again.
- **Fixed in the fix round below.** **The failed save's error** gives the import's warning, with the `certutil -delkey` command for the earlier key, before saying that the
  earlier key was put back; the command's condition ("once no other certificate uses it") no longer holds then. Dropping the command in that
  case would read better.
- **Fixed in the fix round below.** **A name-based delete that fails** (`TryDeleteKeyNamed`) has no seam (`ActionBeforeKeyDeleteForTests` takes a certificate); its texts are
  checked by reading only.
- **The recorded certificate removed by hand**: the record's key is now deleted by name when no certificate of the store names it, where
  before nothing happened. Launch check 14 (b) and the tests cover the re-keyed case; the removed case is the same code path.

## Round 4 fix round (branch `fix/dcfu4-2`, 2026-11-14)

Cut from `fix/dcfu4-1`. The review found five things; all were real, and all are fixed.

### 1 and 3. An earlier key that does not open is put back too (`ClientCertificateStore.AddClientCertificate`)

The owner's answer (b) is "the store as the administrator had it". Round 4 applied it only to an earlier key the import left aside with a
warning (another key set, an unidentified key, a usable key the tool did not create). A store entry whose key provider information names a
key in the import's key set that does not open (a smart card that is not inserted, a TPM or other key that needs a PIN, a key whose rights
keep the administrator out, a deleted key file) got no warning and no `KeyBefore`, so the undo of a failed save and a later save removed
the whole entry, and no message named the key it had. The owner-questions "Done" line, the release note and spec 4.3 described only the
no-key case.

Now, when `IsEntryKeyUsable` turns the entry away without a warning, `IsEntryKeyNotOpenedForeign` decides whether that key is someone
else's: the key provider information was read field by field before the import (`IsFieldsRead`) and names a container, and it is not the
tool's own:

- not the key this import just stored (`IsSameKeyStrict` with the import's key provider information);
- not the key the tool's record names for this certificate (`KeyTool`, `IsSameKeyStrict`);
- not an entry claimed through a record of the old format (a thumbprint with no key named): such a record claims the entry's key by its
  thumbprint, as before round 4, so the entry keeps today's handling (the safe side of `ImportedCertificateRecord`).

Such a key gets `entryBefore=InStoreKeyLeft`, `KeyBefore` (exactly what was read) and a warning that names it:
"The client certificate was already in the computer's certificate store (<store>), but the private key its entry names does not open for
this tool (a key that was deleted, a smart card that is not inserted, a key that needs a PIN, or one this account may not use), and the
tool did not create it. The import gave the entry the private key it stored, and left the entry's earlier key, wherever it is, untouched:
provider P, key container C, in <key set>. A later save that stops using this certificate names that key on the entry again." It gives no
`certutil -delkey` command, unlike the other earlier-key warnings: the key may be a smart card's or need a PIN, which only its owner should
delete. The undo and a later removal then put it back as any other earlier key (`PutBackEarlierKey`). The tool's own key that no longer
opens keeps today's handling: the record's `KeyBefore` stays recorded and the dead key is deleted by its name (`DeleteToolKeyNamed`). An
entry with no key provider information is still removed whole, and so is one whose key provider information could not be read at all.

One consequence for an entry whose key was deleted and which no record names: without a record the tool cannot tell that the dead key was
its own (a key it imported before its record was written, say), so it now puts that name back too. A later save then leaves the entry,
naming a key that is not there, as the administrator had it, where before the entry was removed.

The owner-questions "Done" line, spec 4.3, the release note, `ClientCertificateEntryBefore`'s summaries and
`UndoImportOfFailedSave`'s text for a whole-entry removal ("without a private key of anyone else's that it could name again (none, or the
key this tool gave it before, which no longer opened)") now say this.

### 2. What became of the tool's key is said exactly (`PutBackEarlierKey`, `DeleteToolKey`)

`PutBackEarlierKey` wrote "... and the private key the tool gave it was deleted." before `DeleteToolKeyNamed` ran, then appended its
result, so the text was false when the delete failed, when another certificate of the store named the key (kept, ""), or when the tool's
key was unknown (nothing deleted, ""); the failed put-back branch had the same pattern. `DeleteToolKey` now returns what became of the key
(`ToolKeyFate`: `Deleted`, `NotThere`, `InUse`, `Unknown`, `Failed` with why and where its file is; `TryDeleteKeyNamed` says whether the
key opened, so `Deleted` and `NotThere` differ), and `PutBackEarlierKey` deletes first, then writes only what happened
(`GetToolKeyFateText`): "..., as before that import, and the private key the tool gave it was deleted." only for `Deleted`; otherwise
"..., as before that import. The private key the tool gave it (...) is no longer on this computer." / "... is kept, because another
certificate of the store names it." / "... could not be identified (<why>), so the tool deleted none: ..." / "... could not be deleted:
<why> The key is left on this computer in <file>: delete that file as an administrator." The failed put-back branch says "The entry now
<names ...>." and then the same sentence. `DeleteToolKeyNamed` (the string form the other callers use) is a wrapper that gives the text
only for `Failed`, as before. New seam: `ActionBeforeKeyNamedDeleteForTests`, called with the tool's key provider information in
`TryDeleteKeyNamed` once the key has opened, right before the delete; throwing there is a key that cannot be deleted (the opened object is
disposed without deleting the key).

### 4. The failed save's error leaves out the earlier-key warning once the key is put back (`DbSettingsRunner`)

After an undo that put the earlier key back, the error still began with the import's sentence about that key and its `certutil -delkey`
command, whose condition ("once no other certificate uses it") no longer held: the restored entry uses that key. `ImportPfx` (the public
overload and the internal one with a record) now gives that sentence apart (`warningEntryKeyLeft`), `RemoveImported` has an overload that
says whether the entry names its earlier key again (`isEarlierKeyPutBack`), and `UndoImportOfFailedSave` passes it on. The failed save's
error leaves the sentence out when the key was put back, and keeps it otherwise (no `KeyBefore`, a put-back that failed). A save that
succeeds gives the whole warning as before, the earlier key first; the internal overloads without a record (the tests') still give the
whole warning in `warning`.

### 5. The Direct key-set test cleans up whatever fails (`DirectCertificateKeyProvInfoTests`)

`CheckAndImportPrivateFile_AnEntryWhoseKeyIsInAnotherKeySet_IsNotUsable_WithoutOpeningIt` tracked the second import's key only after
several asserts. It now sets `DirectCertificateImport.ActionAfterKeysStoredForTests` before the first import, as the other two tests of the
class do, so both imports' keys are tracked as they are stored and the `finally` block deletes them whatever fails; it resets the seam
there.

### Tests

- `ClientCertificatePutBackTests.Apply_UndoingAnImportOverAnEntryWhoseKeyDoesNotOpen_PutsThatKeyBack` (findings 1 and 3): the test store's
  entry names a user-set container that no CSP has, through the smart-card CSP name the Direct test uses ("Microsoft Base Smart Card Crypto
  Provider", PROV_RSA_FULL; the seam `ActionBeforePrivateKeyOpenForTests` refuses to open it, so no CSP asks for a card), or through the
  software CAPI provider (a container that does not exist, as a deleted key file; checked before and after that no such container is
  there). For a failed save and a later password save, each kind of key of the file: the import's warning names the key, the entry stays,
  names it again exactly, the tool's key and its file are gone, no record is left. Before the fix the entry was removed.
- `ClientCertificatePutBackTests.Apply_APutBackWhoseToolKeyDeleteFails_SaysTheKeyIsLeft` (finding 2): over a usable key the tool did not
  create, the put-back succeeds and `ActionBeforeKeyNamedDeleteForTests` throws: the message says the entry names its earlier key again and
  that the tool's key could not be deleted, why, and in which file it is left, and never "was deleted"; the key and its file stay (the
  test store's clean-up deletes them).
- `Apply_UndoingAnImportOverAnEntryWithAnEarlierKey_PutsThatKeyBack` and `ClientCertificateImportTests`'s failed-save case 4 (finding 4):
  the failed save's error no longer holds the import's earlier-key sentence or a `-delkey` command; a later save's first result still has
  it.
- `Apply_AnImportOverAnEntryWithoutAUsableKey_IsRecorded_AndALaterSaveDeletesTheKeyTheToolMade` (renamed from `..._AndALaterSaveRemovesItWithTheKeyTheToolMade`):
  (a) an entry with no key provider information (new test-store helper `AddCertificateWithoutKey`) is still removed whole, with no
  warning; (b) an entry whose key was deleted gets the warning and its key name back, and stays. The failed-save test's case 2 (an entry
  whose key was deleted, which the settings name) and `ImportPfx_OfACertificateTheStoreHasWithoutAUsableKey_GivesItTheKeyTheImportStored`
  follow the same rule (a CAPI key that lands in the very container the entry names makes the entry usable, so no warning there).
- `DirectCertificateKeyProvInfoTests` (finding 5): the clean-up change above.

New test-store helper: `ClientCertificateTestStore.NameAbsentKeyForEntry` (the entry names a made-up container through a given provider,
checked by reading it back; nothing is made, opened or deleted for that name).

Results: `dcfu4-2-*.txt` in `.git/od-tools/test-results` (Targeted runs; the tests never touch LocalMachine\My, CurrentUser\My or a real
key, and each test store's clean-up checks what is left after every test).

### Launch checks (administrator only, LocalMachine\My)

18. **An entry whose key does not open** (findings 1 and 3). Put `client-cng.pfx`'s certificate into LocalMachine\My with
    `Import-PfxCertificate` (not the tool), note its "Unique container name" (`certutil -store My <thumbprint>`), then delete that key's
    file in `%ProgramData%\Microsoft\Crypto\Keys` (the entry now names a key that does not open). Import `client-cng.pfx` with the tool:
    the result says the private key its entry names "does not open for this tool", names that container, and ends with the sentence that
    asks that it not be deleted while the record names it. Save with the password: the result says the entry "names its earlier private
    key again", that the tool's key was deleted, and, last, that if someone deleted that earlier key since, the entry names a key that is
    not there (here it is so: the key file was deleted before the import); `certutil -store My <thumbprint>` names
    the noted container again; the tool's key file is gone; LocalMachine\My still holds the certificate. Remove it as that last sentence
    says, with `Remove-Item Cert:\LocalMachine\My\<thumbprint>`. With a smart card or TPM key on a test computer the same holds, and no PIN is asked
    for at the save.
Check 11's second part (a failed save over a certificate an administrator put there) now expects no `certutil -delkey` command in the
error (finding 4).

### Noticed, not changed (for the lead)

- **The tool's own dead key without a record**: an entry whose key the tool imported before any record existed, and whose key file was
  then deleted, now gets that dead name back on a later removal instead of being removed whole. It cannot be told apart from an
  administrator's deleted key without a record; the entry then names a key that is not there, as it did before the import.
- **The not-open warning on a successful save** says that a later save that stops using the certificate names the key again; while the
  settings use the certificate, the entry names the tool's key, which the users can read, as for every other earlier key left aside.


## Round 4 second fix round (branch `fix/dcfu4-3`, 2026-11-14)

Cut from `fix/dcfu4-2`. The review found two things; both were real (the tests below fail against the code before the fix), and both
are fixed. Seam and tests f0b442b5a, code and test expectations 6f287e89c, spec, release notes, owner questions and this note the commit
after. No schema change, no enumeration value stored anywhere, no new dependency.

### 1. An old-format record claimed whatever key its entry named (`ClientCertificateStore.CompleteRecord`)

Round 4 rewrote a record of the old format (a thumbprint line) at the next save that kept the certificate, naming the key its store entry
named then, read without opening it, with no other check. That made the new record claim a key the old rule never claimed: the old rule
claimed the recorded entry's key on a re-import only after `IsEntryKeyUsable` had let the entry through (a key in the import's key set that
opens and signs or decrypts). The sequence the review gave, both cases real:

- (a) An administrator re-keys the recorded certificate's entry to a key in their own user key set. The save records that key as
  `tool.`; the re-import turns the entry away (another key set), leaves that key aside, warns that it was "left ... untouched" with a
  `certutil -delkey` command, and then, because the recorded tool key is not the import's, deletes it by its name in the administrator's
  user key set (`DeleteToolKeyNamed`), without a word. A later password save then names the deleted key on the entry again.
- (b) The entry is re-keyed to a machine key that does not open silently (a TPM key that needs a PIN, a smart-card key). The save records
  it as the tool's; the re-import takes it for the tool's own dead key (`IsEntryKeyNotOpenedForeign`) and opens it by name to delete it.

Now `CompleteRecord` names the entry's key only when the old rule would claim it: its key provider information names a key in the tool's
key set (`IsInToolKeySet`, new: the machine's for LocalMachine\My, the user's for the tests' store) and the key is usable (`IsKeyUsable`,
the check `IsEntryKeyUsable` makes; it opens the key, as the re-import already did for such an entry). Otherwise the record is written in
format 2 with no `tool.` lines, so the old-format rule goes on (the thumbprint claims an entry only once `IsEntryKeyUsable` let it through)
until an import records a key under the strict rule. `ImportedCertificateRecord`'s class summary and `CompleteRecord`'s say so.

The guard: a recorded key outside the tool's key set is never the tool's, since the tool never creates one there (only a record edited
by hand can say so). `IsEntryKeyTools` and `IsEntryKeyNotOpenedForeign` do not match it, the re-import's delete by name skips it, and
`RemoveImported` takes nothing of such a record: the certificate and every key stay, and the warning says "The tool's record names the
client certificate <thumbprint> with a private key outside the key set the tool imports into (...), which the tool never creates, so the
tool leaves the certificate in the computer's certificate store (<store>) and every key as they are. If no settings use it, remove it as an
administrator, in PowerShell: Remove-Item Cert:\<store>\<thumbprint>, and delete its private key only if you know that no one else's
certificate uses it." (nothing when the certificate is no longer in the store). `IsEntryKeyUsable` uses the same helper.

### 2. The import offered to delete an earlier key it records (`GetEntryKeyWhereText`)

Since round 4 an import that leaves the store entry's earlier key aside records it (`KeyBefore`, when its key provider information was
read field by field), and the undo or a later save names it on the entry again. The warning still gave "<who> can delete it, once no other
certificate uses it: certutil [-user] -csp "P" -delkey "C"", whose condition held (no certificate named the key then); an administrator who
ran it got, at the next password save, an entry naming a deleted key and a result that said only that the earlier key was named again.

Now `AddClientCertificate` knows before the warnings are made whether the earlier key will be recorded (`isEarlierKeyRecorded`, the same
condition as `KeyBefore`), and `GetEntryKeyWhereText` (for `GetEntryKeyLeftText`, `GetEntryKeyNotToolsText`, `GetEntryKeyRekeyedText`)
then says, after where the key is: "The tool's record keeps it, and the tool names that key on the entry again when this import is undone
or a later save stops using this certificate, so do not delete that key while the record names it." `GetEntryKeyNotOpenedText` ends with
the same sentence, and so does the unidentified-key text when something was recorded. The `certutil -delkey` command is given only when
nothing could be recorded (`KeyBefore` null). This also covers the failed save whose settings already use the certificate (the import
recorded at once).

The put-back result (`PutBackEarlierKey`, whatever became of the tool's key) now ends: "If someone deleted that earlier key since, the
entry names a key that is not there: then remove the certificate as an administrator, in PowerShell: Remove-Item Cert:\<store>\<thumbprint>."
The tool does not open the earlier key to check (it may be hardware-backed). The review asked for `-DeleteKey` in that command; it is
left out on purpose: in the case the sentence is about the key is gone already, and if it is not, `-DeleteKey` would delete a key the
tool did not create. The wording avoids "was deleted", which the tests reserve for the tool's own key.

### Tests

- `ClientCertificateRecordTests.Apply_AnOldFormatRecordWhoseEntryWasReKeyedByHand_ClaimsNoKeyTheOldRuleDidNot` (finding 1, and 2 for the
  warning): for each kind of key, the tool's import, its record replaced by the thumbprint line, the entry re-keyed by hand (the test store
  adds the same `.pfx` again) and then (a) made to name the machine key set (only the flag; nothing is made, opened or deleted there) or (b)
  refused by the open seam (a key that needs a PIN); a save without an import, the same `.pfx` again, and a password save. No key is
  deleted by its name (new seam `ActionBeforeKeyDeleteByNameForTests`, called in `DeleteToolKey` before anything looks for the key; the
  test's throws for the hand-given key so that even the code before the fix deletes nothing), the saved record names no key, the
  hand-given key and its file stay throughout, the re-import's warning in (a) has the new sentence and no `-delkey`, and the record keeps
  the key as `earlier.`; the password save in (a) names the hand-given key again exactly (that user key and its file still exist; the
  machine-set name stands for it) and ends with the new sentence, in (b) removes the certificate with the second import's key, as the old
  rule did.
- `ClientCertificateRecordTests.Apply_ARecordWhoseToolKeyIsOutsideTheImportsKeySet_IsNotTheTools` (the guard): the record of an import
  rewritten with `tool.keyset=machine`; a password save deletes nothing by its name, leaves the entry and its key, and says why.
- Changed expectations: the earlier-key warnings in `ClientCertificateImportTests` (another key set, a usable key the tool did not create,
  the CNG key named as the import's CAPI container, a key that does not open) now end with the new sentence instead of the `certutil`
  command; every put-back result in `ClientCertificateImportTests` and `ClientCertificatePutBackTests` ends with the new sentence
  (`ClientCertificateRecordTests.GetPutBackHedge`). The unreadable-key case keeps its text (nothing recorded, no command anyway).

**Against the code before the fix** (f0b442b5a, the seam and the tests only): `dcfu4-3-before.txt` (case (a) first: the re-import deletes
the machine-set name by its name, after the warning with its `certutil -delkey` command) and `dcfu4-3-before-b.txt` (case (b) first: the
re-import deletes the hand-given user key, which exists, by its name; the seam refused it); the guard test fails in both (the password save
deletes the record's machine-set key by its name). Every other test passed (212).

**After the fix:** one full lane build (`odbuild.ps1`, the whole solution) with 0 errors, then one Targeted run (`dcfu4-3-final.txt`):

| Assembly | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| NoHqTests | 57 | 57 | 0 | 0 |
| DbConnectionSecurityTests | 225 | 214 | 0 | 11 |
| LibraryUpdateTests | 129 | 129 | 0 | 0 |
| VendorDirectTests | 922 | 922 | 0 | 0 |
| LibraryBindingTests | 17 | 17 | 0 | 0 |
| ProjectFileTests | 132 | 131 | 0 | 1 |

The skips are the earlier rounds' (see Round 4). DbConnectionSecurityTests has the two new tests more than after the first fix round
(223). After the runs no test store is left under `HKCU\Software\Microsoft\SystemCertificates` and no test certificate in CurrentUser\My;
each test class's own clean-up check (`AssertCleanedUp`) passed. No test opens LocalMachine\My or a real key; the machine-set key
provider informations are only names written on certificates of the test stores.

### Launch checks (administrator only, LocalMachine\My)

Checks 7, 8, 10, 11 (second part), 12, 14 and 18 now expect the new sentences: no `certutil -delkey` command for an earlier key the tool
records, and the put-back result's last sentence about a key deleted since (check 8 quotes it). Checks 15 to 17 are unchanged (15 has an
entry whose key the tool made, which `CompleteRecord` still records; 16 and 17 are the Direct import's). New:

19. **An old-format record over an entry re-keyed by hand** (finding 1). With the settings on the password, import `client-cng.pfx` with
    the tool, replace its record by the thumbprint line as check 15 does, then give the entry a key in your own user key set with check
    7's snippet (the store's add replaces the entry's key provider information); note its "Key Container" (`certutil -store My
    <thumbprint>`). Open the tool and save the settings unchanged: `imported-client-certificate.txt` starts with the format 2 line and has
    no `tool.` lines. Import `client-cng.pfx` with the tool again: the result names your key with the sentence that asks that it not be
    deleted, no `certutil -delkey` command; `certutil -user -key "<container>"` still finds your key; the record has `earlier.container=`
    your container and `earlier.keyset=user`. Save with the password: the entry names your key again (`certutil -store My <thumbprint>`),
    `certutil -user -key "<container>"` still finds it. Remove the certificate with `Remove-Item Cert:\LocalMachine\My\<thumbprint>` and
    delete your key with `certutil -user -csp "<provider>" -delkey "<container>"`. A key that does not open silently (a TPM key with a PIN,
    a smart-card key) is the tests' case (b) only: a usable TPM key without a PIN is one the old rule claimed, so `CompleteRecord` records
    it, as the old rule would have taken it on a re-import.

### Noticed, not changed (for the lead)

- **Decided as built by the lead (owner question 4 of the round; the lead fix round below).** **The old-format rule on removal.** A record of the old format whose entry an administrator re-keyed before the upgrade is now kept
  without a key named, so a password save without a re-import still removes that entry with whatever key it names, as the old rule did
  (`RemoveImported` with no key named); only a re-import first records a key under the strict rule. Database connection security is new
  in the next version, so old-format records exist only on test installs; refusing to remove an entry whose old-format record names no
  key would be the stricter choice.
- **A record that cannot be written.** The new earlier-key sentence says the record keeps the key; when the save then cannot write the
  record, its result also says "The tool could not record which client certificate it imported", but the earlier-key sentence before it
  is then not true. Rare (the ConnectionSettings folder is the tool's own); left as it is.
- **A usable key given by hand under an old-format record.** An administrator who re-keyed the recorded entry to a working key of the
  machine key set (`Import-PfxCertificate`, say) before the first save of this build still has that key recorded as the tool's, since the
  old rule claimed it too (the review's rule: claim nothing the old rule did not). Only the kinds of key the old rule refused are now left
  out.


## Round 4, lead fix round (branch `fix/dcfu4-4`, 2026-11-14)

Cut from `fix/dcfu4-3` (5b7d81eb2). The lead's last fix round of round 4: five decisions, each with a test that fails against the code
before the change. Tests and the one new seam 002479e30, code a3475d520, docs the commit after, results the last commit. No schema change,
no enumeration value stored anywhere, no new dependency, no package or version change. The owner's rule frames every decision: the tools
give BUILTIN\Users read access to a private key they import, so they never leave a Users-readable key behind and never touch a key they
did not create; and the owner's answer (b) of 2026-11-13: undoing an import or removing it later puts the store entry's earlier key back
and deletes only the tool's key.

### 1. A record with an earlier key but no tool key (`RemoveImported`, `AddClientCertificate`, `CompleteRecord`)

Such a record (`earlier.` lines, no `tool.` lines) was written only when the import could not read its own key's provider information.
`RemoveImported` then passed the entry's current key to `PutBackEarlierKey` as the tool's key and deleted it after the put-back, though the
tool cannot tell whether that key is its own (someone may have re-keyed the entry by hand since).

- **The guard** (`ClientCertificateStore.IsToolKeyUnknown`, for records already written): `PutBackEarlierKey` takes `isToolKeyKnown`. Without
  a tool key it puts the earlier key back and deletes no key, and the result says: "... its entry names its earlier private key again
  (...), as before that import. Until then the entry named another private key (provider P, key container C, in ...). The tool's record does
  not say which private key the tool gave the entry, so that key may be the one the tool gave it, which the computer's users can read, or
  one someone gave the entry since: the tool deleted no key. Once no certificate names that key, an administrator can delete it if it is not
  someone else's: certutil [-user] -csp "P" -delkey "C"." (then the hedge sentence about a deleted earlier key). A key that cannot be
  identified, or no key named, get their own wording (`GetToolKeyUnknownText`, with `certutil -key` to find it). A failed put-back of such
  a record leaves both keys and keeps the record (decision 2).
- **The import avoids writing such a record:** `AddClientCertificate` reads the import's key provider information once, right after it
  picks the client certificate, and records the entry's earlier key (`isEarlierKeyRecorded`) only when it can also record its own key. When
  it cannot, the record names the certificate only, the earlier-key warning gives the `certutil -delkey` command (nothing records the key),
  and it says: "The tool could not read which private key its import gave the entry (<why>), so its record does not keep the entry's
  earlier key: undoing this import, or a later save that stops using this certificate, removes the entry from the store with the key it
  names then, and does not name the earlier key on it again." The same holds for the tool's own entry without a usable key whose record
  kept the key the entry had before the tool's first import.
- **Nothing turns such a record into a claim:** `CompleteRecord` leaves it as it is, and a re-import that keeps the entry (the old-format
  rule claims it by thumbprint, as for any record that names no tool key) writes the record back as it was instead of naming the entry's
  key as the tool's.

### 2. A put-back that fails keeps the record (owner question 1 of the round; `RemoveImported`, `DbSettingsRunner`)

Before, a put-back that failed while the entry still named the tool's key left that Users-readable key, and the save deleted the record
(the settings no longer used the certificate), so no record named the key. Now `RemoveImported` says `isRecordStillNeeded` (a put-back that
failed while the entry still names the key it named until then; one that could not be checked, decision 5; a store that could not be
opened, for a record with an earlier key), and `DbSettingsRunner.KeepRecordForPutBack` writes the record again: "The tool keeps its record
of the client certificate <thumbprint>, so that a later save tries again to name its earlier private key on its entry."

- **After a later save** (the password, or another certificate): the record of the removed certificate is written back after the save's own
  record write. When the save's record must name the new certificate the settings use (one record names one certificate), it is kept for
  that one, and the result says: "The tool's record names the client certificate <B>, which the settings use, and names one certificate
  only, so it cannot keep <A> as well: no later save tries again, so put the entry back by hand, as said above." A record that cannot be
  written says so, with the same consequence.
- **After a failed save**: `UndoImportOfFailedSave` writes a record naming the import (its tool key and the earlier key), unless the record
  names another certificate (the one the settings as they are use), which it keeps, with the sentence above.
- **The later save that tries again:** `ApplyOrThrow` treats a record with an earlier key whose certificate neither the settings as they
  were nor the saved ones use as a put-back still to do (`isPutBackPending`) and calls `RemoveImported` for it, as for a certificate the save
  stops using. `PlanImportedCertificate` is unchanged (its rule that a record of a certificate the settings no longer use is not "the one
  being replaced" stands; the pending put-back is a separate, narrower case: only records with an earlier key, whose removal gives the
  administrator's key back).
- **An entry that names its earlier key already** (a put-back whose check could not read the store, decision 5): `RemoveImported` takes it
  for the tool's own put-back, deletes only the tool's key, and gives the ordinary put-back text (`GetPutBackDoneText`), not "someone
  changed it by hand".

### 3. The undo of a failed save whose settings already name the certificate (`UndoImportOfFailedSave`)

The "recorded at once" branch (the import's certificate is the one the settings as they are name, or the record names it) kept the
import's key on the entry and recorded it. The owner's answer (b) covers the undo of a failed save, so when the import left the entry's
earlier key aside (`InStoreKeyLeft` with a `KeyBefore`), the undo now puts that key back (`RemoveImported`), the key the save gave the entry
is deleted, and the record stays as it was before the save; the error leaves out the import's warning about that key, as for any undo that
put it back. A put-back that fails there records the import at once, as before (decision 2). Without a `KeyBefore` (the entry had no key,
or the tool's own key from an earlier import, whose dead key the import already deleted by its name, or what the entry named could not be
recorded) the branch records at once, as built: removing such an entry would leave the settings as they are naming no certificate, and
there is nothing to put back.

No concrete reason was found that this breaks a connection the old settings depend on: the entry gets exactly the key provider information
it had before the save, which is what those settings used. When that earlier key worked for the program's users, it works again; when it
did not (a key in an administrator's own key set, a deleted key, a smart card that is not inserted), the connection did not work before
the save either. DCFU3's test case 2 (`Apply_ASaveThatFailsAfterTheImport_UndoesTheImportOrRecordsIt`, an entry whose key was deleted,
which the settings name) expected the old behaviour (the import recorded at once, a later save with the password putting the key back);
it now expects the owner's newer answer: the entry names its deleted key again at once, the import's key and its file are gone, no
record is written, and the settings are as they were.

### 4. The Direct certificate window names an earlier key it leaves (`DirectCertificateImport`)

An import over a store entry of the address's certificate whose key is in another key set than the import's gives the entry the import's
key and leaves that key behind; the window said nothing. `PrivateImportRun.Add` now returns the replaced entry's key provider information
(read before the add, without opening the key), and once every certificate is added (a failure before that puts the store's copy back),
`GetEarlierKeyLeftText` adds to the warning: "The certificate was already in this computer's Direct certificate store with its private key
in another key set than the one the import uses (<subject>, <thumbprint>): the import gave it the private key it stored, and left the
earlier key untouched: provider P, key container C, in <key set>." Only for a key in another key set, as the decision asked; a key in the
import's key set that does not open is left as before, without a warning (Noticed).

### 5. A put-back that cannot be checked (`PutBackEarlierKey`)

When the store could not be read again after the put-back, `keyProvInfoNow` stayed null, so the text said the entry "is no longer in the
store", and the tool's key was deleted unchecked. Now (`GetPutBackNotCheckedText`) no key is deleted (if the write did not take, the
certificate would name a key that is gone), the record is kept (decision 2), and the result says: "... and its earlier private key (...)
was written on its entry again, but the store could not be read again to check the entry: <why> So the tool cannot tell whether the entry
names its earlier private key again or still the private key the tool gave it (...), which the computer's users can read; it deleted no
key, so that the certificate does not name a key that is gone, and left the entry as it is. Check the entry as an administrator (certutil
... -store ...) and, if it does not name its earlier key, import the certificate with that key again." (When the write failed too: "...
but its earlier private key (...) could not be named on its entry again (<why>), and the store could not be read again ...".) The next save
that finds the entry naming its earlier key deletes the tool's key (decision 2's last point). New seam: `ActionBeforePutBackCheckForTests`,
called right before the store is read again.

### The round's owner questions

1. A put-back that fails while the entry still names the tool's key: decided by the lead (keep the record, a later save tries again) and
   done, decision 2.
2. The undo of a failed save whose settings already name the certificate: decided by the lead (answer (b) covers it) and done, decision 3.
3. An entry the tool did not record whose named key does not open: answered by the first fix round (`IsEntryKeyNotOpenedForeign`, its
   items 1 and 3): such a key is left aside with a warning, recorded as the entry's earlier key, and put back by the undo or a later
   removal. Checked again here: `Apply_UndoingAnImportOverAnEntryWhoseKeyDoesNotOpen_PutsThatKeyBack` passes.
4. An old-format record over an entry re-keyed by hand before the upgrade: decided by the lead as built (the old rule, the second fix
   round's Noticed item), and kept documented: old-format records exist only on test installs, because database connection security is new
   in the next version.

`docs/decisions/owner-questions.md` lists them under "DIRECT-CERT-FOLLOWUP round 4's review questions"; the owner can still choose otherwise.

### Tests

- `ClientCertificateRecordTests.Apply_ARecordWithAnEarlierKeyButNoToolKey_PutsTheEarlierKeyBack_AndDeletesNoKey` (decision 1): for each
  kind of key, the tool's import over an entry with a usable key it did not create, the record's `tool.` lines dropped, the entry re-keyed to
  a throwaway key (the test store adds the same `.pfx` again), a save with the password: the entry names its earlier key again exactly, no
  key is deleted by its name (`ActionBeforeKeyDeleteByNameForTests` sees none), the throwaway key and its file are untouched, the result is
  the whole text above, no record is left.
- `ClientCertificateRecordTests.Apply_AnImportThatCannotReadItsOwnKey_RecordsNoEarlierKey_AndSaysSo` (decision 1, the import): the import's
  key provider information made unreadable (`ActionBeforeKeyProvInfoReadForTests`, for the import's container only); the warning gives the
  `-delkey` command and the new sentence, the record names the certificate with no key; a later save with the password removes the entry
  with the import's key, and the earlier key and its file are untouched.
- `ClientCertificatePutBackTests.Apply_APutBackThatFails_LeavesTheToolsKeyWhileTheEntryStillNamesIt` (decision 2), now for (a) a later save
  with the password and (b) a failed save, each with the put-back refused or dropped: as before, plus the result ends with the
  keep-the-record sentence, the record names the certificate, the tool's key and the earlier key; then a save with the password (the seam
  gone) puts the key back, deletes the tool's key and its file, and leaves no record.
- `ClientCertificateImportTests.Apply_ASaveThatFailsAfterTheImport_UndoesTheImportOrRecordsIt` case 2 (decision 3), as said above.
- `DirectCertificateKeyProvInfoTests.ImportPrivateFile_OverAnEntryWhoseKeyIsInAnotherKeySet_WarnsThatTheEarlierKeyWasLeftUntouched`
  (decision 4): the second import over the entry made to name the machine key set (only its flag; nothing is made, opened or deleted
  there) gives exactly the warning above; the entry's new key works, the earlier user key is untouched.
- `ClientCertificatePutBackTests.Apply_APutBackThatCannotBeChecked_DeletesNoKey_AndSaysTheStoreCouldNotBeReadAgain` (decision 5): the seam
  throws as the store is read again: the result is the whole text above, never "is no longer in the store" or "was deleted"; the tool's
  key and its file stay, the record is kept; a later save with the password gives the ordinary put-back text and deletes the tool's key.

**Against the code before the change** (002479e30, the seam and the tests only): `dcfu4-4-before.txt` in `.git/od-tools/test-results`
(DbConnectionSecurityTests and LibraryUpdateTests). Exactly the six tests above fail, each for the reason its decision names: the
guard test deletes the throwaway key by its name; the import test finds the "record keeps it" sentence and a record with an earlier key;
the put-back failure test finds no keep-the-record sentence (the record was deleted); case 2 finds the import recorded at once; the
Direct test gets no warning; the unchecked put-back says "The entry now is no longer in the store" and deletes the tool's key. Every other
test passed (212 and 129); NoHqTests was not built for that run (a project build only).

**After the change:** a Targeted run of the two assemblies on a3475d520 (`dcfu4-4-after-1.txt`: all passed), then one full lane build
(`odbuild.ps1`, the whole solution) with 0 errors, then one Targeted run (`dcfu4-4-final.txt`):

| Assembly | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| NoHqTests | 57 | 57 | 0 | 0 |
| DbConnectionSecurityTests | 228 | 217 | 0 | 11 |
| LibraryUpdateTests | 130 | 130 | 0 | 0 |
| VendorDirectTests | 922 | 922 | 0 | 0 |
| LibraryBindingTests | 17 | 17 | 0 | 0 |
| ProjectFileTests | 132 | 131 | 0 | 1 |

The skips are the earlier rounds' (see Round 4). DbConnectionSecurityTests has the three new tests more than after the second fix round
(225), LibraryUpdateTests one. Clean-up: before the first run and after every run, the key files under `%APPDATA%\Microsoft\Crypto\Keys`
and `RSA\<SID>`, the stores under `HKCU\Software\Microsoft\SystemCertificates`, and the `synthetic` or `example.test` certificates of
CurrentUser\My and LocalMachine\My (read only) were listed and compared: no name new, none missing, no test store left; each test class's
own clean-up check (`AssertCleanedUp`) passed. No test opens LocalMachine\My or a real key; the machine-set key provider informations are
only names written on certificates of the test stores.

### Launch checks (administrator only, LocalMachine\My)

Checks 7 to 19 are unchanged (none of them has a put-back that fails, a failed save whose settings name the certificate, or a Direct
import over an entry in another key set that looks at the warning). New:

20. **A failed save whose settings already name the certificate** (decision 3). As check 18, put `client-cng.pfx`'s certificate into
    LocalMachine\My with `Import-PfxCertificate`, note its "Unique container name", save the settings with that certificate by thumbprint
    (`/thumbprint=`), then delete that key's file in `%ProgramData%\Microsoft\Crypto\Keys`. Hold
    `FreeDentalConfig.xml` open (check 11's snippet) and import `client-cng.pfx` with the tool: it fails with exit code 3, and the error ends
    with the put-back text ("its entry names its earlier private key again (...), as before that import, and the private key the tool gave
    it was deleted", then the sentence about a key deleted since). `certutil -store My <thumbprint>` names the noted container again, the key
    file the import made is gone, `imported-client-certificate.txt` is as it was (absent), and the settings still name the thumbprint.
    `$f.Close()`, then remove the certificate with `Remove-Item Cert:\LocalMachine\My\<thumbprint>`.
21. **The Direct window and a key in another key set** (decision 4). Repeat check 17 up to the import: the import's result now says that the
    certificate was already in this computer's Direct certificate store with its private key in another key set, names your user key's
    provider and container "in a user key set", and that it was left untouched. Clean up as check 17 says.

Checked by the tests only: a record with an earlier key but no tool key (no build writes one any more), an import that cannot read its
own key, a put-back that fails or cannot be checked, and the save that tries again.

### Noticed, not changed (for the lead)

- **A re-import under a record with an earlier key but no tool key** still lets the old-format rule claim the entry's key by thumbprint
  (`IsEntryKeyTools`, `IsEntryKeyNotOpenedForeign`), so that key gets the users' read access if the entry was re-keyed to a usable key of
  the machine key set; the record is written back as it was, never naming that key as the tool's, and its removal deletes no key. Only an
  import of a build before this fix round could have written such a record, and none was released.
- **A record kept for a put-back still to do, and a new certificate the settings use:** one record names one certificate, so the record of
  the new one wins, and the result says that no later save tries again for the other. A record of several certificates would need a format
  change.
- **The Direct window and a key in its own key set that does not open** (deleted, a smart card not inserted): the add gives the entry the
  import's key and leaves that key as before, without a warning; decision 4 asked only for a key in another key set.
- **A removal whose store cannot be opened, for a record without an earlier key** (`RemoveImported`'s store exception): the certificate and
  its key are still there and the result says how to remove them, but the record goes with the save, as before; a later save would not try
  again (only records with an earlier key are a pending put-back).

## Round 4, fix round 5 (branch `fix/dcfu4-5`, 2026-11-14)

Cut from `fix/dcfu4-4` (e3d832ed3). One review finding on the lead fix round. Test 1164a8299, code 5f5ce3ad2, docs the commit after,
results the last commit. No schema change, no record format change, no new dependency, no package or version change.

### A failed save overwrote a put-back still to do (`DbSettingsRunner.UndoImportOfFailedSave`)

Decision 2 made a new kind of record: one kept for a certificate R that the settings no longer use, whose entry still waits for its
earlier key (a put-back that failed or could not be checked). `UndoImportOfFailedSave` guarded it in its first branch only (an import no
settings use: the other record is kept). Its other two branches wrote the record of the import's certificate T over it without a word:
decision 3's branch when T's own put-back failed (`KeepRecordForPutBack(pathRecord,T,null)`), and the "recorded at once" branch (T's
import had no `KeyBefore`). The concrete sequence: the tool imports R over an administrator's key, the settings use R; a save with the
password cannot put R's earlier key back (a store that refuses or drops the write), so the record of R is kept; a save with
`/thumbprint=` T (R's retry fails again); the administrator imports T's `.pfx` while `FreeDentalConfig.xml` is held open, the save fails,
T's put-back fails for the same reason, and T's record replaced R's. The error said "The tool keeps its record of the client certificate
T" and nothing about R, whose entry still named the Users-readable key the tool gave it, which no record or message named again.

- **The choice:** of the two the finding offered, the record names T, the certificate the settings as they are use, and the error names R.
  This is the rule the successful save already follows (the lead fix round's Noticed item: "the record of the new one wins, and the result
  says that no later save tries again for the other"); with T recorded, a later save that stops using T puts T's earlier key back, while
  keeping R instead would leave the settings' own certificate with a key the tool made and no record (a later re-import would then take
  that key for someone else's). An import no settings use keeps the other record as before.
- **The text** (`ClientCertificateStore.GetPutBackNotTriedAgainText`), after the sentence about T: "The tool's record named the client
  certificate R as well, whose entry in the computer's certificate store (LocalMachine\My) was still to get its earlier private key
  (provider P, key container C, in ...) back; the record names one certificate only, so no later save tries that put-back again. Its
  entry may still name the private key the tool gave it (...), which the computer's users can read. Check the entry as an administrator
  (certutil -store "My" R); to put it back as it was, delete the tool's key (certutil -csp "P" -delkey "C"), then import the certificate
  R with its earlier key again." For a record with no tool key (decision 1's older records) it says how to find the key instead. When T's
  record cannot be written, the record of R stays and the sentence is left out.
- **`KeepRecordForPutBack`'s refusal** said "which the settings use" also when the other record was a put-back still to do (the first
  branch); it now says that only when they do, and otherwise "whose entry still waits for its earlier private key, which a later save
  tries again to put back".

### Tests

- `ClientCertificatePutBackTests.Apply_AFailedSaveRecordingTheSettingsCertificate_NamesThePendingPutBackItNoLongerRetries`, with the
  put-back seam (`ActionInsteadOfKeyProvInfoPutBackForTests`, refusing): the import of R over a usable key the tool did not create; a save
  with the password (R's record kept); the settings name T, (a) by a `/thumbprint=` save over a usable key the tool did not create (R's
  record kept again), or (b) T in the store without a key and `FreeDentalConfig.xml` naming it; a save that imports T's `.pfx` fails, the
  seam still refusing, so (a) runs decision 3's branch with T's put-back failing and (b) the "recorded at once" branch. The error must name
  R, say that no later save tries its put-back again, and give the `certutil -delkey` command of R's tool key; the record names T with the
  tool's key (and its earlier key in (a)); R's entry still names the tool's key; a save with the password then removes T as its record
  says (the tool's key of T and its file gone, no record), and R's keys stay (the store's clean-up deletes them). Every case runs, and the
  test fails with each case's failure. For each kind of key.

**Against the code before the change** (1164a8299, the test only): `dcfu4-5-before.txt` in `.git/od-tools/test-results`
(DbConnectionSecurityTests). Exactly the new test fails, all six cases at step 4 (the error does not name R; the record of R was
overwritten); every other test passed (217, 11 skipped). NoHqTests was not built for that run (a project build only).

**After the change:** a Targeted run of DbConnectionSecurityTests on 5f5ce3ad2 (`dcfu4-5-after-1.txt`: 229, 218 passed, 0 failed, 11
skipped), then one full lane build (`odbuild.ps1`, the whole solution) with 0 errors, then one Targeted run (`dcfu4-5-final.txt`):

| Assembly | Total | Passed | Failed | Skipped |
|---|---|---|---|---|
| NoHqTests | 57 | 57 | 0 | 0 |
| DbConnectionSecurityTests | 229 | 218 | 0 | 11 |
| LibraryUpdateTests | 130 | 130 | 0 | 0 |
| VendorDirectTests | 922 | 922 | 0 | 0 |
| LibraryBindingTests | 17 | 17 | 0 | 0 |
| ProjectFileTests | 132 | 131 | 0 | 1 |

The skips are the earlier rounds'. DbConnectionSecurityTests has the one new test more than after the lead fix round (228). Clean-up:
before the first run and after every run, the key files under `%APPDATA%\Microsoft\Crypto\Keys` and `RSA\<SID>`, the stores under
`HKCU\Software\Microsoft\SystemCertificates`, and the `synthetic` or `example.test` certificates of CurrentUser\My and LocalMachine\My
(read only) were listed and compared: no name new, none missing, no test store left; each test class's own clean-up check
(`AssertCleanedUp`) passed. No test opens LocalMachine\My or a real key.

### Launch checks (administrator only, LocalMachine\My)

Unchanged: the sequence needs a store that refuses the put-back, which only the tests' seam gives.
