"""FormAdjMulti on the grid (UI-12 slice 3, mode hand: the planner kept the credit filter's choices drawn into each other and the grid at
31). Main's arrangement on the grid's numbers: the Adjustment Info group at the padding to the right edge: Date with its box and Only TSI
excluded adjustment types after it; the Adjust Per Procedure group (Amount with its box, the three choices a list 16 px after the box) and
the Override group (Provider with its picker button, the clinic picker's box on the same column) under it, one width, their field column on
the Date box's; the Additions and Subtractions lists one width under their captions, 8 px apart, to the content's right edge and the left
stack's bottom. Under the group the grid with Add, Update and Delete beside it at the right edge; the note under its caption and the Credit
Filter group (three choices) under the grid, one bottom; Save at the bottom right."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    gw = R - PAD
    g = h.n("groupBox3")
    top = (g.title_bottom or 18) + 4
    lw = max(h.lw(n) for n in ("labelAdjDate", "labelAmount", "label3"))
    F = 2 * PAD + lw + 6  # the window's column: the inner groups' labels keep their padding
    h.set("labelAdjDate", PAD, top, F - 6 - PAD, H)
    h.text_align("labelAdjDate", "MiddleRight")
    h.set("dateAdjustment", F, top, h.n("dateAdjustment").nw, H)
    tsx = F + h.n("dateAdjustment").nw + 16
    h.check_left("checkOnlyTsiExcludedAdjTypes")
    h.set("checkOnlyTsiExcludedAdjTypes", tsx, top + (H - CH) // 2, h.cw("checkOnlyTsiExcludedAdjTypes"), CH)
    # Adjust Per Procedure.
    g1 = h.n("groupBox1")
    t1 = (g1.title_bottom or 18) + 4
    F1 = F - PAD
    h.set("labelAmount", PAD, t1, lw, H)
    h.text_align("labelAmount", "MiddleRight")
    h.set("textAmt", F1, t1, h.n("textAmt").nw, H)
    rx = F1 + h.n("textAmt").nw + 16
    radios = ("radioFixedAmt", "radioPercentRemBal", "radioPercentProcFee")
    for i, r in enumerate(radios):
        h.check_left(r)
        h.set(r, rx, t1 + (H - CH) // 2 + i * (CH + 4), h.cw(r), CH)
    iw = max(rx + max(h.cw(r) for r in radios), tsx + h.cw("checkOnlyTsiExcludedAdjTypes") - PAD) + PAD
    g1h = t1 + (H - CH) // 2 + 3 * CH + 8 + PAD
    y = top + H + GAP
    h.set("groupBox1", PAD, y, iw, g1h)
    y += g1h + GAP
    # Override.
    g2 = h.n("groupBox2")
    t2 = (g2.title_bottom or 18) + 4
    right = iw - PAD
    h.set("label3", PAD, t2, lw, H)
    h.text_align("label3", "MiddleRight")
    h.set("comboProv", F1, t2, right - 24 - 6 - F1, H)
    h.set("butPickProv", right - 24, t2, 24, H)
    cl = h.clinic_w("comboClinic")
    h.set("comboClinic", F1 - cl, t2 + H + 4, right - 24 - 6 - F1 + cl, H)
    g2h = t2 + 2 * H + 4 + PAD
    h.set("groupBox2", PAD, y, iw, g2h)
    lb = y + g2h
    # The lists.
    lx = PAD + iw + 16
    cr = gw - PAD
    lwid = (cr - lx - GAP) // 2
    lx = cr - 2 * lwid - GAP
    for i, (cap, lst) in enumerate((("labelAdditions", "listTypePos"), ("labelSubtractions", "listTypeNeg"))):
        x = lx + i * (lwid + GAP)
        h.set(cap, x, top, lwid, 17)
        h.text_align(cap, "BottomLeft")
        h.set(lst, x, top + 19, lwid, lb - (top + 19))
    ih = lb + PAD
    h.set("groupBox3", PAD, PAD, gw, ih)
    # The grid, its buttons, the note and the credit filter.
    bw = 76
    bx = R - bw
    gy = PAD + ih + GAP
    for i, b in enumerate(("butAdd", "butUpdate", "butDelete")):
        h.set(b, bx, gy + i * 28, bw, 24)
    gr = bx - GAP
    gc = h.n("groupCreditLogic")
    tc = (gc.title_bottom or 18) + 4
    cradios = ("radioAllocatedOnly", "radioIncludeAll", "radioExcludeAll")
    for i, r in enumerate(cradios):
        h.check_left(r)
        h.set(r, PAD, tc + i * (CH + 4), h.cw(r), CH)
    cw = PAD + max(h.cw(r) for r in cradios) + PAD
    ch = tc + 3 * CH + 8 + PAD
    bottom = HH - PAD
    cy = bottom - ch
    h.set("groupCreditLogic", gr - cw, cy, cw, ch)
    h.set("label5", PAD, cy, h.lw("label5"), 17)
    h.text_align("label5", "BottomLeft")
    h.set("textNote", PAD, cy + 19, gr - cw - GAP - PAD, bottom - (cy + 19))
    h.set("gridMain", PAD, gy, gr - PAD, cy - GAP - gy)
    h.set("butOk", R - h.n("butOk").nw, HH - PAD - 24, None, 24)
    h.watch(".", "groupBox3", "groupBox1", "groupBox2", "groupCreditLogic")
