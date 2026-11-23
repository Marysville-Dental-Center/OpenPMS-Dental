"""FormClaimForms on the grid (UI-12 slice 4, layout script on the planner's tree). The two-line note at the top was 28 px tall (its second
line's descenders cut by the grids): the note as tall as its two lines (30), and everything under it 8 px lower (the grids' bottoms kept)."""


def layout(h):
    h.set("label1", None, 8, None, 30)
    dy = 8 + 30 + 8 - h.n("gridInternal").ny
    for name in ("gridInternal", "gridCustom"):
        g = h.n(name)
        h.set(name, None, g.ny + dy, None, g.nh - dy)
    for name in ("butCopy", "butDefault", "groupBox2", "groupBox1"):
        h.set(name, None, h.n(name).ny + dy)
