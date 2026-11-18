# Port note: UI-5 (grids)

Package UI-5 of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-5 row, sections 3.2, 3.3 and 3.6,
the owner's answers UQ-2 and UQ-5, and the approved design sheet G-UI1 part 1), on `ui/ui5-1` from `main` at `e4118a588`. It changes the look
of the grids on screen and nothing else: no bounds, no layout, no printout. No schema change, no new package, nothing downloaded, no database
used, no window shown.

## 1. For the owner, in plain words

**What changed.** Every grid (the lists with a blue title and column headers, in every module and window) is drawn in the new look:

- The title is a flat blue bar (the program's blue) with white text; it was a blue gradient.
- The column headers are light grey with thin grey lines between them; they were light blue with a dark and a white line.
- Rows are white with light grey lines. The area below the last row is white too (the Chart module's progress notes had grey there).
- The row under the mouse is pale blue-grey; the selected row is light blue with a 2 px bar at its left edge: blue while the grid has focus,
  dark grey (your choice on the design sheet) when it does not.
- The row the arrow keys are on gets a thin blue outline, only after the keyboard was used (a mouse click draws none), as Windows does.
- The outer border is light grey instead of blue. Buttons inside grid cells are flat and white with a thin grey border.
- The title and header heights follow the font. With today's font they are exactly as before. When Segoe UI 9 arrives (UI-2b), rows are
  17 px tall instead of 14 (your answer to UQ-2), headers 18 instead of 15 and titles 20 instead of 18, all inside the grid's own size, so
  about 20% fewer rows show at once.

**What did not change.** Grid sizes and positions; today's fonts (Segoe UI comes with UI-2b); colours the program or the office gives a row,
a cell or a selection (appointment and procedure colours, the Family and Account modules' salmon selection); what the grids do. Printouts,
PDFs and grids on sheets keep today's look (UQ-5): checked identical before and after.

**For you to look at.** The grid part of gallery G-UI1 part 2 (section 5): the grid at 100 to 200%, WinForms above WPF, with and without focus.
And the grid strip for G-UI2: today's font beside Segoe UI 9.

## 2. Files

| File | What |
|---|---|
| `OpenPmsDentalBusiness/UI/GridScreenLook.cs` (new) | the WinForms grid look from the tokens, shared by GridOD and GridOld: title, add button, header, separators, sort arrow, button cells, selection fill, hover blend, text colour, selection bar, focused row, outer border; the title, header and editable row heights from the font tokens |
| `OpenPmsDentalBusiness/UI/GridOD.cs` | the screen drawing through GridScreenLook; the static screen colours, pens, brushes and the title gradient removed (the print code already used `GridPrintLook`); `_heightTitlePrint` (18) for the sheet title and the sheet page breaks; `HeightEditableRow()` (print keeps 19); the focused row (`_indexRowFocused`: the click, the arrows, reset by `EndUpdate`); repaint on focus changes; `IsSelectionFocused` and `IsFocusedRowShown` (protected virtual, for the tests) |
| `OpenPmsDentalBusiness/UI/GridOld.cs` | the same for GridOld (still used: `ControlChart.gridProg`, the progress notes). Its static colours stay for its printouts and sheets (they used them); the five only the screen used are gone. No hover (it never had one) |
| `WpfControlsOD/UI/Controls/Supplemental/GridLook.cs` (new) | the WPF grid look: token brushes for XAML, the selection colour, the see-through hover and selection layers, the bar heights from the font, the bar and line widths at a DPI scale |
| `WpfControlsOD/UI/Controls/Grid.xaml(.cs)` | the new look in the XAML (token brushes, Semibold title, no gradient); one Stroke separator per header column; the sort arrow, text and last row line in the tokens; hover RowHover; selection AccentSubtle; the selection bars and the focused row's outline (`DrawSelectionMarks`, redrawn with the selection and on keyboard focus changes); title and header heights from the font; `SetFocusLookForTest` (test only, refuses outside a test process) |
| `WpfControlsOD/Themes/DataGridDict.xaml` | the same colours (no window uses this style today) |
| `tests/UiRefreshTests/GridLookTests.cs` (new) | the tests and the gallery strips (section 4) |
| `tests/AttributionTests/BrandingTests.cs` | `BrandColor_IsTheProgramsGridTitleBlue` on the token (section 4) |
| `NoHqTests/UiThemeScanTests.cs`, `tests/UiRefreshTests/Data/colour-literals.tsv` | the colour scan reads `GridScreenLook.cs` too; the allow list is 17 rows shorter (22 rows out, 5 in): the screen literals of GridOD, Grid.xaml and Grid.xaml.cs are gone; GridOD's `102,102,122` drops from 4 to 2 (the print code keeps 2); new are the computed hover and overlay colours, the selection blend under its new variable name and the stock selection colour moved to GridLook |

## 3. Decisions made on the way (say if you disagree)

- **Heights follow the font by a fixed margin.** WinForms: the title is the title font's line height at 96 DPI plus 2, the header the bold
  grid font's plus 2, an editable row the cell font's plus 6 (never below 19), a plain row the cell font's height plus 1 as before. With
  today's fonts that is 18, 15, 19 and 14, exactly as before; with UI-2b's 20, 18, 22 and 17. WPF: the title and the header are their
  font's line height less 1 (18 and 15 DIP, as before; the WPF fonts do not change at UI-2b), rows were already 17 DIP. A multiline header
  still grows from its base height. The grid's own size never changes; the rows area gets what is left.
- **Printouts keep 18 px titles and 19 px editable rows**: GridOD and GridOld keep a print title height of their own, and the editable row
  height of a printout or sheet is today's, so a font change can never move a printed row.
- **A colour the program sets wins; the stock colours become tokens.** A grid that keeps the stock selection colour (GridOD's 205,220,235,
  GridOld's Silver, the WPF Grid's 205,220,235) draws AccentSubtle; one a window sets keeps it (`DarkSalmon` in the Family and Account
  modules, the report setup's see-through grey). White rows draw SurfaceRaised and black text draws Text; any other row, cell or text colour
  is drawn as set. The hover darkens a coloured row by as much as RowHover is darker than white, as GridOD's hover did; the WPF hover is a
  see-through layer that shows exactly RowHover over white.
- **The selection bar and the focused row are drawn last**, inside the row (after the border, from its top gridline to its bottom one), so a
  cell colour or a picture in the first column does not hide them. The test checks that no cell text starts under the bar. A picture in a
  first column (an `ImageList` column) starts at the column's edge today, so the bar covers its first 2 px when the row is selected.
- **The WinForms bar stays at the visible left edge** when a grid is scrolled sideways (the row fill starts there); the WPF bar is the row's
  own first 2 DIP and scrolls with it.
- **Grid lines stay 1 device pixel at every scale** (the cells are laid out one pixel after each line); the outer border and the selection bar
  follow the snapping rule of 3.3 (border 2 px and bar 4 px at 200%, checked by a test).
- **OneCell grids** (a selected cell, not a row) get the selection colour on the cell and no bar; grids with no selection get neither.
- **The focused row**: the row the last click or arrow key moved to, or the first selected row. WinForms shows its outline while the grid has
  focus and `ShowFocusCues`; WPF while the grid has keyboard focus and the keyboard was used last (or Windows always shows keyboard cues). The
  arrows that reach a grid through its form without focus (`ArrowsWhenNoFocus`) move the selection as before and draw the unfocused bar.
- **India.** GridOD and GridOld switched their screen fonts to Arial for an Indian culture; the plan's likely reason, the rupee sign, is drawn
  by Segoe UI, Segoe UI Semibold and Microsoft Sans Serif on Windows 10 and 11 (a test checks the glyph), so the screen switch is gone. The
  print fonts keep their switch, so printouts in India keep their look.
- **The sort arrow** keeps its shape in Text, without the white halo it had for the blue header. **The corner between the scroll bars** keeps
  the system's control colour (it matches the stock scroll bars), and so does the WPF corner (`#F0F0F0`).
- **WPF test hook.** A XAML UserControl cannot be subclassed from another assembly (its `InitializeComponent` refuses), so the WPF Grid has a
  test-only `SetFocusLookForTest` (focus, focused row, DPI scale), which throws outside a test process like `UiTheme.SetFontForTest`.
- **Left as they are:** `FormWikiListItemEdit` adds 15 + 18 for a grid's header and title when it sizes itself and `FormSheetFieldGrid` adds
  18 and 15 to a sheet grid field; both are today's values, which UI-5 keeps with today's font. With UI-2b's font the wiki window's sum is 5 px
  short (the grid then shows a little less); UI-10 or UI-2b can take the heights from `GridScreenLook`.

## 4. Tests

- `GridLookTests` in `tests/UiRefreshTests` (9 tests, 1 of them the opt-in gallery):
  - `WinForms_GridOD_TodaysFont_EveryState_InTheTokens`: GridOD off screen with and without focus; the border, title (18 px, white text),
    header (15 px with its line, separators, Text), six rows of 14 px (plain, selected, program-coloured, hovered), the gridlines, the last
    row's Stroke line, the bar (Accent or SelectionBarNoFocus at x 1 and 2), the focused row's outline, the area below the rows, no text under
    the bar; the bounds are unchanged.
  - `WinForms_GridOD_Ui2bFont_HeightsFollowTheFont`: the same with UI-2b's fonts (the token override): title 20, header 18, rows 17.
  - `WinForms_GridOD_ColorsAWindowSets_Stay`: a window's DarkSalmon selection stays (with the bar on it); the colour helpers.
  - `WinForms_GridOD_ScaledBarAndBorder_FollowTheSnappingRule`: border 1, 1, 1, 1, 2 px and bar 2, 2, 3, 4, 4 px at 100 to 200%.
  - `WinForms_GridOld_TodaysFont_TitleHeaderSelection_InTheTokens`: GridOld as GridOD, without hover.
  - `Wpf_Grid_EveryState_InTheTokens`: the WPF Grid rendered off screen (WPF's software renderer; it draws in the disconnected session), with
    and without focus: border, title 18, header 15, rows 17, hover, selection, bar, outline, the coloured row.
  - `Wpf_Grid_HeightsFollowTheFont`: the bar heights, the bar width at 150%, the hover layer over white, the stock and a window's selection.
  - `GridFonts_DrawTheRupeeSign_SoTheIndiaArialSwitchIsGone`.
  - `Gallery_GridStrips_WinFormsBesideWpf` (opt-in, `OPMS_UI_GRID_GALLERY`).
- `BrandingTests.BrandColor_IsTheProgramsGridTitleBlue`: the brand blue is the Accent token; GridScreenLook fills the title with it, GridOD and
  GridOld draw their title through it, the WPF Grid's title is `GridLook.BrushTitle`; the printed title's gradient keeps the blue as its bottom
  (`GridPrintLook.ColorTitleBottom`); the branding files as before.
- **The print guard** (`tests/PrintGuardTests`, unchanged): passes. Its outputs recorded with `PRINT_GUARD_RECORD_DIR` on `main` (`e4118a588`,
  `.git/od-tools/test-results/ui5-print-before`) and on this branch (`ui5-print-after`) have the same SHA-256 for all 19 grid, sheet, perio,
  MigraDoc, report and dashboard outputs (GridOD print page, at zoom 120, sheet bitmap, sheet PDF, sheet canvas; GridOld print page, sheet
  bitmap and PDF; the sheet grids, the sheet PDF and printed page, the perio chart and its claim attachment, MigraDoc, the complex report,
  the graph dashboard page). Not comparable across runs: the nine claim forms differ between two runs of the same build (`ui5-print-after`
  and `ui5-print-after2`), and the RDL PDF differs between the two builds although `RdlEngine` references none of the projects UI-5 changes;
  within each run the guard's own check (today's fonts against UI-2b's, and repeatability) passes for all of them.
- **The scan rerun** (`OPMS_UI_SCAN=1`, into `.git/od-tools/test-results/ui5-scan`): Segoe UI 9 makes 1,775 controls overflow in 450
  windows, 8.25 makes 637 in 245, as UI-4a measured (UI-5 changes no window's controls); `Scan_TodaysFont_ReportsNothingNew` passes (no new
  overflow, no bounds difference); the layout snapshot is unchanged (no designer file changed).
- Targeted runs (x64, after the full build): UiRefreshTests 122 (121 passed, the UI-4a opt-in gallery skipped), PrintGuardTests 17 (16
  passed, 1 skipped as on `main`), UnitTests 1,731 (1,728 passed, 3 skipped), WindowChecksTests 71 (29 passed, 42 opt-in skipped),
  AttributionTests 54/54, NoHqTests 122/122. Full filter build x64: 0 errors.

## 5. Galleries

- **G-UI1 part 2, the grids:** `.git/od-tools/test-results/ui5-G-UI1-part2-grid/` (`index.html` and the zip; the owner's summary is the note
  at the top of the page), made from `.git/od-tools/test-results/ui5-grid-strips/g-ui1-part2/scale-100` to `scale-200` (`grid.png`: the
  WinForms GridOD above the WPF Grid, each with focus and the focused row's outline on the left and without focus on the right; the hovered
  row, the selected row with its bar and the orange program-coloured row in each). WinForms scales through the grid's zoom (LayoutManager's
  path for its fonts and sizes), WPF through its render scale. "Only after", as UI-4a's strips: the before look of the real windows is the
  before set (`ui-before`) for G-UI3.
- **G-UI2, the grid strip:** `.git/od-tools/test-results/ui5-G-UI2-grid/` from `ui5-grid-strips/g-ui2/grid-today-and-ui2b.png`: the WinForms
  grid with today's font beside the same grid with UI-2b's Segoe UI 9 (rows 17 px), and the WPF grid below.

## 6. For the lead

- The strips render off screen and were made in the disconnected session (WinForms and WPF both draw). To make them again (after a full x64
  lane build):

      set OPMS_UI_GRID_GALLERY=<od-tools>\test-results\ui5-grid-strips
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out <od-tools>\test-results\ui5-strips.txt
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -After <od-tools>\test-results\ui5-grid-strips\g-ui1-part2 -Out <od-tools>\test-results\ui5-G-UI1-part2-grid -Name G-UI1 -Note "<the summary of section 1>"
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -After <od-tools>\test-results\ui5-grid-strips\g-ui2 -Out <od-tools>\test-results\ui5-G-UI2-grid -Name G-UI2 -Note "<the G-UI2 line of section 5>"

- The print record: `set PRINT_GUARD_RECORD_DIR=<folder>` and a Targeted run of PrintGuardTests writes each output's SHA-256 there; compare
  the folder with `.git/od-tools/test-results/ui5-print-before` (main `e4118a588`).
- The real windows and modules with the new grids need shown windows: at the owner's connected desktop, after a full Debug x64 lane build, group
  U against the before set (docs/port-notes/UI-0c.md section 5), for G-UI2 and G-UI3:

      set OPMS_WINDOW_CHECKS=1
      set OPMS_WINDOW_CHECKS_DUMP=<od-tools>\winchk-gate3a\opendental_dev.sql
      set OPMS_WINDOW_CHECKS_GROUPS=U
      set OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui5-groupU
      set OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18
      set OPMS_WINDOW_CHECKS_UI_BEFORE=<od-tools>\test-results\ui-before\run1
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <od-tools>\test-results\ui-before\run1 -After <od-tools>\test-results\ui5-groupU -Name G-UI3 -ClipNotes <od-tools>\test-results\ui5-groupU\ui-walker\clip-notes.tsv

  (Split into `UW`, then `UM,UD` if one run does not fit the runner's 20 minutes; the before folders `run1` to `run5` and `fix1` hold the
  groups as UI-0c froze them.) U0 shows every window with a grid as changed; the bounds record must show no difference. G-UI2's main window
  and modules (with and without a patient, the patient dashboard's appointment widget grid) come from the same connected run.
- A shown WPF Frm with a grid shows the live focus behaviour: the bar turns blue when the grid takes keyboard focus, and the focused row's
  outline appears after an arrow key, not after a click.
