"""FormRpOutstandingIns (Outstanding Insurance Claims) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The
planner gave the filter group's last column two left edges (Last Error Definition's combo box 23 px right of the other two, so that its long
label clears the Mine button of the For User row): the script puts the column's three fields on one x, 10 px after Mine plus the label (1000)
and on the group's content edge, each label right-aligned 6 px before."""


def layout(h):
    mine = h.n("butMine")
    right = h.n("comboLastClaimTrack").x + h.n("comboLastClaimTrack").w
    lw3 = h.lw("label3")
    x = max(h.n("comboLastClaimTrack").x, mine.x + mine.w + 10 + lw3 + 6)
    for lab, fld in (("label4", "comboLastClaimTrack"), ("labelCarrier", "textCarrier"), ("label3", "comboErrorDef")):
        h.set(fld, x, None, right - x)
        lw = h.lw(lab)
        h.set(lab, x - 6 - lw, None, lw)
        h.text_align(lab, "MiddleRight")
