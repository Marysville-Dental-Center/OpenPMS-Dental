"""FormEtrans270Edit on the grid (UI-12 slice 2, mode hand: the planner left the bottom block's rows at four heights and the top groups at
two widths). The top band from 8: the dates grid at the content edge, the viewing mode and import groups stacked 8 px after it (one width,
each as tall as its two radio buttons), the raw message group at the right content edge with Print 8 px under it; the band as tall as the two
stacked groups (the dates grid with it). The main grid 8 px under the band to both content edges. Under it: the coinsurance group at the
content edge (as wide as its radio buttons), the two group number rows 16 px after it on one field column, Import and its Selected Benefits
note 16 px after them, the current benefits grid to the right content edge down to 8 px over the bottom bar; the Note caption and box at
the content edge ending 8 px over the bar; Delete and Save on the bar."""


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    R = root.w - 8
    yb = root.h - 8 - 24
    t = 22
    GW = 167
    gx = 8 + 407 + 8
    y = 8
    for grp, r1, r2 in (("groupBox3", "radioModeElect", "radioModeMessage"), ("groupImport", "radioInNetwork", "radioOutNetwork")):
        for i, r in enumerate((r1, r2)):
            h.check_left(r)
            h.set(r, 8, t + i * (CH + 4), h.cw(r), CH)
        gh = t + 2 * CH + 4 + 8
        h.set(grp, gx, y, GW, gh)
        h.watch(grp)
        y += gh + 8
    band = y - 8
    h.set("gridDates", 8, 8, 407, band - 8)
    h.set("butShowRequest", 8, t, 75, 24)
    h.set("butShowResponse", 8 + 75 + 6, t, 75, 24)
    rw = 8 + 75 + 6 + 75 + 8
    h.set("groupBox2", R - rw, 8, rw, t + 24 + 8)
    h.watch("groupBox2")
    h.set("butPrint", R - 75, 8 + t + 24 + 8 + 8, 75, 24)
    ym = band + 8
    MH = 254
    h.set("gridMain", 8, ym, R - 8, MH)
    yB = ym + MH + 8
    # Coinsurance group
    wr = max(h.cw("radioBenefitSendsPat"), h.cw("radioBenefitSendsIns"))
    for i, r in enumerate(("radioBenefitSendsPat", "radioBenefitSendsIns")):
        h.check_left(r)
        h.set(r, 8, t + i * (CH + 4), wr, CH)
    gw = 8 + wr + 8
    h.set("groupImportBenefit", 8, yB, gw, t + 2 * CH + 4 + 8)
    h.watch("groupImportBenefit")
    xg = 8 + gw + 16
    lw = max(h.lw("labelCurrentGroupNum"), h.lw("labelResponseGroupNum"))
    F = xg + lw + 6
    for lab, fld, y in (("labelCurrentGroupNum", "textCurrentGroupNum", yB), ("labelResponseGroupNum", "textResponseGroupNum", yB + P)):
        h.set(lab, xg, y, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F, y, 100, H)
    xi = F + 100 + 16
    h.set("butImport", xi, yB, 81, H)
    h.set("labelImport", xi, yB + P, h.lw("labelImport"), H)
    h.text_align("labelImport", "MiddleLeft")
    xb = max(xi + 81, xi + h.lw("labelImport")) + 16
    h.set("gridBen", xb, yB, R - xb, yb - 8 - yB)
    h.set("textNote", 8, yb - 8 - 40, 355, 40)
    h.set("labelNote", 8, yb - 8 - 40 - 2 - 17, h.lw("labelNote"), 17)
    h.text_align("labelNote", "BottomLeft")
    h.set("butDelete", 8, yb, 81, 24)
    h.set("butSave", R - 75, yb, 75, 24)
