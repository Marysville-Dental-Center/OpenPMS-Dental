"""FormXWebTransactions on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). Refresh stood 10 px short of the grid's
right edge (main had them aligned): Refresh right-aligned on the grid's right edge, the clinic combo and its label 6 px before it; the grid
down to 12 px over the bottom, as far as it stands from the left edge (the planner moved it up 5 px and left 34 px under it)."""


def layout(h):
    g = h.n("gridMain")
    b = h.n("butRefresh")
    xb = g.nx + g.nw - b.nw
    h.set("butRefresh", xb, None, None, None)
    c = h.n("comboClinic")
    xc = xb - 6 - c.nw
    h.set("comboClinic", xc, None, None, None)
    lab = h.n("labelClinic")
    h.set("labelClinic", xc - 6 - lab.nw, None, None, None)
    h.set("gridMain", None, None, None, h.tree.h - 12 - g.ny)
