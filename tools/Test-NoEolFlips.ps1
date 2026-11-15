<#
.SYNOPSIS
	Lists the tracked files whose line ends a branch changed: a file that is all CRLF at the base and has bare LF lines at the head, or all
	LF at the base and has CRLF lines at the head. Exit 1 while any is found.

.DESCRIPTION
	The repository has no text or eol rule for code in .gitattributes, so the blobs keep the line ends they were committed with: Open
	Dental's files are CRLF and many fork files are LF. With core.autocrlf=true (Git for Windows' default) a file that a tool rewrites with
	LF is committed with LF when its blob was CRLF, and the branch's diff then rewrites every line of the file: the real change is hidden,
	git blame is lost and any other branch that touches the file conflicts on all of it (TC-5 fix round 5, finding 2).

	For every file the head changes against the merge base of Base and Head (git diff Base...Head), the script reads both blobs and sorts
	each as crlf (no bare LF), lf (no CRLF), mixed or none (no line end). It reports a file that is crlf at the base and has any bare LF at
	the head, or lf at the base and has any CRLF at the head. A file that was already mixed, a new file, a deleted file, a binary blob (a
	NUL byte in its first 8000 bytes, as git decides) and a file whose text attribute is unset (-text in .gitattributes) are not checked.

	Run it on a committed branch, before it is reviewed or merged. To fix a reported file, write it back with the base's line ends at the
	head's content and commit it with core.autocrlf off for that one command, so the line ends reach the blob as written:
	git -c core.autocrlf=false add <file>. `git diff --ignore-cr-at-eol` against the previous commit must then be empty.

	Exit code 0: nothing changed its line ends. 1: files did. 2: the script could not run.

.PARAMETER Root
	Repository root (any folder in the work tree). Defaults to the parent of the folder that contains this script.

.PARAMETER Base
	The branch or commit the head is compared with (through their merge base). Defaults to main.

.PARAMETER Head
	The branch or commit that is checked. Defaults to HEAD.

.EXAMPLE
	powershell -NoProfile -ExecutionPolicy Bypass -File tools\Test-NoEolFlips.ps1

.EXAMPLE
	powershell -NoProfile -ExecutionPolicy Bypass -File tools\Test-NoEolFlips.ps1 -Base main -Head chart/tc5-5
#>
[CmdletBinding()]
param(
	[string]$Root='',
	[string]$Base='main',
	[string]$Head='HEAD'
)

Set-StrictMode -Version 2.0
$ErrorActionPreference='Stop'

#Runs git in the repository and returns its standard output as bytes (blobs are read as they are stored, with no conversion).
function Invoke-GitBytes([string[]]$GitArgs) {
	$psi=New-Object System.Diagnostics.ProcessStartInfo
	$psi.FileName='git'
	$psi.Arguments=(($GitArgs | ForEach-Object { '"'+($_ -replace '"','\"')+'"' }) -join ' ')
	$psi.WorkingDirectory=$script:RepoRoot
	$psi.UseShellExecute=$false
	$psi.RedirectStandardOutput=$true
	$psi.RedirectStandardError=$true
	$psi.CreateNoWindow=$true
	$process=[System.Diagnostics.Process]::Start($psi)
	$stderrTask=$process.StandardError.ReadToEndAsync()
	$memoryStream=New-Object System.IO.MemoryStream
	$process.StandardOutput.BaseStream.CopyTo($memoryStream)
	$process.WaitForExit()
	$stderr=$stderrTask.Result
	if($process.ExitCode -ne 0) {
		throw ('git '+($GitArgs -join ' ')+' failed ('+$process.ExitCode+'): '+$stderr.Trim())
	}
	return ,$memoryStream.ToArray()
}

#Runs git and returns its standard output as UTF-8 text.
function Invoke-GitText([string[]]$GitArgs) {
	$bytes=Invoke-GitBytes $GitArgs
	return [System.Text.Encoding]::UTF8.GetString($bytes)
}

#Sorts a blob's line ends: 'binary', 'none', 'crlf', 'lf' or 'mixed'. Also returns the counts.
function Get-EolStyle([byte[]]$Bytes) {
	$probe=[Math]::Min($Bytes.Length,8000)
	for($i=0;$i -lt $probe;$i++) {
		if($Bytes[$i] -eq 0) {
			return @{ Style='binary'; Crlf=0; Lf=0 }
		}
	}
	$crlf=0
	$lf=0
	for($i=0;$i -lt $Bytes.Length;$i++) {
		if($Bytes[$i] -ne 10) {
			continue
		}
		if($i -gt 0 -and $Bytes[$i-1] -eq 13) {
			$crlf++
		}
		else {
			$lf++
		}
	}
	$style='mixed'
	if($crlf -eq 0 -and $lf -eq 0) {
		$style='none'
	}
	elseif($lf -eq 0) {
		$style='crlf'
	}
	elseif($crlf -eq 0) {
		$style='lf'
	}
	return @{ Style=$style; Crlf=$crlf; Lf=$lf }
}

try {
	if([string]::IsNullOrEmpty($Root)) {
		$Root=Split-Path -Parent $PSScriptRoot
	}
	$script:RepoRoot=(Resolve-Path -LiteralPath $Root).Path
	$mergeBase=(Invoke-GitText @('merge-base',$Base,$Head)).Trim()
	$headCommit=(Invoke-GitText @('rev-parse','--verify',($Head+'^{commit}'))).Trim()
	#Modified files only (renames are compared as delete plus add, so a renamed file is new and not checked).
	$names=(Invoke-GitText @('diff','--no-renames','--diff-filter=M','--name-only','-z',$mergeBase,$headCommit)).Split([char]0) |
		Where-Object { $_ -ne '' }
}
catch {
	Write-Host ('Test-NoEolFlips could not run: '+$_.Exception.Message)
	exit 2
}

$flipped=New-Object System.Collections.Generic.List[string]
$checked=0
foreach($name in $names) {
	try {
		$attr=(Invoke-GitText @('check-attr','text','--',$name)).Trim()
		if($attr -match ': text: unset$') {
			continue
		}
		$baseEol=Get-EolStyle (Invoke-GitBytes @('cat-file','blob',($mergeBase+':'+$name)))
		if($baseEol.Style -ne 'crlf' -and $baseEol.Style -ne 'lf') {
			continue
		}
		$headEol=Get-EolStyle (Invoke-GitBytes @('cat-file','blob',($headCommit+':'+$name)))
		if($headEol.Style -eq 'binary') {
			continue
		}
		$checked++
		if($baseEol.Style -eq 'crlf' -and $headEol.Lf -gt 0) {
			$flipped.Add($name+': CRLF at the base ('+$baseEol.Crlf+' lines), '+$headEol.Lf+' bare LF lines at the head')
		}
		elseif($baseEol.Style -eq 'lf' -and $headEol.Crlf -gt 0) {
			$flipped.Add($name+': LF at the base ('+$baseEol.Lf+' lines), '+$headEol.Crlf+' CRLF lines at the head')
		}
	}
	catch {
		Write-Host ('Test-NoEolFlips could not read '+$name+': '+$_.Exception.Message)
		exit 2
	}
}

$summary='Test-NoEolFlips: '+$Head+' against '+$Base+' (merge base '+$mergeBase.Substring(0,9)+'): '+$checked+' changed text files checked, '
$summary+=[string]$flipped.Count+' changed their line ends.'
Write-Host $summary
foreach($line in $flipped) {
	Write-Host ('  '+$line)
}
if($flipped.Count -gt 0) {
	exit 1
}
exit 0
