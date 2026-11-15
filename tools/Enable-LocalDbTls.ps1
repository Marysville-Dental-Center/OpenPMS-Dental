<#
.SYNOPSIS
Turns on TLS in the local MariaDB service on 127.0.0.1:3306, the development and test database server (O-12).

.DESCRIPTION
Run once, as administrator, on the development PC (docs/port-notes/DB-TLS.md). The script changes nothing before it has shown what it
will change and you have typed YES.

Without a mode switch (enable):
  1. Checks: the session is elevated; the service (-ServiceName, default MariaDB) runs mysqld.exe or mariadbd.exe with
     --defaults-file=<my.ini>; that my.ini is UTF-8 (or ASCII) text that includes no other option file (!include, !includedir) and
     says port 3306 (or no port) and bind-address=127.0.0.1; the process that listens on 127.0.0.1:3306 is the service's own (and no
     other process listens on port 3306 for every address); the server that answers there reports that port, that bind address and
     that my.ini's datadir; my.ini has no TLS line yet.
  2. Makes, in memory, a test CA (RSA 3072, 10 years) and a server certificate for localhost and 127.0.0.1 (RSA 2048, 5 years,
     serverAuth). Only with -WithClientCertificate also a client certificate for the certificate-login test of S2-2 (RSA 2048, 5 years,
     clientAuth); without it (the default, until the owner has agreed to it) none. The CA's private key is never written anywhere, so
     nothing else can ever be signed with it: a client certificate can only be made in the same run as the CA.
  3. Writes ca.pem, server-cert.pem and server-key.pem to <MariaDB folder>\opms-dev-tls (next to bin and data, not inside data, where
     MariaDB would take the folder for a database), with a protected ACL: SYSTEM and Administrators full control, the service account
     read, nobody else.
  4. Writes the CA certificate (public part only) as ca.pem to -DevFolder (default %LOCALAPPDATA%\OpenPmsDental\DevDbTls), where the dev
     tools and tests look (DataConnectionBase.DevDbTls). The folder is created when missing and gets a protected ACL: -DevUser, SYSTEM
     and Administrators full control, nobody else. That folder never holds a private key.
  5. With -WithClientCertificate: writes client-cert.pem and client-key.pem to -ClientFolder (default DevDbTlsClient beside -DevFolder,
     %LOCALAPPDATA%\OpenPmsDental\DevDbTlsClient), a folder of its own, never -DevFolder or inside it, created when missing, with a
     protected ACL: -DevUser read, SYSTEM and Administrators full control. Nothing is written to the repository.
     Each folder must be <a user profile>\AppData\Local\OpenPmsDental\DevDbTls (DevDbTlsClient for the client folder) of a profile of
     this computer, the only folders -Revert and -RequireSecureTransport accept from the record in my.ini; it must not be or overlap
     the MariaDB folder, the server's data folder or the opms-dev-tls folder; and when it exists it holds only files of this script.
  6. Copies my.ini to my.ini.before-opms-dev-tls and adds, at the end of its [mysqld] section between marker comments, comment lines that
     record -DevFolder (and -ClientFolder, and a parent folder that the run creates for them) for -Revert and -RequireSecureTransport,
     then ssl_ca, ssl_cert, ssl_key and tls_version=TLSv1.2,TLSv1.3.
  7. Restarts the service and verifies a TLS session (have_ssl=YES, Ssl_cipher not empty) and the server certificate against the CA
     (--ssl-ca with --ssl-verify-server-cert). When the server does not come back with TLS, the backup is restored, the service restarted,
     the files the run replaced put back as they were, the files and folders it added removed.
  It does NOT set require_secure_transport: unencrypted connections still work until -RequireSecureTransport.

