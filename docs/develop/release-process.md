# Release process and version guard

Status: 2026-10-19. Implements decoupling plan Phase 4 (our own release channel, local only) and backlog package M1-03 with Risk R1 ([specs/port-backlog.md](../specs/port-backlog.md)).

This page covers four things:

- how version numbers work
- what the program checks at startup
- how an Open Dental database is adopted
- how a new build reaches every workstation

Nothing here talks to Open Dental's servers or reads Open Dental's update files.

## Two kinds of version

| Version | Value now | Where it lives | Used for |
|---|---|---|---|
| Release (product) version | `0.1.1` | `AssemblyInformationalVersion` in `SharedAssemblyInfo.cs` (repository root, linked into every shipped assembly) | About window (`Application.ProductVersion`), release manifests, the `ForkProgramVersion` marker. Our own scheme. It is never compared with Open Dental numbers. |
| Compatibility version | `24.3.63.0` | `AssemblyVersion` (file version), `ForkVersions.CompatVersion` | The frozen legacy conversion chain, and `DataBaseVersion`/`ProgramVersion` on databases the fork markers don't govern yet. |

Raise the release version by editing `AssemblyInformationalVersion` in `SharedAssemblyInfo.cs`, for example to `0.1.1` (two or three numbers: `0.1`, `0.1.1`; `ForkVersions.CompareReleaseVersions` orders them by number). Leave `AssemblyVersion` at `24.3.63.0`. Once the fork markers take over, the database checks no longer look at it.

A build is identified by its **build id**: a SHA-256 over the program's own assemblies (`ForkReleases.ListBuildIdFiles`: `OpenPmsDental.exe`, `OpenPmsDental.dll` (since stage 3b the `.exe` is the apphost and the program's code is in the `.dll`), `OpenPmsDentalBusiness.dll`, `CodeBase.dll` and the others). Every rebuild of a release (`0.1`) therefore counts as a different build, and third-party DLLs aren't hashed at every startup.

Because the release version stays the same across the rebuilds of a release (`0.1`), it can't tell an older build from a newer one. Each build therefore also carries its **build time**. `OpenPmsDental.csproj` (target `ForkWriteBuildStamp`) writes `[assembly: AssemblyMetadata("ForkBuildUtc", "…Z")]` into `OpenPmsDental.exe`. It rewrites the stamp only when something compiled into `OpenPmsDental.exe` changed (its sources, references, resources, the project file or `UpdateFileCopier.exe`), so an unchanged rebuild keeps its build id. The release manifest records the stamp as `BuildUtc`. A higher release version is newer. Within one release version, the later build time is newer. If either time is missing or the two are equal, the order is unknown.

## Fork version markers

These are four preference rows. The fork reads and writes them with plain SQL (`ForkVersions`), so they need no `PrefName` entry. None of the names is an Open Dental preference.

| Preference | Meaning |
|---|---|
| `ForkSchemaLevel` | The last fork migration step applied. `0` right after adoption. If this row exists, the database is adopted. |
| `OdSchemaCompat` | The Open Dental schema the database matches exactly (`26.2.0` once every fork migration step has run, `26.1.70` before package P262-SCHEMA; `25.3.48` or `24.3.63` right after adoption). The fork migration runner and schema verifier advance it. |
| `ForkAdoptedFrom` | The adoption record: `source=25.3.48.0;started=…;adopted=…` (UTC). If it has no `adopted=` and there is no `ForkSchemaLevel`, the adoption didn't finish. If it has `adopted=` but `ForkSchemaLevel` or `OdSchemaCompat` is missing, the markers of an adopted database were removed: that database is refused, never adopted again. |
| `ForkProgramVersion` | The release published from the update server: `release+buildId`, for example `0.1+3f2a…`. |

The fork migration runner also keeps `ForkMigrationsApplied`, the comma-separated levels of every step that finished on the database (`ForkSchemaLevel` is the highest of them), and `ForkMigrationInProgress`. The startup checks read `ForkMigrationsApplied` too.

Adoption also sets `DataBaseVersion` and `ProgramVersion` to the **sentinel** `99.0.0.0`. No Open Dental release will reach that number. An Open Dental client therefore sees a "newer" database: it skips its own conversion, finds no update files and can't use the copy. The fork reads those two preferences only to recognize an unadopted database.

## Startup checks

`FormOpenDental.PrefsStartup` calls `PrefL.CheckForkVersionGuard` once the preference cache is loaded and before anything writes to the database (index re-enabling, the MySQL version backup and repair, any conversion), then once more after conversion. The guard itself only reads. Debug builds run the guard too. Open Dental skipped its version check in Debug builds; the fork doesn't, so a developer build pointed at an office copy can't convert or adopt it by accident. The decision is `ForkVersionGuard.Evaluate`:

