# UI-11-final: the sweep, the leftovers, the reference pictures and the release notes

Package UI-11-final (the lead, 2026-11-24): the closing package of the UI refresh ([ui-refresh-plan.md](../specs/ui-refresh-plan.md)
section 7, order item 8). Branch `ui/ui11final-1` from `e4eeb6760`. Gallery G-UI3-ui11final (`.git/od-tools/test-results/G-UI3-ui11final/`),
audit [UI-11-final-audit.md](UI-11-final-audit.md). Release notes for the office: [docs/release-notes/ui-refresh.md](../release-notes/ui-refresh.md).
`<W>` below is `.git/od-tools/test-results/G-UI3-ui11final-src`.

## 1. The sweep

One pass over every window the harness builds (`WindowBuilder.GetScanTypes`: the program's windows and user controls, the plain exes'
windows, the graph's, the report designer's and ODR's: 1,030 types, 1,026 built; the four it cannot build are below), automated first.

**FinalSweepTests** (`tests/UiRefreshTests/FinalSweepTests.cs`, new; the rules in `FinalSweep`). Each window is built off screen as the
gallery builds it and its controls are walked for what is left of the classic look:

| Rule | A finding |
|---|---|
| `classic-grey` | a control that shows the window's background (`UiPlainWindow.IsBackground`: the window, panels, user controls, group boxes, labels, check boxes, radio buttons) in Windows' grey or the old window background (`IsOldBackground`) |
| `stock-button`, `stock-check-box`, `stock-radio-button`, `stock-group-box`, `stock-tab-control`, `stock-grey-block`, `stock-caption`, `stock-separator`, `stock-3d-border` | a stock type a shared look draws (`IsDrawn`) without that look (`IsAttached`) |
| `input-frame` | an input the frame draws (`UiInputFrame.IsFramed`) without it |
| `scroll` | scroll bars in Windows' look (`UiScrollLook`) |
| `plain-window`, `title-bar` | a plain window (a Form that is not a FormODBase) without `UiPlainWindow.Attach`, or without the program's frame (`UiPlainFrame`: Windows' default title bar) |
| `grid` | a window that is not on the layout grid's list (`grid-windows`), so `GridRulesTests` does not check it |

A finding that stays is a row of `tests/UiRefreshTests/Data/sweep-known.tsv` (window, rule, key, why); a new finding fails, and so does a
row the sweep no longer finds. `OPMS_UI_SWEEP_OUT=<file>` writes every finding of a run (`<W>/sweep-before.tsv` on `main`,
`<W>/sweep-after.tsv` on the branch).

**What it found on `main`** (`e4eeb6760`): 35 look findings in 17 windows and 124 windows not on the grid's list. On the branch: 24 look
findings in 15 windows and the same 124, each a row of `sweep-known.tsv` with its reason (148 rows).

- Fixed: the report viewer library's two dialogs, `DialogMessages` (the report's warnings) and `DataSourcePassword` (a data source's pass
  phrase), drew in Windows' look in the program and in the report designer: the library references no CodeBase, so UI-PLAIN-2 did not reach
  it. Now `RdlViewer.ActionAttachLook` (set by the program in `LayoutManagerForms.SetLooksScale`, by the report designer in its `Main` with
  its own copy of the looks) is called by both constructors (`ReportViewerDialogs_TakeTheLookFromTheHost`). The harness now builds them by
  their constructor (`WindowBuilder`'s list, with the program's hook set), so the sweep and the gallery show them as the program does.
