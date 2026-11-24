# UI-12 slice 12: the audit of gallery G-UI3-ui12-12

Every after picture of `.git/od-tools/test-results/G-UI3-ui12-12` (the slice's 42 windows at 100%, the 9 densest at 150%), opened and
looked at as a designer would: cut or covered text, overlaps, margins, columns and edges, gaps, crowding and empty bands, the control looks,
titles, boxes on the left. Before = `main` at the branch point (`42cc8ea3f`), drawn by the `before` step (UI-12.md, "Slice 12"). What stays
as drawn, and why, is in the line.

**Pass 1** (`work/a1-100` and the second `after` step: the planner and its templates alone): every window had findings. The plain forms'
tree root was their outer bounds, so the bottom bars stood under the client area (FormCentralPasswordCheck drew no OK or Cancel): fixed in
the planner (`plain_client_root`). Stock check boxes wrapped (GridRulesTests: the planner gave them the program check box's lead; Windows
draws them): the plain lead, then 22. System-style group boxes drew no frame or title (FormCentralProdInc, the search columns): the
System style taken out. Margins of 12 to 113 px, fields ending far short of the bars, grids at two tops, OK over Cancel
(FormCentralPatientSearch), the Lock Date group's labels over their boxes (FormCentralSecurity, main too), cut notes (FormCentralChooseDatabase,
FormCentralProdInc), the graph's settings panels untouched (split containers) with labels touching their boxes (FormPrintSettings,
FormPrintImage), the dashboard's hover panels overlapping (DashboardCellCtrl), window sizes grown for main's margins. Fixes: 34 layout
scripts on the planner's tree.
**Pass 2** (`work/after-100` of the fourth `after` step, after the scripts): all 42 opened. Found: FormCentralSecurity's Date and Days
boxes at x 166 (its users control has a label3 and label4 of its own, found first by name: the script reads this window's nodes);
GraphQuantityOverTimeFilter's grouping options still cut (its options container kept the designer's 60 px: sized to the panel); the print
windows' third group title wrapped ("Image Positi..."; groups as wide as their titles); xCrudGenerator's Run queries check box cut its last
letter (8 px wider); ProductionGraphOptionsCtrl's and IncomeGraphOptionsCtrl's check box texts right-aligned 13 to 22 px from their boxes
(left-aligned); DashboardTabCtrl's Print Page cut its descenders (23 px); FormCentralLogOn and FormCentralUserSettings grown 12 and 3 px by
the planner (back to main's); FormDashboardNamePrompt and FormCentralChooseDatabase 1 to 4 px short of the section gap over the bar. All
fixed by the scripts (three new: ProductionGraphOptionsCtrl, IncomeGraphOptionsCtrl, DashboardTabCtrl). GridRulesTests found nothing new.
**Pass 3** (`work/final-100`, `work/final-150`, the final build): the 13 pictures that changed opened (the 29 others byte-identical to pass 2): the pass 2 fixes in place, nothing new. The 7 densest at 150%: FormCentralSecurity (a FormODBase window, laid out at 150% by its LayoutManager) shows nothing. The six plain windows of the seven (FormCentralManager, FormCentralPatientSearch, FormCentralConnections, FormCentralProdInc, FormCentralConnectionEdit, GraphQuantityOverTime) draw their fonts at 150% in their 100% bounds: the off-screen harness scales a window through LayoutManager, and a plain window is scaled by Windows (AutoScaleMode.Dpi, per-monitor) only on a 150% monitor, which the harness does not run. Their text is cut in main's pictures and in these alike, so they are not in the gallery (for the lead: a plain-window scale in `WindowBuilder.LayOutAt`). The pass finds nothing else.
**Pass 4** (the review's fix round, branch `ui/ui12-12-2`; `work/fix-100`, `work/fix-150`, `work/fix2-100`, `work/fix2-150`): the 12
pictures the fixes changed opened (the 30 others byte-identical to pass 3, so drawn in the same session) and the 9 densest at 150%.
The review's findings fixed: FormCentralLogOn's OK on the bar beside Exit; FormDashboardEditCell's OK and Cancel and the print windows'
Close whole in their strips (zoomed: every border drawn); the stock box-left check boxes and radio buttons with main's right-aligned texts
(FormCentralConnections, FormCentralConnectionEdit, FormCentralUserEdit, FormCentralUserPasswordEdit, GraphQuantityOverTime's Series
Grouping) their texts after the box like the others; GraphIncome's From and To one width with one button look; xCrudGenerator's four
check boxes one list, box on the left. The plain windows at 150% (now drawn: the harness scales their bounds) found one more:
FormCentralProdInc's write-off radio buttons (main's TopLeft text) cut their descenders (Using, payment): centred in their rows, redrawn
whole. What the harness shows at 150% and a screen would not: a plain window's menu bar and the text typed in a stock text box keep
the 100% font (main's pictures alike), and FormCentralManager (2751 px wide at 150%) is held at the capture screen's width (2396 px),
its two grids taking the difference. Nothing else.

## The pictures (final)

| Picture | Checked | Result |
|---|---|---|
| 1-slice-windows/FormCentralManager | margins, the two grids' top, the search column, the top block's rows | Grids on one top and the bottom padding, the search column at their top, the Conn Groups row, Refresh Statuses on the combo box's edge, the version line under them; the empty band right of the Filter group is main's (the menu row's width) |
| 1-slice-windows/FormCentralPatientSearch | OK and Cancel, the column, the grid | OK and Cancel side by side on the bar under the column (the planner had OK over Cancel); the grid from the padding to the bottom padding |
| 1-slice-windows/FormCentralSecurity | the three groups, the Lock Date rows, the bar | One top; Lock Date's note whole, Date and Days on one field column (main drew their labels over the boxes); Sync Options at Lock Date's height; Domain Login at its own height with OK and Close on the bottom right under it |
| 1-slice-windows/FormCentralUserEdit | margins, the tab pages, Is Hidden, the bar | The tab control on the padding; the name box over the group list from the page's padding, the security tree and the Domain User row to the page's right padding; Is Hidden's text after its box (pass 4: 26 px); the Name and User Group labels under the tab control as on main (grid-known) |
| 1-slice-windows/FormCentralProdInc | groups, the field column, the radio lists, cut note | Today's Date and the Date Range rows on one column, the arrows on it; Report Type and the write-offs group on one top and height, every radio button centred in its row (pass 4: the write-offs' TopLeft texts cut at 150%); the PPO note whole; group frames and titles drawn (System style out) |
| 1-slice-windows/FormCentralConnections | margins, the right column, the Prefs check boxes | The grid from the padding to the bottom padding; the column 208 px wide with Add, Conn. Order and Prefs 8 px apart; Automatically Log On and Dynamic Mode's two lines left-aligned after their boxes (pass 4: Log On's text had kept main's right alignment, 27 px from its box); main's size |
| 1-slice-windows/FormCentralConnectionGroupEdit | the two grids, the buttons between them | One width, top and bottom, down to the section gap over the bar; Add and Remove centred between them |
| 1-slice-windows/FormCentralConnectionGroups | margins, the size | The grid from the padding to the bottom padding, Add and Close in the column beside it; main's 457 x 421 |
| 1-slice-windows/FormCentralConnectionEdit | the Note box, the check boxes, the bar | Note on the Remote URI box's right edge, down to the section gap; Using eClinicalWorks and Show clinic breakdown with their texts after the box (pass 4: 26 px); the buttons 24 px |
| 1-slice-windows/FormCentralPatientTransfer | the two grids, the bar | One top and bottom, the source caption over the first; Add and Remove under each grid on the bar, Export on the grids' bottom edge, Close bottom right |
| 1-slice-windows/FormCentralChooseDatabase | the cut note, captions | The middle-tier note whole; captions 2 px over their boxes; the check box centred on the bar's buttons |
| 1-slice-windows/FormCentralLogOn | the list, the Password row, OK and Exit | The list from the padding to the bottom padding, the Password box to the right padding; OK and Exit on the bottom bar at the bottom right, Bottom|Right as on main (pass 3 had OK under the password box); main's size |
| 1-slice-windows/FormCentralGroupPermEdit | the field column, the group, the note | Type and the group's rows on one field column, the example note whole across the group |
| 1-slice-windows/FormCentralPasswordChange | the note, the rows | The note across the content; the rows' boxes to the right padding |
| 1-slice-windows/FormCentralPasswordCheck | the bar (pass 1: missing) | OK and Cancel on the bar inside the client area; the box to the right padding |
| 1-slice-windows/FormCentralUserPasswordEdit | the rows, Show | The rows' boxes to the right padding, Show on the field column with its text after the box (pass 4: 26 px) |
| 1-slice-windows/FormCentralUserSettings | the row, the check box | The box to the right padding, the check box on the field column; main's size |
| 1-slice-windows/FormCentralUserGroupEdit | the row, the bar | The box to the right padding; Delete left, OK and Cancel right |
| 1-slice-windows/FormCentralUserGroups | the list, the size | The list to the bottom padding, Add and Close beside it; main's size |
| 1-slice-windows/FormMySQLUserManager | the group, the list | Two captioned columns in the group, Connect on its content edge; the list to the bottom padding with Add, Edit, Drop beside it; main's size |
| 1-slice-windows/FormMySQLUserEdit | the rows, the group | The rows' boxes to the right padding, the Permission Level group on the field column |
| 1-slice-windows/FormDatabaseCheck | margins, the results box | The rows from the padding, Check and Repair on the right padding, the results box across the content to the section gap over Copy Text |
| 1-slice-windows/DocumentationBuilder-Form1 | the notes, the steps, the buttons | Every note whole, 4 px within a step and 8 px between steps, the Build buttons centred under their notes |
| 1-slice-windows/xCrudGenerator-Form1 | the settings column, the notes, the snippet area | The Port Number, Database and Password rows on one field column; the four check boxes one list from the padding, box on the left (pass 4: Schema and Run were text-less boxes after labels on the field column), Run queries whole; Run CRUD Gen on the field column; the notes beside them, the snippet group and box down to the bottom padding; main's size |
| 1-slice-windows/FormDebug | the warning, the rows, the output | The warning on the first row, the rows' boxes to the right padding, the output box to the section gap over Start |
| 1-slice-windows/GraphQuantityOverTime | the settings panel | Chart Title on the Chart Options field column; the three groups on one top and height with their rows at the pitch; All, Top and None with their texts after the circle (pass 4: 23 px); the chart (drawn by the code) as it is |
| 1-slice-windows/GraphQuantityOverTimeFilter | the grouping options (pass 2: cut), Series Grouping | The grouping options whole at their own size; the graph's settings as GraphQuantityOverTime (pass 4: All, Top and None as the Group By radios) |
| 1-slice-windows/GraphIncome | the three groups, the date pickers | One top and height; From and To both 112 px with the same drop button (pass 4: To was 93 px), Filter Dates as wide as them; the chart as the code draws it |
| 1-slice-windows/FormDashboardEditTab | the edges, the side panel | Edge to edge under its menu as on main; the side panel on its padding; Print Page's lowest pixel row meets the tab page's frame line (the tab control's own drawing, as on main) |
| 1-slice-windows/FormDashboardEditCell | the bottom strip (zoomed) | OK and Cancel on the right padding at main's height in the strip, every border drawn (pass 4: 3 px past the strip) |
| 1-slice-windows/FormDashboardNamePrompt | the row, the bar | The box to the right padding, the section gap over the bar |
| 1-slice-windows/FormPrintSettings, FormPrintImage | the groups, labels, title (pass 2: "Image Positi..."), the Close strip (zoomed) | Three groups on one top and height as wide as their titles, labels 6 px before their boxes, Export and Print on the groups' rows, Landscape after Print; Close whole in its strip at main's height (pass 4: cut 3 px, the strip 8 px short); the preview as it is |
| 1-slice-windows/DashboardCellCtrl, DashboardPanelCtrl | the hover button panels | One row on the right edge (they overlapped by 13 px) |
| 1-slice-windows/DashboardTabCtrl | Print Page | 23 px, its text whole |
| 1-slice-windows/BrokenApptGraphOptionsCtrl | the descriptions and combo boxes | One column after the widest radio button, each combo box under its description |
| 1-slice-windows/ProductionGraphOptionsCtrl, IncomeGraphOptionsCtrl | check box texts | Left-aligned after their boxes |
| 1-slice-windows/GroupingOptionsCtrl, HqMessagesRealTimeOptionsCtrl | group, radio buttons | The planner's layout; nothing found |
| 1-slice-windows/Legend | the scroll buttons | The code's table; unchanged |
| 2-at-150/FormCentralSecurity | text, rows and groups at 150% | Nothing cut; the same arrangement as at 100% |
| 2-at-150/FormCentralManager | the search column, the grids | Nothing cut; held at the capture screen's width (the grids narrower than 150%); the menu bar at the 100% font (harness) |
| 2-at-150/FormCentralPatientSearch | the 14 search rows, the check boxes | Nothing cut; the rows and boxes on one column; Fetching Results beside Search |
| 2-at-150/FormCentralConnections | the right column, Prefs | Nothing cut; the check box texts after their boxes |
| 2-at-150/FormCentralProdInc | the groups, the radio buttons, the note | Nothing cut after the pass 4 fix (the write-off radios' descenders); the PPO note whole |
| 2-at-150/FormCentralConnectionEdit | the rows, Note, the bar | Nothing cut |
| 2-at-150/GraphQuantityOverTime, GraphQuantityOverTimeFilter | the settings groups | Nothing cut; the three groups on one top and height |
| 2-at-150/Form1 (xCrudGenerator) | the settings column, notes | Nothing cut; the four check boxes one list; typed text in the boxes at the 100% font (harness, main alike) |
