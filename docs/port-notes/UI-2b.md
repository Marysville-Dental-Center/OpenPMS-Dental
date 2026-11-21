# Port note: UI-2b (the flip to Segoe UI)

Package UI-2b of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7 "UI-2b: the flip", sections 3.4,
5.1, 5.2, 5.4, 8 and 9). Branch `ui/ui2b-1` from `main` at `6792a43d9`. No schema change, no new package, nothing downloaded, no program
started, no window shown. The print guard's tests use its own scratch databases (`ui2a3_*`), dropped by the guard.

Commits, one per step: the fit gallery draws in the program's font on request (`4d8a5ff5a`); step 1, the pins; step 2, the flip; step 3,
the scan and the last size fixes, then the M2 text-fit tests; step 4, the gallery and these notes.

## 1. The pins (step 1)

Every site of the UI-2b checklist (docs/port-notes/UI-2a.md, "UI-2a-4: the lead's decisions and the UI-2b checklist" and UI-2a-5 section 5,
UI-2a-6's note) that drew a printout or office content with an inherited font now names today's font, so the flip cannot reach it.
`CodeBase/UI/UiTheme.Fonts.cs` region "The pins": `FONT_SIZE_PINNED` (8.25), `NewFontPinned()` (Microsoft Sans Serif 8.25 Regular) and
`NewFontPinned(Font)` (Microsoft Sans Serif at an inherited font's scale and style: its size times 8.25 over FontBody's size, so a window's
zoom is kept; with today's tokens exactly the inherited size).

| Site | Kind | Pin |
|---|---|---|
| `FormEquipment.pd_PrintPage` "Total Est Value" | printout | `NewFontPinned()` |
| `FormSheetDefEdit` `DrawInsurance` "(FieldName)" and `DrawSpecial` "(Special:Tooth Grid)" | sheet design canvas | `NewFontPinned()` |
| `SignatureBox.GetSigImage` (the digital signature's picture that sheets print and save, the treatment plan saves, the PayConnect receipt takes) | document | `NewFontPinned(Font)` (the box's zoom kept) |
| `Legend.PrintCopy` (the dashboard graph's printed and exported legend) | printout | `Font=NewFontPinned()` on the copy |
| `SheetEditMobileCtrl` previews (lines 1318-1512, sheet design content) | content | `panelPreview.Font=NewFontPinned()` and `LayoutManagerForms.MarkContent(panelPreview)`; the editor's own label, check box and buttons follow the body font |
| `CodeBase/Utilities/MsgBoxCopyPaste.Print` (the message box's printout) | printout | `NewFontPinned()`, and its 16 and 14 Bold heading fonts name `FONT_FAMILY_TODAY` |
| `FormMountDefEdit` line 343 (the editor's label of an item without text) | chrome | FontBody's family and size at the zoom; 380-382 follow the window font; 358 and 387 stay as `MountHelper` draws (GenericSansSerif) |
| `FormPreferences.PanelInfo_Paint` (Times New Roman), `FormEligibilityResponseDisplay` (Tahoma) | own families | unchanged |

**The WPF drawing `Font` (`WpfControlsOD/UI/Graphics.cs`, the default `Name`).** Its default now follows the body family
(`UiTheme.FontFamilyName`), and `Font.ForDocument()` keeps Microsoft Sans Serif 8.25. Its callers, from a grep:

| Caller | Kind | Font |
|---|---|---|
| `FrmChildCareMap.pd_PrintPage`, `FrmFriendlyException.pd_PrintPage`, `FrmMsgBoxCopyPaste.pd_PrintPage` | printouts | `Font.ForDocument()` |
| `FrmMountLayoutEdit` 439 and 470 (a mount item's own text, as `MountHelper` draws it) | mount content | `Font.ForDocument()` at the item's size |
| `FrmMountLayoutEdit` 430 and 461 (the editor's label of an item without text) | chrome | the default (body family), as `FormMountDefEdit` |
| `Graphics.MeasureString(text)` without a font: `FrmPatientAddAll.cs:1969` (fits a referral name into a WPF text box), `ZoomSlider.xaml.cs:218-221, 326` | screen | the default (body family) |
| `InputBox.xaml.cs:624`, `CtrlEFormFill.xaml.cs:2545, 2761` | screen / eForm | `Font.ForWpf()`, unchanged |

A grep of every other `DrawString`/`MeasureString` with an inherited `Font` (153 lines) and of `DefaultFont` found no other printout or
content: `FormEhrGrowthCharts` draws only on screen, the EHR patient lists measure grid columns, RdlDesign's `DesignCtl` draws its error text.

**Tests.** `tests/PrintGuardTests/PinnedFontPrintGuardTests.cs` renders the equipment printout (the window's own `FillGrid` and page event,
scratch database), a digital signature's picture (on its own and in a LayoutManager window) and the legend's print copy with today's fonts and
with UI-2b's: the same bytes (each would follow the body font without its pin). `tests/UiRefreshTests/FontPinTests.cs`: a source check of every
pinned site (the pin is there, no inherited `Font` is read), the pin helpers in all three font options and at zoom 120%, the mobile layout
previews (pinned, marked, a later preview inherits; the editor follows the body), and the WPF `Font` default and `ForDocument`.

## 2. The flip (step 2)

- **Tokens.** `UiTheme.FontOptionProgram` is `UiFontOption.UI2b`; the token set starts there and `SetFontForTest(null)` gives it back.
  Body Segoe UI 9 (labels and radio buttons at 0.96, `LayoutManagerForms` as before), grid cells and headers at the body size (rows 17 px,
  header 18, editable rows 22, `GridScreenLook`; UQ-2), button panel Segoe UI 9, titles Segoe UI Semibold 10, module captions Segoe UI 8,
  window titles Segoe UI. The mapping applies the rule of 3.4 (Microsoft Sans Serif 8, 8.25 and 8.5 become 9; other sizes keep their number).
  `UiFontOption.Today` stays: the before set of the scan, the galleries and the print guard.
- **The 12 exe `Main`s**: `Application.SetDefaultFont(new System.Drawing.Font("Segoe UI",9f))`, still the first statement (the text scale
  factor still applies); `NoHqTests/DefaultFontTests` checks each against the token (its runtime test renamed `..._HasTheBodyFont_...`).
- **Literal designer fonts** of the plain forms and of the allocator windows without `InitializeLayoutManager` (17, by the rule of 3.4):
  `FormInstallAllocator_Provider` 7 (Segoe UI 9 Bold), `FormReport_GuarantorAllocationCheck.lblTitle` (Segoe UI 14.25),
  `FormReport_ProviderIncomeReport` (the form's own Segoe UI 9 Bold), Central Manager `labelVersion`, `labelFetch` (2), `labelSourceDb` (9
  Bold), xCrudGenerator `label2` (10 Bold), RdlDesign `DataSetRowsCtl.label1` (6) and `DialogAbout.label8` (12 Bold), CodeBase
  `FormProgressStatus.labelMinimize` (9 Bold). `Literals_ControlAndFormFontsOfPlainForms_FollowTheToken` passes.
- **AutoScaleDimensions** (5.1): no form took the fallback (every plain form is on `AutoScaleMode.Dpi` 96,96 since UI-2a-2), so none changes.
- **FormClaimPrint and FormTPsign**: the mapping gives their Microsoft Sans Serif 9 Bold labels Segoe UI inside `InitializeLayoutManager`,
  which lays the window out before `LayoutManager` is set; their `Layout` handlers threw (the scan had listed both as "segoe9 throws" since
  UI-0b). Each handler now returns while `LayoutManager` is null (Open Dental's own pattern, `FormODBase.InitializeLayoutManager`'s comment); a
  test builds both with today's and the program's fonts (`FontTokenTests.Windows_ThatLayOutDuringTheMapping_BuildWithTheProgramsFonts`).
- **net10-plan**: decision 9, the S3b-4 row and the "Default font" row say that the UI refresh plan replaces them.

**Tests that named today's font** (expectations updated, nothing deleted): the token tests expect Segoe UI 9 and check today's option
separately; the print guard, the scan's before set and today's run, the design sheet, the grid look tests named "TodaysFont", the tab strip
and toolbar fit, UI-0b's font swap and the font-literal rule tests name `UiFontOption.Today`; the colours test of the grid reads the program's
row heights (20, 18, 17); `PictureMapTests` builds with the program's default font (a window built with a default font unlike its tokens can
differ: `FormPayment`'s tab strip then selects a tab inside `InitializeComponent`); `EhrTests.MeasureNotes` measures in the body token;
the image-list grid test compares two grids scaled the same way; the exe rows leave `font-literal-allow.tsv` and `font-literal-sites.tsv` is
written again.

## 3. The scan at the program's font (step 3)

`Scan_FontOptions_WritesTheCounts` now runs Segoe UI 9 as the program's own font, with no token override (`ScanOptions.IsProgramFont`;
`OPMS_UI_SCAN_OVERRIDE=1` puts it through the override as before), over the whole scan list (1,026 types, 1,001 built, as in UI-0b; the 15
that throw are the same builder limits as before, `FormClaimPrint` and `FormTPsign` now build in Segoe UI too).

The first run (`.git/od-tools/test-results/ui2b-scan`) reported 136 items at 100% that no UI-10 needs-owner list held: the "(no designer
file)" row of the UI-10 ranking (rank 675: 121 overflows, 2 self-sizing, 12 bounds), which no slice had taken. They are windows whose designer
code sits in their own `.cs` file (the report designer's 31 dialogs and controls, `ODR/FormParameterInput`, `CodeBase/Utilities/MsgBoxCopyPaste`,
Central Manager's `FormCentralChooseDatabase`, `ODGridPageNav`) or whose designer file is named otherwise than the type (`FormProcCodes` in
`FormProcCodes2.designer.cs`, `FormCvxs` in `FormCvx`, `FormMySQLUserManager` in `FormMySQLManager`, `FormConfirmationSetup`,
`FormProvidersMultiPick`). `tools/UiFit/fit_tools.py plan --code-files` now finds those files; the fixes are size-only as in UI-10:
**90 rows in `fit-fixes.tsv`, 33 files**; a second pass planned nothing. What is left is in
**[UI-2b-needs-owner.tsv](UI-2b-needs-owner.tsv)** (50 items: 34 texts with no free space, 13 grid scroll bars and up-down parts that
are their control's own layout, 3 designer lines the tool cannot edit: two `Size` lines that appear twice in `ChartAxisCtl` and one missing in
`ChartCtl`).

**The scan after the fixes** (`.git/od-tools/test-results/ui2b-scan-pass1/options/summary.txt`), Segoe UI 9 with no override:

| At 100% | Before UI-2b's fixes | After |
|---|---|---|
| New overflows | 536 in 233 windows | 450 in 219 windows (13 fixable by the tool's rule but too small to plan, 437 need the owner) |
| ... 1-2 px of height with the ink checked: cut / inside | 32 / 122 | 24 / 118 |
| Self-sizing overlaps and edge passes | 132 in 42 windows | 130 in 40 windows |
| Bounds differences (not in `fit-fixes.tsv`, outside the self-sizing rule) | 604 in 417 windows | 604 in 417 windows (grid scroll bars and up-down parts: their control's own layout) |
| Not on any needs-owner list | 136 | **0** |

Other scales (new overflows): 1.25: 462, 1.5: 1,223, 1.75: 1,247, 2: 540 (mostly 1-2 px of height with the ink inside, as in UI-10). Segoe UI
8.25 (UQ-1's other option): 142 at 100%. The text-scale run (Windows text size 125%, the plain exes): today's font 308 new overflows in 66
windows, Segoe UI 9 429 in 78 windows (these windows take the default font and, on `AutoScaleMode.Dpi`, no longer grow their layout with it,
plan 5.1's trade-off; G-UI4 pictures them).

The bounds record: the scan compares every control's bounds at 100% with the before set (today's font, same designer files) and reports only
what is outside `fit-fixes.tsv` and the self-sizing rule; all 604 are on the needs-owner lists (scroll bars). The harness's group U record
against `.git/od-tools/test-results/ui-before` needs a connected session (section 6).

**The M2 text-fit tests** (`M2INSPLANTests.LayoutDesignerTests.EveryText_FitsItsControl_At96Dpi`,
`M2TERMTests.TerminologyLayoutTests.PreferenceCheckBoxes_TextFits`) measure in the body token, so the flip showed real clipping, as plan
section 9 foresaw. FormInsPlan's labels are now measured at the 0.96 factor LayoutManager draws them with; a control on a needs-owner list
(`labelAssign`, `checkOrthoWaitDays`, `label15` of FormInsPlan, `checkEnableZeroWriteoffOnLimitations` of the preference page) is reported,
not failed; any other text that does not fit still fails.

## 4. Decisions for the owner's review

Taken by the safer or recommended answer (owner, 2026-11-21: build everything, review at the end):

1. **The digital signature on screen follows the window; its saved and printed picture keeps Microsoft Sans Serif 8.25.** The lead's note of
   2026-11-18 said to pin both; the UI-2b brief (newer) recommends the screen follow the window. Taken: the brief. A signature box in a
   filled sheet therefore shows Segoe UI on screen and prints in today's font. If the owner wants screen and paper to match, `OnPaint` takes
   the same `NewFontPinned(Font)`.
2. **MsgBoxCopyPaste's printout (CodeBase) is pinned.** UI-2a-6 sorted the class as chrome; its `Print` method is a printout, so by UQ-5 it
   keeps today's font. Its window follows the body font.
3. **The size fixes of the "(no designer file)" windows** (the report designer's dialogs and the others of section 3) were made in UI-2b
   with the UI-10 tool rather than left clipped, so `main` shows no new clipped text after the flip (plan section 7). G-UI3 has no pictures of
   them yet (section 6).
4. **FormClaimPrint and FormTPsign**: a null check in their `Layout` handlers, the smallest change that keeps them opening after the flip.
5. **The ODtextBox spell check tests** keep Microsoft Sans Serif 8.25 on their own text box: their counts and wavy-line geometry are written
   for 13 px lines, and the spell check does not depend on the family. The test process's default font is the token (Segoe UI 9).
6. **The M2 text-fit tests** accept a control on a needs-owner list (it is the owner's decision); everything else stays strict.
7. **The WPF drawing `Font`'s default** follows the body family for screen drawing; the three WPF printouts and the mount item text are
   pinned (`Font.ForDocument`).
8. **Unchanged by decision**: the appointment book's fonts (UQ-8), the tooth chart's pins, the perio chart and its claim attachment, GridOD's
   print fonts (UQ-5), the chart element fonts of the graph, `FormPreferences.PanelInfo_Paint` and the eligibility grid's own families.

## 5. Tests and runs

- The build lane, x64, the whole filter: 0 errors (`odbuild-20261121-101647`, after the size fixes; again at the end, section 7).
- Step 1 (`ui2b-step1-1.txt`): PrintGuardTests 19 passed, 1 opt-in skipped (the eForm PDF); UiRefreshTests passed but for the two font-scan
  data rows, fixed in the commit (the WPF `Font`'s literal row left the allow list).
- After the flip (`ui2b-flip-4.txt`): UiRefreshTests 260 passed, 14 skipped (opt-in); PrintGuardTests 19, 1 skipped; WindowChecksTests 30, 44
  opt-in skipped; UnitTests 1,728, 3 skipped; EhrTests 328, 2 skipped; NoHqTests 127.
- The scan (`ui2b-scan-1.txt`, `ui2b-scan-2.txt`, with `OPMS_UI_SCAN=1`): UiRefreshTests passed both times; M2INSPLANTests and M2TERMTests
  failed on the clipping of section 3, then passed (`ui2b-m2-1.txt`: 31 and 21).
- The final run is in section 7.
- Not run here (WPF pictures, or a shown window; the owner's desktop session is disconnected): ToothChartImageTests, ToothChartHostTests, the
  opt-in window checks (UF1 and group U), the eForm PDF guard. See section 6.

## 6. For the lead

**Gallery G-UI2, font pass** (made here, off screen): `.git/od-tools/test-results/G-UI2-ui2b` (and `.zip`), from
`G-UI2-ui2b-src/before` (main, today's font: 22 pictures drawn before this branch changed the program, pixel for pixel the same as this branch drawn with today's font; the three dialogs `FormApptEdit`, `FormInsPlan`,
`FormPayment` drawn from this branch with today's font) and `G-UI2-ui2b-src/after`
(this branch, the program's own font). 25 pictures, 23 changed. `ControlImages` and `FormOpenDental` come out the same: the off-screen
builder cannot fill them (the Imaging module needs its own constructor, the main window adds its modules at run time). The commands (each
build of UiRefreshTests together with OpenPmsDental, so its bin holds the right `OpenPmsDental.dll`):

```
odbuild.ps1 -Root <worktree> -Platform x64 -Projects OpenPmsDental,UiRefreshTests -ScopedRestore
set OPMS_UI_FIT_GALLERY=<folder>
set OPMS_UI_FIT_GALLERY_WINDOWS=OpenPmsDental.FormOpenDental,OpenPmsDental.ControlAppt,OpenPmsDental.ControlFamily,OpenPmsDental.ControlAccount,OpenPmsDental.ControlTreat,OpenPmsDental.ControlChart,OpenPmsDental.ControlImages,OpenPmsDental.ControlManage,OpenPmsDental.FormPatientEdit,OpenPmsDental.FormClaimEdit,OpenPmsDental.FormProcEdit,OpenPmsDental.FormApptEdit,OpenPmsDental.FormInsPlan,OpenPmsDental.FormPayment,OpenPmsDental.FormPreferences,OpenPmsDental.UserControlMainWindow,OpenPmsDental.UserControlMainWindowMisc,OpenPmsDental.UserControlApptGeneral,OpenPmsDental.UserControlApptAppearance,OpenPmsDental.UserControlFamilyGeneral,OpenPmsDental.UserControlAccountGeneral,OpenPmsDental.UserControlTreatPlanGeneral,OpenPmsDental.UserControlChartGeneral,OpenPmsDental.UserControlImagingGeneral,OpenPmsDental.UserControlManageGeneral
set OPMS_UI_FIT_GALLERY_FONT=program        (on main: today's font; on this branch: Segoe UI 9)
run-tests.ps1 -Root <worktree> -Mode Targeted -Platform x64 -RequireBuildOutput -Include UiRefreshTests
tools\New-UiGallery.ps1 -Before <before folder> -After <after folder> -Out .git\od-tools\test-results\G-UI2-ui2b -Name G-UI2 -Note "..."
```

`OPMS_UI_FIT_GALLERY_FONT=today` on this branch draws today's font through the override (the check that the before set is main's).

**In the connected session** (WPF draws nothing while the session is locked):
- The main window at 1366x768 and 1920x1080, the About box, the menu bar, the seven modules with and without a patient (the dashboard open
  with a synthetic widget), each also at one zoom step, through the harness's group U (`OPMS_WINDOW_CHECKS=1`,
  `OPMS_WINDOW_CHECKS_GROUPS=U`, `run-tests.ps1 ... -Include WindowChecksTests`) against the before set
  `.git/od-tools/test-results/ui-before`, for G-UI2 and the bounds record (it may differ only by `fit-fixes.tsv` and the self-sizing rule).
- UF1 (the signed note after a zoom change, a resize and a save): `OPMS_WINDOW_CHECKS_GROUPS=UF`.
- ToothChartImageTests and ToothChartHostTests: the 1,682 + 18 tooth chart baselines must stay identical (the chart's pins).
- The eForm PDF guard: `OPMS_PRINT_GUARD_WPF=1`, `-Include PrintGuardTests`.
- G-UI3 pictures of the windows section 3 fixed (the report designer's dialogs are plain forms: the fit gallery draws them with
  `OPMS_UI_FIT_GALLERY_WINDOWS=fyiReporting.RdlDesign.DialogNewTable,...` and `OPMS_UI_FIT_GALLERY_OWNER=docs\port-notes\UI-2b-needs-owner.tsv`).

**The owner's desktop check at 125% and 150%** (plan section 8), on a real desktop with this build: Settings > System > Display > Scale
125%, sign out and in, start the program on the test database; look at the main window, two modules (Appointments and Account), the patient
edit window (Family > Edit Patient), a toolbar, the main menu, a grid with a selected row (Account), the graph dashboard (Reports > Graphic
Dashboard, `FormDashboardEditTab`) and Central Manager's main window; note "fine" or what looks wrong. Then the same at 150%. Also once with
Settings > Accessibility > Text size 125% at 100% scale: the payment window (Account > Payment) must open (the builder's `FormPayment` fails
when the default font differs from the tokens; the product sets both from the same font, but the text size scales only the default), and
Central Manager's main window (plain forms keep their layout and grow only their text, plan 5.1).

**A follow-up**: the 50 items of `UI-2b-needs-owner.tsv` go to the owner with the other needs-owner lists.

## 7. Final run

The build lane, x64, the whole filter: 0 errors (`odbuild-20261121-104105`). Targeted (x64, `-RequireBuildOutput`,
`.git/od-tools/test-results/ui2b-final.txt`), all passed: UiRefreshTests 260 (14 opt-in skipped), PrintGuardTests 19 (1 opt-in skipped,
the eForm PDF), WindowChecksTests 30 (44 opt-in window checks skipped), UnitTests 1,728 (3 skipped), EhrTests 328 (2 skipped), M2INSPLANTests
31, M2TERMTests 21, NoHqTests 127. `tools/Test-NoEolFlips.ps1 -Base main`: 0 flips. No scratch database left.
