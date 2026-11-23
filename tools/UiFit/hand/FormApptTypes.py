"""FormApptTypes on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner has no fixed point here (Add, Up
and Down 32 px lower on every pass; its first pass left them 68 px under the grid's top, lined up with nothing, 2 px off OK's column): Add
on the grid's top, Up and Down under it 4 px apart, all three on OK's column (8 px after the grid)."""


def layout(h):
    g = h.n("gridMain")
    ok = h.n("butOK")
    y = g.ny
    for name in ("butAdd", "butUp", "butDown"):
        h.set(name, ok.nx, y, ok.nw, None)
        y += h.n(name).nh + 4
