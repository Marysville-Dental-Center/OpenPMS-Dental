"""FormRpPaySheet (Daily Payments) on the grid (UI-12 slice 1, layout script of grid_hand.py). The planner placed the Group By group under the
calendars' designer height (155 px), but a month calendar takes its own height when its handle is made (about 175 px at the program's font),
so the group stood on the calendars' Today line; the script lays the window out in two bands: the two calendars beside the Providers and
Clinics lists (each with its caption and its All box over it), then the Group By group with the three options under it beside the three
payment type lists (each with its All box over it), the lists of both bands on one set of columns ending at the content's right edge; OK at
the bottom right. The window keeps its size (844 x 507)."""


def layout(h):
    H = h.H
    CH = h.CH
    root = h.box(".")
    WW, HW = root.node.w, root.node.h
    R = WW - 8
    CAL_W, CAL_H = 178, 176
    h.set("date1", 8, 8, CAL_W, 155)
    h.set("date2", 8 + CAL_W + 16, 8, CAL_W, 155)
    # The lists' columns: the second band's three lists from 16 px after the Group By group to the content edge, 8 px apart; the first band's
    # two lists on the second band's last two columns.
    xg, wg = 8, 186
    x0 = xg + wg + 16
    wl = (R - x0 - 16) // 3
    xs = [x0, x0 + wl + 8, x0 + 2 * (wl + 8)]
    ws = [wl, wl, R - xs[2]]
    # First band: the captions, the All boxes 2 px under them, the lists 2 px under the boxes, down to 8 px over the second band.
    y2 = 8 + CAL_H + 8
    for cap, chk, lst, i in (("label1", "checkAllProv", "listProv", 1), ("labelClin", "checkAllClin", "listClin", 2)):
        h.set(cap, xs[i], 8, ws[i], 18)
        h.check_left(chk)
        h.set(chk, xs[i], 8 + 18 + 2, h.cw(chk), CH)
        h.set(lst, xs[i], 8 + 18 + 2 + CH + 2, ws[i], y2 - 8 - (8 + 18 + 2 + CH + 2))
    h.check_left("checkUnearned")
    h.set("checkUnearned", xs[1] + h.cw("checkAllProv") + 16, 8 + 18 + 2, h.cw("checkUnearned"), CH)
    # Second band: Group By with its radio buttons and the provider option, the three options under it at the content edge.
    gb = h.box("groupBox1", xg, y2, wg)
    gb.checks(("radioCheck", "radioPatient"), gb.left)
    h.check_left("checkShowProvSeparate")
    gb.row(("checkShowProvSeparate", gb.left, wg - 16, 36))
    gb.end()
    y = y2 + h.n("groupBox1").nh + 8
    for name, hh in (("checkReportDisplayUnearnedTP", 36), ("checkShowOnlinePatientPaymentsSeparately", 36), ("checkShowMerchantFees", CH)):
        h.check_left(name)
        h.set(name, xg, y, wg, hh)
        y += hh + 4
    yb = HW - 8 - 24
    for chk, lst, i in (("checkInsuranceTypes", "listInsuranceTypes", 0), ("checkPatientTypes", "listPatientTypes", 1),
                        ("checkAllClaimPayGroups", "listClaimPayGroups", 2)):
        h.check_left(chk)
        h.set(chk, xs[i], y2, min(ws[i], h.cw(chk)), CH)
        h.set(lst, xs[i], y2 + CH + 2, ws[i], yb - 8 - (y2 + CH + 2))
    h.set("butOK", R - 75, yb, 75, 24)
    root.end(w=WW, h=HW)
