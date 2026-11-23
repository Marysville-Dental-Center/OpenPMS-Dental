"""FormRpActivePatients on the grid (UI-12 slice 4, layout script on the planner's tree). A month calendar takes 190 x 176 px when its
handle is made (slice 2, FormRpBrokenAppointments); the planner put the four list captions on the calendars' Today line and the All boxes
on one row away from their lists. The description at the content top across the window; the two calendars 8 px under it with TO between
them (6 px each side, centred on them); 8 px under their drawn height four columns of one width, 8 px apart, from the content edge to the
right content edge (Billing Types, Patient Status, Providers, Clinics: the caption, its All box 2 px under it, the list 2 px under the box
down to 8 px over the bottom bar); OK at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    root = h.tree
    R = root.w - 8
    CAL_W, CAL_H = 190, 176
    h.set("label3", 8, 8, R - 8, 18)
    yc = 8 + 18 + 8
    h.set("dateStart", 8, yc, CAL_W, 155)
    tw = h.lw("labelTO")
    h.set("labelTO", 8 + CAL_W + 6, yc + (CAL_H - H) // 2, tw, H)
    h.text_align("labelTO", "MiddleCenter")
    h.set("dateEnd", 8 + CAL_W + 6 + tw + 6, yc, CAL_W, 155)
    y2 = yc + CAL_H + 8
    yb = root.h - 8 - 24
    cols = (("label2", "checkAllBilling", "listBillingTypes"), ("label4", "checkAllPatStatus", "listPatientStatuses"),
            ("label1", "checkAllProv", "listProv"), ("labelClin", "checkAllClin", "listClin"))
    w = (R - 8 - 8 * 3) // 4
    yl = y2 + 18 + 2 + CH + 2
    for i, (cap, chk, lst) in enumerate(cols):
        x = 8 + i * (w + 8)
        ww = R - x if i == 3 else w
        h.set(cap, x, y2, ww, 18)
        h.text_align(cap, "BottomLeft")
        h.check_left(chk)
        h.set(chk, x, y2 + 18 + 2, h.cw(chk), CH)
        h.set(lst, x, yl, ww, yb - 8 - yl)
    h.set("butOK", R - 75, yb, 75, 24)
