# UI-12 slice 16: gallery audit

Gallery `G-UI3-ui12-16` (UI-12.md section 7, "Slice 16"). Every after picture opened once (`<W>/a3-100`, `<W>/a3-150`; the 35 changed ones
again against their before picture). `<W>` = `.git/od-tools/test-results/G-UI3-ui12-16-src/work`.

| Picture | What was off | Now |
|---|---|---|
| ControlAppt | the book and the calendar column edge to edge; the lower panel's boxes touching or 2 px over each other (Lab Cases, Daily Prod, Daily Goal), labels 1 px before them | the book and the column 8 px from the module's edges and 8 px apart, the tabs 8 px under; the fields at the 27 px pitch, labels 6 px before one field column, the toggle, Clear and Advanced/Search a column beside the pinboard |
| ControlAppt-search (and 150%) | (new state) the group's close button in its title band, the providers' group overlapping its own title | More and the close button on the first row under the title; results, the Date/Time group (one field column, the am/pm choices 6 px after the boxes), the providers' list beside Search by, three buttons on one row at the bottom |
| ControlApptPanel | the horizontal scroll bar ran under the vertical one in the designer | ends 1 px before it, as the code leaves it |
| ControlImages, -note, -import, -pearl | panes edge to edge; the import caption cut (616 of 677 px); Pearl's Layers button in the group's title band | panes 8 px from the module's edges and 8 px apart; captions 2 px over their boxes, the four import buttons a field's height 6 px apart; the Pearl group 70 px with its list and Layers on the first row |
| FormClaimFormEdit (and -internal, 150%) | the form edge to edge; labels 1-4 px before their boxes, uneven row gaps | the form 8 px from the edges; one settings column: caption over Description, the size and offset rows at the 27 px pitch, the check boxes a list, Items with Add on its row, the position rows over the internal note, Print and Save a section gap under it |
| FormClaimPrint (and 150%) | the preview edge to edge; Print 18 px over the bottom | the preview 8 px from three edges and 8 px before the column; Print bottom right, the page arrows and count one row a section gap over it |
| FormPrntScrn (and 150%) | the preview edge to edge; the bar 8 px under it | the preview 8 px from the edges, the bar a section gap under it, 8 px between the buttons |
| FormReportComplex | the preview edge to edge with a 28 px empty band under it | the preview 8 px inside the client area under the toolbar |
| FormRpPrintPreview, FormRpStatement | the preview edge to edge; the page buttons 2-10 px from the edges | the preview 8 px from the edges, a section gap over the buttons (Statement: one 24 px row centred) |
| FormSheetFillEdit (and -buttons, 150%) | the sheet 3 px from the edges; the fields, note and buttons on six different row lines | the sheet 8 px from the edges and a section gap over one block: captions 2 px over their boxes, four button rows 4 px apart, the right column Delete, Save, OK in the same rows |
| FormTPsign, -typed (150%) | the preview edge to edge; the signature boxes 3 px from the panel's top, their captions 2 px before them | the preview 8 px from the edges; boxes 8 px from the panel's top, captions 6 px before them, the typed rows 4 px under, Save bottom right |
| FormTerminal, -done | the list, thank-you and Done touching | the column with the grid's gaps; the connection note and the close panel 8 px from the bottom |
| FormImages, FormImagePickerPatient, FormWiki | lists, slider and image area 2-12 px from the edges | 8 px from the edges and 8 px apart (Wiki's status line on the bottom padding) |
| ODDatePicker, ODDateRangePicker | the date box 1 px down, its bottom line cut | the box at the picker's top |
| ODColorPicker, ODCodeRangeFilter | the two buttons at two heights 3 px apart; the example 2 px in and 2 px past the control | one height, 6 px apart; the example on the box's edge within the control |

Unchanged pictures (35): the controls with nothing to lay out and the windows whose surface the harness draws blank (the web views of
FormCareCreditWeb, FormErx and FormWebView, the hidden tooth chart of FormToothChartingBig, FormPopupFade's centred message): their
changes are in the designer and the layout snapshot, not visible off screen. No text is cut in any picture; no control covers another
but the rows of `grid-known/16.tsv`.

## Fix round (`ui/ui12-16-2`)

Each changed or new after picture opened (`<W>/f1-100`, `<W>/f1-150`, `<W>/f2-100`, `<W>/f2-150`, `<W>/img-100`).

| Picture | Before the fix round | Now |
|---|---|---|
| ControlAppt (and 150%) | the column beside the pinboard with three right edges (79, 75, 114) | Day/Week, Clear and Advanced + Search one 120 px column, 8 px before the pinboard |
| ControlAppt-search (and 150%) | the group 219 px in the 227 px column, the View combo, fields and Waiting header showing in an 8 px strip beside it and the tabs 5 px under it | the group the column's width and down to the tabs' bottom; nothing shows beside or under it |
| ControlImages-note (and new 150%) | a 17 px empty band under the boxes; at 150% the boxes cut by the unscaled 114 px | the panel ends on its boxes at 100% and 150% |
| FormClaimFormEdit-internal (and 150%) | the note cut after three of its five lines | all five lines; the position rows and the list 27 px higher |
| FormRpStatement | the arrows and page count 48 px left of centre | centred in the window |
| FormSheetFillEdit, -buttons (and 150%) | Add Field 81 px and Reduce Font Size 98 px in one column, the latter's text 1-2 px from its edges at 150% | both 108 px, `?` 6 px after; the text with the button padding at 150% |
| FormTPsign-changeuser (new, and 150%) | (no picture) | Change User 6 px before each box under its caption, the signer's name under it right-aligned, ending where the typed row starts |
| FormToothChartingBig | the chart inset 8 px (not visible: the chart draws blank off screen) | main's Dock Fill again; the picture as main's |
| ControlVideoPlayer-sample (new) | a blank 150 x 150 | the recorded clip with the bar: scrub bar and trim marks, the buttons 6 px apart, the time and trim text; the status message ends in an ellipsis at this 884 px width (the control's own trimming) |
| ControlCameraSession-live, -recording (new) | a blank 284 x 261 | the header line, the live picture, the slot list and the bottom bar; recording adds the REC line at the live picture's top right and Stop recording in the bar |
| FormImageViewer-sample (new) | a blank picture | the info line with the grid's padding over the image |

No text is cut and no control covers another in these pictures.
