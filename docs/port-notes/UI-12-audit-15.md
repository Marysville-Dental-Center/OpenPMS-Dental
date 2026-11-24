# UI-12 slice 15: audit of the gallery

Slice 15 (`ui/ui12-15-1`, [UI-12.md](UI-12.md) section 7, "Slice 15"): the report designer's and viewer's remaining dialogs and property
pages, off the layout grid in UI-11-FINAL's sweep. Every after picture opened once at 100% (the program's font, Segoe UI 9), then the
changed ones again; the seven densest at 150%. One line per picture: what was checked (cut or covered text, overlaps, margins, columns,
gaps, crowding, empty bands, the bar, the window's edges) and what was found. `<W>` = `.git/od-tools/test-results/G-UI3-ui12-15-src/work`.

## Pass 1 (the planner's layout, `<W>/a1-100`)

| Picture | Status | What was checked and found |
|---|---|---|
| BodyCtl | ok | Three label + box rows on one column, labels right-aligned 6 px before the boxes. |
| ChartCtl | fixed | The percent caption run into its box (overlap); Chart Type and Palette left-aligned, the other labels right-aligned. |
| ChartLegendCtl | ok | Two label + box rows, the check boxes on the field column. |
| CodeCtl | ok, wrong (fix round) | Captions over the lists, Check Syntax on the right padding; main cut "Function Code". Missed: the panel ran to 464, past the tab page's 450 (checked at the page's own 472). |
| CustomReportItemCtl | fixed | The property grid 13 px in, fx past the page's 440 px content edge. |
| DataSetRowsCtl | fixed | (Main's layout: the harness builds it since this slice.) Buttons 9 to 20 px apart, Load From SQL on three lines. |
| DataSetsCtl | fixed | Refresh Fields cut in a 52 px button (on main too). |
| DataSourcePassword | fixed | Prompt wrapped to two lines, the box 4 px over the bar, OK and Cancel off the right padding. |
| DesignEditLines, DesignXmlDraw, PageDrawing | ok | Drawing surfaces, no controls. |
| DialogAbout | fixed | "E-mail:" on two lines (cut on main too); OK in the middle. |
| DialogDataSourceRef | fixed | Data provider's label run into its box; the bar 51 px over the window's bottom and off the right padding. |
| DialogDataSources | fixed | The template wrapped Data Source Name; the reference box past its check box's row; the bar 89 px over the bottom. |
| DialogEmbeddedImages | fixed | The name's caption over its box; a band of 60 px over the bar; Cancel 28 px from the right edge. |
| DialogExprEditor | fixed | The code's Layout handler: both boxes on the window's right edge region and 4 px over the bar. |
| DialogFilterOperator | fixed | Label on one line now (window 274 wide), but Cancel 23 px from the right edge. |
| DialogListOfStrings | fixed | Caption on two lines, the buttons off the right padding. |
| DialogMessages | fixed | The box 16 px in and 4 px over OK. |
| DialogNew | fixed | The choices list docked over the window (its frame on the window's edges). |
| DialogNewMatrix | fixed | The cell expression's caption over its box; the bar 34 px over the bottom; Matrix Columns' caption on two lines. |
| DialogValidValues | fixed | OK and Cancel 12 px from the right edge. |
| DialogValidateRdl | fixed | Validate and Close 15 px from the right edge. |
| DrillParametersDialog | fixed | The bar off the right padding; Report name's label 18 px in. |
| FiltersCtl, SortingCtl | ok | Grid on the padding, fx, Delete, Up and Down beside it 4 px apart on the content edge. |
| GroupingCtl | fixed | The control 631 px wide (the tab page is 450); its content within 440. |
| ImageCtl | fixed | The Source group's boxes at three left edges (86, 88, 102). |
| ListCtl | ok | Label + box rows, Page Breaks and XML groups over the width, Group Expressions under them. |
| MatrixCtl | fixed | DataSet Name's and No rows message's boxes to 461, past the page's content edge. |
| ModulesClassesCtl | ok | Captions over the two grids, Delete beside each on the content edge. |
| PropertyDialog | fixed | The tab control docked on the window's top, left and right edges; Apply 5 px from the right edge. |
| QueryParametersCtl | ok | The grid on the padding. |
| RdlDesigner | ok | The menus, the code's toolbar and status bar as on main; the hidden image holders inside the client area. |
| RdlEditPreview, -editor, -browser | ok | The three pages of the bottom tab control, edge to edge (the editor's surface, as on main). |
| ReportXmlCtl | ok | Label + box rows on one column, "..." 6 px after the first box. |
| SQLCtl | fixed | The tables tree docked on the window's left and top edges; the window 494 wide. |
| SingleCtlDialog | fixed | A 25 px band under the bar's buttons, Apply 21 px from the right edge. |
| SubreportCtl | fixed | The grid 4 px under the check box. |
| TableColumnCtl | fixed | The control 555 px wide (the Fixed Header note beside its check box). |
| TableCtl, TableRowCtl | ok; TableRowCtl wrong (fix round) | Label + box rows, groups over the width (TableRowCtl's Row Height under its group). Missed: TableRowCtl's group ran to 464, past the tab page's 450 (checked at the page's own 472), and Row Height stood off the field column. |

## Pass 2 (the layout scripts and the code changes, `<W>/a2-100`, then `<W>/a3-100`)

| Picture | Status | What was checked and found |
|---|---|---|
| ChartCtl | ok | One right-aligned label column; the percent caption on one line over its box from the padding; check boxes on the field column. |
| CustomReportItemCtl | ok | fx on the content edge, the property grid beside it down to the page's bottom padding. |
| DataSetRowsCtl | ok | Buttons 4 px apart, Load From SQL on one line; the warning on two lines on the bottom padding. |
| DataSetsCtl | ok | Refresh Fields on one line; Timeout under Data Source's column; SQL Select's caption on Timeout's row. |
| DataSourcePassword | ok | Prompt on one line over its box; OK and Cancel bottom right. |
| DialogAbout | ok | Website and E-mail in one label column, each on one line; OK bottom right. |
| DialogDataSourceRef | ok | Note's three lines; the rows on the grid; the connection box takes the spare height; the bar on the padding. |
| DialogDataSources | ok | List with Add and Remove; caption over the name's box; the reference, provider and connection rows; the bar on the padding. |
| DialogEmbeddedImages | ok | List and its buttons, the name's caption over its box, MIME type row; the preview area (borderless picture box) under them. |
| DialogExprEditor | ok | Two 190 px boxes with ">>" between them at their top, 12 px over the bar, 8 px margins. |
| DialogFilterOperator, DialogListOfStrings, DialogValidValues, DialogMessages, DialogValidateRdl, DrillParametersDialog | ok | The bar bottom right on the padding, 12 px or more under the content. |
| DialogNew, SQLCtl | ok | List and tree on the padding; the bar panel's buttons 12 px under them. |
| DialogNewMatrix | ok | Captions on one line; lists one height; Up, Down and Delete on the right padding; the cell expression over the width. |
| GroupingCtl | ok | Back to 488 x 304; content within 440. |
| ImageCtl | ok | Radio buttons in one column, the boxes on one column, fx and "..." on the group's content edge; Sizing on the box column. |
| MatrixCtl | ok (a3) | One field column with the XML group's box (a2 had them 16 px apart: the `column` rule); boxes to 440. |
| PropertyDialog, SingleCtlDialog | ok | The tab control (PropertyDialog) on the padding; OK, Cancel and Apply 12 px under the content on the bottom and right padding. |
| SubreportCtl | ok (a3) | The check box 8 px wider (a2 wrapped its text: the `text` rule); the grid 8 px under it with Refresh on the content edge. |
| TableColumnCtl | ok | Back to 472 x 288; Column Width and Fixed Header on the group's field column, the note wrapped to the content edge. |

## At 150% (`<W>/a3-150`; before `<W>/before-150`)

| Picture | Status | What was checked and found |
|---|---|---|
| ChartCtl-150, DataSetsCtl-150, GroupingCtl-150, TableCtl-150 | ok | Scaled by PropertyDialog.ScaleToDpi: no cut text; the boxes' own text drawn at the harness's font (as in UI-12F's pictures). |
| DialogDataSourceRef-150, DialogDataSources-150, DialogNewMatrix-150 | ok | Scaled as a whole by the dialogs' old Form.AutoScale (slice 13): no cut text, the single-line boxes keep their font height. |

## Fix round (`ui/ui12-15-2`, the review's 14 points; `<W>/f3-100`, `<W>/f3-150`)

Each property page checked against PropertyDialog's tab page (about 450 px: the content within 440), not only the page's own width.

| Picture | Status | What was checked and found |
|---|---|---|
| CodeCtl | fixed | The panel 8 to 440 (was 464: the code box's scroll bar cut in the tab page); Check Syntax on the panel's right edge (440). |
| TableRowCtl | fixed | The Visibility group 8 to 440 as TableColumnCtl's, fx 8 px inside it; Row Height right-aligned before its box on the group's field column (182). |
| GroupingCtl | fixed | Name, the grid, Label and Parent end at 386; fx, Delete, Up, Down at 392; all buttons 23 px, the stack 4 px apart with the grid as tall (104); Label a section gap under it; the second check-box column on one x (212). |
| DialogAbout | fixed | The two version lines as wide as the title (240, 184), centred under it. |
| DialogDataSources, DialogDataSourceRef | fixed | Data provider at the content edge (MiddleLeft), its box 6 px after the text. |
| DialogNewMatrix | fixed | DataSet at the content edge; the three lists without IntegralHeight: the fields list and Matrix Rows end on one y (307). |
| DialogEmbeddedImages | fixed | The image list without IntegralHeight: it ends with Remove (85). |
| DialogValidateRdl | fixed | The error list without IntegralHeight: 12 px over Validate and Close. |
| ChartCtl-150, DataSetsCtl-150, GroupingCtl-150, TableCtl-150 | fixed | Every font at 150% (FitGallery.ScaleEveryFont for the property pages): the text boxes and combo boxes at the top level as tall as the XML group's; label text and box text on one line within 1 px (TableCtl: No rows message 63 / 62, Detail Element Name 348 / 348). |
| DialogDataSourceRef-150, DialogDataSources-150, DialogNewMatrix-150 | ok | Redrawn for the layout changes above; no cut text. |
