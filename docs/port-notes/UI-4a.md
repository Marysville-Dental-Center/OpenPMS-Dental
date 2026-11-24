# Port note: UI-4a (core controls, part 1)

Package UI-4a of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-4a row, sections 3.2, 3.3 and
3.6, the owner's decisions of 2026-11-17 and the approved design sheet G-UI1 part 1), on `ui/ui4a-1` from `main` at `deb903107`. It changes
the look of the controls it owns and nothing else: no bounds, no layout, no behaviour (the WPF button's pressed look is drawn, its click still
fires on mouse down as before). No schema change, no new package, nothing downloaded, no database used, no window shown.

## 1. For the owner, in plain words

**What changed.** Buttons are flat and white with a thin grey border and small rounded corners; the button a window's Enter key clicks (its
default button) is filled with the program's blue and has white text. Hover and pressed are a light grey; a disabled button is pale grey with
grey text. Check boxes have a crisp box with rounded corners; a ticked box is blue with a white tick (the Fluent icon), a "partly" box blue with
a white dash. WPF radio buttons look like the Windows ones beside them. Group boxes have a light grey border with rounder corners. Tooltips are
white with a thin grey border. With the keyboard (Tab), the control that has focus gets a blue ring.

**What did not change.** Sizes, positions, fonts and texts (the font change is UI-2b). Icons on buttons (UI-7b and UI-9a). eForms: the form a
patient fills keeps today's look (it is office content, like a sheet). Printouts.

**For you to look at.** Gallery G-UI1 part 2 (section 6): every state of each control, WinForms above WPF, at 100 to 200%. And the list of
default buttons for G-UI3: 12 of the 86 are not an OK or Save button (Print, Send, Run Report, Try Again, Check, Create List, Edit); say for
each whether it should stay blue or stay plain.

## 2. Files

