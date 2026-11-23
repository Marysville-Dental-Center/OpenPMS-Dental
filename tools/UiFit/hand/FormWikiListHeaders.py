"""FormWikiListHeaders on the grid (UI-12 slice 4, layout script on the planner's tree). Add and Remove are icon buttons 75 px wide, Remove's
text against its icon: both 84 px (6 px between the icon and the text and after the text) at the right content edge, the pick list grid
and its note 6 px before them. Fix round: the two grids side by side on one top, 2 px under their captions, which share one top and
bottom (the planner left the grids at 73 and 72 and the captions at 8 and 9); the headers grid keeps its bottom."""


def layout(h):
    R = h.tree.w - 8
    x = R - 84
    for name in ("butAdd", "butRemove"):
        h.set(name, x, None, 84, None)
    g = h.n("gridPickList")
    w = x - 6 - g.nx
    cap = h.n("label1")
    top = cap.ny
    cb = cap.ny + cap.nh
    h.set("gridPickList", None, cb + 2, w, None)
    h.set("label1", None, None, w, None)
    for name in ("label3", "label2"):
        h.set(name, None, top, None, cb - top)
    m = h.n("gridMain")
    bottom = m.ny + m.nh
    h.set("gridMain", None, cb + 2, None, bottom - cb - 2)
