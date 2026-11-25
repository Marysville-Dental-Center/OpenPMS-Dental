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
width itself, at 150% `ScaleI(width)` (rounded half to even, as GridOD). The widest normal values, en-US with the four-digit year the
program sets at its start:

| Type | Value | Needs at 100% / 150% | Was (sans serif 8.5, 100%) |
|---|---|---|---|
| date | 12/31/2026 | 67 / 101 | 63 |
| date and time | 12/31/2026 11:00 AM (AM is wider than PM in Segoe UI) | 123 / 184 | 116 |
| time | 11:00 AM | 56 / 84 | 54 |
| money | 9,999.99 | 50 / 74 | 50 |
| percent | 100% | 35 / 52 | 34 |
| phone | (555)555-5555 | 84 / 125 | 81 |
| tooth | 32 | 18 / 26 | 17 |
| surfaces | MODBL | 46 / 69 | 46 |
| procedure code | D0120 | 40 / 59 | |

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
100%). Its defaults are now 522 px: Description 250 (was 275), and Priority 52 (was 45; its heading "Priority" was cut to "Priorit").

## 3. The list (main at the branch point 46b9c66d0)

Width and needs at 100% / 150%. "Grid" is the line of the GridColumn on main, or the display-field list.

| Window | Grid | Column | Type | Width | Needs | Now |
|---|---|---|---|---|---|---|
| FormRpProcNote | line 98 | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| FormASAP | line 141 | Date | date | 65 / 98 | 67 / 101 | 68 |
| FormAuditOrtho | line 56 | Date Time | datetime | 120 / 180 | 123 / 184 | 123 |
| FormCertifications | line 100 | Date | date | 65 / 98 | 67 / 101 | 68 |
| FormCertifications | line 171 | Date | date | 65 / 98 | 67 / 101 | 68 |
| FormCertifications | line 237 | Date | date | 65 / 98 | 67 / 101 | 68 |
| FormClaimEdit | line 1061 | Date | date | 66 / 99 | 67 / 101 | 68 |
| FormClaimOverpay | line 60 | Date | date | 66 / 99 | 67 / 101 | 68 |
| FormClaimPayList | line 56 | Date | date | 65 / 98 | 67 / 101 | 68 |
| FormClaimPayTotal | line 106 | Date | date | 66 / 99 | 67 / 101 | 68 |
| FormEtrans835ClaimPay | line 249 | Date | date | 66 / 99 | 67 / 101 | 68 |
| FormExamSheets | line 65 | Time | time | 54 / 81 | 56 / 84 | 56 |
| FormIncomeTransferManage | line 61 | Date | date | 65 / 98 | 67 / 101 | 68 |
| FormInsPlans | line 77 | Phone | phone | 82 / 123 | 84 / 125 | 84 |
| FormInsPlansMerge | line 44 | Phone | phone | 82 / 123 | 84 / 125 | 84 |
| FormLabCases | line 64 | Appt Date Time | datetime | 120 / 180 | 123 / 184 | 123 |
| FormPatientForms | line 199 | Updated | date | 65 / 98 | 67 / 101 | 68 |
| FormPayPlan | line 245 | Date | date | 64 / 96 | 67 / 101 | 68 |
| FormPayPlanDynamic | line 330 | Date | date | 64 / 96 | 67 / 101 | 68 |
| FormPayment | line 1858 | Date | date | 65 / 98 | 67 / 101 | 68 |
| FormPayment | line 2091 | Date | date | 65 / 98 | 67 / 101 | 68 |
| FormPreAuthReminder | line 34 | Surf | surface | 45 / 68 | 46 / 69 | 46 |
| FormSheetProcSelect | line 31 | Date | date | 67 / 100 | 67 / 101 | 68 |
| FormSheetProcSelect | line 33 | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| FormTaskEdit | line 443 | Date Time | datetime | 120 / 180 | 123 / 184 | 123 |
| FormTrackNext | line 122 | Date | date | 65 / 98 | 67 / 101 | 68 |
| FormTreatPlanCurEdit | line 83 | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| FormUnsched | line 275 | Date | date | 65 / 98 | 67 / 101 | 68 |
| FormUnschedListPatient | line 30 | Date | date | 65 / 98 | 67 / 101 | 68 |
| ControlAccount | line 3574 | Bal | money | 49 / 74 | 50 / 74 | 50 |
| ControlAccount | line 3675 | Date | date | 65 / 98 | 67 / 101 | 68 |
| ControlAccount | line 3689 | Paid | money | 40 / 60 | 50 / 74 | 50 |
| ControlAccount | line 3975 | Date | date | 65 / 98 | 67 / 101 | 68 |
| ControlChart | line 6649 | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| DisplayFields | GetDefaultList.None (progress notes) | Date | date | 67 / 100 | 67 / 101 | 68 |
| DisplayFields | GetDefaultList.None (progress notes) | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| DisplayFields | GetDefaultList.None (progress notes) | Amount | money | 48 / 72 | 50 / 74 | 50 |
| DisplayFields | GetDefaultList.AccountModule | Date | date | 65 / 98 | 67 / 101 | 68 |
| DisplayFields | GetDefaultList.ProcedureGroupNote | Date | date | 67 / 100 | 67 / 101 | 68 |
| DisplayFields | GetDefaultList.ProcedureGroupNote | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| DisplayFields | GetDefaultList.ProcedureGroupNote | Amount | money | 48 / 72 | 50 / 74 | 50 |
| DisplayFields | GetDefaultList.TreatmentPlanModule | Surf | surface | 45 / 68 | 46 / 69 | 46 |
| DisplayFields | GetDefaultList.LimitedCustomStatement | Date | date | 65 / 98 | 67 / 101 | 68 |
| DisplayFields | GetAllAvailableList.None | Date | date | 67 / 100 | 67 / 101 | 68 |
| DisplayFields | GetAllAvailableList.None | Time | time (12:00p) | 40 / 60 | 41 / 61 | 42 |
| DisplayFields | GetAllAvailableList.None | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| DisplayFields | GetAllAvailableList.None | Amount | money | 48 / 72 | 50 / 74 | 50 |
| DisplayFields | GetAllAvailableList.None | Date TP | date | 67 / 100 | 67 / 101 | 68 |
| DisplayFields | GetAllAvailableList.None | Date Entry | date | 67 / 100 | 67 / 101 | 68 |
| DisplayFields | GetAllAvailableList.AccountModule | Date | date | 65 / 98 | 67 / 101 | 68 |
| DisplayFields | GetAllAvailableList.ProcedureGroupNote | Date | date | 67 / 100 | 67 / 101 | 68 |
| DisplayFields | GetAllAvailableList.ProcedureGroupNote | Surf | surface | 40 / 60 | 46 / 69 | 46 |
| DisplayFields | GetAllAvailableList.ProcedureGroupNote | Amount | money | 48 / 72 | 50 / 74 | 50 |
| DisplayFields | GetAllAvailableList.TreatmentPlanModule | Surf | surface | 45 / 68 | 46 / 69 | 46 |
| DisplayFields | GetAllAvailableList.TreatmentPlanModule | DateTP | date | 65 / 98 | 67 / 101 | 68 |
| DisplayFields | GetAllAvailableList.LimitedCustomStatement | Date | date | 65 / 98 | 67 / 101 | 68 |
| FormApptEdit | gridProc (display fields AppointmentEdit, PlannedAppointmentEdit) | Fee (last) | money | 39 / 59 shown | 50 / 74 | 57 / 86 shown (Description 250, Priority 52) |

