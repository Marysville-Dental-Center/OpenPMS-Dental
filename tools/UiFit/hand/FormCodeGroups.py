"""FormCodeGroups on the grid (UI-12 slice 6, layout script on the planner's tree). The grid kept main's 12 px margin and its buttons ended 13
px short of Save: the grid at the padding, Add, Up and Down on Save's right edge, the grid 8 px before them, Show Hidden ending on that
edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    x = R - 75
    for name in ("butAdd", "butUp", "butDown"):
        h.set(name, x, None)
    h.set("gridMain", 8, None, x - 8 - 8, None)
    c = h.n("checkShowHidden")
    h.set("checkShowHidden", R - c.nw, None)
