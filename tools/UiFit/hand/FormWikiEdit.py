"""FormWikiEdit on the grid (UI-12 slice 14, layout script). Its code (ResizeControls, on SizeChanged) lays the markup box and the preview
out at run time; main put them edge to edge, 52 px from the top, the halves 2 px apart. The code now takes the grid's numbers (UiGrid.PAD
under the two toolbars and around the halves, UiGrid.SIBLING_GAP between them), and the designer has them where the code puts them at its
size: the planner refused the window while the designer and the code differed."""

PAD = 8
GAP = 8
TOOLBARS = 50


def layout(h):
    W, HH = h.tree.nw, h.tree.nh
    top = TOOLBARS + PAD
    half = (W - 2 * PAD - GAP) // 2
    h.set("textContent", PAD, top, half, HH - PAD - top)
    x = PAD + half + GAP
    h.set("webViewWiki", x, top, W - PAD - x, HH - PAD - top)
