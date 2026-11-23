"""FormHL7Defs on the grid (UI-12 slice 6, layout script on the planner's tree). The grids stood 4 px apart at the top (36 and 40) from main's
12 px margin, History at 38, Copy and Duplicate 4 px apart: History at the padding, the grids one width and one top from the padding to the
right edge 8 px apart, down to 8 px over the bottom row, Copy and Duplicate on one row 8 px over the bottom, each centred under its grid."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    h.set("butHistory", 8, None)
    top = 36
    gw = (R - 8 - 8) // 2
    yb = B - 24
    h.set("grid1", 8, top, gw, yb - 8 - top)
    h.set("grid2", 8 + gw + 8, top, R - (8 + gw + 8), yb - 8 - top)
    h.set("butCopy", 8 + (gw - 75) // 2, yb)
    h.set("butDuplicate", 8 + gw + 8 + (R - (8 + gw + 8) - 89) // 2, yb)
