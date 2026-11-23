# UI-12 slice 1: the gallery audit (G-UI3-ui12-1)

One line per picture per pass: what was checked (cut or covered text, overlaps, margins, columns and edges, gaps, crowding, empty space,
control looks, group titles, check boxes on the left) and what was wrong. The pictures: `.git/od-tools/test-results/G-UI3-ui12-1-src`
(`a1` the planner alone, `a5` after fix rounds 1 and 2, `a8` after fix round 3, `a9` the final build; `a8-150` the 150% pictures).

## Pass 1 (`a1`, then `a5`): the planner's layout, then the first two fix rounds

| Picture | Found | Fixed by |
|---|---|---|
| ControlTreat | The planner widened the Show tab, kept the stacked insurance and discount summaries over each other, moved the grids | Kept as on main (mode hand) for a later round (UI-12.md, slice 1) |
| FormASAP | The filter combo boxes 7 px under their labels and the Code Range box | Layout script (fix round 3) |
| FormAdjust | The Procedure group's fields at three x (Tooth and Description in a second band, two right columns), the TSI box off the column | Layout script, mode hand (fix round 3) |
| FormAllocationsSetup | The radio buttons' descriptions at three x per group, two right edges, the prepayment note right-aligned, the notes at two x | Layout script, mode hand (fix round 3) |
| FormArManager | Nothing: the totals the code places, the columns and the bottom row on the grid | - |
| FormCarrierEdit | The colour label covered by its picker, Is Trusted at the content edge, In Use By at main's x | Layout script on the planner's tree (round 1; Import Benefit Coinsurance under the check boxes so the 560 px window outside Canada holds it) |
| FormClaimPayEdit | Nothing | - |
| FormClaimProc | Claim Info 8 px left of Carrier Allowed Amount; the Insurance Estimate boxes 3 px narrower than the boxes around them | Layout script on the planner's tree (round 1) |
| FormClearinghouseEdit | The Description note under the clinic picker's label; the ERA list off the right column; the payor note cut after two lines | Layout script on the planner's tree (rounds 1, 3) |
| FormClinicEdit | The alias example over the e-mail box, the Time Zone note parted from its label, None at the window's edge, 1293 px wide | Layout script, mode hand (round 1) |
| FormCloneAdd | Nothing (the window keeps its height: the band under Clone is main's) | - |
| FormCodeGroupEdit | The check boxes cut as label + field rows, the window 1081 px wide | Layout script, mode hand (round 1) |
| FormDispensary | Nothing (the three filter groups centred over their grids as on main) | - |
| FormEbillSetup | Nothing (the URL is the box's content) | - |
| FormEhrLabOrderEdit2014 | The left groups across the window (1732 px), the picker buttons over their labels | Layout script, mode hand (round 1; the top row on the groups' field columns, round 2) |
| FormEhrLabResultEdit2014 | The Coded Elements and Unit of Measure first rows kept as drawn under their picker buttons; the left edge at 12 | Layout script on the planner's tree (rounds 1, 3) |
| FormEmailMessageEdit | Nothing (the preview pane is the pilot's EmailPreviewControl) | - |
| FormEmployeeEdit | Delete at 45 | Layout script on the planner's tree (round 3) |
| FormEtrans835Edit | Payee Name and Payee Medicaid ID 6 px left of the column | Layout script on the planner's tree (round 2) |
| FormFamilyBalancer | The left edge at 12 | Layout script on the planner's tree (round 3) |
| FormInsRemain | The left edge at 12 | Layout script on the planner's tree (round 3) |
| FormInsVerificationList | The Current / Past Due tab control 24 px short of the page's edge | Layout script on the planner's tree (round 2) |
| FormMedical | Nothing | - |
| FormOrthoCase | (found in pass 2) | - |
| FormPayPlan | Total Cost of Loan on the Tx amount note; the totals spread past the grid (1285 px); the Note box off the grid's edges | Layout script on the planner's tree and FormPayPlan.cs (rounds 1, 2) |
| FormPayPlanDynamic | Change Guarantor 2 px under Go To | Layout script on the planner's tree (round 2) |
| FormPayPlanTemplateEdit | Nothing | - |
| FormPaySplitEdit | Nothing (the patient list and box are switched in code) | - |
| FormProviderSetup | Instructors under the note, Alphabetize 26 px tall, Add and Student Bulk Edit off the groups' edge | Layout script on the planner's tree (round 3) |
| FormRecallSetup | The grid at 16, the right column's groups 4 px after the middle column's | Layout script on the planner's tree (round 3) |
| FormReportSetup | The Display Settings page's overlap is UserControlReportSetup's (another designer, switched by mode) | grid-known.tsv with the reason |
| FormRpAging | Two columns of groups at 12/229/235, the lists 15 and 6 px apart | Layout script on the planner's tree (round 3) |
| FormRpCustomAging | The grid at 9, the lists at 881-884, the last write-off option on three lines | Layout script on the planner's tree (round 3) |
| FormRpDPPOvercharged | Refresh 173 px right of the group, the frame at 12 | Layout script on the planner's tree (round 3) |
| FormRpEraAutoProcessed | Nothing (12 px on both sides) | - |
| FormRpLaserLabels | Main's off-page End Name label brought into the page; the left edge at 13 | Layout script on the planner's tree (rounds 1, 3) |
| FormRpOutstandingIns | The last filter column at two x; the left edge at 12 | Layout script on the planner's tree (rounds 2, 3) |
| FormRpPaySheet | Group By on the calendars' Today line (a month calendar takes its own height) | Layout script, mode hand (round 1) |
| FormRpProcNotBilledIns | The filter group cut as rows (From and To off their boxes, Only and Exclude in two columns) | Layout script, mode hand (round 1) |
| FormRpProcNote | The filter group cut as one band of rows, the window 1702 px wide | Layout script, mode hand (round 1) |
| FormRpProcOverpaid | The grid 7 px short of Refresh's edge, the frame at 12 | Layout script on the planner's tree (round 3) |
| FormRpTreatmentFinder | Nothing | - |
| FormRpUnearnedIncome | Each page's OK on its first list's row; the Line Item page's clinic list under the calendars' drawn height; the tab control at 12 | Layout script on the planner's tree (rounds 1, 3) |
| FormRxDefEdit | Delete at 36 | Layout script on the planner's tree (round 3) |
| FormRxEdit | Nothing | - |
| FormScheduleDayEdit | The top row at three heights, the tab control at 12, Delete and Practice off the group's edges | Layout script on the planner's tree (round 1) |
| FormTransworldSetup | Nothing | - |

## Pass 2 (`a8`): every after picture at 100% and the 12 pictures at 150%

| Picture | Checked | Found |
|---|---|---|
| ControlTreat | as on main | nothing new (kept for a later round) |
| FormASAP | the filter row on one top, one field column with the pages | nothing |
| FormAdjust | the field column, the lists, the Procedure group's two columns, the note, the bar | nothing |
| FormAllocationsSetup | descriptions on one x in both groups, the prepayment group, the notes on one x | nothing |
| FormArManager | filters, grid, totals row, buttons | nothing |
| FormCarrierEdit | the field column, the check boxes, the groups on the column, CDAnet | nothing |
| FormClaimPayEdit | one field column, notes after their fields, the groups | nothing |
| FormClaimProc | both columns, the groups' edges, the amounts' columns | nothing |
| FormClearinghouseEdit | the X12 group, the right column on one x, the payor note whole | nothing |
| FormClinicEdit | the two columns, the address pages, the group, the claim defaults | nothing |
| FormCloneAdd | the group, Primary Provider, Clone on the group's edge | nothing |
| FormCodeGroupEdit | the check box columns, the field column, the notes | nothing |
| FormDispensary | groups and grids | nothing |
| FormEbillSetup | the group's columns | nothing |
| FormEhrLabOrderEdit2014 | three columns of groups, the grids on the columns, the bar | nothing |
| FormEhrLabResultEdit2014 | the three columns, the picker rows | nothing |
| FormEmailMessageEdit | lists, preview pane, bar | nothing |
| FormEmployeeEdit | the field column, the bar | nothing |
| FormEtrans835Edit | both field columns, the balancing group, grids, bars | nothing |
| FormFamilyBalancer | groups, progress, output | nothing |
| FormInsRemain | grid and groups, 8 px frame | nothing |
| FormInsVerificationList | filters, tab controls to the edges, the groups | nothing |
| FormMedical | the Medical Info page | nothing |
| FormOrthoCase | groups, grid, bar | the left column's groups at 9 against Delete's 8 (fixed: layout script on the planner's tree) |
| FormPayPlan | the field column, the totals under the grid's columns, the note under Total Tx Amt, the Note box | nothing |
| FormPayPlanDynamic | the field column, Change on its row, the tab page | nothing |
| FormPayPlanTemplateEdit | the field column, the radio groups | nothing |
| FormPaySplitEdit | the field column, the tab page's two columns | nothing |
| FormProviderSetup | the right column's groups and buttons on one edge | nothing |
| FormRecallSetup | the grid at 8, the three columns 8 px apart | nothing |
| FormReportSetup | the tab page (another designer's control) | nothing new |
| FormRpAging | the two columns of groups, the lists | Only Patients With's second check box on two lines while Include Patients With's is on one (fixed: the group as tall as Include, the box on one line) |
| FormRpCustomAging | three columns, the grid to the bar | nothing |
| FormRpDPPOvercharged | the group, Refresh after it, the grid to the edges | nothing |
| FormRpEraAutoProcessed | the filter row, three grids | nothing |
| FormRpLaserLabels | the tab control at 8, the labels page | nothing |
| FormRpOutstandingIns | filters on one x per column, groups, bar | nothing |
| FormRpPaySheet | calendars, lists on one set of columns, Group By | nothing |
| FormRpProcNotBilledIns | three columns of the filter group, the grid | nothing |
| FormRpProcNote | three columns of the filter group, the grid | nothing |
| FormRpProcOverpaid | the group, Refresh, the grid to the edges | nothing |
| FormRpTreatmentFinder | the filter group, the grid, the bar | nothing |
| FormRpUnearnedIncome | each page's OK, the tab control to the edges | nothing |
| FormRxDefEdit | the field column, Delete at 8 | nothing |
| FormRxEdit | the field column, the bar | nothing |
| FormScheduleDayEdit | the top row, the tab control, the right column, the bar | nothing |
| FormTransworldSetup | the groups, the field column | nothing |
| FormAdjust-150 | one height per row, the Procedure group | nothing (labels in the zoom font: the harness) |
| FormCarrierEdit-150 | rows, groups | the bottom is cut at the harness's 1170 px capture (the window is 1492 px tall at 150%): the harness |
| FormClaimProc-150 | both columns | nothing |
| FormClearinghouseEdit-150 | rows, notes | the bottom cut at the capture (the harness) |
| FormClinicEdit-150 | both columns, the pages | nothing |
| FormEhrLabOrderEdit2014-150 | three columns, grids | nothing |
| FormEhrLabResultEdit2014-150 | columns | the window (1393 px at 150%) is cut at the capture and its bottom-anchored Add Note drawn over the groups: the harness clamps the window |
| FormInsVerificationList-150 | filters, tab controls, groups | nothing |
| FormPayPlan-150 | rows, the totals under the grid | nothing |
| FormPayPlanDynamic-150 | rows, groups | nothing |
| FormPaySplitEdit-150 | rows, the tab page | nothing |
| FormRpOutstandingIns-150 | filters, groups | nothing |

## Pass 3 (`a9`): the final build

The two pictures redrawn after pass 2 opened again: FormOrthoCase (the groups on 8, their right edges kept) and FormRpAging (both right-hand
groups of the second and third rows 75 px tall, every check box on one line): nothing found. Every other `a9` picture is byte-identical to its
`a8` picture (compared with `cmp`), and the 150% pictures' windows did not change after `a8-150`. The pass finds nothing.
