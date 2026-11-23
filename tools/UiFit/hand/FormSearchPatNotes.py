"""FormSearchPatNotes on the grid (UI-12 slice 6, layout script on the planner's tree). The box ended 6 px short of OK: on its right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    t = h.n("textPharse")
    h.set("textPharse", None, None, R - t.nx, None)
