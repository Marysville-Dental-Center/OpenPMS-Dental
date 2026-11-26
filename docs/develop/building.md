# Building Open PMS - Dental

The full build reference: prerequisites, the source, the build, the solution layout, and a development copy with its own database. The short version is in the [README](../../README.md#building).


## Prerequisites

- Windows 11, 64-bit
- **Visual Studio 2026** (any edition) or **Build Tools for Visual Studio 2026** (version 18; both install under `Microsoft Visual Studio\18\`), with:
  - .NET desktop build tools (`Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools`)
  - Desktop development with C++ (`Microsoft.VisualStudio.Workload.VCTools`) and its MSVC v143 toolset (`Microsoft.VisualStudio.Component.VC.14.44.17.14.x86.x64`, MSVC 14.44), for `Direct2dWrapper` (native, toolset v143; the default toolset of 2026 is v145)
  - .NET Framework 4.8 SDK and the 4.8 and 4.7.2 targeting packs (`Microsoft.Net.Component.4.8.SDK`, `Microsoft.Net.Component.4.8.TargetingPack`, `Microsoft.Net.Component.4.7.2.TargetingPack`)
  - Testing tools core (`Microsoft.VisualStudio.Component.TestTools.BuildTools`), for `vstest.console.exe`, which runs the MSTest test projects
  - NuGet build tools (`Microsoft.VisualStudio.Component.NuGet.BuildTools`) and the Windows 11 SDK 10.0.26100 (`Microsoft.VisualStudio.Component.Windows11SDK.26100`)
- **The .NET SDK 10.0.115** exactly: `global.json` pins it, for the projects that move to the SDK project format (stage 1 of the .NET 10 move, [docs/specs/net10-plan.md](../specs/net10-plan.md)). Install it machine-wide, or per user into `%LOCALAPPDATA%\Microsoft\dotnet` (`dotnet-install.ps1 -Version 10.0.115` does that). Each monthly .NET 10 patch moves the pin by the bump procedure of section 2.4 of the plan. The SDK that Visual Studio installs itself is a later band and is not used.
- **The offline package folder.** `nuget.config` restores from one local folder only, the one the environment variable `OPMS_NUGET_OFFLINE` names; nothing is downloaded during a build. The project's own build PC keeps it in the git folder (`.git\od-tools\nuget-packages`), which `tools\Initialize-BuildEnvironment.ps1` finds by itself. Elsewhere, put the packages the build restores into a folder of your own and set the variable. Today those are the packages of `Directory.Packages.props`: the libraries, the support assemblies and the WebView2 SDK, whose own files the build and the release take; `Mono.Cecil` 0.10.3 for the test projects that read assemblies; the test packages of MSTest 4 (`MSTest.TestFramework`, `MSTest.TestAdapter` and `MSTest.Analyzers` 4.5.3 and `Microsoft.NET.Test.Sdk` 18.12.0, with the packages they bring: `Microsoft.CodeCoverage`, `Microsoft.TestPlatform.ObjectModel`, `Microsoft.Testing.Platform` and its extensions, `Microsoft.ApplicationInsights`, `System.Reflection.Metadata` and `System.Collections.Immutable`; `docs/port-notes/S4-1.md`); and `System.ValueTuple` 4.6.2, which several libraries depend on and which `Directory.Packages.props` pins for every project, so the test packages' 4.5.0 is not copied into a test output folder (the framework's own copy binds). Every `packages.lock.json` names each package with its version and content hash. No project restores into a `packages` folder any more.

One command installs the Build Tools with everything above, from the installer of the Visual Studio 2026 channel (`https://aka.ms/vs/18/stable/vs_buildtools.exe`):

```bash
vs_buildtools.exe --passive --wait --norestart --add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools --add Microsoft.VisualStudio.Workload.VCTools --includeRecommended --add Microsoft.VisualStudio.Component.VC.14.44.17.14.x86.x64 --add Microsoft.Net.Component.4.8.TargetingPack --add Microsoft.Net.Component.4.8.SDK --add Microsoft.Net.Component.4.7.2.TargetingPack --add Microsoft.VisualStudio.Component.NuGet.BuildTools --add Microsoft.VisualStudio.Component.TestTools.BuildTools --add Microsoft.VisualStudio.Component.Windows11SDK.26100
```

## Get the source

PDFsharp/MigraDoc and the WeCantSpell.Hunspell spell checker are built from their upstream source, which this repository references as git submodules instead of copying it (`third-party/PDFsharp`, `third-party/WeCantSpell.Hunspell`; [docs/develop/third-party-source-builds.md](third-party-source-builds.md)). Clone with them, or fetch them after cloning:

```bash
git clone --recurse-submodules <repository address>
git submodule update --init
```

Without them the build stops with "The upstream source is missing".

## Build

Open **`OpenDental.Local.slnf`** in Visual Studio, not `OpenDental.sln`, and build `Debug | x64` (or `Release | x64` for an office). Since stage 3b of the .NET 10 move (package S3b-5) every project of the filter is .NET 10 but one test stand-in that stays .NET Framework on purpose (TopazStandIn, which no .NET 10 project references), so the whole filter restores and builds. From the command line, in PowerShell at the repository root:

```powershell
. .\tools\Initialize-BuildEnvironment.ps1; Initialize-BuildEnvironment -RepoRoot (Get-Location).Path
& "C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe" OpenDental.Local.slnf -restore -p:RestoreLockedMode=true -p:Configuration=Debug -p:Platform=x64 -m -nodeReuse:false
```

`Initialize-BuildEnvironment` sets, for that PowerShell window only, what the build needs: with a per-user .NET SDK, `DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR` and `PATH`, so MSBuild finds that SDK; `OPMS_NUGET_OFFLINE` when it is not set; no .NET telemetry. The tool scripts under `tools/` call it themselves. With a per-user SDK, start Visual Studio from that window, so it finds the SDK too. With Visual Studio installed instead of Build Tools, replace `BuildTools` in the path with your edition, e.g. `Community`. A clean build produces many compiler warnings and no errors. The main program is `OpenPmsDental\bin\Debug\OpenPmsDental.exe` (`OpenPmsDental\bin\Release\` for a Release build). Every program there is its apphost (`.exe`) with its `.dll`, `.runtimeconfig.json` and `.deps.json`, beside the folder's own x64 .NET runtime. The build also copies UpdateFileCopier's four files, the native file the program loads at run time, `WebView2Loader.dll` (x64, from the WebView2 package), and `Direct2dWrapper.dll` (x64) next to it, and the 32-bit helper with its own x86 runtime into `Helper32\`.

## The solution and the solution filter

- **`OpenDental.Local.slnf`** loads the program and its libraries, services and tools, the adoption and synthetic-data tools under `tools/`, and every test project under `tests/`. The older or standalone projects listed under [Repository layout](repository-layout.md) are not in it. `OpenDental.sln` lists the same projects, so either one builds the same set. The build scripts read the filter, so add a new project to both.
- **`Directory.Build.targets`** gives the test projects their binding redirects, keeps the Microsoft Testing Platform's telemetry extension out of every output folder and applies the MSTest analyzer levels of `tests/MSTest.globalconfig` (the test projects use MSTest 4 from its NuGet packages since S4-1 of the .NET 10 move, `docs/port-notes/S4-1.md`). Every project is built from source: a missing project fails the build, and nothing is swapped for a DLL in `Required dlls/`.
- **`Directory.Build.props`** and **`tests/Directory.Build.props`** hold the settings of the SDK-style projects only (flat `bin\Debug\` and `bin\Release\` output, x64 on .NET 10 and the helper x86, lock files, no generated assembly info); the classic projects ignore them. **`Directory.Packages.props`** names the version of every NuGet package (central package management); a project's `PackageReference` names none. **`global.json`** pins the .NET SDK, **`nuget.config`** the offline package folder. `tools\Get-ProjectBaseline.ps1` records what each project compiles, embeds and outputs, and compares that with the record in `tools/project-baseline/` taken before the change of project format.
- **Version numbers:** `SharedAssemblyInfo.cs` in the repository root gives every shipped assembly its product name, copyright and release version (`0.0`). Each project's `AssemblyVersion` stays on its own line; for `OpenPmsDental.exe` and `OpenPmsDentalBusiness.dll` that is the compatibility version `24.3.63.0`, which the database checks use ([docs/develop/release-process.md](release-process.md)).
- **We don't merge from upstream.** Open Dental's public source stops at 24.3.63; later features are re-implemented from public documentation ([docs/specs/decoupling-plan.md](../specs/decoupling-plan.md)).
- **`.gitignore`** ignores build output (`bin/`, `obj/`, `packages/`, `.vs/`). The `ConversionFiles` already tracked under `OpenPmsDental/bin/Debug` remain tracked.

## Running a development copy

`OpenPmsDental.exe` needs a MariaDB database with its schema. `tools/New-DevDatabase.ps1` builds a development database from files in this repository, so you don't need Open Dental's installer or trial version. It is for development only: its seed is Open Dental's demo database, and its only user has a blank password. An office starts from [db/README.md](../../db/README.md) instead.

### The database server for development

We test with MariaDB 10.11 on `127.0.0.1:3306`, user `root`, blank password. Those are the script's defaults. Use the server settings of [Installing in an office](../admin/installing.md#database-server):

- `sql_mode=` (blank). The convert scripts need it. If the server is stricter, the tool runs `SET GLOBAL sql_mode=''` and warns, the same thing `OpenPmsDental.exe` does at startup.
- `default-storage-engine=MyISAM`. The seed tables are MyISAM, and the convert scripts create new tables in the server's default engine. With another default the database ends up with mixed engines, and the tool warns.

### Create the dev database

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File tools\New-DevDatabase.ps1 -WithSampleData -WriteConfig
```

`-ExecutionPolicy Bypass` applies only to that one process. It's needed because Windows blocks scripts by default. The script builds `tools/DevDbTool`, a small console program, and runs it. The full solution build also builds DevDbTool; its `--export-init-sql` command writes the new office database file (`db/README.md`). Building it also builds `OpenPmsDentalBusiness`, `CodeBase` and `DataConnectionBase` if they're out of date. It:

1. Creates the database `opendental_dev` with the `utf8` character set.
2. Loads the seed `UnitTests/Resources/dump.txt`, the 21.1.9 database the unit tests start from.
3. Upgrades it to this build's version by running the convert methods in `OpenPmsDentalBusiness/Misc/ConvertDatabases*.cs`, the same code `OpenPmsDental.exe` runs on an older database.
4. Applies the default auto codes, procedure buttons and appointment quick-add procedures, as the unit tests' `FreshFromDump` does.
5. Sets `ProgramVersion`, creates an A to Z images folder at `%USERPROFILE%\OpenDentImagesDev\<name>` and points `DocPath` at it.
6. With `-WithSampleData`, adds a few fictional providers, operatories, patients, schedules and appointments.
7. Checks the result: `DataBaseVersion` and `ProgramVersion` match the program version, no conversion is left half-done, and every table and column declared by the code's table types exists. HQ-only tables are skipped.

It takes about a minute. The database starts as the seed's fictional demo practice, with two providers and six operatories. The only user is **`Admin`, with a blank password**.

The tool talks only to the database server. It sends all .NET web requests to a dead local proxy, so if anything tries to call out over HTTP, the tool lists the addresses and exits with code 3. By default it refuses servers other than `localhost`/`127.0.0.1`. `-Force` refuses to drop a database that holds more than 200 patients.

| Option | Meaning |
|---|---|
| `-Name <db>` | Database name (default `opendental_dev`) |
| `-Server`, `-Port`, `-User`, `-Password` | Connection (default `127.0.0.1`, `3306`, `root`, blank) |
| `-Force` | Drop and recreate the database if it exists |
| `-WithSampleData` | Add the fictional sample practice data |
| `-WriteConfig` | Write `OpenPmsDental\bin\Debug\FreeDentalConfig.xml` for this database. An existing file is kept as `.bak` |
| `-AtoZPath <dir>` / `-NoAtoZ` | Use another images folder, or keep the seed's `C:\OpenDentImages\` |
| `-VerifyOnly` | Check an existing database without changing it |
| `-SkipBuild` | Use the already-built `tools\DevDbTool\bin\Debug\DevDbTool.exe` |

`Get-Help .\tools\New-DevDatabase.ps1 -Full` lists every option.

### Point OpenPmsDental.exe at it

`OpenPmsDental.exe` reads its connection from `FreeDentalConfig.xml` in its own folder, `OpenPmsDental\bin\Debug\`. Use one of these:

- Run the script with `-WriteConfig`. Add `-VerifyOnly` to only write the config for an existing database.
- Copy `tools/FreeDentalConfig.dev.xml` to `OpenPmsDental\bin\Debug\FreeDentalConfig.xml` and set `<Database>` if you used `-Name`.
- Pass the connection on the command line: `OpenPmsDental.exe ServerName=127.0.0.1 DatabaseName=opendental_dev MySqlUser=root`.

Then build the solution (see [Build](#build)), start `OpenPmsDental\bin\Debug\OpenPmsDental.exe`, and log in as `Admin` with a blank password.
