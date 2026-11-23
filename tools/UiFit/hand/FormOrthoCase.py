"""FormOrthoCase (Ortho Case) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). One touch on the planner's
layout: the three groups of the left column on the content edge (8, Delete's x; the planner kept main's 9), their right edges kept."""


def layout(h):
    for name in ("groupFee", "groupProcBreakdown", "groupDates"):
        g = h.n(name)
        if g.x != 8:
            h.set(name, 8, None, g.w + g.x - 8, None)
    # Fix round 8: Payment Plan on the Ortho Schedule grid's right edge (main: so; the window grew and the button stayed).
    gr, pp = h.n("gridOrthoSchedule"), h.n("butPatPayPlan")
    h.set("butPatPayPlan", gr.nx + gr.nw - pp.nw, None)
    # Fix round 9: the Ortho Schedule grid ends on the left column's bottom (the Dates group's; main: so). The window grew and the grid kept
    # its height, 107 px short. It is anchored to the top and bottom and keeps that at run time.
    gd = h.n("groupDates")
    h.set("gridOrthoSchedule", None, None, None, gd.ny + gd.nh - gr.ny)
