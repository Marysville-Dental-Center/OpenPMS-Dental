"""FormClaimForms on the grid (UI-12 slice 4, layout script on the planner's tree). The two-line note at the top was 28 px tall (its second
line's descenders cut by the grids): the note as tall as its two lines (30), and everything under it 8 px lower (the grids' bottoms kept).
Fix round: Copy centred between the two grids (it stood 17 px after the left grid and 7 px before the right one)."""


def layout(h):
    h.set("label1", None, 8, None, 30)
    dy = 8 + 30 + 8 - h.n("gridInternal").ny
    for name in ("gridInternal", "gridCustom"):
        g = h.n(name)
        h.set(name, None, g.ny + dy, None, g.nh - dy)
    for name in ("butCopy", "butDefault", "groupBox2", "groupBox1"):
        h.set(name, None, h.n(name).ny + dy)
    gi = h.n("gridInternal")
    gc = h.n("gridCustom")
    c = h.n("butCopy")
    h.set("butCopy", (gi.nx + gi.nw + gc.nx - c.nw) // 2, None, None, None)
