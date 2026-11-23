"""FormInsPlanSelectFam on the grid (UI-12 slice 6, layout script on the planner's tree). The check box and the grid stood at 22 and the
relationship list ended 51 px short of OK, 34 px over the bottom row: the check box and the grid from the padding, the list (and its caption
over it) on the right edge 8 px after the grid, both down to 8 px over the bottom row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    yb = h.n("butOK").ny
    lx = R - 180
    h.set("checkShowPlansNotInUse", 8, None)
    g = h.n("gridMain")
    h.set("gridMain", 8, None, lx - 8 - 8, yb - 8 - g.ny)
    h.set("listRelat", lx, None, 180, yb - 8 - h.n("listRelat").ny)
    h.set("labelRelat", lx, None, 180, None)
