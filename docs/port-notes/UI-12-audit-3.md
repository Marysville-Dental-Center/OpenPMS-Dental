# UI-12 slice 3: the audit of gallery G-UI3-ui12-3

Every after picture of `.git/od-tools/test-results/G-UI3-ui12-3` (the `a6` build, three windows redrawn from `a7`), opened and looked at as a
designer would: cut or covered text, overlaps, margins, columns and edges, gaps, crowding and empty bands, the control looks, titles, boxes
on the left. Before = `main` at the branch point (`33d9fce1f`), drawn by Run-Slice.ps1's before step in the same session. The passes ran on
the builds `a1` (the planner alone) to `a7` (UI-12.md, "Slice 3 (batch 1)"); a line names the pass that found something and what was done;
the last pass finds nothing in any picture. Notes on what stays: a group or window keeps the room of a row hidden in the designer that the
code shows (FormEClinicalWorks' HL7 definition warning, FormApptSearchAdvanced's Appt View, FormCreditCardEdit's procedures group,
FormEtransEdit's Attachments Sent, FormMaintItemEdit's next due line, FormDatabaseMaintenance's table check note); a window never shrinks
(FormPath's band over Save); controls the code switches by Visible stand on one bounds (FormCertifications' two views: since the fix
round the designer hides the Cert Complete view as Load does, and the gallery draws it as a state); FormSecurity's user group control is drawn at its designer height (FormSecurity.cs stretches it to the window's bottom on Load);
the menu bars (FormSecurity, FormOrthoChart) and the payment page (FormPayConnect2's WebView2) draw empty in the harness. The fix round
(section "Fix round" below) gives FormDatabaseMaintenancePat's note boxes their page's colour and adds the other tab pages and views.

**Pass 1** (`a1`, the planner's plan): 27 windows placed badly (FormBillingDefaults with no fixed point at 4097 px, FormPath at 1416 px
and FormUserQuery at 1551 px with long check boxes on one line, FormBillingOptions at 1184 px, overlapping rows kept as drawn in
FormCertifications, FormPayPeriodManager, FormAdjMulti, FormDatabaseMaintenancePat, FormUserEdit, captions and columns at several x
in the rest), small offsets in six planner windows. Fixes: 27 windows in mode hand with layout scripts, 6 scripts on the planner's tree.
**Pass 2** (`a3`): GridRulesTests' findings (inner labels 2 px into their fields where the window's column ignored the groups'
padding, field columns within 40 px of each other in FormEmailAddressEdit, FormInfobutton, FormPayPeriodManager and FormRpProdGoal,
FormEhrPatientSmoking's grids under their 559 px MinimumSize, the rows taller than the pitch in FormEClinicalWorks and FormCreditCardEdit,
the radio list pitch in FormUserQuery, two runtime texts cut) and the pictures' (FormSecurity's host size, FormPath's and FormEhrSettings'
empty note lines, FormUserQuery's split panels 14 px short, the settings groups of FormEmailAddressEdit short of the right edge, Setup
Dunning's and Change Password's texts filling their buttons, FormDepositEdit's Show group content at 14 with two list widths). All fixed.
**Pass 3** (`a4` to `a6`): FormBillingOptions' two buttons over the date group (stacked at the right edge), FormInfobutton's Task Code 8 px
off the Organization column, FormEmailAddressEdit's Gmail box 64 px short of its group's edge, FormEhrSettings' SNOMED CT text filling its
button, FormInsPlansOverrides' Overrides group 6 px after the grid and Select All 3 px past it. All fixed (`a7`).
**Pass 4** (`a6`, `a7`, every picture below): nothing found.

