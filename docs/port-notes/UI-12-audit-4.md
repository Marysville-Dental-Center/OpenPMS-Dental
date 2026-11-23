# UI-12 slice 4: the audit of gallery G-UI3-ui12-4

Every after picture of `.git/od-tools/test-results/G-UI3-ui12-4` (the slice's 104 windows at 100%, the 16 densest at 150%, and the two
earlier windows that host a slice designer), opened and looked at as a designer would: cut or covered text, overlaps, margins, columns
and edges, gaps, crowding and empty bands, the control looks, titles, boxes on the left. Before = `main` at the branch point (`33d9fce1f`),
drawn in the same session as the after pictures (UI-12.md, "Slice 4 (batch 2)"). A line names the pass that found something and what was
done; the last pass finds nothing in any picture. What stays as drawn, and why, is in the line (a window never shrinks; a row the code
shows or hides keeps its room; owner-drawn surfaces keep their place).

**Pass 1** (`after-100` of the first `after` step: the planner and its templates alone): 25 windows with findings. Overlaps and covered
text: FormTimeAdjustEdit's and FormEquipment's radio buttons drawn into each other, FormRpActivePatients', FormRpPPOwriteoffs' and
FormRpWriteoffSheet's captions and radio buttons on the calendars' Today line, FormClaimAttachPasteDXC's warning over Paste Again,
FormReplicationSetup's two notes, UserControlSetupWizClinic's two notes, UserControlReportSetup's row past the page. Cut text:
FormClaimForms' note, FormEhrSummaryOfCare's note, FormIncomeTransferManage's description, FormSecurityLock's explanation,
GraphScheduleDay's Employees box, FormGlobalPaymentsV2's runtime card text, FormCCDPrint's page label. Columns and edges: FormEvaluationReport's
All boxes away from their grids, FormRpRouting's All boxes and date buttons, FormLimitedStatementSelect's To picker, FormPatientStatusTool's
Since label and bottom combo, the clinic pickers of FormRecurringChargesHistory and FormRpInsPendingSupplementals, FormHL7MsgEdit's AptNum
gap, FormRpInsPayPlansPastDue's Provs label, FormEmployeeSelect's note, the reconcile windows' Add buttons and grid edges (and
FormReconcileProblem's stray empty button, a designer leftover), FormTimeCardSetup's and FormTimeCardManage's buttons, FormWikiListHeaders'
icon buttons, the left margins of FormAdjustSelect, FormAdjustmentPicker, FormEhrLabPanels, FormEhrMedicalOrderLabEdit, FormDefEditImages,
FormScheduledProcesses, FormApptsOther's bottom row, FormReferralProcTrack's empty band. Fixes: 40 layout scripts on the planner's tree
(UI-12.md, "Slice 4 (batch 2)").
**Pass 2** (`a2-100`, after the scripts): FormRpPPOwriteoffs' carrier caption ran into the write-offs group (GridRulesTests' overlap; two
lines now), FormAdjustSelect's amounts 2 px short of a bold five-digit amount (71 px now), FormTimeCardManage's Clear buttons 4 px past the
grid's edge (on it now). All fixed.
**Pass 3** (`after-100` of the second `after` step): FormRpPPOwriteoffs whole; nothing else changed and nothing found.
**Pass 4** (the final build, every picture below): nothing found.
**Fix round** (the lead's review of the slice, 35 findings; branch `ui/ui12-4-2`; UI-12.md, "Slice 4, fix round"): the windows of the findings
laid out again from the branch point with their scripts (26 new), drawn again from the fix round's builds in one session (`work/f1`, `f2`,
`f2-150`), their lines below marked "fixed (fix round)". **Fix pass 1** (`f1`): the reconcile windows' Add text still stood against the right
border at 72 px (main's MiddleRight alignment: centred now), FormAutoCodeEdit kept a 56 px band where the note had been (the grid down to the
Save row now), FormXWebTransactions kept 34 px under its grid (12 px now). **Fix pass 2** (`f2`, `f2-150`, the final build): nothing found.

## 100%

