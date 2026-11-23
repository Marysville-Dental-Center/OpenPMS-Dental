"""FormFeesForIns on the grid (UI-12 slice 5, layout script on the planner's tree). The planner left the search note on two lines with its
second line under the first field row (cut), drew both Change buttons on one place (the second row's), kept the type list and the grid
at 13-14 px and the right combo boxes at two x (584 and 588). The note on one line over the field rows beside the list; the right column's
three rows (With Insurance Plan Type, With Fee Schedule, Without Fee Schedule) on the pitch with their boxes to the right content edge,
Carrier Like and Carrier Not Like on the second and third rows; the list from the content edge down to the third row; the grid 8 px
under it to the content edges; the two change rows over the bottom edge on the pitch, each with its Change button, Select All and Print
List on the lower row."""


def layout(h):
    H = h.H
    P = H + 4
    W = h.tree.nw
    R = W - 8
    lst = h.n("listType")
    nx = lst.nx
    h.set("listType", 8, 8, None, None)
    x3 = 8 + lst.nw + 6
    h.set("label3", x3, 8, R - x3, 18)
    y0 = 8 + 18 + 4
    cw = h.n("comboInsPlanTypeWith").nw
    cx = R - cw
    for i, (lab, fld) in enumerate((("label8", "comboInsPlanTypeWith"), ("label4", "comboFeeSchedWith"), ("label1", "comboFeeSchedWithout"))):
        y = y0 + i * P
        lw = h.lw(lab)
        h.set(lab, cx - 6 - lw, y, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, cx, y, cw, H)
    tx = h.n("textCarrier").nx
    for i, (lab, fld) in enumerate((("label2", "textCarrier"), ("label6", "textCarrierNot"))):
        y = y0 + (i + 1) * P
        lw = h.lw(lab)
        h.set(lab, tx - 6 - lw, y, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, tx, y, None, H)
    lb = y0 + 2 * P + H
    h.set("listType", 8, 8, None, lb - 8)
    y2 = h.tree.nh - 8 - H
    y1 = y2 - P
    bx = h.n("comboInsPlanType").nx
    for lab, fld, but, y in (("label7", "comboInsPlanType", "butChangeInsPlanType", y1), ("label5", "comboFeeSchedNew", "butChangeFeeSchedule", y2)):
        lw = h.lw(lab)
        h.set(lab, bx - 6 - lw, y, lw, H)
        h.text_align(lab, "MiddleRight")
        f = h.n(fld)
        h.set(fld, bx, y, None, H)
        h.set(but, bx + f.nw + 6, y, None, H)
    h.set("butSelectAll", 8, y2, None, H)
    s = h.n("butSelectAll")
    h.set("butPrint", 8 + s.nw + 6, y2, None, H)
    gt = lb + 8
    h.set("gridMain", 8, gt, R - 8, y1 - 8 - gt)
