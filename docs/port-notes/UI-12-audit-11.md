# UI-12 slice 11: audit of the gallery

Slice 11 (`ui/ui12-11-1`, [UI-12.md](UI-12.md) section 7, "Slice 11"): the large windows and the windows earlier slices skipped. Every
after picture opened at 100% (the program's font, Segoe UI 9), the densest windows at 150%. One line per picture: what was checked (cut or
covered text, overlaps, margins, columns, gaps, crowding, empty bands, control looks, group titles) and what was found. A window back to
`main` (section 7's skipped list) has no after picture of its own.

## Pass 1 (the planner's layout, `<W>/after-100` of the first after build)

| Picture | Status | What was checked and found |
|---|---|---|
| FormApptPrintSetup | fixed | Color and Orientation groups at x 53 (no column); Save floating over the button row; Preview and the routing buttons off the content edge. |
| FormAutoNoteControlEdit | fixed | Possible responses caption centred on its 293 px box; the arrow buttons 79 px short of the right padding. |
| FormConfirmationSetup | fixed | Grid at 12; the status rows 21 px tall, captions 2 px from the boxes; Group Families floating; Save 27 px tall. |
| FormCvxs | fixed | Grid at x 20 and 82 px short of the right padding. |
| FormDiscountPlanEdit | fixed | Wide fields on three right edges (394, 391, 392); List Patients 21 px tall. |
| FormDoseSpotPropertyEdit | ok | Rows, the Show key check box on the field column, the buttons after their boxes, Save on the bar: nothing found. |
| FormEduResourceEdit | fixed | The Lab Results group's fields on x 124 against the window's field column at 175. |
| FormEhrLabImageEdit | ok | The splitter (now UI.SplitContainer) at the padding, the grid and the preview 8 px inside their panels, Save on the bar: nothing found. |
| FormEquipmentEdit | fixed | Status ended 11 px short of Description and Location. |
| FormFeeSchedTools | fixed | The group combos at x -160; the clinic pickers' boxes 8 px off the column; the tab pages' groups at six widths; only the Copy page drawn. |
| FormMountItemDefEdit | fixed | The text box 8 px left of the field column, it and the camera guide box short of the right padding. |
| FormPatListElementEditEHR2014 | fixed | Restriction and Operand cut their items (Medicatio, GreaterTha) and their last rows. |
| FormPaySimpleSetup | ok | Rows on one column, the note centred over the clinic row, the check boxes on the field column, Save on the bar: nothing found. |
| FormProcCodeNew | fixed | The Type list at 15 under its caption at 8; Add and Add, then another 93 px past the field column's edge. |
| FormProcCodes | fixed | The window grown from 1014 x 696 to 1150 x 900; the Fee groups 348 wide; the group combos beside the fee schedule combos; the buttons' group 113 px under the search group. |
| FormProcTPEdit | ok | One label column, one field column, the description box on it, Delete and Save on the bar: nothing found. |
| FormProgressExtended | back to main | A plain CodeBase window the code fills with progress bars at run time: the planner's picture showed an empty band. |
| FormProgressStatus | back to main | The code grows it by 60 x 120 at run time. |
| FormQuickBooksOnlineSetup | fixed | The Status caption off the label column, its text 340 px away under the buttons. |
| FormReportsMore | fixed | The first column 1 px off the padding, column gutters 30 and 9 px. |
| FormSheetFieldComboBox | fixed | Pre-set options cut its fifth row over a 120 px band; Tab Order 2 px off its column; Remove's icon touching its text. |
| FormSheetFieldImage | ok | Rows, the picture on the field column, File Size group and the check box beside it, the bar: nothing found. |
| FormSheetFieldInput | fixed | The Auto Note buttons one row up, past the column; the list at 13. |
| FormSheetFieldStatic | fixed | The text box at 18, a 19 px gutter to the list, the list 3 px short of the right padding. |
| FormTestLatency | fixed | The first group at 12, the groups ending 26 px short, the Speed note cut (GridRulesTests `text`). |
| FormUpdate | back to main | Its panels are shown by the code one at a time: the picture showed one caption and an empty window. |
| FormUpdateSetup | fixed | The Web Proxy group at 69, the Recopy button 39 px under its note, wide boxes 50 px short. |
| FormWebConfigSettings | back to main | ServiceManager's plain window: its code sets the height (375) for the replication state; the Is one way replication text cut. |
| FormXDRSetup | fixed | The Clinic / Location ID group at 65; Import and Clear off the bar's line. |
| UserControlSetupWizPractice | fixed | The Default Provider list moved past the page's right edge (935 on 930); labels left-aligned 234 px wide; the address group at 170. |

## Pass 2 (the scripts, `<W>/after-100` of the second after build; pictures with no change not opened again)

| Picture | Status | What was checked and found |
|---|---|---|
| FormApptPrintSetup | ok | Groups on the field column, Preview / Save / Print on one row, routing buttons on the content edge. |
| FormAutoNoteControlEdit | ok | Captions on the first line of their boxes, the arrows on the right padding, the boxes 6 px before them. |
| FormConfirmationSetup | ok | Grid on the padding, the status rows at the pitch, Group Families on the first row, Save on the bar. |
| FormCvxs | ok | Search row and grid on the padding, the grid to the right padding and the section gap over OK. |
| FormDiscountPlanEdit | ok | Wide fields on one right edge, List Patients at the field height. |
| FormEduResourceEdit | ok | One field column across both groups and the rows between them. |
| FormEquipmentEdit | ok | Status on the wide fields' right edge. |
| FormFeeSchedTools | fixed | The clinic pickers' boxes 8 px right of the field column (the picker's label is wider than 35 px): placed by the measured label width (`Hand.clinic_w`). |
| FormFeeSchedTools-increase | ok | New picture: rows on the field column, the round-to group on the content edge, the notes after their boxes. |
| FormFeeSchedTools-clear | ok | New picture: the note on one line, Clear under it. |
| FormFeeSchedTools-exportimport | ok | New picture: Export, Import and the date row; the check box under them. |
| FormFeeSchedTools-globalupdates | fixed | New picture: the two notes centred (main's alignment) against left-aligned buttons: left-aligned. |
| FormFeeSchedTools-copycode | ok | New picture: the note as tall as its four lines, the rows on the field column, the radios and Copy on it. |
| FormMountItemDefEdit | ok | The text box on the field column, both boxes on the right padding. |
| FormPatListElementEditEHR2014 | ok | The lists 136 wide with whole rows, every row at the pitch. |
| FormProcCodeNew | ok | The Type list on the padding; the buttons on the combos' right edge. The band under the separator holds the Set to Default row the code shows. |
| FormProcCodes | fixed | Three columns at main's width, 767 tall; `= Provider+Clinic` cut in the colour legend: the right column 250 wide and the legend's second column after the first's longer name. |
| FormQuickBooksOnlineSetup | fixed | Status on the label column; GridRulesTests `gap`: the Relay Port box 5 px under its caption's row: on it. |
| FormReportsMore | ok | Three columns at a 16 px gutter from the padding. |
| FormSheetFieldComboBox | ok | Pre-set options with whole rows down to the section gap; Tab Order on its column; Remove 80 wide. |
| FormSheetFieldInput | ok | The Auto Note buttons on their row, the list on the padding to the section gap. |
| FormSheetFieldStatic | fixed | Text box and list on the padding and the right padding; the list ended 13 px over the section gap: down to it. |
| FormTestLatency | fixed | (GridRulesTests `text` on the Speed note) three equal groups on the padding, the notes as tall as their lines, the Test buttons on one line. |
| FormUpdateSetup | ok | Rows on the two columns (the window's boxes are 20 px tall: its pitch 24), the group from the padding, the Recopy note and button on one row. |
| FormXDRSetup | ok | The group from the padding with its rows on the window's columns; Import and Clear on the bar's line. |
| UserControlSetupWizPractice | ok | One label column and one field column, the address group on them, the Default Provider list on the right column, the note and Advanced on the right padding. |

## Pass 3 (`<W>/f3-100`; the pictures that changed, by byte compare)

| Picture | Status | What was checked and found |
|---|---|---|
| FormProcCodes | ok | The right column 250 wide, the colour legend's names whole; three columns, one bottom edge. |
| FormFeeSchedTools (and its five pages) | ok | The clinic pickers' boxes on the field column and edge; the Global Updates notes left-aligned. |
| FormQuickBooksOnlineSetup | ok | Relay Port's box on its caption's row; the status text inside the group. |
| FormSheetFieldStatic | ok | The list down to the section gap over the bar. |
| FormTestLatency | fixed | GridRulesTests `padding` and `frame`: the Average group's "ms." units past its content edge in the narrower group: the rows on one label column from the content edge. |
| FormEduResourceEdit, FormPatListElementEditEHR2014 | ok | Changed bytes, no layout change (drawing noise): nothing found. |
| FormEhrLabImageEdit | fixed | (The re-plan, not the picture) the planner moved the splitter panels inside the container and grew the window 16 x 16 a pass: back to main's 784 x 589 by a script. |

## Pass 4 (`<W>/f4-100`, `<W>/f4-150`)

| Picture | Status | What was checked and found |
|---|---|---|
| FormTestLatency | fixed | Three equal groups, the units inside; the Latency note centred in the Speed note's height while the Speed note stood at the top: both at the top. |
| FormEhrLabImageEdit | ok | Main's size, the container on the padding to the section gap over Save, the panels where the container puts them, the grid and preview 8 px inside. |
| FormDoseSpotPropertyEdit | ok | New script: the label column from the padding, the buttons on the right padding, the boxes up to them. |
| FormProcCodes-150 | ok | Text, edges and gaps at 150%: nothing cut; the colour legend whole. The grid's title font is the harness's at 150% (every slice's grids). |
| FormFeeSchedTools-150, FormFeeSchedTools-copycode-150 | ok | Rows, pickers, the note's four lines at 150%: nothing found. The tab strip's captions draw in the harness's unscaled font at 150% (every slice's tab controls). |
| FormQuickBooksOnlineSetup-150 | ok | The two notes, the status row and the list captions at 150%: nothing cut. |
| FormSheetFieldStatic-150 | ok | Font group, rows, the insert note at 150%: nothing found. |
| FormUpdateSetup-150 | ok | The two-line captions, the group, the Recopy note at 150%: nothing found. |

## Pass 5 (`<W>/f5-100`, the final build)

| Picture | Status | What was checked and found |
|---|---|---|
| FormTestLatency | ok | Both notes at the top of their groups, the boxes on one row, the units inside the Average group, the Test buttons on one line: nothing found. |

The pass finds nothing: every after picture of the slice has been opened, and every one changed since its last opening has been opened again.

## Pass 6 (fix round, `<W>/f6-100` and `<W>/f6-150`, the fix round's build)

All 31 pictures at 100% and 6 at 150% drawn again from one build; each compared by bytes with its last opened picture (`f4-100`,
FormTestLatency's `f5-100`, `f4-150`). 28 at 100% and 4 at 150% are byte-identical and were not opened again (FormPatListElementEditEHR2014
and FormUpdateSetup among them: the wider compare string label and the taller Recopy note draw their designer texts where they stood).
The run-time texts are not in the pictures (Load sets them); GridRulesTests sets each one in the laid-out window and finds none cut.

| Picture | Status | What was checked and found |
|---|---|---|
| FormAutoNoteControlEdit | ok | Possible responses TopRight from the padding, its line on the box's first line, two lines' height for its run-time text; nothing else moved. |
| FormQuickBooksOnlineSetup | ok | The three list rows 12 px apart, the six lists one width, the arrows centred between each pair, the notes beside their lists; 56 px left above Save. |
| FormSheetFieldStatic | ok | TextW and TextH left-aligned after their boxes, room for "TextW: 9999" before the Calculate buttons. |
| FormQuickBooksOnlineSetup-150 | ok | The same at 150%: nothing cut, the rows apart. |
| FormSheetFieldStatic-150 | ok | The readouts and the Calculate buttons at 150%: nothing found. |

The pass finds nothing.
