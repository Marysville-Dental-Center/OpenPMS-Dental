"""FormPayConnectSetup (PayConnect setup) on the grid (UI-12 slice 2 continuation, mode hand: two columns of groups whose notes wrap). The top
band: the left column at the content edge (Enabled, its box on the left; the clinic note under it; Clinic with its box on the Secret box's column to the column's edge:
the code hides the note and Clinic without clinics, FormPayConnectSetup.cs) and the Version 2.0 group 8 px after it to the right content edge
(API key with its box to the content edge, Use sandbox under the box, the note under them as tall as its lines). The Clinic Payment Settings
group 8 px under the band to both content edges: on the left Secret with its box and Test connection, the three check boxes under the box
one per row; on the right Payment Type and Default Processing Method on one field column with their boxes to the content edge and the
Payment Portal Payments group under them to the content edge. The Terminals group 8 px under it down to 8 px over the bottom bar: the grid
to the content edges, Import Terminals and Add Terminal 6 px apart at the content's bottom right. Error Log and Save on the bottom bar."""

import math


def note_h(n, w):
    """A wrapping note's height at width w: its lines (GDI+ breaks words, so 10% more than its one-line width) at 16 px + 2."""
    lines = max(1, math.ceil((n.text_w or 0) * 1.1 / max(1, w)))
    return lines * 16 + 2


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    W, HH = root.w, root.h
    R = W - 8
    yb = HH - 8 - 24
    # The left column of the top band.
    LW = 240
    h.check_left("checkEnabled")
    h.set("checkEnabled", 8, 8 + (H - CH) // 2 + (h.n("groupPracticeWide").title_bottom or 18) + 4, h.cw("checkEnabled"), CH)
    y = h.n("checkEnabled").ny + CH + 4
    ne = h.n("labelClinicEnable")
    nh = note_h(ne, LW)
    h.set("labelClinicEnable", 8, y, LW, nh)
    h.text_align("labelClinicEnable", "TopLeft")
    y += nh + 4
    # Clinic's box on the Secret box's column (the groups at the content edge: 8 + the group's field column).
    Fs = 8 + 8 + h.lw("label2") + 6
    lc = Fs - 6 - 8
    h.set("labelClinic", 8, y, lc, H)
    h.text_align("labelClinic", "MiddleRight")
    h.set("comboClinic", Fs, y, 8 + LW - Fs, H)
    yl = y + H
    # The Version 2.0 group.
    xg = 8 + LW + 8
    gw = R - xg
    top = (h.n("groupPracticeWide").title_bottom or 18) + 4
    la = h.lw("labelApiKey")
    F = 8 + la + 6
    h.set("labelApiKey", 8, top, la, H)
    h.text_align("labelApiKey", "MiddleRight")
    h.set("textAPIKey", F, top, gw - 8 - F, H)
    h.check_left("checkUseSandbox")
    h.set("checkUseSandbox", F, top + P, h.cw("checkUseSandbox"), CH)
    y = top + P + CH + 4
    npw = h.n("labelPracticeWide")
    nh = note_h(npw, gw - 16)
    h.set("labelPracticeWide", 8, y, gw - 16, nh)
    h.text_align("labelPracticeWide", "TopLeft")
    gh = y + nh + 8
    h.set("groupPracticeWide", xg, 8, gw, gh)
    h.watch("groupPracticeWide")
    y = max(8 + gh, yl) + 8
    # Clinic Payment Settings.
    top = (h.n("groupPaySettings").title_bottom or 18) + 4
    ls = h.lw("label2")
    F = 8 + ls + 6
    h.set("label2", 8, top, ls, H)
    h.text_align("label2", "MiddleRight")
    h.set("textAPISecret", F, top, 175, H)
    bm = h.n("butMerchantInfo")
    h.set("butMerchantInfo", F + 175 + 6, top, bm.nw, 24)
    yc = top + P
    for c in ("checkTerminal", "checkPreventSavingNewCC", "checkSurcharge"):
        h.check_left(c)
        h.set(c, F, yc, h.cw(c), CH)
        yc += CH + 4
    ylb = yc - 4
    xr = F + 175 + 6 + bm.nw + 16
    pw = R - 8  # the group's width
    lr = max(h.lw("label1"), h.lw("labelDefaultProcMethod"))
    Fr = xr + lr + 6
    for i, (lab, fld) in enumerate((("label1", "comboPaymentType"), ("labelDefaultProcMethod", "comboDefaultProcessing"))):
        h.set(lab, xr, top + i * P, lr, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, Fr, top + i * P, pw - 8 - Fr, H)
    yp = top + 2 * P + 4
    t2 = (h.n("groupBox1").title_bottom or 18) + 4
    h.check_left("checkPatientPortalPayEnabled")
    h.set("checkPatientPortalPayEnabled", 8, t2, h.cw("checkPatientPortalPayEnabled"), CH)
    g2h = t2 + CH + 8
    h.set("groupBox1", xr, yp, pw - 8 - xr, g2h)
    h.watch("groupBox1")
    yrb = yp + g2h
    ph = max(ylb, yrb) + 8
    h.set("groupPaySettings", 8, y, pw, ph)
    h.watch("groupPaySettings")
    y += ph + 8
    # Terminals.
    tt = (h.n("groupBoxTerminals").title_bottom or 18) + 4
    th = yb - 8 - y
    ybt = th - 8 - 24
    h.set("gridODTerminals", 8, tt, pw - 16, ybt - 8 - tt)
    ba = h.n("butAddTerminal")
    bi = h.n("butImportTerminals")
    h.set("butAddTerminal", pw - 8 - ba.nw, ybt, ba.nw, 24)
    h.set("butImportTerminals", pw - 8 - ba.nw - 6 - bi.nw, ybt, bi.nw, 24)
    h.set("groupBoxTerminals", 8, y, pw, th)
    h.watch("groupBoxTerminals")
    h.set("butErrorLog", 8, yb, h.n("butErrorLog").nw, 24)
    h.set("butSave", R - 75, yb, 75, 24)
