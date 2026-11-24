# UI-CAL audit: gallery G-UI3-uical

Every picture of `.git/od-tools/test-results/G-UI3-uical` looked at before the owner sees it ([UI-CAL.md](UI-CAL.md) section 5).
The harness draws at 96 dpi with Windows' visual styles and the program's font; nothing on screen.

## Round 1 (first after pictures) and its fixes

| Picture | Finding | Fix |
|---|---|---|
| FormRpPaySheet | The second calendar over the first's Saturdays and under the Providers list (the before picture has the same with Windows' wider calendar) | date2 to x 238; the Providers column to x 473, its list and label 150 px |
| FormRpWriteoffSheet | date2 under the Clinics column; TO hidden under date1 | date2 to 266, TO to 237 between the calendars; the Clinics column to 501, 170 px wide |
| FormRpPHRawPop | TO under date1; date2 touching the adjustment types | date2 to 266, TO to 237; the label and list to 501 |
| FormRpActivePatients, FormRpBrokenAppointments, FormRpPPOwriteoffs | TO under the first calendar | the second calendar to 266, TO to 237 |
| FormRpPHRawScreen | TO under date1 | date1 to 8, date2 to 266, TO to 237 |
| FormRpAdjSheet, FormRpProcSheet | TO under date1 (a gap was there) | TO to 258 and 256, in the gap |
| FormRpPresentedTreatmentProduction, FormRpTreatPlanPresentationStatistics, FormXChargeReconcile | the two calendars overlapping | date2 to 255, 255 and 243 |
| the drop-down pictures | the first run found no picker (a hidden window's pickers are not Visible) | the gallery names each window's picker |

The grid rules (`GridWindows_FollowTheRules`) found these 20 overlaps; after the fixes none, with no new row in grid-known.tsv.

## Round 2 (the pictures of the gallery)

- windows-100, 16 changed: FormCalendar, UserControlTasks, FormXChargeReconcile and 13 report windows show MonthCalendarOD (header
  with the year and month chevrons, the selected day in Accent with today's ring, the other months' days in TextDisabled, the Today
  footer) where Windows' themed calendar was; nothing overlaps; the TO labels show between the calendars. OK.
- windows-100, 26 the same: the windows with only date pickers (the closed picker is unchanged), the accounting reports (MonthCalendarOD
  already), FormJournal (its calendars hidden until its date button is clicked), FormRpUnearnedIncome (its calendars on a page the
  picture does not show). OK.
- UserControlTasks: the calendar's last row is under the tree below it, in the before picture too: the control moves the tree under the
  calendar in code when it shows its calendar (`LayoutManager.MoveLocation(tree,...)`), which the harness does not run. Harness only.
- windows-150 (FormPayment, FormRpPaySheet, FormRpWriteoffSheet, FormRpUnearnedIncome, UserControlTasks): the calendars' boxes, cells and
  arrows at 150%; their text is drawn at the 96 dpi font, small in its cells, as MonthCalendarOD's in the earlier galleries: the harness
  scales the fonts of the stock controls Windows scales (`ScaleFontsAsWindowsDoes`), and on a 150% screen MonthCalendarOD's font in points
  is drawn at that dpi (the drop-down pictures, in the picker's scaled font, show it). Harness only.
- dropdowns-100 and dropdowns-150 (FormPayment, FormTaskSearch, FormRpNetProdDetail, FormIncomeTransferManage, GraphIncome, ControlTreat):
  the calendar below its picker's left edge on the picker's date, in the window's scale and the picker's font. Where it reaches past the
  window (FormTaskSearch at 150%) the picture grows; on screen the drop-down is its own window and ToolStripDropDown keeps it on the
  screen. No shadow in the pictures (Windows draws the drop-down's). OK.

Nothing left to fix. Open for the owner's desktop check: the drop-down at the screen's edges, on a second monitor, at 125% and 150%.
