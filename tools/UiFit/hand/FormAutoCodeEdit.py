"""FormAutoCodeEdit on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner moved Save off the grid's right
edge (which it matched on main) to 8 px from the window's edge, and left the two-line note above and left of it, aligned with nothing:
Save right-aligned on the grid's right edge, the note (two lines, as tall as its lines) ending 6 px before Save on the Save row, the grid
down to 8 px over the row (the note's band under it is gone)."""
import grid_tools as g

LINE = 15


def layout(h):
    grid = h.n("gridMain")
    s = h.n("butSave")
    xs = grid.nx + grid.nw - s.nw
    h.set("butSave", xs, None, None, None)
    lab = h.n("label2")
    lh = g.measured_lines(lab.text, lab.text_w, lab.nw) * LINE + 1
    h.set("label2", xs - 6 - lab.nw, s.ny + (s.nh - lh) // 2, None, lh)
    h.text_align("label2", "MiddleRight")
    h.set("gridMain", None, None, None, s.ny - 8 - grid.ny)
