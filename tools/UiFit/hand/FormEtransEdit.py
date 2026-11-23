"""FormEtransEdit on the grid (UI-12 slice 3, mode hand: the planner left the sent message 7 px above the received one and the left column
at 10). Two columns one width, 8 px apart: the sent message under its caption on the received message's top and bottom, DateTime Trans with
Print at the column's edge, ClaimNum, BatchNumber, TransSetNum, AckCode and Note on one field column (Attachments Sent, shown by the code,
under the note); the Acknowledgement group: the received message under its caption, DateTime Ack with Preview, Note on the same field
column; Save at the bottom right."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    by = HH - PAD - 24
    cw = (R - PAD - GAP) // 2
    gx = R - cw
    lw = max(h.lw(n) for n in ("label8", "label2", "label3", "label4", "label5", "label6", "label9", "label10"))
    F = PAD + lw + 6
    g = h.n("groupAck")
    top = (g.title_bottom or 18) + 4
    cap_y = PAD + top
    box_y = cap_y + 19
    below = 4 + 5 * (H + 4) + h.n("textNote").nh + 4 + CH
    box_h = by - GAP - below - box_y
    lr = gx - GAP  # the left column ends 8 px before the group
    h.set("label1", PAD, cap_y, lr - PAD, 17)
    h.text_align("label1", "BottomLeft")
    h.set("textMessageText", PAD, box_y, lr - PAD, box_h)
    y = box_y + box_h + 4
    h.set("label8", PAD, y, lw, H)
    h.set("textDateTimeTrans", F, y, 200, H)
    h.set("butPrint", lr - 81, y, 81, H)
    y += H + 4
    for lab, fld in (("label2", "textClaimNum"), ("label3", "textBatchNumber"), ("label4", "textTransSetNum"), ("label5", "textAckCode")):
        h.set(lab, PAD, y, lw, H)
        h.set(fld, F, y, 100, H)
        y += H + 4
    nh = h.n("textNote").nh
    h.set("label6", PAD, y, lw, H)
    h.set("textNote", F, y, lr - F, nh)
    y += nh + 4
    h.check_left("checkAttachments")
    h.set("checkAttachments", F, y, h.cw("checkAttachments"), CH)
    for lab in ("label8", "label2", "label3", "label4", "label5", "label6"):
        h.text_align(lab, "MiddleRight")
    # The Acknowledgement group (its coordinates).
    gr = cw - PAD
    Fg = F
    h.set("label7", PAD, top, gr - PAD, 17)
    h.text_align("label7", "BottomLeft")
    h.set("textAckMessage", PAD, top + 19, gr - PAD, box_h)
    yy = top + 19 + box_h + 4
    h.set("label9", PAD, yy, lw, H)
    h.set("textAckDateTime", Fg, yy, 200, H)
    h.set("butPrintAck", gr - 81, yy, 81, H)
    yy += H + 4
    h.set("label10", PAD, yy, lw, H)
    h.set("textAckNote", Fg, yy, gr - Fg, nh)
    for lab in ("label9", "label10"):
        h.text_align(lab, "MiddleRight")
    gh = yy + nh + PAD
    h.set("groupAck", gx, PAD, cw, gh)
    h.set("butSave", R - 75, by, 75, 24)
    h.watch(".", "groupAck")
