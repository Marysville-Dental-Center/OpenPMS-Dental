"""FormRpProcNotBilledIns (Procedures Not Billed to Insurance) on the grid (UI-12 slice 1, layout script of grid_hand.py). The planner cut the
filter group as rows across its three columns (the From and To labels left their boxes, Only and Exclude went to two columns); the script
lays the group out in its three columns: the procedure list with the date range under it, Clinics and Procedure Codes on one field column with
Only and Exclude on it and their note beside them, the two options and Refresh on the right; the grid under the group to 8 px over the bottom
bar. The window keeps its size (1019 x 696)."""


def layout(h):
    H = h.H
    grp = h.n("groupBox2")
    # The date range picker holds a label1 and a label2 of its own (another designer): the script's are the group's.
    for name in ("label1", "label2"):
        h.by_name[name] = [k for k in grp.children if k.name == name][0]
    root = h.box(".")
    WW = root.node.w
    g = h.box("groupBox2", 8, 8, WW - 16)
    y0 = g.top
    # The procedure list over the date range; the list starts at the From box (the date range lays out its own labels and boxes, its From box
    # 65 px from its left) and ends with the date range.
    xr = g.left
    h.set("dateRangePicker", xr, y0 + 58 + 4, 453, 22)
    h.set("listBoxShowProcsBeforeIns", xr + 65, y0, 453 - 65, 58)
    # Clinics and Procedure Codes on one field column; Only and Exclude on it, their note 16 px after Exclude's text.
    lw = h.lw("label1")
    M = xr + 453 + 16 + lw + 6
    cl = h.clinic_w("comboClinics")
    h.set("comboClinics", M - cl, y0, cl + 164, H)
    h.set("label1", M - 6 - lw, y0 + 27, lw, H)
    h.text_align("label1", "MiddleRight")
    h.set("textProcedureCodes", M, y0 + 27, 164, H)
    ych = y0 + 54
    for i, name in enumerate(("checkOnlyProcCodes", "checkExcludeProcCodes")):
        h.check_left(name)
        h.set(name, M, ych + i * 22, h.cw(name), h.CH)
    xn = M + max(h.cw("checkOnlyProcCodes"), h.cw("checkExcludeProcCodes")) + 16
    h.set("label2", xn, ych, 100, 36)
    # The options 16 px after the field column, the hidden Include Medical Procedures box at the end of their list; Refresh at the content's
    # right edge on the last row.
    C = M + 164 + 16
    for i, name in enumerate(("checkAutoGroupProcs", "checkShowProcsInProcess", "checkMedical")):
        h.check_left(name)
        h.set(name, C, y0 + i * 22, h.cw(name), h.CH)
    yr = ych + 22 + h.CH - 24
    h.set("butRefresh", g.right - 75, yr, 75, 24)
    g.bottom_used = max(y0 + 58 + 4 + 22, ych + 22 + h.CH, yr + 24)
    g.end()
    hw = root.node.h
    yb = hw - 8 - 24
    yg = 8 + h.n("groupBox2").nh + 8
    h.set("gridMain", 8, yg, WW - 16, yb - 8 - yg)
    h.set("butPrint", 8, yb, 75, 24)
    h.set("butSelectAll", 432, yb, 75, 24)
    h.set("butNewClaims", 432 + 75 + 6, yb, 94, 24)
    root.end(w=WW, h=hw)
