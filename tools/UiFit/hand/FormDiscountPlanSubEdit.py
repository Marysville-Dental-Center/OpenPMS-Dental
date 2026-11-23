"""FormDiscountPlanSubEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The fields ended at six x (366 + the ...
button 392, Fee Schedule and Adjustment Type 390, Plan Note 392, Name and To 396, Subscriber Note 376): the window's fields and the ...
button on the right content edge (Save's), the group's fields on its content edge, To's label 6 px before its box."""


def layout(h):
    R = h.tree.nw - 8
    b = h.n("butDiscountPlans")
    h.set("butDiscountPlans", R - b.nw, None)
    d = h.n("textDescript")
    h.set("textDescript", None, None, R - b.nw - 6 - d.nx, None)
    for name in ("textFeeSched", "textAdjustmentType", "textPlanNote"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
    gs = h.n("groupSubscriber")
    h.set("groupSubscriber", 8, None, R - 8, None)
    GR = (R - 8) - 8
    for name in ("textName", "textSubNote"):
        n = h.n(name)
        h.set(name, None, None, GR - n.nx, None)
    t = h.n("textDateTerm")
    h.set("textDateTerm", GR - t.nw, None)
    l6 = h.n("label6")
    h.set("label6", GR - t.nw - 6 - l6.nw, None)
