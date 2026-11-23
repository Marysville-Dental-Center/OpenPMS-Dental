"""FormRpProcNote (Incomplete Procedure Notes) on the grid (UI-12 slice 1, layout script of grid_hand.py). The planner cut the filter group's
check boxes and fields as one band of rows (the widest check box's bounds reach under the Providers label) and widened the window to 1702 px;
the script lays the group out in three columns as drawn: the four check boxes at the content edge with the date range under them, Clinics and
Providers on one field column, the Group By group and Refresh at the top right; the grid under the group to 8 px over the bottom bar. The
window grows 24 px wider (916 x 572) so Refresh keeps its place at the top right."""


def layout(h):
    H = h.H
    # The date range picker holds a label1 of its own (another designer): the script's label1 is the group's Providers label.
    h.by_name["label1"] = [k for k in h.n("groupFilter").children if k.name == "label1"][0]
    WW = 916
    root = h.box(".")
    g = h.box("groupFilter", 8, 8, WW - 16)
    y0 = g.top
    checks = ("checkNoNotes", "checkUnsignedNote", "checkAllNoNotes", "checkShowExcludedCodes")
    g.checks(checks, g.left)
    # The date range under the check boxes: the composite lays out its own From and To labels and boxes.
    g.place("dateRangePicker", g.left, gap=0)
    # Clinics and Providers on one field column, 16 px after the widest check box's text.
    cw = max(h.cw(n) for n in checks)
    lw = h.lw("label1")
    M = g.left + cw + 16 + lw + 6
    cl = h.clinic_w("comboClinics")
    h.set("comboClinics", M - cl, y0, cl + 160, H)
    h.set("label1", M - 6 - lw, y0 + 27, lw, H)
    h.text_align("label1", "MiddleRight")
    h.set("comboProvs", M, y0 + 27, 160, H)
    # Group By: its radio buttons a list at the group's content edge; Refresh at the content's right edge on the first row.
    gb = h.box("groupBox1", M + 160 + 16, y0)
    gb.checks(("radioProc", "radioPatient", "radioProcDate"), gb.left)
    wr = max(h.cw(n) for n in ("radioProc", "radioPatient", "radioProcDate"))
    gb.end(w=gb.left + wr + gb.pad)
    h.set("butRefresh", g.right - 82, y0, 82, 24)
    g.bottom_used = max(g.bottom_used, h.n("groupBox1").ny + h.n("groupBox1").nh)
    g.end()
    # The grid under the group to 8 px over the bottom bar; the bottom bar 8 px from the window's bottom (the window keeps its height).
    hw = root.node.h
    yb = hw - 8 - 24
    yg = 8 + h.n("groupFilter").nh + 8
    h.set("gridMain", 8, yg, WW - 16, yb - 8 - yg)
    h.set("butPrint", 8, yb, 79, 24)
    h.set("butExport", 8 + 79 + 6, yb, 79, 24)
    root.end(w=WW, h=hw)
