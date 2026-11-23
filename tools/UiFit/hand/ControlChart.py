"""ControlChart (the Chart module) on the grid where its designer places the controls (UI-12 round 3, layout script of grid_hand.py). The module
is laid out at run time by the Chart layout sheet (SheetFieldsAvailable: ChartModuleTabs 524 x 259, the progress notes, the tooth chart and the
grids are moved and sized from the sheet), so the tab pages keep their size: the Enter Treatment page is 520 x 236 and holds four columns.
Inside it the grid's rules with the numbers that fit the page: 2 px from the page's edges, 6 px between columns, captions 2 px over their
lists, the lists' tops on one line and the bottom row (the date, Priority, Treatment Plans) on one line, the procedure buttons a 3 x 3 grid as
wide as the surface box above them, the Entry Status radio buttons one list at a 19 px pitch (the page's height leaves no more). The button
rows of the Planned, Show and Draw pages: one height, 6 px apart, ending at the edge of what they stand under."""


def layout(h):
    H = h.H
    h.watch("tabEnterTx", "groupBox2", "tabPlanned", "tabShow", "tabDraw")
    # ---- Enter Treatment --------------------------------------------------------------------------------------------------------------
    xa, wa = 2, 96
    xb, wb = xa + wa + 6, 122
    xc, wc = xb + wb + 6, 112
    xd = xc + wc + 6
    wd = 518 - xd
    y0, ycap, ylist, ybot = 2, 29, 48, 211
    # A: the surface box, the procedure buttons under it, the entry status.
    h.set("textSurf", xa, y0, wa, H)
    bw, bh, gap = 30, 19, 3
    cols = [xa, xa + bw + gap, xa + 2 * (bw + gap)]
    rows = [ycap, ycap + bh + gap, ycap + 2 * (bh + gap)]
    for name, c, r in (("butBF", 1, 0), ("butV", 2, 0), ("butM", 0, 1), ("butOI", 1, 1), ("butD", 2, 1), ("butL", 1, 2)):
        h.set(name, cols[c], rows[r], bw, bh, why="the procedure buttons: a 3 x 3 grid as wide as the surface box, 3 px apart")
    yg = rows[2] + bh + 4
    h.set("groupBox2", xa, yg, wa, 234 - yg)
    yr = h.n("groupBox2").title_bottom + 2
    for i, name in enumerate(("radioEntryTP", "radioEntryC", "radioEntryEC", "radioEntryEO", "radioEntryR", "radioEntryCn")):
        h.set(name, 4, yr + 19 * i, h.cw(name), 18, why="the entry status: one list, 19 px apart (the page's height)")
        h.check_left(name)
    # B: today and the date, the diagnosis list, prognosis and priority.
    wt = h.cw("checkToday")
    h.set("checkToday", xb, y0 + (H - h.CH) // 2, wt, h.CH)
    h.check_left("checkToday")
    h.set("textDate", xb + wt + 6, y0, wb - wt - 6, H)
    h.set("labelDx", xb, ycap, wb, 17)
    h.set("label6", xb, ybot - 2 - 17, wb, 17)
    h.set("comboPriority", xb, ybot, wb, H)
    yprog = ybot - 2 - 17 - 4 - H
    h.set("comboPrognosis", xb, yprog, wb, H)
    h.set("labelPrognosis", xb, yprog - 2 - 17, wb, 17)
    h.set("listDx", xb, ylist, wb, yprog - 2 - 17 - 4 - ylist)
    # C: the procedure list button, the button categories, Treatment Plans.
    h.set("butAddProc", xc, y0, wc, H)
    h.set("label3", xc, ycap, wc, 17)
    ych = ybot + (H - h.CH) // 2
    h.set("checkTreatPlans", xc, ych, h.cw("checkTreatPlans"), h.CH)
    h.check_left("checkTreatPlans")
    h.set("listButtonCats", xc, ylist, wc, ych - 4 - ylist)
    # D: the code box and OK, the single click buttons (the quick buttons panel takes the list's place at run time).
    lor = h.lw("label14")
    wok = 40
    h.set("label14", xd, y0, lor, H)
    h.text_align("label14", "MiddleRight")
    h.set("textProcCode", xd + lor + 6, y0, wd - lor - 6 - 6 - wok, H)
    h.set("butOK", xd + wd - wok, y0, wok, H)
    h.set("label13", xd, ycap, wd, 17)
    h.set("listViewButtons", xd, ylist, wd, 234 - ylist)
    h.set("panelQuickButtons", xd, ylist, wd, 234 - ylist)
    for lb in ("labelDx", "labelPrognosis", "label6", "label3", "label13"):
        h.text_align(lb, "BottomLeft")

    # ---- Planned appointments: the button row ------------------------------------------------------------------------------------------
    x = 2
    for name in ("butNew", "butClear", "butPin", "butUp", "butDown"):
        h.set(name, x, 1, None, H)
        x += h.n(name).nw + 6
    for name in ("checkDone", "checkShowCompleted"):
        w = h.cw(name)
        h.set(name, x, 1 + (H - h.CH) // 2, w, h.CH)
        h.check_left(name)
        x += w + 6
    # ---- Show: the chart view buttons under their grid, ending at its right edge -------------------------------------------------------
    # UI-12 slice 2 (pilot follow-up): Show Auto Commlogs as wide as its box and text (142 px; 128 wrapped it onto a second line its 13 px
    # row cut), so the grid starts 14 px further right (6 px after the text) and keeps its right edge; Down 52 px wide to fit the row.
    h.set("gridChartViews", 317, 8, 177, 173)
    grid = h.n("gridChartViews")
    yb = grid.ny + grid.nh + 4
    r = grid.nx + grid.nw
    for name in ("butChartViewDown", "butChartViewUp", "butChartViewAdd"):
        w = 52 if name == "butChartViewDown" else h.n(name).nw
        h.set(name, r - w, yb, w, H)
        r -= w + 6
    # ---- Draw: Add and Delete under the text list, at its edges ------------------------------------------------------------------------
    lst = h.n("listBoxText")
    yb = lst.ny + lst.nh + 4
    h.set("butAddText", lst.nx, yb, None, H)
    h.set("butDeleteText", lst.nx + lst.nw - h.n("butDeleteText").nw, yb, None, H)
