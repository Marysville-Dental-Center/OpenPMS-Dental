<#
.SYNOPSIS
Makes a before/after picture gallery of the UI refresh for the owner (docs/specs/ui-refresh-plan.md, section 8) and zips it.

.DESCRIPTION
Takes a folder of before pictures, a folder of after pictures (*.png, matched by their path relative to the folder) and
the mask file, and writes into -Out:
  before/, after/  the pictures, copied as they are;
  diff/            for each changed picture, the after picture faded to grey with changed pixels red, the area only one
                   picture has magenta and the masked areas hatched in blue;
  gallery.tsv      per picture: group, status, sizes, changed, compared and masked pixels, % changed, the masked pixel
                   SHA-256 of the after and the before picture, whether the owner approved it before, the clip notes;
  masks.tsv        a copy of the mask file used;
  index.html       the page: the pictures at 1:1 device pixels (pixelated, with a zoom toggle), grouped and filterable,
                   the changed ones first, and an Approve/Change column the owner fills (the answers download as a TSV);
and -Out.zip beside it, with forward-slash entry names, so the zip opens on any system.

The masks (tools/ui-gallery-masks.tsv) hide the clock, dates and the copy's database name. The masking and hashing rule is
ONE function, UiPictureMask.GetMaskedPixelSha256 in tools/UiGallery/UiPictureMask.cs, which this script compiles with
Add-Type and the window checks' check U0 compiles in as a linked file (docs/port-notes/UI-0a.md).

The pictures are synthetic only: the harness's winchk_ copies of the synthetic dump, or test pictures. Needs no program,
no database and no network. Windows PowerShell 5.1 or later.

.PARAMETER Before
The folder of before pictures. Optional: without it every picture is "only after" (a first gallery).

.PARAMETER After
The folder of after pictures.

.PARAMETER Out
The gallery folder: a new or empty folder, or an earlier gallery (which is emptied). Any other folder is refused.
Default: %TEMP%\UiGallery-<Name>.

.PARAMETER Name
The gallery's name, like G-UI1, G-UI2, G-UI3, G-UI4 (never shortened to G1: the tooth chart's galleries are G0 to G5).

.PARAMETER Masks
The mask file. Default: tools\ui-gallery-masks.tsv beside this script.

.PARAMETER ClipNotes
A TSV of clip notes: picture, then the note (more fields are joined with "; "). Default: clip-notes.tsv in the after
folder, when it has one.

.PARAMETER Approved
The owner's approvals. Default: tests\WindowChecksTests\UiApproved\approved.tsv of the repository.

.PARAMETER Commit
The commit the after pictures were taken at. Default: git rev-parse --short HEAD of the repository.

.PARAMETER Note
A line for the owner at the top of the page.

.PARAMETER Threshold
A pixel counts as changed when a colour or alpha value differs by more than this. Default 0: any difference.

.PARAMETER NoZip
Do not write the zip.

.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <before folder> -After <after folder> -Name G-UI3
#>
param(
	[string]$Before = '',
	[string]$After = '',
	[string]$Out = '',
	[string]$Name = 'G-UI',
	[string]$Masks = '',
	[string]$ClipNotes = '',
	[string]$Approved = '',
	[string]$Commit = '',
	[string]$Note = '',
	[int]$Threshold = 0,
	[string]$Root = '',
	[switch]$NoZip
)
$ErrorActionPreference = 'Stop'

# The marker file of a gallery folder: a later run empties only a folder that holds it.
$script:MarkerFileName = 'ui-gallery.txt'

# Compiles the one masking and hashing rule (tools/UiGallery/UiPictureMask.cs), once per session.
function Import-UiPictureMask {
	if (-not ('UiGalleryTools.UiPictureMask' -as [type])) {
		Add-Type -AssemblyName System.Drawing
		Add-Type -Path (Join-Path $PSScriptRoot 'UiGallery\UiPictureMask.cs') -ReferencedAssemblies System.Drawing
	}
}

