# Port note: UI-0b (the text-fit scan and the layout records)

Package UI-0b of the UI refresh (`docs/specs/ui-refresh-plan.md`, section 7, with sections 3.4, 3.7, 5.3 and 5.4), on `ui/ui0b-1` from `main`
at `e7f87b1d8`. Tools and tests only: no product file changes, nothing changes on screen.

## 1. Summary

- **New test project** `tests/UiRefreshTests` (SDK-style, net10.0-windows x64 from `tests/Directory.Build.props`, MSTest 4, its lock file
  from the offline folder; no new package), in `OpenDental.sln` and `OpenDental.Local.slnf` the way RdlCodeTests and MiddleTierHostTests
  were added. 22 tests; the full font-option scan is one of them and opt-in (section 4).
- **The shared measuring files** `tests/UiRefreshTests/Shared/TextFit*.cs`: the measurer (`TextFitMeasurer.cs`), the bounds record and its
  rules (`TextFitBoundsRecord.cs`) and the test-side font swap (`TextFitFontSwap.cs`). They use only Windows Forms, `System.Drawing` and
  reflection (the program's types and LayoutManagerForms' fields by name) and C# 8, so UI-0c can link them into `tests/WindowChecksTests`
  as they are. WindowChecksTests is not edited here.
- **The tier 1 builder and scan** (`tests/UiRefreshTests/Scan`): every Form and UserControl type of the program, its libraries and the
  plain exes, 1,026 types; 1,001 built, 3 need a database, 15 throw when built without their constructor, 7 have no designer code
  (section 5). One pass over all of them takes about 10 seconds; the full scan (three fonts, six scales, the text-scale run) 0.8 minutes.
- **The layout snapshot** `tests/UiRefreshTests/Data/layout-snapshot.tsv` (17,751 designer controls) and its test, with
  `tests/UiRefreshTests/Data/fit-fixes.tsv` (empty, for the UI-10 slices).
- **The measured counts** (section 6) replace the plan's modelled estimates. Segoe UI 9 (the owner's UQ-1 answer): 1,771 controls in
  449 windows newly overflow at 100% (988 of them visibly cut), and 856 controls that size themselves newly touch a neighbour or their
  parent's edge in 149 windows. Segoe UI 8.25: 650 in 248 windows (409 visibly cut), and 587 in 110 windows. The plan modelled 1,591 in
  422 files and 391 in 182 files, without the 0.96 factor and without the controls that size themselves.
- Targeted runs (x64): UiRefreshTests 22 of 22 (the opt-in scan skipped without `OPMS_UI_SCAN=1`, passed with it), NoHqTests 115 of 115.
  Full filter x64: 0 errors (section 8).

## 2. The builder (section 5.4 of the plan)

`Scan/WindowBuilder.cs`. For each type: `RuntimeHelpers.GetUninitializedObject`, then only the base constructor (FormODBase, Form or
UserControl, which chains to the rest of WinForms), then each class's own `InitializeComponent` from the base class down. The type's own
constructor never runs, so its arguments and its database calls do not matter.

