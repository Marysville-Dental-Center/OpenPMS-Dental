"""FormBilling on the grid (UI-12 slice 2, mode hand: the planner found no fixed point and spread the filter row across a 1560 px window).
As main arranges it, on the grid: the Unsent/Sent radio list at the content edge on the two filter rows (27 px pitch from 8); Order by and
Email From right-aligned 6 px before one column of combo boxes, the clinic picker 16 px after them on the first row; Start Date and End Date
6 px before one date column that ends 16 px before the right column; Refresh in the right column on the first row. The right column (110 px,
the width the counts' longest runtime text needs: "SentElect=99999", 93 px) at the content's right edge: Edit Selected on the grid's top, the
Counts group under it (its five labels 18 px apart), Defaults, the send note and Send at the bottom right. The grid from the content edge to
8 px before the column, 8 px under the filter rows down to 8 px over the bottom bar; All, None, Print List 6 px apart and the print note 6 px
after them on the bar."""


def layout(h):
    H = h.H
    P = H + 4
    root = h.tree
    W, HH = root.w, root.h
    R = W - 8
    CW = 110
    CX = R - CW
    y0, y1 = 8, 8 + P
    for name, y in (("radioUnsent", y0), ("radioSent", y1)):
        h.check_left(name)
        h.set(name, 8, y + (H - h.CH) // 2, h.cw(name), h.CH)
    lw = max(h.lw("label4"), h.lw("label6"))
    xl = 8 + max(h.cw("radioUnsent"), h.cw("radioSent")) + 16
    F = xl + lw + 6
    for lab, fld, y in (("label4", "comboOrder", y0), ("label6", "comboEmailFrom", y1)):
        h.set(lab, xl, y, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F, y, 173, H)
    cw = h.clinic_w("comboClinic")
    h.set("comboClinic", F + 173 + 16, y0, 226, H)
    DW = 77
    DX = CX - 16 - DW
    lw2 = max(h.lw("label3"), h.lw("label2"))
    for lab, fld, y in (("label3", "textDateStart", y0), ("label2", "textDateEnd", y1)):
        h.set(lab, DX - 6 - lw2, y, lw2, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, DX, y, DW, H)
    h.set("butRefresh", CX, y0, CW, H)
    yg = y1 + H + 8
    yb = HH - 8 - 24
    h.set("gridBill", 8, yg, CX - 8 - 8, yb - 8 - yg)
    h.set("butEdit", CX, yg, CW, 24)
    gtop = (h.n("groupBox1").title_bottom or 18) + 4
    y = gtop
    for name in ("labelTotal", "labelSelected", "labelPrinted", "labelEmailed", "labelSentElect"):
        h.set(name, 8, y, CW - 16, 18)
        h.text_align(name, "MiddleLeft")
        y += 18
    h.set("groupBox1", CX, yg + 24 + 8, CW, y + 8)
    h.set("butSend", CX, yb, CW, 24)
    h.set("label1", CX, yb - 8 - 90, CW, 90)
    h.set("butDefaults", CX, yb - 8 - 90 - 8 - 24, CW, 24)
    x = 8
    for name in ("butAll", "butNone", "butPrintList"):
        b = h.n(name)
        h.set(name, x, yb, b.nw, 24)
        x += b.nw + 6
    h.set("label5", x, yb, h.lw("label5"), 24)
    h.text_align("label5", "MiddleLeft")
    h.watch("groupBox1")
