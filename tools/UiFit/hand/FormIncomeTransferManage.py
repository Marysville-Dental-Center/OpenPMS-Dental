"""FormIncomeTransferManage on the grid (UI-12 slice 4, layout script on the planner's tree). The description at the top left is seven
lines at its 395 px (its last line, "support provider.", was cut at 92 px): the note as tall as its lines (108), the two grids 8 px under
it (their bottoms kept)."""


def layout(h):
    h.set("label1", None, 8, None, 108)
    top = 8 + 108 + 8
    for name in ("gridTransfers", "gridImbalances"):
        g = h.n(name)
        h.set(name, None, top, None, g.ny + g.nh - top)
