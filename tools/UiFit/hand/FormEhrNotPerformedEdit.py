"""FormEhrNotPerformedEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The two Description boxes and the Note ended
82 to 86 px short of their containers' right edges, and the reason radio buttons (hidden; the code shows them for some codes) kept main's
widths, System Reason wrapping onto a second line: the Description boxes to their groups' content edge, the Note to the right content
edge, the three radio buttons one line each, 6 px apart from the field column."""


def layout(h):
    R = h.tree.nw - 8
    GR = h.n("groupItem").nw - 8
    for name in ("textDescription", "textDescriptionReason"):
        n = h.n(name)
        h.set(name, None, None, GR - n.nx, None)
    n = h.n("textNote")
    h.set("textNote", None, None, R - n.nx, None)
    x = h.n("radioMedReason").nx
    y = h.n("radioMedReason").ny
    for name in ("radioMedReason", "radioPatReason", "radioSysReason"):
        h.check_left(name)
        w = h.cw(name)
        h.set(name, x, y, w, h.CH)
        x += w + 6