| Picture | Status | What was checked and found |
|---|---|---|
| 1-slice-windows/FormAdjMulti.png | changed | pass 1: the credit choices drawn into each other, the grid at 31; script: Adjustment Info's two groups on the Date box's column, the lists one width, the grid and its buttons, the note and Credit Filter one bottom. Pass 2: the inner labels 2 px into their boxes (the column now 2 x 8 + label + 6). Pass 4: nothing. |
| 1-slice-windows/FormApptSearchAdvanced.png | changed | pass 1: Starting after 4 px low, its am/pm 9 px right; script: the filter rows on one column, am/pm 16 px after the boxes, grid and group one bottom. Pass 4: nothing (the group's last row is Appt View's, shown by the code). |
| 1-slice-windows/FormBillingDefaults.png | changed | pass 1: no fixed point, 4097 px; script: picker row, Statements in two columns (the label where the check box text starts), Messages with the email group. Pass 4: nothing. |
| 1-slice-windows/FormBillingOptions.png | changed | pass 1: 1184 px, the long check box on one line; script: Filter in the Preferences style, the dunning column. Pass 2: Setup Dunning's text filled its button (98 px). Pass 3: the two buttons over the date group, stacked at the right edge. Pass 4: nothing. |
| 1-slice-windows/FormCertifications.png | changed | pass 1: the Cert Complete view's lists over the grid; script: the two views on one bounds. Pass 4: nothing (both views draw in the harness; the code shows one). |
| 1-slice-windows/FormChooseDatabase.png | changed | pass 1: the SSL path and the middle tier group 20-32 px in; script: everything from the padding, two columns one width. Pass 4: nothing. |
| 1-slice-windows/FormClockEventEdit.png | changed | pass 1: the groups at 80, Delete at 31; script: everything from 8, one field column, the time groups' rows lined up. Pass 4: nothing (the two-line Total Time row: grid-known). |
| 1-slice-windows/FormCreditCardEdit.png | changed | pass 1: the frequency group off the column; script: one field column, the right column, the frequency group. Pass 2: the two-line Run charge row broke the pitch (its own row now, one line). Pass 4: nothing (the band is the procedures group, shown by the code). |
| 1-slice-windows/FormDatabaseMaintenance.png | changed | pass 1: the page's bottom band, None off the grid's edge; script: every page from its padding, the button rows. Pass 4: nothing. |
| 1-slice-windows/FormDatabaseMaintenancePat.png | changed | pass 1: the Old page's note over its grid; script: the pages, the Patient row. Pass 4: nothing (white note boxes: main's text boxes). |
| 1-slice-windows/FormDepositEdit.png | changed | pass 1: the right column's boxes 76 px short of Send QB's edge; script on the planner's tree. Pass 2: the Show group's content at 14, the two lists 165 and 172 (8, one width). Pass 4: nothing. |
| 1-slice-windows/FormDisplayFieldOrthoEdit.png | changed | pass 1: (optional) 3 px high, Up/Down between rows; script on the planner's tree. Pass 2: (this tab only) cut (runtime text). Pass 4: nothing. |
| 1-slice-windows/FormEClinicalWorks.png | changed | pass 1: the HL7 server row off the pitch, the group at 42; script. Pass 2: the two-line note's row taller than the pitch: the window 78 px wider, the notes one line. Pass 4: nothing (the band is the HL7 definition warning, shown by the code). |
| 1-slice-windows/FormEhrLabPanelEdit.png | changed | pass 1: the box, groups and grid at 12, three right edges; script. Pass 2: the groups' labels 2 px into their boxes. Pass 4: nothing. |
| 1-slice-windows/FormEhrPatientSmoking.png | changed | pass 1: the filter choices before the column; script. Pass 2: the grids under their MinimumSize (the window 64 px wider). Pass 4: nothing. |
| 1-slice-windows/FormEhrSettings.png | changed | pass 1: an empty band over the code buttons in one group; script. Pass 2: the note's empty second line (measured wrap). Pass 3: SNOMED CT filled its button (85 px). Pass 4: nothing. |
| 1-slice-windows/FormEmailAddressEdit.png | changed | pass 1: the groups and fields at several x; script, the authentication numbers from the grid (FormEmailAddressEdit.cs). Pass 2: the field columns within 40 px (one column, the password box's), the settings groups short of the edge. Pass 3: the Gmail box short of its group's edge. Pass 4: nothing. |
| 1-slice-windows/FormEtrans835ClaimPay.png | changed | pass 1: the captions over Deductible and Write-off at two heights; script. Pass 4: nothing (the total boxes under their grids' columns: grid-known). |
| 1-slice-windows/FormEtransEdit.png | changed | pass 1: the sent message 7 px above the received one; script, two columns one width. Pass 4: nothing. |
| 1-slice-windows/FormImagingSetup.png | changed | pass 1: Set Default Scanner's text filled its button (124 px, script on the planner's tree). Pass 4: nothing. |
| 1-slice-windows/FormInfobutton.png | changed | pass 1: two field columns, the add buttons' texts against their icons; script. Pass 2: the groups on one field column (the window 82 px wider). Pass 3: Task Code 8 px off the Organization column. Pass 4: nothing. |
| 1-slice-windows/FormInsPlans.png | changed | the filter rows on one pitch, the bands 16 px apart, the grid to the edges. Nothing found. |
| 1-slice-windows/FormInsPlansOverrides.png | changed | pass 3: the Overrides group 6 px after the grid and 7 px short of the edge, Select All 3 px past the grid (script on the planner's tree). Pass 4: nothing. |
| 1-slice-windows/FormMaintItemEdit.png | changed | pass 1: the day boxes off the column, the button rows at two x; script. Pass 4: nothing. |
| 1-slice-windows/FormMedPat.png | changed | pass 1: the Edit note 4 px high, Remove centred over its note; script on the planner's tree. Pass 4: nothing. |
| 1-slice-windows/FormOperatoryEdit.png | changed | pass 1: the notes 1-2 px off their rows, the window 665 px for a 363 px note; script on the planner's tree (618 px). Pass 4: nothing. |
| 1-slice-windows/FormOrthoChart.png | changed | pass 1: the grid at 10, the bottom bar uneven; script. Pass 2: the lock note cut with a user name (runtime text). Pass 4: nothing. |
| 1-slice-windows/FormPath.png | changed | pass 1: 1416 px, the check box on one line; script at main's width. Pass 2: the notes' empty first lines. Pass 4: nothing. |
| 1-slice-windows/FormPayConnect2.png | changed | pass 1: the left column at 21; script inside the narrow window the code sets. Pass 4: nothing (the payment page draws empty in the harness). |
| 1-slice-windows/FormPayPeriodManager.png | changed | pass 1: the count's label under its box; script. Pass 2: Start Date 32 px from the Semi-Monthly column (a captioned box now). Pass 4: nothing. |
| 1-slice-windows/FormProcButtonEdit.png | changed | pass 1: the check box at 12, the captions at two heights; script. Pass 4: nothing. |
| 1-slice-windows/FormRpProdGoal.png | changed | pass 1: the provider column at 34; script as slice 2's report windows. Pass 2: From/To 32 px from Today's Date's box (one column). Pass 4: nothing. |
| 1-slice-windows/FormSecurity.png | changed | pass 1: the filter captions under the group title, Show hidden users box on the right; the control's script (as a part of this window). Pass 2: the host size kept at the designer's 484 px. Pass 4: nothing. |
| 1-slice-windows/FormTsiHistory.png | changed | pass 1: the clinic box 12 px short of Client IDs (script on the planner's tree). Pass 4: nothing. |
| 1-slice-windows/FormUserEdit.png | changed | pass 1: the logoff label over the user group caption; script. Pass 2: Change Password's text filled its button. Pass 4: nothing. |
| 1-slice-windows/FormUserQuery.png | changed | pass 1: 1551 px; script. Pass 2: the split panels 14 px short (their sizes), the radio pitch. Pass 4: nothing. |
| 1-slice-windows/UserControlSecurityTree.png | same | the tree to the control's edges. Nothing found. |
| 2-at-150/FormBillingOptions-150.png | changed | 150%: the labels, check boxes and fields at the scaled font, nothing cut. Nothing found. |
| 2-at-150/FormClockEventEdit-150.png | changed | 150%: both time groups' rows lined up, Total Time's two lines whole. Nothing found. |
| 2-at-150/FormCreditCardEdit-150.png | changed | 150%: the field column and the right column whole. Nothing found. |
| 2-at-150/FormEClinicalWorks-150.png | changed | 150%: the notes after the boxes on one line. Nothing found. |
| 2-at-150/FormEmailAddressEdit-150.png | changed | 150%: the password row on the Password choice's row, the settings notes whole. Nothing found (`a7`). |
| 2-at-150/FormUserEdit-150.png | changed | 150%: the logoff label's two lines, the lists one width. Nothing found (the tab captions draw at 100% in the harness, as before). |

## Fix round (the lead's review of 2026-11-23)

The gallery redrawn from the fix round's builds in one session: before = `b8` (the branch point's designers, this branch's harness with
the new states), after = `a8` (UI-12.md section 7, "Slice 3, fix round"). Every after picture of the round's windows and every new state
was opened again; the other windows' after pictures are the same layout as in pass 4, drawn again in this session.

| Picture | Status | Finding and result |
|---|---|---|
| 1-slice-windows/FormUserQuery.png | changed | Review 1 and 4: Print Orientation's caption on the row of Portrait and Landscape (the button panel 128 px, within the 140 px Load gives the upper panel); the panels' frame runs unbroken (measured: x 8 and 1267 at 209 gray from top to bottom, except the splitter band). Nothing found. |
| 2-at-150/FormUserQuery-150.png | changed | 150%: the button panel, the Format group and the Print Orientation row whole inside the frame. Nothing found. |
| 1-slice-windows/FormCertifications.png | changed | Review 3: the Category view only (Show Only Incomplete, Sort By Date, Employee and Categories lists); the Cert Complete controls hidden as Load hides them. Nothing found. |
| 1-slice-windows/FormCertifications-certcomplete.png | changed (new) | The Cert Complete view: Sort By Date Completed on Show Only Incomplete's place, the Categories and Certifications lists on the Employee and Categories lists' bounds; Employee Search and Reports To hidden as the code hides them (their band stays, as in main). Nothing found. |
| 1-slice-windows/FormDatabaseMaintenancePat.png | changed | Review 5: the Patient caption and the None note in the window's and page's colour, no white rectangles. Nothing found. |
| 1-slice-windows/FormDatabaseMaintenancePat-hidden.png | changed (new) | The Hidden page: the grid to the page's edges, its note under it on one line. Nothing found. |
| 1-slice-windows/FormDatabaseMaintenancePat-old.png | changed (new) | The Old page: the three-line note at the left and the None note on one top, Show Hidden on the Check and Fix row; the note box in the page's colour. Nothing found. |
| 1-slice-windows/FormDatabaseMaintenance-hidden.png | changed (new) | The Hidden page: the note at the padding, the grid 8 px under its box (the box keeps main's two lines: the note nearly fills the width and wraps at 150% or in a longer translation), Select All over Explain at the right edge. Nothing found. |
| 1-slice-windows/FormDatabaseMaintenance-old.png | changed (new) | The Old page: the note as on the Hidden page, the None row and the Check, Fix, Stop DBM, Explain row at the right edge, Show Hidden at the left. Nothing found. |
| 1-slice-windows/FormDatabaseMaintenance-tools.png | changed (new) | The Tools page: the update group (its note, box and Clear) to the content edge, the three-line note and the grid under it, Fix at the right edge. Nothing found. |
| 1-slice-windows/FormPath-database.png | changed (new) | The Database page: the warning note from the page's padding, five lines whole. Nothing found. |
| 1-slice-windows/FormPath-removed.png | changed (new) | The Removed page: the note at the padding. Nothing found. |
| 1-slice-windows/FormPath-sftp.png | changed (new) | The SFTP page: the two notes, the four fields on one column, Clear after the Password box, SSH Host Keys on the column. Nothing found. |
| 1-slice-windows/FormUserEdit.png | changed | Review 6: the User page in the window's background (250), like the slice's other pages. Nothing found. |
| 1-slice-windows/FormUserEdit-clinics.png | changed (new) | The Clinics page: both lists from the padding, one width and one top, 8 px apart, their captions on them. Nothing found. |
| 1-slice-windows/FormUserEdit-alertsubs.png | changed (new) | The Alert Subs page: as the Clinics page. Nothing found. |
| 2-at-150/FormUserEdit-150.png | changed | 150%: the page in the window's background. Nothing found (the tab captions draw at 100% in the harness, as before). |
| 2-at-150/FormUserEdit-clinics-150.png, 2-at-150/FormUserEdit-alertsubs-150.png | changed (new) | 150%: the lists and captions whole. Nothing found. |
| 1-slice-windows/FormSecurity-usergroups.png | changed (new) | The User Groups page of the hosted control (its `--part` script): the three lists one top, 8 px apart, captions on them; Add Group and Edit Group, Set All and Set None, Collapse All and Expand All under their lists. Drawn at the designer height, as the Users page. Nothing found. |
| 2-at-150/FormSecurity-usergroups-150.png | changed (new) | 150%: buttons and captions whole. Nothing found. |
| 1-slice-windows/FormDepositEdit.png | changed | Review 7: Date, Amount and Item Count 2 px over their boxes, on the boxes' left edges, like the column's other captions. Nothing found. |
| 1-slice-windows/FormInsPlans.png, 1-slice-windows/FormInsPlansOverrides.png | changed | Review 8: Get All and Show Hidden 8 px after Order By in both (compared side by side). Nothing found. |
| 1-slice-windows/FormOperatoryEdit.png | changed | Review 9 and 10: the Clinic box ends with the other boxes; Save 24 px, 8 px from the bottom and right edges. Nothing found. |
| 1-slice-windows/FormEhrPatientSmoking.png | changed | Review 11: the filter choices of both groups on 134, 189, 254, 345; the Frequent choices one above the other. Nothing found. |
| 1-slice-windows/FormMedPat.png | changed | Review 12: Save 8 px under Remove's note, no empty band. Nothing found. |

A second look at every picture above after the gallery was assembled found nothing more.
