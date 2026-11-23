"""FormAdvertisingPostcardsSend on the grid (UI-12 slice 5, layout script on the planner's tree). The window's notes are empty in the
designer (the code writes the recipient, design, return-address and estimate notes), so the planner read them as nothing and stacked the
Design, Mail class and Mail date labels under Recipients, away from their fields. Main's rows on the grid: the sandbox note, then on the
27 px pitch Clinic, Recipients (Select Patients after the box), One card per household, the recipient note's two lines, Design (Refresh
after the box), Edit design and New design, the design warning, Mail class, Mail date, Return address with its value's four lines, Check
addresses, Estimate and Place order, the estimate note; the labels right-aligned 6 px before the field column (120). The proof caption
(on the Clinic row) and picture beside the rows to the right content edge; the blocked note and Show every order over the orders grid, the grid to the
bottom bar; the bar's buttons 6 px apart (the cutoff note between Cancel order and Open vendor portal). The window at main's size."""


def layout(h):
    H = h.H
    P = H + 4
    W, HT = 820, 698
    h.set(".", None, None, W, HT)
    R = W - 8
    FX = 120
    LW = FX - 6 - 8
    y = h.n("menuMain").ny + h.n("menuMain").nh + 4
    h.set("labelSandbox", 8, y, R - 8, 18)
    y += 18 + 4

    def lab(name, yy, hh=H):
        h.set(name, 8, yy, LW, hh)
        h.text_align(name, "MiddleRight")

    lab("labelClinic", y)
    h.set("comboClinic", FX, y, None, H)
    y += P
    lab("labelRecipients", y)
    h.set("textRecipients", FX, y, None, H)
    t = h.n("textRecipients")
    h.set("butSelectPatients", FX + t.nw + 6, y, None, H)
    y += P
    h.check_left("checkHousehold")
    h.set("checkHousehold", FX, y + (H - h.CH) // 2, None, h.CH)
    y += P
    h.set("labelRecipientNotes", FX, y, None, 30)
    y += 30 + 4
    lab("labelDesign", y)
    h.set("comboDesign", FX, y, None, H)
    d = h.n("comboDesign")
    h.set("butRefreshDesigns", FX + d.nw + 6, y, None, H)
    y += P
    e = h.n("butEditDesign")
    h.set("butEditDesign", FX, y, None, 24)
    h.set("butNewDesign", FX + e.nw + 6, y, None, 24)
    y += 24 + 4
    h.set("labelDesignWarning", FX, y, None, 30)
    y += 30 + 4
    lab("labelMailClass", y)
    h.set("comboMailClass", FX, y, None, H)
    y += P
    lab("labelMailDate", y)
    h.set("textMailDate", FX, y, None, H)
    y += P
    lab("labelReturnAddress", y)
    h.set("labelReturnAddressValue", FX, y + 3, None, 62)
    y += 62 + 4 + 3
    x = FX
    for name in ("butCheckAddresses", "butEstimate", "butSend"):
        n = h.n(name)
        h.set(name, x, y, None, 24)
        x += n.nw + 6
    y += 24 + 4
    h.set("labelEstimate", FX, y, None, 62)
    y += 62 + 8
    px = h.n("pictureProof").nx
    cy = h.n("labelClinic").ny
    h.set("labelProof", px, cy, R - px, 18)
    h.set("pictureProof", px, cy + 18 + 2, R - px, None)
    b = h.n("labelBaaBlocked")
    ca = h.n("checkAllOrders")
    h.set("checkAllOrders", R - ca.nw, y + b.nh - h.CH, None, h.CH)
    h.set("labelBaaBlocked", 8, y, R - ca.nw - 8 - 8, None)
    y += b.nh + 4
    yb = HT - 8 - 24
    h.set("gridOrders", 8, y, R - 8, yb - 8 - y)
    x = 8
    for name in ("butRefreshOrders", "butCancelOrder"):
        n = h.n(name)
        h.set(name, x, yb, None, 24)
        x += n.nw + 6
    c = h.n("butClose")
    h.set("butClose", R - c.nw, yb)
    o = h.n("butOpenPortal")
    ox = R - c.nw - 8 - o.nw
    h.set("butOpenPortal", ox, yb)
    h.set("labelCutoff", x, yb - 3, ox - 6 - x, 30)
