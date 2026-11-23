"""FormReminderRules on the grid (UI-12 slice 6, layout script on the planner's tree). The grid kept main's 12 px margin with Add (28 px tall)
13 px over the bottom: the grid from the padding to the right edge, down to 8 px over Add, which stands 8 px over the bottom centred under
it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    a = h.n("butAdd")
    ya = B - a.nh
    h.set("butAdd", 8 + (R - 8 - 75) // 2, ya)
    h.set("gridMain", 8, 8, R - 8, ya - 8 - 8)
