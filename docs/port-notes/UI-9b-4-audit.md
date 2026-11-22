# UI-9b-4 fix: the audit of gallery G-UI3-ui9b4-v2

The audit the owner asked for before a gallery reaches him (2026-11-21: "do a round of audit and fix first"). Every after picture of
`.git/od-tools/test-results/G-UI3-ui9b4-v2` was read, at its own size and, where a detail mattered, cropped and enlarged 2 to 5 times. The fix and
its causes: `docs/port-notes/UI-9b.md`, section 5.8.

Checked in every toolbar picture: every button of the list is in the picture (none cut, none missing); every icon in the same box, with the same
optical size as its neighbours (the 32 px designs no larger than the scaled 24 px ones); stroke weight even across the strip; the text buttons
(H1, H2, H3, B, I) at one cap height and not crowding their labels; nothing touching or overlapping the label; colours by role (Delete red,
Complete green, Add accent, the rest Text). In every grid picture: every row whole, the picture in its column, sharp at 150 and 200%, the same
size as the check boxes beside it.

## Rounds

1. First pass (pictures of `fd28b0d3a`): two faults. (a) The headings at 8/7: H2 and H3 filled their 32 px box edge to edge and crowded their
   labels at 150% ("H3 11"). Fixed in `7dfb285f1`: the five text buttons meet at a cap height of 15 (H 15/14, B 0.92, I 15/16), H2 and H3 1 px
   inside the box (a test checks it). (b) The grid pictures cut their last row (the wizard's and the info buttons' only row is half shown):
   their height was measured at the old font.
2. Second pass (pictures of `7dfb285f1`): the toolbars are right; the grids still cut their last row. Fixed in `fb85b2912`: the gallery grows
   each grid until it no longer scrolls.
3. Third pass (the pictures of `fb85b2912`; the toolbar pictures are pixel for pixel those of the second pass): nothing found.
4. Fourth pass, after the second review (`ui/ui9b4fix-2`): the third pass had missed that at 100% H1 to H3 and I were blurred (the optical
   scale in the box of their own 20 px design put their 1 px stems and serifs on fractions of a pixel: grey 2 px columns where `main` has crisp
   1 px ones; the pictures were read at their own size and the headings only compared by height). Fixed (UI-9b.md, 5.8.1): a design in a box
   of its own size is drawn pixel for pixel. Redrawn: only `toolbars/FormEmailEdit-100` and `toolbars/FormWikiEdit-100` differ from the third
   pass (every other picture is byte for byte the same file). Each 100% picture read again, the text buttons and their neighbours enlarged 4x
   and their pixels dumped (luminance per pixel): nothing found. The lines of the 100% pictures below are this pass's; the others are the third
   pass's, unchanged.

## The pictures (third pass; 100% lines: fourth pass)

