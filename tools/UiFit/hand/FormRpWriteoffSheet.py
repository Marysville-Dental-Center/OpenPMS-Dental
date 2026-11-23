"""FormRpWriteoffSheet on the grid (UI-12 slice 4, layout script on the planner's tree). A month calendar takes 190 x 176 px when its handle
is made (slice 2, FormRpBrokenAppointments); the planner put the Clinics caption on the second calendar's Today line, its All box under
the calendar and its list 200 px away beside the write-offs group. The two calendars at the content top with TO between them (6 px each
side, centred on them); after them (16 px) the Clinics and Providers columns of one width, 8 px apart, to the right content edge (the
caption, its All box 2 px under it, the list 2 px under the box down to the calendars' drawn bottom); the Show Insurance Write-offs group
8 px under the calendars at the content edge; OK at the bottom right as the planner placed it."""


def layout(h):
    H = h.H
    CH = h.CH
    R = h.tree.w - 8
    CAL_W, CAL_H = 190, 176
    yc = 8
    h.set("date1", 8, yc, CAL_W, 155)
    tw = h.lw("labelTO")
    h.set("labelTO", 8 + CAL_W + 6, yc + (CAL_H - H) // 2, tw, H)
    h.text_align("labelTO", "MiddleCenter")
    x2 = 8 + CAL_W + 6 + tw + 6
    h.set("date2", x2, yc, CAL_W, 155)
    x = x2 + CAL_W + 16
    w = (R - x - 8) // 2
    yl = yc + 18 + 2 + CH + 2
    for i, (cap, chk, lst) in enumerate((("labelClin", "checkAllClin", "listClin"), ("label1", "checkAllProv", "listProv"))):
        ww = R - x if i == 1 else w
        h.set(cap, x, yc, ww, 18)
        h.text_align(cap, "BottomLeft")
        h.check_left(chk)
        h.set(chk, x, yc + 18 + 2, h.cw(chk), CH)
        h.set(lst, x, yl, ww, yc + CAL_H - yl)
        x += w + 8
    h.set("groupBox3", 8, yc + CAL_H + 8, None, None)
