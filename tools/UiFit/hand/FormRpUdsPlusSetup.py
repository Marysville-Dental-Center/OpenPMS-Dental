"""FormRpUdsPlusSetup on the grid (UI-12 slice 6, layout script on the planner's tree). The instructions and the grid kept main's 12 px margin
with the grid 24 px over the buttons: from the padding to the right edge, the grid down to 8 px over the bottom row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    h.set("labelInstructions", 8, None, R - 8, None)
    g = h.n("gridMain")
    h.set("gridMain", 8, None, R - 8, h.n("butAdd").ny - 8 - g.ny)
