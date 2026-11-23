"""FormRpProcedures on the grid (UI-12 slice 2, mode hand: the planner left the Clinics picker's box 9 px off the Providers column and the
filter row spread). The Filters group at the content edge: the date range (its From box 65 px into it) and Providers on the first row, the
clinic picker on the second with its box on the Providers column (one width), the procedure code caption over its box in a third column,
16 px between columns; Refresh 8 px after the group on its first row; the grid 8 px under the group to both content edges down to 8 px over
the bottom bar; Print and Export 6 px apart at the bottom left, the total right-aligned 8 px before Close at the bottom right."""


def layout(h):
    H = h.H
    P = H + 4
    root = h.tree
    R = root.w - 8
    grp = h.n("groupFilter")
    top = (grp.title_bottom or 18) + 4
    y0, y1 = top, top + P
    dr = h.n("dateRangePicker")
    h.set("dateRangePicker", 8, y0 - 2, None, None)  # its boxes 2 px into its bounds: on the row with Providers (review 2026-11-23)
    x2 = 8 + dr.nw + 16
    lw = h.lw("labelProv")
    F2 = x2 + lw + 6
    h.set("labelProv", x2, y0, lw, H)
    h.text_align("labelProv", "MiddleRight")
    h.set("comboProvs", F2, y0, 160, H)
    cw = h.clinic_w("comboClinics")
    h.set("comboClinics", F2 - cw, y1, cw + 160, H)
    x3 = F2 + 160 + 16
    h.set("labelCode", x3, y0, h.lw("labelCode"), H)
    h.text_align("labelCode", "MiddleLeft")
    h.set("textCode", x3, y1, 100, H)
    RW = 75
    gw = R - RW - 8 - 8
    gh = y1 + H + 8
    h.set("groupFilter", 8, 8, gw, gh)
    h.watch("groupFilter")
    h.set("butRefresh", R - RW, 8 + y0, RW, 24)
    yb = root.h - 8 - 24
    yg = 8 + gh + 8
    h.set("gridMain", 8, yg, R - 8, yb - 8 - yg)
    h.set("butPrint", 8, yb, 79, 24)
    h.set("butExport", 8 + 79 + 6, yb, 79, 24)
    h.set("butClose", R - RW, yb, RW, 24)
    h.set("labelTotal", R - RW - 8 - 400, yb, 400, 24)
    h.text_align("labelTotal", "MiddleRight")
