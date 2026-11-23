"""FormProcButtonEdit on the grid (UI-12 slice 3, mode hand: the planner left Group for multiple visits at 12 and the two list captions at
two heights). Main's arrangement on the grid's numbers: Description and Category on one field column, Group for multiple visits on the
column with its note under it; the procedure code and auto code lists under the note on the lists (centred over them), their captions, one
top and height, the order buttons between them centred on the lists; Add and Delete under the code list on the bottom bar. The right column
at the right edge: Image (20x20) with the picture, Clear and Import on the Category row, the image list under its caption; Save at the bottom
right."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    lw = max(h.lw("label1"), h.lw("label5"))
    F = PAD + lw + 6
    for i, (lab, fld) in enumerate((("label1", "textDescript"), ("label5", "comboCategory"))):
        y = PAD + i * (H + 4)
        h.set(lab, PAD, y, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, F, y, h.n(fld).nw, H)
    LR = F + h.n("textDescript").nw
    y = PAD + 2 * (H + 4)
    h.check_left("checkMultiVisit")
    h.set("checkMultiVisit", F, y, h.cw("checkMultiVisit"), CH)
    y += CH + 2
    nw = LR - F
    nh = h.note_h("label8", nw)
    h.set("label8", F, y, nw, nh)
    h.text_align("label8", "TopLeft")
    y += nh + GAP
    # The two lists and the order buttons.
    la, lb_ = h.n("listADA"), h.n("listAutoCodes")
    xa = PAD
    xb_ = xa + la.nw + GAP
    xc = xb_ + 33 + GAP
    lists_right = xc + lb_.nw
    h.set("label4", PAD, y, lists_right - PAD, 17)
    h.text_align("label4", "BottomCenter")
    y += 17 + 4
    by = HH - PAD - 24
    ly = y + 17 + 2
    lh = by - GAP - ly
    h.set("label2", xa, y, la.nw, 17)
    h.text_align("label2", "BottomLeft")
    h.set("listADA", xa, ly, la.nw, lh)
    h.set("label3", xc, y, lb_.nw, 17)
    h.text_align("label3", "BottomLeft")
    h.set("listAutoCodes", xc, ly, lb_.nw, lh)
    mid = ly + lh // 2
    h.set("butUp", xb_, mid - 2 - 26, 33, 26)
    h.set("butDown", xb_, mid + 2, 33, 26)
    h.set("butAdd", xa, by, 75, 24)
    h.set("butDelete", xa + 75 + 6, by, 75, 24)
    # The right column.
    lv = h.n("listView")
    RC = R - lv.nw
    y2 = PAD + H + 4
    l6 = h.lw("label6")
    h.set("label6", RC, y2, l6, H)
    h.text_align("label6", "MiddleLeft")
    px = RC + l6 + 6
    h.set("pictureBox", px, y2 + (H - 20) // 2, 20, 20)
    h.set("butClear", px + 26, y2, 75, H)
    h.set("butImport", px + 26 + 81, y2, 75, H)
    y3 = y2 + H + 4
    h.set("label7", RC, y3, lv.nw, 17)
    h.text_align("label7", "BottomLeft")
    h.set("listView", RC, y3 + 19, lv.nw, lv.nh)
    h.set("butSave", R - 75, by, 75, 24)
    assert LR + 16 <= RC
