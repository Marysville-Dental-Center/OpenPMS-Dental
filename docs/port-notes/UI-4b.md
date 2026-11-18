# Port note: UI-4b (core controls, part 2)

Package UI-4b of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-4b row, sections 3.2, 3.3 and
3.6, the owner's decisions of 2026-11-17 and the approved design sheet G-UI1 part 1), cut into three slices that run one after another. Each
slice has its own section here.

## UI-4b-1: combo boxes, pickers and lists

On `ui/ui4b1-1` from `main` at `2306500b1` (UI-4a merged). It changes the look of the controls it owns and nothing else: no bounds, no layout,
no text position in the WinForms controls, no behaviour. No schema change, no new package, nothing downloaded, no database used, no window
shown.

### 1. For the owner, in plain words

**What changed.** Combo boxes (the drop-down pickers, the clinic pickers among them) are white with a thin grey border, small rounded corners
and the new down arrow (the Fluent chevron). Under the mouse the border darkens (no more blue wash). With the keyboard (Tab) a blue line shows
along the bottom of the combo box that has focus. Lists (list boxes, the list that drops down from a combo box, the picture list and the tree
in the WPF windows) are white with a grey border; the row under the mouse is pale blue; the selected row is light blue with a thin bar on its
left edge, blue while the list has focus and dark grey when it does not, so you can tell which list you are in. A disabled combo box or list is
pale with grey text.

**What did not change.** Sizes, positions, fonts and texts (the font change is UI-2b). The text in a combo box is cut where it was cut before.
The eForm a patient fills (it has no combo box or list). Printouts.

**For you to look at.** Gallery G-UI1 part 2b (section 6): every state of each control, WinForms above WPF, at 100 to 200%. One thing to say
yes or no to: the "Add patient" window (`FrmPatientAddAll`) sets its list boxes' selection back to today's blue-grey after it clears a
required field's yellow; that window is not part of this package, so those lists show today's selection colour there until it is changed
(section 3).

### 2. Files

