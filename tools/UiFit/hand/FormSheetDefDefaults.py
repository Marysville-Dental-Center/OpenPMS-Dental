"""FormSheetDefDefaults on the grid (UI-12 slice 5, layout script on the planner's tree). The note's third line was cut by the first row,
the two groups stood at 39 with their fields at 145, and the clinic picker's box at 82 (the window's field column is 145): the note at the
content edge on its four lines; the Label row; the groups at the content edge with their fields on the window's field column (labels
right-aligned 6 px before), each as wide as its content; the picker's box on the column; 8 px between the blocks."""


def layout(h):
    H = h.H
    R = h.tree.nw - 8
    fx = h.n("comboLabel").nx
    h.set("label4", 8, 8, R - 8, 66)
    y = 8 + 66 + 8
    l5 = h.n("label5")
    h.set("label5", fx - 6 - l5.nw, y)
    h.set("comboLabel", fx, y + (l5.nh - H) // 2)
    y += l5.nh + 8
    gx = fx - 8
    for grp, rows in (("groupBoxOD2", (("label8", "comboStatements"), ("label1", "comboLimited"), ("label2", "comboInvoice"), ("label3", "comboReceipt"))),
                      ("groupBoxOD1", (("label7", "comboRx"), ("labelChartLayout", "comboBoxChartLayout"), ("labelTreatmentPlan", "comboTreatmentPlan")))):
        fw = 0
        for lab, fld in rows:
            f = h.n(fld)
            fw = f.nw
            h.set(fld, gx, None)
            lw = h.lw(lab)
            h.set(lab, gx - 6 - lw, None, lw, None)
            h.text_align(lab, "MiddleRight")
        g = h.n(grp)
        h.set(grp, 8, y, gx + fw + 8 - 8 + 8, None)
        y += g.nh + 8
        if grp == "groupBoxOD2":
            cw = h.clinic_w("comboClinicDefault")
            c = h.n("comboClinicDefault")
            h.set("comboClinicDefault", fx - cw, y, cw + 200, H)
            y += H + 8
