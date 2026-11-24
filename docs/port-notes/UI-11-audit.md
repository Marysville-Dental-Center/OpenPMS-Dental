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

## Same pictures (17 after the fix round; 20 in the first pass)

FormApptEdit, FormEhrMedicalOrders, FormIncomeTransferManage, FormOrthoRxEdit, FormOrthoRxSetup, FormProgramLinkHideClinics and
FormReplicationSetup (a default-text `ForeColor` line: the same black), FormCareCreditTransactions, FormClinicEdit, FormSheetFieldCheckBox,
FormScheduleDayEdit and FormCentralUserEdit (the changed page or panel is hidden or covered), FormDatabaseMaintenancePat (its caption boxes
were already the Surface value), FormEhrSetup and FormPreferences (the change is in code at Load and on a search), the progress windows
(UiPlainWindow already drew the token).

The first pass also listed FormExamSheets ("hidden or covered"): wrong, its preview's paint code cleared the panel to 252,253,254 over the
token, so the right half kept the old colour. And FormClaimEdit at 100% and 150% ("its white tab pages are not the one the harness shows"):
the shown General page was transparent, so its group boxes stayed white. Both are fixed and changed now (below).

## Fix round (the review's findings)

The changed pictures were redrawn from the fix round's build (`<W>/fix-after-100`, `<W>/fix-after-150`) and copied into `gal-after`; the
gallery was written again (108 pictures: 89 changed, 17 the same, 2 only after). Each redrawn picture was opened.

| Window(s) | Seen | Fix |
|---|---|---|
| ControlAccount at 100% and 150% | the Show SuperFamily check box sat on a 240,240,240 patch over the 250 page (a UI.CheckBox under the Transparent tabMain cannot show a transparent colour, so WinForms drew Windows' grey) | tabMain's and tabShow's Transparent lines removed: the box is on 250 (sampled), nothing else in the picture moved |
| ControlAccount-show, -show-150 (new state `ControlAccount#show`, after only) | the Show tab's four check boxes, which the first pass never drew; on the page's 250 | the same fix (tabShow) |
| FormClaimEdit at 100% and 150% | the General page's group boxes (Crown, Ortho, Claim Referral, Accident) were white on the 250 window: UI.GroupBox answers a transparent parent with white | tabGeneral's Transparent line removed: the group boxes and the Is For Ortho box are on 250 |
| FormRpPatients | the hint was a white box on the 250 tab page (a borderless text box at the TextBox default Window) | the hint box is read-only, its default grey is a caption box's old background, so SetCaptionBoxLook gives it the page's colour; no rectangle left |
| FormEmailInbox | the preview's user name, sent or received and date boxes were white on the 250 preview | their `SystemColors.Window` lines removed (read-only: Windows' grey, which the caption look maps); on 250, the red Unsent text unchanged |
| FormExamSheets | the preview's right half was 252,253,254 next to the 250 list side | `g.Clear(panelSheetPreview.BackColor)`: the whole window on 250 |

No finding open.
