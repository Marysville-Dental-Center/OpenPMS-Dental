"""FormApptTypeEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The Required Procedures group stood 2 px off the
Procedures to Add row with its list 27 px into it (its radio buttons at 8): the group's list on the left list's top, the list and the
radio buttons at the content edge, Add and Remove 6 px after the list, the group as wide and tall as its content. The time bar (tbTime)
stays where the code and the designer put it, Delete beside it."""


def layout(h):
    left = h.n("listBoxProcCodes")
    lst = h.n("listBoxProcCodesRequired")
    top = 23
    h.set("groupBoxOD1", None, left.ny - top)
    h.set("listBoxProcCodesRequired", 8, top)
    bx = 8 + lst.nw + 6
    a = h.n("butAddRequired")
    h.set("butAddRequired", bx, top)
    h.set("butRemoveRequired", bx, top + a.nh + 4)
    y = top + lst.nh + 4
    for name in ("radioButtonAtLeastOne", "radioButtonAll"):
        h.check_left(name)
        h.set(name, 8, y, h.cw(name), h.CH)
        y += h.CH + 4
    h.set("groupBoxOD1", None, None, bx + a.nw + 8, y - 4 + 8)
