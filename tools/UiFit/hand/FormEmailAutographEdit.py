"""FormEmailAutographEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The fields ended 8 px short of Save: on its right
edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    for name in ("textDescription", "textEmailAddress", "textAutographText"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