With -WithTestNetwork <192.0.2.x> (DB-TLS-ENV, the owner's decision of 2026-11-14; docs/port-notes/DB-TLS.md section 11) the enable
run also makes the environment of S2-2's database TLS check (TlsCertificateLoginTests): an address of this computer that is not loopback
and that no other computer can reach. The address must be one host address of 192.0.2.0/24 (TEST-NET-1, RFC 5737, which no network
routes) that this computer does not use, in a range no route of this computer leads to.
  a. A Microsoft KM-TEST Loopback Adapter (hardware id *MSLOOP, Windows' own netloop.inf from the driver store), created through the
     SetupAPI (P/Invoke, no download, no devcon) and named 'OPMS DB TLS test'. Loopback adapters that exist already are not touched.
  b. Its IPv4 address with a /32 prefix and nothing else: no gateway, no DNS server, no DNS registration, NetBIOS over TCP/IP off, DHCP,
     duplicate address detection, forwarding, weak host send and receive and router discovery off, and every binding but Internet
     Protocol Version 4 switched off (File and Printer Sharing, Client for Microsoft Networks, IPv6 and the rest).
  c. When the Windows firewall is on in any profile: two inbound rules for TCP port 3306 on that address only, one that allows it from
     the address itself (this computer) and one that blocks every other remote address.
  d. The server certificate also names the address and the computer's host name.
  e. Two client certificates (RSA 2048, 5 years, clientAuth) signed by the same CA in the same run, each as its own .pfx (in an
     Administrators-only folder, deleted after its import), imported with certutil into LocalMachine\My, not exportable: one key in the
     CAPI provider 'Microsoft Enhanced RSA and AES Cryptographic Provider', one in the 'Microsoft Software Key Storage Provider'; BUILTIN\Users
     get read access to both key files, as the connection settings tool gives (ClientCertificateStore).
  f. bind-address=127.0.0.1,<address> in the block of my.ini (MariaDB 10.11 takes a comma-separated list; 127.0.0.1 stays); after the
     restart the service's process must listen on 127.0.0.1:3306 and <address>:3306 (Get-NetTCPConnection).
  The block records the address, the adapter, the firewall rules and the two certificates; the run prints the three OPMS_DB_TLS_TEST_*
  values of the test. -Revert undoes every step: the firewall rules, both certificates with their keys (never a key that BUILTIN\Users can
  read is left behind), the adapter with its address, and my.ini with the files as before.

-RequireSecureTransport: the lead's later step, once every session connects with TLS. Checks the CA copy in the dev folder that my.ini
records, adds require_secure_transport=ON inside the script's block, restarts, and verifies that a TLS connection works and an
unencrypted one is refused (else it removes the line again).

-Revert: restores my.ini from the backup (it shows what would be lost when my.ini changed since), restarts, verifies an unencrypted
connection, then removes the opms-dev-tls folder, the files the script wrote to the dev folder and the client folder that my.ini
records (whichever account's they are), each folder when it is then empty, the parent folder the enable run created when it is then
empty, and the backup.

-RequireSecureTransport and -Revert never recompute the folders from the account that runs them: they take them from the record in
my.ini and refuse a -DevFolder or -ClientFolder that names another folder, and a recorded folder that an enable run cannot have written
(not <a user profile>\AppData\Local\OpenPmsDental\DevDbTls or \DevDbTlsClient). Only when my.ini has no block of this script do they
fall back to -DevFolder or this account's default (the plan then says that a copy in another account's folder is not touched).

-DryRun: with any mode, shows the checks and the plan and changes nothing. It does not need elevation.

-TestCertificatesFolder <folder>: writes only the certificate set of step 2 (the CA certificate, the server certificate and key, and
with -WithClientCertificate the client certificate and key; with -WithTestNetwork the server certificate's extra names and the two test
client .pfx files with their password in a text file) to an empty folder outside the repository, and nothing else; no elevation, no
service, no network setting read or changed. The tests use it to check the certificates offline
(tests/DbConnectionSecurityTests/DevDbTlsScriptTests.cs).

The client is the MariaDB service's own mariadb.exe (or mysql.exe), always on 127.0.0.1:3306 over TCP, as -User (default root); the
password comes from the MYSQL_PWD environment variable when it is set, otherwise none (the dev server's root has a blank password).

Exit codes: 0 done, nothing to do, or a dry run; 1 a check refused or a value is wrong; 2 you did not type YES; 3 the change failed and
was undone; 4 the change failed and could not be undone (the message says what to do).

.PARAMETER RequireSecureTransport
Adds require_secure_transport=ON (the lead's step after every session connects with TLS).

.PARAMETER Revert
Undoes everything the script did: restores the backup of my.ini, restarts, removes the certificate files (in the folders my.ini records).

.PARAMETER DryRun
Shows the checks and the plan of the chosen mode and changes nothing.

.PARAMETER TestCertificatesFolder
Writes a certificate set to this empty folder and does nothing else (for the tests).

.PARAMETER ServiceName
The MariaDB service. Default: MariaDB.

.PARAMETER DevFolder
Where the CA certificate (public part only) goes for the dev tools and tests; never a private key.
Default: %LOCALAPPDATA%\OpenPmsDental\DevDbTls of the account that runs the script; for another account its own
<profile>\AppData\Local\OpenPmsDental\DevDbTls (no other folder is accepted). With -RequireSecureTransport and -Revert it must be the
folder my.ini records (leave it out).

.PARAMETER DevUser
The account that runs the dev tools and tests and gets access to -DevFolder (and read access to -ClientFolder). Default: the account that
runs the script.

.PARAMETER User
The database account for the checks. Default: root.

.PARAMETER WithClientCertificate
Also makes the client certificate for S2-2's certificate-login test and writes it with its key to -ClientFolder. Off by default: only
once the owner has agreed to a client certificate. Later it means -Revert and this script again (a new CA), because the CA's key is gone.

.PARAMETER ClientFolder
With -WithClientCertificate: where client-cert.pem and client-key.pem go, a folder of its own (never -DevFolder or inside it).
Default: DevDbTlsClient beside -DevFolder (%LOCALAPPDATA%\OpenPmsDental\DevDbTlsClient); it must be a profile's
<profile>\AppData\Local\OpenPmsDental\DevDbTlsClient. With -RequireSecureTransport and -Revert it must be the folder my.ini records
(leave it out).

.PARAMETER WithTestNetwork
With the enable run (or -TestCertificatesFolder): the test address, one host address of 192.0.2.0/24 that this computer does not use, for
the loopback adapter, the second bind-address, the server certificate and the firewall rules of S2-2's database TLS check (steps a to f
above). -Revert and -RequireSecureTransport take it from the record in my.ini (leave it out).

.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -DryRun
Shows what the script would change.

.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -WithClientCertificate -WithTestNetwork 192.0.2.25
From a PowerShell started with "Run as administrator": turns on TLS with the test network of S2-2's check, after you type YES.

.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1
From a PowerShell started with "Run as administrator": turns on TLS after you type YES.

.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Enable-LocalDbTls.ps1 -Revert
Undoes it, in the folders the enable run recorded in my.ini.
#>
[CmdletBinding(PositionalBinding = $false)]
param(
	[switch]$RequireSecureTransport,
	[switch]$Revert,
	[switch]$DryRun,
	[string]$TestCertificatesFolder = '',
	[ValidatePattern('^[A-Za-z0-9_.-]{1,64}$')]
	[string]$ServiceName = 'MariaDB',
	[string]$DevFolder = '',
	[string]$DevUser = '',
	[ValidatePattern('^[A-Za-z0-9_.-]{1,64}$')]
	[string]$User = 'root',
	[switch]$WithClientCertificate,
	[string]$ClientFolder = '',
	[string]$WithTestNetwork = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$MarkerBegin = '# BEGIN Open PMS - Dental local test TLS (tools/Enable-LocalDbTls.ps1; undo with -Revert)'
$MarkerEnd = '# END Open PMS - Dental local test TLS'
# Comment lines inside the block: the folders the enable run wrote to (and a parent folder it created for them), which -Revert and
# -RequireSecureTransport read back.
$RecordDevPrefix = '# dev-folder='
$RecordClientPrefix = '# client-folder='
$RecordParentPrefix = '# created-parent='
$TlsFolderName = 'opms-dev-tls'
# The dev folder and the client folder are always <a user profile's %LOCALAPPDATA%>\OpenPmsDental\<these names>.
$AppFolderName = 'OpenPmsDental'
$DevFolderName = 'DevDbTls'
$ClientFolderName = 'DevDbTlsClient'
$BackupSuffix = '.before-opms-dev-tls'
# Write-OptionFile writes my.ini here first and then moves it over my.ini.
$IniTempSuffix = '.opms-new'
$RepoRoot = Split-Path -Parent $PSScriptRoot
# The test network of S2-2's database TLS check (-WithTestNetwork): the record lines of the block, and the names of what the run makes.
$RecordAddressPrefix = '# test-address='
$RecordAdapterPrefix = '# test-adapter='
$RecordFirewallPrefix = '# test-firewall-rule='
$RecordCapiPrefix = '# test-client-capi='
$RecordCngPrefix = '# test-client-cng='
$TestAdapterName = 'OPMS DB TLS test'
$TestAdapterHardwareId = '*MSLOOP'
$FirewallGroup = 'Open PMS - Dental local test TLS'
$FirewallAllowName = 'OpenPmsDental-DbTlsTest-Allow3306'
$FirewallBlockName = 'OpenPmsDental-DbTlsTest-Block3306'
$CapiProviderName = 'Microsoft Enhanced RSA and AES Cryptographic Provider'
$CngProviderName = 'Microsoft Software Key Storage Provider'
$TestClientCapiName = 'opms-dbtls-test-client-capi'
$TestClientCngName = 'opms-dbtls-test-client-cng'
# The .pfx files of the two test client certificates live here (inside the opms-dev-tls folder, Administrators only) until their import.
$PfxFolderName = 'client-pfx'

# A check refused: say why and stop with exit code 1. Every check runs before the first change, so nothing needs undoing.
function Stop-Refused([string]$Message) {
	Write-Host "REFUSED: $Message"
	exit 1
}

# A path as PowerShell resolves it (relative to the current location), so .NET file calls and cmdlets agree on it.
function Resolve-FullPath([string]$Path) {
	return $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Path)
}

#region Certificates

function New-SerialNumber {
	$bytes = New-Object byte[] 16
	$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
	try {
		$rng.GetBytes($bytes)
	}
	finally {
		$rng.Dispose()
	}
	$bytes[0] = $bytes[0] -band 0x7F
	if ($bytes[0] -eq 0) {
		$bytes[0] = 1
	}
	return , $bytes
}

function ConvertTo-Pem([byte[]]$Der, [string]$Label) {
	$base64 = [Convert]::ToBase64String($Der)
	$builder = New-Object System.Text.StringBuilder
	[void]$builder.Append("-----BEGIN $Label-----`n")
	for ($i = 0; $i -lt $base64.Length; $i += 64) {
		[void]$builder.Append($base64.Substring($i, [Math]::Min(64, $base64.Length - $i)))
		[void]$builder.Append("`n")
	}
	[void]$builder.Append("-----END $Label-----`n")
	return $builder.ToString()
}

function ConvertFrom-Hex([string]$Hex) {
	$bytes = New-Object byte[] ($Hex.Length / 2)
	for ($i = 0; $i -lt $bytes.Length; $i++) {
		$bytes[$i] = [Convert]::ToByte($Hex.Substring($i * 2, 2), 16)
	}
	return , $bytes
}

function New-Request([string]$Subject, [System.Security.Cryptography.RSA]$Key) {
	return New-Object System.Security.Cryptography.X509Certificates.CertificateRequest($Subject, $Key,
		[System.Security.Cryptography.HashAlgorithmName]::SHA256, [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
}

function Add-LeafExtensions($Request, [string]$EkuOid, [string]$AuthorityKeyId) {
	$Request.CertificateExtensions.Add((New-Object System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension($false, $false, 0, $true)))
	$keyUsage = [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature -bor [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyEncipherment
	$Request.CertificateExtensions.Add((New-Object System.Security.Cryptography.X509Certificates.X509KeyUsageExtension($keyUsage, $true)))
	$oids = New-Object System.Security.Cryptography.OidCollection
	[void]$oids.Add((New-Object System.Security.Cryptography.Oid($EkuOid)))
	$Request.CertificateExtensions.Add((New-Object System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension($oids, $false)))
	$Request.CertificateExtensions.Add((New-Object System.Security.Cryptography.X509Certificates.X509SubjectKeyIdentifierExtension($Request.PublicKey, $false)))
	# Authority Key Identifier, keyIdentifier only: SEQUENCE { [0] <the CA's subject key identifier> }. .NET Framework has no class for it.
	$keyId = ConvertFrom-Hex $AuthorityKeyId
	$value = New-Object byte[] (4 + $keyId.Length)
	$value[0] = 0x30
	$value[1] = [byte](2 + $keyId.Length)
	$value[2] = 0x80
	$value[3] = [byte]$keyId.Length
	[Array]::Copy($keyId, 0, $value, 4, $keyId.Length)
	$Request.CertificateExtensions.Add((New-Object System.Security.Cryptography.X509Certificates.X509Extension('2.5.29.35', $value, $false)))
}

# A random password for the .pfx files of the test client certificates: 32 hex digits, used for their export and their import only.
function New-PfxPassword {
	$bytes = New-Object byte[] 16
	$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
	try {
		$rng.GetBytes($bytes)
	}
	finally {
		$rng.Dispose()
	}
	return (($bytes | ForEach-Object { $_.ToString('x2') }) -join '')
}

# One test client certificate of -WithTestNetwork (clientAuth, signed by $Ca) as a .pfx with only this certificate and its key (no CA, so
# that the import adds nothing to another store). Returns the .pfx bytes and the thumbprint; the key is disposed before it returns.
function New-TestClientPfx($Ca, [string]$CaKeyId, [string]$CommonName, [DateTimeOffset]$NotBefore, [string]$Password) {
	$key = New-Object System.Security.Cryptography.RSACng(2048)
	$withKey = $null
	try {
		$request = New-Request "CN=$CommonName, O=Open PMS - Dental local test" $key
		Add-LeafExtensions $request '1.3.6.1.5.5.7.3.2' $CaKeyId
		$certificate = $request.Create($Ca, $NotBefore, $NotBefore.AddYears(5), (New-SerialNumber))
		$withKey = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::CopyWithPrivateKey($certificate, $key)
		$pfx = $withKey.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pkcs12, $Password)
		return [pscustomobject]@{ Pfx = $pfx; Thumbprint = $certificate.Thumbprint }
	}
	finally {
		if ($null -ne $withKey) {
			$withKey.Reset()
		}
		$key.Dispose()
	}
}

# The certificate set: the CA (its key only in memory), the server certificate for localhost and 127.0.0.1 (with $TestAddress also for
# that address and $HostName), optionally the client certificate, and with $TestAddress the two test client certificates as .pfx bytes.
# Returns PEM texts; the keys are disposed before it returns.
function New-CertificateSet([bool]$WithClient, [string]$TestAddress = '', [string]$HostName = '') {
	$notBefore = [DateTimeOffset]::UtcNow.AddMinutes(-10)
	$stamp = [DateTime]::UtcNow.ToString('yyyy-MM-dd')
	$caKey = New-Object System.Security.Cryptography.RSACng(3072)
	$serverKey = New-Object System.Security.Cryptography.RSACng(2048)
	$clientKey = $null
	$ca = $null
	try {
		$caRequest = New-Request "CN=Open PMS - Dental local test CA $stamp, O=Open PMS - Dental development" $caKey
		$caRequest.CertificateExtensions.Add((New-Object System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension($true, $true, 0, $true)))
		$caUsage = [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyCertSign -bor [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::CrlSign
		$caRequest.CertificateExtensions.Add((New-Object System.Security.Cryptography.X509Certificates.X509KeyUsageExtension($caUsage, $true)))
		$caKeyId = New-Object System.Security.Cryptography.X509Certificates.X509SubjectKeyIdentifierExtension($caRequest.PublicKey, $false)
		$caRequest.CertificateExtensions.Add($caKeyId)
		$ca = $caRequest.CreateSelfSigned($notBefore, $notBefore.AddYears(10))

		$serverRequest = New-Request 'CN=localhost, O=Open PMS - Dental local test server' $serverKey
		Add-LeafExtensions $serverRequest '1.3.6.1.5.5.7.3.1' $caKeyId.SubjectKeyIdentifier
		$names = New-Object System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder
		$names.AddDnsName('localhost')
		$names.AddIpAddress([System.Net.IPAddress]::Parse('127.0.0.1'))
		if ($TestAddress -ne '') {
			if ($HostName -ne '') {
				$names.AddDnsName($HostName)
			}
			$names.AddIpAddress([System.Net.IPAddress]::Parse($TestAddress))
		}
		$serverRequest.CertificateExtensions.Add($names.Build($false))
		$server = $serverRequest.Create($ca, $notBefore, $notBefore.AddYears(5), (New-SerialNumber))

		$set = [ordered]@{
			CaCert     = ConvertTo-Pem ($ca.RawData) 'CERTIFICATE'
			ServerCert = ConvertTo-Pem ($server.RawData) 'CERTIFICATE'
			ServerKey  = ConvertTo-Pem ($serverKey.Key.Export([System.Security.Cryptography.CngKeyBlobFormat]::Pkcs8PrivateBlob)) 'PRIVATE KEY'
			CaThumbprint     = $ca.Thumbprint
			ServerThumbprint = $server.Thumbprint
			NotAfter         = $server.NotAfter
			ClientCert = $null
			ClientKey  = $null
			TestPfxPassword  = ''
			TestCapiPfx      = $null
			TestCapiThumbprint = ''
			TestCngPfx       = $null
			TestCngThumbprint = ''
		}
		if ($TestAddress -ne '') {
			$set.TestPfxPassword = New-PfxPassword
			$capi = New-TestClientPfx $ca $caKeyId.SubjectKeyIdentifier $TestClientCapiName $notBefore $set.TestPfxPassword
			$cng = New-TestClientPfx $ca $caKeyId.SubjectKeyIdentifier $TestClientCngName $notBefore $set.TestPfxPassword
			$set.TestCapiPfx = $capi.Pfx
			$set.TestCapiThumbprint = $capi.Thumbprint
			$set.TestCngPfx = $cng.Pfx
			$set.TestCngThumbprint = $cng.Thumbprint
		}
		if ($WithClient) {
			$clientKey = New-Object System.Security.Cryptography.RSACng(2048)
			$clientRequest = New-Request 'CN=opms-dbtls-test-client, O=Open PMS - Dental local test' $clientKey
			Add-LeafExtensions $clientRequest '1.3.6.1.5.5.7.3.2' $caKeyId.SubjectKeyIdentifier
			$client = $clientRequest.Create($ca, $notBefore, $notBefore.AddYears(5), (New-SerialNumber))
			$set.ClientCert = ConvertTo-Pem ($client.RawData) 'CERTIFICATE'
			$set.ClientKey = ConvertTo-Pem ($clientKey.Key.Export([System.Security.Cryptography.CngKeyBlobFormat]::Pkcs8PrivateBlob)) 'PRIVATE KEY'
		}
		return $set
	}
	finally {
		if ($ca -ne $null) {
			$ca.Reset()
		}
		$caKey.Dispose()
		$serverKey.Dispose()
		if ($clientKey -ne $null) {
			$clientKey.Dispose()
		}
	}
}

#endregion

#region Files and ACLs

function Get-Sid([string]$Account) {
	return (New-Object System.Security.Principal.NTAccount($Account)).Translate([System.Security.Principal.SecurityIdentifier])
}

# A protected ACL (nothing inherited): full control for SYSTEM, Administrators and $FullSids, read for $ReadSids; owner Administrators.
function Set-ProtectedAcl([string]$Path, [System.Security.Principal.SecurityIdentifier[]]$FullSids, [System.Security.Principal.SecurityIdentifier[]]$ReadSids) {
	$isFolder = Test-Path -LiteralPath $Path -PathType Container
	if ($isFolder) {
		$acl = New-Object System.Security.AccessControl.DirectorySecurity
		$inherit = [System.Security.AccessControl.InheritanceFlags]'ContainerInherit, ObjectInherit'
	}
	else {
		$acl = New-Object System.Security.AccessControl.FileSecurity
		$inherit = [System.Security.AccessControl.InheritanceFlags]::None
	}
	$acl.SetAccessRuleProtection($true, $false)
	$admins = New-Object System.Security.Principal.SecurityIdentifier('S-1-5-32-544')
	$system = New-Object System.Security.Principal.SecurityIdentifier('S-1-5-18')
	$acl.SetOwner($admins)
	$full = @($system, $admins) + @($FullSids | Where-Object { $_ -ne $null })
	foreach ($sid in $full) {
		$acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule($sid, 'FullControl', $inherit, 'None', 'Allow')))
	}
	foreach ($sid in @($ReadSids | Where-Object { $_ -ne $null })) {
		$acl.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule($sid, 'ReadAndExecute', $inherit, 'None', 'Allow')))
	}
	Set-Acl -LiteralPath $Path -AclObject $acl
}

function Write-AsciiFile([string]$Path, [string]$Text) {
	[System.IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.ASCIIEncoding))
}

function Test-InsideRepository([string]$Path) {
	$full = (Resolve-FullPath $Path).TrimEnd('\') + '\'
	$repo = [System.IO.Path]::GetFullPath($RepoRoot).TrimEnd('\') + '\'
	return $full.StartsWith($repo, [StringComparison]::OrdinalIgnoreCase)
}

# The access rules (DACL) of a file or folder as SDDL, so that a failed run can put them back (Set-AccessSddl).
function Get-AccessSddl([string]$Path) {
	return (Get-Acl -LiteralPath $Path).GetSecurityDescriptorSddlForm([System.Security.AccessControl.AccessControlSections]::Access)
}

# Puts back the access rules that Get-AccessSddl read; nothing else of the security descriptor (the owner stays as it is). Inherited rules
# are taken from the parent again, as they were (Windows may then mark the list auto-inherited, AI: the same rules).
function Set-AccessSddl([string]$Path, [string]$Sddl) {
	if (Test-Path -LiteralPath $Path -PathType Container) {
		$security = New-Object System.Security.AccessControl.DirectorySecurity
		$security.SetSecurityDescriptorSddlForm($Sddl, [System.Security.AccessControl.AccessControlSections]::Access)
		[System.IO.Directory]::SetAccessControl($Path, $security)
	}
	else {
		$security = New-Object System.Security.AccessControl.FileSecurity
		$security.SetSecurityDescriptorSddlForm($Sddl, [System.Security.AccessControl.AccessControlSections]::Access)
		[System.IO.File]::SetAccessControl($Path, $security)
	}
}

# The full form of a path on a drive (C:\...), without . and .. segments and without a final backslash (except a drive root); $null for
# anything else (a relative path, a path on the current drive without a letter, a share, an invalid path).
function Get-CanonicalPath([string]$Path) {
	$value = ([string]$Path).Trim().Replace('/', '\')
	if ($value -notmatch '^[A-Za-z]:\\') {
		return $null
	}
	try {
		$full = [System.IO.Path]::GetFullPath($value)
	}
	catch {
		return $null
	}
	if ($full.Length -gt 3) {
		$full = $full.TrimEnd('\')
	}
	return $full
}

# The %LOCALAPPDATA% folders of the user profiles of this computer: <profile>\AppData\Local of every profile in the registry's profile list
# (readable without elevation), except the service profiles of SYSTEM, LOCAL SERVICE and NETWORK SERVICE, and this account's own.
function Get-LocalAppDataRoots {
	$roots = New-Object System.Collections.Generic.List[string]
	$key = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList'
	foreach ($item in @(Get-ChildItem -LiteralPath $key -ErrorAction SilentlyContinue)) {
		if (@('S-1-5-18', 'S-1-5-19', 'S-1-5-20') -contains $item.PSChildName) {
			continue
		}
		$property = Get-ItemProperty -LiteralPath $item.PSPath -Name 'ProfileImagePath' -ErrorAction SilentlyContinue
		if ($null -ne $property -and [string]$property.ProfileImagePath -ne '') {
			$roots.Add((Join-Path ([Environment]::ExpandEnvironmentVariables([string]$property.ProfileImagePath)) 'AppData\Local'))
		}
	}
	$own = [Environment]::GetFolderPath('LocalApplicationData')
	if ($own -ne '') {
		$roots.Add($own)
	}
	return @($roots | Where-Object { $null -ne (Get-CanonicalPath $_) } | ForEach-Object { Get-CanonicalPath $_ } | Sort-Object -Unique)
}

# True when $Path is <one of $LocalRoots>\OpenPmsDental\$Name: the only form of a dev folder (DevDbTls) or a client folder
# (DevDbTlsClient) that this script writes to, and so the only one -Revert and -RequireSecureTransport accept from the record in my.ini.
function Test-ProfileFolder([string]$Path, [string]$Name, [string[]]$LocalRoots) {
	$canonical = Get-CanonicalPath $Path
	if ($null -eq $canonical) {
		return $false
	}
	foreach ($root in @($LocalRoots)) {
		$rootCanonical = Get-CanonicalPath $root
		if ($null -ne $rootCanonical -and (Get-NormalPath $canonical) -eq (Get-NormalPath (Join-Path $rootCanonical "$AppFolderName\$Name"))) {
			return $true
		}
	}
	return $false
}

#endregion

#region The service, my.ini and the client

function Get-ServiceFacts {
	$service = Get-CimInstance -ClassName Win32_Service -Filter "Name='$ServiceName'"
	if ($service -eq $null) {
		Stop-Refused "The service '$ServiceName' was not found on this computer. Pass -ServiceName for the local MariaDB service."
	}
	$path = [string]$service.PathName
	$match = [regex]::Match($path, '^\s*"?(?<exe>[^"]+?\.exe)"?(?<args>.*)$')
	if (-not $match.Success) {
		Stop-Refused "The service '$ServiceName' has a command line this script does not read: $path"
	}
	$exe = $match.Groups['exe'].Value
	$exeName = [System.IO.Path]::GetFileName($exe).ToLowerInvariant()
	if ($exeName -ne 'mysqld.exe' -and $exeName -ne 'mariadbd.exe') {
		Stop-Refused "The service '$ServiceName' runs $exe, not a MariaDB server (mysqld.exe or mariadbd.exe)."
	}
	# "--defaults-file=C:\...\my.ini" (the installer's form), --defaults-file="...", or without quotes.
	$defaults = [regex]::Match($match.Groups['args'].Value, '"--defaults-file=(?<f>[^"]+)"|--defaults-file="(?<f>[^"]+)"|--defaults-file=(?<f>[^\s"]+)')
	if (-not $defaults.Success) {
		Stop-Refused "The service '$ServiceName' has no --defaults-file, so this script cannot tell which option file it reads."
	}
	$ini = $defaults.Groups['f'].Value
	if (-not (Test-Path -LiteralPath $ini -PathType Leaf)) {
		Stop-Refused "The option file of the service, $ini, does not exist."
	}
	$bin = Split-Path -Parent $exe
	$client = Join-Path $bin 'mariadb.exe'
	if (-not (Test-Path -LiteralPath $client)) {
		$client = Join-Path $bin 'mysql.exe'
	}
	if (-not (Test-Path -LiteralPath $client)) {
		Stop-Refused "No mariadb.exe or mysql.exe next to $exe."
	}
	return [pscustomobject]@{
		Name      = $service.Name
		State     = [string]$service.State
		ProcessId = [int]$service.ProcessId
		Account   = [string]$service.StartName
		Exe       = $exe
		Ini       = $ini
		Bin       = $bin
		Home      = Split-Path -Parent $bin
		Client    = $client
		PluginDir = Join-Path (Split-Path -Parent $bin) 'lib\plugin'
	}
}

# my.ini as lines (each without its line end), its line end and its encoding, so that a write keeps every other byte as it was. Refuses a
# file whose bytes do not come back unchanged from the decoding (bytes that are not UTF-8, such as an ANSI or UTF-16 file): writing it
# back would change bytes the script does not mean to touch.
function Read-OptionFile([string]$Path) {
	$bytes = [System.IO.File]::ReadAllBytes($Path)
	$encoding = New-Object System.Text.UTF8Encoding($false)
	$offset = 0
	if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
		$encoding = New-Object System.Text.UTF8Encoding($true)
		$offset = 3
	}
	$text = [System.Text.Encoding]::UTF8.GetString($bytes, $offset, $bytes.Length - $offset)
	$again = [System.Text.Encoding]::UTF8.GetBytes($text)
	$differsAt = -1
	for ($i = 0; $i -lt [Math]::Max($again.Length, $bytes.Length - $offset); $i++) {
		if ($i -ge $again.Length -or $i -ge $bytes.Length - $offset -or $again[$i] -ne $bytes[$offset + $i]) {
			$differsAt = $offset + $i
			break
		}
	}
	if ($differsAt -ge 0) {
		$byte = ''
		if ($differsAt -lt $bytes.Length) {
			$byte = ' (0x' + $bytes[$differsAt].ToString('X2') + ')'
		}
		Stop-Refused "$Path is not UTF-8 text: the byte at offset $differsAt$byte is not UTF-8, so writing the file back would change bytes this script does not mean to touch. Save it as UTF-8 or ASCII with nothing else changed (the MariaDB installer writes ASCII), then run the script again."
	}
	$crlf = ([regex]::Matches($text, "`r`n")).Count
	$lf = ([regex]::Matches($text, "`n")).Count - $crlf
	$newline = "`n"
	if ($crlf -gt $lf) {
		$newline = "`r`n"
	}
	return [pscustomobject]@{
		Bytes    = $bytes
		Text     = $text
		Encoding = $encoding
		Newline  = $newline
	}
}

# The values of the server's sections ([mysqld], [mariadb], [mariadbd], [server] and their versioned forms), last one wins, keys with
# - and _ alike, lower case.
function Get-ServerOptions([string]$Text) {
	$options = @{}
	$section = ''
	foreach ($rawLine in ($Text -split "`n")) {
		$line = $rawLine.Trim()
		if ($line -eq '' -or $line.StartsWith('#') -or $line.StartsWith(';')) {
			continue
		}
		$header = [regex]::Match($line, '^\[(?<s>[^\]]+)\]')
		if ($header.Success) {
			$section = $header.Groups['s'].Value.Trim().ToLowerInvariant()
			continue
		}
		if ($section -notmatch '^(mysqld|mariadb|mariadbd|server)(-[0-9.]+)?$') {
			continue
		}
		$key = $line
		$value = ''
		$equals = $line.IndexOf('=')
		if ($equals -ge 0) {
			$key = $line.Substring(0, $equals).Trim()
			$value = $line.Substring($equals + 1).Trim().Trim('"').Trim("'")
		}
		$key = $key.ToLowerInvariant().Replace('-', '_')
		$key = $key -replace '^(loose_|enable_)', ''
		$options[$key] = $value
	}
	return $options
}

function Test-HasBlock([string]$Text) {
	return $Text.Contains($MarkerBegin) -and $Text.Contains($MarkerEnd)
}

# The lines of my.ini that read other option files (!include, !includedir): the options there are out of this script's sight.
function Get-IncludeLines([string]$Text) {
	return @($Text -split "`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ -match '^!include' })
}

# my.ini without the script's block (the lines from the begin to the end marker).
function Remove-Block([string]$Text) {
	$pattern = '(?m)^' + [regex]::Escape($MarkerBegin) + '[^\n]*\n(?:.*\n)*?' + [regex]::Escape($MarkerEnd) + '[^\n]*(\n|$)'
	return [regex]::Replace($Text, $pattern, '')
}

# Inserts lines at the end of the [mysqld] section (before the next section header, without the blank lines before it).
function Add-ToMysqldSection([string]$Text, [string[]]$Lines, [string]$Newline) {
	$list = New-Object System.Collections.Generic.List[string]
	foreach ($line in ($Text -split "`n")) {
		$list.Add($line)
	}
	$start = -1
	for ($i = 0; $i -lt $list.Count; $i++) {
		if ($list[$i].Trim().ToLowerInvariant() -match '^\[mysqld\]') {
			$start = $i
			break
		}
	}
	if ($start -lt 0) {
		Stop-Refused 'my.ini has no [mysqld] section.'
	}
	$end = $list.Count
	for ($i = $start + 1; $i -lt $list.Count; $i++) {
		if ($list[$i].Trim().StartsWith('[')) {
			$end = $i
			break
		}
	}
	$insertAt = $end
	while ($insertAt -gt $start + 1 -and $list[$insertAt - 1].Trim() -eq '') {
		$insertAt--
	}
	$lineEnd = ''
	if ($Newline -eq "`r`n") {
		$lineEnd = "`r"
	}
	$insert = @($Lines | ForEach-Object { $_ + $lineEnd })
	# The last line of a file without a final line end gets one, so the block starts on its own line.
	if ($insertAt -eq $list.Count -and $list.Count -gt 0 -and $list[$list.Count - 1] -ne '') {
		$list.Add('')
		$insertAt = $list.Count - 1
	}
	$list.InsertRange($insertAt, [string[]]$insert)
	return ($list -join "`n")
}

function Write-OptionFile($File, [string]$Text) {
	$bytes = $File.Encoding.GetPreamble() + $File.Encoding.GetBytes($Text)
	$temp = $script:Facts.Ini + $IniTempSuffix
	[System.IO.File]::WriteAllBytes($temp, [byte[]]$bytes)
	Move-Item -LiteralPath $temp -Destination $script:Facts.Ini -Force
}

function Join-Arguments([string[]]$Arguments) {
	$quoted = foreach ($argument in $Arguments) {
		if ($argument -match '[\s"]') {
			'"' + $argument.Replace('"', '\"') + '"'
		}
		else {
			$argument
		}
	}
	return ($quoted -join ' ')
}

# Runs one statement list with the service's client on 127.0.0.1:3306 over TCP (or, for the check of -WithTestNetwork, on the recorded
# test address, port 3306). $Tls: 'plain' (no TLS), 'tls' (TLS without verification) or the path of a CA file (TLS, the server verified
# against it, host name included).
function Invoke-DbClient([string]$Sql, [string]$Tls = 'tls', [string]$ServerAddress = '127.0.0.1') {
	$arguments = @('--no-defaults', "--plugin-dir=$($script:Facts.PluginDir)", '--protocol=TCP', "--host=$ServerAddress", '--port=3306',
		"--user=$User", '--connect-timeout=10', '--batch', '--raw', '--skip-column-names')
	if ($Tls -eq 'plain') {
		$arguments += '--skip-ssl'
	}
	elseif ($Tls -eq 'tls') {
		$arguments += '--ssl'
	}
	else {
		$arguments += @("--ssl-ca=$Tls", '--ssl-verify-server-cert')
	}
	$arguments += @('-e', $Sql)
	$startInfo = New-Object System.Diagnostics.ProcessStartInfo
	$startInfo.FileName = $script:Facts.Client
	$startInfo.Arguments = Join-Arguments $arguments
	$startInfo.UseShellExecute = $false
	$startInfo.RedirectStandardOutput = $true
	$startInfo.RedirectStandardError = $true
	$startInfo.CreateNoWindow = $true
	$process = [System.Diagnostics.Process]::Start($startInfo)
	$stdoutTask = $process.StandardOutput.ReadToEndAsync()
	$stderr = $process.StandardError.ReadToEnd()
	$process.WaitForExit()
	return [pscustomobject]@{
		ExitCode = $process.ExitCode
		Lines    = @($stdoutTask.Result -split "`r?`n" | Where-Object { $_ -ne '' })
		Error    = $stderr.Trim()
	}
}

# Name/value lines of a SELECT/SHOW list into a hashtable.
function Get-ServerState([string]$Tls) {
	$sql = "SELECT 'port',@@port UNION ALL SELECT 'bind_address',@@bind_address UNION ALL SELECT 'datadir',@@datadir " +
		"UNION ALL SELECT 'version',@@version UNION ALL SELECT 'have_ssl',@@have_ssl " +
		"UNION ALL SELECT 'require_secure_transport',@@require_secure_transport UNION ALL SELECT 'tls_version',@@tls_version " +
		"UNION ALL SELECT 'ssl_cert',@@ssl_cert; " +
		"SHOW SESSION STATUS WHERE Variable_name IN ('Ssl_cipher','Ssl_version','Ssl_server_not_after')"
	$result = Invoke-DbClient $sql $Tls
	if ($result.ExitCode -ne 0) {
		return [pscustomobject]@{ Ok = $false; Error = $result.Error; Values = @{} }
	}
	$values = @{}
	foreach ($line in $result.Lines) {
		$parts = $line -split "`t", 2
		if ($parts.Count -eq 2) {
			$values[$parts[0].ToLowerInvariant()] = $parts[1]
		}
		else {
			$values[$parts[0].ToLowerInvariant()] = ''
		}
	}
	foreach ($key in @('ssl_cipher', 'ssl_version', 'ssl_server_not_after')) {
		if (-not $values.ContainsKey($key)) {
			$values[$key] = ''
		}
	}
	return [pscustomobject]@{ Ok = $true; Error = ''; Values = $values }
}

function Get-NormalPath([string]$Path) {
	return $Path.Replace('/', '\').TrimEnd('\').ToLowerInvariant()
}

# The listening sockets where a connection to 127.0.0.1:3306 can land: 127.0.0.1 itself, and the addresses for every interface
# (0.0.0.0, ::), which take such a connection whenever nothing listens on 127.0.0.1 itself (during a restart, for one). Each with its
# address and its owning process. Get-NetTCPConnection needs no elevation.
function Get-Listeners {
	$listeners = @()
	foreach ($connection in @(Get-NetTCPConnection -LocalPort 3306 -State Listen -ErrorAction SilentlyContinue)) {
		$address = [string]$connection.LocalAddress
		if (@('127.0.0.1', '0.0.0.0', '::') -notcontains $address) {
			continue
		}
		$processId = [int]$connection.OwningProcess
		$name = ''
		$process = Get-Process -Id $processId -ErrorAction SilentlyContinue
		if ($null -ne $process) {
			$name = [string]$process.ProcessName
		}
		$listeners += [pscustomobject]@{ Address = $address; ProcessId = $processId; ProcessName = $name }
	}
	return , $listeners
}

# Why the listeners ($Listeners, as Get-Listeners gives them) are not the service's own process ($ServicePid), or $null when the service's
# process listens on 127.0.0.1:3306 and no other process listens where a connection to it can land.
function Get-ListenerProblem($Listeners, [int]$ServicePid) {
	$list = @($Listeners | Where-Object { $null -ne $_ })
	if ($ServicePid -le 0) {
		return "the service $ServiceName has no process"
	}
	$own = @($list | Where-Object { $_.Address -eq '127.0.0.1' -and $_.ProcessId -eq $ServicePid })
	if ($own.Count -eq 0) {
		$on127 = @($list | Where-Object { $_.Address -eq '127.0.0.1' })
		if ($on127.Count -eq 0) {
			return "the service's process $ServicePid does not listen on 127.0.0.1:3306 (nothing does)"
		}
		return "process $($on127[0].ProcessId) ($($on127[0].ProcessName)) listens on 127.0.0.1:3306, not the service's process $ServicePid"
	}
	$others = @($list | Where-Object { $_.ProcessId -ne $ServicePid })
	if ($others.Count -gt 0) {
		return "process $($others[0].ProcessId) ($($others[0].ProcessName)) also listens on $($others[0].Address) port 3306, which takes the connections to 127.0.0.1:3306 whenever the service's socket is closed"
	}
	return $null
}

function Get-ServiceProcessId {
	$service = Get-CimInstance -ClassName Win32_Service -Filter "Name='$ServiceName'"
	if ($null -eq $service) {
		return 0
	}
	return [int]$service.ProcessId
}

# Restarts the service and waits until 127.0.0.1:3306 answers and its listener is the service's new process (Get-ListenerProblem).
function Restart-DbService {
	Write-Host "Restarting the service $ServiceName ..."
	Restart-Service -Name $ServiceName -Force
	$deadline = [DateTime]::UtcNow.AddSeconds(90)
	$problem = 'nothing answered on 127.0.0.1:3306'
	while ([DateTime]::UtcNow -lt $deadline) {
		$client = New-Object System.Net.Sockets.TcpClient
		$answers = $false
		try {
			$client.Connect('127.0.0.1', 3306)
			$answers = $true
		}
		catch {
			$answers = $false
		}
		finally {
			$client.Close()
		}
		if ($answers) {
			$problem = Get-ListenerProblem (Get-Listeners) (Get-ServiceProcessId)
			if ($null -eq $problem) {
				return
			}
		}
		Start-Sleep -Milliseconds 500
	}
	throw "After the restart of the service $ServiceName, within 90 seconds: $problem."
}

function Confirm-Change([string]$Question) {
	if ($DryRun) {
		Write-Host ''
		Write-Host 'Dry run: nothing was changed.'
		exit 0
	}
	Write-Host ''
	$answer = ''
	try {
		$answer = Read-Host "$Question Type YES to continue"
	}
	catch {
		$answer = ''
	}
	if ($answer -cne 'YES') {
		Write-Host 'Nothing was changed.'
		exit 2
	}
}

function Test-Elevated {
	$principal = New-Object System.Security.Principal.WindowsPrincipal([System.Security.Principal.WindowsIdentity]::GetCurrent())
	return $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)
}

# The checks every mode shares: the service, its my.ini, and the server on 127.0.0.1:3306 being that service.
function Assert-LocalServer {
	$script:Facts = Get-ServiceFacts
	Write-Host "Service:        $($script:Facts.Name) ($($script:Facts.State)), account $($script:Facts.Account)"
	Write-Host "Server program: $($script:Facts.Exe)"
	Write-Host "Option file:    $($script:Facts.Ini)"
	if ($script:Facts.State -ne 'Running') {
		Stop-Refused "The service $ServiceName is $($script:Facts.State). Start it first."
	}
	$script:IniFile = Read-OptionFile $script:Facts.Ini
	$includes = @(Get-IncludeLines $script:IniFile.Text)
	if ($includes.Count -gt 0) {
		Stop-Refused "my.ini reads other option files ($($includes[0])). Their options (a port, a bind-address, TLS lines) are out of this script's sight and it never edits them: move what they set into my.ini, or turn on TLS by hand."
	}
	$options = Get-ServerOptions $script:IniFile.Text
	$port = '3306'
	if ($options.ContainsKey('port')) {
		$port = $options['port']
	}
	if ($port -ne '3306') {
		Stop-Refused "my.ini sets port=$port. This script is only for the local server on 127.0.0.1:3306."
	}
	# 127.0.0.1 alone; or, when the block of this script records a test address (-WithTestNetwork), 127.0.0.1 and that address as the block
	# sets them.
	$expectedBind = Get-ExpectedBindAddress $script:IniFile.Text
	if (-not $options.ContainsKey('bind_address') -or (Get-BindText $options['bind_address']) -ne $expectedBind) {
		$bind = '(not set: every address)'
		if ($options.ContainsKey('bind_address')) {
			$bind = $options['bind_address']
		}
		if ($expectedBind -eq '127.0.0.1') {
			Stop-Refused "my.ini's bind-address is $bind. This script is only for a server that listens on 127.0.0.1 alone."
		}
		Stop-Refused "my.ini's bind-address is $bind, but the block of this script sets $expectedBind (127.0.0.1 and the recorded test address). Look at my.ini: something else changed it."
	}
	# The process behind 127.0.0.1:3306 must be the service's own before the client sends anything there.
	try {
		$listeners = Get-Listeners
	}
	catch {
		Stop-Refused "This script cannot tell which process listens on 127.0.0.1:3306 (Get-NetTCPConnection: $($_.Exception.Message))."
	}
	$listenerProblem = Get-ListenerProblem $listeners $script:Facts.ProcessId
	if ($null -ne $listenerProblem) {
		Stop-Refused "The server on 127.0.0.1:3306 is not proven to be the service $($ServiceName): $listenerProblem."
	}
	$listenerName = @($listeners | Where-Object { $_.Address -eq '127.0.0.1' })[0].ProcessName
	Write-Host "Listener:       127.0.0.1:3306, process $($script:Facts.ProcessId) ($listenerName), the service's own"
	$state = Get-ServerState 'tls'
	if (-not $state.Ok) {
		Stop-Refused "No connection to 127.0.0.1:3306 as $User ($($state.Error)). Set MYSQL_PWD when the account has a password."
	}
	$values = $state.Values
	if ($values['port'] -ne '3306' -or (Get-BindText $values['bind_address']) -ne $expectedBind) {
		Stop-Refused "The server on 127.0.0.1:3306 reports port $($values['port']) and bind address $($values['bind_address'])."
	}
	if ($options.ContainsKey('datadir') -and (Get-NormalPath $options['datadir']) -ne (Get-NormalPath $values['datadir'])) {
		Stop-Refused "The server on 127.0.0.1:3306 uses the data folder $($values['datadir']), but $($script:Facts.Ini) names $($options['datadir']): it is not the service $ServiceName."
	}
	Write-Host "Server:         $($values['version']) on 127.0.0.1:3306, data folder $($values['datadir'])"
	Write-Host "TLS now:        have_ssl=$($values['have_ssl']), require_secure_transport=$($values['require_secure_transport']), tls_version=$($values['tls_version']), this session's cipher '$($values['ssl_cipher'])'"
	return $state
}

#endregion

#region The test network (-WithTestNetwork)

# A bind-address value without white space, so that '127.0.0.1, 192.0.2.25' and '127.0.0.1,192.0.2.25' compare alike.
function Get-BindText([string]$Value) {
	return ([string]$Value) -replace '\s', ''
}

# The bind-address the server must have: 127.0.0.1, or 127.0.0.1 and the test address when the block of this script records one.
function Get-ExpectedBindAddress([string]$Text) {
	$record = Get-RecordedTestNetwork $Text
	if ($null -eq $record) {
		return '127.0.0.1'
	}
	return "127.0.0.1,$($record.Address)"
}

# An IPv4 address in dotted form as a number (0 to 2^32-1), or $null for anything else.
function ConvertTo-AddressNumber([string]$Address) {
	$value = ([string]$Address).Trim()
	if ($value -notmatch '^[0-9]{1,3}(\.[0-9]{1,3}){3}$') {
		return $null
	}
	[int64]$number = 0
	foreach ($part in $value.Split('.')) {
		$byte = [int64]$part
		if ($byte -gt 255) {
			return $null
		}
		$number = $number * 256 + $byte
	}
	return $number
}

# Why $Address cannot be the test address, or $null when it can: one host address of 192.0.2.0/24 (192.0.2.1 to 192.0.2.254), written
# without leading zeros. TEST-NET-1 (RFC 5737) is the documentation range, which no network routes, so no other computer can reach it.
function Get-TestAddressFormProblem([string]$Address) {
	$value = ([string]$Address).Trim()
	$match = [regex]::Match($value, '^192\.0\.2\.(?<h>[0-9]{1,3})$')
	if ($match.Success) {
		$hostPart = [int]$match.Groups['h'].Value
		if ($match.Groups['h'].Value -eq [string]$hostPart -and $hostPart -ge 1 -and $hostPart -le 254) {
			return $null
		}
	}
	return "-WithTestNetwork $Address is not a host address of 192.0.2.0/24 (192.0.2.1 to 192.0.2.254, without leading zeros). The test network uses only the documentation range TEST-NET-1 (RFC 5737), which no network routes, so that no other computer can reach it."
}

# Why $Address is in use on this computer, or $null: $Addresses are this computer's IPv4 addresses (IPAddress, InterfaceAlias), $Routes its
# IPv4 routes (DestinationPrefix, InterfaceAlias). The address must be on no interface, no other address of 192.0.2.0/24 may be (another
# test network), and no route but the default route may lead into 192.0.2.0/24 (a network that uses the range after all, a VPN for one).
function Get-TestAddressUseProblem([string]$Address, $Addresses, $Routes) {
	$rangeStart = ConvertTo-AddressNumber '192.0.2.0'
	$rangeEnd = $rangeStart + 255
	foreach ($item in @($Addresses | Where-Object { $null -ne $_ })) {
		$ip = [string]$item.IPAddress
		if ($ip -eq $Address) {
			return "-WithTestNetwork $Address is in use: this computer has that address on $($item.InterfaceAlias)."
		}
		$number = ConvertTo-AddressNumber $ip
		if ($null -ne $number -and $number -ge $rangeStart -and $number -le $rangeEnd) {
			return "-WithTestNetwork ${Address}: this computer already has $ip of 192.0.2.0/24 on $($item.InterfaceAlias) (another test network, or an earlier run). Run -Revert, or remove that address first."
		}
	}
	foreach ($route in @($Routes | Where-Object { $null -ne $_ })) {
		$prefix = [string]$route.DestinationPrefix
		$parts = $prefix.Split('/')
		if ($parts.Count -ne 2) {
			continue
		}
		$start = ConvertTo-AddressNumber $parts[0]
		$length = 0
		if ($null -eq $start -or -not [int]::TryParse($parts[1], [ref]$length) -or $length -le 0 -or $length -gt 32) {
			continue
		}
		$size = [int64][Math]::Pow(2, 32 - $length)
		$start = $start - ($start % $size)
		$end = $start + $size - 1
		if ($start -le $rangeEnd -and $end -ge $rangeStart) {
			return "-WithTestNetwork ${Address}: the route $prefix on $($route.InterfaceAlias) leads into 192.0.2.0/24, so a network this computer is on uses that range (a VPN, for one). The test address could hide an address there, or be reached from there."
		}
	}
	return $null
}

# The remote addresses of the firewall rule that blocks port 3306 on the test address: every IPv4 address but the test address itself.
function Get-FirewallBlockRanges([string]$Address) {
	$hostPart = [int]($Address.Split('.')[3])
	return @("0.0.0.0-192.0.2.$($hostPart - 1)", "192.0.2.$($hostPart + 1)-255.255.255.255")
}

# The test network that the block of this script in my.ini records (the comment lines $RecordAddressPrefix to $RecordCngPrefix), or $null
# when the block records no test address (or there is no block).
function Get-RecordedTestNetwork([string]$Text) {
	$block = [regex]::Match($Text, '(?ms)^' + [regex]::Escape($MarkerBegin) + '.*?^' + [regex]::Escape($MarkerEnd))
	if (-not $block.Success) {
		return $null
	}
	$record = [pscustomobject]@{ Address = $null; AdapterId = ''; FirewallRules = @(); CapiThumbprint = ''; CngThumbprint = '' }
	foreach ($rawLine in ($block.Value -split "`n")) {
		$line = $rawLine.Trim()
		if ($line.StartsWith($RecordAddressPrefix)) {
			$record.Address = $line.Substring($RecordAddressPrefix.Length).Trim()
		}
		elseif ($line.StartsWith($RecordAdapterPrefix)) {
			$record.AdapterId = $line.Substring($RecordAdapterPrefix.Length).Trim()
		}
		elseif ($line.StartsWith($RecordFirewallPrefix)) {
			$record.FirewallRules = @($record.FirewallRules) + @($line.Substring($RecordFirewallPrefix.Length).Trim())
		}
		elseif ($line.StartsWith($RecordCapiPrefix)) {
			$record.CapiThumbprint = $line.Substring($RecordCapiPrefix.Length).Trim()
		}
		elseif ($line.StartsWith($RecordCngPrefix)) {
			$record.CngThumbprint = $line.Substring($RecordCngPrefix.Length).Trim()
		}
	}
	if ($null -eq $record.Address) {
		return $null
	}
	return $record
}

# Why a recorded test network ($Record, from Get-RecordedTestNetwork) cannot be one an enable run wrote, or $null when it can: an address of
# 192.0.2.0/24, a root-enumerated network device (ROOT\NET\nnnn), only this script's firewall rule names, and two thumbprints. -Revert
# removes exactly what the record names, so a record of anything else came from somewhere else and is never acted on.
function Get-TestNetworkRecordProblem($Record) {
	if ($null -ne (Get-TestAddressFormProblem $Record.Address)) {
		return "my.ini records the test address $($Record.Address), which is not a host address of 192.0.2.0/24."
	}
	if ([string]$Record.AdapterId -notmatch '^ROOT\\NET\\[0-9]{4}$') {
		return "my.ini records the test adapter '$($Record.AdapterId)', which is not a device this script creates (ROOT\NET\nnnn)."
	}
	foreach ($rule in @($Record.FirewallRules)) {
		if (@($FirewallAllowName, $FirewallBlockName) -notcontains $rule) {
			return "my.ini records the firewall rule '$rule', which is not one of this script's ($FirewallAllowName, $FirewallBlockName)."
		}
	}
	foreach ($thumbprint in @($Record.CapiThumbprint, $Record.CngThumbprint)) {
		if ([string]$thumbprint -notmatch '^[0-9A-F]{40}$') {
			return "my.ini records the test client certificate '$thumbprint', which is not a thumbprint (40 hex digits)."
		}
	}
	return $null
}

# What the enable run's checks read about this computer's network, without elevation and without changing anything.
function Get-NetworkFacts {
	$addresses = @(Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
		ForEach-Object { [pscustomobject]@{ IPAddress = [string]$_.IPAddress; InterfaceAlias = [string]$_.InterfaceAlias } })
	$routes = @(Get-NetRoute -AddressFamily IPv4 -ErrorAction SilentlyContinue |
		ForEach-Object { [pscustomobject]@{ DestinationPrefix = [string]$_.DestinationPrefix; InterfaceAlias = [string]$_.InterfaceAlias } })
	$profiles = @(Get-NetFirewallProfile -ErrorAction SilentlyContinue |
		ForEach-Object { [pscustomobject]@{ Name = [string]$_.Name; Enabled = ([string]$_.Enabled -eq 'True') } })
	$open = @(Get-NetIPInterface -AddressFamily IPv4 -ErrorAction SilentlyContinue |
		Where-Object { [string]$_.Forwarding -eq 'Enabled' -or [string]$_.WeakHostReceive -eq 'Enabled' } |
		ForEach-Object { [pscustomobject]@{ InterfaceAlias = [string]$_.InterfaceAlias; Forwarding = [string]$_.Forwarding; WeakHostReceive = [string]$_.WeakHostReceive } })
	$loopbacks = @(Get-PnpDevice -Class Net -ErrorAction SilentlyContinue | Where-Object { @($_.HardwareID) -contains $TestAdapterHardwareId } |
		ForEach-Object { [string]$_.InstanceId })
	$nameTaken = @(Get-NetAdapter -IncludeHidden -ErrorAction SilentlyContinue | Where-Object { [string]$_.Name -eq $TestAdapterName }).Count -gt 0
	$rules = @(Get-NetFirewallRule -ErrorAction SilentlyContinue | Where-Object { @($FirewallAllowName, $FirewallBlockName) -contains [string]$_.Name } |
		ForEach-Object { [string]$_.Name })
	return [pscustomobject]@{
		Addresses        = $addresses
		Routes           = $routes
		FirewallProfiles = $profiles
		OpenInterfaces   = $open
		Loopbacks        = $loopbacks
		AdapterNameTaken = $nameTaken
		RulesTaken       = $rules
		HasNetloop       = (Test-Path -LiteralPath (Join-Path $env:windir 'INF\netloop.inf') -PathType Leaf)
		HostName         = [System.Net.Dns]::GetHostName()
	}
}

# True when the Windows firewall is on in at least one profile ($Facts from Get-NetworkFacts): the run then adds its two rules.
function Test-FirewallOn($Facts) {
	return @($Facts.FirewallProfiles | Where-Object { $_.Enabled }).Count -gt 0
}

# Why the enable run cannot make the test network on this computer ($Facts from Get-NetworkFacts, $Version the server's @@version), or $null.
function Get-TestNetworkProblem([string]$Address, $Facts, [string]$Version) {
	$use = Get-TestAddressUseProblem $Address $Facts.Addresses $Facts.Routes
	if ($null -ne $use) {
		return $use
	}
	$versionMatch = [regex]::Match($Version, '^(?<major>[0-9]+)\.(?<minor>[0-9]+)')
	if (-not $versionMatch.Success -or ([int]$versionMatch.Groups['major'].Value * 1000 + [int]$versionMatch.Groups['minor'].Value) -lt 10011) {
		return "The server is $Version. A bind-address with two addresses needs MariaDB 10.11 or later."
	}
	if (-not $Facts.HasNetloop) {
		return "Windows' own loopback adapter driver (netloop.inf) is not in $env:windir\INF, so this script cannot create the adapter without a download. Report this to the lead."
	}
	if ($Facts.AdapterNameTaken) {
		return "A network adapter named '$TestAdapterName' exists already (an earlier run?). Run -Revert, or remove or rename it first."
	}
	if (@($Facts.RulesTaken).Count -gt 0) {
		return "The firewall rule $(@($Facts.RulesTaken)[0]) exists already (an earlier run?). Run -Revert, or remove it first."
	}
	$off = @($Facts.FirewallProfiles | Where-Object { -not $_.Enabled } | ForEach-Object { $_.Name })
	$open = @($Facts.OpenInterfaces)
	if ($off.Count -gt 0 -and $open.Count -gt 0) {
		return "The Windows firewall is off in the profile $($off -join ', '), and $($open[0].InterfaceAlias) forwards IPv4 ($($open[0].Forwarding)) or takes packets for other interfaces' addresses (weak host receive $($open[0].WeakHostReceive)), so another computer could reach the test address through it. Turn the firewall on in every profile first."
	}
	return $null
}

# The enable run's plan for the test network: lines to print, each line that starts a step with '#.' (the caller numbers it). Every change
# carries its tag in brackets; -Revert's plan (Get-TestNetworkRevertPlan) undoes each tag.
function Get-TestNetworkEnablePlan([string]$Address, [string]$HostName, $Facts, [string]$TlsFolder) {
	$lines = @()
	$lines += "#. [adapter] Create a Microsoft KM-TEST Loopback Adapter (hardware id $TestAdapterHardwareId, Windows' own netloop.inf from the driver"
	$lines += "     store, through the SetupAPI: no download, no devcon) and name it '$TestAdapterName'."
	$existing = @($Facts.Loopbacks)
	if ($existing.Count -gt 0) {
		$lines += "     Loopback adapters that exist already are not touched: $($existing -join ', ')."
	}
	$lines += "     [address] Give it $Address/32 and nothing else: no gateway, no DNS server, no DNS registration, NetBIOS over TCP/IP"
	$lines += '     off, DHCP, duplicate address detection, forwarding, weak host send and receive and router discovery off. No other computer'
	$lines += '     can reach the address: 192.0.2.0/24 is the documentation range, which no network routes.'
	$lines += "     [bindings] Switch off every binding of the adapter but Internet Protocol Version 4 (ms_tcpip): File and Printer Sharing,"
	$lines += '     Client for Microsoft Networks, IPv6, QoS, the link-layer discovery protocols and the rest. Then check all of it.'
	if (Test-FirewallOn $Facts) {
		$lines += "#. [firewall] Add two inbound firewall rules (group '$FirewallGroup', every profile) for TCP port 3306 on $Address only:"
		$lines += "     $FirewallAllowName allows it from $Address itself (this computer), $FirewallBlockName blocks"
		$lines += "     every other remote address ($((Get-FirewallBlockRanges $Address) -join ', '))."
	}
	else {
		$lines += '#. [firewall] The Windows firewall is off in every profile: no rule is added (no interface forwards IPv4 or receives for other'
		$lines += '     interfaces'' addresses, so no other computer can send to the test address).'
	}
	$lines += "#. [client-certificates] Make two client certificates (RSA 2048, 5 years, clientAuth) signed by the same CA, CN=$TestClientCapiName"
	$lines += "     and CN=$TestClientCngName, each as its own .pfx in $(Join-Path $TlsFolder $PfxFolderName) (Administrators only),"
	$lines += "     and import them with certutil into LocalMachine\My, not exportable: the first with its key in '$CapiProviderName'"
	$lines += "     (CAPI), the second in '$CngProviderName' (CNG). The .pfx files are deleted after the import."
	$lines += '     [key-access] Give BUILTIN\Users read access to both key files, as the connection settings tool gives (ClientCertificateStore),'
	$lines += '     so that the lead''s unelevated test run can use them. -Revert deletes both certificates with their keys.'
	$lines += "     [server-certificate] The server certificate of step 1 also names $Address and this computer's host name $HostName."
	return $lines
}

# What is there now of a recorded test network ($Record), read without elevation and without opening a key: the adapter (its hardware ids,
# its IPv4 addresses), each recorded firewall rule (its group), each recorded certificate in LocalMachine\My (its subject, and whether the
# CA in $CaPath issued it).
function Get-TestNetworkState($Record, [string]$CaPath) {
	$adapter = [pscustomobject]@{ Exists = $false; HardwareIds = @(); Addresses = @() }
	$device = @(Get-PnpDevice -InstanceId $Record.AdapterId -ErrorAction SilentlyContinue)
	if ($device.Count -gt 0) {
		$adapter.Exists = $true
		$adapter.HardwareIds = @($device[0].HardwareID | ForEach-Object { [string]$_ })
		$netAdapter = @(Get-NetAdapter -IncludeHidden -ErrorAction SilentlyContinue | Where-Object { [string]$_.PnPDeviceID -eq $Record.AdapterId })
		if ($netAdapter.Count -gt 0) {
			$adapter.Addresses = @(Get-NetIPAddress -InterfaceIndex $netAdapter[0].InterfaceIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue |
				ForEach-Object { [string]$_.IPAddress })
		}
	}
	$rules = @()
	foreach ($name in @($Record.FirewallRules)) {
		$rule = @(Get-NetFirewallRule -ErrorAction SilentlyContinue | Where-Object { [string]$_.Name -eq $name })
		if ($rule.Count -gt 0) {
			$rules += [pscustomobject]@{ Name = $name; Exists = $true; Group = [string]$rule[0].Group }
		}
		else {
			$rules += [pscustomobject]@{ Name = $name; Exists = $false; Group = '' }
		}
	}
	$ca = $null
	if ($CaPath -ne '' -and (Test-Path -LiteralPath $CaPath -PathType Leaf)) {
		$ca = Read-PemCertificate $CaPath
	}
	$certificates = @()
	foreach ($item in @(@('CAPI', $Record.CapiThumbprint, $TestClientCapiName), @('CNG', $Record.CngThumbprint, $TestClientCngName))) {
		$certificate = Find-MachineCertificate $item[1]
		$state = [pscustomobject]@{ Kind = $item[0]; Thumbprint = $item[1]; Name = $item[2]; Exists = $false; Subject = ''; IssuedByCa = $false }
		if ($null -ne $certificate) {
			$state.Exists = $true
			$state.Subject = $certificate.Subject
			$state.IssuedByCa = ($null -ne $ca) -and (Test-IssuedByCa $certificate $ca)
		}
		$certificates += $state
	}
	return [pscustomobject]@{ Adapter = $adapter; Rules = $rules; Certificates = $certificates; HasCa = ($null -ne $ca) }
}

# Why -Revert must not remove what the record names ($Record, $State from Get-TestNetworkState), or $null: an adapter that is not a
# loopback adapter, a rule of another group, a certificate of another name or not issued by the test CA are not this script's.
function Get-TestNetworkStateProblem($Record, $State) {
	if ($State.Adapter.Exists -and @($State.Adapter.HardwareIds) -notcontains $TestAdapterHardwareId) {
		return "The recorded test adapter $($Record.AdapterId) is not a loopback adapter (hardware ids $(@($State.Adapter.HardwareIds) -join ', ')), so it is not the one this script created."
	}
	foreach ($rule in @($State.Rules | Where-Object { $_.Exists })) {
		if ($rule.Group -ne $FirewallGroup) {
			return "The firewall rule $($rule.Name) is in the group '$($rule.Group)', not '$FirewallGroup', so it is not the one this script added."
		}
	}
	foreach ($certificate in @($State.Certificates | Where-Object { $_.Exists })) {
		if (-not $certificate.Subject.StartsWith("CN=$($certificate.Name),")) {
			return "The recorded certificate $($certificate.Thumbprint) in LocalMachine\My is $($certificate.Subject), not CN=$($certificate.Name), so it is not the one this script imported."
		}
		if (-not $certificate.IssuedByCa) {
			if (-not $State.HasCa) {
				return "The recorded certificate $($certificate.Thumbprint) cannot be checked against the test CA: its ca.pem in the opms-dev-tls folder is missing."
			}
			return "The recorded certificate $($certificate.Thumbprint) in LocalMachine\My was not issued by the test CA of the opms-dev-tls folder, so it is not the one this script imported."
		}
	}
	return $null
}

# -Revert's plan for the test network ($Record, $State from Get-TestNetworkState): one line or more for each tag of the enable run's plan.
function Get-TestNetworkRevertPlan($Record, $State) {
	$lines = @()
	$rules = @($State.Rules | Where-Object { $_.Exists } | ForEach-Object { $_.Name })
	if (@($Record.FirewallRules).Count -eq 0) {
		$lines += '#. [firewall] No firewall rule to remove (the enable run added none: the firewall was off).'
	}
	elseif ($rules.Count -gt 0) {
		$lines += "#. [firewall] Remove the firewall rules $($rules -join ' and ') (group '$FirewallGroup')."
	}
	else {
		$lines += "#. [firewall] The firewall rules $(@($Record.FirewallRules) -join ' and ') are not there any more: nothing to remove."
	}
	foreach ($certificate in @($State.Certificates)) {
		if ($certificate.Exists) {
			$lines += "#. [client-certificates] Remove $($certificate.Subject) ($($certificate.Thumbprint), its key in a $($certificate.Kind) provider) from"
			$lines += '     LocalMachine\My, and delete its key first.'
		}
		else {
			$lines += "#. [client-certificates] The $($certificate.Kind) certificate $($certificate.Thumbprint) is not in LocalMachine\My any more: nothing to remove."
		}
	}
	$lines += '     [key-access] With the keys their files go, BUILTIN\Users'' read access with them: the run checks that each key file is gone,'
	$lines += '     so no key that the users can read is left behind.'
	if ($State.Adapter.Exists) {
		$lines += "#. [adapter] Remove the loopback adapter $($Record.AdapterId) ('$TestAdapterName') and check that it is gone."
		$others = @($State.Adapter.Addresses | Where-Object { $_ -ne $Record.Address })
		if ($others.Count -gt 0) {
			$lines += "     It also has $($others -join ', '), which go with it."
		}
	}
	else {
		$lines += "#. [adapter] The loopback adapter $($Record.AdapterId) is not there any more: nothing to remove."
	}
	$lines += "     [address] [bindings] Its address $($Record.Address)/32 and its bindings go with the adapter; the run checks that this computer no"
	$lines += '     longer has the address.'
	$lines += "     [bind-address] Restoring my.ini (below) removes bind-address=127.0.0.1,$($Record.Address); after the restart the server listens on"
	$lines += "     127.0.0.1 only, and nothing listens on $($Record.Address):3306."
	$lines += "     [server-certificate] The server certificate that names $($Record.Address) goes with the opms-dev-tls folder (below)."
	return $lines
}

# The C# source of the adapter's creation and removal through the SetupAPI (setupapi.dll; what devcon install does, without devcon), for
# Add-Type: the .NET Framework's own compiler, nothing downloaded.
function Get-LoopbackAdapterSource {
	return @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenPmsDentalDevDbTls {
	public static class LoopbackAdapter {
		public const string HardwareId = "*MSLOOP";
		private static readonly Guid GuidDevClassNet = new Guid("4d36e972-e325-11ce-bfc1-08002be10318");
		private const int DICD_GENERATE_ID = 0x1;
		private const int SPDRP_HARDWAREID = 0x1;
		private const int SPDIT_COMPATDRIVER = 0x2;
		private const int DIF_INSTALLDEVICE = 0x2;
		private const int DIF_REMOVE = 0x5;
		private const int DIF_REGISTERDEVICE = 0x19;
		private const int DIF_INSTALLINTERFACES = 0x20;
		private const int DIF_REGISTER_COINSTALLERS = 0x22;
		private const int DI_NEEDRESTART = 0x80;
		private const int DI_NEEDREBOOT = 0x100;

		[StructLayout(LayoutKind.Sequential)]
		private struct SP_DEVINFO_DATA {
			public int cbSize;
			public Guid ClassGuid;
			public int DevInst;
			public IntPtr Reserved;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct SP_DEVINSTALL_PARAMS {
			public int cbSize;
			public int Flags;
			public int FlagsEx;
			public IntPtr hwndParent;
			public IntPtr InstallMsgHandler;
			public IntPtr InstallMsgHandlerContext;
			public IntPtr FileQueue;
			public IntPtr ClassInstallReserved;
			public int Reserved;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string DriverPath;
		}

		[DllImport("setupapi.dll", SetLastError = true)]
		private static extern IntPtr SetupDiCreateDeviceInfoList(ref Guid ClassGuid, IntPtr hwndParent);
		[DllImport("setupapi.dll", SetLastError = true)]
		private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);
		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SetupDiCreateDeviceInfoW")]
		private static extern bool SetupDiCreateDeviceInfo(IntPtr DeviceInfoSet, string DeviceName, ref Guid ClassGuid, string DeviceDescription,
			IntPtr hwndParent, int CreationFlags, ref SP_DEVINFO_DATA DeviceInfoData);
		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SetupDiOpenDeviceInfoW")]
		private static extern bool SetupDiOpenDeviceInfo(IntPtr DeviceInfoSet, string DeviceInstanceId, IntPtr hwndParent, int OpenFlags,
			ref SP_DEVINFO_DATA DeviceInfoData);
		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SetupDiSetDeviceRegistryPropertyW")]
		private static extern bool SetupDiSetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, int Property,
			byte[] PropertyBuffer, int PropertyBufferSize);
		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SetupDiGetDeviceRegistryPropertyW")]
		private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, int Property,
			out int PropertyRegDataType, byte[] PropertyBuffer, int PropertyBufferSize, out int RequiredSize);
		[DllImport("setupapi.dll", SetLastError = true)]
		private static extern bool SetupDiBuildDriverInfoList(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, int DriverType);
		[DllImport("setupapi.dll", SetLastError = true)]
		private static extern bool SetupDiSelectBestCompatDrv(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData);
		[DllImport("setupapi.dll", SetLastError = true)]
		private static extern bool SetupDiCallClassInstaller(int InstallFunction, IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData);
		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SetupDiGetDeviceInstanceIdW")]
		private static extern bool SetupDiGetDeviceInstanceId(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, StringBuilder DeviceInstanceId,
			int DeviceInstanceIdSize, out int RequiredSize);
		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "SetupDiGetDeviceInstallParamsW")]
		private static extern bool SetupDiGetDeviceInstallParams(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData,
			ref SP_DEVINSTALL_PARAMS DeviceInstallParams);

		private static SP_DEVINFO_DATA NewData() {
			SP_DEVINFO_DATA data = new SP_DEVINFO_DATA();
			data.cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
			return data;
		}

		private static Win32Exception Fail(string step) {
			int code = Marshal.GetLastWin32Error();
			return new Win32Exception(code, step + " failed: " + new Win32Exception(code).Message + " (0x" + code.ToString("X8") + ")");
		}

		private static IntPtr NewSet() {
			Guid classGuid = GuidDevClassNet;
			IntPtr set = SetupDiCreateDeviceInfoList(ref classGuid, IntPtr.Zero);
			if (set == new IntPtr(-1)) {
				throw Fail("SetupDiCreateDeviceInfoList");
			}
			return set;
		}

		///<summary>Creates a root-enumerated network device with the hardware id *MSLOOP and installs the best driver for it (Windows'
		///netloop.inf), as devcon install does. Returns its device instance id (ROOT\NET\nnnn). A device registered before a later step failed
		///is removed again before the exception leaves.</summary>
		public static string Create(string description, out bool needsReboot) {
			needsReboot = false;
			Guid classGuid = GuidDevClassNet;
			IntPtr set = NewSet();
			SP_DEVINFO_DATA data = NewData();
			bool isRegistered = false;
			try {
				if (!SetupDiCreateDeviceInfo(set, "NET", ref classGuid, description, IntPtr.Zero, DICD_GENERATE_ID, ref data)) {
					throw Fail("SetupDiCreateDeviceInfo");
				}
				byte[] ids = Encoding.Unicode.GetBytes(HardwareId + "\0\0");
				if (!SetupDiSetDeviceRegistryProperty(set, ref data, SPDRP_HARDWAREID, ids, ids.Length)) {
					throw Fail("SetupDiSetDeviceRegistryProperty (hardware id)");
				}
				if (!SetupDiBuildDriverInfoList(set, ref data, SPDIT_COMPATDRIVER)) {
					throw Fail("SetupDiBuildDriverInfoList");
				}
				if (!SetupDiSelectBestCompatDrv(set, ref data)) {
					throw Fail("SetupDiSelectBestCompatDrv (no driver for " + HardwareId + ")");
				}
				if (!SetupDiCallClassInstaller(DIF_REGISTERDEVICE, set, ref data)) {
					throw Fail("DIF_REGISTERDEVICE");
				}
				isRegistered = true;
				string instanceId = GetInstanceId(set, ref data);
				if (!SetupDiCallClassInstaller(DIF_REGISTER_COINSTALLERS, set, ref data)) {
					throw Fail("DIF_REGISTER_COINSTALLERS");
				}
				if (!SetupDiCallClassInstaller(DIF_INSTALLINTERFACES, set, ref data)) {
					throw Fail("DIF_INSTALLINTERFACES");
				}
				if (!SetupDiCallClassInstaller(DIF_INSTALLDEVICE, set, ref data)) {
					throw Fail("DIF_INSTALLDEVICE");
				}
				SP_DEVINSTALL_PARAMS installParams = new SP_DEVINSTALL_PARAMS();
				installParams.cbSize = Marshal.SizeOf(typeof(SP_DEVINSTALL_PARAMS));
				if (SetupDiGetDeviceInstallParams(set, ref data, ref installParams)) {
					needsReboot = (installParams.Flags & (DI_NEEDREBOOT | DI_NEEDRESTART)) != 0;
				}
				isRegistered = false;
				return instanceId;
			}
			finally {
				if (isRegistered) {
					SetupDiCallClassInstaller(DIF_REMOVE, set, ref data);
				}
				SetupDiDestroyDeviceInfoList(set);
			}
		}

		///<summary>The hardware ids of a device (empty when it has none). Throws when there is no such device.</summary>
		public static string[] GetHardwareIds(string instanceId) {
			IntPtr set = NewSet();
			try {
				SP_DEVINFO_DATA data = NewData();
				if (!SetupDiOpenDeviceInfo(set, instanceId, IntPtr.Zero, 0, ref data)) {
					throw Fail("SetupDiOpenDeviceInfo " + instanceId);
				}
				return ReadHardwareIds(set, ref data);
			}
			finally {
				SetupDiDestroyDeviceInfoList(set);
			}
		}

		///<summary>Removes the device, only when it is a loopback adapter (hardware id *MSLOOP); throws otherwise.</summary>
		public static void Remove(string instanceId) {
			IntPtr set = NewSet();
			try {
				SP_DEVINFO_DATA data = NewData();
				if (!SetupDiOpenDeviceInfo(set, instanceId, IntPtr.Zero, 0, ref data)) {
					throw Fail("SetupDiOpenDeviceInfo " + instanceId);
				}
				string[] ids = ReadHardwareIds(set, ref data);
				if (Array.FindIndex(ids, x => string.Equals(x, HardwareId, StringComparison.OrdinalIgnoreCase)) < 0) {
					throw new InvalidOperationException(instanceId + " is not a loopback adapter (hardware ids " + string.Join(", ", ids) + "); it is not removed.");
				}
				if (!SetupDiCallClassInstaller(DIF_REMOVE, set, ref data)) {
					throw Fail("DIF_REMOVE " + instanceId);
				}
			}
			finally {
				SetupDiDestroyDeviceInfoList(set);
			}
		}

		private static string GetInstanceId(IntPtr set, ref SP_DEVINFO_DATA data) {
			StringBuilder builder = new StringBuilder(512);
			int required;
			if (!SetupDiGetDeviceInstanceId(set, ref data, builder, builder.Capacity, out required)) {
				throw Fail("SetupDiGetDeviceInstanceId");
			}
			return builder.ToString();
		}

		private static string[] ReadHardwareIds(IntPtr set, ref SP_DEVINFO_DATA data) {
			byte[] buffer = new byte[4096];
			int type;
			int required;
			if (!SetupDiGetDeviceRegistryProperty(set, ref data, SPDRP_HARDWAREID, out type, buffer, buffer.Length, out required)) {
				return new string[0];
			}
			return Encoding.Unicode.GetString(buffer, 0, Math.Min(required, buffer.Length)).Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
'@
}

function Add-LoopbackAdapterType {
	if ($null -eq ('OpenPmsDentalDevDbTls.LoopbackAdapter' -as [type])) {
		Add-Type -TypeDefinition (Get-LoopbackAdapterSource) -Language CSharp
	}
}

# The certificate of a PEM file (the first CERTIFICATE block).
function Read-PemCertificate([string]$Path) {
	$text = [System.IO.File]::ReadAllText($Path)
	$match = [regex]::Match($text, '-----BEGIN CERTIFICATE-----(?<b>[A-Za-z0-9+/=\s]+)-----END CERTIFICATE-----')
	if (-not $match.Success) {
		throw "$Path holds no PEM certificate."
	}
	return New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(, [Convert]::FromBase64String(($match.Groups['b'].Value -replace '\s', '')))
}

# The certificate of this thumbprint in LocalMachine\My, or $null. Reading the store needs no elevation and opens no key.
function Find-MachineCertificate([string]$Thumbprint) {
	if ([string]$Thumbprint -notmatch '^[0-9A-Fa-f]{40}$') {
		return $null
	}
	$store = New-Object System.Security.Cryptography.X509Certificates.X509Store('My', [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)
	$store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadOnly -bor [System.Security.Cryptography.X509Certificates.OpenFlags]::OpenExistingOnly)
	try {
		$found = $store.Certificates.Find([System.Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint, $Thumbprint, $false)
		if ($found.Count -gt 0) {
			return $found[0]
		}
		return $null
	}
	finally {
		$store.Close()
	}
}

# True when $Ca (the test CA) issued $Certificate: the chain from it ends in that CA, signatures checked, no revocation look-up.
function Test-IssuedByCa($Certificate, $Ca) {
	$chain = New-Object System.Security.Cryptography.X509Certificates.X509Chain
	try {
		$chain.ChainPolicy.RevocationMode = [System.Security.Cryptography.X509Certificates.X509RevocationMode]::NoCheck
		$chain.ChainPolicy.VerificationFlags = [System.Security.Cryptography.X509Certificates.X509VerificationFlags]::AllowUnknownCertificateAuthority
		[void]$chain.ChainPolicy.ExtraStore.Add($Ca)
		$built = $chain.Build($Certificate)
		return $built -and $chain.ChainElements.Count -eq 2 -and $chain.ChainElements[1].Certificate.Thumbprint -eq $Ca.Thumbprint
	}
	finally {
		$chain.Reset()
	}
}

# Where a certificate's private key is: CAPI (an RSACryptoServiceProvider through the certificate's PrivateKey, its container file under
# %ProgramData%\Microsoft\Crypto\RSA\MachineKeys) or CNG (RSACng, its file under %ProgramData%\Microsoft\Crypto\Keys), as the connection
# settings tool names them (ClientCertificateStore.GetPrivateKeyFilePath). .NET Framework's GetRSAPrivateKey gives an RSACng for a CAPI
# key too, so CAPI is tried first. Opens the key: only for the two certificates this script imports. Needs elevation for a machine key.
function Get-TestKeyInfo($Certificate) {
	$capi = $null
	try {
		$capi = $Certificate.PrivateKey -as [System.Security.Cryptography.RSACryptoServiceProvider]
	}
	catch {
		$capi = $null
	}
	if ($null -ne $capi) {
		$info = $capi.CspKeyContainerInfo
		$folder = Join-Path $env:ProgramData 'Microsoft\Crypto\RSA\MachineKeys'
		return [pscustomobject]@{ Kind = 'CAPI'; Provider = $info.ProviderName; ProviderType = $info.ProviderType; Container = $info.KeyContainerName
			IsMachine = $info.MachineKeyStore; File = (Join-Path $folder $info.UniqueKeyContainerName) }
	}
	$rsa = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($Certificate)
	if ($rsa -is [System.Security.Cryptography.RSACng]) {
		try {
			$key = $rsa.Key
			return [pscustomobject]@{ Kind = 'CNG'; Provider = $key.Provider.Provider; ProviderType = 0; Container = $key.KeyName; IsMachine = $key.IsMachineKey
				File = (Join-Path $env:ProgramData ('Microsoft\Crypto\Keys\' + $key.UniqueName)) }
		}
		finally {
			$rsa.Dispose()
		}
	}
	throw "The private key of $($Certificate.Subject) ($($Certificate.Thumbprint)) does not open as a CAPI or CNG RSA key."
}

# BUILTIN\Users read access to a key file, as ClientCertificateStore.GrantUsersReadToPrivateKey gives it.
function Grant-UsersReadToKeyFile([string]$Path) {
	$users = New-Object System.Security.Principal.SecurityIdentifier('S-1-5-32-545')
	$security = [System.IO.File]::GetAccessControl($Path)
	$security.AddAccessRule((New-Object System.Security.AccessControl.FileSystemAccessRule($users, [System.Security.AccessControl.FileSystemRights]::Read, [System.Security.AccessControl.AccessControlType]::Allow)))
	[System.IO.File]::SetAccessControl($Path, $security)
}

# Runs certutil.exe (Windows' own) with these arguments; the output with $Secret replaced, so the .pfx password is never printed.
function Invoke-Certutil([string[]]$Arguments, [string]$Secret) {
	$startInfo = New-Object System.Diagnostics.ProcessStartInfo
	$startInfo.FileName = Join-Path $env:windir 'System32\certutil.exe'
	$startInfo.Arguments = Join-Arguments $Arguments
	$startInfo.UseShellExecute = $false
	$startInfo.RedirectStandardOutput = $true
	$startInfo.RedirectStandardError = $true
	$startInfo.CreateNoWindow = $true
	$process = [System.Diagnostics.Process]::Start($startInfo)
	$stdoutTask = $process.StandardOutput.ReadToEndAsync()
	$stderr = $process.StandardError.ReadToEnd()
	$process.WaitForExit()
	$output = ($stdoutTask.Result + ' ' + $stderr).Trim()
	if ($Secret -ne '') {
		$output = $output.Replace($Secret, '<password>')
	}
	return [pscustomobject]@{ ExitCode = $process.ExitCode; Output = ($output -replace '\s+', ' ') }
}

# Imports one test client .pfx into LocalMachine\My with its key in $Provider (certutil -csp, not exportable, no chain), checks where the key
# landed ($Kind, the computer's key set) and gives BUILTIN\Users read access to its file. Returns the key's information.
function Import-TestClientCertificate([string]$PfxPath, [string]$Password, [string]$Provider, [string]$Kind, [string]$Thumbprint) {
	$result = Invoke-Certutil @('-f', '-p', $Password, '-csp', $Provider, '-importPFX', 'My', $PfxPath, 'NoExport,NoChain,NoRoot') $Password
	if ($result.ExitCode -ne 0) {
		throw "certutil could not import $PfxPath into LocalMachine\My with the provider $Provider (exit code $($result.ExitCode)): $($result.Output)"
	}
	$certificate = Find-MachineCertificate $Thumbprint
	if ($null -eq $certificate) {
		throw "After certutil's import of $PfxPath, LocalMachine\My has no certificate $Thumbprint."
	}
	$info = Get-TestKeyInfo $certificate
	if ($info.Kind -ne $Kind -or $info.Provider -ne $Provider -or -not $info.IsMachine) {
		throw "The key of $Thumbprint is in the $($info.Kind) provider '$($info.Provider)' (the computer's key set: $($info.IsMachine)), not in the $Kind provider '$Provider' of the computer's key set."
	}
	if (-not (Test-Path -LiteralPath $info.File -PathType Leaf)) {
		throw "The key file of $Thumbprint, $($info.File), was not found."
	}
	Grant-UsersReadToKeyFile $info.File
	return $info
}

# Removes a test client certificate from LocalMachine\My with its key: the key first (CAPI: its container through the CSP; CNG: CngKey.Delete),
# then the store entry, then checks that both are gone. '' when gone (or it was not there), else what is left.
function Remove-TestClientCertificate([string]$Thumbprint) {
	$certificate = Find-MachineCertificate $Thumbprint
	if ($null -eq $certificate) {
		return ''
	}
	$file = ''
	if ($certificate.HasPrivateKey) {
		$info = Get-TestKeyInfo $certificate
		$file = $info.File
		if ($info.Kind -eq 'CAPI') {
			$parameters = New-Object System.Security.Cryptography.CspParameters($info.ProviderType, $info.Provider, $info.Container)
			$parameters.Flags = [System.Security.Cryptography.CspProviderFlags]::UseMachineKeyStore -bor [System.Security.Cryptography.CspProviderFlags]::UseExistingKey
			$csp = New-Object System.Security.Cryptography.RSACryptoServiceProvider($parameters)
			$csp.PersistKeyInCsp = $false
			$csp.Clear()
		}
		else {
			$key = [System.Security.Cryptography.CngKey]::Open($info.Container, (New-Object System.Security.Cryptography.CngProvider($info.Provider)), [System.Security.Cryptography.CngKeyOpenOptions]::MachineKey)
			$key.Delete()
		}
	}
	$store = New-Object System.Security.Cryptography.X509Certificates.X509Store('My', [System.Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)
	$store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite -bor [System.Security.Cryptography.X509Certificates.OpenFlags]::OpenExistingOnly)
	try {
		$store.Remove($certificate)
	}
	finally {
		$store.Close()
	}
	$left = @()
	if ($null -ne (Find-MachineCertificate $Thumbprint)) {
		$left += "the certificate $Thumbprint is still in LocalMachine\My"
	}
	if ($file -ne '' -and (Test-Path -LiteralPath $file)) {
		$left += "its key file $file is still there"
	}
	return ($left -join '; ')
}

# Waits until the new adapter shows as a network adapter, and returns it.
function Wait-TestNetAdapter([string]$InstanceId) {
	$deadline = [DateTime]::UtcNow.AddSeconds(60)
	while ([DateTime]::UtcNow -lt $deadline) {
		$adapter = @(Get-NetAdapter -IncludeHidden -ErrorAction SilentlyContinue | Where-Object { [string]$_.PnPDeviceID -eq $InstanceId })
		if ($adapter.Count -gt 0) {
			return $adapter[0]
		}
		Start-Sleep -Milliseconds 500
	}
	throw "The new loopback adapter $InstanceId did not show as a network adapter within 60 seconds."
}

# Steps a and b: names the new adapter, switches off every binding but IPv4, and gives it the address and nothing else. Returns its
# interface index.
function Set-TestAdapter([string]$InstanceId, [string]$Address) {
	$adapter = Wait-TestNetAdapter $InstanceId
	Rename-NetAdapter -Name $adapter.Name -NewName $TestAdapterName -Confirm:$false
	$index = [int]$adapter.InterfaceIndex
	foreach ($binding in @(Get-NetAdapterBinding -Name $TestAdapterName -ErrorAction SilentlyContinue | Where-Object { $_.ComponentID -ne 'ms_tcpip' -and $_.Enabled })) {
		Disable-NetAdapterBinding -Name $TestAdapterName -ComponentID $binding.ComponentID -Confirm:$false
	}
	Set-NetIPInterface -InterfaceIndex $index -AddressFamily IPv4 -Dhcp Disabled -DadTransmits 0 -Forwarding Disabled -WeakHostSend Disabled -WeakHostReceive Disabled -RouterDiscovery Disabled
	New-NetIPAddress -InterfaceIndex $index -AddressFamily IPv4 -IPAddress $Address -PrefixLength 32 | Out-Null
	Set-DnsClient -InterfaceIndex $index -RegisterThisConnectionsAddress $false
	Set-DnsClientServerAddress -InterfaceIndex $index -ResetServerAddresses
	$configuration = Get-CimInstance -ClassName Win32_NetworkAdapterConfiguration -Filter "InterfaceIndex=$index"
	$result = Invoke-CimMethod -InputObject $configuration -MethodName SetTcpipNetbios -Arguments @{ TcpipNetbiosOptions = [uint32]2 }
	if ([int]$result.ReturnValue -ne 0 -and [int]$result.ReturnValue -ne 1) {
		throw "Switching off NetBIOS over TCP/IP on the test adapter failed (SetTcpipNetbios returned $($result.ReturnValue))."
	}
	return $index
}

# Why the configured adapter (interface $Index) is not as steps a and b make it, or $null: its only IPv4 address is $Address/32 (preferred), no
# gateway, no DNS server, no DNS registration, DHCP, forwarding and weak host receive off, no binding but IPv4. Waits up to 20 seconds.
function Get-TestAdapterProblem([int]$Index, [string]$Address) {
	$deadline = [DateTime]::UtcNow.AddSeconds(20)
	$problem = 'not checked'
	while ([DateTime]::UtcNow -lt $deadline) {
		$problem = $null
		$addresses = @(Get-NetIPAddress -InterfaceIndex $Index -AddressFamily IPv4 -ErrorAction SilentlyContinue)
		if ($addresses.Count -ne 1 -or [string]$addresses[0].IPAddress -ne $Address -or [int]$addresses[0].PrefixLength -ne 32 -or [string]$addresses[0].AddressState -ne 'Preferred') {
			$problem = "its IPv4 addresses are $(@($addresses | ForEach-Object { [string]$_.IPAddress + '/' + $_.PrefixLength + ' ' + $_.AddressState }) -join ', '), not $Address/32 alone (Preferred)"
		}
		elseif (@(Get-NetRoute -InterfaceIndex $Index -AddressFamily IPv4 -ErrorAction SilentlyContinue | Where-Object { [string]$_.NextHop -ne '0.0.0.0' }).Count -gt 0) {
			$problem = 'it has a gateway'
		}
		elseif (@((Get-DnsClientServerAddress -InterfaceIndex $Index -AddressFamily IPv4 -ErrorAction SilentlyContinue).ServerAddresses | Where-Object { $_ }).Count -gt 0) {
			$problem = 'it has a DNS server'
		}
		elseif ((Get-DnsClient -InterfaceIndex $Index -ErrorAction SilentlyContinue).RegisterThisConnectionsAddress) {
			$problem = 'it registers its address in DNS'
		}
		else {
			$interface = Get-NetIPInterface -InterfaceIndex $Index -AddressFamily IPv4
			if ([string]$interface.Dhcp -ne 'Disabled' -or [string]$interface.Forwarding -ne 'Disabled' -or [string]$interface.WeakHostReceive -ne 'Disabled') {
				$problem = "DHCP $($interface.Dhcp), forwarding $($interface.Forwarding), weak host receive $($interface.WeakHostReceive)"
			}
			else {
				$bound = @(Get-NetAdapterBinding -Name $TestAdapterName -ErrorAction SilentlyContinue | Where-Object { $_.Enabled -and $_.ComponentID -ne 'ms_tcpip' } | ForEach-Object { $_.ComponentID })
				if ($bound.Count -gt 0) {
					$problem = "bindings still on: $($bound -join ', ')"
				}
			}
		}
		if ($null -eq $problem) {
			return $null
		}
		Start-Sleep -Milliseconds 500
	}
	return "The test adapter is not as this script set it up: $problem."
}

# Step c: the two firewall rules. Each name goes to $Created.FirewallRules before the rule is added, so a failed run removes it.
function Add-TestFirewallRules([string]$Address, $Created) {
	$common = @{ Group = $FirewallGroup; Direction = 'Inbound'; Protocol = 'TCP'; LocalPort = '3306'; LocalAddress = $Address; Profile = 'Any'; Enabled = 'True' }
	$Created.FirewallRules = @($Created.FirewallRules) + @($FirewallAllowName)
	New-NetFirewallRule -Name $FirewallAllowName -DisplayName "Open PMS - Dental local test TLS: MariaDB on $Address from this computer" -Action Allow -RemoteAddress $Address @common | Out-Null
	$Created.FirewallRules = @($Created.FirewallRules) + @($FirewallBlockName)
	New-NetFirewallRule -Name $FirewallBlockName -DisplayName "Open PMS - Dental local test TLS: MariaDB on $Address from anywhere else" -Action Block -RemoteAddress (Get-FirewallBlockRanges $Address) @common | Out-Null
}

# Removes one of this script's firewall rules (only of its group); nothing when it is not there.
function Remove-TestFirewallRule([string]$Name) {
	$rule = @(Get-NetFirewallRule -ErrorAction SilentlyContinue | Where-Object { [string]$_.Name -eq $Name })
	if ($rule.Count -eq 0) {
		return
	}
	if ([string]$rule[0].Group -ne $FirewallGroup) {
		throw "The firewall rule $Name is in the group '$($rule[0].Group)', not '$FirewallGroup'; it is not removed."
	}
	Remove-NetFirewallRule -Name $Name
}

# Removes the test adapter (only a loopback adapter: LoopbackAdapter.Remove checks it) and waits until neither the device nor the address
# is there; nothing when the device is not there.
function Remove-TestAdapter([string]$InstanceId, [string]$Address) {
	if (@(Get-PnpDevice -InstanceId $InstanceId -ErrorAction SilentlyContinue).Count -gt 0) {
		Add-LoopbackAdapterType
		[OpenPmsDentalDevDbTls.LoopbackAdapter]::Remove($InstanceId)
	}
	$deadline = [DateTime]::UtcNow.AddSeconds(30)
	while ([DateTime]::UtcNow -lt $deadline) {
		$device = @(Get-PnpDevice -InstanceId $InstanceId -ErrorAction SilentlyContinue)
		$address = @(Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue | Where-Object { [string]$_.IPAddress -eq $Address })
		if ($device.Count -eq 0 -and $address.Count -eq 0) {
			return
		}
		Start-Sleep -Milliseconds 500
	}
	throw "After its removal, the test adapter $InstanceId or the address $Address is still there."
}

# Steps a, b, c and e of an enable run with -WithTestNetwork (the certificates in $Set, the paths and the test network in $Paths). Records
# in $Created what it made, as it makes it, so that a failed run undoes exactly that (Undo-TestNetwork); fills in $Paths.TestNetwork's
# adapter for the record in my.ini.
function Install-TestNetwork($Paths, $Set, $Created) {
	$net = $Paths.TestNetwork
	Add-LoopbackAdapterType
	$needsReboot = $false
	$instanceId = [OpenPmsDentalDevDbTls.LoopbackAdapter]::Create($TestAdapterName, [ref]$needsReboot)
	$Created.AdapterId = $instanceId
	$net.AdapterId = $instanceId
	Write-Host "Created the loopback adapter $instanceId."
	if ($needsReboot) {
		throw "Windows asks for a restart to finish the loopback adapter $instanceId; this script does not restart the computer."
	}
	$index = Set-TestAdapter $instanceId $net.Address
	$problem = Get-TestAdapterProblem $index $net.Address
	if ($null -ne $problem) {
		throw $problem
	}
	Write-Host "Gave it $($net.Address)/32 (no gateway, no DNS, only IPv4 bound)."
	if (@($net.FirewallRules).Count -gt 0) {
		Add-TestFirewallRules $net.Address $Created
		Write-Host "Added the firewall rules $(@($net.FirewallRules) -join ' and ')."
	}
	$pfxFolder = Join-Path $Paths.TlsFolder $PfxFolderName
	New-Item -ItemType Directory -Path $pfxFolder | Out-Null
	$Created.PfxFolder = $pfxFolder
	Set-ProtectedAcl $pfxFolder @() @()
	foreach ($item in @(@('CAPI', $CapiProviderName, $Set.TestCapiPfx, $Set.TestCapiThumbprint, 'test-client-capi.pfx'), @('CNG', $CngProviderName, $Set.TestCngPfx, $Set.TestCngThumbprint, 'test-client-cng.pfx'))) {
		$pfxPath = Join-Path $pfxFolder $item[4]
		[System.IO.File]::WriteAllBytes($pfxPath, [byte[]]$item[2])
		$Created.Thumbprints = @($Created.Thumbprints) + @($item[3])
		$info = Import-TestClientCertificate $pfxPath $Set.TestPfxPassword $item[1] $item[0] $item[3]
		Write-Host "Imported $($item[3]) into LocalMachine\My, its key in '$($info.Provider)' ($($info.File), BUILTIN\Users read)."
	}
	Remove-Item -LiteralPath $pfxFolder -Recurse -Force
	$Created.PfxFolder = ''
}

# Undoes what $Created names (a failed enable run's own, or -Revert's record): the firewall rules, the certificates with their keys, the
# adapter with its address, the .pfx folder. Returns what could not be undone (empty when everything is gone).
function Undo-TestNetwork($Created, [string]$Address) {
	$problems = @()
	foreach ($rule in @($Created.FirewallRules | Where-Object { $_ })) {
		try {
			Remove-TestFirewallRule $rule
		}
		catch {
			$problems += "the firewall rule ${rule}: $($_.Exception.Message)"
		}
	}
	foreach ($thumbprint in @($Created.Thumbprints | Where-Object { $_ })) {
		try {
			$left = Remove-TestClientCertificate $thumbprint
			if ($left -ne '') {
				$problems += $left
			}
		}
		catch {
			$problems += "the certificate $thumbprint in LocalMachine\My (with its key): $($_.Exception.Message)"
		}
	}
	if ([string]$Created.AdapterId -ne '') {
		try {
			Remove-TestAdapter $Created.AdapterId $Address
		}
		catch {
			$problems += "the loopback adapter $($Created.AdapterId): $($_.Exception.Message)"
		}
	}
	if ([string]$Created.PfxFolder -ne '' -and (Test-Path -LiteralPath $Created.PfxFolder)) {
		try {
			Remove-Item -LiteralPath $Created.PfxFolder -Recurse -Force
		}
		catch {
			$problems += "the folder $($Created.PfxFolder) (the .pfx files): $($_.Exception.Message)"
		}
	}
	return , $problems
}

# Why the service's process ($ServicePid) does not listen on $Address:3306 within 30 seconds after a restart, or $null when it does.
function Get-TestListenerProblem([string]$Address, [int]$ServicePid) {
	$deadline = [DateTime]::UtcNow.AddSeconds(30)
	while ([DateTime]::UtcNow -lt $deadline) {
		$listeners = @(Get-NetTCPConnection -LocalAddress $Address -LocalPort 3306 -State Listen -ErrorAction SilentlyContinue)
		if (@($listeners | Where-Object { [int]$_.OwningProcess -eq $ServicePid }).Count -gt 0) {
			return $null
		}
		Start-Sleep -Milliseconds 500
	}
	return "The service's process $ServicePid does not listen on ${Address}:3306."
}

# The three values of TlsCertificateLoginTests (docs/port-notes/S2-2.md section 6), as PowerShell lines to paste.
function Write-TestVariables([string]$Address, [string]$CapiThumbprint, [string]$CngThumbprint) {
	Write-Host 'The values for TlsCertificateLoginTests (docs/port-notes/DB-TLS.md section 11), to paste into the lead''s PowerShell window:'
	Write-Host "  `$env:OPMS_DB_TLS_TEST_SERVER='$Address'"
	Write-Host "  `$env:OPMS_DB_TLS_TEST_CAPI_THUMBPRINT='$CapiThumbprint'"
	Write-Host "  `$env:OPMS_DB_TLS_TEST_CNG_THUMBPRINT='$CngThumbprint'"
}

#endregion

#region Modes

# The default client folder: DevDbTlsClient beside the dev folder (%LOCALAPPDATA%\OpenPmsDental\DevDbTlsClient for the default).
function Get-DefaultClientFolder([string]$DevPath) {
	$parent = Split-Path -Parent $DevPath
	if ([string]::IsNullOrEmpty($parent)) {
		Stop-Refused "-DevFolder $DevPath has no parent folder for the client folder. Pass -ClientFolder."
	}
	return Join-Path $parent $ClientFolderName
}

# True when the two folders are the same or one is inside the other.
function Test-FolderOverlap([string]$First, [string]$Second) {
	$a = (Get-NormalPath $First) + '\'
	$b = (Get-NormalPath $Second) + '\'
	return $a.StartsWith($b) -or $b.StartsWith($a)
}

# The folders that the block of this script in my.ini records (the comment lines $RecordDevPrefix and $RecordClientPrefix), each $null when
# the block does not record it or there is no block, and the parent folders the enable run created for them ($RecordParentPrefix lines).
function Get-RecordedFolders([string]$Text) {
	$recorded = [pscustomobject]@{ DevFolder = $null; ClientFolder = $null; CreatedParents = @() }
	$block = [regex]::Match($Text, '(?ms)^' + [regex]::Escape($MarkerBegin) + '.*?^' + [regex]::Escape($MarkerEnd))
	if (-not $block.Success) {
		return $recorded
	}
	foreach ($rawLine in ($block.Value -split "`n")) {
		$line = $rawLine.Trim()
		if ($line.StartsWith($RecordDevPrefix) -and $line.Length -gt $RecordDevPrefix.Length) {
			$recorded.DevFolder = $line.Substring($RecordDevPrefix.Length).Trim().Replace('/', '\')
		}
		elseif ($line.StartsWith($RecordClientPrefix) -and $line.Length -gt $RecordClientPrefix.Length) {
			$recorded.ClientFolder = $line.Substring($RecordClientPrefix.Length).Trim().Replace('/', '\')
		}
		elseif ($line.StartsWith($RecordParentPrefix) -and $line.Length -gt $RecordParentPrefix.Length) {
			$recorded.CreatedParents = @($recorded.CreatedParents) + @($line.Substring($RecordParentPrefix.Length).Trim().Replace('/', '\'))
		}
	}
	return $recorded
}

# Why the folders of a -Revert or -RequireSecureTransport run ($Folders, from Resolve-RecordedFolders) cannot be ones an enable run wrote,
# or $null when they can: the dev folder must be <one of $LocalRoots>\OpenPmsDental\DevDbTls, the client folder (when there is one)
# <one of $LocalRoots>\OpenPmsDental\DevDbTlsClient, and each recorded created parent the parent folder of one of them. An enable run
# accepts no other folder, so a record of any other came from somewhere else, and this script never removes files there.
function Get-RecordProblem($Folders, [string[]]$LocalRoots) {
	$source = '-DevFolder (or this account''s default)'
	if ($Folders.IsRecorded) {
		$source = 'the record in my.ini'
	}
	if (-not (Test-ProfileFolder $Folders.DevFolder $DevFolderName $LocalRoots)) {
		return "The dev folder $($Folders.DevFolder), from $source, is not <a user profile>\AppData\Local\$AppFolderName\$DevFolderName of this computer, the only dev folder this script writes to."
	}
	$parents = @((Get-NormalPath (Split-Path -Parent (Get-CanonicalPath $Folders.DevFolder))))
	if ($Folders.ClientFolder -ne '') {
		if (-not (Test-ProfileFolder $Folders.ClientFolder $ClientFolderName $LocalRoots)) {
			return "The client folder $($Folders.ClientFolder), from $source, is not <a user profile>\AppData\Local\$AppFolderName\$ClientFolderName of this computer, the only client folder this script writes to."
		}
		$parents += Get-NormalPath (Split-Path -Parent (Get-CanonicalPath $Folders.ClientFolder))
	}
	foreach ($created in @($Folders.CreatedParents)) {
		$canonical = Get-CanonicalPath $created
		if ($null -eq $canonical -or $parents -notcontains (Get-NormalPath $canonical)) {
			return "my.ini records the created parent folder $created, which is not the parent of the recorded dev or client folder."
		}
	}
	return $null
}

# Why $Path cannot be the dev folder or the client folder ($Label, -DevFolder or -ClientFolder) of an enable run, or $null when it can.
# It must be <one of $LocalRoots>\OpenPmsDental\$Name, in an AppData\Local that exists (the only form -Revert and -RequireSecureTransport
# accept from the record); it must not be a drive root, hold a user profile, or be or overlap a folder of $Protected (the MariaDB folder,
# the server's data folder, the opms-dev-tls folder: name and path); and when it exists it holds nothing but files of $OwnNames (the
# files this script writes there), so the run replaces only its own files and its ACL hides nothing of anyone else's.
function Get-FolderProblem([string]$Label, [string]$Path, [string]$Name, [string[]]$OwnNames, [System.Collections.IDictionary]$Protected, [string[]]$LocalRoots) {
	$canonical = Get-CanonicalPath $Path
	if ($null -eq $canonical) {
		return "$Label $Path is not a full path on a drive (C:\...)."
	}
	if ((Get-NormalPath $canonical) -eq (Get-NormalPath ([System.IO.Path]::GetPathRoot($canonical)))) {
		return "$Label $canonical is a drive root."
	}
	foreach ($root in @($LocalRoots)) {
		$profileRoot = Split-Path -Parent (Split-Path -Parent $root)
		if (((Get-NormalPath $profileRoot) + '\').StartsWith((Get-NormalPath $canonical) + '\')) {
			return "$Label $canonical is or holds the user profile $profileRoot."
		}
	}
	foreach ($key in @($Protected.Keys)) {
		$value = [string]$Protected[$key]
		if ($value -ne '' -and (Test-FolderOverlap $canonical $value)) {
			return "$Label $canonical is or overlaps $key $value."
		}
	}
	if (-not (Test-ProfileFolder $canonical $Name $LocalRoots)) {
		return "$Label $canonical is not <a user profile>\AppData\Local\$AppFolderName\$Name of this computer. Only such a folder is accepted: -Revert and -RequireSecureTransport take no other from the record in my.ini."
	}
	$local = Split-Path -Parent (Split-Path -Parent $canonical)
	if (-not (Test-Path -LiteralPath $local -PathType Container)) {
		return "$Label $canonical is in $local, which does not exist (has that account signed in on this computer?)."
	}
	if (Test-Path -LiteralPath $canonical) {
		if (-not (Test-Path -LiteralPath $canonical -PathType Container)) {
			return "$Label $canonical is a file."
		}
		$others = @(Get-ChildItem -LiteralPath $canonical -Force | Where-Object { $_.PSIsContainer -or $OwnNames -notcontains $_.Name } | ForEach-Object { $_.Name })
		if ($others.Count -gt 0) {
			return "$Label $canonical exists and holds $($others -join ', '), which this script did not write (it writes only $($OwnNames -join ' and ') there). Move them away first."
		}
	}
	return $null
}

# The dev folder and the client folder ('' for none) of a -Revert or -RequireSecureTransport run. The enable run recorded in my.ini where it
# wrote them, for whichever account (-DevUser, -DevFolder); they are taken from there, never recomputed from the account that runs this, and
# a -DevFolder or -ClientFolder that names another folder is refused. Only without a record (no block, or one without the line) the folder
# is -DevFolder, else $DefaultDev, and the client folder -ClientFolder, else none; IsRecorded then says so.
function Resolve-RecordedFolders($Recorded, [string]$GivenDev, [string]$GivenClient, [string]$DefaultDev) {
	if ($null -eq $Recorded.DevFolder) {
		$dev = $GivenDev
		if ($dev -eq '') {
			$dev = $DefaultDev
		}
		$client = ''
		if ($GivenClient -ne '') {
			$client = Resolve-FullPath $GivenClient
		}
		return [pscustomobject]@{ DevFolder = (Resolve-FullPath $dev); ClientFolder = $client; IsRecorded = $false; CreatedParents = @() }
	}
	if ($GivenDev -ne '' -and (Get-NormalPath (Resolve-FullPath $GivenDev)) -ne (Get-NormalPath $Recorded.DevFolder)) {
		Stop-Refused "my.ini records the dev folder $($Recorded.DevFolder), where the enable run wrote ca.pem, but -DevFolder names $GivenDev. Leave -DevFolder out: this mode uses the recorded folder."
	}
	$client = ''
	if ($null -ne $Recorded.ClientFolder) {
		$client = $Recorded.ClientFolder
	}
	if ($GivenClient -ne '' -and (Get-NormalPath (Resolve-FullPath $GivenClient)) -ne (Get-NormalPath $client)) {
		if ($client -eq '') {
			Stop-Refused "my.ini records no client folder (the enable run wrote no client certificate), but -ClientFolder names $GivenClient. Leave -ClientFolder out."
		}
		Stop-Refused "my.ini records the client folder $client, but -ClientFolder names $GivenClient. Leave -ClientFolder out: this mode uses the recorded folder."
	}
	$parents = @()
	if ($Recorded.PSObject.Properties['CreatedParents']) {
		$parents = @($Recorded.CreatedParents | Where-Object { $_ })
	}
	return [pscustomobject]@{ DevFolder = $Recorded.DevFolder; ClientFolder = $client; IsRecorded = $true; CreatedParents = $parents }
}

# The paths of a run. The enable run takes -DevFolder and -ClientFolder (or their defaults) and notes the parent folders it will create for
# them; the other modes take what my.ini records and refuse a record that no enable run can have written (Get-RecordProblem).
function Get-Paths([bool]$ForEnable) {
	$tlsFolder = Join-Path $script:Facts.Home $TlsFolderName
	$defaultDev = Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) 'OpenPmsDental\DevDbTls'
	# The test network: the enable run fills it in (Invoke-Enable); the other modes take the record and refuse one no enable run wrote.
	$testNetwork = $null
	if (-not $ForEnable) {
		$testNetwork = Get-RecordedTestNetwork $script:IniFile.Text
		if ($null -ne $testNetwork) {
			$netProblem = Get-TestNetworkRecordProblem $testNetwork
			if ($null -ne $netProblem) {
				Stop-Refused "$netProblem This script does not touch it: something else wrote that line of its block in $($script:Facts.Ini). Look at the block, restore my.ini from $($script:Facts.Ini + $BackupSuffix) by hand and undo the test network yourself (docs/port-notes/DB-TLS.md section 11)."
			}
		}
	}
	if ($ForEnable) {
		$dev = $DevFolder
		if ($dev -eq '') {
			$dev = $defaultDev
		}
		$dev = Resolve-FullPath $dev
		$client = ''
		if ($WithClientCertificate) {
			$client = $ClientFolder
			if ($client -eq '') {
				$client = Get-DefaultClientFolder $dev
			}
			$client = Resolve-FullPath $client
		}
		$created = @()
		foreach ($folder in @(@($dev, $client) | Where-Object { $_ -ne '' })) {
			$parent = Split-Path -Parent $folder
			if ($parent -ne '' -and -not (Test-Path -LiteralPath $parent) -and @($created | Where-Object { (Get-NormalPath $_) -eq (Get-NormalPath $parent) }).Count -eq 0) {
				$created += $parent
			}
		}
		$folders = [pscustomobject]@{ DevFolder = $dev; ClientFolder = $client; IsRecorded = $false; CreatedParents = $created }
	}
	else {
		$folders = Resolve-RecordedFolders (Get-RecordedFolders $script:IniFile.Text) $DevFolder $ClientFolder $defaultDev
		$problem = Get-RecordProblem $folders (Get-LocalAppDataRoots)
		if ($null -ne $problem) {
			if ($folders.IsRecorded) {
				Stop-Refused "$problem This script does not touch it: something else wrote that line of its block in $($script:Facts.Ini). Look at the block, restore my.ini from $($script:Facts.Ini + $BackupSuffix) by hand and remove the files the enable run wrote yourself."
			}
			Stop-Refused "$problem Pass -DevFolder (and -ClientFolder) with the folders the enable run wrote to, or remove its files by hand."
		}
		$folders.DevFolder = Get-CanonicalPath $folders.DevFolder
		if ($folders.ClientFolder -ne '') {
			$folders.ClientFolder = Get-CanonicalPath $folders.ClientFolder
		}
		$folders.CreatedParents = @($folders.CreatedParents | ForEach-Object { Get-CanonicalPath $_ })
	}
	$clientCert = ''
	$clientKey = ''
	if ($folders.ClientFolder -ne '') {
		$clientCert = Join-Path $folders.ClientFolder 'client-cert.pem'
		$clientKey = Join-Path $folders.ClientFolder 'client-key.pem'
	}
	return [pscustomobject]@{
		TlsFolder    = $tlsFolder
		Ca           = Join-Path $tlsFolder 'ca.pem'
		ServerCert   = Join-Path $tlsFolder 'server-cert.pem'
		ServerKey    = Join-Path $tlsFolder 'server-key.pem'
		DevFolder    = $folders.DevFolder
		DevCa        = Join-Path $folders.DevFolder 'ca.pem'
		ClientFolder = $folders.ClientFolder
		ClientCert   = $clientCert
		ClientKey    = $clientKey
		IsRecorded   = $folders.IsRecorded
		CreatedParents = @($folders.CreatedParents)
		Backup       = $script:Facts.Ini + $BackupSuffix
		IniTemp      = $script:Facts.Ini + $IniTempSuffix
		TestNetwork  = $testNetwork
	}
}

# The parent folders that the enable run creates (or created) for the dev and client folders, as $Paths names them.
function Get-CreatedParents($Paths) {
	if (-not $Paths.PSObject.Properties['CreatedParents']) {
		return @()
	}
	return @($Paths.CreatedParents | Where-Object { $_ })
}

# The block of this script in my.ini: the record of the dev folder (and the client folder, and a parent folder the enable run created for
# them) as comment lines, then the TLS options.
function Get-BlockLines($Paths, [bool]$WithRequire) {
	$lines = @(
		$MarkerBegin,
		"$RecordDevPrefix$($Paths.DevFolder.Replace('\', '/'))"
	)
	if ($Paths.ClientFolder -ne '') {
		$lines += "$RecordClientPrefix$($Paths.ClientFolder.Replace('\', '/'))"
	}
	foreach ($parent in (Get-CreatedParents $Paths)) {
		$lines += "$RecordParentPrefix$($parent.Replace('\', '/'))"
	}
	# The test network (-WithTestNetwork): the record of what the run made, for -Revert, and the second address for the server.
	$net = $null
	if ($Paths.PSObject.Properties['TestNetwork']) {
		$net = $Paths.TestNetwork
	}
	if ($null -ne $net) {
		$lines += "$RecordAddressPrefix$($net.Address)"
		$lines += "$RecordAdapterPrefix$($net.AdapterId)"
		foreach ($rule in @($net.FirewallRules | Where-Object { $_ })) {
			$lines += "$RecordFirewallPrefix$rule"
		}
		$lines += "$RecordCapiPrefix$($net.CapiThumbprint)"
		$lines += "$RecordCngPrefix$($net.CngThumbprint)"
	}
	$lines += @(
		"ssl_ca=$($Paths.Ca.Replace('\', '/'))",
		"ssl_cert=$($Paths.ServerCert.Replace('\', '/'))",
		"ssl_key=$($Paths.ServerKey.Replace('\', '/'))",
		'tls_version=TLSv1.2,TLSv1.3'
	)
	if ($null -ne $net) {
		$lines += "bind-address=127.0.0.1,$($net.Address)"
	}
	if ($WithRequire) {
		$lines += 'require_secure_transport=ON'
	}
	$lines += $MarkerEnd
	return $lines
}

# The files the script writes outside the opms-dev-tls folder, as $Paths names them.
function Get-DevFiles($Paths) {
	return @(@($Paths.DevCa, $Paths.ClientCert, $Paths.ClientKey) | Where-Object { $_ -ne '' })
}

# How the plan describes a dev or client folder before the ACL it gets.
function Get-FolderPlanText([string]$Folder) {
	if (Test-Path -LiteralPath $Folder) {
		return 'The folder exists (it holds only files of this script); its ACL is replaced with'
	}
	return 'The folder is created with'
}

function Remove-EmptyFolder([string]$Folder) {
	if ($Folder -ne '' -and (Test-Path -LiteralPath $Folder -PathType Container) -and @(Get-ChildItem -LiteralPath $Folder -Force).Count -eq 0) {
		Remove-Item -LiteralPath $Folder -Force
	}
}

# -Revert's removal: the files the script wrote outside the opms-dev-tls folder, the dev and client folders when they are then empty, the
# parent folders the enable run created for them when they are then empty, and the opms-dev-tls folder.
function Remove-NewFiles($Paths) {
	foreach ($path in (Get-DevFiles $Paths)) {
		if (Test-Path -LiteralPath $path) {
			Remove-Item -LiteralPath $path -Force
		}
	}
	foreach ($folder in @(@($Paths.DevFolder, $Paths.ClientFolder) | Where-Object { $_ -ne '' })) {
		Remove-EmptyFolder $folder
	}
	foreach ($parent in (Get-CreatedParents $Paths)) {
		Remove-EmptyFolder $parent
	}
	if (Test-Path -LiteralPath $Paths.TlsFolder) {
		Remove-Item -LiteralPath $Paths.TlsFolder -Recurse -Force
	}
}

# What an enable run is about to replace: the files of Get-DevFiles and the dev and client folders that exist already, with their bytes
# and their access rules, so that a failed run puts them back (Undo-EnableFiles) instead of deleting them.
function Save-ExistingItems($Paths) {
	$files = @()
	foreach ($path in (Get-DevFiles $Paths)) {
		if (Test-Path -LiteralPath $path -PathType Leaf) {
			$files += [pscustomobject]@{ Path = $path; Bytes = [System.IO.File]::ReadAllBytes($path); Access = (Get-AccessSddl $path) }
		}
	}
	$folders = @()
	foreach ($folder in @(@($Paths.DevFolder, $Paths.ClientFolder) | Where-Object { $_ -ne '' })) {
		if (Test-Path -LiteralPath $folder -PathType Container) {
			$folders += [pscustomobject]@{ Path = $folder; Access = (Get-AccessSddl $folder) }
		}
	}
	return [pscustomobject]@{ Files = $files; Folders = $folders }
}

# The files of a failed enable run: each file it replaced gets its bytes and access rules back and each file it added is removed; each
# folder that was there gets its access rules back, and each folder it created (the dev and client folders, their parents) is removed
# when it is then empty; the opms-dev-tls folder (always new: the run refuses to start when it exists) is removed, and so is my.ini's
# temporary copy ($Paths.IniTemp) when a write left it.
function Undo-EnableFiles($Paths, $Saved) {
	foreach ($path in (Get-DevFiles $Paths)) {
		$old = @($Saved.Files | Where-Object { $_.Path -eq $path })
		if ($old.Count -gt 0) {
			[System.IO.File]::WriteAllBytes($path, [byte[]]$old[0].Bytes)
			Set-AccessSddl $path $old[0].Access
		}
		elseif (Test-Path -LiteralPath $path) {
			Remove-Item -LiteralPath $path -Force
		}
	}
	foreach ($folder in @(@($Paths.DevFolder, $Paths.ClientFolder) | Where-Object { $_ -ne '' })) {
		$old = @($Saved.Folders | Where-Object { $_.Path -eq $folder })
		if ($old.Count -gt 0) {
			Set-AccessSddl $folder $old[0].Access
		}
		else {
			Remove-EmptyFolder $folder
		}
	}
	foreach ($parent in (Get-CreatedParents $Paths)) {
		Remove-EmptyFolder $parent
	}
	if (Test-Path -LiteralPath $Paths.TlsFolder) {
		Remove-Item -LiteralPath $Paths.TlsFolder -Recurse -Force
	}
	if ($Paths.IniTemp -ne '' -and (Test-Path -LiteralPath $Paths.IniTemp)) {
		Remove-Item -LiteralPath $Paths.IniTemp -Force
	}
}

function Invoke-Enable {
	$state = Assert-LocalServer
	$paths = Get-Paths $true
	$text = $script:IniFile.Text
	if (Test-HasBlock $text) {
		Write-Host ''
		Write-Host "TLS is already on through this script ($($script:Facts.Ini) has its block). Nothing to do."
		Write-Host 'To make new certificates, run -Revert first and then this script again.'
		$recordedNet = Get-RecordedTestNetwork $text
		if ($null -ne $recordedNet -and $null -eq (Get-TestNetworkRecordProblem $recordedNet)) {
			Write-TestVariables $recordedNet.Address $recordedNet.CapiThumbprint $recordedNet.CngThumbprint
		}
		exit 0
	}
	$options = Get-ServerOptions $text
	foreach ($key in @('ssl', 'ssl_ca', 'ssl_capath', 'ssl_cert', 'ssl_key', 'ssl_cipher', 'tls_version', 'require_secure_transport')) {
		if ($options.ContainsKey($key)) {
			Stop-Refused "my.ini already sets $key. This script only turns on TLS where none is configured; leave it as it is or remove the line first."
		}
	}
	if ($state.Values['have_ssl'] -eq 'YES') {
		Stop-Refused 'The server already has TLS (have_ssl=YES) from somewhere this script does not manage.'
	}
	if (Test-Path -LiteralPath $paths.Backup) {
		Stop-Refused "$($paths.Backup) exists from an earlier run. Run -Revert, or look at it and remove it."
	}
	if (Test-Path -LiteralPath $paths.TlsFolder) {
		Stop-Refused "$($paths.TlsFolder) exists. Run -Revert, or look at it and remove it."
	}
	if (Test-Path -LiteralPath $paths.IniTemp) {
		Stop-Refused "$($paths.IniTemp) exists, left by an earlier run. Look at it and remove it."
	}
	if (Test-InsideRepository $paths.DevFolder) {
		Stop-Refused "-DevFolder $($paths.DevFolder) is inside the repository; certificates and keys never go there."
	}
	if ($paths.ClientFolder -ne '') {
		if (Test-InsideRepository $paths.ClientFolder) {
			Stop-Refused "-ClientFolder $($paths.ClientFolder) is inside the repository; certificates and keys never go there."
		}
		if (Test-FolderOverlap $paths.ClientFolder $paths.DevFolder) {
			Stop-Refused "-ClientFolder $($paths.ClientFolder) and -DevFolder $($paths.DevFolder) overlap. The client key goes to a folder of its own, never where the dev tools read the CA."
		}
	}
	$protected = [ordered]@{
		'the MariaDB folder'       = $script:Facts.Home
		"the server's data folder" = [string]$state.Values['datadir']
		'the certificate folder'   = $paths.TlsFolder
	}
	$localRoots = Get-LocalAppDataRoots
	$problem = Get-FolderProblem '-DevFolder' $paths.DevFolder $DevFolderName @('ca.pem') $protected $localRoots
	if ($null -eq $problem -and $paths.ClientFolder -ne '') {
		$problem = Get-FolderProblem '-ClientFolder' $paths.ClientFolder $ClientFolderName @('client-cert.pem', 'client-key.pem') $protected $localRoots
	}
	if ($null -ne $problem) {
		Stop-Refused $problem
	}
	$serviceSid = $null
	if ($script:Facts.Account -ne '' -and $script:Facts.Account -ne 'LocalSystem') {
		$serviceSid = Get-Sid $script:Facts.Account
	}
	$devUserName = $DevUser
	if ($devUserName -eq '') {
		$devUserName = [System.Security.Principal.WindowsIdentity]::GetCurrent().Name
	}
	$devSid = Get-Sid $devUserName
	$consoleUser = [string](Get-CimInstance -ClassName Win32_ComputerSystem).UserName
	# -WithTestNetwork: the checks of this computer's network (read only), and the record with what the run will name once it has made it.
	$testAddress = ''
	$hostName = ''
	$netFacts = $null
	if ($WithTestNetwork -ne '') {
		$testAddress = $WithTestNetwork.Trim()
		$netFacts = Get-NetworkFacts
		$netProblem = Get-TestNetworkProblem $testAddress $netFacts ([string]$state.Values['version'])
		if ($null -ne $netProblem) {
			Stop-Refused $netProblem
		}
		$hostName = $netFacts.HostName
		$rules = @()
		if (Test-FirewallOn $netFacts) {
			$rules = @($FirewallAllowName, $FirewallBlockName)
		}
		$paths.TestNetwork = [pscustomobject]@{ Address = $testAddress; AdapterId = '<the new adapter, ROOT\NET\nnnn>'; FirewallRules = $rules
			CapiThumbprint = '<the CAPI certificate''s thumbprint>'; CngThumbprint = '<the CNG certificate''s thumbprint>' }
	}
	$block = Get-BlockLines $paths $false
	$newText = Add-ToMysqldSection $text $block $script:IniFile.Newline

	$withClient = $paths.ClientFolder -ne ''

	Write-Host ''
	Write-Host 'Plan:'
	$step = 1
	Write-Host "  $step. Make a test CA (its private key stays in memory and is discarded) and a server certificate for localhost and 127.0.0.1."
	if ($withClient) {
		Write-Host '     With -WithClientCertificate also a client certificate (clientAuth) for the certificate-login test of S2-2.'
	}
	else {
		Write-Host '     No client certificate (-WithClientCertificate is not given): S2-2''s certificate login waits for the owner''s yes. It can only be'
		Write-Host '     made in the same run as the CA, so it then means -Revert and this script again with -WithClientCertificate.'
	}
	$step++
	Write-Host "  $step. Create $($paths.TlsFolder) (owner Administrators; SYSTEM and Administrators full control, $($script:Facts.Account) read) with"
	Write-Host '     ca.pem, server-cert.pem and server-key.pem.'
	$step++
	Write-Host "  $step. Write the CA certificate (public part only) to $($paths.DevCa)."
	Write-Host "     $(Get-FolderPlanText $paths.DevFolder) a protected ACL (nothing inherited): $devUserName, SYSTEM and"
	Write-Host '     Administrators full control, nobody else. No private key goes to this folder.'
	if ($withClient) {
		$step++
		Write-Host "  $step. Write the client certificate and key to $($paths.ClientCert) and $($paths.ClientKey):"
		Write-Host "     a folder of its own, not the CA's. $(Get-FolderPlanText $paths.ClientFolder) a protected ACL (nothing"
		Write-Host "     inherited): $devUserName read only, SYSTEM and Administrators full control."
	}
	foreach ($parent in (Get-CreatedParents $paths)) {
		Write-Host "     $parent does not exist and is created for it (-Revert removes it again when it is then empty)."
	}
	foreach ($existing in (Get-DevFiles $paths)) {
		if (Test-Path -LiteralPath $existing) {
			Write-Host "     $existing exists and is replaced (put back as it was if the run fails)."
		}
	}
	if ($consoleUser -ne '' -and $consoleUser -ne $devUserName) {
		Write-Host "     NOTE: $consoleUser is signed in, but the files go to the folders of $devUserName. The dev tools and tests of"
		Write-Host "     $consoleUser look in their own %LOCALAPPDATA%: pass -DevUser and -DevFolder for that account, or set OPMS_DB_TLS_CA."
	}
	if ($testAddress -ne '') {
		foreach ($line in (Get-TestNetworkEnablePlan $testAddress $hostName $netFacts $paths.TlsFolder)) {
			if ($line.StartsWith('#. ')) {
				$step++
				$line = "  $step. " + $line.Substring(3)
			}
			Write-Host $line
		}
	}
	$step++
	Write-Host "  $step. Copy $($script:Facts.Ini) to $($paths.Backup), then add at the end of its [mysqld] section (the comment lines"
	Write-Host '     record the folders above; -Revert and -RequireSecureTransport read them from there):'
	foreach ($line in $block) {
		Write-Host "       $line"
	}
	if ($testAddress -ne '') {
		Write-Host "     [bind-address] Its last line, bind-address=127.0.0.1,$testAddress, follows my.ini's bind-address=127.0.0.1 and"
		Write-Host '     so takes its place (the last one wins). MariaDB 10.11 takes a comma-separated list; 127.0.0.1 stays. The record lines'
		Write-Host '     name the adapter, the firewall rules and the two certificates, filled in as the run makes them.'
	}
	$step++
	Write-Host "  $step. Restart the service $ServiceName (connections drop for a few seconds) and check a TLS session and the server"
	Write-Host '     certificate against the CA. If the server does not come back with TLS, the backup is restored, the files this run'
	Write-Host '     replaced are put back as they were and the files and folders it added are removed.'
	if ($testAddress -ne '') {
		Write-Host "     Then check that the service's process listens on 127.0.0.1:3306 and on ${testAddress}:3306 (Get-NetTCPConnection),"
		Write-Host "     and that the client verifies the server certificate at $testAddress. If anything fails, the firewall rules, both"
		Write-Host '     certificates with their keys and the adapter are removed again as well.'
		Write-Host '     At the end the run prints the three OPMS_DB_TLS_TEST_* values for TlsCertificateLoginTests.'
	}
	Write-Host '  require_secure_transport is not set: unencrypted connections keep working until -RequireSecureTransport.'
	Confirm-Change 'Turn on TLS in the local MariaDB service?'

	$set = New-CertificateSet $withClient $testAddress $hostName
	$saved = Save-ExistingItems $paths
	$changedIni = $false
	# What the test network part has made so far, for the undo of a failed run (Undo-TestNetwork).
	$created = [pscustomobject]@{ AdapterId = ''; FirewallRules = @(); Thumbprints = @(); PfxFolder = '' }
	if ($testAddress -ne '') {
		$paths.TestNetwork.CapiThumbprint = $set.TestCapiThumbprint
		$paths.TestNetwork.CngThumbprint = $set.TestCngThumbprint
	}
	try {
		New-Item -ItemType Directory -Path $paths.TlsFolder | Out-Null
		Set-ProtectedAcl $paths.TlsFolder @() @($serviceSid)
		Write-AsciiFile $paths.Ca $set.CaCert
		Write-AsciiFile $paths.ServerCert $set.ServerCert
		Write-AsciiFile $paths.ServerKey $set.ServerKey
		foreach ($file in @($paths.Ca, $paths.ServerCert, $paths.ServerKey)) {
			Set-ProtectedAcl $file @() @($serviceSid)
		}
		if (-not (Test-Path -LiteralPath $paths.DevFolder)) {
			New-Item -ItemType Directory -Path $paths.DevFolder | Out-Null
		}
		Set-ProtectedAcl $paths.DevFolder @($devSid) @()
		Write-AsciiFile $paths.DevCa $set.CaCert
		$written = "$($paths.TlsFolder), $($paths.DevCa)"
		if ($withClient) {
			if (-not (Test-Path -LiteralPath $paths.ClientFolder)) {
				New-Item -ItemType Directory -Path $paths.ClientFolder | Out-Null
			}
			Set-ProtectedAcl $paths.ClientFolder @() @($devSid)
			Write-AsciiFile $paths.ClientCert $set.ClientCert
			Write-AsciiFile $paths.ClientKey $set.ClientKey
			foreach ($file in @($paths.ClientCert, $paths.ClientKey)) {
				Set-ProtectedAcl $file @() @($devSid)
			}
			$written += ", $($paths.ClientFolder)"
		}
		if ($testAddress -ne '') {
			Install-TestNetwork $paths $set $created
			# The block names what the run made, now that it has made it.
			$block = Get-BlockLines $paths $false
			$newText = Add-ToMysqldSection $text $block $script:IniFile.Newline
		}
		Copy-Item -LiteralPath $script:Facts.Ini -Destination $paths.Backup
		$changedIni = $true
		Write-OptionFile $script:IniFile $newText
		Write-Host "Wrote $written and the TLS lines of $($script:Facts.Ini)."
		Restart-DbService
		$after = Get-ServerState 'tls'
		if (-not $after.Ok -or $after.Values['have_ssl'] -ne 'YES' -or $after.Values['ssl_cipher'] -eq '') {
			$why = $after.Error
			if ($after.Ok) {
				$why = "have_ssl=$($after.Values['have_ssl']), cipher '$($after.Values['ssl_cipher'])'"
			}
			throw "The server did not come back with TLS ($why). The MariaDB error log in the data folder says why."
		}
		if ($testAddress -ne '') {
			$listenerProblem = Get-TestListenerProblem $testAddress (Get-ServiceProcessId)
			if ($null -ne $listenerProblem) {
				throw "$listenerProblem The MariaDB error log in the data folder says why."
			}
			if ((Get-BindText $after.Values['bind_address']) -ne "127.0.0.1,$testAddress") {
				throw "The server reports the bind address $($after.Values['bind_address']), not 127.0.0.1,$testAddress."
			}
			$atTest = Invoke-DbClient 'SELECT 1' $paths.DevCa $testAddress
			if ($atTest.ExitCode -ne 0 -and $atTest.Error -match '(?i)SSL|TLS|certificate') {
				throw "The client could not verify the server certificate at ${testAddress}: $($atTest.Error)"
			}
		}
	}
	catch {
		$failure = $_.Exception.Message
		Write-Host "FAILED: $failure"
		try {
			if ($changedIni) {
				Copy-Item -LiteralPath $paths.Backup -Destination $script:Facts.Ini -Force
				Restart-DbService
			}
			# The run refused to start with a backup there, so a backup now is this run's own (also a partial copy).
			if (Test-Path -LiteralPath $paths.Backup) {
				Remove-Item -LiteralPath $paths.Backup -Force
			}
			if ($testAddress -ne '') {
				$netLeft = @(Undo-TestNetwork $created $testAddress)
				if ($netLeft.Count -gt 0) {
					throw "the test network could not be undone: $($netLeft -join '; ')"
				}
			}
			Undo-EnableFiles $paths $saved
			Write-Host 'Undone: my.ini restored from the backup, the service restarted, the files this run replaced put back as they were, the files and folders it added removed.'
			if ($testAddress -ne '') {
				Write-Host '        The firewall rules, the two test certificates with their keys and the loopback adapter are removed again.'
			}
			exit 3
		}
		catch {
			Write-Host "Undoing failed too: $($_.Exception.Message)"
			$replaced = @($saved.Files | ForEach-Object { $_.Path })
			$added = @(Get-DevFiles $paths | Where-Object { $replaced -notcontains $_ })
			Write-Host "Restore $($script:Facts.Ini) from $($paths.Backup) by hand (when it is still there), restart the service $ServiceName, and remove $($paths.TlsFolder), $($paths.IniTemp) when it is there, $($added -join ', ')."
			if ($replaced.Count -gt 0) {
				Write-Host "These files existed before the run and now hold the new certificates: $($replaced -join ', ')."
			}
			if ($testAddress -ne '') {
				Write-Host "Check and remove by hand what the run made of the test network (docs/port-notes/DB-TLS.md section 11): the firewall rules $(@($created.FirewallRules) -join ', ') (group '$FirewallGroup'), the certificates $(@($created.Thumbprints) -join ', ') in LocalMachine\My with their keys (certlm.msc), and the loopback adapter $($created.AdapterId) (Device Manager, Network adapters)."
			}
			exit 4
		}
	}
	Write-Host ''
	Write-Host "TLS is on: $($after.Values['ssl_version']), cipher $($after.Values['ssl_cipher']); the server certificate is valid until $($after.Values['ssl_server_not_after'])."
	$verified = Get-ServerState $paths.DevCa
	if ($verified.Ok -and $verified.Values['ssl_cipher'] -ne '') {
		Write-Host "The server certificate was verified against $($paths.DevCa) (chain and host name 127.0.0.1)."
	}
	else {
		Write-Host "WARNING: TLS works, but the client could not verify the server against $($paths.DevCa): $($verified.Error)"
		Write-Host '         Report this to the lead; the tests (DevDbTlsServerTests) check the verification with the program''s own connector.'
	}
	if ($testAddress -ne '') {
		Write-Host "The service listens on 127.0.0.1:3306 and ${testAddress}:3306 (adapter $($paths.TestNetwork.AdapterId), '$TestAdapterName')."
		if ($atTest.ExitCode -eq 0) {
			Write-Host "The server certificate was verified at $testAddress too."
		}
		else {
			Write-Host "At $testAddress the server answered, but refused the check's login ($($atTest.Error));"
			Write-Host '  TlsCertificateLoginTests checks the certificate there with its own account.'
		}
		Write-Host ''
		Write-TestVariables $testAddress $paths.TestNetwork.CapiThumbprint $paths.TestNetwork.CngThumbprint
	}
	Write-Host ''
	Write-Host 'Next: the lead runs the TLS tests, tells the other sessions, and later runs this script with -RequireSecureTransport.'
	exit 0
}

function Invoke-RequireSecureTransport {
	$state = Assert-LocalServer
	$text = $script:IniFile.Text
	if (-not (Test-HasBlock $text)) {
		Stop-Refused "my.ini has no block of this script: turn on TLS first (run the script without -RequireSecureTransport)."
	}
	$paths = Get-Paths $false
	if ($state.Values['require_secure_transport'] -eq '1' -and $text.Contains('require_secure_transport=ON')) {
		Write-Host ''
		Write-Host 'require_secure_transport is already ON. Nothing to do.'
		exit 0
	}
	if (-not (Test-Path -LiteralPath $paths.DevCa)) {
		if ($paths.IsRecorded) {
			Stop-Refused "$($paths.DevCa), the CA copy the enable run recorded in my.ini, is missing; the dev tools and tests would have no CA. Run -Revert and turn TLS on again."
		}
		Stop-Refused "$($paths.DevCa) is missing (my.ini records no dev folder); the dev tools and tests would have no CA. Pass -DevFolder, or run -Revert and turn TLS on again."
	}
	if (Test-Path -LiteralPath $paths.IniTemp) {
		Stop-Refused "$($paths.IniTemp) exists, left by an earlier run. Look at it and remove it."
	}
	$verified = Get-ServerState $paths.DevCa
	if (-not $verified.Ok -or $verified.Values['ssl_cipher'] -eq '') {
		Stop-Refused "A TLS connection verified against $($paths.DevCa) does not work ($($verified.Error)); fix that first."
	}
	$block = Get-BlockLines $paths $true
	$newText = Add-ToMysqldSection (Remove-Block $text) $block $script:IniFile.Newline
	Write-Host ''
	if ($paths.IsRecorded) {
		Write-Host "The CA copy of the dev tools and tests, as my.ini records it: $($paths.DevCa) (verified against the server)."
	}
	else {
		Write-Host "my.ini records no dev folder; the CA copy checked is $($paths.DevCa) (verified against the server)."
	}
	Write-Host 'Plan:'
	Write-Host "  1. Replace the block of this script in $($script:Facts.Ini) with:"
	foreach ($line in $block) {
		Write-Host "       $line"
	}
	Write-Host "  2. Restart the service $ServiceName, then check that a TLS connection works and an unencrypted one is refused."
	Write-Host '     If not, the line is removed again and the service restarted.'
	Write-Host '  From then on every connection without TLS is refused: tell the other sessions first, and make sure their worktrees'
	Write-Host '  have the DevDbTls change (docs/port-notes/DB-TLS.md).'
	Confirm-Change 'Require TLS for every connection to the local MariaDB?'

	try {
		Write-OptionFile $script:IniFile $newText
		Restart-DbService
		$after = Get-ServerState $paths.DevCa
		if (-not $after.Ok -or $after.Values['require_secure_transport'] -ne '1' -or $after.Values['ssl_cipher'] -eq '') {
			throw "After the restart the TLS check failed ($($after.Error); require_secure_transport=$($after.Values['require_secure_transport']))."
		}
		$plain = Get-ServerState 'plain'
		if ($plain.Ok) {
			throw 'An unencrypted connection was still accepted.'
		}
		if ($null -ne $paths.TestNetwork) {
			$listenerProblem = Get-TestListenerProblem $paths.TestNetwork.Address (Get-ServiceProcessId)
			if ($null -ne $listenerProblem) {
				throw $listenerProblem
			}
		}
	}
	catch {
		Write-Host "FAILED: $($_.Exception.Message)"
		try {
			Write-OptionFile $script:IniFile $text
			Restart-DbService
			Write-Host 'Undone: the block is back as it was (TLS on, require_secure_transport not set) and the service restarted.'
			exit 3
		}
		catch {
			Write-Host "Undoing failed too: $($_.Exception.Message). Remove the line require_secure_transport=ON from $($script:Facts.Ini) (and $($paths.IniTemp) when it is there) and restart the service."
			exit 4
		}
	}
	Write-Host ''
	Write-Host "require_secure_transport is ON: TLS connections work ($($after.Values['ssl_version'])), an unencrypted one was refused:"
	Write-Host "  $($plain.Error)"
	exit 0
}

function Invoke-Revert {
	[void](Assert-LocalServer)
	$paths = Get-Paths $false
	$text = $script:IniFile.Text
	if (-not (Test-Path -LiteralPath $paths.Backup)) {
		if (-not (Test-HasBlock $text) -and -not (Test-Path -LiteralPath $paths.TlsFolder)) {
			Write-Host ''
			Write-Host 'Nothing to revert: no backup, no block of this script in my.ini, no certificate folder.'
			exit 0
		}
		Stop-Refused "$($paths.Backup) is missing, so the original my.ini cannot be restored. Remove the block between the marker lines from $($script:Facts.Ini) by hand."
	}
	$backup = Read-OptionFile $paths.Backup
	$lost = @()
	if ((Remove-Block $text) -ne $backup.Text) {
		$current = @((Remove-Block $text) -split "`n" | ForEach-Object { $_.TrimEnd("`r") })
		$old = @($backup.Text -split "`n" | ForEach-Object { $_.TrimEnd("`r") })
		$lost = @(Compare-Object -ReferenceObject $old -DifferenceObject $current | Where-Object { $_.SideIndicator -eq '=>' } |
			ForEach-Object { $_.InputObject })
	}
	# The test network of -WithTestNetwork, as the block records it: what is there of it, and whether it is this script's to remove.
	$net = $paths.TestNetwork
	$netState = $null
	if ($null -ne $net) {
		$netState = Get-TestNetworkState $net $paths.Ca
		$netProblem = Get-TestNetworkStateProblem $net $netState
		if ($null -ne $netProblem) {
			Stop-Refused "$netProblem This script removes only what it made; remove that by hand, or put the record in $($script:Facts.Ini) right (docs/port-notes/DB-TLS.md section 11)."
		}
	}
	Write-Host ''
	Write-Host 'Plan:'
	$step = 0
	if ($null -ne $net) {
		Write-Host "  First the test network of -WithTestNetwork $($net.Address) (the record in my.ini stays until it is gone, so that -Revert can"
		Write-Host '  be run again if a step fails):'
		foreach ($line in (Get-TestNetworkRevertPlan $net $netState)) {
			if ($line.StartsWith('#. ')) {
				$step++
				$line = "  $step. " + $line.Substring(3)
			}
			Write-Host $line
		}
	}
	$step++
	Write-Host "  $step. Restore $($script:Facts.Ini) from $($paths.Backup)."
	if ($lost.Count -gt 0) {
		Write-Host '     my.ini was changed since TLS was turned on; restoring the backup also drops these lines:'
		foreach ($line in $lost) {
			Write-Host "       $line"
		}
	}
	$step++
	Write-Host "  $step. Restart the service $ServiceName and check an unencrypted connection."
	if ($null -ne $net) {
		Write-Host "     Then check that nothing listens on $($net.Address):3306 any more."
	}
	$step++
	Write-Host "  $step. Remove $($paths.TlsFolder), $((Get-DevFiles $paths) -join ', ') and the backup;"
	Write-Host "     $((@(@($paths.DevFolder, $paths.ClientFolder) | Where-Object { $_ -ne '' })) -join ' and ') too when it is then empty."
	foreach ($parent in (Get-CreatedParents $paths)) {
		Write-Host "     $parent too when it is then empty (the enable run created it)."
	}
	if ($paths.IsRecorded) {
		Write-Host '     These are the folders the enable run recorded in my.ini (whichever account they belong to).'
	}
	else {
		Write-Host "     my.ini records no dev folder, so only $($paths.DevCa) is removed (-DevFolder, else this account's %LOCALAPPDATA%)."
		Write-Host '     A ca.pem, client-cert.pem or client-key.pem in another account''s folder is not touched: pass -DevFolder (and'
		Write-Host '     -ClientFolder) for it, or remove it by hand, or the dev tools there keep asking the server for TLS.'
	}
	Confirm-Change 'Turn TLS off again?'
	if ($null -ne $net) {
		$recorded = [pscustomobject]@{ AdapterId = $net.AdapterId; FirewallRules = @($net.FirewallRules); Thumbprints = @($net.CapiThumbprint, $net.CngThumbprint); PfxFolder = (Join-Path $paths.TlsFolder $PfxFolderName) }
		$netLeft = @()
		try {
			$netLeft = @(Undo-TestNetwork $recorded $net.Address)
		}
		catch {
			$netLeft = @($_.Exception.Message)
		}
		if ($netLeft.Count -gt 0) {
			Write-Host "FAILED: the test network is not fully removed: $($netLeft -join '; ')."
			Write-Host "Nothing else was changed: my.ini still has its block (the record), so -Revert can run again once that is put right."
			exit 4
		}
		Write-Host "Removed the test network: the firewall rules, both test certificates with their keys, the adapter $($net.AdapterId) and $($net.Address)."
	}
	try {
		Copy-Item -LiteralPath $paths.Backup -Destination $script:Facts.Ini -Force
		Restart-DbService
		$plain = Get-ServerState 'plain'
		if (-not $plain.Ok) {
			throw "An unencrypted connection does not work after the restore: $($plain.Error)"
		}
		if ($null -ne $net -and @(Get-NetTCPConnection -LocalAddress $net.Address -LocalPort 3306 -State Listen -ErrorAction SilentlyContinue).Count -gt 0) {
			throw "Something still listens on $($net.Address):3306 after the restore."
		}
		Remove-NewFiles $paths
		Remove-Item -LiteralPath $paths.Backup -Force
	}
	catch {
		Write-Host "FAILED: $($_.Exception.Message)"
		Write-Host "The backup $($paths.Backup) is kept. Look at $($script:Facts.Ini) and the MariaDB error log, then restart the service."
		if ($null -ne $net) {
			Write-Host "The test network is already removed, so a my.ini that still has the block (bind-address=127.0.0.1,$($net.Address)) keeps the"
			Write-Host "service from starting: copy $($paths.Backup) over $($script:Facts.Ini) by hand first."
		}
		exit 4
	}
	Write-Host ''
	Write-Host 'TLS is off again: my.ini restored, the service restarted, the certificate files removed.'
	exit 0
}

function Invoke-TestCertificates {
	$TestCertificatesFolder = Resolve-FullPath $TestCertificatesFolder
	if (Test-InsideRepository $TestCertificatesFolder) {
		Stop-Refused "$TestCertificatesFolder is inside the repository; certificates and keys never go there."
	}
	if (Test-Path -LiteralPath $TestCertificatesFolder) {
		if (@(Get-ChildItem -LiteralPath $TestCertificatesFolder -Force).Count -gt 0) {
			Stop-Refused "$TestCertificatesFolder is not empty."
		}
	}
	else {
		New-Item -ItemType Directory -Path $TestCertificatesFolder | Out-Null
	}
	$testAddress = $WithTestNetwork.Trim()
	$hostName = ''
	if ($testAddress -ne '') {
		$hostName = [System.Net.Dns]::GetHostName()
	}
	$set = New-CertificateSet $WithClientCertificate.IsPresent $testAddress $hostName
	Write-AsciiFile (Join-Path $TestCertificatesFolder 'ca.pem') $set.CaCert
	Write-AsciiFile (Join-Path $TestCertificatesFolder 'server-cert.pem') $set.ServerCert
	Write-AsciiFile (Join-Path $TestCertificatesFolder 'server-key.pem') $set.ServerKey
	if ($WithClientCertificate) {
		Write-AsciiFile (Join-Path $TestCertificatesFolder 'client-cert.pem') $set.ClientCert
		Write-AsciiFile (Join-Path $TestCertificatesFolder 'client-key.pem') $set.ClientKey
	}
	if ($testAddress -ne '') {
		# Only here, in the tests' own scratch folder, does the .pfx password go to a file: the tests read the .pfx files back with it.
		[System.IO.File]::WriteAllBytes((Join-Path $TestCertificatesFolder 'test-client-capi.pfx'), [byte[]]$set.TestCapiPfx)
		[System.IO.File]::WriteAllBytes((Join-Path $TestCertificatesFolder 'test-client-cng.pfx'), [byte[]]$set.TestCngPfx)
		Write-AsciiFile (Join-Path $TestCertificatesFolder 'test-client-pfx-password.txt') $set.TestPfxPassword
	}
	Write-Host "Wrote a test certificate set to $TestCertificatesFolder (CA $($set.CaThumbprint), server $($set.ServerThumbprint))."
	exit 0
}

#endregion

try {
	$modes = @($RequireSecureTransport.IsPresent, $Revert.IsPresent, ($TestCertificatesFolder -ne '')) | Where-Object { $_ }
	if (@($modes).Count -gt 1) {
		Stop-Refused 'Use one of -RequireSecureTransport, -Revert and -TestCertificatesFolder at a time.'
	}
	if ($WithClientCertificate -and ($RequireSecureTransport -or $Revert)) {
		Stop-Refused '-WithClientCertificate goes with the enable run (or -TestCertificatesFolder) only: the client certificate is made with the CA.'
	}
	if ($ClientFolder -ne '' -and ($TestCertificatesFolder -ne '' -or (-not $WithClientCertificate -and -not $RequireSecureTransport -and -not $Revert))) {
		Stop-Refused '-ClientFolder needs -WithClientCertificate (the enable run), or names the recorded client folder for -Revert or -RequireSecureTransport.'
	}
	if ($WithTestNetwork -ne '' -and ($RequireSecureTransport -or $Revert)) {
		Stop-Refused '-WithTestNetwork goes with the enable run (or -TestCertificatesFolder) only: -Revert and -RequireSecureTransport take the test network from the record in my.ini.'
	}
	if ($WithTestNetwork -ne '') {
		$addressProblem = Get-TestAddressFormProblem $WithTestNetwork
		if ($null -ne $addressProblem) {
			Stop-Refused $addressProblem
		}
	}
	if ($TestCertificatesFolder -ne '') {
		Invoke-TestCertificates
	}
	if (-not (Test-Elevated)) {
		if (-not $DryRun) {
			Stop-Refused 'This script changes the MariaDB service and must run elevated: start PowerShell with "Run as administrator" (or use -DryRun to see the plan).'
		}
		Write-Host 'Not elevated: a dry run only. The real run must be started as administrator.'
	}
	if ($RequireSecureTransport) {
		Invoke-RequireSecureTransport
	}
	elseif ($Revert) {
		Invoke-Revert
	}
	else {
		Invoke-Enable
	}
}
catch {
	Write-Host "FAILED: $($_.Exception.Message)"
	exit 1
}
