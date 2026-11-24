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

No finding open.
