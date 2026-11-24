# UI-PLAIN audit: G-UI3-uiplain, every picture opened (docs/port-notes/UI-PLAIN.md)

Pass 1: the first after pictures (`work/after-100`, this branch at 854dbf142) against `main`'s (`work/main-100`, `main-150`), drawn in this
session from a fresh build of 2268b57b0. Pass 2: the gallery's after pictures (`work/after-100b`, `after-150`, the fix commit 0a42fd505)
against the same `main` pictures. One line per picture: picture | changed % | what was checked | finding.

## Pass 1 (three findings, fixed in 0a42fd505)

- 1-plain-windows/DashboardTabCtrl.png | 100.0000 | background token, framed inputs, shared looks | ok
- 1-plain-windows/Legend.png | 100.0000 | background token, framed inputs, shared looks | ok
- 1-plain-windows/ProductionGraphOptionsCtrl.png | 99.9955 | background token, framed inputs, shared looks | ok
- 1-plain-windows/IncomeGraphOptionsCtrl.png | 99.9949 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormProgressStatus.png | 99.9216 | background token, framed inputs, shared looks | FINDING (fixed in pass 2): light grey
- 1-plain-windows/ODProgressBar.png | 99.7997 | background token, framed inputs, shared looks | ok
- 1-plain-windows/DashboardCellCtrl.png | 99.7787 | background token, framed inputs, shared looks | ok
- 1-plain-windows/GroupingOptionsCtrl.png | 99.3082 | background token, framed inputs, shared looks | ok
- 1-plain-windows/HqMessagesRealTimeOptionsCtrl.png | 99.2522 | background token, framed inputs, shared looks | ok
- 1-plain-windows/DashboardPanelCtrl.png | 98.5464 | background token, framed inputs, shared looks | FINDING (fixed in pass 2): icon and caption apart
- 1-plain-windows/FormDashboardEditCell.png | 90.9488 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormDashboardEditTab.png | 89.0571 | background token, framed inputs, shared looks | ok
- 1-plain-windows/BrokenApptGraphOptionsCtrl.png | 88.5890 | background token, framed inputs, shared looks | ok
- 1-plain-windows/DocumentationBuilder-Form1.png | 85.2327 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralProdInc.png | 79.0946 | background token, framed inputs, shared looks | ok
- 1-plain-windows/UpdateFileCopier-FormMain.png | 75.1042 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralGroupPermEdit.png | 70.9867 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralPasswordChange.png | 69.0998 | background token, framed inputs, shared looks | FINDING (fixed in pass 2): underlined
- 1-plain-windows/FormMySQLUserEdit.png | 68.4519 | background token, framed inputs, shared looks | FINDING (fixed in pass 2): underlined
- 1-plain-windows/FormCentralConnectionEdit.png | 67.2281 | background token, framed inputs, shared looks | FINDING (fixed in pass 2): underlined
- 1-plain-windows/FormServiceManage.png | 66.6560 | background token, framed inputs, shared looks | ok
- 1-plain-windows/GraphQuantityOverTimeFilter.png | 66.2403 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormWebConfigSettings.png | 65.6357 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormMySQLUserManager.png | 64.4600 | background token, framed inputs, shared looks | FINDING (fixed in pass 2): underlined
- 1-plain-windows/FormProgressExtended.png | 64.1725 | background token, framed inputs, shared looks | FINDING (fixed in pass 2): kept its grey
- 1-plain-windows/ServiceManager-FormMain.png | 62.1012 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralUserGroupEdit.png | 61.9915 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralPasswordCheck.png | 59.9913 | background token, framed inputs, shared looks | FINDING (fixed in pass 2): underlined
- 1-plain-windows/FormCentralUserSettings.png | 59.7423 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralChooseDatabase.png | 58.9488 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralUserPasswordEdit.png | 54.8921 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralLogOn.png | 54.1162 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormDashboardNamePrompt.png | 49.4841 | background token, framed inputs, shared looks | ok
- 1-plain-windows/GraphQuantityOverTime.png | 40.4855 | background token, framed inputs, shared looks | ok
- 1-plain-windows/xCrudGenerator-Form1.png | 34.6973 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormDebug.png | 33.7292 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralUserGroups.png | 31.3689 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralPatientSearch.png | 30.1702 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralConnections.png | 28.4171 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralManager.png | 26.8645 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralConnectionGroups.png | 26.5516 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormDatabaseCheck.png | 24.9083 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralConnectionGroupEdit.png | 21.7279 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralPatientTransfer.png | 21.6348 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralUserEdit.png | 21.2669 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormPrintImage.png | 17.8111 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormPrintSettings.png | 17.8111 | background token, framed inputs, shared looks | ok
- 1-plain-windows/MsgBoxCopyPaste@CodeBase.png | 11.4180 | background token, framed inputs, shared looks | FINDING (fixed in pass 2): underlined
- 1-plain-windows/GraphIncome.png | 2.0437 | background token, framed inputs, shared looks | ok
- 1-plain-windows/FormCentralSecurity.png | 1.0587 | background token, framed inputs, shared looks | ok

