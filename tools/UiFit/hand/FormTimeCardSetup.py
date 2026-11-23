"""FormTimeCardSetup on the grid (UI-12 slice 4, layout script on the planner's tree). Hide pay periods stood 120 px right of the pay
period grid it filters, and each Delete Selected short of its grid's right edge (the rules' one by 66 px): Hide pay periods at the grid's
x, each Delete Selected right-aligned on its grid's right edge. Fix round: the rules grid 8 px after the pay period grid on its top and
height (the planner put it 67 px further right and 4 px higher and widened the window for it), ending as far from the right edge as the
pay period grid starts from the left (main's 1020 px window); Add and Delete Selected under both grids on one row, the Options group 8 px
under the rules' buttons, the window 8 px under it."""

WIDTH = 1020  # main's client width


def layout(h):
    g = h.n("gridMain")
    h.check_left("checkHideOlder")
    h.set("checkHideOlder", g.nx, None)
    xr = g.nx + g.nw + 8
    h.set("gridRules", xr, g.ny, WIDTH - g.nx - xr, g.nh)
    yb = h.n("butAdd").ny
    for but, grid in (("butDelete", "gridMain"), ("butDeleteRules", "gridRules")):
        gg = h.n(grid)
        b = h.n(but)
        h.set(but, gg.nx + gg.nw - b.nw, yb)
    h.set("butAddRule", xr, yb, None, None)
    grp = h.n("groupBox1")
    yg = yb + 24 + 8
    h.set("groupBox1", xr, yg, None, None)
    h.set(".", None, None, WIDTH, max(h.tree.h, yg + grp.nh + 8))
