"""FormPayPeriodManager on the grid (UI-12 slice 3, mode hand: the planner left the pay period count's label under its box and the radio
buttons ragged). Main's arrangement on the grid's numbers: Start Date with its picker; under it the Interval and Pay Day groups one width
(mostly radio buttons and check boxes, so the Preferences style: the choices at the content edge, the labels where their text starts, one
field column for both groups): the four intervals a list (the notes after Bi-Weekly and Semi-Monthly), the pay period count, the Semi-Monthly
Settings group (Period 1 and 2 Day with their boxes, End Date and Pay Date 16 px after the boxes, Last Day under them); Day of Week, OR,
the days after the pay period, Exclude Weekends and the two weekend choices; Generate under the groups at their right edge; the grid in the
right column to the window's edges above Save."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    P = CH + 4
    LX = PAD + 16
    lw = max(h.lw("label5"), h.lw("label4"), h.lw("label2"))
    F = LX + lw + 6
    gw = F + h.n("comboDay").nw + PAD
    L = PAD + gw  # the left column's right edge
    # Start Date.
    l1 = h.lw("label1")
    h.set("label1", PAD, PAD, l1, H)
    h.text_align("label1", "MiddleRight")
    h.set("dateTimeStart", PAD + l1 + 6, PAD, L - (PAD + l1 + 6), H)
    # Interval.
    g2 = h.n("groupBox2")
    top = (g2.title_bottom or 18) + 4
    y = top
    notes = {"radioBiWeekly": "label6", "radioSemiMonthly": "label7"}
    for r in ("radioWeekly", "radioMonthly", "radioBiWeekly", "radioSemiMonthly"):
        h.check_left(r)
        h.set(r, PAD, y, h.cw(r), CH)
        if r in notes:
            h.set(notes[r], PAD + h.cw(r) + 6, y, h.lw(notes[r]), CH)
            h.text_align(notes[r], "MiddleLeft")
        y += P
    h.set("label5", LX, y, lw, H)
    h.text_align("label5", "MiddleLeft")
    h.set("textPayPeriods", F, y, h.n("textPayPeriods").nw, H)
    y += H + GAP
    gs = h.n("groupBoxSemiMonthly")
    ts = (gs.title_bottom or 18) + 4
    slw = max(h.lw("labelPeriod1Day"), h.lw("labelPeriod2Day"))
    SF = PAD + slw + 6
    bw = 32
    for i, (lab, box, rad) in enumerate((("labelPeriod1Day", "textDay1", "radioEndDate"), ("labelPeriod2Day", "textDay2", "radioPayDate"))):
        yy = ts + i * (H + 4)
        h.set(lab, PAD, yy, slw, H)
        h.text_align(lab, "MiddleRight")
        h.set(box, SF, yy, bw, H)
        h.check_left(rad)
        h.set(rad, SF + bw + 16, yy + (H - CH) // 2, h.cw(rad), CH)
    yy = ts + 2 * (H + 4)
    h.check_left("checkLast")
    h.set("checkLast", SF, yy, h.cw("checkLast"), CH)
    sh = yy + CH + PAD
    h.set("groupBoxSemiMonthly", PAD, y, gw - 2 * PAD, sh)
    g2h = y + sh + PAD
    gy = PAD + H + GAP
    h.set("groupBox2", PAD, gy, gw, g2h)
    # Pay Day.
    g3 = h.n("groupBox3")
    t3 = (g3.title_bottom or 18) + 4
    y = t3
    h.set("label2", LX, y, lw, H)
    h.text_align("label2", "MiddleLeft")
    h.set("comboDay", F, y, h.n("comboDay").nw, H)
    y += H + 4
    h.set("label3", LX, y, h.lw("label3"), 17)
    h.text_align("label3", "MiddleLeft")
    y += 17 + 4
    h.set("label4", LX, y, lw, H)
    h.text_align("label4", "MiddleLeft")
    h.set("textDaysAfterPayPeriod", F, y, h.n("textDaysAfterPayPeriod").nw, H)
    y += H + 4
    for c in ("checkExcludeWeekends", "radioPayBefore", "radioPayAfter"):
        h.check_left(c)
        h.set(c, PAD, y, h.cw(c), CH)
        y += P
    g3h = y - 4 + PAD
    g3y = gy + g2h + GAP
    h.set("groupBox3", PAD, g3y, gw, g3h)
    gb = h.n("butGenerate")
    h.set("butGenerate", L - gb.nw, g3y + g3h + GAP, gb.nw, 24)
    need = g3y + g3h + GAP + 24 + PAD
    HH2 = max(HH, need)
    sv = h.n("butSave")
    h.set("butSave", R - sv.nw, HH2 - PAD - 24, sv.nw, 24)
    h.set("gridMain", L + GAP, PAD, R - (L + GAP), HH2 - PAD - 24 - GAP - PAD)
    h.set(".", None, None, W, HH2)
    h.watch(".", "groupBox2", "groupBox3", "groupBoxSemiMonthly")
