"""FormBillingDefaults on the grid (UI-12 slice 3, mode hand: the planner reached no fixed point and spread the Statements group to 4097 px).
Main's arrangement on the grid's numbers: the clinic picker at the padding with the practice defaults check box (shown for a clinic) 16 px
after it; the Statements group: Intermingle family members and Show all transactions in the left column (Single patient only, shown by the
code, after Intermingle), Start Date Last with its days box and Print inserter barcode in the right column 16 px after the left one, the
label where the check box text starts; the Messages group: the general message note over its box, the Email Statements group (the
variables note, Subject and Body as captioned boxes, the autograph check box with Email type and Edit HTML after it), Invoice Note over its
box; 8 px between the blocks; the bottom bar. The window grows down by what the blocks need."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W = root.w
    R = W - PAD
    H = h.H
    CH = h.CH
    # The clinic row.
    pk = h.n("comboClinic")
    h.set("comboClinic", PAD, PAD, pk.nw, H)
    h.check_left("checkUsePracticeDefaults")
    h.set("checkUsePracticeDefaults", PAD + pk.nw + 16, PAD + (H - CH) // 2, h.cw("checkUsePracticeDefaults"), CH)
    gw = R - PAD
    # Statements.
    gs = h.n("groupStatements")
    top = (gs.title_bottom or 18) + 4
    left = [("checkIntermingled", "checkSinglePatient"), ("checkBoxBillShowTransSinceZero",)]
    lright = max(PAD + h.cw("checkBoxBillShowTransSinceZero"), PAD + h.cw("checkIntermingled") + 16 + h.cw("checkSinglePatient"))
    C = lright + 16
    y1 = top
    for name, x in (("checkIntermingled", PAD), ("checkSinglePatient", PAD + h.cw("checkIntermingled") + 16)):
        h.check_left(name)
        h.set(name, x, y1 + (H - CH) // 2, h.cw(name), CH)
    lx = C + 16
    h.set("labelStartDate", lx, y1, h.lw("labelStartDate"), H)
    h.text_align("labelStartDate", "MiddleLeft")
    dx = lx + h.lw("labelStartDate") + 6
    h.set("textDays", dx, y1, h.n("textDays").nw, H)
    h.set("label1", dx + h.n("textDays").nw + 6, y1, h.lw("label1"), H)
    h.text_align("label1", "MiddleLeft")
    y2 = y1 + H + 4
    for name, x in (("checkBoxBillShowTransSinceZero", PAD), ("checkInserterBarcode", C)):
        h.check_left(name)
        h.set(name, x, y2, h.cw(name), CH)
    gsh = y2 + CH + PAD
    gy = PAD + H + GAP
    h.set("groupStatements", PAD, gy, gw, gsh)
    # Messages.
    gm = h.n("groupMessages")
    top = (gm.title_bottom or 18) + 4
    cw = gw - 2 * PAD
    y = h.captioned("label4", "textNote", PAD, top, cw, h.n("textNote").nh) + GAP
    g3 = h.n("groupBox3")
    t3 = (g3.title_bottom or 18) + 4
    iw = cw - 2 * PAD
    nh = h.note_h("label10", iw)
    h.set("label10", PAD, t3, iw, nh)
    y3 = t3 + nh + 4
    y3 = h.captioned("label9", "textBillingEmailSubject", PAD, y3, iw) + GAP
    y3 = h.captioned("label8", "textBillingEmailBody", PAD, y3, iw, h.n("textBillingEmailBody").nh) + GAP
    ck = "checkBillingEmailIncludeAutograph"
    h.check_left(ck)
    h.set(ck, PAD, y3 + (H - CH) // 2, h.cw(ck), CH)
    lx = PAD + h.cw(ck) + 16
    lw = h.lw("labelBillingEmailType")
    h.set("labelBillingEmailType", lx, y3, lw, H)
    h.text_align("labelBillingEmailType", "MiddleRight")
    cx = lx + lw + 6
    h.set("comboBillingEmailType", cx, y3, h.n("comboBillingEmailType").nw, H)
    h.set("butBillingEmailEditHtml", cx + h.n("comboBillingEmailType").nw + 6, y3, None, H)
    g3h = y3 + H + PAD
    h.set("groupBox3", PAD, y, cw, g3h)
    y += g3h + GAP
    y = h.captioned("label11", "textInvoiceNote", PAD, y, cw, h.n("textInvoiceNote").nh)
    gmh = y + PAD
    my = gy + gsh + GAP
    h.set("groupMessages", PAD, my, gw, gmh)
    by = my + gmh + GAP
    hh = max(root.h, by + 24 + PAD)
    by = hh - PAD - 24
    h.set("butEbill", PAD, by, None, 24)
    h.set("butSendSetup", PAD + h.n("butEbill").nw + 6, by, None, 24)
    h.set("butSave", R - h.n("butSave").nw, by, None, 24)
    h.set(".", None, None, W, hh)
    h.watch(".", "groupStatements", "groupMessages", "groupBox3")
