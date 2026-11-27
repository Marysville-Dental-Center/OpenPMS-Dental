# Port note: TC-8 (the engine choice moves to the WPF chart; every host)

Package TC-8 of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), the TC-8 row of
section 4; sections 2, 3, 5 (5.4), 6, 7, 8 and 11; section 10 with the owner's answers): branch `chart/tc8-1` from `main` (`e2fc2306f`, with
TC-0 to TC-7, TC-3b, TC-3c and TC-6 merged). **Status: done on the branch; gallery G4's final part (section 4) waits for the owner, and TC-8
merges only after the owner approves it.** Nothing was downloaded, no program was started with a window on the desktop, and the only program
files edited are `FormGraphics.*` and two comments in `FormPerio.cs` and `FormPerioGraphical.cs` (the file-ownership table gives all three to
TC-2 and TC-8); the tests ran in the lead's desktop session, drawing forms off the desktop as the chart's image tests do.

Commits: `ec1c65fac` (the chart and the settings), `587f11a4b` (File > Graphics), `3f282fc2d` (tests and pins), `01ab2fd5b` (the
`ToothChartHostTests` wiring), `b88c7b033` (the host tests and their baselines), `dde274036` (the gallery's window picture), then the docs.

## 1. What the row asks, and how it is built

### 1.1 The owner's answers in force

- **O-19 and O-34, answered (b1)** (2026-11-09, with G1): the program keeps drawing every WPF window in software (`ProgramEntry.cs`), the
  quick live view is the default on-screen chart, and a per-computer "Draw in software" switch shows the renderer's smoothed picture.
- **O-30**: a computer stored as Simple 2D stays 2D, with no data change.
- **O-31**: the graphical perio chart opens on the WPF chart on every computer.
- **O-26 as revised by the owner on 2026-11-15** ("we can get rid of Directx after our switch"): no DirectX runtime is shipped after the
  switch, TC-9a follows TC-8 directly and the office check is a launch check. Nothing in TC-8 keeps the old engines alive for an office check:
  they stay in the build only until TC-9a deletes them, reached by no program path.
- **Decision 21** (net10-plan section 11): there is no bridge release; wherever the chart plan says "bridge release" or "the 3a gate ships",
  it is read as the 3a gate. **Decision 22**: there is no OpenPMS install to migrate; an office moves from Open Dental once,
  so the save rule's rollback is a return to Open Dental itself, which reads the stored 0, 1 and 2 as before. The save rule is kept as the
  row writes it.
- **The parity rows ENG-1, ENG-2, ENG-3, ENG-5 and NEW-1** (section 10, "TC-8 ... the owner confirms") were not confirmed by the owner, so
  TC-8 follows the answers above, as the brief says, and builds none of them (section 6). Open Dental's own Graphics window (a global "3D
  tooth chart on all computers" setting, "Use the global setting / Use if available / Do not use" per computer, a status line, an "Older
  tooth chart technology" group, the smoothed picture as the default) stays an open question for the owner.

### 1.2 The relay (`WpfControlsOD/UI/ToothChart/ToothChartRelay.cs`)

Each item of the row's "Relay helpers":

- **Stored 0 and 2 start `Wpf3D`, 1 stays Simple 2D (O-30).** `GetEngineForStoredMode(DrawingMode)`: Simple2D for 1, Wpf3D for everything else
  (0, 2, and any other stored number, as the wrapper's `DrawMode` does). `ApplyComputerSettings(wrapper,use)` is now
  `ApplyComputerPref(wrapper,ComputerPrefs.LocalComputer)`, which sets the "Draw in software" switch first and the engine second. The three
  `ToothChartSettingsUse` values stay for the callers (S3a-owned files that TC-8 does not edit) and now do the same.
- **The software switch (O-19 live view chosen).** `IsDrawnInSoftware(computerPref)`: `computerpref.DirectXFormat` equal to
  `ToothChartWrapper.DirectXFormatWpfSoftware` ("WpfSoftware"); empty, or a DirectX device format an older build saved, is the live view.
- **From TC-3's fix round 3: `WpfDrawInSoftware` before `Engine=Wpf3D`.** In `ApplyComputerPref` and `ApplyGraphicalPerioMode`, so a computer
  drawn in software never makes a live view first (a test with a shown wrapper counts the views made).
- **The start-up DirectX search becomes a no-op.** `ChooseStartupDirectXFormat` returns false and searches and saves nothing (a computer saved
  as DirectX with no format used to wait minutes for the search). `FormOpenDental` keeps its call; TC-9a removes it with the engines.
- **The snapshot drops the DirectX and OpenGL fields.** `ToothChartSettingsSnapshot` holds the engine of the stored mode and the software
  switch instead of `GraphicsUseHardware`, `PreferredPixelFormatNum`, `GraphicsSimple` and `DirectXFormat`: a change between 0 and 2 starts
  the same chart and needs no Chart module redraw; a change to or from 1, or of the switch, does.
- **The graphical perio gate and mode per O-31.** `CanOpenGraphicalPerio` returns true with no message on every computer (no DirectX choice
  and no `d3dx9_43.dll` check); `ApplyGraphicalPerioMode` sets the Recession option (TC-7), the computer's software switch, then `Wpf3D`.
- **The developer switch goes.** `IsWpfDeveloperSwitchOn` and `OPMS_TOOTHCHART_WPF` are gone from every program, chart and business file.
- **No `ComputerPrefs.Update` when an image is made.** `ApplyComputerSettings` saves nothing (before TC-8 every sheet, printout and dashboard
  picture saved the OpenGL pixel format the chart chose).
- **Setup > Graphics' helpers:** `Is3DChart`, `SetGraphicsChoice` (the save rule, section 1.4), `ApplyGraphicsPreview`,
  `PaintGraphicsPreview` (the preview's synthetic sample in the office's chart colours) and `GetGraphicsPreviewStatus`.

### 1.3 `OpenPmsDentalBusiness/TableTypes/ComputerPref.cs`: comments only

`GraphicsSimple` (0 and 2 start the WPF chart, 1 the 2D chart, the values stay 0, 1 and 2, and the save rule), `DirectXFormat` (the "Draw in
software" value), `GraphicsUseHardware`, `GraphicsDoubleBuffering` and `PreferredPixelFormatNum` (not read since TC-8, the columns stay) and
`GraphicsUseDirectX11` (unread since Sparks3D went; BIN-A 5 keeps it open). No field, type or attribute changed.

### 1.4 File > Graphics (`FormGraphics.cs`, `.designer.cs`, `.resx`): rebuilt

The menu item is File > Graphics (`FormOpenDentalMenus`), and Setup > Computers opens the same window for another computer.

- **Two choices:** "3D chart" (`radio3DChart`) and "Simple 2D chart" (`radioSimpleChart`, the name the window checks already use). Load shows
  "3D chart" for a stored 0 or 2 and "Simple 2D chart" for 1. On Linux the 3D choice is off and the window says so, as before.
- **The save rule** (`ToothChartRelay.SetGraphicsChoice`, then `ComputerPrefs.Update(computerPref,old)` as before): "Simple 2D chart" writes
  1; "3D chart" writes 0 only when the stored value was 1, so a stored 0 or 2 is kept and a return to Open Dental brings back the engine the
  computer had there; "Draw in software" on writes "WpfSoftware" to `DirectXFormat`, off clears only that value (a DirectX device format an
  older build saved stays for the same return). No other column is written.
- **The medical wording is kept:** in a medical practice or clinic (`Clinics.IsMedicalPracticeOrClinic`) Load words the window as Open Dental
  does there, "Graphics" for the group, "Use 3D Graphics (recommended)" and "Use Simple Graphics" for the two choices (main's Load did the same
  for the old radio buttons; the engine names are left out of the words, as the window names no engine). A source test pins it.
- **"Draw in software"** (`checkDrawInSoftware`, with a line saying what it does): enabled only for "3D chart".
- **A live preview** (`toothChartWrapper` in a "Preview" group, 410x307) with made-up work in the office's chart colours (a completed MOD on 3,
  a completed crown on 14, a planned root canal and build-up on 19, a planned extraction on 1, a missing 30; no patient), redrawn at once for
  each choice (`ApplyGraphicsPreview`): the live view, the smoothed picture with the check box, or the 2D chart. A status line names the
  chart that started ("3D chart, live view.", "3D chart, drawn in software: ...", "Simple 2D chart." or "The 3D chart could not start on
  this computer, so the Simple 2D chart is shown ..."). For another computer the preview is drawn by this computer, and the window says so.
- **No DirectX format grid and no OpenGL pixel grid**, no hardware, double-buffering or "all formats" filters. The remote edit from Setup >
  Computers saves without testing, as before (and the local save tests nothing either: the 3D chart falls back to 2D on its own, logged once).
- The window names no engine type: no `ToothChartEngine`, `DrawingMode`, DirectX, OpenGL, SharpDX or Tao (a source test).

### 1.5 The wrapper (`SparksToothChart/ToothChartWrapper.cs`)

- **A DirectX or OpenGL request starts `Wpf3D`, which falls back to Simple 2D with one log line.** `DrawMode`, the stored kind, starts
  `Engine=Wpf3D` for DirectX and OpenGL and the 2D chart for Simple2D (`StartDrawingModeEngine`). A WPF start failure keeps TC-1's path: one
  `Wpf3DStart` line, Simple 2D (a second-process test through the relay and through `DrawMode=OpenGL`: two wrappers fall back, one line, no
  DirectX or OpenGL line). `DrawMode` still reads DirectX while the engine is `Wpf3D`, so `Wpf3D` has no stored form.
- **`Engine`** still starts DirectX and OpenGL themselves, for the legacy recorder and the tests only (TC-0's recording, the parity probes'
  DirectX side, the bitness checks) until TC-9a deletes them; `TestDirectXFormat` and `GetPreferredDirectXFormat` use it, and the program
  calls neither any more (a source test: nothing in the program or the relay selects DirectX or OpenGL).
- **The UI-thread render failure falls back as a failed `ShowScene` does** (the row's fix-round-3 note). While a wrapper has a live view it
  watches its thread's WPF `Dispatcher.UnhandledException` (the UI thread has no filter of its own, unlike the render thread,
  `ToothChartRenderer`): a failure of WPF's render thread that WPF throws on the UI thread (`IsWpfRenderThreadFailure`: the HRESULT
  `UCEERR_RENDERTHREADFAILURE` 0x88980406 anywhere in the exception, or an exception thrown by `MediaContext.NotifyPartitionIsZombie`) is
  handled, the live view is released, the image host shows the chart from then on (cleared first) and one `Wpf3DLiveView` line is logged, and
  the on-screen rebuild is posted so the image host shows the current data. Every live view of that thread falls back (another wrapper's
  `Handled` does not stop it); any other exception is left unhandled for the program's own handlers. The watch ends when the live view goes.
  The row's last sentence (software as the default under render tier below 2 or in a terminal-server session) applies only if O-34 ever
  allows hardware; the program draws every WPF window in software, so nothing is built for it.
- The perio mode's error, which `FormPerioGraphical` shows when a 2D wrapper is asked for perio (only when the WPF chart could not start),
  names the 3D chart instead of "Only allowed in DirectX".

### 1.6 Files

`SparksToothChart/ToothChartWrapper.cs`, `ToothChartEngine.cs` (a comment); `WpfControlsOD/UI/ToothChart/ToothChartRelay.cs`;
`OpenPmsDentalBusiness/TableTypes/ComputerPref.cs` (comments); `OpenPmsDental/Forms/FormGraphics.cs`, `.designer.cs`, `.resx`;
`OpenPmsDental/Forms/FormPerio.cs` and `FormPerioGraphical.cs` (one comment each); `OpenDental.sln` and `OpenDental.Local.slnf` (the new test
project, added here the way S3aM1Tests and NativeFirstTests were, in its own wiring commit); tests (section 3); the docs.

## 2. Each host, checked end to end

| Host (the row's list) | How it is checked | Picture |
|---|---|---|
| Chart module: refill, selection, the Draw tab, Save to Images, the dashboard event picture | Launch check L-7 (section 5): the module needs the program's main window. The settings path it uses (`ApplyComputerSettings` StoredMode and StartupMode) is in the host and image tests; its painting and dashboard request are TC-2's and TC-3b's tests | L-7 captures |
| Big chart window (926x858) | `HostTests.BigChart_IsTheWpfChartsPicture`: `FormToothChartingBig`'s load and fill (made, not shown), engine `Wpf3D` | `big-chart` |
| Treatment Plan printout, sheet (500x370, colours 14 and 15) | `TreatPlanSheet_ToothChartImg_IsTheWpfChartsPicture`: `GetToothChartHelper` with the plan and Show Completed | `treatplan-sheet` |
| Treatment Plan printout, classic document | Launch check L-7: `ControlTreat.PrepImageForPrinting` needs the module's grids; its code is the TC-2 painter's (CallerSourceTests) and its settings call is the one of the sheet | L-7 |
| Referral letter and automation sheet | `ReferralLetterAndAutomationSheet_ToothChartImg_AreTheWpfChartsPicture`: `GetToothChartHelper` with the override list, as `FormOpenDental` and `AutomationL` call it | `referral-letter` |
| Sheets' toothChart field | `Sheet_ToothChartField_IsTheWpfChartsPicture`; also a computer stored as OpenGL (the same picture), drawn in software (the same picture) and as Simple 2D (O-30: the 2D picture, 32-bit as TC-0 recorded the 2D chart) | `sheet`, `sheet-simple2d` |
| Patient Dashboard on an MTA `ODThread` worker | `Dashboard_OnAnMtaWorker_IsTheWpfChartsPicture_AndNothingIsLogged`: `DashToothChart.RefreshData` on an MTA thread; then a wrapper with the relay's settings on the same kind of thread: `Engine` is still `Wpf3D` and nothing was logged (an exception there is swallowed in the program, so the engine is checked, not only the absence of an exception) | `dashboard` |
| Sheet editor preview | `SheetEditorPreview_IsTheWpfChartsPicture`: `FormSheetDefEdit.SetToothChartImage` | `sheet-editor-preview` |
| Graphical perio chart: display, print and save | `GraphicalPerio_DisplayPrintAndSave_AreTheWpfChartsPictures`: `FormPerioGraphical.LayoutTeeth` (made, not shown), engine `Wpf3D` in perio mode, the chart's picture (649x696, shown and printed) and the 750x1000 save page (`RenderPerioPrintout`, as Print and Save to Images draw it) | `perio-chart`, `perio-save` |
| Graphical perio chart on a 2D computer (O-31) | `GraphicalPerio_OnA2DComputer_OpensOnTheWpfChart_O31`: the gate says yes with no message, and the same pictures | `perio-chart`, `perio-save` |
| File > Graphics | `GraphicsWindowTests`: Load shows the stored choice and the preview starts it (six stored cases); Save writes the save rule to this computer's row (eight cases), and what was saved starts the chart the window showed | gallery only |

Each picture is checked for its size, opacity (every alpha 255), the pixel format TC-0 recorded for the engine (Format24bppRgb for DirectX
and the WPF chart; Format32bppArgb for the 2D chart), a picture that is not blank (at least 50 colours), no fallback message, and its
baseline with the image tests' tolerance (`ChartImageCompare`). The host tests run on a scratch database named `tc8_<time>_<process>_<random>`
on the local MariaDB, loaded from `db/init/new-database.sql`, with synthetic patients only ("Synthetic Work", "Synthetic Empty"), and dropped
afterwards.

## 3. Tests and baselines

### 3.1 What was added or changed

- **New `tests/ToothChartHostTests`** (15 tests; section 2): SDK-style on `tests/Directory.Build.props`, MSTest 4, x86 until S3b-5. The row
  asks for one project reference, to the program, "the libraries come through it": the test projects turn the SDK's transitive project
  references off (`tests/Directory.Build.props`), so the libraries the tests compile against are named too, as S3aM1Tests names them; the
  program is the one that carries them. It links the image tests' `ChartImageCompare.cs`, `PngWriter.cs` and `StaRunner.cs`. Its lock file
  was written by the lane's locked restore. Also `SettingsSnapshot_SeesAnEngineOrSoftwareChange_NotAChangeBetweenZeroAndTwo`.
- **New `ToothChartImageTests/EngineChoiceTests.cs`** (12 tests): the mapping (0 and 2 give `Wpf3D`, 1 Simple 2D; `DrawingMode` keeps 0, 1
  and 2; `Wpf3D` has no stored form and reads as DirectX); the software value reads and writes; `ApplyComputerPref` for every stored mode
  and format, reading and never changing the settings, with the switch before the engine (source and a shown wrapper that makes no live view);
  `DrawMode` requests; the save rule (3D over each stored value, the software value written or cleared, no other field written, and saved
  then applied); the MTA worker; the stored start failure (second process); the render-thread failure on the UI thread (recognised by its
  HRESULT or its thrower, including WPF's own `NotifyPartitionIsZombie` called by reflection; a shown live view released and the image host
  showing the data, the exception handled, another exception left alone).
- **`CallerSourceTests`**: the eight settings sites (Setup > Graphics' preview call in place of its DirectX format test); nothing in the
  program or the relay selects DirectX or OpenGL (the wrappers' designer defaults allowed, in `ControlChart.cs` too); the stored-mode checks
  in the relay; no exception for `FormGraphics` any more; FormGraphics names no engine type and has two choices and the check box; the perio
  gate has no DirectX check; the developer switch is gone (replacing TC-2's DEBUG-only check).
- **Pins TC-8's edits broke:** `PerioSceneTests` (the perio mode's message; `ApplyGraphicalPerioMode`'s order), `WrapperTests` (the message),
  `WpfBackgroundPictureTests` (a message), BINATests `ToothChartFallbackTests` (rebuilt: the old engines' fallback stays for `Engine`; the
  format test starts the DirectX engine itself; the stored request starts `Wpf3D`; the relay moves no computer to OpenGL; a new runtime test
  of `DrawMode` on the test's own thread) and `ToothChartEngineBitnessTests` (the old engines through `Engine`). The legacy recorder and
  `DirectXCharts` start DirectX through `Engine`.
- `ProprietaryBinariesRemovedTests` (its DirectX 11 checks read the rebuilt window and the relay) and `PerioUiSourceTests` needed no change:
  no pin there is broken by TC-8.

### 3.2 The baselines

- **No existing baseline was re-recorded.** Every WPF baseline, the Simple 2D baselines, the legacy recordings (DirectX, OpenGL, Simple 2D,
  smoothed) and the call logs keep their bytes (`git diff main...chart/tc8-1 -- tests/ToothChartImageTests/Baselines
  tests/ToothChartImageTests/CallLogs` is empty): TC-8 changes no drawing.
- **9 new host baselines** in `tests/ToothChartHostTests/Baselines/` (PNG with a `.txt` of its size, format, colours and pixel SHA-256),
  recorded with `OPMS_RECORD_HOST_BASELINES=1` at `b88c7b033`: `sheet`, `treatplan-sheet`, `referral-letter`, `dashboard`,
  `sheet-editor-preview`, `big-chart`, `perio-chart`, `perio-save` and `sheet-simple2d`. Each was looked at; the sheet pictures do not draw
  the synthetic patient's missing 1 and 16, by either engine (the gallery's DirectX pictures show the same): today's sheet painting, not
  TC-8's.

### 3.3 Builds and Targeted runs (build PC, `.git/od-tools/test-results/`)

- **Builds** through the lane: single projects while iterating, then the full build of `OpenDental.Local.slnf` at `dde274036`: 0 errors.
- `tc8-run1.txt` (the first run of the changed image and BINA tests): two pins found (`NothingSelectsDirectXOrOpenGL` met `ControlChart.cs`'s
  own designer defaults; `WrapperTests` read the old message), fixed.
- `tc8-host-record1.txt` and `tc8-host-record2.txt`: the host baselines recorded (the first found the 2D picture's own format).
- `tc8-host-gallery-after*.txt` and `tc8-host-gallery-before.txt`: the gallery's pictures (section 4); the "before" run drew the window of
  `main` (put back for that run only, then restored), so its two window tests failed there as expected.
- **`tc8-targeted-1.txt`** (after the full build, at `dde274036`): all 25 assemblies passed, 0 failed. **ToothChartImageTests 254: 250
  passed, 0 failed, 4 skipped** (the legacy recorder and its comparison, the gallery writer and the opt-in on-screen times); **ToothChartHostTests 15: 14
  passed, 1 skipped** (the gallery's window picture, gallery only); BINATests 34, BPERIOTests 55, M2SHEETSTests 53, SheetTests 31, BTPTests 38,
  BPRINTTests 71, ChartLetterTests 45, NoHqTests 110, LibraryBindingTests 20, S3aM1Tests 61 and ProjectFileTests 150 (149 passed, 1 skipped:
  the project baseline check, which needs its record) passed. Also the projects whose source tests read a changed file or the solution:
  N64ATests 34, P262LinksTests 61, P262PerioTests 36, M2MISCTests 97, ReleaseFlowTests 206 (5 skipped), LibSpellTests 23, LibWebTests 65,
  LibraryUpdateTests 138, MiddleTierMockTests 151, PdfLibraryTests 41, ReportTests 46 and VendorDirectTests 941, all passed.
- **`tc8-targeted-docs.txt`** (after the docs commit; the projects whose tests read `docs/decisions/owner-questions.md`, `docs/release-notes/next.md`
  or the plan): BSHELLTests 73, DbConnectionSecurityTests 250 (3 skipped), InventoryTests 93, LibraryBindingTests 20, ODOPTSTests 57,
  P262SchemaTests 28 (4 skipped), PdfLibraryTests 41, ReleaseFlowTests 206 (5 skipped) and NoHqTests 110: all passed.
- **`tc8-2-targeted.txt`** (review fix round, the medical wording kept, after a full build with 0 errors): ToothChartImageTests 254 (250
  passed, 4 skipped), ToothChartHostTests 15 (14 passed, 1 skipped), BINATests 34, BPERIOTests 55, M2SHEETSTests 53, SheetTests 31, BTPTests 38,
  BPRINTTests 71, ChartLetterTests 45, NoHqTests 110, LibraryBindingTests 20, S3aM1Tests 61, ProjectFileTests 150 (1 skipped), InventoryTests
  93, ODOPTSTests 57 and ReleaseFlowTests 206 (5 skipped): all passed, 0 failed.
- Not run: InitSqlTests and ForkMigrationTests (the coordinator's), and `tests/WindowChecksTests`, which starts the program (section 6).
- `tools/Test-NoEolFlips.ps1` against `main` (merge base `e2fc2306f`), after the docs: 23 changed text files, none changed its line ends.

## 4. Gallery G4, TC-8's part

On the build PC, outside the repository like G1 to G3, in the main checkout's `.git/od-tools/test-results/`:

- **`tc8-gallery-G4/g4-tc8.html`** (the folder also zipped as `tc8-gallery-G4.zip`, 6 MB): the page to show the owner. Its head says what
  TC-8 changes for the office. One row per host: today's DirectX 9 picture of the same host's data (drawn on the build PC with four-sample
  smoothing, the program's own format pick; the printout hosts' painter calls repeated on a DirectX wrapper, because their wrapper is made and
  dropped inside the program, and the big chart's and perio chart's chart data copied from the host's own wrapper) next to the WPF chart's
  picture from the host test, which is its committed baseline. Then File > Graphics drawn off the desktop as a picture: before (the window of
  `main`, with its DirectX format grid) and after (the live view, and "Draw in software"), each control drawn over its place and the computer
  name in the title replaced by a placeholder. Made by `tc8-make_g4_tc8.py` in the same folder from the pictures the host tests wrote with
  `OPMS_CHART_GALLERY` (`pictures/`) and `OPMS_CHART_GALLERY_GRAPHICS_NAME=graphics-before` (`before/`).
- The Chart module and the classic Treatment Plan document are not in the page: they are launch check L-7's, whose captures at 100% and 150%
  (plan section 8, "G4 only") come from the scratch database with synthetic patients only.
- The owner's answer goes into `docs/decisions/owner-questions.md` with the commit and the date (plan section 8). TC-8 merges only after it; TC-9a
  follows it directly (O-26 as revised).

## 5. Launch check L-7, for a person

On the build PC's desktop, with a build of this branch, on a scratch database with synthetic patients only (never an office's data; the G4
captures come only from there). Each step says what to do, what should happen and what to write down. Do every step once at 100% display
scale and once at 150%, and take a screenshot of each host for the gallery.

1. **A computer stored as DirectX.** In the scratch database this computer's `computerpref.GraphicsSimple` is 0 (a new computer's row is
   made with 0), with an empty `DirectXFormat`. Start the program, open a synthetic patient with planned and completed work. *Expected:*
   the Chart module shows the 3D chart at once (no minutes-long wait at start-up for a DirectX format search), smooth-looking teeth with
   slightly jagged edges (the live view). Click a tooth, drag across several, use the Draw tab's pen, eraser and Change Color, add and move a
   free text, move the date slider, switch to another module and back. *Expected:* each responds as in L-4 and L-5.
2. **A computer stored as OpenGL.** In the scratch database, set this computer's `computerpref.GraphicsSimple` to 2 (the window offers no
   OpenGL any more). Restart.
   *Expected:* the same 3D chart. File > Graphics shows "3D chart"; Save without changes leaves the stored 2 (look at the row).
3. **A computer stored as Simple 2D.** Setup > Computers, select this computer, "Use Simple Graphics", restart. *Expected:* the Simple 2D chart
   (pie shapes), as before (O-30). File > Graphics shows "Simple 2D chart". Choose "3D chart", Save. *Expected:* the window closes, the Chart
   module shows the 3D chart, and the row now holds 0.
4. **File > Graphics' preview and the software switch.** Open File > Graphics. *Expected:* two choices, the "Draw in software" check box and a
   preview with made-up work that says "3D chart, live view.". Tick "Draw in software". *Expected:* the preview turns to the smoothed picture at
   once (it may show the background for half a second) and says "drawn in software". Save. *Expected:* the Chart module shows the smoothed
   picture (smooth edges; a refill takes a little longer). Untick it again and Save. Choose "Simple 2D chart" in the window: *Expected:* the
   preview shows the 2D chart and the check box is greyed out.
5. **Another computer from Setup > Computers.** Double-click another (synthetic) computer in Setup > Computers. *Expected:* the warning about
   editing another computer, the window with that computer's choices, and the preview line saying it is drawn by this computer. Change its
   switch and Save: *Expected:* no test and no message; its row changes.
6. **Every host.** With the 3D chart: Show Big (the big chart window); the Treatment Plan module's Print preview, classic and sheet, and its
   e-mail; a referral letter with a tooth chart; a sheet with a tooth chart field filled and printed; the sheet editor with a tooth chart field
   (the preview); Save to Images from the Chart module (a Tooth Charts image category must exist). *Expected:* each shows the 3D chart's
   picture, never an empty box or the 2D chart, and the Treatment Plan module does not flicker when it prints.
7. **The dashboard image.** Open the Patient Dashboard (with a tooth chart widget) and move between synthetic patients, open and close the
   dashboard, refill the Chart module. *Expected:* the widget shows the 3D chart of the current patient, never another patient's.
8. **Graphical perio on a 2D computer (O-31).** With the computer stored as Simple 2D (step 3), open the Perio Chart of a synthetic patient
   with an exam and click Graphical. *Expected:* it opens on the 3D chart (no "you must first select DirectX" message), and Print and Save to
   Images work. Repeat on a 3D computer.
9. **The software switch on a PC with Intel Iris Xe graphics**, if one exists (dotnet/wpf#7076): with the live view the chart shows teeth;
   with "Draw in software" too. Write down the graphics adapter and driver.
10. **A WPF start failure.** The program has no switch to force one (the developer switch is gone), so the forced failure is the test
   `EngineChoiceTests.StoredChoice_WhoseWpfStartFails_FallsBackToSimple2D_WithOneLogLine` (a second process: a stored DirectX and a requested
   OpenGL both fall back to Simple 2D, one log line). On a person's computer, only where the 3D chart really does not start: *Expected:* the
   Chart module shows the Simple 2D chart, no error box, and the ToothChart log has exactly one line saying the Wpf3D tooth chart could not
   start and Simple2D is used; Graphical perio then says "The graphical perio chart needs the 3D tooth chart, which could not start on this
   computer." Otherwise write "covered by the test".
11. **Write down:** for each step, as expected or what differed, with a screenshot of anything that differed (synthetic data only).

**The office check (O-26 as revised: a launch check before a release, not a gate for TC-9a):** one workstation, or a PC
with the office's graphics type, runs the chart's image tests (`ToothChartImageTests`) from a test build with no database and starts the
program once on a scratch database to look at the Chart module's 3D chart and the "Draw in software" switch.

## 6. Open points

- **Owner:** the parity rows ENG-1, ENG-2, ENG-3, ENG-5 and NEW-1 (section 1.1) are not built. Building them would add Open Dental's global
  setting and per-computer "Use if available / Do not use" (reading Open Dental's `DirectX11ToothChartUseIfAvail` and
  `computerpref.GraphicsUseDirectX11`, which answers BIN-A 5), make the smoothed picture the default (revising O-34 (b1)), move 2D computers to
  the 3D chart unless "Do not use" (revising O-30), and refuse graphical perio on 2D computers (revising O-31). The owner decides.
- **`tests/WindowChecksTests`** (the lead's in-process window checks, which start the program and are not part of a Targeted run): B5, I1, I6,
  I7, I11 and the I-group preparation expect the DirectX chart (`radioDirectXChart`, the engine "DirectX", the 2D perio message, the start-up
  format search saving a format). TC-8 leaves them as they are; they need the lead's rewrite for the WPF chart before their next run (the
  rebuilt window keeps `radioSimpleChart` and `butSave`; the 3D choice is `radio3DChart`; the engine is `Wpf3D`; graphical perio opens on a 2D
  computer; the start-up search saves nothing).
- The Chart module's and the classic Treatment Plan document's pictures are checked by L-7 only (section 2).
- TC-9a (directly after TC-8) deletes the DirectX and OpenGL engines, `Engine`'s paths to them, `TestDirectXFormat`, `GetPreferredDirectXFormat`,
  the no-op start-up search and `FormOpenDental`'s call, the legacy recorder's live DirectX path and the tests that start those engines; TC-9b
  then SharpDX, Tao and their release entries.
