"""FormAdjustSelect on the grid (UI-12 slice 4, layout script on the planner's tree). The planner kept main's left column at 13 with 15 px
at the right: the grid and its note from the content edge, the Breakdown group at the right content edge, the grid to 6 px before it."""


def layout(h):
    R = h.tree.w - 8
    g = h.n("groupBoxBreakdown")
    gx = R - g.nw
    h.set("groupBoxBreakdown", gx, None)
    h.set("gridAdjusts", 8, None, gx - 6 - 8, None)
    h.set("label1", 8, None)
