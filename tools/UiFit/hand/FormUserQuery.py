"""FormUserQuery on the grid (UI-12 slice 3, mode hand: the planner spread the query panel to 1551 px). Main's arrangement on the grid's
numbers: the split container at the padding down to 8 px over the bottom bar (its splitter where main has it). The upper panel: the query box
from the panel's left to 8 px before the button panel, which stands at the right edge: Favorites, Add To Favorites, Copy and Paste (one row,
one width), Submit Query in one 140 px column 4 px apart (Run on report server, shown by the code, under them); the Format group 8 px after
the column (Human-readable and Raw a list at the content edge; Column Alignment with its box to the content edge on the first row, Show Row
Numbers and Enable word wrap under it on the label's x); Print Orientation 8 px under the group, its caption on the row of Portrait and
Landscape (the panel stays within the 140 px the code gives the upper panel: FormQuery_Load sets SplitterDistance to Scale(140)). The lower
panel: Title with its box (Row Count, shown by the code, 16 px after it), the results grid 8 px under them to the panel's edges. Every child
of the two panels stands 1 px inside them (FRAME), so the panels' 1 px frame (SplitterPanel.OnPaint: left and right, the upper panel's top,
the lower panel's bottom) runs unbroken. Print Preview, Export .csv, Export .txt and Print at the bottom right, 6 px apart."""

PAD = 8
GAP = 8
FRAME = 1  # the split panels' frame line: their children stand inside it


def layout(h):
    root = h.tree
    W, HH = root.w, root.h
    R = W - PAD
    H = h.H
    CH = h.CH
    by = HH - PAD - 24
    sc = h.n("splitContainer")
    sw = R - PAD
    sh = by - GAP - PAD
    h.set("splitContainer", PAD, PAD, sw, sh)
    p2 = h.n("splitterPanel2")
    p2h = sh - p2.ny
    p1h = h.n("splitterPanel1").nh
    # The program's SplitContainer keeps its panels' sizes in the designer: they follow the container.
    h.set("splitterPanel1", None, None, sw, p1h)
    h.set("splitterPanel2", None, None, sw, p2h)
    # The button panel.
    pn = h.n("panel1")
    pw = pn.nw - FRAME  # main's 591 less the frame line on the right
    col = 140
    y = 0
    for b in ("butFavorite", "butAdd"):
        h.set(b, 0, y, col, H)
        y += H + 4
    half = (col - 6) // 2
    h.set("butCopy", 0, y, half, H)
    h.set("butPaste", col - half, y, half, H)
    y += H + 4
    h.set("butSubmit", 0, y, col, H)
    y += H + 4
    h.check_left("checkReportServer")
    h.set("checkReportServer", 0, y, h.cw("checkReportServer"), CH)
    colb = y + CH
    gx = col + GAP
    gw = pw - gx
    g = h.n("groupBox1")
    top = (g.title_bottom or 18) + 4
    C = PAD + max(h.cw("radioHuman"), h.cw("radioRaw")) + 16
    h.check_left("radioHuman")
    h.set("radioHuman", PAD, top + (H - CH) // 2, h.cw("radioHuman"), CH)
    lw = h.lw("label2")
    h.set("label2", C, top, lw, H)
    h.text_align("label2", "MiddleLeft")
    h.set("comboAlignment", C + lw + 6, top, gw - PAD - (C + lw + 6), H)
    y2 = top + (H - CH) // 2 + CH + 4  # the list pitch under Human-readable
    h.check_left("radioRaw")
    h.set("radioRaw", PAD, y2, h.cw("radioRaw"), CH)
    for i, c in enumerate(("checkNumberedRows", "checkWordWrap")):
        h.check_left(c)
        h.set(c, C, y2 + i * (CH + 4), h.cw(c), CH)
    gh = y2 + 2 * CH + 4 + PAD
    h.set("groupBox1", gx, 0, gw, gh)
    oy = gh + GAP
    lpo = h.lw("labelPrintOrientation")
    h.set("labelPrintOrientation", gx, oy, lpo, CH)
    h.text_align("labelPrintOrientation", "MiddleLeft")
    rx = gx + lpo + 6
    h.check_left("radioPortrait")
    h.set("radioPortrait", rx, oy, h.cw("radioPortrait"), CH)
    h.check_left("radioLandscape")
    h.set("radioLandscape", rx + h.cw("radioPortrait") + 16, oy, h.cw("radioLandscape"), CH)
    ph = max(colb, oy + CH)
    assert FRAME + ph <= 140 - PAD, "the button panel must fit the 140 px upper panel FormQuery_Load sets"
    px = sw - FRAME - pw
    h.set("panel1", px, FRAME, pw, ph)
    h.set("textQuery", FRAME, FRAME, px - GAP - FRAME, p1h - FRAME)
    # The lower panel.
    h.set("label1", FRAME, 0, h.lw("label1"), H)
    h.text_align("label1", "MiddleLeft")
    tx = FRAME + h.lw("label1") + 6
    h.set("textTitle", tx, 0, h.n("textTitle").nw, H)
    h.set("labelRowCount", tx + h.n("textTitle").nw + 16, 0, h.n("labelRowCount").nw, H)
    h.set("_gridResults", FRAME, H + GAP, sw - 2 * FRAME, p2h - (H + GAP) - FRAME)
    x = R
    for b in ("butPrint", "butExportTxt", "butExportCsv", "butPrintPreview"):
        x -= h.n(b).nw
        h.set(b, x, by, None, 24)
        x -= 6
    h.watch("panel1", "groupBox1")
