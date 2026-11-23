<#
.SYNOPSIS
UI-12T item 4: runs a grid rollout slice's mechanical chain (docs/port-notes/UI-12.md section 2 and "Tooling (UI-12T)").
.DESCRIPTION
The steps, in order (-Steps picks some; the default is all of them):
  add       python tools/UiFit/plan_batches.py --add <Batch> --slice <Slice>: the batch's windows into Data/grid-windows/<Slice>.tsv (mode tool).
            Skipped without -Batch; the slice's windows are then the rows of Data/grid-windows/<Slice>.tsv (or -Windows).
  before    build OpenPmsDental and UiRefreshTests through the lane (the branch point), then one Targeted UiRefreshTests run that dumps
            the slice's tree (tree-before.tsv) and draws the before gallery at 100%, and one that draws it at 150%.
  plan      grid_tools.py plan on tree-before.tsv (plan.tsv, unplaced.tsv), apply plan.tsv, apply the slice's hand rows
            (Data/grid-hand/<Slice>.tsv) when there are any.
  after     build OpenPmsDental and UiRefreshTests again, check that OpenPmsDental.dll in the UiRefreshTests bin is the one just built
            (the gallery lesson: the test bin keeps its own copy), then a Targeted run that dumps tree-after.tsv and draws the after gallery at
            100% (and writes the layout snapshot when the snapshot step is chosen too), and one at 150%.
  replan    grid_tools.py plan on tree-after.tsv (replan.tsv): it must be empty (UI-12.md section 2, step 5).
  snapshot  writes the layout snapshot (OPMS_UI_LAYOUT_SNAPSHOT_WRITE=1, one file per slice); with the after step it rides on its first run.
Everything goes to -Work (default %TEMP%\ui12-slice-<Slice>): the trees, the plans, the galleries (before-100, before-150, after-100,
after-150) and the test results. The script commits nothing and shows no window; the slice looks at the galleries, fixes, and commits.
The lane (odbuild.ps1) is -BuildScript or $env:OPMS_ODBUILD; the test runner is -TestScript, $env:OPMS_RUN_TESTS, or od-tools/run-tests.ps1
in the repository's common git folder. Exit 98 from the lane (busy) is retried. -Print shows the commands without running them.
.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tools/UiFit/Run-Slice.ps1 -Slice 2 -Batch 1 -BuildScript <odbuild.ps1>
powershell -NoProfile -ExecutionPolicy Bypass -File tools/UiFit/Run-Slice.ps1 -Slice 2 -Steps after,replan,snapshot
#>
param(
	[Parameter(Mandatory = $true)][string]$Slice,
	[int]$Batch = 0,
	[string]$Windows = '',
	[string[]]$Steps = @('add', 'before', 'plan', 'after', 'replan', 'snapshot'),
	[string]$Root = '',
	[string]$Work = '',
	[string]$BuildScript = $env:OPMS_ODBUILD,
	[string]$TestScript = $env:OPMS_RUN_TESTS,
	[string]$Platform = 'x64',
	[int]$WaitMinutes = 20,
	[switch]$Print
)
$ErrorActionPreference = 'Stop'
if ($Root -eq '') { $Root = (git rev-parse --show-toplevel).Trim() }
$Root = (Resolve-Path $Root).Path
if ($Work -eq '') { $Work = Join-Path $env:TEMP ("ui12-slice-" + $Slice) }
$Steps = @($Steps | ForEach-Object { $_ -split ',' } | Where-Object { $_ -ne '' })
$valid = @('add', 'before', 'plan', 'after', 'replan', 'snapshot')
foreach ($s in $Steps) { if ($valid -notcontains $s) { throw "Unknown step '$s' (steps: $($valid -join ', '))" } }
if ($TestScript -eq '' -or $null -eq $TestScript) {
	$common = (git -C $Root rev-parse --path-format=absolute --git-common-dir).Trim()
	$TestScript = Join-Path $common 'od-tools/run-tests.ps1'
}
New-Item -ItemType Directory -Force $Work | Out-Null
$uiFit = Join-Path $Root 'tools/UiFit'
$summary = New-Object System.Collections.Generic.List[string]

