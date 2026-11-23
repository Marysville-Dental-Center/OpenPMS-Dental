"""FormLateCharges on the grid (UI-12 slice 2, mode hand: the planner scattered the late charge settings' rows over three columns).
As main arranges it, on the grid: Date last run and Date of new charges on one field column; the settings group across the content: its two
check boxes a list at the content edge, then the amount rows on the 27 px pitch on one field column (each label right-aligned 6 px before
its box), the statements row on the same column (the second days box on the column, "and" and the first box before it, "days ago" after);
the billing types caption and list at the group's content edge (as wide as the caption), Save As Default under the list on the last row;
the undo and assign groups 8 px under it side by side, one height (the undo date on the window's date column); Run at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    R = root.w - 8
    lw0 = max(h.lw("labelDateLastRun"), h.lw("labelDateNewCharges"))
    F0 = 8 + lw0 + 6
    for lab, fld, y in (("labelDateLastRun", "textDateLastRun", 8), ("labelDateNewCharges", "textDateNewCharges", 8 + P)):
        h.set(lab, 8, y, lw0, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F0, y, 78, H)
    yg = 8 + 2 * P + 4
    gw = R - 8
    t = (h.n("groupLateCharge").title_bottom or 18) + 4
    y = t
    for name in ("checkExcludeAccountNoTil", "checkExcludeExistingLateCharges"):
        h.check_left(name)
        h.set(name, 8, y, h.cw(name), CH)
        y += CH + 4
    y += 4
    BW = 42
    la, ln = h.lw("labelDateRange1"), h.lw("labelDateRange2")
    Fg = max(8 + max(h.lw(n) for n in ("labelExcludeBalanceLessThan", "labelChargePercent", "labelMinCharge", "labelMaxCharge")) + 6,
             8 + la + 6 + BW + 6 + ln + 6)
    for lab, fld in (("labelExcludeBalanceLessThan", "textExcludeLessThan"), ("labelChargePercent", "textChargePercent"),
            ("labelMinCharge", "textMinCharge"), ("labelMaxCharge", "textMaxCharge")):
        h.set(lab, 8, y, Fg - 6 - 8, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, Fg, y, BW, H)
        y += P
    h.set("textDateRangeStart", Fg, y, BW, H)
    h.set("labelDateRange2", Fg - 6 - ln, y, ln, H)
    h.text_align("labelDateRange2", "MiddleRight")
    h.set("textDateRangeEnd", Fg - 6 - ln - 6 - BW, y, BW, H)
    h.set("labelDateRange1", 8, y, Fg - 6 - ln - 6 - BW - 6 - 8, H)
    h.text_align("labelDateRange1", "MiddleRight")
    h.set("labelDateRange3", Fg + BW + 6, y, h.lw("labelDateRange3"), H)
    h.text_align("labelDateRange3", "MiddleLeft")
    ylast = y
    LW = max(158, h.lw("labelBillType"))
    lx = gw - 8 - LW
    h.set("labelBillType", lx, t, LW, 18)
    h.text_align("labelBillType", "BottomLeft")
    h.set("listBillType", lx, t + 18 + 2, LW, ylast - 4 - (t + 18 + 2))
    h.set("butSaveDefaults", gw - 8 - 99, ylast, 99, H)
    gh = ylast + H + 8
    h.set("groupLateCharge", 8, yg, gw, gh)
    y2 = yg + gh + 8
    t = (h.n("groupBoxUndo").title_bottom or 18) + 4
    Fu = F0 - 8  # the undo date on the window's date column
    lwu = Fu - 6 - 8
    h.set("labelDateUndo", 8, t, lwu, H)
    h.text_align("labelDateUndo", "MiddleRight")
    h.set("textDateUndo", Fu, t, 78, H)
    h.set("butUndo", Fu, t + P, 78, 24)
    uw = Fu + 78 + 8
    bh = t + P + 24 + 8
    h.set("groupBoxUndo", 8, y2, uw, bh)
    t = (h.n("groupBoxAssignCharge").title_bottom or 18) + 4
    h.check_left("radioPatPriProv")
    h.set("radioPatPriProv", 8, t, h.cw("radioPatPriProv"), CH)
    h.check_left("radioSpecificProv")
    h.set("radioSpecificProv", 8, t + CH + 4 + (H - CH) // 2, h.cw("radioSpecificProv"), CH)
    h.set("comboSpecificProv", 8 + h.cw("radioSpecificProv") + 6, t + CH + 4, 174, H)
    aw = 8 + h.cw("radioSpecificProv") + 6 + 174 + 8
    h.set("groupBoxAssignCharge", 8 + uw + 8, y2, aw, bh)
    for g in ("groupLateCharge", "groupBoxUndo", "groupBoxAssignCharge"):
        h.watch(g)
    hr = max(root.h, y2 + bh + 8)
    h.set("butRun", R - 75, hr - 8 - 24, 75, 24)
    h.set(".", None, None, root.w, hr)
