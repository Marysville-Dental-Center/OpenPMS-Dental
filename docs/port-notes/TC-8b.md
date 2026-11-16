# Port note: TC-8b (File > Graphics as Open Dental 26.2 has it)

Package TC-8b of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), section 10, "Open Dental
parity (2026-11-14)", the TC-8 table: rows ENG-1, ENG-2, ENG-3, ENG-5 and NEW-1, and ENG-6 from ENG-2's caveat): branch `chart/tc8b-1` from
`main` (`3d910a435`, TC-8 and TC-9a merged). **Status: done on the branch; its gallery waits for the owner, and TC-8b merges only after the
owner approves it.** Nothing was downloaded into the repository and no package or DLL was added; no schema changed; no program was started with
a window on the desktop (the tests draw forms off the desktop, as the chart's image tests do).

Commits: `97a41c7c4` (the design), `616724865` (the business layer: the relay, the wrapper, `ComputerPref(s)`), `e0d6dc33e` (File > Graphics
and the comments), `4cdcab201` (the tests), then the docs and the fixes after the full build (section 6).

## 1. The owner's answers in force

- **2026-11-15, "Match Open Dental 26.2"** for File > Graphics (`docs/owner-questions.md`, gallery G4's TC-8 entry; the plan's O-30 note):
  rows ENG-1, ENG-2, ENG-3, ENG-5 and NEW-1 are confirmed. They revise **O-30** (a computer stored as Simple 2D now gets the 3D chart unless
  its setting is "Do not use"), **O-31** (graphical perio no longer opens on a 2D computer) and **O-34 (b1)** (the smoothed picture, not the
  quick live view, is the on-screen default). The earlier answers stay in the plan as history.
- **The owner's rule of 2026-11-14:** every tooth chart look and behaviour as the latest Open Dental shows publicly; where a public source does
  not settle a detail, the closest documented reading is built and listed as od-unknown (section 4).
- **BIN-A 5** (`docs/owner-questions.md`, already answered on 2026-11-15): keep `DirectX11ToothChartUseIfAvail` and
  `computerpref.GraphicsUseDirectX11`. TC-8b reads them again.
- **Decision 22:** no OpenPMS install is migrated; a return is a return to Open Dental itself, which must read the stored rows as before.

## 2. Each row: what Open Dental shows, and how it is matched

The sources, all looked at for this package: Open Dental's manual page **Graphics Preferences** (https://www.opendental.com/manual/graphics.html)
and its screenshot (https://www.opendental.com/manual/images/graphics.png, 894x710, the 26.x window); the page **Computers**
(https://www.opendental.com/manual/computer.html); **Graphical Perio Chart** (https://www.opendental.com/manual/graphicalperiochart.html, which
names no graphics mode); Open Dental's DirectX 11 chart in the 2026 tech blog screenshot
(https://i0.wp.com/odtechblog.wordpress.com/wp-content/uploads/2026/08/image-6.png?ssl=1); and Open Dental's own 24.3 source of the same window,
start-up and perio gate (public under the GPL; in this repository's history before BIN-A, `git show 972042584^:OpenDental/Forms/FormGraphics.cs`).

### 2.1 ENG-1: File > Graphics on Open Dental's layout (`OpenPmsDental/Forms/FormGraphics.cs`, `.designer.cs`, `.resx`)

What Open Dental shows (the screenshot, top to bottom): a red "This window is obsolete!" and an explanation; the group **"DirectX 11 Tooth
Chart on all computers"** ("This option affects all computers in the office.  All other options are specific to this computer.", "Use if
available.  Advantage is that this avoids having to install DirectX9 SDK.", "Do not use.  Advantage is that older or underpowered hardware
might work better.  This is an easy first step for troubleshooting certain graphics problems."); then in one row **"DirectX 11 Tooth Chart"**
("Use the global setting", "Use if available", "Do not use"), **"DirectX 11 status on this computer"** ("Available", "Sparks3D.dll not found,
or Chart not used yet, or unable to load due to graphics driver issue", two radio buttons that cannot be changed by hand) and **"Older Tooth
Chart Technology"** ("DirectX 9 (requires installation of SDK)", "Simple 2D (pie shapes)", "OpenGL (no perio)"); then **"Options For Older
Technology"** ("Most users will never need to change any of these options.  These are only used when the 3D tooth chart is not working
properly.", OpenGL filters and the format grid); Save at the bottom right.

How it is built, with the WPF 3D chart in DirectX 11's place:

- The same groups in the same places (client area 892x683 as Open Dental's): **"3D Tooth Chart on all computers"** (`radioChart3DAllUse`,
  `radioChart3DAllDontUse`) with Open Dental's sentences; "Use if available" names its advantage for this program ("the detailed 3D tooth chart
  and the graphical perio chart"), since no SDK is involved. **"3D Tooth Chart"** for this computer (`radioChart3DThisGlobal`,
  `radioChart3DThisUse`, `radioChart3DThisDontUse`). **"3D Tooth Chart status on this computer"** (`radioStatusAvailable`,
  `radioStatusFellBack`, `AutoCheck` off, so they cannot be changed by hand; NEW-1 below). **"Older Tooth Chart Technology"** with
  **"Simple 2D (pie shapes)"** (`radioSimpleChart`, always checked: it is the one older technology left after TC-9a) and a line saying it is
  used when the 3D tooth chart is not, and that the older 3D technologies were retired.
- **"Options For The 3D Tooth Chart"** with Open Dental's sentence ("... only used when the 3D tooth chart is not working properly.") and
  **"Draw in software"** (`checkDrawInSoftware`) with its explanation (ENG-3).
- **The live preview is kept** (the fork's own, beside the options, where Open Dental has its format grid): it draws made-up work with the
  chart the window's choices give on this computer, at once on every click, and says which chart started.
- **The medical wording is kept**, as Open Dental's own Load words the window in a medical practice or clinic: "Use Simple Graphics" for the
  older choice, "Options For 3D Graphics" for the options group and "... when the 3D graphics are not working properly." (Open Dental's 24.3
  Load; its DirectX 11 groups keep their words there, and so do the 3D groups here.) TC-8's three medical strings for its two-choice window
  went with that window.
- The top: an explanation in Open Dental's place, without the red "This window is obsolete!": "You should rarely need to go into this window.
  The upper half is for the 3D tooth chart, ... The lower half is for the older Simple 2D chart and for options of the 3D tooth chart, for
  troubleshooting." This window holds "Draw in software" and the preview, so it is not obsolete here (section 4, item 6).
- Unchanged from TC-8: the window names no engine (no `ToothChartEngine`, `DrawingMode`, DirectX or OpenGL in its code); opened from Setup >
  Computers for another computer it warns, logs `GraphicsRemoteEdit` and **saves without a test**, and its preview is drawn by this computer;
  on Linux it says "Linux users must use simple tooth chart." and this computer's 3D setting is greyed out.
- Save (`butSave_Click`): the relay's save rule on the computer's row (ENG-2), `ComputerPrefs.Update`, then the global setting through
  `ToothChartRelay.SetChart3DOnAllComputers` (`Prefs.UpdateBool`, as Open Dental's Save); when it changed, a `GraphicsEdit` security log entry
  ("3D Tooth Chart on all computers set to Use if available / Do not use") and `DataValid.SetInvalid(InvalidType.Prefs)`, as Open Dental
  signals it.

### 2.2 ENG-2: Open Dental's stored rows, no schema change (`WpfControlsOD/UI/ToothChart/ToothChartRelay.cs`)

What Open Dental does (its 24.3 start-up, `ControlChart.InitializeOnStartup`, and the manual): `PrefC.GetBoolSilent(DirectX11ToothChartUseIfAvail,
true)` turns the DirectX 11 chart on, `GraphicsUseDirectX11` No turns it off and Yes on for the computer; otherwise the chart is
`GraphicsSimple`'s older technology.

Built the same way in the relay's settings helpers:

- `IsChart3DOnAllComputers()`: the preference, "Use if available" (the default 1, also when the row or the cache is missing).
  `SetChart3DOnAllComputers(bool)`: `Prefs.UpdateBool`, true when it changed.
- `GetChart3DThisComputer(computerPref)`: `GraphicsUseDirectX11` (Unknown, a new computer's 0, is "Use the global setting").
- `IsChart3DUsed(global,thisComputer)`: Yes wins, No wins, anything else follows the global setting.
- `GetEngineForComputer(computerPref)`: the WPF chart when the computer uses the 3D tooth chart (it falls back to Simple 2D on its own, logged
  once, when it cannot start); otherwise the older technology, of which only Simple 2D is left after TC-9a, so every stored `GraphicsSimple`
  draws Simple 2D there. `ApplyComputerPref` (every host), the snapshot (`ToothChartSettingsSnapshot`, which now also redraws the Chart module
  when the global setting changes the chart) and the perio gate use it. TC-8's `GetEngineForStoredMode` and `Is3DChart` are gone. The
  wrapper's own `DrawMode` request is unchanged (0 and 2 start the WPF chart, 1 the 2D chart).
- **The save rule** (`SetGraphicsChoice(computerPref,globalBefore,globalAfter,thisComputer,isDrawnInSoftware)`): only what a choice changes.
  `GraphicsUseDirectX11` when this computer's choice changed. `GraphicsSimple` by TC-8's rule, and only when the window's choices change the
  chart the computer draws: a change to Simple 2D writes 1 (so Open Dental, whose older technology then applies, draws Simple 2D too); a
  change to the 3D chart writes 0 over a stored 1 and keeps a stored 0 or 2. So a return to Open Dental reads the same chart: its DirectX 11
  chart where this program draws the WPF chart, its Simple 2D where this program drew Simple 2D after a change. A row that is only kept (a
  computer that draws Simple 2D here under "Do not use" with a stored 0 or 2 nobody changed) is Open Dental's DirectX 9 or OpenGL chart there,
  which this program retired. `DirectXFormat`: ENG-3.
- **The guard** `tests/BINATests/ProprietaryBinariesRemovedTests.cs` (`GraphicsWindow_HasNoDirectX11Settings`, which failed any read of the two
  settings) is rewritten as `DirectX11Settings_ChooseTheWpfChart_ReadOnlyByTheRelay_TC8b`: in the program, the business layer, the chart
  libraries and CodeBase (comments, Open Dental's CRUD, table type, preference list and conversion steps left out), the settings are named
  exactly five times in the relay (the preference's read and write, the column's read, comparison and write) and once in `ComputerPrefs.cs`
  (ENG-6's `UPDATE`), and File > Graphics reaches them only through the relay.

### 2.3 ENG-3: the smoothed picture is the on-screen default; the quick live view a per-computer option

What Open Dental shows: its DirectX 11 chart with smooth, anti-aliased edges (the tech blog screenshot), as TC-0's smoothed DirectX pictures.

Built: `ToothChartRelay.IsDrawnInSoftware` is true unless `computerpref.DirectXFormat` is the new value `WpfLiveView`
(`ToothChartWrapper.DirectXFormatWpfLiveView`, no `;`, so Open Dental reads no DirectX device format from it): an empty value (every computer
so far), TC-8's `WpfSoftware` and a DirectX device format an older build saved all give the smoothed picture. "Draw in software" is ticked by
default; unticked, Save writes `WpfLiveView`; ticked again, Save clears only that value. Nothing is cut: the live view stays, as the option.
The wrapper's own default (a wrapper no relay has set) stays the live view, as every host sets the computer's value through the relay.

G1's times, weighed as the row asks (O-34; TC-3's fix rounds, on the build PC in the program's software mode): a refill takes 332 and 572 ms
with the smoothed picture at 100% and 150%, against 135 and 169 ms with the live view (and 34 and 48 ms with the DirectX chart that is gone); a
selection 16 ms against 10 ms. The default is therefore about 0.2 to 0.4 s slower per refill; the window's line says so ("the quick live view,
which redraws about twice as fast but has jagged edges"), and the release note too.

### 2.4 ENG-5: graphical perio only on a computer that uses the 3D chart (`ToothChartRelay.CanOpenGraphicalPerio`)

What Open Dental shows: the Graphics page lists the Graphical Perio Chart as allowed with DirectX 9 and not allowed with Simple 2D or OpenGL
("DirectX 11: allowed" is inferred: Open Dental's 24.3 gate skipped its check when the DirectX 11 chart was in use). Its 24.3 gate said "In the
Graphics setup window, you must first select DirectX." otherwise.

Built: the gate opens when `GetEngineForComputer` gives the WPF chart, and otherwise refuses with Open Dental's sentence, the 3D tooth chart in
DirectX's place and the menu named: **"In the Graphics setup window (File > Graphics), you must first select the 3D tooth chart."**
(`ToothChartRelay.PerioNeeds3DChartMessage`; FormPerio shows it, as before, through `MsgBox`). A computer stored as Simple 2D under "Use the
global setting" uses the 3D chart, so its graphical perio chart opens. A 3D chart that cannot start still falls back when the perio window starts
it, and the perio mode says the 3D chart could not start (TC-8).

### 2.5 NEW-1: the status line (`ToothChartRelay.GetChart3DStatus`, `ToothChartWrapper.CanStartWpf3D`)

What Open Dental shows: "DirectX 11 status on this computer", "Available" or the line about Sparks3D.dll, set by the program, never by hand.

Built: on Load the window asks whether the WPF chart can start in this process (`CanStartWpf3D`: the renderer's start check, which a computer
where WPF cannot draw fails and keeps failing; Linux keeps Simple 2D) and checks **"Available"**, or checks the second line and shows
**"Fell back: " and the reason** (for example "Fell back: The WPF tooth chart's renderer could not start: ..."). Reading it writes no log line.
For another computer it is this computer's status, as Open Dental's. A test hook (`ToothChartRenderer.SetStartFailureForTests`, never called by
the program) lets the window tests and the gallery show a fallen-back computer without a second process; the image tests also check the real
start failure in a second process.

### 2.6 ENG-6: Setup > Computers' "Use Simple Graphics" (`ComputerPrefs.SetToSimpleGraphics`)

What Open Dental shows: the Computers page says the button sets another computer's Graphics Preferences "to simple", "as a fix for machines
that do not have DirectX or OpenGL installed or working properly", and that it "is the same as selecting the Simple Tooth Chart in Graphics";
the Graphics page says to use it to switch a computer that cannot open the window "to use Simple Graphics". Open Dental's 24.3 code wrote
`GraphicsSimple=1` only.

Built, the closest documented reading: the button writes `GraphicsSimple=1` **and** `GraphicsUseDirectX11=2` ("Do not use"), because under
ENG-2 only "Do not use" lets the older technology apply, and the pages' purpose is a computer that draws Simple 2D afterwards. Its message and
security log entry are unchanged. Listed as od-unknown (section 4, item 1).

### 2.7 Also in the files touched (TC-9a's review)

Comments that still described the deleted engines now say what is: `ControlChart.cs` (the settings call that "saves the pixel format", the
start-up comment about the DirectX 11 settings, "Falls back to OpenGL, then Simple 2D"; the dead, never-wired `butLoadDirectX_Click` with its
commented call is deleted), `ToothChartMouse.cs` (the class summary: the recorded DirectX answers), `ToothGraphic.cs` (`Copy` and the
constructor's callers), `ToothChartWrapper.cs` (`SetRetainedRoot`; the two dead `toothChartOpenGL` lines deleted; `using System.Text;`, which
nothing needed, removed), `ToothChartRelay.cs` (the class summary), `FormSheetDefEdit.cs` (the dead `AutoFinish` line deleted),
`ToothChartEngine.cs`, `FormPerio.cs` and `FormPerioGraphical.cs` (the gate). `ToothChart2D.cs` keeps its history line. The window checks I6
and I7 use the one `SetGraphics` helper of `InProcChecks.IJ.cs` (section 3.2).

## 3. Tests

### 3.1 What was added or changed

- **`ToothChartImageTests/EngineChoiceTests.cs`:** the mapping of every combination of the global setting (on, off), this computer's setting
  (Unknown, Yes, No, an unknown number) and `GraphicsSimple` (0, 1, 2, an unknown number): 32 cases; the preference's read and write in the
  source. The smoothed picture as the default and only `WpfLiveView` as the live view; a new computer's row starts the WPF chart drawn in
  software. `ApplyComputerPref` for every setting, stored mode and format (the switch first, nothing written). The save rule: 14 cases of
  global, computer and stored mode (only what a choice changes; Open Dental reads the same chart), the live view's value written or cleared,
  saved then applied for 54 combinations. The status: "Available", and "Fell back: " with the reason through the renderer's kept failure (then
  cleared; no log line). The perio gate for every combination, with the message. The second-process start failure prints the status too.
- **`CallerSourceTests`:** the preview's and the save rule's calls; the relay's engine choice (no `GetEngineForStoredMode`); File > Graphics'
  groups in Open Dental's order and words, eight radio buttons, no `radio3DChart`, the medical wording, the status, the Save order; the perio
  gate's refusal and FormPerio's message box.
- **`WpfLiveViewTests`:** one message (a DirectX format is another value, and since TC-8b also the default).
- **`ToothChartHostTests`** (through the program on the scratch database): `GraphicsWindowTests` rewritten (Load in 8 states with the radio
  buttons, the box, the status, the preview's engine and line, and nothing written; the fallen-back status and preview; the medical wording;
  Save in 11 cases with the stored row, the preference, the security log entry and the chart the saved row starts; the gallery's seven states).
  `HostTests`: a computer stored as Simple 2D draws the WPF chart's sheet picture under "Use the global setting" and the 2D picture under "Do not
  use" or the global "Do not use" (for 0, 1 and 2), "Use if available" wins over the global "Do not use"; the quick live view gives the same
  pictures; **graphical perio on a 2D computer is refused** with the message, and opens on a computer stored as Simple 2D that uses the 3D
  chart; the snapshot sees the global setting, this computer's and the live view's value. `GraphicsWindowCheckCastTests`: one comment. `HostDb`:
  a run may name its scratch databases' prefix (`OPMS_CHART_HOST_TEST_DB_PREFIX`, "tc8" + letters or digits + "_"); this package's runs used
  `tc8b_`, so they made and dropped only their own databases.
- **`BINATests`:** the guard (section 2.2).
- **`WindowChecksTests`** (built, never run here; the lead's at the gate): **I6** opens graphical perio on the 3D chart as before, then File >
  Graphics > this computer's "Do not use" > Save through `SetGraphics`, and expects Graphical to show the message (`CheckPerioOnSimple2D`, the
  message read and compared; a perio window instead fails). **I7** reads the new window's controls (the global setting, this computer's three
  choices, "Available", Simple 2D, the box ticked by default) as I6 left them, closes it, then `SetGraphics(YN.Unknown)` > Save: the Chart draws
  the WPF chart again and the row is `0 0 ...` (0 over I6's 1); then "Draw in software" unticked (`WpfLiveView`) and ticked again (empty). The
  formerly unused `SetGraphics` is now the one helper of both; `SetDrawInSoftware` no longer touches the 3D settings. `IJTests`' two labels.

### 3.2 The baselines

**No baseline was re-recorded, and none changed.** TC-8b changes no drawing: the on-screen default changes no printout picture (every host's
picture is the renderer's either way), and the 2D picture of a computer that does not use the 3D chart is TC-8's `sheet-simple2d` (the same
chart and data; `GraphicsSimple` does not enter it). `git diff main -- tests/ToothChartImageTests/Baselines tests/ToothChartImageTests/CallLogs
tests/ToothChartHostTests/Baselines` is empty: every WPF, Simple 2D, legacy, smoothed and host baseline keeps its bytes.

## 4. Open Dental unknown (od-unknown), as built until a source settles them

1. **ENG-6:** whether Open Dental 26.2's "Use Simple Graphics" also sets the computer's DirectX 11 setting to "Do not use". Built: yes (section
   2.6). Settled by: Open Dental 26.2's `computerpref` row of a computer before and after the button, or a manual sentence about the DirectX 11
   setting on the Computers page.
2. **ENG-5's wording in 26.2:** the message on a computer whose chart is Simple 2D. Built: the 24.3 sentence adapted (section 2.4). Settled by:
   a screenshot of Open Dental 26.2's Perio Chart > Graphical on a Simple 2D computer.
3. **NEW-1 under "Do not use":** Open Dental 24.3's status showed the second line whenever its DirectX 11 chart was not in use, "Do not use"
   included; the manual says the status shows "whether DirectX 11 is available on the computer". Built: availability ("Available" while the 3D
   chart can start, whatever the settings). Settled by: a screenshot of Open Dental 26.2's window on a computer set to "Do not use".
4. **The fallback line's words:** Open Dental names Sparks3D.dll and a driver issue; this program shows "Fell back: " and the start check's own
   reason (NEW-1's words). Settled by: nothing needed unless the owner wants Open Dental's fixed sentence.
5. **The medical wording of the 3D groups:** Open Dental's 24.3 Load changed only its older choices and options there; 26.2's is not public.
   Built: 24.3's. Settled by: a screenshot of Open Dental 26.2's window in a medical practice.
6. **The red "This window is obsolete!" and Open Dental's explanation:** not built as Open Dental words it (its DirectX 9, OpenGL and SDK
   sentences do not apply, and this window keeps the preview and "Draw in software"); an explanation in the same place says what each half is
   for. A wording choice, not a look of the chart; the owner may ask for the red line.
7. **ENG-3** is implied from Open Dental's screenshots (its DirectX 11 chart is smooth); its own on-screen technique is not public.

## 5. The gallery (outside the repository)

On the build PC, in the main checkout's `.git/od-tools/test-results/tc8b-gallery/` (zipped as `tc8b-gallery.zip`):

- **`tc8b.html`**, the page to show the owner: Open Dental's public screenshot (its URL, and a copy fetched for the page), File > Graphics
  before (TC-8's window as TC-8's gallery drew it: the live view, and "Draw in software") and after in seven states, each drawn off the desktop
  by `GraphicsWindowTests.Gallery_DrawsTheWindowOffTheDesktop_InEachState` with the preview's own picture over its place and the computer's
  name replaced by SYNTHETIC-PC: the default (global on, "Use the global setting", the smoothed picture), the global "Do not use" (Simple 2D),
  this computer "Use if available" under the global "Do not use" (3D), this computer "Do not use", the quick live view, a fallen-back status
  ("Fell back: ..." and the 2D preview), and a medical practice. Under each, what changed and why.
- The host pictures the same run wrote (`pictures/*.wpf.png`, the committed baselines' pictures, unchanged).
- TC-8b merges only after the owner approves it; the answer goes into `docs/owner-questions.md` with the date.

## 6. Builds and runs (build PC, `.git/od-tools/test-results/`)

Section 7 of this note is filled in after the full build; see there.

## 7. Launch check L-7, updated for TC-8b (for a person)

On the build PC's desktop, with a build of this branch, on a scratch database with synthetic patients only (never an office's data). Do each
step once at 100% display scale and once at 150%. Steps 6, 7, 9 and 10 of TC-8's L-7 (`docs/port-notes/TC-8.md`, section 5) stay as written;
these replace steps 1 to 5 and 8:

1. **A new computer.** In the scratch database this computer's `computerpref` row has `GraphicsSimple` 0, `GraphicsUseDirectX11` 0 and an empty
   `DirectXFormat` (a new row is made so), and `preference.DirectX11ToothChartUseIfAvail` is 1. Start the program and open a synthetic
   patient. *Expected:* the 3D chart with smooth tooth edges (the smoothed picture, the default now). Click a tooth, drag across several, use
   the Draw tab, move the date slider. *Expected:* each responds; a refill takes a moment longer than before TC-8b (about a third of a second).
2. **File > Graphics.** *Expected:* a window laid out like Open Dental's (compare its manual picture): "3D Tooth Chart on all computers" with
   "Use if available" checked, "3D Tooth Chart" with "Use the global setting" checked, "3D Tooth Chart status on this computer" with
   "Available" checked (clicking the status does nothing), "Older Tooth Chart Technology" with "Simple 2D (pie shapes)", "Options For The 3D
   Tooth Chart" with "Draw in software" ticked, and the preview of made-up work saying "3D chart, drawn in software". Click each choice:
   *Expected:* the preview changes at once ("Do not use" gives the pie-shaped 2D chart and greys out the check box).
3. **This computer "Do not use".** Choose it, Save. *Expected:* the Chart module shows the Simple 2D chart; the row now has `GraphicsSimple` 1
   and `GraphicsUseDirectX11` 2. Perio Chart > an exam > Graphical. *Expected:* the message "In the Graphics setup window (File > Graphics), you
   must first select the 3D tooth chart." and no graphical perio window. Back in File > Graphics choose "Use the global setting", Save.
   *Expected:* the 3D chart again; the row has `GraphicsSimple` 0 and `GraphicsUseDirectX11` 0; Graphical opens the graphical perio chart.
4. **The global setting.** Choose "Do not use" under "3D Tooth Chart on all computers", Save. *Expected:* the 2D chart here, and on a second
   (synthetic) computer after its next refresh; `preference.DirectX11ToothChartUseIfAvail` is 0 and an audit trail entry "3D Tooth Chart on
   all computers set to Do not use" (Graphics Edit) exists. Choose this computer's "Use if available", Save: *Expected:* the 3D chart here
   while the global setting stays "Do not use". Put both back ("Use the global setting", global "Use if available").
5. **The quick live view.** Untick "Draw in software", Save. *Expected:* the 3D chart with slightly jagged edges, refills a little faster;
   `DirectXFormat` is `WpfLiveView`. Tick it again, Save: the smooth picture, `DirectXFormat` empty.
6. **A computer stored as Simple 2D before TC-8b.** Set this computer's row to `GraphicsSimple` 1 and `GraphicsUseDirectX11` 0 in the scratch
   database, restart. *Expected:* the 3D chart (O-30 as revised); File > Graphics shows "Use the global setting"; Save without changes leaves
   the row as it was.
7. **Another computer from Setup > Computers.** Double-click a synthetic computer: *Expected:* the warning, the window with that computer's
   choices and this computer's status, the preview line saying it is drawn by this computer; change a choice and Save: no test, no message, its
   row changes. Select it and click "Use Simple Graphics": *Expected:* "Done."; its row has `GraphicsSimple` 1 and `GraphicsUseDirectX11` 2.
8. **A computer where the 3D chart cannot start**, only if one exists (otherwise write "covered by the tests"): *Expected:* the Simple 2D chart
   without an error box, File > Graphics' status "Fell back: " and a reason, and Graphical's message as in step 3 only if that computer is set
   to "Do not use" (otherwise the graphical perio window says the 3D chart could not start).
9. **Write down** for each step: as expected, or what differed, with a screenshot of anything that differed (synthetic data only).
