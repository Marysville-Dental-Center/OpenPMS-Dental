<#
.SYNOPSIS
Finds the bitmap pictures of a set of windows and turns them into icons (package UI-9a of the UI refresh): inventory, check and apply.

.DESCRIPTION
Package UI-9a of docs/specs/ui-refresh-plan.md (section 7's UI-9a row, sections 3.5, 4.3 and 4.5; docs/port-notes/UI-9a.md).
One tool for every UI-9a slice. A slice is a row of icons/picture-slices.tsv: a folder and the file name patterns of its files
(UI-9a-1: OpenPmsDental/Forms, FormA*, FormB*, FormC*).

1. Inventory. In the slice's .cs and .resx files it finds every picture use:
   Resources     a "Properties.Resources.<name>" picture assigned to a control's Image or BackgroundImage (designer or code);
   ResourcesCode any other use of a "Properties.Resources.<name>" picture (code that draws or passes the picture);
   Resx          a designer "Image = ((System.Drawing.Image)(resources.GetObject("<name>")))" (or BackgroundImage);
   ImageList     a designer ImageList stream (resources.GetObject("<name>.ImageStream"), package UI-9b);
   ResxOnly      a picture, icon or ImageList entry of the slice's .resx files that no designer line reads (the window icons Icon1);
   Converted     a line this tool wrote earlier (in icons/picture-converted.tsv): reported so the map test sees it.
   For each: the file, line, control, the control's type (from "this.<control> = new <type>()"), the property, the picture's
   pixel size (read from the image: the Resources file or the .resx bytes), the map key and the map's target.
   Map keys: "Resources:<name>" for a Properties.Resources picture (one row covers every window that uses it, section 4.3: one row
   per purpose), "<resx file name>:<entry name>" for a per-window .resx picture (FormApptEdit:butPin.Image). A key may hold "*"
   (PowerShell -like), for example "*:Icon1"; an exact key wins over a pattern.

2. The map (icons/picture-map.tsv): key, target, note and an optional size (<width>x<height>): the bitmap's size when it is not the
   picture's (a 16 x 14 print picture drawn as a 16 x 16 icon; keep the width, where a button's text starts). Target is one of
   EnumIcons.<member>   the WinForms OpenPmsDental.UI.EnumIcons member (icons/manifest.json row of enum EnumIcons);
   DentalIcon.<member>  a custom dental icon of CodeBase.DentalIcon (UI-8);
   keep                 stays a picture; the note gives section 4.5's reason (logo, office content, the tooth chart, window icon);
   UI-9b                an ImageList: package UI-9b converts it.

3. Apply (-Apply). For each Resources or Resx use whose target is an icon:
   - an OpenPmsDental.UI.Button whose picture is 22 x 22 (the slot UI.Button draws its Icon in, Button.DrawTextAndImage) gets
     "this.<control>.Icon = OpenPmsDental.UI.EnumIcons.<member>;" in place of the Image line: the vector icon at every scale;
   - any other control, or a picture of another size, keeps its Image line with the picture made by
     "OpenPmsDental.UI.IconLibrary.GetBitmap(<icon>,<width>,<height>)": the icon at the picture's 96 DPI pixel size, centred,
     so nothing moves (section 3.5: the largest design that fits, never stretched up);
   - the per-window .resx entry of a converted Resx use is removed when no designer line reads it any more;
   - a Properties.Resources picture whose last use in the repository the slice replaced is removed: its Resources.resx entry,
     its property in Resources.Designer.cs and its image file under the project's Resources folder (when no other entry or
     project file names it).
   Each conversion is recorded in icons/picture-converted.tsv (the slice's rows are replaced; other slices' are kept).
   ResourcesCode uses with an icon target are reported for a hand edit, never changed.
   Files keep their bytes but the changed lines: the BOM and every line's end are kept; a file whose bytes do not survive a
   UTF-8 round trip is refused.

-Check writes nothing: it reports what -Apply would change and exits 1 when a use has no map row, when a map row names an
unknown member, or when something would change. Without -Apply or -Check the tool prints the inventory (and writes it with
-Inventory) and exits 1 when a use has no map row.

ASCII only. Windows PowerShell 5.1 or later. Needs no program, no database and no network.

