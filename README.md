<img src="branding/logo-256.png" alt="Open PMS - Dental logo" width="96">

# Open PMS - Dental

Open PMS - Dental is a fork of Open Dental (c) Open Dental Software, Inc. and contributors, licensed under the GNU GPL v2. It is not affiliated with or endorsed by Open Dental Software, Inc.

**Free, open-source practice management software for dental offices, for Windows.** It started from the [Open Dental](https://www.opendental.com) source code (v24.3.63) and keeps everything an office relies on: scheduling, charting, billing, insurance, imaging and reports. It is written for one dental practice and published for anyone to use. See [NOTICE](NOTICE) for attribution and [LICENSE](LICENSE) for the GNU General Public License; every Open Dental copyright line and GPL notice is kept.

## Philosophy

- **Open, all the way down.** Every file the setup installs has published source. Closed libraries are rebuilt in the open or removed. Nothing is decompiled.
- **Nothing phones home.** The program talks to no vendor server of its own. Patient data, images and the program's services stay on the office network. Integrations talk directly to their vendor, stay off until the office turns them on, and carry no advertising or recommended vendors.
- **Nothing is cut.** Every feature and every test is kept and ported, also ones a given office does not use, unless the owner deliberately removes one.
- **Familiar to staff.** The database follows Open Dental's schema, so an office can move over from Open Dental, and the workflows stay where staff expect them.
- **Current and native.** .NET 10, 64-bit, the newest stable version of every library from official sources, and each vendor's native 64-bit interface first.

## How we work

- **Re-implemented, not merged.** Open Dental's public source stops at 24.3.63. Features of later versions, up to 26.1, are written again from Open Dental's public manual, release notes and database documents; the schema follows Open Dental 26.2.
- **Small packages.** Every change is a package with a plan, a port note in [docs/port-notes/](docs/port-notes/) and its own tests. It gets an independent review and reaches `main` only after a full build and the test suites pass.
- **The owner sees what users will see.** Visible changes are shown as before-and-after picture galleries drawn by the tests, audited before review, and every approved picture is recorded (`tests/WindowChecksTests/UiApproved/approved.tsv`).
- **Practice-neutral and synthetic.** No practice names, server names or developer paths in tracked files; tests use synthetic data on scratch databases that they drop afterwards. A test checks every tracked file for the names.

## Features

- **Scheduling:** the appointment book with operatories and providers, recall, the ASAP and unscheduled lists, confirmations.
- **Patients:** families and guarantors, demographics, medical history, sheets and patient forms.
- **Clinical:** a 3D tooth chart (rebuilt in WPF), the perio chart, procedures and progress notes, treatment plans, prescriptions.
- **Imaging:** the Imaging module with open-source TWAIN scanning, native sensor capture and an in-house DICOM reader.
- **Billing:** accounts, payments, payment plans, adjustments and statements.
- **Insurance:** plans and benefits, estimates, printed and electronic claims (X12), electronic remittances.
- **Reports:** standard reports, custom RDL reports and dashboards.
- **Running the practice:** multiple clinics, users and permissions with an audit trail, task lists, a wiki, email, HL7 interfaces, a Middle Tier server.
- **New in this fork:** inventory tracking and routine maintenance tracking.
- **Under the hood:** .NET 10, 64-bit and self-contained (the program carries its own runtime); MariaDB; encrypted database connections with the office's own CA; the database password kept with Windows DPAPI.

## Status and roadmap

November 2026, on `main`:

- **Independent of Open Dental Software, Inc.:** no eServices, registration keys, update servers or help browser; `NoHqTests` checks on every run that nothing calls Open Dental's servers.
- **Open Dental features up to 26.1** on Open Dental's 26.2.0 schema. An Open Dental 24.3.63 or 25.3.48 database can be adopted (rehearsed on synthetic data).
- **Version 0.0** was released (the last .NET Framework 4.8, 32-bit build). `main` is .NET 10, 64-bit.
- **In progress: a UI refresh** ([docs/specs/ui-refresh-plan.md](docs/specs/ui-refresh-plan.md)): new colours, controls, window frame and Fluent icons are done; the program font is Segoe UI 9 pt; every window is now being laid out on one spacing grid.

Next, after the UI refresh: the next release and the office switch-over from Open Dental. The full roadmap with the owner's decisions is [docs/roadmap.md](docs/roadmap.md).

## Building

You need Windows 11 64-bit and:

- **Build Tools for Visual Studio 2026** (or Visual Studio 2026), with the .NET desktop, C++ (MSVC v143) and testing tools; one command installs them, see [docs/building.md](docs/building.md#prerequisites).
- **The .NET SDK that `global.json` pins** (10.0.115), machine-wide or per user.
- **The offline package folder.** The build downloads nothing: `nuget.config` restores from the folder that `OPMS_NUGET_OFFLINE` names, with the packages of `Directory.Packages.props`.

Get the source with its submodules (PDFsharp/MigraDoc and WeCantSpell.Hunspell are built from their upstream source):

```bash
git clone --recurse-submodules https://github.com/Marysville-Dental-Center/OpenPMS-Dental.git
```

Build `OpenDental.Local.slnf` (the program, its libraries, services and tools, and every test project under `tests/`) in Visual Studio as `Debug | x64`, or from PowerShell at the repository root:

```powershell
. .\tools\Initialize-BuildEnvironment.ps1; Initialize-BuildEnvironment -RepoRoot (Get-Location).Path
& "C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe" OpenDental.Local.slnf -restore -p:RestoreLockedMode=true -p:Configuration=Debug -p:Platform=x64 -m -nodeReuse:false
```

The program is `OpenPmsDental\bin\Debug\OpenPmsDental.exe`. Run the tests (they need MariaDB 10.11 on `127.0.0.1:3306`, user `root`, blank password):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\run-tests.ps1 -Mode Core -Platform x64 -RequireBuildOutput
```

Create a development database (the seed's fictional demo practice; log in as `Admin`, blank password) and point the program at it:

```powershell
powershell -ExecutionPolicy Bypass -File tools\New-DevDatabase.ps1 -WithSampleData -WriteConfig
```

The full reference, with the solution layout, version numbers and every option, is [docs/building.md](docs/building.md). The release build and the setup are in [docs/release-process.md](docs/release-process.md).

## More

- [Installing in an office](docs/installing.md): the database server, the workstations and updates.
- [Repository layout](docs/repository-layout.md): what each folder holds, the branches, Open Dental's own resources.
- [Owner decisions and questions](docs/owner-questions.md) and the [port notes](docs/port-notes/) of every package.

## License

The source files say "either version 2 of the License, or (at your option) any later version", so the program is licensed under the **GNU GPL v2 or later**; [LICENSE](LICENSE) holds the version 2 text. [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) lists the third-party components the program ships.

The program as distributed includes PDFsharp and MigraDoc 6.5, which are licensed under the **GNU AGPL v3**. The AGPL combines with GPL version 3 but not with version 2, so the program with them is distributed under the **GNU GPL v3** (which "or later" allows), and PDFsharp and MigraDoc keep the AGPL's terms. By section 13 of both licenses, the AGPL's requirements on interaction through a network apply to the combination as a whole. They apply to a modified version, and the PDFsharp built here is itself one (one file replaced, see below), so they cover any version of the program that users reach over a network, not only versions someone else modifies: a hosted Middle Tier, even one without local changes, must prominently offer its users the Corresponding Source of the whole program, such as a link to the release's `OpenPmsDental-<version>-source.zip`. [LICENSE-GPL-3.0.txt](LICENSE-GPL-3.0.txt) and [LICENSE-AGPL-3.0.txt](LICENSE-AGPL-3.0.txt) hold those texts; [NOTICE](NOTICE) explains it.

PDFsharp and MigraDoc 6.5.0 (empira's AGPL source, tag v6.5.0) and WeCantSpell.Hunspell 7.0.1 are built here from their source, git submodules under `third-party/`. The PDFsharp built here replaces one file, empira's wrapper around its closed `PdfSharp.License.dll`, with an open one (a modified version under AGPL section 5a), so every file the setup installs has published source. Each release publishes its complete source as `OpenPmsDental-<version>-source.zip` next to the setup ([docs/third-party-source-builds.md](docs/third-party-source-builds.md)).

Names that existing installations and databases depend on keep Open Dental's spelling: the database, `FreeDentalConfig.xml`, the Middle Tier's service names, and internal type names. The program's own assemblies carry the code name `OpenPmsDental` ([docs/port-notes/RENAME.md](docs/port-notes/RENAME.md)).
