"""FormRpBrokenAppointments on the grid (UI-12 slice 2, mode hand). A month calendar takes 190 x 176 px when its handle is made (the designer
says 178 x 155; slice 1, FormRpPaySheet), so main's Providers and Clinics captions stood on the calendars' Today line, covered, and the TO
label behind the first calendar; the planner kept them there. The description at the content edge across the window; the two calendars
8 px under it with TO between them (6 px each side, centred on them); under them (8 px under their drawn height) the Providers and Clinics
columns (caption, All 2 px under it, the list 2 px under the box down to 8 px over the bottom bar), 8 px apart; the three report modes a
radio list 16 px after them on the lists' top, the options list (shown for the procedure mode) 16 px after the radio list to the content
edge; OK at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    root = h.tree
    R = root.w - 8
    CAL_W, CAL_H = 190, 176
    h.set("labelDescr", 8, 8, R - 8, 18)
    yc = 8 + 18 + 8
    h.set("dateStart", 8, yc, CAL_W, 155)
    tw = h.lw("labelTO")
    h.set("labelTO", 8 + CAL_W + 6, yc + (CAL_H - H) // 2, tw, H)
    h.text_align("labelTO", "MiddleCenter")
    h.set("dateEnd", 8 + CAL_W + 6 + tw + 6, yc, CAL_W, 155)
    y2 = yc + CAL_H + 8
    yb = root.h - 8 - 24
    LW = 120
    yl = y2 + 18 + 2 + CH + 2
    for (cap, chk, lst), x in zip((("labelProviders", "checkAllProvs", "listProvs"), ("labelClinics", "checkAllClinics", "listClinics")),
            (8, 8 + LW + 8)):
        h.set(cap, x, y2, LW, 18)
        h.text_align(cap, "BottomLeft")
        h.check_left(chk)
        h.set(chk, x, y2 + 18 + 2, h.cw(chk), CH)
        h.set(lst, x, yl, LW, yb - 8 - yl)
    xr = 8 + 2 * LW + 8 + 16
    y = yl
    wr = 0
    for r in ("radioProcs", "radioAdj", "radioAptStatus"):
        h.check_left(r)
        h.set(r, xr, y, h.cw(r), CH)
        wr = max(wr, h.cw(r))
        y += CH + 4
    xo = xr + wr + 16
    h.set("listOptions", xo, yl, R - xo, yb - 8 - yl)
    h.set("butOK", R - 75, yb, 75, 24)
