"""FormTaskInboxSetup on the grid (UI-12 slice 6, layout script on the planner's tree). The grid was drawn over the lower lines of the
instructions (main too: the planner kept it as drawn), the list and Set 34 px under the grid's top: the instructions from the padding to the
right edge as tall as their lines; under them the grid from the padding, Set and the inbox list on one top at the right edge (the list 120
px, Set 8 px before it), the grid 8 px before Set; the grid and the list down to 8 px over Save."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    lh = h.note_h("label1", R - 8)
    h.set("label1", 8, 8, R - 8, lh)
    top = 8 + lh + 8
    bot = h.n("butSave").ny - 8
    xl = R - 120
    xs = xl - 8 - 75
    h.set("listMain", xl, top, 120, bot - top)
    h.set("butSet", xs, top)
    h.set("gridMain", 8, top, xs - 8 - 8, bot - top)
