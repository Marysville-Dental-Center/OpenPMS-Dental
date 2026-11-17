<#
.SYNOPSIS
The part of tools\Test-CleanInstall.ps1 that runs INSIDE Windows Sandbox: a clean install of the setup on a Windows without any .NET, its
checks, and the uninstall. It refuses to run anywhere else.

.DESCRIPTION
Do not run it yourself. tools\Test-CleanInstall.ps1 copies it into the folder it maps read-only into the sandbox and names it in the .wsb
file's LogonCommand, so the sandbox runs it once its user (WDAGUtilityAccount, an administrator of the sandbox) has logged on. On any other
computer or user it refuses before its first step and changes nothing (exit code 3): it installs and uninstalls the setup for real.

In the sandbox, in order (each step one "ok" or "FAIL" line in <results>\results.txt, read by Test-CleanInstall.ps1 on the host):
1. No .NET runtime is there: no dotnet.exe and no shared Microsoft.NETCore.App or Microsoft.WindowsDesktop.App folder in either Program
   Files, no dotnet on the PATH. (Windows Sandbox is a fresh copy of the host's Windows without its installed programs.)
2. The setup runs silently: /VERYSILENT /SUPPRESSMSGBOXES /NORESTART, a database server on the sandbox's own computer (/DBSERVER=localhost,
   so a silent setup runs no connection settings tool) and /PREREQS=download, so setup downloads and installs the x64 Visual C++ runtime,
   which the sandbox lacks: this needs the sandbox's network (the .wsb turns it on). Setup exit code 0; the Visual C++ x64 runtime is
   installed afterwards; still no .NET runtime anywhere.
3. The install folder (the uninstall entry's App Path, C:\Program Files\Open PMS - Dental) holds exactly the staging folder (same files,
   same SHA-256), FreeDentalConfig.xml and the uninstall folder.
4. The installed OpenPmsDentalDbSettings.exe runs on the install folder's own runtime: /check (no window, no connection) ends with exit
   code 0, and started without arguments (its window, which connects to nothing) it loads coreclr.dll and hostfxr.dll from the install
   folder and nothing from Program Files\dotnet; then it is closed.
5. The 64-bit TWAINDSM.dll is in the sandbox's System32 (x64).
6. The silent uninstall: exit code 0, the install folder and the uninstall entry are gone.
Then a line "DONE passed=N failed=M", and the sandbox shuts down (not with -NoShutdown), which discards everything in it.
#>
[CmdletBinding(PositionalBinding=$false)]
param(
	[string]$SetupFile='',
	[string]$StagingFolder='',
	[string]$ResultsFolder='',
	[switch]$NoShutdown
)
Set-StrictMode -Version 2
$ErrorActionPreference='Stop'

#The account Windows Sandbox logs on with; this script runs for no other.
$SandboxUser='WDAGUtilityAccount'
$AppId='{D5640A25-F302-4CAD-96DC-84AAA8B22F98}_is1'
$UninstallKey="HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$AppId"
$DbSettingsExe='OpenPmsDentalDbSettings.exe'
$ResultsFileName='results.txt'
#The folder the sandbox runs this script from, and the host's marker in it (Test-CleanInstall.ps1 writes the host's computer name there).
$SandboxScriptFolder='C:\launch\in'
$HostMarkerName='host.txt'

#region Plain code (the tests in tests\ReleaseFlowTests dot-source the script and call these)
#Appends one line to the results file (ASCII; a character outside ASCII becomes '?') and shows it.
function Write-ResultLine([string]$path,[string]$line){
	Write-Host $line
	$ascii=-join ($line.ToCharArray() | ForEach-Object { if([int]$_ -lt 128){ $_ }else{ '?' } })
	[System.IO.File]::AppendAllText($path,$ascii+"`r`n",[System.Text.Encoding]::ASCII)
}

#The arguments of the silent setup: no window or message, no restart, a database server on this computer, the Visual C++ runtime
#downloaded, the setup log in the results folder.
function Get-SetupArguments([string]$logPath){
	return @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/PREREQS=download','/DBSERVER=localhost','/DBNAME=opms_sandbox','/DBUSER=opms_user',"/LOG=`"$logPath`"")
}

