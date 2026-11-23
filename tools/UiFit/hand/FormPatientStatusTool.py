"""FormPatientStatusTool on the grid (UI-12 slice 4, layout script on the planner's tree). The Since label stood 16 px tall at the top of
its row (its text 4 px over the date box's) and the bottom bar's Change Patient Status To combo 36 px off the criteria's date column
(GridRulesTests' column check): the Since label on the row, right-aligned 6 px before the date box; the bottom combo on the clinic picker's
box column (its label right-aligned 6 px before it), Select All, Deselect All and Run as the planner placed them."""


def layout(h):
    p = h.n("odDatePickerSince")
    lab = h.n("label1")
    lw = h.lw("label1")
    h.set("label1", p.nx + 63 - 6 - lw, p.ny, lw, h.H)
    h.text_align("label1", "MiddleRight")
    g = h.n("groupCriteria")
    c = h.n("comboClinic")
    x = g.nx + c.nx + h.clinic_w("comboClinic")
    h.set("comboChangePatientStatusTo", x, None)
    lw2 = h.n("labelChangeTo").nw
    h.set("labelChangeTo", x - 6 - lw2, None)