| Database | Result |
|---|---|
| Unadopted, `DataBaseVersion` ≤ 24.3.63 | Runs. The frozen legacy chain converts older databases up to 24.3.63, never further. If the fork migration runner has steps and the database is at 24.3.63, startup asks for adoption instead. |
| Unadopted 25.3.48.0 (on the allowlist) | Refused (exit 171). Startup only offers to open the adoption tool. |
| Unadopted, any other Open Dental version (25.3.50, 25.4.x, 24.4.x, …) | Refused (exit 170). Nothing is changed. |
| Adopted, markers and sentinel consistent | Runs. Pending fork migration steps run on the update server, after a backup. A step is pending when its level is not in `ForkMigrationsApplied`, also when it is below `ForkSchemaLevel` (a step merged later with a lower level); right after adoption (`ForkSchemaLevel` `0`, no list) every step is. |
| Adopted by a newer release (`ForkSchemaLevel`, or a level in `ForkMigrationsApplied`, is not a level of this build) | Pulls the published release if there is one, otherwise refused (exit 172). |
| Adopted, but `DataBaseVersion`/`ProgramVersion` changed, or markers incomplete | Refused (exit 173). |
| Sentinel versions with no markers (markers removed) | Refused (exit 173). Restore a backup. |
| `ForkAdoptedFrom` says `adopted=`, but `ForkSchemaLevel` (or `ForkSchemaLevel` and `OdSchemaCompat`) was removed | Refused (exit 173). This is not an unfinished adoption: the adoption tool refuses it too, so the schema level is never reset and the fork migration steps never run a second time. Restore a backup. |
| Adoption started but not finished (`ForkAdoptedFrom` without `adopted=`, no `ForkSchemaLevel`) | Refused. The adoption tool can finish it. |

No case offers a "downgrade". Open Dental's `UpdateFiles` and `UpdateFilesSegment` rows are never read. The only code that names them is `DocumentMiscs.DeleteOpenDentalUpdateFiles`, which adoption calls.

When an inactive workstation becomes active again, `FormOpenDental.IsDbConnectionSafe` runs the same guard, in Debug builds too. In Release builds, a workstation also restarts when a different build has been published, unless it may publish its own build under the rules below (`ForkReleases.MayRunUnpublishedBuild`): the named update server, or, with a blank `WebServiceServerName`, a build known to be newer. A blank preference never exempts a stale build.

The legacy `ConvertDatabases` chain is frozen at 24.3.63 (`ConvertDatabases.FrozenVersion`). Adding a `ToX_Y_Z` method above it throws, and so does running the chain on a newer database. Later schema changes are fork migration steps. `PrefL.ConvertDB` hands an adopted database to the runner through `IForkMigrationRunner` (`ForkMigrationHandOff`), which finds `ForkMigrationRunnerAdapter`: it runs M1-01's `ForkMigrationRunner`, which runs every step whose level is not in `ForkMigrationsApplied`, and, when the schema verifier then finds an exact match, sets `OdSchemaCompat` to the verifier's reference version (`26.1.70` since M2-01). The guard asks the runner only for its step levels (`GetLevels`), never how it finds its steps, and runs it only when one of them is not in `ForkMigrationsApplied`. Levels are dated `yyyyMMddNN` from 2026 on (owner decision of 2026-11-01), every new step above every existing one; a released level is never removed or renumbered, since a database level this build lacks means a newer build. The baseline runner with no steps is used only when no runner is found. `tools/New-DevDatabase.ps1` adopts the 24.3.63 database it builds, so a development database carries the same markers as an adopted office copy.

## Adopting a database

