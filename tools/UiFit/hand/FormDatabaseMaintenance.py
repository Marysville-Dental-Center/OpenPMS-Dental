"""FormDatabaseMaintenance on the grid (UI-12 slice 3, mode hand). Main's arrangement on the grid's numbers: the tab control at the padding to
the right edge and down to the table check note (shown by the code) at the bottom right. Every page from its padding: the page's note box at
the top, the grid 8 px under it to the page's edges; under the grid the None row (its note box and None at the right edge, the log check
box at the left) and the button row (Print or Show Hidden at the left; Check, Fix, Stop DBM and Explain 6 px apart ending at the right
edge); the Hidden page's Select All under its grid at the right edge and Explain on the button row; the Tools page's update group (its note,
the computer box and Clear), its note box, the grid and Fix at the bottom right."""

PAD = 8
GAP = 8


def right_row(h, names, R, y):
    x = R
    for b in reversed(names):
        x -= h.n(b).nw
        h.set(b, x, y, None, 24)
        x -= 6


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    lab = h.n("labelSkipCheckTable")
    ly = HH - PAD - 17
    h.set("labelSkipCheckTable", R - lab.nw, ly, lab.nw, 17)
    tw = R - PAD
    th = ly - 4 - PAD
    h.set("tabControlDBM", PAD, PAD, tw, th)
    pg = h.n("tabChecks")
    pw = tw - 4
    ph = th - pg.ny - 2
    for p in ("tabChecks", "tabHidden", "tabOld", "tabTools"):
        h.set(p, None, None, pw, ph)
    PR = pw - PAD
    cw = PR - PAD
    rb = ph - PAD - 24
    # Checks.
    t = h.n("textChecks").nh
    h.set("textChecks", PAD, PAD, cw, t)
    nb = h.n("textNoneChecks").nh
    ra = rb - GAP - nb
    h.set("gridMain", PAD, PAD + t + GAP, cw, ra - GAP - (PAD + t + GAP))
    h.set("butNoneChecks", PR - 75, ra, 75, 24)
    h.set("textNoneChecks", PR - 75 - 6 - h.n("textNoneChecks").nw, ra, h.n("textNoneChecks").nw, nb)
    h.check_left("checkShow")
    h.set("checkShow", PAD, ra + (nb - CH) // 2, h.cw("checkShow"), CH)
    h.set("butPrint", PAD, rb, None, 24)
    right_row(h, ["butCheck", "butFix", "butStopDBM", "butExplainChecks"], PR, rb)
    # Hidden.
    t = h.n("textHidden").nh
    h.set("textHidden", PAD, PAD, cw, t)
    sa = rb - GAP - 24
    h.set("gridHidden", PAD, PAD + t + GAP, cw, sa - GAP - (PAD + t + GAP))
    h.set("butSelectAll", PR - 75, sa, 75, 24)
    h.set("butExplainHidden", PR - 75, rb, 75, 24)
    # Old.
    t = h.n("textOld").nh
    h.set("textOld", PAD, PAD, cw, t)
    nb = h.n("textNoneOld").nh
    ra = rb - GAP - nb
    h.set("gridOld", PAD, PAD + t + GAP, cw, ra - GAP - (PAD + t + GAP))
    h.set("butNoneOld", PR - 75, ra, 75, 24)
    h.set("textNoneOld", PR - 75 - 6 - h.n("textNoneOld").nw, ra, h.n("textNoneOld").nw, nb)
    h.check_left("checkShowHidden")
    h.set("checkShowHidden", PAD, rb + (24 - CH) // 2, h.cw("checkShowHidden"), CH)
    right_row(h, ["butCheckOld", "butFixOld", "butStopDBMOld", "butExplainOld"], PR, rb)
    # Tools.
    g = h.n("groupBoxUpdateInProg")
    top = (g.title_bottom or 18) + 4
    gcw = cw - 2 * PAD
    nh = h.note_h("labelUpdateInProgress", gcw)
    h.set("labelUpdateInProgress", PAD, top, gcw, nh)
    h.text_align("labelUpdateInProgress", "MiddleLeft")
    yy = top + nh + 4
    h.set("textBoxUpdateInProg", PAD, yy, h.n("textBoxUpdateInProg").nw, H)
    h.set("butClearUpdateInProgress", PAD + h.n("textBoxUpdateInProg").nw + 6, yy, 78, H)
    gh = yy + H + PAD
    h.set("groupBoxUpdateInProg", PAD, PAD, cw, gh)
    y = PAD + gh + GAP
    t = h.n("textTools").nh
    h.set("textTools", PAD, y, cw, t)
    y += t + GAP
    h.set("gridTools", PAD, y, cw, rb - GAP - y)
    h.set("butFixTools", PR - 75, rb, 75, 24)
    h.watch(".", "tabChecks", "tabHidden", "tabOld", "tabTools", "groupBoxUpdateInProg")
