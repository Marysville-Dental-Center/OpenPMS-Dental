"""FormMultiVisitGroup on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 13 px from the left, 12 from the right
and 15 px over Ungroup: 12 px from both sides and over the bottom bar. Fix round (the lead's review of 2026-11-23): Ungroup on the grid's
right edge (it stood 81 px short of it), the bottom bar's button."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "gridGroupedProcs", x=12, r=h.tree.nw - 12)
    s.grid_bottom(h, "gridGroupedProcs")
    g = h.n("gridGroupedProcs")
    u = h.n("butUngroup")
    s.move(h, "butUngroup", x=g.nx + g.nw - u.nw)
