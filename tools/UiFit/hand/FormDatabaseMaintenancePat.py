"""FormDatabaseMaintenancePat on the grid (UI-12 slice 3, mode hand). Main's arrangement on the grid's numbers: the Patient caption (a
borderless text box) as wide as its text before the patient box and its picker button at the padding; the tab control 8 px under them to the
window's edges. Every page from its padding: the grid to the page's edges; under it the None row (the right-aligned note box and None at the
right edge; the log check box, or the Old page's note, at the left) and the button row (Show Hidden at the left on the Old page; Check and
Fix 6 px apart at the right edge); the Hidden page's note under its grid. The note boxes keep main's white text box look (a text box)."""

PAD = 8
GAP = 8


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    lw = 48
    h.set("textBox2", PAD, PAD + (H - 16) // 2, lw, 16)
    h.set("textPatient", PAD + lw + 6, PAD, 200, H)
    h.set("butPatientSelect", PAD + lw + 6 + 200 + 6, PAD, 24, H)
    ty = PAD + H + GAP
    tw = R - PAD
    th = HH - PAD - ty
    h.set("tabControlDBM", PAD, ty, tw, th)
    pg = h.n("tabChecks")
    pw = tw - 4
    ph = th - pg.ny - 2
    for p in ("tabChecks", "tabHidden", "tabOld"):
        h.set(p, None, None, pw, ph)
    PR = pw - PAD
    cw = PR - PAD
    rb = ph - PAD - 24
    nb = 32  # the two-line notes
    nw = PR - 75 - 6 - 365 - 16 - PAD
    l1h = h.note_h("label1", nw)
    for grid, note, none, check, fix in (("gridMain", "textBox1", "butNone", "butCheck", "butFix"),
                                         ("gridOld", "textBox4", "butNoneOld", "butCheckOld", "butFixOld")):
        ra = rb - GAP - (nb if grid == "gridMain" else max(nb, l1h))
        h.set(grid, PAD, PAD, cw, ra - GAP - PAD)
        h.set(none, PR - 75, ra, 75, 24)
        h.set(note, PR - 75 - 6 - 365, ra, 365, nb)
        h.set(fix, PR - 75, rb, 75, 24)
        h.set(check, PR - 75 - 6 - 75, rb, 75, 24)
    ra = rb - GAP - nb
    h.check_left("checkShow")
    h.set("checkShow", PAD, ra + (24 - CH) // 2, h.cw("checkShow"), CH)
    h.set("label1", PAD, rb - GAP - max(nb, l1h), nw, l1h)
    h.text_align("label1", "TopLeft")
    h.check_left("checkShowHidden")
    h.set("checkShowHidden", PAD, rb + (24 - CH) // 2, h.cw("checkShowHidden"), CH)
    lh = h.note_h("labelOld", cw)
    h.set("gridHidden", PAD, PAD, cw, rb + 24 - lh - GAP - PAD)
    h.set("labelOld", PAD, rb + 24 - lh, cw, lh)
    h.text_align("labelOld", "TopLeft")
    h.watch(".", "tabChecks", "tabHidden", "tabOld")
