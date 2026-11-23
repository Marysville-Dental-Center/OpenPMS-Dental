"""FormClaimPayBatch (Insurance Payment batch) on the grid (UI-12 slice 2 continuation, mode hand: the instructions column and the two grids
with their filter group). As main arranges it, on the grid's numbers: the instructions column at the content edge (its caption, then the note
under it as wide as main's), EOB is Scanned with its box and View ERA / View EOB at the column's bottom, on the grids' bottom; the right
column 16 px after it to the right content edge: the Payment Details group (three field columns 16 px apart, each label right-aligned 6 px
before its box at the row pitch: Clinic, Payment Date, Date Issued and Amount; Carrier Name and Note to one right edge, Check # and
Bank-Branch under them; Type and Group on that right edge; Edit on the last row at the content edge), the attached grid 8 px under it, the
row of order arrows, Refresh / Attach / Detach (centred under the grid) and Total Payments (to the right edge) 8 px under the grid, the
Filters group (one row: Carrier with its pick button, Name, Claim ID, 16 px between bands) 8 px under the row, the outstanding claims grid 8
px under it down to 8 px over the bottom bar; Delete and Save on the bottom bar. The code only hides controls (FormClaimPayBatch.cs)."""


def label_field(h, lab, fld, x, y, lw, fw):
    """A label right-aligned in lw before its field at x + lw + 6 (fw: the field's width). Returns the field's right edge."""
    H = h.H
    h.set(lab, x, y, lw, H)
    h.text_align(lab, "MiddleRight")
    h.set(fld, x + lw + 6, y, fw, H)
    return x + lw + 6 + fw


def layout(h):
    H = h.H
    P = H + 4
    root = h.tree
    W, HH = root.w, root.h
    R = W - 8
    yb = HH - 8 - 24
    # The instructions column.
    LW = 208
    h.set("labelInstruct1", 8, 8, LW, 17)
    h.text_align("labelInstruct1", "BottomLeft")
    ni = h.n("labelInstruct2")
    h.set("labelInstruct2", 8, 8 + 17 + 4, LW, ni.nh)
    xr = 8 + LW + 16
    RW = R - xr
    # The Payment Details group.
    top = (h.n("groupBox1").title_bottom or 18) + 4
    gw = RW
    cr = gw - 8
    be = h.n("butClaimPayEdit")
    xe = cr - be.nw
    E = xe - 16  # the right edge of Carrier Name, Note, Type and Group
    lw1 = max(h.lw(n) for n in ("labelClinic", "label6", "labelDateIssued", "label5"))
    x = 8
    r1 = 0
    for i, (lab, fld) in enumerate((("labelClinic", "textClinic"), ("label6", "textDate"), ("labelDateIssued", "textDateIssued"), ("label5", "textAmount"))):
        r1 = label_field(h, lab, fld, x, top + i * P, lw1, 100)
    x2 = r1 + 16
    lw2 = max(h.lw(n) for n in ("label7", "label2", "label4", "label3"))
    F2 = x2 + lw2 + 6
    r2 = 0
    for i, (lab, fld) in enumerate((("label7", "textCarrierName"), ("label2", "textNote"))):
        label_field(h, lab, fld, x2, top + i * P, lw2, E - F2)
    for i, (lab, fld) in enumerate((("label4", "textCheckNum"), ("label3", "textBankBranch"))):
        r2 = label_field(h, lab, fld, x2, top + (i + 2) * P, lw2, 120)
    x3 = r2 + 16
    lw3 = max(h.lw("label10"), h.lw("label13"))
    for i, (lab, fld) in enumerate((("label10", "textPayType"), ("label13", "textPayGroup"))):
        label_field(h, lab, fld, x3, top + (i + 2) * P, lw3, E - (x3 + lw3 + 6))
    h.set("butClaimPayEdit", xe, top + 3 * P, be.nw, 24)
    gh = top + 3 * P + H + 8
    h.set("groupBox1", xr, 8, gw, gh)
    h.watch("groupBox1")
    y = 8 + gh + 8
    # The attached grid and the row under it.
    ga = h.n("gridAttached")
    yrow = y + ga.nh + 8
    h.set("gridAttached", xr, y, RW, ga.nh)
    h.set("butUp", xr, yrow, 39, 24)
    h.set("butDown", xr + 39 + 6, yrow, 39, 24)
    mid = [("butRefresh", 60), ("butAttach", 75), ("butDetach", 75)]
    mw = sum(w for _, w in mid) + 6 * (len(mid) - 1)
    xm = xr + (RW - mw) // 2
    for name, w in mid:
        h.set(name, xm, yrow, w, 24)
        xm += w + 6
    lt = h.lw("label8")
    h.set("textTotal", R - 81, yrow, 81, H)
    h.set("label8", R - 81 - 6 - lt, yrow, lt, H)
    h.text_align("label8", "MiddleRight")
    y = yrow + 24 + 8
    # The Filters group: one row.
    ft = (h.n("groupFilters").title_bottom or 18) + 4
    x = 8
    x = label_field(h, "label9", "textCarrier", x, ft, h.lw("label9"), 132)
    h.set("butCarrierPick", x + 6, ft, 24, H)
    x += 6 + 24 + 16
    x = label_field(h, "label12", "textName", x, ft, h.lw("label12"), 132) + 16
    label_field(h, "label11", "textClaimID", x, ft, h.lw("label11"), 132)
    fh = ft + H + 8
    h.set("groupFilters", xr, y, RW, fh)
    h.watch("groupFilters")
    y += fh + 8
    h.set("gridOut", xr, y, RW, yb - 8 - y)
    # The instructions column's bottom: View ERA and View EOB on the grids' bottom, EOB is Scanned over them.
    yv = yb - 8 - 24
    xv = 8 + LW - 72
    h.set("butView", xv, yv, 72, 24)
    h.set("butViewEra", xv - 6 - 72, yv, 72, 24)
    ye = yv - 4 - H
    h.set("textEobIsScanned", xv, ye, 72, H)
    le = h.lw("label1")
    h.set("label1", xv - 6 - le, ye, le, H)
    h.text_align("label1", "MiddleRight")
    # The instructions note stops 8 px over EOB is Scanned.
    h.set("labelInstruct2", None, None, None, min(ni.nh, ye - 8 - (8 + 17 + 4)))
    h.set("butDelete", 8, yb, 79, 24)
    h.set("butSave", R - 75, yb, 75, 24)