- **Which types.** Every non-abstract `Form` or `UserControl` in OpenPmsDental, OpenPmsDentalBusiness, CodeBase, SparksToothChart,
  OpenPmsDentalGraph, RdlViewer, ODR and WpfControlsOD (loaded from the test's folder, or from their own build folder), and in the plain exes,
  read from their own build folders (`<project>\bin\Debug\<assembly>.dll`, as RdlCodeTests reads RdlDesigner.dll; not built: the
  `BuildOutput` rule): CentralManager, DatabaseIntegrityCheck, DocumentationBuilder, MySQLManager, OpenPmsDentalDbSettings,
  OpenPmsDentalHL7, RdlDesign (RdlDesigner), ServiceManager, UpdateFileCopier, xCrudGenerator (CrudGenerator). OpenPmsDentalHelper32 is
  x86 (decision 23) and cannot load in the 64-bit test process; it has no designer files.
- **How each is built** (`BuildKind`; the types list `types.tsv` gives each type's kind):
  - a FormODBase window whose constructor calls `InitializeLayoutManager` (813): the call by reflection after `InitializeComponent`, then
    the layout FormODBase.OnLoad does, without a handle or a Show: `SetPanelClient`, `SetScaleMS`, the window's bounds by OnLoad's
    formula, `LayoutFormBoundsAndFonts`;
  - a FormODBase window whose constructor does not call it (5): built like a plain form, so it shows as the product shows it, unmapped
    and unscaled. They are the five allocator windows of section 5.1 (`FormProgressViewer`, `FormInstallAllocator_Provider`,
    `FormReport_GuarantorAllocationCheck`, `FormReport_ProviderIncomeReport`, `FormWarnToCloseComputers`); a test pins the list;
  - a plain Form (72): `InitializeComponent` and WinForms' own layout, `AutoScaleMode` included;
  - a UserControl that a program window adds with `LayoutManager.AddUnscaled` (42): the preference pages (FormPreferences' UserControl
    fields), the setup wizard's pages (`SetupWizControl` and its subclasses) and `ControlImages`, docked to fill as FormPreferences and
    FormSetupWizardProgress do, in a bare test FormODBase (`HostFormOD`, which calls `InitializeLayoutManager` in its constructor), then one
    `LayoutControlBoundsAndFonts`;
  - a UserControl of the program, or of a library that the program's source creates with `new` (46): `LayoutManager.Add` in `HostFormOD`;
  - a UserControl no program window hosts (48): a plain test Form. These are the plain exes' and the graph's (the 12 graph UserControls of
    section 5.1 among them).
  This list is also the scope of the "Microsoft Sans Serif" source scan of section 9 (UI-2a-4): under LayoutManager are the first, fourth
  and fifth groups.
- **"Its constructor calls InitializeLayoutManager"** comes from the source (`Scan/SourceIndex.cs`): each class's constructors, found with
  their bodies by matching braces in the text without comments and strings, call it themselves or through a method of the same file that
  calls it (`FormCCDPrint`'s constructors call `Init()`, which calls it). A class with no constructor in the source takes its nearest base
  class's answer.
- **No database.** `NoDatabaseGuard` gives the process the connection string
  `Server=no-database.example.test;...;SslMode=none`, which the program's connection rule (`DbConnectionSecurity.CheckConnectionString`)
  refuses before any connection is made: not this computer, no server verification, no CA, no credentials, and the reserved `.test` host is
  never resolved. Without it the empty connection would go to this computer's server. A type that reads the database while it is built
  fails at once with `DbConnectionRefusedException` and is listed as needing a database.
- **No window.** Nothing calls Show; the scan checks after each build that no window of the process is visible. Some designer code makes
  window handles (a child shown inside the hidden window); an exception in such a control's window procedure (`ComboBoxClinicPicker.OnLoad`
  reading the clinics preference, in `FormFeeSchedTools`) would make WinForms show its "Unhandled exception" dialog, or end the process from
  inside the native callback. The scan's STA thread sets `UnhandledExceptionMode.CatchException` with an `Application.ThreadException`
  handler that keeps the exception for the type, which is then listed with it. Every run is on its own STA thread with a time limit.

## 3. The measurer, the scales and the self-sizing controls (sections 3.4, 5.3 and 5.4)

- **By control type** (`TextFitMeasurer.GetMethod`): `TextRenderer` for the stock Label (and LinkLabel), RadioButton, CheckBox, Button
  and GroupBox; `Graphics.MeasureString` for `UI.Button` (its text rectangle beside a picture or icon, `Button.DrawTextAndImage`),
  `UI.CheckBox` (12 px box scaled, text from 3 px after it, `CheckBox.OnPaint`) and `UI.GroupBox` (caption at 5 px, `GroupBox.OnPaint`).
  Other controls are not text-measured (text boxes are in the self-sizing group). Labels and buttons wrap words; a GroupBox caption does not.
  For a stock button-like control the room for the text is its size less what its own preferred size adds around the text (the glyph,
  the padding).
- **The 0.96 factor.** Under LayoutManager a control is measured with the font LayoutManagerForms gave it, so a Label or RadioButton
  carries the factor (`LayoutChildren`); `FontSizeUnderLayoutManager` gives it for a font that has not been through it. Plain forms get no
  factor, as in the product.
- **Kinds** (section 5.3): `height` (one line fits the width, not the height), `width` (room for one line only, or no wrapping),
  `line` (the wrapped lines need more height). A result is an overflow only when the same control fits in the before set (today's font):
  the TSVs list new overflows only, and the summary counts the ones that already overflow today apart.
- **The ink check.** For a height shortfall of 1 or 2 px the text is drawn the control's way (TextRenderer or DrawString, with its
  alignment) into a bitmap, unclipped, and ink outside the control's text box (its last row or column and beyond) means the cut shows
  (`ink=cut`); otherwise the line box is taller than the room but the glyphs fit (`ink=inside`). DrawString trims what does not fit the
  width, so for the program's own controls only the height can show ink past the box.
- **Free space and the verdict.** For each new overflow the free pixels to the right and below inside the parent, up to the nearest
  visible sibling beside or under it (a sibling that already overlaps it, a panel behind it, does not count); `fixable` when they cover the
  pixels missing, else `needs the owner`.
- **The scales.** A test process cannot change Windows' DPI, so LayoutManager windows are laid out with `SetScaleMS` at 1, 1.25, 1.5, 1.75
  and 2. At a scale LayoutManagerForms leaves most fonts at their zoom size and Windows would draw them larger, so the measurer measures
  with the control's font times the scale; the fully scaled types (TextBox, ComboBox, RichTextBox, TreeView, ListView, ListBox) already
  have the whole scale. Plain forms and the windows without LayoutManager are measured at 100% only (they do not scale through LayoutManager;
  the desktop check of section 8 covers them).
- **The zoom step.** `LayoutManagerForms.ZoomTest` cannot give a program zoom: `ScaleMy` adds it (`_scaleMS+ZoomTest/100`, 2.2 for 120)
  while `ScaleMyFont` multiplies it (`ZoomTest/100*_scaleMS`, 1.2), so bounds and fonts disagree (Open Dental's code; nothing calls it but
  commented-out lines and one UnitTests form). A zoom of 120% at 100% DPI gives every control the same pixel sizes as a Windows scale of
  1.2 (bounds by 1.2, fonts by 1.2, set by LayoutManager instead of Windows), so the scan's zoom step is scale 1.2.
- **Controls that size themselves** (`TextFitBoundsRecord.IsSizingItself`): AutoSize on at run time (designer lines, code, and a
  single-line text box's default), and the stock combo box, date picker and up-down box, whose height Windows takes from the font. At 100%
  their size is what WinForms gives them in the font under test. At other scales a label's or button's text part is measured with the
  scaled font and the rest scaled, a date picker's or up-down box's height likewise; the fully scaled types keep WinForms' size. A growth
  that newly overlaps a visible sibling or newly passes the parent's client edge is reported (a pair of two such controls once); `fixable`
  when the growth fits the free space around its old bounds, or for a text box when the new font still fits today's height less its 2 px
  borders; else `needs the owner`. Docked controls and children of a FlowLayoutPanel or TableLayoutPanel are left out (the panel places
  them).
- **The bounds record** (`TextFitBoundsRecord`): every control's `Bounds` (in its parent), client size, designed visibility (read from
  WinForms' own state, since no window is shown), whether it sizes itself, anchor, dock, its index, and whether a layout panel places it,
  in one TSV format (`TextFitBoundsRecord.HEADER`), for the before set and each font. The compare: a control that does not size itself must
  keep its bounds; one that does keeps the edge it is anchored to (the right or bottom edge when anchored only there) and may change its
  size; a layout panel's child keeps its order; a new overlap or parent-edge pass is a difference; every control `fit-fixes.tsv` lists is
  allowed. UI-0c's walker writes the same format.

## 4. How to run it

- Every Targeted run of UiRefreshTests builds all 1,026 types twice (today's font, and today's font again at 100% and 150%) and checks that
  the scan reports nothing new (`Scan_TodaysFont_ReportsNothingNew`): no new overflow, no new self-sizing overlap, no bounds difference, and
  every type that built once builds again.
- The full scan is opt-in: `OPMS_UI_SCAN=1` runs `Scan_FontOptions_WritesTheCounts` (Segoe UI 9 and 8.25, the six scales, the text-scale
  run). `OPMS_UI_SCAN_OUT` names the folder for the files (default: `ui0b-scan` in the test's output folder):
  ```
  set OPMS_UI_SCAN=1
  set OPMS_UI_SCAN_OUT=<a folder>
  powershell -NoProfile -ExecutionPolicy Bypass -File <od-tools>\run-tests.ps1 -Root . -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests
  ```
  It needs the whole filter built (the plain exes are read from their own build folders).
- **The files** (in `<folder>\options`, and the today run's in `<folder>\today`): `summary.txt` (the counts of section 6), `types.tsv`
  (every type, its build kind and status, the number of controls and of measured controls, the reason it was not built),
  `overflow-<run>.tsv` (new overflows: window, build kind, control path, type, scale, kind, the pixels of width and height missing, the ink
  check, the room and the text's one-line size, the free space, the verdict), `selfsizing-<run>.tsv` (the control, the sibling or
  `parent-edge`, its bounds before and after, the verdict), `bounds-<run>.tsv` (bounds differences at 100%), `bounds-record-<run>.tsv` and
  `bounds-record-before.tsv` (the bounds records at 100%), and `progress.txt` (the type being built, for a run that stops). The runs are
  `segoe9`, `segoe825`, `today-textscale125` and `segoe9-textscale125`, `segoe825-textscale125`.
- **The layout snapshot.** `LayoutSnapshotTests.Snapshot_MatchesTheDesignerFiles` reads every file with an `InitializeComponent` in the
  product folders (`SourceIndex.ArrayFolders`) and compares each control's `Location`, `Size` and `ClientSize` (the window or UserControl
  itself as `.`) with `Data/layout-snapshot.tsv`. A change that is not in `fit-fixes.tsv` fails it. A deliberate change (a new window, the
  Open Dental 26.2 port) writes the snapshot again with `OPMS_UI_LAYOUT_SNAPSHOT_WRITE=1` and commits it with the change. The writer keeps
  the line ends the working file already has (CRLF or LF; a new file gets LF) and writes UTF-8 without a BOM, so a rewrite never flips them:
  the blob is LF (no eol rule, core.autocrlf true), and `git add` stores either working copy as LF (`Write_KeepsTheFilesLineEndsAndNoBom`
  locks this in; `tools/Test-NoEolFlips.ps1 -Base main` stays clean). `fit-fixes.tsv`
  lists one control per row: the designer file, the window type, the control, the fix (`size` or `autosize-off`) and a note.

## 5. The test-side font swap and the font under test

- `TextFitFontSwap.Swap(window, option)` walks a built window and applies the rule of 3.4: Microsoft Sans Serif at 8, 8.25 or 8.5 pt
  becomes the option's body size in its family, another size keeps its number, the style is kept in full, other families stay; with
  today's family the mapping is the identity (the 8 and 8.5 pt designer fonts keep their size until the flip). The window's font and every
  container's are set too, from each control's font before the swap (a child that inherits already shows the new family once its parent
  has it). Under LayoutManager the swap changes the recorded `Control96Info.FontSize96`, as UI-2a-1's mapping will before it records it,
  and lays the window out again, so the rebuild gives labels and radio buttons the 0.96 factor and keeps only Bold, exactly as
  `LayoutChildren`. Content keeps its font: the content types (`ToothChartWrapper`, `ContrPerio`, `ODPrintPreviewControl`,
  `UserControlDashboard`) and the panels UI-2a-1 will mark (`FormSheetFillEdit.panelMain`, `FormSheetDefEdit.panelMain`,
  `FormClaimFormEdit.panel1`), with everything inside them.
- The scan also sets the process's default font to the option's body font (and to it times 1.25 in the text-scale run), as each exe's
  `Main` would: `Application.SetDefaultFont` refuses once a handle exists, so the scan sets the fields Application and Control keep. A
  FormODBase window sets its own font, so the default reaches only the plain forms and the UserControls they host; the text-scale run builds
  only those.
- From UI-2a-1 on the token override `UiTheme.SetFontForTest` takes the swap's place (5.4).

## 6. The measured counts (main at `e7f87b1d8`, 96 DPI)

"Windows" counts window and UserControl types (the plan's model counted files). Visibly cut: every new overflow but the 1-2 px height
cases whose ink stays inside.

| | Segoe UI 9 (UQ-1) | Segoe UI 8.25 |
|---|---|---|
| New overflows at 100% | 1,771 in 449 windows | 650 in 248 windows |
| ... visibly cut | 988 | 409 |
| ... only in height | 1,163 (1-2 px: 943, of which 160 cut, 783 ink inside) | 437 (1-2 px: 350, of which 109 cut, 241 inside) |
| ... need more width | 520 | 168 |
| ... need another line | 88 | 45 |
| ... fixable in free space / need the owner | 1,344 / 427 | 520 / 130 |
| Already overflowing in today's font (not counted above) | 660 | 667 |
| Self-sizing: new overlaps and parent-edge passes at 100% | 856 in 149 windows (791 overlaps, 65 edge) | 587 in 110 windows (585, 2) |
| ... fixable / need the owner | 782 / 74 | 547 / 40 |
| Bounds differences at 100% | 692 in 86 windows (567 bounds, 125 anchored edge) | 96 in 46 windows (96 bounds) |

**By control type at 100%.** Segoe UI 9: Label 791, UI.CheckBox 600, RadioButton 231, Button 83, UI.Button 38, CheckBox 24, LinkLabel 2,
UI.GroupBox 2. Segoe UI 8.25: Label 200, RadioButton 195, UI.CheckBox 145, Button 89, CheckBox 13, UI.Button 6, LinkLabel 2.
Self-sizing, Segoe UI 9: TextBox 515, ValidDate 154, ValidDouble 75, ValidNum 47, Label 28, ComboBox 15, DateTimePicker 9, ValidPhone 9,
NumericUpDown 2, DomainUpDown 1, RadioButton 1; Segoe UI 8.25: TextBox 414, ValidDate 82, ValidDouble 41, ValidNum 24, Label 10,
ValidPhone 5, ComboBox 4, DateTimePicker 2, NumericUpDown 2, RadioButton 2, DomainUpDown 1.

**By build kind (Segoe UI 9, new overflows at 100%).** FormODBase windows 1,348; preference and wizard pages (AddUnscaled) 226; other
hosted UserControls 68; plain forms 66; plain-hosted UserControls 62; the windows without LayoutManager 1.

**The largest windows at 100%.** Segoe UI 9: FormPatientEdit 65, FormProvEdit 38, FormClaimEdit 31, ControlAccount 22, FormProcEdit 21,
FormAllocationsSetup 19, FormEmailInbox 19, FormReportSetup 19, UserControlApptGeneral 19, UserControlFamilyGeneral 19, FormInsPlan 18,
UserControlAccountClaimReceive 18, ControlChart 17, FormFamilyBalancer 17, FormProcCodeEdit 16, UserControlChartGeneral 16. Segoe UI 8.25:
ControlChart 13, FormClaimEdit 13, FormInsPlan 13, FormRpCustomAging 11, FormRpUnearnedIncome 11, FormAllocationsSetup 10, FormRpPaySheet 10,
DialogNewMatrix 9. Self-sizing, Segoe UI 9: FormEhrLabResultEdit2014 34, FormInsPlan 33, FormPatientEdit 33, ControlAccount 31,
FormClaimProc 28, FormClaimEdit 25, FormInsRemain 24, FormClinicEdit 22, FormProcEdit 22, FormOrthoCase 21.

**Bounds differences.** Segoe UI 9: plain forms 460 in 27 windows and plain-hosted UserControls 143 in 14 (the `AutoScaleMode.Font`
containers of section 2 rescale by the new font's metric: FormCentralManager 57, GraphQuantityOverTimeFilter 53, FormCentralPatientSearch 42,
FormDashboardEditTab 34; UI-2a-2 changes them), FormODBase windows 76 in 40 and hosted UserControls 13 in 5: controls that lay out their
own children from the font (`ODDatePicker`'s drop button, the up-down boxes' parts, `SignatureBoxWrapper`'s buttons). Segoe UI 8.25 keeps the
6×13 metric, so no plain form rescales; 96 differences of the second kind.

**At the other scales** (LayoutManager windows and hosted UserControls; new overflows, self-sizing):

| Scale | Segoe UI 9 | Segoe UI 8.25 |
|---|---|---|
| 1.2 (also the zoom step 120%) | 1,627 in 394 windows; 765 | 500 in 166; 401 |
| 1.25 | 1,428 in 364; 567 | 603 in 183; 377 |
| 1.5 | 2,444 in 486; 360 | 339 in 116; 51 |
| 1.75 | 2,333 in 473; 102 | 551 in 159; 32 |
| 2 | 1,801 in 420; 72 | 888 in 251; 38 |

The counts do not fall or rise steadily with the scale: whole-pixel font heights round differently at each step (a 13 px label holds a
12.4 px line at one scale and not at the next). The self-sizing counts above 100% are modelled (section 3); the real desktops at 125% and
150% are checked by hand (section 8 of the plan).

**The text-scale run** (Windows text size 125%, at 96 DPI; the plain forms and the UserControls they host): today's font 257 new overflows in
50 windows (mostly RdlDesign's dialogs and Central Manager), 10 self-sizing, 601 bounds differences (today's `AutoScaleMode.Font` layout grows
with the text, as it does now); Segoe UI 9 299 in 53 windows, 35 self-sizing, 603 bounds differences; Segoe UI 8.25 290 in 50 windows, 31
self-sizing, 603 bounds differences. The counts are against the before set at text size 100%. UI-2a-2 reruns it after the
change to `AutoScaleMode.Dpi`, which stops the layout growing with the text size.

## 7. What the scan cannot build, and why

| Type | Why |
|---|---|
| `FormFeeSchedTools` | needs a database: a `ComboBoxClinicPicker` reads the clinics preference in `OnLoad` when a hidden tab page with a handle is shown |
| `FormRecallList`, `FormSchedule` | need a database: their designer code reads it (`DbConnectionRefusedException`) |
| `FormEhrCarePlans`, `FormEtrans835ClaimEdit`, `FormEtrans835ProcEdit`, `FormMountDefEdit`, `FormSheetDefEdit` | throw: an event handler that the designer code fires uses a field their own constructor sets (NullReferenceException) |
| `FormEhrLabImageEdit` | throws in the product too (Debug): "MS SplitContainers are not allowed" (`LayoutManagerForms.SetNoAnchorDock`) |
| `UserControlSecurityUserGroup` | throws in the product's Debug check too: a control added outside LayoutManager (`panelUserGroups`) |
| `ControlImages`, `UI.GridOD`, `UI.GridOld`, `UI.ODDatePicker`, `UI.ToolBarOD`, `SparksToothChart.ToothChartWrapper`, `CodeBase.PrintPanel`, `fyiReporting.RdlDesign.DataSetRowsCtl` | throw when built on their own (their constructor sets fields their size or layout handlers use); inside the windows that use them they are built by the designer code with their constructor, and scanned there |
| `CodeBase.FormProgressBase`, `ModuleSwitchBlurOverlay`, `OpenPmsDentalDbSettings.FormDbSettings` and `FormSecretPrompt`, RdlDesign's `DesignEditLines` and `DesignXmlDraw`, RdlViewer's `PageDrawing` | no designer code: their controls are made in their constructor |
| OpenPmsDentalHelper32 | a 32-bit process; no designer files |

UI-0c's harness opens windows by their own handlers on the synthetic database, so it covers the ones that need one.

## 8. Tests and runs

`tests/UiRefreshTests` (22 tests):
- `BuilderAndScanTests`: the guard refuses before the network; `FormInstallAllocator_Provider` (and the other four) built without
  `InitializeLayoutManager` (no LayoutManagerForms after the build, no handle), `FormPreferences` with it; the build kinds of `ControlImages`,
  a preference page, `ControlAccount`, a graph UserControl and the plain exes' forms; a test window with an AutoSize label next to a text box
  reports the overlap when the label's text grows (allowed once in the fit list) and no difference when the other label grows into free
  space; the scan on today's font reports nothing new; the opt-in full scan.
- `FontSwapTests`: the rule of 3.4 font by font; the swap on a test FormODBase window gives the rule's fonts with the 0.96 factor and the
  Bold-only rebuild, sets the window's and the containers' fonts, updates LayoutManager's records (the font holds at 150%), and changes
  nothing a second time; on a plain window it keeps the full style; a hosted `ControlAccount` (19 explicit designer fonts, `Add`) and a hosted
  preference page (`UserControlAccountRecAndRepCharges`, 5, `AddUnscaled`) get Segoe UI on every explicit designer font.
- `MeasurerTests`: the method by control type, the factor, a label that fits, wraps, needs width, another line or height, `UI.Button` by
  MeasureString in its text rectangle, the ink check both ways, the classification, the bounds record's format, the self-sizing rule and the
  free space.
- `LayoutSnapshotTests`: the snapshot against the designer files, and the parser on a sample.

Runs (x64, `-RequireBuildOutput`): see the result files `.git/od-tools/test-results/ui0b-*.txt`; the last Targeted run, UiRefreshTests 22/22
and NoHqTests 115/115; with `OPMS_UI_SCAN=1` the full scan passed too (the counts above). Full filter x64: 0 errors.
