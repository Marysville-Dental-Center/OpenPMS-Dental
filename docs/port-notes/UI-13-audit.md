# UI-13 audit: every picture of G-UI3-ui13, looked at

The lead's rule (as for UI-12, `UI-12-audit.md`): before the owner sees the gallery, every after picture is opened and judged as the owner
would ("does every element look like it belongs to one modern Windows 11 program?"), one line per picture, each finding fixed and redrawn,
until a full pass finds nothing. A picture not opened counts as not audited.

## Pass 1 (build of 2e790bdbb, pictures `G-UI3-ui13-src/a1`)

Style sheet (`a1/styles`, 100% and 150%):
- 01-scrollbars: flat Surface track, pill thumb, chevrons; disabled bar without thumb. Nothing wrong.
- 02-textbox-scroll: the text box's and rich text box's scroll bars in the look. Nothing wrong.
- 03-listbox: the program's list and the stock list scroll in the look. Finding: the stock list box keeps Windows' sunken 3-D edge (dark
  top-left line at 150%). Fixed: UiBorderLook flattens list box, tree view and list view edges.
- 04-treeview-listview: scroll bars in the look; the items in the harness's classic picture (Windows 11's Explorer theme in the program).
- 05-trackbar: the Fluent slider at 100% and 150% (track, Accent fill, round thumb, ticks; the disabled one grey). Nothing wrong.
- 06-buttons: stock button, disabled, swatches and the time pattern's handle in the program's button look. Nothing wrong.
- 07-checkboxes: the stock check box like the program's. Nothing wrong.
- 08-block-label: Ins Rem and DPlan Rem in the button look. Nothing wrong.
- 09-progressbar: rounded track, Accent fill, the disabled one grey. Nothing wrong.
- 10-borders: flat StrokeStrong lines. Nothing wrong.
- 11-monthcalendar: Finding: the calendar cut at the sheet's edge (the sheet measured it before its handle). Fixed in the sheet (laid out after
  the handles).
- 12-datagridview: Finding: the empty area right of the columns stayed Windows' dark grey (its default is ControlDark) and the grid lines
  WindowFrame. Fixed: both defaults recognised.
- 13-linklabel: Accent link. Nothing wrong.
- 14-timepattern: Finding: drawn 30 times too wide with misplaced stripes (the sheet gave the time bar a font, which rescales its panels).
  Fixed in the sheet (no font for the time bar, as in the windows).

Windows (`a1`, the 47 pilot windows; 22 changed against main's pictures, all looked at):
- FormProvEdit (owner row 3): the Specialty list's scroll bar in the look. Finding: the Term Date picker's calendar button lost its glyph
  (UiButtonLook drew the date picker's own borderless flat button as a white swatch; the same in FormPatientEdit's Birthdate). Fixed: a flat
  borderless stock button belongs to its control and is left alone.
- ControlAccount (owner row 4): the aging strip on one grid (label at 8, equal columns, one baseline, separators 4 px both sides, Ins Rem a
  button); the grids' scroll bars flat. Nothing wrong.
- ControlChart (owner row 1): the procedure date slider in the Fluent look; the grids' scroll bars flat. Nothing wrong.
- FormProcCodeEdit (owner row 2): the caption, the column and the Minutes box on one left edge. Finding: the column's first eight cells drew
  no lines (the designer's ScrollValue had moved the inner panel until Load, and the look drew the cells where the panel stood). Fixed: the
  cells drawn where LayoutTables puts them.
- FormApptEdit: Finding: the disabled Lab and Req boxes' scroll bars on a white track inside a grey box. Fixed: a frame bar's track takes the
  box's own colour.
- FormApptViewEdit, FormClaimEdit, FormEmailInbox, FormHL7DefEdit, FormInsBenefits, FormInsPlan, FormMedLabEdit, FormPayment (and its two
  style pictures), FormProcEdit, UserControlApptAppearance, the page bar pictures: only the scroll bars (and the stock buttons) changed, in the
  look. Nothing else wrong.
- The 25 unchanged windows (no scroll bar, stock button or other UI-13 type in their pictures): identical to main's pictures.
- Seen: the up-down boxes' printed corner pixel (FormPayment) is still dark: under investigation (`InputFrame_PrintedCorners_InTheParentColour`).

## Pass 2 (builds of e3b0ef8ee to 5f6e70ca2, pictures `a2` to `a5`, `xa2` to `xa5`: the 47 pilot windows, the 14 windows outside the pilot,
## the style sheets)

- FormProvEdit, FormPatientEdit: the date pickers' calendar buttons draw their glyph again (pass 1's fix seen). FormProvEdit's Appointment
  Color and Highlight Outline Color: Finding: the colour picker's own swatch (with its "no colour" diagonal) drawn over as a plain white
  swatch. Fixed: a stock control inside one of the program's own user controls (OpenPmsDental.UI) is left to that control.
- FormProcEdit: Finding: the two small icon buttons on a grey colour (edit, delete) drawn as grey swatches with their icons. Fixed: a button
  with an icon or a text is a button, not a swatch (now the program's button look with the icon).
- FormApptEdit, UserControlApptAppearance: the colour swatches rounded with a StrokeStrong edge. Nothing wrong.
- FormApptEdit: the disabled Lab and Req boxes' scroll bars on the box's grey (pass 1's fix seen). Nothing wrong.
- Every pilot window with a framed input (36 of the 47 pictures): Finding: the dark corner pixel under each input frame's rounded corner
  (`InputFrame_PrintedCorners_InTheParentColour` reproduced it: (0,0) = 74 grey, the parent white). Cause: the parent's colour was filled with
  antialiasing at the default pixel offset, which covers an edge pixel by half and a corner pixel by a quarter (64 = a quarter of white over
  black in a bitmap). Fixed: the fill on whole pixels. The redrawn pictures: every changed corner group is a 4 x 4 corner (checked by
  `work/classify.py` against main's pictures) and the zoomed corners are the parent's colour.
- FormCodeGroupEdit (stock check boxes with the box on the right), FormPerio (radio buttons with the circle on the right): Finding: Windows'
  classic boxes and circles. Fixed: the box or circle drawn in the look where the window puts it.
- FormQuery: Finding: the data grid view's Gainsboro background (the window's own classic grey). Fixed: Gainsboro and DarkGray read as the
  classic greys and become Surface. Its 1 px dark border is the data grid view's own (no paint hook): kept, noted.
- FormJournal: its two calendars are hidden until the V buttons (not a picture of the type): FormRpAdjSheet draws the month calendar instead.
  The calendar shows Windows' classic picture in the harness with the token colours (title Accent, other months grey); with Windows 11's theme
  the program shows Windows' themed calendar (section 4 of UI-13.md).
- FormAutoNotes (tree view), FormProcButtons (list view), FormEtransEdit (rich text boxes), FormFamilyBalancer (progress bar, disabled output
  box), FormPrintTrojan (scroll bar control), FormEHR (stock buttons), FormClaimFormEdit (picture box), FormDatabaseMaintTemp (link label,
  stock buttons), FormEmailMessageEdit (stock list box), FormApptTypeEdit (the time pattern): opened; each type in its look. Nothing else wrong.
- Style sheet: 14-timepattern: Finding: still 440 px wide (a TableTimeBar's own layout leaves its designer panels' width; every window's
  designer gives it 15 x 561). Fixed in the sheet (the designer's size). 12-datagridview at 150%: Finding: the header text cut (the sheet's
  font at 150% without the header's height). Fixed in the sheet (header height from its font).

