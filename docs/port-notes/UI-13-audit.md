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

## Pass 3 (builds of d6bc083e1 to 99fbf774b, pictures `a7` to `a9`, `xa9`, `ya9`; the gallery G-UI3-ui13 is drawn from `a9`)

- FormProvEdit: the colour pickers' own swatches with their "no colour" diagonal (pass 2's fix narrowed: a stock button its owner paints keeps
  its owner's picture; a stock button inside any other control takes the look). FormProcEdit: the signature box's two icon buttons in the
  program's button look. Nothing wrong.
- 14-timepattern (100%, 150%): Finding (a7): the letters stood a row off and part of them were missing (GDI text drawn by the inner panels
  ignored their move of the picture; the cells were placed by the panels' place, which a window lays out only on Load). Fixed: the cells from
  the time bar's own layout rule, the letters in GDI+ after every fill. In `a9` every cell shows its letter (X white on Accent, / on
  AccentSubtle), the handle under the last cell. Nothing wrong.
- The 47 pilot windows: `a9` against `a8`: identical but FormProcCodeEdit's empty column (its lines one pixel up, on the cells' laid-out
  place). Opened again: ControlAccount, ControlChart, FormProcCodeEdit, FormProvEdit (the owner's rows), FormProcEdit, FormApptEdit. Nothing
  wrong.
- The 14 windows outside the pilot (`xa9`): identical to `xa7`, opened in pass 2. Nothing wrong.
- The style sheet at 100% and 150% (`a9/styles`, 28 pictures): opened (01 to 14). Nothing wrong; the tree view's lines and the list view's
  missing header are the harness's classic picture (no visual styles in the test process; Windows 11's Explorer theme in the program).

This pass finds nothing beyond the recorded limits (UI-13.md section 4).

## Pass 4, fix round (the review's eight findings; pictures `f2` (pilot windows), `xf4`/`yf4` (windows outside the pilot, after and before,
## with Windows' visual styles), `sf3`/`sb3` = `xf4/styles`/`yf4/styles` (style sheets); the gallery G-UI3-ui13 is redrawn from them)

The review's findings and what the pictures show now:
- 1 (the looks drawn at 96 dpi size on a 150% monitor): 05-trackbar, 06-buttons, 07-checkboxes, 08-block-label, 09-progressbar at 150%
  are drawn through a FormODBase set to 1.5 (SetScaleMS, the program's dpi path) with no test zoom: the slider's thumb, track and ticks, the
  buttons' corners and padding, the check boxes' 18 px box and gap, the blocks and the progress bar's radius are 1.5 times, the same pixels
  the test zoom gave (05, 08 and 09 identical to pass 3), now from the window's scale. The program's check box (UI.CheckBox) in 07 gets the
  window's LayoutManager as LayoutManagerForms passes it, and matches the stock one. Nothing wrong.
- 2 (the data grid view's classic header bevel and black frame): 12-datagridview at 100% and 150%: the header SurfaceSubtle with 1 px
  Gridline separators and no bevel, the frame StrokeStrong. FormQuery (now with sample columns and rows): the same. Nothing wrong; the row
  header's current-row arrow is the grid's own marker (behaviour).
- 3 (button captions cut on a second line): FormEHR: "Measure Calc", "Quality Meas", "Not Performed", "Clinical Summ." on one line, centred,
  inside their frames. 06-buttons: one line. Nothing wrong. Seen, kept: FormEHR's top text ("Provider for this patien", the paragraph's third
  line under the grid title) is cut in the before picture too: the window's designer layout, outside the pilot (UI-13 changes none of its
  designer files).
- 4 (the time pattern at 150%): 14-timepattern-150 is FormProcCodeEdit at 1.5 by its LayoutManager: cells 21 px, the column 20 px, the
  letters and the frame scaled, the handle 18 x 22 a rounded swatch (not an oval) under the last cell; the caption over the column and the
  Minutes box under it scaled with the window. 14-timepattern-100: unchanged cells (14 px), the window's crop. Nothing wrong.
- 5 (the tree and list views in the classic picture): 04-treeview-listview and FormAutoNotes (sample groups and notes) and FormProcButtons
  (sample category and buttons) drawn with visual styles: no dotted lines, the Explorer theme's flat glyph, the Explorer selection (light
  grey) on the selected note and button. The list view's column header is not printed by the harness (a native child; before and after
  alike, clip note). Nothing wrong.
- 6 (the month calendar's bevelled arrows and red ring): 11-monthcalendar and FormRpAdjSheet with visual styles: Windows' themed calendar
  (flat arrows, today in a blue frame, no red ring); in FormRpAdjSheet the Today line clears "Adjustment Types" below it. The 150% sheet
  shows the calendar at 96 dpi (clip note, UI-13.md section 4). Nothing wrong.
- 7 (FormInsPlan's black rule): a 1 px Stroke line at y 128 on the window's colour (y 129 the window's colour). ControlAccount's aging strip
  separators and FormInsBenefits' dividers (the same kind of panel) are now the same 1 px Stroke line. Nothing wrong.
- 8 (FormQuery's ActiveCaption caption): "Query results" in Accent with TextOnAccent in the title font, as the grids' titles. Nothing wrong.

Every after picture against pass 3: the 47 pilot windows identical but ControlAccount, FormInsBenefits, FormInsPlan (the dividers) and
ControlChart-pagebar-150 (the page bar at the window's scale); each opened. The windows outside the pilot: FormAutoNotes, FormEHR,
FormFamilyBalancer (date pickers and output box themed by the visual styles; its cut "Deletes all transfers ..." text is in the before
picture too, the window's own layout), FormProcButtons, FormQuery, FormRpAdjSheet changed; each opened; the other eight identical to pass 3.
The style sheets: 03 (the stock list themed; the program's list at 150% keeps its own scale in the sheet, which lays it out without a full
LayoutManager layout, as in pass 3), 04, 06, 07, 11, 12, 14 changed; each opened; 01, 02, 05, 08, 09, 10, 13 identical to pass 3.

This pass finds nothing beyond the recorded limits (UI-13.md section 4).
