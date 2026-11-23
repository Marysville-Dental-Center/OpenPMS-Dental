"""FormPayConnect2 on the grid (UI-12 slice 3, mode hand: the planner left the left column 21 px in). FormPayConnect2.cs sizes the window to
430 x 516 (the card fields only, client width 414) or 959 x 516 with the payment page shown; the designer is the wide one. The left column
stays inside the narrow window (8 px from its edge): the Transaction Type and Processing Method groups one width (their choices in a row,
16 px apart), Terminal for transaction as a captioned box under them; Amount (and Credit Card, shown by the code, beside it),
Reference Number as captioned boxes, Save Card, the signature box. The payment page to the wide window's right edge with the update note and
Refresh under it; Save at the bottom right (anchored, it follows the window's edge in both sizes)."""

PAD = 8
GAP = 8
NARROW = 414  # the client width of the 430 px window the code sets


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    LR = NARROW - PAD
    gw = 0
    for grp, radios in (("groupTransType", ("radioSale", "radioAuthorization", "radioVoid", "radioRefund")),
                        ("groupProcessMethod", ("radioWebService", "radioTerminal"))):
        x = PAD
        for r in radios:
            x += h.cw(r) + 16
        gw = max(gw, x - 16 + PAD)
    y = PAD
    for grp, radios in (("groupTransType", ("radioSale", "radioAuthorization", "radioVoid", "radioRefund")),
                        ("groupProcessMethod", ("radioWebService", "radioTerminal"))):
        g = h.n(grp)
        top = (g.title_bottom or 18) + 4
        x = PAD
        for r in radios:
            h.check_left(r)
            h.set(r, x, top, h.cw(r), CH)
            x += h.cw(r) + 16
        gh = top + CH + PAD
        h.set(grp, PAD, y, gw, gh)
        y += gh + GAP
    y = h.captioned("labelTerminal", "comboTerminal", PAD, y, 160) + GAP
    fw = h.n("textAmount").nw
    h.captioned("labelAmount", "textAmount", PAD, y, fw)
    h.captioned("labelCreditCard", "textCreditCard", PAD + fw + 16, y, h.n("textCreditCard").nw)
    y += 17 + 2 + H + GAP
    y = h.captioned("labelRefNumber", "textRefNumber", PAD, y, fw) + GAP
    h.check_left("checkSaveToken")
    h.set("checkSaveToken", PAD, y, h.cw("checkSaveToken"), CH)
    y += CH + GAP
    h.set("sigBoxWrapper", PAD, y, h.n("sigBoxWrapper").nw, h.n("sigBoxWrapper").nh)
    assert PAD + h.n("sigBoxWrapper").nw <= LR
    # The payment page (the wide window).
    wv = h.n("webViewMain")
    by = HH - PAD - 24
    wx = R - wv.nw
    h.set("webViewMain", wx, PAD, wv.nw, by - GAP - PAD)
    h.set("butRefresh", wx, by, None, 24)
    lw = h.lw("label1")
    h.set("label1", wx - 6 - lw, by, lw, 24)
    h.text_align("label1", "MiddleRight")
    h.set("butSave", R - h.n("butSave").nw, by, None, 24)
    h.watch(".", "groupTransType", "groupProcessMethod")
