"""FormMedPat on the grid (UI-12 slice 3, layout script of grid_hand.py on the planner's tree). The planner left the Edit note top-aligned 4
px over its button and Remove centred over its two-line note at 49: the Edit note 6 px after Edit, centred on it; Remove at the padding with
its note 6 px after it, centred on it (two lines, left-aligned). The window only as tall as its content needs (the planner made it 571, a
43 px empty band over Save): Save 8 px under Remove's note, the bottom edge 8 px under Save."""


def layout(h):
    e = h.n("butEdit")
    h.set("labelEdit", e.nx + e.nw + 6, e.ny, h.lw("labelEdit"), e.nh)
    h.text_align("labelEdit", "MiddleLeft")
    r = h.n("butRemove")
    h.set("butRemove", 8, None)
    nh = 34
    h.set("label5", 8 + r.nw + 6, r.ny + r.nh // 2 - nh // 2, 150, nh)
    h.text_align("label5", "MiddleLeft")
    yb = max(r.ny + r.nh, h.n("label5").ny + nh) + 8
    o = h.n("butOK")
    h.set("butOK", None, yb, None, 24)
    h.set(".", None, None, h.tree.nw, yb + 24 + 8)
