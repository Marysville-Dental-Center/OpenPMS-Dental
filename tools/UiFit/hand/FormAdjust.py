"""FormAdjust (Adjustment) on the grid (UI-12 slice 1, layout script of grid_hand.py). The planner put the Procedure group's fields in three
x positions (Tooth and Description in a second band, Patient Paid and This Adjustment in a second right column) and left the group, the TSI
box and the note off the window's columns; the script lays the window out as drawn: the adjustment's fields on one field column (the Fixed
Amount and Percent of Fee radio buttons after the date boxes, the TSI box's box on the column), the Additions and Subtractions lists beside
them down to the TSI row, the Procedure group across the window (Attach and Detach, the procedure's fields on one column, the amounts on a
second column ending at the content edge, the hidden Edit Anyway row at the end of the left column), the note under it, the bottom bar. The
window grows to 731 x 592."""


def layout(h):
    H = h.H
    CH = h.CH
    root = h.box(".")
    WW = root.node.w
    R = WW - 8
    labels = ("label8", "label1", "label7", "label5", "label2")
    lw = max(h.lw(x) for x in labels)
    F = 8 + lw + 6
    c = h.box(".")
    c.field("label8", "textDateEntry", F, 80, lw=lw)
    xr = F + 80 + 16
    c.field("label1", "textAdjDate", F, 80, lw=lw, after=[("radioFixedAmount", xr, None)])
    c.field("label7", "textProcDate", F, 80, lw=lw, after=[("radioPercentOfFee", xr, None)])
    c.field("label5", "textAmount", F, 80, lw=lw, after=[("labelPercentOfFeeAmount", xr, h.lw("labelPercentOfFeeAmount"))])
    c.field("label2", "comboProv", F, 152, lw=lw, after=[("butPickProv", None, 24)])
    yc = c.y
    cl = h.clinic_w("comboClinic")
    h.set("comboClinic", F - cl, yc, cl + 182, H)
    c.y = yc + H + 4
    c.checks(("checkOnlyTsiExcludedAdjTypes",), F)
    yl = c.bottom_used
    # The two lists beside the fields, their captions over them, down to the TSI row.
    x1 = F + max(182, h.cw("checkOnlyTsiExcludedAdjTypes")) + 16
    wl = (R - x1 - 8) // 2
    x2 = R - wl
    for cap, lst, x in (("labelAdditions", "listTypePos", x1), ("labelSubtractions", "listTypeNeg", x2)):
        h.set(cap, x, 8, wl, 18)
        h.set(lst, x, 8 + 18 + 2, wl, yl - (8 + 18 + 2))
    # The Procedure group.
    yg = yl + 8
    g = h.box("groupProcedure", 8, yg, R - 8)
    y0 = g.top
    h.set("butAttachProc", g.left, y0, 75, 24)
    h.set("butDetachProc", g.left + 75 + 6, y0, 75, 24)
    llabels = ("label19", "label17", "labelProcTooth", "label18")
    llw = max(h.lw(x) for x in llabels)
    F2 = F - 8  # the window's field column (the group stands at 8)
    g.y = y0 + 24 + 4
    g.field("label19", "textProcDate2", F2, 80, lw=llw)
    g.field("label17", "textProcProv", F2, 80, lw=llw)
    g.field("labelProcTooth", "textProcTooth", F2, 44, lw=llw)
    g.field("label18", "textProcDescription", F2, 240, lw=llw)
    rlabels = ("label15", "label16", "label14", "label9", "label10", "label11", "label13", "label12")
    rlw = max(h.lw(x) for x in rlabels)
    WF = 80
    R2 = g.right - WF
    y = y0
    for lab, fld in (("label15", "textProcFee"), ("label16", "textProcWriteoff"), ("label14", "textProcInsPaid"), ("label9", "textProcInsEst"),
                     ("label10", "textProcAdj"), ("label11", "textProcPatPaid"), ("label13", "textProcAdjCur"), ("label12", "labelProcRemain")):
        h.set(lab, R2 - 6 - rlw, y, rlw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, R2, y, WF, H)
        y += H + 4
    h.text_align("labelProcRemain", "MiddleRight")
    # The hidden Edit Anyway note and button at the end of the left column; the hidden "procedures disabled" note over the left fields, as on
    # main (one of the two shows, never both with the fields' values).
    ye = y - 4 - 28
    h.set("labelEditAnyway", g.left, ye, 250, 28)
    h.set("butEditAnyway", g.left + 250 + 6, ye + 2, 75, 24)
    h.set("labelProcDisabled", F2, y0 + 24 + 4, 236, 69)
    g.bottom_used = y - 4
    g.end()
    yn = yg + h.n("groupProcedure").nh + 8
    h.set("label4", 8, yn, h.lw("label4"), 18)
    h.set("textNote", 8, yn + 18 + 2, R - 8, 79)
    yb = yn + 18 + 2 + 79 + 8
    h.set("butDelete", 8, yb, 75, 24)
    h.set("butOK", R - 75, yb, 75, 24)
    root.end(w=WW, h=yb + 24 + 8)
