"""FormEhrSettings on the grid (UI-12 slice 3, mode hand: the planner left an empty band over the code buttons in one group and not in the
other, and the warnings off the column). Main's arrangement on the grid's numbers, the three groups at the padding to the right edge, one
field column in both code groups: Global Settings (the Rx alerts check box with its box 6 px after its text, the webmail check box); the
encounter and pregnancy code groups alike: the group's note on its lines, Recommended Codes with its box and the 'none' note after it,
Description with its box to the content edge, the recommended list note under it on the column, the code system buttons 6 px apart, Code
with its box, the warning on the column; the encounter group's Insert Encs with its note at the content edge; Save at the bottom right."""

PAD = 8
GAP = 8


def code_group(h, grp, intro, lab_rec, combo, note_none, lab_desc, desc, note_rec, buttons, lab_code, code, warn, lw, extra=None):
    H = h.H
    g = h.n(grp)
    top = (g.title_bottom or 18) + 4
    gw = g.nw
    cr = gw - PAD
    F = PAD + lw + 6
    nh = h.note_h(intro, cr - PAD)
    h.set(intro, PAD, top, cr - PAD, nh)
    h.text_align(intro, "TopLeft")
    y = top + nh + 4
    h.set(lab_rec, PAD, y, lw, H)
    h.text_align(lab_rec, "MiddleRight")
    h.set(combo, F, y, h.n(combo).nw, H)
    nx = F + h.n(combo).nw + 6
    h.set(note_none, nx, y, h.lw(note_none), H)
    h.text_align(note_none, "MiddleLeft")
    y += H + 4
    h.set(lab_desc, PAD, y, lw, H)
    h.text_align(lab_desc, "MiddleRight")
    dh = h.n(desc).nh
    h.set(desc, F, y, cr - F, dh)
    y += dh + 2
    rh = h.note_h(note_rec, cr - F)
    h.set(note_rec, F, y, cr - F, rh)
    h.text_align(note_rec, "TopLeft")
    y += rh + 4
    x = F
    for b in buttons:
        bw = max(75, ((h.n(b).text_w + 12 + 3) // 4) * 4)  # 6 px on each side of the text (SNOMED CT filled its 77 px)
        h.set(b, x, y, bw, 24)
        x += bw + 6
    y += 24 + 4
    h.set(lab_code, PAD, y, lw, H)
    h.text_align(lab_code, "MiddleRight")
    h.set(code, F, y, h.n(combo).nw, H)
    y += H + 4
    wh = h.note_h(warn, cr - F)
    h.set(warn, F, y, cr - F, wh)
    h.text_align(warn, "TopLeft")
    y += wh
    if extra:
        but, note = extra
        y += GAP
        h.set(but, PAD, y, h.n(but).nw, 24)
        h.set(note, PAD + h.n(but).nw + 6, y, cr - (PAD + h.n(but).nw + 6), 24)
        h.text_align(note, "MiddleLeft")
        y += 24
    return y + PAD


def layout(h):
    root = h.tree
    W = root.w
    R = W - PAD
    H = h.H
    CH = h.CH
    gw = R - PAD
    for g in ("groupGlobalSettings", "groupEncounter", "groupPregnancy"):
        h.set(g, None, None, gw, None)
    g = h.n("groupGlobalSettings")
    top = (g.title_bottom or 18) + 4
    h.check_left("checkAlertHighSeverity")
    h.set("checkAlertHighSeverity", PAD, top + (H - CH) // 2, h.cw("checkAlertHighSeverity"), CH)
    h.set("comboMU2", PAD + h.cw("checkAlertHighSeverity") + 6, top, h.n("comboMU2").nw, H)
    h.check_left("checkAutoWebmails")
    h.set("checkAutoWebmails", PAD, top + H + 4, h.cw("checkAutoWebmails"), CH)
    gh = top + H + 4 + CH + PAD
    h.set("groupGlobalSettings", PAD, PAD, gw, gh)
    y = PAD + gh + GAP
    lw = max(h.lw(n) for n in ("label4", "label2", "label5", "label12", "label10", "label9"))
    eh = code_group(h, "groupEncounter", "label1", "label4", "comboEncCodes", "label6", "label2", "textEncCodeDescript", "label3",
                    ("butEncSnomed", "butEncCdt", "butEncCpt", "butEncHcpcs"), "label5", "textEncCodeValue", "labelEncWarning", lw,
                    extra=("butEncounterTool", "label7"))
    h.set("groupEncounter", PAD, y, gw, eh)
    y += eh + GAP
    ph = code_group(h, "groupPregnancy", "label11", "label12", "comboPregCodes", "label8", "label10", "textPregCodeDescript", "label13",
                    ("butPregSnomed", "butPregIcd9", "butPregIcd10"), "label9", "textPregCodeValue", "labelPregWarning", lw)
    h.set("groupPregnancy", PAD, y, gw, ph)
    y += ph + GAP
    HH = max(root.h, y + 24 + PAD)
    h.set("butSave", R - h.n("butSave").nw, HH - PAD - 24, None, 24)
    h.set(".", None, None, W, HH)
    h.watch(".", "groupGlobalSettings", "groupEncounter", "groupPregnancy")
