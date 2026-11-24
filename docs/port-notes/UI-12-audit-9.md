# UI-12 slice 9: the audit of gallery G-UI3-ui12-9

Every after picture of `.git/od-tools/test-results/G-UI3-ui12-9` (the slice's 82 windows at 100%, the 12 densest at 150%, and the
two windows of earlier slices the lead named), opened and looked at as a designer would: cut or covered text, overlaps, margins,
columns and edges, gaps, crowding and empty bands, the control looks, titles, boxes on the left. Before = `main` at the branch point
(`6cf3e22d9`), drawn by the slice runner's `before` step in the same capture session as every after picture (UI-12.md, "Slice 9
(batch 4)"). A line names the pass that found something and what was done; the last pass finds nothing in any picture. What stays
as drawn, and why, is in the line (a window never shrinks below main; a row the code shows or hides keeps its room).

**The passes.**
- **Pass 1** (`work/a1`, the planner and its templates alone on 90 windows): 9 windows skipped after their pictures (the code lays
  out their preview, kiosk or browser surface, or one control is the window: UI-12.md), 35 scripts written on the planner's tree
  (`work/tree-planned.tsv`), FormReqFieldCondEdit's after the code check; each window laid out again from the branch point by `work/refit.sh`.
- **Pass 2** (`work/a2`, 35 pictures changed, the others byte-identical to pass 1's and already opened): GridRulesTests measured the
  runtime texts (6 widths corrected) and found two overlaps that are the window's own (grid-known/9.tsv); FormProcButtons' 60 px
  band under its buttons; FormSetupWizardProgress, FormSheetFieldAdd and FormTranslationCat (3 more scripts).
- **Pass 3** (`work/a3` and `work/a3-150`; 4 pictures changed, the two earlier-slice windows and the 12 at 150% opened):
  FormReport_GuarantorAllocationCheck's right column cut at 150%.
- **Pass 4** (`work/f` and `work/f-150`, the final build): the final build's 84 pictures at 100% and 12 at 150% compared with pass 3's: FormReport_GuarantorAllocationCheck at both scales opened (the column holds every text); 7 others differ from pass 3 by 150 to 1,900 pixels of text antialiasing and list icons, the layout the same (their trees identical), opened; the re-plan found FormProgramProperty and FormSupplyAdjust 1 to 2 px off the planner's fixed point (two scripts), redrawn in `work/f2` and opened. **This pass finds nothing.**

**At 100%** (`1-slice-windows`)

| Window | Finding and fix |
|---|---|
| ControlFamilyEcw | pass 1: the Fee Schedule label 35 px in (an 80 px box for a 76 px text): the label at the padding, the box 6 px after it (script) |
| ControlImageDisplay | ok (one docked panel, nothing to place) |
| FormComboPicker | ok (no designer control; the code sizes the drop-down) |
| FormCommandLinePassOff | ok (no designer control) |
| FormMsgBox | pass 1: the planner stacked OK over Cancel and grew the window 54 px: main's bar, the message to the bar, the window at main's 284 x 104 (script) |
| FormPayTerminalEdit | pass 1: the clinic picker's box 8 px short of the text boxes: on their right edge (script) |
| FormPayorTypeEdit | ok |
| FormPdmpClientCertificate | pass 1: the intro note a line taller than its two lines (14 px band, then 2 px to the clinic line): the blocks 4 and 8 px apart (script); the clinic line's runtime text recorded |
| FormPerioEdit | ok |
| FormPreferences | ok (the tree on the padding, the window 16 px taller for the bar's section gap; the pages keep FormPreferences' width) |
| FormPrepaymentEdit | ok |
| FormPrintPreview | ok (unchanged: a docked toolbar and preview) |
| FormProcButtons | pass 1: the category list 24 px above the buttons list and the Quick Buttons note drawn into it: two columns, one top and height, the buttons under them (script); pass 2: a 60 px band under the buttons: the lists down to them |
| FormProcLockTool | ok (the two dates and "through" on one row as on main) |
| FormProgramLinkOutputFile | ok (the planner's: the labels no longer cut by their boxes) |
| FormProgramProperty | pass 4: the re-plan moved the boxes 320 -> 322 px (the planner's second pass): at its fixed point (script); ok |
| FormProgressViewer | pass 1: OK and Cancel stacked under Start, 37 px over the messages' bottom: main's place on that bottom (script) |
| FormProvStudentEdit | ok |
| FormProvidersMultiPick | pass 1: the buttons 21 px left of OK, the grid at 12 and 36 px before them: OK's x, the grid 8 px before them (script) |
| FormQBAccountSelect | pass 1: the Income Account caption 11 px tall, cut by the box above (main too): captions 2 px over their boxes, OK on its own bar (script) |
| FormQuestionDefEdit | ok |
| FormQuickBooksOnlineAuthorization | ok (the note's three runtime texts fit its 44 px) |
| FormReactivationEdit | ok |
| FormRecordAudio | ok |
| FormRecurringChargeFailEmailTemplate | pass 1: the intro at 12 against the fields' 8 (script); the replacement tags' runtime text recorded (two lines in its 32 px) |
| FormReport_GuarantorAllocationCheck | pass 1: the report grid 2 px under the title band, 96 px of empty window under the summary: the panel to the bottom padding, the grids sharing it, the right column 8 px after the panel (script); pass 3 (at 150%): Print Preview, From and the dates cut (the window does not scale its positions; main too): the column to the right content edge |
| FormReport_ProviderIncomeReport | pass 1: the panel 8 px past the client area's right edge, the grid inset on one side: inside the padding (script) |
| FormReportsFiltered | pass 1: the list at 13 with 27 px under it: 8 px from every edge (script) |
| FormReqFieldCondEdit | pass 1: the relationship list above the Operator caption, the Condition Type column at 12: main's operator column order (script) |
| FormReqNeededEdit | ok |
| FormReqStudentEdit | ok |
| FormScheduleBlockEdit | pass 1: the Blockout Type list 3 px short of the Note box's bottom: on it (script) |
| FormScheduledProcessesEdit | ok |
| FormSchoolClassEdit | pass 1: Delete at 50: at the padding (script) |
| FormSchoolClasses | pass 1: the list at 16, 36 px over Add: from the padding, 12 px over Add (script) |
| FormSchoolCourseEdit | pass 1: Delete at 49: at the padding (script) |
| FormSetupWizardProgress | pass 2: Next and Skip 112 px before the right edge: on the right content edge (script) |
| FormSheetFieldAdd | pass 2: the note and Pat Image at 12: at the padding (script) |
| FormSheetFieldChart | pass 1: the group at 88, alone: at the padding to the right content edge (script) |
| FormSheetFieldEditPatImage | pass 1: the FieldValue group at 64 with 64 px of empty group after Change: at the padding, its fields on the outer column (script) |
| FormSheetFieldExam | pass 1: the lists 6 px apart, 83 px short of OK's edge: one width, 8 px apart, to OK's edge (script) |
| FormSheetFieldGrid | ok |
| FormSheetFieldLine | ok (Is Payment Option on the field column, box first) |
| FormSheetFieldPatImage | ok |
| FormSheetFieldRect | ok |
| FormSheetFieldSpecial | pass 1: the note (the tooth chart's runtime text, three lines) had two lines' room in 207 px: to Save's edge, as tall as its longest text, the rows under it (script) |
| FormSheetImportEnumPicker | pass 1: the list 98 px before Clear Val, Clear Val 2 px under the list's top: the list to 8 px before the button column, Clear Val on its top (script) |
| FormSheetTools | ok |
| FormSpellChecker | ok (the box 8 px under the menu) |
| FormSplash | ok (unchanged) |
| FormStateAbbrEdit | pass 1: Medicaid ID Length on two lines (GridRulesTests: a row 5 px off the pitch): one line, the column as wide (script) |
| FormSubscriberMove | pass 1: the plan boxes 85 px into their groups: from the groups' padding (script) |
| FormSubscriberSelect | pass 1: the note and the list at 32, More Patients 35 px left of OK: OK's column, the list 8 px before it and 12 px over OK (script) |
| FormSupplierEdit | ok |
| FormSupplyAdjust | pass 4: the re-plan moved the Change box 72 -> 73 px and its note 1 px (the planner's second pass): at its fixed point (script); ok |
| FormTaskAttachmentEdit | pass 1: the text box at 12, 36 px short of Save's edge: to it (script) |
| FormTaskListBlocks | pass 1: the tree at 12, 5 px short of Save's edge: from the padding to it (script) |
| FormTaskListEdit | ok |
| FormTaskNoteEdit | pass 1: the note box 35 px short of Save's edge: on it (script) |
| FormTelephone | ok |
| FormTerminalConnection | pass 1: the boxes at 25, 160 px short of the right edge: from the padding to the right content edge, Progress to the bottom (script) |
| FormTimePick | pass 1: AM after the minute box and PM under it, 62 px apart: AM and PM a list after the minute box (script) |
| FormTranslationCat | pass 2: the list at 28 and 31 px over the bottom: from the padding to 8 px before the right column, to the bottom padding (script) |
| FormTreatmentPlanDiscount | ok |
| FormUpdateInProgress | ok |
| FormUserPassword | pass 1: the runtime row names (New Password, Re-Enter Password) recorded; the label column as wide (script) |
| FormUspsAddressSetup | pass 1: Enabled on the field column over a note at the padding, the secret status (runtime, two lines) 30 px tall: Enabled at the padding, the status as tall as its text (script) |
| FormVaccineDefEdit | ok |
| FormVaccineDefSetup | pass 1: the list and Add at 28, 63 px apart: from the padding, 12 px over Add (script) |
| FormVaccineObsEdit | ok |
| FormVideo | pass 1: Settings 34 px before the picture's edge, Capture at 3: Settings on the edge, Capture at the padding (script) |
| FormWarnToCloseComputers | pass 1: the message and panel at 54, the question row 85 px over the bottom: from the padding, the question and its buttons on the bottom bar (script) |
| FormWebBrowserPrefs | ok (the code sizes the window; nothing placed) |
| FormWikiFileFolder | pass 1: the Path label 42 px in, the box ending 173 px before the edge: label at the padding, box and browse button to the right content edge (script) |
| FormWikiLists | ok |
| SetupWizControl | ok (the title and the description 4 px apart) |
| ToolTipOD | ok (the code sizes it) |
| UserControlDashboard | ok (nothing placed) |
| UserControlDashboardWidget | ok (nothing placed; the code sizes it) |
| UserControlEmailTemplate | ok |
| UserControlSetupWizComplete | ok |
| UserControlSetupWizIntro | ok |

**At 150%** (`2-at-150`)

| Window | Finding and fix |
|---|---|
| FormVaccineObsEdit | ok (text fits, columns and edges as at 100%) |
| FormReqStudentEdit | ok (text fits, columns and edges as at 100%) |
| FormProvStudentEdit | ok (text fits, columns and edges as at 100%) |
| FormSheetFieldEditPatImage | ok (text fits, columns and edges as at 100%) |
| FormSheetFieldGrid | ok (text fits, columns and edges as at 100%) |
| FormSupplierEdit | ok (text fits, columns and edges as at 100%) |
| FormSheetFieldSpecial | ok (text fits, columns and edges as at 100%) |
| FormReport_GuarantorAllocationCheck | pass 3: the right column cut at 150% (the window does not scale its positions; main too): to the right content edge; pass 4: ok |
| FormTaskListEdit | ok (text fits, columns and edges as at 100%) |
| FormPdmpClientCertificate | ok (text fits, columns and edges as at 100%) |
| FormReqFieldCondEdit | ok (text fits, columns and edges as at 100%) |
| FormSheetFieldLine | ok (text fits, columns and edges as at 100%) |

**Earlier slices** (`3-earlier-slice-fixes`; before = main's Button.cs and FormAdjMulti designer, built in this worktree)

| Window | Finding and fix |
|---|---|
| FormMaintBoard | the lead's item: Add Item drew only its icon (main); now its icon and text, as the other icon buttons (UI.Button) |
| FormAdjMulti | the lead's item: Update takes Add's anchor; the picture is the same (an anchor acts on a resize) |
