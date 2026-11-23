"""FormShutdown on the grid (UI-12 slice 6, layout script on the planner's tree). The caption and the grid kept main's 11 px margin, the two
buttons 110 and 107 px wide ending 1 and 4 px short of the right edge: the caption and the grid from the padding, the buttons one width on
the right edge, the grid 8 px before them and down to the bottom padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    bx = R - 110
    h.set("butCloseSessions", bx, None, 110, None)
    h.set("butShutdown", bx, B - 24, 110, None)
    h.set("butCloseSessions", None, B - 24 - 4 - 24)
    h.set("label1", 8, None)
    g = h.n("gridActiveInstances")
    h.set("gridActiveInstances", 8, None, bx - 8 - 8, B - g.ny)
