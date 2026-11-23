"""FormClearinghouseEdit (Edit Clearinghouse) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The planner
paired the Description field's note ("Also used in X12 1000B NM103") with the clinic picker as its label, right-aligned under the picker's own
Clinic label; the script puts the note 6 px after the Description field, left-aligned as on main, and the clinic picker 16 px after the note's
text; the ERA behavior list on the right column's x (the Use Claim Export Path box's) to the Location ID box's right edge; the payor note
(a borderless read-only box) as tall as its three lines."""


def layout(h):
    d = h.n("textDescription")
    wn = h.lw("label17")
    xn = d.x + d.w + 6
    h.set("label17", xn, d.y, wn, d.h)
    h.text_align("label17", "MiddleLeft")
    c = h.n("comboClinic")
    h.set("comboClinic", xn + wn + 16, d.y)
    # The right column on one x (the Use Claim Export Path box's): the ERA behavior list to the Location ID box's right edge.
    chk = h.n("checkIsClaimExportAllowed")
    loc = h.n("textLocationID")
    h.set("listBoxEraBehavior", chk.x, None, loc.x + loc.w - chk.x)
    # The payor note (a borderless read-only box) as tall as its three lines (44 px showed two); the rows hidden under it move down with it.
    t = h.n("textBox2")
    dy = 52 - t.h
    h.set("textBox2", None, None, None, 52)
    bar = h.n("butSave").y
    for k in h.tree.children:
        if k.y >= t.y + t.h and k.y + k.h + dy <= bar - 4 and k.dock in ("None", "") and k.name not in ("butSave", "butDelete"):
            h.set(k.name, None, k.y + dy)
