"""FormRpInsOverpaid on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). A month calendar takes 190 x 176 px when
its handle is made (slice 2, FormRpBrokenAppointments): the planner put the Clinics caption under the From calendar's drawn bottom, where
the calendar covered it, and the All box 3 px left of its list. The Clinics column (the caption, its All box 2 px under it, the list 2 px
under the box) 8 px under the calendar's drawn bottom on the list's x; the two radio buttons and the note beside it move down with it, and
the window grows by as much."""

CAL_H = 176


def layout(h):
    cal = h.n("dateStart")
    lst = h.n("listClin")
    cap = h.n("labelClin")
    chk = h.n("checkAllClin")
    y = cal.ny + CAL_H + 8
    dy = y + cap.nh + 2 + chk.nh + 2 - lst.ny
    x = lst.nx
    h.set("labelClin", x, y, None, None)
    h.check_left("checkAllClin")
    h.set("checkAllClin", x, y + cap.nh + 2, None, None)
    h.set("listClin", x, lst.ny + dy, None, None)
    for name in ("radioGroupByProc", "radioGroupByPat", "labelDisclaimer", "butOK"):
        h.set(name, None, h.n(name).ny + dy, None, None)
    h.set(".", None, None, None, h.tree.h + dy)
