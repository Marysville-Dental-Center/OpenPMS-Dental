<#
.SYNOPSIS
Writes CodeBase/UI/Icons/IconData.g.cs, the icon table of the UI refresh, from icons/manifest.json and the vendored Fluent SVGs.

.DESCRIPTION
Package UI-7a of docs/specs/ui-refresh-plan.md (section 4.2, docs/port-notes/UI-7a.md). Reads icons/manifest.json and, for each
row and each of its design sizes, the SVG third-party/fluentui-system-icons/icons/<fluent>_<size>_<style>.svg (or, for a row
with "custom", icons/custom/<custom>_<size>.svg). Each SVG path becomes a short list of drawing commands in design pixels:
M x y (move), L x y (line), C x1 y1 x2 y2 x y (cubic curve) and Z (close). Relative commands become absolute; H and V become
lines; S becomes C with the reflected control point; Q and T become cubic curves; A (elliptical arcs) becomes cubic curves of at
most 90 degrees each (the endpoint-to-centre conversion of SVG 1.1, appendix F.6.5). Every figure is closed, as filling does.
Numbers are rounded to 4 decimals (invariant culture). The output is deterministic: the same manifest and SVGs give the same
file byte for byte.

The file holds the UiGlyph enum (the manifest's UiGlyph rows, in manifest order, after None), the DentalIcon enum (the custom
dental icons of package UI-8, the manifest's DentalIcon rows, likewise) and IconData's table: one row per design with its enum,
name, colour role, size, file name, the SVG's SHA-256 and the path data, and, for a row with an opticalScale (an EnumIcons row whose
glyph Fluent draws smaller than its neighbours; the UI-9b-4 fix, docs/port-notes/UI-9b.md, section 5.8), that scale. The renderer (CodeBase/UI/Icons/
UiIcons*.cs) draws the table on GDI+, Direct2D and WPF.

Refused (the script stops with a message): a manifest row with an unknown enum, style, role or size, a duplicate name, a
DentalIcon row that is not a custom icon, a missing
SVG, an SVG in the vendored folder that no row names, an SVG whose viewBox is not 0 0 <size> <size>, an SVG with anything but
<path d="..."> elements (groups, clip paths, transforms, fills, even-odd fill rules), and path data it cannot read.

ASCII only. Windows PowerShell 5.1 or later. Needs no program, no database and no network.

.PARAMETER Root
The repository root. Default: the folder above this script's folder.

.PARAMETER OutFile
Where to write the file. Default: CodeBase/UI/Icons/IconData.g.cs under -Root.

.PARAMETER Check
Writes nothing: compares the generated text with -OutFile (line ends ignored) and exits 1 when they differ, 0 when equal.

.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-IconData.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-IconData.ps1 -Check
#>
[CmdletBinding()]
param(
	[string]$Root='',
	[string]$OutFile='',
	[switch]$Check
)
Set-StrictMode -Version 2.0
$ErrorActionPreference='Stop'

if($Root -eq ''){ $Root=Split-Path -Parent $PSScriptRoot }
$Root=(Resolve-Path -LiteralPath $Root).Path
if($OutFile -eq ''){ $OutFile=Join-Path $Root 'CodeBase\UI\Icons\IconData.g.cs' }
$ManifestPath=Join-Path $Root 'icons\manifest.json'
$FluentFolder=Join-Path $Root 'third-party\fluentui-system-icons\icons'
$CustomFolder=Join-Path $Root 'icons\custom'
$Invariant=[System.Globalization.CultureInfo]::InvariantCulture

$AllowedEnums=@('UiGlyph','EnumIcons','DentalIcon')
$AllowedStyles=@('regular','filled')
$AllowedRoles=@('Text','Accent','Danger','Success','Warning')
#28: the Fluent design size that no slot asks for (section 3.5); an EnumIcons row may still name it when its 28 design is the one it draws at
#every size (the UI-9b-4 fix: Bold, docs/port-notes/UI-9b.md, section 5.8).
$AllowedSizes=@(12,16,20,24,28,32,48)

