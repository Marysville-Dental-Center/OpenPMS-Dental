"""FormEdgeExpressTrans on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). SetUI shows the Cash Back Amount pair or
the Transaction ID pair (or neither) by the transaction type: the planner split both pairs, each label about 110 px from its box. Both pairs
on one band under the type list (alternatives: the caption 8 px under the list, its box 2 px under the caption, both on the list's x), the
three check boxes 8 px under the band."""


def layout(h):
    lst = h.n("listBoxTransType")
    x = lst.nx
    y = lst.ny + lst.nh + 8
    H = h.H
    for lab, fld in (("labelCashBackAmt", "textCashBackAmt"), ("labelTransactionID", "textTransactionId")):
        h.set(lab, x, y, None, 16)
        h.text_align(lab, "BottomLeft")
        h.set(fld, x, y + 16 + 2, None, H)
    yc = y + 16 + 2 + H + 8
    for name in ("checkSaveToken", "checkSignature", "checkPrintReceipt"):
        h.set(name, x, yc, None, None)
        yc += 24
