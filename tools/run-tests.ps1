<#
.SYNOPSIS
Runs the fork's MSTest assemblies (tests/*, NoHqTests, the legacy UnitTests and tools/*/*Tests) with vstest, a few at a time, and prints one summary.

.DESCRIPTION
The test assemblies are the *Tests / *.Tests projects listed in OpenDental.Local.slnf, the legacy UnitTests project among them
(it builds 'Unit Tests.exe'; -Include and -List name it UnitTests, and it makes and drops its own database). Build them first
(Debug|x86, see README.md).

Targeted mode runs NoHqTests plus the assemblies named in -Include. Core mode runs every assembly except the slow migration and
adoption ones (add them back with -Include). Full mode runs everything.
Results go to -Out (default: %TEMP%\odtests\<repository folder name>.txt). The last line is "DONE ok=True|False".
If the call is cut off by a tool timeout, the run keeps going; call again with -WaitOnly and the same -Root/-Out
to wait (up to 9 minutes per call) and print the summary.

-Root defaults to the repository that holds this script (the parent of tools\), so it can be run from any folder.

vstest.console.exe is the one of Visual Studio 2026 or its Build Tools (vstest 18), found with vswhere and a version range
(tools\Initialize-BuildEnvironment.ps1). -Toolset 2022 takes that of Visual Studio 2022 instead, the fallback until the
stage 1 gate of the .NET 10 move (docs/specs/net10-plan.md, section 2.4); -VsTestPath names one directly.

The tests are MSTest 4 (docs/port-notes/S4-1.md). vstest runs each assembly with /Platform:x86 (testhost.x86.exe) and finds the
MSTest adapter (MSTest.TestAdapter.dll) next to the assembly, where the build copies it; an assembly without it is reported as
exit=NOADAPTER and not run. The counts come from vstest's summary, in either of its formats: the block that starts with
"Total tests:" (vstest.console) or the one-line "Passed! - Failed: 0, Passed: 5, Skipped: 0, Total: 5" (dotnet test and newer
vstest). An assembly whose run reports no test is exit=NOTESTS; one whose output has no summary is exit=NOSUMMARY. Each result
line is "<assembly> exit= total= passed= failed= skipped= seconds=" (docs/port-notes/S4-2.md).

No test run sends telemetry: the runner sets TESTINGPLATFORM_TELEMETRY_OPTOUT=1 and DOTNET_CLI_TELEMETRY_OPTOUT=1 for vstest and
its test hosts and removes VSTEST_TELEMETRY_OPTEDIN; OPMS_TEST_RUNNER=run-tests.ps1 tells a test that a runner started it
(tests/ProjectFileTests/TestTelemetryTests.cs checks all of this).

