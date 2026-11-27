<#
.SYNOPSIS
Builds an Open PMS - Dental release: a Release x64 build, one checked staging folder, SHA256SUMS.txt and the Inno Setup installer.

.DESCRIPTION
Since stage 3b of the .NET 10 move (docs\specs\net10-plan.md, package S3b-7) the program and every exe beside it are net10.0-windows x64,
self-contained on one x64 .NET runtime in the program folder; the 32-bit helper program is self-contained with its own x86 runtime in the
one sub folder Helper32\ (decisions 19 and 20). A release is staged in three lists: the program's own files, the third-party files, and the
runtime files, which are read from the runtime packs' own file lists (data\RuntimeList.xml of Microsoft.NETCore.App.Runtime and
Microsoft.WindowsDesktop.App.Runtime, win-x64 and win-x86, in the offline NuGet folder: the lists the SDK's ResolveRuntimePackAssets reads),
not typed in here. The release channel publishes the runtime files as the release's runtime part (ForkReleases.GetRuntimePart).

1. Builds as Release x64, through the shared build lane when -BuildLane is given (with -LaneProjects, only those projects and what they
   reference, through the lane's scoped restore). PDFsharp/MigraDoc and WeCantSpell.Hunspell build from their source in the git
   submodules under third-party\ (docs\develop\third-party-source-builds.md).
2. Checks OpenPmsDental\bin\Release: every file must be on the allow-lists below or be one the release never ships (debug symbols,
   XML documentation). A file on the deny-list, an unknown file or a sub folder other than Helper32\ fails the script. The release-flow
   checks refuse a build whose .deps.json files name more than one runtime version, whose helper runs on another runtime version than the
   program, or whose runtime is older than the newest 10.0.x Microsoft.NETCore.App.Runtime.win-x64 in the offline folder (the monthly
   runtime release, section 2.4 of the plan).
3. Stages the allowed files into one folder (<OutDir>\staging), with LICENSE, LICENSE-GPL-3.0.txt, LICENSE-AGPL-3.0.txt,
   LICENSE-LGPL-2.1.txt, NOTICE, THIRD-PARTY-NOTICES.txt, db\init\new-database.sql and db\README.md, and checks the staging folder again: allow-list, deny-list, every allowed file present,
   every third-party file named in THIRD-PARTY-NOTICES.txt, every file built from this repository equal to its project's own output, every
   third-party file of a NuGet package byte for byte the package's file at the version OpenPmsDental\packages.lock.json names (from the
   offline package folder or the global packages folder the restore used; Test-PackageFiles), every other third-party file equal to its copy
   in "Required dlls", every runtime file byte for byte its runtime pack's file, the PE machine of every exe and native file (0x8664 at the
   top and for the x64 runtime, 0x14C for Helper32\; a managed file IL only or of its folder's machine), the native files free of the debug
   C runtime, every .runtimeconfig.json of one runtime version, and OpenPmsDental.exe's versions (file version 24.3.63.0, the database
   compatibility version; product version the release version of SharedAssemblyInfo.cs).
   The 64-bit and the 32-bit TWAINDSM.dll are setup files, never files of the program folder (which still refuses one): both are taken
   from the approved download of the TWAIN Working Group's release (-TwainDsmZip), each checked against its pinned SHA-256, version, PE
   machine and TWAIN Working Group signature, and passed to the setup, which installs the 64-bit one into the 64-bit system folder and the
   32-bit one into the 32-bit system folder (SysWOW64), for the 32-bit TWAIN path through the helper (S3b-8).
4. Prints the build id (ForkReleases.ComputeBuildId over ForkReleases.ListBuildIdFiles) and writes <OutDir>\SHA256SUMS.txt.
5. Writes the setup's information page (installer\setup-info.txt with NOTICE) and License Agreement page (installer\setup-license.txt:
   how the installed program is licensed, then LICENSE-GPL-3.0.txt) into <OutDir>, compiles installer\OpenPmsDental.iss with Inno Setup 6
   into <OutDir>, checks that the files the compiler packed are exactly the staged files and the two TWAINDSM.dll, and adds the setup's SHA-256 to
   SHA256SUMS.txt.
6. Writes the source archive, <OutDir>\OpenPmsDental-<version>-source.zip: the repository at the release commit (git archive HEAD) with
   every submodule at the commit the release commit pins (PDFsharp/MigraDoc and WeCantSpell.Hunspell), the source of the setup's
   TWAINDSM.dll (the approved twain-dsm-2.5.1.zip, byte for byte, as third-party-source/twain-dsm-2.5.1.zip), and SOURCE-COMMITS.txt naming
   them, so the Corresponding Source of everything the setup ships is published with the release even if an upstream repository goes away.
   Its SHA-256 goes into SHA256SUMS.txt. A submodule that is not checked out at its pinned commit fails the script (the shipped files were
   built from the checkout), and so does a missing TWAIN DSM source download or one with another SHA-256; both are checked before the build.
7. With -CopyTo, copies the setup, the source archive, SHA256SUMS.txt and the release notes (docs\release-notes\v<version>.md) there.

The staging folder is exactly what the installer puts in the program folder. The uninstaller goes to the program folder's "uninstall"
sub folder, never the top level: a release published from the update server holds every top-level file of the program folder.

Nothing is downloaded and nothing connects to a database.

Exit codes: 0 done; 1 a check failed; 2 the build failed; 3 the installer did not compile; 4 a tool or file is missing (git, a submodule).

.PARAMETER BuildLane
Path of the shared build lane script (odbuild.ps1) of this PC. The build then runs through it, so only one build runs at a time.
Without it, the script builds with the Build Tools 2026 MSBuild 18 itself, found with vswhere as
tools\Initialize-BuildEnvironment.ps1 finds it, in the environment that script sets (the pinned .NET SDK, the offline NuGet
folder), with a locked restore first.

.PARAMETER LaneWaitMinutes
How long the build lane waits for another build to finish. Default 15.

.PARAMETER LaneProjects
With -BuildLane: build only these projects (names as in OpenDental.sln) and what they reference, through the lane's scoped restore
(-ScopedRestore). Default: the whole solution filter, which restores whole since S3b-5 (no .NET 10 project references a .NET
Framework one any more); a release builds it whole. Naming projects (OpenPmsDental,xDirect2dWrapper) is for a quick dry run.

.PARAMETER TwainDsmZip
The approved download of the TWAIN Working Group's TWAIN DSM release (dsm_020501.zip). Default: od-tools\downloads\s3b7-twaindsm-20261116\
in the repository's common git folder. Its windows\64\TWAINDSM.dll and windows\32\TWAINDSM.dll must each have their pinned SHA-256, the
approved version, their PE machine (0x8664 and 0x14C) and a valid TWAIN Working Group signature.

.PARAMETER TwainDsmSourceZip
The approved download of the source of that TWAIN DSM release (twain-dsm-2.5.1.zip, LGPL 2.1). Default: the same download folder. It must
have the pinned SHA-256; the source archive carries it as third-party-source/twain-dsm-2.5.1.zip. Without it no source archive is written.

.PARAMETER SkipBuild
Use the Release output that is already there. It must come from a Release build of the current source.

.PARAMETER MSBuildPath
Full path of MSBuild.exe when not building through -BuildLane. Default: the Build Tools 2026 (Visual Studio 2026) MSBuild 18,
found with vswhere and the version range [18.0,19.0) (tools\Initialize-BuildEnvironment.ps1).

.PARAMETER OutDir
Where the staging folder, SHA256SUMS.txt and the setup go. Default: installer\bin in this repository (ignored by git).

.PARAMETER SkipInstaller
Stage and check only; do not compile the installer.

.PARAMETER Iscc
Full path of Inno Setup 6's ISCC.exe. Default: Inno Setup 6 under Program Files (x86) or %LOCALAPPDATA%\Programs.

.PARAMETER CopyTo
A folder (created if missing) to copy the setup, SHA256SUMS.txt and the release notes to.

.PARAMETER CheckFolder
Only run the list checks (allow-list, deny-list, required files, third-party notices) on this folder, as if it were the staging folder,
and print its build id. Nothing is built or written. Used by tests\ReleaseFlowTests\InstallerTests.cs.

.PARAMETER Full
With -CheckFolder: also the checks a release runs on its staging folder that need the Release build and the program's restore (step 3):
every file built here equal to its project's Release output, every third-party file the file of its NuGet package or of "Required dlls",
the native files and the versions. Used by tests\ReleaseFlowTests\PackageFilesTests.cs.

.PARAMETER ListFiles
Only print the allow-list, one "own|path", "third|name", "doc|path" or "runtime|path" line per file of the program folder (path relative to
it: Helper32\ for the helper and its runtime), then "setup|TWAINDSM.dll" and "setup|x86\TWAINDSM.dll" for the 64-bit and the 32-bit TWAIN
data source manager, the files the setup installs outside the program folder (the 64-bit and the 32-bit system folder). The
runtime lines come from the runtime packs of the newest 10.0.x runtime in the offline folder, the one a release must have.

.PARAMETER WriteSetupTexts
Only write the setup's two pages, setup-info.txt and setup-license.txt, into this folder (created if missing), as a release writes them
into <OutDir>. Nothing is built or checked. Used by tests\ReleaseFlowTests\InstallerTests.cs.

.PARAMETER WriteSourceArchive
Only write the source archive (step 6) into this folder (created if missing) and print its path. Nothing is built; only the submodules and
the TWAIN DSM source download are checked, as for a release. Used by tests\PdfLibraryTests\SourceArchiveTests.cs and
tests\ReleaseFlowTests\InstallerTests.cs.

.PARAMETER WriteSetupFiles
Only write the setup files into this folder (created if missing) as a release writes them into <OutDir>\setup-files: TWAINDSM.dll (64-bit)
and x86\TWAINDSM.dll (32-bit) from the approved download (-TwainDsmZip), each after the checks a release makes (SHA-256, version, PE
machine, signature), and print each path. Exit 1 when a check fails. Nothing is built. Used by tests\ReleaseFlowTests\InstallerTests.cs.

.EXAMPLE
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Build-Release.ps1 -BuildLane C:\path\to\odbuild.ps1 -CopyTo D:\releases\v0.0
#>
[CmdletBinding(PositionalBinding=$false)]
param(
	[string]$BuildLane='',
	[int]$LaneWaitMinutes=15,
	[string[]]$LaneProjects=@(),
	[string]$TwainDsmZip='',
	[string]$TwainDsmSourceZip='',
	[switch]$SkipBuild,
	[string]$MSBuildPath='',
	[string]$OutDir='',
	[switch]$SkipInstaller,
	[string]$Iscc='',
	[string]$CopyTo='',
	[string]$CheckFolder='',
	[switch]$Full,
	[switch]$ListFiles,
	[string]$WriteSetupTexts='',
	[string]$WriteSourceArchive='',
	[string]$WriteSetupFiles=''
)
Set-StrictMode -Version 2
$ErrorActionPreference='Stop'
$RepoRoot=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
#Get-BuildToolPath and Initialize-BuildEnvironment: the MSBuild look-up and the build environment every tool script shares.
. (Join-Path $PSScriptRoot 'Initialize-BuildEnvironment.ps1')

#region The lists
#The database compatibility version. Every AssemblyVersion and OpenPmsDental.exe's file version stay on it (ForkVersions.CompatVersion).
$CompatVersion='24.3.63.0'

#Files built from this repository, by their path in the program folder. Value: the project output (repository-relative) the staged file
#must equal, so that a prebuilt file of the same name can never ship in its place (Open Dental's closed ImagingDeviceManager.dll had this
#name; ODR.dll ships only when built from source; Direct2dWrapper.dll must be the Release build). '' = OpenPmsDental's own output or a
#generated file. Each .NET 10 exe is its apphost (.exe), its assembly (.dll) and the two files that tell the host how to run it
#(.runtimeconfig.json, .deps.json); an app.config becomes <name>.dll.config, and no *.exe.config is written any more (S3b-4).
$OwnFiles=[ordered]@{
	'OpenPmsDental.exe'=''
	'OpenPmsDental.dll'=''
	'OpenPmsDental.dll.config'=''
	'OpenPmsDental.runtimeconfig.json'=''
	'OpenPmsDental.deps.json'=''
	#Both XmlSerializers files, from Microsoft.XmlSerializer.Generator since S3b-3 and S3b-4 (the plan's critique W1).
	'OpenPmsDental.XmlSerializers.dll'=''
	'OpenPmsDentalBusiness.dll'='OpenPmsDentalBusiness\bin\Release'
	'OpenPmsDentalBusiness.dll.config'=''
	'OpenPmsDentalBusiness.XmlSerializers.dll'='OpenPmsDentalBusiness\bin\Release'
	'CodeBase.dll'='CodeBase\bin\Release'
	'DataConnectionBase.dll'='DataConnectionBase\bin\Release'
	'WpfControlsOD.dll'='WpfControlsOD\bin\Release'
	'WpfControlsOD.dll.config'=''
	'OpenPmsDentalGraph.dll'='OpenPmsDentalGraph\bin\Release'
	'OpenPmsDentalGraph.dll.config'=''
	'OpenPmsDentalStorage.dll'='OpenPmsDentalStorage\bin\Release'
	'SparksToothChart.dll'='SparksToothChart\bin\Release'
	'SparksToothChart.dll.config'=''
	'ODCrypt.dll'='ODCrypt\bin\Release'
	'PasswordVaultWrapper.dll'='PasswordVaultWrapper\bin\Release'
	'ImagingDeviceManager.dll'='ImagingDeviceManager\bin\Release'
	'ImagingCapture.dll'='ImagingCapture\bin\Release'
	'ODR.dll'='ODR\bin\Release'
	#The x64 build of the drawing library (S3a-16, S3b-4): its x64 configuration's output folder.
	'Direct2dWrapper.dll'='Direct2dWrapper\bin\x64\Release'
	'ServiceManager.exe'='ServiceManager\bin\Release'
	'ServiceManager.dll'='ServiceManager\bin\Release'
	'ServiceManager.dll.config'=''
	'ServiceManager.runtimeconfig.json'=''
	'ServiceManager.deps.json'=''
	#The update file copier: OpenPmsDental.csproj copies its four files next to the program; it runs on the program folder's runtime, and a
	#pull starts it from a temp folder with a copy of that runtime (ForkReleases.CopyFileCopier). It has no .dll.config (S3b-4).
	'UpdateFileCopier.exe'='UpdateFileCopier\bin\Release'
	'UpdateFileCopier.dll'='UpdateFileCopier\bin\Release'
	'UpdateFileCopier.runtimeconfig.json'=''
	'UpdateFileCopier.deps.json'=''
	#The connection settings tool (docs/specs/db-connection-security.md); the installer runs it from the program folder for the connection page.
	'OpenPmsDentalDbSettings.exe'='OpenPmsDentalDbSettings\bin\Release'
	'OpenPmsDentalDbSettings.dll'='OpenPmsDentalDbSettings\bin\Release'
	'OpenPmsDentalDbSettings.dll.config'=''
	'OpenPmsDentalDbSettings.runtimeconfig.json'=''
	'OpenPmsDentalDbSettings.deps.json'=''
	#The 32-bit helper program (S3a-13 of the .NET 10 move, docs/port-notes/S3a-13.md): it runs the vendor pieces that exist only as 32-bit code
	#(QuickBooks Desktop's QBFC, and Schick CDR, Topaz and Guru as the fallback of decision 23) for the program, which starts it from Helper32\.
	#Self-contained for win-x86 with its own x86 runtime beside it (decision 20); Helper32\ is the one sub folder of the program folder.
	'Helper32\OpenPmsDentalHelper32.exe'='OpenPmsDentalHelper32\bin\Release'
	'Helper32\OpenPmsDentalHelper32.dll'='OpenPmsDentalHelper32\bin\Release'
	'Helper32\OpenPmsDentalHelper32.runtimeconfig.json'='OpenPmsDentalHelper32\bin\Release'
	'Helper32\OpenPmsDentalHelper32.deps.json'='OpenPmsDentalHelper32\bin\Release'
}
#The one sub folder of the program folder: the helper and its x86 runtime (ForkReleases.HELPER32_FOLDER_NAME).
$HelperFolder='Helper32'