## Pass 2 (the gallery: 59 pictures, 0 findings)

- 1-plain-windows/DashboardTabCtrl.png | 100.0000 | the stock tab control in the program's tab look (strip SurfaceSubtle, Production selected with the Accent line, the page border); Print Page over the strip's right end as on main (the designer's place); two synthetic tabs | ok
- 1-plain-windows/Legend.png | 100.0000 | the four scroll buttons in the button look; the legend's own drawing is chart content | ok
- 1-plain-windows/ProductionGraphOptionsCtrl.png | 99.9955 | the Included Production Sources group; check boxes in the look | ok
- 1-plain-windows/IncomeGraphOptionsCtrl.png | 99.9949 | the Included Income Sources group; check boxes in the look | ok
- 1-plain-windows/FormProgressStatus.png | 99.9216 | the window in the token (pass 1: light grey); the progress bar in the rounded track with the Accent fill | ok
- 1-plain-windows/ODProgressBar.png | 99.7997 | the progress bar's rounded track (main: Windows' classic empty bar) | ok
- 1-plain-windows/DashboardCellCtrl.png | 99.7787 | the cell's toolbar buttons in the button look on the token background; the chart placeholder icon as on main | ok
- 1-plain-windows/GroupingOptionsCtrl.png | 99.3082 | the Group By group; radio circles | ok
- 1-plain-windows/HqMessagesRealTimeOptionsCtrl.png | 99.2522 | the Group By group; radio circles | ok
- 1-plain-windows/DashboardPanelCtrl.png | 98.5464 | Add Col: caption over its icon, Add Row: caption before its icon, as Windows Forms laid them out (pass 1: icon and caption apart) | ok
- 1-plain-windows/FormDashboardEditCell.png | 90.9488 | the token background; the bottom strip's buttons in the button look (the preview button cut by its strip as on main, the designer's 28 px in a 25 px strip) | ok
- 1-plain-windows/FormDashboardEditTab.png | 89.0571 | the menu bar as on main; the dashboard's tab control in the program's tab look; the side panel's list with its flat border | ok
- 1-plain-windows/BrokenApptGraphOptionsCtrl.png | 88.5890 | Count By group in the program's group box; radio circles; the two disabled combo boxes framed in the disabled grey | ok
- 1-plain-windows/DocumentationBuilder-Form1.png | 85.2327 | background token; the read-only connection box framed in the read-only grey; the version boxes framed; the three build buttons in the button look | ok
- 2-at-150/FormCentralProdInc-150.png | 83.4287 | at 150%: bounds and fonts scaled; the looks drawn at the harness's 96 dpi (check boxes, radio circles, frames, group box corners at the 100% size: UI-13 section 4), as in the program they scale with the window's dpi; Date Range, Report Type and Show Insurance Write-offs in the program's group box; radio circles; the read-only Today's Date box grey | ok
- 1-plain-windows/FormCentralProdInc.png | 79.0946 | Date Range, Report Type and Show Insurance Write-offs in the program's group box; radio circles; the read-only Today's Date box grey | ok
- 1-plain-windows/UpdateFileCopier-FormMain.png | 75.1042 | the token background (the copier's own linked copy of the looks); the retry button is hidden until a copy fails | ok
- 1-plain-windows/FormCentralGroupPermEdit.png | 70.9867 | the read-only Type box in the read-only grey; the date group in the program's group box | ok
- 2-at-150/FormCentralConnectionEdit-150.png | 70.7331 | at 150%: bounds and fonts scaled; the looks drawn at the harness's 96 dpi (check boxes, radio circles, frames, group box corners at the 100% size: UI-13 section 4), as in the program they scale with the window's dpi; two groups, framed boxes, check boxes; &OK and &Cancel without the underline until Alt (pass 1: underlined) | ok
- 1-plain-windows/FormCentralPasswordChange.png | 69.0998 | framed boxes; &OK and &Cancel without the underline (pass 1: underlined) | ok
- 1-plain-windows/FormMySQLUserEdit.png | 68.4519 | Permission Level group, radio circles; &OK and &Cancel without the underline (pass 1: underlined) | ok
- 2-at-150/GraphQuantityOverTimeFilter-150.png | 67.7167 | at 150%: bounds and fonts scaled; the looks drawn at the harness's 96 dpi (check boxes, radio circles, frames, group box corners at the 100% size: UI-13 section 4), as in the program they scale with the window's dpi; the Group By group; the settings as GraphQuantityOverTime; the legend's scroll buttons in the button look | ok
- 1-plain-windows/FormCentralConnectionEdit.png | 67.2281 | two groups, framed boxes, check boxes; &OK and &Cancel without the underline until Alt (pass 1: underlined) | ok
- 1-plain-windows/FormServiceManage.png | 66.6560 | the token background; the read-only Status box grey; the buttons in the button look | ok
- 1-plain-windows/GraphQuantityOverTimeFilter.png | 66.2403 | the Group By group; the settings as GraphQuantityOverTime; the legend's scroll buttons in the button look | ok
- 1-plain-windows/FormWebConfigSettings.png | 65.6357 | Connection Settings and Master Connection Settings groups; framed boxes; the stock combo boxes framed with the chevron | ok
- 1-plain-windows/FormMySQLUserManager.png | 64.4600 | Connection Settings group; the read-only Server Name box grey; Add, Edit, Drop, Close without the underline (pass 1: underlined) | ok
- 1-plain-windows/FormProgressExtended.png | 64.1725 | the light grey (ControlLight) window takes the token (pass 1: kept its grey); the log box framed | ok
- 1-plain-windows/ServiceManager-FormMain.png | 62.1012 | the service list with the flat list border; the long help label cut as on main (its text) | ok
- 1-plain-windows/FormCentralUserGroupEdit.png | 61.9915 | framed description box; Delete with its icon (UI.Button) | ok
- 1-plain-windows/FormCentralPasswordCheck.png | 59.9913 | framed box; &OK and &Cancel without the underline (pass 1: underlined) | ok
- 1-plain-windows/FormCentralUserSettings.png | 59.7423 | the read-only User box grey; the check box in the look | ok
- 1-plain-windows/FormCentralChooseDatabase.png | 58.9488 | the middle tier group in the program's group box (title inside); three framed boxes; OK keeps its accent (UI.Button) | ok
- 1-plain-windows/FormCentralUserPasswordEdit.png | 54.8921 | the read-only User box grey; framed password boxes; the Show check box in the look | ok
- 1-plain-windows/FormCentralLogOn.png | 54.1162 | the user list with the flat list border; the password box framed | ok
- 1-plain-windows/FormDashboardNamePrompt.png | 49.4841 | framed box; OK and Cancel in the button look | ok
- 2-at-150/GraphQuantityOverTime-150.png | 41.2752 | at 150%: bounds and fonts scaled; the looks drawn at the harness's 96 dpi (check boxes, radio circles, frames, group box corners at the 100% size: UI-13 section 4), as in the program they scale with the window's dpi; the three setting groups; framed combo boxes, date pickers and up-down box; the split container's panels on the token; the chart is content | ok
- 1-plain-windows/GraphQuantityOverTime.png | 40.4855 | the three setting groups; framed combo boxes, date pickers and up-down box; the split container's panels on the token; the chart is content | ok
- 2-at-150/Form1-150.png | 35.7197 | at 150%: bounds and fonts scaled; the looks drawn at the harness's 96 dpi (check boxes, radio circles, frames, group box corners at the 100% size: UI-13 section 4), as in the program they scale with the window's dpi;  | ok
- 1-plain-windows/xCrudGenerator-Form1.png | 34.6973 | framed boxes; the Create Snippet Tool group; check boxes in the look; the snippet box framed with Fluent scroll bars | ok
- 1-plain-windows/FormDebug.png | 33.7292 | the two read-only boxes and the read-only output box in the read-only grey, framed, as read-only boxes in the program's windows | ok
- 1-plain-windows/FormCentralUserGroups.png | 31.3689 | the list's flat border on the token background | ok
- 1-plain-windows/FormCentralPatientSearch.png | 30.1702 | Search by group; 14 framed boxes; check boxes in the look; the red Fetching Results line as on main (its own colour) | ok
- 1-plain-windows/FormCentralConnections.png | 28.4171 | Conn. Order and Prefs groups in the program's group box; check boxes in the look; the grid's Fluent scroll bar | ok
- 2-at-150/FormCentralConnections-150.png | 28.3118 | at 150%: bounds and fonts scaled; the looks drawn at the harness's 96 dpi (check boxes, radio circles, frames, group box corners at the 100% size: UI-13 section 4), as in the program they scale with the window's dpi; Conn. Order and Prefs groups in the program's group box; check boxes in the look; the grid's Fluent scroll bar | ok
- 2-at-150/FormCentralPatientSearch-150.png | 27.7179 | at 150%: bounds and fonts scaled; the looks drawn at the harness's 96 dpi (check boxes, radio circles, frames, group box corners at the 100% size: UI-13 section 4), as in the program they scale with the window's dpi; Search by group; 14 framed boxes; check boxes in the look; the red Fetching Results line as on main (its own colour) | ok
- 1-plain-windows/FormCentralManager.png | 26.8645 | the menu bar in the token renderer as on main; Filter Connections and Search by groups; 17 framed boxes; the grids' Fluent scroll bars | ok
- 1-plain-windows/FormCentralConnectionGroups.png | 26.5516 | the grid's Fluent scroll bar; the stock combo box framed with the chevron (disabled grey: no data in the harness) | ok
- 1-plain-windows/FormDatabaseCheck.png | 24.9083 | four framed boxes; the results box framed with Fluent scroll bars | ok
- 1-plain-windows/FormCentralConnectionGroupEdit.png | 21.7279 | the two grids' scroll bars in the Fluent look; the group box text box framed | ok
- 1-plain-windows/FormCentralPatientTransfer.png | 21.6348 | the grids' Fluent scroll bars; the stock buttons in the button look | ok
- 1-plain-windows/FormCentralUserEdit.png | 21.2669 | the old look's 252,253,254 window and pages take the token; the lists' flat border; the Domain User box read-only grey | ok
- 1-plain-windows/FormPrintImage.png | 17.8111 | the three setting groups in the program's group box; the print preview's surround is the preview control's own (document area) | ok
- 1-plain-windows/FormPrintSettings.png | 17.8111 | as FormPrintImage | ok
- 2-at-150/FormCentralManager-150.png | 16.8127 | at 150%: bounds and fonts scaled; the looks drawn at the harness's 96 dpi (check boxes, radio circles, frames, group box corners at the 100% size: UI-13 section 4), as in the program they scale with the window's dpi; the menu bar in the token renderer as on main; Filter Connections and Search by groups; 17 framed boxes; the grids' Fluent scroll bars | ok
- 1-plain-windows/MsgBoxCopyPaste@CodeBase.png | 11.4180 | the text box framed with Fluent scroll bars; Print and OK without the underline (pass 1: underlined) | ok
- 1-plain-windows/GraphIncome.png | 2.0437 | View, Group By and Filter Dates groups in the program's group box; the date pickers framed with the chevron; the chart is content (unchanged) | ok
- 1-plain-windows/FormCentralSecurity.png | 1.0587 | a FormODBase window of the Central Manager: its stock group boxes (Lock Date, Sync Options, Domain Login) now in the program's group box | ok
- 2-at-150/FormCentralSecurity-150.png | 0.7392 | at 150%: bounds and fonts scaled; the looks drawn at the harness's 96 dpi (check boxes, radio circles, frames, group box corners at the 100% size: UI-13 section 4), as in the program they scale with the window's dpi; a FormODBase window of the Central Manager: its stock group boxes (Lock Date, Sync Options, Domain Login) now in the program's group box | ok

Findings of pass 1, all real: (1) UiButtonLook drew a button's image and caption where each alignment alone put them, ignoring
TextImageRelation (DashboardPanelCtrl's Add Col, Add Row); (2) the looks drew the access key's underline always (&OK, &Cancel, Add,
Edit, Drop, Print) where Windows hides it until Alt; (3) the CodeBase progress windows' light grey window (ControlLight) stayed.