function Stop-Generator([string]$message){
	throw "New-IconData: $message"
}

#region Path data
#The scanner's state: the path text and the position in it.
$script:PathText=''
$script:PathPos=0
$script:RegexNumber=New-Object System.Text.RegularExpressions.Regex('\G[+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?')

function Skip-Separators{
	while($script:PathPos -lt $script:PathText.Length){
		$c=$script:PathText[$script:PathPos]
		if($c -eq ' ' -or $c -eq ',' -or $c -eq "`t" -or $c -eq "`r" -or $c -eq "`n"){
			$script:PathPos++
		}
		else{
			return
		}
	}
}

#Whether a number follows (after separators): the implicit repeat of the last command.
function Test-NumberNext{
	Skip-Separators
	if($script:PathPos -ge $script:PathText.Length){ return $false }
	$c=$script:PathText[$script:PathPos]
	return ($c -match '[0-9.+\-]')
}

function Read-Number{
	Skip-Separators
	$match=$script:RegexNumber.Match($script:PathText,$script:PathPos)
	if(-not $match.Success -or $match.Index -ne $script:PathPos){
		Stop-Generator ("a number was expected at position "+$script:PathPos+" of the path data")
	}
	$script:PathPos+=$match.Length
	return [double]::Parse($match.Value,[System.Globalization.NumberStyles]::Float,$Invariant)
}

#An arc flag is one character, 0 or 1, and may run into the next number ("a1 1 0 01.5.5").
function Read-Flag{
	Skip-Separators
	if($script:PathPos -ge $script:PathText.Length){ Stop-Generator 'an arc flag was expected at the end of the path data' }
	$c=$script:PathText[$script:PathPos]
	if($c -ne '0' -and $c -ne '1'){ Stop-Generator ("an arc flag (0 or 1) was expected at position "+$script:PathPos) }
	$script:PathPos++
	return ($c -eq '1')
}

function Format-Number([double]$value){
	$text=[Math]::Round($value,4).ToString('0.####',$Invariant)
	if($text -eq '-0'){ $text='0' }
	return $text
}

#The output of one path: the command list and the state of the figure being written.
$script:Out=$null
$script:FigureOpen=$false
$script:StartX=0.0
$script:StartY=0.0
$script:CurX=0.0
$script:CurY=0.0

function Add-Move([double]$x,[double]$y){
	if($script:FigureOpen){ [void]$script:Out.Add('Z') }
	[void]$script:Out.Add('M '+(Format-Number $x)+' '+(Format-Number $y))
	$script:FigureOpen=$true
	$script:StartX=$x
	$script:StartY=$y
	$script:CurX=$x
	$script:CurY=$y
}

#A drawing command after Z starts a new figure at the start point of the closed one.
function Open-FigureIfClosed{
	if(-not $script:FigureOpen){
		Add-Move $script:CurX $script:CurY
	}
}

function Add-Line([double]$x,[double]$y){
	Open-FigureIfClosed
	[void]$script:Out.Add('L '+(Format-Number $x)+' '+(Format-Number $y))
	$script:CurX=$x
	$script:CurY=$y
}

function Add-Cubic([double]$x1,[double]$y1,[double]$x2,[double]$y2,[double]$x,[double]$y){
	Open-FigureIfClosed
	[void]$script:Out.Add('C '+(Format-Number $x1)+' '+(Format-Number $y1)+' '+(Format-Number $x2)+' '+(Format-Number $y2)+' '+(Format-Number $x)+' '+(Format-Number $y))
	$script:CurX=$x
	$script:CurY=$y
}

function Add-Close{
	if($script:FigureOpen){ [void]$script:Out.Add('Z') }
	$script:FigureOpen=$false
	$script:CurX=$script:StartX
	$script:CurY=$script:StartY
}

#The signed angle from vector u to vector v, in radians.
function Get-VectorAngle([double]$ux,[double]$uy,[double]$vx,[double]$vy){
	return [Math]::Atan2($ux*$vy-$uy*$vx,$ux*$vx+$uy*$vy)
}