#Third-party files. Each must be named in THIRD-PARTY-NOTICES.txt. Value: where its bytes come from:
#- 'package:<id>': a file of the NuGet package <id> (stage 2 of the .NET 10 move, docs/specs/net10-plan.md). The build copies it into the
#  program's output from the package at the version OpenPmsDental\packages.lock.json names, and the staged file must be byte for byte that
#  package's file, the runtime or native asset the program's restore names (Test-PackageFiles). Nothing of these comes from "Required dlls" any
#  more (S2-4).
#- 'Required dlls': a byte copy of the repository's "Required dlls" file, for the files that have no NuGet package (the plan's "Left in
#  Required dlls after stage 2": the Direct Project; SharpDX and Tao left with TC-9b, docs/port-notes/TC-9b.md).
#- otherwise the project output it is built from.
$ThirdPartyFiles=[ordered]@{
	'BouncyCastle.Cryptography.dll'='package:BouncyCastle.Cryptography'
	#The WPF tooth chart's 3D helpers (TC-4, docs/port-notes/TC-4.md): Helix Toolkit 3.1.2 and the packages it brings that have a .NET Framework file
	#of their own (the .NET Community Toolkit, System.ComponentModel.Annotations); the System.* 4.3.x packages of System.Net.Http 4.3.4 bring none
	#that the build keeps (the .NET Framework's own assemblies are newer), so none of them ships.
	'CommunityToolkit.Common.dll'='package:CommunityToolkit.Common'
	'CommunityToolkit.Diagnostics.dll'='package:CommunityToolkit.Diagnostics'
	'CommunityToolkit.Mvvm.dll'='package:CommunityToolkit.Mvvm'
	'Google.Apis.dll'='package:Google.Apis'
	'Google.Apis.Auth.dll'='package:Google.Apis.Auth'
	'Google.Apis.Core.dll'='package:Google.Apis.Core'
	'Google.Apis.Gmail.v1.dll'='package:Google.Apis.Gmail.v1'
	'Health.Direct.Agent.dll'='Required dlls'
	'Health.Direct.Common.dll'='Required dlls'
	'Health.Direct.ResolverPlugins.dll'='Required dlls'
	'HelixToolkit.dll'='package:HelixToolkit'
	'HelixToolkit.Geometry.dll'='package:HelixToolkit.Geometry'
	'HelixToolkit.Maths.dll'='package:HelixToolkit.Maths'
	'HelixToolkit.Wpf.dll'='package:HelixToolkit.Wpf'
	'MailKit.dll'='package:MailKit'
	'Microsoft.Extensions.DependencyInjection.Abstractions.dll'='package:Microsoft.Extensions.DependencyInjection.Abstractions'
	'Microsoft.Extensions.Logging.Abstractions.dll'='package:Microsoft.Extensions.Logging.Abstractions'
	#Roslyn's compiler, which RdlEngine uses to compile a report's VB <Code> block on .NET (decision 14, S3b-2): MIT, named in the notices.
	'Microsoft.CodeAnalysis.dll'='package:Microsoft.CodeAnalysis.Common'
	'Microsoft.CodeAnalysis.VisualBasic.dll'='package:Microsoft.CodeAnalysis.VisualBasic'
	'Microsoft.Identity.Client.dll'='package:Microsoft.Identity.Client'
	'Microsoft.IdentityModel.Abstractions.dll'='package:Microsoft.IdentityModel.Abstractions'
	'Microsoft.Web.WebView2.Core.dll'='package:Microsoft.Web.WebView2'
	'Microsoft.Web.WebView2.WinForms.dll'='package:Microsoft.Web.WebView2'
	'Microsoft.Web.WebView2.Wpf.dll'='package:Microsoft.Web.WebView2'
	'MimeKit.dll'='package:MimeKit'
	'MySqlConnector.dll'='package:MySqlConnector'
	'Newtonsoft.Json.dll'='package:Newtonsoft.Json'
	'Renci.SshNet.dll'='package:SSH.NET'
	#Packages of .NET's own libraries that are not part of the shared runtime, so a .NET 10 program carries them (S3b-3, S3b-4): their
	#runtimes\win\lib\net10.0 files, the Windows implementations.
	'System.DirectoryServices.AccountManagement.dll'='package:System.DirectoryServices.AccountManagement'
	'System.DirectoryServices.Protocols.dll'='package:System.DirectoryServices.Protocols'
	'System.IO.Ports.dll'='package:System.IO.Ports'
	'System.Management.dll'='package:System.Management'
	'System.ServiceProcess.ServiceController.dll'='package:System.ServiceProcess.ServiceController'
	'System.Speech.dll'='package:System.Speech'
	#The native x64 loader: the package's runtimes\win-x64\native asset, which NuGet copies for the program's win-x64 target and the package's
	#build files copy to the top of the output (Directory.Build.targets).
	'WebView2Loader.dll'='package:Microsoft.Web.WebView2'
	#The charts' library: the community port of Windows Forms DataVisualization for .NET (decision 10), MIT.
	'WinForms.DataVisualization.dll'='package:WinForms.DataVisualization'
	'WinForms.DataVisualization.Utilities.dll'='package:WinForms.DataVisualization'
}
#Package files that the package's own build files copy instead of its restore assets: name to the package id and the file's path in the
#package. The WebView2 package's targets copy its managed .NET files from lib_manual (netcoreapp3.0, the WPF one net5.0-windows10.0.17763.0)
#for a .NET project (WebView2ProjectKind
#managed, S3b-0 item 5); its restore assets for net10.0 are reference files only.
$PackageBuildFiles=@{
	'Microsoft.Web.WebView2.Core.dll'=@('Microsoft.Web.WebView2','lib_manual/netcoreapp3.0/Microsoft.Web.WebView2.Core.dll')
	'Microsoft.Web.WebView2.WinForms.dll'=@('Microsoft.Web.WebView2','lib_manual/netcoreapp3.0/Microsoft.Web.WebView2.WinForms.dll')
	'Microsoft.Web.WebView2.Wpf.dll'=@('Microsoft.Web.WebView2','lib_manual/net5.0-windows10.0.17763.0/Microsoft.Web.WebView2.Wpf.dll')
}
#The program whose restore names the package files: its lock file and the restore's record of the files it copies (MSBuildProjectExtensionsPath
#is obj\ for every project of the repository).
$ProgramLockFile='OpenPmsDental\packages.lock.json'
$ProgramAssetsFile='OpenPmsDental\obj\project.assets.json'
$ThirdPartyFiles['RdlEngine.dll']='RdlEngine\bin\Release'
$ThirdPartyFiles['RdlViewer.dll']='RdlViewer\bin\Release'
#Built here from the upstream source in the git submodules under third-party\ (docs\develop\third-party-source-builds.md): PDFsharp and MigraDoc
#6.5.0 (third-party\PDFsharp, with this project's PdfSharpLicense.cs in place of empira's wrapper around its closed PdfSharp.License.dll,
#which no release ships) and WeCantSpell.Hunspell 7.0.1 (third-party\WeCantSpell.Hunspell).
foreach($name in @('PdfSharp-gdi','PdfSharp.Charting-gdi','PdfSharp.Shared','PdfSharp.System','MigraDoc.DocumentObjectModel','MigraDoc.Rendering-gdi',
	'WeCantSpell.Hunspell'))
{
	$ThirdPartyFiles[$name+'.dll']='third-party\build\'+$name+'\bin\Release'
}

#Files from the repository: the path in the program folder, and the repository-relative source. LICENSE-LGPL-2.1.txt is the licence
#text the LGPL 2.1 asks for with every copy of the libraries the release ships under it (RdlEngine.dll, RdlViewer.dll,
#WeCantSpell.Hunspell.dll and the en_US word list in WpfControlsOD.dll; THIRD-PARTY-NOTICES.txt). PDFsharp and MigraDoc 6.5 are under the
#GNU AGPL version 3, so the program as distributed is conveyed under the GNU GPL version 3 (its own code is "version 2 or later"): both
#texts ship, LICENSE-AGPL-3.0.txt and LICENSE-GPL-3.0.txt (NOTICE).
$DocumentFiles=[ordered]@{
	'LICENSE'='LICENSE'
	'LICENSE-GPL-3.0.txt'='LICENSE-GPL-3.0.txt'
	'LICENSE-AGPL-3.0.txt'='LICENSE-AGPL-3.0.txt'
	'LICENSE-LGPL-2.1.txt'='LICENSE-LGPL-2.1.txt'
	'NOTICE'='NOTICE'
	'THIRD-PARTY-NOTICES.txt'='THIRD-PARTY-NOTICES.txt'
	'db\init\new-database.sql'='db\init\new-database.sql'
	'db\README.md'='db\README.md'
}
$StagedFolders=@('db','db\init','Helper32')

#Files the program cannot run or ship without, and why. They are on the allow-list too; these get a reason when missing.
$RequiredReasons=[ordered]@{
	'WebView2Loader.dll'='Microsoft.Web.WebView2.Core.dll loads it for every WebView2 window (CodeBase/Controls/ODWebView2.cs)'
	'WeCantSpell.Hunspell.dll'='the spell check of every ODtextBox and TextRich (WpfControlsOD/Logic/SpellCheckL.cs) uses it'
	'ODR.dll'='RDL report definitions load it by file name (<CodeModule>ODR.dll</CodeModule>)'
	'LICENSE-LGPL-2.1.txt'='the LGPL 2.1 (sections 1 and 4) asks for its text with every copy of RdlEngine.dll, RdlViewer.dll and WeCantSpell.Hunspell.dll'
	'LICENSE-AGPL-3.0.txt'='the GNU AGPL 3.0 (section 4) asks for its text with every copy of PDFsharp and MigraDoc'
	'LICENSE-GPL-3.0.txt'='the program with PDFsharp and MigraDoc is conveyed under the GNU GPL 3.0, whose section 4 asks for its text with every copy'
	'PdfSharp-gdi.dll'='every PDF the program makes or reads goes through it; it holds the AGPL licence check that PdfSharpSetup.Init satisfies'
}

#The source archive's name and top folder ({0}: the release version). It holds the repository at the release commit and every submodule
#(third-party\PDFsharp, third-party\WeCantSpell.Hunspell) at the commit the release commit pins (git ls-tree HEAD, .gitmodules).
$SourceArchiveName='OpenPmsDental-{0}-source'

#Native files of the program folder: every apphost and native DLL is x64 (PE machine 0x8664, decision 19) and must not need the debug C
#runtime, which may not be redistributed. WebView2Loader.dll is the WebView2 package's runtimes\win-x64\native asset: an x86 or arm64 loader
#of the same package fails here.
$NativeFiles=@('OpenPmsDental.exe','ServiceManager.exe','UpdateFileCopier.exe','OpenPmsDentalDbSettings.exe','Direct2dWrapper.dll','WebView2Loader.dll')
#Files that are x86 whatever the program is (S3a-13): the 32-bit helper program loads vendor code that exists only as 32-bit code, so it stays
#x86 when the program and everything else is x64. PE machine 0x14C, and for a managed file the CLR header's 32BITREQUIRED flag (an AnyCPU file
#has machine 0x14C too, and would run as a 64-bit process on 64-bit Windows); the helper's .exe is a native x86 apphost (no CLR header), its
#.dll the managed assembly.
$X86OnlyFiles=@('Helper32\OpenPmsDentalHelper32.exe','Helper32\OpenPmsDentalHelper32.dll')
$DebugRuntimeNames=@('VCRUNTIME140D.dll','MSVCP140D.dll','ucrtbased.dll','VCRUNTIME140_1D.dll')
#The managed files built here for the x64 program: x64 (PE32+ machine 0x8664) or IL only (machine 0x14C, ILONLY, not 32BITREQUIRED).
$PeMachineX64=0x8664
$PeMachineX86=0x14C

#Files of OpenPmsDental's Release output that are never shipped: symbols and XML documentation (the WebView2 package's too).
$NotShippedPatterns=@('*.pdb','OpenPmsDental.xml','OpenPmsDentalBusiness.xml','CodeBase.xml','Microsoft.Web.WebView2.*.xml')

#ForkReleases.ListBuildIdFiles: the build id is the SHA-256 over these files' lower-case names and SHA-256 values. On .NET the program's code
#is in OpenPmsDental.dll (the .exe is the apphost).
$BuildIdFiles=@('OpenPmsDental.exe','OpenPmsDental.dll','OpenPmsDentalBusiness.dll','CodeBase.dll','DataConnectionBase.dll','WpfControlsOD.dll',
	'OpenPmsDentalGraph.dll','OpenPmsDentalStorage.dll','SparksToothChart.dll','ODCrypt.dll','PasswordVaultWrapper.dll','ImagingDeviceManager.dll',
	'ImagingCapture.dll','UpdateFileCopier.exe')

