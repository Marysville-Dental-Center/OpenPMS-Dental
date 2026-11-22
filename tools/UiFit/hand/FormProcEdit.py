"""FormProcEdit (Edit Procedure) on the grid (UI-12 round 3, layout script of grid_hand.py). Its four tabs are laid out by the planner in parts
(grid-scope.tsv); this script runs on the planner's tree and places the rest: the dates and the procedure (panel1) and the fields under them
in one field column, the status, provider, notes and signature in a second column, the tab control under both, the bottom bar. The tooth, range,
surface, arch, quadrant and sextant fields are switched by the procedure's treatment area (FillControls): the tooth and the range share a row,
the surfaces and the three area groups share the next rows (one of them shows), so the rows under them leave room for the tallest. The window
grows to 984 x 873."""


def layout(h):
    H = h.H
    PW = 432  # the left column (panel1 and the fields under it)
    p = h.box("panel1", 0, 0, PW)
    F = 126  # the left column's fields: on the tabs' field column (the planner's, 126 px in the window); "E-claim Note (keep it very short)" wraps onto two lines
    wd = 80
    p.field("label12", "textDateEntry", F, wd)
    p.field("label27", "textDateTP", F, wd)
    p.field("labelDate", "textDate", F, wd, after=[("labelStartTime", None, h.lw("labelStartTime")), ("textTimeStart", None, 56),
                                                     ("labelEndTime", None, h.lw("labelEndTime")), ("textTimeEnd", None, 56), ("butNow", None, None)])
    for lb in ("labelStartTime", "labelEndTime"):
        h.text_align(lb, "MiddleRight")
    p.field("label2", "textProc", F, wd, after=[("butChange", None, None)])
    # Original Date Comp and Final time: a second band on the first and the fourth rows, right-aligned to the panel's content edge.
    wb = 80
    xb = p.right - wb
    lo = h.lw("labelOrigDateComp")
    h.set("labelOrigDateComp", xb - 6 - lo, h.n("textDateEntry").ny, lo, H)
    h.text_align("labelOrigDateComp", "MiddleRight")
    h.set("textOrigDateComp", xb, h.n("textDateEntry").ny, wb, H)
    lf = h.lw("labelTimeFinal")
    h.set("labelTimeFinal", xb - 6 - lf, h.n("textProc").ny, lf, H)
    h.text_align("labelTimeFinal", "MiddleRight")
    h.set("textTimeFinal", xb, h.n("textProc").ny, 56, H)
    p.field("label6", "textDesc", F, "R")
    # The tooth row (Tooth or Tooth Range; the range's two lists of teeth stand under its label's row).
    yt = p.y
    lt = max(h.lw("labelTooth"), h.lw("labelRange"))
    for lb in ("labelTooth", "labelRange"):
        h.set(lb, F - 6 - lt, yt, lt, H)
        h.text_align(lb, "MiddleRight")
    h.set("textTooth", F, yt, 40, H)
    h.set("textRange", F, yt, 100, H)
    h.set("listBoxTeeth", F, yt + 3, 275, 17)
    h.set("listBoxTeeth2", F, yt + 3 + 17, 275, 17)
    # The surfaces row; the surface buttons beside the tooth and surface boxes; the arch, quadrant and sextant groups in the surfaces' place.
    ys = yt + 27
    ls = h.lw("labelSurfaces")
    h.set("labelSurfaces", F - 6 - ls, ys, ls, H)
    h.text_align("labelSurfaces", "MiddleRight")
    h.set("textSurfaces", F, ys, 72, H)
    h.set("panelSurfaces", F + 72 + 16, yt, 96, 66)
    hg = 0
    for gname in ("groupArch", "groupQuadrant", "groupSextant"):
        g = h.n(gname)
        top = g.title_bottom + 2
        kids = sorted([k for k in g.children if k.kind == "radio"], key=lambda k: (k.y, k.x))
        cols = sorted({k.x for k in kids})
        rows = sorted({k.y for k in kids})
        cw = max(h.cw(k.name) for k in kids)
        for k in kids:
            c = cols.index(k.x)
            r = rows.index(k.y)
            h.set(k.name, 8 + c * (cw + 8), top + r * 20, cw, 18)
            h.check_left(k.name)
        gw = 8 + len(cols) * (cw + 8)
        gh = top + len(rows) * 20 - 2 + 6
        h.set(gname, F, ys, gw, gh)
        hg = max(hg, gh)
    p.y = ys + max(H, hg, yt + 66 - ys) + 4
    p.field("labelAmount", "textProcFee", F, 72, after=[("labelTaxEst", None, h.lw("labelTaxEst")), ("textTaxAmt", None, 72)])
    h.text_align("labelTaxEst", "MiddleRight")
    h.set("labelTaxEst", h.n("textTaxAmt").nx - 6 - h.lw("labelTaxEst"))
    h.set("textTaxAmt", xb, None, wb)
    h.set("labelTaxEst", xb - 6 - h.lw("labelTaxEst"))
    p.end()

    # ---- Under panel1: the left column's other fields (on the window) ---------------------------------------------------------------------
    root = h.box(".", None, None, None, None)
    root.y = h.n("panel1").nh + 8
    root.bottom_used = root.y
    root.row(("checkHideGraphics", F, None))
    root.field("labelDx", "comboDx", F, PW - 8 - F)
    root.field("labelPriority", "comboPriority", F, PW - 8 - F)
    root.skip(4)
    gp = h.box("groupProsth", 8, root.y, PW - 16)
    fg = F - 8
    l3 = fg - 6 - 8
    h.set("label3", 8, gp.top, l3, 2 * 17 + 2)
    h.text_align("label3", "TopRight")
    h.set("listProsth", fg, gp.top, gp.right - fg, 46)
    gp.y = gp.top + 46 + 4
    gp.bottom_used = gp.y
    gp.field("label4", "textDateOriginalProsth", fg, 80, after=[("checkIsDateProsthEst", None, None)])
    gp.end()
    root.y = gp.node.ny + gp.node.nh + 8
    lc = F - 6 - 8
    h.set("labelClaimNote", 8, root.y, lc, 2 * 17 + 2)
    h.text_align("labelClaimNote", "TopRight")
    h.set("textClaimNote", F, root.y, PW - 8 - F, 46)
    left_bottom = root.y + 46

    # ---- The right column ------------------------------------------------------------------------------------------------------------
    xr = PW + 8
    FR = xr + h.lw("label14") + 6
    right = 976
    r = h.box(".", None, None, None, None)
    r.y = 8
    r.field("label14", "comboProcStatus", FR, 140, after=[("butSetComplete", None, None), ("labelSetComplete", None, h.lw("labelSetComplete"))])
    h.text_align("labelSetComplete", "MiddleLeft")
    h.set("comboClinic", FR - 37, r.y, 37 + 180, H)
    r.y += H + 4
    r.field("label9", "comboProv", FR, 180, after=[("butPickProv", None, H)])
    r.field("label13", "textReferral", FR, 180, after=[("butReferral", None, H)])
    wl = h.n("butLock").nw
    for name in ("butLock", "butInvalidate"):
        h.set(name, right - wl, h.n("textReferral").ny, wl, H)
    yi = r.y
    h.set("labelIncomplete", FR, yi, h.lw("labelIncomplete"), H)
    h.text_align("labelIncomplete", "MiddleLeft")
    llk = h.lw("labelLocked")
    h.set("labelLocked", right - llk, yi, llk, H)
    h.text_align("labelLocked", "MiddleRight")
    r.y += H + 4
    r.field("label16", "textUser", FR, 120, after=[("butChangeUser", None, H), ("butEditAutoNote", None, None), ("buttonUseAutoNote", None, None)])
    wa = h.n("butAppend").nw
    h.set("butAppend", right - wa, h.n("textUser").ny, wa, H)
    yn = r.y
    l7 = h.lw("label7")
    h.set("label7", FR - 6 - l7, yn, l7, H)
    h.text_align("label7", "MiddleRight")
    h.set("textNotes", FR, yn, right - FR, 164)
    ws = h.n("butSearch").nw
    h.set("butSearch", FR - 6 - ws, yn + H + 4, ws, H)
    ysig = yn + 164 + 4
    l15 = h.lw("label15")
    h.set("label15", FR - 6 - max(l15, 72), ysig, max(l15, 72), 2 * 17 + 2)
    h.text_align("label15", "TopRight")
    h.set("signatureBoxWrapper", FR, ysig, 362, 79)
    h.set("labelPermAlert", FR, ysig + 79 + 4, right - FR, 27)
    right_bottom = ysig + 79

    # ---- The tab control and the bottom bar ----------------------------------------------------------------------------------------------
    tc = h.n("tabControl")
    yt = max(left_bottom, right_bottom) + 8
    h.set("tabControl", tc.nx, yt, tc.nw, tc.nh)
    # The Financial tab's grids keep 8 px from the page's edges (the planner keeps a grid's width).
    pw = h.n("tabPageFinancial").nw
    h.set("gridIns", 8, None, pw - 16)
    ga = h.n("gridAdj")
    h.set("gridAdj", None, None, pw - 8 - ga.nx)
    ybar = yt + tc.nh + 12
    h.set("butDelete", 8, ybar, None, 24)
    h.set("butSave", right - h.n("butSave").nw, ybar, None, 24)
    h.set("butMaterials", h.n("butSave").nx - 8 - h.n("butMaterials").nw, ybar, None, 24)
    h.set("butEditAnyway", h.n("butMaterials").nx - 8 - h.n("butEditAnyway").nw, ybar, None, 24)
    lcw = h.n("butEditAnyway").nx - 16 - (8 + h.n("butDelete").nw + 8)
    h.set("labelClaim", 8 + h.n("butDelete").nw + 8, ybar + 24 - 32, lcw, 32)
    h.set(".", None, None, 984, ybar + 24 + 8)
