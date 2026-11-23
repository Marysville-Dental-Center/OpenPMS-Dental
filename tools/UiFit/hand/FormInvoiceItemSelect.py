"""FormInvoiceItemSelect on the grid (UI-12 slice 6, layout script on the planner's tree). The grid and the note kept main's 12 px margin and
Exclude $0 Fee wrapped in the 108 px Filter group: the note and the grid from the padding, the Filter group 120 px on the right edge with
its check box on one line, All and None on its x over OK (they are anchored to the bottom), the grid 8 px before them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    gx = R - 120
    h.set("label1", 8, None)
    h.check_left("checkIsFilteringZeroAmount")
    h.set("checkIsFilteringZeroAmount", 8, 23, h.cw("checkIsFilteringZeroAmount"), 20)
    gb = h.set("groupBox1", gx, None, 120, 23 + 20 + 8)
    y = h.n("butOK").ny - 8 - 26
    h.set("butNone", gx, y)
    h.set("butAll", gx, y - 4 - 26)
    h.set("_gridMain", 8, None, gx - 8 - 8, None)
