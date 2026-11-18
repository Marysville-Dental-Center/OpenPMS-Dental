# Port note: UI-4c (the remaining shared controls)

Package UI-4c of `docs/specs/ui-refresh-plan.md` (section 7), cut into two slices that share no files: UI-4c-1 (imaging and appointment
chrome) and UI-4c-2 (dialogs, progress and the rest). Each slice has its own section below.

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

## UI-4c-2: dialogs, progress and the rest

On `ui/ui4c2-1` from `main` at `1a4fa9152` (UI-4a, UI-4b-1, UI-4b-2, UI-4b-3, UI-5, UI-6, UI-9a-1, UI-9a-2 merged). It changes the look of the
controls it owns and nothing else: no bounds, no layout, no text position, no behaviour. No schema change, no new package, nothing downloaded, no
database used, no window shown.

### 1. For the owner, in plain words

**What changed.** The progress bars (the "Please wait" windows and the program's other progress bar) are a flat white track with a thin light grey
border and rounded ends, filled with the program's blue; while the program does not know how long a job takes, a blue segment glides across the
track (it was the Windows green bar or a shaded bar). The colour buttons that pick a colour (provider and carrier colours) show the colour in a small
rounded box with a grey border that darkens under the mouse, the blue focus ring after Tab, and "no colour" as a white box with a thin diagonal line.
The small warning triangle of the database integrity check is a deeper amber (it was light orange; the white "!" is now easy to read), the red one
the new red; in the WPF windows the red one now really shows red. Picture boxes (not the patient dashboard's picture) have the light grey border of the other containers, and their "no
picture" text is the grey hint colour. The "Ex: D2140-D2394" example under the procedure code range box is the grey hint colour. In print previews the
area around the pages is a light grey (it was the darker Windows grey). The colour dialog's square and hue bar have the grey input border; the hue
handle is the dark text colour. In the WPF windows the signature box's thin frame is the input border grey (it was a slightly lighter grey). The
error line of the progress window is red again (since UI-4a it showed in the normal text colour). The text strip under the tooth chart (TextBelow
procedures) is in the text colour on white, as the plan's UQ-6 answer says.

**What did not change.** Sizes, positions, fonts and texts (the font change is UI-2b). The pages of a print preview (they are the printout), every
signature: its ink, its picture and whether it is valid; the eForm a patient fills keeps today's signature box frame. The colours themselves that a
colour picker or the colour dialog shows (they are the office's). The code range box, the copyable message window and the job manager's tree are
Windows controls and keep the Windows look. The tooth chart and the perio chart drawings (UQ-6). Printouts.

**For you to look at.** Gallery G-UI1 part 2f (section 6): every state of each control, WinForms above WPF, at 100 to 200%. Four small questions in
section 3 (marked "Owner"): the "no colour" mark, the grey around print preview pages, the amber triangle, and the colour button's border.

### 2. Files

| File | What |
|---|---|
| `CodeBase/UI/UiDialogPaint.cs` | new: the WinForms shapes: the progress bar (`DrawProgress`: `SurfaceRaised` track, `Accent` fill, `TextDisabled` disabled, 1 px `Stroke` outline, 4 px corners, as the sheet's), the colour swatch (`DrawSwatch`, `GetSwatchBorder`: the input outline `StrokeStrong`, `StrokeStrongHover`, `Stroke` disabled, the focus ring; "None" a `SurfaceRaised` box with a diagonal `StrokeStrong` line), the warning's token (`GetWarningFill`: `Warning`, `Danger`); built on UI-4a's `UiControlPaint` |
| `OpenPmsDental/UI/OdProgressBar.cs` | `OnPaint` through `DrawBar` from the tokens (no gradient; `BackColor` and `ForeColor` no longer colour it); the target line in `TargetColor` as before; a `LayoutManager` field for the scale; `GetFraction` |
| `OpenPmsDental/UI/ODColorPicker.cs` | the colour button's `Paint` draws the swatch over the Windows button (`IsFocusRingShown`: focus and keyboard cues; `IsHoverColor`); a `LayoutManager` field; the designer is unchanged |
| `OpenPmsDental/UI/ODPictureBox.cs` | the border 1 px `Stroke` (2 px at 200%, the screen's DPI; the picture starts inside it), the hint `TextSecondary`; office content (`IsContentLook`) keeps today's Gray border, Gray hint and 1 px padding |
| `OpenPmsDental/User Controls/Dashboard/DashPatPicture.cs` | `IsContentLook` true: the dashboard's picture widget keeps today's look |
| `OpenPmsDental/UI/ODCodeRangeFilter.cs` | the example label `TextSecondary` |
| `OpenPmsDental/UI/DatabaseIntegrity/WarningIntegrity.cs` | `SetColor`: fill and outline `Warning` (`Danger` red), the mark `TextOnAccent`; the background is the parent's colour (it was always `ColorOD.Background`); the shape, the blinking and pulsing, the tooltip region as before |
| `OpenPmsDental/UI/ODPrintPreviewControl.cs` | the area around the pages `SubtlePressed` (its `BackColor`); the pages, their frame and shadow are Windows' as before |
| `OpenPmsDental/UI/BordersDpi/LayoutManagerForms.cs` | `PassLayoutManager` gives `ODColorPicker` and `OdProgressBar` the window's scale (two blocks, nothing else) |
| `WpfControlsOD/UI/Progress/ProgressBarFlat.cs` | new: the WPF progress bar in the new look (its template built in code, once per UI thread: track, fill, marquee, outline; triggers for disabled and indeterminate; the marquee animation runs only while the bar is indeterminate, visible and loaded) |
| `WpfControlsOD/UI/Progress/FrmProgressAuto.xaml(.cs)` | the bar is a `ProgressBarFlat` (same size and place); the error label `ColorText` `Danger` (the XAML `Foreground` had no effect since UI-4a) |
| `WpfControlsOD/UI/DatabaseIntegrity/WarningIntegrity.xaml(.cs)` | the literals gone; `SetColor` (`Warning`, `Danger`; the red fill now shows), the mark `TextOnAccent` |
| `WpfControlsOD/UI/Dialogs/FrmColorDialog.xaml(.cs)` | `SetLook`: the square's and the hue bar's frames `StrokeStrong`, the hue handle `Text` |
| `WpfControlsOD/UI/Controls/SignatureBoxWrapper.xaml.cs` | `IContentLook`: the hosted WinForms box's frame (its `BackColor`) `StrokeStrong` and the area behind it `SurfaceRaised`; office content keeps `ControlDark` and white (`ColorFramePinned`) |
| `WpfControlsOD/UI/ToothChart/ToothChartTextBelowStrip.cs` | `Text` on `SurfaceRaised` (UQ-6) |
| `tests/ToothChartImageTests/TextBelowStripTests.cs` | line 88 by UQ-6: the `Text` token (and `SurfaceRaised`) in place of black |
| `tests/UiRefreshTests/DialogStates.cs`, `DialogStateTests.cs`, `DialogGallery.cs` | the tests and the gallery strips (section 5) |
| `tests/UiRefreshTests/Data/colour-literals.tsv` | one literal fewer (`WarningIntegrity.cs`'s light orange) |

Unchanged, and why: `OpenPmsDental/UI/BufferedTreeView` (a Windows tree view: "stock look, font and icons only"; the font is UI-2b, its icons the
UI-9 slices); `OpenPmsDental/UI/MsgBoxCopyPaste` (a subclass of `CodeBase.MsgBoxCopyPaste`, a plain Windows form with Windows buttons and a Windows
text box, which keep the Windows look, section 3.6; its window background is UI-11's); `OpenPmsDental/UI/FormPrintPreview` (its frame is UI-3's, its
toolbar UI-6's, its three toolbar pictures UI-9b's; the area around the pages is `ODPrintPreviewControl`'s); `WpfControlsOD/UI/Progress/ProgressWin.cs`
(it runs `FrmProgressAuto` and draws nothing). The window backgrounds (`#FCFDFE` of the two Frms) are UI-11's.

### 3. Decisions made on the way (say if you disagree)

- **The progress bar** draws exactly the design sheet's (`DrawCalendarAndProgress`); disabled fills `TextDisabled` (the sheet has no disabled bar).
  The WinForms target line (`TargetValue`, `TargetColor`, red by default) is the caller's data colour and stays. `OdProgressBar` has no user in the
  program today; it is kept (the rule "never cut unused features") and restyled.
- **The indeterminate bar** (FrmProgressAuto's default): an `Accent` segment a third of the track wide moves left to right in 1.6 s, again and
  again, clipped to the track's corners. The sheet shows only a determinate bar. Windows' own marquee is a moving green glow; a flat segment follows
  "no gradients, no glows".
- **Owner: the colour button.** A swatch is a value the user sets, so it takes the input outline (`StrokeStrong`, the combo box's) rather than the
  button's `Stroke`; hover `StrokeStrongHover`, disabled `Stroke` (the colour stays), the focus ring (keyboard cues only). The colour fills the box
  to its 4 px corners.
- **Owner: "no colour"** (Transparent, after "None"): a white box with a 1 px `StrokeStrong` diagonal from the bottom left to the top right (the usual
  "none" mark). Before, the button showed the parent's background, which looked like a light grey colour.
- **Owner: the warning triangle** is `Warning` (`#9D5D00`, the notify colour) with the white mark: 5.3:1, where the old light orange gave the white
  mark about 1.6:1. The red one is `Danger`. The outline is the fill's colour at full strength, so it shows while the fill blinks or pulses (as the
  orange outline did). Its WinForms background is the parent's colour (the same as before on the windows that use it, whose background is
  `ColorOD.Background`; right on any other).
- **Owner: the area around print preview pages** is `SubtlePressed` (`#E0E0E0`): white pages stand out from it, and it is lighter and calmer than
  Windows' `AppWorkspace` (`#ABABAB`). The pages, the frame and the shadow Windows draws around them do not change (the control has no paint code of its own; a test checks it).
- **The signature box frame** is the input outline `StrokeStrong` (`#8A8A8A`; it was `ControlDark`, `#A0A0A0`), as section 2.1's "border only". The
  frame is the WinForms box's background showing around its signature area; the signature's picture (`GetSigImage`) leaves it out, so signatures,
  their pictures and their checks are as before (a test signs a synthetic signature and compares). The WPF host gives the frame; the WinForms windows'
  own signature boxes (`OpenPmsDentalBusiness/UI/SignatureBoxWrapper`) are not in this slice and keep `ControlDark`. The eForm fill surface keeps
  `ControlDark` through `ContentLook.IsPinned`.
- **The picture box's border** is a container border (`Stroke`, 1 px; 2 px at 200% from the screen's DPI, as section 3.3's rule; the control has no
  LayoutManager); its "no picture" hint is `TextSecondary` (a hint, section 3.2). Section 2.1 says "border only": the hint is the only other thing it
  draws of its own.
- **The patient dashboard keeps the picture box's look.** The dashboard is office content (section 5.1), so its picture widget
  (`DashPatPicture`) and any `ODPictureBox` inside `UserControlDashboard` or a panel marked with `MarkContent` keep today's Gray border, Gray
  hint and 1 px padding (`ODPictureBox.IsContentLook`, which `DashPatPicture` overrides to true; a test checks all three).
- **The colour dialog:** only its chrome (the two frames, the hue handle). The black and white ring of the picker stays: it must show on any colour.
- **The progress window's error line** takes `Danger` through `ColorText`: its red XAML `Foreground` had been overridden by UI-4a's label look.

### 4. Not done, and why

- Pictures of the real windows with these controls (G-UI3: a progress window, a print preview, the colour dialog, a provider's colour, a window with
  a signature box, the Chart module's TextBelow strip) need shown windows: section 7 has the steps for the lead.
- `TextBelowStripTests` (`tests/ToothChartImageTests`) needs the desktop (`WpfLiveViewTests.RequireDesktop`): changed by UQ-6, it runs at the
  lead's connected session (section 7).
- WPF hover and the live marquee are drawn by WPF from the mouse and the clock: the tests set the states by hand and check the template.

### 5. Tests

All in `tests/UiRefreshTests` (no new test assembly):
- `DialogStateTests` (17 tests): WinForms `OdProgressBar` (rest, empty, full, disabled, the target line) at every scale against the sheet's progress
  bar, every pixel; `ODColorPicker`'s swatch (rest, hover, focus, disabled, None) at every scale, every pixel; its bounds and colour; the warning
  triangle and the red one (WinForms picture, WPF brushes and picture); `ODPictureBox` (border, hint, a picture inside; in the dashboard, a marked panel and `DashPatPicture` today's look); `ODCodeRangeFilter`'s hint;
  the print preview (its area `SubtlePressed`, and the control draws nothing of its own, so the pages are Windows' drawing as before; a page is not
  previewed in the test, because a document prints into the preview behind a Windows status window, which these tests never show); the signature box (a synthetic
  signature valid in both looks, its picture the same pixel for pixel, the frame `StrokeStrong`, office content `ControlDark`); the TextBelow strip
  (UQ-6); WPF `ProgressBarFlat` (rest, empty, full, disabled, indeterminate) against the sheet, solid pixels; its template and marquee;
  `FrmProgressAuto` (the flat bar, its size, the error in `Danger`); `FrmColorDialog`'s frames.
- `DialogGallery.Gallery_DialogStrips_WinFormsAboveWpf`: opt-in, the strips of section 6.
- The layout snapshot and the bounds record (`LayoutSnapshotTests`, `Scan_TodaysFont_ReportsNothingNew`): unchanged and green; no designer file
  changed.
- NoHqTests: the colour literal scan (one row fewer), the coverage table (no new row: `CodeBase/UI` is outside it and `ProgressBarFlat` is under
  `WpfControlsOD/UI/Progress/*`), the pen and brush rule (no `using` around a `UiTheme` brush).

### 6. Gallery G-UI1 part 2f

`.git/od-tools/test-results/ui4c2-G-UI1-part2f/` (`index.html` and the zip beside it; the owner's summary is the note at the top of the page), made
from the strips in `.git/od-tools/test-results/ui4c2-G-UI1-part2f-strips/` (each of `scale-100` to `scale-200`): `progressbars.png` (WinForms
`OdProgressBar` in the top row: rest, empty, full, disabled, the target line; WPF `ProgressBarFlat` below: rest, empty, full, disabled,
indeterminate), `colorpicker.png` (WinForms `ODColorPicker` with its None button: rest, hover, focus, disabled, None), `warning.png` (the triangle and
the red one, WinForms above WPF); at 100% only, because they draw at the screen's DPI: `picturebox.png` (the hint, a synthetic picture with and
without the border) and `signaturebox.png` (a synthetic signature in the new frame, then in office content's frame). "Only after", as UI-4a's.

### 7. For the lead

- The strips render off screen and were made in the disconnected session. To make them again:

      set OPMS_UI_DIALOG_GALLERY=<od-tools>\test-results\ui4c2-G-UI1-part2f-strips
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out <od-tools>\test-results\ui4c2-strips.txt
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -After <od-tools>\test-results\ui4c2-G-UI1-part2f-strips -Out <od-tools>\test-results\ui4c2-G-UI1-part2f -Name G-UI1 -Note "<the summary of section 1>"

- `TextBelowStripTests` (UQ-6) at the connected desktop, after a full Debug x64 lane build:

      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include ToothChartImageTests -Out <od-tools>\test-results\ui4c2-toothchart.txt

  (the strip's text is the `Text` token and its background `SurfaceRaised`; the 1,682 + 18 tooth chart baselines must stay identical.)
- The real windows (G-UI3) need shown windows: at the owner's connected desktop, after a full Debug x64 lane build, group U against the before set
  (docs/port-notes/UI-0c.md section 5), as for UI-4a and UI-4b:

      set OPMS_WINDOW_CHECKS=1
      set OPMS_WINDOW_CHECKS_DUMP=<od-tools>\winchk-gate3a\opendental_dev.sql
      set OPMS_WINDOW_CHECKS_GROUPS=U
      set OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui4c2-groupU
      set OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18
      set OPMS_WINDOW_CHECKS_UI_BEFORE=<od-tools>\test-results\ui-before\run1
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <od-tools>\test-results\ui-before\run1 -After <od-tools>\test-results\ui4c2-groupU -Name G-UI3 -ClipNotes <od-tools>\test-results\ui4c2-groupU\ui-walker\clip-notes.tsv

  (Split into `UW`, then `UM,UD` if one run does not fit the runner's 20 minutes; the before folders `run1` to `run5` and `fix1` hold the groups as
  UI-0c froze them.) U0 shows as changed the windows with a colour picker (`FormProvEdit`, `FormCarrierEdit`), a picture box, a code range filter, a
  print preview, a WPF signature box (`FrmCommItem`, `FrmDocSign`, `FrmChartLetterProperties`) and the WPF progress window; the bounds record must
  show no difference. Worth a live look there: a progress window with its moving segment (any long action, a synthetic report), a print preview
  of a synthetic report, the colour dialog, a colour button under the mouse and after Tab, a WPF signature box with a synthetic signature, and an eForm
  with a signature field in `FrmEFormFillEdit` (its frame unchanged).
- The owner's answers to the four questions of section 3.

### 8. Runs

Full filter build x64 through the lane: 0 errors. Targeted run (x64, after the full build, with `OPMS_UI_DIALOG_GALLERY` and `OPMS_UI_SCAN=1`;
`.git/od-tools/test-results/ui4c2-final.txt`): UiRefreshTests 186 (180 passed, 6 opt-ins skipped: the six other galleries; this package's gallery
and the scan ran), WindowChecksTests 72 (29 passed, 43 opt-in skipped), UnitTests 1,731 (1,728 passed, 3 skipped), PrintGuardTests 17 (16
passed, 1 skipped), NoHqTests 122/122. WPF rendered in the disconnected session, so the WPF rows of the strips and the WPF picture tests ran. The
scan rerun (into `.git/od-tools/test-results/ui4c2-scan`): Segoe UI 9 makes 1,775 controls overflow in 450 windows, the same counts as UI-4b-3's
and UI-6's reruns (every summary line the same but the timing: this package changes no text rectangle); today's font reports nothing new.
`tools/Test-NoEolFlips.ps1 -Base main`: no file changed its line ends.

Review fix (ui/ui4c2-2, the dashboard picture keeps today's look): full filter build x64 through the lane, 0 errors; Targeted run
(`.git/od-tools/test-results/ui4c2-2-fix.txt`): UiRefreshTests 187 (179 passed, 8 opt-ins skipped, the new
`WinForms_PictureBox_InOfficeContent_KeepsTodaysLook` passed), WindowChecksTests 72 (29 passed, 43 opt-in skipped), UnitTests 1,731 (1,728
passed, 3 skipped), PrintGuardTests 17 (16 passed, 1 skipped), NoHqTests 122/122. No line ends changed.
