"""FormSheetDefs on the grid (UI-12 slice 6, layout script on the planner's tree). The two grids stood 6 px apart and ended 5 px short of the
right edge, Tools under neither grid's edge: the grids one width 8 px apart to the right edge, Tools and Copy at their grids' left edges,
Duplicate on the right edge with New 6 px before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    x1 = h.n("grid1").nx
    gw = (R - x1 - 8) // 2
    h.set("grid1", x1, None, gw, None)
    x2 = x1 + gw + 8
    h.set("grid2", x2, None, R - x2, None)
    h.set("butTools", x1, None)
    h.set("butCopy", x2, None)
    h.set("butCopy2", R - 89, None)
    h.set("butNew", R - 89 - 6 - 80, None)