function Invoke-Step([string]$Label, [scriptblock]$Block) {
	Write-Host "== $Label"
	if ($Print) { Write-Host ("   " + $Block.ToString().Trim()); return }
	& $Block
}

function Invoke-Python([string[]]$Arguments) {
	if ($Print) { Write-Host ("   python " + ($Arguments -join ' ')); return }
	Push-Location $Root
	try {
		& python @Arguments
		if ($LASTEXITCODE -ne 0) { throw "python $($Arguments -join ' ') failed ($LASTEXITCODE)" }
	}
	finally { Pop-Location }
}

function Invoke-Build {
	if ($Print) { Write-Host "   lane: -Projects OpenPmsDental,UiRefreshTests -ScopedRestore -Platform $Platform"; return }
	if ($BuildScript -eq '' -or $null -eq $BuildScript -or -not (Test-Path $BuildScript)) { throw 'No lane: pass -BuildScript or set OPMS_ODBUILD.' }
	while ($true) {
		& powershell -NoProfile -ExecutionPolicy Bypass -File $BuildScript -Root $Root -WaitMinutes $WaitMinutes -Projects OpenPmsDental,UiRefreshTests -ScopedRestore -Platform $Platform
		if ($LASTEXITCODE -eq 98) { Write-Host 'lane busy: again'; continue }
		if ($LASTEXITCODE -ne 0) { throw "build failed ($LASTEXITCODE)" }
		break
	}
}

# One Targeted UiRefreshTests run with the given environment (the opt-in dump, gallery and snapshot tests read it).
function Invoke-Tests([string]$Name, [hashtable]$Environment) {
	$out = Join-Path $Work ("tests-" + $Name + ".txt")
	if ($Print) { Write-Host ("   tests " + $Name + ": " + (($Environment.Keys | Sort-Object | ForEach-Object { "$_=" + $Environment[$_] }) -join ' ')); return }
	$saved = @{}
	foreach ($k in $Environment.Keys) { $saved[$k] = [Environment]::GetEnvironmentVariable($k); [Environment]::SetEnvironmentVariable($k, $Environment[$k]) }
	try {
		& powershell -NoProfile -ExecutionPolicy Bypass -File $TestScript -Root $Root -Mode Targeted -Platform $Platform -RequireBuildOutput -Include UiRefreshTests -Out $out | Out-Null
	}
	finally {
		foreach ($k in $saved.Keys) { [Environment]::SetEnvironmentVariable($k, $saved[$k]) }
	}
	$done = Get-Content $out | Where-Object { $_ -like 'DONE*' } | Select-Object -Last 1
	$summary.Add("tests ${Name}: $done ($out)")
	Write-Host "   $done"
}

function Get-SliceWindows {
	if ($Windows -ne '') { return $Windows }
	$file = Join-Path $Root ("tests/UiRefreshTests/Data/grid-windows/" + $Slice + ".tsv")
	if (-not (Test-Path $file)) { if ($Print) { return "<the windows of grid-windows/$Slice.tsv>" }; throw "No $file (pass -Batch or -Windows)" }
	$list = Get-Content $file | Where-Object { $_ -ne '' -and -not $_.StartsWith('#') -and -not $_.StartsWith("window`t") } | ForEach-Object { ($_ -split "`t")[0] }
	return ($list -join ',')
}

function Get-GalleryEnvironment([string]$Folder, [string]$Scale, [string]$List) {
	return @{ 'OPMS_UI_FIT_GALLERY' = (Join-Path $Work $Folder); 'OPMS_UI_FIT_GALLERY_FONT' = 'program'; 'OPMS_UI_FIT_GALLERY_WINDOWS' = $List; 'OPMS_UI_FIT_GALLERY_SCALE' = $Scale }
}

function Test-DllCopy {
	if ($Print) { Write-Host '   sha256 of OpenPmsDental.dll: OpenPmsDental bin = UiRefreshTests bin'; return }
	$built = Get-ChildItem (Join-Path $Root 'OpenPmsDental/bin') -Recurse -Filter OpenPmsDental.dll | Sort-Object LastWriteTime | Select-Object -Last 1
	$copy = Get-ChildItem (Join-Path $Root 'tests/UiRefreshTests/bin') -Recurse -Filter OpenPmsDental.dll | Sort-Object LastWriteTime | Select-Object -Last 1
	if ($null -eq $built -or $null -eq $copy) { throw 'OpenPmsDental.dll not found in a bin folder.' }
	$a = (Get-FileHash $built.FullName -Algorithm SHA256).Hash
	$b = (Get-FileHash $copy.FullName -Algorithm SHA256).Hash
	if ($a -ne $b) { throw "The UiRefreshTests bin has another OpenPmsDental.dll ($b) than the build ($a): the gallery would be stale." }
	$summary.Add("OpenPmsDental.dll sha256 $a in both bins")
}