#The files of a folder by relative path, with their SHA-256.
function Get-FileMap([string]$folder){
	$map=@{}
	foreach($file in Get-ChildItem -LiteralPath $folder -Recurse -File -Force){
		$map[$file.FullName.Substring($folder.TrimEnd('\').Length+1)]=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
	}
	return $map
}

#The install folder against the staging folder: missing, changed and unexpected files (FreeDentalConfig.xml and the uninstall folder are
#setup's own).
function Compare-InstallFolder($installedMap,$stagingMap){
	$extraExpected=@('FreeDentalConfig.xml','uninstall\unins000.dat','uninstall\unins000.exe')
	$missing=@($stagingMap.Keys | Where-Object { -not $installedMap.ContainsKey($_) } | Sort-Object)
	$changed=@($stagingMap.Keys | Where-Object { $installedMap.ContainsKey($_) -and $installedMap[$_] -ne $stagingMap[$_] } | Sort-Object)
	$extra=@($installedMap.Keys | Where-Object { -not $stagingMap.ContainsKey($_) -and $extraExpected -notcontains $_ } | Sort-Object)
	$setupOwn=@($extraExpected | Where-Object { $installedMap.ContainsKey($_) })
	return New-Object PSObject -Property @{ Missing=$missing; Changed=$changed; Extra=$extra; SetupOwn=$setupOwn }
}

#Where a .NET runtime would be: dotnet.exe and the shared framework folders of both Program Files, which exist.
function Get-DotNetTraces {
	$traces=New-Object 'System.Collections.Generic.List[string]'
	foreach($root in @($env:ProgramFiles,${env:ProgramFiles(x86)})){
		if(-not $root){ continue }
		foreach($relative in @('dotnet\dotnet.exe','dotnet\shared\Microsoft.NETCore.App','dotnet\shared\Microsoft.WindowsDesktop.App','dotnet\shared\Microsoft.AspNetCore.App')){
			$path=Join-Path $root $relative
			if(Test-Path -LiteralPath $path){ $traces.Add($path) }
		}
	}
	$command=Get-Command 'dotnet.exe' -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
	if($command){ $traces.Add("dotnet.exe on the PATH: $($command.Source)") }
	return ,$traces
}
#endregion Plain code

#Dot-sourced by the tests: the functions above only.
if($MyInvocation.InvocationName -eq '.'){ return }

#Before anything else: outside Windows Sandbox this script does nothing. The sandbox is known by its user, or (should a Windows build name it
#otherwise) by the host's marker beside this copy of the script in the sandbox's read-only folder, naming a computer that is not this one.
$markerPath=Join-Path $PSScriptRoot $HostMarkerName
$hostName=''
if(Test-Path -LiteralPath $markerPath){ $hostName=([System.IO.File]::ReadAllText($markerPath)).Trim() }
$isSandbox=($env:USERNAME -eq $SandboxUser) -or (($PSScriptRoot -eq $SandboxScriptFolder) -and $hostName -ne '' -and $hostName -ne $env:COMPUTERNAME)
if(-not $isSandbox){
	Write-Host "Test-CleanInstall-InSandbox.ps1 runs only inside Windows Sandbox (user $SandboxUser), where tools\Test-CleanInstall.ps1 starts it."
	Write-Host 'It installs and uninstalls the setup for real, so it refuses on this computer. Nothing was changed.'
	Write-Host 'Exit code 3'
	exit 3
}

$ResultsPath=Join-Path $ResultsFolder $ResultsFileName
$script:Passes=0
$script:FailCount=0

function Check([bool]$condition,[string]$what){
	if($condition){ $script:Passes++; Write-ResultLine $ResultsPath "ok    $what" }
	else{ $script:FailCount++; Write-ResultLine $ResultsPath "FAIL  $what" }
}

function Note([string]$what){ Write-ResultLine $ResultsPath "note  $what" }

function Get-RegValue([string]$path,[string]$name){
	if(-not (Test-Path $path)){ return $null }
	return (Get-Item $path).GetValue($name,$null)
}

#The x64 Visual C++ runtime as setup checks it (the VC\Runtimes\x64 key, 64-bit or 32-bit view).
function Test-VcRuntime {
	foreach($key in @('HKLM:\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64','HKLM:\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64')){
		if((Get-RegValue $key 'Installed') -eq 1){ return $true }
	}
	return $false
}

try {
	Write-ResultLine $ResultsPath ("RUN started={0} computer={1} user={2} windows={3}" -f (Get-Date -Format s),$env:COMPUTERNAME,$env:USERNAME,[Environment]::OSVersion.Version)
	$principal=New-Object System.Security.Principal.WindowsPrincipal([System.Security.Principal.WindowsIdentity]::GetCurrent())
	Check ($principal.IsInRole([System.Security.Principal.WindowsBuiltInRole]::Administrator)) "the sandbox's logon command runs as an administrator ($env:USERNAME)"
	Check ([Environment]::Is64BitProcess) 'the sandbox script runs in the 64-bit PowerShell'
	Check (Test-Path -LiteralPath $SetupFile) "the setup is mapped into the sandbox ($SetupFile)"
	Check (Test-Path -LiteralPath $StagingFolder) "the staging folder is mapped into the sandbox ($StagingFolder)"

	#region 1. No .NET
	$traces=Get-DotNetTraces
	Check ($traces.Count -eq 0) "no .NET runtime in the sandbox before the install: no dotnet.exe, no shared Microsoft.NETCore.App (found: $($traces -join ', '))"
	$vcBefore=Test-VcRuntime
	Note "the x64 Visual C++ runtime before the install: $(if($vcBefore){ 'installed' }else{ 'missing (setup downloads it)' })"
	#endregion

	#region 2. The silent setup
	$setupLog=Join-Path $ResultsFolder 'setup.log'
	$process=Start-Process -FilePath $SetupFile -ArgumentList (Get-SetupArguments $setupLog) -Wait -PassThru
	Check ($process.ExitCode -eq 0) "the silent setup ends with exit code 0 (was $($process.ExitCode); log setup.log)"
	Check (Test-VcRuntime) "the x64 Visual C++ runtime is installed after the setup (/PREREQS=download, through the sandbox's network)"
	$traces=Get-DotNetTraces
	Check ($traces.Count -eq 0) "still no .NET runtime outside the install folder after the install (found: $($traces -join ', '))"
	#endregion

	#region 3. The install folder
	$dir=[string](Get-RegValue $UninstallKey 'Inno Setup: App Path')
	$dirExpected=Join-Path $env:ProgramFiles 'Open PMS - Dental'
	Check ($dir.TrimEnd('\') -eq $dirExpected) "the uninstall entry (64-bit view) names $dirExpected (was '$dir')"
	if(-not $dir){ $dir=$dirExpected }
	$installedMap=@{}
	if(Test-Path -LiteralPath $dir){ $installedMap=Get-FileMap $dir }
	$stagingMap=Get-FileMap $StagingFolder
	($installedMap.Keys | Sort-Object) | Out-File -FilePath (Join-Path $ResultsFolder 'installed-files.txt') -Encoding ascii
	$comparison=Compare-InstallFolder $installedMap $stagingMap
	Check ($stagingMap.Count -gt 0 -and $comparison.Missing.Count -eq 0) "every staged file is installed ($($stagingMap.Count) staged; missing: $($comparison.Missing -join ', '))"
	Check ($comparison.Changed.Count -eq 0) "every installed file equals the staged one (different: $($comparison.Changed -join ', '))"
	Check ($comparison.Extra.Count -eq 0) "nothing else in the install folder besides FreeDentalConfig.xml and the uninstall folder (also: $($comparison.Extra -join ', '))"
	Check ($comparison.SetupOwn.Count -eq 3) "setup wrote FreeDentalConfig.xml and the uninstall folder ($($comparison.SetupOwn -join ', '))"
	#endregion

	#region 4. The program's own runtime
	$tool=Join-Path $dir $DbSettingsExe
	if(Test-Path -LiteralPath $tool){
		$checkProcess=Start-Process -FilePath $tool -ArgumentList '/check' -WorkingDirectory $dir -Wait -PassThru
		Check ($checkProcess.ExitCode -eq 0) "$DbSettingsExe /check runs on a Windows without .NET and passes the settings setup wrote (exit code $($checkProcess.ExitCode))"
		$windowProcess=Start-Process -FilePath $tool -WorkingDirectory $dir -PassThru
		$modules=@()
		try {
			$deadline=(Get-Date).AddSeconds(90)
			while((Get-Date) -lt $deadline -and -not $windowProcess.HasExited){
				try { $windowProcess.Refresh(); $modules=@($windowProcess.Modules | ForEach-Object { $_.FileName }) } catch { $modules=@() }
				if(@($modules | Where-Object { (Split-Path $_ -Leaf) -eq 'coreclr.dll' }).Count -gt 0){ break }
				Start-Sleep -Milliseconds 500
			}
		}
		finally {
			if(-not $windowProcess.HasExited){
				[void]$windowProcess.CloseMainWindow()
				if(-not $windowProcess.WaitForExit(10000)){ $windowProcess.Kill(); [void]$windowProcess.WaitForExit(10000) }
			}
		}
		$folder=$dir.TrimEnd('\')+'\'
		$coreclr=@($modules | Where-Object { (Split-Path $_ -Leaf) -eq 'coreclr.dll' })
		$hostfxr=@($modules | Where-Object { (Split-Path $_ -Leaf) -eq 'hostfxr.dll' })
		Check ($coreclr.Count -eq 1 -and $coreclr[0].StartsWith($folder,[StringComparison]::OrdinalIgnoreCase)) "the installed $DbSettingsExe loads coreclr.dll from the install folder (loaded: $($coreclr -join ', '))"
		Check ($hostfxr.Count -eq 1 -and $hostfxr[0].StartsWith($folder,[StringComparison]::OrdinalIgnoreCase)) "and hostfxr.dll from the install folder (loaded: $($hostfxr -join ', '))"
		$dotnet=(Join-Path $env:ProgramFiles 'dotnet').TrimEnd('\')+'\'
		$machineWide=@($modules | Where-Object { $_.StartsWith($dotnet,[StringComparison]::OrdinalIgnoreCase) })
		Check ($machineWide.Count -eq 0) "nothing loaded from $dotnet (loaded: $($machineWide -join ', '))"
		Check $windowProcess.HasExited 'the connection settings tool was closed'
	}
	else { Check $false "$DbSettingsExe is installed in $dir" }
	#endregion

	#region 5. TWAINDSM.dll
	$twainDsm=Join-Path ([Environment]::SystemDirectory) 'TWAINDSM.dll'
	$machine=0
	if(Test-Path -LiteralPath $twainDsm){
		$bytes=[System.IO.File]::ReadAllBytes($twainDsm)
		$machine=[BitConverter]::ToUInt16($bytes,[BitConverter]::ToInt32($bytes,0x3C)+4)
	}
	Check ($machine -eq 0x8664) ("the 64-bit TWAINDSM.dll is in the sandbox's System32 (machine 0x{0:X})" -f $machine)
	#endregion

	#region 6. The silent uninstall
	$uninstaller=Join-Path $dir 'uninstall\unins000.exe'
	if(Test-Path -LiteralPath $uninstaller){
		$uninstallProcess=Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',"/LOG=`"$(Join-Path $ResultsFolder 'uninstall.log')`"") -Wait -PassThru
		Check ($uninstallProcess.ExitCode -eq 0) "the silent uninstall ends with exit code 0 (was $($uninstallProcess.ExitCode); log uninstall.log)"
		#The uninstaller runs its second phase from a copy in %TEMP%; wait (up to two minutes) until the folder is gone.
		$deadline=(Get-Date).AddSeconds(120)
		while((Test-Path -LiteralPath $dir) -and (Get-Date) -lt $deadline){ Start-Sleep -Seconds 1 }
		Check (-not (Test-Path -LiteralPath $dir)) "the install folder is gone after the uninstall ($dir)"
		Check (-not (Test-Path $UninstallKey)) 'the uninstall entry is gone'
	}
	else { Check $false "the uninstaller is installed ($uninstaller)" }
	#endregion
}
catch {
	$script:FailCount++
	Write-ResultLine $ResultsPath "FAIL  the sandbox script stopped: $($_.Exception.Message)"
}
finally {
	Write-ResultLine $ResultsPath ("DONE passed={0} failed={1} finished={2}" -f $script:Passes,$script:FailCount,(Get-Date -Format s))
	#Shutting the sandbox down discards everything in it; the results folder is the host's.
	if(-not $NoShutdown){ & shutdown.exe /s /t 5 /c 'Open PMS - Dental clean-install test finished' }
}
