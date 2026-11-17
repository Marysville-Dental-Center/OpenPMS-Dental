<#
.SYNOPSIS
Installs the setup of a Release dry run in Windows Sandbox, a clean Windows without any .NET, checks the install and uninstalls it there,
then prints one summary line. Not elevated (once Windows Sandbox is on), 64-bit. A launch check of stage 3b (docs\launch-checks-3b.md).

.DESCRIPTION
HOW TO RUN IT. The owner runs it, never an agent: it starts Windows Sandbox. After a Release dry run (tools\Build-Release.ps1 -OutDir
<dryrun>, which writes <dryrun>\staging and <dryrun>\OpenPmsDental-<version>-Setup.exe), open Windows PowerShell (no "Run as
administrator" needed) and paste this one line, with <repo> the repository folder and <dryrun> the dry run's output folder:

  powershell -NoProfile -ExecutionPolicy Bypass -File "<repo>\tools\Test-CleanInstall.ps1" -Setup "<dryrun>\OpenPmsDental-<version>-Setup.exe" -Staging "<dryrun>\staging"

WINDOWS SANDBOX MUST BE ON. The script checks it (WindowsSandbox.exe in System32). When it is off, the script changes nothing, prints the
one command that turns it on, to run once in an elevated PowerShell:

  Enable-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -All

and stops (exit code 3); Windows asks for a restart after that command, and after the restart this script runs without elevation. Windows
Sandbox needs Windows 11 Pro (or Enterprise or Education) and virtualization turned on in the firmware.

-PlanOnly prints every step and the .wsb file it would write, and changes nothing. -OutDir names the results folder (default: a new
%TEMP%\OpenPmsDental-cleaninstall-<id>\results); it must be new or empty. -TimeoutMinutes (default 30) is how long the script waits for the
sandbox's results. -KeepSandboxOpen leaves the sandbox open at the end, for a look (close its window yourself; that discards it).

It refuses, changing nothing (exit code 3): in a 32-bit PowerShell, when Windows Sandbox is off, when a Windows Sandbox is already open
(Windows runs one at a time), when the setup or the staging folder is missing, or when the results folder is not empty.

WHAT IT DOES:
1. Checks that the staging folder is exactly Build-Release's list (tools\Build-Release.ps1 -ListFiles), as Test-Installer.ps1 does.
2. Writes a new folder %TEMP%\OpenPmsDental-cleaninstall-<id> with in\ (a copy of tools\Test-CleanInstall-InSandbox.ps1, and host.txt
   with this PC's name, so that copy never runs here), results\ (or -OutDir) and cleaninstall.wsb: the setup's folder, the staging
   folder and in\ mapped READ-ONLY into the sandbox, the results folder mapped writable, networking on (setup downloads the x64 Visual
   C++ runtime, which the sandbox lacks), no GPU, printer, audio or camera, and a LogonCommand that runs the in-sandbox script.
3. Starts WindowsSandbox.exe with the .wsb. Inside, the script records that no .NET runtime is there, runs the setup silently
   (/VERYSILENT /SUPPRESSMSGBOXES, a database server on the sandbox's own computer so no connection settings tool runs, /PREREQS=download),
   checks the install folder's files against the staging folder, starts the installed OpenPmsDentalDbSettings.exe (/check, then its window,
   whose coreclr.dll and hostfxr.dll must load from the install folder), checks the 64-bit TWAINDSM.dll, uninstalls silently, writes
   results.txt and shuts the sandbox down (see the header of Test-CleanInstall-InSandbox.ps1).
4. Waits (up to -TimeoutMinutes) for the results file's DONE line, reads it, shows every line and prints "N checks passed, M failed" with
   the results folder, and the exit code: 0 every check passed, 1 a check failed.

WHAT IT CHANGES ON THIS PC: nothing outside the new %TEMP%\OpenPmsDental-cleaninstall-<id> folder. Everything the setup does happens in the
sandbox, which Windows discards when it shuts down or its window is closed. At the end the script deletes in\ and cleaninstall.wsb and
keeps the results folder (results.txt, setup.log, uninstall.log, installed-files.txt). It does not install, enable or start anything on
this PC but the Windows Sandbox window.
#>
[CmdletBinding(PositionalBinding=$false)]
param(
	[string]$Setup='',
	[string]$Staging='',
	[string]$OutDir='',
	[int]$TimeoutMinutes=30,
	[switch]$KeepSandboxOpen,
	[switch]$PlanOnly
)
Set-StrictMode -Version 2
$ErrorActionPreference='Stop'

$RepoRoot=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$InSandboxScriptName='Test-CleanInstall-InSandbox.ps1'
$EnableCommand='Enable-WindowsOptionalFeature -Online -FeatureName Containers-DisposableClientVM -All'
#The folders inside the sandbox.
$SandboxSetupFolder='C:\launch\setup'
$SandboxStagingFolder='C:\launch\staging'
$SandboxInFolder='C:\launch\in'
$SandboxResultsFolder='C:\launch\results'
#The processes of an open Windows Sandbox (the launcher, its client and the remote session of newer Windows 11 builds).
$SandboxProcessNames=@('WindowsSandbox','WindowsSandboxClient','WindowsSandboxRemoteSession','WindowsSandboxServer')

#region Plain code (the tests in tests\ReleaseFlowTests dot-source the script and call these; none of them changes anything)
#The command the sandbox runs after its logon: the in-sandbox script from the read-only folder, with the mapped folders.
function Get-LogonCommand([string]$setupName,[bool]$keepOpen){
	$command="powershell.exe -NoProfile -ExecutionPolicy Bypass -File $SandboxInFolder\$InSandboxScriptName -SetupFile `"$SandboxSetupFolder\$setupName`" -StagingFolder $SandboxStagingFolder -ResultsFolder $SandboxResultsFolder"
	if($keepOpen){ $command+=' -NoShutdown' }
	return $command
}

#The .wsb file (Windows Sandbox configuration): the setup's folder, the staging folder and the script folder read-only, the results folder
#writable, networking on for the Visual C++ download, the rest of the host's devices off.
function New-SandboxConfig([string]$setupFolder,[string]$stagingFolder,[string]$inFolder,[string]$resultsFolder,[string]$setupName,[bool]$keepOpen){
	$settings=New-Object System.Xml.XmlWriterSettings
	$settings.Indent=$true
	$settings.IndentChars='  '
	$settings.OmitXmlDeclaration=$true
	$settings.NewLineChars="`r`n"
	$builder=New-Object System.Text.StringBuilder
	$writer=[System.Xml.XmlWriter]::Create($builder,$settings)
	try {
		$writer.WriteStartElement('Configuration')
		$writer.WriteElementString('VGpu','Disable')
		$writer.WriteElementString('Networking','Enable')
		$writer.WriteStartElement('MappedFolders')
		foreach($mapping in @(@($setupFolder,$SandboxSetupFolder,'true'),@($stagingFolder,$SandboxStagingFolder,'true'),@($inFolder,$SandboxInFolder,'true'),
			@($resultsFolder,$SandboxResultsFolder,'false')))
		{
			$writer.WriteStartElement('MappedFolder')
			$writer.WriteElementString('HostFolder',$mapping[0])
			$writer.WriteElementString('SandboxFolder',$mapping[1])
			$writer.WriteElementString('ReadOnly',$mapping[2])
			$writer.WriteEndElement()
		}
		$writer.WriteEndElement()
		$writer.WriteStartElement('LogonCommand')
		$writer.WriteElementString('Command',(Get-LogonCommand $setupName $keepOpen))
		$writer.WriteEndElement()
		$writer.WriteElementString('AudioInput','Disable')
		$writer.WriteElementString('VideoInput','Disable')
		$writer.WriteElementString('PrinterRedirection','Disable')
		$writer.WriteElementString('MemoryInMB','4096')
		$writer.WriteEndElement()
	}
	finally { $writer.Close() }
	return $builder.ToString()+"`r`n"
}

#The results file the in-sandbox script writes: its ok and FAIL lines, its notes, and whether its DONE line is there. A missing file
#reads as nothing done.
function Read-SandboxResults([string]$path){
	$result=New-Object PSObject -Property @{ Passes=New-Object 'System.Collections.Generic.List[string]'; Failures=New-Object 'System.Collections.Generic.List[string]'
		Notes=New-Object 'System.Collections.Generic.List[string]'; Done=$false; Lines=@() }
	if(-not (Test-Path -LiteralPath $path)){ return $result }
	$lines=@([System.IO.File]::ReadAllLines($path))
	$result.Lines=$lines
	foreach($line in $lines){
		if($line.StartsWith('ok    ')){ $result.Passes.Add($line.Substring(6)) }
		elseif($line.StartsWith('FAIL  ')){ $result.Failures.Add($line.Substring(6)) }
		elseif($line.StartsWith('note  ')){ $result.Notes.Add($line.Substring(6)) }
		elseif($line.StartsWith('DONE ')){ $result.Done=$true }
	}
	return $result
}

#Whether the results file has its DONE line yet.
function Test-ResultsDone([string]$path){
	if(-not (Test-Path -LiteralPath $path)){ return $false }
	try { return @([System.IO.File]::ReadAllLines($path) | Where-Object { $_.StartsWith('DONE ') }).Count -gt 0 }
	catch { return $false }
}

#The steps, as -PlanOnly prints them.
function Get-PlanSteps([string]$setupPath,[string]$stagingPath,[string]$workFolder,[string]$resultsFolder){
	return @(
		"check that $stagingPath is exactly Build-Release's list (tools\Build-Release.ps1 -ListFiles)",
		"make $workFolder with in\$InSandboxScriptName (a copy), the results folder $resultsFolder and cleaninstall.wsb",
		"map $(Split-Path -Parent $setupPath) to $SandboxSetupFolder, $stagingPath to $SandboxStagingFolder and in\ to $SandboxInFolder read-only, $resultsFolder to $SandboxResultsFolder writable; networking on (the Visual C++ download)",
		"start WindowsSandbox.exe $workFolder\cleaninstall.wsb; inside: no .NET recorded, the setup run silently (/VERYSILENT /SUPPRESSMSGBOXES /PREREQS=download /DBSERVER=localhost), the install folder checked against the staging folder, OpenPmsDentalDbSettings.exe /check and its window (coreclr.dll and hostfxr.dll from the install folder), the silent uninstall, results.txt, shutdown",
		"wait up to $TimeoutMinutes minutes for the DONE line of $resultsFolder\results.txt and read it",
		"delete in\ and cleaninstall.wsb, keep $resultsFolder, print 'N checks passed, M failed. Logs: $resultsFolder'"
	)
}
#endregion Plain code

#Dot-sourced by the tests: the functions above only.
if($MyInvocation.InvocationName -eq '.'){ return }

function Stop-Unchanged([string]$why){
	Write-Host $why
	Write-Host 'Nothing was changed.'
	Write-Host 'Exit code 3'
	exit 3
}

#A 32-bit PowerShell sees SysWOW64 for System32, where WindowsSandbox.exe never is: refuse it, changing nothing.
if(-not [Environment]::Is64BitProcess){
	Stop-Unchanged 'Test-CleanInstall.ps1 needs the 64-bit Windows PowerShell (this one is 32-bit). Open Windows PowerShell from the Start menu (not "Windows PowerShell (x86)") and run the line in the header of this script.'
}
$SandboxExe=Join-Path ([Environment]::SystemDirectory) 'WindowsSandbox.exe'
$isSandboxOn=Test-Path -LiteralPath $SandboxExe
if(-not $isSandboxOn -and -not $PlanOnly){
	Write-Host 'Windows Sandbox is not turned on on this PC. Turn it on once: open Windows PowerShell with "Run as administrator" and run'
	Write-Host ''
	Write-Host "  $EnableCommand"
	Write-Host ''
	Write-Host 'Windows then asks for a restart. After it, run this script again (no elevation needed).'
	Stop-Unchanged 'Test-CleanInstall.ps1 stops here.'
}

#region Before the first change
$id=[guid]::NewGuid().ToString('N').Substring(0,8)
if(-not $Staging){ $Staging=Join-Path $RepoRoot 'installer\bin\staging' }
if(-not $Setup){
	$found=Get-ChildItem -LiteralPath (Join-Path $RepoRoot 'installer\bin') -Filter 'OpenPmsDental-*-Setup.exe' -ErrorAction SilentlyContinue | Select-Object -First 1
	if($found){ $Setup=$found.FullName }
}
$WorkFolder=Join-Path $env:TEMP "OpenPmsDental-cleaninstall-$id"
if(-not $OutDir){ $OutDir=Join-Path $WorkFolder 'results' }
$refusals=New-Object 'System.Collections.Generic.List[string]'
if(-not $Setup -or -not (Test-Path -LiteralPath $Setup -PathType Leaf)){ $refusals.Add("No setup found ($Setup); pass -Setup with the setup of a Release dry run (tools\Build-Release.ps1).") }
if(-not (Test-Path -LiteralPath $Staging -PathType Container)){ $refusals.Add("No staging folder at $Staging; pass -Staging with the dry run's staging folder.") }
if((Test-Path -LiteralPath $OutDir) -and @(Get-ChildItem -LiteralPath $OutDir -Force).Count -gt 0){ $refusals.Add("The results folder $OutDir is not empty.") }
$open=@(Get-Process -Name $SandboxProcessNames -ErrorAction SilentlyContinue)
if($open.Count -gt 0){ $refusals.Add("A Windows Sandbox is open already ($(@($open | ForEach-Object { $_.ProcessName }) -join ', ')); Windows runs one at a time. Close it first.") }

if($PlanOnly){
	Write-Host 'Test-CleanInstall.ps1 -PlanOnly: the steps of a real run, none of them done.'
	Write-Host "Windows Sandbox: $(if($isSandboxOn){ "on ($SandboxExe)" }else{ "off; a real run prints the command that turns it on: $EnableCommand" })"
	Write-Host "Setup: $Setup"
	Write-Host "Staging: $Staging"
	$number=1
	foreach($step in (Get-PlanSteps $Setup $Staging $WorkFolder $OutDir)){
		Write-Host ("PLAN {0}. {1}" -f $number,$step)
		$number++
	}
	$setupName=if($Setup){ Split-Path -Leaf $Setup }else{ 'OpenPmsDental-<version>-Setup.exe' }
	$setupFolder=if($Setup){ Split-Path -Parent $Setup }else{ '<dryrun>' }
	Write-Host 'The .wsb file it would write:'
	Write-Host (New-SandboxConfig $setupFolder $Staging (Join-Path $WorkFolder 'in') $OutDir $setupName $KeepSandboxOpen.IsPresent)
	foreach($refusal in $refusals){ Write-Host "A real run would refuse: $refusal" }
	Write-Host 'Nothing was changed.'
	Write-Host 'Exit code 0'
	exit 0
}
if($refusals.Count -gt 0){ Stop-Unchanged ($refusals -join "`r`n") }
$Setup=(Resolve-Path -LiteralPath $Setup).Path
$Staging=(Resolve-Path -LiteralPath $Staging).Path
#endregion

$script:Failures=New-Object 'System.Collections.Generic.List[string]'
$script:Passes=0
function Check([bool]$condition,[string]$what){
	if($condition){ $script:Passes++; Write-Host "  ok    $what" }
	else{ $script:Failures.Add($what); Write-Host "  FAIL  $what" }
}

$inFolder=Join-Path $WorkFolder 'in'
$wsbPath=Join-Path $WorkFolder 'cleaninstall.wsb'
$resultsPath=Join-Path $OutDir 'results.txt'
$sandboxStarted=$false
try {
	Write-Host "Setup: $Setup"
	Write-Host ("Setup SHA-256: {0}" -f (Get-FileHash -LiteralPath $Setup -Algorithm SHA256).Hash.ToLowerInvariant())
	Write-Host "Staging: $Staging"
	#region 1. The staging folder is Build-Release's list
	Write-Host '1. The staging folder'
	$lines=@(& powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $RepoRoot 'tools\Build-Release.ps1') -ListFiles 2>$null)
	if($LASTEXITCODE -eq 0){
		$releaseList=@{}
		foreach($line in $lines){
			$parts="$line".Split('|')
			if($parts.Count -eq 2 -and $parts[0] -ne 'setup'){ $releaseList[$parts[1]]=$parts[0] }
		}
		$stagingFiles=@{}
		foreach($file in Get-ChildItem -LiteralPath $Staging -Recurse -File -Force){ $stagingFiles[$file.FullName.Substring($Staging.TrimEnd('\').Length+1)]=$true }
		$missing=@($releaseList.Keys | Where-Object { -not $stagingFiles.ContainsKey($_) } | Sort-Object)
		$extra=@($stagingFiles.Keys | Where-Object { -not $releaseList.ContainsKey($_) } | Sort-Object)
		Check ($missing.Count -eq 0 -and $extra.Count -eq 0) "the staging folder is Build-Release's list: $($releaseList.Count) files (not staged: $($missing -join ', '); not listed: $($extra -join ', '))"
	}
	else { Check $false "tools\Build-Release.ps1 -ListFiles ran (exit code $LASTEXITCODE)" }
	#endregion

	#region 2. The folders and the .wsb file
	Write-Host "2. $WorkFolder"
	New-Item -ItemType Directory -Force -Path $inFolder | Out-Null
	New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
	Copy-Item -LiteralPath (Join-Path $PSScriptRoot $InSandboxScriptName) -Destination (Join-Path $inFolder $InSandboxScriptName)
	#The host's name beside it: the in-sandbox script runs only on another computer (or as the sandbox's user).
	[System.IO.File]::WriteAllText((Join-Path $inFolder 'host.txt'),$env:COMPUTERNAME,[System.Text.Encoding]::ASCII)
	[System.IO.File]::WriteAllText($wsbPath,(New-SandboxConfig (Split-Path -Parent $Setup) $Staging $inFolder $OutDir (Split-Path -Leaf $Setup) $KeepSandboxOpen.IsPresent),
		(New-Object System.Text.UTF8Encoding($false)))
	Check (Test-Path -LiteralPath $wsbPath) "the sandbox configuration is written: $wsbPath"
	#endregion

	#region 3-4. The sandbox
	Write-Host '3. Windows Sandbox (the install runs inside; this takes several minutes)'
	Start-Process -FilePath $SandboxExe -ArgumentList "`"$wsbPath`"" | Out-Null
	$sandboxStarted=$true
	$deadline=(Get-Date).AddMinutes($TimeoutMinutes)
	while(-not (Test-ResultsDone $resultsPath) -and (Get-Date) -lt $deadline){ Start-Sleep -Seconds 5 }
	$results=Read-SandboxResults $resultsPath
	Write-Host '4. The results from the sandbox'
	foreach($line in $results.Lines){ Write-Host "  $line" }
	foreach($pass in $results.Passes){ $script:Passes++ }
	foreach($failure in $results.Failures){ $script:Failures.Add($failure) }
	Check ($results.Lines.Count -gt 0) "the sandbox wrote its results file ($resultsPath)"
	Check $results.Done "the in-sandbox script finished within $TimeoutMinutes minutes (its DONE line)"
	#endregion
}
catch {
	$script:Failures.Add('the test stopped')
	Write-Host "  FAIL  the test stopped: $($_.Exception.Message)"
}
finally {
	if($sandboxStarted -and -not $KeepSandboxOpen){
		#The sandbox shuts itself down after its DONE line; wait for it (up to three minutes) before deleting its read-only folder.
		$deadline=(Get-Date).AddMinutes(3)
		while(@(Get-Process -Name $SandboxProcessNames -ErrorAction SilentlyContinue).Count -gt 0 -and (Get-Date) -lt $deadline){ Start-Sleep -Seconds 5 }
		if(@(Get-Process -Name $SandboxProcessNames -ErrorAction SilentlyContinue).Count -gt 0){
			Write-Host '  note  Windows Sandbox is still open: close its window (that discards the sandbox and everything in it).'
		}
	}
	foreach($path in @($inFolder,$wsbPath)){
		if(Test-Path -LiteralPath $path){
			try { Remove-Item -LiteralPath $path -Recurse -Force }
			catch { Write-Host "  note  $path could not be deleted yet ($($_.Exception.Message)); delete it once the sandbox is closed." }
		}
	}
}
$exitCode=0
if($script:Failures.Count -gt 0){ $exitCode=1 }
Write-Host ("{0} checks passed, {1} failed. Logs: {2}" -f $script:Passes,$script:Failures.Count,$OutDir)
Write-Host "Exit code $exitCode"
exit $exitCode
