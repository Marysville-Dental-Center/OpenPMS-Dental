# Port note: UI-COLS (grid columns wide enough for their values in the new font)

Why: the owner's "fix it" (2026-11-24) on what DOCS-SITE found while drawing the README screenshots: the Account module's Date column
wrapped dates such as 06/10/2025 onto two lines at 100%, and the Edit Appointment window's procedure grid cut the Fee 1260.00. The grid
font changed from sans serif 8.5 (Microsoft Sans Serif) to Segoe UI 9 in UI-2b; Segoe UI's digits are as wide, but its slashes, colons,
parentheses and letters are wider, so a date needs 4 px more, a date and time 7, a phone number 3, a time 2. Restyle only: widths of grid
columns, no behaviour, text or layout change; no window grew or shrank.

## 1. How a column was measured

GridOD draws a cell with `Graphics.DrawString` (ClearType grid fit) in a rectangle as wide as the column, with its StringFormat (a tab stop
at 50, wrapping on; with WrapText off, no wrap and trimming at a character), in the grid font at the scale (GridOD.ComputeRowHeightOne
measures with the font at the Windows scale). A column holds a value when the value measures one line at the column's width: at 100% the
width itself, at 150% `ScaleI(width)` (rounded half to even, as GridOD). A value of fixed width (all but money) must also stand at least
2 px clear of the gridline on the side its alignment leaves free (the right for left-aligned and centered values, the left for
right-aligned ones; the aligned side keeps GDI+'s own padding of about 1/6 em, which no width changes): the value is drawn as GridOD
draws it and its ink scanned. Without that rule a date fit "on one line" at 68 px with its last pixel 1 px before the gridline, running
into the next column's value ("06/10/2025|Alex"); a centered value gets the odd pixel of the room on its left (12/31/2026 centered in
70 px: 3 px left, 1 right), so a centered date needs 71. Money is checked against wrapping only: 9,999.99 is the top of the range (most
amounts are formatted without the separator, as Edit Appointment's 1260.00, and are narrower). The heading of a value column must fit on
one line in the bold header font (GridOD.DrawHeaders: UiTheme.FontGridBold, centered; the header is one line high, so a wider heading is
cut, "Amoun"); a heading of two lines in the source ("Date\r\nShowing") is checked line by line. The widest normal values, en-US with the
four-digit year the program sets at its start:

| Type | Value | One line at 100% / 150% | With 2 px clear at 100% | Was (sans serif 8.5, 100%) |
|---|---|---|---|---|
| date | 12/31/2026 | 67 / 101 | 70 (centered 71) | 63 |
| date and time | 12/31/2026 11:00 AM (AM is wider than PM in Segoe UI) | 123 / 184 | 125 | 116 |
| time | 11:00 AM | 56 / 84 | 57 | 54 |
| money | 9,999.99 | 50 / 74 | (not checked) | 50 |
| percent | 100% | 35 / 52 | | 34 |
| phone | (555)555-5555 | 84 / 125 | 85 | 81 |
| tooth | 32 | 18 / 26 | | 17 |
| surfaces | MODBL | 46 / 69 | 46 | 46 |
| procedure code | D0120 | 40 / 59 | 40 | |

The columns: every `new GridColumn(` with a literal heading and width, and every `new DisplayField(` (the display-field defaults and the
fields one can add, DisplayFields.GetDefaultList and GetAllAvailableList), in the program's folders and the separate programs'
(SourceIndex.ArrayFolders), about 640 of a value type by their heading. The value type comes from the heading (`GridColumnFit.GetKind`);
`tests/UiRefreshTests/Data/grid-col-known.tsv` corrects it where the code shows something else, with the reason (a "Pat" column that
holds an X, a "Time" column that formats 12:00p, a "Due" that is money, a date and time that shows the date only or wraps on two lines by
design since Open Dental). A dynamic column (IsWidthDynamic) and a hidden one (width 0) are sized by their grid and not checked.

Not swept: the WPF grid (WpfControlsOD's Grid: FrmInsPlanSelect, the imaging and capture windows). It draws in WPF text with its own
Segoe UI 11.5 DIPs, as since Open Dental; the font flip did not change it. Columns whose heading or width is computed in code (display
fields read from the database, translated headings in variables) take the defaults above.

## 2. Central fix or one by one

On main 56 columns were too narrow (the list below): 32 date columns of about 210 (most date columns were already 67 to 80 wide), 3 date
and time, 1 time, 2 phone, 11 surfaces, 6 money, the progress notes' time. They do not fail by one ratio: dates by 1 to 7 px, money and
surfaces by what they lacked in the old font too. A central rule in GridOD (a minimum width per value type, or scaling the designer-era
widths by the font's ratio) would widen every grid, also the hundreds that fit, push grids sized to their window into horizontal scroll
bars, and override the widths offices set in Setup > Display Fields. So the failing columns were widened one by one, each to what its value
needs, the room taken from the grid's description or note column so the widths add up as before (where the grid has a dynamic column or a
last column that fills the rest, GridOD takes it there). No window changed size; no column was removed.

The display-field grids of the windows the harness builds are also checked as GridOD lays them out (each column from the left at its
scaled width, the last widened to the grid's edge beside the vertical scroll bar): Edit Appointment's procedure grid had 540 px of
defaults in a 538 px grid, so FormApptEdit turned the horizontal scroll bar on and the Fee column ran past the edge (39 px of it shown at
100%). Its defaults are still 522 px in all, now Priority 50 (was 45; its heading "Priority" was cut to "Priorit"), Tth 25, Surf 46
(MODBL), Code 41 (D2394, 2 px clear), Description 272 and Fee 53. Description 272 holds the longest descriptions of the usual CDT codes
on one line at 100% and 150% ("Resin composite, four or more surfaces, posterior", D2394, needs 272; the first fix round's 250 wrapped
it onto two lines, the row twice as high); Fee fills to the grid's edge with 50 px beside the vertical scroll bar at 100% (75 at 150%),
as 9,999.99 needs. The medical branch's Code is 112, the width of the dental branch's Tth, Surf and Code, so its Fee keeps the same room
(at 125 with Description 272 it would have kept 37 px).

Fix round 2 (the review of G-UI3-uicols): with the 2 px rule and the heading rule above, every date column that round 1 had set to 68
went to 70 (71 where centered), the three date-and-time columns to 125, FormExamSheets' Time to 57, the two phones to 85, and the
progress notes' Amount to 54 (its heading was cut to "Amoun"; Proc Code 64 for the same reason). The same rules found columns on main
outside round 1's list: 16 centered date columns of 70 (71 now), FormQueryMonitor's DateTimeStart (126), and headings cut at 100% or
150% (section 3b). Each again takes its room from the grid's description or note column, or a dynamic column takes it.

## 3. The list (main at the branch point 46b9c66d0)

Width and needs at 100% / 150%. "Grid" is the line of the GridColumn on main, or the display-field list.

| Window | Grid | Column | Type | Width | Needs | Now |
|---|---|---|---|---|---|---|
| FormRpProcNote | line 98 | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| FormASAP | line 141 | Date | date | 65 / 98 | 67 / 101 | 71 |
| FormAuditOrtho | line 56 | Date Time | datetime | 120 / 180 | 123 / 184 | 125 |
| FormCertifications | line 100 | Date | date | 65 / 98 | 67 / 101 | 70 |
| FormCertifications | line 171 | Date | date | 65 / 98 | 67 / 101 | 70 |
| FormCertifications | line 237 | Date | date | 65 / 98 | 67 / 101 | 70 |
| FormClaimEdit | line 1061 | Date | date | 66 / 99 | 67 / 101 | 70 |
| FormClaimOverpay | line 60 | Date | date | 66 / 99 | 67 / 101 | 70 |
| FormClaimPayList | line 56 | Date | date | 65 / 98 | 67 / 101 | 70 |
| FormClaimPayTotal | line 106 | Date | date | 66 / 99 | 67 / 101 | 70 |
| FormEtrans835ClaimPay | line 249 | Date | date | 66 / 99 | 67 / 101 | 70 |
| FormExamSheets | line 65 | Time | time | 54 / 81 | 56 / 84 | 57 |
| FormIncomeTransferManage | line 61 | Date | date | 65 / 98 | 67 / 101 | 71 |
| FormInsPlans | line 77 | Phone | phone | 82 / 123 | 84 / 125 | 85 |
| FormInsPlansMerge | line 44 | Phone | phone | 82 / 123 | 84 / 125 | 85 |
| FormLabCases | line 64 | Appt Date Time | datetime | 120 / 180 | 123 / 184 | 125 |
| FormPatientForms | line 199 | Updated | date | 65 / 98 | 67 / 101 | 70 |
| FormPayPlan | line 245 | Date | date | 64 / 96 | 67 / 101 | 71 |
| FormPayPlanDynamic | line 330 | Date | date | 64 / 96 | 67 / 101 | 71 |
| FormPayment | line 1858 | Date | date | 65 / 98 | 67 / 101 | 71 |
| FormPayment | line 2091 | Date | date | 65 / 98 | 67 / 101 | 71 |
| FormPreAuthReminder | line 34 | Surf | surface | 45 / 68 | 46 / 69 | 46 |
| FormSheetProcSelect | line 31 | Date | date | 67 / 100 | 67 / 101 | 70 |
| FormSheetProcSelect | line 33 | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| FormTaskEdit | line 443 | Date Time | datetime | 120 / 180 | 123 / 184 | 125 |
| FormTrackNext | line 122 | Date | date | 65 / 98 | 67 / 101 | 70 |
| FormTreatPlanCurEdit | line 83 | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| FormUnsched | line 275 | Date | date | 65 / 98 | 67 / 101 | 70 |
| FormUnschedListPatient | line 30 | Date | date | 65 / 98 | 67 / 101 | 71 |
| ControlAccount | line 3574 | Bal | money | 49 / 74 | 50 / 74 | 50 |
| ControlAccount | line 3675 | Date | date | 65 / 98 | 67 / 101 | 70 |
| ControlAccount | line 3689 | Paid | money | 40 / 60 | 50 / 74 | 50 |
| ControlAccount | line 3975 | Date | date | 65 / 98 | 67 / 101 | 70 |
| ControlChart | line 6649 | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| DisplayFields | GetDefaultList.None (progress notes) | Date | date | 67 / 100 | 67 / 101 | 70 |
| DisplayFields | GetDefaultList.None (progress notes) | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| DisplayFields | GetDefaultList.None (progress notes) | Amount | money | 48 / 72 | 50 / 74 | 54 |
| DisplayFields | GetDefaultList.AccountModule | Date | date | 65 / 98 | 67 / 101 | 70 |
| DisplayFields | GetDefaultList.ProcedureGroupNote | Date | date | 67 / 100 | 67 / 101 | 70 |
| DisplayFields | GetDefaultList.ProcedureGroupNote | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| DisplayFields | GetDefaultList.ProcedureGroupNote | Amount | money | 48 / 72 | 50 / 74 | 54 |
| DisplayFields | GetDefaultList.TreatmentPlanModule | Surf | surface | 45 / 68 | 46 / 69 | 46 |
| DisplayFields | GetDefaultList.LimitedCustomStatement | Date | date | 65 / 98 | 67 / 101 | 70 |
| DisplayFields | GetAllAvailableList.None | Date | date | 67 / 100 | 67 / 101 | 70 |
| DisplayFields | GetAllAvailableList.None | Time | time (12:00p) | 40 / 60 | 41 / 61 | 42 |
| DisplayFields | GetAllAvailableList.None | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| DisplayFields | GetAllAvailableList.None | Amount | money | 48 / 72 | 50 / 74 | 54 |
| DisplayFields | GetAllAvailableList.None | Date TP | date | 67 / 100 | 67 / 101 | 70 |
| DisplayFields | GetAllAvailableList.None | Date Entry | date | 67 / 100 | 67 / 101 | 70 |
| DisplayFields | GetAllAvailableList.AccountModule | Date | date | 65 / 98 | 67 / 101 | 70 |
| DisplayFields | GetAllAvailableList.ProcedureGroupNote | Date | date | 67 / 100 | 67 / 101 | 70 |
| DisplayFields | GetAllAvailableList.ProcedureGroupNote | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| DisplayFields | GetAllAvailableList.ProcedureGroupNote | Amount | money | 48 / 72 | 50 / 74 | 54 |
| DisplayFields | GetAllAvailableList.TreatmentPlanModule | Surf | surface | 45 / 68 | 46 / 69 | 46 |
| DisplayFields | GetAllAvailableList.TreatmentPlanModule | DateTP | date | 65 / 98 | 67 / 101 | 70 |
| DisplayFields | GetAllAvailableList.LimitedCustomStatement | Date | date | 65 / 98 | 67 / 101 | 70 |
| FormApptEdit | gridProc (display fields AppointmentEdit, PlannedAppointmentEdit) | Fee (last) | money | 39 / 59 shown | 50 / 74 | 50 / 75 shown (Priority 50, Surf 46, Code 41, Description 272, Fee 53) |

Donors (the grid's widths add up as before; after fix round 2): FormRpProcNote Description 220 to 214; FormASAP Notes 154;
FormAuditOrtho Log Text 564; FormCertifications Note 202 and 162; FormClaimPayList Carrier 178; FormClaimPayTotal and
FormEtrans835ClaimPay Description 126; FormInsPlans Address 117; FormInsPlansMerge Plan Note 177; FormLabCases Instructions 95;
FormPatientForms Description 165; FormPayPlan Description 121 (also for Adjustment's heading); FormPayPlanDynamic Description 140;
FormPreAuthReminder Description 199; FormSheetProcSelect Description 309; FormTaskEdit Note 395; FormTrackNext and FormUnsched Notes 195,
FormUnschedListPatient Notes 194; FormTreatPlanCurEdit Description 189; ControlAccount's family grid Patient 104, payment plans Guarantor
78 and Patient 77, unearned Description 175; DisplayFields: progress notes Description 201 (default and available, procedure group note
default 186: Date, Surf, Amount and Proc Code took 17 px), treatment plan 199 (Surf and Allowed's heading), Account module and limited
statement 265. FormClaimEdit, FormClaimOverpay, FormExamSheets, FormIncomeTransferManage, FormPayment and the Chart's treatment plan grid
have a dynamic column, which takes the room.

### 3b. Fix round 2: columns outside the first list

Found by the 2 px rule and the heading rule (section 1) on columns round 1 had not changed. Width on main at 100%; "Now" with the donor.

| Window | Grid | Column | Was | Why | Now | Donor |
|---|---|---|---|---|---|---|
| FormAdjMulti | line 140 | Date (centered) | 70 | 1 px clear | 71 | dynamic |
| FormAdjustSelect | line 56 | Date (centered) | 70 | 1 px clear | 71 | dynamic |
| FormAdjustSelect | line 66 | Amt Avail | 60 | heading needs 61 / 92 | 61 | dynamic |
| FormAdvertisingPatientList | line 94 | Birthdate (centered) | 70 | 1 px clear | 71 | Email 199 |
| FormCareCreditTransactions | line 291 | Date Processed | 90 | heading needs 93 / 139 | 93 | Status 197 |
| FormClaimEdit | line 2226 | Check Date (centered) | 70 | 1 px clear | 71 | dynamic |
| FormClaimProcBlueBookLog | line 34 | Date (centered) | 70 | 1 px clear | 71 | dynamic |
| FormClaimsSend | lines 1078, 1120 | Date (centered) | 70 | 1 px clear | 71 | dynamic |
| FormDashboardWidgetSetup | line 78 | Allowed | 50 | heading needs 52 / 78 | 52 | dynamic |
| FormEquipment | line 95 | DatePurchased | 90 | heading needs 91 / 137 | 92 | Description 148 |
| FormEvaluations | line 72 | Date (centered) | 70 | 1 px clear | 71 | Title 89 |
| FormMedLabPatSelect, FormMedLabs | lines 161, 98 | Date & Time Reported | 135 | heading needs 203 at 150% | 136 | Test Description 174, Test(s) Description 234 |
| FormPayPlan | line 259 | Adjustment | 70 | heading needs 72 / 108 | 72 | Description |
| FormPayPlanDynamic | lines 292, 294 | Date Showing, Date Production (centered) | 70 | 1 px clear | 71 | Description (widthDesc 168) |
| FormProcEdit | line 1022 | Write-off | 55 | heading needs 59 / 88 | 59 | Remarks 161 |
| FormProcEdit | lines 1247, 1284 | Entry Date (centered) | 70 | 1 px clear | 71 | Note 249 |
| FormProcSelect | line 123 | Date (centered) | 70 | 1 px clear | 71 | Description 289 / 249 |
| FormProdSelect | line 33 | Date (centered) | 70 | 1 px clear | 71 | dynamic |
| FormQueryMonitor | line 213 | DateTimeStart (centered) | 125 | 1 px clear | 126 | dynamic |
| FormRecallsPat | line 50 | PreviousDate | 80 | heading needs 82 / 122 | 82 | Note 98 |
| FormRpClaimNotSent | line 75 | Date of Service | 90 | heading needs 92 / 138 | 92 | Patient Name 148 |
| ControlAccount | lines 3795, 3797 | Start Date, Stop Date (centered) | 70 | 1 px clear | 71 | Note 353 |
| DisplayFields | progress notes, procedure group note (default and available) | Proc Code | 62 | heading needs 63 / 95 | 64 | Description |
| DisplayFields | treatment plan (default and available) | Allowed | 50 | heading needs 52 / 78 | 52 | Description 199 |
| DisplayFields | OutstandingInsReport (default and available) | DateSentOrig | 80 | heading needs 83 / 124 | 83 | PatName 117 |

Not changed: FormFeeSchedGroups' "Fee Schedule" (75; its heading needs 80) holds the fee schedule's name, not money:
the known list marks it, and a heading of a column without a value type is outside this package. Money columns of 50 px keep 9,999.99 on
one line with 0 to 1 px beside the gridline (section 1: money is not held to the 2 px rule).

## 4. Tests

`tests/UiRefreshTests/GridColumnFitTests.cs` (UiRefreshTests, no database, nothing shown):
- `Columns_OfAKnownValueType_HoldTheirWidestValue_At100And150`: every column of section 1 at 100% and 150%: the value on one line, 2 px
  clear of the gridline (not money), the heading on one line in the bold header font. On main: 56 failures of the first rule; after fix
  round 1 (with the 2 px and heading rules of round 2): 96, of them 29 money columns' clearance, which money is not held to.
- `DisplayFieldGrids_InTheirWindows_ShowEveryColumn`: ControlAccount's account grid and FormApptEdit's procedure grid (both appointment
  categories), built by WindowBuilder at 100% and 150%, with their default display fields. On main: the Account Date (65 px, needs 67;
  98 at 150%, needs 101) and the Edit Appointment Fee (39 px shown at 100%, 59 at 150%); after round 1, the Account Date at 68 (1 px
  clear). The same rules as above, at the width each column shows.
- `KnownList_EveryRowMatchesAColumn`, `GetKind_ReadsTheHeadings`.
Measuring and scanning: `GridColumnFit.cs`. The window builds run with the preference cache empty (ControlAccount's Resize handler reads
preferences only when the cache is filled) and give it back afterwards.

## 5. Gallery G-UI3-uicols

`tests/UiRefreshTests/GridColumnGallery.cs`, opt-in (`OPMS_UI_COLS_GALLERY`, `OPMS_UI_COLS_GALLERY_BEFORE`), in
`.git/od-tools/test-results/G-UI3-uicols` (`before`, `after`, and `pairs`: before above after, a magenta band between): the Account
module and Edit Appointment whole, as the harness builds them, their grids filled from the default display fields with fictional rows,
and a strip per changed grid (`Data/grid-col-gallery.tsv`: the grid's columns from its fill code, three rows, the first with each column's
widest value), at 100% and 150%. At 150% the grids paint at 144 dpi (`DesignSheetWindows.IsGridPaintedAtDpi`), as on a 150% screen.
Before: main's build at the branch point. Fix round 2 added the grids of section 3b to the list (after pictures only: they were drawn
after main's before set; on main each column was 1 to 4 px narrower). Audit: [UI-COLS-audit.md](UI-COLS-audit.md).

## 6. Screenshots

With the fix, DocScreenshots' `account.png` and `dialog-appointment-edit.png` are committed (docs/screenshots, site/screenshots) and
shown in the README and on the site. DocScreenshots now draws the account grid and Edit Appointment's patient grid from their first row
(the module scrolls to the last row, which left half a row above the grid's top).

## 7. UI-COLS-2

The owner's "Yes to both" to the two follow-ups of the approval (docs/owner-questions.md), and the screenshots' message panel.

**Rules (GridColumnFit).** 1. Money stands 2 px clear of the gridline its alignment leaves free, like a date: 9,999.99, and -9,999.99
where the heading says a balance or an adjustment (`GridColumnFit.GetValue`, `MONEY_NEGATIVE`). The side the value is held to keeps
GDI+'s own padding (9,999.99 has 1 px at 100% and 3 px at 150% there whatever the width), so "both gridlines" is the free side plus
that padding; no width can make it 2 px at 100%. 2. Every visible column's heading (not only value columns; dynamic and zero-width
columns excepted) fits centered in UiTheme.FontGridBold with 2 px clear of both gridlines at 100% and 150%. A grid whose designer file
sets `HasAutoWrappedHeaders` (FormAdvertisingPatientList, FormTimeCardSetup's gridRules) wraps its headings at the spaces: each word
fits. The display-field window test checks every column's heading too.

**Fix.** 143 column sites widened (16 money values, 127 headings) in 76 files, each grid's total kept: the room from a Description,
Note, Name or similar column of the same grid (93 donor sites), or the grid's dynamic column takes it. The available display-field lists
take the same widths as the defaults (10 sites). Edit Appointment: Stat 35 to 31 gives Tth 2 px and Fee 2 px (9,999.99 needs 52 px at
100% now; Description 272 stays for D2394), the medical branch's Code 112 to 116. grid-col-known.tsv: no new exception (FormFeeSchedGroups'
Fee Schedule row follows its new width 80). The list (file, the widened columns, the donors):

- FormCentralPatientTransfer: Status 30 to 41; donors Note 200 to 189
- FormAdjMulti: Rem Before 70 to 73; donors Fee 70 to 67
- FormAdvertisingPatientList: Last Appointment 80 to 82, Next Appointment 80 to 82; donors Email 199 to 195
- FormAlerts: Alert # 45 to 46; the dynamic Description takes the room
- FormApptRules: Enabled 50 to 51; donors Description 200 to 199
- FormBilling: PayPlanDue 70 to 74; donors Name 180 to 176
- FormBillingOptions: Email 30 to 36; donors Message 150 to 144
- FormCDSSetup: Demographic 80 to 83; donors User Name 120 to 117
- FormClaimEdit: Tth 25 to 27; the dynamic Remarks takes the room
- FormClaimOverpay: Tth 25 to 27, Ins Overpaid 62 to 78, Ins Underpaid 62 to 86; the dynamic Remarks takes the room
- FormClaimPayList: Partial 40 to 42, Scanned 40 to 53; donors Carrier 178 to 163
- FormClaimPayTotal: Tth 25 to 27; the dynamic Remarks takes the room
- FormClaimsSend: M/D 30 to 32, Clearinghouse 85 to 88; the dynamic ProcCodes takes the room
- FormCodeGroups: Hidden 45 to 47; the dynamic Group Name takes the room
- FormCodeSystemsImport: Rows in Database 105 to 106, Minimum Expected 105 to 117; the dynamic Status takes the room
- FormCreditCardManage: Global Payments V1 90 to 121, Global Payments V2 90 to 121; donors Card Number 140 to 125, Clinic 80 to 64, Start Date 80 to 73, Stop Date 80 to 73, Amount 80 to 64, Charge Frequency 140 to 139
- FormCreditRecurringCharges: PayPlan Due 75 to 77, Global Payments V1 90 to 121, Global Payments V2 90 to 121, Global Payments V1 80 to 121, Global Payments V2 80 to 121; donors Name 190 to 160, Clinic 75 to 60, Date 80 to 75, Family Bal 70 to 66, Total Due 65 to 64, Charge Amt 85 to 76, Name 220 to 170, Family Bal 85 to 83, Repeat Amt 90 to 80, Charge Amt 100 to 80
- FormDashboardWidgetSetup: Allowed 52 to 53; the dynamic Name takes the room
- FormDatabaseMaintenance: Explain 45 to 47, Explain 45 to 47, Hidden 45 to 47, Explain 45 to 47; donors Name 740 to 738
- FormDatabaseMaintenancePat: Hidden 45 to 47; donors Name 300 to 298
- FormDiscountPlans: Hidden 20 to 47; donors Description 200 to 173
- FormDunningSetup: Email 35 to 36; donors Message 150 to 149
- FormEhrAmendments: Scanned 25 to 53; donors Description 170 to 142
- FormEhrLabOrderEdit2014: Note Num 60 to 65, Specimen Type 60 to 92 (gridSpecimen, 222 px: the room is free); donors Comments 300 to 295
- FormEhrLabResultEdit2014: Note Num 60 to 65; donors Comments 300 to 295
- FormEhrMeasureCalc: Denominator 80 to 82, Exclusion Count 95 to 97; donors Percent 230 to 226
- FormEhrProviderKeys: Year 30 to 32; donors Last Name 80 to 78
- FormElectIDs: Is Medicaid 70 to 71; donors Comments 350 to 349
- FormErxSetup: Enabled 50 to 51; donors Provider 110 to 109
- FormEtrans835ClaimPay: Split 30 to 31, Tth 25 to 27; the dynamic Remarks takes the room
- FormEvaluationReport: ProvNum 50 to 60; donors Last Name 80 to 75, First Name 80 to 75
- FormFeeSchedGroupEdit: Hidden 25 to 47; donors Description 250 to 228
- FormFeeSchedGroups: Fee Schedule 75 to 80; donors Group Name 200 to 195
- FormHL7DefMessageEdit: Is Optional 67 to 68; donors Note 100 to 99
- FormHL7Defs: Enabled 35 to 51, Enabled 35 to 51; donors Description 100 to 92, In Folder / Socket 130 to 122, Description 100 to 92, In Folder / Socket 130 to 122
- FormImagePicker: Image Name 70 to 79 (the only column of a 252 px grid)
- FormInsBenefits: Amt 50 to 51, Treat Area 65 to 66; donors Category 95 to 93
- FormInsCatsSetup: Hidden 45 to 47, E-Benefit Category 100 to 114; donors Category 90 to 82, To Code 70 to 62
- FormInsPlansOverrides: NoBillIns Override 60 to 111; donors Employer 140 to 115, Carrier 140 to 114
- FormLoincs: Order or Observation 100 to 128; donors Long Name 500 to 472
- FormMedical: Status 40 to 41, Status 40 to 41; donors Patient Note 450 to 449, Reaction 500 to 499
- FormMedicationReconcile: Disc 10 to 28; donors Notes for Patient 225 to 207
- FormPayPlan: Provider 50 to 54; donors Description 121 to 117
- FormPayPlanCredits: Rem Before 70 to 73, Rem After 60 to 65; the dynamic Note takes the room
- FormPayPlanTemplates: Treatment Plan Option 120 to 137; donors Interest Delay 105 to 88
- FormPayment: Tth 25 to 27, Tth 25 to 27; the dynamic Patient takes the room
- FormPrinterSetup: File Extension 50 to 84; donors Printer Used 220 to 186
- FormProcCodes2: Fee 1 50 to 51, Fee 2 50 to 51, Fee 3 50 to 51; donors Description 206 to 203
- FormProcEdit: Write-off 59 to 62; donors Remarks 161 to 158
- FormProvAdditional: State Where Licensed 120 to 129, CareCredit Merch Num 130 to 138; donors Clinic 120 to 103
- FormProviderSetup: Instructor 60 to 62; donors Last Name 90 to 88
- FormQueryFavorites: Released 55 to 56; donors Query 350 to 349
- FormRecallList: #Remind 55 to 56; donors Note 150 to 149
- FormRecallSetup: Remind# 50 to 56; donors Message 500 to 494
- FormReconcileEdit: Withdrawals 75 to 77; donors Date 80 to 78
- FormReconcileMedication: Is Incoming 50 to 72; donors Description 350 to 328
- FormReconcileProblem: Is Incoming 50 to 72; donors Notes 300 to 278
- FormReferralProcTrack: Date Referred 86 to 87; donors Patient 125 to 124
- FormReqAppt: Completed 40 to 68; donors Descript 150 to 122
- FormRpEraAutoProcessed: MoreClaims? 65 to 79; donors Patient Name 250 to 236
- FormRxNorms: InMedList 60 to 62, MedCount 60 to 66; the dynamic Description takes the room
- FormScheduledProcesses: Time To Run 75 to 77; donors Frequency to Run 150 to 148
- FormScreenGroupEdit: NoTeeth 55 to 56; donors Name 100 to 99
- FormSheetDefs: Use Mobile\r\nLayout 65 to 70; the dynamic Description takes the room
- FormSheetImport: Do Import 60 to 65; donors FieldName 140 to 135
- FormShutdown: Connection Type 100 to 102; donors Computer Name 120 to 118
- FormSupplyReceive: Receive Now 78 to 80; donors Description 250 to 248
- FormTimeCard: Total 50 to 51; donors Out 64 to 63
- FormTimeCardManage: Total 50 to 51; donors In 60 to 59
- FormTreatPlanCurEdit: Status 40 to 41; donors Description 189 to 188
- ControlAccount: Bal 50 to 56, Type 30 to 35, Total Cost 60 to 63, Paid 50 to 51; donors Patient 104 to 98, Closed 60 to 51
- UserControlSetupWizClinic: Default Prov 75 to 78, IsHidden 55 to 56; donors Address 120 to 116
- UserControlSetupWizOperatory: IsHygiene 60 to 62; donors OpName 110 to 108
- UserControlTasks: +/- 17 to 24; donors Task List 90 to 83
- DisplayFields GetDefaultList.None: Stat 25 to 29; donors Description 201 to 197
- DisplayFields GetDefaultList.PatientSelect: Pref Name 60 to 66; donors Address 100 to 94
- DisplayFields GetDefaultList.AccountModule: Tth 26 to 27; donors Description 265 to 264
- DisplayFields GetDefaultList.RecallList: #Remind 55 to 56; donors Note 215 to 214
- DisplayFields GetDefaultList.ProcedureGroupNote: Stat 25 to 29; donors Description 186 to 182
- DisplayFields GetDefaultList.TreatmentPlanModule: Fee 50 to 52, Allowed 52 to 53, Pri Ins 50 to 52, Sec Ins 50 to 52, Discount 55 to 56, Pat 50 to 52; donors Description 199 to 189
- DisplayFields GetDefaultList.AppointmentEdit: Tth 25 to 27; donors Stat 35 to 31, Code 112 to 116
- DisplayFields GetDefaultList.PlannedAppointmentEdit: Tth 25 to 27; donors Stat 35 to 31, Code 112 to 116
- DisplayFields GetDefaultList.LimitedCustomStatement: Tth 26 to 27; donors Description 265 to 264
- DisplayFields GetAllAvailableList.None: Description 201 to 197, Stat 25 to 29, Priority 44 to 48, Prognosis 60 to 62, Length 40 to 45, Attachment 55 to 74 (available list: same widths as the defaults, or not in a grid until chosen)
- DisplayFields GetAllAvailableList.PatientSelect: Pref Name 60 to 66, ChartNum 60 to 65, Address 100 to 94, Invoice Number 90 to 97 (available list: same widths as the defaults, or not in a grid until chosen)
- DisplayFields GetAllAvailableList.AccountModule: Tth 26 to 27, Description 265 to 264 (available list: same widths as the defaults, or not in a grid until chosen)
- DisplayFields GetAllAvailableList.RecallList: #Remind 55 to 56, Note 215 to 214 (available list: same widths as the defaults, or not in a grid until chosen)
- DisplayFields GetAllAvailableList.ProcedureGroupNote: Stat 25 to 29 (available list: same widths as the defaults, or not in a grid until chosen)
- DisplayFields GetAllAvailableList.TreatmentPlanModule: Description 199 to 189, Fee 50 to 52, Allowed 52 to 53, Pri Ins 50 to 52, Sec Ins 50 to 52, Discount 55 to 56, Pat 50 to 52, Prognosis 60 to 62 (available list: same widths as the defaults, or not in a grid until chosen)
- DisplayFields GetAllAvailableList.AppointmentEdit: Stat 35 to 31, Code 112 to 116, Tth 25 to 27 (available list: same widths as the defaults, or not in a grid until chosen)
- DisplayFields GetAllAvailableList.PlannedAppointmentEdit: Stat 35 to 31, Code 112 to 116, Tth 25 to 27 (available list: same widths as the defaults, or not in a grid until chosen)
- DisplayFields GetAllAvailableList.OutstandingInsReport: GroupNum 60 to 69 (available list: same widths as the defaults, or not in a grid until chosen)
- DisplayFields GetAllAvailableList.CEMTSearchPatients: Pref Name 60 to 66 (available list: same widths as the defaults, or not in a grid until chosen)
- DisplayFields GetAllAvailableList.LimitedCustomStatement: Tth 26 to 27, Description 265 to 264 (available list: same widths as the defaults, or not in a grid until chosen)
- MaintHistoryGrid: Flagged 50 to 51; the dynamic Results takes the room

**Screenshots.** DocScreenshots draws the module pictures with FormOpenDental's light signal grid (lightSignalGrid1: Op 1, Op 2, Op 3
and the other signal buttons of the scratch database) under the module bar's buttons, the real control placed and filled as
FormOpenDental.LayoutControls and FillSignalButtons do; docs/screenshots and site/screenshots redrawn.

**Gallery G-UI3-uicols2** (`.git/od-tools/test-results/G-UI3-uicols2`): GridColumnGallery with `Data/grid-col-gallery.tsv` now listing
the grids UI-COLS-2 changed, strips at 100% and 150%; before drawn from the same build with the product sources of aeac6c48c (the
strips take their widths from the source), the Account module and Edit Appointment before from G-UI3-uicols' after pictures.
