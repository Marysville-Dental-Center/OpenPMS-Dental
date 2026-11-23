"""FormProgramLinks on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 15 px margin, 8 px short of the
right edge and 21 px over the bottom: the caption and the grid from the padding to the right edge, Add 8 px over the bottom with its note 6
px after it, the grid down to 8 px over Add."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    h.set("label2", 8, None)
    ya = B - 26
    h.set("butAdd", 8, ya)
    h.set("label1", 8 + 75 + 6, ya + (26 - 29) // 2)
    g = h.n("gridProgram")
    h.set("gridProgram", 8, None, R - 8, ya - 8 - g.ny)
