"""FormChooseDatabase on the grid (UI-12 slice 3, mode hand: the planner left the SSL path and the middle tier group 20 to 32 px in from
the edge the connection group starts at). Main's arrangement on the grid's numbers: the Connection Settings group at the padding to the right
edge (its four captioned fields, the server note on its two lines, and Do not show this window under them); the SSL certificate path as a
captioned box to the right edge; Connect to Middle Tier instead; two columns one width, 8 px apart: the middle tier group (its note, three
captioned fields, the two check boxes) and the connection string box under its caption with Dynamic Mode (two lines) under it and the
database type list (hidden by the program) under that; OK at the bottom right."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    gw = R - PAD
    g = h.n("groupDirect")
    top = (g.title_bottom or 18) + 4
    fw = h.n("comboComputerName").nw
    cwid = gw - 2 * PAD
    y = top
    for cap, fld in (("label1", "comboComputerName"), ("label4", "comboDatabase"), ("label3", "textUser"), ("label2", "textPassword")):
        h.captioned(cap, fld, PAD, y, fw, cap_w=cwid)
        y = h.n(fld).ny + H + GAP
    h.check_left("checkNoShow")
    h.set("checkNoShow", PAD, y, h.cw("checkNoShow"), CH)
    gh = y + CH + PAD
    h.set("groupDirect", PAD, PAD, gw, gh)
    y = PAD + gh + GAP
    y = h.captioned("label5", "textSslCaPem", PAD, y, gw) + GAP
    h.check_left("checkConnectServer")
    h.set("checkConnectServer", PAD, y, h.cw("checkConnectServer"), CH)
    y += CH + GAP
    colw = (gw - GAP) // 2
    rx = R - colw
    # The middle tier group.
    gs = h.n("groupServer")
    ts = (gs.title_bottom or 18) + 4
    iw = colw - 2 * PAD
    nh = h.note_h("label6", iw)
    h.set("label6", PAD, ts, iw, nh)
    h.text_align("label6", "TopLeft")
    yy = ts + nh + 4
    for cap, fld in (("label9", "textURI"), ("label11", "textUser2"), ("label10", "textPassword2")):
        yy = h.captioned(cap, fld, PAD, yy, iw) + GAP
    for c in ("checkUsingEcw", "checkBoxAutomaticLogin"):
        h.check_left(c)
        h.set(c, PAD, yy, h.cw(c), CH)
        yy += CH + 4
    sh = yy - 4 + PAD
    h.set("groupServer", PAD, y, colw, sh)
    # The right column.
    ry = h.captioned("label8", "textConnectionString", rx, y, colw, h.n("textConnectionString").nh) + 4
    h.text_align("label8", "BottomLeft")
    dh = h.note_h("checkDynamicMode", colw - 16)
    h.check_left("checkDynamicMode")
    h.set("checkDynamicMode", rx, ry, colw, dh)
    ry += dh + GAP
    ry = h.captioned("label7", "listType", rx, ry, h.n("listType").nw, h.n("listType").nh, cap_w=colw)
    bottom = max(y + sh, ry)
    HH2 = max(HH, bottom + GAP + 24 + PAD)
    h.set("butOK", R - h.n("butOK").nw, HH2 - PAD - 24, None, 24)
    h.set(".", None, None, W, HH2)
    h.watch(".", "groupDirect", "groupServer")