# Every *.png under the folder, as a sorted dictionary of the relative path with forward slashes to the full path.
function Get-UiPictures {
	param([string]$Folder)
	$dict = New-Object 'System.Collections.Generic.SortedDictionary[string,string]' ([System.StringComparer]::OrdinalIgnoreCase)
	if (-not $Folder) { return ,$dict }
	$folderFull = [System.IO.Path]::GetFullPath($Folder).TrimEnd('\') + '\'
	foreach ($file in (Get-ChildItem -LiteralPath $Folder -Recurse -File -Filter '*.png')) {
		$relative = $file.FullName.Substring($folderFull.Length).Replace('\', '/')
		$dict[$relative] = $file.FullName
	}
	return ,$dict
}

# The group of a picture: its first folder, or else the letters before the first digit of its name (the harness's group:
# A1-main.png is in group A, U12-claims.png in group U), or else "other".
function Get-UiGalleryGroup {
	param([string]$Picture)
	$p = $Picture.Replace('\', '/')
	$i = $p.IndexOf('/')
	if ($i -gt 0) { return $p.Substring(0, $i) }
	$m = [regex]::Match($p, '^([A-Za-z]+)\d')
	if ($m.Success) { return $m.Groups[1].Value.ToUpperInvariant() }
	return 'other'
}

# The clip notes: a dictionary of picture (relative path or file name, any case) to its list of notes. Rows: picture, then
# the note; more fields are joined with "; ". Empty lines, lines that start with # and a header "picture ..." are skipped.
function Read-UiClipNotes {
	param([string]$Path)
	$dict = New-Object 'System.Collections.Generic.Dictionary[string,System.Collections.Generic.List[string]]' ([System.StringComparer]::OrdinalIgnoreCase)
	if (-not $Path) { return ,$dict }
	foreach ($line in [System.IO.File]::ReadAllLines($Path)) {
		if ($line.Trim() -eq '' -or $line.TrimStart().StartsWith('#')) { continue }
		$fields = $line.Split("`t")
		$picture = $fields[0].Trim().Replace('\', '/')
		if ($picture -eq 'picture' -or $fields.Count -lt 2) { continue }
		$text = (($fields | Select-Object -Skip 1) | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' }) -join '; '
		if (-not $dict.ContainsKey($picture)) { $dict[$picture] = New-Object 'System.Collections.Generic.List[string]' }
		$dict[$picture].Add($text)
	}
	return ,$dict
}

# The clip notes of one picture: those named by its relative path and those named by its file name.
function Get-UiClipNotesFor {
	param($Dict, [string]$Picture)
	$list = New-Object 'System.Collections.Generic.List[string]'
	$fileName = $Picture.Substring($Picture.LastIndexOf('/') + 1)
	foreach ($key in @($Picture, $fileName) | Select-Object -Unique) {
		if ($Dict.ContainsKey($key)) { $list.AddRange($Dict[$key]) }
	}
	return ,$list
}

function ConvertTo-UiHtml {
	param([string]$Text)
	return [System.Net.WebUtility]::HtmlEncode($Text)
}

# A percentage for people: 2 decimals, "<0.01" for a change smaller than that.
function Format-UiPercent {
	param([double]$Value)
	if ($Value -gt 0 -and $Value -lt 0.01) { return '<0.01' }
	return $Value.ToString('0.00', [System.Globalization.CultureInfo]::InvariantCulture)
}

# "1 picture has", "3 pictures have" (or another verb pair, singular and plural, like 'is,are').
function Format-UiPictures {
	param([int]$Count, [string]$Verb = 'has,have')
	$verbs = $Verb.Split(',')
	if ($Count -eq 1) { return "1 picture $($verbs[0])" }
	return "$Count pictures $($verbs[1])"
}

function Format-UiCount {
	param([long]$Value)
	return $Value.ToString('N0', [System.Globalization.CultureInfo]::InvariantCulture)
}

# True when the folder holds the marker file, or an index.html this script wrote.
function Test-UiGalleryFolder {
	param([string]$Folder)
	if (Test-Path -LiteralPath (Join-Path $Folder $script:MarkerFileName) -PathType Leaf) { return $true }
	$index = Join-Path $Folder 'index.html'
	if (-not (Test-Path -LiteralPath $index -PathType Leaf)) { return $false }
	$html = [System.IO.File]::ReadAllText($index)
	return ($html.StartsWith('<!doctype html>') -and $html.Contains('<title>UI gallery '))
}

# Makes the folder an empty gallery folder with its marker file. A folder that does not exist is made, an empty one is used,
# an earlier gallery is emptied; any other folder is refused and left as it is (so "-Out ." in a checkout deletes nothing).
function Initialize-UiGalleryFolder {
	param([string]$Folder)
	if ((Test-Path -LiteralPath $Folder -PathType Container) -and @(Get-ChildItem -LiteralPath $Folder -Force).Count -gt 0) {
		if (-not (Test-UiGalleryFolder -Folder $Folder)) {
			throw "Not a gallery folder, so it is not emptied: $Folder. It holds neither $($script:MarkerFileName) nor a gallery's index.html. Name a new or empty folder."
		}
		Remove-Item -LiteralPath $Folder -Recurse -Force
	}
	New-Item -ItemType Directory -Force -Path $Folder | Out-Null
	[System.IO.File]::WriteAllText((Join-Path $Folder $script:MarkerFileName), "A UI refresh gallery (tools\New-UiGallery.ps1). The next gallery written to this folder empties it first.`r`n", (New-Object System.Text.UTF8Encoding($false)))
}

# Zips every file under $Folder into $Zip, each entry named by its path relative to $Folder with forward slashes (the zip
# format's separator; as tools\New-ToothChartGallery.ps1 does). Compress-Archive of Windows PowerShell 5.1 writes
# backslashes, which macOS and phone unzippers take as part of a flat file name, so index.html's links would break there.
function New-UiGalleryZip {
	param([string]$Folder, [string]$Zip)
	Add-Type -AssemblyName System.IO.Compression
	Add-Type -AssemblyName System.IO.Compression.FileSystem
	$folderFull = [System.IO.Path]::GetFullPath($Folder).TrimEnd('\') + '\'
	$stream = [System.IO.File]::Open($Zip, [System.IO.FileMode]::CreateNew)
	try {
		$archive = New-Object System.IO.Compression.ZipArchive($stream, [System.IO.Compression.ZipArchiveMode]::Create)
		try {
			foreach ($file in (Get-ChildItem -LiteralPath $Folder -Recurse -File | Sort-Object FullName)) {
				$entryName = $file.FullName.Substring($folderFull.Length).Replace('\', '/')
				[void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $file.FullName, $entryName, [System.IO.Compression.CompressionLevel]::Optimal)
			}
		}
		finally { $archive.Dispose() }
	}
	finally { $stream.Dispose() }
}

# Copies a picture into the gallery under its relative path.
function Copy-UiPicture {
	param([string]$From, [string]$To)
	New-Item -ItemType Directory -Force -Path (Split-Path -Parent $To) | Out-Null
	Copy-Item -LiteralPath $From -Destination $To -Force
}

# Compares every picture of the two sets and copies them into the gallery. Returns one object per picture, the changed
# ones first (most changed first), then the pictures only one set has, then the unchanged ones.
function Get-UiGalleryEntries {
	param($DictBefore, $DictAfter, $ListMasks, $ListApproved, $DictClip, [string]$OutFolder, [int]$ThresholdValue)
	$all = New-Object 'System.Collections.Generic.SortedSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
	foreach ($k in $DictBefore.Keys) { [void]$all.Add($k) }
	foreach ($k in $DictAfter.Keys) { [void]$all.Add($k) }
	$entries = New-Object 'System.Collections.Generic.List[object]'
	foreach ($picture in $all) {
		$entry = [pscustomobject]@{
			Picture = $picture; Group = (Get-UiGalleryGroup -Picture $picture); Status = ''; Rank = 0
			BeforeWidth = 0; BeforeHeight = 0; AfterWidth = 0; AfterHeight = 0
			Changed = [long]0; Compared = [long]0; Masked = [long]0; Percent = [double]0; Bounds = ''
			ShaAfter = ''; ShaBefore = ''; HasDiff = $false
			MasksApplied = @(); MasksWrongSize = @(); Approval = 'none'; ApprovalText = ''; ClipNotes = @()
		}
		$hasBefore = $DictBefore.ContainsKey($picture)
		$hasAfter = $DictAfter.ContainsKey($picture)
		$pixelsBefore = $null
		$pixelsAfter = $null
		if ($hasBefore) {
			Copy-UiPicture -From $DictBefore[$picture] -To (Join-Path $OutFolder ('before/' + $picture))
			$pixelsBefore = [UiGalleryTools.UiPictureMask]::LoadPixels($DictBefore[$picture])
			$entry.BeforeWidth = $pixelsBefore.Width; $entry.BeforeHeight = $pixelsBefore.Height
		}
		if ($hasAfter) {
			Copy-UiPicture -From $DictAfter[$picture] -To (Join-Path $OutFolder ('after/' + $picture))
			$pixelsAfter = [UiGalleryTools.UiPictureMask]::LoadPixels($DictAfter[$picture])
			$entry.AfterWidth = $pixelsAfter.Width; $entry.AfterHeight = $pixelsAfter.Height
		}
		if ($hasBefore -and $hasAfter) {
			$pathDiff = Join-Path $OutFolder ('diff/' + $picture)
			$result = [UiGalleryTools.UiPictureMask]::Compare($pixelsBefore, $pixelsAfter, $picture, $ListMasks, $ThresholdValue, $pathDiff)
			$entry.Changed = $result.CountChanged; $entry.Compared = $result.CountCompared; $entry.Masked = $result.CountMasked
			$entry.Percent = $result.PercentChanged
			$entry.ShaAfter = $result.Sha256After; $entry.ShaBefore = $result.Sha256Before
			$entry.HasDiff = ($result.CountChanged -gt 0)
			if ($result.CountChanged -gt 0) {
				$b = $result.BoundsChanged
				$entry.Bounds = "$($b.X),$($b.Y) $($b.Width)x$($b.Height)"
				$entry.Status = 'changed'; $entry.Rank = 0
			}
			else { $entry.Status = 'same'; $entry.Rank = 3 }
		}
		elseif ($hasAfter) {
			$entry.ShaAfter = [UiGalleryTools.UiPictureMask]::GetMaskedPixelSha256($pixelsAfter, $picture, $ListMasks)
			$entry.Status = 'only after'; $entry.Rank = 1
		}
		else {
			$entry.ShaBefore = [UiGalleryTools.UiPictureMask]::GetMaskedPixelSha256($pixelsBefore, $picture, $ListMasks)
			$entry.Status = 'only before'; $entry.Rank = 2
		}
		if ($hasAfter) {
			$entry.MasksApplied = @([UiGalleryTools.UiPictureMask]::GetMasksFor($ListMasks, $picture, $entry.AfterWidth, $entry.AfterHeight))
			$entry.MasksWrongSize = @([UiGalleryTools.UiPictureMask]::GetMasksWrongSize($ListMasks, $picture, $entry.AfterWidth, $entry.AfterHeight))
			$rowApproved = [UiGalleryTools.UiPictureMask]::FindApproved($ListApproved, $picture)
			if ($null -ne $rowApproved) {
				$entry.ApprovalText = "$($rowApproved.Gallery) at $($rowApproved.Commit), $($rowApproved.Date)"
				if ([UiGalleryTools.UiPictureMask]::IsApproved($ListApproved, $picture, $entry.AfterWidth, $entry.AfterHeight, $entry.ShaAfter)) { $entry.Approval = 'same' }
				else { $entry.Approval = 'differs' }
			}
		}
		$entry.ClipNotes = (Get-UiClipNotesFor -Dict $DictClip -Picture $picture).ToArray()
		$entries.Add($entry)
	}
	return ,@($entries | Sort-Object -Property @{ Expression = 'Rank' }, @{ Expression = 'Percent'; Descending = $true }, @{ Expression = 'Picture' })
}

function Get-UiSizeText {
	param($Entry)
	$b = if ($Entry.BeforeWidth -gt 0) { "$($Entry.BeforeWidth)x$($Entry.BeforeHeight)" } else { '-' }
	$a = if ($Entry.AfterWidth -gt 0) { "$($Entry.AfterWidth)x$($Entry.AfterHeight)" } else { '-' }
	if ($b -eq $a -or $b -eq '-') { return $a }
	if ($a -eq '-') { return $b }
	return "$b to $a"
}

function Get-UiStatusText {
	param($Entry)
	switch ($Entry.Status) {
		'changed' { return "changed, $(Format-UiPercent $Entry.Percent) %" }
		'same' { return 'same' }
		'only after' { return 'new: only after' }
		default { return 'gone: only before' }
	}
}

function Get-UiApprovalHtml {
	param($Entry)
	switch ($Entry.Approval) {
		'same' { return '<span class="ok">approved: ' + (ConvertTo-UiHtml $Entry.ApprovalText) + '</span>' }
		'differs' { return '<span class="hot">changed since approval (' + (ConvertTo-UiHtml $Entry.ApprovalText) + ')</span>' }
		default { return '<span class="meta">not approved yet</span>' }
	}
}

# The Approve/Change inputs of one picture; where says which copy (the table or the picture's card), so the radio groups differ.
# data-pic is the picture's relative path, not its place on the page: the page keeps the answers by picture (with the masked SHA-256 answered),
# so a gallery rebuilt under the same name and commit in another order never moves an answer onto another picture.
function Get-UiAnswerHtml {
	param([string]$Id, [string]$Picture, [string]$Where)
	$pic = ConvertTo-UiHtml $Picture
	return '<span class="ans"><label><input type="radio" name="a-' + $Id + '-' + $Where + '" value="Approve" data-pic="' + $pic + '"> Approve</label> ' +
		'<label><input type="radio" name="a-' + $Id + '-' + $Where + '" value="Change" data-pic="' + $pic + '"> Change</label> ' +
		'<input type="text" class="note" placeholder="note" data-pic="' + $pic + '" aria-label="note"></span>'
}

function Get-UiImageHtml {
	param([string]$Caption, [string]$Path, [int]$Width, [int]$Height)
	if (-not $Path) { return '<figure><figcaption>' + (ConvertTo-UiHtml $Caption) + '</figcaption><div class="none">none</div></figure>' }
	$src = ConvertTo-UiHtml $Path
	return '<figure><figcaption>' + (ConvertTo-UiHtml $Caption) + " ($Width x $Height)</figcaption><div class=""scroll""><img class=""px"" loading=""lazy"" src=""$src"" width=""$Width"" height=""$Height"" data-w=""$Width"" data-h=""$Height"" alt=""$src"" title=""Click: this picture 2x""></div></figure>"
}

# The page. Every picture path is relative (before/, after/, diff/), so the folder and its zip stand alone.
function Get-UiGalleryHtml {
	param($Entries, [string]$GalleryName, [string]$CommitText, [string]$NoteText, [string]$BeforeName, [string]$AfterName, [int]$CountMaskRows, [int]$ThresholdValue)
	$sb = New-Object System.Text.StringBuilder
	$enc = { param($t) [System.Net.WebUtility]::HtmlEncode($t) }
	$countChanged = @($Entries | Where-Object { $_.Status -eq 'changed' }).Count
	$countSame = @($Entries | Where-Object { $_.Status -eq 'same' }).Count
	$countOnlyAfter = @($Entries | Where-Object { $_.Status -eq 'only after' }).Count
	$countOnlyBefore = @($Entries | Where-Object { $_.Status -eq 'only before' }).Count
	$countClip = @($Entries | Where-Object { $_.ClipNotes.Count -gt 0 }).Count
	$countApproved = @($Entries | Where-Object { $_.Approval -eq 'same' }).Count
	$countWrongSize = @($Entries | Where-Object { $_.MasksWrongSize.Count -gt 0 }).Count
	$made = (Get-Date).ToString('yyyy-MM-dd HH:mm', [System.Globalization.CultureInfo]::InvariantCulture)
	[void]$sb.Append("<!doctype html>`r`n<html lang=""en"">`r`n<head>`r`n<meta charset=""utf-8"">`r`n<meta name=""viewport"" content=""width=device-width, initial-scale=1"">`r`n")
	[void]$sb.Append('<title>UI gallery ' + (& $enc $GalleryName) + "</title>`r`n<style>`r`n")
	[void]$sb.Append(@'
:root{--bg:#fafaf8;--fg:#1d1d1b;--muted:#5d5d58;--line:#d8d8d2;--card:#fff;--hot:#b3261e;--ok:#2e6b30;--accent:#3c5a96}
@media (prefers-color-scheme:dark){:root{--bg:#161615;--fg:#ececea;--muted:#a5a59f;--line:#3a3a37;--card:#1f1f1d;--hot:#ff8a80;--ok:#8bc98d;--accent:#9db4e8}}
body{background:var(--bg);color:var(--fg);font:15px/1.5 system-ui,Segoe UI,sans-serif;margin:0;padding:16px}
h1{font-size:1.5em;margin:.2em 0}h2{margin-top:1.6em;border-bottom:1px solid var(--line)}h3{font-size:1em;margin:.2em 0;word-break:break-all}
a{color:var(--accent)}.meta,figcaption{color:var(--muted);font-size:.85em}.hot{color:var(--hot);font-weight:600}.ok{color:var(--ok)}
.box{background:var(--card);border:1px solid var(--line);border-radius:6px;padding:6px 14px;margin:12px 0}
.bar{position:sticky;top:0;z-index:2;background:var(--bg);border-bottom:1px solid var(--line);padding:8px 0;display:flex;flex-wrap:wrap;gap:8px 16px;align-items:center}
.bar label{white-space:nowrap}button,select,input{font:inherit}
.tablewrap{overflow-x:auto;max-width:100%}table{border-collapse:collapse}td,th{border:1px solid var(--line);padding:2px 6px;text-align:left;font-size:.9em;vertical-align:top}
td.num{text-align:right;font-variant-numeric:tabular-nums}
.pic{background:var(--card);border:1px solid var(--line);border-radius:6px;padding:10px;margin:12px 0}
.imgs{display:flex;flex-wrap:wrap;gap:12px;align-items:flex-start}.imgs figure{margin:0;max-width:100%}
.scroll{overflow:auto;max-width:100%;border:1px solid var(--line);background:#fff}
img.px{display:block;image-rendering:pixelated;image-rendering:crisp-edges;max-width:none;cursor:zoom-in}img.px.z2{cursor:zoom-out}
.none{color:var(--muted);font-size:.85em;padding:8px;border:1px dashed var(--line)}
.ans{white-space:nowrap}.ans .note{width:14em;max-width:60vw}
.sha{font-family:Consolas,monospace;font-size:.85em}
'@)
	[void]$sb.Append("</style>`r`n</head>`r`n<body>`r`n")
	[void]$sb.Append('<h1>UI gallery ' + (& $enc $GalleryName) + "</h1>`r`n")
	[void]$sb.Append('<p class="meta">Made ' + $made + $(if ($CommitText) { ' at commit ' + (& $enc $CommitText) } else { '' }) +
		'. Before: ' + (& $enc $BeforeName) + '. After: ' + (& $enc $AfterName) + ". Masks: $CountMaskRows rows (masks.tsv). A pixel counts as changed when a value differs by more than $ThresholdValue. Synthetic data only.</p>`r`n")
	if ($NoteText) { [void]$sb.Append('<p>' + (& $enc $NoteText) + "</p>`r`n") }
	[void]$sb.Append('<section class="box"><h2 style="margin-top:.4em;border:0">In plain words</h2><ul>' +
		"<li><b>$($Entries.Count) pictures</b>: $countChanged changed, $countSame the same, $countOnlyAfter only in the after set, $countOnlyBefore only in the before set.</li>" +
		'<li>Each picture is shown at its real size, one picture pixel on one screen pixel, whatever your Windows scaling, so thin lines and small icons look as they do in the program. <b>Zoom</b> enlarges every picture without blurring; a click on one picture enlarges just that one.</li>' +
		'<li>The difference picture shows changed pixels in <span class="hot">red</span> over a grey copy of the after picture, an area only one picture has in magenta, and in hatched blue the places that change by themselves from run to run (the clock, dates, the test copy''s name), which are not compared.</li>' +
		'<li>For each picture choose <b>Approve</b> or <b>Change</b> (with a note saying what to change), in the table or under the picture. Your answers stay in this browser; <b>Download answers</b> saves them as a file to send back.</li>' +
		$(if ($countClip -gt 0) { "<li class=""hot"">$(Format-UiPictures $countClip) clip notes: text that does not fit, listed under the picture.</li>" } else { '' }) +
		$(if ($countApproved -gt 0) { "<li>$(Format-UiPictures $countApproved -Verb 'is,are') exactly as you approved them before.</li>" } else { '' }) +
		$(if ($countWrongSize -gt 0) { "<li class=""hot"">$(Format-UiPictures $countWrongSize) a mask row for another size: the window changed size, so its masks need a look.</li>" } else { '' }) +
		"</ul></section>`r`n")
	$groups = @($Entries | ForEach-Object { $_.Group } | Sort-Object -Unique)
	[void]$sb.Append('<div class="bar"><label>Group <select id="fGroup"><option value="">all</option>')
	foreach ($g in $groups) { [void]$sb.Append('<option>' + (& $enc $g) + '</option>') }
	[void]$sb.Append('</select></label><label>Show <select id="fStatus"><option value="">all</option><option value="changed">changed</option><option value="new">only in one set</option><option value="same">the same</option><option value="clip">with clip notes</option><option value="unapproved">not approved yet</option><option value="unanswered">not answered</option></select></label>')
	[void]$sb.Append('<label>Name <input type="search" id="fName" size="14"></label><button type="button" id="zoom">Zoom: 1:1</button>')
	[void]$sb.Append('<button type="button" id="dl">Download answers</button><button type="button" id="cp">Copy answers</button><span class="meta" id="answered"></span></div>' + "`r`n")
	[void]$sb.Append('<p class="meta" id="dprnote">Pictures are shown at their size in CSS pixels until the page script runs.</p>' + "`r`n")
	[void]$sb.Append("<h2>Overview, the changed ones first</h2>`r`n<div class=""tablewrap""><table id=""overview""><thead><tr><th>Picture</th><th>Group</th><th>Status</th><th>% changed</th><th>Size</th><th>Clip notes</th><th>Approved before</th><th>Your answer</th></tr></thead><tbody>`r`n")
	$i = 0
	$ids = @{}
	foreach ($e in $Entries) {
		$i++
		$id = 'p' + $i
		$ids[$e.Picture] = $id
		[void]$sb.Append('<tr ' + (Get-UiDataAttributes -Entry $e -Id $id) + '><td><a href="#' + $id + '">' + (& $enc $e.Picture) + '</a></td><td>' + (& $enc $e.Group) + '</td><td>' +
			(& $enc $e.Status) + '</td><td class="num">' + $(if ($e.Status -eq 'changed' -or $e.Status -eq 'same') { Format-UiPercent $e.Percent } else { '' }) +
			'</td><td>' + (& $enc (Get-UiSizeText -Entry $e)) + '</td><td class="num">' + $(if ($e.ClipNotes.Count -gt 0) { '<span class="hot">' + $e.ClipNotes.Count + '</span>' } else { '' }) +
			'</td><td>' + (Get-UiApprovalHtml -Entry $e) + '</td><td>' + (Get-UiAnswerHtml -Id $id -Picture $e.Picture -Where 't') + "</td></tr>`r`n")
	}
	[void]$sb.Append("</tbody></table></div>`r`n")
	foreach ($g in $groups) {
		[void]$sb.Append('<div class="group" data-group="' + (& $enc $g) + '"><h2>Group ' + (& $enc $g) + "</h2>`r`n")
		foreach ($e in @($Entries | Where-Object { $_.Group -eq $g })) {
			$id = $ids[$e.Picture]
			[void]$sb.Append('<section class="pic" id="' + $id + '" ' + (Get-UiDataAttributes -Entry $e -Id $id) + '><h3><a href="#' + $id + '">' + (& $enc $e.Picture) + '</a> <span class="' +
				$(if ($e.Status -eq 'same') { 'ok' } else { 'hot' }) + '">' + (& $enc (Get-UiStatusText -Entry $e)) + "</span></h3>`r`n")
			$facts = New-Object 'System.Collections.Generic.List[string]'
			$facts.Add('Size ' + (Get-UiSizeText -Entry $e))
			if ($e.Status -eq 'changed' -or $e.Status -eq 'same') {
				$facts.Add((Format-UiCount $e.Changed) + ' of ' + (Format-UiCount $e.Compared) + ' pixels changed' + $(if ($e.Bounds) { ' (inside ' + $e.Bounds + ')' } else { '' }))
			}
			if ($e.Masked -gt 0 -or $e.MasksApplied.Count -gt 0) {
				$facts.Add('masked: ' + (($e.MasksApplied | ForEach-Object { $_.What }) -join '; '))
			}
			if ($e.ShaAfter) { $facts.Add('masked pixel SHA-256 <span class="sha" title="' + $e.ShaAfter + '">' + $e.ShaAfter.Substring(0, 12) + '</span>') }
			[void]$sb.Append('<div class="meta">' + (($facts | ForEach-Object { if ($_ -like 'masked pixel*') { $_ } else { & $enc $_ } }) -join '. ') + '. ' + (Get-UiApprovalHtml -Entry $e) + "</div>`r`n")
			foreach ($row in $e.MasksWrongSize) {
				[void]$sb.Append('<div class="hot">Mask row for another size, not applied: ' + (& $enc $row.ToString()) + "</div>`r`n")
			}
			if ($e.ClipNotes.Count -gt 0) {
				[void]$sb.Append('<div class="hot">Clip notes:</div><ul>')
				foreach ($n in $e.ClipNotes) { [void]$sb.Append('<li>' + (& $enc $n) + '</li>') }
				[void]$sb.Append("</ul>`r`n")
			}
			[void]$sb.Append('<div>Your answer: ' + (Get-UiAnswerHtml -Id $id -Picture $e.Picture -Where 'c') + "</div>`r`n")
			$pathBefore = if ($e.BeforeWidth -gt 0) { 'before/' + $e.Picture } else { '' }
			$pathAfter = if ($e.AfterWidth -gt 0) { 'after/' + $e.Picture } else { '' }
			if ($e.Status -eq 'same') {
				[void]$sb.Append('<details><summary>Show the picture (the same before and after)</summary><div class="imgs">' + (Get-UiImageHtml -Caption 'After' -Path $pathAfter -Width $e.AfterWidth -Height $e.AfterHeight) + "</div></details>`r`n")
			}
			else {
				[void]$sb.Append('<div class="imgs">' + (Get-UiImageHtml -Caption 'Before' -Path $pathBefore -Width $e.BeforeWidth -Height $e.BeforeHeight) +
					(Get-UiImageHtml -Caption 'After' -Path $pathAfter -Width $e.AfterWidth -Height $e.AfterHeight))
				if ($e.HasDiff) {
					$w = [Math]::Max($e.BeforeWidth, $e.AfterWidth); $h = [Math]::Max($e.BeforeHeight, $e.AfterHeight)
					[void]$sb.Append((Get-UiImageHtml -Caption 'Difference' -Path ('diff/' + $e.Picture) -Width $w -Height $h))
				}
				[void]$sb.Append("</div>`r`n")
			}
			[void]$sb.Append("</section>`r`n")
		}
		[void]$sb.Append("</div>`r`n")
	}
	[void]$sb.Append('<script>' + "`r`n" + 'var GALLERY=' + (ConvertTo-UiJsString $GalleryName) + ',COMMIT=' + (ConvertTo-UiJsString $CommitText) + ";`r`n")
	[void]$sb.Append(@'
(function(){
var zoom=1,zooms=[1,2,4],answers={},key='ui-gallery:'+GALLERY+':'+COMMIT;
function q(s){return Array.prototype.slice.call(document.querySelectorAll(s));}
function dpr(){return window.devicePixelRatio||1;}
//One picture pixel on one device pixel: CSS size = picture size / devicePixelRatio, times the zoom.
function size(){var r=dpr();q('img.px').forEach(function(i){var f=zoom*(i.classList.contains('z2')?2:1);i.style.width=(i.getAttribute('data-w')*f/r)+'px';i.style.height=(i.getAttribute('data-h')*f/r)+'px';});
document.getElementById('dprnote').textContent='This screen: '+Math.round(r*100)+' % scaling ('+r+' device pixels per CSS pixel). Pictures are shown '+(zoom===1?'at 1:1 device pixels':zoom+' times enlarged, without smoothing')+'.';}
function watch(){try{window.matchMedia('(resolution: '+dpr()+'dppx)').addEventListener('change',function(){size();watch();},{once:true});}catch(e){}}
document.getElementById('zoom').addEventListener('click',function(){zoom=zooms[(zooms.indexOf(zoom)+1)%zooms.length];this.textContent='Zoom: '+(zoom===1?'1:1':zoom+'x');size();});
q('img.px').forEach(function(i){i.addEventListener('click',function(){i.classList.toggle('z2');size();});});
window.addEventListener('resize',size);
function load(){try{var s=window.localStorage.getItem(key);if(s){answers=JSON.parse(s)||{};}}catch(e){answers={};}}
function save(){try{window.localStorage.setItem(key,JSON.stringify(answers));}catch(e){}}
//Answers are kept by picture (its relative path), each with the masked SHA-256 and size it was given for. An answer saved for another
//SHA-256 or size (the picture changed in a rebuilt gallery of the same name and commit) does not count: the picture shows as unanswered.
var pics={};q('section.pic').forEach(function(el){pics[el.getAttribute('data-picture')]={sha:el.getAttribute('data-sha')||'',size:el.getAttribute('data-size')||''};});
function own(o,k){return Object.prototype.hasOwnProperty.call(o,k);}
function got(p){var a=own(answers,p)?answers[p]:null,c=own(pics,p)?pics[p]:null;return (a&&c&&a.sha===c.sha&&a.size===c.size)?a:null;}
function show(){q('[data-pic]').forEach(function(el){var a=got(el.getAttribute('data-pic'))||{};if(el.type==='radio'){el.checked=(a.answer===el.value);}else if(document.activeElement!==el){el.value=a.note||'';}});
var n=0,t=q('section.pic').length;for(var k in pics){if(own(pics,k)){var a=got(k);if(a&&a.answer){n++;}}}document.getElementById('answered').textContent=n+' of '+t+' answered';}
q('[data-pic]').forEach(function(el){el.addEventListener(el.type==='radio'?'change':'input',function(){var p=el.getAttribute('data-pic'),a=got(p);
if(!a){if(!own(pics,p)){return;}a=answers[p]={sha:pics[p].sha,size:pics[p].size};}if(el.type==='radio'){a.answer=el.value;}else{a.note=el.value;}save();show();filter();});});
function filter(){var g=document.getElementById('fGroup').value,s=document.getElementById('fStatus').value,n=document.getElementById('fName').value.toLowerCase();
q('[data-picture]').forEach(function(el){var ok=(!g||el.getAttribute('data-group')===g)&&(!n||el.getAttribute('data-picture').toLowerCase().indexOf(n)>=0);
var st=el.getAttribute('data-status');if(s==='changed'){ok=ok&&st==='changed';}else if(s==='new'){ok=ok&&(st==='only after'||st==='only before');}else if(s==='same'){ok=ok&&st==='same';}
else if(s==='clip'){ok=ok&&el.getAttribute('data-clip')!=='0';}else if(s==='unapproved'){ok=ok&&el.getAttribute('data-approved')!=='same';}
else if(s==='unanswered'){var a=got(el.getAttribute('data-picture'));ok=ok&&!(a&&a.answer);}el.style.display=ok?'':'none';});
q('div.group').forEach(function(d){d.style.display=d.querySelector('section.pic:not([style*="none"])')?'':'none';});}
['fGroup','fStatus'].forEach(function(id){document.getElementById(id).addEventListener('change',filter);});document.getElementById('fName').addEventListener('input',filter);
function clean(t){return String(t||'').replace(/[\t\r\n]+/g,' ');}
function tsv(){var d=new Date(),day=d.getFullYear()+'-'+('0'+(d.getMonth()+1)).slice(-2)+'-'+('0'+d.getDate()).slice(-2);
var lines=['picture\tsize\tsha256\tgallery\tcommit\tdate\tanswer\tnote'];
q('section.pic').forEach(function(el){var a=got(el.getAttribute('data-picture'));if(!a||!(a.answer||a.note)){return;}
lines.push([el.getAttribute('data-picture'),el.getAttribute('data-size'),el.getAttribute('data-sha'),GALLERY,COMMIT,day,clean(a.answer),clean(a.note)].join('\t'));});
return lines.join('\r\n')+'\r\n';}
document.getElementById('dl').addEventListener('click',function(){var b=new Blob([tsv()],{type:'text/tab-separated-values'}),u=URL.createObjectURL(b),a=document.createElement('a');
a.href=u;a.download='answers-'+GALLERY+'.tsv';document.body.appendChild(a);a.click();a.remove();setTimeout(function(){URL.revokeObjectURL(u);},1000);});
document.getElementById('cp').addEventListener('click',function(){var t=tsv(),b=this;try{navigator.clipboard.writeText(t).then(function(){b.textContent='Copied';},function(){window.prompt('Copy the answers:',t);});}catch(e){window.prompt('Copy the answers:',t);}});
load();show();size();watch();filter();
})();
'@)
	[void]$sb.Append("</script>`r`n</body>`r`n</html>`r`n")
	return $sb.ToString()
}

# The data attributes the page's filters and the answers file read.
function Get-UiDataAttributes {
	param($Entry, [string]$Id)
	$size = if ($Entry.AfterWidth -gt 0) { "$($Entry.AfterWidth)x$($Entry.AfterHeight)" } else { "$($Entry.BeforeWidth)x$($Entry.BeforeHeight)" }
	$sha = if ($Entry.ShaAfter) { $Entry.ShaAfter } else { $Entry.ShaBefore }
	return 'data-id="' + $Id + '" data-picture="' + (ConvertTo-UiHtml $Entry.Picture) + '" data-group="' + (ConvertTo-UiHtml $Entry.Group) + '" data-status="' + $Entry.Status +
		'" data-clip="' + $Entry.ClipNotes.Count + '" data-approved="' + $Entry.Approval + '" data-size="' + $size + '" data-sha="' + $sha + '"'
}

# A JavaScript string literal, ASCII only and safe inside a script element.
function ConvertTo-UiJsString {
	param([string]$Text)
	$sb = New-Object System.Text.StringBuilder
	[void]$sb.Append("'")
	foreach ($c in $Text.ToCharArray()) {
		$code = [int]$c
		if ($code -lt 32 -or $code -gt 126 -or $c -eq "'" -or $c -eq '\' -or $c -eq '<' -or $c -eq '>' -or $c -eq '&') { [void]$sb.Append('\u' + $code.ToString('x4')) }
		else { [void]$sb.Append($c) }
	}
	[void]$sb.Append("'")
	return $sb.ToString()
}

# gallery.tsv: one row per picture, in the page's order.
function Get-UiGalleryTsv {
	param($Entries)
	$inv = [System.Globalization.CultureInfo]::InvariantCulture
	$lines = New-Object 'System.Collections.Generic.List[string]'
	$lines.Add("picture`tgroup`tstatus`tbefore_size`tafter_size`tchanged`tcompared`tmasked`tpercent`tsha256_after`tsha256_before`tapproved`tclip_notes")
	foreach ($e in $Entries) {
		$sizeBefore = if ($e.BeforeWidth -gt 0) { "$($e.BeforeWidth)x$($e.BeforeHeight)" } else { '' }
		$sizeAfter = if ($e.AfterWidth -gt 0) { "$($e.AfterWidth)x$($e.AfterHeight)" } else { '' }
		$lines.Add(($e.Picture, $e.Group, $e.Status, $sizeBefore, $sizeAfter, $e.Changed, $e.Compared, $e.Masked, $e.Percent.ToString('0.0000', $inv), $e.ShaAfter, $e.ShaBefore, $e.Approval, $e.ClipNotes.Count) -join "`t")
	}
	return ($lines -join "`r`n") + "`r`n"
}

# Dot-sourced by the tests: the functions above only.
if ($MyInvocation.InvocationName -eq '.') { return }

try {
	if (-not $Root) { $Root = Split-Path -Parent $PSScriptRoot }
	$Root = (Resolve-Path -LiteralPath $Root).Path
	if (-not $After) { throw 'Name the folder of after pictures: -After <folder>.' }
	if (-not (Test-Path -LiteralPath $After -PathType Container)) { throw "No such folder: $After" }
	$afterFull = [System.IO.Path]::GetFullPath($After).TrimEnd('\')
	$beforeFull = ''
	if ($Before) {
		if (-not (Test-Path -LiteralPath $Before -PathType Container)) { throw "No such folder: $Before" }
		$beforeFull = [System.IO.Path]::GetFullPath($Before).TrimEnd('\')
	}
	if (-not $Out) { $Out = Join-Path $env:TEMP ('UiGallery-' + $Name) }
	$outFull = [System.IO.Path]::GetFullPath($Out).TrimEnd('\')
	foreach ($source in @($afterFull, $beforeFull) | Where-Object { $_ }) {
		if ($outFull -eq $source -or $outFull.StartsWith($source + '\', [System.StringComparison]::OrdinalIgnoreCase) -or $source.StartsWith($outFull + '\', [System.StringComparison]::OrdinalIgnoreCase)) {
			throw "The gallery folder $outFull and the picture folder $source overlap. Name another -Out."
		}
	}
	if (-not $Masks) { $Masks = Join-Path $PSScriptRoot 'ui-gallery-masks.tsv' }
	if (-not (Test-Path -LiteralPath $Masks -PathType Leaf)) { throw "No mask file: $Masks" }
	if (-not $Approved) {
		$candidate = Join-Path $Root 'tests\WindowChecksTests\UiApproved\approved.tsv'
		if (Test-Path -LiteralPath $candidate -PathType Leaf) { $Approved = $candidate }
	}
	if (-not $ClipNotes) {
		$candidate = Join-Path $afterFull 'clip-notes.tsv'
		if (Test-Path -LiteralPath $candidate -PathType Leaf) { $ClipNotes = $candidate }
	}
	elseif (-not (Test-Path -LiteralPath $ClipNotes -PathType Leaf)) { throw "No clip notes file: $ClipNotes" }
	if (-not $Commit) {
		try { $Commit = [string](& git -C $Root rev-parse --short HEAD 2>$null) } catch { $Commit = '' }
	}
	Import-UiPictureMask
	$listMasks = [UiGalleryTools.UiPictureMask]::ReadMasks($Masks)
	$listApproved = New-Object 'System.Collections.Generic.List[UiGalleryTools.UiApprovedRow]'
	if ($Approved) { $listApproved = [UiGalleryTools.UiPictureMask]::ReadApproved($Approved) }
	$dictClip = Read-UiClipNotes -Path $ClipNotes
	$dictBefore = Get-UiPictures -Folder $beforeFull
	$dictAfter = Get-UiPictures -Folder $afterFull
	if ($dictBefore.Count + $dictAfter.Count -eq 0) { throw 'No *.png pictures in the before and after folders.' }
	Initialize-UiGalleryFolder -Folder $outFull
	$entries = Get-UiGalleryEntries -DictBefore $dictBefore -DictAfter $dictAfter -ListMasks $listMasks -ListApproved $listApproved -DictClip $dictClip -OutFolder $outFull -ThresholdValue $Threshold
	$utf8 = New-Object System.Text.UTF8Encoding($false)
	Copy-Item -LiteralPath $Masks -Destination (Join-Path $outFull 'masks.tsv')
	[System.IO.File]::WriteAllText((Join-Path $outFull 'gallery.tsv'), (Get-UiGalleryTsv -Entries $entries), $utf8)
	$beforeName = if ($beforeFull) { Split-Path -Leaf $beforeFull } else { '(none)' }
	$html = Get-UiGalleryHtml -Entries $entries -GalleryName $Name -CommitText $Commit -NoteText $Note -BeforeName $beforeName -AfterName (Split-Path -Leaf $afterFull) -CountMaskRows $listMasks.Count -ThresholdValue $Threshold
	[System.IO.File]::WriteAllText((Join-Path $outFull 'index.html'), $html, $utf8)
	$countChanged = @($entries | Where-Object { $_.Status -eq 'changed' }).Count
	$countSame = @($entries | Where-Object { $_.Status -eq 'same' }).Count
	$countOnly = $entries.Count - $countChanged - $countSame
	$zipText = ''
	if (-not $NoZip) {
		$zip = $outFull + '.zip'
		if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
		New-UiGalleryZip -Folder $outFull -Zip $zip
		$zipText = "; zipped to $zip"
	}
	Write-Output ("Gallery {0}: {1} pictures, {2} changed, {3} the same, {4} only in one set; {5}{6}." -f $Name, $entries.Count, $countChanged, $countSame, $countOnly, (Join-Path $outFull 'index.html'), $zipText)
	exit 0
}
catch {
	Write-Output ("New-UiGallery: " + $_.Exception.Message)
	exit 1
}
