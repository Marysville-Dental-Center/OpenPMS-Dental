# UI-COLS: the audit of gallery G-UI3-uicols

The pictures of `.git/od-tools/test-results/G-UI3-uicols` (`before`, `after`, `pairs`), drawn by `GridColumnGallery` (UI-COLS.md
section 5): the Account module and Edit Appointment whole, and 36 strips (one per changed grid), each at 100% and 150%, 80 pictures per set.
Before = `main` at the branch point (46b9c66d0), built and drawn in this worktree before the product change; after = the branch head. Every
pair was opened (the 100% strips on contact sheets of six pairs, the 150% after pictures on three sheets, the two windows on their own) and
looked at for cut or wrapped values, headings, scroll bars and the room left for the other columns.

## Pass 1 (after = babc72f43)

- **Edit Appointment, 100% and 150%.** Before: Fee cut to "1260.0" past the grid's edge behind a horizontal scroll bar; the Priority
  heading cut ("Priorit"). After: Fee, Priority and every value whole, no horizontal scroll bar. Found: Description at 238 wrapped
  "Resin composite, four or more surfaces, posterior" while Fee had 20 px to spare. Fixed: Description 250 (522 px in all, Fee 57 px beside
  the vertical scroll bar).
- **Account module, 100% and 150%.** Before: every date wrapped ("06/10/202" over "5"), each row two lines high. After: one line per row,
  the dates whole. The date stands 1 px before the gridline at 100%, as 9,999.99 does in the program's 50 px money columns (GDI+ keeps its
  own side bearing); not widened further (the test's rule is the value on one line, and the Description gives the room).
- **Strips, 100%.** Before: the dates wrapped in every changed date column, MODBL wrapped in the surface columns, phone numbers wrapped
  after "(555)555-". After: whole on one line. Found: in FormAuditOrtho, FormLabCases and FormTaskEdit the date and time "01/02/2026 11:15
  AM" still wrapped at 122 px (AM is wider than PM in Segoe UI; the test's value was 12:00 PM). Fixed: the test's values are 12/31/2026
  11:00 AM and 11:00 AM; the three columns 123.

## Pass 2 (after = de5149d09, the final pictures)

- Edit Appointment and the Account module at 100% and 150%: nothing cut, no horizontal scroll bar; at 150% the grids paint at 144 dpi and
  every value fits.
- The 36 strips at 100% and 150%: every date, date and time, time, phone number, surface and amount of the changed columns on one line.
  FormExamSheets' 11:15 AM fits its 56 px column.

What stays as drawn, and why (harness, not the program):
- A strip is exactly as wide as its columns' widths, so the vertical scroll bar that its three rows (with a long wrapped note) bring takes
  its width from the last column: ControlAccount's Bal and Total, the treatment plan's Pat, the limited statement's Credits, FormPayPlan's
  Balance, FormSheetProcSelect's Amount are cut in both sets. In the program the last column fills the grid's width (GridOD.ComputeColumns).
- "Sample" in narrow columns without a value type (flags, Type, Status) wraps; FormRpProcNote's strip shows both of its fill code's
  branches (Code and Description twice). Same in both sets.
- The strips of FormIncomeTransferManage ("Paid By") and FormPayPlanDynamic ("Due") put a money value or a date in a column that holds a
  name or money: the gallery takes the value from the heading; the test's known list has Due as money, and Paid By is a dynamic column (not
  checked).
- Labels outside the grids keep the harness's 150% font pass (GridStyleGallery.ScaleFontsAsWindowsDoes), as in the other galleries.

## The README and site screenshots

`account.png` and `dialog-appointment-edit.png` (DocScreenshots, fictional data of DocSampleData, 1440x865 and the window's size): opened.
First run: the account grid scrolled to its last row left half a row above its top, and Edit Appointment's patient grid stood a few pixels
down; DocScreenshots now draws both from their first row. Final pictures: Account dates, codes, amounts and balances whole, rows one
date line high, the grid from its first row (its last visible row continues under the edge, the scroll bar shows more); Edit Appointment's
procedure grid with all four fees (1260.00 whole), no horizontal scroll bar, the patient grid from First Name. Fictional names only
(Sampleton, DDEV, RHYG, Example Dental Benefits, 555-0101). Kept as on main: the quick-add list's first two names are wider than the list
("CompEx-4BW-Pano-Pro-F...", a list box, not a grid column) and the clinic box shows 0 (clinics off in the sample practice).
