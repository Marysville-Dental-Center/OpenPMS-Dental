"""FormRpAging (Aging of Accounts Receivable) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The planner
kept most of the window as drawn (two columns of groups at 12 and 229/235, the lists 15 and 6 px apart); the script lays it out on one
grid: the As Of Date row, two columns of groups of one width with one height per row, the options as a list under the left column, the three
lists with their captions and All boxes on three columns 8 px apart ending at the content edge and down to the groups' bottom, the bottom
bar. The hidden notes stand under the groups and under the lists."""


def layout(h):
    H = h.H
    CH = h.CH
    root = h.tree
    R = root.w - 8
    h.set("textDate", 8, 8, 104, H)
    h.set("label1", 8 + 104 + 6, 8, h.lw("label1"), H)
    h.text_align("label1", "MiddleLeft")
    # The left column as wide as its widest option, the right column as wide as its widest check box + the group's padding; the lists
    # take the rest in three equal columns.
    wg = max(h.cw(n) for n in ("checkAgeWriteoffs", "checkHasDateLastPay", "checkBoxExcludeIncomeTransfers"))
    wB = max(h.cw(n) for n in ("checkIncludeInsNoBal", "checkBadAddress", "checkIncludeNeg")) + 16
    xa, xB = 8, 8 + wg + 8
    xb = xB + wB
    WL = (R - (xb + 16) - 16) // 3
    xs = [R - 3 * WL - 16, R - 2 * WL - 8, R - WL]
    y1 = 8 + H + 8
    rows = ((("groupAgeOfAccount", xa, wg), ("groupExcludePats", xB, wB)), (("groupGroupBy", xa, wg), ("groupIncludePats", xB, wB)))
    y = y1
    for row in rows:
        hh = max(h.n(n).h for n, _, _ in row)
        for name, x, w in row:
            h.set(name, x, y, w, hh)
        y += hh + 8
    y3 = y
    yc = y3
    for name in ("checkAgeWriteoffs", "checkHasDateLastPay", "checkBoxExcludeIncomeTransfers", "checkAgePatPayPlanPayments"):
        h.check_left(name)
        h.set(name, xa, yc, min(wg, h.cw(name)), CH)
        yc += CH + 4
    go = h.n("groupOnlyShow")
    h.set("groupOnlyShow", xB, y3, wB, go.h)
    bottom = max(y3 + go.h, yc - 4)
    h.set("labelAgingExcludeTransfers", xB, y3 + go.h + 8, wB, 40)
    for cap, chk, lst, i in (("label2", "checkBillTypesAll", "listBillType", 0), ("label3", "checkProvAll", "listProv", 1),
                             ("labelClin", "checkAllClin", "listClin", 2)):
        h.set(cap, xs[i], 8, WL, 18)
        h.check_left(chk)
        h.set(chk, xs[i], 8 + 18 + 2, h.cw(chk), CH)
        yl = 8 + 18 + 2 + CH + 2
        h.set(lst, xs[i], yl, WL, bottom - yl)
    h.set("labelFutureTrans", xs[1], bottom + 8, None, 18)
    yb = root.h - 8 - 24
    h.set("butQuery", 8, yb, 91, 24)
    h.set("butOK", R - 75, yb, 75, 24)