Donors (the grid's widths add up as before): FormRpProcNote Description 220 to 214; FormASAP Notes 157; FormAuditOrtho Log Text 566;
FormCertifications Note 204 and 164; FormClaimPayList Note 177; FormClaimPayTotal and FormEtrans835ClaimPay Description 128; FormInsPlans
Address 118; FormInsPlansMerge Plan Note 178; FormLabCases Instructions 97; FormPatientForms Description 167; FormPayPlan Description 126;
FormPayPlanDynamic Description 143; FormPreAuthReminder Description 199; FormSheetProcSelect Description 311; FormTaskEdit Note 397;
FormTrackNext, FormUnsched and FormUnschedListPatient Notes 197; FormTreatPlanCurEdit Description 189; ControlAccount's family grid
Patient 104, payment plans Guarantor 79 and Patient 78, unearned Description 177; DisplayFields: progress notes and procedure group note
Description 209 (default 194 for the group note), treatment plan 201, Account module and limited statement 267. FormClaimEdit,
FormClaimOverpay, FormExamSheets, FormIncomeTransferManage, FormPayment and the Chart's treatment plan grid have a dynamic column, which
takes the room.

## 4. Tests

`tests/UiRefreshTests/GridColumnFitTests.cs` (UiRefreshTests, no database, nothing shown):
- `Columns_OfAKnownValueType_HoldTheirWidestValue_At100And150`: every column of section 1 at 100% and 150%. On main: 56 failures.
- `DisplayFieldGrids_InTheirWindows_ShowEveryColumn`: ControlAccount's account grid and FormApptEdit's procedure grid (both appointment
  categories), built by WindowBuilder at 100% and 150%, with their default display fields. On main: the Account Date (65 px, needs 67;
  98 at 150%, needs 101) and the Edit Appointment Fee (39 px shown at 100%, 59 at 150%).
- `KnownList_EveryRowMatchesAColumn`, `GetKind_ReadsTheHeadings`.
Measuring and scanning: `GridColumnFit.cs`. The window builds run with the preference cache empty (ControlAccount's Resize handler reads
preferences only when the cache is filled) and give it back afterwards.

## 5. Gallery G-UI3-uicols

`tests/UiRefreshTests/GridColumnGallery.cs`, opt-in (`OPMS_UI_COLS_GALLERY`, `OPMS_UI_COLS_GALLERY_BEFORE`), in
`.git/od-tools/test-results/G-UI3-uicols` (`before`, `after`, and `pairs`: before above after, a magenta band between): the Account
module and Edit Appointment whole, as the harness builds them, their grids filled from the default display fields with fictional rows,
and a strip per changed grid (`Data/grid-col-gallery.tsv`: the grid's columns from its fill code, three rows, the first with each column's
widest value), at 100% and 150%. At 150% the grids paint at 144 dpi (`DesignSheetWindows.IsGridPaintedAtDpi`), as on a 150% screen.
Before: main's build at the branch point. Audit: [UI-COLS-audit.md](UI-COLS-audit.md).

## 6. Screenshots

With the fix, DocScreenshots' `account.png` and `dialog-appointment-edit.png` are committed (docs/screenshots, site/screenshots) and
shown in the README and on the site. DocScreenshots now draws the account grid and Edit Appointment's patient grid from their first row
(the module scrolls to the last row, which left half a row above the grid's top).
