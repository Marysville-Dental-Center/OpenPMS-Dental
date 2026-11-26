# Port note: TC-9b (SharpDX, Tao and the DirectX runtime requirement leave the release)

Package TC-9b of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), the TC-9b row of
section 4, the file-ownership table of section 3, launch check L-8): branch `chart/tc9b-1` from `main` at `071c0b18b` (TC-8 and TC-9a
merged: no code uses SharpDX or Tao any more). **Status: done on the branch.** The owner's answer in force: O-26 as revised on 2026-11-15
("we can get rid of Directx after our switch"): no DirectX ships after the switch. Nothing was downloaded, no package or DLL was added, no
schema changed, no setup was run and no program was started with a window.

Commits: `2e1f0fcb4` (the release: DLLs, project files, licence texts, release lists), `04558c7da` (tests), `947f3747a` (release notes),
then the docs commit with this note.

## 1. What left the release

The row's list, item by item (the row's line numbers were from when it was written; each item was found by its content):

| Row item | What went |
|---|---|
| `Required dlls` | `SharpDX.dll`, `SharpDX.Direct3D9.dll`, `SharpDX.Mathematics.dll`, `Tao.OpenGl.dll`, `Tao.Platform.Windows.dll` (deleted) |
| `OpenPmsDental.csproj` | The `SharpDX`, `Tao.OpenGl` and `Tao.Platform.Windows` references and the `Resources\SharpDX.txt` `Resource` item. The SDK-style project names no item for `Resources\Tao_OpenGL.txt` (the classic project's `None` item is the SDK's default glob now), so the file's deletion was enough |
| `xCodeBase.csproj` | The `Tao.OpenGl` and `Tao.Platform.Windows` references (no item for `OpenGLWinFormsControl.cs` was left after S1-1; TC-9a deleted the file) |
| `UnitTests.csproj` | The `SharpDX`, `SharpDX.Direct3D9`, `SharpDX.Mathematics` and `Tao.Platform.Windows` references (no UnitTests code used them) |
| The licence window | `FormLicense.GetVendorTexts`' `SharpDX` and `Tao Framework` lines; the `SharpDX` and `Tao_OpenGL` entries of `Properties/Resources.resx` and their properties in `Properties/Resources.Designer.cs`, edited by hand (no regeneration: every other resource is byte for byte as it was); `Resources/SharpDX.txt` and `Resources/Tao_OpenGL.txt` (deleted) |
| `tools/Build-Release.ps1` | The five names left `ThirdPartyFiles` and joined the deny-list, in its block of retired files (`ForkReleases.ListRetiredFiles`), each with its reason; the comment on the `'Required dlls'` source says what is left (the Direct Project) |
| `THIRD-PARTY-NOTICES.txt` | The `SharpDX` and `Tao Framework` components |
| `ListRetiredFiles`, `[InstallDelete]` | The five files, with a comment each. The setup changes only `[InstallDelete]`: it has never installed the DirectX runtime (no DirectX, d3dx, SharpDX or Tao anywhere in `installer/` before this package) |
| `docs/reports/third-party-binaries-audit.md` | The five rows say removed (no reference, what went, the setup's delete), the NuGet map's SharpDX row and the Tao sentence say so, and a paragraph in the removals since the baseline; Appendix A keeps the baseline's version info |
| `docs/release-notes/next.md` | A section "The tooth chart: SharpDX, Tao and the DirectX runtime leave the program folder" (the DirectX End-User Runtime is no longer needed) and the summary line pointing at it |
| `README.md` | The repository map's `Required dlls/` line no longer names SharpDX and Tao (not in the row; a fact that changed) |

## 2. What stays, and why

- **The SharpDX MIT notice inside Helix Toolkit's entry** of `THIRD-PARTY-NOTICES.txt` (TC-4): `HelixToolkit.Maths.dll` holds MIT code
  derived from SharpDX and SlimMath, so its notice ("Copyright (c) 2010-2014 SharpDX - Alexandre Mutel") ships with Helix, not with a
  SharpDX component. The licence window shows it inside the Helix entry. Re-keying the removed `SharpDX` vendor text to the Helix entry was
  not an option: a vendor text replaces that entry's own licence text (`FormLicense.GetLicenseText`), and every vendor-text key must be a
  listed component (`ReleaseIdentityTests`). `ReleaseFlowTests/SharpDXTaoRetiredTests` pins that SharpDX is named in Helix's entry only.
- **The never-ship patterns for DirectX** in `Build-Release.ps1` (`Microsoft.DirectX*.dll` and `d3dx*.dll`, "redistributable only through
  Microsoft's installer") stay: a release must never carry them, whatever the chart needs.
- **The removed-entry and removed-resource lists of `ReleaseIdentityTests`** are not edited (the row): its shipped components and files lose
  SharpDX, Tao Framework and the five files, and the new tests below check what went.
- **`DrawingMode`'s stored values** (0 DirectX, 1 Simple 2D, 2 OpenGL) and the legacy recordings stay as TC-9a left them; `CodeBase`'s
  assembly description still says "such as Tao" (Open Dental's text, an example only).
- **`tools/project-baseline/`** (`items.txt`, `files.txt`) still lists the program's `Resources\SharpDX.txt` and `Resources\Tao_OpenGL.txt`
  items and the five DLLs in the stage 1 output folders: it is the lead's record, re-recorded after merges, as TC-9a left it (the opt-in
  `ProjectBaselineTests` check stays skipped in a Targeted run).
- **`tests/ToothChartImageTests/CallerSourceTests`** (TC-8b's file while TC-8b runs) still exempts `FormLicense.cs` and
  `Resources.Designer.cs` from its "no SharpDX or Tao" scan, with a comment that says "until TC-9b". Both files now pass the scan without the
  exemption; the exemption and its comment can go when the file is free (a note for the lead, not edited here).

## 3. Tests

| Project | Change |
|---|---|
| AttributionTests | `ReleaseIdentityTests`: `_arrayShippedComponents` loses "SharpDX" and "Tao Framework", `_arrayShippedFiles` the five files |
| LibraryBindingTests | `ReleaseListTests._dictShippedUnused` is empty (its two SharpDX files, which TC-9a left shipped but unused, are gone; the mechanism stays); `RequiredDllsTests`: the list of what `Required dlls` holds and the list of the shipped files from `Required dlls` lose the five files (they pinned the names too, which the plan's grep missed) |
| BINBTests | New `SharpDXTaoRemovedTests.cs` (5 tests): the five files are not in `Required dlls`; no project names SharpDX or Tao (reference, package, `HintPath`, `Content` or `None`); no source has a `using` of them; CodeBase and, when built, the program, the chart library and UnitTests reference neither, and no build output folder holds one of the five; the licence window's texts, resources, designer properties and project item are gone |
| ReleaseFlowTests | New `SharpDXTaoRetiredTests.cs` (6 tests): `-ListFiles` stages none of the five; each is in `ListRetiredFiles`, on the deny-list with a reason, off the allow-list and in `[InstallDelete]`; the DirectX patterns stay; outside `[InstallDelete]` the setup names no DirectX, D3DX, SharpDX or Tao; the notices list neither component and SharpDX only inside Helix's entry; the release notes' section names the runtime and the five files. The existing `CheckFolder_RefusesARetiredFile` and `Iss_DeletesTheRetiredFilesOnAnUpgrade` cover the five through `ListRetiredFiles` |
| BINATests | A comment in `ToothChartEnginesRetiredTests` that said the program's references stay until TC-9b |

## 4. The release dry run

`tools/Build-Release.ps1 -BuildLane <the lane> -LaneWaitMinutes 60 -OutDir <a new scratch folder outside the repository> -SkipInstaller` at
`947f3747a` (the release, tests and release-notes commits; the setup was not built or run):

- **Exit 0.** The Release x86 build of `OpenDental.Local.slnf` through the lane: 0 errors. Every check of the script passed on
  `OpenPmsDental\bin\Release` and on the staging folder: nothing outside the lists, nothing on the deny-list, every allowed file there, every
  third-party file named in `THIRD-PARTY-NOTICES.txt`, the package files equal to the lock file's packages, the built-here files equal to
  their projects' output.
- **86 files staged**, none of the five: no `SharpDX*`, `Tao.*`, `d3dx*` or `Microsoft.DirectX*` file in the staging folder, nor in
  `OpenPmsDental\bin\Release`. `-ListFiles` gives the same 86; against `main` the allow-list differs by exactly the five `'Required dlls'`
  lines (`git diff main -- tools/Build-Release.ps1`).
- Build id `7ade484f220acd4cbbc216dfc640cb584e40420a93ed7e63a136b69964fe7cc6`; the source archive was written (its SHA-256 in the dry
  run's `SHA256SUMS.txt`). The script's one warning was the uncommitted docs of this note, which the archive does not hold.

## 5. Builds and runs (build PC, `.git/od-tools/test-results/`)

- **Builds** through the lane: the full build of `OpenDental.Local.slnf` (Debug x86) after the release commit, the five test projects
  after the tests commit, and the full build at `ba553f427` (after the docs): 0 errors each.
- `tc9b-run1.txt` (after the tests): AttributionTests 50, BINBTests 104, BINATests 19, N64ATests 41, LibraryBindingTests 20,
  LibraryUpdateTests 138, PdfLibraryTests 41, ProjectFileTests 150 (1 skipped: the opt-in project baseline check), ReleaseFlowTests 216 (5
  skipped), NoHqTests 112. All passed.
- `tc9b-run2.txt`, the projects whose source-text tests read a file TC-9b changed (`Required dlls`, `THIRD-PARTY-NOTICES.txt`,
  `Build-Release.ps1`, `Resources.resx`, `OpenPmsDental.iss`, `xCodeBase.csproj`, the release notes): DicomReaderTests 103,
  ImagingDeviceManagerTests 535, LibSpellTests 23, EhrTests 260, LibWebTests 65, ODOPTSTests 57, ReportTests 46, DbConnectionSecurityTests
  250 (3 skipped), SshHostKeyTests 46, VendorDirectTests 941, NoHqTests 112. All passed.
- `tc9b-final1.txt` (after the full build at `ba553f427`, with the dry run's Release build in place): the package's ten projects as in
  run 1, ReleaseFlowTests 216 with 2 skipped (the apphost record test runs once a Release build exists). All passed.
- `tc9b-final2.txt`: ToothChartImageTests 256 (250 passed, 6 skipped, as after TC-9a; `CallerSourceTests` reads `FormLicense.cs` and
  `Resources.Designer.cs`), ReportTests 46 (reads the binaries audit), ODOPTSTests 57 (reads the release notes), NoHqTests 112. All passed.
- **`tools/Test-NoEolFlips.ps1`** against `main` (merge base `071c0b18b`): 18 changed text files, none changed its line ends. No modified
  file gained or lost a BOM (`.csproj`, `.resx` and `Resources.Designer.cs` keep theirs; the blobs' first bytes compared with
  `git cat-file`). The two new test files and this note are new files (LF, as git stores a new file with `core.autocrlf` true).
- Not run: InitSqlTests and ForkMigrationTests (the coordinator's; no schema changed), WindowChecksTests (it starts the program), and
  ToothChartHostTests (TC-8b's; TC-9b changes no host).

## 6. Launch check L-8, for a person (TC-9b's part)

TC-9a's part (a clean VM without the DirectX End-User Runtime: the 3D chart, graphical perio and printing) is `TC-9a.md` section 9; run it on a
build with TC-9b, so the VM also has no SharpDX or Tao. TC-9b's part, with synthetic data only:

1. **A Release dry run without the five DLLs** (done by the agent, section 4; repeat it for the release itself): `tools\Build-Release.ps1`
   with `-OutDir` a new folder outside the repository. *Expected:* exit 0, and the staging folder holds no `SharpDX*.dll` and no `Tao.*.dll`
   (`dir <OutDir>\staging\SharpDX* <OutDir>\staging\Tao.*` finds nothing).
2. **An install over the previous release removes the five DLLs.** On a test computer or VM (never an office workstation) with the previous
   release installed, check that `C:\Program Files (x86)\Open PMS - Dental` holds `SharpDX.dll`, `SharpDX.Direct3D9.dll`,
   `SharpDX.Mathematics.dll`, `Tao.OpenGl.dll` and `Tao.Platform.Windows.dll`. Run this release's setup over it as an administrator.
   *Expected:* the setup finishes, and none of the five files is in the program folder afterwards; the program starts and the Chart module
   shows the 3D chart. (Decision 22: nothing is in production, so this is only a check of `[InstallDelete]`, not an upgrade path an office
   needs; the elevated `tools\Test-Installer.ps1` run of `docs/develop/release-process.md` covers a fresh install.)
3. **Help > About > View Licenses.** *Expected:* no "SharpDX" and no "Tao Framework" entry; the "Helix Toolkit" entry still shows
   "Copyright (c) 2010-2014 SharpDX - Alexandre Mutel".
4. **Write down** for each step: as expected, or what differed, with a screenshot, the Windows version, and the release the install went
   over.
