"""FormAdvertisingPatientList (the advertising patient list) on the grid (UI-12 slice 2 continuation, mode hand: the filter panel stacked
with the user query panel, both switched by User Query: FormAdvertisingPatientList.cs). User Query at the content edge; under it the two
panels on one bounds from the window's left edge, their content at the padding: in the filter panel the clinic picker, then the Patient Age
group (From and To on one field column) and the Appointments group (its three check boxes one per row at the row pitch, the two date
pickers' boxes 6 px after the wider text of their two check boxes) side by side, one top and one height, then the three lists one width, 8 px apart,
their captions 2 px over them, to the Appointments group's right edge; in the user query panel its note and Favorites on one row, the query
box under them to the panel's content edges. Refresh 8 px after the panels on the lists' bottom; the patient grid 8 px under the panels to
both content edges down to 8 px over the bottom bar; Set All, Set Selected, Clear Selected and Clear All 6 px apart at the bottom left, the
selected count (its caption and number on one line) and OK at the bottom right."""


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    W, HH = root.w, root.h
    R = W - 8
    yb = HH - 8 - 24
    h.check_left("checkUserQuery")
    h.set("checkUserQuery", 8, 8, h.cw("checkUserQuery"), CH)
    yp = 8 + CH  # the panels' top: their content starts 8 px under User Query
    # The filter panel.
    cp = h.n("comboClinics")
    h.set("comboClinics", 8, 8, h.clinic_w("comboClinics") + 160, H)
    yg = 8 + H + 8
    # Patient Age.
    top = (h.n("groupBox1").title_bottom or 18) + 4
    lwa = max(h.lw("label3"), h.lw("label4"))
    for i, (lab, fld) in enumerate((("label3", "textPatientAgeFrom"), ("label4", "textPatientAgeTo"))):
        h.set(lab, 8, top + i * P, lwa, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, 8 + lwa + 6, top + i * P, 40, H)
    gw1 = 8 + lwa + 6 + 40 + 8
    # Appointments.
    checks = ("checkHiddenFutureAppt", "checkHidePatientsNotSeenSince", "checkHidePatientsSeenSince")
    cwm = max(h.cw(c) for c in checks[1:])
    bx = 8 + cwm + 6  # the date pickers' boxes, 6 px after the text of the two check boxes they belong to
    dp = h.n("datePickerPatientsNotSeenSince")
    px = bx - 63
    for i, c in enumerate(checks):
        y = top + i * P
        h.check_left(c)
        h.set(c, 8, y + (H - CH) // 2, h.cw(c), CH)
    h.set("datePickerPatientsNotSeenSince", px, top + P, dp.nw, dp.nh)
    h.set("datePickerPatientsSeenSince", px, top + 2 * P, dp.nw, dp.nh)
    gw2 = max(px + dp.nw, 8 + h.cw(checks[0])) + 8
    gh = top + 2 * P + H + 8
    h.set("groupBox1", 8, yg, gw1, gh)
    xa = 8 + gw1 + 8
    h.set("groupAppt", xa, yg, gw2, gh)
    h.watch("groupBox1", "groupAppt")
    E = xa + gw2  # the filters' right edge
    # The three lists with their captions.
    yc = yg + gh + 8
    lw = (E - 8 - 2 * 8) // 3
    lh = h.n("listBoxContactMethods").nh
    x = 8
    for cap, lst in (("labelContact", "listBoxContactMethods"), ("labelPatStatus", "listBoxPatientStatuses"),
            ("labelPatBillingType", "listBoxPatientBillingType")):
        h.set(cap, x, yc, lw, 17)
        h.text_align(cap, "BottomLeft")
        h.set(lst, x, yc + 17 + 2, lw, lh)
        x += lw + 8
    ylb = yc + 17 + 2 + lh  # the lists' bottom
    pw = E + 8
    ph = ylb + 8
    h.set("panelFilterControls", 0, yp, pw, ph)
    h.watch("panelFilterControls")
    # The user query panel on the same bounds.
    fb = h.n("butFavorite")
    h.set("labelUserQuery", 8, 8, h.lw("labelUserQuery"), H)
    h.text_align("labelUserQuery", "MiddleLeft")
    h.set("butFavorite", pw - 8 - fb.nw, 8, fb.nw, 24)
    h.set("textUserQuery", 8, 8 + 24 + 8, pw - 16, ph - 8 - (8 + 24 + 8))
    h.set("panelUserQuery", 0, yp, pw, ph)
    h.watch("panelUserQuery")
    # Refresh 8 px after the panels, on the lists' bottom.
    rb = h.n("butRefreshPatientFilters")
    h.set("butRefreshPatientFilters", pw, yp + ylb - 24, rb.nw, 24)
    yg2 = yp + ph
    h.set("gridMain", 8, yg2, R - 8, yb - 8 - yg2)
    x = 8
    for name in ("butSelectAll", "butSetSelected", "butClearSelected", "butClearAll"):
        b = h.n(name)
        h.set(name, x, yb, b.nw, 24)
        x += b.nw + 6
    ok = h.n("butCommitList")
    h.set("butCommitList", R - ok.nw, yb, ok.nw, 24)
    wn = h.lw("labelNumberPatientsSelected")
    xn = R - ok.nw - 16 - wn
    h.set("labelNumberPatientsSelected", xn, yb, wn, 24)
    h.text_align("labelNumberPatientsSelected", "MiddleLeft")
    wl = h.lw("labelPatsSelected")
    h.set("labelPatsSelected", xn - 6 - wl, yb, wl, 24)
    h.text_align("labelPatsSelected", "MiddleRight")
