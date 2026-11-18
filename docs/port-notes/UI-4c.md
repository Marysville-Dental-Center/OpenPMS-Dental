# Port note: UI-4c (the remaining shared controls)

The plan's section 7 cuts UI-4c into two slices that share no files: UI-4c-1 (imaging and appointment chrome, this section) and UI-4c-2 (dialogs,
progress and the rest, its own section).

## UI-4c-1: imaging and appointment chrome

On `ui/ui4c1-1` from `main` at `1a4fa9152` (UI-4a, UI-4b-1 to UI-4b-3 and UI-9a-2 merged). It changes the look of the controls it owns and nothing
else: no bounds, no layout, no text position, no font, no behaviour. No schema change, no new package, nothing downloaded, no database used, no window
shown.

### 1. For the owner, in plain words

**What changed.**
- Appointments module: the **Day/Week toggle** is white with a thin light grey border; the chosen one is pale blue with a thin blue outline (like a
  pressed toolbar button), the other one turns light grey under the mouse. The **pinboard** is white with a thin light grey border (it was black), its
  "Drag Appointments to this Pinboard" hint dark grey.
- Appointment window: the **provider slider** on the left has lighter lines (the hour lines mid grey, the others pale), a grey outline, and its length
  handle at the bottom is a flat white button with rounded corners that turns light grey under the mouse and while you drag it. The typing cursor in
  the provider bar is blue (it was red).
- Imaging module: the **unmounted bar**'s header is a flat blue bar with white text, like a grid title (it was a gradient). The **windowing slider**
  and the **zoom slider** are flat: no gloss, no gradient. The windowing slider's two handles are small white buttons with a grey outline that darkens
  under the mouse and turns blue while you drag; the zoom slider looks like a toolbar strip: its minus, plus, Fit and 100 cells turn grey under the
  mouse and darker when pressed, the minus and plus are the new Fluent marks, and the zoom value sits in a white box with a thin outline that darkens
  under the mouse and turns blue while you drag it.
- The **docked image's title bar** looks like the floating image windows of the design sheet: blue with white text when selected (its minimise,
  maximise and Windows buttons a lighter blue under the mouse), light grey with dark text when not (darker grey under the mouse), the close button red
  under the mouse; the caption marks are the new Fluent ones.
- The **image list** on the left of the Imaging module looks like the new lists: white with a grey border, the chosen item pale blue with a dark grey
  bar along its left edge, the item under the mouse a very pale blue, the folder rows light grey, and arrows (right when closed, down when open)
  instead of the +/- boxes.
- The **floating windows menu** (the "Windows" button): white with a thin grey border; the places you can send a window to are light grey and turn
  pale blue with a blue outline under the mouse.