| File | What |
|---|---|
| `CodeBase/UI/UiListPaint.cs` | the WinForms shapes: the combo box's box, chevron and focus line (`DrawComboBox`, `DrawComboBoxBox`, `DrawComboBoxFront`, `GetChevronRectangle`, `DrawComboFocusLine`, `GetComboTextRight`), a list's border, a list item's fill and selection bar, the focused row's outline; built on UI-4a's `UiControlPaint`, so the snapping rule of 3.3 holds and the shapes are the design sheet's |
| `OpenPmsDental/UI/ComboBox.cs` | the box from the tokens (the window's `ColorBack` still wins), the Fluent chevron, the text clipped where today's mask cut it, the focus line with focus and keyboard cues (`IsFocusRingShown`) |
| `OpenPmsDental/UI/ClinicSelector/ComboBoxClinicPicker.cs` (+ designer) | the same box right of its label, the label in `Text`, the focus line; its pens and brushes (and their `Dispose` in the designer file) gone |
| `OpenPmsDental/UI/ClinicSelector/FormComboPicker.cs` | the drop-down: the "combobox" on top in its hover look, the list `SurfaceRaised` with a `StrokeStrong` border, `RowHover`, `AccentSubtle` with the selection bar (`HasFocusForSelection`) |
| `OpenPmsDental/UI/ListBox.cs` | `SurfaceRaised`, square `StrokeStrong` border (`Stroke` disabled), `RowHover`, the selection fill (`ColorSelectedBack` a window sets still wins; empty means the token) and bar (`HasFocusForSelection`), the focused row's outline (`IsFocusRingShown`), the text in `Text`, `TextSelected`, `TextDisabled` |
| `WpfControlsOD/UI/Controls/UiLookLists.cs` | the key of the combo box's focus visual style, the list brushes, the Fluent chevrons, the shared item colours (`GetItemFill`, `GetSelectionBar`, `GetItemText`, `GetBack`) |
| `WpfControlsOD/Themes/Generic.xaml` | one block after UI-4a's focus styles: the combo box's focus visual style, the tree's expander style and the `TreeViewItem` style |
| `WpfControlsOD/UI/Controls/ComboBox.xaml(.cs)` | `borderBack` (fill, 4 px corners), the `pathChevron`, the border's corners; `SetColors` from the tokens, disabled look, the focus line as its `FocusVisualStyle` |
| `WpfControlsOD/UI/Controls/ComboClinic.xaml` | the label in `Text` |
| `WpfControlsOD/UI/Controls/ListBox.xaml(.cs)`, `Supplemental/WindowComboPicker.xaml(.cs)` | the outer border and fill from the tokens; each row's selection bar as its 2 px left border; `SetColors` from the tokens; the bar's focus from `IsKeyboardFocusWithin`; `WindowComboPicker.AddItems` taken out of `Loaded` unchanged (the tests fill a picker without showing it) |
| `WpfControlsOD/UI/Controls/ListView.xaml(.cs)` | the border and fill from the tokens; each thumbnail item a 2 px bar rectangle; `SetColors` from the tokens |
| `WpfControlsOD/UI/Controls/TreeView.xaml(.cs)` | the tree's border and fill; `TreeViewItem` takes its own style from `Generic.xaml` (`DefaultStyleKey`) |
| `WpfControlsOD/UI/Controls/Supplemental/PopupHelper2.cs` | unchanged: it builds the link items of context menus and draws nothing (their look is the menus', UI-6); its coverage row is now `no-drawing` |
| `tests/UiRefreshTests/ListControlStates.cs`, `ListControlStateTests.cs`, `ListControlGallery.cs` | the tests and the gallery strips (section 5) |
| `tests/UiRefreshTests/ControlStateTests.cs`, `ControlStateGallery.cs` | UI-4a's comparison helpers and `SaveStrip` made `internal` for the above; nothing else |
| `tests/UiRefreshTests/Data/colour-literals.tsv`, `coverage.tsv` | 28 literals fewer (the package's files have none left); the row for `UiLookLists`; `PopupHelper2` is `no-drawing` |

### 3. Decisions made on the way (say if you disagree)

- **The chevron where the sheet draws it, the text where it was.** The chevron's 12 px square is 6 px from the right edge (as the sheet). The
  WinForms combo box clips its text where today's mask covered it (15 px from the right, `GetComboTextRight`), so the text's width does not
  change; the Fluent glyph's ink lies inside that area. The WPF text is cut at the same place: it keeps its 14 px drop-down column and gains a
  2 px right margin, so a long text now ends 16 px from the right edge, 2 px further left than before (the review's fix: the Fluent chevron's
  ink, 16 to 8 px from the right, would otherwise be drawn over the text's last 2 px). The clinic picker drew its text
  over the arrow area before (no mask); it still does, after the chevron.
- **The focus line needs keyboard cues**, as UI-4a's ring: WinForms `Focused && ShowFocusCues`, WPF the `FocusVisualStyle`. Today's combo box
  showed a blue border whenever it had focus, also after a mouse click; now a click shows nothing until the keyboard is used (section 3.6).
- **Hover darkens the border only.** Today's blue wash and blue border are gone (the sheet's hover is `StrokeStrongHover`).
- **A disabled combo box** is `SurfaceRaised` with a `Stroke` border and `TextDisabled` text and chevron (the sheet); today it was grey.
- **Colours a window sets win.** `ColorBack` on the combo boxes and lists and `ColorSelectedBack` on the lists replace the token only when
  they are not today's default (white; the old blue-grey `#BAC7DB` is no longer the default: empty or null means `AccentSubtle`).
  `FrmPatientAddAll.xaml.cs:865` sets `ColorSelectedBack` back to `#BAC7DB` when it clears a required list's yellow, so that list shows
  today's selection colour afterwards. The Frm is not in this package; changing that line to the token (or to not set it) is a one-line follow
  up for the owner to approve (it is a colour literal outside the shared controls, so the colour-literal scan does not list it).
- **The selection bar in WinForms** covers the item rectangle's first 2 px (scaled), where today's fill started (x = 2, inside the 1 px border
  and today's 1 px white gap). The text is still drawn at x = 2; GDI+'s own leading padding keeps its ink right of the bar, which the tests
  check (`CheckTextClearOfBar`). In WPF the bar is each row's 2 px left border, and the row content's 2 px margin gives way to it, so the text
  stays where it was.
- **The focused row (WinForms list box)** is the row the arrow keys move from (`OnKeyDown`: the last selected row; the first row when none is
  selected), outlined 1 px `Accent` while the list has focus and keyboard cues. The WPF lists' arrow-key row is their selection, so they show
  focus by the bar's colour only; a WPF focused-row outline would need a keyboard-cue state the WPF lists do not have (left for a later
  package with the WPF keyboard checks, like UI-4a's WPF focus ring).
- **The list box's check box mode (WPF)** highlights its arrow-key row as today, now in the selection look (fill and bar); a disabled one keeps
  the selection fill with the grey bar (today it was a grey fill).
- **The WPF tree** keeps the Windows layout (the 19 px expander column, children below, the row as wide as its header); its header starts 2 px
  further right, after the bar. The expander is the Fluent chevron (right when collapsed, down when expanded) in `TextSecondary`, `Text` on
  hover, in the 16 px square of the Windows triangle. The `TreeView` around the items is `SurfaceRaised` with a `StrokeStrong` border, `Stroke`
  when disabled, as the sheet's disabled list (the review's fix): its style sets the colours and a trigger the disabled border, and its
  template is the stock one (border, scroll viewer, items) without the stock disabled trigger that painted `SystemColors.Control`.
- **The drop-down lists** draw their "combobox" on top in the hover look (the pointer is on it while it opens) and their selection bar
  `Accent` while they have focus, as they do while open.
- **The eForm fill surface has none of these controls** (`CtrlEFormFill` builds text boxes, check boxes, radio buttons, labels, date and rich
  text boxes), so there is no template to pin; `EFormLookTests` is unchanged and green. A later eForm field type that uses one of them needs a
  pinned copy (`ContentLook.IsPinned`, UI-4a).
- **Test hooks.** The WinForms controls have `protected virtual` `IsFocusRingShown` and `HasFocusForSelection` (as UI-4a's `IsFocusRingShown`);
  the WPF lists, which a test cannot subclass across assemblies, read a private `_isFocusForced` that only the tests set.

### 4. Not done, and why

- Pictures of the real windows with these controls (G-UI3) need shown windows: section 7 has the steps for the lead.
- WPF hover in the tree and the WPF combo box's live focus line are drawn by WPF from the mouse and the keyboard: the tests apply the same
  look the trigger or the focus style applies; a connected desktop shows them live.

### 5. Tests

All in `tests/UiRefreshTests` (no new test assembly):
- `ListControlStateTests` (7 tests): WinForms `ComboBox` and `ComboBoxClinicPicker` (rest, hover, focus, disabled), `ListBox` (rest, hover,
  focus, disabled, selected with and without focus), `FormComboPicker` (rest, hover, selected with and without focus); WPF `ComboBox` (the
  four states, the focus as the focus visual style laid over it), `ComboClinic` (rest, hover, disabled), `ListBox`, `WindowComboPicker`,
  `ListView` and `TreeView` (rest, hover, disabled, selected with and without focus). Each control is drawn off
  screen without text and compared with the design sheet's drawing of the state at the control's bounds (`ListStateDrawings`: the shapes and
  light tokens of `DesignSheetControls.DrawComboBox` and `DrawListOrGridRow`, the Fluent chevron in place of the sheet's stand-in) by
  ChartImageCompare's tolerance (WinForms every pixel, WPF the solid pixels, as UI-4a). Then with text: its pixels reach the state's text token,
  and no item text starts in the selection bar's columns. The WPF `ComboBox` also with a text longer than the box, in every state: no text
  pixel falls in the chevron's ink columns (its square less 2 px at each side).
- `EFormLookTests.EFormFill_SyntheticForm_LooksAsBefore`: unchanged and green (the eForm picture as before).
- The layout snapshot and the bounds record (`LayoutSnapshotTests`, `Scan_TodaysFont_ReportsNothingNew`), `TerminologyLayoutTests`
  (M2TERMTests), `LayoutDesignerTests` (M2INSPLANTests): unchanged and green; no designer layout changed.
- The scan rerun (`OPMS_UI_SCAN=1`, into `.git/od-tools/test-results/ui4b1-scan`): Segoe UI 9 makes 1,775 controls overflow in 450 windows,
  8.25 makes 637 in 245, the same counts as UI-4a's rerun (this package changes no text rectangle); today's font reports nothing new.

### 6. Gallery G-UI1 part 2b

`.git/od-tools/test-results/ui4b1-G-UI1-part2b/` (`index.html` and the zip beside it; the owner's summary is the note at the top of the page),
made from the strips in `.git/od-tools/test-results/ui4b1-G-UI1-part2b-strips/` (each of `scale-100` to `scale-200`): `combobox.png`
(rest, hover, focus, disabled), `clinic-picker.png` (WinForms `ComboBoxClinicPicker` rest, hover, focus, disabled; WPF `ComboClinic` rest,
hover, disabled), `listbox.png` (WinForms rest, hover, focus, disabled, selected, selected without focus; WPF the same without focus),
`dropdown.png` (`FormComboPicker` and `WindowComboPicker`: rest, hover, selected, selected without focus) and `listview-treeview-wpf.png`
(the WPF `ListView` in the first row, the WPF `TreeView` in the second: rest, hover, disabled, selected, selected without focus). WinForms in
the top row, WPF below, states left to right. WinForms scales through LayoutManager's scale, WPF through its render scale. The WinForms list's
text is drawn in points from its own 8.25 pt font, so in these off-screen pictures at 125-200% it stays at its 96 DPI size (on a real screen at
that scale it grows with it). The pictures are "only after", as UI-4a's: the controls keep no copy of today's look; the before set of real
windows (`ui-before`) is for G-UI3.

### 7. For the lead

- The strips render off screen and were made in the disconnected session (WinForms and WPF both draw). To make them again:

      set OPMS_UI_LISTS_GALLERY=<od-tools>\test-results\ui4b1-G-UI1-part2b-strips
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out <od-tools>\test-results\ui4b1-strips.txt
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -After <od-tools>\test-results\ui4b1-G-UI1-part2b-strips -Out <od-tools>\test-results\ui4b1-G-UI1-part2b -Name G-UI1 -Note "<the summary of section 1>"

- The real windows with the new combo boxes and lists (G-UI3) need shown windows: at the owner's connected desktop, after a full Debug x64
  lane build, group U against the before set (docs/port-notes/UI-0c.md section 5), as for UI-4a:

      set OPMS_WINDOW_CHECKS=1
      set OPMS_WINDOW_CHECKS_DUMP=<od-tools>\winchk-gate3a\opendental_dev.sql
      set OPMS_WINDOW_CHECKS_GROUPS=U
      set OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui4b1-groupU
      set OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18
      set OPMS_WINDOW_CHECKS_UI_BEFORE=<od-tools>\test-results\ui-before\run1
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <od-tools>\test-results\ui-before\run1 -After <od-tools>\test-results\ui4b1-groupU -Name G-UI3 -ClipNotes <od-tools>\test-results\ui4b1-groupU\ui-walker\clip-notes.tsv

  (Split into `UW`, then `UM,UD` if one run does not fit the runner's 20 minutes; the before folders `run1` to `run5` and `fix1` hold the
  groups as UI-0c froze them.) U0 shows every window with a combo box or list as changed; the bounds record must show no difference. Worth a
  live look there: a WPF combo box reached with Tab (its focus line), a tree under the mouse (hover), and a WinForms list box reached with Tab
  (the focused row's outline).
- `FrmPatientAddAll.xaml.cs:865` (section 3): the owner's yes or no on giving its reset the token.

### 8. Runs

Full filter build x64 through the lane: 0 errors. Targeted runs (x64, after the full build, `.git/od-tools/test-results/ui4b1-final.txt`):
UiRefreshTests 110 (107 passed, 3 opt-ins skipped: the two galleries and the scan), WindowChecksTests 71 (29 passed, 42 opt-in skipped),
M2TERMTests 21/21, M2INSPLANTests 31/31, NoHqTests 122/122. The scan run (`ui4b1-scan.txt`) and the strips run (`ui4b1-strips.txt`) passed
too. `tools/Test-NoEolFlips.ps1 -Base main`: no file changed its line ends. The branch merges into the current `main` without conflicts.

The review's fix (`ui/ui4b1-2`: the WPF combo text cut 2 px further left, the disabled WPF tree's border): full filter build x64 0 errors;
Targeted run `.git/od-tools/test-results/ui4b1-fix2.txt`: UiRefreshTests 110 (107 passed, 3 opt-ins skipped), WindowChecksTests 71 (29 passed,
42 opt-in skipped), M2TERMTests 21/21, M2INSPLANTests 31/31, NoHqTests 122/122. `tools/Test-NoEolFlips.ps1 -Base main`: 0 line-end changes.

## UI-4b-2: tabs and splitters

On `ui/ui4b2-1` from `main` at `a0264972c` (UI-4b-1, UI-5, UI-6 merged). It changes the look of the controls it owns and nothing else: no bounds,
no layout, no tab rectangle, no tab width, no behaviour. No schema change, no new package, nothing downloaded, no database used, no window shown.

### 1. For the owner, in plain words

**What changed.** The row of tabs sits on a light grey band. The tabs are flat: their names are dark grey, a soft grey shape shows under the
mouse, and the open tab has dark text with a blue line under it (no more raised light-blue tab with rounded grey outlines). A tab you cannot
use is pale. A tab that has its own colour (the Account module colours its hidden-splits tab red) keeps that colour. The page under the tabs
has a thin light grey border. Splitters (the bar you drag between two panels) are a thin grey line in the gap, blue while you drag; the panels'
borders are thin light grey instead of silver. In the WPF windows a tab reached with the keyboard shows the blue focus ring.

**What did not change.** Every tab keeps its width, its order and its place (the width is still the text's width plus 8 px). Sizes,
positions, fonts and texts (the font change is UI-2b). Where a window set its own splitter or border colour, it keeps it. The 11 stock Windows
tab controls. The eForm a patient fills (it has no tabs or splitters). Printouts.

**Checked for you (UQ-9).** In the four windows with the most tabs (the claim, the medical history, the fee schedule tools and the patient
edit window) no tab runs past the edge of its row, in today's font or in Segoe UI 9, at the window's normal size or squeezed onto a
1024 x 768 screen. The tightest is the fee schedule tools window: its six tabs need 399 of 405 px in Segoe UI 9.

**For you to look at.** Gallery G-UI1 part 2c (section 6): every state, WinForms above WPF, at 100 to 200%. The four windows' tab rows before
and after (G-UI2, G-UI3). Two small questions in section 3: the open tab has no white fill (the approved sheet has none), and the splitter has no
hover colour (the sheet has none; the mouse pointer already changes over it).

### 2. Files

| File | What |
|---|---|
| `CodeBase/UI/UiTabPaint.cs` | the WinForms shapes: the strip (`SurfaceSubtle`), the page border (1 px `Stroke`), a tab header (`DrawHeader`: a page's `ColorTab`, or `SubtleHover` under the mouse, the selected tab's 2 px `Accent` line on the edge facing the page, the text token) and a splitter (`DrawSplitter`: its area, the 1 px line in its middle, `Gridline` or `Accent`); built on UI-4a's `UiControlPaint`, so the snapping rule of 3.3 holds |
| `OpenPmsDental/UI/TabControl/TabControl.cs` | `OnPaint` from the tokens for tabs on top, at the bottom and on the left (`FormEmailInbox`); `LayoutTabs`, the mouse and the designer code unchanged; today's `GetPathTab` and the fixed `#AABEE6` gone |
| `OpenPmsDental/UI/Splitters/SplitContainer.cs` | `OnPaint` from the tokens; `GetColorSplitterBack`, `GetColorBorderLine` (the old default Silver means the token, any other colour a window set wins), `GetRectangleSplitter`; `Invalidate` on press, drag and release so the line's colour follows |
| `OpenPmsDental/UI/Splitters/SplitterPanel.cs` | the panel's outer edges in `GetColorBorderLine`, `GetLineWidth` wide |
| `WpfControlsOD/Themes/TabControlDict.xaml` | the `TabControl` template (the stock layout: the header panel's 2 px margin, the content's 1 px border and 2 px padding; a `SurfaceSubtle` strip behind the header panel; the page `Surface` with a `Stroke` border; Top, Bottom, Left and Right placements) and the `TabPage` template (transparent, `SubtleHover` with 4 px corners, the selected tab's 2 px `Accent` line, `Text`, `TextSecondary`, `TextDisabled`; today's padding and the selected tab's -2 px margins, so no header moves) |
| `WpfControlsOD/UI/Controls/TabControl.cs`, `TabPage.cs` | the token brushes for the dictionary; the `TabPage`'s focus visual style (UI-4a's shared ring) |
| `WpfControlsOD/UI/Controls/SplitContainer.cs`, `WpfControlsOD/Themes/Generic.xaml` | the brushes; in the `SplitContainer`'s block of `Generic.xaml` an implicit `GridSplitter` style (its `Style.Resources`): `Surface` with a 1 px `Gridline` line along the middle (vertical for a column splitter), `Accent` while `IsDragging` |
| `tests/UiRefreshTests/TabStripFitTests.cs`, `Data/tab-overflow.tsv` | the width formula and the four windows' strips (section 5) |
| `tests/UiRefreshTests/TabSplitStates.cs`, `TabSplitStateTests.cs`, `TabSplitGallery.cs` | the state tests and the gallery strips |
| `tests/UiRefreshTests/Data/colour-literals.tsv` | one literal fewer (`TabControl.cs`'s `#AABEE6`) |

### 3. Decisions made on the way (say if you disagree)

- **The open tab has no fill.** The approved design sheet (`DesignSheetControls.DrawTabHeader` and `DrawTabsComposite`) and section 3.6 draw the
  selected tab as `Text` with the 2 px `Accent` line on the strip, without a fill; the package's short brief said "`SurfaceRaised`". The code
  draws what the sheet shows. A white (`SurfaceRaised`) fill is a one-line change in `UiTabPaint.DrawHeader` and the WPF `IsSelected` trigger if
  the owner wants it.
- **No hover colour on splitters.** The sheet's splitter has rest, dragging and focus, no hover (section 3.6: "`Accent` while dragging"); the
  brief said "with a hover". The cursor already turns into the split arrows over a splitter. Not added; one more `Gridline`/`StrokeStrong` state
  if the owner wants it.
- **No focus ring on the WinForms tabs and splitters.** `UI.TabControl` and `UI.SplitContainer` are UserControls with no keyboard handling: they
  never take focus themselves (their pages and panels do), so there is nothing to show a ring for. The WPF `TabPage` takes keyboard focus and
  gets UI-4a's ring (`UiLook.SetFocusVisualStyle`).
- **The selected WinForms tab is no longer drawn 2 px bigger.** Today it was drawn over a rectangle 2 px larger on three sides; now every tab
  draws in its own rectangle from `LayoutTabs`, so the selected tab's text is centred 1 px lower than before. Rectangles, hit testing and widths
  are unchanged. Tabs at the bottom (the Chart module's images) draw below the page's border line, which their rectangle starts on.
- **The WPF headers keep today's layout exactly.** The selected `TabPage` keeps its -2 px margins (it is laid out 4 px narrower and drawn
  2 px over each neighbour), so no header moves when the selection changes; its `Accent` line spans the header as drawn. The header's old
  1 px border never took space (it was a sibling of the content in the template's grid), so dropping it changes no size. The text colour is
  the `TabPage`'s `Foreground` (a header given as an element, like the tests' `TextBlock`, inherits it through the logical tree, not from the
  template's presenter).
- **Splitter colours a window sets still win.** `ColorSplitter` and `ColorBorder` default to Silver in the designer (`DefaultValue`); Silver now
  means "the token" (`Surface` around the panels, `Stroke` for the borders), so no designer file changes. `FormQueryFavorites` and `FormTaskEdit`
  set both to near-white and keep it; `FormOpenDental` sets `ColorBorder` to its own background and keeps it. The line is drawn on any
  colour. The WPF `GridSplitter`'s own `Background` (Silver in both test Frms and the usage note) is today's look and is not drawn.
- **The page inside the border keeps the window's colour.** The WinForms `TabPage` takes its parent's `BackColor` as today (the window
  background becomes `Surface` with UI-11); the gap inside the border is the parent's colour as today. The WPF page is `Surface`.
- **"Its smallest size".** None of the four windows sets a `MinimumSize` (LayoutManager's rule), so the test's smallest size is the designer
  size cut to the smallest desktop the refresh checks, 1024 x 768 less a 40 px taskbar (UI-6's 1024 px toolbars): `FormClaimEdit` (1206 x 747)
  becomes 1024 x 728 and its anchored main tab control 182 px narrower; `FormPatientEdit` loses 2 px of height; the other two fit as they are.
  The windows are built as the scan builds them (designer code and LayoutManager), so the tabs are the designer's; tabs a window adds or hides
  in code are not counted.
- **The eForm fill surface has no tabs or splitters**, so there is no template to pin; `EFormLookTests` is unchanged and green.

### 4. Not done, and why

- Pictures of the real windows with the new tabs and splitters (G-UI3 group U) need shown windows: section 7 has the steps for the lead.
- WPF hover on a tab is WPF's own `IsMouseOver`: the test sets the border as the trigger does and checks that the template's trigger gives
  `SubtleHover`; a connected desktop shows it live. The WPF splitter's drag colour is set through `IsDragging`'s key the same way.

### 5. Tests

All in `tests/UiRefreshTests` (no new test assembly):
- `TabStripFitTests.TabHeaders_KeepTodaysWidthFormula_TextWidthPlus8`: at 100, 150 and 200% every tab starts at x 2 or where the one before it
  ends, and is `ScaleMS(MeasureString(text))` plus `Scale(8)` wide (`LayoutTabs`, 3.3).
- `TabStripFitTests.TabStrips_FourWindows_KeepTheirOrder_AndFit_OrTheOverflowIsListed`: `FormClaimEdit` (10 tabs in `tabControlMain` and
  `tabControlAttach`), `FormMedical` (9 in `tabControlFormMedical` and `tabControl1`), `FormFeeSchedTools` (6), `FormPatientEdit` (5), each tab
  control at the designer size and the smallest size, in today's font and in Segoe UI 9 (`UiTheme.SetFontForTest(UiFontOption.UI2b)`): the tab
  counts of section 7, the tabs in order, every strip wider in Segoe UI 9, and the tabs past the strip's visible right edge (the tab control's
  width, cut by any parent) equal to `Data/tab-overflow.tsv` (`OPMS_UI4B2_WRITE_OVERFLOW=1` rewrites it). It lists none. The widest strips
  (tabs' right edge / room, in px): `FormFeeSchedTools` 375 / 405 today and 399 / 405 in Segoe UI 9; `FormMedical` 522 / 791 and 557 / 791;
  `FormClaimEdit` main 355 / 1000 (smallest) and 375 / 1000; `FormPatientEdit` 349 / 465 and 369 / 465. With `OPMS_UI_TABS_WINDOWS` it draws
  the strips and writes `tab-fit.tsv` beside them.
- `TabSplitStateTests` (6 tests): WinForms `TabControl` on top (rest, hover, a disabled tab, a page colour), at the bottom (rest) and on the left
  (hover); WinForms `SplitContainer` vertical and horizontal, at rest and dragging, and the colours a window sets winning; WPF `TabControl`
  (rest, hover, keyboard focus, a disabled tab), the template's hover trigger and focus style, and the WPF `GridSplitter` at rest and dragging.
  Each control is drawn off screen without text and compared with the design sheet's drawing of the state at the control's own geometry
  (`TabSplitDrawings`: the shapes and light tokens of `DesignSheetControls.DrawTabHeader`, `DrawTabsComposite` and `DrawSplitter`) by
  ChartImageCompare's tolerance (WinForms every pixel, WPF the solid pixels, as UI-4a). Then with text: each tab's text pixels reach its token
  (`Text` on the selected tab and on a page colour, `TextDisabled` on the disabled one, `TextSecondary` otherwise).
- `EFormLookTests`, the layout snapshot and the bounds record (`LayoutSnapshotTests`, `Scan_TodaysFont_ReportsNothingNew`),
  `TerminologyLayoutTests` (M2TERMTests), `LayoutDesignerTests` (M2INSPLANTests): unchanged and green; no designer layout changed.

### 6. Galleries

- **G-UI1 part 2c**: `.git/od-tools/test-results/ui4b2-G-UI1-part2c/` (`index.html` and the zip beside it; the owner's summary is the note at
  the top), made from the strips in `.git/od-tools/test-results/ui4b2-G-UI1-part2c-strips/` (each of `scale-100` to `scale-200`): `tabs.png`
  (WinForms top row: rest, hover, a disabled tab, a page colour; WPF below: rest, hover, keyboard focus, a disabled tab),
  `tabs-bottom-left-winforms.png` (WinForms tabs at the bottom, then on the left: rest and hover) and `splitters.png` (WinForms vertical rest,
  dragging, horizontal rest, dragging; WPF rest and dragging). WinForms scales through LayoutManager's scale, WPF through its render scale; the
  WinForms tab text is drawn in points from its 8.25 pt font, so at 125-200% off screen it stays at its 96 DPI size (on a real screen at that
  scale it grows with it), as in UI-4b-1's strips. "Only after", as UI-4a's: the controls keep no copy of today's look.
- **The four windows' tab strips, before and after (G-UI2, G-UI3)**: `.git/od-tools/test-results/ui4b2-G-UI3-tabstrips/`, from
  `ui4b2-tabs-before` (drawn by `TabStripFitTests` on this branch's first commit, before the look changed) and `ui4b2-tabs-after`: each tab
  control of the four windows at the designer size and the smallest size, in `today/` and `segoe9/` (the strip and 12 px of the page).

### 7. For the lead

- The strips and the window strips render off screen and were made in the disconnected session (WinForms and WPF both draw). To make them again:

      set OPMS_UI_TABS_GALLERY=<od-tools>\test-results\ui4b2-G-UI1-part2c-strips
      set OPMS_UI_TABS_WINDOWS=<od-tools>\test-results\ui4b2-tabs-after
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out <od-tools>\test-results\ui4b2-strips.txt
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -After <od-tools>\test-results\ui4b2-G-UI1-part2c-strips -Out <od-tools>\test-results\ui4b2-G-UI1-part2c -Name G-UI1 -Note "<the summary of section 1>"
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <od-tools>\test-results\ui4b2-tabs-before -After <od-tools>\test-results\ui4b2-tabs-after -Out <od-tools>\test-results\ui4b2-G-UI3-tabstrips -Name G-UI3 -Note "<the strips' summary>"

  (The before strips need the code before the look: check out `bf51eeedd`, build `UiRefreshTests` and run the same with
  `OPMS_UI_TABS_WINDOWS=<od-tools>\test-results\ui4b2-tabs-before`.)
- The real windows with the new tabs and splitters (G-UI3) need shown windows: at the owner's connected desktop, after a full Debug x64 lane
  build, group U against the before set (docs/port-notes/UI-0c.md section 5), as for UI-4a and UI-4b-1:

      set OPMS_WINDOW_CHECKS=1
      set OPMS_WINDOW_CHECKS_DUMP=<od-tools>\winchk-gate3a\opendental_dev.sql
      set OPMS_WINDOW_CHECKS_GROUPS=U
      set OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui4b2-groupU
      set OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18
      set OPMS_WINDOW_CHECKS_UI_BEFORE=<od-tools>\test-results\ui-before\run1
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <od-tools>\test-results\ui-before\run1 -After <od-tools>\test-results\ui4b2-groupU -Name G-UI3 -ClipNotes <od-tools>\test-results\ui4b2-groupU\ui-walker\clip-notes.tsv

  (Split into `UW`, then `UM,UD` if one run does not fit the runner's 20 minutes; the before folders `run1` to `run5` and `fix1` hold the
  groups as UI-0c froze them.) U0 shows every window with a self-drawn tab control or splitter as changed (the main window's splitters
  among them); the bounds record must show no difference. Worth a live look there: a tab under the mouse, the Account module's red
  hidden-splits tab, the Chart module's image tabs at the bottom, `FormEmailInbox`'s tabs on the left, a splitter while it is dragged, and
  a WPF tab reached with the keyboard (`FrmTestAllControls`).
- The owner's answers to the two questions of section 3 (the open tab's fill, a splitter hover).

### 8. Runs

Full filter build x64 through the lane: 0 errors. Targeted run (x64, after the full build, `.git/od-tools/test-results/ui4b2-final.txt`):
UiRefreshTests 153 (148 passed, 5 opt-ins skipped: the four galleries and the scan), WindowChecksTests 72 (29 passed, 43 opt-in skipped),
M2TERMTests 21/21, M2INSPLANTests 31/31, NoHqTests 122/122. The strips run (`ui4b2-run2.txt`, with the two opt-in folders) passed too. The scan
rerun (`OPMS_UI_SCAN=1`, into `.git/od-tools/test-results/ui4b2-scan`, `ui4b2-scan.txt`): Segoe UI 9 makes 1,775 controls overflow in 450
windows, 8.25 makes 637 in 245, the same counts as UI-4b-1's rerun (this package changes no text rectangle); today's font reports nothing new.
`tools/Test-NoEolFlips.ps1 -Base main`: no file changed its line ends.


## UI-4b-3: calendars, date pickers and text boxes

On `ui/ui4b3-1` from `main` at `296aa44d8` (UI-4b-1, UI-4b-2, UI-5, UI-6, UI-9a-1 merged). It changes the look of the controls it owns and nothing
else: no bounds, no layout, no text position, no behaviour. No schema change, no new package, nothing downloaded, no database used, no window shown.

### 1. For the owner, in plain words

**What changed.** The calendars (the month you pick a date from, in the Appointments module and under every date box) are white with a thin light
grey border. The selected day is a blue rounded square with white numbers; today has a thin blue rounded ring (it was orange); the day under the mouse
is soft grey (it was light blue). The year and month arrows are the new Fluent arrows, still with their Y and M beside them. The days of the months
before and after are pale grey as before, the weekday names dark grey. The small "Today" mark at the bottom is the same blue ring. The date pickers'
little drop-down button shows a calendar icon instead of the down arrow; in the WPF windows the whole date picker looks like the new combo boxes
(rounded, thin grey border, darker under the mouse, a blue line along the bottom while you type in it, red border when the date is wrong). The WPF text
boxes have a thin grey border that darkens under the mouse, a blue line along the bottom while you type in them, a lighter border when they are read
only or disabled (disabled text pale grey), and a red border when what you typed is not valid; the little red "!" beside them is the new red.

**What did not change.** Sizes, positions, fonts and texts (the font change is UI-2b). The text inside every text box starts exactly where it did
(the tests check it pixel by pixel). The WinForms text boxes and the WinForms date boxes are Windows text boxes and keep the Windows look (section
3.6). The eForm a patient fills keeps today's text boxes, date boxes and rich text boxes. Printouts.

**For you to look at.** Gallery G-UI1 part 2d (section 6): every state of each control, WinForms above WPF, at 100 to 200%. Two small questions in
section 3: the text boxes are square, as the approved design sheet draws them (the brief said 4 px corners), and the blue line shows whenever a text box
has the cursor, also after a click (as the Windows text boxes do), not only after Tab.

### 2. Files

| File | What |
|---|---|
| `CodeBase/UI/UiCalendarPaint.cs` | the WinForms shapes: a day's hover (`SubtleHover`, 4 px corners), the selected day (`Accent`, `TextDisabled` disabled), today (`DrawToday`: the 1 px `Accent` ring, a white inner ring when today is selected), the focus ring on the selected day (`FocusRingOnAccent`), a day's text token (`GetDayText`), the Fluent chevrons of the arrows and the Fluent calendar glyph (`GetCalendarDesign`, `DrawCalendarGlyph`: calendar_ltr, the icon of `EnumIcons.Appt32`); built on UI-4a's `UiControlPaint` |
| `OpenPmsDental/UI/MonthCalendarOD.cs` | `OnPaint` from the tokens: `SurfaceRaised`, the `Gridline` under the weekdays, hover, the selected day, today, the focus ring (`IsFocusRingShown`: focus and keyboard cues), the hovered buttons and month title, the chevrons where the triangles were (`GetRectangleArrow`), the letters Y and M where they were, the days in `Text`, `TextOnAccent`, `TextDisabled`, the weekdays in `TextSecondary`, the footer's today mark, the 1 px `Stroke` outline; every rectangle and text position as before; `Invalidate` on focus and enabled changes; test hooks `IsTextShown`, `GetPointMouse` |
| `OpenPmsDental/UI/ODDatePicker.cs` (+ designer) | the drop-down button flat `SurfaceRaised`, `SubtleHover`, `SubtlePressed`, the calendar glyph in `Text` (`TextDisabled` when disabled or read only) from its `Paint` (`GetRectangleGlyph`); the designer's arrow picture and grey colours gone |
| `OpenPmsDental/UI/ODDateRangePicker.cs` | unchanged: its two `ODDatePicker`s carry the look; its two week buttons are UI-4a's `UI.Button` (their arrow pictures, section 4) |
| `WpfControlsOD/UI/Controls/UiLookText.cs` | the keys of the three templates, the attached `IsError`, the token brushes, the corner (`CORNER_TEXT`) and focus line metrics, the Fluent chevrons and calendar glyph, `ApplyTemplateLook` (the template and the `StrokeStrong` border, or the Windows ones in office content) |
| `WpfControlsOD/Themes/Generic.xaml` | one block at the end: the template of a `TextBoxBase` (TextBox, RichTextBox) and of a `PasswordBox` (the Windows layout: a border of the box's thickness around `PART_ContentHost`; triggers for hover, read only, `IsError`, keyboard focus, disabled, the text colour) and the date picker's inner text box template (no border colour) |
| `WpfControlsOD/UI/Controls/TextBox.xaml.cs`, `TextPassword.xaml.cs`, `TextRich.xaml.cs` | `IContentLook`: `ApplyLook` gives the inner box the new template, or the Windows one when pinned; the TextBox's underline (`IsUnderline`, eForms) `StrokeStrong`, today's grey when pinned |
| `WpfControlsOD/UI/Controls/TextVDate.xaml(.cs)`, `TextVDouble`, `TextVInt`, `TextVTime` | the same; `SetError` sets `UiLookText.IsError`; the error mark (`ellipseError`) `Danger`, today's red when pinned; `TextVDate`'s un-underline clears the border brush instead of writing `#ABADB3` |
| `WpfControlsOD/UI/Controls/DatePicker.xaml(.cs)` | as a combo box: `borderBack` (`SurfaceRaised`, 4 px corners), `border` (`StrokeStrong`, `StrokeStrongHover`, `Stroke`, `Danger`), `borderFocus` (the 2 px `Accent` line while the text box has keyboard focus), `pathCalendar` (the glyph in the 14 px column); the inner text box keeps its place and its own 1 px border, now transparent; today's blue wash and polyline gone |
| `WpfControlsOD/UI/Controls/MonthCalendar.xaml(.cs)` | the four triangles are Fluent chevron paths; `SetLook` from the tokens as the WinForms calendar; the day shapes snap to device pixels; the shared focus ring (`UiLook.SetFocusVisualStyle`) |
| `WpfControlsOD/UI/Controls/Supplemental/WindowMonthCalendar.xaml.cs` | the window's background `SurfaceRaised` (behind its calendar) |
| `tests/UiRefreshTests/TextCalendarStates.cs`, `TextCalendarStateTests.cs`, `TextCalendarGallery.cs`, `TextPinnedLookTests.cs`, `Data/text-pinned-before.png` | the tests and the gallery strips (section 5) |
| `tests/UiRefreshTests/DesignSheetWindows.cs` | `DrawTree` made `internal` (the WinForms date picker's picture paints its Windows date box the same way); nothing else |
| `tests/UiRefreshTests/Data/colour-literals.tsv`, `coverage.tsv` | 14 literals fewer (`MonthCalendarOD`, the WPF `DatePicker` and `MonthCalendar` have none left, `TextVDate.xaml.cs` none); the row for `UiLookText` |

### 3. Decisions made on the way (say if you disagree)

- **The text boxes are square.** The approved design sheet draws its text input square (the window drawing of G-UI1 part 1,
  `DesignSheetControls.DrawWindowComposite`, beside a combo box with 4 px corners), as the Windows text boxes of the WinForms windows are; the brief
  and section 3.3 say 4 px. The code draws what the sheet shows. 4 px corners are one constant (`UiLookText.CORNER_TEXT`); the focus line's inset
  follows it. The date pickers have 4 px corners, as the combo boxes they look like.
- **The focus line shows whenever a text box has keyboard focus**, also after a click: a text box takes the typing (the caret is in it), today's
  Windows border turned blue on any focus, and the Windows 11 text boxes do the same. UI-4b-1's combo box line needs keyboard cues; a text box is the
  exception. The WPF date picker follows its text box.
- **Read only:** a `Stroke` border that does not darken under the mouse, the text in `Text`, the focus line as usual (the text can be selected and
  copied). The fill stays the window's (`ColorBack`, which many windows set grey for read-only boxes); WPF does not pair read only with a colour.
- **A validation error** (`TextVDate`, `TextVDouble`, `TextVInt`, `TextVTime`, the WPF `DatePicker`): the border `Danger`, and the red "!" mark beside
  the box in `Danger`, where it was (outside the box's right edge, as today). Disabled wins over the error; the focus line stays `Accent`.
- **Disabled:** the window's fill (white: `SurfaceRaised`), a `Stroke` border and `TextDisabled` text, as the sheet's disabled combo box; today's
  56% opacity is gone.
- **The text colour is `Text`** (was black), from the template's trigger, so a colour a window sets (`ColorText`) still wins.
- **Calendar details the sheet does not show:** a day under the mouse, the arrow buttons, the month title and the "Today" footer under the mouse are
  `SubtleHover` with 4 px corners (today light blue; the title turned its text blue); the days of the months around are `TextDisabled` (closest to
  today's grey 170); the weekday names `TextSecondary` (the sheet's); when today is the selected day, a white inner ring inside its `Accent` ring
  shows it on the fill; the letters Y and M stay beside the chevrons (they tell the year arrows from the month arrows); the footer's today mark is the
  today ring. A disabled calendar: texts and chevrons `TextDisabled`, the selected day `TextDisabled`.
- **Focus on a calendar:** WinForms draws the sheet's focus on the selected day (the ring outside a white inner ring) with focus and keyboard cues; the
  WPF calendar has no per-day keyboard state, so it takes UI-4a's shared ring around itself (2 px `Accent`, inside its bounds).
- **The WinForms date picker** keeps its Windows date box (section 3.6: Windows text boxes keep their look; their border is the text area's); only
  the drop-down button changes. The glyph is pale while the picker is read only, because the calendar does not open then.
- **The calendar glyph is `EnumIcons.Appt32`'s Fluent icon (calendar_ltr)**, found through its manifest row, so no icon is added (the icon files
  belong to the UI-9a-2 stream). A test checks it stays calendar_ltr.
- **The WPF date picker's text does not move:** its text box keeps its 1 px border thickness with a transparent brush and its own place in the
  column, on a template of the Windows layout (a test compares its text with the Windows template's). The 14 px drop-down column keeps its width; the
  12 px glyph sits 2 px from the right edge.
- **The WPF calendar's day shapes snap to device pixels** (section 3.3), so the selected day and the rings are crisp at every scale; the old comment
  said the orange ring looked bad snapped, which was the old colour at a fractional width.
- **The eForm fill surface keeps today's text fields.** `CtrlEFormFill` builds `TextBox` (plain, multi-line, underlined), `TextVDate` (plain and
  underlined) and `TextRich`; the pin (`ContentLook.IsPinned`) gives them the Windows template, today's grey underline and the red mark.
  `TextPinnedLookTests` compares them with a picture recorded on this branch's first commit, before any change. The other text controls take the pin
  too, for eForm fields added later.

### 4. Not done, and why

- **The date range picker's week buttons** (`ODDateRangePicker`, "W" with an arrow picture each) are `UI.Button`s, restyled by UI-4a; their arrow
  pictures are `.resx` pictures, and pictures to icons are UI-9's job. They are not in `icons/picture-map.tsv` yet (UI-9a-2's file): for the lead.
- Pictures of the real windows with these controls (G-UI3) need shown windows: section 7 has the steps for the lead.
- WPF hover and the WPF text boxes' live focus line are drawn by WPF from the mouse and the keyboard: the tests set the template's parts as its
  triggers do and check the triggers; a connected desktop shows them live.

### 5. Tests

All in `tests/UiRefreshTests` (no new test assembly):
- `TextCalendarStateTests` (6 tests): WinForms `MonthCalendarOD` (rest, hover on a day and the month-back button, focus, disabled, today selected) and
  `ODDatePicker`'s button (rest, hover, disabled); WPF text boxes (`TextBox`, `TextRich`, `TextPassword`, `TextVDate`, `TextVDouble`, `TextVInt`,
  `TextVTime`: rest, hover, focus, disabled, and read only or a validation error), `DatePicker` (rest, hover, focus, disabled, error) and
  `MonthCalendar` (rest, hover, disabled, today selected). Each control is drawn off screen without text and compared with the design sheet's drawing
  of the state at the control's own geometry (`TextCalendarDrawings`: the shapes and light tokens of `DesignSheetControls.DrawCalendarDay`,
  `DrawCalendarAndProgress`, `DrawComboBox` and the window drawing's input, the Fluent glyphs of UI-7a) by ChartImageCompare's tolerance (WinForms
  every pixel, WPF the solid pixels, as UI-4a). Then with text: its pixels reach the state's token (the WPF calendar: the brushes of its days,
  weekdays, chevrons and header). The text of every WPF text box, of the multi-line box and of the date picker starts where the Windows template put
  it (the ink's rectangle the same in both). The templates' triggers give the states; each text control takes the new template on a window and the
  Windows one inside office content. The calendar glyph is Fluent calendar_ltr.
- `TextPinnedLookTests.EFormTextFields_Pinned_LookAsBefore`: the eForm's text field kinds with the pin, as recorded before the change.
- `EFormLookTests.EFormFill_SyntheticForm_LooksAsBefore`: unchanged and green (the eForm picture as before).
- The layout snapshot and the bounds record (`LayoutSnapshotTests`, `Scan_TodaysFont_ReportsNothingNew`), `TerminologyLayoutTests`
  (M2TERMTests), `LayoutDesignerTests` (M2INSPLANTests): unchanged and green; no designer layout changed (`ODDatePicker`'s designer lost its picture
  and colour lines only).
- The scan rerun (`OPMS_UI_SCAN=1`, into `.git/od-tools/test-results/ui4b3-scan`): Segoe UI 9 makes 1,775 controls overflow in 450 windows, 8.25 makes 637 in 245, the same counts as UI-4b-2's rerun (every file of the two runs is the same but the timing lines: this package changes no text rectangle); today's font reports nothing new.

### 6. Gallery G-UI1 part 2d

`.git/od-tools/test-results/ui4b3-G-UI1-part2d/` (`index.html` and the zip beside it; the owner's summary is the note at the top of the page), made
from the strips in `.git/od-tools/test-results/ui4b3-G-UI1-part2d-strips/` (each of `scale-100` to `scale-200`): `calendars.png` (WinForms
`MonthCalendarOD` in the top row: rest, hover, focus, disabled, today selected; WPF `MonthCalendar` below: rest, hover, disabled, today selected),
`datepickers.png` (WinForms `ODDatePicker`: rest, hover on the button, disabled; WPF `DatePicker`: rest, hover, focus, disabled, error), and the WPF
text boxes two kinds per strip, a row each, states left to right (rest, hover, focus, disabled, then read only or the error):
`textbox-textrich-wpf.png`, `textpassword-textvdate-wpf.png`, `textvint-textvdouble-wpf.png`, `textvtime-wpf.png`. WinForms scales through
LayoutManager's scale, WPF through its render scale; the WinForms texts are drawn in points from their 8.25 pt font, so in these off-screen pictures
at 125-200% they stay near their 96 DPI size (on a real screen at that scale they grow with it), as in UI-4b-1's strips. The WinForms date box is
painted as Windows paints a text box (the design sheet's window pictures do the same: an edit control of a hidden window prints nothing). "Only
after", as UI-4a's: the controls keep no copy of today's look, except the eForm's pinned text fields (`Data/text-pinned-before.png`).

### 7. For the lead

- The strips render off screen and were made in the disconnected session (WinForms and WPF both draw). To make them again:

      set OPMS_UI_TEXT_GALLERY=<od-tools>\test-results\ui4b3-G-UI1-part2d-strips
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out <od-tools>\test-results\ui4b3-strips.txt
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -After <od-tools>\test-results\ui4b3-G-UI1-part2d-strips -Out <od-tools>\test-results\ui4b3-G-UI1-part2d -Name G-UI1 -Note "<the summary of section 1>"

- The real windows with the new calendars, date pickers and WPF text boxes (G-UI3) need shown windows: at the owner's connected desktop, after a full
  Debug x64 lane build, group U against the before set (docs/port-notes/UI-0c.md section 5), as for UI-4a, UI-4b-1 and UI-4b-2:

      set OPMS_WINDOW_CHECKS=1
      set OPMS_WINDOW_CHECKS_DUMP=<od-tools>\winchk-gate3a\opendental_dev.sql
      set OPMS_WINDOW_CHECKS_GROUPS=U
      set OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui4b3-groupU
      set OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18
      set OPMS_WINDOW_CHECKS_UI_BEFORE=<od-tools>\test-results\ui-before\run1
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <od-tools>\test-results\ui-before\run1 -After <od-tools>\test-results\ui4b3-groupU -Name G-UI3 -ClipNotes <od-tools>\test-results\ui4b3-groupU\ui-walker\clip-notes.tsv

  (Split into `UW`, then `UM,UD` if one run does not fit the runner's 20 minutes; the before folders `run1` to `run5` and `fix1` hold the groups as
  UI-0c froze them.) U0 shows every window with a calendar, a date picker or a WPF text box as changed (the Appointments module's calendar among
  them); the bounds record must show no difference. Worth a live look there: a WPF text box under the mouse and while typing in it (the focus line), a
  WPF date box with a wrong date (the red border), a date picker's calendar opened, the Appointments module's calendar under the mouse, and an eForm
  filled in `FrmEFormFillEdit` (unchanged).
- The two week buttons of `ODDateRangePicker` (section 4): their arrow pictures for a UI-9 slice's `picture-map.tsv`.
- The owner's answers to the two questions of section 1 (square text boxes; the focus line after a click).

### 8. Runs

Full filter build x64 through the lane: 0 errors. Targeted run (x64, after the full build, `.git/od-tools/test-results/ui4b3-final.txt`):
UiRefreshTests 169 (163 passed, 6 opt-ins skipped: the five other galleries and the scan; this package's gallery ran with `OPMS_UI_TEXT_GALLERY`),
WindowChecksTests 72 (29 passed, 43 opt-in skipped), M2TERMTests 21/21, M2INSPLANTests 31/31, NoHqTests 122/122. The scan run (`ui4b3-scan.txt`) and the
strips run after the WinForms date picker's picture fix (`ui4b3-strips.txt`) passed too. `tools/Test-NoEolFlips.ps1 -Base main`: no file changed its line ends.
