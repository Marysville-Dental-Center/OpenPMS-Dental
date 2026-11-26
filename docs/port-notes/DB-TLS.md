# Port note: DB-TLS (O-12, the local test database runs encrypted)

Package O-12 of the .NET 10 plan (`docs/specs/net10-plan.md` section 12, O-12, answered by the owner on 2026-11-08: yes), built on branch
`dev/dbtls-1` from `main` (99bb32282, with `main` merged in at e1262eab8); the review's fixes on `dev/dbtls-2` (section 8), and there the
fix round after the two final reviews (section 9, with `main` merged in at 447b09374: S1-6's SDK-style test projects and TC-2). The owner
runs one script once as administrator,
`tools/Enable-LocalDbTls.ps1`, which turns on TLS in the local MariaDB service on `127.0.0.1:3306`. From then on the program's development
set-up, the test helpers and the dev tools connect encrypted and verify the server against the script's test CA. S2-2's TLS and
certificate-login test runs against this server, so O-8's temporary second server is not needed.

The program's own connection rule (`docs/specs/db-connection-security.md`) does not change. Nothing in this package changed the MariaDB
server, its option file or its service: the survey only read through SQL and files, and the script has not been run for real.

## 1. What this PC has (read-only survey, 2026-11-08)

Read through SQL on `127.0.0.1:3306` (the dev connection, `root`) and from files; nothing was changed.

| What | Found |
|---|---|
| Service | `MariaDB`, running, automatic start, account `NT SERVICE\MariaDB` (a virtual service account). Command line: `mysqld.exe "--defaults-file=<install folder>\data\my.ini" MariaDB`, the MariaDB 10.11 installer's layout (`%ProgramFiles%\MariaDB 10.11`). |
| Version | `10.11.19-MariaDB`, `version_compile_os` Win64. |
| TLS library | `version_ssl_library` = `WolfSSL 5.9.1` (MariaDB's Windows builds are linked with wolfSSL; `have_openssl` = `NO`). |
| TLS state | `have_ssl` = `DISABLED` (compiled in, not configured); `ssl_ca`, `ssl_capath`, `ssl_cert`, `ssl_key`, `ssl_cipher`, `ssl_crl`, `ssl_crlpath` empty; `tls_version` = `TLSv1.2,TLSv1.3` (the 10.11 default); `require_secure_transport` = `OFF`. The dev session's `Ssl_cipher` is empty: today every connection is unencrypted. |
| Option file | `my.ini` in the data folder, 633 bytes, ASCII, LF line ends (one CRLF), sections `[mysqld]` and `[client]`; `port=3306`, `bind-address=127.0.0.1` (only this computer can connect), `sql_mode=''` and the other Open Dental settings; no TLS line. ACL: inherited from the data folder (SYSTEM, Administrators, `NT SERVICE\MariaDB` and the developer's account full control, Users read). |
| Own certificate | None. MariaDB 10.11 does not make one; only MariaDB 11.4 and later generate a self-signed certificate at start-up when none is configured. |
| Tools that make PEM files without a download | Windows PowerShell 5.1 on .NET Framework 4.8 (release 534296): `System.Security.Cryptography.X509Certificates.CertificateRequest` and `SubjectAlternativeNameBuilder` (.NET Framework 4.7.2 and later) make a CA and certificates in memory, and an ephemeral `RSACng` key exports as PKCS#8 (`CngKey.Export(CngKeyBlobFormat.Pkcs8PrivateBlob)`, export policy `AllowPlaintextExport`). `New-SelfSignedCertificate` (PKI module) and `certutil.exe` exist too, but they work through the certificate store (keys land in a store before they can be exported). MariaDB's `bin` folder has no `openssl.exe`. No C# step through the build lane is needed. |
| Client | `mariadb.exe` 15.1 (10.11.19) in the same `bin` folder: `--ssl` on by default (falls back to plain when the server has no TLS), `--ssl-ca`, `--ssl-verify-server-cert` (off by default), `--tls-version`. |
| Host names | An in-process TLS handshake on a loopback port (no database) with a certificate for `localhost` and `127.0.0.1` from such a CA: .NET Framework's `SslStream` reports no name mismatch for the target `127.0.0.1`, only the expected untrusted-root error, which MySqlConnector's `SslCa` check then accepts. So `SslMode=VerifyFull` works for the IP address as well as for `localhost`. |
| Lane and test scripts | `run-tests.ps1` (the repository's and the coordinator's copy) and the build lane do not connect to the database. They need no change: the switch below is an environment variable, which the test processes inherit. |

## 2. The server side: `tools/Enable-LocalDbTls.ps1`

ASCII, Windows PowerShell 5.1. It changes nothing before it has printed its plan and you have typed `YES`.

**Checks (every mode).** The session is elevated (else it refuses; `-DryRun` and `-TestCertificatesFolder` need no elevation). The service
(`-ServiceName`, default `MariaDB`) exists and runs `mysqld.exe` or `mariadbd.exe` with `--defaults-file=`. That `my.ini` is UTF-8 (or
ASCII) text whose bytes come back unchanged from the decoding (else it refuses with the offset of the first other byte: writing such a file
back would change bytes the script does not mean to touch), reads no other option file (a line starting `!include` or `!includedir` is
refused: options there are out of its sight), and says `port=3306` (or no port) and `bind-address=127.0.0.1`. The process that listens on
`127.0.0.1:3306` (`Get-NetTCPConnection -LocalPort 3306 -State Listen`, no elevation needed) is the service's own (`Win32_Service`
`ProcessId`), and no other process listens on `0.0.0.0` or `::` port 3306, which would take the connections to `127.0.0.1:3306` while the
service's socket is closed; this is checked before the client sends anything there and again after every restart (the restart waits until
it holds). The server that answers on `127.0.0.1:3306` reports port 3306, bind address `127.0.0.1` and that `my.ini`'s data folder. Every
connection of the script goes to `127.0.0.1:3306` over TCP with the service's own `mariadb.exe`, as `-User` (default `root`), the password
from `MYSQL_PWD` when set.

**Enable (no mode switch).** Refuses when `my.ini` already has a TLS line (`ssl`, `ssl_ca`, `ssl_cert`, `ssl_key`, `ssl_capath`,
`ssl_cipher`, `tls_version`, `require_secure_transport`), when the server already has TLS, or when a backup, a certificate folder or a
temporary `my.ini.opms-new` of an earlier run exists. It refuses a dev folder or a client folder (steps 3 and 4) that

- is not `<a user profile's %LOCALAPPDATA%>\OpenPmsDental\DevDbTls` (`DevDbTlsClient` for the client folder) of a profile of this computer
  (the registry's profile list, readable without elevation, and the running account's own), in an `AppData\Local` that exists; these are
  the only folders `-Revert` and `-RequireSecureTransport` accept from the record (below), so an enable run never records one they would
  refuse;
- is a drive root, is or holds a user profile, or is or overlaps the MariaDB folder, the server's data folder or the `opms-dev-tls` folder;
- exists and holds anything but the files the script writes there (`ca.pem`; `client-cert.pem` and `client-key.pem`), a subfolder
  included, so the run replaces only its own files and its ACL hides nothing of anyone else's;
- is inside the repository, or (the client folder) is the dev folder, inside it or around it.

Then:

1. Makes in memory a test CA (RSA 3072, SHA-256, 10 years, `CA:true, pathlen 0`, keyCertSign and cRLSign) and a server certificate
   (RSA 2048, 5 years, `CN=localhost`, SAN `DNS:localhost` and `IP:127.0.0.1`, serverAuth). The CA's private key is never written
   anywhere: nothing else can ever be signed with it. **No client certificate by default**: the owner approved a test CA and a server
   certificate; S2-2's client certificate (RSA 2048, 5 years, `CN=opms-dbtls-test-client`, clientAuth) is made only with
   `-WithClientCertificate`, once the owner has said yes to it (section 10). Because the CA's key is gone after the run, a client
   certificate can only be made in the same run as the CA: a later yes means `-Revert` and the script again with the switch (a new CA,
   which the dev tools pick up from the same path).
2. Writes `ca.pem`, `server-cert.pem` and `server-key.pem` (PKCS#8) to `<MariaDB folder>\opms-dev-tls`, next to `bin` and `data`. Not inside
   `data`: MariaDB takes every folder there for a database. The folder and files get a protected ACL: owner Administrators, SYSTEM and
   Administrators full control, the service account (`NT SERVICE\MariaDB`) read, nobody else. These are the only keys the server reads.
3. Writes the CA certificate (the public part only) as `ca.pem` to `-DevFolder`, default `%LOCALAPPDATA%\OpenPmsDental\DevDbTls` of the
   account that runs the script. The folder is created when missing (and `OpenPmsDental` with it when that is missing too) and gets a
   protected ACL (nothing inherited): `-DevUser` (default that account), SYSTEM and Administrators full control, nobody else; an existing
   folder has its ACL replaced so. Every worktree of that account finds the CA there. **That folder never holds a private key**
   (`DevDbTlsTests.TheCaFolder_NeverHoldsAPrivateKey_...` checks it where the script ran).
4. Only with `-WithClientCertificate`: writes `client-cert.pem` and `client-key.pem` to `-ClientFolder`, default `DevDbTlsClient` beside
   the dev folder (`%LOCALAPPDATA%\OpenPmsDental\DevDbTlsClient`, `DevDbTls.GetDefaultClientFolder()`), a folder of its own, created when
   missing, with a protected ACL: `-DevUser` read only, SYSTEM and Administrators full control. Nothing is ever written to the repository.
5. Copies `my.ini` to `my.ini.before-opms-dev-tls` and adds at the end of its `[mysqld]` section (every other byte of the file stays as it
   was, line ends and a BOM included). The comment lines record where steps 3 and 4 wrote (the `client-folder` line only with
   `-WithClientCertificate`) and the parent folder the run creates for them (the `created-parent` line only when `OpenPmsDental` did not
   exist); `-Revert` and `-RequireSecureTransport` read them back:

   ```ini
   # BEGIN Open PMS - Dental local test TLS (tools/Enable-LocalDbTls.ps1; undo with -Revert)
   # dev-folder=<dev user's %LOCALAPPDATA%>/OpenPmsDental/DevDbTls
   # client-folder=<dev user's %LOCALAPPDATA%>/OpenPmsDental/DevDbTlsClient
   # created-parent=<dev user's %LOCALAPPDATA%>/OpenPmsDental
   ssl_ca=<MariaDB folder>/opms-dev-tls/ca.pem
   ssl_cert=<MariaDB folder>/opms-dev-tls/server-cert.pem
   ssl_key=<MariaDB folder>/opms-dev-tls/server-key.pem
   tls_version=TLSv1.2,TLSv1.3
   # END Open PMS - Dental local test TLS
   ```

6. Restarts the service (connections drop for a few seconds), waits until `127.0.0.1:3306` answers and its listener is the service's new
   process, and checks a TLS session (`have_ssl=YES`, `Ssl_cipher` not empty), then the server certificate against the CA copy (`--ssl-ca`
   with `--ssl-verify-server-cert`). When anything fails after the first change (exit code 3), it restores `my.ini` from the backup and
   restarts, removes the backup (the run refused to start with one there, so it is the run's own), and undoes the files: a `ca.pem`,
   `client-cert.pem` or `client-key.pem` that existed before the run gets its bytes and access rules back, one the run added is removed, a
   dev or client folder that existed gets its access rules back, one the run created is removed when it is empty and so is the
   `OpenPmsDental` parent it created, the `opms-dev-tls` folder is removed, and so is `my.ini.opms-new` when a write left it. When the undo
   fails too (exit code 4), the message names what to restore and remove by hand, and the files that held something before the run.

It does not set `require_secure_transport`: unencrypted connections keep working until the lead's `-RequireSecureTransport` step.

**The record in `my.ini`.** `-RequireSecureTransport` and `-Revert` never recompute the dev folder or the client folder from the account
that runs them: they take them from the block's `# dev-folder=` and `# client-folder=` lines, so an enable run with `-DevUser`/`-DevFolder`
for another account is undone in that account's folders. A `-DevFolder` or `-ClientFolder` that names another folder than the recorded
one is refused (the same folder written another way is accepted). They refuse a recorded folder that no enable run can have written: a
dev folder that is not `<a user profile's %LOCALAPPDATA%>\OpenPmsDental\DevDbTls` of this computer, a client folder that is not
`...\OpenPmsDental\DevDbTlsClient` (each judged in its full form, so `..` segments and relative paths are refused), and a
`# created-parent=` line that is not the parent folder of one of them; the message says that something else wrote that line and that
`my.ini` is restored and the files removed by hand. Only when `my.ini` has no block of this script (someone removed it) do they fall back
to `-DevFolder`, else the running account's default (the same rule applies to it), and the `-Revert` plan then says that a copy in another
account's folder is not touched.

**`-RequireSecureTransport`** (the lead's later step). Needs the script's block in `my.ini`, the CA copy in the recorded dev folder, a
working verified TLS connection against it, and no `my.ini.opms-new`. Rewrites the block (the record kept) with
`require_secure_transport=ON`, restarts, and checks that a verified TLS connection works and an unencrypted one is refused; otherwise it
puts the block back as it was and restarts (exit code 3).

**`-Revert`.** Restores `my.ini` from `my.ini.before-opms-dev-tls` (when `my.ini` was changed since, the plan lists the lines that the restore
drops), restarts, checks an unencrypted connection, then removes the `opms-dev-tls` folder, `ca.pem` in the recorded dev folder,
`client-cert.pem` and `client-key.pem` in the recorded client folder (each folder too when it is then empty), the recorded created
parent (`OpenPmsDental`) when it is then empty, and the backup. The plan lists each path.

**`-DryRun`** with any mode: the checks and the plan, no change, no elevation. **`-TestCertificatesFolder <folder>`**: writes only a
certificate set (the CA certificate, the server certificate and key, and with `-WithClientCertificate` the client certificate and key) to
an empty folder outside the repository; the tests use it. `-WithClientCertificate` goes with the enable run or `-TestCertificatesFolder`
only; `-ClientFolder` needs it (or names the recorded folder for the later modes).

Exit codes: 0 done, nothing to do, or a dry run; 1 refused; 2 you did not type `YES`; 3 the change failed and was undone; 4 the change
failed and could not be undone (the message says what to do by hand).

## 3. The client side: `DataConnectionBase.DevDbTls`

For the development set-up, the test helpers and the dev tools only. The program never calls it; `DevDbTlsTests.TheProgramNeverCallsDevDbTls`
checks every C# file outside `tests`, `tools`, `UnitTests`, `UnitTestsCore`, `NoHqTests` and the two developer tools `DocumentationBuilder`
and `xCrudGenerator` (below; `TheDevToolsThatCallDevDbTls_AreNotProductProjects` checks that they are no product project). The class is in
`DataConnectionBase.dll` because every test project and dev tool already references that assembly, so no project file had to change.

- **The CA.** `OPMS_DB_TLS_CA` (a file that must exist), else `%LOCALAPPDATA%\OpenPmsDental\DevDbTls\ca.pem` when it exists, else none.
- **The local test server.** `localhost` or a loopback address on port 3306, or on a port named in `OPMS_DB_TLS_RELAY_PORTS` (a test's TCP
  relay to it; `AdoptionValidation.Tests`' `TcpRelay` registers its port while it runs). Any other server or port is left exactly as the
  caller had it: the local end of an SSH forward to another server (the runbook's `13306`) must never be offered the test CA.
- **With the CA**: `Apply(connectionString)` sets `SslMode=VerifyCA;SslCa=<ca>` (replacing any SslMode); `GetSslCa(server)` returns the
  CA path for `DataConnection.SetDb`, `SetDbT`, the `DataConnection` constructor or `BuildSimpleConnectionString`, whose loopback branch then
  writes `SslMode=VerifyCA;SslCa=<ca>`, as it always did for a loopback server with a CA; `GetClientArguments(host,port)` gives
  `--ssl-ca=<ca> --ssl-verify-server-cert` for `mariadb.exe` and `mariadb-dump.exe`.
- **Without the CA**: everything exactly as before (the string unchanged, `""`, no arguments), so nothing breaks before the owner's script.
- **`OPMS_DB_REQUIRE_TLS=1`** (also `true`, `yes`; `0`, `false`, `no` or unset: off; anything else is an error, so a typing error never turns
  it off), the gates' switch: a missing CA for the local test server is an error (`ApplicationException` naming the CA path, the variable
  and the script). Most helpers turn a connection error into an inconclusive test, as they do when no server answers; the canary is
  `DevDbTlsServerTests`, which fails outright when TLS is required and the CA is missing.
- `VerifyCA` (the chain against the test CA) rather than `VerifyFull` for the helpers, as the program's own loopback strings with a CA; the
  server certificate names `localhost` and `127.0.0.1`, so S2-2 can use `VerifyFull` (`DevDbTlsServerTests` checks both names).

`tools/SyntheticData` references nothing of the program, so it has the same rules in its own `Infrastructure/LocalTls.cs` (`Db.Open`;
`SslMode=None` as before without the CA and for other ports). `tools/DevDbTls.ps1` has them for the scripts. A test checks that the three
name the same variables, file and port.

### What connects through it

| Where | Change |
|---|---|
| 89 helpers and test files under `tests/` (every `ScratchDb`, `TestDb` and the like) | `new MySqlConnection(DevDbTls.Apply(...))` and `SetDb`/`SetDbT(...,DevDbTls.GetSslCa(server))`, made mechanically; every other line as it was. |
| `tests/DbConnectionSecurityTests` | The database tests pass the CA (`BuildSimpleConnectionString`, `SetDb`, `new DataConnection`, `MiscData.MakeABackup(sslCa:)`, the tool's `TestConnection` and `CaFile`). The read-only server tests seed the read-only server's own CA preference with the test CA, so it connects the old way with `SslMode=VerifyCA` (and `SslMode=none` without the CA). `DataConnection_Loopback_ConnectsWithoutTls_AsBefore` keeps proving the program's loopback string without a CA is `SslMode=none`; once the server requires TLS it proves the server refuses it. |
| `tests/DbGrantsTests/GrantServer.cs`, `tests/LibraryUpdateTests/EmailMessagesTests.cs` | The CA in the connection string and in the `CentralConnectionBase` override. |
| `tests/DbGrantsTests/GrantRun.Program.cs`, `GrantRun.Refusals.cs` | `MiscData.MakeABackup(serverName:...)` passes `sslCa:DevDbTls.GetSslCa(...)`. The backup to "another server" (`127.0.0.1:3306`) already reached the CA through the `DataConnection(server,...)` constructor's fallback to the thread's `SslCa` (set by the run's `SetDb` with the CA), so this makes it explicit rather than changing what it connects with. |
| `tests/BSERVERTests/ReplicationDelayDbTests.cs` | The read-only server override goes to `ConnectionStoreBase.OverrideConnection` with `SslCa`, because `CentralConnection`'s conversion to `CentralConnectionBase` does not carry `SslCA` (an observation about the program, not changed here). |
| `OpenPmsDentalBusiness/WebServices/OpenDentalServerMockIIS.cs` | The Middle Tier mock server takes an optional `sslCa` (default `""`) and passes it to its thread's `SetDbT`: the program's existing loopback-with-CA branch, so the connection rule does not change and the program still never calls `DevDbTls`. Before, the server side of every mock test that gives it a server built `SslMode=none`, whatever the client side had. |
| `tests/MiddleTierMockTests/ScratchDb.cs`, `ScratchDbCopy.cs` (`NewMockServer`), `tests/EhrTests/Support/ScratchDb.cs` (`UseMockMiddleTier`), `UnitTests/UnitTests/ReportsTests.cs` | The mock server gets `sslCa:DevDbTls.GetSslCa(server)`. |
| `UnitTests/Misc/TestBase.cs` | The `DentalOffice` override (the `DataAction.RunPractice` threads, e.g. `StatementT.CreateStatement`) gets `SslCa=DevDbTls.GetSslCa(TestServer)`, as in `EmailMessagesTests`. |
| `UnitTestsCore/Misc/DatabaseTools.cs` | `SetDbConnection` (the legacy `UnitTests` and its windows) passes `GetSslCa(serverAddr)`. |
| `tools/DevDbTool/DevDatabase.cs` | Both connections pass `GetSslCa`; a remote server (`--allow-remote-server`) gets `""`, as before. |
| `tools/New-DevDatabase.ps1` | `-WriteConfig` adds an `SslCa` element with the CA's path to `FreeDentalConfig.xml` for the local test server, so `OpenPmsDental.exe` (Debug) connects with `SslMode=VerifyCA`: `CentralConnections.TryToConnect` passes the configuration's `SslCa` to `SetDb`. Without the CA the element is left out, as before. |
| `tools/New-OdReferenceDatabase.ps1` | `mariadb.exe` gets the TLS arguments. |
| `tools/AdoptionValidation` | `BusinessLayer.Connect` passes `GetSslCa`; `DatabaseCopier` gives `mariadb-dump.exe` and `mariadb.exe` the TLS arguments (quoted); the tests' `mariadb.exe` call too; `TcpRelay` registers its port. On the office's admin PC (no test CA, or the forward's port) nothing changes. |
| `tools/SyntheticData/Infrastructure/Db.cs` | `LocalTls.Apply` instead of the fixed `SslMode=None`. |
| `tests/ToothChartImageTests/Support/ChartTestDb.cs` (TC-2, from `main` in the fix round) | `DevDbTls.Apply` and `SetDb(...,DevDbTls.GetSslCa(Server))`, as every other helper. |
| `DocumentationBuilder/DataConnection.cs` | Both strings it builds (`SslMode=none`) go through `DevDbTls.Apply`; it creates its `MySqlConnection` directly, so the local test server gets `SslMode=VerifyCA` with the CA, every other server as before. |
| `xCrudGenerator/CrudGenHelper.cs` | `ConnectToDatabase`'s `SetDb` gets `DevDbTls.GetSslCa(server)` (the program's loopback branch with a CA). `ConnectToDatabaseM` (upstream's server over the network, which the rule refuses without an office CA) is unchanged. |

**The dev-only programs outside `tools`** (item 7 of section 9), decided by one rule: a developer tool that is never released and is no
product project (it does not link `SharedAssemblyInfo.cs`, which every shipped project and every side program does) gets the CA from
`DevDbTls`, because its connections are no rule of the program's; a product project keeps its connections as they are, because `DevDbTls`
in it would put the developer's CA path and the `OPMS_DB_*` switches into a program of the product (`TheProgramNeverCallsDevDbTls` keeps
checking it).

| Program | Covered | Why |
|---|---|---|
| `DocumentationBuilder` | yes | Developer tool, not shipped (README), no product project. |
| `xCrudGenerator` | yes | Generates `OpenPmsDentalBusiness/Crud`; never released, no product project. |
| `DatabaseIntegrityCheck` (`FormDatabaseCheck.OpenConnection`, its `SslMode=none` string) | **no** | A side program of the product (in the source, not in the setup; it links `SharedAssemblyInfo.cs`). |
| `CentralManager` (`CentralConnectionHelper.SetCentralConnection`'s `SetDbT`, `FormCentralManager`'s `SetDb`, both with no CA) | **no** | A side program of the product, as above. Its connections come from its central database's connection list; giving them a CA is the program's connection work, not O-12's. |

Both stop reaching the local test server once it requires TLS (section 5 step 4): their loopback strings have `SslMode=none`. Nothing runs
them in a test or a gate.

**Program paths that pass `sslCa=""` for a server they name** (not covered: they are the program's own code, which O-12 does not change).
Each builds `SslMode=none` for the local test server, so a test that reaches one connects without TLS today and fails once the server
requires it; no test in the repository reaches one with the local server, as far as the scan's shapes and a search show:

- `ConnectionStoreBase.SetDb(ConnectionNames,...)` (`DataConnectionBase/ConnectionStoreBase.cs` about line 255; `SetDbT` passes the
  entry's `SslCa`, `SetDb` does not);
- `DataAction.GetT(fn,server,db,user,...)` and `DataAction.Run(action,server,...)` (`OpenPmsDentalBusiness/Db/DataAction.cs` about 108);
- `LargeTableHelper`'s copy to another server (`OpenPmsDentalBusiness/Db/LargeTableHelper.cs` about 426, `SetDbT(_serverTo,...)`);
- the archive database (`OpenPmsDentalBusiness/Db Multi Table/MiscData.cs` about 449, `SetDb(archiveServerName,...)`);
- the Middle Tier's `OpenDentalServerConfig.xml` reader, `Userods.LoadDatabaseInfoFromFile` (`OpenPmsDentalBusiness/Data
  Interface/Userods.cs` about 685): its `ComputerName`/`User`/`Password` elements give `SetDb(server,...)` with no CA (the file has no
  `SslCa` element); only its `ConnectionString` element can carry TLS options (section 5 step 2).

`DevDbTlsTests.EveryTestAndToolConnectionToTheLocalServer_GoesThroughDevDbTls` reads every C# file under `tests`, `tools`, `UnitTests`,
`UnitTestsCore`, `NoHqTests`, `DocumentationBuilder` and `xCrudGenerator` and fails for a call that connects to a server it names without
the test CA:

- `new MySqlConnection(...)` whose argument is not `DevDbTls.Apply(...)`;
- `DbConnectionSecurity.NewMySqlConnection(...)` or `NewMySqlConnectionWithoutPooling(...)` whose argument is neither
  `DevDbTls.Apply(...)` nor a `DataConnection.BuildSimpleConnectionString(...)` call whose `sslCa` argument (`sslCa:` or the sixth) is
  `DevDbTls.GetSslCa(...)`;
- `SetDb`/`SetDbT` with a server (seven or more arguments, the seventh the database type) without `DevDbTls.GetSslCa(...)`;
- `new DataConnection(...)` with a server (five or more arguments) without it;
- a `new CentralConnectionBase { ... }` initializer with `ServerName=` and no `SslCa=DevDbTls.GetSslCa(...)`;
- every `new CentralConnection { ... }` initializer with `ServerName=`, whatever it sets: its conversion to `CentralConnectionBase` drops
  `SslCA`, so the message points to `new CentralConnectionBase { ServerName=...,SslCa=DevDbTls.GetSslCa(...) }`;
- `MiscData.MakeABackup(...)` with `serverName:` (or a server as its first argument) without `sslCa:DevDbTls.GetSslCa(...)`;
- `new OpenDentalServerMockIIS(...)` (any namespace prefix) with `server:` (or a server as its first argument) without it;

apart from a short list of deliberate exceptions, each with its file, its kind of call and its reason (the TLS tests' own strings and their
CA-less refusal check, servers over the network that the rule refuses before connecting, strings that are only created and never opened,
`LoopbackDatabaseTests`' connection without a CA on purpose, the read-only server's own thread string, the grants run's helper that passes
the CA itself, and `DocumentationBuilder`'s `ConnStr`, set from `DevDbTls.Apply` just before). Calls of these names that name no server
(they take the thread's own connection, as `new OpenDentalServerMockIIS()`, `MakeABackup()` or `NewMySqlConnection("")`) are not checked.
It also asserts that it found calls of every kind; `TheConnectionScan_FindsEachCallShape_WithAndWithoutTheCa` checks its reading of each
shape, with and without the CA, on synthetic source. A package that adds a helper after this one is merged gets the fix in the failure
message. Connections the scan cannot see by name (a connection string built in program code from the thread's context) follow from the
helpers' own `SetDb` with the CA.

## 4. The owner's steps (run once)

Before: tell the lead, so that no build-lane or test run uses the database during the restart.

1. Open Windows PowerShell with **Run as administrator** (from the account that runs the dev tools and tests, so that the CA lands in its
   `%LOCALAPPDATA%`).
2. Go to a checkout of `main` that has this package:

   ```powershell
   cd <repository folder>
   ```

3. Look at the plan (changes nothing):

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -DryRun
   ```

4. Turn TLS on, and type `YES` when it asks:

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1
   ```

   This makes the test CA and the server certificate only, as approved on 2026-11-08; the plan's step 1 says `No client certificate`.
   It ends with `TLS is on: TLSv1.3 (or TLSv1.2), cipher ...` and `The server certificate was verified against ...\ca.pem`. A `WARNING`
   line about the verification goes to the lead (the tests check it with the program's connector); `FAILED` with `Undone:` means nothing
   stayed changed.

When the elevated window runs as another account than the one that runs the tests, add
`-DevUser <DOMAIN>\<user> -DevFolder <that user's %LOCALAPPDATA%>\OpenPmsDental\DevDbTls` (the plan says so when it notices). The script
records that folder in `my.ini`, so the undo below finds it without repeating the switches.

Only if the owner has said yes to S2-2's client certificate (section 10), add `-WithClientCertificate` to step 4 (it is made with the CA,
so not later without a new CA). Its key goes to `%LOCALAPPDATA%\OpenPmsDental\DevDbTlsClient` (with `-DevFolder`: `DevDbTlsClient` beside
it), never to the CA's folder.

**To undo it** (also as administrator): `powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -Revert`, then
`YES`. It removes the files in the folders the enable run recorded in `my.ini` (the plan lists them), also another account's after
`-DevUser`/`-DevFolder`, and the `OpenPmsDental` folder when the enable run created it and it is then empty; a `-DevFolder` that names
another folder is refused, and so is a record that no enable run can have written (section 2). The dev tools and tests then connect as
before (they find no CA).

## 5. The lead's steps afterwards

1. Run the TLS tests with the requirement on: `$env:OPMS_DB_REQUIRE_TLS='1'`, then a Targeted run of `DbConnectionSecurityTests`,
   `SyntheticData.Tests` and `NoHqTests`. Every `DevDbTlsServerTests` test passes, except
   `WhenTheServerRequiresTls_AConnectionWithoutTlsIsRefused` (inconclusive until step 4).
2. The coordinator's next Full run with `OPMS_DB_REQUIRE_TLS=1`. Every connection the scan of section 3 checks (the helpers' strings,
   `NewMySqlConnection` with the program's string, `SetDb`/`SetDbT`, `DataConnection` with a server, the `CentralConnectionBase` overrides,
   `MakeABackup` to a server, and the Middle Tier mock server's own thread) then connects with TLS and the test CA, and so does every thread
   that takes its context from them. What the run cannot show before step 4 is a connection outside those shapes that still has no CA (the
   program paths of section 3 that pass `sslCa=""` for a server, `DatabaseIntegrityCheck`, `CentralManager`): the server accepts it. Step
   4's run is the check for that (a connection without TLS is then refused and its test fails), so run the Full run again after step 4.
   **The Middle Tier gate** (the 3b gate's real Middle Tier on the build PC, S3b-6): its `OpenDentalServerConfig.xml` must name the database
   with a `<ConnectionString>` element that carries the TLS options, for example
   `<ConnectionString>Server=127.0.0.1;Port=3306;Database=<db>;User ID=<user>;Password=<password>;SslMode=VerifyCA;SslCa=<the CA path of
   DevDbTls.GetCaPath()></ConnectionString>`, not the `ComputerName`/`User`/`Password` elements, which `Userods.LoadDatabaseInfoFromFile`
   turns into `SslMode=none` for this server (the file has no `SslCa` element); or S3b-6 adds an `SslCa` element to that file and passes
   it to `SetDb`. Once step 4 has run, the elements' form is refused by the server.
3. Tell the other sessions first: once the server requires TLS, a worktree without this package fails its database tests (its `SetDb`
   helpers write `SslMode=none`). Every worktree that still runs database tests merges or rebases onto `main` with this package. The
   uncovered programs of section 3 (`DatabaseIntegrityCheck`, `CentralManager`) no longer reach the local server from then on.
4. As administrator: `powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -RequireSecureTransport`, then `YES`.
   It checks that a verified TLS connection works and an unencrypted one is refused (and undoes the line otherwise). Run step 1 again: now
   `WhenTheServerRequiresTls_AConnectionWithoutTlsIsRefused` and `DataConnection_Loopback_ConnectsWithoutTls_AsBefore` show the refusal.
5. Confirm with the owner right before the drop, then rebuild the old test database over the encrypted connection:

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-DevDatabase.ps1 -Force -WithSampleData -WriteConfig
   ```

   `-Force` drops `opendental_dev` first (it refuses a database with more than 200 patients); `-WriteConfig` writes `FreeDentalConfig.xml`
   with the `SslCa` element, so the Debug program connects encrypted too.
6. S2-2's TLS test needs nothing more: the server's `ssl_ca` is the test CA and its certificate names `127.0.0.1` and `localhost`, so it
   connects with `VerifyFull` (the server listens on `127.0.0.1` only, so there is no other address of the build PC to use). Its
   certificate-login part needs a client certificate signed by that CA, which the script makes only with `-WithClientCertificate` and only
   in the same run as the CA. So it waits for the owner's answer (section 10). With a yes: `-Revert`, then the enable run again with
   `-WithClientCertificate` (a new CA; the dev tools find it at the same path), as administrator, with the other sessions told first as
   for any restart. Then `client-cert.pem` and `client-key.pem` in `DevDbTls.GetDefaultClientFolder()` (`%LOCALAPPDATA%\OpenPmsDental\DevDbTlsClient`)
   are a client certificate the server's CA signed, so a `REQUIRE X509` account accepts it; the test imports that pair through a CAPI and
   a CNG key provider into the current user's store. Until then `DevDbTlsServerTests.ClientCertificateForS22_ChainsToTheServersCa` is
   inconclusive. A new CA for other reasons (new certificates five years from now) is the same `-Revert` and enable run;
   `DevDbTlsServerTests.ServerCertificate_IsValidForAnotherMonth` fails a month before the server certificate expires.

## 6. Checks run here

Without the owner's script (no CA yet), so the server tests were inconclusive by design:

- The script, not elevated: no switch, `-RequireSecureTransport` and `-Revert` each print `REFUSED: This script changes the MariaDB service
  and must run elevated: start PowerShell with "Run as administrator" (or use -DryRun to see the plan).` and exit 1, before any plan.
  `-Revert -RequireSecureTransport` is refused (one mode at a time); a `-TestCertificatesFolder` inside the repository or not empty is
  refused. `-Revert -WithClientCertificate` and `-RequireSecureTransport -WithClientCertificate` print `REFUSED: -WithClientCertificate
  goes with the enable run (or -TestCertificatesFolder) only: the client certificate is made with the CA.`; `-ClientFolder` without
  `-WithClientCertificate` prints `REFUSED: -ClientFolder needs -WithClientCertificate (the enable run), or names the recorded client
  folder for -Revert or -RequireSecureTransport.`; each exits 1 before the elevation check.
- `-DryRun` on this PC after the fix round (exit 0; the user profile, the account and the process id shortened here; `OpenPmsDental`
  does not exist yet in that profile, so the run would create it and records it):

  ```text
  Not elevated: a dry run only. The real run must be started as administrator.
  Service:        MariaDB (Running), account NT SERVICE\MariaDB
  Server program: C:\Program Files\MariaDB 10.11\bin\mysqld.exe
  Option file:    C:\Program Files\MariaDB 10.11\data\my.ini
  Listener:       127.0.0.1:3306, process <pid> (mysqld), the service's own
  Server:         10.11.19-MariaDB on 127.0.0.1:3306, data folder C:\Program Files\MariaDB 10.11\data\
  TLS now:        have_ssl=DISABLED, require_secure_transport=0, tls_version=TLSv1.2,TLSv1.3, this session's cipher ''

  Plan:
    1. Make a test CA (its private key stays in memory and is discarded) and a server certificate for localhost and 127.0.0.1.
       No client certificate (-WithClientCertificate is not given): S2-2's certificate login waits for the owner's yes. It can only be
       made in the same run as the CA, so it then means -Revert and this script again with -WithClientCertificate.
    2. Create C:\Program Files\MariaDB 10.11\opms-dev-tls (owner Administrators; SYSTEM and Administrators full control, NT SERVICE\MariaDB read) with
       ca.pem, server-cert.pem and server-key.pem.
    3. Write the CA certificate (public part only) to C:\Users\<user>\AppData\Local\OpenPmsDental\DevDbTls\ca.pem.
       The folder is created with a protected ACL (nothing inherited): <DOMAIN>\<user>, SYSTEM and
       Administrators full control, nobody else. No private key goes to this folder.
       C:\Users\<user>\AppData\Local\OpenPmsDental does not exist and is created for it (-Revert removes it again when it is then empty).
    4. Copy C:\Program Files\MariaDB 10.11\data\my.ini to C:\Program Files\MariaDB 10.11\data\my.ini.before-opms-dev-tls, then add at the end of its [mysqld] section (the comment lines
       record the folders above; -Revert and -RequireSecureTransport read them from there):
         # BEGIN Open PMS - Dental local test TLS (tools/Enable-LocalDbTls.ps1; undo with -Revert)
         # dev-folder=C:/Users/<user>/AppData/Local/OpenPmsDental/DevDbTls
         # created-parent=C:/Users/<user>/AppData/Local/OpenPmsDental
         ssl_ca=C:/Program Files/MariaDB 10.11/opms-dev-tls/ca.pem
         ssl_cert=C:/Program Files/MariaDB 10.11/opms-dev-tls/server-cert.pem
         ssl_key=C:/Program Files/MariaDB 10.11/opms-dev-tls/server-key.pem
         tls_version=TLSv1.2,TLSv1.3
         # END Open PMS - Dental local test TLS
    5. Restart the service MariaDB (connections drop for a few seconds) and check a TLS session and the server
       certificate against the CA. If the server does not come back with TLS, the backup is restored, the files this run
       replaced are put back as they were and the files and folders it added are removed.
    require_secure_transport is not set: unencrypted connections keep working until -RequireSecureTransport.

  Dry run: nothing was changed.
  ```

  `-DryRun -WithClientCertificate` has the client step in its own folder, and the record line for it:

  ```text
    1. Make a test CA (its private key stays in memory and is discarded) and a server certificate for localhost and 127.0.0.1.
       With -WithClientCertificate also a client certificate (clientAuth) for the certificate-login test of S2-2.
    ...
    4. Write the client certificate and key to C:\Users\<user>\AppData\Local\OpenPmsDental\DevDbTlsClient\client-cert.pem and C:\Users\<user>\AppData\Local\OpenPmsDental\DevDbTlsClient\client-key.pem:
       a folder of its own, not the CA's. The folder is created with a protected ACL (nothing
       inherited): <DOMAIN>\<user> read only, SYSTEM and Administrators full control.
       C:\Users\<user>\AppData\Local\OpenPmsDental does not exist and is created for it (-Revert removes it again when it is then empty).
    5. Copy ... then add at the end of its [mysqld] section (...):
         # BEGIN Open PMS - Dental local test TLS (tools/Enable-LocalDbTls.ps1; undo with -Revert)
         # dev-folder=C:/Users/<user>/AppData/Local/OpenPmsDental/DevDbTls
         # client-folder=C:/Users/<user>/AppData/Local/OpenPmsDental/DevDbTlsClient
         # created-parent=C:/Users/<user>/AppData/Local/OpenPmsDental
         ...
  ```

  Each of these unelevated dry runs exits 1 with its `REFUSED:` line before any plan, and writes nothing (the fix round, items 1 and 6):

  | Switches | Refusal |
  |---|---|
  | `-DevFolder "C:\Program Files\MariaDB 10.11"`, and `...\MariaDB 10.11\data\DevDbTls` | `... is or overlaps the MariaDB folder C:\Program Files\MariaDB 10.11.` |
  | `-DevFolder C:\` | `-DevFolder C:\ is a drive root.` |
  | `-DevFolder C:\Users\<user>` | `... is or holds the user profile C:\Users\<user>.` |
  | `-DevFolder <a temporary folder>\OpenPmsDental\DevDbTls`, `-DevFolder %LOCALAPPDATA%\OpenPmsDental\Other` | `... is not <a user profile>\AppData\Local\OpenPmsDental\DevDbTls of this computer. Only such a folder is accepted: -Revert and -RequireSecureTransport take no other from the record in my.ini.` |
  | `-WithClientCertificate -ClientFolder %LOCALAPPDATA%\OpenPmsDental\DevDbTls\client` | `... and -DevFolder ... overlap. The client key goes to a folder of its own, never where the dev tools read the CA.` |
  | `-WithClientCertificate -ClientFolder %LOCALAPPDATA%\OpenPmsDental\ClientElsewhere` | `... is not <a user profile>\AppData\Local\OpenPmsDental\DevDbTlsClient of this computer. ...` |
  | `-Revert -DevFolder <a temporary folder>\x` | `The dev folder ..., from -DevFolder (or this account's default), is not <a user profile>\AppData\Local\OpenPmsDental\DevDbTls of this computer, the only dev folder this script writes to. ...` |

  `-DryRun -RequireSecureTransport` refuses (no block yet); `-DryRun -Revert` says there is nothing to revert.
- `New-DevDatabase.ps1`'s config writing, run on a temporary folder with `OPMS_DB_TLS_CA`: one `SslCa` element after `MySQLPassHash` with the
  CA, none without a CA or for another port; with `OPMS_DB_REQUIRE_TLS=1` and no CA the CA look-up stops with the message.

Tests (Targeted runs, all passed): `DbConnectionSecurityTests` 140 (130 passed, 10 inconclusive: the nine `DevDbTlsServerTests` and
`DataConnection_Loopback_WithTheLocalTestServersCa_VerifyCa`, until the CA exists), `SyntheticData.Tests` 58 (57 passed, the pre-existing
`RealDump_Has422Tables` inconclusive), `MiddleTierMockTests` 125, `ReleaseFlowTests` 120 (119 passed), `LibraryBindingTests` 12, `NoHqTests`
58, `EhrTests` 88 (its helper came from `main`), and for the hand-edited helpers `BSERVERTests` 57, `LibraryUpdateTests` 100,
`AdoptionValidation.Tests` 69 and `DbGrantsTests` 3 (2 passed, the grants run inconclusive without its variables). The full build has no
error. `ForkMigrationTests` is named in the package's list but left out under the
rule for every package (the coordinator runs it with the init file); its helper change is the same mechanical one as the others.

After the review's fixes (`dev/dbtls-2`, full build without error), Targeted runs, all passed: `DbConnectionSecurityTests` 144 (134
passed, the same 10 inconclusive), `MiddleTierMockTests` 125, `EhrTests` 88, `DbGrantsTests` 3 (2 passed, as before), `SyntheticData.Tests`
58 (57 passed), `ReleaseFlowTests` 120 (119 passed), `LibraryBindingTests` 12, `NoHqTests` 58, and the legacy `UnitTests` 1731 (1728
passed, the 3 skipped as before) for `TestBase` and `ReportsTests`. `ForkMigrationTests` again left out under the same rule (its helper is
not touched by the fixes).

After the fix round (section 9, `main` merged in; full lane build without error), Targeted runs, all passed: `DbConnectionSecurityTests`
150 (140 passed, the same 10 inconclusive until the CA exists), `MiddleTierMockTests` 125, `SyntheticData.Tests` 58 (57 passed),
`ReleaseFlowTests` 120 (119 passed), `LibraryBindingTests` 12, `EhrTests` 88, `NoHqTests` 58, and for TC-2's `ChartTestDb`
`ToothChartImageTests` 119 (116 passed, its three recording and gallery tools skipped as they are on `main`). `ForkMigrationTests` and
`InitSqlTests` left out under the rule for every package.

New tests:

- `tests/DbConnectionSecurityTests/DevDbTlsTests.cs` (no server): the connection-string choices of section 3, the program's loopback string
  with the CA, the command-line arguments, the client folder beside (never inside) the CA's folder and no private key in the CA's folder
  where the script ran, the source checks (the program never calls `DevDbTls`; the scan of section 3 and its reading of each call shape).
- `tests/DbConnectionSecurityTests/DevDbTlsScriptTests.cs`: the script's certificate set from `-TestCertificatesFolder`, checked offline (the
  CA's constraints and usages; the server certificate's names read from the DER, `DNS:localhost` and `IP:127.0.0.1`, serverAuth only; with
  `-WithClientCertificate` the client certificate's clientAuth; both chain to the CA, SHA-256, valid 5 years; each PKCS#8 key matches its
  certificate and signs; no CA key anywhere; a second run makes a new CA, and without the switch no client certificate or key); the
  `my.ini` edit on synthetic LF, CRLF, BOM and unterminated files through the script's own functions; the record of the folders in the
  block, read back for `-Revert` and `-RequireSecureTransport` (another account's folders, a `-DevFolder` or `-ClientFolder` that disagrees
  refused, the same folder written another way accepted, the fallback without a block); the refusals without elevation and of the client
  switches outside the enable run; a dry run that leaves `my.ini` byte for byte as it was, writes no client key by default, records the
  dev folder, and with `-WithClientCertificate` puts the key in the client folder. From the fix round (section 9), all without elevation
  on the script's own functions with scratch folders and synthetic files: the enable run's folder checks (each refusal of item 1 and the
  accepted cases, this account's defaults among them); the record checks of item 6 (each refusal, and `Get-Paths` taking a record in this
  account's own `%LOCALAPPDATA%` and refusing one elsewhere); the listener check on synthetic listeners and on this computer's real one
  (the MariaDB service's own process); ANSI and UTF-16 files and `!include` lines refused, UTF-8 with and without a BOM read; the undo of
  a failed run and `-Revert`'s removal of the created parent on scratch folders (bytes and access rules back, the added files gone); and
  the dry-run test also checks the listener line, the dev user's full control, the created-parent record and the seven refusals above.
- `tests/DbConnectionSecurityTests/DevDbTlsTests.cs`, from the fix round: the scan's new kinds (`NewMySqlConnection` with the builder's
  `sslCa`, `CentralConnection` always flagged) with and without the CA on synthetic source, the scan over the two developer tools, and
  `TheDevToolsThatCallDevDbTls_AreNotProductProjects`.
- `tests/DbConnectionSecurityTests/DevDbTlsServerTests.cs` (once the CA exists; a failure with `OPMS_DB_REQUIRE_TLS=1` and no CA): a
  helper's connection is encrypted (`Ssl_cipher`, TLS 1.2 or 1.3) and the server offers TLS 1.2 and 1.3 only; the program's
  `DataConnection` with the helpers' CA is encrypted; a `dbtls_` scratch database is created, filled, read and dropped over TLS; `VerifyFull`
  works for `127.0.0.1` and `localhost`; another CA is refused (`VerifyCA` and `VerifyFull`), so the verification is real; the server
  certificate is valid for another month; S2-2's client certificate (in the client folder, when the script ran with
  `-WithClientCertificate`) chains to the CA; `mariadb.exe` with the TLS arguments is encrypted;
  with `require_secure_transport` a connection without TLS, and the program's loopback string without a CA, are refused.
- `tools/SyntheticData/SyntheticData.Tests/LocalTlsTests.cs`: `LocalTls`.

## 7. Files

- New: `tools/Enable-LocalDbTls.ps1`, `tools/DevDbTls.ps1`, `DataConnectionBase/DevDbTls.cs`, `tools/SyntheticData/Infrastructure/LocalTls.cs`,
  the four test files above, this note.
- Changed: the helpers and files of section 3, `tools/FreeDentalConfig.dev.xml` (its comment), `docs/specs/db-connection-security.md`
  (section 7: the dev tools' rows, the side programs' rows and a row for `DevDbTls`). In the program's assemblies only `OpenPmsDentalBusiness/WebServices/OpenDentalServerMockIIS.cs`
  (the test-only mock server: an optional `sslCa`, default `""`, which no shipped code path passes). In the fix round also
  `tests/ToothChartImageTests/Support/ChartTestDb.cs` (from `main`), and the two developer tools `DocumentationBuilder/DataConnection.cs`
  and `xCrudGenerator/CrudGenHelper.cs` (no product project).
- Not changed: the program's connection rule and every shipped code path, `run-tests.ps1` and the build lane (they do not connect),
  `docs/specs/net10-plan.md` (the lead's; O-12's row can point here).

## 8. The review's fixes (`dev/dbtls-2`)

1. **The client certificate was made by default and its key written next to the published CA.** Now opt-in (`-WithClientCertificate`,
   off by default, section 2 step 1), so the documented owner command makes only what the owner approved on 2026-11-08 (a test CA and a
   server certificate for `127.0.0.1` and `localhost`). With the switch, the pair goes to a folder of its own (`DevDbTlsClient`, `-DevUser`
   read only), never to the CA's folder, which the dev tools read and which never holds a private key. The plan, the header help, sections
   2, 4 and 5.6 and the script tests match.
2. **`-Revert` recomputed the dev folder from the account that runs it.** The enable run now records the dev folder (and the client
   folder) in its `my.ini` block; `-Revert` and `-RequireSecureTransport`'s CA check read the record and refuse a `-DevFolder` or
   `-ClientFolder` that disagrees, so an enable run for another account (`-DevUser`/`-DevFolder`) is undone there, and a stale `ca.pem`
   can no longer make the helpers ask a server without TLS for `VerifyCA`. Without a record the plan says that another account's folder is
   not touched.
3. **The Middle Tier mock server connected its own thread without TLS.** `OpenDentalServerMockIIS` takes `sslCa` and passes it to `SetDbT`
   (the program's loopback-with-CA branch; the program never calls `DevDbTls`); `NewMockServer` (both), EhrTests' `UseMockMiddleTier` and
   the legacy `ReportsTests` pass `DevDbTls.GetSslCa`. The scan now checks `new OpenDentalServerMockIIS(` with a server.
4. **Two more connection shapes skipped `DevDbTls`.** The legacy `UnitTests`' `DentalOffice` override now has `SslCa` (real: `ConnectionStoreBase.SetDbT`
   passes `conn.SslCa`, which was null, so those threads built `SslMode=none`). The grants run's `MakeABackup(serverName:...)` calls pass
   `sslCa` explicitly; for the backup to `127.0.0.1:3306` this was not a live gap (the `DataConnection(server,...)` constructor falls back
   to the thread's `SslCa` when `sslCa` is `""`, and the run's `SetDb` had set the CA), but the call no longer depends on that fallback and
   the scan can check it. The scan now also checks `new DataConnection(` with a server, `CentralConnectionBase`/`CentralConnection`
   initializers with `ServerName=` and `MakeABackup` with a server (section 3); run against the old caller code it flags exactly the eight
   call sites of findings 3 and 4. Section 3 and section 5 step 2 say what the Full run with the switch does and does not prove.

## 9. The fix round after the final reviews (`dev/dbtls-2`, 2026-11-09)

Two independent final reviews called the documented default command safe to run; the lead had their minor findings fixed before the owner
runs the script. First `main` was merged in (35cf85ecf, no conflict: this branch changed no project file, and S1-6's SDK-style test projects
compile every `.cs` file under their folders); the scan then found main's new `ChartTestDb` (TC-2), which now connects through `DevDbTls`
(b50a30111; EhrTests' `ScratchDb` already did).

The script (f8bf7f03f; its tests c45deb823, section 6):

1. **Folders.** `-DevFolder` and `-ClientFolder` are refused when the folder exists and holds anything but the script's own files, or is
   or overlaps the MariaDB folder, the server's data folder or the `opms-dev-tls` folder, or is a drive root or is or holds a user profile
   (`Get-FolderProblem`, with `Test-FolderOverlap`); and, so that an enable run never records a folder that item 6 refuses, each must be a
   profile's `%LOCALAPPDATA%\OpenPmsDental\DevDbTls` (`DevDbTlsClient`) in an `AppData\Local` that exists. The plan says that the folder is
   created (or its ACL replaced) with a protected ACL and that the dev user gets full control (it said "readable by").
2. **The listener.** The process behind `127.0.0.1:3306` (`Get-NetTCPConnection`) must be the service's own (`Win32_Service.ProcessId`),
   and no other process may listen on `0.0.0.0` or `::` port 3306; before the client sends anything there, and after every restart.
3. **`!include`.** A `my.ini` with a line starting `!include` (also `!includedir`) is refused.
4. **Encoding.** A `my.ini` whose bytes do not come back unchanged from the decoding is refused, with the offset of the first such byte.
5. **The undo.** A failed enable run puts back the files it replaced (bytes and access rules) and the access rules of the folders that
   were there, removes what it added, the `OpenPmsDental` parent it created when that is empty, and `my.ini.opms-new`; a run refuses to
   start when `my.ini.opms-new` exists. The enable run records the parent it creates (`# created-parent=`), and `-Revert` removes it when
   it is empty.
6. **The record.** `-Revert` and `-RequireSecureTransport` refuse a recorded dev or client folder that is not a profile's
   `%LOCALAPPDATA%\OpenPmsDental\DevDbTls` or `\DevDbTlsClient` (judged in its full form), and a recorded created parent that is not the
   parent of one of them (`Get-RecordProblem`); the fallback's `-DevFolder` the same.

The connections (370e2ac28):

7. **The dev-only programs outside `tools`**, decided per program by one rule (section 3): `DocumentationBuilder` and `xCrudGenerator`
   are developer tools and no product project, so they get the CA from `DevDbTls` (no rule of the program's changes);
   `DatabaseIntegrityCheck` and `CentralManager` are side programs of the product (they link `SharedAssemblyInfo.cs`), so they are named
   as not covered and stop reaching the local server once it requires TLS. `docs/specs/db-connection-security.md` section 7's rows say
   so. `TheDevToolsThatCallDevDbTls_AreNotProductProjects` checks the rule.
8. **The scan reads `NewMySqlConnection`.** `DbConnectionSecurity.NewMySqlConnection(...)` and `...WithoutPooling(...)` must go through
   `DevDbTls.Apply`, or take a `BuildSimpleConnectionString(...)` whose `sslCa` argument is `DevDbTls.GetSslCa(...)`; the deliberate
   network and CA-less strings (RuleTests, PreferenceServerTests, LoopbackDatabaseTests, PreferenceServerDatabaseTests' thread string,
   DevDbTlsServerTests' refusal check, the grants run's helper) are on the allow-list, which names the kind of call too.
9. **`new CentralConnection { ServerName=... }`** is always flagged, whatever it sets, and the message points to
   `new CentralConnectionBase { ServerName=...,SslCa=DevDbTls.GetSslCa(...) }`. A negative run (main's `ChartTestDb` and a temporary,
   unbuilt file with both new shapes) flagged exactly its four calls.

The notes (this note's sections 3 and 5):

10. Section 3 lists the program paths that pass `sslCa=""` for a server (`ConnectionStoreBase.SetDb`, `DataAction.GetT`/`Run` with a
    server, `LargeTableHelper` about 426, `MiscData` about 449, `Userods.LoadDatabaseInfoFromFile` about 685, the Middle Tier's
    `OpenDentalServerConfig.xml` reader) as not covered; section 5 step 2 says that the Middle Tier gate's `OpenDentalServerConfig.xml`
    names its database with a `<ConnectionString>` carrying `SslMode=VerifyCA;SslCa=<ca>` (or S3b-6 adds `SslCa` to that file).

Nothing here changed the MariaDB service, `my.ini` or a certificate store; the script ran only unelevated (dry runs and its functions on
scratch folders), and every database connection went to `127.0.0.1:3306`.

## 10. Owner question

- **S2-2's client certificate.** Should the script make a client certificate signed by the test CA, for S2-2's certificate-login test (a
  `REQUIRE X509` test account on the local server; the key in `%LOCALAPPDATA%\OpenPmsDental\DevDbTlsClient`, readable by the dev account)?
  Until the answer is yes the script makes none and that part of S2-2 waits. A yes after the enable run means `-Revert` and the enable run
  again with `-WithClientCertificate` (a new CA), because the CA's key is discarded after each run. **Answered 2026-11-09: yes**
  (`docs/decisions/owner-questions.md`, "Pending items answered on 2026-11-09"); section 11 runs it together with the test network.

## 11. The test network of S2-2's check (DB-TLS-ENV, branch `tools/dbtls-env-1`, 2026-11-14)

S2-2's database TLS check (`TlsCertificateLoginTests`, `docs/port-notes/S2-2.md` section 6) needs three things the script did not make: a
listener on an address of this computer that is not loopback, a server certificate that names it, and two client certificates of the
server's CA in `LocalMachine\My`, one with its key in a CAPI provider and one in a CNG provider. **The owner's decision (2026-11-14):** a
Windows virtual loopback network adapter (the Microsoft KM-TEST Loopback Adapter) with one private address from the documentation range
192.0.2.0/24, which no other computer can reach; the local MariaDB also listens on that address; nothing is exposed on the office network;
`-Revert` removes the adapter again. The owner runs the script elevated; the agents never do, and never install an adapter or change a
network setting, the firewall or the MariaDB service on this PC. **The one exception** to "only `127.0.0.1:3306`": the tests may connect
to that test address on port 3306 (`OPMS_DB_TLS_TEST_SERVER`), nothing else (`docs/decisions/owner-questions.md`, plan O-8 and O-12).

### 11.1 What `-WithTestNetwork <192.0.2.x>` adds to the enable run

The switch goes with the enable run (or `-TestCertificatesFolder`) only; `-Revert` and `-RequireSecureTransport` take the test network
from the record in `my.ini` and refuse the switch. An address that is not one host address of 192.0.2.0/24 (192.0.2.1 to 192.0.2.254,
without leading zeros) is refused before anything else, also before the elevation check. The checks (no elevation, nothing changed) then
refuse an address this computer has, another address of 192.0.2.0/24 on any interface, a route other than the default route that leads
into 192.0.2.0/24 (a VPN, for one), a server older than MariaDB 10.11, a missing `%windir%\INF\netloop.inf`, an adapter already named
`OPMS DB TLS test` or a firewall rule of this script that exists already, and the firewall off in a profile while an interface forwards
IPv4 or takes packets for other interfaces' addresses (weak host receive). The plan tags each change; `-Revert`'s plan names each tag again:

| Tag | The enable run | `-Revert` |
|---|---|---|
| `[adapter]` | A Microsoft KM-TEST Loopback Adapter (hardware id `*MSLOOP`), created through the SetupAPI from Windows' own `netloop.inf`, named `OPMS DB TLS test`. Loopback adapters that exist already are not touched. | First of all: removes the recorded device (only when its hardware id is `*MSLOOP`) and checks that it and the address are gone. |
| `[address]` | `192.0.2.x/32` and nothing else: no gateway, no DNS server, no DNS registration; DHCP, forwarding and weak host receive off (checked); duplicate address detection, weak host send, router discovery and NetBIOS over TCP/IP off (a `WARNING` where Windows keeps one). | Goes with the adapter. |
| `[bindings]` | Every binding but Internet Protocol Version 4 switched off; File and Printer Sharing (`ms_server`), Client for Microsoft Networks (`ms_msclient`) and IPv6 (`ms_tcpip6`) must be off (checked), any other gets a `WARNING` if Windows keeps it. | Go with the adapter. |
| `[firewall]` | When the firewall is on in any profile: `OpenPmsDental-DbTlsTest-Allow3306` (TCP 3306 on the address from the address itself, this computer) and `OpenPmsDental-DbTlsTest-Block3306` (from every other remote address), group `Open PMS - Dental local test TLS`, every profile. | Last: removes the recorded rules (only of that group) once the adapter and its address are gone; while the adapter stays (its removal failed), the rules stay too and are named as kept, so the address on which the server may still listen never lacks its block rule, and a later `-Revert` finds them in the record. |
| `[server-certificate]` | The server certificate also names the address (`IP:`) and the computer's host name (`DNS:`), beside `localhost` and `127.0.0.1`. | Goes with the `opms-dev-tls` folder. |
| `[client-certificates]` | `CN=opms-dbtls-test-client-capi` and `CN=opms-dbtls-test-client-cng` (RSA 2048, 5 years, clientAuth), signed by the same CA in the same run (its key is still discarded), each as its own `.pfx` (only the certificate and its key; a random password) in `opms-dev-tls\client-pfx` (Administrators only), imported with `certutil -csp ... -importPFX My <file> NoExport,NoChain,NoRoot` into `LocalMachine\My`: the first in `Microsoft Enhanced RSA and AES Cryptographic Provider` (CAPI), the second in `Microsoft Software Key Storage Provider` (CNG). The run checks the provider and the computer's key set, then deletes the `.pfx` files. The block records each key's provider, container and file. | Deletes each key (the CAPI container through the CSP, the CNG key with `CngKey.Delete`), removes the store entry and checks that both, and the key file, are gone. Only a certificate of that name that the test CA issued is removed. A certificate that is gone already (its store entry deleted by hand, in `certlm.msc` or with `certutil -delstore`, which leave the key file) does not end it: the recorded key is deleted when its file is still there, once its container opens as that very file. |
| `[key-access]` | `BUILTIN\Users` read access to both key files, as `ClientCertificateStore.GrantUsersReadToPrivateKey` gives it, so the lead's unelevated run can use them. | Goes with the keys: no key that the users can read is left behind (the owner's rule). |
| `[bind-address]` | The block ends with `bind-address=127.0.0.1,192.0.2.x`, after `my.ini`'s own `bind-address=127.0.0.1` (the last one wins). After the restart the server must report that bind address, the service's process must listen on `127.0.0.1:3306` and `192.0.2.x:3306` (`Get-NetTCPConnection`), `server-cert.pem` must name the address (an `IP` entry of its subject alternative names, read from the file), and `mariadb.exe` with `--ssl-ca --ssl-verify-server-cert` must not fail the TLS check at the address. The server refuses `root` there (`root@localhost`: 1130 before the TLS handshake, which the client wraps in `ERROR 2002 ... before completion of TLS handshake`; or 1045, access denied): that only gets a `NOTE`, since `TlsCertificateLoginTests` verifies the certificate at the address with its own X509 user; any other error there fails the run (section 11.8). | Restoring `my.ini` drops it; the run checks that nothing listens on the address any more. |

The block records what the run made, as comment lines after the folders' record (the adapter and the thumbprints are filled in as the run
makes them):

```ini
# test-address=192.0.2.25
# test-adapter=ROOT\NET\0001
# test-firewall-rule=OpenPmsDental-DbTlsTest-Allow3306
# test-firewall-rule=OpenPmsDental-DbTlsTest-Block3306
# test-client-capi=<40 hex digits>
# test-client-cng=<40 hex digits>
# test-client-capi-key=Microsoft Enhanced RSA and AES Cryptographic Provider|<container>|C:/ProgramData/Microsoft/Crypto/RSA/MachineKeys/<file>
# test-client-cng-key=Microsoft Software Key Storage Provider|<key name>|C:/ProgramData/Microsoft/Crypto/Keys/<file>
...
tls_version=TLSv1.2,TLSv1.3
bind-address=127.0.0.1,192.0.2.25
```

Every mode's checks then expect `bind-address=127.0.0.1,<the recorded address>` instead of `127.0.0.1` (in `my.ini` and from the server).
`-Revert` and `-RequireSecureTransport` refuse a record that no enable run can have written (an address outside 192.0.2.0/24, an adapter
that is not `ROOT\NET\nnnn`, another rule name, a value that is no thumbprint, a key that is missing, in another provider than this
script's of its kind, with a path in its container name, or whose file is not directly in `%ProgramData%\Microsoft\Crypto\RSA\MachineKeys`
(CAPI) or `%ProgramData%\Microsoft\Crypto\Keys` (CNG)), and `-Revert` also refuses, before any change, a recorded
adapter that is not a loopback adapter, a rule of another group, and a certificate of another name or not issued by the test CA.
`-RequireSecureTransport` keeps the record and checks the listener on the address after its restart. `-Revert` removes the test network
first, while the record in `my.ini` still names it: when a step fails, nothing else is changed (exit 4) and `-Revert` can run again. A failed
enable run (exit 3) removes the adapter, both certificates with their keys and the firewall rules again, beside the undo of section 2;
when that fails too (exit 4) the message names each of them.

**The journal.** The block goes into `my.ini` only after the whole test network is made, so the run also writes each record line to
`<MariaDB folder>\opms-dev-tls\test-network.record` (a protected ACL: Administrators and SYSTEM only) *before* the item it names is made:
the address before the adapter is created, the adapter's id right after its creation, each rule name before its rule, each thumbprint
before `certutil`, each key line right after the import and before `BUILTIN\Users` get read access to the key file. A failed run's undo
removes the test network in its own step, also when the restore of `my.ini` or the restart failed, and deletes the journal only when that
removal leaves nothing; while it does not, the `opms-dev-tls` folder (the journal and `ca.pem`) stays. When `my.ini`'s block records no
test network, `-Revert` reads the journal instead, refuses one with a line no run writes (as for the block, but items the run had not
made yet may be missing), checks what it names (`*MSLOOP`, the rule group, the certificates' names and the test CA of `ca.pem`), and
removes it before anything else: an interrupted run (Ctrl+C in a wait, a closed window, a crash: no backup, no block) or a failed run whose
undo restored `my.ini` and then could not restart the service or remove the test network (a backup, no block). Without a backup and a block
it leaves `my.ini` and the service alone and removes only what the stopped run made, so the enable run can start again. It removes the
`opms-dev-tls` folder only once nothing the journal names is there (else exit 4, nothing else changed). The journal stays as long as the
test network does; after a whole run it holds the same lines as the block.

**How the adapter is made without a download.** Windows 11 has `netloop.inf` in `%windir%\INF` and in the driver store
(`netloop.inf_amd64_*`), so no driver is needed. Neither of Windows' own command-line tools creates a device: `pnputil` stages and
installs drivers and enables, disables, restarts and removes devices (its commands on this PC: `/add-driver`, `/delete-driver`,
`/export-driver`, `/enum-*`, `/disable-device`, `/enable-device`, `/restart-device`, `/remove-device`, `/scan-devices`), and `devcon` is not
part of Windows. So the script does what `devcon install` does, through the SetupAPI by P/Invoke (`Add-Type`, the .NET Framework's own
compiler): `SetupDiCreateDeviceInfo` (class Net, `DICD_GENERATE_ID`: `ROOT\NET\nnnn`), the hardware id `*MSLOOP`,
`SetupDiBuildDriverInfoList` and `SetupDiSelectBestCompatDrv`, then `DIF_REGISTERDEVICE`, `DIF_REGISTER_COINSTALLERS`,
`DIF_INSTALLINTERFACES` and `DIF_INSTALLDEVICE` for that one device (not `UpdateDriverForPlugAndPlayDevices`, which would reinstall every
other `*MSLOOP` device too). A device registered before a later step fails is removed again (`DIF_REMOVE`); a device that asks for a
restart fails the run. Its removal opens the recorded device and removes it with `DIF_REMOVE` only when its hardware id is `*MSLOOP`.
No owner question was needed.

**MariaDB's list of bind addresses.** The installed 10.11.19's help text (in `bin\server.dll`): "IP address to bind to. Several addresses
may be specified, separated by a comma (,)." The MariaDB documentation (server system variables, `bind_address`) says the same from 10.11
on (from memory: no internet here). The script requires 10.11 or later and checks `@@bind_address` and both listeners after the restart.

**After a restart of the computer** the service starts with both addresses, so the adapter's address must be there by then. Duplicate
address detection is off on the adapter so that the address is usable at once. If the service ever does not start after a restart of the
computer, start it (`Start-Service MariaDB`) and tell the lead; `-Revert` takes the second address out again.

**`TlsCertificateLoginTests` fixed on the way.** It told a CAPI key from a CNG key by the type of `GetRSAPrivateKey()`, but .NET Framework
(the tests' target) opens a CAPI key through CNG too and gives an `RSACng` (checked here with an in-memory CAPI key), so the CAPI certificate
would have failed as "a CNG key". It now reads the certificate's key provider information without opening the key
(`CertificateKeyProvInfo`, provider type 0 for CNG), requires the computer's key set, and then opens the key as that kind
(`PrivateKey` as `RSACryptoServiceProvider` for CAPI). `DevDbTlsTestNetworkTests.TlsCertificateLoginTests_TellsACapiKeyFromACngKey_...`
checks it on synthetic key provider information.

### 11.2 The owner's run (elevated)

Before: tell the lead, so that no build-lane or test run uses the database during the restart. If TLS is already on through the script (an
earlier enable run without the switch), the dry run says `already on`: run `-Revert` first (section 4; a new CA follows, which the dev
tools find at the same path).

1. Open Windows PowerShell (the 64-bit one, not "Windows PowerShell (x86)": the script refuses that) with **Run as administrator**, from
   the account that runs the dev tools and tests, and go to a checkout of `main` with this package.
2. Look at the plan (changes nothing):

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -DryRun -WithClientCertificate -WithTestNetwork 192.0.2.25
   ```

   **Check:** no `REFUSED:` line; the plan shows the steps tagged `[adapter]`, `[address]`, `[bindings]`, `[firewall]` (or "the Windows
   firewall is off in every profile"), `[client-certificates]`, `[key-access]`, `[server-certificate]` and `[bind-address]`, and the block
   ends with `bind-address=127.0.0.1,192.0.2.25`. A refusal that the address is in use, or that a route leads into 192.0.2.0/24, means
   another address of 192.0.2.1 to 192.0.2.254 (or asking the lead).
3. Run it, and type `YES` when it asks:

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -WithClientCertificate -WithTestNetwork 192.0.2.25
   ```

   **Check:** it ends with `TLS is on: ...`, `The server certificate was verified against ...\ca.pem`, `The service listens on
   127.0.0.1:3306 and 192.0.2.25:3306 (adapter ROOT\NET\nnnn, 'OPMS DB TLS test')`, and three lines to paste:

   ```text
     $env:OPMS_DB_TLS_TEST_SERVER='192.0.2.25'
     $env:OPMS_DB_TLS_TEST_CAPI_THUMBPRINT='<40 hex digits>'
     $env:OPMS_DB_TLS_TEST_CNG_THUMBPRINT='<40 hex digits>'
   ```

   Send those three lines, and any `WARNING` line, to the lead. `FAILED` with `Undone:` (exit code 3) means nothing stayed changed, the
   test network included; `Undoing failed too` (exit code 4) names what is left. Then run `-DryRun -Revert` and `-Revert` in this
   elevated window (section 11.4): it also undoes the test network of a run that did not finish, from the journal when `my.ini` has no
   block. The same after a run that was interrupted (Ctrl+C, a closed window, a crash): the enable run refuses to start again while the
   `opms-dev-tls` folder is there, and `-Revert` removes what the stopped run made. By hand instead: the adapter first (Device Manager,
   Network adapters), then each test certificate (the message names the key file and the command that deletes the key, **before** the
   store entry), the firewall rules last:

   ```powershell
   certutil -csp "<provider>" -delkey "<container>"
   certutil -delstore My <thumbprint>
   ```

   and then check that the key file it named is gone (`Test-Path '<key file>'` is `False`). Not `certlm.msc`: its Delete, like
   `certutil -delstore` alone, removes only the store entry and leaves the key file, which `BUILTIN\Users` can read (the owner's rule).
   For a certificate whose key the run had not read yet the message says to take the provider and the container from
   `certutil -store My <thumbprint>` (`Provider =`, `Key Container =`). The run prints the values again whenever it is started later
   (`already on`), from the record in `my.ini`.
4. Optional checks (they change nothing): `Get-NetIPAddress -InterfaceAlias 'OPMS DB TLS test'` shows `192.0.2.25` with prefix 32 only;
   `Get-NetAdapterBinding -Name 'OPMS DB TLS test' | Where-Object Enabled` shows `ms_tcpip` (and only what a `WARNING` named);
   `Get-NetTCPConnection -LocalPort 3306 -State Listen` shows `127.0.0.1` and `192.0.2.25`, the same process;
   `Get-NetFirewallRule -Group 'Open PMS - Dental local test TLS'` shows the two rules; `certlm.msc`, Personal, Certificates, shows
   `opms-dbtls-test-client-capi` and `opms-dbtls-test-client-cng`.
5. After the next restart of the computer: `Get-Service MariaDB` is `Running` (see "After a restart of the computer" above).

### 11.3 The lead's test run (not elevated)

In a worktree with this package, after a build of `DbConnectionSecurityTests` and `NoHqTests`, in a PowerShell window that is not elevated,
of the account the owner ran the script for (the tests find the CA in its `%LOCALAPPDATA%`):

```powershell
$env:OPMS_DB_TLS_TEST_SERVER='192.0.2.25'
$env:OPMS_DB_TLS_TEST_CAPI_THUMBPRINT='<from the owner's run>'
$env:OPMS_DB_TLS_TEST_CNG_THUMBPRINT='<from the owner's run>'
powershell -NoProfile -ExecutionPolicy Bypass -File <common git folder>\od-tools\run-tests.ps1 -Root . -Mode Targeted -Include DbConnectionSecurityTests,NoHqTests -Out <file>
```

**Check:**
- `TlsCertificateLoginTests.NonLoopbackServer_VerifyFull_X509User_WithACapiKeyAndWithACngKey` **passed** (not skipped). A failure says
  which variable or step: a thumbprint not in `LocalMachine\My`, a key the account cannot read, the CAPI certificate's key in a CNG
  provider or the other way round, the session not encrypted, the user not the `REQUIRE X509` one, or the server not refusing the login
  without a certificate or with another CA.
- `WithoutTheVariables_TheCheckIsInconclusive_NeverAPass` is inconclusive (the variables are set), and the `DevDbTlsServerTests` pass now
  that the CA exists, `ClientCertificateForS22_ChainsToTheServersCa` included; `WhenTheServerRequiresTls_AConnectionWithoutTlsIsRefused`
  stays inconclusive until `-RequireSecureTransport` (section 5).
- The test left nothing behind: `SELECT User FROM mysql.user WHERE User LIKE 's22_x509_%'` and `SHOW DATABASES LIKE 's22_tls_%'` (as
  root on `127.0.0.1:3306`) are empty.
- The connections went to `127.0.0.1:3306` and `192.0.2.25:3306` only (the exception above). The test enforces it: before any
  connection, `ReadSetup` fails unless `OPMS_DB_TLS_TEST_SERVER` is an IPv4 address from 192.0.2.1 to 192.0.2.254 that this computer has
  (its own unicast addresses), on port 3306, with a message that names the owner's exception; a name, a LAN address or a 192.0.2.x that
  is not this computer's never gets a connection.

Then the stage 2 gate's database TLS check is done: note the run (date, counts, the result file) in `docs/port-notes/S2-2.md` section 6 and
the plan's gate. The variables live only in that window; the 3b gate runs the same steps again (the environment stays until `-Revert`).

### 11.4 The undo (elevated, the owner)

1. Look at the plan: `powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -DryRun -Revert`.
   **Check:** it starts with the test network, in the order of its removal: `[adapter]` (`ROOT\NET\nnnn`, first) with `[address]
   [bindings]`, `[client-certificates]` (each thumbprint, "its key in a CAPI provider" and "in a CNG provider", each with its recorded key
   file; for a certificate deleted by hand meanwhile, "but its key file ... is still there ...: delete that key"), `[key-access]`,
   `[firewall]` (the two rules, last, "If the adapter cannot be removed, the rules stay"), `[bind-address]`, `[server-certificate]`, then the
   restore of `my.ini`, the restart and the files of section 2. After a run that did not finish, the test network comes from the journal:
   "First the test network 192.0.2.25 of an enable run that did not finish, as its journal ...\test-network.record records it", with
   only the items that run had made ("The record names no loopback adapter (the run stopped before it made one)" and the like); without a
   backup the plan also says "an enable run stopped before it changed my.ini" and has no restore and no restart. A `REFUSED:` line means
   the record or the journal, or what it names, is not this script's (section 11.1); nothing is touched then.
2. Run `powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -Revert`, then `YES`.
   **Check:** `Removed of the test network 192.0.2.25: ...` (each item it removed; `Already gone before this run (not a problem): ...` for
   each item the record names that was not there any more) and `TLS is off again: ...` (after a run that stopped before it changed `my.ini`: `Removed what the enable run that did not finish
   had made; my.ini and the service were not touched.`). Optional: `Get-NetAdapter -Name 'OPMS DB TLS test'` and `Get-NetIPAddress -IPAddress
   192.0.2.25` find nothing, `Get-NetFirewallRule -Group 'Open PMS - Dental local test TLS'` finds nothing, `certlm.msc` shows no
   `opms-dbtls-test-client` certificate, the two key files the plan named are gone (`Test-Path`), the `opms-dev-tls` folder is gone, and
   `Get-NetTCPConnection -LocalPort 3306 -State Listen` shows `127.0.0.1` only.
   `FAILED: the test network is not fully removed` (exit code 4) changed nothing else: the record (the block, or the journal with the
   `opms-dev-tls` folder and `ca.pem`) stays. When the adapter could not be removed, the firewall rules are kept with it ("are kept,
   because the adapter and its address ... are still there"). Put right what it names (for the adapter: Device Manager, or a restart of the
   computer when Windows asks for one) and run `-Revert` again.

There is no switch that drops only the test network: `-Revert` and the enable run without `-WithTestNetwork` (a new CA) do that.

### 11.5 Files and checks run here

- Changed: `tools/Enable-LocalDbTls.ps1` (ASCII), `tests/DbConnectionSecurityTests/DevDbTlsScriptTests.cs` (its helpers shared; the
  driver loads the new settings; the elevation refusal also with the switch), `tests/DbConnectionSecurityTests/TlsCertificateLoginTests.cs`
  (section 11.1), this note, `docs/port-notes/S2-2.md` section 6, `docs/decisions/owner-questions.md`, `docs/specs/net10-plan.md` (O-8, O-12).
  `docs/release-notes/next.md` is not changed: a developer tool, nothing an office notices.
- New: `tests/DbConnectionSecurityTests/DevDbTlsTestNetworkTests.cs`, without elevation and without touching this PC's network, MariaDB
  service or a store: the certificate set of `-TestCertificatesFolder -WithTestNetwork` (the server's names; each `.pfx` read back with
  `EphemeralKeySet`: one certificate, its key, clientAuth only, the same CA, not readable with another password); the refusal of nine
  values that are no host address of 192.0.2.0/24 (`127.0.0.1`, the network and broadcast addresses, leading zeros, a name, IPv6) before
  the service is looked at, and of the switch with `-Revert` and `-RequireSecureTransport`; the
  address-in-use and network checks on synthetic addresses, routes and facts; the record in the block, read back, kept by
  `-RequireSecureTransport`'s block, and refused when no run can have written it; the state checks that keep `-Revert` to what the script
  made; the enable plan and `-Revert`'s plan with every tag (also with the firewall off and with everything gone); a real dry run that lists
  every step and changes nothing; the SetupAPI source compiled (never called) and no download in the script.
- Nothing here ran elevated, created an adapter, or changed a network setting, the firewall, the MariaDB service, `my.ini` or a
  certificate store; every database connection went to `127.0.0.1:3306`.
- Results: the full lane build has no error. Targeted runs, all passed: `DbConnectionSecurityTests` 237 (226 passed; 11 inconclusive by
  design until the owner's run: the nine `DevDbTlsServerTests`, `DataConnection_Loopback_WithTheLocalTestServersCa_VerifyCa` and
  `TlsCertificateLoginTests.NonLoopbackServer_VerifyFull_X509User_WithACapiKeyAndWithACngKey`, whose variables are not set here), with the
  eight new `DevDbTlsTestNetworkTests` and the updated `DevDbTlsScriptTests` passing; `NoHqTests` 57. `InitSqlTests` and
  `ForkMigrationTests` left out under the rule for every package (no schema change here).

### 11.6 The fix round after the review (branch `tools/dbtls-env-2`, 2026-11-14)

1. **A Users-readable key could stay behind.** (a) A failed enable run whose undo also failed (exit 4) said to remove the certificates
   "with their keys (certlm.msc)"; `certlm.msc`'s Delete, like `certutil -delstore`, removes only the store entry, and the CAPI container
   in `RSA\MachineKeys` or the CNG file in `Keys` stays, with the `BUILTIN\Users` read access the run gave it. (b) The record kept only the
   thumbprints, and the removal stopped as soon as the certificate was gone from `LocalMachine\My`, so after a store entry deleted by hand
   `-Revert` said "nothing to remove" and "no key that the users can read is left behind" while the key file was still there.
   **Fixed:** the block records each key as `# test-client-capi-key=` / `# test-client-cng-key=` `<provider>|<container>|<key file>` from
   the import (section 11.1); the later modes refuse a key record that this script cannot have written (another provider, a container name
   with a path in it, a file not directly in the computer's key folder of its kind, or none). `-Revert` reads whether each recorded key
   file is there; its plan says, for a certificate that is gone with its key file left, "delete that key (the container ... in '...', once
   it opens as that very file) and check that the file is gone", and the removal does it (`Remove-TestKey`: CAPI through the CSP with
   `UseMachineKeyStore|UseExistingKey`, `PersistKeyInCsp=false`, `Clear()`; CNG with `CngKey.Open(..., MachineKey).Delete()`; nothing is
   deleted when the container's unique name is not the recorded file's name) and reports a key file that stays. A failed enable run's own
   undo knows each key too, and its exit-4 message names each key file with `certutil -csp "<provider>" -delkey "<container>"` before
   `certutil -delstore My <thumbprint>` (section 11.2, step 3).
2. **The tests could connect elsewhere.** `TlsCertificateLoginTests.ReadSetup` checked only that the server was not loopback and the port
   3306, so a variable set by mistake to an office server's name or a LAN address would have opened `VerifyFull` connections there with the
   computer's client certificates. **Fixed:** before any connection (and before it reads the CA variable) it fails unless the host is an
   IPv4 address from 192.0.2.1 to 192.0.2.254 in its plain form and one of this computer's own unicast addresses
   (`NetworkInterface.GetAllNetworkInterfaces()`), with a message that names the owner's exception; no name is looked up (section 11.3).

Tests: `DevDbTlsTestNetworkTests.TestNetwork_ACertificateDeletedByHand_LeavesItsKeyFile_RevertsPlanAndRemovalDeleteTheRecordedKey` (a
record whose certificates are in no store, one key file present in the test's temporary folder: `-Revert`'s state and plan delete that key;
the removal, with `Remove-TestKey` replaced by one that only deletes that file, deletes it and reports a file that stays; the exit-4 text);
the record test with the key lines and eight refused key records; the plan test with each recorded key file; the dry run's key lines;
`TlsCertificateLoginTests.TheServerVariable_OnlyTheOneLocalTestAddress_AnythingElseFailsBeforeAnyConnection` (synthetic variables and
addresses of "this computer": `10.0.0.5` (also when this computer has it), two host names, `192.0.2.99` that is not local, the network and
broadcast addresses, a leading zero, the one-number form, another documentation range; each refused before the CA is read; the local
`192.0.2.25` passes). Nothing ran elevated, opened or deleted a real key, or connected anywhere but `127.0.0.1:3306`.

Results: the full lane build has no error. Targeted runs, all passed: `DbConnectionSecurityTests` 239 (228 passed; the same 11
inconclusive by design until the owner's run as in section 11.5), with the two new tests passing; `NoHqTests` 57 passed. `InitSqlTests`
and `ForkMigrationTests` left out under the rule for every package (no schema change here).

### 11.7 The second fix round (branch `tools/dbtls-env-3`, 2026-11-14)

1. **`-Revert` could not find the test network of a run that did not finish.** Its only record was the block in `my.ini`, written after
   the whole test network was made. (a) When a failed run's undo copied the backup over `my.ini` and then the restart threw (exit 4), the
   test network was never undone, `my.ini` was the old file again (no record) and the backup stayed: a later `-Revert` restored, restarted,
   deleted the `opms-dev-tls` folder with `ca.pem` and said "TLS is off again" (exit 0), leaving the adapter, the rules and both keys that
   `BUILTIN\Users` can read, which could then no longer be checked against the test CA. (b) An interrupted run (Ctrl+C in the waits of
   up to 60 and 20 seconds, a closed window, a crash) skipped the undo and left all of it with no backup and no block: `-Revert` refused
   ("remove the block between the marker lines by hand", but there was none), and the enable run refused because the folder was there.
   **Fixed:** the journal of section 11.1, written before each item is made; the failed run's undo (`Undo-FailedEnable`) removes the test
   network in its own step whatever happened to `my.ini` and the restart, and keeps the journal and the folder while that removal is not
   complete; `-Revert` reads the journal when the block records no test network, checks it and what it names, and removes it before the
   "backup is missing" refusal and before the folder; with no backup and no block it removes only what the stopped run made. The exit-4
   text says that `-Revert` also undoes an interrupted run's test network (section 11.2, step 3; section 11.4).
2. **The block rule went before the adapter.** `Undo-TestNetwork` removed the firewall rules first and the adapter last, while `-Revert`
   restores `my.ini` and restarts only afterwards, so the server still listened on `192.0.2.x:3306` without its block rule until the
   adapter was gone, and for good when the adapter's removal failed (exit 4). With the firewall on, the enable run does not check for
   interfaces that forward IPv4 or receive for other interfaces' addresses (it relies on the block rule), so on a computer with Internet
   Connection Sharing or a Hyper-V or WSL switch the address could have been reached from beyond it. **Fixed:** the adapter first
   (`Remove-TestAdapter` waits until the address is gone), then the certificates with their keys, the rules last and only when the adapter
   step succeeded; otherwise they are kept and named, and the record that names them stays. `-Revert`'s plan follows that order
   (section 11.1 table, section 11.4).
3. **Found on the way: every `-Revert` with a test network would have stopped with exit 4.** `Undo-TestNetwork` returns its list with
   `return , $problems`, and the callers wrapped the call in `@()`: in Windows PowerShell 5.1 that gives one element, the list itself (an
   empty list counts 1; checked here with a two-line script), so a removal that left nothing still printed "the test network is not fully
   removed" and kept the record, run after run; a failed enable run's undo would always have said "Undoing failed too". The callers now
   assign first and wrap after (also for the new `Get-TestNetworkLeft`). The new driver tests run the real `Undo-TestNetwork` through
   `Invoke-Revert` and `Undo-FailedEnable`, so this is now covered.

Tests (`DevDbTlsTestNetworkTests`, no elevation; every step that would change this computer replaced by a function that only notes its
call, all files in the test's temporary folder):
`TestNetwork_TheJournal_RecordsEachItemBeforeItIsMade_AndReadsBackForRevert` (`Install-TestNetwork` with the adapter's creation replaced
by a type of the same name that reads the journal: the address before the creation, the id right after, each rule before
`New-NetFirewallRule`, each thumbprint before the import, each key before the users' read access, also with a crash there; read back as a
record `-Revert` accepts; a journal of a run that stopped after the address gives a plan with every tag and nothing to remove; a journal
line no run writes, and a key without its thumbprint, refused);
`TestNetwork_Revert_AJournalWithoutABlock_RemovesTheAdapterTheRulesAndTheKeys_AlsoWithoutABackup` (`Invoke-Revert` on a scratch `my.ini`
without a block: without a backup no "backup is missing" refusal, no restore and no restart; with a backup the restore after the removal;
the plan names the adapter, both rules and both keys with every tag; the adapter, the certificates and the rules removed in that order;
an item the journal names that is still there ends it with exit 4, the folder, the journal and the backup kept);
`TestNetwork_UndoTestNetwork_TheAdapterFirst_TheRulesLast_AndKeptWhileTheAdapterStays` (with `Remove-TestAdapter` throwing,
`Remove-TestFirewallRule` is not called and the rules are in the list as kept);
`TestNetwork_AFailedRunsUndo_UndoesTheTestNetwork_AlsoWhenTheRestartFails_AndKeepsTheJournalWhileItIsNotUndone`; the plan test checks the
new order and the journal line of the enable plan.

Results: the full lane build has no error. Targeted runs, all passed: `DbConnectionSecurityTests` 243 (232 passed; the same 11
inconclusive by design until the owner's run as in section 11.5), with the four new tests passing; `NoHqTests` 57 passed. `InitSqlTests`
and `ForkMigrationTests` left out under the rule for every package (no schema change here). Nothing ran elevated or changed a network
setting, the firewall, the MariaDB service, `my.ini` or a certificate store.

### 11.8 The owner's first run (2026-11-14; branch `tools/dbtls-env-fix-1`)

**What happened.** The owner ran `tools\Enable-LocalDbTls.ps1 -WithClientCertificate -WithTestNetwork 192.0.2.25` elevated on `main`
9bf1de770. It created the loopback adapter `ROOT\NET\0000`, gave it `192.0.2.25/32`, added both firewall rules, imported both test
certificates, wrote `opms-dev-tls`, `ca.pem`, `DevDbTlsClient` and the block of `my.ini`, restarted MariaDB, and then stopped with exit 4:

```text
FAILED: The client could not verify the server certificate at 192.0.2.25: ERROR 2002 (HY000): Received error packet before completion
of TLS handshake. The authenticity of the following error cannot be verified: 1130 - Host '192.0.2.25' is not allowed to connect to
this MariaDB server
Undoing failed too: the test network could not be undone: the loopback adapter ROOT\NET\0000: The property 'Count' cannot be found on
this object. Verify that the property exists.; the firewall rules ... are kept, because the adapter and its address 192.0.2.25 are still
there ...; run -Revert again once the adapter is gone
```

Its undo restored `my.ini` (no block; the backup removed), restarted the service, removed the adapter (with its address) and both
certificates with their key files, and removed the files of the dev and client folders; it kept the two firewall rules and the
`opms-dev-tls` folder with the journal and `ca.pem`, as designed when the adapter step reports a failure. The lead read the PC afterwards
(unelevated): exactly that, MariaDB listening on `127.0.0.1:3306` only.

**Cause 1: the check at the test address.** It took every client error that matched `SSL|TLS|certificate` for a certificate failure.
`root` is `root@localhost`, so MariaDB refuses the host `192.0.2.25` with 1130, and it does so before the TLS handshake; the client
wraps that in `ERROR 2002 ... before completion of TLS handshake ...`, which matched. Section 11.1 says that a host refusal or `Access
denied` there only gets a note. **Fixed:** `Get-TestAddressCheck` tells `verified` (exit 0), `login-refused` (1130 or 1045, also
inside the 2002 text), `certificate` (any other error that names SSL, TLS or a certificate, also another error inside the 2002 text:
fatal, exit 3 after the undo) and `failed` (anything else there, such as a lost connection: fatal too, where the old check let it pass).
A refusal prints `NOTE: at 192.0.2.25 the server refused the check's login as root (...)`, that the client could not check the
certificate there, and that `TlsCertificateLoginTests` verifies the certificate at the address with its own X509 user. Since the client
then checks nothing at the address, the run first checks from the file that `server-cert.pem` names the address
(`Get-ServerCertificateAddressProblem`: the `IP` entries of the subject alternative names, read from the extension's DER, because the
.NET Framework has no class for it and `Format()` depends on the language); a certificate without it fails the run.

**Cause 2: the undo's adapter removal.** `Remove-TestAdapter([string]$InstanceId, [string]$Address)` waited with
`$address = @(Get-NetIPAddress ... | Where-Object { [string]$_.IPAddress -eq $Address })`. PowerShell's variable names ignore case, so
that assigned to the `[string]` parameter `$Address`: the list became a string (the empty list `''`), and `$address.Count` threw under
`Set-StrictMode -Version Latest` as soon as the device was gone (while it was there, `-and` never read `$address.Count`). The removal
itself had worked; the error came from the line after it, which is why `@(Get-PnpDevice -InstanceId <missing>).Count` gave 0 in the
lead's check. The driver tests had not caught it: they ran without strict mode and replaced `Remove-TestAdapter`. **Fixed:** no local
variable shares a parameter's name (checked over the whole script from its syntax tree; this was the only one), and the removal no
longer asks `Get-PnpDevice` whether the device is there: `LoopbackAdapter.Exists(instanceId)` (new, SetupAPI `SetupDiOpenDeviceInfo` on
a device information set of any class; false only for `ERROR_NO_SUCH_DEVINST`, any other error throws; it reads only and needs no
elevation) through `Test-TestAdapterExists`, and `Test-HasAddress` for the address (`-ErrorAction Stop`: a failed query is no "gone").
While it waits, a check that throws counts as "not known yet" and is asked again; at the deadline (30 seconds) the last answers decide
and the message names what is still there or could not be checked. `Get-TestNetworkState` (`-Revert`'s view) uses the same check and
`LoopbackAdapter.GetHardwareIds` (now also of any class) instead of `Get-PnpDevice`, and also reads whether the address is there. Every
problem text of the undo, of `-Revert` and of `-RequireSecureTransport`'s undo, the enable run's `FAILED:` line and the top-level one
carry `$_.InvocationInfo.PositionMessage` on one line (`Get-ErrorText`: `<message> [At <script>:<line> char:<n> + <the line>]`), so a next
failure names its line.

Every expression of the undo and adapter paths (`Remove-TestAdapter`, `Undo-TestNetwork`, `Undo-FailedEnable`, `Set-TestAdapter`,
`Wait-TestNetAdapter`, `Get-TestAdapterProblem`, `Get-TestNetworkState`, `Invoke-Revert`, the C# class and what PowerShell makes of its
return values) was read for a `.Count` or another property on something that can be one object or `$null`. Found besides:
`(Get-DnsClientServerAddress ...).ServerAddresses` and `(Get-DnsClient ...).RegisterThisConnectionsAddress` in `Get-TestAdapterProblem`
would throw under strict mode when the cmdlet gives nothing (checked here: `(<a command that returns nothing>).Name` throws); both now go
through `@()` and `ForEach-Object`/`Where-Object`. `Get-TestAdapterHardwareIds` returns its ids one by one, not `, @(...)` (the trap of
section 11.7, item 3). Everything else already wraps in `@()` or reads a `[pscustomobject]` that always has the property.

**`-Revert` on what the run left.** With no block, no backup and the `opms-dev-tls` folder there, `-Revert` takes the journal: the device,
the address, both certificates and both key files are gone and are named so in the plan ("is not there any more: nothing to remove", "Its
address 192.0.2.25/32 is not on this computer any more", "... and its key file ... is gone too: nothing to remove"), the two rules are
removed, then the folder; it reports `Removed of the test network 192.0.2.25: the firewall rule ...Allow3306, the firewall rule
...Block3306.` and `Already gone before this run (not a problem): ...` (`Get-TestNetworkRemovedLines`, from the state before the
removal), not problems. Its plan says that such a folder is also left by an undo that restored `my.ini` and could not remove the whole
test network. A `-DryRun -Revert` by an account that cannot read the folder (Administrators only) is refused, instead of a plan without
the journal's test network.

**Also (the review's small notes).** The 32-bit Windows PowerShell on 64-bit Windows is refused first, before any other check, in every
mode (`Get-ProcessBitnessProblem`): the SetupAPI's device installation fails in WOW64, and the key folders and the registry would be the
32-bit view. `Remove-TestClientCertificate` removes the store entry also when its key no longer opens (its key file deleted by hand) and
the key is known (the record, or the run's own undo): the known key is deleted first when its file is still there; without a known key
the entry stays and the error says to read the provider and container from `certutil -store My <thumbprint>` (the exit-4 text of section
11.2 says the same). An enable run that finds the block already says when it ignores `-WithTestNetwork` (`Get-IgnoredTestNetworkNote`:
no test network recorded, the same address, or another one).

**The owner's steps now** (Windows PowerShell, the 64-bit one, with **Run as administrator**, from the account that runs the dev tools and
tests, in a checkout of `main` with this fix; tell the lead first, because step 3 restarts MariaDB):

1. Look at the plan (changes nothing):
   `powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -DryRun -Revert`
   **Check:** no `REFUSED:`; "an enable run stopped before it changed my.ini ..., or its undo restored my.ini and removed the backup";
   "First the test network 192.0.2.25 of an enable run that did not finish, as its journal C:\Program Files\MariaDB
   10.11\opms-dev-tls\test-network.record records it"; `[adapter] The loopback adapter ROOT\NET\0000 is not there any more: nothing to
   remove.`; "Its address 192.0.2.25/32 is not on this computer any more"; for both certificates "... is not in LocalMachine\My any
   more, and its key file ... is gone too: nothing to remove."; `[firewall] Remove the firewall rules OpenPmsDental-DbTlsTest-Allow3306
   and OpenPmsDental-DbTlsTest-Block3306 ...`; then `Remove C:\Program Files\MariaDB 10.11\opms-dev-tls, ...\DevDbTls\ca.pem;`; no
   restore of `my.ini` and no restart; `Dry run: nothing was changed.`
2. Run it: `powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -Revert`, then `YES`.
   **Check:** `Removed of the test network 192.0.2.25: the firewall rule OpenPmsDental-DbTlsTest-Allow3306, the firewall rule
   OpenPmsDental-DbTlsTest-Block3306.`, `Already gone before this run (not a problem): the loopback adapter ROOT\NET\0000 and its address
   192.0.2.25, the CAPI certificate ... and its key file ..., the CNG certificate ... and its key file ....` and `Removed what the enable
   run that did not finish had made; my.ini and the service were not touched. The enable run can start again.` (exit code 0). Optional:
   `Get-NetFirewallRule -Group 'Open PMS - Dental local test TLS'` finds nothing, and `Test-Path 'C:\Program Files\MariaDB
   10.11\opms-dev-tls'` is `False`.
3. The enable run again, as in section 11.2 steps 2 and 3: first
   `powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -DryRun -WithClientCertificate -WithTestNetwork 192.0.2.25`,
   then the same without `-DryRun`, and `YES`. **Check:** it ends with `TLS is on: ...`, `The server certificate was verified against
   ...\ca.pem`, `The service listens on 127.0.0.1:3306 and 192.0.2.25:3306 (adapter ROOT\NET\nnnn, 'OPMS DB TLS test')`, the expected
   `NOTE: at 192.0.2.25 the server refused the check's login as root (ERROR 2002 ... 1130 ...)` with "the certificate at the address is
   verified by TlsCertificateLoginTests with its own X509 user", and the three `$env:OPMS_DB_TLS_TEST_*` lines. Send those three lines,
   and any `WARNING` line, to the lead (section 11.3). If it fails again, send the whole output: each problem now names its script line.

Tests (`DevDbTlsTestNetworkTests`, no elevation; every step that would change this computer replaced, all files in the test's temporary
folder). The drivers now run under `Set-StrictMode -Version Latest`, as the script does, and the tests start the 64-bit Windows
PowerShell (vstest runs them as x86, whose `powershell.exe` is the 32-bit one, which the script now refuses):
`TestNetwork_OwnersRun_TheServersRefusalOfRootAtTheTestAddress_IsANote_ACertificateFailureStaysFatal` (`Invoke-Enable` from its checks
to its end with real certificates, the client's check ending with the owner's exact error: a `NOTE`, exit 0 and the test's values; a TLS
error and a lost connection: exit 3; the classification of eight client texts; the SAN read from a real server certificate, with and
without the address);
`TestNetwork_OwnersRun_RemoveTestAdapter_UnderStrictMode_TheWaitAsksAgainAfterAnError_AndNoListBecomesAString` (the owner's case, a device
gone already, a check that throws twice, a device that stays, a check that keeps throwing; and the syntax-tree scan of every function for
a local named like a parameter);
`TestNetwork_OwnersPcAfterTheFailedRun_RevertRemovesTheRulesAndTheFolder_AndReportsTheRestAsGone` (this PC's state exactly: the journal
with the address, `ROOT\NET\0000`, both rules, both thumbprints and keys; no device, no address, no certificate, no key file; the rules
there; `my.ini` without block or backup: `-DryRun -Revert`'s plan, and `-Revert` removing the two rules and the folder and reporting the
rest as gone);
`TestNetwork_ACertificateWhoseKeyFileWasDeletedByHand_LosesItsStoreEntryToo_WhenTheKeyIsKnown`;
`TestNetwork_The32BitPowerShell_IsRefusedFirst_AndASecondEnableRunSaysItIgnoresTheAddress` (also the real script in the 32-bit
PowerShell: refused before the service is looked at). `TestNetwork_TheAdaptersSetupApiSource_Compiles_AndNothingIsDownloaded` now also
calls the read-only `Exists` (a device id no computer has: false; `HTREE\ROOT\0`: true). The enable run's dry-run tests are inconclusive
while the `opms-dev-tls` folder of the owner's run is there (the enable run refuses then, by design).

Results: the full lane build has no error. Against `main`'s script (9bf1de770, put into the worktree for one Targeted run and taken out
again; `<common git folder>\od-tools\test-results\dbtls-env-fix-1-against-main-9bf1de770.txt`) the five new tests fail as the owner's
run did: the enable run at the test address ends with exit 3 instead of 0, `Remove-TestAdapter` throws "The property 'Count' cannot be
found on this object" in the owner's case (and in a case that waits, "the address  is still there": the parameter had become empty),
`-Revert` on this PC's state ends with exit 4, "the loopback adapter ROOT\NET\0000: The property 'Count' cannot be found on this object",
the store entry whose key file was deleted by hand is not removed, and the 32-bit PowerShell is not refused; four existing tests fail
there on their new texts. With the fix (`...\test-results\dbtls-env-fix-1.txt`), Targeted runs, all passed: `DbConnectionSecurityTests`
248 (235 passed; 13 inconclusive: the 11 by design until the owner's run as in section 11.5, and the enable run's two dry-run tests
while the owner's `opms-dev-tls` folder is there), with the five new tests passing; `NoHqTests` 57 passed. `InitSqlTests` and
`ForkMigrationTests` left out under the rule for every package (no schema change here). Nothing ran elevated or changed a network
setting, the firewall, the MariaDB service, `my.ini` or a certificate store; the new test's real run of the script was a `-DryRun` in
the 32-bit PowerShell, refused before the service was looked at.

### 11.9 The owner's second run and the lead's test run (2026-11-14)

- **A stale type in the elevated window.** The owner's first `-Revert` after the fix ran in the window of the failed first run, which
  had loaded the older `OpenPmsDentalDevDbTls.LoopbackAdapter` (`Add-Type` cannot replace a loaded type), and stopped with
  "does not contain a method named 'Exists'" before its plan, changing nothing. Run in a new process (`powershell -NoProfile
  -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 ...`) it worked. `Add-LoopbackAdapterType` now refuses such a session
  with that instruction instead of failing later.
- **`-Revert`** (new process) removed the two firewall rules and `opms-dev-tls`, reported the adapter, the address, both certificates
  and their key files as already gone, and left `my.ini` and the service untouched (exit 0).
- **The enable run** with `-WithClientCertificate -WithTestNetwork 192.0.2.25`: the adapter `ROOT\NET\0000` with 192.0.2.25/32, the
  two rules, both client certificates (CAPI and CNG, Users read), TLSv1.3 on, the server certificate verified at 127.0.0.1, the service
  listening on 127.0.0.1:3306 and 192.0.2.25:3306, and the expected NOTE that the server refused root at 192.0.2.25 (1130) with the
  SAN of `server-cert.pem` checked instead. It printed the three `OPMS_DB_TLS_TEST_*` values.
- **The lead's run** of DbConnectionSecurityTests (not elevated) with those values: `NonLoopbackServer_VerifyFull_X509User_WithACapiKeyAndWithACngKey`
  passed, so the program's own connection string opens a `VerifyFull` session at the non-loopback address as a `REQUIRE X509` user with
  a CAPI key and with a CNG key from `LocalMachine\My` (S2-2 section 6; the 3b gate's database TLS check). One older test,
  `TestHelpersConnection_DevDbTlsApply_IsEncrypted_TheServerHasTls12And13Only`, expected `bind_address` to be `127.0.0.1` exactly; it
  now accepts `127.0.0.1` with at most the test network's one 192.0.2.x address after it. Then: 248 total, 245 passed, 0 failed, 3
  skipped (`WhenTheServerRequiresTls_...` until `-RequireSecureTransport`, the enable dry run while TLS is on, and the no-variables
  check while the variables are set).
- **Still to do:** `-RequireSecureTransport` (the owner, elevated) when the other sessions' dev tools use TLS; `-Revert` removes the test
  network whenever the owner wants it gone.