-CompareWith names the results file of an earlier run (of this runner or the older one without skipped=): after the run, a
COMPARE line per assembly says whether its total, passed, failed and skipped counts are the same as there. A Full or Core run
also gives each assembly of the earlier file that it lacks a 'missing' line (a test project dropped from OpenDental.Local.slnf);
the last COMPARE line counts same=, differs=, new= and missing=, and a stage gate wants differs=0 new=0 missing=0 against its
record. It changes no result.
-RequireBuildOutput sets OPMS_REQUIRE_BUILD_OUTPUT=1, so a test that checks a built file fails when the file is not built (the
stage gates' Full runs use it).

.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -Mode Targeted -Include BPAYTests,PayPlanTests
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -Mode Core
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -Mode Full
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -Mode Full -RequireBuildOutput -CompareWith earlier-full.txt
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -WaitOnly
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -List
#>
param(
	[string]$Root = '',
	# Targeted: NoHqTests + -Include. Core: every assembly except the slow migration/adoption ones (add them with -Include).
	# Full: everything. Core and Full take a machine-wide lock, so only one such run uses the shared MariaDB at a time.
	[ValidateSet('Full', 'Core', 'Targeted')][string]$Mode = 'Targeted',
	[string[]]$Include = @(),
	[string]$Out = '',
	[int]$MaxParallel = 3,
	[int]$TimeoutMinutesPerAssembly = 20,
	[switch]$WaitOnly,
	[switch]$List,
	[ValidateSet('2026', '2022')][string]$Toolset = '2026',
	[string]$VsTestPath = '',
	[switch]$RequireBuildOutput,
	[string]$CompareWith = ''
)
$ErrorActionPreference = 'Stop'
# Below-normal priority: vstest and its testhost processes inherit it, so test runs never starve the desktop.
try { (Get-Process -Id $PID).PriorityClass = 'BelowNormal' } catch {}
$heavy = @('AdoptionValidation.Tests', 'SyntheticData.Tests', 'ForkMigrationTests', 'M201Tests')
# The repository is the parent of the tools folder that holds this script, unless -Root says otherwise.
if (-not $Root) { $Root = Split-Path -Parent $PSScriptRoot }
$Root = (Resolve-Path $Root).Path
if (-not (Test-Path (Join-Path $Root 'OpenDental.Local.slnf'))) { throw "OpenDental.Local.slnf not found in $Root. Pass the repository root with -Root." }

# Get-BuildToolPath: the vswhere look-up the tool scripts share.
. (Join-Path $PSScriptRoot 'Initialize-BuildEnvironment.ps1')
function Find-VsTest {
	if ($VsTestPath) {
		if (-not (Test-Path -LiteralPath $VsTestPath -PathType Leaf)) { throw "vstest.console.exe not found at '$VsTestPath'." }
		return $VsTestPath
	}
	# Visual Studio 2026 or its Build Tools (version 18; -Toolset 2022 for version 17), any edition, never an unranged -latest.
	$found = Get-BuildToolPath -Tool VsTest -Toolset $Toolset
	if ($found) { return $found }
	throw "vstest.console.exe of Visual Studio $Toolset was not found. Install the Build Tools $Toolset with the testing tools as described in README.md, or pass -VsTestPath."
}

if (-not $Out) {
	$dir = Join-Path $env:TEMP 'odtests'
	New-Item -ItemType Directory -Force $dir | Out-Null
	$Out = Join-Path $dir ((Split-Path $Root -Leaf) + '.txt')
}
$logDir = $Out + '.logs'
if ($CompareWith -and -not (Test-Path -LiteralPath $CompareWith -PathType Leaf)) { throw "-CompareWith: no results file at '$CompareWith'." }

# The environment vstest and its test hosts inherit (package S4-2): no telemetry from the Microsoft Testing Platform parts of
# MSTest 4, none from the .NET tools, and vstest's own only when VSTEST_TELEMETRY_OPTEDIN is 1, so an inherited value is removed.
function Set-TestRunEnvironment {
	$env:TESTINGPLATFORM_TELEMETRY_OPTOUT = '1'
	$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
	Remove-Item Env:VSTEST_TELEMETRY_OPTEDIN -ErrorAction SilentlyContinue
	$env:OPMS_TEST_RUNNER = 'run-tests.ps1'
	if ($RequireBuildOutput) { $env:OPMS_REQUIRE_BUILD_OUTPUT = '1' }
}

# The counts of one vstest run from its output. vstest.console prints a block, the counts that are not zero under "Total tests:"
#   Total tests: 139
#        Passed: 138
#       Skipped: 1
#    Total time: 27.2629 Seconds
# and newer vstest and dotnet test one line per test assembly, which are added up:
#   Passed!  - Failed:     0, Passed:   138, Skipped:     1, Total:   139, Duration: 27 s - ProjectFileTests.dll (net48)
# The last block counts (a test's own output above it cannot pass for it); a count the block leaves out is 0. Total is '?' when
# neither format is there (a run that stopped before its summary).
function Get-VsTestCounts([string]$Text) {
	$blocks = [regex]::Matches($Text, '(?ms)^Total tests:[ \t]*(\d+)[ \t]*\r?$(.*?)^[ \t]*Total time:')
	if ($blocks.Count -gt 0) {
		$block = $blocks[$blocks.Count - 1]
		$counts = [ordered]@{ Total = [int]$block.Groups[1].Value; Passed = 0; Failed = 0; Skipped = 0; Format = 'block' }
		foreach ($kind in @('Passed', 'Failed', 'Skipped')) {
			$line = [regex]::Match($block.Groups[2].Value, "(?m)^[ \t]+${kind}:[ \t]*(\d+)")
			if ($line.Success) { $counts[$kind] = [int]$line.Groups[1].Value }
		}
		return [pscustomobject]$counts
	}
	$lines = [regex]::Matches($Text, '(?m)^[ \t]*(?:Passed|Failed|Skipped)![ \t]*-[ \t]*Failed:[ \t]*(\d+),[ \t]*Passed:[ \t]*(\d+),[ \t]*Skipped:[ \t]*(\d+),[ \t]*Total:[ \t]*(\d+)')
	if ($lines.Count -gt 0) {
		$counts = [ordered]@{ Total = 0; Passed = 0; Failed = 0; Skipped = 0; Format = 'line' }
		foreach ($line in $lines) {
			$counts.Failed += [int]$line.Groups[1].Value
			$counts.Passed += [int]$line.Groups[2].Value
			$counts.Skipped += [int]$line.Groups[3].Value
			$counts.Total += [int]$line.Groups[4].Value
		}
		return [pscustomobject]$counts
	}
	return [pscustomobject]@{ Total = '?'; Passed = '?'; Failed = '?'; Skipped = '?'; Format = 'none' }
}

# The result lines of a results file, by assembly. A file of the runner before S4-2 has no skipped=; its skipped count is then
# total - passed - failed (its passed=? meant no "Passed:" line, so 0).
function Read-ResultFile([string]$Path) {
	$results = @{}
	foreach ($line in Get-Content -LiteralPath $Path) {
		$m = [regex]::Match($line, '^(\S+) exit=(\S+) total=(\S+) passed=(\S+) failed=(\S+)(?: skipped=(\S+))?')
		if (-not $m.Success) { continue }
		$number = { param($v) if ($v -match '^\d+$') { [int]$v } else { $null } }
		$total = & $number $m.Groups[3].Value
		$passed = & $number $m.Groups[4].Value
		$failed = & $number $m.Groups[5].Value
		$skipped = if ($m.Groups[6].Success) { & $number $m.Groups[6].Value } elseif ($null -ne $total) { $total - [int]$passed - [int]$failed } else { $null }
		if ($null -ne $total -and $null -eq $passed -and $m.Groups[4].Value -eq '?') { $passed = 0 }
		$results[$m.Groups[1].Value] = [pscustomobject]@{ Exit = $m.Groups[2].Value; Total = $total; Passed = $passed; Failed = $failed; Skipped = $skipped }
	}
	return $results
}

# COMPARE lines: each assembly of this run against the same assembly of an earlier results file. A run of every assembly (Full or
# Core) also lists each assembly of the earlier file that this run lacks as missing: a test project dropped from OpenDental.Local.slnf
# is never discovered, so it has no result line of its own. A Targeted run lists none, since it leaves out most assemblies on purpose.
function Write-Comparison([string]$EarlierPath, $Earlier = $null) {
	$earlier = if ($null -ne $Earlier) { $Earlier } else { Read-ResultFile $EarlierPath }
	$now = Read-ResultFile $Out
	Add-Content $Out "COMPARE with=$EarlierPath" -Encoding utf8
	$same = 0; $differ = 0; $new = 0; $missing = 0
	foreach ($name in ($now.Keys | Sort-Object)) {
		$b = $now[$name]
		$text = 'total={0} passed={1} failed={2} skipped={3}' -f $b.Total, $b.Passed, $b.Failed, $b.Skipped
		if (-not $earlier.ContainsKey($name)) { Add-Content $Out "COMPARE $name new $text" -Encoding utf8; $new++; continue }
		$a = $earlier[$name]
		$changes = @(foreach ($field in @('Total', 'Passed', 'Failed', 'Skipped')) { if ("$($a.$field)" -ne "$($b.$field)") { '{0} {1}->{2}' -f $field.ToLower(), $a.$field, $b.$field } })
		if ($changes.Count -eq 0) { Add-Content $Out "COMPARE $name same $text" -Encoding utf8; $same++ }
		else { Add-Content $Out "COMPARE $name differs $($changes -join ', ')" -Encoding utf8; $differ++ }
	}
	if ($Mode -ne 'Targeted') {
		foreach ($name in ($earlier.Keys | Where-Object { -not $now.ContainsKey($_) } | Sort-Object)) {
			$a = $earlier[$name]
			Add-Content $Out ('COMPARE {0} missing total={1} passed={2} failed={3} skipped={4}' -f $name, $a.Total, $a.Passed, $a.Failed, $a.Skipped) -Encoding utf8
			$missing++
		}
	}
	Add-Content $Out "COMPARE same=$same differs=$differ new=$new missing=$missing" -Encoding utf8
}

function Show-Summary {
	if (-not (Test-Path $Out)) { Write-Output "No results yet at $Out"; return }
	Get-Content $Out
	$failed = Get-Content $Out | Where-Object { $_ -match '^(\S+) exit=(?!0\b)' } | ForEach-Object { ($_ -split ' ')[0] }
	foreach ($name in $failed) {
		$log = Join-Path $logDir "$name.log"
		if (Test-Path $log) {
			Write-Output "---- ${name}: failing tests and errors (first 40 lines)"
			Get-Content $log | Where-Object { $_ -match '^\s*Failed\s|^\s*Error Message:|^\s{2,}(Assert\.|System\.\w+Exception|Test method .* threw)|timed out' } | Select-Object -First 40
		}
	}
}

if ($WaitOnly) {
	$deadline = (Get-Date).AddMinutes(9)
	while ((Get-Date) -lt $deadline) {
		if ((Test-Path $Out) -and (Select-String -Path $Out -Pattern '^DONE ' -Quiet)) { break }
		Start-Sleep -Seconds 10
	}
	if (-not ((Test-Path $Out) -and (Select-String -Path $Out -Pattern '^DONE ' -Quiet))) { Write-Output 'STILL RUNNING: call again with -WaitOnly.' }
	Show-Summary
	exit 0
}

# Discover test projects: every *Tests / *.Tests project listed in OpenDental.Local.slnf, the legacy UnitTests project included (package S0-5).
$slnf = Get-Content (Join-Path $Root 'OpenDental.Local.slnf') -Raw | ConvertFrom-Json
$projects = $slnf.solution.projects | Where-Object { $_ -match 'Tests\.csproj$' } | ForEach-Object { Get-Item (Join-Path $Root $_) -ErrorAction SilentlyContinue } | Where-Object { $_ }
$assemblies = @()
foreach ($p in $projects) {
	[xml]$x = Get-Content $p.FullName
	$assemblyName = ($x.Project.PropertyGroup | ForEach-Object { $_.AssemblyName } | Where-Object { $_ } | Select-Object -First 1)
	if (-not $assemblyName) { $assemblyName = $p.BaseName }
	# An assembly name with a space ('Unit Tests') is listed and included by its project name (UnitTests).
	$name = if ($assemblyName -match '\s') { $p.BaseName } else { $assemblyName }
	# A test container is a DLL, or an EXE (the legacy UnitTests project is a WinExe); vstest runs either.
	$dll = foreach ($folder in @('bin\Debug', 'bin\x86\Debug')) { foreach ($extension in @('.dll', '.exe')) { Join-Path $p.DirectoryName "$folder\$assemblyName$extension" } }
	$dll = $dll | Where-Object { Test-Path $_ } | Select-Object -First 1
	$assemblies += [pscustomobject]@{ Name = $name; Dll = $dll }
}
$assemblies = $assemblies | Sort-Object Name -Unique
if ($List) { $assemblies | ForEach-Object { '{0} {1}' -f $_.Name, ($(if ($_.Dll) { 'built' } else { 'NOT BUILT' })) }; exit 0 }

$vstest = Find-VsTest
$extra = @($Include | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() -replace '\.dll$', '' } | Where-Object { $_ })
if ($Mode -eq 'Targeted') {
	$want = @('NoHqTests') + $extra
	$unknown = $want | Where-Object { $n = $_; -not ($assemblies | Where-Object { $_.Name -eq $n }) }
	if ($unknown) { Write-Output "Unknown test assemblies (use -List): $($unknown -join ', ')" }
	$assemblies = $assemblies | Where-Object { $want -contains $_.Name }
} elseif ($Mode -eq 'Core') {
	$assemblies = $assemblies | Where-Object { ($heavy -notcontains $_.Name) -or ($extra -contains $_.Name) }
}

