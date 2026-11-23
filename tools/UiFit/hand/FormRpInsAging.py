"""FormRpInsAging on the grid (UI-12 slice 2 fix round, review of 2026-11-23; layout script of grid_hand.py, on the planner's tree). The
planner kept the left column (the As Of Date box and its caption, the Age of Account, Group By and Filter groups, the outstanding claims
check box) at main's x 12 while the right margin is 8, and the three lists at main's widths (163) 4 and 6 px apart, the Billing Types list
and its All box 2 px over the others: the left column at the content edge (8, its sizes kept); the three lists one width, 8 px apart, ending
on the right content edge, on one top with their captions and All boxes over them (the caption at 8, the All box 2 px under it, the list 4 px
under the box); the future transactions note (shown by the code when the preference asks) stays under the Providers list."""


def layout(h):
    root = h.tree
    R = root.w - 8
    dx = h.n("textDate").nx - 8
    for name in ("textDate", "label1", "groupAgeOfAccount", "groupGroupBy", "checkOnlyShowPatsOutstandingClaims", "groupFilter"):
        n = h.n(name)
        h.set(name, n.nx - dx, None)
    cols = (("label2", "checkBillTypesAll", "listBillType"), ("label3", "checkProvAll", "listProv"), ("labelClin", "checkAllClin", "listClin"))
    first = h.n("listBillType")
    x0 = first.nx
    gap = 8
    w = (R - x0 - gap * (len(cols) - 1)) // len(cols)
    x = R - w * len(cols) - gap * (len(cols) - 1)  # the last list ends on the right content edge
    cap = h.n("label3")
    chk = h.n("checkProvAll")
    lst = h.n("listProv")
    for c, k, l in cols:
        h.set(c, x, cap.ny, w, cap.nh)
        h.check_left(k)
        h.set(k, x, chk.ny, h.cw(k), chk.nh)
        h.set(l, x, lst.ny, w, lst.nh)
        if l == "listProv":
            h.set("labelFutureTrans", x, None)
        x += w + gap
