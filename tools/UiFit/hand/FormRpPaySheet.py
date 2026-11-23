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
    # Fix round 9: one set of columns 8 px apart. A month calendar takes its own size when its handle is made (190 x 176 at the program's
    # font: the designer's 178 x 155 drew 12 px wider, so the calendars stood 4 px apart and the lists under them 8 px off): the left column
    # is the calendar's width (the Group By group and the three options under it end on its right edge), the second calendar and the first
    # list 8 px after it, the lists 8 px apart to the content edge.
    CAL_W, CAL_H = 190, 176
    h.set("date1", 8, 8, CAL_W, 155)
    h.set("date2", 8 + CAL_W + 8, 8, CAL_W, 155)
    # The lists' columns: the second band's three lists from 8 px after the Group By group (the second calendar's x) to the content edge,
    # 8 px apart; the first band's two lists on the second band's last two columns.
    xg, wg = 8, CAL_W
    x0 = xg + wg + 8
    room = R - x0 - 16
    ws = [room // 3, room // 3 + (1 if room % 3 > 1 else 0), room // 3 + (1 if room % 3 > 0 else 0)]
    xs = [x0, x0 + ws[0] + 8, x0 + ws[0] + ws[1] + 16]
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