| Picture | Checked | Found |
|---|---|---|
| toolbars/FormAccounting-100.png | fourth pass: read again (the same file as the third pass); 3 buttons whole (Add, Edit, Export); one box; Add in accent | nothing |
| toolbars/FormAccounting-150.png | as at 100%; the three 32 px designs one size | nothing |
| toolbars/FormAccounting-200.png | as at 150% | nothing |
| toolbars/FormCareCreditWeb-100.png | fourth pass: read again (the same file as the third pass); 2 buttons whole (Back, Forward); arrows the same size | nothing |
| toolbars/FormCareCreditWeb-150.png | as at 100% | nothing |
| toolbars/FormCareCreditWeb-200.png | as at 100% | nothing |
| toolbars/FormClaimsSend-100.png | fourth pass: read again (the same file as the third pass); 7 buttons whole; Preview, New document, Print, Complete, Send, Get Reports, Label one size; Complete green | nothing |
| toolbars/FormClaimsSend-150.png | as at 100%; Preview and New document (the audited "smaller" document icons) as tall as the printer, the printer no larger (enlarged 2x) | nothing |
| toolbars/FormClaimsSend-200.png | as at 150% (enlarged 2x) | nothing |
| toolbars/FormEmailEdit-100.png | fourth pass: 20 buttons whole; one box; Delete red; H1, H2, H3 and I their own 20 px designs, pixel for pixel: 1 px black stems (each H's two stems and crossbar, the 1, the foot of the 2, the bottom of the 3) and black serifs on I, no grey columns (pixels dumped, 4x crop beside Link and B); H 12 px tall, B 12, I 14 (Fluent's design, as on main); not lighter than Link, Ext Link or B | nothing |
| toolbars/FormEmailEdit-150.png | as at 100%; Copy, Paste, Save as Draft the size of Cut and Undo; the headings 1 px inside the box with their label gap (enlarged 2x) | nothing |
| toolbars/FormEmailEdit-200.png | as at 150% | nothing |
| toolbars/FormJournal-100.png | fourth pass: read again (the same file as the third pass); 3 buttons whole (Add, Print, Export) | nothing |
| toolbars/FormJournal-150.png | as at 100%; the printer no larger than Add | nothing |
| toolbars/FormJournal-200.png | as at 150% | nothing |
| toolbars/FormReportComplex-100.png | fourth pass: read again (the same file as the third pass); 6 buttons whole (Print, Back, Forward, Export, Zoom in, Zoom out) | nothing |
| toolbars/FormReportComplex-150.png | as at 100%; one size | nothing |
| toolbars/FormReportComplex-200.png | as at 150% | nothing |
| toolbars/FormReportCustom-100.png | fourth pass: read again (the same file as the third pass); 7 buttons whole (with PDF) | nothing |
| toolbars/FormReportCustom-150.png | as at 100%; PDF (scaled 24 px design) the size of the printer | nothing |
| toolbars/FormReportCustom-200.png | as at 150% | nothing |
| toolbars/FormWiki-100.png | fourth pass: read again (the same file as the third pass); 15 buttons whole (14, New document, was cut in the first gallery); Rename (6) is Rename A, a name with a text cursor; Add in accent | nothing |
| toolbars/FormWiki-150.png | as at 100%; the printer (5) the size of its neighbours | nothing |
| toolbars/FormWiki-200.png | as at 150% | nothing |
| toolbars/FormWikiEdit-100.png | fourth pass: 21 buttons whole, with the two padlocks; the text buttons as the email editor (the same pixels: 4x crop) | nothing |
| toolbars/FormWikiEdit-150.png | as the email editor at 150% | nothing |
| toolbars/FormWikiEdit-200.png | as at 150% | nothing |
| grids/infobutton-100.png | fourth pass: read again (the same file as the third pass); the row whole; the info circle in its column (unchanged from the first gallery) | nothing |
| grids/infobutton-150.png | the row whole; the circle sharp, 24 px | nothing |
| grids/infobutton-200.png | the row whole; the circle sharp, 32 px | nothing |
| grids/reminders-100.png | fourth pass: read again (the same file as the third pass); 5 rows whole; the task list icons (Fluent has no 16 px design: the 20 px one scaled to the 16 px box, a little soft, as before) the size of the check boxes (enlarged 5x) | nothing |
| grids/reminders-150.png | 5 rows whole; every picture sharp, one size | nothing |
| grids/reminders-200.png | as at 150% | nothing |
| grids/tasks-100.png | fourth pass: read again (the same file as the third pass); as reminders at 100% | nothing |
| grids/tasks-150.png | as reminders at 150% | nothing |
| grids/tasks-200.png | as reminders at 200% | nothing |
| grids/wizard-100.png | fourth pass: read again (the same file as the third pass); the row whole; the info circle (unchanged from the first gallery) | nothing |
| grids/wizard-150.png | the row whole; sharp | nothing |
| grids/wizard-200.png | the row whole; sharp | nothing |

Noted, not faults of this package: at 200% a toolbar icon is the 32 px design in a 44 px slot, as at 150% (section 3.5 keeps 32 for 32 to
47 px slots); the one-row grids show the grid's scroll bar, disabled, as every GridOD does. The real wiki toolbar with the optional Read-Only
Copy button is 55 px wider than its window at 100% since UI-2b's font (section 5.8, for UI-12).

## The companion gallery G-UI3-ui9b4-v2-approved (fourth pass)

The 119 approved pictures the box rule changes (UI-9b.md, 5.8.1), `main` before and the branch after. Checked: each diff marks only the
icons the rule names, and each such icon now has its neighbours' size; nothing else in the picture moves.

| Pictures | Checked | Found |
|---|---|---|
| states/modulebar-150, -200, -chart-150, -chart-200, -hover-150, -hover-200, -pressed-150, -pressed-200 (8) | the diffs mark only Tx Plan; read before and after at 150%: Tx Plan was a size smaller than Appts, Family and Chart, now theirs | nothing |
| toolbars/ (16), trees/ (5), lists/ (3) | the module toolbars, trees and task grids of item 1 of 5.8: the icons without a 32 px (150 and 200%) or 16 px (100%) design now as large as their neighbours | nothing |
| windows/DashboardCellCtrl, windows/DashboardPanelCtrl, controls/DashboardCellCtrl.butDrag, controls/DashboardCellCtrl.pictureBox (4) | the diffs mark only Move and Add Chart; both now the size of Refresh, Edit and Preview in their 16 px buttons; the buttons, labels and frames unchanged | nothing |
| icons/ (79) | the 79 pairs side by side on one sheet: each after icon fills the box of its slot (the keyline square of the 16, 20, 24, 32 or 48 px box) where the before was its largest smaller design, centred; strokes even, nothing clipped; the filled Tx Plan twins in accent | nothing |
| sheet-100 to sheet-200 (4) | the icon library sheets: the same 79 icons changed in place; the layout unchanged (same size) | nothing |
