"""FormClearinghouseEdit (Edit Clearinghouse) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The planner
paired the Description field's note ("Also used in X12 1000B NM103") with the clinic picker as its label, right-aligned under the picker's own
Clinic label; the script puts the note 6 px after the Description field, left-aligned as on main, and the clinic picker 16 px after the note's
text."""


def layout(h):
    d = h.n("textDescription")
    wn = h.lw("label17")
    xn = d.x + d.w + 6
    h.set("label17", xn, d.y, wn, d.h)
    h.text_align("label17", "MiddleLeft")
    c = h.n("comboClinic")
    h.set("comboClinic", xn + wn + 16, d.y)
