# Port note: TC-9a (the old engines' code retired)

Package TC-9a of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), the TC-9a row of
section 4; sections 3, 7 and 8; O-26 with the owner's note of 2026-11-15; O-32; launch check L-8): branch `chart/tc9a-1` from `chart/tc8-2`
(`9d9ee9cb5`, TC-8 reviewed and waiting for the owner's approval of its gallery; TC-9a merges right after it). **Status: done on the branch.**
Nothing was downloaded, no package or DLL was added, no schema changed, no program was started with a window, and no baseline picture changed.

The owner's answers in force: **O-26 as revised on 2026-11-15** ("we can get rid of Directx after our switch"): no DirectX runtime ships
after the switch, TC-9a follows TC-8 directly, and the office check is a launch check, not a gate (section 6). **O-32**: every legacy
recording is kept. **Decisions 21 and 22**: no bridge release, and no OpenPMS install to migrate, so the stored `computerpref.GraphicsSimple`
values 0, 1 and 2 stay as Open Dental reads them.

Commits: `009e6be25` (the DirectX numbers recorded while the engine was still in the build), `eb204569e` (the engines retired),
`c08054fda` (the tests), `d98bee7ef` (the window checks), `3418ad808` (two pins the first Targeted runs found), then this note.

## 1. What was deleted

The row's list, item by item:

| Row item | What went |
|---|---|
| `ToothChartDirectX.*`, `ToothChartOpenGL.*` | `SparksToothChart/ToothChartDirectX.cs`, `.Designer.cs`, `.resx`; `ToothChartOpenGL.cs`, `.Designer.cs`, `.resx` |
| `CodeBase/Controls/OpenGLWinFormsControl.cs`, when `xCodeBase.csproj` names no item for it | Deleted here: the SDK-style `xCodeBase.csproj` compiles it by its glob and names no item (only the stage 1 project baseline listed one) |
| The DirectX and OpenGL branches of the wrapper | `TcData`, `OnInvalidated`, `ResetControls`, `GetBitmap`, `OnResize` and `PerioMode` lose their DirectX and OpenGL cases; the fields of both engines (`toothChartDirectX`, `toothChartOpenGL`, `hardwareMode`, `preferredPixelFormatNum`, `deviceFormat`) |
| `D3DX_DLL`, `IsDirectXRuntimeAvailable`, `CanLoadLibrary` | Gone, with `LoadLibrary`, `GetModuleFileName` and `FreeLibrary` and N64-A's D3DX log line (item 2 below) |
| `UseHardware`, `PreferredPixelFormatNumber`, `DeviceFormat` | Gone, with `AutoFinish` (OpenGL's `glFinish` switch, the same kind of OpenGL-only property). The lines that set them went from their callers: the designer-style block in `ControlChart`'s constructor, `FormPerioGraphical.designer.cs`, `FormToothChartBig.designer.cs`, and `AutoFinish=true` in `ControlTreat.PrepImageForPrinting` and `SheetPrinting.GetToothChartHelper` |
| The two DirectX format helpers from TC-2 | `GetPreferredDirectXFormat` and `TestDirectXFormat` |
| Their relay forwards | `ToothChartRelay.ChooseStartupDirectXFormat` (a no-op since TC-8) and `FormOpenDental`'s call with its chart restart and its `DirectXFormat` start-up log phase (TC-8's note: "TC-9a removes it with the engines") |
| `SparksToothChart.csproj`: the SharpDX and Tao references | `SharpDX`, `SharpDX.Direct3D9`, `SharpDX.Mathematics`, `Tao.OpenGl`, `Tao.Platform.Windows`, and the two engines' `DependentUpon` items |
| The old engines' fallback | `GetFallbackDrawMode`, `StartFirstWorkingMode`, `TryStartDrawMode`, `DisposeDirectX`, the `LogFallbackOnce(string)` overload and `ToothChartFallbackKind.DrawMode`. `StartDrawingModeEngine` is now `StartSimple2D`: `DrawMode` starts the WPF chart for a stored 0 or 2 (TC-8) and the 2D chart for 1; `Engine` starts Simple2D or Wpf3D |
| `ToothChartEngine` | `DirectX` and `OpenGL` removed; `Simple2D` keeps 1 (the value of `DrawingMode.Simple2D`) and `Wpf3D` keeps 3. In memory only, never stored |
| `IsGraphicsDeviceException` | SharpDX's exception, which only the DirectX chart threw: the method went, and the Chart module's Save to Images lost its catch of it (`ImageStore.Import` errors still reach the general catch; the picture is made before the `try`) |
| Tests | Section 4 |

**Kept as the row writes:** `DrawingMode` keeps its three stored values (0 DirectX, 1 Simple 2D, 2 OpenGL), so a return to Open Dental reads
them as before. The dead `ComputerPref` columns (`GraphicsUseHardware`, `PreferredPixelFormatNum`, `GraphicsDoubleBuffering`, and
`DirectXFormat` except its "WpfSoftware" value) stay unread, as `GraphicsUseDirectX11` does. `UnitTests/UI Testing/FormToothChartTests.cs` needs
no edit: it stores `DrawingMode` values.

**Comments.** Comments that record what the DirectX chart did (the WPF chart copies its constants, its D3DX fonts and its matrix code) stay
as history; comments that said "until TC-9a" now say what is.

## 2. What stays for TC-9b

As the TC-9b row and O-26 write: the program's own SharpDX and Tao references (`OpenPmsDental.csproj`: SharpDX, Tao.OpenGl,
Tao.Platform.Windows; `xCodeBase.csproj`: the two Tao references; `UnitTests.csproj`), the five DLLs in `Required dlls`, `tools/Build-Release.ps1`
(which still ships the five, staged from `Required dlls`), `THIRD-PARTY-NOTICES.txt`, the licence window's SharpDX and Tao texts and resources,
`ListRetiredFiles` and `[InstallDelete]`, `docs/reports/third-party-binaries-audit.md`, and `ReleaseIdentityTests`. No code uses the program's
references any more.

