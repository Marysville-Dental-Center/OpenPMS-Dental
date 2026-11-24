"""FormQuery (the user query) on the grid (UI-12 slice 14, layout script). The query and its results are the two panes of a split the user
drags (Load sets the split at 140 px); the print preview of a report (IsReport) is placed by Load over the window above the bar. Main: the
split container 1 px from the window's edges, the query box 1 px into its pane, the button column 3 px into a panel beside it, the results'
Title row 2 px under the splitter, the bar 6 px from the bottom with no gap over it, the preview 39 px short of the bottom at 0, 0. Now:
- the split container and the report's preview on the padding (Load places the preview with UiGrid.PAD and the bar's room: the bar's
  24 px and a section gap), the bar's four buttons on the bottom padding ending on the right padding 8 px apart, the report's page
  buttons (panelZoom) on the bar before Query View;
- the query pane: the query box from the pane's top left, the buttons 8 px beside it: Favorites, Add To Favorites, Copy and Paste
  (one width, 6 px apart), Submit Query a row each, Run on report server (shown by Load when a report server is set up) a check box row
  under them, the Format group 8 px beside the column, Export to Excel (hidden) under it. The pane is at least as tall as that column
  (Panel1MinSize, a line in the designer), so no button is cut at any split; the designer's split is Load's 140;
- the results pane: the Title row at its top, the grid's caption a row gap under it, the grid 2 px under the caption to the pane's bottom,
  the pane at least as tall as the title, the caption and four grid rows (Panel2MinSize)."""

PAD = 8
GAP = 4
SPLIT = 140


def layout(h):
    H = h.H
    W, HH = h.tree.nw, h.tree.nh
    # the bar
    ybar = HH - PAD - 24
    x = W - PAD
    for name in ("butPrint", "butExport", "butPrintPreview", "butQView"):
        n = h.n(name)
        x -= n.nw
        h.set(name, x, ybar, None, 24)
        x -= 8
    h.set("panelZoom", x - 144, ybar, 144, 24)
    for name, xx in (("butBack", 6), ("butFwd", 120)):
        h.set(name, xx, 0, 18, 24)
    h.set("labelTotPages", 30, 0, 86, 24)
    h.text_align("labelTotPages", "MiddleLeft")
    hs = ybar - 12 - PAD
    ws = W - 2 * PAD
    h.set("splitContainer", PAD, PAD, ws, hs)
    h.set("printPreviewControl2", PAD, PAD, ws, hs, why="where Load places it: the padding, the bar's 24 px and a section gap")
    h.splitter("splitContainer", SPLIT, why="Load's split (140)")
    h.set("splitterPanel1", 0, 0, ws, SPLIT)
    h.set("splitterPanel2", 0, SPLIT + 4, ws, hs - SPLIT - 4)
    # the query pane
    h.set("textQuery", 0, 0, 557, SPLIT)
    h.set("butFavorite", 0, 0, 140, H)
    h.set("butAdd", 0, H + GAP, 140, H)
    y = 2 * (H + GAP)
    h.set("butCopy", 0, y, 67, H)
    h.set("butPaste", 73, y, 67, H)
    y += H + GAP
    h.set("butSubmit", 0, y, 104, H)
    y += H + GAP
    h.check_left("checkReportServer")
    h.set("checkReportServer", 0, y, 140, 18)
    col = y + 18
    xg = 140 + 8
    h.set("groupBox1", xg, 0, 132, 18 + 4 + 18 + 4 + 18 + 8)
    h.set("radioHuman", 8, 22, 116, 18)
    h.set("radioRaw", 8, 44, 116, 18)
    h.set("butExportExcel", xg, 70 + GAP, 80, H)
    h.set("panel1", 557 + 8, 0, xg + 132, col)
    # the results pane
    lw = h.lw("label1")
    h.text_align("label1", "MiddleRight")
    h.set("label1", 0, 0, lw, H)
    h.set("textTitle", lw + 6, 0, 220, H)
    h.set("labelGridCaption", 0, H + GAP, ws, 19)
    yg = H + GAP + 19 + 2
    h.set("grid2", 0, yg, ws, hs - SPLIT - 4 - yg)
