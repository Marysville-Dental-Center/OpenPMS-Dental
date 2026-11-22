"""ControlAccount (the Account module) on the grid where its designer places the controls (UI-12 round 3, layout script of grid_hand.py). The
module's code puts its split container 63 px from the top (LayoutManager.MoveLocation(splitContainerParent, ...Scale(63))), so the aging strip
keeps its 37 px between the toolbar and the grids: a caption row and a value row, the four aging boxes one width 4 px apart with their
captions centred over them, the amount columns one width per kind with 4 px around each separator line. The side panel's Main page: captions 2 px over
their boxes, the two buttons one width centred on the page, a section gap before the family notes. Its Show page: one field column for the
dates and the buttons under them, the check boxes one list. The insurance and discount summary groups (hidden in the designer, shown over the
grids by the module's code) keep their places."""


def layout(h):
    H = h.H
    h.watch("panelAging", "tabMain", "tabShow")
    # ---- The aging strip (panelAging, 749 x 37) ------------------------------------------------------------------------------------------
    ycap, hcap, yval = 2, 14, 17
    h.set("label2", 4, 2, 56, 33)
    h.text_align("label2", "MiddleRight")
    xb, wbox, gap = 66, 50, 4
    for i, (lb, tb) in enumerate((("label7", "text0_30"), ("label6", "text31_60"), ("label5", "text61_90"), ("label3", "textOver90"))):
        x = xb + i * (wbox + gap)
        h.set(lb, x, ycap, wbox, hcap)
        h.text_align(lb, "BottomCenter")
        h.set(tb, x, 15, wbox, 20, why="the aging boxes: one width, 4 px apart (the strip is 37 px tall: the module's code places the grids under it)")
    x = xb + 4 * (wbox + gap) - gap + 6

    def column(cap, amt, x, wcol):
        h.set(cap, x, ycap, wcol, hcap)
        h.text_align(cap, "BottomCenter")
        h.set(amt, x, yval, wcol, 18)
        h.text_align(amt, "MiddleCenter")
        return x + wcol

    # The amounts in bold (Total, Balance) are 80 wide, the others 72; a separator line 4 px from the columns on each side.
    x = column("labelTotal", "labelTotalAmt", x, 80)
    h.set("panelAgeLine", x + 4, 3, 2, 32)
    x = column("labelInsEst", "labelInsEstAmt", x + 10, 72)
    x = column("labelBalance", "labelBalanceAmt", x + 4, 80)
    h.set("panel1", x + 4, 3, 2, 32)
    x = column("labelPatEstBal", "labelPatEstBalAmt", x + 10, 72)
    h.set("panel2", x + 4, 3, 2, 32)
    x = column("labelUnearned", "labelUnearnedAmt", x + 10, 72)
    h.set("labelInsRem", x + 6, 2, 49, 33)
    h.set("labelDisRem", x + 6, 2, 49, 33)

    # ---- The side panel, Main page (182 wide) ------------------------------------------------------------------------------------------
    # Fix round: the page's content 4 px in from the page's frame on every side, as the Show page (the owner: "text shouldn't be touched by a
    # border"; the captions and the check box stood on the frame), the check box 8 px in as on main.
    x0 = 4
    wm = 182 - 2 * x0
    y = x0
    h.set("labelUrgFinNote", x0, y, wm, 17)
    h.text_align("labelUrgFinNote", "BottomLeft")
    y += 17 + 2
    h.set("textUrgFinNote", x0, y, wm, 80)
    y += 80 + 4
    wbut = 120
    for name in ("butServiceDateView", "butCreditCard"):
        h.set(name, x0 + (wm - wbut) // 2, y, wbut, H)
        y += H + 4
    h.set("gridAcctPat", x0, y, wm, 180)
    y += 180 + 4
    h.set("checkUseSuperFam", 8, y, h.cw("checkUseSuperFam"), h.CH)
    h.check_left("checkUseSuperFam")
    y += h.CH + 12
    h.set("labelFamFinancial", x0, y, wm, 17)
    h.text_align("labelFamFinancial", "BottomLeft")
    y += 17 + 2
    h.set("textFinNote", x0, y, wm, 110)
    w = 182

    # ---- The side panel, Show page: dates, the buttons under them, the check boxes ---------------------------------------------------------
    pad = 4
    f = pad + max(h.lw("labelStartDate"), h.lw("labelEndDate")) + 6
    wf = w - pad - f
    y = pad
    for lb, tb in (("labelStartDate", "textDateStart"), ("labelEndDate", "textDateEnd")):
        lw = h.lw(lb)
        h.set(lb, f - 6 - lw, y, lw, H)
        h.text_align(lb, "MiddleRight")
        h.set(tb, f, y, wf, H)
        y += H + 4
    for name in ("butToday", "but45days", "but90days", "butDatesAll", "butRefresh"):
        h.set(name, f, y, wf, H)
        y += H + 4
    y += 8
    for name in ("checkShowDetail", "checkShowFamilyComm", "checkShowCommAuto", "checkShowCompletePayPlans"):
        h.set(name, pad, y, min(h.cw(name), w - pad), h.CH)
        h.check_left(name)
        y += h.CH + 4
