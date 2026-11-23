"""FormPayorTypes on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 21 px margin with Add ending 7 px
short of the right edge: the note and the grid from the padding, Add on the right edge, the grid 8 px before it and down to the bottom
padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.set("label7", 8, None)
    h.set("butAdd", R - 75, None)
    g = h.n("gridMain")
    h.set("gridMain", 8, None, R - 75 - 8 - 8, e.bottom_edge(h) - g.ny)