- Kept with a reason (the rest of the 35): the old drawn table's scroll corner and the time pattern built on it (`ContrTable`,
  `TableTimeBar` in FormApptTypeEdit, FormProcCodeEdit, FormProcCodeNoteEdit: content, as `colour-literals.tsv` says), ToolBarOD's unused
  designer colour (FormClaimsSend), the popups and covers that paint themselves (FormFrame, FormSnap, FormSplash, FormComboPicker,
  ToolTipOD, ModuleSwitchBlurOverlay, FormPopupFade's own grey), the screening tooth (content), and `OpenPmsDental.MsgBoxCopyPaste`, whose
  base class's constructor attaches the look (the harness skips base constructors).
- Not on the grid's list, each with UI-12's reason (UI-12.md's "Skipped" tables, quoted in `sweep-known.tsv`) or its own: the shared
  controls (GridOD, ToolBarOD, TabControl, the pickers), the windows UI-12 left with their reasons (the main window, the modules with panels
  laid over one another, the editors and document viewers, the plain progress windows, the report designer's property pages), and the
  imaging windows built in code (below).

**What the harness cannot reach** (for a later check; nothing here is known to be wrong):

- Four types it cannot build: `ODDatePicker` and `ToothChartWrapper` (need a running program), `UserControlSecurityUserGroup` (its code adds
  a control without `LayoutManager.Add` while the harness builds it), the report designer's `DataSetRowsCtl` (needs a report). The same four
  failed in every earlier scan.
- The imaging windows built in code with no `InitializeComponent` (imaging upgrades: ControlCameraSession, ControlVideoPlayer,
  FormCameraSettings, FormImageEnhance, FormImageViewer, FormToothPicker): the harness builds them empty. They are drawn by their own
  galleries (`CameraSessionGallery`, `ImageViewerGallery`) and were reviewed there.
- The WPF windows (`WpfControlsOD/Frms`): the harness builds WinForms windows; the WPF controls' looks have their own tests (UI-4a to
  UI-OVF) and this session cannot draw WPF (below).

**The other scans** (all match on the branch):

- `GridRulesTests` over the 906 windows of `grid-windows` with `grid-known` (unchanged), and FinalSweepTests' `grid` rule for the rest.
- The element inventory: `python tools/UiFit/element_inventory.py --check` differed from `main`'s committed file only in three code counts
  UI-OVF added (`ContextMenuStrip`, `ToolStripMenuItem`, `ToolStripSeparator` in ToolBarOD's overflow drop-down); written again. Every one
  of the 27 `keeps` rows is still justified: Windows' dialogs and message boxes (11 rows; plan section 1), web content (3), clinical and
  office content (8: the perio chart, the tooth chart's two controls, the signature box, the schedule graph, the appointment panel, two
  sheet controls), documents (the report viewer), the graph library's charts and legend (3), and Windows 11's themed tooltip.
- `python tools/UiFit/colour_literals.py --check`: matches (857 rows of the UI-11 section).
- The font-literal scan (`FontLiteralScanTests`) and the chrome colour scan (NoHqTests' `ChromeColourScanTests`): pass.

## 2. The leftovers

- **WPF radio buttons, one tab stop per group** (the lead's decision on UI-OVF's question). `WpfControls.UI.RadioButton.UpdateGroupTabStops`:
  the radio buttons of one parent are one tab stop, as in WinForms: the checked one when it is enabled and visible, else the first enabled,
  visible one in tab order (`TabIndex`, then the parent's order); the others are no tab stops. It runs when a radio button is loaded, checked
  or unchecked, enabled or disabled, shown or hidden. The arrow keys move the check (and the stop) inside the group, as before (UI-OVF).
  `UiTabOrder` orders every radio button whether or not it is the stop, and makes each group's stop again once the indexes are set. Tests:
  `WpfFocusTests.RadioButton_Group_IsOneTabStop_TheCheckedOne` (new), the real Tab walk now passes the group box once.
- **FormInsPayFix and FormLanguageAndRegion `textBoxDescript`**: editable multi-line boxes that UI-11 left white and borderless on the
  window's Surface. They take the input frame, as every other editable box: the designers' `BorderStyle.None` line is gone (the default 3-D
  border, which the frame replaces; `UiInputFrame.IsFramed`); their `SystemColors.Window` stays (the input's box, `colour-literals.tsv`).
  Bounds unchanged.
- **UI-OVF's WPF pictures**: `ControlPictures.RendersWpf()` is false in this session (disconnected), so they are not drawn. To draw them in a
  connected, unlocked desktop session (after a full x64 build of this branch or of `main` after the merge):

  ```
  set OPMS_UI_OVF_GALLERY=<od-tools>\test-results\G-UI3-uiovf-wpf
  powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -Include UiRefreshTests -Out <od-tools>\test-results\uiovf-wpf.txt
  ```

  `notes.txt` in the folder then lists `written:` for `imaging-today-100/150` (`-before`, `-after`, `-after-open`, `-after-open-sub`) and
  `wpf-focus-100/150`; audit those ten pictures into [UI-OVF-audit.md](UI-OVF-audit.md) before the owner looks.

## 3. The reference pictures

The pictures later changes are compared with, taken again from this final build (the owner's approvals, `approved.tsv`, are kept as they
are: an approval is the owner's, not a baseline):

- **The windows** (the galleries' before set from now on): every window the harness builds at 100% and the densest tenth at 150%, drawn by
  `FitGallery` in the program's font from the full x64 build of `f20ec8b71` (the product code of this branch's end), frozen in `.git/od-tools/test-results/ui-baseline-final/`
  (`windows-100/`, `windows-150/`, `commit.txt`). `tests/WindowChecksTests/UiApproved/baseline.tsv` (new) records each picture with its
  size and its masked pixel SHA-256 (the galleries' one rule, `UiPictureMask`), in `approved.tsv`'s format, with the commit: a later package
  compares its pictures with it (`New-UiGallery.ps1 -Before .git/od-tools/test-results/ui-baseline-final`, or the hashes). Drawn again
  with a Targeted x64 UiRefreshTests run and `OPMS_UI_FIT_GALLERY=<folder>`, `OPMS_UI_FIT_GALLERY_FONT=program`,
  `OPMS_UI_FIT_GALLERY_WINDOWS=@all`; then `OPMS_UI_FIT_GALLERY_SCALE=1.5` with `OPMS_UI_FIT_GALLERY_WINDOWS=@densest`.
- **The layout snapshot** (`Data/layout-snapshot/`, the designer bounds `LayoutSnapshotTests` compares with): written again from this
  branch (`OPMS_UI_LAYOUT_SNAPSHOT_WRITE=1`); it equals the committed files but for FormLanguageAndRegion's rows (section 2).
- **Needs a connected desktop session** (WPF draws nothing in this disconnected one, `ControlPictures.RendersWpf()` is false, and the
  window checks start the program with its windows, which a package may not do):
  - the window checks' before set (`.git/od-tools/test-results/ui-before/`, from `3809f35db`, the bounds record U0 compares with): one full
    harness run of groups A to K and U from the merged `main`, as UI-0c.md section 5 says, into `.git/od-tools/test-results/ui-before-final/`
    with `commit.txt`, the dump's hash and the DPI; from then on `OPMS_WINDOW_CHECKS_UI_BEFORE` names that folder;
  - the two pinned WPF content pictures (`Data/eform-fill-before.png`, `Data/text-pinned-before.png`): one Targeted x64 UiRefreshTests
    run with `OPMS_UI_EFORM_BEFORE_WRITE=1` and `OPMS_UI_TEXT_PINNED_BEFORE_WRITE=1`, then commit the two files. They pin office content,
    which the refresh did not change; their tests compare with the pictures recorded before UI-4a and UI-4b-3 (skipped where WPF draws
    nothing).

## 4. Release notes

[docs/release-notes/ui-refresh.md](../release-notes/ui-refresh.md): for the office, in plain language: what changed in the look and in the
layout, what stayed (office content and colours, printouts, the tooth chart, Windows' own dialogs), the new keyboard behaviour (the »
toolbar button, toolbars from the keyboard, Tab in the WPF windows, radio button groups, the focus ring) and what is still open.

## 5. Gallery G-UI3-ui11final

`.git/od-tools/test-results/G-UI3-ui11final` (zip beside it): every window the harness builds at 100%, before (`main` at `e4eeb6760`,
built x64, drawn in the same session) and after (the branch, the build of `f20ec8b71`), and the densest tenth at 150% (after only):
1,129 pictures, 4 changed, 1,022 the same, 103 only after. The changed ones are the windows this package changes: FormInsPayFix and
FormLanguageAndRegion (the framed description boxes; the second 12 px taller), and the report viewer's DialogMessages and
DataSourcePassword (the program's frame, background, buttons and inputs). The sweep's findings that stay are in `sweep-known.tsv`; their
windows are in the gallery as they are (the same before and after). Audit: [UI-11-final-audit.md](UI-11-final-audit.md). Raw pictures,
the sweep's records and work files: `<W>` (`before-100`, `after-100` (the first after run), `final-100`, `final-150`, `sweep-before.tsv`,
`sweep-final.tsv`).

```
odbuild.ps1 -Root . -Platform x64 -Projects OpenPmsDental,OpenPmsDentalGraph,xCentralManager,UiRefreshTests -ScopedRestore   (the product files at e4eeb6760)
OPMS_UI_FIT_GALLERY=<W>/before-100 OPMS_UI_FIT_GALLERY_FONT=program OPMS_UI_FIT_GALLERY_WINDOWS=@all OPMS_UI_SWEEP_OUT=<W>/sweep-before.tsv run-tests.ps1 -Mode Targeted -Platform x64 -Include UiRefreshTests
the branch's full x64 build, then the same run into final-100, and OPMS_UI_FIT_GALLERY_SCALE=1.5 OPMS_UI_FIT_GALLERY_WINDOWS=@densest into final-150
tools/New-UiGallery.ps1 -Before <W>/gal-before -After <W>/gal-after -Name G-UI3 -Out .git/od-tools/test-results/G-UI3-ui11final   (windows-100/, windows-150/)
```

## 6. Tests

New: `FinalSweepTests` (the sweep: `Sweep_EveryWindow_NoClassicLookLeft`, `Sweep_EveryWindow_IsOnTheGridList`;
`ReportViewerDialogs_TakeTheLookFromTheHost`), `WpfFocusTests.RadioButton_Group_IsOneTabStop_TheCheckedOne`, WindowChecksTests'
`UiHarnessTests.Baseline_OneCommit_EveryPictureOnce` (the reference set's record). Changed: the real Tab walk passes the radio button group
once; `PlainLookTests` counts five windows built by their constructor (the report viewer's two dialogs added to `WindowBuilder`'s list).
FitGallery: the window lists `@all` and `@densest`.

Full x64 build through the lane at `f20ec8b71` (the last code change; the commits after it are data and notes): 0 errors. Targeted x64 run
(`.git/od-tools/test-results/ui11final-run1.txt`, 2026-11-24): UiRefreshTests 347 passed, 50 skipped (the WPF pictures of a disconnected
session and the opt-in galleries), 0 failed, both real Tab walks passed; WindowChecksTests 31 passed, 44 skipped (opt-in); UnitTests 1728
passed, 3 skipped; PrintGuardTests 19 passed, 1 skipped; NoHqTests 130 passed; P262BillingTests 54 passed. `tools/Test-NoEolFlips.ps1
-Base e4eeb6760`: 0 flips. `python tools/UiFit/element_inventory.py --check` and `python tools/UiFit/colour_literals.py --check`: match.
On `main` (`ui11final-before100.txt`) the same UiRefreshTests run had only the two new sweep tests failing (before `sweep-known.tsv`).
