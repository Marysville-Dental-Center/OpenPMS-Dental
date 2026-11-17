<#
.SYNOPSIS
Records what the .NET Framework 4.8 build of RdlEngine gives for the reports of RdlCodeTests (package S3b-2b of the .NET 10 move,
docs/port-notes/S3b-2.md, part B).

.DESCRIPTION
On .NET Framework, RdlEngine compiles a report's <Code> block with CodeDom's VBCodeProvider. This script compiles RdlCodeReport.cs (the file the
.NET 10 tests compile too) with Add-Type against a .NET Framework 4.8 build of RdlEngine.dll, runs the fixture report and the <Code> blocks of
RdlCodeReport through it, and writes each text to Fixtures\<name>.net48.txt, byte for byte (UTF-8 without a BOM). The .NET 10 tests compare
the Roslyn compile with these files.

It must run in the 32-bit Windows PowerShell 5.1 (C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe), the .NET Framework 4.8 x86
process the program ran in, with an x86 net48 build of RdlEngine (the source of main, on a branch before RdlEngine was retargeted). No
database, no network, no window.

.EXAMPLE
C:\Windows\SysWOW64\WindowsPowerShell\v1.0\powershell.exe -NoProfile -ExecutionPolicy Bypass -File tests\RdlCodeTests\Record-Net48.ps1 -RdlEngine RdlEngine\bin\Debug\RdlEngine.dll -MySqlConnector $env:USERPROFILE\.nuget\packages\mysqlconnector\2.6.2\lib\net48\MySqlConnector.dll
#>
param(
	[Parameter(Mandatory=$true)][string]$RdlEngine,
	# The net48 MySqlConnector.dll (lib\net48 of the MySqlConnector package in the NuGet folder). The engine loads the code module of the
	# report's MySQL.NET provider from the current folder before it asks the host's CreateConnection hook; the fixture connection is used, the
	# module never is.
	[Parameter(Mandatory=$true)][string]$MySqlConnector,
	[string]$OutFolder=''
)
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){ throw 'Run this in the 32-bit Windows PowerShell 5.1 (SysWOW64).' }
if($PSVersionTable.PSVersion.Major -ne 5 -or $PSVersionTable.CLRVersion.Major -ne 4){ throw 'Run this in Windows PowerShell 5.1 (.NET Framework 4).' }
# The records are in en-US (the culture the .NET 10 tests set): numbers are written in the current culture.
if([System.Threading.Thread]::CurrentThread.CurrentCulture.Name -ne 'en-US'){ throw 'Run this with the culture en-US (the records were made in en-US).' }
$RdlEngine=(Resolve-Path -LiteralPath $RdlEngine).Path
$MySqlConnector=(Resolve-Path -LiteralPath $MySqlConnector).Path
if((Split-Path -Leaf $MySqlConnector) -ne 'MySqlConnector.dll'){ throw '-MySqlConnector names MySqlConnector.dll.' }
[Environment]::CurrentDirectory=Split-Path -Parent $MySqlConnector
if(-not $OutFolder){ $OutFolder=Join-Path $PSScriptRoot 'Fixtures' }
New-Item -ItemType Directory -Force -Path $OutFolder | Out-Null
$engine=[System.Reflection.Assembly]::LoadFrom($RdlEngine)
$framework=$engine.GetCustomAttributes([System.Runtime.Versioning.TargetFrameworkAttribute],$false) | Select-Object -First 1
if(-not $framework -or $framework.FrameworkName -notlike '.NETFramework*'){ throw "$RdlEngine is not a .NET Framework build." }
Add-Type -Path (Join-Path $PSScriptRoot 'RdlCodeReport.cs') -ReferencedAssemblies @($RdlEngine,'System.Data','System.Xml')
$utf8=New-Object System.Text.UTF8Encoding($false)
function Write-Record([string]$name,[string]$text){
	$path=Join-Path $OutFolder ($name+'.net48.txt')
	[System.IO.File]::WriteAllText($path,$text,$utf8)
	Write-Host ("{0}: {1} characters" -f $path,$text.Length)
}
$report=[RdlCodeTests.RdlCodeReport]
Write-Record 'rdl-code-report' ($report::RenderToText($report::REPORT_RDL,$report::FixtureRows()))
Write-Record 'code-one-error' ($report::RenderToText($report::CodeOnlyReport($report::CODE_ONE_ERROR),$null))
Write-Record 'code-three-errors' ($report::RenderToText($report::CodeOnlyReport($report::CODE_THREE_ERRORS),$null))
Write-Record 'code-no-vb-import' ($report::RenderToText($report::CodeOnlyReport($report::CODE_NO_VB_IMPORT),$null))
Write-Record 'code-options' ($report::RenderToText($report::CodeOnlyReport($report::CODE_OPTIONS),$null))
