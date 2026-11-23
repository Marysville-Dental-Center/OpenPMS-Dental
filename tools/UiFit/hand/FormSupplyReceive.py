"""FormSupplyReceive on the grid (UI-12 slice 6, layout script on the planner's tree). The scan row started at 18 and the grid kept main's 12
px margin, 4 px short of the right edge and 12 px over the bottom row: the scan row from the padding, its note to the right edge, the grid
from the padding to the right edge down to 8 px over the bottom row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    lw = h.lw("labelScan")
    h.set("labelScan", 8, 12, lw, None)
    h.set("textScan", 8 + lw + 6, 12)
    nx = 8 + lw + 6 + 172 + 6
    h.set("labelScanNote", nx, 8, R - nx, None)
    g = h.n("gridMain")
    h.set("gridMain", 8, None, R - 8, h.n("butClose").ny - 8 - g.ny)
