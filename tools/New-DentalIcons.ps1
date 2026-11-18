<#
.SYNOPSIS
Writes the custom dental icons of the UI refresh, icons/custom/*.svg, from their drawing in tools/DentalIcons/DentalIconArt.cs.

.DESCRIPTION
Package UI-8 of docs/specs/ui-refresh-plan.md (section 4.4; docs/port-notes/UI-8.md; icons/README.md, "The custom icons").
The icons are the program's own artwork, drawn in tools/DentalIcons/DentalIconArt.cs as shapes in Fluent UI System Icons' style
(the same grids, stroke weights, round ends and joins, one colour, a Filled twin). This script compiles that file with Add-Type
and writes, for every icon and every design size (16, 20, 24, 32, 48), icons/custom/<stem>_<size>.svg (Regular) and
icons/custom/<stem>_filled_<size>.svg (Filled): one <path d> each, nonzero fill, UTF-8 without BOM and without a line end.
The output is deterministic: the same drawing gives the same files byte for byte.

After it, run tools/New-IconData.ps1, which reads the SVGs through their rows in icons/manifest.json into
CodeBase/UI/Icons/IconData.g.cs, and commit the drawing, the SVGs, the manifest and the generated file together.

ASCII only. Windows PowerShell 5.1 (WPF's geometry of the .NET Framework). Needs no program, no database, no network and no
window: the shapes are combined with WPF's Geometry.Combine, never rendered (except with -Preview).

.PARAMETER Root
The repository root. Default: the folder above this script's folder.

.PARAMETER Check
Writes nothing: compares the drawing's SVGs with icons/custom and exits 1 when a file differs, is missing or is not the
drawing's, 0 when all agree.

.PARAMETER Preview
A folder to write a picture of every design into (preview-<size>.png, black on white, Regular above Filled), for a look
while drawing. Rendered with WPF off screen; needs no window.

.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-DentalIcons.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-DentalIcons.ps1 -Check
#>
[CmdletBinding()]
param(
	[string]$Root='',
	[switch]$Check,
	[string]$Preview=''
)
Set-StrictMode -Version 2.0
$ErrorActionPreference='Stop'

if($Root -eq ''){ $Root=Split-Path -Parent $PSScriptRoot }
$Root=(Resolve-Path -LiteralPath $Root).Path
$CustomFolder=Join-Path $Root 'icons\custom'
$ArtPath=Join-Path $PSScriptRoot 'DentalIcons\DentalIconArt.cs'

Add-Type -AssemblyName PresentationCore,WindowsBase,System.Xaml
$referenced=@(
	[System.Windows.Media.Geometry].Assembly.Location,
	[System.Windows.Point].Assembly.Location,
	[System.Xaml.XamlReader].Assembly.Location
)
if(-not ('DentalIconTools.DentalIconArt' -as [type])){
	Add-Type -Path $ArtPath -ReferencedAssemblies $referenced
}

$utf8=New-Object System.Text.UTF8Encoding($false)
$wanted=New-Object 'System.Collections.Generic.SortedDictionary[string,string]' ([System.StringComparer]::Ordinal)
foreach($stem in [DentalIconTools.DentalIconArt]::GetStems()){
	foreach($size in [DentalIconTools.DentalIconArt]::Sizes){
		foreach($isFilled in @($false,$true)){
			$fileName=[DentalIconTools.DentalIconArt]::GetFileName($stem,$size,$isFilled)
			$wanted[$fileName]=[DentalIconTools.DentalIconArt]::GetSvg($stem,$size,$isFilled)
		}
	}
}

if($Preview -ne ''){
	if(-not (Test-Path -LiteralPath $Preview -PathType Container)){ [void](New-Item -ItemType Directory -Path $Preview) }
	$stems=[DentalIconTools.DentalIconArt]::GetStems()
	foreach($size in [DentalIconTools.DentalIconArt]::Sizes){
		$cell=$size+8
		$width=$cell*$stems.Count
		$height=$cell*2
		$visual=New-Object System.Windows.Media.DrawingVisual
		$context=$visual.RenderOpen()
		$context.DrawRectangle([System.Windows.Media.Brushes]::White,$null,(New-Object System.Windows.Rect(0,0,$width,$height)))
		for($i=0;$i -lt $stems.Count;$i++){
			for($row=0;$row -lt 2;$row++){
				$svg=$wanted[[DentalIconTools.DentalIconArt]::GetFileName($stems[$i],$size,($row -eq 1))]
				$d=[regex]::Match($svg,'<path d="([^"]*)"').Groups[1].Value
				$geometry=[System.Windows.Media.Geometry]::Parse('F1 '+$d)
				$context.PushTransform((New-Object System.Windows.Media.TranslateTransform(($i*$cell+4),($row*$cell+4))))
				$context.DrawGeometry([System.Windows.Media.Brushes]::Black,$null,$geometry)
				$context.Pop()
			}
		}
		$context.Close()
		$bitmap=New-Object System.Windows.Media.Imaging.RenderTargetBitmap($width,$height,96,96,[System.Windows.Media.PixelFormats]::Pbgra32)
		$bitmap.Render($visual)
		$encoder=New-Object System.Windows.Media.Imaging.PngBitmapEncoder
		$encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
		$stream=[System.IO.File]::Create((Join-Path $Preview ('preview-'+$size+'.png')))
		try{ $encoder.Save($stream) } finally { $stream.Dispose() }
	}
	Write-Output "New-DentalIcons: wrote the previews to $Preview."
}

$present=@()
if(Test-Path -LiteralPath $CustomFolder -PathType Container){
	$present=@(Get-ChildItem -LiteralPath $CustomFolder -File | ForEach-Object { $_.Name })
}
if($Check){
	$problems=New-Object 'System.Collections.Generic.List[string]'
	foreach($fileName in $wanted.Keys){
		$path=Join-Path $CustomFolder $fileName
		if(-not (Test-Path -LiteralPath $path -PathType Leaf)){
			[void]$problems.Add("$fileName is missing")
			continue
		}
		$bytes=[System.IO.File]::ReadAllBytes($path)
		if(-not [System.Linq.Enumerable]::SequenceEqual([byte[]]$bytes,[byte[]]$utf8.GetBytes($wanted[$fileName]))){
			[void]$problems.Add("$fileName is not the drawing's")
		}
	}
	foreach($name in $present){
		if(-not $wanted.ContainsKey($name)){ [void]$problems.Add("$name is not a design of the drawing") }
	}
	if($problems.Count -gt 0){
		Write-Output ("New-DentalIcons: icons/custom does not match the drawing. Run tools\New-DentalIcons.ps1. "+($problems -join '; '))
		exit 1
	}
	Write-Output "New-DentalIcons: icons/custom is up to date ($($wanted.Count) designs)."
	exit 0
}
if(-not (Test-Path -LiteralPath $CustomFolder -PathType Container)){ [void](New-Item -ItemType Directory -Path $CustomFolder) }
foreach($name in $present){
	if(-not $wanted.ContainsKey($name)){ Remove-Item -LiteralPath (Join-Path $CustomFolder $name) }
}
foreach($fileName in $wanted.Keys){
	[System.IO.File]::WriteAllBytes((Join-Path $CustomFolder $fileName),$utf8.GetBytes($wanted[$fileName]))
}
Write-Output "New-DentalIcons: wrote $($wanted.Count) designs to $CustomFolder."
exit 0
