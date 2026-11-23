# UI-12 slice 1: the gallery audit (G-UI3-ui12-1)

One line per picture per pass: what was checked (cut or covered text, overlaps, margins, columns and edges, gaps, crowding, empty space,
control looks, group titles, check boxes on the left) and what was wrong. The pictures: `.git/od-tools/test-results/G-UI3-ui12-1-src`
(`a1` the planner alone, `a5` after fix rounds 1 and 2, `a8` after fix round 3, `a9` the final build; `a8-150` the 150% pictures;
the continuation: `b1`/`b2` and `b2-150` the first build of the lead's answers, `b3`, `b4` the fix rounds, `b5` the final build, with
`b5-150`, `b5-styles`, `b5-lists`; `m2`, `m2-150`, `m2-styles`, `m2-lists` the before pictures from a build of `main` at the branch point).

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

## Pass 4 (`b1`, `b2`, `b2-150`): the continuation's first build (the lead's answers of 2026-11-22)

What changed since pass 3: the clinic picker's label area sized from its text (the box of every picker moved onto its column again), the
report windows on the 8 px margin, ControlTreat's layout script, the rounding rows pinned, the whole window at 150%. Every picture opened.

| Picture | Found | Fixed by |
|---|---|---|
| ControlTreat | The parts on one top and height, the right column, the summaries stacked; the Show page's check boxes 17 px (the fit fixes gave 18 and 19) | Fix round 6: 18 px rows, the top row 4 px taller (`b5`) |
| FormASAP | The clinic picker's box on the filter row's column | nothing |
| FormAdjust | "Clinic" whole, the box on the field column | nothing |
| FormAllocationsSetup | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormArManager | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormCarrierEdit | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormClaimPayEdit | the picker's box on the column (the width grew by the label area) | nothing |
| FormClaimProc | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormClearinghouseEdit | the picker after the Description note, its label whole | nothing |
| FormClinicEdit | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormCloneAdd | Clone between Primary Provider and the hidden Specialty and Clinic rows (shown by Load): the action stands among the fields | Fix round 5: a layout script (the rows under Primary Provider, Clone bottom right) |
| FormCodeGroupEdit | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormDispensary | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormEbillSetup | the picker's box on the address column | nothing |
| FormEhrLabOrderEdit2014 | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormEhrLabResultEdit2014 | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormEmailMessageEdit | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormEmployeeEdit | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormEtrans835Edit | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormFamilyBalancer | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormInsRemain | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormInsVerificationList | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormMedical | Med Urgent 9 px and Service Notes 17 px over their boxes (the captions under them 2 px); Service Notes 2 px short of Medical Summary's bottom; the wide boxes ending at two x short of the page's edge | Fix round 5: a layout script |
| FormOrthoCase | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormPayPlan | the picker in its group on the Provider column | nothing |
| FormPayPlanDynamic | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormPayPlanTemplateEdit | the picker on the field column | nothing |
| FormPaySplitEdit | the picker on the field column (the band under it: the hidden rows' place, as main) | nothing |
| FormProviderSetup | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormRecallSetup | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormReportSetup | The Display Settings page shows UserControlReportSetup's instructions under its user group row (the harness shows both modes) | Fix round 5: the gallery draws the page's mode (FitGallery) |
| FormRpAging | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormRpCustomAging | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormRpDPPOvercharged | "Clinics" whole, the box on the Providers column | nothing |
| FormRpEraAutoProcessed | the 8 px margin both sides, the filter row's gaps kept, "Clinics" whole | nothing |
| FormRpLaserLabels | Two field columns 43 px apart on Patient Labels (Start Name at 159, End Name and the check boxes at 202) | Fix round 5: one column per page (the Insurance Company page too) |
| FormRpOutstandingIns | "Clinics" whole, the box on the column | nothing |
| FormRpPaySheet | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormRpProcNotBilledIns | "Clinics" whole, the box on Procedure Codes' column | nothing |
| FormRpProcNote | "Clinics" whole, Clinics and Providers on one column | nothing |
| FormRpProcOverpaid | "Clinics" whole, the box on the Providers column | nothing |
| FormRpTreatmentFinder | The picker reads "Clinic" (the harness builds without Load, which makes it multi select): its box 5 px left of the Provider column | Fix round 5: the gallery draws it as Load does (the box on the column) |
| FormRpUnearnedIncome | each page's content from the page's 8 px edge, OK 8 px from the corner | nothing |
| FormRxDefEdit | Sig 4 px short of Drug's right edge; the RxNorm button 4 px past the notes' edge | Fix round 5: the layout script |
| FormRxEdit | the picker on the field column | nothing |
| FormScheduleDayEdit | the picker ending at the content edge | nothing |
| FormTransworldSetup | opened: the same picture as pass 3 (`a9`, byte for byte) | nothing |
| FormAdjust-150 | the whole window | The picker's box 18 px left of its column: the harness drew its label at the 100% font (fix round 5: the harness scales the picker's font as Windows does), then 3 px left (a text at 150% is narrower than 1.5 times its 100% width; fix round 5b: the label area scaled from its 96 dpi width) |
| FormCarrierEdit-150 | the whole window (1492 px), every row | nothing |
| FormClaimProc-150 | the whole window | nothing |
| FormClearinghouseEdit-150 | the whole window | the picker as FormAdjust-150 (fixed the same way) |
| FormClinicEdit-150 | both columns, the pages | nothing |
| FormEhrLabOrderEdit2014-150 | the whole window, the grids | nothing |
| FormEhrLabResultEdit2014-150 | the whole window, Add Note under the groups (no longer drawn over them) | nothing |
| FormInsVerificationList-150 | filters, tab controls, groups | nothing |
| FormPayPlan-150 | the whole window | the picker as FormAdjust-150 |
| FormPayPlanDynamic-150 | the whole window | nothing |
| FormPaySplitEdit-150 | rows, the tab page | the picker as FormAdjust-150 |
| FormRpOutstandingIns-150 | the whole window | the picker as FormAdjust-150 |

