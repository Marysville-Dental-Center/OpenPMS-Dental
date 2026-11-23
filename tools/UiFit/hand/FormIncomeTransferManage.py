"""FormIncomeTransferManage on the grid (UI-12 slice 4, layout script on the planner's tree). The description at the top left is seven
lines at its 395 px (its last line, "support provider.", was cut at 92 px): the note as tall as its lines (108), the two grids 8 px under
it (their bottoms kept).
Fix round: the transfer group ends on the grids' right edge (its note narrower, wrapping; the planner widened the group and the window for
it), the window 12 px after the grids as at the left (main's 1073), Transfer on the right grid's x."""
import grid_tools as gt

LINE = 15


def layout(h):
    h.set("label1", None, 8, None, 108)
    top = 8 + 108 + 8
    for name in ("gridTransfers", "gridImbalances"):
        g = h.n(name)
        h.set(name, None, top, None, g.ny + g.nh - top)
    gi = h.n("gridImbalances")
    R = gi.nx + gi.nw
    grp = h.n("groupBoxOD1")
    w = R - grp.nx
    lab = h.n("label6")
    lw = w - 8 - lab.nx
    lh = gt.measured_lines(lab.text, lab.text_w, lw) * LINE + 1
    h.set("label6", None, None, lw, lh)
    h.set("groupBoxOD1", None, None, w, lab.ny + max(lh, 24) + 8)
    h.set(".", None, None, R + h.n("gridTransfers").nx, None)
    h.set("butTransfer", gi.nx, None, None, None)