One consequence TC-9b must undo: `SharpDX.Direct3D9.dll` and `SharpDX.Mathematics.dll` were needed only through `SparksToothChart`'s
references, so the built program folder no longer gets them while the release still ships them. `LibraryBindingTests.ReleaseListTests` lists
the two as shipped but unused, with why (`_dictShippedUnused`), and fails as soon as either is no longer shipped or is used again: TC-9b removes
the list with the files.

## 3. The recordings kept (O-32)

- **Every legacy picture stays**, byte for byte: `tests/ToothChartImageTests/Baselines/legacy/` (769 PNGs with their `.txt`, DirectX, OpenGL
  and Simple 2D: TC-0's 715 and the later scenes' pictures, the gallery's "before" column and the parity probes' target) and
  `Baselines/legacy-smoothed/` (10). Every WPF baseline, the Simple 2D baselines, the call logs and the host baselines are unchanged
  (`git diff chart/tc8-2 -- tests/ToothChartImageTests/Baselines tests/ToothChartImageTests/CallLogs tests/ToothChartHostTests/Baselines`
  adds only the two files below).
- **New: `Baselines/legacy-numbers/`**, the numbers today's DirectX chart gave, recorded in commit `009e6be25` while the engine and
  SharpDX.Mathematics were still in the build (Targeted run `tc9a-record-legacy-numbers.txt` on the build PC: ToothChartImageTests 255, 251
  passed, 4 skipped; NoHqTests 110):
  - `directx-transforms.txt`: the DirectX chain's screen point (SharpDX `Matrix` and `Vector3.Project`, the copies of the DirectX chart's
    matrix code) of all 24,254 sample vertices of the TC-1 transform test, in round-trip floats. The row's "the TC-1 transform test (now
    against recorded numbers)".
  - `directx-mouse.txt`: the DirectX chart's selected teeth after each input, its `SegmentDrawn` strings, its count of committed selections
    and its drawings at the four sizes of the mouse test (104 lines), recorded while the same run checked the shared mouse class against the
    live chart.
