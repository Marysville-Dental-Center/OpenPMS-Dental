"""FormOrthoCase (Ortho Case) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). One touch on the planner's
layout: the three groups of the left column on the content edge (8, Delete's x; the planner kept main's 9), their right edges kept."""


def layout(h):
    for name in ("groupFee", "groupProcBreakdown", "groupDates"):
        g = h.n(name)
        if g.x != 8:
            h.set(name, 8, None, g.w + g.x - 8, None)