.PARAMETER Root
The repository root. Default: the folder above this script's folder.
.PARAMETER Slice
The slice (a row of icons/picture-slices.tsv), for example UI-9a-1.
.PARAMETER Folder
Instead of -Slice: the folder (relative to -Root) to scan.
.PARAMETER Include
With -Folder: the file name patterns (default *).
.PARAMETER Map
The map. Default: icons/picture-map.tsv.
.PARAMETER Record
The record of conversions. Default: icons/picture-converted.tsv. With -Check, the planned conversions are written here
(a different path, so the gallery's before run knows the controls before anything changed).
.PARAMETER Inventory
Writes the inventory to this TSV.
.PARAMETER Apply
Applies the map.
.PARAMETER Check
Reports what -Apply would change; writes nothing but -Inventory and a -Record path that is not the default.

.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Convert-PictureUses.ps1 -Slice UI-9a-1 -Inventory $env:TEMP\ui9a1.tsv
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Convert-PictureUses.ps1 -Slice UI-9a-1 -Check
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Convert-PictureUses.ps1 -Slice UI-9a-1 -Apply
#>
[CmdletBinding()]
param(
	[string]$Root='',
	[string]$Slice='',
	[string]$Folder='',
	[string[]]$Include=@('*'),
	[string]$Map='',
	[string]$Record='',
	[string]$Inventory='',
	[switch]$Apply,
	[switch]$Check
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version 2
Add-Type -AssemblyName System.Drawing

if($Root -eq ''){
	$Root=Split-Path -Parent $PSScriptRoot
}
$Root=(Resolve-Path $Root).Path
if($Map -eq ''){
	$Map=Join-Path $Root 'icons\picture-map.tsv'
}
$recordDefault=Join-Path $Root 'icons\picture-converted.tsv'
if($Record -eq ''){
	$Record=$recordDefault
}
$isRecordDefault=([System.IO.Path]::GetFullPath($Record) -eq [System.IO.Path]::GetFullPath($recordDefault))
if($Apply -and $Check){
	throw 'Convert-PictureUses: -Apply and -Check exclude each other.'
}

#The UI.Button slot: Button.DrawTextAndImage draws an Icon at 22 x 22 (scaled).
$ButtonIconSize=22
$ResourcesProject='OpenPmsDental'
$ResourcesResxRel='OpenPmsDental\Properties\Resources.resx'
$ResourcesDesignerRel='OpenPmsDental\Properties\Resources.Designer.cs'

#region Files that keep their bytes
$script:Utf8=New-Object System.Text.UTF8Encoding($false,$false)

#Reads a file as lines with their ends. Returns an object: Path, Bom, Lines (content), Ends (terminators), Ok (round trip).
function Read-TextFile([string]$path){
	$bytes=[System.IO.File]::ReadAllBytes($path)
	$bom=($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
	$offset=0
	if($bom){
		$offset=3
	}
	$text=$script:Utf8.GetString($bytes,$offset,$bytes.Length-$offset)
	$bytesBack=$script:Utf8.GetBytes($text)
	$ok=($bytesBack.Length -eq ($bytes.Length-$offset))
	if($ok){
		for($i=0;$i -lt $bytesBack.Length;$i++){
			if($bytesBack[$i] -ne $bytes[$i+$offset]){
				$ok=$false
				break
			}
		}
	}
	$lines=New-Object System.Collections.Generic.List[string]
	$ends=New-Object System.Collections.Generic.List[string]
	$pos=0
	while($pos -lt $text.Length){
		$idx=$text.IndexOf("`n",$pos)
		if($idx -lt 0){
			$lines.Add($text.Substring($pos))
			$ends.Add('')
			break
		}
		$end="`n"
		$contentEnd=$idx
		if($idx -gt $pos -and $text[$idx-1] -eq "`r"){
			$end="`r`n"
			$contentEnd=$idx-1
		}
		$lines.Add($text.Substring($pos,$contentEnd-$pos))
		$ends.Add($end)
		$pos=$idx+1
	}
	return New-Object PSObject -Property @{ Path=$path; Bom=$bom; Lines=$lines; Ends=$ends; Ok=$ok; Changed=$false }
}

function Write-TextFile($file){
	if(-not $file.Ok){
		throw "Convert-PictureUses: $($file.Path) does not survive a UTF-8 round trip; edit it by hand."
	}
	$sb=New-Object System.Text.StringBuilder
	for($i=0;$i -lt $file.Lines.Count;$i++){
		[void]$sb.Append($file.Lines[$i])
		[void]$sb.Append($file.Ends[$i])
	}
	$bytes=$script:Utf8.GetBytes($sb.ToString())
	if($file.Bom){
		$bytes=[byte[]](@(0xEF,0xBB,0xBF)+$bytes)
	}
	[System.IO.File]::WriteAllBytes($file.Path,$bytes)
}

#Removes lines [start,end] inclusive.
function Remove-Lines($file,[int]$start,[int]$end){
	$count=$end-$start+1
	$file.Lines.RemoveRange($start,$count)
	$file.Ends.RemoveRange($start,$count)
	$file.Changed=$true
}
#endregion

#region Slices, map and record
function Read-Tsv([string]$path){
	$rows=@()
	if(-not (Test-Path -LiteralPath $path)){
		return ,$rows
	}
	foreach($line in [System.IO.File]::ReadAllLines($path)){
		if($line.Trim() -eq '' -or $line.StartsWith('#')){
			continue
		}
		$rows+=,($line.Split("`t"))
	}
	#The comma keeps a single row an array of rows.
	return ,$rows
}

$folderScan=''
$patterns=$Include
if($Slice -ne ''){
	$rowSlice=$null
	foreach($row in (Read-Tsv (Join-Path $Root 'icons\picture-slices.tsv'))){
		if($row[0] -eq $Slice){
			$rowSlice=$row
		}
	}
	if($null -eq $rowSlice){
		throw "Convert-PictureUses: no row for slice $Slice in icons\picture-slices.tsv."
	}
	$folderScan=$rowSlice[1]
	$patterns=@($rowSlice[2].Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' })
}
elseif($Folder -ne ''){
	$folderScan=$Folder
	if($Apply){
		throw 'Convert-PictureUses: -Apply needs -Slice (the record is kept per slice).'
	}
}
else{
	throw 'Convert-PictureUses: give -Slice or -Folder.'
}
$folderScanFull=Join-Path $Root $folderScan
if(-not (Test-Path -LiteralPath $folderScanFull)){
	throw "Convert-PictureUses: no folder $folderScanFull."
}

$script:MapExact=@{}
$script:MapPatterns=New-Object System.Collections.Generic.List[object]
foreach($row in (Read-Tsv $Map)){
	if($row.Length -lt 2){
		throw "Convert-PictureUses: a map row needs a key and a target: $($row -join ' | ')"
	}
	$entry=New-Object PSObject -Property @{ Key=$row[0]; Target=$row[1]; Note=$(if($row.Length -ge 3){$row[2]}else{''}); Size=$(if($row.Length -ge 4){$row[3].Trim()}else{''}) }
	if($entry.Size -ne '' -and $entry.Size -notmatch '^\d+x\d+$'){
		throw "Convert-PictureUses: the map row $($entry.Key) has the size '$($entry.Size)' (<width>x<height> or empty)."
	}
	if($entry.Target -eq 'keep' -and $entry.Note.Trim() -eq ''){
		throw "Convert-PictureUses: the keep row $($entry.Key) gives no reason (section 4.5)."
	}
	if($entry.Key.Contains('*')){
		$script:MapPatterns.Add($entry)
	}
	else{
		if($script:MapExact.ContainsKey($entry.Key)){
			throw "Convert-PictureUses: the map has two rows for $($entry.Key)."
		}
		$script:MapExact[$entry.Key]=$entry
	}
}

function Get-MapEntry([string]$key){
	if($script:MapExact.ContainsKey($key)){
		return $script:MapExact[$key]
	}
	foreach($entry in $script:MapPatterns){
		if($key -like $entry.Key){
			return $entry
		}
	}
	return $null
}

#The members an icon target may name: the WinForms EnumIcons enum and the DentalIcon rows of the manifest.
function Get-EnumMembers([string]$path,[string]$enumName){
	$text=[System.IO.File]::ReadAllText($path)
	$m=[regex]::Match($text,'enum\s+'+$enumName+'\s*\{(?<body>[^}]*)\}')
	if(-not $m.Success){
		throw "Convert-PictureUses: no enum $enumName in $path."
	}
	$set=@{}
	$body=[regex]::Replace($m.Groups['body'].Value,'//[^\n]*','')
	foreach($part in $body.Split(',')){
		$name=$part.Trim()
		if($name -match '^(\w+)'){
			$set[$Matches[1]]=$true
		}
	}
	return $set
}
$script:EnumIconsMembers=Get-EnumMembers (Join-Path $Root 'OpenPmsDental\UI\IconLibrary\IconLibrary.cs') 'EnumIcons'
$script:DentalIconMembers=Get-EnumMembers (Join-Path $Root 'CodeBase\UI\Icons\IconData.g.cs') 'DentalIcon'

#Returns the C# expression of the target's icon, or throws for a target that names no member.
function Get-IconExpression([string]$target){
	if($target -match '^EnumIcons\.(\w+)$'){
		if(-not $script:EnumIconsMembers.ContainsKey($Matches[1])){
			throw "Convert-PictureUses: the map names EnumIcons.$($Matches[1]), which OpenPmsDental.UI.EnumIcons does not have."
		}
		return 'OpenPmsDental.UI.EnumIcons.'+$Matches[1]
	}
	if($target -match '^DentalIcon\.(\w+)$'){
		if(-not $script:DentalIconMembers.ContainsKey($Matches[1])){
			throw "Convert-PictureUses: the map names DentalIcon.$($Matches[1]), which CodeBase.DentalIcon does not have."
		}
		return 'CodeBase.DentalIcon.'+$Matches[1]
	}
	return $null
}
$allEntries=New-Object System.Collections.Generic.List[object]
foreach($entry in $script:MapExact.Values){
	$allEntries.Add($entry)
}
foreach($entry in $script:MapPatterns){
	$allEntries.Add($entry)
}
foreach($entry in $allEntries){
	if($entry.Target -ne 'keep' -and $entry.Target -ne 'UI-9b'){
		if($null -eq (Get-IconExpression $entry.Target)){
			throw "Convert-PictureUses: the map row $($entry.Key) has the target '$($entry.Target)' (EnumIcons.<member>, DentalIcon.<member>, keep or UI-9b)."
		}
	}
}

$script:RecordRows=New-Object System.Collections.Generic.List[object]
$RecordHeader="slice`tfile`tcontrol`tproperty`tkey`ttarget`thow`twidth`theight"
foreach($row in (Read-Tsv $recordDefault)){
	if($row[0] -eq 'slice'){
		continue
	}
	$script:RecordRows.Add((New-Object PSObject -Property @{ Slice=$row[0]; File=$row[1]; Control=$row[2]; Property=$row[3]; Key=$row[4];
		Target=$row[5]; How=$row[6]; Width=[int]$row[7]; Height=[int]$row[8] }))
}
#endregion

#region Picture sizes
$script:SizeCache=@{}
function Get-ImageSize([byte[]]$bytes){
	$ms=New-Object System.IO.MemoryStream(,$bytes)
	try{
		$img=[System.Drawing.Image]::FromStream($ms)
		try{
			return "$($img.Width)x$($img.Height)"
		}
		finally{
			$img.Dispose()
		}
	}
	catch{
		return ''
	}
	finally{
		$ms.Dispose()
	}
}

#The data entries of a .resx: name -> object with Type, Mime, Value (the joined value text), Start and End (line indexes).
function Get-ResxEntries($file){
	$entries=[ordered]@{}
	$isInComment=$false
	for($i=0;$i -lt $file.Lines.Count;$i++){
		$line=$file.Lines[$i]
		#The resx header comment shows sample entries (Icon1, Bitmap1, Color1): not data.
		if($isInComment){
			if($line.Contains('-->')){
				$isInComment=$false
			}
			continue
		}
		if($line.Contains('<!--')){
			if(-not $line.Substring($line.IndexOf('<!--')).Contains('-->')){
				$isInComment=$true
			}
			continue
		}
		$m=[regex]::Match($line,'^\s*<data name="(?<name>[^"]+)"(?<attrs>[^>]*?)(?<selfclose>/?)>')
		if(-not $m.Success){
			continue
		}
		$attrs=$m.Groups['attrs'].Value
		$type=''
		$mime=''
		$mt=[regex]::Match($attrs,'type="(?<t>[^"]*)"')
		if($mt.Success){
			$type=$mt.Groups['t'].Value
		}
		$mm=[regex]::Match($attrs,'mimetype="(?<t>[^"]*)"')
		if($mm.Success){
			$mime=$mm.Groups['t'].Value
		}
		$start=$i
		$end=$i
		if($m.Groups['selfclose'].Value -ne '/'){
			while($end -lt $file.Lines.Count -and $file.Lines[$end] -notmatch '</data>'){
				$end++
			}
		}
		$sb=New-Object System.Text.StringBuilder
		for($j=$start;$j -le $end;$j++){
			[void]$sb.Append($file.Lines[$j])
			[void]$sb.Append("`n")
		}
		$value=''
		$mv=[regex]::Match($sb.ToString(),'<value>(?<v>[\s\S]*?)</value>')
		if($mv.Success){
			$value=$mv.Groups['v'].Value
		}
		$entries[$m.Groups['name'].Value]=New-Object PSObject -Property @{ Name=$m.Groups['name'].Value; Type=$type; Mime=$mime; Value=$value; Start=$start; End=$end }
		$i=$end
	}
	return $entries
}

function Get-EntryKind($entry){
	if($entry.Type -like 'System.Drawing.Bitmap*'){ return 'Bitmap' }
	if($entry.Type -like 'System.Drawing.Icon*'){ return 'Icon' }
	if($entry.Type -like 'System.Windows.Forms.ImageListStreamer*'){ return 'ImageList' }
	if($entry.Type -like 'System.Resources.ResXFileRef*' -and $entry.Value -match ';System\.Drawing\.(Bitmap|Icon)'){ return 'FileRef' }
	return ''
}

function Get-ResxEntrySize($entry,[string]$folderResx){
	$kind=Get-EntryKind $entry
	if($kind -eq 'Bitmap' -or $kind -eq 'Icon'){
		$b64=($entry.Value -replace '\s','')
		try{
			$bytes=[Convert]::FromBase64String($b64)
		}
		catch{
			return ''
		}
		if($kind -eq 'Icon'){
			try{
				$ms=New-Object System.IO.MemoryStream(,$bytes)
				$icon=New-Object System.Drawing.Icon($ms)
				$s="$($icon.Width)x$($icon.Height)"
				$icon.Dispose()
				$ms.Dispose()
				return $s
			}
			catch{
				return ''
			}
		}
		return Get-ImageSize $bytes
	}
	if($kind -eq 'FileRef'){
		$pathRel=$entry.Value.Split(';')[0].Trim()
		$path=[System.IO.Path]::GetFullPath((Join-Path $folderResx $pathRel))
		if(Test-Path -LiteralPath $path){
			return Get-ImageSize ([System.IO.File]::ReadAllBytes($path))
		}
	}
	return ''
}

$script:ResourcesResxPath=Join-Path $Root $ResourcesResxRel
$script:ResourcesResx=Read-TextFile $script:ResourcesResxPath
$script:ResourcesEntries=Get-ResxEntries $script:ResourcesResx
function Get-ResourceSize([string]$name){
	if(-not $script:ResourcesEntries.Contains($name)){
		return ''
	}
	if(-not $script:SizeCache.ContainsKey($name)){
		$script:SizeCache[$name]=Get-ResxEntrySize $script:ResourcesEntries[$name] (Split-Path -Parent $script:ResourcesResxPath)
	}
	return $script:SizeCache[$name]
}
#endregion

#region Inventory
$RegexAssignResources=New-Object System.Text.RegularExpressions.Regex('^(?<indent>\s*)(?<lhs>(?:this\.)?(?<ctl>\w+)\.(?<prop>Image|BackgroundImage))\s*=\s*(?<expr>(?:global::)?(?:OpenPmsDental\.)?Properties\.Resources\.(?<res>\w+))\s*;(?<rest>.*)$')
$RegexAssignResx=New-Object System.Text.RegularExpressions.Regex('^(?<indent>\s*)(?<lhs>(?:this\.)?(?<ctl>\w+)\.(?<prop>Image|BackgroundImage))\s*=\s*(?<expr>\(\(System\.Drawing\.Image\)\(resources\.GetObject\("(?<resx>[^"]+)"\)\)\))\s*;(?<rest>.*)$')
$RegexImageList=New-Object System.Text.RegularExpressions.Regex('^\s*(?:this\.)?(?<ctl>\w+)\.ImageStream\s*=.*resources\.GetObject\("(?<resx>[^"]+)"\)')
$RegexGetObject=New-Object System.Text.RegularExpressions.Regex('resources\.GetObject\("(?<resx>[^"]+)"\)')
$RegexResourcesAny=New-Object System.Text.RegularExpressions.Regex('(?<![\w.])(?:global::)?(?:OpenPmsDental\.)?Properties\.Resources\.(?<res>\w+)')
$RegexNew=New-Object System.Text.RegularExpressions.Regex('^\s*this\.(?<ctl>\w+)\s*=\s*new\s+(?:global::)?(?<type>[\w.]+)\s*\(')
$RegexIconLine=New-Object System.Text.RegularExpressions.Regex('^\s*(?:this\.)?(?<ctl>\w+)\.Icon\s*=\s*OpenPmsDental\.UI\.EnumIcons\.(?<member>\w+)\s*;')
$RegexGetBitmapLine=New-Object System.Text.RegularExpressions.Regex('^\s*(?:this\.)?(?<ctl>\w+)\.(?<prop>Image|BackgroundImage)\s*=\s*OpenPmsDental\.UI\.IconLibrary\.GetBitmap\(')

function Test-PatternMatch([string]$name){
	foreach($p in $patterns){
		if($name -like $p){
			return $true
		}
	}
	return $false
}

function Get-RelPath([string]$path){
	return $path.Substring($Root.Length).TrimStart('\').Replace('\','/')
}

#The resx file a designer's "resources" reads: the .resx beside it named like the file up to its first dot.
function Get-ResxPathFor([string]$pathCs){
	$name=[System.IO.Path]::GetFileName($pathCs)
	$base=$name.Split('.')[0]
	return Join-Path (Split-Path -Parent $pathCs) ($base+'.resx')
}

$files=@(Get-ChildItem -LiteralPath $folderScanFull -File | Where-Object { ($_.Extension -eq '.cs' -or $_.Extension -eq '.resx') -and (Test-PatternMatch $_.Name) } | Sort-Object Name)
$uses=New-Object System.Collections.Generic.List[object]
$script:FileCache=@{}
function Get-File([string]$path){
	if(-not $script:FileCache.ContainsKey($path)){
		$script:FileCache[$path]=Read-TextFile $path
	}
	return $script:FileCache[$path]
}

function New-Use($kind,$file,$line,$ctl,$type,$prop,$size,$key){
	$entry=Get-MapEntry $key
	$target=''
	$sizeMap=''
	if($null -ne $entry){
		$target=$entry.Target
		$sizeMap=$entry.Size
	}
	$w=0
	$h=0
	if($size -match '^(\d+)x(\d+)$'){
		$w=[int]$Matches[1]
		$h=[int]$Matches[2]
	}
	return New-Object PSObject -Property @{ Kind=$kind; File=$file; Line=$line; Control=$ctl; ControlType=$type; Property=$prop; Width=$w; Height=$h;
		Key=$key; Target=$target; How=''; SizeMap=$sizeMap }
}

#Every resx entry a designer reads, per resx path.
$resxRead=@{}
foreach($fi in $files | Where-Object { $_.Extension -eq '.cs' }){
	$file=Get-File $fi.FullName
	$rel=Get-RelPath $fi.FullName
	$resxPath=Get-ResxPathFor $fi.FullName
	$resxBase=[System.IO.Path]::GetFileNameWithoutExtension($resxPath)
	$types=@{}
	foreach($line in $file.Lines){
		$m=$RegexNew.Match($line)
		if($m.Success){
			$types[$m.Groups['ctl'].Value]=$m.Groups['type'].Value
		}
	}
	for($i=0;$i -lt $file.Lines.Count;$i++){
		$line=$file.Lines[$i]
		$trim=$line.TrimStart()
		if($trim.StartsWith('//')){
			continue
		}
		foreach($mo in $RegexGetObject.Matches($line)){
			if(-not $resxRead.ContainsKey($resxPath)){
				$resxRead[$resxPath]=@{}
			}
			$resxRead[$resxPath][$mo.Groups['resx'].Value]=$true
		}
		$ctlType=''
		$m=$RegexAssignResources.Match($line)
		if($m.Success){
			$ctl=$m.Groups['ctl'].Value
			if($types.ContainsKey($ctl)){ $ctlType=$types[$ctl] }
			$res=$m.Groups['res'].Value
			$uses.Add((New-Use 'Resources' $rel ($i+1) $ctl $ctlType $m.Groups['prop'].Value (Get-ResourceSize $res) ('Resources:'+$res)))
			continue
		}
		$m=$RegexAssignResx.Match($line)
		if($m.Success){
			$ctl=$m.Groups['ctl'].Value
			if($types.ContainsKey($ctl)){ $ctlType=$types[$ctl] }
			$name=$m.Groups['resx'].Value
			$size=''
			if(Test-Path -LiteralPath $resxPath){
				$entries=Get-ResxEntries (Get-File $resxPath)
				if($entries.Contains($name)){
					$size=Get-ResxEntrySize $entries[$name] (Split-Path -Parent $resxPath)
				}
			}
			$uses.Add((New-Use 'Resx' $rel ($i+1) $ctl $ctlType $m.Groups['prop'].Value $size ($resxBase+':'+$name)))
			continue
		}
		$m=$RegexImageList.Match($line)
		if($m.Success){
			$ctl=$m.Groups['ctl'].Value
			if($types.ContainsKey($ctl)){ $ctlType=$types[$ctl] }
			$uses.Add((New-Use 'ImageList' $rel ($i+1) $ctl $ctlType 'ImageStream' '' ($resxBase+':'+$m.Groups['resx'].Value)))
			continue
		}
		foreach($mr in $RegexResourcesAny.Matches($line)){
			$res=$mr.Groups['res'].Value
			if(-not $script:ResourcesEntries.Contains($res)){
				continue
			}
			$kindEntry=Get-EntryKind $script:ResourcesEntries[$res]
			if($kindEntry -eq ''){
				continue
			}
			$uses.Add((New-Use 'ResourcesCode' $rel ($i+1) '' '' '' (Get-ResourceSize $res) ('Resources:'+$res)))
		}
	}
}
foreach($fi in $files | Where-Object { $_.Extension -eq '.resx' }){
	$file=Get-File $fi.FullName
	$rel=Get-RelPath $fi.FullName
	$base=[System.IO.Path]::GetFileNameWithoutExtension($fi.FullName)
	$entries=Get-ResxEntries $file
	foreach($entry in $entries.Values){
		$kind=Get-EntryKind $entry
		if($kind -eq ''){
			continue
		}
		if($resxRead.ContainsKey($fi.FullName) -and $resxRead[$fi.FullName].ContainsKey($entry.Name)){
			continue
		}
		$uses.Add((New-Use 'ResxOnly' $rel ($entry.Start+1) '' $kind '' (Get-ResxEntrySize $entry (Split-Path -Parent $fi.FullName)) ($base+':'+$entry.Name)))
	}
}
#The conversions recorded earlier for files of this scan: still uses of the map's rows.
$relScanned=@{}
foreach($fi in $files){
	$relScanned[(Get-RelPath $fi.FullName)]=$true
}
foreach($row in $script:RecordRows){
	if($relScanned.ContainsKey($row.File)){
		$u=New-Use 'Converted' $row.File 0 $row.Control '' $row.Property "$($row.Width)x$($row.Height)" $row.Key
		$u.How=$row.How
		$uses.Add($u)
	}
}
#endregion

#region Report
$unmapped=@($uses | Where-Object { $_.Target -eq '' })
$header="kind`tkey`ttarget`tfile`tline`tcontrol`tcontroltype`tproperty`twidth`theight`thow"
function Format-Use($u){
	return "$($u.Kind)`t$($u.Key)`t$($u.Target)`t$($u.File)`t$($u.Line)`t$($u.Control)`t$($u.ControlType)`t$($u.Property)`t$($u.Width)`t$($u.Height)`t$($u.How)"
}
if($Inventory -ne ''){
	$linesInv=New-Object System.Collections.Generic.List[string]
	$linesInv.Add($header)
	foreach($u in $uses){
		$linesInv.Add((Format-Use $u))
	}
	[System.IO.File]::WriteAllLines($Inventory,$linesInv,$script:Utf8)
}
$label=$Slice
if($label -eq ''){
	$label=$folderScan
}
Write-Output ("Convert-PictureUses ${label}: {0} files, {1} picture uses ({2})." -f $files.Count,$uses.Count,
	(($uses | Group-Object Kind | Sort-Object Name | ForEach-Object { "$($_.Name) $($_.Count)" }) -join ', '))
foreach($u in $unmapped){
	Write-Output ("  NO MAP ROW: {0} ({1}:{2}, {3} {4}, {5}x{6})" -f $u.Key,$u.File,$u.Line,$u.ControlType,$u.Control,$u.Width,$u.Height)
}
#endregion

#region Plan and apply
#The repository's files with these extensions, without build output, packages, git data and worktrees (folders bin, obj, packages,
#node_modules and the dot folders) and without tools/project-baseline (a dated record of the project items, re-taken by the lead).
function Get-RepoFiles([string[]]$extensions){
	$list=New-Object System.Collections.Generic.List[System.IO.FileInfo]
	$stack=New-Object System.Collections.Generic.Stack[string]
	$stack.Push($Root)
	$skip=@('bin','obj','packages','node_modules')
	$pathBaseline=Join-Path $Root 'tools\project-baseline'
	while($stack.Count -gt 0){
		$dir=$stack.Pop()
		foreach($sub in [System.IO.Directory]::GetDirectories($dir)){
			$name=[System.IO.Path]::GetFileName($sub)
			if($name.StartsWith('.') -or $skip -contains $name -or $sub -eq $pathBaseline){
				continue
			}
			$stack.Push($sub)
		}
		foreach($f in [System.IO.Directory]::GetFiles($dir)){
			if($extensions -contains [System.IO.Path]::GetExtension($f).ToLowerInvariant()){
				$list.Add((New-Object System.IO.FileInfo($f)))
			}
		}
	}
	return $list
}

#The uses to convert: a picture line with an icon target.
$toConvert=@($uses | Where-Object { ($_.Kind -eq 'Resources' -or $_.Kind -eq 'Resx') -and $_.Target -ne '' -and $_.Target -ne 'keep' -and $_.Target -ne 'UI-9b' })
$handEdits=@($uses | Where-Object { $_.Kind -eq 'ResourcesCode' -and $_.Target -ne '' -and $_.Target -ne 'keep' -and $_.Target -ne 'UI-9b' })
$changes=New-Object System.Collections.Generic.List[string]
$newRecord=New-Object System.Collections.Generic.List[object]
foreach($u in $toConvert){
	if($u.Width -le 0 -or $u.Height -le 0){
		throw "Convert-PictureUses: no pixel size for $($u.Key) ($($u.File):$($u.Line)); the picture cannot be read."
	}
	$file=Get-File (Join-Path $Root $u.File.Replace('/','\'))
	$idx=$u.Line-1
	$line=$file.Lines[$idx]
	$m=$RegexAssignResources.Match($line)
	if(-not $m.Success){
		$m=$RegexAssignResx.Match($line)
	}
	if(-not $m.Success){
		throw "Convert-PictureUses: $($u.File):$($u.Line) is no longer the picture line."
	}
	$iconExpr=Get-IconExpression $u.Target
	#The map may give the bitmap another size (a 16 x 14 picture drawn in a 16 x 16 slot): the width stays where the button's text starts.
	if($u.SizeMap -ne ''){
		$u.Width=[int]$u.SizeMap.Split('x')[0]
		$u.Height=[int]$u.SizeMap.Split('x')[1]
	}
	$isButton=($u.ControlType -match '^(OpenPmsDental\.)?UI\.Button$')
	if($isButton -and $u.Property -eq 'Image' -and $u.Width -eq $ButtonIconSize -and $u.Height -eq $ButtonIconSize -and $u.Target.StartsWith('EnumIcons.')){
		$u.How='Icon'
		$lineNew=$m.Groups['indent'].Value+$m.Groups['lhs'].Value.Substring(0,$m.Groups['lhs'].Value.Length-$u.Property.Length)+'Icon = '+$iconExpr+';'+$m.Groups['rest'].Value
	}
	else{
		$u.How='GetBitmap'
		$lineNew=$m.Groups['indent'].Value+$m.Groups['lhs'].Value+' = OpenPmsDental.UI.IconLibrary.GetBitmap('+$iconExpr+','+$u.Width+','+$u.Height+');'+$m.Groups['rest'].Value
	}
	$changes.Add("$($u.File):$($u.Line): $($line.Trim())  ->  $($lineNew.Trim())")
	$file.Lines[$idx]=$lineNew
	$file.Changed=$true
	$newRecord.Add((New-Object PSObject -Property @{ Slice=$Slice; File=$u.File; Control=$u.Control; Property=$u.Property; Key=$u.Key; Target=$u.Target;
		How=$u.How; Width=$u.Width; Height=$u.Height }))
}
foreach($u in $handEdits){
	$changes.Add("$($u.File):$($u.Line): HAND EDIT: $($u.Key) -> $($u.Target) (code, not a picture line)")
}

#Per-window .resx entries no designer reads any more.
$resxRemovals=@{}
foreach($u in $toConvert | Where-Object { $_.Kind -eq 'Resx' }){
	$pathCs=Join-Path $Root $u.File.Replace('/','\')
	$resxPath=Get-ResxPathFor $pathCs
	$name=$u.Key.Substring($u.Key.IndexOf(':')+1)
	#Is the entry still read by a designer line of the same resx (another file of the class, or another line)?
	$stillRead=$false
	$base=[System.IO.Path]::GetFileNameWithoutExtension($resxPath)
	foreach($fi in Get-ChildItem -LiteralPath (Split-Path -Parent $resxPath) -File -Filter ($base+'*.cs')){
		if($fi.Name.Split('.')[0] -ne $base){
			continue
		}
		$f=Get-File $fi.FullName
		foreach($l in $f.Lines){
			if($l.TrimStart().StartsWith('//')){
				continue
			}
			foreach($mo in $RegexGetObject.Matches($l)){
				if($mo.Groups['resx'].Value -eq $name){
					$stillRead=$true
				}
			}
		}
	}
	if(-not $stillRead){
		if(-not $resxRemovals.ContainsKey($resxPath)){
			$resxRemovals[$resxPath]=@{}
		}
		$resxRemovals[$resxPath][$name]=$true
	}
}
foreach($resxPath in @($resxRemovals.Keys)){
	$file=Get-File $resxPath
	$entries=Get-ResxEntries $file
	$ranges=@()
	foreach($name in $resxRemovals[$resxPath].Keys){
		if($entries.Contains($name)){
			$ranges+=New-Object PSObject -Property @{ Start=$entries[$name].Start; End=$entries[$name].End }
			$changes.Add("$(Get-RelPath $resxPath): remove the entry $name")
		}
	}
	foreach($r in ($ranges | Sort-Object Start -Descending)){
		Remove-Lines $file $r.Start $r.End
	}
}

#Properties.Resources pictures whose last use the slice replaced.
$resNames=@($toConvert | Where-Object { $_.Kind -eq 'Resources' } | ForEach-Object { $_.Key.Substring('Resources:'.Length) } | Sort-Object -Unique)
$resourcesRemoved=@()
if($resNames.Count -gt 0){
	#Every source file of the repository but the generated Resources.Designer.cs, with the slice's changes applied in memory.
	$pathDesignerRes=Join-Path $Root $ResourcesDesignerRel
	#Every project's generated Resources.Designer.cs is left out: it declares the pictures, it does not use them.
	$sources=@(Get-RepoFiles @('.cs','.xaml','.vb') | Where-Object { $_.FullName -ne $pathDesignerRes -and $_.Name -ne 'Resources.Designer.cs' })
	$stillUsed=@{}
	foreach($fi in $sources){
		if($script:FileCache.ContainsKey($fi.FullName)){
			$text=(($script:FileCache[$fi.FullName]).Lines -join "`n")
		}
		else{
			$text=[System.IO.File]::ReadAllText($fi.FullName)
		}
		if($text.IndexOf('Resources.',[StringComparison]::Ordinal) -lt 0){
			continue
		}
		foreach($name in $resNames){
			if($stillUsed.ContainsKey($name)){
				continue
			}
			if([regex]::IsMatch($text,'Resources\.'+[regex]::Escape($name)+'\b') -or $text.Contains('GetObject("'+$name+'"')){
				$stillUsed[$name]=Get-RelPath $fi.FullName
			}
		}
	}
	foreach($name in $resNames){
		if($stillUsed.ContainsKey($name)){
			$changes.Add("Resources:${name}: kept, still used in $($stillUsed[$name])")
			continue
		}
		$resourcesRemoved+=$name
	}
}
if($resourcesRemoved.Count -gt 0){
	$resxFile=$script:ResourcesResx
	$entries=Get-ResxEntries $resxFile
	$fileRefs=@{}
	foreach($e in $entries.Values){
		if((Get-EntryKind $e) -eq 'FileRef'){
			$fileRefs[$e.Name]=$e.Value.Split(';')[0].Trim()
		}
	}
	$ranges=@()
	$filesToDelete=@()
	foreach($name in $resourcesRemoved){
		$e=$entries[$name]
		$ranges+=New-Object PSObject -Property @{ Start=$e.Start; End=$e.End }
		$changes.Add("${ResourcesResxRel}: remove the entry $name")
		if($fileRefs.ContainsKey($name)){
			$rel=$fileRefs[$name]
			$others=@($fileRefs.Keys | Where-Object { $_ -ne $name -and $resourcesRemoved -notcontains $_ -and $fileRefs[$_] -eq $rel })
			if($others.Count -eq 0){
				$filesToDelete+=[System.IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $script:ResourcesResxPath) $rel))
			}
		}
	}
	foreach($r in ($ranges | Sort-Object Start -Descending)){
		Remove-Lines $resxFile $r.Start $r.End
	}
	$script:FileCache[$script:ResourcesResxPath]=$resxFile
	#The generated properties.
	$designer=Get-File (Join-Path $Root $ResourcesDesignerRel)
	foreach($name in $resourcesRemoved){
		$idxProp=-1
		for($i=0;$i -lt $designer.Lines.Count;$i++){
			if($designer.Lines[$i] -match ('^\s*(?:public|internal)\s+static\s+[\w.]+\s+'+[regex]::Escape($name)+'\s*\{\s*$')){
				$idxProp=$i
				break
			}
		}
		if($idxProp -lt 0){
			throw "Convert-PictureUses: no property $name in $ResourcesDesignerRel."
		}
		$indent=([regex]::Match($designer.Lines[$idxProp],'^\s*')).Value
		$start=$idxProp
		while($start -gt 0 -and $designer.Lines[$start-1].TrimStart().StartsWith('///')){
			$start--
		}
		$end=$idxProp+1
		while($end -lt $designer.Lines.Count -and $designer.Lines[$end] -ne ($indent+'}')){
			$end++
		}
		if($end+1 -lt $designer.Lines.Count -and $designer.Lines[$end+1].Trim() -eq ''){
			$end++
		}
		Remove-Lines $designer $start $end
		$changes.Add("${ResourcesDesignerRel}: remove the property $name")
	}
	#The image files no entry names any more, unless a project file or another resx names them.
	$projectTexts=@(Get-RepoFiles @('.csproj','.resx','.props','.targets') | Where-Object { $_.FullName -ne $script:ResourcesResxPath })
	foreach($pathImg in $filesToDelete | Sort-Object -Unique){
		$nameImg=[System.IO.Path]::GetFileName($pathImg)
		$named=@($projectTexts | Where-Object { ([System.IO.File]::ReadAllText($_.FullName)).IndexOf($nameImg,[StringComparison]::OrdinalIgnoreCase) -ge 0 })
		if($named.Count -gt 0){
			$changes.Add("$(Get-RelPath $pathImg): kept, named in $(($named | ForEach-Object { Get-RelPath $_.FullName }) -join ', ')")
			continue
		}
		$changes.Add("$(Get-RelPath $pathImg): delete")
		if($Apply -and (Test-Path -LiteralPath $pathImg)){
			Remove-Item -LiteralPath $pathImg
		}
	}
}

foreach($c in $changes){
	Write-Output "  $c"
}
$wouldChange=($toConvert.Count -gt 0 -or $resxRemovals.Count -gt 0 -or $resourcesRemoved.Count -gt 0)
if($Apply){
	foreach($f in $script:FileCache.Values){
		if($f.Changed){
			Write-TextFile $f
		}
	}
	if($newRecord.Count -gt 0){
		$rowsKeep=@($script:RecordRows | Where-Object { $_.Slice -ne $Slice -or -not $relScanned.ContainsKey($_.File) })
		$rowsOld=@($script:RecordRows | Where-Object { $_.Slice -eq $Slice -and $relScanned.ContainsKey($_.File) })
		$all=@($rowsKeep)+@($rowsOld)+@($newRecord)
		$linesRec=New-Object System.Collections.Generic.List[string]
		$linesRec.Add('# The pictures UI-9a turned into icons (tools/Convert-PictureUses.ps1 -Apply writes this file; docs/port-notes/UI-9a.md).')
		$linesRec.Add('# how: Icon (UI.Button.Icon, the 22 px slot drawn as vectors) or GetBitmap (IconLibrary.GetBitmap at the picture''s 96 DPI size).')
		$linesRec.Add($RecordHeader)
		foreach($r in ($all | Sort-Object Slice,File,Control,Property)){
			$linesRec.Add("$($r.Slice)`t$($r.File)`t$($r.Control)`t$($r.Property)`t$($r.Key)`t$($r.Target)`t$($r.How)`t$($r.Width)`t$($r.Height)")
		}
		$bytesRec=$script:Utf8.GetBytes((($linesRec -join "`r`n")+"`r`n"))
		[System.IO.File]::WriteAllBytes($recordDefault,$bytesRec)
	}
	Write-Output ("Applied: {0} picture lines, {1} .resx files, {2} Resources pictures removed." -f $toConvert.Count,$resxRemovals.Count,$resourcesRemoved.Count)
}
elseif($Check -and -not $isRecordDefault){
	#The planned conversions, for the gallery's before run.
	$linesRec=New-Object System.Collections.Generic.List[string]
	$linesRec.Add($RecordHeader)
	foreach($r in $newRecord){
		$linesRec.Add("$($r.Slice)`t$($r.File)`t$($r.Control)`t$($r.Property)`t$($r.Key)`t$($r.Target)`t$($r.How)`t$($r.Width)`t$($r.Height)")
	}
	[System.IO.File]::WriteAllLines($Record,$linesRec,$script:Utf8)
}
if($unmapped.Count -gt 0){
	exit 1
}
if($Check -and $wouldChange){
	exit 1
}
exit 0
#endregion
