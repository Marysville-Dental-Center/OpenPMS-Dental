"""FormClinics on the grid (UI-12 slice 6, layout script on the planner's tree). The grid and the note kept main's 12 px left margin and Add
ended 4 px short of the group boxes and Save: the left column at the padding, the grid 8 px before the group boxes, Add on their right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    e.frame(h, bottom=False)
    g = h.n("gridMain")
    h.set("gridMain", None, None, h.n("groupMovePats").nx - 8 - g.nx, None)