#An elliptical arc from the current point as cubic curves (SVG 1.1, F.6.5 and F.6.6).
function Add-Arc([double]$rx,[double]$ry,[double]$angleDegrees,[bool]$isLarge,[bool]$isSweep,[double]$x,[double]$y){
	$x1=$script:CurX
	$y1=$script:CurY
	if($x1 -eq $x -and $y1 -eq $y){ return }
	$rx=[Math]::Abs($rx)
	$ry=[Math]::Abs($ry)
	if($rx -eq 0 -or $ry -eq 0){
		Add-Line $x $y
		return
	}
	$phi=$angleDegrees*[Math]::PI/180.0
	$cos=[Math]::Cos($phi)
	$sin=[Math]::Sin($phi)
	$dx=($x1-$x)/2.0
	$dy=($y1-$y)/2.0
	$x1p=$cos*$dx+$sin*$dy
	$y1p=-$sin*$dx+$cos*$dy
	$lambda=($x1p*$x1p)/($rx*$rx)+($y1p*$y1p)/($ry*$ry)
	if($lambda -gt 1){
		$root=[Math]::Sqrt($lambda)
		$rx*=$root
		$ry*=$root
	}
	$numerator=$rx*$rx*$ry*$ry-$rx*$rx*$y1p*$y1p-$ry*$ry*$x1p*$x1p
	if($numerator -lt 0){ $numerator=0 }
	$denominator=$rx*$rx*$y1p*$y1p+$ry*$ry*$x1p*$x1p
	$coefficient=0.0
	if($denominator -ne 0){ $coefficient=[Math]::Sqrt($numerator/$denominator) }
	if($isLarge -eq $isSweep){ $coefficient=-$coefficient }
	$cxp=$coefficient*$rx*$y1p/$ry
	$cyp=-$coefficient*$ry*$x1p/$rx
	$cx=$cos*$cxp-$sin*$cyp+($x1+$x)/2.0
	$cy=$sin*$cxp+$cos*$cyp+($y1+$y)/2.0
	$theta1=Get-VectorAngle 1 0 (($x1p-$cxp)/$rx) (($y1p-$cyp)/$ry)
	$delta=Get-VectorAngle (($x1p-$cxp)/$rx) (($y1p-$cyp)/$ry) ((-$x1p-$cxp)/$rx) ((-$y1p-$cyp)/$ry)
	if(-not $isSweep -and $delta -gt 0){ $delta-=2*[Math]::PI }
	if($isSweep -and $delta -lt 0){ $delta+=2*[Math]::PI }
	$count=[int][Math]::Ceiling([Math]::Abs($delta)/([Math]::PI/2.0)-1e-9)
	if($count -lt 1){ $count=1 }
	$step=$delta/$count
	$t=4.0/3.0*[Math]::Tan($step/4.0)
	$theta=$theta1
	for($i=0;$i -lt $count;$i++){
		$cos1=[Math]::Cos($theta)
		$sin1=[Math]::Sin($theta)
		$cos2=[Math]::Cos($theta+$step)
		$sin2=[Math]::Sin($theta+$step)
		#On the unit circle, then scaled by the radii, rotated by phi and moved to the centre.
		$ex1=$cos1-$t*$sin1
		$ey1=$sin1+$t*$cos1
		$ex2=$cos2+$t*$sin2
		$ey2=$sin2-$t*$cos2
		$px1=$cx+$rx*$ex1*$cos-$ry*$ey1*$sin
		$py1=$cy+$rx*$ex1*$sin+$ry*$ey1*$cos
		$px2=$cx+$rx*$ex2*$cos-$ry*$ey2*$sin
		$py2=$cy+$rx*$ex2*$sin+$ry*$ey2*$cos
		if($i -eq $count-1){
			$pxEnd=$x
			$pyEnd=$y
		}
		else{
			$pxEnd=$cx+$rx*$cos2*$cos-$ry*$sin2*$sin
			$pyEnd=$cy+$rx*$cos2*$sin+$ry*$sin2*$cos
		}
		Add-Cubic $px1 $py1 $px2 $py2 $pxEnd $pyEnd
		$theta+=$step
	}
}

