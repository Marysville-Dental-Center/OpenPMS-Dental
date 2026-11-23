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

## 100%

| Picture | Status | What was checked and found |
|---|---|---|
| FormAccountPick | ok | Grid at 8 to 8, Include Inactive under it, OK bottom right. |
| FormAccounting | ok | Menu and toolbar docked; the As of Date row under them, the grid to the edges. |
| FormAdjustSelect | fixed (pass 1, 2) | Grid and note were at 13 with 15 at the right: at 8, Breakdown at the right edge; amounts 71 px for "$99,999.99" bold. |
| FormAdjustmentPicker | fixed (pass 1) | Grid at 12: 8 to 8, Show Unattached Only at 8. |
| FormAlertCategorySetup | ok | Two grids 12 px from both edges (symmetric), Copy centred between them, Duplicate under the right grid's end. |
| FormApptTypes | ok | Two check boxes, the grid, Add/Up/Down beside it, OK; the planner has no fixed point here (re-plan rows, UI-12.md). |
| FormApptViews | ok | Main's arrangement on the grid (no OK button: the window closes from its title bar); the radio list in its group, the list buttons two by two. |
| FormApptsOther | fixed (pass 1) | Bottom row started at 31: from the grid's x, 6 px apart; Entire Family over Schedule Recall; grid and notes to OK's edge. |
| FormAuditOrtho | ok | Two grids 12 px from both edges, 6 px apart. |
| FormAutoCodeEdit | ok | Description row, the check box on the field column, the note over the grid, Add/Delete/Save bar; margins 36/40 as main. |
| FormAutoNoteEdit | ok | Name row, the Text caption and box, Insert centred between box and grid, Add over the grid's end. |
| FormCCDPrint | fixed (pass 1) | Bottom bar at 2 px, three heights: one row, 24 px buttons, the page label 44 px centred (the code centres it). |
| FormCDSSetup | ok | Radio list, the note right-aligned on the grid's edge, Save. |
| FormCarriers | ok | Filter row, grid, Add/Combine column, OK. |
| FormCentralReportSetup | fixed (pass 1) | Tab control at 6, 4: at 8 to 8; the page (UserControlReportSetup) draws both of its modes at once (the program shows one). |
| FormClaimAttachPasteDXC | fixed (pass 1) | Warning cut and over Paste Again: under the narrative box on the column's width; Paste Again and Send on one row. |
| FormClaimAttachment | ok | Grid and its buttons, Narrative and Claim Validation Status side by side with their captions, the send note, Save. |
| FormClaimForms | fixed (pass 1) | The two-line note was cut by the grids: 30 px, the grids 8 px under it. |
| FormClaimPayEditOld | ok | Grid left, field column right, Note caption and box, Delete with its note. |
| FormClaimPayList | ok | Two field columns and Refresh over the grid, Add. |
| FormClearinghouses | ok | Note and clinic row, grid, the Set Default and report groups, Add with its note, Save. |
| FormDefEditImages | fixed (pass 1) | Name caption centred at 23: left-aligned at 8; Usage group at the top to the right edge. |
| FormEdgeExpressTrans | ok | Transaction list, card method group, the band of the cash back and transaction rows (shown by the code), the check list, Save. |
| FormEhrEncryption | ok | Two boxes with captions, Encrypt/Decrypt centred between them, Transmit under Result; no planner fixed point (re-plan rows). |
| FormEhrLabPanels | fixed (pass 1) | Content at 21 with OK at the right edge: at 8, the grid to the right edge. |
| FormEhrMedicalOrderLabEdit | fixed (pass 1) | Grid at 12 under a box at 8: at 8. |
| FormEhrMedicalOrders | ok | Check box, grid, two-line note. |
| FormEhrPatListResults | ok | Grid, Order By group, Print. |
| FormEhrPatientExport | ok | Two field columns and Search, grid, Export Selected and Select All. |
| FormEhrProviderKeyEdit | ok | Note, four field rows with their notes, Delete and Save. |
| FormEhrReminders | ok | Field row with Edit, two grids, Send centred, Delete. |
| FormEhrSummaryOfCare | fixed (pass 1) | Note's third line cut: left column 220 px at 8, right column 8 px after it. |
| FormEmployeeSelect | fixed (pass 1) | Note after Delete All top-aligned: centred on the button. |
| FormEncounterTool | ok | Note, date row, combo, Description box, note, four code buttons, Code, warning, Run. |
| FormEquipment | fixed (pass 1) | Radio buttons drawn into each other: a list at the list pitch, the grid 8 px under it. |
| FormEtrans277Edit | ok | Three field columns, Raw Message, grid. |
| FormEtrans834Preview | ok | Note, three check boxes, grid, Save. |
| FormEtrans835ClaimSelect | ok | Patient with Find, date column, Claim Fee, Refresh, two grids, OK. |
| FormEvaluationReport | fixed (pass 1) | All Courses stood over the Instructors grid: each All at its grid's x; grids one width 8 to 8. |
| FormEvaluations | ok | Grid, date row, Course, the two filter groups, Add and Reports. |
| FormGlobalPaymentsV2 | fixed (pass 1) | Saved card 82 px for "Saved card ending 1234": 149 px; Sale/Refund/Void 6 px apart. |
| FormHL7DefMessageEdit | ok | Two combos, radio list, Item Order, Note, grid, Delete/Add/Save. |
| FormHL7MsgEdit | fixed (pass 1) | 8 px gaps before AptNum and Message Text: on the 27 px pitch. |
| FormHL7Msgs | ok | View group (two field columns, the patient buttons, HL7Status, Refresh), grid. |
| FormImagingDeviceEdit | ok | Field rows with their notes and This, Device Type group, Twain Name, Show Twain UI with note, bottom bar. |
| FormIncomeTransferManage | fixed (pass 1) | Description cut ("call your su"): 108 px, the grids 8 px under it. |
| FormInsAdj | ok | Note, three field rows, Delete and Save. |
| FormInsEditPatLog | ok | Date range row (its week buttons hidden), grid. |
| FormInstallmentPlanEdit | ok | Five field rows (APR with its note), Note box, Delete and Save. |
| FormLetterMerges | ok | Two lists with captions and buttons, Chart Letter, the Create group, Edit Template. |
| FormLimitedStatementSelect | fixed (pass 1) | To picker 16 px left of From: on From's x, its label 6 px before its box. |
| FormMedLabs | ok | View group (dates with note, patient buttons, two check boxes, Refresh, clinic), grid. |
| FormMedicationReconcile | ok | Image row, picture box, two grids with Add/Delete and note, the bottom note. |
| FormPatientForms | ok | Menu and toolbar docked, the two option groups, grid. |
| FormPatientStatusTool | fixed (pass 1) | Since 4 px high; bottom combo off the date column: Since on its row, the combo on the clinic box's column. |
| FormPaymentPlanOptions | ok | Note, radio list, note. |
| FormPerioSetup | ok | Note, four field rows, the Recession check box, Set All 323 and Save. |
| FormProcBroken | ok | Field rows with their notes, Auto Note, two note boxes, Save; the amount note 260 px for "$99,999.99: Non-Refundable portion". |
| FormProcButtonQuickEdit | ok | Three field rows with the picker button, Display as Label, Delete and Save. |
| FormRecallListUndo | ok | Note, Date picker, Patients Affected with its count, OK. |
| FormReconcileAllergy | fixed (pass 1) | Source grids short of the reconciled grid's edge, Add 51 px: one width, Add 60 px under each grid's end. |
| FormReconcileEdit | ok | Grid, field column with Locked, bottom bar. |
| FormReconcileMedication | fixed (pass 1) | As FormReconcileAllergy. |
| FormReconcileProblem | fixed (pass 1) | As FormReconcileAllergy, and the empty textless button at 0, 0 (a designer leftover, no handler) removed. |
| FormRecurringChargesHistory | fixed (pass 1) | Clinic picker's box 6 px left of Status: on Status's edges. |
| FormReferralProcTrack | fixed (pass 1) | 60 px empty band under Print: the grid down to Print, Print 8 px over the bottom. |
| FormReminderRuleEdit | ok | Four field rows with the note and picker button, Delete and Save. |
| FormReplicationSetup | fixed (pass 1) | The two notes drawn into each other, the first cut: each beside its button, two lines, to the grid's edge. |
| FormReqAppt | ok | Two grids, the field column, Add/Remove, the attached grid, Save. |
| FormRpActivePatients | fixed (pass 1) | Captions on the calendars' Today line, All boxes off their lists: four columns under the calendars' drawn height. |
| FormRpAdjSheet | ok | Calendars, Providers column, the two lists under the calendars with captions and All boxes. |
| FormRpConfirm | ok | Two lists with centred captions, OK; the window keeps main's width. |
| FormRpFinanceCharge | ok | Two list columns with All boxes, Date Range group, OK. |
| FormRpInsOverpaid | ok | Note, calendars, All with its list, radio list, note, OK. |
| FormRpInsPayPlansPastDue | fixed (pass 1) | Row at 13 with Provs at 8: one row at 8, the labels centred. |
| FormRpInsPendingSupplementals | fixed (pass 1) | Clinic picker's box 35 px left of Type: on Type's edges. |
| FormRpPPOwriteoffs | fixed (pass 1, 2) | Radio buttons on the Today line: the left column under the calendars, the caption on two lines; the group on the second calendar's x. |
| FormRpPatPortionUncollected | ok | Date range row with week buttons, Clinics, Refresh, the check box, grid, Print. |
| FormRpProcSheet | ok | Calendars, Providers column, the code note and box, Clinics column. |
| FormRpRecall | ok | Two lists with centred captions, OK (disabled until a field is picked). |
| FormRpReceivablesBreakdown | ok | Note, two list columns, the date caption and calendar, the write-offs group, OK. |
| FormRpRouting | fixed (pass 1) | All boxes 100 px from their lists, Today/Displayed alone: columns with their boxes, the Date row with its two buttons. |
| FormRpServiceDateView | ok | Filters group, Refresh, grid, the three bottom buttons. |
| FormRpUdsPlus | ok | Date caption and range, Clinics, Providers list, the UDS lines note (109 px for "UDS lines set up: 99"), Setup, note, bottom bar. |
| FormRpWriteoffSheet | fixed (pass 1) | Clinics caption on the Today line: Clinics and Providers columns after the calendars, the group under them. |
| FormScheduledProcesses | fixed (pass 1) | Grid at 30: at 8, down to the bottom. |
| FormScreenGroups | ok | Navigation row, date row with Refresh and Add, grid; 13 px each side. |
| FormSecurityLock | fixed (pass 1) | Explanation cut: twelve lines, 182 px; the rows and Save under it (window 15 px taller). |
| FormSheetFieldSigBox | ok | Six field rows, three check boxes on the field column, Delete and Save. |
| FormSigButDefEdit | ok | Radio list with the computer box, field rows with the Sync Icon note, Delete and Save. |
| FormSupplyOrders | ok | New Order, Show received, Supplier, two grids, bottom bar. |
| FormSupplyUsageReport | ok | Date row with radio buttons and Refresh, grid, total (300 px for "Total cost: $99,999.99"), Print and Close. |
| FormTaskOptions | ok | Check list with the date row, Save. |
| FormTimeAdjustEdit | fixed (pass 1) | Radio buttons drawn into each other in the label column: a list on the field column, the note after Manually entered. |
| FormTimeCardManage | fixed (pass 1, 2) | Clear Manual Adjusts' text filled its button: 132 px each, on the grid's right edge. |
| FormTimeCardSetup | fixed (pass 1) | Hide pay periods 120 px off its grid, Delete Selected short of the grid edges: at the grid's x, each on its grid's edge. |
| FormTrophyNamePick | ok | Note, grid, New Patient beside it, OK. |
| FormWikiListHeaders | fixed (pass 1) | Remove's text against its icon: Add and Remove 84 px. |
| FormWikiListHistory | ok | Three grids 6 px apart, Revert. |
| FormXWebTransactions | ok | Date row, Clinic, Refresh, grid. |
| FormXchargeTrans | ok | Transaction list with caption, check list, Save. |
| GraphScheduleDay | fixed (pass 1) | Employees wrapped and cut: 81 px; the three groups one row; the graph (owner-drawn) unchanged. The Employees box's background is the employees' bar colour (the code sets it). |
| UserControlReportSetup | fixed (pass 1) | Row past the page: instructions 340 px with Up/Down at the edge, the user group row and the allowed count (148 px) within 525 px; both modes drawn at once (the program shows one). |
| UserControlSetupWizClinic | fixed (pass 1) | Notes over each other and past the edge: instructions and red-items note on one band, the right column 140 px at the edge. |

