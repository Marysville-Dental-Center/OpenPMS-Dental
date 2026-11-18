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
