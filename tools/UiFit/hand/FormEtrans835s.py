"""FormEtrans835s (ERAs) on the grid (UI-12 slice 2, mode hand: the planner put Finalized Only over the status list and cut the Amount
Range title). As main arranges it, on the grid, three filter columns 16 px apart on the 27 px pitch from 8: the date range (its From box at
the content edge + 65), the Amount Range group under it (as wide as its title, Min and Max each 6 px after its label) with Control ID 16 px
after the group on the third row; Carrier, Check# or EFT Trace# and the clinic picker on one field column; the status list with its label,
and under it Automatable Carriers Only, which shows with the list, on the bounds of Finalized Only, which shows without it (Load switches
them by the EraShowStatusAndClinic preference). Refresh at the right content edge on the first row, 16 px after the status list (the window 41 px wider); the grid 8 px under the filters to the
content's right edge down to 8 px over the bottom bar; Auto Processed Report at the bottom left."""


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    R = root.w - 8
    y0, y1, y2 = 8, 8 + P, 8 + 2 * P
    dr = h.n("dateRangePicker")
    h.set("dateRangePicker", 8, y0, None, None)
    grp = h.n("groupBox2")
    top = (grp.title_bottom or 18) + 4
    x = 8
    for lab, fld in (("labelDaysOldMin", "textRangeMin"), ("labelDaysOldMax", "textRangeMax")):
        lw = h.lw(lab)
        if fld == "textRangeMin":
            lw = 65 - 8 - 6  # Min's box on the date range's From box (65 px into the range, the group at the content edge)
        h.set(lab, x, top, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, x + lw + 6, top, 60, H)
        x += lw + 6 + 60 + 16
    gw = max(x - 16 + 8, grp.text_w + 16)
    gh = top + H + 8
    h.set("groupBox2", 8, y1, gw, gh)
    h.watch("groupBox2")
    xc = 8 + gw + 16
    lwc = h.lw("labelControlId")
    h.set("labelControlId", xc, y2, lwc, H)
    h.text_align("labelControlId", "MiddleRight")
    h.set("textControlId", xc + lwc + 6, y2, 111, H)
    x2 = max(8 + dr.nw, xc + lwc + 6 + 111) + 16
    lw2 = max(h.lw("labelCarrier"), h.lw("labelCheckTrace"))
    F2 = x2 + lw2 + 6
    FW = 161
    for lab, fld, y in (("labelCarrier", "textCarrier", y0), ("labelCheckTrace", "textCheckTrace", y1)):
        h.set(lab, x2, y, lw2, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F2, y, FW, H)
    cw = h.clinic_w("comboClinics")
    h.set("comboClinics", F2 - cw, y2, cw + FW, H)
    x3 = F2 + FW + 16
    lws = h.lw("labelStatus")
    h.set("labelStatus", x3, y0, lws, H)
    h.text_align("labelStatus", "MiddleRight")
    h.set("listStatus", x3 + lws + 6, y0, 120, 56)
    yc = y0 + 56 + 4
    wc = max(h.cw("checkAutomatableCarriersOnly"), h.cw("checkShowFinalizedOnly"))
    for name in ("checkAutomatableCarriersOnly", "checkShowFinalizedOnly"):
        h.check_left(name)
        h.set(name, x3, yc, wc, CH)
    # Refresh at the right content edge on the first row, 16 px after the status list: the window grows by what that needs.
    W = max(root.w, x3 + lws + 6 + 120 + 16 + 75 + 8)
    R = W - 8
    h.set(".", None, None, W, None)
    h.set("butRefresh", R - 75, y0, 75, 24)
    yg = max(y1 + gh, yc + CH, y2 + H) + 8
    yb = root.h - 8 - 24
    h.set("gridMain", 8, yg, R - 8, yb - 8 - yg)
    h.set("butAutoProcessedEras", 8, yb, None, 24)
