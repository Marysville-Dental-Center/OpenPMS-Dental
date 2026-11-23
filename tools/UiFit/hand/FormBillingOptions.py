"""FormBillingOptions on the grid (UI-12 slice 3, mode hand: the planner put the Filter group's long check box on one line and the window at
1184 px). Main's two columns on the grid's numbers. The Filter group (mostly check boxes, so the Preferences style): the check boxes at the
content edge, the labels where their text starts, every field on one column, the combo boxes and the billing type list to the content's
right edge, the clinic picker's box on the column with Use clinic default billing options under it, Save As Default on the column with its
note under it. The right column 8 px after the group: the dunning note over the dunning grid; under the grid the Account History Date
Range group (Start and End Date on one column, the four range buttons after them, the zero balance check box under the boxes) with Defaults
and Setup Dunning at the right edge on its top; Intermingle family members and Service Date View, Group by Super Family (and Single patient
only, shown by the code) under it; the general message (on two lines, main cut it) over its box (the multi-clinic note, shown by the code in
the box's place, on the box's top); Undo Billing and Create List on the bottom bar. The window grows right by the group's width."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    H = h.H
    CH = h.CH
    # The Filter group.
    g = h.n("groupBox2")
    top = (g.title_bottom or 18) + 4
    labels = ["label5", "label6", "labelExcludeIfPendingDays", "label1", "labelInsFilingCodes", "label7"]
    lw = max(h.lw(n) for n in labels)
    LX = PAD + 16
    F = LX + lw + 6
    RR = F + h.n("listBillType").nw  # the group's content right edge
    gw = RR + PAD
    y = top
    prev = None

    def gap(kind):
        if prev is None:
            return 0
        return 4

    rows = [("f", "label5", "textLastStatement"), ("c", "checkIncludeChanged"), ("f", "label6", "comboAge"), ("c", "checkBadAddress"),
            ("c", "checkExcludeInactive"), ("c", "checkExcludeInsPending"), ("f", "labelExcludeIfPendingDays", "textDaysInsPendingExclude"),
            ("c", "checkExcludeIfProcs"), ("f", "label1", "textExcludeLessThan"), ("c", "checkShowNegative"), ("c", "checkIgnoreInPerson"),
            ("f", "labelInsFilingCodes", "comboInsFilingCodes"), ("l", "label7", "listBillType")]
    for r in rows:
        y += gap(r[0])
        if r[0] == "c":
            name = r[1]
            h.check_left(name)
            room = RR - PAD
            ch = CH if h.cw(name) <= room else max(CH, h.note_h(name, room - 16))
            h.set(name, PAD, y, h.cw(name) if h.cw(name) <= room else room, ch)
            y += ch
        else:
            lab, fld = r[1], r[2]
            f = h.n(fld)
            fw = RR - F if (f.nw >= 120 or r[0] == "l") else f.nw
            fh = f.nh if r[0] == "l" else H
            h.set(fld, F, y, fw, fh)
            h.set(lab, LX, y, lw, H)
            h.text_align(lab, "MiddleLeft")
            y += fh
        prev = r
    y += 4
    pk = h.n("comboClinic")
    cwl = h.clinic_w("comboClinic")
    h.set("comboClinic", F - cwl, y, RR - F + cwl, H)
    y += H + 4
    h.check_left("checkUseClinicDefaults")
    h.set("checkUseClinicDefaults", PAD, y, h.cw("checkUseClinicDefaults"), CH)
    y += CH + 4
    h.set("butSaveDefault", F, y, h.n("butSaveDefault").nw, 24)
    y += 24 + 2
    h.set("labelSaveDefaults", F, y, RR - F, 17)
    h.text_align("labelSaveDefaults", "TopLeft")
    y += 17
    gh = y + PAD
    # The right column.
    x0 = PAD + gw + GAP
    rw = h.n("gridDunning").nw
    W = x0 + rw + PAD
    R = W - PAD
    # The Account History Date Range group.
    gd = h.n("groupDateRange")
    tdd = (gd.title_bottom or 18) + 4
    dl = max(h.lw("labelStartDate"), h.lw("labelEndDate"))
    DF = PAD + dl + 6
    dw = h.n("textDateStart").nw
    bw = 77
    b1 = DF + dw + 6
    b2 = b1 + bw + 6
    for i, (lab, fld, ba, bb) in enumerate((("labelStartDate", "textDateStart", "but30days", "butDatesAll"),
                                            ("labelEndDate", "textDateEnd", "but45days", "but90days"))):
        yy = tdd + i * (H + 4)
        h.set(lab, PAD, yy, dl, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, DF, yy, dw, H)
        h.set(ba, b1, yy, bw, H)
        h.set(bb, b2, yy, bw, H)
    yy = tdd + 2 * (H + 4)
    h.check_left("checkBoxBillShowTransSinceZero")
    h.set("checkBoxBillShowTransSinceZero", DF, yy, h.cw("checkBoxBillShowTransSinceZero"), CH)
    gdw = max(b2 + bw, DF + h.cw("checkBoxBillShowTransSinceZero")) + PAD
    gdh = yy + CH + PAD
    # From the bottom up: the bar, the general message box, its caption, the check boxes, the date group; the grid fills the rest.
    note_cap = h.note_h("label4", rw)
    need_right = PAD + 17 + 2 + 200 + GAP + gdh + GAP + (CH + 4) * 2 + GAP + note_cap + 2 + h.n("textNote").nh + GAP + 24 + PAD
    HH = max(root.h, PAD + gh + GAP + 24 + PAD, need_right)
    by = HH - PAD - 24
    h.set("butUndo", PAD, by, None, 24)
    h.set("butCreate", R - h.n("butCreate").nw, by, None, 24)
    nb = by - GAP
    tn = h.n("textNote").nh
    h.set("textNote", x0, nb - tn, rw, tn)
    h.set("labelMultiClinicGenMsg", x0, nb - tn, rw, h.note_h("labelMultiClinicGenMsg", rw))
    h.set("label4", x0, nb - tn - 2 - note_cap, rw, note_cap)
    h.text_align("label4", "BottomLeft")
    cy2 = nb - tn - 2 - note_cap - GAP - CH
    cy1 = cy2 - CH - 4
    for name, x, yy in (("checkIntermingled", x0, cy1), ("checkServiceDateView", x0 + h.cw("checkIntermingled") + 16, cy1),
                        ("checkSuperFam", x0, cy2), ("checkSinglePatient", x0 + h.cw("checkIntermingled") + 16, cy2)):
        h.check_left(name)
        h.set(name, x, yy, h.cw(name), CH)
    gdy = cy1 - GAP - gdh
    h.set("groupDateRange", x0, gdy, gdw, gdh)
    h.set("butDunningSetup", R - h.n("butDunningSetup").nw, gdy, None, 24)
    h.set("butDefaults", R - h.n("butDunningSetup").nw - 6 - h.n("butDefaults").nw, gdy, None, 24)
    h.set("label3", x0, PAD, rw, 17)
    h.text_align("label3", "BottomLeft")
    h.set("gridDunning", x0, PAD + 17 + 2, rw, gdy - GAP - (PAD + 17 + 2))
    h.set("groupBox2", PAD, PAD, gw, gh)
    h.set(".", None, None, W, HH)
    h.watch(".", "groupBox2", "groupDateRange")
