"""FormMaintHistory on the grid (UI-12 slice 6, layout script on the planner's tree). The grids kept main's 12 px margin and ended 4 px short
of Close, the filter row stood in the middle (479): the item line at the padding, the filter row on the right edge (Done from, to, Refresh),
the grids from the padding to the right edge and to 8 px before the two buttons, which stand on the right edge; the lower grid down to 8 px
over the bottom row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    x = R - 75
    h.set("butRefresh", x, None)
    x -= 6 + 88
    h.set("textDateTo", x, None)
    x -= 6 + 24
    h.set("labelDateTo", x, None)
    x -= 16 + 88
    h.set("textDateFrom", x, None)
    h.set("labelDateFrom", x - 6 - 68, None)
    h.set("labelItem", 8, None)
    h.set("gridMain", 8, None, R - 8, None)
    bx = R - 80
    h.set("butOpenAttachment", bx, None)
    h.set("butCorrect", bx, None)
    g = h.n("gridValues")
    h.set("gridValues", 8, None, bx - 8 - 8, h.n("butClose").ny - 8 - g.ny)
