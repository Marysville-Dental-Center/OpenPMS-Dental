<#
.SYNOPSIS
Runs the new Middle Tier for real as a Windows service on this PC, with a test certificate from a PFX file, runs the host round-trip tests
against it, and removes everything again. Elevated, 64-bit only. A launch check of stage 3b (docs\admin\launch-checks-3b.md).

.DESCRIPTION
HOW TO RUN IT. The owner runs it, never an agent: it installs and starts a Windows service. Build the server and the host tests first
(the lane's x64 build of OpenDental.Local.slnf, or of the projects OpenPmsDentalServer and MiddleTierHostTests), then open Windows
PowerShell with "Run as administrator" and paste this one line, with <repo> the repository folder:

  powershell -NoProfile -ExecutionPolicy Bypass -File "<repo>\tools\Test-MiddleTierService.ps1"

-PlanOnly prints every step with the names it would use and changes nothing (no elevation needed). -ClientSession adds the client session
of docs\port-notes\S3b-6.md section 7 (step 8 below; it waits for you). -ServerFolder names the built server folder (default: <repo>\
OpenPmsDentalServer\bin\Release when it holds the server, else bin\Debug). -Port fixes the port (default: a free port on 127.0.0.1).
-OutDir is the logs' folder (default: a new %TEMP%\OpenPmsDentalServer-svctest-logs-<id> folder), which the script keeps.

It refuses, changing nothing: without elevation (exit code 2); in a 32-bit PowerShell, when a service named OpenPmsDentalServer-Test
already exists, when the server or the host tests are not built, when the server is not the self-contained x64 build, when no MariaDB
client is found or the database server on 127.0.0.1:3306 does not answer (exit code 3). It connects to no database server but
127.0.0.1:3306 (user root, blank password: the developer database of this PC), and only to its own database there.

WHAT IT DOES, in order (each step is a check: "ok" or "FAIL"):
1. Copies the built server folder whole (self-contained x64: OpenPmsDentalServer.exe with its own .NET runtime) into a new temp folder
   %TEMP%\OpenPmsDentalServer-svctest-<id>.
2. Makes a self-signed test certificate (CN=OpenPmsDentalServer-Test launch check, for 127.0.0.1 and localhost, server authentication,
   valid two days) in memory with .NET and writes it as a PFX FILE into that folder, with a random password. No certificate store is
   used: Kestrel loads the file through its Endpoints configuration. Its SHA-1 thumbprint goes to MIDDLETIER_TEST_CERT_THUMBPRINT, so the
   tests trust that certificate only.
3. Writes the folder's appsettings.json (one Kestrel endpoint https://127.0.0.1:<port>, the PFX file and its password, the Middle Tier
   path /OpenDentalServer) and OpenDentalServerConfig.xml (127.0.0.1, the database below, root, blank password).
4. Reserves a new scratch database mtsvc_<time>_<number> on 127.0.0.1:3306 (it must not exist). The host tests build it (db\init\
   new-database.sql with every fork migration, this build's ProgramVersion and the tests' three users), as in S3b-6 section 6.
5. Registers the service with sc.exe as OpenPmsDentalServer-Test (start=demand, LocalSystem), starts it and waits until it runs and answers
   on its port: a GET of the endpoint gets 405 with "Allow: POST", from a server that shows the test certificate.
6. Sends one request above the server's limit by hand (a Content-Length of 1 GiB plus one byte, no body): 413.
7. Runs MiddleTierHostTests against the service (tools\run-tests.ps1 -Mode Targeted -Platform x64 -RequireBuildOutput, with
   MIDDLETIER_TEST_URL, MIDDLETIER_TEST_DB, MIDDLETIER_TEST_CERT_THUMBPRINT and S3B6_KEEP_DB=1): no failure, at least 16 passed (the three
   tests that need the in-process servers are inconclusive), NoHqTests as usual.
8. Only with -ClientSession: adds the certificate (its public part) to the computer's Trusted Root store, so the program trusts it, and
   waits while you start the program (<repo>\OpenPmsDental\bin\Debug\OpenPmsDental.exe), choose the Middle Tier address the script prints,
   log in as s3b6usera (its password is in tests\MiddleTierHostTests\HostTestRun.cs), open a patient and close the program; then you
   answer y or n. The certificate is removed from Trusted Root again at the end.
Then, also after a failure or Ctrl+C: stops the service, deletes it (sc.exe), saves the Application event log entries of the service in
the logs' folder, deletes the temp folder, drops the scratch database, and prints one summary line "N checks passed, M failed" with the
logs' folder, and the exit code: 0 every check passed, 1 a check failed.