- **The recorders are kept as skipped tests** whose reason says the engine was retired in TC-9a (`[Ignore]`): `LegacyRecorder.RecordLegacyChart`
  (its recording code went: the visible host form, the DirectX format search without multisampling, the adapter and pixel-format facts),
  `WpfTransformTests.RecordDirectXTransforms` and `ToothChartMouseTests.RecordDirectXMouse`. What reads the recordings stays: the parity
  probes, the gallery (`WpfGallery`, its DirectX, smoothed, OpenGL and Simple 2D columns), the manifest and index writer, the sidecar reader,
  the smoothed-picture rule (`CanRecordSmoothed`, with its test) and `CompareLegacyRecording` with `LegacyComparison`.

## 4. Tests

| Project | Change |
|---|---|
| BINATests | Deleted, as the row names them: `ToothChartFallbackTests.cs` (the old engines' fallback, BIN-A and TC-8) and S3a-15's `ToothChartEngineBitnessTests.cs` (S3a-15 ran). New "retired" file `ToothChartEnginesRetiredTests.cs` (7 tests): the files, types, wrapper members and SharpDX/Tao references are gone (by reflection and in the source without its comments), the engines are Simple2D (1) and Wpf3D (3), `DrawingMode` keeps 0, 1 and 2, no `DrawMode` fallback kind, the relay has no start-up search and no `ToothChartSettingsUse`, the fallback shows no dialog; the two tests of what stays moved here unchanged (a stored DirectX or OpenGL choice starts the WPF chart and Simple 2D stays, O-30; the business layer's plain image) |
| N64ATests | `DirectXLogLineTests` (item 2 below) |
| ToothChartImageTests | The `SharpDX.Mathematics` reference removed. `WpfTransformTests`: the transform test compares every recorded point (all keys, no other) within 0.001 mm. `ToothChartMouseTests`: `DirectX_GivesTheSameSelectionsAndStrings_ForTheSameInput` compares the shared class with the recorded answers (the input itself is checked against the recorded input first). `Support/DirectXCharts.cs` deleted. `WpfOnScreenTimings`: no DirectX times (a note; TC-3's are in gallery G1). `WpfBackgroundPictureTests`: the DirectX case of "another engine gives no request" went (Simple 2D stays). `WpfLiveViewTests`: the "WpfSoftware" value holds no ';' (until TC-9a the test parsed it with the DirectX chart's device format). `SupportTests`: the recorder's adapter fact went with the recorder (the test is `Recorder_SmoothedList_NamesSizesOfItsScenes`). `WrapperTests`: `IsGraphicsDeviceException` is gone. `WpfRendererTests`/`SecondProcess`: the log-kinds test writes a line of another kind (Cursor) before the render failures, since the DrawMode kind is gone (`..._AreWrittenEvenAfterALineOfAnotherKind`). `CallerSourceTests` and `EngineChoiceTests`: the callers' pins (below), `StartSimple2D`, no start-up search, no device-exception catch |
| ToothChartHostTests | Gallery G4's DirectX column removed (`HostPictures.WriteDirectXPicture` and `HostDirectX`, which drew each host's data on a DirectX wrapper for the gallery only); `ApplyComputerSettings(wrapper)` |
| LibraryBindingTests | The two shipped-but-unused SharpDX files (section 2) |
| WindowChecksTests | Section 5 |

**Item 2, N64-A's D3DX log line.** The row's deletion of `IsDirectXRuntimeAvailable` and `CanLoadLibrary` removes the code that wrote the line
(the line was written inside the runtime check, from `CanLoadLibrary`'s loaded path), so the line went with nothing in its place, together
with `GetDirectXRuntimeLogLine` and `DescribeLoadedD3dx`. `N64ATests/DirectXLogLineTests` now pins that no D3DX is loaded: three charts that
start and draw (a stored DirectX and OpenGL choice, both the WPF chart, and Simple 2D) leave no `d3dx*` module in the process and no SharpDX or
Tao assembly in the test domain; `SparksToothChart.dll` (read with Mono.Cecil) imports no `d3dx`, `d3d9`, `opengl32` or `glu32` function and
references no SharpDX or Tao assembly; the wrapper's code names no D3DX and calls no `LoadLibrary`. It passed in the x86 runner and in the
lane's x64 test build.

**Item 3, `ToothChartRelay.ApplyComputerSettings`' unused `ToothChartSettingsUse` parameter and enum (TC-8's note).** Removed, with the six
callers (`ControlChart` twice, `ControlTreat`, `SheetPrinting`, `FormSheetDefEdit`, `FormToothChartBig`) and every source pin that names them
updated: `CallerSourceTests` (the four painter-order tests and the eight settings sites, where `ControlChart`'s two sites are now one call
twice), `EngineChoiceTests` (the helper's signature), `ToothChartHostTests` (`HostTests`, `GraphicsWindowTests`). All S3a packages that owned
those files had merged, and no parallel stream was writing.

## 5. The window checks (built, never run here)

`tests/WindowChecksTests` starts the program, so it is the lead's at the gate; it builds with 0 errors. Rewritten for TC-8's behaviour:

- **`WhyNotDirectX` is `WhyNotWpfChart`**: "" while the engine is Wpf3D, no fallback is logged and `GetBitmap` renders. The WPF chart's
  pictures render in software in every session, a locked one included, so the device reasoning (`TEXT_NO_DEVICE`, `IsNoDevice`) and the
  Simple 2D run (`IsSimple2DRun`, `TEXT_SIMPLE2D`, the `I.Simple2D` key) are gone: IPREP checks that the chart draws with the WPF chart and no
  longer moves the copy to Simple 2D. The on-screen test (`ShotChartWindow`, used by B5, I1, I6 and ISCREEN) counts the chart's own window while
  the WPF chart draws it; a blank print means the session is locked (WPF draws nothing on screen then).
- **B5**: the tooth chart runs the WPF chart (its `DrawMode` reads DirectX, the stored value it replaces), no fallback, and its WPF picture is
  always taken.
- **I6**: graphical perio on the WPF chart; then File > Graphics > "Simple 2D chart" > Save, and Graphical again: it opens on the WPF chart in
  perio mode with no message (O-31). `CheckPerioOnSimple2D` replaces `CheckPerioMessage2D` and its "In the Graphics setup window, you must first
  select DirectX."
- **I7**: File > Graphics > "3D chart" > Save: no message (no DirectX format is tested), the Chart draws Wpf3D, and the stored row follows the
  save rule (over the stored 1 of I6 it writes 0); then "Draw in software" on (`DirectXFormat` "WpfSoftware") and off again (the row as it was).
  `SetGraphics` picks `radio3DChart` or `radioSimpleChart`, and `SaveGraphics` is the shared Save. `SetDrawInSoftware` casts
  `checkDrawInSoftware` to `OpenPmsDental.UI.CheckBox`, its field's type (the review's fix in `chart/tc9a-3`: the first version cast it to
  the WinForms `CheckBox`, which would have thrown `InvalidCastException` at the gate). `ToothChartHostTests/GraphicsWindowCheckCastTests`
  pins every cast of a File > Graphics control in `InProcChecks.I.cs` and `InProcChecks.IJ.cs` against the field FormGraphics declares, with
  the short name resolved through the file's own using directives.
- **I8, I9**: no `OPMS_TOOTHCHART_WPF` (TC-8 removed the switch); I9's skip says so.
- **I11** (IJTests): no longer puts DirectX back; the start with no DirectX format saved searches and saves nothing (`DirectXFormat` stays
  empty) and keeps the stored choice.

`HarnessTests` keeps "I.Simple2D" as a sample key of its text round trip: it is only a string there.

## 6. Builds and runs (build PC, `.git/od-tools/test-results/`)

- **Builds** through the lane: single projects while iterating, then the full build of `OpenDental.Local.slnf` (Debug x86) with 0 errors
  (section 8 has the last one).
- `tc9a-record-legacy-numbers.txt`: the recording run (section 3).
- `tc9a-run1.txt` (ToothChartHostTests 15: 14 passed, 1 skipped; BINATests 19; BINBTests 99; BPERIOTests 55; N64ATests 34; LibraryBindingTests
  20 with 1 failure; ProjectFileTests 150, 1 skipped; S3aM1Tests 61; NoHqTests 110): the LibraryBindingTests failure is section 2's.
- `tc9a-run2.txt` (ToothChartImageTests 256: 1 failure, the log-kinds test), fixed in `3418ad808`.
- `tc9a-run3.txt`: ToothChartImageTests 256, 250 passed, 0 failed, 6 skipped (the three recorders, the comparison, the gallery writer and the
  opt-in on-screen times); NoHqTests 110.
- `tc9a-run4.txt` and `tc9a-run5.txt`, the projects whose source tests read a file TC-9a changed or deleted: LibraryBindingTests 20, BAPPTTests
  61, BSHELLTests 73, BTPTests 38, GroupNoteTests 54, M2CARRIERTests 55, M2SHELLTests 39, M2TERMTests 21, P262ClaimsTests 42, S0FTests 29,
  SshHostKeyTests 46, EhrTests 260, AttributionTests 50, BPRINTTests 71, M2SHEETSTests 53, SheetTests 31, MaintenanceTests 111,
  InventoryTests 93, ReleaseFlowTests 206 (5 skipped), DbConnectionSecurityTests 250 (3 skipped), NoHqTests 110: all passed.
- Section 8 has the runs after the docs and the full build.
- `chart/tc9a-3` (the review's fix): the full build with 0 errors, then `tc9a-3-targeted-2.txt`: ToothChartImageTests 256 (250 passed,
  6 skipped), ToothChartHostTests 16 (15 passed, the gallery skipped), BINATests 19, BINBTests 99, BPERIOTests 55, N64ATests 34, NoHqTests
  110, LibraryBindingTests 20, ProjectFileTests 150 (1 skipped), S3aM1Tests 61: all passed. (`tc9a-3-targeted.txt`, the run before it, failed
  the new pin's first version, which wrongly refused `InProcChecks.I.cs` for its `using OpenPmsDental.UI;`.)
- Not run: InitSqlTests and ForkMigrationTests (the coordinator's), and WindowChecksTests (section 5).

## 7. The x64 test build (for TC-10)

- **ToothChartImageTests**: the lane's x64 test build (`-Projects ToothChartImageTests -X64Test`) stops before any test at
  `tests/TestProbeHost` with NETSDK1047 ("Assets file ... doesn't have a target for 'net48/win-x64'"; `tc9a-x64-toothchartimage.txt`).
  TestProbeHost, the second-process host the image tests reference since S3a-8b, is an `Exe`, and an x64 `PlatformTarget` gives an exe the
  runtime identifier win-x64, which its restore (the x86 one, as the lane restores for an x64 test build) has no target for: the lane's own
  note says a program project in the references fails the same way. Nothing TC-9a changed is involved, and TC-9a does not edit the lane or
  TestProbeHost's project: TC-10, which moves the image tests to x64, has to give TestProbeHost an x64 restore (or the image tests another way
  to start the second process).
- **BINATests and N64ATests**, the two library-only projects whose tests pin the retirement, ran in the lane's x64 test build:
  BINATests 19 and N64ATests 34, all passed (`tc9a-x64-bina-n64a.txt`): no D3DX is loaded in a 64-bit process either.

## 8. After the docs

- **Full build** of `OpenDental.Local.slnf` (Debug x86) through the lane at `a01df7819`: 0 errors.
- **`tc9a-final1.txt`**: ToothChartHostTests 15 (14 passed, 1 skipped: the gallery's window picture), BINATests 19, BINBTests 99, BPERIOTests
  55, N64ATests 34, LibraryBindingTests 20, ProjectFileTests 150 (1 skipped: the project baseline check), S3aM1Tests 61, and the projects whose
  tests read the plan, the release notes or the port notes: ODOPTSTests 57, P262SchemaTests 28 (4 skipped), PdfLibraryTests 41, BSHELLTests
  73, InventoryTests 93; NoHqTests 110. All passed.
- **`tc9a-final2.txt`**: ToothChartImageTests 256, 250 passed, 0 failed, 6 skipped; NoHqTests 110.
- **`tc9a-final3.txt`**: VendorDirectTests 941, ImagingTests 81, ReleaseFlowTests 206 (5 skipped), DbConnectionSecurityTests 250 (3
  skipped), NoHqTests 110. All passed.
- **`tools/Test-NoEolFlips.ps1`** against `chart/tc8-2` (merge base `9d9ee9cb5`): 42 changed text files, none changed its line ends; no
  modified file gained or lost a BOM (the blobs' first bytes compared with `git cat-file`).

## 9. Launch check L-8, for a person (TC-9a's part)

TC-9b's part of L-8 (a Release dry run without the five DLLs; an install over the previous release removes them) follows TC-9b. TC-9a's part,
on a build of this branch, on a scratch database with synthetic patients only:

1. **A clean Windows VM without the DirectX End-User Runtime.** Use a Windows 10 or 11 VM on which `C:\Windows\SysWOW64\d3dx9_43.dll` (and
   `C:\Windows\System32\d3dx9_43.dll`) does **not** exist (a fresh install has neither; never delete it from a real computer). Install or copy
   the build and start the program.
   *Expected:* no error at start-up and no minutes-long wait (there is no DirectX search any more).
2. **The 3D chart.** Open a synthetic patient with planned and completed work in the Chart module. *Expected:* the 3D chart (smooth teeth),
   never the pie-shaped Simple 2D chart. Click a tooth, drag across several, use the Draw tab's pen and eraser. *Expected:* each responds.
   Look at `Logger\ToothChart` next to the program's data folder (if it exists). *Expected:* no line about DirectX, OpenGL or `d3dx9_43.dll`.
3. **Graphical perio.** Perio Chart > an exam of the synthetic patient > Graphical. *Expected:* the graphical perio chart opens on the 3D chart
   with no message; Print previews it; Save to Images saves it.
4. **Printing.** Treatment Plan > Print (classic and sheet), a sheet with a tooth chart, and the Patient Dashboard's chart. *Expected:* each
   shows the 3D chart's picture, never an empty box.
5. **File > Graphics.** *Expected:* "3D chart" and "Simple 2D chart" and "Draw in software", no DirectX or OpenGL choice; "Simple 2D chart" >
   Save shows the 2D chart, "3D chart" > Save the 3D chart again.
6. **Write down** for each step: as expected, or what differed, with a screenshot (synthetic data only), the VM's Windows version and its
   graphics adapter (a VM's basic display adapter is the hard case: the chart draws in software there).

The office check (O-26 as revised: a launch check before a release) stays as TC-8.md section 5 writes it.

## 10. Open points and notes for the lead

- **The stage project baseline** (`tools/project-baseline/items.txt`, `resources.txt`) still lists the deleted files' items and the two
  engines' resources; it is the lead's record, re-recorded after merges, so TC-9a leaves it (the opt-in `ProjectBaselineTests` check stays
  skipped in a Targeted run, as before).
- **`docs/port-notes/TC-2.md`** does not exist in the repository (TC-2's details are in the plan's TC-2 row and status lines); nothing here
  depends on it.
- **The x64 image tests** (section 7) are TC-10's.
- **TC-9b** removes `LibraryBindingTests`' shipped-but-unused list (section 2) with the five files.
