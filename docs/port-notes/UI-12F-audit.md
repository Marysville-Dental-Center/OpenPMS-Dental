# UI-12F audit (the final consistency pass, gallery G-UI3-ui12f)

Every after picture was opened once (contact sheets of the changed band of each window, before over after, `<W>/audit-100*`; the 150%
pictures whole, `<W>/audit-150`), then only the pictures a fix changed. `<W>` = `.git/od-tools/test-results/G-UI3-ui12f-src/work`.
Package notes: [UI-12.md](UI-12.md) section 7, "Final consistency pass (UI-12F)".

## First pass (the bar rule as first written: every bottom-anchored control and OK/Save/Cancel/Close in the bottom 60 px)

| Window(s) | Seen | Fix |
|---|---|---|
| FormClaimPayTotal, FormEtrans835ClaimPay | the Deductible and Write-off captions left 12 px over their buttons when the bar moved down | a caption 0 to 4 px over a bar button belongs to the bar |
| FormCounties, FormQuestionDefs, FormRpArizonaPrimaryCareEligibility and Encounter, FormRpOutstandingIns, FormImagePickerDXC | a column of buttons beside a list torn apart (the lowest button moved 8 px, the others stayed) | a row the bar shares with other controls is no bottom bar |
| FormDisplayFieldEdit, FormProcCodes, FormConfirmationSetup | a field row with Save beside it moved away from the rows above | the same (shared row); bottom-anchored rows of labels, fields and check boxes stacked on the bar are one bottom block |
| FormAbout | (dry run) copyright lines chained into the bar as captions | captions only over bar buttons |
| FormReconcileAllergy | (second pass) the bottom block climbed through bottom-anchored buttons and lists and shortened the upper lists | the bottom block takes only labels, fields and check boxes 30 px tall or less; a button only on a bar button's column |
| FormRpTreatmentFinder, FormMaterialsUsed | two rows of bar buttons 12 px apart; Lot Number 12 px over Cancel | a second row of bar buttons on the first row's columns joins the block |
| FormSheetDefEdit | Grid Snap 131 px wide in the wider tool column | the three button columns share the column (103 px each) |
| FormInstallAllocator_Provider | the REPORTS header's white text on the program's light button (invisible) once the window got its LayoutManager | `UiLooks.IsOwnColours`: the designer's own back and fore colours |

## Final pictures

- **Bar gaps** (170 windows): grids, lists and notes 4 to 8 px shorter, or the bar 2 to 10 px lower in a window as many px taller; the
  content 12 px over the bar in every one; columns of buttons, captions and stacked bottom rows kept together. The windows the rule no
  longer touches are as on main.
- **Main's size** (10 windows): slice 10's six with equal gaps (FormRxAlertEdit 30 px between its blocks, FormTranslationEdit 14,
  FormRepeatChargeEditMulti and FormSigElementDefEdit about 10, FormReplicationEdit 8; FormTrackNextSetup's group spans the content);
  FormEhrEncryption's boxes wider with 12 px margins; FormHL7DefMessageEdit's and FormPatientStatusTool's grids down to 12 px over the
  bottom row; FormPaySplitEdit's right column at the wider edge.
- **Labels** (16): each label's text on its field's centre (ControlChart's Movements page is not drawn by default: checked in the
  layout snapshot and by GridRulesTests).
- **150%**: the report designer's ChartAxisCtl, InteractivityCtl, PositionCtl, ReportCtl, ReportParameterCtl, StyleBorderCtl, StyleCtl
  and StyleTextCtl show every text whole (main cut "Major Tick", "Styl", "Wid", "Reverse Dir", "Nam", "Alignm"); the allocator windows
  scale with LayoutManager (main: fonts larger in unscaled bounds, "Provider Inco", "Run Allocator", "View Lo"); the stock date pickers
  take the font's height.
- **Unchanged pictures** (6, `gallery.tsv`): ControlChart (its changed Movements page is not drawn) and windows whose change is a
  browser or picture area 4 to 8 px shorter that the harness draws blank.

No finding open (first gallery).

## Fix round (branch `ui/ui12f-2`, the review of G-UI3-ui12f)

The thirteen findings fixed; every picture the fix changed drawn again (`<W>/fix-100`, `<W>/fix-100b`, `<W>/fix-150`, the build's
OpenPmsDental.dll the same SHA-256 in both bins) and opened once, gaps measured on the pictures (rows and columns of ink).

| Window | Finding | After (measured) |
|---|---|---|
| FormProgressViewer | size limits set before the dpi was applied | set after FormODBase.OnLoad (test at 100 to 250%); pictures unchanged (the harness runs no OnLoad) |
| FormWarnToCloseComputers (100%, 150%) | Arial Black and Arial bold; dark frames on panel1 and the image animator | Segoe UI bold, the message in four lines at both scales, no dark frame |
| FormInstallAllocator_Provider (150%) | REPORTS 143 x 23 in the 215 px panel | REPORTS spans the panel, flush on Provider Income; the four rows fill it at 100% and 150% |
| FormFeesForIns | not in the gallery | before and after added: the grid ends 12 px over the Plan Type row (8 on main), the two field rows and the bar as on main |
| FormClaimOverpay | Totals row detached from its grid | grid 8 to 294, Totals 299 to 321 (4 px under it), the bar 334 (12 px), window 367 |
| FormEncounterTool | the gap moved with the bar | Code to the warning as on main, warning box to Run 12 px (its ink 16), Run 8 px from the bottom |
| FormImageCatMerge | window grown with a 45 px empty band | 600 x 249: groups 8 to 83 and 112 to 187, Merge 217 to 240 (gaps 28 and 29, 8 px under Merge) |
| FormEhrEncryption | 24 px band under Transmit, 12/13 px margins | boxes 29 to 270, Transmit 283 to 310 (12 px, 8 from the bottom), boxes and Transmit 8 px from both sides |
| FormRepeatChargeEditMulti | gaps 11 to 19 | 14, 14, 14, 14, 14 between the six rows, 16 over Run |
| FormReplicationEdit | gaps 8 to 17 | 10, 10, 10, 10, 10, 11 between the seven rows, 16 over the bar (box gap 12) |
| FormRxAlertEdit | 30, 35, 35 | 33, 33, 34 |
| FormHL7DefMessageEdit | grid and Note end 9 px short of Save | grid, Note and Save end at 717, the grid and Delete start at 8 |
| FormPatientStatusTool | margins 19 and 12 | Criteria group, grid and Select All from 8; Create List, grid and Run to 1011 |
| FormSigElementDefEdit | (keep_main's drawn gaps) | 16, 16, 16, 17 between its blocks (15 to 18 before) |

No finding open.
