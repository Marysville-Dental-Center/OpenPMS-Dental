"""FormEquipment on the grid (UI-12 slice 4, layout script on the planner's tree). The planner kept main's three radio buttons 16 px apart
(drawn into each other, 22 px tall): the list pitch from the content top, the grid 8 px under the list (its bottom kept)."""


def layout(h):
    x = h.n("radioPurchased").nx
    y = 8
    for name in ("radioPurchased", "radioSold", "radioAll"):
        h.check_left(name)
        h.set(name, x, y, h.cw(name), h.CH)
        y += h.CH + 4
    g = h.n("gridMain")
    top = y - 4 + 8
    h.set("gridMain", None, top, None, g.ny + g.nh - top)
