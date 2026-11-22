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

