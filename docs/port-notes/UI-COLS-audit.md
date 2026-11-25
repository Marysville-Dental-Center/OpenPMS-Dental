# UI-COLS: the audit of gallery G-UI3-uicols

The pictures of `.git/od-tools/test-results/G-UI3-uicols` (`before`, `after`, `pairs`), drawn by `GridColumnGallery` (UI-COLS.md
section 5): the Account module and Edit Appointment whole, and 36 strips (one per changed grid), each at 100% and 150%, 80 pictures per set.
Before = `main` at the branch point (46b9c66d0), built and drawn in this worktree before the product change; after = the branch head. Every
pair was opened (the 100% strips on contact sheets of six pairs, the 150% after pictures on three sheets, the two windows on their own) and
looked at for cut or wrapped values, headings, scroll bars and the room left for the other columns.

## Pass 1 (after = babc72f43)

- **Edit Appointment, 100% and 150%.** Before: Fee cut to "1260.0" past the grid's edge behind a horizontal scroll bar; the Priority
  heading cut ("Priorit"). After: Fee, Priority and every value whole, no horizontal scroll bar. Found: Description at 238 wrapped
  "Resin composite, four or more surfaces, posterior" while Fee had 20 px to spare. Changed to Description 250 (522 px in all, Fee 57 px
  beside the vertical scroll bar), which did not fix it: at 250 the D2394 row still wrapped onto two lines at 100% and 150% (its
  description needs 272). Pass 2 missed it; the review found it; fix round 2 below.
- **Account module, 100% and 150%.** Before: every date wrapped ("06/10/202" over "5"), each row two lines high. After: one line per row,
  the dates whole. The date stands 1 px before the gridline at 100%, as 9,999.99 does in the program's 50 px money columns (GDI+ keeps its
  own side bearing); not widened further (the test's rule is the value on one line, and the Description gives the room). That was
  wrong: the date ran into the gridline and the next value ("06/10/2025|Alex"), in the README screenshot too; fixed in round 2.
- **Strips, 100%.** Before: the dates wrapped in every changed date column, MODBL wrapped in the surface columns, phone numbers wrapped
  after "(555)555-". After: whole on one line. Found: in FormAuditOrtho, FormLabCases and FormTaskEdit the date and time "01/02/2026 11:15
  AM" still wrapped at 122 px (AM is wider than PM in Segoe UI; the test's value was 12:00 PM). Fixed: the test's values are 12/31/2026
  11:00 AM and 11:00 AM; the three columns 123.

## Pass 2 (after = de5149d09, the final pictures)

- Edit Appointment and the Account module at 100% and 150%: nothing cut, no horizontal scroll bar; at 150% the grids paint at 144 dpi.
  This pass said every value fit; it did not see three faults the review found in these pictures: Edit Appointment's D2394 row on two
  lines (Description 250), the dates 1 px from the gridline (68 px), and in the progress-notes strips (DisplayFields-1 and -2) the Amount
  heading cut to "Amoun" at 100% (and at 150% in the None list). All three are fixed in round 2.
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

## Fix round 2 (branch ui/uicols-2; after = the final Targeted run's pictures)

The review of pass 2 found three faults in the final pictures (above). Fixed, with the test holding each rule (UI-COLS.md sections 1, 2
and 3b): Edit Appointment's Description 272 (Priority 50, Surf 46, Code 41, Fee 53, still 522 px); dates 70 (centered 71) and dates and
times 125, so a value stands at least 2 px clear of the gridline; the progress notes' Amount 54 and Proc Code 64 for their headings. The
new rules also found 35 column sites outside round 1's list (section 3b); their grids were added to the gallery (after pictures only).
Looked at, at full size or zoomed:
- **Edit Appointment, 100% and 150%** (after/FormApptEdit.png, -150): every row one line, the D2394 row "Resin composite, four or more
  surfaces, posterior" included; 1260.00 whole in Fee beside the vertical scroll bar; no horizontal scroll bar; Priority, Surf (MODBL),
  Code (D2394) and their headings whole.
- **Account module, 100%** (after/ControlAccount.png, zoomed, and a pixel scan of row 1): "06/10/2025" ends 3 px before the gridline,
  "Alex" 2 px after it (was 1 px before at 68).
- **Strips, 100%**: DisplayFields-1 and -2 (Amount whole at 54), DisplayFields-3 (treatment plan, Allowed whole), FormCertifications-1,
  FormPayPlan-1 (Date 71, Adjustment whole), FormUnschedListPatient-1, FormLabCases-1, FormInsPlans-1, FormExamSheets-1,
  ControlAccount-2, FormTaskEdit-1, FormPayPlanDynamic-2, and the new FormASAP, FormRpClaimNotSent, FormCareCreditTransactions,
  FormMedLabs, FormEquipment, FormRecallsPat and FormProcEdit-3 strips: values and the changed headings whole, the dates clear of the
  gridline. **150%**: FormASAP-1, DisplayFields-1, FormProcEdit-3, FormQueryMonitor-1: the same.
- The rest of the 130 pictures were not opened one by one in this round; the test checks each changed column at 100% and 150%.

What stays as drawn:
- FormPayPlanDynamic-2 shows the first line of its two-line headings ("Date" of "Date Showing"): the strip's header is one line high, while
  the program's grid gives those headings two lines. The strip leaves out its columns of computed width (Clinic, Description).
- Headings still cut in columns the value-type reading does not cover, the same on main: ControlAccount's payment plans "Total Cost"
  ("Total Cos") and "Due Now", the treatment plan's "Discount", narrow "Type" columns. Outside this round (the test checks the headings of
  value columns: dates, times, money by its usual headings, phones, teeth, surfaces, codes).
- Money in right-aligned 50 px columns (9,999.99) stands about 1 px from the left gridline, as on main: money is held to one line only
  (UI-COLS.md section 1).
- The harness notes of pass 2 (last column under the scroll bar, "Sample" wrapping in narrow columns).

## The README and site screenshots

`account.png` and `dialog-appointment-edit.png` (DocScreenshots, fictional data of DocSampleData, 1440x865 and the window's size): opened.
First run: the account grid scrolled to its last row left half a row above its top, and Edit Appointment's patient grid stood a few pixels
down; DocScreenshots now draws both from their first row. Final pictures: Account dates, codes, amounts and balances whole, rows one
date line high, the grid from its first row (its last visible row continues under the edge, the scroll bar shows more); Edit Appointment's
procedure grid with all four fees (1260.00 whole), no horizontal scroll bar, the patient grid from First Name. Fictional names only
(Sampleton, DDEV, RHYG, Example Dental Benefits, 555-0101). Kept as on main: the quick-add list's first two names are wider than the list
("CompEx-4BW-Pano-Pro-F...", a list box, not a grid column) and the clinic box shows 0 (clinics off in the sample practice).
Fix round 2 redrew both (same sizes, 1440x865 and 1332x773) and recommitted them in docs/screenshots and site/screenshots: in
account.png "06/10/2025" now ends at x=118 with the gridline at x=122 and "Alex" at x=125 (was the gridline at 120, "Alex" at 122);
dialog-appointment-edit.png's procedure grid with Description 272, every row on one line, the four fees whole.
