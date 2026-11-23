"""FormEcwDiagAdv on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 12 px margins with Run 12 px after
the boxes: the content from the padding, Run on the right edge with the boxes and the result grid 8 px before it, the grids down to the
bottom padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    xr = R - 75
    h.set("butRunQ", xr, h.n("textQuery").ny)
    for name in ("textConnString", "listQuery", "gridTables"):
        n = h.n(name)
        h.set(name, 8, None, n.nw + n.nx - 8, None)
    for name in ("textConnString", "textQuery", "gridMain"):
        n = h.n(name)
        h.set(name, None, None, xr - 8 - n.nx, None)
    B = e.bottom_edge(h)
    e.bottom_to(h, "gridTables", B)
    e.bottom_to(h, "gridMain", B)
