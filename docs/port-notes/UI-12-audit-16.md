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
