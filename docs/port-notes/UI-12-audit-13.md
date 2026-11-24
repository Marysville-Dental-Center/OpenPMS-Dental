# UI-12 slice 13: audit of the gallery

Slice 13 (`ui/ui12-13-1`, [UI-12.md](UI-12.md) section 7, "Slice 13"): the windows earlier slices left because the tools could not
reach them (seven the harness now builds with sample data; the report designer's dialogs and controls and ODR's parameter window, whose
designer code is in their `.cs` files). Every after picture opened at 100% (the program's font, Segoe UI 9), the densest at 150%. One line
per picture: what was checked (cut or covered text, overlaps, margins, columns, gaps, crowding, empty bands, control looks, group titles)
and what was found. `<W>` = `.git/od-tools/test-results/G-UI3-ui12-13-src/work`.

## Pass 1 (the planner's layout, `<W>/after-100`)

| Picture | Status | What was checked and found |
|---|---|---|
| FormEhrCarePlans | fixed | Grid at main's x 12, 12 px short of the right padding. |
| FormEtrans835ClaimEdit | fixed | Right column's boxes at three left edges (626, 626, 636); grids 8, 4, 4 and 12 px apart; left labels 36 px in. |
| FormEtrans835ProcEdit | fixed | Same as ClaimEdit: right boxes 90 and 88 wide, grids 8, 4 and 12 px apart. |
| FormMountDefEdit | fixed | Width and Height (window controls over panelRight) over the colour rows; the column widened to 286 px, the combo past it; Flip on Acquire on the field column; window grown to 1327 px. |
| FormRecallList | fixed | Group Families, Show Conflicting Types and Show Do Not Contact wrapped onto three lines in 99 px (cut); Include Reminded alone on a fourth row; bottom buttons 6 px apart on three edges per column. |
| FormSchedule | fixed | Grid 28 px short of the right padding and 162 px above the bar; the holiday check boxes on two lines each. |
| FormSheetDefEdit | fixed | Window 1208 x 1130; the Show check boxes parted from their Add Field buttons; Tab Order over the internal-sheet note. |
| ChartAxisCtl | fixed | Control 525 px wide (the property page is 450); grid line groups' Color and Width labels over their boxes; Interlaced alone. |
| ComboBoxMulti | back to main | The planner padded the text box and drop button inside the 120 x 21 composite input (not a container). |
| DialogDatabase, DialogNewChart, DialogNewTable, DialogToolOptions, FindTab, FormParameterInput | re-planned | Laid out in the window's outer size (frame included): controls past the client area's bottom (the plain-Form rule added, re-planned). |
| InteractivityCtl | fixed | Each box 6 px after its own radio button (three left edges). |
| PositionCtl | fixed | Groups at main's x 24, 64 px of empty room on their right; Hide Duplicates' label over its box; XML Element Style cut. |
| ReportCtl | fixed | Control 556 x 311 (Page Footer past the page). |
| ReportParameterCtl | fixed | No fixed point: 2936 px wide. |
| StyleBorderCtl | fixed | Color and Width captions over the fx buttons, not over their columns. |
| StyleCtl | fixed | Padding labels 2 px in; right boxes at two edges; XML labels 7-11 px in. |
| StyleTextCtl | fixed | Right band at three left edges; the font dialog's "..." past the Font group's padding. |

## Pass 2 (the layout scripts, `<W>/f1-100`, `<W>/f2-100`)

| Picture | Status | What was checked and found |
|---|---|---|
| FormEhrCarePlans | ok | Grid on the padding, down to the section gap over Add. |
| FormEtrans835ClaimEdit | ok | Two label columns, one box column each, grids 8 px apart, section gap over Print. GridRulesTests' column finding is the Balancing group's operators (grid-known). |
| FormEtrans835ProcEdit | ok | As ClaimEdit. |
| FormMountDefEdit | fixed | Measurement Scale's boxes 4 px off the column's field column (moved onto it). |
| FormRecallList | fixed | The window 797 px tall from the planner (back to main's 696). |
| FormSchedule | ok | 230 px column, every check box on one line, grid to the padding and the column's bottom. |
| FormSheetDefEdit | ok | Pairs on one row, kinds 8 px apart, hidden Special and ScreenChart keep their rows; the note and Tab Order stacked (grid-known). |
| ChartAxisCtl | ok | 440 px, two bands, one row per grid line group, check boxes on a 2 x 3 grid. |
| DialogDatabase | ok | Each page from the content edge; OK and Cancel on the docked panel's padding. |
| DialogNewChart | ok | Lists on one grid, buttons beside them, caption over the expression box. |
| DialogNewTable | fixed | Down (compress) past the group's padding (one row of three radio buttons is 43 px too wide: Across plus a list). |
| DialogToolOptions | fixed | Window at 535 x 317 (the planned tree's root taken for a framed window: `write_tree` fixed). |
| FindTab | fixed | Client 3 x 2 px larger than main for nothing (pinned to main's 448 x 206). |
| FormParameterInput | ok | Panel inside the padding, section gap over OK and Cancel. |
| InteractivityCtl | fixed | "Parameters..." cut (button sized for its text). |
| PositionCtl | fixed | Hide Duplicates' box 31 px from Span Table Columns' column (put on it). |
| ReportCtl | fixed | The groups' left boxes 24 px from Width's column (one column). |
| ReportParameterCtl | fixed | Control still 2936 wide in the script's first version; Remove cut; Allow blank past the page (from the label column). |
| StyleBorderCtl | ok | Captions over their columns. |
| StyleCtl | fixed | Groups to 464 px, past the 450 px page (to 448). |
| StyleTextCtl | ok | One column per band, the Font group's rows on the window's columns. |

## Pass 3 (`<W>/f3-100`, every picture)

All 31 pictures opened (the 22 windows and the tab pages: FormRecallList Reminders and Reactivations, FindTab Replace and GoTo,
DialogToolOptions Toolbar and Desktop Server, DialogDatabase Connection, Parameters and Grouping). One finding: DialogToolOptions' Desktop
Server note cut on its last line (the stock label wraps a line more than GDI+ measures): a line taller, the warning over the width.

## Pass 4 (`<W>/f4-100`, the two changed pictures by byte compare)

| Picture | Status | What was checked and found |
|---|---|---|
| DialogToolOptions-desktop | ok | Both notes whole, Port, Directory and Restrict access on the label column. |
| DialogDatabase-connection | ok | The label column holds "Shared Data Source File:" (run-time text). |

Nothing found: the pass is clean.

## 150% (`<W>/f4-150`)

FormRecallList, FormSheetDefEdit, DialogDatabase-parameters, ReportParameterCtl, StyleTextCtl opened. The program windows scale as at
100%. The report designer's plain controls are drawn with their fonts scaled only (no LayoutManager in a plain exe), so their labels cut at
150% as on main (`<W>/before-150`, cut worse there); the exe's own DPI handling is outside this slice.
