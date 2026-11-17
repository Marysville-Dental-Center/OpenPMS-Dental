<#
.SYNOPSIS
Records the .NET Framework 4.8 truth table of S3b-FMT (decision 24 of docs/specs/net10-plan.md, docs/port-notes/S3b-FMT.md).

.DESCRIPTION
Compiles Net48Probe\Net48Probe.cs with the C# compiler of the .NET Framework 4.8 folder (csc.exe of Framework\v4.0.30319, the runtime the
.NET Framework build of the program ran on), as an x86 program (the .NET Framework program was x86) and as an x64 program, runs both on
.NET Framework 4.8 with formats.txt, compares the two outputs row by row, and writes the x86 output to net48-truth.txt next to this
script, with the x64 rows that differ (only 17-digit formats: G17, R, E16) as X rows. The compiled probes go to a scratch folder under
%TEMP%, never into the repository. Synthetic values only; no database, no network.

.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tests\FormatParity\Make-Net48Truth.ps1
#>
[CmdletBinding()]
param(
	[string]$OutFile = ''
)
$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
if ($OutFile -eq '') { $OutFile = Join-Path $here 'net48-truth.txt' }
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) { throw "The .NET Framework 4.8 compiler is missing: $csc" }
$scratch = Join-Path ([System.IO.Path]::GetTempPath()) ('net48probe-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
try {
	$source = Join-Path $here 'Net48Probe\Net48Probe.cs'
	$formats = Join-Path $here 'formats.txt'
	$outputs = @{}
	foreach ($platform in @('x86', 'x64')) {
		$exe = Join-Path $scratch "Net48Probe-$platform.exe"
		& $csc /nologo /optimize+ /target:exe "/platform:$platform" "/out:$exe" $source
		if ($LASTEXITCODE -ne 0) { throw "csc failed for $platform ($LASTEXITCODE)" }
		$out = Join-Path $scratch "truth-$platform.txt"
		& $exe $formats $out
		if ($LASTEXITCODE -ne 0) { throw "The $platform probe failed ($LASTEXITCODE)" }
		$outputs[$platform] = $out
	}
	#The two processes write the same rows; only the header line naming the process differs.
	$rows86 = @(Get-Content -LiteralPath $outputs['x86'] | Where-Object { -not $_.StartsWith('#') })
	$rows64 = @(Get-Content -LiteralPath $outputs['x64'] | Where-Object { -not $_.StartsWith('#') })
	if ($rows86.Count -ne $rows64.Count) { throw "x86 wrote $($rows86.Count) rows, x64 $($rows64.Count)" }
	$differences = 0
	$alternatives = New-Object System.Collections.Generic.List[string]
	for ($i = 0; $i -lt $rows86.Count; $i++) {
		if ($rows86[$i] -cne $rows64[$i]) {
			$differences++
			if ($differences -le 20) { Write-Output "x86/x64 differ: [$($rows86[$i])] [$($rows64[$i])]" }
			#Only the 17-digit formats differ (G17, R, E16: the 17th digit of a few values); the x64 row is the other accepted text.
			$alternatives.Add('X' + $rows64[$i].Substring(1))
		}
	}
	$text = [System.IO.File]::ReadAllText($outputs['x86'])
	$text += "# The x64 .NET Framework's text where it differs from the x86 one (same columns as N; the test accepts either):`n"
	foreach ($row in $alternatives) { $text += $row + "`n" }
	[System.IO.File]::WriteAllText($OutFile, $text, (New-Object System.Text.UTF8Encoding($false)))
	Write-Output "Rows: $($rows86.Count); x86 and x64 differ in $differences; written to $OutFile"
}
finally {
	Remove-Item -LiteralPath $scratch -Recurse -Force -ErrorAction SilentlyContinue
}