Adoption is explicit and runs once, on a restored copy. Normal startup never adopts. For an adoptable database (`24.3.63.0` or `25.3.48.0`; the pair `25.3.47.0` / `25.3.54.0`; or any `26.2.x`, `ProgramVersion` on the same line and not below `DataBaseVersion`), the startup message offers to open **Adopt Database** (`FormAdoptDatabase`). The pair and the 26.2 line are adopted only when the schema equals the reference exactly (25.3.48, or 26.2.0: Open Dental's published 26.2 database documents, owner decision of 2026-11-26), and are recorded as that schema version with the real labels in `ForkAdoptedFrom`. The administrator must confirm three things: that this is a restored copy with a backup, the exact database name, and one final OK.

For adoption, the runbook does not use this window: `AdoptionValidation adopt` (runbook step 4) runs the same `Adoption.Run` on a copy that the database server made of the Open Dental database (runbook step 3.2), without starting the program, because starting it and logging in write to the database before the post-adoption validation (runbook step 6.1). The M1-Z rehearsal copies and adopts with the same code.

`Adoption.Run` performs these steps, in this order. Each step is idempotent. MyISAM has no transactions. A run interrupted before step 6 is finished by running it again. A run interrupted after step 6 is not: `ForkSchemaLevel` then marks the database as adopted while `ForkAdoptedFrom` has no finish time, `Adoption.Run` refuses it as already adopted, and `AdoptionValidation adopt` fails on it, closed (validation report, F13). The office runbook therefore never resumes an adoption: after any stop or failure it drops the copy and adopts a new one.

1. Write `ForkAdoptedFrom` with the start time.
2. Keep `patient._portal_active_email` and its unique index `portal_active_email_uniq` as they are (owner decision of 2026-11-25). This is a patient portal column, not Open Dental schema, that a patient portal relies on: two active patients cannot share an e-mail address. The schema check allows the column and key only with the expected definitions; any other key on the column is dropped.
3. Delete Open Dental's `UpdateFiles` and `UpdateFilesSegment` rows from `documentmisc` without reading them.
4. Blank the preferences that point at Open Dental's servers. This covers a fixed list (`WebServiceHQServerURL`, `UpdateServerAddress`, `UpdateWebsitePath`, `WebHostSynchServerURL`, `MobileSyncServerURL`, `EmailHostingEndpoint`) plus any preference whose whole value is a web address on a blocked host (`CodeBase.VendorDirect.HqHosts`). Text that only mentions such a host is kept.
5. Set `DataBaseVersion` and `ProgramVersion` to `99.0.0.0`.
6. Write `OdSchemaCompat`, then `ForkSchemaLevel` (`0`). From this point the markers govern the database.
7. Run the pending fork migration steps (M1-01 runner).
8. Complete `ForkAdoptedFrom` with the finish time, and add a security log entry.

The preflight refuses these databases:

- any version that is not adoptable (above)
- `ProgramVersion` different from `DataBaseVersion` (24.3.63, 25.3.48), or below it (26.2)
- a schema that differs from the reference (the pair and 26.2)
- a database flagged `CorruptedDatabase`
- an update marked in progress
- an already adopted database
- an adopted database whose markers were removed (`ForkAdoptedFrom` says `adopted=`)
- a middle-tier connection

Re-entering bridge passwords is a separate step of the M1-Z rehearsal and of the office procedure in [adoption-runbook.md](../admin/adoption-runbook.md); `Adoption.Run` doesn't do it. `SecurityHash` columns are kept as they are: tamper detection is removed ([remove-tamper-detection.md](../specs/remove-tamper-detection.md)).

Before adopting the office copy, stop and uninstall the Open Dental services and clients (M1-Z). All workstations switch together. Rollback means going back to the untouched original (or, if something wrote to it, to the folder copy taken before the copy was made) after this program has been removed from every workstation and the adopted database has been dumped and dropped ([adoption-runbook.md](../admin/adoption-runbook.md), Rollback).

## Publishing a release

The **update server** is the workstation named in the `WebServiceServerName` preference (Update Setup). Name it before you publish rebuilds. If that preference is blank (the default), any workstation may act as the update server, but only within limits: it may publish the first release, or a build known to be newer than the published one (a higher release version, or the same release with a later build time). It never replaces a published build that is newer or whose order is unknown. It updates to that build instead.

1. Build the solution in Release x64. `OpenPmsDental.csproj` copies UpdateFileCopier's four files (`.exe`, `.dll`, `.runtimeconfig.json`, `.deps.json`) next to `OpenPmsDental.exe`, because every release ships it, and the 32-bit helper with its own x86 runtime into `Helper32\`.
2. Install the build on the update server with its setup ([Building the installer](#building-the-installer)), and start it as an administrator. Builds from before the rename to `OpenPmsDental.exe` (2026-11-01) cannot be upgraded in place, because UpdateFileCopier never deletes the exe and DLLs of the old names: install a renamed build into a clean folder ([port-notes/RENAME.md](../port-notes/RENAME.md)). The first release after 0.0 is published only when 0.0 is uninstalled on every workstation: until then, click Cancel at the publish offer below ([db-connection-security.md](../admin/db-connection-security.md), section 0).
3. At startup, `PrefL.CheckProgramVersion` sees that this build isn't the published one and asks whether to publish it. If you click Cancel, this workstation runs the build without publishing it. Update Setup's Recopy button publishes the running build at any time, under the same rules: with a blank `WebServiceServerName` it refuses to replace a different published build that this one isn't known to be newer than. A silent run publishes only when nothing is published yet. It never replaces a published release, because nobody is asked. On the named update server, Recopy also replaces a published release whose manifest cannot be read, after asking (see [A manifest that cannot be read](#a-manifest-that-cannot-be-read)).
4. Publishing (`ForkReleases.Publish`) works like this:
   - While it runs, `UpdateInProgressOnComputerName` is set, so other workstations wait.
   - **The runtime part first** (stage 3b, decision 20 of [specs/net10-plan.md](../specs/net10-plan.md); [port-notes/S3b-7.md](../port-notes/S3b-7.md)). The .NET 10 release is self-contained: the x64 .NET runtime beside the programs and the helper's x86 runtime in `Helper32\` are about 250 MB, and they change only with a .NET patch. Its runtime files are the files the programs' `.deps.json` files name from the runtime packs (`ForkReleases.GetRuntimePart`); they must be of one version, and their key is the SHA-256 over the version and each file's path, size and SHA-256. When that key differs from the published runtime part's (the first .NET 10 publish, and each monthly runtime release), it zips them and writes them as `ForkRuntimeSegment` rows (`DocMiscType` 103), then their own manifest as one `ForkRuntimeManifest` row (`DocMiscType` 102), last (`DocumentMiscs.ReplaceForkRuntime`). With the same key it writes no runtime row.
   - It zips the installation folder (the program part), except local files: `FreeDentalConfig.xml`, logs, files with `__` in the name, Open Dental's `Manifest.txt`, the two manifests and the runtime files. The `OpenDentalReplicationService` and `Helper32` sub folders are included (a `Sparks3D` folder no longer is: Sparks3D was removed in BIN-A).
   - It also leaves out the files an earlier release shipped and this one no longer does (`ForkReleases.ListRetiredFiles`, such as the storage library of v0.0, [port-notes/RENAME.md](../port-notes/RENAME.md#storage-rename-2026-11-02)). UpdateFileCopier never deletes a file, so a workstation that pulled a release still has them; nothing loads them, and this way it cannot pass them on when it publishes.
   - It writes the zip as `ForkReleaseSegment` rows (`DocMiscType` 101, about 1 MB of base64 each).
   - It then writes the manifest as one `ForkReleaseManifest` row (`DocMiscType` 100).
   - Finally it sets `ForkProgramVersion`.
   - Open Dental's types are 0 to 2; the column is `tinyint(4)`.

The manifest (`ForkRelease.manifest`, `ReleaseManifest`) names the product `Open PMS - Dental`. It also records:

- the release version, build id, build time and compatibility version
- the publish time and the workstation that published it
- the SHA-256 of the archive
- for every file: its path, size and SHA-256
- optionally, the runtime the release runs on (`Runtime=Microsoft.NETCore.App 10.0.14 win-x64`) and the runtime part it needs
  (`RuntimeVersion`, `RuntimeArchitectures`, `RuntimeArchiveSha256`, `RuntimeKey` and one `RuntimeFile` line per runtime file). The .NET
  Framework releases write none of these; a manifest without a `Runtime` line is a .NET Framework 4.8 x86 release. The .NET 10 release
  writes them (it carries its own runtime, published as a part of its own). The keys, their checks and an example are in
  [port-notes/S3a-10.md](../port-notes/S3a-10.md).

The archive holds a copy of the manifest without the archive hash.

A workstation never treats a manifest it cannot read, or one that names another product, as "nothing published": it neither publishes
over it nor updates from it, says so at startup and keeps running its build. A release for another runtime or architecture than the
workstation's own (the .NET 10 release is 64-bit and carries its own runtime) is refused the same way, with "run the new setup on it":
the new setup installs that release into its own program folder. Update Setup's Recopy refuses both cases too. The one exception is
the switch itself (section 10 step 3 of [specs/net10-plan.md](../specs/net10-plan.md)): a .NET 10 build publishes over a .NET Framework
release by the usual rules for publishing (the named update server, or a newer build with a blank `WebServiceServerName`), at its first
start after the new setup or through Recopy, and writes its `Runtime` line so the .NET Framework workstations refuse it. It never pulls a
.NET Framework release, and no .NET Framework build ever publishes over a .NET release.

### A manifest that cannot be read

If the published manifest row is really damaged (it is written last and in one statement, so a broken publish leaves the old row), the
named update server can replace it (S3a-10f, the owner's decision of 2026-11-15; [port-notes/S3a-10.md](../port-notes/S3a-10.md) section 12).
At startup its message about the unreadable manifest ends with "Recopy in Update Setup offers to replace the published release with this
build". Update Setup > Recopy on that computer then asks, Yes/No with No as the default, whether to replace it: Yes deletes the
`documentmisc` rows of types 100 and 101 and publishes this build as for a first release, and the other workstations update to it when
they start. A security log entry (Update Install) records the replacement and the reason. No changes nothing. Answer No when a newer
version of the program may have published that release (a newer version installed on another computer): its manifest is unreadable to an
older build, and Yes would replace it with the older build. Nothing else ever replaces it: a silent run, startup, a blank
`WebServiceServerName` and every other workstation only refuse, as above.

### What the other workstations do

At startup each workstation compares its build with the published one (`ForkReleases.Decide`):

| Situation | Named update server (`WebServiceServerName` is this computer) | `WebServiceServerName` blank | Another computer is named |
|---|---|---|---|
| Same build id | Nothing. | Nothing. | Nothing. |
| Nothing published yet | Offer to publish. | Offer to publish. | Nothing. |
| A newer build is published (higher release, or same release with a later build time) | Pull. | Pull. | Pull. |
| This build is newer (higher release, or same release with a later build time) | Offer to publish. | Offer to publish. | Refused (exit 174), with a message to run the new build on the update server first. |
| Same release, different build, order unknown | Offer to publish. | Pull. The message says how to publish this build instead: name this computer as the update server. | Pull. |
| Debug build | Never publishes or pulls. The version guard still runs. | Same. | Same. |
| Published manifest not readable, or for another product | Refused: neither publish nor pull; the message says why; the workstation runs on. | Same. | Same. |
| A release for another runtime or architecture (the .NET 10 release on a .NET Framework workstation) | Refused: "run the new setup"; the workstation runs on. | Same. | Same. |
| A .NET Framework release on a .NET 10 build (the switch) | Offer to publish, unless the published release is newer: then refused. | Offer to publish when this build is newer; otherwise refused. | Refused: the message says to run the new setup on the update server; the workstation runs on. |

**Pull** means: the workstation downloads the release and checks it: product name, archive SHA-256, only listed files, safe paths, archive manifest equal to the published one, and every file's size and SHA-256. A release for another runtime or architecture is refused before anything is downloaded. **The runtime part** comes along only when the release's runtime key differs from this workstation's (the key its own `ForkRelease.manifest` names, or for a fresh install the key over its runtime files): then it is downloaded into the same folder and checked in full too (its manifest row must be the one the release names, its archive must match the SHA-256 the release names, and every runtime file its size and SHA-256), so no runtime file of another version is ever copied. The workstation then asks for OK (the message says when a new .NET runtime comes with it), checks the architecture once more, copies UpdateFileCopier's files (the exe, and its `.exe.config`, `.dll`, `.runtimeconfig.json` and `.deps.json` when the release has them) and the x64 runtime files the copier's `.deps.json` names into `%TEMP%\openpms-dental\ForkReleaseCopier` (from the download when the runtime part came along, otherwise from the program folder, whose runtime is then the same version; a copier for another runtime version is refused) and starts it there through ShellExecute, so Windows asks for administrator consent (the copier's manifest asks for administrator rights). UpdateFileCopier stops every process that runs from the program folder (the program, ServiceManager, the connection settings tool, the helper, the HL7 service: they all hold the shared runtime files), checks the manifest and every file again, both in the download and in its local copy, copies the files (`Helper32\` too), then checks every runtime file of the program folder against the release's runtime part and only then writes the manifest, last, and restarts the program. If a runtime file does not match, it writes no manifest and says to run the setup, which carries the full runtime: a workstation never claims runtime files of two versions. If any check before the copy fails, nothing on the workstation changes.

No row offers a downgrade. A pull always goes to a build that is newer, or, when the order is unknown, to the build the office published. A workstation still running an old build is never asked to publish it over the office's release. Only the named update server, which the office chose, can replace a build it can't prove older.

Dynamic mode, where Open Dental kept per-version program folders, isn't part of the fork's release channel.

## Building the installer

A release is `OpenPmsDental-<version>-Setup.exe`, made by `tools/Build-Release.ps1` with Inno Setup 6 (6.7.4 for 0.0, 0.1 and 0.1.1):

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Build-Release.ps1 -BuildLane <path of odbuild.ps1> -CopyTo <release folder>
```

Without `-BuildLane` the script finds MSBuild with vswhere; `-SkipBuild` uses the Release output that is there; `-SkipInstaller` stops after the staging folder (the source archive is still written); `-Iscc` names `ISCC.exe` when Inno Setup is not in its default folder. Build from a clean checkout of the release commit with the submodules checked out at their pins (`git -c protocol.file.allow=always submodule update --init` offline from the local mirrors, or `git submodule update --init` from GitHub; [third-party-source-builds.md](third-party-source-builds.md)): before it builds, the script stops with exit 4 when `git` or a submodule is missing, and with exit 1 when a submodule is checked out at another commit than the release commit pins, because the shipped PDFsharp, MigraDoc and WeCantSpell.Hunspell files are built from that checkout. The script:

1. Builds as Release x64 (PDFsharp/MigraDoc and WeCantSpell.Hunspell from the submodules, by `third-party/build`; with `-LaneProjects OpenPmsDental,xDirect2dWrapper` only those and what they reference, through the lane's scoped restore, while some test projects are still .NET Framework) and checks `OpenPmsDental\bin\Release`. First the release-flow checks of the runtime: it refuses a build whose `.deps.json` or `.runtimeconfig.json` files name more than one runtime version, whose helper runs on another runtime version than the program, or whose runtime is older than the newest 10.0.x `Microsoft.NETCore.App.Runtime.win-x64` in the offline folder (the monthly runtime release: bump first, section 2.4 of [specs/net10-plan.md](../specs/net10-plan.md)). Every file there must be on one of the script's three allow-lists (the program's own files, the third-party files, and the runtime files, which the script reads from the runtime packs' own `data\RuntimeList.xml`: NETCore and WindowsDesktop for win-x64 at the top, and for win-x86 in `Helper32\`, Windows Forms files only) or be one that never ships (`*.pdb`, the XML documentation). A file on the deny-list (Open Dental's closed binaries, the commercial and non-redistributable files, `FreeDentalConfig.xml`, debug symbols, an uninstaller, the pre-rename names, the files of earlier releases that this one no longer ships), any other file and any sub folder but `Helper32\` stop the script. A `bin\Release` folder left from an older build can hold such a file: build clean.
2. Stages one folder, `installer\bin\staging` (ignored by git): the allowed files, with `LICENSE`, `LICENSE-GPL-3.0.txt` and `LICENSE-AGPL-3.0.txt` (PDFsharp and MigraDoc are under the AGPL 3.0, so the program as distributed is under the GPL 3.0; NOTICE), `LICENSE-LGPL-2.1.txt` (the LGPL 2.1 asks for its text with every copy of `RdlEngine.dll`, `RdlViewer.dll` and `WeCantSpell.Hunspell.dll`), `NOTICE`, `THIRD-PARTY-NOTICES.txt`, `db\init\new-database.sql` and `db\README.md`. It checks it again: nothing outside the lists, every allowed file there (with the reason for `WebView2Loader.dll`, `WeCantSpell.Hunspell.dll`, `ODR.dll`, `PdfSharp-gdi.dll` and the licence texts), every third-party file named in `THIRD-PARTY-NOTICES.txt`, every file built here equal to its project's own output (`RdlEngine.dll`, `RdlViewer.dll`, and PDFsharp, MigraDoc and WeCantSpell.Hunspell from `third-party\build\<project>\bin\Release`) (so a prebuilt `ImagingDeviceManager.dll`, `ODR.dll` or `PdfSharp-gdi.dll` can't ship in place of the one built from source), every third-party file of a NuGet package byte for byte the package's own file at the version `OpenPmsDental\packages.lock.json` names (the asset the program's restore copies, compared with the package in the global packages folder the restore used, whose content hash must be the lock file's, and in the offline package folder; a file of another version fails with the package, both versions and the paths in the message), every other third-party file (the Direct Project; SharpDX and Tao left the release in TC-9b) equal to its `Required dlls` copy, every runtime file byte for byte its runtime pack's file, the PE machine of every file (0x8664 for every apphost, native DLL and the x64 runtime, a managed file x64 or IL only; 0x14C in `Helper32\`, the helper's assembly 32-bit required), the native files free of the debug C runtime, and `OpenPmsDental.exe`, `OpenPmsDental.dll` and `OpenPmsDentalBusiness.dll` at file version 24.3.63.0 and product version the release version. empira's closed `PdfSharp.License.dll` is on the deny-list: the PDFsharp built here holds the AGPL licence check itself, so no release ships that file and no release is blocked by it ([owner-questions.md](../decisions/owner-questions.md), "`PdfSharp.License.dll` had no published source").
3. Prints the build id (the same computation as `ForkReleases.ComputeBuildId`) and writes `installer\bin\SHA256SUMS.txt`.
4. Writes the setup's information page (`installer\setup-info.txt` with `NOTICE`) and License Agreement page (`installer\setup-license.txt`: as installed, the program is conveyed under the GNU GPL 3.0, PDFsharp and MigraDoc under the AGPL 3.0, and the program's own source files are GPL 2.0 or later; where the Corresponding Source is; then the text of `LICENSE-GPL-3.0.txt`) into `installer\bin`, takes the 64-bit `TWAINDSM.dll` and the 32-bit one (`x86\TWAINDSM.dll`, S3b-8) from the approved download of the TWAIN Working Group's release (`-TwainDsmZip`; by default `od-tools\downloads\s3b7-twaindsm-20261116\dsm_020501.zip` in the common git folder) after checking each one's pinned SHA-256, its version, its PE machine and its TWAIN Working Group signature, compiles `installer\OpenPmsDental.iss` into `installer\bin`, checks that the files the compiler packed are exactly the staged files and the two `TWAINDSM.dll`, and adds the setup's SHA-256 to `SHA256SUMS.txt` (first line; the staged files follow as `staging/...`).
5. Writes the source archive, `installer\bin\OpenPmsDental-<version>-source.zip`: the repository at the release commit (`git archive HEAD`) with every submodule at the commit that commit pins, under one top folder `OpenPmsDental-<version>-source`, the source of the setup's `TWAINDSM.dll` as `third-party-source/twain-dsm-2.5.1.zip` (byte for byte the TWAIN Working Group's `twain-dsm-2.5.1.zip` of release v2.5.1, the approved download beside `dsm_020501.zip`; `-TwainDsmSourceZip` names another copy of it), and `SOURCE-COMMITS.txt` naming the commits, that file with its SHA-256, and the upstream addresses. It is the Corresponding Source of everything the setup installs, PDFsharp, MigraDoc, WeCantSpell.Hunspell and `TWAINDSM.dll` (LGPL 2.1 section 4) included, and stays available with the release even if an upstream repository goes away. Before it builds, the script stops with exit 4 when that TWAIN DSM source download is missing and with exit 1 when its SHA-256 is not the one the script pins (`$TwainDsmSourceSha256`); the archive is written from the bytes it checked. Its SHA-256 is the second line of `SHA256SUMS.txt`. `-WriteSourceArchive <folder>` writes only the archive. A working tree with changes that are not committed gets a warning: the archive holds the commit, so build releases from a clean checkout.
6. With `-CopyTo`, copies the setup, the source archive, `SHA256SUMS.txt` and `docs\release-notes\v<version>.md` there.

**Tag, no branch:** a release is the tag `v<version>` on the release commit of `main` (`git tag -a v<version> -m "Open PMS - Dental <version>"`, `git push origin v<version>`), and it is built from the tag (`gh workflow run release-build.yml --ref v<version>`, below). The tag alone names the released source, so no `release/<version>` branch is made (owner decision of 2026-11-26). Only when a fix must ship for an older release after `main` has moved on is `release/<version>` made from that release's tag (`git switch -c release/<version> v<version>`); the fix is committed there and to `main`, and the fixed release gets its own tag on that branch.

**Publishing:** attach the setup, the source archive `OpenPmsDental-<version>-source.zip`, `SHA256SUMS.txt` and the release notes to the GitHub release of the tag. The source archive is the release's Corresponding Source (GNU GPL 3.0 and AGPL 3.0 section 6, LGPL 2.1 section 6, and LGPL 2.1 section 4 for the setup's `TWAINDSM.dll`, whose source it carries): the setup's information page, its License Agreement page, NOTICE and Help > About > View Licenses tell the user it is there.

The release version comes from `SharedAssemblyInfo.cs` and must equal `#define AppVersion` in `installer\OpenPmsDental.iss` (and its `VersionInfoVersion`, the release version padded to four numbers: `0.1.1.0` for `0.1.1`); the script stops when they differ. A new third-party file goes on the allow-list and into `THIRD-PARTY-NOTICES.txt`; the script fails until it is in both.

What the setup does, for the release channel:

- It is a 64-bit setup (`ArchitecturesInstallIn64BitMode=x64compatible`, decision 19): it installs exactly the staging folder, with `Helper32\` and both runtimes, into `C:\Program Files\Open PMS - Dental` (per machine, as administrator; it has no per-user mode, decision 7 of [docs/specs/net10-plan.md](../specs/net10-plan.md), so its shortcuts go to the common Start menu and desktop and the Event Log source is always registered). A workstation installed from the same staging folder therefore has the same build id as the update server that published it, and nothing happens at its start.
- It installs no .NET runtime and checks for none: the program carries its own (decision 20), so a silent install needs nothing for .NET either. `/PREREQS=ask|download|skip` covers only the x64 Visual C++ redistributable (its key read in both registry views; the download from Microsoft runs only with a valid Microsoft signature). Below Windows 11 it warns, with Microsoft's list of the Windows versions .NET 10 supports, and goes on when the user agrees; `MinVersion` (Windows 10 1903) is the hard floor.
- It installs the 64-bit `TWAINDSM.dll` into the 64-bit system folder when it is missing or older, as a shared file the uninstall leaves in place; never into the program folder, where the program would load it first.
- It installs the 32-bit `TWAINDSM.dll` of the same TWAIN Working Group release into the 32-bit system folder (`SysWOW64`) the same way (missing or older, a shared file counted in the 32-bit view of SharedDLLs, left in place by the uninstall), for TWAIN sources with only a 32-bit driver, which run in the 32-bit helper (`docs/port-notes/S3b-8.md`).
- On an upgrade it first deletes the files of earlier releases that this one no longer ships (`[InstallDelete]`, the files of `ForkReleases.ListRetiredFiles`; `InstallerTests` checks the two agree).
- The uninstaller is in the `uninstall` sub folder. A release publishes the top-level files only (and `OpenDentalReplicationService` and `Helper32`), so neither the uninstaller nor the `db` folder reaches the other workstations.
- It writes `FreeDentalConfig.xml` only when the folder has none, with no password, and gives Users no rights on it: the file keeps the program folder's rights, so only an administrator changes the connection settings. A release never carries the file.
- It then runs the connection settings tool `OpenPmsDentalDbSettings.exe` ([docs/specs/db-connection-security.md](../specs/db-connection-security.md) section 6): for a new connection to a server over the network or with a security switch (`/silent` in a silent install, which ends with exit code 10 when the tool fails), and in an interactive install for a database on the same computer too (password mode). On an upgrade (the file exists) it runs the tool with the security switches given, or only the tool's `/check` without them.
- The uninstall removes the whole folder, including files a pull added, but keeps a sub folder that looks like an A to Z folder. It never touches the database.
- Setup never starts the program. `RestartApplications=no` keeps it from restarting a program it closed.

`tools/Test-Installer.ps1` is an elevated test install, which the owner runs at each Release dry run from PowerShell opened with "Run as administrator" (open point O-3 of the .NET 10 plan; an agent never runs it); unelevated it changes nothing and ends with exit code 2. The one line to paste, with `<repo>` the repository folder and `<dryrun>` the dry run's `-OutDir`: `powershell -NoProfile -ExecutionPolicy Bypass -File "<repo>\tools\Test-Installer.ps1" -Setup "<dryrun>\OpenPmsDental-<version>-Setup.exe" -Staging "<dryrun>\staging"`. It installs the setup for all users, silently, into new folders under `%TEMP%` (`/DIR=`), and checks the files against the staging folder, `FreeDentalConfig.xml` (and that setup added no access rule to it), the all-users uninstall entry (`HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall`, the 64-bit view, since stage 3b), the program's own runtime (the installed connection settings tool loads `coreclr.dll` from the install folder, not from `%ProgramFiles%\dotnet`), the installed files against Build-Release's list (the program part, the runtime part, `Helper32\`), the 64-bit `TWAINDSM.dll` and the Event Log source key, a reinstall, a refused folder, and a server over the network without the office CA (the tool runs `/silent` and fails, setup ends with exit code 10); the other scenarios use a database server on the same computer, so no tool is needed. After each uninstall it checks that the folder, the uninstall entry and the Event Log source key are gone, and at the end it deletes what it made; the uninstall entry and the Event Log source key of an installation already on the PC are saved first (also as `.reg` files in the log folder) and written back. It never starts the program and never connects to a database; its header lists everything it changes. `tests/ReleaseFlowTests/InstallerTests.cs` runs the script's list checks (`-CheckFolder`) against fake staging folders and checks the two setup pages it writes (`-WriteSetupTexts`); `tests/ReleaseFlowTests/PackageFilesTests.cs` runs a release's staging checks (`-CheckFolder <folder> -Full`) on a folder of the packages' own files and on one with a file of another version of its package; `tests/PdfLibraryTests/SourceArchiveTests.cs` writes the source archive (`-WriteSourceArchive`) and checks that it holds the repository and the submodules at their pins.

## Building on GitHub Actions

`.github/workflows/release-build.yml` runs the same `tools/Build-Release.ps1` from source on GitHub's own Windows runner (`windows-2025-vs2026`: Visual Studio 2026 with MSBuild 18, MSVC 14.44 and the Windows 11 SDK; the Build Tools 2026 are installed only on an image without MSBuild 18). It is started by hand (or by the nightly build, below): Actions > Release build > Run workflow, or `gh workflow run release-build.yml --ref v<version>` (a release is built from its tag; any branch works for a trial build; the optional `label` input names the artifact; by default the commit's short SHA). It needs no secrets and has read access to the repository only.

The runner has no offline package folder, so the workflow fills one (`OPMS_NUGET_OFFLINE`, laid out as a global packages folder) with a locked restore of `OpenDental.Local.slnf` from nuget.org: exactly the packages and content hashes the committed lock files pin. It downloads the TWAIN DSM release (`dsm_020501.zip`, `twain-dsm-2.5.1.zip`) from the TWAIN Working Group's GitHub release and Inno Setup 6.7.4 from its official release (into its own folder, whatever version the image has), each checked against a pinned SHA-256, and passes them to `Build-Release.ps1` (`-TwainDsmZip`, `-TwainDsmSourceZip`, `-Iscc`, `-MSBuildPath`, `-OutDir`; no `-BuildLane`). Every check of the script runs as on a local PC, and any failure fails the run.

It uploads one artifact, `OpenPmsDental-unsigned-<label>`, kept 30 days: `OpenPmsDental-<version>-Setup.exe`, `OpenPmsDental-<version>-source.zip`, `SHA256SUMS.txt`, the `staging` folder and `setup-files` (the two `TWAINDSM.dll`). Everything in it is **unsigned**: it is the build that code signing (SignPath for open source, or Microsoft Artifact Signing) will sign once that is set up. Until then a release is published unsigned from this artifact (0.1 and 0.1.1): run the workflow on the tag, check the artifact's `SHA256SUMS.txt` against its setup, and attach the setup, the source archive, `SHA256SUMS.txt` and the release notes to the GitHub release of the tag (`gh release create v<version> --title "Open PMS - Dental <version>" --notes-file docs/release-notes/v<version>.md ...`).

### The nightly build

`.github/workflows/nightly.yml` runs the same release build on `main` every night (10:17 UTC) and uploads `OpenPmsDental-nightly-<yyyyMMdd>-<commit>`, kept 7 days: the setup, the source archive and `SHA256SUMS.txt` (no staging folder; a full artifact is about 500 MB). It skips the night when `main` has not changed since the last successful nightly build; Actions > Nightly build > Run workflow (`gh workflow run nightly.yml`) builds `main` at once. Each nightly build of `main` also replaces the **`nightly` pre-release** on the Releases page (job `publish`; the tag `nightly` moves to the built commit), so the newest setup has one fixed link: `https://github.com/Marysville-Dental-Center/OpenPMS-Dental/releases/tag/nightly`, with the setup as `OpenPmsDental-nightly-Setup.exe`, its source as `OpenPmsDental-nightly-source.zip` (the setup's Corresponding Source) and `SHA256SUMS.txt` naming them so. A pre-release is never the Latest release. It is free: GitHub's standard runners cost nothing for a public repository (the release builds bill 0 minutes), and so does the artifact storage. A nightly setup is an unsigned test build, not a release: it carries `main`'s release version, so it has the released version's file name (`OpenPmsDental-0.1.1-Setup.exe` until the next release) but a later build time, and installed on an update server it would be published as a newer build of that release ([What the other workstations do](#what-the-other-workstations-do)). Use it on test computers with test databases only (owner decision of 2026-11-26, [owner-questions.md](../decisions/owner-questions.md)).

## Tests

The tests are in `tests/ReleaseFlowTests` (in the solution and in `OpenDental.Local.slnf`). Run them with:

```
vstest.console.exe tests/ReleaseFlowTests/bin/Debug/ReleaseFlowTests.dll /Platform:x64
```

(.NET 10 x64 since S3b-7.)

- Guard decisions for the full startup matrix: fresh 24.3.63, office copy 25.3.48, copies edited to 25.3.50 and 25.4.0, adopted, adopted with markers removed, adopted by a newer build, unfinished adoption.
- Adoption on scratch databases with synthetic data. The 25.3.48 case uses the `od_253_schema` patient definition when it's loaded, otherwise the column and index as a 25.3.48 schema-only dump has them.
- Publish and pull round trips, including tampered segments, a tampered manifest and a manifest for another product.
- The release-channel guards (S3a-10): the `Runtime` and runtime part lines; an unreadable, a foreign and a .NET 10 manifest refused for
  every update server role and never downloaded; UpdateFileCopier's files copied and the copier started through ShellExecute; the
  execution level, versions and icon of every staged `.exe` against the net48 record (`ApphostResourceTests`, inconclusive without a
  Release build, and with a stale one: an exe missing or older than the newest commit to its project).
- Recopy over an unreadable manifest (S3a-10f, `ReplaceUnreadableDbTests`): on the named update server Yes replaces the rows and logs it,
  No changes nothing; another workstation, a blank `WebServiceServerName` and a silent run are never asked.
- The publish/pull decision for the named, blank and other update server cases, with and without build times; the restart rule of `IsDbConnectionSafe`; and the build time stamped into `OpenPmsDental.exe`.
- An adopted copy with only `ForkSchemaLevel`, or `ForkSchemaLevel` and `OdSchemaCompat`, removed: refused (exit 173) and never adopted again.
- The runtime part (S3b-7, `RuntimePartTests`, `RuntimePartDbTests`): a publish on an unchanged runtime writes no runtime row and its pull reads no runtime segment; a new runtime replaces the runtime part and its pull downloads and checks both parts; a runtime part that is not the release's is refused; a folder with a runtime file of another version gets no release manifest; the copier's temp folder holds its runtime and refuses another version; Build-Release's refusals (mixed, a different helper runtime, an older runtime); the layout, the one runtime version and the PE machines of the Release build.
- Source checks: `OpenPmsDental.exe` reports the release version (`0.1.1`) and `24.3.63.0`; the legacy chain is frozen; the guard code has no Debug bypass; Open Dental's update file types are only ever deleted; no downgrade prompt; UpdateFileCopier checks the manifest before copying.

Database tests use `127.0.0.1:3306` (root, blank password). They are inconclusive if no server answers, or if `RELEASEFLOW_TEST_DB=skip` is set.
