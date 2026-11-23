"""FormSupplyCount on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 12 px margin and the Reason row
floated mid-window (356 to 606): the scan row from the padding, the grid and the count line from the padding to the right edge, the Reason
row on the right edge."""
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
    h.set("gridMain", 8, None, R - 8, None)
    h.set("labelCount", 8, None, R - 8, None)
    h.set("textReason", R - 192, None)
    h.set("labelReason", R - 192 - 6 - 52, None)