#The runtime part (decision 20): the runtime packs whose file lists (data\RuntimeList.xml) name the runtime files of the program folder. Each
#entry: the folder in the program folder ('' the top, Helper32 the helper's), the pack id, its runtime identifier, the profiles the program
#there uses ('' every file; WindowsForms: only the files whose Profile names it, as the SDK takes them for a project that uses
#Windows Forms only) and the PE machine of its native files. Resources (the language folders) never ship: SatelliteResourceLanguages=en.
$RuntimePacks=@(
	@('','Microsoft.NETCore.App.Runtime','win-x64','',0x8664),
	@('','Microsoft.WindowsDesktop.App.Runtime','win-x64','',0x8664),
	@('Helper32','Microsoft.NETCore.App.Runtime','win-x86','',0x14C),
	@('Helper32','Microsoft.WindowsDesktop.App.Runtime','win-x86','WindowsForms',0x14C)
)
#The runtime pack whose newest 10.0.x patch in the offline folder a release must have (the monthly runtime release, section 2.4 of the plan).
$RuntimeBandPack='Microsoft.NETCore.App.Runtime.win-x64'
$RuntimeBand='10.0'

#The 64-bit TWAIN data source manager (decision 19, O-16): a file of the setup, which installs it into the 64-bit system folder as a shared
#file when it is missing or older; never a file of the program folder (the deny-list refuses one there). The newest release of the TWAIN
#Working Group's official GitHub releases on 2026-11-16 (v2.5.1, dsm_020501.zip), downloaded under the owner's standing rule; its record is
#od-tools\downloads\s3b7-twaindsm-20261116 (SOURCES.txt, SHA256SUMS.txt, MANIFEST.txt) in the repository's common git folder.
$TwainDsmDownloadFolder='s3b7-twaindsm-20261116'
$TwainDsmZipName='dsm_020501.zip'
$TwainDsmEntry='dsm_020501/windows/64/TWAINDSM.dll'
$TwainDsmSha256='25869f88a2bc76ded62a13aaea9f2084f0165bfdbbb7f46b9c3a43298938d467'
$TwainDsmVersion='2.5.1.0'
$TwainDsmSigner='O=TWAIN Working Group'
#The 32-bit TWAIN data source manager of the same download (owner, docs/decisions/owner-questions.md: "32-bit TWAIN manager: yes"; decision 23, package
#S3b-8): installed into the 32-bit system folder (SysWOW64) as a shared file when it is missing or older, as the 64-bit one, for TWAIN sources
#that have only a 32-bit driver, which run in the 32-bit helper (Helper32\). The same version and signer; its own SHA-256 and PE machine 0x14C.
$TwainDsm32Entry='dsm_020501/windows/32/TWAINDSM.dll'
$TwainDsm32Sha256='cee9a6ff8026dde09eda9f892c57cc928d42e4a4d22bb740fe38ee6e91797a28'
#Setup files, relative to setup-files\ beside the staging folder: the 64-bit DSM, then the 32-bit one in its own sub folder (one name, two files).
$SetupFiles=@('TWAINDSM.dll','x86\TWAINDSM.dll')
#The source of that release (twain-dsm-2.5.1.zip of the same GitHub release, from the same download folder): TWAINDSM.dll is under the
#LGPL 2.1, whose section 4 asks for its source to go with the object code, so the source archive carries it byte for byte as
#third-party-source/twain-dsm-2.5.1.zip (-TwainDsmSourceZip), checked against this SHA-256; a release without it stops.
$TwainDsmSourceZipName='twain-dsm-2.5.1.zip'
$TwainDsmSourceSha256='67e5aebda9c7aa85e39a38b8c9c8b99bd720d4b318134b3e274aec97d4977e0f'
$TwainDsmSourceEntry='third-party-source/twain-dsm-2.5.1.zip'
$TwainDsmSourceUrl='https://github.com/twain/twain-dsm/releases/download/v2.5.1/twain-dsm-2.5.1.zip'
#Read once (Get-OfflineFolder).
$script:OfflineFolderCached=$null

#Never shipped, at any depth (matched against file and folder names, ignoring case), with the reason (report-release.md section 2;
#docs/port-notes/BIN-A.md, BIN-B.md).
$DenyList=@(
	@('xImageDeviceManager.dll',"Open Dental's closed Suni capture library (removed in BIN-A)"),
	@('SuniUSB.dll',"Suni's commercial USB driver that Open Dental shipped (removed in BIN-A)"),
	@('VBbridges.dll',"Open Dental's closed VB.NET bridge helper (removed in BIN-A)"),
	@('VideoCamLib.dll',"Open Dental's closed video capture wrapper (removed in the capture work)"),
	@('Sparks3D*',"Open Dental's Sparks3D tooth chart, under Open Dental's own licence (removed in BIN-A)"),
	@('EZT4*.dll','Atalasoft EZTwain Pro, a commercial per-developer licence'),
	@('Eztwain*','Atalasoft EZTwain Pro, a commercial per-developer licence'),
	@('Bridges.dll',"Open Dental's closed bridge library (removed in the decoupling)"),
	@('CDT.dll',"Open Dental's closed CDT library (removed in the decoupling)"),
	@('OpenDentalHelp.dll',"Open Dental's closed help library (removed in the decoupling)"),
	@('VirtualWeb.dll',"Open Dental's closed browser-hosting library (removed in the decoupling)"),
	@('ODApi.dll',"Open Dental's closed API library (removed in the decoupling)"),
	@('DpsPos.dll',"Open Dental's closed payment terminal library (removed in the decoupling)"),
	@('stdole.dll',"redistributable only through Microsoft's Office PIA installer"),
	@('Interop.*.dll','COM interop generated from a proprietary type library (Office, Adobe, Intuit, Schick)'),
	@('AxInterop.*.dll','ActiveX interop generated from a proprietary type library (Adobe)'),
	@('*.winmd','Windows SDK metadata, part of Windows'),
	@('installutil.exe','part of the .NET Framework'),
	@('vcruntime*.dll','the Visual C++ runtime comes from vc_redist.x64.exe (the .NET runtime files of the runtime packs are not checked against this list)'),
	@('msvcp*.dll','the Visual C++ runtime comes from vc_redist.x64.exe (the .NET runtime files of the runtime packs are not checked against this list)'),
	@('msvcr*.dll','an old Visual C++ runtime, not redistributed with the program'),
	@('ucrtbase*.dll','the Universal C runtime is part of Windows'),
	@('Microsoft.DirectX*.dll',"DirectX, redistributable only through Microsoft's installer"),
	@('d3dx*.dll',"DirectX, redistributable only through Microsoft's installer"),
	@('RS232.dll','unknown author and licence (replaced in BIN-B)'),
	@('SigPlusNET.dll','Topaz: no published redistribution terms; loaded from the Topaz software when installed (BIN-B)'),
	@('zxing*','a dead reference (removed in BIN-B)'),
	@('Avalara*','the AvaTax SDK, a dead reference (removed in BIN-B)'),
	@('Ionic.*','DotNetZip: MS-PL, abandoned, CVE-2024-48510 (replaced in BIN-B)'),
	@('Dicom.*.dll','fo-dicom 4 (MS-PL): replaced by the in-house DICOM reader (docs/specs/dicom-reader.md)'),
	@('NDde.dll','NDde, under its own shared source licence, which does not fit the GPL (REL-FILES 2): the DentX, Dexis Integrator and Vipersoft program links use the in-house DDE client (CodeBase ODDdeClient) instead (S0-R)'),
	@('fo-dicom*','fo-dicom (MS-PL): replaced by the in-house DICOM reader (docs/specs/dicom-reader.md)'),
	@('DotNetZip*','DotNetZip: MS-PL, abandoned, CVE-2024-48510 (replaced in BIN-B)'),
	@('Tamir.SharpSSH.dll','SharpSSH, unmaintained since 2013 (replaced by SSH.NET in the tier 1 library updates)'),
	@('Org.Mentalis.Security.dll',"SharpSSH's cryptography library (removed with SharpSSH)"),
	@('DiffieHellman.dll',"SharpSSH's key exchange library (removed with SharpSSH)"),
	@('OpenPop.dll',"OpenPop.NET, unmaintained (replaced by MailKit's POP3 client in the tier 1 library updates)"),
	@('BouncyCastle.Crypto.dll',"MimeKit 2's Bouncy Castle 1.8; MimeKit 4 uses BouncyCastle.Cryptography.dll"),
	@('Google.Apis*.PlatformServices.dll','the Google APIs 1.44 platform shims; the library has none since 1.50'),
	@('NHunspell.dll','NHunspell 0.9.6, a wrapper of the native Hunspell 1.2 (replaced by the managed WeCantSpell.Hunspell in the library updates)'),
	@('Hunspellx*.dll',"NHunspell's native Hunspell 1.2 (removed with NHunspell; WeCantSpell.Hunspell needs no native file)"),
	@('PdfSharp.License.dll',"empira's closed licence check of PDFsharp 6.5, whose source is not published; the PdfSharp-gdi.dll built here holds the AGPL licence check itself (third-party\build\PdfSharp-gdi\PdfSharpLicense.cs)"),
	@('TWAINDSM.dll',"the TWAIN data source manager is a setup file installed into the Windows system folder; one in the program folder would be loaded first"),
	@('FreeDentalConfig.xml','the connection settings of each computer; the installer writes it, a release never carries it'),
	@('OpenDentalServerConfig.xml','the Middle Tier settings of each server'),
	@('ForkRelease.manifest','written by the release channel on each workstation'),
	@('*.pdb','debug symbols'),
	@('unins*.exe','the uninstaller lives in the uninstall sub folder; a release publishes every top-level file'),
	@('unins*.dat','the uninstaller lives in the uninstall sub folder; a release publishes every top-level file'),
	@('unins*.msg','the uninstaller lives in the uninstall sub folder; a release publishes every top-level file'),
	@('OpenDent*.exe','a file of the pre-rename names; the program is OpenPmsDental.exe'),
	@('OpenDent*.dll','a file of the pre-rename names; the program uses OpenPmsDentalBusiness.dll'),
	#ForkReleases.ListRetiredFiles: files of an earlier release that this one no longer ships (ReleaseFlowTests checks the two lists agree).
	@('OpenPmsDentalCloud.dll','the storage library of v0.0, renamed OpenPmsDentalStorage.dll on 2026-11-02; a leftover of an older build'),
	@('PdfSharp.dll','PDFsharp 1.2 of v0.0; PDFsharp 6.5 ships as PdfSharp-gdi.dll'),
	@('PdfSharp.Charting.dll','PDFsharp 1.2 of v0.0; PDFsharp 6.5 ships as PdfSharp.Charting-gdi.dll'),
	@('MigraDoc.Rendering.dll','MigraDoc 1.2 of v0.0; MigraDoc 6.5 ships as MigraDoc.Rendering-gdi.dll'),
	#SharpDX 4.0.1 and the Tao Framework 2.1 of v0.0, the libraries of the retired DirectX and OpenGL tooth charts (TC-9a): the WPF 3D chart
	#needs neither them nor the DirectX End-User Runtime (TC-9b, docs/port-notes/TC-9b.md).
	@('SharpDX.dll','SharpDX 4.0.1 of v0.0: the library of the retired DirectX tooth chart (TC-9b)'),
	@('SharpDX.Direct3D9.dll','SharpDX 4.0.1 of v0.0: the library of the retired DirectX tooth chart (TC-9b)'),
	@('SharpDX.Mathematics.dll','SharpDX 4.0.1 of v0.0: the library of the retired DirectX tooth chart (TC-9b)'),
	@('Tao.OpenGl.dll','the Tao Framework of v0.0: the library of the retired OpenGL tooth chart (TC-9b)'),
	@('Tao.Platform.Windows.dll','the Tao Framework of v0.0: the library of the retired OpenGL tooth chart (TC-9b)'),
	#The .NET Framework settings files of the programs: on .NET the SDK writes <name>.dll.config instead (S3b-7).
	@('OpenPmsDental.exe.config','the .NET Framework settings file of v0.0; the .NET 10 program reads OpenPmsDental.dll.config'),
	@('ServiceManager.exe.config','the .NET Framework settings file of v0.0; the .NET 10 ServiceManager reads ServiceManager.dll.config'),
	@('OpenPmsDentalDbSettings.exe.config','the .NET Framework settings file of v0.0; the .NET 10 tool reads OpenPmsDentalDbSettings.dll.config'),
	@('OpenPmsDentalHelper32.exe.config','the .NET Framework settings file of the helper; the .NET 10 helper in Helper32\ has none'),
	#Packages of v0.0 whose types are part of .NET 10, so the restore prunes them (S3b-7).
	@('Microsoft.Bcl.AsyncInterfaces.dll','a package of the .NET Framework build whose types are part of .NET 10'),
	@('Microsoft.Bcl.Cryptography.dll','a package of the .NET Framework build whose types are part of .NET 10')
)
#endregion The lists

#region Helpers
function Get-DenyReason([string]$name){
	foreach($entry in $DenyList){
		if($name -like $entry[0]){
			return $entry[1]
		}
	}
	return $null
}

function Get-Sha256([string]$path){
	return (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
}

#The same computation as ForkReleases.ComputeBuildId: the build id files that exist, ordered ignoring case, as "name|sha256\n", hashed.
function Get-BuildId([string]$folder){
	$sorted=New-Object 'System.Collections.Generic.List[string]'
	foreach($name in $BuildIdFiles){ $sorted.Add($name) }
	$sorted.Sort([StringComparer]::OrdinalIgnoreCase)
	$text=New-Object System.Text.StringBuilder
	foreach($name in $sorted){
		$path=Join-Path $folder $name
		if(Test-Path -LiteralPath $path -PathType Leaf){
			[void]$text.Append($name.ToLowerInvariant()+'|'+(Get-Sha256 $path)+"`n")
		}
	}
	$sha=[System.Security.Cryptography.SHA256]::Create()
	try{
		$bytes=$sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($text.ToString()))
	}
	finally{ $sha.Dispose() }
	return (($bytes | ForEach-Object { $_.ToString('x2') }) -join '')
}

