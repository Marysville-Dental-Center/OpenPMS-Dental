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
3. Third pass (the pictures below, `fb85b2912`; the toolbar pictures are pixel for pixel those of the second pass): nothing found.

## The pictures (third pass)

| Picture | Checked | Found |
|---|---|---|
| toolbars/FormAccounting-100.png | 3 buttons whole (Add, Edit, Export); one box; Add in accent | nothing |
| toolbars/FormAccounting-150.png | as at 100%; the three 32 px designs one size | nothing |
| toolbars/FormAccounting-200.png | as at 150% | nothing |
| toolbars/FormCareCreditWeb-100.png | 2 buttons whole (Back, Forward); arrows the same size | nothing |
| toolbars/FormCareCreditWeb-150.png | as at 100% | nothing |
| toolbars/FormCareCreditWeb-200.png | as at 100% | nothing |
| toolbars/FormClaimsSend-100.png | 7 buttons whole; Preview, New document, Print, Complete, Send, Get Reports, Label one size; Complete green | nothing |
| toolbars/FormClaimsSend-150.png | as at 100%; Preview and New document (the audited "smaller" document icons) as tall as the printer, the printer no larger (enlarged 2x) | nothing |
| toolbars/FormClaimsSend-200.png | as at 150% (enlarged 2x) | nothing |
| toolbars/FormEmailEdit-100.png | 20 buttons whole (19, Setup, was missing in the first gallery); one box; Delete red; H1 to H3, B, I one cap height | nothing |
| toolbars/FormEmailEdit-150.png | as at 100%; Copy, Paste, Save as Draft the size of Cut and Undo; the headings 1 px inside the box with their label gap (enlarged 2x) | nothing |
| toolbars/FormEmailEdit-200.png | as at 150% | nothing |
| toolbars/FormJournal-100.png | 3 buttons whole (Add, Print, Export) | nothing |
| toolbars/FormJournal-150.png | as at 100%; the printer no larger than Add | nothing |
| toolbars/FormJournal-200.png | as at 150% | nothing |
| toolbars/FormReportComplex-100.png | 6 buttons whole (Print, Back, Forward, Export, Zoom in, Zoom out) | nothing |
| toolbars/FormReportComplex-150.png | as at 100%; one size | nothing |
| toolbars/FormReportComplex-200.png | as at 150% | nothing |
| toolbars/FormReportCustom-100.png | 7 buttons whole (with PDF) | nothing |
| toolbars/FormReportCustom-150.png | as at 100%; PDF (scaled 24 px design) the size of the printer | nothing |
| toolbars/FormReportCustom-200.png | as at 150% | nothing |
| toolbars/FormWiki-100.png | 15 buttons whole (14, New document, was cut in the first gallery); Rename (6) is Rename A, a name with a text cursor; Add in accent | nothing |
| toolbars/FormWiki-150.png | as at 100%; the printer (5) the size of its neighbours | nothing |
| toolbars/FormWiki-200.png | as at 150% | nothing |
| toolbars/FormWikiEdit-100.png | 21 buttons whole (20, Lock, was missing in the first gallery); as the email editor, with the two padlocks | nothing |
| toolbars/FormWikiEdit-150.png | as the email editor at 150% | nothing |
| toolbars/FormWikiEdit-200.png | as at 150% | nothing |
| grids/infobutton-100.png | the row whole; the info circle in its column (unchanged from the first gallery) | nothing |
| grids/infobutton-150.png | the row whole; the circle sharp, 24 px | nothing |
| grids/infobutton-200.png | the row whole; the circle sharp, 32 px | nothing |
| grids/reminders-100.png | 5 rows whole; the task list icons (Fluent has no 16 px design: the 20 px one scaled to the 16 px box, a little soft, as before) the size of the check boxes (enlarged 5x) | nothing |
| grids/reminders-150.png | 5 rows whole; every picture sharp, one size | nothing |
| grids/reminders-200.png | as at 150% | nothing |
| grids/tasks-100.png | as reminders at 100% | nothing |
| grids/tasks-150.png | as reminders at 150% | nothing |
| grids/tasks-200.png | as reminders at 200% | nothing |
| grids/wizard-100.png | the row whole; the info circle (unchanged from the first gallery) | nothing |
| grids/wizard-150.png | the row whole; sharp | nothing |
| grids/wizard-200.png | the row whole; sharp | nothing |

Noted, not faults of this package: at 200% a toolbar icon is the 32 px design in a 44 px slot, as at 150% (section 3.5 keeps 32 for 32 to
47 px slots); the one-row grids show the grid's scroll bar, disabled, as every GridOD does. The real wiki toolbar with the optional Read-Only
Copy button is 55 px wider than its window at 100% since UI-2b's font (section 5.8, for UI-12).
