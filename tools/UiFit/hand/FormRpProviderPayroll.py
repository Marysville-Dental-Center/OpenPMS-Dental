"""FormRpProviderPayroll on the grid (UI-12 slice 2, mode hand: the planner put the lists' captions 20 px over their All boxes and left
the columns ragged). Two list columns from the content edge, 8 px apart (caption, All 2 px under it, the list 2 px under the box), the
clinic options under the Clinics list on the 22 px pitch ending 8 px over the bottom bar; the right column 16 px after them: Today's Date
on the first row, the Date Range group (the period buttons centred on This Period, From and To on its column) and the report type group
under it, 8 px apart; the special report note at the bottom left, OK at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    R = root.w - 8
    yb = root.h - 8 - 26
    LW = 154
    yl = 8 + 18 + 2 + CH + 2
    yc2 = yb - 8 - CH
    yc1 = yc2 - CH - 4
    for (cap, chk, lst), x in zip((("label1", "checkAllProv", "listProv"), ("labelClin", "checkAllClin", "listClin")), (8, 8 + LW + 8)):
        h.set(cap, x, 8, LW, 18)
        h.text_align(cap, "BottomLeft")
        h.check_left(chk)
        h.set(chk, x, 8 + 18 + 2, h.cw(chk), CH)
        h.set(lst, x, yl, LW, yc1 - 4 - yl)
    for name, y in (("checkClinicInfo", yc1), ("checkClinicBreakdown", yc2)):
        h.check_left(name)
        h.set(name, 8 + LW + 8, y, h.cw(name), CH)
    x3 = 8 + 2 * LW + 8 + 16
    # Today's Date on the date range group's field column (95 px into the group)
    h.set("label4", x3, 8, 95 - 6, H)
    h.text_align("label4", "MiddleRight")
    h.set("textToday", x3 + 95, 8, 100, H)
    GW = 281
    top = (h.n("groupDateRange").title_bottom or 18) + 4
    F = 95
    h.set("butLeft", F - 6 - 45, top, 45, 24)
    h.set("butThis", F, top, 101, 24)
    h.set("butRight", F + 101 + 6, top, 45, 24)
    y = top + 24 + 4
    for lab, fld in (("label2", "dtPickerFrom"), ("label3", "dtPickerTo")):
        h.set(lab, 8, y, F - 6 - 8, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F, y, 101, H)
        y += P
    gh = y - 4 + 8
    yg = 8 + H + 8
    h.set("groupDateRange", x3, yg, GW, gh)
    t2 = (h.n("groupPayrollReportType").title_bottom or 18) + 4
    for i, r in enumerate(("radioSimpleReport", "radioDetailedReport")):
        h.check_left(r)
        h.set(r, 8, t2 + i * (CH + 4), h.cw(r), CH)
    h.set("groupPayrollReportType", x3, yg + gh + 8, GW, t2 + 2 * CH + 4 + 8)
    h.watch("groupDateRange")
    h.watch("groupPayrollReportType")
    h.set("butOK", R - 75, yb, 75, 26)
    h.set("labelSpecialReport", 8, yb, R - 75 - 8 - 8, 26)
    h.text_align("labelSpecialReport", "MiddleLeft")