#The text of every "Files:" field of THIRD-PARTY-NOTICES.txt, with its continuation lines (the format LicenseList.ParseNotices reads).
function Get-NoticedFilesText([string]$noticesPath){
	$builder=New-Object System.Text.StringBuilder
	$inFiles=$false
	foreach($line in [System.IO.File]::ReadAllLines($noticesPath)){
		if($line -match '^--- '){ $inFiles=$false; continue }
		if($line -match '^([A-Za-z]+):\s*(.*)$'){
			$inFiles=($Matches[1] -eq 'Files')
			if($inFiles){ [void]$builder.Append(' '+$Matches[2]) }
			continue
		}
		if($inFiles -and $line -match '^\s+\S'){
			[void]$builder.Append(' '+$line.Trim())
			continue
		}
		$inFiles=$false
	}
	return $builder.ToString()
}

#The PE machine field of the file (0x14C is x86), or -1 when it is not a PE file.
function Get-PeMachine([string]$path){
	$bytes=[System.IO.File]::ReadAllBytes($path)
	if($bytes.Length -lt 64 -or $bytes[0] -ne 0x4D -or $bytes[1] -ne 0x5A){ return -1 }
	$offset=[BitConverter]::ToInt32($bytes,0x3C)
	if($offset -lt 0 -or $offset+6 -gt $bytes.Length){ return -1 }
	if($bytes[$offset] -ne 0x50 -or $bytes[$offset+1] -ne 0x45 -or $bytes[$offset+2] -ne 0 -or $bytes[$offset+3] -ne 0){ return -1 }
	return [int][BitConverter]::ToUInt16($bytes,$offset+4)
}

#The Flags of the file's CLR header (0x1 ILONLY, 0x2 32BITREQUIRED, 0x20000 32BITPREFERRED), or -1 when it has none (a native file) or is not a
#PE file. Reads the optional header's CLR runtime header directory (number 14) and maps its RVA to the file through the section table.
function Get-ClrFlags([string]$path){
	$bytes=[System.IO.File]::ReadAllBytes($path)
	if((Get-PeMachine $path) -lt 0){ return -1 }
	$offset=[BitConverter]::ToInt32($bytes,0x3C)
	$sectionCount=[BitConverter]::ToUInt16($bytes,$offset+6)
	$optionalSize=[BitConverter]::ToUInt16($bytes,$offset+20)
	$optional=$offset+24
	if($optional+2 -gt $bytes.Length){ return -1 }
	$magic=[BitConverter]::ToUInt16($bytes,$optional)
	if($magic -eq 0x10B){ $directories=$optional+96 }elseif($magic -eq 0x20B){ $directories=$optional+112 }else{ return -1 }
	$entry=$directories+14*8
	if($entry+8 -gt $optional+$optionalSize){ return -1 }
	$rva=[BitConverter]::ToUInt32($bytes,$entry)
	if($rva -eq 0){ return -1 }
	$sections=$optional+$optionalSize
	for($i=0;$i -lt $sectionCount;$i++){
		$section=$sections+$i*40
		$virtualSize=[BitConverter]::ToUInt32($bytes,$section+8)
		$virtualAddress=[BitConverter]::ToUInt32($bytes,$section+12)
		$rawPointer=[BitConverter]::ToUInt32($bytes,$section+20)
		if($rva -ge $virtualAddress -and $rva -lt $virtualAddress+$virtualSize){
			$header=$rawPointer+($rva-$virtualAddress)
			if($header+20 -gt $bytes.Length){ return -1 }
			return [int64][BitConverter]::ToUInt32($bytes,$header+16)
		}
	}
	return -1
}

function Read-ReleaseVersion {
	$text=[System.IO.File]::ReadAllText((Join-Path $RepoRoot 'SharedAssemblyInfo.cs'))
	$match=[regex]::Match($text,'(?m)^\s*\[assembly:\s*AssemblyInformationalVersion\("([^"]+)"\)\]')
	if(-not $match.Success){ throw 'SharedAssemblyInfo.cs has no AssemblyInformationalVersion.' }
	return $match.Groups[1].Value
}

function Read-InstallerVersion {
	$text=[System.IO.File]::ReadAllText((Join-Path $RepoRoot 'installer\OpenPmsDental.iss'))
	$match=[regex]::Match($text,'(?m)^#define AppVersion "([^"]+)"')
	if(-not $match.Success){ throw 'installer\OpenPmsDental.iss has no #define AppVersion.' }
	return $match.Groups[1].Value
}

#The program's own files, the third-party files and the documents, then (with a runtime) the runtime files (Get-RuntimeFiles).
function Get-AllowedPaths($runtime=$null){
	$list=New-Object 'System.Collections.Generic.List[string]'
	foreach($key in $OwnFiles.Keys){ $list.Add($key) }
	foreach($key in $ThirdPartyFiles.Keys){ $list.Add($key) }
	foreach($key in $DocumentFiles.Keys){ $list.Add($key) }
	if($runtime){ foreach($key in $runtime.Keys){ $list.Add($key) } }
	return $list
}

#The offline NuGet folder the restore reads (OPMS_NUGET_OFFLINE, tools\Initialize-BuildEnvironment.ps1), where the runtime packs are.
function Get-OfflineFolder {
	if(-not $script:OfflineFolderCached){
		$script:OfflineFolderCached=[string](Initialize-BuildEnvironment -RepoRoot $RepoRoot).OfflineFolder
	}
	return $script:OfflineFolderCached
}

#The newest 10.0.x version of the band's runtime pack in the offline folder (a version folder of three numbers, no prerelease), or $null.
function Get-NewestRuntimeVersion {
	$offline=Get-OfflineFolder
	if(-not $offline){ return $null }
	$folder=Join-Path $offline $RuntimeBandPack.ToLowerInvariant()
	if(-not (Test-Path -LiteralPath $folder -PathType Container)){ return $null }
	$versions=@(Get-ChildItem -LiteralPath $folder -Directory | Where-Object { $_.Name -match ('^'+[regex]::Escape($RuntimeBand)+'\.\d+$') } |
		ForEach-Object { [version]$_.Name } | Sort-Object)
	if($versions.Count -eq 0){ return $null }
	return $versions[-1].ToString()
}

