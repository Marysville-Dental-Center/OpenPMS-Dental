"""FormCentralManager on the grid (UI-12 slice 12, layout script on the planner's tree). The planner left the two grids at two tops (144
and 120), the patient search column at a third (132), the Conn Groups row, Refresh Statuses and the version line on three left edges, and
grew the window 36 px: back to main's 1834 x 953. Under the menu (docked, 24 px): the Conn Groups row from the padding with Refresh
Statuses under its combo box ending on its edge and the version line (its text set by the code) under them; the Filter Connections group
beside them at the band gap, its rows as the planner laid them out. Under that block, at the sibling gap: the connections grid from the
padding, the patients grid beside it (main's widths, 703 and the rest), both down to the bottom padding, and the patient search column on
the right padding at their top: the Search by group as the planner laid it out, the Limit check box under it, then Search on the column's
right edge with the fetching note (hidden in the designer) right-aligned before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 1834, 953)
    R = 1834 - s.PAD
    B = 953 - s.PAD
    y0 = 24 + s.PAD  # under the menu
    fx = s.col_x(h, ["label1"])
    s.row(h, "label1", "comboConnectionGroups", y0, fx)
    c = h.n("comboConnectionGroups")
    cr = c.nx + c.nw
    r = h.n("butRefreshStatuses")
    h.set("butRefreshStatuses", cr - r.nw, y0 + h.H + s.ROWGAP, None, h.H)
    h.set("labelVersion", s.PAD, y0 + 2 * (h.H + s.ROWGAP), cr - s.PAD, h.H)
    h.text_align("labelVersion", "MiddleLeft")
    h.set("groupBox1", cr + 16, y0)
    g1 = h.n("groupBox1")
    top = max(g1.ny + g1.nh, y0 + 3 * h.H + 2 * s.ROWGAP) + s.GAP
    g2 = h.n("groupBox2")
    cx = R - g2.nw
    h.set("gridConns", s.PAD, top, 703, B - top)
    px = s.PAD + 703 + s.GAP
    h.set("gridPats", px, top, cx - s.GAP - px, B - top)
    h.set("groupBox2", cx, top)
    y = top + g2.nh + s.GAP
    h.check_left("checkLimit")
    h.set("checkLimit", cx + s.PAD, y, h.cw("checkLimit"), h.CH)
    y += h.CH + s.ROWGAP
    b = h.n("butSearchPats")
    h.set("butSearchPats", R - b.nw, y, None, h.H)
    lf = h.n("labelFetch")
    h.set("labelFetch", R - b.nw - s.LGAP - lf.nw, y, None, h.H)
    h.text_align("labelFetch", "MiddleRight")
