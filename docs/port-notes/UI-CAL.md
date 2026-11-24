# UI-CAL: every calendar in the new style

The owner (2026-11-24): "Can you make all of calendars have the new style?" UI-11-final had left two kinds of calendar in Windows' look:
the stock `System.Windows.Forms.MonthCalendar` (33 in 18 windows; UI-13 gave them token colours, which Windows 11's themed calendar
ignores) and the drop-down calendar of the stock `DateTimePicker` (41 in the designer files, 2 of them in the up-down style without a
calendar). Now every calendar the program shows is `MonthCalendarOD` (docs/specs/ui-refresh-plan.md section 3.6, [UI-4b.md](UI-4b.md)).
Branch `ui/uical-1` from `6bd8e0d64`. Gallery G-UI3-uical, audit [UI-CAL-audit.md](UI-CAL-audit.md).

## 1. MonthCalendarOD takes over Windows' calendar

`OpenPmsDental/UI/MonthCalendarOD.cs` gained what the windows used of the stock calendar (the members were listed from every window's
code: `SelectionStart`, `SelectionEnd`, `SetDate`, `MaxSelectionCount`, `MinDate`, `DateSelected`, `DateChanged`, `Visible`):

- `SelectionStart`, `SelectionEnd`, `SelectionRange`, `SetDate`, `SetSelectionRange` with Windows' rules (a start after the end moves the
  end, a range longer than `MaxSelectionCount` is cut, the time of day dropped, the month shown).
- `MaxSelectionCount` (default 1, so the existing calendars keep one day): a drag over the days and Shift with a click select a range.
  The converted windows that left Windows' default of 7 have `MaxSelectionCount = 7` in their designer files, so a range still works there.
- `MinDate`, `MaxDate`: the days outside are drawn as the other months' days, cannot be clicked, hovered or reached with the keyboard; a
  selection outside moves inside.
- `DateSelected` (`DateRangeEventHandler`) on the release of the mouse button that chose a day or Today, or after a date typed by the
  month's name, as Windows raises it. `DateChanged` stays an `EventHandler` raised on each change by the user (now also the keyboard's and a
  drag's); neither is raised for the program's changes. FormCalendar's handler took the new signature and reads the selection that
  `MinDate` moved (Windows raised `DateChanged` for that).
- The keyboard (`AllowKeyboard`, off by default so the appointment module's calendar and ODDatePicker's keep their behaviour): the arrows
  by a day or a week, Page Up and Page Down by a month, Home and End to the month's first and last day, Shift extends the range; a click
  gives the calendar the focus. Enter and Tab stay the window's (its OK button).
- A range is drawn with every day as the selected day; the focus ring is on the day the keyboard moves.

The 33 stock calendars (FormCalendar, FormJournal, FormXChargeReconcile, UserControlTasks and the report windows FormRpActivePatients,
FormRpAdjSheet, FormRpBrokenAppointments, FormRpInsOverpaid, FormRpPaySheet, FormRpPHRawPop, FormRpPHRawScreen, FormRpPPOwriteoffs,
FormRpPresentedTreatmentProduction, FormRpProcSheet, FormRpReceivablesBreakdown, FormRpTreatPlanPresentationStatistics,
FormRpUnearnedIncome, FormRpWriteoffSheet) are `MonthCalendarOD`, 227x162 (its own size; Windows sized its calendar itself, wider than
the designer files said: 247x175 in the before pictures), with the keyboard on. The window code is unchanged but FormCalendar's.

**Fit.** At 227x162 the calendars overlapped their neighbours in 12 windows, as Windows' wider calendars already did on screen (the
before pictures: FormRpPaySheet's second calendar over the first's Saturdays and under the Providers list, the TO labels hidden). The
second calendar, the TO label and the columns beside them moved so nothing overlaps (FormRpActivePatients, FormRpAdjSheet,
FormRpBrokenAppointments, FormRpPaySheet, FormRpPHRawPop, FormRpPHRawScreen, FormRpPPOwriteoffs, FormRpPresentedTreatmentProduction,
FormRpProcSheet, FormRpTreatPlanPresentationStatistics, FormRpWriteoffSheet, FormXChargeReconcile); FormRpWriteoffSheet's Clinics and
FormRpPaySheet's Providers lists are narrower (170 and 145 px). `GridWindows_FollowTheRules` passes with no new row in grid-known.tsv; the
layout snapshot was written again (`OPMS_UI_LAYOUT_SNAPSHOT_WRITE=1`).

