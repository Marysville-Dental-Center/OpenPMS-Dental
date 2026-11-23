"""FormClaimsSend (Send Claims) on the grid (UI-12 slice 2, mode hand: the planner spread the window to 4000 px around its splitter). As
main arranges it, on the grid: one filter row 8 px under the toolbar, each filter a label right-aligned 6 px before its box, 16 px from a
box (or Next Unsent, 6 px after the clinic box) to the next label; the claims grid 8 px under the row from the content edge to the right
content edge down to the splitter (the splitter's drag sets the grid's height to its top and the history panel's top to its bottom:
FormClaimsSend.cs, so they stay touching it, as on main); in the history panel the date range with its From box under the clinic filter's box and the history grid at the content edge (8),
the grid to the right content edge."""


def layout(h):
    H = h.H
    root = h.tree
    R = root.w - 8
    y0 = h.n("ToolBarMain").ny + h.n("ToolBarMain").nh + 8
    x = 8
    for lab, fld, w, after in (("labelClinic", "comboClinic", 160, "butNextUnsent"), ("labelCustomTracking", "comboCustomTracking", 160, None),
            ("labelCarrier", "textCarrier", 100, None), ("labelProv", "textProv", 100, None), ("labelProc", "textProc", 100, None)):
        lw = h.lw(lab)
        h.set(lab, x, y0, lw, H)
        h.text_align(lab, "MiddleRight")
        h.set(fld, x + lw + 6, y0, w, H)
        x += lw + 6 + w
        if after:
            b = h.n(after)
            h.set(after, x + 6, y0, b.nw, H)
            x += 6 + b.nw
        x += 16
    sp = h.n("panelSplitter")
    yg = y0 + H + 8
    h.set("gridMain", 8, yg, R - 8, sp.ny - yg)
    ph = h.n("panelHistory")
    h.set("panelHistory", None, sp.ny + sp.nh, None, root.h - (sp.ny + sp.nh) - 1)
    h.set("dateRangePicker", h.n("comboClinic").nx - 65, None)  # its From box (65 px in) under the clinic filter box
    g = h.n("gridHistory")
    h.set("gridHistory", 8, None, R - 8, None)