## 150%

| Picture | Status | What was checked and found |
|---|---|---|
| FormClaimForms-150 | ok | Note whole, grids under it, the groups' buttons. |
| FormEhrSummaryOfCare-150 | ok | Note's three lines whole, the columns. |
| FormEvaluations-150 | ok | Date row, filters groups, buttons. |
| FormGlobalPaymentsV2-150 | ok | Radio rows, the combo, field rows. |
| FormIncomeTransferManage-150 | ok | Description whole over the grids. |
| FormPatientStatusTool-150 | ok | Criteria row, bottom row. |
| FormProcBroken-150 | ok | Field rows, notes, Auto Note. |
| FormReplicationSetup-150 | ok | The two notes beside their buttons, Force Sync group. |
| FormRpActivePatients-150 | ok | Columns whole; the month calendars draw at their own size in the harness (as slice 2's report windows), so a band stands under them at 150%. |
| FormRpPPOwriteoffs-150 | ok | Left column and caption under the calendars, the group. |
| FormRpRouting-150 | ok | Columns with their boxes, the Date row. |
| FormRpWriteoffSheet-150 | ok | Columns after the calendars, the group under them. |
| FormSecurityLock-150 | ok | Explanation whole, rows, Save. |
| FormTimeAdjustEdit-150 | ok | Radio list, field rows, notes. |
| GraphScheduleDay-150 | ok | Three groups, Employees whole. |
| UserControlSetupWizClinic-150 | ok | The band and the right column's notes whole. |

## Hosts (earlier slices' windows with a slice 4 designer inside)

| Picture | Status | What was checked and found |
|---|---|---|
| FormReportSetup (slice 1) | ok | The page in its display mode (instructions, Up and Down at the page's right edge, the grid under them); before = slice 1's after picture. |
| FormScheduleDayEdit (slice 1) | ok | The window opens on its List tab (GraphScheduleDay is on the Graph tab), so the picture is the same as slice 1's after picture; the graph's options row is GraphScheduleDay's picture above. |
