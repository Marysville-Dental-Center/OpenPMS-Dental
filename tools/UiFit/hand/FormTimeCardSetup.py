"""FormTimeCardSetup on the grid (UI-12 slice 4, layout script on the planner's tree). Hide pay periods stood 120 px right of the pay
period grid it filters, and each Delete Selected short of its grid's right edge (the rules' one by 66 px): Hide pay periods at the grid's
x, each Delete Selected right-aligned on its grid's right edge."""


def layout(h):
    g = h.n("gridMain")
    h.check_left("checkHideOlder")
    h.set("checkHideOlder", g.nx, None)
    for but, grid in (("butDelete", "gridMain"), ("butDeleteRules", "gridRules")):
        gg = h.n(grid)
        b = h.n(but)
        h.set(but, gg.nx + gg.nw - b.nw, None)
