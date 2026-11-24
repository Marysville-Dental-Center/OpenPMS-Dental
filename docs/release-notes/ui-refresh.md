# The new look: release notes of the UI refresh

Open PMS - Dental is a fork of Open Dental (c) Open Dental Software, Inc. and contributors, licensed under the GNU GPL v2. It is not affiliated with or endorsed by Open Dental Software, Inc.

For the office. This version gives every window of the program a calm, modern look. Nothing has to be converted or set up: the look
comes with the update, and every setting, colour and picture the office chose stays as it is. The technical record is
`docs/specs/ui-refresh-plan.md` and the notes it names.

## What changed in the look

- **Text.** Every window uses Segoe UI 9 pt, Windows' own font and size, in place of Microsoft Sans Serif 8.25 pt. Text that no longer
  fitted was given room in place, window by window, so no caption is cut off.
- **Windows.** The title bar is the program's blue with white text, the window background a quiet light grey, and every window, dialog
  and message box of the program draws the same frame. The small tools that come with the program (Central Manager, MySQL Manager, the
  database tools, the service manager, the update file copier, the report designer and the dashboard's dialogs) have the same look.
- **Buttons, boxes and lists.** Flat buttons with thin borders and small rounded corners; the main button of a window (Save, OK) filled
  blue. Text boxes, drop-down boxes, date and number boxes share one framed box, with a blue line while you type in it. Check boxes,
  radio buttons, group boxes, tabs, lists, scroll bars, sliders, progress bars and calendars are drawn in the same style.
- **Grids.** Flat grids with light lines; the grid titles in the program's blue; rows follow the text (17 px at 100%), so a grid shows
  about a fifth fewer rows than before.
- **Toolbars and menus.** Flat light-grey toolbars and menus, the module bar on the left in the same style.
- **Icons.** Sharp Microsoft Fluent icons in dark grey, with a few colours for their role: red for delete, green for complete, amber for a
  warning, blue for the main action. They stay sharp at every Windows scaling (125%, 150% and more).
- **New databases** get new-style pictures on the eight default procedure buttons. The pictures an office chose never change.

## What changed in the layout

No window was replaced and nothing was removed. Inside each window the controls now sit on one grid: 8 px from the edges and between
groups, labels right-aligned just before their boxes, the boxes of a column on one left edge, one height for every one-line box, the
window's buttons at the bottom right. Some windows grew a little so that everything fits; none shrank.

## What stayed as it was

- **Office content**: sheets, claim forms, eForms, statements, the dashboard's widgets, the appointment book's text and colours, the
  colours set in Setup (Definitions, Misc Colors), and the pictures the office added.
- **Printouts and PDFs** look as before: printed grids, reports, statements, claim forms and schedules.
- **The tooth chart** and the perio chart's grid are drawn as in Open Dental's newest release.
- **Windows' own windows**: the file pickers, the print dialogs, Windows' message boxes and the inside of Windows' calendars keep Windows'
  look.

## New keyboard behaviour

- **The » button on toolbars.** When a window is too narrow for its toolbar, the buttons that do not fit go into a » button at the
  toolbar's right end instead of being cut off. Click it (or press Enter on it) to reach them; each does what its button does.
- **Toolbars from the keyboard.** Tab reaches a toolbar; the arrow keys move between its buttons, Home and End go to the first and the
  last, Enter or Space clicks, Down opens a button's menu. Screen readers announce each button.
- **Tab in the newer windows.** In the windows built with WPF (the Imaging module, the X-ray capture window and many newer dialogs) Tab now
  also stops on buttons, check boxes and radio buttons, as in the other windows, and moves in reading order (top to bottom, left to right)
  unless the window sets its own order. Enter or Space presses the button that has the focus; Space ticks a check box.
- **Radio buttons.** A group of radio buttons is one Tab stop in every window: Tab lands on the chosen one (or the first, when none is
  chosen) and the arrow keys move the choice inside the group.
- **The focus ring.** A thin blue ring shows which control has the keyboard focus, only after a key is pressed; a mouse click shows none.
- **Access keys.** The underlined letters show when you hold Alt, as in Windows.

## Still open

- **The owner's checks at 125% and 150%.** The look was checked in pictures at 100% and 150%; the owner's own check on a desktop set to
  125% and 150% scaling is still to come.
- **Pictures of the WPF windows' new keys and » button.** The pictures of the Imaging toolbar's » button and of the WPF focus ring are
  drawn on a connected desktop session; they are listed in `docs/port-notes/UI-11-final.md` until then.
- The items the final sweep could not settle are listed in `docs/port-notes/UI-11-final.md` with a picture each.
