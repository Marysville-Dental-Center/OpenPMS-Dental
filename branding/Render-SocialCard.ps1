# Renders branding/social-card.html to the two link preview pictures with headless Chrome (or Edge):
#   site/social-card.png         1200x630, the project site's og:image and twitter:image
#   branding/social-preview.png  1280x640, for the repository's Settings > Social preview (uploaded by hand)
# Run from anywhere: powershell -File branding\Render-SocialCard.ps1
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$browser = @(
	"$env:ProgramFiles\Google\Chrome\Application\chrome.exe",
	"${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $browser) { throw 'Neither Chrome nor Edge was found.' }
$card = 'file:///' + ((Join-Path $PSScriptRoot 'social-card.html') -replace '\\', '/')
$profile = Join-Path ([IO.Path]::GetTempPath()) ('social-card-' + [guid]::NewGuid())
try {
	foreach ($shot in @(
		@{ Path = (Join-Path $root 'site\social-card.png'); Size = '1200,630' },
		@{ Path = (Join-Path $PSScriptRoot 'social-preview.png'); Size = '1280,640' }
	)) {
		if (Test-Path $shot.Path) { Remove-Item $shot.Path }
		& $browser --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=1 "--user-data-dir=$profile" `
			"--window-size=$($shot.Size)" "--screenshot=$($shot.Path)" $card | Out-Null
		$deadline = (Get-Date).AddSeconds(30)
		while (-not (Test-Path $shot.Path) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 250 }
		if (-not (Test-Path $shot.Path)) { throw "No picture was written: $($shot.Path)" }
		Add-Type -AssemblyName System.Drawing
		$image = [System.Drawing.Image]::FromFile($shot.Path)
		try { "$($shot.Path): $($image.Width)x$($image.Height)" } finally { $image.Dispose() }
	}
}
finally {
	Start-Sleep -Milliseconds 500
	Remove-Item -Recurse -Force $profile -ErrorAction SilentlyContinue
}