#The commands of one SVG path's d attribute, appended to $listOut.
function Convert-PathData([string]$d,[System.Collections.Generic.List[string]]$listOut){
	$script:PathText=$d
	$script:PathPos=0
	$script:Out=$listOut
	$script:FigureOpen=$false
	$script:StartX=0.0
	$script:StartY=0.0
	$script:CurX=0.0
	$script:CurY=0.0
	#The second control point of the last C or S, and the control point of the last Q or T, for S and T.
	$lastCubicX=$null
	$lastCubicY=$null
	$lastQuadX=$null
	$lastQuadY=$null
	$command=$null
	while($true){
		Skip-Separators
		if($script:PathPos -ge $script:PathText.Length){ break }
		$c=$script:PathText[$script:PathPos]
		if($c -match '[MmLlHhVvCcSsQqTtAaZz]'){
			$command=[string]$c
			$script:PathPos++
		}
		elseif($null -eq $command){
			Stop-Generator 'the path data does not start with a command'
		}
		elseif($command -ceq 'Z' -or $command -ceq 'z'){
			Stop-Generator ("a number after Z at position "+$script:PathPos)
		}
		$isRelative=($command -cmatch '[a-z]')
		$upper=$command.ToUpperInvariant()
		$isFirst=$true
		do{
			$baseX=0.0
			$baseY=0.0
			if($isRelative){
				$baseX=$script:CurX
				$baseY=$script:CurY
			}
			$isCubic=$false
			$isQuad=$false
			switch($upper){
				'M'{
					$x=$baseX+(Read-Number)
					$y=$baseY+(Read-Number)
					if($isFirst){
						Add-Move $x $y
					}
					else{
						Add-Line $x $y
					}
				}
				'L'{
					$x=$baseX+(Read-Number)
					$y=$baseY+(Read-Number)
					Add-Line $x $y
				}
				'H'{
					$x=$baseX+(Read-Number)
					Add-Line $x $script:CurY
				}
				'V'{
					$y=$baseY+(Read-Number)
					Add-Line $script:CurX $y
				}
				'C'{
					$x1=$baseX+(Read-Number)
					$y1=$baseY+(Read-Number)
					$x2=$baseX+(Read-Number)
					$y2=$baseY+(Read-Number)
					$x=$baseX+(Read-Number)
					$y=$baseY+(Read-Number)
					Add-Cubic $x1 $y1 $x2 $y2 $x $y
					$lastCubicX=$x2
					$lastCubicY=$y2
					$isCubic=$true
				}
				'S'{
					$x1=$script:CurX
					$y1=$script:CurY
					if($null -ne $lastCubicX){
						$x1=2*$script:CurX-$lastCubicX
						$y1=2*$script:CurY-$lastCubicY
					}
					$x2=$baseX+(Read-Number)
					$y2=$baseY+(Read-Number)
					$x=$baseX+(Read-Number)
					$y=$baseY+(Read-Number)
					Add-Cubic $x1 $y1 $x2 $y2 $x $y
					$lastCubicX=$x2
					$lastCubicY=$y2
					$isCubic=$true
				}
				'Q'{
					$qx=$baseX+(Read-Number)
					$qy=$baseY+(Read-Number)
					$x=$baseX+(Read-Number)
					$y=$baseY+(Read-Number)
					$x0=$script:CurX
					$y0=$script:CurY
					Add-Cubic ($x0+2.0/3.0*($qx-$x0)) ($y0+2.0/3.0*($qy-$y0)) ($x+2.0/3.0*($qx-$x)) ($y+2.0/3.0*($qy-$y)) $x $y
					$lastQuadX=$qx
					$lastQuadY=$qy
					$isQuad=$true
				}
				'T'{
					$qx=$script:CurX
					$qy=$script:CurY
					if($null -ne $lastQuadX){
						$qx=2*$script:CurX-$lastQuadX
						$qy=2*$script:CurY-$lastQuadY
					}
					$x=$baseX+(Read-Number)
					$y=$baseY+(Read-Number)
					$x0=$script:CurX
					$y0=$script:CurY
					Add-Cubic ($x0+2.0/3.0*($qx-$x0)) ($y0+2.0/3.0*($qy-$y0)) ($x+2.0/3.0*($qx-$x)) ($y+2.0/3.0*($qy-$y)) $x $y
					$lastQuadX=$qx
					$lastQuadY=$qy
					$isQuad=$true
				}
				'A'{
					$rx=Read-Number
					$ry=Read-Number
					$angle=Read-Number
					$isLarge=Read-Flag
					$isSweep=Read-Flag
					$x=$baseX+(Read-Number)
					$y=$baseY+(Read-Number)
					Add-Arc $rx $ry $angle $isLarge $isSweep $x $y
				}
				'Z'{
					Add-Close
				}
			}
			if(-not $isCubic){
				$lastCubicX=$null
				$lastCubicY=$null
			}
			if(-not $isQuad){
				$lastQuadX=$null
				$lastQuadY=$null
			}
			$isFirst=$false
		}while($upper -ne 'Z' -and (Test-NumberNext))
	}
	if($script:FigureOpen){
		[void]$listOut.Add('Z')
		$script:FigureOpen=$false
	}
}
#endregion Path data

