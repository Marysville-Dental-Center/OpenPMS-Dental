"""FormPaySimple on the grid (UI-12 slice 2, mode hand: the planner put the right column's captions 9 px under the left column's and left
a 60 px band under the fields). The tab control at the content edge to the right content edge; on the Credit/Debit page the transaction
type group (its radio buttons one row, 12 px apart) and two columns of captioned fields (the caption 2 px over its box, 46 px from caption to
caption) 16 px apart: card number, expiration and name on card with One-Time Payment under them; Ref Number, security code, zip code and
amount with Print Receipt under them. The ACH page in the same style in one column at the content edge. Save at the bottom right."""


def caption(h, cap, fld, x, y, w):
    h.set(cap, x, y, w, 17)
    h.text_align(cap, "BottomLeft")
    h.set(fld, x, y + 19, w, h.H)
    return y + 19 + h.H + 4


def radios(h, grp, names, x, y, w):
    top = (h.n(grp).title_bottom or 18) + 4
    rx = 8
    for r in names:
        h.check_left(r)
        h.set(r, rx, top, h.cw(r), h.CH)
        rx += h.cw(r) + 12
    gh = top + h.CH + 8
    h.set(grp, x, y, max(w, rx - 12 + 8), gh)
    h.watch(grp)
    return y + gh + 8


def layout(h):
    CH = h.CH
    root = h.tree
    R = root.w - 8
    yb = root.h - 8 - 24
    h.set("tabControl", 8, 8, R - 8, yb - 8 - 8)
    LW, RW = 228, 136
    y = radios(h, "groupTransType", ("radioSale", "radioAuthorization", "radioVoid", "radioReturn"), 8, 8, LW)
    yl = y
    for cap, fld in (("label3", "textCardNumber"), ("label4", "textExpDate"), ("label5", "textNameOnCard")):
        yl = caption(h, cap, fld, 8, yl, LW if fld != "textExpDate" else 150)
    h.check_left("checkOneTimePayment")
    h.set("checkOneTimePayment", 8, yl, h.cw("checkOneTimePayment"), CH)
    xr = 8 + LW + 16
    yr = y
    for cap, fld in (("labelRefNumber", "textRefNumber"), ("label6", "textSecurityCode"), ("label7", "textZipCode"), ("labelAmount", "textAmount")):
        yr = caption(h, cap, fld, xr, yr, RW)
    h.check_left("checkPrintReceipt")
    h.set("checkPrintReceipt", xr, yr, h.cw("checkPrintReceipt"), CH)
    h.watch("tabCredit")
    y = radios(h, "groupBankAccountType", ("radioCheckings", "radioSavings"), 8, 8, LW)
    for cap, fld in (("label8", "textRoutingNumber"), ("label2", "textCheckSaveNumber"), ("label1", "textBankName"), ("labelAmountACH", "textAmountACH")):
        y = caption(h, cap, fld, 8, y, LW if fld != "textAmountACH" else RW)
    for name in ("checkOneTimePaymentACH", "checkPrintReceiptACH"):
        h.check_left(name)
        h.set(name, 8, y, h.cw(name), CH)
        y += CH + 4
    h.watch("tabACH")
    h.set("butSave", R - 75, yb, 75, 24)
