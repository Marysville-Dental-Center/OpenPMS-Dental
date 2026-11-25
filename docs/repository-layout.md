# Repository layout

What each folder of the repository holds, the branches, and Open Dental's own resources.


| Project | What it is |
|---|---|
| `OpenPmsDental/` | The WinForms desktop application (`OpenPmsDental.exe`): forms, modules, UI |
| `OpenPmsDentalBusiness/` | Business logic and data access: table types, `Crud/` (generated), `Db*/`, eClaims, X12, HL7, bridges, sheets, web services, the fork migrations and the release channel (`Misc/ForkMigrations`, `Misc/ForkRelease`) |
| `WpfControlsOD/` | WPF controls and newer UI hosted in the WinForms app |
| `CodeBase/` | Shared utilities with no database dependency, including stored-secret obfuscation and the `SecurityHash` tamper hashes (`CodeBase/Crypto/`, see its README) |
| `DataConnectionBase/` | Low-level MySQL/MariaDB connection layer |
| `ImagingDeviceManager/`, `ImagingCapture/` | Open source TWAIN scanning and native image capture for the Imaging module ([docs/imaging-capture-design.md](imaging-capture-design.md)) |
| `ODCrypt/` | Standard hash, random and compression helpers |
| `xCrudGenerator/` | Generates `OpenPmsDentalBusiness/Crud/*` from the table types. Run it after changing a table type |
| `OpenPmsDentalServer/` | Middle-tier server (ASP.NET Core, one HTTPS endpoint, run as a Windows service or in IIS, self-contained x64; S3b-6). Its path stays /OpenDentalServer, so the workstations' saved addresses and logins keep working |
| `OpenPmsDentalHL7/`, `ServiceManager/`, `CentralManager/`, `DatabaseIntegrityCheck/`, `MySQLManager/`, `UpdateFileCopier/` | Supporting services and tools; `UpdateFileCopier.exe` installs a pulled release |
| `OpenPmsDentalStorage/`, `OpenPmsDentalGraph/`, `SparksToothChart/`, `Direct2dWrapper/`, `PasswordVaultWrapper/` | SFTP A to Z storage, dashboards, tooth chart rendering, native Direct2D, and Windows Credential Manager libraries |
| `tests/` | MSTest 4 test projects (run by vstest), one per package, in the solution filter. Tests that need a database create a scratch database with a unique name on `127.0.0.1:3306` and drop it afterwards |
| `UnitTests/`, `UnitTestsCore/`, `NoHqTests/` | Open Dental's tests (MSTest 4 since S4-1) and their helpers, and the checks that no Open Dental servers are contacted. `tools/run-tests.ps1` runs the legacy `UnitTests` project (`Unit Tests.exe`, named `UnitTests` in `-Include`) with the others. It builds its own database `unittest<version>_<time>_<process>` on `127.0.0.1:3306` from the seed dump and drops it at the end (set `OPENPMS_UNITTESTS_KEEPDB=1` to keep it). Its developer windows under `UI Testing/` are opened by hand, not run as tests |
| `tools/` | `New-DevDatabase.ps1` and `DevDbTool/` (development database), `New-InitSql.ps1` (regenerates `db/init/new-database.sql`), `AdoptionValidation/` (adopting an Open Dental database), `SyntheticData/` (synthetic test data), `run-tests.ps1` (the test runner), `Initialize-BuildEnvironment.ps1` (finds MSBuild and vstest of the Build Tools 2026 and sets the build's environment, for the other scripts), `Get-ProjectBaseline.ps1` (records and compares what each project compiles, embeds and outputs; the record is `project-baseline/`), `Test-NoHqHosts.ps1` (fails if code still references Open Dental's hosts), `Test-NoOldNames.ps1` (lists the program's old internal names, for the rename), `Build-Release.ps1` (the release: staging, checks, checksums and the setup), `Test-Installer.ps1` (an elevated test install and uninstall of the setup, for all users; the owner runs it from an elevated PowerShell at a Release dry run) and `ForkMeasure/` (`measure_fork.py`: how much of the code is unchanged from Open Dental, the numbers of [differences.md](differences.md#the-numbers)) |
| `installer/` | The Inno Setup 6 script of the workstation setup (`OpenPmsDental.iss`) and its information page |
| `docs/` | Design notes, the release process, the adoption runbook, the binaries audit and one port note per package (`docs/port-notes/`) |
| `Required dlls/` | The third-party files the build references that have no NuGet package (the Direct Project, the QuickBooks Desktop and Schick interop); the files of NuGet packages come from the packages (`Directory.Packages.props`). What ships is listed in [THIRD-PARTY-NOTICES.txt](../THIRD-PARTY-NOTICES.txt); the rest is reviewed in [docs/third-party-binaries-audit.md](third-party-binaries-audit.md) |
| `third-party/` | Upstream source the program builds instead of shipping prebuilt files: the git submodules `PDFsharp` (PDFsharp/MigraDoc 6.5.0, empira's AGPL source) and `WeCantSpell.Hunspell` (7.0.1), pinned to their release commits, and `build/`, this project's own project files that compile them in the solution, with the one PDFsharp file the build replaces ([docs/third-party-source-builds.md](third-party-source-builds.md)) |
| `ODR/`, `RdlEngine/`, `RdlViewer/` | RDL reports: `ODR.dll`, the code module report definitions call, and the fyiReporting RDL engine and viewer (LGPL 2.1), built from source in the solution and copied next to `OpenPmsDental.exe` ([docs/port-notes/RDL-SRC.md](port-notes/RDL-SRC.md)) |
| `RdlDesign/` | fyiReporting's RDL report designer (`RdlDesigner.exe`), built in the solution against `RdlEngine/` and `RdlViewer/`; a developer tool, not shipped |
| `DocumentationBuilder/` | Open Dental's documentation tool: the schema documentation (`OpenDentalDocumentation.xml` and the diff against a previous version), the unit test and the preference documentation; built in the solution against the program; a developer tool, not shipped |
| `PluginExample/` | Older standalone project, not in the solution filter and not built |

## Branches

| Branch | Contents |
|---|---|
| `main` | Our development line: Open Dental 24.3.63 and the fork's own work |
| `24_3` | Upstream Open Dental v24.3 releases (one commit per build, e.g. `version 24.3.63.0`), kept for reference |
| `21_1` to `24_2` | Older upstream release lines, kept for reference |

The upstream branches are not merged into `main` any more (see [The solution and the solution filter](building.md#the-solution-and-the-solution-filter)).

## Upstream resources

- [Source Code](https://www.opendental.com/site/sourcecode.html): Open Dental's own build notes
- [Programming Resources](https://www.opendental.com/site/programmingresources.html): database schema documentation
- [Previous Versions](https://opendentalsoft.com:1943/ODBugTracker/PreviousVersions.aspx): changes per release
- [Subversion (SVN)](https://opendentalsoft.com:23793/svn/opendental): source before 2022 (username **guest**, password **od123**)
- [Downloads](https://www.opendental.com/manual/downloads.html): source before 2007

Open Dental does not accept outside code contributions. See [Programming Assistance](https://www.opendental.com/site/programmingassistance.html). This repository is licensed under the GNU GPL v2 or later, and the program as distributed with PDFsharp and MigraDoc under the GNU GPL v3; see [LICENSE](../LICENSE), [LICENSE-GPL-3.0.txt](../LICENSE-GPL-3.0.txt) and [NOTICE](../NOTICE).