#The runtime files of the program folder for the runtime version, read from the runtime packs' own file lists ($RuntimePacks: data\RuntimeList.xml
#in the offline folder): an ordered map from the path in the program folder (Helper32\ for the helper's) to its record (the pack files it may
#be, its type, the PE machine of its folder). Resources never ship; a profile limits a pack to the files whose
#Profile attribute names it (WindowsFormsIntegration.dll, which has none, is WPF's and stays out). Throws when a pack or its list is missing (the restore needs them too).
function Get-RuntimeFiles([string]$version){
	$offline=Get-OfflineFolder
	$files=[ordered]@{}
	foreach($pack in $RuntimePacks){
		$packFolder=Join-Path $offline (($pack[1]+'.'+$pack[2]).ToLowerInvariant()+'\'+$version)
		$listPath=Join-Path $packFolder 'data\RuntimeList.xml'
		if(-not (Test-Path -LiteralPath $listPath -PathType Leaf)){
			throw "No runtime pack $($pack[1]).$($pack[2]) $version in the offline folder ($listPath is missing). Fetch it under the monthly runtime approval (section 2.4 of the plan)."
		}
		[xml]$list=[System.IO.File]::ReadAllText($listPath)
		foreach($file in @($list.FileList.File)){
			if($file.Type -eq 'Resources'){ continue }
			if($pack[3]){
				$profileValue=$file.GetAttribute('Profile')
				if(-not $profileValue -or (@($profileValue.Split(";")) -notcontains $pack[3])){ continue }
			}
			$name=$file.Path.Split('/')[-1]
			$relative=if($pack[0]){ $pack[0]+'\'+$name }else{ $name }
			$packFile=Join-Path $packFolder ($file.Path.Replace('/','\'))
			#A name in both packs of a folder (Microsoft.VisualBasic.dll, System.Drawing.dll, WindowsBase.dll: a facade in NETCore, the
			#implementation in WindowsDesktop): the SDK keeps one of them, so the staged file must be one of the two.
			if($files.Contains($relative)){
				$files[$relative].PackFiles+=@($packFile)
				continue
			}
			$files[$relative]=[pscustomobject]@{ PackFiles=@($packFile); Type=$file.Type; Machine=$pack[4] }
		}
	}
	return $files
}

#The runtime packs a .deps.json names (its libraries of type runtimepack): a list of "id/version" names.
function Get-DepsRuntimePacks([string]$path){
	$deps=[System.IO.File]::ReadAllText($path) | ConvertFrom-Json
	$list=New-Object 'System.Collections.Generic.List[string]'
	$libraries=Get-JsonValue $deps 'libraries'
	if(-not $libraries){ return ,$list }
	foreach($library in $libraries.PSObject.Properties){
		if((Get-JsonValue $library.Value 'type') -eq 'runtimepack'){ $list.Add($library.Name) }
	}
	return ,$list
}

#The release-flow checks of the runtime (S3b-7, section 2.4 of the plan): the runtime version the folder's .deps.json files name (Version, the
#program's) and the problems: more than one version among them, a helper on another version than the program, a .runtimeconfig.json whose
#frameworks are of another version, and (unless -AnyVersion) a version older than the newest 10.0.x runtime pack in the offline folder.
function Test-RuntimeVersions([string]$folder,[switch]$AnyVersion){
	$result=[pscustomobject]@{ Version=$null; Problems=(New-Object 'System.Collections.Generic.List[string]') }
	$byFolder=@{ ''=(New-Object 'System.Collections.Generic.List[string]'); $HelperFolder=(New-Object 'System.Collections.Generic.List[string]') }
	$depsFiles=@(Get-ChildItem -LiteralPath $folder -Filter '*.deps.json' -File -ErrorAction SilentlyContinue)
	$helperPath=Join-Path $folder $HelperFolder
	if(Test-Path -LiteralPath $helperPath -PathType Container){ $depsFiles+=@(Get-ChildItem -LiteralPath $helperPath -Filter '*.deps.json' -File) }
	foreach($deps in $depsFiles){
		$key=if($deps.DirectoryName.TrimEnd('\') -eq $helperPath.TrimEnd('\')){ $HelperFolder }else{ '' }
		try{ $packs=Get-DepsRuntimePacks $deps.FullName }
		catch{
			$result.Problems.Add("Not readable: $($deps.Name) is not a .deps.json ($($_.Exception.Message))")
			continue
		}
		foreach($pack in $packs){
			$version=$pack.Split('/')[-1]
			if(-not $byFolder[$key].Contains($version)){ $byFolder[$key].Add($version) }
		}
	}
	$all=@(@($byFolder['']) + @($byFolder[$HelperFolder]) | Sort-Object -Unique)
	if($all.Count -eq 0){
		$result.Problems.Add('No runtime: no .deps.json of the folder names a runtime pack, so it is not a self-contained .NET build.')
		return $result
	}
	if($byFolder[''].Count -gt 1){ $result.Problems.Add("Mixed runtime versions: the programs' .deps.json files name $($byFolder[''] -join ', '). The programs of one folder run on one runtime (decision 20).") }
	if($byFolder[$HelperFolder].Count -gt 1){ $result.Problems.Add("Mixed runtime versions in ${HelperFolder}: $($byFolder[$HelperFolder] -join ', ').") }
	if($byFolder[''].Count -ge 1){ $result.Version=$byFolder[''][0] }else{ $result.Version=$byFolder[$HelperFolder][0] }
	if($byFolder[$HelperFolder].Count -eq 0){ $result.Problems.Add("No helper runtime: $HelperFolder\ has no .deps.json that names a runtime pack.") }
	elseif(@($byFolder[$HelperFolder] | Where-Object { $_ -ne $result.Version }).Count -gt 0){
		$result.Problems.Add("A different helper runtime: the helper in $HelperFolder\ runs on .NET $($byFolder[$HelperFolder] -join ', '), the program on .NET $($result.Version). Both come from one SDK and its runtime packs.")
	}
	#Every .runtimeconfig.json (the helper's too) includes the frameworks of that one version.
	$configs=@(Get-ChildItem -LiteralPath $folder -Filter '*.runtimeconfig.json' -File -ErrorAction SilentlyContinue)
	if(Test-Path -LiteralPath $helperPath -PathType Container){ $configs+=@(Get-ChildItem -LiteralPath $helperPath -Filter '*.runtimeconfig.json' -File) }
	foreach($config in $configs){
		try{ $options=Get-JsonValue ([System.IO.File]::ReadAllText($config.FullName) | ConvertFrom-Json) 'runtimeOptions' }
		catch{
			$result.Problems.Add("Not readable: $($config.Name) is not a .runtimeconfig.json ($($_.Exception.Message))")
			continue
		}
		$frameworks=@(Get-JsonValue $options 'includedFrameworks')
		if($frameworks.Count -eq 0 -or $null -eq $frameworks[0]){
			$result.Problems.Add("Not self-contained: $($config.Name) includes no framework.")
			continue
		}
		foreach($framework in $frameworks){
			if((Get-JsonValue $framework 'version') -ne $result.Version){
				$result.Problems.Add("Mixed runtime versions: $($config.Name) includes $((Get-JsonValue $framework 'name')) $((Get-JsonValue $framework 'version')), the program's runtime is $($result.Version).")
			}
		}
	}
	if(-not $AnyVersion){
		$newest=Get-NewestRuntimeVersion
		if(-not $newest){ $result.Problems.Add("No $RuntimeBandPack $RuntimeBand.x in the offline folder ($(Get-OfflineFolder)), so the runtime cannot be checked.") }
		elseif([version]$result.Version -lt [version]$newest){
			$result.Problems.Add("An older runtime: the release runs on .NET $($result.Version), but the offline folder has $RuntimeBandPack $newest. Do the monthly runtime bump first (section 2.4 of docs\specs\net10-plan.md), then build again.")
		}
	}
	return $result
}

#Whether a PE file fits its folder: at the top x64 (machine 0x8664) or IL only (machine 0x14C, ILONLY, not 32BITREQUIRED); in Helper32\ x86
#(0x14C, native or managed). Returns a problem or $null; a file that is not a PE file (json, config) fits.
function Test-PeFile([string]$path,[string]$relative){
	$machine=Get-PeMachine $path
	if($machine -lt 0){ return $null }
	$clrFlags=Get-ClrFlags $path
	if($relative.StartsWith($HelperFolder+'\',[StringComparison]::OrdinalIgnoreCase)){
		if($machine -ne $PeMachineX86){ return ("Not an x86 file in ${HelperFolder}: {0} (PE machine 0x{1:X}); the helper and its runtime are x86" -f $relative,$machine) }
		return $null
	}
	if($machine -eq $PeMachineX64){ return $null }
	if($clrFlags -ge 0 -and $machine -eq $PeMachineX86 -and ($clrFlags -band 0x1) -ne 0 -and ($clrFlags -band 0x2) -eq 0){ return $null }
	if($clrFlags -lt 0){ return ("Not an x64 file: {0} (PE machine 0x{1:X}); every native file and apphost of the program folder is x64 (decision 19)" -f $relative,$machine) }
	return ("Not an x64 or IL-only file: {0} (PE machine 0x{1:X}, CLR flags 0x{2:X})" -f $relative,$machine,$clrFlags)
}

#The repository's common git folder (for a worktree, the main repository's .git), where od-tools\downloads keeps the approved downloads.
function Get-GitCommonFolder {
	$git=Find-Git
	if(-not $git){ return $null }
	$output=@(& $git -C $RepoRoot -c (Get-SafeDirectoryArgument $RepoRoot) rev-parse --git-common-dir 2>$null)
	if($LASTEXITCODE -ne 0 -or $output.Count -eq 0){ return $null }
	$path=$output[0].Trim()
	if(-not [System.IO.Path]::IsPathRooted($path)){ $path=Join-Path $RepoRoot $path }
	return (Resolve-Path -LiteralPath $path).Path
}

#A TWAINDSM.dll from the approved download (-TwainDsmZip) after its checks: the pinned SHA-256, the file version, the PE machine, and a valid
#Authenticode signature of the TWAIN Working Group. -Bits 64 (the default): the 64-bit one, written to <folder>\TWAINDSM.dll, PE machine 0x8664.
#-Bits 32: the 32-bit one (S3b-8), written to <folder>\x86\TWAINDSM.dll, PE machine 0x14C. Returns Path and Problems.
function Get-TwainDsm([string]$folder,[int]$Bits=64){
	$result=[pscustomobject]@{ Path=$null; Problems=(New-Object 'System.Collections.Generic.List[string]') }
	if($Bits -eq 32){
		$entryName=$TwainDsm32Entry; $shaApproved=$TwainDsm32Sha256; $machineApproved=$PeMachineX86; $label='the 32-bit TWAINDSM.dll'
		$folder=Join-Path $folder 'x86'
	}
	else{
		$entryName=$TwainDsmEntry; $shaApproved=$TwainDsmSha256; $machineApproved=$PeMachineX64; $label='TWAINDSM.dll'
	}
	$zip=$TwainDsmZip
	if(-not $zip){
		$common=Get-GitCommonFolder
		if($common){ $zip=Join-Path $common "od-tools\downloads\$TwainDsmDownloadFolder\$TwainDsmZipName" }
	}
	if(-not $zip -or -not (Test-Path -LiteralPath $zip -PathType Leaf)){
		$result.Problems.Add("No approved TWAIN DSM download at '$zip' (od-tools\downloads\$TwainDsmDownloadFolder in the common git folder, or -TwainDsmZip).")
		return $result
	}
	Add-Type -AssemblyName System.IO.Compression
	Add-Type -AssemblyName System.IO.Compression.FileSystem
	New-Item -ItemType Directory -Force -Path $folder | Out-Null
	$target=Join-Path $folder 'TWAINDSM.dll'
	$archive=[System.IO.Compression.ZipFile]::OpenRead($zip)
	try{
		$entry=$archive.GetEntry($entryName)
		if(-not $entry){
			$result.Problems.Add("The TWAIN DSM download $zip has no $entryName.")
			return $result
		}
		[System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry,$target,$true)
	}
	finally{ $archive.Dispose() }
	$sha=Get-Sha256 $target
	if($sha -ne $shaApproved){ $result.Problems.Add("$label is not the approved download: SHA-256 $sha, approved $shaApproved.") }
	$machine=Get-PeMachine $target
	if($machine -ne $machineApproved){ $result.Problems.Add(("{0} is not the {1}-bit DSM (PE machine 0x{2:X})." -f $label,$Bits,$machine)) }
	$fileVersion=[System.Diagnostics.FileVersionInfo]::GetVersionInfo($target).FileVersion
	if($fileVersion -ne $TwainDsmVersion){ $result.Problems.Add("$label is version $fileVersion, the approved download $TwainDsmVersion.") }
	$signature=Get-AuthenticodeSignature -LiteralPath $target
	$subject=if($signature.SignerCertificate){ $signature.SignerCertificate.Subject }else{ '' }
	if($signature.Status -ne 'Valid' -or $subject -notlike "*$TwainDsmSigner*"){
		$result.Problems.Add("$label has no valid signature of the TWAIN Working Group (status $($signature.Status), signer '$subject').")
	}
	if($result.Problems.Count -eq 0){ $result.Path=$target }
	return $result
}

#The approved download of the TWAIN DSM release's source (-TwainDsmSourceZip; by default twain-dsm-2.5.1.zip in the TWAIN DSM download
#folder), checked against its pinned SHA-256. Returns Path (only when it passes), Missing and Problems.
function Find-TwainDsmSource {
	$result=[pscustomobject]@{ Path=$null; Missing=$false; Problems=(New-Object 'System.Collections.Generic.List[string]') }
	$zip=$TwainDsmSourceZip
	if(-not $zip){
		$common=Get-GitCommonFolder
		if($common){ $zip=Join-Path $common "od-tools\downloads\$TwainDsmDownloadFolder\$TwainDsmSourceZipName" }
	}
	if(-not $zip -or -not (Test-Path -LiteralPath $zip -PathType Leaf)){
		$result.Missing=$true
		$result.Problems.Add("Missing: the source of TWAINDSM.dll, no approved $TwainDsmSourceZipName at '$zip' (od-tools\downloads\$TwainDsmDownloadFolder in the common git folder, or -TwainDsmSourceZip). The LGPL 2.1 asks for it with the release.")
		return $result
	}
	$sha=Get-Sha256 $zip
	if($sha -ne $TwainDsmSourceSha256){
		$result.Problems.Add("$zip is not the approved source of TWAINDSM.dll: SHA-256 $sha, approved $TwainDsmSourceSha256.")
		return $result
	}
	$result.Path=(Resolve-Path -LiteralPath $zip).Path
	return $result
}

#The setup's two pages, written into the folder: setup-info.txt (the information page: installer\setup-info.txt with NOTICE) and
#setup-license.txt (the License Agreement page: installer\setup-license.txt, which says how the installed program is licensed, with
#LICENSE-GPL-3.0.txt, the license the program is conveyed under as installed). UTF-8 with a byte order mark and CRLF line ends, as Inno
#Setup reads them. Returns the two paths.
function Write-SetupTexts([string]$folder,[string]$version){
	New-Item -ItemType Directory -Force -Path $folder | Out-Null
	$utf8Bom=New-Object System.Text.UTF8Encoding($true)
	$notice=[System.IO.File]::ReadAllText((Join-Path $RepoRoot 'NOTICE'))
	$info=[System.IO.File]::ReadAllText((Join-Path $RepoRoot 'installer\setup-info.txt'))
	$info=$info.Replace('@VERSION@',$version).Replace('@NOTICE@',$notice.TrimEnd())
	$info=($info -replace "`r`n","`n") -replace "`n","`r`n"
	$infoPath=Join-Path $folder 'setup-info.txt'
	[System.IO.File]::WriteAllText($infoPath,$info,$utf8Bom)
	$gpl3=[System.IO.File]::ReadAllText((Join-Path $RepoRoot $DocumentFiles['LICENSE-GPL-3.0.txt']))
	$license=[System.IO.File]::ReadAllText((Join-Path $RepoRoot 'installer\setup-license.txt'))
	$license=$license.Replace('@VERSION@',$version).Replace('@GPL3@',$gpl3.Trim([char[]]"`r`n"))
	$license=($license -replace "`r`n","`n") -replace "`n","`r`n"
	$licensePath=Join-Path $folder 'setup-license.txt'
	[System.IO.File]::WriteAllText($licensePath,$license,$utf8Bom)
	return @($infoPath,$licensePath)
}

#The value of the object's property, or $null when it has none (Set-StrictMode refuses a missing property).
function Get-JsonValue($object,[string]$name){
	if($null -eq $object){ return $null }
	$property=$object.PSObject.Properties[$name]
	if($property){ return $property.Value }
	return $null
}

#The NuGet package files the build copies into the program's output, by file name (ignoring case): for each, the package id and version, the
#asset (its path in the package, with '/'), the content hash OpenPmsDental\packages.lock.json names for that package, and the package's folders
#the file may be compared with: the global packages folder the restore used (project.assets.json, packageFolders) and the offline package folder
#nuget.config restores from (OPMS_NUGET_OFFLINE, tools\Initialize-BuildEnvironment.ps1). The assets are the runtime and native ones of the
#target the SDK copies from: the program's win-x64 target (its lib folder for net10.0 or older .NET, runtimes\win-x64\native, and a package's own
#runtimes\win\lib file where it has one). A file the build then leaves out because the shared runtime has that assembly (a package the restore
#did not prune) is named here too; no such file is on the allow-list.
#Returns Files (name to record) and Problems (what could not be read).
function Get-ProgramPackageFiles {
	$result=[pscustomobject]@{ Files=@{}; Problems=(New-Object 'System.Collections.Generic.List[string]') }
	$lockPath=Join-Path $RepoRoot $ProgramLockFile
	$assetsPath=Join-Path $RepoRoot $ProgramAssetsFile
	if(-not (Test-Path -LiteralPath $lockPath -PathType Leaf)){
		$result.Problems.Add("No lock file: $ProgramLockFile")
		return $result
	}
	if(-not (Test-Path -LiteralPath $assetsPath -PathType Leaf)){
		$result.Problems.Add("No restore of the program: $ProgramAssetsFile is missing, so the files the build copies from each package are unknown (build OpenDental.Local.slnf first).")
		return $result
	}
	#The lock file: each package once, by id (NuGet's ids ignore case, as a PowerShell hashtable does).
	$locked=@{}
	$lock=[System.IO.File]::ReadAllText($lockPath) | ConvertFrom-Json
	foreach($target in (Get-JsonValue $lock 'dependencies').PSObject.Properties){
		foreach($entry in $target.Value.PSObject.Properties){
			$type=Get-JsonValue $entry.Value 'type'
			if($type -eq 'Project'){ continue }
			$version=Get-JsonValue $entry.Value 'resolved'
			$contentHash=Get-JsonValue $entry.Value 'contentHash'
			if(-not $version -or -not $contentHash){
				$result.Problems.Add("$ProgramLockFile names the package $($entry.Name) without a resolved version and content hash.")
				continue
			}
			if($locked.Contains($entry.Name) -and $locked[$entry.Name].Version -ne $version){
				$result.Problems.Add("$ProgramLockFile names the package $($entry.Name) at $($locked[$entry.Name].Version) and at $version.")
				continue
			}
			$locked[$entry.Name]=[pscustomobject]@{ Id=$entry.Name; Version=$version; ContentHash=$contentHash }
		}
	}
	$assets=[System.IO.File]::ReadAllText($assetsPath) | ConvertFrom-Json
	$packageFolders=@()
	$folders=Get-JsonValue $assets 'packageFolders'
	if($folders){ $packageFolders=@($folders.PSObject.Properties | ForEach-Object { $_.Name }) }
	$offline=(Initialize-BuildEnvironment -RepoRoot $RepoRoot).OfflineFolder
	$libraries=Get-JsonValue $assets 'libraries'
	#The target the build copies from: the program's win-x64 target (the SDK's ResolvePackageAssets takes the target of the project's runtime
	#identifier when it has one, as the x64 exe does), else the framework's. A package can have other files there: System.Net.Http 4.3.4 had
	#lib\net46 for net48 and runtimes\win\lib\net46 for win-x86 (TC-4), neither of which the .NET Framework build kept.
	$targets=@((Get-JsonValue $assets 'targets').PSObject.Properties)
	$ridTargets=@($targets | Where-Object { $_.Name.Contains('/') })
	if($ridTargets.Count -gt 0){ $targets=$ridTargets }
	foreach($target in $targets){
		foreach($library in $target.Value.PSObject.Properties){
			if((Get-JsonValue $library.Value 'type') -ne 'package'){ continue }
			$parts=$library.Name.Split('/')
			$id=$parts[0]
			$version=$parts[1]
			$entry=$locked[$id]
			if(-not $entry){
				$result.Problems.Add("The restore ($ProgramAssetsFile) has the package $($library.Name), which $ProgramLockFile does not name.")
				continue
			}
			if($entry.Version -ne $version){
				$result.Problems.Add("The restore ($ProgramAssetsFile) has the package $($library.Name), but $ProgramLockFile names $($entry.Version).")
				continue
			}
			$relative=Get-JsonValue (Get-JsonValue $libraries $library.Name) 'path'
			if(-not $relative){ $relative=$id.ToLowerInvariant()+'/'+$version.ToLowerInvariant() }
			foreach($section in @('runtime','native')){
				$sectionValue=Get-JsonValue $library.Value $section
				if(-not $sectionValue){ continue }
				foreach($asset in @($sectionValue.PSObject.Properties | ForEach-Object { $_.Name })){
					$name=$asset.Split('/')[-1]
					if($name -eq '_._'){ continue }
					if($result.Files.Contains($name)){
						if($result.Files[$name].Id -ne $entry.Id -or $result.Files[$name].Asset -ne $asset){
							$result.Problems.Add("Two package files named ${name}: $($result.Files[$name].Id) $($result.Files[$name].Asset) and $($entry.Id) $asset.")
						}
						continue
					}
					$restored=@($packageFolders | ForEach-Object { Join-Path $_ ($relative.Replace('/','\')) })
					$result.Files[$name]=[pscustomobject]@{ Id=$entry.Id; Version=$entry.Version; ContentHash=$entry.ContentHash; Asset=$asset
						RestoredFolders=$restored
						OfflineFolder=$(if($offline){ Join-Path $offline ($entry.Id.ToLowerInvariant()+'\'+$entry.Version.ToLowerInvariant()) }else{ '' }) }
				}
			}
		}
	}
	#Files a package's own build files copy rather than its restore assets ($PackageBuildFiles): the locked package and the file's path in it.
	foreach($name in $PackageBuildFiles.Keys){
		if($result.Files.Contains($name)){ continue }
		$id=$PackageBuildFiles[$name][0]
		$entry=$locked[$id]
		if(-not $entry){ continue }
		$relative=$id.ToLowerInvariant()+'/'+$entry.Version.ToLowerInvariant()
		$restored=@($packageFolders | ForEach-Object { Join-Path $_ ($relative.Replace('/','\')) })
		$result.Files[$name]=[pscustomobject]@{ Id=$entry.Id; Version=$entry.Version; ContentHash=$entry.ContentHash; Asset=$PackageBuildFiles[$name][1]
			RestoredFolders=$restored
			OfflineFolder=$(if($offline){ Join-Path $offline ($entry.Id.ToLowerInvariant()+'\'+$entry.Version.ToLowerInvariant()) }else{ '' }) }
	}
	return $result
}

#Whether each staged third-party file of a NuGet package ('package:<id>' in $ThirdPartyFiles) is byte for byte the package's file: the asset
#the program's restore names for that file name, of the package the list names, at the version OpenPmsDental\packages.lock.json names. The
#file is compared with the package in the global packages folder the restore used, whose .nupkg.metadata must hold the lock file's content
#hash, and with the package in the offline folder, wherever the package is; at least one of the two must hold it. Returns the problems found.
function Test-PackageFiles([string]$folder){
	$problems=New-Object 'System.Collections.Generic.List[string]'
	$packageFiles=$null
	foreach($name in $ThirdPartyFiles.Keys){
		$source=$ThirdPartyFiles[$name]
		if(-not $source.StartsWith('package:')){ continue }
		$staged=Join-Path $folder $name
		if(-not (Test-Path -LiteralPath $staged -PathType Leaf)){ continue }
		if($null -eq $packageFiles){
			$packageFiles=Get-ProgramPackageFiles
			foreach($problem in $packageFiles.Problems){ $problems.Add($problem) }
		}
		$id=$source.Substring('package:'.Length)
		$record=$packageFiles.Files[$name]
		if(-not $record){
			$problems.Add("No package for: $name (the list names the NuGet package $id, but the program's restore copies no file of that name from any package)")
			continue
		}
		if($record.Id -ne $id){
			$problems.Add("Another package: $name comes from the NuGet package $($record.Id) $($record.Version), but the list names $id")
			continue
		}
		$stagedSha=Get-Sha256 $staged
		$compared=New-Object 'System.Collections.Generic.List[string]'
		$differing=New-Object 'System.Collections.Generic.List[string]'
		foreach($packageFolder in $record.RestoredFolders){
			$path=Join-Path $packageFolder ($record.Asset.Replace('/','\'))
			if(-not (Test-Path -LiteralPath $path -PathType Leaf)){ continue }
			$metadataPath=Join-Path $packageFolder '.nupkg.metadata'
			$contentHash=$null
			if(Test-Path -LiteralPath $metadataPath -PathType Leaf){
				$contentHash=Get-JsonValue ([System.IO.File]::ReadAllText($metadataPath) | ConvertFrom-Json) 'contentHash'
			}
			if($contentHash -ne $record.ContentHash){
				$problems.Add("Not the locked package: $packageFolder has the content hash $contentHash, $ProgramLockFile names $($record.ContentHash) for $($record.Id) $($record.Version)")
				continue
			}
			$compared.Add($path)
			if((Get-Sha256 $path) -ne $stagedSha){ $differing.Add($path) }
		}
		if($record.OfflineFolder -and (Test-Path -LiteralPath $record.OfflineFolder -PathType Container)){
			$path=Join-Path $record.OfflineFolder ($record.Asset.Replace('/','\'))
			if(Test-Path -LiteralPath $path -PathType Leaf){
				$compared.Add($path)
				if((Get-Sha256 $path) -ne $stagedSha){ $differing.Add($path) }
			}
			else{
				$problems.Add("Not in the package: the offline folder's $($record.Id) $($record.Version) has no $($record.Asset)")
			}
		}
		if($differing.Count -gt 0){
			$stagedVersion=[System.Diagnostics.FileVersionInfo]::GetVersionInfo($staged).FileVersion
			$packageVersion=[System.Diagnostics.FileVersionInfo]::GetVersionInfo($differing[0]).FileVersion
			if(-not $stagedVersion){ $stagedVersion='none' }
			if(-not $packageVersion){ $packageVersion='none' }
			$problems.Add("Not the package's file: $name (file version $stagedVersion) differs from $($record.Asset) of the NuGet package $($record.Id) $($record.Version) (file version $packageVersion), the version $ProgramLockFile names: $($differing -join '; ')")
		}
		if($compared.Count -eq 0){
			$problems.Add("Package not found: $($record.Id) $($record.Version) is neither in the global packages folder the restore used ($($record.RestoredFolders -join ', ')) nor in the offline folder ($($record.OfflineFolder)), so $name cannot be compared with it")
		}
	}
	return ,$problems
}

#The checks of a staging folder. -Full adds the checks that need the real build: built-from-source copies, package files, runtime files, the
#release-flow checks of the runtime, PE machines, native files, versions. The runtime list is that of the folder's runtime version with -Full
#(Test-RuntimeVersions), otherwise that of the newest 10.0.x runtime in the offline folder (the one a release must have).
#Returns the problems found; none means the folder may ship.
function Test-StagingFolder([string]$folder,[switch]$Full){
	$problems=New-Object 'System.Collections.Generic.List[string]'
	$folder=(Resolve-Path -LiteralPath $folder).Path.TrimEnd('\')
	$runtimeVersion=$null
	if($Full){
		$runtimeCheck=Test-RuntimeVersions $folder
		foreach($problem in $runtimeCheck.Problems){ $problems.Add($problem) }
		$runtimeVersion=$runtimeCheck.Version
	}
	if(-not $runtimeVersion){ $runtimeVersion=Get-NewestRuntimeVersion }
	$runtime=[ordered]@{}
	if($runtimeVersion){
		try{ $runtime=Get-RuntimeFiles $runtimeVersion }
		catch{ $problems.Add("The runtime list: $($_.Exception.Message)") }
	}
	else{ $problems.Add("No $RuntimeBandPack $RuntimeBand.x in the offline folder ($(Get-OfflineFolder)), so the runtime files cannot be listed.") }
	$allowed=Get-AllowedPaths $runtime
	$allowedSet=New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
	foreach($path in $allowed){ [void]$allowedSet.Add($path) }
	$folderSet=New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
	foreach($path in $StagedFolders){ [void]$folderSet.Add($path) }
	#The lists themselves: nothing allowed may be denied (the runtime packs' own files are Microsoft's runtime and not looked up here: the
	#WindowsDesktop runtime carries its own vcruntime140_cor3.dll), and no file is on two lists.
	foreach($path in Get-AllowedPaths){
		$reason=Get-DenyReason (Split-Path $path -Leaf)
		if($reason){ $problems.Add("The allow-list names a denied file: $path ($reason)") }
		if($runtime.Contains($path)){ $problems.Add("On the runtime list and on another list: $path") }
	}
	foreach($item in Get-ChildItem -LiteralPath $folder -Recurse -Force){
		$relative=$item.FullName.Substring($folder.Length).TrimStart('\')
		if($runtime.Contains($relative)){ continue }
		$reason=Get-DenyReason $item.Name
		if($item.PSIsContainer){
			if($reason){ $problems.Add("Denied folder: $relative ($reason)") }
			elseif(-not $folderSet.Contains($relative)){ $problems.Add("Unexpected folder: $relative") }
			continue
		}
		if($reason){ $problems.Add("Denied file: $relative ($reason)") }
		elseif(-not $allowedSet.Contains($relative)){ $problems.Add("Not on the allow-list: $relative") }
	}
	foreach($path in $allowed){
		if(-not (Test-Path -LiteralPath (Join-Path $folder $path) -PathType Leaf)){
			if($RequiredReasons.Contains($path)){ $problems.Add("Missing: $path ($($RequiredReasons[$path]))") }
			else{ $problems.Add("Missing: $path") }
		}
	}
	#Every third-party file is named in the notices file that ships with it.
	$noticesPath=Join-Path $folder 'THIRD-PARTY-NOTICES.txt'
	if(Test-Path -LiteralPath $noticesPath -PathType Leaf){
		$noticed=Get-NoticedFilesText $noticesPath
		foreach($name in $ThirdPartyFiles.Keys){
			if(-not [regex]::IsMatch($noticed,'(?<![\w.])'+[regex]::Escape($name)+'(?![\w.])',[System.Text.RegularExpressions.RegexOptions]::IgnoreCase)){
				$problems.Add("Not named in THIRD-PARTY-NOTICES.txt: $name")
			}
		}
	}
	if(-not $Full){
		return ,$problems
	}
	#Built from this repository, or byte copies of the repository's own files (repository-relative source paths). The files of NuGet packages
	#are compared with their packages (Test-PackageFiles).
	$sources=[ordered]@{}
	foreach($key in $OwnFiles.Keys){ if($OwnFiles[$key]){ $sources[$key]=$OwnFiles[$key]+'\'+(Split-Path $key -Leaf) } }
	foreach($key in $ThirdPartyFiles.Keys){
		if(-not $ThirdPartyFiles[$key].StartsWith('package:')){ $sources[$key]=$ThirdPartyFiles[$key]+'\'+$key }
	}
	foreach($key in $DocumentFiles.Keys){ $sources[$key]=$DocumentFiles[$key] }
	foreach($name in $sources.Keys){
		$staged=Join-Path $folder $name
		$source=Join-Path $RepoRoot $sources[$name]
		if(-not (Test-Path -LiteralPath $staged -PathType Leaf)){ continue }
		if(-not (Test-Path -LiteralPath $source -PathType Leaf)){
			$problems.Add("No source to compare with: $name (expected $($sources[$name]))")
		}
		elseif((Get-Sha256 $staged) -ne (Get-Sha256 $source)){
			$problems.Add("Not the file of this repository: $name differs from $($sources[$name])")
		}
	}
	$packageProblems=Test-PackageFiles $folder
	foreach($problem in $packageProblems){ $problems.Add($problem) }
	#Every runtime file is byte for byte its runtime pack's file, the pack in the offline folder at the runtime version.
	foreach($relative in $runtime.Keys){
		$staged=Join-Path $folder $relative
		if(-not (Test-Path -LiteralPath $staged -PathType Leaf)){ continue }
		$record=$runtime[$relative]
		$missingPackFiles=@($record.PackFiles | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
		if($missingPackFiles.Count -gt 0){ $problems.Add("Not in its runtime pack: $relative ($($missingPackFiles -join ', ') is missing)"); continue }
		$stagedSha=Get-Sha256 $staged
		if(@($record.PackFiles | Where-Object { (Get-Sha256 $_) -eq $stagedSha }).Count -eq 0){
			$problems.Add("Not the runtime pack's file: $relative differs from $($record.PackFiles -join ' and ')")
		}
	}
	#The PE machine of every file of the folder: x64 or IL only at the top (the apphosts, the native DLLs and the x64 runtime 0x8664), x86 in
	#Helper32\ (the helper and its runtime 0x14C); an IL-only file is checked as IL only (decision 19).
	foreach($item in Get-ChildItem -LiteralPath $folder -Recurse -File -Force){
		$relative=$item.FullName.Substring($folder.Length).TrimStart('\')
		if($relative.StartsWith('db\',[StringComparison]::OrdinalIgnoreCase)){ continue }
		$problem=Test-PeFile $item.FullName $relative
		if($problem){ $problems.Add($problem) }
	}
	foreach($name in $NativeFiles){
		$path=Join-Path $folder $name
		if(-not (Test-Path -LiteralPath $path -PathType Leaf)){ continue }
		$machine=Get-PeMachine $path
		if($machine -ne $PeMachineX64){ $problems.Add(("Not an x64 file: {0} (PE machine 0x{1:X})" -f $name,$machine)) }
		$text=[System.Text.Encoding]::GetEncoding(28591).GetString([System.IO.File]::ReadAllBytes($path))
		foreach($debugName in $DebugRuntimeNames){
			if($text.IndexOf($debugName,[StringComparison]::OrdinalIgnoreCase) -ge 0){
				$problems.Add("Needs the debug C runtime ($debugName), which may not be redistributed: $name (a Debug build?)")
			}
		}
	}
	foreach($name in $X86OnlyFiles){
		$path=Join-Path $folder $name
		if(-not (Test-Path -LiteralPath $path -PathType Leaf)){ continue }
		$machine=Get-PeMachine $path
		if($machine -ne $PeMachineX86){
			$problems.Add(("Not an x86 file: {0} (PE machine 0x{1:X}); the 32-bit helper program stays x86" -f $name,$machine))
			continue
		}
		$clrFlags=Get-ClrFlags $path
		if($clrFlags -ge 0 -and ($clrFlags -band 0x2) -eq 0){
			$problems.Add(("Not an x86 file: {0} (an AnyCPU assembly, CLR flags 0x{1:X}, runs as a 64-bit process); the 32-bit helper program stays x86" -f $name,$clrFlags))
		}
	}
	$releaseVersion=Read-ReleaseVersion
	foreach($name in @('OpenPmsDental.exe','OpenPmsDental.dll','OpenPmsDentalBusiness.dll')){
		$path=Join-Path $folder $name
		if(-not (Test-Path -LiteralPath $path -PathType Leaf)){ continue }
		$info=[System.Diagnostics.FileVersionInfo]::GetVersionInfo($path)
		if($info.FileVersion -ne $CompatVersion){ $problems.Add("$name file version is $($info.FileVersion), not the compatibility version $CompatVersion") }
		if($info.ProductVersion -ne $releaseVersion){ $problems.Add("$name product version is $($info.ProductVersion), not the release version $releaseVersion") }
		if($info.ProductName -ne 'Open PMS - Dental'){ $problems.Add("$name product name is $($info.ProductName)") }
	}
	return ,$problems
}

function Write-Problems([System.Collections.Generic.List[string]]$problems,[string]$what){
	Write-Host "$what failed, $($problems.Count) problem(s):"
	foreach($problem in $problems){ Write-Host "  $problem" }
}

function Write-Sums([string]$sumsPath,[string[]]$lines){
	$utf8=New-Object System.Text.UTF8Encoding($false)
	[System.IO.File]::WriteAllText($sumsPath,(($lines -join "`n")+"`n"),$utf8)
}

#The Build Tools 2026 (Visual Studio 2026) MSBuild 18, never the newest instance whatever its version; $null when there is none.
function Find-MSBuild {
	return Get-BuildToolPath -Tool MSBuild
}

#safe.directory=<folder> for git's -c, so that git works in this checkout (and its submodules) when another Windows account owns the folder
#("detected dubious ownership"). It trusts only the folder the script builds from, for that one git call; the global git configuration is
#not changed.
function Get-SafeDirectoryArgument([string]$folder){
	return 'safe.directory='+(([System.IO.Path]::GetFullPath($folder)).TrimEnd('\') -replace '\\','/')
}

#git from the PATH, or $null.
function Find-Git {
	$command=Get-Command 'git.exe' -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
	if($command){ return $command.Source }
	return $null
}

#Runs git in the folder and returns its output lines. Throws with git's own message when git fails.
function Invoke-Git([string]$folder,[string[]]$arguments){
	$saved=$ErrorActionPreference
	$ErrorActionPreference='Continue'
	try{
		$output=@(& $script:GitPath -C $folder -c (Get-SafeDirectoryArgument $folder) @arguments 2>&1 | ForEach-Object { "$_" })
		$exitCode=$LASTEXITCODE
	}
	finally{ $ErrorActionPreference=$saved }
	if($exitCode -ne 0){ throw "git $($arguments -join ' ') failed in ${folder}: $($output -join ' ')" }
	return $output
}

#The submodules the release commit pins: their path, the pinned commit (the gitlinks of git ls-tree HEAD) and their address in the
#release commit's .gitmodules.
function Get-PinnedSubmodules {
	$list=New-Object 'System.Collections.Generic.List[object]'
	foreach($line in Invoke-Git $RepoRoot @('ls-tree','-r','HEAD')){
		if($line -match '^160000 commit ([0-9a-f]{40})\t(.+)$'){
			$commit=$Matches[1]
			$path=$Matches[2]
			$url=@(Invoke-Git $RepoRoot @('config','--blob','HEAD:.gitmodules','--get',"submodule.$path.url"))[0]
			$list.Add([pscustomobject]@{ Path=$path; Commit=$commit; Url=$url })
		}
	}
	return ,$list
}

#Whether every submodule is checked out at the commit the release commit pins: the release's files are built from the checkout and the
#source archive holds the pinned commits, so they must be the same. Returns one line per problem; a problem whose line starts with
#"Missing:" means the submodule is not checked out at all.
function Test-SubmoduleCheckouts {
	$problems=New-Object 'System.Collections.Generic.List[string]'
	$submodules=Get-PinnedSubmodules
	foreach($submodule in $submodules){
		$folder=Join-Path $RepoRoot $submodule.Path
		if(-not (Test-Path -LiteralPath (Join-Path $folder '.git'))){
			$problems.Add("Missing: the submodule $($submodule.Path) is not checked out. Run git -c protocol.file.allow=always submodule update --init (docs\develop\third-party-source-builds.md).")
			continue
		}
		$checkedOut=@(Invoke-Git $folder @('rev-parse','HEAD'))[0].Trim()
		if($checkedOut -ne $submodule.Commit){
			$problems.Add("The submodule $($submodule.Path) is checked out at $checkedOut, but the release commit pins $($submodule.Commit). Run git submodule update, or commit the new pin.")
		}
	}
	return ,$problems
}

#Writes <folder>\OpenPmsDental-<version>-source.zip and returns its path: the repository at the release commit (git archive HEAD), every
#submodule at the commit HEAD pins, and the source of the setup's TWAINDSM.dll ($TwainDsmSourceEntry, the approved download byte for byte),
#under one top folder, with SOURCE-COMMITS.txt naming them. git archive writes each part; the parts are joined into one zip (an entry two
#parts share, the folder of a submodule, is written once). Assert-SourceArchivePossible runs first.
function Write-SourceArchive([string]$folder,[string]$version){
	#The TWAIN DSM source again, as it is copied: a release never carries another file under its name.
	$twainDsmSource=Find-TwainDsmSource
	if($twainDsmSource.Problems.Count -gt 0){
		Write-Problems $twainDsmSource.Problems 'The TWAIN DSM source check'
		if($twainDsmSource.Missing){ exit 4 }
		exit 1
	}
	Add-Type -AssemblyName System.IO.Compression
	Add-Type -AssemblyName System.IO.Compression.FileSystem
	New-Item -ItemType Directory -Force -Path $folder | Out-Null
	$folder=(Resolve-Path -LiteralPath $folder).Path
	$name=$SourceArchiveName -f $version
	$archivePath=Join-Path $folder ($name+'.zip')
	$commit=@(Invoke-Git $RepoRoot @('rev-parse','HEAD'))[0].Trim()
	$submodules=Get-PinnedSubmodules
	if(@(Invoke-Git $RepoRoot @('status','--porcelain','--untracked-files=no')).Count -gt 0){
		Write-Host "Warning: the working tree has changes that are not committed. The source archive holds the commit $commit."
	}
	$temp=Join-Path ([System.IO.Path]::GetTempPath()) ('OpenPmsDental-source-'+[guid]::NewGuid().ToString('N'))
	New-Item -ItemType Directory -Path $temp | Out-Null
	try{
		$parts=New-Object 'System.Collections.Generic.List[string]'
		$part=Join-Path $temp 'repository.zip'
		[void](Invoke-Git $RepoRoot @('archive','--format=zip',"--prefix=$name/",'-o',$part,'HEAD'))
		$parts.Add($part)
		$index=0
		foreach($submodule in $submodules){
			$index++
			$part=Join-Path $temp "submodule$index.zip"
			[void](Invoke-Git (Join-Path $RepoRoot $submodule.Path) @('archive','--format=zip',"--prefix=$name/$($submodule.Path)/",'-o',$part,$submodule.Commit))
			$parts.Add($part)
		}
		if(Test-Path -LiteralPath $archivePath){ Remove-Item -LiteralPath $archivePath -Force }
		$seen=New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
		$zipTarget=[System.IO.Compression.ZipFile]::Open($archivePath,[System.IO.Compression.ZipArchiveMode]::Create)
		try{
			foreach($part in $parts){
				$zipSource=[System.IO.Compression.ZipFile]::OpenRead($part)
				try{
					foreach($entry in $zipSource.Entries){
						if(-not $seen.Add($entry.FullName)){ continue }
						$entryTarget=$zipTarget.CreateEntry($entry.FullName,[System.IO.Compression.CompressionLevel]::Optimal)
						$entryTarget.LastWriteTime=$entry.LastWriteTime
						if($entry.FullName.EndsWith('/')){ continue }
						$streamIn=$entry.Open()
						$streamOut=$entryTarget.Open()
						try{ $streamIn.CopyTo($streamOut) }
						finally{
							$streamOut.Dispose()
							$streamIn.Dispose()
						}
					}
				}
				finally{ $zipSource.Dispose() }
			}
			#The TWAIN DSM source, stored as it is (already a zip), from the bytes whose SHA-256 was checked.
			$sourceBytes=[System.IO.File]::ReadAllBytes($twainDsmSource.Path)
			$hasher=[System.Security.Cryptography.SHA256]::Create()
			try{ $sourceSha=(($hasher.ComputeHash($sourceBytes) | ForEach-Object { $_.ToString('x2') }) -join '') }
			finally{ $hasher.Dispose() }
			if($sourceSha -ne $TwainDsmSourceSha256){ throw "$($twainDsmSource.Path) changed while the source archive was written: SHA-256 $sourceSha, approved $TwainDsmSourceSha256." }
			$entrySource=$zipTarget.CreateEntry("$name/$TwainDsmSourceEntry",[System.IO.Compression.CompressionLevel]::NoCompression)
			$entrySource.LastWriteTime=[System.IO.File]::GetLastWriteTime($twainDsmSource.Path)
			$streamSource=$entrySource.Open()
			try{ $streamSource.Write($sourceBytes,0,$sourceBytes.Length) }
			finally{ $streamSource.Dispose() }
			$lines=New-Object 'System.Collections.Generic.List[string]'
			$lines.Add("Open PMS - Dental $version`: the source of this release")
			$lines.Add('')
			$lines.Add("Repository: commit $commit")
			$lines.Add('')
			$lines.Add('Submodules, each at the commit the release commit pins, built from this source by third-party/build:')
			foreach($submodule in $submodules){ $lines.Add("$($submodule.Path): commit $($submodule.Commit), $($submodule.Url)") }
			$lines.Add('')
			$lines.Add('The source of TWAINDSM.dll, which the setup installs into the Windows system folder (GNU LGPL 2.1, LICENSE-LGPL-2.1.txt):')
			$lines.Add("$TwainDsmSourceEntry`: SHA-256 $TwainDsmSourceSha256, the TWAIN Working Group's source of the release TWAINDSM.dll $TwainDsmVersion comes from, as $TwainDsmSourceUrl publishes it")
			$lines.Add('')
			$lines.Add('Build and upgrade steps: README.md and docs/develop/third-party-source-builds.md.')
			$entryCommits=$zipTarget.CreateEntry("$name/SOURCE-COMMITS.txt",[System.IO.Compression.CompressionLevel]::Optimal)
			$writer=New-Object System.IO.StreamWriter($entryCommits.Open(),(New-Object System.Text.UTF8Encoding($false)))
			try{ $writer.Write(($lines -join "`r`n")+"`r`n") }
			finally{ $writer.Dispose() }
		}
		finally{ $zipTarget.Dispose() }
	}
	finally{
		Remove-Item -LiteralPath $temp -Recurse -Force -ErrorAction SilentlyContinue
	}
	return $archivePath
}

#Checks git and the submodules for the source archive, and exits when it cannot be written: 4 when git or a submodule is missing, 1 when a
#submodule is not at its pinned commit.
function Assert-SourceArchivePossible {
	$script:GitPath=Find-Git
	if(-not $script:GitPath){
		Write-Host 'git.exe not found on the PATH. The source archive needs it.'
		exit 4
	}
	$problems=Test-SubmoduleCheckouts
	if($problems.Count -gt 0){
		Write-Problems $problems 'The submodule check'
		if(@($problems | Where-Object { $_.StartsWith('Missing:') }).Count -gt 0){ exit 4 }
		exit 1
	}
	#The source of the setup's TWAINDSM.dll (LGPL 2.1): exit 4 when the approved download is missing, 1 when it is another file.
	$twainDsmSource=Find-TwainDsmSource
	if($twainDsmSource.Problems.Count -gt 0){
		Write-Problems $twainDsmSource.Problems 'The TWAIN DSM source check'
		if($twainDsmSource.Missing){ exit 4 }
		exit 1
	}
}

function Find-Iscc {
	foreach($candidate in @((Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),(Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
		(Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')))
	{
		if(Test-Path -LiteralPath $candidate -PathType Leaf){ return $candidate }
	}
	return $null
}
#endregion Helpers

if($ListFiles){
	foreach($key in $OwnFiles.Keys){ Write-Output "own|$key" }
	foreach($key in $ThirdPartyFiles.Keys){ Write-Output "third|$key" }
	foreach($key in $DocumentFiles.Keys){ Write-Output "doc|$key" }
	$newestRuntime=Get-NewestRuntimeVersion
	if(-not $newestRuntime){
		Write-Host "No $RuntimeBandPack $RuntimeBand.x in the offline folder ($(Get-OfflineFolder)), so the runtime files cannot be listed."
		exit 4
	}
	try{ $runtimeFiles=Get-RuntimeFiles $newestRuntime }
	catch{
		Write-Host $_.Exception.Message
		exit 4
	}
	foreach($key in $runtimeFiles.Keys){ Write-Output "runtime|$key" }
	foreach($name in $SetupFiles){ Write-Output "setup|$name" }
	exit 0
}

if($WriteSetupTexts){
	foreach($path in Write-SetupTexts $WriteSetupTexts (Read-ReleaseVersion)){ Write-Host "Wrote $path" }
	exit 0
}

if($WriteSetupFiles){
	$failed=$false
	foreach($bits in @(64,32)){
		$setupFile=Get-TwainDsm $WriteSetupFiles -Bits $bits
		if($setupFile.Problems.Count -gt 0){
			Write-Problems $setupFile.Problems ("The {0}-bit TWAIN DSM check" -f $bits)
			$failed=$true
			continue
		}
		Write-Host "Wrote $($setupFile.Path)"
	}
	if($failed){ exit 1 }
	exit 0
}

if($WriteSourceArchive){
	Assert-SourceArchivePossible
	Write-Host "Wrote $(Write-SourceArchive $WriteSourceArchive (Read-ReleaseVersion))"
	exit 0
}

if($CheckFolder){
	if(-not (Test-Path -LiteralPath $CheckFolder -PathType Container)){
		Write-Host "No such folder: $CheckFolder"
		exit 4
	}
	$problems=Test-StagingFolder $CheckFolder -Full:$Full
	Write-Host "Build id: $(Get-BuildId (Resolve-Path -LiteralPath $CheckFolder).Path)"
	if($problems.Count -gt 0){
		Write-Problems $problems 'The check'
		exit 1
	}
	Write-Host 'The check passed.'
	exit 0
}

#region Versions
$releaseVersion=Read-ReleaseVersion
$installerVersion=Read-InstallerVersion
if($installerVersion -ne $releaseVersion){
	Write-Host "installer\OpenPmsDental.iss says version $installerVersion, SharedAssemblyInfo.cs says $releaseVersion. Make them the same."
	exit 1
}
Write-Host "Release version $releaseVersion (compatibility version $CompatVersion)"
#endregion Versions

#region Submodules
#Before the build: PDFsharp/MigraDoc and WeCantSpell.Hunspell build from the submodules, and the source archive holds their pinned commits.
Assert-SourceArchivePossible
#endregion Submodules

#region Build
if(-not $SkipBuild){
	if($BuildLane){
		if(-not (Test-Path -LiteralPath $BuildLane -PathType Leaf)){
			Write-Host "No build lane script at $BuildLane"
			exit 4
		}
		$laneArguments=@('-NoProfile','-ExecutionPolicy','Bypass','-File',$BuildLane,'-Root',$RepoRoot,'-Configuration','Release','-Platform','x64',
			'-WaitMinutes',$LaneWaitMinutes)
		$projects=@($LaneProjects | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
		if($projects.Count -gt 0){ $laneArguments+=@('-Projects',($projects -join ','),'-ScopedRestore') }
		& powershell.exe @laneArguments
		$exitBuild=$LASTEXITCODE
		#The lane answers 98 while another build holds it: wait for it once more, as a person would run it again.
		if($exitBuild -eq 98){
			& powershell.exe @laneArguments
			$exitBuild=$LASTEXITCODE
		}
	}
	else{
		if(-not $MSBuildPath){ $MSBuildPath=Find-MSBuild }
		if(-not $MSBuildPath -or -not (Test-Path -LiteralPath $MSBuildPath -PathType Leaf)){
			Write-Host 'MSBuild.exe of Visual Studio 2026 or its Build Tools not found. Install them as README.md describes, or pass -MSBuildPath or -BuildLane.'
			exit 4
		}
		#The pinned .NET SDK and the offline NuGet folder for this process; the restore is locked to the committed lock files.
		$buildEnvironment=Initialize-BuildEnvironment -RepoRoot $RepoRoot
		Write-Host "MSBuild $MSBuildPath, .NET SDK $($buildEnvironment.SdkVersion) from '$($buildEnvironment.SdkFolder)'"
		& $MSBuildPath (Join-Path $RepoRoot 'OpenDental.Local.slnf') /restore /p:RestoreLockedMode=true /p:Configuration=Release /p:Platform=x64 /m /nodeReuse:false /v:minimal /nologo
		$exitBuild=$LASTEXITCODE
	}
	if($exitBuild -ne 0){
		Write-Host "The Release build failed (exit $exitBuild)."
		exit 2
	}
}
#endregion Build

#region Release output
$releaseOutput=Join-Path $RepoRoot 'OpenPmsDental\bin\Release'
if(-not (Test-Path -LiteralPath $releaseOutput -PathType Container)){
	Write-Host "No Release output at $releaseOutput. Build Release x64 first."
	exit 4
}
$problems=New-Object 'System.Collections.Generic.List[string]'
#The release-flow checks of the runtime come first: a build on mixed or old runtime files is never staged.
$runtimeCheck=Test-RuntimeVersions $releaseOutput
if($runtimeCheck.Problems.Count -gt 0){
	Write-Problems $runtimeCheck.Problems 'The runtime check of the Release output'
	exit 1
}
try{ $runtimeFiles=Get-RuntimeFiles $runtimeCheck.Version }
catch{
	Write-Host $_.Exception.Message
	exit 4
}
Write-Host "Runtime .NET $($runtimeCheck.Version): $($runtimeFiles.Count) runtime files from the runtime packs' file lists"
$allowedSet=New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
foreach($key in $OwnFiles.Keys){ [void]$allowedSet.Add($key) }
foreach($key in $ThirdPartyFiles.Keys){ [void]$allowedSet.Add($key) }
foreach($key in $runtimeFiles.Keys){ [void]$allowedSet.Add($key) }
#The top of the Release output and Helper32\, the one sub folder.
$outputItems=@(Get-ChildItem -LiteralPath $releaseOutput -Force)
$helperOutput=Join-Path $releaseOutput $HelperFolder
if(Test-Path -LiteralPath $helperOutput -PathType Container){ $outputItems+=@(Get-ChildItem -LiteralPath $helperOutput -Force) }
foreach($item in $outputItems){
	$relative=$item.FullName.Substring($releaseOutput.TrimEnd('\').Length+1)
	if($item.PSIsContainer){
		if($relative -eq $HelperFolder){ continue }
		$reason=Get-DenyReason $item.Name
		if($reason){ $problems.Add("Denied folder in the Release output: $relative ($reason)") }
		else{ $problems.Add("Unexpected folder in the Release output: $relative") }
		continue
	}
	$notShipped=$false
	foreach($pattern in $NotShippedPatterns){ if($item.Name -like $pattern){ $notShipped=$true } }
	if($notShipped){ continue }
	if($runtimeFiles.Contains($relative)){ continue }
	$reason=Get-DenyReason $item.Name
	if($reason){ $problems.Add("Denied file in the Release output: $relative ($reason)") }
	elseif(-not $allowedSet.Contains($relative)){
		$problems.Add("Unexpected file in the Release output: $relative (add it to the allow-list of tools\Build-Release.ps1, with its notice if it is third-party, or remove it from the build)")
	}
}
foreach($name in $allowedSet){
	if(-not (Test-Path -LiteralPath (Join-Path $releaseOutput $name) -PathType Leaf)){
		$why=if($RequiredReasons.Contains($name)){ " ($($RequiredReasons[$name]))" }else{ '' }
		$problems.Add("Missing from the Release output: $name$why")
	}
}
if($problems.Count -gt 0){
	Write-Problems $problems 'The Release output check'
	exit 1
}
#endregion Release output

#region Stage
if(-not $OutDir){ $OutDir=Join-Path $RepoRoot 'installer\bin' }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
$OutDir=(Resolve-Path -LiteralPath $OutDir).Path.TrimEnd('\')
$staging=Join-Path $OutDir 'staging'
if(Test-Path -LiteralPath $staging){
	Remove-Item -LiteralPath $staging -Recurse -Force
}
New-Item -ItemType Directory -Path $staging | Out-Null
New-Item -ItemType Directory -Path (Join-Path $staging $HelperFolder) | Out-Null
foreach($name in $OwnFiles.Keys){ Copy-Item -LiteralPath (Join-Path $releaseOutput $name) -Destination (Join-Path $staging $name) }
foreach($name in $ThirdPartyFiles.Keys){ Copy-Item -LiteralPath (Join-Path $releaseOutput $name) -Destination (Join-Path $staging $name) }
foreach($name in $runtimeFiles.Keys){ Copy-Item -LiteralPath (Join-Path $releaseOutput $name) -Destination (Join-Path $staging $name) }
foreach($path in $DocumentFiles.Keys){
	$target=Join-Path $staging $path
	New-Item -ItemType Directory -Force -Path (Split-Path $target -Parent) | Out-Null
	Copy-Item -LiteralPath (Join-Path $RepoRoot $DocumentFiles[$path]) -Destination $target
}
$problems=Test-StagingFolder $staging -Full
if($problems.Count -gt 0){
	Write-Problems $problems 'The staging check'
	exit 1
}
$buildId=Get-BuildId $staging
$stagedFiles=@(Get-ChildItem -LiteralPath $staging -Recurse -File -Force | ForEach-Object { $_.FullName.Substring($staging.Length+1) } |
	Sort-Object -Property @{ Expression={ $_.ToLowerInvariant() } })
Write-Host "Staged $($stagedFiles.Count) files in $staging"
Write-Host "Build id: $buildId"
$sumLines=New-Object 'System.Collections.Generic.List[string]'
foreach($relative in $stagedFiles){
	$sumLines.Add((Get-Sha256 (Join-Path $staging $relative))+'  staging/'+$relative.Replace('\','/'))
}
$sumsPath=Join-Path $OutDir 'SHA256SUMS.txt'
Write-Sums $sumsPath $sumLines.ToArray()
#The staged list in two parts, as the release channel publishes it (ForkReleases.GetRuntimePart): the runtime part and the program part.
$runtimeStaged=@($stagedFiles | Where-Object { $runtimeFiles.Contains($_) })
Write-Host "The runtime part: $($runtimeStaged.Count) files (.NET $($runtimeCheck.Version), win-x64 and $HelperFolder\ win-x86); the program part: $($stagedFiles.Count-$runtimeStaged.Count) files"
#endregion Stage

#region Installer
$setupName="OpenPmsDental-$releaseVersion-Setup.exe"
$setupPath=Join-Path $OutDir $setupName
if(-not $SkipInstaller){
	if(-not $Iscc){ $Iscc=Find-Iscc }
	if(-not $Iscc -or -not (Test-Path -LiteralPath $Iscc -PathType Leaf)){
		Write-Host 'Inno Setup 6 (ISCC.exe) not found. Pass -Iscc, or -SkipInstaller to stage only.'
		exit 4
	}
	#The information page (the attribution and the source address, then NOTICE) and the License Agreement page (how the installed program is
	#licensed, then the GNU GPL 3.0 it is conveyed under). Both stay out of the staging folder, which is exactly what is installed.
	$setupTexts=Write-SetupTexts $OutDir $releaseVersion
	$infoPath=$setupTexts[0]
	$licensePath=$setupTexts[1]
	#The 64-bit TWAINDSM.dll from the approved download, checked, beside the staging folder: a setup file, never a file of the program folder.
	$twainDsm=Get-TwainDsm (Join-Path $OutDir 'setup-files')
	if($twainDsm.Problems.Count -gt 0){
		Write-Problems $twainDsm.Problems 'The TWAIN DSM check'
		exit 1
	}
	Write-Host "TWAINDSM.dll $TwainDsmVersion (x64, signed by the TWAIN Working Group, SHA-256 $TwainDsmSha256)"
	#The 32-bit TWAINDSM.dll of the same download (S3b-8), checked the same way, for the 32-bit system folder.
	$twainDsm32=Get-TwainDsm (Join-Path $OutDir 'setup-files') -Bits 32
	if($twainDsm32.Problems.Count -gt 0){
		Write-Problems $twainDsm32.Problems 'The 32-bit TWAIN DSM check'
		exit 1
	}
	Write-Host "x86\TWAINDSM.dll $TwainDsmVersion (x86, signed by the TWAIN Working Group, SHA-256 $TwainDsm32Sha256)"
	if(Test-Path -LiteralPath $setupPath){ Remove-Item -LiteralPath $setupPath -Force }
	#Not quiet: ISCC names every file it packs ("Compressing: <path>"), which is checked against the staged list below.
	$isccOutput=@(& $Iscc "/DStagingDir=$staging" "/DInfoFile=$infoPath" "/DLicenseTextFile=$licensePath" "/DTwainDsmFile=$($twainDsm.Path)" "/DTwainDsm32File=$($twainDsm32.Path)" "/O$OutDir" (Join-Path $RepoRoot 'installer\OpenPmsDental.iss') 2>&1 | ForEach-Object { "$_" })
	$exitIscc=$LASTEXITCODE
	if($exitIscc -ne 0 -or -not (Test-Path -LiteralPath $setupPath -PathType Leaf)){
		$isccOutput | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
		Write-Host "The installer did not compile (ISCC exit $exitIscc)."
		exit 3
	}
	foreach($line in $isccOutput){ if($line -match '^\s*Warning:'){ Write-Host "ISCC $($line.Trim())" } }
	#The setup's file list is the staged list and the two TWAINDSM.dll, nothing more and nothing less.
	$packed=New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
	foreach($line in $isccOutput){
		if($line -match '^\s*Compressing: (.+?)(\s+\(\d+(\.\d+)*\))?\s*$'){ [void]$packed.Add([System.IO.Path]::GetFullPath($Matches[1])) }
	}
	$expected=New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
	foreach($relative in $stagedFiles){ [void]$expected.Add([System.IO.Path]::GetFullPath((Join-Path $staging $relative))) }
	[void]$expected.Add([System.IO.Path]::GetFullPath($twainDsm.Path))
	[void]$expected.Add([System.IO.Path]::GetFullPath($twainDsm32.Path))
	$notPacked=@($expected | Where-Object { -not $packed.Contains($_) })
	$notStaged=@($packed | Where-Object { -not $expected.Contains($_) })
	if($notPacked.Count -gt 0 -or $notStaged.Count -gt 0){
		Write-Host "The setup's file list differs from the staged list: not in the setup: $($notPacked -join ', '); not staged: $($notStaged -join ', ')."
		exit 3
	}
	Write-Host "The setup packs the $($stagedFiles.Count) staged files and the 64-bit and 32-bit TWAINDSM.dll."
	$setupSha=Get-Sha256 $setupPath
	$sumLines.Insert(0,$setupSha+'  '+$setupName)
	Write-Sums $sumsPath $sumLines.ToArray()
	Write-Host "Setup: $setupPath"
	Write-Host "Setup SHA-256: $setupSha"
}
#endregion Installer

#region Source archive
#The Corresponding Source of the release: the repository and the submodules at the release commit (docs\develop\release-process.md attaches it to
#the GitHub release). Its SHA-256 follows the setup's in SHA256SUMS.txt.
$sourceArchivePath=Write-SourceArchive $OutDir $releaseVersion
$sourceArchiveName=Split-Path $sourceArchivePath -Leaf
$sourceArchiveSha=Get-Sha256 $sourceArchivePath
$sumLines.Insert($(if($SkipInstaller){ 0 }else{ 1 }),$sourceArchiveSha+'  '+$sourceArchiveName)
Write-Sums $sumsPath $sumLines.ToArray()
Write-Host "Source archive: $sourceArchivePath"
Write-Host "Source archive SHA-256: $sourceArchiveSha"
Write-Host "Checksums: $sumsPath"
#endregion Source archive

#region Copy
if($CopyTo){
	$notes=Join-Path $RepoRoot "docs\release-notes\v$releaseVersion.md"
	foreach($path in @($sumsPath,$notes,$sourceArchivePath)){
		if(-not (Test-Path -LiteralPath $path -PathType Leaf)){
			Write-Host "Missing: $path"
			exit 4
		}
	}
	if($SkipInstaller){
		Write-Host '-CopyTo needs the installer; it was skipped.'
		exit 4
	}
	New-Item -ItemType Directory -Force -Path $CopyTo | Out-Null
	Copy-Item -LiteralPath $setupPath -Destination (Join-Path $CopyTo $setupName) -Force
	Copy-Item -LiteralPath $sourceArchivePath -Destination (Join-Path $CopyTo $sourceArchiveName) -Force
	Copy-Item -LiteralPath $sumsPath -Destination (Join-Path $CopyTo 'SHA256SUMS.txt') -Force
	Copy-Item -LiteralPath $notes -Destination (Join-Path $CopyTo "release-notes-v$releaseVersion.md") -Force
	Write-Host "Copied the setup, the source archive, SHA256SUMS.txt and the release notes to $CopyTo"
}
#endregion Copy
exit 0
