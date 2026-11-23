"""FormEdgeExpressSetup on the grid (UI-12 slice 2, mode hand). The planner cut the clinic note (446 px on one line in 413), put the picker,
Payment Type and the credential fields on three columns and left 20 px of air in the Card Entry Page group. The two check boxes a list at
the content edge; the clinic note on two lines across the content; one field column for the window (the picker's box, Payment Type, the
three credentials in their nested group, Return mode and Return URL: 6 px after the widest label of them all), each field to its
container's content edge; the payment settings' check boxes a list at the group's content edge; the return URL note under its field; each
group as tall as its content + 8, 8 px apart; Save at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    W = root.w
    R = W - 8
    y = 8
    for name in ("checkUseTestGateway", "checkEnabled"):
        h.check_left(name)
        h.set(name, 8, y, h.cw(name), CH)
        y += CH + 4
    y += 4
    h.set("labelClinicEnable", 8, y, R - 8, 32)
    h.text_align("labelClinicEnable", "MiddleLeft")
    y += 32 + 4
    lw = max(h.lw(n) for n in ("labelPaymentType", "labelXWebID", "labelAuthKey", "labelTerminalID", "labelReturnMode", "labelReturnUrl"))
    F = 8 + 8 + 8 + lw + 6  # the window's field column: the nested credentials group's labels start at its content edge (24)
    cw = h.clinic_w("comboClinic")
    h.set("comboClinic", F - cw, y, R - 8 - (F - cw), H)
    y += H + 8
    gw = R - 8
    # Payment settings
    gp = h.n("groupPaySettings")
    top = (gp.title_bottom or 18) + 4
    gy = top
    Fg = F - 8
    h.set("labelPaymentType", Fg - 6 - lw, gy, lw, H)
    h.text_align("labelPaymentType", "MiddleRight")
    h.set("comboPaymentType", Fg, gy, gw - 8 - Fg, H)
    gy += P
    for name in ("checkPromptSig", "checkPrintReceipt", "checkForceDuplicate", "checkPreventSavingNewCC", "checkWebPayEnabled"):
        h.check_left(name)
        h.set(name, 8, gy, h.cw(name), CH)
        gy += CH + 4
    gy += 4
    gc = h.n("groupCredentials")
    ctop = (gc.title_bottom or 18) + 4
    cwid = gw - 16
    Fc = F - 16
    cy = ctop
    for lab, fld in (("labelXWebID", "textXWebID"), ("labelAuthKey", "textAuthKey"), ("labelTerminalID", "textTerminalID")):
        h.set(lab, Fc - 6 - lw, cy, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, Fc, cy, cwid - 8 - Fc, H)
        cy += P
    ch = cy - 4 + 8
    h.set("groupCredentials", 8, gy, cwid, ch)
    h.watch("groupCredentials")
    gy += ch + 8
    h.set("groupPaySettings", 8, y, gw, gy)
    h.watch("groupPaySettings")
    y += gy + 8
    # Card entry page
    ge = h.n("groupCardEntry")
    ey = (ge.title_bottom or 18) + 4
    for lab, fld in (("labelReturnMode", "comboReturnMode"), ("labelReturnUrl", "textReturnUrl")):
        h.set(lab, Fg - 6 - lw, ey, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, Fg, ey, gw - 8 - Fg, H)
        ey += P
    h.set("labelReturnUrlHint", Fg, ey - 2, gw - 8 - Fg, 32)
    h.text_align("labelReturnUrlHint", "TopLeft")
    ey += 32 - 2 + 8
    h.set("groupCardEntry", 8, y, gw, ey)
    h.watch("groupCardEntry")
    y += ey + 8
    hr = max(root.h, y + 24 + 8)
    h.set("butSave", R - 75, hr - 8 - 26, 75, 26)
    h.set(".", None, None, W, hr)
