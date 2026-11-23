"""FormRpProdGoal on the grid (UI-12 slice 3, mode hand: the planner left the provider column at 34). Main's arrangement on the grid's
numbers, as slice 2's report windows: the Providers and Clinics columns from the padding, one width, 8 px apart (the caption, the All box 2 px
under it, the list 4 px under the box), Show Clinic Breakdown under the clinic list, the lists ending where the right column ends; the right
column 16 px after them to the right edge: Today's Date with its box, the Date Range group (the month buttons and the From and To boxes on one
column) and the Show Insurance Write-offs group (the three choices a list, the claim date choice on two lines, the manual note under them);
OK at the bottom right."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    lw_ = h.n("listProv").nw
    cols = (("label1", "checkAllProv", "listProv", PAD), ("labelClin", "checkAllClin", "listClin", PAD + lw_ + GAP))
    RC = PAD + 2 * lw_ + GAP + 16
    gw = R - RC
    # Today's Date.
    l4 = h.lw("label4")
    h.set("label4", RC, PAD, l4, H)
    h.text_align("label4", "MiddleLeft")
    h.set("textToday", RC + l4 + 6, PAD, h.n("textToday").nw, H)
    y = PAD + H + GAP
    # Date Range.
    g = h.n("groupBox2")
    top = (g.title_bottom or 18) + 4
    DF = l4 + 6  # the From and To boxes on Today's Date box's column (window coordinates)
    dl = DF - 6 - PAD
    assert dl >= max(h.lw("label2"), h.lw("label3"))
    h.set("butLeft", DF, top, 45, 26)
    h.set("butThis", DF + 51, top, 101, 26)
    h.set("butRight", DF + 158, top, 45, 26)
    yy = top + 26 + 4
    for lab, fld in (("label2", "textDateFrom"), ("label3", "textDateTo")):
        h.set(lab, PAD, yy, dl, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, DF, yy, 100, H)
        yy += H + 4
    gh = yy - 4 + PAD
    h.set("groupBox2", RC, y, gw, gh)
    y += gh + GAP
    # Show Insurance Write-offs.
    g2 = h.n("groupShowInsWriteoffs")
    t2 = (g2.title_bottom or 18) + 4
    yy = t2
    room = gw - 2 * PAD
    for r in ("radioWriteoffPay", "radioWriteoffProc", "radioWriteoffClaim"):
        h.check_left(r)
        rh = CH if h.cw(r) <= room else h.note_h(r, room - 16)
        h.set(r, PAD, yy, min(h.cw(r), room), rh)
        yy += rh + 4
    nh = h.note_h("label5", room)
    h.set("label5", PAD, yy, room, nh)
    h.text_align("label5", "TopLeft")
    g2h = yy + nh + PAD
    h.set("groupShowInsWriteoffs", RC, y, gw, g2h)
    bottom = y + g2h
    # The two list columns.
    lb = bottom - CH - 4
    for cap, chk, lst, x in cols:
        h.set(cap, x, PAD, lw_, 17)
        h.text_align(cap, "BottomLeft")
        h.check_left(chk)
        h.set(chk, x, PAD + 19, h.cw(chk), CH)
        ly = PAD + 19 + CH + 4
        h.set(lst, x, ly, lw_, lb - ly)
    h.check_left("checkClinicBreakdown")
    h.set("checkClinicBreakdown", cols[1][3], lb + 4, h.cw("checkClinicBreakdown"), CH)
    HH2 = max(HH, bottom + GAP + 24 + PAD)
    h.set("butOK", R - 75, HH2 - PAD - 24, 75, 24)
    h.set(".", None, None, W, HH2)
    h.watch(".", "groupBox2", "groupShowInsWriteoffs")
