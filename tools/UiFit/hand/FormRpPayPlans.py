"""FormRpPayPlans on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner kept main's left column
at x 23 to 47 and the two lists at two widths. The date captions 2 px over their date boxes, the first at the content edge, the second 16 px
after it (the date boxes keep main's 224 px); under them the plan types group with the three options under it at the
content edge, and the Providers and Clinics columns (caption, All 2 px under it, the list 2 px under the box) 16 px after the group, 8 px
apart and one width, to the right content edge, down to 8 px over the bottom bar; OK at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    root = h.tree
    R = root.w - 8
    yb = root.h - 8 - 24
    g = h.n("groupBox1")
    GW = g.nw
    x1 = 8 + GW + 16
    W = (R - x1 - 8) // 2
    x2 = x1 + W + 8
    h.set("label2", 8, 8, h.lw("label2"), 17)
    h.text_align("label2", "BottomLeft")
    h.set("dateStart", 8, 8 + 17 + 2, 224, H)
    h.set("label3", 8 + 224 + 16, 8, h.lw("label3"), 17)
    h.text_align("label3", "BottomLeft")
    h.set("dateEnd", 8 + 224 + 16, 8 + 17 + 2, 224, H)
    yc = 8 + 17 + 2 + H + 8
    h.set("groupBox1", 8, yc, GW, None)
    y = yc + g.nh + 8
    for name in ("checkHideCompletePlans", "checkShowFamilyBalance", "checkHasDateRange"):
        h.check_left(name)
        h.set(name, 8, y, h.cw(name), CH)
        y += CH + 4
    yl = yc + 18 + 2 + CH + 2
    for (cap, chk, lst), x, w in zip((("label1", "checkAllProv", "listProv"), ("labelClin", "checkAllClin", "listClin")), (x1, x2), (W, R - x2)):
        h.set(cap, x, yc, w, 18)
        h.text_align(cap, "BottomLeft")
        h.check_left(chk)
        h.set(chk, x, yc + 18 + 2, h.cw(chk), CH)
        h.set(lst, x, yl, w, yb - 8 - yl)
    h.set("butOK", R - 75, yb, 75, 24)
