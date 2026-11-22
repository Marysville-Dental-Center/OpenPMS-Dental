"""FormEhrLabOrderEdit2014 (EHR Lab Order) on the grid (UI-12 slice 1, layout script of grid_hand.py). The planner widened the left column's
three group boxes across the window (1732 px) and laid the picker buttons over their labels; the script lays the window out in three columns
of group boxes (one field column per group, a group's picker button on its first row before the labels, the nested Assigning Authority and
TQ1 groups on their parent's field column), the grids and their buttons under them on the same three columns, the bottom bar. The window's
code sets its height to the screen's working area (the grids are anchored), so the designer's 805 px only sets the proportions."""


def layout(h):
    H = h.H
    root = h.box(".")
    WW = root.node.w
    R = WW - 8
    CW = (R - 8 - 16) // 3
    XL, XM, XR = 8, 8 + CW + 8, R - CW
    y0 = 8 + H + 8
    # The left column: the three order number groups.
    y = y0
    for gname, hid in (("groupBox4", "checkAutoID"), ("groupBox2", None), ("groupBox3", None)):
        b, _ = h.group_rows(gname, XL, y, CW, hidden_first=hid)
        b.end()
        y += h.n(gname).nh + 8
    bottom_l = y - 8
    # The middle column: the ordering provider with its Assigning Authority group, then Results Handling.
    b5, F5 = h.group_rows("groupBox5", XM, y0, CW, button="butProvPicker")
    yb1 = b5.y
    b1, _ = h.group_rows("groupBox1", b5.left, yb1, CW - 2 * b5.pad, align=F5 - b5.left)
    b1.end()
    b5.bottom_used = yb1 + h.n("groupBox1").nh
    b5.end()
    y7 = y0 + h.n("groupBox5").nh + 8
    b7 = h.box("groupBox7", XM, y7, CW)
    # The two options a list (side by side the second passed the group's edge).
    b7.checks(("checkResultsHandlingF", "checkResultsHandlingN"), b7.left)
    b7.end()
    bottom_m = y7 + h.n("groupBox7").nh
    # The right column: the service identifier, then TQ1 on its field column.
    b6, F6 = h.group_rows("groupBox6", XR, y0, CW, button="butServicePicker")
    b6.end()
    y8 = y0 + h.n("groupBox6").nh + 8
    b8, _ = h.group_rows("groupBox8", XR, y8, CW, align=F6)
    b8.end()
    bottom_r = y8 + h.n("groupBox8").nh
    # The top row: View, the parent result on the ordering provider's field column, the last update on the service identifier's field column
    # (the patient row, hidden in the designer, at the content edge before them).
    xp = XM + F5
    lp = h.lw("label16")
    h.set("label16", xp - 6 - lp, 8, lp, H)
    h.text_align("label16", "MiddleRight")
    h.set("butViewParent", XM, 8, xp - 6 - lp - 6 - XM, H)
    h.set("textParentOrderNum", xp, 8, 156, H)
    h.set("butParentPicker", xp + 156 + 6, 8, 24, H)
    lpat = h.lw("labelPatient")
    h.set("labelPatient", 8, 8, lpat, H)
    h.text_align("labelPatient", "MiddleRight")
    h.set("textName", 8 + lpat + 6, 8, XM - 8 - 6 - 24 - 6 - (8 + lpat + 6), H)
    h.set("butPatientPick", XM - 8 - 24, 8, 24, H)
    xu = XR + F6
    lu = h.lw("label20")
    h.set("textResultDateTime", xu, 8, R - xu, H)
    h.set("label20", xu - 6 - lu, 8, lu, H)
    h.text_align("label20", "MiddleRight")
    # The grids: notes and copy-to under the left and middle columns, clinical information and specimens under the right column with their
    # buttons at the content edge; lab results under the left and middle columns; the result status and specimen action rows; the bottom bar.
    yg = max(bottom_l, bottom_m, bottom_r) + 8
    BW = 86
    XB = R - BW
    WRG = XB - 6 - XR
    MR = XM + CW
    h.set("gridNotes", XL, yg, XM - 8 - XL + 120, 101)
    h.set("gridResultsCopyTo", XM + 120, yg, MR - XM - 120, 101)
    h.set("gridClinicalInformation", XR, yg, WRG, 101)
    for i, name in enumerate(("butAddNote", "butAddCopyTo", "butAddClinicalInfo")):
        h.set(name, XB, yg + i * 28, BW, 24)
    y2 = yg + 101 + 8
    h.set("gridSpecimen", XR, y2, WRG, 145)
    h.set("butAdd", XB, y2, BW, 24)
    h.set("butAddSpecimens", XB, y2 + 28, BW, 24)
    h.set("label19", XR, y2 + 145 + 4, WRG, 18)
    ys = y2 + 145 + 4 + 18 + 2
    h.set("comboSpecimenActionCode", XR, ys, R - XR, H)
    h.set("gridMain", XL, y2, MR - XL, ys - 4 - y2)
    l26 = h.lw("label26")
    h.set("label26", XL, ys, l26, H)
    h.text_align("label26", "MiddleRight")
    h.set("comboResultStatus", XL + l26 + 6, ys, MR - (XL + l26 + 6), H)
    yb = ys + H + 8
    h.set("butDelete", XL, yb, 75, 24)
    h.set("butManageImages", XM, yb, 93, 24)
    h.set("labelManageImages", XM + 93 + 6, yb, h.lw("labelManageImages"), 24)
    h.text_align("labelManageImages", "MiddleLeft")
    h.set("butOk", R - 75, yb, 75, 24)
    root.end(w=WW, h=yb + 24 + 8)
