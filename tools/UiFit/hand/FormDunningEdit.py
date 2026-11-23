"""FormDunningEdit on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner left the left column at
main's 12, the Email Statement Override group 12 px lower than the Age of Account group beside it (the clinic picker's row stands over the
email group only) and the billing type list 34 px short of the groups beside it. Every column starts under the picker's row (8 + 23 + 8):
the billing type list (its caption 2 px over it) at the content edge down to the Insurance Payment Pending group's bottom, the two groups
8 px after it, the Statement Notes group under them at the content edge, the email group 8 px after the left column to the right content
edge; the picker at the right content edge on the first row; the groups' fields that ended on their content edge end on it again."""


def grow_inside(h, grp, dw):
    """Moves the controls of a group that end at its content edge by the group's change of width."""
    g = h.n(grp)
    edge = g.nw - 8
    for k in g.children:
        if not k.name or k.dock not in ("None", ""):
            continue
        if abs(k.nx + k.nw - edge) <= 6:
            if k.kind == "button" or k.nw < 120:
                h.set(k.name, k.nx + dw, None)
            else:
                h.set(k.name, None, None, k.nw + dw, None)


def layout(h):
    H = h.H
    root = h.tree
    R = root.w - 8
    T = 8 + H + 8
    LW = 158
    g1, g2, g3, g4 = (h.n(n) for n in ("groupBox1", "groupBox2", "groupBox3", "groupBox4"))
    xg = 8 + LW + 8
    left_r = 8 + g3.nw
    w12 = left_r - xg
    grow_inside(h, "groupBox1", w12 - g1.nw)
    grow_inside(h, "groupBox2", w12 - g2.nw)
    h.set("groupBox1", xg, T, w12, None)
    h.set("groupBox2", xg, T + g1.nh + 8, w12, None)
    b2 = T + g1.nh + 8 + g2.nh
    # The Statement Notes group keeps its height (its boxes fill it): the window grows by the 12 px the picker's row adds.
    hr = max(root.h, b2 + 8 + g3.nh + 8 + 24 + 8)
    h.set(".", None, None, root.w, hr)
    yb = hr - 8 - 24
    h.set("butDelete", 8, yb, None, 24)
    h.set("butSave", R - h.n("butSave").nw, yb, None, 24)
    h.set("label8", 8, T - 2 - 17, LW, 17)
    h.text_align("label8", "BottomLeft")
    h.set("listBillType", 8, T, LW, b2 - T)
    h.set("groupBox3", 8, b2 + 8, None, None)
    x4 = left_r + 8
    grow_inside(h, "groupBox4", (R - x4) - g4.nw)
    dh = (yb - 8 - T) - g4.nh
    body = h.n("textEmailBody")
    h.set("textEmailBody", None, None, None, body.nh + dh)
    h.set("groupBox4", x4, T, R - x4, yb - 8 - T)
    c = h.n("comboClinics")
    h.set("comboClinics", R - c.nw, 8, None, H)
    h.set("checkSuperFamily", x4, 8 + (H - h.CH) // 2, None, None)
    for g in ("groupBox1", "groupBox2", "groupBox3", "groupBox4"):
        h.watch(g)
