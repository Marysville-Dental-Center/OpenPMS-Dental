# UI-11 audit (the remaining background colours, gallery G-UI3-ui11)

Every after picture was opened once: contact sheets of all 107 after pictures (six per row), then the changed windows whose change was
largest or least expected one by one (ControlAppt, ControlChart, FormBackup, FormInsPlan's diff, FormClaimFormEdit, UserControlAccountGeneral,
UserControlScreenTooth's diff). Then only the changed pictures again. `<W>` = `.git/od-tools/test-results/G-UI3-ui11-src`.
Package notes: [UI-11.md](UI-11.md).

## First pass

| Window(s) | Seen | Fix |
|---|---|---|
| UserControlScreenTooth | the screening tooth's grey between its cells changed (6% of the picture): its window row was mapped, because the file reason named `User Controls/` while the file is in `SheetFramework/` | the reason names the right folder; the designer line is back (content: the screening chart on a sheet); the picture left the gallery (no change) |
| The module pages, the 28 preference pages, UserControlDashboard | white to the window background token: their group boxes, check boxes and labels now on Surface like every other window; the grids and inputs stay white (SurfaceRaised) and stand out as in the other windows | none |
| FormInsPlan | the white plan panels (Insurance Plan Information, Other Ins Info) now Surface like the subscriber and benefit side | none |
| Tab pages and group boxes in 252,253,254 or 249,249,249 (FormFeeSchedTools, FormProvEdit, FormRpUnearnedIncome, FormPaySimple, FormPayPlanDynamic, FormLateCharges ...) | a 2 to 5 levels darker page (Surface, 250): the large "changed" percentages are this near-white shift over the whole page | none |
| FormBackup, FormDatabaseMaintenance, FormInsCatsSetup | the borderless caption boxes on the page's colour (were 252,253,254 on the 250 page: a faint rectangle) | none |
| FormClaimFormEdit | the editor's surround Surface (was WhiteSmoke); the claim form's white canvas unchanged | none |
| FormWikiCompare | the window and its two title labels on Surface; the old/new legend labels keep their pink and green | none |
| ControlImages | the collapse arrow panel's background SurfaceSubtle (ColorOD.Control, was 240) | none |
| The 150% pictures | the same changes at 150%; nothing moved | none |

## Same pictures (20)

FormApptEdit, FormEhrMedicalOrders, FormIncomeTransferManage, FormOrthoRxEdit, FormOrthoRxSetup, FormProgramLinkHideClinics and
FormReplicationSetup (a default-text `ForeColor` line: the same black), FormClaimEdit at 100% and 150% (its white tab pages are not the one
the harness shows), FormCareCreditTransactions, FormClinicEdit, FormExamSheets, FormSheetFieldCheckBox, FormScheduleDayEdit and
FormCentralUserEdit (the changed page or panel is hidden or covered), FormDatabaseMaintenancePat (its caption boxes were already the
Surface value), FormEhrSetup and FormPreferences (the change is in code at Load and on a search), the progress windows (UiPlainWindow
already drew the token).

No finding open.
