# UI-12 slice 6: the audit of gallery G-UI3-ui12-6

Every after picture of `.git/od-tools/test-results/G-UI3-ui12-6` (the slice's 97 windows at 100%, the 15 densest at 150%), opened and
looked at as a designer would: cut or covered text, overlaps, margins, columns and edges, gaps, crowding and empty bands, the control
looks, titles, boxes on the left. Before = `main` at the branch point (`fce94f468`), drawn by the `before` step in the same capture session
as the after pictures (UI-12.md, "Slice 6 (batch 2)"). A line names the pass that found something and what was done; the last pass finds
nothing in any picture. What stays as drawn, and why, is in the line (a window never shrinks; a row the code shows or hides keeps its room;
a band where a run-time text goes keeps its height).

**Pass 1** (`work/a1-100`, the first `after` step: the planner and its templates alone): 83 windows with findings. Margins: most list
windows kept main's 11 to 27 px left margin with right edges 4 to 20 px short of Save or OK and bottom rows 20 to 40 px over the bottom
(the planner keeps anchored grids where main drew them). Side-by-side grids and lists at two tops, two widths, 6 px apart. Cut text:
FormAutomationEdit's two-line label, the "includes 2 files" notes of FormEhrClinicalSummary and FormEhrElectronicCopy, FormEmailTemplateEdit's
replacement fields caption, FormLetterEdit's Body of Letter label, FormUnschedListSetup's note, SheetEditMobileCtrl's drag note, FormPrinterEdit's
14 px rule line (its run-time text takes two lines). Covered text: FormTaskInboxSetup's grid over the instructions and UserControlSetupWizOperatory's
red-items note over the instructions (both main too), FormXChargeReconcile's calendars over two button rows, FormDatabaseMaintTemp's Step 1
caption into its combo. Rows: FormClaimPayTotal's totals row taken apart, FormInventorySetup's check box put on the field column,
FormSupplies' Up parted from Down, FormTimeCardRuleEdit's check boxes at two x, FormEhrPatientSelectSimple's search rows a third down the
grid. Window sizes: FormMaintRecordEdit grown from 760 to 1033 px and FormSupplyStock from 1084 to 1636 px with empty middles. Fixes: 83
layout scripts on the planner's tree (UI-12.md, "Slice 6 (batch 2)").
**Pass 2** (`work/a2-100`, after the scripts): FormDatabaseMaintTemp's Step 5 note right-aligned in its box (main's alignment), FormRequiredFields'
Field Type combo narrower than its list, FormSpellCheck's Add 7 px under the box it takes the word from, UserControlSetupWizOperatory's
red-items note 45 px tall for two lines; GridRulesTests' cut text: FormEmailTemplateEdit's caption ("Replacement" wraps at 76 px),
FormLetterEdit's label, FormTaskInboxSetup's instructions and FormUnschedListSetup's note a few px short of their measured lines, and
FormDatabaseMaintInvalidChars' explanation sized for its designer placeholder. All fixed.
**Pass 3** (`work/a3-100`): the fixed windows whole; then, checking the window sizes against `main` for the docs, FormQueryParser grown to 839 x
718 (past the 720 px window height its Show Text button sets), FormQueryMonitor grown by 22 x 14 and FormPopupDisplay from 425 to 634 px
for a group that is never shown, the two split containers' panels moved to 8, 8: fixed (main's sizes, the panels at 0, 0).
**Final pass** (`work/final-100`, `work/final-150`, the final build after the whole filter built: every picture of the gallery): nothing
found. Of the 97 pictures at 100%, 10 differ from the pictures opened in passes 1 to 3 (the three windows above, the seven whose placeholder
labels now draw their run-time texts) and were opened again; the other 87 are pixel-identical (SHA-256 of the pixels,
`work/final-identical.txt`) to a picture opened in pass 1 (14 windows without a script), pass 2 (66) or pass 3 (7). The 15 pictures at 150%
were all opened.
**Fix round** (`ui/ui12-6-2`, the lead's review of 2026-11-23: 17 findings in 18 windows and the two hosts of DashIndividualDiscount; `work/f1-100`, `work/f1-150`, the fix build after the whole filter built): the 18 fixed pictures at 100% and the 3 at 150% opened; the other 79 and 12 are pixel-identical to the final pass's (the same capture session). Passes 1 to 3 had missed: four windows that kept the planner's layout with content off the 8 px padding (FormTxtMsgEdit, FormBlockoutCutCopyPaste, FormElectIDs, FormProcApptColorEdit), three icon buttons at 23 px, unequal inner margins in six groups, a cut hosted user control (DashIndividualDiscount at its hosts' size) and four rows off their neighbours. Each line below says what was fixed. A last pass over the 21 changed pictures finds nothing.