Fix round (`ui/uical-2`, the review's findings): FormJournal's calendarTo to (289,62), 8 px right of calendarFrom (at 208 it hid
calendarFrom's Thursday to Saturday when the date button shows both); FormRpPaySheet's date2 to x 243 (an 8 px gap), its Providers column
(label1, listProv, checkAllProv) to x 478 and 145 px wide (right edge 623 kept), checkUnearned to (514,28) and 117 px wide, between
checkAllProv and checkAllClin; the TO label of FormRpProcSheet, FormRpAdjSheet, FormRpPHRawPop and FormRpPHRawScreen `MiddleCenter`
and centred vertically on the calendars (top + 69), as in the other report windows. The layout snapshot rows were changed with them.

## 2. The date pickers' drop-down

`CodeBase/UI/UiDateDropLook.cs` (a `NativeWindow` on each stock `DateTimePicker`, attached by `UiLooks.AttachOne` with the other looks, so
every window that gets the looks gets it; linked into RdlDesign and UpdateFileCopier as the other look files): a click on the drop button
(`DTM_GETDATETIMEPICKERINFO`'s `rcButton`), F4 and Alt+Down (Alt+Up) never reach Windows, which would open its calendar; the program's
calendar opens instead in a `ToolStripDropDown` below the picker (at its right edge with `DropDownAlign.Right`, above it when the screen has
no room below), and a second click on the button closes it. The calendar is made by the program (`LayoutManagerForms.CreateDropCalendar`,
given as `UiDateDropLook.FuncCreateCalendar` in `SetLooksScale`, as CodeBase cannot reference MonthCalendarOD; it implements CodeBase's
`IUiDropCalendar`): at the scale of the picker's window (`FormODBase.GetLayoutManager`, new), in the picker's font, the keyboard on,
without the month name's date box (the picker is the box to type in), its days limited to the picker's `MinDate` and `MaxDate`.

A day clicked, or Enter on the day the arrows moved to, becomes the picker's date with its time of day kept and its check box checked;
Escape or a click elsewhere closes without a change. The picker raises `DropDown`, `ValueChanged` (when the date changed or the box was
checked, as Windows' picker does) and `CloseUp` in Windows' order, through its protected `On...` methods. Typing in the box, the check
box, the custom formats and the up-down style (no calendar) are Windows' as before. Without `FuncCreateCalendar` (a plain exe) Windows'
drop-down stays; the plain exes have no date picker or calendar (no `DateTimePicker` or `MonthCalendar` outside OpenPmsDental and
OpenPmsDentalGraph, whose GraphIncome and GraphQuantityOverTime pickers get the drop-down in the program).

## 3. The other calendars

- The appointment module's calendar, ODDatePicker's and the accounting reports' were MonthCalendarOD already (UI-4b); unchanged.
- WPF: `WpfControlsOD/UI/Controls/MonthCalendar.xaml` and the WPF date picker are in the new style since UI-4b; no stock WPF calendar is
  used (`System.Windows.Controls.Calendar` and `DatePicker` appear in no window).
- `UiLooks.SetMonthCalendarLook` and `SetDateTimePickerLook` stay (a stock calendar a later port brings in, a picker without the program).

## 4. Tests

- `FinalSweepTests`: rule `stock-calendar`, a stock `MonthCalendar` or a date picker (not up-down) whose drop-down is not the program's
  calendar (`UiDateDropLook.IsProgramCalendar`); none found over the 1,030 swept types (`sweep-final.tsv`).
- `CalendarBehaviourTests` (UiRefreshTests, 12 tests): the selection and range rules, MinDate and MaxDate, a click's events (DateChanged on
  the press, DateSelected on the release), a drag, the keyboard on and off, Shift ranges; the picker's drop-down on the picker's date and
  MinDate, a chosen day with its time of day and the events' order, the click and Enter and Escape in the drop-down, the check box, the
  up-down style, F4 and Alt+Down on a picker's window (Windows' calendar not open, `DTM_GETMONTHCAL`).
- `CalendarDropGallery` (opt-in `OPMS_UI_CAL_DROP_GALLERY`): pictures of the drop-down open in six windows.
- coverage.tsv rows for `UiDateDropLook` and MonthCalendarOD's UI-CAL; NoHqTests' package list has UI-CAL; the element inventory's
  calendar rows (`element_inventory.py`) written again.
- WindowChecksTests (fix round): C6 sets FormRpPHRawPop's dates on `MonthCalendarOD` (it cast them to Windows' calendar); C22 looks for
  `MonthCalendarOD` in FormRpPHRawPop (`C22-monthcalendarod.png`) and opens FormRpPayPlans' first picker's drop-down with
  `UiDateDropLook.OpenForTest`, checks it holds `MonthCalendarOD` and pictures it (`C22-datetimepicker-dropdown.png`); its label and
  WINCHK-HEADLESS.md say so. Both are opt-in window checks (`OPMS_WINDOW_CHECKS=1` with a dump), skipped in a Targeted run.
- The picker tests that make a picker's window use Windows' visual styles (`VisualStylesScope`), as the program runs: a picker window
  made earlier without them registered comctl32 version 5's class and failed ElementLookTests' themed picker test in the first run.

## 5. Gallery G-UI3-uical

`.git/od-tools/test-results/G-UI3-uical` (zip beside it), 61 pictures: windows-100 (the 42 windows with a calendar or a date picker,
and since the fix round FormJournal-calendars and FormRpUnearnedIncome-lineitem, the gallery states `FormJournal#calendars`
(ToggleCalendars, as its date button) and `FormRpUnearnedIncome#lineitem` (the Line Item page); before from `6bd8e0d64` with the fix
round's FitGallery.cs, after from this branch; 18 changed), windows-150 (the densest five, after only), dropdowns-100 and
dropdowns-150 (six windows with the drop-down open, after only: Windows' drop-down is a window of its own the harness cannot draw).
`<W>` is `.git/od-tools/test-results/G-UI3-uical-src`.

```
odbuild.ps1 -Root . -WaitMinutes 20                    (the base 6bd8e0d64, before any change)
OPMS_UI_FIT_GALLERY=<W>/before-100 OPMS_UI_FIT_GALLERY_FONT=program OPMS_UI_FIT_GALLERY_VISUAL_STYLES=1 OPMS_UI_FIT_GALLERY_WINDOWS=<the 42>
  run-tests.ps1 -Mode Targeted -Platform x64 -Include UiRefreshTests
the branch's build, then the same into <W>/after-100 with OPMS_UI_CAL_DROP_GALLERY=<W>/drop-100,
and OPMS_UI_FIT_GALLERY_SCALE=1.5 into <W>/after-150 and <W>/drop-150
tools/New-UiGallery.ps1 -Before <W>/gal-before -After <W>/gal-after -Name G-UI3-uical -Out .git/od-tools/test-results/G-UI3-uical
```

## 6. Results

Full build through the lane: 0 errors. Targeted x64 run (`.git/od-tools/test-results/uical-final.txt`, 2026-11-24): UiRefreshTests 361
passed, 49 skipped; WindowChecksTests 31 passed, 44 skipped; UnitTests 1728 passed, 3 skipped; PrintGuardTests 19 passed, 1 skipped;
NoHqTests 130 passed; P262BillingTests 54 passed; 0 failed. `tools/Test-NoEolFlips.ps1 -Base 6bd8e0d64`: 0 flips.
`python tools/UiFit/element_inventory.py --check` and `python tools/UiFit/colour_literals.py --check`: match.

Fix round (`ui/uical-2`): full build through the lane, 0 errors. Targeted x64 run (`.git/od-tools/test-results/uical2-final.txt`,
2026-11-24): UiRefreshTests 360 passed, 50 skipped (the opt-in drop-down gallery not asked for); WindowChecksTests 31 passed, 44 skipped;
UnitTests 1728 passed, 3 skipped; PrintGuardTests 19 passed, 1 skipped; NoHqTests 130 passed; P262BillingTests 54 passed; 0 failed.

## 7. Open

- The owner's look at the gallery, and on a desktop: the drop-down's position at the screen's edges and on a second monitor, at 125% and
  150% (the harness draws no screen).
- A window-checks session for C6 and C22 (fix round): it starts the program on a `winchk_` copy of a dump, which this package's rules did
  not allow; the coordinator's or the owner's next window-checks run confirms them.

## 8. UI-CAL-2: the calendar at 150% is the 100% calendar scaled

The owner's review of G-UI3-uical ([UI-CAL-owner-review.tsv](UI-CAL-owner-review.tsv)): "dropdowns-150/GraphIncome-drop-150.png: calendar
too compressed. Don't look right". Branch `ui/uical2-1` from `642322a74`. Gallery G-UI3-uical2, audit [UI-CAL-2-audit.md](UI-CAL-2-audit.md).
UserControlTasks' two rows of the review (the calendar cut off) are slice 14's, which rewrites its layout code; not touched here.

**What a 150% screen showed.** MonthCalendarOD sized its box, cells, header and footer by its LayoutManager (`ScaleMy`: the monitor's dpi
times the zoom) but drew its text in its `Font`, in points, which GDI+ draws at the graphics' dpi: on a screen Windows' system dpi (the
primary monitor's at sign-in, whatever monitor the window is on; the program is per-monitor aware), 96 in a picture. So:

| Host | Before UI-CAL-2 on a 150% screen | Before in the gallery |
|---|---|---|
| Program window (FormODBase), inline or drop-down | box x1.5, text x1.5 through the system dpi; the month's name, the Today line and the rows' text placed by measures scaled twice (`ScaleMS` of a measure already at 144 dpi): the text high in its cells, the Today line off centre | inline: text at 100% in the 150% box (the harness scaled only the stock controls' fonts); drop-down: as the screen |
| Plain window (GraphIncome, GraphQuantityOverTime in the dashboard's plain windows) | the drop-down's LayoutManager was a bare one (no window): the 100% box, with the picker's 150% text squeezed in (the owner's picture) | as the screen |
| A program window moved to a monitor whose dpi is not the system dpi | box at the monitor's scale, text at the system dpi's | not drawn |
| WPF-hosted | none: no WinForms date picker or MonthCalendarOD sits in a `WindowsFormsHost` (the hosts hold the signature box and the progress ring); the WPF calendar (`MonthCalendar.xaml`) is scaled by WPF | |

**The fix.**

- `MonthCalendarOD.GetFontDraw`: the text is drawn (and measured) in a font in pixels, its size the calendar's 96 dpi font size times
  `LayoutManager.ScaleMy()`, so every part grows by the one factor on every monitor and in a picture; the `ScaleMS` of the measures is gone
  (they are at the scale already). The 96 dpi size (`GetFontSize96Drawn`): `FontSize96` when set, else the size the window's LayoutManager
  recorded for the calendar (`LayoutManagerForms.GetFontSize96`, the designer font mapped to the body token), else its `Font` as
  LayoutManager sets fonts (the zoom only). At 100% the font is the same 12 px as 9 pt at 96 dpi: the 100% pictures are pixel for pixel the
  same (dropdowns-100: 6 the same).
- `LayoutManagerForms.CreateDropCalendar`: in a program window the window's LayoutManager and the picker's recorded 96 dpi size; in a plain
  window a LayoutManager of the picker's monitor dpi (`GetScalePlain`: `DeviceDpi/96`) without the zoom (`IsZoomOff`, new: Windows scales a
  plain window by its dpi and nothing zooms it), and the picker's font as Windows draws it brought back to 96 dpi (`GetFontSize96Plain`: its
  pixels at the system dpi over the window's scale, so a 9 pt font at 144 dpi in a 150% window and the 6 pt font Windows Forms gives it on a
  100% monitor are both 9 pt). The calendar is made at each opening, so a window moved to another monitor opens it at that monitor's dpi.
- Placement: `UiDateDropLook.GetDropBounds` (the picker's bounds on the screen, the calendar's size, the working area of the picker's
  monitor): below the box at its left edge (right edge with `DropDownAlign.Right`), above it when there is no room below and there is above,
  as low as it fits when neither; moved left at the screen's right edge and right at its left edge. Never past the working area. Before,
  ToolStripDropDown alone kept it on the screen sideways.

**The harness.** `ScaleFontsAsWindowsDoes` scales MonthCalendarOD's font too, as Windows' dpi drew it before UI-CAL-2 (a build of main
draws what its screen shows; the calendar now ignores it and draws from its 96 dpi size). `CalendarDropGallery` gives a plain window's
picker the picture's scale (`LayoutManagerForms.FuncGetScalePlainForTest`, set through reflection so the file also builds against main).
`TextCalendarControls.RenderWinFormsCalendar` sets `FontSize96` instead of scaling the font.

**Tests** (`CalendarScaleTests`, UiRefreshTests, 7 tests): MonthCalendarOD at 100%, 125%, 150% and 175% against the 100% one scaled (its size
within a pixel; its text's pixel size exactly; the ink of each weekday and date cell, the month's name and the Today line where the 100%
ones are times the scale and as large, within one 100% pixel scaled per cell: each size's hinting rounds the glyphs to its own pixels, a
cell's width within a pixel per character, the long texts within 5%); the same for the drop-down of a program window (the picker's font
grown as Windows draws it; the calendar keeps the 96 dpi size) and of a plain window; the plain window's font brought back to 96 dpi on
either monitor; the placement rule at the edges, on a short screen and on a second monitor at negative coordinates, and for every place of
a picker on a 1920 x 1040 working area (wholly on the screen, against the box whenever there is room). coverage.tsv: the two rows say
UI-CAL-2.

**Gallery G-UI3-uical2** (`.git/od-tools/test-results/G-UI3-uical2`, zip beside it), 21 pictures, 15 changed: dropdowns-100 (6, the same),
dropdowns-150 (6, changed) and windows-150 (FormCalendar, FormJournal#calendars, FormXChargeReconcile, FormRpPaySheet, FormRpWriteoffSheet,
FormRpAdjSheet, FormRpAccountingGenLedg, FormRpAccountingBalanceSheet, FormRpUnearnedIncome#lineitem: 9, changed). Before: the base
`642322a74` with this branch's CalendarDropGallery.cs and GridStyleGallery.cs (so main is drawn as its screen shows it); after: this branch.
`<W>` is `.git/od-tools/test-results/G-UI3-uical2-src`.

```
OPMS_UI_CAL_DROP_GALLERY=<W>/<side>-drop-100 OPMS_UI_FIT_GALLERY_FONT=program OPMS_UI_FIT_GALLERY_VISUAL_STYLES=1
  run-tests.ps1 -Mode Targeted -Platform x64 -Include UiRefreshTests
the same with OPMS_UI_FIT_GALLERY_SCALE=1.5, OPMS_UI_CAL_DROP_GALLERY=<W>/<side>-drop-150, OPMS_UI_FIT_GALLERY=<W>/<side>-150 and
  OPMS_UI_FIT_GALLERY_WINDOWS=<the nine windows above, full names>
tools/New-UiGallery.ps1 -Before <W>/gal-before -After <W>/gal-after -Name G-UI3-uical2 -Out .git/od-tools/test-results/G-UI3-uical2
```

**Results.** Full build through the lane: 0 errors. Targeted x64 run (`.git/od-tools/test-results/uical2b-final.txt`, 2026-11-24):
UiRefreshTests 380 passed, 52 skipped; WindowChecksTests 31 passed, 44 skipped; UnitTests 1728 passed, 3 skipped; PrintGuardTests 19
passed, 1 skipped; NoHqTests 130 passed; P262BillingTests 54 passed; 0 failed. `tools/Test-NoEolFlips.ps1 -Base 642322a74`: 0 flips.
`python tools/UiFit/element_inventory.py --check` and `python tools/UiFit/colour_literals.py --check`: match.

**Open.** The owner's look at the gallery, and on a desktop: a drop-down at 150% in a graph's window and in a program window, one near the
screen's bottom and right edges, and a window moved to a monitor of another scale (the harness draws no screen).