function Get-FileSha256([string]$path){
	$sha=[System.Security.Cryptography.SHA256]::Create()
	try{
		$bytes=[System.IO.File]::ReadAllBytes($path)
		return (($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join '')
	}
	finally{
		$sha.Dispose()
	}
}

#The SHA-256 of a text file with its line ends as LF, so a CRLF checkout gives the same value.
function Get-TextSha256([string]$path){
	$text=[System.IO.File]::ReadAllText($path).Replace("`r`n","`n")
	$sha=[System.Security.Cryptography.SHA256]::Create()
	try{
		$bytes=(New-Object System.Text.UTF8Encoding($false)).GetBytes($text)
		return (($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') }) -join '')
	}
	finally{
		$sha.Dispose()
	}
}

#The path data of one SVG file: every <path d> of the root, in order, as one command string.
function Read-SvgPathData([string]$path,[int]$size){
	$xml=New-Object System.Xml.XmlDocument
	$xml.XmlResolver=$null
	$xml.Load($path)
	$svg=$xml.DocumentElement
	$name=Split-Path -Leaf $path
	if($svg.LocalName -ne 'svg'){ Stop-Generator "$name is not an SVG file" }
	$viewBox=$svg.GetAttribute('viewBox')
	if($viewBox -ne "0 0 $size $size"){ Stop-Generator "$name has the viewBox '$viewBox', not '0 0 $size $size'" }
	foreach($attribute in $svg.Attributes){
		if(@('xmlns','width','height','viewBox','fill') -notcontains $attribute.LocalName){
			Stop-Generator "$name has the attribute $($attribute.Name) on <svg>, which the generator does not read"
		}
	}
	$fill=$svg.GetAttribute('fill')
	if($fill -ne '' -and $fill -ne 'none' -and $fill -ne 'currentColor'){ Stop-Generator "$name sets fill '$fill' on <svg>" }
	$listCommands=New-Object 'System.Collections.Generic.List[string]'
	$countPaths=0
	foreach($node in $svg.ChildNodes){
		if($node.NodeType -eq [System.Xml.XmlNodeType]::Comment -or $node.NodeType -eq [System.Xml.XmlNodeType]::Whitespace){ continue }
		if($node.NodeType -ne [System.Xml.XmlNodeType]::Element -or $node.LocalName -ne 'path'){
			Stop-Generator "$name holds <$($node.Name)>; the generator reads only <path d> elements (no groups, clip paths or shapes)"
		}
		foreach($attribute in $node.Attributes){
			if($attribute.LocalName -ne 'd'){
				Stop-Generator "$name has a path with the attribute $($attribute.Name) (only d is read: no fills, rules or transforms)"
			}
		}
		Convert-PathData $node.GetAttribute('d') $listCommands
		$countPaths++
	}
	if($countPaths -eq 0){ Stop-Generator "$name has no path" }
	return ($listCommands -join ' ')
}

#region Manifest
if(-not (Test-Path -LiteralPath $ManifestPath -PathType Leaf)){ Stop-Generator "no manifest at $ManifestPath" }
$manifest=[System.IO.File]::ReadAllText($ManifestPath) | ConvertFrom-Json
$fluentVersion=[string]$manifest.fluentVersion
if($fluentVersion -notmatch '^\d+\.\d+\.\d+$'){ Stop-Generator "the manifest's fluentVersion '$fluentVersion' is not a version" }
$listRows=@($manifest.icons)
if($listRows.Count -eq 0){ Stop-Generator 'the manifest lists no icons' }
$namesSeen=@{}
$filesUsed=@{}
$listDesignLines=New-Object 'System.Collections.Generic.List[string]'
$listGlyphLines=New-Object 'System.Collections.Generic.List[string]'
$listDentalLines=New-Object 'System.Collections.Generic.List[string]'
foreach($row in $listRows){
	$enum=[string]$row.enum
	$name=[string]$row.name
	$style=[string]$row.style
	$role=[string]$row.role
	$purpose=[string]$row.purpose
	$fluent=''
	$custom=''
	if($row.PSObject.Properties.Name -contains 'fluent'){ $fluent=[string]$row.fluent }
	if($row.PSObject.Properties.Name -contains 'custom'){ $custom=[string]$row.custom }
	if($AllowedEnums -cnotcontains $enum){ Stop-Generator "row '$name': the enum '$enum' is not one of $($AllowedEnums -join ', ')" }
	if($name -cnotmatch '^[A-Z][A-Za-z0-9]*$'){ Stop-Generator "row '$name': the name is not a C# member name" }
	if($name -eq 'None'){ Stop-Generator "row '$name': None is the enum's empty member" }
	$key=$enum+'.'+$name
	if($namesSeen.ContainsKey($key)){ Stop-Generator "the manifest lists $key twice" }
	$namesSeen[$key]=$true
	if($AllowedStyles -cnotcontains $style){ Stop-Generator "row '$name': the style '$style' is not one of $($AllowedStyles -join ', ')" }
	if($AllowedRoles -cnotcontains $role){ Stop-Generator "row '$name': the role '$role' is not one of $($AllowedRoles -join ', ')" }
	if(($fluent -eq '') -eq ($custom -eq '')){ Stop-Generator "row '$name': give fluent or custom, not both and not neither" }
	if($fluent -ne '' -and $fluent -cnotmatch '^[a-z0-9]+(_[a-z0-9]+)*$'){ Stop-Generator "row '$name': '$fluent' is not a Fluent file name stem" }
	if($custom -ne '' -and $custom -cnotmatch '^[a-z0-9]+(_[a-z0-9]+)*$'){ Stop-Generator "row '$name': '$custom' is not a custom file name stem" }
	if($enum -eq 'DentalIcon' -and $custom -eq ''){ Stop-Generator "row '$name': a DentalIcon row is a custom icon (icons/custom)" }
	if($purpose.Trim() -eq ''){ Stop-Generator "row '$name' gives no purpose" }
	$sizes=@($row.sizes | ForEach-Object { [int]$_ })
	if($sizes.Count -eq 0){ Stop-Generator "row '$name' lists no sizes" }
	for($i=0;$i -lt $sizes.Count;$i++){
		if($AllowedSizes -notcontains $sizes[$i]){ Stop-Generator "row '$name': the size $($sizes[$i]) is not one of $($AllowedSizes -join ', ')" }
		if($i -gt 0 -and $sizes[$i] -le $sizes[$i-1]){ Stop-Generator "row '$name': the sizes are not in rising order" }
	}
	#opticalScale (optional; the UI-9b-4 fix): the row's designs are drawn this much larger about their centre. EnumIcons rows only, 1 to 1.5.
	$opticalScaleText=''
	if($row.PSObject.Properties.Name -contains 'opticalScale'){
		$opticalScale=[double]$row.opticalScale
		if($enum -ne 'EnumIcons'){ Stop-Generator "row '$name': only an EnumIcons row takes an opticalScale" }
		if($opticalScale -le 1 -or $opticalScale -gt 1.5){ Stop-Generator "row '$name': the opticalScale $opticalScale is not above 1 and at most 1.5" }
		$opticalScaleText=','+(Format-Number $opticalScale)+'f'
	}
	foreach($size in $sizes){
		if($fluent -ne ''){
			$fileName=$fluent+'_'+$size+'_'+$style+'.svg'
			$filePath=Join-Path $FluentFolder $fileName
			$fileShown=$fileName
		}
		else{
			$fileName=$custom+'_'+$size+'.svg'
			$filePath=Join-Path $CustomFolder $fileName
			$fileShown='custom/'+$fileName
		}
		if(-not (Test-Path -LiteralPath $filePath -PathType Leaf)){ Stop-Generator "row '$name': $fileShown is missing" }
		$filesUsed[$filePath.ToLowerInvariant()]=$true
		$pathData=Read-SvgPathData $filePath $size
		$sha256=Get-FileSha256 $filePath
		[void]$listDesignLines.Add("`t`t`tnew IconDesign(`"$enum`",`"$name`",`"$role`",$size,`"$fileShown`",`"$sha256`",`"$pathData`"$opticalScaleText),")
	}
	if($enum -eq 'UiGlyph'){
		if($fluent -ne ''){ $source=$fluent+', '+$style } else { $source='custom '+$custom }
		$summary=($purpose.Replace('&','&amp;').Replace('<','&lt;').Replace('>','&gt;'))
		[void]$listGlyphLines.Add("`t`t///<summary>Fluent $source ($($sizes -join ', ') px designs): $summary.</summary>")
		[void]$listGlyphLines.Add("`t`t$name,")
	}
	if($enum -eq 'DentalIcon'){
		$summary=($purpose.Replace('&','&amp;').Replace('<','&lt;').Replace('>','&gt;'))
		[void]$listDentalLines.Add("`t`t///<summary>Custom icons/custom/$($custom)_*.svg, $style ($($sizes -join ', ') px designs): $summary.</summary>")
		[void]$listDentalLines.Add("`t`t$name,")
	}
}
foreach($folder in @($FluentFolder,$CustomFolder)){
	if(Test-Path -LiteralPath $folder -PathType Container){
		foreach($file in (Get-ChildItem -LiteralPath $folder -File | Sort-Object Name)){
			if(-not $filesUsed.ContainsKey($file.FullName.ToLowerInvariant())){
				Stop-Generator "$($file.Name) in $folder is in no manifest row: remove it or add its row"
			}
		}
	}
}
#endregion Manifest

#region Output
$manifestSha256=Get-TextSha256 $ManifestPath
$lines=New-Object 'System.Collections.Generic.List[string]'
[void]$lines.Add('// <auto-generated>')
[void]$lines.Add('// Generated by tools/New-IconData.ps1 from icons/manifest.json and the SVGs in third-party/fluentui-system-icons/icons and icons/custom.')
[void]$lines.Add('// Do not edit: change the manifest or the SVGs and run the script (docs/port-notes/UI-7a.md, docs/port-notes/UI-8.md, icons/README.md).')
[void]$lines.Add("// The SVGs under third-party are Fluent UI System Icons $fluentVersion, Copyright (c) 2020 Microsoft Corporation, MIT License")
[void]$lines.Add('// (third-party/fluentui-system-icons/LICENSE; THIRD-PARTY-NOTICES.txt); those in icons/custom are the program''s own artwork under its GPL')
[void]$lines.Add('// (tools/New-DentalIcons.ps1 draws them; icons/README.md). The path data is in design pixels, (0,0) the top left corner of')
[void]$lines.Add('// the design square: M x y moves, L x y draws a line, C x1 y1 x2 y2 x y a cubic curve, Z closes the figure. Fill rule: nonzero.')
[void]$lines.Add('// System only, C# 7.3: RdlDesign can compile this file in as a linked file (section 4.2 of docs/specs/ui-refresh-plan.md).')
[void]$lines.Add('// </auto-generated>')
[void]$lines.Add('using System;')
[void]$lines.Add('')
[void]$lines.Add('namespace CodeBase {')
[void]$lines.Add("`t///<summary>The control glyphs of the UI refresh (icons/manifest.json, the rows of enum UiGlyph), drawn by UiIcons on GDI+, Direct2D and WPF.</summary>")
[void]$lines.Add("`tpublic enum UiGlyph {")
[void]$lines.Add("`t`t///<summary>No glyph: draws nothing.</summary>")
[void]$lines.Add("`t`tNone,")
foreach($line in $listGlyphLines){ [void]$lines.Add($line) }
[void]$lines.Add("`t}")
[void]$lines.Add('')
[void]$lines.Add("`t///<summary>The custom dental icons of the UI refresh (icons/manifest.json, the rows of enum DentalIcon; package UI-8), each Regular icon followed")
[void]$lines.Add("`t///by its Filled twin (<name>Filled), drawn by UiIcons on GDI+, Direct2D and WPF.</summary>")
[void]$lines.Add("`tpublic enum DentalIcon {")
[void]$lines.Add("`t`t///<summary>No icon: draws nothing.</summary>")
[void]$lines.Add("`t`tNone,")
foreach($line in $listDentalLines){ [void]$lines.Add($line) }
[void]$lines.Add("`t}")
[void]$lines.Add('')
[void]$lines.Add("`tpublic static partial class IconData {")
[void]$lines.Add("`t`t///<summary>The Fluent UI System Icons version (@fluentui/svg-icons) of the SVGs.</summary>")
[void]$lines.Add("`t`tpublic const string FLUENT_VERSION=`"$fluentVersion`";")
[void]$lines.Add("`t`t///<summary>The SHA-256 of icons/manifest.json (UTF-8, line ends as LF) the table was made from.</summary>")
[void]$lines.Add("`t`tpublic const string MANIFEST_SHA256=`"$manifestSha256`";")
[void]$lines.Add('')
[void]$lines.Add("`t`t///<summary>Every design of every manifest row, in manifest order and by rising size: enum, name, colour role, design size, the SVG")
[void]$lines.Add("`t`t///(under third-party/fluentui-system-icons/icons, or icons/custom), its SHA-256, its path data and, when the row has one, its optical scale.</summary>")
[void]$lines.Add("`t`tprivate static readonly IconDesign[] _arrayDesigns=new IconDesign[] {")
foreach($line in $listDesignLines){ [void]$lines.Add($line) }
[void]$lines.Add("`t`t};")
[void]$lines.Add("`t}")
[void]$lines.Add('}')
$text=($lines -join "`r`n")+"`r`n"
foreach($ch in $text.ToCharArray()){
	if([int]$ch -gt 126){ Stop-Generator 'the output is not ASCII' }
}
if($Check){
	if(-not (Test-Path -LiteralPath $OutFile -PathType Leaf)){
		Write-Output "New-IconData: $OutFile is missing."
		exit 1
	}
	$current=[System.IO.File]::ReadAllText($OutFile).Replace("`r`n","`n")
	if($current -ne $text.Replace("`r`n","`n")){
		Write-Output "New-IconData: $OutFile is not the output of the manifest and the SVGs. Run tools\New-IconData.ps1."
		exit 1
	}
	Write-Output "New-IconData: $OutFile is up to date ($($listDesignLines.Count) designs, $($listGlyphLines.Count/2) glyphs, $($listDentalLines.Count/2) dental icons)."
	exit 0
}
$folderOut=Split-Path -Parent $OutFile
if(-not (Test-Path -LiteralPath $folderOut -PathType Container)){ [void](New-Item -ItemType Directory -Path $folderOut) }
[System.IO.File]::WriteAllText($OutFile,$text,(New-Object System.Text.UTF8Encoding($false)))
Write-Output "New-IconData: wrote $OutFile ($($listDesignLines.Count) designs, $($listGlyphLines.Count/2) glyphs, $($listDentalLines.Count/2) dental icons)."
exit 0
#endregion Output
