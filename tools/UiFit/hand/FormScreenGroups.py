"""FormScreenGroups on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner moved the grid up 19 px and left
its height (32 px under it against 8 px over the top row) and moved Add (Top|Right) 34 px left of the grid's right edge: the grid down to
12 px over the bottom (main's 13; 13 px from the left edge), Add right-aligned on the grid's right edge (main's)."""


def layout(h):
    g = h.n("gridMain")
    h.set("gridMain", None, None, None, h.tree.h - 12 - g.ny)
    a = h.n("butAdd")
    h.set("butAdd", g.nx + g.nw - a.nw, None, None, None)