# Core and Full runs hit the one local MariaDB hard: take turns machine-wide (the lock frees itself if its holder dies).
$lock = $null
if ($Mode -ne 'Targeted') {
	$lock = New-Object System.Threading.Mutex($false, 'Local\OpenPMS-Dental-Tests')
	Write-Output "Waiting for the machine-wide test lock (another Core/Full run may be using the database)..."
	try { [void]$lock.WaitOne() } catch [System.Threading.AbandonedMutexException] { }
}

Set-TestRunEnvironment
New-Item -ItemType Directory -Force $logDir | Out-Null
# The earlier record is read before -Out is written: -CompareWith may name the same file (the default -Out of an earlier run).
$earlierRecord = if ($CompareWith) { Read-ResultFile $CompareWith } else { $null }
Set-Content -Path $Out -Value "RUN mode=$Mode root=$Root started=$(Get-Date -Format s) assemblies=$($assemblies.Count) vstest=$vstest" -Encoding utf8
$queue = New-Object System.Collections.Queue
$assemblies | ForEach-Object { $queue.Enqueue($_) }
$running = @()
$allOk = $true
while ($queue.Count -gt 0 -or $running.Count -gt 0) {
	while ($running.Count -lt $MaxParallel -and $queue.Count -gt 0) {
		$a = $queue.Dequeue()
		$log = Join-Path $logDir "$($a.Name).log"
		if (-not $a.Dll) {
			Add-Content $Out "$($a.Name) exit=NOTBUILT total=0 passed=0 failed=0 skipped=0 seconds=0" -Encoding utf8
			$allOk = $false
			continue
		}
		# vstest loads the test adapters it finds next to the assembly (*.TestAdapter.dll); without MSTest's it would find no test.
		if (-not (Test-Path -LiteralPath (Join-Path (Split-Path $a.Dll) 'MSTest.TestAdapter.dll') -PathType Leaf)) {
			Add-Content $Out "$($a.Name) exit=NOADAPTER total=0 passed=0 failed=0 skipped=0 seconds=0" -Encoding utf8
			$allOk = $false
			continue
		}
		$proc = Start-Process -FilePath $vstest -ArgumentList @("`"$($a.Dll)`"", '/Platform:x86') -WorkingDirectory (Split-Path $a.Dll) -NoNewWindow -PassThru -RedirectStandardOutput $log -RedirectStandardError ($log + '.err')
		$null = $proc.Handle  # cache the handle so ExitCode is available after the process exits
		$running += [pscustomobject]@{ A = $a; P = $proc; Log = $log; Start = Get-Date }
	}
	Start-Sleep -Milliseconds 500
	$still = @()
	foreach ($r in $running) {
		$elapsed = ((Get-Date) - $r.Start).TotalMinutes
		if (-not $r.P.HasExited -and $elapsed -gt $TimeoutMinutesPerAssembly) {
			& taskkill.exe /T /F /PID $r.P.Id 2>&1 | Out-Null
			Add-Content $Out "$($r.A.Name) exit=TIMEOUT total=? passed=? failed=? skipped=? seconds=$([int]($elapsed*60))" -Encoding utf8
			$allOk = $false
			continue
		}
		if (-not $r.P.HasExited) { $still += $r; continue }
		$r.P.WaitForExit()
		$text = (Get-Content $r.Log -Raw -ErrorAction SilentlyContinue) + ''
		$counts = Get-VsTestCounts $text
		$code = "$($r.P.ExitCode)"
		# vstest ends with 0 when it finds no test at all: that is no pass.
		if ($code -eq '0' -and $counts.Total -eq '?' -and $text -match 'No test is available|No test matches') { $code = 'NOTESTS' }
		elseif ($code -eq '0' -and $counts.Total -eq '?') { $code = 'NOSUMMARY' }
		elseif ($code -eq '0' -and $counts.Total -eq 0) { $code = 'NOTESTS' }
		if ($code -ne '0') { $allOk = $false }
		Add-Content $Out "$($r.A.Name) exit=$code total=$($counts.Total) passed=$($counts.Passed) failed=$($counts.Failed) skipped=$($counts.Skipped) seconds=$([int](((Get-Date) - $r.Start).TotalSeconds))" -Encoding utf8
	}
	$running = $still
}
if ($lock) { try { $lock.ReleaseMutex() } catch {} }
if ($CompareWith) { Write-Comparison $CompareWith $earlierRecord }
Add-Content $Out "DONE ok=$allOk finished=$(Get-Date -Format s)" -Encoding utf8
Show-Summary
if ($allOk) { exit 0 } else { exit 1 }
