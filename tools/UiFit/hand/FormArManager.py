"""FormArManager (Accounts Receivable Manager) on the grid (UI-12 slice 1 fix round 9, layout script of grid_hand.py, on the planner's tree).
Two touches on each of the three pages (Unsent, Sent, Excluded), which share their structure:
- the filter row: each filter a label (right-aligned, as wide as its text) 6 px before its box, one gap between filters (16 px from a box's
  end to the next label's text: a right-aligned label's text, as drawn, starts its measured width less 6 px into it and ends 3 px before
  its right side, so label to box is 9 px of ink to box on every filter), Save As Default after the last box, ending on the grid's right
  edge (the planner kept main's x for the boxes and labels, so the gaps were 27 to 41 px);
- the bottom bar's right-hand buttons (Run Aging, Print, Export) 6 px apart, the last on the grid's right edge (the window grew 20 px and
  they stayed). The totals boxes over the bar are placed by FormArManager.cs under the grid's columns at run time."""

GAP = 16  # from a box's end to the next label's text


def page(h, grid, save, bar):
    g = h.n(grid)
    right = g.nx + g.nw
    sv = h.n(save)
    y = sv.ny
    # the filter row: the labels and fields of the page on Save As Default's row, left to right
    kids = [k for k in g.parent.children if k.visible and k.dock in ("None", "") and k.ny == y and k is not sv]
    kids.sort(key=lambda k: k.nx)
    labels = [k for k in kids if k.kind == "label"]
    fields = [k for k in kids if k.kind == "field"]
    assert len(labels) == len(fields), (grid, [k.name for k in kids])
    x = None
    for lb, fd in zip(labels, fields):
        lw = h.lw(lb.name)
        if x is None:
            rl = fd.nx - 6  # the first filter keeps its place
        else:
            rl = x + GAP + lb.text_w - 6
        h.set(lb.name, rl - lw, y, lw, h.H)
        h.text_align(lb.name, "MiddleRight")
        h.set(fd.name, rl + 6, y, fd.nw, h.H)
        x = rl + 6 + fd.nw
    h.set(save, right - sv.nw, y)
    # the bottom bar's right-hand buttons, right to left from the grid's edge
    xr = right
    for name in bar:
        b = h.n(name)
        h.set(name, xr - b.nw, None)
        xr -= b.nw + 6


def layout(h):
    page(h, "gridUnsent", "butUnsentSaveDefault", ("butRunAging", "butUnsentPrint", "butUnsentExport"))
    page(h, "gridSent", "butSentSaveDefaults", ("butSentPrint", "butSentExport"))
    page(h, "gridExcluded", "butExcludedSaveDefault", ("butExcludedRunAging", "butExcludedPrint", "butExcludedExport"))
