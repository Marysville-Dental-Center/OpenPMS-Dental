"""FormRpPPOwriteoffs on the grid (UI-12 slice 4, layout script on the planner's tree). A month calendar takes 190 x 176 px when its handle
is made (slice 2, FormRpBrokenAppointments); the planner put the Individual Claims radio button on the first calendar's Today line. The two
calendars at the content top with TO between them (6 px each side, centred on them); 8 px under their drawn height the left column (the
two radio buttons as a list, the carrier caption 8 px under them, its box 2 px under it) and the Show Insurance Write-offs group on the
second calendar's x to the right content edge; OK at the bottom right as the planner placed it."""


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
    y = yc + CAL_H + 8
    for r in ("radioIndividual", "radioGroup"):
        h.check_left(r)
        h.set(r, 8, y, h.cw(r), CH)
        y += CH + 4
    y += 4
    h.set("label1", 8, y, h.lw("label1"), 18)
    h.text_align("label1", "BottomLeft")
    h.set("textCarrier", 8, y + 18 + 2, None, H)
    g = h.n("groupBox3")
    h.set("groupBox3", x2, yc + CAL_H + 8, R - x2, None)