**What did not change.** Sizes, positions, fonts and texts (the font change is UI-2b). The appointments on the pinboard and in the book (UQ-8), the
providers' colours in the slider (they are the office's), the black-to-white ramp of the windowing slider (it shows the image's window and level),
the mount's background colour and the outlines and yellow/orange selection marks of the unmounted images (they sit on that colour). Printouts.

**For you to look at.** Gallery G-UI1 part 2e (section 6): every state of each control, WinForms above WPF, at 100 to 200%. The questions in section
3 are small; the recommended answer is what you see.

### 2. Files

| File | What |
|---|---|
| `CodeBase/UI/UiSliderPaint.cs` (new) | the shared tokens and glyphs: the windowing handles (`GetHandleFill`: `SurfaceRaised`, `ButtonHover`, `ButtonPressed`; `GetHandleOutline`: `StrokeStrong`, `StrokeStrongHover`, `Accent`, `Stroke`), the zoom cells (`GetCellFill`: none, `SubtleHover`, `SubtlePressed`), the text token, the 12 px glyph square, the Fluent plus (`GetAddDesign`: `EnumIcons.Add`'s manifest row, Fluent add, so no icon is added) and the minus (`UiGlyph.Subtract`); on UI-4a's `UiControlPaint`; Windows Forms and System.Drawing only |
| `OpenPmsDental/UI/Appt/ToggleDayWeek.cs` | `OnPaint` from the tokens (`GetRectangleItem`, `GetRectangleGlyph` where today's rectangles were); the two unused static brushes gone; test hook `IsTextShown` |
| `OpenPmsDental/UI/Appt/PinBoard.cs` | the board and its border from the tokens, the hint `TextSecondary`; the appointments as before; test hook `IsTextShown` |
| `OpenPmsDental/UI/Appt/ControlApptProvSlider.cs` | the cells, lines, outline and cursor from the tokens on whole pixels (`SmoothingMode.None`; the lines on today's pixels), the slider `UiControlPaint.DrawButton` (`GetRectangleSlider`: the pixels today's path covered; `GetSliderState`); the providers' colours kept; test hooks `IsTextShown`, `IsCursorShown` |
| `OpenPmsDental/UI/ImagesModule/UnmountedBar.cs` (+ designer) | the header a flat `Accent` bar (`GetRectangleHeader`), the title `TextOnAccent`, the outline `Stroke`; the gradient brush gone; the images' outlines and the yellow selection kept; test hook `IsTextShown` |
| `OpenPmsDental/UI/ImagesModule/WindowingSlider.cs` | `Surface`, the range outline and the flat handles from `UiSliderPaint` on today's paths; the ramp kept |
| `OpenPmsDental/UI/ImagesModule/ZoomSlider.cs` (+ designer) | `SurfaceSubtle` with a `Gridline` bottom line, the cells as toolbar buttons (`GetCellState`), the `Stroke` dividers (`GetXDividers`), the Fluent minus and plus, the `StrokeStrong` tick line (`GetRectanglesTickLine`), the value box (`GetValueBoxOutline`); the gradient brushes and the static pens gone; test hook `IsTextShown` |
| `WpfControlsOD/UI/Controls/ImagingModule/UiLookImaging.cs` (new) | the token brushes for the imaging XAML, the Fluent minus and plus geometries, `SetSnapArea` (the floating windows' menu) |
| `WpfControlsOD/UI/Controls/ImagingModule/ImageDockHeader.xaml(.cs)` | `SetColors` from the tokens as the sheet's floating windows; the caption marks are `Path`s with the Fluent subtract, maximize and dismiss (12 DIP, where the lines were); the Windows label coloured through `ColorText` |
| `WpfControlsOD/UI/Controls/ImagingModule/ImageSelector.xaml(.cs)` | `SurfaceRaised`, the `StrokeStrong` outline (`borderOutline`), text `Text`; `SetRowLook` (fill and the selection bar); the expander a Fluent chevron (`pathChevron`); the lines `Stroke` and `Gridline` |
| `WpfControlsOD/UI/Controls/ImagingModule/ImageSelectorTypes.cs` | unchanged: types only, it draws nothing |
| `WpfControlsOD/UI/Controls/ImagingModule/UnmountedBar.xaml` | `Surface`, the header a flat `Accent` rectangle (`rectangleHeader`), the title `TextOnAccent` (`textBlockTitle`); the `.cs` unchanged (the images' outlines kept) |
| `WpfControlsOD/UI/Controls/ImagingModule/WindowingSlider.xaml(.cs)` | `Surface`; `SetLook`: the handles and the range's lines from `UiSliderPaint` (disabled too) |
| `WpfControlsOD/UI/Controls/ImagingModule/ZoomSlider.xaml(.cs)` | `SurfaceSubtle`, the dividers `Stroke` drawn over the cells, the tick line `StrokeStrong`, the bottom `Gridline`, the Fluent minus and plus `Path`s (`pathMinus`, `pathPlus`, where the lines were); `Draw` sets the cells (4 px corners), the value box outline and the text token as the WinForms one |
| `WpfControlsOD/UI/Controls/ImagingModule/WindowImageFloatWindows.xaml(.cs)` | `SurfaceRaised` with a `Stroke` border, the white block `SurfaceRaised`; the snap areas through `UiLookImaging.SetSnapArea` (the 14 hover branches now one line each) |
| `tests/UiRefreshTests/ImagingApptStates.cs`, `ImagingApptStateTests.cs`, `ImagingApptGallery.cs` | the tests and the gallery strips (section 5) |
| `tests/UiRefreshTests/Data/colour-literals.tsv` | 52 literals fewer (23 WinForms, 29 WPF); the WPF imaging folder has none left, `ControlApptProvSlider` keeps its two provider colour defaults |

### 3. Decisions made on the way (say if you disagree)

- **The sheet defines only three of these looks.** The dock header follows the sheet's floating windows (`DesignSheetControls.DrawFloatingWindows`),
  the zoom cells its toolbar button (`DrawToolbarButton`), the image list its list row (`DrawListOrGridRow`) and the toggle's chosen row its toggled
  toolbar button. The other states were chosen from the tokens as the earlier slices did:
  - windowing handles as small buttons (`SurfaceRaised`, `ButtonHover`, `ButtonPressed`) with an input's outline (`StrokeStrong`,
    `StrokeStrongHover` under the mouse, `Accent` while dragged, `Stroke` disabled); the range between them takes the same outline by its own state;
  - the zoom value box as an input (square, as the sheet's text input): `SurfaceRaised`, `StrokeStrong`, `StrokeStrongHover`, `Accent` while dragged;
  - the provider slider's length handle as a plain button (`UiControlPaint.DrawButton`), its hour lines `StrokeStrong`, the others `Gridline`, its
    outline `StrokeStrong` (it is an input), its typing cursor `Accent` (was red);
  - the floating windows' snap areas as toggles: `SurfaceSubtle` with `StrokeStrong` at rest, `AccentSubtle` with `Accent` under the mouse;
  - the dock header's Windows button while its menu is open: `SurfaceRaised` with `Text` (today's white with black);
  - the zoom slider's cells: their hover fill has 4 px corners and lies under the dividers, which stay full height (they are cell borders, not toolbar
    separators, so they are not inset).
- **Kept as content:** the windowing ramp (black, the grey gradient, white; it is the image's window and level, not decoration, so decision 6's "no
  gradients" does not apply), the providers' colours, the appointments on the pinboard and the selected one's provider-coloured outline (UQ-8), the
  mount's background colour and the unmounted images' outlines and selection marks (WinForms yellow, WPF orange): they sit on the office's mount colour,
  often black, where the accent would be hard to see.
- **Fonts stay** (UI-2b): the provider slider's Courier and Sans Serif, the zoom slider's Segoe UI 9, the unmounted bar's titles, the pinboard's hint.
- **The image list's selection bar** is `SelectionBarNoFocus` in practice: the selector never takes keyboard focus (it has no keyboard handling), so
  the bar is `Accent` only if it ever does (`IsKeyboardFocusWithin`, as the lists). The bar starts inside the selector's 1 px outline (which lies over
  the rows' first pixel) and lies under the row's icon and text: the Fluent document icons start on the bar's second pixel and still show whole.
- **The toggle shows no focus ring:** it takes focus on a click but has no keyboard handling, so a ring would promise keys that do nothing.
- **The dock header's Windows label** is a `UI.Label`, which since UI-4a ignores `Foreground` and draws `Text` unless `ColorText` is set; today's code
  set `Foreground`, so on `main` the label shows dark text on the selected blue header. It now goes through `ColorText` (white on the selected header,
  invisible on the empty one, as before UI-4a).
- **Lines on whole pixels:** the provider slider draws without antialiasing now (its lines and cells were antialiased on integer coordinates); the
  zoom slider's lines are filled rectangles; the WinForms toggle's and the windowing range's 1 px outlines are on the pixels today's `DrawRectangle`
  covered.

**Owner questions** (the recommended answer is what is built): the provider slider's cursor in `Accent` (or `Text`, like a Windows caret); the kept
yellow/orange marks of the unmounted images (or `Accent`); the zoom dividers full height (or inset 4 px like toolbar separators).

### 4. Not done, and why

- Pictures of the real windows (the Appointments and Imaging modules, `FormApptEdit`, the floating image windows) need shown windows: section 7 has
  the steps for the lead.
- WPF hover is set in the tests as the mouse handlers set it (the private hover fields and `Draw`/`SetColors`); a connected desktop shows it live.
- The floating windows' menu is checked at its border, background and two snap areas made by its own `CreatePath`; its `Loaded` step places the areas
  from the real screens and needs a shown window.

### 5. Tests

All in `tests/UiRefreshTests` (no new test assembly):
- `ImagingApptStateTests` (13 tests): `SliderPaint_TokensAndGlyphs_AsChosen`; WinForms `ToggleDayWeek` (Day chosen, the mouse on Week, Week chosen),
  `PinBoard` (empty, two synthetic appointments), `ControlApptProvSlider` (rest, the mouse on the slider, dragging, the cursor), `UnmountedBar`,
  `WindowingSlider` (rest, a handle and the range under the mouse, dragging, disabled), `ZoomSlider` (rest, the minus under the mouse, Fit pressed, the
  value box under the mouse, dragging it, disabled): each drawn off screen without text and compared with the token drawing of the state at the
  control's own geometry (`ImagingApptDrawings`) by ChartImageCompare's tolerance, every pixel; then with text, its pixels reach the state's token. WPF
  `ImageDockHeader` (empty, selected with and without hover on maximise and close, not selected with and without hover on minimise, the Windows menu
  open), `UnmountedBar`, `WindowingSlider`, `ZoomSlider`: the solid pixels, then the texts and the glyphs (their brushes and that they draw). WPF
  `ImageSelector` (rows, hover, the selection bar inside the outline, the chevrons, the outline) and the floating windows' menu (border, background,
  snap areas) pixel by pixel and by brush. The kept content is checked as kept (the appointments' pixels, the provider colour, the mount's outlines).
- The layout snapshot and the bounds record (`LayoutSnapshotTests`, `Scan_TodaysFont_ReportsNothingNew`): unchanged and green; no designer layout
  changed (two designers lost their brush disposals only).
- The scan rerun (`OPMS_UI_SCAN=1`, into `.git/od-tools/test-results/ui4c1-scan`): every file the same as UI-4b-3's rerun but the timing line (Segoe
  UI 9 makes 1,775 controls overflow in 450 windows, 8.25 makes 637 in 245); today's font reports nothing new.

### 6. Gallery G-UI1 part 2e

`.git/od-tools/test-results/ui4c1-G-UI1-part2e/` (`index.html` and the zip beside it; the owner's summary is the note at the top of the page), made
from the strips in `.git/od-tools/test-results/ui4c1-G-UI1-part2e-strips/` (each of `scale-100` to `scale-200`): `toggle-pinboard-winforms.png`
(the toggle's three states above the pinboard's two), `provslider-winforms.png`, `unmountedbar.png`, `windowingslider.png` (at its real width,
154), `zoomslider.png` (WinForms above WPF), `dockheader-wpf.png` (selected above not selected and empty), `imageselector-floatwindows-wpf.png`
(the image list with an item chosen, the same with the mouse on another item, the floating windows' menu). WinForms scales through LayoutManager's
scale, WPF through its render scale; texts drawn in points stay near their 96 DPI size off screen, as in the earlier slices' strips. "Only after", as
UI-4a's: the controls keep no copy of today's look. The state tests run the windowing slider 263 wide so its handles sit on whole pixels.

### 7. For the lead

- The strips render off screen and were made in the disconnected session (WinForms and WPF both draw). To make them again:

      set OPMS_UI_IMAGING_GALLERY=<od-tools>\test-results\ui4c1-G-UI1-part2e-strips
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out <od-tools>\test-results\ui4c1-strips.txt
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -After <od-tools>\test-results\ui4c1-G-UI1-part2e-strips -Out <od-tools>\test-results\ui4c1-G-UI1-part2e -Name G-UI1 -Note "<the summary of section 1>"

- The real windows (G-UI2 and G-UI3) need shown windows: at the owner's connected desktop, after a full Debug x64 lane build, group U against the
  before set (docs/port-notes/UI-0c.md section 5), as for the earlier slices:

      set OPMS_WINDOW_CHECKS=1
      set OPMS_WINDOW_CHECKS_DUMP=<od-tools>\winchk-gate3a\opendental_dev.sql
      set OPMS_WINDOW_CHECKS_GROUPS=U
      set OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui4c1-groupU
      set OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18
      set OPMS_WINDOW_CHECKS_UI_BEFORE=<od-tools>\test-results\ui-before\run1
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <od-tools>\test-results\ui-before\run1 -After <od-tools>\test-results\ui4c1-groupU -Name G-UI3 -ClipNotes <od-tools>\test-results\ui4c1-groupU\ui-walker\clip-notes.tsv

  (Split into `UW`, then `UM,UD` if one run does not fit; the before folders `run1` to `run5` and `fix1` hold the groups as UI-0c froze them.) U0
  shows the Appointments module (toggle, pinboard), `FormApptEdit` (the provider slider), the Imaging module (sliders, image list, dock header,
  unmounted bar) and the windows with a WinForms zoom or windowing slider as changed; the bounds record must show no difference. Worth a live look:
  the zoom and windowing sliders under the mouse and while dragging, the dock header selected and not, the Windows menu of a docked image with the
  mouse over a snap area, the image list's chosen item, and the provider slider while typing in it.
- `docs/port-notes/UI-4c.md` is shared with UI-4c-2, which writes its own section: on merge keep both sections under the one title.
- The owner's answers to the three questions of section 3.

### 8. Runs

Full filter build x64 through the lane: 0 errors. Targeted run (x64, after the full build, `.git/od-tools/test-results/ui4c1-final.txt`):
UiRefreshTests 183 (175 passed, 8 opt-ins skipped: the seven galleries and the scan), WindowChecksTests 72 (29 passed, 43 opt-in skipped),
UnitTests 1,731 (1,728 passed, 3 skipped), NoHqTests 122/122. The scan and strips run (`ui4c1-scan-strips.txt`, with `OPMS_UI_SCAN` and
`OPMS_UI_IMAGING_GALLERY`) passed too. `tools/Test-NoEolFlips.ps1 -Base main`: no file changed its line ends.
