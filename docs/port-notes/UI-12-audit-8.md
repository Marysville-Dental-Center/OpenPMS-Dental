# UI-12 slice 8: the audit of gallery G-UI3-ui12-8

Every after picture of `.git/od-tools/test-results/G-UI3-ui12-8` (the slice's 102 windows at 100%, the 15 densest at 150%, the two
earlier-slice fixes), opened and looked at as a designer would: cut or covered text, overlaps, margins, columns and edges, gaps, crowding
and empty bands, the control looks, titles, boxes on the left. Before = `main` at the branch point (`1dd1f3086`), drawn by the `before`
step in the same capture session as the after pictures (UI-12.md, "Slice 8 (batch 4)"). What stays as drawn, and why, is in the line (a
window never shrinks: the lead's decision on slice 6's question; a row the code shows or hides keeps its room; a field-only dialog keeps
main's height with the band over its bar when nothing in it can grow).

**Pass 1** (`work/a1-100`, the first `after` step: the planner and its templates alone): 80 windows with findings. Margins: most windows
kept main's 12 to 44 px left margin and right edges short of the bar. Window sizes the planner broke: grown for controls hidden in the
designer (FormAutomationConditionEdit, FormClaimReports, FormHL7DefFieldEdit, FormEhrMeasureEventEdit, FormFeeEdit, found in the final pass),
for padding around a browser or a frame panel (FormEhrEduBrowser, FormEhrSummaryCcdEdit, FormEmailEdit, FormEligibilityResponseDisplay,
FormConnectionLost, FormLoginFailed) and others by 4 to 82 px. Covered text: FormEhrHash's SHA-1 note over Generate Hash, FormLoginFailed's
error label over the User row. Rows: FormOrthoRxEdit's tooth example taken for a button caption, FormEhrSetup's button columns at two x,
FormRpProcCodes' sort group floating. Empty bands over the bars where a list can grow (19 windows). Seven windows went back to `main` and
out of the slice (UI-12.md: the imaging surfaces, the print previews, the browsers under a toolbar). Fixes: 73 layout scripts on the
planner's tree.
**Pass 2** (`work/after-100`, after the scripts): every changed picture opened (71; the 31 others pixel-identical to pass 1); nothing new
in the pictures. GridRulesTests listed the run-time captions cut (FormEhrAptObsEdit's Value, FormEhrPatListElementEdit's Compare string, the
two ortho windows' Teeth), FormMedicationEdit's Generic name box 4 px off its row and the frame panels' overlaps: fixed (the overlaps by
design, `grid-known/8.tsv`).
**Final pass** (`work/final-100`, `work/final-150`, the final build after the whole filter built): FormFeeEdit's size (checking every
window's client size against `main` for the docs) and, at 150%, FormLanguagesUsed's custom Add (its arrow over the border of a 23 px icon
button): fixed and drawn again in the final build; then every picture of the gallery: nothing found. The 102 pictures at 100% are
pixel-identical (SHA-256 of the pixels) to the ones opened in pass 1 or 2 except FormFeeEdit, FormMedicationEdit and FormLanguagesUsed,
opened again; the 15 at 150% and the two earlier-slice pictures were all opened.
**Fix round** (`ui/ui12-8-2`, the lead's review of 2026-11-23, 17 findings; `work/f2-100`, `work/f3-100`, `work/f3-150`): the 17 windows drawn again from the fix build (and FormInsHistSetup with rows, at 100% and 150%), each opened; the lines below say what changed. FormLanguagesUsed's note first went to 262 px, where GridRulesTests' measure wraps two of its lines: the list and group 10 px wider instead, redrawn. Nothing else found.

## 100%

| Picture | Status | What was checked and found |
|---|---|---|
| FormAbout | fixed (pass 1) | Pass 1: the connection group at x 60, the logo and buttons 11 px short of the right padding, the version label 10 px above the product name's centre, the copyright lines 20 / 38 / 20 px apart. Pass 2: group and lines on the padding, the four lines on one 20 px pitch ending on the button row, the version centred on the name. |
| FormAccountingAutoPayEdit | fixed (pass 1) | Pass 1: Delete at x 28, the list 2 px short of the combo's edge, a 75 px band over the bar. Pass 2: Delete on the padding, the list on the combo's edge down to Add / Remove 8 px over the bar. |
| FormAllergyDefEdit | ok | Pass 1: columns shared across the two groups (151), right edges one (427); a 42 px band over the bar: the window keeps main's height and nothing in it grows (field rows only). |
| FormAmountEdit | ok | Pass 1: one row and OK on the padding. |
| FormApptBreak | fixed (pass 1) | Pass 1: the three buttons at x 29, 143 wide, neither centred nor on the group's edges. Pass 2: as wide as the group, 24 px on a 4 px gap, ending on the bottom padding. |
| FormApptFieldDefs | fixed (pass 1) | Pass 1: the window grown from 383 to 396, the note at 15 and the list at 18, the button column 29 px short of the right padding. Pass 2: main's width, note and list on the padding, the buttons on the right padding. |
| FormAutoCode | fixed (pass 1) | Pass 1: the list at 26 with a 49 px band under Add and Delete. Pass 2: the list from padding to padding down to 8 px over the buttons. |
| FormAutomationConditionEdit | fixed (fix round) | Pass 1: the window grown from 748 to 881 for the warning hidden in the designer, the lists at 24 and 211. Pass 2: main's width, the lists from the padding 8 px apart, the text column and the warning 8 px after them. Fix round (finding 16): Field and Comparison centred over their lists, Text at its box, the captions 4 px over: all three BottomLeft at their boxes' x, 2 px over them. Opened. |
| FormBenefitFrequencyEdit | fixed (pass 1) | Pass 1: the note at 12, Patient Override at 76, the lists ending 12 px apart. Pass 2: note on the padding, one bottom for the lists, the check box on the Code Group list's column. |
| FormBillingTypeMerge | fixed (pass 1) | Pass 1: Change 10 px above its row and 22 px short of the content edge, Merge 14 px short of the groups' edge. Pass 2: Change on the row at the field height on the content edge, Merge on the groups' edge. |
| FormCalendar | ok | Pass 1: the calendar and OK on the padding. |
| FormCertificationEdit | fixed (pass 1) | Pass 1: a 30 px band between Hidden and the bar. Pass 2: the list grown so Hidden stands 8 px over the bar. |
| FormChartViewDateFilter | ok | Pass 1: the preset list and the date rows on one field column, Now buttons 6 px after the boxes; a band over OK (main's height, nothing grows). |
| FormClaimAttachPasteDXCItem | fixed (pass 1) | Pass 1: a 66 px band under the Image Type list. Pass 2: the list down to 8 px over the bar. |
| FormClaimCustomTrackingUpdate | fixed (pass 1) | Pass 1: the window grown from 379 to 389, the note box 9 px short of the right padding. Pass 2: main's height, the note box to the right padding and down to Save. |
| FormClaimReports | fixed (pass 1) | Pass 1: the window grown from 324 to 386 for the hidden Retrieving note, the combo at 12. Pass 2: main's width, combo and Retrieve from padding to padding, the note under them. |
| FormConnectionLost | fixed (pass 1) | Pass 1: the frame panel moved to 8, 8 and the window grown from 400 x 200 to 416 x 252. Pass 2: the frame at 0, 0 in main's size, the message and buttons inside its padding (grid-known/8.tsv: the frame's overlaps). |
| FormContactEdit | fixed (pass 1) | Pass 1: Delete at 42, the Notes box 45 px short of the right padding with a 64 px band under it. Pass 2: Delete on the padding, Notes to the right padding and down to the bar. |
| FormCounties | fixed (pass 1) | Pass 1: the window grown from 464 to 546, the list at 11, Add and Remove in the middle of the right side. Pass 2: main's width, the list on the padding to the bottom padding, Add over Remove on the bottom right padding. |
| FormCreditRecurringDateChoose | fixed (pass 1) | Pass 1: the month buttons at 17 and 14, the hidden month combo over Last Month, the no-dates note at 45. Pass 2: buttons and combo on the padding, rows on a 28 px pitch, lines 6 px after the buttons; the window as wide as the code's widest line ("Cannot make payment before start date: " + the start date's DateTime text). |
| FormDefEdit | fixed (pass 1) | Pass 1: the window grown from 423 to 462, content at x 12, the Value caption 5 px higher than Name and one line for texts up to 315 px. Pass 2: main's width, Name and Value 170 wide 8 px apart, captions bottom-aligned (Value two lines), the color column after them. |
| FormDefEditBlockout | fixed (pass 1) | Pass 1: the window grown from 493 to 501, content at 9, the Usage group 2 px under the Name box, Color 18 px under the Name caption. Pass 2: main's width, the group 8 px after the box on its top (the box as tall as the group), Color on the caption's and box's rows. |
| FormDefaultCCProcs | fixed (pass 1) | Pass 1: the window grown from 473 to 496, note at 12, list at 15, the button column 5 px short of Save's edge. Pass 2: main's height, note and list on the padding, the buttons on the right padding. |
| FormDiseaseDefMerge | fixed (fix round) | Pass 1: the explanation and the choices at x 12. Pass 2: from padding to padding. The choices' texts are set by the code (grid-runtime-texts/8.tsv: two lines each); the picture shows the empty circles the harness leaves. Fix round (finding 17): the gallery drew the two choices as bare circles (FitGallery filled run-time texts of labels only): it now fills radio buttons and check boxes with no designer text from grid-runtime-texts. Opened: both choices on two lines beside their circles, nothing cut or covered. |
| FormDisplayFieldCategories | fixed (pass 1) | Pass 1: caption and list at 23, 15 px short of the right padding. Pass 2: from padding to padding, the list down to OK. |
| FormDisplayFieldEdit | fixed (pass 1) | Pass 1: the window grown from 154 to 185, Internal Name 7 px wider than the Description boxes. Pass 2: main's height with Save beside the Column Width row, one right edge. |
| FormDrCeph | fixed (fix round) | Pass 1: the two lists centred on their captions over the field column, the rows on one column; a band over Save (main's height). Fix round (finding 4; a layout script now): a 72 px band over Save and Browse 38 px short of it: the lists take the spare height (28..195), the captions start where the lists start (BottomLeft, 2 px over), the three rows at the pitch with the section gap over Save, the path boxes and Browse on the right padding (573). Opened. |
| FormDrugManufacturerSetup | fixed (pass 1) | Pass 1: list and Add at 28 with a 71 px band. Pass 2: the list from padding to padding down to 8 px over Add. |
| FormDrugUnitSetup | fixed (pass 1) | Pass 1: list and Add at 28 with a 71 px band. Pass 2: the list from padding to padding down to 8 px over Add. |
| FormEcwDiag | fixed (pass 1) | Pass 1: the log at 12, the Run Check column 4 px short of the right padding. Pass 2: column on the right padding, the log from the padding to it and to the bottom padding. |
| FormEdgeExpressPayPage | fixed (pass 1) | Pass 1: the browser and its wait note at 12, the browser 18 px over Close. Pass 2: from padding to padding, down to 8 px over Close (the browser draws nothing in the harness: the picture is the same). |
| FormEhrAptObsEdit | fixed (fix round) | Pass 1: Delete at 5. Pass 2: on the padding. GridRulesTests (pass 2): the Value caption cut for the code's "SNOMEDCT Value" and "Facility Address": widened to the widest, its right edge kept. Fix round (finding 6): an 82 px band under Value Units: the spare height as equal 23 px gaps between the four rows and the bar. Opened. |
| FormEhrCarePlanEdit | ok | Pass 1: one field column, the Instructions box to the right padding; a 20 px band over the bar. |
| FormEhrEduBrowser | fixed (pass 1) | Pass 1: the window grown by 14 x 15 for padding around the browser, Print 83 px short of the right padding. Pass 2: main's size, the browser inside the padding, Print on the right padding. |
| FormEhrGrowthCharts | ok | Pass 1: no controls (the code sizes the window); the same picture as main. |
| FormEhrHash | fixed (pass 1) | Pass 1: "( uses SHA-1 )" drawn over Generate Hash, the message box 8 px short. Pass 2: the note 6 px after Generate Hash on the bar's row, the Hash row and the message box on the right padding. |
| FormEhrMeasureEventEdit | fixed (pass 1) | Pass 1: the window grown from 261 to 270 for the hidden tobacco rows. Pass 2: main's height, the More information box 51 px, the tobacco rows on the pitch under it, the bar 8 px under them. |
| FormEhrMedicalOrderRadEdit | fixed (pass 1) | Pass 1: the Instructions caption 5 px over its box. Pass 2: 2 px over it. |
| FormEhrPatListElementEdit | fixed (fix round) | Pass 1: the Restriction and Operand lists 75 px wide, items cut (Medication, GreaterThan). Pass 2: as wide as the Compare string box; GridRulesTests: the Compare string caption widened for "For display and sorting". Fix round (finding 9): Operand showed the top of Equals, Restriction cut Lab result: whole rows of UI.ListBox's 17 px line (Restriction 4 rows, 72; Operand all three, 55, no scroll bar), the rows under them at the row gap (the code's Lab value row kept under Order by). Opened: no cut row. |
| FormEhrSetup | fixed (pass 1) | Pass 1: the left column at 27, the buttons after the group at 202 against the group's 214, the left column 3 px higher. Pass 2: left column on the padding, the group 16 px after it, one button column inside and after the group, one 28 px pitch, Emergency Now on the first row. The band under the columns stays (a launcher without a bar keeps main's height). |
| FormEhrSummaryCcdEdit | fixed (pass 1) | Pass 1: grown by 14 x 15, the button row scattered (Print 181 px short of the right padding). Pass 2: main's size; Reconcile caption (hidden, shown by the code) and its three buttons from the padding, Show xml and Print on the right padding. |
| FormEligibilityResponseDisplay | fixed (pass 1) | Pass 1: grown from 814 to 823, Print Preview 13 px short, a 35 px band. Pass 2: main's width, Print Preview on the padding, the grid down to it. |
| FormEmailDigitalSignature | ok | Pass 1: one field column, the multi-line boxes on the pitch, Trust on the padding. |
| FormEmailEdit | fixed (pass 1) | Pass 1: grown by 14 x 13, the captions under their boxes. Pass 2: main's size, the captions over the two boxes (one width, 8 px apart), the Raw HTML box on Save's row (the captions' bottom anchors made Top: anchors.tsv). |
| FormEmployers | fixed (pass 1) | Pass 1: grown from 570 to 577, the list at 18, the button column 64 px under its top. Pass 2: main's height, the column on the right padding from the list's top (anchored Top|Right now), the list to it. |
| FormEncounterEdit | ok | Pass 1: one field column, the code buttons 6 px apart on the Code row; the planner moves the row 2 px each pass, pinned (grid-hand/8.tsv). |
| FormFamilyHealthEdit | ok | Pass 1: one field column; a 40 px band over the bar (main's height, field rows only). |
| FormFamilyMemberSelect | fixed (pass 1) | Pass 1: the list at 24 with a 63 px band. Pass 2: from padding to padding down to the bar. |
| FormFeeEdit | fixed (final pass) | Pass 1 (missed then; the final pass's size check): grown from 374 x 132 to 536 x 336 for three controls no code shows. Fixed: main's size, the two hidden check boxes under the Fee row, textBox1 at main's place under the bottom edge (grid-known/8.tsv). |
| FormGlidewellAcctEdit | fixed (fix round) | Pass 1: one field column; the gap under API Key holds the code's status line (grid-runtime-texts/8.tsv). Fix round (finding 10; a layout script now): the providers caption and list at 12..411: 8..416, the bar's edges. Opened. |
| FormGmailApiSetup | ok | Pass 1: note, Enabled on the field column, the two boxes to the right padding; the planner moves the boxes 2 px each pass, pinned. |
| FormGuardianEdit | fixed (pass 1) | Pass 1: grown from 372 to 444 for the Guardian note. Pass 2: as wide as that row needs (403; main drew the note past its edge). |
| FormHL7DefFieldEdit | fixed (pass 1) | Pass 1: grown from 678 to 747 for the hidden internal-definition note, Delete at 9. Pass 2: main's height, the note 8 px over Delete, the list down to the bar. |
| FormHouseCalls | fixed (fix round) | Pass 1: the radio button and the group at 42. Pass 2: on the padding. Fix round (finding 3): the group ended at 401 with its buttons 37 px in, an 87 px band over OK: the group 8..478, the buttons on its content edge (470), the spare height as two equal 47 px gaps (radio to group, group to OK). Opened. |
| FormIcd9s | fixed (pass 1) | Pass 1: grown from 690 to 721, the list at 20 and 85 px short. Pass 2: main's height, the search row and list on the padding to the right padding. |
| FormImageDrawColor | fixed (pass 1) | Pass 1: grown from 146 to 153, the note at 16, Transparent 40 px tall. Pass 2: main's height, note on the padding, Transparent one line centred on its row. |
| FormImageFloat | ok | Pass 1: no controls; the same picture as main. |
| FormInsBenefitNotes | fixed (pass 1) | Pass 1: grown from 660 to 686, the box 95 px short of the right padding. Pass 2: main's height, the box from padding to padding. |
| FormInsBlueBookRuleEdit | ok | Pass 1: one field column, the Limit Type caption over its list. |
| FormInsHistSetup | fixed (fix round) | Pass 1: the rows are made by the code, which sets the window's height from Save. Fix round (review 2026-11-23, finding 1): the default picture has no code group, so it showed no row; the window is drawn again with rows (FormInsHistSetup-rows below). The designer's heading now from the padding to the right padding, the captions over the code's two slots (140, 334), Save on the right padding. |
| FormInsHistSetup-rows | fixed (fix round) | New picture (gallery state `rows`: six code groups in the cache, Perio Scaling with four quadrant rows; AddCategoryRows run before the layout manager as the constructor does). Before the fix (review finding 1) the code placed 24 px rows with 23 px boxes 1 px apart, labels 1 px from their boxes, 22 px Edit / Remove at y - 1, Save 14 px from the right. Now UiGrid: labels 8..134 right-aligned, the slots at 140 and 334 (box, Edit, Remove 6 px apart, the second ending on the right padding 522), the pitch 27 under the captions' caption gap, Save on the right padding, the height Save's bottom + 8. Opened: one column, even gaps, nothing cut. |
| FormInsPayFix | fixed (fix round) | Pass 1: the note and Run on the padding. Fix round (finding 14; a layout script now): the note at 12..411 and Run ending at 424: the note 8..428, Run on the right and bottom padding. Opened. |
| FormInsPlanConvert_7_5_17 | ok | Pass 1: the note and the two radio buttons on the padding, OK on the padding. |
| FormInsSpanEdit | fixed (fix round) | Pass 1: the two captioned boxes and the bar on the padding. Fix round (finding 12; a layout script now): From at 13, To ending at 219: From 8..119, To 127..238, both boxes 111 wide. Opened. |
| FormJournalEntryEdit | fixed (pass 1) | Pass 1: grown from 320 to 342 (the reconcile caption and box under a 60 px Notes box), the Credit caption 2 px right of its box. Pass 2: main's height, Notes 46 px like Memo, the reconcile row 8 px over the bar, the captions over their boxes. |
| FormLabCaseProductEdit | ok | Pass 1: one field column; the product line under Notes holds the code's text. |
| FormLanguageAndRegion | fixed (pass 1) | Pass 1: the description at 36, the check box at 85. Pass 2: both on the padding. |
| FormLanguagesUsed | fixed (fix round) | Pass 1: the left column at 29-32, the right 8 px short, Delete / Up / Down 72 px short of the list's edge. Pass 2: columns on the padding, Add centred between the lists. Final pass (150%): the custom Add's arrow over its right border (23 px icon button): 24 px. Fix round (finding 2): the Custom Language group was the stock GroupBox (square frame, title cut into the line) with a 37 px right margin and its bottom 13 px over Save's row: now UI.GroupBox under the All Languages list at its width (8..296; the list 10 px wider so the note's three lines do not wrap), the text box and Add on its 8 px content edges, the note 4 px under them, the group's bottom on Save's. Opened: even margins, the note's three lines, no band. |
| FormLicense | fixed (pass 1) | Pass 1: the list at 10 ending 98 px over the bottom padding. Pass 2: list and text on the padding down to the bottom padding. |
| FormLoginFailed | fixed (pass 1) | Pass 1: the frame panel moved to 8, 8, the window grown from 400 x 150 and the error label over the User row. Pass 2: the frame at 0, 0 in main's size, User and Password 8 px apart from padding to padding, Login on the padding. |
| FormMaintFieldEdit | fixed (pass 1) | Pass 1: Minimum and Maximum at 30, their texts short of the label column, Minimum 2 px high. Pass 2: one x, the longer text ending 6 px before the fields, each centred on its row. |
| FormMedicationEdit | fixed (pass 1) | Pass 1: the Dependencies group at 43, 108 px short of the right padding. Pass 2: the group from padding to padding, its combos still on the field column. GridRulesTests: the Generic name box 4 px under its row: on it. |
| FormMessageReplacementTextEdit | fixed (fix round) | Pass 1: the box from padding to padding, the bar on the padding. Fix round (finding 11; a layout script now): the editor at 12..491, 14 px over the bar: 8..496, 8 px over the bar. Opened. |
| FormMessagingButSetup | fixed (pass 1) | Pass 1: the lists at 34 and 274 with 102 px empty on the right and a 59 px band over Up / Down. Pass 2: one width from padding to padding, Up / Down under the Buttons list on its edge. |
| FormMessagingSetup | fixed (pass 1) | Pass 1: the lists from 28 with a 36 px band, the button column 118 px under their top. Pass 2: lists from the padding down to the bottom padding, the buttons from the lists' top. |
| FormMountainside | fixed (fix round) | Pass 1: one field column shared with the group's; a band over Save (main's height). Fix round (finding 13; a layout script now): the group's box and the two boxes above ended 32 px short: all three end on the group's content edge (546). Opened. |
| FormOrthoHardwareEdit | fixed (pass 1) | Pass 1: the Hidden check box 4 px after the field column, its label 16 px tall. Pass 2: on the field column, the label as tall as the rows; GridRulesTests: Teeth widened for "Tooth Range". |
| FormOrthoHardwareSpecEdit | ok | Pass 1: one field column, Hidden on it; a band over the bar. |
| FormOrthoPat | ok | Pass 1: one field column, Use Default Fee 6 px after the Fee box. |
| FormOrthoRxEdit | fixed (fix round) | Pass 1: the Comments label (the code's tooth example) taken for the buttons' caption, Delete at 25. Pass 2: the example on the field column under Teeth, the buttons on its row ending on the Teeth box's edge; Teeth widened for "Tooth Range". Fix round (finding 5): a 107 px band under the rows: the spare height as equal 38 px gaps between the Description row, the Hardware Spec row, the Teeth row with its example and buttons, and the bar. Opened. |
| FormOrthoRxSelect | fixed (pass 1) | Pass 1: lists at 44 and 394, two widths, Select / Remove off-centre, 49 px band. Pass 2: one width from padding to padding, the buttons centred between and on the lists' middle. |
| FormOryxUserSettings | ok | Pass 1: two rows on one column, Save on the padding. |
| FormPatFieldCheckEdit | fixed (fix round) | Pass 1: the box and its caption (the field's name) on the padding. Fix round (finding 15; a layout script now): the field name 11 px from the box: at the check box's text lead (x 22, MiddleLeft on the box's row). Opened. |
| FormPatFieldCurrencyEdit | fixed (pass 1) | Pass 1: caption and box at 19. Pass 2: on the padding. |
| FormPatFieldDateEdit | fixed (pass 1) | Pass 1: grown from 272 to 276, caption and box at 19. Pass 2: main's width, on the padding. |
| FormPatFieldEdit | fixed (pass 1) | Pass 1: caption and box at 19, a 37 px band. Pass 2: from padding to padding, the box down to the bar. |
| FormPatFieldPickEdit | fixed (pass 1) | Pass 1: caption and list at 19. Pass 2: from padding to padding, the list down to the bar. |
| FormPayConnect2iFrame | ok | Pass 1: the browser alone (docked); the same picture as main. |
| FormPayPeriodEdit | fixed (pass 1) | Pass 1: Delete at 12, 4 px above Save. Pass 2: the bar on the padding. |
| FormPayPlanChargeSelection | fixed (pass 1) | Pass 1: the list at 12. Pass 2: from padding to padding down to the bar. |
| FormRpAccountingBalanceSheet | fixed (pass 1) | Pass 1: caption and calendar at 29. Pass 2: on the padding; the band over OK stays (main's height). |
| FormRpAccountingGenLedg | fixed (pass 1) | Pass 1: the calendars at 27 and 260, 6 px apart. Pass 2: from the padding 8 px apart. |
| FormRpAccountingProfitLoss | fixed (pass 1) | Pass 1: the calendars at 27 and 260, 6 px apart. Pass 2: from the padding 8 px apart. |
| FormRpBirthday | fixed (pass 1) | Pass 1: Date Range at 21, Postcards 23 px short of Report's edge. Pass 2: groups from the padding 8 px apart, Postcards to the right padding with its buttons on its content edge. |
| FormRpDentalSealantMeasure | ok | Pass 1: one row, OK on the padding. |
| FormRpDiscountPlan | ok | Pass 1: the three-line label beside its box, OK on the padding. |
| FormRpInsCo | ok | Pass 1: the two-line label beside its box, OK on the padding. |
| FormRpNewPatients | fixed (pass 1) | Pass 1: the left column at 41, Date Range 40 px short. Pass 2: left column on the padding, the group on the right padding with Today's Date on its date column. |
| FormRpPHRawScreen | fixed (fix round) | Pass 1: the From calendar at 34. Pass 2: on the padding, TO 8 px after its drawn 190 px, To 6 px after TO. Fix round (finding 7): the calendars in the top-left corner over a 93 px band with 136 px empty to the right: the row of calendars and TO centred in the content area across (73 / 74 px) and down (44 / 45 px), as main had it. Opened. |
| FormRpPrescriptions | fixed (pass 1) | Pass 1: the Patient / Drug panel 4 px short of the right padding. Pass 2: on it. |
| FormRpProcCodes | fixed (pass 1) | Pass 1: lists 6 px apart and 2 px off one top, the sort group floating at 217. Pass 2: lists from padding to padding 8 px apart, Show blank fees and the group 4 px under them. |
| FormRpReferralAnalysis | fixed (pass 1) | Pass 1: the left column at 41, Date Range 95 px short. Pass 2: left column on the padding, the group on the right padding. |
| FormRpReferrals | fixed (fix round) | Pass 1: tab control and SQL box at 14 (the box 128 px short). Pass 2: from padding to padding, the box down to OK. Fix round (finding 8): All and None 18 px short of the list, 25 px under them on the page: each (184 - 6) / 2 = 89 wide, None ending on the list's edge, their bottom on the page's padding, the list down to 4 px over them. Opened. |

## 150% (the 15 densest)

| Picture | Status | What was checked and found |
|---|---|---|
| FormMedicationEdit-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |
| FormJournalEntryEdit-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |
| FormEhrSetup-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |
| FormEncounterEdit-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |
| FormEhrMeasureEventEdit-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |
| FormAbout-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |
| FormRpReferralAnalysis-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |
| FormBillingTypeMerge-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |
| FormAllergyDefEdit-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |
| FormRpReferrals-150 | fixed (fix round) | Text, edges, gaps and the inputs' height at 150%: nothing found. Fix round: redrawn with All and None on the list's width: nothing found. |
| FormInsHistSetup-rows-150 | ok (fix round) | New picture: the code's rows at 150%: labels, boxes, Edit and Remove on one pitch, nothing cut. |
| FormLanguagesUsed-150 | fixed (fix round) | The custom Add's arrow painted over its right border (a 23 px icon button): 24 px; redrawn, nothing found. Fix round: redrawn with the UI.GroupBox group: even margins, the note's three lines, the arrow inside Add. |
| FormRpNewPatients-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |
| FormEmailDigitalSignature-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |
| FormEhrAptObsEdit-150 | fixed (fix round) | Text, edges, gaps and the inputs' height at 150%: nothing found. Fix round: redrawn with the equal gaps: nothing found. |
| FormDisplayFieldEdit-150 | ok | Text, edges, gaps and the inputs' height at 150%: nothing found. |

## Earlier-slice fixes (before = main)

| Picture | Status | What was checked and found |
|---|---|---|
| FormBlockoutCutCopyPaste | fixed (final pass) | The lead's decision (a window never shrinks): back to main's 290 x 383, both groups 274 wide at the 8 px margins, the # caption and box after Repeat, the spare height as three equal 21 px gaps. Opened: margins equal, no band at the bottom. |
| FormEhrLabResultEdit | fixed (final pass) | The lead's decision: back to main's 390 x 338, Delete and Save on the bottom padding, the spare height as three equal 19 px gaps between Date Time, the groups and the bar. Opened: no band. |
