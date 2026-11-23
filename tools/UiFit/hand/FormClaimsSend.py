"""FormClaimsSend (Send Claims) on the grid (UI-12 slice 2, mode hand: the planner spread the window to 4000 px around its splitter). As
main arranges it, on the grid: one filter row 8 px under the toolbar, each filter a label right-aligned 6 px before its box, 16 px from a
box (or Next Unsent, 6 px after the clinic box) to the next label; the claims grid 8 px under the row from the content edge to the right
content edge down to the splitter (the splitter's drag sets the grid's height to its top and the history panel's top to its bottom:
FormClaimsSend.cs, so they stay touching it, as on main); in the history panel one row 8 px under the splitter (the date range with its From box under the clinic filter's box, Type and its box,
the history toolbar's gray frame centred on the row), the history grid 8 px under the row at the content edge (8), to the right content
edge and 8 px above the window bottom."""


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
    # The history panel touches the splitter and ends 1 px above the window bottom: the splitter's drag sets its top to the splitter's
    # bottom and its height to ClientSize.Height - top - 1 (FormClaimsSend.cs panelSplitter_MouseMove), so the 8 px margins are inside it.
    ptop = sp.ny + sp.nh
    ph_h = root.h - ptop - 1
    h.set("panelHistory", None, ptop, None, ph_h)
    # The panel's one row, laid out from the panel's top (review of 2026-11-23): the date range's boxes 8 px under the splitter (the range
    # draws its boxes 2 px into its bounds, so its top is 6), Type and its combo box on the boxes' top and height, the gray box (the history
    # toolbar's frame: the 25 px toolbar + its 1 px border, so 27 px tall) centred on the row (2 px above and below the boxes).
    yb = 8
    h.set("dateRangePicker", h.n("comboClinic").nx - 65, yb - 2)  # its From box (65 px in) under the clinic filter box
    lw = h.lw("label4")
    h.set("label4", h.n("comboHistoryType").nx - 6 - lw, yb, lw, H)
    h.text_align("label4", "MiddleRight")
    h.set("comboHistoryType", None, yb, None, H)
    gb = h.n("panel1")
    gy = yb - (gb.nh - H) // 2
    h.set("panel1", R - gb.nw, gy, None, None)  # on the grids' right edge
    row_bottom = max(yb + H, gy + gb.nh)
    # The history grid 8 px under the row's lowest box (the gray box), to the right content edge, ending 8 px above the window bottom.
    gy2 = row_bottom + 8
    h.set("gridHistory", 8, gy2, R - 8, ph_h - 7 - gy2)
