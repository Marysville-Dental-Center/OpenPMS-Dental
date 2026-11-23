"""FormEtrans835ClaimPay on the grid (UI-12 slice 3, mode hand: the planner left the two captions over Deductible and Write-off at two
heights and away from their buttons). Main's arrangement on the grid's numbers: the three grids from the padding to the right edge; the EOB
Totals and Totals rows under their grids (the total boxes keep their places under the grids' columns, 1 px left with the grids); the write-off
check box with Split Claim at the right edge between the second and third grid; on the bottom bar Deductible and Write-off with their
captions 2 px over them, EOB Claim Details after them, the EOB note (two lines) right-aligned 16 px before Save."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    dx = PAD - h.n("gridClaimAdjustments").nx
    gw = R - PAD
    y = PAD
    h.set("gridClaimAdjustments", PAD, y, gw, 100)
    y += 100 + 4
    h.set("gridProcedureBreakdown", PAD, y, gw, 168)
    y += 168 + 4
    first = None
    for b in ("textEobClaimFee", "textEobDedApplied", "textEobInsPayAllowed", "textEobInsPayAmt"):
        n = h.n(b)
        h.set(b, n.nx + dx, y, 62, H)
        first = n.nx + dx if first is None else min(first, n.nx + dx)
    lw = h.lw("label5")
    h.set("label5", first - 6 - lw, y, lw, H)
    h.text_align("label5", "MiddleRight")
    y += H + 4
    h.check_left("checkIncludeWOPercCoPay")
    h.set("checkIncludeWOPercCoPay", PAD, y + (24 - CH) // 2, h.cw("checkIncludeWOPercCoPay"), CH)
    h.set("butSplitProcs", R - 97, y, 97, 24)
    y += 24 + GAP
    by = HH - PAD - 24
    cy = by - 2 - 17
    ty = cy - GAP - H
    h.set("gridPayments", PAD, y, gw, ty - 4 - y)
    first = None
    for b in ("textClaimFee", "textDedApplied", "textInsPayAllowed", "textInsPayAmt", "textWriteOff"):
        n = h.n(b)
        h.set(b, n.nx + dx, ty, 62, H)
        first = n.nx + dx if first is None else min(first, n.nx + dx)
    lw = h.lw("label1")
    h.set("label1", first - 6 - lw, ty, lw, H)
    h.text_align("label1", "MiddleRight")
    l3, l4 = h.lw("label3"), h.lw("label4")
    h.set("label3", PAD, cy, l3, 17)
    h.text_align("label3", "BottomLeft")
    h.set("butDeductible", PAD, by, 92, 24)
    x2 = PAD + max(92, l3) + 16
    h.set("label4", x2, cy, l4, 17)
    h.text_align("label4", "BottomLeft")
    h.set("butWriteOff", x2, by, 90, 24)
    x3 = x2 + max(90, l4) + 16
    h.set("butViewEobDetails", x3, by, 135, 24)
    h.set("butSave", R - 75, by, 75, 24)
    nw = 300
    nh = h.note_h("label2", nw)
    h.set("label2", R - 75 - 16 - nw, by + 12 - nh // 2, nw, nh)
    h.text_align("label2", "MiddleRight")
    assert x3 + 135 + 16 <= R - 75 - 16 - nw
