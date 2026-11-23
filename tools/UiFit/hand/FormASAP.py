"""FormASAP (ASAP List) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). One touch on the planner's layout:
the filter row's combo boxes at the row's top, with the Code Range filter's box (the planner centred them on the filter's 37 px, its box and
its example line under it, so they stood 7 px under their labels and the Code Range box); the Unscheduled Recalls page's date fields on
the window's field column with the Appointments page's status box and the Provider box."""


def layout(h):
    y = h.n("codeRangeFilter").y
    for name in ("comboProv", "comboClinic", "comboSite"):
        h.set(name, None, y)
    # One field column for the window and its pages: the Appointments page's status box (it cannot move left: its label is the widest), the
    # Provider box (its right edge kept, so the Clinic picker keeps its place) and the Unscheduled Recalls page's date fields.
    sc, tc = h.n("splitContainer"), h.n("tabControl")
    xw = sc.x + tc.x + h.n("tabPageAppts").x + h.n("comboAptStatus").x
    p = h.n("comboProv")
    h.set("comboProv", xw, None, p.x + p.w - xw)
    l4 = h.lw("label4")
    h.set("label4", xw - 6 - l4, y, l4, h.H)
    h.text_align("label4", "MiddleRight")
    pg = h.n("tabPageRecalls")
    # (a split container's panel stands at its 0,0 whatever the tree says)
    dx = xw - (sc.x + tc.x + pg.x + h.n("textDateStart").x)
    for name in ("textDateStart", "textDateEnd", "label1", "label2", "checkGroupFamilies"):
        h.set(name, h.n(name).x + dx, None)
    # Fix round 8: the filter row as label + 6 px + box, one gap between filters (16-17 px from a box's end to the next label's text), the
    # three boxes one width: from the Provider box on the field column to the Code Range label, which keeps its place (its filter ends on the
    # row's right edge as on main). The clinic picker draws its own label in its measured label area. Labels are right-aligned: a label's text
    # starts its width less the text's measured width into it.
    H = h.H
    lc, cr = h.n("labelCodeRange"), h.n("codeRangeFilter")
    wlc = h.lw("labelCodeRange")
    xlc = cr.nx - 6 - wlc
    h.set("labelCodeRange", xlc, y, wlc, H)
    h.text_align("labelCodeRange", "MiddleRight")
    ink_lc = xlc + wlc - lc.text_w
    cwc = h.clinic_w("comboClinic")
    ls = h.lw("labelSite")
    # the room the three boxes and the three gaps share
    room = ink_lc - xw - cwc - (h.n("labelSite").text_w + 6)
    B = (room - 3 * 16) // 3
    gaps = room - 3 * B
    g1, g2 = gaps // 3, gaps // 3
    g3 = gaps - g1 - g2
    h.set("comboProv", xw, y, B, H)
    xc = xw + B + g1
    h.set("comboClinic", xc, y, cwc + B, H)
    xs_ink = xc + cwc + B + g2
    xsl = xs_ink - (ls - h.n("labelSite").text_w)
    h.set("labelSite", xsl, y, ls, H)
    h.text_align("labelSite", "MiddleRight")
    h.set("comboSite", xsl + ls + 6, y, B, H)
