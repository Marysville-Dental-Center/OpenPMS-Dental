"""FormRpCustomAging (Custom Aging) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The planner kept the
window as drawn (the grid at 9, the right column's lists at 881-884, a 3-line radio button in the 157 px Age Write-offs group); the script
lays it out in three columns: the aging grid and its totals row from the content edge down to the bottom bar, the options column (Refresh,
As Of Date, the five groups 8 px apart, 20 px wider so the last write-off option takes its two lines, Print on the bottom bar), the lists
column (each caption, All box and list, then the four check boxes as one list) ending at the content edge."""


def layout(h):
    H = h.H
    CH = h.CH
    root = h.tree
    R = root.w - 8
    WR = 165
    XR = R - WR
    WM = 177
    XM = XR - 8 - WM
    yb = root.h - 8 - 24
    # The options column.
    h.set("butRefresh", XM, 8, 75, 24)
    h.set("label1", XM, 8 + 24 + 8, WM, 18)
    h.set("textDate", XM, 8 + 24 + 8 + 18 + 2, WM, H)
    y = 8 + 24 + 8 + 18 + 2 + H + 8
    for gname in ("groupBox1", "groupBox2", "groupBox3", "groupBox4", "groupGroupBy"):
        g = h.n(gname)
        hh = g.h
        if gname == "groupBox4":
            r = h.n("radioWriteoffClaimDate")
            h.set("radioWriteoffClaimDate", None, None, WM - 16, 36)
            hh = r.y + 36 + 8
        h.set(gname, XM, y, WM, hh)
        y += hh + 8
    h.set("butPrint", XM, yb, 75, 24)
    # The lists column.
    y = 8
    for cap, chk, lst in (("label3", "checkAllBillType", "listBoxBillTypes"), ("label4", "checkAllProv", "listBoxProvs"),
                          ("labelClinic", "checkAllClin", "listBoxClins")):
        h.set(cap, XR, y, WR, 18)
        h.check_left(chk)
        h.set(chk, XR, y + 18 + 2, h.cw(chk), CH)
        h.set(lst, XR, y + 18 + 2 + CH + 2, WR, 147)
        y += 18 + 2 + CH + 2 + 147 + 8
    for name in ("checkAgeCredits", "checkExcludeInactive", "checkExcludeArchive", "checkExcludeBadAddresses"):
        h.check_left(name)
        h.set(name, XR, y, h.cw(name), CH)
        y += CH + 4
    # The grid and its totals row down to the bottom bar's bottom.
    WG = XM - 8 - 8
    yt = yb + 24 - 20
    h.set("gridTotals", 8, yt, WG, 20)
    h.set("gridMain", 8, 8, WG, yt - 4 - 8)