## Pass 5 (`b5`, then `b6`): the continuation's final build

Compared with `cmp`: of the `b5` pictures only ControlTreat differs from `b4` (fix round 6); the other 46 slice pictures, the 12 at 150%, the
8 style pictures and the 25 list-control strips are byte-identical to `b4`. Opened again: every picture that changed after pass 4's `b2`
pictures (the fix rounds 5 and 6: five at 100%, five at 150%) and the four style pictures of the gallery, which pass 4 did not list. `b6`
(fix round 7, a harness change only) differs from `b5` in ControlTreat alone; its OpenPmsDental.dll is `b5`'s (sha256 `a1da07e6ad6f2cbc`
in both bins), so the 150% pictures are `b5`'s.

| Picture | Found | Fixed by |
|---|---|---|
| ControlTreat (`b5`) | The top row on one top and height (155), the Show page's seven check boxes 18 px apart, the right column's captions 2 px over their list and date, the summaries stacked; but the discount plan summary drawn over the family insurance summary on its place ("ary" of Secondary showing past it): FillSummary shows one or the other, the harness drew both (main's picture too) | Fix round 7: the gallery hides the discount summary, as FillSummary does for a patient without a discount plan (`FitGallery.FillSampleContent`, `b6`) |
| ControlTreat (`b6`) | The family and individual insurance summaries 4 px apart, Set Priority's list 4 px over them, the Procedures grid 4 px from the right column; "Ded Remai" is DashIndividualInsurance's own drawing (the dashboard surface, owner-drawn, left out of the slice; the same on main) | nothing (the "Ded Remain" cut: for the lead) |
| FormCloneAdd | Specialty and Clinic rows' place under Primary Provider (shown by Load), Clone at the bottom right | nothing |
| FormMedical | The captions 2 px over their boxes, Service Notes and Medical History to the page's content edge, Service Notes ending on Medical Summary's bottom | nothing |
| FormReportSetup | The Display Settings page in its display mode (the instructions, no user group row), Up and Down ending on the grid's edge | nothing |
| FormRpLaserLabels | One field column on Patient Labels (the boxes, the check boxes) | nothing |
| FormRxDefEdit | Sig as wide as Drug, the RxNorm button ending on the notes' edge | nothing |
| FormAdjust-150 | The whole window; the picker's box 1 px left of the Provider column (170 against 171: LayoutManager rounds the picker's left, 106.5, and its label area, 64.5, each on its own, as it rounds any control at an odd 96 dpi position; 0 px at 100%) | nothing (rounding) |
| FormClearinghouseEdit-150 | The whole window (1335 px), the picker's box after "Clinic" | nothing |
| FormPayPlan-150 | The whole window; the picker in its group, its box 1 px left of the Provider column (as FormAdjust-150) | nothing (rounding) |
| FormPaySplitEdit-150 | The whole window, the picker's box on the field column | nothing |
| FormRpOutstandingIns-150 | The whole window, "Clinics" whole, the box on the Treat Provs column | nothing |
| clinic-picker-100, clinic-picker-150 (styles) | The WinForms picker's states: "Clinic" right-aligned before the box; the second row (the WPF ComboClinic, WpfControlsOD) byte-identical to main's | nothing |
| FormPayment-inputs-100, FormPayment-inputs-150 (styles) | Every input kind of FormPayment, the picker's box on the field column; "Clinic"'s ink 10 px from its box, the labels under it 7-8 (the picker draws its text with TextRenderer 1 px left of where a Label draws it, and "c" ends 1 px inside its cell: the drawing the pilot's fix round 2 set and the owner approved) | nothing |

At 150% the clinic picker's label, like the text in the text boxes, date pickers and up-down boxes, is drawn at the font Windows gives the
scaled control, the harness's other labels at its zoom font (UI-12.md, "Slice 1, continuation", the 150% pictures), so "Clinic" reads larger
than the labels beside it in the 150% pictures only. The pass finds nothing more: `b6` is the gallery's after set.