| File | What |
|---|---|
| `CodeBase/UI/UiControlPaint.cs` | the WinForms shapes of 3.6 from the tokens with the snapping rule of 3.3: `Fill`, `Outline`, `FocusRing`, `FocusRingOnAccent`, `DrawButton`, `GetButtonFill`, `DrawCheckBoxBox`, `GetCheckFocusRectangle`; the same shapes as the design sheet's `SheetPainter` (so the control and the sheet give the same pixels); WinForms and `System.Drawing` only |
| `OpenPmsDental/UI/Button.cs` (+ designer) | the new paint; `IsPrimary` (the form's `AcceptButton`, minus an allow list for G-UI3's answers, empty now); `IsFocusRingShown` (focus and keyboard cues); no gradient, no glow; `DrawSimpleButton` (ODButtonPanel, UI-6) unchanged |
| `OpenPmsDental/UI/CheckBox.cs` | the box in the tokens (the same 13 px it covered), the Fluent check mark and dash, the hover border, the focus ring around box and text |
| `OpenPmsDental/UI/GroupBox.cs` | 1 px `Stroke`, 6 px corners, the title in `Text` where it was |
| `OpenPmsDental/UI/PanelOD.cs` | the default border (Silver) is `Stroke`; square (below) |
| `OpenPmsDental/UI/ToolTipOD.cs` | `SurfaceRaised`, 1 px `Stroke`, 4 px corners (the window cut to them), `Text` |
| `WpfControlsOD/UI/Controls/UiLook.cs` | `UiLook` (token brushes for XAML, the keys of the shared focus visual styles, `SetFocusVisualStyle`), `ContentLook.IsPinned` (inherited; office content keeps today's look), `IContentLook` |
| `WpfControlsOD/Themes/Generic.xaml` | the two shared focus visual styles (`FocusVisualStyle`, `FocusVisualStyleOnAccent`, `ComponentResourceKey`s of `UiLook`); the GroupBox style restyled |
| `WpfControlsOD/UI/Controls/Button.xaml(.cs)` | the new look (`borderBack`), the pressed look, `IsPrimary`; today's look kept beside it (gradient, `borderHover`, `borderClassic`) for the pin |
| `WpfControlsOD/UI/Controls/CheckBox.xaml(.cs)`, `RadioButton.xaml(.cs)` | the new box and circle beside today's, hover for the radio button, the pin |
| `WpfControlsOD/UI/Controls/Label.xaml.cs` | text in `Text` unless the window sets `ColorText`; today's black in office content |
| `WpfControlsOD/UI/Controls/ToolTip.xaml` | `SurfaceRaised`, 1 px `Stroke`, 4 px corners (`AllowsTransparency`), `Text` |
| `WpfControlsOD/UI/Controls/UserControls/CtrlEFormFill.xaml` | `ui:ContentLook.IsPinned="True"` on its root |
| 12 Frm XAML files, `FrmApptProvPrompt.xaml.cs` | `IsPrimary` on the button each Frm's Enter handler clicks (section 4) |
| `tests/UiRefreshTests/ControlStateTests.cs`, `ControlStates.cs`, `ControlPictures.cs`, `ControlStateGallery.cs`, `EFormLookTests.cs` | the tests and the gallery strips (section 5) |
| `tests/UiRefreshTests/Data/eform-fill-before.png` | the synthetic eForm as it looked on `main` before UI-4a (recorded at the first commit, before any control changed) |
| `tests/UiRefreshTests/Data/accept-buttons.tsv` | the AcceptButton list for G-UI3 |
| `tests/UiRefreshTests/Data/coverage.tsv`, `colour-literals.tsv` | the row for `UiLook`; ten Button.cs and CheckBox.cs literals fewer |

## 3. Decisions made on the way (say if you disagree)

- **The check box keeps the pixels it covered.** "Same box size as today": today's 12 px square was outlined one pixel beyond itself, so the
  box covered 13 x 13 px at 100% (and the WPF box is 13 DIP). The new box fills those 13 px (`sqrSize` plus one line width), so the text does
  not move. The design sheet draws 12; at the control's own size the shapes and tokens are the sheet's.
- **The WinForms check box's focus ring stays inside the bounds.** The box sits at the control's left edge (bounds unchanged), so the ring
  "around the box and its text, 4 px outside" is cut at the bounds and its left side lies on the box's left edge. Visible in the gallery.
- **A disabled checked box** (the sheet has none) is filled `TextDisabled` with the white mark, as Windows 11 greys it.
- **PanelOD stays square.** Its border is Windows' own (`WS_BORDER`) plus the panel's line inside it; rounding it would mean dropping the
  Windows border, which changes the client area and so where the children sit (decision 1). Only its default border colour takes `Stroke`;
  a colour a window sets stays. The WPF `Panel`, `PanelAutoScroll` and `PictureBox` draw only the background and border colours their
  windows set (white and transparent by default): nothing of theirs is a theme colour, so they keep their look; `LinkLabel` likewise.
- **WPF buttons and check boxes cannot take keyboard focus today** (`Focusable=false`, UserControl's default). The shared focus visual style
  is set on them, so it shows as soon as one is made focusable, but making them focusable would change tab order, which is behaviour, not
  look; left to the owner (a later package could do it with the keyboard checks). The WinForms controls show the ring now.
- **The default WinForms button is the form's `AcceptButton` whenever it is a `UI.Button`** (86 of the 107 assignments; the other 21 are stock
  buttons in RdlDesign, RdlViewer and others, which keep the Windows look). The 12 that are not OK or Save are marked `owner` in
  `accept-buttons.tsv`; one the owner turns down goes on `_arrayAcceptButtonsPlain` in Button.cs (UI-PLAIN fix round: now in CodeBase's UiButtonLook, UiLooks.cs, which UI.Button.IsPrimary asks) as `FormName.buttonName`. The plan counted
  87 and 13; the 13th (`button1`) is in `UnitTests`, which the list leaves out with the other test code.
- **The icon on a WinForms button** is drawn as today; Direct2D's background for it is the flat fill of the state (it was a gradient matched
  to the old one), so the icon sits on the right colour.
- **The pinned look is a copy beside the new one**, not a second template: the WPF controls are UserControls, so their XAML keeps today's
  elements and the new ones, and `SetColors` shows one set. `ContentLook.IsPinned` is inherited, so every control CtrlEFormFill adds later is
  pinned too.

## 4. IsPrimary on the Frms

Each Frm handles Enter in its own `KeyDown` (FrmODBase); grep of `Key.Enter`/`Key.Return` in `WpfControlsOD`:

| Frm | Button with `IsPrimary` |
|---|---|
| FrmApptFieldDefEdit, FrmApptFieldEdit, FrmDocInfo, FrmEmployerEdit, FrmSupplyNeededEdit (`butsave`), FrmZipCodeEdit, FrmZipSelect | butSave |
| FrmDatePicker, InputBox | butOK |
| FrmPatientSelect | butOK (Enter clicks OK once the grid is filled, Search before) |
| FrmAutoNotePromptList | butNext (Enter does what Next does) |
| FrmTestAllControls | button1 |
| FrmApptProvPrompt | butYes or butNo, set in Load by the prompt mode (PromptDefaultYes / PromptDefaultNo) |
| FrmMountLayoutEdit, FrmPatientAddAll | none: Enter moves focus or closes a list there |

`OpenPmsDental/UIManagement` (`WpfConverter`, `ButtonHelper`) still reports or wires the AcceptButton as before; emitting `IsPrimary` from the
converter is left to its owner.

## 5. Tests

All in `tests/UiRefreshTests` (no new test assembly):
- `ControlStateTests` (7 tests): every state of the design sheet for WinForms Button (plain and default: rest, hover, pressed, focus,
  disabled), CheckBox (rest, hover, focus, disabled, checked, indeterminate, disabled indeterminate), GroupBox, ToolTipOD and PanelOD; WPF Button
  (plain and IsPrimary), CheckBox, RadioButton, GroupBox, ToolTip and Label. Each control is drawn off screen without text and compared with
  the design sheet's drawing of the state at the control's bounds (`StateDrawings`: `SheetPainter`'s shapes and the light tokens, the calls of
  `DesignSheetControls`' families; the Fluent mark and dash in place of the sheet's stand-ins) by ChartImageCompare's tolerance (8 per channel,
  0.1%, the 16 x 16 block rule). WinForms: every pixel. WPF: the pixels the drawing paints in a solid token colour away from round edges (WPF
  antialiases its own way). Then drawn with text: its pixels must reach the state's text token. The WPF focus state is the shared focus visual
  style laid over the control (an off-screen control cannot take keyboard focus).
