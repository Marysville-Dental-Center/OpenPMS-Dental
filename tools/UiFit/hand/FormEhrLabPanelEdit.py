"""FormEhrLabPanelEdit on the grid (UI-12 slice 3, mode hand: the planner left the message box, the groups and the grid at 12 and three right
edges). Main's arrangement on the grid's numbers, everything from the padding to the right edge: the raw message under its caption;
Patient Name, the Service group (LOINC, Name), Lab Name / Address (two lines), Specimen Condition and the Specimen Source group (Code with
its format note, Location) on one field column for the window and the groups; the results grid; Delete, Add Result (centred, as main) and
Save on the bottom bar."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W = root.w
    R = W - PAD
    H = h.H
    lw = max(h.lw(n) for n in ("label5", "label3", "label2", "label8", "label9", "label4", "label6"))
    F = PAD + lw + 6
    Fi = F - PAD
    y = h.captioned("label1", "textRawMsg", PAD, PAD, R - PAD, h.n("textRawMsg").nh) + 4

    def row(lab, fld, w, fh=None, x0=PAD, f=F, ta="MiddleRight"):
        nonlocal y
        fh = fh or H
        h.set(lab, x0, y, lw, H if ta == "MiddleRight" else 17)
        h.text_align(lab, ta)
        h.set(fld, f, y, w, fh)
        y += fh + 4

    row("label5", "textName", R - F)
    y += GAP - 4
    gw = R - PAD
    cr = gw - PAD
    for grp, rows in (("groupBox2", (("label8", "textServiceID", 106, None), ("label9", "textServiceName", None, None))),):
        pass
    g = h.n("groupBox2")
    top = (g.title_bottom or 18) + 4
    h.set("label8", PAD, top, lw, H)
    h.set("textServiceID", Fi, top, 106, H)
    h.set("label9", PAD, top + H + 4, lw, H)
    h.set("textServiceName", Fi, top + H + 4, cr - Fi, H)
    gh = top + 2 * H + 4 + PAD
    h.set("groupBox2", PAD, y, gw, gh)
    y += gh + GAP
    lh = h.n("textLabName").nh
    h.set("label2", PAD, y + 3, lw, 17)
    h.text_align("label2", "TopRight")
    h.set("textLabName", F, y, R - F, lh)
    y += lh + 4
    row("label3", "textSpecimenCondition", 158)
    y += GAP - 4
    g1 = h.n("groupBox1")
    t1 = (g1.title_bottom or 18) + 4
    h.set("label4", PAD, t1, lw, H)
    h.set("textSpecimenSourceCode", Fi, t1, 64, H)
    h.set("label7", Fi + 70, t1, h.lw("label7"), H)
    h.text_align("label7", "MiddleLeft")
    h.set("label6", PAD, t1 + H + 4, lw, H)
    h.set("textSpecimenLocation", Fi, t1 + H + 4, cr - Fi, H)
    g1h = t1 + 2 * H + 4 + PAD
    h.set("groupBox1", PAD, y, gw, g1h)
    y += g1h + GAP
    for lab in ("label8", "label9", "label4", "label6"):
        h.text_align(lab, "MiddleRight")
    gm = h.n("gridMain")
    HH = max(root.h, y + gm.nh + GAP + 24 + PAD)
    by = HH - PAD - 24
    h.set("gridMain", PAD, y, R - PAD, by - GAP - y)
    h.set("butDelete", PAD, by, 75, 24)
    h.set("butAdd", (W - 81) // 2, by, 81, 24)
    h.set("butSave", R - 75, by, 75, 24)
    h.set(".", None, None, W, HH)
    h.watch(".", "groupBox1", "groupBox2")
