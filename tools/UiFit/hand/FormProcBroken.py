"""FormProcBroken on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The clinic picker's box ended 8 px short of the
Provider combo under it, and the two Auto Note buttons differed (the account one 3 px left and 1 px shorter): the picker's box on the
combo's right edge, the account Auto Note on the chart one's x and size."""


def layout(h):
    c = h.n("comboClinic")
    p = h.n("comboProv")
    h.set("comboClinic", None, None, p.nx + p.nw - c.nx, None)
    a = h.n("butAutoNoteChart")
    h.set("butAutoNoteAccount", a.nx, None, a.nw, a.nh)
