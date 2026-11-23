"""FormRpProdInc (Production and Income) on the grid (UI-12 slice 2, mode hand: the planner set the Providers and Clinics lists at two
tops and cut the write-offs note). The Report Type group at the content edge; under it two list columns 8 px apart (caption, All 2 px under
it, the list 2 px under the box), Include Unearned under the Providers list and the two clinic options under the Clinics list, on the
22 px pitch ending 8 px over the bottom bar (the lists keep main's 186 px: the window grows by 68 px); the right column 16 px after them: Today's Date on the first row, the Date Range group (the
period buttons centred on This, From and To on its column) and the write-offs group (its radio buttons a list, the two-line one as tall as
its lines, the manual note under them) 8 px apart, both 300 px wide (the note's 279 px text); OK at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    R = root.w - 8
    LW = 154
    t1 = (h.n("groupBox1").title_bottom or 18) + 4
    y = t1
    for r in ("radioDaily", "radioMonthly", "radioAnnual", "radioProvider"):
        h.check_left(r)
        h.set(r, 8, y, h.cw(r), CH)
        y += CH + 4
    g1h = y - 4 + 8
    h.set("groupBox1", 8, 8, LW, g1h)
    h.watch("groupBox1")
    yc = 8 + g1h + 8
    yl = yc + 18 + 2 + CH + 2
    # The lists keep main's height (186): the window grows by what the grid's rows add.
    hr = max(root.h, yl + 186 + 4 + CH + 4 + CH + 8 + 26 + 8)
    h.set(".", None, None, root.w, hr)
    yb = hr - 8 - 26
    yc2 = yb - 8 - CH
    yc1 = yc2 - CH - 4
    for (cap, chk, lst), x in zip((("label1", "checkAllProv", "listProv"), ("labelClin", "checkAllClin", "listClin")), (8, 8 + LW + 8)):
        h.set(cap, x, yc, LW, 18)
        h.text_align(cap, "BottomLeft")
        h.check_left(chk)
        h.set(chk, x, yc + 18 + 2, h.cw(chk), CH)
        h.set(lst, x, yl, LW, yc1 - 4 - yl)
    for name, x, y in (("checkUnearned", 8, yc1), ("checkClinicInfo", 8 + LW + 8, yc1), ("checkClinicBreakdown", 8 + LW + 8, yc2)):
        h.check_left(name)
        h.set(name, x, y, h.cw(name), CH)
    x3 = 8 + 2 * LW + 8 + 16
    lw = h.lw("label4")
    h.set("label4", x3, 8, lw, H)
    h.text_align("label4", "MiddleRight")
    h.set("textToday", x3 + lw + 6, 8, 100, H)
    GW = 300
    top = (h.n("groupBox2").title_bottom or 18) + 4
    F = 95
    h.set("butLeft", F - 6 - 45, top, 45, 24)
    h.set("butThis", F, top, 101, 24)
    h.set("butRight", F + 101 + 6, top, 45, 24)
    y = top + 24 + 4
    for lab, fld in (("label2", "textDateFrom"), ("label3", "textDateTo")):
        h.set(lab, 8, y, F - 6 - 8, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F, y, 100, H)
        y += P
    gh = y - 4 + 8
    yg = 8 + H + 8
    h.set("groupBox2", x3, yg, GW, gh)
    h.watch("groupBox2")
    t2 = (h.n("groupShowInsWriteoffs").title_bottom or 18) + 4
    y = t2
    for r, rh in (("radioWriteoffPay", CH), ("radioWriteoffProc", CH), ("radioWriteoffBoth", 34)):
        h.check_left(r)
        h.set(r, 8, y, GW - 16 if rh > CH else h.cw(r), rh)
        y += rh + 4
    h.set("label5", 8, y, GW - 16, 18)
    y += 18 + 8
    h.set("groupShowInsWriteoffs", x3, yg + gh + 8, GW, y)
    h.watch("groupShowInsWriteoffs")
    h.set("butOK", R - 75, yb, 75, 26)