## 100%

| Picture | Status | What was checked and found |
|---|---|---|
| DashIndividualDiscount | fixed (fix round) | Pass 1: nothing found. Fix round (review 1): the control had grown to 190 x 124 with the group at 8, 8 while its hosts keep 170 x 84 / 173 x 84 (third row cut there). The group at 0, 0, the control 174 x 108; ControlTreat and ControlAccount give it that size. Opened: the three rows and the group's border whole, 8 px inside on the right. |
| FormAutomationEdit | fixed (pass 1) | Pass 1: labelProcCodes 2 lines 29 tall (needs 32), gap Trigger->ProcCodes 30 not 27; butDelete (local var) left at x48 y393 off Save row -> x8 on Save row. Pass 2: rows on the pitch, the two-line label centred on its field, grid with Add on the right edge, Delete on Save's row. |
| FormBlockoutCutCopyPaste | fixed (fix round) | Pass 1: (groups 190 wide in 290 window: as main). Fix round (reviews 2, 11): the column at 26 with 92 px empty on the right, Clipboard Contents 21 px short, Repeat 23 px. Now Clear Day and both groups at 8, one right edge (211) for the check box and both groups, the window 219 wide; the note 4 lines, the box and the # box 8 px inside, Repeat 24. Opened: margins 8 on every side, no text cut. |
| FormCDSILabResult | fixed (fix round) | Pass 1: nothing found. Fix round (review 12): the SNOMED description box ended 14 px from the group's border. Now 283 wide, ending with the units combo 8 px inside. Opened: both groups' right edges match. |
| FormCanadaSummaryReconciliation | fixed (fix round) | Pass 1: right edges: group 309, Treating Provider list 284, date field 297 -> list and date field to 309. Pass 2: list and date row on the group's right edge. Fix round (review 6): the Carrier and Network lists ended 18 px short of the group's border. Now 286 wide (8 px on both sides). Opened. |
| FormClaimAttachSnipDXC | fixed (pass 1) | Pass 1: the warning at 19, the preview 76 px short of the right edge, the Send buttons 22 and 24 px short of it and 27 px over the bottom. Pass 2: warning from the padding, the preview and the Send buttons on the right edge. |
| FormClaimCreate | ok | Pass 1: nothing found. |
| FormClaimOverpay | fixed (pass 1) | Pass 1: 40 px empty band between Totals row and bottom bar; grid down so totals row 8 above bar. Pass 2: Totals row 8 px over the bar. |
| FormClaimPayTotal | fixed (pass 1) | Pass 1: script - planner broke totals row (DedApplied 5 px low, overlaps); captions far from their buttons; restore main's arrangement on grid. Pass 2: the totals row one line of abutting boxes, the captions bottom-aligned over their buttons, the note under the boxes. |
| FormClaimResend | ok | Pass 1: nothing found. |
| FormClinics | fixed (pass 1) | Pass 1: Add ends 902 vs groups/Save 906; grid/label at 12/14 vs 8. Pass 2: grid and note at the padding, 8 px before the groups, Add on their edge. |
| FormCodeGroups | fixed (pass 1) | Pass 1: grid at 12, note at 10; buttons end 643, Save 656, Show Hidden 652. Pass 2: grid at the padding, buttons and Save on one right edge. |
| FormCodeSystemsImport | fixed (pass 1) | Pass 1: grids/note at 12 (right 13), buttons 28 from bottom -> 8 margins, buttons 8 from bottom. Pass 2: content on the padding, buttons 8 px over the bottom. |
| FormDashboardWidgetSetup | fixed (fix round) | Pass 1: grids at 12, Add ends 806 vs Custom grid 847 vs Save 860 -> margins 8, one right edge. Pass 2: grids one width, top and bottom; Copy centred between; top row from the Custom grid's x. Fix round (review 4): Add 65 x 23, its icon over the bottom-left border. Now 65 x 24 on the top row's 8. Opened: the border whole. |
| FormDatabaseMaintInvalidChars | fixed (pass 1, 2) | Pass 1: margins 12; labelExplain runtime text (4 lines) height; runtime rows labelExplain/labelCannotFix/labelSummary. Pass 2: the explanation 18 px tall (the script measured the designer text; the runtime text takes three lines): 50 px, redrawn in pass 3. Final: the explanation (its run-time text, now drawn) on its three lines, the cannot-fix line. |
| FormDatabaseMaintTemp | fixed (fix round) | Pass 1: script - step labels: Step1 label runs into combo; Step2/4 labels right-aligned; Fix group labels; one step column. Pass 2: Step 5's note right-aligned in its box (main's MiddleRight): TopLeft. Fix round (review 3): Print 75 x 23, its icon over the bottom-left border. Now 75 x 24 as on main, the Step 4 row 24 px. Opened at 100% and 150%: the border whole. |
| FormEcwDiagAdv | fixed (pass 1) | Pass 1: margins 12 -> 8, Run at right edge. Pass 2: content on the padding, Run on the right edge, grids to the bottom padding. |
| FormEhrClinicalSummary | fixed (pass 1) | Pass 1: label1 cut (viewin), grid right 248 vs groups 264. Pass 2: the note's third line whole, grid on the groups' edge. |
| FormEhrEduResourcesPat | fixed (pass 1) | Pass 1: margins 12, Delete 35 from bottom. Pass 2: content on the padding, Delete 8 px over the bottom. |
| FormEhrElectronicCopy | fixed (pass 1) | Pass 1: label2 cut; left edge 27; right edges 404/345. Pass 2: everything from the padding, the note's third line whole. |
| FormEhrLabNoteEdit | fixed (pass 1) | Pass 1: margins 12; Add ends 702 vs Save 706; Delete note floats above Delete -> beside it. Pass 2: Add on Save's edge, the note beside Delete. |
| FormEhrLabOrderImport | ok | Pass 1: the band over Save is the run-time note labelExistingLab (hidden in the designer, shown by the code). |
| FormEhrLabResultEdit | fixed (fix round) | Pass 1: groups at x52 while Delete at 8 -> groups 8..right edge. Pass 2: groups from the padding to the right edge, one field column with Date Time. Fix round (review 13): a 39 px band over Delete and Save, both 28 px tall. Now 75 x 24, 8 px under the Test Result group, the window 303 tall. Opened at 100% and 150%. |
| FormEhrPatientSelectSimple | fixed (pass 1) | Pass 1: search cluster at y120 -> top; Search ends 761 vs 765. Pass 2: the search rows at the grid's top, Search on the fields' edge. |
| FormEhrTimeSynch | fixed (pass 1) | Pass 1: 56 px band over the check box (main's) -> Message box grows to 12 over it; Measure/fields right edge. Pass 2: the explanation from the padding to Measure's edge (the band over the check box is labelSetClock, the time offer the code shows). |
| FormElectIDEdit | fixed (pass 1) | Pass 1: 40 px under Attributes grid -> grid to 8 over Save. Pass 2: grid and Payer Service on Save's edge, 8 px over Save. |
| FormElectIDs | fixed (fix round) | Pass 1: nothing found. Fix round (review 2): Add at 7 under the grid at 8. Now at 8 (script: frame). Opened. |
| FormEmailAddresses | fixed (pass 1) | Pass 1: margins 16 -> 8; Add ends 868 vs group 900. Pass 2: content from the padding, the group and Add one width on the right edge. |
| FormEmailAutographEdit | fixed (pass 1) | Pass 1: text box ends 957 vs Save 965. Pass 2: fields on Save's right edge. |
| FormEmailTemplateEdit | fixed (pass 1, 2) | Pass 1: script - rows under Attach (Description y52), Body box x106 vs fields 97, label4 cut (3 lines), Edit buttons x97. Pass 2: the replacement fields caption cut at 76 px ("Replacement" wraps; GridRulesTests): the group 8 px wider, the field column at 116; redrawn in pass 3. |
| FormEtrans270EBraw | fixed (pass 1) | Pass 1: margins 14, grid 37 from bottom. Pass 2: content from the padding to the right edge and the bottom padding. |
| FormFeeSchedGroupEdit | fixed (fix round) | Pass 1: grids 18..250/334..566 -> 8..581; 40 px under grids. Pass 2: grids one width, arrows centred between, Delete on Save's row. Fix round (review 17): Description ended 5 px before the '...' button. Now 157 wide, ending with it. Opened. |
| FormFeeSchedGroups | fixed (pass 1) | Pass 1: grids x10/458, tops 70/68, bottoms 646/644 -> 8, one top/bottom, R=W-8. Pass 2: filter group and Groups grid one width, grids one top and bottom, Add Group 8 px over the bottom. |
| FormFieldDefLink | fixed (pass 1) | Pass 1: margins 16; 40 px under grids. Pass 2: grids one width, arrows centred, 8 px over Save. |
| FormGlidewellSetup | fixed (pass 1) | Pass 1: margins 12, right edges 482/567/581/585 -> one (band 348-428 = runtime labels BaaConfirmed/Transport: kept). Pass 2: content from the padding to the right edge, Add on it (the band under the check box: the two run-time lines). |
| FormHL7Defs | fixed (pass 1) | Pass 1: grids tops 36/40, margins 12, History x38. Pass 2: History at the padding, grids one top and width, Copy and Duplicate centred under them. |
| FormImageCatMerge | fixed (pass 1) | Pass 1: Change buttons end 19 px short of group content edge; Merge 590 vs group 592. Pass 2: Change on the groups' content edge, Merge on the groups' edge. |
| FormImagePickerDXC | fixed (pass 1) | Pass 1: script - right column: captions at 1023 off both columns, right edges 1215/1219, margins 10. Pass 2: right column on one label column and one right edge, the Send buttons stacked on it, the panels from the padding to the bottom padding. |
| FormInnoDb | fixed (pass 1) | Pass 1: margins 14, button row 22 from bottom. Pass 2: content on the padding, the buttons 8 px over the bottom. |
| FormInsFilingCodeEdit | fixed (pass 1) | Pass 1: Delete at x27 -> 8. Pass 2: Delete at the padding. |
| FormInsPlanSelectFam | fixed (pass 1) | Pass 1: grid at 22, list ends 735 vs OK 786. Pass 2: check box and grid from the padding, the list on the right edge, both 8 px over the bottom row. |
| FormInventorySetup | fixed (fix round) | Pass 1: script - planner put the check box on the field column (x245); check at 8, note under its text, the expiring row. Pass 2: check box at the padding, its note under its text, the expiry row from the padding. Fix round (review 15): three left edges (8, 20, 24). The expiry caption at 8 as wide as its text, left-aligned, the box and its note 6 px after it. Opened: the check box and the expiry row start at the padding, the note under the check box's text. |
| FormInvoiceItemSelect | fixed (pass 1) | Pass 1: margins 12; Exclude $0 Fee wraps in 108 px group. Pass 2: Exclude $0 Fee on one line, All and None over OK on the group's x. |
| FormJournal | fixed (pass 1) | Pass 1: filter row gaps uneven (61/110/27/67) -> one band gap. Pass 2: the filter pairs 16 px apart from the padding, (optional) centred under Amt Max. |
| FormLetterEdit | fixed (pass 1, 2) | Pass 1: label7 5 lines cut (+13); body ends 790 vs Save 795; 33 px under body. Pass 2: the Body of Letter label 8 px short of its five lines (GridRulesTests): 8 px taller; redrawn in pass 3. Final: the label 8 px taller, whole. |
| FormMaintHistory | fixed (pass 1) | Pass 1: margins 12; grids end 1087/1001 vs Close 1092; filter row centred at 479 -> from left. Pass 2: filter row on the right edge (the item line, the code's text, at the padding), grids and buttons on one right edge. Final: the item line drawn with its run-time text at the padding. |
| FormMaintRecordEdit | fixed (pass 1) | Pass 1: script - planner grew window 760->1033 for the Date done row (runtime labels); back to 760, one row: labelDue / Date done + field / labelDoneBy; Note row. Pass 2: main's width again, one row of due line, Date done and done-by line (run-time texts), grids on the padding, Note row. Final: the due and done-by lines drawn with their run-time texts on the Date done row (the item line over the description is the item's name, empty in the harness). |
| FormMarkupTableEdit | fixed (pass 1) | Pass 1: margins 12; Up/Down 44 vs 70; Man Edit/Paste Cells 75 vs groups 88. Pass 2: right column one width, the group buttons one width (L and R a pair). |
| FormMedLabPatSelect | fixed (pass 1) | Pass 1: margins 12; grids end 811 vs OK 816; 34 px under grid. Pass 2: grids from the padding to the right edge (the band over OK: the run-time existing-lab note). |
| FormPatFieldDefs | fixed (fix round) | Pass 1: margins 16, top band 24, buttons 414..492 vs R 504. Pass 2: content from the padding under the menu, buttons on the right edge. Fix round (review 14): Add 26 px tall, 9 px above the grid. Now 79 x 24 on the grid's top, Up and Down 4 px apart under it. Opened. |
| FormPatienteBill | ok | Pass 1: nothing found. |
| FormPayPlanRecalculate | fixed (pass 1) | Pass 1: group ends 281 vs note/Save 371. Pass 2: note and group from the padding to the right edge. |
| FormPayorTypes | fixed (pass 1) | Pass 1: margins 22, Add 613..687 vs R 694, bottom 15. Pass 2: grid from the padding to 8 px before Add, down to the bottom padding. |
| FormPopupDisplay | fixed (pass 1, final) | Pass 1: description box at x53 on no column -> x8 to the hidden Disable group's x-8; group at top. Pass 2: the description box from the padding to 8 px before the Disable for group (hidden in the designer, shown by the code). Final: main's 425 px width; the description box from the padding to the right edge. |
| FormPrinterEdit | fixed (pass 1) | Pass 1: script - labelPrinterRule runtime text 2 lines in 14 px; label2 at 12; rows; ODtextBox 21 tall. Pass 2: rows on one field column, the band under Printer is the rule line (two lines of run-time text), notes beside their controls. Final: the rule line drawn with its two-line run-time text, the status after the combo. |
| FormProcApptColorEdit | fixed (fix round) | Pass 1: nothing found. Fix round (review 2): Delete at 6. Now at 8 (script: frame). Opened. |
| FormProcCodesOther | fixed (pass 1) | Pass 1: left column at 12 vs right edge 8. Pass 2: content from the padding to the right edge. |
| FormProcEditAll | ok | Pass 1: nothing found. |
| FormProgramLinks | fixed (pass 1) | Pass 1: margins 15/16, bottom 24. Pass 2: grid from the padding to the right edge, Add and its note 8 px over the bottom. |
| FormProvStudentBulkEdit | fixed (fix round) | Pass 1: grid at 12, gap to right column 5 px, 28 under grid. Pass 2: grid from the padding, 8 px before the right column, down to the bottom padding. Fix round (review 8): Save in Edit Students ended 29 px from the border. Now at 105, ending with the swatches at 180. Opened. |
| FormQueryEdit | fixed (pass 1) | Pass 1: left column at 13 vs R 674. Pass 2: left column at the padding, Format group on the right edge. |
| FormQueryMonitor | fixed (fix round) | Pass 1: button column starts y77 and ends 1044 (R 1050), 25 px gutter. Pass 2: the button column from the top on the right edge, Copy (bottom-anchored) on the bottom padding; the split panels' content draws 16 px inside the split container, as on main. Final: main's 1036 x 512, the split panels inside their container again. Fix round (review 16): Command 5 px lower than Start, Stop and Elapsed; Elapsed 111 wide. Command at 20, 17 tall; Elapsed 123. Opened: one caption row, three boxes one width. |
| FormQueryParser | fixed (pass 1, final) | Pass 1: groups at 12 vs R; (code Height 325/720: keep top group within 325). Pass 2: split panels from the padding to the right edge. Final: main's 817 x 681 (the code's 720 px window height shows OK), the panels inside their container. |
| FormReactivationSetup | fixed (pass 1) | Pass 1: margins 16; groups 6 apart, heights 78/82. Pass 2: grid from the padding, the two groups one height 8 px apart, the combos 8 px before them. |
| FormRedundantIndexes | fixed (pass 1) | Pass 1: margins 12 (R 1217 vs 1222). Pass 2: content from the padding, the check box on the right edge. |
| FormReminderRules | fixed (pass 1) | Pass 1: margins 12. Pass 2: grid from the padding to the right edge, Add centred 8 px over the bottom. |
| FormRequiredFields | fixed (pass 1, 2) | Pass 1: script - three columns on one top: captions row, lists from one y; margins. Pass 2: the Field Type combo 120 px over the 158 px list of its column: the list's width; redrawn in pass 3. |
| FormRpCapitation | ok | Pass 1: nothing found. |
| FormRpPHRawPop | fixed (pass 1) | Pass 1: script - calendars at x35 y58 (empty band), list column; calendar report helper. Pass 2: calendars at their drawn size from the padding with TO between, the list column 16 px after. |
| FormRpUdsPlusSetup | fixed (pass 1) | Pass 1: margins 12. Pass 2: instructions and grid from the padding to the right edge, 8 px over the buttons. |
| FormRpUnfinalizedInsPay | ok | Pass 1: nothing found. |
| FormRxNorms | fixed (pass 1) | Pass 1: margins 26, grid ends 667 vs OK 685, 36 px under grid, filter row from x66. Pass 2: search row from the padding, grid to the right edge 8 px over the bottom row. |
| FormSearchPatNotes | fixed (pass 1) | Pass 1: box ends 271 vs OK 278. Pass 2: the box on OK's right edge. |
| FormSheetDefs | fixed (pass 1) | Pass 1: right edge 905 vs R 911. Pass 2: grids one width 8 px apart to the right edge, Tools and Copy at their grids' x. |
| FormSheetOutputFormat | fixed (pass 1) | Pass 1: script - Paper copies box at x30, checks/boxes at 78: one column at 8, boxes under the check text. Pass 2: copies box at the padding, each check box on one line with its box under its text to the right edge. |
| FormSheetProcSelect | fixed (pass 1) | Pass 1: margins 12. Pass 2: caption and grid from the padding, 8 px over OK. |
| FormShutdown | fixed (pass 1) | Pass 1: margins 12, buttons end 637/638 vs R 640. Pass 2: buttons one width on the right edge, grid 8 px before them to the bottom padding. |
| FormSpellCheck | fixed (pass 1, 2) | Pass 1: grid at 12, gutters 7/12, field ends 521 vs R 527. Pass 2: Add (the arrow into the list) 7 px under the Custom Word box it takes the word from: centred on the box's row; redrawn in pass 3. |
| FormSupplies | fixed (fix round) | Pass 1: script - planner parted Up (under grid) from Down (bottom bar); note under group; restore bottom block: Up over Down, group, Print + note on OK row. Pass 2: Up over Down, the order group beside Up, Print with its note on the OK row, grid 8 px over them. Fix round (review 5): Add Supply 93 x 23, crammed, its icon over the border. Now 110 x 24 on the grid's right edge. Opened at 100% and 150%. |
| FormSupplyCount | fixed (pass 1) | Pass 1: margins 12; Reason row floats mid (368..606) -> to R; labelCount runtime text. Pass 2: scan row from the padding, grid and count line to the right edge, Reason on the right edge. |
| FormSupplyInventory | fixed (pass 1) | Pass 1: top band 24, margins 12, Stock Count row 31 pitch vs 28. Pass 2: four rows on one pitch from the padding under the menu, notes 6 px after, grid to Add and Print. |
| FormSupplyProcCodes | fixed (pass 1) | Pass 1: margins 12, gutter 7. Pass 2: grids 8 px apart to the right edge, Add Supply at the materials grid's x. |
| FormSupplyReceive | fixed (pass 1) | Pass 1: margins 12, grid ends 747 vs Close 751. Pass 2: scan row from the padding, grid to the right edge 8 px over the bottom row. |
| FormSupplyStock | fixed (pass 1) | Pass 1: script - planner widened window 1084->1636 with an empty middle; back to main's arrangement on the grid. Pass 2: main's 1084 x 472 with the grids 8 px apart from the padding to the right edge, the bottom row under them. Final: the supply line drawn with its run-time text. |
| FormTaskInboxSetup | fixed (pass 1, 2) | Pass 1: script - grid drawn over the instructions (main too): note as tall as its lines, grid/Set/list under it. Pass 2: the instructions 6 px short of their seven lines (GridRulesTests): 8 px taller, the grids under them move; redrawn in pass 3. |
| FormTerminalManager | fixed (fix round) | Pass 1: margins 18, group ends 1032 vs R 1037, 39 px band at bottom. Pass 2: note and grid from the padding, the patient group on the right edge at the grid's top, the password group on the bottom padding. Fix round (review 7): the forms list and Add or Remove Forms 8 px in and 14 px short. Now 164 wide like the captions. Opened. |
| FormTimeCardRuleEdit | fixed (pass 1) | Pass 1: script - check boxes one at 8 one at 190; group indented 29; one field column, checks a list at 8. Pass 2: one field column (190) with the Rate2 group's fields on it, the check boxes a list at the padding, the Employee list to the right edge. |
| FormTreatPlanAddProcs | fixed (pass 1) | Pass 1: margins 12; labelPlan runtime text. Pass 2: left column at the padding (Add ending on the quick grid's edge), the procedures grid 8 px after it to the right edge. Final: the plan line drawn with its run-time text. |
| FormTreatPlanCurEdit | fixed (pass 1) | Pass 1: margins 12, R 915 vs grid 911. Pass 2: grids one width, the arrows centred between, fields ending on the left grid's edge. |
| FormTxtMsgEdit | fixed (fix round) | Pass 1: nothing found. Fix round (reviews 2, 9): the fields at 25 under the group at 8; the phone and count boxes 14 and 10 px short of the right edge. Now everything from 8 to 369; the phone box at 229 with its caption over it, the patient box 6 px before it, the count box on Find's row. Opened. |
| FormUnschedListSetup | fixed (fix round) | Pass 1: label1 cut (+9 height). Pass 2: the note 10 px short of its three lines (GridRulesTests): taller, the group grows; redrawn in pass 3. Fix round (review 10): Days Past and Days Future ended 26 px short of the group's border. Now 8 px inside, captions moved with them. Opened. |
| FormWikiAllPages | fixed (pass 1) | Pass 1: grid at 12 (browser pane not drawn by the harness). Pass 2: grid from the padding, Add and [[ ]] on the right edge (the browser pane between is not drawn by the harness). |
| FormWikiCompare | ok | Pass 1: the planner refuses it (Load sizes and places the two boxes and their labels by the divider): as on main. |
| FormWikiSearch | fixed (pass 1) | Pass 1: search box, limit row and grid at 12, check boxes 73 px after Search, Restore 4 px short of the right edge. Pass 2: left column from the padding, the check boxes 6 px apart after Search, Restore on the right edge. |
| FormXChargeReconcile | fixed (pass 1) | Pass 1: the calendars draw at 190 x 176 and covered View Imported and Payments. Pass 2: calendars at their drawn size under Import, the five buttons one width on a 28 px pitch under them. |
| SheetEditMobileCtrl | fixed (pass 1) | Pass 1: the drag note cut at 240 px, the check box against it. Pass 2: the drag note whole, the check box 12 px after it, the buttons on the right edge. |
| UserControlSetupWizOperatory | fixed (pass 1, 2) | Pass 1: the red-items note drawn over the instructions (main too), the grid at 18. Pass 2: the red-items note 45 px tall for two lines, its text 14 px under the instructions' top: two lines tall on the instructions' band, the grid and the right column 8 px under it; redrawn in pass 3. |

## 150%

| Picture | Status | What was checked and found |
|---|---|---|
| FormAutomationEdit-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
| FormClaimPayTotal-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
| FormDatabaseMaintTemp-150 | fixed (fix round) | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. Fix round: Print 24 px, its border whole. |
| FormEhrLabResultEdit-150 | fixed (fix round) | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. Fix round: Delete and Save 24 px, 8 px under the group. |
| FormEmailTemplateEdit-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
| FormImagePickerDXC-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
| FormJournal-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
| FormMaintRecordEdit-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
| FormPrinterEdit-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
| FormReactivationSetup-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
| FormRequiredFields-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
| FormSupplies-150 | fixed (fix round) | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. Fix round: Add Supply 110 x 24, its border whole. |
| FormSupplyInventory-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
| FormTimeCardRuleEdit-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
| FormTreatPlanAddProcs-150 | ok | The final build at 150%: every text whole at the scaled font, the inputs one height per row, the columns and edges as at 100%. |