function Get-PlanRows([string]$Path) {
	if ($Print -or -not (Test-Path $Path)) { return 0 }
	return [Math]::Max(0, @(Get-Content $Path).Count - 1)
}

if ($Steps -contains 'add') {
	if ($Batch -gt 0) {
		Invoke-Step "add: batch $Batch into slice $Slice" { Invoke-Python @('tools/UiFit/plan_batches.py', '--add', "$Batch", '--slice', $Slice) }
	}
	else { Write-Host '== add: no -Batch, the slice file stays as it is' }
}
$list = Get-SliceWindows
$treeBefore = Join-Path $Work 'tree-before.tsv'
$treeAfter = Join-Path $Work 'tree-after.tsv'
if ($Steps -contains 'before') {
	Invoke-Step 'before: build the branch point' { Invoke-Build }
	$envBefore = Get-GalleryEnvironment 'before-100' '1' $list
	$envBefore['OPMS_UI_GRID_DUMP'] = $treeBefore
	$envBefore['OPMS_UI_GRID_WINDOWS'] = $list
	Invoke-Tests 'before-100' $envBefore
	Invoke-Tests 'before-150' (Get-GalleryEnvironment 'before-150' '1.5' $list)
}
if ($Steps -contains 'plan') {
	$plan = Join-Path $Work 'plan.tsv'
	Invoke-Python @('tools/UiFit/grid_tools.py', 'plan', '--tree', $treeBefore, '--windows', $list, '--out', $plan, '--unplaced', (Join-Path $Work 'unplaced.tsv'))
	Invoke-Python @('tools/UiFit/grid_tools.py', 'apply', '--plan', $plan)
	$hand = Join-Path $Root ("tests/UiRefreshTests/Data/grid-hand/" + $Slice + ".tsv")
	if ($Print -or (Test-Path $hand)) { Invoke-Python @('tools/UiFit/grid_tools.py', 'apply', '--plan', $hand) }
	$summary.Add("plan: $(Get-PlanRows $plan) rows ($plan), unplaced: $(Get-PlanRows (Join-Path $Work 'unplaced.tsv'))")
}
if ($Steps -contains 'after') {
	Invoke-Step 'after: build the applied slice' { Invoke-Build }
	Test-DllCopy
	$envAfter = Get-GalleryEnvironment 'after-100' '1' $list
	$envAfter['OPMS_UI_GRID_DUMP'] = $treeAfter
	$envAfter['OPMS_UI_GRID_WINDOWS'] = $list
	if ($Steps -contains 'snapshot') { $envAfter['OPMS_UI_LAYOUT_SNAPSHOT_WRITE'] = '1' }
	Invoke-Tests 'after-100' $envAfter
	Invoke-Tests 'after-150' (Get-GalleryEnvironment 'after-150' '1.5' $list)
}
elseif ($Steps -contains 'snapshot') {
	Invoke-Tests 'snapshot' @{ 'OPMS_UI_LAYOUT_SNAPSHOT_WRITE' = '1' }
}
if ($Steps -contains 'replan') {
	$replan = Join-Path $Work 'replan.tsv'
	Invoke-Python @('tools/UiFit/grid_tools.py', 'plan', '--tree', $treeAfter, '--windows', $list, '--out', $replan, '--unplaced', (Join-Path $Work 'replan-unplaced.tsv'))
	$rows = Get-PlanRows $replan
	$summary.Add("replan: $rows rows ($replan)" + $(if ($rows -gt 0) { ' - not empty: a rule is not a fixed point, or rows of other designers' } else { '' }))
}
Write-Host '== summary'
foreach ($line in $summary) { Write-Host "   $line" }
Write-Host "   work folder: $Work"
