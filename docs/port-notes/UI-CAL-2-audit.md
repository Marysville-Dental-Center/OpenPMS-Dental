# UI-CAL-2 audit: gallery G-UI3-uical2

Every picture of `.git/od-tools/test-results/G-UI3-uical2` looked at before the owner sees it ([UI-CAL.md](UI-CAL.md) section 8). The
harness draws at 96 dpi with Windows' visual styles and the program's font; nothing on screen. Before: the base `642322a74` drawn as its
150% screen shows it (the calendars' fonts scaled as Windows' dpi drew them); after: `ui/uical2-1`.

## Round 1

- dropdowns-100 (6, the same): FormPayment, FormTaskSearch, FormRpNetProdDetail, FormIncomeTransferManage, GraphIncome, ControlTreat. Pixel
  for pixel the before pictures: the font in pixels is the same 12 px as 9 pt at 96 dpi. OK.
- dropdowns-150, GraphIncome (the owner's picture): before, the 227x162 calendar with the picker's 150% text squeezed into it (the
  chevrons over the letters, the rows touching, the Today line cut at the bottom); after, the 341x243 calendar (the plain window's 150%)
  with its text at 150%, every row, the header and the Today line in their places as at 100%. OK.
- dropdowns-150, the five program windows: before, the 150% box with 150% text whose places were scaled twice (the rows' text high in
  their cells, the month's name high, the Today line left of centre); after, centred as at 100%. FormTaskSearch's calendar reaches past
  the window's right edge, so the picture grows (the drop-down is a window of its own); on a screen `GetDropBounds` moves it left inside
  the working area (tested, not drawn). OK.
- windows-150 (9): FormCalendar, FormJournal#calendars, FormXChargeReconcile, FormRpPaySheet, FormRpWriteoffSheet, FormRpAdjSheet,
  FormRpAccountingGenLedg, FormRpAccountingBalanceSheet, FormRpUnearnedIncome#lineitem. Before, the inline calendars' text high in the
  cells and the Today line off centre, as above; after, each calendar is its 100% picture scaled. The UI-CAL gallery drew these with 100%
  text in the 150% box (its harness did not scale MonthCalendarOD's font); neither picture was what a 150% screen showed. OK.
- All pictures: the Y and M letters sit a little below the chevrons beside them, and the selected day's digits a pixel above its fill's
  centre, at 100% as at 150% (UI-4b's design, unchanged here). Not a finding of this package.

No fix round was needed.
