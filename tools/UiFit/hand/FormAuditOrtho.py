"""FormAuditOrtho on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner moved both grids to the top and
left their height (21 px under them against 8 over), 12 px from the sides and 6 px apart: both grids 8 px from every edge of the window,
8 px apart, the history grid keeping its width."""


def layout(h):
    root = h.tree
    hist = h.n("gridHist")
    h.set("gridHist", 8, 8, None, root.h - 16)
    x = 8 + hist.nw + 8
    h.set("gridMain", x, 8, root.w - 8 - x, root.h - 16)
