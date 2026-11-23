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
    # Fix round 9: the row placed from the text as the controls draw it, so its gaps are equal in ink (they were 20 to 23 px, label to box 8
    # to 11), and the window on the 8 px content edges. A right-aligned label (and the clinic picker's label, drawn as one since this round)
    # draws its text from its right side less its measured width plus 6 px to 3 px before its right side: label to box is 9 px of ink to box
    # on every filter. The three boxes one width B, one gap G (16 px, the remainder of the room as 1 px more on a gap) from a box's end to the
    # next label's text; the Provider box on the window's field column, the Code Range filter ending on the content edge (Refresh and Print
    # on it under it), the split container from the left content edge to 8 px before them.
    H = h.H
    WW = h.tree.nw
    R = WW - 8
    rf, pr = h.n("butRefresh"), h.n("butPrint")
    for name in ("butRefresh", "butPrint"):
        h.set(name, R - h.n(name).nw, None)
    h.set("splitContainer", 8, None, R - rf.nw - 8 - 8, None)
    lc, cr, ls = h.n("labelCodeRange"), h.n("codeRangeFilter"), h.n("labelSite")
    h.set("codeRangeFilter", R - cr.nw, y)
    xcr = R - cr.nw
    wlc = h.lw("labelCodeRange")
    h.set("labelCodeRange", xcr - 6 - wlc, y, wlc, H)
    h.text_align("labelCodeRange", "MiddleRight")
    cwc = h.clinic_w("comboClinic")
    tws, twc = ls.text_w, lc.text_w
    # the field column again, from the split container's new x
    xw = sc.nx + tc.x + h.n("tabPageAppts").x + h.n("comboAptStatus").x
    # ink of the Code Range label starts at xcr - 6 - twc + 6; the boxes and gaps fill the room from the Provider box to it
    room = (xcr - twc) - xw - (cwc - 6) - (tws - 6) - 6
    G = 16
    B = (room - 3 * G) // 3
    rem = room - 3 * B - 3 * G
    gaps = [G + (1 if rem > 0 else 0), G + (1 if rem > 1 else 0), G]
    h.set("comboProv", xw, y, B, H)
    l4 = h.lw("label4")
    h.set("label4", xw - 6 - l4, y, l4, H)
    h.text_align("label4", "MiddleRight")
    # the picker: its label's text starts 6 px into its bounds (its label area is its measured text + 6 px)
    xc = xw + B + gaps[0] - 6
    h.set("comboClinic", xc, y, cwc + B, H)
    # Site: its text starts gaps[1] after the picker's box
    rs = xc + cwc + B + gaps[1] + tws - 6
    lsw = h.lw("labelSite")
    h.set("labelSite", rs - lsw, y, lsw, H)
    h.text_align("labelSite", "MiddleRight")
    h.set("comboSite", rs + 6, y, B, H)
    # dx for the Recalls page's date fields: they follow the field column
    dx2 = xw - (sc.nx + tc.x + pg.x + h.n("textDateStart").nx)
    for name in ("textDateStart", "textDateEnd", "label1", "label2", "checkGroupFamilies"):
        h.set(name, h.n(name).nx + dx2, None)
