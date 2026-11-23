"""FormAllocationsSetup (Allocations Setup) on the grid (UI-12 slice 1, layout script of grid_hand.py). The planner left the radio buttons'
descriptions at three x positions per group, the two groups at two right edges, the prepayment group's check box on a field column with its
note right-aligned under it, and the check boxes' notes at two x positions; the script lays the window out: the explanation and its three
buttons in a left column, the Paysplits and Adjustments groups beside it to the content edge (each radio button with its description on one x
for both groups, the description's first line on the button's line), the prepayment group across the window (its check box at the content
edge with the hidden refundable box beside it, the note under the check box's text, the unearned type row), the five check boxes as a list
with their notes on one x, No Permission before Save on the bottom bar."""
import grid_tools as gt


def note_h(h, name, room):
    n = h.n(name)
    return gt.measured_lines(n.text, n.text_w, room) * 16


def layout(h):
    H = h.H
    CH = h.CH
    root = h.box(".")
    WW = root.node.w
    R = WW - 8
    LW = 180
    hn = note_h(h, "label40", LW)
    h.set("label40", 8, 8, LW, hn)
    y = 8 + hn + 8
    for name in ("butLineItem", "butDefault", "butSimple"):
        h.set(name, 8, y, LW, 24)
        y += 24 + 4
    left_bottom = y - 4
    XG = 8 + LW + 16
    GW = R - XG
    radios = ("radioPayEnforce", "radioPayAuto", "radioPayDont", "radioAdjustEnforce", "radioAdjustLink", "radioAdjustDont")
    NX = 8 + max(h.cw(r) for r in radios) + 16
    yg = 8
    for gname, rows in (("groupBoxOD1", (("radioPayEnforce", "label1"), ("radioPayAuto", "label3"), ("radioPayDont", "label2"))),
                        ("groupBoxOD2", (("radioAdjustEnforce", "label8"), ("radioAdjustLink", "label6"), ("radioAdjustDont", "label7")))):
        g = h.box(gname, XG, yg, GW)
        room = g.right - NX
        for radio, note in rows:
            nh = note_h(h, note, room)
            h.check_left(radio)
            h.set(radio, g.left, g.y - 2, h.cw(radio), CH)
            h.set(note, NX, g.y, room, nh)
            h.text_align(note, "TopLeft")
            g.bottom_used = g.y + max(nh, CH - 2)
            g.y += max(nh, CH - 2) + 6
        g.end()
        yg += h.n(gname).nh + 8
    # The prepayment group across the window.
    yt = max(left_bottom, yg - 8) + 8
    t = h.box("groupBox5", 8, yt, R - 8)
    y0 = t.top
    h.check_left("checkAllowPrePayToTpProcs")
    h.set("checkAllowPrePayToTpProcs", t.left, y0, h.cw("checkAllowPrePayToTpProcs"), CH)
    h.check_left("checkIsRefundable")
    h.set("checkIsRefundable", t.left + h.cw("checkAllowPrePayToTpProcs") + 16, y0, h.cw("checkIsRefundable"), CH)
    room = t.right - (t.left + 16)
    nh = note_h(h, "labelRefundable", room)
    h.set("labelRefundable", t.left + 16, y0 + CH + 2, room, nh)
    h.text_align("labelRefundable", "TopLeft")
    t.y = y0 + CH + 2 + nh + 4
    t.bottom_used = t.y - 4
    l57 = h.lw("label57")
    t.field("label57", "comboTpUnearnedType", t.left + l57 + 6, 164, lw=l57)
    t.end()
    # The check boxes, their notes on one x.
    checks = ("checkHidePaysplits", "checkShowIncomeTransferManager", "checkClaimPayByTotalSplitsAuto", "checkIncomeTransfersMadeUponClaimReceived",
              "checkAdjustmentsOffset")
    NX2 = 8 + max(h.cw(c) for c in checks) + 16
    y = yt + h.n("groupBox5").nh + 8
    for c in checks:
        h.check_left(c)
        h.set(c, 8, y, h.cw(c), CH)
        if c == "checkHidePaysplits":
            h.set("label4", NX2, y, h.lw("label4"), CH)
        if c == "checkIncomeTransfersMadeUponClaimReceived":
            h.set("labelIncomeTransfersMadeUponClaimReceivedDesc", NX2, y, h.lw("labelIncomeTransfersMadeUponClaimReceivedDesc"), CH)
        y += CH + 4
    yb = max(root.node.h - 8 - 24, y - 4 + 12)
    h.set("butOK", R - 75, yb, 75, 24)
    lp = h.lw("labelPermission")
    h.set("labelPermission", R - 75 - 16 - lp, yb, lp, 24)
    h.text_align("labelPermission", "MiddleRight")
    root.end(w=WW, h=yb + 24 + 8)
