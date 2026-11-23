# UI-12 slice 2: the audit of gallery G-UI3-ui12-2

Every after picture of `.git/od-tools/test-results/G-UI3-ui12-2` (the `a6` build), opened and looked at as a designer would: cut or covered
text, overlaps, margins, columns and edges, gaps, crowding and empty bands, the control looks, titles, boxes on the left. Before = `main` at
the branch point (`647febb65`). The passes ran on the builds `a1` (the planner alone) to `a6` (UI-12.md, "Slice 2"); a line names the pass
that found something and what was done; the last pass (on `a6`) finds nothing in any picture. Notes on what stays: a group keeps the room of a
row hidden in the designer at its end (the rule of section 1: FormGlobalSecurity's Central Manager note, FormTaskPreferences' Global
Filter), a window never shrinks (the merge windows keep a band over Merge), FormGraphics' preview is the tooth chart's own surface (its
picture is not scaled by the 150% harness).

Pictures at 100% and 150% were drawn with the session's font smoothing on (ClearType) for both sides (UI-12.md, "Slice 2": the setting
changed during the session; the before pictures were redrawn, `m3`, so each pair shares it).

**Pass 1** (`a1`, the planner's plan): 25 windows placed badly (two without a fixed point, one at 4000 px, the grid windows' filter rows cut
as columns, the calendars' captions covered), 12 cut texts in the pilot windows, small offsets in the rest. Fixes: 19 windows in mode hand
with layout scripts, 19 scripts on the planner's tree, 5 windows left for the continuation, the pilot follow-up rows.
**Pass 2** (`a2` to `a5`): the column findings (the date range boxes, Today's Date, Min, the undo date), FormEtrans835s' Refresh, FormTimeCard's
totals group, FormRpPayPlans' options, the presentation reports' OK, FormEtrans270Edit's empty band, the 150% pictures' two text sizes
(the harness now scales check boxes, buttons and group titles with the labels). All fixed.
**Pass 3** (`a6`, every picture below): nothing found.

| Picture | Status | What was checked and found |
|---|---|---|
| 1-slice-windows/FormAudit.png | changed | filter rows on one pitch, labels 6 px before their boxes, Refresh and Print on the right column; grid to both edges. Nothing found. |
| 1-slice-windows/FormBilling.png | changed | pass 1: the planner's 1560 px spread (no fixed point); mode hand: radio list, two field columns, picker, date column, 110 px right column; the counts whole at their runtime width. Pass 2: nothing. |
| 1-slice-windows/FormCareCreditTransactions.png | changed | filter row on the date range; Status in the range's week-button room (grid-known, as main); the tab control and grid to the edges. Nothing found. |
| 1-slice-windows/FormClaimsSend.png | changed | pass 1: 4000 px (the planner around the splitter); mode hand: one filter row under the toolbar, grid and history touching the splitter, the history range's From box under the clinic box. Pass 2: nothing. |
| 1-slice-windows/FormConfirmList.png | changed | the View group's two field columns, the Set Status and Email From groups one height; grid and the bottom bar. Nothing found. |
| 1-slice-windows/FormDiscountPlanMerge.png | changed | pass 1: captions 1-4 px off their boxes; script: captioned rows, Change on the edge. Pass 2: nothing (the band over Merge is the window's kept height). |
| 1-slice-windows/FormDunningEdit.png | changed | pass 1: left column at 12, the email group 12 px lower, the list 34 px short; script: columns under the picker row, the list to the groups' bottom. Pass 2: nothing. |
| 1-slice-windows/FormDunningSetup.png | changed | pass 1: the picker on no column, the list short of the groups; script. Pass 2: nothing. |
| 1-slice-windows/FormEdgeExpressSetup.png | changed | pass 1: the clinic note cut (446 px in 413), three field columns, air in Card Entry Page; mode hand, one field column. Pass 2: nothing. |
| 1-slice-windows/FormEhrLabSpecimenEdit.png | changed | two groups on one field column, the two grids to the right edge; Save. Nothing found. |
| 1-slice-windows/FormEtrans270Edit.png | changed | pass 1: four heights in the bottom block; mode hand; pass 2: an empty band left of the benefits grid, the Note box now fills it. Pass 3: nothing. |
| 1-slice-windows/FormEtrans835s.png | changed | pass 1: Finalized Only over the status list, the Amount Range title cut; mode hand; pass 2: Refresh over Automatable Carriers Only (fixed: the window 37 px wider), Min off the From column (fixed). Pass 3: nothing. |
| 1-slice-windows/FormFeeScheds.png | changed | pass 1: grid and group at 17, the group's note cut (3 lines in 42 px); script. Pass 2: nothing (the centred notes under their buttons are main's). |
| 1-slice-windows/FormFinanceCharges.png | changed | pass 1: 1830 px (no fixed point); mode hand, the filters in the Preferences style. Pass 2: nothing. |
| 1-slice-windows/FormGlobalSecurity.png | changed | pass 1: the logoff box over its label, the groups on a narrow column; mode hand. Pass 2: the Global Lock group keeps the room of its hidden Central Manager note (Load shows it; a designer-hidden end row). Nothing else. |
| 1-slice-windows/FormGraphics.png | changed | pass 1: two radio buttons over one another, 28 px margins; mode hand. Pass 2: nothing. |
| 1-slice-windows/FormInsVerificationSetup.png | changed | pass 1: the Standard group in the window's middle, check boxes at x 1; mode hand. Pass 2: nothing. |
| 1-slice-windows/FormLateCharges.png | changed | pass 1: rows over three columns; mode hand; pass 2: the undo date off the window's date column (fixed). Pass 3: nothing. |
| 1-slice-windows/FormLetterMergeEdit.png | changed | pass 1: the selection hint's second line cut (main too), the lists on two bottoms; script. Pass 2: nothing. |
| 1-slice-windows/FormMedicationMerge.png | changed | pass 1: caption offsets; script. Pass 2: nothing. |
| 1-slice-windows/FormPatientMerge.png | changed | pass 1: the second group's title cut (709 px in 677); script, the window 741 px. Pass 2: nothing. |
| 1-slice-windows/FormPaySimple.png | changed | pass 1: the right column's captions 9 px under the left's, a 60 px band under the fields; mode hand. Pass 2: nothing. |
| 1-slice-windows/FormPharmacyEdit.png | changed | pass 1: the Canadian city label would be cut (runtime-text check); script. Pass 2: nothing. |
| 1-slice-windows/FormProcGroup.png | changed | pass 1: the first row 3 px under its label, Edit Auto Note's text wrapped (main too); script. Pass 2: nothing (the signature boxes' buttons are SignatureBoxWrapper's). |
| 1-slice-windows/FormProgramLinkEdit.png | changed | pass 1: Text on button and Plug-in dll name paired with the wrong boxes, the Notes caption at the picker; script. Pass 2: nothing. |
| 1-slice-windows/FormProviderMerge.png | changed | pass 1: caption offsets; script. Pass 2: nothing. |
| 1-slice-windows/FormReferralMerge.png | changed | pass 1: caption offsets; script. Pass 2: nothing. |
| 1-slice-windows/FormRpArizonaPrimaryCareEligibility.png | changed | pass 1: left column at 12, Copy Log Text's text filling its button; script. Pass 2: nothing. |
| 1-slice-windows/FormRpArizonaPrimaryCareEncounter.png | changed | pass 1: left column at 9, the same button; script. Pass 2: nothing. |
| 1-slice-windows/FormRpBrokenAppointments.png | changed | pass 1: captions under the calendars' Today line, TO behind a calendar; mode hand. Pass 2: nothing. |
| 1-slice-windows/FormRpClaimNotSent.png | changed | one filter group row and the grid to the edges. Nothing found. |
| 1-slice-windows/FormRpHiddenPaySplits.png | changed | pass 1: lists at two widths, captions 1 px apart; script (the Clinics column shows with clinics). Pass 2: nothing. |
| 1-slice-windows/FormRpInsAging.png | changed | the options column and three list columns on one top; OK. Nothing found. |
| 1-slice-windows/FormRpPayPlans.png | changed | pass 1: left column at 23-47, lists at two widths; script; pass 2: the options' text ran under the Providers list (fixed). Pass 3: nothing. |
| 1-slice-windows/FormRpPresentedTreatmentProduction.png | changed | pass 1: captions under the calendars' Today line; mode hand; pass 2: OK over the last group (fixed: the window 11 px taller). Pass 3: nothing. |
| 1-slice-windows/FormRpProcedures.png | changed | pass 1: the Clinics box 9 px off Providers; mode hand. Pass 2: nothing. |
| 1-slice-windows/FormRpProdInc.png | changed | pass 1: lists at two tops, the write-offs note cut; mode hand; pass 2: Today's Date off the range's column (fixed). Pass 3: nothing. |
| 1-slice-windows/FormRpProviderPayroll.png | changed | pass 1: captions 20 px over their boxes; mode hand; pass 2: Today's Date off the range's column (fixed). Pass 3: nothing. |
| 1-slice-windows/FormRpTreatPlanPresentationStatistics.png | changed | as FormRpPresentedTreatmentProduction (pass 2: OK over the last group, fixed). Pass 3: nothing. |
| 1-slice-windows/FormScreenGroupEdit.png | changed | fields on one column, the patients grid and its buttons; the Screenings grid to the edges. Nothing found. |
| 1-slice-windows/FormTaskPreferences.png | changed | Preferences style; the Global Settings group keeps the room of its hidden Global Filter row (a designer-hidden end row). Nothing else. |
| 1-slice-windows/FormTaskSearch.png | changed | pass 1: four right edges in the search panel; script. Pass 2: nothing. |
| 1-slice-windows/FormTimeCard.png | changed | pass 1: Time Card/Breaks over one another; mode hand; pass 2: the totals radios under the group's title band and inside its width (fixed). Pass 3: nothing (the Employee group shows at run time). |
| 1-slice-windows/FormTrackNext.png | changed | pass 1: rows 14 px under the menu, the range off the first column; script; pass 2: the second column on the To box (fixed). Pass 3: nothing. |
| 1-slice-windows/FormUnsched.png | changed | pass 1: the grid in the window's middle; mode hand; pass 2: the second column on the To box (fixed). Pass 3: nothing. |
| 1-slice-windows/FormXchargeSetup.png | changed | pass 1: Payment Type and the picker 72 and 88 px short; script. Pass 2: nothing. |
| 2-at-150/FormBilling-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormDunningEdit-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormEdgeExpressSetup-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormEtrans835s-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormFinanceCharges-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormGlobalSecurity-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormGraphics-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormInsVerificationSetup-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormLateCharges-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormProcGroup-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormRpProdInc-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormTimeCard-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormUnsched-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 2-at-150/FormXchargeSetup-150.png | changed | at 150% (labels, check boxes, buttons and group titles at the scaled font with the inputs): rows, columns and text whole. Nothing found. |
| 3-styles/FormPayment-inputs-100.png | same | the same pixels as before (the harness changes 150% only; the picker's 100% area is unchanged). Nothing found. |
| 3-styles/FormPayment-inputs-150.png | same | the same pixels as main's product drawn with this branch's harness: the labels and check boxes at the scaled font beside the inputs; the picker's one-rounding edge changes no box here (its x and area did not both round up). Nothing found. |
| 3-styles/clinic-picker-100.png | same | the same pixels as before (the harness changes 150% only; the picker's 100% area is unchanged). Nothing found. |
| 3-styles/clinic-picker-150.png | same | the same pixels as main's product drawn with this branch's harness: the labels and check boxes at the scaled font beside the inputs; the picker's one-rounding edge changes no box here (its x and area did not both round up). Nothing found. |
| 4-pilot-follow-up/ControlAccount-150.png | same | at 150%: the Annual Max captions and the TOTAL Owed caption are in the hidden summary groups and the off-strip panel, not drawn here; GridRulesTests' text check passes. Nothing found. |
| 4-pilot-follow-up/ControlAccount.png | same | the Annual Max captions and the TOTAL Owed caption are in the hidden summary groups and the off-strip panel, not drawn here; GridRulesTests' text check passes. Nothing found. |
| 4-pilot-follow-up/ControlChart-150.png | same | at 150%: the Show page's check boxes and the Draw page's note are not on the page drawn; GridRulesTests passes; the Enter Treatment page as approved. Nothing found. |
| 4-pilot-follow-up/ControlChart.png | same | the Show page's check boxes and the Draw page's note are not on the page drawn; GridRulesTests passes; the Enter Treatment page as approved. Nothing found. |
| 4-pilot-follow-up/ControlTreat-150.png | changed | at 150%: 'Ded Remain' whole in the individual insurance summary. Nothing found. |
| 4-pilot-follow-up/ControlTreat.png | same | 'Ded Remain' whole in the individual insurance summary. Nothing found. |
| 4-pilot-follow-up/FormApptEdit-150.png | changed | at 150%: the time bar's markers 1 and 2 whole (15 px). Nothing found. |
| 4-pilot-follow-up/FormApptEdit.png | changed | the time bar's markers 1 and 2 whole (15 px). Nothing found. |
| 4-pilot-follow-up/FormBackup-150.png | same | at 150%: the Archive page's Server Name note is not on the page drawn; GridRulesTests passes. Nothing found. |
| 4-pilot-follow-up/FormBackup.png | same | the Archive page's Server Name note is not on the page drawn; GridRulesTests passes. Nothing found. |
| 4-pilot-follow-up/FormClaimEdit-150.png | changed | at 150%: the missing teeth note is on the General tab, not the tab drawn; GridRulesTests passes. Nothing found. |
| 4-pilot-follow-up/FormClaimEdit.png | changed | the missing teeth note is on the General tab, not the tab drawn; GridRulesTests passes. Nothing found. |
| 4-pilot-follow-up/FormMedLabEdit-150.png | changed | at 150%: the print note's two lines whole, Show HL7 16 px after it. Nothing found. |
| 4-pilot-follow-up/FormMedLabEdit.png | changed | the print note's two lines whole, Show HL7 16 px after it. Nothing found. |
| 4-pilot-follow-up/FormPractice-150.png | changed | at 150%: the two-line title label whole, centred on its box. Nothing found. |
| 4-pilot-follow-up/FormPractice.png | changed | the two-line title label whole, centred on its box. Nothing found. |

## Continuation (2026-11-23): the five windows left

The five windows the slice left (UI-12.md, "Slice 2, continuation"), before = main's designers drawn with this branch's harness (`b1`),
after = `a7`, at 100% and 150%, both under the session's text smoothing of that hour (ClearType: `SystemInformation.FontSmoothingType` 2; no setting changed). The passes: `c1` (the scripts'
first run), `c2`, `a7`.

**Pass 1** (`c1`): GridRulesTests' column findings (FormClaimPayBatch's filter fields 38 px and 18 px off the Payment Details columns,
Type/Group 6 px off Claim ID; FormTaskEdit's description, Attachments and Object Type on three columns 12 to 24 px apart, the reminder
group's and the repeating panel's columns 24 px apart; FormPayConnectSetup's Clinic 8 px off Secret), the reminder group's panels at its
edge (no padding), FormTaskEdit's unused hidden `textBox1` 72 px over the window; in the pictures the reminder group and the wizard's day
groups not shown (hidden in the designer). Fixes: one column per side (the scripts), the panels inside the padding, `textBox1` on TaskNum's
bounds, the gallery draws the default reminder group and the checked days; the weekly day panel under the frequency panel (the code shows
both: found in `RefreshReminderGroup` while checking the stacked panels).
**Pass 2** (`c2`): nothing found; GridRulesTests passes on the five.
**Pass 3** (`a7`, every picture below; the 100% pictures are byte-identical to `c2`'s, and the 80 pictures of the 46 windows, the
styles and the pilot follow-up byte-identical to `a6`'s, so their lines above stand): nothing found.

| Picture | Status | What was checked and found |
|---|---|---|
| 1-slice-windows/FormAdvertisingPatientList.png | changed | User Query; the clinic picker, Patient Age and Appointments one top and one height, the date pickers' boxes on one column after their check boxes' text, the three lists one width with captions 2 px over them; Refresh on the lists' bottom 8 px after the panel; grid to both edges; the bottom bar's count whole. Nothing found. |
| 1-slice-windows/FormClaimPayBatch.png | changed | the instructions column whole; Payment Details' three field columns at the row pitch, Carrier Name/Note and Type/Group to one edge, Edit on the last row; the arrows, the centred Attach buttons and Total Payments under the grid; the filter fields on the Payment Details columns; both grids to the right edge; View ERA/View EOB on the grids' bottom. Nothing found. |
| 1-slice-windows/FormPayConnectSetup.png | changed | Enabled, the clinic note (two lines) and Clinic on the Secret column beside the Version 2.0 group (its note's two lines whole); Secret with Test connection, the check boxes under the box; Payment Type and Default Processing Method on one column (the long label on one line), the portal group under them; Terminals to the bottom bar. Nothing found. |
| 1-slice-windows/FormTaskEdit.png | changed | the check row, the three date rows with Now; the reminder group (Type, Every with its unit, the day boxes and their key) on one column; the right column with its buttons; the description, Short Descript, Attachments and Object Type on the date fields' column; the object panel and TaskNum; the bottom bar. Width 1002 (the code's). Nothing found. |
| 1-slice-windows/UserControlSetupWizSchedule.png | changed | the provider line and Next Prov note whole (main's picture had SetupWizControl's labels over them: the harness fix); the seven groups one size on the four columns, Friday to Sunday centred, each check box on its group's title band, Add at the bottom left. Nothing found. |
| 2-at-150/FormAdvertisingPatientList-150.png | changed | at 150%: groups, date pickers, list captions and the count whole and on their columns. Nothing found. |
| 2-at-150/FormClaimPayBatch-150.png | changed | at 150%: the three field columns, the filter row on them, the instructions' lines, Total Payments whole. Nothing found. |
| 2-at-150/FormPayConnectSetup-150.png | changed | at 150%: Default Processing Method on one line, both notes whole, Clinic on the Secret column. Nothing found. |
| 2-at-150/FormTaskEdit-150.png | changed | at 150%: the date rows, the reminder group's rows and day key, the right column, the bottom rows and bar whole. Nothing found. |
| 2-at-150/UserControlSetupWizSchedule-150.png | changed | at 150%: the layout scales; the page's text stays at its 100% size on both sides (the harness does not scale the fonts of a page the program adds with AddUnscaled; main's picture the same). Nothing found. |