- `Wpf_PinnedLook_IsTodaysLook_AndDiffersFromTheNewLook`: the pinned button border is today's `#1C5180`, the pinned check box today's grey, the
  pinned label black, and the synthetic eForm without the pin differs from the pinned one (so the eForm test proves the pin).
- `EFormLookTests.EFormFill_SyntheticForm_LooksAsBefore`: the synthetic eForm (two check boxes, a radio group, a text field, the page buttons
  and label) is unchanged against `Data/eform-fill-before.png`, recorded on `main` before any control changed.
- `ControlStateGallery.AcceptButtons_ListMatchesTheSource`; the opt-in `Gallery_StateStrips_WinFormsBesideWpf`.
- The layout snapshot and the bounds record (`LayoutSnapshotTests`, the scan's `Scan_TodaysFont_ReportsNothingNew`), `TerminologyLayoutTests`
  (M2TERMTests), `LayoutDesignerTests` (M2INSPLANTests): unchanged and green; no designer file changed.
- The scan rerun (`OPMS_UI_SCAN=1`, into `.git/od-tools/test-results/ui4a-scan`): Segoe UI 9 makes 1,775 controls overflow in 450 windows,
  8.25 makes 637 in 245 (UI-0b measured 1,771/449 and 650/248 on an older `main`; UI-4a changes no text rectangle, so the difference is
  `main`'s own since then); today's font reports nothing new.

Targeted runs (x64, after the full build): UiRefreshTests 91 (89 passed, the 2 opt-ins skipped), WindowChecksTests 69 (27 passed, 42 opt-in
skipped), M2TERMTests 21/21, M2INSPLANTests 31/31, NoHqTests 122/122. Full filter build x64: 0 errors.

## 6. Gallery G-UI1 part 2

`.git/od-tools/test-results/ui4a-G-UI1-part2/` (`index.html` and the zip; the owner's summary is the note at the top of the page), made from
the strips in `.git/od-tools/test-results/ui4a-G-UI1-part2-strips/` (each of `scale-100` to `scale-200`: `button.png`, `button-default.png`,
`checkbox.png`, `radiobutton-wpf.png`, `groupbox-tooltip.png` (group box rest and disabled, then the tooltip) and `panel-label.png` (PanelOD
above the WPF Label, rest and disabled)). Each strip: WinForms cells in the top row, WPF in the bottom row, the states left to right as in
the design sheet. The tooltip at 125-200% is painted at that scale through a test subclass (ToolTipOD's `GetScale` is virtual for it; in the
program it is the monitor's). PanelOD has no LayoutManager: at each scale it has the bounds a scaled window gives it and its 1 px border. WinForms scales through LayoutManager's scale, WPF through its render scale. The
pictures are "only after": the WinForms controls have no copy of today's look to draw a before from; the before set of real windows
(`ui-before`) is for G-UI3. `accept-buttons-for-G-UI3.tsv` is beside the page.

## 7. For the lead

- The strips render off screen and were made in the disconnected session (WinForms and WPF both draw). To make them again:

      set OPMS_UI_STATES_GALLERY=<od-tools>\test-results\ui4a-G-UI1-part2-strips
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests -Out <od-tools>\test-results\ui4a-strips.txt
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -After <od-tools>\test-results\ui4a-G-UI1-part2-strips -Out <od-tools>\test-results\ui4a-G-UI1-part2 -Name G-UI1 -Note "<the summary of section 1>"

- The real windows with the new controls (G-UI3, and the AcceptButtons in their windows) need shown windows: at the owner's connected
  desktop, after a full Debug x64 lane build, group U against the before set (docs/port-notes/UI-0c.md section 5):

      set OPMS_WINDOW_CHECKS=1
      set OPMS_WINDOW_CHECKS_DUMP=<od-tools>\winchk-gate3a\opendental_dev.sql
      set OPMS_WINDOW_CHECKS_GROUPS=U
      set OPMS_WINDOW_CHECKS_OUT=<od-tools>\test-results\ui4a-groupU
      set OPMS_WINDOW_CHECKS_BUDGET_MINUTES=18
      set OPMS_WINDOW_CHECKS_UI_BEFORE=<od-tools>\test-results\ui-before\run1
      powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include WindowChecksTests
      powershell -NoProfile -ExecutionPolicy Bypass -File tools\New-UiGallery.ps1 -Before <od-tools>\test-results\ui-before\run1 -After <od-tools>\test-results\ui4a-groupU -Name G-UI3 -ClipNotes <od-tools>\test-results\ui4a-groupU\ui-walker\clip-notes.tsv

  (Split into `UW`, then `UM,UD` if one run does not fit the runner's 20 minutes; the before folders `run1` to `run5` and `fix1` hold the
  groups as UI-0c froze them.) U0 shows every window with a button or check box as changed; the bounds record must show no difference.
- A shown WPF Frm with keyboard focus on a focusable control would show the shared focus style live; none of UI-4a's WPF controls is
  focusable today (section 3).