WHAT IT CHANGES ON THIS PC while it runs, and what it puts back:
- the service OpenPmsDentalServer-Test (HKLM\SYSTEM\CurrentControlSet\Services\OpenPmsDentalServer-Test): stopped and deleted at the end.
- the temp folder %TEMP%\OpenPmsDentalServer-svctest-<id> (the server copy, the PFX file, the settings): deleted at the end.
- the scratch database mtsvc_<time>_<number> on 127.0.0.1:3306: dropped at the end.
- the Event Log source key HKLM\SYSTEM\CurrentControlSet\Services\EventLog\Application\OpenPmsDentalServer, which the service's own logging
  registers when it writes its first event: deleted at the end when it was not there before. The events the service wrote stay in the
  Application log (saved in the logs' folder too).
- with -ClientSession only: the test certificate in Cert:\LocalMachine\Root, removed at the end.
- Windows keeps the key of a PFX that a service loads in its machine key folder while the service runs; the script lists any key file that
  is new in those folders at the end (a "note" line), without deleting anything.
- the logs' folder, which it keeps.
#>
[CmdletBinding(PositionalBinding=$false)]
param(
	[string]$ServerFolder='',
	[string]$OutDir='',
	[int]$Port=0,
	[switch]$ClientSession,
	[switch]$PlanOnly
)
Set-StrictMode -Version 2
$ErrorActionPreference='Stop'

$ServiceName='OpenPmsDentalServer-Test'
$ServiceDisplayName='Open PMS - Dental Middle Tier (launch check)'
$ServerExe='OpenPmsDentalServer.exe'
#The only database server this script talks to.
$DbHost='127.0.0.1'
$DbPort=3306
$DbUser='root'
$DbPrefix='mtsvc_'
#Double quotes: the old-name check allows the IIS path after a double quote (tools\Test-NoOldNames.ps1).
$PathBase="/OpenDentalServer"
$CertificateSubject='CN=OpenPmsDentalServer-Test launch check'
#The event source the service's logging and its start-up error use (ServerHost.SERVICE_NAME; the application name of AddWindowsService).
$EventLogSourceKey='HKLM:\SYSTEM\CurrentControlSet\Services\EventLog\Application\OpenPmsDentalServer'
$EventLogSource='OpenPmsDentalServer'
#The limit the shipped settings give (MiddleTier:MaxRequestBodySize, 1 GiB); one byte more must get 413.
$MaxRequestBodySize=1073741824
$RepoRoot=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path

#region Plain code (the tests in tests\ReleaseFlowTests dot-source the script and call these; none of them changes anything outside its arguments)
#A name for this run's scratch database: mtsvc_, the time (yyMMddHHmmss) and a random number; lower-case letters, digits and underscores.
function New-DatabaseName([DateTime]$now){
	return ('{0}{1}_{2}' -f $DbPrefix,$now.ToString('yyMMddHHmmss',[Globalization.CultureInfo]::InvariantCulture),(Get-Random -Minimum 1000 -Maximum 10000))
}

#Whether the name is one this script may create and drop.
function Test-DatabaseName([string]$name){
	return ($name -cmatch '^mtsvc_[a-z0-9_]+$') -and $name.Length -le 64
}

#A random password for the PFX file (base64 of 24 random bytes).
function New-PfxPassword {
	$bytes=New-Object byte[] 24
	$random=[System.Security.Cryptography.RandomNumberGenerator]::Create()
	try { $random.GetBytes($bytes) } finally { $random.Dispose() }
	return [Convert]::ToBase64String($bytes)
}

#The self-signed test certificate, made in memory (an ephemeral CNG key, no key container, no certificate store) and written as a PFX file
#to $path with $password. Returns the thumbprint and the certificate's public part (DER).
function New-TestCertificatePfx([string]$path,[string]$password){
	$rsa=New-Object System.Security.Cryptography.RSACng -ArgumentList 2048
	try {
		$name=New-Object System.Security.Cryptography.X509Certificates.X500DistinguishedName -ArgumentList $CertificateSubject
		$request=New-Object System.Security.Cryptography.X509Certificates.CertificateRequest -ArgumentList $name,$rsa,([System.Security.Cryptography.HashAlgorithmName]::SHA256),([System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
		$san=New-Object System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder
		$san.AddIpAddress([System.Net.IPAddress]::Loopback)
		$san.AddDnsName('localhost')
		$request.CertificateExtensions.Add($san.Build())
		$request.CertificateExtensions.Add((New-Object System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension -ArgumentList $false,$false,0,$true))
		$usage=[System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature -bor [System.Security.Cryptography.X509Certificates.X509KeyUsageFlags]::KeyEncipherment
		$request.CertificateExtensions.Add((New-Object System.Security.Cryptography.X509Certificates.X509KeyUsageExtension -ArgumentList $usage,$true))
		$oids=New-Object System.Security.Cryptography.OidCollection
		[void]$oids.Add((New-Object System.Security.Cryptography.Oid -ArgumentList '1.3.6.1.5.5.7.3.1'))
		$request.CertificateExtensions.Add((New-Object System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension -ArgumentList $oids,$false))
		$now=[DateTimeOffset]::UtcNow
		$certificate=$request.CreateSelfSigned($now.AddMinutes(-10),$now.AddDays(2))
		try {
			$pfx=$certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pfx,$password)
			[System.IO.File]::WriteAllBytes($path,$pfx)
			return New-Object PSObject -Property @{ Thumbprint=$certificate.Thumbprint; Cer=$certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert) }
		}
		finally { $certificate.Dispose() }
	}
	finally { $rsa.Dispose() }
}

#The server folder's appsettings.json: one Kestrel endpoint on 127.0.0.1 with the certificate from the PFX file (no store), the Middle
#Tier settings as shipped, logging.
function New-AppSettingsJson([int]$port,[string]$pfxPath,[string]$pfxPassword){
	$lines=@(
		'{',
		'  "MiddleTier": {',
		('    "PathBase": {0},' -f (ConvertTo-Json $PathBase)),
		('    "MaxRequestBodySize": {0}' -f $MaxRequestBodySize),
		'  },',
		'  "Kestrel": {',
		'    "Endpoints": {',
		'      "Https": {',
		('        "Url": {0},' -f (ConvertTo-Json ('https://{0}:{1}' -f $DbHost,$port))),
		'        "Certificate": {',
		('          "Path": {0},' -f (ConvertTo-Json $pfxPath)),
		('          "Password": {0}' -f (ConvertTo-Json $pfxPassword)),
		'        }',
		'      }',
		'    }',
		'  },',
		'  "Logging": {',
		'    "LogLevel": {',
		'      "Default": "Information",',
		'      "Microsoft.AspNetCore": "Warning"',
		'    }',
		'  },',
		'  "AllowedHosts": "*"',
		'}'
	)
	return ($lines -join "`r`n")+"`r`n"
}

#The server's database settings (OpenDentalServerConfig.xml, found by the application name /OpenDentalServer): the scratch database on
#127.0.0.1 as root with a blank password. Every element of the shipped sample (OpenPmsDentalServer\OpenDentalServerConfig.xml): the server's
#reader (Userods.LoadDatabaseInfoFromFile) requires UserLow and PasswordLow too, and without them every request failed with "Object reference
#not set to an instance of an object" (docs\port-notes\S3b-6.md, section 9).
function New-ServerConfigXml([string]$database){
	$e=[System.Security.SecurityElement]::Escape($database)
	$lines=@(
		'<?xml version="1.0"?>',
		'<ConnectionSettings>',
		'  <DatabaseConnection>',
		('    <ComputerName>{0}</ComputerName>' -f $DbHost),
		('    <Database>{0}</Database>' -f $e),
		('    <User>{0}</User>' -f $DbUser),
		'    <Password></Password>',
		'    <UserLow></UserLow>',
		'    <PasswordLow></PasswordLow>',
		'    <DatabaseType>MySql</DatabaseType>',
		('    <ApplicationName>{0}</ApplicationName>' -f $PathBase),
		'    <VerboseLogging>false</VerboseLogging>',
		'    <LogDirectory></LogDirectory>',
		'  </DatabaseConnection>',
		'</ConnectionSettings>'
	)
	return ($lines -join "`r`n")+"`r`n"
}

#The address the tests and the program use.
function Get-ServiceUrl([int]$port){
	return ('https://{0}:{1}{2}/ServiceMain.asmx' -f $DbHost,$port,$PathBase)
}

#The request text of a raw HTTP/1.1 request: GET, or a POST that declares contentLength and sends no body.
function New-RawRequest([string]$method,[int]$port,[long]$contentLength){
	$text="$method $PathBase/ServiceMain.asmx HTTP/1.1`r`nHost: ${DbHost}:$port`r`n"
	if($method -eq 'POST'){ $text+="Content-Type: text/plain; charset=utf-8`r`nContent-Length: $contentLength`r`n" }
	return $text+"Connection: close`r`n`r`n"
}

#The status code of an HTTP response's first line, or 0.
function Get-StatusCode([string]$response){
	$match=[regex]::Match($response,'^HTTP/1\.[01] (\d{3})')
	if($match.Success){ return [int]$match.Groups[1].Value }
	return 0
}

#The result line of one assembly in a results file of tools\run-tests.ps1, or $null.
function Read-TestRunLine([string[]]$lines,[string]$assembly){
	foreach($line in $lines){
		$match=[regex]::Match($line.TrimStart([char]0xFEFF),'^(\S+) exit=(\S+) total=(\S+) passed=(\S+) failed=(\S+)(?: skipped=(\S+))?')
		if($match.Success -and $match.Groups[1].Value -eq $assembly){
			return New-Object PSObject -Property @{ Exit=$match.Groups[2].Value; Total=$match.Groups[3].Value; Passed=$match.Groups[4].Value
				Failed=$match.Groups[5].Value; Skipped=$match.Groups[6].Value }
		}
	}
	return $null
}

#Whether the host tests' run went as expected against a running server: the run finished, no failure, at least 16 passed; NoHqTests
#without a failure.
function Test-HostTestRun([string[]]$lines){
	$host1=Read-TestRunLine $lines 'MiddleTierHostTests'
	$noHq=Read-TestRunLine $lines 'NoHqTests'
	$done=@($lines | Where-Object { $_ -match '^DONE ' }).Count -gt 0
	$hostOk=($null -ne $host1) -and $host1.Exit -eq '0' -and $host1.Failed -eq '0' -and ($host1.Passed -match '^\d+$') -and ([int]$host1.Passed -ge 16)
	$noHqOk=($null -ne $noHq) -and $noHq.Failed -eq '0'
	return New-Object PSObject -Property @{ Done=$done; HostOk=$hostOk; NoHqOk=$noHqOk; Host=$host1; NoHq=$noHq }
}

#The steps, as -PlanOnly prints them.
function Get-PlanSteps([string]$serverFolder,[string]$workFolder,[string]$outDir,[string]$database,[string]$portText,[bool]$withClientSession){
	$steps=@(
		"copy the built server folder $serverFolder whole into the new temp folder $workFolder",
		"make the self-signed test certificate ($CertificateSubject, 127.0.0.1 and localhost, two days) in memory and write it as the PFX file $workFolder\mtsvc-test.pfx with a random password (no certificate store)",
		"write $workFolder\appsettings.json (https://${DbHost}:$portText, the PFX file) and $workFolder\OpenDentalServerConfig.xml (${DbHost}, $database, $DbUser)",
		"reserve the scratch database $database on ${DbHost}:$DbPort (CREATE DATABASE; the host tests build it)",
		"sc.exe create $ServiceName binPath= `"$workFolder\$ServerExe`" start= demand; sc.exe start $ServiceName; wait until it runs and a GET of https://${DbHost}:$portText$PathBase/ServiceMain.asmx answers 405 with Allow: POST, showing the test certificate",
		"send one POST with Content-Length $($MaxRequestBodySize+1) and no body: 413",
		"run tools\run-tests.ps1 -Mode Targeted -Platform x64 -RequireBuildOutput -Include MiddleTierHostTests with MIDDLETIER_TEST_URL, MIDDLETIER_TEST_DB=$database, MIDDLETIER_TEST_CERT_THUMBPRINT and S3B6_KEEP_DB=1; results in $outDir\host-tests.txt"
	)
	if($withClientSession){
		$steps+="add the certificate's public part to Cert:\LocalMachine\Root and wait for the client session (start the program, Middle Tier address, log in as s3b6usera, open a patient, close it; answer y or n)"
	}
	$steps+=@(
		"at the end, also after a failure: sc.exe stop and sc.exe delete $ServiceName; save the service's Application event log entries to $outDir\service-events.txt",
		"delete $workFolder; DROP DATABASE $database; delete the Event Log source key $EventLogSourceKey if the run made it",
		"print 'N checks passed, M failed. Logs: $outDir'"
	)
	if($withClientSession){ $steps+="remove the certificate from Cert:\LocalMachine\Root" }
	return $steps
}
#endregion Plain code

#Dot-sourced by the tests: the functions above only.
if($MyInvocation.InvocationName -eq '.'){ return }

#Before anything else: an unelevated run changes nothing (-PlanOnly changes nothing anyway and needs no elevation).
$principal=New-Object System.Security.Principal.WindowsPrincipal([System.Security.Principal.WindowsIdentity]::GetCurrent())
$isElevated=$principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)
if(-not $isElevated -and -not $PlanOnly){
	Write-Host 'Test-MiddleTierService.ps1 needs an elevated PowerShell: it installs and starts a Windows service.'
	Write-Host 'Open Windows PowerShell with "Run as administrator" and run the line in the header of this script. Nothing was changed.'
	Write-Host 'Exit code 2'
	exit 2
}
if(-not [Environment]::Is64BitProcess){
	Write-Host 'Test-MiddleTierService.ps1 needs the 64-bit Windows PowerShell (this one is 32-bit). Open Windows PowerShell with "Run as administrator"'
	Write-Host 'from the Start menu (not "Windows PowerShell (x86)") and run the line in the header of this script. Nothing was changed.'
	Write-Host 'Exit code 3'
	exit 3
}

$script:Failures=New-Object 'System.Collections.Generic.List[string]'
$script:Passes=0
$script:Refusals=New-Object 'System.Collections.Generic.List[string]'

function Check([bool]$condition,[string]$what){
	if($condition){ $script:Passes++; Write-Host "  ok    $what" }
	else{ $script:Failures.Add($what); Write-Host "  FAIL  $what" }
}

function Note([string]$what){ Write-Host "  note  $what" }

#The MariaDB client of this PC (mariadb.exe or mysql.exe): on the PATH, or in the bin folder of a MariaDB installation.
function Find-DbClient {
	foreach($name in @('mariadb.exe','mysql.exe')){
		$command=Get-Command $name -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
		if($command){ return $command.Source }
	}
	foreach($folder in @(Get-ChildItem -LiteralPath $env:ProgramFiles -Directory -Filter 'MariaDB*' -ErrorAction SilentlyContinue | Sort-Object Name -Descending)){
		foreach($name in @('mariadb.exe','mysql.exe')){
			$path=Join-Path $folder.FullName "bin\$name"
			if(Test-Path -LiteralPath $path){ return $path }
		}
	}
	return $null
}

#Runs one SQL statement on 127.0.0.1:3306 as root (no option files, so nothing else decides the server). Returns the output lines; throws
#when the client fails.
function Invoke-Sql([string]$sql){
	#Windows PowerShell turns a native program's standard error into error records, which 'Stop' would throw at once: collect them instead.
	$ErrorActionPreference='Continue'
	$output=@(& $script:DbClient '--no-defaults' "--host=$DbHost" "--port=$DbPort" '--protocol=TCP' "--user=$DbUser" '--batch' '--skip-column-names' "--execute=$sql" 2>&1 | ForEach-Object { "$_" })
	if($LASTEXITCODE -ne 0){ throw "the MariaDB client failed (exit $LASTEXITCODE): $($output -join ' ')" }
	return $output
}

#A raw HTTPS request to the service: the response's head, the status code and the thumbprint of the certificate the server showed. Only
#that certificate is accepted.
function Invoke-RawHttps([int]$port,[string]$request,[string]$thumbprint){
	$result=New-Object PSObject -Property @{ Status=0; Response=''; ServerThumbprint=''; Error='' }
	$seen=@{ Thumbprint='' }
	$callback=[System.Net.Security.RemoteCertificateValidationCallback]({
		param($sender,$certificate,$chain,$errors)
		$hash=$certificate.GetCertHashString()
		$seen.Thumbprint=$hash
		return ($hash -eq $thumbprint)
	}.GetNewClosure())
	$client=New-Object System.Net.Sockets.TcpClient
	try {
		$client.ReceiveTimeout=20000
		$client.SendTimeout=20000
		$client.Connect($DbHost,$port)
		$ssl=New-Object System.Net.Security.SslStream -ArgumentList $client.GetStream(),$false,$callback
		try {
			$ssl.AuthenticateAsClient($DbHost,$null,[System.Security.Authentication.SslProtocols]::Tls12,$false)
			$bytes=[System.Text.Encoding]::ASCII.GetBytes($request)
			$ssl.Write($bytes,0,$bytes.Length)
			$ssl.Flush()
			$buffer=New-Object byte[] 8192
			$text=New-Object System.Text.StringBuilder
			while($text.Length -lt 65536){
				$count=$ssl.Read($buffer,0,$buffer.Length)
				if($count -le 0){ break }
				[void]$text.Append([System.Text.Encoding]::ASCII.GetString($buffer,0,$count))
				if($text.ToString().Contains("`r`n`r`n")){ break }
			}
			$result.Response=$text.ToString()
			$result.Status=Get-StatusCode $result.Response
		}
		finally { $ssl.Dispose() }
	}
	catch { $result.Error=$_.Exception.GetBaseException().Message }
	finally { $client.Close() }
	$result.ServerThumbprint=$seen.Thumbprint
	return $result
}

function Get-FreePort {
	$listener=New-Object System.Net.Sockets.TcpListener -ArgumentList ([System.Net.IPAddress]::Loopback),0
	$listener.Start()
	try { return $listener.LocalEndpoint.Port } finally { $listener.Stop() }
}

#The PE machine of a file (0x8664 x64).
function Get-PeMachine([string]$path){
	$bytes=[System.IO.File]::ReadAllBytes($path)
	$offset=[BitConverter]::ToInt32($bytes,0x3C)
	return [BitConverter]::ToUInt16($bytes,$offset+4)
}

function Get-ServiceOrNull([string]$name){
	return Get-Service -Name $name -ErrorAction SilentlyContinue
}

#sc.exe with one argument string (passed as is, so a path with spaces keeps its quotes); its output goes to the logs.
function Invoke-Sc([string]$arguments,[string]$logName){
	$log=Join-Path $OutDir $logName
	$process=Start-Process -FilePath 'sc.exe' -ArgumentList $arguments -Wait -PassThru -NoNewWindow -RedirectStandardOutput $log
	return $process.ExitCode
}

function Wait-ServiceStatus([string]$status,[int]$seconds){
	$deadline=(Get-Date).AddSeconds($seconds)
	while((Get-Date) -lt $deadline){
		$service=Get-ServiceOrNull $ServiceName
		if($null -eq $service){ return $false }
		$service.Refresh()
		if("$($service.Status)" -eq $status){ return $true }
		if($status -eq 'Running' -and "$($service.Status)" -eq 'Stopped'){ return $false }
		Start-Sleep -Milliseconds 500
	}
	return $false
}

#Key files in Windows' machine key folders (CAPI and CNG), and in LocalSystem's own, by full name.
function Get-MachineKeyFiles {
	$folders=@((Join-Path $env:ProgramData 'Microsoft\Crypto\RSA\MachineKeys'),(Join-Path $env:ProgramData 'Microsoft\Crypto\Keys'),
		(Join-Path $env:SystemRoot 'System32\config\systemprofile\AppData\Roaming\Microsoft\Crypto\Keys'))
	$names=@{}
	foreach($folder in $folders){
		foreach($file in @(Get-ChildItem -LiteralPath $folder -File -Force -ErrorAction SilentlyContinue)){ $names[$file.FullName]=$true }
	}
	return $names
}

#region Before the first change
$id=[guid]::NewGuid().ToString('N').Substring(0,8)
if(-not $ServerFolder){
	$ServerFolder=Join-Path $RepoRoot 'OpenPmsDentalServer\bin\Release'
	if(-not (Test-Path -LiteralPath (Join-Path $ServerFolder $ServerExe))){ $ServerFolder=Join-Path $RepoRoot 'OpenPmsDentalServer\bin\Debug' }
}
if(-not $OutDir){ $OutDir=Join-Path $env:TEMP "OpenPmsDentalServer-svctest-logs-$id" }
$WorkFolder=Join-Path $env:TEMP "OpenPmsDentalServer-svctest-$id"
$Database=New-DatabaseName (Get-Date)
$HostTestsDll=Join-Path $RepoRoot 'tests\MiddleTierHostTests\bin\Debug\MiddleTierHostTests.dll'
$ProgramExe=Join-Path $RepoRoot 'OpenPmsDental\bin\Debug\OpenPmsDental.exe'
$script:DbClient=Find-DbClient

if($null -ne (Get-ServiceOrNull $ServiceName)){ $script:Refusals.Add("A service named $ServiceName exists already on this PC; remove it first (sc.exe delete $ServiceName) or look at what made it.") }
$serverPath=Join-Path $ServerFolder $ServerExe
if(-not (Test-Path -LiteralPath $serverPath)){
	$script:Refusals.Add("The server is not built: no $serverPath. Build OpenPmsDentalServer (x64) through the lane first, or pass -ServerFolder.")
}
else {
	$machine=Get-PeMachine $serverPath
	$runtimeConfig=Join-Path $ServerFolder 'OpenPmsDentalServer.runtimeconfig.json'
	$isSelfContained=(Test-Path -LiteralPath $runtimeConfig) -and ([System.IO.File]::ReadAllText($runtimeConfig) -match '"includedFrameworks"') -and (Test-Path -LiteralPath (Join-Path $ServerFolder 'coreclr.dll'))
	if($machine -ne 0x8664 -or -not $isSelfContained){ $script:Refusals.Add(("$serverPath is not the self-contained x64 build (machine 0x{0:X}, own runtime {1})." -f $machine,$isSelfContained)) }
}
if(-not (Test-Path -LiteralPath $HostTestsDll)){ $script:Refusals.Add("The host tests are not built: no $HostTestsDll. Build MiddleTierHostTests (x64) through the lane first.") }
if(-not $script:DbClient){ $script:Refusals.Add("No MariaDB client (mariadb.exe or mysql.exe) on the PATH or in $env:ProgramFiles\MariaDB*\bin.") }
if(Test-Path -LiteralPath $WorkFolder){ $script:Refusals.Add("The temp folder $WorkFolder exists already.") }
if(-not (Test-DatabaseName $Database)){ $script:Refusals.Add("The database name $Database is not one this script may create.") }

if($PlanOnly){
	Write-Host "Test-MiddleTierService.ps1 -PlanOnly: the steps of a real run, none of them done."
	Write-Host "PowerShell: 64-bit; elevated: $isElevated (a real run needs it)"
	Write-Host "Server folder: $ServerFolder"
	Write-Host "MariaDB client: $(if($script:DbClient){ $script:DbClient }else{ '(none found)' })"
	Write-Host "Service: $ServiceName ($ServiceDisplayName)"
	Write-Host "Database: $Database on ${DbHost}:$DbPort"
	$portText=if($Port -gt 0){ "$Port" }else{ '<a free port>' }
	$number=1
	foreach($step in (Get-PlanSteps $ServerFolder $WorkFolder $OutDir $Database $portText $ClientSession.IsPresent)){
		Write-Host ("PLAN {0}. {1}" -f $number,$step)
		$number++
	}
	foreach($refusal in $script:Refusals){ Write-Host "A real run would refuse: $refusal" }
	Write-Host 'Nothing was changed.'
	Write-Host 'Exit code 0'
	exit 0
}
if($script:Refusals.Count -gt 0){
	foreach($refusal in $script:Refusals){ Write-Host $refusal }
	Write-Host 'Nothing was changed.'
	Write-Host 'Exit code 3'
	exit 3
}
try { [void](Invoke-Sql 'SELECT 1') }
catch {
	Write-Host "The database server on ${DbHost}:$DbPort does not answer as $DbUser with a blank password: $($_.Exception.Message)"
	Write-Host 'Nothing was changed.'
	Write-Host 'Exit code 3'
	exit 3
}
if(@(Invoke-Sql "SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME='$Database'")[0] -ne '0'){
	Write-Host "The database $Database exists already. Nothing was changed."
	Write-Host 'Exit code 3'
	exit 3
}
if($Port -le 0){ $Port=Get-FreePort }
$eventSourceBefore=Test-Path $EventLogSourceKey
$keyFilesBefore=Get-MachineKeyFiles
$start=Get-Date
#endregion

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
Write-Host "Test-MiddleTierService.ps1: service $ServiceName on https://${DbHost}:$Port, database $Database, logs $OutDir"
$serviceCreated=$false
$databaseReserved=$false
$rootAdded=$false
$thumbprint=''
$envNames=@('MIDDLETIER_TEST_URL','MIDDLETIER_TEST_DB','MIDDLETIER_TEST_CERT_THUMBPRINT','S3B6_KEEP_DB')
try {
	#region 1-3. The server folder, the certificate, the settings
	Write-Host '1. The server folder, the test certificate and the settings'
	Copy-Item -LiteralPath $ServerFolder -Destination $WorkFolder -Recurse
	Check (Test-Path -LiteralPath (Join-Path $WorkFolder $ServerExe)) "the server folder is copied to $WorkFolder ($(@(Get-ChildItem -LiteralPath $WorkFolder -Recurse -File).Count) files)"
	$pfxPath=Join-Path $WorkFolder 'mtsvc-test.pfx'
	$pfxPassword=New-PfxPassword
	$certificate=New-TestCertificatePfx $pfxPath $pfxPassword
	$thumbprint=$certificate.Thumbprint
	Check ((Test-Path -LiteralPath $pfxPath) -and $thumbprint -match '^[0-9A-F]{40}$') "the test certificate is the PFX file $pfxPath (thumbprint $thumbprint)"
	[System.IO.File]::WriteAllText((Join-Path $WorkFolder 'appsettings.json'),(New-AppSettingsJson $Port $pfxPath $pfxPassword),(New-Object System.Text.UTF8Encoding($false)))
	[System.IO.File]::WriteAllText((Join-Path $WorkFolder 'OpenDentalServerConfig.xml'),(New-ServerConfigXml $Database),(New-Object System.Text.UTF8Encoding($false)))
	Check $true "appsettings.json (https://${DbHost}:$Port, the PFX file) and OpenDentalServerConfig.xml ($Database) are written"
	#endregion

	#region 4. The scratch database
	Write-Host "2. The scratch database $Database"
	[void](Invoke-Sql "CREATE DATABASE ``$Database`` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci")
	$databaseReserved=$true
	Check (@(Invoke-Sql "SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME='$Database'")[0] -eq '1') "the scratch database $Database is reserved on ${DbHost}:$DbPort"
	#endregion

	#region 5-6. The service
	Write-Host "3. The Windows service $ServiceName"
	$exit=Invoke-Sc ("create {0} binPath= `"\`"{1}\`"`" start= demand DisplayName= `"{2}`"" -f $ServiceName,(Join-Path $WorkFolder $ServerExe),$ServiceDisplayName) 'sc-create.txt'
	$serviceCreated=($null -ne (Get-ServiceOrNull $ServiceName))
	Check ($exit -eq 0 -and $serviceCreated) "sc.exe create $ServiceName (exit code $exit)"
	if(-not $serviceCreated){ throw 'the service was not created' }
	$exit=Invoke-Sc "start $ServiceName" 'sc-start.txt'
	$running=Wait-ServiceStatus 'Running' 60
	Check ($exit -eq 0 -and $running) "sc.exe start ${ServiceName}: the service runs (exit code $exit)"
	if(-not $running){ throw 'the service does not run (its reason is in the Application event log, saved at the end)' }
	$get=$null
	$deadline=(Get-Date).AddSeconds(60)
	while((Get-Date) -lt $deadline){
		$get=Invoke-RawHttps $Port (New-RawRequest 'GET' $Port 0) $thumbprint
		if($get.Status -gt 0){ break }
		Start-Sleep -Seconds 1
	}
	Check ($get.Status -eq 405 -and $get.Response -match "(?m)^Allow: POST\r?$") "the service answers on https://${DbHost}:$Port : a GET gets 405 with Allow: POST (status $($get.Status) $($get.Error))"
	Check ($get.ServerThumbprint -eq $thumbprint) "the service shows the test certificate from the PFX file (shown: $($get.ServerThumbprint))"
	$big=Invoke-RawHttps $Port (New-RawRequest 'POST' $Port ($MaxRequestBodySize+1)) $thumbprint
	Check ($big.Status -eq 413) "a request one byte above the limit ($($MaxRequestBodySize+1) bytes declared) gets 413 (status $($big.Status) $($big.Error))"
	#endregion

	#region 7. The host round-trip tests against the service
	Write-Host '4. MiddleTierHostTests against the service'
	$env:MIDDLETIER_TEST_URL=Get-ServiceUrl $Port
	$env:MIDDLETIER_TEST_DB=$Database
	$env:MIDDLETIER_TEST_CERT_THUMBPRINT=$thumbprint
	$env:S3B6_KEEP_DB='1'
	$hostOut=Join-Path $OutDir 'host-tests.txt'
	& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $RepoRoot 'tools\run-tests.ps1') -Root $RepoRoot -Mode Targeted -Platform x64 -RequireBuildOutput -Include MiddleTierHostTests -Out $hostOut | Out-Host
	$lines=@(if(Test-Path -LiteralPath $hostOut){ Get-Content -LiteralPath $hostOut }else{ @() })
	$run=Test-HostTestRun $lines
	Check $run.Done "the test run finished ($hostOut)"
	$hostText=if($run.Host){ "total $($run.Host.Total), passed $($run.Host.Passed), failed $($run.Host.Failed), inconclusive $($run.Host.Skipped)" }else{ 'no result line' }
	Check $run.HostOk "MiddleTierHostTests against the service: no failure, at least 16 passed ($hostText)"
	Check $run.NoHqOk "NoHqTests: no failure"
	#endregion

	#region 8. The client session (opt-in)
	if($ClientSession){
		Write-Host '5. The client session'
		$public=New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 -ArgumentList (,[byte[]]$certificate.Cer)
		$store=New-Object System.Security.Cryptography.X509Certificates.X509Store -ArgumentList 'Root','LocalMachine'
		$store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
		try { $store.Add($public); $rootAdded=$true } finally { $store.Close() }
		Check $rootAdded "the test certificate is in Cert:\LocalMachine\Root for the session (removed at the end)"
		Write-Host ''
		Write-Host "  Start the program: $ProgramExe"
		Write-Host '  In the Choose Database window, choose the Middle Tier and give this address:'
		Write-Host "    $(Get-ServiceUrl $Port)"
		Write-Host '  Log in as s3b6usera (its password is in tests\MiddleTierHostTests\HostTestRun.cs), open a patient, then close the program.'
		$answer=Read-Host '  Did the program log in through the service and open a patient? (y/n)'
		Check ($answer -match '^\s*[yY]') "the client session through the service: log in as s3b6usera, open a patient, close (answer: $answer)"
	}
	#endregion
}
catch {
	$script:Failures.Add('the test stopped')
	Write-Host "  FAIL  the test stopped: $($_.Exception.Message)"
}
finally {
	Write-Host 'Clean-up'
	foreach($name in $envNames){ Remove-Item -Path "Env:\$name" -ErrorAction SilentlyContinue }
	if($serviceCreated){
		try {
			$service=Get-ServiceOrNull $ServiceName
			if($null -ne $service -and "$($service.Status)" -ne 'Stopped'){
				[void](Invoke-Sc "stop $ServiceName" 'sc-stop.txt')
				if(-not (Wait-ServiceStatus 'Stopped' 60)){
					#The service's own process, which this script started through the service control manager.
					$processId=(Get-CimInstance Win32_Service -Filter "Name='$ServiceName'").ProcessId
					if($processId -gt 0){ Stop-Process -Id $processId -Force; Start-Sleep -Seconds 2 }
				}
			}
			$exit=Invoke-Sc "delete $ServiceName" 'sc-delete.txt'
			$deadline=(Get-Date).AddSeconds(30)
			while(($null -ne (Get-ServiceOrNull $ServiceName)) -and (Get-Date) -lt $deadline){ Start-Sleep -Milliseconds 500 }
			Check ($null -eq (Get-ServiceOrNull $ServiceName)) "the service $ServiceName is stopped and deleted (sc.exe delete exit code $exit)"
		}
		catch { Check $false "the service $ServiceName is stopped and deleted: $($_.Exception.Message)" }
	}
	try {
		$events=@(Get-WinEvent -FilterHashtable @{ LogName='Application'; ProviderName=$EventLogSource; StartTime=$start } -ErrorAction SilentlyContinue)
		$events | Format-List TimeCreated,LevelDisplayName,Message | Out-File -FilePath (Join-Path $OutDir 'service-events.txt') -Encoding utf8
		Note "$($events.Count) Application event log entries of $EventLogSource since the start, in service-events.txt"
	}
	catch { Note "the service's event log entries could not be read: $($_.Exception.Message)" }
	if(Test-Path -LiteralPath $WorkFolder){
		$removed=$false
		for($i=0;$i -lt 10 -and -not $removed;$i++){
			try { Remove-Item -LiteralPath $WorkFolder -Recurse -Force; $removed=$true } catch { Start-Sleep -Seconds 1 }
		}
		Check $removed "the temp folder $WorkFolder is deleted"
	}
	if($databaseReserved){
		try {
			[void](Invoke-Sql "DROP DATABASE IF EXISTS ``$Database``")
			Check (@(Invoke-Sql "SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME='$Database'")[0] -eq '0') "the scratch database $Database is dropped"
		}
		catch { Check $false "the scratch database $Database is dropped: $($_.Exception.Message)" }
	}
	if(-not $eventSourceBefore -and (Test-Path $EventLogSourceKey)){
		try { Remove-Item -Path $EventLogSourceKey -Recurse -Force; Note "deleted the Event Log source key the service registered: $EventLogSourceKey" }
		catch { Check $false "the Event Log source key $EventLogSourceKey is deleted: $($_.Exception.Message)" }
	}
	if($rootAdded){
		try {
			$store=New-Object System.Security.Cryptography.X509Certificates.X509Store -ArgumentList 'Root','LocalMachine'
			$store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
			try {
				foreach($found in @($store.Certificates.Find([System.Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint,$thumbprint,$false))){ $store.Remove($found) }
				Check (@($store.Certificates.Find([System.Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint,$thumbprint,$false)).Count -eq 0) 'the test certificate is removed from Cert:\LocalMachine\Root'
			}
			finally { $store.Close() }
		}
		catch { Check $false "the test certificate is removed from Cert:\LocalMachine\Root: $($_.Exception.Message)" }
	}
	$keyFilesAfter=Get-MachineKeyFiles
	$newKeyFiles=@($keyFilesAfter.Keys | Where-Object { -not $keyFilesBefore.ContainsKey($_) } | Sort-Object)
	if($newKeyFiles.Count -gt 0){ Note "new key files in Windows' machine key folders since the start (likely the PFX key the service loaded; nothing deleted): $($newKeyFiles -join ', ')" }
}
$exitCode=0
if($script:Failures.Count -gt 0){ $exitCode=1 }
Write-Host ("{0} checks passed, {1} failed. Logs: {2}" -f $script:Passes,$script:Failures.Count,$OutDir)
Write-Host "Exit code $exitCode"
exit $exitCode
