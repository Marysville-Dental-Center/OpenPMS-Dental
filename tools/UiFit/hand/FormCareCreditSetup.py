"""FormCareCreditSetup on the grid (UI-12 slice 5, layout script on the planner's tree). The Clinic Settings group stood at 24 with its
fields at 194 (the window's at 210) and the picker's box 4 px short of the column's right edge; the Connection group kept 50 px empty under
Test connection; Run on this computer (120 px) wrapped its text onto a second, cut line. The group at the content edge to 8 px before the
Connection group, its fields, the CareCredit admin group and the picker's box on the window's field column and right edge; the
Connection group 8 px under its last row and the Background group 8 px under it; Run on this computer as wide as its text with the
computer label right-aligned 6 px before it on its row."""

import grid_tools as g


def layout(h):
    H = h.H
    fx = h.n("textMerchantNumberPractice").nx
    fw = h.n("textMerchantNumberPractice").nw
    con = h.n("groupConnection")
    cs = h.n("groupClinicSettings")
    h.set("groupClinicSettings", 8, None, con.nx - 8 - 8, None)
    gx = fx - 8
    for lab, fld in (("labelPaymentType", "comboPaymentType"), ("labelMerchantNumberClinic", "textMerchantNumberClinic")):
        h.set(fld, gx, None, fw, None)
        lw = h.lw(lab)
        h.set(lab, gx - 6 - lw, None, lw, None)
    h.set("groupPromotions", gx, None, fw, None)
    p = h.n("groupPromotions")
    b = h.n("butPromotions")
    h.set("butPromotions", (fw - b.nw) // 2, None)
    q = h.n("groupQSBatch")
    h.set("groupQSBatch", None, None, (con.nx - 8 - 8) - 8 - q.nx, None)
    cw = h.clinic_w("comboClinics")
    h.set("comboClinics", fx - cw, None, cw + fw, H)
    t = h.n("butTestConnection")
    h.set("groupConnection", None, None, None, t.ny + t.nh + 8)
    bg = h.n("groupBackground")
    h.set("groupBackground", None, con.ny + con.nh + 8)
    r = h.n("butRunHere")
    rw = g.snap_up((r.text_w or 116) + 20)
    rx = bg.nw - 8 - rw
    h.set("butRunHere", rx, 26, rw, H)
    h.set("labelJobComputer", 8, 26, rx - 6 - 8, H)
    h.text_align("labelJobComputer", "MiddleRight")
