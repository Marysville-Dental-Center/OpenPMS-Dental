"""FormInfobutton on the grid (UI-12 slice 3, mode hand: the planner left the Provider and Organization fields on two columns and the add
buttons' texts against their icons). Main's arrangement on the grid's numbers: the Context group at the padding to the right edge: the
Patient group (the picker button, Name, Birthday and Language on a field column, the Administrative Gender group at its right edge); the
Provider and Organization groups side by side, one width and height, on the same field column; the Encounter group (its own, wider label
column); Requestor and Recipient side by side and Task Code with its box at the right edge. Under the group the add buttons in two rows of
four, one width with 6 px between the icon and the text (Lab Result, shown by the code, in the first row); the grid; Preview XML (shown by
the code) and Preview Request at the bottom left, Send at the bottom right."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W = root.w
    R = W - PAD
    H = h.H
    CH = h.CH
    gw = R - PAD
    cw = gw - 2 * PAD  # the Context group's content width
    lw = max(h.lw(n) for n in ("label1", "label3", "label4", "label5", "label6", "label7", "label10", "label9"))
    Fi = PAD + 24 + 6 + lw + 6
    ctx = h.n("groupBoxContext")
    ct = (ctx.title_bottom or 18) + 4
    # Patient.
    gp = h.n("groupBox3")
    tp = (gp.title_bottom or 18) + 4
    gg = h.n("groupBox4")
    tg = (gg.title_bottom or 18) + 4
    rad = ("radioPatGenFem", "radioPatGenMale", "radioPatGenUn")
    ggw = PAD + max(h.cw(r) for r in rad) + 24
    for i, r in enumerate(rad):
        h.check_left(r)
        h.set(r, PAD, tg + i * (CH + 4), h.cw(r), CH)
    ggh = tg + 3 * CH + 8 + PAD
    ggx = cw - PAD - ggw
    h.set("groupBox4", ggx, tp, ggw, ggh)
    fr = ggx - GAP
    h.set("butPatSelect", PAD, tp, 24, H)
    y = tp
    for lab, fld in (("label1", "textPatName"), ("label3", "textPatBirth"), ("label4", "comboPatLang")):
        h.set(lab, Fi - 6 - lw, y, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, Fi, y, fr - Fi, H)
        y += H + 4
    gph = max(y - 4, tp + ggh) + PAD
    h.set("groupBox3", PAD, ct, cw, gph)
    yc = ct + gph + GAP
    # Provider and Organization.
    half = (cw - GAP) // 2
    rows = {"groupBox6": (("label5", "textProvName"), ("label6", "textProvID"), ("label7", "comboProvLang")),
            "groupBox7": (("label10", "textOrgName"), ("label9", "textOrgID"))}
    tpv = (h.n("groupBox6").title_bottom or 18) + 4
    for k, grp in enumerate(("groupBox6", "groupBox7")):
        y = tpv
        for lab, fld in rows[grp]:
            h.set(lab, Fi - 6 - lw, y, lw, H)
            h.text_align(lab, "MiddleRight")
            h.set(fld, Fi, y, half - PAD - Fi, H)
            y += H + 4
    h.set("butProvSelect", PAD, tpv, 24, H)
    ph = tpv + 3 * (H + 4) - 4 + PAD
    h.set("groupBox6", PAD, yc, half, ph)
    h.set("groupBox7", PAD + cw - half, yc, half, ph)
    yc += ph + GAP
    # Encounter.
    te = (h.n("groupBox5").title_bottom or 18) + 4
    elw = max(h.lw("label26"), h.lw("label8"))
    Fe = PAD + elw + 6
    for i, (lab, fld, w) in enumerate((("label26", "comboEncType", 151), ("label8", "textEncLocID", 151))):
        h.set(lab, PAD, te + i * (H + 4), elw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, Fe, te + i * (H + 4), w, H)
    eh = te + 2 * H + 4 + PAD
    h.set("groupBox5", PAD, yc, cw, eh)
    yc += eh + GAP
    # Requestor, Recipient, Task Code.
    tr = (h.n("groupBox1").title_bottom or 18) + 4
    rw = 136
    for k, (grp, a, b) in enumerate((("groupBox1", "radioReqPat", "radioReqProv"), ("groupBox2", "radioRecPat", "radioRecProv"))):
        for i, r in enumerate((a, b)):
            h.check_left(r)
            h.set(r, PAD, tr + i * (CH + 4), h.cw(r), CH)
        h.set(grp, PAD + k * (rw + GAP), yc, rw, tr + 2 * CH + 4 + PAD)
    rh = tr + 2 * CH + 4 + PAD
    tc = h.n("comboTask")
    h.set("comboTask", cw - PAD - tc.nw, yc + tr, tc.nw, H)
    tl = h.lw("label17")
    h.set("label17", cw - PAD - tc.nw - 6 - tl, yc + tr, tl, H)
    h.text_align("label17", "MiddleRight")
    yc += rh + PAD
    h.set("groupBoxContext", PAD, PAD, gw, yc)
    y = PAD + yc + GAP
    # The add buttons.
    bw = max(6 + 16 + 6 + h.n(b).text_w + 6 for b in ("butAddDisease", "butAddRxNorm", "butAddAllergy", "button3", "butAddSnomed",
                                                       "butAddIcd9", "butAddIcd10", "butAddLoinc"))
    bw = ((bw + 3) // 4) * 4
    for row in (("butAddDisease", "butAddRxNorm", "butAddAllergy", "button3"), ("butAddSnomed", "butAddIcd9", "butAddIcd10", "butAddLoinc")):
        x = PAD
        for b in row:
            h.set(b, x, y, bw, 24)
            x += bw + 6
        y += 24 + 4
    y += GAP - 4
    gm = h.n("gridMain")
    HH = max(root.h, y + gm.nh + GAP + 24 + PAD)
    by = HH - PAD - 24
    h.set("gridMain", PAD, y, gw, by - GAP - y)
    h.set("butPreview", PAD, by, 127, 24)
    h.set("butPreviewRequest", PAD + 127 + 6, by, 127, 24)
    h.set("butSend", R - 75, by, 75, 24)
    h.set(".", None, None, W, HH)
    h.watch(".", "groupBoxContext", "groupBox3", "groupBox4", "groupBox6", "groupBox7", "groupBox5", "groupBox1", "groupBox2")
