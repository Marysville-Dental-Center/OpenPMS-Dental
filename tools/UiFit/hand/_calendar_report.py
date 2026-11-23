"""The calendar report windows' Clinics column (FormRpAdjSheet, FormRpProcSheet; UI-12 slice 4 fix round). A month calendar takes 190 x 176
px when its handle is made (slice 2, FormRpBrokenAppointments): the Clinics caption 8 px under the calendar's drawn bottom with its All box
and list under it (each keeps its distance to the caption), the OK row and the window moving down by as much."""

CAL_W, CAL_H = 190, 176


def clinics_column(h, cal, x, others=()):
    """Places the Clinics column at x under calendar cal; others move down with it. Returns the move."""
    c = h.n(cal)
    y = c.ny + CAL_H + 8
    cap = h.n("labelClin")
    chk = h.n("checkAllClin")
    lst = h.n("listClin")
    dy = y - cap.ny
    h.set("labelClin", x, y, None, None)
    h.check_left("checkAllClin")
    h.set("checkAllClin", x, chk.ny + dy, None, None)
    h.set("listClin", x, lst.ny + dy, None, None)
    for name in others:
        h.set(name, None, h.n(name).ny + dy, None, None)
    h.set("butOK", None, h.n("butOK").ny + dy, None, None)
    h.set(".", None, None, None, h.tree.h + dy)
    return dy
