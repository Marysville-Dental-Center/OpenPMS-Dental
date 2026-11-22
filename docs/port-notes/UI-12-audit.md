# UI-12 pilot: the audit of gallery G-UI3-ui12p

The owner (2026-11-21): "do a round of audit and fix first before I review further. Last round of quality is low". Every after picture of the
gallery was opened and looked at as a whole window (not only the numbers): cut text, overlaps, uneven margins, misaligned columns or edges,
crowding, empty areas, inconsistent control looks, covered titles, check boxes not on the left. One line per picture and pass. A finding
fixed in a later pass says so; what is still open at the end is in the last pass's lines and in UI-12.md section 4.

The pictures are drawn off screen by GDI without ClearType (a disconnected session): text looks heavier than on a desktop.

## Pass 1 (gallery passes 1 to 5 of the work, the first tool results)

Opened: the windows below (the preference pages through UserControlFamilyGeneral, UserControlApptAppearance and
UserControlAccountRecAndRepCharges as their samples). Findings and what was done:

- FormClaimEdit: doubled text and outlines (the owner's "bad overlay") on combos, labels and the Totals row: the gallery's drawing (fixed in the
  gallery: each control printed without its children). After that the tool's layout cut the stacked bottom buttons and nested groups: window
  set to hand (kept as on main).
- FormPatientEdit: good rows and columns; the Age box drawn inside the Family Relationships list's area; the window passes 1000 px (owner question).
- FormProcEdit: the tooth/surface panels vanished from their place, "Time Start" label cut, the Prosthesis group under the tab control: hand.
- ControlAccount: the right panel moved and the module widened: hand.
- FormPayment: labels parted from their fields in the top rows, the window widened to 1440 px, check boxes over the Outstanding grid's title: hand
  (the owner's row is the input style, which is in).
- FormApptEdit: fields column on three x values, the panel widened past the quick-add list and the window to 1590 px: hand, the quick-add note fixed.
- FormInsPlan: subscriber panel under the benefit grid, the window 963 px tall: hand, the ortho check box fixed.
- FormProcCodeEdit: rows on one pitch, but the time bar column split from its slider, "Is Radiology" and "Bypass Global Lock Date" off the field
  column: rule added (a parted check box returns to the field column); the time bar group kept as drawn.
- FormProvEdit: good; the fields under the Clinic Overrides group 8 px right of the ones above (another stack): open, see the last pass.
- FormPractice: the Billing Address note's first line under the group's title; Save floating under a group: rules added (wrapped notes as tall as
  their lines; dialog buttons bottom right).
- FormBackup: good; Backup and Restore groups' boxes at different x (two containers).
- FormInsBenefits: the boxes are no longer touching; the Other Benefits grid drawn over the Categories group (the grid is placed by Load at run
  time, which the harness does not run): known.
- FormStatementOptions: the four groups of different heights: rule added (rows of containers across columns).
- FormEmailInbox: "View messages for address:" whole; good.
- FormHL7DefEdit: good; HL7 options fields at two x values: rule added (one field column per stack).
- FormScreenEdit: the Race/Ethnicity and Urgency lists off the field column: rule added (a label beside a list pairs with it).
- Preference pages (sampled): boxes on the left, groups spanning the page, equal margins; the content of a group started 12 px under the title's
  letters (title gap refined to 4 px); fields of one group at two or three x values (rule added); pages taller than FormPreferences' page area
  (it does not scroll): FormPreferences now sets AutoScroll on its pages and the planner keeps the scroll bar's width free.
- Styles: the page bar drawn with chevrons; the procedure buttons on their grid; FormPayment's inputs all with the combo box's frame.

## Pass 2 (the fixed-point planner, gallery run after3)

- UserControlMainWindow: boxes left, groups span the page, equal margins; an empty row in Select Patient Window (a check box hidden in the
  designer keeps its row, shown at run time); fine.
- UserControlMainWindowMisc: groups fine; the rows outside any group use three field x values (Sync code, Update Server, Number of Audit
  Trail entries: rows parted by check boxes into separate stacks): open.
- UserControlApptGeneral: "Recall and Schedules" title clear (owner row 1); Appointment Behavior has two field x values (347, 390): open.
- UserControlApptAppearance: fine; the long Blockouts check box wraps to two lines (scroll bar room kept, the page is taller than the area).
- UserControlFamilyGeneral: fine (owner rows 15-18); the Super Family label column stands alone right of the boxes.
- UserControlFamilyInsurance: fine (owner row 19).
- UserControlAccountGeneral: the "DateTime the currently running aging started" box and its clear button run past the Functionality group's
  right edge; uneven pitch between the check boxes (rows drawn 31 px apart kept as "sections"): rule fixed (a gap is a section break only
  when clearly larger than the stack's usual gap), the box: open.
- UserControlAccountAdjustments: fine; "Automate Sales Tax" box on the field column.
- UserControlAccountClaimSend: fine in the groups; the two rows under the groups use their own field column.
- UserControlAccountClaimReceive: fine but Claims Medical rows 32 px apart: fixed by the same section rule.
- UserControlAccountPayments: fine; "Pay Plan charge logic" label without a visible field (its control is hidden in the designer).
- UserControlAccountRecAndRepCharges: rows 32 px apart: fixed by the section rule.

## Pass 3 (the final build: gallery G-UI3-ui12p's after pictures)

Every after picture was opened, except the seven preference pages whose picture is byte-identical to the one opened in pass 2
(UserControlAccountClaimReceive, -ClaimSend, -Payments, UserControlApptAppearance, -ApptGeneral, UserControlFamilyGeneral, -FamilyInsurance:
their pass 2 lines stand). The owner's check box crops are parts of the preference pages opened whole; the other crops were opened.
"Open" = found and not fixed in the pilot (listed for the rollout in UI-12.md section 4).

| Picture | Checked | Found |
|---|---|---|
| FormPatientEdit | rows, columns, groups, tab page | Good. The Age label and box sit inside the Family Relationships list's area (open, a known GridRulesTests row). Client area 1002 px tall (owner question 1). |
| FormProvEdit | rows, columns, groups | Good on the left; the check boxes on the right stand at three x values (387, 420, 433) with uneven gaps; the Term Date picker's own layout leaves a white strip (ODDatePicker). Open. |
| FormProcCodeEdit | rows, check boxes, lists, notes | Rows on one pitch; boxes on the field column with their notes after them. "Is Radiology" stays at the left (its row is parted from the others by the Paint Text pair); the "Time Pattern" caption reaches over the time bar. Open. |
| FormPractice | everything | Good: one field column per group, notes as tall as their lines, Save bottom right, the width kept. |
| FormInsBenefits | Categories and Ortho groups | Boxes no longer touch; one pitch. The Other Benefits grid over the Categories group is the designer place (Load moves it under the panel). Open as known. |
| FormApptViewEdit | rows, lists | Good. |
| FormMedLabEdit | groups, rows | Good; the Patient group's two columns line up. |
| FormScreenEdit | rows | Good rows; Race/Ethnicity and Urgency lists 16 px right of the field column. Open. |
| FormStatementOptions | groups, check boxes | Boxes on the left; the 2 x 2 groups of different heights again (the later rules changed the cut) and "Receipt" at the content edge while "Sent" is on the field column. Open. |
| FormBackup | groups, notes | Good; Backup and Restore boxes 4 px apart in x (two groups). |
| FormEmailInbox | the search bar, the owner's label | Good; "View messages for address:" whole. |
| FormHL7DefEdit | rows, groups, notes | Good; the HL7 options fields at two x values (440, 449); radio texts far from their circles (stock radio buttons, as drawn). Open. |
| FormApptEdit, FormInsPlan, ControlChart (hand) | their hand rows in the crops and style pictures | The quick-add note has its three lines; the ortho check box on one line; the procedure buttons on their grid. The rest as on main. |
| FormClaimEdit, FormProcEdit, FormPayment, ControlAccount (hand) | doubled drawing | No doubled text or outlines any more; as on main otherwise. |
| ControlChart-procbuttons 100/150 | sizes, gaps, centring | Equal buttons, equal gaps, the surface box over the group. |
| ControlChart-pagebar 100/150 | the bar | Chevrons, page numbers as flat buttons, page 2 in the Accent fill; centred. |
| FormPayment-inputs 100/150 | the input frames | Every text box, combo box, up-down box and date picker has the same frame and corners; the date pickers have the combo box's chevron. At 150% the up-down boxes' text is cut at the bottom (the harness draws their inner box with the scaled font in an unscaled height). Open. |
| UserControlMainWindow | page | Good. |
| UserControlMainWindowMisc | page | The rows outside the groups at three field x values. Open. |
| UserControlAccountGeneral | page | Aging row inside its group now (fixed in pass 2's rules); fine. |
| UserControlAccountAdjustments | page | Good. |
| UserControlAccountRecAndRepCharges | page | One pitch now (fixed). |
| UserControlTreatPlanGeneral | page | The "Sort procedures by" radio buttons still have their circle on the right (they are positioned by the page's code). Open. |
| UserControlTreatPlanFreqLimit | page | Good. |
| UserControlChartGeneral | page | The ICD-10 field and its button run 1 px past the Medical Codes group. Open. |
| UserControlChartProcedures | page | Good. |
| UserControlImagingGeneral | page | The Imports group's fields at four x values (two columns of rows the cut keeps apart). Open. |
| UserControlManageGeneral | page | Good. |
| UserControlManageBillingStatements | page | Good. |
| UserControlOrtho | page | Good; the Ortho Placement group's list and buttons as drawn. |
| UserControlEnterpriseGeneral | page | Good; the Global Lock group does not span the page (it stands in a column of its own). |
| UserControlEnterpriseAccount | page | "Receive at an interval" and "Receive at a set time" radio buttons at two x values; the Claim Identification Prefix group floats right. Open. |
| UserControlEnterpriseAppts, -Family, -Manage | page | Good (EnterpriseManage's check boxes are on the left now: their Name differs from their field, which the planner reads now). |
| UserControlEnterpriseReports, UserControlServerConnections | page | Good layout; their stock ComboBoxes (Database, Server Name) still draw the Windows drop button: `UiInputFrame` covers text boxes, up-down boxes and date pickers, not the stock combo box. Open. |
| UserControlExperimentalPrefs | page | Good. |
| Owner crops | the 26 crops | Each control whole and outlined: labels whole, notes on their lines, boxes on the left. |

The open items are for the rollout's first slice to fold into the planner or fix by hand; none hides a control or cuts the text of an
owner's row.

## Round 2 (2026-11-21: the owner's answers, the lead's audit)

Every round 2 picture was opened: the windows whole, the Preference pages in montages of four to six whole pages at full size, the owner's
26 crops in two montages, the style pictures with crops of the inputs at 150%. Small details were checked in 2x crops (`work2/crop.py`).

### Pass 1 (gallery r2a and r2b: the new rules from main's tree)

- UserControlApptGeneral: the Preferences style works (labels at the check box text, one field column, combo boxes to the right edge, long
  labels on two lines). Fine.
- FormPatientEdit: columns shared across the groups; the band under Employer is gone; found: the right column's groups of three widths
  (rule added: group boxes stacked in one column share one right edge), an empty row at the top of Address and Phone (the hidden "Same for
  entire super family" box, two lines tall: kept, it is that box's room when it shows), the Age box (fixed: relative move), the date
  pickers' stray line (fixed in the gallery: the hidden calendar).
- FormApptEdit: fields narrowed before the panel grew (rule added: a field narrowed in one pass widens again), the check boxes on a minor
  column (rule: the dominant field column), Lab and Req (rule: a button before its box stands where a label would), the fields' right edges
  ragged (rule: in a part laid out alone the fields end at one right edge), the quick-add note's third line cut (scope and hand row), the
  appointment note narrower than the panel and list above it (hand row).
- FormProcCodeEdit: Is Radiology in the column; the empty area below Assign To Prov (rule: hidden rows go to the end); the fee notes
  ragged (rule: notes and their buttons centred on their column).
- FormStatementOptions: Receipt off the column (rule: stacked check boxes are a check box), Save on its own line (rule: the bar's buttons on
  one line), the note boxes past the groups (rule: a wide box keeps its right edge).
- FormHL7DefEdit: the Messages grid short of the groups' edge (rule: a wide grid reaches its column's edge); Warn if Procs off its list
  (hand row); the bottom buttons on two lines (rule).
- FormBackup: the Backup controls 3-11 px off the Restore group's edges (hand rows; they stand over the group, not in it).
- FormInsBenefits: the benefit grid over the Categories group (the harness does not run Load; at run time the grid went under the
  simplified panel, which ended 37 px above the group: hand row, the panel as tall as the group).
- FormPayment (laid out in parts): Hide Splits on the Note label (hand row).
- UserControlMainWindowMisc, UserControlServerConnections: fine.

### Pass 2 (gallery r2c and r2d)

- UserControlEnterpriseAccount: the "Receive at an interval" radio button's text covered by the "minutes (5 to 60)" label (rule: a
  Preferences label after another control on its row follows it); the Claim Identification Prefix group in the right half with a blank
  half beside it (rule: a Preferences group box alone on its rows spans the page); the pay plan note right-aligned at the far right (hand
  rows, also on UserControlAccountPayments).
- UserControlAccountClaimReceive: Claims Medical rows 34 px apart (rule: a one-line check box keeps a one-line row).
- UserControlTreatPlanGeneral: the Default Note box past the page's right edge (rule: Preferences note boxes end at the content's edge).
- FormPayment: the two filter groups of two heights (rule: scoped group boxes side by side get one height); the tab controls under them
  overlapped the left group by 3 px and stood 5 px apart (hand rows); the buttons and totals under the grids on four lines (hand rows); the
  totals' labels touching their boxes (hand rows).
- Pages UserControlMainWindow, -ApptAppearance, -FamilyGeneral, -FamilyInsurance, -AccountGeneral, -AccountAdjustments, -AccountClaimSend,
  -AccountPayments, -AccountRecAndRepCharges, -TreatPlanFreqLimit, -ChartGeneral, -ChartProcedures, -ImagingGeneral, -ManageGeneral,
  -ManageBillingStatements, -Ortho, -EnterpriseGeneral, -EnterpriseAppts, -EnterpriseFamily, -EnterpriseManage, -EnterpriseReports,
  -ExperimentalPrefs: one field column per page, boxes left, groups spanning, no cut text, no overlap.

### Pass 3 (gallery r2f, the final build; G-UI3-ui12p's after pictures)

| Picture | Checked | Found |
|---|---|---|
| FormPatientEdit | rows, columns, groups, tab page | Good. Accepted: the empty row under "Same for entire family" in Address and Phone is the hidden super family box's place. |
| FormProvEdit | rows, columns, groups | Good: one field column on the left (also in Clinic Overrides), the check boxes on the right at one x. |
| FormProcCodeEdit | rows, check boxes, lists, notes | Good but one nit open: the row of Is Radiology is 8 px further from Is Prosthesis than the other check box rows (the Paint Text field on that row stands under the Category list, and the row goes under the list's bottom). |
| FormPractice | everything | Good. Nit open: the City/ST/Zip rows end 10 px right of the Address fields; the right column's Default Provider combo, Billing Provider group and Switzerland group end at three x values (as drawn). |
| FormInsBenefits | groups, rows | Good on the grid; the benefit grid drawn over the Categories group is the harness (Load moves it; at run time it now goes under the group). |
| FormApptViewEdit, FormMedLabEdit | groups, rows | Good. |
| FormScreenEdit | rows, lists | Good. |
| FormStatementOptions | groups, check boxes, notes | Good. |
| FormBackup | groups | Good. |
| FormEmailInbox | search bar, the owner's label | Good; the preview pane's labels touch their boxes (EmailPreviewControl, another designer: its own slice, a known grid finding). |
| FormHL7DefEdit | rows, groups, grid, bar | Good. |
| FormApptEdit | the fields panel, the list, the note | Good (the owner's row 25). Accepted: the band under Req is the hidden Syndromic Observations row's place. |
| FormPayment | parts, groups, tab controls, bar | Good. |
| FormInsPlan, FormClaimEdit, FormProcEdit, ControlAccount, ControlChart | as on main but their hand rows | **Not laid out (open, UI-12.md section 5).** |
| Preference pages (28) | whole pages in montages | Good: the Preferences style everywhere. Nits open: UserControlAccountAdjustments' Bad Debt list stands right of the page column; UserControlChartProcedures has an extra gap above the last check box (a hidden row's place); the stock radio buttons' text stands 3 px further from the circle than the check boxes' text. |
| Owner crops (26) | each crop | Each control whole, boxes left, labels whole. |
| FormPayment-inputs 100/150 | frames, text | Every input with the combo box's frame; the up-down boxes' text whole at 150% (fixed). |
| UserControlServerConnections (crop) | the stock combo boxes | The program's frame and chevron (fixed). |
| ControlChart-procbuttons, -pagebar 100/150 | sizes, gaps | As in round 1: equal buttons and gaps; the page bar flat with chevrons. |

The pass still finds the open items above, so the audit is not clean: round 2 is reported partial.

## Round 3 (2026-11-21: the lead's audit of round 2, the owner's addition, the five windows)

Every round 3 picture was opened: the forms whole, the Preference pages in ten montages of three whole pages at full size, the owner's 26
crops in two montages, the style pictures (the procedure buttons and the page bar at 100% and 150% in one montage, FormPayment's inputs at
150% whole), and the parts the gallery cannot show (FormClaimEdit's UB04 and Canadian tabs) drawn from the tree with `render.py`.

### Pass 1 (galleries r3a and r3b: the round 3 rules, the five windows' first layouts)

- FormInsPlan: laid out (three columns, the patient band); found: the Note label centred on its box (moved to the box's top), the
  subscriber and Canadian groups on two field columns (one now), the patient band's fields 6 px left of the plan column (on it now).
- FormProcEdit: laid out; found: Original Date Comp's label over the Date Entry box (the left column 432 wide now), the Financial tab's grids
  past the page's edge (8 px from the edges now), the left column 6 px off the tabs' field column (on it now).
- FormClaimEdit: laid out; found: the window 758 px tall (the script took the planned tree's size as the designer's; fixed in grid_hand),
  the General tab's claim note past the tab (8 px from its edge now).
- ControlChart: the Enter Treatment page in four columns; found: "Type Proc Code" cut in its box, "Procedure Buttons:" 0 px wider than its
  text, the ExstCurProv radio button on the entry status group's edge (the columns re-balanced: 96, 122, 112, 168 px).
- ControlAccount: the aging strip and the side panel; found: the bold amounts 8 px wider than their columns (80 px columns for them now).
- FormPatientEdit: the four lists, Add and Defaults, the Zip box fixed; found: the band under "Same for entire family" still there (the
  gap under the row was measured to the row without its hidden box: fixed in the planner).

### Pass 2 (galleries r3c to r3f: GridRulesTests in every window, the owner's addition)

- The new checks (text past its container, equal gaps) found: FormInsPlan's check list rows of two heights (one height now), the
  Benefits Last Verified label over the patient band (the band ends before it), the top right's dates and the subscriber's To date at three
  x values within 40 px (one column now); ControlChart's entry status radio button on the group's edge (fixed above); the zero-size tooltip
  label of UserControlAccountRecAndRepCharges (not a text: the check skips it).
- Preference pages: UserControlAccountClaimSend's and UserControlEnterpriseAccount's Claim Identification Prefix boxes off the page's field
  column, the receive time box after its radio button's text, UserControlOrtho's placement list off the column with its buttons centred,
  UserControlAccountGeneral's allocation note right-aligned at the far right: hand rows (the re-run stays empty).
- Known findings with their reasons (`grid-known.tsv`): the parts kept as on main (ControlChart's Missing, Movements, Show and Ortho pages,
  ControlAccount's hidden summary groups and Auto Ortho page, what the Chart layout sheet places), another designer's controls
  (EmailPreviewControl, SignatureBoxWrapper), rows a two-line label makes taller, stacked switched controls, radio buttons in field rows.

### Pass 3 (gallery r3i, the final build; G-UI3-ui12p's after pictures)

| Picture | Checked | Found |
|---|---|---|
| FormPatientEdit | rows, columns, groups, lists, the Zip box | Good: no band under "Same for entire family" (the hidden super family box at the end of its group), the Zip box one box with its chevron, the four lists one top and height with Add and Defaults under the last one's edge. |
| FormProvEdit | rows, columns, groups, radio buttons | Good. |
| FormProcCodeEdit | rows, check boxes, lists, notes | Good: Is Radiology 1 px off the list's pitch (its row holds the Paint Text field; a known gap). |
| FormPractice | everything | Good: City, ST, Zip end with the Address fields; the right column one left and one right edge; the radio buttons drawn like check boxes. |
| FormInsBenefits, FormApptViewEdit, FormMedLabEdit, FormScreenEdit, FormBackup, FormStatementOptions, FormHL7DefEdit | groups, rows | Good (FormInsBenefits' benefit grid is drawn where the designer has it: Load moves it). |
| FormEmailInbox | search bar, the owner's label | Good; the preview pane is another designer (known). |
| FormApptEdit, FormPayment | the parts, the inputs | Good. |
| FormInsPlan | the band, three columns, the top right, the bottom bar | Good: one field column per column, the subscriber and Canadian groups on one, the top right's dates on the To-date column, check list one pitch, the radio buttons at the bottom drawn like check boxes. |
| FormProcEdit | panel1, the right column, the tabs | Good. The tooth, surface and area rows are empty in the picture (hidden in the designer; FillControls shows the treatment area's ones). |
| FormClaimEdit | the bands, the groups, the grids, the tabs (General drawn; UB04 and Canadian from the tree) | Good. The harness draws the bottom bar 21 px lower than the designer puts it (FormClaimEdit computes its client area in Load, which the harness does not run; on main the same): the bar is cut at the picture's bottom edge. |
| ControlChart | the Enter Treatment page, the procedure buttons, the Planned button row | Good: four columns 6 px apart, captions and lists on one line, the bottom row on one line, "Type Proc Code" whole. |
| ControlAccount | the aging strip, the side panel | Good: the aging boxes 4 px apart under centred captions, the amount columns even, the side panel's buttons one width. |
| Preference pages (28) | whole pages in montages | Good: the Preferences style everywhere, the prefix boxes, the receive time box and the placement list on their pages' field columns, the allocation note at the content's edge, the procedures page's last check box on one line. |
| Owner crops (26) | each crop | Each control whole and outlined, boxes left, labels whole. |
| FormPayment-inputs 100/150, ControlChart-procbuttons and -pagebar 100/150 | frames, sizes, gaps | Good: one frame for every input, the procedure buttons 30 x 19 px 3 px apart, the page bar flat. |

The pass finds nothing beyond the known rows (each with its reason in `grid-known.tsv`) and the harness's FormClaimEdit bottom bar.

## Round 4 (2026-11-21: the fix round, the lead's 27 review findings)

The findings and what changed: UI-12.md section 4, "Round 4". Each pass opened every picture named (whole pictures; the preference pages
in montages of four at full size; the owner crops in two montages; the 150% inputs also measured from the picture's pixels).

### Pass 1 (gallery r4a, the first round 4 build)

| Picture | Checked | Found |
|---|---|---|
| FormInsBenefits (owner rows 24, 35) | the benefit grid, Categories' columns, the notes row, the frequency grid | Grid under the simplified panel, no overlap; columns on 298/368; Edit under Delete. The Notes label centred on the 98 px box instead of on its first line (fixed: 23 px label). |
| FormClaimEdit (owner row 02) | the bottom bar, the right margin, General's groups, Claim Referral | The bottom bar and the 8 px margin are in the picture now; Crown and Ortho one height; the referral fields on the buttons' edge. Good. |
| FormPayment | the splits panel's edges, the row under the grids, the clinic picker | Good: 8 / 1103 edges, one gutter, one row height. |
| FormPayment-hidesplits-100/150 | the hidden state | Not drawn: the toggle reads `Visible`, false in a window never shown (fixed: `SetSplitsShown`, which the harness calls). |
| FormPayment-inputs-150 | input heights per row (pixels) | Combo 25-26 px, text boxes 27, up-down and date picker 30-31: not one height yet (fixed in pass 3). |
| FormPatientEdit | the left column's edge, the Address group, Referred From, the clinic picker | One right edge (480), the Address group ends under Zip, Referred From 23 px framed, Clinic label 6 px before its box. The right column's tab control ended 24 px higher than before (fixed: it keeps its bottom). |
| FormEmailInbox | the three groups, captions, toolbar | Drawn maximized at the screen's size after the harness created its handles (fixed: the harness draws a window at its designer size). |
| FormScreenEdit | Age, Gender | The age arrows were behind the Age box (z-order; fixed with the applier's `AddBefore`); the radios on the field column. |
| FormApptEdit (owner row 25) | the fields panel's pitch | Every row 23 px at a 27 px pitch, the clinic picker too. Good. |
| FormInsPlan (owner row 36) | the ortho check box | On the field column, two lines to the group's edge. Good. |
| ControlAccount | the side panel's Main page | 4 px inset, the check box 8 px in. Good. |

GridRulesTests (r4a) listed the new `hidden-frame` rule's findings: hidden scroll bars of lists (the rule now looks at layout containers
and named controls only), the two groups grown by code (known rows), FormPayment's Treat Plan warning 1 px past its narrower page and
the From/To label 5 px left of the group's padding (fixed), FormPractice's notes past their narrower groups (fixed), FormScreenEdit's
arrows over the Age box (known: one box), the wrapped labels that make a Preferences row taller (known), the time bar's hidden head panel
and the Chart's hidden button (known: other designers / kept as on main).

### Pass 2 (gallery r4b and r4c)

| Picture | Checked | Found |
|---|---|---|
| FormPayment-hidesplits-100, -150 | the hidden state | The bottom row 15 px (22 at 150%) under the note and Show Splits, nothing covered. Good. |
| FormScreenEdit | the age box | The arrows inside the box's right end; moved 1 px in from the frame. Good after that. |
| FormEmailInbox | groups, captions, toolbar | One height (149), captions on their fields, Search/Clear on the fields' edge, four message buttons one width. "Subject/Body" was cut by the Date To caption's bounds (fixed: the date captions as wide as their boxes). |
| FormMedLabEdit | the bottom bar | Notes beside their buttons; a 55 px band over the bar (fixed: the facilities grid down to the bar). |
| FormStatementOptions | the content box | 8..712 for groups, notes and the bar. Good. |
| FormPractice | group padding | 8 px in every group, the notes inside. Good. |
| FormBackup | the Backup and Restore buttons | On the content edge. Good. |
| FormApptViewEdit | Rows Displayed, the clinic picker | The group to 1084 with even padding. The picker's own Clinic label was under Show mirrored appointments (fixed: the picker after the check box's text; GridRulesTests now counts the picker's label in its ink). |
| FormProvEdit | the left column's edges | One edge; Edit on the fields' edge. Good. |
| FormProcEdit, FormProcCodeEdit | panel1's second band, the time fields | Time Start on the band; End and Now inside panel1 (hidden in the designer). Good. |
| Preference pages (28, seven montages) | the 50% column, wrapped labels, notes, groups | Field column at 256 on every page that has one; labels that pass half the page wrap; the Global Lock and Select Patient groups end 8 px under their last row; the replication and Insurance History notes end 8 px over their frames. UserControlEnterpriseAccount's interval unit stood before its box (fixed: after it). |
| ControlChart, FormHL7DefEdit | unchanged parts | As round 3. Good. |
| Owner crops (26, two montages) | each crop | Each control whole, boxes left, labels whole, nothing covered. |

### Pass 3 (gallery r4e, the final build; G-UI3-ui12p's after pictures)

| Picture | Checked | Found |
|---|---|---|
| FormPayment-inputs-100, -150 | input heights per row (pixels) | 100%: every input 23 px (the combo box's frame 1 px inside its bounds, as approved). 150%: text boxes 30, up-down boxes 30, date pickers 31, combo boxes 28 (their frame 1-2 px inside their 31 px bounds): one height per row within the combo frame's inset (was 34 / 31 / 23). |
| All 47 windows, the 38 owner rows, the style pictures | changes since pass 2 | Only the designer changes of pass 2's fixes and the 150% input heights; each looked at again. Good. |

The pass finds nothing beyond the known rows (each with its reason in `grid-known.tsv`) and the 123 re-run rows of the hand-row windows
(UI-12.md section 5).

## Round 5 (2026-11-22: the second fix round, the lead's 13 review findings of round 4)

The findings and what changed: UI-12.md section 4, "Round 5". Pictures from the build of each pass (`G-UI3-ui12p-src/r5a` to `r5d`); a
picture byte-identical to round 4's final gallery (`r4e`) was audited there (round 4, pass 3) and is listed as such.

### Pass 1 (gallery r5a and r5b)

| Picture | Checked | Found |
|---|---|---|
| FormPayment (owner row 28), -inputs-100, -inputs-150, -hidesplits-100, -hidesplits-150 | captions over their boxes, the second filter row's columns, Invoice's edge, 150% heights | Every caption on its box's left edge, the dates and Proc Codes/Refresh on the first row's columns, Invoice to Refresh's edge, the splits filter's Refresh on Amt Max; the same at 150%; the hidden state unchanged. Good. |
| FormInsPlan | the adjustments list, the top-right gaps, the verified row | r5a: list 340 px and the request group at 1043 put its date 11 px off the subscriber's To-date column (GridRulesTests `column`); fixed: the request group back on the To-date column, the list to 8 px before it (329 px, the sample line whole with room to spare), import 8 px after, the verified date clear of the column. r5b good. |
| FormApptViewEdit (owner row 33) | the grids' bottoms, Up/Down, the left margin | One bottom for both grids, Up/Down 8 px under them and 8 px over the group's edge, the left column at 8, the bottom bar 8 px under the content. Good. |
| FormHL7DefEdit (owner row 34, crop label17) | content box, grid, Browse padding, rows under the group | 8..884 for every edge, the grid to 8 px over the bar, the Browse buttons on the group's content edge, one pitch under the group, the notes centred on their rows. Good. |
| FormProcCodeEdit (owner row 23) | the top-right lists, Paint Text, the check box pitch, the fee notes | One top (25) and one bottom (380), Paint Text in the Paint Type column, the six boxes 24 px apart, the notes 4 px under the grid. Good. |
| FormInsBenefits (owner rows 24, 35, crop label17) | Benefit Year's gap, the left column's right edge | 8 px to Categories; the grid, the Ortho group and the Family column end on 291. Good. |
| FormEmailInbox (owner row 30) | the three groups' gutters, the window's edges | 8 and 8 px. The left edge 12 against the groups' 8 px right margin, the tab control and Refresh 10-14 px short of the groups' edge (fixed in pass 2: one content box). |
| FormMedLabEdit (owner row 37) | the gaps under Patient and Ordering Physician, the grids | 8 px each, the grids 8 px apart, the bar in place. GridRulesTests `column`: the Patient group moved 4 px left and its second field column (429) no longer met the Date/Time column (433) (fixed: the Date/Time column on 429). |
| UserControlEnterpriseGeneral | Patient Select's first label | 32 px, centred on its box, one pitch; the check boxes under the group 8 px under it. Good. |
| UserControlMainWindowMisc | the gaps between the groups and fields | r5a: the measured wrap read the trailing line end of two labels as a third line (GridRulesTests `gap`; fixed: a line end after the text draws nothing). r5b: 8 px between the groups, 4 px between the fields. Good. |
| UserControlManageGeneral, UserControlAccountClaimSend | hyphen breaks | "Auto-processing posts write-offs on / non-primary claims", "PPO write-off description (blank for / "Write-off")". Good. |
| UserControlAccountGeneral | the Future Dated note, the aging label | The note left-aligned at the check box text, the aging label's third line ("Usually blank") shown, the allocation note 8 px under the group. Good. |
| UserControlApptGeneral | the provider change row | One line, one pitch. Good. |
| FormApptEdit, FormPatientEdit, FormProcEdit, FormClaimEdit (the clinic picker) | the picker label's ink against its neighbours (pixels) | FormApptEdit: ends at 423 against 422, 1 px lower than Provider's; FormPatientEdit: 178 against 177, 1 px lower; within the 1 px asked (it was 2 px right and 2 px low). The boxes stand on one x (431). Good. |
| FormPatientEdit | the Public Health tab, the window's bottom | The tab's last row at the pitch (the 12 px gap closed), the window 8 px shorter, the tab control's bottom 34 px over Save as before. Good. |
| FormPractice | the bottom bar | 4 px higher, 8 px under the content. Good. |

### Pass 2 (galleries r5c and r5d, the final build; G-UI3-ui12p's after pictures)

| Picture | Checked | Found |
|---|---|---|
| FormEmailInbox | the content box | r5c: the hand rows took the drawn window's values for the bottom-anchored tab control and Delete (9 px less than the designer's: the menu), which moved Delete 9 px up (fixed). r5d: one content box 8..1227, Delete 8 px over the bottom. Good. |
| FormInsPlan, FormMedLabEdit, UserControlMainWindowMisc | the pass 1 fixes | As in pass 1's last build (r5b): good. |
| The other 21 pictures that changed since round 4 | changes since pass 1 | Byte-identical to r5b; audited in pass 1. |
| ControlAccount, ControlChart, ControlChart-pagebar-100/150, ControlChart-procbuttons-100/150, FormBackup, FormProvEdit, FormScreenEdit, FormStatementOptions and 22 preference pages; 24 of the 26 owner crops | unchanged | Byte-identical to round 4's final pictures (r4e), audited in round 4, pass 3. |

GridRulesTests (r5d): no findings beyond `grid-known.tsv` (two rows removed this round: FormProcCodeEdit's Is Radiology gap and
FormHL7DefEdit's server row gap no longer happen). The pass finds nothing new.

Open, outside this round's findings (seen while auditing, for the lead): FormPractice's groups stand at x = 19 (labels outside them start at 8) against the
bottom bar's 8 px (their fields share the window's field column at 132); FormProcCodeEdit's tab control starts at 75 (the label column's
edge) and leaves an empty corner at the bottom right; both as in round 4.

## Round 6 (2026-11-22: the polish round, the lead's non-blocking list of round 5)

The findings and what changed: UI-12.md section 4, "Round 6". Pictures from the build of each pass (`G-UI3-ui12p-src/r6a`, `r6b`). Every
picture was opened in each pass: the full windows one by one, the 28 preference pages three to a sheet at full size, the 26 owner crops on
three sheets, the style pictures at 100% and 150% whole and zoomed (the up-down buttons and the multi-line boxes at 4x).

### Pass 1 (gallery r6a, the first round 6 build)

| Picture | Checked | Found |
|---|---|---|
| FormPayment-inputs-100, -150, -hidesplits-100, -150 | the up-down buttons, the Note box, one look per input | The arrows light with the combo box's chevrons, the Note box framed like the text boxes. At 150% the chevrons stayed at their 100% size (the harness's windows are at 96 dpi; fixed: they follow the box's height too). |
| FormProvEdit | the Specialty list, the right column's edges and gutter | Every specialty whole, one right edge (760), 8 px gutter. Not a Person's text ran 87 px past the column's edge (fixed: it wraps at 760). |
| FormProcCodeEdit (owner row 23) | the Treatment Area list, the tab control, the fee notes | Tooth Range in view but the list still scrolled (138 px held 136 px of rows and the border: fixed, 140 px); the tab control to 1145, no empty corner, the notes 8 px over it; Description on the Only if combo's edge (the planner's new rule). |
| FormEmailInbox (owner row 30) | the preview's label gaps, Add Image | 6 px everywhere, the labels centred on their fields, Add Image beside Add File; the body box framed. Good. |
| FormPatientEdit | Credit Type, the multi-line boxes | 24 px; the Address and Phone Notes box framed. Good. |
| FormScreenEdit (owner row 26) | Grade Level, the age arrows | On Name's edge; the arrows light inside the Age box. Good. |
| FormClaimEdit (owner row 02) | the top row's right edge | 1160 for the plan groups' column and both payment groups, Split Claim under Enter Payment's content. Good. |
| UserControlAccountRecAndRepCharges (owner row 08's page) | Edit Template | On the combo's edge. Good. |
| FormMedLabEdit (owner row 37) | the Patient group, Fasting, the right column | Fasting inside the column, the group 8 px past its content, the right column from 8; the three note boxes framed. Good. |
| FormPractice (owner rows 22, 38) | the groups' x, the right column | The groups at 8, the fields still on 132. The right column 14 px from the window's right edge against Save's 8 and 6-7 px after its lists (fixed: 8 px gutter, to 883). |
| UserControlTreatPlanGeneral | the Default Note row | The label centred on its 66 px box while the ordinary forms' labels stand on a box's first line (fixed: the planner's Preferences rule). |
| FormInsPlan, FormApptEdit (owner row 25), FormProcEdit, FormHL7DefEdit (owner row 34), FormStatementOptions (owner row 29), FormInsBenefits (owner row 24), ControlChart (owner row 31), ControlAccount, ControlChart-pagebar-100 | the multi-line boxes, nothing else changed | Every multi-line box framed like the inputs (the notes, the Lab and Req boxes, the claim and E-claim notes, the treatment note), the scroll bars inside the frame; the rest as audited in round 5. Good. |

### Pass 2 (gallery r6b, the final build; G-UI3-ui12p's after pictures)

| Picture | Checked | Found |
|---|---|---|
| FormProvEdit | the wrapped Not a Person, the rows under it | Two lines ending inside 760, the list pitch kept, Default Claim Form 16 px lower. Good. |
| FormProcCodeEdit, 23-FormProcCodeEdit | the Treatment Area list | Eight items, no scroll bar, & Tooth Range 4 px under it. Good. |
| FormPractice, 22-FormPractice, 38-FormPractice-label18 | the right column | One gutter (8), one right edge with Save (883), the Switzerland fields to its content edge. Good. |
| UserControlTreatPlanGeneral | Default Note | On the box's first line. Good. |
| FormPayment-inputs-150, FormPayment-hidesplits-150 | the up-down chevrons at 150% | Larger with the box, inside the button area. Good. The date pickers' text larger than the labels' is the harness's (section 5, round 4). |
| FormPayment, FormPayment-inputs-100, FormPayment-hidesplits-100, 28-FormPayment | the inputs | One frame, height and chevron look for the combo boxes, text boxes, date pickers and up-down boxes; the Note box framed. Good. |
| ControlChart-pagebar-100, -150, ControlChart-procbuttons-100, -150 | the page bar, the procedure buttons | As approved in round 4 (the page bar's left edge shows the framed note box's corner). Good. |
| FormApptViewEdit (owner row 33), FormBackup (owner row 27), 33-FormApptViewEdit-label5 | unchanged since round 5 | Opened again: the grids' bottoms, the content box, the Browse buttons on the content edge. Good. |
| FormInsPlan, FormApptEdit, FormProcEdit, FormHL7DefEdit, FormStatementOptions, FormInsBenefits, FormClaimEdit, FormEmailInbox, FormPatientEdit, FormScreenEdit, FormMedLabEdit, ControlChart, ControlAccount and their owner rows (02, 24-26, 29-31, 34-37) | unchanged since pass 1 (byte-identical to r6a) | Opened whole in pass 1 (above). Good. |
| The other 26 preference pages (UserControlAccountAdjustments to UserControlTreatPlanFreqLimit) | unchanged since round 5 | Opened on the page sheets at full size: one field column per page, labels where the check box text starts, the groups 8 px apart, no text cut or covered. Good. |
| The 26 owner crops (rows 03-21, 32-38) | each crop | No text cut, every box on the left, the labels 6 px before their fields; FormHL7DefEdit-label17 and FormPractice-label18 changed only by the framed boxes and the groups' new x. Good. |

GridRulesTests (r6b): no findings beyond `grid-known.tsv` (nine rows removed this round: the preview pane's label gaps no longer happen). The
pass finds nothing new.

Seen and kept, for the lead (not in this round's list, as audited before): FormApptEdit's grids and buttons on the right stand 1 px from the
window's right edge as on main (the window is laid out in parts, `grid-scope.tsv`, and they are outside them); FormProvEdit's hidden Allow
NewCrop eRx Option box ends at 787 past the column's 760 when the code shows it (it stands 6 px after Use Electronic Prescriptions, as on main).

## Round 7 (2026-11-22: the fix round of round 6, the lead's finding on the single-line ODtextBoxes)

### Pass 1 (the first round 7 build, gallery r7a)

GridRulesTests on the build found two new gaps in the message preview (FormEmailInbox): Username to Sent/Received 2 px and Date / Time to
From 1 px, against the column's 4 px once the address rows were on the 27 px pitch. Fixed (the three display lines 4 px apart and 4 px over
From); the pictures were redrawn before they were audited.

### Pass 2 (gallery r7b, the final build; G-UI3-ui12p's after pictures)

| Picture | Checked | Found |
|---|---|---|
| FormEmailInbox, 30-FormEmailInbox (owner row 30) | the preview pane's address rows, labels, buttons, the body box, the Attachments tab control | From, To, CC, BCC, Signed By (read-only fill) and Subject framed like the Search group's boxes and the body box, 4 px apart, one left edge (89) and one right edge 8 px before the Attachments tab control; the labels 6 px before their fields and centred on them; `...` and Sig 6 px after their fields at the field height; Add Image and Add File on the fields' edge, 8 px apart; the Username, Sent/Received and Date / Time lines 4 px apart; the body box 4 px under Subject. Good. |
| The other 92 pictures | byte compare against round 6's (r6b) | Identical (ControlChart's search box stands on the Show page, which its picture does not show). Audited in round 6, pass 2. |

GridRulesTests (r7b and the final run): no findings beyond `grid-known.tsv` (the Sent/Received to Date / Time row removed), no `unframed`
text box in any grid window. The pass finds nothing new.

Seen and kept, for the lead (as since round 4, not this round's finding): the gallery's framed inputs that print themselves (rich text
boxes, up-down boxes, date pickers) keep the Windows border's dark pixel in each outer corner under the rounded frame (UI-12.md section 5,
round 7).
