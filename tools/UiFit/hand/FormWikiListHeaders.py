"""FormWikiListHeaders on the grid (UI-12 slice 4, layout script on the planner's tree). Add and Remove are icon buttons 75 px wide, Remove's
text against its icon: both 84 px (6 px between the icon and the text and after the text) at the right content edge, the pick list grid
and its note 6 px before them."""


def layout(h):
    R = h.tree.w - 8
    x = R - 84
    for name in ("butAdd", "butRemove"):
        h.set(name, x, None, 84, None)
    g = h.n("gridPickList")
    w = x - 6 - g.nx
    h.set("gridPickList", None, None, w, None)
    h.set("label1", None, None, w, None)
