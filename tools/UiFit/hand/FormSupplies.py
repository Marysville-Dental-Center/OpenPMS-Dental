"""FormSupplies on the grid (UI-12 slice 6, layout script on the planner's tree). The planner put Up under the grid at 556 and left Down on the
bottom bar (main stacks them), the order group over the print note with the note 2 px under it, Add Supply 4 px short of the right edge: the
grid from the padding to the right edge; Down and Print on the bottom bar with the print note 6 px after Print; Up over Down and the order
group beside it ending 12 px over the bar; the grid down to 8 px over them; Add Supply on the right edge. The code sets the window's height
from the screen (bottom-anchored rows)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    yb = B - 24
    h.set("butAdd", R - 93, None)
    h.set("butDown", 8, yb)
    h.set("butPrint", 8 + 67 + 8, yb)
    px = 8 + 67 + 8 + 80 + 6
    h.set("labelPrint", px, yb + (24 - 36) // 2, h.n("butOK").nx - 8 - px, None)
    gb = yb - 12
    g = h.n("groupCreateOrders")
    h.set("groupCreateOrders", 8 + 67 + 8, gb - g.nh)
    h.set("butUp", 8, gb - 24)
    h.set("gridMain", 8, None, R - 8, gb - g.nh - 8 - h.n("gridMain").ny)
