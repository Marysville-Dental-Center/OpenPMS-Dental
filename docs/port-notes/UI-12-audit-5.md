# UI-12 slice 5: the audit of gallery G-UI3-ui12-5

Every after picture of `.git/od-tools/test-results/G-UI3-ui12-5` (the slice's 44 windows at 100% and FormRecallTypeEdit's Child Prophy
state, the 7 densest windows at 150%, and the 8 earlier windows whose bottom-anchored controls this slice fixed), opened and looked at as
a designer would: cut or covered text, overlaps, margins, columns and edges, gaps, crowding and empty bands, the control looks, titles,
boxes on the left. Before = `main` at the branch point (`ab84ef8b1`), drawn in the same session as the after pictures of the first pass
(UI-12.md, "Slice 5 (batch 3)"). A line names the pass that found something and what was done; the last pass finds nothing in any
picture. What stays as drawn, and why, is in the line (a window never shrinks below main; a row the code shows or hides keeps its room;
owner-drawn surfaces keep their place).

**Pass 1** (`a1`: the planner and its templates alone, from the branch point's tree with the anchors LayoutManager stores): 37 of the 48
windows with findings; four windows left the slice (below). **Pass 2** (`a2`: the 40 layout scripts): 6 windows with findings, and the
GridRulesTests run of that build listed 27 findings, 13 of them fixed in the windows and 16 known (`grid-known/5.tsv`, the windows' own
arrangements). **Pass 3** (`a3`): FormQuickBooksSetup's rows under the deposit note had not moved (the script read the note's height
after resizing it) and FormRecallTypeEdit's blockouts label needed 106 px for two lines. **Pass 4** (`a4`, the final build, 100% and
150%): every picture whose bytes differ from the render opened in an earlier pass opened again (the three pass-3 windows, the
Child Prophy state, FormCreditRecurringCharges, FormRpNetProdDetail, the four anchor windows whose pictures equal main's, the seven at
150%); the other 44 are byte-identical to the render opened before: nothing. **Review round 2** (branch `ui/ui12-5-2`): the
reviewer's eleven findings in nine windows (the compound panel of FormTransactionEdit, never drawn before; FormPerio's keypad;
ControlManage's Daily margins and message lists; FormShowFeatures' radio frame; FormTransactionEdit's Attach column; FormSheetDef's
group gap; FormEvaluationEdit's grade rows; FormGlobalPaymentsV2Setup's two field columns; FormSheetFieldOutput's right edges;
FormVitalsignEdit2014's Change Default), each fixed in the window's script (the lines below). **Pass 5** (`f2`, the fix build,
100% and 150%, the whole gallery drawn again and FormTransactionEdit's state `compound` added): the nine windows and the new state
opened: nothing; GridRulesTests lists no new finding.

## The slice's windows (100%)

| Window | Pass 1 | Then |
|---|---|---|
| FormRecallTypeEdit | The Child Prophy group (shown by the code instead of the trigger rows and the interval group) drawn over the triggers; the window 830 px wide for the Append note on one line | Script: the note wrapped after the check box's text at main's 646 px, the group on the trigger band with the groups' x and width; the gallery draws the two special-type states apart (state `childprophy`); pass 2: the blockouts label on two lines (106 px). Pass 4: ok |
| FormRpNetProdDetail | The list columns at 26 | Script: from 8, one width, 8 px before the groups; pass 2: the special-report note 8 px wider (cut 7 px). Pass 4: ok |
| FormBenefitEdit | The Applies-to fields at three x, the quantity box off the field column | Script: one field column; pass 2: Teeth's row on the pitch. Pass 4: ok |
| FormCareCreditSetup | Clinic Settings at 24, the picker's box 4 px short, Run on this computer cut | Script; pass 2: the Connection group keeps the band of the test's result note. Pass 4: ok |
| FormComputers | The list and the Fix group (window children over the Workstation group) over the note and Delete, the note cut | Script: the group around them again; pass 2: the note on two lines. Pass 4: ok |
| FormEHR | "Provider for this patient:" cut by its value, two label columns, side buttons with their texts on the edges | Script. Pass 2 and 4: ok |
| FormEhrAmendmentEdit | "Descriptio / n/Location" wrapped mid-word | Script: the label as wide as its text. Pass 2 and 4: ok |
| FormErxSetup | ok | Pass 4: ok |
| FormFeesForIns | The note's second line cut, both Change buttons on one place, two x for the right combo boxes, the list and grid at 13 | Script. Pass 2 and 4: ok |
| FormGlobalPaymentsV2Setup | The App Key row 30 px with its box 3 px low (the key note on the row); the clinic picker's own "Clinic" beside labelClinic (main too) | Script; the picker's ShowLabel = false. Pass 2 and 4: ok. Review round 2: Enabled, Use Sandbox and the clinic picker on a column of their own (189), 47 px after the group's field column; script: on the field column (142). Pass 5: ok |
| FormLabCaseEdit | The left column's right edges at five x | Script: one edge 8 px before Tracking. Pass 2 and 4: ok |
| FormShowFeatures | Special Use Only at 270 (Insurance at 262) | Script. Pass 4: ok. Review round 2: the framed charge radio pair at 47 in Charge Tools (47 px inside on the left, 8 on the right); script: on the content edge, to the right content edge. Pass 5: ok |
| FormSupplyEdit | ok (the rows with a two-line note are as tall as the note; known gaps) | Pass 4: ok |
| FormVitalsignEdit2014 | The reason row on a second field column after its one-line check box; the pregnancy notice (hidden) over the check box's text | Script; pass 2: main's 602 px width again. Pass 4: ok. Review round 2: Change Default's text 2-3 px from its border; script: 105 px (text + 2 x 8), the right edge kept, Pregnancy Code 6 px before it. Pass 5: ok (and at 150%) |
| UserControlSetupWizPrinter | The panel's rows and the Default combo box apart, the scanner group past the page, texts cut to three letters | Script from main's arrangement. Pass 2 and 4: ok |
| FormRpAppointments | The lists from 12, 46 px short of the groups' bottom, the Clinics All text on the group | Script. Pass 2 and 4: ok |
| FormAdvertisingPostcardsSetup | ok | Pass 4: ok |
| FormApptTypeEdit | The Required Procedures group 2 px off the list row, its list 27 px in | Script. Pass 2 and 4: ok |
| FormCreditRecurringCharges | The grid at 12 and 55 px over the bar, the side column at three right edges | Script; pass 2: Counts to the grid's bottom. Pass 4: ok |
| FormEvaluationEdit | The grids at 18 and 6 px apart | Script; pass 2: the overall grade rows on the field column (their labels 1 px from the boxes). Pass 4: ok. Review round 2: the overall grade boxes 20 px tall on a 26 px pitch, each override box 1 px after its computed box; script: 23 px on the 27 px pitch (441, 468 on the Delete / Save row), the override boxes 6 px after. Pass 5: ok |
| FormLaboratoryEdit | The rows' right edges at four x, the grid 36 px short, Add and Delete at 17 | Script. Pass 2 and 4: ok |
| FormQuickBooksSetup | Enable QuickBooks Class Refs between the columns | Script; pass 2: the deposit note on three lines (its third line cut); pass 3: the rows under it 16 px down. Pass 4: ok (the band under the check box holds the hidden class rows, as main) |
| FormRecallEdit | Delete at 26 | Script. Pass 4: ok |
| FormRepeatChargeEdit | ok in the picture | Pass 2: Charge Amount 2 px off the pitch (GridRulesTests): script. Pass 4: ok |
| FormSheetDefDefaults | The note's third line cut, the groups at 39, the picker's box off the column | Script. Pass 2 and 4: ok |
| FormSheetFieldOutput | The note, caption and list at 13-15 | Script. Pass 4: ok. Review round 2: three right edges in the right column (463, 470, 474); script: the Font group and the outer boxes to the content edge (Save's), Name and Align 8 px inside the group. Pass 5: ok |
| FormTransactionEdit | ok (the simple panel over the compound one, as main) | The gallery draws the simple panel alone (FillSimple/FillCompound show one). Pass 4: ok. Review round 2: the compound panel had not been drawn (the planner's layout: its button row ended 10 px past the split grid); new state `compound` (FormTransactionEdit-compound, 100% and 150%, no before picture: main's gallery has no such state); script: the grid to Export's right edge; the Source Documents group's Attach buttons at two x (340, 324): one column at 324, the three boxes one width. Pass 5: ok (both panels, 100% and 150%) |
| FormTreatPlanEdit | Heading and Note 26 px short of Save's edge | Script. Pass 4: ok (the band under User Entry as main: the signature rows the code shows) |
| ControlManage | The filter fields 72 px; the fourth caption and list and the History grid 5-7 px off the others | Script; pass 2: the server time 112 x 23. Pass 4: ok. Review round 2: the Daily buttons 104 px (42 px right margin against 16); the message list 13 px after Extras; script: 117 px columns at 16 and 139, the list, its caption, Send Message and the Sending note 6 px after Extras to 6 px before the History grid. Pass 5: ok (and at 150%) |
| FormRpPatients | The tab control and SQL box at 16 ending 93 px short; the referral lists 5 px short | Script. Pass 2 and 4: ok |
| FormAdvertisingPostcardsSend | Design, Mail class and Mail date labels stacked away from their fields (the runtime notes are empty in the designer) | Script from main's rows. Pass 2 and 4: ok (the empty bands are the notes the code writes) |
| FormCdsTriggerEdit | The groups at 12, an 80 px band under the grid | Script. Pass 2 and 4: ok |
| FormChartView | Family and Super Family lost their indent under Commlog | Script. Pass 2 and 4: ok |
| FormDiscountPlanSubEdit | Right edges at six x | Script. Pass 2 and 4: ok |
| FormEhrExportCCD | The sections caption cut, Result off its column | Script: the window as wide as the caption. Pass 2 and 4: ok |
| FormEhrLabPanelImport | The Info group at 12, right edges short of OK's | Script; pass 2: the group 8 px before the right column. Pass 4: ok |
| FormEhrMeasureCalcDetail | Notes and grid at 12 | Script. Pass 2 and 4: ok |
| FormEhrNotPerformedEdit | Description boxes and Note 80-90 px short; the hidden System Reason wrapping | Script. Pass 2 and 4: ok (the band in the reason group is the radio row the code shows) |
| FormEhrVaccinePatEdit | The right column's long rows at four right edges | Script. Pass 2 and 4: ok |
| FormEmailCertRegister | Right edges at six x | Script. Pass 2 and 4: ok |
| FormPerio | The F / L / L / F band labels stacked at the chart's top | Script: back on their bands. Pass 2 and 4: ok. Review round 2: the probing keypad off one grid (8 and 9 2 and 4 px right of the keys under them, gaps 6 / 4, row pitch 36 / 36 / 39); script: columns 1005 / 1041 / 1077, rows 137 / 173 / 209 / 245, the microphone on the 1 2 3 row, Launch Bola Voice on the 0 / 10 row. Pass 5: ok (and at 150%) |
| FormPodiumSetup | The trigger boxes 18 px into their group | Script. Pass 4: ok |
| FormProcSelect | The group and grid at 15, Negative Adjs' amount off its column | Script; pass 2: the amounts 71 px for "$99,999.99". Pass 4: ok |
| FormSheetDef | The two groups side by side on two tops | Script. Pass 2 and 4: ok. Review round 2: the Default Font group 20 px before the Font Size Reduction group; script: to 8 px before it (on the Description box's right edge), its Name box to the content edge. Pass 5: ok |

**Left the slice in pass 1** (designers back to main, UI-12.md "Skipped"): FormTasks and UserControlTasks (the planner moved the split
container's panels; the code lays the task panel out under a 24 px filter band), FormSheetFieldCheckBox (the field type switches its
rows in about eight states; the planner moved the hidden rows to the column ends), FormQuery (a user-dragged split container; the planner
moved its panels apart).

## At 150%

FormPerio, FormCareCreditSetup, UserControlSetupWizPrinter, ControlManage, FormEhrVaccinePatEdit, FormTransactionEdit,
FormVitalsignEdit2014 (the densest seventh). Pass 4: ok (the inputs at the scaled height, no text cut or covered). Pass 5: the same
seven and FormTransactionEdit-compound at 150%: ok (FormPerio's keypad on one grid, ControlManage's columns, Change Default's text with
room on both sides, the compound button row on the grid's right edge).

## The earlier slices' anchored controls

| Window | Pass 1 | Then |
|---|---|---|
| FormASAP (slice 1) | Print List at the bottom of the side column | Pass 4: ok |
| FormRpArizonaPrimaryCareEligibility, FormRpArizonaPrimaryCareEncounter (slice 2) | Copy Log Text, Run and Finished at the bottom of the button column on the log's bottom edge | Pass 4: ok |
| FormProcGroup (slice 2) | The signature label as tall as its box, its text where it stood | Pass 4: ok |
| FormProcCodeEdit (pilot), FormPayPlanTemplateEdit (slice 1), FormAdvertisingPatientList (slice 2), FormTaskEdit (slice 2) | No position change (the controls take the anchor of what they stand with) | Pass 4: ok, as main |
