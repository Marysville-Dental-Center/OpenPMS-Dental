# UI-CLOSE audit: gallery G-UI3-uiclose

The visual audit and fix round before the owner sees the gallery (the rule of 2026-11-21). Gallery:
`.git/od-tools/test-results/G-UI3-uiclose` (zip beside it), 4 pictures, 4 changed, 0 the same: FormBackup and ServiceManager's
FormServiceManage at 100% and 150%, before (the branch point's build, main) and after (the final build). Each picture was opened before
and after, and the diff pictures checked: the changed pixels lie only on the moved and resized controls.

## The pictures

| Picture | Before | After | Verdict |
|---|---|---|---|
| `1-windows-100/FormBackup.png` | Every example note cut: the Backup notes show 2 of 3 (the first line wrapped) and 3 of 4 lines, the three Restore notes their first line only (no example folder); 59 px empty under Save Defaults | Every note shows all its lines; both groups on the grid with one rhythm (content 4 px under the title band, field 2 px under its note, next note 6 px under the field, 8 px under the last row); the groups taller, Save Defaults 8 px under the Restore group, 15 px over the tab page's bottom | fine, findings 4 and 5 |
| `2-at-150/FormBackup-150.png` | The same cuts at 150% | All lines shown (after the fix round below); the same margins in both groups | fine, findings 1, 4 and 5 |
| `1-windows-100/FormServiceManage.png` | A 50 px empty band between the Status row and the button bar | The Service Name and Path rows, 27 px, Status and Refresh, 27 px, the bar on the bottom padding; window 719 x 166 as on main | fine |
| `2-at-150/FormServiceManage-150.png` | The same band at 150% | The same equal gaps at 150% | fine, note 3 |

## Findings and what was done

1. **At 150% the two Backup notes hid their last line.** The first round sized each note to exactly its lines (15 px a line, the 150%
   pitch 25 checked: 50 and 67 px); drawn at 150%, "\\server\mysql\data\" and "\\frontdesk\backups\" were missing, while the Restore
   notes, 1 px taller than their lines at 150%, showed both. The edit control needs 1 px more than its lines. Fixed: each note 1 px taller
   than its lines at every scale up to 175% (measured: 35, 53 and 70 px), the controls under them moved by as much. Redrawn: every line
   shows at 100% and 150%.
2. **The brief named textBox5 only; the four other example notes had the same cut** (the 100% before picture: textBox4 "D:\" and
   textBox3 "C:\mysql\data\" missing, textBox2 and textBox1 short of their last lines). Fixed alike in the same layout change, as the
   audit of a changed window asks; the window keeps main's size.
3. **Not changed (as on main):** at 150% the service window's text boxes are shorter than its buttons (the plain exe's stock text boxes
   take the font's height, the buttons are scaled); the Backup page's example folders (`C:\mysql\data\`) are the window's texts, and
   textBox5's folder is UI-BRAND-2's text (`C:\OpenPmsImages\` after its merge; measured: two lines too).

## Review fix (ui/uiclose-2)

The review measured FormBackup's two groups and found two problems; both are fixed and FormBackup redrawn at 100% and 150%
(FormServiceManage, drawn again in the same runs, came out byte for byte as before).

4. **The two groups had different inner margins** (100%: Backup 4 px from its title letters to the first note and 13 px under the last
   field, Restore 13 px and 7 px). Main's Backup group started its content at group y 16, inside the 18 px title band, and ended 14 px
   under its last row. Fixed: both groups by UI-12.md 1.1, content at group y 22 (the Backup controls, drawn over the group, at tab
   y 88), the group ending 8 px under its last row. Measured on the new pictures: the first note 26 px under each group's top line at
   100% (40 px at 150%), the last field 7 px over each group's bottom line at 100% (15 px at 150%), the same in both groups.
5. **Two rhythms of note and field** (2 / 6 px in the Backup group, 4 / 4 px in the Restore group). Fixed: one rhythm, each field 2 px
   under the note that names it and the next note 6 px under that field, in both groups. The Restore group 225 -> 222 px, Save Defaults
   516 -> 513 (8 px under the group); the window keeps 796 x 595. What still varies on the 100% picture (the note's last text line to
   its field: 9, 11, 8, 6, 6 px) is the notes' own height: each is as tall as its lines at 175% (finding 1), so at 100% a four-line note
   has 10 px under its last line and a two-line note 5 px. The layout gaps are now the same everywhere.

## Item 1 (no pictures)

MDIChild is on the grid list (slice 17) and GridRulesTests builds it with no finding: the report editor fills it, nothing moved. The four
files in `sweep-known.tsv` (FormODBase, UnmountedBar, WindowingSlider, DashFamilyInsurance) are not built by the harness and nothing of
them changed, so they have no pictures.
