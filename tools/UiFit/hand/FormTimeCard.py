"""FormTimeCard on the grid (UI-12 slice 2, mode hand: the planner left the Time Card/Breaks radio buttons over one another and the Employee
group off the top band). The top band: the Pay Period, Employee and view groups 8 px apart, one height, the last two at the right content
edge; in Pay Period the arrows, then Start Date and End Date on one field column, Paycheck Date and Note on another (16 px between columns,
each label right-aligned 6 px before its box, the two-line note box under Paycheck Date to the content edge); the time card grid 8 px under
the band to both content edges, down to 8 px over the totals band (the totals grid and the totals format group, 8 px apart); Add
Adjustment, Calc Week OT and Calc Daily 6 px apart at the bottom left, Print and Save at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    R = root.w - 8
    top = (h.n("groupBox1").title_bottom or 18) + 4
    GH = top + P + 40 + 8
    GW2 = 140
    x2 = R - GW2
    x1 = x2 - 8 - GW2
    gw = x1 - 8 - 8
    # Pay Period
    h.set("butLeft", 8, top, 39, 24)
    h.set("butRight", 8 + 39 + 6, top, 39, 24)
    xa = 8 + 39 + 6 + 39 + 16
    lwa = max(h.lw("label2"), h.lw("label3"))
    Fa = xa + lwa + 6
    for lab, fld, y in (("label2", "textDateStart", top), ("label3", "textDateStop", top + P)):
        h.set(lab, xa, y, lwa, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, Fa, y, 100, H)
    xb = Fa + 100 + 16
    lwb = max(h.lw("label4"), h.lw("labelNote"))
    Fb = xb + lwb + 6
    h.set("label4", xb, top, lwb, H)
    h.text_align("label4", "MiddleRight")
    h.set("textDatePaycheck", Fb, top, 100, H)
    h.set("labelNote", xb, top + P, lwb, H)
    h.text_align("labelNote", "MiddleRight")
    h.set("textNote", Fb, top + P, gw - 8 - Fb, 40)
    h.set("groupBox1", 8, 8, gw, GH)
    # Employee
    h.set("butPrevEmp", (GW2 - 84) // 2, top, 39, 24)
    h.set("butNextEmp", (GW2 - 84) // 2 + 45, top, 39, 24)
    h.set("groupEmployee", x1, 8, GW2, GH)
    # View
    y = top
    for r in ("radioTimeCard", "radioBreaks"):
        h.check_left(r)
        h.set(r, 8, y, h.cw(r), CH)
        y += CH + 4
    h.set("groupBox2", x2, 8, GW2, GH)
    for g in ("groupBox1", "groupEmployee", "groupBox2"):
        h.watch(g)
    yb = root.h - 8 - 24
    # The totals format group draws its (empty) title band like every group: its radio buttons start under it; the totals grid as tall.
    tt = (h.n("groupTotalsFormat").title_bottom or 18) + 4
    th = tt + 2 * CH + 4 + 8
    yt = yb - 8 - th
    h.set("gridTotals", 8, yt, 630, th)
    y = tt
    for r in ("radioTotalsDecimal", "radioTotalsColon"):
        h.check_left(r)
        h.set(r, 8, y, h.cw(r), CH)
        y += CH + 4
    h.set("groupTotalsFormat", 8 + 630 + 8, yt, 120, th)
    h.watch("groupTotalsFormat")
    yg = 8 + GH + 8
    h.set("gridMain", 8, yg, R - 8, yt - 8 - yg)
    x = 8
    for name in ("butAdj", "butCalcWeekOT", "butCalcDaily"):
        b = h.n(name)
        h.set(name, x, yb, b.nw, 24)
        x += b.nw + 6
    h.set("butSave", R - 75, yb, 75, 24)
    h.set("butPrint", R - 75 - 8 - 86, yb, 86, 24)