| Picture | Status | What was checked and found |
|---|---|---|
| FormAccountPick | ok | Grid at 8 to 8, Include Inactive under it, OK bottom right. |
| FormAccounting | ok | Menu and toolbar docked; the As of Date row under them, the grid to the edges. |
| FormAdjustSelect | fixed (pass 1, 2) | Grid and note were at 13 with 15 at the right: at 8, Breakdown at the right edge; amounts 71 px for "$99,999.99" bold. |
| FormAdjustmentPicker | fixed (pass 1) | Grid at 12: 8 to 8, Show Unattached Only at 8. |
| FormAlertCategorySetup | ok | Two grids 12 px from both edges (symmetric), Copy centred between them, Duplicate under the right grid's end. |
| FormApptTypes | fixed (fix round) | No fixed point in the planner (Add, Up, Down 32 px lower each pass, 68 px under the grid's top): Add on the grid's top, Up and Down under it, on OK's column (a script). |
| FormApptViews | ok | Main's arrangement on the grid (no OK button: the window closes from its title bar); the radio list in its group, the list buttons two by two. |
| FormApptsOther | fixed (pass 1) | Bottom row started at 31: from the grid's x, 6 px apart; Entire Family over Schedule Recall; grid and notes to OK's edge. |
| FormAuditOrtho | fixed (fix round) | The grids 21 px over the bottom, 12 px at the sides and 6 px apart: 8 px from every edge, 8 px apart. |
| FormAutoCodeEdit | fixed (fix round) | Save off the grid's edge and the note floating above it: Save on the grid's right edge, the note 6 px before it on its row, the grid down to 8 px over the row. |
| FormAutoNoteEdit | ok | Name row, the Text caption and box, Insert centred between box and grid, Add over the grid's end. |
| FormCCDPrint | fixed (pass 1) | Bottom bar at 2 px, three heights: one row, 24 px buttons, the page label 44 px centred (the code centres it). |
| FormCDSSetup | ok | Radio list, the note right-aligned on the grid's edge, Save. |
| FormCarriers | fixed (fix round) | Add and Combine (anchored to the bottom) were at the column's top over the ITRANS group: at its bottom, Combine on the grid's bottom edge, Add over it (main's place). |
| FormCentralReportSetup | fixed (fix round) | The picture drew both of the page's modes over each other: now the permission mode both callers open (user group row, Set All, Set None, the count), whole and on one row. |
| FormClaimAttachPasteDXC | fixed (pass 1) | Warning cut and over Paste Again: under the narrative box on the column's width; Paste Again and Send on one row. |
| FormClaimAttachment | fixed (fix round) | The Save note under the validation box, three right edges, the counter and the grid off the narrative box: the note 6 px before Save on its row; box, side buttons (as wide as their caption) and Save on the right content edge; the counter on the narrative box's edge, the grid on its x. |
| FormClaimForms | fixed (fix round) | Copy 17 px after the left grid and 7 px before the right: centred between them. |
| FormClaimPayEditOld | fixed (fix round) | Delete and its note (anchored to the bottom) were under Show Unattached: at the column's bottom, the note ending on the grid's bottom edge; Clinic, Carrier Name and Note end on one edge (the planner's widths pinned). |
| FormClaimPayList | ok | Two field columns and Refresh over the grid, Add. |
| FormClearinghouses | fixed (fix round) | Set Default's buttons off-centre, the two groups of two heights 57 px apart: Set Default 8 px around its buttons, the report group 8 px after it, both 132 px tall. |
| FormDefEditImages | fixed (pass 1) | Name caption centred at 23: left-aligned at 8; Usage group at the top to the right edge. |
| FormEdgeExpressTrans | fixed (fix round) | The hidden Cash Back and Transaction ID pairs were split about 110 px apart: both pairs (alternatives) on one band under the list, caption over box; the check boxes 8 px under the band. |
| FormEhrEncryption | fixed (fix round) | The result box 11 px right of its caption and Transmit off its edge, the window growing each pass: the box under its caption, Transmit on its right edge, the window 12 px after them as on the left. |
| FormEhrLabPanels | fixed (pass 1) | Content at 21 with OK at the right edge: at 8, the grid to the right edge. |
| FormEhrMedicalOrderLabEdit | fixed (pass 1) | Grid at 12 under a box at 8: at 8. |
| FormEhrMedicalOrders | ok | Check box, grid, two-line note. |
| FormEhrPatListResults | fixed (fix round) | Print (anchored to the bottom) was under the Order By group: on the grid's bottom edge. |
| FormEhrPatientExport | ok | Two field columns and Search, grid, Export Selected and Select All. |
| FormEhrProviderKeyEdit | ok | Note, four field rows with their notes, Delete and Save. |
| FormEhrReminders | fixed (fix round) | Edit 7 px past the grids, margins 12 and 15: grids 12 px from each side, Edit on their edge, the contact box 6 px before Edit. |
| FormEhrSummaryOfCare | fixed (pass 1) | Note's third line cut: left column 220 px at 8, right column 8 px after it. |
| FormEmployeeSelect | fixed (fix round) | Working Office was under the bottom row and Working From Home 40 px tall: both one line beside Furloughed and Non-Furloughed; the window 8 px under the bottom row. |
| FormEncounterTool | ok | Note, date row, combo, Description box, note, four code buttons, Code, warning, Run. |
| FormEquipment | fixed (pass 1) | Radio buttons drawn into each other: a list at the list pitch, the grid 8 px under it. |
| FormEtrans277Edit | ok | Three field columns, Raw Message, grid. |
| FormEtrans834Preview | ok | Note, three check boxes, grid, Save. |
| FormEtrans835ClaimSelect | ok | Patient with Find, date column, Claim Fee, Refresh, two grids, OK. |
| FormEvaluationReport | fixed (pass 1) | All Courses stood over the Instructors grid: each All at its grid's x; grids one width 8 to 8. |
| FormEvaluations | fixed (fix round) | The grid 41 px over the bottom and the Date row and Course ending 22 px short: the grid 12 px over the bottom, Date (two boxes of one width, "to" between) and Course end with the groups' fields, Refresh, Add and Reports. |
| FormGlobalPaymentsV2 | fixed (fix round) | Right edges at 391, 421, 424 and 432: the terminal combo to the Card group's content edge, the void box and the result panel to the window's. |
| FormHL7DefMessageEdit | fixed (fix round) | The hidden delete note had a row of its own (56 px empty band): the button row 8 px under the grid with the note between Delete and Add, Delete on the grid's x, the window 525 px tall. |
| FormHL7MsgEdit | fixed (pass 1) | 8 px gaps before AptNum and Message Text: on the 27 px pitch. |
| FormHL7Msgs | fixed (fix round) | No fixed point in the planner (the status combo and Refresh 2 px further each pass): pinned at the planner's first pass (a script). |
| FormImagingDeviceEdit | fixed (fix round) | Computer Name's box and This 4 px under their label, the group 5 px off the field column, Device Type 5 px wider: all on the label's row and the field column, the label right-aligned with the others. |
| FormIncomeTransferManage | fixed (fix round) | The transfer group past the grids and the window 11 px wider: the group ends on the grids' edge (its note wraps), the window 1073 (12 px margins), Transfer on the right grid's x. |
| FormInsAdj | ok | Note, three field rows, Delete and Save. |
| FormInsEditPatLog | ok | Date range row (its week buttons hidden), grid. |
| FormInstallmentPlanEdit | ok | Five field rows (APR with its note), Note box, Delete and Save. |
| FormLetterMerges | ok | Two lists with captions and buttons, Chart Letter, the Create group, Edit Template. |
| FormLimitedStatementSelect | fixed (pass 1) | To picker 16 px left of From: on From's x, its label 6 px before its box. |
| FormMedLabs | ok | View group (dates with note, patient buttons, two check boxes, Refresh, clinic), grid. |
| FormMedicationReconcile | fixed (fix round) | Add Medication 29 px off its grid and the window 13 px wider for a one-line note: Add on the grid's x, the event note wrapping to the grids' edge, the window 918 (12 px margins), the picture note across. |
| FormPatientForms | ok | Menu and toolbar docked, the two option groups, grid. |
| FormPatientStatusTool | fixed (fix round) | A 36 px band under the bottom row and Run and Create List 7 px short of the grid's edge: the window 8 px under the row, both buttons on the grid's right edge. |
| FormPaymentPlanOptions | ok | Note, radio list, note. |
| FormPerioSetup | ok | Note, four field rows, the Recession check box, Set All 323 and Save. |
| FormProcBroken | fixed (fix round) | The clinic box 8 px short of Provider, the two Auto Note buttons different: the box on Provider's edge, both buttons one x and size. |
| FormProcButtonQuickEdit | ok | Three field rows with the picker button, Display as Label, Delete and Save. |
| FormRecallListUndo | fixed (fix round) | Patients Affected and its count were alone at the middle right: on the OK row, 6 px before OK. |
| FormReconcileAllergy | fixed (fix round) | Add's text ended 1 px from the border (main's right-aligned text): 72 px with the text centred after the icon. |
| FormReconcileEdit | ok | Grid, field column with Locked, bottom bar. |
| FormReconcileMedication | fixed (fix round) | As FormReconcileAllergy: Add 72 px, text centred after the icon. |
| FormReconcileProblem | fixed (fix round) | As FormReconcileAllergy: Add 72 px, text centred after the icon. |
| FormRecurringChargesHistory | fixed (pass 1) | Clinic picker's box 6 px left of Status: on Status's edges. |
| FormReferralProcTrack | fixed (pass 1) | 60 px empty band under Print: the grid down to Print, Print 8 px over the bottom. |
| FormReminderRuleEdit | ok | Four field rows with the note and picker button, Delete and Save. |
| FormReplicationSetup | fixed (fix round) | Add (anchored to the bottom) was at the grid's top: on the grid's bottom edge 6 px after it; the notes wrap to 6 px before the Replication Failure group, which stands beside them on the band's top; Load takes the band's height off the window and keeps the designer's width (no literal 901 x 640). |
| FormReqAppt | ok | Two grids, the field column, Add/Remove, the attached grid, Save. |
| FormRpActivePatients | fixed (pass 1) | Captions on the calendars' Today line, All boxes off their lists: four columns under the calendars' drawn height. |
| FormRpAdjSheet | fixed (fix round) | The Clinics caption and All 23 px right of their list, the captions 2 px under the calendars' Today band, From 3 px off its list: the column on its list's x, captions 8 px under the calendars' drawn bottom, From on the list's x, TO centred between the calendars. |
| FormRpConfirm | ok | Two lists with centred captions, OK; the window keeps main's width. |
| FormRpFinanceCharge | fixed (fix round) | The Billing Types column stood 2 px lower than Providers: one caption top and height, one All row, one list top, the Date Range group on the lists' top. |
| FormRpInsOverpaid | fixed (fix round) | The Clinics caption was under the From calendar and the All box 3 px off its list: the column 8 px under the calendar's drawn bottom on the list's x, the radio buttons and note moved down with it, the window 23 px taller. |
| FormRpInsPayPlansPastDue | fixed (pass 1) | Row at 13 with Provs at 8: one row at 8, the labels centred. |
| FormRpInsPendingSupplementals | fixed (pass 1) | Clinic picker's box 35 px left of Type: on Type's edges. |
| FormRpPPOwriteoffs | fixed (pass 1, 2) | Radio buttons on the Today line: the left column under the calendars, the caption on two lines; the group on the second calendar's x. |
| FormRpPatPortionUncollected | ok | Date range row with week buttons, Clinics, Refresh, the check box, grid, Print. |
| FormRpProcSheet | fixed (fix round) | The Clinics caption touched the To calendar's Today band and its column lined up with nothing: the column under the To calendar's left edge 8 px under its drawn bottom; the code caption on its row, its box on the All row. |
| FormRpRecall | ok | Two lists with centred captions, OK (disabled until a field is picked). |
| FormRpReceivablesBreakdown | fixed (fix round) | The write-offs group ran under the calendar's top-left corner: 258 px, ending on the Clinic list's edge 6 px before the calendar. |
| FormRpRouting | fixed (pass 1) | All boxes 100 px from their lists, Today/Displayed alone: columns with their boxes, the Date row with its two buttons. |
| FormRpServiceDateView | ok | Filters group, Refresh, grid, the three bottom buttons. |
| FormRpUdsPlus | ok | Date caption and range, Clinics, Providers list, the UDS lines note (109 px for "UDS lines set up: 99"), Setup, note, bottom bar. |
| FormRpWriteoffSheet | fixed (pass 1) | Clinics caption on the Today line: Clinics and Providers columns after the calendars, the group under them. |
| FormScheduledProcesses | fixed (pass 1) | Grid at 30: at 8, down to the bottom. |
| FormScreenGroups | fixed (fix round) | The grid 32 px over the bottom and Add 34 px short of its edge: the grid 12 px over the bottom, Add on its right edge. |
| FormSecurityLock | fixed (pass 1) | Explanation cut: twelve lines, 182 px; the rows and Save under it (window 15 px taller). |
| FormSheetFieldSigBox | ok | Six field rows, three check boxes on the field column, Delete and Save. |
| FormSigButDefEdit | ok | Radio list with the computer box, field rows with the Sync Icon note, Delete and Save. |
| FormSupplyOrders | fixed (fix round) | The supplier combo was 60 px (supplier names cut at run time): 160 px on the grids' right edge, Supplier and Show received before it. |
| FormSupplyUsageReport | ok | Date row with radio buttons and Refresh, grid, total (300 px for "Total cost: $99,999.99"), Print and Close. |
| FormTaskOptions | ok | Check list with the date row, Save. |
| FormTimeAdjustEdit | fixed (pass 1) | Radio buttons drawn into each other in the label column: a list on the field column, the note after Manually entered. |
| FormTimeCardManage | fixed (pass 1, 2) | Clear Manual Adjusts' text filled its button: 132 px each, on the grid's right edge. |
| FormTimeCardSetup | fixed (fix round) | The rules grid 67 px further right and 4 px higher, the window 1067 wide: 8 px after the pay period grid on its top and height, both button rows on one line, Options 8 px under them, the window 1020 again. |
| FormTrophyNamePick | ok | Note, grid, New Patient beside it, OK. |
| FormWikiListHeaders | fixed (fix round) | The two grids at 73 and 72 and their captions at 8 and 9: one caption top and bottom, both grids 2 px under them. |
| FormWikiListHistory | ok | Three grids 6 px apart, Revert. |
| FormXWebTransactions | fixed (fix round) | Refresh 10 px short of the grid's edge and 34 px under the grid: Refresh on the edge (the clinic combo before it), the grid 12 px over the bottom. |
| FormXchargeTrans | ok | Transaction list with caption, check list, Save. |
| GraphScheduleDay | fixed (fix round) | The three groups 71 px tall for one row: 54 px (8 px under the row), the graph's panels 17 px higher. |
| UserControlReportSetup | fixed (fix round) | The picture drew both modes over each other: now the display mode alone (instructions, Up and Down 75 px, the icon apart from the text); the permission mode is its own picture. |
| UserControlReportSetup-permission | new (fix round) | The page in its permission mode (FormReportSetup's Security Permissions page): User Group, Set All, Set None and the allowed count on one row, whole; the grid under them. No before picture (main drew both modes at once). |
| UserControlSetupWizClinic | fixed (pass 1) | Notes over each other and past the edge: instructions and red-items note on one band, the right column 140 px at the edge. |

## 150%

| Picture | Status | What was checked and found |
|---|---|---|
| FormClaimForms-150 | fixed (fix round) | Copy centred between the grids. |
| FormEhrSummaryOfCare-150 | ok | Note's three lines whole, the columns. |
| FormEvaluations-150 | fixed (fix round) | The grid 12 px over the bottom, the Date row and Course on the fields' edge. |
| FormGlobalPaymentsV2-150 | fixed (fix round) | The combo, the void box and the result panel on their containers' edges. |
| FormIncomeTransferManage-150 | fixed (fix round) | The group on the grids' edge, its note whole on four lines, Transfer on the right grid's x. |
| FormPatientStatusTool-150 | fixed (fix round) | The bottom row 8 px over the bottom, Run and Create List on the grid's edge. |
| FormProcBroken-150 | fixed (fix round) | The clinic box on Provider's edge, both Auto Note buttons alike. |
| FormReplicationSetup-150 | fixed (fix round) | Add on the grid's bottom edge, the notes beside their buttons wrapping before the failure group. |
| FormRpActivePatients-150 | ok | Columns whole; the month calendars draw at their own size in the harness (as slice 2's report windows), so a band stands under them at 150%. |
| FormRpPPOwriteoffs-150 | ok | Left column and caption under the calendars, the group. |
| FormRpRouting-150 | ok | Columns with their boxes, the Date row. |
| FormRpWriteoffSheet-150 | ok | Columns after the calendars, the group under them. |
| FormSecurityLock-150 | ok | Explanation whole, rows, Save. |
| FormTimeAdjustEdit-150 | ok | Radio list, field rows, notes. |
| GraphScheduleDay-150 | fixed (fix round) | The groups as tall as their row, Employees whole. |
| UserControlSetupWizClinic-150 | ok | The band and the right column's notes whole. |

## Hosts (earlier slices' windows with a slice 4 designer inside)

| Picture | Status | What was checked and found |
|---|---|---|
| FormReportSetup (slice 1) | fixed (fix round) | The page in its display mode: Up and Down 75 px (the arrow apart from Down's text), the grid under them; before = slice 1's after picture. |
| FormScheduleDayEdit (slice 1) | ok | The window opens on its List tab (GraphScheduleDay is on the Graph tab), so the picture is the same as slice 1's after picture; the graph's options row is GraphScheduleDay's picture above. |
