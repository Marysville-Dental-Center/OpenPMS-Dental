"""FormApptSearchAdvanced on the grid (UI-12 slice 3, mode hand: the planner left the Starting after row 4 px low with its am and pm 9 px
right of the before row's). Main's arrangement on the grid's numbers: the results grid from the padding to 8 px before the Filters group,
which stands at the right edge with its rows on one field column (From, To, Starting before and after with am and pm 16 px after the
boxes, Providers with its picker button, Blockout, the clinic picker's box, Appt View shown by the code at the end), the grid and the group
one bottom; the Quick Search group at the bottom left, More and Search at the grid's right edge."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    labels = ("label9", "label10", "label1", "label11", "label2", "labelBlockout", "labelAptViews")
    lw = max(h.lw(n) for n in labels)
    F = PAD + lw + 6
    fw = 130
    g = h.n("groupBox2")
    top = (g.title_bottom or 18) + 4
    y = top

    def lab(name):
        h.set(name, PAD, y, lw, H)
        h.text_align(name, "MiddleRight")

    lab("label9")
    h.set("dateSearchFrom", F, y, fw, H)
    y += H + 4
    lab("label10")
    h.set("dateSearchTo", F, y, fw, H)
    y += H + 4
    tx = F + 44 + 16
    pm_x = h.cw("radioBeforeAM") + 16
    lab("label1")
    h.set("textBefore", F, y, 44, H)
    h.check_left("radioBeforeAM")
    h.set("radioBeforeAM", tx, y + (H - CH) // 2, h.cw("radioBeforeAM"), CH)
    h.check_left("radioBeforePM")
    h.set("radioBeforePM", tx + pm_x, y + (H - CH) // 2, h.cw("radioBeforePM"), CH)
    y += H + 4
    lab("label11")
    h.set("textAfter", F, y, 44, H)
    h.check_left("radioAfterAM")
    h.set("radioAfterAM", 0, 0, h.cw("radioAfterAM"), CH)
    h.check_left("radioAfterPM")
    h.set("radioAfterPM", pm_x, 0, h.cw("radioAfterPM"), CH)
    h.set("panel1", tx, y + (H - CH) // 2, pm_x + h.cw("radioAfterPM"), CH)
    y += H + 4
    lab("label2")
    h.set("comboBoxMultiProv", F, y, fw, H)
    h.set("butProviders", F + fw + 6, y, 24, H)
    y += H + 4
    lab("labelBlockout")
    h.set("comboBlockout", F, y, fw, H)
    y += H + 4
    cl = h.clinic_w("comboBoxClinic")
    h.set("comboBoxClinic", F - cl, y, fw + cl, H)
    y += H + 4
    lab("labelAptViews")
    h.set("comboApptView", F, y, fw, H)
    y += H + PAD
    gw = F + fw + 6 + 24 + PAD
    gx = R - gw
    # The bottom left and the grid.
    q = h.n("groupBox1")
    tq = (q.title_bottom or 18) + 4
    h.set("butProvDentist", PAD, tq, 75, 24)
    h.set("butProvHygenist", PAD + 81, tq, 75, 24)
    qh = tq + 24 + PAD
    qw = PAD + 81 + 75 + PAD
    HH2 = max(HH, PAD + y + GAP + qh + PAD)
    qy = HH2 - PAD - qh
    h.set("groupBox1", PAD, qy, qw, qh)
    gr = gx - GAP
    h.set("butSearch", gr - 75, HH2 - PAD - 24, 75, 24)
    h.set("butMore", gr - 75, HH2 - PAD - 24 - 28, 75, 24)
    gb = min(qy, HH2 - PAD - 24 - 28) - GAP
    h.set("gridMain", PAD, PAD, gr - PAD, gb - PAD)
    h.set("groupBox2", gx, PAD, gw, gb - PAD)
    h.set(".", None, None, W, HH2)
    h.watch(".", "groupBox1", "groupBox2", "panel1")
