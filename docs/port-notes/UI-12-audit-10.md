# UI-12 slice 10: the audit of gallery G-UI3-ui12-10

Every after picture of `.git/od-tools/test-results/G-UI3-ui12-10` (the slice's 86 windows at 100% and the 14 densest at 150%), opened and
looked at as a designer would: cut or covered text, overlaps, margins, columns and edges, gaps, crowding and empty bands, the control
looks, titles, boxes on the left. Before = `main` at the branch point (`9817dee17`), drawn by the slice runner's `before` step (the 150%
pictures redrawn with the same build after the disk filled up) in the same capture session as every after picture (UI-12.md, "Slice 10
(batch 2 and 6)"). A line names the pass that found something and what was done; the last pass finds nothing in any picture. What stays as
drawn, and why, is in the line (a window never shrinks below main; a row the code shows or hides keeps its room; owner-drawn surfaces keep
their place).

**The passes.**
- **Pass 1** (`work/planned`, the planner and its templates alone, 878 plan rows, 17 unplaced rows in 7 windows): 81 of the 89 windows of
  the batches with findings; 82 layout scripts written on the planner's tree (`work/tree-planned.tsv`, helper `hand/_s10.py`). Three
  windows left out after a look (PrintPanel and DashApptGrid: the code sizes or places their only surface; FormInstallAllocator_Provider:
  no LayoutManager, its own colours and fonts, the plain windows' slice).
- **Pass 2** (`work/a2`, the first build with the scripts; 82 pictures changed, opened): GridRulesTests' findings (the clinic box off its
  column, a note cut on one line, the parts of library controls that overlap as on main: grid-known rows) and two looks (FormWikiHistory's
  lock note, the setup wizard's instructions), fixed by the scripts.
- **Pass 3** (`work/a3` and `work/a3-150`; 10 pictures changed against pass 2, opened; the 14 at 150% opened): GridRulesTests' last
  finding, FormPayPlanChargeEdit's clinic box 8 px left of the column (the picker draws its own 43 px label area first), fixed by its
  script.
- **Final pass** (`work/f` and `work/f-150`, the whole filter built, `OpenPmsDental.dll` `D281CDF7...` in both bins, GridRulesTests and
  the layout snapshot green): 1 picture changed against pass 3 at 100% and 1 at 150% (FormPayPlanChargeEdit), opened; the others
  byte-identical to pictures opened in passes 2 and 3. Nothing.

## The 86 windows at 100%

- **FormPatientListDiscount**: pass 1: grid x22 -> 8..R, bottom 8 (31 px empty band); fixed by its script (`hand/FormPatientListDiscount.py`). Final pass: nothing.
- **FormPatListEHR2014**: pass 1: grid 16 before the button column -> 8; fixed by its script (`hand/FormPatListEHR2014.py`). Final pass: nothing.
- **FormPatListResultsEHR2014**: pass 1: main's 886x370; Print on R, grid 8 before it, bottom 8; fixed by its script (`hand/FormPatListResultsEHR2014.py`). Final pass: nothing.
- **FormPayPlanChargeEdit**: pass 1: the clinic picker's box ended 4 px short of Provider's right edge (307 vs 311): on it; fixed by its script (`hand/FormPayPlanChargeEdit.py`). Pass 2: GridRulesTests: the clinic picker's box 4 px off the field column (the picker draws its own label first): the box on the column, ending on Provider's edge; fixed by its script. Pass 3: GridRulesTests: the clinic box still off the column (the picker's label area is 43 px): fixed by its script. Final pass: nothing.
- **FormPayPlanCredits**: pass 1: main's 937x510; Print/grid x12 -> 8, group on R, grid 8 before it; Total row on the Save row ending on the grid's right, grid 12 over it; fixed by its script (`hand/FormPayPlanCredits.py`). Final pass: nothing.
- **FormPayPlanSelect**: pass 1: grid x12 -> 8, check to R, hidden note x8; fixed by its script (`hand/FormPayPlanSelect.py`). Final pass: nothing.
- **FormPayPlanTemplates**: pass 1: picker/grid x26 -> 8, grid to R (38 short) and 12 over OK (48); hidden Add on R; fixed by its script (`hand/FormPayPlanTemplates.py`). Final pass: nothing.
- **FormPerioGraphical**: pass 1: planner grew the window 768x696 -> 772x712 ("Do NOT change the size of this form"): main's size; buttons a column at 672 from 8, 4 apart, one width 88; fixed by its script (`hand/FormPerioGraphical.py`). Final pass: nothing.
- **FormPerioGraphicalSetup**: pass 1: the group's right column ended 67 px short of its content edge: the right column's buttons on it (labels with them); fixed by its script (`hand/FormPerioGraphicalSetup.py`). Final pass: nothing.
- **FormPharmacies**: pass 1: grid x12 -> 8..R; fixed by its script (`hand/FormPharmacies.py`). Final pass: nothing.
- **FormPlansForFamily**: pass 1: note/grid x12 -> 8..R, grid bottom 8 (27); fixed by its script (`hand/FormPlansForFamily.py`). Final pass: nothing.
- **FormPopupsForFam**: pass 1: grid 24..654 -> 8..R, 12 over Add; Show Deleted (Bottom|Right) on R, centred on Add's row; fixed by its script (`hand/FormPopupsForFam.py`). Final pass: nothing.
- **FormPreAuthReminder**: pass 1: note/grid x12 -> 8..R, 12 over the bar; fixed by its script (`hand/FormPreAuthReminder.py`). Final pass: nothing.
- **FormPrinterSetup**: pass 1: main's 941x404; note/grid x12 -> 8..R, 12 over the bar; fixed by its script (`hand/FormPrinterSetup.py`). Final pass: nothing.
- **FormPrintTrojan**: pass 1: printPanel 35 vs vScroll 33 (code: vScroll at Width-29, 33): code uses printPanel.Top; both 623 high to 8 over the bottom; main's ClientSize 686 (code sets Width); fixed by its script (`hand/FormPrintTrojan.py`). Final pass: nothing.
- **FormProcApptColors**: pass 1: note x18, grid x16..301 -> 8..R; grid 12 over Add; fixed by its script (`hand/FormProcApptColors.py`). Final pass: nothing.
- **FormProcBandingSelect**: pass 1: grid to R, 12 over OK; fixed by its script (`hand/FormProcBandingSelect.py`). Final pass: nothing.
- **FormProcCodeEditMore**: pass 1: grid 12..548 -> 8..R, bottom 8; clinic picker to R; fixed by its script (`hand/FormProcCodeEditMore.py`). Final pass: nothing.
- **FormProcCodeNoteEdit**: pass 1: planner grew 646 -> 685 high (bar under the Minutes row): main's 646; time bar and list from 44 (labels' bottom 40-42, Provider/Note 2 over their boxes), Minutes row on the bottom bar's row, note box to R and down to 8 over the bar; fixed by its script (`hand/FormProcCodeNoteEdit.py`). Pass 2: GridRulesTests: the slider on the time bar and the bar's hidden head panel (the bar's own parts, as on main): grid-known rows; kept, with the reason. Final pass: nothing.
- **FormProcNoteAppend**: pass 1: note boxes 4 short of R: to R; fixed by its script (`hand/FormProcNoteAppend.py`). Pass 2: GridRulesTests: SignatureBoxWrapper's buttons over its box (the wrapper's own parts): grid-known rows; kept, with the reason. Final pass: nothing.
- **FormProcTools**: pass 1: planner grew the window 698 -> 1279 (check box texts on one line, 600 px empty band under them): main's 698, the check boxes wrap to their measured lines (list pitch), the two rules 14 apart; fixed by its script (`hand/FormProcTools.py`). Final pass: nothing.
- **FormProdSelect**: pass 1: grid x12 -> 8..R, 12 over OK; fixed by its script (`hand/FormProdSelect.py`). Final pass: nothing.
- **FormProgramLinkHideClinics**: pass 1: content x12 -> 8, right list to R, lists 12 over Save, arrows centred between the lists; fixed by its script (`hand/FormProgramLinkHideClinics.py`). Final pass: nothing.
- **FormProvAdditional**: pass 1: grid x12 -> 8..R, 8 over the merchant note; fixed by its script (`hand/FormProvAdditional.py`). Final pass: nothing.
- **FormQueryFavorites**: pass 1: planner moved the split container's panels (8,8 / 473,8) and grew the window 1084x656 -> 1129x684 (code sets Width 500/1100): main's size, panels at 0,0 / 455,0, panel content 8..446 and 8..R2, bottoms on one line; fixed by its script (`hand/FormQueryFavorites.py`). Final pass: nothing.
- **FormQuestionDefs**: pass 1: grid x15 -> 8, bottom 8; Add/Up/Down column on R ending on the grid's bottom; grid to 8 before it; note x8; fixed by its script (`hand/FormQuestionDefs.py`). Final pass: nothing.
- **FormRadOrderList**: pass 1: note/grid x12 -> 8, Approve group on R, grid to 8 before it, grid bottom 8; fixed by its script (`hand/FormRadOrderList.py`). Final pass: nothing.
- **FormRecallsPat**: pass 1: grid x12 -> 8..R, 12 over the row (29); Set Perio 6 after Add (263); fixed by its script (`hand/FormRecallsPat.py`). Final pass: nothing.
- **FormRecallTypes**: pass 1: the planner put the check box over Add and Sync (no fixed point): main's rows (Add, Sync with its note after it, the check box under them), grid 8..R and 12 over the row; fixed by its script (`hand/FormRecallTypes.py`). Final pass: nothing.
- **FormReconciles**: pass 1: grid x18 -> 8..R, bottom 12 over Add; fixed by its script (`hand/FormReconciles.py`). Final pass: nothing.
- **FormRegistrationKey**: pass 1: caption/list x12 -> 8, the box 8 after the list to R, both from one top, 12 over OK; fixed by its script (`hand/FormRegistrationKey.py`). Final pass: nothing.
- **FormRepeatChargeEditMulti**: pass 1: rows 4-6 labels 4-5 px over their fields (the two-line notes): one pitch per row (field, label and note on one top), fields one width, notes at one x to R; fixed by its script (`hand/FormRepeatChargeEditMulti.py`). Pass 2: GridRulesTests: the Current Charge note cut (needs 7 px more on one line): every note as tall as its measured lines at its width; fixed by its script. Final pass: nothing.
- **FormRepeatChargesUpdate**: pass 1: the description (with the code's aging line) cut at the bottom: 196 high, the bar 12 under it, window 243 -> 250; x8 to R; fixed by its script (`hand/FormRepeatChargesUpdate.py`). Final pass: nothing.
- **FormReplicationEdit**: pass 1: the two check boxes' notes at 353 and 420: one x (420, after the wider box's text) to R; fixed by its script (`hand/FormReplicationEdit.py`). Final pass: nothing.
- **FormReqNeededs**: pass 1: grid x16 -> 8 to 8 before the groups, 12 over the bar; Copy 26 high like Add (the two groups one height); fixed by its script (`hand/FormReqNeededs.py`). Final pass: nothing.
- **FormReqStudentOne**: pass 1: grid margins 12/16 -> 8; fixed by its script (`hand/FormReqStudentOne.py`). Final pass: nothing.
- **FormReqStudentsMany**: pass 1: grid x15 -> 8, bottom 8; Class/Course combos (and labels) to R; fixed by its script (`hand/FormReqStudentsMany.py`). Final pass: nothing.
- **FormRxAlertEdit**: pass 1: the fields outside the group end 14 px short of the group's (630 vs 644): group to R, every field on its content edge (681); fixed by its script (`hand/FormRxAlertEdit.py`). Final pass: nothing.
- **FormRxCombine**: pass 1: grid to R (6 short), 12 over OK (27); fixed by its script (`hand/FormRxCombine.py`). Final pass: nothing.
- **FormRxManage**: pass 1: grid 22 margins -> 8..R, 12 over the buttons; New Rx at 8 (82), Print Selected 6 after it (219); fixed by its script (`hand/FormRxManage.py`). Final pass: nothing.
- **FormRxSelect**: pass 1: the instructions (470 at 8) ran under Blank (435; main too): Blank 6 after the text, the note centred on the row; grid x12 -> 8, Search group on R, grid to 8 before it and 12 over the row; fixed by its script (`hand/FormRxSelect.py`). Final pass: nothing.
- **FormRxSetup**: pass 1: check/grid x12 -> 8, grid to R, 12 over the bar; fixed by its script (`hand/FormRxSetup.py`). Final pass: nothing.
- **FormScheduleEdit**: pass 1: operatory list to R (12 short); hidden clinic picker's box on the field column (89 -> 97); fixed by its script (`hand/FormScheduleEdit.py`). Final pass: nothing.
- **FormSchoolCourses**: pass 1: grid 12..437 -> 8..R, 12 over the bar (31); fixed by its script (`hand/FormSchoolCourses.py`). Final pass: nothing.
- **FormSecurityCentralUserEdit**: pass 1: Edit on R, grid 8..8 before it (6), bottom 8; fixed by its script (`hand/FormSecurityCentralUserEdit.py`). Final pass: nothing.
- **FormSetupWizard**: pass 1: grid to R (15), 12 over Set Up All (22); fixed by its script (`hand/FormSetupWizard.py`). Final pass: nothing.
- **FormSheetExport**: pass 1: note/grid x12 -> 8; Export at R, top = grid top; grid to 8 before Export; grid bottom 8; fixed by its script (`hand/FormSheetExport.py`). Final pass: nothing.
- **FormSheetImport**: pass 1: grid x27 -> 8, pictures column on R (822), grid to 8 before it; grid bottom = pictures' bottom (668); fixed by its script (`hand/FormSheetImport.py`). Final pass: nothing.
- **FormSigElementDefEdit**: pass 1: ok (the Sound group's buttons on the field column); kept. Final pass: nothing.
- **FormSnomeds**: pass 1: grid x20 -> 8, r 848 (hidden group at 856); label1 x8, textCode 146..592; fixed by its script (`hand/FormSnomeds.py`). Final pass: nothing.
- **FormSshHostKeys**: pass 1: note/grid x12 -> 8..R; Remove on R (25 short), Accept and the key boxes with it; the three rows 12 over the bar (61 px empty band), grid 8 over them; fixed by its script (`hand/FormSshHostKeys.py`). Final pass: nothing.
- **FormStateAbbrs**: pass 1: grid 16..320 -> 8..R, 12 over the bar; fixed by its script (`hand/FormStateAbbrs.py`). Final pass: nothing.
- **FormStatementChoices**: pass 1: note/grid x12 -> 8..R, 12 over OK; fixed by its script (`hand/FormStatementChoices.py`). Final pass: nothing.
- **FormSubscribersList**: pass 1: grid 23/22/28 margins -> 8; fixed by its script (`hand/FormSubscribersList.py`). Final pass: nothing.
- **FormSuppliers**: pass 1: grid 12 margins -> 8..R, 12 over Add (34); fixed by its script (`hand/FormSuppliers.py`). Final pass: nothing.
- **FormSupplyOrderEdit**: pass 1: nothing (the planner's layout kept). Final pass: nothing.
- **FormSupplyOrderItemEdit**: pass 1: nothing (the planner's layout kept). Final pass: nothing.
- **FormSupplyProcPendings**: pass 1: main's 760x482; note/grid x12 -> 8..R, 12 over the bar; fixed by its script (`hand/FormSupplyProcPendings.py`). Final pass: nothing.
- **FormTaskAttachments**: pass 1: Add/grid x12 -> 8..R, bottom 8; fixed by its script (`hand/FormTaskAttachments.py`). Final pass: nothing.
- **FormTaskHist**: pass 1: nothing (the planner's layout kept). Final pass: nothing.
- **FormTaskListSelect**: pass 1: grid ends 104 px short of R: to R; the hidden multi check on the OK row, grid 12 over it; fixed by its script (`hand/FormTaskListSelect.py`). Final pass: nothing.
- **FormTasksForAppt**: pass 1: note/grid at 12 with 13/14 margins -> 8; fixed by its script (`hand/FormTasksForAppt.py`). Final pass: nothing.
- **FormTaskSubscribers**: pass 1: grid right/bottom 14 -> 8; fixed by its script (`hand/FormTaskSubscribers.py`). Final pass: nothing.
- **FormTaxAddress**: pass 1: note x12 -> 8; fixed by its script (`hand/FormTaxAddress.py`). Final pass: nothing.
- **FormTrackNextSetup**: pass 1: group at x29 (29 left / 51 right): x8, width to R; fixed by its script (`hand/FormTrackNextSetup.py`). Final pass: nothing.
- **FormTranslation**: pass 1: buttons stacked on one x (overlap); one row: Delete Selected, Delete Unused, note centred; grid 8..R down to 12 over the row; fixed by its script (`hand/FormTranslation.py`). Final pass: nothing.
- **FormTranslationEdit**: pass 1: the four boxes end 49 px short of R: to R; fixed by its script (`hand/FormTranslationEdit.py`). Final pass: nothing.
- **FormUcums**: pass 1: as Snomeds: label 8, grid 8..R(777), bottom 12 over OK; fixed by its script (`hand/FormUcums.py`). Final pass: nothing.
- **FormUnschedListPatient**: pass 1: grid x12 -> 8..R; fixed by its script (`hand/FormUnschedListPatient.py`). Final pass: nothing.
- **FormUpdateHistory**: pass 1: note, grid x14 -> 8..R, grid bottom to 8 from bottom; fixed by its script (`hand/FormUpdateHistory.py`). Final pass: nothing.
- **FormUserPrefAdditional**: pass 1: grid x12 -> 8..R; fixed by its script (`hand/FormUserPrefAdditional.py`). Final pass: nothing.
- **FormWaitingRoom**: pass 1: nothing (the planner's layout kept). Final pass: nothing.
- **FormWikiDrafts**: pass 1: code's working area literal (294,12,W-397,H-24) -> from the numbers box (its right 18 px covered, as drawn) to 8 before the button column, PAD top/bottom (UiGrid); designer: grid 8,8, numbers 262, boxes 8..630, buttons on R; fixed by its script (`hand/FormWikiDrafts.py`). Pass 2: GridRulesTests: the text box over the numbers' right 18 px (as on main, the code's layout): a grid-known row; kept, with the reason. Final pass: nothing.
- **FormWikiHistory**: pass 1: code put textContent at gridMain.Width+6 (assumes grid at 0: over the grid by 6, main too) -> gridMain.Right+ITEM_GAP, both boxes to R-PAD (UiGrid); designer: buttons row 8, grid 8,40, boxes 262..; fixed by its script (`hand/FormWikiHistory.py`). Pass 2: lock note's text on the buttons' row top-aligned: MiddleLeft; fixed by its script. Final pass: nothing.
- **FormWikiIncomingLinks**: pass 1: grid x12 -> 8, bottom 8; web view 6 after the grid to R (was 51 short); fixed by its script (`hand/FormWikiIncomingLinks.py`). Final pass: nothing.
- **FormWikiListEdit**: pass 1: search row from 8 (label sized for "Advanced Search", runtime), grid x12 -> 8 to 8 before the groups, 12 over Delete; fixed by its script (`hand/FormWikiListEdit.py`). Final pass: nothing.
- **FormWikiListItemEdit**: pass 1: grid x12 -> 8..R; fixed by its script (`hand/FormWikiListItemEdit.py`). Final pass: nothing.
- **FormXchargeTokenTool**: pass 1: Total Cards label 28 px over its box (three rows apart): the three count rows on the pitch up from the bottom, labels 6 before the boxes; grid to R, 12 over them; fixed by its script (`hand/FormXchargeTokenTool.py`). Final pass: nothing.
- **FormXConnectValidation**: pass 1: header/grid x12 -> 8..R, 12 over the bar; fixed by its script (`hand/FormXConnectValidation.py`). Final pass: nothing.
- **FormXWeb**: pass 1: right column at three x (246/263): one column on R (272..400, captions on it), left column 8..264, group and note at 8; fixed by its script (`hand/FormXWeb.py`). Final pass: nothing.
- **TextBoxWiki**: pass 1: nothing (the planner's layout kept). Final pass: nothing.
- **UserControlSetupWizDefinitions**: pass 1: list/caption x29 -> 8, grid 8 after the list to R and down to the list's bottom; Edit Items' buttons from the group's content edge (48); Guidelines as a caption over its box at 8 (label at 24, box at 128 on no column); fixed by its script (`hand/UserControlSetupWizDefinitions.py`). Final pass: nothing.
- **UserControlSetupWizEmployee**: pass 1: grid's top 20 under the notes' top: grid at 8 (8 after the notes column) down to 8 from the bottom, the right column from the grid's top on R; fixed by its script (`hand/UserControlSetupWizEmployee.py`). Final pass: nothing.
- **UserControlSetupWizFeatures**: pass 1: the planner packed the check boxes (pitch 24) and the info icons (pitch 22) apart: each icon beside its check box (one pitch), the panel 6 after the widest check box; the explanation label covered the group's title (on main too): inside the content box; group to R, the further-modifications note and Advanced under it on R; fixed by its script (`hand/UserControlSetupWizFeatures.py`). Pass 2: instructions right-aligned far from the boxes: MiddleLeft; fixed by its script. Final pass: nothing.
- **UserControlSetupWizFeeSched**: pass 1: grid x21 -> 8, bottom 8; buttons a column 8 after the grid from its top, their info icons 6 after them on their rows; fixed by its script (`hand/UserControlSetupWizFeeSched.py`). Final pass: nothing.
- **UserControlSetupWizProvider**: pass 1: planner grew it 530 -> 540 (the wizard sizes it): 530; note/grid x20 -> 8, the right column 129 wide on R (2 past it), Add on the grid's top, grid 8 before the column, bottom 8; fixed by its script (`hand/UserControlSetupWizProvider.py`). Final pass: nothing.

## The 14 densest at 150%

- **FormWikiListEdit** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormPayPlanCredits** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **UserControlSetupWizFeatures** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormSupplyOrderItemEdit** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormSigElementDefEdit** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormXWeb** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormSupplyOrderEdit** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormReplicationEdit** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormRepeatChargeEditMulti** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormPayPlanChargeEdit** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormReqNeededs** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormProcNoteAppend** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormProcCodeNoteEdit** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
- **FormPerioGraphicalSetup** (150%): final pass: nothing (no text cut or covered, the same edges as at 100%).
