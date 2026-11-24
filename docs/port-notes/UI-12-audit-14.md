# UI-12 slice 14: audit of the gallery

Slice 14 (`ui/ui12-14-1`, [UI-12.md](UI-12.md) section 7, "Slice 14"): the main program's and the separate programs' windows laid out by
their code, which the final sweep listed as off the grid. Every after picture opened at 100% (the program's font, Segoe UI 9), the densest
at 150%; plain windows whole, with the title bar. One line per picture: what was checked (cut or covered text, overlaps, margins, columns,
gaps, empty bands, the states the code shows) and what was found. Pictures opened at each pass; a later pass opened the changed ones
only. `<W>` = `.git/od-tools/test-results/G-UI3-ui12-14-src/work`.

## Pass 1 (`<W>/a1-100`, the first layout: code on UiGrid, the planner's rows, the first scripts)

| Picture | Status | What was checked and found |
|---|---|---|
| FormTasks, FormTasks-date | good | Filter row 4 px under the toolbar on the padding, tabs, calendar (date tabs only), tree and grid 4 px apart and 8 px off the edges, the grid to the bottom padding; main's empty lower pane gone (Load collapses it). |
| UserControlTasks | fixed | The tree and grid over the calendar and the calendar shown on For User: hosted at its own size it got no Resize (harness: `AfterHosting`). |
| ControlFamily, -superclones | good | Picture and the top grids 4 px under the toolbar, the parts 4 px apart; the insurance grid beside the pane or in its place. The insurance grid 1 px right of the Recall grid with the pane shown (the pane's width is the code's, from its columns). |
| FormSheetFieldCheckBox, -radio, -allergy | fixed | Rules: the main column's fields at 446, the groups' at 450 (one column within 40 px). Each state's controls in the one slot; Required's note on two lines. |
| ServiceManager-FormMain | fixed | The planner kept main's 23 px margins and the note's last line under the list. |
| FormUpdate-versions | good | The four groups 8 px apart under the message, captions, boxes, buttons on the right padding, Beta's check box beside its buttons. |
| FormQuery | good | The split on the padding, the query box and the button column top-aligned, the Format group beside them, the Title row 4 px under the splitter, the bar on the padding. |
| FormCameraSettings | good | Field rows 27 px, check boxes 18 px on the field column, each section 12 px under the one before, the bar 12 px under the last note. |
| FormDbSettings | good | One label column, the fields to the right padding, the credential choices with their rows under the text, the bar on the bottom padding. |
| FormWebConfigSettings | good | Groups on the padding, one field column, the one-way check box on it, the bar 12 px under the master group. |
| FormServiceManage | good | Rows on one field column; 19 px of main's 166 px window left under the buttons (a window never shrinks). |
| FormInstallAllocator_Provider | fixed | Left column 9 to 12 px in, the report panel off the right padding, an empty band under the choices. |
| FormProgressExtended-bars | fixed | Log panel parts 9 to 12 px in; the window taller than its content (an empty band under the buttons). |
| MsgBoxCopyPaste@CodeBase | fixed | Text 12 px in, Copy All 126 px in, buttons 26 and 28 px tall. |
| FormImageEnhance | fixed | Rules: the two button rows read as content over the bar (8 px, not 12). |
| UpdateFileCopier-FormMain | fixed | Label 12 px in. |
| FormProgressStatus, ODProgressBar | fixed | The planner's padding moved the bar and the message down and the control grew; the name 3 px off the bar's centre. |

## Pass 2 (`<W>/a2-100`, the scripts of pass 1's findings)

| Picture | Status | What was checked and found |
|---|---|---|
| ServiceManager-FormMain | fixed | Note 337 x 130: GridRulesTests' text rule asks 5 px more. |
| FormProgressStatus, -history | good | Minimize box in the corner on the padding, the bar on the bottom padding, the message 2 px over it; history: the box over the window, the bar under it. |
| ODProgressBar | good | Name, bar and percent on one row centred, the title 2 px over the bar. |
| FormProgressExtended-bars | fixed | Still 97 px of empty band under the buttons with no bar (the designer height; AutoSize only grows). |
| FormSecretPrompt, UpdateFileCopier-FormMain, FormToothPicker | good | Padding, caption gap, bar a section gap under the last row. |
| FormInstallAllocator_Provider | good | Welcome, text to the bottom row, report panel on the right padding at the text's top, choices and Run Allocator Tool on the bottom padding. |
| FormUpdate, -classic | fixed | Default: the window empty under the message (the fork's only state). Classic: the panel's rows on one column under the message. |
| FormImageEnhance | good | The two button rows one bar, All in mount over Close, 12 px under the note. |
| MsgBoxCopyPaste, @CodeBase, @UpdateFileCopier | good | Text on the padding, the bar 12 px under it, the three buttons 26 px. |
| FormSheetFieldCheckBox, -misc, -single | good | One field column for the column and the groups; the misc note over Reportable Name; Tab Order on the bottom row 12 px over the bar. |
| FormWebConfigSettings-service | good | The window shorter by the hidden rows; the bar 12 px under the PEM row. |
| FormWikiEdit | good | The halves on the padding under the toolbars, 8 px apart (the preview is WebView2, not drawn off screen, as on main). |
| FormOpenDental | good | Split container from the module bar's edge and the toolbar, the splitter at 540, the signal grid at 442 (seven buttons + 12). |
| FormReportCustom, FormReportForRdl | good | The viewer under the toolbar over the window (a page surface), as the code puts it; the same picture as main. |
| FormQuery-report, -split | good | Report: the preview on the padding over the bar, the page buttons on the bar. Split at 320: both panes whole. |
| SplitContainer | good | The library control alone shows its design hint (no panels); the same picture as main. |

## Pass 3 (`<W>/a3-100` and `<W>/a4-100`, the changed pictures)

| Picture | Status | What was checked and found |
|---|---|---|
| ServiceManager-FormMain | fixed, then good | a3: the ninth line "(double click to manage)" under the list (GDI+ measures one line fewer than the stock label draws); a4: nine lines, the list 4 px under them, Add on the bottom padding. |
| FormProgressExtended, -bars | fixed, then good | a3: the window still 300 px tall (the flow panel's designer height); a4: 482 x 173, the log panel on the padding; with the bars the window grows (AutoSize). |
| FormUpdate-load | fixed, then good | a3: the state failed (the client panel shortened twice); a4: the message, the window 8 px under it. |
| UserControlTasks, -date | good | As FormTasks: For User with no calendar, By Date with it. |
| ControlFamily | good | The insurance grid in the pane's place. |
| UserControlSecurityUserGroup | good | Built (sample data); the same picture as FormSecurity's User Groups page of slice 3. |

## At 150% (`<W>/a4-150`)

| Picture | Status | What was checked and found |
|---|---|---|
| FormSheetFieldCheckBox, -allergy | good | Columns and rows scale together; Required's note on two lines; no cut text. |
| FormCameraSettings | good (note) | Layout scales with the window; the combo boxes' texts draw in the 100% font in the harness (the code-built combo boxes' fonts are not passed through the harness's font pass; the layout is not affected). |
| FormWebConfigSettings, ServiceManager-FormMain | good | Plain windows scaled as Windows scales them: the note's nine lines, one field column. |
| UserControlTasks, FormQuery, FormUpdate | good | The filter row, the tabs and the grid; the panes and the bar; the version line and the message. |

Before pictures: `<W>/before-100` and `<W>/before-100s` (the branch point's product with this slice's harness; the states' befores from
the same build), `<W>/before-150`. Five pictures have no before: FormProgressStatus-history, FormUpdate-load, FormWebConfigSettings-service
(states of code this slice wrote) and, at 150%, FormUpdate and ServiceManager's FormMain. Gallery `G-UI3-ui12-14`: 52 pictures, 42
changed, 5 the same (FormReportCustom and FormReportForRdl: the code already placed the viewer; FormToothPicker: its numbers were the
grid's, now named; SplitContainer; UserControlSecurityUserGroup: laid out by slice 3), 5 only after.

## Fix round (`ui/ui12-14-2`; `<W>/f2-100` from the final build, `<W>/f1-150`; the review's nine findings)

| Picture | Status | What was checked and found |
|---|---|---|
| UserControlTasks-date, FormTasks-date | fixed, then good | The tree had covered the calendar's Today line in UserControlTasks-date (y 240): the stock calendar sized itself after the tree was placed. Now both pictures alike: the calendar ends at y 254, the tree's top 4 px under it (259). |
| UserControlTasks, FormTasks | good | The tree and grid on the padding (x 8 to the right padding), as the tab strip. |
| FormSheetFieldCheckBox-single | fixed, then good | The mobile item caption on Required's row, 12 px under Width / Height (main's slot under the hidden Required left 58 px empty). |
| FormSheetFieldCheckBox, -radio, -misc, -allergy, -problem | fixed, then good | Required's note and Yes / No's note on one left edge (x 532); Required's note on two even lines ("Same for the" / "whole group"): it needs 148 px, 135 are left at that edge. |
| FormProgressExtended, -bars | fixed, then good | The window a padding under Pause and Cancel with and without the bars (the 22 px band was AutoSize sizing the framed window for Windows' title bar); the buttons 12 px under the log box (the hidden warning's row only while it shows). |
| FormProgressStatus, -history | fixed, then good | The bar on the padding under the history box (x 8 to 520, as the box). |
| FormServiceManage | fixed, then good | Install, Uninstall, Start and Stop a bottom bar on the padding (x 8, 8 px over the bottom); the fields from the top padding. |
| FormCameraSettings | fixed, then good | Section headings 18 to 20 px under the content before them (main 18 to 35): the trigger notes as tall as their two lines, the one-line notes one line tall. |
| FormCameraSettings-150, FormSheetFieldCheckBox-150, -allergy-150, UserControlTasks-150 | good | As at 100%; UserControlTasks' tree and grid 12 px from both edges (x 12 to 1399 in 1412). |
| FormUpdate-150, FormMain-150 (before) | drawn | The branch point's two windows (6f343389d's FormUpdate and ServiceManager FormMain files with this slice's harness), so every window and state but the three code states has a before. |

Gallery `G-UI3-ui12-14` redrawn: 52 pictures, 44 changed, 5 the same, 3 only after (FormProgressStatus-history, FormUpdate-load,
FormWebConfigSettings-service: states of code this slice wrote).
