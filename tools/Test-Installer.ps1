<#
.SYNOPSIS
Test-installs the Open PMS - Dental setup on this PC as an administrator, checks the result and uninstalls it. Elevated only.

.DESCRIPTION
HOW TO RUN IT. The owner runs it, never an agent (open point O-3 of docs\specs\net10-plan.md): at a stage gate, after the Release dry
run (tools\Build-Release.ps1 -OutDir <dryrun>, which writes <dryrun>\staging and <dryrun>\OpenPmsDental-<version>-Setup.exe). Open
Windows PowerShell with "Run as administrator" and paste this one line, with <repo> the repository folder and <dryrun> the dry run's
output folder:

  powershell -NoProfile -ExecutionPolicy Bypass -File "<repo>\tools\Test-Installer.ps1" -Setup "<dryrun>\OpenPmsDental-<version>-Setup.exe" -Staging "<dryrun>\staging"

Without elevation it changes nothing: it says that it needs an elevated PowerShell and ends with exit code 2. It also changes nothing and
ends with exit code 3 when it runs in a 32-bit PowerShell (it must read both registry views; the Windows PowerShell that "Run as
administrator" opens is 64-bit), when the setup or the staging folder is missing, or when this PC has a per-user uninstall entry of the
setup's AppId (this setup makes none). If a test install ever lands anywhere but its own test folder, the remaining scenarios are skipped
(a FAIL line says so), so it never installs over an installation already on the PC more than once.
Since stage 3b (docs\port-notes\S3b-7.md) the setup is 64-bit: it installs in 64-bit mode into Program Files, with its uninstall entry in
the 64-bit registry view, and the program carries its own .NET runtime (x64 beside the programs, x86 in Helper32\ for the helper), so no
.NET runtime is installed and none is needed. An installation of v0.0 (32-bit, its uninstall entry in the 32-bit view) is a separate
installation for this setup: the script does not touch it and checks at the end that its uninstall entry is as before.

WHAT IT CHANGES ON THIS PC while it runs, and what it puts back:
- new folders %TEMP%\OpenPmsDental-installtest-<scenario>-<id>\Open PMS - Dental, the test program folders (setup's /DIR=). The
  uninstall removes each one; the script then deletes the parent folders, which it made, and the A to Z folder of scenario B.
- the uninstall entry HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{D5640A25-F302-4CAD-96DC-84AAA8B22F98}_is1
  (the setup installs in 64-bit mode, so Windows keeps it in the 64-bit view; before stage 3b it was
  HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{D5640A25-F302-4CAD-96DC-84AAA8B22F98}_is1, the 32-bit view of the
  32-bit setup) and the Event Log source key
  HKLM\SYSTEM\CurrentControlSet\Services\EventLog\Application\OpenPMS-Dental. Setup writes both; each uninstall deletes both, which
  the script checks. Whatever an uninstall leaves of the two, the script deletes at the end (a FAIL line says so).
- When the PC has a 64-bit installation of the program already (C:\Program Files\Open PMS - Dental), the test installs share its
  uninstall entry and Event Log source key, and each test uninstall deletes them; an installation of v0.0 shares the Event Log source key.
  The script therefore saves both first, in memory and as .reg files in -OutDir (uninstall-entry-before.reg, eventlog-source-before.reg;
  double-click one to put it back by hand if the script was stopped), and writes them back at the end. That installation's folder, files and
  shortcuts are not touched; the last check compares every Start menu and desktop shortcut of the program with what was there before the
  test. Until the end, Apps does not list it, and the program's Event Log source is missing.
- the 64-bit TWAIN data source manager C:\Windows\System32\TWAINDSM.dll: setup installs it when it is missing or older, as a shared file its
  uninstall leaves in place, and counts it in HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDLLs. The script saves that count first
  and writes it back at the end; when the PC had no TWAINDSM.dll before, it deletes the one the test installs put there, so the PC ends as
  it was (a TWAIN driver's own setup installs the DSM where it needs one).
  When the PC had an OLDER TWAINDSM.dll, setup replaces it with 2.5.1 and it stays replaced: the script does not put the old file back.
- the 32-bit TWAIN data source manager C:\Windows\SysWOW64\TWAINDSM.dll (S3b-8: TWAIN sources with only a 32-bit driver run in the
  32-bit helper), treated exactly as the 64-bit one: setup installs it when it is missing or older, as a shared file its uninstall leaves
  in place, and counts it in the 32-bit view of SharedDLLs, HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\SharedDLLs, under
  the name 32-bit setups use for it, C:\Windows\System32\TWAINDSM.dll (what a 32-bit process sees). The script saves that count first and
  writes it back at the end; when the PC had no 32-bit TWAINDSM.dll before, it deletes the one the test installs put there.
  An OLDER 32-bit TWAINDSM.dll is replaced with 2.5.1 and stays replaced, as the 64-bit one.
- scenario A starts the installed connection settings tool (OpenPmsDentalDbSettings.exe, its window, without arguments, so it connects to
  nothing), reads which files the process loaded and closes it.
- the setup and uninstall logs in -OutDir (default: an install-test folder next to the setup), which it keeps.
Setup installs no shortcut and no Visual C++ runtime (/PREREQS=skip): /NOICONS skips the Start menu entry, and /MERGETASKS="!desktopicon"
turns the desktop task off explicitly, because setup would otherwise take the task choice of an installation already on the PC (the
task's 'unchecked' default applies only when there is none) and so overwrite, then uninstall, its desktop shortcut. The script never starts the
program (the setup has no [Run] entry) and never connects to a database: scenarios A, B and C give a database server on this computer,
for which a silent setup runs no connection settings tool, and the tool of scenario D stops before it connects.

A. Fresh install with the connection page's values on the command line (/DBSERVER=localhost:3307, /DBNAME, /DBUSER) and /NOICONS:
   - the program folder holds exactly the staging folder (same files, same SHA-256), FreeDentalConfig.xml and the uninstall folder, and the
     staging folder holds exactly Build-Release's list (tools\Build-Release.ps1 -ListFiles: the program part, the runtime part, Helper32\);
   - the program's own runtime: the installed connection settings tool, started with /PREREQS=skip and no .NET installed by setup, runs on
     the install folder's runtime (coreclr.dll and hostfxr.dll load from the install folder, nothing from %ProgramFiles%\dotnet), and closes;
   - the 64-bit TWAINDSM.dll is in the 64-bit system folder (x64, version 2.5.1.0 or later, signed by the TWAIN Working Group) and stays
     there after the uninstall (a shared file);
   - the 32-bit TWAINDSM.dll is in the 32-bit system folder (x86, version 2.5.1.0 or later, signed by the TWAIN Working Group) and stays
     there after the uninstall (a shared file);
   - FreeDentalConfig.xml has the given values, no password and NoShowOnStartup False, and setup added no access rule to it: it keeps
     the folder's rights, so BUILTIN\Users get no modify right from setup (only an administrator changes the connection settings, with
     the connection settings tool, docs/specs/db-connection-security.md);
   - a server on this computer, silent, needs no connection settings tool: the setup log has no run of it, and setup exits 0;
   - no uninstaller file at the top level, no new or changed Start menu or desktop shortcut (all users or this user);
   - the all-users uninstall entry (64-bit view) names this folder and the setup's version; there is no per-user entry;
   - the Event Log source key, with its message file;
   then the silent uninstall: the folder, the uninstall entry and the Event Log source key ([Registry] Flags: uninsdeletekey) are gone.
B. Reinstall over an installation: FreeDentalConfig.xml (a server on this computer) is kept as it is, and the upgrade runs the tool's
   /check only, which passes. Files a self-update added, a Logger folder and an A to Z images folder are put into the program folder; the
   uninstall removes everything except the A to Z folder, and the uninstall entry and the Event Log source key.
C. A folder that already holds other files is refused, nothing is written into it, and no uninstall entry or Event Log source is left.
D. A database server over the network (/DBSERVER=dbserver.example.test:3307) without the office CA: setup runs the connection settings
   tool /silent, the tool refuses (exit code 5, the office CA is missing) before it changes anything, and the silent setup ends with exit
   code 10. The files are installed, FreeDentalConfig.xml is the one setup wrote, and the tool added nothing to the program folder. Then
   the uninstall, as in A.

Prints one "ok" or "FAIL" line per check, then a summary and the exit code: 0 every check passed, 1 a check failed, 2 not elevated,
3 the PC or the arguments do not allow the test (nothing changed).
#>
[CmdletBinding(PositionalBinding=$false)]
param(
	[string]$Setup='',
	[string]$Staging='',
	[string]$OutDir=''
)
Set-StrictMode -Version 2
$ErrorActionPreference='Stop'

#Before anything else: an unelevated run changes nothing.
$principal=New-Object System.Security.Principal.WindowsPrincipal([System.Security.Principal.WindowsIdentity]::GetCurrent())
if(-not $principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)){
	Write-Host 'Test-Installer.ps1 needs an elevated PowerShell: the setup always installs for all users, as an administrator.'
	Write-Host 'Open Windows PowerShell with "Run as administrator" and run the line in the header of this script. Nothing was changed.'
	Write-Host 'Exit code 2'
	exit 2
}
#A 32-bit PowerShell sees HKLM\SOFTWARE\Microsoft redirected to WOW6432Node, so its 64-bit view check would be wrong: refuse it, changing nothing.
if(-not [Environment]::Is64BitProcess){
	Write-Host 'Test-Installer.ps1 needs the 64-bit Windows PowerShell (this one is 32-bit). Open Windows PowerShell with "Run as administrator"'
	Write-Host 'from the Start menu (not "Windows PowerShell (x86)") and run the line in the header of this script. Nothing was changed.'
	Write-Host 'Exit code 3'
	exit 3
}

$RepoRoot=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$AppId='{D5640A25-F302-4CAD-96DC-84AAA8B22F98}_is1'
#The setup installs in 64-bit mode (S3b-7), so Windows keeps its all-users uninstall entry in the 64-bit view. The 32-bit view's entry is that of
#an installation of v0.0 (the 32-bit setup), which the test leaves alone.
$UninstallKey="HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$AppId"
$UninstallKey32="HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\$AppId"
$UninstallKeyUser="HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\$AppId"
#The program's Event Log source ([Registry] of installer\OpenPmsDental.iss, ProgramNames.EventLogSource).
$EventLogKey='HKLM:\SYSTEM\CurrentControlSet\Services\EventLog\Application\OpenPMS-Dental'
$EventMessageFile='%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\EventLogMessages.dll'
$DbSettingsExe='OpenPmsDentalDbSettings.exe'
#The 64-bit TWAIN data source manager the setup installs into the 64-bit system folder ([Files] of the .iss), and its shared-file count.
$TwainDsmPath=Join-Path ([Environment]::SystemDirectory) 'TWAINDSM.dll'
$TwainDsmMinimumVersion=[version]'2.5.1.0'
$SharedDllsKey='HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDLLs'
#The 32-bit TWAIN data source manager the setup installs into the 32-bit system folder (S3b-8), and its shared-file count in the 32-bit view
#([Files] of the .iss: the 32bit flag).
$TwainDsm32Path=Join-Path ([Environment]::GetFolderPath('SystemX86')) 'TWAINDSM.dll'
$SharedDllsKey32='HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\SharedDLLs'
#The count's name: the file's path as a 32-bit process sees it ({sys} with the 32bit flag; Inno Setup's sharedfile documentation).
$TwainDsm32CountName=Join-Path ([Environment]::SystemDirectory) 'TWAINDSM.dll'
$NetworkServer='dbserver.example.test:3307'
$ShortcutFolders=@('CommonPrograms','CommonDesktopDirectory','Programs','Desktop') | ForEach-Object { [Environment]::GetFolderPath($_) }
$script:Failures=New-Object 'System.Collections.Generic.List[string]'
$script:Passes=0
$script:TestFolders=New-Object 'System.Collections.Generic.List[string]'

function Stop-Unchanged([string]$why,[string]$changed='Nothing was changed.'){
	Write-Host $why
	Write-Host $changed
	Write-Host 'Exit code 3'
	exit 3
}

#The sub key of HKLM under a PowerShell path HKLM:\..., for the Registry class.
function Get-HklmSubKey([string]$path){
	return $path.Substring('HKLM:\'.Length)
}

#The values of an HKLM key (name, data unexpanded, kind), or $null when the key is missing. The two keys the test installs share with an
#installation have values only; a sub key stops the test before any change.
function Save-HklmKey([string]$path){
	$key=[Microsoft.Win32.Registry]::LocalMachine.OpenSubKey((Get-HklmSubKey $path))
	if($key -eq $null){ return $null }
	try {
		if($key.SubKeyCount -gt 0){ Stop-Unchanged "$path has sub keys, which this script cannot put back." }
		$values=New-Object 'System.Collections.Generic.List[object]'
		foreach($name in $key.GetValueNames()){
			$values.Add(@($name,$key.GetValue($name,$null,[Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames),$key.GetValueKind($name)))
		}
		return ,$values
	}
	finally { $key.Close() }
}

#Writes the key back as it was saved: its values only, after removing what a test install left in it.
function Restore-HklmKey([string]$path,$values){
	if(Test-Path $path){ Remove-Item -Path $path -Recurse -Force }
	$key=[Microsoft.Win32.Registry]::LocalMachine.CreateSubKey((Get-HklmSubKey $path))
	try { foreach($value in $values){ $key.SetValue($value[0],$value[1],$value[2]) } }
	finally { $key.Close() }
}

#The program's shortcuts in the Start menu and on the desktop, all users and this user, with their times: an installation's own stay as
#they are, and the test installs add none.
function Get-Shortcuts {
	$map=@{}
	foreach($folder in $ShortcutFolders){
		foreach($link in @(Get-ChildItem -LiteralPath $folder -Filter 'Open PMS - Dental*.lnk' -File -Force -ErrorAction SilentlyContinue)){
			$map[$link.FullName]=$link.LastWriteTimeUtc.Ticks
		}
	}
	return $map
}

function Check([bool]$condition,[string]$what){
	if($condition){ $script:Passes++; Write-Host "  ok    $what" }
	else{ $script:Failures.Add($what); Write-Host "  FAIL  $what" }
}

function Get-Pids([string]$name){
	return @(Get-Process -Name $name -ErrorAction SilentlyContinue | ForEach-Object { $_.Id })
}

function New-TestFolder([string]$name){
	$parent=Join-Path $env:TEMP ("OpenPmsDental-installtest-{0}-{1}" -f $name,[guid]::NewGuid().ToString('N').Substring(0,8))
	$script:TestFolders.Add($parent)
	return (Join-Path $parent 'Open PMS - Dental')
}

function Invoke-Setup([string]$dir,[string]$log,[string[]]$extra){
	#/MERGETASKS="!desktopicon" turns the desktop task off explicitly: the test installs share the AppId of an installation already on
	#the PC, so setup restores that installation's tasks (UsePreviousTasks), and 'Flags: unchecked' applies only without one. Without it,
	#an installation set up with the desktop icon would have its common desktop shortcut overwritten and then deleted by the test uninstall.
	$arguments=@('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/NOICONS','/MERGETASKS="!desktopicon"','/PREREQS=skip',"/DIR=`"$dir`"","/LOG=`"$log`"")+$extra
	$process=Start-Process -FilePath $Setup -ArgumentList $arguments -Wait -PassThru
	return $process.ExitCode
}

function Invoke-Uninstall([string]$dir,[string]$log){
	$uninstaller=Join-Path $dir 'uninstall\unins000.exe'
	if(-not (Test-Path -LiteralPath $uninstaller)){ return -1 }
	$process=Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',"/LOG=`"$log`"") -Wait -PassThru
	#The uninstaller runs its second phase from a copy in %TEMP%; wait (up to a minute) until it has removed its own files.
	$deadline=(Get-Date).AddSeconds(60)
	while((Test-Path -LiteralPath $uninstaller) -and (Get-Date) -lt $deadline){ Start-Sleep -Milliseconds 500 }
	return $process.ExitCode
}

function Get-FileMap([string]$folder){
	$map=@{}
	foreach($file in Get-ChildItem -LiteralPath $folder -Recurse -File -Force){
		$map[$file.FullName.Substring($folder.Length+1)]=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
	}
	return $map
}

#A registry value, or $null when the key or the value is missing (Get-ItemProperty would stop the script under strict mode).
function Get-RegValue([string]$path,[string]$name){
	if(-not (Test-Path $path)){ return $null }
	return (Get-Item $path).GetValue($name,$null,[Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
}

function Read-Log([string]$path){
	if(Test-Path -LiteralPath $path){ return [System.IO.File]::ReadAllText($path) }
	return ''
}

#The program folder holds exactly the staging folder, FreeDentalConfig.xml and the uninstall folder.
#Build-Release's list of the program folder (tools\Build-Release.ps1 -ListFiles): path to kind (own, third, doc, runtime); the setup file
#(setup|TWAINDSM.dll) goes elsewhere. $null when the script fails.
function Get-ReleaseList {
	$lines=@(& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $RepoRoot 'tools\Build-Release.ps1') -ListFiles 2>$null)
	if($LASTEXITCODE -ne 0){ return $null }
	$map=@{}
	foreach($line in $lines){
		$parts="$line".Split('|')
		if($parts.Count -eq 2 -and $parts[0] -ne 'setup'){ $map[$parts[1]]=$parts[0] }
	}
	return $map
}

#The staging folder is exactly Build-Release's list: the program part (own, third-party and document files), the runtime part (the x64 runtime
#and, in Helper32\, the helper's x86 runtime) and the helper itself.
function Test-StagingIsTheReleaseList {
	if($null -eq $releaseList){ Check $false 'Build-Release.ps1 -ListFiles ran'; return }
	$missing=@($releaseList.Keys | Where-Object { -not $stagingMap.ContainsKey($_) } | Sort-Object)
	$extra=@($stagingMap.Keys | Where-Object { -not $releaseList.ContainsKey($_) } | Sort-Object)
	$runtime=@($releaseList.Keys | Where-Object { $releaseList[$_] -eq 'runtime' })
	$helper=@($releaseList.Keys | Where-Object { $_.StartsWith('Helper32\') })
	Check ($missing.Count -eq 0 -and $extra.Count -eq 0) ("the staging folder is Build-Release's list: {0} files, the runtime part {1}, the program part {2}, {3} in Helper32\ (not staged: {4}; not listed: {5})" -f $releaseList.Count,$runtime.Count,($releaseList.Count-$runtime.Count),$helper.Count,($missing -join ', '),($extra -join ', '))
	Check ($runtime.Count -gt 0 -and @($runtime | Where-Object { $_.StartsWith('Helper32\') }).Count -gt 0) 'the list has both runtimes, the x64 one and the helper''s x86 one in Helper32\'
}

#The installed connection settings tool runs on the install folder's own runtime (decision 20): started without arguments (its window; it
#connects to nothing), its process loads coreclr.dll and hostfxr.dll from the install folder and nothing from %ProgramFiles%\dotnet, even on a
#PC with machine-wide .NET runtimes. Then it is closed, and it must have ended.
function Test-OwnRuntime([string]$dir){
	$tool=Join-Path $dir $DbSettingsExe
	if(-not (Test-Path -LiteralPath $tool)){ Check $false "$DbSettingsExe installed"; return }
	$process=Start-Process -FilePath $tool -WorkingDirectory $dir -PassThru
	$modules=@()
	try {
		$deadline=(Get-Date).AddSeconds(60)
		while((Get-Date) -lt $deadline -and -not $process.HasExited){
			try { $process.Refresh(); $modules=@($process.Modules | ForEach-Object { $_.FileName }) } catch { $modules=@() }
			if(@($modules | Where-Object { (Split-Path $_ -Leaf) -eq 'coreclr.dll' }).Count -gt 0){ break }
			Start-Sleep -Milliseconds 500
		}
	}
	finally {
		if(-not $process.HasExited){
			[void]$process.CloseMainWindow()
			if(-not $process.WaitForExit(10000)){ $process.Kill(); [void]$process.WaitForExit(10000) }
		}
	}
	$folder=$dir.TrimEnd('\')+'\'
	$coreclr=@($modules | Where-Object { (Split-Path $_ -Leaf) -eq 'coreclr.dll' })
	$hostfxr=@($modules | Where-Object { (Split-Path $_ -Leaf) -eq 'hostfxr.dll' })
	Check ($coreclr.Count -eq 1 -and $coreclr[0].StartsWith($folder,[StringComparison]::OrdinalIgnoreCase)) "the installed $DbSettingsExe runs on the install folder's coreclr.dll (loaded: $($coreclr -join ', '))"
	Check ($hostfxr.Count -eq 1 -and $hostfxr[0].StartsWith($folder,[StringComparison]::OrdinalIgnoreCase)) "its hostfxr.dll is the install folder's (loaded: $($hostfxr -join ', '))"
	$dotnet=(Join-Path $env:ProgramFiles 'dotnet').TrimEnd('\')+'\'
	$machineWide=@($modules | Where-Object { $_.StartsWith($dotnet,[StringComparison]::OrdinalIgnoreCase) })
	Check ($machineWide.Count -eq 0) "nothing loaded from $dotnet (loaded: $($machineWide -join ', '))"
	Check $process.HasExited "the connection settings tool was closed"
}

#The 64-bit TWAIN data source manager in the 64-bit system folder: x64, at least the approved version, signed by the TWAIN Working Group.
function Test-TwainDsm([string]$what){
	if(-not (Test-Path -LiteralPath $TwainDsmPath)){ Check $false "TWAINDSM.dll is in the 64-bit system folder $what"; return }
	$bytes=[System.IO.File]::ReadAllBytes($TwainDsmPath)
	$offset=[BitConverter]::ToInt32($bytes,0x3C)
	$machine=[BitConverter]::ToUInt16($bytes,$offset+4)
	$version=[version]([System.Diagnostics.FileVersionInfo]::GetVersionInfo($TwainDsmPath).FileVersion)
	$signature=Get-AuthenticodeSignature -LiteralPath $TwainDsmPath
	$subject=if($signature.SignerCertificate){ $signature.SignerCertificate.Subject }else{ '' }
	Check (($machine -eq 0x8664) -and ($version -ge $TwainDsmMinimumVersion) -and ($signature.Status -eq 'Valid') -and ($subject -like '*O=TWAIN Working Group*')) ("the 64-bit TWAINDSM.dll is in the system folder {0} (machine 0x{1:X}, version {2}, signature {3}, {4})" -f $what,$machine,$version,$signature.Status,$subject)
}

#The 32-bit TWAIN data source manager in the 32-bit system folder (S3b-8): x86, at least the approved version, signed by the TWAIN Working Group.
function Test-TwainDsm32([string]$what){
	if(-not (Test-Path -LiteralPath $TwainDsm32Path)){ Check $false "the 32-bit TWAINDSM.dll is in the 32-bit system folder $what"; return }
	$bytes=[System.IO.File]::ReadAllBytes($TwainDsm32Path)
	$offset=[BitConverter]::ToInt32($bytes,0x3C)
	$machine=[BitConverter]::ToUInt16($bytes,$offset+4)
	$version=[version]([System.Diagnostics.FileVersionInfo]::GetVersionInfo($TwainDsm32Path).FileVersion)
	$signature=Get-AuthenticodeSignature -LiteralPath $TwainDsm32Path
	$subject=if($signature.SignerCertificate){ $signature.SignerCertificate.Subject }else{ '' }
	Check (($machine -eq 0x14C) -and ($version -ge $TwainDsmMinimumVersion) -and ($signature.Status -eq 'Valid') -and ($subject -like '*O=TWAIN Working Group*')) ("the 32-bit TWAINDSM.dll is in the 32-bit system folder {0} (machine 0x{1:X}, version {2}, signature {3}, {4})" -f $what,$machine,$version,$signature.Status,$subject)
}

function Test-ProgramFolder([string]$dir){
	Check (Test-Path -LiteralPath (Join-Path $dir 'OpenPmsDental.exe')) 'OpenPmsDental.exe installed'
	if(-not (Test-Path -LiteralPath $dir)){ return }
	$installedMap=Get-FileMap $dir
	$extra=@($installedMap.Keys | Where-Object { -not $stagingMap.ContainsKey($_) } | Sort-Object)
	$missing=@($stagingMap.Keys | Where-Object { -not $installedMap.ContainsKey($_) } | Sort-Object)
	$changed=@($stagingMap.Keys | Where-Object { $installedMap.ContainsKey($_) -and $installedMap[$_] -ne $stagingMap[$_] } | Sort-Object)
	Check ($missing.Count -eq 0) "every staged file installed (missing: $($missing -join ', '))"
	Check ($changed.Count -eq 0) "every installed file equals the staged one (different: $($changed -join ', '))"
	$extraExpected=@('FreeDentalConfig.xml','uninstall\unins000.dat','uninstall\unins000.exe')
	$extraOther=@($extra | Where-Object { $extraExpected -notcontains $_ })
	Check ($extraOther.Count -eq 0) "nothing else in the program folder besides FreeDentalConfig.xml and the uninstall folder (also: $($extraOther -join ', '))"
	foreach($name in $extraExpected){ Check ($extra -contains $name) "installed $name" }
	Check (@(Get-ChildItem -LiteralPath $dir -Filter 'unins*' -File).Count -eq 0) 'no uninstaller file at the top level'
}

#FreeDentalConfig.xml as setup writes it: the given values, no password, and the folder's rights.
function Test-SetupConfig([string]$dir,[string]$server){
	$configPath=Join-Path $dir 'FreeDentalConfig.xml'
	if(-not (Test-Path -LiteralPath $configPath)){ Check $false 'FreeDentalConfig.xml exists'; return }
	[xml]$config=[System.IO.File]::ReadAllText($configPath)
	$connection=$config.ConnectionSettings.DatabaseConnection
	Check ($connection.ComputerName -eq $server) "config ComputerName $server (was $($connection.ComputerName))"
	Check ($connection.Database -eq 'opms_test') "config Database opms_test (was $($connection.Database))"
	Check ($connection.User -eq 'opms_user') "config User opms_user (was $($connection.User))"
	Check (($connection.Password -eq '') -and ($connection.MySQLPassHash -eq '')) 'config has no password'
	$names=@($connection.ChildNodes | ForEach-Object { $_.Name })
	Check (@($names | Where-Object { @('MySQLPassDpapi','SslCa','ClientCertThumbprint') -contains $_ }).Count -eq 0) "config has no stored password, office CA or client certificate (elements: $($names -join ', '))"
	Check ($connection.NoShowOnStartup -eq 'False') 'config NoShowOnStartup False'
	Check ($config.ConnectionSettings.DatabaseType -eq 'MySql') 'config DatabaseType MySql'
	#Setup gives Users no modify rights (owner decision Q7): every rule on the file comes from its folder.
	$usersSid=New-Object System.Security.Principal.SecurityIdentifier('S-1-5-32-545')
	$acl=Get-Acl -LiteralPath $configPath
	$explicitRules=@($acl.Access | Where-Object { -not $_.IsInherited })
	Check ($explicitRules.Count -eq 0) "setup added no access rule to FreeDentalConfig.xml (explicit: $(@($explicitRules | ForEach-Object { $_.IdentityReference.Value }) -join ', '))"
	Check (-not $acl.AreAccessRulesProtected) 'FreeDentalConfig.xml keeps the rights of its folder'
	$usersModify=@($acl.Access | Where-Object {
		-not $_.IsInherited -and $_.AccessControlType -eq 'Allow' -and $_.IdentityReference.Translate([System.Security.Principal.SecurityIdentifier]).Equals($usersSid) -and
		($_.FileSystemRights -band [System.Security.AccessControl.FileSystemRights]::Modify) -eq [System.Security.AccessControl.FileSystemRights]::Modify })
	Check ($usersModify.Count -eq 0) 'setup gave BUILTIN\Users no modify right on FreeDentalConfig.xml'
}

#What an installation leaves in the registry: the all-users uninstall entry of this folder, no per-user one, and the Event Log source.
function Test-Installed([string]$dir){
	Check (Test-Path $UninstallKey) 'all-users uninstall entry written (64-bit view)'
	if(Test-Path $UninstallKey){
		$name=[string](Get-RegValue $UninstallKey 'DisplayName')
		#Inno Setup marks the name when the same AppId also has an entry in the other registry view (here a v0.0 install in the 32-bit view):
		#'Marked uninstall display name to avoid duplicate entries' in the setup log, so Apps lists 'Open PMS - Dental (64-bit)' beside v0.0.
		$nameExpected=if(Test-Path $UninstallKey32){ 'Open PMS - Dental (64-bit)' } else { 'Open PMS - Dental' }
		Check ($name -ceq $nameExpected) "uninstall entry name $nameExpected (was $name)"
		$version=[string](Get-RegValue $UninstallKey 'DisplayVersion')
		if($ExpectedVersion){ Check ($version -eq $ExpectedVersion) "uninstall entry version $ExpectedVersion (was $version)" }
		$appPath=[string](Get-RegValue $UninstallKey 'Inno Setup: App Path')
		Check ($appPath.TrimEnd('\') -eq $dir.TrimEnd('\')) "uninstall entry names the test folder (was $appPath)"
		if($appPath.TrimEnd('\') -ne $dir.TrimEnd('\')){
			#Setup did not install into the test folder (/DIR= ignored): stop here rather than install over that folder again.
			throw "the test install landed in $appPath, not in its test folder; the remaining scenarios are skipped"
		}
	}
	Check (-not (Test-Path $UninstallKeyUser)) 'no per-user uninstall entry'
	Check (Test-Path $EventLogKey) 'the Event Log source key is registered'
	if(Test-Path $EventLogKey){
		$value=[string](Get-RegValue $EventLogKey 'EventMessageFile')
		$kind=''
		if($value){ $kind=[string](Get-Item $EventLogKey).GetValueKind('EventMessageFile') }
		Check (($value -eq $EventMessageFile) -and ($kind -eq 'ExpandString')) "the Event Log source's EventMessageFile is $EventMessageFile, REG_EXPAND_SZ (was $value, $kind)"
	}
	$links=Get-Shortcuts
	$new=@($links.Keys | Where-Object { (-not $shortcutsBefore.ContainsKey($_)) -or ($shortcutsBefore[$_] -ne $links[$_]) } | Sort-Object)
	Check ($new.Count -eq 0) "no new or changed Start menu or desktop shortcut with /NOICONS and the desktop task off (found: $($new -join ', '))"
}

#What an uninstall leaves in the registry: nothing of the two keys ([Registry] Flags: uninsdeletekey).
function Test-Uninstalled([string]$what){
	Check (-not (Test-Path $UninstallKey)) "the uninstall entry is gone $what"
	Check (-not (Test-Path $EventLogKey)) "the Event Log source key is gone $what (uninsdeletekey)"
}

#region Before the first change
if(-not $Staging){ $Staging=Join-Path $RepoRoot 'installer\bin\staging' }
if(-not $Setup){
	$found=Get-ChildItem -LiteralPath (Join-Path $RepoRoot 'installer\bin') -Filter 'OpenPmsDental-*-Setup.exe' -ErrorAction SilentlyContinue | Select-Object -First 1
	if($found){ $Setup=$found.FullName }
}
if(-not $Setup -or -not (Test-Path -LiteralPath $Setup -PathType Leaf)){ Stop-Unchanged "No setup found ($Setup); pass -Setup with the setup of a Release dry run (tools\Build-Release.ps1)." }
if(-not (Test-Path -LiteralPath $Staging -PathType Container)){ Stop-Unchanged "No staging folder at $Staging; pass -Staging with the dry run's staging folder." }
$Setup=(Resolve-Path -LiteralPath $Setup).Path
$Staging=(Resolve-Path -LiteralPath $Staging).Path
foreach($key in @($UninstallKeyUser)){
	if(Test-Path $key){ Stop-Unchanged "This PC has an uninstall entry of the program that this setup does not make ($key). Remove that installation first, or run the test on another PC." }
}
#An installation of v0.0 (32-bit view): not touched; its entry is compared at the end.
$savedUninstall32=Save-HklmKey $UninstallKey32
#The 64-bit TWAINDSM.dll and its shared-file count, as they were: written back at the end.
$twainDsmBefore=Test-Path -LiteralPath $TwainDsmPath
$sharedCountBefore=Get-RegValue $SharedDllsKey $TwainDsmPath
#The 32-bit TWAINDSM.dll and its count in the 32-bit view, the same way.
$twainDsm32Before=Test-Path -LiteralPath $TwainDsm32Path
$sharedCount32Before=Get-RegValue $SharedDllsKey32 $TwainDsm32CountName
#An installation of the program on this PC: the test installs share these two keys, so they are saved here and written back at the end.
$savedUninstall=Save-HklmKey $UninstallKey
$savedEventLog=Save-HklmKey $EventLogKey
if($null -ne $savedUninstall){
	Write-Host ("This PC has an installation of the program in {0}; its uninstall entry and Event Log source key are saved and written back at the end." -f (Get-RegValue $UninstallKey 'Inno Setup: App Path'))
}
$shortcutsBefore=Get-Shortcuts
if(-not $OutDir){ $OutDir=Join-Path (Split-Path -Parent $Setup) 'install-test' }
$ExpectedVersion=''
if((Split-Path -Leaf $Setup) -match '^OpenPmsDental-(.+)-Setup\.exe$'){ $ExpectedVersion=$Matches[1] }
#endregion

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
#The saved keys as .reg files too, to put back by hand if the script is stopped before its end.
foreach($pair in @(@($savedUninstall,$UninstallKey,'uninstall-entry-before.reg'),@($savedEventLog,$EventLogKey,'eventlog-source-before.reg'))){
	if($null -eq $pair[0]){ continue }
	$regFile=Join-Path $OutDir $pair[2]
	& reg.exe export ('HKLM\'+(Get-HklmSubKey $pair[1])) $regFile /y | Out-Null
	if($LASTEXITCODE -ne 0){ Stop-Unchanged "reg.exe could not save $($pair[1]) to $regFile (exit $LASTEXITCODE)." "Nothing was installed; only $OutDir was made." }
	Write-Host "Saved $($pair[1]) to $regFile"
}
Write-Host "Setup: $Setup"
Write-Host ("Setup SHA-256: {0}" -f (Get-FileHash -LiteralPath $Setup -Algorithm SHA256).Hash.ToLowerInvariant())
Write-Host "Staging: $Staging"
$pidsBefore=Get-Pids 'OpenPmsDental'
$stagingMap=Get-FileMap $Staging
$releaseList=Get-ReleaseList

try {
	#region A. Fresh install, a database server on this computer
	Write-Host 'A. Fresh install for all users, a database server on this computer'
	$dirA=New-TestFolder 'a'
	$exit=Invoke-Setup $dirA (Join-Path $OutDir 'a-install.log') @('/DBSERVER=localhost:3307','/DBNAME=opms_test','/DBUSER=opms_user')
	Check ($exit -eq 0) "setup exit code 0 (was $exit)"
	Test-StagingIsTheReleaseList
	Test-ProgramFolder $dirA
	Test-SetupConfig $dirA 'localhost:3307'
	Test-Installed $dirA
	Test-TwainDsm 'after the install'
	Test-TwainDsm32 'after the install'
	Test-OwnRuntime $dirA
	$logA=Read-Log (Join-Path $OutDir 'a-install.log')
	Check ($logA -match 'Installation process succeeded') 'the setup log says the installation succeeded'
	Check ($logA -match 'Wrote FreeDentalConfig\.xml') 'the setup log records the config file'
	Check ($logA -notmatch "Running $([regex]::Escape($DbSettingsExe))") 'a silent setup for a server on this computer runs no connection settings tool'
	$exit=Invoke-Uninstall $dirA (Join-Path $OutDir 'a-uninstall.log')
	Check ($exit -eq 0) "uninstall exit code 0 (was $exit)"
	Check (-not (Test-Path -LiteralPath $dirA)) 'the program folder is gone after the uninstall'
	Test-Uninstalled 'after A'
	Test-TwainDsm 'after the uninstall (a shared file the uninstall leaves)'
	Test-TwainDsm32 'after the uninstall (a shared file the uninstall leaves)'
	#endregion

	#region B. Reinstall, self-update files, A to Z folder
	Write-Host 'B. Reinstall over an installation, then uninstall with files the self-update added'
	$dirB=New-TestFolder 'b'
	$exit=Invoke-Setup $dirB (Join-Path $OutDir 'b-install-1.log') @('/DBSERVER=127.0.0.1','/DBNAME=opms_first','/DBUSER=first_user')
	Check ($exit -eq 0) "first setup exit code 0 (was $exit)"
	$configB=Join-Path $dirB 'FreeDentalConfig.xml'
	#A server on this computer, so the upgrade's /check passes without an office CA. The second setup below passes a network name on the
	#command line on purpose: an upgrade keeps this config and takes no page value, which the checks prove.
	$saved='<?xml version="1.0" encoding="utf-8"?><ConnectionSettings><DatabaseConnection><ComputerName>localhost</ComputerName><Database>saved_db</Database><User>saved_user</User><Password></Password><MySQLPassHash></MySQLPassHash><NoShowOnStartup>True</NoShowOnStartup></DatabaseConnection><DatabaseType>MySql</DatabaseType><UseDynamicMode>False</UseDynamicMode></ConnectionSettings>'
	if(Test-Path -LiteralPath $dirB){ [System.IO.File]::WriteAllText($configB,$saved) }
	$exit=Invoke-Setup $dirB (Join-Path $OutDir 'b-install-2.log') @('/DBSERVER=second.example.test','/DBNAME=opms_second','/DBUSER=second_user')
	Check ($exit -eq 0) "second setup exit code 0 (was $exit)"
	Check ((Test-Path -LiteralPath $configB) -and ([System.IO.File]::ReadAllText($configB) -eq $saved)) 'a reinstall keeps FreeDentalConfig.xml as it is'
	$logB=Read-Log (Join-Path $OutDir 'b-install-2.log')
	Check ($logB -match 'FreeDentalConfig\.xml exists already and is kept as it is') 'the setup log says the config file is kept'
	Check ($logB -match "Running $([regex]::Escape($DbSettingsExe)) /check") 'the upgrade runs the connection settings tool''s /check'
	Check ($logB -match "$([regex]::Escape($DbSettingsExe)) exit code 0") 'the /check of the kept settings passes'
	Check ($logB -notmatch "Running $([regex]::Escape($DbSettingsExe)) /server=") 'the upgrade passes none of the connection page''s values to the tool'
	Test-Installed $dirB
	if(Test-Path -LiteralPath $dirB){
		[System.IO.File]::WriteAllText((Join-Path $dirB 'AddedBySelfUpdate.dll'),'synthetic')
		New-Item -ItemType Directory -Path (Join-Path $dirB 'Logger') | Out-Null
		[System.IO.File]::WriteAllText((Join-Path $dirB 'Logger\synthetic.log'),'synthetic')
		$atoz=Join-Path $dirB 'Images'
		New-Item -ItemType Directory -Path (Join-Path $atoz 'A\SyntheticPatient1') | Out-Null
		[System.IO.File]::WriteAllText((Join-Path $atoz 'A\SyntheticPatient1\note.txt'),'synthetic')
	}
	$exit=Invoke-Uninstall $dirB (Join-Path $OutDir 'b-uninstall.log')
	Check ($exit -eq 0) "uninstall exit code 0 (was $exit)"
	$left=@(Get-ChildItem -LiteralPath $dirB -Force -ErrorAction SilentlyContinue | ForEach-Object { $_.Name })
	Check (($left.Count -eq 1) -and ($left[0] -eq 'Images')) "only the A to Z folder is left (left: $($left -join ', '))"
	Check (Test-Path -LiteralPath (Join-Path $dirB 'Images\A\SyntheticPatient1\note.txt')) 'the A to Z folder is untouched'
	Test-Uninstalled 'after B'
	#endregion

	#region C. A folder with other files
	Write-Host 'C. A folder that holds other files'
	$dirC=New-TestFolder 'c'
	New-Item -ItemType Directory -Path $dirC | Out-Null
	[System.IO.File]::WriteAllText((Join-Path $dirC 'other.txt'),'synthetic')
	$exit=Invoke-Setup $dirC (Join-Path $OutDir 'c-install.log') @('/DBSERVER=localhost')
	Check ($exit -ne 0) "setup refuses the folder (exit code $exit)"
	$leftC=@(Get-ChildItem -LiteralPath $dirC -Force | ForEach-Object { $_.Name })
	Check (($leftC.Count -eq 1) -and ($leftC[0] -eq 'other.txt')) "nothing written into it (holds: $($leftC -join ', '))"
	Check (-not (Test-Path $UninstallKey)) 'no uninstall entry'
	Check (-not (Test-Path $EventLogKey)) 'no Event Log source key'
	#endregion

	#region D. A server over the network without the office CA: the tool runs /silent and fails, setup ends with 10
	Write-Host 'D. A database server over the network without the office CA: the connection settings tool fails, setup exit code 10'
	$dirD=New-TestFolder 'd'
	$dbSettingsBefore=Get-Pids ([System.IO.Path]::GetFileNameWithoutExtension($DbSettingsExe))
	$exit=Invoke-Setup $dirD (Join-Path $OutDir 'd-install.log') @("/DBSERVER=$NetworkServer",'/DBNAME=opms_test','/DBUSER=opms_user')
	Check ($exit -eq 10) "setup exit code 10, the connection settings step did not finish (was $exit)"
	Test-ProgramFolder $dirD
	Test-SetupConfig $dirD $NetworkServer
	Test-Installed $dirD
	$logD=Read-Log (Join-Path $OutDir 'd-install.log')
	Check ($logD -match 'Installation process succeeded') 'the setup log says the installation succeeded'
	#Inno Setup's AddQuotes quotes a value only when it has a space.
	$toolLine="Running $DbSettingsExe /server=$NetworkServer /database=opms_test /user=opms_user /silent"
	Check ($logD.Contains($toolLine)) "setup ran the tool silently with the page's values ($toolLine)"
	Check ($logD -match "$([regex]::Escape($DbSettingsExe)) exit=5 A database server over the network \($([regex]::Escape($NetworkServer))\) needs the office CA certificate file") 'the tool''s result line: exit 5, the office CA is missing'
	Check ($logD -match "$([regex]::Escape($DbSettingsExe)) exit code 5") 'the setup log has the tool''s exit code 5'
	Check ($logD -match 'Setup exit code 10: the connection settings were not completed') 'the setup log says why it ends with 10'
	$dbSettingsNow=Get-Pids ([System.IO.Path]::GetFileNameWithoutExtension($DbSettingsExe))
	Check (@($dbSettingsNow | Where-Object { $dbSettingsBefore -notcontains $_ }).Count -eq 0) 'the connection settings tool has ended'
	$exit=Invoke-Uninstall $dirD (Join-Path $OutDir 'd-uninstall.log')
	Check ($exit -eq 0) "uninstall exit code 0 (was $exit)"
	Check (-not (Test-Path -LiteralPath $dirD)) 'the program folder is gone after the uninstall'
	Test-Uninstalled 'after D'
	#endregion

	$pidsNow=Get-Pids 'OpenPmsDental'
	Check (@($pidsNow | Where-Object { $pidsBefore -notcontains $_ }).Count -eq 0) 'no OpenPmsDental process was started'
}
catch {
	$script:Failures.Add('the test stopped')
	Write-Host "  FAIL  the test stopped: $($_.Exception.Message)"
}
finally {
	#Remove what the script made, and what an uninstall left of the two keys it must delete (a FAIL line above says so); then write back
	#the two keys of an installation that was there before.
	foreach($pair in @(@($savedUninstall,$UninstallKey,'uninstall entry'),@($savedEventLog,$EventLogKey,'Event Log source key'))){
		try {
			if($null -ne $pair[0]){
				Restore-HklmKey $pair[1] $pair[0]
				Write-Host "Wrote back the $($pair[2]) of the installation that was there before: $($pair[1])"
			}
			elseif(Test-Path $pair[1]){
				Remove-Item -Path $pair[1] -Recurse -Force
				Write-Host "Removed the $($pair[2]) a test install left: $($pair[1])"
			}
		}
		catch { $script:Failures.Add("putting back $($pair[1])"); Write-Host "  FAIL  $($pair[1]) could not be put back: $($_.Exception.Message)" }
	}
	#The 64-bit TWAINDSM.dll and its shared-file count as they were before the test.
	try {
		if($null -ne $sharedCountBefore){ Set-ItemProperty -Path $SharedDllsKey -Name $TwainDsmPath -Value $sharedCountBefore -Type DWord }
		elseif($null -ne (Get-RegValue $SharedDllsKey $TwainDsmPath)){ Remove-ItemProperty -Path $SharedDllsKey -Name $TwainDsmPath }
		if(-not $twainDsmBefore -and (Test-Path -LiteralPath $TwainDsmPath)){
			Remove-Item -LiteralPath $TwainDsmPath -Force
			Write-Host "Removed the TWAINDSM.dll the test installs put into $([Environment]::SystemDirectory) (there was none before)."
		}
	}
	catch { $script:Failures.Add("putting back $TwainDsmPath"); Write-Host "  FAIL  $TwainDsmPath or its shared-file count could not be put back: $($_.Exception.Message)" }
	#The 32-bit TWAINDSM.dll and its shared-file count (32-bit view) as they were before the test, exactly as the 64-bit one.
	try {
		if($null -ne $sharedCount32Before){ Set-ItemProperty -Path $SharedDllsKey32 -Name $TwainDsm32CountName -Value $sharedCount32Before -Type DWord }
		elseif($null -ne (Get-RegValue $SharedDllsKey32 $TwainDsm32CountName)){ Remove-ItemProperty -Path $SharedDllsKey32 -Name $TwainDsm32CountName }
		if(-not $twainDsm32Before -and (Test-Path -LiteralPath $TwainDsm32Path)){
			Remove-Item -LiteralPath $TwainDsm32Path -Force
			Write-Host "Removed the 32-bit TWAINDSM.dll the test installs put into $([Environment]::GetFolderPath('SystemX86')) (there was none before)."
		}
	}
	catch { $script:Failures.Add("putting back $TwainDsm32Path"); Write-Host "  FAIL  $TwainDsm32Path or its shared-file count could not be put back: $($_.Exception.Message)" }
	foreach($parent in $script:TestFolders){
		if(Test-Path -LiteralPath $parent){
			try { Remove-Item -LiteralPath $parent -Recurse -Force }
			catch { $script:Failures.Add("removing $parent"); Write-Host "  FAIL  the test folder $parent could not be removed: $($_.Exception.Message)" }
		}
	}
}
#Every shortcut of the program as before the test: none new, none changed, none missing.
$shortcutsEnd=Get-Shortcuts
$changed=@(@($shortcutsBefore.Keys | Where-Object { (-not $shortcutsEnd.ContainsKey($_)) -or ($shortcutsEnd[$_] -ne $shortcutsBefore[$_]) }) +
	@($shortcutsEnd.Keys | Where-Object { -not $shortcutsBefore.ContainsKey($_) }) | Sort-Object -Unique)
Check ($changed.Count -eq 0) "at the end every Start menu and desktop shortcut of the program is as before the test (differs: $($changed -join ', '))"
#An installation of v0.0 in the 32-bit view is a separate installation for the 64-bit setup: its uninstall entry is as before.
$uninstall32End=Save-HklmKey $UninstallKey32
Check ((($null -eq $savedUninstall32) -and ($null -eq $uninstall32End)) -or (($null -ne $savedUninstall32) -and ($null -ne $uninstall32End) -and (($savedUninstall32 | ForEach-Object { "$($_[0])=$($_[1])" }) -join '|') -eq (($uninstall32End | ForEach-Object { "$($_[0])=$($_[1])" }) -join '|'))) 'the 32-bit view''s uninstall entry (an installation of v0.0) is as before the test'
$exitCode=0
if($script:Failures.Count -gt 0){ $exitCode=1 }
Write-Host ("{0} checks passed, {1} failed. Logs: {2}" -f $script:Passes,$script:Failures.Count,$OutDir)
Write-Host "Exit code $exitCode"
exit $exitCode
